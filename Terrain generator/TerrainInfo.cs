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

        private Random r;

        // this actually shouldn't be writing to a Graphics, should it?
        public void Generate(Graphics g)
        {
            r = Seed == -1 ? new Random() : new Random(Seed);

            Brush ground = new SolidBrush(Color.Black);
            Brush sky = new SolidBrush(Color.White);

            g.FillRectangle(sky, 0, 0, Width, Height);

            double[] noise = PerlinNoise(Width, new int[] { 256, 128, 64, 32 }, new double[] { 350, 200, 100, 30 });
            for ( int i=0; i<Width; i++ )
            {
                g.FillRectangle(ground, i, (float)(Height - noise[i]), 1, (float)noise[i]);
            }
        }
        /*
        private double[] PerlinNoise(int range, double persistance, int octaves)
        {
            double[] output = new double[range];
            for (int o = 0; o < octaves; o++)
            {
                double[] noise = GenerateNoise(range, (int)Math.Pow(2, o));
                double amplitude = Math.Pow(persistance, o);
                for (int i = 0; i < range; i++)
                    output[i] += noise[i] * amplitude;
            }
            return output;
        }
        */
        private double[] PerlinNoise(int range, int[] spacings, double[] amplitudes)
        {
            if (spacings.Length != amplitudes.Length)
                throw new Exception("Parameter length error");

            double[] output = new double[range];
            for (int o = 0; o < spacings.Length; o++)
            {
                double[] noise = GenerateNoise(range, spacings[o]);
                for (int i = 0; i < range; i++)
                    output[i] += noise[i] * amplitudes[o];
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
