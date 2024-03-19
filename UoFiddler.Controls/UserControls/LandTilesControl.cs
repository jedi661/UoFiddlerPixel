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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class LandTilesControl : UserControl
    {
        public LandTilesControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _refMarker = this;
        }

        public bool IsLoaded { get; private set; }

        private const int _landTileMax = 0x4000;

        private static LandTilesControl _refMarker;
        private int _selectedGraphicId = -1;
        private readonly List<int> _tileList = new List<int>();
        private bool _showFreeSlots;

        public int SelectedGraphicId
        {
            get => _selectedGraphicId;
            set
            {
                _selectedGraphicId = value < 0 ? 0 : value;
                UpdateToolStripLabels(_selectedGraphicId);
                LandTilesTileView.FocusIndex = _tileList.IndexOf(_selectedGraphicId);
            }
        }

        /// <summary>
        /// Searches Objtype and Select
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public static bool SearchGraphic(int graphic)
        {
            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            if (_refMarker._tileList.All(t => t != graphic))
            {
                return false;
            }

            // we have to invalidate focus so it will scroll to item
            _refMarker.LandTilesTileView.FocusIndex = -1;
            _refMarker.SelectedGraphicId = graphic;

            return true;
        }

        /// <summary>
        /// Searches for name and selects
        /// </summary>
        /// <param name="name"></param>
        /// <param name="next">private bool Loaded = false;</param>
        /// <returns></returns>
        public static bool SearchName(string name, bool next)
        {
            int index = 0;
            if (next)
            {
                if (_refMarker._selectedGraphicId >= 0)
                {
                    index = _refMarker._tileList.IndexOf(_refMarker._selectedGraphicId) + 1;
                }

                if (index >= _refMarker._tileList.Count)
                {
                    index = 0;
                }
            }

            var searchMethod = SearchHelper.GetSearchMethod();

            for (int i = index; i < _refMarker._tileList.Count; ++i)
            {
                var searchResult = searchMethod(name, TileData.LandTable[_refMarker._tileList[i]].Name);
                if (searchResult.HasErrors)
                {
                    break;
                }

                if (!searchResult.EntryFound)
                {
                    continue;
                }

                // we have to invalidate focus so it will scroll to item
                _refMarker.LandTilesTileView.FocusIndex = -1;
                _refMarker.SelectedGraphicId = _refMarker._tileList[i];

                return true;
            }

            return false;
        }

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!IsLoaded)
            {
                return;
            }

            _selectedGraphicId = -1;
            _tileList.Clear();

            OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;

            _showFreeSlots = false;
            showFreeSlotsToolStripMenuItem.Checked = false;

            for (int i = 0; i < _landTileMax; ++i)
            {
                if (Art.IsValidLand(i))
                {
                    _tileList.Add(i);
                }
            }

            LandTilesTileView.VirtualListSize = _tileList.Count;
            UpdateTileView();

            if (!IsLoaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.LandTileChangeEvent += OnLandTileChangeEvent;
                ControlEvents.TileDataChangeEvent += OnTileDataChangeEvent;
            }

            IsLoaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void UpdateToolStripLabels(int graphic)
        {
            if (!IsLoaded)
            {
                return;
            }

            NameLabel.Text = $"Name: {TileData.LandTable[graphic].Name}";
            GraphicLabel.Text = string.Format("ID: 0x{0:X4} ({0})", graphic);
            FlagsLabel.Text = $"Flags: {TileData.LandTable[graphic].Flags}";
        }

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

            if (id < 0 || id > 0x3FFF)
            {
                return;
            }

            if (_selectedGraphicId != id)
            {
                return;
            }

            UpdateToolStripLabels(id);
        }

        private void OnLandTileChangeEvent(object sender, int index)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Art.IsValidLand(index))
            {
                bool done = false;
                for (int i = 0; i < _tileList.Count; ++i)
                {
                    if (index < _tileList[i])
                    {
                        _tileList.Insert(i, index);
                        done = true;
                        break;
                    }

                    if (index != _tileList[i])
                    {
                        continue;
                    }

                    done = true;
                    break;
                }

                if (!done)
                {
                    _tileList.Add(index);
                }
            }
            else
            {
                if (_showFreeSlots)
                {
                    return;
                }

                _tileList.Remove(index);
            }

            LandTilesTileView.VirtualListSize = _tileList.Count;
            LandTilesTileView.Invalidate();
        }

        private LandTileSearchForm _showForm;

        private void OnClickSearch(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new LandTileSearchForm(SearchGraphic, SearchName)
            {
                TopMost = true
            };
            _showForm.Show();
        }

        private void OnClickFindFree(object sender, EventArgs e)
        {
            if (_showFreeSlots)
            {
                int i = _selectedGraphicId > -1 ? _tileList.IndexOf(_selectedGraphicId) + 1 : 0;
                for (; i < _tileList.Count; ++i)
                {
                    if (Art.IsValidLand(_tileList[i]))
                    {
                        continue;
                    }

                    SelectedGraphicId = _tileList[i];
                    LandTilesTileView.Invalidate();
                    break;
                }
            }
            else
            {
                int id = _selectedGraphicId;
                ++id;

                for (int i = _tileList.IndexOf(_selectedGraphicId) + 1; i < _tileList.Count; ++i, ++id)
                {
                    if (id >= _tileList[i])
                    {
                        continue;
                    }

                    SelectedGraphicId = _tileList[i];
                    LandTilesTileView.Invalidate();
                    break;
                }
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (!Art.IsValidLand(_selectedGraphicId))
            {
                return;
            }

            DialogResult result =
                        MessageBox.Show($"Are you sure to remove {_selectedGraphicId}", "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Art.RemoveLand(_selectedGraphicId);
            ControlEvents.FireLandTileChangeEvent(this, _selectedGraphicId);

            if (!_showFreeSlots)
            {
                _tileList.Remove(_selectedGraphicId);
                LandTilesTileView.VirtualListSize = _tileList.Count;
                var moveToIndex = --_selectedGraphicId;
                SelectedGraphicId = moveToIndex <= 0 ? 0 : _selectedGraphicId; // TODO: get last index visible instead just curr -1
            }
            LandTilesTileView.Invalidate();

            Options.ChangedUltimaClass["Art"] = true;
        }

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

                    Art.ReplaceLand(_selectedGraphicId, bitmap);

                    ControlEvents.FireLandTileChangeEvent(this, _selectedGraphicId);

                    LandTilesTileView.Invalidate();

                    Options.ChangedUltimaClass["Art"] = true;
                }
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
            {
                InsertText.ForeColor = Art.IsValidLand(index) ? Color.Red : Color.Black;
            }
            else
            {
                InsertText.ForeColor = Color.Red;
            }
        }

        private void OnKeyDownInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            const int graphicIdMin = 0;
            const int graphicIdMax = 0x3FFF;

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, graphicIdMin, graphicIdMax))
            {
                return;
            }

            if (Art.IsValidLand(index))
            {
                return;
            }

            LandTilesContextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = $"Choose image file to insert at 0x{index:X}";
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

                    Art.ReplaceLand(index, bitmap);

                    ControlEvents.FireLandTileChangeEvent(this, index);

                    if (_showFreeSlots)
                    {
                        SelectedGraphicId = index;
                        UpdateToolStripLabels(index);
                    }
                    else
                    {
                        bool done = false;
                        for (int i = 0; i < _tileList.Count; ++i)
                        {
                            if (index >= _tileList[i])
                            {
                                continue;
                            }

                            _tileList.Insert(i, index);
                            done = true;
                            break;
                        }

                        if (!done)
                        {
                            _tileList.Add(index);
                        }

                        LandTilesTileView.VirtualListSize = _tileList.Count;
                        LandTilesTileView.Invalidate();
                        SelectedGraphicId = index;

                        Options.ChangedUltimaClass["Art"] = true;
                    }
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show("Are you sure? Will take a while", "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Art.Save(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(
                $"Saved to {Options.OutputPath}",
                "Save",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Art"] = false;
        }

        #region Save Format
        private void OnClickExportBmp(object sender, EventArgs e)
        {
            // Check if any items are selected
            if (LandTilesTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Iterate through the selected graphic IDs
            foreach (int selectedIndex in LandTilesTileView.SelectedIndices)
            {
                // Get the graphic for the selected item
                Bitmap bitmap = Art.GetLand(_tileList[selectedIndex]);
                // Check if the graphic exists
                if (bitmap != null)
                {
                    // Save the graphic                    
                    ExportLandTileImage(_tileList[selectedIndex], ImageFormat.Bmp);
                }
            }
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            // Check if any items are selected
            if (LandTilesTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Iterate through the selected graphic IDs
            foreach (int selectedIndex in LandTilesTileView.SelectedIndices)
            {
                // Get the graphic for the selected item
                Bitmap bitmap = Art.GetLand(_tileList[selectedIndex]);
                // Check if the graphic exists
                if (bitmap != null)
                {
                    // Save the graphic                 
                    ExportLandTileImage(_tileList[selectedIndex], ImageFormat.Tiff);
                }
            }
        }

        //Old version
        /*private void OnClickExportJpg(object sender, EventArgs e)
        {
            if (_selectedGraphicId < 0)
            {
                return;
            }

            ExportLandTileImage(_selectedGraphicId, ImageFormat.Jpeg);
        }*/

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            // Check if any items are selected
            if (LandTilesTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Iterate through the selected graphic IDs
            foreach (int selectedIndex in LandTilesTileView.SelectedIndices)
            {
                // Get the graphic for the selected item
                Bitmap bitmap = Art.GetLand(_tileList[selectedIndex]);
                // Check if the graphic exists
                if (bitmap != null)
                {
                    // Save the graphic                    
                    ExportLandTileImage(_tileList[selectedIndex], ImageFormat.Jpeg);
                }
            }
        }

        private void OnClickExportPng(object sender, EventArgs e)
        {
            // Check if any items are selected
            if (LandTilesTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            // Iterate through the selected graphic IDs
            foreach (int selectedIndex in LandTilesTileView.SelectedIndices)
            {
                // Get the graphic for the selected item
                Bitmap bitmap = Art.GetLand(_tileList[selectedIndex]);
                // Check if the graphic exists
                if (bitmap != null)
                {
                    // Save the graphic                    
                    ExportLandTileImage(_tileList[selectedIndex], ImageFormat.Png);
                }
            }
        }


        private static void ExportLandTileImage(int index, ImageFormat imageFormat)
        {
            if (!Art.IsValidLand(index))
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Landtile 0x{index:X4}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Art.GetLand(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show($"Landtile saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        #endregion

        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            if (_selectedGraphicId >= 0)
            {
                TileDataControl.Select(_selectedGraphicId, true);
            }
        }

        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (_selectedGraphicId >= 0)
            {
                RadarColorControl.Select(_selectedGraphicId, true);
            }
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Bmp);
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Tiff);
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Jpeg);
        }

        private void OnClick_SaveAllPng(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Png);
        }

        private void ExportAllLandTiles(ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                foreach (var index in _tileList)
                {
                    if (!Art.IsValidLand(index))
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Landtile 0x{index:X4}.{fileExtension}");
                    using (Bitmap bit = new Bitmap(Art.GetLand(index)))
                    {
                        bit.Save(fileName, imageFormat);
                    }
                }

                Cursor.Current = Cursors.Default;

                MessageBox.Show($"All land tiles saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void LandTilesTileView_DrawItem(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Point itemPoint = new Point(e.Bounds.X + LandTilesTileView.TilePadding.Left, e.Bounds.Y + LandTilesTileView.TilePadding.Top);
            const int fixedTileSize = 44;
            Size itemSize = new Size(fixedTileSize, fixedTileSize);
            Rectangle itemRec = new Rectangle(itemPoint, itemSize);

            var previousClip = e.Graphics.Clip;

            e.Graphics.Clip = new Region(itemRec);

            Bitmap bitmap = Art.GetLand(_tileList[e.Index], out bool patched);

            if (bitmap == null)
            {
                e.Graphics.Clip = new Region(itemRec);

                itemRec.X += 5;
                itemRec.Y += 5;

                itemRec.Width -= 10;
                itemRec.Height -= 10;

                e.Graphics.FillRectangle(Brushes.Red, itemRec);
                e.Graphics.Clip = previousClip;
            }
            else
            {
                if (patched)
                {
                    // different background for verdata patched tiles
                    e.Graphics.FillRectangle(Brushes.LightCoral, itemRec);
                }

                e.Graphics.DrawImage(bitmap, itemRec);

                e.Graphics.Clip = previousClip;
            }
        }



        private void LandTilesTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            LandTilesTileView.MultiSelect = true;

            if (!e.IsSelected)
            {
                return;
            }

            if (_tileList.Count == 0)
            {
                return;
            }

            SelectedGraphicId = e.ItemIndex < 0 || e.ItemIndex > _tileList.Count
                ? _tileList[0]
                : _tileList[e.ItemIndex];
        }

        private void ReplaceStartingFromTb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            const int graphicIdMin = 0;
            const int graphicIdMax = 0x3FFF;

            if (!Utils.ConvertStringToInt(ReplaceStartingFromTb.Text, out int index, graphicIdMin, graphicIdMax))
            {
                return;
            }

            LandTilesContextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose images to replace starting at 0x{index:X}";
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
                        AddSingleLandTile(dialog.FileNames[i], currentIdx);
                    }
                }

                LandTilesTileView.VirtualListSize = _tileList.Count;
                LandTilesTileView.Invalidate();
                SelectedGraphicId = index;

                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        /// <summary>
        /// Check if it's valid index for land tile. Land tiles has fixed size 0x4000.
        /// </summary>
        /// <param name="index">Starting Index</param>
        private static bool IsIndexValid(int index)
        {
            return index < 0x4000;
        }

        /// <summary>
        /// Adds a single land tile.
        /// </summary>
        /// <param name="fileName">Filename of the image to add.</param>
        /// <param name="index">Index where the land tile will be added.</param>
        private void AddSingleLandTile(string fileName, int index)
        {
            using (var bmpTemp = new Bitmap(fileName))
            {
                Bitmap bitmap = new Bitmap(bmpTemp);

                if (fileName.Contains(".bmp"))
                {
                    bitmap = Utils.ConvertBmp(bitmap);
                }

                Art.ReplaceLand(index, bitmap);

                ControlEvents.FireLandTileChangeEvent(this, index);

                bool done = false;

                for (int i = 0; i < _tileList.Count; ++i)
                {
                    if (index > _tileList[i])
                    {
                        continue;
                    }

                    _tileList[i] = index;
                    done = true;
                    break;
                }

                if (!done)
                {
                    _tileList.Add(index);
                }
            }
        }

        public void UpdateTileView()
        {
            LandTilesTileView.TileBorderColor = Options.RemoveTileBorder
                ? Color.Transparent
                : Color.Gray;

            var sameFocusColor = LandTilesTileView.TileFocusColor == Options.TileFocusColor;
            var sameSelectionColor = LandTilesTileView.TileHighlightColor == Options.TileSelectionColor;
            if (sameFocusColor && sameSelectionColor)
            {
                return;
            }

            LandTilesTileView.TileFocusColor = Options.TileFocusColor;
            LandTilesTileView.TileHighlightColor = Options.TileSelectionColor;
            LandTilesTileView.Invalidate();
        }

        private void ShowFreeSlotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;

            if (_showFreeSlots)
            {
                for (int j = 0; j < _landTileMax; ++j)
                {
                    if (_tileList.Count > j)
                    {
                        if (_tileList[j] != j)
                        {
                            _tileList.Insert(j, j);
                        }
                    }
                    else
                    {
                        _tileList.Insert(j, j);
                    }
                }

                var prevSelected = SelectedGraphicId;

                LandTilesTileView.VirtualListSize = _tileList.Count;

                if (prevSelected >= 0)
                {
                    SelectedGraphicId = prevSelected;
                }

                LandTilesTileView.Invalidate();
            }
            else
            {
                Reload();
            }
        }

        #region new Search Tab ´Landtiles

        private void SearchByNameToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            SearchName(searchByNameToolStripTextBox.Text, false);
        }

        private void SearchByNameToolStripButton_Click(object sender, EventArgs e)
        {
            SearchName(searchByNameToolStripTextBox.Text, true);
        }

        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Utils.ConvertStringToInt(searchByIdToolStripTextBox.Text, out int indexValue))
            {
                return;
            }

            const int maximumIndex = 0x3FFF;

            if (indexValue < 0)
            {
                indexValue = 0;
            }

            if (indexValue > maximumIndex)
            {
                indexValue = maximumIndex;
            }

            // we have to invalidate focus so it will scroll to item
            LandTilesTileView.FocusIndex = -1;
            SelectedGraphicId = indexValue;
        }
        #endregion

        #region Copy clipboard - Event handler for the click event of the copyToolStripMenuItem control
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a graphic is selected
            if (_selectedGraphicId >= 0)
            {
                // Get the bitmap of the selected graphic using the Art class
                Bitmap originalBitmap = Art.GetLand(_selectedGraphicId);
                if (originalBitmap != null)
                {
                    // Erstellen Sie eine Kopie des Originalbildes
                    Bitmap bitmap = new Bitmap(originalBitmap);

                    // Farbänderungsfunktion direkt eingebaut
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            Color pixelColor = bitmap.GetPixel(x, y);
                            if (pixelColor.R == 211 && pixelColor.G == 211 && pixelColor.B == 211) // Check if the color of the pixel is #D3D3D3
                            {
                                bitmap.SetPixel(x, y, Color.Black); // Change the color of the pixel to black
                            }
                        }
                    }

                    // Convert the image to a 24-bit color depth
                    Bitmap bmp24bit = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(bmp24bit))
                    {
                        g.DrawImage(bitmap, new Rectangle(0, 0, bmp24bit.Width, bmp24bit.Height));
                    }

                    // Copy the graphic to the clipboard
                    Clipboard.SetImage(bmp24bit);

                    // Show a message box indicating success
                    MessageBox.Show("The image has been copied to the clipboard!");
                }
                else
                {
                    // Show a message box indicating failure
                    MessageBox.Show("No image to copy!");
                }
            }
        }
        #endregion

        #region Cliport LandTiles - The graphics are imported from the clipboard.        
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the clipboard contains an image
            if (Clipboard.ContainsImage())
            {
                // Retrieve the image from the clipboard
                using (Bitmap bmp = new Bitmap(Clipboard.GetImage()))
                {
                    // Define the size of the desired graphic
                    int graphicSize = 44;

                    // Calculate the offset to center the graphic
                    int offsetX = (bmp.Width - graphicSize) / 2;
                    int offsetY = (bmp.Height - graphicSize) / 2;

                    // Determine the position of the selected graphic in the _tileList.
                    int index = _tileList.IndexOf(_selectedGraphicId);

                    if (index >= 0 && index < _tileList.Count)
                    {
                        // Create a new bitmap with the same size as the image from the clipboard
                        Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

                        // Define the colors to ignore
                        Color[] colorsToIgnore = new Color[]
                        {
                    Color.FromArgb(211, 211, 211), // #D3D3D3
                    Color.FromArgb(0, 0, 0),       // #000000
                    Color.FromArgb(255, 255, 255)  // #FFFFFF
                        };

                        // Iterate through each pixel of the image
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            for (int y = 0; y < bmp.Height; y++)
                            {
                                // Check if the current pixel is within the desired graphic bounds
                                if (x >= offsetX && x < offsetX + graphicSize && y >= offsetY && y < offsetY + graphicSize)
                                {
                                    // Get the color of the current pixel
                                    Color pixelColor = bmp.GetPixel(x, y);

                                    // Check if the color of the current pixel is one of the colors to ignore
                                    if (colorsToIgnore.Contains(pixelColor))
                                    {
                                        // Set the color of the current pixel to transparent
                                        newBmp.SetPixel(x, y, Color.Transparent);
                                    }
                                    else
                                    {
                                        // Set the color of the current pixel to the color of the original image
                                        newBmp.SetPixel(x, y, pixelColor);
                                    }
                                }
                                else
                                {
                                    // Set the color of pixels outside the desired graphic bounds to transparent
                                    newBmp.SetPixel(x, y, Color.Transparent);
                                }
                            }
                        }

                        // Call the OnClickReplace method with the selected graphic ID and the new bitmap
                        Art.ReplaceLand(index, newBmp);
                        ControlEvents.FireLandTileChangeEvent(this, index);

                        // Update the _tileList to insert the index only once
                        if (!_tileList.Contains(index))
                        {
                            _tileList.Add(index);
                            _tileList.Sort();
                        }

                        LandTilesTileView.VirtualListSize = _tileList.Count;
                        LandTilesTileView.Invalidate();
                        SelectedGraphicId = index;
                        Options.ChangedUltimaClass["Land Tiles"] = true;
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

        //Keydown Import Strg+V
        private void LandTilesControl_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Ctrl+V key combination has been pressed
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Calling the importToolStripMenuItem_Click method to import the graphic from the clipboard.
                importToolStripMenuItem_Click(sender, e);
            }
            // Checking if the Ctrl+X key combination has been pressed
            else if (e.Control && e.KeyCode == Keys.X)
            {
                // Calling the copyToolStripMenuItem_Click method to import the graphic from the clipboard.
                copyToolStripMenuItem_Click(sender, e);
            }
            // Verify that the Ctrl+J key combination was pressed
            else if (e.Control && e.KeyCode == Keys.J)
            {
                // Call the gotoMarkToolStripMenuItem_Click method to jump to the marked position
                gotoMarkToolStripMenuItem_Click(sender, e);
            }
        }

        //Ingnore to temp.
        /*private void importToTempToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                using (Bitmap bmp = new Bitmap(Clipboard.GetImage()))
                {
                    int graphicSize = 44;

                    int offsetX = (bmp.Width - graphicSize) / 2;
                    int offsetY = (bmp.Height - graphicSize) / 2;

                    int index = _tileList.IndexOf(_selectedGraphicId);

                    if (index >= 0 && index < _tileList.Count)
                    {
                        Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

                        Color[] colorsToIgnore = new Color[]
                        {
                    Color.FromArgb(211, 211, 211), // #D3D3D3
                    Color.FromArgb(0, 0, 0),       // #000000
                    Color.FromArgb(255, 255, 255)  // #FFFFFF
                        };

                        for (int x = 0; x < bmp.Width; x++)
                        {
                            for (int y = 0; y < bmp.Height; y++)
                            {
                                if (x >= offsetX && x < offsetX + graphicSize && y >= offsetY && y < offsetY + graphicSize)
                                {
                                    Color pixelColor = bmp.GetPixel(x, y);

                                    if (colorsToIgnore.Contains(pixelColor))
                                    {
                                        // Set the color of the current pixel to transparent
                                        newBmp.SetPixel(x, y, Color.Transparent);
                                    }
                                    else
                                    {
                                        newBmp.SetPixel(x, y, pixelColor);
                                    }
                                }
                                else
                                {
                                    newBmp.SetPixel(x, y, Color.Transparent);
                                }
                            }
                        }

                        // Create the "clipboardTemp" directory in the same directory as the main program
                        string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clipboardTemp");
                        Directory.CreateDirectory(directoryPath);

                        // Save the final bitmap to a file in the "clipboardTemp" directory with the selected index and an additional name "LandTiles"
                        string fileName = $"LandTiles_{index:X}.bmp";
                        string filePath = Path.Combine(directoryPath, fileName);
                        newBmp.Save(filePath);

                        // Import the saved bitmap
                        using (var bmpTemp = new Bitmap(filePath))
                        {
                            Bitmap bitmap = new Bitmap(bmpTemp);

                            Art.ReplaceLand(index, bitmap);

                            ControlEvents.FireLandTileChangeEvent(this, index);

                            if (!_tileList.Contains(index))
                            {
                                _tileList.Add(index);
                                _tileList.Sort();
                            }

                            LandTilesTileView.VirtualListSize = _tileList.Count;
                            LandTilesTileView.Invalidate();
                            SelectedGraphicId = index;
                            Options.ChangedUltimaClass["Land Tiles"] = true;
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
            }*/

        private void importToTempToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the clipboard contains an image
            if (Clipboard.ContainsImage())
            {
                // Retrieve the image from the clipboard
                using (Bitmap bmp = new Bitmap(Clipboard.GetImage()))
                {
                    // Define the size of the desired graphic
                    int graphicSize = 44;
                    // Calculate the offset to center the graphic
                    int offsetX = (bmp.Width - graphicSize) / 2;
                    int offsetY = (bmp.Height - graphicSize) / 2;
                    // Determine the position of the selected graphic in the _tileList.
                    int index = _tileList.IndexOf(_selectedGraphicId);

                    if (index >= 0 && index < _tileList.Count)
                    {
                        // Create a new bitmap with the same size as the image from the clipboard
                        Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

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
                            {
                                // Check if the current pixel is within the desired graphic bounds
                                if (x >= offsetX && x < offsetX + graphicSize && y >= offsetY && y < offsetY + graphicSize)
                                {
                                    // Get the color of the current pixel
                                    Color pixelColor = bmp.GetPixel(x, y);
                                    // Check if the color of the current pixel is one of the colors to Convert
                                    if (colorsToConvert.Contains(pixelColor))
                                    {   // Set the color of the current pixel to Convert
                                        newBmp.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                                    }
                                    else
                                    {
                                        // Set the color of the current pixel to the color of the original image
                                        newBmp.SetPixel(x, y, pixelColor);
                                    }
                                }
                                else
                                {
                                    // Set the color of pixels outside the desired graphic bounds to transparent
                                    newBmp.SetPixel(x, y, Color.Transparent);
                                }
                            }
                        }

                        // Create the "clipboardTemp" directory in the same directory as the main program
                        string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clipboardTemp");
                        Directory.CreateDirectory(directoryPath);

                        // Save the final bitmap to a file in the "clipboardTemp" directory with the selected index and an additional name "LandTiles"
                        string fileName = $"LandTiles_Hex_Adress_{index:X}.bmp";
                        string filePath = Path.Combine(directoryPath, fileName);
                        newBmp.Save(filePath);

                        // Import the saved bitmap
                        using (var bmpTemp = new Bitmap(filePath))
                        {
                            Bitmap bitmap = new Bitmap(bmpTemp);

                            Art.ReplaceLand(index, bitmap);

                            ControlEvents.FireLandTileChangeEvent(this, index);

                            if (!_tileList.Contains(index))
                            {
                                _tileList.Add(index);
                                _tileList.Sort();
                            }

                            LandTilesTileView.VirtualListSize = _tileList.Count;
                            LandTilesTileView.Invalidate();
                            SelectedGraphicId = index;
                            Options.ChangedUltimaClass["Land Tiles"] = true;
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

        #region Bitmap graphic 90 degrees to the left
        private void rotateBy90DegreesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a valid index is selected
            if (_selectedGraphicId < 0)
            {
                return;
            }
            // Get the graphic at the selected index
            Bitmap originalBmp = Art.GetLand(_selectedGraphicId);
            if (originalBmp == null)
            {
                return;
            }

            // Create a new Bitmap with the same size as the original graphic
            Bitmap rotatedBmp = new Bitmap(originalBmp.Height, originalBmp.Width);
            // Use the Graphics class to rotate the image 90 degrees to the left
            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                // Move the origin to the center of the Bitmap
                g.TranslateTransform((float)rotatedBmp.Width / 2, (float)rotatedBmp.Height / 2);
                // Rotate the image 90 degrees to the left
                g.RotateTransform(-90);
                // Move the origin back to the top left corner
                g.TranslateTransform(-(float)originalBmp.Width / 2, -(float)originalBmp.Height / 2);
                // Draw the original image onto the new Bitmap
                g.DrawImage(originalBmp, new Point(0, 0));
            }

            // Replace the graphic at the selected index with the rotated image
            Art.ReplaceLand(_selectedGraphicId, rotatedBmp);
            ControlEvents.FireLandTileChangeEvent(this, _selectedGraphicId);

            // Update the view
            LandTilesTileView.Invalidate();
        }
        #endregion

        #region Zoom Image
        private void zoomImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create a new shape
            Form zoomForm = new Form();

            // Load the image
            Image image = Art.GetLand(_selectedGraphicId);

            // Create a new PictureBox
            PictureBox pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = image, // Place the image on the selected LandTile
                SizeMode = PictureBoxSizeMode.Normal, // Set the SizeMode to Normal to not scale the image
                Width = image.Width, // Set the initial size to the size of the image
                Height = image.Height  // Set the initial size to the size of the image
            };
            zoomForm.Controls.Add(pictureBox);

            int zoomCount = 0; // Add a counter to track the number of zooms
            int currentId = _selectedGraphicId; // Add a variable to track the current ID

            // Create a button to zoom out
            Button zoomOutButton = new Button
            {
                Text = "Zoom out",
                Dock = DockStyle.Top
            };
            zoomOutButton.Click += (s, args) =>
            {
                if (zoomCount > 0)
                {
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Change the SizeMode to Zoom to scale the image
                    pictureBox.Width = (int)(pictureBox.Width / 1.1);
                    pictureBox.Height = (int)(pictureBox.Height / 1.1);
                    zoomCount--;
                }
                else if (zoomCount == 0)
                {
                    pictureBox.SizeMode = PictureBoxSizeMode.Normal; // Change the SizeMode to Normal to not scale the image
                    pictureBox.Width = image.Width; // Set the size to the size of the image
                    pictureBox.Height = image.Height; // Set the size to the size of the image
                    zoomCount--; // Reduce the zoomCount to prevent further zooming out
                }
            };
            zoomForm.Controls.Add(zoomOutButton);

            // Create a button to zoom in
            Button zoomInButton = new Button
            {
                Text = "Zoom in",
                Dock = DockStyle.Top
            };
            zoomInButton.Click += (s, args) =>
            {
                if (zoomCount < 2)
                {
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Change the SizeMode to Zoom to scale the image
                    pictureBox.Width = (int)(pictureBox.Width * 1.1);
                    pictureBox.Height = (int)(pictureBox.Height * 1.1);
                    zoomCount++;
                }
            };
            zoomForm.Controls.Add(zoomInButton);

            // Create a button to switch to the next ID
            Button nextIdButton = new Button
            {
                Text = "Forward",
                Dock = DockStyle.Bottom
            };
            nextIdButton.Click += (s, args) =>
            {
                currentId++; // Increase the current ID
                pictureBox.Image = Art.GetLand(currentId); // Load the image of the new ID
            };
            zoomForm.Controls.Add(nextIdButton);

            // Create a button to switch to previous ID
            Button previousIdButton = new Button
            {
                Text = "Backward",
                Dock = DockStyle.Bottom
            };
            previousIdButton.Click += (s, args) =>
            {
                if (currentId > 0) // Make sure the ID doesn't become negative
                {
                    currentId--; // Decrease the current ID
                    pictureBox.Image = Art.GetLand(currentId); // Load the image of the new ID
                }
            };
            zoomForm.Controls.Add(previousIdButton);

            // Create an OK button to close the form
            Button okButton = new Button
            {
                Text = "OK",
                Dock = DockStyle.Bottom
            };
            okButton.Click += (s, args) => zoomForm.Close();
            zoomForm.Controls.Add(okButton);

            // Display the shape
            zoomForm.ShowDialog();
        }
        #endregion

        #region markToolStripMenuItem
        // Variable for storing the marked position
        private int markedPosition = -1;
        private void markToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LandTilesTileView.SelectedIndices.Count > 0)
            {
                markedPosition = LandTilesTileView.SelectedIndices[0];
            }
        }
        #endregion

        #region gotoMarkToolStripMenuItem
        private void gotoMarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (markedPosition >= 0 && markedPosition < LandTilesTileView.VirtualListSize)
            {
                // Set the selected position to the marked position
                LandTilesTileView.SelectedIndices.Clear();
                LandTilesTileView.SelectedIndices.Add(markedPosition);
            }
        }
        #endregion
    }
}