/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Ultima;

namespace UoFiddler.Controls.MapViewer.Build
{
    /// <summary>
    /// Rendered image of a template. <see cref="OriginX"/>/<see cref="OriginY"/> is the position of the upper
    /// corner of template cell 0,0 at z 0 inside the bitmap.
    /// </summary>
    public sealed class MultiImage : IDisposable
    {
        public Bitmap Bitmap { get; set; }
        public int OriginX { get; set; }
        public int OriginY { get; set; }

        public void Dispose()
        {
            Bitmap?.Dispose();
            Bitmap = null;
        }
    }

    /// <summary>
    /// Renders a <see cref="MultiTemplate"/> like the client (statics only, sorted like the region view)
    /// </summary>
    public static class MultiPreview
    {
        /// <summary>Larger images are not rendered (memory)</summary>
        public const long MaxPixels = 48L * 1024 * 1024;

        public static MultiImage Render(MultiTemplate t, ArtPixelCache art, bool includeInvisible)
        {
            var items = new List<(int sortA, int sortZ, int seq, int x, int y, ArtImage img)>();
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            int seq = 0;

            foreach (MultiComponent c in t.Components)
            {
                if (!includeInvisible && !c.IsVisible)
                {
                    continue;
                }

                ArtImage img = art.GetStatic(c.ItemId, c.Hue);
                if (img == null)
                {
                    continue;
                }

                int px = ((c.X - c.Y) * RegionData.TileHalf) - (img.Width / 2);
                int py = ((c.X + c.Y) * RegionData.TileHalf) - (c.Z * RegionData.ZScale) + RegionData.TileSize - img.Height;

                TileFlag flags = c.ItemId < TileData.ItemTable.Length ? TileData.ItemTable[c.ItemId].Flags : TileFlag.None;
                int height = c.ItemId < TileData.ItemTable.Length ? TileData.ItemTable[c.ItemId].Height : 0;
                int sortZ = c.Z + ((flags & TileFlag.Background) != 0 ? 0 : 1) + (height > 0 ? 1 : 0);

                items.Add((c.X + c.Y, sortZ, seq++, px, py, img));
                minX = Math.Min(minX, px);
                minY = Math.Min(minY, py);
                maxX = Math.Max(maxX, px + img.Width);
                maxY = Math.Max(maxY, py + img.Height);
            }

            if (items.Count == 0)
            {
                return null;
            }

            int width = maxX - minX, height2 = maxY - minY;
            if ((long)width * height2 > MaxPixels)
            {
                return null;
            }

            items.Sort((a, b) =>
            {
                int r = a.sortA.CompareTo(b.sortA);
                if (r != 0)
                {
                    return r;
                }

                r = a.sortZ.CompareTo(b.sortZ);
                return r != 0 ? r : a.seq.CompareTo(b.seq);
            });

            var buffer = new int[width * height2];
            foreach ((int _, int _, int _, int x, int y, ArtImage img) in items)
            {
                Blit(buffer, width, height2, img, x - minX, y - minY);
            }

            var bmp = new Bitmap(width, height2, PixelFormat.Format32bppArgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height2), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < height2; y++)
                {
                    Marshal.Copy(buffer, y * width, bd.Scan0 + (y * bd.Stride), width);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            return new MultiImage { Bitmap = bmp, OriginX = -minX, OriginY = -minY };
        }

        private static void Blit(int[] buffer, int bw, int bh, ArtImage art, int x, int y)
        {
            int sx0 = Math.Max(0, -x), sy0 = Math.Max(0, -y);
            int sx1 = Math.Min(art.Width, bw - x), sy1 = Math.Min(art.Height, bh - y);
            int[] src = art.Pixels;
            for (int sy = sy0; sy < sy1; sy++)
            {
                int srcRow = sy * art.Width;
                int dstRow = (y + sy) * bw;
                for (int sx = sx0; sx < sx1; sx++)
                {
                    int c = src[srcRow + sx];
                    if ((c >> 24) != 0)
                    {
                        buffer[dstRow + x + sx] = c | unchecked((int)0xFF000000);
                    }
                }
            }
        }

        /// <summary>
        /// Thumbnail that fits into the given size (or null)
        /// </summary>
        public static Bitmap Thumbnail(MultiTemplate t, ArtPixelCache art, Size size, Color background)
        {
            using (MultiImage image = Render(t, art, false))
            {
                var bmp = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(background);
                    if (image?.Bitmap == null)
                    {
                        string text = t.Components.Count == 0 ? "empty" : "too large for a preview";
                        g.DrawString(text, SystemFonts.DefaultFont, Brushes.Gray, 8, 8);
                        return bmp;
                    }

                    float scale = Math.Min(1f, Math.Min(size.Width / (float)image.Bitmap.Width, size.Height / (float)image.Bitmap.Height));
                    int w = (int)(image.Bitmap.Width * scale), h = (int)(image.Bitmap.Height * scale);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image.Bitmap, new Rectangle((size.Width - w) / 2, (size.Height - h) / 2, w, h));
                }

                return bmp;
            }
        }
    }
}
