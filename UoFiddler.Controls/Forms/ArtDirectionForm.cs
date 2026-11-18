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
            top.Controls.Add(MakeButton("Generate", (s, e) => Regenerate()));
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
                "Directional (cannon, chair...)"
            }, 0, 190);
            new ToolTip().SetToolTip(_kindBox,
                "Wall / fence: north = south (front and back look the same)." + Environment.NewLine +
                "Flat: lies on the ground, turned 180°." + Environment.NewLine +
                "Solid: back side as shaded template to paint over." + Environment.NewLine +
                "Directional: things that point somewhere (cannon barrel, chair) - turned 180° on the ground.");
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
            _repairHole = MakeNumeric(1, 200, 12, 0, 1);
            opt.Controls.Add(_repairHole);
            opt.Controls.Add(MakeButton("Repair source", (s, e) => RepairSource()));

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
            bottom.Controls.Add(MakeButton("Insert all checked into Art", (s, e) => InsertAll()));
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
            tip.SetToolTip(mc.Axis, "Axis slope for the turns: 0 = measured, 0.5 = \\ diagonal, -0.5 = / diagonal");
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
            if (bitmap == null)
            {
                return;
            }

            _source = FromBitmap(bitmap, false);
            _sourceId = itemId;
            _sourceText = itemId >= 0
                ? $"Item 0x{itemId:X4} ({bitmap.Width}x{bitmap.Height})"
                : $"{bitmap.Width}x{bitmap.Height}";
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
        private static unsafe ArtPixelImage FromBitmap(Bitmap bmp, bool whiteTransparent)
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

        private static unsafe Bitmap ToBitmap(ArtPixelImage img)
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

        private string ErrorText(ArtPixelImage img, out bool hasErrors)
        {
            ArtDirectionGenerator.FindErrors(img, (int)_repairHole.Value, out int holes, out int strays, out int blacks, out int white);
            hasErrors = holes + strays + blacks + white > 0;
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

                // UO anchor: bottom center
                using var pen = new Pen(Color.FromArgb(160, _background.GetBrightness() < 0.5f ? Color.White : Color.Black)) { DashStyle = DashStyle.Dot };
                g.DrawLine(pen, view.Width / 2, view.Height - (4 * zoom), view.Width / 2, view.Height - 1);
            }

            pb.Image = view;
            old?.Dispose();
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
            SetStatus($"{d} inserted as 0x{id:X4}. Save the Art (and TileData) to write the files.");
            return true;
        }

        private void InsertAll()
        {
            var used = _allDirections
                .Where(d => _results.ContainsKey(d) && !(d == SourceDirection && _sourceId >= 0))
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
                // the drawn direction of an existing item is not written again
                if (d == SourceDirection && _sourceId >= 0)
                {
                    continue;
                }

                if (_results.ContainsKey(d) && InsertOne(d, false))
                {
                    ++count;
                }
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
            foreach (PictureBox pb in _previews.Values.Append(_sourcePreview))
            {
                pb?.Image?.Dispose();
            }

            base.OnFormClosed(e);
        }
    }
}
