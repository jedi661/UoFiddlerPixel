/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.SendItem.Classes;

namespace UoFiddler.Plugin.SendItem.Forms
{
    /// <summary>
    /// SendItem: browse and search statics with preview, place them on a ServUO / RunUO shard,
    /// and keep statics and commands as buttons on category tabs.
    /// </summary>
    public partial class SendItemOptionsForm : Form
    {
        private const string ButtonDragFormat = "SendItem.ButtonId";
        private const int MaxHistory = 100;

        private static SendItemOptionsForm _instance;

        private static readonly string[] _flagFilters =
        {
            "Wall", "Roof", "Surface", "Window", "Door", "Foliage", "Container", "LightSource",
            "Wet", "Impassable", "Bridge", "Background", "Wearable", "Animation", "PartialHue"
        };

        private readonly SendItemData _data;
        private readonly List<Image> _buttonImages = new();
        private readonly System.Windows.Forms.Timer _clientTimer = new() { Interval = 2000 };
        private readonly ContextMenuStrip _contextTabArea = new();

        private List<int> _allIds = new();
        private string[] _namesLower = Array.Empty<string>();
        private bool _staticsLoaded;
        private bool _sending;
        private bool _initializing = true;
        private Button _menuButton;
        private Point _buttonDragStart;
        private Button _buttonDragSource;
        private Font _smallFont;

        #region Construction / singleton

        public SendItemOptionsForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _data = SendItemStore.Load();

            // Items tab settings (old SendItem options)
            cmdtext.Text = SendItemPluginBase.Cmd;
            argstext.Text = SendItemPluginBase.CmdArg;
            SendOnClick.Checked = SendItemPluginBase.OverrideClick;
            cmdtext.TextChanged += (_, _) => SendItemPluginBase.Cmd = cmdtext.Text;
            argstext.TextChanged += (_, _) => SendItemPluginBase.CmdArg = argstext.Text;
            SendOnClick.CheckedChanged += (_, _) => SendItemPluginBase.OverrideClick = SendOnClick.Checked;

            txtDelay.Text = _data.SendDelayMs.ToString(CultureInfo.InvariantCulture);
            txtDelay.Leave += (_, _) => ReadDelay();
            BtnFocusClient.Checked = _data.FocusClientAfterSend;
            BtnFocusClient.CheckedChanged += (_, _) => _data.FocusClientAfterSend = BtnFocusClient.Checked;

            comboTemplate.Items.AddRange(CommandText.PlaceTemplatePresets.Cast<object>().ToArray());
            comboTemplate.Text = _data.PlaceTemplate;

            comboFlag.Items.Add("All statics");
            comboFlag.Items.Add("Only with name");
            foreach (string flag in _flagFilters)
            {
                if (Enum.TryParse(flag, out TileFlag _))
                {
                    comboFlag.Items.Add(flag);
                }
            }

            comboFlag.SelectedIndex = 0;
            int sizeIndex = comboSize.Items.IndexOf(_data.TileSize.ToString(CultureInfo.InvariantCulture));
            comboSize.SelectedIndex = sizeIndex < 0 ? 1 : sizeIndex;

            staticGrid.NameProvider = StaticImages.GetName;
            staticGrid.TileSize = _data.TileSize;

            var areaNewCommand = new ToolStripMenuItem("New command button…");
            areaNewCommand.Click += BtnNewCommand_Click;
            var areaAddStatics = new ToolStripMenuItem("Add selected statics");
            areaAddStatics.Click += BtnAddStatics_Click;
            _contextTabArea.Items.Add(areaNewCommand);
            _contextTabArea.Items.Add(areaAddStatics);

            tabButtons.AllowDrop = true;
            tabButtons.DragOver += TabButtons_DragOver;

            BuildTabs();
            RefreshHistory();
            UpdateClientStatus();

            _clientTimer.Tick += (_, _) => UpdateClientStatus();
            _clientTimer.Start();

            Load += SendItemOptionsForm_Load;
            KeyDown += SendItemOptionsForm_KeyDown;
            FormClosing += (_, _) => SaveAll();
            FormClosed += (_, _) =>
            {
                _clientTimer.Dispose();
                _contextTabArea.Dispose();
                DisposeButtonImages();
                pictureStatic.Image?.Dispose();
                _smallFont?.Dispose();
                if (_instance == this)
                {
                    _instance = null;
                }
            };

            _initializing = false;
        }

        /// <summary>
        /// Only one SendItem window: returns the open one or creates it.
        /// </summary>
        public static SendItemOptionsForm GetInstance()
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new SendItemOptionsForm();
            }
            else if (_instance.WindowState == FormWindowState.Minimized)
            {
                _instance.WindowState = FormWindowState.Normal;
            }

            return _instance;
        }

        private async void SendItemOptionsForm_Load(object sender, EventArgs e)
        {
            try
            {
                splitMain.SplitterDistance = Math.Clamp(_data.SplitterDistance, splitMain.Panel1MinSize,
                    Math.Max(splitMain.Panel1MinSize, splitMain.Width - splitMain.Panel2MinSize));
            }
            catch (InvalidOperationException)
            {
                // window too small for the saved position
            }

            await LoadStaticsAsync();
        }

        private void SendItemOptionsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                textSearch.Focus();
                textSearch.SelectAll();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void SaveAll()
        {
            try
            {
                ReadDelay();
                _data.PlaceTemplate = string.IsNullOrWhiteSpace(comboTemplate.Text) ? CommandText.PlaceTemplatePresets[0] : comboTemplate.Text.Trim();
                _data.SplitterDistance = splitMain.SplitterDistance;
                SendItemStore.Save(_data);
            }
            catch (Exception ex)
            {
                SetStatus("Could not save: " + ex.Message);
            }
        }

        private void ReadDelay()
        {
            if (int.TryParse(txtDelay.Text.Trim(), out int ms) && ms >= 0 && ms <= 10000)
            {
                _data.SendDelayMs = ms;
            }
            else
            {
                txtDelay.Text = _data.SendDelayMs.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void SetStatus(string text) => lblStatus.Text = text;

        private void UpdateClientStatus()
        {
            bool running;
            try
            {
                running = Client.Running;
            }
            catch
            {
                running = false;
            }

            lblClient.Text = running ? "● Client found" : "● Client not found";
            lblClient.ForeColor = running ? Color.FromArgb(20, 135, 60) : Color.FromArgb(195, 40, 40);
        }

        #endregion

        #region Static browser – loading and search

        private async Task LoadStaticsAsync()
        {
            SetStatus("Loading statics …");
            UseWaitCursor = true;
            try
            {
                int max = Art.GetMaxItemId();
                var ids = new List<int>(max + 1);
                var names = new string[max + 1];

                for (int i = 0; i <= max; i++)
                {
                    if (Art.IsValidStatic(i))
                    {
                        ids.Add(i);
                        names[i] = StaticImages.GetName(i).ToLowerInvariant();
                    }

                    // keep the window responsive while scanning the art file
                    if (i % 4000 == 0)
                    {
                        await Task.Delay(1);
                        if (IsDisposed)
                        {
                            return;
                        }
                    }
                }

                _allIds = ids;
                _namesLower = names;
                _staticsLoaded = true;
                ApplyFilter();
                SetStatus($"{ids.Count:N0} statics loaded. Double click / Enter = place in game, drag onto a tab = save as button.");
            }
            catch (Exception ex)
            {
                SetStatus("Could not load the statics: " + ex.Message);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void TextSearch_TextChanged(object sender, EventArgs e)
        {
            timerSearch.Stop();
            timerSearch.Start();
        }

        private void TimerSearch_Tick(object sender, EventArgs e)
        {
            timerSearch.Stop();
            ApplyFilter();
        }

        private void TextSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                timerSearch.Stop();
                ApplyFilter();
                if (staticGrid.Items.Count > 0)
                {
                    if (staticGrid.FocusedId < 0)
                    {
                        staticGrid.SelectId(staticGrid.Items[0]);
                    }

                    staticGrid.Focus();
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape && textSearch.TextLength > 0)
            {
                textSearch.Clear();
                e.SuppressKeyPress = true;
            }
        }

        private void ComboFlag_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_initializing)
            {
                ApplyFilter();
            }
        }

        private void ComboSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(comboSize.SelectedItem?.ToString(), out int size))
            {
                _data.TileSize = size;
                staticGrid.TileSize = size;
            }
        }

        /// <summary>
        /// Search: every term must match. Terms can be a name part, an id (0x1234 / 4660) or a range (0x1000-0x10FF).
        /// </summary>
        private void ApplyFilter()
        {
            if (!_staticsLoaded)
            {
                return;
            }

            string[] terms = textSearch.Text.Trim().ToLowerInvariant()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var matchers = new List<Func<int, bool>>();
            int exactId = -1;

            foreach (string term in terms)
            {
                int dash = term.IndexOf('-', 1);
                if (dash > 0 && CommandText.TryParseId(term.Substring(0, dash), out int from) &&
                    CommandText.TryParseId(term.Substring(dash + 1), out int to))
                {
                    int a = Math.Min(from, to), b = Math.Max(from, to);
                    matchers.Add(id => id >= a && id <= b);
                }
                else if (term.StartsWith("0x", StringComparison.Ordinal) && CommandText.TryParseId(term, out int hex))
                {
                    exactId = hex;
                    string prefix = term.Substring(2).TrimStart('0');
                    matchers.Add(id => id == hex || (prefix.Length > 0 && id.ToString("x", CultureInfo.InvariantCulture).StartsWith(prefix, StringComparison.Ordinal)));
                }
                else if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out int dec))
                {
                    exactId = dec;
                    string t = term;
                    matchers.Add(id => id == dec || (_namesLower[id] ?? "").Contains(t, StringComparison.Ordinal));
                }
                else
                {
                    string t = term;
                    matchers.Add(id => (_namesLower[id] ?? "").Contains(t, StringComparison.Ordinal));
                }
            }

            string flagText = comboFlag.SelectedIndex > 1 ? comboFlag.SelectedItem?.ToString() : null;
            TileFlag flag = TileFlag.None;
            bool filterFlag = flagText != null && Enum.TryParse(flagText, out flag);
            bool onlyNamed = comboFlag.SelectedIndex == 1;
            ItemData[] table = TileData.ItemTable;

            var result = new List<int>();
            foreach (int id in _allIds)
            {
                if (onlyNamed && string.IsNullOrEmpty(_namesLower[id]))
                {
                    continue;
                }

                if (filterFlag && (table == null || id >= table.Length || (table[id].Flags & flag) == 0))
                {
                    continue;
                }

                bool ok = true;
                foreach (Func<int, bool> m in matchers)
                {
                    if (!m(id))
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    result.Add(id);
                }
            }

            // exact id first
            if (exactId >= 0 && result.Remove(exactId))
            {
                result.Insert(0, exactId);
            }

            staticGrid.SetItems(result);
            if (exactId >= 0 && result.Count > 0 && result[0] == exactId)
            {
                staticGrid.SelectId(exactId);
            }

            lblCount.Text = result.Count == _allIds.Count
                ? $"{_allIds.Count:N0} statics"
                : $"{result.Count:N0} of {_allIds.Count:N0} statics";
        }

        #endregion

        #region Static browser – preview and placing

        private int CurrentStaticId => staticGrid.FocusedId >= 0 ? staticGrid.FocusedId : staticGrid.SelectedIds.FirstOrDefault(-1);

        private int CurrentHue => (int)numHue.Value;

        private string CurrentTemplate =>
            string.IsNullOrWhiteSpace(comboTemplate.Text) ? CommandText.PlaceTemplatePresets[0] : comboTemplate.Text.Trim();

        private void StaticGrid_SelectionChanged(object sender, EventArgs e) => UpdatePreview();

        private void NumHue_ValueChanged(object sender, EventArgs e) => UpdatePreview();

        private void ComboTemplate_TextChanged(object sender, EventArgs e)
        {
            if (!_initializing)
            {
                _data.PlaceTemplate = CurrentTemplate;
            }

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            Image old = pictureStatic.Image;
            pictureStatic.Image = null;
            old?.Dispose();

            int id = CurrentStaticId;
            int selectedCount = staticGrid.SelectedIds.Count;
            BtnPlace.Enabled = id >= 0;
            BtnAddToTab.Enabled = selectedCount > 0;
            BtnCopyId.Enabled = id >= 0;

            if (id < 0)
            {
                lblStaticInfo.Text = "Select a static";
                txtCommandPreview.Text = "";
                return;
            }

            string name = StaticImages.GetName(id);
            Bitmap bmp = StaticImages.GetHued(id, CurrentHue);
            if (bmp != null)
            {
                pictureStatic.SizeMode = bmp.Width > pictureStatic.ClientSize.Width || bmp.Height > pictureStatic.ClientSize.Height
                    ? PictureBoxSizeMode.Zoom
                    : PictureBoxSizeMode.CenterImage;
                pictureStatic.Image = bmp;
            }

            var info = new StringBuilder();
            info.Append($"0x{id:X4}  ({id})");
            if (selectedCount > 1)
            {
                info.Append($"   ·   {selectedCount} selected");
            }

            info.AppendLine();
            info.AppendLine(name.Length > 0 ? name : "(no name)");

            try
            {
                if (id < TileData.ItemTable.Length)
                {
                    ItemData data = TileData.ItemTable[id];
                    string flags = string.Join(", ", _flagFilters.Where(f => Enum.TryParse(f, out TileFlag tf) && (data.Flags & tf) != 0));
                    info.Append($"Height {data.Height}" + (flags.Length > 0 ? "  ·  " + flags : ""));
                }
            }
            catch
            {
                // no tiledata
            }

            lblStaticInfo.Text = info.ToString();
            txtCommandPreview.Text = CommandText.Expand(CurrentTemplate, id, name, CurrentHue);
        }

        private async void StaticGrid_ItemActivated(object sender, int id) => await PlaceStaticAsync(id, CurrentHue, CurrentTemplate);

        private async void BtnPlace_Click(object sender, EventArgs e)
        {
            int id = CurrentStaticId;
            if (id >= 0)
            {
                await PlaceStaticAsync(id, CurrentHue, CurrentTemplate);
            }
        }

        private async Task PlaceStaticAsync(int id, int hue, string template)
        {
            string name = StaticImages.GetName(id);
            string command = CommandText.Expand(template, id, name, hue);
            await SendLinesAsync(new[] { command }, $"0x{id:X4} {name}".Trim());
        }

        private void BtnCopyId_Click(object sender, EventArgs e)
        {
            int id = CurrentStaticId;
            if (id >= 0)
            {
                Clipboard.SetText($"0x{id:X4}");
                SetStatus($"Copied 0x{id:X4}");
            }
        }

        private void BtnFromItems_Click(object sender, EventArgs e)
        {
            int id = SendItemPluginBase.GetItemsTabSelectedId();
            if (id < 0)
            {
                SetStatus("No item selected in the Items tab.");
                return;
            }

            comboFlag.SelectedIndex = 0;
            textSearch.Text = $"0x{id:X4}";
            timerSearch.Stop();
            ApplyFilter();
            staticGrid.SelectId(id);
        }

        private void ContextGrid_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int count = staticGrid.SelectedIds.Count;
            if (count == 0)
            {
                e.Cancel = true;
                return;
            }

            menuGridAddToTab.Text = count == 1
                ? $"Add to tab \"{CurrentTab?.Name}\""
                : $"Add {count} statics to tab \"{CurrentTab?.Name}\"";
        }

        private void MenuGridPlace_Click(object sender, EventArgs e) => BtnPlace_Click(sender, e);

        private void MenuGridAddToTab_Click(object sender, EventArgs e) => BtnAddStatics_Click(sender, e);

        private void MenuGridCopyId_Click(object sender, EventArgs e)
        {
            List<int> ids = staticGrid.SelectedIds;
            if (ids.Count > 0)
            {
                Clipboard.SetText(string.Join(Environment.NewLine, ids.Select(i => $"0x{i:X4}")));
                SetStatus($"Copied {ids.Count} id(s)");
            }
        }

        #endregion

        #region Sending

        /// <summary>
        /// Sends the lines to the client one after another. "#wait 500" pauses, "//" lines are comments.
        /// </summary>
        private async Task<bool> SendLinesAsync(IEnumerable<string> lines, string label)
        {
            List<string> list = lines.Select(t => (t ?? "").Trim())
                .Where(t => t.Length > 0 && !t.StartsWith("//", StringComparison.Ordinal))
                .ToList();
            if (list.Count == 0)
            {
                return false;
            }

            UpdateClientStatus();
            if (!Client.Running)
            {
                SystemSounds.Beep.Play();
                SetStatus("Game client not found – start the client and log in first.");
                return false;
            }

            if (_sending)
            {
                SetStatus("Still sending the previous commands …");
                return false;
            }

            ReadDelay();
            _sending = true;
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string line = list[i];
                    if (line.StartsWith("#wait", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(line.Substring(5).Trim(), out int ms) && ms > 0)
                        {
                            await Task.Delay(Math.Min(ms, 60000));
                        }

                        continue;
                    }

                    if (!Client.SendText(line))
                    {
                        SetStatus("Sending failed – was the client closed?");
                        return false;
                    }

                    AddHistory(line);
                    if (i < list.Count - 1 && _data.SendDelayMs > 0)
                    {
                        await Task.Delay(_data.SendDelayMs);
                    }
                }
            }
            finally
            {
                _sending = false;
            }

            if (_data.FocusClientAfterSend)
            {
                Client.BringToTop();
            }

            SetStatus($"Sent: {label}" + (list.Count > 1 ? $"  ({list.Count} lines)" : ""));
            return true;
        }

        private async Task SendButtonAsync(CommandButton b)
        {
            if (b.Kind == ButtonKind.Static)
            {
                string template = string.IsNullOrWhiteSpace(b.Template) ? CurrentTemplate : b.Template;
                await PlaceStaticAsync(b.StaticId, b.Hue, template);
                return;
            }

            int id = CurrentStaticId;
            string name = id >= 0 ? StaticImages.GetName(id) : "";
            string[] lines = (b.Commands ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (id < 0 && lines.Any(l => l.Contains("{hex}") || l.Contains("{id}") || l.Contains("{name}")))
            {
                SetStatus($"\"{b.Text}\" uses the selected static – select one on the left first.");
                SystemSounds.Beep.Play();
                return;
            }

            IEnumerable<string> expanded = lines.Select(l => l.Contains('{') ? CommandText.Expand(l, Math.Max(id, 0), name, CurrentHue) : l);
            await SendLinesAsync(expanded, b.Text);
        }

        #endregion

        #region History

        private void AddHistory(string line)
        {
            _data.History.Remove(line);
            _data.History.Insert(0, line);
            if (_data.History.Count > MaxHistory)
            {
                _data.History.RemoveRange(MaxHistory, _data.History.Count - MaxHistory);
            }

            RefreshHistory();
        }

        private void RefreshHistory()
        {
            listHistory.BeginUpdate();
            listHistory.Items.Clear();
            listHistory.Items.AddRange(_data.History.Cast<object>().ToArray());
            listHistory.EndUpdate();
        }

        private async void MenuHistorySend_Click(object sender, EventArgs e)
        {
            if (listHistory.SelectedItem is string line)
            {
                await SendLinesAsync(new[] { line }, line);
            }
        }

        private void MenuHistorySave_Click(object sender, EventArgs e)
        {
            if (listHistory.SelectedItem is not string line || CurrentTab == null)
            {
                return;
            }

            var b = new CommandButton { Kind = ButtonKind.Command, Commands = line, Text = line.TrimStart('[', '.') };
            using var dialog = new CommandButtonDialog(b, CurrentTemplate, true);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                CurrentTab.Buttons.Add(b);
                AfterTabsChanged();
            }
        }

        private void MenuHistoryClear_Click(object sender, EventArgs e)
        {
            _data.History.Clear();
            RefreshHistory();
        }

        #endregion

        #region Tabs and buttons – building

        private CommandTab CurrentTab => tabButtons.SelectedTab?.Tag as CommandTab;

        private void DisposeButtonImages()
        {
            foreach (Image img in _buttonImages)
            {
                img.Dispose();
            }

            _buttonImages.Clear();
        }

        private void BuildTabs(CommandTab select = null)
        {
            select ??= CurrentTab;

            tabButtons.SuspendLayout();
            foreach (TabPage page in tabButtons.TabPages)
            {
                page.Dispose();
            }

            tabButtons.TabPages.Clear();
            DisposeButtonImages();

            foreach (CommandTab tab in _data.Tabs)
            {
                var page = new TabPage($"{tab.Name} ({tab.Buttons.Count})") { Tag = tab, UseVisualStyleBackColor = true };
                var flow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    Padding = new Padding(6),
                    AllowDrop = true,
                    Tag = tab,
                    ContextMenuStrip = _contextTabArea
                };
                flow.DragOver += Flow_DragOver;
                flow.DragDrop += Flow_DragDrop;

                flow.SuspendLayout();
                foreach (CommandButton b in tab.Buttons)
                {
                    flow.Controls.Add(CreateButton(b));
                }

                if (tab.Buttons.Count == 0)
                {
                    flow.Controls.Add(new Label
                    {
                        Text = "Empty tab.\r\n\r\nDrag statics from the left onto this area,\r\nor use \"+ Command button\" / \"+ Selected statics\".",
                        AutoSize = true,
                        ForeColor = SystemColors.GrayText,
                        Margin = new Padding(12)
                    });
                }

                flow.ResumeLayout();
                page.Controls.Add(flow);
                tabButtons.TabPages.Add(page);
            }

            if (select != null)
            {
                foreach (TabPage page in tabButtons.TabPages)
                {
                    if (page.Tag == select)
                    {
                        tabButtons.SelectedTab = page;
                    }
                }
            }

            tabButtons.ResumeLayout();
            UpdateTabButtons();
        }

        private Button CreateButton(CommandButton b)
        {
            var btn = new Button
            {
                Tag = b,
                Text = b.Text,
                Margin = new Padding(3),
                ContextMenuStrip = contextButton,
                UseVisualStyleBackColor = b.Color == 0,
                AutoEllipsis = true
            };

            if (b.Kind == ButtonKind.Static)
            {
                Bitmap thumb = StaticImages.GetThumbnail(b.StaticId, b.Hue, 56);
                _buttonImages.Add(thumb);
                btn.Image = thumb;
                btn.Size = new Size(104, 96);
                btn.TextImageRelation = TextImageRelation.ImageAboveText;
                btn.ImageAlign = ContentAlignment.TopCenter;
                btn.TextAlign = ContentAlignment.BottomCenter;
                _smallFont ??= new Font(Font.FontFamily, 8F);
                btn.Font = _smallFont;
            }
            else
            {
                btn.AutoSize = true;
                btn.AutoSizeMode = AutoSizeMode.GrowOnly;
                btn.MinimumSize = new Size(100, 40);
                btn.MaximumSize = new Size(260, 0);
                btn.Padding = new Padding(6, 2, 6, 2);
            }

            if (b.Color != 0)
            {
                Color c = Color.FromArgb(b.Color);
                btn.BackColor = c;
                btn.ForeColor = (c.R * 299 + c.G * 587 + c.B * 114) / 1000 > 140 ? Color.Black : Color.White;
            }

            toolTip.SetToolTip(btn, ButtonTooltip(b));

            btn.Click += async (_, _) => await SendButtonAsync(b);
            btn.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _buttonDragSource = (Button)s;
                    _buttonDragStart = e.Location;
                }
            };
            btn.MouseUp += (_, _) => _buttonDragSource = null;
            btn.MouseMove += (s, e) =>
            {
                if (_buttonDragSource != s || e.Button != MouseButtons.Left)
                {
                    return;
                }

                Size drag = SystemInformation.DragSize;
                if (Math.Abs(e.X - _buttonDragStart.X) > drag.Width || Math.Abs(e.Y - _buttonDragStart.Y) > drag.Height)
                {
                    _buttonDragSource = null;
                    var data = new DataObject();
                    data.SetData(ButtonDragFormat, b.Id);
                    ((Button)s).DoDragDrop(data, DragDropEffects.Move);
                }
            };

            return btn;
        }

        private string ButtonTooltip(CommandButton b)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(b.Tooltip))
            {
                sb.AppendLine(b.Tooltip);
            }

            if (b.Kind == ButtonKind.Static)
            {
                string name = StaticImages.GetName(b.StaticId);
                sb.AppendLine($"0x{b.StaticId:X4}  {name}" + (b.Hue > 0 ? $"  hue {b.Hue}" : ""));
                string template = string.IsNullOrWhiteSpace(b.Template) ? CurrentTemplate : b.Template;
                sb.Append("Sends: " + CommandText.Expand(template, b.StaticId, name, b.Hue));
            }
            else
            {
                sb.Append(b.Commands);
            }

            return sb.ToString();
        }

        private void AfterTabsChanged(CommandTab select = null)
        {
            BuildTabs(select);
            SaveAll();
        }

        private void TabButtons_SelectedIndexChanged(object sender, EventArgs e) => UpdateTabButtons();

        private void UpdateTabButtons()
        {
            int index = tabButtons.SelectedIndex;
            BtnRenameTab.Enabled = index >= 0;
            BtnDeleteTab.Enabled = index >= 0;
            BtnTabLeft.Enabled = index > 0;
            BtnTabRight.Enabled = index >= 0 && index < tabButtons.TabCount - 1;
        }

        #endregion

        #region Tabs – add, rename, delete, move

        private void BtnNewTab_Click(object sender, EventArgs e)
        {
            string name = InputDialog.Show(this, "New tab", "Name of the category:", "");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var tab = new CommandTab { Name = name.Trim() };
            _data.Tabs.Add(tab);
            AfterTabsChanged(tab);
        }

        private void BtnRenameTab_Click(object sender, EventArgs e)
        {
            CommandTab tab = CurrentTab;
            if (tab == null)
            {
                return;
            }

            string name = InputDialog.Show(this, "Rename tab", "Name of the category:", tab.Name);
            if (!string.IsNullOrWhiteSpace(name))
            {
                tab.Name = name.Trim();
                AfterTabsChanged(tab);
            }
        }

        private void BtnDeleteTab_Click(object sender, EventArgs e)
        {
            CommandTab tab = CurrentTab;
            if (tab == null)
            {
                return;
            }

            if (tab.Buttons.Count > 0 &&
                MessageBox.Show(this, $"Delete tab \"{tab.Name}\" with {tab.Buttons.Count} button(s)?", "SendItem",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            int index = _data.Tabs.IndexOf(tab);
            _data.Tabs.Remove(tab);
            if (_data.Tabs.Count == 0)
            {
                _data.Tabs.Add(new CommandTab { Name = "Statics" });
            }

            AfterTabsChanged(_data.Tabs[Math.Clamp(index, 0, _data.Tabs.Count - 1)]);
        }

        private void MoveTab(int delta)
        {
            CommandTab tab = CurrentTab;
            int index = _data.Tabs.IndexOf(tab);
            int target = index + delta;
            if (tab == null || target < 0 || target >= _data.Tabs.Count)
            {
                return;
            }

            _data.Tabs.RemoveAt(index);
            _data.Tabs.Insert(target, tab);
            AfterTabsChanged(tab);
        }

        private void BtnTabLeft_Click(object sender, EventArgs e) => MoveTab(-1);

        private void BtnTabRight_Click(object sender, EventArgs e) => MoveTab(1);

        #endregion

        #region Buttons – add, edit, delete, move

        private void BtnNewCommand_Click(object sender, EventArgs e)
        {
            CommandTab tab = CurrentTab;
            if (tab == null)
            {
                return;
            }

            var b = new CommandButton { Kind = ButtonKind.Command };
            using var dialog = new CommandButtonDialog(b, CurrentTemplate, true);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                tab.Buttons.Add(b);
                AfterTabsChanged(tab);
            }
        }

        private void BtnAddStatics_Click(object sender, EventArgs e)
        {
            List<int> ids = staticGrid.SelectedIds;
            if (ids.Count == 0)
            {
                SetStatus("Select one or more statics on the left first (Ctrl / Shift for several).");
                return;
            }

            AddStaticButtons(CurrentTab, ids, -1);
        }

        private void BtnAddToTab_Click(object sender, EventArgs e) => BtnAddStatics_Click(sender, e);

        private void AddStaticButtons(CommandTab tab, List<int> ids, int insertAt)
        {
            if (tab == null)
            {
                return;
            }

            if (ids.Count > 200 &&
                MessageBox.Show(this, $"Add {ids.Count} buttons to \"{tab.Name}\"?", "SendItem",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            int index = insertAt < 0 || insertAt > tab.Buttons.Count ? tab.Buttons.Count : insertAt;
            foreach (int id in ids)
            {
                string name = StaticImages.GetName(id);
                tab.Buttons.Insert(index++, new CommandButton
                {
                    Kind = ButtonKind.Static,
                    StaticId = id,
                    Hue = CurrentHue,
                    Text = name.Length > 0 ? name : $"0x{id:X4}"
                });
            }

            AfterTabsChanged(tab);
            SetStatus($"{ids.Count} static(s) added to \"{tab.Name}\"");
        }

        private (CommandTab Tab, CommandButton Button) FindButton(string id)
        {
            foreach (CommandTab tab in _data.Tabs)
            {
                CommandButton b = tab.Buttons.FirstOrDefault(x => x.Id == id);
                if (b != null)
                {
                    return (tab, b);
                }
            }

            return (null, null);
        }

        private void ContextButton_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _menuButton = contextButton.SourceControl as Button;
            if (_menuButton?.Tag is not CommandButton b)
            {
                e.Cancel = true;
                return;
            }

            (CommandTab tab, _) = FindButton(b.Id);
            int index = tab?.Buttons.IndexOf(b) ?? -1;
            menuBtnLeft.Enabled = index > 0;
            menuBtnRight.Enabled = tab != null && index < tab.Buttons.Count - 1;

            menuBtnMoveTo.DropDownItems.Clear();
            foreach (CommandTab other in _data.Tabs.Where(t => t != tab))
            {
                CommandTab target = other;
                var item = new ToolStripMenuItem(other.Name);
                item.Click += (_, _) =>
                {
                    tab?.Buttons.Remove(b);
                    target.Buttons.Add(b);
                    AfterTabsChanged(tab);
                    SetStatus($"\"{b.Text}\" moved to \"{target.Name}\"");
                };
                menuBtnMoveTo.DropDownItems.Add(item);
            }

            menuBtnMoveTo.Enabled = menuBtnMoveTo.DropDownItems.Count > 0;
        }

        private CommandButton MenuButton => _menuButton?.Tag as CommandButton;

        private async void MenuBtnSend_Click(object sender, EventArgs e)
        {
            if (MenuButton != null)
            {
                await SendButtonAsync(MenuButton);
            }
        }

        private void MenuBtnEdit_Click(object sender, EventArgs e)
        {
            CommandButton b = MenuButton;
            if (b == null)
            {
                return;
            }

            (CommandTab tab, _) = FindButton(b.Id);
            CommandButton copy = b.Clone();
            copy.Id = b.Id;
            using var dialog = new CommandButtonDialog(copy, CurrentTemplate, false);
            if (dialog.ShowDialog(this) != DialogResult.OK || tab == null)
            {
                return;
            }

            tab.Buttons[tab.Buttons.IndexOf(b)] = copy;
            AfterTabsChanged(tab);
        }

        private void MenuBtnDuplicate_Click(object sender, EventArgs e)
        {
            CommandButton b = MenuButton;
            (CommandTab tab, _) = b != null ? FindButton(b.Id) : (null, null);
            if (tab == null)
            {
                return;
            }

            tab.Buttons.Insert(tab.Buttons.IndexOf(b) + 1, b.Clone());
            AfterTabsChanged(tab);
        }

        private void MenuBtnDelete_Click(object sender, EventArgs e)
        {
            CommandButton b = MenuButton;
            (CommandTab tab, _) = b != null ? FindButton(b.Id) : (null, null);
            if (tab == null)
            {
                return;
            }

            if (MessageBox.Show(this, $"Delete button \"{b.Text}\"?", "SendItem", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
            {
                tab.Buttons.Remove(b);
                AfterTabsChanged(tab);
            }
        }

        private void MoveButton(int delta)
        {
            CommandButton b = MenuButton;
            (CommandTab tab, _) = b != null ? FindButton(b.Id) : (null, null);
            if (tab == null)
            {
                return;
            }

            int index = tab.Buttons.IndexOf(b);
            int target = index + delta;
            if (target < 0 || target >= tab.Buttons.Count)
            {
                return;
            }

            tab.Buttons.RemoveAt(index);
            tab.Buttons.Insert(target, b);
            AfterTabsChanged(tab);
        }

        private void MenuBtnLeft_Click(object sender, EventArgs e) => MoveButton(-1);

        private void MenuBtnRight_Click(object sender, EventArgs e) => MoveButton(1);

        #endregion

        #region Drag & drop

        private void TabButtons_DragOver(object sender, DragEventArgs e)
        {
            // hovering over a tab header while dragging switches to that tab
            Point p = tabButtons.PointToClient(new Point(e.X, e.Y));
            for (int i = 0; i < tabButtons.TabCount; i++)
            {
                if (tabButtons.GetTabRect(i).Contains(p))
                {
                    if (tabButtons.SelectedIndex != i)
                    {
                        tabButtons.SelectedIndex = i;
                    }

                    break;
                }
            }

            e.Effect = DragDropEffects.None;
        }

        private static void Flow_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(StaticGrid.DragFormat) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data?.GetDataPresent(ButtonDragFormat) == true)
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Flow_DragDrop(object sender, DragEventArgs e)
        {
            var flow = (FlowLayoutPanel)sender;
            if (flow.Tag is not CommandTab tab)
            {
                return;
            }

            int insertAt = DropIndex(flow, flow.PointToClient(new Point(e.X, e.Y)));

            if (e.Data?.GetData(StaticGrid.DragFormat) is int[] ids)
            {
                AddStaticButtons(tab, ids.ToList(), insertAt);
                return;
            }

            if (e.Data?.GetData(ButtonDragFormat) is string id)
            {
                (CommandTab source, CommandButton b) = FindButton(id);
                if (b == null)
                {
                    return;
                }

                int oldIndex = source.Buttons.IndexOf(b);
                source.Buttons.Remove(b);
                if (source == tab && oldIndex < insertAt)
                {
                    insertAt--;
                }

                tab.Buttons.Insert(Math.Clamp(insertAt, 0, tab.Buttons.Count), b);
                AfterTabsChanged(tab);
            }
        }

        /// <summary>
        /// Index of the button under the point (drop before it), or the end of the list.
        /// </summary>
        private static int DropIndex(FlowLayoutPanel flow, Point p)
        {
            var buttons = flow.Controls.OfType<Button>().ToList();
            for (int i = 0; i < buttons.Count; i++)
            {
                Rectangle r = buttons[i].Bounds;
                if (r.Contains(p))
                {
                    return p.X > r.Left + r.Width / 2 ? i + 1 : i;
                }

                // point is left of this button on its row, or above the row
                if (p.Y < r.Top || (p.Y < r.Bottom && p.X < r.Left))
                {
                    return i;
                }
            }

            return buttons.Count;
        }

        #endregion

        #region Import / export / help

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json",
                FileName = "SendItemTabs_" + DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                SendItemStore.ExportTabs(dialog.FileName, _data.Tabs);
                SetStatus($"{_data.Tabs.Count} tab(s) exported to {dialog.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Export failed: " + ex.Message, "SendItem", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Filter = "JSON (*.json)|*.json|All files (*.*)|*.*" };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                List<CommandTab> tabs = SendItemStore.ImportTabs(dialog.FileName);
                foreach (CommandTab tab in tabs)
                {
                    string name = string.IsNullOrWhiteSpace(tab.Name) ? "Imported" : tab.Name;
                    string unique = name;
                    for (int n = 2; _data.Tabs.Any(t => string.Equals(t.Name, unique, StringComparison.OrdinalIgnoreCase)); n++)
                    {
                        unique = $"{name} ({n})";
                    }

                    tab.Name = unique;
                    _data.Tabs.Add(tab);
                }

                AfterTabsChanged(tabs.FirstOrDefault());
                SetStatus($"{tabs.Count} tab(s) imported with {tabs.Sum(t => t.Buttons.Count)} button(s)");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Import failed: " + ex.Message, "SendItem", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "STATICS (left)\r\n" +
                "• Search by name, id (0x1234 or 4660) or range (0x1000-0x10FF). Several words = all must match.\r\n" +
                "• Double click / Enter / \"Place in game\" sends the template, e.g. [add static 0x1234 – then target in game.\r\n" +
                "• Ctrl / Shift selects several statics. Drag them onto a tab to save them as buttons.\r\n\r\n" +
                "TABS (right)\r\n" +
                "• Tabs are your categories (Walls, Floors, Decoration …). Click a button to send it.\r\n" +
                "• Right click a button: edit, duplicate, move, delete. Drag buttons to sort them or onto another tab header.\r\n" +
                "• Command buttons can hold several lines; \"#wait 500\" pauses 500 ms.\r\n\r\n" +
                "PLACEHOLDERS\r\n" + CommandText.PlaceholderHelp + "\r\n\r\n" +
                "Templates: [add static {hex}{sethue} = place one,  [tile static {hex} = fill an area.\r\n\r\n" +
                "Data file: " + SendItemStore.FilePath,
                "SendItem – Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }

    /// <summary>
    /// Small text input dialog.
    /// </summary>
    internal static class InputDialog
    {
        public static string Show(IWin32Window owner, string title, string label, string value)
        {
            using var form = new Form
            {
                Text = title,
                Font = new Font("Segoe UI", 9F),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(360, 120)
            };
            var text = new TextBox { Text = value, Bounds = new Rectangle(14, 36, 330, 23) };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Bounds = new Rectangle(172, 76, 82, 28) };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(262, 76, 82, 28) };
            form.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(12, 12) });
            form.Controls.Add(text);
            form.Controls.Add(ok);
            form.Controls.Add(cancel);
            form.AcceptButton = ok;
            form.CancelButton = cancel;
            return form.ShowDialog(owner) == DialogResult.OK ? text.Text : null;
        }
    }
}
