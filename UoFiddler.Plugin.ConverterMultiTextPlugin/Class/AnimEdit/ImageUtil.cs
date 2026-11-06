using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit
{
    /// <summary>
    /// Shared lock for the Ultima SDK. Its caches are not thread safe, so every access from
    /// the editor (UI thread and background loaders) goes through this lock.
    /// </summary>
    internal static class SdkSync
    {
        public static readonly object Lock = new object();
    }

    /// <summary>Small image helpers used by the animation editor.</summary>
    internal static class ImageUtil
    {
        /// <summary>Copies any image into a new 32bpp ARGB bitmap.</summary>
        public static Bitmap To32(Image src)
        {
            if (src == null) return null;
            var bmp = new Bitmap(Math.Max(1, src.Width), Math.Max(1, src.Height), PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel);
            }
            return bmp;
        }

        /// <summary>Loads an image file as an independent copy (Image.FromFile would keep the file locked).</summary>
        public static Bitmap LoadCopy(string path)
        {
            using (var tmp = new Bitmap(path))
                return To32(tmp);
        }

        /// <summary>Loads all frames of a (possibly animated) GIF or a single image.</summary>
        public static List<Bitmap> LoadAllFrames(string path)
        {
            var list = new List<Bitmap>();
            using (var img = Image.FromFile(path))
            {
                int count = 1;
                try
                {
                    if (Array.IndexOf(img.FrameDimensionsList, FrameDimension.Time.Guid) >= 0)
                        count = img.GetFrameCount(FrameDimension.Time);
                }
                catch { count = 1; }

                for (int i = 0; i < count; i++)
                {
                    if (count > 1) img.SelectActiveFrame(FrameDimension.Time, i);
                    list.Add(To32(img));
                }
            }
            return list;
        }

        /// <summary>Pixel exact scaling (no blur for pixel art).</summary>
        public static Bitmap ScaleNearest(Image src, int width, int height)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawImage(src, new Rectangle(0, 0, width, height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel);
            }
            return bmp;
        }

        public static Bitmap Mirror(Image src)
        {
            Bitmap b = To32(src);
            b.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return b;
        }

        /// <summary>Draws an image with an opacity (0..1) and an optional color tint.</summary>
        public static void DrawImage(Graphics g, Image img, Rectangle dest, float opacity, Color? tint = null)
        {
            if (img == null || opacity <= 0f) return;
            if (opacity >= 0.999f && tint == null)
            {
                g.DrawImage(img, dest, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                return;
            }

            float r = 1, gg = 1, b = 1, tr = 0, tg = 0, tb = 0;
            if (tint.HasValue)
            {
                // Mix 50 % of the tint color into the image
                r = gg = b = 0.5f;
                tr = tint.Value.R / 255f * 0.5f;
                tg = tint.Value.G / 255f * 0.5f;
                tb = tint.Value.B / 255f * 0.5f;
            }

            var cm = new ColorMatrix(new[]
            {
                new[] { r, 0f, 0f, 0f, 0f },
                new[] { 0f, gg, 0f, 0f, 0f },
                new[] { 0f, 0f, b, 0f, 0f },
                new[] { 0f, 0f, 0f, opacity, 0f },
                new[] { tr, tg, tb, 0f, 1f }
            });
            using (var ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm);
                g.DrawImage(img, dest, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
            }
        }

        /// <summary>Makes exact key colors transparent (fast, via LockBits). Returns a new bitmap.</summary>
        public static Bitmap MakeTransparent(Image source, IEnumerable<Color> keys, int tolerance = 0)
        {
            Bitmap bmp = To32(source);
            var pb = new PixelBuffer(bmp);
            foreach (Color k in keys)
                pb.ReplaceColor(k.ToArgb(), 0, tolerance, true);
            pb.WriteTo(bmp);
            return bmp;
        }

        /// <summary>Bounding box of all non transparent pixels (Rectangle.Empty when the image is empty).</summary>
        public static Rectangle ContentBounds(Bitmap bmp)
        {
            if (bmp == null) return Rectangle.Empty;
            var pb = new PixelBuffer(bmp);
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
            for (int y = 0; y < pb.Height; y++)
            {
                int row = y * pb.Width;
                for (int x = 0; x < pb.Width; x++)
                {
                    if (((uint)pb.Pixels[row + x] >> 24) == 0) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
            return maxX < 0 ? Rectangle.Empty : Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }

        /// <summary>The distinct visible colors of an image ordered by frequency (max "limit").</summary>
        public static List<Color> UsedColors(Bitmap bmp, int limit)
        {
            var counts = new Dictionary<int, int>();
            if (bmp == null) return new List<Color>();
            var pb = new PixelBuffer(bmp);
            foreach (int c in pb.Pixels)
            {
                if (((uint)c >> 24) == 0) continue;
                counts.TryGetValue(c, out int n);
                counts[c] = n + 1;
            }
            var list = new List<KeyValuePair<int, int>>(counts);
            list.Sort((a, b) => b.Value.CompareTo(a.Value));
            var result = new List<Color>();
            for (int i = 0; i < list.Count && i < limit; i++)
                result.Add(Color.FromArgb(list[i].Key));
            return result;
        }

        public static string ToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        /// <summary>Draws a checkerboard (used as "transparent" background).</summary>
        public static void DrawChecker(Graphics g, Rectangle r, int cell = 8)
        {
            using (var light = new SolidBrush(Color.FromArgb(235, 235, 235)))
            using (var dark = new SolidBrush(Color.FromArgb(200, 200, 200)))
            {
                g.FillRectangle(light, r);
                for (int y = r.Top; y < r.Bottom; y += cell)
                {
                    for (int x = r.Left + ((((y - r.Top) / cell) % 2) * cell); x < r.Right; x += cell * 2)
                    {
                        g.FillRectangle(dark, x, y, Math.Min(cell, r.Right - x), Math.Min(cell, r.Bottom - y));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Pixel access to a 32bpp ARGB bitmap through an int array (much faster than GetPixel/SetPixel).
    /// Read with the constructor, change the pixels, then write back with <see cref="WriteTo"/>.
    /// All drawing is pixel exact (no anti aliasing), which is what pixel art needs.
    /// </summary>
    internal sealed class PixelBuffer
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int[] Pixels;

        /// <summary>Optional per stroke mask: each pixel is blended only once per stroke.</summary>
        public bool[] StrokeMask;

        public PixelBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new int[width * height];
        }

        public PixelBuffer(Bitmap bmp)
        {
            Width = bmp.Width;
            Height = bmp.Height;
            Pixels = new int[Width * Height];

            Bitmap src = bmp.PixelFormat == PixelFormat.Format32bppArgb ? bmp : ImageUtil.To32(bmp);
            try
            {
                BitmapData data = src.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    for (int y = 0; y < Height; y++)
                        Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), Pixels, y * Width, Width);
                }
                finally { src.UnlockBits(data); }
            }
            finally
            {
                if (!ReferenceEquals(src, bmp)) src.Dispose();
            }
        }

        public void WriteTo(Bitmap bmp)
        {
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < Height; y++)
                    Marshal.Copy(Pixels, y * Width, IntPtr.Add(data.Scan0, y * data.Stride), Width);
            }
            finally { bmp.UnlockBits(data); }
        }

        public Bitmap ToBitmap()
        {
            var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            WriteTo(bmp);
            return bmp;
        }

        public bool Inside(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public int Get(int x, int y) => Inside(x, y) ? Pixels[y * Width + x] : 0;

        /// <summary>Paints one pixel. erase = make transparent, otherwise "source over" blending.</summary>
        public void Plot(int x, int y, int argb, bool erase)
        {
            if (!Inside(x, y)) return;
            int i = y * Width + x;

            if (StrokeMask != null)
            {
                if (StrokeMask[i]) return;
                StrokeMask[i] = true;
            }

            if (erase) { Pixels[i] = 0; return; }

            uint sa = (uint)argb >> 24;
            if (sa == 255) { Pixels[i] = argb; return; }
            if (sa == 0) return;

            int dst = Pixels[i];
            uint da = (uint)dst >> 24;
            if (da == 0) { Pixels[i] = argb; return; }

            float a = sa / 255f;
            float outA = a + da / 255f * (1 - a);
            int r = (int)((((argb >> 16) & 255) * a + ((dst >> 16) & 255) * (da / 255f) * (1 - a)) / outA);
            int g = (int)((((argb >> 8) & 255) * a + ((dst >> 8) & 255) * (da / 255f) * (1 - a)) / outA);
            int b = (int)(((argb & 255) * a + (dst & 255) * (da / 255f) * (1 - a)) / outA);
            Pixels[i] = ((int)(outA * 255) << 24) | (Clamp(r) << 16) | (Clamp(g) << 8) | Clamp(b);
        }

        private static int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

        /// <summary>Brush stamp centered on (cx, cy).</summary>
        public void Stamp(int cx, int cy, int size, int argb, bool erase, bool round)
        {
            if (size <= 1) { Plot(cx, cy, argb, erase); return; }

            int r0 = (size - 1) / 2;
            int r1 = size / 2;
            double rad = size / 2.0;
            for (int dy = -r0; dy <= r1; dy++)
            {
                for (int dx = -r0; dx <= r1; dx++)
                {
                    if (round)
                    {
                        double fx = dx - (r1 - r0) / 2.0, fy = dy - (r1 - r0) / 2.0;
                        if (fx * fx + fy * fy > rad * rad) continue;
                    }
                    Plot(cx + dx, cy + dy, argb, erase);
                }
            }
        }

        /// <summary>Bresenham line with brush size.</summary>
        public void Line(int x0, int y0, int x1, int y1, int size, int argb, bool erase, bool round)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                Stamp(x0, y0, size, argb, erase, round);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        public void Rect(Rectangle r, int size, int argb, bool erase, bool filled)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            if (filled)
            {
                for (int y = r.Top; y < r.Bottom; y++)
                    for (int x = r.Left; x < r.Right; x++)
                        Plot(x, y, argb, erase);
                return;
            }
            int x2 = r.Right - 1, y2 = r.Bottom - 1;
            Line(r.Left, r.Top, x2, r.Top, size, argb, erase, false);
            Line(x2, r.Top, x2, y2, size, argb, erase, false);
            Line(x2, y2, r.Left, y2, size, argb, erase, false);
            Line(r.Left, y2, r.Left, r.Top, size, argb, erase, false);
        }

        public void Ellipse(Rectangle r, int size, int argb, bool erase, bool filled)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            double cx = r.Left + (r.Width - 1) / 2.0, cy = r.Top + (r.Height - 1) / 2.0;
            double rx = Math.Max(0.5, (r.Width - 1) / 2.0), ry = Math.Max(0.5, (r.Height - 1) / 2.0);

            if (filled)
            {
                for (int y = r.Top; y < r.Bottom; y++)
                {
                    double ny = (y - cy) / ry;
                    if (Math.Abs(ny) > 1) continue;
                    double w = rx * Math.Sqrt(1 - ny * ny);
                    for (int x = (int)Math.Round(cx - w); x <= (int)Math.Round(cx + w); x++)
                        Plot(x, y, argb, erase);
                }
                return;
            }

            int steps = Math.Max(16, (int)((rx + ry) * 4));
            int px = (int)Math.Round(cx + rx), py = (int)Math.Round(cy);
            for (int s = 1; s <= steps; s++)
            {
                double t = s * 2 * Math.PI / steps;
                int nx = (int)Math.Round(cx + rx * Math.Cos(t));
                int ny = (int)Math.Round(cy + ry * Math.Sin(t));
                Line(px, py, nx, ny, size, argb, erase, true);
                px = nx; py = ny;
            }
        }

        private static bool Similar(int a, int b, int tol)
        {
            if (a == b) return true;
            if (tol <= 0) return false;
            // Two fully transparent pixels are always "equal"
            if (((uint)a >> 24) == 0 && ((uint)b >> 24) == 0) return true;
            return Math.Abs(((a >> 24) & 255) - ((b >> 24) & 255)) <= tol
                && Math.Abs(((a >> 16) & 255) - ((b >> 16) & 255)) <= tol
                && Math.Abs(((a >> 8) & 255) - ((b >> 8) & 255)) <= tol
                && Math.Abs((a & 255) - (b & 255)) <= tol;
        }

        /// <summary>Scanline flood fill (4-neighbour).</summary>
        public void FloodFill(int x, int y, int argb, bool erase, int tolerance)
        {
            if (!Inside(x, y)) return;
            int target = Pixels[y * Width + x];
            int replacement = erase ? 0 : argb;
            if (target == replacement && tolerance == 0) return;

            var visited = new bool[Pixels.Length];
            var stack = new Stack<Point>();
            stack.Push(new Point(x, y));
            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                int lx = p.X;
                int row = p.Y * Width;
                while (lx > 0 && !visited[row + lx - 1] && Similar(Pixels[row + lx - 1], target, tolerance)) lx--;
                bool up = false, down = false;
                for (int cx = lx; cx < Width; cx++)
                {
                    int i = row + cx;
                    if (visited[i] || !Similar(Pixels[i], target, tolerance)) break;
                    visited[i] = true;
                    Plot(cx, p.Y, argb, erase);

                    if (p.Y > 0)
                    {
                        int iu = i - Width;
                        bool ok = !visited[iu] && Similar(Pixels[iu], target, tolerance);
                        if (ok && !up) { stack.Push(new Point(cx, p.Y - 1)); up = true; }
                        else if (!ok) up = false;
                    }
                    if (p.Y < Height - 1)
                    {
                        int id = i + Width;
                        bool ok = !visited[id] && Similar(Pixels[id], target, tolerance);
                        if (ok && !down) { stack.Push(new Point(cx, p.Y + 1)); down = true; }
                        else if (!ok) down = false;
                    }
                }
            }
        }

        /// <summary>Replaces a color everywhere. ignoreAlpha compares only RGB of opaque pixels.</summary>
        public int ReplaceColor(int from, int to, int tolerance, bool ignoreAlpha)
        {
            int changed = 0;
            for (int i = 0; i < Pixels.Length; i++)
            {
                int p = Pixels[i];
                if (ignoreAlpha)
                {
                    if (((uint)p >> 24) == 0) continue;
                    if (!Similar(p | unchecked((int)0xFF000000), from | unchecked((int)0xFF000000), tolerance)) continue;
                }
                else if (!Similar(p, from, tolerance)) continue;

                Pixels[i] = to;
                changed++;
            }
            return changed;
        }

        /// <summary>Draws a 1 pixel outline around all visible pixels (good for sprites).</summary>
        public void Outline(int argb)
        {
            var copy = (int[])Pixels.Clone();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (((uint)copy[y * Width + x] >> 24) != 0) continue;
                    bool near =
                        (x > 0 && ((uint)copy[y * Width + x - 1] >> 24) != 0) ||
                        (x < Width - 1 && ((uint)copy[y * Width + x + 1] >> 24) != 0) ||
                        (y > 0 && ((uint)copy[(y - 1) * Width + x] >> 24) != 0) ||
                        (y < Height - 1 && ((uint)copy[(y + 1) * Width + x] >> 24) != 0);
                    if (near) Pixels[y * Width + x] = argb;
                }
            }
        }
    }
}
