using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter
{
    /// <summary>Settings of the live color adjustment (all 0 = no change).</summary>
    internal struct ColorAdjust
    {
        public int Brightness;  // -100 .. 100
        public int Contrast;    // -100 .. 100
        public int Gamma;       // -90 .. 300  (gamma = 1 + value / 100)
        public int Saturation;  // -100 .. 100
        public int Hue;         // -180 .. 180 degrees
        public bool Grayscale;

        public bool IsNeutral => Brightness == 0 && Contrast == 0 && Gamma == 0 && Saturation == 0 && Hue == 0 && !Grayscale;
    }

    /// <summary>A named color mode (old buttons R1 / R2).</summary>
    internal sealed class ColorMode
    {
        public ColorMode(string name, Func<int, int, int, int> map)
        {
            Name = name;
            Map = map;
        }

        public string Name { get; }

        /// <summary>(r, g, b) → packed RGB (0xRRGGBB).</summary>
        public Func<int, int, int, int> Map { get; }

        public override string ToString() => Name;
    }

    /// <summary>Image filters of the Texture Cutter. Every method works on 32bpp ARGB bitmaps.</summary>
    internal static class TextureImaging
    {
        public const int LandTileSize = 44;

        private static int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

        private static int Clamp(double v) => v < 0 ? 0 : v > 255 ? 255 : (int)(v + 0.5);

        private static int Pack(int a, int r, int g, int b) => (a << 24) | (r << 16) | (g << 8) | b;

        /// <summary>Pure black and pure white are left alone when "protect" is on (UO transparency colors).</summary>
        private static bool IsProtected(int p) => (p & 0xFFFFFF) == 0 || (p & 0xFFFFFF) == 0xFFFFFF;

        /// <summary>Runs a function over every pixel and returns a new bitmap.</summary>
        public static Bitmap MapPixels(Bitmap source, bool protectBlackWhite, Func<int, int> map)
        {
            Bitmap result = CutterImaging.To32Bpp(source);
            int[] px = CutterImaging.GetPixels(result);
            for (int i = 0; i < px.Length; i++)
            {
                int p = px[i];
                if ((p >> 24) == 0 || (protectBlackWhite && IsProtected(p)))
                {
                    continue;
                }
                px[i] = map(p);
            }
            CutterImaging.SetPixels(result, px);
            return result;
        }

        #region HSL

        public static void RgbToHsl(int r, int g, int b, out double h, out double s, out double l)
        {
            double rf = r / 255.0, gf = g / 255.0, bf = b / 255.0;
            double max = Math.Max(rf, Math.Max(gf, bf));
            double min = Math.Min(rf, Math.Min(gf, bf));
            l = (max + min) / 2;
            if (max == min)
            {
                h = s = 0;
                return;
            }
            double d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            if (max == rf)
            {
                h = (gf - bf) / d + (gf < bf ? 6 : 0);
            }
            else if (max == gf)
            {
                h = (bf - rf) / d + 2;
            }
            else
            {
                h = (rf - gf) / d + 4;
            }
            h /= 6;
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1 / 2.0) return q;
            if (t < 2 / 3.0) return p + (q - p) * (2 / 3.0 - t) * 6;
            return p;
        }

        public static void HslToRgb(double h, double s, double l, out int r, out int g, out int b)
        {
            h -= Math.Floor(h);
            if (s <= 0)
            {
                r = g = b = Clamp(l * 255);
                return;
            }
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = Clamp(HueToRgb(p, q, h + 1 / 3.0) * 255);
            g = Clamp(HueToRgb(p, q, h) * 255);
            b = Clamp(HueToRgb(p, q, h - 1 / 3.0) * 255);
        }

        #endregion

        #region Color adjustments

        /// <summary>Brightness, contrast, gamma, saturation, hue and grayscale in one pass.</summary>
        public static Bitmap Adjust(Bitmap source, ColorAdjust a, bool protectBlackWhite)
        {
            double contrast = (100.0 + a.Contrast) / 100.0;
            contrast *= contrast;
            double gamma = 1.0 + a.Gamma / 100.0;
            double invGamma = gamma <= 0.05 ? 20 : 1.0 / gamma;
            double sat = 1.0 + a.Saturation / 100.0;
            double hueShift = a.Hue / 360.0;
            int brightness = (int)Math.Round(a.Brightness * 2.55);

            // Brightness / contrast / gamma do not depend on the other channels: look-up table.
            var lut = new int[256];
            for (int i = 0; i < 256; i++)
            {
                double v = i + brightness;
                v = ((v / 255.0 - 0.5) * contrast + 0.5) * 255.0;
                v = Math.Max(0, Math.Min(255, v));
                v = 255.0 * Math.Pow(v / 255.0, invGamma);
                lut[i] = Clamp(v);
            }

            bool hsl = a.Saturation != 0 || a.Hue != 0;
            return MapPixels(source, protectBlackWhite, p =>
            {
                int alpha = (p >> 24) & 0xFF;
                int r = (p >> 16) & 0xFF, g = (p >> 8) & 0xFF, b = p & 0xFF;
                if (hsl)
                {
                    RgbToHsl(r, g, b, out double h, out double s, out double l);
                    HslToRgb(h + hueShift, Math.Max(0, Math.Min(1, s * sat)), l, out r, out g, out b);
                }
                r = lut[r];
                g = lut[g];
                b = lut[b];
                if (a.Grayscale)
                {
                    int gray = Clamp(r * 0.3 + g * 0.59 + b * 0.11);
                    r = g = b = gray;
                }
                return Pack(alpha, r, g, b);
            });
        }

        public static Bitmap Invert(Bitmap source, bool protect) =>
            MapPixels(source, protect, p => (p & unchecked((int)0xFF000000)) | (~p & 0xFFFFFF));

        /// <summary>Old "color enhancement": more saturation (the old version only made the image brighter).</summary>
        public static Bitmap AutoEnhance(Bitmap source, bool protect) =>
            Adjust(source, new ColorAdjust { Saturation = 20, Contrast = 5 }, protect);

        /// <summary>Stretches every channel to the full range 0..255 (old "Color Normalization").</summary>
        public static Bitmap Normalize(Bitmap source, bool protect)
        {
            int[] px = CutterImaging.GetPixels(source);
            int minR = 255, minG = 255, minB = 255, maxR = 0, maxG = 0, maxB = 0;
            foreach (int p in px)
            {
                if ((p >> 24) == 0 || (protect && IsProtected(p)))
                {
                    continue;
                }
                int r = (p >> 16) & 0xFF, g = (p >> 8) & 0xFF, b = p & 0xFF;
                minR = Math.Min(minR, r); maxR = Math.Max(maxR, r);
                minG = Math.Min(minG, g); maxG = Math.Max(maxG, g);
                minB = Math.Min(minB, b); maxB = Math.Max(maxB, b);
            }
            int Scale(int v, int min, int max) => max <= min ? v : Clamp((v - min) * 255 / (max - min));
            return MapPixels(source, protect, p =>
                Pack((p >> 24) & 0xFF, Scale((p >> 16) & 0xFF, minR, maxR), Scale((p >> 8) & 0xFF, minG, maxG), Scale(p & 0xFF, minB, maxB)));
        }

        /// <summary>
        /// White balance. strength 0..100 %. Lighten: every channel moves towards the brightest value of that channel;
        /// darken: away from it (same as before, now fast and without overflow).
        /// </summary>
        public static Bitmap WhiteBalance(Bitmap source, int strength, bool darken)
        {
            int[] px = CutterImaging.GetPixels(source);
            int maxR = 0, maxG = 0, maxB = 0;
            foreach (int p in px)
            {
                if ((p >> 24) == 0)
                {
                    continue;
                }
                maxR = Math.Max(maxR, (p >> 16) & 0xFF);
                maxG = Math.Max(maxG, (p >> 8) & 0xFF);
                maxB = Math.Max(maxB, p & 0xFF);
            }
            int Move(int v, int max) => darken ? Clamp(v - (max - v) * strength / 100) : Clamp(v + (max - v) * strength / 100);
            return MapPixels(source, true, p =>
                Pack((p >> 24) & 0xFF, Move((p >> 16) & 0xFF, maxR), Move((p >> 8) & 0xFF, maxG), Move(p & 0xFF, maxB)));
        }

        /// <summary>Sharpen with a 3 × 3 kernel. amount 0..100 %. Reads from the unchanged source (the old code read changed pixels).</summary>
        public static Bitmap Sharpen(Bitmap source, int amount, bool protect)
        {
            double k = amount / 100.0;
            int w = source.Width, h = source.Height;
            int[] src = CutterImaging.GetPixels(source);
            var dst = (int[])src.Clone();
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int i = y * w + x;
                    int p = src[i];
                    if ((p >> 24) == 0 || (protect && IsProtected(p)))
                    {
                        continue;
                    }
                    double r = 0, g = 0, b = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int q = src[i + dy * w + dx];
                            double f = dx == 0 && dy == 0 ? 1 + 8 * k : -k;
                            r += ((q >> 16) & 0xFF) * f;
                            g += ((q >> 8) & 0xFF) * f;
                            b += (q & 0xFF) * f;
                        }
                    }
                    dst[i] = Pack((p >> 24) & 0xFF, Clamp(r), Clamp(g), Clamp(b));
                }
            }
            var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            CutterImaging.SetPixels(result, dst);
            return result;
        }

        /// <summary>Reduces the colors to what UO stores (5 bit per channel, ARGB 1555).</summary>
        public static Bitmap To16Bit(Bitmap source) =>
            MapPixels(source, false, p =>
            {
                int a = ((p >> 24) & 0xFF) >= 128 ? 255 : 0;
                int r = ((p >> 16) & 0xFF) >> 3, g = ((p >> 8) & 0xFF) >> 3, b = (p & 0xFF) >> 3;
                return Pack(a, (r << 3) | (r >> 2), (g << 3) | (g >> 2), (b << 3) | (b >> 2));
            });

        /// <summary>Fewer brightness steps per channel (2..64), useful for a painted look or small palettes.</summary>
        public static Bitmap Posterize(Bitmap source, int levels, bool protect)
        {
            levels = Math.Max(2, Math.Min(256, levels));
            double step = 255.0 / (levels - 1);
            int Q(int v) => Clamp(Math.Round(v / step) * step);
            return MapPixels(source, protect, p => Pack((p >> 24) & 0xFF, Q((p >> 16) & 0xFF), Q((p >> 8) & 0xFF), Q(p & 0xFF)));
        }

        #endregion

        #region Replace / erase colors

        private static bool Near(int p, Color c, int tolerance) =>
            Math.Abs(((p >> 16) & 0xFF) - c.R) <= tolerance &&
            Math.Abs(((p >> 8) & 0xFF) - c.G) <= tolerance &&
            Math.Abs((p & 0xFF) - c.B) <= tolerance;

        /// <summary>Replaces a color (tolerance 0..255 per channel). keepShading: the brightness of each pixel is kept.</summary>
        public static Bitmap ReplaceColor(Bitmap source, Color from, Color to, int tolerance, bool keepShading, out int count)
        {
            int n = 0;
            double fromL = (from.R * 0.3 + from.G * 0.59 + from.B * 0.11) / 255.0;
            Bitmap result = MapPixels(source, false, p =>
            {
                if (!Near(p, from, tolerance))
                {
                    return p;
                }
                n++;
                if (!keepShading)
                {
                    return Pack((p >> 24) & 0xFF, to.R, to.G, to.B);
                }
                double l = (((p >> 16) & 0xFF) * 0.3 + ((p >> 8) & 0xFF) * 0.59 + (p & 0xFF) * 0.11) / 255.0;
                double f = fromL <= 0.001 ? 1 : l / fromL;
                return Pack((p >> 24) & 0xFF, Clamp(to.R * f), Clamp(to.G * f), Clamp(to.B * f));
            });
            count = n;
            return result;
        }

        /// <summary>Removes a color: transparent, or white like the old "Delete" field.</summary>
        public static Bitmap EraseColor(Bitmap source, Color color, int tolerance, bool toWhite, out int count)
        {
            int n = 0;
            Bitmap result = MapPixels(source, false, p =>
            {
                if (!Near(p, color, tolerance))
                {
                    return p;
                }
                n++;
                return toWhite ? unchecked((int)0xFFFFFFFF) : 0;
            });
            count = n;
            return result;
        }

        /// <summary>
        /// Particle grey: all gray pixels (#030303 .. #FBFBFB) get the target color.
        /// shaded = the brightness of the gray is kept ("Particle Grey Shadow").
        /// </summary>
        public static Bitmap ParticleGrey(Bitmap source, Color target, bool shaded) =>
            MapPixels(source, false, p =>
            {
                int r = (p >> 16) & 0xFF, g = (p >> 8) & 0xFF, b = p & 0xFF;
                if (r != g || g != b || r < 3 || r > 0xFB)
                {
                    return p;
                }
                if (!shaded)
                {
                    return Pack((p >> 24) & 0xFF, target.R, target.G, target.B);
                }
                double f = r / 255.0;
                return Pack((p >> 24) & 0xFF, Clamp(target.R * f), Clamp(target.G * f), Clamp(target.B * f));
            });

        /// <summary>Colors of the image with their pixel count, most common first.</summary>
        public static List<KeyValuePair<int, int>> CountColors(Bitmap source, bool skipBlackWhite)
        {
            var counts = new Dictionary<int, int>();
            foreach (int p in CutterImaging.GetPixels(source))
            {
                if ((p >> 24) == 0 || (skipBlackWhite && IsProtected(p)))
                {
                    continue;
                }
                int rgb = p & 0xFFFFFF;
                counts.TryGetValue(rgb, out int c);
                counts[rgb] = c + 1;
            }
            return counts.OrderByDescending(k => k.Value).ToList();
        }

        #endregion

        #region Color modes (old R1 / R2)

        private static int Rgb(int r, int g, int b) => (Clamp(r) << 16) | (Clamp(g) << 8) | Clamp(b);

        /// <summary>The color modes of the old buttons R1 and R2, now with names.</summary>
        public static readonly ColorMode[] ColorModes =
        {
            new ColorMode("Red → blue (red parts only)", (r, g, b) => r > g && r > b ? Rgb(b, g, r) : Rgb(r, g, b)),
            new ColorMode("Swap red / green", (r, g, b) => Rgb(g, r, b)),
            new ColorMode("Yellow", (r, g, b) => Rgb(g, g, r)),
            new ColorMode("Brown", (r, g, b) => Rgb(r, g / 2, b / 2)),
            new ColorMode("Orange", (r, g, b) => Rgb(r, g / 2, 0)),
            new ColorMode("Violet", (r, g, b) => Rgb(r / 2, 0, b)),
            new ColorMode("Green", (r, g, b) => Rgb(0, g, 0)),
            new ColorMode("Turquoise", (r, g, b) => Rgb(0, g, b)),
            new ColorMode("Pink", (r, g, b) => Rgb(r, 0, b)),
            new ColorMode("Light blue", (r, g, b) => Rgb(0, g, r)),
            new ColorMode("Dark red", (r, g, b) => Rgb(r / 2, 0, 0)),
            new ColorMode("Purple", (r, g, b) => Rgb(r / 2, 0, b / 2)),
            new ColorMode("Yellow green / olive", (r, g, b) => Rgb(r / 2, g / 2, 0)),
            new ColorMode("Magenta", (r, g, b) => Rgb(r, 0, b / 2)),
            new ColorMode("Cyan", (r, g, b) => Rgb(0, g / 2, b)),
            new ColorMode("Dark green", (r, g, b) => Rgb(0, g / 2, 0)),
            new ColorMode("Turquoise blue", (r, g, b) => Rgb(0, g / 2, b / 2)),
            new ColorMode("Darker (half)", (r, g, b) => Rgb(r / 2, g / 2, b / 2)),
            new ColorMode("Bright red", (r, g, b) => Rgb(r, 0, 0)),
            new ColorMode("Blue only", (r, g, b) => Rgb(0, 0, b)),
            new ColorMode("Dark brown", (r, g, b) => Rgb(r / 2, g / 4, b / 4)),
            new ColorMode("Gray", (r, g, b) => { int v = (r + g + b) / 3; return Rgb(v, v, v); }),
            new ColorMode("Dark gray", (r, g, b) => { int v = Math.Max(0, (r + g + b) / 3 - 64); return Rgb(v, v, v); }),
            new ColorMode("Gold", (r, g, b) => Rgb(r, g / 2, 0)),
            new ColorMode("Bronze", (r, g, b) => Rgb(r / 2, g / 4, 0)),
            new ColorMode("Copper", (r, g, b) => Rgb(r, g / 3, 0)),
            new ColorMode("Channels sorted: max, mid, min", (r, g, b) => Sorted(r, g, b, 0)),
            new ColorMode("Channels sorted: max, min, mid", (r, g, b) => Sorted(r, g, b, 1)),
            new ColorMode("Channels sorted: mid, max, min", (r, g, b) => Sorted(r, g, b, 2)),
            new ColorMode("Channels sorted: mid, min, max", (r, g, b) => Sorted(r, g, b, 3)),
            new ColorMode("Channels sorted: min, max, mid", (r, g, b) => Sorted(r, g, b, 4)),
            new ColorMode("Channels sorted: min, mid, max", (r, g, b) => Sorted(r, g, b, 5)),
            new ColorMode("Magenta (full red / blue)", (r, g, b) => Rgb(255, g, 255)),
            new ColorMode("Swap red / blue", (r, g, b) => Rgb(b, g, r)),
        };

        private static int Sorted(int r, int g, int b, int order)
        {
            int max = Math.Max(r, Math.Max(g, b));
            int min = Math.Min(r, Math.Min(g, b));
            int mid = r + g + b - max - min;
            switch (order)
            {
                case 0: return Rgb(max, mid, min);
                case 1: return Rgb(max, min, mid);
                case 2: return Rgb(mid, max, min);
                case 3: return Rgb(mid, min, max);
                case 4: return Rgb(min, max, mid);
                default: return Rgb(min, mid, max);
            }
        }

        public static Bitmap ApplyColorMode(Bitmap source, ColorMode mode, bool protect) =>
            MapPixels(source, protect, p =>
            {
                int rgb = mode.Map((p >> 16) & 0xFF, (p >> 8) & 0xFF, p & 0xFF);
                return (p & unchecked((int)0xFF000000)) | (rgb & 0xFFFFFF);
            });

        #endregion

        #region Size, tiles, land tiles

        public enum ResizeQuality { Pixel, Smooth, Sharp }

        public static Bitmap Resize(Bitmap source, int width, int height, ResizeQuality quality)
        {
            var result = new Bitmap(Math.Max(1, width), Math.Max(1, height), PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceCopy;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.InterpolationMode = quality == ResizeQuality.Pixel ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                using (var attributes = new ImageAttributes())
                {
                    // Avoids the half transparent border GDI+ creates at the edges.
                    attributes.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(source, new Rectangle(0, 0, result.Width, result.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            if (quality == ResizeQuality.Sharp)
            {
                Bitmap sharp = Sharpen(result, 15, false);
                result.Dispose();
                result = sharp;
            }
            return result;
        }

        public static Bitmap Crop(Bitmap source, Rectangle area)
        {
            area = Rectangle.Intersect(area, new Rectangle(Point.Empty, source.Size));
            if (area.Width <= 0 || area.Height <= 0)
            {
                return null;
            }
            return source.Clone(area, PixelFormat.Format32bppArgb);
        }

        /// <summary>Cuts the image into tiles. Tiles at the right / bottom edge are only full size when padEdges is on.</summary>
        public static List<(Bitmap Tile, Point Position)> CutTiles(Bitmap source, Size tile, Point offset, bool padEdges, bool skipEmpty)
        {
            var tiles = new List<(Bitmap, Point)>();
            for (int y = offset.Y; y < source.Height; y += tile.Height)
            {
                for (int x = offset.X; x < source.Width; x += tile.Width)
                {
                    bool partial = x + tile.Width > source.Width || y + tile.Height > source.Height;
                    if (partial && !padEdges)
                    {
                        continue;
                    }
                    var bmp = new Bitmap(tile.Width, tile.Height, PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(source, new Rectangle(0, 0, tile.Width, tile.Height), new Rectangle(x, y, tile.Width, tile.Height), GraphicsUnit.Pixel);
                    }
                    if (skipEmpty && CutterImaging.IsEmpty(bmp))
                    {
                        bmp.Dispose();
                        continue;
                    }
                    tiles.Add((bmp, new Point(x, y)));
                }
            }
            return tiles;
        }

        /// <summary>True if (x, y) lies inside the 44 × 44 UO land diamond.</summary>
        public static bool InLandDiamond(int x, int y)
        {
            int half = y < 22 ? y + 1 : 44 - y;
            return x >= 22 - half && x < 22 + half;
        }

        private static int Sample(int[] src, int w, int h, double u, double v)
        {
            // Bilinear sampling, u / v in pixels of the source.
            u = Math.Max(0, Math.Min(w - 1.001, u));
            v = Math.Max(0, Math.Min(h - 1.001, v));
            int x0 = (int)u, y0 = (int)v;
            double fx = u - x0, fy = v - y0;
            int p00 = src[y0 * w + x0], p10 = src[y0 * w + x0 + 1], p01 = src[(y0 + 1) * w + x0], p11 = src[(y0 + 1) * w + x0 + 1];
            int Ch(int shift)
            {
                double a = ((p00 >> shift) & 0xFF) * (1 - fx) + ((p10 >> shift) & 0xFF) * fx;
                double b = ((p01 >> shift) & 0xFF) * (1 - fx) + ((p11 >> shift) & 0xFF) * fx;
                return Clamp(a * (1 - fy) + b * fy);
            }
            return Pack(255, Ch(16), Ch(8), Ch(0));
        }

        /// <summary>
        /// Turns a square texture into a 44 × 44 UO land tile (diamond). The top left corner of the
        /// texture becomes the top corner of the diamond. Outside the diamond the tile stays black / transparent.
        /// </summary>
        public static Bitmap TextureToLandTile(Bitmap texture, bool blackOutside)
        {
            int w = texture.Width, h = texture.Height;
            int[] src = CutterImaging.GetPixels(texture);
            var dst = new int[LandTileSize * LandTileSize];
            int outside = blackOutside ? unchecked((int)0xFF000000) : 0;
            for (int y = 0; y < LandTileSize; y++)
            {
                for (int x = 0; x < LandTileSize; x++)
                {
                    if (!InLandDiamond(x, y))
                    {
                        dst[y * LandTileSize + x] = outside;
                        continue;
                    }
                    double cx = x + 0.5 - 22, cy = y + 0.5 - 22;
                    double u = (cx + cy) / 44.0 + 0.5;
                    double v = (cy - cx) / 44.0 + 0.5;
                    dst[y * LandTileSize + x] = Sample(src, w, h, u * w - 0.5, v * h - 0.5);
                }
            }
            var result = new Bitmap(LandTileSize, LandTileSize, PixelFormat.Format32bppArgb);
            CutterImaging.SetPixels(result, dst);
            return result;
        }

        /// <summary>The way back: a 44 × 44 land tile becomes a square texture (64 or 128).</summary>
        public static Bitmap LandTileToTexture(Bitmap tile, int size)
        {
            int[] src = CutterImaging.GetPixels(tile);
            int w = tile.Width, h = tile.Height;
            var dst = new int[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double u = (x + 0.5) / size - 0.5, v = (y + 0.5) / size - 0.5;
                    // inverse of TextureToLandTile
                    double cx = (u - v) * 22, cy = (u + v) * 22;
                    dst[y * size + x] = Sample(src, w, h, cx + 22 - 0.5, cy + 22 - 0.5);
                }
            }
            var result = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            CutterImaging.SetPixels(result, dst);
            return result;
        }

        /// <summary>The old "Create Tiles": 33 × 33, rotated by 45°, centered on a black 44 × 44 tile.</summary>
        public static Bitmap OldRotatedTile(Bitmap source)
        {
            using (Bitmap small = Resize(source, 33, 33, ResizeQuality.Smooth))
            {
                var result = new Bitmap(LandTileSize, LandTileSize, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(result))
                {
                    g.Clear(Color.Black);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.TranslateTransform(22, 22);
                    g.RotateTransform(-45);
                    g.DrawImage(small, -16.5f, -16.5f, 33, 33);
                }
                return result;
            }
        }

        /// <summary>
        /// Makes the edges of a texture fit together when it is repeated: the image is blended with a copy
        /// moved by half its size, the seam fades out towards the middle.
        /// </summary>
        public static Bitmap MakeSeamless(Bitmap source, int blendPercent)
        {
            int w = source.Width, h = source.Height;
            int[] src = CutterImaging.GetPixels(source);
            var dst = new int[src.Length];
            double band = Math.Max(0.05, blendPercent / 100.0) * 0.5;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int p = src[y * w + x];
                    int q = src[((y + h / 2) % h) * w + (x + w / 2) % w];
                    // distance to the nearest edge, 0 at the edge, 0.5 in the middle
                    double dx = Math.Min(x + 0.5, w - x - 0.5) / w;
                    double dy = Math.Min(y + 0.5, h - y - 0.5) / h;
                    double t = Math.Min(1, Math.Min(dx, dy) / band); // 0 at the edge → shifted copy, 1 inside → original
                    int Mix(int shift) => Clamp(((p >> shift) & 0xFF) * t + ((q >> shift) & 0xFF) * (1 - t));
                    dst[y * w + x] = Pack(Math.Max((p >> 24) & 0xFF, (q >> 24) & 0xFF), Mix(16), Mix(8), Mix(0));
                }
            }
            var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            CutterImaging.SetPixels(result, dst);
            return result;
        }

        #endregion
    }
}
