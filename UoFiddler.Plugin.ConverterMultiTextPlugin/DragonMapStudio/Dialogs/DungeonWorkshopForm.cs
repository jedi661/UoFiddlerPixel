/***************************************************************************
 *
 *  * Dragon Map Studio – dungeon workshop: an own, big window for dungeons and cellars
 *  *
 *  *  The marked place of the CGI map is copied into a working copy. Everything happens there first – two CGI views
 *  *  (above: the map, below: the level under the map), a 2D plan to draw by hand, the style as script, undo / redo,
 *  *  move, height, clear – and only "In die Karte übernehmen" writes it back (one undo step of the map).
 *  *
 *  *  Kinds: classic dungeon (floor texture + wall statics in a learned style or the built-in map0 style),
 *  *  mountain dungeon (rock walls in a grid), mountain wall (rock labyrinth), cellar (statics under the map).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    /// <summary>What the workshop needs from the studio.</summary>
    public sealed class WorkshopHost
    {
        public MapData Map;
        public Rectangle Area;
        public CgiBrushSettings Brushes;
        public Action<IsoMapView> Configure;
        public Func<Rectangle> CurrentArea;
        public Func<List<int>> LandList;
        public Func<ushort[]> RockTiles;
        public Func<ushort[]> CaveFloor;
        public Func<string, string, string> WriteLog;
        public Action<string> Status;
        public Action Applied;
        public Action<string> Help;
    }

    public enum WorkshopKind
    {
        Classic,
        MountainDungeon,
        MountainWall,
        Cellar
    }

    public sealed class DungeonWorkshopForm : Form
    {
        private readonly WorkshopHost _host;
        private MapData _work;
        private Point _origin;
        private Rectangle _content;
        private int _ground;
        private int _level;
        private ushort _void = 0x0002;
        private WorkshopKind _kind;
        private DungeonPlan _plan;

        private readonly IsoMapView _top = new IsoMapView { Dock = DockStyle.Fill };
        private readonly IsoMapView _bottom = new IsoMapView { Dock = DockStyle.Fill };
        private IsoMapView _active;
        private readonly PlanEditor _planView = new PlanEditor { Dock = DockStyle.Fill };
        private readonly TextBox _script = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 9f) };
        private readonly TextBox _log = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false, ReadOnly = true, Font = new Font("Consolas", 9f) };
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
        private Label _levelText;
        private TrackBar _levelBar;
        private NumericUpDown _levelNum;
        private TextBox _voidBox;
        private PictureBox _voidPic;
        private ComboBox _style;
        private readonly Dictionary<WorkshopKind, RadioButton> _kindButtons = new Dictionary<WorkshopKind, RadioButton>();
        private bool _syncing;
        private bool _holeTool;
        private readonly ArtCatalog _catalog = new ArtCatalog();
        private int _radius;
        private bool _grid;
        private bool _statics = true;
        private int _placeId = -1;
        private int _landId = -1;
        private int _multiId = -1;
        private ComboBox _pieces;
        private ForeignMapForm _foreign;

        public DungeonWorkshopForm(WorkshopHost host, WorkshopKind kind)
        {
            _host = host;
            _kind = kind;
            Text = L.T("Dungeon-Werkstatt") + " – Dragon Map Studio";
            Size = new Size(1400, 900);
            MinimumSize = new Size(1000, 640);
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;
            Font = new Font("Segoe UI", 9f);
            BuildUi();
            _catalog.ShowCategory(ArtKind.Static, "Dungeon (map0)");
            LoadPlace(host.Area);
        }

        // ================================================================== working copy

        private void LoadPlace(Rectangle area)
        {
            if (area.IsEmpty || _host.Map == null)
            {
                return;
            }
            Rectangle region = Rectangle.Inflate(area, 8, 8);
            region.Intersect(new Rectangle(0, 0, _host.Map.Width, _host.Map.Height));
            _work = _host.Map.CopyRegion(region, out _origin);
            _content = new Rectangle(area.X - _origin.X, area.Y - _origin.Y, area.Width, area.Height);
            _ground = Workshop.GroundLevel(_host.Map, area);
            _plan = null;
            ConfigureViews();
            foreach (IsoMapView v in new[] { _top, _bottom })
            {
                v.SetMap(_work);
                v.SetArea(_content);
                v.CenterOn(_content.X + _content.Width / 2.0, _content.Y + _content.Height / 2.0);
            }
            _bottom.UnderView = true;
            SetLevel(_kind == WorkshopKind.Cellar ? _ground - 20 : _ground);
            _planView.Plan = null;
            Say(L.T("Platz") + $" {area.X}, {area.Y} – {area.Width}×{area.Height}, " + L.T("Kartenhöhe") + $" z {_ground}. " +
                L.T("Alles geschieht erst hier; 'In die Karte übernehmen' schreibt es zurück."));
        }

        private void SetLevel(int z)
        {
            _level = Math.Clamp(z, -128, 127);
            _syncing = true;
            try
            {
                _levelNum.Value = _level;
                int step = (int)Math.Round((_level - _ground) / 20.0);
                _levelBar.Value = Math.Clamp(step, _levelBar.Minimum, _levelBar.Maximum);
            }
            finally
            {
                _syncing = false;
            }
            int floor = (_level - _ground) / 20;
            _levelText.Text = (floor == 0 ? L.T("Ebene der Karte") : floor < 0 ? L.T("Etage") + $" {floor} (" + L.T("unter der Karte") + ")" : L.T("Etage") + $" +{floor}") +
                              $" · z {_level} · " + L.T("Kartenhöhe") + $" {_ground}";
            // the lower view shows the level and hides what is above it
            _bottom.MaxZ = _level < _ground ? _level + 19 : 127;
            ConfigureViews();
            _top.Refresh(true);
            _bottom.Refresh(true);
        }

        /// <summary>Both views get the settings of the studio; the lower one its own copy with the building level.</summary>
        private void ConfigureViews()
        {
            IsoTool t1 = _top.Tool, t2 = _bottom.Tool;
            _host.Configure?.Invoke(_top);
            _host.Configure?.Invoke(_bottom);
            _top.Tool = t1;
            _bottom.Tool = t2;
            foreach (IsoMapView v in new[] { _top, _bottom })
            {
                v.LandRadius = _radius;
                v.ShowGrid = _grid;
                v.ShowStatics = _statics;
                if (_placeId >= 0)
                {
                    v.PlaceId = (ushort)_placeId;
                }
                if (_landId >= 0)
                {
                    v.PlaceLandId = (ushort)_landId;
                    v.PlaceLandIds = new[] { (ushort)_landId };
                }
                if (_multiId >= 0)
                {
                    v.MultiId = _multiId;
                }
            }
            if (_holeTool)
            {
                foreach (IsoMapView v in new[] { _top, _bottom })
                {
                    v.PlaceLandId = 0x0002;
                    v.PlaceLandIds = new ushort[] { 0x0002 };
                    v.LandFlatten = false;
                }
            }
            CgiBrushSettings copy = JsonSerializer.Deserialize<CgiBrushSettings>(JsonSerializer.Serialize(_host.Brushes)) ?? new CgiBrushSettings();
            copy.Normalize();
            copy.UnderOffset = Math.Min(0, _level - _ground);
            _bottom.BrushSettings = copy;
        }

        private void Changed(string what)
        {
            _top.Refresh(true);
            _bottom.Refresh(true);
            _top.SetArea(_content);
            _bottom.SetArea(_content);
            _planView.Plan = _plan;
            Say(what);
        }

        private void Say(string text)
        {
            _status.Text = "  " + text;
            _host.Status?.Invoke(text);
        }

        private HashSet<ushort> VoidSet => new HashSet<ushort> { 0x0002, 0x0244, _void };

        private DungeonSettings Settings => _host.Brushes.Dungeon;

        private DungeonLook ChosenLook()
        {
            if (_style.SelectedItem is DungeonLook l)
            {
                return l;
            }
            return Workshop.Classic();
        }

        // ================================================================== actions

        /// <summary>Clears the place and builds the chosen kind there (one undo step of the working copy).</summary>
        private void Generate(bool newPlan)
        {
            if (_work == null)
            {
                return;
            }
            int seed = Settings.Seed != 0 ? Settings.Seed : new Random().Next(1, int.MaxValue);
            var rnd = new Random(seed);
            DungeonSettings o = Settings;
            if (newPlan || _plan == null || _plan.W != _content.Width || _plan.H != _content.Height)
            {
                DungeonSettings p = Copy(o);
                if (_kind == WorkshopKind.MountainWall)
                {
                    p.Layout = DungeonLayout.Corridors;
                    p.Rough = Math.Max(30, p.Rough);
                }
                else if (_kind == WorkshopKind.MountainDungeon)
                {
                    p.Layout = DungeonLayout.Grid;
                }
                _plan = _kind == WorkshopKind.Cellar ? null : Builders.Plan(_content.Width, _content.Height, p, new Random(seed));
            }
            _work.BeginLand();
            // 1) empty the place: statics away, land = void tile on its own height (no holes into the depth)
            if (_kind != WorkshopKind.Cellar)
            {
                // the mountain kinds keep the land (they raise rock from it), the classic dungeon starts from nothing
                (List<StaticChange> s, List<LandChange> l) = Workshop.Clear(_work, _content, true, _kind == WorkshopKind.Classic, _void);
                _work.ApplyStatics(s);
                foreach (LandChange c in l)
                {
                    _work.SetLand(c.Index % _work.Width, c.Index / _work.Width, c.NewId, c.NewZ);
                }
            }
            string what;
            switch (_kind)
            {
                case WorkshopKind.Classic:
                    {
                        DungeonLook look = ChosenLook();
                        var use = new DungeonLook
                        {
                            Name = look.Name, VoidLand = _void.ToString("x4"), FloorLand = look.FloorLand, FloorStatics = look.FloorStatics,
                            DecorPercent = look.DecorPercent, Decor = look.Decor, Walls = look.Walls
                        };
                        bool under = _level < _ground;
                        int floors = Math.Clamp(o.Floors, 1, 6);
                        int fh = Math.Max(5, o.FloorHeight);
                        // lower floors first (statics only, under the map), then the top floor
                        var plans = new List<DungeonPlan> { _plan };
                        for (int f = 1; f < floors; f++)
                        {
                            DungeonSettings pf = Copy(o);
                            plans.Add(Builders.Plan(_content.Width, _content.Height, pf, new Random(seed + f * 7919)));
                        }
                        for (int f = floors - 1; f >= 1; f--)
                        {
                            _work.ApplyStatics(DungeonStyleTools.Build(_work, _content, plans[f], use, _level - f * fh, true, false, false, rnd));
                        }
                        _work.ApplyStatics(DungeonStyleTools.Build(_work, _content, _plan, use, _level, under, true, false, rnd));
                        string stairsNote = floors > 1 ? ConnectFloors(plans, fh) : string.Empty;
                        what = L.T("Klassischer Dungeon") + $" '{look.Name}': {_plan.Rooms.Count} " + L.T("Räume") + (under ? ", " + L.T("unter der Karte") : string.Empty) +
                               (floors > 1 ? $", {floors} " + L.T("Etagen") + stairsNote : string.Empty);
                        break;
                    }
                case WorkshopKind.Cellar:
                    {
                        CellarSettings c = _host.Brushes.Cellar;
                        int depth = Math.Max(5, _ground - _level);
                        var cs = new CellarSettings
                        {
                            Depth = depth, StepHeight = c.StepHeight, Direction = c.Direction, StairWidth = c.StairWidth, FloorIds = c.FloorIds,
                            UpperFloorIds = c.UpperFloorIds, WallSet = c.WallSet, Land = c.Land
                        };
                        WallSet walls = _host.Brushes.WallSets.FirstOrDefault(w => w.Name == c.WallSet);
                        StairSet stairs = _host.Brushes.StairSets.FirstOrDefault(w => w.Name == _host.Brushes.StairSet);
                        _work.ApplyStatics(Builders.BuildCellar(_work, _content, cs, walls, stairs, rnd));
                        what = L.T("Keller") + $" z {_ground - depth}" + (walls == null ? " – " + L.T("ohne Wand-Set keine Wände (Reiter Pinsel → Wand)") : string.Empty);
                        break;
                    }
                default:
                    {
                        DungeonSettings b = Copy(o);
                        b.Style = DungeonStyle.Cave;
                        b.FixedFloor = true;
                        b.FloorZ = _level;
                        _work.ApplyStatics(Builders.BuildDungeon(_work, _content, _plan, b, null, null, _host.CaveFloor?.Invoke(), _host.RockTiles?.Invoke(), rnd));
                        what = (_kind == WorkshopKind.MountainWall ? L.T("Bergwall") : L.T("Berg-Dungeon")) + $": {_plan.Rooms.Count} " + L.T("Räume");
                        break;
                    }
            }
            int n = _work.EndLand();
            _work.NameLastStep(what);
            Changed(what + $" · {n} " + L.T("Änderungen") + " · " + L.T("Zufalls-Zahl") + $" {seed}");
        }

        /// <summary>
        /// Stairs between the floors: in a room that is open on both floors, a flight of 4 steps down from the upper floor;
        /// the floor of the upper level above the steps becomes a hole (NoDraw land / floor statics away).
        /// </summary>
        private string ConnectFloors(List<DungeonPlan> plans, int fh)
        {
            StairSet set = _host.Brushes.StairSets.FirstOrDefault(s => s.Name == _host.Brushes.StairSet);
            if (set == null || HexList.Parse(set.DownWest).Length == 0)
            {
                return " – " + L.T("ohne Treppen (erst ein Treppen-Set wählen / lernen)");
            }
            int made = 0;
            var floorIds = new HashSet<ushort> { 0x053d, 0x053e };
            for (int f = 0; f < plans.Count - 1; f++)
            {
                DungeonPlan up = plans[f], down = plans[f + 1];
                Point? at = null;
                // 6 cells in a row along X (4 steps + landing + a free cell), open on both floors, with a free row beside
                for (int y = 1; y < up.H - 1 && at == null; y++)
                {
                    for (int x = 1; x < up.W - 7 && at == null; x++)
                    {
                        bool ok = true;
                        for (int k = -1; k < 6 && ok; k++)
                        {
                            ok = up.Floor(x + k, y) && down.Floor(x + k, y) && down.Floor(x + k, y - 1) && up.Floor(x + k, y - 1);
                        }
                        if (ok)
                        {
                            at = new Point(x, y);
                        }
                    }
                }
                if (at == null)
                {
                    continue;
                }
                int topZ = _level - f * fh;
                var p = new Point(_content.X + at.Value.X, _content.Y + at.Value.Y);
                var o = new StaircaseSettings { Up = Compass.East, Floors = 1, Steps = Math.Max(1, fh / 5), StepHeight = 5, Width = 1, Downwards = true, TopLanding = true, Fill = true };
                List<StaticChange> st = Staircase.Build(_work, p, topZ, o, set);
                // hole in the upper floor over the steps
                var holes = new List<StaticChange>();
                for (int k = 0; k < o.Steps; k++)
                {
                    int x = p.X + k, y = p.Y;
                    if (f == 0 && _level >= _ground)
                    {
                        _work.SetLand(x, y, 0x0002, _work.LandZ(x, y));
                    }
                    holes.AddRange(_work.StaticsAt(x, y).Where(s => s.Z == topZ && floorIds.Contains(s.Id)).ToList().Select(MapData.Remove));
                }
                _work.ApplyStatics(holes);
                _work.ApplyStatics(st);
                made++;
            }
            return $", {made} " + L.T("Treppen");
        }

        private static DungeonSettings Copy(DungeonSettings o) => JsonSerializer.Deserialize<DungeonSettings>(JsonSerializer.Serialize(o));

        private Rectangle Target => !_top.Area.IsEmpty && _top.Area != _content ? _top.Area : _content;

        private void ClearPlace(bool statics, bool land)
        {
            if (_work == null)
            {
                return;
            }
            (List<StaticChange> s, List<LandChange> l) = Workshop.Clear(_work, Target, statics, land, _void);
            _work.Apply(s, l);
            _work.NameLastStep(L.T("Leeren"));
            Changed(L.T("Geleert") + $": {s.Count} Statics, {l.Count} " + L.T("Land-Tiles") + (land ? $" → {_void:x4}" : string.Empty));
        }

        private void Move(int dx, int dy)
        {
            if (_work == null)
            {
                return;
            }
            (List<StaticChange> s, List<LandChange> l, Rectangle moved) = Workshop.Shift(_work, _content, dx, dy, _void);
            _work.Apply(s, l);
            _work.NameLastStep(L.T("Verschieben"));
            _content = moved;
            Changed(L.T("Verschoben") + $" ({dx:+0;-0;0}, {dy:+0;-0;0})");
        }

        private void Raise(int dz)
        {
            if (_work == null)
            {
                return;
            }
            (List<StaticChange> s, List<LandChange> l) = Workshop.Raise(_work, _content, dz, VoidSet);
            _work.Apply(s, l);
            _work.NameLastStep(L.T("Höhe") + $" {dz:+0;-0}");
            SetLevel(_level + dz);
            Changed(L.T("Höhe") + $" {dz:+0;-0} → z {_level}");
        }

        private void Undo(bool redo)
        {
            if (_work == null)
            {
                return;
            }
            bool ok = redo ? _work.Redo() : _work.Undo();
            Changed(ok ? (redo ? L.T("Wiederholt.") : L.T("Rückgängig.")) : L.T("Nichts mehr."));
        }

        /// <summary>Learns the style of a dungeon of the map (the area marked in the CGI view) and writes the log.</summary>
        private void Scan(bool copyHere)
        {
            Rectangle a = _host.CurrentArea?.Invoke() ?? Rectangle.Empty;
            if (a.IsEmpty)
            {
                Say(L.T("In der CGI-Ansicht des Studios einen Dungeon mit 'Bereich' markieren (mit etwas Rand), dann hier scannen."));
                return;
            }
            if (copyHere)
            {
                CopyIn(WorkshopPiece.From(_host.Map, a, "Scan", "Scan"), L.T("1:1 kopiert"));
                return;
            }
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name des Dungeon-Stils (z. B. Deceit, Doom):"), Text, L.T("Mein Dungeon"));
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            (DungeonLook look, string report) = DungeonStyleTools.Analyze(_host.Map, a, null, name.Trim(), $"{_host.Map.Source} ({a.X}, {a.Y}, {a.Width}×{a.Height})");
            _host.Brushes.DungeonLooks.RemoveAll(l => l.Name == look.Name);
            _host.Brushes.DungeonLooks.Add(look);
            FillStyles(look.Name);
            _log.Text = report.Replace("\n", Environment.NewLine);
            string path = _host.WriteLog?.Invoke("dungeon_analyse_" + string.Concat(look.Name.Select(c => char.IsLetterOrDigit(c) ? c : '_')) + ".txt", report);
            Say(L.T("Stil gelernt") + $" '{look.Name}' · " + L.T("Log") + $": {path}");
        }

        /// <summary>A piece (scan, other map, saved template) into the place: its ground lands on the chosen level.</summary>
        private void CopyIn(WorkshopPiece piece, string what)
        {
            if (_work == null || piece == null)
            {
                return;
            }
            _work.BeginLand();
            Rectangle r = piece.WriteTo(_work, _content.Location, _level);
            int n = _work.EndLand();
            _work.NameLastStep(what);
            _content = new Rectangle(_content.X, _content.Y, Math.Max(1, r.Width), Math.Max(1, r.Height));
            _plan = null;
            Changed(what + $": {piece.Width}×{piece.Height}, {piece.Statics.Count} Statics, {n} " + L.T("Änderungen") + " – " +
                    L.T("mit Verschieben / Höhe anpassen, Rückgängig nimmt es zurück") +
                    (piece.Width > _work.Width - _content.X || piece.Height > _work.Height - _content.Y ? " · " + L.T("größer als der Platz – abgeschnitten, einen größeren Platz markieren") : string.Empty));
        }

        private void OpenForeign()
        {
            if (_foreign != null)
            {
                _foreign.Activate();
                return;
            }
            _foreign = new ForeignMapForm(_host.Configure) { Owner = this };
            _foreign.Taken += (s, p) =>
            {
                CopyIn(p, L.T("Aus fremder Karte"));
                Activate();
            };
            _foreign.FormClosed += (s, e) => _foreign = null;
            _foreign.Show(this);
        }

        private void SavePiece()
        {
            if (_work == null)
            {
                return;
            }
            string cat = Microsoft.VisualBasic.Interaction.InputBox(L.T("Kategorie (Ordner), z. B. Dungeon, Keller, Räume:"), Text, _kind == WorkshopKind.Cellar ? "Keller" : "Dungeon");
            if (string.IsNullOrWhiteSpace(cat))
            {
                return;
            }
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name der Vorlage:"), Text, L.T("Mein Dungeon"));
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            WorkshopPiece p = WorkshopPiece.From(_work, Target, name.Trim(), cat.Trim());
            // stored relative to the chosen level, so it loads on any level the same way
            int d = p.Ground - _level;
            p.Ground = _level;
            for (int i = 0; i < p.LandZ.Length; i++)
            {
                p.LandZ[i] += d;
            }
            foreach (PieceStatic st in p.Statics)
            {
                st.Z += d;
            }
            string path = p.Save();
            FillPieces();
            Say(L.T("Vorlage gespeichert") + $": {path}");
        }

        private void FillPieces()
        {
            if (_pieces == null)
            {
                return;
            }
            _pieces.Items.Clear();
            foreach ((string path, string title) in WorkshopPiece.List())
            {
                _pieces.Items.Add(new KeyValuePair<string, string>(path, title));
            }
            _pieces.DisplayMember = "Value";
        }

        private void LoadPiece()
        {
            if (!(_pieces.SelectedItem is KeyValuePair<string, string> kv))
            {
                Say(L.T("Erst eine Vorlage in der Liste wählen."));
                return;
            }
            try
            {
                CopyIn(WorkshopPiece.Load(kv.Key), L.T("Vorlage") + $" '{kv.Value}'");
            }
            catch (Exception ex)
            {
                Say(L.T("Vorlage nicht lesbar") + ": " + ex.Message);
            }
        }

        private void FillStyles(string select)
        {
            _style.Items.Clear();
            _style.Items.Add(Workshop.Classic());
            foreach (DungeonLook l in _host.Brushes.DungeonLooks)
            {
                _style.Items.Add(l);
            }
            int i = 0;
            for (int k = 0; k < _style.Items.Count; k++)
            {
                if (((DungeonLook)_style.Items[k]).Name == select)
                {
                    i = k;
                }
            }
            _style.SelectedIndex = i;
        }

        private void ShowScript()
        {
            _script.Text = JsonSerializer.Serialize(ChosenLook(), new JsonSerializerOptions { WriteIndented = true });
        }

        private void ApplyScript(bool asNew)
        {
            try
            {
                DungeonLook look = JsonSerializer.Deserialize<DungeonLook>(_script.Text);
                if (look == null)
                {
                    return;
                }
                if (asNew)
                {
                    string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name des neuen Stils:"), Text, look.Name + " 2");
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        return;
                    }
                    look.Name = name.Trim();
                }
                _host.Brushes.DungeonLooks.RemoveAll(l => l.Name == look.Name);
                _host.Brushes.DungeonLooks.Add(look);
                FillStyles(look.Name);
                Say(L.T("Skript übernommen") + $": '{look.Name}' – " + L.T("'Aus Plan bauen' zeigt es"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, L.T("Das Skript ist kein gültiges JSON:") + Environment.NewLine + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyToMap()
        {
            if (_work == null)
            {
                return;
            }
            int n = _host.Map.WriteBack(_work, _origin);
            _host.Applied?.Invoke();
            _work.ClearHistory();
            Say(L.T("In die Karte übernommen") + $": {n} " + L.T("Änderungen – in der CGI-Ansicht mit Strg+Z zurück."));
        }

        // ================================================================== user interface

        private void BuildUi()
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 340 };
            var left = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(6), BackColor = Color.FromArgb(241, 245, 249) };
            split.Panel1.Controls.Add(left);

            // ---- right: views, plan, script, log
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var views = new TabPage(L.T("Ansicht: oben Karte · unten Etage"));
            var viewSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            viewSplit.Panel1.Controls.Add(_top);
            viewSplit.Panel1.Controls.Add(new Label { Dock = DockStyle.Top, Height = 18, Text = "  " + L.T("OBEN – die Karte, wie sie ist"), BackColor = Color.FromArgb(22, 128, 61), ForeColor = Color.White });
            viewSplit.Panel2.Controls.Add(_bottom);
            viewSplit.Panel2.Controls.Add(new Label { Dock = DockStyle.Top, Height = 18, Text = "  " + L.T("UNTEN – die gewählte Etage (Statics unter dem Boden sichtbar, darüber ausgeblendet)"), BackColor = Color.FromArgb(126, 34, 206), ForeColor = Color.White });
            var viewTools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            void ToolBtn(string text, IsoTool tool, Action extra = null)
            {
                var b = new ToolStripButton(L.T(text));
                b.Click += (s, e) =>
                {
                    _holeTool = false;
                    ConfigureViews();
                    _top.Tool = tool;
                    _bottom.Tool = tool;
                    extra?.Invoke();
                    (_active ?? _top).Focus();
                    Say(L.T(text) + " – " + L.T("in einer der beiden Ansichten benutzen"));
                };
                viewTools.Items.Add(b);
            }
            ToolBtn("Auswahl", IsoTool.Select);
            ToolBtn("Bereich", IsoTool.Area);
            ToolBtn("Static setzen", IsoTool.Place);
            ToolBtn("Löschen", IsoTool.Delete);
            ToolBtn("Wand / Zaun", IsoTool.Wall);
            ToolBtn("Loch (NoDraw)", IsoTool.LandSet, () =>
            {
                _holeTool = true;
                foreach (IsoMapView v in new[] { _top, _bottom })
                {
                    v.PlaceLandId = 0x0002;
                    v.PlaceLandIds = new ushort[] { 0x0002 };
                    v.LandFlatten = false;
                }
            });
            ToolBtn("Land setzen", IsoTool.LandSet);
            ToolBtn("Radierer", IsoTool.Eraser);
            ToolBtn("Treppe", IsoTool.Stairs);
            ToolBtn("Multi setzen", IsoTool.Multi);
            ToolBtn("Höhe +", IsoTool.Raise);
            ToolBtn("Höhe −", IsoTool.Lower);
            ToolBtn("Rampe", IsoTool.Ramp);
            ToolBtn("Glätten", IsoTool.Smooth);
            ToolBtn("Messen", IsoTool.Measure);
            viewTools.Items.Add(new ToolStripSeparator());
            viewTools.Items.Add(new ToolStripLabel(L.T("Pinsel:")));
            var brushSize = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 50, ToolTipText = L.T("Pinselgröße für Radierer, Land, Höhe, Loch (1 = ein Tile)") };
            brushSize.Items.AddRange(new object[] { "1", "3", "5", "7", "9", "13", "17" });
            brushSize.SelectedIndex = 0;
            brushSize.SelectedIndexChanged += (s, e) =>
            {
                _radius = new[] { 0, 1, 2, 3, 4, 6, 8 }[brushSize.SelectedIndex];
                _top.LandRadius = _radius;
                _bottom.LandRadius = _radius;
            };
            viewTools.Items.Add(brushSize);
            var stairOpt = new ToolStripButton(L.T("Treppe …"));
            stairOpt.Click += (s, e) =>
            {
                using var f = new Form { Text = L.T("Treppe"), Size = new Size(340, 260), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.SizableToolWindow };
                f.Controls.Add(new PropertyGrid { Dock = DockStyle.Fill, ToolbarVisible = false, SelectedObject = _host.Brushes.Staircase });
                f.ShowDialog(this);
                ConfigureViews();
            };
            viewTools.Items.Add(stairOpt);
            viewTools.Items.Add(new ToolStripSeparator());
            var gridBtn = new ToolStripButton(L.T("Gitter")) { CheckOnClick = true };
            gridBtn.Click += (s, e) =>
            {
                _grid = gridBtn.Checked;
                ConfigureViews();
                _top.Refresh(true);
                _bottom.Refresh(true);
            };
            var staticsBtn = new ToolStripButton(L.T("Statics")) { CheckOnClick = true, Checked = true };
            staticsBtn.Click += (s, e) =>
            {
                _statics = staticsBtn.Checked;
                ConfigureViews();
                _top.Refresh(true);
                _bottom.Refresh(true);
            };
            viewTools.Items.Add(gridBtn);
            viewTools.Items.Add(staticsBtn);
            var zoomIn = new ToolStripButton("Zoom +");
            zoomIn.Click += (s, e) =>
            {
                _top.ZoomStep(1);
                _bottom.ZoomStep(1);
            };
            var zoomOut = new ToolStripButton("Zoom −");
            zoomOut.Click += (s, e) =>
            {
                _top.ZoomStep(-1);
                _bottom.ZoomStep(-1);
            };
            var twoD = new ToolStripButton("2D") { CheckOnClick = true };
            twoD.Click += (s, e) =>
            {
                _top.TopDown = twoD.Checked;
                _bottom.TopDown = twoD.Checked;
            };
            var catBtn = new ToolStripButton(L.T("Katalog")) { CheckOnClick = true, Checked = true, ToolTipText = L.T("Statics / Land / Multis mit Suche und Kategorien unten ein- / ausblenden") };
            viewTools.Items.AddRange(new ToolStripItem[] { zoomIn, zoomOut, twoD, catBtn });
            var catSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 520, FixedPanel = FixedPanel.Panel2 };
            catSplit.Panel1.Controls.Add(viewSplit);
            catSplit.Panel2.Controls.Add(_catalog);
            catSplit.Panel2.Controls.Add(new Label
            {
                Dock = DockStyle.Top, Height = 18, Text = "  " + L.T("KATALOG – Klick = Teil wählen (Static setzen / Land setzen / Multi setzen) · Rechtsklick = eigene Kategorien"),
                BackColor = Color.FromArgb(180, 83, 9), ForeColor = Color.White
            });
            catBtn.Click += (s, e) => catSplit.Panel2Collapsed = !catBtn.Checked;
            _catalog.Picked += (s, e) => PickFromCatalog(e);
            views.Controls.Add(catSplit);
            views.Controls.Add(viewTools);
            foreach (IsoMapView v in new[] { _top, _bottom })
            {
                IsoMapView other = v == _top ? _bottom : _top;
                v.Enter += (s, e) => _active = v;
                v.MapChanged += (s, e) => other.Refresh(true);
                v.Message += (s, t) => Say(t);
            }
            var plan = new TabPage(L.T("2D-Plan (nachzeichnen)"));
            var planTools = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4) };
            Button PB(string text, Action a)
            {
                var b = new Button { Text = L.T(text), AutoSize = true, Margin = new Padding(3) };
                b.Click += (s, e) => a();
                return b;
            }
            planTools.Controls.Add(PB("Leerer Plan", () =>
            {
                _plan = new DungeonPlan { W = _content.Width, H = _content.Height, Cells = new byte[_content.Width * _content.Height] };
                _planView.Plan = _plan;
            }));
            planTools.Controls.Add(PB("Neu generieren", () => Generate(true)));
            planTools.Controls.Add(PB("Aus Plan bauen", () => Generate(false)));
            planTools.Controls.Add(new Label { Text = L.T("Pinsel:"), AutoSize = true, Margin = new Padding(12, 9, 2, 0) });
            var brush = new NumericUpDown { Minimum = 1, Maximum = 3, Value = 1, Width = 45 };
            brush.ValueChanged += (s, e) => _planView.BrushSize = (int)brush.Value;
            planTools.Controls.Add(brush);
            plan.Controls.Add(_planView);
            plan.Controls.Add(planTools);
            _planView.PlanEdited += (s, e) => Say(L.T("Plan geändert – 'Aus Plan bauen' baut ihn im Stil."));
            var script = new TabPage(L.T("Skript / Stil"));
            var scriptTools = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4) };
            scriptTools.Controls.Add(PB("Stil anzeigen", ShowScript));
            scriptTools.Controls.Add(PB("Skript übernehmen", () => ApplyScript(false)));
            scriptTools.Controls.Add(PB("Als neuen Stil speichern", () => ApplyScript(true)));
            script.Controls.Add(_script);
            script.Controls.Add(scriptTools);
            script.Controls.Add(new Label
            {
                Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(4), ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Der Stil als Skript (JSON): Void- und Boden-Land, Boden-Statics, Deko, und für jede Lage (Boden / Leere, Nachbar-Maske N1 O2 S4 W8, Ecken 16–128, Innenwände 256 / 512 / 768) die Wand-Stapel 'id@höhe'. Anpassen, übernehmen, neu bauen.")
            });
            var log = new TabPage(L.T("Log"));
            log.Controls.Add(_log);
            tabs.TabPages.AddRange(new[] { views, plan, script, log });
            split.Panel2.Controls.Add(tabs);

            // ---- left: settings and buttons
            Control Head(string text, Color c) => new Label
            {
                Text = L.T(text), AutoSize = false, Width = 310, Height = 22, Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White, BackColor = c, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 8, 0, 2)
            };
            FlowLayoutPanel Line() => new FlowLayoutPanel { Width = 310, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = new Padding(0) };
            Button B(string text, Action a, int width = 0)
            {
                var b = new Button { Text = L.T(text), AutoSize = width == 0, Width = width == 0 ? 75 : width, Height = 26, Margin = new Padding(2), FlatStyle = FlatStyle.System };
                b.Click += (s, e) => a();
                return b;
            }

            left.Controls.Add(Head("1 Art", Color.FromArgb(68, 64, 60)));
            var kinds = Line();
            foreach ((WorkshopKind k, string text) in new[]
                     {
                         (WorkshopKind.Classic, "Klassischer Dungeon"), (WorkshopKind.Cellar, "Keller"),
                         (WorkshopKind.MountainDungeon, "Berg-Dungeon"), (WorkshopKind.MountainWall, "Bergwall")
                     })
            {
                var r = new RadioButton { Text = L.T(text), AutoSize = true, Checked = k == _kind, Margin = new Padding(3) };
                WorkshopKind kk = k;
                r.CheckedChanged += (s, e) =>
                {
                    if (r.Checked)
                    {
                        _kind = kk;
                        _plan = null;
                        if (kk == WorkshopKind.Cellar && _level >= _ground)
                        {
                            SetLevel(_ground - 20);
                        }
                    }
                };
                _kindButtons[k] = r;
                kinds.Controls.Add(r);
            }
            left.Controls.Add(kinds);

            left.Controls.Add(Head("2 Platz und Etage", Color.FromArgb(37, 99, 235)));
            _levelText = new Label { AutoSize = false, Width = 310, Height = 20 };
            left.Controls.Add(_levelText);
            _levelBar = new TrackBar { Minimum = -4, Maximum = 4, TickFrequency = 1, Width = 300, Value = 0 };
            _levelBar.ValueChanged += (s, e) =>
            {
                if (!_syncing)
                {
                    SetLevel(_ground + _levelBar.Value * 20);
                }
            };
            left.Controls.Add(_levelBar);
            var lv = Line();
            lv.Controls.Add(new Label { Text = L.T("z genau:"), AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
            _levelNum = new NumericUpDown { Minimum = -128, Maximum = 127, Width = 60 };
            _levelNum.ValueChanged += (s, e) =>
            {
                if (!_syncing)
                {
                    SetLevel((int)_levelNum.Value);
                }
            };
            lv.Controls.Add(_levelNum);
            lv.Controls.Add(B("Platz neu aus CGI", () => LoadPlace(_host.CurrentArea?.Invoke() ?? Rectangle.Empty)));
            left.Controls.Add(lv);

            left.Controls.Add(Head("3 Fläche außen (Leere)", Color.FromArgb(30, 41, 59)));
            var vd = Line();
            _voidPic = new PictureBox { Width = 44, Height = 44, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black, Margin = new Padding(2) };
            _voidBox = new TextBox { Width = 60, Text = "0002", Margin = new Padding(2, 12, 2, 2) };
            _voidBox.TextChanged += (s, e) =>
            {
                ushort[] v = HexList.Parse(_voidBox.Text);
                if (v.Length > 0)
                {
                    _void = v[0];
                    try
                    {
                        _voidPic.Image = UoArtCache.Land(_void);
                    }
                    catch (Exception)
                    {
                        _voidPic.Image = null;
                    }
                }
            };
            vd.Controls.Add(_voidPic);
            vd.Controls.Add(_voidBox);
            vd.Controls.Add(B("NoDraw", () => _voidBox.Text = "0002"));
            vd.Controls.Add(B("Schwarz", () => _voidBox.Text = "0244"));
            vd.Controls.Add(B("Wählen …", () =>
            {
                using var pick = new TilePickerForm(_host.LandList?.Invoke() ?? new List<int>(), new List<int>(), TileCatalogMode.Land, _void);
                if (pick.ShowDialog(this) == DialogResult.OK && pick.SelectedId >= 0)
                {
                    _voidBox.Text = pick.SelectedId.ToString("x4");
                }
            }));
            left.Controls.Add(vd);
            left.Controls.Add(new Label { Text = L.T("NoDraw = nichts (unsichtbar), Schwarz = 0244, 'Wählen' = z. B. Schwarz mit Sternen aus dem Land-Katalog."), AutoSize = false, Width = 310, Height = 34, ForeColor = Color.FromArgb(71, 85, 105) });

            left.Controls.Add(Head("4 Stil (klassischer Dungeon)", Color.FromArgb(126, 34, 206)));
            _style = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
            left.Controls.Add(_style);
            FillStyles(Settings.Look);
            _style.SelectedIndexChanged += (s, e) =>
            {
                if (_style.SelectedItem is DungeonLook l)
                {
                    Settings.Look = l.Name;
                }
            };
            var sc = Line();
            sc.Controls.Add(B("Bereich scannen + Stil lernen", () => Scan(false), 300));
            sc.Controls.Add(B("Gescannten Bereich 1:1 hierher kopieren", () => Scan(true), 300));
            left.Controls.Add(sc);

            left.Controls.Add(Head("4b Vorlagen und fremde Karten", Color.FromArgb(146, 64, 14)));
            var pc = Line();
            pc.Controls.Add(B("Fremde Karte öffnen …", OpenForeign, 300));
            _pieces = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
            pc.Controls.Add(_pieces);
            pc.Controls.Add(B("Vorlage laden", LoadPiece, 148));
            pc.Controls.Add(B("Platz als Vorlage speichern …", SavePiece, 148));
            pc.Controls.Add(B("🖼 Vorlagen mit Bild …", () =>
            {
                using var tb = new TemplateBrowserForm(false);
                if (tb.ShowDialog(this) == DialogResult.OK && tb.Selected is string path)
                {
                    try
                    {
                        CopyIn(WorkshopPiece.Load(path), L.T("Vorlage") + " " + System.IO.Path.GetFileNameWithoutExtension(path));
                    }
                    catch (Exception ex)
                    {
                        Say(L.T("Vorlage nicht lesbar") + ": " + ex.Message);
                    }
                }
            }, 300));
            left.Controls.Add(pc);
            FillPieces();
            left.Controls.Add(new Label { Text = L.T("Fremde Karte: z. B. die Original-map0 öffnen, einen Dungeon markieren und hierher kopieren – dann ändern und als Vorlage (nach Kategorie) speichern oder in die Karte übernehmen."), AutoSize = false, Width = 310, Height = 48, ForeColor = Color.FromArgb(71, 85, 105) });

            left.Controls.Add(Head("5 Grundriss", Color.FromArgb(180, 83, 9)));
            var grid = new PropertyGrid { Width = 310, Height = 230, ToolbarVisible = false, HelpVisible = true, PropertySort = PropertySort.Categorized, SelectedObject = Settings };
            left.Controls.Add(grid);
            var cellar = new PropertyGrid { Width = 310, Height = 170, ToolbarVisible = false, HelpVisible = true, PropertySort = PropertySort.Categorized, SelectedObject = _host.Brushes.Cellar };
            left.Controls.Add(new Label { Text = L.T("Keller:"), AutoSize = true, Margin = new Padding(0, 4, 0, 0) });
            left.Controls.Add(cellar);

            left.Controls.Add(Head("6 Bauen", Color.FromArgb(22, 128, 61)));
            var gen = Line();
            gen.Controls.Add(B("▶ Generieren", () => Generate(true), 148));
            gen.Controls.Add(B("🎲 Neue Zufalls-Zahl", () =>
            {
                Settings.Seed = new Random().Next(1, 999999);
                grid.Refresh();
                Generate(true);
            }, 148));
            gen.Controls.Add(B("↶ Rückgängig", () => Undo(false), 98));
            gen.Controls.Add(B("↷ Wiederholen", () => Undo(true), 98));
            gen.Controls.Add(B("Alles zurück", () => LoadPlace(new Rectangle(_origin.X + _content.X, _origin.Y + _content.Y, _content.Width, _content.Height)), 98));
            left.Controls.Add(gen);

            left.Controls.Add(Head("7 Verschieben und Höhe", Color.FromArgb(8, 145, 178)));
            var mv = Line();
            mv.Controls.Add(B("↖ N", () => Move(0, -1), 56));
            mv.Controls.Add(B("↘ S", () => Move(0, 1), 56));
            mv.Controls.Add(B("↙ W", () => Move(-1, 0), 56));
            mv.Controls.Add(B("↗ O", () => Move(1, 0), 56));
            mv.Controls.Add(B("z +1", () => Raise(1), 56));
            mv.Controls.Add(B("z −1", () => Raise(-1), 56));
            mv.Controls.Add(B("z +5", () => Raise(5), 56));
            mv.Controls.Add(B("z −5", () => Raise(-5), 56));
            mv.Controls.Add(B("+ Etage", () => Raise(20), 56));
            mv.Controls.Add(B("− Etage", () => Raise(-20), 56));
            left.Controls.Add(mv);
            left.Controls.Add(new Label { Text = L.T("N / S / W / O = so wie die Karte im Client (N = nach oben rechts). Verschiebt den ganzen Bau mit Land und Statics."), AutoSize = false, Width = 310, Height = 34, ForeColor = Color.FromArgb(71, 85, 105) });

            left.Controls.Add(Head("8 Schnell löschen", Color.FromArgb(185, 28, 28)));
            var del = Line();
            del.Controls.Add(B("Statics", () => ClearPlace(true, false), 98));
            del.Controls.Add(B("Land / Textur", () => ClearPlace(false, true), 98));
            del.Controls.Add(B("Beides", () => ClearPlace(true, true), 98));
            left.Controls.Add(del);
            left.Controls.Add(new Label { Text = L.T("Gilt für den Platz – oder für einen kleineren Bereich, der oben mit 'Bereich' markiert ist. Land wird zur Leere oben, auf seiner Höhe."), AutoSize = false, Width = 310, Height = 34, ForeColor = Color.FromArgb(71, 85, 105) });

            left.Controls.Add(Head("Hilfe", Color.FromArgb(37, 99, 235)));
            var hp = Line();
            hp.Controls.Add(B("Generieren", () => _host.Help?.Invoke("ws-generate"), 98));
            hp.Controls.Add(B("2D-Plan", () => _host.Help?.Invoke("ws-2d"), 98));
            hp.Controls.Add(B("3D bauen", () => _host.Help?.Invoke("ws-3d"), 98));
            left.Controls.Add(hp);

            left.Controls.Add(Head("9 Fertig", Color.FromArgb(15, 23, 42)));
            var done = Line();
            done.Controls.Add(B("✔ In die Karte übernehmen", ApplyToMap, 200));
            done.Controls.Add(B("Schließen", Close, 98));
            left.Controls.Add(done);

            Controls.Add(split);
            Controls.Add(_status);
        }

        private void PickFromCatalog(ArtPickedEventArgs e)
        {
            _holeTool = false;
            IsoTool tool;
            switch (e.Kind)
            {
                case ArtKind.Land:
                    _landId = e.Id;
                    tool = IsoTool.LandSet;
                    break;
                case ArtKind.Multi:
                    _multiId = e.Id;
                    tool = IsoTool.Multi;
                    break;
                default:
                    _placeId = e.Id;
                    tool = IsoTool.Place;
                    break;
            }
            ConfigureViews();
            _top.Tool = tool;
            _bottom.Tool = tool;
            Say(L.T(tool == IsoTool.Place ? "Static setzen" : tool == IsoTool.LandSet ? "Land setzen" : "Multi setzen") + $": {e.Id:x4} " +
                (e.Kind == ArtKind.Static ? UoArtCache.ItemName(e.Id) : e.Kind == ArtKind.Land ? UoArtCache.LandName(e.Id) : string.Empty) +
                " – " + L.T("oben auf der Karte oder unten auf der Etage klicken"));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Control && e.KeyCode == Keys.Z && !(ActiveControl is TextBox))
            {
                Undo(false);
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Y && !(ActiveControl is TextBox))
            {
                Undo(true);
                e.Handled = true;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (_work != null && _work.CanUndo &&
                MessageBox.Show(this, L.T("Die Werkstatt hat Änderungen, die noch nicht in der Karte sind. Trotzdem schließen?"), Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }
    }
}
