using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace UoFiddler.Controls.Uop
{
    public class UopFileReader
    {
        private string _filePath;
        private Dictionary<ulong, List<UopDataHeader>> _uopEntries;

        public string FilePath => _filePath;
        public bool IsLoaded { get; private set; }
        public uint BlockCapacity { get; private set; }

        public UopFileReader(string filePath)
        {
            _filePath = filePath;
            _uopEntries = new Dictionary<ulong, List<UopDataHeader>>();
            IsLoaded = false;
        }

        public bool Load()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"Erreur: Fichier UOP non trouvé à {_filePath}");
                return false;
            }

            try
            {
                using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new BinaryReader(stream))
                {
                    uint formatID = reader.ReadUInt32();
                    if (formatID != 0x0050594D) // "MYP\0" inversé
                    {
                        Console.WriteLine($"Avertissement: Le fichier UOP '{_filePath}' a un formatID inattendu: {formatID:X8}");
                        return false;
                    }

                    reader.ReadUInt32(); // formatVersion
                    reader.ReadUInt32(); // Signature?
                    ulong nextBlockOffset = reader.ReadUInt64();
                    BlockCapacity = reader.ReadUInt32(); // Block capacity?
                    reader.ReadUInt32(); // Total files count

                    _uopEntries.Clear();
                    int blockCount = 0;
                    int totalEntriesFound = 0;

                    while (nextBlockOffset != 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"🔍 [UOPReader] Reading TOC block at {nextBlockOffset:X16}");
                        stream.Seek((long)nextBlockOffset, SeekOrigin.Begin);

                        uint countInBlock = reader.ReadUInt32();
                        ulong currentNext = reader.ReadUInt64();
                        
                        // System.Diagnostics.Debug.WriteLine($"🔍 [UOPReader] Block contains {countInBlock} files. Next block pointer read: {currentNext:X16}");
                        
                        nextBlockOffset = currentNext;

                        for (int i = 0; i < countInBlock; i++)
                        {
                            ulong offset = reader.ReadUInt64();
                            uint headerSize = reader.ReadUInt32();
                            uint compressedSize = reader.ReadUInt32();
                            uint decompressedSize = reader.ReadUInt32();
                            ulong hash = reader.ReadUInt64();
                            reader.ReadUInt32(); // unknown
                            ushort flag = reader.ReadUInt16();

                            if (offset != 0)
                            {
                                if (!_uopEntries.TryGetValue(hash, out var list))
                                {
                                    list = new List<UopDataHeader>();
                                    _uopEntries[hash] = list;
                                }
                                list.Add(new UopDataHeader(offset, headerSize, compressedSize, decompressedSize, hash, flag));
                                
                                // if (hash == 0xC0165B63D153B1DD) // MainMisc Table Hash
                                // {
                                //     System.Diagnostics.Debug.WriteLine($"🎯 [UOPReader] Found MainMisc Table chunk: Offset={offset}, Comp={compressedSize}, Decomp={decompressedSize}");
                                // }
                            }
                        }
                    }
                    // System.Diagnostics.Debug.WriteLine($"✅ [UOPReader] Finished loading. Total entries: {_uopEntries.Count}.");
                }
                IsLoaded = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement du fichier UOP '{_filePath}': {ex.Message}");
                IsLoaded = false;
                return false;
            }
        }

        public byte[]? ReadData(UopDataHeader header)
        {
            if (!IsLoaded || header.Offset == 0 || header.DecompressedSize == 0)
            {
                return null;
            }

            try
            {
                using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    stream.Seek((long)header.Offset + header.HeaderSize, SeekOrigin.Begin);

                    int bytesToRead = (header.Flag == 0) ? (int)header.DecompressedSize : (int)header.CompressedSize;
                    byte[] data = new byte[bytesToRead];
                    int bytesReadCount = stream.Read(data, 0, data.Length);

                    if (bytesReadCount != data.Length)
                    {
                        return null;
                    }

                    if (header.Flag != 0)
                    {
                        byte[] decompressedData = new byte[header.DecompressedSize];
                        int totalRead = 0;
                        
                        using (var memoryStream = new MemoryStream(data))
                        {
                            // On essaie de lire autant que possible, même si le flux prétend être fini
                            // car certains UOP ont des marqueurs internes qui trompent ZLibStream.
                            while (totalRead < (int)header.DecompressedSize)
                            {
                                try
                                {
                                    using (var zlibStream = new ZLibStream(memoryStream, CompressionMode.Decompress, true))
                                    {
                                        int bytesReadInPart = 0;
                                        int read;
                                        while (totalRead < (int)header.DecompressedSize && 
                                               (read = zlibStream.Read(decompressedData, totalRead, (int)header.DecompressedSize - totalRead)) > 0)
                                        {
                                            totalRead += read;
                                            bytesReadInPart += read;
                                        }
                                        
                                        if (bytesReadInPart == 0) break; // Plus rien à lire du tout
                                    }
                                }
                                catch 
                                { 
                                    // Si erreur, on tente de forcer la lecture du reste si c'est de l'ASCII/Raw
                                    break; 
                                }
                                
                                // Si on a fini de décompresser, on sort
                                if (totalRead >= (int)header.DecompressedSize) break;
                            }
                        }
                        
                        // Si on a réussi à lire au moins une partie, on renvoie ce qu'on a.
                        // Dans votre cas, on veut atteindre 340720.
                        return decompressedData;
                    }
                    else
                    {
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la lecture des données du fichier UOP '{_filePath}': {ex.Message}");
                return null;
            }
        }

        public byte[]? ReadRawData(UopDataHeader header)
        {
            if (!IsLoaded || header.Offset == 0)
            {
                return null;
            }

            try
            {
                using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    stream.Seek((long)header.Offset + header.HeaderSize, SeekOrigin.Begin);

                    int bytesToRead = (header.Flag == 0) ? (int)header.DecompressedSize : (int)header.CompressedSize;
                    byte[] data = new byte[bytesToRead];
                    int bytesReadCount = stream.Read(data, 0, data.Length);

                    if (bytesReadCount != data.Length)
                    {
                        return null;
                    }
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la lecture des données brutes du fichier UOP '{_filePath}': {ex.Message}");
                return null;
            }
        }

        public byte[] ReadHeaderData(UopDataHeader header)
        {
            if (!IsLoaded || header.Offset == 0 || header.HeaderSize == 0)
            {
                return new byte[0];
            }

            try
            {
                using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    stream.Seek((long)header.Offset, SeekOrigin.Begin);
                    byte[] headerData = new byte[header.HeaderSize];
                    int bytesRead = stream.Read(headerData, 0, headerData.Length);
                    return bytesRead == headerData.Length ? headerData : new byte[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lecture Header: {ex.Message}");
                return new byte[0];
            }
        }

        public UopDataHeader? GetEntryByHash(ulong hash)
        {
            if (_uopEntries.TryGetValue(hash, out var list) && list.Count > 0)
                return list[0];
            return null;
        }

        public List<UopDataHeader> GetEntriesByHash(ulong hash)
        {
            if (_uopEntries.TryGetValue(hash, out var list))
                return list;
            return new List<UopDataHeader>();
        }

        public IEnumerable<KeyValuePair<ulong, UopDataHeader>> GetAllEntries()
        {
            foreach (var kvp in _uopEntries)
            {
                yield return new KeyValuePair<ulong, UopDataHeader>(kvp.Key, kvp.Value[0]);
            }
        }

        public Dictionary<ulong, List<UopDataHeader>> GroupedEntries => _uopEntries;

        public static ulong CreateHash(string s)
        {
            byte[] data = Encoding.ASCII.GetBytes(s.ToLowerInvariant());
            uint length = (uint)data.Length;
            uint len = length;
            uint a, b, c;

            a = b = c = 0xdeadbeef + len;

            int offset = 0;
            while (len > 12)
            {
                a += BitConverter.ToUInt32(data, offset);
                b += BitConverter.ToUInt32(data, offset + 4);
                c += BitConverter.ToUInt32(data, offset + 8);
                Mix(ref a, ref b, ref c);
                len -= 12;
                offset += 12;
            }

            switch (len)
            {
                case 12: c += (uint)data[offset + 11] << 24; goto case 11;
                case 11: c += (uint)data[offset + 10] << 16; goto case 10;
                case 10: c += (uint)data[offset + 9] << 8; goto case 9;
                case 9: c += data[offset + 8]; goto case 8;
                case 8: b += (uint)data[offset + 7] << 24; goto case 7;
                case 7: b += (uint)data[offset + 6] << 16; goto case 6;
                case 6: b += (uint)data[offset + 5] << 8; goto case 5;
                case 5: b += data[offset + 4]; goto case 4;
                case 4: a += (uint)data[offset + 3] << 24; goto case 3;
                case 3: a += (uint)data[offset + 2] << 16; goto case 2;
                case 2: a += (uint)data[offset + 1] << 8; goto case 1;
                case 1: a += data[offset + 0]; break;
            }

            FinalMix(ref a, ref b, ref c);

            return ((ulong)b << 32) | c;
        }

        private static void Mix(ref uint a, ref uint b, ref uint c)
        {
            a -= c; a ^= Rot(c, 4); c += b;
            b -= a; b ^= Rot(a, 6); a += c;
            c -= b; c ^= Rot(b, 8); b += a;
            a -= c; a ^= Rot(c, 16); c += b;
            b -= a; b ^= Rot(a, 19); a += c;
            c -= b; c ^= Rot(b, 4); b += a;
        }

        private static void FinalMix(ref uint a, ref uint b, ref uint c)
        {
            c ^= b; c -= Rot(b, 14);
            a ^= c; a -= Rot(c, 11);
            b ^= a; b -= Rot(a, 25);
            c ^= b; c -= Rot(b, 16);
            a ^= c; a -= Rot(c, 4);
            b ^= a; b -= Rot(a, 14);
            c ^= b; c -= Rot(b, 24);
        }

        private static uint Rot(uint x, int k)
        {
            return (x << k) | (x >> (32 - k));
        }
    }

    public struct UopDataHeader
    {
        public ulong Offset { get; set; }
        public uint HeaderSize { get; set; }
        public uint CompressedSize { get; set; }
        public uint DecompressedSize { get; set; }
        public ulong Hash { get; set; }
        public ushort Flag { get; set; }

        public UopDataHeader(ulong offset, uint headerSize, uint compressedSize, uint decompressedSize, ulong hash, ushort flag)
        {
            Offset = offset;
            HeaderSize = headerSize;
            CompressedSize = compressedSize;
            DecompressedSize = decompressedSize;
            Hash = hash;
            Flag = flag;
        }
    }
}