using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ultima;

namespace UoFiddler.Controls.Uop
{
    public class MainMiscManager
    {
        private const ulong MAIN_MISC_TABLE_HASH = 0xC0165B63D153B1DD;
        
        private struct TableEntry
        {
            public uint Id;
            public uint Flag;
        }

        private struct FileEntry
        {
            public byte[] Data;
            public uint HeaderSize;
            public byte[] HeaderBytes;
        }

        private List<TableEntry> _table = new List<TableEntry>();
        private Dictionary<ulong, FileEntry> _otherFiles = new Dictionary<ulong, FileEntry>();
        
        public bool IsModified { get; private set; }
        public bool IsLoaded { get; private set; }
        public int EntryCount => _table.Count;

        public bool Load()
        {
            string path = Files.GetFilePath("MainMisc.uop") ?? Path.Combine(Files.RootDir, "MainMisc.uop");
            System.Diagnostics.Debug.WriteLine($"🔍 [MainMisc] Loading from: {path}");

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                System.Diagnostics.Debug.WriteLine("❌ [MainMisc] File NOT FOUND.");
                return false;
            }

            _table.Clear();
            _otherFiles.Clear();
            IsModified = false;

            var reader = new UopFileReader(path);
            if (!reader.Load())
            {
                System.Diagnostics.Debug.WriteLine("❌ [MainMisc] UopFileReader failed to load file.");
                return false;
            }

            var seenOffsets = new HashSet<ulong>();

            foreach (var group in reader.GroupedEntries)
            {
                byte[] fullData;
                uint headerSize = 0;
                byte[] headerBytes = null;

                using (var ms = new MemoryStream())
                {
                    foreach (var entryHeader in group.Value)
                    {
                        if (seenOffsets.Contains(entryHeader.Offset)) continue;
                        seenOffsets.Add(entryHeader.Offset);

                        if (headerSize == 0)
                        {
                            headerSize = entryHeader.HeaderSize;
                            headerBytes = reader.ReadHeaderData(entryHeader);
                        }

                        byte[] part = reader.ReadData(entryHeader);
                        if (part != null) ms.Write(part, 0, part.Length);
                    }
                    fullData = ms.ToArray();
                }

                if (fullData.Length == 0) continue;

                if (group.Key == MAIN_MISC_TABLE_HASH)
                {
                    using (var ms = new MemoryStream(fullData))
                    using (var bin = new BinaryReader(ms))
                    {
                        long rawEntryCount = fullData.Length / 8;
                        for (int i = 0; i < rawEntryCount; i++)
                        {
                            _table.Add(new TableEntry { 
                                Id = bin.ReadUInt32(), 
                                Flag = bin.ReadUInt32() 
                            });
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"✅ [MainMisc] Loaded {_table.Count} entries.");
                }
                else
                {
                    _otherFiles[group.Key] = new FileEntry { Data = fullData, HeaderSize = headerSize, HeaderBytes = headerBytes };
                }
            }
            IsLoaded = true;
            return true;
        }

        public bool ContainsId(uint id, uint flag) 
        {
            return _table.Any(e => e.Id == id && e.Flag == flag);
        }

        public uint GetIdFlag(uint id)
        {
            var entry = _table.FirstOrDefault(e => e.Id == id && e.Flag == 0x0C000000);
            if (entry.Id == id && entry.Flag == 0x0C000000) return entry.Flag;
            entry = _table.FirstOrDefault(e => e.Id == id);
            return entry.Id == id ? entry.Flag : 0;
        }

        public void AddId(uint id, uint flag = 0x0C000000)
        {
            if (!ContainsId(id, flag))
            {
                _table.Add(new TableEntry { Id = id, Flag = flag });
                IsModified = true;
                System.Diagnostics.Debug.WriteLine($"✅ [MainMisc] Added Entry: ID {id} with Flag {flag:X8}");
            }
        }

        public bool RemoveId(uint id, uint flag = 0x0C000000)
        {
            int removed = _table.RemoveAll(e => e.Id == id && e.Flag == flag);
            if (removed > 0)
            {
                IsModified = true;
                return true;
            }
            return false;
        }

        public void ClearTable()
        {
            _table.Clear();
            IsModified = true;
        }

        public void Save(string outputPath)
        {
            var fileDataList = new List<UopFileData>();

            foreach (var kvp in _otherFiles)
            {
                fileDataList.Add(new UopFileData
                {
                    Hash = kvp.Key,
                    Data = kvp.Value.Data,
                    HeaderSize = kvp.Value.HeaderSize,
                    HeaderBytes = kvp.Value.HeaderBytes,
                    DecompressedSize = (uint)kvp.Value.Data.Length,
                    IsCompressed = true,
                    IsEmpty = false
                });
            }

            _table.Sort((a, b) => {
                int res = a.Id.CompareTo(b.Id);
                return res != 0 ? res : a.Flag.CompareTo(b.Flag);
            });

            byte[] tableData;
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                foreach (var entry in _table)
                {
                    writer.Write(entry.Id);
                    writer.Write(entry.Flag);
                }
                tableData = ms.ToArray();
            }

            fileDataList.Add(new UopFileData
            {
                Hash = MAIN_MISC_TABLE_HASH,
                Data = tableData,
                DecompressedSize = (uint)tableData.Length,
                IsCompressed = true,
                IsEmpty = false,
                HeaderSize = 0
            });

            fileDataList = fileDataList.OrderBy(e => e.Hash).ToList();
            UopFileWriter.WriteUopFile(outputPath, fileDataList, 100);
            IsModified = false;
        }
    }
}