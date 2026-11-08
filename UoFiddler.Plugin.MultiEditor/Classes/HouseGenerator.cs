/***************************************************************************
 *
 * MultiEditor – house generator with style learning
 *
 * Learning: example multis are analysed. Every tile gets a role
 * (wall along X, wall along Y, corner, post, floor, roof slopes, stairs,
 * windows) from its neighbours and the tiledata flags. The most used id per
 * role becomes a "style". All votes are stored, so every new example makes
 * the role detection better (used for rotate / mirror and restyling, too).
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
    public enum TileRole
    {
        None = 0,
        WallX,
        WallY,
        Corner,
        Post,
        Floor,
        RoofNorth,
        RoofSouth,
        RoofWest,
        RoofEast,
        RoofRidge,
        Stair,
        WindowX,
        WindowY
    }

    public sealed class HouseStyle
    {
        public string Name { get; set; } = "Style";
        public string Source { get; set; } = string.Empty;
        public int WallX { get; set; }
        public int WallY { get; set; }
        public int Corner { get; set; }
        public int Post { get; set; }
        public int Floor { get; set; }
        public int RoofNorth { get; set; }
        public int RoofSouth { get; set; }
        public int RoofWest { get; set; }
        public int RoofEast { get; set; }
        public int RoofRidge { get; set; }
        public int Stair { get; set; }
        public int WindowX { get; set; }
        public int WindowY { get; set; }

        /// <summary>
        /// Exact layout learned from the example house (null for old / manual styles)
        /// </summary>
        public HouseTemplate Template { get; set; }

        public bool IsUsable => Template != null || (WallX > 0 && WallY > 0 && Floor > 0);

        public bool HasGableRoofX => RoofNorth > 0 && RoofSouth > 0;

        public bool HasGableRoofY => RoofWest > 0 && RoofEast > 0;

        public int Get(TileRole role)
        {
            return role switch
            {
                TileRole.WallX => WallX,
                TileRole.WallY => WallY,
                TileRole.Corner => Corner,
                TileRole.Post => Post,
                TileRole.Floor => Floor,
                TileRole.RoofNorth => RoofNorth,
                TileRole.RoofSouth => RoofSouth,
                TileRole.RoofWest => RoofWest,
                TileRole.RoofEast => RoofEast,
                TileRole.RoofRidge => RoofRidge,
                TileRole.Stair => Stair,
                TileRole.WindowX => WindowX,
                TileRole.WindowY => WindowY,
                _ => 0
            };
        }

        public void Set(TileRole role, int id)
        {
            switch (role)
            {
                case TileRole.WallX: WallX = id; break;
                case TileRole.WallY: WallY = id; break;
                case TileRole.Corner: Corner = id; break;
                case TileRole.Post: Post = id; break;
                case TileRole.Floor: Floor = id; break;
                case TileRole.RoofNorth: RoofNorth = id; break;
                case TileRole.RoofSouth: RoofSouth = id; break;
                case TileRole.RoofWest: RoofWest = id; break;
                case TileRole.RoofEast: RoofEast = id; break;
                case TileRole.RoofRidge: RoofRidge = id; break;
                case TileRole.Stair: Stair = id; break;
                case TileRole.WindowX: WindowX = id; break;
                case TileRole.WindowY: WindowY = id; break;
            }
        }

        public HouseStyle Clone()
        {
            var c = (HouseStyle)MemberwiseClone();
            if (Template != null)
            {
                c.Template = System.Text.Json.JsonSerializer.Deserialize<HouseTemplate>(System.Text.Json.JsonSerializer.Serialize(Template));
            }

            return c;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class StyleKnowledge
    {
        public List<HouseStyle> Styles { get; set; } = new List<HouseStyle>();

        /// <summary>
        /// id -> role -> votes
        /// </summary>
        public Dictionary<int, Dictionary<TileRole, int>> RoleVotes { get; set; } = new Dictionary<int, Dictionary<TileRole, int>>();

        public int LearnedExamples { get; set; }

        /// <summary>
        /// Roles set by hand (win over the votes)
        /// </summary>
        public Dictionary<int, TileRole> RoleOverrides { get; set; } = new Dictionary<int, TileRole>();

        private static StyleKnowledge _instance;

        public static StyleKnowledge Instance => _instance ??= Load();

        private static string FilePath => Path.Combine(Options.AppDataPath ?? AppContext.BaseDirectory, "plugins", "MultiEditorStyles.json");

        private static StyleKnowledge Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    StyleKnowledge k = JsonSerializer.Deserialize<StyleKnowledge>(File.ReadAllText(FilePath));
                    if (k != null)
                    {
                        return k;
                    }
                }
            }
            catch
            {
                // ignore
            }

            return new StyleKnowledge();
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

        public void Reset()
        {
            Styles.Clear();
            RoleVotes.Clear();
            RoleOverrides?.Clear();
            LearnedExamples = 0;
        }

        private void Vote(int id, TileRole role, int weight = 1)
        {
            if (!RoleVotes.TryGetValue(id, out Dictionary<TileRole, int> votes))
            {
                votes = new Dictionary<TileRole, int>();
                RoleVotes[id] = votes;
            }

            votes.TryGetValue(role, out int n);
            votes[role] = n + weight;
        }

        /// <summary>
        /// Most voted role of a tile; falls back to tiledata / art analysis
        /// </summary>
        public TileRole GetRole(int id)
        {
            if (RoleOverrides != null && RoleOverrides.TryGetValue(id, out TileRole forced))
            {
                return forced;
            }

            if (RoleVotes.TryGetValue(id, out Dictionary<TileRole, int> votes) && votes.Count > 0)
            {
                return votes.OrderByDescending(v => v.Value).First().Key;
            }

            return GuessRole(id);
        }

        /// <summary>
        /// Role from tiledata flags and the shape of the art
        /// </summary>
        public static TileRole GuessRole(int id)
        {
            if (id < 0 || id >= TileData.ItemTable.Length)
            {
                return TileRole.None;
            }

            TileFlag flags = TileData.ItemTable[id].Flags;
            if ((flags & (TileFlag.StairBack | TileFlag.StairRight)) != 0)
            {
                return TileRole.Stair;
            }

            if ((flags & TileFlag.Roof) != 0)
            {
                return TileRole.RoofRidge;
            }

            bool wall = (flags & TileFlag.Wall) != 0;
            bool window = (flags & TileFlag.Window) != 0;
            if (wall || window)
            {
                TileRole r = GuessWallOrientation(id);
                if (window)
                {
                    return r == TileRole.WallY ? TileRole.WindowY : TileRole.WindowX;
                }

                return r;
            }

            if ((flags & TileFlag.Surface) != 0 && TileData.ItemTable[id].Height <= 3)
            {
                return TileRole.Floor;
            }

            return TileRole.None;
        }

        /// <summary>
        /// Walls along X are drawn in the right half of the art, walls along Y in the left half
        /// </summary>
        public static TileRole GuessWallOrientation(int id)
        {
            Bitmap bmp = Art.GetStatic(id);
            if (bmp == null)
            {
                return TileRole.None;
            }

            if (bmp.Width <= 26)
            {
                return TileRole.Post;
            }

            int left = 0;
            int right = 0;
            int mid = bmp.Width / 2;
            int startY = Math.Max(0, bmp.Height - 30);
            for (int y = startY; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    Color c = bmp.GetPixel(x, y);
                    if (c.A == 0 || (c.R == 0 && c.G == 0 && c.B == 0))
                    {
                        continue;
                    }

                    if (x < mid - 3)
                    {
                        ++left;
                    }
                    else if (x > mid + 3)
                    {
                        ++right;
                    }
                }
            }

            if (left == 0 && right == 0)
            {
                return TileRole.None;
            }

            double ratio = (right + 1.0) / (left + 1.0);
            if (ratio > 2.0)
            {
                return TileRole.WallX;
            }

            if (ratio < 0.5)
            {
                return TileRole.WallY;
            }

            return TileRole.Corner;
        }

        /// <summary>
        /// Learns from one example. Returns the detected style (may be unusable if no house parts were found).
        /// </summary>
        public HouseStyle Learn(IList<ImportTile> tiles, string name)
        {
            var style = new HouseStyle { Name = name, Source = name };
            if (tiles == null || tiles.Count == 0)
            {
                return style;
            }

            var byCell = new Dictionary<(int, int), List<ImportTile>>();
            foreach (ImportTile t in tiles)
            {
                if (t.Id >= TileData.ItemTable.Length)
                {
                    continue;
                }

                if (!byCell.TryGetValue((t.X, t.Y), out List<ImportTile> l))
                {
                    l = new List<ImportTile>();
                    byCell[(t.X, t.Y)] = l;
                }

                l.Add(t);
            }

            bool IsFlag(ImportTile t, TileFlag f) => t.Id < TileData.ItemTable.Length && (TileData.ItemTable[t.Id].Flags & f) != 0;

            bool HasWallAt(int x, int y, int z)
            {
                return byCell.TryGetValue((x, y), out List<ImportTile> l) &&
                       l.Any(o => Math.Abs(o.Z - z) <= 3 && (IsFlag(o, TileFlag.Wall) || IsFlag(o, TileFlag.Window)));
            }

            int? RoofZAt(int x, int y, int z)
            {
                if (!byCell.TryGetValue((x, y), out List<ImportTile> l))
                {
                    return null;
                }

                ImportTile r = l.Where(o => IsFlag(o, TileFlag.Roof) && Math.Abs(o.Z - z) <= 6).OrderBy(o => Math.Abs(o.Z - z)).FirstOrDefault();
                return r?.Z;
            }

            var counts = new Dictionary<TileRole, Dictionary<int, int>>();

            void Count(TileRole role, int id)
            {
                if (!counts.TryGetValue(role, out Dictionary<int, int> d))
                {
                    d = new Dictionary<int, int>();
                    counts[role] = d;
                }

                d.TryGetValue(id, out int n);
                d[id] = n + 1;
                Vote(id, role);
            }

            foreach (ImportTile t in tiles)
            {
                if (t.Id == 0x1 || t.Id >= TileData.ItemTable.Length)
                {
                    continue;
                }

                ItemData data = TileData.ItemTable[t.Id];
                TileFlag f = data.Flags;

                if ((f & (TileFlag.StairBack | TileFlag.StairRight)) != 0)
                {
                    Count(TileRole.Stair, t.Id);
                    continue;
                }

                if ((f & TileFlag.Roof) != 0)
                {
                    int? n = RoofZAt(t.X, t.Y - 1, t.Z);
                    int? s = RoofZAt(t.X, t.Y + 1, t.Z);
                    int? w = RoofZAt(t.X - 1, t.Y, t.Z);
                    int? e = RoofZAt(t.X + 1, t.Y, t.Z);
                    TileRole role;
                    if (s > t.Z && !(n > t.Z))
                    {
                        role = TileRole.RoofNorth; // rises to the south -> north slope
                    }
                    else if (n > t.Z && !(s > t.Z))
                    {
                        role = TileRole.RoofSouth;
                    }
                    else if (e > t.Z && !(w > t.Z))
                    {
                        role = TileRole.RoofWest;
                    }
                    else if (w > t.Z && !(e > t.Z))
                    {
                        role = TileRole.RoofEast;
                    }
                    else
                    {
                        role = TileRole.RoofRidge;
                    }

                    Count(role, t.Id);
                    continue;
                }

                bool isWall = (f & TileFlag.Wall) != 0;
                bool isWindow = (f & TileFlag.Window) != 0;
                if (isWall || isWindow)
                {
                    bool hasX = HasWallAt(t.X - 1, t.Y, t.Z) || HasWallAt(t.X + 1, t.Y, t.Z);
                    bool hasY = HasWallAt(t.X, t.Y - 1, t.Z) || HasWallAt(t.X, t.Y + 1, t.Z);
                    TileRole role;
                    if (hasX && !hasY)
                    {
                        role = isWindow ? TileRole.WindowX : TileRole.WallX;
                    }
                    else if (hasY && !hasX)
                    {
                        role = isWindow ? TileRole.WindowY : TileRole.WallY;
                    }
                    else if (hasX)
                    {
                        // both: corner (north-west) when walls continue to +X and +Y
                        bool plusX = HasWallAt(t.X + 1, t.Y, t.Z);
                        bool plusY = HasWallAt(t.X, t.Y + 1, t.Z);
                        role = plusX && plusY && !HasWallAt(t.X - 1, t.Y, t.Z) && !HasWallAt(t.X, t.Y - 1, t.Z)
                            ? TileRole.Corner
                            : GuessWallOrientation(t.Id);
                    }
                    else
                    {
                        role = TileRole.Post;
                    }

                    if (role == TileRole.None)
                    {
                        continue;
                    }

                    Count(role, t.Id);
                    continue;
                }

                if ((f & TileFlag.Surface) != 0 && data.Height <= 3)
                {
                    Count(TileRole.Floor, t.Id);
                }
            }

            foreach (KeyValuePair<TileRole, Dictionary<int, int>> kv in counts)
            {
                int best = kv.Value.OrderByDescending(v => v.Value).First().Key;
                style.Set(kv.Key, best);
            }

            // a window is not a good wall
            if (style.WallX == 0 && style.WindowX > 0)
            {
                style.WallX = style.WindowX;
            }

            if (style.WallY == 0 && style.WindowY > 0)
            {
                style.WallY = style.WindowY;
            }

            if (style.Corner == 0)
            {
                style.Corner = style.WallX;
            }

            if (style.Post == 0)
            {
                style.Post = style.Corner;
            }

            ++LearnedExamples;

            try
            {
                style.Template = HouseTemplate.Learn(tiles);
            }
            catch
            {
                style.Template = null;
            }

            if (style.Template != null)
            {
                HouseTemplate t = style.Template;
                if (style.Floor <= 0)
                {
                    style.Floor = t.FloorId;
                }
            }

            if (!style.IsUsable)
            {
                return style;
            }

            HouseStyle existing = Styles.FirstOrDefault(s => SameStyle(s, style));
            if (existing == null)
            {
                Styles.Add(style);
            }
            else if (existing.Template == null && style.Template != null)
            {
                existing.Template = style.Template;
            }

            return style;
        }

        private static bool SameStyle(HouseStyle a, HouseStyle b)
        {
            return a.WallX == b.WallX && a.WallY == b.WallY && a.Floor == b.Floor && a.RoofNorth == b.RoofNorth && a.RoofSouth == b.RoofSouth;
        }

        /// <summary>
        /// WallX &lt;-&gt; WallY etc. for rotation, built from all styles
        /// </summary>
        public Dictionary<ushort, ushort> GetRotationSwapMap()
        {
            var map = new Dictionary<ushort, ushort>();

            void Pair(int a, int b)
            {
                if (a <= 0 || b <= 0 || a == b)
                {
                    return;
                }

                map.TryAdd((ushort)a, (ushort)b);
                map.TryAdd((ushort)b, (ushort)a);
            }

            foreach (HouseStyle s in Styles)
            {
                Pair(s.WallX, s.WallY);
                Pair(s.WindowX, s.WindowY);
                Pair(s.RoofNorth, s.RoofWest);
                Pair(s.RoofSouth, s.RoofEast);
            }

            return map;
        }

        /// <summary>
        /// Builds an id map that replaces the parts of the current design with the parts of the target style
        /// </summary>
        public Dictionary<ushort, ushort> GetRestyleMap(IEnumerable<int> usedIds, HouseStyle target)
        {
            var map = new Dictionary<ushort, ushort>();
            foreach (int id in usedIds.Distinct())
            {
                TileRole role = GetRole(id);
                int newId = target.Get(role);
                if (newId > 0 && newId != id)
                {
                    map[(ushort)id] = (ushort)newId;
                }
            }

            return map;
        }
    }

    public enum RoofType
    {
        None,
        Flat,
        GableX,
        GableY,
        FromTemplate
    }

    public sealed class HouseGeneratorOptions
    {
        public int Width { get; set; } = 7;
        public int Depth { get; set; } = 7;
        public int Floors { get; set; } = 1;
        public int FloorHeight { get; set; } = 20;
        public int BaseZ { get; set; }
        public RoofType Roof { get; set; } = RoofType.GableX;
        public bool DoorGap { get; set; } = true;
        public bool Windows { get; set; } = true;
        public int WindowSpacing { get; set; } = 3;
        public bool Stairs { get; set; } = true;
        public bool InnerWall { get; set; }
        public int RoofStep { get; set; } = 3;
        public HouseStyle Style { get; set; }

        /// <summary>
        /// Building parts placed into the house
        /// </summary>
        public List<PrefabPlacement> Parts { get; set; } = new List<PrefabPlacement>();
    }

    public sealed class PrefabPlacement
    {
        public string Prefab { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Floor { get; set; }
        public int Rotation { get; set; }
        public int Dz { get; set; }
    }

    public static class HouseGenerator
    {
        /// <summary>
        /// Generates tiles in cell coordinates 0..Width / 0..Depth (walls on the outer lines)
        /// </summary>
        public static List<ImportTile> Generate(HouseGeneratorOptions o)
        {
            List<ImportTile> tiles = GenerateCore(o);
            if (tiles.Count == 0)
            {
                return tiles;
            }

            // building parts
            int floorHeight = o.Style?.Template?.FloorHeight ?? o.FloorHeight;
            foreach (PrefabPlacement p in o.Parts)
            {
                Prefab prefab = StructureLibrary.Instance.Prefabs.FirstOrDefault(x => x.Name == p.Prefab);
                if (prefab == null)
                {
                    continue;
                }

                List<ImportTile> part = prefab.ToTiles();
                for (int i = 0; i < ((p.Rotation % 4) + 4) % 4; ++i)
                {
                    part = StructureBuilder.RotateCw(part, StyleKnowledge.Instance.GetRotationSwapMap());
                }

                int z = o.BaseZ + (p.Floor * floorHeight) + p.Dz;
                foreach (ImportTile t in part)
                {
                    t.X += p.X;
                    t.Y += p.Y;
                    t.Z += z;
                    tiles.Add(t);
                }
            }

            // cells start at 0,0
            int minX = tiles.Min(t => t.X);
            int minY = tiles.Min(t => t.Y);
            if (minX != 0 || minY != 0)
            {
                foreach (ImportTile t in tiles)
                {
                    t.X -= minX;
                    t.Y -= minY;
                }
            }

            return tiles;
        }

        private static List<ImportTile> GenerateCore(HouseGeneratorOptions o)
        {
            var tiles = new List<ImportTile>();
            HouseStyle s = o.Style;
            if (s == null || !s.IsUsable)
            {
                return tiles;
            }

            if (s.Template != null)
            {
                tiles = s.Template.Generate(o);
                if (o.Stairs && s.Stair > 0 && o.Floors > 1)
                {
                    AddTemplateStairs(tiles, o, s);
                }

                return tiles;
            }

            if (o.Roof == RoofType.FromTemplate)
            {
                o.Roof = s.HasGableRoofX ? RoofType.GableX : RoofType.Flat;
            }

            int w = Math.Max(2, o.Width);
            int h = Math.Max(2, o.Depth);
            int floors = Math.Max(1, o.Floors);

            void Add(int id, int x, int y, int z)
            {
                if (id > 0)
                {
                    tiles.Add(new ImportTile { Id = (ushort)id, X = x, Y = y, Z = z, Flag = 1 });
                }
            }

            int doorX = w / 2;
            var stairCells = new HashSet<(int, int)>();
            int stairX = 1;
            int stairSteps = Math.Max(1, o.FloorHeight / 5);

            for (int f = 0; f < floors; ++f)
            {
                int z = o.BaseZ + (f * o.FloorHeight);

                // floor (on upper floors: leave the stair opening free)
                for (int x = 0; x < w; ++x)
                {
                    for (int y = 0; y < h; ++y)
                    {
                        if (f > 0 && stairCells.Contains((x, y)))
                        {
                            continue;
                        }

                        Add(s.Floor, x, y, z);
                    }
                }

                bool windowFloor = o.Windows && (floors == 1 || f > 0 || w > 6);

                int WallXId(int i) => windowFloor && s.WindowX > 0 && i % o.WindowSpacing == o.WindowSpacing - 1 ? s.WindowX : s.WallX;
                int WallYId(int i) => windowFloor && s.WindowY > 0 && i % o.WindowSpacing == o.WindowSpacing - 1 ? s.WindowY : s.WallY;

                Add(s.Corner > 0 ? s.Corner : s.WallX, 0, 0, z);
                for (int x = 1; x < w; ++x)
                {
                    Add(WallXId(x), x, 0, z); // north
                }

                for (int y = 1; y < h; ++y)
                {
                    Add(WallYId(y), 0, y, z); // west
                }

                for (int x = 0; x < w; ++x)
                {
                    if (f == 0 && o.DoorGap && x == doorX)
                    {
                        continue;
                    }

                    Add(f == 0 ? s.WallX : WallXId(x), x, h, z); // south
                }

                for (int y = 0; y < h; ++y)
                {
                    Add(WallYId(y), w, y, z); // east
                }

                Add(s.Post > 0 ? s.Post : s.Corner, w, h, z);

                if (o.InnerWall && w >= 6)
                {
                    int ix = w / 2;
                    for (int y = 1; y < h; ++y)
                    {
                        if (y == h / 2)
                        {
                            continue; // passage
                        }

                        Add(s.WallY, ix, y, z);
                    }
                }

                // stairs to the next floor
                if (f < floors - 1 && o.Stairs && s.Stair > 0 && h >= stairSteps + 2)
                {
                    stairCells.Clear();
                    for (int i = 0; i < stairSteps; ++i)
                    {
                        int y = h - 2 - i;
                        Add(s.Stair, stairX, y, z + (i * 5));
                        stairCells.Add((stairX, y));
                    }
                }
            }

            int top = o.BaseZ + (floors * o.FloorHeight);
            switch (o.Roof)
            {
                case RoofType.Flat:
                    for (int x = 0; x < w; ++x)
                    {
                        for (int y = 0; y < h; ++y)
                        {
                            Add(s.Floor, x, y, top);
                        }
                    }

                    break;
                case RoofType.GableX when s.HasGableRoofX:
                    for (int y = 0; y <= h; ++y)
                    {
                        int fromNorth = y;
                        int fromSouth = h - y;
                        int dist = Math.Min(fromNorth, fromSouth);
                        int id = fromNorth < fromSouth ? s.RoofNorth : fromSouth < fromNorth ? s.RoofSouth : (s.RoofRidge > 0 ? s.RoofRidge : s.RoofNorth);
                        for (int x = 0; x <= w; ++x)
                        {
                            Add(id, x, y, top + (dist * o.RoofStep));
                        }
                    }

                    break;
                case RoofType.GableY when s.HasGableRoofY:
                    for (int x = 0; x <= w; ++x)
                    {
                        int fromWest = x;
                        int fromEast = w - x;
                        int dist = Math.Min(fromWest, fromEast);
                        int id = fromWest < fromEast ? s.RoofWest : fromEast < fromWest ? s.RoofEast : (s.RoofRidge > 0 ? s.RoofRidge : s.RoofWest);
                        for (int y = 0; y <= h; ++y)
                        {
                            Add(id, x, y, top + (dist * o.RoofStep));
                        }
                    }

                    break;
                case RoofType.GableX:
                case RoofType.GableY:
                    // style has no matching roof: flat
                    for (int x = 0; x < w; ++x)
                    {
                        for (int y = 0; y < h; ++y)
                        {
                            Add(s.Floor, x, y, top);
                        }
                    }

                    break;
            }

            return tiles;
        }

        /// <summary>
        /// Straight stairs along the west wall up to every next floor, the upper floor gets an opening
        /// </summary>
        private static void AddTemplateStairs(List<ImportTile> tiles, HouseGeneratorOptions o, HouseStyle s)
        {
            int fh = s.Template.FloorHeight;
            int steps = Math.Max(1, fh / 5);
            if (o.Depth < steps + 2)
            {
                return;
            }

            for (int f = 0; f < o.Floors - 1; ++f)
            {
                int z = o.BaseZ + (f * fh);
                for (int i = 0; i < steps; ++i)
                {
                    int x = 1;
                    int y = o.Depth - 2 - i;
                    tiles.Add(new ImportTile { Id = (ushort)s.Stair, X = x, Y = y, Z = z + (i * 5) });
                    int upper = z + fh;
                    tiles.RemoveAll(t => t.X == x && t.Y == y && t.Z == upper && HouseTemplate.IsFloorTile(t.Id));
                }
            }
        }

        /// <summary>
        /// Random house from the learned styles
        /// </summary>
        public static HouseGeneratorOptions Randomize(Random rnd, StyleKnowledge knowledge)
        {
            List<HouseStyle> usable = knowledge.Styles.Where(s => s.IsUsable).ToList();
            var o = new HouseGeneratorOptions
            {
                Width = rnd.Next(5, 15),
                Depth = rnd.Next(5, 15),
                Floors = rnd.Next(1, 4),
                Windows = rnd.Next(2) == 0,
                InnerWall = rnd.Next(3) == 0,
                Style = usable.Count > 0 ? usable[rnd.Next(usable.Count)] : null
            };
            o.Roof = o.Style != null && o.Style.HasGableRoofX && rnd.Next(3) > 0 ? RoofType.GableX : RoofType.Flat;
            return o;
        }
    }
}
