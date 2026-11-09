using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter
{
    /// <summary>What happens with transparent pixels when an image is written to a file.</summary>
    internal enum TransparentFill { Keep, Black, White }

    /// <summary>Bitmap helpers of the Graphic Cutter. All results are 32bpp ARGB bitmaps.</summary>
    internal static class CutterImaging
    {
        public const string ImageFilter = "Images|*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|All files|*.*";

        public static Bitmap To32Bpp(Image image)
        {
            var bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
            }
            return bmp;
        }

        /// <summary>Loads an image without keeping the file locked.</summary>
        public static Bitmap LoadBitmap(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var image = Image.FromStream(stream))
            {
                return To32Bpp(image);
            }
        }

        public static int[] GetPixels(Bitmap bmp)
        {
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var pixels = new int[bmp.Width * bmp.Height];
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                return pixels;
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        public static void SetPixels(Bitmap bmp, int[] pixels)
        {
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        /// <summary>Makes every pixel transparent whose color is within the tolerance of one of the given colors.</summary>
        public static void MakeTransparent(Bitmap bmp, int tolerance, IReadOnlyList<Color> colors)
        {
            if (colors.Count == 0)
            {
                return;
            }
            int[] pixels = GetPixels(bmp);
            for (int i = 0; i < pixels.Length; i++)
            {
                int p = pixels[i];
                if ((p >> 24) == 0)
                {
                    continue;
                }
                int r = (p >> 16) & 0xFF, g = (p >> 8) & 0xFF, b = p & 0xFF;
                for (int c = 0; c < colors.Count; c++)
                {
                    Color col = colors[c];
                    if (Math.Abs(r - col.R) <= tolerance && Math.Abs(g - col.G) <= tolerance && Math.Abs(b - col.B) <= tolerance)
                    {
                        pixels[i] = 0;
                        break;
                    }
                }
            }
            SetPixels(bmp, pixels);
        }

        /// <summary>Area of all non transparent pixels, or an empty rectangle.</summary>
        public static Rectangle GetContentBounds(Bitmap bmp)
        {
            int[] pixels = GetPixels(bmp);
            int w = bmp.Width, h = bmp.Height;
            int minX = w, minY = h, maxX = -1, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    if ((pixels[row + x] >> 24) != 0)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }
            return maxX < 0 ? Rectangle.Empty : Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }

        /// <summary>Returns the image cut to its non transparent area, or null if it is fully transparent.</summary>
        public static Bitmap TrimTransparent(Bitmap bmp)
        {
            Rectangle bounds = GetContentBounds(bmp);
            return bounds.IsEmpty ? null : bmp.Clone(bounds, PixelFormat.Format32bppArgb);
        }

        /// <summary>Removes only the empty rows at the top (UO art stands on its bottom edge).</summary>
        public static Bitmap TrimTop(Bitmap bmp)
        {
            Rectangle bounds = GetContentBounds(bmp);
            if (bounds.IsEmpty)
            {
                return null;
            }
            return bmp.Clone(new Rectangle(0, bounds.Top, bmp.Width, bmp.Height - bounds.Top), PixelFormat.Format32bppArgb);
        }

        public static bool IsEmpty(Bitmap bmp) => GetContentBounds(bmp).IsEmpty;

        public static void FillTransparent(Bitmap bmp, Color color)
        {
            int[] pixels = GetPixels(bmp);
            int argb = color.ToArgb();
            for (int i = 0; i < pixels.Length; i++)
            {
                if ((pixels[i] >> 24) == 0)
                {
                    pixels[i] = argb;
                }
            }
            SetPixels(bmp, pixels);
        }

        /// <summary>Copy of the image prepared for a file: transparent pixels filled as chosen.</summary>
        public static Bitmap PrepareForFile(Bitmap bmp, TransparentFill fill)
        {
            Bitmap copy = To32Bpp(bmp);
            if (fill == TransparentFill.Black)
            {
                FillTransparent(copy, Color.Black);
            }
            else if (fill == TransparentFill.White)
            {
                FillTransparent(copy, Color.White);
            }
            return copy;
        }

        public static ImageFormat FormatFromExtension(string extension)
        {
            switch ((extension ?? string.Empty).ToLowerInvariant())
            {
                case ".png": return ImageFormat.Png;
                case ".tif":
                case ".tiff": return ImageFormat.Tiff;
                case ".jpg":
                case ".jpeg": return ImageFormat.Jpeg;
                default: return ImageFormat.Bmp;
            }
        }

        public static void SaveImage(Bitmap bmp, string file, TransparentFill fill)
        {
            using (Bitmap copy = PrepareForFile(bmp, fill))
            {
                copy.Save(file, FormatFromExtension(Path.GetExtension(file)));
            }
        }

        /// <summary>Puts the image on the clipboard as bitmap and as PNG (PNG keeps the transparency).</summary>
        public static void CopyToClipboard(Bitmap bmp)
        {
            var data = new DataObject();
            data.SetData(DataFormats.Bitmap, true, bmp);
            var png = new MemoryStream();
            bmp.Save(png, ImageFormat.Png);
            data.SetData("PNG", false, png);
            Clipboard.SetDataObject(data, true);
        }

        /// <summary>Image of the clipboard as 32bpp bitmap, or null.</summary>
        public static Bitmap GetClipboardImage()
        {
            if (Clipboard.ContainsData("PNG") && Clipboard.GetData("PNG") is Stream png)
            {
                using (var image = Image.FromStream(png))
                {
                    return To32Bpp(image);
                }
            }
            if (Clipboard.ContainsImage())
            {
                using (Image image = Clipboard.GetImage())
                {
                    return image == null ? null : To32Bpp(image);
                }
            }
            return null;
        }

        /// <summary>Draws a checkerboard that shows transparent areas.</summary>
        public static void DrawChecker(Graphics g, Rectangle area)
        {
            const int cell = 8;
            g.FillRectangle(Brushes.White, area);
            using (var brush = new SolidBrush(Color.FromArgb(204, 204, 204)))
            {
                for (int y = area.Top; y < area.Bottom; y += cell)
                {
                    for (int x = area.Left + (((y - area.Top) / cell) % 2) * cell; x < area.Right; x += cell * 2)
                    {
                        g.FillRectangle(brush, x, y, Math.Min(cell, area.Right - x), Math.Min(cell, area.Bottom - y));
                    }
                }
            }
        }

        /// <summary>Draws an image scaled down (never up beyond 2x) and centered into a box.</summary>
        public static void DrawFitted(Graphics g, Image image, Rectangle box, bool allowEnlarge)
        {
            if (image == null || image.Width <= 0 || image.Height <= 0)
            {
                return;
            }
            double scale = Math.Min(box.Width / (double)image.Width, box.Height / (double)image.Height);
            if (!allowEnlarge)
            {
                scale = Math.Min(scale, 1.0);
            }
            else
            {
                scale = Math.Min(scale, 4.0);
            }
            int w = Math.Max(1, (int)(image.Width * scale));
            int h = Math.Max(1, (int)(image.Height * scale));
            var dest = new Rectangle(box.X + (box.Width - w) / 2, box.Y + (box.Height - h) / 2, w, h);
            InterpolationMode old = g.InterpolationMode;
            g.InterpolationMode = scale >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, dest);
            g.InterpolationMode = old;
        }

        /// <summary>Parses "0x1A2B", "1A2B h" style hex or a decimal number. Plain digits are read as decimal.</summary>
        public static bool TryParseId(string text, out int id)
        {
            id = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            string t = text.Trim();
            if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(t.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out id);
            }
            if (t.EndsWith("h", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(t.Substring(0, t.Length - 1).Trim(), System.Globalization.NumberStyles.HexNumber, null, out id);
            }
            if (int.TryParse(t, out id))
            {
                return true;
            }
            // Contains A-F: hex without prefix.
            return int.TryParse(t, System.Globalization.NumberStyles.HexNumber, null, out id);
        }

        public static string IdText(int id) => $"0x{id:X4} ({id})";
    }
}
