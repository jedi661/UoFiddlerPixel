/***************************************************************************
 *
 *  * Dragon Map Studio – stair designer: build stairs by hand in a test field (blocks stepped up, the sloped stair
 *  *  piece on top), generate one from a stair set, take one from the map, save models by category, give a model
 *  *  to the tool "Treppe" (up, down, under the map, several times). "Log + Bild" for the developer.
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
    public sealed class StairDesignerHost
    {
        public MapData Map;
        public CgiBrushSettings Brushes;
        public Action<IsoMapView> Configure;
        public Func<Rectangle> CurrentArea;
        public Action<StairModel> Use;
        public Action<string> Status;
        public Action<string> Help;
    }

    public sealed class StairDesignerForm : Form
    {
        private const int Size0 = 48;
        private readonly StairDesignerHost _host;
        private readonly IsoMapView _view = new IsoMapView { Dock = DockStyle.Fill };
        private readonly ArtCatalog _catalog = new ArtCatalog();
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
        private MapData _test;
        private StairModel _model = new StairModel();
        private ComboBox _models;
        private PropertyGrid _modelGrid;
        private readonly StaircaseSettings _gen = new StaircaseSettings { Up = Compass.North, Floors = 1, Turn = StairTurn.Straight };
        private Compass _previewUp = Compass.North;
        private bool _turnsArmed;
        private ToolStripLabel _pieceHeight;
        private NumericUpDown _zStep;

        public StairDesignerForm(StairDesignerHost host)
        {
            _host = host;
            Text = L.T("Treppen-Designer") + " – Dragon Map Studio";
            Size = new Size(1400, 900);
            MinimumSize = new Size(1000, 640);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.Z && !(ActiveControl is TextBox))
                {
                    _view.Undo();
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.Y && !(ActiveControl is TextBox))
                {
                    _view.Redo();
                    e.Handled = true;
                }
            };
            BuildUi();
            NewField();
            FillModels(null);
            _catalog.ShowCategory(ArtKind.Static, "Treppen");
            Say(L.T("Bauen: im Katalog Block / Stufe wählen, ins Testfeld klicken – 'Höhe: auf den Stapel' legt es auf das oberste Teil (Höhen aus der TileData). Oder 'Aus Treppen-Set erzeugen'. Dann mit 'Bereich' markieren und 'Als Modell übernehmen'."));
        }

        private void Say(string text)
        {
            _status.Text = "  " + text;
            _host.Status?.Invoke(text);
        }

        // ================================================================== test field

        private void NewField()
        {
            var tiles = Enumerable.Range(0, Size0 * Size0).Select(i => (ushort)(0x0003 + i % 4)).ToArray();
            _test = new MapData(Size0, Size0, tiles, new sbyte[Size0 * Size0]) { Source = L.T("Treppen-Testfeld") };
            _view.SetMap(_test);
            _view.CenterOn(Size0 / 2.0, Size0 / 2.0);
            _view.Refresh(true);
        }

        private Rectangle Marked => _view.Area.IsEmpty ? new Rectangle(0, 0, Size0, Size0) : _view.Area;

        private void Generate()
        {
            StairSet set = _host.Brushes.StairSets.FirstOrDefault(s => s.Name == _host.Brushes.StairSet);
            if (set == null || HexList.Parse(set.DownNorth + " " + set.DownEast + " " + set.DownSouth + " " + set.DownWest).Length < 4)
            {
                set = BridgeKit.AutoStairs(out _);
                Say(L.T("Kein Treppen-Set gewählt – Stufen aus der TileData (Neigung aus dem Bild)") + $": {set.Name}");
            }
            NewField();
            List<StaticChange> ch = Staircase.Build(_test, new Point(Size0 / 2, Size0 / 2 + 4), 0, _gen, set);
            _test.Apply(ch);
            _test.ClearHistory();
            _view.Refresh(true);
            Say(L.T("Erzeugt") + $": {ch.Count} Statics – " + L.T("von Hand ändern, dann mit 'Bereich' markieren und 'Als Modell übernehmen'."));
        }

        private void TakeModel()
        {
            Rectangle r = Marked;
            if (_view.Selection.Count > 0 && _view.Area.IsEmpty)
            {
                // exactly the selected pieces (several stairs in the field: only this one)
                var sel = _view.Selection.ToList();
                r = Rectangle.FromLTRB(sel.Min(s => s.X), sel.Min(s => s.Y), sel.Max(s => s.X) + 1, sel.Max(s => s.Y) + 1);
                var only = new MapData(Size0, Size0, Enumerable.Repeat((ushort)3, Size0 * Size0).ToArray(), new sbyte[Size0 * Size0]);
                only.Apply(sel.Select(s => MapData.Add(s.Copy())).ToList());
                StairModel ms = StairModel.Capture(only, r, _model.Up, 0, _model.Name);
                ms.Category = _model.Category;
                ms.Note = _model.Note;
                _model = ms;
                _modelGrid.SelectedObject = _model;
                FillTurns();
                Say(L.T("Modell aus der Auswahl übernommen") + $": {ms.Parts.Count} " + L.T("Teile") + $", " + L.T("hinauf nach") + $" {ms.Up}");
                return;
            }
            if (_view.Area.IsEmpty)
            {
                // the used part of the field
                var st = _test.AllStatics().ToList();
                if (st.Count == 0)
                {
                    Say(L.T("Das Testfeld ist leer."));
                    return;
                }
                r = Rectangle.FromLTRB(st.Min(s => s.X), st.Min(s => s.Y), st.Max(s => s.X) + 1, st.Max(s => s.Y) + 1);
            }
            StairModel m = StairModel.Capture(_test, r, _model.Up, 0, _model.Name);
            m.Category = _model.Category;
            m.Note = _model.Note;
            _model = m;
            _modelGrid.SelectedObject = _model;
            FillTurns();
            Say(L.T("Modell übernommen") + $": {m.Parts.Count} " + L.T("Teile") + $", " + L.T("Anstieg") + $" {m.Rise} Z, " + L.T("Länge") + $" {m.Run}, " + L.T("hinauf nach") + $" {m.Up} – " +
                L.T("Dreh-Tabelle prüfen, dann speichern."));
        }

        /// <summary>Marks → model → save in one go.</summary>
        private void TakeAndSave()
        {
            TakeModel();
            if (_model.Parts.Count > 0)
            {
                SaveModel();
            }
        }

        // ------------------------------------------------------------------ turn table

        private readonly DataGridView _turns = new DataGridView
        {
            Width = 310, Height = 260, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false, ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false, BackgroundColor = Color.FromArgb(40, 40, 40)
        };

        private void SetupTurns()
        {
            foreach ((string name, string head) in new[] { ("N", "N"), ("E", "O"), ("S", "S"), ("W", "W") })
            {
                _turns.Columns.Add(new DataGridViewImageColumn { Name = name, HeaderText = head, ImageLayout = DataGridViewImageCellLayout.Zoom, Width = 70 });
            }
            _turns.RowTemplate.Height = 56;
            _turns.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    StairTurn4 row = _model.Turns[e.RowIndex];
                    Compass c = (Compass)e.ColumnIndex;
                    Say(L.T("Dreh-Tabelle") + $": " + L.T("hinauf nach") + $" {c} = {row[c]:x4} {UoArtCache.ItemName(row[c])} – " + L.T("im Katalog doppelklicken setzt hier eine andere Grafik"));
                }
            };
        }

        private void FillTurns()
        {
            _turns.Rows.Clear();
            if (_model.Turns.Count == 0 && _model.Parts.Count > 0)
            {
                _model.AutoTurns(false);
            }
            foreach (StairTurn4 t in _model.Turns)
            {
                int i = _turns.Rows.Add();
                foreach (Compass c in new[] { Compass.North, Compass.East, Compass.South, Compass.West })
                {
                    _turns.Rows[i].Cells[(int)c].Value = UoArtCache.Static(t[c], 0);
                    _turns.Rows[i].Cells[(int)c].ToolTipText = $"{t[c]:x4} {UoArtCache.ItemName(t[c])}";
                }
            }
        }

        /// <summary>Double click in the catalog with a cell of the turn table chosen: that graphic for that direction.</summary>
        private bool AssignTurn(int id)
        {
            if (_turns.CurrentCell == null || !_turns.Focused && !_turns.ContainsFocus && _turns.CurrentCell.RowIndex < 0 || _model.Turns.Count == 0)
            {
                return false;
            }
            int r = _turns.CurrentCell.RowIndex, c = _turns.CurrentCell.ColumnIndex;
            if (r < 0 || r >= _model.Turns.Count || c < 0 || c > 3)
            {
                return false;
            }
            _model.Turns[r][(Compass)c] = (ushort)id;
            _turnsArmed = false;
            FillTurns();
            Say(L.T("Dreh-Tabelle") + $": " + L.T("hinauf nach") + $" {(Compass)c} = {id:x4} {UoArtCache.ItemName(id)} – " + L.T("'Gedreht ansehen' zeigt es"));
            return true;
        }

        private void TakeFromMap()
        {
            Rectangle r = _host.CurrentArea?.Invoke() ?? Rectangle.Empty;
            if (r.IsEmpty || _host.Map == null)
            {
                Say(L.T("In der CGI-Ansicht eine Treppe der Karte mit 'Bereich' markieren, dann hier übernehmen."));
                return;
            }
            int ground = Enumerable.Range(r.Top, r.Height).SelectMany(y => Enumerable.Range(r.Left, r.Width).Select(x => _host.Map.LandZ(x, y))).Min();
            StairModel m = StairModel.Capture(_host.Map, r, _model.Up, ground, _model.Name);
            m.Category = _model.Category;
            m.Note = $"Aus der Karte ({r.X}, {r.Y}, {r.Width}×{r.Height})";
            _model = m;
            _modelGrid.SelectedObject = _model;
            FillTurns();
            ShowModel(_model.Up);
            Say(L.T("Treppe aus der Karte übernommen") + $": {m.Parts.Count} " + L.T("Teile") + $", " + L.T("Anstieg") + $" {m.Rise} Z.");
        }

        /// <summary>Shows the model in the test field, turned to a direction (to check the turning).</summary>
        private void ShowModel(Compass up)
        {
            _previewUp = up;
            NewField();
            List<StaticChange> ch = _model.Build(_test, new Point(Size0 / 2 - 3, Size0 / 2 - 3), 0, up, 1, false);
            _test.Apply(ch);
            _test.ClearHistory();
            _view.Refresh(true);
            Say(L.T("Modell") + $" '{_model.Name}' " + L.T("hinauf nach") + $" {up}: {ch.Count} Statics" + (up != _model.Up ? " – " + L.T("gedreht, Stufen nach ihrem Bild getauscht") : string.Empty));
        }

        private void FillModels(string select)
        {
            _models.Items.Clear();
            foreach (StairModel m in StairModel.LoadAll())
            {
                _models.Items.Add(m);
            }
            for (int i = 0; i < _models.Items.Count; i++)
            {
                if (_models.Items[i].ToString() == select)
                {
                    _models.SelectedIndex = i;
                }
            }
        }

        private void SaveModel()
        {
            if (_model.Parts.Count == 0)
            {
                Say(L.T("Erst ein Modell übernehmen."));
                return;
            }
            string cat = Microsoft.VisualBasic.Interaction.InputBox(L.T("Kategorie (Ordner), z. B. Holz, Stein, Keller, Turm:"), Text, _model.Category);
            if (string.IsNullOrWhiteSpace(cat))
            {
                return;
            }
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name des Treppen-Modells:"), Text, _model.Name);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            _model.Category = cat.Trim();
            _model.Name = name.Trim();
            string path = _model.Save();
            FillModels(_model.ToString());
            Say(L.T("Gespeichert") + ": " + path);
        }

        // ================================================================== user interface

        private void BuildUi()
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 345 };
            var left = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(6), BackColor = Color.FromArgb(241, 245, 249) };
            split.Panel1.Controls.Add(left);

            var right = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 560 };
            var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            void Tool(string text, IsoTool t)
            {
                var b = new ToolStripButton(L.T(text));
                b.Click += (s, e) =>
                {
                    _view.Tool = t;
                    _view.Focus();
                };
                tools.Items.Add(b);
            }
            Tool("Auswahl", IsoTool.Select);
            Tool("Bereich", IsoTool.Area);
            Tool("Static setzen", IsoTool.Place);
            Tool("Löschen", IsoTool.Delete);
            Tool("Radierer", IsoTool.Eraser);
            tools.Items.Add(new ToolStripSeparator());
            // height of the next piece: on top of the stack (tiledata heights, block on block) or a fixed z over the ground
            tools.Items.Add(new ToolStripLabel(L.T("Höhe:")));
            var mode = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 175 };
            mode.Items.AddRange(new object[] { L.T("auf den Stapel (TileData-Höhe)"), L.T("feste Höhe über dem Boden") });
            mode.SelectedIndex = 0;
            var z = new NumericUpDown { Minimum = -60, Maximum = 120, Increment = 5, Width = 55, Enabled = false };
            void ApplyMode()
            {
                bool stack = mode.SelectedIndex == 0;
                _view.StackOnStatics = stack;
                _view.ZOffset = stack ? 0 : (int)z.Value;
                z.Enabled = !stack;
            }
            mode.SelectedIndexChanged += (s, e) => ApplyMode();
            z.ValueChanged += (s, e) => ApplyMode();
            tools.Items.Add(mode);
            tools.Items.Add(new ToolStripControlHost(z) { ToolTipText = L.T("Feste Höhe über dem Boden (Schritt = Höhe des gewählten Teils aus der TileData)") });
            _pieceHeight = new ToolStripLabel("h –") { ToolTipText = L.T("Höhe des gewählten Teils laut TileData") };
            tools.Items.Add(_pieceHeight);
            _zStep = z;
            tools.Items.Add(new ToolStripSeparator());
            // move the selected pieces up / down by their height or by 1
            tools.Items.Add(new ToolStripLabel(L.T("Auswahl:")));
            void Move(int dz, bool byHeight)
            {
                if (_view.Selection.Count == 0)
                {
                    Say(L.T("Erst Teile mit 'Auswahl' wählen (Strg = mehrere)."));
                    return;
                }
                int step = byHeight ? Math.Max(1, _view.Selection.Max(s2 => Math.Max(1, UoArtCache.ItemHeight(s2.Id)))) : 1;
                _view.MoveSelection(0, 0, dz * step, false);
            }
            foreach ((string text, int dz, bool byH) in new[] { ("▲ h", 1, true), ("▼ h", -1, true), ("+1", 1, false), ("−1", -1, false) })
            {
                var mb = new ToolStripButton(text) { ToolTipText = byH ? L.T("um die Höhe des Teils (TileData) hoch / runter") : L.T("um 1 Z") };
                mb.Click += (s, e) => Move(dz, byH);
                tools.Items.Add(mb);
            }
            var setZ = new ToolStripButton("z = …") { ToolTipText = L.T("Die gewählten Teile auf eine genaue Höhe setzen") };
            setZ.Click += (s, e) =>
            {
                if (_view.Selection.Count == 0)
                {
                    Say(L.T("Erst Teile mit 'Auswahl' wählen (Strg = mehrere)."));
                    return;
                }
                string v = Microsoft.VisualBasic.Interaction.InputBox(L.T("Höhe z der gewählten Teile:"), Text, _view.Selection.First().Z.ToString());
                if (int.TryParse(v, out int nz))
                {
                    _view.ModifySelection(s2 => s2.Z = Math.Clamp(nz, -128, 127), L.T("Höhe gesetzt") + $": z {nz}");
                }
            };
            tools.Items.Add(setZ);
            var grid = new ToolStripButton(L.T("Gitter")) { CheckOnClick = true, Checked = true };
            grid.Click += (s, e) =>
            {
                _view.ShowGrid = grid.Checked;
                _view.Refresh(true);
            };
            var ids = new ToolStripButton(L.T("IDs")) { CheckOnClick = true };
            ids.Click += (s, e) =>
            {
                _view.ShowStaticIds = ids.Checked;
                _view.Refresh(true);
            };
            var zin = new ToolStripButton("Zoom +");
            zin.Click += (s, e) => _view.ZoomStep(1);
            var zout = new ToolStripButton("Zoom −");
            zout.Click += (s, e) => _view.ZoomStep(-1);
            var undo = new ToolStripButton("↶ " + L.T("Rückgängig")) { ToolTipText = "Strg+Z" };
            undo.Click += (s, e) => _view.Undo();
            var redo = new ToolStripButton("↷ " + L.T("Wiederholen")) { ToolTipText = "Strg+Y" };
            redo.Click += (s, e) => _view.Redo();
            tools.Items.AddRange(new ToolStripItem[] { grid, ids, zin, zout, undo, redo });
            // second row: move the selected pieces north / south / west / east
            var moveBar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            moveBar.Items.Add(new ToolStripLabel(L.T("Auswahl verschieben:")));
            foreach ((string text, int dx, int dy) in new[] { ("▲ N", 0, -1), ("▼ S", 0, 1), ("◀ W", -1, 0), ("▶ O", 1, 0) })
            {
                var mb = new ToolStripButton(text) { ToolTipText = L.T("Gewählte Teile ein Feld verschieben (N = oben rechts, wie im Client)") };
                mb.Click += (s, e) =>
                {
                    if (_view.Selection.Count == 0)
                    {
                        Say(L.T("Erst Teile mit 'Auswahl' wählen (Strg = mehrere)."));
                        return;
                    }
                    _view.MoveSelection(dx, dy, 0, false);
                };
                moveBar.Items.Add(mb);
            }
            var del = new ToolStripButton("✖ " + L.T("Auswahl löschen"));
            del.Click += (s, e) => _view.DeleteSelection();
            moveBar.Items.Add(del);
            right.Panel1.Controls.Add(moveBar);
            right.Panel1.Controls.Add(_view);
            right.Panel1.Controls.Add(tools);
            right.Panel2.Controls.Add(_catalog);
            right.Panel2.Controls.Add(new Label
            {
                Dock = DockStyle.Top, Height = 18, Text = "  " + L.T("KATALOG – Klick = zum Setzen im Testfeld (Block, Stufe, Geländer …) · Rechtsklick = Kategorien"),
                BackColor = Color.FromArgb(180, 83, 9), ForeColor = Color.White
            });
            split.Panel2.Controls.Add(right);
            _host.Configure?.Invoke(_view);
            _view.BrushSettings = new CgiBrushSettings();
            _view.UnderView = false;
            _view.MaxZ = 127;
            _view.ShowGrid = true;
            _view.StackOnStatics = true;
            _view.ZOffset = 0;
            _view.Message += (s, t) => Say(t);
            _view.Enter += (s, e) => _turnsArmed = false;
            _catalog.Picked += (s, e) =>
            {
                if (e.Activated && e.Kind == ArtKind.Static && _turnsArmed && AssignTurn(e.Id))
                {
                    return;
                }
                if (e.Kind == ArtKind.Static)
                {
                    _view.PlaceId = (ushort)e.Id;
                    _view.Tool = IsoTool.Place;
                    int ih = UoArtCache.ItemHeight(e.Id);
                    _pieceHeight.Text = $"h {ih}";
                    if (ih > 0 && _zStep != null)
                    {
                        _zStep.Increment = ih;
                    }
                    Compass? up = null;
                    try
                    {
                        up = BridgeKit.StairUp(e.Id, out _);
                    }
                    catch (Exception)
                    {
                        // no art
                    }
                    Say($"{e.Id:x4} {UoArtCache.ItemName(e.Id)} h{UoArtCache.ItemHeight(e.Id)}" + (up != null ? " · " + L.T("Stufe hinauf nach") + $" {up}" : string.Empty) + " – " + L.T("ins Testfeld klicken"));
                }
            };

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

            left.Controls.Add(Head("1 Modelle (gespeichert nach Kategorie)", Color.FromArgb(68, 64, 60)));
            _models = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
            _models.SelectedIndexChanged += (s, e) =>
            {
                if (_models.SelectedItem is StairModel m)
                {
                    _model = m;
                    _modelGrid.SelectedObject = _model;
                    FillTurns();
                    ShowModel(m.Up);
                }
            };
            left.Controls.Add(_models);
            var ml = Line();
            ml.Controls.Add(B("Speichern …", SaveModel, 98));
            ml.Controls.Add(B("Löschen", () =>
            {
                if (_models.SelectedItem is StairModel m && MessageBox.Show(this, L.T("Modell löschen?") + $" {m}", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    m.Delete();
                    FillModels(null);
                }
            }, 98));
            ml.Controls.Add(B("Hilfe", () => _host.Help?.Invoke("stairs"), 98));
            left.Controls.Add(ml);
            _modelGrid = new PropertyGrid { Width = 310, Height = 210, ToolbarVisible = false, HelpVisible = true, PropertySort = PropertySort.Categorized, SelectedObject = _model };
            left.Controls.Add(_modelGrid);

            left.Controls.Add(Head("2 Bauen", Color.FromArgb(180, 83, 9)));
            left.Controls.Add(new Label
            {
                AutoSize = false, Width = 310, Height = 64, ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("So ist eine Treppe aufgebaut: viereckige Blöcke stufig übereinander (je 5 Z laut TileData), oben auf jeder Stufe das geneigte Treppenstück. Im Katalog wählen und ins Testfeld klicken: 'auf den Stapel' legt jedes Teil auf das oberste – Block auf Block, die Stufe darauf.")
            });
            var gl = Line();
            gl.Controls.Add(B("Testfeld leeren", NewField, 148));
            gl.Controls.Add(B("Aus Treppen-Set erzeugen", Generate, 148));
            left.Controls.Add(gl);
            left.Controls.Add(new PropertyGrid { Width = 310, Height = 200, ToolbarVisible = false, HelpVisible = false, PropertySort = PropertySort.Categorized, SelectedObject = _gen });

            left.Controls.Add(Head("3 Übernehmen und prüfen", Color.FromArgb(126, 34, 206)));
            var tl = Line();
            tl.Controls.Add(B("Als Modell übernehmen", TakeModel, 148));
            tl.Controls.Add(B("Treppe aus der Karte", TakeFromMap, 148));
            tl.Controls.Add(B("Richtungen festlegen …", () => DirectionGroupsForm.ShowShared(this), 148));
            tl.Controls.Add(B("💾 Markierte Treppe speichern …", TakeAndSave, 300));
            left.Controls.Add(tl);
            left.Controls.Add(new Label
            {
                AutoSize = false, Width = 310, Height = 34, ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Mehrere Treppen im Feld: eine mit 'Bereich' markieren oder ihre Teile mit 'Auswahl' (Rahmen) wählen – nur diese wird übernommen.")
            });
            left.Controls.Add(Head("Dreh-Tabelle (Grafik je Richtung)", Color.FromArgb(8, 145, 178)));
            SetupTurns();
            _turns.Enter += (s, e) => _turnsArmed = true;
            left.Controls.Add(_turns);
            var tt = Line();
            tt.Controls.Add(B("Automatisch füllen", () =>
            {
                _model.Turns.Clear();
                _model.AutoTurns(false);
                FillTurns();
            }, 148));
            tt.Controls.Add(B("Zelle → Katalog", () =>
            {
                _turnsArmed = true;
                Say(L.T("Jetzt im Katalog die Grafik doppelklicken – sie kommt in die gewählte Zelle der Dreh-Tabelle."));
            }, 148));
            left.Controls.Add(tt);
            left.Controls.Add(new Label
            {
                AutoSize = false, Width = 310, Height = 48, ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Je Zeile ein Teil des Modells, Spalten = die Grafik, wenn die Treppe nach N / O / S / W hinauf geht. Zelle anklicken, im Katalog doppelklicken = andere Grafik. Blöcke bleiben gleich.")
            });
            var rl = Line();
            rl.Controls.Add(new Label { Text = L.T("Gedreht ansehen:"), AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
            foreach ((string t, Compass c) in new[] { ("N", Compass.North), ("O", Compass.East), ("S", Compass.South), ("W", Compass.West) })
            {
                rl.Controls.Add(B(t, () => ShowModel(c), 40));
            }
            left.Controls.Add(rl);
            var lg = Line();
            lg.Controls.Add(B("📋 Log + Bild", () => DesignLog.Export(this, _view, _test, _view.Area, "Treppe", _model.Parts.Count > 0 ? _model.Describe() : null), 148));
            left.Controls.Add(lg);
            left.Controls.Add(new Label
            {
                AutoSize = false, Width = 310, Height = 48, ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("'Log + Bild' speichert ein Bild des Testfelds und jede Stufe mit Position, Höhe, ID, Name und Neigung – schick es dem Entwickler.")
            });

            left.Controls.Add(Head("4 Im Studio benutzen", Color.FromArgb(22, 128, 61)));
            var ul = Line();
            ul.Controls.Add(B("✔ Im Studio benutzen", () =>
            {
                if (_model.Parts.Count == 0)
                {
                    Say(L.T("Erst ein Modell übernehmen oder laden."));
                    return;
                }
                _model.Save();
                FillModels(_model.ToString());
                _host.Use?.Invoke(_model);
                Say(L.T("Modell ist im Werkzeug Treppe") + $" ('{_model}') – " + L.T("klicken setzt es; R dreht, 'Nach unten bauen', 'Modell mehrmals', unter der Karte mit 'Unter der Karte bauen'."));
            }, 200));
            ul.Controls.Add(B("Schließen", Close, 98));
            left.Controls.Add(ul);

            Controls.Add(split);
            Controls.Add(_status);
        }
    }
}
