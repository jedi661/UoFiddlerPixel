using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter
{
    /// <summary>Settings for cutting an image into vertical strips (UO art pieces).</summary>
    internal sealed class StripSettings
    {
        public int StripWidth = 44;
        public TransparentFill Fill = TransparentFill.White;
        public bool TrimTop;
        public bool SkipEmpty = true;
        public bool SaveFiles = true;
        public string Extension = ".bmp";
        public bool WriteToArt;
        public int StartId;
        public bool SkipUsedIds = true;
        public bool AddToCollection;
    }

    /// <summary>Dialog for "Split into art pieces".</summary>
    internal sealed class StripsDialog : Form
    {
        private readonly NumericUpDown _width = new NumericUpDown { Minimum = 1, Maximum = 1024, Value = 44, Width = 70 };
        private readonly ComboBox _fill = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
        private readonly CheckBox _trimTop = new CheckBox { Text = "Remove empty rows at the top of each piece", AutoSize = true };
        private readonly CheckBox _skipEmpty = new CheckBox { Text = "Skip empty pieces", AutoSize = true, Checked = true };
        private readonly CheckBox _saveFiles = new CheckBox { Text = "Save as files in tempGrafic", AutoSize = true, Checked = true };
        private readonly ComboBox _format = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
        private readonly CheckBox _writeArt = new CheckBox { Text = "Write into art.mul as items from ID", AutoSize = true };
        private readonly TextBox _startId = new TextBox { Width = 80 };
        private readonly CheckBox _skipUsed = new CheckBox { Text = "Skip used IDs", AutoSize = true, Checked = true };
        private readonly CheckBox _collection = new CheckBox { Text = "Add pieces to the collection", AutoSize = true };
        private readonly Label _preview = new Label { AutoSize = true, ForeColor = SystemColors.GrayText };
        private readonly int _imageWidth;

        public StripsDialog(Size imageSize)
        {
            _imageWidth = imageSize.Width;
            Text = "Split into art pieces / In Art-Teile zerlegen";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(10);

            _fill.Items.AddRange(new object[] { "keep transparent", "black", "white" });
            _fill.SelectedIndex = 2;
            _format.Items.AddRange(new object[] { "BMP", "PNG", "TIFF" });
            _format.SelectedIndex = 0;

            var tip = new ToolTip { AutoPopDelay = 15000 };
            tip.SetToolTip(_width, "Width of every piece. UO art: 44.\n\nDE: Breite jedes Teils. UO-Art: 44.");
            tip.SetToolTip(_fill, "What transparent pixels become in the files. Old behavior: white.\n\nDE: Was aus transparenten Pixeln in den Dateien wird. Bisher: weiß.");
            tip.SetToolTip(_trimTop, "Each piece keeps its bottom edge and loses the empty rows above.\n\nDE: Jedes Teil behält die Unterkante, leere Zeilen oben fallen weg.");
            tip.SetToolTip(_writeArt, "Pieces are written in memory. Save art.mul afterwards.\n\nDE: Teile werden im Speicher geschrieben. Danach art.mul speichern.");

            bool artAvailable = CutterArt.IsAvailable;
            _writeArt.Enabled = artAvailable;
            _startId.Enabled = artAvailable;
            _skipUsed.Enabled = artAvailable;

            var findFree = new Button { Text = "Find free block", AutoSize = true, Enabled = artAvailable, UseVisualStyleBackColor = true };
            findFree.Click += (s, e) => FindFree();

            var ok = new Button { Text = "Split", DialogResult = DialogResult.OK, Width = 90, UseVisualStyleBackColor = true };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90, UseVisualStyleBackColor = true };
            AcceptButton = ok;
            CancelButton = cancel;
            ok.Click += Ok_Click;

            var table = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Location = new Point(10, 10) };
            table.Controls.Add(Row(new Label { Text = "Piece width:", AutoSize = true, Margin = new Padding(3, 6, 3, 0) }, _width, _preview));
            table.Controls.Add(Row(new Label { Text = "Transparent pixels in files:", AutoSize = true, Margin = new Padding(3, 6, 3, 0) }, _fill));
            table.Controls.Add(_trimTop);
            table.Controls.Add(_skipEmpty);
            table.Controls.Add(Row(_saveFiles, _format));
            table.Controls.Add(Row(_writeArt, _startId, findFree));
            table.Controls.Add(Row(new Label { Width = 18 }, _skipUsed));
            table.Controls.Add(_collection);
            table.Controls.Add(Row(ok, cancel));
            Controls.Add(table);

            _width.ValueChanged += (s, e) => UpdatePreview();
            UpdatePreview();
            if (artAvailable)
            {
                int free = CutterArt.FindFree(ArtKind.Item, 0x4000 > CutterArt.MaxId(ArtKind.Item) ? 0 : 0x4000);
                if (free >= 0)
                {
                    _startId.Text = $"0x{free:X4}";
                }
            }
        }

        public StripSettings Settings { get; private set; }

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            row.Controls.AddRange(controls);
            return row;
        }

        private int PieceCount => (_imageWidth + (int)_width.Value - 1) / (int)_width.Value;

        private void UpdatePreview() => _preview.Text = $"→ {PieceCount} pieces (image is {_imageWidth} px wide)";

        private void FindFree()
        {
            CutterImaging.TryParseId(_startId.Text, out int start);
            int id = CutterArt.FindFreeBlock(ArtKind.Item, start, PieceCount);
            if (id < 0)
            {
                MessageBox.Show(this, $"No {PieceCount} free item IDs in a row found.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _startId.Text = $"0x{id:X4}";
            _writeArt.Checked = true;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            int startId = 0;
            if (_writeArt.Checked && !CutterImaging.TryParseId(_startId.Text, out startId))
            {
                MessageBox.Show(this, "Enter the first item ID (hex 0x1A2B or decimal).", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.None;
                return;
            }
            if (!_saveFiles.Checked && !_writeArt.Checked && !_collection.Checked)
            {
                MessageBox.Show(this, "Choose at least one target: files, art.mul or collection.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.None;
                return;
            }
            Settings = new StripSettings
            {
                StripWidth = (int)_width.Value,
                Fill = (TransparentFill)_fill.SelectedIndex,
                TrimTop = _trimTop.Checked,
                SkipEmpty = _skipEmpty.Checked,
                SaveFiles = _saveFiles.Checked,
                Extension = _format.SelectedIndex == 1 ? ".png" : _format.SelectedIndex == 2 ? ".tiff" : ".bmp",
                WriteToArt = _writeArt.Checked,
                StartId = startId,
                SkipUsedIds = _skipUsed.Checked,
                AddToCollection = _collection.Checked
            };
        }

        /// <summary>Cuts the image into strips. Pieces keep their transparency (the fill is only for files).</summary>
        public static List<Bitmap> Split(Bitmap image, StripSettings settings)
        {
            var pieces = new List<Bitmap>();
            int w = settings.StripWidth;
            int count = (image.Width + w - 1) / w;
            for (int i = 0; i < count; i++)
            {
                int x = i * w;
                int width = Math.Min(w, image.Width - x);
                var strip = new Bitmap(w, image.Height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(strip))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(image, new Rectangle(0, 0, width, image.Height), new Rectangle(x, 0, width, image.Height), GraphicsUnit.Pixel);
                }

                if (settings.SkipEmpty && CutterImaging.IsEmpty(strip))
                {
                    strip.Dispose();
                    continue;
                }
                if (settings.TrimTop)
                {
                    Bitmap trimmed = CutterImaging.TrimTop(strip);
                    if (trimmed != null)
                    {
                        strip.Dispose();
                        strip = trimmed;
                    }
                }
                pieces.Add(strip);
            }
            return pieces;
        }
    }
}
