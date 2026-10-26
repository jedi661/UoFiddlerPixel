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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Ultima;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Image as 32 bit ARGB pixel array (alpha 0 = transparent). Used by the software renderer.
    /// </summary>
    public sealed class ArtImage
    {
        public ArtImage(int width, int height, int[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public int Width { get; }
        public int Height { get; }
        public int[] Pixels { get; }

        public bool IsTransparentAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                return true;
            }

            return (Pixels[(y * Width) + x] >> 24) == 0;
        }

        public static ArtImage FromBitmap(Bitmap bmp)
        {
            if (bmp == null || bmp.Width <= 0 || bmp.Height <= 0)
            {
                return null;
            }

            var pixels = new int[bmp.Width * bmp.Height];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Marshal.Copy(bd.Scan0 + (y * bd.Stride), pixels, y * bmp.Width, bmp.Width);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            return new ArtImage(bmp.Width, bmp.Height, pixels);
        }
    }

    /// <summary>
    /// Caches land art, static art (also hued) and texmaps as <see cref="ArtImage"/>.
    /// Not thread safe, use it from the UI thread only (the Ultima file access is not thread safe either).
    /// </summary>
    public sealed class ArtPixelCache
    {
        private readonly Dictionary<int, ArtImage> _land = new Dictionary<int, ArtImage>();
        private readonly Dictionary<long, ArtImage> _statics = new Dictionary<long, ArtImage>();
        private readonly Dictionary<int, ArtImage> _textures = new Dictionary<int, ArtImage>();

        // marker for "not available" (null values can not be distinguished from missing keys otherwise)
        private static readonly ArtImage _missing = new ArtImage(0, 0, Array.Empty<int>());

        public void Clear()
        {
            _land.Clear();
            _statics.Clear();
            _textures.Clear();
        }

        public ArtImage GetLand(int id)
        {
            if (!_land.TryGetValue(id, out ArtImage image))
            {
                image = Load(() => Art.GetLand(id));
                _land[id] = image;
            }

            return image == _missing ? null : image;
        }

        public ArtImage GetTexture(int textureId)
        {
            if (textureId <= 0)
            {
                return null;
            }

            if (!_textures.TryGetValue(textureId, out ArtImage image))
            {
                image = Load(() => Textures.GetTexture(textureId));
                _textures[textureId] = image;
            }

            return image == _missing ? null : image;
        }

        /// <summary>
        /// Returns the static art with the hue applied (hue = raw value of the statics file, 0 = no hue).
        /// </summary>
        public ArtImage GetStatic(int id, int hue)
        {
            int hueIndex = hue & 0x3FFF;
            long key = ((long)id << 16) | (uint)hueIndex;
            if (_statics.TryGetValue(key, out ArtImage image))
            {
                return image == _missing ? null : image;
            }

            if (hueIndex == 0)
            {
                image = Load(() => Art.GetStatic(id));
            }
            else
            {
                ArtImage plain = GetStatic(id, 0);
                image = plain == null ? _missing : ApplyHue(plain, hueIndex - 1, IsPartialHue(id));
            }

            _statics[key] = image;
            return image == _missing ? null : image;
        }

        private static bool IsPartialHue(int id)
        {
            ItemData[] table = TileData.ItemTable;
            return table != null && id >= 0 && id < table.Length && (table[id].Flags & TileFlag.PartialHue) != 0;
        }

        private static ArtImage Load(Func<Bitmap> loader)
        {
            try
            {
                return ArtImage.FromBitmap(loader()) ?? _missing;
            }
            catch (Exception)
            {
                return _missing;
            }
        }

        private static ArtImage ApplyHue(ArtImage source, int hueIndex, bool onlyGray)
        {
            Hue hue;
            try
            {
                hue = Hues.GetHue(hueIndex);
            }
            catch (Exception)
            {
                return source;
            }

            if (hue?.Colors == null)
            {
                return source;
            }

            var colors = new int[32];
            for (int i = 0; i < 32; i++)
            {
                colors[i] = Color555ToArgb(hue.Colors[i]);
            }

            var pixels = (int[])source.Pixels.Clone();
            for (int i = 0; i < pixels.Length; i++)
            {
                int c = pixels[i];
                if ((c >> 24) == 0)
                {
                    continue;
                }

                int r = (c >> 16) & 0xFF;
                int g = (c >> 8) & 0xFF;
                int b = c & 0xFF;

                if (onlyGray && (r != g || r != b))
                {
                    continue;
                }

                pixels[i] = colors[r >> 3];
            }

            return new ArtImage(source.Width, source.Height, pixels);
        }

        public static int Color555ToArgb(ushort c)
        {
            int r = ((c >> 10) & 0x1F) * 255 / 31;
            int g = ((c >> 5) & 0x1F) * 255 / 31;
            int b = (c & 0x1F) * 255 / 31;
            return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
        }
    }
}
