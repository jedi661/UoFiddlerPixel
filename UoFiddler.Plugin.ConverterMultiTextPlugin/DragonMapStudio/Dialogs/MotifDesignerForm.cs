/***************************************************************************
 *
 *  * Dragon Map Studio – motif designer ("Muster-Baukasten"): the same building bench as the stair designer, for
 *  *  anything – battlements, tents, a wall with windows, a fortress wall, a floor with ceiling …
 *  *
 *  *  Every motif has a version for north, east, south and west: build one direction by hand, take it, let the studio
 *  *  turn the missing ones (or build them by hand too). Saved as Kategorie / Name with the four directions below.
 *  *  "Ziehen testen" shows how the motif repeats; "Im Studio benutzen" gives it to the tool "Muster".
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class MotifDesignerHost
    {
        public MapData Map;
        public CgiBrushSettings Brushes;
        public Action<IsoMapView> Configure;
        public Func<Rectangle> CurrentArea;
        public Action<Motif, Compass> Use;
        public Action<string> Status;
        public Action<string> Help;
    }

    public sealed class MotifDesignerForm : Form
    {
        private const int Size0 = 64;
        private static readonly Compass[] Dirs = { Compass.North, Compass.East, Compass.South, Compass.West };
        private readonly MotifDesignerHost _host;
        private readonly IsoMapView _view = new IsoMapView { Dock = DockStyle.Fill };
        private readonly ArtCatalog _catalog = new ArtCatalog();
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
        private readonly TreeView _tree = new TreeView { Width = 310, Height = 220, HideSelection = false };
        private readonly Dictionary<Compass, RadioButton> _dirButtons = new Dictionary<Compass, RadioButton>();
        private readonly MotifDrawSettings _draw = new MotifDrawSettings { Drag = true };
        private MapData _test;
        private Motif _motif = new Motif();
        private Compass _building = Compass.East;
        private PropertyGrid _grid;
        private ToolStripLabel _pieceHeight;
        private NumericUpDown _zStep;

        public MotifDesignerForm(MotifDesignerHost host)
        {
            _host = host;
            Text = L.T("Muster-Baukasten") + " – Dragon Map Studio";
            Size = new Size(1450, 920);
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
            FillTree(null);
            Say(L.T("Richtung wählen (z. B. Ost = das Muster läuft nach Osten), im Katalog Teile wählen und ins Testfeld bauen, mit 'Bereich' markieren, 'Markiertes als … übernehmen'. Fehlende Richtungen dreht das Studio, oder du baust sie selbst."));
        }

        private void Say(string text)
        {
            _status.Text = "  " + text;
            _host.Status?.Invoke(text);
        }

        // ================================================================== field

        private void NewField()
        {
            var tiles = Enumerable.Range(0, Size0 * Size0).Select(i => (ushort)(0x0003 + i % 4)).ToArray();
            _test = new MapData(Size0, Size0, tiles, new sbyte[Size0 * Size0]) { Source = L.T("Muster-Testfeld") };
            _view.SetMap(_test);
            _view.CenterOn(Size0 / 2.0, Size0 / 2.0);
            _view.Refresh(true);
        }

        private void Put(List<PieceStatic> parts, Point at, int z = 0)
        {
            _test.Apply(parts.Select(p => MapData.Add(new StaticItem { Id = p.Id, Hue = p.Hue, X = at.X + p.X, Y = at.Y + p.Y, Z = z + p.Z }))
                .Where(c => _test.InBounds(c.After.X, c.After.Y)).ToList());
        }

        /// <summary>Shows one direction of the motif in the field (to change it by hand).</summary>
        private void ShowVariant(Compass c)
        {
            _building = c;
            if (_dirButtons.TryGetValue(c, out RadioButton r) && !r.Checked)
            {
                r.Checked = true;
            }
            NewField();
            Put(_motif.Variant(c), new Point(Size0 / 2 - 4, Size0 / 2 - 4));
            _test.ClearHistory();
            _view.Refresh(true);
            Say(L.T("Richtung") + $" {L.T(Motif.DirName(c))}: {_motif.Variant(c).Count} " + L.T("Teile") + (_motif.Has(c) ? " – " + L.T("ändern, markieren, wieder übernehmen") : " – " + L.T("noch leer: bauen oder 'Fehlende Richtungen automatisch'")));
        }

        private void ShowAll()
        {
            NewField();
            var at = new Dictionary<Compass, Point>
            {
                [Compass.North] = new Point(30, 6), [Compass.East] = new Point(44, 28), [Compass.South] = new Point(30, 46), [Compass.West] = new Point(8, 28)
            };
            foreach (Compass c in Dirs)
            {
                Put(_motif.Variant(c), at[c]);
            }
            _test.ClearHistory();
            _view.Refresh(true);
            Say(L.T("Alle vier Richtungen: oben Nord, rechts Ost, unten Süd, links West (so wie im Client: Nord = oben rechts)."));
        }

        private void TakeMarked()
        {
            List<PieceStatic> parts;
            if (_view.Selection.Count > 0 && _view.Area.IsEmpty)
            {
                parts = Motif.Capture(_test, Rectangle.Empty, 0, _view.Selection.ToList());
            }
            else
            {
                Rectangle r = _view.Area;
                if (r.IsEmpty)
                {
                    var st = _test.AllStatics().ToList();
                    if (st.Count == 0)
                    {
                        Say(L.T("Das Testfeld ist leer."));
                        return;
                    }
                    r = Rectangle.FromLTRB(st.Min(s => s.X), st.Min(s => s.Y), st.Max(s => s.X) + 1, st.Max(s => s.Y) + 1);
                }
                parts = Motif.Capture(_test, r, 0);
            }
            if (parts.Count == 0)
            {
                Say(L.T("Im markierten Bereich sind keine Statics."));
                return;
            }
            _motif.SetVariant(_building, parts);
            FillTree(_motif.ToString());
            Say(L.T("Übernommen als Richtung") + $" {L.T(Motif.DirName(_building))}: {parts.Count} " + L.T("Teile") + $", " + L.T("Höhe") + $" {_motif.HeightOf(_building)}" +
                (_motif.StairRise(_building) > 0 ? $", " + L.T("Treppen erkannt") + $" ({_motif.StairRise(_building)} Z)" : string.Empty) + " – " + L.T("fehlende Richtungen: 'automatisch' oder selbst bauen"));
        }

        private void AutoTurn(bool overwrite)
        {
            int n = _motif.AutoVariants(_motif.Has(_building) ? _building : (Compass?)null, overwrite);
            FillTree(_motif.ToString());
            ShowAll();
            Say(n + " " + L.T("Richtungen gedreht (Stufen nach ihrer Neigung, Wände nach ihrem Bild, Blöcke bleiben) – falsche Grafik? Richtung wählen, von Hand ändern, wieder übernehmen."));
        }

        private void TryDrag()
        {
            if (!_motif.Has(Compass.East) && !_motif.Has(Compass.North) && !_motif.Has(Compass.South) && !_motif.Has(Compass.West))
            {
                Say(L.T("Erst eine Richtung übernehmen."));
                return;
            }
            NewField();
            var ground = new Func<int, int, int>((x, y) => 0);
            int total = 0;
            foreach ((Point a, Point b) in new[] { (new Point(6, 10), new Point(40, 10)), (new Point(56, 20), new Point(56, 58)), (new Point(54, 60), new Point(14, 60)), (new Point(4, 56), new Point(4, 22)) })
            {
                List<(int x, int y, int z, ushort id, ushort hue)> parts = _motif.Layout(a, b, _draw, ground, out _, out _);
                _test.Apply(parts.Where(p => _test.InBounds(p.x, p.y)).Select(p => MapData.Add(new StaticItem { Id = p.id, Hue = p.hue, X = p.x, Y = p.y, Z = p.z })).ToList());
                total += parts.Count;
            }
            _test.ClearHistory();
            _view.Refresh(true);
            Say(L.T("Ziehen getestet: nach Ost (oben), Süd (rechts), West (unten), Nord (links)") + $" – {total} Statics");
        }

        private void FillTree(string select)
        {
            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            var all = Motif.LoadAll();
            if (!all.Any(m => m.ToString() == _motif.ToString()) && (_motif.North.Count + _motif.East.Count + _motif.South.Count + _motif.West.Count) > 0)
            {
                all.Add(_motif);
            }
            TreeNode sel = null;
            foreach (IGrouping<string, Motif> cat in all.GroupBy(m => m.Category).OrderBy(g => g.Key))
            {
                TreeNode cn = _tree.Nodes.Add(cat.Key);
                foreach (Motif m in cat.OrderBy(m => m.Name))
                {
                    Motif use = m.ToString() == _motif.ToString() ? _motif : m;
                    TreeNode mn = cn.Nodes.Add(m.Name);
                    mn.Tag = use;
                    foreach (Compass c in new[] { Compass.South, Compass.West, Compass.North, Compass.East })
                    {
                        TreeNode dn = mn.Nodes.Add((use.Has(c) ? "✔ " : "· ") + L.T(Motif.DirName(c)) + $" ({use.Variant(c).Count})");
                        dn.Tag = (use, c);
                    }
                    if (use.ToString() == select)
                    {
                        sel = mn;
                    }
                }
            }
            _tree.ExpandAll();
            _tree.EndUpdate();
            if (sel != null)
            {
                _tree.SelectedNode = sel;
            }
        }

        private void SaveMotif()
        {
            if (!Dirs.Any(_motif.Has))
            {
                Say(L.T("Erst eine Richtung übernehmen."));
                return;
            }
            string cat = Microsoft.VisualBasic.Interaction.InputBox(L.T("Kategorie (Ordner), z. B. Burg, Zelte, Mauern, Häuser:"), Text, _motif.Category);
            if (string.IsNullOrWhiteSpace(cat))
            {
                return;
            }
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name des Musters (z. B. Zinnen, Mauer mit Fenster):"), Text, _motif.Name);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            _motif.Category = cat.Trim();
            _motif.Name = name.Trim();
            if (Dirs.Any(c => !_motif.Has(c)) && MessageBox.Show(this, L.T("Nicht alle vier Richtungen sind gebaut. Die fehlenden automatisch drehen?"), Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _motif.AutoVariants();
            }
            string path = _motif.Save();
            _grid.SelectedObject = _motif;
            FillTree(_motif.ToString());
            Say(L.T("Gespeichert") + ": " + path + " – " + string.Join(", ", Dirs.Select(c => L.T(Motif.DirName(c)) + (_motif.Has(c) ? " ✔" : " –"))));
        }

        // ================================================================== user interface

        private void BuildUi()
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 345 };
            var left = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(6), BackColor = Color.FromArgb(241, 245, 249) };
            split.Panel1.Controls.Add(left);
            var right = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 560 };

            // ---- tool bars (like the stair designer)
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
            Tool("Wand / Zaun", IsoTool.Wall);
            tools.Items.Add(new ToolStripSeparator());
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
            tools.Items.Add(new ToolStripControlHost(z));
            _zStep = z;
            _pieceHeight = new ToolStripLabel("h –");
            tools.Items.Add(_pieceHeight);
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
            var moveBar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            moveBar.Items.Add(new ToolStripLabel(L.T("Auswahl verschieben:")));
            void Move(int dx, int dy, int dz, bool byHeight)
            {
                if (_view.Selection.Count == 0)
                {
                    Say(L.T("Erst Teile mit 'Auswahl' wählen (Strg = mehrere)."));
                    return;
                }
                int step = byHeight ? Math.Max(1, _view.Selection.Max(s2 => Math.Max(1, UoArtCache.StackHeight(s2.Id)))) : 1;
                _view.MoveSelection(dx, dy, dz * step, false);
            }
            foreach ((string text, int dx, int dy, int dz, bool h) in new[]
                     {
                         ("▲ N", 0, -1, 0, false), ("▼ S", 0, 1, 0, false), ("◀ W", -1, 0, 0, false), ("▶ O", 1, 0, 0, false), ("z ▲ h", 0, 0, 1, true), ("z ▼ h", 0, 0, -1, true), ("z +1", 0, 0, 1, false), ("z −1", 0, 0, -1, false)
                     })
            {
                var mb = new ToolStripButton(text);
                mb.Click += (s, e) => Move(dx, dy, dz, h);
                moveBar.Items.Add(mb);
            }
            var copy = new ToolStripButton(L.T("Kopieren"));
            copy.Click += (s, e) => _view.CopySelection();
            var del = new ToolStripButton("✖ " + L.T("Auswahl löschen"));
            del.Click += (s, e) => _view.DeleteSelection();
            moveBar.Items.AddRange(new ToolStripItem[] { copy, del });
            right.Panel1.Controls.Add(_view);
            right.Panel1.Controls.Add(moveBar);
            right.Panel1.Controls.Add(tools);
            right.Panel2.Controls.Add(_catalog);
            right.Panel2.Controls.Add(new Label
            {
                Dock = DockStyle.Top, Height = 18, Text = "  " + L.T("KATALOG – Klick = zum Setzen im Testfeld · Rechtsklick = Kategorien"),
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
                    int ih = UoArtCache.ItemHeight(e.Id), sh = UoArtCache.StackHeight(e.Id);
                    _pieceHeight.Text = sh != ih ? $"h {ih} ({L.T("stapelt")} {sh})" : $"h {ih}";
                    if (sh > 0)
                    {
                        _zStep.Increment = sh;
                    }
                }
            };

            // ---- left side
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

            left.Controls.Add(Head("1 Muster (Kategorie → Name → Richtung)", Color.FromArgb(68, 64, 60)));
            _tree.AfterSelect += (s, e) =>
            {
                if (e.Node?.Tag is Motif m)
                {
                    _motif = m;
                    _grid.SelectedObject = _motif;
                    ShowAll();
                }
                else if (e.Node?.Tag is ValueTuple<Motif, Compass> mc)
                {
                    _motif = mc.Item1;
                    _grid.SelectedObject = _motif;
                    ShowVariant(mc.Item2);
                }
            };
            left.Controls.Add(_tree);
            var ml = Line();
            ml.Controls.Add(B("Neu", () =>
            {
                _motif = new Motif();
                _grid.SelectedObject = _motif;
                NewField();
                FillTree(null);
                Say(L.T("Neues Muster: Richtung wählen und bauen."));
            }, 70));
            ml.Controls.Add(B("Speichern …", SaveMotif, 90));
            ml.Controls.Add(B("Löschen", () =>
            {
                if (MessageBox.Show(this, L.T("Muster löschen?") + $" {_motif}", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _motif.Delete();
                    _motif = new Motif();
                    _grid.SelectedObject = _motif;
                    FillTree(null);
                }
            }, 70));
            ml.Controls.Add(B("Hilfe", () => _host.Help?.Invoke("motif"), 60));
            left.Controls.Add(ml);
            _grid = new PropertyGrid { Width = 310, Height = 110, ToolbarVisible = false, HelpVisible = false, PropertySort = PropertySort.NoSort, SelectedObject = _motif };
            left.Controls.Add(_grid);

            left.Controls.Add(Head("2 Richtung, die ich baue", Color.FromArgb(180, 83, 9)));
            var dl = Line();
            foreach (Compass c in Dirs)
            {
                var r = new RadioButton { Text = L.T(Motif.DirName(c)), AutoSize = true, Checked = c == _building, Margin = new Padding(6, 4, 6, 0) };
                Compass cc = c;
                r.CheckedChanged += (s, e) =>
                {
                    if (r.Checked && _building != cc)
                    {
                        ShowVariant(cc);
                    }
                };
                _dirButtons[c] = r;
                dl.Controls.Add(r);
            }
            left.Controls.Add(dl);
            left.Controls.Add(new Label
            {
                AutoSize = false, Width = 310, Height = 34, ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Richtung = in diese Richtung läuft das Muster beim Ziehen (Ost: eine Mauer West → Ost, die Zinnen zeigen nach Süden …).")
            });
            var tl = Line();
            tl.Controls.Add(B("✔ Markiertes als diese Richtung übernehmen", TakeMarked, 300));
            tl.Controls.Add(B("Fehlende Richtungen automatisch", () => AutoTurn(false), 200));
            tl.Controls.Add(B("Alle neu drehen", () => AutoTurn(true), 96));
            tl.Controls.Add(B("Alle 4 ansehen", ShowAll, 148));
            tl.Controls.Add(B("Testfeld leeren", NewField, 148));
            left.Controls.Add(tl);

            left.Controls.Add(Head("3 Ziehen testen", Color.FromArgb(37, 99, 235)));
            left.Controls.Add(new PropertyGrid { Width = 310, Height = 250, ToolbarVisible = false, HelpVisible = true, PropertySort = PropertySort.Categorized, SelectedObject = _draw });
            left.Controls.Add(B("▶ Probe ziehen (alle 4 Richtungen)", TryDrag, 300));

            left.Controls.Add(Head("4 Log und Studio", Color.FromArgb(22, 128, 61)));
            var ul = Line();
            ul.Controls.Add(B("📋 Log + Bild", () => DesignLog.Export(this, _view, _test, _view.Area, "Muster", _motif.Describe()), 148));
            ul.Controls.Add(B("✔ Im Studio benutzen", () =>
            {
                if (!Dirs.Any(_motif.Has))
                {
                    Say(L.T("Erst eine Richtung übernehmen."));
                    return;
                }
                if (Dirs.Any(c => !_motif.Has(c)))
                {
                    _motif.AutoVariants();
                }
                _motif.Save();
                FillTree(_motif.ToString());
                _host.Brushes.MotifDraw = JsonSerializer.Deserialize<MotifDrawSettings>(JsonSerializer.Serialize(_draw));
                _host.Use?.Invoke(_motif, _building);
                Say(L.T("Muster ist im Werkzeug 'Muster' (CGI) und im Karten-Editor – klicken = einmal, ziehen = wiederholen."));
            }, 148));
            ul.Controls.Add(B("Schließen", Close, 98));
            left.Controls.Add(ul);

            Controls.Add(split);
            Controls.Add(_status);
        }
    }
}
