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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Hues
{
    public sealed class EcHue
    {
        public int Number { get; internal set; }
        public EcUopEntry Entry { get; internal set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Hues.uop: one 256 x 2 pixel 32 bit BMP per hue ("data/definitions/hues/hue{nr:D4}.bmp", the color ramp),
    /// a text list "number,name" and "build/hues/hues.dds".
    /// </summary>
    public sealed class EcHues
    {
        public const string NamePattern = "data/definitions/hues/hue{0:D4}.bmp";

        private readonly SortedDictionary<int, EcHue> _hues = new SortedDictionary<int, EcHue>();
        private EcUopEntry _nameEntry;
        private bool _namesChanged;
        private string _newLine = "\n";

        public EcUopPackage Package { get; }
        public IEnumerable<EcHue> Hues => _hues.Values;
        public int Count => _hues.Count;

        public EcHues(EcUopPackage package)
        {
            Package = package;
            package.ResolveNames(NamePattern, 0, 9999);

            foreach (EcUopEntry entry in package.Entries)
            {
                if (entry.Name != null && entry.Name.StartsWith("data/definitions/hues/hue", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(Path.GetFileNameWithoutExtension(entry.Name).Substring(3), NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
                {
                    _hues[number] = new EcHue { Number = number, Entry = entry };
                }
            }

            // the name list is the text entry starting with "1,"
            foreach (EcUopEntry entry in package.Entries.Where(e => e.DecompressedSize < 1_000_000 && (e.Name == null || e.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || e.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))))
            {
                byte[] head = package.ReadDataHead(entry, 16);
                if (head.Length > 2 && head[0] == (byte)'1' && head[1] == (byte)',')
                {
                    _nameEntry = entry;
                    string names = Encoding.UTF8.GetString(package.ReadData(entry));
                    _newLine = names.Contains("\r\n") ? "\r\n" : "\n";
                    ReadNames(names);
                    break;
                }
            }
        }

        private void ReadNames(string text)
        {
            foreach (string line in text.Split('\n'))
            {
                string l = line.TrimEnd('\r');
                int comma = l.IndexOf(',');
                if (comma > 0 && int.TryParse(l.Substring(0, comma), NumberStyles.Integer, CultureInfo.InvariantCulture, out int number) &&
                    _hues.TryGetValue(number, out EcHue hue))
                {
                    hue.Name = l.Substring(comma + 1);
                }
            }
        }

        public EcHue Get(int number) => _hues.TryGetValue(number, out EcHue hue) ? hue : null;

        /// <summary>The 256 colors of the hue ramp (BGRA, first row of the bitmap).</summary>
        public byte[] GetRamp(EcHue hue, out int width, out int height)
        {
            byte[] bmp = Package.ReadData(hue.Entry);
            return ReadBmp(bmp, out width, out height);
        }

        public void SetName(EcHue hue, string name)
        {
            hue.Name = name;
            _namesChanged = true;
        }

        /// <summary>Replaces the ramp with BGRA pixels (any width; stored as a 32 bit BMP).</summary>
        public void SetRamp(EcHue hue, byte[] bgra, int width, int height)
        {
            byte[] original = Package.ReadData(hue.Entry);
            byte[] patched = PatchBmp(original, bgra, width, height);
            Package.SetData(hue.Entry, patched ?? WriteBmp(bgra, width, height));
        }

        /// <summary>
        /// Writes the pixels into the original bitmap when the size is the same (keeps the original header byte by byte).
        /// </summary>
        public static byte[] PatchBmp(byte[] bmp, byte[] bgra, int width, int height)
        {
            if (bmp == null || bmp.Length < 54 || BitConverter.ToUInt16(bmp, 28) != 32 ||
                BitConverter.ToInt32(bmp, 18) != width || Math.Abs(BitConverter.ToInt32(bmp, 22)) != height)
            {
                return null;
            }

            byte[] copy = (byte[])bmp.Clone();
            int offset = BitConverter.ToInt32(bmp, 10);
            bool bottomUp = BitConverter.ToInt32(bmp, 22) > 0;
            for (int y = 0; y < height; y++)
            {
                int row = bottomUp ? height - 1 - y : y;
                int dst = offset + (row * width * 4);
                if (dst + (width * 4) > copy.Length)
                {
                    return null;
                }
                Buffer.BlockCopy(bgra, y * width * 4, copy, dst, width * 4);
            }
            return copy;
        }

        /// <summary>Adds a new hue (copy of an existing ramp or the given pixels).</summary>
        public EcHue Add(int number, byte[] bgra, int width, int height, string name)
        {
            EcUopEntry entry = Package.AddOrReplace(string.Format(CultureInfo.InvariantCulture, NamePattern, number), WriteBmp(bgra, width, height), Package.PreferredCompression);
            var hue = new EcHue { Number = number, Entry = entry, Name = name };
            _hues[number] = hue;
            _namesChanged = true;
            return hue;
        }

        public void Remove(EcHue hue)
        {
            Package.Remove(hue.Entry);
            _hues.Remove(hue.Number);
            _namesChanged = true;
        }

        /// <summary>Writes the changed name list into the package.</summary>
        public void Apply()
        {
            if (!_namesChanged || _nameEntry == null)
            {
                return;
            }

            var sb = new StringBuilder();
            int max = _hues.Count == 0 ? 0 : _hues.Keys.Max();
            for (int i = 1; i <= max; i++)
            {
                sb.Append(i.ToString(CultureInfo.InvariantCulture)).Append(',');
                if (_hues.TryGetValue(i, out EcHue hue) && hue.Name != null)
                {
                    sb.Append(hue.Name);
                }
                sb.Append(_newLine);
            }
            Package.SetData(_nameEntry, Encoding.UTF8.GetBytes(sb.ToString()));
            _namesChanged = false;
        }

        // ------------------------------------------------------------------ bmp

        public static byte[] ReadBmp(byte[] bmp, out int width, out int height)
        {
            int offset = BitConverter.ToInt32(bmp, 10);
            width = BitConverter.ToInt32(bmp, 18);
            int rawHeight = BitConverter.ToInt32(bmp, 22);
            int bits = BitConverter.ToUInt16(bmp, 28);
            height = Math.Abs(rawHeight);
            bool bottomUp = rawHeight > 0;
            int bytesPerPixel = bits / 8;
            int stride = ((width * bits) + 31) / 32 * 4;

            var pixels = new byte[width * height * 4];
            for (int y = 0; y < height; y++)
            {
                int row = bottomUp ? height - 1 - y : y;
                for (int x = 0; x < width; x++)
                {
                    int src = offset + (row * stride) + (x * bytesPerPixel);
                    int dst = ((y * width) + x) * 4;
                    if (src + bytesPerPixel > bmp.Length)
                    {
                        continue;
                    }
                    pixels[dst] = bmp[src];
                    pixels[dst + 1] = bmp[src + 1];
                    pixels[dst + 2] = bmp[src + 2];
                    pixels[dst + 3] = bytesPerPixel == 4 ? bmp[src + 3] : (byte)255;
                }
            }
            return pixels;
        }

        public static byte[] WriteBmp(byte[] bgra, int width, int height)
        {
            int size = 54 + (width * height * 4);
            var bmp = new byte[size];
            bmp[0] = (byte)'B';
            bmp[1] = (byte)'M';
            BitConverter.GetBytes(size).CopyTo(bmp, 2);
            BitConverter.GetBytes(54).CopyTo(bmp, 10);
            BitConverter.GetBytes(40).CopyTo(bmp, 14);
            BitConverter.GetBytes(width).CopyTo(bmp, 18);
            BitConverter.GetBytes(height).CopyTo(bmp, 22);
            BitConverter.GetBytes((ushort)1).CopyTo(bmp, 26);
            BitConverter.GetBytes((ushort)32).CopyTo(bmp, 28);
            for (int y = 0; y < height; y++)
            {
                Buffer.BlockCopy(bgra, ((height - 1 - y) * width) * 4, bmp, 54 + (y * width * 4), width * 4);
            }
            return bmp;
        }
    }
}
