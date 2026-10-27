/***************************************************************************
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

namespace UoFiddler.Controls.MapViewer.Build
{
    /// <summary>
    /// Meta data of a template in the <see cref="MultiLibrary"/> (the parts are loaded on demand)
    /// </summary>
    public sealed class MultiLibraryEntry
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public string Author { get; internal set; } = string.Empty;
        public string Category { get; internal set; } = string.Empty;
        public int Count { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int MinZ { get; internal set; }
        public int MaxZ { get; internal set; }
        public DateTime Created { get; internal set; }

        /// <summary>position of the template record in the data file</summary>
        internal long Offset { get; set; }

        public override string ToString() => $"{Id}: {Name}";
    }

    /// <summary>
    /// Own building library for a very large number of templates (millions).
    /// <para>
    /// <c>name.uml</c> is an append-only log: new templates, meta data changes and deletions are only appended
    /// (fast and safe if the program crashes). <c>name.umx</c> is a cache of the index for a fast start;
    /// it is rebuilt from the data file if it is missing or outdated. <see cref="Compact"/> removes old records.
    /// </para>
    /// </summary>
    public sealed class MultiLibrary : IDisposable
    {
        private const string DataMagic = "UOFML1";
        private const string IndexMagic = "UOFMX1";
        private const byte RecordTemplate = 1;
        private const byte RecordMeta = 2;
        private const byte RecordDelete = 3;

        private readonly object _lock = new object();
        private readonly Dictionary<int, MultiLibraryEntry> _entries = new Dictionary<int, MultiLibraryEntry>();
        private bool _indexDirty;
        private int _maxId;

        // length of the data file that is reflected in the index in memory (other instances may append)
        private long _knownLength;

        public MultiLibrary(string dataFile)
        {
            DataFile = Path.GetFullPath(dataFile);
            IndexFile = Path.ChangeExtension(DataFile, ".umx");
            Open();
        }

        public string DataFile { get; }
        public string IndexFile { get; }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _entries.Count;
                }
            }
        }

        public long DataSize => File.Exists(DataFile) ? new FileInfo(DataFile).Length : 0;

        /// <summary>Name of the folder that holds the library (inside the output directory by default)</summary>
        public const string FolderName = "UoFiddler_BuildingLibrary";

        /// <summary>
        /// Default library: <c>OutputPath\UoFiddler_BuildingLibrary\BuildingLibrary.uml</c>
        /// (AppData if no output directory is set)
        /// </summary>
        public static string DefaultFile => Path.Combine(
            string.IsNullOrEmpty(Classes.Options.OutputPath) ? Path.Combine(Classes.Options.AppDataPath ?? AppContext.BaseDirectory, "MultiLibrary") : Path.Combine(Classes.Options.OutputPath, FolderName),
            "BuildingLibrary.uml");

        /// <summary>Library file of older versions (AppData)</summary>
        public static string OldDefaultFile => Path.Combine(Classes.Options.AppDataPath ?? AppContext.BaseDirectory, "MultiLibrary", "MultiLibrary.uml");

        #region Open / index

        private void Open()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DataFile));
            WriteReadme(Path.GetDirectoryName(DataFile));
            if (!File.Exists(DataFile))
            {
                using (var fs = new FileStream(DataFile, FileMode.CreateNew, FileAccess.Write))
                {
                    fs.Write(Encoding.ASCII.GetBytes(DataMagic));
                }
            }

            long replayFrom = TryLoadIndex();
            if (replayFrom < 0)
            {
                _entries.Clear();
                replayFrom = DataMagic.Length;
            }

            Replay(replayFrom);
            _maxId = _entries.Count == 0 ? 0 : _entries.Keys.Max();
            _knownLength = DataSize;
        }

        /// <summary>
        /// Reads records that another instance (e.g. another window) appended. Returns true if something changed.
        /// </summary>
        public bool Refresh()
        {
            lock (_lock)
            {
                return SyncUnlocked();
            }
        }

        private bool SyncUnlocked()
        {
            long size = DataSize;
            if (size == _knownLength)
            {
                return false;
            }

            if (size < _knownLength)
            {
                // the file was compacted by another instance: read everything again
                _entries.Clear();
                Replay(DataMagic.Length);
            }
            else
            {
                Replay(_knownLength);
            }

            _maxId = Math.Max(_maxId, _entries.Count == 0 ? 0 : _entries.Keys.Max());
            _knownLength = DataSize;
            _indexDirty = true;
            return true;
        }

        /// <summary>Returns the data file length stored in the index or -1</summary>
        private long TryLoadIndex()
        {
            if (!File.Exists(IndexFile))
            {
                return -1;
            }

            try
            {
                using (var br = new BinaryReader(new FileStream(IndexFile, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8))
                {
                    if (Encoding.ASCII.GetString(br.ReadBytes(IndexMagic.Length)) != IndexMagic)
                    {
                        return -1;
                    }

                    long dataLength = br.ReadInt64();
                    if (dataLength > DataSize)
                    {
                        return -1;
                    }

                    int count = br.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        var e = new MultiLibraryEntry
                        {
                            Id = br.ReadInt32(),
                            Offset = br.ReadInt64(),
                            Count = br.ReadInt32(),
                            Width = br.ReadInt32(),
                            Height = br.ReadInt32(),
                            MinZ = br.ReadInt32(),
                            MaxZ = br.ReadInt32(),
                            Created = new DateTime(br.ReadInt64()),
                            Name = br.ReadString(),
                            Author = br.ReadString(),
                            Category = br.ReadString()
                        };
                        _entries[e.Id] = e;
                    }

                    return dataLength;
                }
            }
            catch (Exception)
            {
                _entries.Clear();
                return -1;
            }
        }

        /// <summary>Reads the records of the data file from a position (rebuilds the index)</summary>
        private void Replay(long from)
        {
            using (var fs = new FileStream(DataFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var br = new BinaryReader(fs, Encoding.UTF8))
            {
                if (fs.Length < DataMagic.Length || Encoding.ASCII.GetString(br.ReadBytes(DataMagic.Length)) != DataMagic)
                {
                    throw new InvalidDataException($"{DataFile} is not a multi library.");
                }

                fs.Position = from;
                while (fs.Position < fs.Length)
                {
                    long start = fs.Position;
                    try
                    {
                        byte type = br.ReadByte();
                        int id = br.ReadInt32();
                        switch (type)
                        {
                            case RecordTemplate:
                                {
                                    MultiLibraryEntry e = ReadTemplateHeader(br, id, start);
                                    br.BaseStream.Seek(e.Count * 14L, SeekOrigin.Current);
                                    if (fs.Position > fs.Length)
                                    {
                                        throw new EndOfStreamException();
                                    }

                                    _entries[id] = e;
                                    _maxId = Math.Max(_maxId, id);
                                    break;
                                }
                            case RecordMeta:
                                {
                                    string name = br.ReadString(), author = br.ReadString(), category = br.ReadString();
                                    if (_entries.TryGetValue(id, out MultiLibraryEntry e))
                                    {
                                        e.Name = name;
                                        e.Author = author;
                                        e.Category = category;
                                    }

                                    break;
                                }
                            case RecordDelete:
                                _entries.Remove(id);
                                break;
                            default:
                                throw new InvalidDataException($"Unknown record type {type} at {start}");
                        }
                    }
                    catch (EndOfStreamException)
                    {
                        // incomplete last record (program crashed while writing): cut it off
                        fs.Close();
                        using (var truncate = new FileStream(DataFile, FileMode.Open, FileAccess.Write))
                        {
                            truncate.SetLength(start);
                        }

                        break;
                    }

                    _indexDirty = true;
                }
            }
        }

        private static void WriteReadme(string folder)
        {
            try
            {
                string file = Path.Combine(folder, "README.txt");
                if (!File.Exists(file))
                {
                    File.WriteAllText(file,
                        "UoFiddler building library\r\n" +
                        "==========================\r\n\r\n" +
                        "*.uml  = buildings (multis) of the library, managed by UoFiddler (Region Viewer > Build > Multis / library).\r\n" +
                        "*.umx  = index cache, it is rebuilt automatically if it is missing.\r\n" +
                        "*.bak  = old version after \"Compact\".\r\n\r\n" +
                        "Do not edit these files by hand. Use \"Move library...\" in UoFiddler to move the library.\r\n");
                }
            }
            catch (Exception)
            {
                // only information
            }
        }

        private static MultiLibraryEntry ReadTemplateHeader(BinaryReader br, int id, long offset)
        {
            return new MultiLibraryEntry
            {
                Id = id,
                Offset = offset,
                Name = br.ReadString(),
                Author = br.ReadString(),
                Category = br.ReadString(),
                Created = new DateTime(br.ReadInt64()),
                Width = br.ReadInt32(),
                Height = br.ReadInt32(),
                MinZ = br.ReadInt32(),
                MaxZ = br.ReadInt32(),
                Count = br.ReadInt32()
            };
        }

        /// <summary>Writes the index cache (called on dispose; can be called any time)</summary>
        public void Flush()
        {
            lock (_lock)
            {
                SyncUnlocked();
                if (!_indexDirty && File.Exists(IndexFile))
                {
                    return;
                }

                string temp = IndexFile + ".tmp";
                using (var bw = new BinaryWriter(new FileStream(temp, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    bw.Write(Encoding.ASCII.GetBytes(IndexMagic));
                    bw.Write(_knownLength);
                    bw.Write(_entries.Count);
                    foreach (MultiLibraryEntry e in _entries.Values)
                    {
                        bw.Write(e.Id);
                        bw.Write(e.Offset);
                        bw.Write(e.Count);
                        bw.Write(e.Width);
                        bw.Write(e.Height);
                        bw.Write(e.MinZ);
                        bw.Write(e.MaxZ);
                        bw.Write(e.Created.Ticks);
                        bw.Write(e.Name ?? string.Empty);
                        bw.Write(e.Author ?? string.Empty);
                        bw.Write(e.Category ?? string.Empty);
                    }
                }

                File.Move(temp, IndexFile, true);
                _indexDirty = false;
            }
        }

        public void Dispose()
        {
            try
            {
                Flush();
            }
            catch (Exception)
            {
                // the index is only a cache
            }
        }

        #endregion

        #region Read

        /// <summary>Snapshot of all entries ordered by id</summary>
        public List<MultiLibraryEntry> GetEntries()
        {
            lock (_lock)
            {
                return _entries.Values.OrderBy(e => e.Id).ToList();
            }
        }

        public MultiLibraryEntry GetEntry(int id)
        {
            lock (_lock)
            {
                return _entries.TryGetValue(id, out MultiLibraryEntry e) ? e : null;
            }
        }

        public List<string> GetCategories()
        {
            lock (_lock)
            {
                return _entries.Values.Select(e => e.Category ?? string.Empty).Where(c => c.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c).ToList();
            }
        }

        public MultiTemplate Load(int id)
        {
            MultiLibraryEntry e = GetEntry(id) ?? throw new KeyNotFoundException($"Library entry {id} not found.");

            lock (_lock)
            {
                using (var br = new BinaryReader(new FileStream(DataFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8))
                {
                    br.BaseStream.Position = e.Offset;
                    br.ReadByte();
                    br.ReadInt32();
                    MultiLibraryEntry header = ReadTemplateHeader(br, id, e.Offset);

                    var t = new MultiTemplate { Name = e.Name, Author = e.Author, Category = e.Category, Source = $"Library #{id}" };
                    for (int i = 0; i < header.Count; i++)
                    {
                        ushort item = br.ReadUInt16();
                        short x = br.ReadInt16();
                        short y = br.ReadInt16();
                        short z = br.ReadInt16();
                        ushort hue = br.ReadUInt16();
                        short flags = br.ReadInt16();
                        short level = br.ReadInt16();
                        t.Components.Add(new MultiComponent(item, x, y, z, hue, flags, level));
                    }

                    return t;
                }
            }
        }

        #endregion

        #region Write

        /// <summary>Adds a template, returns the new entry</summary>
        public MultiLibraryEntry Add(MultiTemplate t)
        {
            lock (_lock)
            {
                SyncUnlocked();
                int id = ++_maxId;
                using (var fs = new FileStream(DataFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var bw = new BinaryWriter(fs, Encoding.UTF8))
                {
                    long offset = fs.Position;
                    var e = new MultiLibraryEntry
                    {
                        Id = id,
                        Offset = offset,
                        Name = t.Name ?? string.Empty,
                        Author = t.Author ?? string.Empty,
                        Category = t.Category ?? string.Empty,
                        Created = DateTime.Now,
                        Width = t.Width,
                        Height = t.Height,
                        MinZ = t.MinZ,
                        MaxZ = t.MaxZ,
                        Count = t.Components.Count
                    };

                    WriteTemplateRecord(bw, e, t.Components);
                    bw.Flush();
                    _knownLength = fs.Length;
                    _entries[id] = e;
                    _indexDirty = true;
                    return e;
                }
            }
        }

        private static void WriteTemplateRecord(BinaryWriter bw, MultiLibraryEntry e, IEnumerable<MultiComponent> components)
        {
            bw.Write(RecordTemplate);
            bw.Write(e.Id);
            bw.Write(e.Name ?? string.Empty);
            bw.Write(e.Author ?? string.Empty);
            bw.Write(e.Category ?? string.Empty);
            bw.Write(e.Created.Ticks);
            bw.Write(e.Width);
            bw.Write(e.Height);
            bw.Write(e.MinZ);
            bw.Write(e.MaxZ);
            bw.Write(e.Count);
            foreach (MultiComponent c in components)
            {
                bw.Write(c.ItemId);
                bw.Write((short)c.X);
                bw.Write((short)c.Y);
                bw.Write((short)c.Z);
                bw.Write(c.Hue);
                bw.Write((short)c.Flags);
                bw.Write((short)c.Level);
            }
        }

        /// <summary>Changes name, author and category</summary>
        public void UpdateMeta(int id, string name, string author, string category)
        {
            lock (_lock)
            {
                SyncUnlocked();
                if (!_entries.TryGetValue(id, out MultiLibraryEntry e))
                {
                    return;
                }

                using (var bw = new BinaryWriter(new FileStream(DataFile, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8))
                {
                    bw.Write(RecordMeta);
                    bw.Write(id);
                    bw.Write(name ?? string.Empty);
                    bw.Write(author ?? string.Empty);
                    bw.Write(category ?? string.Empty);
                }

                e.Name = name ?? string.Empty;
                e.Author = author ?? string.Empty;
                e.Category = category ?? string.Empty;
                _knownLength = DataSize;
                _indexDirty = true;
            }
        }

        public void Delete(IEnumerable<int> ids)
        {
            lock (_lock)
            {
                SyncUnlocked();
                using (var bw = new BinaryWriter(new FileStream(DataFile, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8))
                {
                    foreach (int id in ids.ToList())
                    {
                        if (!_entries.Remove(id))
                        {
                            continue;
                        }

                        bw.Write(RecordDelete);
                        bw.Write(id);
                    }
                }

                _knownLength = DataSize;
                _indexDirty = true;
            }
        }

        /// <summary>
        /// Rewrites the data file with the current entries only (removes deleted and replaced records).
        /// The old file is kept as .bak.
        /// </summary>
        public void Compact()
        {
            lock (_lock)
            {
                SyncUnlocked();
                string temp = DataFile + ".tmp";
                var newOffsets = new Dictionary<int, long>();
                using (var bw = new BinaryWriter(new FileStream(temp, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                {
                    bw.Write(Encoding.ASCII.GetBytes(DataMagic));
                    foreach (MultiLibraryEntry e in _entries.Values.OrderBy(e => e.Id).ToList())
                    {
                        MultiTemplate t = LoadUnlocked(e);
                        newOffsets[e.Id] = bw.BaseStream.Position;
                        WriteTemplateRecord(bw, e, t.Components);
                    }
                }

                File.Copy(DataFile, DataFile + ".bak", true);
                File.Move(temp, DataFile, true);
                foreach (KeyValuePair<int, long> pair in newOffsets)
                {
                    _entries[pair.Key].Offset = pair.Value;
                }

                _knownLength = DataSize;
                _indexDirty = true;
            }

            Flush();
        }

        private MultiTemplate LoadUnlocked(MultiLibraryEntry e)
        {
            using (var br = new BinaryReader(new FileStream(DataFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8))
            {
                br.BaseStream.Position = e.Offset;
                br.ReadByte();
                br.ReadInt32();
                MultiLibraryEntry header = ReadTemplateHeader(br, e.Id, e.Offset);
                var t = new MultiTemplate();
                for (int i = 0; i < header.Count; i++)
                {
                    t.Components.Add(new MultiComponent(br.ReadUInt16(), br.ReadInt16(), br.ReadInt16(), br.ReadInt16(),
                        br.ReadUInt16(), br.ReadInt16(), br.ReadInt16()));
                }

                return t;
            }
        }

        #endregion
    }
}

namespace UoFiddler.Controls.MapViewer.Build
{
    /// <summary>
    /// Where the building library is stored (remembered in AppData\MultiLibrary\LibraryPath.txt)
    /// </summary>
    public static class MultiLibraryLocation
    {
        private static string SettingFile => Path.Combine(Classes.Options.AppDataPath ?? AppContext.BaseDirectory, "MultiLibrary", "LibraryPath.txt");

        /// <summary>
        /// The library file: the chosen one, an existing library of an older version (AppData) or the default
        /// in the output directory.
        /// </summary>
        public static string CurrentFile
        {
            get
            {
                try
                {
                    if (File.Exists(SettingFile))
                    {
                        string path = File.ReadAllText(SettingFile).Trim();
                        if (path.Length > 0)
                        {
                            return path;
                        }
                    }
                }
                catch (Exception)
                {
                    // default
                }

                return File.Exists(MultiLibrary.OldDefaultFile) ? MultiLibrary.OldDefaultFile : MultiLibrary.DefaultFile;
            }
            set
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingFile));
                File.WriteAllText(SettingFile, value ?? string.Empty);
            }
        }

        /// <summary>
        /// Moves the library files (.uml, .umx, .bak) into another folder and remembers the new place.
        /// The library must be closed (disposed) before. Returns the new data file.
        /// </summary>
        public static string Move(string currentFile, string targetFolder)
        {
            string name = Path.GetFileName(currentFile);
            string target = Path.Combine(targetFolder, name);
            if (string.Equals(Path.GetFullPath(currentFile), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
            {
                return currentFile;
            }

            Directory.CreateDirectory(targetFolder);
            if (File.Exists(target))
            {
                throw new IOException($"{target} already exists.");
            }

            string[] files =
            {
                currentFile,
                Path.ChangeExtension(currentFile, ".umx"),
                currentFile + ".bak"
            };

            // copy first, delete afterwards (nothing is lost if copying fails)
            foreach (string file in files.Where(File.Exists))
            {
                File.Copy(file, Path.Combine(targetFolder, Path.GetFileName(file)), false);
            }

            foreach (string file in files.Where(File.Exists))
            {
                File.Delete(file);
            }

            CurrentFile = target;
            return target;
        }
    }
}
