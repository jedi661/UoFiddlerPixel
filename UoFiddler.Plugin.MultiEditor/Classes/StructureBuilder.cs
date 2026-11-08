/***************************************************************************
 *
 * MultiEditor – structure tool
 *
 * Drag a line = wall, drag a rectangle = room (walls, floor, flat or
 * gable roof). Dragging again over a building stacks the next floor.
 *
 * Extending: implement IStructurePattern and add it to
 * StructureBuilder.BuiltInPatterns (or let the user learn line patterns
 * from a selection, they are stored in MultiEditorStructures.json together
 * with the building parts library).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    public sealed class StructureContext
    {
        /// <summary>
        /// Start / end cell of the mouse drag (inclusive)
        /// </summary>
        public Point Start { get; set; }

        public Point End { get; set; }

        public int Z { get; set; }

        public HouseStyle Style { get; set; }

        public int FloorHeight { get; set; } = 20;

        public int RoofStep { get; set; } = 3;

        public bool DoorGap { get; set; }

        public bool Windows { get; set; }

        /// <summary>
        /// Shift pressed while dragging
        /// </summary>
        public bool Alternate { get; set; }

        /// <summary>
        /// Draw tile (for patterns that use it)
        /// </summary>
        public int DrawTile { get; set; } = -1;

        public int Hue { get; set; }

        public IDictionary<ushort, ushort> SwapMap { get; set; }

        public int MinX => Math.Min(Start.X, End.X);
        public int MaxX => Math.Max(Start.X, End.X);
        public int MinY => Math.Min(Start.Y, End.Y);
        public int MaxY => Math.Max(Start.Y, End.Y);

        /// <summary>
        /// Line along X when the drag is wider than deep
        /// </summary>
        public bool AlongX => Math.Abs(End.X - Start.X) >= Math.Abs(End.Y - Start.Y);
    }

    public interface IStructurePattern
    {
        string Name { get; }

        /// <summary>
        /// true = line (wall), false = area (room)
        /// </summary>
        bool IsLine { get; }

        /// <summary>
        /// Tiles in cell coordinates
        /// </summary>
        List<ImportTile> Build(StructureContext c);
    }

    /// <summary>
    /// Wall along the drag line (X or Y), with a post at the end
    /// </summary>
    public sealed class WallLinePattern : IStructurePattern
    {
        public string Name => L.T("Mauer (Linie ziehen)", "Wall (drag a line)");

        public bool IsLine => true;

        public List<ImportTile> Build(StructureContext c)
        {
            var list = new List<ImportTile>();
            HouseStyle s = c.Style;
            if (s?.Template != null && s.Template.Levels.Count > 0)
            {
                // learned side: north side along X, west side along Y
                LevelTemplate lt = s.Template.Levels[0];
                int len = c.AlongX ? c.MaxX - c.MinX + 1 : c.MaxY - c.MinY + 1;
                List<TileStack> stacks = (c.AlongX ? lt.North : lt.West).Build(len, c.Windows);
                for (int k = 0; k < stacks.Count; ++k)
                {
                    foreach (int[] t in stacks[k].Tiles)
                    {
                        list.Add(new ImportTile
                        {
                            Id = (ushort)t[0],
                            X = c.AlongX ? c.MinX + k : c.Start.X,
                            Y = c.AlongX ? c.Start.Y : c.MinY + k,
                            Z = c.Z + t[1],
                            Hue = c.Hue
                        });
                    }
                }

                return list;
            }

            if (s == null || s.WallX <= 0 || s.WallY <= 0)
            {
                return list;
            }

            int i = 0;
            if (c.AlongX)
            {
                for (int x = c.MinX; x <= c.MaxX; ++x, ++i)
                {
                    int id = c.Windows && s.WindowX > 0 && i % 3 == 2 ? s.WindowX : s.WallX;
                    list.Add(new ImportTile { Id = (ushort)id, X = x, Y = c.Start.Y, Z = c.Z, Hue = c.Hue });
                }

                if (s.Post > 0 && !c.Alternate)
                {
                    list.Add(new ImportTile { Id = (ushort)s.Post, X = c.MaxX + 1, Y = c.Start.Y, Z = c.Z, Hue = c.Hue });
                }
            }
            else
            {
                for (int y = c.MinY; y <= c.MaxY; ++y, ++i)
                {
                    int id = c.Windows && s.WindowY > 0 && i % 3 == 2 ? s.WindowY : s.WallY;
                    list.Add(new ImportTile { Id = (ushort)id, X = c.Start.X, Y = y, Z = c.Z, Hue = c.Hue });
                }

                if (s.Post > 0 && !c.Alternate)
                {
                    list.Add(new ImportTile { Id = (ushort)s.Post, X = c.Start.X, Y = c.MaxY + 1, Z = c.Z, Hue = c.Hue });
                }
            }

            return list;
        }
    }

    /// <summary>
    /// Room: walls on the rectangle border, floor inside, optional roof
    /// </summary>
    public sealed class RoomPattern : IStructurePattern
    {
        private readonly RoofType _roof;
        private readonly bool _withWalls;
        private readonly bool _withFloor;

        public RoomPattern(RoofType roof, bool withWalls = true, bool withFloor = true)
        {
            _roof = roof;
            _withWalls = withWalls;
            _withFloor = withFloor;
        }

        public string Name
        {
            get
            {
                if (!_withWalls && !_withFloor)
                {
                    return _roof == RoofType.Flat ? L.T("Nur Flachdach", "Flat roof only") : L.T("Nur Satteldach (Schindeln)", "Gable roof only (shingles)");
                }

                if (!_withWalls)
                {
                    return L.T("Nur Boden (Fläche)", "Floor only (area)");
                }

                return _roof switch
                {
                    RoofType.Flat => L.T("Raum + Flachdach", "Room + flat roof"),
                    RoofType.GableX or RoofType.GableY => L.T("Raum + Satteldach (Schindeln)", "Room + gable roof (shingles)"),
                    _ => L.T("Raum (Wände + Boden)", "Room (walls + floor)")
                };
            }
        }

        public bool IsLine => false;

        public List<ImportTile> Build(StructureContext c)
        {
            var list = new List<ImportTile>();
            HouseStyle s = c.Style;
            if (s == null)
            {
                return list;
            }

            if (s.Template != null && s.Template.Levels.Count > 0)
            {
                return BuildFromTemplate(c, s.Template);
            }

            // walls lie on the border lines, the room inside is w x h cells
            int w = Math.Max(1, c.MaxX - c.MinX);
            int h = Math.Max(1, c.MaxY - c.MinY);

            if (_withWalls && s.IsUsable && w >= 2 && h >= 2)
            {
                var o = new HouseGeneratorOptions
                {
                    Width = w,
                    Depth = h,
                    Floors = 1,
                    FloorHeight = c.FloorHeight,
                    BaseZ = c.Z,
                    RoofStep = c.RoofStep,
                    DoorGap = c.DoorGap,
                    Windows = c.Windows,
                    Stairs = false,
                    Roof = ResolveRoof(w, h, c.Alternate, s),
                    Style = s
                };
                list.AddRange(HouseGenerator.Generate(o));
                if (!_withFloor)
                {
                    list.RemoveAll(t => t.Id == s.Floor && t.Z == c.Z);
                }
            }
            else
            {
                // no walls: floor and / or roof over the full rectangle
                int fw = c.MaxX - c.MinX + 1;
                int fh = c.MaxY - c.MinY + 1;
                int floorId = c.DrawTile > 0 && s.Floor <= 0 ? c.DrawTile : s.Floor;
                if (_withFloor && floorId > 0)
                {
                    for (int x = 0; x < fw; ++x)
                    {
                        for (int y = 0; y < fh; ++y)
                        {
                            list.Add(new ImportTile { Id = (ushort)floorId, X = x, Y = y, Z = c.Z });
                        }
                    }
                }

                if (_roof != RoofType.None)
                {
                    list.AddRange(BuildRoof(s, fw - 1, fh - 1, c.Z, c.RoofStep, ResolveRoof(fw, fh, c.Alternate, s)));
                }
            }

            foreach (ImportTile t in list)
            {
                t.X += c.MinX;
                t.Y += c.MinY;
                t.Hue = c.Hue;
            }

            return list;
        }

        /// <summary>
        /// The rectangle is the wall lines, floor and roof follow the learned offsets
        /// </summary>
        private List<ImportTile> BuildFromTemplate(StructureContext c, HouseTemplate t)
        {
            var list = new List<ImportTile>();
            int x0 = c.MinX, x1 = c.MaxX, y0 = c.MinY, y1 = c.MaxY;

            if (_withWalls && x1 - x0 >= 1 && y1 - y0 >= 1)
            {
                list.AddRange(t.BuildWalls(t.Levels[0], y0, y1, x0, x1, c.Z, c.Windows, c.DoorGap, false));
            }

            if (_withFloor)
            {
                int fid = _withWalls ? t.FloorId : (t.FloorId > 0 ? t.FloorId : c.DrawTile);
                if (_withWalls)
                {
                    HouseTemplate.AddFloor(list, fid, x0 - t.OffsetWest, y0 - t.OffsetNorth, x1 - t.OffsetEast, y1 - t.OffsetSouth, c.Z);
                }
                else
                {
                    HouseTemplate.AddFloor(list, fid, x0, y0, x1, y1, c.Z);
                }
            }

            if (_roof != RoofType.None)
            {
                RoofType roof = _roof == RoofType.Flat ? RoofType.Flat : (t.Roof is RoofType.GableX or RoofType.GableY ? ResolveTemplateRoof(x1 - x0, y1 - y0, c.Alternate, t) : t.Roof);
                int top = _withWalls ? c.Z + t.FloorHeight : c.Z - t.RoofDz;
                list.AddRange(t.BuildRoof(roof == RoofType.None ? RoofType.Flat : roof, y0, y1, x0, x1, top));
            }

            foreach (ImportTile tile in list)
            {
                tile.Hue = c.Hue;
            }

            return list;
        }

        private static RoofType ResolveTemplateRoof(int w, int h, bool alternate, HouseTemplate t)
        {
            // the learned roof has a direction; for other proportions the same ids are used turned (Shift = other way)
            bool alongX = w >= h;
            if (alternate)
            {
                alongX = !alongX;
            }

            return alongX ? RoofType.GableX : RoofType.GableY;
        }

        private RoofType ResolveRoof(int w, int h, bool alternate, HouseStyle s)
        {
            if (_roof is not (RoofType.GableX or RoofType.GableY))
            {
                return _roof;
            }

            // ridge along the longer side, Shift = other direction
            bool alongX = w >= h;
            if (alternate)
            {
                alongX = !alongX;
            }

            if (alongX && s.HasGableRoofX)
            {
                return RoofType.GableX;
            }

            if (!alongX && s.HasGableRoofY)
            {
                return RoofType.GableY;
            }

            return s.HasGableRoofX ? RoofType.GableX : s.HasGableRoofY ? RoofType.GableY : RoofType.Flat;
        }

        /// <summary>
        /// Roof over cells 0..w / 0..h (same as the house generator)
        /// </summary>
        public static List<ImportTile> BuildRoof(HouseStyle s, int w, int h, int z, int step, RoofType roof)
        {
            var list = new List<ImportTile>();
            switch (roof)
            {
                case RoofType.Flat:
                    if (s.Floor > 0)
                    {
                        for (int x = 0; x <= w; ++x)
                        {
                            for (int y = 0; y <= h; ++y)
                            {
                                list.Add(new ImportTile { Id = (ushort)s.Floor, X = x, Y = y, Z = z });
                            }
                        }
                    }

                    break;
                case RoofType.GableX:
                    for (int y = 0; y <= h; ++y)
                    {
                        int dist = Math.Min(y, h - y);
                        int id = y < h - y ? s.RoofNorth : h - y < y ? s.RoofSouth : (s.RoofRidge > 0 ? s.RoofRidge : s.RoofNorth);
                        for (int x = 0; x <= w; ++x)
                        {
                            list.Add(new ImportTile { Id = (ushort)id, X = x, Y = y, Z = z + (dist * step) });
                        }
                    }

                    break;
                case RoofType.GableY:
                    for (int x = 0; x <= w; ++x)
                    {
                        int dist = Math.Min(x, w - x);
                        int id = x < w - x ? s.RoofWest : w - x < x ? s.RoofEast : (s.RoofRidge > 0 ? s.RoofRidge : s.RoofWest);
                        for (int y = 0; y <= h; ++y)
                        {
                            list.Add(new ImportTile { Id = (ushort)id, X = x, Y = y, Z = z + (dist * step) });
                        }
                    }

                    break;
            }

            list.RemoveAll(t => t.Id == 0);
            return list;
        }
    }

    /// <summary>
    /// A repeating line pattern learned from a selection (e.g. wall with window every 3rd tile, battlements on top)
    /// </summary>
    public sealed class LinePatternData
    {
        public string Name { get; set; }

        /// <summary>
        /// The pattern was learned along X
        /// </summary>
        public bool AlongX { get; set; } = true;

        public int Period { get; set; } = 1;

        /// <summary>
        /// [id, along, across, dz, hue]
        /// </summary>
        public List<int[]> Tiles { get; set; } = new List<int[]>();
    }

    public sealed class LearnedLinePattern : IStructurePattern
    {
        private readonly LinePatternData _data;

        public LearnedLinePattern(LinePatternData data)
        {
            _data = data;
        }

        public LinePatternData Data => _data;

        public string Name => L.T("Muster: ", "Pattern: ") + _data.Name;

        public bool IsLine => true;

        public List<ImportTile> Build(StructureContext c)
        {
            var list = new List<ImportTile>();
            bool alongX = c.AlongX;
            bool swap = alongX != _data.AlongX;
            int length = alongX ? c.MaxX - c.MinX + 1 : c.MaxY - c.MinY + 1;
            int period = Math.Max(1, _data.Period);

            for (int i = 0; i < length; ++i)
            {
                int phase = i % period;
                foreach (int[] t in _data.Tiles.Where(t => t[1] == phase))
                {
                    ushort id = (ushort)t[0];
                    if (swap && c.SwapMap != null && c.SwapMap.TryGetValue(id, out ushort n))
                    {
                        id = n;
                    }

                    int along = (alongX ? c.MinX : c.MinY) + i;
                    int across = (alongX ? c.Start.Y : c.Start.X) + t[2];
                    list.Add(new ImportTile
                    {
                        Id = id,
                        X = alongX ? along : across,
                        Y = alongX ? across : along,
                        Z = c.Z + t[3],
                        Hue = t.Length > 4 && t[4] > 0 ? t[4] : c.Hue
                    });
                }
            }

            return list;
        }

        /// <summary>
        /// Learns a pattern from tiles (the longer side of the selection is the line direction)
        /// </summary>
        public static LinePatternData Learn(IList<ImportTile> tiles, string name, int period)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return null;
            }

            int minX = tiles.Min(t => t.X);
            int minY = tiles.Min(t => t.Y);
            int minZ = tiles.Min(t => t.Z);
            int spanX = tiles.Max(t => t.X) - minX;
            int spanY = tiles.Max(t => t.Y) - minY;
            bool alongX = spanX >= spanY;
            int length = (alongX ? spanX : spanY) + 1;
            period = period <= 0 ? length : Math.Min(period, length);

            var data = new LinePatternData { Name = name, AlongX = alongX, Period = period };
            foreach (ImportTile t in tiles)
            {
                int along = alongX ? t.X - minX : t.Y - minY;
                if (along >= period)
                {
                    continue;
                }

                int across = alongX ? t.Y - minY : t.X - minX;
                data.Tiles.Add(new[] { (int)t.Id, along, across, t.Z - minZ, t.Hue });
            }

            return data;
        }
    }

    public sealed class Prefab
    {
        public string Name { get; set; }

        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// [id, x, y, z, flag, hue, deco] relative to the smallest x / y / z
        /// </summary>
        public List<int[]> Tiles { get; set; } = new List<int[]>();

        public List<ImportTile> ToTiles()
        {
            return Tiles.Where(t => t.Length >= 4).Select(t => new ImportTile
            {
                Id = (ushort)t[0],
                X = t[1],
                Y = t[2],
                Z = t[3],
                Flag = t.Length > 4 ? t[4] : 1,
                Hue = t.Length > 5 ? t[5] : 0,
                Deco = t.Length > 6 && t[6] != 0
            }).ToList();
        }

        public static Prefab FromTiles(string name, string category, IList<ImportTile> tiles)
        {
            var p = new Prefab { Name = name, Category = category ?? string.Empty };
            if (tiles == null || tiles.Count == 0)
            {
                return p;
            }

            int minX = tiles.Min(t => t.X);
            int minY = tiles.Min(t => t.Y);
            int minZ = tiles.Min(t => t.Z);
            foreach (ImportTile t in tiles)
            {
                p.Tiles.Add(new[] { (int)t.Id, t.X - minX, t.Y - minY, t.Z - minZ, t.Flag, t.Hue, t.Deco ? 1 : 0 });
            }

            return p;
        }

        public override string ToString() => string.IsNullOrEmpty(Category) ? Name : $"{Category} / {Name}";
    }

    /// <summary>
    /// Stored building parts and learned line patterns
    /// </summary>
    public sealed class StructureLibrary
    {
        public List<Prefab> Prefabs { get; set; } = new List<Prefab>();

        public List<LinePatternData> LinePatterns { get; set; } = new List<LinePatternData>();

        private static StructureLibrary _instance;

        public static StructureLibrary Instance => _instance ??= Load();

        private static string FilePath => Path.Combine(Options.AppDataPath ?? AppContext.BaseDirectory, "plugins", "MultiEditorStructures.json");

        private static StructureLibrary Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    StructureLibrary l = JsonSerializer.Deserialize<StructureLibrary>(File.ReadAllText(FilePath));
                    if (l != null)
                    {
                        return l;
                    }
                }
            }
            catch
            {
                // ignore
            }

            return new StructureLibrary();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Simple staircases from the learned styles (only if the library has none yet)
        /// </summary>
        public void SeedFromStyles(StyleKnowledge k)
        {
            if (Prefabs.Any(p => p.Category == "Auto"))
            {
                return;
            }

            foreach (HouseStyle s in k.Styles.Where(s => s.Stair > 0).GroupBy(s => s.Stair).Select(g => g.First()))
            {
                var tiles = new List<ImportTile>();
                for (int i = 0; i < 4; ++i)
                {
                    tiles.Add(new ImportTile { Id = (ushort)s.Stair, X = 0, Y = 3 - i, Z = i * 5 });
                }

                Prefabs.Add(Prefab.FromTiles(L.T($"Treppe 0x{s.Stair:X}", $"Stairs 0x{s.Stair:X}"), "Auto", tiles));
            }
        }
    }

    public static class StructureBuilder
    {
        public static readonly IStructurePattern[] BuiltInPatterns =
        {
            new WallLinePattern(),
            new RoomPattern(RoofType.None),
            new RoomPattern(RoofType.Flat),
            new RoomPattern(RoofType.GableX),
            new RoomPattern(RoofType.None, false),
            new RoomPattern(RoofType.Flat, false, false),
            new RoomPattern(RoofType.GableX, false, false)
        };

        /// <summary>
        /// Built in patterns + learned line patterns
        /// </summary>
        public static List<IStructurePattern> GetPatterns()
        {
            var list = new List<IStructurePattern>(BuiltInPatterns);
            list.AddRange(StructureLibrary.Instance.LinePatterns.Select(p => (IStructurePattern)new LearnedLinePattern(p)));
            return list;
        }

        /// <summary>
        /// Z to build on: top of walls / floors in the area (roofs are ignored), null if empty
        /// </summary>
        public static int? DetectTopZ(IEnumerable<MultiTile> tiles)
        {
            int? top = null;
            foreach (MultiTile t in tiles)
            {
                if (t.IsVirtualFloor || t.IsDeco || t.Id >= TileData.ItemTable.Length)
                {
                    continue;
                }

                ItemData d = TileData.ItemTable[t.Id];
                if ((d.Flags & TileFlag.Roof) != 0)
                {
                    continue;
                }

                bool wall = (d.Flags & (TileFlag.Wall | TileFlag.Window)) != 0;
                bool surface = (d.Flags & TileFlag.Surface) != 0;
                if (!wall && !surface)
                {
                    continue;
                }

                int z = t.Z + (wall ? d.Height : d.CalcHeight);
                if (top == null || z > top)
                {
                    top = z;
                }
            }

            return top;
        }

        /// <summary>
        /// Removes generated tiles that already exist (same id, cell and z)
        /// </summary>
        public static List<ImportTile> RemoveExisting(List<ImportTile> tiles, IEnumerable<MultiTile> existing)
        {
            var set = new HashSet<(int, int, int, int)>(existing.Where(t => !t.IsVirtualFloor).Select(t => ((int)t.Id, t.X, t.Y, t.Z)));
            return tiles.Where(t => !set.Contains((t.Id, t.X, t.Y, t.Z))).ToList();
        }

        /// <summary>
        /// Rotates tiles 90° clockwise around their bounding box (stamp rotation)
        /// </summary>
        public static List<ImportTile> RotateCw(IList<ImportTile> tiles, IDictionary<ushort, ushort> swap)
        {
            if (tiles.Count == 0)
            {
                return tiles.ToList();
            }

            int maxY = tiles.Max(t => t.Y);
            return tiles.Select(t =>
            {
                ImportTile c = t.Clone();
                c.X = maxY - t.Y;
                c.Y = t.X;
                if (swap != null && swap.TryGetValue(c.Id, out ushort n))
                {
                    c.Id = n;
                }

                return c;
            }).ToList();
        }
    }
}
