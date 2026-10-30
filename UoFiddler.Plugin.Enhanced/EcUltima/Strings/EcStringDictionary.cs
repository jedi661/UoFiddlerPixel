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
using System.Text;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Strings
{
    /// <summary>
    /// string_dictionary.uop – the list of names, shader names and texture paths the tileart entries point to.
    /// References in tileart.uop are 1 based: index n means <see cref="Strings"/>[n - 1].
    /// </summary>
    public sealed class EcStringDictionary
    {
        // Latin1 keeps every byte as it is, so unknown characters survive a load/save cycle.
        private static readonly Encoding _encoding = Encoding.Latin1;

        private readonly List<string> _strings = new List<string>();
        private Dictionary<string, int> _lookup;
        private int _countDelta;

        public EcUopPackage Package { get; private set; }
        public EcUopEntry Entry { get; private set; }

        public ulong Header1 { get; private set; }
        public uint Header2 { get; private set; }

        public IReadOnlyList<string> Strings => _strings;
        public int Count => _strings.Count;
        public bool IsModified { get; private set; }

        public static EcStringDictionary Load(EcUopPackage package)
        {
            if (package.Count == 0)
            {
                throw new InvalidDataException("string_dictionary.uop is empty.");
            }

            var dict = new EcStringDictionary { Package = package, Entry = package.Entries[0] };
            dict.Parse(package.ReadData(dict.Entry));
            return dict;
        }

        private void Parse(byte[] data)
        {
            _strings.Clear();
            Header1 = BitConverter.ToUInt64(data, 0);
            int declared = BitConverter.ToInt32(data, 8);
            Header2 = BitConverter.ToUInt32(data, 12);

            int p = 16;
            while (p + 2 <= data.Length)
            {
                int length = BitConverter.ToUInt16(data, p);
                p += 2;
                if (p + length > data.Length)
                {
                    break;
                }
                _strings.Add(_encoding.GetString(data, p, length));
                p += length;
            }

            // The stored count is one more than the number of strings in the original files; keep that difference.
            _countDelta = declared - _strings.Count;
            _lookup = null;
            IsModified = false;
        }

        /// <summary>
        /// Text for a 1 based reference (as stored in tileart). Returns null for 0 or out of range.
        /// </summary>
        public string Get(uint reference)
        {
            if (reference == 0 || reference > _strings.Count)
            {
                return null;
            }
            return _strings[(int)reference - 1];
        }

        /// <summary>
        /// 1 based reference of the text, or 0 if it doesn't exist.
        /// </summary>
        public uint Find(string text)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<string, int>(StringComparer.Ordinal);
                for (int i = 0; i < _strings.Count; i++)
                {
                    _lookup.TryAdd(_strings[i], i);
                }
            }
            return _lookup.TryGetValue(text, out int index) ? (uint)index + 1 : 0;
        }

        /// <summary>
        /// Returns the reference of the text and adds it at the end if it doesn't exist yet.
        /// </summary>
        public uint GetOrAdd(string text)
        {
            uint existing = Find(text);
            if (existing != 0)
            {
                return existing;
            }

            CheckLength(text);
            _strings.Add(text);
            _lookup[text] = _strings.Count - 1;
            IsModified = true;
            return (uint)_strings.Count;
        }

        /// <summary>
        /// Changes an existing text. Careful: every tileart entry pointing to it changes too.
        /// </summary>
        public void Set(uint reference, string text)
        {
            if (reference == 0 || reference > _strings.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(reference));
            }

            CheckLength(text);
            _strings[(int)reference - 1] = text;
            _lookup = null;
            IsModified = true;
        }

        private static void CheckLength(string text)
        {
            if (text == null || _encoding.GetByteCount(text) > ushort.MaxValue)
            {
                throw new ArgumentException("Text is empty or too long.");
            }
        }

        public byte[] Write()
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(Header1);
                w.Write(_strings.Count + _countDelta);
                w.Write(Header2);
                foreach (string s in _strings)
                {
                    byte[] bytes = _encoding.GetBytes(s);
                    w.Write((ushort)bytes.Length);
                    w.Write(bytes);
                }
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Writes the strings back into the package (the package still needs to be saved).
        /// </summary>
        public void Apply()
        {
            if (!IsModified)
            {
                return;
            }

            Package.SetData(Entry, Write());
            IsModified = false;
        }
    }
}
