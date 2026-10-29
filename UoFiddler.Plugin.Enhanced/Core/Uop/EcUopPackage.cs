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
using System.IO.Compression;
using System.Linq;

namespace UoFiddler.Plugin.Enhanced.Core.Uop
{
    /// <summary>
    /// Compression flag of an UOP entry.
    /// </summary>
    public enum EcUopCompression : ushort
    {
        None = 0,
        Zlib = 1,
    }

    /// <summary>
    /// One file packed inside an UOP package.
    /// </summary>
    public sealed class EcUopEntry
    {
        public ulong Hash { get; internal set; }

        /// <summary>
        /// Original file name (only known when it was resolved through <see cref="EcUopPackage.ResolveName"/>).
        /// </summary>
        public string Name { get; internal set; }

        public long Offset { get; internal set; }
        public int HeaderSize { get; internal set; }
        public int CompressedSize { get; internal set; }
        public int DecompressedSize { get; internal set; }
        public ushort Flag { get; internal set; }
        public uint HeaderHash { get; internal set; }

        /// <summary>
        /// Position in the original table of contents (block / slot).
        /// </summary>
        public int Block { get; internal set; }
        public int Slot { get; internal set; }

        public bool IsModified => NewData != null;
        public bool IsNew { get; internal set; }

        /// <summary>
        /// Flags other than 0 (none) and 1 (zlib) can't be decoded by this plugin. The data is kept as it is.
        /// </summary>
        public bool IsSupported => Flag == (ushort)EcUopCompression.None || Flag == (ushort)EcUopCompression.Zlib;

        internal byte[] HeaderBytes { get; set; }
        internal byte[] NewData { get; set; }

        public string DisplayName => Name ?? $"0x{Hash:X16}";

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// Reader/writer for Mythic UOP packages (MYP format, version 4/5) as used by the Enhanced Client.
    /// The table of contents is read when opening; the data of the entries is read on demand,
    /// so even large files (Texture.uop) open quickly. Unchanged entries are copied byte by byte when saving.
    /// </summary>
    public sealed class EcUopPackage : IDisposable
    {
        public const uint Magic = 0x50594D; // "MYP\0"
        private const int HeaderLength = 40;
        private const int TocEntryLength = 34;
        private const int TocHeaderLength = 12;

        private readonly List<EcUopEntry> _entries = new List<EcUopEntry>();
        private readonly Dictionary<ulong, EcUopEntry> _byHash = new Dictionary<ulong, EcUopEntry>();
        private readonly HashSet<ulong> _removed = new HashSet<ulong>();
        private FileStream _stream;
        private byte[] _headerTail = new byte[HeaderLength - 28];

        public string FilePath { get; private set; }
        public uint Version { get; private set; } = 4;
        public uint Signature { get; private set; } = 0xFD23EC43;
        public uint BlockCapacity { get; private set; } = 100;
        public int DeclaredFileCount { get; private set; }

        public IReadOnlyList<EcUopEntry> Entries => _entries;
        public int Count => _entries.Count;

        /// <summary>
        /// True when entries were changed, added or removed since the last load/save.
        /// </summary>
        public bool IsDirty { get; private set; }

        public event EventHandler Changed;

        private EcUopPackage()
        {
        }

        public static EcUopPackage Open(string path)
        {
            var package = new EcUopPackage();
            package.Load(path);
            return package;
        }

        /// <summary>
        /// Creates an empty package in memory. It needs to be saved with <see cref="Save"/> before it exists on disk.
        /// </summary>
        public static EcUopPackage CreateEmpty(uint blockCapacity = 100)
        {
            return new EcUopPackage { BlockCapacity = blockCapacity, IsDirty = true };
        }

        private void Load(string path)
        {
            CloseStream();
            _entries.Clear();
            _byHash.Clear();
            _removed.Clear();
            IsDirty = false;

            FilePath = path;
            _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var reader = new BinaryReader(_stream);

            if (_stream.Length < HeaderLength || reader.ReadUInt32() != Magic)
            {
                CloseStream();
                throw new InvalidDataException($"'{Path.GetFileName(path)}' is not a valid UOP file.");
            }

            Version = reader.ReadUInt32();
            Signature = reader.ReadUInt32();
            long nextTable = reader.ReadInt64();
            BlockCapacity = reader.ReadUInt32();
            DeclaredFileCount = reader.ReadInt32();
            _headerTail = reader.ReadBytes(HeaderLength - 28);

            int block = 0;
            var visited = new HashSet<long>();
            while (nextTable > 0 && nextTable < _stream.Length && visited.Add(nextTable))
            {
                _stream.Seek(nextTable, SeekOrigin.Begin);
                int filesInBlock = reader.ReadInt32();
                nextTable = reader.ReadInt64();

                for (int slot = 0; slot < filesInBlock; slot++)
                {
                    long offset = reader.ReadInt64();
                    int headerSize = reader.ReadInt32();
                    int compressedSize = reader.ReadInt32();
                    int decompressedSize = reader.ReadInt32();
                    ulong hash = reader.ReadUInt64();
                    uint headerHash = reader.ReadUInt32();
                    ushort flag = reader.ReadUInt16();

                    if (offset == 0 || _byHash.ContainsKey(hash))
                    {
                        continue;
                    }

                    var entry = new EcUopEntry
                    {
                        Hash = hash,
                        Offset = offset,
                        HeaderSize = headerSize,
                        CompressedSize = compressedSize,
                        DecompressedSize = decompressedSize,
                        HeaderHash = headerHash,
                        Flag = flag,
                        Block = block,
                        Slot = slot
                    };
                    _entries.Add(entry);
                    _byHash[hash] = entry;
                }

                block++;
            }
        }

        public EcUopEntry GetEntry(ulong hash)
        {
            return _byHash.TryGetValue(hash, out EcUopEntry entry) ? entry : null;
        }

        public EcUopEntry GetEntry(string name)
        {
            EcUopEntry entry = GetEntry(EcUopHash.HashFileName(name));
            if (entry != null && entry.Name == null)
            {
                entry.Name = name;
            }
            return entry;
        }

        public bool Contains(string name) => _byHash.ContainsKey(EcUopHash.HashFileName(name));

        /// <summary>
        /// Assigns the given name to the entry with the matching hash. Returns false if no entry matches.
        /// </summary>
        public bool ResolveName(string name)
        {
            if (_byHash.TryGetValue(EcUopHash.HashFileName(name), out EcUopEntry entry))
            {
                entry.Name = name;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries the name pattern (e.g. "build/worldart/{0:D8}.dds") for every id in the range and returns the number of hits.
        /// </summary>
        public int ResolveNames(string pattern, int firstId, int lastId)
        {
            int hits = 0;
            for (int id = firstId; id <= lastId; id++)
            {
                if (ResolveName(string.Format(pattern, id)))
                {
                    hits++;
                }
            }
            return hits;
        }

        public int ResolveNames(string pattern, IEnumerable<int> ids)
        {
            int hits = 0;
            foreach (int id in ids)
            {
                if (ResolveName(string.Format(pattern, id)))
                {
                    hits++;
                }
            }
            return hits;
        }

        /// <summary>
        /// Raw bytes as stored in the file (compressed, without the metadata header).
        /// </summary>
        public byte[] ReadRaw(EcUopEntry entry)
        {
            if (entry.NewData != null)
            {
                return entry.Flag == (ushort)EcUopCompression.Zlib ? Compress(entry.NewData) : entry.NewData;
            }

            EnsureStream();
            int size = entry.Flag == (ushort)EcUopCompression.None ? entry.DecompressedSize : entry.CompressedSize;
            var buffer = new byte[size];
            lock (_stream)
            {
                _stream.Seek(entry.Offset + entry.HeaderSize, SeekOrigin.Begin);
                ReadExactly(_stream, buffer, size);
            }
            return buffer;
        }

        /// <summary>
        /// Decompressed data of the entry.
        /// </summary>
        public byte[] ReadData(EcUopEntry entry)
        {
            if (entry == null)
            {
                return null;
            }

            if (entry.NewData != null)
            {
                return entry.NewData;
            }

            byte[] raw = ReadRaw(entry);
            switch (entry.Flag)
            {
                case (ushort)EcUopCompression.None:
                    return raw;
                case (ushort)EcUopCompression.Zlib:
                    return Decompress(raw, entry.DecompressedSize);
                default:
                    throw new NotSupportedException($"Compression flag {entry.Flag} is not supported.");
            }
        }

        public byte[] ReadData(string name) => ReadData(GetEntry(name));

        private byte[] ReadHeaderBytes(EcUopEntry entry)
        {
            if (entry.HeaderBytes != null)
            {
                return entry.HeaderBytes;
            }

            if (entry.HeaderSize <= 0 || entry.IsNew)
            {
                return Array.Empty<byte>();
            }

            EnsureStream();
            var buffer = new byte[entry.HeaderSize];
            lock (_stream)
            {
                _stream.Seek(entry.Offset, SeekOrigin.Begin);
                ReadExactly(_stream, buffer, buffer.Length);
            }
            return buffer;
        }

        /// <summary>
        /// Replaces the data of an existing entry. The compression of the entry stays the same.
        /// </summary>
        public void SetData(EcUopEntry entry, byte[] data)
        {
            if (!entry.IsSupported)
            {
                throw new NotSupportedException($"Entry {entry.DisplayName} uses an unknown compression ({entry.Flag}) and can't be changed.");
            }

            entry.NewData = data ?? throw new ArgumentNullException(nameof(data));
            entry.DecompressedSize = data.Length;
            MarkDirty();
        }

        /// <summary>
        /// Replaces the entry with the given name, or adds a new one if it doesn't exist.
        /// </summary>
        public EcUopEntry AddOrReplace(string name, byte[] data, EcUopCompression compression)
        {
            ulong hash = EcUopHash.HashFileName(name);
            if (_byHash.TryGetValue(hash, out EcUopEntry existing))
            {
                existing.Name ??= name;
                SetData(existing, data);
                return existing;
            }

            var entry = new EcUopEntry
            {
                Hash = hash,
                Name = name,
                Flag = (ushort)compression,
                IsNew = true,
                HeaderBytes = CreateMetadataHeader(),
                NewData = data,
                DecompressedSize = data.Length,
                Block = -1,
                Slot = -1
            };
            entry.HeaderSize = entry.HeaderBytes.Length;
            _entries.Add(entry);
            _byHash[hash] = entry;
            _removed.Remove(hash);
            MarkDirty();
            return entry;
        }

        public bool Remove(EcUopEntry entry)
        {
            if (entry == null || !_byHash.Remove(entry.Hash))
            {
                return false;
            }

            _entries.Remove(entry);
            _removed.Add(entry.Hash);
            MarkDirty();
            return true;
        }

        public bool Remove(string name) => Remove(GetEntry(name));

        /// <summary>
        /// The compression most entries of this package use (used for new entries).
        /// </summary>
        public EcUopCompression PreferredCompression
        {
            get
            {
                int zlib = _entries.Count(e => e.Flag == (ushort)EcUopCompression.Zlib);
                return zlib > _entries.Count / 2 ? EcUopCompression.Zlib : EcUopCompression.None;
            }
        }

        /// <summary>
        /// Writes the package. When the target file exists, a temporary file is written first and the old file is kept
        /// as backup (if <paramref name="backup"/> is true). Afterwards the package works with the new file;
        /// the <see cref="EcUopEntry"/> objects stay valid.
        /// </summary>
        public void Save(string path, bool backup = true)
        {
            string fullTarget = Path.GetFullPath(path);
            bool overwriteSource = FilePath != null && string.Equals(fullTarget, Path.GetFullPath(FilePath), StringComparison.OrdinalIgnoreCase);
            string writePath = overwriteSource || File.Exists(fullTarget) ? fullTarget + ".tmp" : fullTarget;

            string directory = Path.GetDirectoryName(fullTarget);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            List<(EcUopEntry Entry, long Offset, int HeaderSize, int Compressed, int Decompressed, uint HeaderHash, byte[] Header)> written;
            try
            {
                using (var output = new FileStream(writePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    written = Write(output);
                }
            }
            catch
            {
                try
                {
                    File.Delete(writePath);
                }
                catch (IOException)
                {
                }
                throw;
            }

            CloseStream();

            if (writePath != fullTarget)
            {
                if (File.Exists(fullTarget))
                {
                    if (backup)
                    {
                        string backupPath = fullTarget + $".{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                        File.Move(fullTarget, backupPath);
                    }
                    else
                    {
                        File.Delete(fullTarget);
                    }
                }
                File.Move(writePath, fullTarget);
            }

            // The entry objects stay the same (other code keeps references to them); only their position changes.
            FilePath = fullTarget;
            DeclaredFileCount = written.Count;
            for (int i = 0; i < written.Count; i++)
            {
                var w = written[i];
                EcUopEntry entry = w.Entry;
                entry.Offset = w.Offset;
                entry.HeaderSize = w.HeaderSize;
                entry.CompressedSize = w.Compressed;
                entry.DecompressedSize = w.Decompressed;
                entry.HeaderHash = w.HeaderHash;
                entry.HeaderBytes = null;
                entry.NewData = null;
                entry.IsNew = false;
                entry.Block = i / (int)Math.Max(1, BlockCapacity);
                entry.Slot = i % (int)Math.Max(1, BlockCapacity);
            }
            _removed.Clear();
            IsDirty = false;

            Changed?.Invoke(this, EventArgs.Empty);
        }

        private List<(EcUopEntry Entry, long Offset, int HeaderSize, int Compressed, int Decompressed, uint HeaderHash, byte[] Header)> Write(Stream output)
        {
            var written = new List<(EcUopEntry, long, int, int, int, uint, byte[])>();
            var writer = new BinaryWriter(output);
            int capacity = (int)Math.Max(1, BlockCapacity);
            List<EcUopEntry> entries = _entries.ToList();

            writer.Write(Magic);
            writer.Write(Version);
            writer.Write(Signature);
            writer.Write((long)(entries.Count > 0 ? HeaderLength : 0));
            writer.Write((uint)capacity);
            writer.Write(entries.Count);
            writer.Write(_headerTail.Length == HeaderLength - 28 ? _headerTail : new byte[HeaderLength - 28]);

            for (int start = 0; start < entries.Count; start += capacity)
            {
                int count = Math.Min(capacity, entries.Count - start);
                long tablePosition = output.Position;
                long tableLength = TocHeaderLength + ((long)capacity * TocEntryLength);

                // Reserve the table, write the data, then come back and fill the table.
                output.Write(new byte[tableLength], 0, (int)tableLength);

                var toc = new (long Offset, int HeaderSize, int Compressed, int Decompressed, ulong Hash, uint HeaderHash, ushort Flag)[count];
                for (int i = 0; i < count; i++)
                {
                    EcUopEntry entry = entries[start + i];
                    byte[] header = ReadHeaderBytes(entry);
                    byte[] raw;
                    int decompressed;

                    if (entry.NewData != null)
                    {
                        raw = entry.Flag == (ushort)EcUopCompression.Zlib ? Compress(entry.NewData) : entry.NewData;
                        decompressed = entry.NewData.Length;
                    }
                    else
                    {
                        raw = ReadRaw(entry);
                        decompressed = entry.DecompressedSize;
                    }

                    long offset = output.Position;
                    output.Write(header, 0, header.Length);
                    output.Write(raw, 0, raw.Length);

                    uint headerHash = header.Length > 0 ? EcUopHash.Adler32(header, 0, header.Length) : entry.HeaderHash;
                    int compressed = entry.Flag == (ushort)EcUopCompression.None ? decompressed : raw.Length;
                    toc[i] = (offset, header.Length, compressed, decompressed, entry.Hash, headerHash, entry.Flag);
                    written.Add((entry, offset, header.Length, compressed, decompressed, headerHash, header));
                }

                long end = output.Position;
                bool last = start + capacity >= entries.Count;

                output.Seek(tablePosition, SeekOrigin.Begin);
                writer.Write(count);
                writer.Write(last ? 0L : end);
                foreach (var t in toc)
                {
                    writer.Write(t.Offset);
                    writer.Write(t.HeaderSize);
                    writer.Write(t.Compressed);
                    writer.Write(t.Decompressed);
                    writer.Write(t.Hash);
                    writer.Write(t.HeaderHash);
                    writer.Write(t.Flag);
                }
                output.Seek(end, SeekOrigin.Begin);
            }

            writer.Flush();
            return written;
        }

        /// <summary>
        /// Metadata header like the one Mythic writes: ushort 3, ushort 8, FILETIME.
        /// </summary>
        private static byte[] CreateMetadataHeader()
        {
            var header = new byte[12];
            BitConverter.GetBytes((ushort)3).CopyTo(header, 0);
            BitConverter.GetBytes((ushort)8).CopyTo(header, 2);
            BitConverter.GetBytes(DateTime.UtcNow.ToFileTimeUtc()).CopyTo(header, 4);
            return header;
        }

        public static byte[] Compress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var zlib = new ZLibStream(ms, CompressionLevel.Optimal, true))
                {
                    zlib.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data, int expectedSize)
        {
            using (var input = new MemoryStream(data))
            using (var zlib = new ZLibStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream(Math.Max(expectedSize, 0)))
            {
                zlib.CopyTo(output);
                return output.ToArray();
            }
        }

        private static void ReadExactly(Stream stream, byte[] buffer, int count)
        {
            int read = 0;
            while (read < count)
            {
                int n = stream.Read(buffer, read, count - read);
                if (n <= 0)
                {
                    throw new EndOfStreamException();
                }
                read += n;
            }
        }

        private void MarkDirty()
        {
            IsDirty = true;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void EnsureStream()
        {
            if (_stream == null)
            {
                if (FilePath == null)
                {
                    throw new InvalidOperationException("The package has no file.");
                }
                _stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
        }

        private void CloseStream()
        {
            _stream?.Dispose();
            _stream = null;
        }

        public void Dispose()
        {
            CloseStream();
        }
    }
}
