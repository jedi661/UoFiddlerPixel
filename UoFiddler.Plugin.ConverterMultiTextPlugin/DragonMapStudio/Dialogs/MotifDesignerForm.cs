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
        public Func<MapData> CurrentMap;
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
        private readonly Dictionary<Compass, CheckBox> _allowBoxes = new Dictionary<Compass, CheckBox>();
        private bool _allowSync;
        private readonly MotifDrawSettings _draw = new MotifDrawSettings { Drag = true };
        private MapData _test;
        private Motif _motif = new Motif();
        private Compass _building = Compass.East;
        private MotifPart _part = MotifPart.Middle;
        private readonly Dictionary<MotifPart, RadioButton> _partButtons = new Dictionary<MotifPart, RadioButton>();
        private readonly ListBox _alts = new ListBox { Width = 310, Height = 84, IntegralHeight = false };

        /// <summary>Tag of a tree node: one piece slot of a motif.</summary>
        private sealed class SlotTag
        {
            public Motif M;
            public MotifPart P;
            public Compass C;
        }
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
            List<PieceStatic> v = _motif.Piece(_part, c);
            Put(v, new Point(Size0 / 2 - 4, Size0 / 2 - 4));
            _test.ClearHistory();
            _view.Refresh(true);
            Say(L.T(Motif.SlotName(_part, c)) + $": {v.Count} " + L.T("Teile") + (v.Count > 0 ? " – " + L.T("ändern, markieren, wieder übernehmen") : " – " + L.T("noch leer: bauen oder 'Fehlende Richtungen automatisch'")));
        }

        private void ShowAll()
        {
            NewField();
            var at = new Dictionary<Compass, Point>
            {
                [Compass.North] = new Point(30, 6), [Compass.East] = new Point(44, 28), [Compass.South] = new Point(30, 46), [Compass.West] = new Point(8, 28)
            };
            foreach (Compass c in Dirs.Where(_motif.Allowed))
            {
                Put(_motif.Variant(c), at[c]);
            }
            _test.ClearHistory();
            _view.Refresh(true);
            Say(L.T("Alle vier Richtungen: oben Nord, rechts Ost, unten Süd, links West (so wie im Client: Nord = oben rechts).") + " " + L.T("Erlaubt") + ": " + string.Join(", ", Dirs.Where(_motif.Allowed).Select(c => L.T(Motif.DirName(c)))));
        }

        /// <summary>The marked statics of the test field (area, selection, or everything).</summary>
        private List<PieceStatic> CaptureMarked()
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
                        return null;
                    }
                    r = Rectangle.FromLTRB(st.Min(s => s.X), st.Min(s => s.Y), st.Max(s => s.X) + 1, st.Max(s => s.Y) + 1);
                }
                parts = Motif.Capture(_test, r, 0);
            }
            if (parts.Count == 0)
            {
                Say(L.T("Im markierten Bereich sind keine Statics."));
                return null;
            }
            return parts;
        }

        private void TakeMarked()
        {
            List<PieceStatic> parts = CaptureMarked();
            if (parts == null)
            {
                return;
            }
            int axis = Motif.AxisOf(parts);
            if (_part != MotifPart.Corner && axis != 0 && axis != Motif.AxisOf(_building))
            {
                Compass fits = Staircase.RightOf(_building);
                string runs = axis > 0 ? L.T("West → Ost") : L.T("Nord → Süd");
                DialogResult r = MessageBox.Show(this,
                    L.T("Das Markierte läuft") + " " + runs + ", " + L.T("die Richtung") + " " + L.T(Motif.DirName(_building)) + " " + L.T("läuft aber quer dazu.") + "\n\n" +
                    L.T("Ja = als Richtung") + " " + L.T(Motif.DirName(fits)) + " " + L.T("übernehmen (beim Ziehen wird die Mauer länger)") + "\n" +
                    L.T("Nein = trotzdem als") + " " + L.T(Motif.DirName(_building)) + " " + L.T("(z. B. Zelte, Reihe von Kopien nebeneinander)"),
                    Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel)
                {
                    return;
                }
                if (r == DialogResult.Yes)
                {
                    _building = fits;
                    if (!_motif.Allowed(fits))
                    {
                        _motif.SetAllowed(fits, true);
                        SyncAllow();
                    }
                    if (_dirButtons.TryGetValue(fits, out RadioButton rb))
                    {
                        _allowSync = true;
                        rb.Checked = true;
                        _allowSync = false;
                    }
                }
            }
            _motif.SetPiece(_part, _building, parts);
            FillTree(_motif.ToString());
            if (_part != MotifPart.Middle)
            {
                Say(L.T("Übernommen als") + " " + L.T(Motif.SlotName(_part, _building)) + $": {parts.Count} " + L.T("Teile") + " – " + L.T("die anderen Richtungen dreht das Studio selbst"));
                return;
            }
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

        private bool HasAnything => Dirs.Any(_motif.Has) || (_motif.Alternatives?.Any(a => a.Parts.Count > 0) ?? false);

        private void PutLayout(List<(int x, int y, int z, ushort id, ushort hue)> parts) =>
            _test.Apply(parts.Where(p => _test.InBounds(p.x, p.y)).Select(p => MapData.Add(new StaticItem { Id = p.id, Hue = p.hue, X = p.x, Y = p.y, Z = p.z })).ToList());

        /// <summary>Test drawing in the field – as the chosen "Zeichen-Art" (line: 4 drags, points: a square with corners, radius).</summary>
        private void TryDrag()
        {
            if (!HasAnything)
            {
                Say(L.T("Erst eine Richtung übernehmen."));
                return;
            }
            NewField();
            var ground = new Func<int, int, int>((x, y) => 0);
            int total = 0;
            string what;
            if (_draw.Mode == MotifMode.Radius)
            {
                List<(int x, int y, int z, ushort id, ushort hue)> parts = _motif.Layout(new Point(32, 32), new Point(52, 32), _draw, ground, out _, out int n);
                PutLayout(parts);
                total = parts.Count;
                what = L.T("Im Radius 20 gestreut") + $": {n} " + L.T("Stücke");
            }
            else if (_draw.Mode == MotifMode.Points)
            {
                List<(int x, int y, int z, ushort id, ushort hue)> parts = _motif.Path(new[] { new Point(8, 8), new Point(54, 8), new Point(54, 54), new Point(8, 54), new Point(8, 14) }, _draw, ground, out int n);
                PutLayout(parts);
                total = parts.Count;
                what = L.T("Punkt zu Punkt (Viereck mit Ecken)") + $": {n} " + L.T("Stücke");
            }
            else
            {
                foreach ((Point a, Point b) in new[] { (new Point(6, 10), new Point(40, 10)), (new Point(56, 20), new Point(56, 58)), (new Point(54, 60), new Point(14, 60)), (new Point(4, 56), new Point(4, 22)) })
                {
                    List<(int x, int y, int z, ushort id, ushort hue)> parts = _motif.Layout(a, b, _draw, ground, out _, out _);
                    PutLayout(parts);
                    total += parts.Count;
                }
                what = L.T("Ziehen getestet: nach Ost (oben), Süd (rechts), West (unten), Nord (links)");
            }
            _draw.Seed = Environment.TickCount & 0x7fffffff;
            _test.ClearHistory();
            _view.Refresh(true);
            Say(what + $" – {total} Statics");
        }

        private void RefreshAlts()
        {
            _alts.BeginUpdate();
            _alts.Items.Clear();
            foreach (MotifAlt a in _motif.Alternatives ?? new List<MotifAlt>())
            {
                _alts.Items.Add(a);
            }
            _alts.EndUpdate();
        }

        private void AddAlt()
        {
            List<PieceStatic> parts = CaptureMarked();
            if (parts == null)
            {
                return;
            }
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name der Variante (z. B. Eiche, Busch, Mauer ohne Fenster):"), Text, L.T("Variante") + " " + (_motif.Alternatives.Count + 1));
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            string w = Microsoft.VisualBasic.Interaction.InputBox(L.T("Gewicht (wie oft im Vergleich, z. B. 1, 2, 5):"), Text, "1");
            _motif.Alternatives.Add(new MotifAlt { Name = name.Trim(), Weight = int.TryParse(w, out int wi) ? Math.Max(0, wi) : 1, Built = _building, Parts = parts });
            RefreshAlts();
            FillTree(_motif.ToString());
            Say(L.T("Zufalls-Variante hinzugefügt") + $": {name} ({parts.Count} " + L.T("Teile") + ") – " + L.T("Zeichen-Art 'Im Radius streuen' oder 'Mitte zufällig' benutzt sie."));
        }

        /// <summary>From the CGI view ("→ Muster"): fetch the marked area.</summary>
        public void FetchFromMap()
        {
            Activate();
            FromMap();
        }

        /// <summary>Copies the statics of the area marked in the CGI view into the test field (heights relative to the lowest land there).</summary>
        private void FromMap()
        {
            MapData map = _host.CurrentMap?.Invoke() ?? _host.Map;
            Rectangle r = _host.CurrentArea?.Invoke() ?? Rectangle.Empty;
            if (map == null || r.IsEmpty)
            {
                Say(L.T("Erst in der CGI-Ansicht mit 'Bereich' etwas markieren (z. B. ein Stück Burgmauer), dann hier 'Aus der Karte holen'."));
                return;
            }
            r.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (r.Width > Size0 - 4 || r.Height > Size0 - 4)
            {
                Say(L.T("Der Bereich ist zu groß für das Testfeld") + $" (max {Size0 - 4}×{Size0 - 4}).");
                return;
            }
            int ground = int.MaxValue;
            for (int y = r.Top; y < r.Bottom; y++)
            {
                for (int x = r.Left; x < r.Right; x++)
                {
                    ground = Math.Min(ground, map.LandZ(x, y));
                }
            }
            List<PieceStatic> parts = Motif.Capture(map, r, ground);
            if (parts.Count == 0)
            {
                Say(L.T("Im markierten Bereich sind keine Statics."));
                return;
            }
            NewField();
            Put(parts, new Point((Size0 - r.Width) / 2, (Size0 - r.Height) / 2));
            _test.ClearHistory();
            _view.Refresh(true);
            Say(parts.Count + " " + L.T("Statics aus der Karte geholt – Teil und Richtung wählen, markieren, übernehmen."));
        }

        private void ExportMotifs()
        {
            using var d = new SaveFileDialog { Filter = "ZIP (alle Muster)|*.zip|JSON (dieses Muster)|*.json", FileName = "Muster.zip", Title = L.T("Muster exportieren") };
            if (d.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            List<Motif> list = d.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? new List<Motif> { _motif } : Motif.LoadAll();
            Motif.Export(list, d.FileName);
            Say(list.Count + " " + L.T("Muster exportiert") + ": " + d.FileName);
        }

        private void ImportMotifs()
        {
            using var d = new OpenFileDialog { Filter = "Muster (*.zip;*.json)|*.zip;*.json", Multiselect = true };
            if (d.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            int n = 0;
            foreach (string f in d.FileNames)
            {
                try
                {
                    n += Motif.Import(f).Count;
                }
                catch (Exception ex)
                {
                    Say(L.T("Fehler beim Import") + ": " + ex.Message);
                }
            }
            FillTree(_motif.ToString());
            Say(n + " " + L.T("Muster importiert."));
        }

        private void ExportToProject()
        {
            using var d = new FolderBrowserDialog
            {
                Description = L.T("Ordner 'DragonData\\Muster' im Projekt wählen (UoFiddler.Plugin.ConverterMultiTextPlugin\\DragonMapStudio\\DragonData\\Muster) – die Muster werden dann mit ausgeliefert."),
                UseDescriptionForTitle = true
            };
            if (Motif.ShippedFolder != null)
            {
                d.SelectedPath = Motif.ShippedFolder;
            }
            if (d.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            var choice = MessageBox.Show(this, L.T("Alle Muster ins Projekt schreiben? (Nein = nur dieses Muster)"), Text, MessageBoxButtons.YesNoCancel);
            if (choice == DialogResult.Cancel)
            {
                return;
            }
            List<Motif> list = choice == DialogResult.Yes ? Motif.LoadAll() : new List<Motif> { _motif };
            int n = Motif.ExportToFolder(list, d.SelectedPath);
            Say(n + " " + L.T("Muster ins Projekt geschrieben") + ": " + d.SelectedPath + " (" + L.T("Kategorie-Ordner / Name.json") + ")");
        }

        private void FillTree(string select)
        {
            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            var all = Motif.LoadAll();
            if (!all.Any(m => m.ToString() == _motif.ToString()) && HasAnything)
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
                    TreeNode mn = cn.Nodes.Add(m.Name + (m.Shipped ? " (" + L.T("Projekt") + ")" : string.Empty));
                    mn.Tag = use;
                    foreach (Compass c in new[] { Compass.South, Compass.West, Compass.North, Compass.East })
                    {
                        TreeNode dn = mn.Nodes.Add((!use.Allowed(c) ? "✖ " : use.Has(c) ? "✔ " : "· ") + L.T(Motif.DirName(c)) + (use.Allowed(c) ? $" ({use.Variant(c).Count})" : " – " + L.T("nicht erlaubt")));
                        dn.Tag = new SlotTag { M = use, P = MotifPart.Middle, C = c };
                    }
                    foreach (MotifPart part in new[] { MotifPart.Start, MotifPart.End, MotifPart.Corner })
                    {
                        if (!use.HasPart(part))
                        {
                            continue;
                        }
                        TreeNode pn = mn.Nodes.Add(L.T(Motif.PartName(part)));
                        foreach (Compass c in Dirs)
                        {
                            List<PieceStatic> v = use.Piece(part, c);
                            TreeNode dn = pn.Nodes.Add((v.Count > 0 ? "✔ " : "· ") + L.T(Motif.SlotName(part, c)) + $" ({v.Count})");
                            dn.Tag = new SlotTag { M = use, P = part, C = c };
                        }
                    }
                    if (use.Alternatives?.Count > 0)
                    {
                        TreeNode an = mn.Nodes.Add(L.T("Zufalls-Varianten") + $" ({use.Alternatives.Count})");
                        foreach (MotifAlt a in use.Alternatives)
                        {
                            an.Nodes.Add(a.ToString()).Tag = a;
                        }
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
            if (!HasAnything)
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
            if (Dirs.Any(c => _motif.Allowed(c) && !_motif.Has(c)) && MessageBox.Show(this, L.T("Nicht alle erlaubten Richtungen sind gebaut. Die fehlenden automatisch drehen?"), Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _motif.AutoVariants();
            }
            string path = _motif.Save();
            _grid.SelectedObject = _motif;
            FillTree(_motif.ToString());
            Say(L.T("Gespeichert") + ": " + path + " – " + string.Join(", ", Dirs.Select(c => L.T(Motif.DirName(c)) + (_motif.Has(c) ? " ✔" : " –"))));
        }

        /// <summary>Which piece is built (middle, start, end, corner) – the direction buttons show the corners for "Ecke".</summary>
        private void SetPart(MotifPart p)
        {
            _part = p;
            _allowSync = true;
            if (_partButtons.TryGetValue(p, out RadioButton pr))
            {
                pr.Checked = true;
            }
            foreach (KeyValuePair<Compass, RadioButton> kv in _dirButtons)
            {
                kv.Value.Text = L.T(p == MotifPart.Corner ? Motif.CornerName(kv.Key) : Motif.DirName(kv.Key));
            }
            _allowSync = false;
        }

        /// <summary>Check boxes of the allowed directions = the motif.</summary>
        private void SyncAllow()
        {
            _allowSync = true;
            foreach (KeyValuePair<Compass, CheckBox> kv in _allowBoxes)
            {
                kv.Value.Checked = _motif.Allowed(kv.Key);
            }
            foreach (KeyValuePair<Compass, RadioButton> kv in _dirButtons)
            {
                kv.Value.ForeColor = _motif.Allowed(kv.Key) ? SystemColors.ControlText : Color.FromArgb(148, 163, 184);
            }
            _allowSync = false;
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
                    SyncAllow();
                    RefreshAlts();
                    _grid.SelectedObject = _motif;
                    ShowAll();
                }
                else if (e.Node?.Tag is SlotTag st)
                {
                    _motif = st.M;
                    SyncAllow();
                    RefreshAlts();
                    _grid.SelectedObject = _motif;
                    SetPart(st.P);
                    ShowVariant(st.C);
                }
                else if (e.Node?.Tag is MotifAlt alt)
                {
                    NewField();
                    Put(alt.Parts, new Point(Size0 / 2 - 4, Size0 / 2 - 4));
                    _test.ClearHistory();
                    _view.Refresh(true);
                    Say(L.T("Zufalls-Variante") + $" {alt}");
                }
            };
            left.Controls.Add(_tree);
            var ml = Line();
            ml.Controls.Add(B("Neu", () =>
            {
                _motif = new Motif();
                SyncAllow();
                RefreshAlts();
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
                    if (!_motif.Delete() && _motif.Shipped)
                    {
                        Say(L.T("Dieses Muster kommt aus dem Projekt (DragonData\\Muster) – dort löschen."));
                        return;
                    }
                    _motif = new Motif();
                    RefreshAlts();
                    _grid.SelectedObject = _motif;
                    FillTree(null);
                }
            }, 70));
            ml.Controls.Add(B("Hilfe", () => _host.Help?.Invoke("motif"), 60));
            left.Controls.Add(ml);
            _grid = new PropertyGrid { Width = 310, Height = 110, ToolbarVisible = false, HelpVisible = false, PropertySort = PropertySort.NoSort, SelectedObject = _motif };
            left.Controls.Add(_grid);

            left.Controls.Add(Head("2 Teil und Richtung, die ich baue", Color.FromArgb(180, 83, 9)));
            var pl = Line();
            foreach (MotifPart part in new[] { MotifPart.Middle, MotifPart.Start, MotifPart.End, MotifPart.Corner })
            {
                var r = new RadioButton { Text = L.T(Motif.PartName(part)), AutoSize = true, Checked = part == _part, Margin = new Padding(6, 4, 6, 0) };
                MotifPart pp = part;
                r.CheckedChanged += (s, e) =>
                {
                    if (r.Checked && _part != pp && !_allowSync)
                    {
                        SetPart(pp);
                        ShowVariant(_building);
                    }
                };
                _partButtons[part] = r;
                pl.Controls.Add(r);
            }
            left.Controls.Add(pl);
            left.Controls.Add(new Label
            {
                AutoSize = false, Width = 310, Height = 46, ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Mitte = wiederholt sich. Anfang / Ende = einmal an den Enden der Reihe (z. B. Türme). Ecke = an jedem Knick bei 'Punkt zu Punkt' (ein Feld, das Knick-Feld).")
            });
            var dl = Line();
            foreach (Compass c in Dirs)
            {
                var r = new RadioButton { Text = L.T(Motif.DirName(c)), AutoSize = true, Checked = c == _building, Margin = new Padding(6, 4, 6, 0) };
                Compass cc = c;
                r.CheckedChanged += (s, e) =>
                {
                    if (r.Checked && _building != cc && !_allowSync)
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
                AutoSize = false, Width = 310, Height = 48, ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Richtung = in diese Richtung läuft das Muster beim Ziehen. Ost / West: die Mauer läuft West → Ost (auf dem Bild „\\“), Nord / Süd: Nord → Süd („/“).")
            });
            left.Controls.Add(new Label { Text = L.T("Erlaubte Richtungen (nur diese werden gedreht und gezeichnet):"), AutoSize = false, Width = 310, Height = 18 });
            var al = Line();
            foreach (Compass c in Dirs)
            {
                var cb = new CheckBox { Text = L.T(Motif.DirName(c)), AutoSize = true, Checked = true, Margin = new Padding(6, 2, 6, 0) };
                Compass cc = c;
                cb.CheckedChanged += (s, e) =>
                {
                    if (_allowSync)
                    {
                        return;
                    }
                    _motif.SetAllowed(cc, cb.Checked);
                    SyncAllow();
                    FillTree(_motif.ToString());
                    Say(L.T("Erlaubt") + ": " + string.Join(", ", Dirs.Where(_motif.Allowed).Select(d => L.T(Motif.DirName(d)))) + " – " + L.T("speichern nicht vergessen"));
                };
                _allowBoxes[c] = cb;
                al.Controls.Add(cb);
            }
            void Only(params Compass[] cs)
            {
                foreach (Compass c in Dirs)
                {
                    _motif.SetAllowed(c, true);
                }
                foreach (Compass c in Dirs.Where(d => !cs.Contains(d)))
                {
                    _motif.SetAllowed(c, false);
                }
                SyncAllow();
                FillTree(_motif.ToString());
                Say(L.T("Erlaubt") + ": " + string.Join(", ", Dirs.Where(_motif.Allowed).Select(d => L.T(Motif.DirName(d)))) + " – " + L.T("speichern nicht vergessen"));
            }
            al.Controls.Add(B("alle", () => Only(Dirs), 50));
            al.Controls.Add(B("nur N–S", () => Only(Compass.North, Compass.South), 70));
            al.Controls.Add(B("nur O–W", () => Only(Compass.East, Compass.West), 70));
            al.Controls.Add(B("nur diese", () => Only(_building), 80));
            left.Controls.Add(al);
            var tl = Line();
            tl.Controls.Add(B("✔ Markiertes als diese Richtung übernehmen", TakeMarked, 300));
            tl.Controls.Add(B("Fehlende Richtungen automatisch", () => AutoTurn(false), 200));
            tl.Controls.Add(B("Alle neu drehen", () => AutoTurn(true), 96));
            tl.Controls.Add(B("Alle 4 ansehen", ShowAll, 148));
            tl.Controls.Add(B("Testfeld leeren", NewField, 148));
            left.Controls.Add(tl);

            left.Controls.Add(Head("Zufalls-Varianten (Bäume, Pflanzen, Mauer-Abwechslung)", Color.FromArgb(21, 128, 61)));
            left.Controls.Add(_alts);
            var vl = Line();
            vl.Controls.Add(B("+ Markiertes als Variante", AddAlt, 170));
            vl.Controls.Add(B("Gewicht …", () =>
            {
                if (_alts.SelectedItem is MotifAlt a)
                {
                    string w = Microsoft.VisualBasic.Interaction.InputBox(L.T("Gewicht (wie oft im Vergleich, z. B. 1, 2, 5):"), Text, a.Weight.ToString());
                    if (int.TryParse(w, out int wi))
                    {
                        a.Weight = Math.Max(0, wi);
                        RefreshAlts();
                        FillTree(_motif.ToString());
                    }
                }
            }, 80));
            vl.Controls.Add(B("Ansehen", () =>
            {
                if (_alts.SelectedItem is MotifAlt a)
                {
                    NewField();
                    Put(a.Parts, new Point(Size0 / 2 - 4, Size0 / 2 - 4));
                    _test.ClearHistory();
                    _view.Refresh(true);
                }
            }, 70));
            vl.Controls.Add(B("Entfernen", () =>
            {
                if (_alts.SelectedItem is MotifAlt a)
                {
                    _motif.Alternatives.Remove(a);
                    RefreshAlts();
                    FillTree(_motif.ToString());
                }
            }, 80));
            left.Controls.Add(vl);

            left.Controls.Add(Head("Karte, Austausch, Richtungen", Color.FromArgb(87, 83, 78)));
            var xl = Line();
            xl.Controls.Add(B("⇩ Aus der Karte holen", FromMap, 150));
            xl.Controls.Add(B("Richtungen festlegen …", () => DirectionGroupsForm.ShowShared(this), 150));
            xl.Controls.Add(B("Exportieren …", ExportMotifs, 100));
            xl.Controls.Add(B("Importieren …", ImportMotifs, 100));
            xl.Controls.Add(B("Ins Projekt …", ExportToProject, 100));
            left.Controls.Add(xl);

            left.Controls.Add(Head("3 Ziehen testen", Color.FromArgb(37, 99, 235)));
            left.Controls.Add(new PropertyGrid { Width = 310, Height = 250, ToolbarVisible = false, HelpVisible = true, PropertySort = PropertySort.Categorized, SelectedObject = _draw });
            left.Controls.Add(B("▶ Probe zeichnen (wie Zeichen-Art)", TryDrag, 300));

            left.Controls.Add(Head("4 Log und Studio", Color.FromArgb(22, 128, 61)));
            var ul = Line();
            ul.Controls.Add(B("📋 Log + Bild", () => DesignLog.Export(this, _view, _test, _view.Area, "Muster", _motif.Describe()), 148));
            ul.Controls.Add(B("✔ Im Studio benutzen", () =>
            {
                if (!HasAnything)
                {
                    Say(L.T("Erst eine Richtung übernehmen."));
                    return;
                }
                if (Dirs.Any(c => _motif.Allowed(c) && !_motif.Has(c)))
                {
                    _motif.AutoVariants();
                }
                if (!_motif.Allowed(_building))
                {
                    _building = _motif.NextAllowed(_building);
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
