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
using UoFiddler.Controls.MapViewer.Build;
using UoFiddler.Controls.MapViewer.Editor.Transitions;

namespace UoFiddler.Controls.MapViewer.Editor.Tools
{
    public enum BrushShape
    {
        Square,
        Round
    }

    /// <summary>Cells of a brush</summary>
    public static class MapBrush
    {
        public static List<Point> Cells(int cx, int cy, int size, BrushShape shape, int mapWidth, int mapHeight)
        {
            var result = new List<Point>();
            size = Math.Max(1, size);
            int start = -(size - 1) / 2;
            double r = size / 2.0;
            for (int dy = start; dy < start + size; dy++)
            {
                for (int dx = start; dx < start + size; dx++)
                {
                    if (shape == BrushShape.Round && size > 2)
                    {
                        double ox = dx - start + 0.5 - r, oy = dy - start + 0.5 - r;
                        if ((ox * ox) + (oy * oy) > r * r)
                        {
                            continue;
                        }
                    }

                    int x = cx + dx, y = cy + dy;
                    if (x >= 0 && y >= 0 && x < mapWidth && y < mapHeight)
                    {
                        result.Add(new Point(x, y));
                    }
                }
            }

            return result;
        }

        public static Rectangle Bounds(IEnumerable<Point> cells)
        {
            int x1 = int.MaxValue, y1 = int.MaxValue, x2 = int.MinValue, y2 = int.MinValue;
            foreach (Point p in cells)
            {
                x1 = Math.Min(x1, p.X);
                y1 = Math.Min(y1, p.Y);
                x2 = Math.Max(x2, p.X);
                y2 = Math.Max(y2, p.Y);
            }

            return x1 > x2 ? Rectangle.Empty : Rectangle.FromLTRB(x1, y1, x2 + 1, y2 + 1);
        }
    }

    /// <summary>Everything a tool needs (set by the editor before each use)</summary>
    public sealed class ToolContext
    {
        public ToolContext(Map map, RegionEditSession session)
        {
            Map = map;
            Session = session;
        }

        public Map Map { get; }
        public RegionEditSession Session { get; }

        /// <summary>brush of the paint tool (textures or pattern), can be null</summary>
        public BrushPreset Brush { get; set; }

        /// <summary>cell under the mouse (middle of the brush); null = middle of the cells</summary>
        public Point? BrushCenter { get; set; }

        /// <summary>map position of pattern cell 0,0 in this stroke (set by the first cell)</summary>
        public Point? PatternOrigin { get; set; }

        /// <summary>land tiles chosen in the palette (variants, picked by position)</summary>
        public IReadOnlyList<ushort> Tiles { get; set; } = Array.Empty<ushort>();

        /// <summary>land tiles that are replaced (replace tool)</summary>
        public HashSet<ushort> ReplaceFrom { get; set; } = new HashSet<ushort>();

        /// <summary>raise / lower step</summary>
        public int Amount { get; set; } = 1;

        /// <summary>height for "set height"</summary>
        public int TargetZ { get; set; }

        public ObstacleFilter StaticFilter { get; set; } = ObstacleFilter.All;

        /// <summary>statics keep their height above the ground when the ground height changes</summary>
        public bool StaticsFollowGround { get; set; } = true;

        public TransitionSet Transition { get; set; }
        public TransitionLibrary Library { get; set; }

        /// <summary>terrain tool: true = paint group B of the transition, false = group A</summary>
        public bool PaintTargetTerrain { get; set; }

        /// <summary>cells already changed in this stroke (raise/lower only once per cell)</summary>
        public HashSet<Point> StrokeVisited { get; } = new HashSet<Point>();

        /// <summary>z of the first cell of the stroke (flatten)</summary>
        public int? StrokeZ { get; set; }

        /// <summary>Text for the status bar of the last use</summary>
        public string Message { get; set; }

        public void BeginStroke()
        {
            StrokeVisited.Clear();
            StrokeZ = null;
            PatternOrigin = null;
            Message = null;
        }
    }

    /// <summary>
    /// A tool of the map editor. New tools only need to derive from this class and be added to <see cref="MapTool.All"/>.
    /// </summary>
    public abstract class MapTool
    {
        public abstract string Name { get; }

        public abstract string Help { get; }

        /// <summary>The tool uses the tiles selected in the palette</summary>
        public virtual bool UsesPalette => false;

        public virtual bool UsesBrush => true;

        /// <summary>Changes the given cells (inside of a batch of the session = one undo step per stroke)</summary>
        public abstract void Apply(ToolContext c, IReadOnlyList<Point> cells);

        public override string ToString() => Name;

        public static readonly MapTool[] All =
        {
            new PaintLandTool(),
            new TerrainTool(),
            new ReplaceLandTool(),
            new RaiseLandTool(1),
            new RaiseLandTool(-1),
            new FlattenTool(),
            new SetHeightTool(),
            new SmoothTool(),
            new EraseStaticsTool(),
            new EyedropperTool(),
            new CopyToBrushTool(),
            new LineTool(),
            new ShapeTool(),
            new SelectAreaTool()
        };

        #region Helpers

        protected static sbyte ClampZ(int z) => (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, z));

        /// <summary>Sets new land heights and moves the statics of these cells by the same difference</summary>
        protected static void SetHeights(ToolContext c, Dictionary<Point, int> newZ, string name)
        {
            TileMatrix tiles = c.Map.Tiles;
            var land = new List<(int X, int Y, ushort Id, sbyte Z)>();
            var delta = new Dictionary<Point, int>();
            foreach (KeyValuePair<Point, int> pair in newZ)
            {
                Tile t = tiles.GetLandTile(pair.Key.X, pair.Key.Y);
                sbyte z = ClampZ(pair.Value);
                if (z == t.Z)
                {
                    continue;
                }

                land.Add((pair.Key.X, pair.Key.Y, t.Id, z));
                delta[pair.Key] = z - t.Z;
            }

            if (land.Count == 0)
            {
                return;
            }

            using (c.Session.Batch(name))
            {
                c.Session.SetLandTiles(land, name);
                if (c.StaticsFollowGround)
                {
                    Rectangle b = MapBrush.Bounds(delta.Keys);
                    foreach (MapStatic s in c.Session.GetStaticsInArea(b.Left, b.Top, b.Right - 1, b.Bottom - 1))
                    {
                        if (delta.TryGetValue(new Point(s.X, s.Y), out int d))
                        {
                            c.Session.SetStaticZ(s.X, s.Y, s.Id, s.Z, s.Hue, ClampZ(s.Z + d));
                        }
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>What a paint stroke changes (also used for the preview under the mouse)</summary>
    public sealed class PaintPlan
    {
        public List<(int X, int Y, ushort Id, sbyte Z)> Land { get; } = new List<(int, int, ushort, sbyte)>();
        public List<MapStatic> AddStatics { get; } = new List<MapStatic>();

        /// <summary>cells whose statics are removed first</summary>
        public HashSet<Point> ClearStatics { get; } = new HashSet<Point>();

        /// <summary>height change per cell (to move the statics that stay)</summary>
        public Dictionary<Point, int> HeightDelta { get; } = new Dictionary<Point, int>();
    }

    /// <summary>
    /// Paints with the brush: textures (random variants) or a copied pattern with heights and statics
    /// </summary>
    public sealed class PaintLandTool : MapTool
    {
        public override string Name => "Paint brush";
        public override string Help => "Paints with the brush: textures (palette, random variants) or a pattern copied from the map (land, heights, statics).";
        public override bool UsesPalette => true;

        private static int Mod(int a, int m) => ((a % m) + m) % m;

        /// <summary>
        /// Calculates the changes for the cells. <paramref name="skipPainted"/>: cells already painted in this stroke are skipped
        /// (a pattern with statics must not be set twice on the same cell).
        /// </summary>
        public static PaintPlan Plan(ToolContext c, IReadOnlyList<Point> cells, bool skipPainted)
        {
            var plan = new PaintPlan();
            BrushPreset b = c.Brush;
            TileMatrix tiles = c.Map.Tiles;

            if (b == null || b.Mode == BrushMode.Textures)
            {
                IReadOnlyList<ushort> list = b != null && b.Tiles.Count > 0 ? b.Tiles : c.Tiles;
                if (list.Count == 0)
                {
                    c.Message = "The brush has no textures: click land tiles in the palette.";
                    return plan;
                }

                foreach (Point p in cells)
                {
                    Tile t = tiles.GetLandTile(p.X, p.Y);
                    ushort id = TransitionEngine.Pick(list, p.X, p.Y);
                    if (id != t.Id)
                    {
                        plan.Land.Add((p.X, p.Y, id, t.Z));
                    }
                }

                return plan;
            }

            if (!b.HasPattern)
            {
                c.Message = "The brush has no pattern: \"Copy from map\" or \"Clip -> brush\".";
                return plan;
            }

            BrushPattern pat = b.Pattern;
            if (cells.Count == 0)
            {
                return plan;
            }

            // same offset as the brush (-(size - 1) / 2): brush size = pattern size stamps the whole pattern
            Rectangle bounds = MapBrush.Bounds(cells);
            Point first = c.BrushCenter ?? new Point(bounds.X + ((bounds.Width - 1) / 2), bounds.Y + ((bounds.Height - 1) / 2));
            if (b.Anchor == PatternAnchor.MapGrid)
            {
                c.PatternOrigin = Point.Empty;
            }
            else
            {
                c.PatternOrigin ??= new Point(first.X - ((pat.Width - 1) / 2), first.Y - ((pat.Height - 1) / 2));
            }

            c.StrokeZ ??= tiles.GetLandTile(first.X, first.Y).Z;
            int relative = c.StrokeZ.Value - pat.GetZ((pat.Width - 1) / 2, (pat.Height - 1) / 2);
            Point origin = c.PatternOrigin.Value;

            foreach (Point p in cells)
            {
                if (skipPainted && !c.StrokeVisited.Add(p))
                {
                    continue;
                }

                int px = Mod(p.X - origin.X, pat.Width), py = Mod(p.Y - origin.Y, pat.Height);
                Tile t = tiles.GetLandTile(p.X, p.Y);
                // an empty pattern cell keeps the ground (tile and height)
                ushort? patId = pat.Pick(px, py, p.X, p.Y);
                int z = patId == null ? t.Z : b.Height switch
                {
                    PatternHeight.PatternAbsolute => pat.GetZ(px, py),
                    PatternHeight.PatternRelative => pat.GetZ(px, py) + relative,
                    _ => t.Z
                };
                sbyte newZ = ClampZ(z);
                ushort id = b.PaintLand && patId.HasValue ? patId.Value : t.Id;
                if (id != t.Id || newZ != t.Z)
                {
                    plan.Land.Add((p.X, p.Y, id, newZ));
                }

                if (newZ != t.Z)
                {
                    plan.HeightDelta[p] = newZ - t.Z;
                }

                IReadOnlyList<ClipStatic> statics = b.StaticsAt(px, py);
                if (b.PaintStatics && (patId.HasValue || statics.Count > 0))
                {
                    if (b.ReplaceStatics)
                    {
                        plan.ClearStatics.Add(p);
                    }

                    foreach (ClipStatic s in statics)
                    {
                        plan.AddStatics.Add(new MapStatic(p.X, p.Y, s.Id, ClampZ(newZ + (s.Z - pat.GetZ(px, py))), s.Hue));
                    }
                }
            }

            return plan;
        }

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
            bool pattern = c.Brush != null && c.Brush.Mode == BrushMode.Pattern;
            PaintPlan plan = Plan(c, cells, pattern);
            if (plan.Land.Count == 0 && plan.AddStatics.Count == 0 && plan.ClearStatics.Count == 0)
            {
                return;
            }

            using (c.Session.Batch(Name))
            {
                IEnumerable<Point> touched = plan.ClearStatics.Concat(plan.HeightDelta.Keys);
                Rectangle area = MapBrush.Bounds(touched);
                if (!area.IsEmpty)
                {
                    List<MapStatic> existing = c.Session.GetStaticsInArea(area.Left, area.Top, area.Right - 1, area.Bottom - 1);
                    List<MapStatic> remove = existing.Where(s => plan.ClearStatics.Contains(new Point(s.X, s.Y))).ToList();
                    if (remove.Count > 0)
                    {
                        c.Session.RemoveStatics(remove, Name);
                    }

                    if (c.StaticsFollowGround)
                    {
                        foreach (MapStatic s in existing)
                        {
                            var key = new Point(s.X, s.Y);
                            if (!plan.ClearStatics.Contains(key) && plan.HeightDelta.TryGetValue(key, out int d))
                            {
                                c.Session.SetStaticZ(s.X, s.Y, s.Id, s.Z, s.Hue, ClampZ(s.Z + d));
                            }
                        }
                    }
                }

                if (plan.Land.Count > 0)
                {
                    c.Session.SetLandTiles(plan.Land, Name);
                }

                if (plan.AddStatics.Count > 0)
                {
                    c.Session.AddStatics(plan.AddStatics, Name);
                }
            }
        }
    }

    /// <summary>Points on the map, then the line/curve is drawn with the chosen tool. Handled by the editor.</summary>
    public sealed class LineTool : MapTool
    {
        public override string Name => "Line / curve";
        public override string Help => "Click points on the map, Enter (or \"Draw\") draws the line with the brush size and the tool of \"Draw with\". Backspace = last point away.";

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
        }
    }

    /// <summary>Rectangle, ellipse, triangle or line by dragging, drawn or filled with the chosen tool. Handled by the editor.</summary>
    public sealed class ShapeTool : MapTool
    {
        public override string Name => "Shape (rectangle, circle ...)";
        public override string Help => "Drag a shape on the map. Filled or outline (outline width = brush size), drawn with the tool of \"Draw with\".";

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
        }
    }

    /// <summary>Marks an area (for the generator). Handled by the editor.</summary>
    public sealed class SelectAreaTool : MapTool
    {
        public override string Name => "Select area (generator)";
        public override string Help => "Drag an area on the map. The generator works in this area.";
        public override bool UsesBrush => false;

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
        }
    }

    /// <summary>Copies the brush area under the mouse into the brush (pattern). Handled by the editor.</summary>
    public sealed class CopyToBrushTool : MapTool
    {
        public override string Name => "Copy to brush";
        public override string Help => "Click: copies the ground (and statics) under the brush outline into the brush as pattern. Then paint with \"Paint brush\".";

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
        }
    }

    /// <summary>Paints terrain A or B of a transition and sets the transition graphics around it</summary>
    public sealed class TerrainTool : MapTool
    {
        public override string Name => "Terrain + transitions";
        public override string Help => "Paints terrain A or B of the chosen transition and sets the edge/corner graphics automatically.";

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
            if (c.Transition == null || c.Library == null)
            {
                c.Message = "Choose a transition (Transitions... to create one).";
                return;
            }

            IReadOnlyList<ushort> idsA = c.Library.GroupIds(c.Transition.GroupA);
            IReadOnlyList<ushort> idsB = c.Library.GroupIds(c.Transition.GroupB);
            IReadOnlyList<ushort> paint = c.PaintTargetTerrain ? idsB : idsA;
            if (paint.Count == 0)
            {
                c.Message = $"The terrain group \"{(c.PaintTargetTerrain ? c.Transition.GroupB : c.Transition.GroupA)}\" has no tiles.";
                return;
            }

            TileMatrix tiles = c.Map.Tiles;
            var painted = new Dictionary<Point, ushort>();
            var land = new List<(int X, int Y, ushort Id, sbyte Z)>();
            var keep = new HashSet<ushort>(paint);
            foreach (Point p in cells)
            {
                Tile t = tiles.GetLandTile(p.X, p.Y);
                ushort id = keep.Contains(t.Id) ? t.Id : TransitionEngine.Pick(paint, p.X, p.Y);
                painted[p] = id;
                if (id != t.Id)
                {
                    land.Add((p.X, p.Y, id, t.Z));
                }
            }

            using (c.Session.Batch(Name))
            {
                c.Session.SetLandTiles(land, Name);
                Rectangle area = MapBrush.Bounds(cells);
                area.Inflate(2, 2);
                TransitionResult result = TransitionEngine.Compute(c.Map, area, c.Transition, idsA, idsB);
                c.Session.SetLandTiles(result.Land, Name);
                if (result.Missing.Count > 0)
                {
                    c.Message = "Missing graphics: " + string.Join(", ", result.Missing.Keys.Select(TransitionSet.Label).Distinct());
                }
            }
        }
    }

    public sealed class ReplaceLandTool : MapTool
    {
        public override string Name => "Replace texture";
        public override string Help => "Replaces the tiles of the \"replace\" list (eyedropper: Alt+click) by the tiles selected in the palette.";
        public override bool UsesPalette => true;

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
            if (c.Tiles.Count == 0 || c.ReplaceFrom.Count == 0)
            {
                c.Message = "Set the tiles to replace (eyedropper / list) and select the new tiles in the palette.";
                return;
            }

            TileMatrix tiles = c.Map.Tiles;
            var land = new List<(int X, int Y, ushort Id, sbyte Z)>();
            foreach (Point p in cells)
            {
                Tile t = tiles.GetLandTile(p.X, p.Y);
                if (c.ReplaceFrom.Contains(t.Id))
                {
                    land.Add((p.X, p.Y, TransitionEngine.Pick(c.Tiles, p.X, p.Y), t.Z));
                }
            }

            c.Session.SetLandTiles(land, Name);
        }
    }

    public sealed class RaiseLandTool : MapTool
    {
        private readonly int _sign;

        public RaiseLandTool(int sign)
        {
            _sign = sign;
        }

        public override string Name => _sign > 0 ? "Raise ground" : "Lower ground";
        public override string Help => $"{(_sign > 0 ? "Raises" : "Lowers")} the ground by the step (once per stroke and cell). Statics move with it (option).";

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
            TileMatrix tiles = c.Map.Tiles;
            var newZ = new Dictionary<Point, int>();
            foreach (Point p in cells)
            {
                if (c.StrokeVisited.Add(p))
                {
                    newZ[p] = tiles.GetLandTile(p.X, p.Y).Z + (_sign * Math.Max(1, c.Amount));
                }
            }

            SetHeights(c, newZ, Name);
        }
    }

    public sealed class FlattenTool : MapTool
    {
        public override string Name => "Flatten (z of the first cell)";
        public override string Help => "Sets the ground to the height of the cell where the stroke started.";

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
            if (cells.Count == 0)
            {
                return;
            }

            c.StrokeZ ??= c.Map.Tiles.GetLandTile(cells[cells.Count / 2].X, cells[cells.Count / 2].Y).Z;
            SetHeights(c, cells.ToDictionary(p => p, _ => c.StrokeZ.Value), Name);
        }
    }

    public sealed class SetHeightTool : MapTool
    {
        public override string Name => "Set height";
        public override string Help => "Sets the ground to the given z.";

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
            SetHeights(c, cells.ToDictionary(p => p, _ => c.TargetZ), Name);
        }
    }

    public sealed class SmoothTool : MapTool
    {
        public override string Name => "Smooth";
        public override string Help => "Each cell gets the average height of its 3x3 neighbors (drag several times for softer hills).";

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
            TileMatrix tiles = c.Map.Tiles;
            var newZ = new Dictionary<Point, int>();
            foreach (Point p in cells)
            {
                int sum = 0, count = 0;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int x = p.X + dx, y = p.Y + dy;
                        if (x >= 0 && y >= 0 && x < c.Map.Width && y < c.Map.Height)
                        {
                            sum += tiles.GetLandTile(x, y).Z;
                            count++;
                        }
                    }
                }

                newZ[p] = (int)Math.Round(sum / (double)count);
            }

            SetHeights(c, newZ, Name);
        }
    }

    public sealed class EraseStaticsTool : MapTool
    {
        public override string Name => "Erase statics";
        public override string Help => "Removes statics (all or only trees/plants/rocks, see filter).";

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
            if (cells.Count == 0)
            {
                return;
            }

            var set = new HashSet<Point>(cells);
            Rectangle b = MapBrush.Bounds(cells);
            List<MapStatic> remove = c.Session.GetStaticsInArea(b.Left, b.Top, b.Right - 1, b.Bottom - 1)
                .Where(s => set.Contains(new Point(s.X, s.Y)) && BuildPlacement.MatchesFilter(s, c.StaticFilter == ObstacleFilter.None ? ObstacleFilter.All : c.StaticFilter))
                .ToList();
            c.Session.RemoveStatics(remove, Name);
        }
    }

    /// <summary>Takes the land tile under the mouse (handled by the editor)</summary>
    public sealed class EyedropperTool : MapTool
    {
        public override string Name => "Eyedropper";
        public override string Help => "Click: takes the land tile into the palette and the \"replace\" list and its z into \"Set height\".";
        public override bool UsesBrush => false;

        public override void Apply(ToolContext c, IReadOnlyList<Point> cells)
        {
        }
    }
}
