// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using UoFiddler.Controls.ArtLibrary.Core;

namespace UoFiddler.Controls.GumpLibrary.Helpers
{
    /// <summary>Image tools for gumps. All results are new ARGB1555 bitmaps; the source is not changed.</summary>
    public static class GumpImageTools
    {
        /// <summary>Mirrors left/right (horizontal) or top/bottom.</summary>
        public static Bitmap Mirror(Bitmap source, bool horizontal)
        {
            int w = source.Width;
            int h = source.Height;
            ushort[] src = ArtLibraryCodec.ToPixels(source);
            var dst = new ushort[src.Length];
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int nx = horizontal ? w - 1 - x : x;
                    int ny = horizontal ? y : h - 1 - y;
                    dst[(ny * w) + nx] = src[(y * w) + x];
                }
            }

            return ArtLibraryCodec.ToBitmap(dst, w, h);
        }

        /// <summary>Rotates by 90° (width and height swap).</summary>
        public static Bitmap Rotate(Bitmap source, bool clockwise)
        {
            int w = source.Width;
            int h = source.Height;
            ushort[] src = ArtLibraryCodec.ToPixels(source);
            var dst = new ushort[src.Length];
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    int nx = clockwise ? h - 1 - y : y;
                    int ny = clockwise ? x : w - 1 - x;
                    dst[(ny * h) + nx] = src[(y * w) + x];
                }
            }

            return ArtLibraryCodec.ToBitmap(dst, h, w);
        }

        /// <summary>Cuts away transparent borders. Returns a copy of the source when nothing is visible.</summary>
        public static Bitmap Trim(Bitmap source)
        {
            int w = source.Width;
            int h = source.Height;
            ushort[] src = ArtLibraryCodec.ToPixels(source);
            int minX = w, minY = h, maxX = -1, maxY = -1;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    if ((src[(y * w) + x] & 0x8000) != 0)
                    {
                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }

            if (maxX < 0)
            {
                return ArtLibraryCodec.ToBitmap(src, w, h);
            }

            int nw = maxX - minX + 1;
            int nh = maxY - minY + 1;
            var dst = new ushort[nw * nh];
            for (int y = 0; y < nh; ++y)
            {
                Array.Copy(src, ((y + minY) * w) + minX, dst, y * nw, nw);
            }

            return ArtLibraryCodec.ToBitmap(dst, nw, nh);
        }

        /// <summary>
        /// A preview image: the gump scaled down (never up) so the longer side is at most
        /// <paramref name="maxSize"/>, transparency kept.
        /// </summary>
        public static Bitmap CreatePreview(Bitmap source, int maxSize)
        {
            double factor = Math.Min(1.0, maxSize / (double)Math.Max(source.Width, source.Height));
            int w = Math.Max(1, (int)Math.Round(source.Width * factor));
            int h = Math.Max(1, (int)Math.Round(source.Height * factor));
            using (var work = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(work))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = factor < 1.0 ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(source, new Rectangle(0, 0, w, h));
                }

                return ArtLibraryCodec.ToBitmap(ArtLibraryCodec.ToPixels(work), w, h);
            }
        }

        /// <summary>Scales an image so it fits into <paramref name="maxWidth"/> x <paramref name="maxHeight"/> (for drawing).</summary>
        public static Size FitInto(Size size, int maxWidth, int maxHeight)
        {
            if (size.Width <= maxWidth && size.Height <= maxHeight)
            {
                return size;
            }

            double factor = Math.Min(maxWidth / (double)size.Width, maxHeight / (double)size.Height);
            return new Size(Math.Max(1, (int)(size.Width * factor)), Math.Max(1, (int)(size.Height * factor)));
        }
    }
}
