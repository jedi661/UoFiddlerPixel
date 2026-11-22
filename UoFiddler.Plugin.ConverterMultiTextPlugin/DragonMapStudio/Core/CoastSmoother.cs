/***************************************************************************
 *
 *  * Dragon Map Studio – coast without saw teeth: soft shore profile
 *  *
 *  *  A diagonal or ragged coast in the colour map gives a staircase of land / water cells. With the classic profile
 *  *  (beach -3, sea floor -15 right next to it) every land tile at the water gets one corner pulled down to -15 and the
 *  *  client draws a row of sand teeth. Here the height follows the real (Euclidean, softened) distance to the shore
 *  *  line instead of the cell steps: the beach sinks smoothly to the water surface and the sea floor drops over a few
 *  *  tiles to its depth – the depth stays, it only moves with the coast.
 *  *
 *  *  Optional: the shore line itself is straightened (single land spikes and 1-tile notches are removed), the statics
 *  *  go with the ground (water statics stay on the surface, new water gets water statics).
 *  *  Works for a brush, a marked area, the whole map or only where errors are found; also used by the compiler.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>Settings of "Küste glätten" (CGI view, tab Küste / Wasser).</summary>
    public sealed class CoastSmoothSettings
    {
        [Category("Küste glätten"), DisplayName("Abfall-Breite (Tiles)"),
         Description("Auf so vielen Tiles fällt der Meeresgrund von der Wasserlinie bis zur vollen Tiefe ('Meeresgrund z'). Größer = flacher, weicher. Standard 3.")]
        public double DropWidth { get; set; } = 3;

        [Category("Küste glätten"), DisplayName("Weichheit (Durchgänge)"),
         Description("So oft wird der Abstand zur Küste geglättet: 0 = genau den Zellen folgen, 1 = keine Zacken an schrägen Küsten (Standard), 2 = noch runder.")]
        public int BlurPasses { get; set; } = 1;

        [Category("Küste glätten"), DisplayName("Küstenlinie begradigen (Durchgänge)"),
         Description("Entfernt einzelne Land-Spitzen und 1-Tile-Buchten (Ecken und Kanten). 0 = Land / Wasser bleibt genau wie es ist, nur die Höhen werden weich.")]
        public int OutlinePasses { get; set; } = 1;

        [Category("Küste glätten"), DisplayName("Statics mitnehmen"),
         Description("Statics auf geändertem Land gehen mit dem Boden mit; Wasser-Statics bleiben auf der Wasserhöhe, neues Wasser bekommt Wasser-Statics, neues Land verliert sie.")]
        public bool MoveStatics { get; set; } = true;

        [Category("Fehler suchen"), DisplayName("Fehler: Höhensprung ab"),
         Description("Ein Strand-Tile, das zu einem Nachbarn im Wasser mindestens so tief abfällt, gilt als Zacke (klassisch: -3 → -15 = 12). Standard 7.")]
        public int ErrorDrop { get; set; } = 7;

        [Category("Fehler suchen"), DisplayName("Fehler: Umkreis (Tiles)"),
         Description("'Nur Fehlerstellen': so weit um jede gefundene Stelle wird geglättet. Standard 3.")]
        public int ErrorRadius { get; set; } = 3;

        public CoastSmoothSettings Copy() => (CoastSmoothSettings)MemberwiseClone();

        public override string ToString() => $"Abfall {DropWidth}, Weichheit {BlurPasses}, begradigen {OutlinePasses}";
    }

    /// <summary>Heights of the soft shore profile.</summary>
    public sealed class ShoreProfile
    {
        /// <summary>Beach heights at 1, 2, 3 … tiles from the water (e.g. -3 -1 0).</summary>
        public List<int> LandHeights { get; set; } = new List<int> { -3, -1, 0 };

        /// <summary>Land higher than this stays (cliffs).</summary>
        public int MaxLandZ { get; set; } = 10;

        public int SurfaceZ { get; set; } = -5;
        public int FloorZ { get; set; } = -15;

        /// <summary>Shore water band in tiles (8-neighbour distance to land); the outermost ring lies at the surface.</summary>
        public int WaterWidth { get; set; } = 6;

        public double DropWidth { get; set; } = 3;
        public int BlurPasses { get; set; } = 1;

        public static ShoreProfile From(CoastSettings c, CoastSmoothSettings s) => new ShoreProfile
        {
            LandHeights = c.Heights(),
            MaxLandZ = c.MaxLandZ,
            SurfaceZ = c.SurfaceZ,
            FloorZ = c.FloorZ,
            WaterWidth = c.WaterWidth,
            DropWidth = Math.Max(0.5, s?.DropWidth ?? 3),
            BlurPasses = Math.Clamp(s?.BlurPasses ?? 1, 0, 4)
        };

        /// <summary>Search radius of the distances.</summary>
        internal int Radius => (int)Math.Ceiling(Math.Max(DropWidth, LandHeights.Count + 0.5)) + 1;

        internal int Margin => Math.Max(Radius, Math.Max(0, WaterWidth)) + BlurPasses + 2;

        /// <summary>Land height at a (signed, positive) distance to the shore line.</summary>
        internal double Land(double sd)
        {
            if (LandHeights.Count == 0)
            {
                return SurfaceZ;
            }
            if (sd <= 0.5)
            {
                return SurfaceZ + (LandHeights[0] - SurfaceZ) * Math.Max(0, sd) / 0.5;
            }
            double k = sd - 0.5;
            int i = (int)Math.Floor(k);
            if (i >= LandHeights.Count - 1)
            {
                return LandHeights[LandHeights.Count - 1];
            }
            return LandHeights[i] + (LandHeights[i + 1] - LandHeights[i]) * (k - i);
        }

        /// <summary>Sea floor height at a (signed, negative) distance to the shore line.</summary>
        internal double Water(double sd)
        {
            double t = Math.Min(1, -sd / Math.Max(0.5, DropWidth));
            return SurfaceZ + (FloorZ - SurfaceZ) * t;
        }
    }

    public static class CoastSmoother
    {
        private const int Chunk = 128;

        /// <summary>
        /// Soft shore heights for the cells of <paramref name="area"/> (only where <paramref name="allowed"/> says yes).
        /// <paramref name="isWater"/> / <paramref name="getZ"/> read the map (also outside the area), the new heights go to
        /// <paramref name="setZ"/> (only cells whose height changes). Works in chunks, so whole maps stay small in memory.
        /// </summary>
        public static void Profile(int mapW, int mapH, Rectangle area, Func<int, int, bool> isWater, Func<int, int, int> getZ,
            Func<int, int, bool> allowed, ShoreProfile p, Action<int, int, int> setZ, Func<int, int, bool> chunkActive = null,
            IProgress<int> progress = null, CancellationToken cancel = default)
        {
            area.Intersect(new Rectangle(0, 0, mapW, mapH));
            if (area.IsEmpty)
            {
                return;
            }
            int margin = p.Margin, radius = p.Radius;
            int chunksX = (area.Width + Chunk - 1) / Chunk, chunksY = (area.Height + Chunk - 1) / Chunk, done = 0;
            // offsets within the search radius, nearest first: the first hit is the nearest cell of the other kind
            var offsets = new List<(int dx, int dy, int d2)>();
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if ((dx != 0 || dy != 0) && dx * dx + dy * dy <= radius * radius)
                    {
                        offsets.Add((dx, dy, dx * dx + dy * dy));
                    }
                }
            }
            offsets.Sort((a, b) => a.d2.CompareTo(b.d2));

            for (int cy = 0; cy < chunksY; cy++)
            {
                for (int cx = 0; cx < chunksX; cx++)
                {
                    cancel.ThrowIfCancellationRequested();
                    var core = new Rectangle(area.X + cx * Chunk, area.Y + cy * Chunk, Math.Min(Chunk, area.Right - (area.X + cx * Chunk)), Math.Min(Chunk, area.Bottom - (area.Y + cy * Chunk)));
                    done++;
                    if (chunkActive != null && !chunkActive(core.X / Chunk, core.Y / Chunk) && !ChunkTouched(core, chunkActive))
                    {
                        continue;
                    }
                    ProfileChunk(mapW, mapH, core, margin, isWater, getZ, allowed, p, setZ, offsets);
                    progress?.Report(done * 100 / Math.Max(1, chunksX * chunksY));
                }
            }
        }

        private static bool ChunkTouched(Rectangle core, Func<int, int, bool> chunkActive)
        {
            // the core of an area chunk can lie across two map chunks
            return chunkActive((core.Right - 1) / Chunk, core.Y / Chunk) || chunkActive(core.X / Chunk, (core.Bottom - 1) / Chunk) ||
                   chunkActive((core.Right - 1) / Chunk, (core.Bottom - 1) / Chunk);
        }

        private static void ProfileChunk(int mapW, int mapH, Rectangle core, int margin, Func<int, int, bool> isWater, Func<int, int, int> getZ,
            Func<int, int, bool> allowed, ShoreProfile p, Action<int, int, int> setZ, List<(int dx, int dy, int d2)> offsets)
        {
            Rectangle win = Rectangle.Inflate(core, margin, margin);
            win.Intersect(new Rectangle(0, 0, mapW, mapH));
            int w = win.Width, h = win.Height, n = w * h;
            var water = new bool[n];
            bool anyWater = false, anyLand = false;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool v = isWater(win.X + x, win.Y + y);
                    water[y * w + x] = v;
                    anyWater |= v;
                    anyLand |= !v;
                }
            }
            if (!anyWater || !anyLand)
            {
                return; // no coast here
            }
            int band = Math.Max(p.Radius, p.WaterWidth) + 1;
            int[] toOther = ChessDistances(water, w, h, band, null);
            int[] toLand = ChessDistances(water, w, h, band, false);
            int radius = p.Radius;

            // signed Euclidean distance to the shore line (cell centre to the nearest cell of the other kind, minus ½)
            var sd = new double[n];
            for (int i = 0; i < n; i++)
            {
                int x = i % w, y = i / w;
                bool wet = water[i];
                double d = radius + 0.5;
                if (toOther[i] <= radius)
                {
                    foreach ((int dx, int dy, int d2) in offsets)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                        {
                            continue;
                        }
                        if (water[ny * w + nx] != wet)
                        {
                            d = Math.Sqrt(d2) - 0.5;
                            break;
                        }
                    }
                }
                sd[i] = wet ? -d : d;
            }
            for (int pass = 0; pass < p.BlurPasses; pass++)
            {
                var next = new double[n];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        double sum = 0, weight = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                                {
                                    continue;
                                }
                                double k = dx == 0 && dy == 0 ? 2 : 1;
                                sum += sd[ny * w + nx] * k;
                                weight += k;
                            }
                        }
                        next[y * w + x] = sum / weight;
                    }
                }
                sd = next;
            }

            for (int y = core.Top; y < core.Bottom; y++)
            {
                for (int x = core.Left; x < core.Right; x++)
                {
                    if (allowed != null && !allowed(x, y))
                    {
                        continue;
                    }
                    int i = (y - win.Y) * w + (x - win.X);
                    int z = getZ(x, y);
                    int nz;
                    if (water[i])
                    {
                        int dl = toLand[i];
                        if (p.WaterWidth <= 0 || dl < 1 || dl > p.WaterWidth)
                        {
                            continue; // open water stays
                        }
                        if (dl == p.WaterWidth)
                        {
                            nz = p.SurfaceZ; // outer ring joins the open water
                        }
                        else
                        {
                            double s = Math.Min(-0.05, sd[i]);
                            nz = (int)Math.Round(p.Water(s), MidpointRounding.AwayFromZero);
                            nz = Math.Clamp(nz, Math.Min(p.FloorZ, p.SurfaceZ - 1), p.SurfaceZ - 1);
                        }
                    }
                    else
                    {
                        if (z > p.MaxLandZ)
                        {
                            continue; // cliff coast stays
                        }
                        double s = Math.Max(0.05, sd[i]);
                        if (s >= p.LandHeights.Count + 0.5)
                        {
                            continue;
                        }
                        int t = (int)Math.Round(p.Land(s), MidpointRounding.AwayFromZero);
                        nz = s <= 0.75 || z < t ? t : t + (z - t) / 2;
                    }
                    nz = Math.Clamp(nz, -128, 127);
                    if (nz != z)
                    {
                        setZ(x, y, nz);
                    }
                }
            }
        }

        /// <summary>
        /// 8-neighbour distance (capped at <paramref name="max"/>+1) to the nearest cell of the other kind
        /// (<paramref name="toKind"/> null) or to the nearest land (false) / water (true) cell.
        /// </summary>
        private static int[] ChessDistances(bool[] water, int w, int h, int max, bool? toKind)
        {
            int n = w * h;
            var dist = new int[n];
            Array.Fill(dist, max + 1);
            var q = new Queue<int>();
            for (int i = 0; i < n; i++)
            {
                bool seed;
                if (toKind.HasValue)
                {
                    seed = water[i] == toKind.Value;
                }
                else
                {
                    // a cell next to the other kind has distance 1
                    seed = false;
                    int x = i % w, y = i / w;
                    for (int dy = -1; dy <= 1 && !seed; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx >= 0 && ny >= 0 && nx < w && ny < h && water[ny * w + nx] != water[i])
                            {
                                seed = true;
                                break;
                            }
                        }
                    }
                    if (seed)
                    {
                        dist[i] = 1;
                        q.Enqueue(i);
                    }
                    continue;
                }
                if (seed)
                {
                    dist[i] = 0;
                    q.Enqueue(i);
                }
            }
            while (q.Count > 0)
            {
                int i = q.Dequeue();
                if (dist[i] >= max)
                {
                    continue;
                }
                int x = i % w, y = i / w;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                        {
                            continue;
                        }
                        int j = ny * w + nx;
                        if (toKind == null && water[j] != water[i])
                        {
                            continue; // stay on the own side
                        }
                        if (dist[j] > dist[i] + 1)
                        {
                            dist[j] = dist[i] + 1;
                            q.Enqueue(j);
                        }
                    }
                }
            }
            return dist;
        }

        /// <summary>
        /// Straightens the shore line: a cell with at most 2 of 8 neighbours of its own kind (single spike, tip of a 1-tile
        /// strip, 1-tile notch) changes to the other kind. Returns the changed cells (true = becomes water).
        /// </summary>
        public static Dictionary<(int x, int y), bool> Straighten(int mapW, int mapH, Rectangle area, Func<int, int, bool> isWater,
            Func<int, int, bool> allowed, int passes, CancellationToken cancel = default)
        {
            var flips = new Dictionary<(int, int), bool>();
            area.Intersect(new Rectangle(0, 0, mapW, mapH));
            if (area.IsEmpty || passes <= 0)
            {
                return flips;
            }
            int margin = passes + 2;
            for (int cy = area.Top; cy < area.Bottom; cy += Chunk)
            {
                for (int cx = area.Left; cx < area.Right; cx += Chunk)
                {
                    cancel.ThrowIfCancellationRequested();
                    var core = Rectangle.FromLTRB(cx, cy, Math.Min(area.Right, cx + Chunk), Math.Min(area.Bottom, cy + Chunk));
                    Rectangle win = Rectangle.Inflate(core, margin, margin);
                    win.Intersect(new Rectangle(0, 0, mapW, mapH));
                    int w = win.Width, h = win.Height;
                    var water = new bool[w * h];
                    bool anyWater = false, anyLand = false;
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            bool v = isWater(win.X + x, win.Y + y);
                            water[y * w + x] = v;
                            anyWater |= v;
                            anyLand |= !v;
                        }
                    }
                    if (!anyWater || !anyLand)
                    {
                        continue;
                    }
                    var original = (bool[])water.Clone();
                    for (int pass = 0; pass < passes; pass++)
                    {
                        var next = (bool[])water.Clone();
                        for (int y = core.Top; y < core.Bottom; y++)
                        {
                            for (int x = core.Left; x < core.Right; x++)
                            {
                                if (allowed != null && !allowed(x, y))
                                {
                                    continue;
                                }
                                int lx = x - win.X, ly = y - win.Y, i = ly * w + lx;
                                int same = 0, other = 0;
                                for (int dy = -1; dy <= 1; dy++)
                                {
                                    for (int dx = -1; dx <= 1; dx++)
                                    {
                                        int nx = lx + dx, ny = ly + dy;
                                        if ((dx == 0 && dy == 0) || nx < 0 || ny < 0 || nx >= w || ny >= h)
                                        {
                                            continue;
                                        }
                                        if (water[ny * w + nx] == water[i])
                                        {
                                            same++;
                                        }
                                        else
                                        {
                                            other++;
                                        }
                                    }
                                }
                                if (same <= 2 && other >= 5)
                                {
                                    next[i] = !water[i];
                                }
                            }
                        }
                        water = next;
                    }
                    for (int y = core.Top; y < core.Bottom; y++)
                    {
                        for (int x = core.Left; x < core.Right; x++)
                        {
                            int i = (y - win.Y) * w + (x - win.X);
                            if (water[i] != original[i])
                            {
                                flips[(x, y)] = water[i];
                            }
                        }
                    }
                }
            }
            return flips;
        }

        // ================================================================== on a built map (CGI view)

        private sealed class WetCache
        {
            private readonly Dictionary<ushort, bool> _land = new Dictionary<ushort, bool>();
            private readonly Dictionary<ushort, bool> _item = new Dictionary<ushort, bool>();
            private readonly HashSet<ushort> _floor;

            public WetCache(HashSet<ushort> floor) => _floor = floor;

            public bool Land(ushort id)
            {
                if (!_land.TryGetValue(id, out bool v))
                {
                    _land[id] = v = _floor.Contains(id) || UoArtCache.LandIsWet(id);
                }
                return v;
            }

            public bool Item(ushort id)
            {
                if (!_item.TryGetValue(id, out bool v))
                {
                    _item[id] = v = UoArtCache.ItemIsWet(id);
                }
                return v;
            }
        }

        private static Func<int, int, bool> WaterTest(MapData map, WetCache wet)
        {
            return (x, y) =>
            {
                ushort id = map.LandId(x, y);
                if (wet.Land(id))
                {
                    return true;
                }
                int z = map.LandZ(x, y);
                foreach (StaticItem s in map.StaticsAt(x, y))
                {
                    if (s.Z > z && wet.Item(s.Id))
                    {
                        return true;
                    }
                }
                return false;
            };
        }

        /// <summary>
        /// "Küste glätten" on a built map: straighten the shore line (optional), soft heights, statics along.
        /// <paramref name="allowed"/> limits the changed cells (brush circle, error places), <paramref name="allowFlip"/>
        /// the cells whose kind (land / water) may change.
        /// </summary>
        public static CoastEdit Smooth(MapData map, Rectangle area, CoastSettings coast, CoastSmoothSettings o, Random rnd,
            Func<int, int, bool> allowed = null, Func<int, int, bool> allowFlip = null, Func<int, int, bool> chunkActive = null,
            IProgress<int> progress = null, CancellationToken cancel = default)
        {
            o ??= new CoastSmoothSettings();
            var edit = new CoastEdit();
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (area.IsEmpty)
            {
                return edit;
            }
            var floor = new HashSet<ushort>(HexList.Parse(coast.FloorTiles));
            ushort[] floorIds = floor.ToArray();
            ushort[] waterIds = HexList.Parse(coast.WaterStaticIds);
            var wet = new WetCache(floor);
            Func<int, int, bool> original = WaterTest(map, wet);

            // 1) shore line
            Dictionary<(int x, int y), bool> flips = o.OutlinePasses > 0
                ? Straighten(map.Width, map.Height, area, original,
                    (x, y) => (allowed == null || allowed(x, y)) && (allowFlip == null || allowFlip(x, y)), o.OutlinePasses, cancel)
                : new Dictionary<(int, int), bool>();
            bool IsWater(int x, int y) => flips.TryGetValue((x, y), out bool f) ? f : original(x, y);

            // 2) heights
            ShoreProfile p = ShoreProfile.From(coast, o);
            var newZ = new Dictionary<(int x, int y), int>();
            Profile(map.Width, map.Height, area, IsWater, map.LandZ, allowed, p, (x, y, z) => newZ[(x, y)] = z, chunkActive, progress, cancel);

            // 3) land + statics
            foreach ((int x, int y) in flips.Keys.Concat(newZ.Keys).Distinct())
            {
                ushort id = map.LandId(x, y);
                int z = map.LandZ(x, y);
                int nz = newZ.TryGetValue((x, y), out int v) ? v : z;
                ushort nid = id;
                bool flipped = flips.TryGetValue((x, y), out bool toWater);
                if (flipped && toWater && floorIds.Length > 0)
                {
                    nid = floorIds[rnd.Next(floorIds.Length)];
                }
                else if (flipped && !toWater)
                {
                    nid = CommonLandNeighbour(map, x, y, original, id);
                    if (!newZ.ContainsKey((x, y)))
                    {
                        nz = Math.Max(nz, coast.Heights().FirstOrDefault());
                    }
                }
                if (nid != id || nz != z)
                {
                    edit.Land.Add((x, y, nid, nz));
                }
                if (!o.MoveStatics)
                {
                    continue;
                }
                List<StaticItem> statics = map.StaticsAt(x, y).ToList();
                if (flipped && toWater)
                {
                    bool hasWater = false;
                    foreach (StaticItem s in statics)
                    {
                        if (wet.Item(s.Id))
                        {
                            hasWater = true;
                            if (s.Z != coast.SurfaceZ)
                            {
                                edit.Statics.Add(MapData.Modify(s, t => t.Z = coast.SurfaceZ));
                            }
                        }
                        else
                        {
                            edit.Statics.Add(MapData.Remove(s)); // the spike is gone: what stood on it too
                        }
                    }
                    if (!hasWater && waterIds.Length > 0)
                    {
                        edit.Statics.Add(MapData.Add(new StaticItem { Id = waterIds[rnd.Next(waterIds.Length)], X = x, Y = y, Z = coast.SurfaceZ }));
                    }
                    continue;
                }
                if (flipped)
                {
                    edit.Statics.AddRange(statics.Where(s => wet.Item(s.Id)).Select(MapData.Remove));
                    continue;
                }
                int dz = nz - z;
                if (dz == 0)
                {
                    continue;
                }
                bool cellWet = IsWater(x, y);
                foreach (StaticItem s in statics)
                {
                    // water and shore statics stay on the surface, the rest goes with the ground
                    int sz = wet.Item(s.Id) ? (cellWet || s.Z <= coast.SurfaceZ + 1 ? coast.SurfaceZ : s.Z) : Math.Clamp(s.Z + dz, -128, 127);
                    if (sz != s.Z)
                    {
                        edit.Statics.Add(MapData.Modify(s, t => t.Z = sz));
                    }
                }
            }
            return edit;
        }

        private static ushort CommonLandNeighbour(MapData map, int x, int y, Func<int, int, bool> isWater, ushort fallback)
        {
            var count = new Dictionary<ushort, int>();
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if ((dx != 0 || dy != 0) && map.InBounds(nx, ny) && !isWater(nx, ny))
                    {
                        ushort id = map.LandId(nx, ny);
                        count[id] = count.TryGetValue(id, out int c) ? c + 1 : 1;
                    }
                }
            }
            return count.Count == 0 ? fallback : count.OrderByDescending(kv => kv.Value).First().Key;
        }

        /// <summary>
        /// Coast errors: beach tiles that drop at least <see cref="CoastSmoothSettings.ErrorDrop"/> to a water neighbour
        /// (saw teeth), single land spikes and 1-tile notches.
        /// </summary>
        public static List<Point> FindErrors(MapData map, Rectangle area, CoastSettings coast, CoastSmoothSettings o, CancellationToken cancel = default)
        {
            o ??= new CoastSmoothSettings();
            var list = new List<Point>();
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            var wet = new WetCache(new HashSet<ushort>(HexList.Parse(coast.FloorTiles)));
            Func<int, int, bool> isWater = WaterTest(map, wet);
            for (int cy = area.Top; cy < area.Bottom; cy += Chunk)
            {
                for (int cx = area.Left; cx < area.Right; cx += Chunk)
                {
                    cancel.ThrowIfCancellationRequested();
                    var core = Rectangle.FromLTRB(cx, cy, Math.Min(area.Right, cx + Chunk), Math.Min(area.Bottom, cy + Chunk));
                    Rectangle win = Rectangle.Inflate(core, 1, 1);
                    win.Intersect(new Rectangle(0, 0, map.Width, map.Height));
                    int w = win.Width, h = win.Height;
                    var water = new bool[w * h];
                    bool anyWater = false, anyLand = false;
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            bool v = isWater(win.X + x, win.Y + y);
                            water[y * w + x] = v;
                            anyWater |= v;
                            anyLand |= !v;
                        }
                    }
                    if (!anyWater || !anyLand)
                    {
                        continue;
                    }
                    for (int y = core.Top; y < core.Bottom; y++)
                    {
                        for (int x = core.Left; x < core.Right; x++)
                        {
                            int i = (y - win.Y) * w + (x - win.X);
                            int same = 0, other = 0, z = map.LandZ(x, y);
                            bool tooth = false;
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                for (int dx = -1; dx <= 1; dx++)
                                {
                                    int nx = x + dx - win.X, ny = y + dy - win.Y;
                                    if ((dx == 0 && dy == 0) || nx < 0 || ny < 0 || nx >= w || ny >= h)
                                    {
                                        continue;
                                    }
                                    bool nw = water[ny * w + nx];
                                    if (nw == water[i])
                                    {
                                        same++;
                                        continue;
                                    }
                                    other++;
                                    if (!water[i] && z <= coast.MaxLandZ && z - map.LandZ(x + dx, y + dy) >= o.ErrorDrop)
                                    {
                                        tooth = true;
                                    }
                                }
                            }
                            if (tooth || (other > 0 && same <= 2 && other >= 5))
                            {
                                list.Add(new Point(x, y));
                            }
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>"Nur Fehlerstellen": finds the errors and smooths only around them (radius).</summary>
        public static CoastEdit SmoothErrors(MapData map, Rectangle area, CoastSettings coast, CoastSmoothSettings o, Random rnd,
            out int errors, IProgress<int> progress = null, CancellationToken cancel = default)
        {
            o ??= new CoastSmoothSettings();
            List<Point> found = FindErrors(map, area, coast, o, cancel);
            errors = found.Count;
            if (found.Count == 0)
            {
                return new CoastEdit();
            }
            int r = Math.Max(1, o.ErrorRadius);
            var cells = new HashSet<long>();
            var chunks = new HashSet<long>();
            foreach (Point e in found)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        int x = e.X + dx, y = e.Y + dy;
                        if (map.InBounds(x, y) && dx * dx + dy * dy <= r * r + r)
                        {
                            cells.Add((long)y * map.Width + x);
                            chunks.Add(((long)(y / Chunk) << 32) | (uint)(x / Chunk));
                        }
                    }
                }
            }
            Rectangle bounds = Rectangle.FromLTRB(found.Min(p => p.X) - r, found.Min(p => p.Y) - r, found.Max(p => p.X) + r + 1, found.Max(p => p.Y) + r + 1);
            bounds.Intersect(area);
            return Smooth(map, bounds, coast, o, rnd, (x, y) => cells.Contains((long)y * map.Width + x), null,
                (cx, cy) => chunks.Contains(((long)cy << 32) | (uint)cx), progress, cancel);
        }
    }
}
