/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.UserControls.TileView;

namespace UoFiddler.Controls.UserControls
{
    public partial class ItemsControl : UserControl
    {
        private TileDataControl tileDataControl = new TileDataControl(); //Refesh image pictureBoxItem TiledataControl
        public ItemsControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            RefMarker = this;
            DetailTextBox.AddBasicContextMenu();
        }

        private List<int> _itemList = new List<int>();
        private bool _showFreeSlots;

        private int _selectedGraphicId = -1;

        #region SelectedGraphicId
        public int SelectedGraphicId
        {
            get => _selectedGraphicId;
            set
            {
                _selectedGraphicId = value < 0 ? 0 : value;
                ItemsTileView.FocusIndex = _itemList.Count == 0 ? -1 : _itemList.IndexOf(_selectedGraphicId);

                UpdateToolStripLabels(_selectedGraphicId);
                UpdateDetail(_selectedGraphicId);
            }
        }
        #endregion

        public IReadOnlyList<int> ItemList { get => _itemList.AsReadOnly(); }
        public static ItemsControl RefMarker { get; private set; }
        public static TileViewControl TileView => RefMarker.ItemsTileView;
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Updates if TileSize is changed
        /// </summary>
        /// 
        #region UpdateTileView
        public void UpdateTileView()
        {
            var newSize = new Size(Options.ArtItemSizeWidth, Options.ArtItemSizeHeight);

            ItemsTileView.TileBorderColor = Options.RemoveTileBorder
                ? Color.Transparent
                : Color.Gray;

            if (Options.OverrideBackgroundColorFromTile)
            {
                ItemsTileView.BackColor = _backgroundColorItem;
            }

            var sameTileSize = ItemsTileView.TileSize == newSize;
            var sameFocusColor = ItemsTileView.TileFocusColor == Options.TileFocusColor;
            var sameSelectionColor = ItemsTileView.TileHighlightColor == Options.TileSelectionColor;
            if (sameTileSize && sameFocusColor && sameSelectionColor)
            {
                return;
            }

            ItemsTileView.TileFocusColor = Options.TileFocusColor;
            ItemsTileView.TileHighlightColor = Options.TileSelectionColor;

            ItemsTileView.TileSize = newSize;
            ItemsTileView.Invalidate();

            if (_selectedGraphicId != -1)
            {
                UpdateDetail(_selectedGraphicId);
            }
        }
        #endregion

        /// <summary>
        /// Searches graphic number and selects it
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        /// 
        #region SearchGraphic
        public static bool SearchGraphic(int graphic)
        {
            if (!RefMarker.IsLoaded)
            {
                RefMarker.OnLoad(RefMarker, EventArgs.Empty);
            }

            if (RefMarker._itemList.All(t => t != graphic))
            {
                return false;
            }

            // we have to invalidate focus so it will scroll to item
            RefMarker.ItemsTileView.FocusIndex = -1;
            RefMarker.SelectedGraphicId = graphic;

            return true;
        }
        #endregion

        /// <summary>
        /// Searches for name and selects
        /// </summary>
        /// <param name="name"></param>
        /// <param name="next">starting from current selected</param>
        /// <returns></returns>
        /// 
        #region SearchName
        public static bool SearchName(string name, bool next)
        {
            int index = 0;
            if (next)
            {
                if (RefMarker._selectedGraphicId >= 0)
                {
                    index = RefMarker._itemList.IndexOf(RefMarker._selectedGraphicId) + 1;
                }

                if (index >= RefMarker._itemList.Count)
                {
                    index = 0;
                }
            }

            var searchMethod = SearchHelper.GetSearchMethod();

            for (int i = index; i < RefMarker._itemList.Count; ++i)
            {
                var searchResult = searchMethod(name, TileData.ItemTable[RefMarker._itemList[i]].Name);
                if (searchResult.HasErrors)
                {
                    break;
                }

                if (!searchResult.EntryFound)
                {
                    continue;
                }

                // we have to invalidate focus so it will scroll to item
                RefMarker.ItemsTileView.FocusIndex = -1;
                RefMarker.SelectedGraphicId = RefMarker._itemList[i];

                return true;
            }

            return false;
        }
        #endregion

        #region OnLoad
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

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["Animdata"] = true;
            Options.LoadedUltimaClass["Hues"] = true;

            if (!IsLoaded) // only once
            {
                Plugin.PluginEvents.FireModifyItemShowContextMenuEvent(TileViewContextMenuStrip);
            }

            UpdateTileView();

            _showFreeSlots = false;
            showFreeSlotsToolStripMenuItem.Checked = false;

            var prevSelected = SelectedGraphicId;

            int staticLength = Art.GetMaxItemId();
            _itemList = new List<int>(staticLength);
            for (int i = 0; i <= staticLength; ++i)
            {
                if (Art.IsValidStatic(i))
                {
                    _itemList.Add(i);
                }
            }

            ItemsTileView.VirtualListSize = _itemList.Count;

            if (prevSelected >= 0)
            {
                SelectedGraphicId = _itemList.Contains(prevSelected) ? prevSelected : 0;
            }

            if (!IsLoaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.ItemChangeEvent += OnItemChangeEvent;
                ControlEvents.TileDataChangeEvent += OnTileDataChangeEvent;
            }

            IsLoaded = true;
            Cursor.Current = Cursors.Default;
        }
        #endregion

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        /// 
        #region Reload
        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
        }
        #endregion

        #region OnFilePathChangeEvent
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region OnTileDataChangeEvent
        private void OnTileDataChangeEvent(object sender, int id)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (id < 0x4000)
            {
                return;
            }

            id -= 0x4000;

            if (_selectedGraphicId != id)
            {
                return;
            }

            UpdateToolStripLabels(id);
            UpdateDetail(id);
        }
        #endregion

        #region OnItemChangeEvent
        private void OnItemChangeEvent(object sender, int index)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Art.IsValidStatic(index))
            {
                bool done = false;
                for (int i = 0; i < _itemList.Count; ++i)
                {
                    if (index < _itemList[i])
                    {
                        _itemList.Insert(i, index);
                        done = true;
                        break;
                    }

                    if (index != _itemList[i])
                    {
                        continue;
                    }

                    done = true;
                    break;
                }

                if (!done)
                {
                    _itemList.Add(index);
                }
            }
            else
            {
                if (_showFreeSlots)
                {
                    return;
                }

                _itemList.Remove(index);
            }

            ItemsTileView.VirtualListSize = _itemList.Count;
            ItemsTileView.Invalidate();
        }
        #endregion

        private Color _backgroundColorItem = Color.White;

        #region ChangeBackgroundColorToolStripMenuItem
        private void ChangeBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _backgroundColorItem = colorDialog.Color;

            if (Options.OverrideBackgroundColorFromTile)
            {
                ItemsTileView.BackColor = _backgroundColorItem;
            }

            ItemsTileView.Invalidate();
        }
        #endregion

        private Color _backgroundDetailColor = Color.White;

        #region UpdateDetail
        private void UpdateDetail(int graphic)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (!IsLoaded)
            {
                return;
            }

            if (_scrolling)
            {
                return;
            }

            ItemData item = TileData.ItemTable[graphic];
            Bitmap bit = Art.GetStatic(graphic);

            int xMin = 0;
            int xMax = 0;
            int yMin = 0;
            int yMax = 0;

            const int defaultSplitterDistance = 180;
            if (bit == null)
            {
                splitContainer2.SplitterDistance = defaultSplitterDistance;
                Bitmap newBit = new Bitmap(DetailPictureBox.Size.Width, DetailPictureBox.Size.Height);
                using (Graphics newGraph = Graphics.FromImage(newBit))
                {
                    newGraph.Clear(_backgroundDetailColor);
                }

                DetailPictureBox.Image?.Dispose();
                DetailPictureBox.Image = newBit;
            }
            else
            {
                var distance = bit.Size.Height + 10;
                splitContainer2.SplitterDistance = distance < defaultSplitterDistance ? defaultSplitterDistance : distance;

                Bitmap newBit = new Bitmap(DetailPictureBox.Size.Width, DetailPictureBox.Size.Height);
                using (Graphics newGraph = Graphics.FromImage(newBit))
                {
                    newGraph.Clear(_backgroundDetailColor);
                    newGraph.DrawImage(bit, (DetailPictureBox.Size.Width - bit.Width) / 2, 5);
                }

                DetailPictureBox.Image?.Dispose();
                DetailPictureBox.Image = newBit;

                Art.Measure(bit, out xMin, out yMin, out xMax, out yMax);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Name: {item.Name}");
            sb.AppendLine($"Graphic: 0x{graphic:X4}");
            sb.AppendLine($"Height/Capacity: {item.Height}");
            sb.AppendLine($"Weight: {item.Weight}");
            sb.AppendLine($"Animation: {item.Animation}");
            sb.AppendLine($"Quality/Layer/Light: {item.Quality}");
            sb.AppendLine($"Quantity: {item.Quantity}");
            sb.AppendLine($"Hue: {item.Hue}");
            sb.AppendLine($"StackingOffset/Unk4: {item.StackingOffset}");
            sb.AppendLine($"Flags: {item.Flags}");
            sb.AppendLine($"Graphic pixel size width, height: {bit?.Width ?? 0} {bit?.Height ?? 0} ");
            sb.AppendLine($"Graphic pixel offset xMin, yMin, xMax, yMax: {xMin} {yMin} {xMax} {yMax}");

            if ((item.Flags & TileFlag.Animation) != 0)
            {
                Animdata.AnimdataEntry info = Animdata.GetAnimData(graphic);
                if (info != null)
                {
                    sb.AppendLine($"Animation FrameCount: {info.FrameCount} Interval: {info.FrameInterval}");
                }
            }

            DetailTextBox.Clear();
            DetailTextBox.AppendText(sb.ToString());

            // Apply color change if checkbox is checked = Particle Grey
            InsertNewImage((Image)DetailPictureBox.Image);
        }
        #endregion

        #region ChangeBackgroundColorToolStripMenuItemDetail
        private void ChangeBackgroundColorToolStripMenuItemDetail_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _backgroundDetailColor = colorDialog.Color;
            if (_selectedGraphicId != -1)
            {
                UpdateDetail(_selectedGraphicId);
            }
        }
        #endregion

        private ItemSearchForm _showForm;
        private bool _scrolling;

        #region OnSearchClick
        private void OnSearchClick(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new ItemSearchForm(SearchGraphic, SearchName)
            {
                TopMost = true
            };
            _showForm.Show();
        }
        #endregion

        #region OnClickFindFree
        private void OnClickFindFree(object sender, EventArgs e)
        {
            if (_showFreeSlots)
            {
                int i = _selectedGraphicId > -1 ? _itemList.IndexOf(_selectedGraphicId) + 1 : 0;
                for (; i < _itemList.Count; ++i)
                {
                    if (Art.IsValidStatic(_itemList[i]))
                    {
                        continue;
                    }

                    SelectedGraphicId = _itemList[i];
                    ItemsTileView.Invalidate();
                    break;
                }
            }
            else
            {
                int id, i;

                if (_selectedGraphicId > -1)
                {
                    id = _selectedGraphicId + 1;
                    i = _itemList.IndexOf(_selectedGraphicId) + 1;
                }
                else
                {
                    id = 0;
                    i = 0;
                }

                for (; i < _itemList.Count; ++i, ++id)
                {
                    if (id >= _itemList[i])
                    {
                        continue;
                    }

                    SelectedGraphicId = _itemList[i];
                    ItemsTileView.Invalidate();
                    break;
                }
            }
        }
        #endregion

        #region OnClickReplace
        private void OnClickReplace(object sender, EventArgs e)
        {
            if (_selectedGraphicId < 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    Art.ReplaceStatic(_selectedGraphicId, bitmap);

                    ControlEvents.FireItemChangeEvent(this, _selectedGraphicId);

                    ItemsTileView.Invalidate();
                    UpdateToolStripLabels(_selectedGraphicId);
                    UpdateDetail(_selectedGraphicId);

                    Options.ChangedUltimaClass["Art"] = true;
                }
            }
        }
        #endregion

        #region OnClickRemove
        private void OnClickRemove(object sender, EventArgs e)
        {
            if (!Art.IsValidStatic(_selectedGraphicId))
            {
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure to remove 0x{_selectedGraphicId:X}", "Save",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Art.RemoveStatic(_selectedGraphicId);
            ControlEvents.FireItemChangeEvent(this, _selectedGraphicId);

            if (!_showFreeSlots)
            {
                _itemList.Remove(_selectedGraphicId);
                ItemsTileView.VirtualListSize = _itemList.Count;
                var moveToIndex = --_selectedGraphicId;
                SelectedGraphicId = moveToIndex <= 0 ? 0 : _selectedGraphicId; // TODO: get last index visible instead just curr -1
            }
            ItemsTileView.Invalidate();

            Options.ChangedUltimaClass["Art"] = true;
        }
        #endregion

        #region OnTextChangedInsert
        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxItemId()))
            {
                InsertText.ForeColor = Art.IsValidStatic(index) ? Color.Red : Color.Black;
            }
            else
            {
                InsertText.ForeColor = Color.Red;
            }
        }
        #endregion

        #region OnKeyDownInsertText
        private void OnKeyDownInsertText(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxItemId()))
            {
                return;
            }

            if (Art.IsValidStatic(index))
            {
                return;
            }

            TileViewContextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = $"Choose images to replace starting at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                AddSingleItem(dialog.FileName, index);
            }
        }
        #endregion

        #region UpdateToolStripLabels
        private void UpdateToolStripLabels(int graphic)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (!IsLoaded)
            {
                return;
            }

            if (_scrolling)
            {
                return;
            }

            NameLabel.Text = !Art.IsValidStatic(graphic) ? "Name: FREE" : $"Name: {TileData.ItemTable[graphic].Name}";
            GraphicLabel.Text = $"Graphic: 0x{graphic:X4} ({graphic})";
        }
        #endregion

        #region OnClickSave
        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure? Will take a while", "Save", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            ProgressBarDialog barDialog = new ProgressBarDialog(Art.GetIdxLength(), "Save");
            Art.Save(Options.OutputPath);
            barDialog.Dispose();
            Cursor.Current = Cursors.Default;
            Options.ChangedUltimaClass["Art"] = false;
            MessageBox.Show($"Saved to {Options.OutputPath}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        // This method is an event handler for a button or control click
        #region OnClickShowFreeSlots
        private void OnClickShowFreeSlots(object sender, EventArgs e)
        {
            // Toggle the value of the _showFreeSlots variable
            _showFreeSlots = !_showFreeSlots;
            // If _showFreeSlots is true
            if (_showFreeSlots)
            {
                // Loop through all possible item IDs up to the maximum item ID
                for (int j = 0; j <= Art.GetMaxItemId(); ++j)
                {
                    // Check if the item is already in the _itemList
                    if (_itemList.Count > j)
                    {
                        // If the item is not in the _itemList, insert it at the current position
                        if (_itemList[j] != j)
                        {
                            _itemList.Insert(j, j);
                        }
                    }
                    else
                    {
                        // If the item is not in the _itemList, insert it at the current position
                        _itemList.Insert(j, j);
                    }
                }

                // Store the previously selected item ID
                var prevSelected = SelectedGraphicId;

                // Update the VirtualListSize property of the ItemsTileView control to reflect the new number of items
                ItemsTileView.VirtualListSize = _itemList.Count;

                // If there was a previously selected item, try to reselect it
                if (prevSelected >= 0)
                {
                    SelectedGraphicId = prevSelected;
                }

                // Force the ItemsTileView control to redraw
                ItemsTileView.Invalidate();
            }
            else
            {
                // If _showFreeSlots is false, call the Reload method
                Reload();
            }
        }
        #endregion

        #region Save format

        //old
        /*private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            if (_selectedGraphicId == -1)
            {
                return;
            }

            ExportItemImage(_selectedGraphicId, ImageFormat.Bmp);
        }*/

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            // Check if any items are selected in the ItemsTileView
            if (ItemsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Iterate through the selected indices
            foreach (int selectedIndex in ItemsTileView.SelectedIndices)
            {
                // Get the graphics for the selected item
                Bitmap bitmap = Art.GetStatic(_itemList[selectedIndex]);
                // Check if the graphics exist
                if (bitmap != null)
                {
                    // Save the graphics
                    ExportItemImage(_itemList[selectedIndex], ImageFormat.Bmp);
                }
            }
        }


        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            // Check if any items are selected in the ItemsTileView
            if (ItemsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Iterate through the selected indices
            foreach (int selectedIndex in ItemsTileView.SelectedIndices)
            {
                // Get the graphics for the selected item
                Bitmap bitmap = Art.GetStatic(_itemList[selectedIndex]);
                // Check if the graphics exist
                if (bitmap != null)
                {
                    // Save the graphics
                    ExportItemImage(_itemList[selectedIndex], ImageFormat.Tiff);
                }
            }
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            // Check if any items are selected in the ItemsTileView
            if (ItemsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Iterate through the selected indices
            foreach (int selectedIndex in ItemsTileView.SelectedIndices)
            {
                // Get the graphics for the selected item
                Bitmap bitmap = Art.GetStatic(_itemList[selectedIndex]);
                // Check if the graphics exist
                if (bitmap != null)
                {
                    // Save the graphics
                    ExportItemImage(_itemList[selectedIndex], ImageFormat.Jpeg);
                }
            }
        }

        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            // Check if any items are selected in the ItemsTileView
            if (ItemsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Iterate through the selected indices
            foreach (int selectedIndex in ItemsTileView.SelectedIndices)
            {
                // Get the graphics for the selected item
                Bitmap bitmap = Art.GetStatic(_itemList[selectedIndex]);
                // Check if the graphics exist
                if (bitmap != null)
                {
                    // Save the graphics
                    ExportItemImage(_itemList[selectedIndex], ImageFormat.Png);
                }
            }
        }

        private static void ExportItemImage(int index, ImageFormat imageFormat)
        {
            if (!Art.IsValidStatic(index))
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Item 0x{index:X4}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Art.GetStatic(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show($"Item saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        #endregion

        #region OnClickSelectTiledata
        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            if (_selectedGraphicId == -1)
            {
                tileDataControl.RefreshPictureBoxItem(); // Refresh the picture box
            }

            if (_selectedGraphicId >= 0)
            {
                TileDataControl.Select(_selectedGraphicId, false);
                //tileDataControl.RefreshPictureBoxItem(); //Select pictureBoxItem TileDataControl
            }
        }
        #endregion

        #region OnClickSelectRadarCol
        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (_selectedGraphicId >= 0)
            {
                RadarColorControl.Select(_selectedGraphicId, false);
            }
        }
        #endregion

        #region Misc Save
        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            ExportAllItemImages(ImageFormat.Bmp);
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            ExportAllItemImages(ImageFormat.Tiff);
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            ExportAllItemImages(ImageFormat.Jpeg);
        }

        private void OnClick_SaveAllPng(object sender, EventArgs e)
        {
            ExportAllItemImages(ImageFormat.Png);
        }

        // This method exports all item images in a specified image format
        private void ExportAllItemImages(ImageFormat imageFormat)
        {
            // Get the file extension for the specified image format
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            // Create a new FolderBrowserDialog to prompt the user to select a directory
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // Set the cursor to the wait cursor
                Cursor.Current = Cursors.WaitCursor;

                // Create a new ProgressBarDialog to show the progress of the export
                using (new ProgressBarDialog(_itemList.Count, $"Export to {fileExtension}", false))
                {
                    // Loop through all items in the _itemList
                    foreach (var artItemIndex in _itemList)
                    {
                        // Update the progress bar
                        ControlEvents.FireProgressChangeEvent();
                        Application.DoEvents();

                        int index = artItemIndex;
                        if (index < 0)
                        {
                            continue;
                        }

                        // Create the file name for the image
                        string fileName = Path.Combine(dialog.SelectedPath, $"Item 0x{index:X4}.{fileExtension}");
                        // Save the image to the specified file
                        using (Bitmap bit = new Bitmap(Art.GetStatic(index)))
                        {
                            bit.Save(fileName, imageFormat);
                        }
                    }
                }
                // Reset the cursor to the default cursor
                Cursor.Current = Cursors.Default;
                // Show a message that all items have been saved
                MessageBox.Show($"All items saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region OnClickPreLoad
        private void OnClickPreLoad(object sender, EventArgs e)
        {
            if (PreLoader.IsBusy)
            {
                return;
            }

            ProgressBar.Minimum = 1;
            ProgressBar.Maximum = _itemList.Count;
            ProgressBar.Step = 1;
            ProgressBar.Value = 1;
            ProgressBar.Visible = true;
            PreLoader.RunWorkerAsync();
        }
        #endregion

        #region PreLoaderDoWork
        private void PreLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            foreach (int item in _itemList)
            {
                Art.GetStatic(item);
                PreLoader.ReportProgress(1);
            }
        }
        #endregion

        #region PreLoaderProgressChanged
        private void PreLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }
        #endregion

        #region PreLoaderCompleted
        private void PreLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
        }
        #endregion

        #region ItemsTileView_DrawItem
        private void ItemsTileView_DrawItem(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Point itemPoint = new Point(e.Bounds.X + ItemsTileView.TilePadding.Left, e.Bounds.Y + ItemsTileView.TilePadding.Top);

            Rectangle rect = new Rectangle(itemPoint, ItemsTileView.TileSize);

            var previousClip = e.Graphics.Clip;

            e.Graphics.Clip = new Region(rect);

            var selected = ItemsTileView.SelectedIndices.Contains(e.Index);
            if (!selected)
            {
                e.Graphics.Clear(_backgroundColorItem);
            }

            var bitmap = Art.GetStatic(_itemList[e.Index], out bool patched);
            if (bitmap == null)
            {
                e.Graphics.Clip = new Region(rect);

                rect.X += 5;
                rect.Y += 5;

                rect.Width -= 10;
                rect.Height -= 10;

                e.Graphics.FillRectangle(Brushes.Red, rect);
                e.Graphics.Clip = previousClip;
            }
            else
            {
                if (patched && !selected)
                {
                    e.Graphics.FillRectangle(Brushes.LightCoral, rect);
                }

                if (Options.ArtItemClip)
                {
                    e.Graphics.DrawImage(bitmap, itemPoint);
                }
                else
                {
                    int width = bitmap.Width;
                    int height = bitmap.Height;
                    if (width > ItemsTileView.TileSize.Width)
                    {
                        width = ItemsTileView.TileSize.Width;
                        height = ItemsTileView.TileSize.Height * bitmap.Height / bitmap.Width;
                    }

                    if (height > ItemsTileView.TileSize.Height)
                    {
                        height = ItemsTileView.TileSize.Height;
                        width = ItemsTileView.TileSize.Width * bitmap.Width / bitmap.Height;
                    }

                    e.Graphics.DrawImage(bitmap, new Rectangle(itemPoint, new Size(width, height)));
                }

                e.Graphics.Clip = previousClip;
            }
        }
        #endregion

        #region ItemsTileView_ItemSelectionChanged
        private void ItemsTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            UpdateSelection(e.ItemIndex);
        }
        #endregion

        #region ItemsTileView_FocusSelectionChanged
        private void ItemsTileView_FocusSelectionChanged(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (!e.IsFocused)
            {
                return;
            }

            UpdateSelection(e.FocusedItemIndex);
        }
        #endregion

        #region UpdateSelection
        private void UpdateSelection(int itemIndex)
        {
            if (_itemList.Count == 0)
            {
                return;
            }

            SelectedGraphicId = itemIndex < 0 || itemIndex > _itemList.Count
                ? _itemList[0]
                : _itemList[itemIndex];
        }
        #endregion

        #region ItemsTileView_MouseDoubleClick
        public void ItemsTileView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (ItemsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            ItemDetailForm f = new ItemDetailForm(_itemList[ItemsTileView.SelectedIndices[0]])
            {
                TopMost = true
            };
            f.Show();
        }
        #endregion

        #region ItemsTileView_KeyDown
        private void ItemsTileView_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Ctrl+V key combination has been pressed
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Calling the importToolStripclipboardMenuItem_Click method to import the graphic from the clipboard.
                importToolStripclipboardMenuItem_Click(sender, e);
            }
            // Checking if the Ctrl+X key combination has been pressed.
            else if (e.Control && e.KeyCode == Keys.X)
            {
                // Calling the cutToolStripclipboardMenuItem_Click method to cut the selected area.
                copyToolStripMenuItem_Click(sender, e);
            }
            // Checking if the Page Down or Page Up key combination has been pressed.
            else if (e.KeyData == Keys.PageDown || e.KeyData == Keys.PageUp)
            {
                _scrolling = true;
            }
            // Check if the Ctrl+F3 key combination has been pressed
            else if (e.Control && e.KeyCode == Keys.F3)
            {
                // Call the searchByNameToolStripButton_Click method
                searchByNameToolStripButton_Click(sender, e);
            }
        }
        #endregion

        #region ItemsTileView_KeyUp
        private void ItemsTileView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData != Keys.PageDown && e.KeyData != Keys.PageUp)
            {
                return;
            }

            _scrolling = false;

            if (ItemsTileView.FocusIndex > 0)
            {
                UpdateToolStripLabels(_selectedGraphicId);
                UpdateDetail(_selectedGraphicId);
            }
        }
        #endregion

        private const int _maleGumpOffset = 50_000;
        private const int _femaleGumpOffset = 60_000;

        #region SelectInGumpsTab
        private static void SelectInGumpsTab(int graphicId, bool female = false)
        {
            int gumpOffset = female ? _femaleGumpOffset : _maleGumpOffset;
            var itemData = TileData.ItemTable[graphicId];

            GumpControl.Select(itemData.Animation + gumpOffset);
        }
        #endregion

        #region SelectInGumpsTabMaleToolStripMenuItem
        private void SelectInGumpsTabMaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedGraphicId <= 0)
            {
                return;
            }

            SelectInGumpsTab(SelectedGraphicId);
        }
        #endregion

        #region SelectInGumpsTabFemaleToolStripMenuItem
        private void SelectInGumpsTabFemaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedGraphicId <= 0)
            {
                return;
            }

            SelectInGumpsTab(SelectedGraphicId, true);
        }
        #endregion

        #region TileViewContextMenuStrip
        private void TileViewContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (SelectedGraphicId <= 0)
            {
                selectInGumpsTabMaleToolStripMenuItem.Enabled = false;
                selectInGumpsTabFemaleToolStripMenuItem.Enabled = false;
            }
            else
            {
                var itemData = TileData.ItemTable[SelectedGraphicId];

                if (itemData.Animation > 0)
                {
                    selectInGumpsTabMaleToolStripMenuItem.Enabled =
                        GumpControl.HasGumpId(itemData.Animation + _maleGumpOffset);

                    selectInGumpsTabFemaleToolStripMenuItem.Enabled =
                        GumpControl.HasGumpId(itemData.Animation + _femaleGumpOffset);
                }
                else
                {
                    selectInGumpsTabMaleToolStripMenuItem.Enabled = false;
                    selectInGumpsTabFemaleToolStripMenuItem.Enabled = false;
                }
            }
        }
        #endregion

        #region ReplaceStartingFromText_KeyDown
        private void ReplaceStartingFromText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(ReplaceStartingFromText.Text, out int index, 0, Art.GetMaxItemId()))
            {
                return;
            }

            TileViewContextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose image file replace starting at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < dialog.FileNames.Length; i++)
                {
                    var currentIdx = index + i;

                    if (IsIndexValid(currentIdx))
                    {
                        AddSingleItem(dialog.FileNames[i], currentIdx);
                    }
                }

                ItemsTileView.VirtualListSize = _itemList.Count;
                ItemsTileView.Invalidate();

                SelectedGraphicId = index;

                UpdateToolStripLabels(index);
                UpdateDetail(index);
            }
        }
        #endregion

        /// <summary>
        /// Adds a single static item.
        /// </summary>
        /// <param name="fileName">Filename of the image to add.</param>
        /// <param name="index">Index where the static item will be added.</param>
        /// 
        #region AddSingleItem
        private void AddSingleItem(string fileName, int index)
        {
            using (var bmpTemp = new Bitmap(fileName))
            {
                Bitmap bitmap = new Bitmap(bmpTemp);

                if (fileName.Contains(".bmp"))
                {
                    bitmap = Utils.ConvertBmp(bitmap);
                }

                Art.ReplaceStatic(index, bitmap);

                ControlEvents.FireItemChangeEvent(this, index);

                Options.ChangedUltimaClass["Art"] = true;

                if (_showFreeSlots)
                {
                    SelectedGraphicId = index;

                    UpdateToolStripLabels(index);
                    UpdateDetail(index);
                }
                else
                {
                    bool done = false;

                    for (int i = 0; i < _itemList.Count; ++i)
                    {
                        if (index > _itemList[i])
                        {
                            continue;
                        }

                        _itemList[i] = index;

                        done = true;

                        break;
                    }

                    if (!done)
                    {
                        _itemList.Add(index);
                    }

                    ItemsTileView.VirtualListSize = _itemList.Count;
                    ItemsTileView.Invalidate();

                    SelectedGraphicId = index;

                    UpdateToolStripLabels(index);
                    UpdateDetail(index);
                }
            }
        }
        #endregion

        /// <summary>
        /// Check if it's valid index for land tile. Land tiles has fixed size 0x4000.
        /// </summary>
        /// <param name="index">Starting Index</param>
        /// 
        #region IsIndexValid
        private static bool IsIndexValid(int index)
        {
            return index >= 0 && index <= Art.GetMaxItemId();
        }
        #endregion

        #region Copy clipboard
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if any items are selected in the ItemsTileView
            if (ItemsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Iterate through the selected indices
            foreach (int selectedIndex in ItemsTileView.SelectedIndices)
            {
                // Get the graphic for the selected item
                Bitmap bitmap = Art.GetStatic(_itemList[selectedIndex]);
                // Check if the graphic exists
                if (bitmap != null)
                {
                    // Change the color #D3D3D3 to #FFFFFF
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            Color pixelColor = bitmap.GetPixel(x, y);
                            if (pixelColor.R == 211 && pixelColor.G == 211 && pixelColor.B == 211)
                            {
                                bitmap.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                            }
                        }
                    }

                    // Convert the image to a 16-bit color depth
                    Bitmap bmp16bit = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
                    using (Graphics g = Graphics.FromImage(bmp16bit))
                    {
                        g.DrawImage(bitmap, new Rectangle(0, 0, bmp16bit.Width, bmp16bit.Height));
                    }

                    // Copy the graphic to the clipboard
                    Clipboard.SetImage(bmp16bit);
                    MessageBox.Show($"The image {selectedIndex} has been copied to the clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Show a MessageBox to inform the user that the image was successfully copied
                    MessageBox.Show($"No image to copy for index {selectedIndex}!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Import clipbord image 
        private void importToolStripclipboardMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the clipboard contains an image
            if (Clipboard.ContainsImage())
            {
                using (Image image = Clipboard.GetImage())
                {
                    Size imageSize = image.Size;
                    int bytesPerPixel = 4; // assuming 32-bit image
                    int imageSizeInBytes = imageSize.Width * imageSize.Height * bytesPerPixel;
                }
                // Retrieve the image from the clipboard
                using (Bitmap bmp = new Bitmap(Clipboard.GetImage()))
                {   // Get the selected index from the ItemsTileView
                    int index = SelectedGraphicId;

                    if (index >= 0 && index < Art.GetMaxItemId())
                    {   // Create a new bitmap with the same size as the image from the clipboard
                        Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);
                        // Set the resolution of the new bitmap to 96 DPI
                        newBmp.SetResolution(96, 96);
                        // Define the colors to Convert
                        Color[] colorsToConvert = new Color[]
                        {
                            Color.FromArgb(211, 211, 211), // #D3D3D3 => #000000
                            Color.FromArgb(0, 0, 0),       // #000000 => #000000
                            Color.FromArgb(255, 255, 255), // #FFFFFF => #000000
                            Color.FromArgb(254, 254, 254)  // #FEFEFE => #000000
                        };
                        // Iterate through each pixel of the image
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            for (int y = 0; y < bmp.Height; y++)
                            {   // Get the color of the current pixel
                                Color pixelColor = bmp.GetPixel(x, y);
                                if (colorsToConvert.Contains(pixelColor))
                                {
                                    newBmp.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                                }
                                else
                                {
                                    newBmp.SetPixel(x, y, pixelColor);
                                }
                            }
                        }
                        // Create a new bitmap with the specified pixel format (32-bit)
                        Bitmap finalBmp = newBmp.Clone(new Rectangle(0, 0, newBmp.Width, newBmp.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        // Create the "clipboardTemp" directory in the same directory as the main program
                        string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clipboardTemp");
                        Directory.CreateDirectory(directoryPath);

                        // Save the final bitmap to a file in the "clipboardTemp" directory with the selected index and an additional name "Arts"
                        string fileName = $"Art_Hex_Adress_{index:X}.bmp";
                        string filePath = Path.Combine(directoryPath, fileName);
                        finalBmp.Save(filePath);

                        // Import the saved bitmap
                        using (var bmpTemp = new Bitmap(filePath))
                        {
                            Bitmap bitmap = new Bitmap(bmpTemp);

                            if (filePath.Contains(".bmp"))
                            {
                                bitmap = Utils.ConvertBmp(bitmap);
                            }

                            Art.ReplaceStatic(index, bitmap);

                            ControlEvents.FireItemChangeEvent(this, index);

                            if (!_itemList.Contains(index))
                            {
                                _itemList.Add(index);
                                _itemList.Sort();
                            }
                            ItemsTileView.VirtualListSize = _itemList.Count;
                            ItemsTileView.Invalidate();
                            SelectedGraphicId = index;
                            Options.ChangedUltimaClass["Art"] = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("No image in the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        #endregion

        #region Mirror
        private void mirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if any items are selected in the ItemsTileView.
            if (ItemsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Iterating through the selected indices.
            foreach (int selectedIndex in ItemsTileView.SelectedIndices)
            {
                // Getting the image for the selected item.
                Bitmap bitmap = Art.GetStatic(_itemList[selectedIndex]);

                // Checking if the image is available.
                if (bitmap != null)
                {
                    // Mirroring the image horizontally.
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    // Replacing the original image with the mirrored image.
                    Art.ReplaceStatic(_itemList[selectedIndex], bitmap);
                }
            }

            // Updating the DetailPictureBox.
            UpdateDetail(_selectedGraphicId);

            // Updating the ItemsTileView.
            ItemsTileView.Invalidate();
        }
        #endregion

        #region new Search
        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Utils.ConvertStringToInt(searchByIdToolStripTextBox.Text, out int indexValue))
            {
                return;
            }

            var maximumIndex = Art.GetMaxItemId();

            if (indexValue < 0)
            {
                indexValue = 0;
            }

            if (indexValue > maximumIndex)
            {
                indexValue = maximumIndex;
            }

            // we have to invalidate focus so it will scroll to item
            ItemsTileView.FocusIndex = -1;
            SelectedGraphicId = indexValue;
        }
        private void SearchByNameToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            SearchName(searchByNameToolStripTextBox.Text, false);
        }
        private void searchByNameToolStripButton_Click(object sender, EventArgs e)
        {
            SearchName(searchByNameToolStripTextBox.Text, true);
            // Update _reverseSearchIndex after forward search
            _reverseSearchIndex = _itemList.IndexOf(SelectedGraphicId);
        }
        #endregion

        #region Select ID to Hex
        private void SelectIDToHexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedGraphicId >= 0)
            {
                // Convert the selected ID to a hex address
                string hexAddress = $"0x{_selectedGraphicId:X4}";

                // Copy the hex address to the clipboard
                Clipboard.SetText(hexAddress);
            }
        }
        #endregion

        #region Image swap
        private void imageSwapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Make sure that exactly two items are selected
            if (ItemsTileView.SelectedIndices.Count != 2)
            {
                MessageBox.Show("Please select exactly two items to exchange.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get the Selected Indices
            int index1 = ItemsTileView.SelectedIndices[0];
            int index2 = ItemsTileView.SelectedIndices[1];

            // Save the graphics temporarily
            Bitmap ArtTempImage1 = Art.GetStatic(_itemList[index1]);
            Bitmap ArtTempImage2 = Art.GetStatic(_itemList[index2]);

            // Swap the graphics
            ReplaceStaticSwap(_itemList[index1], ArtTempImage1, _itemList[index2], ArtTempImage2);

            // Update the view and labels
            ItemsTileView.Invalidate();
            UpdateToolStripLabels(_selectedGraphicId);
            UpdateDetail(_selectedGraphicId);

            Options.ChangedUltimaClass["Art"] = true;
        }

        private void ReplaceStaticSwap(int index1, Bitmap newGraphic1, int index2, Bitmap newGraphic2)
        {
            // Replace the graph at 'index1' with 'newGraphic2'
            _selectedGraphicId = index1;
            OnClickReplace(newGraphic2);

            // Replace the graphic at 'index2' with 'newGraphic1'
            _selectedGraphicId = index2;
            OnClickReplace(newGraphic1);

        }

        private void OnClickReplace(Bitmap bitmap)
        {
            Art.ReplaceStatic(_selectedGraphicId, bitmap);
            ControlEvents.FireItemChangeEvent(this, _selectedGraphicId);
        }
        #endregion

        #region reverse search

        // Global variable to store the current index of the backward search        
        private int _reverseSearchIndex = -1;

        private void ReverseSearchByName(string name)
        {
            // Check if the name is empty
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            // If _reverseSearchIndex is -1 or if a forward search was performed, initialize it with the last index in _itemList
            if (_reverseSearchIndex == -1 || _reverseSearchIndex >= _itemList.Count)
            {
                _reverseSearchIndex = _itemList.Count - 1;
            }

            // Loop through the _itemList in reverse order starting at _reverseSearchIndex
            for (int i = _reverseSearchIndex; i >= 0; i--)
            {
                // Get the item at the current position
                var item = _itemList[i];

                // Check whether the name of the item contains the name you are looking for (partial match)
                if (TileData.ItemTable[item].Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    // If yes, set SelectedGraphicId to the index of the found item and terminate the loop
                    SelectedGraphicId = item;

                    // Update _reverseSearchIndex for next search
                    _reverseSearchIndex = i - 1;
                    break;
                }
            }

            // When the entire _itemList has been traversed, set _reverseSearchIndex back to -1
            if (_reverseSearchIndex < 0)
            {
                _reverseSearchIndex = -1;
            }
        }

        private void ReverseSearchToolStripButton_Click(object sender, EventArgs e)
        {
            // Get the name from the TextBox
            string name = searchByNameToolStripTextBox.Text;

            // Perform the reverse search
            ReverseSearchByName(name);
        }
        #endregion

        #region Paricle Gray Shadow
        private void particleGraylToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Überprüfen, ob ein Bild vorhanden ist
            if (DetailPictureBox.Image != null)
            {
                // Get the selected image from DetailPictureBox
                Bitmap bmp = new Bitmap(DetailPictureBox.Image);

                // List of colors to change
                List<string> colorsToChange = new List<string>
                {
                    // Add any other colors here...
                    "030303", "040404", "050505", "060606", "070707", "080808", "090909", "0A0A0A", "0B0B0B", "0C0C0C", "0D0D0D", "0E0E0E", "0F0F0F",
                    "101010", "111111", "121212", "131313", "141414", "151515", "161616", "171717", "181818", "191919", "1A1A1A", "1B1B1B", "1C1C1C",
                    "1D1D1D", "1E1E1E", "1F1F1F", "202020", "212121", "222222", "232323", "242424", "252525", "262626", "272727", "282828", "292929",
                    "2A2A2A", "2B2B2B", "2C2C2C", "2D2D2D", "2E2E2E", "2F2F2F", "303030", "313131", "323232", "333333", "343434", "353535", "363636",
                    "373737", "383838", "393939", "3A3A3A", "3B3B3B", "3C3C3C", "3D3D3D", "3E3E3E", "3F3F3F", "404040", "414141", "424242", "434343",
                    "444444", "454545", "464646", "474747", "484848", "494949", "4A4A4A", "4B4B4B", "4C4C4C", "4D4D4D", "4E4E4E", "4F4F4F", "505050",
                    "515151", "525252", "535353", "545454", "555555", "565656", "575757", "585858", "595959", "5A5A5A", "5B5B5B", "5C5C5C", "5D5D5D",
                    "5E5E5E", "5F5F5F", "606060", "616161", "626262", "636363", "646464", "656565", "666666", "676767", "686868", "696969", "6A6A6A",
                    "6B6B6B", "6C6C6C", "6D6D6D", "6E6E6E", "6F6F6F", "707070", "717171", "727272", "737373", "747474", "757575", "767676", "777777",
                    "787878", "797979", "7A7A7A", "7B7B7B", "7C7C7C", "7D7D7D", "7E7E7E", "7F7F7F", "808080", "818181", "828282", "838383", "848484",
                    "858585", "868686", "878787", "888888", "898989", "8A8A8A", "8B8B8B", "8C8C8C", "8D8D8D", "8E8E8E", "8F8F8F", "909090", "919191",
                    "929292", "939393", "949494", "959595", "969696", "979797", "989898", "999999", "9A9A9A", "9B9B9B", "9C9C9C", "9D9D9D", "9E9E9E",
                    "9F9F9F", "A0A0A0", "A1A1A1", "A2A2A2", "A3A3A3", "A4A4A4", "A5A5A5", "A6A6A6", "A7A7A7", "A8A8A8", "A9A9A9", "AAAAAA", "ABABAB",
                    "ACACAC", "ADADAD", "AEAEAE", "AFAFAF", "B0B0B0", "B1B1B1", "B2B2B2", "B3B3B3", "B4B4B4", "B5B5B5", "B6B6B6", "B7B7B7", "B8B8B8",
                    "B9B9B9", "BABABA", "BBBBBB", "BCBCBC", "BDBDBD", "BEBEBE", "BFBFBF", "C0C0C0", "C1C1C1", "C2C2C2", "C3C3C3", "C4C4C4", "C5C5C5",
                    "C6C6C6", "C7C7C7", "C8C8C8", "C9C9C9", "CACACA", "CBCBCB", "CCCCCC", "CDCDCD", "CECECE", "CFCFCF", "D0D0D0", "D1D1D1", "D2D2D2",
                    "D3D3D3", "D4D4D4", "D5D5D5", "D6D6D6", "D7D7D7", "D8D8D8", "D9D9D9", "DADADA", "DBDBDB", "DCDCDC", "DDDDDD", "DEDEDE", "DFDFDF",
                    "E0E0E0", "E1E1E1", "E2E2E2", "E3E3E3", "E4E4E4", "E5E5E5", "E6E6E6", "E7E7E7", "E8E8E8", "E9E9E9", "EAEAEA", "EBEBEB", "ECECEC",
                    "EDEDED", "EEEEEE", "EFEFEF", "F0F0F0", "F1F1F1", "F2F2F2", "F3F3F3", "F4F4F4", "F5F5F5", "F6F6F6", "F7F7F7", "F8F8F8", "F9F9F9",
                    "FAFAFA", "FBFBFB"
                };

                // New color
                // Color newColor = Color.Blue;

                // New color
                Color newColor = selectedColor; // Use the color selected by the user

                // Loop through the pixels
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        // Get the pixel color at coordinate
                        Color oldColor = bmp.GetPixel(x, y);

                        // Check if its one of the colors to change
                        string colorHex = oldColor.R.ToString("X2") + oldColor.G.ToString("X2") + oldColor.B.ToString("X2");
                        if (colorsToChange.Contains(colorHex))
                        {
                            // Get the brightness of the original color (0-1)
                            float brightness = oldColor.GetBrightness();

                            // Create a new color that is the original color adjusted by brightness
                            int newR = (int)(newColor.R * brightness);
                            int newG = (int)(newColor.G * brightness);
                            int newB = (int)(newColor.B * brightness);

                            Color newShadedColor = Color.FromArgb(newR, newG, newB);

                            // Change it to the new shaded color
                            bmp.SetPixel(x, y, newShadedColor);
                        }
                    }
                }

                // Set the new image
                DetailPictureBox.Image = bmp;
            }
            else
            {
                // Handling the case when there is no image...
                // MessageBox.Show("No image was selected. Please select an image first.");
            }
        }

        private void chkApplyColorChange_CheckedChanged(object sender, EventArgs e)
        {
            ApplyColorChange();
        }

        private void InsertNewImage(Image newImage)
        {
            // Set the new image
            DetailPictureBox.Image = newImage;

            // Apply color change if checkbox is checked
            ApplyColorChange();
        }

        private void ApplyColorChange()
        {
            // Check if the checkbox is checked
            if (chkApplyColorChange.Checked)
            {
                // Call the color change method directly
                particleGraylToolStripMenuItem_Click(null, null);
            }
        }
        #endregion

        #region Particle Gray ColorDialog
        private Color selectedColor = Color.Blue; // Standardfarbe
        private void particleGrayColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedColor = colorDialog.Color;
                }
            }
        }
        #endregion
    }
}
