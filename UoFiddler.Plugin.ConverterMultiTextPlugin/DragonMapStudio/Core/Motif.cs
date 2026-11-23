/***************************************************************************
 *
 *  * Dragon Map Studio – motifs ("Muster-Baukasten"): pieces built by hand (battlements, tents, a wall with windows,
 *  *  a fortress wall with towers …) with a version for every direction (north, east, south, west).
 *  *
 *  *  Drawn by clicking (once) or by dragging: the motif repeats in the drag direction, using the version of that
 *  *  direction. Height: every piece on the ground, all on the height at the start, or rising (stairs inside are
 *  *  found: every repeat goes up by their height). Several floors in one go.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum MotifHeight
    {
        [Description("Alle auf der Höhe am Start (gerade, z. B. Zinnen)")]
        Level,

        [Description("Jedes Stück auf dem Boden (folgt dem Gelände)")]
        Ground,

        [Description("Steigend (Treppen: jedes Stück höher)")]
        Rise
    }

    public enum MotifRepeat
    {
        [Description("Reihe fortsetzen (Mauer wird länger)")]
        Along,

        [Description("Kopien nebeneinander (hintereinander gestellt)")]
        Across
    }

    public sealed class MotifDrawSettings
    {
        [Category("Zeichnen"), DisplayName("Wiederholen"), Description("Reihe fortsetzen: beim Ziehen wird die Version genommen, die in Zieh-Richtung läuft – eine Mauer wird länger. Kopien nebeneinander: das Muster so wie es ist mehrmals hintereinander stellen.")]
        public MotifRepeat Repeat { get; set; } = MotifRepeat.Along;

        [Category("Zeichnen"), DisplayName("Ziehen wiederholt"), Description("An = ziehen wiederholt das Muster in Zieh-Richtung. Aus = jeder Klick / Zug setzt es genau einmal.")]
        public bool Drag { get; set; } = true;

        [Category("Zeichnen"), DisplayName("Richtung beim Klick"), Description("Welche Richtung ein einfacher Klick setzt (beim Ziehen bestimmt die Zieh-Richtung die Richtung). Im Werkzeug dreht R.")]
        public Compass Direction { get; set; } = Compass.East;

        [Category("Höhe"), DisplayName("Höhe"), Description("Gerade auf der Höhe am Start, jedes Stück auf dem Boden, oder steigend wie eine Treppe.")]
        public MotifHeight Height { get; set; } = MotifHeight.Level;

        [Category("Höhe"), DisplayName("Anstieg je Stück (Z)"), Description("Nur 'steigend': so viel höher ist jedes weitere Stück. 0 = automatisch (Treppen im Muster: ihre Höhe, sonst 5).")]
        public int Rise { get; set; }

        [Category("Höhe"), DisplayName("Z +"), Description("Zur Höhe addieren.")]
        public int ZOffset { get; set; }

        [Category("Etagen"), DisplayName("Etagen"), Description("So oft übereinander in einem Zug (z. B. 2–3 Etagen einer Mauer mit Fenstern).")]
        public int Floors { get; set; } = 1;

        [Category("Etagen"), DisplayName("Etagen-Höhe (Z)"), Description("Abstand der Etagen. 0 = Höhe des Musters.")]
        public int FloorHeight { get; set; }

        [Category("Zeichnen"), DisplayName("Abstand (Tiles)"), Description("Lücke zwischen den Wiederholungen (Zelte, Pfosten …).")]
        public int Gap { get; set; }

        [Category("Zeichnen"), DisplayName("Alte Statics ersetzen"), Description("Statics auf den Feldern des Musters vorher entfernen.")]
        public bool Replace { get; set; }
    }

    public sealed class Motif
    {
        [Category("Muster"), DisplayName("Name")]
        public string Name { get; set; } = "Mein Muster";

        [Category("Muster"), DisplayName("Kategorie"), Description("Ordner, z. B. Burg, Zelte, Mauern, Häuser.")]
        public string Category { get; set; } = "Allgemein";

        [Category("Muster"), DisplayName("Notiz")]
        public string Note { get; set; } = string.Empty;

        /// <summary>Directions this motif may be drawn in (e.g. only north – south). Not allowed ones are never turned or drawn.</summary>
        [Browsable(false)]
        public bool AllowNorth { get; set; } = true;

        [Browsable(false)]
        public bool AllowEast { get; set; } = true;

        [Browsable(false)]
        public bool AllowSouth { get; set; } = true;

        [Browsable(false)]
        public bool AllowWest { get; set; } = true;

        public bool Allowed(Compass c) => c switch
        {
            Compass.North => AllowNorth,
            Compass.East => AllowEast,
            Compass.South => AllowSouth,
            _ => AllowWest
        };

        public void SetAllowed(Compass c, bool on)
        {
            switch (c)
            {
                case Compass.North:
                    AllowNorth = on;
                    break;
                case Compass.East:
                    AllowEast = on;
                    break;
                case Compass.South:
                    AllowSouth = on;
                    break;
                default:
                    AllowWest = on;
                    break;
            }
            if (!AllowNorth && !AllowEast && !AllowSouth && !AllowWest)
            {
                AllowEast = true;
            }
        }

        /// <summary>Allowed directions as text ("Nord, Süd").</summary>
        public string AllowedText() => string.Join(", ", new[] { Compass.North, Compass.East, Compass.South, Compass.West }.Where(Allowed).Select(DirName));

        /// <summary>
        /// The axis a set of pieces runs along: +1 = west – east (x), −1 = north – south (y), 0 = square / unknown.
        /// From the footprint; a single row / square uses the art of the walls in it.
        /// </summary>
        public static int AxisOf(List<PieceStatic> v)
        {
            if (v == null || v.Count == 0)
            {
                return 0;
            }
            int w = v.Max(p => p.X) - v.Min(p => p.X) + 1, h = v.Max(p => p.Y) - v.Min(p => p.Y) + 1;
            if (w != h)
            {
                return w > h ? 1 : -1;
            }
            int sum = 0;
            foreach (ushort id in v.Select(p => p.Id).Distinct())
            {
                try
                {
                    sum += BridgeKit.Orientation(id);
                }
                catch (Exception)
                {
                    // no art (tests without client files)
                }
            }
            return Math.Sign(sum);
        }

        /// <summary>The axis a direction runs along: east / west = +1 (x), north / south = −1 (y).</summary>
        public static int AxisOf(Compass c) => c == Compass.East || c == Compass.West ? 1 : -1;

        [Browsable(false)]
        public List<PieceStatic> North { get; set; } = new List<PieceStatic>();

        [Browsable(false)]
        public List<PieceStatic> East { get; set; } = new List<PieceStatic>();

        [Browsable(false)]
        public List<PieceStatic> South { get; set; } = new List<PieceStatic>();

        [Browsable(false)]
        public List<PieceStatic> West { get; set; } = new List<PieceStatic>();

        public override string ToString() => Category + " / " + Name;

        public List<PieceStatic> Variant(Compass c) => c switch
        {
            Compass.North => North,
            Compass.East => East,
            Compass.South => South,
            _ => West
        };

        public void SetVariant(Compass c, List<PieceStatic> parts)
        {
            switch (c)
            {
                case Compass.North:
                    North = parts;
                    break;
                case Compass.East:
                    East = parts;
                    break;
                case Compass.South:
                    South = parts;
                    break;
                default:
                    West = parts;
                    break;
            }
        }

        public bool Has(Compass c) => Variant(c).Count > 0;

        public static string DirName(Compass c) => c switch
        {
            Compass.North => "Nord",
            Compass.East => "Ost",
            Compass.South => "Süd",
            _ => "West"
        };

        /// <summary>The statics of a rectangle (or only <paramref name="only"/>), heights relative to <paramref name="groundZ"/>.</summary>
        public static List<PieceStatic> Capture(MapData map, Rectangle r, int groundZ, ICollection<StaticItem> only = null)
        {
            var list = new List<PieceStatic>();
            IEnumerable<StaticItem> items = only ?? Enumerable.Range(r.Top, r.Height).SelectMany(y => Enumerable.Range(r.Left, r.Width).SelectMany(x => map.StaticsAt(x, y)));
            List<StaticItem> all = items.ToList();
            if (all.Count == 0)
            {
                return list;
            }
            int minX = all.Min(s => s.X), minY = all.Min(s => s.Y);
            foreach (StaticItem s in all)
            {
                list.Add(new PieceStatic { X = s.X - minX, Y = s.Y - minY, Z = s.Z - groundZ, Id = s.Id, Hue = s.Hue });
            }
            return list;
        }

        /// <summary>A version turned from another: positions turn, stair pieces by their slope, walls / railings by their art, blocks stay.</summary>
        public static List<PieceStatic> Turn(List<PieceStatic> parts, Compass from, Compass to)
        {
            int q = (((int)to - (int)from) % 4 + 4) % 4;
            var turned = parts.Select(p =>
            {
                int x = p.X, y = p.Y;
                for (int i = 0; i < q; i++)
                {
                    (x, y) = (-y, x);
                }
                return new PieceStatic { X = x, Y = y, Z = p.Z, Id = StairModel.TurnedPiece(p.Id, q), Hue = p.Hue };
            }).ToList();
            if (turned.Count > 0)
            {
                int minX = turned.Min(p => p.X), minY = turned.Min(p => p.Y);
                foreach (PieceStatic p in turned)
                {
                    p.X -= minX;
                    p.Y -= minY;
                }
            }
            return turned;
        }

        /// <summary>The missing directions turned from a built one (the first built one, or <paramref name="from"/>).</summary>
        public int AutoVariants(Compass? from = null, bool overwrite = false)
        {
            Compass src = from ?? new[] { Compass.East, Compass.South, Compass.North, Compass.West }.Where(Allowed).Concat(new[] { Compass.East, Compass.South, Compass.North, Compass.West }).FirstOrDefault(Has);
            if (!Has(src))
            {
                return 0;
            }
            int made = 0;
            foreach (Compass c in new[] { Compass.North, Compass.East, Compass.South, Compass.West })
            {
                if (!Allowed(c))
                {
                    continue;
                }
                if (c != src && (overwrite || !Has(c)))
                {
                    SetVariant(c, Turn(Variant(src), src, c));
                    made++;
                }
            }
            return made;
        }

        // ------------------------------------------------------------------ measures

        private static int Top(PieceStatic p) => p.Z + Math.Max(0, UoArtCache.StackHeight(p.Id));

        /// <summary>Height of the version (bottom to top of the highest piece).</summary>
        public int HeightOf(Compass c)
        {
            List<PieceStatic> v = Variant(c);
            return v.Count == 0 ? 0 : v.Max(Top) - v.Min(p => p.Z);
        }

        /// <summary>Stairs in the version: their rise (lowest to highest step + one step), 0 = none.</summary>
        public int StairRise(Compass c)
        {
            var steps = Variant(c).Where(p => StairModel.UpOf(p.Id) != null && UoArtCache.ItemHeight(p.Id) <= 6).ToList();
            return steps.Count == 0 ? 0 : steps.Max(p => p.Z) - steps.Min(p => p.Z) + 5;
        }

        // ------------------------------------------------------------------ drawing

        /// <summary>
        /// The pieces for a click (a == b: once in the click direction) or a drag (repeated towards b).
        /// <paramref name="ground"/> gives the land height (CGI) – null = heights stay relative (map editor).
        /// Returns (x, y, z, id, hue) and the used direction.
        /// </summary>
        public List<(int x, int y, int z, ushort id, ushort hue)> Layout(Point a, Point b, MotifDrawSettings o, Func<int, int, int> ground, out Compass dir, out int repeats)
        {
            var result = new List<(int, int, int, ushort, ushort)>();
            bool drag = o.Drag && a != b;
            Compass want = drag ? Staircase.DirectionOf(a, b) : o.Direction;
            if (!new[] { Compass.North, Compass.East, Compass.South, Compass.West }.Any(c => Allowed(c) && Has(c)))
            {
                AutoVariants();
            }
            dir = ChooseVariant(want, drag && o.Repeat == MotifRepeat.Along);
            List<PieceStatic> v = Variant(dir);
            repeats = 0;
            if (v.Count == 0)
            {
                return result;
            }

            // where the copies go: along the drag; "Reihe fortsetzen" with a direction that runs the other way
            // (only north – south allowed, dragged east …): along the axis of the motif, as far as the drag reaches there
            Compass step = want;
            if (drag && o.Repeat == MotifRepeat.Along)
            {
                int axis = AxisOf(v);
                if (axis == 0)
                {
                    axis = AxisOf(dir);
                }
                if (axis != AxisOf(want))
                {
                    step = axis > 0 ? (b.X >= a.X ? Compass.East : Compass.West) : (b.Y >= a.Y ? Compass.South : Compass.North);
                }
            }
            int w = v.Max(p => p.X) + 1, h = v.Max(p => p.Y) + 1;
            bool alongX = step == Compass.East || step == Compass.West;
            int period = (alongX ? w : h) + Math.Max(0, o.Gap);
            int dist = alongX ? Math.Abs(b.X - a.X) : Math.Abs(b.Y - a.Y);
            int n = drag ? dist / Math.Max(1, period) + 1 : 1;
            repeats = n;
            int rise = o.Height != MotifHeight.Rise ? 0 : o.Rise > 0 ? o.Rise : Math.Max(StairRise(dir), 5);
            int floorH = o.FloorHeight > 0 ? o.FloorHeight : Math.Max(1, HeightOf(dir));
            int startZ = (ground?.Invoke(a.X, a.Y) ?? 0) + o.ZOffset;
            for (int k = 0; k < n; k++)
            {
                int ox = a.X, oy = a.Y;
                switch (step)
                {
                    case Compass.East:
                        ox = a.X + k * period;
                        break;
                    case Compass.West:
                        ox = a.X - (k + 1) * period + 1 + Math.Max(0, o.Gap);
                        break;
                    case Compass.South:
                        oy = a.Y + k * period;
                        break;
                    default:
                        oy = a.Y - (k + 1) * period + 1 + Math.Max(0, o.Gap);
                        break;
                }
                int baseZ = o.Height == MotifHeight.Ground && ground != null ? ground(ox, oy) + o.ZOffset : startZ + rise * k;
                for (int f = 0; f < Math.Clamp(o.Floors, 1, 20); f++)
                {
                    foreach (PieceStatic p in v)
                    {
                        result.Add((ox + p.X, oy + p.Y, Math.Clamp(baseZ + p.Z + f * floorH, -128, 127), p.Id, p.Hue));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// The version to draw for a wanted direction: only allowed, built ones (missing allowed ones are turned).
        /// <paramref name="alongDrag"/>: prefer a version that runs along the wanted direction (a wall continues instead of
        /// standing side by side) – the wanted one, its opposite, then the others.
        /// </summary>
        public Compass ChooseVariant(Compass want, bool alongDrag)
        {
            var order = new List<Compass> { want, Staircase.Opposite(want), Staircase.RightOf(want), Staircase.LeftOf(want) };
            List<Compass> usable = order.Where(c => Allowed(c) && Has(c)).ToList();
            if (usable.Count == 0)
            {
                AutoVariants();
                usable = order.Where(c => Allowed(c) && Has(c)).ToList();
            }
            if (usable.Count == 0)
            {
                usable = order.Where(Has).ToList();
            }
            if (usable.Count == 0)
            {
                return want;
            }
            if (alongDrag)
            {
                int axis = AxisOf(want);
                foreach (Compass c in usable)
                {
                    int ax = AxisOf(Variant(c));
                    if (ax == axis || ax == 0)
                    {
                        return c;
                    }
                }
            }
            return usable[0];
        }

        /// <summary>The next allowed direction after <paramref name="c"/> (R in the tool).</summary>
        public Compass NextAllowed(Compass c)
        {
            for (int i = 1; i <= 4; i++)
            {
                var n = (Compass)(((int)c + i) % 4);
                if (Allowed(n))
                {
                    return n;
                }
            }
            return c;
        }

        /// <summary>The changes for the CGI map (optionally removing what stood there).</summary>
        public List<StaticChange> Build(MapData map, Point a, Point b, MotifDrawSettings o, int zOffset, out Compass dir, out int repeats)
        {
            var changes = new List<StaticChange>();
            List<(int x, int y, int z, ushort id, ushort hue)> parts = Layout(a, b, o, (x, y) => map.LandZ(x, y) + zOffset, out dir, out repeats);
            var removed = new HashSet<StaticItem>();
            var placed = new HashSet<(int, int, int, ushort)>();
            foreach ((int x, int y, int z, ushort id, ushort hue) in parts)
            {
                if (!map.InBounds(x, y) || !placed.Add((x, y, z, id)))
                {
                    continue;
                }
                if (o.Replace)
                {
                    foreach (StaticItem s in map.StaticsAt(x, y).ToList())
                    {
                        if (removed.Add(s))
                        {
                            changes.Add(MapData.Remove(s));
                        }
                    }
                }
                changes.Add(MapData.Add(new StaticItem { Id = id, Hue = hue, X = x, Y = y, Z = z }));
            }
            return changes;
        }

        public string Describe()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Muster '{this}'");
            foreach (Compass c in new[] { Compass.North, Compass.East, Compass.South, Compass.West })
            {
                List<PieceStatic> v = Variant(c);
                sb.AppendLine($"== {DirName(c)}{(Allowed(c) ? string.Empty : " (nicht erlaubt)")}: {v.Count} Teile, läuft {(AxisOf(v) > 0 ? "West-Ost" : AxisOf(v) < 0 ? "Nord-Süd" : "quadratisch")}, Höhe {HeightOf(c)}, Treppen-Anstieg {StairRise(c)}");
                foreach (PieceStatic p in v.OrderBy(p => p.Y).ThenBy(p => p.X).ThenBy(p => p.Z))
                {
                    sb.AppendLine($"  ({p.X},{p.Y}) z {p.Z,3}  {p.Id:x4} {UoArtCache.ItemName(p.Id),-20} h{UoArtCache.ItemHeight(p.Id)} (stapelt {UoArtCache.StackHeight(p.Id)})" + (p.Hue != 0 ? $" hue {p.Hue}" : string.Empty));
                }
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------ files

        public static string Folder => Path.Combine(StudioSettings.SettingsFolder, "Muster");

        private static string Safe(string s) => string.Concat((s ?? "x").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();

        public string Save()
        {
            string dir = Path.Combine(Folder, Safe(string.IsNullOrWhiteSpace(Category) ? "Allgemein" : Category));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, Safe(Name) + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            return path;
        }

        public static List<Motif> LoadAll()
        {
            var list = new List<Motif>();
            if (!Directory.Exists(Folder))
            {
                return list;
            }
            foreach (string f in Directory.GetFiles(Folder, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    Motif m = JsonSerializer.Deserialize<Motif>(File.ReadAllText(f));
                    if (m != null)
                    {
                        m.Category = Path.GetFileName(Path.GetDirectoryName(f));
                        list.Add(m);
                    }
                }
                catch (Exception)
                {
                    // broken file – skip
                }
            }
            return list.OrderBy(m => m.Category).ThenBy(m => m.Name).ToList();
        }

        public void Delete()
        {
            string path = Path.Combine(Folder, Safe(Category), Safe(Name) + ".json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
