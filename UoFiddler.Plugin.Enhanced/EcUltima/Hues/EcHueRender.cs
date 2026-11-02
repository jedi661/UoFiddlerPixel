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
using System.Globalization;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Hues
{
    /// <summary>How a 5 bit classic color value (0..31) becomes an 8 bit value.</summary>
    public enum HueColorScale : byte
    {
        /// <summary>v * 255 / 31 (31 → 255). Used by huesmul2uop and this plugin.</summary>
        Exact = 0,

        /// <summary>v * 8 (31 → 248), the AxisII way.</summary>
        Shift = 1,

        /// <summary>(v &lt;&lt; 3) | (v &gt;&gt; 2), bit replication (31 → 255).</summary>
        Replicate = 2
    }

    /// <summary>How the 32 colors become a 256 pixel ramp.</summary>
    public enum HueRampStyle : byte
    {
        /// <summary>Linear between the colors, color i at pixel 8 * i (the original client files).</summary>
        Linear = 0,

        /// <summary>8 pixels per color, then blurred (huesmul2uop; blur 0 = hard blocks).</summary>
        Blurred = 1
    }

    /// <summary>
    /// How the classic colors are drawn into the EC images. The same settings must be used to get the colors back exactly.
    /// </summary>
    public sealed class HueRenderOptions
    {
        /// <summary>Ramp of line 0 of the hue bitmaps.</summary>
        public HueRampStyle BmpStyle { get; set; } = HueRampStyle.Linear;

        /// <summary>Blur of line 0 when <see cref="BmpStyle"/> is blurred (0 = none, 1..17; huesmul2uop: 4).</summary>
        public int BmpBlur { get; set; } = 4;

        /// <summary>Ramp of the rows in build/hues/hues.dds.</summary>
        public HueRampStyle DdsStyle { get; set; } = HueRampStyle.Linear;

        /// <summary>Blur of the hues.dds rows when blurred (huesmul2uop: 6).</summary>
        public int DdsBlur { get; set; } = 6;

        public HueColorScale Scale { get; set; } = HueColorScale.Shift;

        /// <summary>Like the files of the original Enhanced Client (linear ramps, colors * 8).</summary>
        public static HueRenderOptions OriginalClient => new HueRenderOptions();

        /// <summary>Like huesmul2uop (blurred blocks, colors * 255 / 31).</summary>
        public static HueRenderOptions HuesMul2Uop => new HueRenderOptions { BmpStyle = HueRampStyle.Blurred, BmpBlur = 4, DdsStyle = HueRampStyle.Blurred, DdsBlur = 6, Scale = HueColorScale.Exact };

        public HueRenderOptions Clone() => new HueRenderOptions { BmpStyle = BmpStyle, BmpBlur = BmpBlur, DdsStyle = DdsStyle, DdsBlur = DdsBlur, Scale = Scale };

        public bool SameAs(HueRenderOptions other) => other != null && other.ToString() == ToString();

        public static string ModelText(HueRampStyle style, int blur) => style == HueRampStyle.Linear ? "linear" : blur == 0 ? "blocks" : "blur " + blur.ToString(CultureInfo.InvariantCulture);

        public override string ToString() => $"bitmap {ModelText(BmpStyle, BmpBlur)}, hues.dds {ModelText(DdsStyle, DdsBlur)}, colors {ScaleText(Scale)}";

        public static string ScaleText(HueColorScale scale) => scale switch
        {
            HueColorScale.Shift => "x 8",
            HueColorScale.Replicate => "bit copy",
            _ => "x 255 / 31"
        };
    }

    /// <summary>
    /// Draws the EC hue images from the 32 classic colors (original client style or huesmul2uop style):
    /// <list type="bullet">
    /// <item>hue####.bmp, 256 x 2, 32 bit: line 0 = the ramp of the 32 colors; line 1 = color 31 over the whole width.</item>
    /// <item>hues.dds, 1024 x 1024: one blurred row of 256 pixels per hue, 4 columns of 1024 hues (hue n: column n / 1024, row n % 1024).</item>
    /// <item>the 64 x 64 overview (unnamed entry 0xFA5C6A1BC0D8B01B): one pixel (color 31) per hue, column n / 64, row n % 64.</item>
    /// </list>
    /// </summary>
    public static class EcHueRender
    {
        public const int RampWidth = 256;
        public const int BlockWidth = 8;
        public const int MaxBlur = 17;

        private static readonly int[] _alphaTable = { 14, 10, 8, 6, 5, 5, 4, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2 };

        /// <summary>Weight of the blur (16 = no blur).</summary>
        public static int Alpha(int radius) => radius < 1 ? 16 : radius > MaxBlur ? 1 : _alphaTable[radius - 1];

        public static int To8(int v5, HueColorScale scale)
        {
            v5 &= 0x1F;
            return scale switch
            {
                HueColorScale.Shift => v5 * 8,
                HueColorScale.Replicate => (v5 << 3) | (v5 >> 2),
                _ => v5 * 255 / 31
            };
        }

        /// <summary>The 32 possible 8 bit values of a scale.</summary>
        public static int[] Levels(HueColorScale scale)
        {
            var levels = new int[32];
            for (int i = 0; i < 32; i++)
            {
                levels[i] = To8(i, scale);
            }
            return levels;
        }

        /// <summary>Nearest 5 bit value for an 8 bit value.</summary>
        public static int To5(double v8, HueColorScale scale)
        {
            int best = 0;
            double bestDiff = double.MaxValue;
            for (int i = 0; i < 32; i++)
            {
                double d = Math.Abs(To8(i, scale) - v8);
                if (d < bestDiff)
                {
                    bestDiff = d;
                    best = i;
                }
            }
            return best;
        }

        /// <summary>BGRA of a classic color (alpha 255).</summary>
        public static void ToBgra(ushort color, HueColorScale scale, byte[] target, int offset)
        {
            target[offset] = (byte)To8(ClassicHue.Blue(color), scale);
            target[offset + 1] = (byte)To8(ClassicHue.Green(color), scale);
            target[offset + 2] = (byte)To8(ClassicHue.Red(color), scale);
            target[offset + 3] = 255;
        }

        /// <summary>Classic color from 8 bit BGRA (nearest values).</summary>
        public static ushort FromBgra(byte[] bgra, int offset, HueColorScale scale)
        {
            return ClassicHue.Make(To5(bgra[offset + 2], scale), To5(bgra[offset + 1], scale), To5(bgra[offset], scale));
        }

        /// <summary>
        /// Blurs one row of 8 bit values in place: a pass left to right and a pass right to left
        /// (integer math identical to the blur of huesmul2uop, a one line image has no vertical pass).
        /// </summary>
        public static void BlurChannel(int[] row, int alpha)
        {
            if (alpha >= 16)
            {
                return;
            }
            int n = row.Length;
            int acc = row[0] << 4;
            for (int j = 1; j < n; j++)
            {
                acc += ((row[j] << 4) - acc) * alpha / 16;
                row[j] = (byte)(acc >> 4);
            }
            acc = row[n - 1] << 4;
            for (int j = n - 2; j >= 0; j--)
            {
                acc += ((row[j] << 4) - acc) * alpha / 16;
                row[j] = (byte)(acc >> 4);
            }
        }

        /// <summary>One channel of the ramp from the 32 values (8 bit).</summary>
        public static int[] RenderChannel(int[] values8, HueRampStyle style, int radius)
        {
            var row = new int[RampWidth];
            if (style == HueRampStyle.Linear)
            {
                for (int x = 0; x < RampWidth; x++)
                {
                    int i = x / BlockWidth;
                    int j = x % BlockWidth;
                    int a = values8[i];
                    int b = values8[Math.Min(31, i + 1)];
                    row[x] = ((a * (BlockWidth - j)) + (b * j) + (BlockWidth / 2)) / BlockWidth;
                }
                return row;
            }
            for (int x = 0; x < RampWidth; x++)
            {
                row[x] = values8[x / BlockWidth];
            }
            BlurChannel(row, Alpha(radius));
            return row;
        }

        /// <summary>The same ramp without rounding: the share of each color in each pixel (for solving).</summary>
        public static double[] Response(int block, HueRampStyle style, int radius)
        {
            var row = new double[RampWidth];
            if (style == HueRampStyle.Linear)
            {
                for (int x = 0; x < RampWidth; x++)
                {
                    int i = x / BlockWidth;
                    double t = (x % BlockWidth) / (double)BlockWidth;
                    int next = Math.Min(31, i + 1);
                    if (i == block)
                    {
                        row[x] += 1 - t;
                    }
                    if (next == block)
                    {
                        row[x] += t;
                    }
                }
                return row;
            }
            for (int x = block * BlockWidth; x < (block + 1) * BlockWidth; x++)
            {
                row[x] = 1;
            }
            double f = Alpha(radius) / 16.0;
            double acc = row[0];
            for (int j = 1; j < RampWidth; j++)
            {
                acc += (row[j] - acc) * f;
                row[j] = acc;
            }
            acc = row[RampWidth - 1];
            for (int j = RampWidth - 2; j >= 0; j--)
            {
                acc += (row[j] - acc) * f;
                row[j] = acc;
            }
            return row;
        }

        /// <summary>The ramp row (256 BGRA pixels).</summary>
        public static byte[] RenderRow(ushort[] colors, HueRampStyle style, int radius, HueColorScale scale)
        {
            var result = new byte[RampWidth * 4];
            var values = new int[32];
            for (int ch = 0; ch < 3; ch++)
            {
                for (int i = 0; i < 32; i++)
                {
                    values[i] = To8(Channel(colors[i], ch), scale);
                }
                int[] row = RenderChannel(values, style, radius);
                for (int x = 0; x < RampWidth; x++)
                {
                    result[(x * 4) + ch] = (byte)row[x];
                }
            }
            for (int x = 0; x < RampWidth; x++)
            {
                result[(x * 4) + 3] = 255;
            }
            return result;
        }

        public static byte[] RenderBmpRow(ushort[] colors, HueRenderOptions o) => RenderRow(colors, o.BmpStyle, o.BmpBlur, o.Scale);

        public static byte[] RenderDdsRow(ushort[] colors, HueRenderOptions o) => RenderRow(colors, o.DdsStyle, o.DdsBlur, o.Scale);

        /// <summary>The BGRA pixels of hue####.bmp (256 x 2).</summary>
        public static byte[] RenderRampPixels(ushort[] colors, HueRenderOptions options)
        {
            var pixels = new byte[RampWidth * 2 * 4];
            Buffer.BlockCopy(RenderBmpRow(colors, options), 0, pixels, 0, RampWidth * 4);
            for (int x = 0; x < RampWidth; x++)
            {
                ToBgra(colors[31], options.Scale, pixels, (RampWidth + x) * 4);
            }
            return pixels;
        }

        /// <summary>The complete hue####.bmp file.</summary>
        public static byte[] RenderRampBmp(ushort[] colors, HueRenderOptions options) => EcHues.WriteBmp(RenderRampPixels(colors, options), RampWidth, 2);

        /// <summary>Channel 0 = blue, 1 = green, 2 = red (BGRA order).</summary>
        public static int Channel(ushort color, int channel) => channel switch
        {
            0 => ClassicHue.Blue(color),
            1 => ClassicHue.Green(color),
            _ => ClassicHue.Red(color)
        };

        /// <summary>
        /// Hues an image like the client: the brightness of a pixel picks one of the 32 colors.
        /// <paramref name="partial"/>: only gray pixels are hued (tiledata flag "partial hue").
        /// </summary>
        public static void ApplyHue(byte[] bgra, ushort[] colors, HueColorScale scale, bool partial)
        {
            var table = new byte[32 * 3];
            for (int i = 0; i < 32; i++)
            {
                table[i * 3] = (byte)To8(ClassicHue.Blue(colors[i]), scale);
                table[(i * 3) + 1] = (byte)To8(ClassicHue.Green(colors[i]), scale);
                table[(i * 3) + 2] = (byte)To8(ClassicHue.Red(colors[i]), scale);
            }
            for (int i = 0; i + 3 < bgra.Length; i += 4)
            {
                if (bgra[i + 3] == 0)
                {
                    continue;
                }
                int b = bgra[i], g = bgra[i + 1], r = bgra[i + 2];
                if (partial && (Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b)) > 8))
                {
                    continue;
                }
                int gray = partial ? r : ((r * 299) + (g * 587) + (b * 114)) / 1000;
                int index = Math.Min(31, gray * 32 / 256);
                bgra[i] = table[index * 3];
                bgra[i + 1] = table[(index * 3) + 1];
                bgra[i + 2] = table[(index * 3) + 2];
            }
        }

        /// <summary>32 colors from a dark to a light color (linear).</summary>
        public static ushort[] Gradient(int r0, int g0, int b0, int r1, int g1, int b1, HueColorScale scale)
        {
            var colors = new ushort[32];
            for (int i = 0; i < 32; i++)
            {
                double t = i / 31.0;
                colors[i] = ClassicHue.Make(
                    To5(r0 + ((r1 - r0) * t), scale),
                    To5(g0 + ((g1 - g0) * t), scale),
                    To5(b0 + ((b1 - b0) * t), scale));
            }
            return colors;
        }

        /// <summary>Scales any image row to 256 BGRA pixels (for ramps that were imported as images).</summary>
        public static byte[] ResampleRow(byte[] bgra, int width, int y)
        {
            var result = new byte[RampWidth * 4];
            for (int x = 0; x < RampWidth; x++)
            {
                int sx = Math.Min(width - 1, x * width / RampWidth);
                Buffer.BlockCopy(bgra, ((y * width) + sx) * 4, result, x * 4, 4);
                result[(x * 4) + 3] = 255;
            }
            return result;
        }
    }
}
