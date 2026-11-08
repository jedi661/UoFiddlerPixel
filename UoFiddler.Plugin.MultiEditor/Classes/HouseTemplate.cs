/***************************************************************************
 *
 * MultiEditor – house template learned from a well built example house.
 *
 * Instead of guessing where walls belong, the exact layout of the example
 * is learned relative to its floor:
 *   - floor ids, floor height (distance between levels)
 *   - wall line offsets (north / south / west / east) to the floor box
 *   - the 4 corner stacks and the 4 side sequences (with repeating
 *     pattern, e.g. wall - wall - window) per level
 *   - roof: type, start Z, step, overhang and the ids per row
 * A new house of any size is then built with the same rules.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// Tiles of one cell of one level: [id, dz]
    /// </summary>
    public sealed class TileStack
    {
        public List<int[]> Tiles { get; set; } = new List<int[]>();

        public bool IsEmpty => Tiles.Count == 0;

        public string Key => string.Join("+", Tiles.OrderBy(t => t[1]).ThenBy(t => t[0]).Select(t => $"{t[0]}@{t[1]}"));

        public override string ToString() => IsEmpty ? "-" : string.Join("+", Tiles.OrderBy(t => t[1]).Select(t => $"0x{t[0]:X4}@{t[1]}"));

        public static TileStack Parse(string text)
        {
            var s = new TileStack();
            if (string.IsNullOrWhiteSpace(text) || text.Trim() == "-")
            {
                return s;
            }

            foreach (string part in text.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] p = part.Trim().Split('@');
                if (!TryId(p[0], out int id))
                {
                    continue;
                }

                int dz = 0;
                if (p.Length > 1)
                {
                    int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out dz);
                }

                s.Tiles.Add(new[] { id, dz });
            }

            return s;
        }

        internal static bool TryId(string t, out int id)
        {
            t = t.Trim();
            return t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? int.TryParse(t.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id)
                : int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }
    }

    /// <summary>
    /// One side of a level: first stack, repeating pattern, last stack
    /// </summary>
    public sealed class SideTemplate
    {
        public TileStack First { get; set; } = new TileStack();
        public List<TileStack> Pattern { get; set; } = new List<TileStack>();
        public TileStack Last { get; set; } = new TileStack();

        /// <summary>
        /// Most used stack (plain wall without windows)
        /// </summary>
        public TileStack Plain { get; set; } = new TileStack();

        public List<TileStack> Build(int length, bool withPattern)
        {
            var list = new List<TileStack>();
            List<TileStack> pattern = withPattern && Pattern.Count > 0 ? Pattern : new List<TileStack> { Plain };
            for (int i = 0; i < length; ++i)
            {
                list.Add(pattern[i % pattern.Count]);
            }

            if (length > 0 && withPattern && !First.IsEmpty)
            {
                list[0] = First;
            }

            if (length > 1 && withPattern && !Last.IsEmpty)
            {
                list[length - 1] = Last;
            }

            return list;
        }

        public string PatternText
        {
            get => string.Join(" | ", Pattern.Select(p => p.ToString()));
            set => Pattern = (value ?? string.Empty).Split('|').Select(TileStack.Parse).Where(s => !s.IsEmpty).ToList();
        }
    }

    public sealed class LevelTemplate
    {
        public TileStack CornerNW { get; set; } = new TileStack();
        public TileStack CornerNE { get; set; } = new TileStack();
        public TileStack CornerSW { get; set; } = new TileStack();
        public TileStack CornerSE { get; set; } = new TileStack();
        public SideTemplate North { get; set; } = new SideTemplate();
        public SideTemplate South { get; set; } = new SideTemplate();
        public SideTemplate West { get; set; } = new SideTemplate();
        public SideTemplate East { get; set; } = new SideTemplate();

        public IEnumerable<int> AllIds()
        {
            IEnumerable<TileStack> stacks = new[] { CornerNW, CornerNE, CornerSW, CornerSE }
                .Concat(new[] { North, South, West, East }.SelectMany(s => s.Pattern.Concat(new[] { s.First, s.Last, s.Plain })));
            return stacks.SelectMany(s => s.Tiles.Select(t => t[0])).Distinct();
        }
    }

    public sealed class HouseTemplate
    {
        public int FloorId { get; set; }
        public int UpperFloorId { get; set; }
        public int FloorHeight { get; set; } = 20;

        /// <summary>
        /// Wall line minus floor box edge (e.g. -1 = wall one cell outside the floor)
        /// </summary>
        public int OffsetNorth { get; set; }
        public int OffsetSouth { get; set; }
        public int OffsetWest { get; set; }
        public int OffsetEast { get; set; }

        /// <summary>
        /// Floor covers the cells of the wall lines too
        /// </summary>
        public bool FloorUnderWalls { get; set; }

        public List<LevelTemplate> Levels { get; set; } = new List<LevelTemplate>();

        public RoofType Roof { get; set; } = RoofType.None;

        /// <summary>
        /// Roof start Z above the top of the last level
        /// </summary>
        public int RoofDz { get; set; }

        public int RoofStep { get; set; } = 3;

        /// <summary>
        /// Roof edge minus wall line (negative north/west = overhang)
        /// </summary>
        public int RoofOverNorth { get; set; }
        public int RoofOverSouth { get; set; }
        public int RoofOverWest { get; set; }
        public int RoofOverEast { get; set; }

        /// <summary>
        /// Ids per row from the first edge (north / west) to the ridge
        /// </summary>
        public List<int> RoofRowsA { get; set; } = new List<int>();

        /// <summary>
        /// Ids per row from the second edge (south / east) to the ridge
        /// </summary>
        public List<int> RoofRowsB { get; set; } = new List<int>();

        public int RoofRidge { get; set; }

        /// <summary>
        /// Flat roof id (0 = upper floor id)
        /// </summary>
        public int FlatRoofId { get; set; }

        /// <summary>
        /// Example in cell coordinates: [id, x, y, z]
        /// </summary>
        public List<int[]> Example { get; set; } = new List<int[]>();

        /// <summary>
        /// Wall lines of the example (for showing what was learned)
        /// </summary>
        public int ExampleNorth { get; set; }
        public int ExampleSouth { get; set; }
        public int ExampleWest { get; set; }
        public int ExampleEast { get; set; }

        public IEnumerable<int> WallIds() => Levels.SelectMany(l => l.AllIds()).Distinct();

        public static bool IsFloorTile(int id)
        {
            if (id <= 1 || id >= TileData.ItemTable.Length)
            {
                return false;
            }

            ItemData d = TileData.ItemTable[id];
            return (d.Flags & TileFlag.Surface) != 0 &&
                   (d.Flags & (TileFlag.Wall | TileFlag.Roof | TileFlag.StairBack | TileFlag.StairRight | TileFlag.Impassable)) == 0 &&
                   d.Height <= 3;
        }

        private static bool IsRoof(int id) => id < TileData.ItemTable.Length && (TileData.ItemTable[id].Flags & TileFlag.Roof) != 0;

        private static int MostCommon(IEnumerable<int> values)
        {
            return values.GroupBy(v => v).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
        }

        /// <summary>
        /// Learns a template, null if the example has no clear floor / walls
        /// </summary>
        public static HouseTemplate Learn(IList<ImportTile> source)
        {
            List<ImportTile> tiles = source.Where(t => t.Id > 1 && t.Id < TileData.ItemTable.Length && !t.Deco).ToList();
            if (tiles.Count < 8)
            {
                return null;
            }

            int ox = tiles.Min(t => t.X);
            int oy = tiles.Min(t => t.Y);
            tiles = tiles.Select(t => new ImportTile { Id = t.Id, X = t.X - ox, Y = t.Y - oy, Z = t.Z, Hue = t.Hue }).ToList();

            List<ImportTile> floors = tiles.Where(t => IsFloorTile(t.Id)).ToList();
            if (floors.Count < 4)
            {
                return null;
            }

            var byZ = floors.GroupBy(t => t.Z).Select(g => (z: g.Key, count: g.Count())).OrderBy(g => g.z).ToList();
            int maxCount = byZ.Max(g => g.count);
            int l0 = byZ.First(g => g.count >= maxCount * 0.3).z;
            List<ImportTile> f0 = floors.Where(t => t.Z == l0).ToList();

            // floor box: rows / columns with at least half of the usual floor count (porches and steps are ignored)
            int maxCol = f0.GroupBy(t => t.X).Max(g => g.Count());
            int maxRow = f0.GroupBy(t => t.Y).Max(g => g.Count());
            List<int> cols = f0.GroupBy(t => t.X).Where(g => g.Count() >= maxCol * 0.5).Select(g => g.Key).ToList();
            List<int> rows = f0.GroupBy(t => t.Y).Where(g => g.Count() >= maxRow * 0.5).Select(g => g.Key).ToList();
            int minX = cols.Min(), maxX = cols.Max(), minY = rows.Min(), maxY = rows.Max();

            var levelZ = byZ.Where(g => g.z >= l0 && g.count >= f0.Count * 0.3).Select(g => g.z).ToList();
            var t = new HouseTemplate
            {
                FloorId = MostCommon(f0.Select(f => (int)f.Id)),
                FloorHeight = levelZ.Count > 1 ? Math.Max(5, levelZ[1] - levelZ[0]) : 20
            };
            t.UpperFloorId = levelZ.Count > 1 ? MostCommon(floors.Where(f => f.Z == levelZ[1]).Select(f => (int)f.Id)) : t.FloorId;

            bool IsWall(ImportTile w) => (TileData.ItemTable[w.Id].Flags & (TileFlag.Wall | TileFlag.Window)) != 0 && !IsRoof(w.Id);

            // wall lines of the ground level
            List<ImportTile> walls0 = tiles.Where(w => IsWall(w) && w.Z >= l0 - 3 && w.Z < l0 + t.FloorHeight).ToList();
            if (walls0.Count < 4)
            {
                return null;
            }

            int width = maxX - minX + 1;
            int depth = maxY - minY + 1;

            int? FindLine(Func<ImportTile, int> axis, Func<ImportTile, int> other, int from, int to, int lo, int hi, int need)
            {
                int step = from <= to ? 1 : -1;
                for (int v = from; v != to + step; v += step)
                {
                    int count = walls0.Where(w => axis(w) == v && other(w) >= lo - 2 && other(w) <= hi + 2).Select(other).Distinct().Count();
                    if (count >= need)
                    {
                        return v;
                    }
                }

                return null;
            }

            int? north = FindLine(w => w.Y, w => w.X, minY - 2, minY + 2, minX, maxX, Math.Max(2, width / 2));
            int? south = FindLine(w => w.Y, w => w.X, maxY + 2, maxY - 2, minX, maxX, Math.Max(2, width / 2));
            int? west = FindLine(w => w.X, w => w.Y, minX - 2, minX + 2, minY, maxY, Math.Max(2, depth / 2));
            int? east = FindLine(w => w.X, w => w.Y, maxX + 2, maxX - 2, minY, maxY, Math.Max(2, depth / 2));
            if (north == null || south == null || west == null || east == null || south <= north || east <= west)
            {
                return null;
            }

            t.OffsetNorth = north.Value - minY;
            t.OffsetSouth = south.Value - maxY;
            t.OffsetWest = west.Value - minX;
            t.OffsetEast = east.Value - maxX;
            t.FloorUnderWalls = f0.Any(f => f.Y == north.Value && f.X > west.Value && f.X < east.Value);

            // levels
            int levels = Math.Max(1, levelZ.Count);
            for (int i = 0; i < levels; ++i)
            {
                int lz = l0 + (i * t.FloorHeight);
                LevelTemplate lt = LearnLevel(tiles, lz, t.FloorHeight, north.Value, south.Value, west.Value, east.Value);
                if (lt.AllIds().Any())
                {
                    t.Levels.Add(lt);
                }
            }

            if (t.Levels.Count == 0)
            {
                return null;
            }

            // a floor level without walls on top is the (flat) roof, not an own floor
            LearnRoof(t, tiles, l0 + (t.Levels.Count * t.FloorHeight), north.Value, south.Value, west.Value, east.Value);
            if (levelZ.Count > t.Levels.Count && t.Levels.Count == 1)
            {
                t.UpperFloorId = t.FloorId;
            }

            t.Example = tiles.Take(8000).Select(e => new[] { (int)e.Id, e.X, e.Y, e.Z }).ToList();
            t.ExampleNorth = north.Value;
            t.ExampleSouth = south.Value;
            t.ExampleWest = west.Value;
            t.ExampleEast = east.Value;
            return t;
        }

        private static TileStack StackAt(List<ImportTile> tiles, int x, int y, int lz, int height)
        {
            var s = new TileStack();
            foreach (ImportTile e in tiles.Where(e => e.X == x && e.Y == y && e.Z >= lz - 2 && e.Z < lz + height - 2))
            {
                if (IsFloorTile(e.Id) || IsRoof(e.Id))
                {
                    continue;
                }

                s.Tiles.Add(new[] { (int)e.Id, e.Z - lz });
            }

            return s;
        }

        private static LevelTemplate LearnLevel(List<ImportTile> tiles, int lz, int height, int n, int s, int w, int e)
        {
            var lt = new LevelTemplate
            {
                CornerNW = StackAt(tiles, w, n, lz, height),
                CornerNE = StackAt(tiles, e, n, lz, height),
                CornerSW = StackAt(tiles, w, s, lz, height),
                CornerSE = StackAt(tiles, e, s, lz, height),
                North = LearnSide(Enumerable.Range(w + 1, Math.Max(0, e - w - 1)).Select(x => StackAt(tiles, x, n, lz, height)).ToList()),
                South = LearnSide(Enumerable.Range(w + 1, Math.Max(0, e - w - 1)).Select(x => StackAt(tiles, x, s, lz, height)).ToList()),
                West = LearnSide(Enumerable.Range(n + 1, Math.Max(0, s - n - 1)).Select(y => StackAt(tiles, w, y, lz, height)).ToList()),
                East = LearnSide(Enumerable.Range(n + 1, Math.Max(0, s - n - 1)).Select(y => StackAt(tiles, e, y, lz, height)).ToList())
            };
            return lt;
        }

        private static SideTemplate LearnSide(List<TileStack> seq)
        {
            var side = new SideTemplate();
            if (seq.Count == 0)
            {
                return side;
            }

            side.Plain = seq.Where(st => !st.IsEmpty).GroupBy(st => st.Key).OrderByDescending(g => g.Count()).Select(g => g.First()).FirstOrDefault() ?? new TileStack();

            // repeating pattern over the whole side (empty cells like doors don't count)
            int bestPeriod = 1;
            double bestScore = double.MaxValue;
            for (int p = 1; p <= Math.Max(1, Math.Min(6, seq.Count / 2)); ++p)
            {
                int mismatch = 0;
                int compared = 0;
                for (int i = 0; i + p < seq.Count; ++i)
                {
                    if (seq[i].IsEmpty || seq[i + p].IsEmpty)
                    {
                        continue;
                    }

                    ++compared;
                    if (seq[i].Key != seq[i + p].Key)
                    {
                        ++mismatch;
                    }
                }

                double score = (compared == 0 ? 1 : (double)mismatch / compared) + (p * 0.02);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestPeriod = p;
                }
            }

            for (int i = 0; i < bestPeriod; ++i)
            {
                // most common stack of this phase (first and last cell only count if nothing else is there)
                List<TileStack> phase = seq.Where((st, idx) => idx % bestPeriod == i && !st.IsEmpty).ToList();
                List<TileStack> inner = seq.Where((st, idx) => idx % bestPeriod == i && !st.IsEmpty && idx > 0 && idx < seq.Count - 1).ToList();
                List<TileStack> use = inner.Count > 0 ? inner : phase;
                TileStack stack = use.GroupBy(st => st.Key).OrderByDescending(g => g.Count()).Select(g => g.First()).FirstOrDefault();
                side.Pattern.Add(stack ?? side.Plain);
            }

            // first / last cell only when they differ from the pattern (e.g. an end piece)
            if (!seq[0].IsEmpty && seq[0].Key != side.Pattern[0].Key)
            {
                side.First = seq[0];
            }

            TileStack lastExpected = side.Pattern[(seq.Count - 1) % side.Pattern.Count];
            if (seq.Count > 1 && !seq[seq.Count - 1].IsEmpty && seq[seq.Count - 1].Key != lastExpected.Key)
            {
                side.Last = seq[seq.Count - 1];
            }

            return side;
        }

        private static void LearnRoof(HouseTemplate t, List<ImportTile> tiles, int topZ, int n, int s, int w, int e)
        {
            List<ImportTile> roof = tiles.Where(r => IsRoof(r.Id)).ToList();
            if (roof.Count == 0)
            {
                // flat roof = floor tiles on top
                List<ImportTile> flat = tiles.Where(f => IsFloorTile(f.Id) && f.Z >= topZ - 2).ToList();
                if (flat.Count > 0)
                {
                    t.Roof = RoofType.Flat;
                    t.FlatRoofId = MostCommon(flat.Select(f => (int)f.Id));
                    t.RoofDz = flat.Min(f => f.Z) - topZ;
                }

                return;
            }

            t.RoofOverNorth = roof.Min(r => r.Y) - n;
            t.RoofOverSouth = roof.Max(r => r.Y) - s;
            t.RoofOverWest = roof.Min(r => r.X) - w;
            t.RoofOverEast = roof.Max(r => r.X) - e;
            t.RoofDz = roof.Min(r => r.Z) - topZ;

            // does the height change from row to row (ridge along X) or from column to column (ridge along Y)?
            double SpreadBy(Func<ImportTile, int> key)
            {
                var avg = roof.GroupBy(key).Select(g => g.Average(r => r.Z)).ToList();
                return avg.Count < 2 ? 0 : avg.Max() - avg.Min();
            }

            bool alongX = SpreadBy(r => r.Y) >= SpreadBy(r => r.X);
            if (SpreadBy(r => r.Y) < 2 && SpreadBy(r => r.X) < 2)
            {
                t.Roof = RoofType.Flat;
                t.FlatRoofId = MostCommon(roof.Select(r => (int)r.Id));
                return;
            }

            t.Roof = alongX ? RoofType.GableX : RoofType.GableY;
            Func<ImportTile, int> rowKey = alongX ? r => r.Y : r => r.X;

            // per row: lowest tile (the visible roof surface) id and z
            var rowsInfo = roof.GroupBy(rowKey).OrderBy(g => g.Key)
                .Select(g =>
                {
                    int z = g.Min(r => r.Z);
                    return (row: g.Key, z, id: MostCommon(g.Where(r => r.Z <= z + 2).Select(r => (int)r.Id)));
                }).ToList();

            int maxZ = rowsInfo.Max(r => r.z);
            int ridgeIdx = rowsInfo.FindIndex(r => r.z == maxZ);
            int ridgeLast = rowsInfo.FindLastIndex(r => r.z == maxZ);
            t.RoofRowsA = rowsInfo.Take(ridgeIdx).Select(r => r.id).ToList();
            t.RoofRowsB = rowsInfo.Skip(ridgeLast + 1).Reverse().Select(r => r.id).ToList();
            t.RoofRidge = rowsInfo[ridgeIdx].id;

            var steps = new List<int>();
            for (int i = 1; i <= ridgeIdx; ++i)
            {
                steps.Add(rowsInfo[i].z - rowsInfo[i - 1].z);
            }

            steps = steps.Where(v => v > 0).OrderBy(v => v).ToList();
            t.RoofStep = steps.Count > 0 ? steps[steps.Count / 2] : 3;
        }

        /// <summary>
        /// Builds a house: floor box 0..width-1 / 0..depth-1. Tiles may have negative coordinates (caller normalizes).
        /// </summary>
        public List<ImportTile> Generate(HouseGeneratorOptions o)
        {
            int w = Math.Max(1, o.Width);
            int d = Math.Max(1, o.Depth);
            int north = OffsetNorth;
            int south = d - 1 + OffsetSouth;
            int west = OffsetWest;
            int east = w - 1 + OffsetEast;
            var tiles = new List<ImportTile>();

            for (int f = 0; f < Math.Max(1, o.Floors); ++f)
            {
                int lz = o.BaseZ + (f * FloorHeight);
                int floorId = f == 0 ? FloorId : (UpperFloorId > 0 ? UpperFloorId : FloorId);
                AddFloor(tiles, floorId, 0, 0, w - 1, d - 1, lz);
                tiles.AddRange(BuildWalls(Levels[Math.Min(f, Levels.Count - 1)], north, south, west, east, lz, o.Windows, f == 0 && o.DoorGap, o.InnerWall));
            }

            int top = o.BaseZ + (Math.Max(1, o.Floors) * FloorHeight);
            RoofType roof = o.Roof == RoofType.FromTemplate ? Roof : o.Roof;
            tiles.AddRange(BuildRoof(roof, north, south, west, east, top));
            return tiles;
        }

        public static void AddFloor(List<ImportTile> tiles, int id, int x0, int y0, int x1, int y1, int z)
        {
            if (id <= 0)
            {
                return;
            }

            for (int x = x0; x <= x1; ++x)
            {
                for (int y = y0; y <= y1; ++y)
                {
                    tiles.Add(new ImportTile { Id = (ushort)id, X = x, Y = y, Z = z });
                }
            }
        }

        private static void AddStack(List<ImportTile> tiles, TileStack s, int x, int y, int z)
        {
            foreach (int[] t in s.Tiles)
            {
                tiles.Add(new ImportTile { Id = (ushort)t[0], X = x, Y = y, Z = z + t[1] });
            }
        }

        /// <summary>
        /// Walls of one level on the given wall lines
        /// </summary>
        public List<ImportTile> BuildWalls(LevelTemplate lt, int north, int south, int west, int east, int z, bool windows, bool doorGap, bool innerWall)
        {
            var tiles = new List<ImportTile>();
            AddStack(tiles, lt.CornerNW, west, north, z);
            AddStack(tiles, lt.CornerNE, east, north, z);
            AddStack(tiles, lt.CornerSW, west, south, z);
            AddStack(tiles, lt.CornerSE, east, south, z);

            int lenX = east - west - 1;
            int lenY = south - north - 1;
            List<TileStack> n = lt.North.Build(lenX, windows);
            List<TileStack> s = lt.South.Build(lenX, windows);
            List<TileStack> wst = lt.West.Build(lenY, windows);
            List<TileStack> est = lt.East.Build(lenY, windows);
            int door = lenX / 2;
            for (int i = 0; i < lenX; ++i)
            {
                AddStack(tiles, n[i], west + 1 + i, north, z);
                if (!(doorGap && i == door))
                {
                    AddStack(tiles, s[i], west + 1 + i, south, z);
                }
            }

            for (int i = 0; i < lenY; ++i)
            {
                AddStack(tiles, wst[i], west, north + 1 + i, z);
                AddStack(tiles, est[i], east, north + 1 + i, z);
            }

            if (innerWall && lenX >= 5)
            {
                int ix = west + 1 + (lenX / 2);
                List<TileStack> inner = lt.West.Build(lenY, false);
                for (int i = 0; i < lenY; ++i)
                {
                    if (i != lenY / 2)
                    {
                        AddStack(tiles, inner[i], ix, north + 1 + i, z);
                    }
                }
            }

            return tiles;
        }

        public List<ImportTile> BuildRoof(RoofType roof, int north, int south, int west, int east, int topZ)
        {
            var tiles = new List<ImportTile>();
            int x0 = west + RoofOverWest;
            int x1 = east + RoofOverEast;
            int y0 = north + RoofOverNorth;
            int y1 = south + RoofOverSouth;
            int z0 = topZ + RoofDz;

            switch (roof)
            {
                case RoofType.Flat:
                    {
                        int id = FlatRoofId > 0 ? FlatRoofId : (UpperFloorId > 0 ? UpperFloorId : FloorId);
                        AddFloor(tiles, id, Math.Min(west, x0), Math.Min(north, y0), Math.Max(east, x1), Math.Max(south, y1), Roof == RoofType.Flat ? z0 : topZ);
                        break;
                    }
                case RoofType.GableX:
                case RoofType.GableY:
                    {
                        if (RoofRowsA.Count == 0 && RoofRowsB.Count == 0 && RoofRidge == 0)
                        {
                            break;
                        }

                        bool alongX = roof == RoofType.GableX;
                        int rows = alongX ? y1 - y0 + 1 : x1 - x0 + 1;

                        // learned in the other direction: turn the roof ids (north <-> west, south <-> east)
                        Dictionary<ushort, ushort> turn = Roof is RoofType.GableX or RoofType.GableY && Roof != roof
                            ? StyleKnowledge.Instance.GetRotationSwapMap()
                            : null;
                        int Turn(int id) => turn != null && turn.TryGetValue((ushort)id, out ushort n) ? n : id;
                        for (int r = 0; r < rows; ++r)
                        {
                            int distA = r;
                            int distB = rows - 1 - r;
                            int dist = Math.Min(distA, distB);
                            int id;
                            if (distA < distB)
                            {
                                id = RoofRowsA.Count > 0 ? RoofRowsA[Math.Min(distA, RoofRowsA.Count - 1)] : RoofRidge;
                            }
                            else if (distB < distA)
                            {
                                id = RoofRowsB.Count > 0 ? RoofRowsB[Math.Min(distB, RoofRowsB.Count - 1)] : RoofRidge;
                            }
                            else
                            {
                                id = RoofRidge > 0 ? RoofRidge : (RoofRowsA.Count > 0 ? RoofRowsA[RoofRowsA.Count - 1] : 0);
                            }

                            if (id <= 0)
                            {
                                continue;
                            }

                            id = Turn(id);
                            int z = z0 + (dist * RoofStep);
                            if (alongX)
                            {
                                for (int x = x0; x <= x1; ++x)
                                {
                                    tiles.Add(new ImportTile { Id = (ushort)id, X = x, Y = y0 + r, Z = z });
                                }
                            }
                            else
                            {
                                for (int y = y0; y <= y1; ++y)
                                {
                                    tiles.Add(new ImportTile { Id = (ushort)id, X = x0 + r, Y = y, Z = z });
                                }
                            }
                        }

                        break;
                    }
            }

            return tiles;
        }

        /// <summary>
        /// Walls only (ground level template) on a rectangle of wall lines – for the rectangle tool
        /// </summary>
        public List<ImportTile> BuildWallRectangle(int x0, int y0, int x1, int y1, int z, bool windows)
        {
            if (Levels.Count == 0)
            {
                return new List<ImportTile>();
            }

            return BuildWalls(Levels[0], Math.Min(y0, y1), Math.Max(y0, y1), Math.Min(x0, x1), Math.Max(x0, x1), z, windows, false, false);
        }
    }
}
