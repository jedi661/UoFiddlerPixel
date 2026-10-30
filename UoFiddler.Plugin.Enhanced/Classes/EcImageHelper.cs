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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;

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
        /// Tries to decode any entry data as image (DDS, TGA or a common image format). Returns null if it is no image.
        /// </summary>
        /// <param name="nameHint">Packed file name if known (helps to detect TGA, which has no magic number).</param>
        public static Bitmap TryDecode(byte[] data, out string info, string nameHint = null)
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

                bool tgaName = nameHint != null && nameHint.EndsWith(".tga", StringComparison.OrdinalIgnoreCase);
                if (tgaName ? TgaImage.LooksLikeTga(data) : TgaImage.IsLikelyTga(data))
                {
                    DdsImage tga = TgaImage.Decode(data);
                    info = $"{tga.Width} x {tga.Height}  {tga.FormatName}";
                    return ToBitmap(tga);
                }
            }
            catch (Exception ex)
            {
                info = "Image error: " + ex.Message;
            }

            return null;
        }

        /// <summary>
        /// Short description of non-image data.
        /// </summary>
        public static string DescribeData(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return "empty";
            }
            if (data.Length >= 8 && Encoding.ASCII.GetString(data, 0, 8) == "Gamebryo")
            {
                int end = Array.IndexOf(data, (byte)0x0A, 0, Math.Min(data.Length, 80));
                return "3D model (" + Encoding.ASCII.GetString(data, 0, end > 0 ? end : Math.Min(40, data.Length)) + ")";
            }
            if (data.Length >= 4 && data[0] == '8' && data[1] == 'B' && data[2] == 'P' && data[3] == 'S')
            {
                return "Photoshop image (PSD, no preview)";
            }
            if (data.Length >= 4 && data[0] == 0x1B && data[1] == 'L' && data[2] == 'u' && data[3] == 'a')
            {
                return "compiled Lua";
            }
            if (data.Length >= 4 && data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F')
            {
                return "RIFF (wav)";
            }
            if (data.Length >= 3 && data[0] == 'I' && data[1] == 'D' && data[2] == '3')
            {
                return "MP3";
            }
            if (data.Length >= 4 && data[0] == 'O' && data[1] == 'g' && data[2] == 'g' && data[3] == 'S')
            {
                return "OGG";
            }
            if (IsText(data))
            {
                return data.Length > 14 && Encoding.ASCII.GetString(data, 0, 14) == "particlesystem" ? "particle system script (text)" : "text";
            }
            return "binary";
        }

        /// <summary>
        /// True when the first 4 KB are (almost) only printable characters.
        /// </summary>
        public static bool IsText(byte[] data)
        {
            int length = Math.Min(data.Length, 4096);
            if (length == 0)
            {
                return false;
            }
            int printable = 0;
            for (int i = 0; i < length; i++)
            {
                byte b = data[i];
                if (b == 9 || b == 10 || b == 13 || (b >= 32 && b < 127) || b >= 160)
                {
                    printable++;
                }
                else if (b == 0)
                {
                    return false;
                }
            }
            return printable >= length * 0.95;
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
