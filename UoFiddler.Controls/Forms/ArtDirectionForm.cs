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
        private Color _background = Color.FromArgb(255, 200, 200, 200);

        // targets / preview
        private readonly Dictionary<ArtDirection, CheckBox> _targetChecks = new Dictionary<ArtDirection, CheckBox>();
        private readonly Dictionary<ArtDirection, PictureBox> _previews = new Dictionary<ArtDirection, PictureBox>();
        private readonly Dictionary<ArtDirection, TextBox> _idBoxes = new Dictionary<ArtDirection, TextBox>();
        private readonly Dictionary<ArtDirection, Label> _notes = new Dictionary<ArtDirection, Label>();
        private PictureBox _sourcePreview;
        private Label _sourceLabel;
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
            opt.Controls.Add(_sourceDirectionBox);

            opt.Controls.Add(MakeLabel("Object:"));
            _kindBox = MakeCombo(new object[]
            {
                "Wall / fence (front = back)",
                "Flat on the ground",
                "Solid object (furniture)"
            }, 0, 170);
            opt.Controls.Add(_kindBox);

            _relightCheck = new CheckBox { Text = "Swap face light", AutoSize = true, Checked = true, Margin = new Padding(10, 7, 3, 3) };
            _relightCheck.CheckedChanged += (s, e) => Regenerate();
            opt.Controls.Add(_relightCheck);
            _relightStrength = new TrackBar { Minimum = 0, Maximum = 100, Value = 100, TickFrequency = 25, Width = 110, AutoSize = false, Height = 28 };
            _relightStrength.ValueChanged += (s, e) => Regenerate();
            opt.Controls.Add(_relightStrength);

            opt.Controls.Add(MakeLabel("Shadow:"));
            _shadowBox = MakeCombo(new object[] { "Keep as is (mirrored)", "Detect + recast (UO light)", "Always cast", "Remove" }, 1, 170);
            opt.Controls.Add(_shadowBox);
            opt.Controls.Add(MakeLabel("Length"));
            _shadowLength = MakeNumeric(0.1m, 2.0m, 0.5m, 1, 0.1m);
            opt.Controls.Add(_shadowLength);
            opt.Controls.Add(MakeLabel("Angle°"));
            _shadowAngle = MakeNumeric(-45, 75, 14, 0, 1);
            opt.Controls.Add(_shadowAngle);
            opt.Controls.Add(MakeLabel("Shadow ≤ brightness"));
            _shadowLuma = MakeNumeric(10, 160, 70, 0, 5);
            opt.Controls.Add(_shadowLuma);

            _overlayCheck = new CheckBox { Text = "Show faces", AutoSize = true, Margin = new Padding(10, 7, 3, 3) };
            _overlayCheck.CheckedChanged += (s, e) => RefreshPreviews();
            opt.Controls.Add(_overlayCheck);

            opt.Controls.Add(MakeLabel("Zoom:"));
            _zoomBox = MakeCombo(new object[] { "1x", "2x", "3x", "4x" }, 1, 50);
            _zoomBox.SelectedIndexChanged -= OnOptionChanged;
            _zoomBox.SelectedIndexChanged += (s, e) => RefreshPreviews();
            opt.Controls.Add(_zoomBox);
            opt.Controls.Add(MakeButton("Background...", (s, e) =>
            {
                using var dlg = new ColorDialog { Color = _background };
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _background = dlg.Color;
                    RefreshPreviews();
                }
            }));
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
            layout.Controls.Add(WrapScroll(_sourcePreview), 0, 0);
            _sourceLabel = MakeLabel("-");
            layout.Controls.Add(_sourceLabel, 0, 1);
            group.Controls.Add(layout);
            return group;
        }

        private Control BuildTargetPanel(ArtDirection d)
        {
            var group = new GroupBox { Text = d.ToString(), Dock = DockStyle.Fill };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            PictureBox pb = MakePreview();
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

            group.Controls.Add(layout);
            return group;
        }

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
            _sourceLabel.Text = itemId >= 0
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
            _sourceLabel.Text = $"{Path.GetFileName(dlg.FileName)} ({tmp.Width}x{tmp.Height})";
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
            _sourceLabel.Text = $"Clipboard ({tmp.Width}x{tmp.Height})";
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
                Shadow = (ArtShadowMode)Math.Max(0, _shadowBox.SelectedIndex),
                ShadowLength = (double)_shadowLength.Value,
                ShadowAngle = (double)_shadowAngle.Value,
                ShadowMaxLuma = (int)_shadowLuma.Value
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

                _results[d] = ArtDirectionGenerator.Generate(_source, SourceDirection, d, options);
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

            SetPreview(_sourcePreview, _source, zoom, _overlayCheck.Checked ? ArtDirectionGenerator.ClassifyForPreview(_source, options) : null);

            foreach (ArtDirection d in _allDirections)
            {
                if (_results.TryGetValue(d, out ArtDirectionResult r))
                {
                    ArtFaceClass[] overlay = _overlayCheck.Checked ? ArtDirectionGenerator.ClassifyForPreview(r.Image, options) : null;
                    SetPreview(_previews[d], r.Image, zoom, overlay);
                    _notes[d].Text = (d == SourceDirection ? "(drawn direction) " : string.Empty) + r.Note;
                    _notes[d].ForeColor = r.IsExact ? Color.DimGray : Color.DarkOrange;
                }
                else
                {
                    SetPreview(_previews[d], null, zoom, null);
                    _notes[d].Text = _targetChecks[d].Checked ? string.Empty : "(not selected)";
                }
            }
        }

        private void SetPreview(PictureBox pb, ArtPixelImage img, int zoom, ArtFaceClass[] overlay)
        {
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
                            Color? tint = overlay[(y * img.Width) + x] switch
                            {
                                ArtFaceClass.LeftFace => Color.FromArgb(110, 255, 60, 60),
                                ArtFaceClass.RightFace => Color.FromArgb(110, 60, 90, 255),
                                ArtFaceClass.Top => Color.FromArgb(110, 60, 220, 60),
                                ArtFaceClass.Edge => Color.FromArgb(140, 255, 220, 0),
                                ArtFaceClass.Shadow => Color.FromArgb(140, 190, 0, 190),
                                _ => null
                            };

                            if (tint.HasValue)
                            {
                                using var brush = new SolidBrush(tint.Value);
                                g.FillRectangle(brush, x * zoom, y * zoom, zoom, zoom);
                            }
                        }
                    }
                }

                // UO anchor: bottom center
                using var pen = new Pen(Color.FromArgb(120, Color.Black)) { DashStyle = DashStyle.Dot };
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

                while (next <= max && Art.IsValidStatic(next))
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

            if (!Utils.ConvertStringToInt(_idBoxes[d].Text, out int id, 0, Art.GetMaxExtendedItemId()))
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
                using Bitmap bmp = ToBitmap(r.Image);
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

            using Bitmap bmp = ToBitmap(r.Image);
            bmp.Save(dlg.FileName, dlg.FilterIndex == 2 ? ImageFormat.Bmp : ImageFormat.Png);
            SetStatus($"{d} saved to {dlg.FileName}.");
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
