using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ultima
{
    /// <summary>
    /// "UOFiddler Extended" side-car format for more than 1,000,000 static items.
    ///
    /// DESIGN
    /// ------
    /// The classic files (art.mul / artidx.mul / tiledata.mul or artLegacyMUL.uop) are NOT changed in layout.
    /// They keep holding the legacy item range 0x0000..0xFFFF exactly as the Ultima Online client expects
    /// (same entry count, same offsets, same tiledata group structure) - so the current client keeps working.
    ///
    /// Every item id from <see cref="BaseId"/> (0x10000) upwards lives in three additional files next to them:
    ///
    ///   artextidx.mul    header (32 bytes) + one 16-byte entry per slot:
    ///                        int64 offset (-1 = empty), int32 length, int32 extra
    ///   artext.mul       raw static art, byte-identical to the art.mul static encoding
    ///                        (int32 header, int16 width, int16 height, uint16 row lookups, RLE runs)
    ///   tiledataext.mul  header (32 bytes) + one 41-byte record per slot, identical to the High Seas
    ///                        item record (uint64 flags, weight, quality, misc, unk2, quantity, anim, unk3,
    ///                        hue, stacking offset, value, height, char[20] name) - without the 32-entry
    ///                        group headers of tiledata.mul.
    ///
    /// Why side-car files instead of a bigger art.mul?
    ///   * artidx.mul offsets are signed 32 bit: one art.mul with 1M+ images would cross the 2 GB limit.
    ///     artextidx.mul uses 64-bit offsets.
    ///   * Tools and clients detect the art/tiledata version from the file SIZE (0x4000 / 0xC000 / 0x13FDC
    ///     entries); a bigger artidx.mul or tiledata.mul would be misdetected or crash older readers.
    ///   * The client protocol uses 16-bit graphic ids - ids above 0xFFFF need a new client anyway. A future
    ///     client only has to read the two extra files; everything below 0x10000 stays where it always was.
    ///
    /// Header (both idx and tiledata, little endian, 32 bytes):
    ///   0  char[4] magic      "UOAX" (art index) / "UOTX" (tiledata)
    ///   4  int32   version    1
    ///   8  int32   baseId     first item id stored (0x10000)
    ///   12 int32   count      number of slots
    ///   16 int32   recordSize 16 (art index) / 41 (tiledata)
    ///   20 int32[3] reserved  0
    /// </summary>
    public static class ExtendedFormat
    {
        public const int BaseId = 0x10000;
        public const int Version = 1;
        public const int HeaderSize = 32;
        public const int ArtIndexRecordSize = 16;
        public const int TileDataRecordSize = 41;

        /// <summary>Hard upper bound of slots (16,777,216) - protects against corrupt headers.</summary>
        public const int MaxSlots = 0x1000000;

        /// <summary>Default number of slots offered when creating the extension (1,048,576 -> ids up to 0x10FFFF).</summary>
        public const int DefaultSlots = 0x100000;

        public const string ArtIndexFileName = "artextidx.mul";
        public const string ArtDataFileName = "artext.mul";
        public const string TileDataFileName = "tiledataext.mul";

        private static readonly byte[] _artMagic = Encoding.ASCII.GetBytes("UOAX");
        private static readonly byte[] _tileMagic = Encoding.ASCII.GetBytes("UOTX");

        public struct ArtIndexEntry
        {
            public long Offset;
            public int Length;
            public int Extra;

            public readonly bool IsValid => Offset >= 0 && Length > 0;

            public static ArtIndexEntry Empty => new ArtIndexEntry { Offset = -1, Length = 0, Extra = 0 };
        }

        /// <summary>One tiledata record in the High Seas layout (41 bytes).</summary>
        public struct TileRecord
        {
            public ulong Flags;
            public byte Weight;
            public byte Quality;
            public short MiscData;
            public byte Unk2;
            public byte Quantity;
            public short Animation;
            public byte Unk3;
            public byte Hue;
            public byte StackingOffset;
            public byte Value;
            public byte Height;
            public string Name;
        }

        #region Header
        private static void WriteHeader(BinaryWriter bw, byte[] magic, int baseId, int count, int recordSize)
        {
            bw.Write(magic);
            bw.Write(Version);
            bw.Write(baseId);
            bw.Write(count);
            bw.Write(recordSize);
            bw.Write(0);
            bw.Write(0);
            bw.Write(0);
        }

        private static void ReadHeader(BinaryReader br, byte[] magic, int recordSize, out int baseId, out int count)
        {
            byte[] m = br.ReadBytes(4);
            if (m.Length != 4 || m[0] != magic[0] || m[1] != magic[1] || m[2] != magic[2] || m[3] != magic[3])
            {
                throw new InvalidDataException("Not a UOFiddler extended file (bad magic).");
            }

            int version = br.ReadInt32();
            if (version < 1 || version > Version)
            {
                throw new InvalidDataException($"Unsupported extended file version {version}.");
            }

            baseId = br.ReadInt32();
            count = br.ReadInt32();
            int size = br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();

            if (size != recordSize)
            {
                throw new InvalidDataException($"Unexpected record size {size} (expected {recordSize}).");
            }

            if (baseId != BaseId)
            {
                throw new InvalidDataException($"Unsupported base id 0x{baseId:X} (expected 0x{BaseId:X}).");
            }

            if (count < 0 || count > MaxSlots)
            {
                throw new InvalidDataException($"Invalid slot count {count}.");
            }
        }
        #endregion

        #region Art index
        public static ArtIndexEntry[] ReadArtIndex(Stream stream)
        {
            using var br = new BinaryReader(stream, Encoding.ASCII, true);
            ReadHeader(br, _artMagic, ArtIndexRecordSize, out _, out int count);

            long available = (stream.Length - HeaderSize) / ArtIndexRecordSize;
            var entries = new ArtIndexEntry[count];
            for (int i = 0; i < count; ++i)
            {
                if (i >= available)
                {
                    entries[i] = ArtIndexEntry.Empty;
                    continue;
                }

                entries[i].Offset = br.ReadInt64();
                entries[i].Length = br.ReadInt32();
                entries[i].Extra = br.ReadInt32();
            }

            return entries;
        }

        public static ArtIndexEntry[] ReadArtIndex(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return ReadArtIndex(fs);
        }

        public static void WriteArtIndex(Stream stream, ArtIndexEntry[] entries)
        {
            using var bw = new BinaryWriter(stream, Encoding.ASCII, true);
            WriteHeader(bw, _artMagic, BaseId, entries.Length, ArtIndexRecordSize);
            foreach (ArtIndexEntry e in entries)
            {
                bw.Write(e.IsValid ? e.Offset : -1L);
                bw.Write(e.IsValid ? e.Length : 0);
                bw.Write(e.Extra);
            }
        }
        #endregion

        #region TileData
        public static TileRecord[] ReadTileData(Stream stream)
        {
            using var br = new BinaryReader(stream, Encoding.ASCII, true);
            ReadHeader(br, _tileMagic, TileDataRecordSize, out _, out int count);

            long available = (stream.Length - HeaderSize) / TileDataRecordSize;
            var records = new TileRecord[count];
            for (int i = 0; i < count; ++i)
            {
                if (i >= available)
                {
                    records[i].Name = string.Empty;
                    continue;
                }

                records[i] = ReadTileRecord(br);
            }

            return records;
        }

        public static TileRecord[] ReadTileData(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return ReadTileData(fs);
        }

        public static void WriteTileDataHeader(BinaryWriter bw, int count)
        {
            WriteHeader(bw, _tileMagic, BaseId, count, TileDataRecordSize);
        }

        public static TileRecord ReadTileRecord(BinaryReader br)
        {
            var r = new TileRecord
            {
                Flags = br.ReadUInt64(),
                Weight = br.ReadByte(),
                Quality = br.ReadByte(),
                MiscData = br.ReadInt16(),
                Unk2 = br.ReadByte(),
                Quantity = br.ReadByte(),
                Animation = br.ReadInt16(),
                Unk3 = br.ReadByte(),
                Hue = br.ReadByte(),
                StackingOffset = br.ReadByte(),
                Value = br.ReadByte(),
                Height = br.ReadByte()
            };

            byte[] name = br.ReadBytes(20);
            int len = Array.IndexOf(name, (byte)0);
            if (len < 0)
            {
                len = name.Length;
            }

            r.Name = Encoding.ASCII.GetString(name, 0, len);
            return r;
        }

        public static void WriteTileRecord(BinaryWriter bw, TileRecord r)
        {
            bw.Write(r.Flags);
            bw.Write(r.Weight);
            bw.Write(r.Quality);
            bw.Write(r.MiscData);
            bw.Write(r.Unk2);
            bw.Write(r.Quantity);
            bw.Write(r.Animation);
            bw.Write(r.Unk3);
            bw.Write(r.Hue);
            bw.Write(r.StackingOffset);
            bw.Write(r.Value);
            bw.Write(r.Height);

            var b = new byte[20];
            if (!string.IsNullOrEmpty(r.Name))
            {
                byte[] bb = Encoding.ASCII.GetBytes(r.Name);
                Array.Copy(bb, b, Math.Min(20, bb.Length));
            }

            bw.Write(b);
        }
        #endregion

        #region Inspection (check what is really on disk)
        /// <summary>Result of <see cref="Inspect"/>: what the extended files in one folder really contain.</summary>
        public sealed class FolderReport
        {
            public string Folder;
            public bool IndexExists, DataExists, TileDataExists;
            public long IndexSize, DataSize, TileDataSize;
            public DateTime IndexTime, DataTime, TileDataTime;
            public int Slots = -1;            // -1 = index not readable
            public int TileSlots = -1;
            public readonly List<int> UsedIds = new List<int>();       // valid image entries
            public readonly List<int> BrokenIds = new List<int>();     // entry points outside artext.mul or bad image header
            public readonly List<int> NamedTileIds = new List<int>();  // tiledata records with a name or flags
            public string Error;

            public bool HasFiles => IndexExists || DataExists || TileDataExists;
        }

        /// <summary>
        /// Reads artextidx.mul / artext.mul / tiledataext.mul in <paramref name="folder"/> directly from disk
        /// (independent of what is loaded) and checks every used entry: inside artext.mul and a sane image header.
        /// </summary>
        public static FolderReport Inspect(string folder)
        {
            var r = new FolderReport { Folder = folder };
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                r.Error = "folder does not exist";
                return r;
            }

            string idx = Path.Combine(folder, ArtIndexFileName);
            string mul = Path.Combine(folder, ArtDataFileName);
            string tile = Path.Combine(folder, TileDataFileName);

            FileInfo fi;
            if ((fi = new FileInfo(idx)).Exists) { r.IndexExists = true; r.IndexSize = fi.Length; r.IndexTime = fi.LastWriteTime; }
            if ((fi = new FileInfo(mul)).Exists) { r.DataExists = true; r.DataSize = fi.Length; r.DataTime = fi.LastWriteTime; }
            if ((fi = new FileInfo(tile)).Exists) { r.TileDataExists = true; r.TileDataSize = fi.Length; r.TileDataTime = fi.LastWriteTime; }

            try
            {
                if (r.IndexExists)
                {
                    ArtIndexEntry[] entries = ReadArtIndex(idx);
                    r.Slots = entries.Length;

                    using FileStream data = r.DataExists ? new FileStream(mul, FileMode.Open, FileAccess.Read, FileShare.ReadWrite) : null;
                    var header = new byte[8];
                    for (int k = 0; k < entries.Length; ++k)
                    {
                        ArtIndexEntry e = entries[k];
                        if (!e.IsValid)
                        {
                            continue;
                        }

                        int id = BaseId + k;
                        if (data == null || e.Offset + e.Length > data.Length || e.Length < 8)
                        {
                            r.BrokenIds.Add(id);
                            continue;
                        }

                        data.Seek(e.Offset, SeekOrigin.Begin);
                        data.ReadExactly(header, 0, 8);
                        int width = BitConverter.ToUInt16(header, 4);
                        int height = BitConverter.ToUInt16(header, 6);
                        if (width < 1 || height < 1 || width > 4096 || height > 4096 || e.Length < 8 + (height * 2))
                        {
                            r.BrokenIds.Add(id);
                            continue;
                        }

                        r.UsedIds.Add(id);
                    }
                }

                if (r.TileDataExists)
                {
                    TileRecord[] records = ReadTileData(tile);
                    r.TileSlots = records.Length;
                    for (int k = 0; k < records.Length; ++k)
                    {
                        if (!string.IsNullOrEmpty(records[k].Name) || records[k].Flags != 0)
                        {
                            r.NamedTileIds.Add(BaseId + k);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                r.Error = ex.Message;
            }

            return r;
        }

        /// <summary>Short list like "0x10000, 0x10001 ... (+12 more)".</summary>
        public static string FormatIds(IReadOnlyList<int> ids, int max = 40)
        {
            if (ids == null || ids.Count == 0)
            {
                return "-";
            }

            var sb = new StringBuilder();
            for (int i = 0; i < ids.Count && i < max; ++i)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append("0x").Append(ids[i].ToString("X"));
            }

            if (ids.Count > max)
            {
                sb.Append($" ... (+{ids.Count - max} more)");
            }

            return sb.ToString();
        }
        #endregion

        /// <summary>Highest item id reachable with <paramref name="slots"/> extended slots.</summary>
        public static int GetMaxId(int slots) => slots <= 0 ? BaseId - 1 : BaseId + slots - 1;

        public static bool IsExtendedId(int id) => id >= BaseId;
    }
}
