/***************************************************************************
 *
 *  * Dragon Map Studio – more brushes and edits of the CGI view (pure logic, no user interface)
 *  *
 *  *  Statics: scatter brush (sets from the Dragon item files or own sets), wall / fence tool, hue brush,
 *  *           drop to ground, rotate / mirror, search & replace
 *  *  Land:    river, road along a curve, ramp, plateau / terraces, noise, tile variants, soft brush edge
 *  *  Select:  magic wand, lasso (polygon) – both give a cell mask
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    // ====================================================================== settings

    /// <summary>A set for the scatter brush: groups of statics (trees with leaves …) with their frequency.</summary>
    public sealed class ScatterSet
    {
        public string Name { get; set; } = "Eigenes Set";

        /// <summary>Like a line of a Dragon item file: frequency + statics (id, z, hue, dx, dy).</summary>
        public List<ItemGroupDef> Groups { get; set; } = new List<ItemGroupDef>();

        /// <summary>From the Dragon item files (not saved in the settings).</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool FromScripts { get; set; }

        public HashSet<ushort> Ids() => new HashSet<ushort>(Groups.SelectMany(g => g.Parts).Select(p => p.Id));

        public override string ToString() => Name + $"  ({Groups.Count})";

        public static ScatterSet FromItemFile(ItemFile f)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(f.Path ?? string.Empty);
            return new ScatterSet { Name = "Dragon: " + name, Groups = f.Groups.Where(g => g.Parts.Count > 0).ToList(), FromScripts = true };
        }
    }

    public sealed class ScatterSettings
    {
        [Category("Streuen"), DisplayName("Dichte (%)"), Description("Chance pro Feld und Pinsel-Tupfer, dass etwas gesetzt wird (1–100). Wald dicht: 15, lichter Wald: 5, Steppe: 2.")]
        public int Density { get; set; } = 6;

        [Category("Streuen"), DisplayName("Mindestabstand (Tiles)"), Description("Kein anderes Static desselben Sets näher als so viele Tiles (0 = egal).")]
        public int MinDistance { get; set; } = 2;

        [Category("Streuen"), DisplayName("Z +"), Description("Zur Bodenhöhe addieren.")]
        public int ZOffset { get; set; }

        [Category("Streuen"), DisplayName("Nur auf Land-Tiles (hex)"), Description("Leer = auf jedem Boden. Sonst nur auf diesen Land-Tiles, z. B. Gras '0003 0004 0005 0006'. Knopf 'Boden unter der Maus übernehmen' im Reiter Pinsel.")]
        public string OnlyOnTiles { get; set; } = string.Empty;

        [Category("Streuen"), DisplayName("Nicht auf Wasser"), Description("Wasser-Tiles und Felder mit Wasser-Statics auslassen.")]
        public bool NotOnWater { get; set; } = true;

        [Category("Streuen"), DisplayName("Nicht auf Statics"), Description("Felder, auf denen schon ein Static steht, auslassen.")]
        public bool NotOnStatics { get; set; } = true;

        [Category("Streuen"), DisplayName("Zufällige Hues (hex)"), Description("Leer = die Hue aus dem Set. Sonst wird eine davon zufällig gewählt (z. B. Herbstfarben).")]
        public string RandomHues { get; set; } = string.Empty;

        [Category("Streuen"), DisplayName("Höchste Steigung"), Description("Nicht auf Hänge setzen, die zum Nachbarn mehr als so viel Z abfallen (0 = egal).")]
        public int MaxSlope { get; set; } = 12;
    }

    /// <summary>Wall / fence: pieces along X (east – west), along Y (north – south) and a post for ends and corners.</summary>
    public sealed class WallSet
    {
        public string Name { get; set; } = "Mauer";

        [Description("Statics für eine Wand, die von Westen nach Osten läuft (hex, mehrere = zufällig).")]
        public string AlongX { get; set; } = string.Empty;

        [Description("Statics für eine Wand, die von Norden nach Süden läuft (hex, mehrere = zufällig).")]
        public string AlongY { get; set; } = string.Empty;

        [Description("Pfosten / Ecke (hex), leer = keiner.")]
        public string Post { get; set; } = string.Empty;

        public override string ToString() => Name;
    }

    public sealed class WallSettings
    {
        [Category("Wand"), DisplayName("Stockwerke"), Description("So viele Lagen übereinander (Mauer 1, Burgmauer 2–3).")]
        public int Levels { get; set; } = 1;

        [Category("Wand"), DisplayName("Höhe je Stockwerk"), Description("Z-Abstand der Lagen (UO-Wände: 20).")]
        public int LevelHeight { get; set; } = 20;

        [Category("Wand"), DisplayName("Z +"), Description("Zur Bodenhöhe addieren.")]
        public int ZOffset { get; set; }

        [Category("Wand"), DisplayName("Pfosten an Enden"), Description("Am Anfang und Ende jeder Strecke den Pfosten setzen (wenn das Set einen hat).")]
        public bool PostsAtEnds { get; set; } = true;

        [Category("Wand"), DisplayName("Alte Statics ersetzen"), Description("Statics auf der Strecke vorher entfernen (sonst wird nur dazugesetzt).")]
        public bool ReplaceExisting { get; set; }

        [Category("Wand"), DisplayName("Gleiche Höhe"), Description("Alle Teile auf die Bodenhöhe des Startpunkts (gerade Mauerkrone) statt der Höhe jedes Feldes.")]
        public bool Level { get; set; }
    }

    public sealed class RiverSettings
    {
        [Category("Fluss"), DisplayName("Tiefe"), Description("Wasserspiegel so viel unter dem Boden am Startpunkt des Strichs.")]
        public int Depth { get; set; } = 5;

        [Category("Fluss"), DisplayName("Wasser-Tiles (hex)"), Description("Land-Tiles des Wassers (Standard: Wasser 00a8–00ab).")]
        public string WaterTiles { get; set; } = "00a8 00a9 00aa 00ab";

        [Category("Fluss"), DisplayName("Ufer-Breite"), Description("So viele Tiles neben dem Wasser steigt das Ufer weich an.")]
        public int BankWidth { get; set; } = 2;

        [Category("Fluss"), DisplayName("Ufer-Steigung"), Description("Z pro Tile am Ufer.")]
        public int BankStep { get; set; } = 2;

        [Category("Fluss"), DisplayName("Spiegel folgt dem Gelände"), Description("An = der Wasserspiegel liegt überall 'Tiefe' unter dem Boden (Gebirgsbach). Aus = ein Spiegel für den ganzen Strich (ruhiger Fluss, See).")]
        public bool FollowTerrain { get; set; }

        [Category("Fluss"), DisplayName("Übergänge danach"), Description("Nach dem Strich die Dragon-Übergänge (Wasser → Sand / Gras) um den Fluss berechnen.")]
        public bool Transitions { get; set; } = true;
    }

    public sealed class RoadSettings
    {
        [Category("Weg"), DisplayName("Breite (Tiles)")]
        public int Width { get; set; } = 3;

        [Category("Weg"), DisplayName("Weg-Tiles (hex)"), Description("Leer = die Land-Tiles aus dem Reiter Land (oder die Vorlage dort, z. B. Pflaster).")]
        public string Tiles { get; set; } = string.Empty;

        [Category("Weg"), DisplayName("Höhe folgt dem Gelände"), Description("An = der Weg folgt dem Boden, geglättet. Aus = gleichmäßige Steigung vom ersten zum letzten Punkt.")]
        public bool FollowTerrain { get; set; } = true;

        [Category("Weg"), DisplayName("Glättung (Tiles)"), Description("Wie lang die Strecke ist, über die die Höhe gemittelt wird (keine Buckel).")]
        public int Smoothing { get; set; } = 6;

        [Category("Weg"), DisplayName("Bankett (Tiles)"), Description("So viele Tiles neben dem Weg werden an die Weghöhe angeglichen (weicher Rand).")]
        public int Shoulder { get; set; } = 1;

        [Category("Weg"), DisplayName("Übergänge danach")]
        public bool Transitions { get; set; } = true;
    }

    public sealed class RampSettings
    {
        [Category("Rampe"), DisplayName("Breite (Tiles)"), Description("0 = Pinselgröße oben.")]
        public int Width { get; set; }

        [Category("Rampe"), DisplayName("Tiles behalten"), Description("An = nur die Höhen ändern. Aus = die Land-Tiles aus dem Reiter Land setzen.")]
        public bool KeepTiles { get; set; } = true;

        [Category("Rampe"), DisplayName("Bankett (Tiles)"), Description("Ränder neben der Rampe weich angleichen.")]
        public int Shoulder { get; set; } = 1;
    }

    public enum PlateauMode
    {
        [Description("Abflachen auf eine Höhe")]
        Flatten,

        [Description("Terrassen (Stufen)")]
        Terraces
    }

    public sealed class PlateauSettings
    {
        [Category("Plateau"), DisplayName("Art"), Description("Abflachen = alles auf die Höhe unter dem ersten Klick (oder 'Höhe ='). Terrassen = Höhen auf Stufen runden.")]
        public PlateauMode Mode { get; set; } = PlateauMode.Flatten;

        [Category("Plateau"), DisplayName("Stufen-Höhe"), Description("Terrassen: Höhe jeder Stufe.")]
        public int TerraceStep { get; set; } = 5;

        [Category("Plateau"), DisplayName("Feste Höhe ('Höhe =')"), Description("Abflachen auf die Höhe 'Höhe =' aus dem Reiter Land statt auf die Höhe unter dem ersten Klick.")]
        public bool UseTargetHeight { get; set; }
    }

    public sealed class NoiseSettings
    {
        [Category("Unebenheit"), DisplayName("Stärke (± Z)")]
        public int Amplitude { get; set; } = 3;

        [Category("Unebenheit"), DisplayName("Größe der Wellen (Tiles)")]
        public int Scale { get; set; } = 5;

        [Category("Unebenheit"), DisplayName("Zufalls-Zahl")]
        public int Seed { get; set; } = 7;
    }

    public enum WandMode
    {
        [Description("Gleiches Land-Tile")]
        SameTile,

        [Description("Gleiche Tile-Familie (Farbe der Farbtabelle)")]
        SameFamily,

        [Description("Ähnliche Höhe")]
        SameHeight
    }

    public sealed class WandSettings
    {
        [Category("Zauberstab"), DisplayName("Art")]
        public WandMode Mode { get; set; } = WandMode.SameFamily;

        [Category("Zauberstab"), DisplayName("Höhen-Toleranz"), Description("Nachbarn höchstens so viel Z anders (alle Arten).")]
        public int ZTolerance { get; set; } = 5;

        [Category("Zauberstab"), DisplayName("Höchstens Felder")]
        public int MaxCells { get; set; } = 250000;
    }

    /// <summary>All settings of the new CGI brushes (saved with the studio settings).</summary>
    public sealed class CgiBrushSettings
    {
        [Category("Pinsel"), DisplayName("Härte (%)"), Description("Höhe +/−, Glätten, Plateau, Unebenheit: 100 = harter Rand, kleiner = weicher Rand (zur Mitte stärker).")]
        public int Hardness { get; set; } = 100;

        public ScatterSettings Scatter { get; set; } = new ScatterSettings();
        public string ScatterSet { get; set; } = "Dragon: forest";
        public List<ScatterSet> OwnSets { get; set; } = new List<ScatterSet>();

        public WallSettings Wall { get; set; } = new WallSettings();
        public string WallSet { get; set; } = string.Empty;
        public List<WallSet> WallSets { get; set; } = new List<WallSet>();

        public RiverSettings River { get; set; } = new RiverSettings();
        public RoadSettings Road { get; set; } = new RoadSettings();
        public RampSettings Ramp { get; set; } = new RampSettings();
        public PlateauSettings Plateau { get; set; } = new PlateauSettings();
        public NoiseSettings Noise { get; set; } = new NoiseSettings();
        public WandSettings Wand { get; set; } = new WandSettings();

        public BridgeSettings Bridge { get; set; } = new BridgeSettings();
        public string BridgeSet { get; set; } = string.Empty;
        public List<BridgeSet> BridgeSets { get; set; } = new List<BridgeSet>();

        public CellarSettings Cellar { get; set; } = new CellarSettings();
        public string StairSet { get; set; } = string.Empty;
        public List<StairSet> StairSets { get; set; } = new List<StairSet>();

        public DungeonSettings Dungeon { get; set; } = new DungeonSettings();
        public List<DungeonLook> DungeonLooks { get; set; } = new List<DungeonLook>();
        public CourseSettings Course { get; set; } = new CourseSettings();

        /// <summary>"Unter der Karte bauen": statics are placed this much below the ground (−20 = one floor).</summary>
        public int UnderOffset { get; set; } = -20;

        /// <summary>Kinds of statics hidden in the view.</summary>
        public StaticKind HiddenKinds { get; set; }

        /// <summary>Bookmarks "name|x|y".</summary>
        public List<string> Bookmarks { get; set; } = new List<string>();

        public void Normalize()
        {
            Scatter ??= new ScatterSettings();
            OwnSets ??= new List<ScatterSet>();
            Wall ??= new WallSettings();
            WallSets ??= new List<WallSet>();
            River ??= new RiverSettings();
            Road ??= new RoadSettings();
            Ramp ??= new RampSettings();
            Plateau ??= new PlateauSettings();
            Noise ??= new NoiseSettings();
            Wand ??= new WandSettings();
            Bookmarks ??= new List<string>();
            Bridge ??= new BridgeSettings();
            BridgeSets ??= new List<BridgeSet>();
            Cellar ??= new CellarSettings();
            StairSets ??= new List<StairSet>();
            Dungeon ??= new DungeonSettings();
            DungeonLooks ??= new List<DungeonLook>();
            Course ??= new CourseSettings();
            Hardness = Math.Clamp(Hardness, 1, 100);
        }
    }

    // ====================================================================== logic

    public static class CgiBrushes
    {
        /// <summary>Cells of a round brush (radius 0 = one tile).</summary>
        public static IEnumerable<(int x, int y, double d)> Circle(MapData map, int cx, int cy, int r)
        {
            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    int dd = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                    if (map.InBounds(x, y) && (r <= 1 || dd <= r * r + r))
                    {
                        yield return (x, y, Math.Sqrt(dd));
                    }
                }
            }
        }

        /// <summary>Strength of the brush at a distance: 1 in the middle, soft towards the edge when hardness &lt; 100.</summary>
        public static double Weight(double d, int r, int hardness)
        {
            if (hardness >= 100 || r <= 0)
            {
                return 1;
            }
            double inner = r * hardness / 100.0;
            if (d <= inner)
            {
                return 1;
            }
            double t = Math.Clamp((d - inner) / Math.Max(0.5, r + 0.5 - inner), 0, 1);
            return 1 - t * t * (3 - 2 * t);
        }

        /// <summary>Whole number of a fractional amount with random rounding (soft edges without banding).</summary>
        public static int Dither(double v, Random rnd)
        {
            double f = Math.Floor(v);
            return (int)f + (rnd.NextDouble() < v - f ? 1 : 0);
        }

        private static bool HasWater(MapData map, int x, int y)
        {
            if (UoArtCache.LandIsWet(map.LandId(x, y)))
            {
                return true;
            }
            foreach (StaticItem s in map.StaticsAt(x, y))
            {
                if (UoArtCache.ItemIsWet(s.Id))
                {
                    return true;
                }
            }
            return false;
        }

        // ------------------------------------------------------------------ statics

        /// <summary>
        /// Scatter brush: random groups of the set on the cells of the brush (density, distance, ground filter).
        /// With <paramref name="erase"/> the statics of the set inside the brush are removed.
        /// </summary>
        public static List<StaticChange> Scatter(MapData map, int cx, int cy, int r, ScatterSet set, ScatterSettings o, Random rnd, bool erase, HashSet<int> visited)
        {
            var changes = new List<StaticChange>();
            if (set == null || set.Groups.Count == 0)
            {
                return changes;
            }
            HashSet<ushort> ids = set.Ids();
            if (erase)
            {
                foreach ((int x, int y, double _) in Circle(map, cx, cy, r))
                {
                    changes.AddRange(map.StaticsAt(x, y).Where(s => ids.Contains(s.Id)).Select(MapData.Remove));
                }
                return changes;
            }
            var only = new HashSet<ushort>(HexList.Parse(o.OnlyOnTiles));
            ushort[] hues = HexList.Parse(o.RandomHues);
            int total = set.Groups.Sum(g => Math.Max(1, g.Frequency));
            var placedNow = new List<Point>();
            foreach ((int x, int y, double _) in Circle(map, cx, cy, r))
            {
                int key = y * map.Width + x;
                if (visited != null && !visited.Add(key))
                {
                    continue; // every cell is tried once per stroke
                }
                if (rnd.Next(100) >= Math.Clamp(o.Density, 1, 100))
                {
                    continue;
                }
                if (only.Count > 0 && !only.Contains(map.LandId(x, y)))
                {
                    continue;
                }
                if (o.NotOnWater && HasWater(map, x, y))
                {
                    continue;
                }
                if (o.NotOnStatics && map.StaticsAt(x, y).Any())
                {
                    continue;
                }
                int z = map.LandZ(x, y);
                if (o.MaxSlope > 0 && (Math.Abs(map.LandZ(x + 1, y) - z) > o.MaxSlope || Math.Abs(map.LandZ(x, y + 1) - z) > o.MaxSlope ||
                                       Math.Abs(map.LandZ(x + 1, y + 1) - z) > o.MaxSlope))
                {
                    continue;
                }
                if (o.MinDistance > 0 && TooClose(map, x, y, o.MinDistance, ids, placedNow))
                {
                    continue;
                }
                // weighted choice like Dragon (frequency)
                int pick = rnd.Next(total);
                ItemGroupDef g = set.Groups[0];
                foreach (ItemGroupDef c in set.Groups)
                {
                    pick -= Math.Max(1, c.Frequency);
                    if (pick < 0)
                    {
                        g = c;
                        break;
                    }
                }
                ushort hue = hues.Length > 0 ? hues[rnd.Next(hues.Length)] : (ushort)0;
                foreach (ItemPart p in g.Parts)
                {
                    int px = x + p.Dx, py = y + p.Dy;
                    if (!map.InBounds(px, py))
                    {
                        continue;
                    }
                    changes.Add(MapData.Add(new StaticItem
                    {
                        Id = p.Id, X = px, Y = py, Hue = hues.Length > 0 ? hue : p.Hue, Z = Math.Clamp(map.LandZ(x, y) + p.Z + o.ZOffset, -128, 127)
                    }));
                }
                placedNow.Add(new Point(x, y));
            }
            return changes;
        }

        private static bool TooClose(MapData map, int x, int y, int dist, HashSet<ushort> ids, List<Point> placedNow)
        {
            foreach (Point p in placedNow)
            {
                if (Math.Abs(p.X - x) <= dist && Math.Abs(p.Y - y) <= dist)
                {
                    return true;
                }
            }
            for (int yy = y - dist; yy <= y + dist; yy++)
            {
                for (int xx = x - dist; xx <= x + dist; xx++)
                {
                    if (map.InBounds(xx, yy) && map.StaticsAt(xx, yy).Any(s => ids.Contains(s.Id)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>Hue brush: every static in the brush gets the hue.</summary>
        public static List<StaticChange> HueBrush(MapData map, int cx, int cy, int r, ushort hue, Func<StaticItem, bool> filter = null)
        {
            var changes = new List<StaticChange>();
            foreach ((int x, int y, double _) in Circle(map, cx, cy, r))
            {
                foreach (StaticItem s in map.StaticsAt(x, y))
                {
                    if (s.Hue != hue && (filter == null || filter(s)))
                    {
                        changes.Add(MapData.Modify(s, t => t.Hue = hue));
                    }
                }
            }
            return changes;
        }

        /// <summary>
        /// Wall from a to b, snapped to the main direction (west – east or north – south): pieces along the line,
        /// posts at the ends, several levels.
        /// </summary>
        public static List<StaticChange> Wall(MapData map, Point a, Point b, WallSet set, WallSettings o, Random rnd)
        {
            var changes = new List<StaticChange>();
            if (set == null)
            {
                return changes;
            }
            bool alongX = Math.Abs(b.X - a.X) >= Math.Abs(b.Y - a.Y);
            ushort[] pieces = HexList.Parse(alongX ? set.AlongX : set.AlongY);
            ushort[] posts = HexList.Parse(set.Post);
            if (pieces.Length == 0 && posts.Length == 0)
            {
                return changes;
            }
            var end = alongX ? new Point(b.X, a.Y) : new Point(a.X, b.Y);
            int n = Math.Max(Math.Abs(end.X - a.X), Math.Abs(end.Y - a.Y));
            int sx = Math.Sign(end.X - a.X), sy = Math.Sign(end.Y - a.Y);
            int levelZ = map.LandZ(a.X, a.Y);
            for (int k = 0; k <= n; k++)
            {
                int x = a.X + sx * k, y = a.Y + sy * k;
                if (!map.InBounds(x, y))
                {
                    continue;
                }
                if (o.ReplaceExisting)
                {
                    changes.AddRange(map.StaticsAt(x, y).Select(MapData.Remove));
                }
                bool isEnd = k == 0 || k == n;
                ushort[] use = isEnd && o.PostsAtEnds && posts.Length > 0 ? posts : pieces.Length > 0 ? pieces : posts;
                int ground = (o.Level ? levelZ : map.LandZ(x, y)) + o.ZOffset;
                for (int l = 0; l < Math.Max(1, o.Levels); l++)
                {
                    changes.Add(MapData.Add(new StaticItem
                    {
                        Id = use[rnd.Next(use.Length)], X = x, Y = y, Z = Math.Clamp(ground + l * o.LevelHeight, -128, 127)
                    }));
                }
            }
            return changes;
        }

        /// <summary>Puts statics on the ground: the lowest one of every tile on the land height, the others keep their distance to it.</summary>
        public static List<StaticChange> DropToGround(MapData map, IEnumerable<StaticItem> statics)
        {
            var changes = new List<StaticChange>();
            foreach (IGrouping<(int, int), StaticItem> tile in statics.GroupBy(s => (s.X, s.Y)))
            {
                int low = tile.Min(s => s.Z);
                int dz = map.LandZ(tile.Key.Item1, tile.Key.Item2) - low;
                if (dz == 0)
                {
                    continue;
                }
                changes.AddRange(tile.Select(s => MapData.Modify(s, t => t.Z = Math.Clamp(t.Z + dz, -128, 127))));
            }
            return changes;
        }

        /// <summary>Rotates (quarter turns clockwise) or mirrors statics around the middle of their bounding box.</summary>
        public static List<StaticChange> TransformStatics(MapData map, IReadOnlyCollection<StaticItem> statics, int quarterTurns, bool mirrorX, bool mirrorY)
        {
            var changes = new List<StaticChange>();
            if (statics.Count == 0)
            {
                return changes;
            }
            int minX = statics.Min(s => s.X), maxX = statics.Max(s => s.X), minY = statics.Min(s => s.Y), maxY = statics.Max(s => s.Y);
            foreach (StaticItem s in statics)
            {
                (int nx, int ny) = Transform(s.X - minX, s.Y - minY, maxX - minX + 1, maxY - minY + 1, quarterTurns, mirrorX, mirrorY);
                nx += minX;
                ny += minY;
                if (map.InBounds(nx, ny) && (nx != s.X || ny != s.Y))
                {
                    changes.Add(MapData.Modify(s, t =>
                    {
                        t.X = nx;
                        t.Y = ny;
                    }));
                }
            }
            return changes;
        }

        /// <summary>Position in a w × h box after the transformation (the box itself turns: w and h swap for odd turns).</summary>
        public static (int x, int y) Transform(int x, int y, int w, int h, int quarterTurns, bool mirrorX, bool mirrorY)
        {
            if (mirrorX)
            {
                x = w - 1 - x;
            }
            if (mirrorY)
            {
                y = h - 1 - y;
            }
            for (int q = 0; q < ((quarterTurns % 4) + 4) % 4; q++)
            {
                (x, y) = (h - 1 - y, x);
                (w, h) = (h, w);
            }
            return (x, y);
        }

        /// <summary>
        /// Rotates / mirrors the land and the statics of a rectangle in place (top left stays; the size turns with odd turns).
        /// Note: the pictures of the statics do not turn – walls facing south stay facing south.
        /// </summary>
        public static (List<StaticChange> statics, List<LandChange> land) TransformArea(MapData map, Rectangle area, int quarterTurns, bool mirrorX, bool mirrorY)
        {
            var statics = new List<StaticChange>();
            var land = new List<LandChange>();
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (area.IsEmpty)
            {
                return (statics, land);
            }
            int w = area.Width, h = area.Height;
            var tiles = new ushort[w * h];
            var z = new sbyte[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = (area.Y + y) * map.Width + area.X + x;
                    tiles[y * w + x] = map.Tiles[i];
                    z[y * w + x] = map.Z[i];
                }
            }
            var written = new Dictionary<int, LandChange>();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    (int nx, int ny) = Transform(x, y, w, h, quarterTurns, mirrorX, mirrorY);
                    nx += area.X;
                    ny += area.Y;
                    if (!map.InBounds(nx, ny))
                    {
                        continue;
                    }
                    int i = ny * map.Width + nx;
                    written[i] = new LandChange { Index = i, OldId = map.Tiles[i], OldZ = map.Z[i], NewId = tiles[y * w + x], NewZ = z[y * w + x] };
                }
            }
            land.AddRange(written.Values.Where(c => c.OldId != c.NewId || c.OldZ != c.NewZ));
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    foreach (StaticItem s in map.StaticsAt(x, y).ToList())
                    {
                        (int nx, int ny) = Transform(x - area.X, y - area.Y, w, h, quarterTurns, mirrorX, mirrorY);
                        nx += area.X;
                        ny += area.Y;
                        if (!map.InBounds(nx, ny))
                        {
                            statics.Add(MapData.Remove(s));
                        }
                        else if (nx != s.X || ny != s.Y)
                        {
                            statics.Add(MapData.Modify(s, t =>
                            {
                                t.X = nx;
                                t.Y = ny;
                            }));
                        }
                    }
                }
            }
            return (statics, land);
        }

        /// <summary>Search &amp; replace: statics (ids, optionally only one hue) and land tiles inside the cells.</summary>
        public static (List<StaticChange> statics, List<LandChange> land) Replace(MapData map, Rectangle area, Func<int, int, bool> inside,
            ushort[] fromStatic, ushort[] toStatic, int onlyHue, int newHue, ushort[] fromLand, ushort[] toLand, Random rnd)
        {
            var statics = new List<StaticChange>();
            var land = new List<LandChange>();
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            var fs = new HashSet<ushort>(fromStatic ?? Array.Empty<ushort>());
            var fl = new HashSet<ushort>(fromLand ?? Array.Empty<ushort>());
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    if (inside != null && !inside(x, y))
                    {
                        continue;
                    }
                    if (fs.Count > 0)
                    {
                        foreach (StaticItem s in map.StaticsAt(x, y))
                        {
                            if (!fs.Contains(s.Id) || (onlyHue >= 0 && s.Hue != onlyHue))
                            {
                                continue;
                            }
                            if (toStatic == null || toStatic.Length == 0)
                            {
                                statics.Add(MapData.Remove(s)); // empty target = delete
                                continue;
                            }
                            ushort id = toStatic.Length == fromStatic.Length ? toStatic[Array.IndexOf(fromStatic, s.Id)] : toStatic[rnd.Next(toStatic.Length)];
                            statics.Add(MapData.Modify(s, t =>
                            {
                                t.Id = id;
                                if (newHue >= 0)
                                {
                                    t.Hue = (ushort)newHue;
                                }
                            }));
                        }
                    }
                    if (fl.Count > 0 && toLand != null && toLand.Length > 0)
                    {
                        int i = y * map.Width + x;
                        ushort old = map.Tiles[i];
                        if (fl.Contains(old))
                        {
                            ushort id = toLand.Length == fromLand.Length ? toLand[Array.IndexOf(fromLand, old)] : toLand[rnd.Next(toLand.Length)];
                            if (id != old)
                            {
                                land.Add(new LandChange { Index = i, OldId = old, OldZ = map.Z[i], NewId = id, NewZ = map.Z[i] });
                            }
                        }
                    }
                }
            }
            return (statics, land);
        }

        // ------------------------------------------------------------------ land

        /// <summary>River brush (one dab): water tiles at the water level inside, the bank rises softly outside.</summary>
        public static void River(MapData map, int cx, int cy, int r, int level, RiverSettings o, Func<int, int, int> groundBefore, Random rnd)
        {
            ushort[] water = HexList.Parse(o.WaterTiles);
            if (water.Length == 0)
            {
                water = new ushort[] { 0x00A8, 0x00A9, 0x00AA, 0x00AB };
            }
            int bank = Math.Max(0, o.BankWidth);
            foreach ((int x, int y, double d) in Circle(map, cx, cy, r + bank))
            {
                int lv = o.FollowTerrain ? groundBefore(x, y) - o.Depth : level;
                ushort id = map.LandId(x, y);
                if (d <= r + 0.5)
                {
                    ushort nid = Array.IndexOf(water, id) >= 0 ? id : water[rnd.Next(water.Length)];
                    map.SetLand(x, y, nid, lv);
                    continue;
                }
                if (Array.IndexOf(water, id) >= 0)
                {
                    continue; // water of an earlier dab stays water
                }
                int z = map.LandZ(x, y);
                int max = lv + (int)Math.Ceiling(d - r) * Math.Max(1, o.BankStep);
                if (z > max)
                {
                    map.SetLand(x, y, id, max);
                }
            }
        }

        /// <summary>Points along a smooth curve (Catmull-Rom) through the clicked points, about one per half tile.</summary>
        public static List<PointF> Spline(IReadOnlyList<Point> pts)
        {
            var result = new List<PointF>();
            if (pts.Count == 0)
            {
                return result;
            }
            if (pts.Count == 1)
            {
                result.Add(new PointF(pts[0].X + 0.5f, pts[0].Y + 0.5f));
                return result;
            }
            PointF P(int i)
            {
                Point p = pts[Math.Clamp(i, 0, pts.Count - 1)];
                return new PointF(p.X + 0.5f, p.Y + 0.5f);
            }
            for (int i = 0; i < pts.Count - 1; i++)
            {
                PointF p0 = P(i - 1), p1 = P(i), p2 = P(i + 1), p3 = P(i + 2);
                double len = Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
                int steps = Math.Max(2, (int)Math.Ceiling(len * 2));
                for (int k = 0; k < steps; k++)
                {
                    float t = k / (float)steps, t2 = t * t, t3 = t2 * t;
                    float x = 0.5f * (2 * p1.X + (-p0.X + p2.X) * t + (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 + (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3);
                    float y = 0.5f * (2 * p1.Y + (-p0.Y + p2.Y) * t + (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 + (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3);
                    result.Add(new PointF(x, y));
                }
            }
            result.Add(P(pts.Count - 1));
            return result;
        }

        /// <summary>
        /// Road along the curve: road tiles within half the width, heights smoothed along the curve (or an even slope),
        /// the shoulder next to it blends to the road height. Returns the changed area.
        /// </summary>
        public static Rectangle Road(MapData map, IReadOnlyList<Point> points, RoadSettings o, ushort[] tiles, Random rnd)
        {
            List<PointF> curve = Spline(points);
            if (curve.Count == 0)
            {
                return Rectangle.Empty;
            }
            // height along the curve: ground smoothed over 'Smoothing' tiles, or even slope from start to end
            var ground = curve.Select(p => (double)map.LandZ((int)p.X, (int)p.Y)).ToArray();
            var h = new double[curve.Count];
            int win = Math.Max(0, o.Smoothing) * 2; // samples are half a tile apart
            for (int i = 0; i < curve.Count; i++)
            {
                if (!o.FollowTerrain)
                {
                    h[i] = ground[0] + (ground[ground.Length - 1] - ground[0]) * i / Math.Max(1, curve.Count - 1);
                    continue;
                }
                double sum = 0;
                int n = 0;
                for (int k = Math.Max(0, i - win); k <= Math.Min(curve.Count - 1, i + win); k++)
                {
                    sum += ground[k];
                    n++;
                }
                h[i] = sum / n;
            }
            return Stripe(map, curve, h, Math.Max(1, o.Width) / 2.0, Math.Max(0, o.Shoulder), tiles, rnd);
        }

        /// <summary>Ramp from a to b: even slope from the ground at a to the ground at b (or the given heights).</summary>
        public static Rectangle Ramp(MapData map, Point a, Point b, int width, int za, int zb, RampSettings o, ushort[] tiles, Random rnd)
        {
            var curve = new List<PointF>();
            double len = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
            int steps = Math.Max(1, (int)Math.Ceiling(len * 2));
            for (int k = 0; k <= steps; k++)
            {
                float t = k / (float)steps;
                curve.Add(new PointF(a.X + 0.5f + (b.X - a.X) * t, a.Y + 0.5f + (b.Y - a.Y) * t));
            }
            double[] h = Enumerable.Range(0, curve.Count).Select(k => za + (zb - za) * k / (double)steps).ToArray();
            return Stripe(map, curve, h, Math.Max(1, width) / 2.0, Math.Max(0, o.Shoulder), o.KeepTiles ? null : tiles, rnd);
        }

        /// <summary>Cells near the curve get the height of the nearest curve point (and the tiles), the shoulder blends.</summary>
        private static Rectangle Stripe(MapData map, List<PointF> curve, double[] h, double half, int shoulder, ushort[] tiles, Random rnd)
        {
            double reach = half + shoulder + 0.5;
            int minX = (int)Math.Floor(curve.Min(p => p.X) - reach), maxX = (int)Math.Ceiling(curve.Max(p => p.X) + reach);
            int minY = (int)Math.Floor(curve.Min(p => p.Y) - reach), maxY = (int)Math.Ceiling(curve.Max(p => p.Y) + reach);
            var box = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            box.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            for (int y = box.Top; y < box.Bottom; y++)
            {
                for (int x = box.Left; x < box.Right; x++)
                {
                    double best = double.MaxValue;
                    int bi = 0;
                    for (int i = 0; i < curve.Count; i++)
                    {
                        double dx = x + 0.5 - curve[i].X, dy = y + 0.5 - curve[i].Y, d = dx * dx + dy * dy;
                        if (d < best)
                        {
                            best = d;
                            bi = i;
                        }
                    }
                    double dist = Math.Sqrt(best);
                    if (dist > reach)
                    {
                        continue;
                    }
                    ushort id = map.LandId(x, y);
                    int z = map.LandZ(x, y);
                    int target = (int)Math.Round(h[bi], MidpointRounding.AwayFromZero);
                    if (dist <= half + 0.25)
                    {
                        ushort nid = tiles == null || tiles.Length == 0 ? id : Array.IndexOf(tiles, id) >= 0 ? id : tiles[rnd.Next(tiles.Length)];
                        map.SetLand(x, y, nid, target);
                    }
                    else
                    {
                        // shoulder: halfway (or more near the road) to the road height
                        double t = 1 - (dist - half) / (shoulder + 0.5);
                        map.SetLand(x, y, id, (int)Math.Round(z + (target - z) * Math.Clamp(t, 0, 1)));
                    }
                }
            }
            return box;
        }

        /// <summary>Plateau brush (one dab): flatten towards a level or round to terraces, soft edge with the hardness.</summary>
        public static void Plateau(MapData map, int cx, int cy, int r, int level, PlateauSettings o, int hardness, Random rnd)
        {
            foreach ((int x, int y, double d) in Circle(map, cx, cy, r))
            {
                double w = Weight(d, r, hardness);
                int z = map.LandZ(x, y);
                int target = o.Mode == PlateauMode.Terraces && o.TerraceStep > 0
                    ? (int)Math.Floor(z / (double)o.TerraceStep) * o.TerraceStep
                    : level;
                map.SetLand(x, y, map.LandId(x, y), z + Dither((target - z) * w, rnd));
            }
        }

        /// <summary>Smooth value noise 0..1.</summary>
        public static double Noise(int x, int y, int scale, int seed)
        {
            scale = Math.Max(1, scale);
            double fx = x / (double)scale, fy = y / (double)scale;
            int x0 = (int)Math.Floor(fx), y0 = (int)Math.Floor(fy);
            double tx = fx - x0, ty = fy - y0;
            tx = tx * tx * (3 - 2 * tx);
            ty = ty * ty * (3 - 2 * ty);
            double a = Hash(x0, y0, seed), b = Hash(x0 + 1, y0, seed), c = Hash(x0, y0 + 1, seed), d = Hash(x0 + 1, y0 + 1, seed);
            return a + (b - a) * tx + (c - a) * ty + (a - b - c + d) * tx * ty;
        }

        private static double Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 982451653);
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0xFFFFFF) / (double)0xFFFFFF;
            }
        }

        /// <summary>Noise brush (one dab): ± Amplitude by smooth noise; every cell once per stroke (no adding up).</summary>
        public static void NoiseBrush(MapData map, int cx, int cy, int r, NoiseSettings o, int hardness, HashSet<int> visited, Func<int, int, int> groundBefore, Random rnd)
        {
            foreach ((int x, int y, double d) in Circle(map, cx, cy, r))
            {
                if (!visited.Add(y * map.Width + x))
                {
                    continue;
                }
                double n = Noise(x, y, o.Scale, o.Seed) * 2 - 1;
                double w = Weight(d, r, hardness);
                map.SetLand(x, y, map.LandId(x, y), groundBefore(x, y) + Dither(n * o.Amplitude * w, rnd));
            }
        }

        /// <summary>Variant brush (one dab): every tile becomes a random tile of its family (same colour of the colour table).</summary>
        public static void Variants(MapData map, int cx, int cy, int r, Func<ushort, ushort[]> family, HashSet<int> visited, Random rnd)
        {
            foreach ((int x, int y, double _) in Circle(map, cx, cy, r))
            {
                if (!visited.Add(y * map.Width + x))
                {
                    continue;
                }
                ushort id = map.LandId(x, y);
                ushort[] f = family?.Invoke(id);
                if (f != null && f.Length > 1)
                {
                    map.SetLand(x, y, f[rnd.Next(f.Length)], map.LandZ(x, y));
                }
            }
        }

        // ------------------------------------------------------------------ selection masks

        /// <summary>Magic wand: the connected cells like the clicked one (tile, family or height).</summary>
        public static HashSet<int> Wand(MapData map, int sx, int sy, WandSettings o, Func<ushort, ushort[]> family)
        {
            var cells = new HashSet<int>();
            if (!map.InBounds(sx, sy))
            {
                return cells;
            }
            ushort id0 = map.LandId(sx, sy);
            var fam = new HashSet<ushort>(family?.Invoke(id0) ?? new[] { id0 }) { id0 };
            int z0 = map.LandZ(sx, sy);
            bool Same(int x, int y, int fromZ)
            {
                ushort id = map.LandId(x, y);
                int z = map.LandZ(x, y);
                if (Math.Abs(z - fromZ) > Math.Max(0, o.ZTolerance) && o.Mode != WandMode.SameHeight)
                {
                    return false;
                }
                return o.Mode switch
                {
                    WandMode.SameTile => id == id0,
                    WandMode.SameFamily => fam.Contains(id),
                    _ => Math.Abs(z - z0) <= Math.Max(0, o.ZTolerance)
                };
            }
            var q = new Queue<(int x, int y)>();
            q.Enqueue((sx, sy));
            cells.Add(sy * map.Width + sx);
            int[] dx = { 1, -1, 0, 0 }, dy = { 0, 0, 1, -1 };
            while (q.Count > 0 && cells.Count < Math.Max(1, o.MaxCells))
            {
                (int x, int y) = q.Dequeue();
                int z = map.LandZ(x, y);
                for (int k = 0; k < 4; k++)
                {
                    int nx = x + dx[k], ny = y + dy[k];
                    if (!map.InBounds(nx, ny))
                    {
                        continue;
                    }
                    int i = ny * map.Width + nx;
                    if (!cells.Contains(i) && Same(nx, ny, z))
                    {
                        cells.Add(i);
                        q.Enqueue((nx, ny));
                    }
                }
            }
            return cells;
        }

        /// <summary>Lasso: the cells whose middle lies inside the polygon (map coordinates).</summary>
        public static HashSet<int> Polygon(MapData map, IReadOnlyList<PointF> poly)
        {
            var cells = new HashSet<int>();
            if (poly.Count < 3)
            {
                return cells;
            }
            int minX = Math.Max(0, (int)Math.Floor(poly.Min(p => p.X))), maxX = Math.Min(map.Width - 1, (int)Math.Ceiling(poly.Max(p => p.X)));
            int minY = Math.Max(0, (int)Math.Floor(poly.Min(p => p.Y))), maxY = Math.Min(map.Height - 1, (int)Math.Ceiling(poly.Max(p => p.Y)));
            for (int y = minY; y <= maxY; y++)
            {
                double py = y + 0.5;
                for (int x = minX; x <= maxX; x++)
                {
                    double px = x + 0.5;
                    bool inside = false;
                    for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
                    {
                        if ((poly[i].Y > py) != (poly[j].Y > py) &&
                            px < (poly[j].X - poly[i].X) * (py - poly[i].Y) / (poly[j].Y - poly[i].Y) + poly[i].X)
                        {
                            inside = !inside;
                        }
                    }
                    if (inside)
                    {
                        cells.Add(y * map.Width + x);
                    }
                }
            }
            return cells;
        }

        /// <summary>Bounding box of a cell mask.</summary>
        public static Rectangle Bounds(MapData map, HashSet<int> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return Rectangle.Empty;
            }
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (int i in cells)
            {
                int x = i % map.Width, y = i / map.Width;
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }
            return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }
    }
}
