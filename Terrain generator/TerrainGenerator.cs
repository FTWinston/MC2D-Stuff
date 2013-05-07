//#define DEBUG_WORLDGEN_1
//#define DEBUG_WORLDGEN_2
//#define DEBUG_WORLDGEN_3
#define DEBUG_WORLDGEN_4
//#define DEBUG_WORLDGEN_5

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

using CaveGraph = Terrain_generator.Graph<Terrain_generator.TerrainGenerator.CaveNode, Terrain_generator.TerrainGenerator.CaveLink>;

namespace Terrain_generator
{
    public class TerrainGenerator
    {
        internal class CaveNode : CaveGraph.Node
        {
            public CaveNode(int x, int y, bool isSurface)
            {
                X = x; Y = y;
                IsSurface = isSurface;
            }
            public int X, Y;
            public bool IsSurface;
        }

        internal class CaveLink : CaveGraph.Link
        {
            public CaveLink(CaveNode node, bool rightward) :
                base(node)
            { Rightward = rightward; }
            public bool Rightward;
        }

        public int Width, Height, Seed = -1;
        public int GroundVerticalExtent = -1, GroundBumpiness = -1; // ground level is in pixels from the bottom, bumpiness & amplitude range from 0-1000
        public int CaveComplexity = 5; // 1 - 8, with 1 meaning "none"

        private Random r;

        private const double minGroundHeight = 16, caveMinWidth = 128, caveMaxWidth = 512, caveMinHeight = 64, caveMaxHeight = 320;
        private const double GroundLevelHeightFraction = 0.390625;
        private const int caveDeformSteps = 256, tunnelHeight = 20, maxTunnelLength = 400;
        private static readonly Brush ground = new SolidBrush(Color.Black), sky = new SolidBrush(Color.White);
        private static readonly Pen tunnel = new Pen(Color.White, tunnelHeight);

        public Bitmap Generate()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(bmp);
            
            r = Seed == -1 ? new Random() : new Random(Seed);

            g.FillRectangle(sky, 0, 0, Width, Height);

            // first, sort out the ground level
            double[] groundLevel = DetermineGroundLevel(r);
            for (int i = 0; i < Width; i++)
                g.FillRectangle(ground, i, 0, 1, (float)groundLevel[i]);

            GenerateCaveSystem(g, r, groundLevel);
            /*
            // now make some tunnels underground from the surface, and generate some caves around them
            int retriesLeft = 10;
            for (int cave = 0; cave < CaveQuantity; cave++)
            {
                Point[] points = FitCaveSystem(r, groundLevel);
                if (points != null)
                    RenderCaves(g, r, points, groundLevel);
                else if (retriesLeft > 0)
                {// try again, but don't infinite loop
                    retriesLeft--;
                    cave--;
                }
            }
            */


            // floating islands above ground

            g.Dispose();
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }

        private double[] DetermineGroundLevel(Random r)
        {
            double verticalExtent = (GroundVerticalExtent < 0 ? r.Next(1001) : GroundVerticalExtent) / 2000.0 * Height; // maximum value should be half the image height
            double bumpiness = (GroundBumpiness < 0 ? r.Next(1001) : GroundBumpiness) / 1666.6666667; // maximum value should be 0.6

            double[] groundLevel = PerlinNoise(Width, 1.0, bumpiness, new int[] { 256, 128, 64, 32 });

            double min, max;
            FindMinMax(groundLevel, out min, out max);

            // scale such that max - min = verticalRange,
            // and offset such that such that (min + max)/2 = ground level ... min should be >= minGroundHeight
            double scale = verticalExtent / (max - min);
            double offset = GroundLevelHeightFraction * Height - (max + min) * scale / 2.0;
            min = min * scale + offset;
            if (min < minGroundHeight)
                scale += minGroundHeight - min;

            for (int i = 0; i < groundLevel.Length; i++)
                groundLevel[i] = groundLevel[i] * scale + offset;
            return groundLevel;
        }

        private void GenerateCaveSystem(Graphics g, Random r, double[] groundLevel)
        {
            int caveComplexity = CaveComplexity < 1 ? r.Next(1, 9) : CaveComplexity;

            if (caveComplexity < 2)
                return;

            // pick some random points underground, based on the desired cave complexity, spread out from each other
            List<Point> nodes = PickCaveNetworkNodes(r, groundLevel, caveComplexity, g);

            // create a relative neighborhood graph for these points
            CaveGraph caveGraph = GenerateCaveGraph(nodes);

#if DEBUG_WORLDGEN_3
            RenderGraph(g, caveGraph);
#endif

            // connect to the surface
            AddCaveSurfaceAccess(r, caveGraph, groundLevel, caveComplexity);

#if DEBUG_WORLDGEN_4
            RenderGraph(g, caveGraph);
#endif

            // see which nodes can only reach the surface via "too steep" connections
            // for each,
            //  try removing the "too steep" connection,
            //  replace it with a shallow connection to another node instead
            //  if that doens't work, remove the "unescapable" nodes
            FixupInescapableNodes(caveGraph);

#if DEBUG_WORLDGEN_5
            RenderGraph(g, caveGraph);
#endif

/*
apply perlin noise to the "line" of each connection, with the highest magnitude at the middle, and very low at the ends

render each tunnel, by calculating a ceiling and a floor by adding perlin noise onto the noisy line.
	the ceiling noise have a much higher magnitude than the floor noise

for each (non-surface) connection, try rendering some larger caves along it,
	with a maximum size such that they don't overlap any other tunnels (or their caves)
*/
        }

        private void RenderGraph(Graphics g, CaveGraph caveGraph)
        {
            Pen debug1 = new Pen(Color.FromArgb(128, Color.Red), tunnelHeight);
            Pen debug2 = new Pen(Color.FromArgb(128, Color.Green), 3);

            // draw the graph, for debugging
            int num = 0;
            foreach (var n in caveGraph.Nodes)
            {
                num++;
                g.DrawEllipse(debug1, n.X - tunnelHeight / 2, n.Y - tunnelHeight / 2, tunnelHeight, tunnelHeight);
                g.DrawString(num.ToString(), new Font(FontFamily.GenericMonospace, 12), sky, n.X, n.Y);

                foreach (var l in n.LinkedNodes)
                    DrawLineWrap(g, debug2, l.Rightward, n.X, n.Y, l.Node.X, l.Node.Y);
            }

            for (int i = 0; i < caveGraph.Nodes.Count; i++)
                Console.WriteLine(string.Format("node {0} is at {1},{2}", i + 1, caveGraph.Nodes[i].X, caveGraph.Nodes[i].Y));
        }

        // draw a line, accounting for wrapping
        private void DrawLineWrap(Graphics g, Pen p, bool rightward, int x1, int y1, int x2, int y2)
        {
            int wrap;
            if (rightward)
                wrap = x1 > x2 ? 1 : 0;
            else
                wrap = x1 < x2 ? 2 : 0;

            switch (wrap)
            {
                case 0:
                    g.DrawLine(p, x1, y1, x2, y2);
                    break;
                case 1:
                    g.DrawLine(p, x1, y1, x2 + Width, y2);
                    g.DrawLine(p, x1 - Width, y1, x2, y2);
                    break;
                case 2:
                    g.DrawLine(p, x1, y1, x2 - Width, y2);
                    g.DrawLine(p, x1 + Width, y1, x2, y2);
                    break;
            }
        }

        private CaveGraph GenerateCaveGraph(List<Point> nodes)
        {
            var caveGraph = new CaveGraph();
            foreach (Point p in nodes)
                caveGraph.Nodes.Add(new CaveNode(p.X, p.Y, false));

            for (int i = 0; i < caveGraph.Nodes.Count; i++)
            {
                CaveNode a = caveGraph.Nodes[i];
                for (int j = i + 1; j < caveGraph.Nodes.Count; j++)
                {
                    CaveNode b = caveGraph.Nodes[j];

                    // if wrapping on the x-axis gives a smaller circle, use that instead
                    int ax = a.X, bx = b.X;
                    if (ax > bx)
                    {
                        if (ax - bx > Width / 2)
                            bx += Width;
                    }
                    else if (bx - ax > Width / 2)
                        ax += Width;
                    int centerX = (ax + bx) / 2, centerY = (a.Y + b.Y) / 2;
                    int radiusSq = DistanceSq(ax, a.Y, centerX, centerY);

                    bool valid = true;
                    for (int k = 0; k < caveGraph.Nodes.Count; k++)
                    {
                        if (k == i || k == j)
                            continue;

                        CaveNode c = caveGraph.Nodes[k];

                        /*int radius = (int)Math.Sqrt(radiusSq);
                        g.DrawEllipse(new Pen(Color.Orange), centerX - radius, centerY - radius, radius * 2, radius * 2);
                        g.DrawEllipse(new Pen(Color.Orange), centerX - radius - Width, centerY - radius, radius * 2, radius * 2);*/
                        if (InsideCircle(c.X, c.Y, centerX, centerY, radiusSq))
                        {
                            valid = false;
                            break;
                        }
                    }

                    bool abRightward;
                    if (a.X > b.X)
                        abRightward = a.X - b.X > Width / 2;
                    else
                        abRightward = b.X - a.X < Width / 2;

                    if (valid)
                    {
                        a.LinkedNodes.Add(new CaveLink(b, abRightward));
                        b.LinkedNodes.Add(new CaveLink(a, !abRightward));
                    }
                }
            }
            return caveGraph;
        }

        private bool InsideCircle(int px, int py, int centerX, int centerY, int radiusSq)
        {
            if (centerX - px > Width / 2)
                px += Width;

            return DistanceSq(px, py, centerX, centerY) < radiusSq;
        }

        private const int minCaveNodeHeight = (int)minGroundHeight + tunnelHeight;
        private const int minCaveNodeDepth = minCaveNodeHeight + 15;
        private List<Point> PickCaveNetworkNodes(Random r, double[] groundLevel, int caveComplexity, Graphics g)
        {
            List<Point> nodes = new List<Point>();
            int retries = 10;
            for (int i = 0; i < caveComplexity; i++)
            {
                int x = r.Next(Width);
                int yMin = minCaveNodeHeight;
                int yMax = (int)groundLevel[x] - 50;
                if (yMax < yMin)
                {// a point can't fit at this x coordinate. Try again, but don't retry indefinitely.
                    if (retries > 0)
                    {
                        retries--;
                        i--;
                    }
                    continue;
                }

                int y = yMin + r.Next(yMax - yMin);

#if DEBUG_WORLDGEN_1 || DEBUG_WORLDGEN_2
                Pen debug1 = new Pen(Color.FromArgb(128, Color.Red), tunnelHeight);
                g.DrawEllipse(debug1, x - tunnelHeight / 2, y - tunnelHeight / 2, tunnelHeight, tunnelHeight);
#endif
#if DEBUG_WORLDGEN_1
                g.DrawString((i+1).ToString(), new Font(FontFamily.GenericMonospace, 12), sky, x, y);
#endif

                nodes.Add(FindSpaceForNode(x, y, nodes, groundLevel, g));
            }

            return nodes;
        }

        private Point FindSpaceForNode(int x, int y, List<Point> nodes, double[] groundLevel, Graphics g)
        {
            // apply an inverse-cubed distance based force between each pair of nodes (up to a fixed max)
            // and an inverse-squared distance based force from the surface and bottom
            const double forceStrength = 400000, surfaceBottomForceStrength = 250000;

            for (int step = 0; step < 5; step++)
            {
#if DEBUG_WORLDGEN_2
                int prevX = x; int prevY = y;
#endif
                double fx = 0, fy = 0;

                // surface and bottom should repel nodes
                double depth = groundLevel[x] - y, doubleY = y;
                fy -= surfaceBottomForceStrength / depth / depth / depth;
                fy += surfaceBottomForceStrength / doubleY / doubleY / doubleY;

                foreach (Point other in nodes)
                {
                    // the force should wrap about the x-axis
                    int otherX = other.X;
                    if (other.X > x)
                    {
                        if (other.X - x > Width / 2)
                            otherX -= Width;
                    }
                    else if (x - other.X > Width / 2)
                        otherX += Width;

                    Size separation = new Size(otherX - x, other.Y - y);
                    double distSq = separation.Width * separation.Width + separation.Height * separation.Height;
                    double dist = Math.Sqrt(distSq);

                    fx -= separation.Width * forceStrength / (distSq * dist);
                    fy -= separation.Height * forceStrength / (distSq * dist);
                }

                x += (int)fx; y += (int)fy;
                
                while (x >= Width)
                    x -= -Width;
                while (x < 0)
                    x += Width;

                if (y < minCaveNodeHeight)
                    y = minCaveNodeHeight;
                else if (y > groundLevel[x] - minCaveNodeDepth)
                    y = (int)groundLevel[x] - minCaveNodeDepth;


#if DEBUG_WORLDGEN_2
                // draw circle
                Pen debug2 = new Pen(Color.FromArgb(128, Color.Green), tunnelHeight);
                g.DrawEllipse(debug2, x - tunnelHeight / 2, y - tunnelHeight / 2, tunnelHeight, tunnelHeight);

                // line connecting it to the previous
                DrawLineWrap(g, debug2, fx > 0, prevX, prevY, x, y);
#endif
            }

#if DEBUG_WORLDGEN_2
            // draw label at the end point
            g.DrawString((nodes.Count+1).ToString(), new Font(FontFamily.GenericMonospace, 12), sky, x, y);
#endif

            return new Point(x, y);
        }

        private class CaveExitInfo : IComparable<CaveExitInfo>
        {
            public Point Exit;
            public CaveNode Node;
            public int cost;
            public bool rightward;

            public int CompareTo(CaveExitInfo other)
            {
                return cost.CompareTo(other.cost);
            }
        }

        private void AddCaveSurfaceAccess(Random r, CaveGraph caveGraph, double[] groundLevel, int caveComplexity)
        {
            int numExits = caveComplexity > 5 ? 3 : 2;
            List<CaveExitInfo> possibleExits = new List<CaveExitInfo>();
            
            foreach (CaveNode node in caveGraph.Nodes)
            {
                Point exit = new Point(node.X, node.Y);
                bool wrapped = false;
                while (groundLevel[exit.X] > exit.Y)
                {
                    exit.X -= 2; exit.Y++;
                    if (exit.X < 0)
                    {
                        exit.X += Width;
                        wrapped = true;
                    }
                }
                
                TryAddCaveExit(caveGraph, possibleExits, node, exit, false, wrapped);

                exit = new Point(node.X, node.Y);
                wrapped = false;
                while (groundLevel[exit.X] > exit.Y)
                {
                    exit.X += 2; exit.Y++;
                    if (exit.X >= Width)
                    {
                        exit.X -= Width;
                        wrapped = true;
                    }
                }

                TryAddCaveExit(caveGraph, possibleExits, node, exit, true, wrapped);
            }

            possibleExits.Sort();
            for ( int i=0; i<possibleExits.Count && i < numExits; i++ )
            {
                CaveExitInfo exit = possibleExits[i];

                // don't use any exit that crosses the path of another exit. That would create a single entry chokepoint.
                bool disallowed = false;
                for (int j = 0; j < i; j++)
                {
                    CaveExitInfo other = possibleExits[j];
                    if (LinesIntersect(exit.Node.X, exit.Node.Y, exit.Exit.X, exit.Exit.Y, exit.rightward,
                                        other.Node.X, other.Node.Y, other.Exit.X, other.Exit.Y, other.rightward))
                    {
                        disallowed = true;
                        break;
                    }
                }
                if (disallowed)
                    continue;

                var exitNode = new CaveNode(exit.Exit.X, exit.Exit.Y, true);
                caveGraph.Nodes.Add(exitNode);
                exitNode.LinkedNodes.Add(new CaveLink(exit.Node, !exit.rightward));
                exit.Node.LinkedNodes.Add(new CaveLink(exitNode, exit.rightward));
            }
        }

        private void TryAddCaveExit(CaveGraph caveGraph, List<CaveExitInfo> exits, CaveNode node, Point exit, bool rightward, bool wrapped)
        {
            // does this link cross any existing ones? only consider rightward nodes, cos we'd do them all twice otherwise
            bool valid = true;
            foreach (CaveNode other in caveGraph.Nodes)
            {
                if (other == node)
                    continue;

                foreach (CaveLink link in other.LinkedNodes)
                    if (link.Rightward && link.Node != node)
                    {
                        if (LinesIntersect(node.X, node.Y,
                            exit.X, exit.Y, rightward,
                            other.X, other.Y,
                            link.Node.X, link.Node.Y, link.Rightward))
                        {
                            valid = false;
                            break;
                        }
                    }

                if (!valid)
                    break;
            }

            if (!valid)
                return;

            // it's valid. Determine a "cost" for it
            int cost = DistanceSq(node.X, node.Y, exit.X, exit.Y);

            int widthSq = Width * Width;

            // if too close (angularly) to another link from this node, mark me down
            foreach ( CaveLink link in node.LinkedNodes )
            {
                double cosAngle = GetCosAngle(rightward && exit.X < node.X ? exit.X + Width : exit.X, exit.Y,
                    node.X, node.Y,
                    link.Rightward && link.Node.X < node.X ? link.Node.X + Width : link.Node.X, link.Node.Y);
                if (cosAngle < 0.866) // > 30 degrees
                    continue;

                cost += (int)((cosAngle - 0.866) * widthSq);
            }

            foreach (CaveExitInfo other in exits)
            {
                // multiple exits from the one node is bad, so if there's another, mark the worst one (this or that) down a lot
                if (other.Node == node)
                {
                    if (other.cost > cost)
                        other.cost += widthSq;
                    else
                        cost += widthSq;
                    break;
                }

                // exits shouldn't be too close to each other on the surface
                // if there's another one nearby, mark down the one (this or that) with the worst score
                int distSq = DistanceSq(exit.X, exit.Y, other.Exit.X, other.Exit.Y);
                int extraCost = distSq == 0 ? int.MaxValue : widthSq / distSq;
                
                if (other.cost > cost)
                    other.cost += extraCost;
                else
                    cost += extraCost;
            }

            exits.Add(new CaveExitInfo()
            {
                Node = node,
                rightward = rightward,
                Exit = exit,
                cost = cost
            });
        }

        private void FixupInescapableNodes(CaveGraph caveGraph)
        {
            List<CaveNode> canExit = new List<CaveNode>(), cantExit = new List<CaveNode>();
            foreach (CaveNode node in caveGraph.Nodes)
                if (node.IsSurface)
                    canExit.Add(node);
                else
                    cantExit.Add(node);

            int prevNum, num = cantExit.Count;
            do
            {
                prevNum = num;

                for ( int i=0; i<cantExit.Count; i++ )
                {// if this node is connected to one in canExit, via a slope that isn't too steep
                    CaveNode node = cantExit[i];
                    foreach ( CaveLink link in node.LinkedNodes )
                        if (canExit.Contains(link.Node))
                        {
                            bool passable = link.Node.Y <= node.Y;

                            if (!passable)
                            {// its not downward, so ensure it isn't too steep
                                double dx = link.Node.X - node.X;
                                if (link.Rightward)
                                {
                                    if (link.Node.X < node.X)
                                        dx = link.Node.X + Width - node.X;
                                }
                                else if (link.Node.X > node.X)
                                    dx = node.X + Width - link.Node.X;

                                passable = Math.Abs((link.Node.Y - node.Y) / dx) < 1;
                            }

                            if (passable)
                            {
                                canExit.Add(node);
                                cantExit.RemoveAt(i);
                                i--;
                                break;
                            }
                        }
                }

                num = cantExit.Count;
            } while (num != 0 && num != prevNum); // continue as long as there's change

            if (cantExit.Count == 0)
                return;

            ;

            // just remove all those that are inescapable - including all links to them
            foreach (CaveNode node in cantExit)
            {
                foreach ( CaveLink link in node.LinkedNodes )
                    for ( int i=0; i<link.Node.LinkedNodes.Count; i++ )
                        if ( link.Node.LinkedNodes[i].Node == node )
                        {
                            link.Node.LinkedNodes.RemoveAt(i);
                            break;
                        }
                caveGraph.Nodes.Remove(node);
            }
        }
        
        private double GetCosAngle(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            double p12sq = DistanceSq(x1, y1, x2, y2);
            double p23sq = DistanceSq(x2, y2, x3, y3);
            double p13sq = DistanceSq(x1, y1, x3, y3);

            return (p12sq + p23sq - p13sq) / (2.0 * Math.Sqrt(p12sq * p23sq));
        }

        private bool LinesIntersect(int aX1, int aY1, int aX2, int aY2, bool aRightward, int bX1, int bY1, int bX2, int bY2, bool bRightward)
        {
            if (aRightward)
            {
                if (aX1 > aX2)
                    aX2 += Width;
            }
            else if (aX2 > aX1)
                aX2 -= Width;

            if (bRightward)
            {
                if (bX1 > bX2)
                    bX2 += Width;
            }
            else if (bX2 > bX1)
                bX2 -= Width;

            int A1 = aY2 - aY1;
            int B1 = aX1 - aX2;
            int C1 = A1 * aX1 + B1 * aY1;

            int A2 = bY2 - bY1;
            int B2 = bX1 - bX2;
            int C2 = A2 * bX1 + B2 * bY1;

            double determinant = A1 * B2 - A2 * B1;
            if (determinant == 0)
                return false;
            
            double x = (B2 * C1 - B1 * C2) / determinant;
            double y = (A1 * C2 - A2 * C1) / determinant;

            // check that these x & y values fall within the range of BOTH lines

            if (aRightward)
            {
                if (x > aX2 || x < aX1)
                    return false;
            }
            else if (x > aX1 || x < aX2)
                return false;

            if (bRightward)
            {
                if (x > bX2 || x < bX1)
                    return false;
            }
            else if (x > bX1 || x < bX2)
                return false;

            if (aY2 > aY1)
            {
                if (y > aY2 || y < aY1)
                    return false;
            }
            else if (y > aY1 || y < aY2)
                return false;

            if (bY2 > bY1)
            {
                if (y > bY2 || y < bY1)
                    return false;
            }
            else if (y > bY1 || y < bY2)
                return false;

            return true;
        }

        private GraphicsPath DeformCave(Rectangle bounds)
        {
            GraphicsPath path = new GraphicsPath(FillMode.Alternate);

            double[] deformation = PerlinNoise(caveDeformSteps, 0.4, 0.5, new int[] { 64, 32, 16, 8 });
            Point center = GetRectangleCenter(bounds);

            // for each step, circle around the center, going out to the distance we should for an ellipse,
            // then moving inward by the deformation amount. Join this point to the previous one.

            double stepAngle = Math.PI * 2.0 / caveDeformSteps;
            double halfWidth = bounds.Width / 2.0, halfHeight = bounds.Height / 2.0;

            double px = halfWidth * (1 - deformation[0]) + center.X;
            double py = center.Y;

            for (int i = 1; i < caveDeformSteps; i++)
            {
                double angle = stepAngle * i;
                double x = Math.Cos(angle) * halfWidth * (1 - deformation[i]) + center.X;
                double y = Math.Sin(angle) * halfHeight * (1 - deformation[i]) + center.Y;

                path.AddLine((float)px, (float)py, (float)x, (float)y);

                px = x; py = y;
            }

            path.CloseFigure();
            return path;
        }

        // this accounts for horizontal wrapping
        private int DistanceSq(int x1, int y1, int x2, int y2)
        {
            if (x1 > x2)
            {
                if (x1 - x2 > Width / 2)
                    x2 += Width;
            }
            else if (x2 - x1 > Width / 2)
                x1 += Width;
 
            return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
        }

        private double Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(DistanceSq(x1, y1, x2, y2));
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(DistanceSq(p1.X, p1.Y, p2.X, p2.Y));
        }

        private static Point GetRectangleCenter(Rectangle r)
        {
            return new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
        }

        private double[] PerlinNoise(int range, double amplitude, double persistance, int[] spacing)
        {
            double[] output = new double[range];
            for (int o = 0; o < spacing.Length; o++)
            {
                double[] noise = GenerateNoise(range, spacing[o]);
                for (int i = 0; i < range; i++)
                    output[i] += noise[i] * amplitude;
                amplitude *= persistance;
            }
            return output;
        }

        private double[] GenerateNoise(int range, int smoothness)
        {
            if ( range % smoothness != 0 )
                throw new Exception("Range must divide by smoothness!");

            double[] output = new double[range];

            // generate fixed points
            for (int i = 0; i < range; i+=smoothness)
                output[i] = r.NextDouble();

            int a = range - smoothness, b = 0, c = smoothness, d = smoothness * 2;
            
            if (c >= range)
            {
                c -= range;
                d -= range;
            }
            else if (d >= range)
            {
                d -= range;
            }

            // interpolate the remaining points
            for (int i = 0; i < range; i++)
            {
                if (i % smoothness == 0)
                    continue;
                if (i > b + smoothness)
                {
                    a = b; b = c; c = d;
                    d = c + smoothness;

                    if (c >= range)
                    {
                        c -= range;
                        d -= range;
                    }
                    else if (d >= range)
                    {
                        d -= range;
                    }
                }

                double P = output[d] - output[c] - output[a] + output[b];
                double Q = output[a] - output[b] - P;
                double R = output[c] - output[a];
                double S = output[b];

                double mu = (double)(i-b)/smoothness;

                output[i] = P * mu * mu * mu + Q * mu * mu + R * mu + S;
            }

            return output;
        }

        private void FindMinMax(double[] data, out double min, out double max)
        {
            min = double.MaxValue;
            max = double.MinValue;
            double val;
            for (int i = 0; i < data.Length; i++)
            {
                val = data[i];
                if (val < min)
                    min = val;
                if (val > max)
                    max = val;
            }
        }

        private Point FindClosestPoint(Point p, List<Point> nodes, out int distSq)
        {
            Point bestPoint = nodes[0];
            distSq = (bestPoint.X - p.X) * (bestPoint.X - p.X) + (bestPoint.Y - p.Y) * (bestPoint.Y - p.Y);

            for (int i = 1; i < nodes.Count; i++)
            {
                Point test = nodes[i];
                int testDist = (test.X - p.X) * (test.X - p.X) + (test.Y - p.Y) * (test.Y - p.Y);

                if (testDist < distSq)
                {
                    bestPoint = test;
                    distSq = testDist;
                }
            }

            return bestPoint;
        }

        private bool RectanglesIntersect(Rectangle r1, Rectangle r2, int xMax)
        {
            if (r1.IntersectsWith(r2))
                return true;
            
            // if BOTH stick out beyond xMax, don't need to do anything fancy. It's only if one does and the other doesn't.
            if (r1.Right > xMax)
            {
                if ( r2.Right > xMax )
                    return false;

                r1.X -= xMax;
                bool retVal = r1.IntersectsWith(r2);
                r1.X += xMax;
                return retVal;
            }
            else if (r2.Right > xMax)
            {
                r2.X -= xMax;
                bool retVal = r1.IntersectsWith(r2);
                r2.X += xMax;
                return retVal;
            }

            return false;
        }

        private int Wrap(int x)
        {
            if (x < 0)
                return x + Width;
            else if (x >= Width)
                return x - Width;
            else
                return x;
        }

        private void DrawPath(Graphics g, Pen p, GraphicsPath path, bool mustDrawTwice)
        {
            g.DrawPath(p, path);

            if (mustDrawTwice)
            {
                g.TranslateTransform(-Width, 0);
                g.DrawPath(p, path);
                g.TranslateTransform(Width, 0);
            }
        }

        private void FillPath(Graphics g, Brush b, GraphicsPath path, bool mustDrawTwice)
        {
            g.FillPath(b, path);

            if (mustDrawTwice)
            {
                g.TranslateTransform(-Width, 0);
                g.FillPath(b, path);
                g.TranslateTransform(Width, 0);
            }
        }

        private const char separator = ';';
        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Seed); sb.Append(separator);
            sb.Append(Width); sb.Append(separator);
            sb.Append(Height);

            if (GroundVerticalExtent >= 0 || GroundBumpiness >= 0 && CaveComplexity >= 0)
            {
                sb.Append(separator);

                if (GroundVerticalExtent >= 0)
                    sb.Append(GroundVerticalExtent);
                sb.Append(separator);

                if (GroundBumpiness >= 0)
                    sb.Append(GroundBumpiness);
                sb.Append(separator);

                if (CaveComplexity >= 0)
                    sb.Append(CaveComplexity);
            }
            return sb.ToString();
        }

        public static TerrainGenerator Deserialize(string serialized)
        {
            string[] parts = serialized.Split(separator);
            if (parts.Length != 3 && parts.Length != 6)
                throw new Exception("Incorrect number of parts");

            int width, height, seed, groundVerticalExtent, groundBumpiness, caveComplexity;
            if (!int.TryParse(parts[0], out seed))
                throw new Exception("Invalid seed");
            if (!int.TryParse(parts[1], out width))
                throw new Exception("Invalid width");
            if (!int.TryParse(parts[2], out height))
                throw new Exception("Invalid height");

            if (parts.Length < 4 || parts[3].Length == 0)
                groundVerticalExtent = -1;
            else if (!int.TryParse(parts[3], out groundVerticalExtent))
                throw new Exception("Invalid ground vertical extent");

            if (parts.Length < 5 || parts[4].Length == 0)
                groundBumpiness = -1;
            else if (!int.TryParse(parts[4], out groundBumpiness))
                throw new Exception("Invalid ground bumpiness");

            if (parts.Length < 6 || parts[5].Length == 0)
                caveComplexity = -1;
            else if (!int.TryParse(parts[5], out caveComplexity))
                throw new Exception("Invalid cave complexity");

            return new TerrainGenerator()
            {
                Width = width,
                Height = height,
                Seed = seed,
                GroundVerticalExtent = groundVerticalExtent,
                GroundBumpiness = groundBumpiness,
                CaveComplexity = caveComplexity
            };
        }
    }
}
