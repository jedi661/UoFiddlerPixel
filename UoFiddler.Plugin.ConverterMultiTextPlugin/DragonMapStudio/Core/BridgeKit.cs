/***************************************************************************
 *
 *  * Dragon Map Studio – bridges of statics
 *  *
 *  *  - finds the pieces in the tiledata (names like "bridge", "plank", "railing", "post", "pillar", "stairs")
 *  *    and tells from the art whether a piece runs west – east or north – south
 *  *  - a bridge plan (BridgeTemplate): pieces of a sample bridge along X (and Y), split into start, a repeated
 *  *    middle and end – a bridge of any length repeats the middle
 *  *  - builds a bridge from the plan: arch, pillars down to the ground / water, stairs at the ends
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ultima;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>One static of a bridge plan: position along / across the bridge, height over the deck.</summary>
    public sealed class BridgePart
    {
        public int Along { get; set; }
        public int Across { get; set; }
        public int Dz { get; set; }
        public ushort Id { get; set; }
        public ushort Hue { get; set; }

        public BridgePart Copy() => (BridgePart)MemberwiseClone();
    }

    /// <summary>Bridge plan: a sample bridge, the middle is repeated for longer bridges.</summary>
    public sealed class BridgeTemplate
    {
        [Category("Plan"), DisplayName("Name")]
        public string Name { get; set; } = "Brücke";

        [Category("Plan"), DisplayName("Kategorie"), Description("Ordner, in dem der Plan gespeichert wird (z. B. Holz, Stein, Dungeon).")]
        public string Category { get; set; } = "Allgemein";

        [Category("Plan"), DisplayName("Breite (Tiles)")]
        public int Width { get; set; } = 3;

        [Category("Plan"), DisplayName("Länge des Musters"), Description("So viele Tiles lang ist die Muster-Brücke.")]
        public int Length { get; set; } = 4;

        [Category("Plan"), DisplayName("Anfang (Tiles)"), Description("So viele Tiles am Anfang werden einmal gebaut (z. B. Endpfosten).")]
        public int StartLen { get; set; } = 1;

        [Category("Plan"), DisplayName("Ende (Tiles)"), Description("So viele Tiles am Ende werden einmal gebaut.")]
        public int EndLen { get; set; } = 1;

        [Category("Pfeiler"), DisplayName("Pfeiler (hex)"), Description("Stütze unter der Brücke, wird bis zum Boden / Wasser gestapelt. Leer = keine.")]
        public string Pillar { get; set; } = string.Empty;

        [Category("Pfeiler"), DisplayName("Pfeiler alle … Tiles")]
        public int PillarEvery { get; set; } = 6;

        [Category("Pfeiler"), DisplayName("Pfeiler an den Seiten"), Description("An = unter beiden Rändern, aus = unter der Mitte.")]
        public bool PillarAtSides { get; set; }

        [Category("Treppe"), DisplayName("Treppe Nord (hex)"), Description("Stufe, die man nach Norden hinuntergeht. Leer = Rampe aus Brettern.")]
        public string StairNorth { get; set; } = string.Empty;

        [Category("Treppe"), DisplayName("Treppe Ost (hex)")]
        public string StairEast { get; set; } = string.Empty;

        [Category("Treppe"), DisplayName("Treppe Süd (hex)")]
        public string StairSouth { get; set; } = string.Empty;

        [Category("Treppe"), DisplayName("Treppe West (hex)")]
        public string StairWest { get; set; } = string.Empty;

        [Category("Plan"), DisplayName("Notiz")]
        public string Note { get; set; } = string.Empty;

        /// <summary>Pieces of the sample bridge from west to east (Along = x, Across = y).</summary>
        [Browsable(false)]
        public List<BridgePart> PartsX { get; set; } = new List<BridgePart>();

        /// <summary>Pieces of the sample bridge from north to south (Along = y, Across = x); empty = turned from <see cref="PartsX"/>.</summary>
        [Browsable(false)]
        public List<BridgePart> PartsY { get; set; } = new List<BridgePart>();

        public override string ToString() => Category + " / " + Name;
    }

    /// <summary>Settings of the generator "Brücke aus Teilen".</summary>
    public sealed class BridgeGenSettings
    {
        [Category("Teile"), DisplayName("Bretter (hex)")]
        public string Deck { get; set; } = string.Empty;

        [Category("Teile"), DisplayName("Geländer West-Ost (hex)")]
        public string RailX { get; set; } = string.Empty;

        [Category("Teile"), DisplayName("Geländer Nord-Süd (hex)")]
        public string RailY { get; set; } = string.Empty;

        [Category("Teile"), DisplayName("Pfosten (hex)")]
        public string Post { get; set; } = string.Empty;

        [Category("Teile"), DisplayName("Pfeiler (hex)")]
        public string Pillar { get; set; } = string.Empty;

        [Category("Teile"), DisplayName("Lampe (hex)"), Description("Laterne auf dem Geländer (wie Bild 2), leer = keine.")]
        public string Lamp { get; set; } = string.Empty;

        [Category("Form"), DisplayName("Breite (Tiles)")]
        public int Width { get; set; } = 3;

        [Category("Form"), DisplayName("Pfosten alle … Tiles")]
        public int PostEvery { get; set; } = 2;

        [Category("Form"), DisplayName("Lampe alle … Tiles")]
        public int LampEvery { get; set; } = 6;

        [Category("Form"), DisplayName("Pfeiler alle … Tiles")]
        public int PillarEvery { get; set; } = 6;

        [Category("Form"), DisplayName("Geländer-Höhe (Z)"), Description("Geländer liegt so viel über den Brettern (Höhe der Bretter).")]
        public int RailDz { get; set; }
    }

    public static class BridgeKit
    {
        public enum Role
        {
            Deck,
            Rail,
            Post,
            Pillar,
            Stairs,
            Lamp,
            Wall
        }

        private static readonly Dictionary<Role, string[]> Words = new Dictionary<Role, string[]>
        {
            [Role.Deck] = new[] { "bridge", "plank", "board", "dock", "pier", "boardwalk", "wooden floor", "floor" },
            [Role.Rail] = new[] { "railing", "rail", "banister", "balustrade", "fence", "handrail" },
            [Role.Post] = new[] { "post", "newel", "fence post" },
            [Role.Pillar] = new[] { "pillar", "column", "support", "pylon", "beam", "pile", "strut" },
            [Role.Stairs] = new[] { "stair", "steps", "step" },
            [Role.Lamp] = new[] { "lamp post", "lamppost", "street lamp", "lantern", "lamp" },
            [Role.Wall] = new[] { "stone wall", "wall", "battlement", "parapet" }
        };

        /// <summary>German name of a role (for the UI).</summary>
        public static string RoleName(Role r) => r switch
        {
            Role.Deck => "Bretter / Boden",
            Role.Rail => "Geländer",
            Role.Post => "Pfosten",
            Role.Pillar => "Pfeiler / Stütze",
            Role.Stairs => "Treppe",
            Role.Lamp => "Lampe",
            _ => "Mauer"
        };

        private static readonly Dictionary<Role, List<int>> _found = new Dictionary<Role, List<int>>();

        public static void ClearCache()
        {
            lock (_found)
            {
                _found.Clear();
                _orient.Clear();
            }
        }

        /// <summary>
        /// Items of the tiledata for a role, best first: the name contains one of the words (earlier word = better),
        /// the flags fit (deck / stairs: surface or bridge, rail / post / wall: not a surface).
        /// </summary>
        public static List<int> Find(Role role, int max = 400)
        {
            lock (_found)
            {
                if (_found.TryGetValue(role, out List<int> cached))
                {
                    return cached.Take(max).ToList();
                }
            }
            var scored = new List<(int id, int score)>();
            int last;
            try
            {
                last = Math.Min(TileData.ItemTable.Length - 1, UoArtCache.MaxItemId);
            }
            catch (Exception)
            {
                return new List<int>();
            }
            string[] words = Words[role];
            for (int id = 1; id <= last; id++)
            {
                ItemData d;
                try
                {
                    d = TileData.ItemTable[id];
                }
                catch (Exception)
                {
                    break;
                }
                string name = (d.Name ?? string.Empty).Trim().ToLowerInvariant();
                if (name.Length == 0)
                {
                    continue;
                }
                int w = Array.FindIndex(words, k => name.Contains(k));
                if (w < 0)
                {
                    continue;
                }
                bool surface = (d.Flags & TileFlag.Surface) != 0;
                bool bridge = (d.Flags & TileFlag.Bridge) != 0;
                bool wall = (d.Flags & TileFlag.Wall) != 0;
                int score = 100 - w * 8;
                switch (role)
                {
                    case Role.Deck:
                        if (!surface || d.Height > 5)
                        {
                            continue;
                        }
                        score += name.Contains("bridge") ? 30 : 0;
                        score -= d.Height;
                        break;
                    case Role.Stairs:
                        if (!surface && !bridge)
                        {
                            continue;
                        }
                        score += bridge ? 10 : 0;
                        break;
                    case Role.Rail:
                    case Role.Post:
                        if (surface)
                        {
                            continue;
                        }
                        score -= Math.Max(0, d.Height - 12);
                        break;
                    case Role.Pillar:
                        if (surface && !wall)
                        {
                            continue;
                        }
                        score += wall ? 5 : 0;
                        break;
                    case Role.Wall:
                        if (!wall)
                        {
                            continue;
                        }
                        score += d.Height <= 12 ? 15 : 0;
                        break;
                    case Role.Lamp:
                        if (surface)
                        {
                            continue;
                        }
                        break;
                }
                if (UoArtCache.Static(id, 0) == null)
                {
                    continue;
                }
                scored.Add((id, score));
            }
            List<int> list = scored.OrderByDescending(s => s.score).ThenBy(s => s.id).Select(s => s.id).ToList();
            lock (_found)
            {
                _found[role] = list;
            }
            return list.Take(max).ToList();
        }

        // ------------------------------------------------------------------ direction from the art

        private static readonly Dictionary<int, int> _orient = new Dictionary<int, int>();

        /// <summary>
        /// Direction of a piece from its art: +1 = it runs west – east (on the screen from top left to bottom right "\"),
        /// −1 = north – south ("/"), 0 = round / square / unknown. Uses the covariance of the opaque pixels.
        /// </summary>
        public static int Orientation(int id)
        {
            lock (_found)
            {
                if (_orient.TryGetValue(id, out int o))
                {
                    return o;
                }
            }
            int result = 0;
            Bitmap art = UoArtCache.Static(id, 0);
            if (art != null)
            {
                try
                {
                    result = OrientationOf(art);
                }
                catch (Exception)
                {
                    result = 0;
                }
            }
            lock (_found)
            {
                _orient[id] = result;
            }
            return result;
        }

        public static int OrientationOf(Bitmap art)
        {
            int w = art.Width, h = art.Height;
            var data = art.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var px = new int[w * h];
                for (int y = 0; y < h; y++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0 + y * data.Stride, px, y * w, w);
                }
                double n = 0, sx = 0, sy = 0, sxx = 0, syy = 0, sxy = 0;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (((uint)px[y * w + x] >> 24) < 128)
                        {
                            continue;
                        }
                        n++;
                        sx += x;
                        sy += y;
                        sxx += x * (double)x;
                        syy += y * (double)y;
                        sxy += x * (double)y;
                    }
                }
                if (n < 12)
                {
                    return 0;
                }
                double cxy = sxy / n - sx / n * (sy / n);
                double cxx = sxx / n - sx / n * (sx / n), cyy = syy / n - sy / n * (sy / n);
                double corr = cxy / Math.Sqrt(Math.Max(1e-6, cxx * cyy));
                // a tall thin piece (post, lamp) has no direction
                if (Math.Abs(corr) < 0.25 || w < 20)
                {
                    return 0;
                }
                return corr > 0 ? 1 : -1;
            }
            finally
            {
                art.UnlockBits(data);
            }
        }

        /// <summary>
        /// The same piece for the other direction: an item with the same name close by (±12) whose art runs the other way;
        /// the id itself when it has no direction or none is found.
        /// </summary>
        public static ushort Turned(ushort id)
        {
            int o = Orientation(id);
            if (o == 0)
            {
                return id;
            }
            string name = UoArtCache.ItemName(id).Trim().ToLowerInvariant();
            for (int d = 1; d <= 12; d++)
            {
                foreach (int c in new[] { id + d, id - d })
                {
                    if (c <= 0 || c > UoArtCache.MaxItemId)
                    {
                        continue;
                    }
                    if (UoArtCache.ItemName(c).Trim().ToLowerInvariant() == name && Orientation(c) == -o)
                    {
                        return (ushort)c;
                    }
                }
            }
            return id;
        }

        // ------------------------------------------------------------------ automatic pieces

        /// <summary>First good pieces from the tiledata: deck, railing for both directions, post, pillar, lamp, stairs.</summary>
        public static BridgeGenSettings AutoPieces()
        {
            var g = new BridgeGenSettings();
            List<int> deck = Find(Role.Deck, 40);
            g.Deck = string.Join(" ", deck.Take(2).Select(i => i.ToString("x4")));
            List<int> rail = Find(Role.Rail, 80);
            int rx = rail.FirstOrDefault(i => Orientation(i) > 0);
            int ry = rx != 0 ? Turned((ushort)rx) : 0;
            if (ry == rx)
            {
                ry = rail.FirstOrDefault(i => Orientation(i) < 0);
            }
            g.RailX = rx != 0 ? rx.ToString("x4") : string.Empty;
            g.RailY = ry != 0 ? ry.ToString("x4") : string.Empty;
            int post = Find(Role.Post, 40).FirstOrDefault(i => Orientation(i) == 0);
            g.Post = post != 0 ? post.ToString("x4") : string.Empty;
            int pillar = Find(Role.Pillar, 40).FirstOrDefault();
            g.Pillar = pillar != 0 ? pillar.ToString("x4") : string.Empty;
            return g;
        }

        /// <summary>Stairs of the tiledata sorted by direction (from the flags / art): N, E, S, W – empty when none.</summary>
        public static StairSet AutoStairs()
        {
            var set = new StairSet { Name = "Treppe (TileData)" };
            List<int> stairs = Find(Role.Stairs, 60);
            // in the original art the stair pieces come in groups of four directions with the same name
            foreach (int id in stairs)
            {
                string name = UoArtCache.ItemName(id).Trim().ToLowerInvariant();
                var group = Enumerable.Range(id, 4).Where(c => UoArtCache.ItemName(c).Trim().ToLowerInvariant() == name).ToList();
                if (group.Count == 4)
                {
                    // order of the original stair groups: south, east, north, west (up direction) → walking down the other way
                    set.DownNorth = group[0].ToString("x4");
                    set.DownWest = group[1].ToString("x4");
                    set.DownSouth = group[2].ToString("x4");
                    set.DownEast = group[3].ToString("x4");
                    set.Name = "Treppe " + name + $" ({id:x4})";
                    return set;
                }
            }
            return set;
        }

        /// <summary>A bridge plan from pieces: deck, railings with posts, lamps, pillars (sample 1 + period + 1 tiles).</summary>
        public static BridgeTemplate Generate(BridgeGenSettings g, string name)
        {
            int w = Math.Max(1, g.Width);
            int every = Math.Max(1, g.PostEvery);
            int lampEvery = Math.Max(every, g.LampEvery);
            // the middle must repeat posts and lamps: least common multiple
            int period = Lcm(every, string.IsNullOrWhiteSpace(g.Lamp) ? every : lampEvery);
            var t = new BridgeTemplate
            {
                Name = name, Width = w, StartLen = 1, EndLen = 1, Length = period + 2, Pillar = g.Pillar, PillarEvery = Math.Max(2, g.PillarEvery),
                Note = "Aus Teilen erzeugt"
            };
            ushort[] deck = HexList.Parse(g.Deck);
            ushort[] railX = HexList.Parse(g.RailX);
            ushort[] railY = HexList.Parse(g.RailY);
            ushort[] post = HexList.Parse(g.Post);
            ushort[] lamp = HexList.Parse(g.Lamp);
            for (int dir = 0; dir < 2; dir++)
            {
                List<BridgePart> parts = dir == 0 ? t.PartsX : t.PartsY;
                ushort[] rail = dir == 0 ? railX : railY.Length > 0 ? railY : railX.Select(Turned).ToArray();
                for (int a = 0; a < t.Length; a++)
                {
                    bool end = a == 0 || a == t.Length - 1;
                    bool postHere = end || (a - 1) % every == 0;
                    bool lampHere = lamp.Length > 0 && !end && (a - 1) % lampEvery == lampEvery / 2;
                    for (int c = 0; c < w; c++)
                    {
                        if (deck.Length > 0)
                        {
                            parts.Add(new BridgePart { Along = a, Across = c, Dz = 0, Id = deck[(a + c) % deck.Length] });
                        }
                        bool side = (c == 0 || c == w - 1) && w >= 2;
                        if (!side)
                        {
                            continue;
                        }
                        if (rail.Length > 0)
                        {
                            parts.Add(new BridgePart { Along = a, Across = c, Dz = g.RailDz, Id = rail[a % rail.Length] });
                        }
                        if (postHere && post.Length > 0)
                        {
                            parts.Add(new BridgePart { Along = a, Across = c, Dz = g.RailDz, Id = post[0] });
                        }
                        if (lampHere)
                        {
                            parts.Add(new BridgePart { Along = a, Across = c, Dz = g.RailDz, Id = lamp[0] });
                        }
                    }
                }
            }
            return t;
        }

        private static int Lcm(int a, int b)
        {
            int x = a, y = b;
            while (y != 0)
            {
                (x, y) = (y, x % y);
            }
            return Math.Max(1, a / Math.Max(1, x) * b);
        }

        /// <summary>
        /// A plan from statics of a map (the designer or a real bridge): the marked area, the bridge runs along X or Y,
        /// the deck height is <paramref name="deckZ"/>. Pieces of the whole area become the sample.
        /// </summary>
        public static BridgeTemplate Capture(MapData map, Rectangle r, bool alongX, int deckZ, string name)
        {
            var t = new BridgeTemplate { Name = name, Width = alongX ? r.Height : r.Width, Length = alongX ? r.Width : r.Height };
            t.StartLen = Math.Min(1, t.Length);
            t.EndLen = t.Length > 2 ? 1 : 0;
            List<BridgePart> parts = alongX ? t.PartsX : t.PartsY;
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    foreach (StaticItem s in map.StaticsAt(x, y))
                    {
                        parts.Add(new BridgePart
                        {
                            Along = alongX ? x - r.Left : y - r.Top, Across = alongX ? y - r.Top : x - r.Left, Dz = s.Z - deckZ, Id = s.Id, Hue = s.Hue
                        });
                    }
                }
            }
            return t;
        }

        /// <summary>The other direction: along ↔ across with the turned pieces (same name, art the other way).</summary>
        public static List<BridgePart> Turn(IEnumerable<BridgePart> parts) =>
            parts.Select(p => new BridgePart { Along = p.Along, Across = p.Across, Dz = p.Dz, Id = Turned(p.Id), Hue = p.Hue }).ToList();

        // ------------------------------------------------------------------ build

        /// <summary>Column of the sample for tile k of a bridge with n tiles: start, repeated middle, end.</summary>
        public static int SampleColumn(BridgeTemplate t, int k, int n)
        {
            int len = Math.Max(1, t.Length);
            int s = Math.Clamp(t.StartLen, 0, len), e = Math.Clamp(t.EndLen, 0, len - s);
            int mid = len - s - e;
            if (k < s && k < n - e)
            {
                return k;
            }
            if (k >= n - e)
            {
                return len - (n - k);
            }
            if (mid <= 0)
            {
                return Math.Max(0, s - 1);
            }
            return s + (k - s) % mid;
        }

        /// <summary>
        /// Bridge from a to b (snapped to west – east or north – south) after the plan: arch, pillars down to the ground,
        /// stairs at both ends when the deck lies higher than the ground there.
        /// </summary>
        public static List<StaticChange> Build(MapData map, Point a, Point b, BridgeTemplate t, BridgeSettings o, Random rnd)
        {
            var statics = new List<StaticChange>();
            if (t == null || t.PartsX.Count == 0 && t.PartsY.Count == 0)
            {
                return statics;
            }
            bool alongX = Math.Abs(b.X - a.X) >= Math.Abs(b.Y - a.Y);
            var end = alongX ? new Point(b.X, a.Y) : new Point(a.X, b.Y);
            int n = Math.Max(Math.Abs(end.X - a.X), Math.Abs(end.Y - a.Y)) + 1;
            int dir = alongX ? Math.Sign(end.X - a.X) : Math.Sign(end.Y - a.Y);
            if (dir == 0)
            {
                dir = 1;
            }
            List<BridgePart> parts = alongX ? t.PartsX : t.PartsY;
            if (parts.Count == 0)
            {
                parts = Turn(alongX ? t.PartsY : t.PartsX);
            }
            // pieces by sample column
            var byCol = parts.GroupBy(p => p.Along).ToDictionary(g => g.Key, g => g.ToList());
            int width = Math.Max(1, t.Width);
            int lo = -(width - 1) / 2;
            // the sample runs west → east / north → south; drawn the other way, the column order is mirrored
            Point first = dir > 0 ? a : end;
            int baseZ = Math.Max(map.LandZ(a.X, a.Y), map.LandZ(end.X, end.Y)) + o.ZOffset;
            var deckZ = new int[n];
            // arch of stairs: steps of 5 z up to the arch height, flat on top, down again on the other side
            ushort upStair = HexList.Parse(alongX ? t.StairWest : t.StairNorth).FirstOrDefault();
            ushort downStair = HexList.Parse(alongX ? t.StairEast : t.StairSouth).FirstOrDefault();
            bool stairArch = o.ArchStairs && o.Arch >= 5 && upStair != 0 && downStair != 0;
            int rise = stairArch ? Math.Min(o.Arch / 5, (n - 1) / 2) : 0;
            for (int k = 0; k < n; k++)
            {
                double f = n <= 1 ? 0 : k / (double)(n - 1);
                ushort stepHere = 0;
                if (stairArch)
                {
                    int s = Math.Min(rise, Math.Min(k + 1, n - k));
                    deckZ[k] = Math.Clamp(baseZ + 5 * s, -128, 127);
                    if (k < rise)
                    {
                        stepHere = upStair; // walking back to the start goes down
                    }
                    else if (n - 1 - k < rise)
                    {
                        stepHere = downStair;
                    }
                }
                else
                {
                    deckZ[k] = Math.Clamp(baseZ + (int)Math.Round(o.Arch * Math.Sin(Math.PI * f)), -128, 127);
                }
                int col = SampleColumn(t, k, n);
                if (!byCol.TryGetValue(col, out List<BridgePart> list))
                {
                    continue;
                }
                foreach (BridgePart p in list)
                {
                    if (!o.Railings && p.Dz >= 0 && IsRailLike(p.Id))
                    {
                        continue;
                    }
                    int across = lo + p.Across;
                    int x = alongX ? first.X + k : first.X + across;
                    int y = alongX ? first.Y + across : first.Y + k;
                    if (!map.InBounds(x, y))
                    {
                        continue;
                    }
                    if (stepHere != 0 && p.Dz == 0 && !IsRailLike(p.Id) && !IsRailPart(t, p))
                    {
                        // the deck piece becomes a step (its top is the deck height)
                        statics.Add(MapData.Add(new StaticItem { Id = stepHere, X = x, Y = y, Z = Math.Clamp(deckZ[k] - 5, -128, 127) }));
                        continue;
                    }
                    statics.Add(MapData.Add(new StaticItem { Id = p.Id, Hue = p.Hue, X = x, Y = y, Z = Math.Clamp(deckZ[k] + p.Dz, -128, 127) }));
                }
            }
            // pillars: stacked from below the deck down to the ground / water
            ushort[] pillar = HexList.Parse(t.Pillar);
            if (pillar.Length > 0 && n > 2)
            {
                int every = Math.Max(2, t.PillarEvery);
                int h = Math.Max(3, UoArtCache.ItemHeight(pillar[0]));
                var rows = t.PillarAtSides && width > 1 ? new[] { lo, lo + width - 1 } : new[] { lo + (width - 1) / 2 };
                for (int k = every / 2; k < n - 1; k += every)
                {
                    if (k == 0)
                    {
                        continue;
                    }
                    foreach (int across in rows)
                    {
                        int x = alongX ? first.X + k : first.X + across;
                        int y = alongX ? first.Y + across : first.Y + k;
                        if (!map.InBounds(x, y))
                        {
                            continue;
                        }
                        int ground = map.LandZ(x, y);
                        for (int z = deckZ[k] - h, i = 0; z + h > ground && i < 30; z -= h, i++)
                        {
                            statics.Add(MapData.Add(new StaticItem { Id = pillar[rnd.Next(pillar.Length)], X = x, Y = y, Z = Math.Clamp(z, -128, 127) }));
                        }
                    }
                }
            }
            // stairs at both ends, when the deck is higher than the ground in front of it
            if (o.Stairs)
            {
                for (int side = 0; side < 2; side++)
                {
                    int k = side == 0 ? 0 : n - 1;
                    int step = side == 0 ? -1 : 1; // outward along the bridge (in sample direction)
                    Compass down = alongX ? (step < 0 ? Compass.West : Compass.East) : (step < 0 ? Compass.North : Compass.South);
                    ushort[] ids = HexList.Parse(down switch
                    {
                        Compass.North => t.StairNorth,
                        Compass.East => t.StairEast,
                        Compass.South => t.StairSouth,
                        _ => t.StairWest
                    });
                    int top = deckZ[k];
                    for (int s = 1, z = top - 5; s <= 12; s++, z -= 5)
                    {
                        int along = k + step * s;
                        bool any = false;
                        for (int c = 0; c < width; c++)
                        {
                            int across = lo + c;
                            int x = alongX ? first.X + along : first.X + across;
                            int y = alongX ? first.Y + across : first.Y + along;
                            if (!map.InBounds(x, y) || map.LandZ(x, y) >= z + 5 - 2)
                            {
                                continue;
                            }
                            any = true;
                            ushort id = ids.Length > 0 ? ids[0] : (ushort)0;
                            if (id == 0)
                            {
                                continue;
                            }
                            // the step plus filling down to the ground
                            for (int zz = z, i = 0; zz > map.LandZ(x, y) - 3 && i < 20; zz -= 5, i++)
                            {
                                statics.Add(MapData.Add(new StaticItem { Id = id, X = x, Y = y, Z = Math.Clamp(zz, -128, 127) }));
                            }
                        }
                        if (!any)
                        {
                            break;
                        }
                    }
                }
            }
            return statics;
        }

        /// <summary>A piece on the side rows that is not the deck (railing / post / lamp of a generated plan stand on the edge rows).</summary>
        private static bool IsRailPart(BridgeTemplate t, BridgePart p) =>
            (p.Across == 0 || p.Across == t.Width - 1) && t.Width >= 2 &&
            (t.PartsX.Concat(t.PartsY).Count(q => q.Along == p.Along && q.Across == p.Across && q.Dz == p.Dz) > 1 && !IsDeckId(t, p.Id));

        private static bool IsDeckId(BridgeTemplate t, ushort id)
        {
            // deck = what lies on height 0 in the inner rows (or the most common piece on 0 for narrow bridges)
            var all = t.PartsX.Concat(t.PartsY).Where(q => q.Dz == 0).ToList();
            var inner = all.Where(q => q.Across > 0 && q.Across < t.Width - 1).Select(q => q.Id).ToList();
            if (inner.Count > 0)
            {
                return inner.Contains(id);
            }
            return all.GroupBy(q => q.Id).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() == id;
        }

        private static bool IsRailLike(ushort id)
        {
            string n = UoArtCache.ItemName(id).ToLowerInvariant();
            return Words[Role.Rail].Any(n.Contains) || Words[Role.Post].Any(n.Contains) || Words[Role.Lamp].Any(n.Contains);
        }

        // ------------------------------------------------------------------ files

        public static string Folder => Path.Combine(StudioSettings.SettingsFolder, "Bruecken");

        private static string Safe(string s) => string.Concat((s ?? "x").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();

        public static string Save(BridgeTemplate t)
        {
            string dir = Path.Combine(Folder, Safe(string.IsNullOrWhiteSpace(t.Category) ? "Allgemein" : t.Category));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, Safe(t.Name) + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(t, new JsonSerializerOptions { WriteIndented = true }));
            return path;
        }

        public static List<BridgeTemplate> LoadAll()
        {
            var list = new List<BridgeTemplate>();
            if (!Directory.Exists(Folder))
            {
                return list;
            }
            foreach (string f in Directory.GetFiles(Folder, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    BridgeTemplate t = JsonSerializer.Deserialize<BridgeTemplate>(File.ReadAllText(f));
                    if (t != null)
                    {
                        t.Category = Path.GetFileName(Path.GetDirectoryName(f));
                        list.Add(t);
                    }
                }
                catch (Exception)
                {
                    // broken file – skip
                }
            }
            return list.OrderBy(t => t.Category).ThenBy(t => t.Name).ToList();
        }

        public static void Delete(BridgeTemplate t)
        {
            string path = Path.Combine(Folder, Safe(t.Category), Safe(t.Name) + ".json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
