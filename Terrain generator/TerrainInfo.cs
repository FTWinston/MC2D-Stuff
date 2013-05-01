using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Terrain_generator
{
    public class TerrainInfo
    {
        public int Width, Height, Seed = -1;
        public double Bumpiness = 0.5; // from 0 to ~ 0.6
        public double Amplitude = 350; // from 0 to ~ 1/2 the height

        private Random r;

        // this actually shouldn't be writing to a Graphics, should it?
        public Bitmap Generate()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(bmp);
            
            r = Seed == -1 ? new Random() : new Random(Seed);

            Brush ground = new SolidBrush(Color.Black);
            Brush sky = new SolidBrush(Color.White);

            g.FillRectangle(sky, 0, 0, Width, Height);

            double[] groundLevel = PerlinNoise(Width, Amplitude, Bumpiness, new int[] { 256, 128, 64, 32 });
            for (int i = 0; i < Width; i++)
            {
                g.FillRectangle(ground, i, (float)(Height - groundLevel[i]), 1, (float)groundLevel[i]);
            }

            g.Dispose();
            return bmp;
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
    }
}
