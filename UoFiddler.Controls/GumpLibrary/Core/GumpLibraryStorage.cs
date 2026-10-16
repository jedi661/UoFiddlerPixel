// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using UoFiddler.Controls.ArtLibrary.Core;

namespace UoFiddler.Controls.GumpLibrary.Core
{
    /// <summary>The two storages of a gump library.</summary>
    public enum GumpStorageKind
    {
        Gumps = 0,
        Previews = 1
    }

    public static class GumpStorageKindExtensions
    {
        public static string IdxExtension(this GumpStorageKind kind) => kind == GumpStorageKind.Gumps ? ".gumplib.idx" : ".gumpprev.idx";

        public static string MulExtension(this GumpStorageKind kind) => kind == GumpStorageKind.Gumps ? ".gumplib.mul" : ".gumpprev.mul";
    }

    /// <summary>
    /// Graphic storage of a gump library. Same file layout as the art library (ArtLibraryHeader +
    /// 16 byte ArtLibraryEntry per slot), the data is <see cref="GumpLibraryCodec"/>.
    /// <code>
    /// Name.gumplib.idx / Name.gumplib.mul   gumps           (Kind = Gump)
    /// Name.gumpprev.idx / Name.gumpprev.mul own previews    (Kind = Gump)
    /// </code>
    /// <para>With AutoSave on, every change is written through immediately (data appended to the .mul,
    /// then only the 16 byte entry updated). Replaced data stays as unused space until <see cref="Compact"/>.</para>
    /// </summary>
    public sealed class GumpLibraryStorage : IDisposable
    {
        public const int DefaultCapacity = 1_000_000;
        public const int MaxCapacity = 100_000_000; // index file of ~1.6 GB

        private const int EntryChunk = 65536;  // entries per read/write block
        private const int GumpCacheLimit = 384;     // gumps can be big
        private const int PreviewCacheLimit = 1024;  // previews are usually small

        private readonly object _sync = new object();
        private readonly GumpLibraryCache _cache;

        private ArtLibraryHeader _header;
        private ArtLibraryEntry[] _entries;
        private FileStream _idxStream;
        private FileStream _mulStream;
        private int _occupiedCount;
        private bool _autoSave = true;

        // ids whose index entry changed in memory but is not written yet (only when AutoSave is off)
        private readonly HashSet<int> _pendingIds = new HashSet<int>();
        private bool _disposed;

        /// <summary>
        /// Raised after an entry was written, removed or the whole library changed (grow, compact).
        /// Raised on the thread that made the change.
        /// </summary>
        public event EventHandler<ArtLibraryChangedEventArgs> EntryChanged;

        private GumpLibraryStorage(GumpStorageKind kind, string basePath)
        {
            StorageKind = kind;
            BasePath = basePath;
            _cache = new GumpLibraryCache(kind == GumpStorageKind.Gumps ? GumpCacheLimit : PreviewCacheLimit);
        }

        /// <summary>Gumps or preview images.</summary>
        public GumpStorageKind StorageKind { get; }

        #region [ Properties ]

        /// <summary>Full path without extension, e.g. C:\Libs\MeineDeko</summary>
        public string BasePath { get; }

        public string IdxPath => BasePath + StorageKind.IdxExtension();
        public string MulPath => BasePath + StorageKind.MulExtension();

        /// <summary>File name without extension (MeineDeko).</summary>
        public string FileName => Path.GetFileName(BasePath);

        /// <summary>Display name stored in the header.</summary>
        public string Name => _header.Name;

        public int Capacity => _header.Capacity;
        public ArtLibraryKind Kind => _header.Kind;
        public DateTime Created => _header.Created;
        public int Version => _header.Version;

        /// <summary>Highest usable id (Capacity - 1).</summary>
        public int MaxId => Capacity - 1;

        public int OccupiedCount
        {
            get
            {
                lock (_sync)
                {
                    return _occupiedCount;
                }
            }
        }

        public long MulLength
        {
            get
            {
                lock (_sync)
                {
                    ThrowIfDisposed();
                    return _mulStream.Length;
                }
            }
        }

        public bool IsDisposed => _disposed;

        /// <summary>
        /// On (default): every change is written to the index file immediately.
        /// Off: changes are kept in memory until <see cref="Save"/>; closing without saving discards them.
        /// Switching it on writes all pending changes.
        /// </summary>
        public bool AutoSave
        {
            get
            {
                lock (_sync)
                {
                    return _autoSave;
                }
            }
            set
            {
                lock (_sync)
                {
                    _autoSave = value;
                    if (value && !_disposed)
                    {
                        WritePendingEntries();
                    }
                }
            }
        }

        /// <summary>True when changes are waiting for <see cref="Save"/> (AutoSave off).</summary>
        public bool HasUnsavedChanges
        {
            get
            {
                lock (_sync)
                {
                    return _pendingIds.Count > 0;
                }
            }
        }

        /// <summary>Number of ids with unsaved changes.</summary>
        public int UnsavedCount
        {
            get
            {
                lock (_sync)
                {
                    return _pendingIds.Count;
                }
            }
        }

        #endregion

        #region [ Create / Open ]

        /// <summary>
        /// Creates the files of one layer and opens it. Fails when they already exist.
        /// </summary>
        public static GumpLibraryStorage Create(GumpStorageKind kind, string basePath, string name, int capacity)
        {
            if (capacity < 1 || capacity > MaxCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, $"Capacity must be between 1 and {MaxCapacity:N0}.");
            }

            string idxPath = basePath + kind.IdxExtension();
            string mulPath = basePath + kind.MulExtension();
            if (File.Exists(idxPath) || File.Exists(mulPath))
            {
                throw new IOException($"{Path.GetFileName(idxPath)} already exists.");
            }

            var header = new ArtLibraryHeader
            {
                Capacity = capacity,
                Kind = ArtLibraryKind.Gump,
                Created = DateTime.UtcNow,
                Name = name
            };

            using (var idx = new FileStream(idxPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                header.Write(idx);
                WriteEmptyEntries(idx, ArtLibraryHeader.Size, capacity);
            }

            using (new FileStream(mulPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                // empty data file
            }

            return Open(kind, basePath);
        }

        /// <summary>
        /// Opens one layer of an existing terrain library (<paramref name="basePath"/> without extension).
        /// </summary>
        public static GumpLibraryStorage Open(GumpStorageKind kind, string basePath)
        {
            var library = new GumpLibraryStorage(kind, basePath);
            try
            {
                library.Load();
            }
            catch
            {
                library.Dispose();
                throw;
            }

            return library;
        }

        private void Load()
        {
            if (!File.Exists(IdxPath))
            {
                throw new FileNotFoundException("Library index file not found.", IdxPath);
            }

            if (!File.Exists(MulPath))
            {
                throw new FileNotFoundException("Library data file not found.", MulPath);
            }

            _idxStream = new FileStream(IdxPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            _mulStream = new FileStream(MulPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);

            _header = ArtLibraryHeader.Read(_idxStream);

            if (_header.Kind != ArtLibraryKind.Gump)
            {
                throw new InvalidDataException($"{Path.GetFileName(IdxPath)} contains {_header.Kind}, expected {ArtLibraryKind.Gump}.");
            }

            if (_header.Capacity > MaxCapacity)
            {
                throw new InvalidDataException($"Library capacity {_header.Capacity:N0} exceeds the supported maximum of {MaxCapacity:N0}.");
            }

            _entries = ReadEntries(_idxStream, _header.Capacity, _mulStream.Length, out _occupiedCount);
        }

        /// <summary>
        /// Reads all entries. Entries missing at the end of a truncated file and entries pointing outside
        /// the data file are treated as empty.
        /// </summary>
        private static ArtLibraryEntry[] ReadEntries(Stream idx, int capacity, long mulLength, out int occupied)
        {
            var entries = new ArtLibraryEntry[capacity];
            occupied = 0;

            long available = Math.Max(0, (idx.Length - ArtLibraryHeader.Size) / ArtLibraryEntry.Size);
            int readable = (int)Math.Min(capacity, available);

            var buffer = new byte[EntryChunk * ArtLibraryEntry.Size];
            idx.Seek(ArtLibraryHeader.Size, SeekOrigin.Begin);

            for (int start = 0; start < readable; start += EntryChunk)
            {
                int count = Math.Min(EntryChunk, readable - start);
                idx.ReadExactly(buffer, 0, count * ArtLibraryEntry.Size);

                for (int i = 0; i < count; ++i)
                {
                    var entry = ArtLibraryEntry.Read(buffer.AsSpan(i * ArtLibraryEntry.Size, ArtLibraryEntry.Size));
                    if (entry.IsEmpty || entry.Offset + entry.Length > mulLength)
                    {
                        entry = ArtLibraryEntry.Empty;
                    }
                    else
                    {
                        ++occupied;
                    }

                    entries[start + i] = entry;
                }
            }

            for (int i = readable; i < capacity; ++i)
            {
                entries[i] = ArtLibraryEntry.Empty;
            }

            return entries;
        }

        private static void WriteEmptyEntries(Stream idx, long position, int count)
        {
            int chunk = Math.Min(EntryChunk, Math.Max(count, 1));
            var buffer = new byte[chunk * ArtLibraryEntry.Size];
            ArtLibraryEntry empty = ArtLibraryEntry.Empty;
            for (int i = 0; i < chunk; ++i)
            {
                empty.Write(buffer.AsSpan(i * ArtLibraryEntry.Size, ArtLibraryEntry.Size));
            }

            idx.Seek(position, SeekOrigin.Begin);
            for (int written = 0; written < count; written += chunk)
            {
                int n = Math.Min(chunk, count - written);
                idx.Write(buffer, 0, n * ArtLibraryEntry.Size);
            }
        }

        #endregion

        #region [ Read ]

        public bool IsValidId(int id) => id >= 0 && id < Capacity;

        /// <summary>True when the entry of <paramref name="id"/> changed but is not saved yet (AutoSave off).</summary>
        public bool IsPending(int id)
        {
            lock (_sync)
            {
                return _pendingIds.Contains(id);
            }
        }

        /// <summary>True when the id holds a graphic.</summary>
        public bool IsOccupied(int id)
        {
            lock (_sync)
            {
                return IsValidId(id) && !_entries[id].IsEmpty;
            }
        }

        public ArtLibraryEntry GetEntry(int id)
        {
            lock (_sync)
            {
                ThrowIfInvalidId(id);
                return _entries[id];
            }
        }

        /// <summary>
        /// Returns the encoded graphic (art.mul item format) or null for an empty id.
        /// </summary>
        public byte[] GetRaw(int id)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                if (!IsValidId(id))
                {
                    return null;
                }

                return ReadData(_entries[id]);
            }
        }

        /// <summary>
        /// Returns the decoded graphic or null for an empty id. The bitmap is cached and shared:
        /// do not dispose or modify it - clone it if you need to change it.
        /// </summary>
        public Bitmap GetBitmap(int id)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                if (!IsValidId(id) || _entries[id].IsEmpty)
                {
                    return null;
                }

                if (_cache.TryGet(id, out Bitmap cached))
                {
                    return cached;
                }

                byte[] data = ReadData(_entries[id]);
                Bitmap bitmap = data == null ? null : GumpLibraryCodec.DecodeBitmap(data);
                if (bitmap == null)
                {
                    return null;
                }

                _cache.Add(id, bitmap);
                return bitmap;
            }
        }

        /// <summary>
        /// Reads only width and height of a graphic.
        /// </summary>
        public bool TryGetSize(int id, out int width, out int height)
        {
            width = height = 0;
            lock (_sync)
            {
                ThrowIfDisposed();
                if (!IsValidId(id) || _entries[id].IsEmpty)
                {
                    return false;
                }

                var header = new byte[GumpLibraryCodec.HeaderLength];
                if (_entries[id].Length < header.Length)
                {
                    return false;
                }

                _mulStream.Seek(_entries[id].Offset, SeekOrigin.Begin);
                _mulStream.ReadExactly(header, 0, header.Length);
                return GumpLibraryCodec.TryReadSize(header, out width, out height);
            }
        }

        /// <summary>
        /// All occupied ids in ascending order.
        /// </summary>
        public List<int> GetOccupiedIds()
        {
            lock (_sync)
            {
                var result = new List<int>(_occupiedCount);
                for (int i = 0; i < _entries.Length; ++i)
                {
                    if (!_entries[i].IsEmpty)
                    {
                        result.Add(i);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// First free id at or after <paramref name="startId"/>, or -1 if there is none.
        /// </summary>
        public int FindNextFree(int startId = 0)
        {
            lock (_sync)
            {
                for (int i = Math.Max(0, startId); i < _entries.Length; ++i)
                {
                    if (_entries[i].IsEmpty)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// Bytes in the data file that are no longer referenced (reclaimable with <see cref="Compact"/>).
        /// </summary>
        public long GetUnusedBytes()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                var seen = new HashSet<long>();
                long used = 0;
                foreach (ArtLibraryEntry entry in _entries)
                {
                    if (!entry.IsEmpty && seen.Add(entry.Offset))
                    {
                        used += entry.Length;
                    }
                }

                return Math.Max(0, _mulStream.Length - used);
            }
        }

        private byte[] ReadData(ArtLibraryEntry entry)
        {
            if (entry.IsEmpty)
            {
                return null;
            }

            var data = new byte[entry.Length];
            _mulStream.Seek(entry.Offset, SeekOrigin.Begin);
            _mulStream.ReadExactly(data, 0, data.Length);
            return data;
        }

        #endregion

        #region [ Write ]

        /// <summary>
        /// Stores already encoded data (GumpLibraryCodec format) on <paramref name="id"/>.
        /// </summary>
        public void SetRaw(int id, byte[] data)
        {
            if (data == null || !GumpLibraryCodec.IsValid(data))
            {
                throw new ArgumentException("Data is not a valid gump.", nameof(data));
            }

            lock (_sync)
            {
                ThrowIfDisposed();
                ThrowIfInvalidId(id);

                _mulStream.Seek(0, SeekOrigin.End);
                long offset = _mulStream.Position;
                _mulStream.Write(data, 0, data.Length);
                _mulStream.Flush();

                SetEntry(id, new ArtLibraryEntry { Offset = offset, Length = data.Length, Flags = _entries[id].Flags });
            }

            OnEntryChanged(id);
        }

        /// <summary>
        /// Encodes and stores a bitmap on <paramref name="id"/>. The bitmap is not kept; the caller still owns it.
        /// </summary>
        public void SetBitmap(int id, Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            SetRaw(id, GumpLibraryCodec.Encode(bitmap));
        }

        /// <summary>
        /// Empties <paramref name="id"/>. The data stays in the .mul until <see cref="Compact"/>.
        /// </summary>
        public void Remove(int id)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                ThrowIfInvalidId(id);
                if (_entries[id].IsEmpty)
                {
                    return;
                }

                SetEntry(id, ArtLibraryEntry.Empty);
            }

            OnEntryChanged(id);
        }

        /// <summary>
        /// Copies the graphic of <paramref name="sourceId"/> to <paramref name="targetId"/>
        /// (both ids then share the same data; no data is duplicated).
        /// </summary>
        public void Copy(int sourceId, int targetId)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                ThrowIfInvalidId(sourceId);
                ThrowIfInvalidId(targetId);
                if (sourceId == targetId)
                {
                    return;
                }

                ArtLibraryEntry source = _entries[sourceId];
                SetEntry(targetId, source.IsEmpty ? ArtLibraryEntry.Empty : source);
            }

            OnEntryChanged(targetId);
        }

        /// <summary>
        /// Moves the graphic of <paramref name="sourceId"/> to <paramref name="targetId"/>; the source becomes empty.
        /// </summary>
        public void Move(int sourceId, int targetId)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                ThrowIfInvalidId(sourceId);
                ThrowIfInvalidId(targetId);
                if (sourceId == targetId)
                {
                    return;
                }

                ArtLibraryEntry source = _entries[sourceId];
                SetEntry(targetId, source.IsEmpty ? ArtLibraryEntry.Empty : source);
                SetEntry(sourceId, ArtLibraryEntry.Empty);
            }

            OnEntryChanged(sourceId);
            OnEntryChanged(targetId);
        }

        /// <summary>
        /// Exchanges the graphics of two ids.
        /// </summary>
        public void Swap(int firstId, int secondId)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                ThrowIfInvalidId(firstId);
                ThrowIfInvalidId(secondId);
                if (firstId == secondId)
                {
                    return;
                }

                ArtLibraryEntry first = _entries[firstId];
                SetEntry(firstId, _entries[secondId]);
                SetEntry(secondId, first);
            }

            OnEntryChanged(firstId);
            OnEntryChanged(secondId);
        }

        /// <summary>
        /// Sets the reserved flags of an entry (e.g. favorite). Ignored for empty ids.
        /// </summary>
        public void SetFlags(int id, ArtLibraryEntryFlags flags)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                ThrowIfInvalidId(id);
                ArtLibraryEntry entry = _entries[id];
                if (entry.IsEmpty)
                {
                    return;
                }

                entry.Flags = (int)flags;
                SetEntry(id, entry);
            }

            OnEntryChanged(id);
        }

        /// <summary>
        /// Changes the display name stored in the header (the file name stays the same).
        /// </summary>
        public void Rename(string displayName)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                _header.Name = displayName ?? string.Empty;
                byte[] name = ArtLibraryHeader.EncodeName(_header.Name);
                _idxStream.Seek(ArtLibraryHeader.NameFieldOffset, SeekOrigin.Begin);
                _idxStream.Write(name, 0, name.Length);
                _idxStream.Flush();
            }
        }

        /// <summary>
        /// Writes one entry to memory and to the index file.
        /// </summary>
        private void SetEntry(int id, ArtLibraryEntry entry)
        {
            bool wasEmpty = _entries[id].IsEmpty;
            if (entry.IsEmpty)
            {
                entry = ArtLibraryEntry.Empty;
            }

            _entries[id] = entry;

            if (wasEmpty && !entry.IsEmpty)
            {
                ++_occupiedCount;
            }
            else if (!wasEmpty && entry.IsEmpty)
            {
                --_occupiedCount;
            }

            _cache.Remove(id);

            if (_autoSave)
            {
                WriteEntry(id);
                _idxStream.Flush();
            }
            else
            {
                _pendingIds.Add(id);
            }
        }

        private void WriteEntry(int id)
        {
            var buffer = new byte[ArtLibraryEntry.Size];
            _entries[id].Write(buffer);
            _idxStream.Seek(ArtLibraryHeader.Size + ((long)id * ArtLibraryEntry.Size), SeekOrigin.Begin);
            _idxStream.Write(buffer, 0, buffer.Length);
        }

        private void WritePendingEntries()
        {
            if (_pendingIds.Count == 0)
            {
                return;
            }

            foreach (int id in _pendingIds)
            {
                if (id < _entries.Length)
                {
                    WriteEntry(id);
                }
            }

            _pendingIds.Clear();
            _idxStream.Flush();
        }

        /// <summary>
        /// Writes all pending changes and flushes both files to disk.
        /// </summary>
        public void Save()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                WritePendingEntries();
                _idxStream.Flush(true);
                _mulStream.Flush(true);
            }
        }

        #endregion

        #region [ Grow / Compact ]

        /// <summary>
        /// Enlarges the library to <paramref name="newCapacity"/> ids. Existing graphics are not touched.
        /// </summary>
        public void Grow(int newCapacity)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                if (newCapacity <= Capacity)
                {
                    throw new ArgumentOutOfRangeException(nameof(newCapacity), newCapacity, $"New capacity must be greater than the current capacity ({Capacity:N0}).");
                }

                if (newCapacity > MaxCapacity)
                {
                    throw new ArgumentOutOfRangeException(nameof(newCapacity), newCapacity, $"Capacity must not exceed {MaxCapacity:N0}.");
                }

                int oldCapacity = Capacity;

                // first the new entries, then the header: an interrupted grow leaves a valid, smaller library
                WriteEmptyEntries(_idxStream, ArtLibraryHeader.Size + ((long)oldCapacity * ArtLibraryEntry.Size), newCapacity - oldCapacity);
                _idxStream.Flush();

                var buffer = BitConverter.GetBytes(newCapacity);
                _idxStream.Seek(ArtLibraryHeader.CapacityFieldOffset, SeekOrigin.Begin);
                _idxStream.Write(buffer, 0, buffer.Length);
                _idxStream.Flush();

                var entries = new ArtLibraryEntry[newCapacity];
                Array.Copy(_entries, entries, oldCapacity);
                for (int i = oldCapacity; i < newCapacity; ++i)
                {
                    entries[i] = ArtLibraryEntry.Empty;
                }

                _entries = entries;
                _header.Capacity = newCapacity;
            }

            OnEntryChanged(ArtLibraryChangedEventArgs.AllIds);
        }

        /// <summary>
        /// Rewrites the data file without unused space (replaced or removed graphics).
        /// Ids and graphics stay the same. Returns the number of bytes saved.
        /// </summary>
        public long Compact(IProgress<int> progress = null)
        {
            long saved;
            lock (_sync)
            {
                ThrowIfDisposed();

                string idxTemp = IdxPath + ".tmp";
                string mulTemp = MulPath + ".tmp";
                long oldLength = _mulStream.Length;

                var newEntries = new ArtLibraryEntry[_entries.Length];
                var offsetMap = new Dictionary<long, long>();

                using (var mul = new FileStream(mulTemp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    for (int i = 0; i < _entries.Length; ++i)
                    {
                        ArtLibraryEntry entry = _entries[i];
                        if (entry.IsEmpty)
                        {
                            newEntries[i] = ArtLibraryEntry.Empty;
                            continue;
                        }

                        if (!offsetMap.TryGetValue(entry.Offset, out long newOffset))
                        {
                            newOffset = mul.Position;
                            byte[] data = ReadData(entry);
                            mul.Write(data, 0, data.Length);
                            offsetMap[entry.Offset] = newOffset;
                        }

                        newEntries[i] = new ArtLibraryEntry { Offset = newOffset, Length = entry.Length, Flags = entry.Flags };

                        if (progress != null && (i & 0xFFFF) == 0)
                        {
                            progress.Report((int)((long)i * 100 / _entries.Length));
                        }
                    }
                }

                using (var idx = new FileStream(idxTemp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    _header.Write(idx);
                    var buffer = new byte[EntryChunk * ArtLibraryEntry.Size];
                    for (int start = 0; start < newEntries.Length; start += EntryChunk)
                    {
                        int count = Math.Min(EntryChunk, newEntries.Length - start);
                        for (int i = 0; i < count; ++i)
                        {
                            newEntries[start + i].Write(buffer.AsSpan(i * ArtLibraryEntry.Size, ArtLibraryEntry.Size));
                        }

                        idx.Write(buffer, 0, count * ArtLibraryEntry.Size);
                    }
                }

                _idxStream.Dispose();
                _mulStream.Dispose();

                File.Move(mulTemp, MulPath, true);
                File.Move(idxTemp, IdxPath, true);

                _idxStream = new FileStream(IdxPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                _mulStream = new FileStream(MulPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                _entries = newEntries;
                _pendingIds.Clear(); // the new index file was written from memory, so everything is saved
                _cache.Clear();

                saved = oldLength - _mulStream.Length;
                progress?.Report(100);
            }

            OnEntryChanged(ArtLibraryChangedEventArgs.AllIds);
            return saved;
        }

        #endregion

        #region [ Helpers / Dispose ]

        public void Flush()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                _idxStream.Flush(true);
                _mulStream.Flush(true);
            }
        }

        private void OnEntryChanged(int id)
        {
            EntryChanged?.Invoke(this, new ArtLibraryChangedEventArgs(id));
        }

        private void ThrowIfInvalidId(int id)
        {
            if (!IsValidId(id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, $"Id must be between 0 and {MaxId:N0} (0x{MaxId:X}).");
            }
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _idxStream?.Dispose();
                _mulStream?.Dispose();
                _cache.Clear();
            }
        }

        public override string ToString() => $"{Name} {StorageKind} ({OccupiedCount:N0} / {Capacity:N0})";

        #endregion
    }
}
