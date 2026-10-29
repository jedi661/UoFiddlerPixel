/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using UoFiddler.Plugin.Enhanced.Core.Dds;

namespace UoFiddler.Plugin.Enhanced.Classes
{
    /// <summary>
    /// Conversion between the plugin's BGRA pixel buffers / DDS files and GDI+ bitmaps.
    /// </summary>
    public static class EcImageHelper
    {
        public static Bitmap ToBitmap(DdsImage image)
        {
            return ToBitmap(image.Pixels, image.Width, image.Height);
        }

        public static Bitmap ToBitmap(byte[] bgra, int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(bgra, y * width * 4, data.Scan0 + (y * data.Stride), width * 4);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
            return bitmap;
        }

        public static byte[] ToBgra(Bitmap source, out int width, out int height)
        {
            width = source.Width;
            height = source.Height;
            using (var copy = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(copy))
                {
                    g.DrawImage(source, new Rectangle(0, 0, width, height));
                }

                var pixels = new byte[width * height * 4];
                BitmapData data = copy.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    for (int y = 0; y < height; y++)
                    {
                        Marshal.Copy(data.Scan0 + (y * data.Stride), pixels, y * width * 4, width * 4);
                    }
                }
                finally
                {
                    copy.UnlockBits(data);
                }
                return pixels;
            }
        }

        /// <summary>
        /// Loads a picture file (dds, png, bmp, gif, jpg, tif) as bitmap.
        /// </summary>
        public static Bitmap LoadImageFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            if (DdsImage.IsDds(bytes))
            {
                return ToBitmap(DdsImage.Decode(bytes));
            }

            using (var ms = new MemoryStream(bytes))
            using (var image = Image.FromStream(ms))
            {
                return new Bitmap(image);
            }
        }

        /// <summary>
        /// Makes pixels of the given color fully transparent (useful for images exported with a background color).
        /// </summary>
        public static void MakeTransparent(byte[] bgra, Color color)
        {
            for (int i = 0; i < bgra.Length; i += 4)
            {
                if (bgra[i] == color.B && bgra[i + 1] == color.G && bgra[i + 2] == color.R)
                {
                    bgra[i + 3] = 0;
                }
            }
        }

        /// <summary>
        /// Tries to decode any entry data as image (DDS or a common image format). Returns null if it is no image.
        /// </summary>
        public static Bitmap TryDecode(byte[] data, out string info)
        {
            info = null;
            if (data == null || data.Length < 8)
            {
                return null;
            }

            try
            {
                if (DdsImage.IsDds(data))
                {
                    DdsImage dds = DdsImage.Decode(data);
                    info = $"{dds.Width} x {dds.Height}  {dds.FormatName}  mips: {dds.MipMapCount}";
                    return ToBitmap(dds);
                }

                bool png = data[0] == 0x89 && data[1] == 'P' && data[2] == 'N' && data[3] == 'G';
                bool jpg = data[0] == 0xFF && data[1] == 0xD8;
                bool bmp = data[0] == 'B' && data[1] == 'M';
                bool gif = data[0] == 'G' && data[1] == 'I' && data[2] == 'F';
                if (png || jpg || bmp || gif)
                {
                    using (var ms = new MemoryStream(data))
                    using (var image = Image.FromStream(ms))
                    {
                        info = $"{image.Width} x {image.Height}  {(png ? "PNG" : jpg ? "JPEG" : bmp ? "BMP" : "GIF")}";
                        return new Bitmap(image);
                    }
                }
            }
            catch (Exception ex)
            {
                info = "Image error: " + ex.Message;
            }

            return null;
        }

        /// <summary>
        /// Scales the image into a square of <paramref name="size"/> pixels (never enlarges more than 2x).
        /// </summary>
        public static Bitmap CreateThumbnail(Bitmap source, int size)
        {
            float scale = Math.Min(2f, Math.Min((float)size / source.Width, (float)size / source.Height));
            int w = Math.Max(1, (int)(source.Width * scale));
            int h = Math.Max(1, (int)(source.Height * scale));
            var thumb = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(thumb))
            {
                g.InterpolationMode = scale < 1 ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(source, new Rectangle(0, 0, w, h));
            }
            return thumb;
        }

        public static void DrawCheckerboard(Graphics g, Rectangle area, int cell = 8)
        {
            using (var light = new SolidBrush(Color.FromArgb(255, 255, 255)))
            using (var dark = new SolidBrush(Color.FromArgb(225, 225, 225)))
            {
                g.FillRectangle(light, area);
                for (int y = area.Top; y < area.Bottom; y += cell)
                {
                    for (int x = area.Left + (((y - area.Top) / cell % 2) * cell); x < area.Right; x += cell * 2)
                    {
                        g.FillRectangle(dark, x, y, Math.Min(cell, area.Right - x), Math.Min(cell, area.Bottom - y));
                    }
                }
            }
        }

        public static string FormatSize(long bytes)
        {
            if (bytes >= 1024 * 1024)
            {
                return $"{bytes / (1024.0 * 1024.0):0.0} MB";
            }
            if (bytes >= 1024)
            {
                return $"{bytes / 1024.0:0.0} KB";
            }
            return $"{bytes} B";
        }
    }
}
