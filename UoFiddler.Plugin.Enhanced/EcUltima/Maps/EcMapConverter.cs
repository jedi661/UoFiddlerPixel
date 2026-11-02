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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Maps
{
    public sealed class MapConvertOptions
    {
        public bool Statics { get; set; } = true;

        /// <summary>Land ids replaced while converting (e.g. a season table: grass → snow).</summary>
        public Dictionary<ushort, ushort> LandTable { get; set; }

        public bool Compress { get; set; } = true;
    }

    /// <summary>Text report of a conversion / analysis.</summary>
    public sealed class MapReport
    {
        private readonly StringBuilder _text = new StringBuilder();
        public int Problems { get; private set; }
        public int Fixed { get; set; }

        public void Info(string line) => _text.AppendLine(line);

        public void Problem(string line)
        {
            Problems++;
            if (Problems <= 300)
            {
                _text.AppendLine("  ! " + line);
            }
            else if (Problems == 301)
            {
                _text.AppendLine("  ! ... (more problems, not listed)");
            }
        }

        public override string ToString() => _text.ToString();
    }

    /// <summary>
    /// Converts maps between the classic client (map*.mul + statics) and the Enhanced Client (facet*.uop),
    /// creates new maps, analyses and repairs both formats.
    /// <para>
    /// EC sector tiles on the sector border store the land of their neighbours outside the sector ("border layers":
    /// direction, z, land) – used by the client to blend the terrain across sectors. Directions: 0 left, 1 top left,
    /// 2 top, 3 right, 4 bottom right, 5 bottom, 6 bottom left, 7 top right. Edge tiles store the one neighbour across
    /// the edge, corner tiles also the diagonal one, sorted by direction; neighbours outside the map are left out
    /// (checked against the client files).
    /// </para>
    /// </summary>
    public static class EcMapConverter
    {
        public const string SizeEntryPattern = "build/sectors/facet_{0:D2}/data.map";

        private static readonly (int Dx, int Dy)[] _directions =
        {
            (-1, 0), (-1, -1), (0, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (1, -1)
        };

        /// <summary>Directions stored for a tile of the sector (local x, y), sorted.</summary>
        public static IEnumerable<int> BorderDirections(int lx, int ly)
        {
            bool left = lx == 0, right = lx == EcSector.Size - 1, top = ly == 0, bottom = ly == EcSector.Size - 1;
            if (left) yield return 0;
            if (left && top) yield return 1;
            if (top) yield return 2;
            if (right) yield return 3;
            if (right && bottom) yield return 4;
            if (bottom) yield return 5;
            if (left && bottom) yield return 6;
            if (right && top) yield return 7;
        }

        /// <summary>Border layers of a tile from a land function (map coordinates), neighbours outside the map are left out.</summary>
        public static List<EcLandLayer> BorderLayers(int gx, int gy, int width, int height, Func<int, int, (ushort Id, sbyte Z)> land)
        {
            var layers = new List<EcLandLayer>();
            int lx = gx % EcSector.Size, ly = gy % EcSector.Size;
            foreach (int d in BorderDirections(lx, ly).OrderBy(d => d))
            {
                int nx = gx + _directions[d].Dx, ny = gy + _directions[d].Dy;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                {
                    continue;
                }
                (ushort id, sbyte z) = land(nx, ny);
                layers.Add(new EcLandLayer { Blend = (byte)d, Z = z, LandId = id });
            }
            return layers;
        }

        // ================================================================== classic -> EC

        /// <summary>Builds one EC sector from a land function and a statics function (map coordinates).</summary>
        public static EcSector BuildSector(int facetId, int sx, int sy, int sectorsY, int width, int height,
            Func<int, int, (ushort Id, sbyte Z)> land, Func<int, int, IEnumerable<EcStatic>> statics, MapReport report = null)
        {
            var sector = new EcSector { Facet = (byte)facetId, Index = (ushort)((sx * sectorsY) + sy) };
            for (int lx = 0; lx < EcSector.Size; lx++)
            {
                for (int ly = 0; ly < EcSector.Size; ly++)
                {
                    int gx = (sx * EcSector.Size) + lx, gy = (sy * EcSector.Size) + ly;
                    var tile = new EcMapTile();
                    if (gx < width && gy < height)
                    {
                        (tile.LandId, tile.Z) = land(gx, gy);
                        if (statics != null)
                        {
                            foreach (EcStatic s in statics(gx, gy))
                            {
                                if (tile.Statics.Count == 255)
                                {
                                    report?.Problem($"{gx},{gy}: more than 255 statics – the rest is left out (the EC can't store more).");
                                    break;
                                }
                                tile.Statics.Add(s);
                            }
                        }
                    }
                    tile.Layers.AddRange(BorderLayers(gx, gy, width, height, land));
                    sector.Tiles[(lx * EcSector.Size) + ly] = tile;
                }
            }
            return sector;
        }

        /// <summary>
        /// Classic map → facet{N}.uop (sectors + data.map with the size). Existing output is kept as .bak.
        /// </summary>
        public static MapReport ClassicToEc(ClassicMap map, int facetId, string outUop, MapConvertOptions options,
            IProgress<int> progress = null, CancellationToken cancel = default)
        {
            var report = new MapReport();
            int width = map.Width, height = map.Height;
            int sectorsX = (width + EcSector.Size - 1) / EcSector.Size, sectorsY = (height + EcSector.Size - 1) / EcSector.Size;
            report.Info($"Classic map {width} x {height} → facet {facetId}: {sectorsX} x {sectorsY} = {sectorsX * sectorsY} sectors");
            foreach (string p in map.Problems)
            {
                report.Problem(p);
            }

            Dictionary<ushort, ushort> table = options.LandTable;
            (ushort, sbyte) Land(int x, int y)
            {
                (ushort id, sbyte z) = map.GetLand(x, y);
                return (table != null && table.TryGetValue(id, out ushort sub) ? sub : id, z);
            }

            bool withStatics = options.Statics && map.HasStatics;
            if (options.Statics && !map.HasStatics)
            {
                report.Info("No staidx / statics file – the facet is written without statics.");
            }

            var package = EcUopPackage.CreateEmpty(1000);
            int done = 0;
            long staticCount = 0;
            int broken = 0;
            var sync = new object();
            for (int sx = 0; sx < sectorsX; sx++)
            {
                cancel.ThrowIfCancellationRequested();
                var column = new byte[sectorsY][];
                int column_sx = sx;
                Parallel.For(0, sectorsY, sy =>
                {
                    // statics of the 8 x 8 blocks of this sector
                    var blockStatics = new Dictionary<int, List<ClassicStatic>[]>();
                    var brokenHere = new int[1];
                    IEnumerable<EcStatic> Statics(int x, int y)
                    {
                        int bx = x >> 3, by = y >> 3;
                        int key = (bx << 16) | by;
                        if (!blockStatics.TryGetValue(key, out List<ClassicStatic>[] cells))
                        {
                            cells = map.GetBlockStatics(bx, by, brokenHere);
                            blockStatics[key] = cells;
                        }
                        List<ClassicStatic> list = cells[((y & 7) << 3) + (x & 7)];
                        return list == null ? Enumerable.Empty<EcStatic>() : list.Select(s => new EcStatic { ItemId = s.Id, Z = s.Z, Hue = s.Hue });
                    }
                    var localReport = new MapReport();
                    EcSector sector = BuildSector(facetId, column_sx, sy, sectorsY, width, height, Land, withStatics ? Statics : null, localReport);
                    column[sy] = sector.Write();
                    lock (sync)
                    {
                        staticCount += sector.Tiles.Sum(t => (long)t.Statics.Count);
                        broken += brokenHere[0];
                        if (localReport.Problems > 0)
                        {
                            foreach (string line in localReport.ToString().Split('\n').Where(l => l.Trim().Length > 0))
                            {
                                report.Problem(line.Trim().TrimStart('!').Trim());
                            }
                        }
                    }
                });
                for (int sy = 0; sy < sectorsY; sy++)
                {
                    string name = string.Format(CultureInfo.InvariantCulture, EcFacet.SectorPattern, facetId, (sx * sectorsY) + sy);
                    package.AddOrReplace(name, column[sy], options.Compress ? EcUopCompression.Zlib : EcUopCompression.None);
                }
                done += sectorsY;
                progress?.Report((int)(done * 95L / (sectorsX * sectorsY)));
            }

            package.AddOrReplace(string.Format(CultureInfo.InvariantCulture, SizeEntryPattern, facetId), SizeEntry(width, height), EcUopCompression.None);
            if (broken > 0)
            {
                report.Problem($"{broken} broken statics index entries / statics were skipped.");
            }
            report.Info($"{staticCount:N0} statics written.");
            package.Save(outUop, true);
            progress?.Report(100);
            report.Info($"Saved {outUop}");
            return report;
        }

        /// <summary>The 8 byte size entry of a facet: uint height, uint width.</summary>
        public static byte[] SizeEntry(int width, int height)
        {
            var data = new byte[8];
            BitConverter.GetBytes((uint)height).CopyTo(data, 0);
            BitConverter.GetBytes((uint)width).CopyTo(data, 4);
            return data;
        }

        // ================================================================== EC -> classic

        /// <summary>facet*.uop → map*.mul (+ staidx*.mul / statics*.mul). Existing files are kept as .bak.</summary>
        public static MapReport EcToClassic(EcFacet facet, string outMap, string outStaIdx, string outStatics, MapConvertOptions options,
            IProgress<int> progress = null, CancellationToken cancel = default)
        {
            var report = new MapReport();
            int width = facet.Width / 8 * 8, height = facet.Height / 8 * 8;
            report.Info($"facet {facet.FacetId} {facet.Width} x {facet.Height} → classic map {width} x {height}");
            if (width != facet.Width || height != facet.Height)
            {
                report.Problem("The size is not a multiple of 8 – the classic map is cut to whole blocks.");
            }

            Dictionary<ushort, ushort> table = options.LandTable;
            int missing = 0, bigIds = 0;
            long staticCount = 0;
            var missingSectors = new HashSet<int>();
            (ushort, sbyte) Land(int x, int y)
            {
                EcMapTile t = facet.GetTile(x, y);
                if (t == null)
                {
                    if (missingSectors.Add(facet.SectorIndex(x / EcSector.Size, y / EcSector.Size)))
                    {
                        missing++;
                    }
                    return (0, 0);
                }
                return (table != null && table.TryGetValue(t.LandId, out ushort sub) ? sub : t.LandId, t.Z);
            }
            IEnumerable<ClassicStatic> Statics(int x, int y)
            {
                EcMapTile t = facet.GetTile(x, y);
                if (t == null)
                {
                    yield break;
                }
                foreach (EcStatic s in t.Statics)
                {
                    if (s.ItemId > ushort.MaxValue || s.Hue > ushort.MaxValue)
                    {
                        bigIds++;
                        continue;
                    }
                    staticCount++;
                    yield return new ClassicStatic { Id = (ushort)s.ItemId, Z = s.Z, Hue = (ushort)s.Hue };
                }
            }

            ClassicMapWriter.Write(outMap, options.Statics ? outStaIdx : null, options.Statics ? outStatics : null, width, height,
                Land, options.Statics ? Statics : null, progress, cancel);

            if (missing > 0)
            {
                report.Problem($"{missing} sectors are missing in the facet – written as land 0.");
            }
            if (bigIds > 0)
            {
                report.Problem($"{bigIds} statics with an item id or hue above 0xFFFF can't be stored in the classic files – left out.");
            }
            report.Info($"{staticCount:N0} statics written.");
            report.Info($"Saved {outMap}" + (options.Statics ? $", {outStaIdx}, {outStatics}" : string.Empty));
            return report;
        }

        // ================================================================== radar image

        /// <summary>
        /// Radar image facet{NN}.dds (1 pixel per tile, DXT1 with mip maps) from radarcol.mul: the colour of the highest
        /// static, else of the land (like the client). <paramref name="hueColor"/> (optional) colours hued statics.
        /// </summary>
        public static void WriteRadarImage(ClassicMap map, string radarColPath, string outDds, Func<ushort, int, ushort?> hueColor = null,
            IProgress<int> progress = null, CancellationToken cancel = default)
        {
            byte[] raw = File.ReadAllBytes(radarColPath);
            var colors = new ushort[raw.Length / 2];
            Buffer.BlockCopy(raw, 0, colors, 0, colors.Length * 2);

            int width = map.Width, height = map.Height;
            var bgra = new byte[(long)width * height * 4];
            for (int bx = 0; bx < map.BlocksX; bx++)
            {
                cancel.ThrowIfCancellationRequested();
                for (int by = 0; by < map.BlocksY; by++)
                {
                    List<ClassicStatic>[] cells = map.HasStatics ? map.GetBlockStatics(bx, by) : null;
                    for (int cy = 0; cy < 8; cy++)
                    {
                        for (int cx = 0; cx < 8; cx++)
                        {
                            int x = (bx * 8) + cx, y = (by * 8) + cy;
                            (ushort land, sbyte z) = map.GetLand(x, y);
                            ushort color = land < colors.Length ? colors[land] : (ushort)0;
                            List<ClassicStatic> list = cells?[(cy << 3) + cx];
                            if (list != null && list.Count > 0)
                            {
                                ClassicStatic top = list[0];
                                foreach (ClassicStatic s in list)
                                {
                                    if (s.Z >= top.Z)
                                    {
                                        top = s;
                                    }
                                }
                                if (top.Z >= z)
                                {
                                    int ci = 0x4000 + top.Id;
                                    color = ci < colors.Length ? colors[ci] : color;
                                    if (top.Hue != 0 && hueColor != null)
                                    {
                                        color = hueColor(top.Hue, (color >> 10) & 0x1F) ?? color;
                                    }
                                }
                            }
                            long o = (((long)y * width) + x) * 4;
                            bgra[o] = (byte)((color & 0x1F) * 255 / 31);
                            bgra[o + 1] = (byte)(((color >> 5) & 0x1F) * 255 / 31);
                            bgra[o + 2] = (byte)(((color >> 10) & 0x1F) * 255 / 31);
                            bgra[o + 3] = 255;
                        }
                    }
                }
                progress?.Report((int)((bx + 1) * 50L / map.BlocksX));
            }
            byte[] dds = DdsImage.Encode(bgra, width, height, DdsSaveFormat.Dxt1, 0);
            string temp = outDds + ".tmp";
            File.WriteAllBytes(temp, dds);
            ClassicMapWriter.Replace(temp, outDds);
            progress?.Report(100);
        }

        // ================================================================== new maps

        /// <summary>New facet{N}.uop filled with one land tile (z), border layers and data.map.</summary>
        public static MapReport CreateEc(int facetId, int width, int height, ushort land, sbyte z, string outUop, IProgress<int> progress = null)
        {
            var report = new MapReport();
            int sectorsX = (width + 63) / 64, sectorsY = (height + 63) / 64;
            var package = EcUopPackage.CreateEmpty(1000);
            (ushort, sbyte) Land(int x, int y) => (land, z);
            for (int sx = 0; sx < sectorsX; sx++)
            {
                for (int sy = 0; sy < sectorsY; sy++)
                {
                    EcSector sector = BuildSector(facetId, sx, sy, sectorsY, width, height, Land, null);
                    package.AddOrReplace(string.Format(CultureInfo.InvariantCulture, EcFacet.SectorPattern, facetId, (sx * sectorsY) + sy), sector.Write(), EcUopCompression.Zlib);
                }
                progress?.Report((sx + 1) * 100 / sectorsX);
            }
            package.AddOrReplace(string.Format(CultureInfo.InvariantCulture, SizeEntryPattern, facetId), SizeEntry(width, height), EcUopCompression.None);
            package.Save(outUop, true);
            report.Info($"New facet {facetId}: {width} x {height}, {sectorsX * sectorsY} sectors, land 0x{land:X4} z {z} → {outUop}");
            return report;
        }

        /// <summary>New classic map (map*.mul filled with one land tile, empty staidx / statics).</summary>
        public static MapReport CreateClassic(int width, int height, ushort land, sbyte z, string outMap, string outStaIdx, string outStatics, IProgress<int> progress = null)
        {
            var report = new MapReport();
            ClassicMapWriter.Write(outMap, outStaIdx, outStatics, width, height, (x, y) => (land, z), (x, y) => Enumerable.Empty<ClassicStatic>(), progress);
            report.Info($"New classic map {width} x {height}, land 0x{land:X4} z {z} → {outMap}, {outStaIdx}, {outStatics}");
            return report;
        }

        // ================================================================== analysis / repair

        /// <summary>Checks a classic map: file sizes, statics index, statics positions, duplicates, too many statics per tile.</summary>
        public static MapReport AnalyzeClassic(ClassicMap map, int maxItemId, IProgress<int> progress = null, CancellationToken cancel = default)
        {
            var report = new MapReport();
            report.Info($"Classic map {map.Width} x {map.Height} ({map.BlocksX} x {map.BlocksY} blocks)");
            foreach (string p in map.Problems)
            {
                report.Problem(p);
            }
            if (!map.HasStatics)
            {
                report.Info("No statics files – only the land was checked.");
            }

            long statics = 0, duplicates = 0, invalid = 0, crowded = 0, emptyBlocks = 0;
            var broken = new int[1];
            for (int bx = 0; bx < map.BlocksX; bx++)
            {
                cancel.ThrowIfCancellationRequested();
                for (int by = 0; by < map.BlocksY; by++)
                {
                    if (!map.HasStatics)
                    {
                        continue;
                    }
                    if (map.StaticsIndex(bx, by) == null)
                    {
                        emptyBlocks++;
                    }
                    List<ClassicStatic>[] cells = map.GetBlockStatics(bx, by, broken);
                    for (int c = 0; c < 64; c++)
                    {
                        List<ClassicStatic> list = cells[c];
                        if (list == null)
                        {
                            continue;
                        }
                        statics += list.Count;
                        int x = (bx * 8) + (c & 7), y = (by * 8) + (c >> 3);
                        if (list.Count > 255)
                        {
                            crowded++;
                            report.Problem($"{x},{y}: {list.Count} statics – the EC can store 255 per tile.");
                        }
                        var seen = new HashSet<(ushort, sbyte, ushort)>();
                        foreach (ClassicStatic s in list)
                        {
                            if (!seen.Add((s.Id, s.Z, s.Hue)))
                            {
                                duplicates++;
                            }
                            if (maxItemId > 0 && s.Id >= maxItemId)
                            {
                                invalid++;
                                if (invalid <= 20)
                                {
                                    report.Problem($"{x},{y}: static 0x{s.Id:X4} is above the highest item id 0x{maxItemId - 1:X4}.");
                                }
                            }
                        }
                    }
                }
                progress?.Report((int)((bx + 1) * 100L / map.BlocksX));
            }
            report.Info($"{statics:N0} statics, {emptyBlocks:N0} blocks without statics");
            if (broken[0] > 0)
            {
                report.Problem($"{broken[0]} broken statics index entries or statics with a position outside the block.");
            }
            if (duplicates > 0)
            {
                report.Problem($"{duplicates:N0} duplicate statics (same item, z and hue on the same tile).");
            }
            if (invalid > 20)
            {
                report.Problem($"{invalid:N0} statics with an invalid item id in all.");
            }
            report.Info(report.Problems == 0 ? "No problems found." : $"{report.Problems} problems found.");
            return report;
        }

        /// <summary>
        /// Writes cleaned statics (broken entries left out; optionally duplicates and invalid ids removed). The map file
        /// stays as it is. Old staidx / statics are kept as .bak.
        /// </summary>
        public static MapReport RepairClassicStatics(ClassicMap map, string staIdxPath, string staticsPath, bool removeDuplicates, int maxItemId,
            IProgress<int> progress = null)
        {
            var report = new MapReport();
            long removed = 0;
            string idxTemp = staIdxPath + ".tmp", staTemp = staticsPath + ".tmp";
            using (var idx = new BinaryWriter(File.Create(idxTemp)))
            using (var sta = new BinaryWriter(File.Create(staTemp)))
            {
                for (int bx = 0; bx < map.BlocksX; bx++)
                {
                    for (int by = 0; by < map.BlocksY; by++)
                    {
                        long start = sta.BaseStream.Position;
                        int count = 0;
                        List<ClassicStatic>[] cells = map.GetBlockStatics(bx, by);
                        for (int c = 0; c < 64; c++)
                        {
                            if (cells[c] == null)
                            {
                                continue;
                            }
                            var seen = new HashSet<(ushort, sbyte, ushort)>();
                            foreach (ClassicStatic s in cells[c])
                            {
                                if ((removeDuplicates && !seen.Add((s.Id, s.Z, s.Hue))) || (maxItemId > 0 && s.Id >= maxItemId))
                                {
                                    removed++;
                                    continue;
                                }
                                sta.Write(s.Id);
                                sta.Write((byte)(c & 7));
                                sta.Write((byte)(c >> 3));
                                sta.Write(s.Z);
                                sta.Write(s.Hue);
                                count++;
                            }
                        }
                        idx.Write(count == 0 ? -1 : (int)start);
                        idx.Write(count == 0 ? -1 : count * ClassicMap.StaticSize);
                        idx.Write(0);
                    }
                    progress?.Report((int)((bx + 1) * 100L / map.BlocksX));
                }
            }
            ClassicMapWriter.Replace(idxTemp, staIdxPath);
            ClassicMapWriter.Replace(staTemp, staticsPath);
            report.Fixed = (int)Math.Min(int.MaxValue, removed);
            report.Info($"Statics written again: {removed:N0} removed. Saved {staIdxPath}, {staticsPath} (old files kept as .bak).");
            return report;
        }

        public sealed class EcRepairOptions
        {
            public bool SizeEntry { get; set; } = true;
            public bool Headers { get; set; } = true;
            public bool BorderLayers { get; set; } = true;
            public bool MissingSectors { get; set; } = true;
            public bool Duplicates { get; set; }
            public bool ExtraSectors { get; set; }
        }

        /// <summary>
        /// Checks a facet (and repairs it when <paramref name="repair"/> is given): size entry, missing / extra sectors,
        /// sector headers, border layers that don't match the neighbour tiles, duplicate statics, unreadable sectors.
        /// The package is saved by the caller.
        /// </summary>
        public static MapReport AnalyzeEc(EcFacet facet, EcRepairOptions repair = null, IProgress<int> progress = null, CancellationToken cancel = default)
        {
            var report = new MapReport();
            EcUopPackage package = facet.Package;
            int count = facet.SectorsX * facet.SectorsY;
            report.Info($"{facet.FileName}: facet {facet.FacetId}, {facet.Width} x {facet.Height}, {facet.SectorsX} x {facet.SectorsY} = {count} sectors expected, {package.Entries.Count} entries");

            string sizeName = string.Format(CultureInfo.InvariantCulture, SizeEntryPattern, facet.FacetId);
            EcUopEntry sizeEntry = package.GetEntry(sizeName);
            if (sizeEntry == null)
            {
                report.Problem("The size entry (data.map) is missing – the client and the tools guess the size.");
                if (repair?.SizeEntry == true)
                {
                    package.AddOrReplace(sizeName, SizeEntry(facet.Width, facet.Height), EcUopCompression.None);
                    report.Fixed++;
                }
            }

            var expected = new HashSet<ulong>();
            int missing = 0, headers = 0, unreadable = 0, borders = 0, duplicates = 0;
            for (int sx = 0; sx < facet.SectorsX; sx++)
            {
                cancel.ThrowIfCancellationRequested();
                for (int sy = 0; sy < facet.SectorsY; sy++)
                {
                    int index = facet.SectorIndex(sx, sy);
                    string name = facet.SectorName(index);
                    expected.Add(EcUopHash.HashFileName(name));
                    EcSector sector;
                    if (package.GetEntry(name) == null)
                    {
                        missing++;
                        if (missing <= 20)
                        {
                            report.Problem($"Sector {index} ({sx},{sy}) is missing.");
                        }
                        if (repair?.MissingSectors == true)
                        {
                            // empty sector (land 0); its border layers take the real neighbour tiles
                            int ox = sx * EcSector.Size, oy = sy * EcSector.Size;
                            sector = BuildSector(facet.FacetId, sx, sy, facet.SectorsY, facet.Width, facet.Height, (x, y) =>
                            {
                                if (x >= ox && y >= oy && x < ox + EcSector.Size && y < oy + EcSector.Size)
                                {
                                    return ((ushort)0, (sbyte)0);
                                }
                                EcMapTile n = facet.GetTile(x, y);
                                return n == null ? ((ushort)0, (sbyte)0) : (n.LandId, n.Z);
                            }, null);
                            package.AddOrReplace(name, sector.Write(), EcUopCompression.Zlib);
                            report.Fixed++;
                        }
                        continue;
                    }
                    try
                    {
                        sector = facet.GetSector(sx, sy);
                    }
                    catch (Exception ex)
                    {
                        unreadable++;
                        report.Problem($"Sector {index} can't be read: {ex.Message}");
                        continue;
                    }

                    bool changed = false;
                    if (sector.Facet != facet.FacetId || sector.Index != index)
                    {
                        headers++;
                        if (headers <= 20)
                        {
                            report.Problem($"Sector {index}: header says facet {sector.Facet}, sector {sector.Index}.");
                        }
                        if (repair?.Headers == true)
                        {
                            sector.Facet = (byte)facet.FacetId;
                            sector.Index = (ushort)index;
                            changed = true;
                        }
                    }

                    for (int lx = 0; lx < EcSector.Size; lx++)
                    {
                        for (int ly = 0; ly < EcSector.Size; ly++)
                        {
                            EcMapTile tile = sector[lx, ly];
                            int gx = (sx * EcSector.Size) + lx, gy = (sy * EcSector.Size) + ly;
                            if (lx == 0 || ly == 0 || lx == EcSector.Size - 1 || ly == EcSector.Size - 1 || tile.Layers.Count > 0)
                            {
                                List<EcLandLayer> should = BorderLayers(gx, gy, facet.Width, facet.Height, (x, y) =>
                                {
                                    EcMapTile n = facet.GetTile(x, y);
                                    return n == null ? ((ushort)0, (sbyte)0) : (n.LandId, n.Z);
                                });
                                if (!SameLayers(tile.Layers, should))
                                {
                                    borders++;
                                    if (repair?.BorderLayers == true)
                                    {
                                        tile.Layers.Clear();
                                        tile.Layers.AddRange(should);
                                        changed = true;
                                    }
                                }
                            }
                            if (tile.Statics.Count > 1)
                            {
                                var seen = new HashSet<(uint, sbyte, uint)>();
                                int before = tile.Statics.Count;
                                List<EcStatic> unique = tile.Statics.Where(s => seen.Add((s.ItemId, s.Z, s.Hue))).ToList();
                                if (unique.Count != before)
                                {
                                    duplicates += before - unique.Count;
                                    if (repair?.Duplicates == true)
                                    {
                                        tile.Statics.Clear();
                                        tile.Statics.AddRange(unique);
                                        changed = true;
                                    }
                                }
                            }
                        }
                    }
                    if (changed)
                    {
                        // written at once (keeping thousands of changed sectors in memory is too much)
                        package.AddOrReplace(name, sector.Write(), EcUopCompression.Zlib);
                        report.Fixed++;
                    }
                }
                progress?.Report((int)((sx + 1) * 100L / facet.SectorsX));
            }

            // entries that are no sector of this map (and not the size entry)
            ulong sizeHash = EcUopHash.HashFileName(sizeName);
            List<EcUopEntry> extra = package.Entries.Where(e => e.Hash != sizeHash && !expected.Contains(e.Hash)).ToList();
            if (extra.Count > 0)
            {
                report.Problem($"{extra.Count} entries are no sector of a {facet.Width} x {facet.Height} map (wrong size or other data).");
                if (repair?.ExtraSectors == true)
                {
                    foreach (EcUopEntry e in extra)
                    {
                        package.Remove(e);
                    }
                    report.Fixed++;
                }
            }
            if (missing > 20)
            {
                report.Problem($"{missing} sectors are missing in all.");
            }
            if (headers > 20)
            {
                report.Problem($"{headers} sector headers are wrong in all.");
            }
            if (borders > 0)
            {
                report.Problem($"{borders:N0} border tiles don't match their neighbours (terrain blending between sectors).");
            }
            if (duplicates > 0)
            {
                report.Problem($"{duplicates:N0} duplicate statics.");
            }
            if (unreadable > 0)
            {
                report.Problem($"{unreadable} sectors can't be read.");
            }
            if (repair != null)
            {
                report.Info($"{report.Fixed} repairs made – save the facet to keep them.");
            }
            report.Info(report.Problems == 0 ? "No problems found." : $"{report.Problems} problems found.");
            return report;
        }

        private static bool SameLayers(List<EcLandLayer> a, List<EcLandLayer> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].Blend != b[i].Blend || a[i].Z != b[i].Z || a[i].LandId != b[i].LandId)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Reads a land table: lines "original replacement" (decimal or 0x hex), # = comment.</summary>
        public static Dictionary<ushort, ushort> ReadLandTable(string path, MapReport report = null)
        {
            var table = new Dictionary<ushort, ushort>();
            int line = 0;
            foreach (string raw in File.ReadAllLines(path))
            {
                line++;
                string text = raw.Trim();
                if (text.Length == 0 || text.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }
                string[] v = text.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (v.Length >= 2 && TryNum(v[0], out ushort a) && TryNum(v[1], out ushort b))
                {
                    table[a] = b;
                }
                else
                {
                    report?.Problem($"{Path.GetFileName(path)} line {line} can't be read.");
                }
            }
            return table;
        }

        private static bool TryNum(string text, out ushort value)
        {
            return text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? ushort.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)
                : ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }
}
