// /***************************************************************************
//  *
//  * Dragon Map Studio – load / save the 8-bit Dragon BMP
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
        public const string OpenFilter = "Bilder|*.bmp;*.png;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|8-Bit BMP (Dragon)|*.bmp|Alle Dateien|*.*";
        public const string SaveFilter = "8-Bit BMP für Dragon (*.bmp)|*.bmp|PNG Vorschau (*.png)|*.png";

        public static IndexedMap Load(string path, DragonRuleSet rules, ColorMatchMode mode, List<string> warnings)
        {
            byte[] data = File.ReadAllBytes(path);
            IndexedMap map = null;
            if (mode == ColorMatchMode.KeepIndices && TryReadBmp8(data, out int w, out int h, out byte[] pixels, out _))
            {
                map = new IndexedMap(w, h, pixels);
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
            var map = new IndexedMap(w, h);
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

        /// <summary>Writes an uncompressed 8-bit BMP with the 256 palette colours (Dragon format).</summary>
        public static void SaveBmp8(string path, IndexedMap map, Color[] palette)
        {
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
                    Color c = palette[i];
                    bw.Write(c.B);
                    bw.Write(c.G);
                    bw.Write(c.R);
                    bw.Write((byte)0);
                }
                var row = new byte[stride];
                for (int y = map.Height - 1; y >= 0; y--)
                {
                    Buffer.BlockCopy(map.Pixels, y * map.Width, row, 0, map.Width);
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
            var argb = new int[256];
            for (int i = 0; i < 256; i++)
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

        public static void SavePng(string path, IndexedMap map, Color[] palette)
        {
            using Bitmap bmp = ToBitmap(map, palette);
            bmp.Save(path, ImageFormat.Png);
        }
    }

    /// <summary>Finds the palette index for an RGB colour (exact, alias, then nearest defined colour).</summary>
    public sealed class PaletteMatcher
    {
        private readonly Dictionary<int, byte> _cache = new Dictionary<int, byte>();
        private readonly List<(int r, int g, int b, byte index)> _candidates = new List<(int, int, int, byte)>();

        public PaletteMatcher(DragonRuleSet rules)
        {
            foreach (ColorAlias a in rules.ColorAliases)
            {
                if (a.Index >= 0 && a.Index < 256)
                {
                    _cache[a.Rgb] = (byte)a.Index;
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
                    _cache.TryAdd(rgb, (byte)e.Index);
                }
            }
            foreach (DragonPaletteEntry e in rules.Palette)
            {
                if (e.Defined)
                {
                    _candidates.Add((e.R, e.G, e.B, (byte)e.Index));
                }
            }
            if (_candidates.Count == 0)
            {
                foreach (DragonPaletteEntry e in rules.Palette)
                {
                    _candidates.Add((e.R, e.G, e.B, (byte)e.Index));
                }
            }
        }

        public long Inexact { get; private set; }

        public byte Match(int rgb)
        {
            if (_cache.TryGetValue(rgb, out byte idx))
            {
                return idx;
            }
            int r = (rgb >> 16) & 0xFF, g = (rgb >> 8) & 0xFF, b = rgb & 0xFF;
            int best = int.MaxValue;
            byte bestIndex = 0;
            foreach ((int cr, int cg, int cb, byte index) in _candidates)
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
