/***************************************************************************
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Generates the other facings (north / east / south / west) of a drawn item - pixel exact, with the face
    /// light swapped and the shadow cast in the UO light direction. See <see cref="ArtDirectionGenerator"/>.
    /// </summary>
    public sealed class ArtDirectionForm : Form
    {
        private static readonly ArtDirection[] _allDirections = { ArtDirection.North, ArtDirection.East, ArtDirection.South, ArtDirection.West };

        private readonly Action<int, Bitmap> _insertCallback;

        // learning
        private readonly ArtDirectionRuleStore _rules =
            new ArtDirectionRuleStore(Path.Combine(Options.AppDataPath ?? Path.GetTempPath(), "ArtDirectionRules.json"));
        private CheckBox _autoDetectCheck;
        private CheckBox _learnOnInsertCheck;
        private Label _learnLabel;

        private ArtPixelImage _source;
        private int _sourceId = -1;
        private readonly Dictionary<ArtDirection, ArtDirectionResult> _results = new Dictionary<ArtDirection, ArtDirectionResult>();

        // options
        private ComboBox _sourceDirectionBox;
        private ComboBox _kindBox;
        private CheckBox _relightCheck;
        private TrackBar _relightStrength;
        private ComboBox _shadowBox;
        private NumericUpDown _shadowLength;
        private NumericUpDown _shadowAngle;
        private NumericUpDown _shadowLuma;
        private CheckBox _overlayCheck;
        private TextBox _searchBox;
        private CheckBox _subFolderCheck;
        private TextBox _subFolderName;
        private bool _imported;              // results come from an imported sprite sheet (not generated)
        private SpriteSheetInfo _lastSheet;  // layout of the last exported / imported sheet (for the AI prompt)
        private CheckBox _fieldCheck;
        private CheckBox _whiteTransparentCheck;
        private ComboBox _zoomBox;
        private Color _background = Color.Black; // UO: black = transparent
        private CheckBox _repairCheck;
        private NumericUpDown _repairHole;
        private ComboBox _exportBackgroundBox;

        // targets / preview
        private readonly Dictionary<ArtDirection, CheckBox> _targetChecks = new Dictionary<ArtDirection, CheckBox>();
        private readonly Dictionary<ArtDirection, PictureBox> _previews = new Dictionary<ArtDirection, PictureBox>();
        private readonly Dictionary<ArtDirection, TextBox> _idBoxes = new Dictionary<ArtDirection, TextBox>();
        private readonly Dictionary<ArtDirection, Label> _notes = new Dictionary<ArtDirection, Label>();
        private PictureBox _sourcePreview;
        private Label _sourceLabel;
        private string _sourceText = string.Empty;
        private Label _statusLabel;
        private bool _suspendUpdate;

        public ArtDirectionForm(Action<int, Bitmap> insertCallback)
        {
            _insertCallback = insertCallback;

            Text = "Art Directions - generate North / East / South / West";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1280, 760);
            MinimumSize = new Size(900, 560);
            KeyPreview = true;

            BuildUi();
            KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.V)
                {
                    LoadFromClipboard();
                    e.Handled = true;
                }
            };
        }

        #region UI
        private void BuildUi()
        {
            _suspendUpdate = true;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            // --- row 1: source + generate
            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            top.Controls.Add(MakeButton("Load image...", (s, e) => LoadFromFile()));
            top.Controls.Add(MakeButton("Paste (Ctrl+V)", (s, e) => LoadFromClipboard()));
            top.Controls.Add(MakeButton("Load item ID...", (s, e) => LoadFromItemId()));

            // browse through the arts and search by name / id / hex
            var prev = MakeButton("\u25c0", (s, e) => Browse(-1));
            var next = MakeButton("\u25b6", (s, e) => Browse(1));
            new ToolTip().SetToolTip(prev, "Previous art (item with a graphic)");
            new ToolTip().SetToolTip(next, "Next art (item with a graphic)");
            top.Controls.Add(prev);
            top.Controls.Add(next);
            _searchBox = new TextBox { Width = 120 };
            new ToolTip().SetToolTip(_searchBox, "Name (part of the TileData name), decimal id or hex (0x...). Enter = find (again = next match).");
            _searchBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    Search();
                    e.SuppressKeyPress = true;
                }
            };
            top.Controls.Add(_searchBox);
            top.Controls.Add(MakeButton("Find", (s, e) => Search()));
            _whiteTransparentCheck = new CheckBox { Text = "White = transparent (file/clipboard)", AutoSize = true, Checked = true, Margin = new Padding(8, 7, 3, 3) };
            top.Controls.Add(_whiteTransparentCheck);

            top.Controls.Add(MakeLabel("   Targets:"));
            foreach (ArtDirection d in _allDirections)
            {
                var cb = new CheckBox { Text = d.ToString(), AutoSize = true, Checked = true, Margin = new Padding(3, 7, 3, 3) };
                cb.CheckedChanged += (s, e) => Regenerate();
                _targetChecks[d] = cb;
                top.Controls.Add(cb);
            }

            top.Controls.Add(MakeButton("All", (s, e) =>
            {
                _suspendUpdate = true;
                foreach (CheckBox c in _targetChecks.Values)
                {
                    c.Checked = true;
                }

                _suspendUpdate = false;
                Regenerate();
            }));
            top.Controls.Add(MakeButton("Generate", (s, e) => { _imported = false; Regenerate(); }));
            root.Controls.Add(top, 0, 0);

            // --- row 2: options
            var opt = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };

            opt.Controls.Add(MakeLabel("Drawn direction:"));
            _sourceDirectionBox = MakeCombo(new object[] { ArtDirection.South, ArtDirection.East, ArtDirection.North, ArtDirection.West }, 0, 80);
            new ToolTip().SetToolTip(_sourceDirectionBox,
                "Where the drawn item points / looks to on screen:" + Environment.NewLine +
                "South = lower left,  East = lower right,  North = upper right,  West = upper left.");
            opt.Controls.Add(_sourceDirectionBox);

            opt.Controls.Add(MakeLabel("Object:"));
            _kindBox = MakeCombo(new object[]
            {
                "Wall / fence (front = back)",
                "Flat on the ground",
                "Solid object (template)",
                "Directional (cannon, chair...)",
                "Box / furniture (chest, crate)"
            }, 0, 190);
            new ToolTip().SetToolTip(_kindBox,
                "Wall / fence: north = south (front and back look the same)." + Environment.NewLine +
                "Flat: lies on the ground, turned 180°." + Environment.NewLine +
                "Solid: back side as shaded template to paint over." + Environment.NewLine +
                "Directional: things that point somewhere (cannon barrel, chair) - turned 180° on the ground." + Environment.NewLine +
                "Box / furniture: chest, crate, table - same footprint, north = south and west = east in the same place.");
            opt.Controls.Add(_kindBox);

            _relightCheck = new CheckBox { Text = "Swap face light", AutoSize = true, Checked = true, Margin = new Padding(10, 7, 3, 3) };
            _relightCheck.CheckedChanged += (s, e) => Regenerate();
            opt.Controls.Add(_relightCheck);
            _relightStrength = new TrackBar { Minimum = 0, Maximum = 100, Value = 100, TickFrequency = 25, Width = 110, AutoSize = false, Height = 28 };
            _relightStrength.ValueChanged += (s, e) => Regenerate();
            opt.Controls.Add(_relightStrength);

            opt.Controls.Add(MakeLabel("Shadow:"));
            _shadowBox = MakeCombo(new object[] { "Without shadow", "With shadow (UO light)", "Keep original shadow" }, 0, 160);
            opt.Controls.Add(_shadowBox);
            opt.Controls.Add(MakeLabel("Length"));
            _shadowLength = MakeNumeric(0.1m, 2.0m, 0.5m, 1, 0.1m);
            opt.Controls.Add(_shadowLength);
            opt.Controls.Add(MakeLabel("Angle°"));
            _shadowAngle = MakeNumeric(-45, 75, 14, 0, 1);
            opt.Controls.Add(_shadowAngle);
            opt.Controls.Add(MakeLabel("Shadow ≤ brightness"));
            _shadowLuma = MakeNumeric(10, 160, 50, 0, 5);
            opt.Controls.Add(_shadowLuma);

            _repairCheck = new CheckBox { Text = "Auto repair", AutoSize = true, Checked = true, Margin = new Padding(10, 7, 3, 3) };
            _repairCheck.CheckedChanged += (s, e) => Regenerate();
            opt.Controls.Add(_repairCheck);
            opt.Controls.Add(MakeLabel("holes ≤"));
            _repairHole = MakeNumeric(1, 200, 4, 0, 1);
            opt.Controls.Add(_repairHole);
            opt.Controls.Add(MakeButton("Repair source", (s, e) => RepairSource()));

            _fieldCheck = new CheckBox { Text = "UO field", AutoSize = true, Checked = true, Margin = new Padding(10, 7, 3, 3) };
            new ToolTip().SetToolTip(_fieldCheck,
                "Shows the UO tile the graphic stands on: the middle pixel of the lowest row (red) is the fix point" + Environment.NewLine +
                "on the bottom corner of the 44x44 diamond; dashed: centre line and ground line.");
            _fieldCheck.CheckedChanged += (s, e) => RefreshPreviews();
            opt.Controls.Add(_fieldCheck);

            _overlayCheck = new CheckBox { Text = "Show errors", AutoSize = true, Checked = true, Margin = new Padding(10, 7, 3, 3) };
            _overlayCheck.CheckedChanged += (s, e) => RefreshPreviews();
            opt.Controls.Add(_overlayCheck);

            opt.Controls.Add(MakeLabel("Zoom:"));
            _zoomBox = MakeCombo(new object[] { "1x", "2x", "3x", "4x" }, 1, 50);
            _zoomBox.SelectedIndexChanged -= OnOptionChanged;
            _zoomBox.SelectedIndexChanged += (s, e) => RefreshPreviews();
            opt.Controls.Add(_zoomBox);
            opt.Controls.Add(MakeLabel("Background:"));
            opt.Controls.Add(MakeButton("Black", (s, e) => SetBackground(Color.Black)));
            opt.Controls.Add(MakeButton("White", (s, e) => SetBackground(Color.White)));
            opt.Controls.Add(MakeButton("...", (s, e) =>
            {
                using var dlg = new ColorDialog { Color = _background };
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    SetBackground(dlg.Color);
                }
            }));
            opt.Controls.Add(MakeLabel("Copy/PNG background:"));
            _exportBackgroundBox = MakeCombo(new object[] { "Black (UO)", "Transparent" }, 0, 100);
            _exportBackgroundBox.SelectedIndexChanged -= OnOptionChanged;
            opt.Controls.Add(_exportBackgroundBox);

            // --- learning: auto detect, remembered corrections, similar existing items
            _autoDetectCheck = new CheckBox { Text = "Auto detect / learned", AutoSize = true, Checked = true, Margin = new Padding(10, 7, 3, 3) };
            new ToolTip().SetToolTip(_autoDetectCheck,
                "When an image is loaded: use a remembered correction for similar items, otherwise detect object type and" + Environment.NewLine +
                "drawn direction from the TileData flags (wall, door, surface ...) and the shape.");
            opt.Controls.Add(_autoDetectCheck);
            opt.Controls.Add(MakeButton("Detect now", (s, e) =>
            {
                AutoConfigure(true);
                FillFreeIds();
                Regenerate();
            }));
            opt.Controls.Add(MakeButton("Remember settings", (s, e) => RememberCurrent(false)));
            _learnOnInsertCheck = new CheckBox { Text = "Learn from inserts", AutoSize = true, Checked = true, Margin = new Padding(6, 7, 3, 3) };
            new ToolTip().SetToolTip(_learnOnInsertCheck, "Inserting into Art also remembers the current settings for similar items.");
            opt.Controls.Add(_learnOnInsertCheck);
            opt.Controls.Add(MakeButton("Similar existing items...", (s, e) => ShowSimilar()));
            _learnLabel = new Label { AutoSize = true, ForeColor = Color.DarkSlateBlue, Margin = new Padding(10, 8, 3, 3) };
            opt.Controls.Add(_learnLabel);
            root.Controls.Add(opt, 0, 1);

            // --- row 3: previews
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1 };
            for (int i = 0; i < 5; ++i)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            }

            grid.Controls.Add(BuildSourcePanel(), 0, 0);
            int col = 1;
            foreach (ArtDirection d in _allDirections)
            {
                grid.Controls.Add(BuildTargetPanel(d), col++, 0);
            }

            root.Controls.Add(grid, 0, 2);

            // --- row 4: insert all + status
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            bottom.Controls.Add(MakeButton("Next free IDs", (s, e) => FillFreeIds()));
            bottom.Controls.Add(MakeButton("4 free slots", (s, e) => FindFourFreeSlots()));
            var place = MakeButton("Place...", (s, e) => OpenPlacement());
            new ToolTip().SetToolTip(place, "Choose the slots of the directions: from an address, free blocks (browse with arrows), the selection in the Items tab or by hand");
            bottom.Controls.Add(place);
            bottom.Controls.Add(MakeButton("Insert all checked into Art", (s, e) => InsertAll()));
            bottom.Controls.Add(MakeLabel("|"));
            bottom.Controls.Add(MakeButton("Save PNGs", (s, e) => SavePngs(false)));
            bottom.Controls.Add(MakeButton("Folder...", (s, e) => SavePngs(true)));
            _subFolderCheck = new CheckBox { Text = "new folder:", AutoSize = true, Checked = true, Margin = new Padding(6, 7, 0, 3) };
            bottom.Controls.Add(_subFolderCheck);
            _subFolderName = new TextBox { Width = 140 };
            new ToolTip().SetToolTip(_subFolderName, "Name of the new folder inside the export folder (empty = item name + id)");
            bottom.Controls.Add(_subFolderName);
            bottom.Controls.Add(MakeLabel("|"));
            bottom.Controls.Add(MakeButton("Export sprite sheet...", (s, e) => ExportSheet()));
            bottom.Controls.Add(MakeButton("Import sprite sheet...", (s, e) => ImportSheet()));
            bottom.Controls.Add(MakeButton("Copy AI prompt", (s, e) => CopyAiPrompt()));
            _statusLabel = MakeLabel("Load an image (or select an item in the Items tab and open this window).");
            _statusLabel.AutoSize = true;
            bottom.Controls.Add(_statusLabel);
            root.Controls.Add(bottom, 0, 3);

            _suspendUpdate = false;
        }

        private Control BuildSourcePanel()
        {
            var group = new GroupBox { Text = "Source", Dock = DockStyle.Fill };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _sourcePreview = MakePreview();
            _sourcePreview.ContextMenuStrip = BuildImageMenu(() => _source, null);
            layout.Controls.Add(WrapScroll(_sourcePreview), 0, 0);
            _sourceLabel = MakeLabel("-");
            layout.Controls.Add(_sourceLabel, 0, 1);
            group.Controls.Add(layout);
            return group;
        }

        private Control BuildTargetPanel(ArtDirection d)
        {
            var group = new GroupBox { Text = d.ToString(), Dock = DockStyle.Fill };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            PictureBox pb = MakePreview();
            pb.ContextMenuStrip = BuildImageMenu(() => _results.TryGetValue(d, out ArtDirectionResult r) ? r.Image : null, d);
            _previews[d] = pb;
            layout.Controls.Add(WrapScroll(pb), 0, 0);

            var note = new Label { AutoSize = true, MaximumSize = new Size(230, 0), ForeColor = Color.DimGray };
            _notes[d] = note;
            layout.Controls.Add(note, 0, 1);

            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            buttons.Controls.Add(MakeLabel("ID:"));
            var idBox = new TextBox { Width = 70 };
            _idBoxes[d] = idBox;
            buttons.Controls.Add(idBox);
            buttons.Controls.Add(MakeButton("Insert", (s, e) => InsertOne(d, true)));
            buttons.Controls.Add(MakeButton("Copy", (s, e) => CopyOne(d)));
            buttons.Controls.Add(MakeButton("PNG...", (s, e) => SaveOne(d)));
            layout.Controls.Add(buttons, 0, 2);
            layout.Controls.Add(BuildManualRow(d), 0, 3);

            group.Controls.Add(layout);
            return group;
        }

        #region Manual orientation per direction
        private sealed class ManualControls
        {
            public ComboBox Method;
            public NumericUpDown Axis;
            public NumericUpDown Dx;
            public NumericUpDown Dy;
        }

        private readonly Dictionary<ArtDirection, ManualControls> _manual = new Dictionary<ArtDirection, ManualControls>();

        /// <summary>
        /// Manual orientation for one direction: method (copy / mirror / flip / 180 / turn across the cross axis /
        /// turn by column shift), axis slope for the turns (0 = measured) and a pixel offset.
        /// </summary>
        private Control BuildManualRow(ArtDirection d)
        {
            var row = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
            var mc = new ManualControls
            {
                Method = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 },
                Axis = new NumericUpDown { Minimum = -3, Maximum = 3, DecimalPlaces = 2, Increment = 0.05m, Width = 52 },
                Dx = new NumericUpDown { Minimum = -200, Maximum = 200, Width = 46 },
                Dy = new NumericUpDown { Minimum = -200, Maximum = 200, Width = 46 }
            };
            mc.Method.Items.AddRange(new object[]
            {
                "Auto", "Copy", "Mirror \u2194", "Flip \u2195", "Rotate 180\u00b0", "Turn (cross axis)", "Turn (column shift)"
            });
            mc.Method.SelectedIndex = 0;

            var tip = new ToolTip();
            tip.SetToolTip(mc.Method,
                "Auto: what the object type does." + Environment.NewLine +
                "Copy: same as the drawn image.   Mirror: left <-> right." + Environment.NewLine +
                "Flip: upside down.   Rotate 180: turned in the picture." + Environment.NewLine +
                "Turn (cross axis): points the other way, round parts stay round." + Environment.NewLine +
                "Turn (column shift): points the other way, vertical parts stay vertical.");
            tip.SetToolTip(mc.Axis, "Axis slope for the turns: 0 = measured, 1 = \\ UO tile diagonal (45 degrees), -1 = / diagonal");
            tip.SetToolTip(mc.Dx, "Move the result left (-) / right (+) in pixels");
            tip.SetToolTip(mc.Dy, "Move the result up (-) / down (+) in pixels");

            mc.Method.SelectedIndexChanged += (s, e) => Regenerate();
            mc.Axis.ValueChanged += (s, e) => Regenerate();
            mc.Dx.ValueChanged += (s, e) => Regenerate();
            mc.Dy.ValueChanged += (s, e) => Regenerate();

            row.Controls.Add(MakeLabel("Manual:"));
            row.Controls.Add(mc.Method);
            row.Controls.Add(MakeLabel("Axis"));
            row.Controls.Add(mc.Axis);
            row.Controls.Add(MakeLabel("X"));
            row.Controls.Add(mc.Dx);
            row.Controls.Add(MakeLabel("Y"));
            row.Controls.Add(mc.Dy);
            row.Controls.Add(MakeButton("Reset", (s, e) =>
            {
                _suspendUpdate = true;
                mc.Method.SelectedIndex = 0;
                mc.Axis.Value = 0;
                mc.Dx.Value = 0;
                mc.Dy.Value = 0;
                _suspendUpdate = false;
                Regenerate();
            }));

            _manual[d] = mc;
            return row;
        }
        #endregion

        private static Panel WrapScroll(Control c)
        {
            var p = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            p.Controls.Add(c);
            return p;
        }

        private static PictureBox MakePreview() => new PictureBox { SizeMode = PictureBoxSizeMode.AutoSize, Location = new Point(0, 0) };

        private static Button MakeButton(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            return b;
        }

        private static Label MakeLabel(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(3, 8, 3, 3) };

        private ComboBox MakeCombo(object[] items, int selected, int width)
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = width };
            c.Items.AddRange(items);
            c.SelectedIndex = selected;
            c.SelectedIndexChanged += OnOptionChanged;
            return c;
        }

        private NumericUpDown MakeNumeric(decimal min, decimal max, decimal value, int decimals, decimal inc)
        {
            var n = new NumericUpDown { Minimum = min, Maximum = max, Value = value, DecimalPlaces = decimals, Increment = inc, Width = 55 };
            n.ValueChanged += OnOptionChanged;
            return n;
        }

        private void OnOptionChanged(object sender, EventArgs e) => Regenerate();
        #endregion

        #region Loading
        /// <summary>Sets the source image (called by the Items tab with the selected item).</summary>
        public void LoadSource(Bitmap bitmap, int itemId)
        {
            _imported = false;
            if (bitmap == null)
            {
                return;
            }

            _source = FromBitmap(bitmap, false);
            _sourceId = itemId;
            _sourceText = itemId >= 0
                ? $"Item 0x{itemId:X4} ({bitmap.Width}x{bitmap.Height})"
                : $"{bitmap.Width}x{bitmap.Height}";
            AutoConfigure();
            FillFreeIds();
            Regenerate();
        }

        private void LoadFromFile()
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Choose image",
                Filter = "Image files (*.png;*.bmp;*.tif;*.tiff;*.gif)|*.png;*.bmp;*.tif;*.tiff;*.gif"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using var tmp = new Bitmap(dlg.FileName);
            _source = FromBitmap(tmp, _whiteTransparentCheck.Checked);
            _sourceId = -1;
            _sourceText = $"{Path.GetFileName(dlg.FileName)} ({tmp.Width}x{tmp.Height})";
            AutoConfigure();
            FillFreeIds();
            Regenerate();
        }

        private void LoadFromClipboard()
        {
            if (!Clipboard.ContainsImage())
            {
                SetStatus("No image in the clipboard.");
                return;
            }

            using Image img = Clipboard.GetImage();
            using var tmp = new Bitmap(img);
            _source = FromBitmap(tmp, _whiteTransparentCheck.Checked);
            _sourceId = -1;
            _sourceText = $"Clipboard ({tmp.Width}x{tmp.Height})";
            AutoConfigure();
            FillFreeIds();
            Regenerate();
        }

        private void LoadFromItemId()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Item ID (hex 0x... or decimal):", "Load item", _sourceId >= 0 ? $"0x{_sourceId:X}" : "0x");
            if (!Utils.ConvertStringToInt(input, out int id, 0, Art.GetMaxExtendedItemId()) || !Art.IsValidStatic(id))
            {
                if (!string.IsNullOrWhiteSpace(input))
                {
                    SetStatus("Invalid or empty item ID.");
                }

                return;
            }

            LoadSource(Art.GetStatic(id), id);
        }

        /// <summary>Converts a bitmap to art pixels. Transparent: no alpha, pure black (UO) and optionally white.</summary>
        internal static unsafe ArtPixelImage FromBitmap(Bitmap bmp, bool whiteTransparent)
        {
            var img = new ArtPixelImage(bmp.Width, bmp.Height);
            using Bitmap copy = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format16bppArgb1555);
            BitmapData bd = copy.LockBits(new Rectangle(0, 0, copy.Width, copy.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            try
            {
                for (int y = 0; y < img.Height; ++y)
                {
                    var line = (ushort*)((byte*)bd.Scan0 + (y * bd.Stride));
                    for (int x = 0; x < img.Width; ++x)
                    {
                        ushort c = line[x];
                        int rgb = c & 0x7FFF;
                        bool transparent = (c & 0x8000) == 0 || rgb == 0 || (whiteTransparent && rgb == 0x7FFF);
                        img[x, y] = transparent ? (ushort)0 : (ushort)(c | 0x8000);
                    }
                }
            }
            finally
            {
                copy.UnlockBits(bd);
            }

            return img;
        }

        internal static unsafe Bitmap ToBitmap(ArtPixelImage img)
        {
            var bmp = new Bitmap(Math.Max(1, img.Width), Math.Max(1, img.Height), PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            try
            {
                for (int y = 0; y < img.Height; ++y)
                {
                    var line = (ushort*)((byte*)bd.Scan0 + (y * bd.Stride));
                    for (int x = 0; x < img.Width; ++x)
                    {
                        ushort c = img[x, y];
                        line[x] = c == 0 ? (ushort)0 : (ushort)(c | 0x8000);
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            return bmp;
        }
        #endregion

        #region Learning
        /// <summary>
        /// Sets object type, drawn direction and the manual rows for the loaded source: a remembered rule of a
        /// similar item wins, otherwise TileData flags + shape (<see cref="ArtDirectionLearning.Suggest"/>).
        /// </summary>
        private void AutoConfigure(bool force = false)
        {
            if (_source == null || (!force && (_autoDetectCheck == null || !_autoDetectCheck.Checked)))
            {
                return;
            }

            bool old = _suspendUpdate;
            _suspendUpdate = true;
            try
            {
                ArtFeatures features = ArtFeatures.Compute(_source, _sourceId);
                ArtDirectionRule rule = _rules.Match(features, _sourceId, out double distance);
                if (rule != null)
                {
                    _kindBox.SelectedIndex = Math.Clamp(rule.Kind, 0, _kindBox.Items.Count - 1);
                    _sourceDirectionBox.SelectedItem = (ArtDirection)rule.Direction;
                    if (rule.RelightStrength >= 0)
                    {
                        _relightCheck.Checked = rule.RelightStrength > 0;
                        _relightStrength.Value = Math.Clamp(rule.RelightStrength, 0, 100);
                    }
                    foreach (ArtDirection d in _allDirections)
                    {
                        if (_manual.TryGetValue(d, out ManualControls mc))
                        {
                            int i = (int)d;
                            mc.Method.SelectedIndex = Math.Clamp(rule.Method[i], 0, mc.Method.Items.Count - 1);
                            mc.Axis.Value = Math.Clamp((decimal)rule.Axis[i], mc.Axis.Minimum, mc.Axis.Maximum);
                            mc.Dx.Value = Math.Clamp(rule.Dx[i], (int)mc.Dx.Minimum, (int)mc.Dx.Maximum);
                            mc.Dy.Value = Math.Clamp(rule.Dy[i], (int)mc.Dy.Minimum, (int)mc.Dy.Maximum);
                        }
                    }

                    rule.Uses++;
                    _rules.Save();
                    _learnLabel.Text = $"Learned: settings of '{rule.Name}' applied ({ArtFeatures.Similarity(distance)}% similar).";
                    return;
                }

                bool has = _sourceId >= 0 && TileData.ItemTable != null && _sourceId < TileData.ItemTable.Length;
                ItemData data = has ? TileData.ItemTable[_sourceId] : default;
                ArtSuggestion s = ArtDirectionLearning.Suggest(_source, has,
                    has && (data.Flags & TileFlag.Wall) != 0, has && (data.Flags & TileFlag.Door) != 0,
                    has && (data.Flags & TileFlag.Window) != 0, has && (data.Flags & TileFlag.Surface) != 0,
                    has && (data.Flags & TileFlag.Foliage) != 0, has ? data.Height : 1,
                    has && (data.Flags & TileFlag.Container) != 0);

                _kindBox.SelectedIndex = (int)s.Kind;
                _sourceDirectionBox.SelectedItem = s.Direction;
                ResetManualRows();
                _learnLabel.Text = $"Auto: {_kindBox.SelectedItem}, drawn {s.Direction} ({s.Reason}, {s.Confidence:P0}).";
            }
            finally
            {
                _suspendUpdate = old;
            }
        }

        private void ResetManualRows()
        {
            foreach (ManualControls mc in _manual.Values)
            {
                mc.Method.SelectedIndex = 0;
                mc.Axis.Value = 0;
                mc.Dx.Value = 0;
                mc.Dy.Value = 0;
            }
        }

        /// <summary>Stores the current settings as a rule for this and similar items.</summary>
        private void RememberCurrent(bool silent)
        {
            if (_source == null)
            {
                return;
            }

            var rule = new ArtDirectionRule
            {
                Name = !string.IsNullOrWhiteSpace(_sourceText) ? _sourceText : "image",
                SourceId = _sourceId,
                Kind = _kindBox.SelectedIndex,
                Direction = (int)SourceDirection,
                RelightStrength = _relightCheck.Checked ? _relightStrength.Value : 0
            };
            foreach (ArtDirection d in _allDirections)
            {
                if (_manual.TryGetValue(d, out ManualControls mc))
                {
                    int i = (int)d;
                    rule.Method[i] = mc.Method.SelectedIndex;
                    rule.Axis[i] = (double)mc.Axis.Value;
                    rule.Dx[i] = (int)mc.Dx.Value;
                    rule.Dy[i] = (int)mc.Dy.Value;
                }
            }

            rule.SetFeatures(ArtFeatures.Compute(_source, _sourceId));
            try
            {
                _rules.Remember(rule);
                _learnLabel.Text = $"Remembered for similar items ({_rules.Rules.Count} rules learned).";
                if (!silent)
                {
                    SetStatus("Settings remembered - similar items get them automatically.");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Could not save the learned rules: {ex.Message}");
            }
        }

        private void ShowSimilar()
        {
            if (_source == null)
            {
                SetStatus("Load an image first.");
                return;
            }

            new ArtSimilarItemsForm(_source, _sourceId,
                (kind, relight, why) =>
                {
                    _suspendUpdate = true;
                    _kindBox.SelectedIndex = (int)kind;
                    if (relight >= 0)
                    {
                        _relightCheck.Checked = relight > 0;
                        _relightStrength.Value = (int)Math.Round(relight * 100);
                    }

                    _suspendUpdate = false;
                    _learnLabel.Text = $"Object type {_kindBox.SelectedItem}" + (relight >= 0 ? $", light swap {relight:P0}" : string.Empty) + $" {why}.";
                    Regenerate();
                },
                id =>
                {
                    if (Art.IsValidStatic(id))
                    {
                        LoadSource(Art.GetStatic(id), id);
                        Activate();
                    }
                }).Show(this);
        }
        #endregion

        #region Generate / preview
        private ArtDirectionOptions CurrentOptions()
        {
            return new ArtDirectionOptions
            {
                Kind = (ArtObjectKind)Math.Max(0, _kindBox.SelectedIndex),
                Relight = _relightCheck.Checked,
                RelightStrength = _relightStrength.Value / 100.0,
                Shadow = _shadowBox.SelectedIndex switch
                {
                    1 => ArtShadowMode.AlwaysCast, // with shadow: drawn shadow is replaced, otherwise one is cast
                    2 => ArtShadowMode.KeepAsIs,
                    _ => ArtShadowMode.Remove      // without shadow: a drawn shadow is removed
                },
                ShadowLength = (double)_shadowLength.Value,
                ShadowAngle = (double)_shadowAngle.Value,
                ShadowMaxLuma = (int)_shadowLuma.Value,
                Repair = _repairCheck.Checked,
                RepairMaxHole = (int)_repairHole.Value
            };
        }

        private ArtDirection SourceDirection => (ArtDirection)(_sourceDirectionBox.SelectedItem ?? ArtDirection.South);

        private void Regenerate()
        {
            if (_suspendUpdate || _source == null)
            {
                return;
            }

            if (_imported)
            {
                RefreshPreviews();
                SetStatus("Showing the imported sprite sheet - press 'Generate' to compute the directions again.");
                return;
            }

            ArtDirectionOptions options = CurrentOptions();
            _results.Clear();
            foreach (ArtDirection d in _allDirections)
            {
                if (!_targetChecks[d].Checked)
                {
                    continue;
                }

                ArtDirectionResult r;
                if (_manual.TryGetValue(d, out ManualControls mc) && mc.Method.SelectedIndex > 0)
                {
                    r = ArtDirectionGenerator.GenerateManual(_source, d, (ArtManualMethod)mc.Method.SelectedIndex, (double)mc.Axis.Value, options);
                }
                else
                {
                    r = ArtDirectionGenerator.Generate(_source, SourceDirection, d, options);
                }

                if (mc != null && (mc.Dx.Value != 0 || mc.Dy.Value != 0))
                {
                    r.Image = ArtDirectionGenerator.Translate(r.Image, (int)mc.Dx.Value, (int)mc.Dy.Value);
                    r.Note += $" Moved {mc.Dx.Value:+0;-0;0}/{mc.Dy.Value:+0;-0;0} px.";
                }

                _results[d] = r;
            }

            RefreshPreviews();
            SetStatus($"Generated {_results.Count} direction(s). Drawn direction: {SourceDirection}.");
        }

        private void RefreshPreviews()
        {
            if (_source == null)
            {
                return;
            }

            int zoom = Math.Max(1, _zoomBox.SelectedIndex + 1);
            ArtDirectionOptions options = CurrentOptions();

            SetPreview(_sourcePreview, _source, zoom);
            _sourceLabel.Text = _sourceText + Environment.NewLine + ErrorText(_source, out _);

            foreach (ArtDirection d in _allDirections)
            {
                if (_results.TryGetValue(d, out ArtDirectionResult r))
                {
                    SetPreview(_previews[d], r.Image, zoom);
                    string errors = ErrorText(r.Image, out bool hasErrors);
                    _notes[d].Text = (d == SourceDirection ? "(drawn direction) " : string.Empty) + r.Note + Environment.NewLine + errors;
                    _notes[d].ForeColor = hasErrors ? Color.Firebrick : r.IsExact ? Color.DimGray : Color.DarkOrange;
                }
                else
                {
                    SetPreview(_previews[d], null, zoom);
                    _notes[d].Text = _targetChecks[d].Checked ? string.Empty : "(not selected)";
                }
            }
        }

        /// <summary>Main UO tile under the graphic (see UoTile): diamond, centre line, ground line, fix point.</summary>
        private void DrawUoField(Graphics g, int w, int h, int zoom)
        {
            (int tx, int ty) = UoTile.TileOrigin(w, h);
            (int X, int Y)[] d = UoTile.Diamond(tx, ty);
            using var pen = new Pen(Color.FromArgb(200, 255, 200, 40), Math.Max(1, zoom / 2f));
            g.DrawPolygon(pen, d.Select(c => new PointF(c.X * zoom, c.Y * zoom)).ToArray());
            using var help = new Pen(Color.FromArgb(120, 255, 200, 40)) { DashStyle = DashStyle.Dash };
            g.DrawLine(help, (tx + UoTile.Half) * zoom, 0, (tx + UoTile.Half) * zoom, h * zoom);
            g.DrawLine(help, 0, (ty + UoTile.Size) * zoom - 1, w * zoom, (ty + UoTile.Size) * zoom - 1);
            (int ax, int ay) = UoTile.Anchor(w, h);
            g.DrawRectangle(Pens.Red, ax * zoom, ay * zoom, Math.Max(1, zoom - 1), Math.Max(1, zoom - 1));
        }

        private string ErrorText(ArtPixelImage img, out bool hasErrors)
        {
            ArtDirectionGenerator.FindErrors(img, (int)_repairHole.Value, out int holes, out int strays, out int blacks, out int white);

            // see-through pixels the source already has (glass, gaps between spindles) are part of the art
            int sourceHoles = 0;
            if (_source != null)
            {
                ArtDirectionGenerator.FindErrors(_source, (int)_repairHole.Value, out sourceHoles, out _, out _, out _);
            }

            bool holesWanted = sourceHoles > 0;
            hasErrors = (holesWanted ? 0 : holes) + strays + blacks + white > 0;
            if (!hasErrors && holes > 0)
            {
                return $"{img.Width}x{img.Height} - no errors ({holes} see-through px as in the source)";
            }
            if (!hasErrors)
            {
                return $"{img.Width}x{img.Height} - no errors";
            }

            var parts = new List<string>();
            if (holes > 0) { parts.Add($"{holes} hole px"); }
            if (strays > 0) { parts.Add($"{strays} stray"); }
            if (blacks > 0) { parts.Add($"{blacks} black"); }
            if (white > 0) { parts.Add($"{white} white edge"); }
            return $"{img.Width}x{img.Height} - errors: " + string.Join(", ", parts);
        }

        private void SetBackground(Color color)
        {
            _background = color;
            RefreshPreviews();
        }

        private void SetPreview(PictureBox pb, ArtPixelImage img, int zoom)
        {
            ArtDirectionGenerator.ArtError[] overlay = img != null && _overlayCheck.Checked
                ? ArtDirectionGenerator.FindErrors(img, (int)_repairHole.Value, out _, out _, out _, out _)
                : null;

            Image old = pb.Image;
            if (img == null)
            {
                pb.Image = null;
                old?.Dispose();
                return;
            }

            using Bitmap art = ToBitmap(img);
            var view = new Bitmap(img.Width * zoom, img.Height * zoom);
            using (Graphics g = Graphics.FromImage(view))
            {
                g.Clear(_background);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(art, new Rectangle(0, 0, view.Width, view.Height));

                if (overlay != null)
                {
                    for (int y = 0; y < img.Height; ++y)
                    {
                        for (int x = 0; x < img.Width; ++x)
                        {
                            ArtDirectionGenerator.ArtError e = overlay[(y * img.Width) + x];
                            Color? tint = e == ArtDirectionGenerator.ArtError.None ? null
                                : (e & ArtDirectionGenerator.ArtError.Hole) != 0 ? Color.Magenta
                                : (e & ArtDirectionGenerator.ArtError.Black) != 0 ? Color.Red
                                : (e & ArtDirectionGenerator.ArtError.Stray) != 0 ? Color.Cyan
                                : Color.Orange; // white edge

                            if (tint.HasValue)
                            {
                                using var brush = new SolidBrush(tint.Value);
                                g.FillRectangle(brush, x * zoom, y * zoom, zoom, zoom);
                            }
                        }
                    }
                }

                if (_fieldCheck == null || _fieldCheck.Checked)
                {
                    DrawUoField(g, img.Width, img.Height, zoom);
                }
            }

            pb.Image = view;
            old?.Dispose();
        }
        #endregion

        #region Browse / search / free slots / export / sprite sheet
        private bool IsSourceSlot(ArtDirection d)
        {
            return d == SourceDirection && _sourceId >= 0 &&
                   Utils.ConvertStringToInt(_idBoxes[d].Text, out int id, 0, Art.GetMaxExtendedItemId()) && id == _sourceId;
        }

        private void LoadItem(int id)
        {
            _imported = false;
            LoadSource(Art.GetStatic(id), id);
        }

        /// <summary>Previous / next item that has a graphic (legacy range and extension).</summary>
        private void Browse(int step)
        {
            int max = Art.GetMaxExtendedItemId();
            int id = _sourceId >= 0 ? _sourceId : (step > 0 ? -1 : max + 1);
            for (int guard = 0; guard <= max + 1; ++guard)
            {
                id += step;
                if (id < 0 || id > max)
                {
                    SetStatus(step > 0 ? "Last art reached." : "First art reached.");
                    return;
                }

                if (Art.IsLegalStaticId(id) && Art.IsValidStatic(id))
                {
                    LoadItem(id);
                    return;
                }
            }
        }

        /// <summary>Decimal / hex id loads that item; any other text searches the TileData names (next match).</summary>
        private void Search()
        {
            string text = (_searchBox.Text ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                return;
            }

            bool isNumber = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || text.All(char.IsDigit);
            int max = Art.GetMaxExtendedItemId();
            if (isNumber)
            {
                if (Utils.ConvertStringToInt(text, out int id, 0, max) && Art.IsLegalStaticId(id) && Art.IsValidStatic(id))
                {
                    LoadItem(id);
                }
                else
                {
                    SetStatus($"'{text}' is no art with a graphic.");
                }

                return;
            }

            int start = _sourceId >= 0 ? _sourceId : -1;
            for (int n = 1; n <= max + 1; ++n)
            {
                int id = (start + n) % (max + 1);
                if (!Art.IsLegalStaticId(id) || TileData.ItemTable == null || id >= TileData.ItemTable.Length)
                {
                    continue;
                }

                string name = TileData.ItemTable[id].Name;
                if (!string.IsNullOrEmpty(name) && name.Contains(text, StringComparison.OrdinalIgnoreCase) && Art.IsValidStatic(id))
                {
                    LoadItem(id);
                    SetStatus($"Found '{name.Trim()}' 0x{id:X4} (Find again = next match).");
                    return;
                }
            }

            SetStatus($"No art named '{text}'.");
        }

        /// <summary>Four free ids in a row (after the source) -> North, East, South, West, so all four get new slots.</summary>
        private void FindFourFreeSlots()
        {
            int max = Art.GetMaxExtendedItemId();
            int start = Math.Max(1, _sourceId + 1);
            for (int pass = 0; pass < 2; ++pass)
            {
                for (int id = start; id + 3 <= max; ++id)
                {
                    bool free = true;
                    for (int k = 0; k < 4 && free; ++k)
                    {
                        free = Art.IsLegalStaticId(id + k) && !Art.IsValidStatic(id + k);
                    }

                    if (!free)
                    {
                        continue;
                    }

                    for (int k = 0; k < _allDirections.Length; ++k)
                    {
                        _idBoxes[_allDirections[k]].Text = $"0x{id + k:X4}";
                    }

                    SetStatus($"4 free slots 0x{id:X4} - 0x{id + 3:X4}: North, East, South, West. 'Insert all' places all four there.");
                    return;
                }

                start = 1; // nothing after the source: search from the beginning
            }

            SetStatus("No 4 free slots in a row found.");
        }

        private string DefaultExportName()
        {
            string name = _sourceId >= 0 && TileData.ItemTable != null && _sourceId < TileData.ItemTable.Length
                ? (TileData.ItemTable[_sourceId].Name ?? string.Empty).Trim()
                : Path.GetFileNameWithoutExtension(_sourceText);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "art";
            }

            if (_sourceId >= 0)
            {
                name += $"_0x{_sourceId:X4}";
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name.Replace(' ', '_');
        }

        /// <summary>The export folder (remembered), optionally with a new sub folder.</summary>
        private string ExportFolder(bool choose)
        {
            ArtDirectionSettings settings = ArtDirectionSettings.Load();
            string folder = settings.ExportFolder;
            if (choose || string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                using var dlg = new FolderBrowserDialog
                {
                    Description = "Folder for the exported graphics (remembered)",
                    SelectedPath = Directory.Exists(folder ?? string.Empty) ? folder : Options.OutputPath
                };
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return null;
                }

                folder = dlg.SelectedPath;
                settings.ExportFolder = folder;
                settings.Save();
            }

            if (_subFolderCheck.Checked)
            {
                string sub = string.IsNullOrWhiteSpace(_subFolderName.Text) ? DefaultExportName() : _subFolderName.Text.Trim();
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    sub = sub.Replace(c, '_');
                }

                folder = Path.Combine(folder, sub);
            }

            Directory.CreateDirectory(folder);
            return folder;
        }

        private void SavePngs(bool chooseFolder)
        {
            if (_source == null)
            {
                return;
            }

            string folder = ExportFolder(chooseFolder);
            if (folder == null)
            {
                return;
            }

            string name = DefaultExportName();
            int count = 0;
            using (Bitmap src = ToExportBitmap(_source))
            {
                src.Save(Path.Combine(folder, $"{name}_Source.png"), ImageFormat.Png);
                ++count;
            }

            foreach (ArtDirection d in _allDirections)
            {
                if (_results.TryGetValue(d, out ArtDirectionResult r))
                {
                    using Bitmap bmp = ToExportBitmap(r.Image);
                    bmp.Save(Path.Combine(folder, $"{name}_{d}.png"), ImageFormat.Png);
                    ++count;
                }
            }

            SetStatus($"{count} PNG(s) saved to {folder}");
        }

        private List<(string Label, int ItemId, ArtPixelImage Image)> SheetSprites()
        {
            var list = new List<(string, int, ArtPixelImage)> { ("Source", _sourceId, _source) };
            foreach (ArtDirection d in _allDirections)
            {
                int id = Utils.ConvertStringToInt(_idBoxes[d].Text, out int v, 0, Art.GetMaxExtendedItemId()) ? v : -1;
                list.Add((d.ToString(), id, _results.TryGetValue(d, out ArtDirectionResult r) ? r.Image : null));
            }

            return list;
        }

        /// <summary>
        /// One picture with Source, North, East, South, West in cells (magenta lines, black = transparent, bottom
        /// centre = UO fix point) + a .json and a .txt with the instructions - to send it to somebody / an AI.
        /// </summary>
        private void ExportSheet()
        {
            if (_source == null)
            {
                return;
            }

            string folder = ExportFolder(false);
            if (folder == null)
            {
                return;
            }

            string name = DefaultExportName();
            ArtPixelImage sheet = ArtSpriteSheet.Compose(SheetSprites(), 5, 4, name, out SpriteSheetInfo info);
            string png = Path.Combine(folder, $"{name}_sheet.png");
            using (Bitmap bmp = ToExportBitmap(sheet, Color.Black))
            {
                bmp.Save(png, ImageFormat.Png);
            }

            File.WriteAllText(Path.ChangeExtension(png, ".json"), ArtSpriteSheet.ToJson(info));
            File.WriteAllText(Path.Combine(folder, $"{name}_sheet_prompt.txt"), ArtSpriteSheet.AiPrompt(info));
            _lastSheet = info;
            SetStatus($"Sprite sheet saved: {png} (+ .json and _prompt.txt). 'Copy AI prompt' puts the instructions into the clipboard.");
        }

        /// <summary>Placement window for the current directions (North, East, South, West).</summary>
        private void OpenPlacement()
        {
            var items = _allDirections.Where(d => _results.ContainsKey(d)).Select(d => (d.ToString(), _results[d].Image)).ToList();
            if (items.Count == 0)
            {
                SetStatus("Nothing to place - generate or import first.");
                return;
            }

            new SpriteSheetPlacementForm(items, Math.Max(1, _sourceId + 1), _insertCallback).Show(this);
        }

        private void CopyAiPrompt()
        {
            SpriteSheetInfo info = _lastSheet;
            if (info == null && _source != null)
            {
                ArtSpriteSheet.Compose(SheetSprites(), 5, 4, DefaultExportName(), out info);
            }

            Clipboard.SetText(ArtSpriteSheet.AiPrompt(info));
            SetStatus("AI prompt copied to the clipboard - paste it together with the sprite sheet.");
        }

        /// <summary>
        /// Reads a sprite sheet (PNG, with or without .json) - e.g. edited by somebody else or an AI - and shows its
        /// cells as Source / North / East / South / West, ready to insert.
        /// </summary>
        private void ImportSheet()
        {
            using var dlg = new OpenFileDialog { Filter = "Sprite sheet (*.png;*.bmp)|*.png;*.bmp", Title = "Import sprite sheet" };
            string folder = ArtDirectionSettings.Load().ExportFolder;
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                dlg.InitialDirectory = folder;
            }

            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            ArtPixelImage sheet;
            using (var bmp = new Bitmap(dlg.FileName))
            {
                sheet = FromBitmap(bmp, false);
            }

            string json = Path.ChangeExtension(dlg.FileName, ".json");
            SpriteSheetInfo info = File.Exists(json) ? ArtSpriteSheet.FromJson(File.ReadAllText(json)) : null;
            List<(SpriteSheetCell Cell, ArtPixelImage Image)> cells = ArtSpriteSheet.Slice(sheet, info);
            if (cells.Count == 0)
            {
                SetStatus("No cells found (magenta #FF00FF separator lines or a .json are needed).");
                return;
            }

            // a sheet with more cells (new graphics from somebody): all of them can go into free slots at once
            var filled = cells.Where(c => c.Image != null).ToList();
            if (cells.Count > 5 && filled.Count > 0 &&
                MessageBox.Show(this, $"The sheet has {filled.Count} graphics. Place ALL of them as new items (choose the slots)?" +
                    Environment.NewLine + "(No = show the first five cells as Source / North / East / South / West.)",
                    "Import sprite sheet", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var items = cells.Select((c, i) => (string.IsNullOrEmpty(c.Cell.Label) ? $"Cell {i} (col {c.Cell.Column}, row {c.Cell.Row})" : c.Cell.Label, c.Image)).ToList();
                new SpriteSheetPlacementForm(items, Math.Max(1, _sourceId + 1), _insertCallback).Show(this);
                return;
            }

            // assign by label, otherwise by position: Source, North, East, South, West
            string[] order = { "Source", "North", "East", "South", "West" };
            var byLabel = new Dictionary<string, (SpriteSheetCell Cell, ArtPixelImage Image)>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cells.Count; ++i)
            {
                string label = !string.IsNullOrEmpty(cells[i].Cell.Label) ? cells[i].Cell.Label : i < order.Length ? order[i] : $"Cell{i}";
                byLabel[label] = cells[i];
            }

            if (byLabel.TryGetValue("Source", out var srcCell) && srcCell.Image != null)
            {
                _source = srcCell.Image;
                _sourceId = srcCell.Cell.ItemId;
                _sourceText = $"{Path.GetFileName(dlg.FileName)} (sheet)";
            }

            _results.Clear();
            int count = 0;
            foreach (ArtDirection d in _allDirections)
            {
                if (byLabel.TryGetValue(d.ToString(), out var c) && c.Image != null)
                {
                    _results[d] = new ArtDirectionResult { Direction = d, Image = c.Image, IsExact = true, Note = "Imported from the sprite sheet." };
                    if (c.Cell.ItemId >= 0)
                    {
                        _idBoxes[d].Text = $"0x{c.Cell.ItemId:X4}";
                    }

                    ++count;
                }
            }

            if (_source == null)
            {
                _source = _results.Values.FirstOrDefault()?.Image;
            }

            _lastSheet = info;
            _imported = true;
            RefreshPreviews();
            SetStatus($"{count} direction(s) imported from {Path.GetFileName(dlg.FileName)}" +
                      (info == null ? " (cells found by the magenta lines)" : string.Empty) + ". Check them and 'Insert all'.");
        }
        #endregion

        #region Output
        private void FillFreeIds()
        {
            int max = Art.GetMaxExtendedItemId();
            int next = _sourceId >= 0 ? _sourceId + 1 : 0;
            foreach (ArtDirection d in _allDirections)
            {
                if (d == SourceDirection && _sourceId >= 0)
                {
                    _idBoxes[d].Text = $"0x{_sourceId:X4}";
                    continue;
                }

                while (next <= max && (!Art.IsLegalStaticId(next) || Art.IsValidStatic(next)))
                {
                    ++next;
                }

                _idBoxes[d].Text = next <= max ? $"0x{next:X4}" : string.Empty;
                ++next;
            }
        }

        private bool InsertOne(ArtDirection d, bool showMessage)
        {
            if (!_results.TryGetValue(d, out ArtDirectionResult r))
            {
                if (showMessage)
                {
                    SetStatus($"{d}: nothing generated.");
                }

                return false;
            }

            if (!Utils.ConvertStringToInt(_idBoxes[d].Text, out int id, 0, Art.GetMaxExtendedItemId()) || !Art.IsLegalStaticId(id))
            {
                SetStatus($"{d}: invalid ID '{_idBoxes[d].Text}' (max 0x{Art.GetMaxExtendedItemId():X}).");
                return false;
            }

            if (d == SourceDirection && id == _sourceId && r.Image != null)
            {
                // writing the drawn direction back onto itself only makes sense if the shadow changed
                if (showMessage && MessageBox.Show(this, $"Overwrite the source item 0x{id:X4}?", "Insert", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return false;
                }
            }
            else if (Art.IsValidStatic(id) && showMessage &&
                     MessageBox.Show(this, $"0x{id:X4} is already used. Replace it?", "Insert", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return false;
            }

            _insertCallback?.Invoke(id, ToBitmap(r.Image));
            if (showMessage && _learnOnInsertCheck != null && _learnOnInsertCheck.Checked)
            {
                RememberCurrent(true);
            }

            SetStatus($"{d} inserted as 0x{id:X4}. Save the Art (and TileData) to write the files.");
            return true;
        }

        private void InsertAll()
        {
            var used = _allDirections
                .Where(d => _results.ContainsKey(d) && !IsSourceSlot(d))
                .Where(d => Utils.ConvertStringToInt(_idBoxes[d].Text, out int id, 0, Art.GetMaxExtendedItemId()) && Art.IsValidStatic(id))
                .ToList();

            if (used.Count > 0 && MessageBox.Show(this,
                    $"{string.Join(", ", used)}: target ID already used. Replace?", "Insert all",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            int count = 0;
            foreach (ArtDirection d in _allDirections)
            {
                // the drawn direction is not written onto the source item itself (but into a new slot, e.g. "4 free slots")
                if (IsSourceSlot(d))
                {
                    continue;
                }

                if (_results.ContainsKey(d) && InsertOne(d, false))
                {
                    ++count;
                }
            }

            if (count > 0 && _learnOnInsertCheck != null && _learnOnInsertCheck.Checked)
            {
                RememberCurrent(true);
            }

            SetStatus($"{count} item(s) inserted. Save the Art (and TileData) to write the files.");
        }

        private void CopyOne(ArtDirection d)
        {
            if (_results.TryGetValue(d, out ArtDirectionResult r))
            {
                using Bitmap bmp = ToExportBitmap(r.Image);
                Clipboard.SetImage(bmp);
                SetStatus($"{d} copied to the clipboard.");
            }
        }

        private void SaveOne(ArtDirection d)
        {
            if (!_results.TryGetValue(d, out ArtDirectionResult r))
            {
                return;
            }

            using var dlg = new SaveFileDialog { Filter = "PNG (*.png)|*.png|Bitmap (*.bmp)|*.bmp", FileName = $"Item_{d}.png" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using Bitmap bmp = ToExportBitmap(r.Image);
            bmp.Save(dlg.FileName, dlg.FilterIndex == 2 ? ImageFormat.Bmp : ImageFormat.Png);
            SetStatus($"{d} saved to {dlg.FileName}.");
        }

        /// <summary>Copy/PNG output: transparent pixels black (UO convention) or real transparency.</summary>
        private Bitmap ToExportBitmap(ArtPixelImage img)
        {
            bool black = _exportBackgroundBox == null || _exportBackgroundBox.SelectedIndex == 0;
            return ToExportBitmap(img, black ? Color.Black : Color.Transparent);
        }

        /// <summary>Same size as the art image, transparent pixels filled with <paramref name="background"/>.</summary>
        private static Bitmap ToExportBitmap(ArtPixelImage img, Color background)
        {
            using Bitmap art = ToBitmap(img);
            bool transparent = background.A == 0;
            var bmp = new Bitmap(art.Width, art.Height, transparent ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(background);
                g.DrawImageUnscaled(art, 0, 0);
            }

            return bmp;
        }

        /// <summary>Puts the image into the clipboard (bitmap + PNG stream, so transparency survives in paint programs).</summary>
        private void CopyToClipboard(ArtPixelImage img, Color background, string what)
        {
            if (img == null)
            {
                SetStatus("No image.");
                return;
            }

            using Bitmap bmp = ToExportBitmap(img, background);
            var data = new DataObject();
            data.SetData(DataFormats.Bitmap, true, bmp);
            var png = new MemoryStream();
            bmp.Save(png, ImageFormat.Png);
            data.SetData("PNG", false, png);
            Clipboard.SetDataObject(data, true);

            string bg = background.A == 0 ? "transparent" : background == Color.Black ? "black" : "white";
            SetStatus($"{what} copied to the clipboard ({img.Width}x{img.Height}, {bg} background).");
        }

        /// <summary>Right-click menu of a preview image.</summary>
        private ContextMenuStrip BuildImageMenu(Func<ArtPixelImage> getImage, ArtDirection? direction)
        {
            string what = direction?.ToString() ?? "Source";
            var menu = new ContextMenuStrip();
            menu.Items.Add("Copy (black background)", null, (s, e) => CopyToClipboard(getImage(), Color.Black, what));
            menu.Items.Add("Copy (white background)", null, (s, e) => CopyToClipboard(getImage(), Color.White, what));
            menu.Items.Add("Copy (transparent)", null, (s, e) => CopyToClipboard(getImage(), Color.Transparent, what));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Save as PNG...", null, (s, e) => SaveImage(getImage(), what));
            menu.Items.Add("Open in Transform window (skew / distort)...", null, (s, e) =>
            {
                ArtPixelImage img = getImage();
                if (img == null)
                {
                    return;
                }

                var form = new ArtTransformForm(_insertCallback);
                form.Show(this);
                form.LoadSource(img.Clone(), direction == null ? _sourceId : -1,
                    direction == null ? _sourceText : $"{direction} of {_sourceText}");
            });
            if (direction.HasValue)
            {
                menu.Items.Add("Insert into Art", null, (s, e) => InsertOne(direction.Value, true));
            }
            else
            {
                menu.Items.Add("Repair source", null, (s, e) => RepairSource());
                menu.Items.Add("Paste as source (Ctrl+V)", null, (s, e) => LoadFromClipboard());
            }

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Background black", null, (s, e) => SetBackground(Color.Black));
            menu.Items.Add("Background white", null, (s, e) => SetBackground(Color.White));
            var errors = new ToolStripMenuItem("Show errors") { CheckOnClick = true };
            errors.Click += (s, e) => _overlayCheck.Checked = errors.Checked;
            menu.Opening += (s, e) =>
            {
                errors.Checked = _overlayCheck.Checked;
                bool has = getImage() != null;
                foreach (ToolStripItem item in menu.Items)
                {
                    if (item.Text.StartsWith("Copy") || item.Text.StartsWith("Save") || item.Text.StartsWith("Insert") || item.Text.StartsWith("Repair"))
                    {
                        item.Enabled = has;
                    }
                }
            };
            menu.Items.Add(errors);
            return menu;
        }

        private void SaveImage(ArtPixelImage img, string what)
        {
            if (img == null)
            {
                return;
            }

            using var dlg = new SaveFileDialog { Filter = "PNG (*.png)|*.png|Bitmap (*.bmp)|*.bmp", FileName = $"Item_{what}.png" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using Bitmap bmp = ToExportBitmap(img);
            bmp.Save(dlg.FileName, dlg.FilterIndex == 2 ? ImageFormat.Bmp : ImageFormat.Png);
            SetStatus($"{what} saved to {dlg.FileName} ({img.Width}x{img.Height}).");
        }

        private void RepairSource()
        {
            if (_source == null)
            {
                return;
            }

            ArtPixelImage repaired = ArtDirectionGenerator.Repair(_source, (int)_repairHole.Value);
            int changed = repaired.Pixels.Where((p, i) => p != _source.Pixels[i]).Count();
            _source = repaired;
            Regenerate();
            SetStatus(changed == 0
                ? "Source checked: nothing to repair."
                : $"Source repaired: {changed} pixel(s) changed. Insert the drawn direction to write it back.");
        }

        private void SetStatus(string text)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = text;
            }
        }
        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // detach first, then dispose: a PictureBox that still shows a disposed image throws
            // "Parameter is not valid" (ImageAnimator.CanAnimate) when the window is hidden/shown
            foreach (PictureBox pb in _previews.Values.Append(_sourcePreview))
            {
                if (pb == null)
                {
                    continue;
                }

                Image img = pb.Image;
                pb.Image = null;
                img?.Dispose();
            }

            base.OnFormClosed(e);
        }
    }
}
