/***************************************************************************
 *
 *  * Dragon Map Studio – cave entrance stamp for the colour map (2D editor, generator)
 *  *
 *  * Paints the original entrance with Dragon colours, the compiler then builds
 *  * the same tiles as the CGI stamp:
 *  *   top row + mouth row : colour of the entrance group (01DB, ~ground + 20)
 *  *                         -> rule entrance -> mountain (map/cave-ent2mountain.txt)
 *  *                            gives 01D3/01D4/01D5 on top (+10) and 01DA/01D6 at the sides
 *  *   front row           : cave floor (group 03) on the ground height
 *  *                         -> rule 03 -> 09 (map/cave2dirt.txt) gives 01E6/01DF/01E4
 *  *   path                : dirt (group 09), grass2dirt makes the soft edge
 *  *   NODRAW statics      : the compiler (option "Höhleneingang: NODRAW-Statics").
 *  * Variant "invisible": group 1d (0002) instead of the black group.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public sealed class CaveColorOptions
    {
        [Category("1 Eingang"), DisplayName("Richtung"), Description("Sichtbar sind nur Eingänge nach Süden oder Osten.")]
        public CaveFacing Facing { get; set; } = CaveFacing.Auto;

        [Category("1 Eingang"), DisplayName("Unsichtbar (Gruppe 1d)"), Description("An: Mund mit der Gruppe 1d (Tile 0002, wie der Generator bisher). Aus: schwarzer Mund 01DB (Gruppe wird bei Bedarf angelegt).")]
        public bool Invisible { get; set; }

        [Category("1 Eingang"), DisplayName("Breite"), Description("Mund-Tiles ohne Rahmen (Original 3).")]
        public int Width { get; set; } = 3;

        [Category("1 Eingang"), DisplayName("Mund über Boden"), Description("Höhe der Eingangs-Farbe über dem Boden (die obere Reihe bekommt durch die Regel +5…15 dazu).")]
        public int MouthHeight { get; set; } = 20;

        [Category("2 Gruppen"), DisplayName("Berg-Gruppe")]
        public int MountainGroup { get; set; } = 0x06;

        [Category("2 Gruppen"), DisplayName("Höhlenboden-Gruppe"), Description("Gruppe 03 = Dungeon Caves (0245–0249), Regel 03 → 09 cave2dirt.txt.")]
        public int CaveFloorGroup { get; set; } = 0x03;

        [Category("2 Gruppen"), DisplayName("Erd-Gruppe"), Description("Gruppe 09 = Erde / Weg.")]
        public int DirtGroup { get; set; } = 0x09;

        [Category("2 Gruppen"), DisplayName("Wasser-Gruppe")]
        public int WaterGroup { get; set; } = 0x02;

        [Category("3 Weg"), DisplayName("Weg-Länge"), Description("Reihen Erde vor der Höhlenboden-Reihe (0 = kein Weg).")]
        public int PathLength { get; set; } = 2;
    }

    public static class CaveColorStamp
    {
        public const string VisibleGroupName = "Höhleneingang schwarz (01DB)";

        /// <summary>
        /// The black entrance group (tile 01DB, colours with height 10 / 20 / 30) and its rule to the mountain.
        /// Created when missing (first free group and colour addresses). Returns the group or -1.
        /// </summary>
        public static int EnsureVisibleGroup(DragonRuleSet rules, int mountainGroup, List<string> log)
        {
            DragonGroup g = rules.Groups.FirstOrDefault(x => x.Name == VisibleGroupName);
            int id = g?.Id ?? -1;
            if (id < 0)
            {
                // an existing group whose colours are only 01DB (e.g. maptrans105: 1a = cave entrance)
                id = rules.Palette.Where(p => p.Defined && p.Group >= 0 && p.GetTileIds().Length == 1 && p.GetTileIds()[0] == 0x01DB)
                              .Select(p => p.Group).DefaultIfEmpty(-1).First();
            }
            if (id < 0)
            {
                id = RuleSetTools.FreeGroupId(rules);
                if (id < 0)
                {
                    log?.Add("Keine freie Gruppe für den schwarzen Höhleneingang.");
                    return -1;
                }
                rules.Groups.Add(new DragonGroup { Id = id, Name = VisibleGroupName, Comment = "Dragon Map Studio" });
                foreach (int alt in new[] { 10, 20, 30 })
                {
                    int index = rules.FirstFreeIndex();
                    if (index < 0)
                    {
                        break;
                    }
                    DragonPaletteEntry e = rules.Palette[index];
                    e.Defined = true;
                    e.SetColor(RuleSetTools.UniqueColor(rules, Color.FromArgb(24 + alt, 8, 24 + alt), index));
                    e.Name = $"Höhleneingang schwarz {alt}";
                    e.Category = "Höhle";
                    e.Group = id;
                    e.Altitude = alt;
                    e.Tiles = "01db";
                }
                log?.Add($"Gruppe {rules.HexGroup(id)} '{VisibleGroupName}' mit 3 Farben (Höhe 10 / 20 / 30, Tile 01DB) angelegt.");
            }
            if (!rules.Transitions.Any(t => t.Layer == TransitionLayer.Map && t.GroupA == id))
            {
                TransitionFile f = rules.FindTransitionFile("map/cave-ent2mountain.txt") ?? rules.FindTransitionFile("map/cave-enti2mountain.txt");
                if (f != null)
                {
                    rules.Transitions.Add(new TransitionRule { Layer = TransitionLayer.Map, ScriptSet = 1, GroupA = id, GroupB = mountainGroup, File = f.Path, Comment = "Höhleneingang schwarz (Dragon Map Studio)" });
                    log?.Add($"Regel {rules.HexGroup(id)} → {rules.HexGroup(mountainGroup)} {f.Path} aktiviert.");
                }
            }
            return id;
        }

        /// <summary>Rule 03 -> 09 cave2dirt.txt (front row 01E6 / 01DF / 01E4).</summary>
        public static void EnsureCaveDirtRule(DragonRuleSet rules, CaveColorOptions o, List<string> log)
        {
            if (rules.Transitions.Any(t => t.Layer == TransitionLayer.Map && t.GroupA == o.CaveFloorGroup && t.GroupB == o.DirtGroup))
            {
                return;
            }
            TransitionFile f = rules.FindTransitionFile("map/cave2dirt.txt");
            if (f != null)
            {
                rules.Transitions.Add(new TransitionRule { Layer = TransitionLayer.Map, ScriptSet = 1, GroupA = o.CaveFloorGroup, GroupB = o.DirtGroup, File = f.Path, Comment = "Höhlenboden → Erde (Dragon Map Studio)" });
                log?.Add($"Regel {rules.HexGroup(o.CaveFloorGroup)} → {rules.HexGroup(o.DirtGroup)} {f.Path} aktiviert.");
            }
        }

        /// <summary>Colour of a group with the altitude closest to <paramref name="z"/>, or -1.</summary>
        public static int ColourFor(DragonRuleSet rules, int group, int z)
        {
            return rules.EntriesOfGroup(group).Where(e => e.Defined).OrderBy(e => Math.Abs(e.Altitude - z)).Select(e => e.Index).DefaultIfEmpty(-1).First();
        }

        /// <summary>
        /// Paints an entrance next to (x, y). Returns the changed area or Rectangle.Empty (message says why).
        /// <paramref name="entranceGroup"/>: group of the mouth (black group or 1d).
        /// </summary>
        public static Rectangle Stamp(IndexedMap map, DragonRuleSet rules, int x, int y, CaveColorOptions o, int entranceGroup, out string message)
        {
            message = string.Empty;
            int[] groupOf = GroupTable(rules);
            int G(int px, int py) => map.InBounds(px, py) && map[px, py] < groupOf.Length ? groupOf[map[px, py]] : -1;
            bool Mountain(int px, int py) => G(px, py) == o.MountainGroup;
            bool Ground(int px, int py)
            {
                int g = G(px, py);
                return g >= 0 && g != o.MountainGroup && g != o.WaterGroup && g != entranceGroup;
            }
            int FindFront(bool south)
            {
                int best = int.MinValue, c = south ? y : x;
                for (int d = -1; d <= 4; d++)
                {
                    int f = c - d;
                    bool ok = south ? Mountain(x, f) && Ground(x, f + 1) : Mountain(f, y) && Ground(f + 1, y);
                    if (ok && (best == int.MinValue || Math.Abs(f - c) < Math.Abs(best - c)))
                    {
                        best = f;
                    }
                }
                return best;
            }
            int fs = o.Facing != CaveFacing.East ? FindFront(true) : int.MinValue;
            int fe = o.Facing != CaveFacing.South ? FindFront(false) : int.MinValue;
            if (fs == int.MinValue && fe == int.MinValue)
            {
                message = "Kein Bergrand gefunden: neben einen Berg klicken, dessen Süd- oder Ostseite an Boden grenzt.";
                return Rectangle.Empty;
            }
            bool south = fe == int.MinValue || (fs != int.MinValue && Math.Abs(fs - y) <= Math.Abs(fe - x));
            int front = south ? fs : fe;
            Point Cell(int along, int row) => south ? new Point(along, front + 1 - row) : new Point(front + 1 - row, along);
            int width = Math.Max(1, o.Width);
            int first = (south ? x : y) - (width - 1) / 2 - 1, last = first + width + 1;

            var grounds = new List<int>();
            for (int a = first; a <= last; a++)
            {
                Point g = Cell(a, 0);
                if (Ground(g.X, g.Y))
                {
                    grounds.Add(rules.Palette[map[g.X, g.Y]].Altitude);
                }
            }
            if (grounds.Count == 0)
            {
                message = "Vor dem Berg liegt kein Boden.";
                return Rectangle.Empty;
            }
            grounds.Sort();
            int groundZ = grounds[grounds.Count / 2];
            int entrance = ColourFor(rules, entranceGroup, groundZ + o.MouthHeight);
            int floor = ColourFor(rules, o.CaveFloorGroup, groundZ);
            int dirt = ColourFor(rules, o.DirtGroup, groundZ);
            if (entrance < 0 || floor < 0 || dirt < 0)
            {
                message = $"Farben fehlen: Eingang {rules.HexGroup(entranceGroup)} / Höhlenboden {rules.HexGroup(o.CaveFloorGroup)} / Erde {rules.HexGroup(o.DirtGroup)}.";
                return Rectangle.Empty;
            }
            void Set(Point p, int index)
            {
                if (map.InBounds(p.X, p.Y))
                {
                    map[p.X, p.Y] = (ushort)index;
                }
            }
            for (int a = first; a <= last; a++)
            {
                Set(Cell(a, 2), entrance);
                Set(Cell(a, 1), entrance);
                Set(Cell(a, 0), floor);
            }
            // dirt beside the front row and in front of it: the corners 01E6 / 01E4 and the soft edge to the grass
            for (int row = 0; row >= -Math.Max(0, o.PathLength); row--)
            {
                for (int a = first - 1; a <= last + 1; a++)
                {
                    if (row == 0 && a >= first && a <= last)
                    {
                        continue;
                    }
                    Point p = Cell(a, row);
                    if (Ground(p.X, p.Y))
                    {
                        Set(p, dirt);
                    }
                }
            }
            Rectangle r = south
                ? Rectangle.FromLTRB(first - 1, front - 1, last + 2, front + 2 + Math.Max(0, o.PathLength))
                : Rectangle.FromLTRB(front - 1, first - 1, front + 2 + Math.Max(0, o.PathLength), last + 2);
            r.Intersect(map.Bounds);
            message = $"Höhleneingang nach {(south ? "Süden" : "Osten")} gemalt ({width} breit + Rahmen, Boden {groundZ}). Die Tiles entstehen beim Bauen.";
            return r;
        }

        /// <summary>Paints the entrance at (x, y) back: entrance colours -> mountain, cave floor / dirt in front stay (paint them yourself).</summary>
        public static Rectangle Remove(IndexedMap map, DragonRuleSet rules, int x, int y, CaveColorOptions o, int entranceGroup, out string message)
        {
            int[] groupOf = GroupTable(rules);
            bool IsEntrance(int px, int py) => map.InBounds(px, py) && map[px, py] < groupOf.Length &&
                                               (groupOf[map[px, py]] == entranceGroup || groupOf[map[px, py]] == 0x1d);
            Point start = new Point(-1, -1);
            for (int r = 0; r <= 3 && start.X < 0; r++)
            {
                for (int dy = -r; dy <= r && start.X < 0; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (IsEntrance(x + dx, y + dy))
                        {
                            start = new Point(x + dx, y + dy);
                            break;
                        }
                    }
                }
            }
            if (start.X < 0)
            {
                message = "Hier ist kein Höhleneingang (Gruppe der Eingangs-Farbe / 1d).";
                return Rectangle.Empty;
            }
            var cells = new List<Point>();
            var seen = new HashSet<Point> { start };
            var queue = new Queue<Point>();
            queue.Enqueue(start);
            while (queue.Count > 0 && cells.Count < 400)
            {
                Point p = queue.Dequeue();
                cells.Add(p);
                foreach (Point n in new[] { new Point(p.X + 1, p.Y), new Point(p.X - 1, p.Y), new Point(p.X, p.Y + 1), new Point(p.X, p.Y - 1) })
                {
                    if (!seen.Contains(n) && IsEntrance(n.X, n.Y))
                    {
                        seen.Add(n);
                        queue.Enqueue(n);
                    }
                }
            }
            foreach (Point p in cells)
            {
                // the mountain colour of the neighbours (keeps the height step of the mountain)
                int best = -1;
                foreach (Point n in new[] { new Point(p.X, p.Y - 1), new Point(p.X - 1, p.Y), new Point(p.X + 1, p.Y), new Point(p.X, p.Y + 1), new Point(p.X, p.Y - 2), new Point(p.X - 2, p.Y) })
                {
                    if (map.InBounds(n.X, n.Y) && map[n.X, n.Y] < groupOf.Length && groupOf[map[n.X, n.Y]] == o.MountainGroup)
                    {
                        best = map[n.X, n.Y];
                        break;
                    }
                }
                if (best < 0)
                {
                    best = ColourFor(rules, o.MountainGroup, 30);
                }
                if (best >= 0)
                {
                    map[p.X, p.Y] = (ushort)best;
                }
            }
            int minX = cells.Min(c => c.X), minY = cells.Min(c => c.Y), maxX = cells.Max(c => c.X), maxY = cells.Max(c => c.Y);
            message = $"Höhleneingang entfernt: {cells.Count} Pixel wieder Berg. Höhlenboden / Erde davor ggf. mit Gras übermalen.";
            Rectangle area = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            area.Intersect(map.Bounds);
            return area;
        }

        private static int[] GroupTable(DragonRuleSet rules)
        {
            var t = new int[Math.Max(256, rules.Palette.Count)];
            Array.Fill(t, -1);
            foreach (DragonPaletteEntry e in rules.Palette)
            {
                if (e.Defined && e.Index < t.Length)
                {
                    t[e.Index] = e.Group;
                }
            }
            return t;
        }
    }
}
