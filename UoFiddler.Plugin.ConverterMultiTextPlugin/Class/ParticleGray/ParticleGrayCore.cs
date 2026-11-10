using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ParticleGray
{
    internal enum GrayMode { Luminance, Average, Lightest, Darkest }

    internal sealed class GraySettings
    {
        public GrayMode Mode = GrayMode.Luminance;
        public bool Stretch = true;      // use all 32 gray levels
        public int Brightness;           // -50..50 (levels * 0.5)
        public int Contrast;             // -50..50
        public bool AvoidBlack = true;   // level 0 would be transparent in UO
    }

    /// <summary>
    /// Particle gray (partial hue) like Ultima Online: the client recolors only pixels that are exactly gray in
    /// 15 bit (red = green = blue, 5 bit each). The gray level 0..31 picks the color of the hue table.
    /// Pure black is transparent.
    /// </summary>
    internal static class ParticleGrayCore
    {
        public static int R5(int p) => ((p >> 16) & 0xFF) >> 3;

        public static int G5(int p) => ((p >> 8) & 0xFF) >> 3;

        public static int B5(int p) => (p & 0xFF) >> 3;

        /// <summary>True if UO would hue this pixel (exact gray in 15 bit, not transparent, not black).</summary>
        public static bool IsHued(int p)
        {
            if (((p >> 24) & 0xFF) < 128)
            {
                return false;
            }
            int r = R5(p);
            return r == G5(p) && r == B5(p) && r > 0;
        }

        public static int GrayLevel(int p) => R5(p);

        /// <summary>A 5 bit gray level as 24 bit color (exactly what UO stores).</summary>
        public static int LevelToArgb(int level)
        {
            level = Math.Max(0, Math.Min(31, level));
            int v = (level << 3) | (level >> 2);
            return unchecked((int)0xFF000000) | (v << 16) | (v << 8) | v;
        }

        private static double Value(int p, GrayMode mode)
        {
            int r = (p >> 16) & 0xFF, g = (p >> 8) & 0xFF, b = p & 0xFF;
            switch (mode)
            {
                case GrayMode.Average: return (r + g + b) / 3.0;
                case GrayMode.Lightest: return Math.Max(r, Math.Max(g, b));
                case GrayMode.Darkest: return Math.Min(r, Math.Min(g, b));
                default: return r * 0.299 + g * 0.587 + b * 0.114;
            }
        }

        private static bool Use(bool[] mask, int i) => mask == null || mask[i];

        /// <summary>Converts the marked pixels (or all when mask is null) into UO particle gray.</summary>
        public static Bitmap Convert(Bitmap source, bool[] mask, GraySettings s, out int count)
        {
            Bitmap result = CutterImaging.To32Bpp(source);
            int[] px = CutterImaging.GetPixels(result);
            double min = 255, max = 0;
            for (int i = 0; i < px.Length; i++)
            {
                if (Use(mask, i) && ((px[i] >> 24) & 0xFF) >= 128 && (px[i] & 0xFFFFFF) != 0)
                {
                    double v = Value(px[i], s.Mode);
                    min = Math.Min(min, v);
                    max = Math.Max(max, v);
                }
            }
            count = 0;
            double contrast = 1 + s.Contrast / 50.0;
            for (int i = 0; i < px.Length; i++)
            {
                int p = px[i];
                if (!Use(mask, i) || ((p >> 24) & 0xFF) < 128 || (p & 0xFFFFFF) == 0)
                {
                    continue; // transparent and black stay as they are
                }
                double v = Value(p, s.Mode) / 255.0;
                if (s.Stretch && max > min)
                {
                    v = (Value(p, s.Mode) - min) / (max - min);
                }
                v = (v - 0.5) * contrast + 0.5 + s.Brightness / 100.0;
                int level = (int)Math.Round(Math.Max(0, Math.Min(1, v)) * 31);
                if (s.AvoidBlack && level < 1)
                {
                    level = 1;
                }
                px[i] = (p & unchecked((int)0xFF000000)) | (LevelToArgb(level) & 0xFFFFFF);
                count++;
            }
            CutterImaging.SetPixels(result, px);
            return result;
        }

        /// <summary>Puts the original colors back into the marked pixels.</summary>
        public static Bitmap Restore(Bitmap current, Bitmap original, bool[] mask)
        {
            Bitmap result = CutterImaging.To32Bpp(current);
            if (original == null || original.Size != current.Size)
            {
                return result;
            }
            int[] px = CutterImaging.GetPixels(result), orig = CutterImaging.GetPixels(original);
            for (int i = 0; i < px.Length; i++)
            {
                if (Use(mask, i))
                {
                    px[i] = orig[i];
                }
            }
            CutterImaging.SetPixels(result, px);
            return result;
        }

        /// <summary>Old "Dyeing in": gray pixels (in the mask) get the color, their brightness is kept.</summary>
        public static Bitmap Colorize(Bitmap source, bool[] mask, Color color, out int count)
        {
            Bitmap result = CutterImaging.To32Bpp(source);
            int[] px = CutterImaging.GetPixels(result);
            count = 0;
            for (int i = 0; i < px.Length; i++)
            {
                int p = px[i];
                if (!Use(mask, i) || !IsHued(p))
                {
                    continue;
                }
                double f = ((p >> 16) & 0xFF) / 255.0;
                px[i] = (p & unchecked((int)0xFF000000)) | ((int)(color.R * f) << 16) | ((int)(color.G * f) << 8) | (int)(color.B * f);
                count++;
            }
            CutterImaging.SetPixels(result, px);
            return result;
        }

        /// <summary>
        /// Colored areas can contain pixels that are exactly gray by chance – UO would hue them too.
        /// Moves those pixels (outside the protected mask) by one 5 bit step in blue so they keep their color.
        /// </summary>
        public static Bitmap FixAccidentalGrays(Bitmap source, bool[] keep, out int count)
        {
            Bitmap result = CutterImaging.To32Bpp(source);
            int[] px = CutterImaging.GetPixels(result);
            count = 0;
            for (int i = 0; i < px.Length; i++)
            {
                int p = px[i];
                if ((keep != null && keep[i]) || !IsHued(p))
                {
                    continue;
                }
                int b5 = B5(p);
                b5 = b5 < 31 ? b5 + 1 : b5 - 1;
                int b = (b5 << 3) | (b5 >> 2);
                px[i] = (p & unchecked((int)0xFFFFFF00)) | b;
                count++;
            }
            CutterImaging.SetPixels(result, px);
            return result;
        }

        /// <summary>The image as UO shows it with a hue: only gray pixels (partial hue) or all pixels.</summary>
        public static Bitmap ApplyHue(Bitmap source, ushort[] hueColors, bool onlyGray)
        {
            Bitmap result = CutterImaging.To32Bpp(source);
            if (hueColors == null || hueColors.Length < 32)
            {
                return result;
            }
            var table = new int[32];
            for (int i = 0; i < 32; i++)
            {
                int c = hueColors[i];
                int r = (c >> 10) & 0x1F, g = (c >> 5) & 0x1F, b = c & 0x1F;
                table[i] = ((r << 3) | (r >> 2)) << 16 | ((g << 3) | (g >> 2)) << 8 | ((b << 3) | (b >> 2));
            }
            int[] px = CutterImaging.GetPixels(result);
            for (int i = 0; i < px.Length; i++)
            {
                int p = px[i];
                if (((p >> 24) & 0xFF) < 128 || (p & 0xFFFFFF) == 0)
                {
                    continue;
                }
                if (onlyGray && !IsHued(p))
                {
                    continue;
                }
                px[i] = (p & unchecked((int)0xFF000000)) | table[R5(p)];
            }
            CutterImaging.SetPixels(result, px);
            return result;
        }

        /// <summary>Shows which pixels UO would hue (magenta), the rest dimmed.</summary>
        public static Bitmap HighlightHued(Bitmap source)
        {
            Bitmap result = CutterImaging.To32Bpp(source);
            int[] px = CutterImaging.GetPixels(result);
            for (int i = 0; i < px.Length; i++)
            {
                int p = px[i];
                if (((p >> 24) & 0xFF) == 0)
                {
                    continue;
                }
                if (IsHued(p))
                {
                    int l = R5(p) * 8;
                    px[i] = unchecked((int)0xFF000000) | (Math.Min(255, 80 + l) << 16) | (l / 3 << 8) | Math.Min(255, 80 + l);
                }
                else
                {
                    int g = (int)(((p >> 16) & 0xFF) * 0.3 + ((p >> 8) & 0xFF) * 0.59 + (p & 0xFF) * 0.11) / 3;
                    px[i] = unchecked((int)0xFF000000) | (g << 16) | (g << 8) | g;
                }
            }
            CutterImaging.SetPixels(result, px);
            return result;
        }

        /// <summary>How many pixels use each of the 32 gray levels (only hued pixels).</summary>
        public static int[] LevelHistogram(Bitmap source, out int hued, out int total)
        {
            var hist = new int[32];
            hued = total = 0;
            foreach (int p in CutterImaging.GetPixels(source))
            {
                if (((p >> 24) & 0xFF) < 128 || (p & 0xFFFFFF) == 0)
                {
                    continue;
                }
                total++;
                if (IsHued(p))
                {
                    hist[R5(p)]++;
                    hued++;
                }
            }
            return hist;
        }

        public static ushort[] HueColors(int hueNumber, out string name)
        {
            name = null;
            try
            {
                lock (SdkSync.Lock)
                {
                    if (Hues.List == null || hueNumber < 1 || hueNumber > Hues.List.Length)
                    {
                        return null;
                    }
                    Hue hue = Hues.List[hueNumber - 1];
                    name = hue.Name;
                    return (ushort[])hue.Colors.Clone();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static int HueCount
        {
            get
            {
                try
                {
                    return Hues.List?.Length ?? 0;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        #region Mask helpers

        public static bool[] MaskWhere(Bitmap source, Func<int, bool> test)
        {
            int[] px = CutterImaging.GetPixels(source);
            var m = new bool[px.Length];
            for (int i = 0; i < px.Length; i++)
            {
                m[i] = test(px[i]);
            }
            return m;
        }

        /// <summary>Colored (not gray, not transparent, not black) pixels.</summary>
        public static bool IsColored(int p) => ((p >> 24) & 0xFF) >= 128 && (p & 0xFFFFFF) != 0 && !IsHued(p);

        public static bool[] Grow(bool[] m, int w, int h, bool grow)
        {
            var r = (bool[])m.Clone();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    if (m[i] == grow)
                    {
                        continue;
                    }
                    bool n = (x > 0 && m[i - 1] == grow) || (x < w - 1 && m[i + 1] == grow) || (y > 0 && m[i - w] == grow) || (y < h - 1 && m[i + w] == grow);
                    if (n)
                    {
                        r[i] = grow;
                    }
                }
            }
            return r;
        }

        #endregion

        #region Stamps

        public static string StampFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ParticleGrayStamps");

        public static readonly string[] ShapeNames = { "Circle", "Square", "Diamond", "Ring", "Star", "Cross", "Triangle", "Heart" };

        /// <summary>A built-in stamp shape of the given size.</summary>
        public static bool[,] MakeShape(int index, int size)
        {
            size = Math.Max(1, size);
            var s = new bool[size, size];
            double c = (size - 1) / 2.0, r = size / 2.0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double dx = (x - c) / r, dy = (y - c) / r;
                    double d = Math.Sqrt(dx * dx + dy * dy);
                    bool v;
                    switch (index)
                    {
                        case 1: v = true; break;
                        case 2: v = Math.Abs(dx) + Math.Abs(dy) <= 1.0; break;
                        case 3: v = d <= 1.0 && d >= 0.55; break;
                        case 4:
                            double a = Math.Atan2(dy, dx);
                            double star = 0.55 + 0.45 * Math.Pow(Math.Abs(Math.Cos(2.5 * a)), 3);
                            v = d <= star;
                            break;
                        case 5: v = Math.Abs(dx) <= 0.3 || Math.Abs(dy) <= 0.3; break;
                        case 6: v = dy >= -1 && Math.Abs(dx) <= (dy + 1) / 2.0; break;
                        case 7:
                            double hx = dx * 1.1, hy = -dy * 1.1 + 0.25;
                            double q = hx * hx + hy * hy - 0.6;
                            v = q * q * q - hx * hx * hy * hy * hy <= 0;
                            break;
                        default: v = d <= 1.0; break;
                    }
                    s[x, y] = v || size <= 2;
                }
            }
            return s;
        }

        /// <summary>The marked area of a mask as stamp (cut to its bounds), or null.</summary>
        public static bool[,] ShapeFromMask(bool[] mask, int w, int h)
        {
            int minX = w, minY = h, maxX = -1, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (mask[y * w + x])
                    {
                        minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
                    }
                }
            }
            if (maxX < 0)
            {
                return null;
            }
            var s = new bool[maxX - minX + 1, maxY - minY + 1];
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    s[x - minX, y - minY] = mask[y * w + x];
                }
            }
            return s;
        }

        public static Bitmap ShapeToBitmap(bool[,] s)
        {
            int w = s.GetLength(0), h = s.GetLength(1);
            var px = new int[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    px[y * w + x] = s[x, y] ? unchecked((int)0xFFFFFFFF) : unchecked((int)0xFF000000);
                }
            }
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            CutterImaging.SetPixels(bmp, px);
            return bmp;
        }

        public static bool[,] ShapeFromBitmap(Bitmap bmp)
        {
            int[] px = CutterImaging.GetPixels(bmp);
            var s = new bool[bmp.Width, bmp.Height];
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    int p = px[y * bmp.Width + x];
                    s[x, y] = ((p >> 24) & 0xFF) >= 128 && (((p >> 16) & 0xFF) + ((p >> 8) & 0xFF) + (p & 0xFF)) > 384;
                }
            }
            return s;
        }

        /// <summary>Own stamps saved as PNG (white = marked) in the stamp folder.</summary>
        public static List<(string Name, bool[,] Shape, Bitmap Preview)> LoadStamps()
        {
            var list = new List<(string, bool[,], Bitmap)>();
            if (!Directory.Exists(StampFolder))
            {
                return list;
            }
            foreach (string file in Directory.GetFiles(StampFolder, "*.png").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    Bitmap bmp = CutterImaging.LoadBitmap(file);
                    list.Add((Path.GetFileNameWithoutExtension(file), ShapeFromBitmap(bmp), bmp));
                }
                catch (Exception)
                {
                    // unreadable file
                }
            }
            return list;
        }

        public static string SaveStamp(bool[,] shape, string name)
        {
            Directory.CreateDirectory(StampFolder);
            string safe = string.Concat((string.IsNullOrWhiteSpace(name) ? "Stamp" : name.Trim()).Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
            string file = Path.Combine(StampFolder, safe + ".png");
            int n = 2;
            while (File.Exists(file))
            {
                file = Path.Combine(StampFolder, $"{safe}_{n++}.png");
            }
            using (Bitmap bmp = ShapeToBitmap(shape))
            {
                bmp.Save(file, ImageFormat.Png);
            }
            return file;
        }

        #endregion
    }
}
