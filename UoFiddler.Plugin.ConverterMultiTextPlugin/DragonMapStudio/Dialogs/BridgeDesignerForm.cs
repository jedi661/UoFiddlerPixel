/***************************************************************************
 *
 *  * Dragon Map Studio – bridge designer: an own window to design bridges of statics
 *  *
 *  *  A test field (two banks and water) shows the bridge like the client. The plan is built from pieces
 *  *  (found in the tiledata: planks, railings, posts, pillars, lamps, stairs – shown as pictures), can be
 *  *  edited by hand in the test field (place / delete / eraser / multi), taken from the map or a multi,
 *  *  and saved by category. "Im Studio benutzen" gives the plan to the bridge tool of the CGI view.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class BridgeDesignerHost
    {
        public MapData Map;
        public CgiBrushSettings Brushes;
        public Action<IsoMapView> Configure;
        public Func<Rectangle> CurrentArea;
        public Action<BridgeTemplate> Use;
        public Action<string> Status;
        public Action<string> Help;
    }

    public sealed class BridgeDesignerForm : Form
    {
        private const int MapW = 112;
        private const int MapH = 112;
        private const int Bank = 0;
        private const int WaterZ = -5;

        private readonly BridgeDesignerHost _host;
        private BridgeTemplate _plan = new BridgeTemplate { Name = "Meine Brücke", Category = "Holz" };
        private readonly BridgeGenSettings _gen = new BridgeGenSettings();
        private readonly BridgeSettings _build = new BridgeSettings { Width = 3, Arch = 4 };
        private MapData _test;
        private bool _alongX = true;
        private bool _sample = true;
        private int _length = 24;
        private Rectangle _sampleRect;
        private int _sampleZ;
        private string _slot;

        private readonly IsoMapView _view = new IsoMapView { Dock = DockStyle.Fill };
        private readonly ArtCatalog _catalog = new ArtCatalog();
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
        private readonly Label _slotLabel = new Label { AutoSize = false, Width = 310, Height = 34, ForeColor = Color.FromArgb(180, 83, 9), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        private ComboBox _plans;
        private PropertyGrid _planGrid;
        private PropertyGrid _genGrid;
        private readonly FlowLayoutPanel _pieces = new FlowLayoutPanel { Width = 310, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true };

        public BridgeDesignerForm(BridgeDesignerHost host)
        {
            _host = host;
            Text = L.T("Brücken-Designer") + " – Dragon Map Studio";
            Size = new Size(1400, 900);
            MinimumSize = new Size(1000, 640);
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;
            Font = new Font("Segoe UI", 9f);
            BuildUi();
            BridgeGenSettings auto = BridgeKit.AutoPieces();
            CopyGen(auto, _gen);
            StairSet st = BridgeKit.AutoStairs();
            _plan.StairNorth = st.DownNorth;
            _plan.StairEast = st.DownEast;
            _plan.StairSouth = st.DownSouth;
            _plan.StairWest = st.DownWest;
            _genGrid.Refresh();
            FillPlans(null);
            FromPieces();
            Say(string.IsNullOrEmpty(_gen.Deck)
                ? L.T("Keine Bretter in der TileData gefunden – Teile unten im Katalog suchen und mit den Knöpfen links zuordnen.")
                : L.T("Teile aus der TileData gefunden") + $": " + L.T("Bretter") + $" {_gen.Deck}, " + L.T("Geländer") + $" {_gen.RailX} / {_gen.RailY}, " + L.T("Pfosten") + $" {_gen.Post}, " + L.T("Pfeiler") + $" {_gen.Pillar}. " +
                  L.T("Falsche Teile? Links 'Teil wählen' und im Katalog doppelklicken."));
        }

        // ================================================================== test field

        private void NewTestField()
        {
            var tiles = new ushort[MapW * MapH];
            var z = new sbyte[MapW * MapH];
            for (int y = 0; y < MapH; y++)
            {
                for (int x = 0; x < MapW; x++)
                {
                    int across = _alongX ? x : y;
                    int size = _alongX ? MapW : MapH;
                    int half = Math.Min(size / 2 - 4, Math.Max(4, (_sample ? _plan.Length : _length) / 2 - 1));
                    bool water = Math.Abs(across - size / 2) < half;
                    tiles[y * MapW + x] = water ? (ushort)(0x00A8 + (x + y) % 4) : (ushort)(0x0003 + (x * 7 + y * 3) % 4);
                    z[y * MapW + x] = (sbyte)(water ? WaterZ : Bank);
                }
            }
            _test = new MapData(MapW, MapH, tiles, z) { Source = L.T("Brücken-Testfeld") };
            _view.SetMap(_test);
        }

        /// <summary>Builds the plan in the test field: the sample (flat, editable) or a long preview with arch, pillars, stairs.</summary>
        private void ShowPlan()
        {
            NewTestField();
            int len = _sample ? Math.Max(1, _plan.Length) : Math.Max(2, _length);
            int w = Math.Max(1, _plan.Width);
            int along0 = (_alongX ? MapW : MapH) / 2 - len / 2;
            int mid = (_alongX ? MapH : MapW) / 2;
            Point a = _alongX ? new Point(along0, mid) : new Point(mid, along0);
            Point b = _alongX ? new Point(along0 + len - 1, mid) : new Point(mid, along0 + len - 1);
            var o = new BridgeSettings
            {
                Width = w, Arch = _sample ? 0 : _build.Arch, ZOffset = _build.ZOffset, Railings = true, Stairs = !_sample && _build.Stairs, ArchStairs = _build.ArchStairs
            };
            BridgeTemplate t = _plan;
            if (_sample)
            {
                // the sample exactly once: start + middle + end without repeating
                t = Clone(_plan);
                t.PillarEvery = Math.Max(2, t.PillarEvery);
            }
            List<StaticChange> parts = BridgeKit.Build(_test, a, b, t, o, new Random(1));
            _test.Apply(parts);
            _test.ClearHistory();
            int lo = -(w - 1) / 2;
            _sampleRect = _alongX ? new Rectangle(a.X, a.Y + lo, len, w) : new Rectangle(a.X + lo, a.Y, w, len);
            _sampleZ = Bank + o.ZOffset;
            _view.SetArea(_sampleRect);
            _view.CenterOn(_sampleRect.X + _sampleRect.Width / 2.0, _sampleRect.Y + _sampleRect.Height / 2.0);
            _view.Refresh(true);
            Say((_sample ? L.T("Muster") : L.T("Vorschau")) + $" '{_plan.Name}': {parts.Count} Statics, {len} " + L.T("Tiles lang") + $", {w} " + L.T("breit") + " · " +
                (_alongX ? L.T("West-Ost") : L.T("Nord-Süd")) +
                (_sample ? " – " + L.T("im Muster von Hand setzen / löschen, dann 'Muster übernehmen'") : string.Empty));
        }

        private static BridgeTemplate Clone(BridgeTemplate t) =>
            System.Text.Json.JsonSerializer.Deserialize<BridgeTemplate>(System.Text.Json.JsonSerializer.Serialize(t));

        private static void CopyGen(BridgeGenSettings from, BridgeGenSettings to)
        {
            to.Deck = from.Deck;
            to.DeckY = from.DeckY;
            to.RailX = from.RailX;
            to.RailY = from.RailY;
            to.Post = from.Post;
            to.Pillar = from.Pillar;
            if (!string.IsNullOrEmpty(from.Lamp))
            {
                to.Lamp = from.Lamp;
            }
        }

        // ================================================================== plan actions

        private void FromPieces()
        {
            BridgeTemplate t = BridgeKit.Generate(_gen, _plan.Name);
            t.Category = _plan.Category;
            t.StairNorth = _plan.StairNorth;
            t.StairEast = _plan.StairEast;
            t.StairSouth = _plan.StairSouth;
            t.StairWest = _plan.StairWest;
            t.Note = _plan.Note;
            _plan = t;
            _planGrid.SelectedObject = _plan;
            ShowPlan();
        }

        /// <summary>The (hand edited) sample of the test field becomes the plan of this direction.</summary>
        private void TakeSample()
        {
            if (!_sample)
            {
                Say(L.T("Erst 'Muster' zeigen (nicht die lange Vorschau), dort bauen, dann übernehmen."));
                return;
            }
            BridgeTemplate c = BridgeKit.Capture(_test, _sampleRect, _alongX, _sampleZ, _plan.Name);
            // pillars are built from the plan (down to the ground) – not as pieces of the sample
            var pillar = new HashSet<ushort>(HexList.Parse(_plan.Pillar));
            c.PartsX.RemoveAll(p => pillar.Contains(p.Id) && p.Dz < 0);
            c.PartsY.RemoveAll(p => pillar.Contains(p.Id) && p.Dz < 0);
            if (_alongX)
            {
                _plan.PartsX = c.PartsX;
            }
            else
            {
                _plan.PartsY = c.PartsY;
            }
            _plan.Length = c.Length;
            _plan.Width = c.Width;
            _planGrid.Refresh();
            Say(L.T("Muster übernommen") + $": {(_alongX ? _plan.PartsX.Count : _plan.PartsY.Count)} " + L.T("Teile") + " (" + (_alongX ? L.T("West-Ost") : L.T("Nord-Süd")) + ")");
            ShowPlan();
        }

        /// <summary>A real bridge of the map (area marked in the CGI view) becomes the plan.</summary>
        private void FromMap()
        {
            Rectangle r = _host.CurrentArea?.Invoke() ?? Rectangle.Empty;
            if (r.IsEmpty || _host.Map == null)
            {
                Say(L.T("In der CGI-Ansicht eine Brücke der Karte mit 'Bereich' markieren (genau Länge × Breite), dann hier übernehmen."));
                return;
            }
            bool alongX = r.Width >= r.Height;
            // deck height: the most common height of the surface pieces
            List<int> zs = new List<int>();
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    zs.AddRange(_host.Map.StaticsAt(x, y).Where(s => UoArtCache.ItemHas(s.Id, Ultima.TileFlag.Surface)).Select(s => s.Z));
                }
            }
            int deck = zs.Count > 0 ? zs.GroupBy(v => v).OrderByDescending(g => g.Count()).First().Key : _host.Map.LandZ(r.X, r.Y);
            BridgeTemplate c = BridgeKit.Capture(_host.Map, r, alongX, deck, _plan.Name);
            if (c.PartsX.Count + c.PartsY.Count == 0)
            {
                Say(L.T("Im markierten Bereich sind keine Statics."));
                return;
            }
            c.Category = _plan.Category;
            c.Pillar = _plan.Pillar;
            c.StairNorth = _plan.StairNorth;
            c.StairEast = _plan.StairEast;
            c.StairSouth = _plan.StairSouth;
            c.StairWest = _plan.StairWest;
            c.Note = $"Aus der Karte ({r.X}, {r.Y}, {r.Width}×{r.Height})";
            _plan = c;
            _planGrid.SelectedObject = _plan;
            _alongX = alongX;
            _sample = true;
            ShowPlan();
            Say(L.T("Brücke aus der Karte übernommen") + $": {c.PartsX.Count + c.PartsY.Count} " + L.T("Teile") + $", " + L.T("Brett-Höhe") + $" z {deck}. " +
                L.T("Anfang / Ende im Plan einstellen – die Mitte wird wiederholt."));
        }

        /// <summary>A multi (catalog) becomes the plan.</summary>
        private void FromMulti(int id)
        {
            List<MultiPart> parts = UoArtCache.Multi(id);
            if (parts.Count == 0)
            {
                return;
            }
            int minX = parts.Min(p => p.Dx), minY = parts.Min(p => p.Dy), maxX = parts.Max(p => p.Dx), maxY = parts.Max(p => p.Dy);
            bool alongX = maxX - minX >= maxY - minY;
            int deck = parts.Where(p => UoArtCache.ItemHas(p.Id, Ultima.TileFlag.Surface)).Select(p => p.Dz).DefaultIfEmpty(0).GroupBy(v => v).OrderByDescending(g => g.Count()).First().Key;
            var t = new BridgeTemplate
            {
                Name = $"Multi {id:x4}", Category = _plan.Category, Width = alongX ? maxY - minY + 1 : maxX - minX + 1, Length = alongX ? maxX - minX + 1 : maxY - minY + 1,
                StartLen = 1, EndLen = 1, Note = $"Aus Multi {id:x4}"
            };
            List<BridgePart> list = alongX ? t.PartsX : t.PartsY;
            foreach (MultiPart p in parts.Where(p => p.Visible))
            {
                list.Add(new BridgePart { Along = alongX ? p.Dx - minX : p.Dy - minY, Across = alongX ? p.Dy - minY : p.Dx - minX, Dz = p.Dz - deck, Id = p.Id });
            }
            _plan = t;
            _planGrid.SelectedObject = _plan;
            _alongX = alongX;
            _sample = true;
            ShowPlan();
            Say(L.T("Multi als Brücken-Plan geladen") + $": {list.Count} " + L.T("Teile"));
        }

        private void TurnPlan()
        {
            if (_plan.PartsX.Count > 0)
            {
                _plan.PartsY = BridgeKit.Turn(_plan.PartsX);
                Say(L.T("Nord-Süd-Version aus West-Ost erzeugt (gleicher Name, Bild in die andere Richtung). Mit 'N-S' ansehen und von Hand verbessern."));
            }
            else if (_plan.PartsY.Count > 0)
            {
                _plan.PartsX = BridgeKit.Turn(_plan.PartsY);
                Say(L.T("West-Ost-Version aus Nord-Süd erzeugt."));
            }
            _alongX = !_alongX;
            ShowPlan();
        }

        private void FillPlans(string select)
        {
            _plans.Items.Clear();
            foreach (BridgeTemplate t in BridgeKit.LoadAll())
            {
                _plans.Items.Add(t);
            }
            for (int i = 0; i < _plans.Items.Count; i++)
            {
                if (_plans.Items[i].ToString() == select)
                {
                    _plans.SelectedIndex = i;
                }
            }
        }

        private void SavePlan()
        {
            string cat = Microsoft.VisualBasic.Interaction.InputBox(L.T("Kategorie (Ordner), z. B. Holz, Stein, Dungeon:"), Text, _plan.Category);
            if (string.IsNullOrWhiteSpace(cat))
            {
                return;
            }
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name des Brücken-Plans:"), Text, _plan.Name);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            _plan.Category = cat.Trim();
            _plan.Name = name.Trim();
            string path = BridgeKit.Save(_plan);
            FillPlans(_plan.ToString());
            _planGrid.Refresh();
            Say(L.T("Gespeichert") + $": {path}");
        }

        private void UseInStudio()
        {
            string path = BridgeKit.Save(_plan);
            FillPlans(_plan.ToString());
            _host.Brushes.BridgeTemplate = _plan.ToString();
            _host.Brushes.Bridge.Width = _plan.Width;
            _host.Brushes.Bridge.Arch = _build.Arch;
            _host.Brushes.Bridge.ZOffset = _build.ZOffset;
            _host.Brushes.Bridge.Stairs = _build.Stairs;
            _host.Brushes.Bridge.ArchStairs = _build.ArchStairs;
            _host.Use?.Invoke(_plan);
            Say(L.T("Plan ist jetzt im Brücken-Werkzeug der CGI-Ansicht") + $" ('{_plan}', {path}) – " + L.T("dort von Ufer zu Ufer ziehen."));
        }

        // ================================================================== pieces (slots)

        private void SetSlot(string slot, BridgeKit.Role role)
        {
            _slot = slot;
            _slotLabel.Text = L.T("Teil wählen für") + $": {L.T(slot)} – " + L.T("im Katalog doppelklicken");
            _catalog.ShowIds(ArtKind.Static, BridgeKit.Find(role, 400), L.T(BridgeKit.RoleName(role)) + " (TileData)");
        }

        private void AssignSlot(int id)
        {
            string hex = id.ToString("x4");
            switch (_slot)
            {
                case "Bretter":
                    _gen.Deck = hex;
                    break;
                case "Bretter N-S":
                    _gen.DeckY = hex;
                    break;
                case "Geländer W-O":
                    _gen.RailX = hex;
                    break;
                case "Geländer N-S":
                    _gen.RailY = hex;
                    break;
                case "Pfosten":
                    _gen.Post = hex;
                    break;
                case "Pfeiler":
                    _gen.Pillar = hex;
                    _plan.Pillar = hex;
                    break;
                case "Lampe":
                    _gen.Lamp = hex;
                    break;
                case "Treppe Nord":
                    _plan.StairNorth = hex;
                    break;
                case "Treppe Ost":
                    _plan.StairEast = hex;
                    break;
                case "Treppe Süd":
                    _plan.StairSouth = hex;
                    break;
                case "Treppe West":
                    _plan.StairWest = hex;
                    break;
                default:
                    return;
            }
            _genGrid.Refresh();
            _planGrid.Refresh();
            Say(L.T(_slot) + $" = {hex} {UoArtCache.ItemName(id)} – " + (_slot.StartsWith("Treppe") ? L.T("in der Vorschau zu sehen") : L.T("'Aus Teilen erzeugen' baut neu")));
            if (!_slot.StartsWith("Treppe"))
            {
                FromPieces();
            }
            else
            {
                ShowPlan();
            }
        }

        // ================================================================== user interface

        private void Say(string text)
        {
            _status.Text = "  " + text;
            _host.Status?.Invoke(text);
        }

        private void BuildUi()
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 345 };
            var left = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(6), BackColor = Color.FromArgb(241, 245, 249) };
            split.Panel1.Controls.Add(left);

            // ---- right: test field (top) + catalog (bottom)
            var right = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 560 };
            var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            void Tool(string text, IsoTool t)
            {
                var b = new ToolStripButton(L.T(text));
                b.Click += (s, e) =>
                {
                    _view.Tool = t;
                    _view.Focus();
                    Say(L.T(text) + " – " + L.T("im Testfeld"));
                };
                tools.Items.Add(b);
            }
            Tool("Auswahl", IsoTool.Select);
            Tool("Static setzen", IsoTool.Place);
            Tool("Multi setzen", IsoTool.Multi);
            Tool("Löschen", IsoTool.Delete);
            Tool("Radierer", IsoTool.Eraser);
            tools.Items.Add(new ToolStripSeparator());
            var grid = new ToolStripButton(L.T("Gitter")) { CheckOnClick = true };
            grid.Click += (s, e) =>
            {
                _view.ShowGrid = grid.Checked;
                _view.Refresh(true);
            };
            var statics = new ToolStripButton(L.T("Statics")) { CheckOnClick = true, Checked = true };
            statics.Click += (s, e) =>
            {
                _view.ShowStatics = statics.Checked;
                _view.Refresh(true);
            };
            var zin = new ToolStripButton("Zoom +");
            zin.Click += (s, e) => _view.ZoomStep(1);
            var zout = new ToolStripButton("Zoom −");
            zout.Click += (s, e) => _view.ZoomStep(-1);
            var size = new ToolStripLabel(L.T("Pinsel:"));
            var brush = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 50 };
            brush.Items.AddRange(new object[] { "1", "3", "5", "7", "9" });
            brush.SelectedIndex = 0;
            brush.SelectedIndexChanged += (s, e) => _view.LandRadius = brush.SelectedIndex;
            tools.Items.AddRange(new ToolStripItem[] { grid, statics, zin, zout, size, brush });
            right.Panel1.Controls.Add(_view);
            right.Panel1.Controls.Add(tools);
            right.Panel2.Controls.Add(_catalog);
            right.Panel2.Controls.Add(new Label
            {
                Dock = DockStyle.Top, Height = 18, Text = "  " + L.T("KATALOG – Klick = zum Setzen im Testfeld · Doppelklick = dem gewählten Teil links zuordnen · Rechtsklick = Kategorien"),
                BackColor = Color.FromArgb(180, 83, 9), ForeColor = Color.White
            });
            split.Panel2.Controls.Add(right);
            _host.Configure?.Invoke(_view);
            _view.UnderView = false;
            _view.MaxZ = 127;
            _view.BrushSettings = new CgiBrushSettings();
            _view.Message += (s, t) => Say(t);
            _catalog.Picked += (s, e) =>
            {
                if (e.Activated && _slot != null && e.Kind == ArtKind.Static)
                {
                    AssignSlot(e.Id);
                    return;
                }
                if (e.Activated && e.Kind == ArtKind.Multi && MessageBox.Show(this, L.T("Multi als Brücken-Plan laden?") + $" {e.Id:x4}", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    FromMulti(e.Id);
                    return;
                }
                switch (e.Kind)
                {
                    case ArtKind.Static:
                        _view.PlaceId = (ushort)e.Id;
                        _view.Tool = IsoTool.Place;
                        break;
                    case ArtKind.Land:
                        _view.PlaceLandId = (ushort)e.Id;
                        _view.PlaceLandIds = new[] { (ushort)e.Id };
                        _view.Tool = IsoTool.LandSet;
                        break;
                    case ArtKind.Multi:
                        _view.MultiId = e.Id;
                        _view.Tool = IsoTool.Multi;
                        break;
                }
            };

            // ---- left
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

            left.Controls.Add(Head("1 Plan (gespeichert nach Kategorie)", Color.FromArgb(68, 64, 60)));
            _plans = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
            _plans.SelectedIndexChanged += (s, e) =>
            {
                if (_plans.SelectedItem is BridgeTemplate t)
                {
                    _plan = Clone(t);
                    _plan.Category = t.Category;
                    _planGrid.SelectedObject = _plan;
                    _alongX = _plan.PartsX.Count > 0 || _plan.PartsY.Count == 0;
                    ShowPlan();
                }
            };
            left.Controls.Add(_plans);
            var pl = Line();
            pl.Controls.Add(B("Speichern …", SavePlan, 98));
            pl.Controls.Add(B("Löschen", () =>
            {
                if (_plans.SelectedItem is BridgeTemplate t && MessageBox.Show(this, L.T("Plan löschen?") + $" {t}", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    BridgeKit.Delete(t);
                    FillPlans(null);
                }
            }, 98));
            pl.Controls.Add(B("Hilfe", () => _host.Help?.Invoke("bridge"), 98));
            pl.Controls.Add(B("📋 Log + Bild", () => DesignLog.Export(this, _view, _test, Rectangle.Inflate(_sampleRect, 1, 2), "Bruecke",
                $"Plan '{_plan}': Breite {_plan.Width}, Länge {_plan.Length}, Anfang {_plan.StartLen}, Ende {_plan.EndLen}, Pfeiler {_plan.Pillar}, Treppe N {_plan.StairNorth} O {_plan.StairEast} S {_plan.StairSouth} W {_plan.StairWest}; " +
                $"Bogen {_build.Arch}, Bogen aus Stufen {_build.ArchStairs}, {(_alongX ? "West-Ost" : "Nord-Süd")}, {(_sample ? "Muster" : "Vorschau lang " + _length)}"), 98));
            pl.Controls.Add(B("🖼 Pläne mit Bild …", () =>
            {
                using var tb = new TemplateBrowserForm(true);
                if (tb.ShowDialog(this) == DialogResult.OK && tb.Selected is BridgeTemplate t)
                {
                    _plan = Clone(t);
                    _plan.Category = t.Category;
                    _planGrid.SelectedObject = _plan;
                    _alongX = _plan.PartsX.Count > 0 || _plan.PartsY.Count == 0;
                    _sample = true;
                    ShowPlan();
                }
            }, 300));
            left.Controls.Add(pl);
            _planGrid = new PropertyGrid { Width = 310, Height = 260, ToolbarVisible = false, HelpVisible = true, PropertySort = PropertySort.Categorized, SelectedObject = _plan };
            _planGrid.PropertyValueChanged += (s, e) => ShowPlan();
            left.Controls.Add(_planGrid);

            left.Controls.Add(Head("2 Teile (aus der TileData, mit Bild)", Color.FromArgb(180, 83, 9)));
            left.Controls.Add(_slotLabel);
            foreach ((string slot, BridgeKit.Role role) in new[]
                     {
                         ("Bretter", BridgeKit.Role.Deck), ("Bretter N-S", BridgeKit.Role.Deck), ("Geländer W-O", BridgeKit.Role.Rail), ("Geländer N-S", BridgeKit.Role.Rail),
                         ("Pfosten", BridgeKit.Role.Post), ("Pfeiler", BridgeKit.Role.Pillar), ("Lampe", BridgeKit.Role.Lamp),
                         ("Treppe Nord", BridgeKit.Role.Stairs), ("Treppe Ost", BridgeKit.Role.Stairs), ("Treppe Süd", BridgeKit.Role.Stairs), ("Treppe West", BridgeKit.Role.Stairs)
                     })
            {
                _pieces.Controls.Add(B(slot, () => SetSlot(slot, role), 98));
            }
            left.Controls.Add(_pieces);
            _genGrid = new PropertyGrid { Width = 310, Height = 250, ToolbarVisible = false, HelpVisible = true, PropertySort = PropertySort.Categorized, SelectedObject = _gen };
            left.Controls.Add(_genGrid);
            var gl = Line();
            gl.Controls.Add(B("▶ Aus Teilen erzeugen", FromPieces, 150));
            gl.Controls.Add(B("Teile neu suchen", () =>
            {
                BridgeKit.ClearCache();
                CopyGen(BridgeKit.AutoPieces(), _gen);
                _genGrid.Refresh();
                FromPieces();
            }, 150));
            left.Controls.Add(gl);

            left.Controls.Add(Head("3 Testfeld", Color.FromArgb(37, 99, 235)));
            var tf = Line();
            tf.Controls.Add(B("Muster", () =>
            {
                _sample = true;
                ShowPlan();
            }, 70));
            tf.Controls.Add(B("Vorschau lang", () =>
            {
                _sample = false;
                ShowPlan();
            }, 100));
            tf.Controls.Add(B("W-O / N-S", () =>
            {
                _alongX = !_alongX;
                ShowPlan();
            }, 90));
            tf.Controls.Add(new Label { Text = L.T("Länge:"), AutoSize = true, Margin = new Padding(3, 8, 0, 0) });
            left.Controls.Add(tf);
            // sliders: length, width, arch (the long preview follows at once)
            Label Value(string text) => new Label { Text = text, AutoSize = false, Width = 300, Height = 16, Margin = new Padding(3, 4, 0, 0) };
            TrackBar Slider(int min, int max, int value) => new TrackBar { Minimum = min, Maximum = max, Value = Math.Clamp(value, min, max), Width = 300, TickFrequency = Math.Max(1, (max - min) / 10), AutoSize = false, Height = 30 };
            Label lenText = Value(L.T("Länge") + $": {_length} Tiles");
            TrackBar len = Slider(3, 90, _length);
            len.ValueChanged += (s, e) =>
            {
                _length = len.Value;
                lenText.Text = L.T("Länge") + $": {_length} Tiles";
                _sample = false;
                ShowPlan();
            };
            Label widthText = Value(L.T("Breite") + $": {_gen.Width} Tiles");
            TrackBar width = Slider(1, 12, 3);
            width.ValueChanged += (s, e) =>
            {
                _gen.Width = width.Value;
                _build.Width = width.Value;
                widthText.Text = L.T("Breite") + $": {width.Value} Tiles";
                _genGrid.Refresh();
                if (_plan.Note == "Aus Teilen erzeugt")
                {
                    FromPieces();
                }
                else
                {
                    Say(L.T("Breite gilt für Pläne aus Teilen – ein Plan von Hand / aus der Karte behält seine Breite."));
                }
            };
            Label archText = Value(L.T("Bogen") + $": {_build.Arch} Z");
            TrackBar arch = Slider(0, 40, _build.Arch);
            arch.ValueChanged += (s, e) =>
            {
                _build.Arch = arch.Value;
                archText.Text = L.T("Bogen") + $": {_build.Arch} Z" + (_build.ArchStairs ? $" = {_build.Arch / 5} " + L.T("Stufen") : string.Empty);
                _sample = false;
                ShowPlan();
            };
            var stairsArch = new CheckBox { Text = L.T("Bogen aus Stufen (Treppe hinauf, oben flach, hinunter)"), AutoSize = true, Checked = _build.ArchStairs };
            stairsArch.CheckedChanged += (s, e) =>
            {
                _build.ArchStairs = stairsArch.Checked;
                if (_build.ArchStairs && _build.Arch < 10)
                {
                    arch.Value = 20;
                }
                if (_build.ArchStairs && (string.IsNullOrEmpty(_plan.StairWest) || string.IsNullOrEmpty(_plan.StairEast)))
                {
                    Say(L.T("Für den Bogen aus Stufen braucht der Plan Treppen-Stufen (Knöpfe 'Treppe …' links) – West / Ost für West-Ost-Brücken, Nord / Süd für Nord-Süd."));
                }
                _sample = false;
                ShowPlan();
            };
            left.Controls.AddRange(new Control[] { lenText, len, widthText, width, archText, arch, stairsArch });
            var build = new PropertyGrid { Width = 310, Height = 170, ToolbarVisible = false, HelpVisible = false, PropertySort = PropertySort.Categorized, SelectedObject = _build };
            build.PropertyValueChanged += (s, e) =>
            {
                _plan.Width = _build.Width;
                ShowPlan();
            };
            left.Controls.Add(build);

            left.Controls.Add(Head("4 Von Hand / aus Karte / Multi", Color.FromArgb(126, 34, 206)));
            var hl = Line();
            hl.Controls.Add(B("Muster übernehmen", TakeSample, 150));
            hl.Controls.Add(B("Andere Richtung ableiten", TurnPlan, 150));
            hl.Controls.Add(B("Brücke aus der Karte", FromMap, 150));
            hl.Controls.Add(B("Multi laden …", () =>
            {
                _catalog.Kind = ArtKind.Multi;
                Say(L.T("Im Katalog eine Multi doppelklicken – sie wird zum Brücken-Plan."));
            }, 150));
            left.Controls.Add(hl);
            left.Controls.Add(new Label
            {
                AutoSize = false, Width = 310, Height = 64, ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Muster = die Brücke einmal, flach: hier mit Katalog + Static setzen / Löschen / Radierer bauen, dann 'Muster übernehmen'. Anfang und Ende stehen im Plan, die Mitte wird bei langen Brücken wiederholt.")
            });

            left.Controls.Add(Head("5 Fertig", Color.FromArgb(22, 128, 61)));
            var dl = Line();
            dl.Controls.Add(B("✔ Im Studio benutzen", UseInStudio, 150));
            dl.Controls.Add(B("Schließen", Close, 98));
            left.Controls.Add(dl);

            Controls.Add(split);
            Controls.Add(_status);
        }
    }
}
