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
using UoFiddler.Controls.TerrainLibrary.Core;

namespace UoFiddler.Controls.TerrainLibrary.Helpers
{
    /// <summary>How a land tile is created from a texture.</summary>
    public enum LandConversionMode
    {
        /// <summary>Texture turned by 45° (top left corner of the texture = top corner of the diamond), like the client draws it.</summary>
        Isometric = 0,

        /// <summary>Every diamond row samples the full texture width (same method as the Texture Window "Make Tile").</summary>
        RowStretch = 1
    }

    /// <summary>
    /// Image tools for land tiles and textures: bring images to the right size, rotate, mirror,
    /// create a land tile from a texture. All results are new ARGB1555 bitmaps.
    /// </summary>
    public static class TerrainImageTools
    {
        /// <summary>
        /// Returns a new bitmap in the right size for the layer (land 44x44, texture 64x64 / 128x128).
        /// Other sizes are scaled when <paramref name="autoScale"/> is on, otherwise an exception with a
        /// readable message is thrown. Textures are made fully opaque; land tiles are cut to the diamond.
        /// </summary>
        public static Bitmap PrepareForLayer(Image source, TerrainLayer layer, bool autoScale, out bool scaled)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            int target;
            if (layer == TerrainLayer.LandTile)
            {
                target = TerrainCodec.LandSize;
            }
            else if (source.Width == source.Height && TerrainCodec.IsValidTextureSize(source.Width))
            {
                target = source.Width;
            }
            else
            {
                target = Math.Max(source.Width, source.Height) <= 96 ? TerrainCodec.TextureSmall : TerrainCodec.TextureLarge;
            }

            scaled = source.Width != target || source.Height != target;
            if (scaled && !autoScale)
            {
                throw new InvalidOperationException(layer == TerrainLayer.LandTile
                    ? $"Das Bild ist {source.Width}x{source.Height} groß, LandTiles müssen 44x44 sein.\n(Auto-Skalieren ist ausgeschaltet.)"
                    : $"Das Bild ist {source.Width}x{source.Height} groß, Texturen müssen 64x64 oder 128x128 sein.\n(Auto-Skalieren ist ausgeschaltet.)");
            }

            using (var work = new Bitmap(target, target, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(work))
                {
                    g.Clear(Color.Black);
                    g.InterpolationMode = scaled ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(source, new Rectangle(0, 0, target, target));
                }

                ushort[] pixels = ArtLibraryCodec.ToPixels(work);
                for (int y = 0; y < target; ++y)
                {
                    for (int x = 0; x < target; ++x)
                    {
                        int i = (y * target) + x;
                        bool keep = layer == TerrainLayer.Texture || TerrainCodec.IsInsideLand(x, y);
                        pixels[i] = keep ? (ushort)(pixels[i] | 0x8000) : (ushort)0;
                    }
                }

                return ArtLibraryCodec.ToBitmap(pixels, target, target);
            }
        }

        /// <summary>Rotates a square graphic by 90° (clockwise or counter clockwise).</summary>
        public static Bitmap Rotate(Bitmap source, bool clockwise)
        {
            int size = RequireSquare(source);
            ushort[] src = ArtLibraryCodec.ToPixels(source);
            var dst = new ushort[src.Length];
            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    int nx = clockwise ? size - 1 - y : y;
                    int ny = clockwise ? x : size - 1 - x;
                    dst[(ny * size) + nx] = src[(y * size) + x];
                }
            }

            return ArtLibraryCodec.ToBitmap(dst, size, size);
        }

        /// <summary>Mirrors a square graphic horizontally (left/right) or vertically (top/bottom).</summary>
        public static Bitmap Mirror(Bitmap source, bool horizontal)
        {
            int size = RequireSquare(source);
            ushort[] src = ArtLibraryCodec.ToPixels(source);
            var dst = new ushort[src.Length];
            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    int nx = horizontal ? size - 1 - x : x;
                    int ny = horizontal ? y : size - 1 - y;
                    dst[(ny * size) + nx] = src[(y * size) + x];
                }
            }

            return ArtLibraryCodec.ToBitmap(dst, size, size);
        }

        /// <summary>Creates a 44x44 land tile from a texture (or any square image).</summary>
        public static Bitmap LandFromTexture(Bitmap texture, LandConversionMode mode)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            int w = texture.Width;
            int h = texture.Height;
            ushort[] src = ArtLibraryCodec.ToPixels(texture);
            const int size = TerrainCodec.LandSize;
            var dst = new ushort[size * size];

            for (int y = 0; y < size; ++y)
            {
                TerrainCodec.GetLandRow(y, out int startX, out int count);
                for (int x = startX; x < startX + count; ++x)
                {
                    double u, v;
                    if (mode == LandConversionMode.Isometric)
                    {
                        double cx = x + 0.5 - 22.0;
                        double cy = y + 0.5 - 22.0;
                        u = ((cx + cy) / 44.0) + 0.5;
                        v = ((cy - cx) / 44.0) + 0.5;
                    }
                    else
                    {
                        u = count > 1 ? (double)(x - startX) / (count - 1) : 0.5;
                        v = y / 43.0;
                    }

                    dst[(y * size) + x] = (ushort)(Sample(src, w, h, u * (w - 1), v * (h - 1)) | 0x8000);
                }
            }

            return ArtLibraryCodec.ToBitmap(dst, size, size);
        }

        /// <summary>Bilinear sample of an ARGB1555 buffer (5 bit channels).</summary>
        private static ushort Sample(ushort[] src, int w, int h, double fx, double fy)
        {
            fx = Math.Clamp(fx, 0, w - 1);
            fy = Math.Clamp(fy, 0, h - 1);
            int x0 = (int)fx;
            int y0 = (int)fy;
            int x1 = Math.Min(x0 + 1, w - 1);
            int y1 = Math.Min(y0 + 1, h - 1);
            double tx = fx - x0;
            double ty = fy - y0;

            int Channel(int shift)
            {
                double c00 = (src[(y0 * w) + x0] >> shift) & 0x1F;
                double c10 = (src[(y0 * w) + x1] >> shift) & 0x1F;
                double c01 = (src[(y1 * w) + x0] >> shift) & 0x1F;
                double c11 = (src[(y1 * w) + x1] >> shift) & 0x1F;
                double top = c00 + ((c10 - c00) * tx);
                double bottom = c01 + ((c11 - c01) * tx);
                return (int)Math.Round(top + ((bottom - top) * ty));
            }

            return (ushort)((Channel(10) << 10) | (Channel(5) << 5) | Channel(0));
        }

        private static int RequireSquare(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            if (bitmap.Width != bitmap.Height)
            {
                throw new ArgumentException("Only square graphics can be rotated or mirrored.");
            }

            return bitmap.Width;
        }
    }
}
