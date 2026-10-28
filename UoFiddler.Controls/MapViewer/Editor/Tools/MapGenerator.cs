/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Ultima;
using UoFiddler.Controls.MapViewer.Editor.Transitions;

namespace UoFiddler.Controls.MapViewer.Editor.Tools
{
    public sealed class GeneratorOptions
    {
        public Rectangle Area { get; set; }
        public int Seed { get; set; } = 1;

        // ---- settlement
        public bool Houses { get; set; } = true;
        public int HouseCount { get; set; } = 6;
        public int PlotWidth { get; set; } = 7;
        public int PlotHeight { get; set; } = 7;

        /// <summary>cells between two plots</summary>
        public int Spacing { get; set; } = 3;

        /// <summary>cells between the road and the plots</summary>
        public int RoadDistance { get; set; } = 2;

        /// <summary>floor of the plots (textures or pattern); null = only flatten/clear</summary>
        public BrushPreset FloorBrush { get; set; }

        public bool FlattenPlots { get; set; } = true;
        public bool ClearStatics { get; set; } = true;

        // ---- road
        public bool Road { get; set; } = true;
        public bool RoadHorizontal { get; set; } = true;
        public int RoadWidth { get; set; } = 3;
        public BrushPreset RoadBrush { get; set; }
        public bool PathsToHouses { get; set; } = true;

        // ---- roughness
        public bool Roughness { get; set; } = true;

        /// <summary>max. height change</summary>
        public int RoughAmount { get; set; } = 1;

        /// <summary>chance per cell in percent</summary>
        public int RoughPercent { get; set; } = 30;

        public bool RoughAlsoLower { get; set; }
        public bool RoughRoad { get; set; } = true;
        public bool RoughGround { get; set; } = true;

        // ---- lake
        public bool Lake { get; set; }
        public Point LakeCenter { get; set; }
        public int LakeRadiusX { get; set; } = 8;
        public int LakeRadiusY { get; set; } = 6;

        /// <summary>0 = exact ellipse, 100 = very irregular shore</summary>
        public int LakeIrregular { get; set; } = 35;

        public List<ushort> WaterTiles { get; set; } = new List<ushort> { 0xA8, 0xA9, 0xAA, 0xAB };
        public int WaterZ { get; set; } = -5;

        /// <summary>shore slope over this many cells</summary>
        public int ShoreWidth { get; set; } = 2;

        /// <summary>transition water -> ground (optional; group A must be the water)</summary>
        public TransitionSet ShoreTransition { get; set; }

        // ---- vegetation
        public bool Vegetation { get; set; } = true;
        public List<ushort> TreeIds { get; set; } = new List<ushort> { 0x0CCA, 0x0CCB, 0x0CCC, 0x0CCD, 0x0CD0, 0x0CD3, 0x0CD6, 0x0CD8, 0x0CDA, 0x0CDD, 0x0CE0, 0x0CE3, 0x0CE6 };

        /// <summary>chance per cell in per mille (10 = 1 %)</summary>
        public int TreeDensity { get; set; } = 25;

        public int TreeMinDistance { get; set; } = 2;
        public List<ushort> PlantIds { get; set; } = new List<ushort>();
        public int PlantDensity { get; set; } = 40;

        /// <summary>only on these land tiles (empty = everywhere except water/road/plots)</summary>
        public HashSet<ushort> VegetationGround { get; set; } = new HashSet<ushort>();

        /// <summary>no vegetation on cells that have statics already</summary>
        public bool VegetationOnlyEmpty { get; set; } = true;
    }

    public sealed class GeneratorResult
    {
        public List<Rectangle> Plots { get; } = new List<Rectangle>();
        public int RoadCells { get; set; }
        public int LakeCells { get; set; }
        public int RoughCells { get; set; }
        public int Trees { get; set; }
        public int Plants { get; set; }
        public List<string> Notes { get; } = new List<string>();

        public override string ToString()
        {
            return $"{Plots.Count} plot(s), {RoadCells} road cells, {LakeCells} water cells, {RoughCells} cells roughened, {Trees} trees, {Plants} plants" +
                   (Notes.Count > 0 ? "\n" + string.Join("\n", Notes) : string.Empty);
        }
    }

    /// <summary>
    /// Creates a settlement (plots with floors, road, paths), a lake, rough ground and vegetation in an area.
    /// Everything is one undo step.
    /// </summary>
    public static class MapGenerator
    {
        public const string StepName = "Generator";

        public static GeneratorResult Run(Map map, RegionEditSession session, GeneratorOptions o, TransitionLibrary transitions)
        {
            var result = new GeneratorResult();
            Rectangle area = Rectangle.Intersect(o.Area, new Rectangle(0, 0, map.Width, map.Height));
            if (area.Width < 3 || area.Height < 3)
            {
                result.Notes.Add("The area is too small.");
                return result;
            }

            var random = new Random(o.Seed);
            var blocked = new HashSet<Point>(); // plots, road, water: no roughness/vegetation
            var roadCells = new HashSet<Point>();
            var water = new HashSet<Point>();

            using (session.Batch(StepName))
            {
                if (o.Lake)
                {
                    MakeLake(map, session, o, transitions, random, area, water, result);
                    blocked.UnionWith(water);
                }

                if (o.Houses || o.Road)
                {
                    MakeSettlement(map, session, o, random, area, blocked, roadCells, water, result);
                }

                if (o.Roughness)
                {
                    Roughen(map, session, o, random, area, blocked, roadCells, result);
                }

                if (o.Vegetation)
                {
                    Plant(map, session, o, random, area, blocked, result);
                }
            }

            map.Tiles.CloseStreams();
            return result;
        }

        #region Lake

        private static void MakeLake(Map map, RegionEditSession session, GeneratorOptions o, TransitionLibrary transitions, Random random,
            Rectangle area, HashSet<Point> water, GeneratorResult result)
        {
            if (o.WaterTiles.Count == 0)
            {
                result.Notes.Add("Lake: no water tiles.");
                return;
            }

            // irregular shore: radius changes with the angle (sum of waves with random phases)
            double p1 = random.NextDouble() * Math.PI * 2, p2 = random.NextDouble() * Math.PI * 2, p3 = random.NextDouble() * Math.PI * 2;
            double irregular = Math.Max(0, Math.Min(100, o.LakeIrregular)) / 100.0;
            double Radius(double angle) => 1 + (irregular * 0.45 * ((Math.Sin((2 * angle) + p1) * 0.5) + (Math.Sin((3 * angle) + p2) * 0.3) + (Math.Sin((5 * angle) + p3) * 0.2)));

            int rx = Math.Max(1, o.LakeRadiusX), ry = Math.Max(1, o.LakeRadiusY);
            Point c = o.LakeCenter;
            int shore = Math.Max(0, o.ShoreWidth);
            var land = new List<(int X, int Y, ushort Id, sbyte Z)>();
            var shoreZ = new Dictionary<Point, int>();
            TileMatrix tiles = map.Tiles;

            for (int y = c.Y - (ry * 2) - shore; y <= c.Y + (ry * 2) + shore; y++)
            {
                for (int x = c.X - (rx * 2) - shore; x <= c.X + (rx * 2) + shore; x++)
                {
                    if (!area.Contains(x, y))
                    {
                        continue;
                    }

                    double dx = (x - c.X) / (double)rx, dy = (y - c.Y) / (double)ry;
                    double d = Math.Sqrt((dx * dx) + (dy * dy)) / Radius(Math.Atan2(dy, dx));
                    var p = new Point(x, y);
                    if (d <= 1.0)
                    {
                        water.Add(p);
                        land.Add((x, y, TransitionEngine.Pick(o.WaterTiles, x, y), (sbyte)o.WaterZ));
                    }
                    else if (shore > 0)
                    {
                        // distance outside of the lake in cells (approximately)
                        double outside = (d - 1.0) * Math.Min(rx, ry);
                        if (outside < shore)
                        {
                            int ground = tiles.GetLandTile(x, y).Z;
                            double t = (outside + 1) / (shore + 1);
                            shoreZ[p] = (int)Math.Round(o.WaterZ + ((ground - o.WaterZ) * Math.Max(0.35, t)));
                        }
                    }
                }
            }

            foreach (KeyValuePair<Point, int> s in shoreZ)
            {
                Tile t = tiles.GetLandTile(s.Key.X, s.Key.Y);
                if (s.Value < t.Z)
                {
                    land.Add((s.Key.X, s.Key.Y, t.Id, (sbyte)s.Value));
                }
            }

            if (water.Count == 0)
            {
                result.Notes.Add("Lake: the center is outside of the area.");
                return;
            }

            Rectangle bounds = MapBrush.Bounds(water);
            List<MapStatic> statics = session.GetStaticsInArea(bounds.Left, bounds.Top, bounds.Right - 1, bounds.Bottom - 1)
                .Where(s => water.Contains(new Point(s.X, s.Y))).ToList();
            session.RemoveStatics(statics, StepName);
            session.SetLandTiles(land, StepName);
            result.LakeCells = water.Count;

            if (o.ShoreTransition != null && transitions != null)
            {
                bounds.Inflate(2, 2);
                TransitionResult tr = TransitionEngine.Compute(map, bounds, o.ShoreTransition,
                    transitions.GroupIds(o.ShoreTransition.GroupA), transitions.GroupIds(o.ShoreTransition.GroupB));
                session.SetLandTiles(tr.Land, StepName);
                if (tr.Missing.Count > 0)
                {
                    result.Notes.Add("Shore transition: missing graphics " + string.Join(", ", tr.Missing.Keys.Select(TransitionSet.Label)));
                }
            }
        }

        #endregion

        #region Settlement

        private static void MakeSettlement(Map map, RegionEditSession session, GeneratorOptions o, Random random, Rectangle area,
            HashSet<Point> blocked, HashSet<Point> roadCells, HashSet<Point> water, GeneratorResult result)
        {
            bool horizontal = o.RoadHorizontal;
            int length = horizontal ? area.Width : area.Height;
            int across = horizontal ? area.Height : area.Width;
            int roadWidth = Math.Max(1, o.RoadWidth);
            int mid = across / 2;
            int roadStart = mid - ((roadWidth - 1) / 2);

            // map position from (along, across) inside of the area
            Point P(int along, int acr) => horizontal ? new Point(area.X + along, area.Y + acr) : new Point(area.X + acr, area.Y + along);

            var road = new List<Point>();
            if (o.Road)
            {
                for (int a = 0; a < length; a++)
                {
                    for (int w = 0; w < roadWidth; w++)
                    {
                        Point p = P(a, roadStart + w);
                        if (!water.Contains(p))
                        {
                            road.Add(p);
                        }
                    }
                }
            }

            // plots on both sides of the road
            var plots = new List<(Rectangle Rect, bool Upper)>();
            if (o.Houses && o.HouseCount > 0)
            {
                int pw = Math.Max(2, horizontal ? o.PlotWidth : o.PlotHeight); // along the road
                int ph = Math.Max(2, horizontal ? o.PlotHeight : o.PlotWidth); // across
                int perSide = (o.HouseCount + 1) / 2;
                int fit = Math.Max(0, (length - 2 + o.Spacing) / (pw + o.Spacing));
                if (fit < perSide)
                {
                    result.Notes.Add($"Only {fit * 2} plots fit into the area (wanted {o.HouseCount}).");
                    perSide = fit;
                }

                int used = (perSide * pw) + (Math.Max(0, perSide - 1) * o.Spacing);
                int start = (length - used) / 2;
                int upperAcross = roadStart - o.RoadDistance - ph;
                int lowerAcross = roadStart + roadWidth + o.RoadDistance;
                if (upperAcross < 0 || lowerAcross + ph > across)
                {
                    result.Notes.Add("The area is too narrow for plots on both sides of the road.");
                }

                int placed = 0;
                for (int i = 0; i < perSide && placed < o.HouseCount; i++)
                {
                    int along = start + (i * (pw + o.Spacing));
                    foreach (bool upper in new[] { true, false })
                    {
                        if (placed >= o.HouseCount)
                        {
                            break;
                        }

                        int acr = upper ? upperAcross : lowerAcross;
                        if (acr < 0 || acr + ph > across)
                        {
                            continue;
                        }

                        Point a = P(along, acr), b = P(along + pw - 1, acr + ph - 1);
                        Rectangle rect = Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X) + 1, Math.Max(a.Y, b.Y) + 1);
                        if (Cells(rect).Any(water.Contains))
                        {
                            continue;
                        }

                        plots.Add((rect, upper));
                        placed++;
                    }
                }
            }

            // paths from the middle of each plot to the road
            var paths = new List<Point>();
            if (o.Road && o.PathsToHouses)
            {
                foreach ((Rectangle rect, bool upper) in plots)
                {
                    Point center = new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
                    for (int d = 1; d <= o.RoadDistance; d++)
                    {
                        Point p = horizontal
                            ? new Point(center.X, upper ? rect.Bottom - 1 + d : rect.Top - d)
                            : new Point(upper ? rect.Right - 1 + d : rect.Left - d, center.Y);
                        if (area.Contains(p) && !water.Contains(p))
                        {
                            paths.Add(p);
                        }
                    }
                }
            }

            var allRoad = road.Concat(paths).ToList();
            var plotCells = plots.SelectMany(p => Cells(p.Rect)).ToList();

            // statics on plots and road
            if (o.ClearStatics && (plotCells.Count > 0 || allRoad.Count > 0))
            {
                var clear = new HashSet<Point>(plotCells.Concat(allRoad));
                List<MapStatic> remove = session.GetStaticsInArea(area.Left, area.Top, area.Right - 1, area.Bottom - 1)
                    .Where(s => clear.Contains(new Point(s.X, s.Y))).ToList();
                session.RemoveStatics(remove, StepName);
            }

            TileMatrix tiles = map.Tiles;

            // plots: flatten, floor
            foreach ((Rectangle rect, bool _) in plots)
            {
                List<Point> cells = Cells(rect);
                if (o.FlattenPlots)
                {
                    int z = (int)Math.Round(cells.Average(p => (double)tiles.GetLandTile(p.X, p.Y).Z));
                    session.SetLandTiles(cells.Select(p => (p.X, p.Y, tiles.GetLandTile(p.X, p.Y).Id, (sbyte)z)).ToList(), StepName);
                }

                if (o.FloorBrush != null)
                {
                    var c = new ToolContext(map, session) { Brush = o.FloorBrush, Tiles = o.FloorBrush.Tiles, StaticsFollowGround = false, PatternOrigin = rect.Location };
                    new PaintLandTool().Apply(c, cells);
                }

                result.Plots.Add(rect);
                blocked.UnionWith(cells);
            }

            // road
            if (allRoad.Count > 0 && o.RoadBrush != null)
            {
                var c = new ToolContext(map, session) { Brush = o.RoadBrush, Tiles = o.RoadBrush.Tiles, StaticsFollowGround = false, PatternOrigin = area.Location };
                new PaintLandTool().Apply(c, allRoad);
            }
            else if (allRoad.Count > 0)
            {
                result.Notes.Add("Road: no road brush/tiles, only the statics were removed.");
            }

            roadCells.UnionWith(allRoad);
            blocked.UnionWith(allRoad);
            result.RoadCells = allRoad.Count;
        }

        private static List<Point> Cells(Rectangle r)
        {
            var list = new List<Point>(r.Width * r.Height);
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    list.Add(new Point(x, y));
                }
            }

            return list;
        }

        #endregion

        #region Roughness

        private static void Roughen(Map map, RegionEditSession session, GeneratorOptions o, Random random, Rectangle area,
            HashSet<Point> blocked, HashSet<Point> roadCells, GeneratorResult result)
        {
            TileMatrix tiles = map.Tiles;
            int amount = Math.Max(1, o.RoughAmount);
            var land = new List<(int X, int Y, ushort Id, sbyte Z)>();
            var delta = new Dictionary<Point, int>();
            foreach (Point p in Cells(area))
            {
                bool isRoad = roadCells.Contains(p);
                if ((isRoad && !o.RoughRoad) || (!isRoad && (!o.RoughGround || blocked.Contains(p))))
                {
                    continue;
                }

                if (random.Next(100) >= o.RoughPercent)
                {
                    continue;
                }

                int change = random.Next(1, amount + 1);
                if (o.RoughAlsoLower && random.Next(2) == 0)
                {
                    change = -change;
                }

                Tile t = tiles.GetLandTile(p.X, p.Y);
                int z = Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, t.Z + change));
                land.Add((p.X, p.Y, t.Id, (sbyte)z));
                delta[p] = z - t.Z;
            }

            session.SetLandTiles(land, StepName);

            // statics stay on the ground
            foreach (MapStatic s in session.GetStaticsInArea(area.Left, area.Top, area.Right - 1, area.Bottom - 1))
            {
                if (delta.TryGetValue(new Point(s.X, s.Y), out int d) && d != 0)
                {
                    session.SetStaticZ(s.X, s.Y, s.Id, s.Z, s.Hue, Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, s.Z + d)));
                }
            }

            result.RoughCells = land.Count;
        }

        #endregion

        #region Vegetation

        private static void Plant(Map map, RegionEditSession session, GeneratorOptions o, Random random, Rectangle area,
            HashSet<Point> blocked, GeneratorResult result)
        {
            TileMatrix tiles = map.Tiles;
            var occupied = new HashSet<Point>(o.VegetationOnlyEmpty
                ? session.GetStaticsInArea(area.Left, area.Top, area.Right - 1, area.Bottom - 1).Select(s => new Point(s.X, s.Y))
                : Enumerable.Empty<Point>());
            var trees = new HashSet<Point>();
            var add = new List<MapStatic>();
            int minDistance = Math.Max(0, o.TreeMinDistance);

            bool FarFromTrees(Point p)
            {
                for (int dy = -minDistance; dy <= minDistance; dy++)
                {
                    for (int dx = -minDistance; dx <= minDistance; dx++)
                    {
                        if (trees.Contains(new Point(p.X + dx, p.Y + dy)))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            bool Allowed(Point p)
            {
                if (blocked.Contains(p) || occupied.Contains(p))
                {
                    return false;
                }

                ushort id = tiles.GetLandTile(p.X, p.Y).Id;
                if (o.VegetationGround.Count > 0)
                {
                    return o.VegetationGround.Contains(id);
                }

                return !o.WaterTiles.Contains(id) && id != 2;
            }

            foreach (Point p in Cells(area))
            {
                if (o.TreeIds.Count > 0 && random.Next(1000) < o.TreeDensity && Allowed(p) && FarFromTrees(p))
                {
                    trees.Add(p);
                    occupied.Add(p);
                    add.Add(new MapStatic(p.X, p.Y, o.TreeIds[random.Next(o.TreeIds.Count)], tiles.GetLandTile(p.X, p.Y).Z, 0));
                    result.Trees++;
                    continue;
                }

                if (o.PlantIds.Count > 0 && random.Next(1000) < o.PlantDensity && Allowed(p))
                {
                    occupied.Add(p);
                    add.Add(new MapStatic(p.X, p.Y, o.PlantIds[random.Next(o.PlantIds.Count)], tiles.GetLandTile(p.X, p.Y).Z, 0));
                    result.Plants++;
                }
            }

            session.AddStatics(add, StepName);
        }

        #endregion
    }
}
