/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace UoFiddler.Plugin.UopPacker.Classes
{
    /// <summary>One entry of the table of contents of a UOP file.</summary>
    public sealed class UopInfoEntry
    {
        public int Index { get; set; }
        public long Offset { get; set; }
        public int HeaderLength { get; set; }
        public int Size { get; set; }
        public int DecompressedSize { get; set; }
        public ulong Hash { get; set; }
        public uint Checksum { get; set; }
        public short Compression { get; set; }

        /// <summary>Packed file name if it is known, else null.</summary>
        public string Name { get; set; }

        /// <summary>null = not checked, true / false = Adler-32 of the stored data matches.</summary>
        public bool? ChecksumOk { get; set; }
    }

    /// <summary>Header, table of contents and statistics of a UOP file (tab "UOP Info").</summary>
    public sealed class UopInfo
    {
        public string Path { get; set; }
        public long FileSize { get; set; }
        public int Version { get; set; }
        public uint Signature { get; set; }
        public long FirstTable { get; set; }
        public int TableSize { get; set; }
        public int FileCount { get; set; }
        public int Tables { get; set; }
        public UopFileSet Set { get; set; }
        public List<UopInfoEntry> Entries { get; } = new List<UopInfoEntry>();
        public List<string> Problems { get; } = new List<string>();

        public string Summary()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"File:        {Path}");
            sb.AppendLine($"Size:        {FileSize:N0} bytes");
            sb.AppendLine($"Type:        {(Set != null ? Set.Title : "unknown (names are not resolved)")}");
            sb.AppendLine($"Version:     {Version}   signature 0x{Signature:X8}");
            sb.AppendLine($"Tables:      {Tables} (first at 0x{FirstTable:X}, {TableSize} entries each)");
            sb.AppendLine($"Entries:     {Entries.Count} (header says {FileCount})");
            foreach (var g in Entries.GroupBy(e => e.Compression).OrderBy(g => g.Key))
            {
                string name = g.Key switch { 0 => "none", 1 => "zlib", 3 => "Mythic (zlib + transform)", _ => "unknown" };
                sb.AppendLine($"Compression: {name,-26} {g.Count(),8} entries");
            }
            long stored = Entries.Sum(e => (long)e.Size), unpacked = Entries.Sum(e => (long)e.DecompressedSize);
            sb.AppendLine($"Data:        {stored:N0} bytes stored, {unpacked:N0} bytes unpacked");
            int named = Entries.Count(e => e.Name != null);
            sb.AppendLine($"Names:       {named} known, {Entries.Count - named} unknown");
            int @checked = Entries.Count(e => e.ChecksumOk.HasValue);
            if (@checked > 0)
            {
                sb.AppendLine($"Checksums:   {Entries.Count(e => e.ChecksumOk == true)} ok, {Entries.Count(e => e.ChecksumOk == false)} wrong (of {@checked} checked)");
            }
            if (Problems.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Problems:");
                foreach (string p in Problems.Take(50))
                {
                    sb.AppendLine("  " + p);
                }
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("No problems found.");
            }
            return sb.ToString();
        }
    }

    public static class UopInspector
    {
        /// <summary>
        /// Reads the header and the table of contents; names are resolved for the file set found by the file name.
        /// <paramref name="verify"/> reads all data and checks the Adler-32 checksums (takes longer).
        /// </summary>
        public static UopInfo Read(string path, bool verify, IProgress<int> progress = null, CancellationToken cancel = default)
        {
            var info = new UopInfo { Path = path, Set = UopFileTypes.Detect(path) };
            using (var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                Stream stream = reader.BaseStream;
                info.FileSize = stream.Length;
                if (stream.Length < 0x28 || reader.ReadInt32() != 0x50594D)
                {
                    info.Problems.Add("Not a UOP (MYP) file.");
                    return info;
                }
                info.Version = reader.ReadInt32();
                info.Signature = reader.ReadUInt32();
                info.FirstTable = reader.ReadInt64();
                info.TableSize = reader.ReadInt32();
                info.FileCount = reader.ReadInt32();

                Dictionary<ulong, string> names = Names(info.Set);
                var seen = new HashSet<ulong>();
                long next = info.FirstTable;
                int index = 0;
                var visited = new HashSet<long>();
                while (next != 0)
                {
                    cancel.ThrowIfCancellationRequested();
                    if (next < 0 || next >= stream.Length || !visited.Add(next))
                    {
                        info.Problems.Add($"Table address 0x{next:X} is outside the file or repeats – the table chain is broken.");
                        break;
                    }
                    stream.Seek(next, SeekOrigin.Begin);
                    int count = reader.ReadInt32();
                    next = reader.ReadInt64();
                    info.Tables++;
                    for (int i = 0; i < count; i++)
                    {
                        var e = new UopInfoEntry
                        {
                            Offset = reader.ReadInt64(),
                            HeaderLength = reader.ReadInt32(),
                            Size = reader.ReadInt32(),
                            DecompressedSize = reader.ReadInt32(),
                            Hash = reader.ReadUInt64(),
                            Checksum = reader.ReadUInt32(),
                            Compression = reader.ReadInt16()
                        };
                        if (e.Offset == 0)
                        {
                            continue;
                        }
                        e.Index = index++;
                        names.TryGetValue(e.Hash, out string name);
                        e.Name = name;
                        if (e.Offset + e.HeaderLength + e.Size > stream.Length)
                        {
                            info.Problems.Add($"Entry {e.Index} ({e.Name ?? e.Hash.ToString("X16")}) points behind the end of the file.");
                        }
                        if (!seen.Add(e.Hash))
                        {
                            info.Problems.Add($"Entry {e.Index}: name hash {e.Hash:X16} is used twice.");
                        }
                        if (e.Compression != 0 && e.Compression != 1 && e.Compression != 3)
                        {
                            info.Problems.Add($"Entry {e.Index}: unknown compression {e.Compression}.");
                        }
                        if (e.Compression == 0 && e.Size != e.DecompressedSize)
                        {
                            info.Problems.Add($"Entry {e.Index}: not compressed, but the sizes differ ({e.Size} / {e.DecompressedSize}).");
                        }
                        info.Entries.Add(e);
                    }
                }

                if (info.Set != null && info.Entries.Count(e => e.Name == null) > 0)
                {
                    info.Problems.Add($"{info.Entries.Count(e => e.Name == null)} entries have a name that does not fit {info.Set.Uop}.");
                }

                if (verify)
                {
                    for (int i = 0; i < info.Entries.Count; i++)
                    {
                        cancel.ThrowIfCancellationRequested();
                        UopInfoEntry e = info.Entries[i];
                        if (e.Offset + e.HeaderLength + e.Size > stream.Length)
                        {
                            e.ChecksumOk = false;
                            continue;
                        }
                        stream.Seek(e.Offset + e.HeaderLength, SeekOrigin.Begin);
                        byte[] data = reader.ReadBytes(e.Size);
                        e.ChecksumOk = LegacyMulFileConverter.HashAdler32(data) == e.Checksum;
                        if (e.ChecksumOk == false && info.Problems.Count < 200)
                        {
                            info.Problems.Add($"Entry {e.Index} ({e.Name ?? e.Hash.ToString("X16")}): Adler-32 checksum does not match.");
                        }
                        if (i % 256 == 0)
                        {
                            progress?.Report(i * 100 / Math.Max(1, info.Entries.Count));
                        }
                    }
                }
            }
            progress?.Report(100);
            return info;
        }

        /// <summary>All packed names of a file set (hash → name).</summary>
        private static Dictionary<ulong, string> Names(UopFileSet set)
        {
            var names = new Dictionary<ulong, string>();
            if (set == null)
            {
                return names;
            }
            string[] formats = UopFileTypes.GetHashFormat(set.Type, set.TypeIndex, out int maxId);
            for (int i = 0; i < maxId; i++)
            {
                foreach (string f in formats.Where(f => f.Length > 0))
                {
                    string name = string.Format(f, i);
                    names.TryAdd(LegacyMulFileConverter.HashLittle2(name), name);
                }
            }
            if (set.Type == FileType.MultiCollection)
            {
                names[LegacyMulFileConverter.HashLittle2(UopFileTypes.HousingBinName)] = UopFileTypes.HousingBinName;
            }
            return names;
        }
    }
}
