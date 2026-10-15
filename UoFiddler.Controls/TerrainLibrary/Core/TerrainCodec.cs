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
using System.Buffers.Binary;
using System.Drawing;
using UoFiddler.Controls.ArtLibrary.Core;

namespace UoFiddler.Controls.TerrainLibrary.Core
{
    /// <summary>
    /// Encodes/decodes land tiles and textures in exactly the byte layout of the original files, so
    /// graphics can be copied 1:1 between the library and art.mul / texmaps.mul.
    /// <para><b>Land tile</b> (art.mul index 0x0000-0x3FFF): 44x44 diamond, 1012 pixels (2024 bytes), no header.
    /// Rows 0..21 hold 2,4,..,44 pixels starting at x = 21 - y, rows 22..43 hold 44,42,..,2 pixels
    /// starting at x = y - 22. Pixels are ARGB1555 stored without the alpha bit.</para>
    /// <para><b>Texture</b> (texmaps.mul): square 64x64 (8192 bytes) or 128x128 (32768 bytes), no header,
    /// every pixel stored without the alpha bit (textures are always opaque).</para>
    /// </summary>
    public static class TerrainCodec
    {
        public const int LandSize = 44;
        public const int LandPixelCount = 1012;
        public const int LandDataLength = LandPixelCount * 2;

        public const int TextureSmall = 64;
        public const int TextureLarge = 128;

        /// <summary>Start x and pixel count of a land tile row (same as Ultima.Art).</summary>
        public static void GetLandRow(int y, out int startX, out int count)
        {
            if (y < 22)
            {
                startX = 21 - y;
                count = 2 + (y * 2);
            }
            else
            {
                startX = y - 22;
                count = 44 - ((y - 22) * 2);
            }
        }

        /// <summary>True when (x, y) lies inside the land tile diamond.</summary>
        public static bool IsInsideLand(int x, int y)
        {
            if (y < 0 || y >= LandSize)
            {
                return false;
            }

            GetLandRow(y, out int startX, out int count);
            return x >= startX && x < startX + count;
        }

        public static bool IsValidTextureSize(int size) => size == TextureSmall || size == TextureLarge;

        /// <summary>Largest land entry accepted (original files may pad land tiles, e.g. to 2048 bytes).</summary>
        public const int LandMaxDataLength = 4096;

        public static bool IsValid(TerrainLayer layer, ReadOnlySpan<byte> data)
        {
            return layer == TerrainLayer.LandTile
                ? data.Length >= LandDataLength && data.Length <= LandMaxDataLength
                : data.Length == TextureSmall * TextureSmall * 2 || data.Length == TextureLarge * TextureLarge * 2;
        }

        /// <summary>Width/height of encoded data (land 44, texture 64 or 128), 0 when invalid.</summary>
        public static int GetSize(TerrainLayer layer, int dataLength)
        {
            if (layer == TerrainLayer.LandTile)
            {
                return dataLength >= LandDataLength && dataLength <= LandMaxDataLength ? LandSize : 0;
            }

            if (dataLength == TextureSmall * TextureSmall * 2)
            {
                return TextureSmall;
            }

            return dataLength == TextureLarge * TextureLarge * 2 ? TextureLarge : 0;
        }

        #region [ Land ]

        /// <summary>Encodes a 44x44 ARGB1555 pixel buffer (pixels outside the diamond are ignored).</summary>
        public static byte[] EncodeLand(ushort[] pixels)
        {
            if (pixels == null || pixels.Length < LandSize * LandSize)
            {
                throw new ArgumentException("A land tile needs 44 x 44 pixels.", nameof(pixels));
            }

            var data = new byte[LandDataLength];
            int pos = 0;
            for (int y = 0; y < LandSize; ++y)
            {
                GetLandRow(y, out int startX, out int count);
                for (int x = startX; x < startX + count; ++x)
                {
                    BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(pos), (ushort)(pixels[(y * LandSize) + x] & 0x7FFF));
                    pos += 2;
                }
            }

            return data;
        }

        /// <summary>Decodes to a 44x44 ARGB1555 pixel buffer, outside of the diamond transparent. Null when invalid.</summary>
        public static ushort[] DecodeLand(ReadOnlySpan<byte> data)
        {
            if (data.Length < LandDataLength)
            {
                return null;
            }

            var pixels = new ushort[LandSize * LandSize];
            int pos = 0;
            for (int y = 0; y < LandSize; ++y)
            {
                GetLandRow(y, out int startX, out int count);
                for (int x = startX; x < startX + count; ++x)
                {
                    pixels[(y * LandSize) + x] = (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(pos)) | 0x8000);
                    pos += 2;
                }
            }

            return pixels;
        }

        #endregion

        #region [ Texture ]

        /// <summary>Encodes a square 64x64 or 128x128 ARGB1555 pixel buffer.</summary>
        public static byte[] EncodeTexture(ushort[] pixels, int size)
        {
            if (!IsValidTextureSize(size))
            {
                throw new ArgumentException($"Texturen müssen 64x64 oder 128x128 groß sein (nicht {size}x{size}).");
            }

            if (pixels == null || pixels.Length < size * size)
            {
                throw new ArgumentException("Pixel buffer is too small.", nameof(pixels));
            }

            var data = new byte[size * size * 2];
            for (int i = 0; i < size * size; ++i)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(i * 2), (ushort)(pixels[i] & 0x7FFF));
            }

            return data;
        }

        public static ushort[] DecodeTexture(ReadOnlySpan<byte> data, out int size)
        {
            size = GetSize(TerrainLayer.Texture, data.Length);
            if (size == 0)
            {
                return null;
            }

            var pixels = new ushort[size * size];
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = (ushort)(BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(i * 2)) | 0x8000);
            }

            return pixels;
        }

        #endregion

        #region [ Bitmap level ]

        /// <summary>
        /// Encodes a bitmap that already has the right size (land 44x44, texture 64x64 / 128x128).
        /// Use TerrainImageTools.PrepareForLayer to scale other sizes first.
        /// </summary>
        public static byte[] Encode(TerrainLayer layer, Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            ushort[] pixels = ArtLibraryCodec.ToPixels(bitmap);
            if (layer == TerrainLayer.LandTile)
            {
                if (bitmap.Width != LandSize || bitmap.Height != LandSize)
                {
                    throw new ArgumentException($"LandTiles müssen 44x44 groß sein (nicht {bitmap.Width}x{bitmap.Height}).");
                }

                return EncodeLand(pixels);
            }

            if (bitmap.Width != bitmap.Height)
            {
                throw new ArgumentException($"Texturen müssen quadratisch sein (nicht {bitmap.Width}x{bitmap.Height}).");
            }

            return EncodeTexture(pixels, bitmap.Width);
        }

        /// <summary>Decodes to a new Format16bppArgb1555 bitmap, or null.</summary>
        public static Bitmap Decode(TerrainLayer layer, ReadOnlySpan<byte> data)
        {
            if (layer == TerrainLayer.LandTile)
            {
                ushort[] land = DecodeLand(data);
                return land == null ? null : ArtLibraryCodec.ToBitmap(land, LandSize, LandSize);
            }

            ushort[] texture = DecodeTexture(data, out int size);
            return texture == null ? null : ArtLibraryCodec.ToBitmap(texture, size, size);
        }

        #endregion
    }
}
