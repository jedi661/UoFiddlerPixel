/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Rewritten: virtual list instead of two tree views with 80,000+ nodes (the tab opened slowly and every colour
 * change searched all nodes), extended ids (land 0x4000+, items 0x10000+), filters, eyedropper, undo per entry,
 * batch averages with progress / cancel. Fixed: the background worker handlers were added again on every reload,
 * "RadarCol" vs. "RadarColor" change flag (unsaved changes were not reported), the last item was missing,
 * division by zero for empty graphics, leaked preview bitmaps, a new timer per key press in the filter.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using Ultima.Helpers;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class RadarColorControl : UserControl
    {
        private const string ChangeKey = "RadarColor"; // same key as in Options (was "RadarCol" -> never reported)

        private static RadarColorControl _refMarker;

        // ---- data
        private bool _land;                       // false = items, true = land tiles
        private int[] _ids = Array.Empty<int>();   // ids shown in the list (filtered, sorted)
        private readonly Dictionary<int, ushort> _originalItems = new Dictionary<int, ushort>();
        private readonly Dictionary<int, ushort> _originalLand = new Dictionary<int, ushort>();
        private int _selectedId = -1;
        private ushort _currentColor;
        private bool _updating;

        // ---- preview
        private Bitmap _previewImage;     // owned (drawn), disposed on change
        private Bitmap _previewSource;    // NOT owned (Art cache)
        private RectangleF _previewRect;  // where the source is drawn inside the picture box

        // ---- batch job
        private Timer _jobTimer;
        private IEnumerator<int> _job;
        private int _jobTotal, _jobDone;
        private bool _jobCancel;
        private string _jobName;

        // ---- controls
        private ToolStripButton _itemsButton, _landButton;
        private ToolStripTextBox _searchBox;
        private ToolStripComboBox _filterBox;
        private ToolStripLabel _countLabel;
        private ListView _list;
        private PictureBox _preview, _swatchCurrent, _swatchFile;
        private Label _titleLabel, _infoLabel, _statusLabel;
        private NumericUpDown _r, _g, _b;
        private TextBox _hexBox, _htmlBox, _fromBox, _toBox;
        private CheckBox _autoApply, _copyHexOnSelect;
        private RadioButton _useSelection, _useRange;
        private Button _applyButton, _revertButton, _revertAllButton;
        private ProgressBar _progress;
        private Button _cancelJobButton;
        private readonly Timer _searchTimer = new Timer { Interval = 300 };
        private readonly ToolTip _tips = new ToolTip();

        public RadarColorControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            _refMarker = this;
            BuildLayout();
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                RebuildList(true);
            };
        }

        public bool IsLoaded { get; private set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort CurrentColor
        {
            get => _currentColor;
            set
            {
                _currentColor = (ushort)(value & 0x7FFF);
                ShowCurrentColor();
            }
        }

        #region Public API (used by Items, LandTiles, TileData)

        /// <summary>Selects an item (land = false) or a land tile in the list.</summary>
        public static void Select(int graphic, bool land)
        {
            if (_refMarker == null)
            {
                return;
            }

            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            _refMarker.SelectId(graphic, land);
        }

        /// <summary>Brings the RadarColor tab to the front.</summary>
        public static void ActivateTab()
        {
            TabNavigation.BringToFront(_refMarker);
        }

        #endregion

        #region Layout

        private void BuildLayout()
        {
            // --- tool strip
            var strip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            _itemsButton = new ToolStripButton("Items") { Checked = true, CheckOnClick = false, ToolTipText = "Radar colours of the items (statics)" };
            _landButton = new ToolStripButton("Land tiles") { ToolTipText = "Radar colours of the land tiles" };
            _itemsButton.Click += (s, e) => SetMode(false);
            _landButton.Click += (s, e) => SetMode(true);
            _searchBox = new ToolStripTextBox { AutoSize = false, Width = 160, ToolTipText = "Search: id (0x1F00 or 7936) or part of the name" };
            _searchBox.TextChanged += (s, e) =>
            {
                _searchTimer.Stop();
                _searchTimer.Start();
            };
            _searchBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    _searchTimer.Stop();
                    JumpToSearch();
                }
            };
            _filterBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 190, ToolTipText = "Which entries are listed" };
            _filterBox.Items.AddRange(new object[]
            {
                "All ids", "With graphic", "Changed (not saved)", "Black / missing colour", "Black with graphic", "Extended ids only"
            });
            _filterBox.SelectedIndex = 0;
            _filterBox.SelectedIndexChanged += (s, e) => RebuildList(true);
            _countLabel = new ToolStripLabel { ForeColor = Color.DimGray };

            var save = new ToolStripButton("Save") { ToolTipText = "Writes radarcol.mul (and radarcolext.mul / radarcollandext.mul for extended ids) into the output folder" };
            save.Click += (s, e) => SaveFile();
            var misc = new ToolStripDropDownButton("Misc");
            misc.DropDownItems.Add("Export CSV (output folder)", null, (s, e) => ExportCsv());
            misc.DropDownItems.Add("Import CSV...", null, (s, e) => ImportCsv());
            misc.DropDownItems.Add(new ToolStripSeparator());
            misc.DropDownItems.Add("Fill black / missing colours from the graphics (all)...", null, (s, e) => FillAllBlack());
            misc.DropDownItems.Add(new ToolStripSeparator());
            misc.DropDownItems.Add("Extended formats (items / land tiles)...", null,
                (s, e) => ExtendedArtFormatForm.ShowFor(_land ? UoExtensionKind.LandTiles : UoExtensionKind.Items));

            strip.Items.AddRange(new ToolStripItem[]
            {
                _itemsButton, _landButton, new ToolStripSeparator(),
                new ToolStripLabel("Search:"), _searchBox, new ToolStripLabel("Show:"), _filterBox, new ToolStripSeparator(),
                save, misc, new ToolStripSeparator(), _countLabel
            });

            // --- list (virtual: only visible rows exist)
            _list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                VirtualMode = true,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = true,
                OwnerDraw = true,
                GridLines = false,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            _list.Columns.Add("Colour", 56);
            _list.Columns.Add("ID", 120);
            _list.Columns.Add("Name", 190);
            _list.Columns.Add("Value", 70);
            _list.Columns.Add("RGB", 90);
            _list.RetrieveVirtualItem += OnRetrieveVirtualItem;
            _list.DrawColumnHeader += (s, e) => e.DrawDefault = true;
            _list.DrawSubItem += OnDrawSubItem;
            _list.SelectedIndexChanged += (s, e) => OnListSelectionChanged();
            _list.VirtualItemsSelectionRangeChanged += (s, e) => OnListSelectionChanged();
            _list.KeyDown += OnListKeyDown;
            _list.ContextMenuStrip = BuildListMenu();
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(_list, true);

            // --- right side
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(6), AutoScroll = true };
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var head = new Panel { Dock = DockStyle.Top, Height = 42 };
            _titleLabel = new Label { AutoSize = true, Font = new Font(Font.FontFamily, 10f, FontStyle.Bold), Location = new Point(0, 0) };
            _infoLabel = new Label { AutoSize = true, ForeColor = Color.DimGray, Location = new Point(0, 22) };
            head.Controls.Add(_titleLabel);
            head.Controls.Add(_infoLabel);
            right.Controls.Add(head, 0, 0);

            _preview = new PictureBox { Dock = DockStyle.Fill, MinimumSize = new Size(120, 120), BackColor = Color.FromArgb(40, 40, 40), Cursor = Cursors.Cross };
            _preview.Resize += (s, e) => RenderPreview();
            _preview.MouseClick += OnPreviewClick;
            _tips.SetToolTip(_preview, "Click on a pixel to take its colour (eyedropper).");
            right.Controls.Add(_preview, 0, 1);

            right.Controls.Add(BuildColorGroup(), 0, 2);
            right.Controls.Add(BuildBatchGroup(), 0, 3);

            _statusLabel = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 6, 3, 3) };
            right.Controls.Add(_statusLabel, 0, 4);

            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.None };
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(right);

            Controls.Add(split);
            Controls.Add(strip);
            HandleCreated += (s, e) =>
            {
                try
                {
                    split.SplitterDistance = Math.Max(200, Math.Min(560, Width / 2));
                }
                catch (InvalidOperationException)
                {
                    // too small yet
                }
            };
        }

        private GroupBox BuildColorGroup()
        {
            var group = new GroupBox { Text = "Colour", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 6 };

            _swatchCurrent = new PictureBox { Size = new Size(64, 40), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            _swatchCurrent.Click += (s, e) => PickWithDialog();
            _tips.SetToolTip(_swatchCurrent, "New colour - click to choose with the colour dialog");
            _swatchFile = new PictureBox { Size = new Size(32, 40), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            _swatchFile.Click += (s, e) => RevertSelected();
            _tips.SetToolTip(_swatchFile, "Colour in the loaded file - click to go back to it");

            _r = MakeNumber();
            _g = MakeNumber();
            _b = MakeNumber();
            _hexBox = new TextBox { Width = 70, Font = new Font(FontFamily.GenericMonospace, 9f) };
            _hexBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ParseHex();
                }
            };
            _hexBox.Leave += (s, e) => ParseHex();
            _tips.SetToolTip(_hexBox, "Radar colour value (15 bit, 0x0000 - 0x7FFF)");
            _htmlBox = new TextBox { Width = 70, Font = new Font(FontFamily.GenericMonospace, 9f) };
            _htmlBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ParseHtml();
                }
            };
            _htmlBox.Leave += (s, e) => ParseHtml();
            _tips.SetToolTip(_htmlBox, "#RRGGBB (as in image programs) - double click copies it");
            _htmlBox.DoubleClick += (s, e) => CopyText(_htmlBox.Text);

            grid.Controls.Add(_swatchCurrent, 0, 0);
            grid.SetRowSpan(_swatchCurrent, 2);
            grid.Controls.Add(_swatchFile, 1, 0);
            grid.SetRowSpan(_swatchFile, 2);
            grid.Controls.Add(Pair("R", _r), 2, 0);
            grid.Controls.Add(Pair("G", _g), 3, 0);
            grid.Controls.Add(Pair("B", _b), 4, 0);
            grid.Controls.Add(Pair("Value", _hexBox), 2, 1);
            grid.Controls.Add(Pair("HTML", _htmlBox), 3, 1);
            grid.SetColumnSpan(grid.GetControlFromPosition(3, 1), 2);

            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
            _applyButton = MakeButton("Apply (Enter)", (s, e) => ApplyToSelection());
            _tips.SetToolTip(_applyButton, "Sets the colour for all selected entries");
            buttons.Controls.Add(_applyButton);
            buttons.Controls.Add(MakeButton("Average of graphic", (s, e) => AverageOfCurrent()));
            _revertButton = MakeButton("Revert", (s, e) => RevertSelected());
            _revertAllButton = MakeButton("Revert all", (s, e) => RevertAll());
            buttons.Controls.Add(_revertButton);
            buttons.Controls.Add(_revertAllButton);
            _autoApply = new CheckBox { Text = "Apply while editing", AutoSize = true, Checked = true, Margin = new Padding(8, 7, 3, 3) };
            _tips.SetToolTip(_autoApply, "Every change of R/G/B, value, eyedropper ... is written to the selected entry at once");
            buttons.Controls.Add(_autoApply);
            _copyHexOnSelect = new CheckBox { Text = "Copy value on select", AutoSize = true, Margin = new Padding(8, 7, 3, 3) };
            buttons.Controls.Add(_copyHexOnSelect);

            var jump = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
            jump.Controls.Add(new Label { Text = "Show in:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
            jump.Controls.Add(MakeButton("Items / LandTiles", (s, e) => ShowInArtTab()));
            jump.Controls.Add(MakeButton("TileData", (s, e) => ShowInTileData()));

            var outer = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1 };
            outer.Controls.Add(grid, 0, 0);
            outer.Controls.Add(buttons, 0, 1);
            outer.Controls.Add(jump, 0, 2);
            group.Controls.Add(outer);
            return group;
        }

        private GroupBox BuildBatchGroup()
        {
            var group = new GroupBox { Text = "Averages for several entries", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            _useSelection = new RadioButton { Text = "Selected rows", AutoSize = true, Checked = true, Margin = new Padding(3, 6, 3, 3) };
            _useRange = new RadioButton { Text = "Range", AutoSize = true, Margin = new Padding(3, 6, 3, 3) };
            _fromBox = new TextBox { Width = 80 };
            _toBox = new TextBox { Width = 80 };
            _tips.SetToolTip(_fromBox, "First id (0x... or decimal) - or right click a row: 'Set as range from'");
            _tips.SetToolTip(_toBox, "Last id (included)");
            _fromBox.TextChanged += (s, e) => _useRange.Checked = true;
            _toBox.TextChanged += (s, e) => _useRange.Checked = true;
            flow.Controls.Add(_useSelection);
            flow.Controls.Add(_useRange);
            flow.Controls.Add(_fromBox);
            flow.Controls.Add(new Label { Text = "to", AutoSize = true, Margin = new Padding(3, 7, 3, 3) });
            flow.Controls.Add(_toBox);
            flow.SetFlowBreak(_toBox, true);
            flow.Controls.Add(MakeButton("Current = their average", (s, e) => CurrentToGroupAverage()));
            flow.Controls.Add(MakeButton("All = one common average", (s, e) => GroupToCommonAverage()));
            flow.Controls.Add(MakeButton("Each = its own average", (s, e) => GroupToOwnAverage()));
            _progress = new ProgressBar { Width = 220, Height = 18, Visible = false, Margin = new Padding(3, 8, 3, 3) };
            _cancelJobButton = MakeButton("Cancel", (s, e) => _jobCancel = true);
            _cancelJobButton.Visible = false;
            flow.SetFlowBreak(flow.Controls[flow.Controls.Count - 1], true);
            flow.Controls.Add(_progress);
            flow.Controls.Add(_cancelJobButton);
            group.Controls.Add(flow);
            return group;
        }

        private ContextMenuStrip BuildListMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Show in Items / LandTiles tab", null, (s, e) => ShowInArtTab());
            menu.Items.Add("Show in TileData tab", null, (s, e) => ShowInTileData());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Copy id (hex)", null, (s, e) => CopyText(_selectedId >= 0 ? $"0x{_selectedId:X4}" : string.Empty));
            menu.Items.Add("Copy colour value", null, (s, e) => CopyText($"0x{CurrentColor:X4}"));
            menu.Items.Add("Copy colour as #RRGGBB", null, (s, e) => CopyText(_htmlBox.Text));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Set as range from", null, (s, e) => _fromBox.Text = _selectedId >= 0 ? $"0x{_selectedId:X}" : string.Empty);
            menu.Items.Add("Set as range to", null, (s, e) => _toBox.Text = _selectedId >= 0 ? $"0x{_selectedId:X}" : string.Empty);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Each selected = its own average", null, (s, e) => { _useSelection.Checked = true; GroupToOwnAverage(); });
            menu.Items.Add("Revert selected", null, (s, e) => RevertSelected());
            return menu;
        }

        private NumericUpDown MakeNumber()
        {
            var n = new NumericUpDown { Minimum = 0, Maximum = 255, Width = 52 };
            n.ValueChanged += (s, e) =>
            {
                if (_updating)
                {
                    return;
                }

                SetColorFromEditor(HueHelpers.ColorToHue(Color.FromArgb((int)_r.Value, (int)_g.Value, (int)_b.Value)));
            };
            return n;
        }

        private static Control Pair(string label, Control control)
        {
            var p = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            p.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 6, 0, 3) });
            p.Controls.Add(control);
            return p;
        }

        private static Button MakeButton(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            return b;
        }

        #endregion

        #region Load / list

        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
        }

        public void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (IsLoaded && (!(e is MyEventArgs args) || args.Type != MyEventArgs.Types.ForceReload))
            {
                return;
            }

            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

            if (IsLoaded)
            {
                // reload (file paths changed): the colours in memory come from the new files
                _originalItems.Clear();
                _originalLand.Clear();
            }
            else
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                UoExtensions.Changed += OnExtensionChanged;
                Disposed += (s, ev) =>
                {
                    ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
                    UoExtensions.Changed -= OnExtensionChanged;
                };
            }

            IsLoaded = true;
            RebuildList(false);
            UpdateButtons();
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnExtensionChanged(UoExtensionKind? kind)
        {
            if (!IsLoaded || (kind != null && kind != UoExtensionKind.Items && kind != UoExtensionKind.LandTiles))
            {
                return;
            }

            RadarCol.SyncExtensionSizes();
            RebuildList(true);
        }

        private int MaxId => _land ? Art.GetMaxLandId() : Art.GetMaxExtendedItemId();

        private int ExtendedBase => _land ? ExtendedFormat.LandBaseId : ExtendedFormat.BaseId;

        private Dictionary<int, ushort> Originals => _land ? _originalLand : _originalItems;

        private static bool IsLegal(int id, bool land) => land ? Art.IsLegalLandId(id) : Art.IsLegalStaticId(id);

        private static bool HasGraphic(int id, bool land) => land ? Art.IsValidLand(id) : Art.IsValidStatic(id);

        private static ushort GetColor(int id, bool land) => land ? RadarCol.GetLandColor(id) : RadarCol.GetItemColor(id);

        private static string GetName(int id, bool land)
        {
            if (land)
            {
                return TileData.LandTable != null && id < TileData.LandTable.Length ? TileData.LandTable[id].Name : string.Empty;
            }

            return TileData.ItemTable != null && id < TileData.ItemTable.Length ? TileData.ItemTable[id].Name : string.Empty;
        }

        /// <summary>Builds the id list for the current mode, filter and search (fast: ints only, no tree nodes).</summary>
        private void RebuildList(bool keepSelection)
        {
            if (!IsLoaded)
            {
                return;
            }

            int keep = keepSelection ? _selectedId : -1;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                int filter = _filterBox.SelectedIndex;
                string search = _searchBox.Text.Trim();
                bool searchIsId = TryParseId(search, out int searchId);
                Dictionary<int, ushort> originals = Originals;
                int max = MaxId;
                var ids = new List<int>(max + 1);
                for (int id = 0; id <= max; ++id)
                {
                    if (!IsLegal(id, _land))
                    {
                        continue; // gap before the item extension (0xFFDD..0xFFFF with High Seas)
                    }

                    switch (filter)
                    {
                        case 1 when !HasGraphic(id, _land):
                        case 2 when !originals.ContainsKey(id):
                        case 3 when GetColor(id, _land) != 0:
                        case 4 when GetColor(id, _land) != 0 || !HasGraphic(id, _land):
                        case 5 when id < ExtendedBase:
                            continue;
                    }

                    if (search.Length > 0 && !searchIsId)
                    {
                        string name = GetName(id, _land);
                        if (name == null || name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            continue;
                        }
                    }

                    ids.Add(id);
                }

                _ids = ids.ToArray();
                _list.VirtualListSize = _ids.Length;
                _list.Invalidate();
                _countLabel.Text = $"{_ids.Length:N0} {(_land ? "land tiles" : "items")}" +
                                   (originals.Count > 0 ? $"  |  {originals.Count:N0} changed" : string.Empty) +
                                   (_land ? $"  |  max 0x{max:X4}" : $"  |  max 0x{max:X}");

                if (searchIsId && search.Length > 0)
                {
                    keep = searchId;
                }

                // keep the selection only if it is in the new list (never reset the filter the user just chose)
                int keepRow = keep >= 0 ? Array.BinarySearch(_ids, keep) : -1;
                if (keepRow >= 0)
                {
                    SelectRow(keepRow);
                }
                else if (_ids.Length > 0)
                {
                    SelectRow(0);
                }
                else
                {
                    ShowEntry(-1);
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void JumpToSearch()
        {
            if (TryParseId(_searchBox.Text.Trim(), out int id))
            {
                SelectId(id, _land);
                return;
            }

            // next name match after the current row
            string search = _searchBox.Text.Trim();
            if (search.Length == 0 || _ids.Length == 0)
            {
                return;
            }

            int start = Math.Max(0, Array.BinarySearch(_ids, _selectedId) + 1);
            for (int k = 0; k < _ids.Length; ++k)
            {
                int row = (start + k) % _ids.Length;
                string name = GetName(_ids[row], _land);
                if (name != null && name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    SelectRow(row);
                    return;
                }
            }
        }

        private static bool TryParseId(string text, out int id)
        {
            id = -1;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id) && id >= 0;
            }

            return text.All(char.IsDigit) && int.TryParse(text, out id);
        }

        private void SetMode(bool land)
        {
            if (_land == land && IsLoaded)
            {
                return;
            }

            _land = land;
            _itemsButton.Checked = !land;
            _landButton.Checked = land;
            _selectedId = -1;
            RebuildList(false);
        }

        private void SelectId(int id, bool land)
        {
            if (_land != land)
            {
                _land = land;
                _itemsButton.Checked = !land;
                _landButton.Checked = land;
                _selectedId = id;
                RebuildList(true);
            }

            int row = Array.BinarySearch(_ids, id);
            if (row < 0 && (_filterBox.SelectedIndex != 0 || _searchBox.Text.Length > 0) && IsLegal(id, land) && id <= MaxId)
            {
                // not in the filtered list: show everything again
                _searchBox.Text = string.Empty;
                _searchTimer.Stop();
                _filterBox.SelectedIndex = 0; // rebuilds
                row = Array.BinarySearch(_ids, id);
            }

            if (row >= 0)
            {
                SelectRow(row);
            }
        }

        private void SelectRow(int row)
        {
            if (row < 0 || row >= _ids.Length)
            {
                return;
            }

            _list.SelectedIndices.Clear();
            _list.SelectedIndices.Add(row);
            _list.FocusedItem = _list.Items[row];
            _list.EnsureVisible(row);
            ShowEntry(_ids[row]);
        }

        private void OnRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int id = e.ItemIndex >= 0 && e.ItemIndex < _ids.Length ? _ids[e.ItemIndex] : 0;
            ushort c = GetColor(id, _land);
            Color rgb = HueHelpers.HueToColor(c);
            var item = new ListViewItem(string.Empty) { Tag = id };
            item.SubItems.Add(_land || id < 0x10000 ? $"0x{id:X4} ({id})" : $"0x{id:X5} ({id})");
            item.SubItems.Add(GetName(id, _land));
            item.SubItems.Add($"0x{c:X4}");
            item.SubItems.Add($"{rgb.R},{rgb.G},{rgb.B}");
            e.Item = item;
        }

        private void OnDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            int id = e.ItemIndex >= 0 && e.ItemIndex < _ids.Length ? _ids[e.ItemIndex] : 0;
            bool selected = e.Item.Selected;
            Color back = selected ? SystemColors.Highlight : (e.ItemIndex % 2 == 0 ? _list.BackColor : Color.FromArgb(246, 247, 249));
            Color fore = selected ? SystemColors.HighlightText : _list.ForeColor;
            if (!selected && Originals.ContainsKey(id))
            {
                fore = Color.MediumBlue; // changed, not saved
            }

            using (var brush = new SolidBrush(back))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            if (e.ColumnIndex == 0)
            {
                Rectangle r = Rectangle.Inflate(e.Bounds, -4, -3);
                using (var brush = new SolidBrush(HueHelpers.HueToColor(GetColor(id, _land))))
                {
                    e.Graphics.FillRectangle(brush, r);
                }

                e.Graphics.DrawRectangle(Pens.Gray, r);
                return;
            }

            Font font = e.ColumnIndex == 1 && Originals.ContainsKey(id) ? new Font(_list.Font, FontStyle.Bold) : _list.Font;
            try
            {
                TextRenderer.DrawText(e.Graphics, e.SubItem.Text, font, e.Bounds, fore,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
            finally
            {
                if (!ReferenceEquals(font, _list.Font))
                {
                    font.Dispose();
                }
            }
        }

        private void OnListSelectionChanged()
        {
            if (_list.FocusedItem != null && _list.SelectedIndices.Contains(_list.FocusedItem.Index))
            {
                ShowEntry(_ids[_list.FocusedItem.Index]);
            }
            else if (_list.SelectedIndices.Count > 0)
            {
                ShowEntry(_ids[_list.SelectedIndices[0]]);
            }

            UpdateButtons();
        }

        private void OnListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyToSelection();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                e.SuppressKeyPress = true;
                CopyText($"0x{CurrentColor:X4}");
            }
            else if (e.Control && e.KeyCode == Keys.Z)
            {
                e.SuppressKeyPress = true;
                RevertSelected();
            }
        }

        /// <summary>The ids of all selected rows (sorted).</summary>
        private List<int> SelectedIds()
        {
            var result = new List<int>(_list.SelectedIndices.Count);
            foreach (int row in _list.SelectedIndices)
            {
                if (row >= 0 && row < _ids.Length)
                {
                    result.Add(_ids[row]);
                }
            }

            result.Sort();
            return result;
        }

        private void RedrawId(int id)
        {
            int row = Array.BinarySearch(_ids, id);
            if (row >= 0)
            {
                _list.RedrawItems(row, row, true);
            }
        }

        #endregion

        #region Detail panel

        private void ShowEntry(int id)
        {
            _selectedId = id;
            if (id < 0)
            {
                _titleLabel.Text = "-";
                _infoLabel.Text = string.Empty;
                _previewSource = null;
                RenderPreview();
                return;
            }

            bool extended = id >= ExtendedBase;
            _titleLabel.Text = (_land ? "Land tile " : "Item ") + (id < 0x10000 ? $"0x{id:X4}" : $"0x{id:X5}") + $" ({id})  {GetName(id, _land)}";
            ushort fileColor = Originals.TryGetValue(id, out ushort original) ? original : GetColor(id, _land);
            _infoLabel.Text = (extended ? (_land ? "extended land tile - colour in radarcollandext.mul" : "extended item - colour in radarcolext.mul") : "radarcol.mul") +
                              (Originals.ContainsKey(id) ? $"   |   changed (file: 0x{original:X4})" : string.Empty);

            _swatchFile.BackColor = HueHelpers.HueToColor(fileColor);
            _previewSource = _land ? Art.GetLand(id) : Art.GetStatic(id);
            RenderPreview();

            _updating = true;
            try
            {
                CurrentColor = GetColor(id, _land);
            }
            finally
            {
                _updating = false;
            }

            if (_copyHexOnSelect.Checked)
            {
                CopyText($"0x{CurrentColor:X4}");
            }

            UpdateButtons();
        }

        private void ShowCurrentColor()
        {
            Color c = HueHelpers.HueToColor(_currentColor);
            bool old = _updating;
            _updating = true;
            try
            {
                _swatchCurrent.BackColor = c;
                _r.Value = c.R;
                _g.Value = c.G;
                _b.Value = c.B;
                _hexBox.Text = $"0x{_currentColor:X4}";
                _htmlBox.Text = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
            }
            finally
            {
                _updating = old;
            }
        }

        /// <summary>A colour edit (R/G/B, value, HTML, eyedropper, dialog): show it and - if wanted - apply it.</summary>
        private void SetColorFromEditor(ushort color)
        {
            CurrentColor = color;
            if (_autoApply.Checked)
            {
                ApplyToSelection();
            }
        }

        private void ParseHex()
        {
            if (_updating)
            {
                return;
            }

            string t = _hexBox.Text.Trim();
            if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                t = t.Substring(2);
            }

            if (int.TryParse(t, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int v) && v >= 0 && v <= 0xFFFF)
            {
                if ((ushort)(v & 0x7FFF) != _currentColor)
                {
                    SetColorFromEditor((ushort)v);
                }
            }
            else
            {
                ShowCurrentColor(); // invalid: show the current value again
            }
        }

        private void ParseHtml()
        {
            if (_updating)
            {
                return;
            }

            string t = _htmlBox.Text.Trim().TrimStart('#');
            if (t.Length == 6 && int.TryParse(t, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgb))
            {
                ushort v = HueHelpers.ColorToHue(Color.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF));
                if (v != _currentColor)
                {
                    SetColorFromEditor(v);
                }
            }
            else
            {
                ShowCurrentColor();
            }
        }

        private void PickWithDialog()
        {
            using var dlg = new ColorDialog { Color = HueHelpers.HueToColor(_currentColor), FullOpen = true };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                SetColorFromEditor(HueHelpers.ColorToHue(dlg.Color));
            }
        }

        private void DisposeOwnedImages()
        {
            if (_preview != null)
            {
                _preview.Image = null; // detach before dispose (never dispose an image a PictureBox still shows)
            }

            _previewImage?.Dispose();
            _previewImage = null;
        }

        /// <summary>Draws the graphic centred and (if needed) scaled down pixel exact; the Art bitmap itself is cached and not disposed.</summary>
        private void RenderPreview()
        {
            if (_preview == null || _preview.Width < 4 || _preview.Height < 4)
            {
                return;
            }

            Bitmap old = _previewImage;
            var img = new Bitmap(_preview.Width, _preview.Height);
            using (Graphics g = Graphics.FromImage(img))
            {
                g.Clear(_preview.BackColor);
                Bitmap src = _previewSource;
                if (src != null)
                {
                    float scale = Math.Min(1f, Math.Min((img.Width - 8f) / src.Width, (img.Height - 8f) / src.Height));
                    if (scale <= 0)
                    {
                        scale = 1f;
                    }

                    // small graphics: enlarge by whole numbers (pixel exact)
                    if (scale >= 1f)
                    {
                        scale = Math.Max(1, Math.Min(4, (int)Math.Min((img.Width - 8f) / src.Width, (img.Height - 8f) / src.Height)));
                    }

                    float w = src.Width * scale;
                    float h = src.Height * scale;
                    _previewRect = new RectangleF((img.Width - w) / 2f, (img.Height - h) / 2f, w, h);
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(src, _previewRect);
                }
                else
                {
                    _previewRect = RectangleF.Empty;
                    TextRenderer.DrawText(g, _selectedId >= 0 ? "no graphic" : string.Empty, Font, new Rectangle(Point.Empty, img.Size), Color.Silver,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            _preview.Image = img;
            _previewImage = img;
            old?.Dispose();
        }

        /// <summary>Eyedropper: takes the colour of the clicked pixel of the graphic.</summary>
        private void OnPreviewClick(object sender, MouseEventArgs e)
        {
            Bitmap src = _previewSource;
            if (src == null || _previewRect.IsEmpty || !_previewRect.Contains(e.Location))
            {
                return;
            }

            int x = (int)((e.X - _previewRect.X) * src.Width / _previewRect.Width);
            int y = (int)((e.Y - _previewRect.Y) * src.Height / _previewRect.Height);
            if (x < 0 || y < 0 || x >= src.Width || y >= src.Height)
            {
                return;
            }

            Color c = src.GetPixel(x, y);
            if (c.A == 0)
            {
                _statusLabel.Text = "Transparent pixel - nothing taken.";
                return;
            }

            SetColorFromEditor(HueHelpers.ColorToHue(c));
            _statusLabel.Text = $"Colour taken from pixel {x},{y}.";
        }

        private void UpdateButtons()
        {
            if (_revertButton == null)
            {
                return;
            }

            _revertButton.Enabled = SelectedIds().Any(id => Originals.ContainsKey(id));
            _revertAllButton.Enabled = _originalItems.Count > 0 || _originalLand.Count > 0;
            _applyButton.Enabled = _list.SelectedIndices.Count > 0;
        }

        #endregion

        #region Changing colours

        /// <summary>Sets one colour and remembers the colour of the file for revert.</summary>
        private void SetColor(int id, ushort color, bool land)
        {
            ushort now = GetColor(id, land);
            if (now == color)
            {
                return;
            }

            Dictionary<int, ushort> originals = land ? _originalLand : _originalItems;
            if (!originals.TryGetValue(id, out ushort original))
            {
                originals[id] = now;
            }
            else if (original == color)
            {
                originals.Remove(id); // back to the value in the file
            }

            if (land)
            {
                RadarCol.SetLandColor(id, color);
            }
            else
            {
                RadarCol.SetItemColor(id, color);
            }

            Options.ChangedUltimaClass[ChangeKey] = _originalItems.Count > 0 || _originalLand.Count > 0;
        }

        private void ApplyToSelection()
        {
            List<int> ids = SelectedIds();
            if (ids.Count == 0)
            {
                return;
            }

            foreach (int id in ids)
            {
                SetColor(id, _currentColor, _land);
            }

            AfterChange(ids);
        }

        private void AfterChange(IReadOnlyCollection<int> ids)
        {
            if (ids.Count <= 64)
            {
                foreach (int id in ids)
                {
                    RedrawId(id);
                }
            }
            else
            {
                _list.Invalidate();
            }

            if (_selectedId >= 0 && ids.Contains(_selectedId))
            {
                ushort fileColor = Originals.TryGetValue(_selectedId, out ushort o) ? o : GetColor(_selectedId, _land);
                _swatchFile.BackColor = HueHelpers.HueToColor(fileColor);
                _infoLabel.Text = (_selectedId >= ExtendedBase ? "extended - colour in the side-car file" : "radarcol.mul") +
                                  (Originals.ContainsKey(_selectedId) ? $"   |   changed (file: 0x{o:X4})" : string.Empty);
            }

            int changed = _originalItems.Count + _originalLand.Count;
            _countLabel.Text = $"{_ids.Length:N0} {(_land ? "land tiles" : "items")}" + (changed > 0 ? $"  |  {Originals.Count:N0} changed" : string.Empty);
            UpdateButtons();
        }

        private void RevertSelected()
        {
            List<int> ids = SelectedIds().Where(id => Originals.ContainsKey(id)).ToList();
            foreach (int id in ids)
            {
                SetColor(id, Originals[id], _land);
            }

            if (_selectedId >= 0)
            {
                _updating = true;
                try
                {
                    CurrentColor = GetColor(_selectedId, _land);
                }
                finally
                {
                    _updating = false;
                }
            }

            AfterChange(ids);
        }

        private void RevertAll()
        {
            if (MessageBox.Show(this, "Revert all changed radar colours (items and land tiles) to the values of the loaded files?", "Revert all",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            foreach (KeyValuePair<int, ushort> p in _originalItems.ToList())
            {
                RadarCol.SetItemColor(p.Key, p.Value);
            }

            foreach (KeyValuePair<int, ushort> p in _originalLand.ToList())
            {
                RadarCol.SetLandColor(p.Key, p.Value);
            }

            _originalItems.Clear();
            _originalLand.Clear();
            Options.ChangedUltimaClass[ChangeKey] = false;
            _list.Invalidate();
            ShowEntry(_selectedId);
            AfterChange(Array.Empty<int>());
        }

        private void AverageOfCurrent()
        {
            if (_selectedId < 0)
            {
                return;
            }

            if (!TryAverage(_land ? Art.GetLand(_selectedId) : Art.GetStatic(_selectedId), out Color c))
            {
                _statusLabel.Text = "No graphic (or only transparent pixels) - no average.";
                return;
            }

            SetColorFromEditor(HueHelpers.ColorToHue(c));
        }

        /// <summary>Average of all visible pixels (colour 0 = transparent is ignored). False for empty graphics.</summary>
        private static unsafe bool TryAverage(Bitmap image, out Color color)
        {
            color = Color.Black;
            if (image == null)
            {
                return false;
            }

            BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            long r = 0, g = 0, b = 0, count = 0;
            try
            {
                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                for (int y = 0; y < image.Height; ++y, line += delta)
                {
                    for (int x = 0; x < image.Width; ++x)
                    {
                        ushort p = line[x];
                        if ((p & 0x7FFF) == 0)
                        {
                            continue;
                        }

                        r += HueHelpers.HueToColorR(p);
                        g += HueHelpers.HueToColorG(p);
                        b += HueHelpers.HueToColorB(p);
                        ++count;
                    }
                }
            }
            finally
            {
                image.UnlockBits(bd);
            }

            if (count == 0)
            {
                return false;
            }

            color = Color.FromArgb((int)(r / count), (int)(g / count), (int)(b / count));
            return true;
        }

        /// <summary>The ids for the batch buttons: selected rows or the range (only legal ids).</summary>
        private List<int> GroupIds()
        {
            if (_useSelection.Checked)
            {
                List<int> sel = SelectedIds();
                if (sel.Count == 0)
                {
                    MessageBox.Show(this, "No rows selected (Shift / Ctrl + click selects several).", "Radar colours", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }

                return sel;
            }

            int max = MaxId;
            if (!Utils.ConvertStringToInt(_fromBox.Text.Trim(), out int from, 0, max) || !Utils.ConvertStringToInt(_toBox.Text.Trim(), out int to, 0, max))
            {
                MessageBox.Show(this, $"Range: enter two ids between 0 and 0x{max:X} (hex with 0x or decimal).", "Radar colours",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            if (from > to)
            {
                (from, to) = (to, from);
            }

            var ids = new List<int>(to - from + 1);
            for (int id = from; id <= to; ++id)
            {
                if (IsLegal(id, _land))
                {
                    ids.Add(id);
                }
            }

            return ids;
        }

        /// <summary>Average over the graphics (each graphic counts the same, empty ones are skipped).</summary>
        private bool GroupAverage(IEnumerable<int> ids, out ushort color, out int used)
        {
            long r = 0, g = 0, b = 0;
            used = 0;
            foreach (int id in ids)
            {
                if (TryAverage(_land ? Art.GetLand(id) : Art.GetStatic(id), out Color c))
                {
                    r += c.R;
                    g += c.G;
                    b += c.B;
                    ++used;
                }
            }

            color = used > 0 ? HueHelpers.ColorToHue(Color.FromArgb((int)(r / used), (int)(g / used), (int)(b / used))) : (ushort)0;
            return used > 0;
        }

        private void CurrentToGroupAverage()
        {
            List<int> ids = GroupIds();
            if (ids == null)
            {
                return;
            }

            if (!GroupAverage(ids, out ushort color, out int used))
            {
                _statusLabel.Text = "None of these entries has a graphic.";
                return;
            }

            SetColorFromEditor(color);
            _statusLabel.Text = $"Average of {used:N0} graphic(s).";
        }

        private void GroupToCommonAverage()
        {
            List<int> ids = GroupIds();
            if (ids == null)
            {
                return;
            }

            if (!GroupAverage(ids, out ushort color, out int used))
            {
                _statusLabel.Text = "None of these entries has a graphic.";
                return;
            }

            foreach (int id in ids)
            {
                SetColor(id, color, _land);
            }

            _statusLabel.Text = $"{ids.Count:N0} entr(y/ies) set to 0x{color:X4} (average of {used:N0} graphics).";
            AfterChange(ids);
            ShowEntry(_selectedId);
        }

        private void GroupToOwnAverage()
        {
            List<int> ids = GroupIds();
            if (ids == null)
            {
                return;
            }

            bool land = _land;
            StartJob($"Own average ({ids.Count:N0})", ids.Count, IterateOwnAverage(ids, land, false));
        }

        private void FillAllBlack()
        {
            if (MessageBox.Show(this,
                    "Calculate the radar colour from the graphic for every item AND land tile whose colour is black (0) or missing?" +
                    Environment.NewLine + "Entries with a colour are not changed. You can cancel at any time.",
                    "Fill black colours", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            var land = Enumerable.Range(0, Art.GetMaxLandId() + 1).Where(Art.IsLegalLandId).ToList();
            var items = Enumerable.Range(0, Art.GetMaxExtendedItemId() + 1).Where(Art.IsLegalStaticId).ToList();
            StartJob("Fill black colours", land.Count + items.Count,
                IterateOwnAverage(land, true, true).Concat(IterateOwnAverage(items, false, true)));
        }

        /// <summary>Sets every id to the average of its own graphic; yields the number of changed entries so far.</summary>
        private IEnumerable<int> IterateOwnAverage(List<int> ids, bool land, bool onlyBlack)
        {
            Dictionary<int, ushort> originals = land ? _originalLand : _originalItems;
            foreach (int id in ids)
            {
                if (!onlyBlack || GetColor(id, land) == 0)
                {
                    if (HasGraphic(id, land) && TryAverage(land ? Art.GetLand(id) : Art.GetStatic(id), out Color c))
                    {
                        SetColor(id, HueHelpers.ColorToHue(c), land);
                    }
                }

                yield return originals.Count;
            }
        }

        /// <summary>Runs a long job in small time slices on the UI thread (Art is not thread safe) with progress and cancel.</summary>
        private void StartJob(string name, int total, IEnumerable<int> steps)
        {
            if (_job != null)
            {
                MessageBox.Show(this, "A job is still running.", "Radar colours", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _jobName = name;
            _jobTotal = Math.Max(1, total);
            _jobDone = 0;
            _jobCancel = false;
            _job = steps.GetEnumerator();
            _progress.Maximum = _jobTotal;
            _progress.Value = 0;
            _progress.Visible = true;
            _cancelJobButton.Visible = true;
            _jobTimer ??= new Timer { Interval = 15 };
            _jobTimer.Tick -= OnJobTick;
            _jobTimer.Tick += OnJobTick;
            _jobTimer.Start();
        }

        private void OnJobTick(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();
            bool finished = false;
            while (sw.ElapsedMilliseconds < 60)
            {
                if (_jobCancel || !_job.MoveNext())
                {
                    finished = true;
                    break;
                }

                ++_jobDone;
            }

            _progress.Value = Math.Min(_progress.Maximum, _jobDone);
            _statusLabel.Text = $"{_jobName}: {_jobDone:N0} / {_jobTotal:N0}";
            if (!finished)
            {
                return;
            }

            _jobTimer.Stop();
            _job.Dispose();
            _job = null;
            _progress.Visible = false;
            _cancelJobButton.Visible = false;
            _statusLabel.Text = $"{_jobName}: {(_jobCancel ? "cancelled" : "done")} after {_jobDone:N0} entries. Changed now: {_originalItems.Count + _originalLand.Count:N0}.";
            _list.Invalidate();
            ShowEntry(_selectedId);
            AfterChange(Array.Empty<int>());
        }

        #endregion

        #region Files

        private void SaveFile()
        {
            string path = Options.OutputPath;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show(this, "The output folder is not set or does not exist (Options).", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fileName = Path.Combine(path, "radarcol.mul");
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                RadarCol.Save(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Saving failed: {ex.Message}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            _originalItems.Clear();
            _originalLand.Clear();
            Options.ChangedUltimaClass[ChangeKey] = false;
            _list.Invalidate();
            AfterChange(Array.Empty<int>());

            string ext = string.Empty;
            if (RadarCol.LandExtendedCount > 0)
            {
                ext += Environment.NewLine + Path.Combine(path, ExtendedFormat.RadarLandFileName) + $" ({RadarCol.LandExtendedCount:N0} land tiles)";
            }

            if (RadarCol.ItemExtendedCount > 0)
            {
                ext += Environment.NewLine + Path.Combine(path, ExtendedFormat.RadarItemFileName) + $" ({RadarCol.ItemExtendedCount:N0} items)";
            }

            MessageBox.Show(this, $"Saved:{Environment.NewLine}{fileName}{ext}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportCsv()
        {
            string fileName = Path.Combine(Options.OutputPath, "RadarColor.csv");
            try
            {
                RadarCol.ExportToCSV(fileName);
                MessageBox.Show(this, $"Exported to {fileName}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Export failed: {ex.Message}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportCsv()
        {
            using var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose csv file to import",
                CheckFileExists = true,
                Filter = "csv files (*.csv)|*.csv"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            // remember the file values for revert: snapshot of the current colours before the import
            ushort[] beforeLegacy = (ushort[])RadarCol.Colors.Clone();
            var beforeLand = Enumerable.Range(ExtendedFormat.LandBaseId, RadarCol.LandExtendedCount).ToDictionary(id => id, RadarCol.GetLandColor);
            var beforeItems = Enumerable.Range(ExtendedFormat.BaseId, RadarCol.ItemExtendedCount).ToDictionary(id => id, RadarCol.GetItemColor);
            try
            {
                RadarCol.ImportFromCSV(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Import failed: {ex.Message}", "Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int changed = 0;
            void Track(int id, bool land, ushort before)
            {
                if (GetColor(id, land) == before)
                {
                    return;
                }

                Dictionary<int, ushort> originals = land ? _originalLand : _originalItems;
                originals.TryAdd(id, before);
                ++changed;
            }

            for (int i = 0; i < beforeLegacy.Length; ++i)
            {
                if (i < RadarCol.LandCount)
                {
                    Track(i, true, beforeLegacy[i]);
                }
                else
                {
                    Track(i - RadarCol.LandCount, false, beforeLegacy[i]);
                }
            }

            foreach (KeyValuePair<int, ushort> p in beforeLand)
            {
                Track(p.Key, true, p.Value);
            }

            foreach (KeyValuePair<int, ushort> p in beforeItems)
            {
                Track(p.Key, false, p.Value);
            }

            Options.ChangedUltimaClass[ChangeKey] = _originalItems.Count > 0 || _originalLand.Count > 0;
            _list.Invalidate();
            ShowEntry(_selectedId);
            AfterChange(Array.Empty<int>());
            _statusLabel.Text = $"Import: {changed:N0} colour(s) changed.";
        }

        #endregion

        #region Navigation to other tabs

        private void ShowInArtTab()
        {
            if (_selectedId < 0)
            {
                return;
            }

            bool found = _land ? LandTilesControl.SearchGraphic(_selectedId) : ItemsControl.SearchGraphic(_selectedId);
            if (!found)
            {
                MessageBox.Show(this, $"0x{_selectedId:X} has no graphic (or the {(_land ? "LandTiles" : "Items")} tab is not loaded yet).", "Show",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_land)
            {
                LandTilesControl.ActivateTab();
            }
            else
            {
                ItemsControl.ActivateTab();
            }
        }

        private void ShowInTileData()
        {
            if (_selectedId < 0)
            {
                return;
            }

            TileDataControl.Select(_selectedId, _land);
            TileDataControl.ActivateTab();
        }

        private void CopyText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                Clipboard.SetText(text);
                _statusLabel.Text = $"Copied {text}";
            }
            catch (Exception)
            {
                // clipboard busy - ignore
            }
        }

        #endregion
    }
}
