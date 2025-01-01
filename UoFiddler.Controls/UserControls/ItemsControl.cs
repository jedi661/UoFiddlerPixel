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
using System.Collections;
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
using System.Media;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.UserControls.TileView;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UoFiddler.Controls.UserControls
{
    public partial class ItemsControl : UserControl
    {
        private TileDataControl tileDataControl = new TileDataControl(); //Refesh image pictureBoxItem TiledataControl

        private int occupiedItemCount = 0; // items counter
        private bool isDrawingRhombusActive = false; // DrawRhombus

        private Form imageForm; // DetailPictureBox_MouseDoubleClick
        private PictureBox imagePictureBox; // DetailPictureBox_MouseDoubleClick

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

        #region [ SelectedGraphicId ]
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

        #region [ UpdateTileView ]
        /// <summary>
        /// Updates if TileSize is changed
        /// </summary>
        /// 
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

        #region [ SearchGraphic ]
        /// <summary>
        /// Searches graphic number and selects it
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        /// 
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

        #region [ SearchName ]
        /// <summary>
        /// Searches for name and selects
        /// </summary>
        /// <param name="name"></param>
        /// <param name="next">starting from current selected</param>
        /// <returns></returns>
        /// 
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

        #region [ OnLoad ]
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

        #region [ Reload ]
        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        /// 
        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
        }
        #endregion

        #region [ OnFilePathChangeEvent ]
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region [ OnTileDataChangeEvent ]
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

        #region [ OnItemChangeEvent ]
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

        #region [ ChangeBackgroundColorToolStripMenuItem ]

        private Color _backgroundColorItem = Color.White;
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

        #region [ UpdateDetail ]

        private Color _backgroundDetailColor = Color.White;
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

        #region [ ChangeBackgroundColorToolStripMenuItemDetail ]
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

        #region [ OnSearchClick ]
        private ItemSearchForm _showForm;
        private bool _scrolling;

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

        #region [ OnClickFindFree ]
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

        #region [ OnClickReplace ]
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

        #region [ OnClickRemove ]
        private void OnClickRemove(object sender, EventArgs e)
        {
            // Check if multiple artworks are selected
            if (ItemsTileView.SelectedIndices.Count > 1)
            {                
                DialogResult result = MessageBox.Show($"Are you sure you want to remove the selected artworks?", "Remove artwork", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Create a list of indexes to remove
                var indicesToRemove = ItemsTileView.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();

                // Iterate through all selected indexes and remove artworks
                foreach (int selectedIndex in indicesToRemove)
                {
                    int graphicId = _itemList[selectedIndex];
                    if (Art.IsValidStatic(graphicId))
                    {
                        Art.RemoveStatic(graphicId);
                        ControlEvents.FireItemChangeEvent(this, graphicId);
                    }

                    // Remove from list
                    _itemList.RemoveAt(selectedIndex);
                }

                // Update UI
                ItemsTileView.VirtualListSize = _itemList.Count;
                SelectedGraphicId = _itemList.Count > 0 ? _itemList[0] : 0; // No selection if list empty
                ItemsTileView.Invalidate();
            }
            else
            {
                // Remove single artwork (original logic)
                if (!Art.IsValidStatic(_selectedGraphicId))
                {
                    return;
                }

                DialogResult result = MessageBox.Show($"Are you sure you want to use the artwork 0x{_selectedGraphicId:X} want to remove?", "Remove artwork", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
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
                    SelectedGraphicId = moveToIndex <= 0 ? 0 : _selectedGraphicId; // TODO: last visible index instead of just curr -1
                }
                ItemsTileView.Invalidate();
            }

            Options.ChangedUltimaClass["Art"] = true;
        }
        #endregion

        #region [ OnTextChangedInsert ]
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

        #region [ OnKeyDownInsertText ]
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

        #region [ UpdateToolStripLabels ]
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
            GraphicLabel.Text = $"Graphic Hex: 0x{graphic:X4} ";
            toolStripStatusLabelGraficDecimal.Text = $"Graphic Decimal: {graphic}";
        }
        #endregion

        #region [ OnClickSave ]
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

        #region [ OnClickShowFreeSlots ]
        // This method is an event handler for a button or control click

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

        #region [ Save format ]

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

        #region [ OnClickSelectTiledata ]
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

        #region [ OnClickSelectRadarCol ]
        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (_selectedGraphicId >= 0)
            {
                RadarColorControl.Select(_selectedGraphicId, false);
            }
        }
        #endregion

        #region [ Misc Save ]
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

        #region [ OnClickPreLoad ]
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

        #region [ PreLoaderDoWork ]
        private void PreLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            foreach (int item in _itemList)
            {
                Art.GetStatic(item);
                PreLoader.ReportProgress(1);
            }
        }
        #endregion

        #region [ PreLoaderProgressChanged ]
        private void PreLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }
        #endregion

        #region [ PreLoaderCompleted ]
        private void PreLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
        }
        #endregion

        #region [ ItemsTileView_DrawItem ]
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

        #region [ ItemsTileView_ItemSelectionChanged ]
        private void ItemsTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            UpdateSelection(e.ItemIndex);
            //ItemsTileView.Focus(e.ItemIndex);

            // Call the update colors method for Datagridview => colorsImageToolStripMenuItem
            UpdateColors();
        }
        #endregion

        #region [ ItemsTileView_FocusSelectionChanged ]
        private void ItemsTileView_FocusSelectionChanged(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (!e.IsFocused)
            {
                return;
            }

            UpdateSelection(e.FocusedItemIndex);
        }
        #endregion

        #region [ UpdateSelection ]
        private void UpdateSelection(int itemIndex)
        {
            // Update the currentImageID when a new image is selected - Grid
            currentImageID = itemIndex;

            if (_itemList.Count == 0)
            {
                return;
            }

            SelectedGraphicId = itemIndex < 0 || itemIndex > _itemList.Count
                ? _itemList[0]
                : _itemList[itemIndex];
        }
        #endregion

        #region [ ItemsTileView_MouseDoubleClick ]
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

        #region [ ItemsTileView_KeyDown ]
        private void ItemsTileView_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Ctrl+V key combination has been pressed
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Calling the importToolStripclipboardMenuItem_Click method to import the graphic from the clipboard.
                ImportToolStripclipboardMenuItem_Click(sender, e);
            }
            // Checking if the Ctrl+X key combination has been pressed.
            else if (e.Control && e.KeyCode == Keys.X)
            {
                // Calling the cutToolStripclipboardMenuItem_Click method to cut the selected area.
                CopyToolStripMenuItem_Click(sender, e);
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
                SearchByNameToolStripButton_Click(sender, e);
            }
            // Verify that the Ctrl+J key combination was pressed
            else if (e.Control && e.KeyCode == Keys.J)
            {
                // Call the goToMarkedPositionToolStripMenuItem_Click method to jump to the marked position
                GoToMarkedPositionToolStripMenuItem_Click(sender, e);
            }
        }
        #endregion

        #region [ ItemsTileView_KeyUp ]
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

        #region [ SelectInGumpsTab ]
        private const int _maleGumpOffset = 50_000;
        private const int _femaleGumpOffset = 60_000;

        private static void SelectInGumpsTab(int graphicId, bool female = false)
        {
            int gumpOffset = female ? _femaleGumpOffset : _maleGumpOffset;
            var itemData = TileData.ItemTable[graphicId];

            GumpControl.Select(itemData.Animation + gumpOffset);
        }
        #endregion

        #region [ SelectInGumpsTabMaleToolStripMenuItem ]
        private void SelectInGumpsTabMaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedGraphicId <= 0)
            {
                return;
            }

            SelectInGumpsTab(SelectedGraphicId);
        }
        #endregion

        #region [ SelectInGumpsTabFemaleToolStripMenuItem ]
        private void SelectInGumpsTabFemaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedGraphicId <= 0)
            {
                return;
            }

            SelectInGumpsTab(SelectedGraphicId, true);
        }
        #endregion

        #region [ TileViewContextMenuStrip ]
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

        #region [ ReplaceStartingFromText_KeyDown ]
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

        #region [ AddSingleItem ]
        /// <summary>
        /// Adds a single static item.
        /// </summary>
        /// <param name="fileName">Filename of the image to add.</param>
        /// <param name="index">Index where the static item will be added.</param>
        /// 
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

        #region [ IsIndexValid ]
        /// <summary>
        /// Check if it's valid index for land tile. Land tiles has fixed size 0x4000.
        /// </summary>
        /// <param name="index">Starting Index</param>
        /// 
        private static bool IsIndexValid(int index)
        {
            return index >= 0 && index <= Art.GetMaxItemId();
        }
        #endregion

        #region [ Copy clipboard ]
        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
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

        #region [ Import clipbord image ]
        private void ImportToolStripclipboardMenuItem_Click(object sender, EventArgs e)
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

        #region [ Mirror Image ]
        private void MirrorToolStripMenuItem_Click(object sender, EventArgs e)
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

        #region [ new Search ]
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
        private void SearchByNameToolStripButton_Click(object sender, EventArgs e)
        {
            SearchName(searchByNameToolStripTextBox.Text, true);
            // Update _reverseSearchIndex after forward search
            _reverseSearchIndex = _itemList.IndexOf(SelectedGraphicId);
        }
        #endregion

        #region [ Select ID to Hex ]
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

        #region [ Image swap ]
        private void ImageSwapToolStripMenuItem_Click(object sender, EventArgs e)
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

        #region [ reverse search ]

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

        #region [ Paricle Gray Shadow ]
        private void ParticleGraylToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void ChkApplyColorChange_CheckedChanged(object sender, EventArgs e)
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
                ParticleGraylToolStripMenuItem_Click(null, null);
            }
        }
        #endregion

        #region [ Particle Gray ColorDialog ]
        private Color selectedColor = Color.Blue; // Standardfarbe
        private void ParticleGrayColorToolStripMenuItem_Click(object sender, EventArgs e)
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

        #region [ drawRhombus ]
        private void DrawRhombusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Make sure there is an image in the PictureBox
            if (DetailPictureBox.Image == null)
            {
                return;
            }

            // Create a new Graphics object from the existing image
            using (Graphics g = Graphics.FromImage(DetailPictureBox.Image))
            {
                // Define the points for your top diamond
                Point[] pointsUpper = new Point[4];
                pointsUpper[0] = new Point(DetailPictureBox.Image.Width / 2, 0); // center
                pointsUpper[1] = new Point(DetailPictureBox.Image.Width / 2 + 22, 22); // Bottom right
                pointsUpper[2] = new Point(DetailPictureBox.Image.Width / 2, 44); // Below
                pointsUpper[3] = new Point(DetailPictureBox.Image.Width / 2 - 22, 22); // Below left

                // Draw the top diamond
                g.DrawPolygon(Pens.Black, pointsUpper);

                // Draw lines from the corners of the top diamond upward
                g.DrawLine(Pens.Black, pointsUpper[0], new Point(pointsUpper[0].X, 0)); // From the middle up
                g.DrawLine(Pens.Black, pointsUpper[1], new Point(pointsUpper[1].X, 0)); // From bottom right to top
                g.DrawLine(Pens.Black, pointsUpper[3], new Point(pointsUpper[3].X, 0)); // From bottom left to top

                // Calculate the X coordinates for the horizontal line
                int lineWidth = 100;
                int lineStartX = (DetailPictureBox.Image.Width - lineWidth) / 2;
                int lineEndX = lineStartX + lineWidth;

                // Note the height of the image for the position of the bottom diamond
                int imageHeight = DetailPictureBox.Image.Height;

                // Draw a horizontal line at the top of the bottom diamond
                g.DrawLine(Pens.Black, new Point(lineStartX, imageHeight - 66), new Point(lineEndX, imageHeight - 66));

                // Define the points for your bottom diamond
                Point[] pointsLower = new Point[4];
                pointsLower[0] = new Point(DetailPictureBox.Image.Width / 2, imageHeight - 66); // center
                pointsLower[1] = new Point(DetailPictureBox.Image.Width / 2 + 22, imageHeight - 88); // Bottom right
                pointsLower[2] = new Point(DetailPictureBox.Image.Width / 2, imageHeight - 110); // Below
                pointsLower[3] = new Point(DetailPictureBox.Image.Width / 2 - 22, imageHeight - 88); // Below left

                // Draw the bottom diamond
                g.DrawPolygon(Pens.Black, pointsLower);

                // Draw lines from the corners of the bottom diamond up
                g.DrawLine(Pens.Black, pointsLower[0], new Point(pointsLower[0].X, pointsLower[0].Y - 22)); // From the middle up
                g.DrawLine(Pens.Black, pointsLower[1], new Point(pointsLower[1].X, pointsLower[1].Y - 22)); // From bottom right to top
                g.DrawLine(Pens.Black, pointsLower[3], new Point(pointsLower[3].X, pointsLower[3].Y - 22)); // From bottom left to top

                // Connect the lines of the upper and lower diamonds
                g.DrawLine(Pens.Black, pointsUpper[0], pointsLower[0]); // Connect the middle points
                g.DrawLine(Pens.Black, pointsUpper[1], pointsLower[1]); // Connect the right dots
                g.DrawLine(Pens.Black, pointsUpper[3], pointsLower[3]); // Connect the left dots
            }

            // Refresh the PictureBox to reflect the changes
            DetailPictureBox.Invalidate();
        }
        #endregion

        #region [ GridPictureToolStripMenuItem ]

        private int currentImageID;

        private void GridPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an image is selected
            if (currentImageID >= 0)
            {
                // Call the ShowImageWithBackground method to display the selected image
                ShowImageWithBackground(currentImageID);
            }
            else
            {
                // If no image is selected, you will receive an error message
                MessageBox.Show("Please first select an image from the ItemsTileView.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region [ ShowImageWithBackground ]
        private void ShowImageWithBackground(int imageIndex)
        {
            // Load the image you want to display
            Image foregroundImage = Art.GetStatic(_itemList[imageIndex]);

            // Download the wallpaper from resources
            Image backgroundImage = Properties.Resources.rasterpink_png;

            // Change the color of the background image
            backgroundImage = ChangeImageColor(backgroundImage, Color.FromArgb(244, 101, 255), selectedColorGrid);

            // Create a new bitmap large enough to hold both images
            Bitmap combinedImage = new Bitmap(Math.Max(backgroundImage.Width, foregroundImage.Width), Math.Max(backgroundImage.Height, foregroundImage.Height));

            // Create a Graphics object to be able to draw on the bitmap
            using (Graphics g = Graphics.FromImage(combinedImage))
            {
                // First draw the foreground image
                g.DrawImage(foregroundImage, (combinedImage.Width - foregroundImage.Width) / 2, (combinedImage.Height - foregroundImage.Height));

                // Draw the background image at the calculated position
                g.DrawImage(backgroundImage, (combinedImage.Width - backgroundImage.Width) / 2, (combinedImage.Height - backgroundImage.Height));
            }

            // Assign the combined image to the PictureBox
            DetailPictureBox.Image = combinedImage;
        }

        #endregion

        #region [ Copy Clipboard DetailPictureBox ]
        private void CopyClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check whether an image is displayed in the DetailPictureBox
            if (DetailPictureBox.Image != null)
            {
                // Copy the image to the clipboard
                Clipboard.SetImage(DetailPictureBox.Image);
            }
            else
            {
                // If no image is displayed, you will receive an error message
                MessageBox.Show("No image is displayed. Please select an image first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region [ Grid Color ]
        private Color selectedColorGrid = Color.FromArgb(244, 101, 255); // Default color #f465ff

        private void SelectColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                // Set the initial color to the currently selected color
                colorDialog.Color = selectedColorGrid;

                // Display the dialog and verify that the user clicked "OK."
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    // Update the selected color
                    selectedColorGrid = colorDialog.Color;
                }
            }
        }

        private Image ChangeImageColor(Image image, Color oldColor, Color newColor)
        {
            Bitmap bmp = new Bitmap(image);

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);

                    // Check if the current pixel is the old color
                    if (pixelColor == oldColor)
                    {
                        // If so, change the color of the pixel to the new color
                        bmp.SetPixel(x, y, newColor);
                    }
                }
            }

            return bmp;
        }
        #endregion

        #region [ colorsImageToolStripMenuIte ]
        // Global variable for the DataGridView and the shape
        DataGridView colorGrid;
        Form colorForm;
        private void ColorsImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create a new shape
            colorForm = new Form();
            colorForm.Text = "Image colors";
            // Set the FormBorderStyle to FixedDialog to fix the size of the shape
            colorForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            // Disable the Maximize button
            colorForm.MaximizeBox = false;
            // Make sure the shape always stays in the foreground
            colorForm.TopMost = true;

            // Create a new DataGridView and add it to the shape
            colorGrid = new DataGridView();
            colorGrid.Dock = DockStyle.Fill;
            colorForm.Controls.Add(colorGrid);

            // Add a column to the DataGridView
            colorGrid.Columns.Add("Color", "Color");

            // Add a cell click event handler
            colorGrid.CellClick += (s, args) =>
            {
                // Check if a cell was clicked
                if (args.RowIndex >= 0 && args.ColumnIndex >= 0)
                {
                    // Get the value of the clicked cell
                    string colorHex = (string)colorGrid.Rows[args.RowIndex].Cells[args.ColumnIndex].Value;

                    // Copy the value to the clipboard
                    Clipboard.SetText(colorHex);
                }
            };

            // Display the shape
            colorForm.Show();

            // Update the colors
            UpdateColors();
        }
        #endregion

        #region [ UpdateColors Datagridview ]
        private void UpdateColors()
        {
            // Check that colorGrid and colorForm are initialized and the shape is visible
            if (colorGrid == null || colorForm == null || !colorForm.Visible)
            {
                return;
            }

            // Delete all previous lines
            colorGrid.Rows.Clear();

            // Get the selected image
            Bitmap selectedImage = (Bitmap)DetailPictureBox.Image;

            // Create a HashSet to store unique colors
            HashSet<string> uniqueColors = new HashSet<string>();

            // Loop through every pixel in the image
            for (int x = 0; x < selectedImage.Width; x++)
            {
                for (int y = 0; y < selectedImage.Height; y++)
                {
                    // Get the color of the pixel
                    Color pixelColor = selectedImage.GetPixel(x, y);

                    // Convert the color to a hexadecimal code
                    string colorHex = pixelColor.R.ToString("X2") + pixelColor.G.ToString("X2") + pixelColor.B.ToString("X2");

                    // Add the color to the HashSet
                    uniqueColors.Add(colorHex);
                }
            }

            // Loop through each unique color and add it to the DataGridView
            foreach (string color in uniqueColors)
            {
                // Add a new row to the DataGridView
                int rowIndex = colorGrid.Rows.Add();

                // Set the cell background color to the color
                colorGrid.Rows[rowIndex].Cells[0].Style.BackColor = ColorTranslator.FromHtml("#" + color);

                // Set the cell's text to the hexcode of the color
                colorGrid.Rows[rowIndex].Cells[0].Value = color;
            }
        }
        #endregion

        #region [ markToolStripMenuItem ]
        // Variable for storing the marked position
        private int markedPosition = -1;

        // Method of marking position
        private void MarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ItemsTileView.SelectedIndices.Count > 0)
            {
                markedPosition = ItemsTileView.SelectedIndices[0];
            }
        }
        #endregion

        #region [ goToMarkedPositionToolStripMenuItem ]
        // Method to return to marked position
        private void GoToMarkedPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (markedPosition >= 0 && markedPosition < ItemsTileView.VirtualListSize)
            {
                // Set the selected position to the marked position
                ItemsTileView.SelectedIndices.Clear();
                ItemsTileView.SelectedIndices.Add(markedPosition);
            }
        }
        #endregion

        #region [ TileViewContextMenuStrip_Closing ]
        private void TileViewContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            // Check if any items are selected in the ItemsTileView
            if (ItemsTileView.SelectedIndices.Count > 0)
            {
                // Set the focus to the first selected item
                ItemsTileView.FocusIndex = ItemsTileView.SelectedIndices[0];
            }
        }
        #endregion

        #region [ grayscaleToolStripMenuItem ] 
        #region [ ArrayList grayscaleColors ]
        private ArrayList grayscaleColors =
        [
            ColorTranslator.FromHtml("#030303"), ColorTranslator.FromHtml("#040404"),
            ColorTranslator.FromHtml("#050505"), ColorTranslator.FromHtml("#060606"),
            ColorTranslator.FromHtml("#070707"), ColorTranslator.FromHtml("#080808"),
            ColorTranslator.FromHtml("#090909"), ColorTranslator.FromHtml("#0A0A0A"),
            ColorTranslator.FromHtml("#0B0B0B"), ColorTranslator.FromHtml("#0C0C0C"),
            ColorTranslator.FromHtml("#0D0D0D"), ColorTranslator.FromHtml("#0E0E0E"),
            ColorTranslator.FromHtml("#0F0F0F"), ColorTranslator.FromHtml("#101010"),
            ColorTranslator.FromHtml("#111111"), ColorTranslator.FromHtml("#121212"),
            ColorTranslator.FromHtml("#131313"), ColorTranslator.FromHtml("#141414"),
            ColorTranslator.FromHtml("#151515"), ColorTranslator.FromHtml("#161616"),
            ColorTranslator.FromHtml("#171717"), ColorTranslator.FromHtml("#181818"),
            ColorTranslator.FromHtml("#191919"), ColorTranslator.FromHtml("#1A1A1A"),
            ColorTranslator.FromHtml("#1B1B1B"), ColorTranslator.FromHtml("#1C1C1C"),
            ColorTranslator.FromHtml("#1D1D1D"), ColorTranslator.FromHtml("#1E1E1E"),
            ColorTranslator.FromHtml("#1F1F1F"), ColorTranslator.FromHtml("#202020"),
            ColorTranslator.FromHtml("#212121"), ColorTranslator.FromHtml("#222222"),
            ColorTranslator.FromHtml("#232323"), ColorTranslator.FromHtml("#242424"),
            ColorTranslator.FromHtml("#252525"), ColorTranslator.FromHtml("#262626"),
            ColorTranslator.FromHtml("#272727"), ColorTranslator.FromHtml("#282828"),
            ColorTranslator.FromHtml("#292929"), ColorTranslator.FromHtml("#2A2A2A"),
            ColorTranslator.FromHtml("#2B2B2B"), ColorTranslator.FromHtml("#2C2C2C"),
            ColorTranslator.FromHtml("#2D2D2D"), ColorTranslator.FromHtml("#2E2E2E"),
            ColorTranslator.FromHtml("#2F2F2F"), ColorTranslator.FromHtml("#303030"),
            ColorTranslator.FromHtml("#313131"), ColorTranslator.FromHtml("#323232"),
            ColorTranslator.FromHtml("#333333"), ColorTranslator.FromHtml("#343434"),
            ColorTranslator.FromHtml("#353535"), ColorTranslator.FromHtml("#363636"),
            ColorTranslator.FromHtml("#373737"), ColorTranslator.FromHtml("#383838"),
            ColorTranslator.FromHtml("#393939"), ColorTranslator.FromHtml("#3A3A3A"),
            ColorTranslator.FromHtml("#3B3B3B"), ColorTranslator.FromHtml("#3C3C3C"),
            ColorTranslator.FromHtml("#3D3D3D"), ColorTranslator.FromHtml("#3E3E3E"),
            ColorTranslator.FromHtml("#3F3F3F"), ColorTranslator.FromHtml("#404040"),
            ColorTranslator.FromHtml("#414141"), ColorTranslator.FromHtml("#424242"),
            ColorTranslator.FromHtml("#434343"), ColorTranslator.FromHtml("#444444"),
            ColorTranslator.FromHtml("#454545"), ColorTranslator.FromHtml("#464646"),
            ColorTranslator.FromHtml("#474747"), ColorTranslator.FromHtml("#484848"),
            ColorTranslator.FromHtml("#494949"), ColorTranslator.FromHtml("#4A4A4A"),
            ColorTranslator.FromHtml("#4B4B4B"), ColorTranslator.FromHtml("#4C4C4C"),
            ColorTranslator.FromHtml("#4D4D4D"), ColorTranslator.FromHtml("#4E4E4E"),
            ColorTranslator.FromHtml("#4F4F4F"), ColorTranslator.FromHtml("#505050"),
            ColorTranslator.FromHtml("#515151"), ColorTranslator.FromHtml("#525252"),
            ColorTranslator.FromHtml("#535353"), ColorTranslator.FromHtml("#545454"),
            ColorTranslator.FromHtml("#555555"), ColorTranslator.FromHtml("#565656"),
            ColorTranslator.FromHtml("#575757"), ColorTranslator.FromHtml("#585858"),
            ColorTranslator.FromHtml("#595959"), ColorTranslator.FromHtml("#5A5A5A"),
            ColorTranslator.FromHtml("#5B5B5B"), ColorTranslator.FromHtml("#5C5C5C"),
            ColorTranslator.FromHtml("#5D5D5D"), ColorTranslator.FromHtml("#5E5E5E"),
            ColorTranslator.FromHtml("#5F5F5F"), ColorTranslator.FromHtml("#606060"),
            ColorTranslator.FromHtml("#616161"), ColorTranslator.FromHtml("#626262"),
            ColorTranslator.FromHtml("#636363"), ColorTranslator.FromHtml("#646464"),
            ColorTranslator.FromHtml("#656565"), ColorTranslator.FromHtml("#666666"),
            ColorTranslator.FromHtml("#676767"), ColorTranslator.FromHtml("#686868"),
            ColorTranslator.FromHtml("#696969"), ColorTranslator.FromHtml("#6A6A6A"),
            ColorTranslator.FromHtml("#6B6B6B"), ColorTranslator.FromHtml("#6C6C6C"),
            ColorTranslator.FromHtml("#6D6D6D"), ColorTranslator.FromHtml("#6E6E6E"),
            ColorTranslator.FromHtml("#6F6F6F"), ColorTranslator.FromHtml("#707070"),
            ColorTranslator.FromHtml("#717171"), ColorTranslator.FromHtml("#727272"),
            ColorTranslator.FromHtml("#737373"), ColorTranslator.FromHtml("#747474"),
            ColorTranslator.FromHtml("#757575"), ColorTranslator.FromHtml("#767676"),
            ColorTranslator.FromHtml("#777777"), ColorTranslator.FromHtml("#787878"),
            ColorTranslator.FromHtml("#797979"), ColorTranslator.FromHtml("#7A7A7A"),
            ColorTranslator.FromHtml("#7B7B7B"), ColorTranslator.FromHtml("#7C7C7C"),
            ColorTranslator.FromHtml("#7D7D7D"), ColorTranslator.FromHtml("#7E7E7E"),
            ColorTranslator.FromHtml("#7F7F7F"), ColorTranslator.FromHtml("#808080"),
            ColorTranslator.FromHtml("#818181"), ColorTranslator.FromHtml("#828282"),
            ColorTranslator.FromHtml("#838383"), ColorTranslator.FromHtml("#848484"),
            ColorTranslator.FromHtml("#858585"), ColorTranslator.FromHtml("#868686"),
            ColorTranslator.FromHtml("#878787"), ColorTranslator.FromHtml("#888888"),
            ColorTranslator.FromHtml("#898989"), ColorTranslator.FromHtml("#8A8A8A"),
            ColorTranslator.FromHtml("#8B8B8B"), ColorTranslator.FromHtml("#8C8C8C"),
            ColorTranslator.FromHtml("#8D8D8D"), ColorTranslator.FromHtml("#8E8E8E"),
            ColorTranslator.FromHtml("#8F8F8F"), ColorTranslator.FromHtml("#909090"),
            ColorTranslator.FromHtml("#919191"), ColorTranslator.FromHtml("#929292"),
            ColorTranslator.FromHtml("#939393"), ColorTranslator.FromHtml("#949494"),
            ColorTranslator.FromHtml("#959595"), ColorTranslator.FromHtml("#969696"),
            ColorTranslator.FromHtml("#979797"), ColorTranslator.FromHtml("#989898"),
            ColorTranslator.FromHtml("#999999"), ColorTranslator.FromHtml("#9A9A9A"),
            ColorTranslator.FromHtml("#9B9B9B"), ColorTranslator.FromHtml("#9C9C9C"),
            ColorTranslator.FromHtml("#9D9D9D"), ColorTranslator.FromHtml("#9E9E9E"),
            ColorTranslator.FromHtml("#9F9F9F"), ColorTranslator.FromHtml("#A0A0A0"),
            ColorTranslator.FromHtml("#A1A1A1"), ColorTranslator.FromHtml("#A2A2A2"),
            ColorTranslator.FromHtml("#A3A3A3"), ColorTranslator.FromHtml("#A4A4A4"),
            ColorTranslator.FromHtml("#A5A5A5"), ColorTranslator.FromHtml("#A6A6A6"),
            ColorTranslator.FromHtml("#A7A7A7"), ColorTranslator.FromHtml("#A8A8A8"),
            ColorTranslator.FromHtml("#A9A9A9"), ColorTranslator.FromHtml("#AAAAAA"),
            ColorTranslator.FromHtml("#ABABAB"), ColorTranslator.FromHtml("#ACACAC"),
            ColorTranslator.FromHtml("#ADADAD"), ColorTranslator.FromHtml("#AEAEAE"),
            ColorTranslator.FromHtml("#AFAFAF"), ColorTranslator.FromHtml("#B0B0B0"),
            ColorTranslator.FromHtml("#B1B1B1"), ColorTranslator.FromHtml("#B2B2B2"),
            ColorTranslator.FromHtml("#B3B3B3"), ColorTranslator.FromHtml("#B4B4B4"),
            ColorTranslator.FromHtml("#B5B5B5"), ColorTranslator.FromHtml("#B6B6B6"),
            ColorTranslator.FromHtml("#B7B7B7"), ColorTranslator.FromHtml("#B8B8B8"),
            ColorTranslator.FromHtml("#B9B9B9"), ColorTranslator.FromHtml("#BABABA"),
            ColorTranslator.FromHtml("#BBBBBB"), ColorTranslator.FromHtml("#BCBCBC"),
            ColorTranslator.FromHtml("#BDBDBD"), ColorTranslator.FromHtml("#BEBEBE"),
            ColorTranslator.FromHtml("#BFBFBF"), ColorTranslator.FromHtml("#C0C0C0"),
            ColorTranslator.FromHtml("#C1C1C1"), ColorTranslator.FromHtml("#C2C2C2"),
            ColorTranslator.FromHtml("#C3C3C3"), ColorTranslator.FromHtml("#C4C4C4"),
            ColorTranslator.FromHtml("#C5C5C5"), ColorTranslator.FromHtml("#C6C6C6"),
            ColorTranslator.FromHtml("#C7C7C7"), ColorTranslator.FromHtml("#C8C8C8"),
            ColorTranslator.FromHtml("#C9C9C9"), ColorTranslator.FromHtml("#CACACA"),
            ColorTranslator.FromHtml("#CBCBCB"), ColorTranslator.FromHtml("#CCCCCC"),
            ColorTranslator.FromHtml("#CDCDCD"), ColorTranslator.FromHtml("#CECECE"),
            ColorTranslator.FromHtml("#CFCFCF"), ColorTranslator.FromHtml("#D0D0D0"),
            ColorTranslator.FromHtml("#D1D1D1"), ColorTranslator.FromHtml("#D2D2D2"),
            ColorTranslator.FromHtml("#D3D3D3"), ColorTranslator.FromHtml("#D4D4D4"),
            ColorTranslator.FromHtml("#D5D5D5"), ColorTranslator.FromHtml("#D6D6D6"),
            ColorTranslator.FromHtml("#D7D7D7"), ColorTranslator.FromHtml("#D8D8D8"),
            ColorTranslator.FromHtml("#D9D9D9"), ColorTranslator.FromHtml("#DADADA"),
            ColorTranslator.FromHtml("#DBDBDB"), ColorTranslator.FromHtml("#DCDCDC"),
            ColorTranslator.FromHtml("#DDDDDD"), ColorTranslator.FromHtml("#DEDEDE"),
            ColorTranslator.FromHtml("#DFDFDF"), ColorTranslator.FromHtml("#E0E0E0"),
            ColorTranslator.FromHtml("#E1E1E1"), ColorTranslator.FromHtml("#E2E2E2"),
            ColorTranslator.FromHtml("#E3E3E3"), ColorTranslator.FromHtml("#E4E4E4"),
            ColorTranslator.FromHtml("#E5E5E5"), ColorTranslator.FromHtml("#E6E6E6"),
            ColorTranslator.FromHtml("#E7E7E7"), ColorTranslator.FromHtml("#E8E8E8"),
            ColorTranslator.FromHtml("#E9E9E9"), ColorTranslator.FromHtml("#EAEAEA"),
            ColorTranslator.FromHtml("#EBEBEB"), ColorTranslator.FromHtml("#ECECEC"),
            ColorTranslator.FromHtml("#EDEDED"), ColorTranslator.FromHtml("#EEEEEE"),
            ColorTranslator.FromHtml("#EFEFEF"), ColorTranslator.FromHtml("#F0F0F0"),
            ColorTranslator.FromHtml("#F1F1F1"), ColorTranslator.FromHtml("#F2F2F2"),
            ColorTranslator.FromHtml("#F3F3F3"), ColorTranslator.FromHtml("#F4F4F4"),
            ColorTranslator.FromHtml("#F5F5F5"), ColorTranslator.FromHtml("#F6F6F6"),
            ColorTranslator.FromHtml("#F7F7F7"), ColorTranslator.FromHtml("#F8F8F8"),
            ColorTranslator.FromHtml("#F9F9F9"), ColorTranslator.FromHtml("#FAFAFA"),
            ColorTranslator.FromHtml("#FBFBFB")
        ];
        #endregion

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Get the current image from DetailPictureBox
            Bitmap? image = DetailPictureBox.Image as Bitmap;

            if (image == null)
            {
                MessageBox.Show("No image selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            Color originalColor;

            // Convert the image to grayscale
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    originalColor = image.GetPixel(x, y);

                    // Skip #000000 and #FFFFFF
                    if (originalColor.ToArgb() == Color.Black.ToArgb() || originalColor.ToArgb() == Color.White.ToArgb())
                    {
                        continue;
                    }

                    int brightness = (int)(originalColor.GetBrightness() * (grayscaleColors.Count - 1)); // Ensure the index is within the range

                    brightness = Math.Clamp(brightness, 0, grayscaleColors.Count - 1); // Ensure the index is within bounds

                    if (grayscaleColors[brightness] is Color color)
                    {
                        image.SetPixel(x, y, color);
                    }
                }
            }

            // Update the DetailPictureBox with the new grayscale image
            DetailPictureBox.Image = image;

            DetailPictureBox.Update();
        }
        #endregion

        #region [ SaveImageNameAndHexToTempToolStripMenuItem ]
        private void SaveImageNameAndHexToTempToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check whether one or more graphics are selected
            if (ItemsTileView.SelectedIndices.Count == 0)
            {
                MessageBox.Show("No graphics selected.");
                return;
            }

            // Pfad zum temporären Verzeichnis abrufen
            string programDirectory = Application.StartupPath;
            string tempDirectory = Path.Combine(programDirectory, "tempGrafic");

            // Create directory if it does not exist
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            // Liste der gespeicherten Dateipfade
            List<string> savedFiles = new List<string>();

            // Iterate through the selected graphics and save
            foreach (int selectedIndex in ItemsTileView.SelectedIndices)
            {
                int graphicId = _itemList[selectedIndex];
                ItemData itemData = TileData.ItemTable[graphicId];

                // Get graphics and details
                Bitmap bitmap = Art.GetStatic(graphicId);
                if (bitmap == null)
                {
                    MessageBox.Show($"Graphic with ID {graphicId} could not be retrieved.");
                    continue;
                }

                string hexAddress = $"0x{graphicId:X}";
                string imageName = itemData.Name;
                string fileName = $"{hexAddress}_{imageName}.bmp";

                // Save graphic
                string filePath = Path.Combine(tempDirectory, fileName);
                bitmap.Save(filePath, ImageFormat.Bmp);
                savedFiles.Add(filePath);
            }

            // Play sound
            string soundPath = Path.Combine(programDirectory, "Sound.wav");
            if (File.Exists(soundPath))
            {
                using (SoundPlayer player = new SoundPlayer(soundPath))
                {
                    player.Play();
                }
            }
            else
            {
                MessageBox.Show("Sound file not found.");
            }

            // Show MessageBox with the storage locations
            string message = "The following graphics were saved:\n" + string.Join("\n", savedFiles);
            MessageBox.Show(message);
        }
        #endregion

        #region [ Items Counter ]
        #region [ UpdateOccupiedItemCount ]
        private void UpdateOccupiedItemCount()
        {
            toolStripStatusLabelItemHowMuch.Text = $"Occupied Items: {occupiedItemCount}";
        }
        #endregion

        #region [ CountOccupiedItems ]
        private void CountOccupiedItems()
        {
            occupiedItemCount = 0;
            foreach (var itemId in _itemList)
            {
                if (IsItemOccupied(itemId))
                {
                    occupiedItemCount++;
                }
            }
            UpdateOccupiedItemCount();
        }
        #endregion

        #region [ IsItemOccupied ]
        private bool IsItemOccupied(int itemId)
        {
            // Check if the item has an associated graphic
            var bitmap = Art.GetStatic(itemId, out bool _);
            return bitmap != null;
        }
        #endregion

        #region [ countItemsToolStripMenuItem ]
        private void countItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CountOccupiedItems();
        }
        #endregion
        #endregion

        #region [ toolStripButtonColorImag ]
        private void toolStripButtonColorImage_Click(object sender, EventArgs e)
        {
            // Create a new form
            Form colorForm = new Form
            {
                Text = "Color Values for Selected Image",
                Width = 1000,
                Height = 970
            };

            // Hide the icon
            colorForm.ShowIcon = false;

            // Create a new SplitContainer
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };

            // Add the SplitContainer to the form
            colorForm.Controls.Add(splitContainer);

            // Create a new PictureBox with Scrollbars
            PictureBox pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.AutoSize
            };

            // Create a Panel to host the PictureBox and enable scrolling
            Panel picturePanel = new Panel
            {
                AutoScroll = true,
                Dock = DockStyle.Fill
            };
            picturePanel.Controls.Add(pictureBox);

            // Create a new RichTextBox
            RichTextBox colorBox = new RichTextBox
            {
                Dock = DockStyle.Fill
            };

            // Add the RichTextBox to the SplitContainer
            splitContainer.Panel2.Controls.Add(colorBox);

            // Add the Panel with the PictureBox to the SplitContainer
            splitContainer.Panel1.Controls.Add(picturePanel);

            // Get the selected image from the DetailPictureBox
            Bitmap selectedImage = DetailPictureBox.Image as Bitmap;

            // Check if the selectedImage is null
            if (selectedImage == null)
            {
                MessageBox.Show("No image selected in DetailPictureBox.");
                return;
            }

            // Calculate the zoomed image size
            int zoomFactor = 10;
            int zoomedWidth = selectedImage.Width * zoomFactor;
            int zoomedHeight = selectedImage.Height * zoomFactor;

            // Create a new bitmap to hold the zoomed image
            Bitmap zoomedImage = new Bitmap(zoomedWidth, zoomedHeight);

            using (Graphics g = Graphics.FromImage(zoomedImage))
            {
                // Draw the original image scaled by a factor of 10
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(selectedImage, new Rectangle(0, 0, zoomedWidth, zoomedHeight));
            }

            // Set the PictureBox's image to the zoomed image
            pictureBox.Image = zoomedImage;

            // Create a StringBuilder to collect color text
            StringBuilder colorTextBuilder = new StringBuilder();

            // Array to hold the line indices of the colorBox
            int[,] lineIndices = new int[selectedImage.Width, selectedImage.Height];

            // Populate the RichTextBox with color information excluding black and white
            Task.Run(() =>
            {
                int currentLineIndex = 0;
                for (int y = 0; y < selectedImage.Height; y++)
                {
                    for (int x = 0; x < selectedImage.Width; x++)
                    {
                        Color pixelColor = selectedImage.GetPixel(x, y);
                        string hexColor = ColorTranslator.ToHtml(pixelColor);

                        // Skip the colors #FFFFFF (white) and #000000 (black)
                        if (hexColor.Equals("#FFFFFF", StringComparison.OrdinalIgnoreCase) || hexColor.Equals("#000000", StringComparison.OrdinalIgnoreCase))
                        {
                            lineIndices[x, y] = -1; // Mark as skipped
                            continue;
                        }

                        // Create the text for the color
                        string colorText = $"Pixel ({x}, {y}): Color [R={pixelColor.R}, G={pixelColor.G}, B={pixelColor.B}], Hex: {hexColor}";
                        lineIndices[x, y] = currentLineIndex++; // Store the line index for this pixel

                        // Update the RichTextBox on the UI thread
                        colorBox.Invoke((Action)(() =>
                        {
                            int start = colorBox.Text.Length;
                            colorBox.AppendText(colorText);

                            // Add 5 spaces without color
                            colorBox.AppendText("     ");

                            // Add 5 spaces with the background color
                            int colorStart = colorBox.Text.Length;
                            colorBox.AppendText("     ");
                            colorBox.Select(colorStart, 5);
                            colorBox.SelectionBackColor = pixelColor;

                            // Add a new line
                            colorBox.AppendText("\n");
                        }));
                    }
                }
            });

            // Variable to store the previously highlighted line index
            int previousLineIndex = -1;

            pictureBox.MouseClick += (s, evt) =>
            {
                int x = evt.X / zoomFactor;
                int y = evt.Y / zoomFactor;

                if (x >= 0 && x < selectedImage.Width && y >= 0 && y < selectedImage.Height)
                {
                    int lineIndex = lineIndices[x, y];

                    // Highlight the line only if it's not skipped
                    if (lineIndex >= 0 && lineIndex < colorBox.Lines.Length)
                    {
                        // Clear previous highlight
                        if (previousLineIndex >= 0 && previousLineIndex < colorBox.Lines.Length)
                        {
                            int prevStart = colorBox.GetFirstCharIndexFromLine(previousLineIndex);
                            int prevLength = colorBox.Lines[previousLineIndex].IndexOf("Hex:") + 10; // Length up to and including the hex color
                            colorBox.Select(prevStart, prevLength);
                            colorBox.SelectionBackColor = colorBox.BackColor;

                            // Restore color background for color square
                            string prevLineText = colorBox.Lines[previousLineIndex];
                            Match prevMatch = Regex.Match(prevLineText, @"Hex: (#?[0-9A-Fa-f]{6})");
                            if (prevMatch.Success)
                            {
                                int prevColorStart = prevStart + prevLineText.Length + 5; // Color square position
                                colorBox.Select(prevColorStart, 5);
                                Color prevPixelColor = selectedImage.GetPixel(x, y);
                                colorBox.SelectionBackColor = prevPixelColor;
                            }
                        }

                        // Select the new line up to the hex color
                        int start = colorBox.GetFirstCharIndexFromLine(lineIndex);
                        int length = colorBox.Lines[lineIndex].IndexOf("Hex:") + 10; // Length up to and including the hex color
                        colorBox.Select(start, length);
                        colorBox.SelectionBackColor = Color.LightGray;

                        // Scroll to the selected line
                        colorBox.ScrollToCaret();

                        // Copy the hex color code to clipboard
                        string lineText = colorBox.Lines[lineIndex];
                        Match match = Regex.Match(lineText, @"Hex: (#?[0-9A-Fa-f]{6})");
                        if (match.Success)
                        {
                            Clipboard.SetText(match.Groups[1].Value);
                        }

                        // Update the previous line index
                        previousLineIndex = lineIndex;
                    }
                    else
                    {
                        // Handle the case where black or white is clicked
                        Color pixelColor = selectedImage.GetPixel(x, y);
                        string hexColor = ColorTranslator.ToHtml(pixelColor);
                        Clipboard.SetText(hexColor); // Copy the color code to the clipboard
                        MessageBox.Show($"Selected color {hexColor} is not listed in the RichTextBox.");
                    }
                }
            };

            // View the form
            colorForm.Show();
        }
        #endregion

        #region [ class PixelBox ]
        public class PixelBox : Control
        {
            public Bitmap Image { get; set; }
            public event Action<int, int> PixelSelected;

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (Image != null)
                {
                    for (int x = 0; x < Image.Width; x++)
                    {
                        for (int y = 0; y < Image.Height; y++)
                        {
                            Color pixelColor = Image.GetPixel(x, y);
                            using (Brush brush = new SolidBrush(pixelColor))
                            {
                                e.Graphics.FillRectangle(brush, x * 10, y * 10, 10, 10);
                            }
                        }
                    }
                }
            }

            #region OnMouseDown
            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                int x = e.X / 10;
                int y = e.Y / 10;

                PixelSelected?.Invoke(x, y);
            }
            #endregion
        }
        #endregion

        #region [ toolStripButtondrawRhombus ]
        private void toolStripButtondrawRhombus_Click(object sender, EventArgs e)
        {
            ToggleRhombusDrawing();

            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "sound.wav";
            player.Play();
        }
        #endregion

        #region [ ToggleRhombusDrawing ]
        private void ToggleRhombusDrawing()
        {
            isDrawingRhombusActive = !isDrawingRhombusActive;
            toolStripButtondrawRhombus.Checked = isDrawingRhombusActive;
            DetailPictureBox.Invalidate(); // Force repaint to show/hide the rhombus
        }
        #endregion

        #region [ DetailPictureBox_Paint ]
        private void DetailPictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (isDrawingRhombusActive)
            {
                DrawRhombus(e.Graphics);
            }
        }
        #endregion

        #region [ DrawRhombus ]
        private void DrawRhombus(Graphics g)
        {
            if (DetailPictureBox.Image == null)
            {
                return;
            }

            Point[] pointsUpper = new Point[4];
            pointsUpper[0] = new Point(DetailPictureBox.Image.Width / 2, 0);
            pointsUpper[1] = new Point(DetailPictureBox.Image.Width / 2 + 22, 22);
            pointsUpper[2] = new Point(DetailPictureBox.Image.Width / 2, 44);
            pointsUpper[3] = new Point(DetailPictureBox.Image.Width / 2 - 22, 22);

            g.DrawPolygon(Pens.Black, pointsUpper);
            g.DrawLine(Pens.Black, pointsUpper[0], new Point(pointsUpper[0].X, 0));
            g.DrawLine(Pens.Black, pointsUpper[1], new Point(pointsUpper[1].X, 0));
            g.DrawLine(Pens.Black, pointsUpper[3], new Point(pointsUpper[3].X, 0));

            int lineWidth = 100;
            int lineStartX = (DetailPictureBox.Image.Width - lineWidth) / 2;
            int lineEndX = lineStartX + lineWidth;
            int imageHeight = DetailPictureBox.Image.Height;
            g.DrawLine(Pens.Black, new Point(lineStartX, imageHeight - 66), new Point(lineEndX, imageHeight - 66));

            Point[] pointsLower = new Point[4];
            pointsLower[0] = new Point(DetailPictureBox.Image.Width / 2, imageHeight - 66);
            pointsLower[1] = new Point(DetailPictureBox.Image.Width / 2 + 22, imageHeight - 88);
            pointsLower[2] = new Point(DetailPictureBox.Image.Width / 2, imageHeight - 110);
            pointsLower[3] = new Point(DetailPictureBox.Image.Width / 2 - 22, imageHeight - 88);

            g.DrawPolygon(Pens.Black, pointsLower);
            g.DrawLine(Pens.Black, pointsLower[0], new Point(pointsLower[0].X, pointsLower[0].Y - 22));
            g.DrawLine(Pens.Black, pointsLower[1], new Point(pointsLower[1].X, pointsLower[1].Y - 22));
            g.DrawLine(Pens.Black, pointsLower[3], new Point(pointsLower[3].X, pointsLower[3].Y - 22));

            g.DrawLine(Pens.Black, pointsUpper[0], pointsLower[0]);
            g.DrawLine(Pens.Black, pointsUpper[1], pointsLower[1]);
            g.DrawLine(Pens.Black, pointsUpper[3], pointsLower[3]);
        }
        #endregion

        #region [ DetailPictureBox_MouseDoubleClick ] // Double Click - Picturebox 1000x1000 and zoom
        private float zoomFactor = 1.0f; // Initial zoom factor
        private const float zoomStep = 0.1f; // Increment/decrement for zooming

        #region [ DetailPictureBox_MouseDoubleClick ]
        private void DetailPictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Check if any image is selected in the ItemsTileView
            if (ItemsTileView.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Please select an image from the list.", "No image selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // If the form is not open or has been disposed, create a new one
            if (imageForm == null || imageForm.IsDisposed)
            {
                // Initialize a new form to display the image
                imageForm = new Form
                {
                    Text = "Enlarged image view",
                    Size = new Size(1020, 1020), // 1000x1000 plus margin
                    StartPosition = FormStartPosition.Manual,
                    Location = new Point(this.Location.X + this.Width + 10, this.Location.Y), // Position next to main form
                    FormBorderStyle = FormBorderStyle.FixedToolWindow, // Simple window, no maximize button
                    ShowIcon = false // Disable the icon in the title bar
                };

                // Initialize a new PictureBox to display the selected image
                imagePictureBox = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom, // Enable zoom support
                };

                // Add MouseWheel event handler for zoom functionality
                imagePictureBox.MouseWheel += ImagePictureBox_MouseWheel;

                imageForm.Controls.Add(imagePictureBox);
                imageForm.Show(); // Show the form as a non-modal window

                // Add a selection change event to dynamically update the image when selection changes
                ItemsTileView.ItemSelectionChanged += (s, ev) =>
                {
                    // Only update if a new item is selected and form is open
                    if (ev.IsSelected && imageForm != null && !imageForm.IsDisposed)
                    {
                        UpdateImageInForm();
                    }
                };
            }

            // Method to update the image in the PictureBox
            void UpdateImageInForm()
            {
                int selectedIndex = ItemsTileView.SelectedIndices[0];
                var bitmap = Art.GetStatic(_itemList[selectedIndex]);

                if (bitmap != null)
                {
                    // Reset zoom factor on new image load
                    zoomFactor = 1.0f;
                    imagePictureBox.Image = bitmap;
                    AdjustZoom();
                }
                else
                {
                    MessageBox.Show("The selected image could not be loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Initial image load
            UpdateImageInForm();
        }
        #endregion

        #region [ ImagePictureBox_MouseWheel ]
        // Handle MouseWheel event to zoom in and out
        private void ImagePictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            // Update zoom factor based on scroll direction
            if (e.Delta > 0)
            {
                zoomFactor += zoomStep; // Zoom in
            }
            else if (e.Delta < 0 && zoomFactor > zoomStep)
            {
                zoomFactor -= zoomStep; // Zoom out
            }
            AdjustZoom();
        }
        #endregion

        #region [ AdjustZoom ]
        // Adjust the PictureBox to the current zoom factor and center it
        private void AdjustZoom()
        {
            if (imagePictureBox.Image != null)
            {
                int newWidth = (int)(imagePictureBox.Image.Width * zoomFactor);
                int newHeight = (int)(imagePictureBox.Image.Height * zoomFactor);

                // Set the size based on zoom
                imagePictureBox.Size = new Size(newWidth, newHeight);

                // Center the PictureBox within the form
                imagePictureBox.Location = new Point(
                    (imageForm.ClientSize.Width - newWidth) / 2,
                    (imageForm.ClientSize.Height - newHeight) / 2
                );
            }
        }
        #endregion
        #endregion

        #region [ RemoveAllToolStripMenuItem ]
        private void RemoveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            DialogResult result = MessageBox.Show("Are you sure you want to remove all items?", "Remove all elements", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
            {
                return;
            }

            // Loop through all items in _itemList and remove
            foreach (var itemId in _itemList.ToList())
            {
                if (Art.IsValidStatic(itemId))
                {
                    Art.RemoveStatic(itemId);
                    ControlEvents.FireItemChangeEvent(this, itemId);
                }
            }

            // Clear list and refresh UI
            _itemList.Clear();
            ItemsTileView.VirtualListSize = _itemList.Count;
            SelectedGraphicId = 0; // Keine Auswahl
            ItemsTileView.Invalidate();

            // Update options
            Options.ChangedUltimaClass["Art"] = true;
            MessageBox.Show("All elements have been removed.", "Removal completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion
    }
}
