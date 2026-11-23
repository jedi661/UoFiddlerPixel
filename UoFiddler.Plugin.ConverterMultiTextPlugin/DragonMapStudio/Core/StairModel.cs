/***************************************************************************
 *
 *  * Dragon Map Studio – stair models: a stair built by hand (blocks stepped up, the sloped stair piece on top),
 *  *  saved by category and placed on the map in any direction, up, down or under the map, repeated for more floors.
 *  *
 *  *  The model remembers the direction it goes up. Turned to another direction the positions turn and every stair
 *  *  piece is swapped for the piece of the same name whose art goes up the new way (BridgeKit.StairUp);
 *  *  railings / walls are swapped by their art direction (BridgeKit.Turned), blocks stay.
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
    /// <summary>
    /// One piece of a model in all four directions: the graphic used when the model goes up towards north, east, south, west.
    /// A block without direction has the same id four times. Set automatically from the art, changed by hand in the designer.
    /// </summary>
    public sealed class StairTurn4
    {
        public ushort N { get; set; }
        public ushort E { get; set; }
        public ushort S { get; set; }
        public ushort W { get; set; }

        public ushort this[Compass c]
        {
            get => c switch { Compass.North => N, Compass.East => E, Compass.South => S, _ => W };
            set
            {
                switch (c)
                {
                    case Compass.North:
                        N = value;
                        break;
                    case Compass.East:
                        E = value;
                        break;
                    case Compass.South:
                        S = value;
                        break;
                    default:
                        W = value;
                        break;
                }
            }
        }

        public bool Contains(ushort id) => N == id || E == id || S == id || W == id;
    }

    public sealed class StairModel
    {
        [Category("Modell"), DisplayName("Name")]
        public string Name { get; set; } = "Meine Treppe";

        [Category("Modell"), DisplayName("Kategorie"), Description("Ordner, z. B. Holz, Stein, Keller, Turm.")]
        public string Category { get; set; } = "Allgemein";

        [Category("Modell"), DisplayName("Gebaut hinauf nach"), Description("In diese Richtung steigt das Modell an, so wie es gebaut wurde.")]
        public Compass Up { get; set; } = Compass.North;

        [Category("Modell"), DisplayName("Anstieg (Z)"), Description("So viel höher endet das Modell (Original: 20 = eine Etage). Für 'mehrmals' und 'nach unten'.")]
        public int Rise { get; set; } = 20;

        [Category("Modell"), DisplayName("Länge (Tiles)"), Description("So weit reicht das Modell in Aufstiegs-Richtung (für 'mehrmals': der nächste Lauf beginnt dahinter).")]
        public int Run { get; set; } = 5;

        [Category("Modell"), DisplayName("Notiz")]
        public string Note { get; set; } = string.Empty;

        [Browsable(false)]
        public List<PieceStatic> Parts { get; set; } = new List<PieceStatic>();

        /// <summary>Graphics of every piece for the four directions (the model as built = its column <see cref="Up"/>).</summary>
        [Browsable(false)]
        public List<StairTurn4> Turns { get; set; } = new List<StairTurn4>();

        /// <summary>The row of the turn table for a piece of the model (as built).</summary>
        public StairTurn4 TurnOf(ushort id) => Turns.FirstOrDefault(t => t[Up] == id);

        /// <summary>Fills the turn table: stair pieces by their slope (same name), railings / walls by their art, blocks stay.</summary>
        public void AutoTurns(bool keepExisting = true)
        {
            foreach (ushort id in Parts.Select(p => p.Id).Distinct())
            {
                if (keepExisting && TurnOf(id) != null)
                {
                    continue;
                }
                Turns.RemoveAll(t => t[Up] == id);
                var row = new StairTurn4();
                foreach (Compass c in new[] { Compass.North, Compass.East, Compass.South, Compass.West })
                {
                    row[c] = TurnedPiece(id, ((int)c - (int)Up + 4) % 4);
                }
                Turns.Add(row);
            }
            Turns.RemoveAll(t => !Parts.Any(p => p.Id == t[Up]));
        }

        /// <summary>Direction the model goes up, read from its stair pieces (most of them), null when none says it.</summary>
        public Compass? GuessUp()
        {
            var votes = Parts.Select(p => UpOf(p.Id)).Where(u => u != null).GroupBy(u => u.Value).OrderByDescending(g => g.Count()).FirstOrDefault();
            return votes?.Key;
        }

        /// <summary>For dragging: the stair piece going up towards <paramref name="up"/> and the block (most common piece without slope).</summary>
        public (ushort stair, ushort block) StepPieces(Compass up)
        {
            int q = (((int)up - (int)Up) % 4 + 4) % 4;
            ushort stairBuilt = Parts.Where(p => UpOf(p.Id) == Up).GroupBy(p => p.Id).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            ushort block = Parts.Where(p => UpOf(p.Id) == null).GroupBy(p => p.Id).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            return (stairBuilt == 0 ? (ushort)0 : Piece(stairBuilt, q), block == 0 ? (ushort)0 : Piece(block, q));
        }

        /// <summary>The piece of the model turned by q quarter turns: from the turn table, else automatic.</summary>
        public ushort Piece(ushort id, int q)
        {
            q = ((q % 4) + 4) % 4;
            StairTurn4 row = TurnOf(id);
            if (row != null)
            {
                ushort t = row[(Compass)(((int)Up + q) % 4)];
                if (t != 0)
                {
                    return t;
                }
            }
            return TurnedPiece(id, q);
        }

        public override string ToString() => Category + " / " + Name;

        /// <summary>The statics of a rectangle; heights relative to <paramref name="groundZ"/>, positions to the top left corner.</summary>
        public static StairModel Capture(MapData map, Rectangle r, Compass up, int groundZ, string name)
        {
            var m = new StairModel { Name = name, Up = up };
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    foreach (StaticItem s in map.StaticsAt(x, y))
                    {
                        m.Parts.Add(new PieceStatic { X = x - r.Left, Y = y - r.Top, Z = s.Z - groundZ, Id = s.Id, Hue = s.Hue });
                    }
                }
            }
            if (m.Parts.Count > 0)
            {
                // the direction from the stair pieces themselves (their slope), else the one given
                Compass? guess = m.GuessUp();
                if (guess != null)
                {
                    m.Up = guess.Value;
                }
                m.Rise = m.Parts.Max(p => p.Z + Math.Max(0, UoArtCache.StackHeight(p.Id))) - m.Parts.Min(p => p.Z);
                m.Run = m.Up == Compass.North || m.Up == Compass.South ? r.Height : r.Width;
                m.AutoTurns(false);
            }
            return m;
        }

        // ------------------------------------------------------------------ turning

        private static readonly Dictionary<(ushort, int), ushort> _turned = new Dictionary<(ushort, int), ushort>();
        private static readonly Dictionary<ushort, Compass?> _up = new Dictionary<ushort, Compass?>();

        public static Compass? UpOf(ushort id)
        {
            lock (_up)
            {
                if (_up.TryGetValue(id, out Compass? c))
                {
                    return c;
                }
            }
            Compass? r;
            try
            {
                r = BridgeKit.StairUp(id, out _);
                if (UoArtCache.ItemHeight(id) > 6)
                {
                    r = null; // a tall block is no sloped piece
                }
            }
            catch (Exception)
            {
                r = null;
            }
            lock (_up)
            {
                _up[id] = r;
            }
            return r;
        }

        /// <summary>The piece for the model turned by <paramref name="q"/> quarter turns clockwise.</summary>
        public static ushort TurnedPiece(ushort id, int q)
        {
            q = ((q % 4) + 4) % 4;
            if (q == 0)
            {
                return id;
            }
            lock (_turned)
            {
                if (_turned.TryGetValue((id, q), out ushort t))
                {
                    return t;
                }
            }
            ushort result = id;
            Compass? up = UpOf(id);
            string name = UoArtCache.ItemName(id).Trim().ToLowerInvariant();
            if (up != null)
            {
                var want = (Compass)(((int)up.Value + q) % 4);
                for (int d = 1; d <= 24 && result == id; d++)
                {
                    foreach (int c in new[] { id + d, id - d })
                    {
                        if (c > 0 && c <= UoArtCache.MaxItemId && UoArtCache.ItemName(c).Trim().ToLowerInvariant() == name && UpOf((ushort)c) == want)
                        {
                            result = (ushort)c;
                            break;
                        }
                    }
                }
            }
            else if (q % 2 == 1 && !UoArtCache.ItemHas(id, Ultima.TileFlag.Surface))
            {
                // railings, walls: the partner with the art the other way; blocks and floors (surface) stay
                try
                {
                    result = BridgeKit.Turned(id);
                }
                catch (Exception)
                {
                    result = id; // no art: keep the piece
                }
            }
            lock (_turned)
            {
                _turned[(id, q)] = result;
            }
            return result;
        }

        // ------------------------------------------------------------------ build

        /// <summary>
        /// The model at <paramref name="at"/> (top left of the turned model), going up towards <paramref name="up"/>,
        /// <paramref name="repeats"/> times one after the other (every one <see cref="Run"/> further, <see cref="Rise"/> higher).
        /// Downwards: the click is the top, the model ends there.
        /// </summary>
        public List<StaticChange> Build(MapData map, Point at, int z, Compass up, int repeats, bool downwards)
        {
            var result = new List<StaticChange>();
            if (Parts.Count == 0)
            {
                return result;
            }
            int q = (((int)up - (int)Up) % 4 + 4) % 4;
            int n = Math.Clamp(repeats, 1, 20);
            int baseZ = downwards ? z - Rise * n : z;
            // turn the positions (clockwise: (x, y) → (−y, x)), then back to a top left corner of 0, 0
            var turned = Parts.Select(p =>
            {
                int x = p.X, y = p.Y;
                for (int i = 0; i < q; i++)
                {
                    (x, y) = (-y, x);
                }
                return (x, y, p);
            }).ToList();
            int minX = turned.Min(t => t.x), minY = turned.Min(t => t.y);
            Point step = Staircase.Step(up);
            var placed = new HashSet<(int, int, int, ushort)>();
            for (int r = 0; r < n; r++)
            {
                foreach ((int x, int y, PieceStatic p) in turned)
                {
                    int tx = at.X + x - minX + step.X * Run * r, ty = at.Y + y - minY + step.Y * Run * r;
                    int tz = Math.Clamp(baseZ + p.Z + Rise * r, -128, 127);
                    ushort id = Piece(p.Id, q);
                    if (!map.InBounds(tx, ty) || !placed.Add((tx, ty, tz, id)))
                    {
                        continue;
                    }
                    result.Add(MapData.Add(new StaticItem { Id = id, Hue = p.Hue, X = tx, Y = ty, Z = tz }));
                }
            }
            return result;
        }

        /// <summary>Text for the developer: every piece with position, height, name, item height and slope direction.</summary>
        public string Describe()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Treppen-Modell '{this}': hinauf {Up}, Anstieg {Rise} Z, Länge {Run}, {Parts.Count} Teile");
            foreach (PieceStatic p in Parts.OrderBy(p => p.Y).ThenBy(p => p.X).ThenBy(p => p.Z))
            {
                sb.AppendLine($"  ({p.X},{p.Y}) z {p.Z,3}  {p.Id:x4} {UoArtCache.ItemName(p.Id),-20} h{UoArtCache.ItemHeight(p.Id)}  hinauf {UpOf(p.Id)?.ToString() ?? "-"}  hue {p.Hue}");
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------ files

        public static string Folder => Path.Combine(StudioSettings.SettingsFolder, "Treppen");

        private static string Safe(string s) => string.Concat((s ?? "x").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();

        public string Save()
        {
            string dir = Path.Combine(Folder, Safe(string.IsNullOrWhiteSpace(Category) ? "Allgemein" : Category));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, Safe(Name) + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            return path;
        }

        public static List<StairModel> LoadAll()
        {
            var list = new List<StairModel>();
            if (!Directory.Exists(Folder))
            {
                return list;
            }
            foreach (string f in Directory.GetFiles(Folder, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    StairModel m = JsonSerializer.Deserialize<StairModel>(File.ReadAllText(f));
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
