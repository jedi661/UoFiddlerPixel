using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter
{
    /// <summary>Where texture B appears in a transition.</summary>
    internal enum TransitionShape
    {
        LeftToRight,
        TopToBottom,
        DiagonalDown,
        DiagonalUp,
        CornerTopLeft,
        CornerTopRight,
        CornerBottomLeft,
        CornerBottomRight,
        Island
    }

    /// <summary>Soft transitions between two textures (e.g. grass → sand).</summary>
    internal static class TextureBlend
    {
        public static readonly string[] ShapeNames =
        {
            "Left → right", "Top → bottom", "Diagonal ↘", "Diagonal ↗",
            "Corner top left", "Corner top right", "Corner bottom left", "Corner bottom right", "Island (center)"
        };

        /// <summary>Smooth value noise in 0..1 that repeats over the image (tileable), two octaves.</summary>
        private static double[] Noise(int w, int h, int seed)
        {
            var rnd = new Random(seed);
            double[] Grid(int cells)
            {
                var g = new double[cells * cells];
                for (int i = 0; i < g.Length; i++)
                {
                    g[i] = rnd.NextDouble();
                }
                return g;
            }
            double Sample(double[] g, int cells, double u, double v)
            {
                double x = u * cells, y = v * cells;
                int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
                double fx = x - x0, fy = y - y0;
                fx = fx * fx * (3 - 2 * fx);
                fy = fy * fy * (3 - 2 * fy);
                int Wrap(int i) => ((i % cells) + cells) % cells;
                double a = g[Wrap(y0) * cells + Wrap(x0)], b = g[Wrap(y0) * cells + Wrap(x0 + 1)];
                double c = g[Wrap(y0 + 1) * cells + Wrap(x0)], d = g[Wrap(y0 + 1) * cells + Wrap(x0 + 1)];
                return (a * (1 - fx) + b * fx) * (1 - fy) + (c * (1 - fx) + d * fx) * fy;
            }
            double[] g1 = Grid(4), g2 = Grid(9);
            var result = new double[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double u = (double)x / w, v = (double)y / h;
                    result[y * w + x] = Sample(g1, 4, u, v) * 0.65 + Sample(g2, 9, u, v) * 0.35;
                }
            }
            return result;
        }

        /// <summary>0 = texture A, 1 = texture B, before softness and noise. The border is at 0.5.</summary>
        private static double ShapeValue(TransitionShape shape, double u, double v)
        {
            const double cornerRadius = 0.5;
            switch (shape)
            {
                case TransitionShape.LeftToRight: return u;
                case TransitionShape.TopToBottom: return v;
                case TransitionShape.DiagonalDown: return (u + v) / 2;
                case TransitionShape.DiagonalUp: return (u + 1 - v) / 2;
                case TransitionShape.CornerTopLeft: return 1 - Math.Sqrt(u * u + v * v) / (2 * cornerRadius);
                case TransitionShape.CornerTopRight: return 1 - Math.Sqrt((1 - u) * (1 - u) + v * v) / (2 * cornerRadius);
                case TransitionShape.CornerBottomLeft: return 1 - Math.Sqrt(u * u + (1 - v) * (1 - v)) / (2 * cornerRadius);
                case TransitionShape.CornerBottomRight: return 1 - Math.Sqrt((1 - u) * (1 - u) + (1 - v) * (1 - v)) / (2 * cornerRadius);
                default: // island
                    return 1 - Math.Sqrt((u - 0.5) * (u - 0.5) + (v - 0.5) * (v - 0.5)) / 0.7;
            }
        }

        /// <summary>
        /// Mixes texture B into texture A. softness 0..100 = width of the soft border,
        /// roughness 0..100 = how much noise moves the border, seed = which noise pattern.
        /// </summary>
        public static Bitmap MakeTransition(Bitmap a, Bitmap b, TransitionShape shape, int softness, int roughness, int seed)
        {
            int w = a.Width, h = a.Height;
            using (Bitmap bScaled = TextureImaging.Resize(b, w, h, TextureImaging.ResizeQuality.Smooth))
            {
                int[] pa = CutterImaging.GetPixels(a);
                int[] pb = CutterImaging.GetPixels(bScaled);
                double[] noise = roughness > 0 ? Noise(w, h, seed) : null;
                double width = Math.Max(0.004, softness / 100.0 * 0.5);
                double rough = roughness / 100.0 * 0.6;
                var dst = new int[pa.Length];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        double v = ShapeValue(shape, (x + 0.5) / w, (y + 0.5) / h);
                        if (noise != null)
                        {
                            v += (noise[i] - 0.5) * rough;
                        }
                        double t = (v - 0.5) / width + 0.5;
                        t = Math.Max(0, Math.Min(1, t));
                        t = t * t * (3 - 2 * t);
                        int p = pa[i], q = pb[i];
                        int Mix(int shift) => (int)(((p >> shift) & 0xFF) * (1 - t) + ((q >> shift) & 0xFF) * t + 0.5);
                        dst[i] = (Mix(24) << 24) | (Mix(16) << 16) | (Mix(8) << 8) | Mix(0);
                    }
                }
                var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                CutterImaging.SetPixels(result, dst);
                return result;
            }
        }
    }

    /// <summary>A color palette that can be saved, loaded and filled from UO hues.</summary>
    internal sealed class CutterPalette
    {
        public List<Color> Colors { get; } = new List<Color>();

        public static string DefaultFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TextureCutterPalette.pal");

        public void Add(Color c)
        {
            c = Color.FromArgb(255, c);
            if (!Colors.Any(x => x.ToArgb() == c.ToArgb()))
            {
                Colors.Add(c);
            }
        }

        /// <summary>Saves as JASC-PAL (Paint Shop Pro), readable by many paint programs.</summary>
        public void Save(string file)
        {
            var sb = new StringBuilder();
            sb.AppendLine("JASC-PAL");
            sb.AppendLine("0100");
            sb.AppendLine(Colors.Count.ToString(CultureInfo.InvariantCulture));
            foreach (Color c in Colors)
            {
                sb.AppendLine($"{c.R} {c.G} {c.B}");
            }
            File.WriteAllText(file, sb.ToString());
        }

        /// <summary>Reads JASC-PAL, GIMP .gpl or one hex code (#RRGGBB) per line.</summary>
        public static CutterPalette Load(string file)
        {
            var palette = new CutterPalette();
            string[] lines = File.ReadAllLines(file);
            bool jasc = lines.Length > 0 && lines[0].Trim() == "JASC-PAL";
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("#") && line.Length != 7 || line.StartsWith("GIMP") || line.StartsWith("Name:") || line.StartsWith("Columns:"))
                {
                    continue;
                }
                if (jasc && i < 3)
                {
                    continue;
                }
                if (line.StartsWith("#") || (line.Length == 6 && int.TryParse(line, NumberStyles.HexNumber, null, out _)))
                {
                    if (int.TryParse(line.TrimStart('#'), NumberStyles.HexNumber, null, out int rgb))
                    {
                        palette.Add(Color.FromArgb(rgb));
                    }
                    continue;
                }
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 && byte.TryParse(parts[0], out byte r) && byte.TryParse(parts[1], out byte g) && byte.TryParse(parts[2], out byte b))
                {
                    palette.Add(Color.FromArgb(r, g, b));
                }
            }
            return palette;
        }

        /// <summary>The 32 colors of a UO hue (hues.mul), or null.</summary>
        public static List<Color> FromHue(int index, out string name)
        {
            name = null;
            try
            {
                lock (SdkSync.Lock)
                {
                    Hue hue = Hues.GetHue(index);
                    if (hue?.Colors == null)
                    {
                        return null;
                    }
                    name = hue.Name;
                    return hue.Colors.Select(c =>
                    {
                        int r = (c >> 10) & 0x1F, g = (c >> 5) & 0x1F, b = c & 0x1F;
                        return Color.FromArgb((r << 3) | (r >> 2), (g << 3) | (g >> 2), (b << 3) | (b >> 2));
                    }).ToList();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
