/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Maps
{
    /// <summary>A static of the classic statics*.mul.</summary>
    public struct ClassicStatic
    {
        public ushort Id;
        public sbyte Z;
        public ushort Hue;
    }

    /// <summary>
    /// File names of one map: classic (map*.mul or map*LegacyMUL.uop, staidx*.mul, statics*.mul, radarcol.mul)
    /// and Enhanced Client (facet*.uop, facet*.dds). "x" maps (map1x.mul ↔ facet1x.uop) use the statics without x.
    /// </summary>
    public sealed class MapFileNames
    {
        public int Index { get; }
        public bool X { get; }

        public MapFileNames(int index, bool x)
        {
            Index = index;
            X = x;
        }

        private string Suffix => X ? "x" : string.Empty;

        public string MapMul => $"map{Index}{Suffix}.mul";
        public string MapUop => $"map{Index}{Suffix}LegacyMUL.uop";
        public string StaIdx => $"staidx{Index}.mul";
        public string Statics => $"statics{Index}.mul";
        public const string RadarCol = "radarcol.mul";
        public string Facet => $"facet{Index}{Suffix}.uop";
        public string RadarDds => $"facet{Index:D2}.dds";

        public static readonly string[] MapNames = { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "Ter Mur" };

        public string Title => (Index < MapNames.Length ? MapNames[Index] : "Map " + Index) + (X ? " (x)" : string.Empty);

        /// <summary>Known sizes of the maps (width, height); 0 = unknown (custom map).</summary>
        public static (int Width, int Height) DefaultSize(int index) => index switch
        {
            0 => (7168, 4096),
            1 => (7168, 4096),
            2 => (2304, 1600),
            3 => (2560, 2048),
            4 => (1448, 1448),
            5 => (1280, 4096),
            _ => (0, 0)
        };

        /// <summary>
        /// Size of a classic map from the length of map*.mul (the height of the known maps, the width from the blocks;
        /// map0 / map1 can be 6144 or 7168 wide).
        /// </summary>
        public static (int Width, int Height) SizeFromMapLength(int index, long length)
        {
            long blocks = length / ClassicMap.BlockSize;
            (int w, int h) = DefaultSize(index);
            if (h == 0 || blocks == 0)
            {
                return (w, h);
            }
            long blocksY = h / 8;
            long blocksX = blocks / blocksY;
            return ((int)(blocksX * 8), h);
        }
    }

    /// <summary>
    /// Classic client map: land from map*.mul (or map*LegacyMUL.uop) and statics from staidx*.mul / statics*.mul.
    /// Blocks of 8 x 8 tiles, block number = x block * (height / 8) + y block; in a block the cells go row by row.
    /// </summary>
    public sealed class ClassicMap
    {
        public const int BlockSize = 196;   // 4 bytes header + 64 cells of 3 bytes
        public const int StaticSize = 7;    // ushort id, byte x, byte y, sbyte z, ushort hue
        public const int MapChunkSize = 0xC4000;

        private byte[] _map;
        private byte[] _staidx;
        private byte[] _statics;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int BlocksX => Width / 8;
        public int BlocksY => Height / 8;
        public bool HasStatics => _staidx != null && _statics != null;
        public List<string> Problems { get; } = new List<string>();

        /// <summary>Opens the map. <paramref name="mapPath"/> can be map*.mul or map*LegacyMUL.uop; statics are optional.</summary>
        public static ClassicMap Open(string mapPath, string staIdxPath, string staticsPath, int width, int height)
        {
            if (width <= 0 || height <= 0 || width % 8 != 0 || height % 8 != 0)
            {
                throw new ArgumentException("Width and height must be greater than 0 and a multiple of 8.");
            }
            var map = new ClassicMap { Width = width, Height = height };
            map._map = mapPath.EndsWith(".uop", StringComparison.OrdinalIgnoreCase) ? ReadMapUop(mapPath) : File.ReadAllBytes(mapPath);

            long expected = (long)map.BlocksX * map.BlocksY * BlockSize;
            if (map._map.Length < expected)
            {
                map.Problems.Add($"The map file has {map._map.Length:N0} bytes, {width} x {height} needs {expected:N0} – the missing blocks are read as land 0.");
            }
            else if (map._map.Length > expected)
            {
                map.Problems.Add($"The map file has {map._map.Length - expected:N0} bytes more than {width} x {height} needs (wrong size chosen?).");
            }

            if (!string.IsNullOrWhiteSpace(staIdxPath) && !string.IsNullOrWhiteSpace(staticsPath) && File.Exists(staIdxPath) && File.Exists(staticsPath))
            {
                map._staidx = File.ReadAllBytes(staIdxPath);
                map._statics = File.ReadAllBytes(staticsPath);
            }
            return map;
        }

        private static readonly Regex _uopIndex = new Regex(@"map(\d+)x?legacymul\.uop$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>map*LegacyMUL.uop: chunks "build/map{n}legacymul/{i:D8}.dat" of 0xC4000 bytes put together again.</summary>
        public static byte[] ReadMapUop(string path)
        {
            Match m = _uopIndex.Match(Path.GetFileName(path));
            int index = m.Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
            using (EcUopPackage package = EcUopPackage.Open(path))
            {
                string pattern = $"build/map{index}legacymul/" + "{0:D8}.dat";
                package.ResolveNames(pattern, 0, 0x400);
                var chunks = new SortedDictionary<int, byte[]>();
                for (int i = 0; i < 0x400; i++)
                {
                    EcUopEntry entry = package.GetEntry(string.Format(CultureInfo.InvariantCulture, pattern, i));
                    if (entry != null)
                    {
                        chunks[i] = package.ReadData(entry);
                    }
                }
                if (chunks.Count == 0)
                {
                    throw new InvalidDataException($"{Path.GetFileName(path)} has no map chunks (build/map{index}legacymul/...).");
                }
                int last = 0;
                foreach (var c in chunks)
                {
                    last = Math.Max(last, (c.Key * MapChunkSize) + c.Value.Length);
                }
                var data = new byte[last];
                foreach (var c in chunks)
                {
                    c.Value.CopyTo(data, c.Key * MapChunkSize);
                }
                return data;
            }
        }

        public int BlockNumber(int bx, int by) => (bx * BlocksY) + by;

        /// <summary>Land of a tile (land 0, z 0 outside the file).</summary>
        public (ushort Id, sbyte Z) GetLand(int x, int y)
        {
            long offset = ((long)BlockNumber(x >> 3, y >> 3) * BlockSize) + 4 + ((((y & 7) << 3) + (x & 7)) * 3);
            if (x < 0 || y < 0 || x >= Width || y >= Height || offset + 3 > _map.Length)
            {
                return (0, 0);
            }
            return ((ushort)(_map[offset] | (_map[offset + 1] << 8)), (sbyte)_map[offset + 2]);
        }

        /// <summary>Statics of a block: index entry (lookup, length) or null when empty or broken.</summary>
        public (int Lookup, int Length)? StaticsIndex(int bx, int by)
        {
            if (!HasStatics)
            {
                return null;
            }
            long i = (long)BlockNumber(bx, by) * 12;
            if (i + 12 > _staidx.Length)
            {
                return null;
            }
            int lookup = BitConverter.ToInt32(_staidx, (int)i);
            int length = BitConverter.ToInt32(_staidx, (int)i + 4);
            if (lookup < 0 || length <= 0)
            {
                return null;
            }
            return (lookup, length);
        }

        /// <summary>
        /// Statics of a block per cell (index = y * 8 + x), in file order. Broken entries are skipped
        /// (and counted in <paramref name="broken"/> if given).
        /// </summary>
        public List<ClassicStatic>[] GetBlockStatics(int bx, int by, int[] broken = null)
        {
            var cells = new List<ClassicStatic>[64];
            (int Lookup, int Length)? index = StaticsIndex(bx, by);
            if (index == null)
            {
                return cells;
            }
            (int lookup, int length) = index.Value;
            if ((long)lookup + length > _statics.Length || length % StaticSize != 0)
            {
                if (broken != null)
                {
                    broken[0]++;
                }
                length = Math.Max(0, Math.Min(length, _statics.Length - lookup)) / StaticSize * StaticSize;
            }
            for (int p = lookup; p + StaticSize <= lookup + length; p += StaticSize)
            {
                int x = _statics[p + 2], y = _statics[p + 3];
                if (x > 7 || y > 7)
                {
                    if (broken != null)
                    {
                        broken[0]++;
                    }
                    continue;
                }
                int c = (y << 3) + x;
                (cells[c] ??= new List<ClassicStatic>()).Add(new ClassicStatic
                {
                    Id = BitConverter.ToUInt16(_statics, p),
                    Z = (sbyte)_statics[p + 4],
                    Hue = BitConverter.ToUInt16(_statics, p + 5)
                });
            }
            return cells;
        }
    }

    /// <summary>Writes classic map files (map*.mul, staidx*.mul, statics*.mul).</summary>
    public static class ClassicMapWriter
    {
        /// <summary>
        /// Writes the map block by block. <paramref name="land"/> gives land and z of a tile; <paramref name="statics"/>
        /// (optional) the statics of a tile. Files are written next to the targets first (.tmp) and then moved.
        /// </summary>
        public static void Write(string mapPath, string staIdxPath, string staticsPath, int width, int height,
            Func<int, int, (ushort Id, sbyte Z)> land, Func<int, int, IEnumerable<ClassicStatic>> statics,
            IProgress<int> progress = null, System.Threading.CancellationToken cancel = default)
        {
            int blocksX = width / 8, blocksY = height / 8;
            string mapTemp = mapPath + ".tmp";
            string idxTemp = staIdxPath != null ? staIdxPath + ".tmp" : null;
            string staTemp = staticsPath != null ? staticsPath + ".tmp" : null;
            try
            {
                using (var map = new BinaryWriter(new FileStream(mapTemp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20)))
                using (BinaryWriter idx = idxTemp != null ? new BinaryWriter(new FileStream(idxTemp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20)) : null)
                using (BinaryWriter sta = staTemp != null ? new BinaryWriter(new FileStream(staTemp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20)) : null)
                {
                    for (int bx = 0; bx < blocksX; bx++)
                    {
                        cancel.ThrowIfCancellationRequested();
                        for (int by = 0; by < blocksY; by++)
                        {
                            map.Write(0); // block header
                            for (int cy = 0; cy < 8; cy++)
                            {
                                for (int cx = 0; cx < 8; cx++)
                                {
                                    (ushort id, sbyte z) = land((bx * 8) + cx, (by * 8) + cy);
                                    map.Write(id);
                                    map.Write(z);
                                }
                            }

                            if (idx == null)
                            {
                                continue;
                            }
                            long start = sta.BaseStream.Position;
                            int count = 0;
                            if (statics != null)
                            {
                                for (int cy = 0; cy < 8; cy++)
                                {
                                    for (int cx = 0; cx < 8; cx++)
                                    {
                                        foreach (ClassicStatic s in statics((bx * 8) + cx, (by * 8) + cy))
                                        {
                                            sta.Write(s.Id);
                                            sta.Write((byte)cx);
                                            sta.Write((byte)cy);
                                            sta.Write(s.Z);
                                            sta.Write(s.Hue);
                                            count++;
                                        }
                                    }
                                }
                            }
                            if (count == 0)
                            {
                                idx.Write(-1);
                                idx.Write(-1);
                                idx.Write(0);
                            }
                            else
                            {
                                idx.Write((int)start);
                                idx.Write(count * ClassicMap.StaticSize);
                                idx.Write(0);
                            }
                        }
                        progress?.Report((int)((bx + 1) * 100L / blocksX));
                    }
                }
                Replace(mapTemp, mapPath);
                if (idxTemp != null)
                {
                    Replace(idxTemp, staIdxPath);
                    Replace(staTemp, staticsPath);
                }
            }
            finally
            {
                foreach (string t in new[] { mapTemp, idxTemp, staTemp })
                {
                    if (t != null && File.Exists(t))
                    {
                        File.Delete(t);
                    }
                }
            }
        }

        /// <summary>Replaces the target; an existing file is kept as &lt;name&gt;.&lt;date&gt;.bak.</summary>
        public static void Replace(string temp, string target)
        {
            if (File.Exists(target))
            {
                File.Move(target, $"{target}.{DateTime.Now:yyyyMMdd_HHmmss}.bak", true);
            }
            File.Move(temp, target);
        }
    }
}
