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

namespace UoFiddler.Plugin.Enhanced.EcUltima.Strings
{
    public sealed class EcClilocEntry
    {
        public int Id { get; set; }
        public byte Flag { get; set; }
        public string Text { get; set; }
    }

    /// <summary>
    /// One language of LocalizedStrings.uop ("data/localizedstrings/{nr:D3}.cliloc").
    /// Layout: uint version, ushort unknown, then entries: int id, byte flag, ushort length, UTF-8 text.
    /// </summary>
    public sealed class EcCliloc
    {
        public static readonly string[] Languages =
        {
            "English", "Japanese", "Korean", "French", "German", "Spanish", "Unused", "Chinese (traditional)"
        };

        public const string NamePattern = "data/localizedstrings/{0:D3}.cliloc";

        public uint Version { get; set; } = 2;
        public ushort Unknown { get; set; } = 1;
        public List<EcClilocEntry> Entries { get; } = new List<EcClilocEntry>();

        public static string LanguageName(int number) => number >= 1 && number <= Languages.Length ? Languages[number - 1] : $"Language {number}";

        public static EcCliloc Read(byte[] data)
        {
            var cliloc = new EcCliloc
            {
                Version = BitConverter.ToUInt32(data, 0),
                Unknown = BitConverter.ToUInt16(data, 4)
            };

            int p = 6;
            while (p + 7 <= data.Length)
            {
                int id = BitConverter.ToInt32(data, p);
                byte flag = data[p + 4];
                int length = BitConverter.ToUInt16(data, p + 5);
                p += 7;
                if (p + length > data.Length)
                {
                    throw new InvalidDataException($"Cliloc entry {id} is cut off.");
                }
                cliloc.Entries.Add(new EcClilocEntry { Id = id, Flag = flag, Text = Encoding.UTF8.GetString(data, p, length) });
                p += length;
            }
            return cliloc;
        }

        public byte[] Write()
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(Version);
                w.Write(Unknown);
                foreach (EcClilocEntry e in Entries)
                {
                    byte[] text = Encoding.UTF8.GetBytes(e.Text ?? string.Empty);
                    if (text.Length > ushort.MaxValue)
                    {
                        throw new InvalidOperationException($"Text of {e.Id} is too long.");
                    }
                    w.Write(e.Id);
                    w.Write(e.Flag);
                    w.Write((ushort)text.Length);
                    w.Write(text);
                }
                w.Flush();
                return ms.ToArray();
            }
        }

        public EcClilocEntry Find(int id) => Entries.FirstOrDefault(e => e.Id == id);

        /// <summary>Adds or changes an entry; entries stay sorted by id.</summary>
        public EcClilocEntry Set(int id, string text)
        {
            EcClilocEntry entry = Find(id);
            if (entry != null)
            {
                entry.Text = text;
                return entry;
            }

            entry = new EcClilocEntry { Id = id, Text = text };
            int index = Entries.FindIndex(e => e.Id > id);
            if (index < 0)
            {
                Entries.Add(entry);
            }
            else
            {
                Entries.Insert(index, entry);
            }
            return entry;
        }
    }
}
