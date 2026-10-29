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
using System.Linq;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Animations
{
    public sealed class EcAnimFrame
    {
        /// <summary>Item (tileart) id shown in this frame.</summary>
        public uint TileId { get; set; }

        /// <summary>Byte after the id (0 in all original entries).</summary>
        public byte Flag { get; set; }

        public override string ToString() => $"0x{TileId:X4}";
    }

    /// <summary>
    /// One entry of AnimationDefinition.uop ("build/animationdefinition/{id:D8}.bin"):
    /// the animation of an item (fire, water, doors, ...) – the Enhanced Client version of animdata.mul.
    /// Layout: uint id, 12 bytes, uint (1), uint (0), int (-1), float frame time (seconds), uint count, count x (uint tile id, byte).
    /// </summary>
    public sealed class EcAnimDefinition
    {
        public uint Id { get; set; }
        public byte[] Unknown1 { get; set; } = new byte[12];
        public uint Unknown2 { get; set; } = 1;
        public uint Unknown3 { get; set; }
        public int Unknown4 { get; set; } = -1;

        /// <summary>Time per frame in seconds.</summary>
        public float FrameTime { get; set; } = 0.25f;

        public List<EcAnimFrame> Frames { get; } = new List<EcAnimFrame>();

        /// <summary>Bytes after the frame list (none in the original files, kept if present).</summary>
        public byte[] Tail { get; set; } = Array.Empty<byte>();

        public static EcAnimDefinition Read(byte[] data)
        {
            using (var r = new BinaryReader(new MemoryStream(data)))
            {
                var a = new EcAnimDefinition
                {
                    Id = r.ReadUInt32(),
                    Unknown1 = r.ReadBytes(12),
                    Unknown2 = r.ReadUInt32(),
                    Unknown3 = r.ReadUInt32(),
                    Unknown4 = r.ReadInt32(),
                    FrameTime = r.ReadSingle()
                };
                uint count = r.ReadUInt32();
                if (count > 10000 || 36 + (count * 5) > data.Length)
                {
                    throw new InvalidDataException("Invalid frame count.");
                }
                for (int i = 0; i < count; i++)
                {
                    a.Frames.Add(new EcAnimFrame { TileId = r.ReadUInt32(), Flag = r.ReadByte() });
                }
                a.Tail = r.ReadBytes((int)(data.Length - r.BaseStream.Position));
                return a;
            }
        }

        public byte[] Write()
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(Id);
                w.Write(Unknown1.Length == 12 ? Unknown1 : new byte[12]);
                w.Write(Unknown2);
                w.Write(Unknown3);
                w.Write(Unknown4);
                w.Write(FrameTime);
                w.Write((uint)Frames.Count);
                foreach (EcAnimFrame f in Frames)
                {
                    w.Write(f.TileId);
                    w.Write(f.Flag);
                }
                w.Write(Tail);
                w.Flush();
                return ms.ToArray();
            }
        }

        public EcAnimDefinition Clone() => Read(Write());
    }

    public sealed class EcAnimDefinitionRecord
    {
        public EcUopEntry UopEntry { get; internal set; }
        public EcAnimDefinition Definition { get; internal set; }

        /// <summary>Id of the packed file name (falls back to the id inside the data).</summary>
        public uint Id { get; internal set; }

        public bool IsModified { get; internal set; }
    }

    /// <summary>
    /// AnimationDefinition.uop – item animations of the Enhanced Client.
    /// </summary>
    public sealed class EcAnimDefinitionFile
    {
        public const string NamePattern = "build/animationdefinition/{0:D8}.bin";

        private readonly List<EcAnimDefinitionRecord> _records = new List<EcAnimDefinitionRecord>();
        private readonly List<EcAnimDefinitionRecord> _removed = new List<EcAnimDefinitionRecord>();

        public EcUopPackage Package { get; private set; }
        public IReadOnlyList<EcAnimDefinitionRecord> Records => _records;
        public int FailedEntries { get; private set; }
        public bool IsModified => _removed.Count > 0 || _records.Any(r => r.IsModified);

        public static EcAnimDefinitionFile Load(EcUopPackage package)
        {
            var file = new EcAnimDefinitionFile { Package = package };
            var parsed = new List<(EcUopEntry, EcAnimDefinition)>();
            foreach (EcUopEntry entry in package.Entries)
            {
                try
                {
                    parsed.Add((entry, EcAnimDefinition.Read(package.ReadData(entry))));
                }
                catch (Exception)
                {
                    file.FailedEntries++;
                }
            }

            package.ResolveNames(NamePattern, parsed.Where(p => p.Item1.Name == null).Select(p => (int)Math.Min(p.Item2.Id, int.MaxValue)));

            foreach (var (entry, def) in parsed)
            {
                uint id = TileArtFile.TryGetIdFromName(entry.Name, out uint nameId) ? nameId : def.Id;
                file._records.Add(new EcAnimDefinitionRecord { UopEntry = entry, Definition = def, Id = id });
            }
            file._records.Sort((a, b) => a.Id.CompareTo(b.Id));
            return file;
        }

        public EcAnimDefinitionRecord Get(uint id) => _records.FirstOrDefault(r => r.Id == id);

        public void MarkModified(EcAnimDefinitionRecord record) => record.IsModified = true;

        /// <summary>
        /// Adds a new animation (copy of <paramref name="template"/> or an empty one).
        /// </summary>
        public EcAnimDefinitionRecord Add(uint id, EcAnimDefinition template = null)
        {
            if (Get(id) != null)
            {
                throw new InvalidOperationException($"Animation {id} already exists.");
            }

            EcAnimDefinition def = template?.Clone() ?? new EcAnimDefinition();
            def.Id = id;
            if (template == null)
            {
                def.Frames.Add(new EcAnimFrame { TileId = id });
            }

            var record = new EcAnimDefinitionRecord { Definition = def, Id = id, IsModified = true };
            _records.Add(record);
            _records.Sort((a, b) => a.Id.CompareTo(b.Id));
            return record;
        }

        public void Remove(EcAnimDefinitionRecord record)
        {
            if (_records.Remove(record) && record.UopEntry != null)
            {
                _removed.Add(record);
            }
        }

        /// <summary>
        /// Writes the changes into the package (the package still needs to be saved).
        /// </summary>
        public void Apply()
        {
            foreach (EcAnimDefinitionRecord record in _removed)
            {
                Package.Remove(record.UopEntry);
            }
            _removed.Clear();

            foreach (EcAnimDefinitionRecord record in _records.Where(r => r.IsModified))
            {
                if (record.UopEntry != null)
                {
                    Package.SetData(record.UopEntry, record.Definition.Write());
                }
                else
                {
                    record.UopEntry = Package.AddOrReplace(string.Format(NamePattern, record.Id), record.Definition.Write(), EcUopCompression.Zlib);
                }
                record.IsModified = false;
            }
        }
    }
}
