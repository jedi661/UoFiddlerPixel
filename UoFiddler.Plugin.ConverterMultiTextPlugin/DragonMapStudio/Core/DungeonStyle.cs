/***************************************************************************
 *
 *  * Dragon Map Studio – dungeon style: learned from a real dungeon, used by the dungeon generator
 *  *
 *  *  UO dungeons (Deceit, Doom, Shame …) are built like this: the floor is land with a dungeon texture, around it
 *  *  black void land, the walls are statics standing at the border floor ↔ void at the height of the floor, plus
 *  *  decoration (webs, bones, torches) on the floor. Which wall piece stands where depends on which neighbours are
 *  *  floor – exactly that is learned here: for every cell "is it floor" + "which of N / E / S / W are floor" the
 *  *  stacks of statics found there (id + height above the floor). The generator puts the same stacks into the same
 *  *  situations, so a generated dungeon looks like the original.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>Statics found in one situation: cell is floor or not, floor neighbours N=1 E=2 S=4 W=8.</summary>
    public sealed class WallRule
    {
        public bool OnFloor { get; set; }
        public int Mask { get; set; }

        /// <summary>Stacks of statics as "id@dz id@dz …" (dz = height above the floor), one per sample cell.</summary>
        public List<string> Stacks { get; set; } = new List<string>();

        /// <summary>How many cells of the sample had this situation (with or without statics).</summary>
        public int Cells { get; set; }

        /// <summary>How many of them had statics (the stacks are only a sample of them).</summary>
        public int Filled { get; set; }

        /// <summary>Things hanging on the wall in this situation (torches, paintings, tapestries) as stacks.</summary>
        public List<string> Extras { get; set; } = new List<string>();

        /// <summary>How many cells of the situation had such things.</summary>
        public int ExtraFilled { get; set; }

        /// <summary>Special masks of inner walls (wall statics on floor land with floor all around).</summary>
        public const int InnerAlongX = 256;
        public const int InnerAlongY = 512;
        public const int InnerPost = 768;

        public override string ToString() => $"{(OnFloor ? "Boden" : "Leere")} N{(Mask & 1) > 0} O{(Mask & 2) > 0} S{(Mask & 4) > 0} W{(Mask & 8) > 0}: {Stacks.Count}/{Cells}";
    }

    /// <summary>Look of a dungeon, learned from an area of a map (or written by hand).</summary>
    public sealed class DungeonLook
    {
        [Category("Stil"), DisplayName("Name")]
        public string Name { get; set; } = "Dungeon";

        [Category("Land"), DisplayName("Void-Land (hex)"), Description("Schwarzes Land außerhalb der Gänge (z. B. 0244). Leer = das Land außerhalb bleibt.")]
        public string VoidLand { get; set; } = "0244";

        [Category("Land"), DisplayName("Boden-Land (hex)"), Description("Land-Tiles des Bodens (Dungeon-Textur), mehrere = zufällig.")]
        public string FloorLand { get; set; } = string.Empty;

        [Category("Statics"), DisplayName("Boden-Statics"), Description("Statics auf dem Boden als 'id@dz' (mehrere = zufällig), leer = keine (Boden ist Land).")]
        public string FloorStatics { get; set; } = string.Empty;

        [Category("Statics"), DisplayName("Deko je Bodenfeld (%)"), Description("So oft liegt auf einem Bodenfeld Deko (Spinnweben, Knochen …) wie im Original.")]
        public double DecorPercent { get; set; }

        [Browsable(false)]
        public List<string> Decor { get; set; } = new List<string>();

        [Category("Statics"), DisplayName("Einrichtung je Wandfeld (%)"), Description("So oft steht an einem Bodenfeld neben der Wand Einrichtung (Tische, Stühle, Kisten, Steine) wie im gelernten Dungeon.")]
        public double FurniturePercent { get; set; }

        /// <summary>Furniture learned from the original ("id@dz" stacks), set next to walls.</summary>
        [Browsable(false)]
        public List<string> Furniture { get; set; } = new List<string>();

        [Browsable(false)]
        public List<WallRule> Walls { get; set; } = new List<WallRule>();

        [Category("Stil"), DisplayName("Gelernt aus"), ReadOnly(true)]
        public string Source { get; set; } = string.Empty;

        public override string ToString() => Name;

        public static List<(ushort id, int dz)> ParseStack(string stack)
        {
            var list = new List<(ushort, int)>();
            foreach (string part in (stack ?? string.Empty).Split(new[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] t = part.Split('@');
                if (t.Length >= 1 && ushort.TryParse(t[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort id))
                {
                    int dz = t.Length > 1 && int.TryParse(t[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
                    list.Add((id, dz));
                }
            }
            return list;
        }
    }

    public static class DungeonStyleTools
    {
        private const int MaxStacks = 24;

        /// <summary>
        /// Floor of a sample area: every land tile that is not void (the most common tile at the border of the area,
        /// or the given void tiles) and not NoDraw.
        /// </summary>
        public static HashSet<ushort> GuessVoid(MapData map, Rectangle area)
        {
            var count = new Dictionary<ushort, int>();
            void Add(int x, int y)
            {
                if (map.InBounds(x, y))
                {
                    ushort id = map.LandId(x, y);
                    count[id] = count.TryGetValue(id, out int c) ? c + 1 : 1;
                }
            }
            for (int x = area.Left; x < area.Right; x++)
            {
                Add(x, area.Top);
                Add(x, area.Bottom - 1);
            }
            for (int y = area.Top; y < area.Bottom; y++)
            {
                Add(area.Left, y);
                Add(area.Right - 1, y);
            }
            var set = new HashSet<ushort> { 0x0002 };
            if (count.Count > 0)
            {
                set.Add(count.OrderByDescending(kv => kv.Value).First().Key);
            }
            return set;
        }

        /// <summary>
        /// Situation of a cell: floor neighbours N=1 E=2 S=4 W=8; a cell that touches the floor only diagonally (outer corner)
        /// gets the diagonal bits NE=16 SE=32 SW=64 NW=128 instead.
        /// </summary>
        public static int Mask(Func<int, int, bool> floor, int x, int y)
        {
            int m = (floor(x, y - 1) ? 1 : 0) | (floor(x + 1, y) ? 2 : 0) | (floor(x, y + 1) ? 4 : 0) | (floor(x - 1, y) ? 8 : 0);
            if (m == 0)
            {
                m = (floor(x + 1, y - 1) ? 16 : 0) | (floor(x + 1, y + 1) ? 32 : 0) | (floor(x - 1, y + 1) ? 64 : 0) | (floor(x - 1, y - 1) ? 128 : 0);
            }
            return m;
        }

        /// <summary>Height of the floor near a cell (own land when floor, else the average of the floor neighbours).</summary>
        private static int FloorZ(MapData map, Func<int, int, bool> floor, int x, int y)
        {
            if (floor(x, y))
            {
                return map.LandZ(x, y);
            }
            int sum = 0, n = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (floor(x + dx, y + dy))
                    {
                        sum += map.LandZ(x + dx, y + dy);
                        n++;
                    }
                }
            }
            return n == 0 ? map.LandZ(x, y) : (int)Math.Round(sum / (double)n);
        }

        /// <summary>Learns the look of the dungeon in the area and writes a report (log).</summary>
        public static (DungeonLook look, string report) Analyze(MapData map, Rectangle area, HashSet<ushort> voidTiles, string name, string source)
        {
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            voidTiles ??= GuessVoid(map, area);
            bool Floor(int x, int y) => map.InBounds(x, y) && area.Contains(x, y) && !voidTiles.Contains(map.LandId(x, y));
            var look = new DungeonLook
            {
                Name = name,
                Source = source,
                VoidLand = string.Join(" ", voidTiles.Where(v => v != 0x0002).Select(v => v.ToString("x4")))
            };
            var floorLand = new Dictionary<ushort, int>();
            var floorZ = new Dictionary<int, int>();
            var floorStatics = new Dictionary<string, int>();
            var rules = new Dictionary<(bool, int), WallRule>();
            var idCount = new Dictionary<(ushort id, int dz, string where), int>();
            int openCells = 0, decorCells = 0, wallStacks = 0, innerCells = 0;
            int nearWallCells = 0, furnitureCells = 0;
            var furnitureList = new List<string>();
            var decor = new List<string>();
            var rnd = new Random(1);
            WallRule Rule(bool onFloor, int mask)
            {
                if (!rules.TryGetValue((onFloor, mask), out WallRule r))
                {
                    rules[(onFloor, mask)] = r = new WallRule { OnFloor = onFloor, Mask = mask };
                }
                return r;
            }
            void Sample(List<string> list, string stack)
            {
                if (list.Count < MaxStacks)
                {
                    list.Add(stack);
                }
                else
                {
                    list[rnd.Next(MaxStacks)] = stack; // keep a random sample
                }
            }
            // walls on floor land (inner walls) make a cell not open
            bool HasWall(int x, int y) => map.StaticsAt(x, y).Any(st => Role(st.Id, 0, true) == "Wand");
            bool Open(int x, int y) => Floor(x, y) && !HasWall(x, y);
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    bool floor = Floor(x, y);
                    int mask = Mask(Floor, x, y);
                    bool edge = floor ? mask != 15 : mask != 0;
                    int fz = FloorZ(map, Floor, x, y);
                    List<StaticItem> statics = map.StaticsAt(x, y).OrderBy(st => st.Z).ToList();
                    if (floor)
                    {
                        ushort land = map.LandId(x, y);
                        floorLand[land] = floorLand.TryGetValue(land, out int c) ? c + 1 : 1;
                        floorZ[fz] = floorZ.TryGetValue(fz, out int cz) ? cz + 1 : 1;
                    }
                    if (!floor && !edge)
                    {
                        continue; // deep void: nothing to learn
                    }
                    var wallPart = new List<string>();
                    var hung = new List<string>();
                    var loose = new List<string>();
                    var furniture = new List<string>();
                    foreach (StaticItem st in statics)
                    {
                        int dz = st.Z - fz;
                        string role = floor ? Role(st.Id, dz, false) : (IsHung(st.Id, dz) ? "Wand-Deko" : "Wand");
                        string entry = $"{st.Id:x4}@{dz}";
                        switch (role)
                        {
                            case "Wand":
                                wallPart.Add(entry);
                                break;
                            case "Boden":
                                floorStatics[entry] = floorStatics.TryGetValue(entry, out int fc) ? fc + 1 : 1;
                                break;
                            case "Wand-Deko":
                                hung.Add(entry);
                                break;
                            case "Deko":
                                loose.Add(entry);
                                break;
                            case "Einrichtung":
                                furniture.Add(entry);
                                break;
                        }
                        var key = (st.Id, dz, role);
                        idCount[key] = idCount.TryGetValue(key, out int ic) ? ic + 1 : 1;
                    }
                    // inner wall: wall statics on floor land with floor all around – the direction comes from the open neighbours
                    bool inner = floor && !edge && wallPart.Count > 0;
                    if (inner)
                    {
                        innerCells++;
                        bool ns = Open(x, y - 1) || Open(x, y + 1), ew = Open(x - 1, y) || Open(x + 1, y);
                        WallRule r = Rule(true, ns && !ew ? WallRule.InnerAlongX : ew && !ns ? WallRule.InnerAlongY : WallRule.InnerPost);
                        r.Cells++;
                        r.Filled++;
                        wallStacks++;
                        Sample(r.Stacks, string.Join(" ", wallPart));
                        if (hung.Count > 0)
                        {
                            r.ExtraFilled++;
                            Sample(r.Extras, string.Join(" ", hung));
                        }
                        continue;
                    }
                    if (edge)
                    {
                        WallRule rule = Rule(floor, mask);
                        rule.Cells++;
                        if (wallPart.Count > 0)
                        {
                            wallStacks++;
                            rule.Filled++;
                            Sample(rule.Stacks, string.Join(" ", wallPart));
                        }
                        if (hung.Count > 0)
                        {
                            rule.ExtraFilled++;
                            Sample(rule.Extras, string.Join(" ", hung));
                        }
                    }
                    if (floor && wallPart.Count == 0 && Mask(Floor, x, y) != 15)
                    {
                        nearWallCells++;
                        if (furniture.Count > 0)
                        {
                            furnitureCells++;
                            if (furnitureList.Count < 300)
                            {
                                furnitureList.Add(string.Join(" ", furniture));
                            }
                        }
                    }
                    if (floor && wallPart.Count == 0)
                    {
                        openCells++;
                        if (loose.Count > 0)
                        {
                            decorCells++;
                            if (decor.Count < 300)
                            {
                                decor.Add(string.Join(" ", loose));
                            }
                        }
                    }
                }
            }
            look.FloorLand = string.Join(" ", floorLand.OrderByDescending(kv => kv.Value).Where(kv => kv.Value * 50 >= floorLand.Values.Sum()).Take(12).Select(kv => kv.Key.ToString("x4")));
            look.FloorStatics = string.Join(" ", floorStatics.Where(kv => kv.Value * 10 >= openCells).OrderByDescending(kv => kv.Value).Take(8).Select(kv => kv.Key));
            look.Walls = rules.Values.Where(r => r.Cells > 0).OrderBy(r => r.OnFloor).ThenBy(r => r.Mask).ToList();
            look.Decor = decor;
            look.Furniture = furnitureList;
            look.FurniturePercent = nearWallCells == 0 ? 0 : Math.Round(furnitureCells * 100.0 / nearWallCells, 1);
            look.DecorPercent = openCells == 0 ? 0 : Math.Round(decorCells * 100.0 / openCells, 1);
            int floorCells = openCells;

            // ---------------------------------------------------------- report
            var sb = new StringBuilder();
            sb.AppendLine("DUNGEON-ANALYSE / DUNGEON ANALYSIS – Dragon Map Studio");
            sb.AppendLine(new string('=', 60));
            sb.AppendLine($"Quelle / source: {source}");
            sb.AppendLine($"Bereich / area: {area.X},{area.Y} {area.Width}×{area.Height}");
            sb.AppendLine($"Void-Land (außen / outside): {string.Join(" ", voidTiles.Select(v => $"{v:x4} {UoArtCache.LandName(v)}"))}");
            sb.AppendLine($"Offene Bodenfelder / open floor cells: {floorCells}, Wand-Stapel / wall stacks: {wallStacks} (davon Innenwände / inner walls {innerCells}), lose Deko / loose decor: {decorCells} ({look.DecorPercent} %)");
            sb.AppendLine("Rollen / roles: Wand = Wand-Flag oder auf der Leere · Wand-Deko = Fackeln, Bilder, Teppiche (an der Wand) · Deko = lose (Schutt, Blut, Knochen, Spinnweben) · Einrichtung = Tische, Stühle, Steine, Treppen (wird an Wänden gesetzt, so oft wie im Original) · Boden = flache Boden-Statics");
            sb.AppendLine();
            sb.AppendLine("BODEN-LAND (Textur) / FLOOR LAND:");
            foreach (var kv in floorLand.OrderByDescending(kv => kv.Value).Take(20))
            {
                sb.AppendLine($"  {kv.Key:x4}  {UoArtCache.LandName(kv.Key),-24} {kv.Value,6}×   Textur {UoArtCache.LandTextureId(kv.Key):x4}");
            }
            sb.AppendLine("BODEN-HÖHEN / FLOOR HEIGHTS:");
            foreach (var kv in floorZ.OrderByDescending(kv => kv.Value).Take(10))
            {
                sb.AppendLine($"  z {kv.Key,4}  {kv.Value,6}×");
            }
            sb.AppendLine();
            sb.AppendLine("STATICS (id, Höhe über dem Boden / height above floor, Rolle / role, Anzahl / count):");
            foreach (var kv in idCount.OrderBy(kv => kv.Key.where).ThenByDescending(kv => kv.Value).Take(120))
            {
                sb.AppendLine($"  {kv.Key.id:x4} dz {kv.Key.dz,4}  {kv.Key.where,-5} {kv.Value,6}×  {UoArtCache.ItemName(kv.Key.id)} ({UoArtCache.ItemFlagsText(kv.Key.id)})");
            }
            sb.AppendLine();
            sb.AppendLine("WIE DIE WÄNDE STEHEN / HOW THE WALLS STAND");
            sb.AppendLine("  Feld = Boden oder Leere, dahinter welche Nachbarn Boden sind (N O S W) → häufigste Stapel 'id@dz'");
            foreach (WallRule r in look.Walls)
            {
                string n = r.Mask >= 256
                    ? (r.Mask == WallRule.InnerAlongX ? "Innen W-O" : r.Mask == WallRule.InnerAlongY ? "Innen N-S" : "Innen Ecke")
                    : r.Mask >= 16
                    ? $"Ecke {((r.Mask & 16) > 0 ? "NO" : "")}{((r.Mask & 32) > 0 ? "SO" : "")}{((r.Mask & 64) > 0 ? "SW" : "")}{((r.Mask & 128) > 0 ? "NW" : "")}"
                    : $"{((r.Mask & 1) > 0 ? "N" : "-")}{((r.Mask & 2) > 0 ? "O" : "-")}{((r.Mask & 4) > 0 ? "S" : "-")}{((r.Mask & 8) > 0 ? "W" : "-")}";
                var top = r.Stacks.GroupBy(s => s).OrderByDescending(g => g.Count()).Take(3).Select(g => $"[{g.Key}] ×{g.Count()}");
                sb.AppendLine($"  {(r.OnFloor ? "Boden " : "Leere ")} {n,-10} Felder {r.Cells,5}, mit Statics {r.Filled,5}: {string.Join("  ", top)}" +
                              (r.ExtraFilled > 0 ? $"   Wand-Deko {r.ExtraFilled}×: {string.Join("  ", r.Extras.GroupBy(e => e).OrderByDescending(g => g.Count()).Take(3).Select(g => $"[{g.Key}]"))}" : string.Empty));
            }
            sb.AppendLine();
            sb.AppendLine("PLAN (# = Wand-Statics, . = Boden, d = Deko, ' ' = Leere) – höchstens 160×120:");
            for (int y = area.Top; y < Math.Min(area.Bottom, area.Top + 120); y++)
            {
                var line = new StringBuilder();
                for (int x = area.Left; x < Math.Min(area.Right, area.Left + 160); x++)
                {
                    bool floor = Floor(x, y);
                    bool hasStatics = map.StaticsAt(x, y).Any();
                    bool wall = map.StaticsAt(x, y).Any(st => !floor || Role(st.Id, 0, true) == "Wand");
                    line.Append(wall ? '#' : floor ? (hasStatics ? 'd' : '.') : ' ');
                }
                sb.AppendLine(line.ToString().TrimEnd());
            }
            return (look, sb.ToString());
        }

        /// <summary>
        /// Builds a dungeon plan with a learned look (inside BeginLand / EndLand): void and floor land at the floor height,
        /// the learned wall stacks in the same situations (outer walls by the floor neighbours, inner walls by direction),
        /// things on the walls, floor statics and loose decoration. Returns the static changes.
        /// </summary>
        public static List<StaticChange> Build(MapData map, Rectangle area, DungeonPlan plan, DungeonLook look, int floorZ, bool underMap, bool fillVoid, bool clearStatics, Random rnd)
        {
            var statics = new List<StaticChange>();
            bool Land(int x, int y) => plan.LandFloor(x - area.X, y - area.Y);
            bool Open(int x, int y) => plan.Floor(x - area.X, y - area.Y);
            ushort[] voidLand = HexList.Parse(look.VoidLand);
            ushort[] floorLand = HexList.Parse(look.FloorLand);
            var floorStatics = (look.FloorStatics ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (underMap && floorStatics.Length == 0)
            {
                // under the map there is no land to walk on: cave floor statics (053d / 053e like in map0)
                floorStatics = new[] { "053d@0", "053e@0" };
            }
            var rules = look.Walls.ToDictionary(r => (r.OnFloor, r.Mask));
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    if (!map.InBounds(x, y))
                    {
                        continue;
                    }
                    bool land = Land(x, y), open = Open(x, y), inner = plan.IsInner(x - area.X, y - area.Y);
                    int mask = Mask(Land, x, y);
                    bool edge = land ? mask != 15 : mask != 0;
                    if (!land && !edge && !fillVoid)
                    {
                        continue;
                    }
                    if (clearStatics)
                    {
                        statics.AddRange(map.StaticsAt(x, y).Select(MapData.Remove));
                    }
                    if (underMap)
                    {
                        // NoDraw on the height of the map (no holes into the depth), the dungeon lies below as statics
                        map.SetLand(x, y, 0x0002, map.LandZ(x, y));
                    }
                    else if (land)
                    {
                        map.SetLand(x, y, floorLand.Length > 0 ? floorLand[rnd.Next(floorLand.Length)] : map.LandId(x, y), floorZ);
                    }
                    else if (voidLand.Length > 0)
                    {
                        map.SetLand(x, y, voidLand[rnd.Next(voidLand.Length)], floorZ);
                    }
                    void Put(string stack)
                    {
                        foreach ((ushort id, int dz) in DungeonLook.ParseStack(stack))
                        {
                            statics.Add(MapData.Add(new StaticItem { Id = id, X = x, Y = y, Z = Math.Clamp(floorZ + dz, -128, 127) }));
                        }
                    }
                    WallRule rule = null;
                    if (inner && !edge)
                    {
                        bool ns = Open(x, y - 1) || Open(x, y + 1), ew = Open(x - 1, y) || Open(x + 1, y);
                        int key = ns && !ew ? WallRule.InnerAlongX : ew && !ns ? WallRule.InnerAlongY : WallRule.InnerPost;
                        if (!rules.TryGetValue((true, key), out rule) || rule.Stacks.Count == 0)
                        {
                            // no inner walls in the sample: the outer wall pieces of the same direction
                            rule = key == WallRule.InnerAlongX ? Best(rules, (true, 11), (false, 4)) : key == WallRule.InnerAlongY ? Best(rules, (true, 13), (false, 2)) : Best(rules, (false, 6), (false, 2));
                        }
                        if (rule != null && rule.Stacks.Count > 0)
                        {
                            Put(rule.Stacks[rnd.Next(rule.Stacks.Count)]);
                        }
                    }
                    else if (edge)
                    {
                        rules.TryGetValue((land, mask), out rule);
                        if (rule != null && rule.Stacks.Count > 0 && rnd.NextDouble() < Math.Max(rule.Filled, rule.Stacks.Count) / (double)Math.Max(1, Math.Max(rule.Cells, rule.Filled)))
                        {
                            Put(rule.Stacks[rnd.Next(rule.Stacks.Count)]);
                        }
                        else if (rule == null && !land)
                        {
                            // a situation the sample did not have: the most similar one (most matching neighbour bits)
                            WallRule near = look.Walls.Where(r => !r.OnFloor && r.Stacks.Count > 0 && r.Mask < 256 && (r.Mask >= 16) == (mask >= 16))
                                .OrderBy(r => BitCount(r.Mask ^ mask)).ThenByDescending(r => r.Filled).FirstOrDefault();
                            if (near != null)
                            {
                                Put(near.Stacks[rnd.Next(near.Stacks.Count)]);
                            }
                        }
                    }
                    if (rule != null && rule.Extras.Count > 0 && rnd.NextDouble() < rule.ExtraFilled / (double)Math.Max(1, rule.Cells))
                    {
                        Put(rule.Extras[rnd.Next(rule.Extras.Count)]);
                    }
                    if (open && floorStatics.Length > 0)
                    {
                        Put(floorStatics[rnd.Next(floorStatics.Length)]);
                    }
                    bool wallHere = rule != null && rule.Stacks.Count > 0 && (inner || rule.Filled * 2 >= rule.Cells);
                    if (open && !wallHere && look.Decor.Count > 0 && rnd.NextDouble() * 100 < look.DecorPercent)
                    {
                        Put(look.Decor[rnd.Next(look.Decor.Count)]);
                    }
                    else if (open && !wallHere && mask != 15 && (look.Furniture?.Count ?? 0) > 0 && rnd.NextDouble() * 100 < look.FurniturePercent)
                    {
                        // furniture stands next to the walls like in the learned dungeon
                        Put(look.Furniture[rnd.Next(look.Furniture.Count)]);
                    }
                }
            }
            return statics;
        }

        private static WallRule Best(Dictionary<(bool, int), WallRule> rules, params (bool, int)[] keys)
        {
            foreach ((bool, int) k in keys)
            {
                if (rules.TryGetValue(k, out WallRule r) && r.Stacks.Count > 0)
                {
                    return r;
                }
            }
            return null;
        }

        // ------------------------------------------------------------------ roles of statics

        private static readonly string[] HungWords = { "torch", "sconce", "painting", "portrait", "tapestry", "banner", "candelabra", "lantern", "mirror", "plaque", "sign" };
        private static readonly string[] LooseWords = { "garbage", "blood", "bone", "skull", "web", "rubble", "debris", "spike", "rock", "pebble", "dirt", "moss", "puddle" };

        /// <summary>Hangs on a wall: torches, paintings … (or something high up that is no surface).</summary>
        private static bool IsHung(ushort id, int dz)
        {
            string name = (UoArtCache.ItemName(id) ?? string.Empty).ToLowerInvariant();
            return HungWords.Any(name.Contains) || (dz >= 8 && !UoArtCache.ItemHas(id, Ultima.TileFlag.Surface) && !UoArtCache.ItemHas(id, Ultima.TileFlag.Wall));
        }

        /// <summary>Role of a static on floor land: Wand, Boden, Wand-Deko, Deko (loose) or Einrichtung (furniture, stones, stairs).</summary>
        private static string Role(ushort id, int dz, bool onlyWall)
        {
            StaticKind k = UoArtCache.ItemKind(id);
            if (k == StaticKind.Wall || k == StaticKind.Roof)
            {
                return "Wand";
            }
            if (onlyWall)
            {
                return "-";
            }
            if (k == StaticKind.Floor)
            {
                return "Boden";
            }
            if (IsHung(id, dz))
            {
                return "Wand-Deko";
            }
            string name = (UoArtCache.ItemName(id) ?? string.Empty).ToLowerInvariant();
            bool loose = UoArtCache.ItemHas(id, Ultima.TileFlag.Background) || LooseWords.Any(name.Contains) ||
                         (!UoArtCache.ItemHas(id, Ultima.TileFlag.Surface) && !UoArtCache.ItemHas(id, Ultima.TileFlag.Impassable) && UoArtCache.ItemHeight(id) <= 1);
            if (name.Contains("stair") || name == "stone")
            {
                loose = false;
            }
            return loose ? "Deko" : "Einrichtung";
        }

        private static int BitCount(int v)
        {
            int n = 0;
            for (; v != 0; v &= v - 1)
            {
                n++;
            }
            return n;
        }
    }
}
