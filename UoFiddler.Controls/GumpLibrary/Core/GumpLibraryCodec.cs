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
using UoFiddler.Controls.ArtLibrary.Core;

namespace UoFiddler.Controls.GumpLibrary.Core
{
    /// <summary>
    /// Data format of one gump in a gump library (similar to gumpart.mul, but with its own header
    /// so the size is stored with the data and big gumps have no row table limit):
    /// <code>
    /// int32  magic  "GMP1"
    /// uint16 width, uint16 height
    /// int32  row offsets[height]  (in 4 byte runs from the start of the run block)
    /// runs:  uint16 color (ARGB1555, 0 = transparent), uint16 length - every row is complete
    /// </code>
    /// </summary>
    public static class GumpLibraryCodec
    {
        public const int Magic = 0x31504D47; // "GMP1"
        public const int HeaderLength = 8;
        public const int MaxSize = 4096;

        public static bool TryReadSize(ReadOnlySpan<byte> data, out int width, out int height)
        {
            width = height = 0;
            if (data.Length < HeaderLength || BinaryPrimitives.ReadInt32LittleEndian(data) != Magic)
            {
                return false;
            }

            width = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(4));
            height = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(6));
            return width > 0 && height > 0 && width <= MaxSize && height <= MaxSize;
        }

        public static bool IsValid(ReadOnlySpan<byte> data) => Decode(data, out _, out _) != null;

        /// <summary>Pixels without the alpha bit are stored as transparent (0).</summary>
        public static byte[] Encode(ushort[] pixels, int width, int height)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }

            if (width <= 0 || height <= 0 || width > MaxSize || height > MaxSize)
            {
                throw new ArgumentException($"Ungültige Gump-Größe {width} x {height} (maximal {MaxSize} x {MaxSize}).");
            }

            if (pixels.Length < width * height)
            {
                throw new ArgumentException("Pixel buffer is smaller than width * height.", nameof(pixels));
            }

            var runs = new List<uint>(height * 4);
            var rowOffsets = new int[height];
            for (int y = 0; y < height; ++y)
            {
                rowOffsets[y] = runs.Count;
                int row = y * width;
                int x = 0;
                while (x < width)
                {
                    ushort color = Normalize(pixels[row + x]);
                    int length = 1;
                    while (x + length < width && length < ushort.MaxValue && Normalize(pixels[row + x + length]) == color)
                    {
                        ++length;
                    }

                    runs.Add(color | ((uint)length << 16));
                    x += length;
                }
            }

            var result = new byte[HeaderLength + (height * 4) + (runs.Count * 4)];
            Span<byte> span = result;
            BinaryPrimitives.WriteInt32LittleEndian(span, Magic);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(4), (ushort)width);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(6), (ushort)height);
            int pos = HeaderLength;
            for (int y = 0; y < height; ++y, pos += 4)
            {
                BinaryPrimitives.WriteInt32LittleEndian(span.Slice(pos), rowOffsets[y]);
            }

            foreach (uint run in runs)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(pos), run);
                pos += 4;
            }

            return result;
        }

        /// <summary>Returns the ARGB1555 pixels or null when the data is malformed.</summary>
        public static ushort[] Decode(ReadOnlySpan<byte> data, out int width, out int height)
        {
            if (!TryReadSize(data, out width, out height))
            {
                return null;
            }

            int runStart = HeaderLength + (height * 4);
            if (data.Length < runStart || (data.Length - runStart) % 4 != 0)
            {
                return null;
            }

            int runCount = (data.Length - runStart) / 4;
            var pixels = new ushort[width * height];
            for (int y = 0; y < height; ++y)
            {
                int run = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(HeaderLength + (y * 4)));
                int x = 0;
                int row = y * width;
                while (x < width)
                {
                    if (run < 0 || run >= runCount)
                    {
                        return null;
                    }

                    uint value = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(runStart + (run * 4)));
                    ushort color = (ushort)(value & 0xFFFF);
                    int length = (int)(value >> 16);
                    if (length <= 0 || x + length > width)
                    {
                        return null;
                    }

                    if (color != 0)
                    {
                        pixels.AsSpan(row + x, length).Fill(color);
                    }

                    x += length;
                    ++run;
                }
            }

            return pixels;
        }

        public static byte[] Encode(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            return Encode(ArtLibraryCodec.ToPixels(bitmap), bitmap.Width, bitmap.Height);
        }

        public static Bitmap DecodeBitmap(ReadOnlySpan<byte> data)
        {
            ushort[] pixels = Decode(data, out int width, out int height);
            return pixels == null ? null : ArtLibraryCodec.ToBitmap(pixels, width, height);
        }

        private static ushort Normalize(ushort pixel) => (pixel & 0x8000) != 0 ? (ushort)(pixel | 0x8000) : (ushort)0;
    }
}
