using System;
using System.Collections.Generic;
using System.IO;

namespace Ultima
{
    /// <summary>
    /// One side-car extension made of an index file (32-byte header + 16-byte entries) and a data file with the raw
    /// encoded entries (same encoding as in the classic mul). Used for land tiles (artlandextidx.mul / artlandext.mul)
    /// and textures (texextidx.mul / texmapsext.mul).
    ///
    /// Changed entries are kept as encoded bytes in memory until <see cref="Save"/>; unchanged ones are read from the
    /// data file on demand. Ids are absolute (e.g. land id 0x4000 = slot 0).
    /// </summary>
    public sealed class ExtendedBlobStore
    {
        private ExtendedFormat.ArtIndexEntry[] _index;
        private byte[][] _pending;   // new data set in memory (null = take it from the file)
        private int[] _pendingExtra;
        private bool[] _removed;
        private bool[] _dirty;
        private string _dataPath;
        private FileStream _stream;

        public ExtendedBlobStore(string indexFileName, string dataFileName, string magic, int baseId, int maxSlots)
        {
            IndexFileName = indexFileName;
            DataFileName = dataFileName;
            Magic = magic;
            BaseId = baseId;
            MaxSlots = maxSlots;
        }

        public string IndexFileName { get; }
        public string DataFileName { get; }
        public string Magic { get; }
        public int BaseId { get; }
        public int MaxSlots { get; }

        /// <summary>True if the extension is loaded or was created in this session.</summary>
        public bool Exists => _index != null;

        public int Count => _index?.Length ?? 0;

        /// <summary>Highest id of the extension (BaseId - 1 if there is none).</summary>
        public int MaxId => BaseId + Count - 1;

        /// <summary>Path of the loaded data file (null if only in memory).</summary>
        public string DataPath => _dataPath;

        public string LastSaveFolder { get; private set; }

        /// <summary>Set whenever something changes; cleared by <see cref="Save"/> and <see cref="Load"/>.</summary>
        public bool Modified { get; private set; }

        public bool Contains(int id) => _index != null && id >= BaseId && id < BaseId + _index.Length;

        /// <summary>(Re)loads the files from the path settings / Ultima Online folder. Unsaved changes are discarded.</summary>
        public void Load()
        {
            Close();
            Reset();
            try
            {
                string idx = Files.GetFilePathWithRootFallback(IndexFileName);
                string mul = Files.GetFilePathWithRootFallback(DataFileName);
                if (idx == null || mul == null)
                {
                    return;
                }

                LoadFrom(idx, mul);
            }
            catch (Exception)
            {
                // corrupt or foreign file: behave as if there were no extension (the classic files stay usable)
                Reset();
            }
        }

        /// <summary>Loads from explicit file paths (throws on errors).</summary>
        public void LoadFrom(string indexPath, string dataPath)
        {
            Close();
            ExtendedFormat.ArtIndexEntry[] entries = ExtendedFormat.ReadIndex(indexPath, Magic, BaseId);
            Allocate(entries.Length);
            _index = entries;
            _dataPath = dataPath;
            Modified = false;
        }

        private void Reset()
        {
            _index = null;
            _pending = null;
            _pendingExtra = null;
            _removed = null;
            _dirty = null;
            _dataPath = null;
            Modified = false;
        }

        private void Allocate(int slots)
        {
            _pending = new byte[slots][];
            _pendingExtra = new int[slots];
            _removed = new bool[slots];
            _dirty = new bool[slots];
        }

        /// <summary>Releases the open data file (e.g. before it is replaced).</summary>
        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }

        /// <summary>Creates the extension in memory or resizes it. Shrinking drops everything above the new size.</summary>
        public void CreateOrResize(int slots)
        {
            if (slots < 1 || slots > MaxSlots)
            {
                throw new ArgumentOutOfRangeException(nameof(slots), slots, $"Must be between 1 and {MaxSlots}.");
            }

            int old = Count;
            if (_index == null)
            {
                _index = new ExtendedFormat.ArtIndexEntry[slots];
                Allocate(slots);
            }
            else
            {
                Array.Resize(ref _index, slots);
                Array.Resize(ref _pending, slots);
                Array.Resize(ref _pendingExtra, slots);
                Array.Resize(ref _removed, slots);
                Array.Resize(ref _dirty, slots);
            }

            for (int i = old; i < slots; ++i)
            {
                _index[i] = ExtendedFormat.ArtIndexEntry.Empty;
            }

            Modified = true;
        }

        /// <summary>Removes the whole extension from memory (the files on disk are not touched).</summary>
        public void Drop()
        {
            Close();
            Reset();
            Modified = true;
        }

        public bool IsValid(int id)
        {
            if (!Contains(id))
            {
                return false;
            }

            int k = id - BaseId;
            if (_removed[k])
            {
                return false;
            }

            return _pending[k] != null || _index[k].IsValid;
        }

        /// <summary>Encoded data of <paramref name="id"/> (null if empty). <paramref name="extra"/> = the index "extra" value.</summary>
        public byte[] Read(int id, out int extra)
        {
            extra = 0;
            if (!IsValid(id))
            {
                return null;
            }

            int k = id - BaseId;
            if (_pending[k] != null)
            {
                extra = _pendingExtra[k];
                return _pending[k];
            }

            ExtendedFormat.ArtIndexEntry e = _index[k];
            extra = e.Extra;
            if (_dataPath == null)
            {
                return null;
            }

            if (_stream == null || !_stream.CanRead)
            {
                if (!File.Exists(_dataPath))
                {
                    return null;
                }

                _stream = new FileStream(_dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }

            if (e.Offset + e.Length > _stream.Length)
            {
                return null;
            }

            var buffer = new byte[e.Length];
            _stream.Seek(e.Offset, SeekOrigin.Begin);
            _stream.ReadExactly(buffer, 0, e.Length);
            return buffer;
        }

        public void Set(int id, byte[] data, int extra)
        {
            if (!Contains(id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"0x{id:X} is outside the extension 0x{BaseId:X}-0x{MaxId:X}.");
            }

            int k = id - BaseId;
            if (data == null || data.Length == 0)
            {
                Remove(id);
                return;
            }

            _pending[k] = data;
            _pendingExtra[k] = extra;
            _removed[k] = false;
            _dirty[k] = true;
            Modified = true;
        }

        public void Remove(int id)
        {
            if (!Contains(id))
            {
                return;
            }

            int k = id - BaseId;
            _pending[k] = null;
            _removed[k] = true;
            _dirty[k] = true;
            Modified = true;
        }

        public List<int> GetUsedIds()
        {
            var ids = new List<int>();
            for (int k = 0; k < Count; ++k)
            {
                if (IsValid(BaseId + k))
                {
                    ids.Add(BaseId + k);
                }
            }

            return ids;
        }

        public List<int> GetUnsavedIds()
        {
            var ids = new List<int>();
            for (int k = 0; k < Count; ++k)
            {
                if (_dirty[k])
                {
                    ids.Add(BaseId + k);
                }
            }

            return ids;
        }

        public int CountUsed()
        {
            int n = 0;
            for (int k = 0; k < Count; ++k)
            {
                if (IsValid(BaseId + k))
                {
                    ++n;
                }
            }

            return n;
        }

        /// <summary>
        /// Writes index + data into <paramref name="folder"/> (temp files, then replaced). Identical entries are stored
        /// once. Afterwards the store reads from the written files.
        /// </summary>
        public void Save(string folder)
        {
            if (!Exists)
            {
                return;
            }

            Directory.CreateDirectory(folder);
            string idxFinal = Path.Combine(folder, IndexFileName);
            string mulFinal = Path.Combine(folder, DataFileName);
            string idxTmp = idxFinal + ".tmp";
            string mulTmp = mulFinal + ".tmp";

            int count = Count;
            var entries = new ExtendedFormat.ArtIndexEntry[count];
            var written = new Dictionary<string, ExtendedFormat.ArtIndexEntry>();

            using (var sha = System.Security.Cryptography.SHA256.Create())
            using (var fsMul = new FileStream(mulTmp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                for (int k = 0; k < count; ++k)
                {
                    entries[k] = ExtendedFormat.ArtIndexEntry.Empty;
                    byte[] data = Read(BaseId + k, out int extra);
                    if (data == null || data.Length == 0)
                    {
                        continue;
                    }

                    string key = Convert.ToHexString(sha.ComputeHash(data)) + ":" + extra;
                    if (written.TryGetValue(key, out ExtendedFormat.ArtIndexEntry existing))
                    {
                        entries[k] = existing;
                        continue;
                    }

                    var entry = new ExtendedFormat.ArtIndexEntry { Offset = fsMul.Position, Length = data.Length, Extra = extra };
                    fsMul.Write(data, 0, data.Length);
                    entries[k] = entry;
                    written[key] = entry;
                }
            }

            using (var fsIdx = new FileStream(idxTmp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                ExtendedFormat.WriteIndex(fsIdx, Magic, BaseId, entries);
            }

            Close(); // the source may be the file that gets replaced
            File.Move(mulTmp, mulFinal, true);
            File.Move(idxTmp, idxFinal, true);

            // from now on everything comes from the new files
            _index = entries;
            _dataPath = mulFinal;
            Allocate(count);
            Modified = false;
            LastSaveFolder = Path.GetFullPath(folder);
        }
    }
}
