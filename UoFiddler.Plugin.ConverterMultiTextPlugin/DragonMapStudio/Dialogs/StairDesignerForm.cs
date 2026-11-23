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

        public StairDesignerForm(StairDesignerHost host)
        {
            _host = host;
            Text = L.T("Treppen-Designer") + " – Dragon Map Studio";
            Size = new Size(1400, 900);
            MinimumSize = new Size(1000, 640);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            BuildUi();
            NewField();
            FillModels(null);
            _catalog.ShowCategory(ArtKind.Static, "Treppen");
            Say(L.T("Bauen: im Katalog Block / Stufe wählen, ins Testfeld klicken ('Z +' oben = Höhe, 'auf Statics stapeln' an). Oder 'Aus Treppen-Set erzeugen'. Dann mit 'Bereich' markieren und 'Als Modell übernehmen'."));
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
            Say(L.T("Modell übernommen") + $": {m.Parts.Count} " + L.T("Teile") + $", " + L.T("Anstieg") + $" {m.Rise} Z, " + L.T("Länge") + $" {m.Run} – " +
                L.T("'Gebaut hinauf nach' prüfen, dann speichern."));
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
            tools.Items.Add(new ToolStripLabel("Z +"));
            var z = new NumericUpDown { Minimum = -60, Maximum = 120, Increment = 5, Width = 55 };
            z.ValueChanged += (s, e) => _view.ZOffset = (int)z.Value;
            tools.Items.Add(new ToolStripControlHost(z) { ToolTipText = L.T("Höhe über dem Boden für 'Static setzen' (Stufen: 0, 5, 10, 15 …)") });
            var stack = new ToolStripButton(L.T("auf Statics stapeln")) { CheckOnClick = true, Checked = true };
            stack.CheckedChanged += (s, e) => _view.StackOnStatics = stack.Checked;
            tools.Items.Add(stack);
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
            var undo = new ToolStripButton("↶");
            undo.Click += (s, e) => _view.Undo();
            var redo = new ToolStripButton("↷");
            redo.Click += (s, e) => _view.Redo();
            tools.Items.AddRange(new ToolStripItem[] { grid, ids, zin, zout, undo, redo });
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
            _catalog.Picked += (s, e) =>
            {
                if (e.Kind == ArtKind.Static)
                {
                    _view.PlaceId = (ushort)e.Id;
                    _view.Tool = IsoTool.Place;
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
                Text = L.T("So ist eine Treppe aufgebaut: viereckige Blöcke stufig übereinander (je 5 Z), oben auf jeder Stufe das geneigte Treppenstück. Im Katalog wählen, 'Z +' setzen, ins Testfeld klicken – oder erzeugen und ändern.")
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
            left.Controls.Add(tl);
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
