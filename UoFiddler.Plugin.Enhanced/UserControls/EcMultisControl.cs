/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Multis;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Editor for MultiCollection.uop (houses, boats, ...), built like the Multis tab of UoFiddler:
    /// list with the names of Multilist.xml, isometric graphic with height slider, component list,
    /// highlighting of parts, image and part exports. The structure is the same as in multi.mul, only the images
    /// come from the Enhanced Client (or the legacy art).
    /// </summary>
    public sealed class EcMultisControl : UserControl, IEcEditor
    {
        public const string FileName = "MultiCollection.uop";

        // grid columns
        private const int ColShow = 0, ColItem = 1, ColName = 2, ColX = 3, ColY = 4, ColZ = 5, ColFlags = 6, ColClilocs = 7;

        private readonly TreeView _tree;
        private readonly ToolStripTextBox _searchBox;
        private readonly ToolStripButton _saveButton;
        private readonly DataGridView _parts;
        private readonly EcPreviewPanel _preview;
        private readonly TrackBar _heightBar;
        private readonly Label _heightLabel;
        private readonly NumericUpDown _minZBox;
        private readonly ComboBox _floorBox;
        private readonly ComboBox _artBox;
        private readonly CheckBox _hiddenBox;
        private readonly CheckBox _gridBox;
        private readonly PictureBox _partImage;
        private readonly Label _partInfo;
        private readonly ToolStripStatusLabel _statusLabel;
        private readonly ToolStripStatusLabel _hoverLabel;
        private readonly ToolStripMenuItem _transparencyItem;

        private EcClient _client;
        private EcUopPackage _package;
        private bool _loaded;
        private readonly List<(int Id, EcUopEntry Entry)> _multis = new List<(int, EcUopEntry)>();
        private readonly Dictionary<int, (string Name, string Tip)> _multiNames = new Dictionary<int, (string, string)>();
        private EcMulti _current;
        private EcUopEntry _currentEntry;
        private bool _filling;
        private bool _settingHeight;
        private readonly HashSet<int> _hiddenParts = new HashSet<int>();
        private readonly HashSet<int> _highlighted = new HashSet<int>();
        private int _hoverPart = -1;
        private RenderResult _render;
        private Color _backgroundColor = Color.White;

        private readonly Dictionary<(uint Id, bool Legacy), Sprite> _sprites = new Dictionary<(uint, bool), Sprite>();

        /// <summary>
        /// Item image cut to its used area, with its position relative to the tile:
        /// X from the left corner of the tile, Y from the bottom corner (negative = up).
        /// </summary>
        private sealed class Sprite
        {
            public Bitmap Image;
            public int OffsetX;
            public int OffsetY;
            public bool Background;
            public int Height;
            public string Name;
            public string Source;
        }

        private sealed class Placed
        {
            public int Index;
            public EcMultiPart Part;
            public Sprite Sprite;
            public Rectangle Rect;
            public Point[] Tile;
        }

        private sealed class RenderResult
        {
            public Bitmap Image;
            public List<Placed> Placed = new List<Placed>();
        }

        public EcMultisControl()
        {
            Dock = DockStyle.Fill;

            // ---------------------------------------------------------------- tool strip (like the Multis tab)
            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _searchBox = new ToolStripTextBox { Width = 120, ToolTipText = "Id (decimal or 0x hex) or part of the name – Enter = next hit" };
            _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Search(); } };
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false };

            var misc = new ToolStripDropDownButton("Misc");
            var exportAllImages = new ToolStripMenuItem("Export all images");
            exportAllImages.DropDownItems.Add("As Bmp", null, (s, e) => ExportAllImages(ImageFormat.Bmp));
            exportAllImages.DropDownItems.Add("As Tiff", null, (s, e) => ExportAllImages(ImageFormat.Tiff));
            exportAllImages.DropDownItems.Add("As Jpg", null, (s, e) => ExportAllImages(ImageFormat.Jpeg));
            exportAllImages.DropDownItems.Add("As Png", null, (s, e) => ExportAllImages(ImageFormat.Png));
            misc.DropDownItems.Add(exportAllImages);
            var exportAllParts = new ToolStripMenuItem("Export all parts");
            foreach (PartFormat f in Enum.GetValues(typeof(PartFormat)))
            {
                PartFormat format = f;
                exportAllParts.DropDownItems.Add($"To {FormatTitle(format)}", null, (s, e) => ExportAllParts(format));
            }
            misc.DropDownItems.Add(exportAllParts);
            misc.DropDownItems.Add(new ToolStripSeparator());
            misc.DropDownItems.Add("Change background color...", null, (s, e) => ChangeBackgroundColor());
            _transparencyItem = new ToolStripMenuItem("Use transparency for PNG") { CheckOnClick = true, Checked = true };
            misc.DropDownItems.Add(_transparencyItem);

            var exportButton = new ToolStripDropDownButton("Export");
            foreach (PartFormat f in Enum.GetValues(typeof(PartFormat)))
            {
                PartFormat format = f;
                exportButton.DropDownItems.Add($"To {FormatTitle(format)}...", null, (s, e) => ExportParts(format));
            }
            var importButton = new ToolStripButton("Import...", null, (s, e) => ImportParts()) { ToolTipText = "Replace the parts of the multi with a text / UOA / CSV / WSC file" };

            toolStrip.Items.Add(new ToolStripLabel("Index:"));
            toolStrip.Items.Add(_searchBox);
            toolStrip.Items.Add(new ToolStripButton("Search", null, (s, e) => Search()));
            toolStrip.Items.Add(misc);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("New / copy...", null, (s, e) => AddMulti()));
            toolStrip.Items.Add(new ToolStripButton("Remove", null, (s, e) => DeleteMulti()));
            toolStrip.Items.Add(importButton);
            toolStrip.Items.Add(exportButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            // ---------------------------------------------------------------- multi list
            _tree = new TreeView { Dock = DockStyle.Fill, HideSelection = false, ShowLines = false, ShowRootLines = false, ShowNodeToolTips = true, FullRowSelect = true };
            _tree.AfterSelect += (s, e) => ShowSelected();

            // ---------------------------------------------------------------- graphic
            _heightBar = new TrackBar { Dock = DockStyle.Fill, TickStyle = TickStyle.BottomRight, Minimum = 0, Maximum = 0, AutoSize = false, Height = 32 };
            _heightBar.ValueChanged += (s, e) => { if (!_settingHeight) { UpdateHeightLabel(); Redraw(); } };
            _heightLabel = new Label { AutoSize = true, Margin = new Padding(3, 8, 6, 0), Text = "Max z: 0" };
            _minZBox = new NumericUpDown { Minimum = -128, Maximum = 512, Width = 55, Margin = new Padding(0, 5, 6, 0) };
            _minZBox.ValueChanged += (s, e) => { if (!_settingHeight) { Redraw(); } };
            _floorBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120, Margin = new Padding(0, 5, 6, 0) };
            _floorBox.SelectedIndexChanged += (s, e) => SelectFloor();
            _artBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140, Margin = new Padding(0, 5, 6, 0) };
            _artBox.Items.AddRange(new object[] { "Enhanced (EC art)", "Legacy (classic art)" });
            _artBox.SelectedIndex = 0;
            _artBox.SelectedIndexChanged += (s, e) => Redraw();
            _hiddenBox = new CheckBox { Text = "Parts with flags ≠ 0", AutoSize = true, Checked = true, Margin = new Padding(0, 7, 6, 0) };
            _hiddenBox.CheckedChanged += (s, e) => Redraw();
            _gridBox = new CheckBox { Text = "Grid", AutoSize = true, Margin = new Padding(0, 7, 6, 0) };
            _gridBox.CheckedChanged += (s, e) => Redraw();

            var tip = new ToolTip();
            tip.SetToolTip(_heightBar, "Highest z that is drawn (like the height slider of the Multis tab)");
            tip.SetToolTip(_minZBox, "Lowest z that is drawn");
            tip.SetToolTip(_floorBox, "Show one floor: from its z up to the next floor");
            tip.SetToolTip(_hiddenBox, "Parts with flags other than 0 are not shown in the game (the UOP packer writes them with classic flags 0). " +
                                       "The Multis tab draws them too.");

            var heightRow = new TableLayoutPanel { Dock = DockStyle.Top, Height = 34, ColumnCount = 2 };
            heightRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            heightRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            heightRow.Controls.Add(_heightLabel, 0, 0);
            heightRow.Controls.Add(_heightBar, 1, 0);

            var optionsRow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 30, WrapContents = false };
            optionsRow.Controls.Add(new Label { Text = "Min z:", AutoSize = true, Margin = new Padding(3, 8, 2, 0) });
            optionsRow.Controls.Add(_minZBox);
            optionsRow.Controls.Add(new Label { Text = "Floor:", AutoSize = true, Margin = new Padding(3, 8, 2, 0) });
            optionsRow.Controls.Add(_floorBox);
            optionsRow.Controls.Add(new Label { Text = "Art:", AutoSize = true, Margin = new Padding(3, 8, 2, 0) });
            optionsRow.Controls.Add(_artBox);
            optionsRow.Controls.Add(_hiddenBox);
            optionsRow.Controls.Add(_gridBox);
            optionsRow.Controls.Add(new Label { Text = "Ctrl + wheel = zoom, click = select part", AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(6, 8, 0, 0) });

            _preview = new EcPreviewPanel { Dock = DockStyle.Fill, BackColor = Color.White };
            _preview.Overlay = DrawOverlay;
            _preview.MouseMove += (s, e) => HoverAt(e.Location);
            _preview.MouseLeave += (s, e) => { _hoverPart = -1; _hoverLabel.Text = string.Empty; _preview.Invalidate(); };
            _preview.MouseClick += (s, e) => ClickAt(e.Location, e.Button);
            _preview.ContextMenuStrip = CreatePreviewMenu();

            var graphicPanel = new Panel { Dock = DockStyle.Fill };
            graphicPanel.Controls.Add(_preview);
            graphicPanel.Controls.Add(optionsRow);
            graphicPanel.Controls.Add(heightRow);

            // ---------------------------------------------------------------- component list
            _parts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersWidth = 24
            };
            _parts.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Show", HeaderText = "Show", FillWeight = 30, ToolTipText = "Only for the view, not saved" });
            foreach (string c in new[] { "Item", "Name", "X", "Y", "Z", "Flags", "Clilocs" })
            {
                _parts.Columns.Add(c, c);
            }
            _parts.Columns[ColName].ReadOnly = true;
            _parts.Columns[ColName].FillWeight = 160;
            _parts.Columns[ColName].DefaultCellStyle.ForeColor = SystemColors.GrayText;
            _parts.CellEndEdit += (s, e) => { if (e.ColumnIndex != ColShow) { PartsChanged(); } };
            _parts.UserDeletedRow += (s, e) => PartsChanged();
            _parts.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (_parts.CurrentCell?.ColumnIndex == ColShow)
                {
                    _parts.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            _parts.CellValueChanged += (s, e) =>
            {
                if (!_filling && e.ColumnIndex == ColShow && e.RowIndex >= 0 && e.RowIndex < (_current?.Parts.Count ?? 0))
                {
                    bool show = _parts.Rows[e.RowIndex].Cells[ColShow].Value as bool? ?? true;
                    if (show)
                    {
                        _hiddenParts.Remove(e.RowIndex);
                    }
                    else
                    {
                        _hiddenParts.Add(e.RowIndex);
                    }
                    Redraw();
                }
            };
            _parts.SelectionChanged += (s, e) => PartSelectionChanged();
            _parts.CellMouseEnter += (s, e) => { if (e.RowIndex >= 0) { ShowPartImage(e.RowIndex); } };
            _parts.MouseLeave += (s, e) => ShowPartImage(_parts.CurrentRow?.Index ?? -1);

            var partsMenu = new ContextMenuStrip();
            partsMenu.Items.Add("Show all", null, (s, e) => SetAllVisible(true));
            partsMenu.Items.Add("Hide selected", null, (s, e) => SetSelectedVisible(false));
            partsMenu.Items.Add("Show only selected", null, (s, e) => { SetAllVisible(false); SetSelectedVisible(true); });
            partsMenu.Items.Add(new ToolStripSeparator());
            partsMenu.Items.Add("Select all with this item", null, (s, e) => SelectSameItem());
            partsMenu.Items.Add("Select all at this z", null, (s, e) => SelectSameZ());
            _parts.ContextMenuStrip = partsMenu;

            _partImage = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.CenterImage, BackColor = Color.White };
            _partInfo = new Label { Dock = DockStyle.Top, Height = 48, Padding = new Padding(4), Font = EcDialogs.MonoFont(8.5f) };
            var partPanel = new Panel { Dock = DockStyle.Right, Width = 200, BorderStyle = BorderStyle.FixedSingle };
            partPanel.Controls.Add(_partImage);
            partPanel.Controls.Add(_partInfo);

            var componentPanel = new Panel { Dock = DockStyle.Fill };
            componentPanel.Controls.Add(_parts);
            componentPanel.Controls.Add(partPanel);

            var rightSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            rightSplit.Panel1.Controls.Add(graphicPanel);
            rightSplit.Panel2.Controls.Add(componentPanel);
            rightSplit.HandleCreated += (s, e) => { try { rightSplit.SplitterDistance = Math.Max(250, rightSplit.Height * 2 / 3); } catch (InvalidOperationException) { } };

            var split = new SplitContainer { Dock = DockStyle.Fill };
            split.Panel1.Controls.Add(_tree);
            split.Panel2.Controls.Add(rightSplit);
            split.HandleCreated += (s, e) => { try { split.SplitterDistance = 300; } catch (InvalidOperationException) { } };

            var status = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Choose the Enhanced Client folder on top.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _hoverLabel = new ToolStripStatusLabel(string.Empty);
            status.Items.Add(_statusLabel);
            status.Items.Add(_hoverLabel);

            Controls.Add(split);
            Controls.Add(toolStrip);
            Controls.Add(status);
        }

        private ContextMenuStrip CreatePreviewMenu()
        {
            var menu = new ContextMenuStrip();
            var extract = new ToolStripMenuItem("Extract image..");
            extract.DropDownItems.Add("As Bmp", null, (s, e) => ExtractImage(ImageFormat.Bmp));
            extract.DropDownItems.Add("As Tiff", null, (s, e) => ExtractImage(ImageFormat.Tiff));
            extract.DropDownItems.Add("As Jpg", null, (s, e) => ExtractImage(ImageFormat.Jpeg));
            extract.DropDownItems.Add("As Png", null, (s, e) => ExtractImage(ImageFormat.Png));
            menu.Items.Add(extract);
            menu.Items.Add("Copy", null, (s, e) => CopyImage());
            var background = new ToolStripMenuItem("Background image");
            background.DropDownItems.Add("Background off", null, (s, e) => _preview.BackgroundTile = null);
            background.DropDownItems.Add("Grass", null, (s, e) => _preview.BackgroundTile = UoFiddler.Controls.Properties.Resources.Grass);
            background.DropDownItems.Add("Water", null, (s, e) => _preview.BackgroundTile = UoFiddler.Controls.Properties.Resources.Water);
            menu.Items.Add(background);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Show all parts", null, (s, e) => SetAllVisible(true));
            return menu;
        }

        // ==================================================================== loading

        public void SetClient(EcClient client)
        {
            _client = client;
            _loaded = false;
            _package = null;
            _multis.Clear();
            _tree.Nodes.Clear();
            _parts.Rows.Clear();
            _preview.Image = null;
            _render = null;
            _current = null;
            _currentEntry = null;
            ClearSprites();
            _saveButton.Enabled = false;
            if (Visible && client != null)
            {
                OnActivated();
            }
        }

        public void OnActivated()
        {
            if (_loaded || _client == null)
            {
                return;
            }
            _loaded = true;
            _package = _client.GetPackage(FileName);
            if (_package == null)
            {
                _statusLabel.Text = $"{FileName} was not found.";
                return;
            }
            _package.Changed += (s, e) => _saveButton.Enabled = _package.IsDirty;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                _client.GetTileArt();
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"tileart.uop: {ex.Message}";
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
            LoadMultiNames();
            FillList();
        }

        /// <summary>Names from Multilist.xml next to UoFiddler.exe (the same file the Multis tab uses).</summary>
        private void LoadMultiNames()
        {
            _multiNames.Clear();
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Multilist.xml");
            if (!File.Exists(file))
            {
                return;
            }
            try
            {
                var doc = new XmlDocument();
                doc.Load(file);
                var tips = new Dictionary<int, string>();
                foreach (XmlElement e in doc.SelectNodes("/Multis/ToolTip").OfType<XmlElement>())
                {
                    if (int.TryParse(e.GetAttribute("id"), out int id))
                    {
                        tips[id] = e.GetAttribute("text");
                    }
                }
                foreach (XmlElement e in doc.SelectNodes("/Multis/Multi").OfType<XmlElement>())
                {
                    if (int.TryParse(e.GetAttribute("id"), out int id))
                    {
                        tips.TryGetValue(id, out string tipText);
                        _multiNames[id] = (e.GetAttribute("name"), tipText);
                    }
                }
            }
            catch (Exception)
            {
                // names are optional
            }
        }

        private void FillList()
        {
            _multis.Clear();
            foreach (EcUopEntry e in _package.Entries)
            {
                if (e.Name != null && e.Name.StartsWith("build/multicollection/", StringComparison.OrdinalIgnoreCase) && EcClient.TryGetIdFromName(e.Name, out int id))
                {
                    _multis.Add((id, e));
                }
            }
            _multis.Sort((a, b) => a.Id.CompareTo(b.Id));

            int selected = _current?.Id ?? -1;
            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            TreeNode select = null;
            foreach (var (id, entry) in _multis)
            {
                _multiNames.TryGetValue(id, out var name);
                var node = new TreeNode($"{id,5} (0x{id:X}) {name.Name}".TrimEnd())
                {
                    Tag = entry,
                    Name = id.ToString(CultureInfo.InvariantCulture),
                    ToolTipText = string.IsNullOrEmpty(name.Tip) ? name.Name : $"{name.Name}\r\n{name.Tip}"
                };
                _tree.Nodes.Add(node);
                if (id == selected)
                {
                    select = node;
                }
            }
            _tree.EndUpdate();
            if (select != null)
            {
                _tree.SelectedNode = select;
            }
            else if (_tree.Nodes.Count > 0 && _tree.SelectedNode == null)
            {
                _tree.SelectedNode = _tree.Nodes[0];
            }
            _statusLabel.Text = $"{_multis.Count} multis" + (_multiNames.Count == 0 ? " (Multilist.xml not found – no names)" : string.Empty);
        }

        private void Search()
        {
            string text = _searchBox.Text.Trim();
            if (text.Length == 0 || _tree.Nodes.Count == 0)
            {
                return;
            }
            if (EcDialogs.TryParseNumber(text, out long id))
            {
                TreeNode[] found = _tree.Nodes.Find(id.ToString(CultureInfo.InvariantCulture), false);
                if (found.Length > 0)
                {
                    _tree.SelectedNode = found[0];
                    found[0].EnsureVisible();
                    return;
                }
            }
            // name search, starting after the selected node
            int start = _tree.SelectedNode?.Index ?? -1;
            for (int i = 1; i <= _tree.Nodes.Count; i++)
            {
                TreeNode node = _tree.Nodes[(start + i) % _tree.Nodes.Count];
                if (node.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _tree.SelectedNode = node;
                    node.EnsureVisible();
                    return;
                }
            }
            _statusLabel.Text = $"\"{text}\" not found.";
        }

        private void ShowSelected()
        {
            if (_tree.SelectedNode?.Tag is not EcUopEntry entry)
            {
                return;
            }
            try
            {
                _currentEntry = entry;
                _current = EcMulti.Read(_package.ReadData(entry));
                _hiddenParts.Clear();
                _highlighted.Clear();
                _hoverPart = -1;
                SetupHeights();
                FillParts();
                Redraw();
            }
            catch (Exception ex)
            {
                _statusLabel.Text = ex.Message;
            }
        }

        private void SetupHeights()
        {
            _settingHeight = true;
            var b = _current.Bounds();
            int minZ = Math.Min(0, b.MinZ), maxZ = Math.Max(0, b.MaxZ);
            _heightBar.Minimum = minZ;
            _heightBar.Maximum = maxZ;
            _heightBar.Value = maxZ;
            _heightBar.TickFrequency = Math.Max(1, (maxZ - minZ) / 40);
            _minZBox.Value = Math.Max(_minZBox.Minimum, minZ);

            // floors: heights of background / surface parts that cover a good part of the multi
            _floorBox.Items.Clear();
            _floorBox.Items.Add("All");
            var floorZ = _current.Parts
                .Where(p => SpriteFor(p.ItemId, false).Background)
                .GroupBy(p => (int)p.Z)
                .Where(g => g.Count() >= Math.Max(2, _current.Parts.Count / 100))
                .Select(g => g.Key)
                .OrderBy(z => z)
                .ToList();
            foreach (int z in floorZ)
            {
                _floorBox.Items.Add(new FloorItem(z));
            }
            _floorBox.SelectedIndex = 0;
            _settingHeight = false;
            UpdateHeightLabel();
        }

        private sealed class FloorItem
        {
            public int Z { get; }
            public FloorItem(int z) { Z = z; }
            public override string ToString() => $"z {Z}";
        }

        private void SelectFloor()
        {
            if (_settingHeight || _current == null)
            {
                return;
            }
            _settingHeight = true;
            if (_floorBox.SelectedItem is FloorItem floor)
            {
                FloorItem next = _floorBox.Items.OfType<FloorItem>().FirstOrDefault(f => f.Z > floor.Z);
                _minZBox.Value = Math.Max(_minZBox.Minimum, floor.Z);
                _heightBar.Value = Math.Min(_heightBar.Maximum, next != null ? next.Z - 1 : _heightBar.Maximum);
            }
            else
            {
                _minZBox.Value = Math.Max(_minZBox.Minimum, _heightBar.Minimum);
                _heightBar.Value = _heightBar.Maximum;
            }
            _settingHeight = false;
            UpdateHeightLabel();
            Redraw();
        }

        private void UpdateHeightLabel()
        {
            _heightLabel.Text = $"Max z: {_heightBar.Value,3}";
        }

        private void FillParts()
        {
            _filling = true;
            _parts.Rows.Clear();
            for (int i = 0; i < _current.Parts.Count; i++)
            {
                EcMultiPart p = _current.Parts[i];
                _parts.Rows.Add(!_hiddenParts.Contains(i), $"0x{p.ItemId:X4}", ItemName(p.ItemId), p.X, p.Y, p.Z, p.Flags, string.Join(" ", p.Clilocs));
            }
            _filling = false;
        }

        private string ItemName(uint id)
        {
            try
            {
                TileArtEntry tile = _client?.TileArtIfLoaded?.Get(id)?.Entry;
                return tile != null && _client.HasStringDictionary ? _client.StringDictionary.Get(tile.NameIndex) ?? string.Empty : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void PartsChanged()
        {
            if (_filling || _current == null)
            {
                return;
            }
            try
            {
                var parts = new List<EcMultiPart>();
                foreach (DataGridViewRow row in _parts.Rows)
                {
                    if (row.IsNewRow || row.Cells[ColItem].Value == null)
                    {
                        continue;
                    }
                    var p = new EcMultiPart
                    {
                        ItemId = (ushort)Num(row.Cells[ColItem].Value),
                        X = (short)Num(row.Cells[ColX].Value),
                        Y = (short)Num(row.Cells[ColY].Value),
                        Z = (short)Num(row.Cells[ColZ].Value),
                        Flags = (ushort)Num(row.Cells[ColFlags].Value)
                    };
                    foreach (string c in (row.Cells[ColClilocs].Value?.ToString() ?? string.Empty).Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        p.Clilocs.Add((uint)Num(c));
                    }
                    parts.Add(p);
                }
                _current.Parts.Clear();
                _current.Parts.AddRange(parts);
                _package.SetData(_currentEntry, _current.Write());
                _saveButton.Enabled = true;

                // refresh the name and show check of new rows
                _filling = true;
                for (int i = 0; i < parts.Count && i < _parts.Rows.Count; i++)
                {
                    _parts.Rows[i].Cells[ColName].Value = ItemName(parts[i].ItemId);
                    if (_parts.Rows[i].Cells[ColShow].Value == null)
                    {
                        _parts.Rows[i].Cells[ColShow].Value = true;
                    }
                }
                _filling = false;
                Redraw();
            }
            catch (FormatException ex)
            {
                EcDialogs.ShowError(this, "Invalid number in the component list.", ex);
            }
        }

        private static long Num(object value)
        {
            string text = value?.ToString() ?? "0";
            if (!EcDialogs.TryParseNumber(text, out long v))
            {
                throw new FormatException(text);
            }
            return v;
        }

        // ==================================================================== images

        // EC images are 1.5 times the classic size: a tile is 66 pixels wide, one z step 6 pixels
        private const float EcScale = 1.5f;

        private bool LegacyMode => _artBox.SelectedIndex == 1;

        private Sprite SpriteFor(uint id, bool legacy)
        {
            if (_sprites.TryGetValue((id, legacy), out Sprite sprite))
            {
                return sprite;
            }

            sprite = new Sprite();
            _sprites[(id, legacy)] = sprite;
            if (_client == null)
            {
                return sprite;
            }

            TileArtEntry tile = _client.TileArtIfLoaded?.Get(id)?.Entry;
            if (tile != null)
            {
                sprite.Background = ((tile.Flags2 | (ulong)tile.Flags) & 0x1) != 0;
                sprite.Height = (int)(tile.Properties.FirstOrDefault(p => p.Id == TileArtPropertyId.Height)?.Value ?? 0);
                sprite.Name = ItemName(id);
            }

            try
            {
                if (!legacy && tile != null)
                {
                    // EC world art "build/worldart/<id>.dds": the item is the area (x start, y start)-(x end, y end) of the
                    // padded DDS; the offset places it on the tile. Floors etc. have no area (they are drawn from terrain
                    // like textures in the client) – for them the legacy art is used, scaled to EC size.
                    TileArtImageOffset o = tile.EcImageOffset;
                    if (o.XEnd > o.XStart && o.YEnd > o.YStart)
                    {
                        Bitmap full = Decode(_client.GetArtData(EcClient.ArtSources[0], (int)id));
                        Bitmap cut = Cut(full, o);
                        if (cut != null)
                        {
                            sprite.Image = cut;
                            sprite.OffsetX = o.XOffset;
                            sprite.OffsetY = o.YOffset - cut.Height;
                            sprite.Source = "World art (EC)";
                            return sprite;
                        }
                    }
                }

                // legacy (classic) art "build/tileartlegacy/<id>.dds": the classic image is the area of the classic offset
                Bitmap legacyFull = Decode(_client.GetArtData(EcClient.ArtSources[1], (int)id));
                Bitmap image = tile != null ? Cut(legacyFull, tile.CcImageOffset) : legacyFull;
                if (image == null && legacyFull == null)
                {
                    // last try: any image tileart points to
                    image = Decode(_client.GetItemImage(id, out _));
                }
                if (image == null)
                {
                    return sprite;
                }

                if (!legacy)
                {
                    var scaled = new Bitmap(Math.Max(1, (int)Math.Round(image.Width * EcScale)), Math.Max(1, (int)Math.Round(image.Height * EcScale)), PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(scaled))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                        g.DrawImage(image, 0, 0, scaled.Width, scaled.Height);
                    }
                    image.Dispose();
                    image = scaled;
                }

                // classic placement: bottom center of the image on the bottom corner of the tile
                int tileWidth = legacy ? 44 : 66;
                sprite.Image = image;
                sprite.OffsetX = (tileWidth / 2) - (image.Width / 2);
                sprite.OffsetY = -image.Height;
                sprite.Source = legacy ? "Legacy art" : "Legacy art (scaled, no EC image)";
            }
            catch (Exception)
            {
                // drawn as red frame
            }
            return sprite;
        }

        private static Bitmap Decode(byte[] data) => data == null ? null : EcImageHelper.TryDecode(data, out _);

        /// <summary>Cuts the used area out of a padded image (disposes the full image if a part is returned).</summary>
        private static Bitmap Cut(Bitmap full, TileArtImageOffset o)
        {
            if (full == null)
            {
                return null;
            }
            if (o.XEnd <= o.XStart || o.YEnd <= o.YStart)
            {
                return full;
            }
            Rectangle rect = Rectangle.Intersect(new Rectangle(o.XStart, o.YStart, o.XEnd - o.XStart, o.YEnd - o.YStart), new Rectangle(0, 0, full.Width, full.Height));
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return full;
            }
            if (rect.Size == full.Size)
            {
                return full;
            }
            Bitmap cut = full.Clone(rect, PixelFormat.Format32bppArgb);
            full.Dispose();
            return cut;
        }

        private void ClearSprites()
        {
            foreach (Sprite s in _sprites.Values)
            {
                s.Image?.Dispose();
            }
            _sprites.Clear();
        }

        // ==================================================================== drawing

        /// <summary>
        /// Same drawing as the Multis tab (MultiComponentList): cell by cell (x, then y), the parts of one cell sorted by
        /// z + threshold (+1 for height &gt; 0, +1 for not background), then by the threshold, then by their order.
        /// Bottom center of a classic image on the bottom corner of the tile, 4 pixels per z (EC: 1.5 times).
        /// </summary>
        private RenderResult Render(EcMulti multi, int minZ, int maxZ, bool legacy, bool showFlagged, ICollection<int> hiddenParts, bool grid)
        {
            var result = new RenderResult();
            if (multi == null || multi.Parts.Count == 0)
            {
                return result;
            }

            int half = legacy ? 22 : 33;
            int tile = half * 2;
            int zStep = legacy ? 4 : 6;

            var order = Enumerable.Range(0, multi.Parts.Count)
                .Where(i => hiddenParts == null || !hiddenParts.Contains(i))
                .Select(i => (Index: i, Part: multi.Parts[i], Sprite: SpriteFor(multi.Parts[i].ItemId, legacy)))
                .Where(t => t.Part.Z <= maxZ && t.Part.Z >= minZ && (showFlagged || t.Part.Flags == 0))
                .Select(t =>
                {
                    int threshold = (t.Sprite.Height > 0 ? 1 : 0) + (t.Sprite.Background ? 0 : 1);
                    return (t.Index, t.Part, t.Sprite, Threshold: threshold);
                })
                .OrderBy(t => t.Part.X).ThenBy(t => t.Part.Y)
                .ThenBy(t => t.Part.Z + t.Threshold).ThenBy(t => t.Threshold).ThenBy(t => t.Index)
                .ToList();

            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var (index, p, sprite, _) in order)
            {
                int left = ((p.X - p.Y) * half) - half;
                int bottom = ((p.X + p.Y) * half) - (p.Z * zStep);
                Rectangle rect = sprite.Image != null
                    ? new Rectangle(left + sprite.OffsetX, bottom + sprite.OffsetY, sprite.Image.Width, sprite.Image.Height)
                    : new Rectangle(left, bottom - tile, tile, tile);
                var diamond = new[]
                {
                    new Point(left + half, bottom - tile), new Point(left + tile, bottom - half),
                    new Point(left + half, bottom), new Point(left, bottom - half)
                };
                result.Placed.Add(new Placed { Index = index, Part = p, Sprite = sprite, Rect = rect, Tile = diamond });
                minX = Math.Min(minX, rect.Left);
                minY = Math.Min(minY, rect.Top);
                maxX = Math.Max(maxX, rect.Right);
                maxY = Math.Max(maxY, rect.Bottom);
            }
            if (result.Placed.Count == 0)
            {
                return result;
            }

            // grid area: all cells of the multi at z 0
            var b = multi.Bounds();
            if (grid)
            {
                minX = Math.Min(minX, ((b.MinX - b.MaxY) * half) - half);
                maxX = Math.Max(maxX, ((b.MaxX - b.MinY) * half) + half);
                minY = Math.Min(minY, ((b.MinX + b.MinY) * half) - tile);
                maxY = Math.Max(maxY, (b.MaxX + b.MaxY) * half);
            }

            const int margin = 10;
            int dx = margin - minX, dy = margin - minY;
            int width = Math.Min(12000, maxX - minX + (2 * margin)), height = Math.Min(12000, maxY - minY + (2 * margin));
            foreach (Placed placed in result.Placed)
            {
                placed.Rect.Offset(dx, dy);
                for (int i = 0; i < placed.Tile.Length; i++)
                {
                    placed.Tile[i].Offset(dx, dy);
                }
            }

            var canvas = new Bitmap(Math.Max(1, width), Math.Max(1, height), PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(canvas))
            {
                if (grid)
                {
                    using (var pen = new Pen(Color.FromArgb(90, Color.SteelBlue)))
                    {
                        for (int ty = b.MinY; ty <= b.MaxY; ty++)
                        {
                            for (int tx = b.MinX; tx <= b.MaxX; tx++)
                            {
                                int left = ((tx - ty) * half) - half + dx;
                                int bottom = ((tx + ty) * half) + dy;
                                g.DrawPolygon(pen, new[] { new Point(left + half, bottom - tile), new Point(left + tile, bottom - half), new Point(left + half, bottom), new Point(left, bottom - half) });
                            }
                        }
                    }
                }

                foreach (Placed placed in result.Placed)
                {
                    if (placed.Sprite.Image == null)
                    {
                        g.DrawPolygon(Pens.Red, placed.Tile);
                    }
                    else
                    {
                        g.DrawImageUnscaled(placed.Sprite.Image, placed.Rect.X, placed.Rect.Y);
                    }
                }
            }
            result.Image = canvas;
            return result;
        }

        private void Redraw()
        {
            if (_current == null)
            {
                _preview.Image = null;
                _render = null;
                return;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                _render = Render(_current, (int)_minZBox.Value, _heightBar.Value, LegacyMode, _hiddenBox.Checked, _hiddenParts, _gridBox.Checked);
                _preview.Image = _render.Image;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            var b = _current.Bounds();
            int surface = _current.Parts.Select(p => (p.X, p.Y)).Distinct().Count();
            int missing = _render.Placed.Count(p => p.Sprite.Image == null);
            _statusLabel.Text = $"Multi {_current.Id}: Size: {b.MaxX - b.MinX + 1},{b.MaxY - b.MinY + 1} MaxHeight: {b.MaxZ} " +
                                $"MultiRegion: {b.MinX},{b.MinY},{b.MaxX},{b.MaxY} Surface: {surface}  –  {_current.Parts.Count} parts, {_render.Placed.Count} drawn" +
                                (missing > 0 ? $", {missing} without image (red)" : string.Empty);
        }

        /// <summary>Highlight of the selected and hovered parts (drawn on top, the image is not rendered again).</summary>
        private void DrawOverlay(Graphics g, Rectangle area, float zoom)
        {
            if (_render == null || (_highlighted.Count == 0 && _hoverPart < 0))
            {
                return;
            }
            var state = g.Save();
            g.TranslateTransform(area.X, area.Y);
            g.ScaleTransform(zoom, zoom);
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            using (var fill = new SolidBrush(Color.FromArgb(70, Color.Gold)))
            using (var pen = new Pen(Color.OrangeRed, 2f / zoom))
            using (var hoverFill = new SolidBrush(Color.FromArgb(60, Color.DeepSkyBlue)))
            using (var hoverPen = new Pen(Color.DeepSkyBlue, 1.5f / zoom))
            {
                foreach (Placed placed in _render.Placed)
                {
                    if (_highlighted.Contains(placed.Index))
                    {
                        g.FillPolygon(fill, placed.Tile);
                        g.DrawRectangle(pen, placed.Rect);
                    }
                    if (placed.Index == _hoverPart)
                    {
                        g.FillPolygon(hoverFill, placed.Tile);
                        g.DrawRectangle(hoverPen, placed.Rect);
                    }
                }
            }
            g.Restore(state);
        }

        /// <summary>Topmost drawn part under an image pixel (transparent pixels don't count).</summary>
        private Placed PartAt(Point? imagePoint)
        {
            if (_render == null || imagePoint == null)
            {
                return null;
            }
            Point p = imagePoint.Value;
            for (int i = _render.Placed.Count - 1; i >= 0; i--)
            {
                Placed placed = _render.Placed[i];
                if (!placed.Rect.Contains(p))
                {
                    continue;
                }
                if (placed.Sprite.Image == null || placed.Sprite.Image.GetPixel(p.X - placed.Rect.X, p.Y - placed.Rect.Y).A > 16)
                {
                    return placed;
                }
            }
            return null;
        }

        private void HoverAt(Point location)
        {
            Placed placed = PartAt(_preview.ClientToImage(location));
            int index = placed?.Index ?? -1;
            if (index == _hoverPart)
            {
                return;
            }
            _hoverPart = index;
            _hoverLabel.Text = placed == null ? string.Empty
                : $"0x{placed.Part.ItemId:X4} {placed.Sprite.Name}  x {placed.Part.X} y {placed.Part.Y} z {placed.Part.Z} flags {placed.Part.Flags}";
            _preview.Invalidate();
        }

        private void ClickAt(Point location, MouseButtons button)
        {
            if (button != MouseButtons.Left)
            {
                return;
            }
            Placed placed = PartAt(_preview.ClientToImage(location));
            if (placed == null || placed.Index >= _parts.Rows.Count)
            {
                return;
            }
            bool add = (ModifierKeys & Keys.Control) != 0;
            if (!add)
            {
                _parts.ClearSelection();
            }
            DataGridViewRow row = _parts.Rows[placed.Index];
            row.Selected = !add || !row.Selected;
            _parts.CurrentCell = row.Cells[ColItem];
            _parts.FirstDisplayedScrollingRowIndex = Math.Max(0, placed.Index - 3);
        }

        private void PartSelectionChanged()
        {
            _highlighted.Clear();
            foreach (DataGridViewRow row in _parts.SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    _highlighted.Add(row.Index);
                }
            }
            _preview.Invalidate();
            ShowPartImage(_parts.CurrentRow?.Index ?? -1);
        }

        private void ShowPartImage(int index)
        {
            if (_current == null || index < 0 || index >= _current.Parts.Count)
            {
                _partImage.Image = null;
                _partInfo.Text = string.Empty;
                return;
            }
            EcMultiPart p = _current.Parts[index];
            Sprite sprite = SpriteFor(p.ItemId, LegacyMode);
            _partImage.Image = sprite.Image;
            _partInfo.Text = $"0x{p.ItemId:X4} {sprite.Name}\r\nx {p.X} y {p.Y} z {p.Z} flags {p.Flags}\r\n{sprite.Source ?? "no image"}";
        }

        private void SetAllVisible(bool visible)
        {
            if (_current == null)
            {
                return;
            }
            _hiddenParts.Clear();
            if (!visible)
            {
                for (int i = 0; i < _current.Parts.Count; i++)
                {
                    _hiddenParts.Add(i);
                }
            }
            UpdateShowColumn();
        }

        private void SetSelectedVisible(bool visible)
        {
            foreach (DataGridViewRow row in _parts.SelectedRows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }
                if (visible)
                {
                    _hiddenParts.Remove(row.Index);
                }
                else
                {
                    _hiddenParts.Add(row.Index);
                }
            }
            UpdateShowColumn();
        }

        private void UpdateShowColumn()
        {
            _filling = true;
            for (int i = 0; i < _parts.Rows.Count; i++)
            {
                if (!_parts.Rows[i].IsNewRow)
                {
                    _parts.Rows[i].Cells[ColShow].Value = !_hiddenParts.Contains(i);
                }
            }
            _filling = false;
            Redraw();
        }

        private void SelectSameItem()
        {
            if (_current == null || _parts.CurrentRow == null || _parts.CurrentRow.Index >= _current.Parts.Count)
            {
                return;
            }
            ushort item = _current.Parts[_parts.CurrentRow.Index].ItemId;
            SelectWhere(p => p.ItemId == item);
        }

        private void SelectSameZ()
        {
            if (_current == null || _parts.CurrentRow == null || _parts.CurrentRow.Index >= _current.Parts.Count)
            {
                return;
            }
            short z = _current.Parts[_parts.CurrentRow.Index].Z;
            SelectWhere(p => p.Z == z);
        }

        private void SelectWhere(Func<EcMultiPart, bool> predicate)
        {
            _parts.ClearSelection();
            for (int i = 0; i < _current.Parts.Count && i < _parts.Rows.Count; i++)
            {
                if (predicate(_current.Parts[i]))
                {
                    _parts.Rows[i].Selected = true;
                }
            }
        }

        // ==================================================================== image export

        private string OutputFolder()
        {
            try
            {
                string path = UoFiddler.Controls.Classes.Options.OutputPath;
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    return path;
                }
            }
            catch (Exception)
            {
                // fall back to the documents folder
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private static string Extension(ImageFormat format) =>
            format.Equals(ImageFormat.Bmp) ? "bmp" : format.Equals(ImageFormat.Tiff) ? "tiff" : format.Equals(ImageFormat.Jpeg) ? "jpg" : "png";

        private void SaveImage(Bitmap image, string fileName, ImageFormat format)
        {
            bool transparent = format.Equals(ImageFormat.Png) && _transparencyItem.Checked;
            using (var output = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(output))
                {
                    g.Clear(transparent ? Color.Transparent : _backgroundColor);
                    g.DrawImageUnscaled(image, 0, 0);
                }
                output.Save(fileName, format);
            }
        }

        private void ExtractImage(ImageFormat format)
        {
            if (_render?.Image == null || _current == null)
            {
                return;
            }
            string suffix = _heightBar.Value < _heightBar.Maximum || _minZBox.Value > _heightBar.Minimum ? $"_Z{_minZBox.Value:000}-{_heightBar.Value:000}" : string.Empty;
            using (var dialog = new SaveFileDialog
            {
                InitialDirectory = OutputFolder(),
                FileName = $"Multi 0x{_current.Id:X4}{suffix}.{Extension(format)}",
                Filter = $"{Extension(format).ToUpperInvariant()}|*.{Extension(format)}"
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    SaveImage(_render.Image, dialog.FileName, format);
                    _statusLabel.Text = $"Multi saved to {dialog.FileName}";
                }
            }
        }

        private void CopyImage()
        {
            if (_render?.Image == null)
            {
                return;
            }
            using (var copy = new Bitmap(_render.Image.Width, _render.Image.Height))
            {
                using (Graphics g = Graphics.FromImage(copy))
                {
                    g.Clear(_backgroundColor);
                    g.DrawImageUnscaled(_render.Image, 0, 0);
                }
                Clipboard.SetImage(copy);
            }
            _statusLabel.Text = "Image copied to the clipboard.";
        }

        private void ChangeBackgroundColor()
        {
            using (var dialog = new ColorDialog { Color = _backgroundColor })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _backgroundColor = dialog.Color;
                    _preview.BackColor = dialog.Color;
                    _preview.Invalidate();
                }
            }
        }

        private void ExportAllImages(ImageFormat format)
        {
            if (_package == null)
            {
                return;
            }
            using (var dialog = new FolderBrowserDialog { SelectedPath = OutputFolder(), Description = "Folder for the multi images" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                int done = 0;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    foreach (var (id, entry) in _multis)
                    {
                        try
                        {
                            EcMulti multi = EcMulti.Read(_package.ReadData(entry));
                            RenderResult r = Render(multi, short.MinValue, short.MaxValue, LegacyMode, _hiddenBox.Checked, null, false);
                            if (r.Image != null)
                            {
                                SaveImage(r.Image, Path.Combine(dialog.SelectedPath, $"Multi 0x{id:X4}.{Extension(format)}"), format);
                                r.Image.Dispose();
                                done++;
                            }
                        }
                        catch (Exception)
                        {
                            // skip broken entries
                        }
                        if (done % 10 == 0)
                        {
                            _statusLabel.Text = $"Exporting images... {done}";
                            Application.DoEvents();
                        }
                    }
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
                _statusLabel.Text = $"{done} images saved to {dialog.SelectedPath}";
            }
        }

        // ==================================================================== part export / import

        private enum PartFormat
        {
            Text,
            Uoa,
            Wsc,
            Csv,
            Uox3,
            Xml
        }

        private static string FormatTitle(PartFormat format) => format switch
        {
            PartFormat.Text => "Text file",
            PartFormat.Uoa => "UOA file",
            PartFormat.Wsc => "WSC file",
            PartFormat.Csv => "CSV file",
            PartFormat.Uox3 => "UOX3 file",
            _ => "XML file (CentrED+)"
        };

        private static string FormatExtension(PartFormat format) => format switch
        {
            PartFormat.Text => "txt",
            PartFormat.Uoa => "uoa.txt",
            PartFormat.Wsc => "wsc",
            PartFormat.Csv => "csv",
            PartFormat.Uox3 => "uox3",
            _ => "xml"
        };

        /// <summary>Classic flag of a part (the UOP packer writes 1 for EC flags 0, else 0).</summary>
        private static int ClassicFlag(EcMultiPart p) => p.Flags == 0 ? 1 : 0;

        /// <summary>
        /// Same formats as the Multis tab. The text file keeps the EC flags (it is read back by Import);
        /// the formats of other tools get the classic flag.
        /// </summary>
        private static string FormatParts(EcMulti multi, PartFormat format)
        {
            var sb = new StringBuilder();
            IList<EcMultiPart> parts = multi.Parts;
            switch (format)
            {
                case PartFormat.Text:
                    foreach (EcMultiPart p in parts)
                    {
                        sb.AppendLine($"0x{p.ItemId:X} {p.X} {p.Y} {p.Z} {p.Flags}");
                    }
                    break;
                case PartFormat.Uoa:
                    sb.AppendLine("6 version");
                    sb.AppendLine("1 template id");
                    sb.AppendLine("-1 item version");
                    sb.AppendLine($"{parts.Count} num components");
                    foreach (EcMultiPart p in parts)
                    {
                        sb.AppendLine($"{p.ItemId} {p.X} {p.Y} {p.Z} {ClassicFlag(p)}");
                    }
                    break;
                case PartFormat.Wsc:
                    for (int i = 0; i < parts.Count; i++)
                    {
                        sb.AppendLine($"SECTION WORLDITEM {i}");
                        sb.AppendLine("{");
                        sb.AppendLine($"\tID\t{parts[i].ItemId}");
                        sb.AppendLine($"\tX\t{parts[i].X}");
                        sb.AppendLine($"\tY\t{parts[i].Y}");
                        sb.AppendLine($"\tZ\t{parts[i].Z}");
                        sb.AppendLine("\tColor\t0");
                        sb.AppendLine("}");
                    }
                    break;
                case PartFormat.Csv:
                    sb.AppendLine("TileID,OffsetX,OffsetY,OffsetZ,Flag,Cliloc");
                    foreach (EcMultiPart p in parts)
                    {
                        sb.AppendLine($"0x{p.ItemId:x4},{p.X},{p.Y},{p.Z},0x{ClassicFlag(p):x},{string.Join(" ", p.Clilocs)}");
                    }
                    break;
                case PartFormat.Uox3:
                    for (int i = 0; i < parts.Count; i++)
                    {
                        sb.AppendLine($"[HOUSE ITEM {i}]");
                        sb.AppendLine("{");
                        sb.AppendLine($"ITEM=0x{parts[i].ItemId:X4}");
                        sb.AppendLine($"X={parts[i].X}");
                        sb.AppendLine($"Y={parts[i].Y}");
                        sb.AppendLine($"Z={parts[i].Z}");
                        sb.AppendLine("}");
                        sb.AppendLine(string.Empty);
                    }
                    break;
                default:
                    sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sb.AppendLine($"<Entry ID=\"{multi.Id}\">");
                    foreach (EcMultiPart p in parts)
                    {
                        sb.AppendLine($"  <Item X=\"{p.X}\" Y=\"{p.Y}\" Z=\"{p.Z}\" ID=\"0x{p.ItemId:X4}\" />");
                    }
                    sb.AppendLine("</Entry>");
                    break;
            }
            return sb.ToString();
        }

        private void ExportParts(PartFormat format)
        {
            if (_current == null)
            {
                return;
            }
            string ext = FormatExtension(format);
            using (var dialog = new SaveFileDialog
            {
                InitialDirectory = OutputFolder(),
                FileName = $"Multi 0x{_current.Id:X4}.{ext}",
                Filter = $"{FormatTitle(format)}|*.{ext.Split('.').Last()}|All files (*.*)|*.*"
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, FormatParts(_current, format), new UTF8Encoding(false));
                    _statusLabel.Text = $"Multi saved to {dialog.FileName}";
                }
            }
        }

        private void ExportAllParts(PartFormat format)
        {
            if (_package == null)
            {
                return;
            }
            using (var dialog = new FolderBrowserDialog { SelectedPath = OutputFolder(), Description = $"Folder for the {FormatTitle(format)}s" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                int done = 0;
                foreach (var (id, entry) in _multis)
                {
                    try
                    {
                        EcMulti multi = EcMulti.Read(_package.ReadData(entry));
                        File.WriteAllText(Path.Combine(dialog.SelectedPath, $"Multi 0x{id:X4}.{FormatExtension(format)}"), FormatParts(multi, format), new UTF8Encoding(false));
                        done++;
                    }
                    catch (Exception)
                    {
                        // skip broken entries
                    }
                }
                _statusLabel.Text = $"{done} multis saved to {dialog.SelectedPath}";
            }
        }

        /// <summary>
        /// Reads text ("item x y z flags", the EC flags), UOA (classic flags), CSV (classic flags) or WSC files.
        /// </summary>
        private void ImportParts()
        {
            if (_current == null)
            {
                return;
            }
            using (var dialog = new OpenFileDialog { Filter = "Multi files|*.txt;*.uoa;*.csv;*.wsc|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    string[] lines = File.ReadAllLines(dialog.FileName);
                    var parts = new List<EcMultiPart>();
                    bool uoa = lines.Length > 3 && lines[0].Trim().EndsWith("version", StringComparison.OrdinalIgnoreCase) && lines[3].Contains("num components");
                    bool csv = lines.Length > 0 && lines[0].StartsWith("TileID", StringComparison.OrdinalIgnoreCase);
                    bool wsc = lines.Any(l => l.TrimStart().StartsWith("SECTION WORLDITEM", StringComparison.OrdinalIgnoreCase));

                    if (wsc)
                    {
                        EcMultiPart part = null;
                        foreach (string raw in lines)
                        {
                            string line = raw.Trim();
                            if (line.StartsWith("SECTION", StringComparison.OrdinalIgnoreCase))
                            {
                                part = new EcMultiPart();
                                parts.Add(part);
                                continue;
                            }
                            string[] v = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (part == null || v.Length < 2)
                            {
                                continue;
                            }
                            switch (v[0].ToUpperInvariant())
                            {
                                case "ID": part.ItemId = (ushort)Num(v[1]); break;
                                case "X": part.X = (short)Num(v[1]); break;
                                case "Y": part.Y = (short)Num(v[1]); break;
                                case "Z": part.Z = (short)Num(v[1]); break;
                            }
                        }
                    }
                    else
                    {
                        foreach (string line in lines.Skip(uoa ? 4 : csv ? 1 : 0))
                        {
                            string[] v = line.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            if (v.Length < 4)
                            {
                                continue;
                            }
                            var p = new EcMultiPart
                            {
                                ItemId = (ushort)Num(v[0]),
                                X = (short)Num(v[1]),
                                Y = (short)Num(v[2]),
                                Z = (short)Num(v[3])
                            };
                            if (v.Length > 4)
                            {
                                long flag = Num(v[4]);
                                // UOA / CSV hold the classic flag: 1 = shown -> EC 0
                                p.Flags = uoa || csv ? (ushort)(flag != 0 ? 0 : 1) : (ushort)flag;
                            }
                            if (csv)
                            {
                                foreach (string c in v.Skip(5))
                                {
                                    p.Clilocs.Add((uint)Num(c));
                                }
                            }
                            parts.Add(p);
                        }
                    }

                    _current.Parts.Clear();
                    _current.Parts.AddRange(parts);
                    _package.SetData(_currentEntry, _current.Write());
                    _saveButton.Enabled = true;
                    _hiddenParts.Clear();
                    SetupHeights();
                    FillParts();
                    Redraw();
                    _statusLabel.Text = $"{parts.Count} parts imported (not saved yet).";
                }
                catch (FormatException ex)
                {
                    EcDialogs.ShowError(this, "Invalid line in the file.", ex);
                }
            }
        }

        // ==================================================================== add / remove / save

        private void AddMulti()
        {
            if (_package == null)
            {
                return;
            }
            int next = _multis.Count == 0 ? 0 : _multis.Max(m => m.Id) + 1;
            string text = EcDialogs.Prompt(this, "New multi", "Id (the selected multi is copied, nothing selected = empty):", next.ToString(CultureInfo.InvariantCulture));
            if (text == null || !EcDialogs.TryParseNumber(text, out long id) || id < 0)
            {
                return;
            }
            string name = string.Format(CultureInfo.InvariantCulture, EcMulti.NamePattern, id);
            if (_package.Contains(name) && !EcDialogs.Confirm(this, $"Multi {id} exists already. Replace it?"))
            {
                return;
            }
            var multi = new EcMulti { Id = (int)id };
            if (_current != null)
            {
                multi.Parts.AddRange(_current.Parts.Select(p => p.Clone()));
            }
            _package.AddOrReplace(name, multi.Write(), EcUopCompression.Zlib);
            _saveButton.Enabled = true;
            _current = multi;
            FillList();
            _searchBox.Text = id.ToString(CultureInfo.InvariantCulture);
            Search();
        }

        private void DeleteMulti()
        {
            if (_currentEntry == null || !EcDialogs.Confirm(this, $"Remove multi {_current?.Id}?"))
            {
                return;
            }
            _package.Remove(_currentEntry);
            _current = null;
            _currentEntry = null;
            _parts.Rows.Clear();
            _preview.Image = null;
            _render = null;
            _saveButton.Enabled = true;
            FillList();
        }

        private void Save(bool saveAs)
        {
            if (_package != null && EcPackageSaver.Save(this, new[] { _package }, saveAs))
            {
                _saveButton.Enabled = false;
                _statusLabel.Text = "Saved.";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearSprites();
            }
            base.Dispose(disposing);
        }
    }
}
