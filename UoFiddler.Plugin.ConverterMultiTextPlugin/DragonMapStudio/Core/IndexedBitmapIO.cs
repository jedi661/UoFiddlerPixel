// /***************************************************************************
//  *
//  * Dragon Map Studio – load / save the 8-bit Dragon BMP and the 16-bit map PNG
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>How an image with other colours is turned into palette indices.</summary>
    public enum ColorMatchMode
    {
        /// <summary>8-bit images: keep the pixel values (exactly like Dragon). RGB images: nearest colour.</summary>
        KeepIndices,

        /// <summary>Always compare the colours (for 8-bit images with another palette).</summary>
        NearestColor
    }

    /// <summary>
    /// Reads and writes uncompressed 8-bit BMPs directly (exact palette, any size, no GDI+ limits)
    /// and converts other images (PNG, JPG, 24/32-bit BMP …) with the nearest palette colour.
    /// </summary>
    public static class IndexedBitmapIO
    {
        public const string OpenFilter = "Bilder|*.bmp;*.png;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|8-Bit BMP (Dragon)|*.bmp|16-Bit-Karte (PNG)|*.png|Alle Dateien|*.*";
        public const string SaveFilter = "8-Bit BMP für Dragon (*.bmp)|*.bmp|16-Bit-Karte (*.png)|*.png";

        public static IndexedMap Load(string path, DragonRuleSet rules, ColorMatchMode mode, List<string> warnings)
        {
            byte[] data = File.ReadAllBytes(path);
            IndexedMap map = null;
            if (TryReadPng16(data, out int w16, out int h16, out ushort[] pixels16))
            {
                map = new IndexedMap(w16, h16, pixels16, 16);
            }
            else if (mode == ColorMatchMode.KeepIndices && TryReadBmp8(data, out int w, out int h, out byte[] pixels, out _))
            {
                var p16 = new ushort[pixels.Length];
                for (int i = 0; i < pixels.Length; i++)
                {
                    p16[i] = pixels[i];
                }
                map = new IndexedMap(w, h, p16, 8);
            }
            else
            {
                using var bmp = new Bitmap(new MemoryStream(data));
                map = FromBitmap(bmp, rules, warnings);
            }
            map.FilePath = path;
            if (map.Width % 8 != 0 || map.Height % 8 != 0)
            {
                warnings?.Add($"Breite/Höhe ({map.Width}×{map.Height}) ist nicht durch 8 teilbar – für die MUL-Erzeugung wird aufgefüllt bzw. abgeschnitten.");
            }
            return map;
        }

        /// <summary>Reads an uncompressed 8-bit BMP (bottom-up or top-down).</summary>
        public static bool TryReadBmp8(byte[] data, out int width, out int height, out byte[] pixels, out Color[] palette)
        {
            width = height = 0;
            pixels = null;
            palette = null;
            if (data.Length < 54 || data[0] != 'B' || data[1] != 'M')
            {
                return false;
            }
            int offBits = BitConverter.ToInt32(data, 10);
            int headerSize = BitConverter.ToInt32(data, 14);
            if (headerSize < 40)
            {
                return false;
            }
            width = BitConverter.ToInt32(data, 18);
            int rawHeight = BitConverter.ToInt32(data, 22);
            int bpp = BitConverter.ToUInt16(data, 28);
            int compression = BitConverter.ToInt32(data, 30);
            if (bpp != 8 || compression != 0 || width <= 0 || rawHeight == 0)
            {
                return false;
            }
            bool topDown = rawHeight < 0;
            height = Math.Abs(rawHeight);
            int stride = (width + 3) & ~3;
            if (offBits + (long)stride * (height - 1) + width > data.Length)
            {
                return false;
            }
            palette = PaletteIO.ReadBmpPalette(data);
            pixels = new byte[width * height];
            for (int y = 0; y < height; y++)
            {
                int src = offBits + (topDown ? y : height - 1 - y) * stride;
                Buffer.BlockCopy(data, src, pixels, y * width, width);
            }
            return true;
        }

        /// <summary>Converts any bitmap to palette indices (colour aliases first, then nearest colour).</summary>
        public static IndexedMap FromBitmap(Bitmap source, DragonRuleSet rules, List<string> warnings)
        {
            int w = source.Width, h = source.Height;
            var map = new IndexedMap(w, h, 0, rules.ColorBits);
            var matcher = new PaletteMatcher(rules);
            var rect = new Rectangle(0, 0, w, h);
            BitmapData bd = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var row = new int[w];
                for (int y = 0; y < h; y++)
                {
                    Marshal.Copy(bd.Scan0 + y * bd.Stride, row, 0, w);
                    int o = y * w;
                    for (int x = 0; x < w; x++)
                    {
                        map.Pixels[o + x] = matcher.Match(row[x] & 0xFFFFFF);
                    }
                }
            }
            finally
            {
                source.UnlockBits(bd);
            }
            if (matcher.Inexact > 0)
            {
                warnings?.Add($"{matcher.Inexact:N0} Farben des Bildes sind nicht in der Palette und wurden der nächsten Farbe zugeordnet.");
            }
            return map;
        }

        /// <summary>Saves the map in its format: 8 bit = Dragon BMP, 16 bit = 16-bit grey PNG.</summary>
        public static void Save(string path, IndexedMap map, Color[] palette)
        {
            if (map.Bits == 16)
            {
                SavePng16(path, map);
            }
            else
            {
                SaveBmp8(path, map, palette);
            }
        }

        /// <summary>Writes an uncompressed 8-bit BMP with the 256 palette colours (Dragon format).</summary>
        public static void SaveBmp8(string path, IndexedMap map, Color[] palette)
        {
            if (map.HighestUsedIndex() > 0xFF)
            {
                throw new InvalidOperationException("Die Karte benutzt Farben über ff – als 16-Bit-Karte (PNG) speichern.");
            }
            int stride = (map.Width + 3) & ~3;
            int imageSize = stride * map.Height;
            const int paletteSize = 1024;
            int offBits = 14 + 40 + paletteSize;
            string tmp = path + ".tmp";
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write((byte)'B');
                bw.Write((byte)'M');
                bw.Write(offBits + imageSize);
                bw.Write(0);
                bw.Write(offBits);
                bw.Write(40);
                bw.Write(map.Width);
                bw.Write(map.Height);
                bw.Write((short)1);
                bw.Write((short)8);
                bw.Write(0);
                bw.Write(imageSize);
                bw.Write(3779);
                bw.Write(3779);
                bw.Write(256);
                bw.Write(256);
                for (int i = 0; i < 256; i++)
                {
                    Color c = i < palette.Length ? palette[i] : Color.Black;
                    bw.Write(c.B);
                    bw.Write(c.G);
                    bw.Write(c.R);
                    bw.Write((byte)0);
                }
                var row = new byte[stride];
                for (int y = map.Height - 1; y >= 0; y--)
                {
                    int o = y * map.Width;
                    for (int x = 0; x < map.Width; x++)
                    {
                        row[x] = (byte)map.Pixels[o + x];
                    }
                    bw.Write(row);
                }
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tmp, path);
        }

        /// <summary>Creates a 32-bit bitmap (for PNG export / previews). <paramref name="scale"/> &lt; 1 shrinks.</summary>
        public static Bitmap ToBitmap(IndexedMap map, Color[] palette, int maxSize = 0)
        {
            int step = 1;
            if (maxSize > 0)
            {
                while (map.Width / step > maxSize || map.Height / step > maxSize)
                {
                    step++;
                }
            }
            int w = Math.Max(1, map.Width / step), h = Math.Max(1, map.Height / step);
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppRgb);
            var argb = new int[65536];
            for (int i = 0; i < palette.Length && i < argb.Length; i++)
            {
                argb[i] = palette[i].ToArgb();
            }
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                var row = new int[w];
                for (int y = 0; y < h; y++)
                {
                    int sy = y * step;
                    for (int x = 0; x < w; x++)
                    {
                        row[x] = argb[map.Pixels[sy * map.Width + x * step]];
                    }
                    Marshal.Copy(row, 0, bd.Scan0 + y * bd.Stride, w);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return bmp;
        }

        // ------------------------------------------------------------------ 16-bit PNG (grey value = colour address)

        private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

        /// <summary>Writes the map as 16-bit greyscale PNG: every grey value is the colour address (lossless).</summary>
        public static void SavePng16(string path, IndexedMap map)
        {
            string tmp = path + ".tmp";
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20))
            {
                fs.Write(PngSignature, 0, PngSignature.Length);
                var ihdr = new byte[13];
                WriteBE(ihdr, 0, map.Width);
                WriteBE(ihdr, 4, map.Height);
                ihdr[8] = 16;  // bit depth
                ihdr[9] = 0;   // greyscale
                WriteChunk(fs, "IHDR", ihdr);
                WriteChunk(fs, "tEXt", System.Text.Encoding.Latin1.GetBytes("Software\0Dragon Map Studio 16-bit map"));
                using (var raw = new MemoryStream())
                {
                    using (var z = new ZLibStream(raw, CompressionLevel.Fastest, true))
                    {
                        var row = new byte[1 + map.Width * 2];
                        for (int y = 0; y < map.Height; y++)
                        {
                            row[0] = 0; // filter: none
                            int o = y * map.Width;
                            for (int x = 0; x < map.Width; x++)
                            {
                                ushort v = map.Pixels[o + x];
                                row[1 + x * 2] = (byte)(v >> 8);
                                row[2 + x * 2] = (byte)v;
                            }
                            z.Write(row, 0, row.Length);
                        }
                    }
                    WriteChunk(fs, "IDAT", raw.ToArray());
                }
                WriteChunk(fs, "IEND", Array.Empty<byte>());
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(tmp, path);
        }

        /// <summary>Is the file a 16-bit greyscale PNG (= 16-bit map)?</summary>
        public static bool IsPng16(string path)
        {
            try
            {
                using var fs = File.OpenRead(path);
                var head = new byte[26];
                return fs.Read(head, 0, head.Length) == head.Length && IsPng16Header(head);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsPng16Header(byte[] d)
        {
            if (d.Length < 26)
            {
                return false;
            }
            for (int i = 0; i < 8; i++)
            {
                if (d[i] != PngSignature[i])
                {
                    return false;
                }
            }
            return d[12] == 'I' && d[13] == 'H' && d[14] == 'D' && d[15] == 'R' && d[24] == 16 && d[25] == 0;
        }

        /// <summary>Reads a 16-bit greyscale PNG (all PNG row filters, no interlace).</summary>
        public static bool TryReadPng16(byte[] data, out int width, out int height, out ushort[] pixels)
        {
            width = height = 0;
            pixels = null;
            if (!IsPng16Header(data) || data[28] != 0)
            {
                return false; // not 16-bit grey or interlaced
            }
            width = ReadBE(data, 16);
            height = ReadBE(data, 20);
            if (width <= 0 || height <= 0)
            {
                return false;
            }
            using var idat = new MemoryStream();
            int p = 8;
            while (p + 8 <= data.Length)
            {
                int len = ReadBE(data, p);
                string type = System.Text.Encoding.ASCII.GetString(data, p + 4, 4);
                if (type == "IDAT")
                {
                    idat.Write(data, p + 8, len);
                }
                else if (type == "IEND")
                {
                    break;
                }
                p += 12 + len;
            }
            idat.Position = 0;
            int stride = width * 2;
            var prev = new byte[stride];
            var cur = new byte[stride];
            pixels = new ushort[width * height];
            using var z = new ZLibStream(idat, CompressionMode.Decompress);
            for (int y = 0; y < height; y++)
            {
                int filter = z.ReadByte();
                ReadExactly(z, cur, stride);
                Unfilter(filter, cur, prev, 2);
                int o = y * width;
                for (int x = 0; x < width; x++)
                {
                    pixels[o + x] = (ushort)((cur[x * 2] << 8) | cur[x * 2 + 1]);
                }
                (prev, cur) = (cur, prev);
            }
            return true;
        }

        private static void ReadExactly(Stream s, byte[] buffer, int count)
        {
            int read = 0;
            while (read < count)
            {
                int n = s.Read(buffer, read, count - read);
                if (n <= 0)
                {
                    throw new EndOfStreamException("PNG-Daten zu kurz.");
                }
                read += n;
            }
        }

        private static void Unfilter(int filter, byte[] cur, byte[] prev, int bpp)
        {
            for (int i = 0; i < cur.Length; i++)
            {
                int a = i >= bpp ? cur[i - bpp] : 0, b = prev[i], c = i >= bpp ? prev[i - bpp] : 0;
                int add;
                switch (filter)
                {
                    case 1: add = a; break;
                    case 2: add = b; break;
                    case 3: add = (a + b) / 2; break;
                    case 4:
                        {
                            int pp = a + b - c, pa = Math.Abs(pp - a), pb = Math.Abs(pp - b), pc = Math.Abs(pp - c);
                            add = pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
                            break;
                        }
                    default: add = 0; break;
                }
                cur[i] = (byte)(cur[i] + add);
            }
        }

        private static void WriteBE(byte[] b, int o, int v)
        {
            b[o] = (byte)(v >> 24);
            b[o + 1] = (byte)(v >> 16);
            b[o + 2] = (byte)(v >> 8);
            b[o + 3] = (byte)v;
        }

        private static int ReadBE(byte[] b, int o) => (b[o] << 24) | (b[o + 1] << 16) | (b[o + 2] << 8) | b[o + 3];

        private static void WriteChunk(Stream s, string type, byte[] data)
        {
            var len = new byte[4];
            WriteBE(len, 0, data.Length);
            s.Write(len, 0, 4);
            byte[] t = System.Text.Encoding.ASCII.GetBytes(type);
            s.Write(t, 0, 4);
            s.Write(data, 0, data.Length);
            uint crc = Crc32(t, 0xFFFFFFFF);
            crc = Crc32(data, crc) ^ 0xFFFFFFFF;
            var c = new byte[4];
            WriteBE(c, 0, (int)crc);
            s.Write(c, 0, 4);
        }

        private static uint[] _crcTable;

        private static uint Crc32(byte[] data, uint crc)
        {
            if (_crcTable == null)
            {
                var t = new uint[256];
                for (uint n = 0; n < 256; n++)
                {
                    uint c = n;
                    for (int k = 0; k < 8; k++)
                    {
                        c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                    }
                    t[n] = c;
                }
                _crcTable = t;
            }
            foreach (byte b in data)
            {
                crc = _crcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
            }
            return crc;
        }

        public static void SavePng(string path, IndexedMap map, Color[] palette)
        {
            using Bitmap bmp = ToBitmap(map, palette);
            bmp.Save(path, ImageFormat.Png);
        }
    }

    /// <summary>Finds the palette index for an RGB colour (exact, alias, then nearest defined colour).</summary>
    public sealed class PaletteMatcher
    {
        private readonly Dictionary<int, ushort> _cache = new Dictionary<int, ushort>();
        private readonly List<(int r, int g, int b, ushort index)> _candidates = new List<(int, int, int, ushort)>();

        public PaletteMatcher(DragonRuleSet rules)
        {
            foreach (ColorAlias a in rules.ColorAliases)
            {
                if (a.Index >= 0 && a.Index < rules.Palette.Count)
                {
                    _cache[a.Rgb] = (ushort)a.Index;
                }
            }
            // exact colours of the palette – defined colours win over free ones
            for (int pass = 0; pass < 2; pass++)
            {
                foreach (DragonPaletteEntry e in rules.Palette)
                {
                    if ((pass == 0) != e.Defined)
                    {
                        continue;
                    }
                    int rgb = (e.R << 16) | (e.G << 8) | e.B;
                    _cache.TryAdd(rgb, (ushort)e.Index);
                }
            }
            foreach (DragonPaletteEntry e in rules.Palette)
            {
                if (e.Defined)
                {
                    _candidates.Add((e.R, e.G, e.B, (ushort)e.Index));
                }
            }
            if (_candidates.Count == 0)
            {
                foreach (DragonPaletteEntry e in rules.Palette)
                {
                    _candidates.Add((e.R, e.G, e.B, (ushort)e.Index));
                }
            }
        }

        public long Inexact { get; private set; }

        public ushort Match(int rgb)
        {
            if (_cache.TryGetValue(rgb, out ushort idx))
            {
                return idx;
            }
            int r = (rgb >> 16) & 0xFF, g = (rgb >> 8) & 0xFF, b = rgb & 0xFF;
            int best = int.MaxValue;
            ushort bestIndex = 0;
            foreach ((int cr, int cg, int cb, ushort index) in _candidates)
            {
                // weighted distance (closer to how the eye sees colours)
                int dr = r - cr, dg = g - cg, db = b - cb;
                int d = 2 * dr * dr + 4 * dg * dg + 3 * db * db;
                if (d < best)
                {
                    best = d;
                    bestIndex = index;
                }
            }
            _cache[rgb] = bestIndex;
            Inexact++;
            return bestIndex;
        }
    }
}
