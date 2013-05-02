﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Terrain_generator
{
    public class TerrainInfo
    {
        public int Width, Height, Seed = -1;
        public int GroundLevel = 300, GroundVerticalExtent = 500, GroundBumpiness = 500; // ground level is in pixels from the bottom, bumpiness & amplitude range from 0-1000
        public int CaveQuantity = 5; // 0 - 10

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

        private static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
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
            sb.Append(CaveQuantity);

            return sb.ToString();
        }

        public static TerrainInfo Deserialize(string serialized)
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

            return new TerrainInfo()
            {
                Width = width,
                Height = height,
                Seed = seed,
                GroundLevel = groundLevel,
                GroundVerticalExtent = groundVerticalExtent,
                GroundBumpiness = groundBumpiness,
                CaveQuantity = caveQuantity
            };
        }
    }
}
