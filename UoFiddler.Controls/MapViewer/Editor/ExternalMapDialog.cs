/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Opens a map of any directory (e.g. a copy or a map of another shard) for the map editor.
    /// The map is only known in this session (<see cref="MapRegistry.AddTemporary"/>).
    /// </summary>
    public class ExternalMapDialog : Form
    {
        private readonly TextBox _textBoxFolder = new TextBox();
        private readonly Button _buttonBrowse = new Button();
        private readonly ListBox _listFiles = new ListBox();
        private readonly NumericUpDown _numericWidth = new NumericUpDown();
        private readonly NumericUpDown _numericHeight = new NumericUpDown();
        private readonly TextBox _textBoxName = new TextBox();
        private readonly Label _labelInfo = new Label();
        private readonly Button _buttonOk = new Button();
        private readonly Button _buttonCancel = new Button();
        private List<DetectedMapFile> _files = new List<DetectedMapFile>();

        public ExternalMapDialog(string folder)
        {
            Text = "Open external map";
            Icon = Options.GetFiddlerIcon();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(520, 360);

            var labelFolder = new Label { Text = "Directory with mapX.mul / staidxX.mul / staticsX.mul:", Left = 12, Top = 12, Width = 490 };
            _textBoxFolder.SetBounds(12, 32, 410, 23);
            _buttonBrowse.Text = "Browse...";
            _buttonBrowse.SetBounds(428, 31, 80, 25);
            _buttonBrowse.Click += OnClickBrowse;
            _textBoxFolder.TextChanged += (s, e) => Detect();

            var labelFiles = new Label { Text = "Maps in the directory:", Left = 12, Top = 64, Width = 300 };
            _listFiles.SetBounds(12, 84, 496, 120);
            _listFiles.SelectedIndexChanged += OnFileSelected;
            _listFiles.DoubleClick += (s, e) => OnClickOk(s, e);

            var labelSize = new Label { Text = "Size (tiles):", Left = 12, Top = 217, Width = 90 };
            _numericWidth.SetBounds(104, 214, 80, 23);
            _numericHeight.SetBounds(196, 214, 80, 23);
            foreach (NumericUpDown n in new[] { _numericWidth, _numericHeight })
            {
                n.Minimum = 8;
                n.Maximum = 65536;
                n.Increment = 8;
                n.Value = 1024;
            }

            var labelName = new Label { Text = "Name:", Left = 12, Top = 249, Width = 90 };
            _textBoxName.SetBounds(104, 246, 404, 23);

            _labelInfo.SetBounds(12, 278, 496, 36);
            _labelInfo.Text = "The size is guessed from the file size. Width and height must be a multiple of 8.";

            _buttonOk.Text = "Open";
            _buttonOk.SetBounds(332, 322, 85, 27);
            _buttonOk.Click += OnClickOk;
            _buttonCancel.Text = "Cancel";
            _buttonCancel.SetBounds(423, 322, 85, 27);
            _buttonCancel.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                labelFolder, _textBoxFolder, _buttonBrowse, labelFiles, _listFiles, labelSize, _numericWidth, _numericHeight,
                labelName, _textBoxName, _labelInfo, _buttonOk, _buttonCancel
            });
            AcceptButton = _buttonOk;
            CancelButton = _buttonCancel;

            _textBoxFolder.Text = folder ?? string.Empty;
        }

        /// <summary>The opened map (after OK)</summary>
        public MapDefinition Definition { get; private set; }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Directory of the map", SelectedPath = _textBoxFolder.Text })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _textBoxFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void Detect()
        {
            _listFiles.Items.Clear();
            _files = MapRegistry.DetectMapFiles(_textBoxFolder.Text.Trim());
            foreach (DetectedMapFile f in _files)
            {
                _listFiles.Items.Add($"{Path.GetFileName(f.FilePath)}   {f.SuggestedWidth} x {f.SuggestedHeight}" + (f.HasStatics ? "   + statics" : "   (no statics)"));
            }

            if (_listFiles.Items.Count > 0)
            {
                _listFiles.SelectedIndex = 0;
            }

            _buttonOk.Enabled = _files.Count > 0;
        }

        private void OnFileSelected(object sender, EventArgs e)
        {
            int i = _listFiles.SelectedIndex;
            if (i < 0 || i >= _files.Count)
            {
                return;
            }

            DetectedMapFile f = _files[i];
            _numericWidth.Value = Math.Max(_numericWidth.Minimum, Math.Min(_numericWidth.Maximum, f.SuggestedWidth));
            _numericHeight.Value = Math.Max(_numericHeight.Minimum, Math.Min(_numericHeight.Maximum, f.SuggestedHeight));
            string folderName = Path.GetFileName(_textBoxFolder.Text.Trim().TrimEnd('\\', '/'));
            _textBoxName.Text = $"{folderName} map{f.FileIndex}";
        }

        private void OnClickOk(object sender, EventArgs e)
        {
            int i = _listFiles.SelectedIndex;
            if (i < 0 || i >= _files.Count)
            {
                return;
            }

            int width = (int)_numericWidth.Value & ~7, height = (int)_numericHeight.Value & ~7;
            DetectedMapFile f = _files[i];
            long blocks = (long)(width >> 3) * (height >> 3);
            if (f.BlockCount > 0 && blocks > f.BlockCount &&
                MessageBox.Show($"The file has only {f.BlockCount:N0} blocks, the size needs {blocks:N0}.\nOpen anyway?", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            string name = string.IsNullOrWhiteSpace(_textBoxName.Text) ? $"External map{f.FileIndex}" : _textBoxName.Text.Trim();
            Definition = MapRegistry.AddTemporary(_textBoxFolder.Text.Trim(), f.FileIndex, width, height, name);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
