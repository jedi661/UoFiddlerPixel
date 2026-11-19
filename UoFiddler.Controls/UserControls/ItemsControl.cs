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
using UoFiddler.Controls.ArtLibrary.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.UserControls.TileView;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace UoFiddler.Controls.UserControls
{
    public partial class ItemsControl : UserControl
    {

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

            InitializeExtraMenus();

            // the extension window (opened from any tab) tells every tab when an extension changed
            UoExtensions.Changed += kind =>
            {
                if ((kind == null || kind == UoExtensionKind.Items) && IsLoaded)
                {
                    Reload();
                }
            };
        }

        #region [ Extra menus (extended format / direction generator) ]
        private void InitializeExtraMenus()
        {
            var extendedItem = new ToolStripMenuItem("Extended formats (items 1M+, land tiles, textures, hues)...");
            extendedItem.Click += (s, e) => ShowExtendedFormatForm();
            MiscToolStripDropDownButton.DropDownItems.Add(new ToolStripSeparator());
            MiscToolStripDropDownButton.DropDownItems.Add(extendedItem);
            var sheetsItem = new ToolStripMenuItem("Sprite sheets (export / import a range)...");
            sheetsItem.Click += (s, e) =>
            {
                IReadOnlyList<int> sel = GetSelectedGraphicIds();
                new SpriteSheetBatchForm(BatchInsert, BatchEnd, sel.Count > 1 ? sel[0] : 0, sel.Count > 1 ? sel[^1] : Art.GetMaxExtendedItemId()).Show();
            };
            MiscToolStripDropDownButton.DropDownItems.Add(sheetsItem);

            var checkItem = new ToolStripMenuItem("Check extended files (what is saved)...");
            checkItem.Click += (s, e) => new ExtendedCheckForm(GoToId, () => UoExtensions.RaiseChanged(null)).Show();
            MiscToolStripDropDownButton.DropDownItems.Add(checkItem);

            var directionsButton = new ToolStripButton("Directions N/E/S/W", ToolIcons.Directions())
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ToolTipText = "Directions N/E/S/W" + Environment.NewLine +
                              "Generates the other facings of the selected item (mirror, turn, light, shadow)." + Environment.NewLine +
                              "Shows the UO field: the middle pixel of the lowest row is the fix point on the tile."
            };
            directionsButton.Click += (s, e) => ShowDirectionForm();
            ToolStrip.Items.Add(directionsButton);

            // three clear views: items only / items + free slots (the classic "Show Free Slots") / free slots only
            showFreeSlotsToolStripMenuItem.Text = "Show items + free slots";
            showFreeSlotsToolStripMenuItem.ToolTipText = "All ids: used items and empty slots in between (red = free)";

            _viewBox = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                ToolTipText = "Items only / items + free slots / free slots only (incl. the extended range 0x10000+)",
                AutoSize = false,
                Width = 140
            };
            _viewBox.Items.AddRange(new object[] { "Items only", "Items + free slots", "Free slots only" });
            _viewBox.SelectedIndex = 0;
            _viewBox.SelectedIndexChanged += (s, e) =>
            {
                if (_syncingView)
                {
                    return;
                }

                ShowSlots(_viewBox.SelectedIndex > 0, _viewBox.SelectedIndex == 2);
            };
            ToolStrip.Items.Add(new ToolStripLabel("View:"));
            ToolStrip.Items.Add(_viewBox);

            _onlyFreeMenuItem = new ToolStripMenuItem("Show only free slots");
            _onlyFreeMenuItem.Click += (s, e) => ShowSlots(!_showOnlyFree, !_showOnlyFree);
            var gotoExtended = new ToolStripMenuItem("Go to extended range (0x10000)");
            gotoExtended.Click += (s, e) => GoToExtendedRange();
            int freeIndex = TileViewContextMenuStrip.Items.IndexOf(showFreeSlotsToolStripMenuItem);
            TileViewContextMenuStrip.Items.Insert(freeIndex + 1, _onlyFreeMenuItem);
            TileViewContextMenuStrip.Items.Insert(freeIndex + 2, gotoExtended);

            var transformButton = new ToolStripButton("Transform", ToolIcons.Transform())
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                ToolTipText = "Transform (pixel exact)" + Environment.NewLine +
                              "Flip, rotate, skew, scale, distort, warp and move every grid dot - colours stay identical." + Environment.NewLine +
                              "UO field grid, position in the field (8 arrows), undo/redo; remembered per item."
            };
            transformButton.Click += (s, e) => ShowTransformForm();
            ToolStrip.Items.Add(transformButton);

            var transformMenu = new ToolStripMenuItem("Transform (skew / distort / warp)...");
            transformMenu.Click += (s, e) => ShowTransformForm();

            var directionsMenu = new ToolStripMenuItem("Generate directions (N/E/S/W)...");
            directionsMenu.Click += (s, e) => ShowDirectionForm();
            int mirrorIndex = TileViewContextMenuStrip.Items.IndexOf(mirrorToolStripMenuItem);
            TileViewContextMenuStrip.Items.Insert(mirrorIndex + 1, directionsMenu);
            TileViewContextMenuStrip.Items.Insert(mirrorIndex + 2, transformMenu);
        }


        private void ShowExtendedFormatForm()
        {
            ExtendedArtFormatForm.ShowFor(UoExtensionKind.Items, GoToId);
        }

        private ArtDirectionForm _directionForm;
        private ArtTransformForm _transformForm;

        private void ShowTransformForm()
        {
            if (_transformForm == null || _transformForm.IsDisposed)
            {
                _transformForm = new ArtTransformForm(InsertGeneratedItem);
                _transformForm.Show();
            }
            else
            {
                _transformForm.Activate();
            }

            if (_selectedGraphicId >= 0 && Art.IsValidStatic(_selectedGraphicId))
            {
                _transformForm.LoadSource(Art.GetStatic(_selectedGraphicId), _selectedGraphicId);
            }
        }

        private void ShowDirectionForm()
        {
            if (_directionForm == null || _directionForm.IsDisposed)
            {
                _directionForm = new ArtDirectionForm(InsertGeneratedItem);
                _directionForm.Show();
            }
            else
            {
                _directionForm.Activate();
            }

            if (_selectedGraphicId >= 0 && Art.IsValidStatic(_selectedGraphicId))
            {
                _directionForm.LoadSource(Art.GetStatic(_selectedGraphicId), _selectedGraphicId);
            }
        }

        /// <summary>Callback of the direction generator: writes a generated image into art (and the list).</summary>
        private void InsertGeneratedItem(int id, Bitmap bitmap)
        {
            if (bitmap == null || !Art.IsLegalStaticId(id))
            {
                MessageBox.Show($"0x{id:X} is not a valid item id (0x0000-0x{Art.GetMaxItemId():X4}" +
                                (Art.HasExtension ? $", 0x{ExtendedFormat.BaseId:X}-0x{Art.GetMaxExtendedItemId():X})" : ")"),
                    "Insert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Art.ReplaceStatic(id, bitmap);
            ControlEvents.FireItemChangeEvent(this, id);
            Options.ChangedUltimaClass["Art"] = true;

            InsertIntoItemList(id);
            ItemsTileView.VirtualListSize = _itemList.Count;
            ItemsTileView.Invalidate();
            SelectedGraphicId = id;
        }

        /// <summary>Inserts an id sorted into _itemList (no duplicates).</summary>
        private void InsertIntoItemList(int id)
        {
            int pos = _itemList.BinarySearch(id);
            if (pos < 0)
            {
                _itemList.Insert(~pos, id);
            }
        }

        private static bool HasTileData(int id) => TileData.ItemTable != null && id >= 0 && id < TileData.ItemTable.Length;

        private static string GetItemName(int id) => HasTileData(id) ? TileData.ItemTable[id].Name ?? string.Empty : string.Empty;
        #endregion

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

        /// <summary>Brings the Items tab to the front.</summary>
        public static void ActivateTab()
        {
            TabNavigation.BringToFront(RefMarker);
        }
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
                var searchResult = searchMethod(name, GetItemName(RefMarker._itemList[i]));
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
            _showOnlyFree = false;
            showFreeSlotsToolStripMenuItem.Checked = false;
            if (_onlyFreeMenuItem != null)
            {
                _onlyFreeMenuItem.Checked = false;
            }

            SyncViewBox();

            var prevSelected = SelectedGraphicId;

            // the extension may exist although Art was initialised before the paths were final
            if (Art.EnsureExtensionLoaded())
            {
                TileData.ReloadExtension();
            }

            int staticLength = Art.GetMaxExtendedItemId(); // includes the extended side-car range if present
            _itemList = new List<int>(Math.Min(staticLength + 1, 0x20000));
            for (int i = 0; i <= staticLength; ++i)
            {
                if (i > Art.GetMaxItemId() && i < ExtendedFormat.BaseId)
                {
                    i = ExtendedFormat.BaseId - 1; // skip the gap between art.mul and the extension
                    continue;
                }

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
                InsertIntoItemList(index);
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

            ItemData item = HasTileData(graphic) ? TileData.ItemTable[graphic] : default;
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

                Image oldDetail = DetailPictureBox.Image;
                DetailPictureBox.Image = newBit; // set the new image first, then free the old one
                oldDetail?.Dispose();
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

                Image oldDetail = DetailPictureBox.Image;
                DetailPictureBox.Image = newBit; // set the new image first, then free the old one
                oldDetail?.Dispose();

                Art.Measure(bit, out xMin, out yMin, out xMax, out yMax);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Name: {item.Name}");
            sb.AppendLine($"Graphic: 0x{graphic:X4}{(graphic >= ExtendedFormat.BaseId ? " (extended)" : string.Empty)}");
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
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";
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

                int firstIndex = indicesToRemove.Last();

                // Iterate through all selected indexes and remove artworks
                foreach (int selectedIndex in indicesToRemove)
                {
                    if (selectedIndex < 0 || selectedIndex >= _itemList.Count)
                    {
                        continue;
                    }

                    int graphicId = _itemList[selectedIndex];
                    if (Art.IsValidStatic(graphicId))
                    {
                        Art.RemoveStatic(graphicId);
                        ControlEvents.FireItemChangeEvent(this, graphicId);
                    }

                    // in "show free slots" mode the slot stays visible (now empty)
                    if (!_showFreeSlots)
                    {
                        _itemList.RemoveAt(selectedIndex);
                    }
                }

                // Update UI: keep the position instead of jumping to the very first item
                ItemsTileView.VirtualListSize = _itemList.Count;
                if (_itemList.Count > 0)
                {
                    SelectedGraphicId = _itemList[Math.Clamp(firstIndex - 1, 0, _itemList.Count - 1)];
                }

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
                    // select the previous entry of the list (the old code selected id-1, which is often not in the list)
                    int listIndex = _itemList.IndexOf(_selectedGraphicId);
                    _itemList.Remove(_selectedGraphicId);
                    ItemsTileView.VirtualListSize = _itemList.Count;
                    if (_itemList.Count > 0)
                    {
                        SelectedGraphicId = _itemList[Math.Clamp(listIndex - 1, 0, _itemList.Count - 1)];
                    }
                }
                ItemsTileView.Invalidate();
            }

            Options.ChangedUltimaClass["Art"] = true;
        }
        #endregion

        #region [ OnTextChangedInsert ]
        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxExtendedItemId()) && Art.IsLegalStaticId(index))
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

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxExtendedItemId()) || !Art.IsLegalStaticId(index))
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
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";

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

            NameLabel.Text = !Art.IsValidStatic(graphic) ? "Name: FREE" : $"Name: {GetItemName(graphic)}";
            GraphicLabel.Text = $"Graphic Hex: 0x{graphic:X4} ";
            toolStripStatusLabelGraficDecimal.Text = $"Graphic Decimal: {graphic}";
        }
        #endregion

        #region [ OnClickSave ]
        /*private void OnClickSave(object sender, EventArgs e)
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
        }*/

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure? Will take a while", "Save", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            // Runs on the UI thread on purpose: Art.Save reads the same cached GDI bitmaps the tile view draws,
            // and the progress dialog listens to Files.FileSaveEvent - both must not be touched from a worker
            // thread (the former Task.Run version caused cross-thread exceptions and "object in use" errors).
            Cursor.Current = Cursors.WaitCursor;
            int totalItems = Art.GetIdxLength() + Art.ExtendedCount;
            try
            {
                using (new ProgressBarDialog(totalItems, "Saving Art..."))
                {
                    Art.Save(Options.OutputPath);
                }

                Options.ChangedUltimaClass["Art"] = false;
                string ext = string.Empty;
                bool ok = true;
                if (Art.HasExtension)
                {
                    // read the written files back from disk and compare with memory
                    ok = ExtendedCheck.VerifySaved(Options.OutputPath, out string check);
                    ext = $"\n+ {ExtendedFormat.ArtIndexFileName} / {ExtendedFormat.ArtDataFileName} ({Art.ExtendedCount:N0} extended slots)\n\n{check}";

                    string loadedDir = Art.ExtendedDataPath != null ? Path.GetDirectoryName(Path.GetFullPath(Art.ExtendedDataPath)) : null;
                    if (loadedDir != null && !string.Equals(loadedDir.TrimEnd('\\'), Path.GetFullPath(Options.OutputPath).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
                    {
                        ext += $"\n\nNote: the extension is loaded from\n{loadedDir}\nAfter a restart UOFiddler shows the files from there - copy the saved files or use 'Load extension from folder'.";
                    }
                }

                MessageBox.Show($"Saved to {Options.OutputPath}{ext}", "Save", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion

        #region [ OnClickShowFreeSlots ]
        // This method is an event handler for a button or control click

        private void OnClickShowFreeSlots(object sender, EventArgs e)
        {
            // toggle: all slots (used + free) <-> only used items
            ShowSlots(!_showFreeSlots || _showOnlyFree, false);
        }

        private bool _showOnlyFree;
        private ToolStripMenuItem _onlyFreeMenuItem;
        private ToolStripComboBox _viewBox;
        private bool _syncingView;

        /// <summary>Keeps the toolbar view box in line with the current view without re-triggering it.</summary>
        private void SyncViewBox()
        {
            if (_viewBox == null)
            {
                return;
            }

            _syncingView = true;
            _viewBox.SelectedIndex = !_showFreeSlots ? 0 : _showOnlyFree ? 2 : 1;
            _syncingView = false;
        }

        /// <summary>
        /// showSlots=false: normal view (only used items). showSlots=true: every id from 0 to the highest id
        /// (incl. the extended range 0x10000+); with onlyFree only the free ids are listed. In both slot views
        /// a selected free slot can be filled with Replace, Ctrl+V or the Directions window.
        /// </summary>
        private void ShowSlots(bool showSlots, bool onlyFree)
        {
            if (!showSlots)
            {
                _showFreeSlots = false;
                _showOnlyFree = false;
                Reload(); // resets the menu checks too
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            _showFreeSlots = true; // "slot mode": inserting/removing never changes the list
            _showOnlyFree = onlyFree;
            showFreeSlotsToolStripMenuItem.Checked = !onlyFree;
            if (_onlyFreeMenuItem != null)
            {
                _onlyFreeMenuItem.Checked = onlyFree;
            }

            var prevSelected = SelectedGraphicId;

            // built in one go (the old insert-loop was O(n^2) and unusable for the 1M+ extended range)
            int max = Art.GetMaxExtendedItemId();
            _itemList = new List<int>(onlyFree ? 0x10000 : max + 1);
            int used = 0, free = 0;
            for (int j = 0; j <= max; ++j)
            {
                if (!Art.IsLegalStaticId(j))
                {
                    continue; // gap between art.mul and the extension (would write to item 0)
                }

                bool isUsed = Art.IsValidStatic(j);
                if (isUsed)
                {
                    ++used;
                }
                else
                {
                    ++free;
                }

                if (!onlyFree || !isUsed)
                {
                    _itemList.Add(j);
                }
            }

            ItemsTileView.VirtualListSize = _itemList.Count;
            if (prevSelected >= 0 && _itemList.Count > 0)
            {
                int pos = _itemList.BinarySearch(prevSelected);
                SelectedGraphicId = _itemList[Math.Min(pos < 0 ? ~pos : pos, _itemList.Count - 1)];
            }

            ItemsTileView.Invalidate();
            Cursor.Current = Cursors.Default;

            toolStripStatusLabelItemHowMuch.Text = onlyFree
                ? $"Free slots: {free:N0} (0x0000 - 0x{max:X})"
                : $"Items: {used:N0}  free: {free:N0}  (0x0000 - 0x{max:X})";
            SyncViewBox();
        }

        /// <summary>Inserts one graphic during a batch import (no redraw - call <see cref="BatchEnd"/> at the end).</summary>
        public void BatchInsert(int id, Bitmap bitmap)
        {
            if (bitmap == null || !Art.IsLegalStaticId(id))
            {
                return;
            }

            Art.ReplaceStatic(id, bitmap);
            ControlEvents.FireItemChangeEvent(this, id);
            Options.ChangedUltimaClass["Art"] = true;
            InsertIntoItemList(id);
        }

        /// <summary>Redraws the Items tab once after a batch import.</summary>
        public void BatchEnd()
        {
            ItemsTileView.VirtualListSize = _itemList.Count;
            ItemsTileView.Invalidate();
        }

        /// <summary>Ids of the slots selected in the Items tab (sorted), e.g. as targets for imported graphics.</summary>
        public static IReadOnlyList<int> GetSelectedGraphicIds()
        {
            ItemsControl m = RefMarker;
            if (m == null || !m.IsLoaded)
            {
                return Array.Empty<int>();
            }

            return m.ItemsTileView.SelectedIndices.Cast<int>()
                .Where(i => i >= 0 && i < m._itemList.Count)
                .Select(i => m._itemList[i])
                .Distinct()
                .OrderBy(id => id)
                .ToList();
        }

        /// <summary>Shows an id in the Items tab (free slots too: switches to the slot view if needed).</summary>
        public static void ShowGraphic(int id)
        {
            RefMarker?.GoToId(id);
        }

        /// <summary>Selects any id in the Items tab; switches to the slot view if the id is not in the current list.</summary>
        private void GoToId(int id)
        {
            if (!IsLoaded)
            {
                OnLoad(this, EventArgs.Empty);
            }

            if (!SearchGraphic(id))
            {
                ShowSlots(true, false);
                SearchGraphic(id);
            }

            FindForm()?.Activate();
        }

        /// <summary>Jumps to the first id of the extended range (switches to the slot view if it is empty).</summary>
        private void GoToExtendedRange()
        {
            if (!Art.HasExtension)
            {
                MessageBox.Show(
                    "No extended range is loaded." + Environment.NewLine + Environment.NewLine +
                    $"The files {ExtendedFormat.ArtIndexFileName}, {ExtendedFormat.ArtDataFileName} and {ExtendedFormat.TileDataFileName} " +
                    $"are searched in the Ultima Online folder:{Environment.NewLine}{Files.RootDir}" + Environment.NewLine + Environment.NewLine +
                    "Copy them there (they are saved into the output folder), or open Misc -> Extended Art Format " +
                    "and use 'Load extension from folder...' (or create a new one there).",
                    "Extended range", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int target = ExtendedFormat.BaseId;
            int pos = _itemList.BinarySearch(target);
            if (pos < 0 && ~pos >= _itemList.Count)
            {
                ShowSlots(true, _showOnlyFree);
                pos = _itemList.BinarySearch(target);
            }

            if (_itemList.Count == 0)
            {
                return;
            }

            ItemsTileView.FocusIndex = -1;
            SelectedGraphicId = _itemList[Math.Min(pos < 0 ? ~pos : pos, _itemList.Count - 1)];
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
            // (the old code created a hidden second TileDataControl here; it took over the static reference of the
            // TileData tab, so "Select in TileData" selected the item in an invisible control)
            if (_selectedGraphicId >= 0)
            {
                TileDataControl.Select(_selectedGraphicId, false);
                TileDataControl.ActivateTab();
            }
        }
        #endregion

        #region [ OnClickSelectRadarCol ]
        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            // ids 0x10000+ have their colours in radarcolext.mul
            if (_selectedGraphicId >= 0)
            {
                RadarColorControl.Select(_selectedGraphicId, false);
                RadarColorControl.ActivateTab();
            }
        }
        #endregion

        #region [ Misc Save ] // Save all items in a specified image format
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
        #endregion

        // This method exports all item images in a specified image format
        #region [ ExportAllItemImages ]
        private CancellationTokenSource _cancellationTokenSource;

        private void ExportAllItemImages(ImageFormat imageFormat)
        {
            // Get file extension for the specified image format
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                _cancellationTokenSource = new CancellationTokenSource(); // Initialize abort token

                Cursor.Current = Cursors.WaitCursor;

                int itemsExported = 0; // Counting variable for exported items

                using (var progressBarDialog = new ProgressBarDialog2(_itemList.Count, $"Export to {fileExtension}", false))
                {
                    progressBarDialog.CancelClicked += () => _cancellationTokenSource.Cancel(); // Link cancellation logic

                    try
                    {
                        foreach (var artItemIndex in _itemList)
                        {
                            // Check termination
                            if (_cancellationTokenSource.Token.IsCancellationRequested || progressBarDialog.IsCancelled)
                            {
                                MessageBox.Show("The export was aborted.", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                            }

                            // Update progress bar
                            ControlEvents.FireProgressChangeEvent();
                            Application.DoEvents();

                            int index = artItemIndex;
                            if (index < 0)
                            {
                                continue;
                            }

                            // Retrieve and save graphics
                            var artBitmap = Art.GetStatic(index);
                            if (artBitmap is null)
                            {
                                continue;
                            }

                            string fileName = Path.Combine(dialog.SelectedPath, $"Item 0x{index:X4}.{fileExtension}");
                            using (Bitmap bit = new Bitmap(artBitmap))
                            {
                                bit.Save(fileName, imageFormat);
                            }

                            itemsExported++; // Increment counter
                        }
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                        progressBarDialog.Dispose();
                    }
                }

                // Display the number of exported items at the end
                MessageBox.Show($"All items saved to {dialog.SelectedPath}\nTotal items exported: {itemsExported}", "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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

                // in the slot views write the id into the free slot, so gaps can be found at a glance
                if (_showFreeSlots && rect.Width >= 30)
                {
                    using var font = new Font(FontFamily.GenericSansSerif, rect.Width >= 60 ? 8f : 6.5f);
                    using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString($"{_itemList[e.Index]:X}", font, Brushes.White, rect, format);
                }

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

            SelectedGraphicId = itemIndex < 0 || itemIndex >= _itemList.Count
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
            if (!HasTileData(graphicId))
            {
                return;
            }

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
            if (SelectedGraphicId <= 0 || !HasTileData(SelectedGraphicId))
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

            if (!Utils.ConvertStringToInt(ReplaceStartingFromText.Text, out int index, 0, Art.GetMaxExtendedItemId()))
            {
                return;
            }

            TileViewContextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose image file replace starting at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";

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
                    // insert sorted (the old code overwrote the next list entry, which hid that item)
                    InsertIntoItemList(index);

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
            return Art.IsLegalStaticId(index);
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
                int graphicId = _itemList[selectedIndex];
                Bitmap source = Art.GetStatic(graphicId);
                // Check if the graphic exists
                if (source != null)
                {
                    // work on a copy - the old code changed the cached art image itself
                    using var bitmap = new Bitmap(source);

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
                    MessageBox.Show($"The image 0x{graphicId:X4} has been copied to the clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"No image to copy for 0x{graphicId:X4}!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    if (index >= 0 && index <= Art.GetMaxExtendedItemId())
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

                            InsertIntoItemList(index);
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
                int graphicId = _itemList[selectedIndex];
                Bitmap source = Art.GetStatic(graphicId);

                // Checking if the image is available.
                if (source != null)
                {
                    // Mirroring a copy horizontally.
                    var bitmap = new Bitmap(source);
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    // Replacing the original image with the mirrored image.
                    Art.ReplaceStatic(graphicId, bitmap);
                    ControlEvents.FireItemChangeEvent(this, graphicId);
                    Options.ChangedUltimaClass["Art"] = true;
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

            var maximumIndex = Art.GetMaxExtendedItemId();

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

            int id1 = _itemList[ItemsTileView.SelectedIndices[0]];
            int id2 = _itemList[ItemsTileView.SelectedIndices[1]];

            // copies, so both slots never share (and later dispose/modify) one bitmap
            Bitmap source1 = Art.GetStatic(id1);
            Bitmap source2 = Art.GetStatic(id2);
            Bitmap image1 = source1 == null ? null : new Bitmap(source1);
            Bitmap image2 = source2 == null ? null : new Bitmap(source2);

            // the old code did ReplaceStatic(id, null) for an empty slot, which made the original file
            // image re-appear, and it changed _selectedGraphicId behind the selection
            SwapSet(id1, image2);
            SwapSet(id2, image1);

            ItemsTileView.Invalidate();
            UpdateToolStripLabels(_selectedGraphicId);
            UpdateDetail(_selectedGraphicId);

            Options.ChangedUltimaClass["Art"] = true;
        }

        private void SwapSet(int id, Bitmap bitmap)
        {
            if (bitmap == null)
            {
                Art.RemoveStatic(id);
            }
            else
            {
                Art.ReplaceStatic(id, bitmap);
            }

            ControlEvents.FireItemChangeEvent(this, id);
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
                if (GetItemName(item).Contains(name, StringComparison.OrdinalIgnoreCase))
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

                        // pure greys #030303..#FBFBFB are changed (same set as the former 249-entry string list)
                        if (oldColor.R == oldColor.G && oldColor.G == oldColor.B && oldColor.R >= 0x03 && oldColor.R <= 0xFB)
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
            if (imageIndex < 0 || imageIndex >= _itemList.Count)
            {
                return;
            }

            // Load the image you want to display
            Image foregroundImage = Art.GetStatic(_itemList[imageIndex]);
            if (foregroundImage == null)
            {
                MessageBox.Show("The selected slot has no image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
            if (DetailPictureBox.Image is not Bitmap selectedImage)
            {
                return;
            }

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

                // Get graphics and details
                Bitmap bitmap = Art.GetStatic(graphicId);
                if (bitmap == null)
                {
                    MessageBox.Show($"Graphic with ID {graphicId} could not be retrieved.");
                    continue;
                }

                string hexAddress = $"0x{graphicId:X}";
                string imageName = GetItemName(graphicId).Trim();
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    imageName = imageName.Replace(c, '_');
                }

                string fileName = string.IsNullOrEmpty(imageName) ? $"{hexAddress}.bmp" : $"{hexAddress}_{imageName}.bmp";

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

        #region [ toolStripButtonColorImage ]
        private void toolStripButtonColorImage_Click(object sender, EventArgs e)
        {
            // Get the selected image from the DetailPictureBox
            Bitmap selectedImage = DetailPictureBox.Image as Bitmap;

            if (selectedImage == null)
            {
                MessageBox.Show("No image selected in DetailPictureBox.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create a snapshot to avoid cross-thread access on the original bitmap
            Bitmap imageCopy = new Bitmap(selectedImage);

            Form colorForm = new Form
            {
                Text = "Color Values for Selected Image",
                Width = 1000,
                Height = 970,
                ShowIcon = false
            };

            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };
            colorForm.Controls.Add(splitContainer);

            PictureBox pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.AutoSize
            };

            Panel picturePanel = new Panel
            {
                AutoScroll = true,
                Dock = DockStyle.Fill
            };
            picturePanel.Controls.Add(pictureBox);

            RichTextBox colorBox = new RichTextBox
            {
                Dock = DockStyle.Fill
            };

            splitContainer.Panel1.Controls.Add(picturePanel);
            splitContainer.Panel2.Controls.Add(colorBox);

            // Calculate the zoomed image
            int zoomFactor = 10;
            int zoomedWidth = imageCopy.Width * zoomFactor;
            int zoomedHeight = imageCopy.Height * zoomFactor;

            Bitmap zoomedImage = new Bitmap(zoomedWidth, zoomedHeight);
            using (Graphics g = Graphics.FromImage(zoomedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.DrawImage(imageCopy, new Rectangle(0, 0, zoomedWidth, zoomedHeight));
            }
            pictureBox.Image = zoomedImage;

            // Free zoomedImage when form closes
            colorForm.FormClosed += (s, ev) =>
            {
                pictureBox.Image = null; // detach before disposing
                zoomedImage.Dispose();
                imageCopy.Dispose();
            };

            // lineIndices[x, y] = line index in colorBox, or -1 if skipped
            int[,] lineIndices = new int[imageCopy.Width, imageCopy.Height];

            // Show the form BEFORE starting the background task so the handle exists
            colorForm.Show();

            // Variable to store the previously highlighted line index and its pixel coords
            int previousLineIndex = -1;
            int previousPixelX = -1;
            int previousPixelY = -1;

            // Populate colorBox in background
            Task.Run(() =>
            {
                int currentLineIndex = 0;
                for (int y = 0; y < imageCopy.Height; y++)
                {
                    for (int x = 0; x < imageCopy.Width; x++)
                    {
                        Color pixelColor = imageCopy.GetPixel(x, y);
                        string hexColor = ColorTranslator.ToHtml(pixelColor);

                        if (hexColor.Equals("#FFFFFF", StringComparison.OrdinalIgnoreCase) ||
                            hexColor.Equals("#000000", StringComparison.OrdinalIgnoreCase))
                        {
                            lineIndices[x, y] = -1;
                            continue;
                        }

                        string colorText = $"Pixel ({x}, {y}): Color [R={pixelColor.R}, G={pixelColor.G}, B={pixelColor.B}], Hex: {hexColor}";
                        lineIndices[x, y] = currentLineIndex++;

                        // Capture loop variables for the closure
                        Color capturedColor = pixelColor;
                        string capturedText = colorText;

                        // Bug fix: guard against disposed form/control
                        if (colorBox.IsDisposed || !colorBox.IsHandleCreated)
                        {
                            return;
                        }

                        colorBox.Invoke((Action)(() =>
                        {
                            if (colorBox.IsDisposed)
                            {
                                return;
                            }

                            // Append text
                            colorBox.AppendText(capturedText);

                            // 5 plain spaces as separator
                            colorBox.AppendText("     ");

                            // 5 spaces with pixel background color as color swatch
                            int colorStart = colorBox.Text.Length;
                            colorBox.AppendText("     ");
                            colorBox.Select(colorStart, 5);
                            colorBox.SelectionBackColor = capturedColor;

                            colorBox.AppendText("\n");
                        }));
                    }
                }
            });

            pictureBox.MouseClick += (s, evt) =>
            {
                int x = evt.X / zoomFactor;
                int y = evt.Y / zoomFactor;

                if (x < 0 || x >= imageCopy.Width || y < 0 || y >= imageCopy.Height)
                {
                    return;
                }

                int lineIndex = lineIndices[x, y];

                if (lineIndex >= 0 && lineIndex < colorBox.Lines.Length)
                {
                    // --- Clear previous highlight ---
                    if (previousLineIndex >= 0 && previousLineIndex < colorBox.Lines.Length)
                    {
                        int prevStart = colorBox.GetFirstCharIndexFromLine(previousLineIndex);
                        string prevLine = colorBox.Lines[previousLineIndex];
                        int prevLength = prevLine.IndexOf("Hex:") + 10;
                        colorBox.Select(prevStart, prevLength);
                        colorBox.SelectionBackColor = colorBox.BackColor;

                        // Bug fix: restore color swatch with the PREVIOUS pixel's color, not the current one
                        Match prevMatch = Regex.Match(prevLine, @"Hex: (#?[0-9A-Fa-f]{6})");
                        if (prevMatch.Success && previousPixelX >= 0 && previousPixelY >= 0)
                        {
                            // Position of color swatch: after text + 5 separator spaces
                            int prevSwatchStart = prevStart + prevLine.Length + 5;
                            colorBox.Select(prevSwatchStart, 5);
                            Color prevPixelColor = imageCopy.GetPixel(previousPixelX, previousPixelY);
                            colorBox.SelectionBackColor = prevPixelColor;
                        }
                    }

                    // --- Highlight new line ---
                    int start = colorBox.GetFirstCharIndexFromLine(lineIndex);
                    int length = colorBox.Lines[lineIndex].IndexOf("Hex:") + 10;
                    colorBox.Select(start, length);
                    colorBox.SelectionBackColor = Color.LightGray;
                    colorBox.ScrollToCaret();

                    // Copy hex color to clipboard
                    string lineText = colorBox.Lines[lineIndex];
                    Match match = Regex.Match(lineText, @"Hex: (#?[0-9A-Fa-f]{6})");
                    if (match.Success)
                    {
                        Clipboard.SetText(match.Groups[1].Value);
                    }

                    previousLineIndex = lineIndex;
                    previousPixelX = x;
                    previousPixelY = y;
                }
                else
                {
                    // Black or white pixel: still copy hex to clipboard, no MessageBox spam
                    Color pixelColor = imageCopy.GetPixel(x, y);
                    string hexColor = ColorTranslator.ToHtml(pixelColor);
                    Clipboard.SetText(hexColor);
                }
            };
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

            // optional click sound - the old code threw FileNotFoundException when sound.wav was missing
            string soundPath = Path.Combine(Application.StartupPath, "sound.wav");
            if (File.Exists(soundPath))
            {
                using var player = new SoundPlayer(soundPath);
                player.Play();
            }
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

                // subscribe only once (the old code added one more handler for every newly opened window)
                if (!_enlargedViewHooked)
                {
                    _enlargedViewHooked = true;
                    ItemsTileView.ItemSelectionChanged += (s, ev) =>
                    {
                        // Only update if a new item is selected and form is open
                        if (ev.IsSelected && imageForm != null && !imageForm.IsDisposed)
                        {
                            UpdateImageInForm(false);
                        }
                    };
                }
            }

            // Initial image load
            UpdateImageInForm(true);
        }

        private bool _enlargedViewHooked;

        // Method to update the image in the PictureBox
        private void UpdateImageInForm(bool showError)
        {
            if (ItemsTileView.SelectedIndices.Count == 0 || imagePictureBox == null)
            {
                return;
            }

            int selectedIndex = ItemsTileView.SelectedIndices[0];
            if (selectedIndex < 0 || selectedIndex >= _itemList.Count)
            {
                return;
            }

            var bitmap = Art.GetStatic(_itemList[selectedIndex]);

            if (bitmap != null)
            {
                // Reset zoom factor on new image load
                zoomFactor = 1.0f;
                imagePictureBox.Image = bitmap;
                AdjustZoom();
            }
            else if (showError)
            {
                MessageBox.Show("The selected image could not be loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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


        #region [ toolStripButtonArtWorkGallery_Click ]
        private ArtworkGallery artworkGalleryInstance;
        private void toolStripButtonArtWorkGallery_Click(object sender, EventArgs e)
        {
            if (artworkGalleryInstance == null || artworkGalleryInstance.IsDisposed)
            {
                artworkGalleryInstance = new ArtworkGallery();
                artworkGalleryInstance.Show();
                artworkGalleryInstance.InitializeGallery(_selectedGraphicId); // NEU HINZUFÜGEN
            }
            else
            {
                artworkGalleryInstance.Focus();
                artworkGalleryInstance.InitializeGallery(_selectedGraphicId); // NEU HINZUFÜGEN
            }
        }
        #endregion

        #region [ ToolStripButtonArtLibrary_Click ]
        private ArtLibraryForm _artLibraryForm;
        private void ToolStripButtonArtLibrary_Click(object sender, EventArgs e)
        {
            // the art library is an independent window next to the items tab (single instance)
            if (_artLibraryForm == null || _artLibraryForm.IsDisposed)
            {
                _artLibraryForm = new ArtLibraryForm();
                _artLibraryForm.Show();
            }
            else
            {
                if (_artLibraryForm.WindowState == FormWindowState.Minimized)
                {
                    _artLibraryForm.WindowState = FormWindowState.Normal;
                }

                _artLibraryForm.Activate();
            }
        }
        #endregion
    }
}
