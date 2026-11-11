using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using System.Xml.Linq;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ScriptCreator
{
    internal enum BrowserTab { Items, Animations, Sounds, Hues, Gumps, Cliloc, Overview }

    /// <summary>
    /// Browser for the client data a script needs: items (art), animations (bodies), sounds, hues, gumps, cliloc texts
    /// and an overview where to find which ID. Opened from a field it returns the chosen value.
    /// </summary>
    internal sealed class UoBrowserForm : Form
    {
        private static bool[] _usedItems;
        private static List<(int Id, string Name)> _soundList;
        private static StringList _cliloc;

        private readonly bool _pick;
        private readonly TabControl _tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly ToolTip _tip = new ToolTip();
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0) };
        private readonly SoundPlayer _player = new SoundPlayer();
        private readonly List<int> _suggestions;

        public int SelectedValue { get; private set; } = -1;

        public BrowserTab Tab { get; private set; }

        public UoBrowserForm(BrowserTab tab, int current, IEnumerable<int> suggestions = null, bool pick = true)
        {
            _pick = pick;
            _suggestions = suggestions?.ToList() ?? new List<int>();
            Text = pick ? "Choose / Auswählen – UO browser" : "UO browser – overview / Übersicht";
            Size = new Size(1100, 760);
            MinimumSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;
            KeyPreview = true;

            BuildItems();
            BuildAnimations();
            BuildSounds();
            BuildHues();
            BuildGumps();
            BuildCliloc();
            BuildOverview();

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
            var close = new Button { Text = pick ? "Cancel" : "Close", AutoSize = true, Height = 28, DialogResult = DialogResult.Cancel };
            bottom.Controls.Add(close);
            if (pick)
            {
                var use = new Button { Text = "Use / Übernehmen", AutoSize = true, Height = 28 };
                use.Click += (s, e) => Choose();
                bottom.Controls.Add(use);
                AcceptButton = use;
            }
            var copy = new Button { Text = "Copy ID", AutoSize = true, Height = 28 };
            copy.Click += (s, e) => CopyCurrent();
            bottom.Controls.Add(copy);
            CancelButton = close;

            Controls.Add(_tabs);
            Controls.Add(_status);
            Controls.Add(bottom);
            _tabs.SelectedIndexChanged += (s, e) => TabShown();
            Tab = tab;
            _tabs.SelectedIndex = (int)tab;
            Shown += (s, e) =>
            {
                TabShown();
                Preselect(tab, current);
            };
        }

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private void SetStatus(string text) => _status.Text = text;

        /// <summary>The value of the visible tab, or -1.</summary>
        private int CurrentValue()
        {
            switch ((BrowserTab)_tabs.SelectedIndex)
            {
                case BrowserTab.Items: return _itemGrid.SelectedItem;
                case BrowserTab.Animations: return _animBody;
                case BrowserTab.Sounds: return _soundView.SelectedItems.Count > 0 ? (int)_soundView.SelectedItems[0].Tag : -1;
                case BrowserTab.Hues: return _hueList.SelectedItem is HueRow h ? h.Number : -1;
                case BrowserTab.Gumps: return _gumpGrid.SelectedItem;
                case BrowserTab.Cliloc: return _clilocView.SelectedItems.Count > 0 ? (int)_clilocView.SelectedItems[0].Tag : -1;
                default: return -1;
            }
        }

        private void Choose()
        {
            int v = CurrentValue();
            if (v < 0)
            {
                SetStatus("Select an entry first. / Erst einen Eintrag wählen.");
                return;
            }
            if (!_pick)
            {
                CopyCurrent();
                return;
            }
            SelectedValue = v;
            Tab = (BrowserTab)_tabs.SelectedIndex;
            DialogResult = DialogResult.OK;
        }

        private void CopyCurrent()
        {
            int v = CurrentValue();
            if (v < 0)
            {
                return;
            }
            string text = (BrowserTab)_tabs.SelectedIndex == BrowserTab.Cliloc || (BrowserTab)_tabs.SelectedIndex == BrowserTab.Hues ? v.ToString(CultureInfo.InvariantCulture) : $"0x{v:X4}";
            try
            {
                Clipboard.SetText(text);
                SetStatus("Copied " + text);
            }
            catch (Exception)
            {
                // clipboard busy
            }
        }

        private void Preselect(BrowserTab tab, int value)
        {
            if (value < 0)
            {
                return;
            }
            switch (tab)
            {
                case BrowserTab.Items:
                    if (!_itemGrid.Items.Contains(value))
                    {
                        _itemFilter.SelectedIndex = _itemFilter.Items.IndexOf("All items"); // refills the grid
                    }
                    _itemGrid.SelectItem(value);
                    break;
                case BrowserTab.Animations:
                    _animId.Text = value.ToString(CultureInfo.InvariantCulture);
                    ShowBody(value);
                    break;
                case BrowserTab.Sounds:
                    foreach (ListViewItem li in _soundView.Items)
                    {
                        if ((int)li.Tag == value)
                        {
                            li.Selected = true;
                            li.EnsureVisible();
                            break;
                        }
                    }
                    break;
                case BrowserTab.Hues:
                    for (int i = 0; i < _hueList.Items.Count; i++)
                    {
                        if (((HueRow)_hueList.Items[i]).Number == (value & 0x3FFF))
                        {
                            _hueList.SelectedIndex = i;
                            break;
                        }
                    }
                    break;
                case BrowserTab.Gumps:
                    _gumpGrid.SelectItem(value);
                    break;
                case BrowserTab.Cliloc:
                    _clilocSearch.Text = value.ToString(CultureInfo.InvariantCulture);
                    break;
            }
        }

        private void TabShown()
        {
            switch ((BrowserTab)_tabs.SelectedIndex)
            {
                case BrowserTab.Items:
                    if (_itemGrid.Items.Count == 0)
                    {
                        FillItems();
                    }
                    break;
                case BrowserTab.Sounds:
                    if (_soundView.Items.Count == 0)
                    {
                        FillSounds();
                    }
                    break;
                case BrowserTab.Gumps:
                    if (_gumpGrid.Items.Count == 0)
                    {
                        FillGumps();
                    }
                    break;
            }
        }

        private static FlowLayoutPanel Bar(params Control[] controls)
        {
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(2) };
            bar.Controls.AddRange(controls);
            return bar;
        }

        private static Label Lbl(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 0) };

        private static void DrawFit(Graphics g, Rectangle area, Image img, int maxScale)
        {
            g.Clear(Color.FromArgb(45, 45, 48));
            if (img == null)
            {
                return;
            }
            Rectangle box = Rectangle.Inflate(area, -6, -6);
            double scale = Math.Min(maxScale, Math.Min(box.Width / (double)img.Width, box.Height / (double)img.Height));
            int w = Math.Max(1, (int)(img.Width * scale)), h = Math.Max(1, (int)(img.Height * scale));
            g.InterpolationMode = scale >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(img, new Rectangle(box.X + (box.Width - w) / 2, box.Y + (box.Height - h) / 2, w, h));
        }

        #region Items

        private ArtTileGrid _itemGrid;
        private TextBox _itemSearch;
        private ComboBox _itemFilter;
        private Panel _itemPreview;
        private Label _itemInfo;
        private NumericUpDown _itemHue;

        private void BuildItems()
        {
            var page = new TabPage("Items (art)");
            _itemSearch = new TextBox { Width = 200, PlaceholderText = "name or ID / Name oder ID" };
            _itemSearch.TextChanged += (s, e) => FillItems();
            var filters = new List<string> { "All items", "Wearable", "Weapon", "Armor", "Container", "Light source", "Door", "Wall", "Surface (tables …)", "Foliage", "Animated" };
            if (_suggestions.Count > 0)
            {
                filters.Insert(0, $"Suggested ({_suggestions.Count})");
            }
            _itemFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
            _itemFilter.Items.AddRange(filters.Cast<object>().ToArray());
            _itemFilter.SelectedIndex = 0;
            _itemFilter.SelectedIndexChanged += (s, e) => FillItems();
            _itemGrid = new ArtTileGrid { Dock = DockStyle.Fill, CellSize = new Size(72, 86), Text = "art.mul is not loaded." };
            _itemGrid.ImageProvider = id => CutterArt.GetShared(ArtKind.Item, id);
            _itemGrid.LabelProvider = id => $"0x{id:X4}";
            _itemGrid.SelectionChanged += (s, e) => ShowItem();
            _itemGrid.ItemActivated += (s, e) => Choose();
            _itemPreview = new Panel { Dock = DockStyle.Top, Height = 260 };
            _itemPreview.Paint += (s, e) =>
            {
                int id = _itemGrid.SelectedItem;
                Bitmap img = id >= 0 ? UoPreview.Item(id, (int)_itemHue.Value) : null;
                DrawFit(e.Graphics, _itemPreview.ClientRectangle, img, 3);
                img?.Dispose();
            };
            _itemHue = new NumericUpDown { Minimum = 0, Maximum = 3000, Width = 70 };
            _itemHue.ValueChanged += (s, e) => _itemPreview.Invalidate();
            _itemInfo = new Label { Dock = DockStyle.Fill, Padding = new Padding(4), Font = new Font("Consolas", 9f) };
            var right = new Panel { Dock = DockStyle.Right, Width = 300 };
            right.Controls.Add(_itemInfo);
            right.Controls.Add(Bar(Lbl("Hue preview:"), _itemHue));
            right.Controls.Add(_itemPreview);
            page.Controls.Add(_itemGrid);
            page.Controls.Add(right);
            page.Controls.Add(Bar(Lbl("Search:"), _itemSearch, _itemFilter));
            _tabs.TabPages.Add(page);
        }

        private void FillItems()
        {
            if (!CutterArt.IsAvailable)
            {
                _itemGrid.Items = Array.Empty<int>();
                SetStatus("art.mul is not loaded.");
                return;
            }
            Cursor = Cursors.WaitCursor;
            try
            {
                if (_usedItems == null)
                {
                    _usedItems = CutterArt.ScanUsed(ArtKind.Item);
                }
                string filter = _itemFilter.SelectedItem as string ?? string.Empty;
                bool suggested = filter.StartsWith("Suggested", StringComparison.Ordinal);
                TileFlag flag = filter switch
                {
                    "Wearable" => TileFlag.Wearable,
                    "Weapon" => TileFlag.Weapon,
                    "Armor" => TileFlag.Armor,
                    "Container" => TileFlag.Container,
                    "Light source" => TileFlag.LightSource,
                    "Door" => TileFlag.Door,
                    "Wall" => TileFlag.Wall,
                    "Surface (tables …)" => TileFlag.Surface,
                    "Foliage" => TileFlag.Foliage,
                    "Animated" => TileFlag.Animation,
                    _ => TileFlag.None
                };
                string search = _itemSearch.Text.Trim();
                bool isId = ScriptTemplate.TryParseInt(search, out long searchId);
                var list = new List<int>();
                IEnumerable<int> source = suggested ? _suggestions : Enumerable.Range(0, _usedItems.Length).Where(i => _usedItems[i]);
                lock (SdkSync.Lock)
                {
                    ItemData[] table = TileData.ItemTable;
                    foreach (int id in source)
                    {
                        ItemData? d = table != null && id < table.Length ? table[id] : (ItemData?)null;
                        if (flag != TileFlag.None && (d == null || (d.Value.Flags & flag) == 0))
                        {
                            continue;
                        }
                        if (search.Length > 0)
                        {
                            bool match = isId && id == searchId;
                            if (!match && d != null)
                            {
                                match = (d.Value.Name ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
                            }
                            if (!match)
                            {
                                continue;
                            }
                        }
                        list.Add(id);
                    }
                }
                _itemGrid.Items = list;
                SetStatus($"{list.Count} items. Double click = use. / Doppelklick = übernehmen.");
                if (isId)
                {
                    _itemGrid.SelectItem((int)searchId);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ShowItem()
        {
            int id = _itemGrid.SelectedItem;
            _itemPreview.Invalidate();
            if (id < 0)
            {
                _itemInfo.Text = string.Empty;
                return;
            }
            string text = $"Item ID  0x{id:X4} ({id})\nArt idx  0x{id + 0x4000:X4}\n";
            try
            {
                lock (SdkSync.Lock)
                {
                    ItemData d = TileData.ItemTable[id];
                    text += $"Name     {d.Name?.Trim()}\nWeight   {d.Weight}\nHeight   {d.Height}\n";
                    if ((d.Flags & TileFlag.Wearable) != 0)
                    {
                        text += $"Layer    {d.Quality}\nAnimID   {d.Animation}\nGump M   {d.Animation + 50000}\nGump F   {d.Animation + 60000}\n";
                    }
                    text += "\nFlags:\n" + string.Join("\n", d.Flags.ToString().Split(',').Select(f => "  " + f.Trim()));
                }
            }
            catch (Exception)
            {
                // tiledata missing
            }
            _itemInfo.Text = text;
        }

        #endregion

        #region Animations

        private ListView _animView;
        private TextBox _animSearch, _animId;
        private NumericUpDown _animAction;
        private ComboBox _animDir;
        private Panel _animPreview;
        private Label _animInfo;
        private readonly Timer _animTimer = new Timer { Interval = 140 };
        private AnimationFrame[] _frames;
        private int _frame;
        private int _animBody = -1;
        private List<(int Body, string Name, int Type)> _mobs;

        private void BuildAnimations()
        {
            var page = new TabPage("Animations (body)");
            _animSearch = new TextBox { Width = 200, PlaceholderText = "name or body / Name oder Body" };
            _animSearch.TextChanged += (s, e) => FillAnimations();
            _animId = new TextBox { Width = 70 };
            var go = new Button { Text = "Show", AutoSize = true };
            go.Click += (s, e) => { if (ScriptTemplate.TryParseInt(_animId.Text, out long b) && b >= 0 && b < 0x10000) ShowBody((int)b); };
            _animView = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false };
            _animView.Columns.Add("Body", 70);
            _animView.Columns.Add("Hex", 60);
            _animView.Columns.Add("Name", 230);
            _animView.Columns.Add("Type", 110);
            _animView.SelectedIndexChanged += (s, e) =>
            {
                if (_animView.SelectedItems.Count > 0)
                {
                    ShowBody((int)_animView.SelectedItems[0].Tag);
                }
            };
            _animView.DoubleClick += (s, e) => Choose();
            _animAction = new NumericUpDown { Minimum = 0, Maximum = 34, Width = 50 };
            _animAction.ValueChanged += (s, e) => LoadFrames();
            _animDir = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
            _animDir.Items.AddRange(new object[] { "0 north", "1 right", "2 east", "3 down", "4 south", "5 left", "6 west", "7 up" });
            _animDir.SelectedIndex = 1;
            _animDir.SelectedIndexChanged += (s, e) => LoadFrames();
            _animPreview = new Panel { Dock = DockStyle.Top, Height = 300 };
            _animPreview.Paint += (s, e) =>
            {
                Bitmap bmp = _frames != null && _frames.Length > 0 ? _frames[_frame % _frames.Length]?.Bitmap : null;
                DrawFit(e.Graphics, _animPreview.ClientRectangle, bmp, 3);
            };
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(_animPreview, true);
            _animTimer.Tick += (s, e) =>
            {
                if (_frames != null && _frames.Length > 1)
                {
                    _frame++;
                    _animPreview.Invalidate();
                }
            };
            _animTimer.Start();
            _animInfo = new Label { Dock = DockStyle.Fill, Padding = new Padding(4), Font = new Font("Consolas", 9f) };
            var right = new Panel { Dock = DockStyle.Right, Width = 330 };
            right.Controls.Add(_animInfo);
            right.Controls.Add(Bar(Lbl("Action:"), _animAction, _animDir));
            right.Controls.Add(_animPreview);
            page.Controls.Add(_animView);
            page.Controls.Add(right);
            page.Controls.Add(Bar(Lbl("Search:"), _animSearch, Lbl("Body:"), _animId, go));
            _tabs.TabPages.Add(page);
            LoadMobList();
            FillAnimations();
        }

        private void LoadMobList()
        {
            _mobs = new List<(int, string, int)>();
            string[] candidates =
            {
                Path.Combine(UoFiddler.Controls.Classes.Options.AppDataPath ?? string.Empty, "Animationlist.xml"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Animationlist.xml")
            };
            foreach (string file in candidates)
            {
                try
                {
                    if (!File.Exists(file))
                    {
                        continue;
                    }
                    foreach (XElement mob in XDocument.Load(file).Descendants("Mob").Concat(XDocument.Load(file).Descendants("Equip")))
                    {
                        if (int.TryParse((string)mob.Attribute("body"), out int body))
                        {
                            int.TryParse((string)mob.Attribute("type"), out int type);
                            _mobs.Add((body, (string)mob.Attribute("name") ?? string.Empty, type));
                        }
                    }
                    break;
                }
                catch (Exception)
                {
                    // try the next file
                }
            }
        }

        private void FillAnimations()
        {
            string f = _animSearch.Text.Trim();
            bool isId = ScriptTemplate.TryParseInt(f, out long id);
            string[] types = { "Monster", "Sea", "Animal", "Human / equip" };
            _animView.BeginUpdate();
            _animView.Items.Clear();
            foreach ((int body, string name, int type) in _mobs.Where(m => f.Length == 0 || (isId && m.Body == id) || m.Name.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0).Take(3000))
            {
                _animView.Items.Add(new ListViewItem(new[] { body.ToString(CultureInfo.InvariantCulture), $"0x{body:X}", name, type >= 0 && type < types.Length ? types[type] : type.ToString() }) { Tag = body });
            }
            _animView.EndUpdate();
            SetStatus(_mobs.Count == 0
                ? "Animationlist.xml not found – type a body number and press Show. / Animationlist.xml fehlt – Body-Nummer eingeben und Show drücken."
                : $"{_animView.Items.Count} animations (names from Animationlist.xml).");
        }

        private void ShowBody(int body)
        {
            _animBody = body;
            _animId.Text = body.ToString(CultureInfo.InvariantCulture);
            LoadFrames();
        }

        private void LoadFrames()
        {
            if (_animBody < 0)
            {
                return;
            }
            int body = _animBody;
            int dir = Math.Max(0, _animDir.SelectedIndex);
            AnimationFrame[] frames = null;
            int fileType = 1, converted = body;
            try
            {
                lock (SdkSync.Lock)
                {
                    int hue = 0;
                    frames = Animations.GetAnimation(body, (int)_animAction.Value, dir, ref hue, false, false);
                    fileType = BodyConverter.Convert(ref converted);
                }
            }
            catch (Exception)
            {
                frames = null;
            }
            if (frames != null && dir > 4)
            {
                // the client mirrors directions 5-7
                frames = frames.Select(fr =>
                {
                    if (fr?.Bitmap == null)
                    {
                        return fr;
                    }
                    var copy = new Bitmap(fr.Bitmap);
                    copy.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    return new AnimationFrame { Bitmap = copy, Center = fr.Center };
                }).ToArray();
            }
            _frames = frames;
            _frame = 0;
            string name = _mobs.FirstOrDefault(m => m.Body == body).Name;
            int group = body < 200 ? 0 : body < 400 ? 1 : 2;
            _animInfo.Text = $"Body     {body} (0x{body:X})\nName     {name}\nFile     {(fileType <= 1 ? "anim.mul" : $"anim{fileType}.mul")} (body {converted} there)\n" +
                             $"Group    {(group == 0 ? "monster, 22 actions" : group == 1 ? "animal, 13 actions" : "human / equipment, 35 actions")}\n" +
                             $"Frames   {(frames?.Length ?? 0)} in action {_animAction.Value}\n\n" +
                             "ServUO:  Body = " + body + ";\nSphere:  ID=0" + body.ToString("x", CultureInfo.InvariantCulture) +
                             (frames == null || frames.Length == 0 ? "\n\n(no frames for this action / direction)" : string.Empty);
            _animPreview.Invalidate();
        }

        #endregion

        #region Sounds

        private ListView _soundView;
        private TextBox _soundSearch;
        private Label _soundInfo;

        private void BuildSounds()
        {
            var page = new TabPage("Sounds");
            _soundSearch = new TextBox { Width = 200, PlaceholderText = "name or ID / Name oder ID" };
            _soundSearch.TextChanged += (s, e) => FillSounds();
            _soundView = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false };
            _soundView.Columns.Add("ID", 70);
            _soundView.Columns.Add("Hex", 70);
            _soundView.Columns.Add("Name", 300);
            _soundView.DoubleClick += (s, e) => PlaySelected(0);
            _soundView.SelectedIndexChanged += (s, e) => ShowSoundInfo();
            var play = new Button { Text = "▶ Play", AutoSize = true };
            play.Click += (s, e) => PlaySelected(0);
            var stop = new Button { Text = "■", AutoSize = true };
            stop.Click += (s, e) => _player.Stop();
            var set = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, Padding = new Padding(4) };
            set.Controls.Add(new Label
            {
                AutoSize = true,
                MaximumSize = new Size(300, 0),
                Text = "Creature sound set (ServUO BaseSoundID = first of 5):\nKreatur-Sounds (ServUO BaseSoundID = erster von 5):"
            });
            string[] names = { "+0 anger / Wut", "+1 idle / Ruhe", "+2 attack / Angriff", "+3 hurt / Schmerz", "+4 death / Tod" };
            for (int i = 0; i < 5; i++)
            {
                int offset = i;
                var b = new Button { Text = "▶ " + names[i], Width = 200, Height = 26 };
                b.Click += (s, e) => PlaySelected(offset);
                set.Controls.Add(b);
            }
            _soundInfo = new Label { Dock = DockStyle.Fill, Padding = new Padding(4), Font = new Font("Consolas", 9f) };
            var right = new Panel { Dock = DockStyle.Right, Width = 320 };
            right.Controls.Add(_soundInfo);
            right.Controls.Add(set);
            page.Controls.Add(_soundView);
            page.Controls.Add(right);
            page.Controls.Add(Bar(Lbl("Search:"), _soundSearch, play, stop));
            _tabs.TabPages.Add(page);
        }

        private void FillSounds()
        {
            if (_soundList == null)
            {
                _soundList = new List<(int, string)>();
                try
                {
                    lock (SdkSync.Lock)
                    {
                        for (int id = 0; id < 0x1000; id++)
                        {
                            if (Sounds.IsValidSound(id, out string name, out _))
                            {
                                _soundList.Add((id, name ?? string.Empty));
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // sound.mul missing
                }
            }
            string f = _soundSearch.Text.Trim();
            bool isId = ScriptTemplate.TryParseInt(f, out long sid);
            _soundView.BeginUpdate();
            _soundView.Items.Clear();
            foreach ((int id, string name) in _soundList.Where(x => f.Length == 0 || (isId && x.Id == sid) || x.Name.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                _soundView.Items.Add(new ListViewItem(new[] { id.ToString(CultureInfo.InvariantCulture), $"0x{id:X3}", name }) { Tag = id });
            }
            _soundView.EndUpdate();
            SetStatus(_soundList.Count == 0 ? "sound.mul is not loaded." : $"{_soundView.Items.Count} sounds. Double click = play. / Doppelklick = abspielen.");
        }

        private void ShowSoundInfo()
        {
            if (_soundView.SelectedItems.Count == 0)
            {
                return;
            }
            int id = (int)_soundView.SelectedItems[0].Tag;
            var lines = new List<string> { $"Sound    {id} (0x{id:X3})", string.Empty, "As BaseSoundID:" };
            string[] roles = { "anger", "idle", "attack", "hurt", "death" };
            for (int i = 0; i < 5; i++)
            {
                (int Id, string Name) s = _soundList.FirstOrDefault(x => x.Id == id + i);
                lines.Add($"  +{i} {roles[i],-7} {id + i,5}  {s.Name}");
            }
            lines.Add(string.Empty);
            lines.Add($"ServUO:  BaseSoundID = {id};");
            lines.Add($"         Effects.PlaySound(loc, map, 0x{id:X});");
            lines.Add($"Sphere:  SOUND=0{id:x}");
            _soundInfo.Text = string.Join("\n", lines);
        }

        private void PlaySelected(int offset)
        {
            if (_soundView.SelectedItems.Count == 0)
            {
                return;
            }
            int id = (int)_soundView.SelectedItems[0].Tag + offset;
            try
            {
                UoSound sound;
                lock (SdkSync.Lock)
                {
                    sound = Sounds.GetSound(id);
                }
                if (sound?.Buffer == null)
                {
                    SetStatus($"Sound {id} does not exist.");
                    return;
                }
                _player.Stop();
                _player.Stream = new MemoryStream(sound.Buffer);
                _player.Play();
                SetStatus($"Playing {id} (0x{id:X3}) {sound.Name}");
            }
            catch (Exception ex)
            {
                SetStatus("Cannot play: " + ex.Message);
            }
        }

        #endregion

        #region Hues

        private ListBox _hueList;
        private TextBox _hueSearch, _hueItem;
        private Panel _huePreview;

        private sealed class HueRow
        {
            public int Number;
            public string Name;
            public Color[] Colors;
            public override string ToString() => $"{Number} {Name}";
        }

        private void BuildHues()
        {
            var page = new TabPage("Hues");
            _hueSearch = new TextBox { Width = 200, PlaceholderText = "name or number / Name oder Nummer" };
            _hueSearch.TextChanged += (s, e) => FillHues();
            _hueList = new ListBox { Dock = DockStyle.Fill, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 20, IntegralHeight = false };
            _hueList.DrawItem += (s, e) =>
            {
                if (e.Index < 0)
                {
                    return;
                }
                var row = (HueRow)_hueList.Items[e.Index];
                e.DrawBackground();
                float w = 160 / 32f;
                for (int i = 0; i < 32 && row.Colors != null; i++)
                {
                    using (var b = new SolidBrush(row.Colors[i]))
                    {
                        e.Graphics.FillRectangle(b, e.Bounds.X + 2 + i * w, e.Bounds.Y + 2, w + 1, e.Bounds.Height - 4);
                    }
                }
                TextRenderer.DrawText(e.Graphics, $"{row.Number,5}  0x{row.Number:X4}  {row.Name}", e.Font, new Point(e.Bounds.X + 170, e.Bounds.Y + 2),
                    (e.State & DrawItemState.Selected) != 0 ? SystemColors.HighlightText : SystemColors.ControlText);
            };
            _hueList.SelectedIndexChanged += (s, e) => _huePreview.Invalidate();
            _hueList.DoubleClick += (s, e) => Choose();
            _hueItem = new TextBox { Width = 80, Text = "0x1517" };
            _hueItem.TextChanged += (s, e) => _huePreview.Invalidate();
            _huePreview = new Panel { Dock = DockStyle.Top, Height = 260 };
            _huePreview.Paint += (s, e) =>
            {
                int hue = _hueList.SelectedItem is HueRow r ? r.Number : 0;
                Bitmap img = ScriptTemplate.TryParseInt(_hueItem.Text, out long id) ? UoPreview.Item((int)id, hue) : null;
                DrawFit(e.Graphics, _huePreview.ClientRectangle, img, 3);
                img?.Dispose();
            };
            var right = new Panel { Dock = DockStyle.Right, Width = 300 };
            right.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(4),
                Text = "Hue number = as in scripts (Hue = 1153).\nhues.mul entry = number − 1.\n0 = original colors.\n\nHue-Nummer = wie in Skripten.\nEintrag in hues.mul = Nummer − 1.\n0 = Originalfarben."
            });
            right.Controls.Add(Bar(Lbl("Preview item:"), _hueItem));
            right.Controls.Add(_huePreview);
            page.Controls.Add(_hueList);
            page.Controls.Add(right);
            page.Controls.Add(Bar(Lbl("Search:"), _hueSearch));
            _tabs.TabPages.Add(page);
            FillHues();
        }

        private List<HueRow> _hues;

        private void FillHues()
        {
            if (_hues == null)
            {
                _hues = new List<HueRow> { new HueRow { Number = 0, Name = "(no hue)" } };
                try
                {
                    Hue[] list = Ultima.Hues.List;
                    for (int i = 0; list != null && i < list.Length; i++)
                    {
                        _hues.Add(new HueRow { Number = i + 1, Name = list[i].Name?.Trim(), Colors = UoPreview.HueColors(i + 1, out _) });
                    }
                }
                catch (Exception)
                {
                    // hues.mul missing
                }
            }
            string f = _hueSearch.Text.Trim();
            bool isId = ScriptTemplate.TryParseInt(f, out long n);
            _hueList.BeginUpdate();
            _hueList.Items.Clear();
            foreach (HueRow r in _hues.Where(h => f.Length == 0 || (isId && h.Number == n) || (h.Name ?? string.Empty).IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                _hueList.Items.Add(r);
            }
            _hueList.EndUpdate();
        }

        #endregion

        #region Gumps

        private ArtTileGrid _gumpGrid;
        private ComboBox _gumpRange;
        private TextBox _gumpSearch;
        private Panel _gumpPreview;

        private void BuildGumps()
        {
            var page = new TabPage("Gumps");
            _gumpRange = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
            _gumpRange.Items.AddRange(new object[] { "Interface, containers, books (0 – 49999)", "Paperdoll male (50000 – 59999)", "Paperdoll female (60000+)", "All" });
            _gumpRange.SelectedIndex = 0;
            _gumpRange.SelectedIndexChanged += (s, e) => FillGumps();
            _gumpSearch = new TextBox { Width = 100, PlaceholderText = "ID" };
            _gumpSearch.TextChanged += (s, e) => { if (ScriptTemplate.TryParseInt(_gumpSearch.Text, out long id)) _gumpGrid.SelectItem((int)id); };
            _gumpGrid = new ArtTileGrid { Dock = DockStyle.Fill, CellSize = new Size(90, 104), Text = "gumps are not loaded." };
            _gumpGrid.ImageProvider = id =>
            {
                lock (SdkSync.Lock)
                {
                    return Gumps.GetGump(id);
                }
            };
            _gumpGrid.LabelProvider = id => $"0x{id:X4}";
            _gumpGrid.SelectionChanged += (s, e) => _gumpPreview.Invalidate();
            _gumpGrid.ItemActivated += (s, e) => Choose();
            _gumpPreview = new Panel { Dock = DockStyle.Right, Width = 320 };
            _gumpPreview.Paint += (s, e) =>
            {
                Bitmap img = null;
                int id = _gumpGrid.SelectedItem;
                if (id >= 0)
                {
                    lock (SdkSync.Lock)
                    {
                        img = Gumps.GetGump(id);
                    }
                }
                DrawFit(e.Graphics, _gumpPreview.ClientRectangle, img, 2);
                if (id >= 0)
                {
                    TextRenderer.DrawText(e.Graphics, $"0x{id:X4} ({id})" + (img != null ? $"  {img.Width}×{img.Height}" : string.Empty), Font,
                        new Point(6, _gumpPreview.Height - 22), Color.Gainsboro);
                }
            };
            page.Controls.Add(_gumpGrid);
            page.Controls.Add(_gumpPreview);
            page.Controls.Add(Bar(_gumpRange, Lbl("Go to:"), _gumpSearch,
                new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(8, 7, 3, 0), Text = "Container gumps: DefaultGumpID (e.g. 0x3C backpack, 0x3E box, 0x43 wooden box)" }));
            _tabs.TabPages.Add(page);
        }

        private void FillGumps()
        {
            var list = new List<int>();
            try
            {
                lock (SdkSync.Lock)
                {
                    int count = Gumps.GetCount();
                    int from = 0, to = count;
                    switch (_gumpRange.SelectedIndex)
                    {
                        case 0: to = Math.Min(count, 50000); break;
                        case 1: from = 50000; to = Math.Min(count, 60000); break;
                        case 2: from = 60000; break;
                    }
                    for (int i = from; i < to; i++)
                    {
                        if (Gumps.IsValidIndex(i))
                        {
                            list.Add(i);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // gumps missing
            }
            _gumpGrid.Items = list;
            SetStatus($"{list.Count} gumps.");
        }

        #endregion

        #region Cliloc

        private ListView _clilocView;
        private TextBox _clilocSearch;

        private void BuildCliloc()
        {
            var page = new TabPage("Cliloc (texts)");
            _clilocSearch = new TextBox { Width = 300, PlaceholderText = "number or text, Enter = search / Nummer oder Text" };
            _clilocSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; FillCliloc(); } };
            var go = new Button { Text = "Search", AutoSize = true };
            go.Click += (s, e) => FillCliloc();
            _clilocView = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false };
            _clilocView.Columns.Add("Number", 90);
            _clilocView.Columns.Add("Text", 800);
            _clilocView.DoubleClick += (s, e) => Choose();
            page.Controls.Add(_clilocView);
            page.Controls.Add(Bar(Lbl("Search:"), _clilocSearch, go,
                new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(8, 7, 3, 0), Text = "Used for LabelNumber (item names), messages and properties." }));
            _tabs.TabPages.Add(page);
        }

        private void FillCliloc()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                if (_cliloc == null)
                {
                    _cliloc = new StringList("enu", UoFiddler.Controls.Classes.Options.NewClilocFormat);
                }
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                SetStatus("cliloc.enu could not be read: " + ex.Message);
                return;
            }
            string f = _clilocSearch.Text.Trim();
            bool isNumber = int.TryParse(f, out int number);
            _clilocView.BeginUpdate();
            _clilocView.Items.Clear();
            int shown = 0;
            foreach (StringEntry entry in _cliloc.Entries)
            {
                if (f.Length > 0 && !(isNumber && entry.Number == number) && (entry.Text ?? string.Empty).IndexOf(f, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }
                _clilocView.Items.Add(new ListViewItem(new[] { entry.Number.ToString(CultureInfo.InvariantCulture), entry.Text }) { Tag = entry.Number });
                if (++shown >= 2000)
                {
                    break;
                }
            }
            _clilocView.EndUpdate();
            Cursor = Cursors.Default;
            SetStatus(shown >= 2000 ? "First 2000 results – search more exactly." : $"{shown} entries.");
            if (isNumber && _clilocView.Items.Count > 0)
            {
                _clilocView.Items[0].Selected = true;
            }
        }

        #endregion

        #region Overview

        public const string OverviewText =
@"WHERE DO I FIND WHICH ID? / WO FINDE ICH WELCHE ID?

ITEM ID (graphic of an item / Grafik eines Items)
  • Tab Items here, or UoFiddler tab ""Items"" (art.mul). Name, flags, layer and AnimID come from tiledata.mul.
  • ServUO: base(0x1517) or ItemID = 0x1517 · Sphere: ID=01517
  • Flip IDs = the same item turned (usually the next ID): [Flipable(0x1517, 0x1518)]

BODY (animation of a creature / Animation einer Kreatur)
  • Tab Animations here, or UoFiddler tab ""Animations"" / ""Animation list"". Names: Animationlist.xml.
  • Which file: bodyconv.def maps a body to anim2 … anim5; body.def replaces missing bodies; mobtypes.txt says
    MONSTER / ANIMAL / SEA_MONSTER / HUMAN / EQUIPMENT (changes which actions the client plays).
  • 400 / 401 = human male / female, 0x190 / 0x191 the same in hex.
  • ServUO: Body = 0x3C; mounts also need the mount item ID (layer 25) · Sphere: ID=03c in CHARDEF.

SOUND
  • Tab Sounds here, or UoFiddler tab ""Sounds"" (sound.mul). The number in scripts is the ID shown here.
  • Creatures: BaseSoundID = first of 5 sounds: +0 anger, +1 idle, +2 attack, +3 hurt, +4 death.
  • Weapons: DefHitSound / DefMissSound · Doors: open / close sound · Sphere: SOUND=0xxx

HUE (color / Farbe)
  • Tab Hues here, or UoFiddler tab ""Hues"" (hues.mul). Script hue = number; hues.mul entry = number − 1.
  • Only gray pixels are hued when the item has the tiledata flag PartialHue.

GUMP
  • Tab Gumps here, or UoFiddler tab ""Gumps"" (gumpart). Containers: DefaultGumpID (0x3C backpack, 0x3E box …).
  • Paperdoll: male gump = AnimID + 50000, female gump = AnimID + 60000 (see the UO calculator).

CLILOC (texts / Texte)
  • Tab Cliloc here, or UoFiddler tab ""Cliloc"". LabelNumber = cliloc number of the item name (1023651 …).

LAYER, WEIGHT, FLAGS
  • tiledata.mul (UoFiddler tab ""TileData""): Wearable + layer (Quality) + Animation for clothing and weapons.

DE – KURZ:
  Item-ID → Reiter Items / UoFiddler Items · Body → Reiter Animations (bodyconv.def, mobtypes.txt) ·
  Sound → Reiter Sounds (BaseSoundID = erster von 5) · Hue → Reiter Hues · Gump → Reiter Gumps ·
  Cliloc → Reiter Cliloc · Layer / Flags → tiledata.mul";

        private void BuildOverview()
        {
            var page = new TabPage("Overview / Übersicht");
            page.Controls.Add(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.75f),
                BackColor = SystemColors.Window,
                Text = OverviewText.Replace("\r\n", "\n").Replace("\n", "\r\n")
            });
            _tabs.TabPages.Add(page);
        }

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C) && !(ActiveControl is TextBoxBase))
            {
                CopyCurrent();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _animTimer.Stop();
            _animTimer.Dispose();
            _player.Stop();
            _player.Dispose();
            _tip.Dispose();
            base.OnFormClosed(e);
        }
    }
}
