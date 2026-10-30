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
    public sealed partial class EcMultisControl : UserControl, IEcEditor
    {
        public const string FileName = "MultiCollection.uop";


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

        /// <summary>Highest id shown with "Free slots" (like the multi limit of the UOP packer), at least the last id + 1.</summary>
        private const int FreeSlotLimit = 0x2200;

        private bool _showFreeSlots;
        private Bitmap _customBackground;

        public EcMultisControl()
        {
            InitializeComponent();

            // runtime only (not in the designer)
            comboBoxArt.SelectedIndex = 0;
            previewPanel.Overlay = DrawOverlay;
            Disposed += (s, e) =>
            {
                ClearSprites();
                _customBackground?.Dispose();
            };
        }

        // ==================================================================== event handlers (designer)

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Search();
            }
        }

        private void OnClickSearch(object sender, EventArgs e) => Search();

        private void OnAfterSelectMulti(object sender, TreeViewEventArgs e) => ShowSelected();

        /// <summary>Right click selects the node first, so the context menu works on it.</summary>
        private void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeViewMulti.SelectedNode = e.Node;
            }
        }

        private void OnTreeMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool open = _package != null;
            bool multi = SelectedEntry != null;
            bool free = SelectedId >= 0 && !multi;
            newToolStripMenuItem.Enabled = open;
            newToolStripMenuItem.Text = free ? $"New multi in slot {SelectedId}..." : "New / copy...";
            importToolStripMenuItem.Enabled = open && SelectedId >= 0;
            exportToolStripMenuItem.Enabled = multi;
            removeToolStripMenuItem.Enabled = multi;
            reloadToolStripMenuItem.Enabled = _client != null;
            saveToolStripMenuItem.Enabled = open && _package.IsDirty;
            saveAsToolStripMenuItem.Enabled = open;
        }

        private void OnFreeSlotsChanged(object sender, EventArgs e) => SetFreeSlots(toolStripButtonFreeSlots.Checked);

        private void OnFreeSlotsMenuChanged(object sender, EventArgs e) => SetFreeSlots(showFreeSlotsToolStripMenuItem.Checked);

        private void OnClickNew(object sender, EventArgs e) => AddMulti();

        private void OnClickImport(object sender, EventArgs e) => ImportParts();

        private void OnClickRemove(object sender, EventArgs e) => DeleteMulti();

        private void OnClickReload(object sender, EventArgs e) => ReloadFile();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnClickExportParts(object sender, EventArgs e) => ExportParts(FormatOf(sender));

        private void OnClickExportAllParts(object sender, EventArgs e) => ExportAllParts(FormatOf(sender));

        private void OnClickExportAllImages(object sender, EventArgs e) => ExportAllImages(ImageFormatOf(sender));

        private void OnClickExtractImage(object sender, EventArgs e) => ExtractImage(ImageFormatOf(sender));

        private void OnClickCopyImage(object sender, EventArgs e) => CopyImage();

        private void OnClickBackgroundColor(object sender, EventArgs e) => ChangeBackgroundColor();

        private void OnClickBackgroundOff(object sender, EventArgs e) => previewPanel.BackgroundTile = null;

        private void OnClickBackgroundGrass(object sender, EventArgs e) => previewPanel.BackgroundTile = UoFiddler.Controls.Properties.Resources.Grass;

        private void OnClickBackgroundWater(object sender, EventArgs e) => previewPanel.BackgroundTile = UoFiddler.Controls.Properties.Resources.Water;

        private void OnClickBackgroundFile(object sender, EventArgs e) => LoadBackgroundImage();

        private void OnClickShowAll(object sender, EventArgs e) => SetAllVisible(true);

        private void OnClickHideSelected(object sender, EventArgs e) => SetSelectedVisible(false);

        private void OnClickShowOnlySelected(object sender, EventArgs e)
        {
            SetAllVisible(false);
            SetSelectedVisible(true);
        }

        private void OnClickSelectSameItem(object sender, EventArgs e) => SelectSameItem();

        private void OnClickSelectSameZ(object sender, EventArgs e) => SelectSameZ();

        private void OnHeightChanged(object sender, EventArgs e)
        {
            if (!_settingHeight)
            {
                UpdateHeightLabel();
                Redraw();
            }
        }

        private void OnMinZChanged(object sender, EventArgs e)
        {
            if (!_settingHeight)
            {
                Redraw();
            }
        }

        private void OnFloorChanged(object sender, EventArgs e) => SelectFloor();

        private void OnViewOptionChanged(object sender, EventArgs e) => Redraw();

        private void OnPreviewMouseMove(object sender, MouseEventArgs e) => HoverAt(e.Location);

        private void OnPreviewMouseLeave(object sender, EventArgs e)
        {
            _hoverPart = -1;
            toolStripStatusLabelHover.Text = string.Empty;
            previewPanel.Invalidate();
        }

        private void OnPreviewMouseClick(object sender, MouseEventArgs e) => ClickAt(e.Location, e.Button);

        private void OnPartsCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != columnShow.Index)
            {
                PartsChanged();
            }
        }

        private void OnPartsUserDeletedRow(object sender, DataGridViewRowEventArgs e) => PartsChanged();

        private void OnPartsDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridViewParts.CurrentCell?.ColumnIndex == columnShow.Index)
            {
                dataGridViewParts.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void OnPartsCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_filling || e.ColumnIndex != columnShow.Index || e.RowIndex < 0 || e.RowIndex >= (_current?.Parts.Count ?? 0))
            {
                return;
            }
            bool show = dataGridViewParts.Rows[e.RowIndex].Cells[columnShow.Index].Value as bool? ?? true;
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

        private void OnPartsSelectionChanged(object sender, EventArgs e) => PartSelectionChanged();

        private void OnPartsCellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                ShowPartImage(e.RowIndex);
            }
        }

        private void OnPartsMouseLeave(object sender, EventArgs e) => ShowPartImage(dataGridViewParts.CurrentRow?.Index ?? -1);

        private void OnPackageChanged(object sender, EventArgs e) => toolStripButtonSave.Enabled = _package?.IsDirty ?? false;

        private static PartFormat FormatOf(object sender) =>
            Enum.TryParse((sender as ToolStripItem)?.Tag as string, true, out PartFormat format) ? format : PartFormat.Text;

        private static ImageFormat ImageFormatOf(object sender) => ((sender as ToolStripItem)?.Tag as string) switch
        {
            "Bmp" => ImageFormat.Bmp,
            "Tiff" => ImageFormat.Tiff,
            "Jpg" => ImageFormat.Jpeg,
            _ => ImageFormat.Png
        };

        // ==================================================================== free slots, reload, background

        /// <summary>Id of the selected node (multi or free slot), or -1.</summary>
        private int SelectedId =>
            treeViewMulti.SelectedNode != null && int.TryParse(treeViewMulti.SelectedNode.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? id : -1;

        private EcUopEntry SelectedEntry => treeViewMulti.SelectedNode?.Tag as EcUopEntry;

        private void SetFreeSlots(bool show)
        {
            if (_showFreeSlots == show)
            {
                return;
            }
            _showFreeSlots = show;
            toolStripButtonFreeSlots.Checked = show;
            showFreeSlotsToolStripMenuItem.Checked = show;
            if (_package != null)
            {
                FillList();
            }
        }

        private void ReloadFile()
        {
            if (_client == null)
            {
                return;
            }
            if (_package != null && _package.IsDirty && !EcDialogs.Confirm(this, $"{FileName} has unsaved changes. Read it again and lose them?"))
            {
                return;
            }
            int selected = SelectedId;
            if (_package != null)
            {
                _package.Changed -= OnPackageChanged;
            }
            _package = _client.ReloadPackage(FileName);
            _current = null;
            _currentEntry = null;
            if (_package == null)
            {
                treeViewMulti.Nodes.Clear();
                toolStripStatusLabel.Text = $"{FileName} was not found.";
                return;
            }
            _package.Changed += OnPackageChanged;
            toolStripButtonSave.Enabled = false;
            ClearSprites();
            LoadMultiNames();
            FillList();
            SelectId(selected);
            toolStripStatusLabel.Text = $"{FileName} read again.";
        }

        private void SelectId(int id)
        {
            TreeNode[] found = treeViewMulti.Nodes.Find(id.ToString(CultureInfo.InvariantCulture), false);
            if (found.Length > 0)
            {
                treeViewMulti.SelectedNode = found[0];
                found[0].EnsureVisible();
            }
        }

        private void LoadBackgroundImage()
        {
            using (var dialog = new OpenFileDialog { Filter = "Images|*.png;*.bmp;*.jpg;*.jpeg;*.gif;*.tif;*.tiff;*.dds;*.tga|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    Bitmap image = EcImageHelper.LoadImageFile(dialog.FileName);
                    previewPanel.BackgroundTile = image;
                    _customBackground?.Dispose();
                    _customBackground = image;
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "The image could not be read.", ex);
                }
            }
        }

        // ==================================================================== loading

        public void SetClient(EcClient client)
        {
            _client = client;
            _loaded = false;
            _package = null;
            _multis.Clear();
            treeViewMulti.Nodes.Clear();
            dataGridViewParts.Rows.Clear();
            previewPanel.Image = null;
            _render = null;
            _current = null;
            _currentEntry = null;
            ClearSprites();
            toolStripButtonSave.Enabled = false;
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
                toolStripStatusLabel.Text = $"{FileName} was not found.";
                return;
            }
            _package.Changed += OnPackageChanged;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                _client.GetTileArt();
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = $"tileart.uop: {ex.Message}";
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

            int selected = SelectedId >= 0 ? SelectedId : _current?.Id ?? -1;
            var entries = _multis.ToDictionary(m => m.Id, m => m.Entry);
            int last = _multis.Count == 0 ? 0 : _multis[_multis.Count - 1].Id;
            IEnumerable<int> ids = _showFreeSlots ? Enumerable.Range(0, Math.Max(FreeSlotLimit, last + 1)) : _multis.Select(m => m.Id);
            var nodes = new List<TreeNode>();
            TreeNode select = null;
            int free = 0;
            foreach (int id in ids)
            {
                entries.TryGetValue(id, out EcUopEntry entry);
                _multiNames.TryGetValue(id, out var name);
                var node = new TreeNode($"{id,5} (0x{id:X}) {name.Name}".TrimEnd())
                {
                    Tag = entry,
                    Name = id.ToString(CultureInfo.InvariantCulture),
                    ToolTipText = string.IsNullOrEmpty(name.Tip) ? name.Name : $"{name.Name}\r\n{name.Tip}"
                };
                if (entry == null)
                {
                    // free slot
                    node.ForeColor = Color.Gray;
                    node.Text = $"{id,5} (0x{id:X}) – free";
                    node.ToolTipText = "Free slot: New or Import in the context menu";
                    free++;
                }
                nodes.Add(node);
                if (id == selected)
                {
                    select = node;
                }
            }
            treeViewMulti.BeginUpdate();
            treeViewMulti.Nodes.Clear();
            treeViewMulti.Nodes.AddRange(nodes.ToArray());
            treeViewMulti.EndUpdate();
            if (select != null)
            {
                treeViewMulti.SelectedNode = select;
            }
            else if (treeViewMulti.Nodes.Count > 0 && treeViewMulti.SelectedNode == null)
            {
                treeViewMulti.SelectedNode = treeViewMulti.Nodes[0];
            }
            toolStripStatusLabel.Text = $"{_multis.Count} multis" + (_showFreeSlots ? $", {free} free slots" : string.Empty) +
                                        (_multiNames.Count == 0 ? " (Multilist.xml not found – no names)" : string.Empty);
        }

        private void Search()
        {
            string text = toolStripTextBoxSearch.Text.Trim();
            if (text.Length == 0 || treeViewMulti.Nodes.Count == 0)
            {
                return;
            }
            if (EcDialogs.TryParseNumber(text, out long id))
            {
                TreeNode[] found = treeViewMulti.Nodes.Find(id.ToString(CultureInfo.InvariantCulture), false);
                if (found.Length > 0)
                {
                    treeViewMulti.SelectedNode = found[0];
                    found[0].EnsureVisible();
                    return;
                }
            }
            // name search, starting after the selected node
            int start = treeViewMulti.SelectedNode?.Index ?? -1;
            for (int i = 1; i <= treeViewMulti.Nodes.Count; i++)
            {
                TreeNode node = treeViewMulti.Nodes[(start + i) % treeViewMulti.Nodes.Count];
                if (node.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    treeViewMulti.SelectedNode = node;
                    node.EnsureVisible();
                    return;
                }
            }
            toolStripStatusLabel.Text = $"\"{text}\" not found.";
        }

        private void ShowSelected()
        {
            if (treeViewMulti.SelectedNode?.Tag is not EcUopEntry entry)
            {
                if (SelectedId >= 0)
                {
                    // free slot
                    _current = null;
                    _currentEntry = null;
                    _render = null;
                    _hiddenParts.Clear();
                    _highlighted.Clear();
                    _filling = true;
                    dataGridViewParts.Rows.Clear();
                    _filling = false;
                    previewPanel.Image = null;
                    toolStripStatusLabel.Text = $"Slot {SelectedId} (0x{SelectedId:X}) is free – New or Import in the context menu.";
                }
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
                toolStripStatusLabel.Text = ex.Message;
            }
        }

        private void SetupHeights()
        {
            _settingHeight = true;
            var b = _current.Bounds();
            int minZ = Math.Min(0, b.MinZ), maxZ = Math.Max(0, b.MaxZ);
            trackBarHeight.Minimum = minZ;
            trackBarHeight.Maximum = maxZ;
            trackBarHeight.Value = maxZ;
            trackBarHeight.TickFrequency = Math.Max(1, (maxZ - minZ) / 40);
            numericUpDownMinZ.Value = Math.Max(numericUpDownMinZ.Minimum, minZ);

            // floors: heights of background / surface parts that cover a good part of the multi
            comboBoxFloor.Items.Clear();
            comboBoxFloor.Items.Add("All");
            var floorZ = _current.Parts
                .Where(p => SpriteFor(p.ItemId, false).Background)
                .GroupBy(p => (int)p.Z)
                .Where(g => g.Count() >= Math.Max(2, _current.Parts.Count / 100))
                .Select(g => g.Key)
                .OrderBy(z => z)
                .ToList();
            foreach (int z in floorZ)
            {
                comboBoxFloor.Items.Add(new FloorItem(z));
            }
            comboBoxFloor.SelectedIndex = 0;
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
            if (comboBoxFloor.SelectedItem is FloorItem floor)
            {
                FloorItem next = comboBoxFloor.Items.OfType<FloorItem>().FirstOrDefault(f => f.Z > floor.Z);
                numericUpDownMinZ.Value = Math.Max(numericUpDownMinZ.Minimum, floor.Z);
                trackBarHeight.Value = Math.Min(trackBarHeight.Maximum, next != null ? next.Z - 1 : trackBarHeight.Maximum);
            }
            else
            {
                numericUpDownMinZ.Value = Math.Max(numericUpDownMinZ.Minimum, trackBarHeight.Minimum);
                trackBarHeight.Value = trackBarHeight.Maximum;
            }
            _settingHeight = false;
            UpdateHeightLabel();
            Redraw();
        }

        private void UpdateHeightLabel()
        {
            labelMaxZ.Text = $"Max z: {trackBarHeight.Value,3}";
        }

        private void FillParts()
        {
            _filling = true;
            dataGridViewParts.Rows.Clear();
            for (int i = 0; i < _current.Parts.Count; i++)
            {
                EcMultiPart p = _current.Parts[i];
                dataGridViewParts.Rows.Add(!_hiddenParts.Contains(i), $"0x{p.ItemId:X4}", ItemName(p.ItemId), p.X, p.Y, p.Z, p.Flags, string.Join(" ", p.Clilocs));
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
                foreach (DataGridViewRow row in dataGridViewParts.Rows)
                {
                    if (row.IsNewRow || row.Cells[columnItem.Index].Value == null)
                    {
                        continue;
                    }
                    var p = new EcMultiPart
                    {
                        ItemId = (ushort)Num(row.Cells[columnItem.Index].Value),
                        X = (short)Num(row.Cells[columnX.Index].Value),
                        Y = (short)Num(row.Cells[columnY.Index].Value),
                        Z = (short)Num(row.Cells[columnZ.Index].Value),
                        Flags = (ushort)Num(row.Cells[columnFlags.Index].Value)
                    };
                    foreach (string c in (row.Cells[columnClilocs.Index].Value?.ToString() ?? string.Empty).Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        p.Clilocs.Add((uint)Num(c));
                    }
                    parts.Add(p);
                }
                _current.Parts.Clear();
                _current.Parts.AddRange(parts);
                _package.SetData(_currentEntry, _current.Write());
                toolStripButtonSave.Enabled = true;

                // refresh the name and show check of new rows
                _filling = true;
                for (int i = 0; i < parts.Count && i < dataGridViewParts.Rows.Count; i++)
                {
                    dataGridViewParts.Rows[i].Cells[columnName.Index].Value = ItemName(parts[i].ItemId);
                    if (dataGridViewParts.Rows[i].Cells[columnShow.Index].Value == null)
                    {
                        dataGridViewParts.Rows[i].Cells[columnShow.Index].Value = true;
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

        private bool LegacyMode => comboBoxArt.SelectedIndex == 1;

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
                    // Several items can share one image (atlas): the sails and the mast of a boat all point to
                    // "00016093_mast.tga" and only use different areas of it. So the image tileart points to is used,
                    // not "worldart/<item id>".
                    TileArtImageOffset o = tile.EcImageOffset;
                    int artId = WorldArtId(tile, id);
                    if (o.XEnd > o.XStart && o.YEnd > o.YStart && artId >= 0)
                    {
                        Bitmap cut = Cut(WorldArt(artId), o);
                        if (cut != null)
                        {
                            sprite.Image = cut;
                            sprite.OffsetX = o.XOffset;
                            sprite.OffsetY = o.YOffset - cut.Height;
                            sprite.Source = artId == id ? "World art (EC)" : $"World art (EC) {artId:D8}, area {o.XStart},{o.YStart}-{o.XEnd},{o.YEnd}";
                            return sprite;
                        }
                    }
                }

                // legacy (classic) art "build/tileartlegacy/<id>.dds": the classic image is the area of the classic offset
                Bitmap legacyFull = Decode(_client.GetArtData(EcClient.ArtSources[1], (int)id));
                Bitmap image = tile != null ? Cut(legacyFull, tile.CcImageOffset) : legacyFull;
                if (image != legacyFull)
                {
                    legacyFull?.Dispose();
                }
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

        private readonly Dictionary<int, Bitmap> _worldArt = new Dictionary<int, Bitmap>();

        /// <summary>Decoded world art image (cached, shared by all items that use it – don't dispose).</summary>
        private Bitmap WorldArt(int artId)
        {
            if (!_worldArt.TryGetValue(artId, out Bitmap image))
            {
                image = Decode(_client.GetArtData(EcClient.ArtSources[0], artId));
                _worldArt[artId] = image;
            }
            return image;
        }

        /// <summary>
        /// World art id of the item image: the slot 0 image of tileart with the item id if there is one, else the
        /// first slot 0 image that is not a helper texture (ids from 1000000 on: noise, ripple, ground textures, ...).
        /// </summary>
        private int WorldArtId(TileArtEntry tile, uint itemId)
        {
            var ids = new List<int>();
            if (_client.HasStringDictionary)
            {
                foreach (TileArtTexture texture in tile.Textures.Where(t => t.Slot == 0))
                {
                    foreach (TileArtTextureImage image in texture.Images)
                    {
                        if (EcClient.TryParseTexturePath(_client.StringDictionary.Get(image.NameIndex), out EcArtSource source, out int artId)
                            && source == EcClient.ArtSources[0])
                        {
                            ids.Add(artId);
                        }
                    }
                }
            }
            if (ids.Contains((int)itemId))
            {
                return (int)itemId;
            }
            if (ids.Count == 0)
            {
                return (int)itemId;
            }
            List<int> items = ids.Where(i => i < 1000000).ToList();
            return items.Count > 0 ? items[0] : -1;
        }

        /// <summary>Cuts the used area out of a padded image or atlas (the full image is kept; may return it unchanged).</summary>
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
            return full.Clone(rect, PixelFormat.Format32bppArgb);
        }

        private void ClearSprites()
        {
            var shared = new HashSet<Bitmap>(_worldArt.Values.Where(b => b != null));
            foreach (Sprite s in _sprites.Values)
            {
                if (s.Image != null && !shared.Contains(s.Image))
                {
                    s.Image.Dispose();
                }
            }
            _sprites.Clear();
            foreach (Bitmap b in shared)
            {
                b.Dispose();
            }
            _worldArt.Clear();
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
                previewPanel.Image = null;
                _render = null;
                return;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                _render = Render(_current, (int)numericUpDownMinZ.Value, trackBarHeight.Value, LegacyMode, checkBoxFlaggedParts.Checked, _hiddenParts, checkBoxGrid.Checked);
                previewPanel.Image = _render.Image;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            var b = _current.Bounds();
            int surface = _current.Parts.Select(p => (p.X, p.Y)).Distinct().Count();
            int missing = _render.Placed.Count(p => p.Sprite.Image == null);
            toolStripStatusLabel.Text = $"Multi {_current.Id}: Size: {b.MaxX - b.MinX + 1},{b.MaxY - b.MinY + 1} MaxHeight: {b.MaxZ} " +
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
            Placed placed = PartAt(previewPanel.ClientToImage(location));
            int index = placed?.Index ?? -1;
            if (index == _hoverPart)
            {
                return;
            }
            _hoverPart = index;
            toolStripStatusLabelHover.Text = placed == null ? string.Empty
                : $"0x{placed.Part.ItemId:X4} {placed.Sprite.Name}  x {placed.Part.X} y {placed.Part.Y} z {placed.Part.Z} flags {placed.Part.Flags}";
            previewPanel.Invalidate();
        }

        private void ClickAt(Point location, MouseButtons button)
        {
            if (button != MouseButtons.Left)
            {
                return;
            }
            Placed placed = PartAt(previewPanel.ClientToImage(location));
            if (placed == null || placed.Index >= dataGridViewParts.Rows.Count)
            {
                return;
            }
            bool add = (ModifierKeys & Keys.Control) != 0;
            if (!add)
            {
                dataGridViewParts.ClearSelection();
            }
            DataGridViewRow row = dataGridViewParts.Rows[placed.Index];
            row.Selected = !add || !row.Selected;
            dataGridViewParts.CurrentCell = row.Cells[columnItem.Index];
            dataGridViewParts.FirstDisplayedScrollingRowIndex = Math.Max(0, placed.Index - 3);
        }

        private void PartSelectionChanged()
        {
            _highlighted.Clear();
            foreach (DataGridViewRow row in dataGridViewParts.SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    _highlighted.Add(row.Index);
                }
            }
            previewPanel.Invalidate();
            ShowPartImage(dataGridViewParts.CurrentRow?.Index ?? -1);
        }

        private void ShowPartImage(int index)
        {
            if (_current == null || index < 0 || index >= _current.Parts.Count)
            {
                pictureBoxPart.Image = null;
                labelPartInfo.Text = string.Empty;
                return;
            }
            EcMultiPart p = _current.Parts[index];
            Sprite sprite = SpriteFor(p.ItemId, LegacyMode);
            pictureBoxPart.Image = sprite.Image;
            labelPartInfo.Text = $"0x{p.ItemId:X4} {sprite.Name}\r\nx {p.X} y {p.Y} z {p.Z} flags {p.Flags}\r\n{sprite.Source ?? "no image"}";
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
            foreach (DataGridViewRow row in dataGridViewParts.SelectedRows)
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
            for (int i = 0; i < dataGridViewParts.Rows.Count; i++)
            {
                if (!dataGridViewParts.Rows[i].IsNewRow)
                {
                    dataGridViewParts.Rows[i].Cells[columnShow.Index].Value = !_hiddenParts.Contains(i);
                }
            }
            _filling = false;
            Redraw();
        }

        private void SelectSameItem()
        {
            if (_current == null || dataGridViewParts.CurrentRow == null || dataGridViewParts.CurrentRow.Index >= _current.Parts.Count)
            {
                return;
            }
            ushort item = _current.Parts[dataGridViewParts.CurrentRow.Index].ItemId;
            SelectWhere(p => p.ItemId == item);
        }

        private void SelectSameZ()
        {
            if (_current == null || dataGridViewParts.CurrentRow == null || dataGridViewParts.CurrentRow.Index >= _current.Parts.Count)
            {
                return;
            }
            short z = _current.Parts[dataGridViewParts.CurrentRow.Index].Z;
            SelectWhere(p => p.Z == z);
        }

        private void SelectWhere(Func<EcMultiPart, bool> predicate)
        {
            dataGridViewParts.ClearSelection();
            for (int i = 0; i < _current.Parts.Count && i < dataGridViewParts.Rows.Count; i++)
            {
                if (predicate(_current.Parts[i]))
                {
                    dataGridViewParts.Rows[i].Selected = true;
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
            bool transparent = format.Equals(ImageFormat.Png) && useTransparencyForPngToolStripMenuItem.Checked;
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
            string suffix = trackBarHeight.Value < trackBarHeight.Maximum || numericUpDownMinZ.Value > trackBarHeight.Minimum ? $"_Z{numericUpDownMinZ.Value:000}-{trackBarHeight.Value:000}" : string.Empty;
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
                    toolStripStatusLabel.Text = $"Multi saved to {dialog.FileName}";
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
            toolStripStatusLabel.Text = "Image copied to the clipboard.";
        }

        private void ChangeBackgroundColor()
        {
            using (var dialog = new ColorDialog { Color = _backgroundColor })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _backgroundColor = dialog.Color;
                    previewPanel.BackColor = dialog.Color;
                    previewPanel.Invalidate();
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
                            RenderResult r = Render(multi, short.MinValue, short.MaxValue, LegacyMode, checkBoxFlaggedParts.Checked, null, false);
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
                            toolStripStatusLabel.Text = $"Exporting images... {done}";
                            Application.DoEvents();
                        }
                    }
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
                toolStripStatusLabel.Text = $"{done} images saved to {dialog.SelectedPath}";
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
                    toolStripStatusLabel.Text = $"Multi saved to {dialog.FileName}";
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
                toolStripStatusLabel.Text = $"{done} multis saved to {dialog.SelectedPath}";
            }
        }

        /// <summary>
        /// Reads text ("item x y z flags", the EC flags), UOA (classic flags), CSV (classic flags) or WSC files.
        /// </summary>
        private void ImportParts()
        {
            // into the selected multi, or into the selected free slot (a new multi)
            int targetId = _current?.Id ?? SelectedId;
            if (_package == null || targetId < 0)
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

                    if (_current == null || _currentEntry == null)
                    {
                        var multi = new EcMulti { Id = targetId };
                        multi.Parts.AddRange(parts);
                        _package.AddOrReplace(string.Format(CultureInfo.InvariantCulture, EcMulti.NamePattern, targetId), multi.Write(), EcUopCompression.Zlib);
                        toolStripButtonSave.Enabled = true;
                        FillList();
                        SelectId(targetId);
                    }
                    else
                    {
                        _current.Parts.Clear();
                        _current.Parts.AddRange(parts);
                        _package.SetData(_currentEntry, _current.Write());
                        toolStripButtonSave.Enabled = true;
                        _hiddenParts.Clear();
                        SetupHeights();
                        FillParts();
                        Redraw();
                    }
                    toolStripStatusLabel.Text = $"{parts.Count} parts imported into multi {targetId} (not saved yet).";
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
            // on a free slot: that id, else the next id after the last multi
            int next = SelectedEntry == null && SelectedId >= 0 ? SelectedId : (_multis.Count == 0 ? 0 : _multis.Max(m => m.Id) + 1);
            string text = EcDialogs.Prompt(this, "New multi", "Id (the selected multi is copied, free slot = empty multi):", next.ToString(CultureInfo.InvariantCulture));
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
            toolStripButtonSave.Enabled = true;
            _current = multi;
            FillList();
            SelectId((int)id);
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
            dataGridViewParts.Rows.Clear();
            previewPanel.Image = null;
            _render = null;
            toolStripButtonSave.Enabled = true;
            FillList();
        }

        private void Save(bool saveAs)
        {
            if (_package != null && EcPackageSaver.Save(this, new[] { _package }, saveAs))
            {
                toolStripButtonSave.Enabled = false;
                toolStripStatusLabel.Text = "Saved.";
            }
        }
    }
}
