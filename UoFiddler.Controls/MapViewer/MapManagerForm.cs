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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Add, edit, hide and remove maps of <see cref="MapRegistry"/>.
    /// </summary>
    public partial class MapManagerForm : Form
    {
        private bool _updating;
        private MapDefinition _selected;

        public MapManagerForm()
        {
            InitializeComponent();
            HelpRequested += (s, e) =>
            {
                Help.MapHelpForm.ShowTopic(this, Help.MapHelpContent.ManageMaps);
                e.Handled = true;
            };
            Icon = Options.GetFiddlerIcon();

            MapRegistryStore.EnsureLoaded();
            FillList(null);
            SelectDefinition(MapRegistry.Definitions.FirstOrDefault());
        }

        #region List

        private void FillList(MapDefinition select)
        {
            _updating = true;
            listViewMaps.BeginUpdate();
            try
            {
                listViewMaps.Items.Clear();
                foreach (MapDefinition def in MapRegistry.Definitions)
                {
                    bool exists = def.FilesExist();
                    var item = new ListViewItem(string.Empty)
                    {
                        Checked = def.Visible,
                        Tag = def,
                        ForeColor = exists ? SystemColors.WindowText : Color.Firebrick
                    };
                    item.SubItems.Add(def.Id.ToString());
                    item.SubItems.Add(def.Name);
                    item.SubItems.Add($"map{def.FileIndex}");
                    item.SubItems.Add($"{def.Width}x{def.Height}");
                    item.SubItems.Add(def.IsBuiltIn ? "built-in" : def.UsesCustomPath ? def.CustomPath : "client");
                    item.SubItems.Add(exists ? "ok" : "missing");
                    item.ToolTipText = def.UsesCustomPath ? def.CustomPath : Files.RootDir;
                    listViewMaps.Items.Add(item);

                    if (def == select)
                    {
                        item.Selected = true;
                        item.EnsureVisible();
                    }
                }
            }
            finally
            {
                listViewMaps.EndUpdate();
                _updating = false;
            }
        }

        private void OnMapSelectionChanged(object sender, EventArgs e)
        {
            if (_updating || listViewMaps.SelectedItems.Count == 0)
            {
                return;
            }

            SelectDefinition(listViewMaps.SelectedItems[0].Tag as MapDefinition);
        }

        private void OnMapItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_updating || e.Item.Tag is not MapDefinition def)
            {
                return;
            }

            if (!e.Item.Checked && MapRegistry.Definitions.Count(d => d.Visible) <= 1)
            {
                // at least one map has to stay visible
                _updating = true;
                e.Item.Checked = true;
                _updating = false;
                return;
            }

            def.Visible = e.Item.Checked;
            if (def == _selected)
            {
                checkBoxVisible.Checked = def.Visible;
            }

            Commit(def);
        }

        #endregion

        #region Details

        private void SelectDefinition(MapDefinition def)
        {
            _selected = def;
            bool isNew = def == null;
            bool builtIn = def?.IsBuiltIn == true;

            numericId.Value = isNew ? MapRegistry.NextFreeId() : def.Id;
            textBoxName.Text = isNew ? $"Map {numericId.Value}" : def.Name;
            numericFileIndex.Value = isNew ? Math.Min(numericFileIndex.Maximum, numericId.Value) : def.FileIndex;
            numericWidth.Value = Clamp(numericWidth, isNew ? 6144 : def.Width);
            numericHeight.Value = Clamp(numericHeight, isNew ? 4096 : def.Height);
            radioCustom.Checked = !isNew && def.UsesCustomPath;
            radioClient.Checked = !radioCustom.Checked;
            textBoxPath.Text = isNew ? string.Empty : def.CustomPath ?? string.Empty;
            checkBoxVisible.Checked = isNew || def.Visible;

            numericId.Enabled = isNew;
            numericFileIndex.Enabled = !builtIn;
            numericWidth.Enabled = !builtIn;
            numericHeight.Enabled = !builtIn;
            buttonGuessSize.Enabled = !builtIn;
            radioClient.Enabled = !builtIn;
            radioCustom.Enabled = !builtIn;
            buttonRemove.Enabled = !isNew && !builtIn;
            buttonApply.Text = isNew ? "Add" : "Apply";
            UpdatePathControls();

            labelInfo.Text = isNew
                ? "New map: enter the values and click \"Add\"."
                : builtIn
                    ? "Built-in map: only the name and the visibility can be changed. Size and files are set in the options."
                    : BuildFileInfo(def);
        }

        private static string BuildFileInfo(MapDefinition def)
        {
            try
            {
                Map map = def.GetMap();
                TileMatrix tiles = map.Tiles;
                return $"Map: {tiles.MapPath ?? "not found"}\nStaidx: {tiles.IndexPath ?? "not found"}\nStatics: {tiles.StaticsPath ?? "not found"}";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static decimal Clamp(NumericUpDown control, int value)
        {
            return Math.Max(control.Minimum, Math.Min(control.Maximum, value));
        }

        private void OnSourceChanged(object sender, EventArgs e)
        {
            UpdatePathControls();
        }

        private void UpdatePathControls()
        {
            bool custom = radioCustom.Checked && radioCustom.Enabled;
            textBoxPath.Enabled = custom;
            buttonBrowse.Enabled = custom;
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            string path = BrowseDirectory(textBoxPath.Text);
            if (path != null)
            {
                textBoxPath.Text = path;
            }
        }

        private static string BrowseDirectory(string start)
        {
            using (var dialog = new FolderBrowserDialog
            {
                Description = "Select the directory with the map files",
                ShowNewFolderButton = false,
                SelectedPath = Directory.Exists(start) ? start : Files.RootDir ?? string.Empty
            })
            {
                return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
            }
        }

        private void OnClickGuessSize(object sender, EventArgs e)
        {
            string directory = radioCustom.Checked ? textBoxPath.Text : Files.RootDir;
            int index = (int)numericFileIndex.Value;
            DetectedMapFile file = MapRegistry.DetectMapFiles(directory).FirstOrDefault(f => f.FileIndex == index);
            if (file == null)
            {
                MessageBox.Show($"map{index}.mul / map{index}LegacyMUL.uop not found in\n{directory}", "Manage Maps", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            numericWidth.Value = Clamp(numericWidth, file.SuggestedWidth);
            numericHeight.Value = Clamp(numericHeight, file.SuggestedHeight);
        }

        private void OnClickNew(object sender, EventArgs e)
        {
            listViewMaps.SelectedItems.Clear();
            SelectDefinition(null);
            textBoxName.Focus();
            textBoxName.SelectAll();
        }

        private void OnClickApply(object sender, EventArgs e)
        {
            string name = textBoxName.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show("Please enter a name.", "Manage Maps", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (radioCustom.Checked && radioCustom.Enabled && !Directory.Exists(textBoxPath.Text))
            {
                MessageBox.Show("The directory does not exist.", "Manage Maps", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int width = ((int)numericWidth.Value >> 3) << 3;
            int height = ((int)numericHeight.Value >> 3) << 3;

            if (_selected == null)
            {
                int id = (int)numericId.Value;
                if (MapRegistry.GetById(id) != null)
                {
                    MessageBox.Show($"Map id {id} is already used.", "Manage Maps", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var def = new MapDefinition
                {
                    Id = id,
                    Name = name,
                    FileIndex = (int)numericFileIndex.Value,
                    Width = width,
                    Height = height,
                    CustomPath = radioCustom.Checked ? textBoxPath.Text : null,
                    Visible = checkBoxVisible.Checked
                };

                MapRegistry.Add(def);
                MapRegistryStore.Save();
                FillList(def);
                SelectDefinition(def);
                return;
            }

            _selected.Name = name;
            _selected.Visible = checkBoxVisible.Checked || MapRegistry.Definitions.Count(d => d.Visible && d != _selected) == 0;
            if (!_selected.IsBuiltIn)
            {
                _selected.FileIndex = (int)numericFileIndex.Value;
                _selected.Width = width;
                _selected.Height = height;
                _selected.CustomPath = radioCustom.Checked ? textBoxPath.Text : null;
            }

            Commit(_selected);
            FillList(_selected);
            SelectDefinition(_selected);
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (_selected == null || _selected.IsBuiltIn)
            {
                return;
            }

            if (MessageBox.Show($"Remove \"{_selected.Name}\" from the map list?\nThe files are not deleted. Markers of this map are kept.",
                    "Manage Maps", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            MapRegistry.Remove(_selected);
            MapRegistryStore.Save();
            FillList(null);
            SelectDefinition(MapRegistry.Definitions.FirstOrDefault());
        }

        private static void Commit(MapDefinition def)
        {
            MapRegistry.Update(def);
            MapRegistryStore.Save();
        }

        #endregion

        #region Detect

        private void OnClickDetectBrowse(object sender, EventArgs e)
        {
            string path = BrowseDirectory(textBoxDetectPath.Text);
            if (path != null)
            {
                Detect(path);
            }
        }

        private void OnClickDetectClient(object sender, EventArgs e)
        {
            Detect(Files.RootDir);
        }

        private void Detect(string directory)
        {
            textBoxDetectPath.Text = directory ?? string.Empty;
            listViewDetected.BeginUpdate();
            try
            {
                listViewDetected.Items.Clear();
                foreach (DetectedMapFile file in MapRegistry.DetectMapFiles(directory))
                {
                    bool known = IsKnown(file, directory);
                    var item = new ListViewItem(Path.GetFileName(file.FilePath))
                    {
                        Tag = file,
                        Checked = !known,
                        ForeColor = known ? SystemColors.GrayText : SystemColors.WindowText
                    };
                    item.SubItems.Add(file.FileIndex.ToString());
                    item.SubItems.Add($"{file.SuggestedWidth}x{file.SuggestedHeight}");
                    item.SubItems.Add(file.HasStatics ? "yes" : "no");
                    item.SubItems.Add(known ? "yes" : "no");
                    listViewDetected.Items.Add(item);
                }
            }
            finally
            {
                listViewDetected.EndUpdate();
            }

            if (listViewDetected.Items.Count == 0)
            {
                MessageBox.Show("No map files found.", "Manage Maps", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static bool IsClientDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(Files.RootDir))
            {
                return false;
            }

            return string.Equals(Path.GetFullPath(directory).TrimEnd('\\', '/'),
                Path.GetFullPath(Files.RootDir).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsKnown(DetectedMapFile file, string directory)
        {
            bool client = IsClientDirectory(directory);
            return MapRegistry.Definitions.Any(d =>
                d.FileIndex == file.FileIndex &&
                (client ? !d.UsesCustomPath : d.UsesCustomPath && IsSameDirectory(d.CustomPath, directory)));
        }

        private static bool IsSameDirectory(string a, string b)
        {
            try
            {
                return string.Equals(Path.GetFullPath(a).TrimEnd('\\', '/'), Path.GetFullPath(b).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void OnClickAddDetected(object sender, EventArgs e)
        {
            string directory = textBoxDetectPath.Text;
            bool client = IsClientDirectory(directory);
            var added = new List<MapDefinition>();

            foreach (ListViewItem item in listViewDetected.CheckedItems)
            {
                if (item.Tag is not DetectedMapFile file)
                {
                    continue;
                }

                var def = new MapDefinition
                {
                    Id = MapRegistry.NextFreeId(),
                    Name = client ? $"Map {file.FileIndex}" : $"{new DirectoryInfo(directory).Name} map{file.FileIndex}",
                    FileIndex = file.FileIndex,
                    Width = file.SuggestedWidth,
                    Height = file.SuggestedHeight,
                    CustomPath = client ? null : directory,
                    Visible = true
                };

                MapRegistry.Add(def);
                added.Add(def);
            }

            if (added.Count == 0)
            {
                return;
            }

            MapRegistryStore.Save();
            FillList(added[0]);
            SelectDefinition(added[0]);
            tabControl.SelectedTab = tabPageDetails;
            Detect(directory);

            MessageBox.Show($"{added.Count} map(s) added. Please check the size of each map.", "Manage Maps", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        private void OnClickClose(object sender, EventArgs e)
        {
            Close();
        }
    }
}
