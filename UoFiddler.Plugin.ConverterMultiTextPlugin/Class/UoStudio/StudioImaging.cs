/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio
{
    /// <summary>
    /// Isometric projection modes of the pixel art converter.
    /// </summary>
    public enum IsoMode
    {
        None = 0,
        Floor = 1,
        WallLeft = 2,
        WallRight = 3
    }

    /// <summary>
    /// Shared image helpers for the UO Studio tools (pixel art converter, animation studio,
    /// equipment generator and spell/effect editor). All functions work on 32bpp ARGB bitmaps
    /// and always return new bitmaps (the input is never modified).
    /// </summary>
    public static class StudioImaging
    {
        #region Pixel access

        /// <summary>Copies a bitmap into a plain ARGB int array.</summary>
        public static int[] GetPixels(Bitmap bmp)
        {
            using Bitmap src = To32(bmp);
            var rect = new Rectangle(0, 0, src.Width, src.Height);
            BitmapData bd = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var data = new int[src.Width * src.Height];
            for (int y = 0; y < src.Height; y++)
            {
                Marshal.Copy(bd.Scan0 + (y * bd.Stride), data, y * src.Width, src.Width);
            }
            src.UnlockBits(bd);
            return data;
        }

        /// <summary>Creates a 32bpp ARGB bitmap from a pixel array.</summary>
        public static Bitmap FromPixels(int[] data, int width, int height)
        {
            var bmp = new Bitmap(Math.Max(1, width), Math.Max(1, height), PixelFormat.Format32bppArgb);
            if (width <= 0 || height <= 0)
            {
                return bmp;
            }
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(data, y * width, bd.Scan0 + (y * bd.Stride), width);
            }
            bmp.UnlockBits(bd);
            return bmp;
        }

        /// <summary>Returns a 32bpp ARGB copy of any image.</summary>
        public static Bitmap To32(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            }
            return bmp;
        }

        /// <summary>Loads an image file without locking it on disk.</summary>
        public static Bitmap LoadImage(string fileName)
        {
            byte[] bytes = File.ReadAllBytes(fileName);
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            return To32(img);
        }

        /// <summary>Loads all frames of an (animated) GIF, or a single frame for other formats.</summary>
        public static List<Bitmap> LoadFrames(string fileName)
        {
            var list = new List<Bitmap>();
            byte[] bytes = File.ReadAllBytes(fileName);
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            int count = 1;
            FrameDimension dim = null;
            try
            {
                if (img.FrameDimensionsList.Length > 0)
                {
                    dim = new FrameDimension(img.FrameDimensionsList[0]);
                    count = img.GetFrameCount(dim);
                }
            }
            catch
            {
                count = 1;
            }

            for (int i = 0; i < count; i++)
            {
                if (dim != null && count > 1)
                {
                    img.SelectActiveFrame(dim, i);
                }
                list.Add(To32(img));
            }
            return list;
        }

        #endregion

        #region Geometry

        /// <summary>Scales the image so that it fits into maxW x maxH (keeps aspect ratio, never upscales unless allowUpscale).</summary>
        public static Bitmap ResizeFit(Bitmap src, int maxW, int maxH, InterpolationMode mode, bool allowUpscale = false)
        {
            double f = Math.Min((double)maxW / src.Width, (double)maxH / src.Height);
            if (!allowUpscale)
            {
                f = Math.Min(1.0, f);
            }
            int w = Math.Max(1, (int)Math.Round(src.Width * f));
            int h = Math.Max(1, (int)Math.Round(src.Height * f));
            return Resize(src, w, h, mode);
        }

        public static Bitmap Resize(Bitmap src, int w, int h, InterpolationMode mode)
        {
            var bmp = new Bitmap(Math.Max(1, w), Math.Max(1, h), PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.InterpolationMode = mode;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.CompositingMode = CompositingMode.SourceCopy;
            using var attr = new ImageAttributes();
            attr.SetWrapMode(WrapMode.TileFlipXY);
            g.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, attr);
            return bmp;
        }

        /// <summary>Crops away fully transparent borders.</summary>
        public static Bitmap AutoCrop(Bitmap src, int padding = 0)
        {
            int[] p = GetPixels(src);
            int w = src.Width, h = src.Height;
            int minX = w, minY = h, maxX = -1, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (((p[(y * w) + x] >> 24) & 0xFF) > 0)
                    {
                        if (x < minX) { minX = x; }
                        if (y < minY) { minY = y; }
                        if (x > maxX) { maxX = x; }
                        if (y > maxY) { maxY = y; }
                    }
                }
            }
            if (maxX < 0)
            {
                return new Bitmap(src);
            }
            minX = Math.Max(0, minX - padding);
            minY = Math.Max(0, minY - padding);
            maxX = Math.Min(w - 1, maxX + padding);
            maxY = Math.Min(h - 1, maxY + padding);
            return Crop(src, new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1));
        }

        public static Bitmap Crop(Bitmap src, Rectangle r)
        {
            r.Intersect(new Rectangle(0, 0, src.Width, src.Height));
            if (r.Width <= 0 || r.Height <= 0)
            {
                return new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            }
            var bmp = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImage(src, new Rectangle(0, 0, r.Width, r.Height), r, GraphicsUnit.Pixel);
            return bmp;
        }

        public static Bitmap FlipHorizontal(Bitmap src)
        {
            var bmp = new Bitmap(src);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return bmp;
        }

        /// <summary>Splits a sprite sheet into rows x cols cells (row major).</summary>
        public static List<List<Bitmap>> SliceSheet(Bitmap sheet, int cols, int rows, bool autoCrop)
        {
            var result = new List<List<Bitmap>>();
            int cw = sheet.Width / Math.Max(1, cols);
            int ch = sheet.Height / Math.Max(1, rows);
            for (int r = 0; r < rows; r++)
            {
                var row = new List<Bitmap>();
                for (int c = 0; c < cols; c++)
                {
                    Bitmap cell = Crop(sheet, new Rectangle(c * cw, r * ch, cw, ch));
                    if (IsEmpty(cell))
                    {
                        cell.Dispose();
                        continue;
                    }
                    if (autoCrop)
                    {
                        Bitmap cropped = AutoCrop(cell);
                        cell.Dispose();
                        cell = cropped;
                    }
                    row.Add(cell);
                }
                result.Add(row);
            }
            return result;
        }

        public static bool IsEmpty(Bitmap bmp)
        {
            int[] p = GetPixels(bmp);
            for (int i = 0; i < p.Length; i++)
            {
                if (((p[i] >> 24) & 0xFF) > 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Projects the image isometrically the UO way (45 degree): as floor diamond (width x width,
        /// like a 44x44 land tile), or as left/right wall (columns are sheared 1:1 like UO walls).
        /// </summary>
        public static Bitmap IsoProject(Bitmap src, IsoMode mode, int width)
        {
            if (mode == IsoMode.None)
            {
                return new Bitmap(src);
            }

            width = Math.Max(4, width);
            int[] sp = GetPixels(src);
            int sw = src.Width, sh = src.Height;

            if (mode == IsoMode.Floor)
            {
                int w = width, h = width;
                var dp = new int[w * h];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        double fx = (x + 0.5 - (w / 2.0)) / w;
                        double fy = (y + 0.5 - (h / 2.0)) / h;
                        double u = fx + fy + 0.5;
                        double v = fy - fx + 0.5;
                        if (u < 0 || u >= 1 || v < 0 || v >= 1)
                        {
                            continue;
                        }
                        int sx = Math.Min(sw - 1, (int)(u * sw));
                        int sy = Math.Min(sh - 1, (int)(v * sh));
                        dp[(y * w) + x] = sp[(sy * sw) + sx];
                    }
                }
                return FromPixels(dp, w, h);
            }
            else
            {
                // wall: scale the source to the requested wall width, then shear
                int ww = width / 2; // a wall covers half a tile horizontally (22 px for a 44 px tile)
                int wh = Math.Max(1, (int)Math.Round(sh * (double)ww / sw));
                using Bitmap scaled = Resize(src, ww, wh, InterpolationMode.HighQualityBicubic);
                int[] p = GetPixels(scaled);
                int h = wh + ww;
                var dp = new int[ww * h];
                for (int x = 0; x < ww; x++)
                {
                    int shift = mode == IsoMode.WallLeft ? x : ww - 1 - x;
                    for (int y = 0; y < wh; y++)
                    {
                        int ty = y + shift;
                        if (ty >= 0 && ty < h)
                        {
                            dp[(ty * ww) + x] = p[(y * ww) + x];
                        }
                    }
                }
                return FromPixels(dp, ww, h);
            }
        }

        #endregion

        #region Color adjustments

        /// <summary>Makes all pixels similar to the key color transparent.</summary>
        public static Bitmap RemoveBackground(Bitmap src, Color key, int tolerance)
        {
            int[] p = GetPixels(src);
            int t2 = tolerance * tolerance * 3;
            for (int i = 0; i < p.Length; i++)
            {
                int c = p[i];
                int dr = ((c >> 16) & 0xFF) - key.R;
                int dg = ((c >> 8) & 0xFF) - key.G;
                int db = (c & 0xFF) - key.B;
                if ((dr * dr) + (dg * dg) + (db * db) <= t2)
                {
                    p[i] = 0;
                }
            }
            return FromPixels(p, src.Width, src.Height);
        }

        /// <summary>Flood-fills transparency from the image borders (removes only the connected background).</summary>
        public static Bitmap RemoveBackgroundFromEdges(Bitmap src, Color key, int tolerance)
        {
            int[] p = GetPixels(src);
            int w = src.Width, h = src.Height;
            int t2 = tolerance * tolerance * 3;
            var visited = new bool[p.Length];
            var stack = new Stack<int>();

            bool Match(int c)
            {
                if (((c >> 24) & 0xFF) == 0)
                {
                    return true;
                }
                int dr = ((c >> 16) & 0xFF) - key.R;
                int dg = ((c >> 8) & 0xFF) - key.G;
                int db = (c & 0xFF) - key.B;
                return (dr * dr) + (dg * dg) + (db * db) <= t2;
            }

            for (int x = 0; x < w; x++)
            {
                stack.Push(x);
                stack.Push(((h - 1) * w) + x);
            }
            for (int y = 0; y < h; y++)
            {
                stack.Push(y * w);
                stack.Push((y * w) + w - 1);
            }

            while (stack.Count > 0)
            {
                int i = stack.Pop();
                if (i < 0 || i >= p.Length || visited[i])
                {
                    continue;
                }
                visited[i] = true;
                if (!Match(p[i]))
                {
                    continue;
                }
                p[i] = 0;
                int x = i % w, y = i / w;
                if (x > 0) { stack.Push(i - 1); }
                if (x < w - 1) { stack.Push(i + 1); }
                if (y > 0) { stack.Push(i - w); }
                if (y < h - 1) { stack.Push(i + w); }
            }
            return FromPixels(p, w, h);
        }

        /// <summary>Brightness (-100..100), contrast (-100..100) and saturation (0..200 %).</summary>
        public static Bitmap AdjustColors(Bitmap src, int brightness, int contrast, int saturation)
        {
            int[] p = GetPixels(src);
            double c = (100.0 + contrast) / 100.0;
            c *= c;
            double s = saturation / 100.0;
            for (int i = 0; i < p.Length; i++)
            {
                int a = (p[i] >> 24) & 0xFF;
                if (a == 0)
                {
                    continue;
                }
                double r = (p[i] >> 16) & 0xFF;
                double g = (p[i] >> 8) & 0xFF;
                double b = p[i] & 0xFF;

                r += brightness * 2.55;
                g += brightness * 2.55;
                b += brightness * 2.55;

                r = (((r / 255.0) - 0.5) * c + 0.5) * 255.0;
                g = (((g / 255.0) - 0.5) * c + 0.5) * 255.0;
                b = (((b / 255.0) - 0.5) * c + 0.5) * 255.0;

                double l = (0.299 * r) + (0.587 * g) + (0.114 * b);
                r = l + ((r - l) * s);
                g = l + ((g - l) * s);
                b = l + ((b - l) * s);

                p[i] = (a << 24) | (Clamp(r) << 16) | (Clamp(g) << 8) | Clamp(b);
            }
            return FromPixels(p, src.Width, src.Height);
        }

        /// <summary>Converts to gray scale (needed so that UO hues can be applied completely).</summary>
        public static Bitmap ToGray(Bitmap src)
        {
            int[] p = GetPixels(src);
            for (int i = 0; i < p.Length; i++)
            {
                int a = (p[i] >> 24) & 0xFF;
                if (a == 0)
                {
                    continue;
                }
                int l = Clamp((0.299 * ((p[i] >> 16) & 0xFF)) + (0.587 * ((p[i] >> 8) & 0xFF)) + (0.114 * (p[i] & 0xFF)));
                p[i] = (a << 24) | (l << 16) | (l << 8) | l;
            }
            return FromPixels(p, src.Width, src.Height);
        }

        /// <summary>
        /// Converts black (or dark) pixels into transparency for additive particle effects.
        /// Pixels darker than the threshold become transparent.
        /// </summary>
        public static Bitmap BlackToTransparent(Bitmap src, int threshold)
        {
            int[] p = GetPixels(src);
            for (int i = 0; i < p.Length; i++)
            {
                int r = (p[i] >> 16) & 0xFF, g = (p[i] >> 8) & 0xFF, b = p[i] & 0xFF;
                if (Math.Max(r, Math.Max(g, b)) <= threshold)
                {
                    p[i] = 0;
                }
            }
            return FromPixels(p, src.Width, src.Height);
        }

        /// <summary>Hard alpha (UO has only fully transparent or opaque pixels).</summary>
        public static Bitmap HardAlpha(Bitmap src, int threshold)
        {
            int[] p = GetPixels(src);
            for (int i = 0; i < p.Length; i++)
            {
                int a = (p[i] >> 24) & 0xFF;
                p[i] = a < threshold ? 0 : (p[i] | unchecked((int)0xFF000000));
            }
            return FromPixels(p, src.Width, src.Height);
        }

        /// <summary>
        /// Adds an outline. outer = 1 px line around the shape; otherwise the edge pixels of the
        /// shape itself are darkened (inner outline, keeps the size).
        /// </summary>
        public static Bitmap Outline(Bitmap src, Color color, bool outer, bool diagonal)
        {
            int[] p = GetPixels(src);
            int w = src.Width, h = src.Height;
            if (outer)
            {
                w += 2;
                h += 2;
                var np = new int[w * h];
                for (int y = 0; y < src.Height; y++)
                {
                    Array.Copy(p, y * src.Width, np, ((y + 1) * w) + 1, src.Width);
                }
                p = np;
            }

            var result = (int[])p.Clone();
            int col = color.ToArgb() | unchecked((int)0xFF000000);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool opaque = ((p[(y * w) + x] >> 24) & 0xFF) > 0;
                    if (outer == opaque)
                    {
                        continue;
                    }

                    bool edge = false;
                    for (int dy = -1; dy <= 1 && !edge; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if ((dx == 0 && dy == 0) || (!diagonal && dx != 0 && dy != 0))
                            {
                                continue;
                            }
                            int nx = x + dx, ny = y + dy;
                            bool nOpaque = nx >= 0 && ny >= 0 && nx < w && ny < h && ((p[(ny * w) + nx] >> 24) & 0xFF) > 0;
                            if (nOpaque == outer)
                            {
                                edge = true;
                                break;
                            }
                        }
                    }

                    if (edge)
                    {
                        if (outer)
                        {
                            result[(y * w) + x] = col;
                        }
                        else
                        {
                            // inner: blend 60 % outline colour into the edge pixel
                            int c = p[(y * w) + x];
                            int r = (int)((((c >> 16) & 0xFF) * 0.4) + (color.R * 0.6));
                            int g = (int)((((c >> 8) & 0xFF) * 0.4) + (color.G * 0.6));
                            int b = (int)(((c & 0xFF) * 0.4) + (color.B * 0.6));
                            result[(y * w) + x] = unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
                        }
                    }
                }
            }
            return FromPixels(result, w, h);
        }

        /// <summary>Simple 3x3 sharpen filter (only RGB, alpha untouched).</summary>
        public static Bitmap Sharpen(Bitmap src, double amount)
        {
            int[] p = GetPixels(src);
            int w = src.Width, h = src.Height;
            var r = (int[])p.Clone();
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int i = (y * w) + x;
                    int a = (p[i] >> 24) & 0xFF;
                    if (a == 0)
                    {
                        continue;
                    }
                    double[] acc = new double[3];
                    for (int ch = 0; ch < 3; ch++)
                    {
                        int sh = 16 - (ch * 8);
                        double center = (p[i] >> sh) & 0xFF;
                        double n = ((p[i - 1] >> sh) & 0xFF) + ((p[i + 1] >> sh) & 0xFF) + ((p[i - w] >> sh) & 0xFF) + ((p[i + w] >> sh) & 0xFF);
                        acc[ch] = center + (amount * ((4 * center) - n));
                    }
                    r[i] = (a << 24) | (Clamp(acc[0]) << 16) | (Clamp(acc[1]) << 8) | Clamp(acc[2]);
                }
            }
            return FromPixels(r, w, h);
        }

        #endregion

        #region Palette reduction (median cut + optional Floyd-Steinberg)

        /// <summary>Builds an optimized palette (median cut) of at most maxColors colors from the opaque pixels.</summary>
        public static List<Color> BuildPalette(IEnumerable<Bitmap> images, int maxColors)
        {
            var hist = new Dictionary<int, int>();
            foreach (Bitmap img in images)
            {
                int[] p = GetPixels(img);
                foreach (int c in p)
                {
                    if (((c >> 24) & 0xFF) < 128)
                    {
                        continue;
                    }
                    // pre quantise to 15 bit (UO colour depth)
                    int key = (((c >> 19) & 0x1F) << 10) | (((c >> 11) & 0x1F) << 5) | ((c >> 3) & 0x1F);
                    hist.TryGetValue(key, out int n);
                    hist[key] = n + 1;
                }
            }

            var colors = hist.Select(kv => (Key: kv.Key, Count: kv.Value)).ToList();
            var palette = new List<Color>();
            if (colors.Count == 0)
            {
                return palette;
            }

            if (colors.Count <= maxColors)
            {
                palette.AddRange(colors.Select(c => From15(c.Key)));
                return palette;
            }

            var boxes = new List<List<(int Key, int Count)>> { colors };
            while (boxes.Count < maxColors)
            {
                // split the box with the biggest range * population
                int best = -1;
                long bestScore = -1;
                int bestChannel = 0;
                for (int b = 0; b < boxes.Count; b++)
                {
                    if (boxes[b].Count < 2)
                    {
                        continue;
                    }
                    GetRange(boxes[b], out int ch, out int range);
                    long pop = boxes[b].Sum(x => (long)x.Count);
                    long score = range * (long)Math.Sqrt(pop);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = b;
                        bestChannel = ch;
                    }
                }
                if (best < 0)
                {
                    break;
                }

                var box = boxes[best];
                int shift = 10 - (bestChannel * 5);
                box.Sort((a, c) => ((a.Key >> shift) & 0x1F).CompareTo((c.Key >> shift) & 0x1F));
                long half = box.Sum(x => (long)x.Count) / 2;
                long acc = 0;
                int split = 1;
                for (int i = 0; i < box.Count - 1; i++)
                {
                    acc += box[i].Count;
                    if (acc >= half)
                    {
                        split = i + 1;
                        break;
                    }
                }
                boxes[best] = box.GetRange(0, split);
                boxes.Add(box.GetRange(split, box.Count - split));
            }

            foreach (var box in boxes)
            {
                long tot = 0, r = 0, g = 0, bl = 0;
                foreach (var (key, count) in box)
                {
                    tot += count;
                    r += ((key >> 10) & 0x1F) * (long)count;
                    g += ((key >> 5) & 0x1F) * (long)count;
                    bl += (key & 0x1F) * (long)count;
                }
                if (tot == 0)
                {
                    continue;
                }
                palette.Add(Color.FromArgb(Expand5((int)(r / tot)), Expand5((int)(g / tot)), Expand5((int)(bl / tot))));
            }
            return palette;
        }

        private static void GetRange(List<(int Key, int Count)> box, out int channel, out int range)
        {
            int[] min = { 31, 31, 31 }, max = { 0, 0, 0 };
            foreach (var (key, _) in box)
            {
                for (int ch = 0; ch < 3; ch++)
                {
                    int v = (key >> (10 - (ch * 5))) & 0x1F;
                    if (v < min[ch]) { min[ch] = v; }
                    if (v > max[ch]) { max[ch] = v; }
                }
            }
            channel = 0;
            range = -1;
            for (int ch = 0; ch < 3; ch++)
            {
                if (max[ch] - min[ch] > range)
                {
                    range = max[ch] - min[ch];
                    channel = ch;
                }
            }
        }

        /// <summary>Maps every opaque pixel to the closest palette entry (optional Floyd-Steinberg dithering).</summary>
        public static Bitmap ApplyPalette(Bitmap src, IReadOnlyList<Color> palette, bool dither)
        {
            int[] p = GetPixels(src);
            int w = src.Width, h = src.Height;
            if (palette == null || palette.Count == 0)
            {
                return FromPixels(p, w, h);
            }

            var cache = new Dictionary<int, int>();
            double[] er = new double[p.Length], eg = new double[p.Length], eb = new double[p.Length];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = (y * w) + x;
                    int c = p[i];
                    if (((c >> 24) & 0xFF) < 128)
                    {
                        p[i] = 0;
                        continue;
                    }
                    int r = Clamp(((c >> 16) & 0xFF) + er[i]);
                    int g = Clamp(((c >> 8) & 0xFF) + eg[i]);
                    int b = Clamp((c & 0xFF) + eb[i]);
                    int key = (r << 16) | (g << 8) | b;
                    if (!cache.TryGetValue(key, out int idx))
                    {
                        idx = Nearest(palette, r, g, b);
                        cache[key] = idx;
                    }
                    Color pc = palette[idx];
                    p[i] = unchecked((int)0xFF000000) | (pc.R << 16) | (pc.G << 8) | pc.B;

                    if (!dither)
                    {
                        continue;
                    }
                    double dr = r - pc.R, dg = g - pc.G, db = b - pc.B;
                    Spread(x + 1, y, 7 / 16.0);
                    Spread(x - 1, y + 1, 3 / 16.0);
                    Spread(x, y + 1, 5 / 16.0);
                    Spread(x + 1, y + 1, 1 / 16.0);

                    void Spread(int sx, int sy, double f)
                    {
                        if (sx < 0 || sy < 0 || sx >= w || sy >= h)
                        {
                            return;
                        }
                        int j = (sy * w) + sx;
                        er[j] += dr * f;
                        eg[j] += dg * f;
                        eb[j] += db * f;
                    }
                }
            }
            return FromPixels(p, w, h);
        }

        private static int Nearest(IReadOnlyList<Color> palette, int r, int g, int b)
        {
            int best = 0;
            int bestD = int.MaxValue;
            for (int i = 0; i < palette.Count; i++)
            {
                int dr = palette[i].R - r, dg = palette[i].G - g, db = palette[i].B - b;
                // weighted distance (human perception)
                int d = (dr * dr * 3) + (dg * dg * 4) + (db * db * 2);
                if (d < bestD)
                {
                    bestD = d;
                    best = i;
                }
            }
            return best;
        }

        #endregion

        #region UO specific

        /// <summary>
        /// Snaps all colours to UO 15 bit, makes alpha binary and replaces pure black by
        /// the darkest non transparent colour (0x0000 is transparent in UO files).
        /// </summary>
        public static Bitmap ToUoColors(Bitmap src)
        {
            int[] p = GetPixels(src);
            for (int i = 0; i < p.Length; i++)
            {
                int c = p[i];
                if (((c >> 24) & 0xFF) < 128)
                {
                    p[i] = 0;
                    continue;
                }
                int r = (c >> 19) & 0x1F, g = (c >> 11) & 0x1F, b = (c >> 3) & 0x1F;
                if (r == 0 && g == 0 && b == 0)
                {
                    r = g = b = 1;
                }
                p[i] = unchecked((int)0xFF000000) | (Expand5(r) << 16) | (Expand5(g) << 8) | Expand5(b);
            }
            return FromPixels(p, src.Width, src.Height);
        }

        /// <summary>Converts a 32bpp image into the 16bpp ARGB1555 format used by the Ultima SDK caches.</summary>
        public static unsafe Bitmap To1555(Bitmap src)
        {
            using Bitmap uo = ToUoColors(src);
            int[] p = GetPixels(uo);
            int w = uo.Width, h = uo.Height;
            var bmp = new Bitmap(w, h, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            for (int y = 0; y < h; y++)
            {
                ushort* line = (ushort*)(bd.Scan0 + (y * bd.Stride));
                for (int x = 0; x < w; x++)
                {
                    int c = p[(y * w) + x];
                    if (((c >> 24) & 0xFF) == 0)
                    {
                        line[x] = 0;
                        continue;
                    }
                    line[x] = (ushort)(0x8000 | (((c >> 19) & 0x1F) << 10) | (((c >> 11) & 0x1F) << 5) | ((c >> 3) & 0x1F));
                }
            }
            bmp.UnlockBits(bd);
            return bmp;
        }

        /// <summary>
        /// Applies a UO hue like the client does: the red channel (gray value) of a pixel selects one
        /// of the 32 hue colours. grayOnly = only pixels with R == G == B are hued (partial hue).
        /// </summary>
        public static Bitmap ApplyHue(Bitmap src, Ultima.Hue hue, bool grayOnly)
        {
            if (hue == null)
            {
                return new Bitmap(src);
            }
            int[] p = GetPixels(src);
            for (int i = 0; i < p.Length; i++)
            {
                int c = p[i];
                int a = (c >> 24) & 0xFF;
                if (a == 0)
                {
                    continue;
                }
                int r = (c >> 16) & 0xFF, g = (c >> 8) & 0xFF, b = c & 0xFF;
                if (grayOnly && !(Math.Abs(r - g) < 9 && Math.Abs(g - b) < 9))
                {
                    continue;
                }
                int idx = grayOnly ? r >> 3 : ((r + g + b) / 3) >> 3;
                ushort hc = hue.Colors[Math.Min(31, idx)];
                p[i] = (a << 24) | (Expand5((hc >> 10) & 0x1F) << 16) | (Expand5((hc >> 5) & 0x1F) << 8) | Expand5(hc & 0x1F);
            }
            return FromPixels(p, src.Width, src.Height);
        }

        /// <summary>Draws a checker board (transparency background) into the graphics.</summary>
        public static void DrawChecker(Graphics g, Rectangle r, int size = 8)
        {
            using var light = new SolidBrush(Color.FromArgb(235, 235, 235));
            using var dark = new SolidBrush(Color.FromArgb(200, 200, 200));
            g.FillRectangle(light, r);
            for (int y = r.Top; y < r.Bottom; y += size)
            {
                for (int x = r.Left; x < r.Right; x += size)
                {
                    if ((((x - r.Left) / size) + ((y - r.Top) / size)) % 2 == 0)
                    {
                        g.FillRectangle(dark, x, y, Math.Min(size, r.Right - x), Math.Min(size, r.Bottom - y));
                    }
                }
            }
        }

        public static int CountColors(Bitmap bmp)
        {
            var set = new HashSet<int>();
            foreach (int c in GetPixels(bmp))
            {
                if (((c >> 24) & 0xFF) > 0)
                {
                    set.Add(c & 0xFFFFFF);
                }
            }
            return set.Count;
        }

        public static Color From15(int c) => Color.FromArgb(Expand5((c >> 10) & 0x1F), Expand5((c >> 5) & 0x1F), Expand5(c & 0x1F));

        public static int Expand5(int v) => (v << 3) | (v >> 2);

        public static int Clamp(double v) => v < 0 ? 0 : v > 255 ? 255 : (int)v;

        #endregion
    }
}
