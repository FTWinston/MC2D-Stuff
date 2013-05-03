using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Terrain_generator
{
    public class TerrainGenerator
    {
        public int Width, Height, Seed = -1;
        public int GroundLevel = 300, GroundVerticalExtent = 500, GroundBumpiness = 500; // ground level is in pixels from the bottom, bumpiness & amplitude range from 0-1000
        public int CaveComplexity = 5; // 1 - 8, with 1 meaning "none"

        private Random r;

        private const double minGroundHeight = 16, caveMinWidth = 128, caveMaxWidth = 512, caveMinHeight = 64, caveMaxHeight = 320;
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
            double[] groundLevel = DetermineGroundLevel();
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

        private double[] DetermineGroundLevel()
        {
            double verticalExtent = GroundVerticalExtent / 2000.0 * Height; // maximum value should be half the image height
            double bumpiness = GroundBumpiness / 1666.6666667; // maximum value should be 0.6

            double[] groundLevel = PerlinNoise(Width, 1.0, bumpiness, new int[] { 256, 128, 64, 32 });

            double min, max;
            FindMinMax(groundLevel, out min, out max);

            // scale such that max - min = verticalRange,
            // and offset such that such that (min + max)/2 = ground level ... min should be >= minGroundHeight
            double scale = verticalExtent / (max - min);
            double offset = GroundLevel - (max + min) * scale / 2.0;
            min = min * scale + offset;
            if (min < minGroundHeight)
                scale += minGroundHeight - min;

            for (int i = 0; i < groundLevel.Length; i++)
                groundLevel[i] = groundLevel[i] * scale + offset;
            return groundLevel;
        }

        private void GenerateCaveSystem(Graphics g, Random r, double[] groundLevel)
        {
            if (CaveComplexity < 2)
                return;

            // pick some random points underground, based on the desired cave complexity
            List<Point> nodes = PickCaveNetworkNodes(r, groundLevel);
            for (int i = 0; i < 5; i++)
                SpreadOutCaveNetworkNodes(nodes, groundLevel);
            
            // create a relative neighborhood graph for these points
            Graph caveGraph = GenerateCaveGraph(nodes);

            Pen debug1 = new Pen(Color.FromArgb(128, Color.Red), tunnelHeight);
            Pen debug2 = new Pen(Color.FromArgb(128, Color.Green), 3);

            // draw the graph, for debugging
            foreach (Graph.Node n in caveGraph.Nodes)
            {
                g.DrawEllipse(debug1, n.X - tunnelHeight / 2, n.Y - tunnelHeight / 2, tunnelHeight, tunnelHeight);

                foreach ( Graph.Link l in n.LinkedNodes )
                {
                    int wrap;
                    if (l.Rightwards)
                        wrap = n.X > l.Node.X ? 1 : 0;
                    else
                        wrap = n.X < l.Node.X ? 2 : 0;

                    // draw line from n to n2, accounting for wrapping
                    // the wrapping doesn't work!
                    switch (wrap)
                    {
                        case 0:
                            g.DrawLine(debug2, n.X, n.Y, l.Node.X, l.Node.Y);
                            break;
                        case 1:
                            g.DrawLine(debug2, n.X, n.Y, l.Node.X + Width, l.Node.Y);
                            g.DrawLine(debug2, n.X - Width, n.Y, l.Node.X, l.Node.Y);
                            break;
                        case 2:
                            g.DrawLine(debug2, n.X, n.Y, l.Node.X - Width, l.Node.Y);
                            g.DrawLine(debug2, n.X + Width, n.Y, l.Node.X, l.Node.Y);
                            break;
                    }
                }
            }

/*
compute a delaunay triangulation for these, looped on the x-axis
	http://en.wikipedia.org/wiki/Delaunay_triangulation
	if any connection is too steep, discard it.
	if any connection goes above ground, discard it.
	if any node has only one connection, discard it.

remove connections, going down to either a relative neighborhood graph or a gabriel graph
	http://en.wikipedia.org/wiki/Relative_neighborhood_graph
	http://en.wikipedia.org/wiki/Gabriel_graph	

pick 2-4 points (based on cave complexity) on the surface to be entrances
	do so by moving upward (at an appropriate angle) from the "topmost" tunnel nodes
	add connections to these

apply perlin noise to the "line" of each connection, with the highest magnitude at the middle, and very low at the ends

render each tunnel, by calculating a ceiling and a floor by adding perlin noise onto the noisy line.
	the ceiling noise have a much higher magnitude than the floor noise

for each (non-surface) connection, try rendering some larger caves along it,
	with a maximum size such that they don't overlap any other tunnels (or their caves)
*/

        }

        private Graph GenerateCaveGraph(List<Point> nodes)
        {
            Graph caveGraph = new Graph();
            foreach (Point p in nodes)
                caveGraph.Nodes.Add(new Graph.Node(p.X, p.Y));

            for (int i = 0; i < caveGraph.Nodes.Count; i++)
            {
                Graph.Node a = caveGraph.Nodes[i];
                for (int j = i + 1; j < caveGraph.Nodes.Count; j++)
                {
                    Graph.Node b = caveGraph.Nodes[j];
                    double distSq = DistanceSq(a.X, a.Y, b.X, b.Y);

                    bool valid = true;
                    for (int k = j + 1; k < caveGraph.Nodes.Count; k++)
                    {
                        Graph.Node c = caveGraph.Nodes[k];

                        if (DistanceSq(a.X, a.Y, c.X, c.Y) < distSq && DistanceSq(b.X, b.Y, c.X, c.Y) < distSq)
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
                        a.LinkedNodes.Add(new Graph.Link(b, abRightward));
                        b.LinkedNodes.Add(new Graph.Link(a, !abRightward));
                    }
                }
            }
            return caveGraph;
        }

        private const int minCaveNodeHeight = (int)minGroundHeight + tunnelHeight;
        private const int minCaveNodeDepth = minCaveNodeHeight + 15;
        private List<Point> PickCaveNetworkNodes(Random r, double[] groundLevel)
        {
            List<Point> nodes = new List<Point>();
            int retries = 10;
            for (int i = 0; i < CaveComplexity; i++)
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

                Point p = new Point(x, y);

                // check this point isn't too close to the other points
                /*if ( i != 0 ) 
                {
                    int distSq;
                    Point closest = FindClosestPoint(p, nodes, out distSq);
                    
                    if (distSq < 5000)
                    {
                        if (retries > 0)
                        {
                            retries--;
                            i--;
                        }
                        continue;
                    }

                }*/
                nodes.Add(p);
            }

            return nodes;
        }

        private void SpreadOutCaveNetworkNodes(List<Point> nodes, double[] groundLevel)
        {
            // apply an inverse-square distance based force between each pair of nodes (up to a fixed max)
            const double forceStrength = 400000, surfaceBottomForceStrength = 250000;

            for ( int i=0; i<nodes.Count; i++ )
            {
                Point node = nodes[i];

                double forceX = 0, forceY = 0;

                // surface and bottom should repel nodes
                double depth = groundLevel[node.X] - node.Y;
                forceY -= surfaceBottomForceStrength / depth / depth / depth;
                forceY += surfaceBottomForceStrength / node.Y / node.Y / node.Y;

                // nodes should repel each other
                for ( int j=0; j<nodes.Count; j++ )
                {
                    if ( j == i )
                        continue;

                    // the force should wrap about the x-axis
                    Point other = nodes[j];
                    if (other.X > node.X)
                    {
                        if (other.X - node.X > Width / 2)
                            other.X -= Width;
                    }
                    else if (node.X - other.X > Width / 2)
                        other.X += Width;

                    Size separation = new Size(other.X - node.X, other.Y - node.Y);
                    double distSq = separation.Width * separation.Width + separation.Height * separation.Height;
                    double dist = Math.Sqrt(distSq);

                    forceX -= separation.Width * forceStrength / (distSq * dist);
                    forceY -= separation.Height * forceStrength / (distSq * dist);
                }

                node = new Point(node.X + (int)forceX, node.Y + (int)forceY);
                while (node.X >= Width)
                    node.X -= -Width;
                while (node.X < 0)
                    node.X += Width;

                if (node.Y < minCaveNodeHeight)
                    node.Y = minCaveNodeHeight;
                else if (node.Y > groundLevel[node.X] - minCaveNodeDepth)
                    node.Y = (int)groundLevel[node.X] - minCaveNodeDepth;

                nodes[i] = node;
            }
        }


        /*
        private const int minTunnelWidth = 200;

        private Point[] FitCaveSystem(Random r, double[] groundLevel)
        {
            int x1 = r.Next(groundLevel.Length);
            int x3 = x1 + r.Next(Width - minTunnelWidth * 2);
            int y1 = (int)groundLevel[x1] + tunnelHeight;
            int y3 = (int)groundLevel[Wrap(x3)] + tunnelHeight;
            
            // choose the "mid" (lowest) point, such that it's below ground, but no deeper than 2/3 the distance between x1 & x3,
            // from the lowest of 1 & 3.
            int x2 = x1 + r.Next((x3-x1) * 2 / 3) + (x3-x1) / 3;
            int lowestEnd = Math.Min(y1, y3);
            int y2max = Math.Min(lowestEnd - 50 - tunnelHeight, (int)groundLevel[Wrap(x2)] - 100);
            int y2min = Math.Max(lowestEnd - (x3 - x1) * 2 / 3, (int)minGroundHeight);

            // Also, it should be no more than ~35 degrees down from 1 or 3.
            y2min = Math.Max(y2min, y1 - (x2 - x1) * 2 / 3);
            y2min = Math.Max(y2min, y3 - (x3 - x2) * 2 / 3);

            if (y2min > y2max)
                return null;

            int y2 = y2min + (int)(r.NextDouble() * (y2max - y2min));

            return new Point[] { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3) };
        }

        private void RenderCaves(Graphics g, Random r, Point[] points, double[] groundLevel)
        {
            // draw the core tunnel
            GraphicsPath gp = new GraphicsPath();
            gp.AddCurve(points, 0.3f);
            DrawPath(g, tunnel, gp, points[points.Length - 1].X >= Width);

            // now add caves to that tunnel
            Point tunnelMid = points[1];

            Rectangle caveBounds = TryFitCave(tunnelMid, groundLevel);
            g.FillPath(sky, DeformCave(caveBounds));
        }


        private Rectangle TryFitCave(Point lowerMiddle, double[] groundLevel)
        {
            // decide on the size of the cave we want to place, that won't break the surface
            int caveWidth = (int)(caveMinWidth + r.NextDouble() * (caveMaxWidth - caveMinWidth));
            int caveX = lowerMiddle.X - caveWidth / 2;

            double maxHeight = caveMaxHeight;
            maxHeight = Math.Min(maxHeight, groundLevel[Wrap(lowerMiddle.X)] - lowerMiddle.Y - 5);
            maxHeight = Math.Min(maxHeight, groundLevel[Wrap(caveX)] - lowerMiddle.Y - 5);
            maxHeight = Math.Min(maxHeight, groundLevel[Wrap(caveX + caveWidth)] - lowerMiddle.Y - 5);
            int caveHeight = (int)(caveMinHeight + r.NextDouble() * (maxHeight - caveMinHeight));

            return new Rectangle(caveX, lowerMiddle.Y - caveHeight * 1 / 3, caveWidth, caveHeight);
        }
        */

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
        private double DistanceSq(int x1, int y1, int x2, int y2)
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
            sb.Append(Height); sb.Append(separator);
            sb.Append(GroundLevel); sb.Append(separator);
            sb.Append(GroundVerticalExtent); sb.Append(separator);
            sb.Append(GroundBumpiness); sb.Append(separator);
            sb.Append(CaveComplexity);

            return sb.ToString();
        }

        public static TerrainGenerator Deserialize(string serialized)
        {
            string[] parts = serialized.Split(separator);
            if (parts.Length != 7)
                throw new Exception("Incorrect number of parts");

            int width, height, seed, groundLevel, groundVerticalExtent, groundBumpiness, caveQuantity;
            if (!int.TryParse(parts[0], out seed))
                throw new Exception("Invalid seed");
            if (!int.TryParse(parts[1], out width))
                throw new Exception("Invalid width");
            if (!int.TryParse(parts[2], out height))
                throw new Exception("Invalid height");
            if (!int.TryParse(parts[3], out groundLevel))
                throw new Exception("Invalid ground level");
            if (!int.TryParse(parts[4], out groundVerticalExtent))
                throw new Exception("Invalid ground vertical extent");
            if (!int.TryParse(parts[5], out groundBumpiness))
                throw new Exception("Invalid ground bumpiness");
            if (!int.TryParse(parts[6], out caveQuantity))
                throw new Exception("Invalid cave quantity");

            return new TerrainGenerator()
            {
                Width = width,
                Height = height,
                Seed = seed,
                GroundLevel = groundLevel,
                GroundVerticalExtent = groundVerticalExtent,
                GroundBumpiness = groundBumpiness,
                CaveComplexity = caveQuantity
            };
        }
    }
}
