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
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Ultima;
using Ultima.Helpers;

namespace UoFiddler.Plugin.UopPacker.Classes
{
    /// <summary>Result of a conversion (shown in the log).</summary>
    public sealed class ConversionResult
    {
        public int Entries { get; set; }
        public int Skipped { get; set; }
        public long Bytes { get; set; }
        public List<string> Warnings { get; } = new List<string>();

        public override string ToString() =>
            $"{Entries} entries, {Bytes:N0} bytes" + (Skipped > 0 ? $", {Skipped} skipped" : string.Empty) +
            (Warnings.Count > 0 ? $", {Warnings.Count} warnings" : string.Empty);
    }

    /// <summary>
    /// Converts the classic client files between MUL / IDX and UOP (LegacyMUL format): art, gumps, sounds, maps, multis.
    /// Output is written to a temporary file first and replaces the target only when everything worked
    /// (the old target is copied to the backup folder before, if the backup is on).
    /// </summary>
    public static class LegacyMulFileConverter
    {
        private struct IdxEntry
        {
            public int Id;
            public int Offset;
            public int Size;
            public int Extra;
        }

        private struct TableEntry
        {
            public long Offset;
            public int HeaderLength;
            public int Size;
            public int DecompressedSize;
            public ulong Identifier;
            public uint Hash;
            public short CompressionFlag;
        }

        // Same for all UOP files
        private const long FirstTable = 0x200;
        private const int TableSize = 0x64;

        private static readonly byte[] _emptyTableEntry = new byte[8 + 4 + 4 + 4 + 8 + 4 + 2];

        // ================================================================== MUL -> UOP

        /// <summary>
        /// Packs MUL / IDX into a UOP file. For maps there is no IDX (the map is cut into 0xC4000 byte chunks).
        /// For multis <paramref name="housingBin"/> is the housing.bin that is put into MultiCollection.uop (needed by the client).
        /// </summary>
        public static ConversionResult ToUop(string inFile, string inFileIdx, string outFile, FileType type, int typeIndex,
            CompressionFlag compressionFlag = CompressionFlag.None, string housingBin = null, PackerBackup backup = null,
            IProgress<int> progress = null, CancellationToken cancel = default)
        {
            if (compressionFlag == CompressionFlag.Mythic && type != FileType.GumpartLegacyMul)
            {
                throw new ArgumentException("Mythic compression is only used for gumps (gumpartLegacyMUL.uop). Use None or Zlib.");
            }
            if (type != FileType.MapLegacyMul && (string.IsNullOrWhiteSpace(inFileIdx) || !File.Exists(inFileIdx)))
            {
                throw new FileNotFoundException("The IDX file is missing.", inFileIdx);
            }

            var result = new ConversionResult();
            string temp = outFile + ".tmp";
            try
            {
                using (var reader = new BinaryReader(new FileStream(inFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                using (BinaryReader readerIdx = type == FileType.MapLegacyMul ? null : new BinaryReader(new FileStream(inFileIdx, FileMode.Open, FileAccess.Read, FileShare.Read)))
                using (var writer = new BinaryWriter(new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    List<IdxEntry> idxEntries = ReadEntries(reader, readerIdx, type);

                    byte[] housing = null;
                    if (type == FileType.MultiCollection)
                    {
                        if (!string.IsNullOrWhiteSpace(housingBin) && File.Exists(housingBin))
                        {
                            housing = File.ReadAllBytes(housingBin);
                        }
                        else
                        {
                            result.Warnings.Add("housing.bin was not found – MultiCollection.uop is written without it (the client will probably not work with it).");
                        }
                    }

                    int multiRecord = type == FileType.MultiCollection ? MultiRecordSize(idxEntries) : 0;
                    int fileCount = idxEntries.Count + (housing != null ? 1 : 0);
                    bool gump = type == FileType.GumpartLegacyMul;

                    // File header (gumps: version 4, first table directly after the header – like before)
                    writer.Write(0x50594D); // MYP
                    writer.Write(gump ? 4 : 5); // version
                    writer.Write(0xFD23EC43); // signature
                    writer.Write(gump ? (long)0x28 : FirstTable); // first table
                    writer.Write(TableSize); // table size
                    writer.Write(fileCount); // file count
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    if (!gump)
                    {
                        for (int i = 0x28; i < FirstTable; ++i)
                        {
                            writer.Write((byte)0);
                        }
                    }

                    int tableCount = (int)Math.Ceiling((double)fileCount / TableSize);
                    var tableEntries = new TableEntry[TableSize];
                    string[] hashFormat = UopFileTypes.GetHashFormat(type, typeIndex, out int _);

                    for (int i = 0; i < tableCount; ++i)
                    {
                        cancel.ThrowIfCancellationRequested();
                        long thisTable = writer.BaseStream.Position;
                        int idxStart = i * TableSize;
                        int idxEnd = Math.Min((i + 1) * TableSize, fileCount);

                        // Table header
                        writer.Write(idxEnd - idxStart);
                        writer.Write((long)0); // next table, filled in later
                        writer.Seek(34 * TableSize, SeekOrigin.Current); // table entries, filled in later

                        int tableIdx = 0;
                        for (int j = idxStart; j < idxEnd; ++j, ++tableIdx)
                        {
                            byte[] data;
                            ulong identifier;
                            if (j >= idxEntries.Count)
                            {
                                // the housing.bin of MultiCollection.uop
                                data = housing;
                                identifier = HashLittle2(UopFileTypes.HousingBinName);
                            }
                            else
                            {
                                IdxEntry e = idxEntries[j];
                                reader.BaseStream.Seek(e.Offset, SeekOrigin.Begin);
                                data = reader.ReadBytes(e.Size);

                                if (gump)
                                {
                                    // width and height are put before the data
                                    var withSize = new byte[data.Length + 8];
                                    BitConverter.GetBytes((e.Extra >> 16) & 0xFFFF).CopyTo(withSize, 0);
                                    BitConverter.GetBytes(e.Extra & 0xFFFF).CopyTo(withSize, 4);
                                    data.CopyTo(withSize, 8);
                                    data = withSize;
                                }
                                else if (type == FileType.MultiCollection)
                                {
                                    data = MulMultiToUop(e.Id, data, multiRecord);
                                }

                                // gump 9834 uses a 7 digit name in the client files
                                identifier = gump && e.Id == 9834
                                    ? HashLittle2(string.Format(hashFormat[1], e.Id))
                                    : HashLittle2(string.Format(hashFormat[0], e.Id));
                            }

                            byte[] stored = Compress(data, compressionFlag, out int decompressed);

                            tableEntries[tableIdx].Offset = writer.BaseStream.Position;
                            tableEntries[tableIdx].DecompressedSize = decompressed;
                            tableEntries[tableIdx].Size = stored.Length;
                            tableEntries[tableIdx].CompressionFlag = (short)(compressionFlag == CompressionFlag.None ? 0 : (int)compressionFlag);
                            tableEntries[tableIdx].Identifier = identifier;
                            tableEntries[tableIdx].Hash = HashAdler32(stored);
                            writer.Write(stored);
                            result.Entries++;
                            result.Bytes += stored.Length;
                        }

                        long nextTable = writer.BaseStream.Position;

                        // Go back and fix the table header
                        if (i < tableCount - 1)
                        {
                            writer.BaseStream.Seek(thisTable + 4, SeekOrigin.Begin);
                            writer.Write(nextTable);
                        }
                        else
                        {
                            writer.BaseStream.Seek(thisTable + 12, SeekOrigin.Begin);
                        }

                        // Table entries
                        tableIdx = 0;
                        for (int j = idxStart; j < idxEnd; ++j, ++tableIdx)
                        {
                            writer.Write(tableEntries[tableIdx].Offset);
                            writer.Write(0); // header length
                            writer.Write(tableEntries[tableIdx].Size); // compressed size
                            writer.Write(tableEntries[tableIdx].DecompressedSize); // decompressed size
                            writer.Write(tableEntries[tableIdx].Identifier);
                            writer.Write(tableEntries[tableIdx].Hash);
                            writer.Write(tableEntries[tableIdx].CompressionFlag); // compression method
                        }
                        for (; tableIdx < TableSize; ++tableIdx)
                        {
                            writer.Write(_emptyTableEntry);
                        }

                        writer.BaseStream.Seek(nextTable, SeekOrigin.Begin);
                        progress?.Report((int)((i + 1) * 100L / Math.Max(1, tableCount)));
                    }
                }

                Commit(temp, outFile, backup);
                return result;
            }
            finally
            {
                TryDelete(temp);
            }
        }

        private static List<IdxEntry> ReadEntries(BinaryReader reader, BinaryReader readerIdx, FileType type)
        {
            var idxEntries = new List<IdxEntry>();
            if (type == FileType.MapLegacyMul)
            {
                // No IDX file, just group the data into 0xC4000 long chunks
                long length = reader.BaseStream.Length;
                int id = 0;
                for (long position = 0; position < length; position += UopFileTypes.MapChunkSize)
                {
                    idxEntries.Add(new IdxEntry { Id = id++, Offset = (int)position, Size = UopFileTypes.MapChunkSize });
                }
                return idxEntries;
            }

            int idxEntryCount = (int)(readerIdx.BaseStream.Length / 12);
            long mulLength = reader.BaseStream.Length;
            for (int i = 0; i < idxEntryCount; ++i)
            {
                int offset = readerIdx.ReadInt32();
                int size = readerIdx.ReadInt32();
                int extra = readerIdx.ReadInt32();
                if (offset < 0 || size < 0 || offset + (long)size > mulLength)
                {
                    continue; // empty (or broken) entry
                }
                idxEntries.Add(new IdxEntry { Id = i, Offset = offset, Size = size, Extra = extra });
            }
            return idxEntries;
        }

        /// <summary>
        /// Data as stored in the UOP. <paramref name="decompressedSize"/> = size after zlib (for Mythic: header + transformed data,
        /// like the client files).
        /// </summary>
        private static byte[] Compress(byte[] data, CompressionFlag flag, out int decompressedSize)
        {
            decompressedSize = data.Length;
            switch (flag)
            {
                case CompressionFlag.Zlib:
                {
                    var r = UopUtils.Compress(data);
                    if (!r.success)
                    {
                        throw new InvalidDataException("Zlib compression failed.");
                    }
                    return r.compressedData;
                }
                case CompressionFlag.Mythic:
                {
                    // Mythic: 4 byte header (length ^ 0x8E2C9A3D) + transformed data, then zlib (like the client files)
                    byte[] transformed = MythicDecompress.Transform(data);
                    var withHeader = new byte[transformed.Length + 4];
                    BitConverter.GetBytes((uint)data.Length ^ 0x8E2C9A3D).CopyTo(withHeader, 0);
                    transformed.CopyTo(withHeader, 4);
                    decompressedSize = withHeader.Length;
                    var r = UopUtils.Compress(withHeader);
                    if (!r.success)
                    {
                        throw new InvalidDataException("Zlib compression failed.");
                    }
                    return r.compressedData;
                }
                default:
                    return data;
            }
        }

        // ------------------------------------------------------------------ multis

        /// <summary>Size of one part in multi.mul: 16 bytes (Stygian Abyss and newer) or 12 bytes (older clients).</summary>
        private static int MultiRecordSize(List<IdxEntry> entries) =>
            entries.Count > 0 && entries.All(e => e.Size % 16 == 0) ? 16 : 12;

        /// <summary>
        /// multi.mul record → MultiCollection.uop entry: int id, int count, parts (ushort item, short x, y, z, ushort flags,
        /// uint cliloc count). Classic flag 0 (not shown) → UOP flag 1, other classic flags → 0 (shown).
        /// </summary>
        private static byte[] MulMultiToUop(int id, byte[] data, int recordSize)
        {
            int count = data.Length / recordSize;
            using (var ms = new MemoryStream(8 + (count * 14)))
            using (var w = new BinaryWriter(ms))
            {
                w.Write(id);
                w.Write(count);
                for (int i = 0; i < count; i++)
                {
                    int o = i * recordSize;
                    w.Write(BitConverter.ToUInt16(data, o)); // item
                    w.Write(BitConverter.ToInt16(data, o + 2)); // x
                    w.Write(BitConverter.ToInt16(data, o + 4)); // y
                    w.Write(BitConverter.ToInt16(data, o + 6)); // z
                    uint flags = BitConverter.ToUInt32(data, o + 8);
                    w.Write((ushort)(flags == 0 ? 1 : 0));
                    w.Write(0); // no clilocs
                }
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>MultiCollection.uop entry → multi.mul record (16 byte parts).</summary>
        private static void WriteMultiUopEntryToMul(BinaryWriter mulWriter, byte[] chunkData)
        {
            Span<byte> span = chunkData.AsSpan();
            uint count = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(span[4..]);
            span = span[8..];

            for (int i = 0; i < count && span.Length >= 14; i++)
            {
                ushort itemId = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(span);
                short x = System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(span[2..]);
                short y = System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(span[4..]);
                short z = System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(span[6..]);
                ushort flagValue = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(span[8..]);
                uint clilocsCount = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(span[10..]);

                int skip = (int)Math.Min(clilocsCount, (uint)((span.Length - 14) / 4)) * 4;
                span = span[(14 + skip)..];

                mulWriter.Write(itemId);
                mulWriter.Write(x);
                mulWriter.Write(y);
                mulWriter.Write(z);
                mulWriter.Write(flagValue != 0 ? 0 : 1);
                mulWriter.Write(0);
            }
        }

        // ================================================================== UOP -> MUL

        /// <summary>
        /// Unpacks a UOP file into MUL / IDX. Maps have no IDX (<paramref name="outFileIdx"/> is ignored).
        /// For MultiCollection.uop <paramref name="housingBinFile"/> receives the housing.bin (needed to pack it again).
        /// Entries with unknown names are skipped and listed in the warnings.
        /// </summary>
        public static ConversionResult FromUop(string inFile, string outFile, string outFileIdx, FileType type, int typeIndex,
            string housingBinFile = null, PackerBackup backup = null, IProgress<int> progress = null, CancellationToken cancel = default)
        {
            var result = new ConversionResult();
            var chunkIds = new Dictionary<ulong, int>();
            string[] formats = UopFileTypes.GetHashFormat(type, typeIndex, out int maxId);
            for (int i = 0; i < maxId; ++i)
            {
                chunkIds[HashLittle2(string.Format(formats[0], i))] = i;
                if (formats[1] != string.Empty)
                {
                    chunkIds.TryAdd(HashLittle2(string.Format(formats[1], i)), i);
                }
            }
            ulong housingHash = HashLittle2(UopFileTypes.HousingBinName);

            bool map = type == FileType.MapLegacyMul;
            bool[] used = new bool[maxId];
            string tempMul = outFile + ".tmp";
            string tempIdx = map || string.IsNullOrWhiteSpace(outFileIdx) ? null : outFileIdx + ".tmp";
            string tempHousing = string.IsNullOrWhiteSpace(housingBinFile) ? null : housingBinFile + ".tmp";
            bool housingWritten = false;

            try
            {
                using (var reader = new BinaryReader(new FileStream(inFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                using (var mulWriter = new BinaryWriter(new FileStream(tempMul, FileMode.Create, FileAccess.Write, FileShare.None)))
                using (BinaryWriter idxWriter = tempIdx == null ? null : new BinaryWriter(new FileStream(tempIdx, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    if (reader.ReadInt32() != 0x50594D) // MYP
                    {
                        throw new InvalidDataException($"{Path.GetFileName(inFile)} is not a UOP file.");
                    }

                    Stream stream = reader.BaseStream;
                    reader.ReadInt32(); // version
                    reader.ReadInt32(); // signature
                    long nextTable = reader.ReadInt64();
                    reader.ReadInt32(); // table size
                    int fileCount = reader.ReadInt32();
                    int done = 0;

                    while (nextTable != 0 && nextTable < stream.Length)
                    {
                        cancel.ThrowIfCancellationRequested();
                        stream.Seek(nextTable, SeekOrigin.Begin);
                        int entries = reader.ReadInt32();
                        nextTable = reader.ReadInt64();

                        var offsets = new TableEntry[entries];
                        for (int i = 0; i < entries; ++i)
                        {
                            // empty entries are read too, they don't always mark the end of the table (7.0.26.4+ maps)
                            offsets[i].Offset = reader.ReadInt64();
                            offsets[i].HeaderLength = reader.ReadInt32();
                            offsets[i].Size = reader.ReadInt32();
                            offsets[i].DecompressedSize = reader.ReadInt32();
                            offsets[i].Identifier = reader.ReadUInt64();
                            offsets[i].Hash = reader.ReadUInt32();
                            offsets[i].CompressionFlag = reader.ReadInt16();
                        }

                        for (int i = 0; i < offsets.Length; ++i)
                        {
                            if (offsets[i].Offset == 0)
                            {
                                continue; // empty entry
                            }
                            done++;

                            stream.Seek(offsets[i].Offset + offsets[i].HeaderLength, SeekOrigin.Begin);
                            byte[] chunkData = reader.ReadBytes(offsets[i].Size);
                            chunkData = Decompress(chunkData, offsets[i], result);
                            if (chunkData == null)
                            {
                                result.Skipped++;
                                continue;
                            }

                            if (type == FileType.MultiCollection && offsets[i].Identifier == housingHash)
                            {
                                // housing.bin: not needed by the muls, but to pack the multis again
                                if (tempHousing != null)
                                {
                                    File.WriteAllBytes(tempHousing, chunkData);
                                    housingWritten = true;
                                }
                                continue;
                            }

                            if (!chunkIds.TryGetValue(offsets[i].Identifier, out int chunkId))
                            {
                                result.Skipped++;
                                if (result.Warnings.Count < 20)
                                {
                                    result.Warnings.Add($"Unknown entry name (hash {offsets[i].Identifier:X16}) skipped.");
                                }
                                continue;
                            }

                            if (map)
                            {
                                // write the chunk at its position (no IDX)
                                mulWriter.Seek(chunkId * UopFileTypes.MapChunkSize, SeekOrigin.Begin);
                                mulWriter.Write(chunkData);
                            }
                            else
                            {
                                idxWriter.Seek(chunkId * 12, SeekOrigin.Begin);
                                idxWriter.Write((uint)mulWriter.BaseStream.Position);
                                switch (type)
                                {
                                    case FileType.GumpartLegacyMul:
                                    {
                                        // width and height are put before the data
                                        int width = BitConverter.ToInt32(chunkData, 0);
                                        int height = BitConverter.ToInt32(chunkData, 4);
                                        idxWriter.Write(chunkData.Length - 8);
                                        idxWriter.Write((width << 16) | height);
                                        mulWriter.Write(chunkData, 8, chunkData.Length - 8);
                                        break;
                                    }
                                    case FileType.SoundLegacyMul:
                                        // extra holds the id of the sound + 1
                                        idxWriter.Write(chunkData.Length);
                                        idxWriter.Write(chunkId + 1);
                                        mulWriter.Write(chunkData);
                                        break;
                                    case FileType.MultiCollection:
                                    {
                                        long start = mulWriter.BaseStream.Position;
                                        WriteMultiUopEntryToMul(mulWriter, chunkData);
                                        idxWriter.Write((int)(mulWriter.BaseStream.Position - start));
                                        idxWriter.Write(0);
                                        break;
                                    }
                                    default:
                                        idxWriter.Write(chunkData.Length);
                                        idxWriter.Write(0);
                                        mulWriter.Write(chunkData);
                                        break;
                                }
                                used[chunkId] = true;
                            }
                            result.Entries++;
                            result.Bytes += chunkData.Length;
                            if (fileCount > 0 && done % 64 == 0)
                            {
                                progress?.Report((int)Math.Min(99, done * 100L / fileCount));
                            }
                        }
                    }

                    // unused index entries
                    if (idxWriter != null)
                    {
                        for (int i = 0; i < used.Length; ++i)
                        {
                            if (!used[i])
                            {
                                idxWriter.Seek(i * 12, SeekOrigin.Begin);
                                idxWriter.Write(-1);
                                idxWriter.Write((long)0);
                            }
                        }
                    }
                }

                if (map)
                {
                    TrimMapFile(tempMul, typeIndex, result);
                }
                if (type == FileType.MultiCollection && tempHousing != null && !housingWritten)
                {
                    result.Warnings.Add("The UOP has no housing.bin.");
                }

                Commit(tempMul, outFile, backup);
                if (tempIdx != null)
                {
                    Commit(tempIdx, outFileIdx, backup);
                }
                if (housingWritten)
                {
                    Commit(tempHousing, housingBinFile, backup);
                }
                progress?.Report(100);
                return result;
            }
            finally
            {
                TryDelete(tempMul);
                TryDelete(tempIdx);
                TryDelete(tempHousing);
            }
        }

        private static byte[] Decompress(byte[] data, TableEntry entry, ConversionResult result)
        {
            switch (entry.CompressionFlag)
            {
                case 0:
                    return data;
                case 1:
                case 3:
                {
                    using (var zlib = new ZLibStream(new MemoryStream(data), CompressionMode.Decompress))
                    using (var output = new MemoryStream(Math.Max(16, entry.DecompressedSize)))
                    {
                        zlib.CopyTo(output);
                        byte[] decompressed = output.ToArray();
                        // 3 = Mythic: zlib, then the Mythic transform (newer gumpartLegacyMUL.uop)
                        return entry.CompressionFlag == 3 ? MythicDecompress.Decompress(decompressed) : decompressed;
                    }
                }
                default:
                    if (result.Warnings.Count < 20)
                    {
                        result.Warnings.Add($"Entry {entry.Identifier:X16}: unknown compression {entry.CompressionFlag} – skipped.");
                    }
                    return null;
            }
        }

        /// <summary>
        /// Some maps have extra bytes at the end after unpacking; many tools expect the exact size.
        /// Only bytes after the known size that are all zero are removed – bigger custom maps stay as they are.
        /// </summary>
        private static void TrimMapFile(string path, int typeIndex, ConversionResult result)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                long expected = UopFileTypes.ExpectedMapFileSize(typeIndex);
                long target = expected > 0 && file.Length > expected ? expected : file.Length - (file.Length % UopFileTypes.MapBlockSize);
                if (target <= 0 || target >= file.Length)
                {
                    return;
                }

                file.Seek(target, SeekOrigin.Begin);
                var buffer = new byte[65536];
                int read;
                while ((read = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < read; i++)
                    {
                        if (buffer[i] != 0)
                        {
                            result.Warnings.Add($"The map is bigger than the known size ({file.Length:N0} bytes) – kept (custom map?).");
                            return;
                        }
                    }
                }
                result.Warnings.Add($"{file.Length - target:N0} empty bytes at the end of the map removed.");
                file.SetLength(target);
            }
        }

        // ================================================================== helpers

        /// <summary>Replaces the target with the temporary file (backup of the old target first).</summary>
        private static void Commit(string temp, string target, PackerBackup backup)
        {
            backup?.BackupExisting(target);
            File.Move(temp, target, true);
            backup?.CopyNew(target);
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (path != null && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
                // leave it
            }
        }

        //
        // Hash functions (EA didn't write these, see http://burtleburtle.net/bob/c/lookup3.c)
        //
        public static ulong HashLittle2(string s)
        {
            int length = s.Length;

            uint a, b, c;
            a = b = c = 0xDEADBEEF + (uint)length;

            int k = 0;

            while (length > 12)
            {
                a += s[k];
                a += (uint)s[k + 1] << 8;
                a += (uint)s[k + 2] << 16;
                a += (uint)s[k + 3] << 24;
                b += s[k + 4];
                b += (uint)s[k + 5] << 8;
                b += (uint)s[k + 6] << 16;
                b += (uint)s[k + 7] << 24;
                c += s[k + 8];
                c += (uint)s[k + 9] << 8;
                c += (uint)s[k + 10] << 16;
                c += (uint)s[k + 11] << 24;

                a -= c; a ^= c << 4 | c >> 28; c += b;
                b -= a; b ^= a << 6 | a >> 26; a += c;
                c -= b; c ^= b << 8 | b >> 24; b += a;
                a -= c; a ^= c << 16 | c >> 16; c += b;
                b -= a; b ^= a << 19 | a >> 13; a += c;
                c -= b; c ^= b << 4 | b >> 28; b += a;

                length -= 12;
                k += 12;
            }

            if (length != 0)
            {
                switch (length)
                {
                    case 12: c += (uint)s[k + 11] << 24; goto case 11;
                    case 11: c += (uint)s[k + 10] << 16; goto case 10;
                    case 10: c += (uint)s[k + 9] << 8; goto case 9;
                    case 9: c += s[k + 8]; goto case 8;
                    case 8: b += (uint)s[k + 7] << 24; goto case 7;
                    case 7: b += (uint)s[k + 6] << 16; goto case 6;
                    case 6: b += (uint)s[k + 5] << 8; goto case 5;
                    case 5: b += s[k + 4]; goto case 4;
                    case 4: a += (uint)s[k + 3] << 24; goto case 3;
                    case 3: a += (uint)s[k + 2] << 16; goto case 2;
                    case 2: a += (uint)s[k + 1] << 8; goto case 1;
                    case 1: a += s[k]; break;
                }

                c ^= b; c -= b << 14 | b >> 18;
                a ^= c; a -= c << 11 | c >> 21;
                b ^= a; b -= a << 25 | a >> 7;
                c ^= b; c -= b << 16 | b >> 16;
                a ^= c; a -= c << 4 | c >> 28;
                b ^= a; b -= a << 14 | a >> 18;
                c ^= b; c -= b << 24 | b >> 8;
            }

            return ((ulong)b << 32) | c;
        }

        public static uint HashAdler32(byte[] d)
        {
            uint a = 1;
            uint b = 0;

            for (int i = 0; i < d.Length; i++)
            {
                a = (a + d[i]) % 65521;
                b = (b + a) % 65521;
            }

            return (b << 16) | a;
        }
    }
}
