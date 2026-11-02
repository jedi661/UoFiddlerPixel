/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Hues
{
    /// <summary>How sure the classic colors of an EC hue are.</summary>
    public enum HueDecodeQuality
    {
        /// <summary>Taken from the stored classic data and checked against the bitmap.</summary>
        Exact,

        /// <summary>Calculated back; drawing the colors again gives the bitmap pixel for pixel.</summary>
        Verified,

        /// <summary>Calculated back; at most 2 steps (of 255) difference to the bitmap.</summary>
        Close,

        /// <summary>The bitmap was not made from 32 classic colors (e.g. an imported image); nearest colors.</summary>
        Approximate
    }

    public sealed class HueDecodeResult
    {
        public ushort[] Colors { get; set; }
        public HueDecodeQuality Quality { get; set; }

        /// <summary>Largest difference (0..255) between the bitmap and the colors drawn again.</summary>
        public int MaxDiff { get; set; }

        public static string Text(HueDecodeQuality quality) => quality switch
        {
            HueDecodeQuality.Exact => "exact (stored)",
            HueDecodeQuality.Verified => "exact (verified)",
            HueDecodeQuality.Close => "close",
            _ => "approximate"
        };
    }

    /// <summary>
    /// Calculates the 32 classic colors back from an EC hue bitmap (and the hues.dds row).
    /// <para>
    /// Both ramp styles are linear in the colors, so the 32 values of each channel are found by least squares,
    /// rounded to the 32 possible values and then corrected step by step with the exact integer drawing
    /// until drawing them again gives the same pixels. Line 1 (color 31) and the hues.dds row are used as
    /// additional measurements.
    /// </para>
    /// </summary>
    public sealed class EcHueDecoder
    {
        private const int Width = EcHueRender.RampWidth;

        private static readonly Dictionary<(HueRampStyle, int), double[,]> _inverseCache = new Dictionary<(HueRampStyle, int), double[,]>();
        private static readonly object _cacheLock = new object();

        /// <summary>Blur radii with different weights (the others give the same filter), most usual first.</summary>
        public static readonly int[] DistinctBlurs = { 4, 6, 0, 1, 2, 3, 7, 8, 12, 18 };

        public HueRenderOptions Options { get; }

        public EcHueDecoder(HueRenderOptions options)
        {
            Options = options ?? new HueRenderOptions();
        }

        /// <summary>
        /// Decodes a hue bitmap (BGRA, any size; 256 x 2 gives exact results). <paramref name="ddsRow"/> (256 BGRA) is optional.
        /// </summary>
        public HueDecodeResult Decode(byte[] pixels, int width, int height, byte[] ddsRow = null, bool refine = true)
        {
            byte[] line0 = width == Width ? pixels : EcHueRender.ResampleRow(pixels, width, 0);
            int line1Offset = height >= 2 ? (width + (width / 2)) * 4 : -1;

            var channels = new int[3][];
            int maxDiff = 0;
            for (int ch = 0; ch < 3; ch++)
            {
                var y = new int[Width];
                for (int x = 0; x < Width; x++)
                {
                    y[x] = line0[(x * 4) + ch];
                }
                int anchor = line1Offset >= 0 ? pixels[line1Offset + ch] : -1;
                int[] dds = null;
                if (ddsRow != null)
                {
                    dds = new int[Width];
                    for (int x = 0; x < Width; x++)
                    {
                        dds[x] = ddsRow[(x * 4) + ch];
                    }
                }
                channels[ch] = SolveChannel(y, anchor, dds, refine, out int diff);
                maxDiff = Math.Max(maxDiff, diff);
            }

            var result = new ushort[32];
            for (int i = 0; i < 32; i++)
            {
                result[i] = ClassicHue.Make(channels[2][i], channels[1][i], channels[0][i]);
            }

            HueDecodeQuality quality = width != Width ? HueDecodeQuality.Approximate
                : maxDiff == 0 ? HueDecodeQuality.Verified
                : maxDiff <= 2 ? HueDecodeQuality.Close
                : HueDecodeQuality.Approximate;
            return new HueDecodeResult { Colors = result, Quality = quality, MaxDiff = maxDiff };
        }

        // ------------------------------------------------------------------ solving

        private int[] SolveChannel(int[] y, int anchor, int[] dds, bool refine, out int maxDiff)
        {
            int[] levels = EcHueRender.Levels(Options.Scale);
            double[,] inverse = Inverse(Options.BmpStyle, Options.BmpBlur);

            var v = new int[32];
            for (int i = 0; i < 32; i++)
            {
                double sum = 0;
                for (int x = 0; x < Width; x++)
                {
                    sum += inverse[i, x] * y[x];
                }
                v[i] = EcHueRender.To5(sum, Options.Scale);
            }
            // line 1 is color 31 without blur
            if (anchor >= 0)
            {
                int a = EcHueRender.To5(anchor, Options.Scale);
                if (levels[a] == anchor)
                {
                    v[31] = a;
                }
            }

            long error = Error(v, y, anchor, dds, levels);
            if (refine && error > 0)
            {
                int[] steps = { -1, 1, -2, 2, -3, 3 };
                for (int pass = 0; pass < 8 && error > 0; pass++)
                {
                    bool improved = false;
                    for (int i = 0; i < 32 && error > 0; i++)
                    {
                        int keep = v[i];
                        foreach (int s in steps)
                        {
                            int candidate = keep + s;
                            if (candidate < 0 || candidate > 31)
                            {
                                continue;
                            }
                            v[i] = candidate;
                            long e = Error(v, y, anchor, dds, levels);
                            if (e < error)
                            {
                                error = e;
                                keep = candidate;
                                improved = true;
                            }
                        }
                        v[i] = keep;
                    }
                    if (!improved)
                    {
                        break;
                    }
                }
            }

            int[] row = Render(v, levels, Options.BmpStyle, Options.BmpBlur);
            maxDiff = 0;
            for (int x = 0; x < Width; x++)
            {
                maxDiff = Math.Max(maxDiff, Math.Abs(row[x] - y[x]));
            }
            return v;
        }

        private static int[] Render(int[] v, int[] levels, HueRampStyle style, int blur)
        {
            var values = new int[32];
            for (int i = 0; i < 32; i++)
            {
                values[i] = levels[v[i]];
            }
            return EcHueRender.RenderChannel(values, style, blur);
        }

        private long Error(int[] v, int[] y, int anchor, int[] dds, int[] levels)
        {
            long e = 0;
            int[] row = Render(v, levels, Options.BmpStyle, Options.BmpBlur);
            for (int x = 0; x < Width; x++)
            {
                int d = row[x] - y[x];
                e += d * d;
            }
            if (anchor >= 0)
            {
                int d = levels[v[31]] - anchor;
                e += 8L * d * d;
            }
            if (dds != null)
            {
                row = Render(v, levels, Options.DdsStyle, Options.DdsBlur);
                for (int x = 0; x < Width; x++)
                {
                    int d = row[x] - dds[x];
                    e += d * d;
                }
            }
            return e;
        }

        /// <summary>Least squares solution matrix (32 x 256) of the (linear) ramp model.</summary>
        private static double[,] Inverse(HueRampStyle style, int radius)
        {
            (HueRampStyle, int) key = (style, style == HueRampStyle.Linear ? 0 : EcHueRender.Alpha(radius));
            lock (_cacheLock)
            {
                if (_inverseCache.TryGetValue(key, out double[,] cached))
                {
                    return cached;
                }
            }

            var a = new double[Width, 32];
            for (int b = 0; b < 32; b++)
            {
                double[] row = EcHueRender.Response(b, style, radius);
                for (int x = 0; x < Width; x++)
                {
                    a[x, b] = row[x];
                }
            }

            // (AᵀA)⁻¹ Aᵀ
            var ata = new double[32, 32];
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    double s = 0;
                    for (int x = 0; x < Width; x++)
                    {
                        s += a[x, i] * a[x, j];
                    }
                    ata[i, j] = s + (i == j ? 1e-9 : 0);
                }
            }
            double[,] inv = Invert(ata);
            var result = new double[32, Width];
            for (int i = 0; i < 32; i++)
            {
                for (int x = 0; x < Width; x++)
                {
                    double s = 0;
                    for (int k = 0; k < 32; k++)
                    {
                        s += inv[i, k] * a[x, k];
                    }
                    result[i, x] = s;
                }
            }
            lock (_cacheLock)
            {
                _inverseCache[key] = result;
            }
            return result;
        }

        private static double[,] Invert(double[,] m)
        {
            int n = m.GetLength(0);
            var a = (double[,])m.Clone();
            var inv = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                inv[i, i] = 1;
            }
            for (int col = 0; col < n; col++)
            {
                int pivot = col;
                for (int r = col + 1; r < n; r++)
                {
                    if (Math.Abs(a[r, col]) > Math.Abs(a[pivot, col]))
                    {
                        pivot = r;
                    }
                }
                if (pivot != col)
                {
                    for (int k = 0; k < n; k++)
                    {
                        (a[col, k], a[pivot, k]) = (a[pivot, k], a[col, k]);
                        (inv[col, k], inv[pivot, k]) = (inv[pivot, k], inv[col, k]);
                    }
                }
                double p = a[col, col];
                for (int k = 0; k < n; k++)
                {
                    a[col, k] /= p;
                    inv[col, k] /= p;
                }
                for (int r = 0; r < n; r++)
                {
                    double f = a[r, col];
                    if (r == col || f == 0)
                    {
                        continue;
                    }
                    for (int k = 0; k < n; k++)
                    {
                        a[r, k] -= f * a[col, k];
                        inv[r, k] -= f * inv[col, k];
                    }
                }
            }
            return inv;
        }

        // ------------------------------------------------------------------ detection

        /// <summary>All ramp models to try: linear first (original client), then the blurs.</summary>
        private static IEnumerable<(HueRampStyle Style, int Blur)> Models()
        {
            yield return (HueRampStyle.Linear, 0);
            foreach (int blur in DistinctBlurs)
            {
                yield return (HueRampStyle.Blurred, blur);
            }
        }

        /// <summary>
        /// Finds the drawing settings (color scale, bitmap ramp, hues.dds ramp) the hue bitmaps of a file were made with,
        /// by trying every combination on samples (bitmap pixels 256 x 2, optionally the hues.dds row).
        /// <paramref name="exactSamples"/> = samples that are drawn back pixel exact with the result.
        /// </summary>
        public static HueRenderOptions Detect(IList<(byte[] Pixels, byte[] DdsRow)> samples, out int exactSamples)
        {
            exactSamples = 0;
            var best = HueRenderOptions.OriginalClient;
            if (samples.Count == 0)
            {
                return best;
            }

            long bestScore = long.MaxValue;
            foreach (HueColorScale scale in new[] { HueColorScale.Shift, HueColorScale.Exact, HueColorScale.Replicate })
            {
                foreach ((HueRampStyle style, int blur) in Models())
                {
                    var options = new HueRenderOptions { BmpStyle = style, BmpBlur = blur, DdsStyle = style, DdsBlur = style == HueRampStyle.Linear ? 6 : blur == 4 ? 6 : blur, Scale = scale };
                    var decoder = new EcHueDecoder(options);
                    long score = 0;
                    int exact = 0;
                    foreach ((byte[] pixels, _) in samples)
                    {
                        HueDecodeResult r = decoder.Decode(pixels, Width, 2, null, false);
                        score += r.MaxDiff;
                        if (r.MaxDiff == 0)
                        {
                            exact++;
                        }
                    }
                    // more exact samples first, then the smaller difference; on a tie the earlier (usual) settings win
                    long combined = ((long)(samples.Count - exact) << 32) + score;
                    if (combined < bestScore)
                    {
                        bestScore = combined;
                        best = options;
                        exactSamples = exact;
                    }
                }
            }

            // hues.dds: draw the found colors with each model and compare with the rows
            var withDds = samples.Where(s => s.DdsRow != null).ToList();
            if (withDds.Count > 0)
            {
                var decoder = new EcHueDecoder(best);
                List<ushort[]> decoded = withDds.Select(s => decoder.Decode(s.Pixels, Width, 2, null, true).Colors).ToList();
                long bestDds = long.MaxValue;
                foreach ((HueRampStyle style, int blur) in Models())
                {
                    long score = 0;
                    for (int i = 0; i < withDds.Count; i++)
                    {
                        byte[] row = EcHueRender.RenderRow(decoded[i], style, blur, best.Scale);
                        byte[] dds = withDds[i].DdsRow;
                        for (int x = 0; x < Width * 4; x++)
                        {
                            if ((x & 3) != 3)
                            {
                                score += Math.Abs(row[x] - dds[x]);
                            }
                        }
                    }
                    if (score < bestDds)
                    {
                        bestDds = score;
                        best.DdsStyle = style;
                        best.DdsBlur = style == HueRampStyle.Linear ? best.DdsBlur : blur;
                    }
                }
            }
            return best;
        }
    }
}
