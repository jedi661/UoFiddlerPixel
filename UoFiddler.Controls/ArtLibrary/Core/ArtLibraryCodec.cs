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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace UoFiddler.Controls.ArtLibrary.Core
{
    /// <summary>
    /// Encodes/decodes item (static) art in exactly the same byte layout as the original art.mul,
    /// so graphics can be copied 1:1 between a library and art.mul without re-encoding.
    /// <para>Layout (little endian, ushort = 16 bit):</para>
    /// <code>
    /// int32    Flag        unused by the client, written as 1234 (same as UoFiddler)
    /// ushort   Width
    /// ushort   Height
    /// ushort[] Lookup      one per row: start of the row in ushorts, counted from the end of this table
    /// rows:    repeated   (ushort xOffset, ushort run, ushort[run] pixels) terminated by (0, 0)
    ///          xOffset is relative to the end of the previous run of the same row.
    /// </code>
    /// Pixels are ARGB1555. Only opaque pixels (alpha bit 0x8000 set) are stored; they are stored
    /// without the alpha bit and get it back when decoded.
    /// <para>This is an independent copy of the logic in Ultima.Art so the original class stays untouched.</para>
    /// </summary>
    public static class ArtLibraryCodec
    {
        public const int HeaderFlag = 1234;
        private const int HeaderBytes = 8;

        /// <summary>
        /// Reads width and height of encoded data without decoding the image.
        /// </summary>
        public static bool TryReadSize(ReadOnlySpan<byte> data, out int width, out int height)
        {
            width = height = 0;
            if (data.Length < HeaderBytes)
            {
                return false;
            }

            width = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(4));
            height = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(6));

            return width > 0 && height > 0 && data.Length >= HeaderBytes + (height * 2);
        }

        /// <summary>
        /// Checks whether <paramref name="data"/> is a complete, decodable item art graphic.
        /// </summary>
        public static bool IsValid(ReadOnlySpan<byte> data)
        {
            return Decode(data, out _, out _) != null;
        }

        #region [ Pixel buffer level ]

        /// <summary>
        /// Encodes an ARGB1555 pixel buffer (row major, width * height) into art.mul item format.
        /// </summary>
        public static byte[] Encode(ushort[] pixels, int width, int height)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (width <= 0 || height <= 0 || width > ushort.MaxValue || height > ushort.MaxValue)
            {
                throw new ArgumentException($"Invalid image size {width} x {height}.");
            }

            if (pixels.Length < width * height)
            {
                throw new ArgumentException("Pixel buffer is smaller than width * height.", nameof(pixels));
            }

            // Everything is collected as ushorts; the first 4 ushorts are the header, then the lookup table.
            var words = new List<ushort>((int)Math.Min(((long)width * height) + (height * 4L) + 4, 1 << 20))
            {
                (ushort)(HeaderFlag & 0xFFFF),
                (ushort)(HeaderFlag >> 16),
                (ushort)width,
                (ushort)height
            };

            int lookupStart = words.Count;
            for (int y = 0; y < height; ++y)
            {
                words.Add(0);
            }

            int dataStart = words.Count;

            for (int y = 0; y < height; ++y)
            {
                int rowStart = words.Count - dataStart;
                if (rowStart > ushort.MaxValue)
                {
                    throw new ArgumentException(
                        $"Image {width} x {height} has too much visible content for the art.mul format (row table overflow).");
                }

                words[lookupStart + y] = (ushort)rowStart;

                int rowOffset = y * width;
                int x = 0;
                while (x < width)
                {
                    // first opaque pixel
                    int i = x;
                    while (i < width && !IsOpaque(pixels[rowOffset + i]))
                    {
                        ++i;
                    }

                    if (i >= width)
                    {
                        break;
                    }

                    // first transparent pixel after it
                    int j = i + 1;
                    while (j < width && IsOpaque(pixels[rowOffset + j]))
                    {
                        ++j;
                    }

                    words.Add((ushort)(i - x)); // xOffset
                    words.Add((ushort)(j - i)); // run

                    for (int p = i; p < j; ++p)
                    {
                        words.Add((ushort)(pixels[rowOffset + p] & 0x7FFF));
                    }

                    x = j;
                }

                words.Add(0); // xOffset
                words.Add(0); // run
            }

            var result = new byte[words.Count * 2];
            for (int i = 0; i < words.Count; ++i)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(i * 2), words[i]);
            }

            return result;
        }

        /// <summary>
        /// Decodes art.mul item format into an ARGB1555 pixel buffer (row major).
        /// Returns null when the data is malformed.
        /// </summary>
        public static ushort[] Decode(ReadOnlySpan<byte> data, out int width, out int height)
        {
            if (!TryReadSize(data, out width, out height))
            {
                return null;
            }

            int wordCount = data.Length / 2;
            int dataStart = 4 + height;
            var pixels = new ushort[width * height];

            for (int y = 0; y < height; ++y)
            {
                int pos = dataStart + ReadWord(data, 4 + y);
                int x = 0;
                int rowOffset = y * width;

                while (true)
                {
                    if (pos + 1 >= wordCount)
                    {
                        return null;
                    }

                    int xOffset = ReadWord(data, pos++);
                    int run = ReadWord(data, pos++);

                    if (xOffset + run == 0)
                    {
                        break;
                    }

                    x += xOffset;
                    if (x + run > width || pos + run > wordCount)
                    {
                        return null;
                    }

                    for (int i = 0; i < run; ++i)
                    {
                        pixels[rowOffset + x++] = (ushort)(ReadWord(data, pos++) | 0x8000);
                    }
                }
            }

            return pixels;
        }

        private static bool IsOpaque(ushort pixel) => (pixel & 0x8000) != 0;

        private static ushort ReadWord(ReadOnlySpan<byte> data, int wordIndex)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(wordIndex * 2));
        }

        #endregion

        #region [ Bitmap level ]

        /// <summary>
        /// Encodes a bitmap of any pixel format. Transparent pixels (alpha below 50%) are not stored.
        /// </summary>
        public static byte[] Encode(Bitmap bitmap)
        {
            ushort[] pixels = ToPixels(bitmap);
            return Encode(pixels, bitmap.Width, bitmap.Height);
        }

        /// <summary>
        /// Decodes to a new Format16bppArgb1555 bitmap, or null when the data is malformed.
        /// </summary>
        public static Bitmap DecodeBitmap(ReadOnlySpan<byte> data)
        {
            ushort[] pixels = Decode(data, out int width, out int height);
            return pixels == null ? null : ToBitmap(pixels, width, height);
        }

        /// <summary>
        /// Copies a bitmap into an ARGB1555 pixel buffer (row major).
        /// </summary>
        public static unsafe ushort[] ToPixels(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            int width = bitmap.Width;
            int height = bitmap.Height;
            var pixels = new ushort[width * height];

            // LockBits converts any source format to ARGB1555 (alpha below 50% becomes transparent)
            BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            try
            {
                fixed (ushort* target = pixels)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        var line = (ushort*)((byte*)bd.Scan0 + ((long)y * bd.Stride));
                        Buffer.MemoryCopy(line, target + (y * width), width * 2L, width * 2L);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bd);
            }

            return pixels;
        }

        /// <summary>
        /// Creates a Format16bppArgb1555 bitmap from an ARGB1555 pixel buffer (row major).
        /// </summary>
        public static unsafe Bitmap ToBitmap(ushort[] pixels, int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            try
            {
                fixed (ushort* source = pixels)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        var line = (ushort*)((byte*)bd.Scan0 + ((long)y * bd.Stride));
                        Buffer.MemoryCopy(source + (y * width), line, width * 2L, width * 2L);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bd);
            }

            return bitmap;
        }

        #endregion
    }
}
