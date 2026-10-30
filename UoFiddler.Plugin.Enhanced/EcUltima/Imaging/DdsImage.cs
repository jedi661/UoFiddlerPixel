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
    /// Formats the plugin can write.
    /// </summary>
    public enum DdsSaveFormat
    {
        /// <summary>Uncompressed 32 bit with alpha (largest, lossless).</summary>
        A8R8G8B8,
        /// <summary>BC1 – 4 bit per pixel, 1 bit alpha.</summary>
        Dxt1,
        /// <summary>BC2 – explicit 4 bit alpha.</summary>
        Dxt3,
        /// <summary>BC3 – interpolated alpha (usual format of the Enhanced Client textures).</summary>
        Dxt5
    }

    /// <summary>
    /// Decoded DDS image (top mip level) as 32 bit BGRA pixels (same memory layout as Format32bppArgb).
    /// </summary>
    public sealed class DdsImage
    {
        private const uint DdsMagic = 0x20534444; // "DDS "

        private const uint DdpfAlphaPixels = 0x1;
        private const uint DdpfAlpha = 0x2;
        private const uint DdpfFourCc = 0x4;
        private const uint DdpfRgb = 0x40;
        private const uint DdpfLuminance = 0x20000;

        public int Width { get; }
        public int Height { get; }

        /// <summary>
        /// BGRA pixels, 4 bytes per pixel, rows top to bottom.
        /// </summary>
        public byte[] Pixels { get; }

        /// <summary>
        /// Name of the source format (e.g. "DXT5", "A8R8G8B8").
        /// </summary>
        public string FormatName { get; }

        public int MipMapCount { get; }

        /// <summary>
        /// The save format closest to the source format (used when replacing an image).
        /// </summary>
        public DdsSaveFormat SuggestedSaveFormat { get; }

        public DdsImage(int width, int height, byte[] pixels, string formatName = "BGRA", int mipMapCount = 1, DdsSaveFormat suggested = DdsSaveFormat.Dxt5)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
            FormatName = formatName;
            MipMapCount = mipMapCount;
            SuggestedSaveFormat = suggested;
        }

        public static bool IsDds(byte[] data)
        {
            return data != null && data.Length >= 128 && BitConverter.ToUInt32(data, 0) == DdsMagic;
        }

        /// <summary>
        /// Reads only the header (format, size, mip maps) without decoding pixels.
        /// </summary>
        public static bool TryReadInfo(byte[] data, out int width, out int height, out string format, out int mipMaps)
        {
            width = height = mipMaps = 0;
            format = null;
            if (!IsDds(data))
            {
                return false;
            }

            height = BitConverter.ToInt32(data, 12);
            width = BitConverter.ToInt32(data, 16);
            mipMaps = Math.Max(1, BitConverter.ToInt32(data, 28));
            format = DescribeFormat(data);
            return true;
        }

        private static string DescribeFormat(byte[] data)
        {
            uint pfFlags = BitConverter.ToUInt32(data, 80);
            if ((pfFlags & DdpfFourCc) != 0)
            {
                string fourCc = FourCcToString(BitConverter.ToUInt32(data, 84));
                if (fourCc == "DX10" && data.Length >= 148)
                {
                    return "DX10/DXGI " + BitConverter.ToUInt32(data, 128);
                }
                return fourCc;
            }

            int bits = BitConverter.ToInt32(data, 88);
            if ((pfFlags & DdpfRgb) != 0)
            {
                return ((pfFlags & DdpfAlphaPixels) != 0 ? "ARGB" : "RGB") + bits;
            }
            if ((pfFlags & DdpfLuminance) != 0)
            {
                return "L" + bits;
            }
            if ((pfFlags & DdpfAlpha) != 0)
            {
                return "A" + bits;
            }
            return "Unknown";
        }

        /// <summary>
        /// Block size in bytes (8 or 16) of DXT1/DXT3/DXT5 files, 0 for other formats.
        /// </summary>
        private static int BlockSize(byte[] data)
        {
            if ((BitConverter.ToUInt32(data, 80) & DdpfFourCc) == 0)
            {
                return 0;
            }
            switch (FourCcToString(BitConverter.ToUInt32(data, 84)))
            {
                case "DXT1":
                    return 8;
                case "DXT2":
                case "DXT3":
                case "DXT4":
                case "DXT5":
                    return 16;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Decodes one mip level (0 = full size). Large map images are shown with a smaller level.
        /// </summary>
        public static DdsImage DecodeMip(byte[] data, int level)
        {
            if (!IsDds(data))
            {
                throw new InvalidDataException("Not a DDS file.");
            }

            int blockSize = BlockSize(data);
            int mipMaps = Math.Max(1, BitConverter.ToInt32(data, 28));
            if (level <= 0 || blockSize == 0 || level >= mipMaps)
            {
                return Decode(data);
            }

            int width = BitConverter.ToInt32(data, 16);
            int height = BitConverter.ToInt32(data, 12);
            long offset = 128;
            for (int l = 0; l < level; l++)
            {
                offset += (long)Math.Max(1, (width + 3) / 4) * Math.Max(1, (height + 3) / 4) * blockSize;
                width = Math.Max(1, width / 2);
                height = Math.Max(1, height / 2);
            }

            int size = Math.Max(1, (width + 3) / 4) * Math.Max(1, (height + 3) / 4) * blockSize;
            var copy = new byte[128 + size];
            Buffer.BlockCopy(data, 0, copy, 0, 128);
            Buffer.BlockCopy(data, (int)offset, copy, 128, Math.Min(size, data.Length - (int)offset));
            BitConverter.GetBytes(height).CopyTo(copy, 12);
            BitConverter.GetBytes(width).CopyTo(copy, 16);
            BitConverter.GetBytes(1).CopyTo(copy, 28);
            return Decode(copy);
        }

        /// <summary>
        /// Decodes only a part of the full size image (fast for block compressed images: only the needed blocks are read).
        /// </summary>
        public static DdsImage DecodeRegion(byte[] data, int x, int y, int regionWidth, int regionHeight)
        {
            int width = BitConverter.ToInt32(data, 16);
            int height = BitConverter.ToInt32(data, 12);
            int blockSize = BlockSize(data);

            DdsImage source;
            int originX, originY;
            if (blockSize == 0)
            {
                source = Decode(data);
                originX = 0;
                originY = 0;
            }
            else
            {
                int bx0 = Math.Max(0, x / 4), by0 = Math.Max(0, y / 4);
                int bx1 = Math.Min((width + 3) / 4, ((x + regionWidth) + 3) / 4);
                int by1 = Math.Min((height + 3) / 4, ((y + regionHeight) + 3) / 4);
                int blocksX = Math.Max(1, bx1 - bx0), blocksY = Math.Max(1, by1 - by0);
                int rowBlocks = (width + 3) / 4;

                var copy = new byte[128 + (blocksX * blocksY * blockSize)];
                Buffer.BlockCopy(data, 0, copy, 0, 128);
                for (int by = 0; by < blocksY; by++)
                {
                    long src = 128 + ((((long)(by0 + by) * rowBlocks) + bx0) * blockSize);
                    if (src + (blocksX * blockSize) <= data.Length)
                    {
                        Buffer.BlockCopy(data, (int)src, copy, 128 + (by * blocksX * blockSize), blocksX * blockSize);
                    }
                }
                BitConverter.GetBytes(blocksY * 4).CopyTo(copy, 12);
                BitConverter.GetBytes(blocksX * 4).CopyTo(copy, 16);
                BitConverter.GetBytes(1).CopyTo(copy, 28);
                source = Decode(copy);
                originX = bx0 * 4;
                originY = by0 * 4;
            }

            var pixels = new byte[regionWidth * regionHeight * 4];
            for (int ry = 0; ry < regionHeight; ry++)
            {
                int sy = y + ry - originY;
                if (sy < 0 || sy >= source.Height)
                {
                    continue;
                }
                for (int rx = 0; rx < regionWidth; rx++)
                {
                    int sx = x + rx - originX;
                    if (sx < 0 || sx >= source.Width)
                    {
                        continue;
                    }
                    Buffer.BlockCopy(source.Pixels, ((sy * source.Width) + sx) * 4, pixels, ((ry * regionWidth) + rx) * 4, 4);
                }
            }
            return new DdsImage(regionWidth, regionHeight, pixels, source.FormatName, 1, source.SuggestedSaveFormat);
        }

        public static DdsImage Decode(byte[] data)
        {
            if (!IsDds(data))
            {
                throw new InvalidDataException("Not a DDS file.");
            }

            int height = BitConverter.ToInt32(data, 12);
            int width = BitConverter.ToInt32(data, 16);
            int mipMaps = Math.Max(1, BitConverter.ToInt32(data, 28));
            uint pfFlags = BitConverter.ToUInt32(data, 80);
            uint fourCc = BitConverter.ToUInt32(data, 84);
            int bitCount = BitConverter.ToInt32(data, 88);
            uint rMask = BitConverter.ToUInt32(data, 92);
            uint gMask = BitConverter.ToUInt32(data, 96);
            uint bMask = BitConverter.ToUInt32(data, 100);
            uint aMask = BitConverter.ToUInt32(data, 104);

            if (width <= 0 || height <= 0 || width > 16384 || height > 16384)
            {
                throw new InvalidDataException($"Invalid DDS size {width}x{height}.");
            }

            int offset = 128;
            var pixels = new byte[width * height * 4];

            if ((pfFlags & DdpfFourCc) != 0)
            {
                string cc = FourCcToString(fourCc);
                if (cc == "DX10")
                {
                    uint dxgi = BitConverter.ToUInt32(data, 128);
                    offset = 148;
                    cc = dxgi switch
                    {
                        70 or 71 or 72 => "DXT1",
                        73 or 74 or 75 => "DXT3",
                        76 or 77 or 78 => "DXT5",
                        79 or 80 or 81 => "ATI1",
                        82 or 83 or 84 => "ATI2",
                        87 or 88 or 91 => "BGRA",
                        28 or 29 => "RGBA",
                        _ => throw new NotSupportedException($"DXGI format {dxgi} is not supported.")
                    };
                    if (cc == "BGRA")
                    {
                        DecodeMasked(data, offset, width, height, 32, 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000, true, pixels);
                        return new DdsImage(width, height, pixels, "DXGI B8G8R8A8", mipMaps, DdsSaveFormat.A8R8G8B8);
                    }
                    if (cc == "RGBA")
                    {
                        DecodeMasked(data, offset, width, height, 32, 0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000, true, pixels);
                        return new DdsImage(width, height, pixels, "DXGI R8G8B8A8", mipMaps, DdsSaveFormat.A8R8G8B8);
                    }
                }

                switch (cc)
                {
                    case "DXT1":
                        DecodeBlocks(data, offset, width, height, 8, pixels, (src, o, block) => DecodeColorBlock(src, o, block, true));
                        return new DdsImage(width, height, pixels, "DXT1", mipMaps, DdsSaveFormat.Dxt1);
                    case "DXT2":
                    case "DXT3":
                        DecodeBlocks(data, offset, width, height, 16, pixels, (src, o, block) =>
                        {
                            DecodeColorBlock(src, o + 8, block, false);
                            for (int i = 0; i < 16; i++)
                            {
                                int nibble = (src[o + (i >> 1)] >> ((i & 1) * 4)) & 0xF;
                                block[(i * 4) + 3] = (byte)(nibble * 17);
                            }
                        });
                        return new DdsImage(width, height, pixels, cc, mipMaps, DdsSaveFormat.Dxt3);
                    case "DXT4":
                    case "DXT5":
                        DecodeBlocks(data, offset, width, height, 16, pixels, (src, o, block) =>
                        {
                            DecodeColorBlock(src, o + 8, block, false);
                            DecodeAlphaBlock(src, o, block, 3);
                        });
                        return new DdsImage(width, height, pixels, cc, mipMaps, DdsSaveFormat.Dxt5);
                    case "ATI1":
                    case "BC4U":
                        DecodeBlocks(data, offset, width, height, 8, pixels, (src, o, block) =>
                        {
                            DecodeAlphaBlock(src, o, block, 2);
                            for (int i = 0; i < 16; i++)
                            {
                                block[i * 4] = block[(i * 4) + 1] = block[(i * 4) + 2];
                                block[(i * 4) + 3] = 255;
                            }
                        });
                        return new DdsImage(width, height, pixels, cc, mipMaps, DdsSaveFormat.A8R8G8B8);
                    case "ATI2":
                    case "BC5U":
                        DecodeBlocks(data, offset, width, height, 16, pixels, (src, o, block) =>
                        {
                            DecodeAlphaBlock(src, o, block, 2);     // red
                            DecodeAlphaBlock(src, o + 8, block, 1); // green
                            for (int i = 0; i < 16; i++)
                            {
                                block[i * 4] = 0;
                                block[(i * 4) + 3] = 255;
                            }
                        });
                        return new DdsImage(width, height, pixels, cc, mipMaps, DdsSaveFormat.A8R8G8B8);
                    default:
                        // D3DFMT values stored as FourCC
                        switch (fourCc)
                        {
                            case 21: // A8R8G8B8
                                DecodeMasked(data, offset, width, height, 32, 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000, true, pixels);
                                return new DdsImage(width, height, pixels, "A8R8G8B8", mipMaps, DdsSaveFormat.A8R8G8B8);
                            case 22: // X8R8G8B8
                                DecodeMasked(data, offset, width, height, 32, 0x00FF0000, 0x0000FF00, 0x000000FF, 0, false, pixels);
                                return new DdsImage(width, height, pixels, "X8R8G8B8", mipMaps, DdsSaveFormat.A8R8G8B8);
                        }
                        throw new NotSupportedException($"DDS format '{cc}' is not supported.");
                }
            }

            bool hasAlpha = (pfFlags & (DdpfAlphaPixels | DdpfAlpha)) != 0 && aMask != 0;
            if ((pfFlags & DdpfLuminance) != 0)
            {
                DecodeMasked(data, offset, width, height, bitCount, rMask, rMask, rMask, hasAlpha ? aMask : 0, hasAlpha, pixels);
                return new DdsImage(width, height, pixels, "L" + bitCount, mipMaps, DdsSaveFormat.A8R8G8B8);
            }
            if ((pfFlags & DdpfAlpha) != 0 && (pfFlags & DdpfRgb) == 0)
            {
                DecodeMasked(data, offset, width, height, bitCount, 0, 0, 0, aMask, true, pixels);
                return new DdsImage(width, height, pixels, "A" + bitCount, mipMaps, DdsSaveFormat.A8R8G8B8);
            }

            DecodeMasked(data, offset, width, height, bitCount, rMask, gMask, bMask, hasAlpha ? aMask : 0, hasAlpha, pixels);
            return new DdsImage(width, height, pixels, (hasAlpha ? "ARGB" : "RGB") + bitCount, mipMaps, DdsSaveFormat.A8R8G8B8);
        }

        // ------------------------------------------------------------------ decoding

        private delegate void BlockDecoder(byte[] src, int offset, byte[] block);

        private static void DecodeBlocks(byte[] data, int offset, int width, int height, int blockSize, byte[] pixels, BlockDecoder decoder)
        {
            int blocksX = Math.Max(1, (width + 3) / 4);
            int blocksY = Math.Max(1, (height + 3) / 4);
            var block = new byte[64];

            for (int by = 0; by < blocksY; by++)
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    if (offset + blockSize > data.Length)
                    {
                        return;
                    }

                    decoder(data, offset, block);
                    offset += blockSize;

                    for (int y = 0; y < 4; y++)
                    {
                        int py = (by * 4) + y;
                        if (py >= height)
                        {
                            break;
                        }
                        for (int x = 0; x < 4; x++)
                        {
                            int px = (bx * 4) + x;
                            if (px >= width)
                            {
                                break;
                            }
                            Buffer.BlockCopy(block, ((y * 4) + x) * 4, pixels, ((py * width) + px) * 4, 4);
                        }
                    }
                }
            }
        }

        private static void DecodeColorBlock(byte[] src, int o, byte[] block, bool allowOneBitAlpha)
        {
            ushort c0 = BitConverter.ToUInt16(src, o);
            ushort c1 = BitConverter.ToUInt16(src, o + 2);
            uint indices = BitConverter.ToUInt32(src, o + 4);

            Span<byte> colors = stackalloc byte[16];
            Unpack565(c0, colors, 0);
            Unpack565(c1, colors, 4);
            colors[3] = colors[7] = 255;

            if (c0 > c1 || !allowOneBitAlpha)
            {
                for (int i = 0; i < 3; i++)
                {
                    colors[8 + i] = (byte)(((2 * colors[i]) + colors[4 + i] + 1) / 3);
                    colors[12 + i] = (byte)((colors[i] + (2 * colors[4 + i]) + 1) / 3);
                }
                colors[11] = colors[15] = 255;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    colors[8 + i] = (byte)((colors[i] + colors[4 + i]) / 2);
                    colors[12 + i] = 0;
                }
                colors[11] = 255;
                colors[15] = 0;
            }

            for (int i = 0; i < 16; i++)
            {
                int index = (int)((indices >> (2 * i)) & 3);
                block[i * 4] = colors[index * 4];
                block[(i * 4) + 1] = colors[(index * 4) + 1];
                block[(i * 4) + 2] = colors[(index * 4) + 2];
                block[(i * 4) + 3] = colors[(index * 4) + 3];
            }
        }

        /// <summary>
        /// Decodes a BC4 style 8 byte block into channel <paramref name="channel"/> (0=B,1=G,2=R,3=A) of the block.
        /// </summary>
        private static void DecodeAlphaBlock(byte[] src, int o, byte[] block, int channel)
        {
            byte a0 = src[o];
            byte a1 = src[o + 1];
            Span<byte> values = stackalloc byte[8];
            values[0] = a0;
            values[1] = a1;
            if (a0 > a1)
            {
                for (int i = 1; i < 7; i++)
                {
                    values[i + 1] = (byte)((((7 - i) * a0) + (i * a1) + 3) / 7);
                }
            }
            else
            {
                for (int i = 1; i < 5; i++)
                {
                    values[i + 1] = (byte)((((5 - i) * a0) + (i * a1) + 2) / 5);
                }
                values[6] = 0;
                values[7] = 255;
            }

            ulong bits = 0;
            for (int i = 0; i < 6; i++)
            {
                bits |= (ulong)src[o + 2 + i] << (8 * i);
            }

            for (int i = 0; i < 16; i++)
            {
                block[(i * 4) + channel] = values[(int)((bits >> (3 * i)) & 7)];
            }
        }

        private static void Unpack565(ushort c, Span<byte> dst, int o)
        {
            int r = (c >> 11) & 31;
            int g = (c >> 5) & 63;
            int b = c & 31;
            dst[o] = (byte)((b << 3) | (b >> 2));
            dst[o + 1] = (byte)((g << 2) | (g >> 4));
            dst[o + 2] = (byte)((r << 3) | (r >> 2));
        }

        private static void DecodeMasked(byte[] data, int offset, int width, int height, int bitCount, uint rMask, uint gMask, uint bMask, uint aMask, bool hasAlpha, byte[] pixels)
        {
            int bytesPerPixel = Math.Max(1, bitCount / 8);
            for (int i = 0; i < width * height; i++)
            {
                int p = offset + (i * bytesPerPixel);
                if (p + bytesPerPixel > data.Length)
                {
                    break;
                }

                uint value = 0;
                for (int b = 0; b < bytesPerPixel; b++)
                {
                    value |= (uint)data[p + b] << (8 * b);
                }

                pixels[i * 4] = Extract(value, bMask);
                pixels[(i * 4) + 1] = Extract(value, gMask);
                pixels[(i * 4) + 2] = Extract(value, rMask);
                pixels[(i * 4) + 3] = hasAlpha ? Extract(value, aMask) : (byte)255;
            }
        }

        private static byte Extract(uint value, uint mask)
        {
            if (mask == 0)
            {
                return 0;
            }

            int shift = 0;
            while (((mask >> shift) & 1) == 0)
            {
                shift++;
            }

            uint max = mask >> shift;
            uint v = (value & mask) >> shift;
            return (byte)((v * 255 + (max / 2)) / max);
        }

        private static string FourCcToString(uint fourCc)
        {
            return new string(new[] { (char)(fourCc & 0xFF), (char)((fourCc >> 8) & 0xFF), (char)((fourCc >> 16) & 0xFF), (char)(fourCc >> 24) });
        }

        // ------------------------------------------------------------------ encoding

        /// <summary>
        /// Writes a DDS file from BGRA pixels.
        /// </summary>
        /// <param name="mipMaps">Number of mip levels to write (1 = only the image; 0 = full chain down to 1x1).</param>
        public static byte[] Encode(byte[] bgra, int width, int height, DdsSaveFormat format, int mipMaps = 1)
        {
            int fullChain = 1 + (int)Math.Floor(Math.Log(Math.Max(width, height), 2));
            int levels = mipMaps <= 0 ? fullChain : Math.Min(mipMaps, fullChain);

            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                bool compressed = format != DdsSaveFormat.A8R8G8B8;
                int blockSize = format == DdsSaveFormat.Dxt1 ? 8 : 16;
                int topSize = compressed ? Math.Max(1, (width + 3) / 4) * Math.Max(1, (height + 3) / 4) * blockSize : width * height * 4;

                const uint ddsdCaps = 0x1, ddsdHeight = 0x2, ddsdWidth = 0x4, ddsdPitch = 0x8, ddsdPixelFormat = 0x1000, ddsdMipMapCount = 0x20000, ddsdLinearSize = 0x80000;
                uint flags = ddsdCaps | ddsdHeight | ddsdWidth | ddsdPixelFormat | (compressed ? ddsdLinearSize : ddsdPitch);
                if (levels > 1)
                {
                    flags |= ddsdMipMapCount;
                }

                w.Write(DdsMagic);
                w.Write(124);
                w.Write(flags);
                w.Write(height);
                w.Write(width);
                w.Write(compressed ? topSize : width * 4);
                w.Write(0); // depth
                w.Write(levels > 1 ? levels : 0);
                for (int i = 0; i < 11; i++)
                {
                    w.Write(0);
                }

                // pixel format
                w.Write(32);
                if (compressed)
                {
                    w.Write(DdpfFourCc);
                    w.Write(format switch
                    {
                        DdsSaveFormat.Dxt1 => 0x31545844u,
                        DdsSaveFormat.Dxt3 => 0x33545844u,
                        _ => 0x35545844u
                    });
                    w.Write(0);
                    w.Write(0);
                    w.Write(0);
                    w.Write(0);
                    w.Write(0);
                }
                else
                {
                    w.Write(DdpfRgb | DdpfAlphaPixels);
                    w.Write(0);
                    w.Write(32);
                    w.Write(0x00FF0000);
                    w.Write(0x0000FF00);
                    w.Write(0x000000FF);
                    w.Write(0xFF000000);
                }

                const uint capsTexture = 0x1000, capsComplex = 0x8, capsMipMap = 0x400000;
                w.Write(levels > 1 ? capsTexture | capsComplex | capsMipMap : capsTexture);
                w.Write(0);
                w.Write(0);
                w.Write(0);
                w.Write(0);

                byte[] level = bgra;
                int lw = width, lh = height;
                for (int l = 0; l < levels; l++)
                {
                    switch (format)
                    {
                        case DdsSaveFormat.A8R8G8B8:
                            w.Write(level, 0, lw * lh * 4);
                            break;
                        default:
                            w.Write(EncodeBlocks(level, lw, lh, format));
                            break;
                    }

                    if (l + 1 < levels)
                    {
                        level = Downsample(level, lw, lh, out lw, out lh);
                    }
                }

                w.Flush();
                return ms.ToArray();
            }
        }

        private static byte[] Downsample(byte[] src, int width, int height, out int newWidth, out int newHeight)
        {
            newWidth = Math.Max(1, width / 2);
            newHeight = Math.Max(1, height / 2);
            var dst = new byte[newWidth * newHeight * 4];
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int sum0 = 0, sum1 = 0, sum2 = 0, sumA = 0, n = 0;
                    for (int dy = 0; dy < 2; dy++)
                    {
                        for (int dx = 0; dx < 2; dx++)
                        {
                            int sx = Math.Min(width - 1, (x * 2) + dx);
                            int sy = Math.Min(height - 1, (y * 2) + dy);
                            int p = ((sy * width) + sx) * 4;
                            int a = src[p + 3];
                            // weight color by alpha so transparent pixels don't bleed dark edges
                            sum0 += src[p] * a;
                            sum1 += src[p + 1] * a;
                            sum2 += src[p + 2] * a;
                            sumA += a;
                            n++;
                        }
                    }
                    int d = ((y * newWidth) + x) * 4;
                    if (sumA > 0)
                    {
                        dst[d] = (byte)(sum0 / sumA);
                        dst[d + 1] = (byte)(sum1 / sumA);
                        dst[d + 2] = (byte)(sum2 / sumA);
                    }
                    dst[d + 3] = (byte)(sumA / n);
                }
            }
            return dst;
        }

        private static byte[] EncodeBlocks(byte[] bgra, int width, int height, DdsSaveFormat format)
        {
            int blocksX = Math.Max(1, (width + 3) / 4);
            int blocksY = Math.Max(1, (height + 3) / 4);
            int blockSize = format == DdsSaveFormat.Dxt1 ? 8 : 16;
            var output = new byte[blocksX * blocksY * blockSize];
            var block = new byte[64];
            int o = 0;

            for (int by = 0; by < blocksY; by++)
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    // gather 4x4 pixels (clamped at the borders)
                    for (int y = 0; y < 4; y++)
                    {
                        int py = Math.Min(height - 1, (by * 4) + y);
                        for (int x = 0; x < 4; x++)
                        {
                            int px = Math.Min(width - 1, (bx * 4) + x);
                            Buffer.BlockCopy(bgra, ((py * width) + px) * 4, block, ((y * 4) + x) * 4, 4);
                        }
                    }

                    switch (format)
                    {
                        case DdsSaveFormat.Dxt1:
                            EncodeColorBlock(block, output, o, true);
                            break;
                        case DdsSaveFormat.Dxt3:
                            for (int i = 0; i < 8; i++)
                            {
                                int a0 = (block[(i * 2 * 4) + 3] + 8) / 17;
                                int a1 = (block[(((i * 2) + 1) * 4) + 3] + 8) / 17;
                                output[o + i] = (byte)(Math.Min(15, a0) | (Math.Min(15, a1) << 4));
                            }
                            EncodeColorBlock(block, output, o + 8, false);
                            break;
                        default:
                            EncodeAlphaBlock(block, output, o);
                            EncodeColorBlock(block, output, o + 8, false);
                            break;
                    }
                    o += blockSize;
                }
            }

            return output;
        }

        private static void EncodeColorBlock(byte[] block, byte[] output, int o, bool dxt1Alpha)
        {
            bool anyTransparent = false;
            int minR = 255, minG = 255, minB = 255, maxR = 0, maxG = 0, maxB = 0;
            int opaque = 0;

            for (int i = 0; i < 16; i++)
            {
                int p = i * 4;
                if (dxt1Alpha && block[p + 3] < 128)
                {
                    anyTransparent = true;
                    continue;
                }
                opaque++;
                minB = Math.Min(minB, block[p]);
                minG = Math.Min(minG, block[p + 1]);
                minR = Math.Min(minR, block[p + 2]);
                maxB = Math.Max(maxB, block[p]);
                maxG = Math.Max(maxG, block[p + 1]);
                maxR = Math.Max(maxR, block[p + 2]);
            }

            if (opaque == 0)
            {
                // fully transparent DXT1 block: c0 <= c1 and every index = 3
                output[o] = output[o + 1] = output[o + 2] = output[o + 3] = 0;
                output[o + 4] = output[o + 5] = output[o + 6] = output[o + 7] = 0xFF;
                return;
            }

            // inset the bounding box a little to reduce error (like stb_dxt / squish "range fit")
            int insetR = (maxR - minR) >> 4, insetG = (maxG - minG) >> 4, insetB = (maxB - minB) >> 4;
            minR = Math.Min(255, minR + insetR); maxR = Math.Max(0, maxR - insetR);
            minG = Math.Min(255, minG + insetG); maxG = Math.Max(0, maxG - insetG);
            minB = Math.Min(255, minB + insetB); maxB = Math.Max(0, maxB - insetB);

            ushort cMax = Pack565(maxR, maxG, maxB);
            ushort cMin = Pack565(minR, minG, minB);

            bool threeColor = dxt1Alpha && anyTransparent;
            if (threeColor)
            {
                // 3 color mode needs c0 <= c1
                if (cMax > cMin)
                {
                    (cMax, cMin) = (cMin, cMax);
                }
            }
            else
            {
                if (cMax < cMin)
                {
                    (cMax, cMin) = (cMin, cMax);
                }
                if (cMax == cMin)
                {
                    // force 4 color mode
                    if (cMin > 0)
                    {
                        cMin--;
                    }
                    else
                    {
                        cMax++;
                    }
                }
            }

            Span<byte> palette = stackalloc byte[16];
            Unpack565(cMax, palette, 0);
            Unpack565(cMin, palette, 4);
            int paletteCount;
            if (threeColor)
            {
                for (int i = 0; i < 3; i++)
                {
                    palette[8 + i] = (byte)((palette[i] + palette[4 + i]) / 2);
                }
                paletteCount = 3;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    palette[8 + i] = (byte)(((2 * palette[i]) + palette[4 + i] + 1) / 3);
                    palette[12 + i] = (byte)((palette[i] + (2 * palette[4 + i]) + 1) / 3);
                }
                paletteCount = 4;
            }

            uint indices = 0;
            for (int i = 0; i < 16; i++)
            {
                int p = i * 4;
                int best;
                if (threeColor && block[p + 3] < 128)
                {
                    best = 3;
                }
                else
                {
                    best = 0;
                    int bestDist = int.MaxValue;
                    for (int c = 0; c < paletteCount; c++)
                    {
                        int db = block[p] - palette[c * 4];
                        int dg = block[p + 1] - palette[(c * 4) + 1];
                        int dr = block[p + 2] - palette[(c * 4) + 2];
                        int dist = (dr * dr) + (dg * dg) + (db * db);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            best = c;
                        }
                    }
                }
                indices |= (uint)best << (2 * i);
            }

            BitConverter.GetBytes(cMax).CopyTo(output, o);
            BitConverter.GetBytes(cMin).CopyTo(output, o + 2);
            BitConverter.GetBytes(indices).CopyTo(output, o + 4);
        }

        private static void EncodeAlphaBlock(byte[] block, byte[] output, int o)
        {
            int min = 255, max = 0;
            for (int i = 0; i < 16; i++)
            {
                int a = block[(i * 4) + 3];
                min = Math.Min(min, a);
                max = Math.Max(max, a);
            }

            output[o] = (byte)max;
            output[o + 1] = (byte)min;

            Span<int> values = stackalloc int[8];
            values[0] = max;
            values[1] = min;
            if (max > min)
            {
                for (int i = 1; i < 7; i++)
                {
                    values[i + 1] = (((7 - i) * max) + (i * min) + 3) / 7;
                }
            }
            else
            {
                for (int i = 2; i < 8; i++)
                {
                    values[i] = max;
                }
            }

            ulong bits = 0;
            for (int i = 0; i < 16; i++)
            {
                int a = block[(i * 4) + 3];
                int best = 0, bestDist = int.MaxValue;
                for (int c = 0; c < 8; c++)
                {
                    int d = Math.Abs(a - values[c]);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = c;
                    }
                }
                bits |= (ulong)best << (3 * i);
            }

            for (int i = 0; i < 6; i++)
            {
                output[o + 2 + i] = (byte)(bits >> (8 * i));
            }
        }

        private static ushort Pack565(int r, int g, int b)
        {
            return (ushort)((((r * 31) + 127) / 255 << 11) | ((((g * 63) + 127) / 255) << 5) | (((b * 31) + 127) / 255));
        }
    }
}
