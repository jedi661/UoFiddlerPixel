/***************************************************************************
 *
 *  * Dragon Map Studio – motifs ("Muster-Baukasten"): pieces built by hand (battlements, tents, a wall with windows,
 *  *  a fortress wall with towers …) with a version for every direction (north, east, south, west).
 *  *
 *  *  Drawn by clicking (once), by dragging (the row continues in the drag direction), point to point (several
 *  *  stretches with corner pieces) or scattered in a radius (random variants, e.g. trees: trunk + crown).
 *  *  A motif can have a start, a middle (repeated) and an end piece, corner pieces and random variants.
 *  *  Height: every piece on the ground, all on the height at the start, or rising; several floors in one go;
 *  *  the land below can be levelled so nothing sinks into the ground.
 *  *
 *  *  Files: own motifs in SettingsFolder\Muster\<Kategorie>\<Name>.json, shipped ones (project) in
 *  *  DragonData\Muster\<Kategorie>\<Name>.json – an own one with the same name wins.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public enum MotifMode
    {
        [Description("Linie: Klick = einmal, ziehen = Reihe")]
        Line,

        [Description("Punkt zu Punkt: klicken, klicken … Doppelklick / Enter = fertig (mit Ecken)")]
        Points,

        [Description("Im Radius streuen (Zufalls-Varianten, z. B. Bäume)")]
        Radius
    }

    public enum MotifFlatten
    {
        [Description("Aus")]
        Off,

        [Description("Auf die Höhe am Start")]
        StartHeight,

        [Description("Auf eine feste Höhe (Z)")]
        FixedZ
    }

    /// <summary>The kinds of pieces of a motif.</summary>
    public enum MotifPart
    {
        Middle,
        Start,
        End,
        Corner
    }

    public sealed class MotifDrawSettings
    {
        [Category("Zeichnen"), DisplayName("Zeichen-Art"), Description("Linie (Klick / ziehen), Punkt zu Punkt mit Ecken (Doppelklick oder Enter = fertig, Esc = abbrechen, Rücktaste = letzten Punkt weg), oder im Radius streuen.")]
        public MotifMode Mode { get; set; } = MotifMode.Line;

        [Category("Zeichnen"), DisplayName("Wiederholen"), Description("Reihe fortsetzen: beim Ziehen wird die Version genommen, die in Zieh-Richtung läuft – eine Mauer wird länger. Kopien nebeneinander: das Muster so wie es ist mehrmals hintereinander stellen.")]
        public MotifRepeat Repeat { get; set; } = MotifRepeat.Along;

        [Category("Zeichnen"), DisplayName("Ziehen wiederholt"), Description("An = ziehen wiederholt das Muster in Zieh-Richtung. Aus = jeder Klick / Zug setzt es genau einmal.")]
        public bool Drag { get; set; } = true;

        [Category("Zeichnen"), DisplayName("Richtung beim Klick"), Description("Welche Richtung ein einfacher Klick setzt (beim Ziehen bestimmt die Zieh-Richtung die Richtung). Im Werkzeug dreht R.")]
        public Compass Direction { get; set; } = Compass.East;

        [Category("Zeichnen"), DisplayName("Anfang / Ende benutzen"), Description("Hat das Muster ein Anfangs- und Endstück (z. B. Türme), kommen sie an die Enden der Reihe, dazwischen wiederholt sich die Mitte.")]
        public bool UseStartEnd { get; set; } = true;

        [Category("Zeichnen"), DisplayName("Ecken benutzen"), Description("Punkt zu Punkt: an jedem Knick das Eckstück des Musters (falls gebaut).")]
        public bool UseCorners { get; set; } = true;

        [Category("Zeichnen"), DisplayName("Letztes Stück anpassen"), Description("An = die Reihe endet genau am Ende des Zugs (das letzte Mittelstück wird gekürzt). Aus = nur ganze Stücke (die Reihe kann etwas länger werden).")]
        public bool FitLast { get; set; }

        [Category("Zeichnen"), DisplayName("Mitte zufällig aus Varianten"), Description("Statt immer derselben Mitte zufällig eine der Zufalls-Varianten des Musters (nach Gewicht).")]
        public bool RandomMiddle { get; set; }

        [Category("Zeichnen"), DisplayName("Abstand (Tiles)"), Description("Lücke zwischen den Wiederholungen (Zelte, Pfosten …).")]
        public int Gap { get; set; }

        [Category("Zeichnen"), DisplayName("Alte Statics ersetzen"), Description("Statics auf den Feldern des Musters vorher entfernen.")]
        public bool Replace { get; set; }

        [Category("Radius"), DisplayName("Mindest-Abstand (Tiles)"), Description("Im Radius streuen: so weit stehen zwei Stücke mindestens auseinander.")]
        public int Spacing { get; set; } = 3;

        [Category("Radius"), DisplayName("Dichte (%)"), Description("Im Radius streuen: wie viele der möglichen Plätze besetzt werden.")]
        public int Fill { get; set; } = 70;

        [Category("Radius"), DisplayName("Zufällige Richtung"), Description("Im Radius streuen: jedes Stück in einer zufälligen erlaubten Richtung.")]
        public bool RandomDirection { get; set; } = true;

        /// <summary>Random seed of the radius / random middle (new after every placing, so the preview stays still).</summary>
        [Browsable(false)]
        public int Seed { get; set; } = 1;

        [Category("Höhe"), DisplayName("Höhe"), Description("Gerade auf der Höhe am Start, jedes Stück auf dem Boden, oder steigend wie eine Treppe.")]
        public MotifHeight Height { get; set; } = MotifHeight.Level;

        [Category("Höhe"), DisplayName("Anstieg je Stück (Z)"), Description("Nur 'steigend': so viel höher ist jedes weitere Stück. 0 = automatisch (Treppen im Muster: ihre Höhe, sonst 5).")]
        public int Rise { get; set; }

        [Category("Höhe"), DisplayName("Z +"), Description("Zur Höhe addieren.")]
        public int ZOffset { get; set; }

        [Category("Boden"), DisplayName("Boden begradigen"), Description("Das Land unter dem Muster (und ein Rand) wird auf eine Höhe gesetzt – dann stecken die Grafiken nicht halb im Boden. Nur CGI-Ansicht.")]
        public MotifFlatten Flatten { get; set; } = MotifFlatten.Off;

        [Category("Boden"), DisplayName("Boden-Höhe (Z)"), Description("Nur 'auf eine feste Höhe': auf dieses Z wird der Boden gesetzt (das Muster steht darauf).")]
        public int FlattenZ { get; set; }

        [Category("Boden"), DisplayName("Boden-Rand (Tiles)"), Description("So viele Tiles um das Muster herum werden mit begradigt (1 = die Kanten der Tiles unter dem Muster liegen flach).")]
        public int FlattenMargin { get; set; } = 1;

        [Category("Etagen"), DisplayName("Etagen"), Description("So oft übereinander in einem Zug (z. B. 2–3 Etagen einer Mauer mit Fenstern).")]
        public int Floors { get; set; } = 1;

        [Category("Etagen"), DisplayName("Etagen-Höhe (Z)"), Description("Abstand der Etagen. 0 = Höhe des Musters.")]
        public int FloorHeight { get; set; }
    }

    /// <summary>A random variant of a motif (e.g. another tree, a wall piece without window).</summary>
    public sealed class MotifAlt
    {
        public string Name { get; set; } = "Variante";

        /// <summary>How often it is chosen compared with the others (the middle of the motif has <see cref="Motif.MainWeight"/>).</summary>
        public int Weight { get; set; } = 1;

        /// <summary>The direction it was built in (it is turned for the others).</summary>
        public Compass Built { get; set; } = Compass.East;

        public List<PieceStatic> Parts { get; set; } = new List<PieceStatic>();

        public override string ToString() => $"{Name} (×{Weight}, {Parts.Count} Teile)";
    }

    public sealed class Motif
    {
        private static readonly Compass[] Dirs = { Compass.North, Compass.East, Compass.South, Compass.West };

        [Category("Muster"), DisplayName("Name")]
        public string Name { get; set; } = "Mein Muster";

        [Category("Muster"), DisplayName("Kategorie"), Description("Ordner, z. B. Burg, Zelte, Mauern, Häuser.")]
        public string Category { get; set; } = "Allgemein";

        [Category("Muster"), DisplayName("Notiz")]
        public string Note { get; set; } = string.Empty;

        [Category("Muster"), DisplayName("Gewicht der Mitte"), Description("Zufalls-Varianten / Radius: wie oft die Mitte selbst gewählt wird (0 = nur die Varianten).")]
        public int MainWeight { get; set; } = 1;

        /// <summary>Shipped with the project (DragonData\Muster) – saving makes an own copy.</summary>
        [Browsable(false), JsonIgnore]
        public bool Shipped { get; set; }

        /// <summary>Directions this motif may be drawn in (e.g. only north – south). Not allowed ones are never turned or drawn.</summary>
        [Browsable(false)]
        public bool AllowNorth { get; set; } = true;

        [Browsable(false)]
        public bool AllowEast { get; set; } = true;

        [Browsable(false)]
        public bool AllowSouth { get; set; } = true;

        [Browsable(false)]
        public bool AllowWest { get; set; } = true;

        [Browsable(false)]
        public List<PieceStatic> North { get; set; } = new List<PieceStatic>();

        [Browsable(false)]
        public List<PieceStatic> East { get; set; } = new List<PieceStatic>();

        [Browsable(false)]
        public List<PieceStatic> South { get; set; } = new List<PieceStatic>();

        [Browsable(false)]
        public List<PieceStatic> West { get; set; } = new List<PieceStatic>();

        /// <summary>Start, end and corner pieces: key "Start.East", "End.North", "Corner.North" (= corner north-east) …</summary>
        [Browsable(false)]
        public Dictionary<string, List<PieceStatic>> Extra { get; set; } = new Dictionary<string, List<PieceStatic>>();

        /// <summary>Random variants (middle at random, scattering in a radius).</summary>
        [Browsable(false)]
        public List<MotifAlt> Alternatives { get; set; } = new List<MotifAlt>();

        public override string ToString() => Category + " / " + Name;

        // ------------------------------------------------------------------ directions

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
        public string AllowedText() => string.Join(", ", Dirs.Where(Allowed).Select(DirName));

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

        public static string PartName(MotifPart p) => p switch
        {
            MotifPart.Start => "Anfang",
            MotifPart.End => "Ende",
            MotifPart.Corner => "Ecke",
            _ => "Mitte"
        };

        /// <summary>Corner pieces are stored under a direction: north = corner north-east, east = south-east, south = south-west, west = north-west.</summary>
        public static string CornerName(Compass c) => c switch
        {
            Compass.North => "Ecke NO",
            Compass.East => "Ecke SO",
            Compass.South => "Ecke SW",
            _ => "Ecke NW"
        };

        /// <summary>Name of a piece slot, e.g. "Anfang Ost", "Ecke NO".</summary>
        public static string SlotName(MotifPart p, Compass c) => p == MotifPart.Corner ? CornerName(c) : PartName(p) + " " + DirName(c);

        /// <summary>
        /// The corner at a bend: coming from <paramref name="incoming"/> (direction of the stretch before) and going on in
        /// <paramref name="outgoing"/>. Null for straight on / back.
        /// </summary>
        public static Compass? CornerOf(Compass incoming, Compass outgoing)
        {
            Compass a = incoming, b = Staircase.Opposite(outgoing);
            if (a == b || a == Staircase.Opposite(b))
            {
                return null;
            }
            bool Is(Compass x, Compass y) => (a == x && b == y) || (a == y && b == x);
            if (Is(Compass.North, Compass.East))
            {
                return Compass.North;
            }
            if (Is(Compass.East, Compass.South))
            {
                return Compass.East;
            }
            if (Is(Compass.South, Compass.West))
            {
                return Compass.South;
            }
            return Compass.West;
        }

        private static string Key(MotifPart p, Compass c) => p + "." + c;

        public List<PieceStatic> Piece(MotifPart p, Compass c)
        {
            if (p == MotifPart.Middle)
            {
                return Variant(c);
            }
            return Extra != null && Extra.TryGetValue(Key(p, c), out List<PieceStatic> l) && l != null ? l : new List<PieceStatic>();
        }

        public void SetPiece(MotifPart p, Compass c, List<PieceStatic> parts)
        {
            if (p == MotifPart.Middle)
            {
                SetVariant(c, parts ?? new List<PieceStatic>());
                return;
            }
            Extra ??= new Dictionary<string, List<PieceStatic>>();
            if (parts == null || parts.Count == 0)
            {
                Extra.Remove(Key(p, c));
            }
            else
            {
                Extra[Key(p, c)] = parts;
            }
        }

        public bool HasPiece(MotifPart p, Compass c) => Piece(p, c).Count > 0;

        public bool HasPart(MotifPart p) => Dirs.Any(c => HasPiece(p, c));

        /// <summary>The piece for a direction – built, or turned from a built one (start / end / corner).</summary>
        public List<PieceStatic> PieceOrTurned(MotifPart p, Compass c)
        {
            if (HasPiece(p, c))
            {
                return Piece(p, c);
            }
            foreach (Compass src in new[] { Staircase.Opposite(c), Staircase.RightOf(c), Staircase.LeftOf(c) })
            {
                if (HasPiece(p, src))
                {
                    return Turn(Piece(p, src), src, c);
                }
            }
            return new List<PieceStatic>();
        }

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

        // ------------------------------------------------------------------ capture / turn

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

        /// <summary>A version turned from another: positions turn, pieces by the direction table / their slope / their art.</summary>
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

        /// <summary>The missing (allowed) directions turned from a built one – middle, start, end and the corners.</summary>
        public int AutoVariants(Compass? from = null, bool overwrite = false)
        {
            int made = 0;
            foreach (MotifPart part in new[] { MotifPart.Middle, MotifPart.Start, MotifPart.End, MotifPart.Corner })
            {
                Compass[] order = { Compass.East, Compass.South, Compass.North, Compass.West };
                Compass src = from != null && HasPiece(part, from.Value)
                    ? from.Value
                    : order.Where(c => part == MotifPart.Corner || Allowed(c)).Concat(order).FirstOrDefault(c => HasPiece(part, c));
                if (!HasPiece(part, src))
                {
                    continue;
                }
                foreach (Compass c in Dirs)
                {
                    if (part != MotifPart.Corner && !Allowed(c))
                    {
                        continue;
                    }
                    if (c != src && (overwrite || !HasPiece(part, c)))
                    {
                        SetPiece(part, c, Turn(Piece(part, src), src, c));
                        if (part == MotifPart.Middle)
                        {
                            made++;
                        }
                    }
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
        /// The pieces for a click (a == b: once in the click direction), a drag (repeated towards b) or – in the mode
        /// "Radius" – the scattering around a with the radius |ab|.
        /// <paramref name="ground"/> gives the land height (CGI) – null = heights stay relative (map editor).
        /// Returns (x, y, z, id, hue) and the used direction.
        /// </summary>
        public List<(int x, int y, int z, ushort id, ushort hue)> Layout(Point a, Point b, MotifDrawSettings o, Func<int, int, int> ground, out Compass dir, out int repeats)
        {
            if (o.Mode == MotifMode.Radius)
            {
                dir = o.Direction;
                return Scatter(a, b, o, ground, out repeats);
            }
            var rnd = new Random(o.Seed);
            int k = 0;
            int startZ = (ground?.Invoke(a.X, a.Y) ?? 0) + o.ZOffset;
            var result = new List<(int, int, int, ushort, ushort)>();
            Segment(result, a, b, o, o.Drag && a != b, ground, startZ, true, true, false, false, ref k, rnd, out dir, out repeats);
            return result;
        }

        /// <summary>
        /// Point to point: stretches between the points (each snapped to west – east or north – south), the corner piece
        /// at every bend. Returns the pieces and the number of placed pieces.
        /// </summary>
        public List<(int x, int y, int z, ushort id, ushort hue)> Path(IList<Point> points, MotifDrawSettings o, Func<int, int, int> ground, out int pieces)
        {
            var result = new List<(int, int, int, ushort, ushort)>();
            pieces = 0;
            if (points == null || points.Count == 0)
            {
                return result;
            }
            // snapped corners
            var pts = new List<Point> { points[0] };
            for (int i = 1; i < points.Count; i++)
            {
                Point p = pts[pts.Count - 1], q = points[i];
                Point s = Math.Abs(q.X - p.X) >= Math.Abs(q.Y - p.Y) ? new Point(q.X, p.Y) : new Point(p.X, q.Y);
                if (s != p)
                {
                    pts.Add(s);
                }
            }
            var rnd = new Random(o.Seed);
            int k = 0;
            int startZ = (ground?.Invoke(pts[0].X, pts[0].Y) ?? 0) + o.ZOffset;
            if (pts.Count == 1)
            {
                Segment(result, pts[0], pts[0], o, false, ground, startZ, true, true, false, false, ref k, rnd, out _, out pieces);
                return result;
            }
            var dirs = new List<Compass>();
            for (int i = 0; i + 1 < pts.Count; i++)
            {
                dirs.Add(Staircase.DirectionOf(pts[i], pts[i + 1]));
            }
            var corners = new bool[pts.Count];
            for (int i = 1; i + 1 < pts.Count; i++)
            {
                Compass? c = CornerOf(dirs[i - 1], dirs[i]);
                if (c == null || !o.UseCorners)
                {
                    continue;
                }
                List<PieceStatic> cp = PieceOrTurned(MotifPart.Corner, c.Value);
                if (cp.Count == 0)
                {
                    continue;
                }
                corners[i] = true;
                AddPieces(result, cp, pts[i].X, pts[i].Y, o, ground, startZ, ref k, ref pieces);
            }
            for (int i = 0; i + 1 < pts.Count; i++)
            {
                Segment(result, pts[i], pts[i + 1], o, true, ground, startZ, i == 0, i + 2 == pts.Count, corners[i], corners[i + 1], ref k, rnd, out _, out int n);
                pieces += n;
            }
            return result;
        }

        /// <summary>The snapped points of a path (what <see cref="Path"/> really draws) – for previews.</summary>
        public static List<Point> SnapPath(IList<Point> points)
        {
            var pts = new List<Point>();
            if (points == null || points.Count == 0)
            {
                return pts;
            }
            pts.Add(points[0]);
            for (int i = 1; i < points.Count; i++)
            {
                Point p = pts[pts.Count - 1], q = points[i];
                Point s = Math.Abs(q.X - p.X) >= Math.Abs(q.Y - p.Y) ? new Point(q.X, p.Y) : new Point(p.X, q.Y);
                if (s != p)
                {
                    pts.Add(s);
                }
            }
            return pts;
        }

        private void AddPieces(List<(int, int, int, ushort, ushort)> result, List<PieceStatic> v, int ox, int oy, MotifDrawSettings o, Func<int, int, int> ground,
            int startZ, ref int k, ref int count, Func<PieceStatic, bool> keep = null, int floorHeight = 0)
        {
            int rise = o.Height != MotifHeight.Rise ? 0 : o.Rise > 0 ? o.Rise : Math.Max(StairRiseOf(v), 5);
            int floorH = floorHeight > 0 ? floorHeight : o.FloorHeight > 0 ? o.FloorHeight : Math.Max(1, HeightOfParts(v));
            int baseZ = o.Height == MotifHeight.Ground && ground != null ? ground(ox, oy) + o.ZOffset : startZ + rise * k;
            for (int f = 0; f < Math.Clamp(o.Floors, 1, 20); f++)
            {
                foreach (PieceStatic p in v)
                {
                    if (keep == null || keep(p))
                    {
                        result.Add((ox + p.X, oy + p.Y, Math.Clamp(baseZ + p.Z + f * floorH, -128, 127), p.Id, p.Hue));
                    }
                }
            }
            k++;
            count++;
        }

        private static int HeightOfParts(List<PieceStatic> v) => v.Count == 0 ? 0 : v.Max(Top) - v.Min(p => p.Z);

        private static int StairRiseOf(List<PieceStatic> v)
        {
            var steps = v.Where(p => StairModel.UpOf(p.Id) != null && UoArtCache.ItemHeight(p.Id) <= 6).ToList();
            return steps.Count == 0 ? 0 : steps.Max(p => p.Z) - steps.Min(p => p.Z) + 5;
        }

        /// <summary>A random middle (the middle itself or a variant, by weight), turned to <paramref name="dir"/>.</summary>
        private List<PieceStatic> RandomPiece(Compass dir, Random rnd)
        {
            var pool = new List<(List<PieceStatic> parts, int weight)>();
            List<PieceStatic> mid = Variant(dir);
            if (mid.Count > 0 && MainWeight > 0)
            {
                pool.Add((mid, MainWeight));
            }
            foreach (MotifAlt alt in Alternatives ?? new List<MotifAlt>())
            {
                if (alt.Parts != null && alt.Parts.Count > 0 && alt.Weight > 0)
                {
                    pool.Add((alt.Built == dir ? alt.Parts : Turn(alt.Parts, alt.Built, dir), alt.Weight));
                }
            }
            if (pool.Count == 0)
            {
                return mid;
            }
            int total = pool.Sum(p => p.weight);
            int r = rnd.Next(total);
            foreach ((List<PieceStatic> parts, int weight) in pool)
            {
                if (r < weight)
                {
                    return parts;
                }
                r -= weight;
            }
            return pool[0].parts;
        }

        /// <summary>One stretch from a to b: start, the middles (repeated), end – or once (click).</summary>
        private void Segment(List<(int, int, int, ushort, ushort)> result, Point a, Point b, MotifDrawSettings o, bool drag, Func<int, int, int> ground, int startZ,
            bool first, bool last, bool cornerBefore, bool cornerAfter, ref int k, Random rnd, out Compass dir, out int repeats)
        {
            Compass want = drag ? Staircase.DirectionOf(a, b) : o.Direction;
            dir = ChooseVariant(want, drag && o.Repeat == MotifRepeat.Along);
            List<PieceStatic> mid = Variant(dir);
            repeats = 0;
            bool hasAlt = o.RandomMiddle && (Alternatives?.Any(x => x.Parts?.Count > 0) ?? false);
            if (mid.Count == 0 && !hasAlt)
            {
                return;
            }
            if (!drag)
            {
                List<PieceStatic> one = hasAlt ? RandomPiece(dir, rnd) : mid;
                AddPieces(result, one, a.X, a.Y, o, ground, startZ, ref k, ref repeats);
                return;
            }

            // where the copies go: along the drag; "Reihe fortsetzen" with a version that runs the other way
            // (only north – south allowed, dragged east …): along the axis of the motif, as far as the drag reaches there
            Compass step = want;
            if (o.Repeat == MotifRepeat.Along)
            {
                int axis = AxisOf(mid);
                if (axis == 0)
                {
                    axis = AxisOf(dir);
                }
                if (axis != AxisOf(want))
                {
                    step = axis > 0 ? (b.X >= a.X ? Compass.East : Compass.West) : (b.Y >= a.Y ? Compass.South : Compass.North);
                }
            }
            bool alongX = step == Compass.East || step == Compass.West;
            bool forward = step == Compass.East || step == Compass.South;
            int Len(List<PieceStatic> v) => v.Count == 0 ? 0 : alongX ? v.Max(p => p.X) + 1 : v.Max(p => p.Y) + 1;
            int total = (alongX ? Math.Abs(b.X - a.X) : Math.Abs(b.Y - a.Y)) + 1;
            int t = cornerBefore ? 1 : 0;
            int stop = total - (cornerAfter ? 1 : 0);
            int gap = Math.Max(0, o.Gap);
            int floorH = o.FloorHeight > 0 ? o.FloorHeight : Math.Max(1, HeightOf(dir));
            List<PieceStatic> start = o.UseStartEnd && first && !cornerBefore ? PieceOrTurned(MotifPart.Start, dir) : new List<PieceStatic>();
            List<PieceStatic> end = o.UseStartEnd && last && !cornerAfter ? PieceOrTurned(MotifPart.End, dir) : new List<PieceStatic>();
            int count = 0;

            void Place(List<PieceStatic> v, int at, ref int kk, Func<PieceStatic, bool> keep = null)
            {
                int len = Len(v);
                int ox = a.X, oy = a.Y;
                if (alongX)
                {
                    ox = forward ? a.X + at : a.X - at - len + 1;
                }
                else
                {
                    oy = forward ? a.Y + at : a.Y - at - len + 1;
                }
                AddPieces(result, v, ox, oy, o, ground, startZ, ref kk, ref count, keep, floorH);
            }

            if (start.Count > 0)
            {
                Place(start, t, ref k);
                t += Len(start);
            }
            int endLen = Len(end);
            // whole pieces may run past the end only at the free end of the row – before a bend / corner the stretch
            // always ends exactly there
            bool fit = o.FitLast || !last || cornerAfter;
            if (fit)
            {
                while (true)
                {
                    List<PieceStatic> m = hasAlt ? RandomPiece(dir, rnd) : mid;
                    int len = Len(m);
                    if (len <= 0)
                    {
                        break;
                    }
                    int room = stop - endLen - t;
                    if (room <= 0)
                    {
                        break;
                    }
                    if (len <= room)
                    {
                        Place(m, t, ref k);
                        t += len + gap;
                        continue;
                    }
                    // the last middle, cut to the room that is left (the part nearest to the start of the stretch)
                    int r = room;
                    Func<PieceStatic, bool> keep = forward
                        ? p => (alongX ? p.X : p.Y) < r
                        : p => (alongX ? p.X : p.Y) >= len - r;
                    Place(m, t, ref k, keep);
                    t += r;
                    break;
                }
            }
            else
            {
                do
                {
                    List<PieceStatic> m = hasAlt ? RandomPiece(dir, rnd) : mid;
                    int len = Len(m);
                    if (len <= 0)
                    {
                        break;
                    }
                    Place(m, t, ref k);
                    t += len + gap;
                }
                while (t < stop - endLen);
                if (gap > 0 && end.Count > 0)
                {
                    t -= gap;
                }
            }
            if (end.Count > 0)
            {
                Place(end, fit ? Math.Max(t, stop - endLen) : t, ref k);
            }
            repeats = count;
        }

        /// <summary>Scattering in a radius: random places (at least <see cref="MotifDrawSettings.Spacing"/> apart), random variants.</summary>
        private List<(int x, int y, int z, ushort id, ushort hue)> Scatter(Point a, Point b, MotifDrawSettings o, Func<int, int, int> ground, out int placed)
        {
            var result = new List<(int, int, int, ushort, ushort)>();
            placed = 0;
            var rnd = new Random(o.Seed);
            double radius = Math.Max(0.5, Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y)));
            int spacing = Math.Max(1, o.Spacing);
            var spots = new List<Point>();
            if (radius < 1)
            {
                spots.Add(a);
            }
            else
            {
                int tries = (int)Math.Min(20000, Math.PI * radius * radius * 3 + 20);
                for (int i = 0; i < tries; i++)
                {
                    double ang = rnd.NextDouble() * Math.PI * 2, d = radius * Math.Sqrt(rnd.NextDouble());
                    var p = new Point(a.X + (int)Math.Round(Math.Cos(ang) * d), a.Y + (int)Math.Round(Math.Sin(ang) * d));
                    if (spots.All(s => (s.X - p.X) * (s.X - p.X) + (s.Y - p.Y) * (s.Y - p.Y) >= spacing * spacing))
                    {
                        spots.Add(p);
                    }
                }
            }
            Compass[] allowed = Dirs.Where(Allowed).ToArray();
            if (allowed.Length == 0)
            {
                allowed = new[] { o.Direction };
            }
            int startZ = (ground?.Invoke(a.X, a.Y) ?? 0) + o.ZOffset;
            int k = 0;
            foreach (Point p in spots)
            {
                if (rnd.Next(100) >= Math.Clamp(o.Fill, 1, 100))
                {
                    continue;
                }
                Compass d = o.RandomDirection ? allowed[rnd.Next(allowed.Length)] : (Allowed(o.Direction) ? o.Direction : allowed[0]);
                if (!Has(d) && Dirs.Any(Has))
                {
                    AutoVariants();
                }
                List<PieceStatic> v = RandomPiece(d, rnd);
                if (v.Count == 0)
                {
                    continue;
                }
                int w = v.Max(q => q.X) + 1, h = v.Max(q => q.Y) + 1;
                int ox = p.X - w / 2, oy = p.Y - h / 2;
                int baseZ = o.Height == MotifHeight.Level || ground == null ? startZ : ground(p.X, p.Y) + o.ZOffset;
                foreach (PieceStatic q in v)
                {
                    result.Add((ox + q.X, oy + q.Y, Math.Clamp(baseZ + q.Z, -128, 127), q.Id, q.Hue));
                }
                k++;
                placed++;
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
                return Allowed(want) ? want : NextAllowed(want);
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

        // ------------------------------------------------------------------ building into the CGI map

        /// <summary>The changes for the CGI map (optionally removing what stood there).</summary>
        public List<StaticChange> Build(MapData map, Point a, Point b, MotifDrawSettings o, int zOffset, out Compass dir, out int repeats) =>
            Build(map, a, b, o, zOffset, out dir, out repeats, out _);

        /// <summary>The changes for the CGI map – with the levelled land (<see cref="MotifDrawSettings.Flatten"/>).</summary>
        public List<StaticChange> Build(MapData map, Point a, Point b, MotifDrawSettings o, int zOffset, out Compass dir, out int repeats, out List<LandChange> land) =>
            Build(map, a, b, o, zOffset, null, out dir, out repeats, out land);

        /// <summary><paramref name="fixedGround"/>: build on this height instead of the land (cellar level; no levelling).</summary>
        public List<StaticChange> Build(MapData map, Point a, Point b, MotifDrawSettings o, int zOffset, int? fixedGround, out Compass dir, out int repeats, out List<LandChange> land)
        {
            Compass d = Compass.East;
            int n = 0;
            List<StaticChange> ch = BuildWith(map, o, zOffset, g =>
            {
                List<(int x, int y, int z, ushort id, ushort hue)> r = Layout(a, b, o, g, out d, out n);
                return r;
            }, (map.LandZ(a.X, a.Y)), out land, fixedGround);
            dir = d;
            repeats = n;
            return ch;
        }

        /// <summary>Point to point into the CGI map.</summary>
        public List<StaticChange> BuildPath(MapData map, IList<Point> points, MotifDrawSettings o, int zOffset, out int pieces, out List<LandChange> land, int? fixedGround = null)
        {
            int n = 0;
            Point p0 = points.Count > 0 ? points[0] : Point.Empty;
            List<StaticChange> ch = BuildWith(map, o, zOffset, g => Path(points, o, g, out n), map.LandZ(p0.X, p0.Y), out land, fixedGround);
            pieces = n;
            return ch;
        }

        private List<StaticChange> BuildWith(MapData map, MotifDrawSettings o, int zOffset, Func<Func<int, int, int>, List<(int x, int y, int z, ushort id, ushort hue)>> layout,
            int startLand, out List<LandChange> land, int? fixedGround = null)
        {
            land = new List<LandChange>();
            Func<int, int, int> ground = fixedGround != null ? (x, y) => fixedGround.Value + zOffset : (x, y) => map.LandZ(x, y) + zOffset;
            List<(int x, int y, int z, ushort id, ushort hue)> parts = layout(ground);
            if (o.Flatten != MotifFlatten.Off && parts.Count > 0 && fixedGround == null)
            {
                int target = o.Flatten == MotifFlatten.FixedZ ? o.FlattenZ : startLand;
                land = FlattenLand(map, parts.Select(p => new Point(p.x, p.y)), target, o.FlattenMargin);
                // the motif stands on the levelled ground
                parts = layout((x, y) => target + zOffset);
            }
            var changes = new List<StaticChange>();
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

        /// <summary>
        /// Sets the land under the cells (plus <paramref name="margin"/> tiles around) to height <paramref name="z"/> –
        /// the tiles stay, only the height changes. For the motif and for "Boden begradigen" under a house.
        /// </summary>
        public static List<LandChange> FlattenLand(MapData map, IEnumerable<Point> cells, int z, int margin)
        {
            var set = new HashSet<int>();
            margin = Math.Clamp(margin, 0, 8);
            foreach (Point c in cells)
            {
                for (int dy = -margin; dy <= margin; dy++)
                {
                    for (int dx = -margin; dx <= margin; dx++)
                    {
                        int x = c.X + dx, y = c.Y + dy;
                        if (map.InBounds(x, y))
                        {
                            set.Add(y * map.Width + x);
                        }
                    }
                }
            }
            var nz = (sbyte)Math.Clamp(z, -128, 127);
            return set.Where(i => map.Z[i] != nz)
                      .Select(i => new LandChange { Index = i, OldId = map.Tiles[i], OldZ = map.Z[i], NewId = map.Tiles[i], NewZ = nz })
                      .ToList();
        }

        /// <summary>
        /// "Boden begradigen" afterwards: the land under <paramref name="cells"/> (+ margin) to height <paramref name="z"/>;
        /// with <paramref name="moveStatics"/> the statics standing there move by the same amount as the lowest one, so a
        /// house keeps its shape and stands on the new ground.
        /// </summary>
        public static (List<StaticChange> statics, List<LandChange> land) Level(MapData map, ICollection<Point> cells, int z, int margin, bool moveStatics, IEnumerable<StaticItem> only = null)
        {
            List<LandChange> land = FlattenLand(map, cells, z, margin);
            var statics = new List<StaticChange>();
            if (moveStatics)
            {
                List<StaticItem> items = (only ?? cells.SelectMany(c => map.StaticsAt(c.X, c.Y))).Distinct().ToList();
                if (items.Count > 0)
                {
                    int dz = z - items.Min(s => s.Z);
                    if (dz != 0)
                    {
                        statics.AddRange(items.Select(s => MapData.Modify(s, t => t.Z = (sbyte)Math.Clamp(t.Z + dz, -128, 127))));
                    }
                }
            }
            return (statics, land);
        }

        // ------------------------------------------------------------------ describe

        public string Describe()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Muster '{this}'" + (Shipped ? " (aus dem Projekt)" : string.Empty) + $", erlaubt: {AllowedText()}");
            foreach (MotifPart part in new[] { MotifPart.Middle, MotifPart.Start, MotifPart.End, MotifPart.Corner })
            {
                foreach (Compass c in Dirs)
                {
                    List<PieceStatic> v = Piece(part, c);
                    if (part != MotifPart.Middle && v.Count == 0)
                    {
                        continue;
                    }
                    string head = part == MotifPart.Middle ? DirName(c) + (Allowed(c) ? string.Empty : " (nicht erlaubt)") : SlotName(part, c);
                    sb.AppendLine($"== {head}: {v.Count} Teile, läuft {(AxisOf(v) > 0 ? "West-Ost" : AxisOf(v) < 0 ? "Nord-Süd" : "quadratisch")}, Höhe {HeightOfParts(v)}, Treppen-Anstieg {StairRiseOf(v)}");
                    foreach (PieceStatic p in v.OrderBy(p => p.Y).ThenBy(p => p.X).ThenBy(p => p.Z))
                    {
                        sb.AppendLine($"  ({p.X},{p.Y}) z {p.Z,3}  {p.Id:x4} {UoArtCache.ItemName(p.Id),-20} h{UoArtCache.ItemHeight(p.Id)} (stapelt {UoArtCache.StackHeight(p.Id)})" + (p.Hue != 0 ? $" hue {p.Hue}" : string.Empty));
                    }
                }
            }
            foreach (MotifAlt alt in Alternatives ?? new List<MotifAlt>())
            {
                sb.AppendLine($"== Zufalls-Variante '{alt.Name}' Gewicht {alt.Weight}, gebaut {DirName(alt.Built)}: {alt.Parts.Count} Teile");
                foreach (PieceStatic p in alt.Parts.OrderBy(p => p.Y).ThenBy(p => p.X).ThenBy(p => p.Z))
                {
                    sb.AppendLine($"  ({p.X},{p.Y}) z {p.Z,3}  {p.Id:x4} {UoArtCache.ItemName(p.Id)}");
                }
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------ files

        public static string Folder => System.IO.Path.Combine(StudioSettings.SettingsFolder, "Muster");

        /// <summary>Motifs shipped with the project: DragonData\Muster (null when the data folder is not found).</summary>
        public static string ShippedFolder => StudioPaths.DataFolder == null ? null : System.IO.Path.Combine(StudioPaths.DataFolder, "Muster");

        private static readonly JsonSerializerOptions JsonOpt = new JsonSerializerOptions { WriteIndented = true };

        private static string Safe(string s) => string.Concat((s ?? "x").Select(c => System.IO.Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();

        public string ToJson() => JsonSerializer.Serialize(this, JsonOpt);

        public string Save()
        {
            string dir = System.IO.Path.Combine(Folder, Safe(string.IsNullOrWhiteSpace(Category) ? "Allgemein" : Category));
            Directory.CreateDirectory(dir);
            string path = System.IO.Path.Combine(dir, Safe(Name) + ".json");
            File.WriteAllText(path, ToJson());
            Shipped = false;
            return path;
        }

        private static void ReadFolder(string folder, bool shipped, Dictionary<string, Motif> into)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return;
            }
            foreach (string f in Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    Motif m = JsonSerializer.Deserialize<Motif>(File.ReadAllText(f));
                    if (m != null)
                    {
                        string cat = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(f));
                        if (!string.Equals(System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(f)).TrimEnd('\\', '/'), System.IO.Path.GetFullPath(folder).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
                        {
                            m.Category = cat;
                        }
                        m.Shipped = shipped;
                        m.Extra ??= new Dictionary<string, List<PieceStatic>>();
                        m.Alternatives ??= new List<MotifAlt>();
                        into[m.ToString()] = m;
                    }
                }
                catch (Exception)
                {
                    // broken file – skip
                }
            }
        }

        /// <summary>Shipped motifs (project) and own ones – an own one with the same category / name wins.</summary>
        public static List<Motif> LoadAll()
        {
            var all = new Dictionary<string, Motif>();
            ReadFolder(ShippedFolder, true, all);
            ReadFolder(Folder, false, all);
            return all.Values.OrderBy(m => m.Category).ThenBy(m => m.Name).ToList();
        }

        /// <summary>Deletes the own file. False when the motif only exists in the project (shipped).</summary>
        public bool Delete()
        {
            string path = System.IO.Path.Combine(Folder, Safe(Category), Safe(Name) + ".json");
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }

        /// <summary>Writes the motifs as Kategorie\Name.json below <paramref name="folder"/> (e.g. DragonData\Muster of the project).</summary>
        public static int ExportToFolder(IEnumerable<Motif> motifs, string folder)
        {
            int n = 0;
            foreach (Motif m in motifs)
            {
                string dir = System.IO.Path.Combine(folder, Safe(string.IsNullOrWhiteSpace(m.Category) ? "Allgemein" : m.Category));
                Directory.CreateDirectory(dir);
                File.WriteAllText(System.IO.Path.Combine(dir, Safe(m.Name) + ".json"), m.ToJson());
                n++;
            }
            return n;
        }

        /// <summary>One motif as .json, several as .zip (Kategorie/Name.json).</summary>
        public static void Export(IList<Motif> motifs, string file)
        {
            if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && motifs.Count == 1)
            {
                File.WriteAllText(file, motifs[0].ToJson());
                return;
            }
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            using ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Create);
            foreach (Motif m in motifs)
            {
                ZipArchiveEntry e = zip.CreateEntry(Safe(string.IsNullOrWhiteSpace(m.Category) ? "Allgemein" : m.Category) + "/" + Safe(m.Name) + ".json");
                using var w = new StreamWriter(e.Open());
                w.Write(m.ToJson());
            }
        }

        /// <summary>Reads motifs from a .json or .zip and saves them as own motifs. Returns the imported ones.</summary>
        public static List<Motif> Import(string file)
        {
            var list = new List<Motif>();
            void Take(string json, string catFromPath)
            {
                Motif m = JsonSerializer.Deserialize<Motif>(json);
                if (m == null)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(catFromPath))
                {
                    m.Category = catFromPath;
                }
                m.Extra ??= new Dictionary<string, List<PieceStatic>>();
                m.Alternatives ??= new List<MotifAlt>();
                m.Save();
                list.Add(m);
            }
            if (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using ZipArchive zip = ZipFile.OpenRead(file);
                foreach (ZipArchiveEntry e in zip.Entries.Where(e => e.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
                {
                    using var r = new StreamReader(e.Open());
                    string[] parts = e.FullName.Replace('\\', '/').Split('/');
                    Take(r.ReadToEnd(), parts.Length >= 2 ? parts[parts.Length - 2] : null);
                }
            }
            else
            {
                Take(File.ReadAllText(file), null);
            }
            return list;
        }
    }
}
