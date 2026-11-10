using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter
{
    /// <summary>
    /// Processes many images in one go: resize, make seamless, UO 16-bit colors, texture → land tile,
    /// then save them as files and / or write them into texmaps.mul or art.mul (land tiles).
    /// </summary>
    internal sealed class BatchDialog : Form
    {
        private static readonly string[] Extensions = { ".png", ".bmp", ".gif", ".jpg", ".jpeg", ".tif", ".tiff" };

        private readonly ListBox _files = new ListBox { Width = 520, Height = 180, HorizontalScrollbar = true, SelectionMode = SelectionMode.MultiExtended };
        private readonly CheckBox _chkResize = new CheckBox { Text = "Resize to", AutoSize = true, Checked = true, Margin = new Padding(3, 6, 3, 0) };
        private readonly NumericUpDown _numW = new NumericUpDown { Minimum = 1, Maximum = 4096, Value = 64, Width = 60 };
        private readonly NumericUpDown _numH = new NumericUpDown { Minimum = 1, Maximum = 4096, Value = 64, Width = 60 };
        private readonly ComboBox _quality = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
        private readonly CheckBox _chkSeamless = new CheckBox { Text = "Make seamless, blend %", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
        private readonly NumericUpDown _numBlend = new NumericUpDown { Minimum = 10, Maximum = 100, Value = 50, Width = 60 };
        private readonly CheckBox _chk16 = new CheckBox { Text = "UO 16-bit colors", AutoSize = true };
        private readonly CheckBox _chkLand = new CheckBox { Text = "Texture → land tile 44 × 44 (black outside)", AutoSize = true };
        private readonly CheckBox _chkFiles = new CheckBox { Text = "Save files into", AutoSize = true, Checked = true, Margin = new Padding(3, 6, 3, 0) };
        private readonly TextBox _txtFolder = new TextBox { Width = 330 };
        private readonly ComboBox _format = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
        private readonly CheckBox _chkTex = new CheckBox { Text = "Write into texmaps.mul from ID", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
        private readonly TextBox _txtTexId = new TextBox { Width = 80 };
        private readonly CheckBox _chkLandArt = new CheckBox { Text = "Write into art.mul land tiles from ID", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
        private readonly TextBox _txtLandId = new TextBox { Width = 80 };
        private readonly CheckBox _chkSkipUsed = new CheckBox { Text = "Skip used IDs", AutoSize = true, Checked = true };
        private readonly ProgressBar _progress = new ProgressBar { Width = 520, Height = 18 };
        private readonly Label _status = new Label { AutoSize = true };

        public BatchDialog(string outputDirectory)
        {
            Text = "Batch – many images at once / Stapelverarbeitung";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(10);

            _quality.Items.AddRange(new object[] { "Pixel (hard)", "Smooth", "Smooth + sharpen" });
            _quality.SelectedIndex = 1;
            _format.Items.AddRange(new object[] { "PNG", "BMP", "TIFF" });
            _format.SelectedIndex = 0;
            _txtFolder.Text = Path.Combine(outputDirectory, "Batch_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            _files.AllowDrop = true;
            _files.DragEnter += (s, e) => e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            _files.DragDrop += (s, e) => AddPaths((string[])e.Data.GetData(DataFormats.FileDrop));

            bool art = CutterArt.IsAvailable, tex = TextureMul.IsAvailable;
            _chkTex.Enabled = _txtTexId.Enabled = tex;
            _chkLandArt.Enabled = _txtLandId.Enabled = art;
            if (tex)
            {
                int free = TextureMul.FindFree(1);
                _txtTexId.Text = free >= 0 ? $"0x{free:X4}" : string.Empty;
            }

            var tip = new ToolTip { AutoPopDelay = 15000 };
            tip.SetToolTip(_files, "Drop image files or folders here.\n\nDE: Bilddateien oder Ordner hier hineinziehen.");
            tip.SetToolTip(_chkTex, "Textures must end up 64 × 64 or 128 × 128 (use \"Resize to\").\n\nDE: Texturen müssen am Ende 64 × 64 oder 128 × 128 sein (\"Resize to\" benutzen).");
            tip.SetToolTip(_chkLandArt, "Needs 44 × 44 images: turn on \"Texture → land tile\".\n\nDE: Braucht 44 × 44 Bilder: \"Texture → land tile\" einschalten.");

            Button Btn(string text, EventHandler click)
            {
                var b = new Button { Text = text, AutoSize = true, UseVisualStyleBackColor = true };
                b.Click += click;
                return b;
            }

            var run = Btn("Run", (s, e) => Run());
            var close = new Button { Text = "Close", DialogResult = DialogResult.Cancel, AutoSize = true, UseVisualStyleBackColor = true };
            CancelButton = close;

            var table = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Location = new Point(10, 10) };
            table.Controls.Add(Row(Btn("Add files...", (s, e) => AddFiles()), Btn("Add folder...", (s, e) => AddFolder()),
                Btn("Remove", (s, e) => { foreach (object o in _files.SelectedItems.Cast<object>().ToList()) _files.Items.Remove(o); }),
                Btn("Clear", (s, e) => _files.Items.Clear())));
            table.Controls.Add(_files);
            table.Controls.Add(new Label { Text = "Steps (in this order):", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 8, 3, 2) });
            table.Controls.Add(Row(_chkResize, _numW, new Label { Text = "×", AutoSize = true, Margin = new Padding(0, 6, 0, 0) }, _numH, _quality));
            table.Controls.Add(Row(_chkSeamless, _numBlend));
            table.Controls.Add(_chk16);
            table.Controls.Add(_chkLand);
            table.Controls.Add(new Label { Text = "Targets:", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 8, 3, 2) });
            table.Controls.Add(Row(_chkFiles, _txtFolder, _format));
            table.Controls.Add(Row(_chkTex, _txtTexId));
            table.Controls.Add(Row(_chkLandArt, _txtLandId));
            table.Controls.Add(_chkSkipUsed);
            table.Controls.Add(_progress);
            table.Controls.Add(_status);
            table.Controls.Add(Row(run, close));
            Controls.Add(table);
        }

        /// <summary>IDs written into texmaps.mul (the Texture Cutter refreshes its list).</summary>
        public List<int> WrittenTextures { get; } = new List<int>();

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            row.Controls.AddRange(controls);
            return row;
        }

        private void AddPaths(IEnumerable<string> paths)
        {
            foreach (string p in paths)
            {
                if (Directory.Exists(p))
                {
                    AddPaths(Directory.GetFiles(p).OrderBy(f => f, StringComparer.OrdinalIgnoreCase));
                }
                else if (File.Exists(p) && Extensions.Contains(Path.GetExtension(p).ToLowerInvariant()) && !_files.Items.Contains(p))
                {
                    _files.Items.Add(p);
                }
            }
            _status.Text = $"{_files.Items.Count} images";
        }

        private void AddFiles()
        {
            using (var dialog = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Multiselect = true })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    AddPaths(dialog.FileNames);
                }
            }
        }

        private void AddFolder()
        {
            using (var dialog = new FolderBrowserDialog { Description = "Folder with images", UseDescriptionForTitle = true })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    AddPaths(new[] { dialog.SelectedPath });
                }
            }
        }

        private Bitmap Process(Bitmap bmp)
        {
            Bitmap Step(Bitmap current, Func<Bitmap, Bitmap> f)
            {
                Bitmap next = f(current);
                current.Dispose();
                return next;
            }
            if (_chkResize.Checked)
            {
                var q = (TextureImaging.ResizeQuality)_quality.SelectedIndex;
                bmp = Step(bmp, b => TextureImaging.Resize(b, (int)_numW.Value, (int)_numH.Value, q));
            }
            if (_chkSeamless.Checked)
            {
                bmp = Step(bmp, b => TextureImaging.MakeSeamless(b, (int)_numBlend.Value));
            }
            if (_chk16.Checked)
            {
                bmp = Step(bmp, TextureImaging.To16Bit);
            }
            if (_chkLand.Checked)
            {
                bmp = Step(bmp, b => TextureImaging.TextureToLandTile(b, true));
            }
            return bmp;
        }

        private void Run()
        {
            if (_files.Items.Count == 0)
            {
                MessageBox.Show(this, "Add images first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int texId = 0, landId = 0;
            if (_chkTex.Checked && !CutterImaging.TryParseId(_txtTexId.Text, out texId))
            {
                MessageBox.Show(this, "Enter the first texture ID.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_chkLandArt.Checked && !CutterImaging.TryParseId(_txtLandId.Text, out landId))
            {
                MessageBox.Show(this, "Enter the first land tile ID.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!_chkFiles.Checked && !_chkTex.Checked && !_chkLandArt.Checked)
            {
                MessageBox.Show(this, "Choose at least one target.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if ((_chkTex.Checked || _chkLandArt.Checked) &&
                MessageBox.Show(this, "The images are written into the loaded mul files (in memory). Continue?", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            string ext = _format.SelectedIndex == 1 ? ".bmp" : _format.SelectedIndex == 2 ? ".tiff" : ".png";
            var errors = new List<string>();
            int saved = 0, textures = 0, lands = 0;
            _progress.Maximum = _files.Items.Count;
            _progress.Value = 0;
            Cursor = Cursors.WaitCursor;
            try
            {
                if (_chkFiles.Checked)
                {
                    Directory.CreateDirectory(_txtFolder.Text);
                }
                foreach (string file in _files.Items.Cast<string>().ToList())
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    try
                    {
                        using (Bitmap result = Process(CutterImaging.LoadBitmap(file)))
                        {
                            if (_chkFiles.Checked)
                            {
                                CutterImaging.SaveImage(result, Path.Combine(_txtFolder.Text, name + ext), TransparentFill.Keep);
                                saved++;
                            }
                            if (_chkTex.Checked)
                            {
                                if (!TextureMul.IsValidSize(result.Size))
                                {
                                    errors.Add($"{name}: {result.Width}×{result.Height} is no texture size (64 / 128).");
                                }
                                else
                                {
                                    while (_chkSkipUsed.Checked && texId <= TextureMul.MaxId && TextureMul.IsValid(texId))
                                    {
                                        texId++;
                                    }
                                    if (texId > TextureMul.MaxId)
                                    {
                                        errors.Add($"{name}: no texture ID left.");
                                    }
                                    else
                                    {
                                        TextureMul.Write(texId, result, this);
                                        WrittenTextures.Add(texId);
                                        texId++;
                                        textures++;
                                    }
                                }
                            }
                            if (_chkLandArt.Checked)
                            {
                                if (result.Width != CutterArt.LandSize || result.Height != CutterArt.LandSize)
                                {
                                    errors.Add($"{name}: {result.Width}×{result.Height} is no land tile (44 × 44).");
                                }
                                else
                                {
                                    while (_chkSkipUsed.Checked && landId <= CutterArt.MaxId(ArtKind.Land) && CutterArt.IsValid(ArtKind.Land, landId))
                                    {
                                        landId++;
                                    }
                                    if (landId > CutterArt.MaxId(ArtKind.Land))
                                    {
                                        errors.Add($"{name}: no land tile ID left.");
                                    }
                                    else
                                    {
                                        CutterArt.Write(ArtKind.Land, landId, result, this);
                                        landId++;
                                        lands++;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{name}: {ex.Message}");
                    }
                    _progress.Value++;
                    _status.Text = $"{_progress.Value} / {_progress.Maximum}";
                    Application.DoEvents();
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            string report = $"Done. Files: {saved}, textures: {textures}, land tiles: {lands}.";
            if (textures > 0 || lands > 0)
            {
                report += "\nSave texmaps.mul / art.mul to keep the written graphics.";
            }
            if (errors.Count > 0)
            {
                report += $"\n\n{errors.Count} problems:\n" + string.Join("\n", errors.Take(15)) + (errors.Count > 15 ? "\n…" : string.Empty);
            }
            _status.Text = report.Split('\n')[0];
            MessageBox.Show(this, report, Text, MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
    }
}
