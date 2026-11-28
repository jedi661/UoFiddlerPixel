/***************************************************************************
 *
 *  * Dragon Map Studio – window of the cave tools (opens with "Höhle" / "Eingang"):
 *  *   Höhle:   which floor texture is painted – sets with pictures, one tile fixed or mixed at random, own textures
 *  *            added from the object list; floor height (first click / fixed), walls. Mouse wheel while painting.
 *  *   Eingang: one click = whole entrance – the standard entrance or a saved one (pictures), height, direction;
 *  *            save the marked area as an entrance; "Malen" = the old painting of the mouth.
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
    public sealed class CaveToolHost
    {
        public IsoMapView View;
        public StudioSettings Settings;
        public Action SaveSoon;
        public Action<string> SetFloorIds;
        public NumericUpDown WallHeight;
        public CheckBox RaiseWalls;
        public CheckBox UseFixedHeight;
        public NumericUpDown FixedHeight;
        public Action OpenObjects;
        public Action<IsoTool> SelectTool;
        public Action<object, string> ShowSettings;
        public Action<string> Status;
        public Action Guide;
    }

    public sealed class CaveToolForm : Form
    {
        private readonly CaveToolHost _host;
        private readonly TabControl _tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly ComboBox _sets = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250 };
        private readonly ListView _tiles = new ListView { Dock = DockStyle.Fill, View = View.LargeIcon, MultiSelect = false, HideSelection = false };
        private readonly ImageList _tileImages = new ImageList { ImageSize = new Size(64, 64), ColorDepth = ColorDepth.Depth32Bit };
        private readonly RadioButton _fixed = new RadioButton { AutoSize = true };
        private readonly RadioButton _random = new RadioButton { AutoSize = true, Checked = true };
        private readonly ListView _entrances = new ListView { Dock = DockStyle.Fill, View = View.LargeIcon, MultiSelect = false, HideSelection = false };
        private readonly ImageList _entranceImages = new ImageList { ImageSize = new Size(96, 96), ColorDepth = ColorDepth.Depth32Bit };
        private readonly NumericUpDown _height = new NumericUpDown { Minimum = -60, Maximum = 60, Increment = 5, Width = 60 };
        private readonly ComboBox _facing = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
        private readonly RadioButton _click = new RadioButton { AutoSize = true, Checked = true };
        private readonly RadioButton _paint = new RadioButton { AutoSize = true };
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(4), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
        private bool _sync;
        private bool _showing;

        public CaveToolForm(CaveToolHost host)
        {
            _host = host;
            Text = L.T("Höhlen-Werkzeug");
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(470, 640);
            Font = new Font("Segoe UI", 9f);
            BuildUi();
            Rectangle r = Parse(host.Settings.CaveWindow);
            Bounds = r.IsEmpty ? new Rectangle(Screen.PrimaryScreen.WorkingArea.Right - 500, 120, 470, 640) : r;
            ResizeEnd += (s, e) => Remember();
            Move += (s, e) => Remember();
            host.View.CaveToolChanged += ViewChanged;
            FormClosed += (s, e) => host.View.CaveToolChanged -= ViewChanged;
            FillSets();
            FillEntrances();
        }

        private static Rectangle Parse(string text)
        {
            int[] v = (text ?? string.Empty).Split(',').Select(t => int.TryParse(t, out int n) ? n : int.MinValue).ToArray();
            if (v.Length != 4 || v.Any(n => n == int.MinValue) || v[2] < 200 || v[3] < 200)
            {
                return Rectangle.Empty;
            }
            var r = new Rectangle(v[0], v[1], v[2], v[3]);
            return Screen.AllScreens.Any(sc => sc.WorkingArea.IntersectsWith(r)) ? r : Rectangle.Empty;
        }

        private void Remember()
        {
            if (WindowState == FormWindowState.Normal)
            {
                _host.Settings.CaveWindow = $"{Left},{Top},{Width},{Height}";
                _host.SaveSoon?.Invoke();
            }
        }

        private void Say(string t)
        {
            _status.Text = t;
            _host.Status?.Invoke(t);
        }

        /// <summary>Shows the page of the tool (Höhle / Eingang).</summary>
        public void ShowTool(IsoTool t)
        {
            _showing = true;
            _tabs.SelectedIndex = t == IsoTool.CaveEntrance ? 1 : 0;
            _showing = false;
        }

        private void ViewChanged(object sender, EventArgs e)
        {
            _sync = true;
            _height.Value = Math.Clamp(_host.View.CaveEntranceHeight, (int)_height.Minimum, (int)_height.Maximum);
            int i = _host.View.CaveEntranceIndex;
            if (i >= 0 && i < _entrances.Items.Count)
            {
                _entrances.SelectedIndices.Clear();
                _entrances.SelectedIndices.Add(i);
                _entrances.EnsureVisible(i);
            }
            _facing.SelectedIndex = (int)(_host.View.CaveStamp?.Facing ?? CaveFacing.Auto);
            _sync = false;
        }

        // ================================================================== layout

        private void BuildUi()
        {
            Button B(Control parent, string t, Action a, int w = 0)
            {
                var b = new Button { Text = L.T(t), AutoSize = w == 0, Width = w == 0 ? 75 : w, Height = 26, FlatStyle = FlatStyle.System, Margin = new Padding(2) };
                b.Click += (s, e) => a();
                parent.Controls.Add(b);
                return b;
            }
            FlowLayoutPanel Row(Control parent, DockStyle dock = DockStyle.Top)
            {
                var p = new FlowLayoutPanel { Dock = dock, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Padding = new Padding(2) };
                parent.Controls.Add(p);
                return p;
            }

            // ---- page Höhle
            var cave = new TabPage(L.T("Höhle (Boden malen)"));
            _tiles.LargeImageList = _tileImages;
            cave.Controls.Add(_tiles);
            var hint = new Label
            {
                Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(4), ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Malen: am Bergfuß anfangen, in den Berg ziehen. Mausrad bei gedrückter Taste = Boden ±5 (Umschalt ±1) – alles, was in diesem Zug gemalt ist, geht mit. Strg+Mausrad = Zoom.")
            };
            cave.Controls.Add(hint);
            var height = Row(cave, DockStyle.Bottom);
            var firstClick = new RadioButton { Text = L.T("Boden = Höhe des ersten Klicks"), AutoSize = true, Checked = !_host.UseFixedHeight.Checked };
            var fixedZ = new RadioButton { Text = L.T("feste Höhe:"), AutoSize = true, Checked = _host.UseFixedHeight.Checked };
            var fz = new NumericUpDown { Minimum = _host.FixedHeight.Minimum, Maximum = _host.FixedHeight.Maximum, Value = _host.FixedHeight.Value, Width = 55 };
            var wall = new NumericUpDown { Minimum = _host.WallHeight.Minimum, Maximum = _host.WallHeight.Maximum, Value = _host.WallHeight.Value, Width = 55, Increment = 5 };
            var raise = new CheckBox { Text = L.T("Rand → Wand, Höhe"), AutoSize = true, Checked = _host.RaiseWalls.Checked };
            void SyncHeightRow()
            {
                firstClick.Checked = !_host.UseFixedHeight.Checked;
                fixedZ.Checked = _host.UseFixedHeight.Checked;
            }
            EventHandler follow = (s, e) => SyncHeightRow();
            _host.UseFixedHeight.CheckedChanged += follow;
            FormClosed += (s, e) => _host.UseFixedHeight.CheckedChanged -= follow;
            fixedZ.CheckedChanged += (s, e) => _host.UseFixedHeight.Checked = fixedZ.Checked;
            fz.ValueChanged += (s, e) => _host.FixedHeight.Value = fz.Value;
            wall.ValueChanged += (s, e) => _host.WallHeight.Value = wall.Value;
            raise.CheckedChanged += (s, e) => _host.RaiseWalls.Checked = raise.Checked;
            height.Controls.AddRange(new Control[] { firstClick, fixedZ, fz, raise, wall });
            var mode = Row(cave);
            _fixed.Text = L.T("nur das gewählte Tile (fest)");
            _random.Text = L.T("zufällig aus dem ganzen Set");
            _fixed.CheckedChanged += (s, e) => ApplyFloor();
            _tiles.SelectedIndexChanged += (s, e) => ApplyFloor();
            mode.Controls.AddRange(new Control[] { _fixed, _random });
            var setRow = Row(cave);
            setRow.Controls.Add(new Label { Text = L.T("Set:"), AutoSize = true, Margin = new Padding(2, 7, 2, 0) });
            setRow.Controls.Add(_sets);
            _sets.SelectedIndexChanged += (s, e) =>
            {
                if (!_sync)
                {
                    FillTiles();
                    ApplyFloor();
                }
            };
            B(setRow, "Neu …", NewSet);
            B(setRow, "Löschen", DeleteSet);
            var tileRow = Row(cave);
            B(tileRow, "+ Textur aus der Liste …", () => _host.OpenObjects?.Invoke());
            B(tileRow, "– Tile entfernen", RemoveTile);
            B(tileRow, "❓", () => _host.Guide?.Invoke(), 32);

            // ---- page Eingang
            var ent = new TabPage(L.T("Eingang (ein Klick)"));
            _entrances.LargeImageList = _entranceImages;
            ent.Controls.Add(_entrances);
            var ehint = new Label
            {
                Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(4), ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Klick vor den Berg = ganzer Eingang. Mausrad = nächster Eingang (Vorschau an der Maus), Umschalt+Mausrad = Höhe, Strg+Mausrad = Zoom, R = Richtung des Standard-Eingangs.")
            };
            ent.Controls.Add(ehint);
            var erow2 = Row(ent, DockStyle.Bottom);
            B(erow2, "💾 Markierten Bereich als Eingang speichern …", SaveEntrance);
            B(erow2, "Löschen", DeleteEntrance);
            B(erow2, "Standard-Einstellungen …", () => _host.ShowSettings?.Invoke(_host.View.CaveStamp, L.T("Höhlen-Stempel")));
            var erow = Row(ent);
            erow.Controls.Add(new Label { Text = L.T("Höhe:"), AutoSize = true, Margin = new Padding(2, 7, 2, 0) });
            erow.Controls.Add(_height);
            erow.Controls.Add(new Label { Text = L.T("Richtung:"), AutoSize = true, Margin = new Padding(8, 7, 2, 0) });
            _facing.Items.AddRange(new object[] { L.T("automatisch"), L.T("Süden"), L.T("Osten") });
            _facing.SelectedIndex = (int)(_host.View.CaveStamp?.Facing ?? CaveFacing.Auto);
            erow.Controls.Add(_facing);
            var emode = Row(ent);
            _click.Text = L.T("Klick = ganzer Eingang");
            _paint.Text = L.T("Malen (Mund von Hand, wie bisher)");
            _click.Checked = _host.View.CaveEntranceClick;
            _paint.Checked = !_host.View.CaveEntranceClick;
            _click.CheckedChanged += (s, e) =>
            {
                _host.View.CaveEntranceClick = _click.Checked;
                Say(L.T(_click.Checked ? "Eingang: ein Klick setzt den gewählten Eingang." : "Eingang: Malen – am Bergfuß klicken und 2–3 Tiles in den Hang ziehen."));
            };
            emode.Controls.AddRange(new Control[] { _click, _paint });
            _height.ValueChanged += (s, e) =>
            {
                if (!_sync)
                {
                    _host.View.CaveEntranceHeight = (int)_height.Value;
                    _host.View.Invalidate();
                }
            };
            _facing.SelectedIndexChanged += (s, e) =>
            {
                if (!_sync)
                {
                    _host.View.CaveStamp ??= new CaveStampSettings();
                    _host.View.CaveStamp.Facing = (CaveFacing)_facing.SelectedIndex;
                    _host.View.Invalidate();
                }
            };
            _entrances.SelectedIndexChanged += (s, e) =>
            {
                if (!_sync && _entrances.SelectedIndices.Count > 0)
                {
                    _host.View.CaveEntranceIndex = _entrances.SelectedIndices[0];
                    _host.View.CaveEntranceClick = true;
                    _click.Checked = true;
                    _host.SelectTool?.Invoke(IsoTool.CaveEntrance);
                    _host.View.Invalidate();
                }
            };

            _tabs.TabPages.Add(cave);
            _tabs.TabPages.Add(ent);
            _tabs.SelectedIndexChanged += (s, e) =>
            {
                if (!_showing)
                {
                    _host.SelectTool?.Invoke(_tabs.SelectedIndex == 1 ? IsoTool.CaveEntrance : IsoTool.Cave);
                }
            };
            var bottom = new CheckBox { Dock = DockStyle.Bottom, Text = L.T("Fenster mit den Werkzeugen Höhle / Eingang öffnen"), Checked = _host.Settings.CaveWindowAuto, Height = 24 };
            bottom.CheckedChanged += (s, e) =>
            {
                _host.Settings.CaveWindowAuto = bottom.Checked;
                _host.SaveSoon?.Invoke();
            };
            Controls.Add(_tabs);
            Controls.Add(bottom);
            Controls.Add(_status);
            _host.View.CaveEntrancePresets = () => CaveEntrancePresets.All;
        }

        // ================================================================== floor sets

        private List<CaveTileSet> Sets
        {
            get
            {
                if (_host.Settings.CaveTileSets == null || _host.Settings.CaveTileSets.Count == 0)
                {
                    _host.Settings.CaveTileSets = CaveTileSet.Defaults();
                }
                return _host.Settings.CaveTileSets;
            }
        }

        private CaveTileSet CurrentSet => _sets.SelectedItem as CaveTileSet;

        private void FillSets(int select = 0)
        {
            _sync = true;
            _sets.Items.Clear();
            foreach (CaveTileSet s in Sets)
            {
                _sets.Items.Add(s);
            }
            _sets.SelectedIndex = Math.Clamp(select, 0, _sets.Items.Count - 1);
            _sync = false;
            FillTiles();
        }

        private void FillTiles()
        {
            _tiles.BeginUpdate();
            _tiles.Items.Clear();
            _tileImages.Images.Clear();
            foreach (ushort id in HexList.Parse(CurrentSet?.Ids ?? string.Empty))
            {
                var bmp = new Bitmap(64, 64);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(30, 41, 59));
                    Image art = null;
                    try
                    {
                        art = UoArtCache.Texture(id) ?? UoArtCache.Land(id);
                    }
                    catch (Exception)
                    {
                        art = null;
                    }
                    if (art != null)
                    {
                        g.DrawImage(art, new Rectangle(4, 4, 56, 56));
                    }
                }
                _tileImages.Images.Add(bmp);
                _tiles.Items.Add(new ListViewItem($"{id:X4}", _tileImages.Images.Count - 1) { Tag = id, ToolTipText = UoArtCache.LandName(id) });
            }
            _tiles.EndUpdate();
            if (_tiles.Items.Count > 0)
            {
                _tiles.Items[0].Selected = true;
            }
        }

        /// <summary>The tool paints the chosen tile (fixed) or the whole set (random).</summary>
        private void ApplyFloor()
        {
            if (_sync || CurrentSet == null)
            {
                return;
            }
            string ids = _fixed.Checked && _tiles.SelectedItems.Count > 0 ? ((ushort)_tiles.SelectedItems[0].Tag).ToString("x4") : CurrentSet.Ids;
            if (string.IsNullOrWhiteSpace(ids))
            {
                return;
            }
            _host.SetFloorIds?.Invoke(ids);
            Say(L.T("Höhle malt") + ": " + ids + (_fixed.Checked ? " (" + L.T("fest") + ")" : " (" + L.T("zufällig") + ")"));
        }

        /// <summary>From the object list: a land tile goes into the current set.</summary>
        public void AddTile(int id)
        {
            CaveTileSet s = CurrentSet;
            if (s == null)
            {
                return;
            }
            List<ushort> ids = HexList.Parse(s.Ids).ToList();
            if (!ids.Contains((ushort)id))
            {
                ids.Add((ushort)id);
            }
            s.Ids = string.Join(" ", ids.Select(i => i.ToString("x4")));
            _host.SaveSoon?.Invoke();
            FillTiles();
            foreach (ListViewItem it in _tiles.Items)
            {
                it.Selected = (ushort)it.Tag == id;
            }
            ApplyFloor();
            Say(L.T("Zum Set hinzugefügt") + $": 0x{id:X4} {UoArtCache.LandName(id)}");
        }

        private void RemoveTile()
        {
            CaveTileSet s = CurrentSet;
            if (s == null || _tiles.SelectedItems.Count == 0)
            {
                return;
            }
            ushort id = (ushort)_tiles.SelectedItems[0].Tag;
            s.Ids = string.Join(" ", HexList.Parse(s.Ids).Where(i => i != id).Select(i => i.ToString("x4")));
            _host.SaveSoon?.Invoke();
            FillTiles();
            ApplyFloor();
        }

        private void NewSet()
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name des Boden-Sets (z. B. Kristallhöhle, Lavaboden):"), Text, L.T("Mein Höhlenboden"));
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            Sets.Add(new CaveTileSet { Name = name.Trim(), Ids = CurrentSet?.Ids ?? "0245" });
            _host.SaveSoon?.Invoke();
            FillSets(Sets.Count - 1);
            Say(L.T("Neues Set – mit '+ Textur aus der Liste' eigene Texturen dazu, '– Tile entfernen' weg."));
        }

        private void DeleteSet()
        {
            if (CurrentSet == null || Sets.Count <= 1)
            {
                return;
            }
            Sets.Remove(CurrentSet);
            _host.SaveSoon?.Invoke();
            FillSets(0);
        }

        // ================================================================== entrances

        private void FillEntrances()
        {
            _entrances.BeginUpdate();
            _entrances.Items.Clear();
            foreach (Image i in _entranceImages.Images)
            {
                i.Dispose();
            }
            _entranceImages.Images.Clear();
            Bitmap Thumb(MapTemplate t)
            {
                var bmp = new Bitmap(96, 96);
                using Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.FromArgb(30, 41, 59));
                Image pic = null;
                try
                {
                    pic = t == null ? UoArtCache.Land(0x01DB) : TemplatePreview.Render(t, 92, 92);
                }
                catch (Exception)
                {
                    pic = null;
                }
                if (pic != null)
                {
                    g.DrawImage(pic, (96 - Math.Min(92, pic.Width)) / 2, (96 - Math.Min(92, pic.Height)) / 2, Math.Min(92, pic.Width), Math.Min(92, pic.Height));
                }
                return bmp;
            }
            _entranceImages.Images.Add(Thumb(null));
            _entrances.Items.Add(new ListViewItem(L.T("Standard (Stempel)"), 0));
            foreach (MapTemplate t in CaveEntrancePresets.All)
            {
                _entranceImages.Images.Add(Thumb(t));
                _entrances.Items.Add(new ListViewItem(t.Name, _entranceImages.Images.Count - 1) { Tag = t });
            }
            _entrances.EndUpdate();
            int i0 = Math.Clamp(_host.View.CaveEntranceIndex, 0, _entrances.Items.Count - 1);
            _sync = true;
            _entrances.Items[i0].Selected = true;
            _sync = false;
        }

        private void SaveEntrance()
        {
            MapTemplate t = _host.View.CaptureEntrance();
            if (t == null)
            {
                Say(L.T("Erst mit 'Bereich' einen fertigen Eingang markieren (Mund, Rahmen, Weg davor) – dann hier speichern."));
                return;
            }
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name des Eingangs (z. B. Bergwerk Süd, Grotte Ost):"), Text, L.T("Eingang") + $" {CaveEntrancePresets.All.Count + 1}");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            t.Name = name.Trim();
            string path = CaveEntrancePresets.Save(t);
            FillEntrances();
            int i = CaveEntrancePresets.All.FindIndex(x => x.Name == t.Name) + 1;
            _host.View.CaveEntranceIndex = Math.Max(0, i);
            FillEntrances();
            Say(L.T("Eingang gespeichert") + $": {path} – " + L.T("Werkzeug Eingang, Mausrad wählt ihn, Klick setzt ihn."));
        }

        private void DeleteEntrance()
        {
            if (_entrances.SelectedItems.Count == 0 || !(_entrances.SelectedItems[0].Tag is MapTemplate t))
            {
                Say(L.T("Den Standard-Eingang kann man nicht löschen."));
                return;
            }
            if (MessageBox.Show(this, L.T("Eingang löschen?") + $" {t.Name}", Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            if (!CaveEntrancePresets.Delete(t))
            {
                Say(L.T("Dieser Eingang kommt aus dem Projekt (DragonData\\Hoehleneingaenge) – dort löschen."));
                return;
            }
            _host.View.CaveEntranceIndex = 0;
            FillEntrances();
        }
    }
}
