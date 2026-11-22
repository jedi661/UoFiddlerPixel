// /***************************************************************************
//  *
//  * Dragon Map Studio – stamp menu of the map editor (pictures, categories, search)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    /// <summary>
    /// Colour stamps as pictures with category filter and search. The host (window or right panel of the editor) reacts on
    /// the events; <see cref="Undocked"/> switches the header button text.
    /// </summary>
    [ToolboxItem(false)]
    public sealed class StampBrowser : Panel
    {
        private readonly ComboBox _category;
        private readonly TextBox _search;
        private readonly ListView _list;
        private readonly ImageList _images;
        private readonly CheckBox _adapt;
        private readonly CheckBox _repeat;
        private readonly ComboBox _mode;
        private readonly ComboBox _layers;
        private readonly CheckBox _stepUndo;
        private readonly CheckBox _live;
        private readonly ComboBox _height;
        private readonly NumericUpDown _heightValue;
        private readonly NumericUpDown _heightOffset;
        private readonly CheckBox _autoFix;
        private bool _filling;
        private readonly Button _dock;
        private readonly Label _info;
        private List<ColorStamp> _all = new List<ColorStamp>();

        public StampBrowser()
        {
            BackColor = Color.FromArgb(236, 239, 244);
            Padding = new Padding(4);
            var head = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.FromArgb(180, 83, 9) };
            var title = new Label { Text = L.T("Stempel"), Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0) };
            var close = HeadButton("✕", L.T("Ausblenden"));
            close.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
            _dock = HeadButton("⧉", L.T("Als eigenes Fenster (z. B. auf einen anderen Monitor) / wieder rechts andocken"));
            _dock.Click += (s, e) => DockToggleRequested?.Invoke(this, EventArgs.Empty);
            head.Controls.Add(title);
            head.Controls.Add(_dock);
            head.Controls.Add(close);

            var filter = new Panel { Dock = DockStyle.Top, Height = 28, Padding = new Padding(0, 3, 0, 2) };
            _category = new ComboBox { Dock = DockStyle.Left, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            _search = new TextBox { Dock = DockStyle.Fill, PlaceholderText = L.T("Suchen …") };
            filter.Controls.Add(_search);
            filter.Controls.Add(_category);
            _category.SelectedIndexChanged += (s, e) => Fill();
            _search.TextChanged += (s, e) => Fill();

            _images = new ImageList { ImageSize = new Size(72, 72), ColorDepth = ColorDepth.Depth32Bit };
            _list = new ListView
            {
                Dock = DockStyle.Fill,
                LargeImageList = _images,
                View = View.LargeIcon,
                MultiSelect = false,
                HideSelection = false,
                ShowItemToolTips = true
            };
            _list.DoubleClick += (s, e) => RaisePlace();
            _list.SelectedIndexChanged += (s, e) => ShowInfo();

            // how the selected stamp is drawn (stored in the stamp)
            var drawPanel = new TableLayoutPanel { Dock = DockStyle.Bottom, ColumnCount = 2, RowCount = 5, AutoSize = true, Padding = new Padding(0, 4, 0, 0) };
            drawPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            drawPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _mode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            _mode.Items.AddRange(new object[] { L.T("Einmal (Stempel, Klick)"), L.T("Freihand (Pinsel, durchgehend)") });
            _layers = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            _layers.Items.AddRange(new object[] { L.T("Land + Statics"), L.T("Nur Land (Farben)"), L.T("Nur Statics") });
            drawPanel.Controls.Add(new Label { Text = L.T("Zeichnen:"), AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, 0, 0);
            drawPanel.Controls.Add(_mode, 1, 0);
            drawPanel.Controls.Add(new Label { Text = L.T("Ebene:"), AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, 0, 1);
            drawPanel.Controls.Add(_layers, 1, 1);
            var opts = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true, Margin = new Padding(0) };
            _stepUndo = new CheckBox { Text = L.T("Rückgängig schrittweise"), AutoSize = true, Margin = new Padding(3, 3, 3, 3) };
            _live = new CheckBox { Text = L.T("Live-CGI beim Zeichnen"), AutoSize = true, Checked = true, Margin = new Padding(3, 3, 3, 3) };
            new ToolTip().SetToolTip(_stepUndo, L.T("Ein Freihand-Strich wird in Teile zu je 32 Feldern zerlegt: Strg+Z nimmt Teil für Teil zurück, 'Ganzen Strich rückgängig' (Strg+Umschalt+Z) alles."));
            new ToolTip().SetToolTip(_live, L.T("Beim Zeichnen zeigt ein kleines CGI-Fenster die Umgebung der Maus gebaut – wie es im Spiel aussieht."));
            opts.Controls.Add(_stepUndo);
            opts.Controls.Add(_live);
            drawPanel.Controls.Add(opts, 0, 4);
            drawPanel.SetColumnSpan(opts, 2);
            // heights: terrain of the map / original from the stamp / fixed, plus an offset
            _height = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            _height.Items.AddRange(new object[] { L.T("Original (Höhen aus dem Stempel)"), L.T("Gelände der Karte behalten"), L.T("Feste Höhe") });
            drawPanel.Controls.Add(new Label { Text = L.T("Höhe:"), AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, 0, 2);
            drawPanel.Controls.Add(_height, 1, 2);
            var hrow = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true, Margin = new Padding(0) };
            hrow.Controls.Add(new Label { Text = L.T("fest Z"), AutoSize = true, Margin = new Padding(3, 7, 2, 3) });
            _heightValue = new NumericUpDown { Minimum = -128, Maximum = 127, Width = 52 };
            hrow.Controls.Add(_heightValue);
            hrow.Controls.Add(new Label { Text = "±", AutoSize = true, Margin = new Padding(6, 7, 2, 3) });
            _heightOffset = new NumericUpDown { Minimum = -60, Maximum = 60, Width = 52 };
            hrow.Controls.Add(_heightOffset);
            _autoFix = new CheckBox { Text = L.T("Nachbessern"), AutoSize = true, Margin = new Padding(8, 5, 3, 3) };
            new ToolTip().SetToolTip(_autoFix, L.T("Nach dem Strich Höhen-Spitzen, Stufen und Löcher am Rand an die Nachbarn angleichen (im selben Rückgängig-Schritt)."));
            hrow.Controls.Add(_autoFix);
            drawPanel.Controls.Add(hrow, 0, 3);
            drawPanel.SetColumnSpan(hrow, 2);
            _height.SelectedIndexChanged += (s, e) => RaiseHeight();
            _heightValue.ValueChanged += (s, e) => RaiseHeight();
            _heightOffset.ValueChanged += (s, e) => RaiseHeight();
            _autoFix.CheckedChanged += (s, e) => RaiseHeight();
            _mode.SelectedIndexChanged += (s, e) => ModeChanged();
            _layers.SelectedIndexChanged += (s, e) => ModeChanged();

            _info = new Label { Dock = DockStyle.Bottom, Height = 48, Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(51, 65, 85) };
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Padding = new Padding(0, 2, 0, 0) };
            _adapt = new CheckBox { Text = L.T("Höhen an den Boden anpassen"), Checked = true, AutoSize = true, Margin = new Padding(3, 6, 3, 3) };
            _repeat = new CheckBox { Text = L.T("Mehrfach setzen"), Checked = false, AutoSize = true, Margin = new Padding(3, 6, 3, 3) };
            Button cancel = ActionButton(L.T("Abbrechen"), Color.FromArgb(71, 85, 105), false);
            cancel.Click += (s, e) => CancelRequested?.Invoke(this, EventArgs.Empty);
            Button chooseFolder = ActionButton(L.T("Ordner wählen…"), Color.FromArgb(37, 99, 235), false);
            chooseFolder.Click += (s, e) => ChooseFolderRequested?.Invoke(this, EventArgs.Empty);
            Button import = ActionButton(L.T("Hinzufügen…"), Color.FromArgb(37, 99, 235), false);
            import.Click += (s, e) => ImportRequested?.Invoke(this, EventArgs.Empty);
            Button place = ActionButton(L.T("Setzen"), Color.FromArgb(180, 83, 9), true);
            place.Click += (s, e) => RaisePlace();
            Button del = ActionButton(L.T("Löschen"), Color.FromArgb(185, 28, 28), false);
            del.Click += (s, e) => { if (Selected != null) { DeleteRequested?.Invoke(this, Selected); } };
            Button folder = ActionButton(L.T("Ordner"), Color.FromArgb(71, 85, 105), false);
            folder.Click += (s, e) => FolderRequested?.Invoke(this, EventArgs.Empty);
            Button learn = ActionButton(L.T("Regeln lernen…"), Color.FromArgb(22, 128, 61), false);
            learn.Click += (s, e) => { if (Selected != null) { LearnRequested?.Invoke(this, Selected); } };
            Button reload = ActionButton(L.T("Neu laden"), Color.FromArgb(71, 85, 105), false);
            reload.Click += (s, e) => ReloadRequested?.Invoke(this, EventArgs.Empty);
            bottom.Controls.AddRange(new Control[] { place, cancel, learn, del, folder, chooseFolder, import, reload, _adapt, _repeat });

            Controls.Add(_list);
            Controls.Add(_info);
            Controls.Add(drawPanel);
            Controls.Add(bottom);
            Controls.Add(filter);
            Controls.Add(head);
        }

        public event EventHandler<ColorStamp> PlaceRequested;
        public event EventHandler<ColorStamp> DeleteRequested;
        public event EventHandler FolderRequested;
        public event EventHandler ReloadRequested;
        public event EventHandler DockToggleRequested;
        public event EventHandler CloseRequested;
        public event EventHandler CancelRequested;
        public event EventHandler<ColorStamp> LearnRequested;
        public event EventHandler ChooseFolderRequested;
        public event EventHandler ImportRequested;

        public bool PlaceRepeated => _repeat.Checked;

        public bool StepUndo => _stepUndo.Checked;

        public bool LivePreview => _live.Checked;

        private bool _settingHeight;

        /// <summary>Height options changed (the form stores them in the settings).</summary>
        public event EventHandler HeightOptionsChanged;

        public PatternHeightMode HeightMode => (PatternHeightMode)Math.Max(0, _height.SelectedIndex);

        public int HeightValue => (int)_heightValue.Value;

        public int HeightOffset => (int)_heightOffset.Value;

        public bool AutoFix => _autoFix.Checked;

        public void SetHeightOptions(PatternHeightMode mode, int value, int offset, bool autoFix)
        {
            _settingHeight = true;
            _height.SelectedIndex = (int)mode;
            _heightValue.Value = Math.Clamp(value, -128, 127);
            _heightOffset.Value = Math.Clamp(offset, -60, 60);
            _autoFix.Checked = autoFix;
            _heightValue.Enabled = mode == PatternHeightMode.Fixed;
            _settingHeight = false;
        }

        private void RaiseHeight()
        {
            _heightValue.Enabled = _height.SelectedIndex == (int)PatternHeightMode.Fixed;
            if (!_settingHeight)
            {
                HeightOptionsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Draw mode / layers of the selected stamp were changed (the form saves the stamp).</summary>
        public event EventHandler<ColorStamp> StampChanged;

        private void ModeChanged()
        {
            ColorStamp s = Selected;
            if (_filling || s == null)
            {
                return;
            }
            var mode = (StampDrawMode)Math.Max(0, _mode.SelectedIndex);
            var layers = (StampLayers)Math.Max(0, _layers.SelectedIndex);
            if (s.DrawMode != mode || s.Layers != layers)
            {
                s.DrawMode = mode;
                s.Layers = layers;
                StampChanged?.Invoke(this, s);
                ShowInfo();
            }
        }

        /// <summary>Folder of the stamps (shown in the info line).</summary>
        public string Folder { get; set; } = string.Empty;

        /// <summary>Picture of a stamp (set by the form: colours of the colour table, statics as dots).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<ColorStamp, Image> Thumbnail { get; set; }

        public bool AdaptHeights => _adapt.Checked;

        public ColorStamp Selected => _list.SelectedItems.Count > 0 ? _list.SelectedItems[0].Tag as ColorStamp : null;

        public bool Undocked
        {
            set => _dock.Text = value ? "⇥" : "⧉";
        }

        public void SetStamps(List<ColorStamp> stamps)
        {
            _all = stamps ?? new List<ColorStamp>();
            string cur = _category.SelectedIndex > 0 ? _category.Text : null;
            _category.Items.Clear();
            _category.Items.Add(L.T("Alle"));
            _category.Items.AddRange(_all.Select(s => string.IsNullOrWhiteSpace(s.Category) ? L.T("ohne Kategorie") : s.Category)
                                         .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c).Cast<object>().ToArray());
            _category.SelectedItem = cur != null && _category.Items.Contains(cur) ? cur : _category.Items[0];
            Fill();
        }

        private void Fill()
        {
            string cat = _category.SelectedIndex > 0 ? _category.Text : null;
            string f = _search.Text.Trim();
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (Image img in _images.Images)
            {
                img.Dispose();
            }
            _images.Images.Clear();
            foreach (ColorStamp s in _all)
            {
                string c = string.IsNullOrWhiteSpace(s.Category) ? L.T("ohne Kategorie") : s.Category;
                if ((cat != null && !string.Equals(c, cat, StringComparison.OrdinalIgnoreCase)) ||
                    (f.Length > 0 && !s.Name.Contains(f, StringComparison.OrdinalIgnoreCase) && !c.Contains(f, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                Image img = null;
                try
                {
                    img = Thumbnail?.Invoke(s);
                }
                catch (Exception)
                {
                    img = null;
                }
                _images.Images.Add(img ?? new Bitmap(72, 72));
                _list.Items.Add(new ListViewItem(s.Name, _images.Images.Count - 1)
                {
                    Tag = s,
                    ToolTipText = $"{s.Name}\r\n{c} · {s.Width}×{s.Height} · Boden Z {s.BaseAltitude} · {s.Statics?.Count ?? 0} Statics"
                });
            }
            _list.EndUpdate();
            ShowInfo();
        }

        private void ShowInfo()
        {
            ColorStamp s = Selected;
            _filling = true;
            _mode.Enabled = _layers.Enabled = s != null;
            if (s != null)
            {
                _mode.SelectedIndex = (int)s.DrawMode;
                _layers.SelectedIndex = (int)s.Layers;
            }
            _filling = false;
            _info.Text = s == null
                ? L.T("Doppelklick oder 'Setzen': Stempel hängt an der Maus, Klick setzt (einmal oder 'Mehrfach setzen'), Abbrechen / Esc = fertig.") + "\r\n" + Folder
                : $"{s.Name}: {s.Width}×{s.Height}, Boden Z {s.BaseAltitude}, {s.Statics?.Count ?? 0} Statics" +
                  (s.DrawMode == StampDrawMode.Freehand ? L.T(" · Freihand: Pinselgröße in der Leiste, ziehen = malen") : string.Empty) +
                  (s.CanLearn ? L.T(" · mit Land-Tiles (auch in der CGI-Ansicht als Pinsel)") : string.Empty);
        }

        private void RaisePlace()
        {
            if (Selected != null)
            {
                PlaceRequested?.Invoke(this, Selected);
            }
        }

        private static Button HeadButton(string text, string tip)
        {
            var b = new Button
            {
                Text = text,
                Dock = DockStyle.Right,
                Width = 28,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(60, Color.White),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            new ToolTip().SetToolTip(b, tip);
            return b;
        }

        private static Button ActionButton(string text, Color color, bool primary)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? color : Color.White,
                ForeColor = primary ? Color.White : Color.FromArgb(30, 41, 59),
                Font = new Font("Segoe UI", 8.5f, primary ? FontStyle.Bold : FontStyle.Regular),
                Margin = new Padding(2)
            };
            b.FlatAppearance.BorderColor = color;
            return b;
        }
    }
}
