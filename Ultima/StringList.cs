using System.Collections.Generic;
using System.IO;
using System.Text;
using Ultima.Helpers;

namespace Ultima
{
    public sealed class StringList
    {
        private readonly bool _decompress;
        private int _header1;
        private short _header2;

        public List<StringEntry> Entries { get; private set; }
        public string Language { get; }

        private Dictionary<int, string> _stringTable;
        private Dictionary<int, StringEntry> _entryTable;
        private static byte[] _buffer = new byte[1024];

        /// <summary>
        /// Initialize <see cref="StringList"/> of Language
        /// </summary>
        /// <param name="language"></param>
        /// <param name="decompress"></param>
        public StringList(string language, bool decompress)
        {
            _decompress = decompress;
            Language = language;
            LoadEntry(Files.GetFilePath($"cliloc.{language}"));
        }

        /// <summary>
        /// Initialize <see cref="StringList"/> of Language from path
        /// </summary>
        /// <param name="language"></param>
        /// <param name="path"></param>
        /// <param name="decompress"></param>
        public StringList(string language, string path, bool decompress)
        {
            _decompress = decompress;
            Language = language;
            LoadEntry(path);
        }

        private void LoadEntry(string path)
        {
            if (path == null)
            {
                Entries = new List<StringEntry>(0);
                return;
            }

            Entries = new List<StringEntry>();
            _stringTable = new Dictionary<int, string>();
            _entryTable = new Dictionary<int, StringEntry>();

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] buffer = new byte[fileStream.Length];
                _ = fileStream.Read(buffer, 0, buffer.Length);

                byte[] clilocData = _decompress
                    ? MythicDecompress.Decompress(buffer)
                    : buffer;

                using (var reader = new BinaryReader(new MemoryStream(clilocData)))
                {
                    _header1 = reader.ReadInt32();
                    _header2 = reader.ReadInt16();

                    while (reader.BaseStream.Length != reader.BaseStream.Position)
                    {
                        int number = reader.ReadInt32();
                        byte flag = reader.ReadByte();
                        int length = reader.ReadInt16();

                        if (length > _buffer.Length)
                        {
                            _buffer = new byte[(length + 1023) & ~1023];
                        }

                        reader.Read(_buffer, 0, length);
                        string text = Encoding.UTF8.GetString(_buffer, 0, length);

                        var se = new StringEntry(number, text, flag);
                        Entries.Add(se);

                        _stringTable[number] = text;
                        _entryTable[number] = se;
                    }
                }
            }
        }

        /// <summary>
        /// Saves <see cref="SaveStringList"/> to fileName
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveStringList(string fileName)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var bin = new BinaryWriter(memoryStream))
                {
                    // Sort entries by number
                    Entries.Sort(new NumberComparer(false));

                    // Write each entry to the memory stream
                    foreach (StringEntry entry in Entries)
                    {
                        bin.Write(entry.Number);
                        bin.Write((byte)entry.Flag);
                        byte[] utf8String = Encoding.UTF8.GetBytes(entry.Text);
                        var length = (ushort)utf8String.Length;
                        bin.Write(length);
                        bin.Write(utf8String);
                    }
                }

                byte[] data = memoryStream.ToArray();
                if (_decompress)
                {
                    byte[] data2 = new byte[data.Length + 6];
                    using (MemoryStream ms = new MemoryStream(data2))
                    {
                        using (var bin = new BinaryWriter(ms))
                        {
                            bin.Write(_header1);
                            bin.Write(_header2);

                            bin.Write(data);
                        }
                    }
                    uint length = (uint)data2.Length;
                    data2 = MythicDecompress.Transform(data2);
                    byte[] data3 = new byte[data2.Length + 4];

                    using (MemoryStream ms = new MemoryStream(data3))
                    using (var bin = new BinaryWriter(ms))
                    {
                        bin.Write((uint)length ^ 0x8E2C9A3D); // xored decrypted data length
                        bin.Write(data2);
                    }
                    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var bin = new BinaryWriter(fileStream))
                    {
                        bin.Write(data3);
                    }
                }
                else
                {
                    // Write the final output to the file
                    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var bin = new BinaryWriter(fileStream))
                    {
                        // Write the headers at the beginning
                        bin.Write(_header1);
                        bin.Write(_header2);

                        bin.Write(data);
                    }
                }
            }
        }

        public string GetString(int number)
        {
            return _stringTable?.ContainsKey(number) != true ? null : _stringTable[number];
        }

        public StringEntry GetEntry(int number)
        {
            return _entryTable?.ContainsKey(number) != true ? null : _entryTable[number];
        }

        public class NumberComparer : IComparer<StringEntry>
        {
            private readonly bool _sortDescending;

            public NumberComparer(bool sortDescending)
            {
                _sortDescending = sortDescending;
            }

            public int Compare(StringEntry x, StringEntry y)
            {
                if (x.Number == y.Number)
                {
                    return 0;
                }
                else if (_sortDescending)
                {
                    return (x.Number < y.Number) ? 1 : -1;
                }
                else
                {
                    return (x.Number < y.Number) ? -1 : 1;
                }
            }
        }

        public class FlagComparer : IComparer<StringEntry>
        {
            private readonly bool _sortDescending;

            public FlagComparer(bool sortDescending)
            {
                _sortDescending = sortDescending;
            }

            public int Compare(StringEntry x, StringEntry y)
            {
                if ((byte)x.Flag == (byte)y.Flag)
                {
                    if (x.Number == y.Number)
                    {
                        return 0;
                    }

                    if (_sortDescending)
                    {
                        return (x.Number < y.Number) ? 1 : -1;
                    }
                    else
                    {
                        return (x.Number < y.Number) ? -1 : 1;
                    }
                }

                if (_sortDescending)
                {
                    return ((byte)x.Flag < (byte)y.Flag) ? 1 : -1;
                }
                else
                {
                    return ((byte)x.Flag < (byte)y.Flag) ? -1 : 1;
                }
            }
        }

        public class TextComparer : IComparer<StringEntry>
        {
            private readonly bool _sortDescending;

            public TextComparer(bool sortDescending)
            {
                _sortDescending = sortDescending;
            }

            public int Compare(StringEntry x, StringEntry y)
            {
                return _sortDescending
                    ? string.CompareOrdinal(y.Text, x.Text)
                    : string.CompareOrdinal(x.Text, y.Text);
            }
        }
    }
}