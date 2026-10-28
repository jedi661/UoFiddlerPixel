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

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Asks where the changed map files are written and whether a backup is made
    /// </summary>
    public partial class MapSaveDialog : Form
    {
        private readonly Map _map;
        private readonly string _mapName;
        private readonly string _sourceDirectory;
        private List<string> _files;
        private bool _ready;

        /// <summary>Only for the designer</summary>
        public MapSaveDialog()
        {
            InitializeComponent();
            HelpRequested += (s, e) =>
            {
                Help.MapHelpForm.ShowTopic(this, Help.MapHelpContent.Save);
                e.Handled = true;
            };
        }

        public MapSaveDialog(Map map, string mapName, int changeCount)
        {
            InitializeComponent();
            HelpRequested += (s, e) =>
            {
                Help.MapHelpForm.ShowTopic(this, Help.MapHelpContent.Save);
                e.Handled = true;
            };
            Icon = Options.GetFiddlerIcon();

            _map = map;
            _mapName = mapName;
            _sourceDirectory = MapSaveService.GetSourceDirectory(map);
            _files = MapSaveService.GetFilesToWrite(map);
            numericIndex.Value = Math.Max(numericIndex.Minimum, Math.Min(numericIndex.Maximum, map.FileIndex));

            TileMatrix tiles = map.Tiles;
            string changed = tiles.HasModifiedLand && tiles.HasStaticOverrides ? "land and statics"
                : tiles.HasModifiedLand ? "land" : tiles.HasStaticOverrides ? "statics" : "nothing";
            labelInfo.Text = $"{changeCount} unsaved change(s) of \"{mapName}\" (map{map.FileIndex}), changed: {changed}.";

            labelOutputPath.Text = string.IsNullOrEmpty(Options.OutputPath) ? "(not set in the options)" : Options.OutputPath;
            labelOriginalPath.Text = _sourceDirectory ?? "(unknown)";
            radioOriginal.Enabled = _sourceDirectory != null;
            radioOutput.Enabled = !string.IsNullOrEmpty(Options.OutputPath);
            if (!radioOutput.Enabled)
            {
                radioOther.Checked = true;
            }

            _ready = true;
            UpdateState();
        }

        /// <summary>What is written (land/statics, file number, complete set)</summary>
        public MapSaveOptions SaveOptions => new MapSaveOptions
        {
            IncludeLand = checkBoxLand.Checked,
            IncludeStatics = checkBoxStatics.Checked,
            CompleteSet = checkBoxCompleteSet.Checked,
            OutputIndex = (int)numericIndex.Value
        };

        public string TargetDirectory
        {
            get
            {
                if (radioOriginal.Checked)
                {
                    return _sourceDirectory;
                }

                return radioOther.Checked ? textBoxOther.Text.Trim() : Options.OutputPath;
            }
        }

        public bool CreateBackup => checkBoxBackup.Checked;

        public bool SaveProtocol => checkBoxProtocol.Checked;

        private void OnTargetChanged(object sender, EventArgs e)
        {
            UpdateState();
        }

        private void UpdateState()
        {
            if (_map == null || !_ready)
            {
                return;
            }

            MapSaveOptions options = SaveOptions;
            _files = MapSaveService.GetFilesToWrite(_map, options);
            labelFileNames.Text = _files.Count == 0 ? "Nothing to write." : "Files: " + string.Join(", ", _files);

            textBoxOther.Enabled = buttonBrowse.Enabled = radioOther.Checked;

            string target = TargetDirectory;
            bool replacesLoaded = target != null && MapSaveService.IsSameDirectory(target, _sourceDirectory) && options.OutputIndex == _map.FileIndex;

            var warnings = new List<string>();
            TileMatrix tiles = _map.Tiles;
            if ((!options.IncludeLand && tiles.HasModifiedLand) || (!options.IncludeStatics && tiles.HasStaticOverrides))
            {
                warnings.Add("Not all changes are written (land or statics is unchecked); the rest stays unsaved in memory.");
            }

            if (target != null && MapSaveService.IsSameDirectory(target, _sourceDirectory) && options.OutputIndex != _map.FileIndex)
            {
                warnings.Add($"Files with number {options.OutputIndex} in the map directory are replaced (backup recommended).");
            }

            if (replacesLoaded)
            {
                warnings.Add("The original files are replaced. Close the client and the server before saving.");
                if (_map.Tiles.MapPath?.EndsWith(".uop", StringComparison.OrdinalIgnoreCase) == true && _files.Contains($"map{options.OutputIndex}.mul"))
                {
                    warnings.Add($"The loaded map is a UOP file: map{_map.FileIndex}.mul is written next to it and is used instead (pack it with the UOP packer for the client).");
                }

                if (!checkBoxBackup.Checked)
                {
                    warnings.Add("Without a backup the old files can not be restored!");
                }
            }

            labelWarning.Text = string.Join("\n", warnings);
            labelBackup.Text = string.IsNullOrEmpty(target)
                ? string.Empty
                : $"Backup folder: {Path.Combine(target, MapSaveService.BackupFolderName)}";
            buttonSave.Enabled = !string.IsNullOrEmpty(target) && _files.Count > 0;
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Directory for the map files", SelectedPath = textBoxOther.Text })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxOther.Text = dialog.SelectedPath;
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            string target = TargetDirectory;
            if (string.IsNullOrEmpty(target))
            {
                return;
            }

            if (!Directory.Exists(target) &&
                MessageBox.Show($"The directory does not exist:\n{target}\n\nCreate it?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            MapSaveOptions options = SaveOptions;
            TileMatrix tiles = _map.Tiles;
            bool replacesLoaded = MapSaveService.IsSameDirectory(target, _sourceDirectory) && options.OutputIndex == _map.FileIndex;
            if (replacesLoaded && ((!options.IncludeLand && tiles.HasModifiedLand) || (!options.IncludeStatics && tiles.HasStaticOverrides)))
            {
                MessageBox.Show("When the loaded map files are replaced, the map is read again.\n" +
                                "Please save land and statics together here, otherwise the unsaved part is lost.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (replacesLoaded &&
                MessageBox.Show("The original map files will be replaced.\nAre the client and the server closed?", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnClickRestore(object sender, EventArgs e)
        {
            using (var form = new MapBackupForm(_mapName, new[] { Options.OutputPath, _sourceDirectory, textBoxOther.Text }))
            {
                form.ShowDialog(this);
                if (form.Restored)
                {
                    DialogResult = DialogResult.Abort;
                    Close();
                }
            }
        }
    }
}
