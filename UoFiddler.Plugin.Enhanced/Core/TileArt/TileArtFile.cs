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
using System.Globalization;
using System.IO;
using System.Linq;
using UoFiddler.Plugin.Enhanced.Core.Uop;

namespace UoFiddler.Plugin.Enhanced.Core.TileArt
{
    /// <summary>
    /// One packed file of tileart.uop together with its parsed content.
    /// </summary>
    public sealed class TileArtRecord
    {
        public EcUopEntry UopEntry { get; internal set; }
        public TileArtEntry Entry { get; internal set; }

        /// <summary>
        /// Id taken from the packed file name ("build/tileart/{id}.bin"). This is the id the client uses.
        /// Falls back to <see cref="TileArtEntry.TileId"/> when the name is unknown.
        /// </summary>
        public uint Id { get; internal set; }

        /// <summary>
        /// True when the packed file name was found (and <see cref="Id"/> is reliable).
        /// </summary>
        public bool NameKnown { get; internal set; }

        /// <summary>
        /// True when the id inside the data differs from the id of the file name.
        /// </summary>
        public bool IdMismatch => NameKnown && Entry.TileId != Id;

        public bool IsModified { get; internal set; }

        public override string ToString() => $"{Id}";
    }

    /// <summary>
    /// tileart.uop – the tile data of the Enhanced Client.
    /// </summary>
    public sealed class TileArtFile
    {
        public const string NamePattern = "build/tileart/{0:D8}.bin";

        private readonly List<TileArtRecord> _records = new List<TileArtRecord>();
        private readonly Dictionary<uint, TileArtRecord> _byId = new Dictionary<uint, TileArtRecord>();
        private readonly List<TileArtRecord> _removed = new List<TileArtRecord>();

        public EcUopPackage Package { get; private set; }

        public IReadOnlyList<TileArtRecord> Records => _records;
        public int Count => _records.Count;
        public int FailedEntries { get; private set; }

        public bool IsModified => _removed.Count > 0 || _records.Any(r => r.IsModified);

        public static TileArtFile Load(EcUopPackage package, IProgress<int> progress = null)
        {
            var file = new TileArtFile { Package = package };

            // Most ids are below 0x20000; the rest is resolved with the ids found inside the data.
            package.ResolveNames(NamePattern, 0, 0x20000);

            var parsed = new List<(EcUopEntry Uop, TileArtEntry Entry)>(package.Count);
            int done = 0;
            foreach (EcUopEntry uopEntry in package.Entries)
            {
                try
                {
                    parsed.Add((uopEntry, TileArtEntry.Read(package.ReadData(uopEntry))));
                }
                catch (Exception)
                {
                    file.FailedEntries++;
                }

                if (progress != null && (++done % 2000) == 0)
                {
                    progress.Report(done * 100 / Math.Max(1, package.Count));
                }
            }

            package.ResolveNames(NamePattern, parsed.Where(p => p.Uop.Name == null).Select(p => (int)Math.Min(p.Entry.TileId, int.MaxValue)));

            foreach (var (uop, entry) in parsed)
            {
                var record = new TileArtRecord { UopEntry = uop, Entry = entry };
                if (TryGetIdFromName(uop.Name, out uint id))
                {
                    record.Id = id;
                    record.NameKnown = true;
                }
                else
                {
                    record.Id = entry.TileId;
                }
                file.Add(record);
            }

            file._records.Sort((a, b) => a.Id.CompareTo(b.Id));
            return file;
        }

        private void Add(TileArtRecord record)
        {
            _records.Add(record);
            // A known file name wins over a guessed id
            if (!_byId.TryGetValue(record.Id, out TileArtRecord existing) || (!existing.NameKnown && record.NameKnown))
            {
                _byId[record.Id] = record;
            }
        }

        public static bool TryGetIdFromName(string name, out uint id)
        {
            id = 0;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            string file = Path.GetFileNameWithoutExtension(name);
            return uint.TryParse(file, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }

        public TileArtRecord Get(uint id) => _byId.TryGetValue(id, out TileArtRecord r) ? r : null;

        public bool Contains(uint id) => _byId.ContainsKey(id);

        /// <summary>
        /// Call after changing the fields of an entry.
        /// </summary>
        public void MarkModified(TileArtRecord record)
        {
            record.IsModified = true;
        }

        /// <summary>
        /// Adds a copy of <paramref name="source"/> with a new id.
        /// </summary>
        public TileArtRecord Duplicate(TileArtRecord source, uint newId)
        {
            if (_byId.ContainsKey(newId))
            {
                throw new InvalidOperationException($"Id {newId} already exists.");
            }

            TileArtEntry copy = source.Entry.Clone();
            copy.TileId = newId;
            var record = new TileArtRecord { Entry = copy, Id = newId, NameKnown = true, IsModified = true };
            Add(record);
            _records.Sort((a, b) => a.Id.CompareTo(b.Id));
            return record;
        }

        public bool Remove(TileArtRecord record)
        {
            if (!_records.Remove(record))
            {
                return false;
            }

            if (_byId.TryGetValue(record.Id, out TileArtRecord current) && current == record)
            {
                _byId.Remove(record.Id);
                TileArtRecord other = _records.FirstOrDefault(r => r.Id == record.Id);
                if (other != null)
                {
                    _byId[record.Id] = other;
                }
            }

            if (record.UopEntry != null)
            {
                _removed.Add(record);
            }
            return true;
        }

        /// <summary>
        /// Writes the changed entries into the package (the package still needs to be saved).
        /// </summary>
        public void Apply()
        {
            foreach (TileArtRecord record in _removed)
            {
                Package.Remove(record.UopEntry);
            }
            _removed.Clear();

            foreach (TileArtRecord record in _records.Where(r => r.IsModified))
            {
                if (record.UopEntry != null)
                {
                    Package.SetData(record.UopEntry, record.Entry.Write());
                }
                else
                {
                    record.UopEntry = Package.AddOrReplace(string.Format(NamePattern, record.Id), record.Entry.Write(), EcUopCompression.Zlib);
                }
                record.IsModified = false;
            }
        }

        public uint NextFreeId(uint start)
        {
            uint id = start;
            while (_byId.ContainsKey(id))
            {
                id++;
            }
            return id;
        }
    }
}
