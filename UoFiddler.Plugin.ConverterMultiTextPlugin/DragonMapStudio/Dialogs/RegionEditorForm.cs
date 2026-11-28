/***************************************************************************
 *
 *  * Dragon Map Studio – region window ("Bereich bearbeiten"): a marked area of the CGI map is copied and edited in
 *  *  its own window with ALL tools of the CGI view (the whole CGI work surface moves into this window). "Fertig"
 *  *  writes only what changed back to the same place of the big map (one undo step there), "Abbrechen" throws the
 *  *  copy away.
 *  *
 *  *  Next to the work surface (side by side, every pane can be detached to another monitor):
 *  *    – the other level: the cellar / under the map while working on the surface, or the house above while
 *  *      building below ("Unter der Karte bauen")
 *  *    – the buildings of the area with pictures: choose, move, turn, as template, level the ground, delete,
 *  *      "Stadt neu ordnen" (rearrange in rows with streets)
 *  *  plus a quick menu for building out (ground, forest, river, road, bridge, mountain, cave, cellar, dungeon …),
 *  *  coordinate lines and a tile info at the mouse.
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
    public sealed class RegionHost
    {
        public IsoMapView MainView;
        public Func<MapData> Map;
        public Point Origin;
        public Action<IsoMapView> Configure;
        public Action<IsoTool> SelectTool;
        public Action<bool> SetUnder;
        public Action<bool> Finish;
        public Action LevelGround;
        public Action<MapTemplate> SaveTemplate;
        public Action<string> Status;
        public Action<string> Help;
        public Action TogglePalette;
        public Action ToggleSide;
        public Action UndockSide;
        public Action ToggleOptions;
    }

    public sealed class RegionEditorForm : Form
    {
        private readonly RegionHost _host;
        private readonly SplitContainer _split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
        private readonly SplitContainer _side = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
        private readonly IsoMapView _level = new IsoMapView { Dock = DockStyle.Fill };
        private readonly ListView _list = new ListView { Dock = DockStyle.Fill, View = View.LargeIcon, HideSelection = false, MultiSelect = true };
        private readonly ImageList _images = new ImageList { ImageSize = new Size(96, 96), ColorDepth = ColorDepth.Depth32Bit };
        private readonly Label _levelTitle = new Label { Dock = DockStyle.Top, Height = 22, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.White, BackColor = Color.FromArgb(126, 34, 206) };
        private readonly Timer _sync = new Timer { Interval = 400 };
        private readonly TownLayoutSettings _town = new TownLayoutSettings();
        private List<TownBuilding> _buildings = new List<TownBuilding>();
        private bool _finishing;
        private bool _closingNow;
        private bool _followMain = true;

        /// <summary>The panel the CGI work surface is put into.</summary>
        public Panel WorkspaceHost { get; } = new Panel { Dock = DockStyle.Fill };

        public RegionEditorForm(RegionHost host)
        {
            _host = host;
            Text = L.T("Bereich bearbeiten") + " – Dragon Map Studio";
            Size = new Size(1700, 980);
            MinimumSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9f);
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (ActiveControl is TextBoxBase)
                {
                    return;
                }
                if (e.Control && e.KeyCode == Keys.Z)
                {
                    _host.MainView.Undo();
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.Y)
                {
                    _host.MainView.Redo();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F7)
                {
                    _host.TogglePalette?.Invoke();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.F8)
                {
                    _host.ToggleSide?.Invoke();
                    e.Handled = true;
                }
            };
            BuildUi();
            FormClosing += OnClosing;
            Shown += (s, e) =>
            {
                _split.SplitterDistance = Math.Max(400, ClientSize.Width - 760);
                _side.SplitterDistance = 380;
                SetupLevel();
                Detect();
            };
        }

        private MapData Map => _host.Map();

        private void Say(string t)
        {
            _status.Text = "  " + t;
            _host.Status?.Invoke(t);
        }

        /// <summary>Closes without asking (the studio has taken the work surface back).</summary>
        public void CloseQuiet()
        {
            _finishing = true;
            _sync.Stop();
            _host.MainView.ViewChanged -= MainViewChanged;
            _host.MainView.MapChanged -= MainMapChanged;
            if (!_closingNow)
            {
                Close();
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (_finishing)
            {
                return;
            }
            bool apply = false;
            if (e.CloseReason != CloseReason.FormOwnerClosing && e.CloseReason != CloseReason.ApplicationExitCall && e.CloseReason != CloseReason.WindowsShutDown &&
                Map != null && Map.UndoCount > 0)
            {
                DialogResult r = MessageBox.Show(this, L.T("Die Änderungen im Bereich in die Karte übernehmen?\n\nJa = zurückschreiben (Fertig)\nNein = verwerfen\nAbbrechen = weiter bearbeiten"),
                    Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                apply = r == DialogResult.Yes;
            }
            // the studio takes its work surface back before this window goes away
            _closingNow = true;
            _host.Finish(apply);
        }

        // ================================================================== layout

        private void BuildUi()
        {
            var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, ImageScalingSize = new Size(20, 20) };
            ToolStripButton Btn(string text, string tip, Action a, bool bold = false)
            {
                var b = new ToolStripButton(L.T(text)) { ToolTipText = L.T(tip) };
                if (bold)
                {
                    b.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                b.Click += (s, e) => a();
                bar.Items.Add(b);
                return b;
            }
            Btn("✔ Fertig – zurückschreiben", "Alles, was im Bereich geändert wurde, an derselben Stelle in die große Karte schreiben (ein Rückgängig-Schritt dort) und das Fenster schließen", () => _host.Finish(true), true);
            Btn("✖ Abbrechen", "Die Kopie verwerfen – die große Karte bleibt wie sie war", () => Close());
            bar.Items.Add(new ToolStripSeparator());

            // quick building out: the tools of the studio
            var quick = new ToolStripDropDownButton(L.T("Schnell ausbauen")) { ToolTipText = L.T("Werkzeug wählen – alle Einstellungen oben in der Leiste / im Optionen-Fenster") };
            foreach ((string text, IsoTool tool, string hint) in new[]
                     {
                         ("Boden füllen (Bereich + F)", IsoTool.Area, "Bereich aufziehen, F = mit dem Land-Tile füllen, T = Übergänge"),
                         ("Land-Pinsel / neuer Boden", IsoTool.LandSet, string.Empty),
                         ("Unebenheit", IsoTool.Noise, string.Empty),
                         ("Plateau / Terrassen", IsoTool.Plateau, string.Empty),
                         ("Berg", IsoTool.Mountain, string.Empty),
                         ("Bergwall / Labyrinth", IsoTool.MountainWall, string.Empty),
                         ("Wald / Pflanzen streuen", IsoTool.Scatter, string.Empty),
                         ("Fluss", IsoTool.River, string.Empty),
                         ("Fluss-Lauf", IsoTool.Course, string.Empty),
                         ("Weg / Straße", IsoTool.Road, string.Empty),
                         ("Brücke", IsoTool.Bridge, string.Empty),
                         ("Wand / Zaun", IsoTool.Wall, string.Empty),
                         ("Muster (Mauern, Zinnen …)", IsoTool.Motif, string.Empty),
                         ("Vorlage / Gebiets-Stempel", IsoTool.Template, string.Empty),
                         ("Static setzen", IsoTool.Place, string.Empty),
                         ("Multi (Haus)", IsoTool.Multi, string.Empty),
                         ("Treppe", IsoTool.Stairs, string.Empty),
                         ("Keller / unter der Karte", IsoTool.Cellar, "Kanalisation, Keller: Rechteck aufziehen – das Land wird unsichtbar, darunter bauen"),
                         ("Höhle", IsoTool.Cave, string.Empty),
                         ("Dungeon-Generator", IsoTool.Dungeon, string.Empty),
                         ("Radierer", IsoTool.Eraser, string.Empty)
                     })
            {
                IsoTool t = tool;
                string h = hint;
                quick.DropDownItems.Add(L.T(text), null, (s, e) =>
                {
                    _host.SelectTool(t);
                    _host.MainView.Focus();
                    if (h.Length > 0)
                    {
                        Say(L.T(h));
                    }
                });
            }
            bar.Items.Add(quick);
            var under = new ToolStripButton(L.T("⬇ Unter der Karte bauen")) { CheckOnClick = true, ToolTipText = L.T("Hauptansicht unter dem Boden (Keller, Kanalisation, Höhlen) – die Ebenen-Ansicht daneben zeigt dann die Oberfläche (das Haus darüber)") };
            under.Click += (s, e) =>
            {
                _host.SetUnder(under.Checked);
                SyncLevel(true);
            };
            bar.Items.Add(under);
            _sync.Tick += (s, e) =>
            {
                if (under.Checked != _host.MainView.UnderView)
                {
                    under.Checked = _host.MainView.UnderView;
                }
                SyncLevel(false);
            };
            bar.Items.Add(new ToolStripSeparator());
            bar.Items.Add(new ToolStripLabel(L.T("Linien:")));
            var lines = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 90, ToolTipText = L.T("Koordinaten-Linien alle N Tiles (8 = ein Karten-Block) mit den Zahlen der großen Karte") };
            int[] steps = { 0, 1, 4, 8, 16, 32, 64 };
            lines.Items.AddRange(steps.Select(v => (object)(v == 0 ? L.T("aus") : v.ToString())).ToArray());
            lines.SelectedIndex = 3;
            lines.SelectedIndexChanged += (s, e) =>
            {
                _host.MainView.CoordLines = steps[lines.SelectedIndex];
                _level.CoordLines = steps[lines.SelectedIndex];
                _host.MainView.Invalidate();
                _level.Invalidate();
            };
            bar.Items.Add(lines);
            var info = new ToolStripButton(L.T("Tile-Info")) { CheckOnClick = true, Checked = true, ToolTipText = L.T("Am Mauszeiger: X / Y (große Karte), Boden-Höhe, Land-Tile und die Statics auf dem Feld mit ihrer Höhe") };
            info.Click += (s, e) =>
            {
                _host.MainView.TileInfo = info.Checked;
                _host.MainView.Invalidate();
            };
            bar.Items.Add(info);
            bar.Items.Add(new ToolStripSeparator());
            Btn("◧ Werkzeuge", "Werkzeug-Palette ein / aus (F7) – über ihren Kopf lässt sie sich lösen und auf einen anderen Monitor ziehen", () => _host.TogglePalette?.Invoke());
            Btn("◨ Seite", "Seitenleiste ein / aus (F8)", () => _host.ToggleSide?.Invoke());
            Btn("⧉ Seite lösen", "Seitenleiste (Items, Multis, Land, Pinsel …) als eigenes Fenster", () => _host.UndockSide?.Invoke());
            Btn("⧉ Optionen", "Werkzeug-Optionen als eigenes Fenster", () => _host.ToggleOptions?.Invoke());
            Btn("Hilfe", "Hilfe zum Bereichs-Fenster", () => _host.Help?.Invoke("region"));

            // ---- side panes: other level | buildings (side by side, each can be detached)
            Panel levelPane = Pane(L.T("Andere Ebene"), _levelTitle, LevelContent(), 1);
            Panel buildPane = Pane(L.T("Gebäude"), null, BuildingsContent(), 2);
            _side.Panel1.Controls.Add(levelPane);
            _side.Panel2.Controls.Add(buildPane);
            _split.Panel1.Controls.Add(WorkspaceHost);
            _split.Panel2.Controls.Add(_side);
            Controls.Add(_split);
            Controls.Add(bar);
            Controls.Add(_status);
            Say(L.T("Alle Werkzeuge wirken auf die Kopie des Bereichs. '✔ Fertig' schreibt die Änderungen an dieselbe Stelle zurück, '✖ Abbrechen' verwirft sie. Strg+Z / Strg+Y, F7 / F8 gehen auch hier."));
        }

        /// <summary>A side pane with a title and "⧉ lösen" (own window, closing docks it back).</summary>
        private Panel Pane(string title, Label extraTitle, Control content, int slot)
        {
            var pane = new Panel { Dock = DockStyle.Fill };
            var head = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, BackColor = Color.FromArgb(226, 232, 240) };
            head.Items.Add(new ToolStripLabel(title) { Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            var undock = new ToolStripButton("⧉ " + L.T("lösen")) { Alignment = ToolStripItemAlignment.Right, ToolTipText = L.T("Als eigenes Fenster (z. B. auf einen anderen Monitor) – Schließen dockt es wieder an") };
            head.Items.Add(undock);
            pane.Controls.Add(content);
            if (extraTitle != null)
            {
                pane.Controls.Add(extraTitle);
            }
            pane.Controls.Add(head);
            Form floating = null;
            undock.Click += (s, e) =>
            {
                if (floating != null)
                {
                    floating.Close();
                    return;
                }
                SplitterPanel home = slot == 1 ? _side.Panel1 : _side.Panel2;
                floating = new Form
                {
                    Text = title + " – " + L.T("Bereich bearbeiten"), Owner = this, ShowInTaskbar = false, FormBorderStyle = FormBorderStyle.SizableToolWindow,
                    StartPosition = FormStartPosition.Manual, Bounds = new Rectangle(Right - 40, Top + 60, 640, Math.Max(500, Height - 120))
                };
                home.Controls.Remove(pane);
                floating.Controls.Add(pane);
                if (slot == 1)
                {
                    _side.Panel1Collapsed = true;
                }
                else
                {
                    _side.Panel2Collapsed = true;
                }
                if (_side.Panel1Collapsed && _side.Panel2Collapsed)
                {
                    _split.Panel2Collapsed = true;
                }
                undock.Text = "⇤ " + L.T("andocken");
                floating.FormClosing += (s2, e2) =>
                {
                    floating.Controls.Remove(pane);
                    if (!IsDisposed)
                    {
                        home.Controls.Add(pane);
                        _split.Panel2Collapsed = false;
                        if (slot == 1)
                        {
                            _side.Panel1Collapsed = false;
                        }
                        else
                        {
                            _side.Panel2Collapsed = false;
                        }
                    }
                    undock.Text = "⧉ " + L.T("lösen");
                    floating = null;
                };
                floating.Show(this);
            };
            return pane;
        }

        // ================================================================== other level (cellar / surface)

        private Control LevelContent()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var strip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Bottom };
            var follow = new ToolStripButton(L.T("folgt der Hauptansicht")) { CheckOnClick = true, Checked = true, ToolTipText = L.T("Die Ebenen-Ansicht zeigt immer dieselbe Stelle wie die Hauptansicht") };
            follow.Click += (s, e) =>
            {
                _followMain = follow.Checked;
                SyncLevel(true);
            };
            var swap = new ToolStripButton("⇅ " + L.T("Ebenen tauschen")) { ToolTipText = L.T("Hauptansicht und Ebenen-Ansicht tauschen (oben ↔ unten) – gebaut wird immer in der Hauptansicht") };
            swap.Click += (s, e) =>
            {
                _host.SetUnder(!_host.MainView.UnderView);
                SyncLevel(true);
            };
            var zin = new ToolStripButton("Zoom +");
            zin.Click += (s, e) => _level.ZoomStep(1);
            var zout = new ToolStripButton("Zoom −");
            zout.Click += (s, e) => _level.ZoomStep(-1);
            strip.Items.AddRange(new ToolStripItem[] { follow, swap, zin, zout });
            panel.Controls.Add(_level);
            panel.Controls.Add(strip);
            return panel;
        }

        private void SetupLevel()
        {
            _host.Configure?.Invoke(_level);
            _level.BrushSettings = new CgiBrushSettings();
            _level.MaxZ = 127;
            _level.Tool = IsoTool.Select;
            _level.CoordOffset = _host.Origin;
            _level.CoordLines = _host.MainView.CoordLines;
            _level.SetMap(Map);
            _level.Message += (s, t) => Say(t);
            _host.MainView.ViewChanged += MainViewChanged;
            _host.MainView.MapChanged += MainMapChanged;
            SyncLevel(true);
            _sync.Start();
        }

        private void MainViewChanged(object sender, EventArgs e)
        {
            if (_followMain && !_level.IsDisposed)
            {
                Point c = _host.MainView.CameraTile;
                _level.CenterOn(c.X, c.Y);
            }
        }

        private void MainMapChanged(object sender, EventArgs e)
        {
            if (!_level.IsDisposed)
            {
                _level.Refresh(true);
            }
        }

        /// <summary>The level view always shows the OTHER level than the main view.</summary>
        private void SyncLevel(bool force)
        {
            if (_level.IsDisposed || _host.Map() == null)
            {
                return;
            }
            bool wantUnder = !_host.MainView.UnderView;
            if (force || _level.UnderView != wantUnder)
            {
                _level.UnderView = wantUnder;
                _levelTitle.Text = "  " + (wantUnder ? L.T("Unter der Karte: Keller, Kanalisation, Höhlen") : L.T("Oberfläche: das Haus / die Stadt darüber"));
                _levelTitle.BackColor = wantUnder ? Color.FromArgb(126, 34, 206) : Color.FromArgb(22, 128, 61);
                if (_followMain)
                {
                    Point c = _host.MainView.CameraTile;
                    _level.CenterOn(c.X, c.Y);
                }
                _level.Refresh(true);
            }
        }

        // ================================================================== buildings

        private Control BuildingsContent()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            _list.LargeImageList = _images;
            var top = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            ToolStripButton T(ToolStrip s, string text, string tip, Action a)
            {
                var b = new ToolStripButton(L.T(text)) { ToolTipText = L.T(tip) };
                b.Click += (x, e) => a();
                s.Items.Add(b);
                return b;
            }
            T(top, "↻ Erkennen", "Gebäude im markierten Bereich (sonst im ganzen Bereichs-Fenster) suchen: Wände, Dächer, Böden, die sich berühren", Detect);
            T(top, "🏘 Stadt neu ordnen …", "Die gewählten Gebäude (keins gewählt = alle) in Reihen mit Straßen dazwischen neu setzen – im markierten Bereich, sonst im ganzen Fenster", Rearrange);
            var act = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Bottom };
            T(act, "Wählen", "Gebäude auswählen: dann mit Pfeilen / Ziehen (Auswahl) verschieben, Strg+Pfeil = Kopie daneben", () => WithBuilding(SelectBuilding));
            T(act, "↻ 90°", "Gebäude mit Boden um 90° drehen (Wände werden getauscht, Richtungs-Tabelle gilt)", () => WithBuilding(b =>
            {
                _host.MainView.SetAreaAndShow(Square(b.Bounds), false);
                _host.MainView.TransformArea(1, false, false);
                Detect();
            }));
            T(act, "→ Vorlage", "Als Vorlage (Gebiets-Stempel) speichern – dann mit dem Werkzeug 'Vorlage' beliebig oft setzen", () => WithBuilding(b => _host.SaveTemplate?.Invoke(b.ToTemplate(Map, true))));
            T(act, "Keller darunter", "Zum Gebäude, Hauptansicht unter die Karte – daneben sieht man das Haus darüber", () => WithBuilding(b =>
            {
                _host.MainView.SetAreaAndShow(b.Bounds);
                _host.SetUnder(true);
                SyncLevel(true);
                Say(L.T("Unter dem Gebäude bauen: Keller (Rechteck aufziehen), Treppe, Static setzen – die Ebenen-Ansicht zeigt das Haus darüber."));
            }));
            T(act, "Begradigen …", "Boden unter dem Gebäude auf eine Höhe setzen (Statics mitnehmen = höher / tiefer)", () => WithBuilding(b =>
            {
                _host.MainView.SetAreaAndShow(b.Bounds, false);
                _host.LevelGround?.Invoke();
                Detect();
            }));
            T(act, "✖ Löschen", "Alle Statics des Gebäudes entfernen (Strg+Z nimmt es zurück)", () => WithBuilding(b =>
            {
                Map.Apply(b.Items.Select(MapData.Remove).ToList());
                _host.MainView.Refresh(true);
                Detect();
            }));
            _list.SelectedIndexChanged += (s, e) =>
            {
                if (_list.SelectedItems.Count == 1 && _list.SelectedItems[0].Tag is TownBuilding b)
                {
                    _host.MainView.SetAreaAndShow(b.Bounds);
                }
            };
            _list.DoubleClick += (s, e) => WithBuilding(SelectBuilding);
            panel.Controls.Add(_list);
            panel.Controls.Add(act);
            panel.Controls.Add(top);
            return panel;
        }

        private static Rectangle Square(Rectangle r)
        {
            int s = Math.Max(r.Width, r.Height);
            return new Rectangle(r.X, r.Y, s, s);
        }

        private void WithBuilding(Action<TownBuilding> a)
        {
            if (_list.SelectedItems.Count == 0 || !(_list.SelectedItems[0].Tag is TownBuilding b))
            {
                Say(L.T("Erst ein Gebäude in der Liste anklicken (oder '↻ Erkennen')."));
                return;
            }
            a(b);
        }

        private void SelectBuilding(TownBuilding b)
        {
            _host.SelectTool(IsoTool.Select);
            _host.MainView.SetAreaAndShow(b.Bounds);
            _host.MainView.SelectAreaStatics();
            _host.MainView.Focus();
            Say(b + " – " + L.T("ausgewählt: Pfeile = verschieben, Strg+Pfeil = Kopie daneben, Bild↑/↓ = Höhe, R im Bereich = drehen"));
        }

        private Rectangle WorkArea()
        {
            Rectangle a = _host.MainView.Area;
            return a.IsEmpty ? new Rectangle(0, 0, Map.Width, Map.Height) : a;
        }

        private void Detect()
        {
            MapData map = Map;
            if (map == null)
            {
                return;
            }
            UseWaitCursor = true;
            try
            {
                _buildings = TownTools.FindBuildings(map, WorkArea());
                _list.BeginUpdate();
                _list.Items.Clear();
                foreach (Image img in _images.Images)
                {
                    img.Dispose();
                }
                _images.Images.Clear();
                foreach (TownBuilding b in _buildings)
                {
                    Image pic;
                    try
                    {
                        pic = TemplatePreview.Render(b.ToTemplate(map, false), 96, 96);
                    }
                    catch (Exception)
                    {
                        pic = null;
                    }
                    var bmp = new Bitmap(96, 96);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.FromArgb(30, 41, 59));
                        if (pic != null)
                        {
                            g.DrawImage(pic, (96 - pic.Width) / 2, (96 - pic.Height) / 2, pic.Width, pic.Height);
                            pic.Dispose();
                        }
                    }
                    _images.Images.Add(bmp);
                    _list.Items.Add(new ListViewItem($"{b.Number}: {b.Bounds.Width}×{b.Bounds.Height}", _images.Images.Count - 1) { Tag = b, ToolTipText = b.ToString() });
                }
                _list.EndUpdate();
            }
            finally
            {
                UseWaitCursor = false;
            }
            Say(_buildings.Count + " " + L.T("Gebäude gefunden") + (_host.MainView.Area.IsEmpty ? string.Empty : " " + L.T("im markierten Bereich")) +
                " – " + L.T("anklicken = hinspringen, doppelklicken = auswählen (verschieben)"));
        }

        private void Rearrange()
        {
            MapData map = Map;
            List<TownBuilding> pick = _list.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag).OfType<TownBuilding>().ToList();
            if (pick.Count == 0)
            {
                pick = _buildings.ToList();
            }
            if (pick.Count == 0)
            {
                Say(L.T("Keine Gebäude – erst '↻ Erkennen'."));
                return;
            }
            Rectangle target = WorkArea();
            using (var f = new Form
                   {
                       Text = L.T("Stadt neu ordnen"), Size = new Size(460, 420), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.SizableToolWindow, Font = Font
                   })
            {
                var grid = new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = _town, ToolbarVisible = false, PropertySort = PropertySort.NoSort };
                var note = new Label
                {
                    Dock = DockStyle.Top, Height = 52, Padding = new Padding(6),
                    Text = $"{pick.Count} " + L.T("Gebäude werden in Reihen gesetzt, Ziel") + $": {target.Width}×{target.Height} " + L.T("(markierter Bereich, sonst das ganze Fenster)") + ". " + L.T("Ein Rückgängig-Schritt.")
                };
                var ok = new Button { Text = L.T("Neu ordnen"), Dock = DockStyle.Bottom, Height = 32, DialogResult = DialogResult.OK };
                f.Controls.Add(grid);
                f.Controls.Add(note);
                f.Controls.Add(ok);
                f.AcceptButton = ok;
                if (f.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
            }
            (List<StaticChange> statics, List<LandChange> land) = TownTools.Rearrange(map, pick, target, _town, out int n, out List<string> log);
            map.Apply(statics, land);
            map.NameLastStep(L.T("Stadt neu ordnen"));
            _host.MainView.Refresh(true);
            Detect();
            string skipped = string.Join(Environment.NewLine, log.Where(l => !l.Contains("→")));
            Say(n + " " + L.T("Gebäude neu gesetzt") + (pick.Count > n ? $", {pick.Count - n} " + L.T("passten nicht (bleiben stehen)") : string.Empty) + " · Strg+Z " + L.T("nimmt es zurück") + ".");
            if (skipped.Length > 0)
            {
                MessageBox.Show(this, skipped, L.T("Stadt neu ordnen"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
