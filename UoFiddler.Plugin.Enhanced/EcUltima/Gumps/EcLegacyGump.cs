/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Gumps
{
    /// <summary>
    /// Classic gump image as stored in gumpartLegacyMUL.uop ("build/gumpartlegacymul/{id:D8}.tga" – not a real TGA):
    /// uint width, uint height, height row offsets (uint, in 4 byte units from the start of the offset table),
    /// then per row runs of (ushort color RGB555, ushort length); color 0 is transparent. An 8 byte entry (0, 0) is empty.
    /// </summary>
    public static class EcLegacyGump
    {
        public static bool IsLegacyGump(byte[] data)
        {
            if (data == null || data.Length < 8)
            {
                return false;
            }
            int width = BitConverter.ToInt32(data, 0);
            int height = BitConverter.ToInt32(data, 4);
            if (width == 0 && height == 0)
            {
                return true;
            }
            return width > 0 && height > 0 && width < 4096 && height < 4096 && data.Length >= 8 + (height * 4);
        }

        /// <summary>BGRA pixels (empty array for an empty gump).</summary>
        public static byte[] Decode(byte[] data, out int width, out int height)
        {
            width = BitConverter.ToInt32(data, 0);
            height = BitConverter.ToInt32(data, 4);
            var pixels = new byte[Math.Max(0, width * height * 4)];
            if (width <= 0 || height <= 0)
            {
                return pixels;
            }

            const int table = 8;
            for (int y = 0; y < height; y++)
            {
                int start = table + (BitConverter.ToInt32(data, table + (y * 4)) * 4);
                int end = y + 1 < height ? table + (BitConverter.ToInt32(data, table + ((y + 1) * 4)) * 4) : data.Length;
                int x = 0;
                for (int p = start; p + 4 <= end && x < width; p += 4)
                {
                    ushort color = BitConverter.ToUInt16(data, p);
                    int run = BitConverter.ToUInt16(data, p + 2);
                    if (color != 0)
                    {
                        byte r = (byte)(((color >> 10) & 0x1F) * 255 / 31);
                        byte g = (byte)(((color >> 5) & 0x1F) * 255 / 31);
                        byte b = (byte)((color & 0x1F) * 255 / 31);
                        for (int i = 0; i < run && x + i < width; i++)
                        {
                            int o = ((y * width) + x + i) * 4;
                            pixels[o] = b;
                            pixels[o + 1] = g;
                            pixels[o + 2] = r;
                            pixels[o + 3] = 255;
                        }
                    }
                    x += run;
                }
            }
            return pixels;
        }

        /// <summary>Encodes BGRA pixels (alpha below 128 = transparent).</summary>
        public static byte[] Encode(byte[] bgra, int width, int height)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(width);
                w.Write(height);
                if (width <= 0 || height <= 0)
                {
                    return ms.ToArray();
                }

                var rows = new List<List<(ushort Color, ushort Run)>>();
                for (int y = 0; y < height; y++)
                {
                    var runs = new List<(ushort, ushort)>();
                    int x = 0;
                    while (x < width)
                    {
                        ushort color = Color555(bgra, ((y * width) + x) * 4);
                        int run = 1;
                        while (x + run < width && run < ushort.MaxValue && Color555(bgra, ((y * width) + x + run) * 4) == color)
                        {
                            run++;
                        }
                        runs.Add((color, (ushort)run));
                        x += run;
                    }
                    rows.Add(runs);
                }

                int offset = height; // in 4 byte units, from the start of the table
                foreach (var runs in rows)
                {
                    w.Write(offset);
                    offset += runs.Count;
                }
                foreach (var runs in rows)
                {
                    foreach (var (color, run) in runs)
                    {
                        w.Write(color);
                        w.Write(run);
                    }
                }
                w.Flush();
                return ms.ToArray();
            }
        }

        private static ushort Color555(byte[] bgra, int o)
        {
            if (bgra[o + 3] < 128)
            {
                return 0;
            }
            int value = ((bgra[o + 2] >> 3) << 10) | ((bgra[o + 1] >> 3) << 5) | (bgra[o] >> 3);
            // pure black would be transparent, use the darkest visible color instead
            return (ushort)(value == 0 ? 1 : value);
        }
    }
}
