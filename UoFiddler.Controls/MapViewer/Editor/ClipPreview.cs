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
using System.Runtime.InteropServices;
using Ultima;
using UoFiddler.Controls.MapViewer.Build;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Renders a <see cref="MapClip"/> (flat land art + statics) as a half transparent image for placing.
    /// <see cref="MultiImage.OriginX"/>/<see cref="MultiImage.OriginY"/> is the upper corner of clip cell 0,0 at z 0.
    /// </summary>
    public static class ClipPreview
    {
        public static MultiImage Render(MapClip clip, ArtPixelCache art, ClipPasteOptions options)
        {
            if (clip == null)
            {
                return null;
            }

            // rough size check first: the canvas grows with width + height
            long canvasW = (long)(clip.Width + clip.Height) * RegionData.TileHalf + 64;
            long canvasH = canvasW + ((clip.MaxLandZ - clip.MinLandZ + 128) * RegionData.ZScale);
            if (canvasW * canvasH > MultiPreview.MaxPixels)
            {
                return null;
            }

            bool land = options?.IncludeLand ?? true;
            bool statics = options?.IncludeStatics ?? true;

            var items = new List<(int sortA, int sortZ, int seq, int x, int y, ArtImage img)>();
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            int seq = 0;

            void Add(int cx, int cy, int sortZ, ArtImage img, int px, int py)
            {
                items.Add((cx + cy, sortZ, seq++, px, py, img));
                minX = Math.Min(minX, px);
                minY = Math.Min(minY, py);
                maxX = Math.Max(maxX, px + img.Width);
                maxY = Math.Max(maxY, py + img.Height);
            }

            if (land)
            {
                for (int y = 0; y < clip.Height; y++)
                {
                    for (int x = 0; x < clip.Width; x++)
                    {
                        ushort id = clip.GetLandId(x, y);
                        if (IsoRenderer.IsNoDrawLand(id))
                        {
                            continue;
                        }

                        ArtImage img = art.GetLand(id);
                        if (img == null)
                        {
                            continue;
                        }

                        int z = clip.GetLandZ(x, y);
                        int px = ((x - y) * RegionData.TileHalf) - (img.Width / 2);
                        int py = ((x + y) * RegionData.TileHalf) - (z * RegionData.ZScale) + RegionData.TileSize - img.Height;
                        Add(x, y, z - 1, img, px, py);
                    }
                }
            }

            if (statics)
            {
                foreach (ClipStatic s in clip.Statics)
                {
                    if (options != null && (options.ExcludedIds.Contains(s.Id) || s.Z < options.MinZ || s.Z > options.MaxZ))
                    {
                        continue;
                    }

                    if (IsoRenderer.IsNoDrawStatic(s.Id))
                    {
                        continue;
                    }

                    ArtImage img = art.GetStatic(s.Id, s.Hue);
                    if (img == null)
                    {
                        continue;
                    }

                    int px = ((s.X - s.Y) * RegionData.TileHalf) - (img.Width / 2);
                    int py = ((s.X + s.Y) * RegionData.TileHalf) - (s.Z * RegionData.ZScale) + RegionData.TileSize - img.Height;

                    TileFlag flags = s.Id < TileData.ItemTable.Length ? TileData.ItemTable[s.Id].Flags : TileFlag.None;
                    int height = s.Id < TileData.ItemTable.Length ? TileData.ItemTable[s.Id].Height : 0;
                    int sortZ = s.Z + ((flags & TileFlag.Background) != 0 ? 0 : 1) + (height > 0 ? 1 : 0);
                    Add(s.X, s.Y, sortZ, img, px, py);
                }
            }

            if (items.Count == 0)
            {
                return null;
            }

            int width = maxX - minX, height2 = maxY - minY;
            if ((long)width * height2 > MultiPreview.MaxPixels)
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

        /// <summary>Small picture of the clip for the "Clip" tab</summary>
        public static Bitmap Thumbnail(MultiImage image, Size size, Color background, string emptyText)
        {
            var bmp = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(background);
                if (image?.Bitmap == null)
                {
                    g.DrawString(emptyText, SystemFonts.DefaultFont, Brushes.Gray, 8, 8);
                    return bmp;
                }

                float scale = Math.Min(1f, Math.Min(size.Width / (float)image.Bitmap.Width, size.Height / (float)image.Bitmap.Height));
                int w = Math.Max(1, (int)(image.Bitmap.Width * scale)), h = Math.Max(1, (int)(image.Bitmap.Height * scale));
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image.Bitmap, new Rectangle((size.Width - w) / 2, (size.Height - h) / 2, w, h));
            }

            return bmp;
        }
    }
}
