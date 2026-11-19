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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using static UoFiddler.Controls.UserControls.LandTilesControl;

namespace UoFiddler.Controls.UserControls
{
    public partial class LandTilesControl : UserControl
    {
        public LandTilesControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _refMarker = this;
            InitializeExtensionMenus();
        }

        public bool IsLoaded { get; private set; }

        /// <summary>Number of land ids incl. the land extension (0x4000 without).</summary>
        private static int LandTileMax => Art.GetMaxLandId() + 1;

        #region [ Extension (land ids 0x4000+) ]
        private void InitializeExtensionMenus()
        {
            var extItem = new ToolStripMenuItem("Extended land tiles (0x4000+)...");
            extItem.ToolTipText = "Create / resize / save / load the land tile extension (artlandext*, tiledatalandext, radarcollandext).";
            extItem.Click += (s, e) => ExtendedArtFormatForm.ShowFor(UoExtensionKind.LandTiles);
            var gotoExt = new ToolStripMenuItem("Go to extended range (0x4000)");
            gotoExt.Click += (s, e) =>
            {
                if (!Art.HasLandExtension)
                {
                    MessageBox.Show("There is no land tile extension yet. Create it with Misc -> Extended land tiles.", "Land tiles",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int target = _tileList.FirstOrDefault(t => t >= ExtendedFormat.LandBaseId, -1);
                if (target < 0)
                {
                    if (!_showFreeSlots)
                    {
                        ShowFreeSlotsToolStripMenuItem_Click(this, EventArgs.Empty);
                    }

                    target = ExtendedFormat.LandBaseId;
                }

                LandTilesTileView.FocusIndex = -1;
                SelectedGraphicId = target;
            };
            MiscToolStripDropDownButton.DropDownItems.Add(new ToolStripSeparator());
            MiscToolStripDropDownButton.DropDownItems.Add(extItem);
            MiscToolStripDropDownButton.DropDownItems.Add(gotoExt);

            // view: tiles / tiles + free slots / free slots / extended range (the old "Show free slots" switch)
            _viewBox = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 150,
                ToolTipText = "Land tiles only / land tiles + free slots / free slots only / extended range (0x4000+)"
            };
            _viewBox.Items.AddRange(new object[] { "Land tiles only", "Land tiles + free slots", "Free slots only", "Extended range (0x4000+)" });
            _viewBox.SelectedIndex = 0;
            _viewBox.SelectedIndexChanged += (s, e) =>
            {
                if (!_syncingView)
                {
                    SetViewMode(_viewBox.SelectedIndex);
                }
            };
            LandTilesToolStrip.Items.Add(new ToolStripSeparator());
            LandTilesToolStrip.Items.Add(new ToolStripLabel("View:"));
            LandTilesToolStrip.Items.Add(_viewBox);

            var studio = new ToolStripButton("Land + texture studio")
            {
                ToolTipText = "Make a land tile AND its texture from one image (client orientation), check it on a map preview,\n" +
                              "patch both into their slots and link them in the tiledata."
            };
            studio.Click += (s, e) => LandTextureStudioForm.ShowStudio(_selectedGraphicId >= 0 && Art.IsValidLand(_selectedGraphicId) ? _selectedGraphicId : -1);
            var check = new ToolStripButton("Check...")
            {
                ToolTipText = "Find empty / one colour placeholder land tiles and textures (and delete them),\n" +
                              "check the link land tile <-> texture (missing texture, wrong orientation)."
            };
            check.Click += (s, e) => TerrainCheckForm.ShowCheck();
            LandTilesToolStrip.Items.Add(new ToolStripSeparator());
            LandTilesToolStrip.Items.Add(studio);
            LandTilesToolStrip.Items.Add(check);

            // direct jumps for the selected land tile: its tiledata entry and its texture
            var tileDataButton = new ToolStripButton("Show in TileData") { ToolTipText = "Shows the tiledata entry of the selected land tile (name, flags, texture id)" };
            tileDataButton.Click += (s, e) => OnClickSelectTiledata(s, e);
            var textureButton = new ToolStripButton("Show texture") { ToolTipText = "Shows the texture the tiledata of the selected land tile points to" };
            textureButton.Click += (s, e) => ShowTextureOfSelected();
            LandTilesToolStrip.Items.Add(new ToolStripSeparator());
            LandTilesToolStrip.Items.Add(tileDataButton);
            LandTilesToolStrip.Items.Add(textureButton);

            var studioMenu = new ToolStripMenuItem("Open in land + texture studio");
            studioMenu.Click += (s, e) => LandTextureStudioForm.ShowStudio(_selectedGraphicId);
            var textureMenu = new ToolStripMenuItem("Show its texture (Textures tab)");
            textureMenu.Click += (s, e) => ShowTextureOfSelected();
            LandTilesContextMenuStrip.Items.Insert(0, textureMenu);
            LandTilesContextMenuStrip.Items.Insert(0, studioMenu);
            LandTilesContextMenuStrip.Items.Insert(2, new ToolStripSeparator());

            UoExtensions.Changed += kind =>
            {
                if (kind == null || kind == UoExtensionKind.LandTiles)
                {
                    Reload();
                }
            };
        }
        #endregion

        private static LandTilesControl _refMarker;
        private int _selectedGraphicId = -1;
        private readonly List<int> _tileList = new List<int>();
        private bool _showFreeSlots;

        private void ShowTextureOfSelected()
        {
            int tex = _selectedGraphicId >= 0 && TileData.LandTable != null && _selectedGraphicId < TileData.LandTable.Length
                ? TileData.LandTable[_selectedGraphicId].TextureId
                : 0;
            if (_selectedGraphicId < 0)
            {
                return;
            }

            if (tex <= 0 || !Textures.TestTexture(tex))
            {
                MessageBox.Show(tex <= 0
                        ? $"Land tile 0x{_selectedGraphicId:X} has no texture id in the tiledata (flat only)."
                        : $"The tiledata of land tile 0x{_selectedGraphicId:X} points to texture 0x{tex:X}, but that texture does not exist.",
                    "Texture", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TexturesControl.SelectInTab(tex);
        }
        private int _viewMode;              // 0 tiles, 1 tiles + free, 2 free only, 3 extended range
        private ToolStripComboBox _viewBox;
        private bool _syncingView;

        /// <summary>Fills the list for the current view (incl. the land extension).</summary>
        private void BuildList()
        {
            _tileList.Clear();
            int max = LandTileMax;
            for (int id = 0; id < max; ++id)
            {
                bool valid = Art.IsValidLand(id);
                bool show = _viewMode switch
                {
                    1 => true,
                    2 => !valid,
                    3 => id >= ExtendedFormat.LandBaseId,
                    _ => valid
                };
                if (show)
                {
                    _tileList.Add(id);
                }
            }
        }

        private void SetViewMode(int mode)
        {
            int previous = _selectedGraphicId;
            _viewMode = mode;
            _showFreeSlots = mode != 0;
            showFreeSlotsToolStripMenuItem.Checked = _showFreeSlots;
            if (_viewBox != null && _viewBox.SelectedIndex != mode)
            {
                _syncingView = true;
                _viewBox.SelectedIndex = mode;
                _syncingView = false;
            }

            if (!IsLoaded)
            {
                return;
            }

            BuildList();
            LandTilesTileView.VirtualListSize = _tileList.Count;
            if (previous >= 0 && _tileList.Contains(previous))
            {
                LandTilesTileView.FocusIndex = -1;
                SelectedGraphicId = previous;
            }

            LandTilesTileView.Invalidate();
        }

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

            if (!_refMarker._tileList.Contains(graphic))
            {
                if (!Art.IsLegalLandId(graphic))
                {
                    return false;
                }

                // e.g. a free slot or the extended range: show all ids so it can be selected
                _refMarker.SetViewMode(1);
                if (!_refMarker._tileList.Contains(graphic))
                {
                    return false;
                }
            }

            // we have to invalidate focus so it will scroll to item
            _refMarker.LandTilesTileView.FocusIndex = -1;
            _refMarker.SelectedGraphicId = graphic;

            return true;
        }

        /// <summary>Brings the LandTiles tab to the front.</summary>
        public static void ActivateTab()
        {
            TabNavigation.BringToFront(_refMarker);
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

            _showFreeSlots = _viewMode != 0;
            showFreeSlotsToolStripMenuItem.Checked = _showFreeSlots;
            BuildList();

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

            bool hasData = TileData.LandTable != null && graphic >= 0 && graphic < TileData.LandTable.Length;
            NameLabel.Text = $"Name: {(hasData ? TileData.LandTable[graphic].Name : string.Empty)}";
            GraphicLabel.Text = string.Format("ID: 0x{0:X4} ({0})", graphic) + (graphic >= ExtendedFormat.LandBaseId ? "  [extended]" : string.Empty);
            FlagsLabel.Text = $"Flags: {(hasData ? TileData.LandTable[graphic].Flags : 0)}";
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

            ControlEvents.DecodeTileDataEventId(id, out id, out bool land);
            if (!land)
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

                    Art.ReplaceLand(_selectedGraphicId, bitmap);

                    ControlEvents.FireLandTileChangeEvent(this, _selectedGraphicId);

                    LandTilesTileView.Invalidate();

                    Options.ChangedUltimaClass["Art"] = true;
                }
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxLandId()))
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
            int graphicIdMax = Art.GetMaxLandId();

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

        #region [ OnClickExportBmp ]
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
        #endregion

        #region [ OnClickExportTiff ]
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
        #endregion

        #region [ OnClickExportJpg ]
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
        #endregion

        #region [ OnClickExportPng ]
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
        #endregion

        #region [ ExportLandTileImage ]
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

        #region [ OnClickSelectTiledata ]
        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            if (_selectedGraphicId >= 0)
            {
                TileDataControl.ActivateTab(); // show the tab first, then select (so the list scrolls to the entry)
                TileDataControl.Select(_selectedGraphicId, true);
            }
        }
        #endregion

        #region [ OnClickSelectRadarCol ]
        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (_selectedGraphicId >= 0)
            {
                RadarColorControl.ActivateTab(); // show the tab first, then select (so the list scrolls to the entry)
                RadarColorControl.Select(_selectedGraphicId, true);
            }
        }
        #endregion

        #region [ OnClick_SaveAllBmp ]
        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Bmp);
        }
        #endregion

        #region [ OnClick_SaveAllTiff ]
        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Tiff);
        }
        #endregion

        #region [ OnClick_SaveAllJpg ]
        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Jpeg);
        }
        #endregion

        #region [ OnClick_SaveAllPng ]
        private void OnClick_SaveAllPng(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Png);
        }
        #endregion

        #region [ ExportAllLandTiles ]

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

                var progressBarDialog = new ProgressBarDialog2(_tileList.Count, $"Exporting Land Tiles to {fileExtension}", false);
                progressBarDialog.CancelClicked += () =>
                {
                    MessageBox.Show("Export was aborted.", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                progressBarDialog.Show();

                Cursor.Current = Cursors.WaitCursor;

                Task.Run(() =>
                {
                    int exportedCount = 0;

                    try
                    {
                        foreach (var index in _tileList)
                        {
                            if (progressBarDialog.IsCancelled)
                            {
                                Invoke((Action)(() =>
                                {
                                    MessageBox.Show("Export was aborted.", "Cancel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }));
                                break;
                            }

                            if (!Art.IsValidLand(index))
                            {
                                continue;
                            }

                            var landTile = Art.GetLand(index);
                            if (landTile is null)
                            {
                                continue;
                            }

                            string fileName = Path.Combine(dialog.SelectedPath, $"0x{index:X4}.{fileExtension}");

                            using (Bitmap bit = new Bitmap(landTile))
                            {
                                bit.Save(fileName, imageFormat);
                            }

                            exportedCount++;
                            Invoke((Action)(() => progressBarDialog.OnChangeEvent()));
                        }

                        Invoke((Action)(() => progressBarDialog.MarkProcessFinished())); // Complete process
                    }
                    finally
                    {
                        Invoke((Action)(() =>
                        {
                            Cursor.Current = Cursors.Default;
                            progressBarDialog.Close();

                            if (!progressBarDialog.IsCancelled)
                            {
                                MessageBox.Show($"All land tiles were saved in {dialog.SelectedPath}\n" +
                                                $"Total number of exported land tiles: {exportedCount}",
                                    "Save completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }));
                    }
                });
            }
        }
        #endregion

        #region [ LandTilesTileView_DrawItem ]
        private void LandTilesTileView_DrawItem(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode() || e.Index < 0 || e.Index >= _tileList.Count)
            {
                return;
            }

            int id = _tileList[e.Index];
            Point itemPoint = new Point(e.Bounds.X + LandTilesTileView.TilePadding.Left, e.Bounds.Y + LandTilesTileView.TilePadding.Top);
            const int fixedTileSize = 44;
            Rectangle itemRec = new Rectangle(itemPoint, new Size(fixedTileSize, fixedTileSize));

            // Save/Restore instead of "new Region" per tile (the old code leaked a Region for every drawn tile)
            System.Drawing.Drawing2D.GraphicsState state = e.Graphics.Save();
            try
            {
                e.Graphics.SetClip(itemRec);
                if (id >= ExtendedFormat.LandBaseId)
                {
                    using var ext = new SolidBrush(Color.FromArgb(232, 240, 255)); // extended range: light blue background
                    e.Graphics.FillRectangle(ext, itemRec);
                }

                Bitmap bitmap = Art.GetLand(id, out bool patched);
                if (bitmap == null)
                {
                    // free slot: dashed diamond outline instead of a red square
                    Point[] diamond =
                    {
                        new Point(itemRec.X + 21, itemRec.Y + 1), new Point(itemRec.X + 42, itemRec.Y + 21),
                        new Point(itemRec.X + 21, itemRec.Y + 42), new Point(itemRec.X + 1, itemRec.Y + 21)
                    };
                    using var pen = new Pen(Color.IndianRed) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                    e.Graphics.DrawPolygon(pen, diamond);
                }
                else
                {
                    if (patched)
                    {
                        // different background for verdata patched tiles
                        e.Graphics.FillRectangle(Brushes.LightCoral, itemRec);
                    }

                    e.Graphics.DrawImage(bitmap, itemRec);
                }
            }
            finally
            {
                e.Graphics.Restore(state);
            }
        }
        #endregion

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
            int graphicIdMax = Art.GetMaxLandId();

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
        /// Check if it's valid index for land tile: 0x0000-0x3FFF plus the land extension (0x4000+).
        /// </summary>
        /// <param name="index">Starting Index</param>
        private static bool IsIndexValid(int index)
        {
            return Art.IsLegalLandId(index);
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

                // keep the list sorted and without duplicates (the old code overwrote the next entry)
                int pos = _tileList.BinarySearch(index);
                if (pos < 0)
                {
                    _tileList.Insert(~pos, index);
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
            // the old switch: land tiles <-> land tiles + free slots (more views in the "View" box)
            SetViewMode(_viewMode == 0 ? 1 : 0);
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

            int maximumIndex = Art.GetMaxLandId();

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

        #region [ CopyToolStripMenuItem_Click ] Copy clipboard - Event handler for the click event of the copyToolStripMenuItem control
        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
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

        #region [ ImportToolStripMenuItem_Click ] Clipboard LandTiles - The graphics are imported from the clipboard.
        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
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

                        // Replace the graphic in the Art object
                        Art.ReplaceLand(_selectedGraphicId, newBmp);

                        // Fire the LandTileChangeEvent to inform other parts of the application
                        ControlEvents.FireLandTileChangeEvent(this, _selectedGraphicId);

                        // Refresh the view
                        LandTilesTileView.Invalidate();
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
        #endregion

        #region LandTilesControl_KeyDown
        //Keydown Import Strg+V
        private void LandTilesControl_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Ctrl+V key combination has been pressed
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Calling the importToolStripMenuItem_Click method to import the graphic from the clipboard.
                ImportToolStripMenuItem_Click(sender, e);
            }
            // Checking if the Ctrl+X key combination has been pressed
            else if (e.Control && e.KeyCode == Keys.X)
            {
                // Calling the copyToolStripMenuItem_Click method to import the graphic from the clipboard.
                CopyToolStripMenuItem_Click(sender, e);
            }
            // Verify that the Ctrl+J key combination was pressed
            else if (e.Control && e.KeyCode == Keys.J)
            {
                // Call the gotoMarkToolStripMenuItem_Click method to jump to the marked position
                GotoMarkToolStripMenuItem_Click(sender, e);
            }
        }
        #endregion        

        #region [ ImportToTempToolStripMenuItem_Click] // and Save Image TempDir
        private void ImportToTempToolStripMenuItem_Click(object sender, EventArgs e)
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
                    int index = _selectedGraphicId; // Changed from _tileList.IndexOf(_selectedGraphicId);

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

                            // Update the view
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

        #region [ RotateBy90DegreesToolStripMenuItem_Click ] Bitmap graphic 90 degrees to the left
        private void RotateBy90DegreesToolStripMenuItem_Click(object sender, EventArgs e)
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

        #region [ ZoomImageToolStripMenuItem_Click ] Zoom Image
        private void ZoomImageToolStripMenuItem_Click(object sender, EventArgs e)
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

        #region [ MarkToolStripMenuItem_Click ] // Mark the selected position
        // Variable for storing the marked position
        private int markedPosition = -1;
        private void MarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LandTilesTileView.SelectedIndices.Count > 0)
            {
                markedPosition = LandTilesTileView.SelectedIndices[0];
            }
        }
        #endregion

        #region [ GotoMarkToolStripMenuItem ]
        private void GotoMarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (markedPosition >= 0 && markedPosition < LandTilesTileView.VirtualListSize)
            {
                // Set the selected position to the marked position
                LandTilesTileView.SelectedIndices.Clear();
                LandTilesTileView.SelectedIndices.Add(markedPosition);
            }
        }
        #endregion

        #region [ BackgroundToolStripMenuItem_Click ]
        private bool isBackgroundWhite = true;
        private void BackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isBackgroundWhite)
            {
                // Change the background color to black
                LandTilesTileView.BackColor = Color.FromArgb(0, 0, 0); // #000000
            }
            else
            {
                // Change the background color to white
                LandTilesTileView.BackColor = Color.FromArgb(255, 255, 255); // #FFFFFF
            }

            // Switch status for next time
            isBackgroundWhite = !isBackgroundWhite;
        }
        #endregion

        #region [ ColorsImagesToolStripMenuItem_Click ]
        private void ColorsImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Use the selected graphic ID directly
            int selectedId = _selectedGraphicId;

            // Create a new form
            Form colorForm = new Form
            {
                Text = $"Color Values for Graphic ID: {selectedId}",
                Width = 500,
                Height = 970
            };

            // Create a new SplitContainer
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            // Add the SplitContainer to the form
            colorForm.Controls.Add(splitContainer);

            // Create a new PixelBox
            PixelBox pixelBox = new PixelBox
            {
                Dock = DockStyle.Fill,
                Cursor = Cursors.Cross // Place the mouse cursor here
            };

            // Create a new RichTextBox
            RichTextBox colorBox = new RichTextBox
            {
                Dock = DockStyle.Fill
            };

            // Add an event handler for the MouseClick event
            colorBox.MouseClick += (sender, e) =>
            {
                // Check if a row is selected
                if (colorBox.GetLineFromCharIndex(colorBox.SelectionStart) == colorBox.GetLineFromCharIndex(colorBox.SelectionStart + colorBox.SelectionLength - 1))
                {
                    // Extract the color code from the selected row
                    string selectedLine = colorBox.Lines[colorBox.GetLineFromCharIndex(colorBox.SelectionStart)];
                    Match match = Regex.Match(selectedLine, @"Hex: (#?[0-9A-Fa-f]{6})");
                    if (match.Success)
                    {
                        // Copy the color code to the clipboard
                        Clipboard.SetText(match.Value);
                    }
                }
            };

            // Add the RichTextBox to the SplitContainer
            splitContainer.Panel2.Controls.Add(colorBox);

            // Add the PixelBox to the SplitContainer
            splitContainer.Panel1.Controls.Add(pixelBox);

            // Get the selected image
            Bitmap selectedImage = null;
            if (selectedId >= 0 && selectedId < _tileList.Count)
            {
                selectedImage = Art.GetLand(selectedId);
            }

            // Set the PixelBox's image to the selected image
            pixelBox.Image = selectedImage;

            pixelBox.PixelSelected += (x, y) =>
            {
                // Check that the x and y coordinates are within the boundaries of the image
                if (x >= 0 && x < selectedImage.Width && y >= 0 && y < selectedImage.Height)
                {
                    // Reset the text color for all text in the RichTextBox
                    colorBox.SelectAll();
                    colorBox.SelectionColor = Color.Black;
                    colorBox.DeselectAll();

                    // Find the line in the RichTextBox that corresponds to the selected pixel                   
                    int lineIndex = y * selectedImage.Width + x;

                    // Get the color of the current pixel                    
                    Color pixelColor = selectedImage.GetPixel(x, y);

                    // Convert the RGB color values into a hex color code
                    string hexColor = ColorTranslator.ToHtml(pixelColor);

                    // Extract the hex color code from the selected row
                    string selectedLine = colorBox.Lines[lineIndex];
                    Match match = Regex.Match(selectedLine, @"Hex: (#?[0-9A-Fa-f]{6})");
                    if (match.Success)
                    {
                        string selectedHexColor = match.Groups[1].Value;

                        // Compare the hex color codes                      
                        if (hexColor == selectedHexColor)
                        {
                            // The color codes match, highlight the line in the RichTextBox
                            colorBox.Select(colorBox.GetFirstCharIndexFromLine(lineIndex), colorBox.Lines[lineIndex].Length);

                            // Change the text color of the selected line
                            colorBox.SelectionColor = Color.LightGray;

                            // Scroll to the selected line in the RichTextBox
                            colorBox.ScrollToCaret();
                        }
                    }
                }
            };

            // Loop through every pixel in the image
            for (int y = 0; y < selectedImage.Height; y++)
            {
                for (int x = 0; x < selectedImage.Width; x++)
                {
                    // Get the color of the current pixel
                    Color pixelColor = selectedImage.GetPixel(x, y);

                    // Add the RGB color values and the hex color code to the RichTextBox
                    string hexColor = ColorTranslator.ToHtml(pixelColor);

                    // Fügen Sie die RGB-Farbwerte und den Hex-Farbcode zur RichTextBox hinzu
                    string colorText = $"Pixel ({x}, {y}): Color [R={pixelColor.R}, G={pixelColor.G}, B={pixelColor.B}], Hex: {hexColor}";
                    colorBox.AppendText(colorText);

                    // Add 5 spaces without color
                    colorBox.AppendText("     ");

                    // Add 5 spaces with the background color
                    colorBox.SelectionBackColor = pixelColor;
                    colorBox.AppendText("     ");
                    colorBox.SelectionBackColor = Color.White;

                    // Add a new line
                    colorBox.AppendText("\n");
                }
            }
            // View the form
            colorForm.Show();
        }

        #region class PixelBox
        public class PixelBox : Control
        {
            // Image is a public property of type Bitmap. It represents the image to be processed.
            public Bitmap Image { get; set; }
            // PixelSelected is an event that fires when a pixel is selected. It takes the x and y coordinates of the selected pixel as parameters.
            public event Action<int, int> PixelSelected;

            // OnPaint is a method that is called when the control is redrawn. It is overridden by the Control base class.
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (Image != null)
                {
                    //Iterate through each pixel in the image.
                    for (int x = 0; x < Image.Width; x++)
                    {
                        for (int y = 0; y < Image.Height; y++)
                        {
                            // Get the color of the current pixel.
                            Color pixelColor = Image.GetPixel(x, y);
                            //Create a new brush with the color of the current pixel.
                            using (Brush brush = new SolidBrush(pixelColor))
                            {
                                //Draw a rectangle in the control at the position of the current pixel, using the brush with the color of the pixel.
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

                // Calculate the selected pixel based on the mouse position and the actual pixel size
                int x = e.X / 10;
                int y = e.Y / 10;

                // Raise the PixelSelected event
                PixelSelected?.Invoke(x, y);
            }
            #endregion
        }
        #endregion
        #endregion

        #region [ ToolStripButtonTerrainLibrary_Click ]
        private void ToolStripButtonTerrainLibrary_Click(object sender, EventArgs e)
        {
            // opens the terrain library window (land tiles / textures share one library)
            UoFiddler.Controls.TerrainLibrary.Forms.TerrainWindows.ShowLandTiles(this, SelectedGraphicId);
        }
        #endregion
    }
}