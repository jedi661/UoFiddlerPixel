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
using System.IO;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Imaging
{
    /// <summary>
    /// Reader / writer for Truevision TGA images (used for the effect textures of the Enhanced Client).
    /// Supports uncompressed and RLE images with 8 (gray or color mapped), 15/16, 24 and 32 bit.
    /// </summary>
    public static class TgaImage
    {
        /// <summary>
        /// Checks the header: TGA has no magic number, so the fields are validated instead.
        /// </summary>
        public static bool LooksLikeTga(byte[] data)
        {
            if (data == null || data.Length < 18)
            {
                return false;
            }

            byte colorMapType = data[1];
            byte imageType = data[2];
            int width = BitConverter.ToUInt16(data, 12);
            int height = BitConverter.ToUInt16(data, 14);
            byte bpp = data[16];

            if (colorMapType > 1 || width == 0 || height == 0)
            {
                return false;
            }
            switch (imageType)
            {
                case 1:
                case 2:
                case 3:
                case 9:
                case 10:
                case 11:
                    break;
                default:
                    return false;
            }
            return bpp == 8 || bpp == 15 || bpp == 16 || bpp == 24 || bpp == 32;
        }

        /// <summary>
        /// Stricter test for data without a file name: the pixel data must fill the file
        /// (uncompressed) or the RLE packets must end close to the end of the data.
        /// </summary>
        public static bool IsLikelyTga(byte[] data)
        {
            if (!LooksLikeTga(data) || (data[17] & 0xC0) != 0)
            {
                return false;
            }

            int width = BitConverter.ToUInt16(data, 12);
            int height = BitConverter.ToUInt16(data, 14);
            int bytesPerPixel = (data[16] + 7) / 8;
            int colorMapBytes = data[1] == 1 ? BitConverter.ToUInt16(data, 5) * ((data[7] + 7) / 8) : 0;
            long p = 18 + data[0] + colorMapBytes;
            long pixels = (long)width * height;

            if (data[2] < 9)
            {
                long end = p + (pixels * bytesPerPixel);
                return end <= data.Length && data.Length - end <= 1024;
            }

            long done = 0;
            while (done < pixels)
            {
                if (p >= data.Length)
                {
                    return false;
                }
                byte header = data[p++];
                int run = (header & 0x7F) + 1;
                p += (header & 0x80) != 0 ? bytesPerPixel : (long)run * bytesPerPixel;
                done += run;
            }
            return done == pixels && p <= data.Length && data.Length - p <= 1024;
        }

        public static DdsImage Decode(byte[] data)
        {
            if (!LooksLikeTga(data))
            {
                throw new InvalidDataException("Not a TGA image.");
            }

            int idLength = data[0];
            byte colorMapType = data[1];
            byte imageType = data[2];
            int colorMapStart = BitConverter.ToUInt16(data, 3);
            int colorMapLength = BitConverter.ToUInt16(data, 5);
            int colorMapBits = data[7];
            int width = BitConverter.ToUInt16(data, 12);
            int height = BitConverter.ToUInt16(data, 14);
            int bpp = data[16];
            byte descriptor = data[17];
            bool topToBottom = (descriptor & 0x20) != 0;
            bool rightToLeft = (descriptor & 0x10) != 0;
            bool rle = imageType >= 9;
            int baseType = imageType & 7;

            int p = 18 + idLength;

            byte[] palette = null;
            if (colorMapType == 1)
            {
                int entryBytes = (colorMapBits + 7) / 8;
                palette = new byte[(colorMapStart + colorMapLength) * 4];
                for (int i = 0; i < colorMapLength; i++)
                {
                    ReadColor(data, p + (i * entryBytes), colorMapBits, palette, (colorMapStart + i) * 4, false);
                }
                p += colorMapLength * entryBytes;
            }

            int bytesPerPixel = (bpp + 7) / 8;
            var pixels = new byte[width * height * 4];
            int count = width * height;
            int index = 0;

            void Put(int src)
            {
                int x = index % width;
                int y = index / width;
                if (!topToBottom)
                {
                    y = height - 1 - y;
                }
                if (rightToLeft)
                {
                    x = width - 1 - x;
                }
                int dst = ((y * width) + x) * 4;

                if (baseType == 1 && palette != null)
                {
                    int entry = bytesPerPixel == 2 ? BitConverter.ToUInt16(data, src) : data[src];
                    if (entry * 4 + 3 < palette.Length)
                    {
                        Buffer.BlockCopy(palette, entry * 4, pixels, dst, 4);
                    }
                }
                else if (baseType == 3)
                {
                    byte v = data[src];
                    pixels[dst] = pixels[dst + 1] = pixels[dst + 2] = v;
                    pixels[dst + 3] = bytesPerPixel == 2 ? data[src + 1] : (byte)255;
                }
                else
                {
                    ReadColor(data, src, bpp, pixels, dst, false);
                }
                index++;
            }

            if (!rle)
            {
                while (index < count && p + bytesPerPixel <= data.Length)
                {
                    Put(p);
                    p += bytesPerPixel;
                }
            }
            else
            {
                while (index < count && p < data.Length)
                {
                    byte header = data[p++];
                    int run = (header & 0x7F) + 1;
                    if ((header & 0x80) != 0)
                    {
                        if (p + bytesPerPixel > data.Length)
                        {
                            break;
                        }
                        for (int i = 0; i < run && index < count; i++)
                        {
                            Put(p);
                        }
                        p += bytesPerPixel;
                    }
                    else
                    {
                        for (int i = 0; i < run && index < count && p + bytesPerPixel <= data.Length; i++)
                        {
                            Put(p);
                            p += bytesPerPixel;
                        }
                    }
                }
            }

            // Some writers store 0 in every alpha byte although the image has no transparency
            bool anyAlpha = false;
            for (int i = 3; i < pixels.Length; i += 4)
            {
                if (pixels[i] != 0)
                {
                    anyAlpha = true;
                    break;
                }
            }
            if (!anyAlpha)
            {
                for (int i = 3; i < pixels.Length; i += 4)
                {
                    pixels[i] = 255;
                }
            }

            string name = $"TGA {(baseType == 1 ? "indexed" : baseType == 3 ? "gray" : bpp + " bit")}{(rle ? " RLE" : string.Empty)}";
            return new DdsImage(width, height, pixels, name, 1, DdsSaveFormat.A8R8G8B8);
        }

        private static void ReadColor(byte[] data, int src, int bits, byte[] dst, int o, bool ignoreAlpha)
        {
            switch (bits)
            {
                case 32:
                    dst[o] = data[src];
                    dst[o + 1] = data[src + 1];
                    dst[o + 2] = data[src + 2];
                    dst[o + 3] = ignoreAlpha ? (byte)255 : data[src + 3];
                    break;
                case 24:
                    dst[o] = data[src];
                    dst[o + 1] = data[src + 1];
                    dst[o + 2] = data[src + 2];
                    dst[o + 3] = 255;
                    break;
                case 15:
                case 16:
                    int v = BitConverter.ToUInt16(data, src);
                    int r = (v >> 10) & 31, g = (v >> 5) & 31, b = v & 31;
                    dst[o] = (byte)((b << 3) | (b >> 2));
                    dst[o + 1] = (byte)((g << 3) | (g >> 2));
                    dst[o + 2] = (byte)((r << 3) | (r >> 2));
                    dst[o + 3] = 255;
                    break;
                default:
                    dst[o] = dst[o + 1] = dst[o + 2] = data[src];
                    dst[o + 3] = 255;
                    break;
            }
        }

        /// <summary>
        /// Writes an uncompressed 32 bit TGA (top to bottom) from BGRA pixels.
        /// </summary>
        public static byte[] Encode(byte[] bgra, int width, int height)
        {
            var output = new byte[18 + (width * height * 4)];
            output[2] = 2;
            BitConverter.GetBytes((ushort)width).CopyTo(output, 12);
            BitConverter.GetBytes((ushort)height).CopyTo(output, 14);
            output[16] = 32;
            output[17] = 0x28; // 8 alpha bits, top-left origin
            Buffer.BlockCopy(bgra, 0, output, 18, width * height * 4);
            return output;
        }
    }
}
