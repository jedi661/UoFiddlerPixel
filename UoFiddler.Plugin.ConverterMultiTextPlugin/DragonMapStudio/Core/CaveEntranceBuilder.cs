/***************************************************************************
 *
 *  * Dragon Map Studio – cave entrances like the original map (IMod13 / Dragon "1d -> mountain")
 *  *
 *  * The entrance is a small region cut into the mountain side:
 *  *  - front row (touches the open ground): mouth tile 0002 (invisible, the client does not draw
 *  *    land ids <= 2) on the ground height,
 *  *  - cells next to the mountain: the entrance tiles 01D3–01DA (black arch with the rock teeth),
 *  *    chosen by the 8 neighbours exactly like map/cave-enti2mountain.txt, lifted to "mouth height"
 *  *    so the client stretches their texture up the mountain face.
 *  * Ground and mountain are told apart by the tile (mountain group / tiledata name), not by height,
 *  * so grass that lies higher than usual still counts as ground.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>Kinds of entrances found on the original map.</summary>
    public enum CaveVariant
    {
        [Description("Original: schwarzer Mund, Übergang 01E6/01DF/01E4 und Erdweg davor")]
        Original,

        [Description("Ohne Weg: schwarzer Mund direkt auf dem Boden (Gras bleibt)")]
        OnGround,

        [Description("Unsichtbar: Mund 0002, davor Höhlenboden (Dungeon-Seite)")]
        Invisible
    }

    public enum CaveFacing
    {
        [Description("Automatisch")]
        Auto,

        [Description("Süden (Berg im Norden)")]
        South,

        [Description("Osten (Berg im Westen)")]
        East
    }

    /// <summary>How entrance cells are tiled (shared by the entrance brush and the stamp).</summary>
    public sealed class CaveEntranceOptions
    {
        /// <summary>Interior / front tile: 0002 invisible (Dragon 1d, original) or 01DB black.</summary>
        public ushort FillId { get; set; } = 0x0002;

        /// <summary>Patterns of map/cave-enti2mountain.txt by mask; null or empty = built-in copy.</summary>
        public IReadOnlyDictionary<byte, TransitionPattern> Patterns { get; set; }

        /// <summary>Tile is mountain / rock. Null = height rule (<see cref="Rise"/>) only.</summary>
        public Func<ushort, bool> IsMountain { get; set; }

        /// <summary>Only without <see cref="IsMountain"/>: a neighbour this much above the ground counts as mountain.</summary>
        public int Rise { get; set; } = 5;
    }

    /// <summary>
    /// Settings of the cave stamp. Default values measured on the original map (Britannia, see LIESMICH):
    /// mouth 01DB about 20 above the ground, frame 01DA / 01D6 about 15, top row 01D3 / 01D4 / 01D5 about 33.
    /// </summary>
    public sealed class CaveStampSettings
    {
        [Category("1 Eingang"), DisplayName("Richtung"), Description("Wohin der Eingang zeigt. Sichtbar sind nur Eingänge nach Süden oder Osten (die Kamera schaut von Südosten).")]
        public CaveFacing Facing { get; set; } = CaveFacing.Auto;

        [Category("1 Eingang"), DisplayName("Variante"), Description("Original = schwarzer Mund mit Übergangsreihe und Erdweg; Ohne Weg = Mund direkt auf dem Boden; Unsichtbar = Mund 0002 mit Höhlenboden davor (wie innen im Dungeon).")]
        public CaveVariant Variant { get; set; } = CaveVariant.Original;

        [Category("4 Weg davor"), DisplayName("Höhlenboden-Tiles"), Description("Variante Unsichtbar: Boden vor dem Mund (Original 0245–0249).")]
        public string CaveFloorTiles { get; set; } = "0245 0246 0247 0248 0249";

        [Category("4 Weg davor"), DisplayName("Übergänge berechnen"), Description("Nach dem Stempeln die Dragon-Übergänge (z. B. Gras → Erde, Berg → Erde) rund um den Eingang neu setzen, damit der Weg weich ins Gras übergeht.")]
        public bool Transitions { get; set; } = true;

        [Category("1 Eingang"), DisplayName("Breite"), Description("Anzahl schwarzer Mund-Tiles 01DB (Original: 3). Dazu kommt links und rechts der Rahmen.")]
        public int Width { get; set; } = 3;

        [Category("2 Höhen über dem Boden"), DisplayName("Mund 01DB"), Description("Höhe der schwarzen Mund-Tiles über dem Boden davor (Original 17–25). Beim Ziehen: die Höhe am Ende des Zuges.")]
        public int MouthHeight { get; set; } = 22;

        [Category("2 Höhen über dem Boden"), DisplayName("Rahmen 01DA / 01D6"), Description("Höhe der Rahmen-Tiles links / rechts vom Mund (Original 10–20).")]
        public int FrameHeight { get; set; } = 15;

        [Category("2 Höhen über dem Boden"), DisplayName("Oben 01D3 / 01D4 / 01D5"), Description("Höhe der oberen Reihe über dem Boden (Original 30–37).")]
        public int ArchHeight { get; set; } = 33;

        [Category("2 Höhen über dem Boden"), DisplayName("Berg dahinter +"), Description("Die Bergreihe hinter der oberen Reihe liegt mindestens so viel höher (Original +5…+12, 0 = nicht anheben).")]
        public int WallAbove { get; set; } = 8;

        [Category("3 Unsichtbare Statics"), DisplayName("NODRAW-Statics setzen"), Description("Original: auf jedem Tile der Mund-Reihe (Rahmen + Mund) liegt ein unsichtbares Static auf Bodenhöhe – darauf läuft man in die Höhle.")]
        public bool NoDrawStatics { get; set; } = true;

        [Category("3 Unsichtbare Statics"), DisplayName("NODRAW-Static-ID"), Description("Item-ID (hex) des unsichtbaren Statics. 0001 = nodraw (der Client zeichnet 0001, 21BC, 2198–21A4 und Namen 'nodraw…' nicht).")]
        public string NoDrawStaticId { get; set; } = "0001";

        [Category("4 Weg davor"), DisplayName("Weg-Länge"), Description("Reihen Erdweg vor der Übergangsreihe 01E6 / 01DF / 01E4 (0 = kein Weg).")]
        public int PathLength { get; set; } = 2;

        [Category("4 Weg davor"), DisplayName("Weg-Tiles"), Description("Land-Tiles (hex) des Weges, zufällig gemischt – Original: Erde 0071–0078.")]
        public string PathTiles { get; set; } = "0071 0072 0073 0074 0075 0076 0077 0078";

        [Category("4 Weg davor"), DisplayName("Weg-Breite +"), Description("Der Weg ist so viele Tiles breiter als Mund + Rahmen (je Seite die Hälfte).")]
        public int PathExtraWidth { get; set; } = 2;
    }

    public sealed class CaveStampResult
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = string.Empty;
        public CaveFacing Facing { get; set; }
        public int GroundZ { get; set; }
        public int MountainZ { get; set; }
        public Rectangle Region { get; set; }
    }

    public static class CaveEntranceBuilder
    {
        private static readonly int[] NbDx = { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static readonly int[] NbDy = { -1, -1, -1, 0, 1, 1, 1, 0 };

        /// <summary>
        /// Built-in copy of map/cave-enti2mountain.txt: A = entrance, B = mountain, order NW N NE E SE S SW W.
        /// Z = the middle of Z1..Z2 of the file (the north side 01D4 and two corners +10).
        /// </summary>
        public static readonly (string pattern, ushort tile, int z)[] DefaultPatterns =
        {
            ("BBBBBBBA", 0x01DA, 0), ("BBBABBBB", 0x01D6, 0),
            ("BAAAAABB", 0x01DA, 0), ("BAAAAAAB", 0x01DA, 0), ("AAAAAABB", 0x01DA, 0), ("AAAAAAAB", 0x01DA, 0), ("BAAAAABA", 0x01DA, 0),
            ("AABBBAAA", 0x01D6, 0), ("AAABBAAA", 0x01D6, 0), ("AABBAAAA", 0x01D6, 0), ("AAABAAAA", 0x01D6, 0), ("AABABAAA", 0x01D6, 0),
            ("AAAABBBA", 0x01D8, 0), ("AAAAABBA", 0x01D8, 0), ("AAAABBAA", 0x01D8, 0), ("AAAAABAA", 0x01D8, 0), ("AAAABABA", 0x01D8, 0),
            ("BBBAAAAA", 0x01D4, 10), ("ABBAAAAA", 0x01D4, 10), ("BBAAAAAA", 0x01D4, 10), ("ABAAAAAA", 0x01D4, 10), ("BABAAAAA", 0x01D4, 10),
            ("AAAAAABA", 0x01D5, 0), ("ABABBAAA", 0x01D5, 0),
            ("BAAAAAAA", 0x01D7, 0), ("AAABABBA", 0x01D7, 0),
            ("AAAABAAA", 0x01D3, 0), ("ABBAAAAB", 0x01D3, 0),
            ("AABAAAAA", 0x01D9, 0), ("BAAAABAB", 0x01D9, 0),
            ("AABBBBBA", 0x01D7, 0), ("AAABBBBA", 0x01D7, 0), ("AABBBBAA", 0x01D7, 0), ("AAABBBAA", 0x01D7, 0),
            ("BBBBBAAA", 0x01D5, 10), ("BBBBAAAA", 0x01D5, 10), ("ABBBBAAA", 0x01D5, 10), ("ABBBAAAA", 0x01D5, 10),
            ("BBBAAABB", 0x01D3, 10), ("BBAAAABB", 0x01D3, 10), ("BBBAAAAB", 0x01D3, 10), ("BBAAAAAB", 0x01D3, 10),
            ("BAAABBBB", 0x01D9, 0), ("AAAABBBB", 0x01D9, 0), ("BAAAABBB", 0x01D9, 0), ("AAAAABBB", 0x01D9, 0)
        };

        private static Dictionary<byte, (ushort tile, int z)> _defaultTable;

        /// <summary>Entrance tiles: 01D3–01DB, the invisible 0002 and the fill tile.</summary>
        public static bool IsEntranceTile(ushort id, ushort fill) => (id >= 0x01D3 && id <= 0x01DB) || id == 0x0002 || id == fill;

        /// <summary>The client never draws land tiles 0, 1, 2 (ClassicUO: AllowedToDraw = Graphic > 2).</summary>
        public static bool IsNoDraw(ushort id) => id <= 2;

        public static bool TryPattern(CaveEntranceOptions o, byte mask, Random rnd, out ushort tile, out int z)
        {
            if (o.Patterns != null && o.Patterns.Count > 0)
            {
                if (o.Patterns.TryGetValue(mask, out TransitionPattern p) && p.GetTileIds().Length > 0)
                {
                    ushort[] ids = p.GetTileIds();
                    tile = ids[ids.Length == 1 ? 0 : rnd.Next(ids.Length)];
                    z = (p.Z1 + p.Z2) / 2;
                    return true;
                }
                tile = 0;
                z = 0;
                return false;
            }
            if (_defaultTable == null)
            {
                var t = new Dictionary<byte, (ushort, int)>();
                foreach ((string pattern, ushort tile, int z) e in DefaultPatterns)
                {
                    t[TransitionPattern.PatternToMask(e.pattern)] = (e.tile, e.z);
                }
                _defaultTable = t;
            }
            if (_defaultTable.TryGetValue(mask, out (ushort tile, int z) d))
            {
                tile = d.tile;
                z = d.z;
                return true;
            }
            tile = 0;
            z = 0;
            return false;
        }

        /// <summary>Neighbour (not an entrance cell) counts as mountain: by tile, or by height without a tile rule.</summary>
        public static bool IsMountainCell(MapData map, int x, int y, int groundZ, CaveEntranceOptions o)
        {
            ushort id = map.LandId(x, y);
            if (o.IsMountain != null)
            {
                return o.IsMountain(id);
            }
            return map.LandZ(x, y) >= groundZ + Math.Max(1, o.Rise);
        }

        /// <summary>
        /// Gives every entrance cell in <paramref name="r"/> its tile like Dragon: neighbours that are mountain = B.
        /// <paramref name="heightOf"/>(x, y, offset) returns the new height or null to keep the height.
        /// </summary>
        public static void Retile(MapData map, Rectangle r, CaveEntranceOptions o, Func<int, int, int> groundOf, Func<int, int, int, int?> heightOf, Random rnd)
        {
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    if (!map.InBounds(x, y) || !IsEntranceTile(map.LandId(x, y), o.FillId))
                    {
                        continue;
                    }
                    int ground = groundOf(x, y);
                    byte mask = 0;
                    for (int k = 0; k < 8; k++)
                    {
                        int nx = x + NbDx[k], ny = y + NbDy[k];
                        if (!map.InBounds(nx, ny) || IsEntranceTile(map.LandId(nx, ny), o.FillId))
                        {
                            continue;
                        }
                        if (IsMountainCell(map, nx, ny, ground, o))
                        {
                            mask |= (byte)(1 << k);
                        }
                    }
                    ushort tile = o.FillId;
                    int dz = 0;
                    bool edge = mask != 0 && TryPattern(o, mask, rnd, out tile, out dz);
                    if (!edge)
                    {
                        tile = o.FillId;
                        dz = 0;
                    }
                    int? nz = heightOf(x, y, edge ? dz : int.MinValue);
                    int z = nz ?? map.LandZ(x, y);
                    if (tile != map.LandId(x, y) || z != map.LandZ(x, y))
                    {
                        map.SetLand(x, y, tile, Math.Clamp(z, -128, 127));
                    }
                }
            }
        }

        /// <summary>
        /// The original entrance as a fixed raster (measured on the Britannia map), south facing (mountain north, ground south),
        /// columns: left frame | mouth × Width | right frame:
        ///   top row   (2nd mountain row):  01D3 | 01D4 … | 01D5     ≈ ground + 33
        ///   mouth row (1st mountain row):  01DA | 01DB … | 01D6     ≈ ground + 15 / + 22 / + 15, NODRAW statics on the ground
        ///   front row (1st ground row):    01E6 | 01DF … | 01E4     ground (cave2dirt transition)
        ///   then the dirt path.
        /// East facing (mountain west) is the same turned: top 01D3 | 01DA … | 01D9, mouth 01D4 | 01DB … | 01D8, front 01E7 | 01DD … | 01E4.
        /// </summary>
        private static (ushort first, ushort middle, ushort last) RowTiles(bool south, int row) => (south, row) switch
        {
            (true, 2) => (0x01D3, 0x01D4, 0x01D5),
            (true, 1) => (0x01DA, 0x01DB, 0x01D6),
            (true, 0) => (0x01E6, 0x01DF, 0x01E4),
            (false, 2) => (0x01D3, 0x01DA, 0x01D9),
            (false, 1) => (0x01D4, 0x01DB, 0x01D8),
            _ => (0x01E7, 0x01DD, 0x01E4)
        };

        /// <summary>Land tiles of an entrance built like the original: 0000–0002 and 01D3–01E7.</summary>
        public static bool IsEntrancePart(ushort id) => id <= 2 || (id >= 0x01D3 && id <= 0x01E7);

        /// <summary>
        /// Removes the cave entrance at / next to (x, y): mouth, frame and top row become mountain again (height from the
        /// mountain around), the front row becomes the ground around, NODRAW statics on it are deleted.
        /// Call inside BeginLand / EndLand. Returns the area or Rectangle.Empty.
        /// </summary>
        public static Rectangle Remove(MapData map, int x, int y, ushort[] mountainTiles, Func<ushort, bool> isMountain, Random rnd, out string message)
        {
            message = string.Empty;
            Point start = new Point(-1, -1);
            for (int r = 0; r <= 3 && start.X < 0; r++)
            {
                for (int dy = -r; dy <= r && start.X < 0; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (map.InBounds(x + dx, y + dy) && IsEntrancePart(map.LandId(x + dx, y + dy)))
                        {
                            start = new Point(x + dx, y + dy);
                            break;
                        }
                    }
                }
            }
            if (start.X < 0 || mountainTiles == null || mountainTiles.Length == 0)
            {
                message = start.X < 0 ? "Hier ist kein Höhleneingang (01D3–01E7 / 0002)." : "Keine Berg-Tiles bekannt.";
                return Rectangle.Empty;
            }
            var cells = new HashSet<Point>();
            var queue = new Queue<Point>();
            queue.Enqueue(start);
            cells.Add(start);
            while (queue.Count > 0 && cells.Count < 400)
            {
                Point p = queue.Dequeue();
                for (int k = 0; k < 8; k++)
                {
                    var n = new Point(p.X + NbDx[k], p.Y + NbDy[k]);
                    if (map.InBounds(n.X, n.Y) && !cells.Contains(n) && IsEntrancePart(map.LandId(n.X, n.Y)))
                    {
                        cells.Add(n);
                        queue.Enqueue(n);
                    }
                }
            }
            int minX = cells.Min(c => c.X), minY = cells.Min(c => c.Y), maxX = cells.Max(c => c.X), maxY = cells.Max(c => c.Y);
            // decide per cell: front row tiles (01DC–01E7, cave2dirt) were ground, the rest was mountain
            var plan = new List<(Point p, ushort id, int z)>();
            foreach (Point c in cells)
            {
                ushort id = map.LandId(c.X, c.Y);
                bool wasGround = id >= 0x01DC && id <= 0x01E7;
                var mountainZ = new List<int>();
                var ground = new List<(ushort id, int z)>();
                for (int r = 1; r <= 2; r++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        for (int dx = -r; dx <= r; dx++)
                        {
                            int nx = c.X + dx, ny = c.Y + dy;
                            if (!map.InBounds(nx, ny) || cells.Contains(new Point(nx, ny)))
                            {
                                continue;
                            }
                            ushort nid = map.LandId(nx, ny);
                            if (isMountain != null && isMountain(nid))
                            {
                                mountainZ.Add(map.LandZ(nx, ny));
                            }
                            else
                            {
                                ground.Add((nid, map.LandZ(nx, ny)));
                            }
                        }
                    }
                }
                if (wasGround && ground.Count > 0)
                {
                    ushort gid = ground.GroupBy(g => g.id).OrderByDescending(g => g.Count()).First().Key;
                    plan.Add((c, gid, (int)Math.Round(ground.Average(g => g.z))));
                }
                else
                {
                    int z = mountainZ.Count > 0 ? (int)Math.Round(mountainZ.Average()) : map.LandZ(c.X, c.Y) + 10;
                    plan.Add((c, mountainTiles[rnd.Next(mountainTiles.Length)], Math.Max(z, map.LandZ(c.X, c.Y))));
                }
            }
            foreach ((Point p, ushort id, int z) in plan)
            {
                map.SetLand(p.X, p.Y, id, Math.Clamp(z, -128, 127));
            }
            var statics = new List<StaticChange>();
            foreach (Point c in cells)
            {
                statics.AddRange(map.StaticsAt(c.X, c.Y).Where(st => IsNoDrawStatic(st.Id, null)).ToList().Select(MapData.Remove));
            }
            map.ApplyStatics(statics);
            message = $"Höhleneingang entfernt: {cells.Count} Tiles wieder Berg / Boden" + (statics.Count > 0 ? $", {statics.Count} NODRAW-Statics gelöscht" : string.Empty) + ".";
            return Rectangle.Inflate(Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1), 2, 2);
        }

        /// <summary>Static the client never draws (ClassicUO GameObject.CanBeDrawn).</summary>
        public static bool IsNoDrawStatic(ushort id, string name)
        {
            return id == 0x0001 || id == 0x21BC || (id >= 0xA1FE && id <= 0xA201) || (id >= 0x2198 && id <= 0x21A4) ||
                   (!string.IsNullOrEmpty(name) && name.StartsWith("nodraw", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Cuts a complete entrance like the original into the mountain face next to (<paramref name="cx"/>, <paramref name="cy"/>):
        /// finds where ground meets mountain, takes the ground height in front (whatever it is, higher grass included), writes the
        /// three rows, the NODRAW statics and the path. <paramref name="dragEnd"/>: dragged along the face = width, the end on
        /// the face = mouth height. Call inside BeginLand / EndLand for one undo step.
        /// </summary>
        public static CaveStampResult Stamp(MapData map, int cx, int cy, CaveStampSettings s, CaveEntranceOptions o, Random rnd, Point? dragEnd = null)
        {
            var result = new CaveStampResult();
            if (o.IsMountain == null)
            {
                result.Message = "Keine Berg-Tiles bekannt (Regeln / TileData nicht geladen).";
                return result;
            }
            bool Mountain(int x, int y) => map.InBounds(x, y) && o.IsMountain(map.LandId(x, y));
            bool Ground(int x, int y) => map.InBounds(x, y) && !o.IsMountain(map.LandId(x, y)) && !IsEntranceTile(map.LandId(x, y), o.FillId);

            // where does the mountain start? south facing: mountain at (x, f), ground at (x, f + 1)
            int FindFront(bool south)
            {
                int best = int.MinValue, c = south ? cy : cx;
                for (int d = -1; d <= 4; d++)
                {
                    int f = c - d;
                    bool ok = south ? Mountain(cx, f) && Ground(cx, f + 1) : Mountain(f, cy) && Ground(f + 1, cy);
                    if (ok && (best == int.MinValue || Math.Abs(f - c) < Math.Abs(best - c)))
                    {
                        best = f;
                    }
                }
                return best;
            }
            int fs = s.Facing != CaveFacing.East ? FindFront(true) : int.MinValue;
            int fe = s.Facing != CaveFacing.South ? FindFront(false) : int.MinValue;
            bool south;
            if (fs != int.MinValue && fe != int.MinValue)
            {
                south = Math.Abs(fs - cy) <= Math.Abs(fe - cx);
            }
            else if (fs != int.MinValue || fe != int.MinValue)
            {
                south = fs != int.MinValue;
            }
            else
            {
                result.Message = s.Facing == CaveFacing.Auto
                    ? "Kein Bergfuß gefunden: auf den Boden direkt vor einer Bergwand klicken, die nach Süden oder Osten zeigt."
                    : "Kein Bergfuß in dieser Richtung gefunden.";
                return result;
            }
            int front = south ? fs : fe; // first mountain row = mouth row

            // map (along, row) -> cell; row 1 = mouth row, 2 = top row, 0 = front ground row, -1.. = path
            Point Cell(int along, int row) => south ? new Point(along, front + 1 - row) : new Point(front + 1 - row, along);

            int width = Math.Max(1, s.Width);
            int mouthH = s.MouthHeight, frameH = s.FrameHeight, archH = s.ArchHeight;
            int alongCenter = south ? cx : cy;
            if (dragEnd is Point end && end != new Point(cx, cy))
            {
                // dragged along the face (left to right or back): width; the end up on the face: mouth height
                int a0 = south ? cx : cy, a1 = south ? end.X : end.Y;
                if (Math.Abs(a1 - a0) >= 2)
                {
                    width = Math.Abs(a1 - a0) - 1; // start and end are the frames
                    alongCenter = (a0 + a1) / 2;
                }
                int frontGround = map.LandZ(Cell(alongCenter, 0).X, Cell(alongCenter, 0).Y);
                // the drag ends at the top corner (01D3 / 01D5 / 01D9): its height above the ground = top row height
                int h = map.LandZ(end.X, end.Y) - frontGround;
                if (Mountain(end.X, end.Y) && h >= 8)
                {
                    double k = Math.Min(90, h) / (double)Math.Max(1, s.ArchHeight);
                    mouthH = (int)Math.Round(s.MouthHeight * k);
                    frameH = (int)Math.Round(s.FrameHeight * k);
                    archH = (int)Math.Round(s.ArchHeight * k);
                }
            }
            int first = alongCenter - (width - 1) / 2 - 1, last = first + width + 1; // frame columns

            // ground: the front ground row (grass may lie higher – it is still ground)
            var grounds = new List<int>();
            for (int a = first; a <= last; a++)
            {
                Point g = Cell(a, 0);
                if (Ground(g.X, g.Y))
                {
                    grounds.Add(map.LandZ(g.X, g.Y));
                }
            }
            if (grounds.Count == 0)
            {
                result.Message = "Vor dem Eingang liegt kein Boden.";
                return result;
            }
            grounds.Sort();
            int groundZ = grounds[grounds.Count / 2];

            // the three rows (variant "on the ground": no front row; "invisible": front row = cave floor, mouth 0002)
            ushort[] caveFloor = HexList.Parse(s.CaveFloorTiles);
            for (int row = s.Variant == CaveVariant.OnGround ? 1 : 0; row <= 2; row++)
            {
                (ushort t0, ushort tm, ushort t1) = RowTiles(south, row);
                if (row == 1 && s.Variant == CaveVariant.Invisible)
                {
                    tm = 0x0002;
                }
                if (row == 0 && s.Variant == CaveVariant.Invisible && caveFloor.Length > 0)
                {
                    t0 = tm = t1 = caveFloor[rnd.Next(caveFloor.Length)];
                }
                for (int a = first; a <= last; a++)
                {
                    Point c = Cell(a, row);
                    if (!map.InBounds(c.X, c.Y))
                    {
                        continue;
                    }
                    bool edge = a == first || a == last;
                    ushort tile = a == first ? t0 : a == last ? t1 : tm;
                    if (row == 0 && s.Variant == CaveVariant.Invisible && caveFloor.Length > 0)
                    {
                        tile = caveFloor[rnd.Next(caveFloor.Length)];
                    }
                    int z = row switch
                    {
                        0 => groundZ,
                        1 => groundZ + (edge ? frameH : mouthH),
                        _ => groundZ + archH
                    };
                    map.SetLand(c.X, c.Y, tile, Math.Clamp(z, -128, 127));
                }
            }

            // mountain behind the top row and beside the frames: never lower than the entrance
            int mountainZ = int.MaxValue;
            if (s.WallAbove > 0)
            {
                int wallZ = Math.Clamp(groundZ + archH + s.WallAbove, -128, 127);
                for (int a = first - 1; a <= last + 1; a++)
                {
                    Point m = Cell(a, 3);
                    if (Mountain(m.X, m.Y) && map.LandZ(m.X, m.Y) < wallZ)
                    {
                        map.SetLand(m.X, m.Y, map.LandId(m.X, m.Y), wallZ);
                    }
                    if (map.InBounds(m.X, m.Y))
                    {
                        mountainZ = Math.Min(mountainZ, map.LandZ(m.X, m.Y));
                    }
                }
                foreach (int a in new[] { first - 1, last + 1 })
                {
                    Point side = Cell(a, 2);
                    if (Mountain(side.X, side.Y) && map.LandZ(side.X, side.Y) < groundZ + archH)
                    {
                        map.SetLand(side.X, side.Y, map.LandId(side.X, side.Y), groundZ + archH);
                    }
                }
            }

            // invisible statics on the ground of the mouth row (the original has one on every tile of the row)
            ushort[] nd = HexList.Parse(s.NoDrawStaticId);
            if (s.NoDrawStatics && nd.Length > 0)
            {
                var changes = new List<StaticChange>();
                for (int a = first; a <= last; a++)
                {
                    Point c = Cell(a, 1);
                    if (!map.InBounds(c.X, c.Y) || map.StaticsAt(c.X, c.Y).Any(st => st.Id == nd[0] && st.Z == groundZ))
                    {
                        continue;
                    }
                    changes.Add(MapData.Add(new StaticItem { Id = nd[0], X = c.X, Y = c.Y, Z = Math.Clamp(groundZ, -128, 127) }));
                }
                map.ApplyStatics(changes);
            }

            // path in front on the ground height
            ushort[] path = s.Variant == CaveVariant.Invisible ? caveFloor : HexList.Parse(s.PathTiles);
            if (s.Variant != CaveVariant.OnGround && s.PathLength > 0 && path.Length > 0)
            {
                int extra = Math.Max(0, s.PathExtraWidth);
                // beside the front row the path also lies on the ground (the corners 01E6 / 01E4 have dirt at the side)
                foreach (int a in new[] { first - 1, last + 1 })
                {
                    Point side = Cell(a, 0);
                    if (extra > 0 && Ground(side.X, side.Y))
                    {
                        map.SetLand(side.X, side.Y, path[rnd.Next(path.Length)], groundZ);
                    }
                }
                for (int row = -1; row >= -s.PathLength; row--)
                {
                    for (int a = first - extra / 2; a <= last + (extra + 1) / 2; a++)
                    {
                        Point p = Cell(a, row);
                        if (!Ground(p.X, p.Y))
                        {
                            continue;
                        }
                        int own = map.LandZ(p.X, p.Y);
                        int z = row == -1 ? groundZ : (groundZ + own) / 2;
                        map.SetLand(p.X, p.Y, path[rnd.Next(path.Length)], z);
                    }
                }
            }
            result.Ok = true;
            result.Facing = south ? CaveFacing.South : CaveFacing.East;
            result.GroundZ = groundZ;
            result.MountainZ = mountainZ == int.MaxValue ? groundZ + archH : mountainZ;
            result.Region = Rectangle.Inflate(south
                ? Rectangle.FromLTRB(first - 1, front - 2, last + 2, front + 2 + Math.Max(0, s.PathLength))
                : Rectangle.FromLTRB(front - 2, first - 1, front + 2 + Math.Max(0, s.PathLength), last + 2), 1, 1);
            result.Message = $"Höhleneingang ({s.Variant}) nach {(south ? "Süden" : "Osten")}: Boden {groundZ}, Rahmen +{frameH}, Mund +{mouthH}, oben +{archH}, " +
                             $"{width} breit + Rahmen" + (s.NoDrawStatics ? ", NODRAW-Statics" : string.Empty) + ".";
            return result;
        }
    }
}
