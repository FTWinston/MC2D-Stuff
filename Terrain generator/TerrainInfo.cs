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

        private const double minGroundHeight = 16, caveMinWidth = 48, caveMaxWidth = 400, caveMinHeight = 36, caveMaxHeight = 192;
        private const int caveDeformSteps = 256;

        public Bitmap Generate()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(bmp);
            
            r = Seed == -1 ? new Random() : new Random(Seed);

            Brush ground = new SolidBrush(Color.Black);
            Brush sky = new SolidBrush(Color.White);

            g.FillRectangle(sky, 0, 0, Width, Height);

            // first, sort out the ground level
            double[] groundLevel = DetermineGroundLevel();
            for (int i = 0; i < Width; i++)
                g.FillRectangle(ground, i, 0, 1, (float)groundLevel[i]);

            // fit caves underground
            List<Rectangle> caves = new List<Rectangle>();
            for (int cave = 0; cave < CaveQuantity; cave++) // try several times to place each before quitting
                for (int attempt = 0; attempt < 8; attempt++)
                {
                    Rectangle bounds = TryFitCave(caves, groundLevel);
                    if (bounds == Rectangle.Empty)
                        continue;
                    
                    caves.Add(bounds);

                    GraphicsPath path = DeformCave(bounds);

                    g.FillPath(sky, path);

                    if (bounds.Right > Width)
                    {
                        g.TranslateTransform(-Width, 0);
                        g.FillPath(sky, path);
                        g.TranslateTransform(Width, 0);
                    }
                    break;
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

        private Rectangle TryFitCave(List<Rectangle> otherCaves, double[] groundLevel)
        {
            // decide on the size of the cave we want to place
            int caveWidth = (int)(caveMinWidth + r.NextDouble() * (caveMaxWidth - caveMinWidth));
            int caveHeight = (int)(caveMinHeight + r.NextDouble() * (caveMaxHeight - caveMinHeight));

            // pick an X point for the cave
            int caveX = r.Next(Width);

            // find a Y point it could be placed at, without breaking the surface
            double yMin = minGroundHeight, yMax = Height;
            for (int i = 0; i < caveWidth; i++)
            {
                int x = caveX + i;
                if (x >= Width)
                    x -= Width;

                yMax = Math.Min(yMax, groundLevel[x] - 16);
            }
            if (yMax - yMin < caveMinHeight)
                return Rectangle.Empty;
            else if (yMax - yMin < caveHeight)
                caveHeight = (int)(yMax - yMin);

            int caveY = (int)yMin + r.Next((int)(yMax - yMin - caveHeight));

            Rectangle bounds = new Rectangle(caveX, caveY, caveWidth, caveHeight);

            // check that's free of other caves
            for (int i = 0; i < otherCaves.Count; i++)
                if (RectanglesIntersect(bounds, otherCaves[i], groundLevel.Length))
                    return Rectangle.Empty;

            return bounds;
        }

        private GraphicsPath DeformCave(Rectangle bounds)
        {
            GraphicsPath path = new GraphicsPath();

            double[] deformation = PerlinNoise(caveDeformSteps, 0.4, 0.5, new int[] { 64, 32, 16, 8 });
            Point center = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
            
            // for each step, circle around the center, going out to the distance we should for an ellipse,
            // then moving inward by the deformation amount. Join this point to the previous one.

            double stepAngle = Math.PI * 2.0 / caveDeformSteps;
            double halfWidth = bounds.Width / 2.0, halfHeight = bounds.Height / 2.0;

            double px = halfWidth * (1-deformation[0]) + center.X;
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
    }
}
