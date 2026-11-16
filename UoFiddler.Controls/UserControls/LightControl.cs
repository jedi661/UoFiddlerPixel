/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Light tab: lights of light.mul with names, search, overview, preview
 * (alone or like in the game with ground and light source item), export /
 * import, more than 100 entries, light designer for new lights.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class LightControl : UserControl
    {
        private bool _loaded;
        private int _landTile = 0x3;
        private int _lightTile = 0x0B20;
        private bool _renaming;

        public LightControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        private void UnhookEvents()
        {
            ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
        }

        private int? SelectedLight => treeViewLights.SelectedNode?.Tag is int i ? i : (int?)null;

        #region Load

        private void Reload()
        {
            if (_loaded)
            {
                Light.Reload();
                OnLoad(this, EventArgs.Empty);
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Light"] = true;

            if (!_loaded)
            {
                _txtLand.Text = $"0x{_landTile:X4}";
                _txtItem.Text = $"0x{_lightTile:X4}";
                _cmbZoom.SelectedIndex = 0;
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            BuildTree(null);
            UpdateTestTiles();
            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent() => Reload();

        /// <summary>fills the list (search: id, 0x…, range or part of the name)</summary>
        private void BuildTree(int? select)
        {
            int? keep = select ?? SelectedLight;
            string filter = _txtSearch.Text.Trim();
            HashSet<int> ids = null;
            if (filter.Length > 0 && filter.Any(char.IsDigit) && TileDataBatch.TryParseRanges(filter, Light.Count, out List<int> list, out _) && list.Count > 0)
            {
                ids = new HashSet<int>(list);
            }

            treeViewLights.BeginUpdate();
            try
            {
                treeViewLights.Nodes.Clear();
                int count = 0;
                for (int i = 0; i < Light.Count; ++i)
                {
                    if (!Light.TestLight(i))
                    {
                        continue;
                    }

                    count++;
                    string name = LightNames.Get(i) ?? string.Empty;
                    if (filter.Length > 0 && !(ids?.Contains(i) ?? false) && name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    treeViewLights.Nodes.Add(new TreeNode(LightNames.Display(i)) { Tag = i, ToolTipText = name });
                }

                int fileCount = Light.GetFileCount();
                _lblCount.Text = $"{count} lights · {Light.Count} entries" + (fileCount != Light.Count ? $" (file: {fileCount})" : string.Empty);
                _txtCount.Text = Light.Count.ToString(CultureInfo.InvariantCulture);
            }
            finally
            {
                treeViewLights.EndUpdate();
            }

            TreeNode node = keep.HasValue ? treeViewLights.Nodes.Cast<TreeNode>().FirstOrDefault(n => (int)n.Tag == keep.Value) : null;
            treeViewLights.SelectedNode = node ?? (treeViewLights.Nodes.Count > 0 ? treeViewLights.Nodes[0] : null);
            if (treeViewLights.SelectedNode == null)
            {
                UpdatePreview();
            }
        }

        private void OnSearchChanged(object sender, EventArgs e) => BuildTree(null);

        #endregion

        #region Preview

        private void AfterSelect(object sender, TreeViewEventArgs e) => UpdatePreview();

        private void OnPictureSizeChanged(object sender, EventArgs e)
        {
            if (_loaded && _chkIgPreview.Checked)
            {
                UpdatePreview();
            }
        }

        private void OnPreviewOptionChanged(object sender, EventArgs e)
        {
            iGPreviewToolStripMenuItem.Checked = _chkIgPreview.Checked;
            if (_loaded)
            {
                UpdatePreview();
            }
        }

        private void OnClickIgPreview(object sender, EventArgs e) => _chkIgPreview.Checked = !_chkIgPreview.Checked;

        private LightImage SelectedImage()
        {
            if (!SelectedLight.HasValue)
            {
                return null;
            }

            byte[] data = Light.GetRawLight(SelectedLight.Value, out int w, out int h);
            return data == null ? null : new LightImage(w, h, (byte[])data.Clone());
        }

        private void UpdatePreview()
        {
            LightImage light = SelectedImage();
            Image old = pictureBoxPreview.Image;
            pictureBoxPreview.Image = light == null ? null : BuildPreview(light);
            old?.Dispose();

            if (light == null)
            {
                _lblInfo.Text = "-";
                return;
            }

            int id = SelectedLight.Value;
            string name = LightNames.Get(id);
            _lblInfo.Text = $"Light {id} (0x{id:X2}){(string.IsNullOrEmpty(name) ? string.Empty : " – " + name)}   ·   {light.Width} × {light.Height}" +
                            $"   ·   brightest {light.MaxValue}/31";
        }

        private Bitmap BuildPreview(LightImage light)
        {
            Bitmap image;
            if (_chkIgPreview.Checked)
            {
                var options = new LightPreviewOptions
                {
                    LandId = _landTile,
                    ItemId = _lightTile,
                    Darkness = (int)_numDark.Value,
                    LightOffsetX = (int)_numLightX.Value,
                    LightOffsetY = (int)_numLightY.Value
                };
                Size size = pictureBoxPreview.ClientSize;
                image = LightPreview.RenderScene(new Size(Math.Max(100, size.Width), Math.Max(100, size.Height)), light, options);
                return image;
            }

            image = Light.ToBitmap(light.Data, light.Width, light.Height);
            int zoom = _cmbZoom.SelectedIndex + 1;
            if (_cmbZoom.SelectedIndex == 3)
            {
                Size area = pictureBoxPreview.ClientSize;
                zoom = Math.Max(1, Math.Min(area.Width / Math.Max(1, light.Width), area.Height / Math.Max(1, light.Height)));
            }

            if (zoom <= 1)
            {
                return image;
            }

            var scaled = new Bitmap(light.Width * zoom, light.Height * zoom);
            using (Graphics g = Graphics.FromImage(scaled))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(image, new Rectangle(0, 0, scaled.Width, scaled.Height));
            }

            image.Dispose();
            return scaled;
        }

        private void OnClickCopyPreview(object sender, EventArgs e)
        {
            if (pictureBoxPreview.Image != null)
            {
                Clipboard.SetImage(pictureBoxPreview.Image);
            }
        }

        private void OnClickSavePreview(object sender, EventArgs e)
        {
            if (pictureBoxPreview.Image == null)
            {
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = "PNG (*.png)|*.png", FileName = $"Light {SelectedLight} preview.png" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    pictureBoxPreview.Image.Save(dialog.FileName, ImageFormat.Png);
                }
            }
        }

        #endregion

        #region Ground and light source item (◀ ▶, overview)

        private static bool TryParseId(string text, out int id) => TileDataBatch.TryParseId(text, out id);

        private void UpdateTestTiles()
        {
            _picLand.Image = Art.IsValidLand(_landTile) ? Art.GetLand(_landTile) : null;
            _picItem.Image = Art.IsValidStatic(_lightTile) ? Art.GetStatic(_lightTile) : null;
            _txtLand.Text = $"0x{_landTile:X4}";
            _txtItem.Text = $"0x{_lightTile:X4}";
            string itemName = TileData.ItemTable != null && _lightTile < TileData.ItemTable.Length ? TileData.ItemTable[_lightTile].Name : string.Empty;
            string landName = TileData.LandTable != null && _landTile < TileData.LandTable.Length ? TileData.LandTable[_landTile].Name : string.Empty;
            _txtItem.ForeColor = Art.IsValidStatic(_lightTile) ? SystemColors.WindowText : Color.Red;
            _txtLand.ForeColor = Art.IsValidLand(_landTile) ? SystemColors.WindowText : Color.Red;
            _lblItem.Text = $"Light source: {itemName}";
            _lblLand.Text = $"Ground: {landName}";
            if (_loaded && _chkIgPreview.Checked)
            {
                UpdatePreview();
            }
        }

        private void SetLand(int id)
        {
            if (id >= 0 && id < 0x4000)
            {
                _landTile = id;
                _chkIgPreview.Checked = true;
                UpdateTestTiles();
            }
        }

        private void SetItem(int id)
        {
            if (id >= 0 && id <= Art.GetMaxItemId())
            {
                _lightTile = id;
                _chkIgPreview.Checked = true;
                UpdateTestTiles();
            }
        }

        private void OnLandTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                OnLandTextLeave(sender, e);
            }
        }

        private void OnLandTextLeave(object sender, EventArgs e)
        {
            if (TryParseId(_txtLand.Text, out int id) && id != _landTile)
            {
                SetLand(id);
            }
        }

        private void OnItemTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                OnItemTextLeave(sender, e);
            }
        }

        private void OnItemTextLeave(object sender, EventArgs e)
        {
            if (TryParseId(_txtItem.Text, out int id) && id != _lightTile)
            {
                SetItem(id);
            }
        }

        private void OnClickLandPrev(object sender, EventArgs e) => SetLand(NextLand(_landTile, -1));

        private void OnClickLandNext(object sender, EventArgs e) => SetLand(NextLand(_landTile, 1));

        private static int NextLand(int from, int dir)
        {
            for (int i = from + dir; i >= 0 && i < 0x4000; i += dir)
            {
                if (Art.IsValidLand(i))
                {
                    return i;
                }
            }

            return from;
        }

        private void OnClickItemPrev(object sender, EventArgs e) => SetItem(NextItem(_lightTile, -1));

        private void OnClickItemNext(object sender, EventArgs e) => SetItem(NextItem(_lightTile, 1));

        /// <summary>next valid item (only light sources when ticked)</summary>
        private int NextItem(int from, int dir)
        {
            int max = Art.GetMaxItemId();
            bool onlyLight = _chkOnlyLightItems.Checked && TileData.ItemTable != null;
            for (int i = from + dir; i >= 0 && i <= max; i += dir)
            {
                if (!Art.IsValidStatic(i))
                {
                    continue;
                }

                if (onlyLight && (i >= TileData.ItemTable.Length || (TileData.ItemTable[i].Flags & TileFlag.LightSource) == 0))
                {
                    continue;
                }

                return i;
            }

            return from;
        }

        private void OnClickLandPick(object sender, EventArgs e)
        {
            using (var picker = new TilePickerForm(TilePickerMode.Land, _landTile, false))
            {
                if (picker.ShowDialog(this) == DialogResult.OK)
                {
                    SetLand(picker.SelectedId);
                }
            }
        }

        private void OnClickItemPick(object sender, EventArgs e)
        {
            using (var picker = new TilePickerForm(TilePickerMode.Item, _lightTile, _chkOnlyLightItems.Checked))
            {
                if (picker.ShowDialog(this) == DialogResult.OK)
                {
                    SetItem(picker.SelectedId);
                }
            }
        }

        private void OnClickOverview(object sender, EventArgs e)
        {
            using (var picker = new TilePickerForm(TilePickerMode.Light, SelectedLight ?? -1, false))
            {
                if (picker.ShowDialog(this) == DialogResult.OK)
                {
                    _txtSearch.Text = string.Empty;
                    BuildTree(picker.SelectedId);
                }
            }
        }

        #endregion

        #region Names

        private void OnClickRename(object sender, EventArgs e) => StartRename(treeViewLights.SelectedNode);

        private void StartRename(TreeNode node)
        {
            if (node == null)
            {
                return;
            }

            _renaming = true;
            node.Text = LightNames.Get((int)node.Tag) ?? string.Empty;
            node.BeginEdit();
        }

        private void OnBeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            // only the name is edited (not "12 (0x0C) – …")
            if (!_renaming)
            {
                e.CancelEdit = true;
                TreeNode node = e.Node;
                BeginInvoke(new Action(() => StartRename(node)));
            }
        }

        private void OnAfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            _renaming = false;
            int id = (int)e.Node.Tag;
            if (e.Label != null)
            {
                LightNames.Set(id, e.Label);
            }

            // the tree puts the edited text back after this event – the display text is set afterwards
            e.CancelEdit = true;
            TreeNode node = e.Node;
            BeginInvoke(new Action(() =>
            {
                node.Text = LightNames.Display(id);
                UpdatePreview();
            }));
        }

        private void OnTreeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                StartRename(treeViewLights.SelectedNode);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                OnClickRemove(sender, e);
                e.Handled = true;
            }
        }

        private void OnClickCopyId(object sender, EventArgs e)
        {
            if (SelectedLight.HasValue)
            {
                Clipboard.SetText(SelectedLight.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void OnTreeMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool sel = SelectedLight.HasValue;
            foreach (ToolStripItem item in new ToolStripItem[]
                     {
                         asPngToolStripMenuItem, asBmpToolStripMenuItem, asTiffToolStripMenuItem, asJpgToolStripMenuItem, renameToolStripMenuItem,
                         copyIdToolStripMenuItem, removeToolStripMenuItem, replaceToolStripMenuItem, copyToolStripMenuItem, editToolStripMenuItem, sizeToolStripMenuItem
                     })
            {
                item.Enabled = sel;
            }

            importToolStripMenuItem.Enabled = Clipboard.ContainsImage();
        }

        #endregion

        #region Remove, replace, insert, clipboard

        private void LightChanged(int id)
        {
            Options.ChangedUltimaClass["Light"] = true;
            _txtSearch.Text = string.Empty;
            BuildTree(id);
            UpdatePreview();
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (!SelectedLight.HasValue)
            {
                return;
            }

            int i = SelectedLight.Value;
            if (MessageBox.Show(string.Format("Are you sure to remove {0} (0x{0:X})", i), "Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            int index = treeViewLights.SelectedNode.Index;
            Light.Remove(i);
            Options.ChangedUltimaClass["Light"] = true;
            treeViewLights.Nodes.Remove(treeViewLights.SelectedNode);
            if (treeViewLights.Nodes.Count > 0)
            {
                treeViewLights.SelectedNode = treeViewLights.Nodes[Math.Min(index, treeViewLights.Nodes.Count - 1)];
            }
            else
            {
                UpdatePreview();
            }
        }

        private static Bitmap LoadImageFile(string fileName)
        {
            using (var temp = new Bitmap(fileName))
            {
                return new Bitmap(temp);
            }
        }

        private string AskImageFile(string title)
        {
            using (var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = title,
                CheckFileExists = true,
                Filter = "Image files (*.png;*.bmp;*.tif;*.tiff)|*.png;*.bmp;*.tif;*.tiff"
            })
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
            }
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (!SelectedLight.HasValue)
            {
                return;
            }

            string file = AskImageFile("Choose image file to replace");
            if (file == null)
            {
                return;
            }

            using (Bitmap bmp = LoadImageFile(file))
            {
                Light.Replace(SelectedLight.Value, bmp);
            }

            LightChanged(SelectedLight.Value);
        }

        private bool TryReadTargetId(string text, out int id)
        {
            if (!TryParseId(text, out id) || id < 0 || id >= Light.MaxCount)
            {
                MessageBox.Show($"Please enter an ID between 0 and {Light.MaxCount - 1} (decimal or 0x…).", "Light");
                return false;
            }

            if (id >= Light.Count &&
                MessageBox.Show($"ID {id} is behind the end of the file ({Light.Count} entries).\nThe file is extended to {id + 1} entries. Continue?",
                    "Light", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return false;
            }

            return !Light.TestLight(id) ||
                   MessageBox.Show("A light with this ID already exists. Do you want to replace it?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            InsertText.ForeColor = TryParseId(InsertText.Text, out int index) && index >= 0 && index < Light.MaxCount
                ? (Light.TestLight(index) ? Color.Red : Color.Black)
                : Color.Red;
        }

        private void OnKeyDownInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            if (!TryReadTargetId(InsertText.Text, out int index))
            {
                return;
            }

            treeViewContextMenuStrip.Close();
            string file = AskImageFile($"Select an image file to insert at {index} (0x{index:X})");
            if (file == null)
            {
                return;
            }

            using (Bitmap bmp = LoadImageFile(file))
            {
                Light.Replace(index, bmp);
            }

            LightChanged(index);
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (!SelectedLight.HasValue)
            {
                return;
            }

            Bitmap bitmap = Light.GetLight(SelectedLight.Value);
            if (bitmap == null)
            {
                MessageBox.Show("No image to copy!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Clipboard.SetImage(new Bitmap(bitmap));
        }

        private void OnClickImportClipboard(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("There is no image in the clipboard.", "Import");
                return;
            }

            int index;
            if (!string.IsNullOrWhiteSpace(toolStripTextBoxInsertImport.Text))
            {
                if (!TryReadTargetId(toolStripTextBoxInsertImport.Text, out index))
                {
                    return;
                }
            }
            else if (SelectedLight.HasValue)
            {
                index = SelectedLight.Value;
                if (MessageBox.Show($"Replace light {index} with the image of the clipboard?", "Import", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }
            else
            {
                MessageBox.Show("Please select a light or enter an ID.");
                return;
            }

            treeViewContextMenuStrip.Close();
            using (var clipboardImage = new Bitmap(Clipboard.GetImage()))
            {
                // the brightness of the image becomes the light value (black = no light)
                Light.Replace(index, clipboardImage);
            }

            LightChanged(index);
        }

        private void OnKeyDownImportClipboard(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                OnClickImportClipboard(sender, e);
            }
        }

        #endregion

        #region Export / save / entries

        private void Export(ImageFormat format, string extension)
        {
            if (!SelectedLight.HasValue)
            {
                return;
            }

            int i = SelectedLight.Value;
            string fileName = Path.Combine(Options.OutputPath, $"Light {i}.{extension}");
            Light.GetLight(i).Save(fileName, format);
            MessageBox.Show($"Light saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportPng(object sender, EventArgs e) => Export(ImageFormat.Png, "png");

        private void OnClickExportBmp(object sender, EventArgs e) => Export(ImageFormat.Bmp, "bmp");

        private void OnClickExportTiff(object sender, EventArgs e) => Export(ImageFormat.Tiff, "tiff");

        private void OnClickExportJpg(object sender, EventArgs e) => Export(ImageFormat.Jpeg, "jpg");

        private void OnClickExportAll(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { SelectedPath = Options.OutputPath, Description = "Folder for the light images" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                int count = 0;
                for (int i = 0; i < Light.Count; i++)
                {
                    Bitmap bmp = Light.TestLight(i) ? Light.GetLight(i) : null;
                    if (bmp == null)
                    {
                        continue;
                    }

                    string name = LightNames.Get(i);
                    string file = Path.Combine(dialog.SelectedPath, $"Light {i}{(string.IsNullOrEmpty(name) ? string.Empty : " " + LightLibrary.SafeFileName(name))}.png");
                    bmp.Save(file, ImageFormat.Png);
                    count++;
                }

                MessageBox.Show($"{count} lights saved to {dialog.SelectedPath}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Light.Save(Options.OutputPath);
            string note = Light.Count > Light.DefaultCount
                ? $"\n\nThe files have {Light.Count} entries. ClassicUO reads all entries of lightidx.mul; other clients may only use the first {Light.DefaultCount}."
                : string.Empty;
            MessageBox.Show($"Saved to {Options.OutputPath}{note}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Light"] = false;
        }

        /// <summary>more (or fewer) entries in lightidx.mul</summary>
        private void OnClickSetCount(object sender, EventArgs e)
        {
            if (!int.TryParse(_txtCount.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int count) || count < 1 || count > Light.MaxCount)
            {
                MessageBox.Show($"Please enter a number between 1 and {Light.MaxCount}.", "Entries");
                return;
            }

            if (count == Light.Count)
            {
                return;
            }

            int lost = Enumerable.Range(count, Math.Max(0, Light.Count - count)).Count(Light.TestLight);
            string text = count > Light.Count
                ? $"Extend the light files from {Light.Count} to {count} entries?\n\nThe new entries are empty – insert lights there (Insert at ID, light designer).\n" +
                  "ClassicUO reads all entries of lightidx.mul. The light id of an item is set in the tiledata."
                : $"Shorten the light files from {Light.Count} to {count} entries?" + (lost > 0 ? $"\n\n{lost} light(s) at the end are removed!" : string.Empty);
            if (MessageBox.Show(text, "Entries", MessageBoxButtons.YesNo, lost > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Light.SetCount(count);
            Options.ChangedUltimaClass["Light"] = true;
            BuildTree(null);
        }

        #endregion

        #region Light designer

        private int FirstFreeId()
        {
            for (int i = 0; i < Light.Count; i++)
            {
                if (!Light.TestLight(i))
                {
                    return i;
                }
            }

            return Light.Count;
        }

        private void OnClickNewLight(object sender, EventArgs e) => OpenDesigner(null, FirstFreeId());

        private void OnClickEditLight(object sender, EventArgs e)
        {
            LightImage image = SelectedImage();
            if (image == null)
            {
                OpenDesigner(null, FirstFreeId());
                return;
            }

            OpenDesigner(image, SelectedLight.Value);
        }

        private void OpenDesigner(LightImage image, int target)
        {
            var form = new LightDesignerForm(image, target, id => LightChanged(id));
            form.SetTestTiles(_landTile, _lightTile);
            form.Show(FindForm());
        }

        #endregion
    }
}
