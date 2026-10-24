// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>
    /// Seeded 2D gradient (Perlin) noise. Thread safe after construction.
    /// Same seed => same map, so a generated world can be reproduced.
    /// </summary>
    internal sealed class GradientNoise
    {
        private readonly int[] _perm = new int[512];
        private readonly double _offsetX;
        private readonly double _offsetY;

        private static readonly double[] GradX = { 1, -1, 1, -1, 1, -1, 0, 0, 0.7071, -0.7071, 0.7071, -0.7071 };
        private static readonly double[] GradY = { 0, 0, 1, -1, -1, 1, 1, -1, 0.7071, 0.7071, -0.7071, -0.7071 };

        public GradientNoise(int seed)
        {
            Random random = new Random(seed);
            int[] p = new int[256];
            for (int i = 0; i < 256; i++)
            {
                p[i] = i;
            }

            for (int i = 255; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (p[i], p[j]) = (p[j], p[i]);
            }

            for (int i = 0; i < 512; i++)
            {
                _perm[i] = p[i & 255];
            }

            // random offset so that different seeds do not share the lattice origin
            _offsetX = random.NextDouble() * 4096.0;
            _offsetY = random.NextDouble() * 4096.0;
        }

        /// <summary>Noise in about [-1, 1].</summary>
        public double Sample(double x, double y)
        {
            x += _offsetX;
            y += _offsetY;
            double fx = Math.Floor(x);
            double fy = Math.Floor(y);
            int xi = (int)fx & 255;
            int yi = (int)fy & 255;
            double xf = x - fx;
            double yf = y - fy;

            double u = Fade(xf);
            double v = Fade(yf);

            int aa = _perm[_perm[xi] + yi];
            int ab = _perm[_perm[xi] + yi + 1];
            int ba = _perm[_perm[xi + 1] + yi];
            int bb = _perm[_perm[xi + 1] + yi + 1];

            double x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
            double x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);
            return Lerp(x1, x2, v) * 1.414;
        }

        /// <summary>Fractal Brownian motion, result in about [-1, 1].</summary>
        public double Fbm(double x, double y, int octaves, double lacunarity = 2.0, double gain = 0.5)
        {
            double sum = 0;
            double amp = 1;
            double norm = 0;
            for (int i = 0; i < octaves; i++)
            {
                sum += Sample(x, y) * amp;
                norm += amp;
                amp *= gain;
                x *= lacunarity;
                y *= lacunarity;
            }

            return sum / norm;
        }

        /// <summary>Ridged noise (good for mountain chains), result in [0, 1].</summary>
        public double Ridged(double x, double y, int octaves, double lacunarity = 2.0, double gain = 0.5)
        {
            double sum = 0;
            double amp = 1;
            double norm = 0;
            for (int i = 0; i < octaves; i++)
            {
                double n = 1.0 - Math.Abs(Sample(x, y));
                sum += n * n * amp;
                norm += amp;
                amp *= gain;
                x *= lacunarity;
                y *= lacunarity;
            }

            return sum / norm;
        }

        private static double Fade(double t) => t * t * t * ((t * ((t * 6) - 15)) + 10);

        private static double Lerp(double a, double b, double t) => a + (t * (b - a));

        private static double Grad(int hash, double x, double y)
        {
            int h = hash % 12;
            return (GradX[h] * x) + (GradY[h] * y);
        }
    }

    internal static class MathUtil
    {
        public static double SmoothStep(double edge0, double edge1, double x)
        {
            if (edge1 <= edge0)
            {
                return x >= edge1 ? 1 : 0;
            }

            double t = Math.Clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
            return t * t * (3 - (2 * t));
        }

        public static int RoundTo(int value, int step)
        {
            if (step <= 1)
            {
                return value;
            }

            return (int)Math.Round(value / (double)step, MidpointRounding.AwayFromZero) * step;
        }

        /// <summary>
        /// Returns the threshold so that about <paramref name="fraction"/> of the selected values are above it.
        /// Uses a histogram, so it also works for 30 million tiles.
        /// </summary>
        public static float ThresholdForTopFraction(float[] values, Func<int, bool> include, double fraction, int count)
        {
            if (fraction <= 0)
            {
                return float.PositiveInfinity;
            }

            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            long total = 0;
            for (int i = 0; i < count; i++)
            {
                if (include != null && !include(i))
                {
                    continue;
                }

                float v = values[i];
                if (v < min)
                {
                    min = v;
                }

                if (v > max)
                {
                    max = v;
                }

                total++;
            }

            if (total == 0)
            {
                return float.PositiveInfinity;
            }

            if (fraction >= 1)
            {
                return float.NegativeInfinity;
            }

            if (max <= min)
            {
                return min;
            }

            const int bins = 4096;
            long[] histogram = new long[bins];
            double scale = (bins - 1) / (double)(max - min);
            for (int i = 0; i < count; i++)
            {
                if (include != null && !include(i))
                {
                    continue;
                }

                histogram[(int)((values[i] - min) * scale)]++;
            }

            long wanted = (long)Math.Round(total * fraction);
            long acc = 0;
            for (int b = bins - 1; b >= 0; b--)
            {
                acc += histogram[b];
                if (acc >= wanted)
                {
                    return (float)(min + (b / scale));
                }
            }

            return min;
        }
    }
}
