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
using System.Text;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Uop
{
    /// <summary>
    /// List of known packed file names by hash. Reads and writes the ".dic" format of the Mythic Package Editor
    /// ("DIC\0", version byte 1, then per entry: ulong hash, byte has-name, .NET string).
    /// </summary>
    public sealed class EcNameDictionary
    {
        private readonly Dictionary<ulong, string> _names = new Dictionary<ulong, string>();

        public int Count => _names.Count;

        public string FilePath { get; private set; }

        public bool TryGet(ulong hash, out string name) => _names.TryGetValue(hash, out name);

        public void Add(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _names[EcUopHash.HashFileName(name)] = name;
            }
        }

        public static EcNameDictionary Load(string path)
        {
            var dictionary = new EcNameDictionary { FilePath = path };
            using (var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8))
            {
                byte[] id = reader.ReadBytes(4);
                if (id.Length < 4 || id[0] != 'D' || id[1] != 'I' || id[2] != 'C')
                {
                    throw new InvalidDataException("Not a name dictionary (.dic).");
                }

                reader.ReadByte(); // version
                long length = reader.BaseStream.Length;
                while (reader.BaseStream.Position + 9 <= length)
                {
                    ulong hash = reader.ReadUInt64();
                    if (reader.ReadByte() == 1)
                    {
                        dictionary._names[hash] = reader.ReadString();
                    }
                }
            }
            return dictionary;
        }

        public void Save(string path)
        {
            using (var writer = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write), Encoding.UTF8))
            {
                writer.Write((byte)'D');
                writer.Write((byte)'I');
                writer.Write((byte)'C');
                writer.Write((byte)0);
                writer.Write((byte)1);
                foreach (KeyValuePair<ulong, string> kv in _names)
                {
                    writer.Write(kv.Key);
                    writer.Write((byte)1);
                    writer.Write(kv.Value);
                }
            }
        }

        /// <summary>
        /// Gives the entries of the package the names found in the dictionary. Returns the number of new names.
        /// </summary>
        public int Apply(EcUopPackage package)
        {
            int count = 0;
            foreach (EcUopEntry entry in package.Entries)
            {
                if (entry.Name == null && _names.TryGetValue(entry.Hash, out string name))
                {
                    entry.Name = name;
                    count++;
                }
            }
            return count;
        }
    }
}
