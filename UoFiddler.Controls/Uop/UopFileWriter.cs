using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace UoFiddler.Controls.Uop
{
    /// <summary>
    /// Represents the data for a single entry to be written to a UOP file.
    /// </summary>
    public class UopFileData
    {
        public ulong Hash { get; set; }
        public uint HeaderSize { get; set; } // To preserve original header size
        public byte[]? HeaderBytes { get; set; } // The actual header content
        public byte[]? Data { get; set; } // Decompressed data, used if PrecompressedData is null.
        public byte[]? PrecompressedData { get; set; } // Raw, compressed data. If set, this is used directly.
        public uint DecompressedSize { get; set; } // Must be set if PrecompressedData is used.
        public bool IsCompressed { get; set; } = true; // Used if Data is set.
        public bool IsEmpty { get; set; } = false; // Flag for entries with no data.
    }

    /// <summary>
    /// Handles writing of UO UOP package files.
    /// </summary>
    public static class UopFileWriter
    {
        /// <summary>
        /// Writes a collection of file data into a UOP file, overwriting if it exists.
        /// </summary>
        /// <param name="filePath">The path to the UOP file to create.</param>
        /// <param name="fileEntries">The collection of file entries to write.</param>
        /// <param name="blockCapacity">The number of entries per TOC block.</param>
        public static void WriteUopFile(string filePath, IEnumerable<UopFileData> fileEntries, uint blockCapacity)
        {
            var entries = fileEntries.ToList();
            if (blockCapacity == 0) blockCapacity = 100; // Safety fallback
            
            var tocEntries = new List<TocEntry>();
            long currentDataOffset = 32; // Initial offset after the main header

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(stream))
            {
                // 1. Write placeholder main header
                writer.Write((uint)0x0050594D); // "MYP\0"
                writer.Write((uint)5);          // Version
                writer.Write((uint)0xFD23EC43); // Signature
                writer.Write((ulong)0);         // Placeholder for first TOC block offset
                writer.Write(blockCapacity);    // Block capacity
                writer.Write((uint)entries.Count);      // Total files count
                writer.Write((uint)0);                  // 4 bytes of padding
                
                if(stream.Position != 32) throw new InvalidOperationException("UOP writer error: Header size is not 32 bytes.");

                // 2. Write data blocks and prepare TOC entries
                foreach (var entry in entries)
                {
                    if (entry.IsEmpty)
                    {
                        tocEntries.Add(new TocEntry
                        {
                            Offset = 0, HeaderSize = 0, CompressedSize = 0, DecompressedSize = 0,
                            Hash = entry.Hash, Flag = 0
                        });
                        continue;
                    }

                    byte[] dataToWrite;
                    uint compressedSize;
                    uint decompressedSize;
                    bool isCompressed;

                    // Write header if present
                    if (entry.HeaderSize > 0)
                    {
                        if (entry.HeaderBytes != null && entry.HeaderBytes.Length == entry.HeaderSize)
                        {
                            writer.Write(entry.HeaderBytes);
                        }
                        else
                        {
                            // Fallback: write zeros if no header data provided but size is set
                            writer.Write(new byte[entry.HeaderSize]);
                        }
                    }

                    if (entry.PrecompressedData != null)
                    {
                        dataToWrite = entry.PrecompressedData;
                        compressedSize = (uint)dataToWrite.Length;
                        decompressedSize = entry.DecompressedSize;
                        isCompressed = true; // We assume precompressed data is, in fact, compressed.
                    }
                    else
                    {
                        decompressedSize = (uint)entry.Data.Length;
                        isCompressed = entry.IsCompressed;

                        if (entry.IsCompressed)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                using (var zlibStream = new ZLibStream(memoryStream, CompressionMode.Compress))
                                {
                                    zlibStream.Write(entry.Data, 0, entry.Data.Length);
                                }
                                dataToWrite = memoryStream.ToArray();
                                compressedSize = (uint)dataToWrite.Length;
                            }
                        }
                        else
                        {
                            dataToWrite = entry.Data;
                            compressedSize = decompressedSize;
                        }
                    }

                    writer.Write(dataToWrite, 0, dataToWrite.Length);

                    tocEntries.Add(new TocEntry
                    {
                        Offset = (ulong)currentDataOffset,
                        HeaderSize = entry.HeaderSize,
                        CompressedSize = compressedSize,
                        DecompressedSize = decompressedSize,
                        Hash = entry.Hash,
                        Flag = (ushort)(isCompressed ? 1 : 0)
                    });

                    currentDataOffset = stream.Position;
                }

                // 3. Write TOC blocks
                long firstTocOffset = stream.Position;
                int tocEntryIndex = 0;

                while (tocEntryIndex < tocEntries.Count)
                {
                    long currentTocBlockOffset = stream.Position;
                    var blockEntries = tocEntries.Skip(tocEntryIndex).Take((int)blockCapacity).ToList();
                    
                    writer.Write((uint)blockEntries.Count);
                    
                    long nextTocBlockOffset = 0;
                    if (tocEntryIndex + blockEntries.Count < tocEntries.Count)
                    {
                        nextTocBlockOffset = currentTocBlockOffset + 4 + 8 + (blockEntries.Count * 34);
                    }
                    writer.Write((ulong)nextTocBlockOffset);

                    foreach (var tocEntry in blockEntries)
                    {
                        writer.Write(tocEntry.Offset);
                        writer.Write(tocEntry.HeaderSize);
                        writer.Write(tocEntry.CompressedSize);
                        writer.Write(tocEntry.DecompressedSize);
                        writer.Write(tocEntry.Hash);
                        writer.Write((uint)0); // Unknown
                        writer.Write(tocEntry.Flag);
                    }
                    
                    tocEntryIndex += blockEntries.Count;
                }
        
                // 4. Go back and write the final header
                stream.Seek(12, SeekOrigin.Begin); // Position for first TOC block offset
                writer.Write((ulong)firstTocOffset);
            }
        }

        private class TocEntry
        {
            public ulong Offset { get; set; }
            public uint HeaderSize { get; set; }
            public uint CompressedSize { get; set; }
            public uint DecompressedSize { get; set; }
            public ulong Hash { get; set; }
            public ushort Flag { get; set; }
        }
    }
}
