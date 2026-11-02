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
using System.IO;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Hues
{
    /// <summary>
    /// One hue of the classic hues.mul. <see cref="Number"/> is the client number (1 = first entry of the file).
    /// </summary>
    public sealed class ClassicHue
    {
        public const int ColorCount = 32;
        public const int NameLength = 20;

        public int Number { get; }

        /// <summary>The 32 colors (RGB555, bit 15 unused).</summary>
        public ushort[] Colors { get; } = new ushort[ColorCount];

        public ushort TableStart { get; set; }
        public ushort TableEnd { get; set; }

        /// <summary>The 20 name bytes as stored (kept for a byte exact write when the name is not changed).</summary>
        public byte[] RawName { get; private set; } = new byte[NameLength];

        public ClassicHue(int number)
        {
            Number = number;
        }

        /// <summary>The name up to the first 0 byte.</summary>
        public string Name
        {
            get
            {
                int end = Array.IndexOf(RawName, (byte)0);
                return Encoding.ASCII.GetString(RawName, 0, end < 0 ? NameLength : end);
            }
            set
            {
                if (value == Name)
                {
                    return;
                }
                var raw = new byte[NameLength];
                byte[] bytes = Encoding.ASCII.GetBytes(value ?? string.Empty);
                Array.Copy(bytes, raw, Math.Min(bytes.Length, NameLength));
                RawName = raw;
            }
        }

        /// <summary>True when every color is 0 and there is no name (an unused slot).</summary>
        public bool IsEmpty => Colors.All(c => (c & 0x7FFF) == 0) && Name.Length == 0;

        public void SetColors(ushort[] colors)
        {
            for (int i = 0; i < ColorCount; i++)
            {
                Colors[i] = (ushort)(colors[i] & 0x7FFF);
            }
        }

        /// <summary>Sets the table values the way the client files have them: first and last color.</summary>
        public void SetDefaultTable()
        {
            TableStart = Colors[0];
            TableEnd = Colors[ColorCount - 1];
        }

        public bool SameColors(ushort[] colors)
        {
            for (int i = 0; i < ColorCount; i++)
            {
                if ((Colors[i] & 0x7FFF) != (colors[i] & 0x7FFF))
                {
                    return false;
                }
            }
            return true;
        }

        public ClassicHue Clone(int number)
        {
            var copy = new ClassicHue(number) { TableStart = TableStart, TableEnd = TableEnd, RawName = (byte[])RawName.Clone() };
            Array.Copy(Colors, copy.Colors, ColorCount);
            return copy;
        }

        internal void SetRawName(byte[] raw) => RawName = raw;

        public static int Red(ushort c) => (c >> 10) & 0x1F;
        public static int Green(ushort c) => (c >> 5) & 0x1F;
        public static int Blue(ushort c) => c & 0x1F;
        public static ushort Make(int r, int g, int b) => (ushort)(((r & 0x1F) << 10) | ((g & 0x1F) << 5) | (b & 0x1F));
    }

    /// <summary>
    /// The classic hues.mul: groups of 8 hues, each group starts with a 4 byte header;
    /// a hue is 32 colors (RGB555), table start, table end and a 20 byte name (88 bytes).
    /// Reading and writing is byte exact (headers, table values and name bytes are kept).
    /// </summary>
    public sealed class ClassicHueFile
    {
        public const string FileName = "hues.mul";
        public const int HuesPerGroup = 8;
        public const int EntrySize = 88;
        public const int GroupSize = 4 + (HuesPerGroup * EntrySize); // 708
        public const int DefaultHueCount = 3000;

        private readonly List<ClassicHue> _hues = new List<ClassicHue>();
        private readonly List<int> _headers = new List<int>();

        /// <summary>Hue 1 is at index 0.</summary>
        public IReadOnlyList<ClassicHue> Hues => _hues;

        public int Count => _hues.Count;

        /// <summary>Bytes after the last complete group (kept when written again).</summary>
        public byte[] Tail { get; private set; } = Array.Empty<byte>();

        /// <summary>Hue by client number (1 based), null when outside the file.</summary>
        public ClassicHue Get(int number) => number >= 1 && number <= _hues.Count ? _hues[number - 1] : null;

        public static ClassicHueFile Load(string path) => FromBytes(File.ReadAllBytes(path));

        public static ClassicHueFile FromBytes(byte[] data)
        {
            var file = new ClassicHueFile();
            int groups = data.Length / GroupSize;
            int pos = 0;
            for (int g = 0; g < groups; g++)
            {
                file._headers.Add(BitConverter.ToInt32(data, pos));
                pos += 4;
                for (int i = 0; i < HuesPerGroup; i++)
                {
                    var hue = new ClassicHue(file._hues.Count + 1);
                    for (int c = 0; c < ClassicHue.ColorCount; c++)
                    {
                        hue.Colors[c] = BitConverter.ToUInt16(data, pos + (c * 2));
                    }
                    hue.TableStart = BitConverter.ToUInt16(data, pos + 64);
                    hue.TableEnd = BitConverter.ToUInt16(data, pos + 66);
                    var raw = new byte[ClassicHue.NameLength];
                    Array.Copy(data, pos + 68, raw, 0, ClassicHue.NameLength);
                    hue.SetRawName(raw);
                    file._hues.Add(hue);
                    pos += EntrySize;
                }
            }
            if (pos < data.Length)
            {
                file.Tail = data.Skip(pos).ToArray();
            }
            return file;
        }

        /// <summary>An empty file with the given number of hues (whole groups, header 0).</summary>
        public static ClassicHueFile Create(int hueCount = DefaultHueCount)
        {
            var file = new ClassicHueFile();
            file.EnsureCount(hueCount);
            return file;
        }

        /// <summary>Adds empty groups until the hue fits.</summary>
        public void EnsureCount(int hueCount)
        {
            while (_hues.Count < hueCount)
            {
                _headers.Add(0);
                for (int i = 0; i < HuesPerGroup; i++)
                {
                    _hues.Add(new ClassicHue(_hues.Count + 1));
                }
            }
        }

        /// <summary>Header of the group the hue belongs to.</summary>
        public int GroupHeader(int number) => _headers[(number - 1) / HuesPerGroup];

        public void SetGroupHeader(int number, int header) => _headers[(number - 1) / HuesPerGroup] = header;

        public byte[] ToBytes()
        {
            using (var ms = new MemoryStream((_headers.Count * GroupSize) + Tail.Length))
            using (var w = new BinaryWriter(ms))
            {
                for (int g = 0; g < _headers.Count; g++)
                {
                    w.Write(_headers[g]);
                    for (int i = 0; i < HuesPerGroup; i++)
                    {
                        ClassicHue hue = _hues[(g * HuesPerGroup) + i];
                        foreach (ushort c in hue.Colors)
                        {
                            w.Write(c);
                        }
                        w.Write(hue.TableStart);
                        w.Write(hue.TableEnd);
                        w.Write(hue.RawName);
                    }
                }
                w.Write(Tail);
                w.Flush();
                return ms.ToArray();
            }
        }

        public ClassicHueFile Clone() => FromBytes(ToBytes());
    }
}
