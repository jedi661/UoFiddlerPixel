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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
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

    /// <summary>Settings of the cave stamp (tab "Höhle / Weg").</summary>
    public sealed class CaveStampSettings
    {
        [Category("1 Eingang"), DisplayName("Richtung"), Description("Wohin der Eingang zeigt. Sichtbar sind nur Eingänge nach Süden oder Osten (die Kamera schaut von Südosten).")]
        public CaveFacing Facing { get; set; } = CaveFacing.Auto;

        [Category("1 Eingang"), DisplayName("Breite"), Description("Breite des Mundes in Tiles (Original: 2–4).")]
        public int Width { get; set; } = 3;

        [Category("1 Eingang"), DisplayName("Tiefe"), Description("Reihen in den Berg: 2 = Vorderkante + Bogen (Original), 3 = höherer Bogen.")]
        public int Depth { get; set; } = 2;

        [Category("1 Eingang"), DisplayName("Mund-Höhe"), Description("So hoch über dem Boden liegt die hintere Reihe (Bogen). Wird durch die Berg-Höhe begrenzt.")]
        public int MouthHeight { get; set; } = 20;

        [Category("1 Eingang"), DisplayName("Berg anheben"), Description("Ist der Berg hinter dem Eingang niedriger als der Bogen + diese Höhe, wird die Reihe dahinter so weit angehoben (0 = nie).")]
        public int WallAbove { get; set; } = 15;

        [Category("2 Weg davor"), DisplayName("Weg-Länge"), Description("Tiles Weg vor dem Eingang auf Bodenhöhe (0 = kein Weg).")]
        public int PathLength { get; set; } = 2;

        [Category("2 Weg davor"), DisplayName("Weg-Tiles"), Description("Land-Tiles (hex) des Weges, zufällig gemischt – z. B. Erde 0071–0078 oder Höhlenboden 0245–0249.")]
        public string PathTiles { get; set; } = "0071 0072 0073 0074 0075 0076 0077 0078";

        [Category("2 Weg davor"), DisplayName("Weg-Breite +"), Description("Der Weg ist so viele Tiles breiter als der Mund (je Seite die Hälfte).")]
        public int PathExtraWidth { get; set; } = 0;
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
        /// Cuts a complete entrance into the mountain face next to (<paramref name="cx"/>, <paramref name="cy"/>):
        /// finds where ground meets mountain, takes the ground height in front (whatever it is) and the mountain
        /// height behind, writes front row / arch / path. Call inside BeginLand / EndLand for one undo step.
        /// </summary>
        public static CaveStampResult Stamp(MapData map, int cx, int cy, CaveStampSettings s, CaveEntranceOptions o, Random rnd)
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
                int best = int.MinValue;
                for (int d = -1; d <= 4; d++)
                {
                    int f = (south ? cy : cx) - d;
                    bool ok = south ? Mountain(cx, f) && Ground(cx, f + 1) : Mountain(f, cy) && Ground(f + 1, cy);
                    if (ok && (best == int.MinValue || Math.Abs(f - (south ? cy : cx)) < Math.Abs(best - (south ? cy : cx))))
                    {
                        best = f;
                    }
                }
                return best;
            }
            int fs = s.Facing != CaveFacing.East ? FindFront(true) : int.MinValue;
            int fe = s.Facing != CaveFacing.South ? FindFront(false) : int.MinValue;
            bool southFacing;
            if (fs != int.MinValue && fe != int.MinValue)
            {
                southFacing = Math.Abs(fs - cy) <= Math.Abs(fe - cx);
            }
            else if (fs != int.MinValue)
            {
                southFacing = true;
            }
            else if (fe != int.MinValue)
            {
                southFacing = false;
            }
            else
            {
                result.Message = s.Facing == CaveFacing.Auto
                    ? "Kein Bergfuß gefunden: auf den Boden direkt vor einer Bergwand klicken, die nach Süden oder Osten zeigt."
                    : "Kein Bergfuß in dieser Richtung gefunden.";
                return result;
            }
            int front = southFacing ? fs : fe;
            int width = Math.Max(1, s.Width), depth = Math.Clamp(s.Depth, 2, 4);
            int along0 = (southFacing ? cx : cy) - width / 2;

            // map (along, depth row) -> cell. row 0 = front (first mountain row), row k = k rows into the mountain, row -1.. = path
            Point Cell(int along, int row) => southFacing ? new Point(along, front - row) : new Point(front - row, along);

            // ground: the open ground in front of every column (grass may lie higher – it is still ground)
            var grounds = new List<int>();
            for (int a = along0; a < along0 + width; a++)
            {
                Point g = Cell(a, -1);
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

            // mountain behind the arch
            int mountainZ = int.MaxValue;
            for (int a = along0; a < along0 + width; a++)
            {
                Point m = Cell(a, depth);
                if (map.InBounds(m.X, m.Y))
                {
                    mountainZ = Math.Min(mountainZ, map.LandZ(m.X, m.Y));
                }
            }
            if (mountainZ == int.MaxValue)
            {
                mountainZ = groundZ + s.MouthHeight;
            }
            int archZ = groundZ + Math.Max(5, s.MouthHeight);
            if (s.WallAbove > 0 && mountainZ < archZ + s.WallAbove)
            {
                // the mountain behind is too low for the arch: raise the row behind it (and the side columns)
                int wallZ = Math.Clamp(archZ + s.WallAbove, -128, 127);
                for (int a = along0 - 1; a <= along0 + width; a++)
                {
                    for (int row = depth; row <= depth + 1; row++)
                    {
                        Point m = Cell(a, row);
                        if (Mountain(m.X, m.Y) && map.LandZ(m.X, m.Y) < wallZ)
                        {
                            map.SetLand(m.X, m.Y, map.LandId(m.X, m.Y), wallZ);
                        }
                    }
                }
                mountainZ = Math.Max(mountainZ, wallZ);
            }
            else
            {
                archZ = Math.Min(archZ, mountainZ);
            }

            // region: front row on the ground, the rows behind rising to the arch height
            var heights = new Dictionary<Point, int>();
            for (int a = along0; a < along0 + width; a++)
            {
                for (int row = 0; row < depth; row++)
                {
                    Point c = Cell(a, row);
                    if (!map.InBounds(c.X, c.Y))
                    {
                        continue;
                    }
                    int z = row == 0 ? groundZ : groundZ + (archZ - groundZ) * row / (depth - 1);
                    heights[c] = z;
                    map.SetLand(c.X, c.Y, o.FillId, z);
                }
            }
            Rectangle region = southFacing
                ? new Rectangle(along0, front - depth + 1, width, depth)
                : new Rectangle(front - depth + 1, along0, depth, width);
            Rectangle retile = Rectangle.Inflate(region, 1, 1);
            Retile(map, retile, o, (x, y) => groundZ,
                (x, y, dz) => heights.TryGetValue(new Point(x, y), out int z)
                    ? (dz == int.MinValue || IsFrontRow(x, y) ? z : z + dz)
                    : (int?)null, rnd);

            // the front row keeps the ground height also where it gets a frame tile (01DA / 01D6 at the ends)
            bool IsFrontRow(int x, int y) => southFacing ? y == front : x == front;

            // path in front on the ground height
            ushort[] path = HexList.Parse(s.PathTiles);
            if (s.PathLength > 0 && path.Length > 0)
            {
                int extra = Math.Max(0, s.PathExtraWidth);
                for (int row = -1; row >= -s.PathLength; row--)
                {
                    for (int a = along0 - extra / 2; a < along0 + width + (extra + 1) / 2; a++)
                    {
                        Point p = Cell(a, row);
                        if (!Ground(p.X, p.Y))
                        {
                            continue;
                        }
                        // first row on the floor height, then blend into the ground
                        int own = map.LandZ(p.X, p.Y);
                        int z = row == -1 ? groundZ : (groundZ + own) / 2;
                        map.SetLand(p.X, p.Y, path[rnd.Next(path.Length)], z);
                    }
                }
            }
            result.Ok = true;
            result.Facing = southFacing ? CaveFacing.South : CaveFacing.East;
            result.GroundZ = groundZ;
            result.MountainZ = mountainZ;
            result.Region = region;
            result.Message = $"Höhleneingang nach {(southFacing ? "Süden" : "Osten")}: Boden {groundZ}, Bogen {archZ}, Berg dahinter {mountainZ}, {width}×{depth} Tiles.";
            return result;
        }
    }
}
