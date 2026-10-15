// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.ArtLibrary.Forms;
using UoFiddler.Controls.ArtLibrary.Helpers;
using UoFiddler.Controls.ArtLibrary.Metadata;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.TerrainLibrary.Core;
using UoFiddler.Controls.TerrainLibrary.Helpers;
using UoFiddler.Controls.TerrainLibrary.Metadata;
using UoFiddler.Controls.UserControls.TileView;

namespace UoFiddler.Controls.TerrainLibrary.Forms
{
    /// <summary>
    /// The library view used by both windows (LandTileLibraryForm / TextureLibraryForm). <see cref="Layer"/>
    /// decides which graphics are shown in the grid; slot settings, names, colors and the partner preview
    /// are shared, because both windows work on the same open library (TerrainSession).
    /// </summary>
    public partial class TerrainLibraryPanel : UserControl
    {
        private static readonly Font _freeSlotFont = new Font(FontFamily.GenericSansSerif, 6.5f);

        private TerrainLayer _layer = TerrainLayer.LandTile;
        private List<int> _viewIds = new List<int>();
        private int _selectedId = -1;
        private Color _backgroundColor = Color.White;
        private bool _initialized;
        private bool _followingSelection;

        private bool _updatingLibraryCombo;
        private bool _updatingFilter;
        private bool _updatingDetail;
        private bool _infoDirty;
        private bool _tileDirty;
        private bool _colorDirty;
        private ushort? _editColor;

        // filter
        private string _searchText = string.Empty;
        private int? _searchId;
        private int _categoryFilter = AllCategories; // AllCategories, NoCategory or a category id
        private HashSet<int> _categoryFilterIds = new HashSet<int>();
        private string _tagFilter;
        private bool _favoritesOnly;
        private bool _withPartnerOnly;
        private bool _withSettingsOnly;

        private const int AllCategories = -1;
        private const int NoCategory = -2;

        public TerrainLibraryPanel()
        {
            InitializeComponent();
        }

        #region [ Public ]

        /// <summary>Layer shown in the grid (set by the window).</summary>
        [DefaultValue(TerrainLayer.LandTile)]
        public TerrainLayer Layer
        {
            get => _layer;
            set
            {
                _layer = value;
                if (_initialized)
                {
                    ApplyLayerTexts();
                    ReloadList();
                }
            }
        }

        public int SelectedId => _selectedId;

        /// <summary>Called by the window after it was created.</summary>
        public void Initialize()
        {
            if (_initialized || IsInDesigner)
            {
                return;
            }

            _initialized = true;
            pictureBoxPreview.AllowDrop = true;
            TerrainFlags.Fill(checkedListBoxFlags);

            TerrainSession.LibraryChanged += OnSessionLibraryChanged;
            TerrainSession.SlotChanged += OnSessionSlotChanged;
            TerrainSession.MetadataChanged += OnSessionMetadataChanged;
            TerrainSession.SaveStateChanged += OnSessionSaveStateChanged;
            TerrainSession.BatchCompleted += OnSessionBatchCompleted;
            TerrainSession.SlotSelected += OnSessionSlotSelected;

            ApplyLayerTexts();
            ApplyOptionChecks();
            ApplyTileViewOptions();
            FillLibraryCombo();
            FillFilterCombos();
            FillDetailCombos();
            ReloadList();

            if (TerrainSession.Library == null && !string.IsNullOrEmpty(Settings.LastLibrary)
                && File.Exists(TerrainLibraryFile.GetBasePath(Settings.LastLibrary) + TerrainLayer.LandTile.IdxExtension()))
            {
                OpenLibrary(Settings.LastLibrary, silent: true);
            }
            else if (LibraryTileView.VirtualListSize > 0)
            {
                RestoreSelection(0);
            }
        }

        /// <summary>Selects a slot (clears filters / shows free slots when needed).</summary>
        public void SelectSlot(int id)
        {
            if (Lib == null || !Lib.IsValidId(id))
            {
                return;
            }

            if (IsListMode && GetIndexOfId(id) < 0)
            {
                if (IsFilterActive)
                {
                    ClearFilters();
                }

                if (!ShowFreeSlots && !Store.IsOccupied(id))
                {
                    SetShowFreeSlots(true);
                }
            }

            int index = GetIndexOfId(id);
            if (index < 0)
            {
                return;
            }

            LibraryTileView.FocusIndex = index;
            SetSelectedId(id);
        }

        /// <summary>Selects a slot only when it is visible in the current view (does not change filters or free-slot mode).</summary>
        public void SelectSlotIfVisible(int id)
        {
            int index = GetIndexOfId(id);
            if (index >= 0)
            {
                LibraryTileView.FocusIndex = index;
                SetSelectedId(id);
            }
        }

        /// <summary>
        /// Asks what to do with unsaved changes before the library is closed. Returns false when cancelled.
        /// </summary>
        public bool ConfirmUnsavedChanges()
        {
            ApplyDetailIfDirty();
            if (!TerrainSession.HasUnsavedChanges)
            {
                return true;
            }

            if (Settings.AutoSave)
            {
                return SaveAll(false);
            }

            DialogResult answer = MessageBox.Show(FindForm(),
                $"Die Terrain-Bibliothek \"{Lib.Name}\" hat ungespeicherte Änderungen.\n\nJetzt speichern?",
                "Terrain-Bibliothek", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (answer == DialogResult.Cancel)
            {
                return false;
            }

            return answer != DialogResult.Yes || SaveAll(false);
        }

        /// <summary>Called when the window closes.</summary>
        public void Shutdown()
        {
            if (!_initialized)
            {
                return;
            }

            timerSave.Stop();
            TerrainSession.LibraryChanged -= OnSessionLibraryChanged;
            TerrainSession.SlotChanged -= OnSessionSlotChanged;
            TerrainSession.MetadataChanged -= OnSessionMetadataChanged;
            TerrainSession.SaveStateChanged -= OnSessionSaveStateChanged;
            TerrainSession.BatchCompleted -= OnSessionBatchCompleted;
            TerrainSession.SlotSelected -= OnSessionSlotSelected;

            if (!TerrainWindows.IsOtherOpen(_layer))
            {
                TerrainSession.CloseLibrary();
            }
        }

        public void UpdateDockButton()
        {
            toolStripButtonDock.Checked = TerrainWindows.Docked;
        }

        #endregion

        #region [ Shortcuts ]

        private static TerrainLibrarySettings Settings => TerrainSession.Settings;

        private static TerrainLibraryFile Lib => TerrainSession.Library;

        private static TerrainMetadata Meta => Lib?.Metadata;

        private TerrainLayerStorage Store => Lib?[_layer];

        private TerrainLayerStorage PartnerStore => Lib?[_layer.Other()];

        private bool IsInDesigner => LicenseManager.UsageMode == LicenseUsageMode.Designtime || FormsDesignerHelper.IsInDesignMode();

        private IWin32Window Owner => FindForm();

        private bool ShowFreeSlots
        {
            get => _layer == TerrainLayer.LandTile ? Settings.LandShowFreeSlots : Settings.TextureShowFreeSlots;
            set
            {
                if (_layer == TerrainLayer.LandTile)
                {
                    Settings.LandShowFreeSlots = value;
                }
                else
                {
                    Settings.TextureShowFreeSlots = value;
                }
            }
        }

        #endregion

        #region [ Layer texts / options ]

        private void ApplyLayerTexts()
        {
            string other = _layer.Other().DisplayName();
            toolStripButtonPartner.Text = $"{other} ▶";
            toolStripButtonPartner.ToolTipText = $"Fenster \"{other}\" öffnen (gleiche Slots)";
            labelPartner.Text = $"Partner ({_layer.Other().SingleName()}):";
            ctxRemove.Text = $"{_layer.SingleName()} entfernen";
            editLandFromTextureMenu.Text = ctxLandFromTexture.Text = "LandTile aus Textur des Slots erzeugen";
            LibraryTileView.TileSize = _layer == TerrainLayer.LandTile
                ? new Size(Math.Max(Options.ArtItemSizeWidth, 46), Math.Max(Options.ArtItemSizeHeight, 46))
                : new Size(68, 68);
        }

        private void ApplyOptionChecks()
        {
            toolStripButtonAutoSave.Checked = Settings.AutoSave;
            toolStripButtonShowFreeSlots.Checked = ShowFreeSlots;
            ctxShowFreeSlots.Checked = ShowFreeSlots;
            optAutoScaleMenu.Checked = Settings.AutoScale;
            optAutoTileDataMenu.Checked = Settings.AutoTileData;
            optFetchLinkedTextureMenu.Checked = Settings.FetchLinkedTexture;
            optAutoRadarColorMenu.Checked = Settings.AutoRadarColor;
            optWriteRadarColorMenu.Checked = Settings.WriteRadarColorToOriginal;
            optBuiltInPresetsMenu.Checked = Settings.UseBuiltInPresets;
            optLandIsoMenu.Checked = Settings.LandConversion == LandConversionMode.Isometric;
            optLandStretchMenu.Checked = Settings.LandConversion == LandConversionMode.RowStretch;
            optSyncSelectionMenu.Checked = Settings.SyncSelection;
            UpdateDockButton();
        }

        private void ApplyTileViewOptions()
        {
            LibraryTileView.TileBorderColor = Options.RemoveTileBorder ? Color.Transparent : Color.Gray;
            LibraryTileView.TileFocusColor = Options.TileFocusColor;
            LibraryTileView.TileHighlightColor = Options.TileSelectionColor;
        }

        #endregion

        #region [ Open / Create ]

        private void OpenLibrary(string path, bool silent = false)
        {
            if (!ConfirmUnsavedChanges())
            {
                FillLibraryCombo();
                return;
            }

            TerrainLibraryFile library;
            try
            {
                // the files are opened for writing, the current instance must be closed first
                if (Lib != null && string.Equals(TerrainLibraryFile.GetBasePath(path), Lib.BasePath, StringComparison.OrdinalIgnoreCase))
                {
                    TerrainSession.CloseLibrary();
                }

                library = TerrainLibraryFile.Open(path);
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    MessageBox.Show(Owner, $"Die Terrain-Bibliothek konnte nicht geöffnet werden:\n{ex.Message}",
                        "Terrain-Bibliothek", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (!File.Exists(TerrainLibraryFile.GetBasePath(path) + TerrainLayer.LandTile.IdxExtension()))
                {
                    Settings.RemoveRecent(path);
                    Settings.Save();
                }

                FillLibraryCombo();
                return;
            }

            TerrainSession.SetLibrary(library);
        }

        private void CreateLibrary()
        {
            if (!ConfirmUnsavedChanges())
            {
                return;
            }

            using (var dialog = new TerrainNewForm())
            {
                dialog.LibraryDirectory = Directory.Exists(Settings.LastDirectory) ? Settings.LastDirectory : Options.OutputPath ?? string.Empty;
                if (dialog.ShowDialog(Owner) != DialogResult.OK)
                {
                    return;
                }

                TerrainLibraryFile library;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    library = TerrainLibraryFile.Create(dialog.LibraryDirectory, dialog.LibraryName, dialog.Capacity);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Owner, $"Die Terrain-Bibliothek konnte nicht angelegt werden:\n{ex.Message}",
                        "Neue Terrain-Bibliothek", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

                TerrainSession.SetLibrary(library);
            }
        }

        private void FillLibraryCombo()
        {
            _updatingLibraryCombo = true;
            try
            {
                toolStripComboBoxLibrary.Items.Clear();
                foreach (string path in Settings.RecentLibraries)
                {
                    toolStripComboBoxLibrary.Items.Add(new RecentItem(path));
                }

                toolStripComboBoxLibrary.SelectedIndex = -1;
                if (Lib != null)
                {
                    for (int i = 0; i < toolStripComboBoxLibrary.Items.Count; ++i)
                    {
                        if (((RecentItem)toolStripComboBoxLibrary.Items[i]).Matches(Lib.MainPath))
                        {
                            toolStripComboBoxLibrary.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            finally
            {
                _updatingLibraryCombo = false;
            }
        }

        private sealed class RecentItem
        {
            public RecentItem(string path)
            {
                Path = path;
            }

            public string Path { get; }

            public bool Matches(string other) => string.Equals(
                TerrainLibraryFile.GetBasePath(Path), TerrainLibraryFile.GetBasePath(other), StringComparison.OrdinalIgnoreCase);

            public override string ToString()
            {
                string basePath = TerrainLibraryFile.GetBasePath(Path);
                return $"{System.IO.Path.GetFileName(basePath)}  ({System.IO.Path.GetDirectoryName(basePath)})";
            }
        }

        #endregion

        #region [ Session events ]

        private void OnSessionLibraryChanged(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            _infoDirty = _tileDirty = _colorDirty = false;
            FillLibraryCombo();
            ResetFilterState();
            FillFilterCombos();
            FillDetailCombos();
            ReloadList();
            if (LibraryTileView.VirtualListSize > 0)
            {
                LibraryTileView.FocusIndex = 0;
                SetSelectedId(GetIdAtIndex(0));
            }
            else
            {
                SetSelectedId(-1);
            }
        }

        private void OnSessionSlotChanged(object sender, TerrainSlotChangedEventArgs e)
        {
            if (IsDisposed || Lib == null)
            {
                return;
            }

            if (e.IsAll)
            {
                int keep = _selectedId;
                ReloadList();
                RestoreSelection(keep);
                return;
            }

            if (IsListMode)
            {
                UpdateViewMembership(e.Id);
            }

            LibraryTileView.Invalidate();
            UpdateStatus();
            if (e.Id == _selectedId)
            {
                UpdateDetail();
            }
        }

        private void OnSessionMetadataChanged(object sender, EventArgs e)
        {
            if (IsDisposed || ReferenceEquals(sender, this))
            {
                return;
            }

            FillFilterCombos();
            FillDetailCombos();
            if (IsFilterActive)
            {
                int keep = _selectedId;
                ReloadList();
                RestoreSelection(keep);
            }
            else
            {
                LibraryTileView.Invalidate();
                UpdateDetail();
            }
        }

        private void OnSessionSaveStateChanged(object sender, EventArgs e)
        {
            if (!IsDisposed)
            {
                UpdateSaveStatus();
            }
        }

        private void OnSessionBatchCompleted(object sender, int selectId)
        {
            if (IsDisposed)
            {
                return;
            }

            int keep = _selectedId;
            ReloadList();
            if (selectId >= 0 && ContainsFocus)
            {
                SelectSlot(selectId);
            }
            else
            {
                RestoreSelection(keep);
            }
        }

        private void OnSessionSlotSelected(object sender, int id)
        {
            if (IsDisposed || ReferenceEquals(sender, this) || !Settings.SyncSelection || id == _selectedId || Lib == null)
            {
                return;
            }

            _followingSelection = true;
            try
            {
                int index = GetIndexOfId(id);
                if (index >= 0)
                {
                    LibraryTileView.FocusIndex = index;
                    SetSelectedId(id);
                }
                else
                {
                    SetSelectedId(id); // not visible in the grid (filter), but show its details
                }
            }
            finally
            {
                _followingSelection = false;
            }
        }

        #endregion

        #region [ Save ]

        private bool SaveAll(bool showMessage)
        {
            if (Lib == null)
            {
                return true;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ApplyDetailIfDirty();
                TerrainSession.Save();
                if (showMessage)
                {
                    ShowStatusMessage($"Terrain-Bibliothek \"{Lib.Name}\" gespeichert ({TerrainSession.LastSaved:HH:mm:ss})");
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Owner, $"Speichern fehlgeschlagen:\n{ex.Message}", "Speichern", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void UpdateSaveStatus()
        {
            Form form = FindForm();
            string title = _layer == TerrainLayer.LandTile ? "LandTile-Bibliothek" : "Textur-Bibliothek";
            if (Lib == null)
            {
                statusLabelSave.Text = string.Empty;
                if (form != null)
                {
                    form.Text = title;
                }

                return;
            }

            bool unsaved = TerrainSession.HasUnsavedChanges || _infoDirty || _tileDirty || _colorDirty;
            if (unsaved)
            {
                int count = Lib.UnsavedCount;
                statusLabelSave.Text = count > 0 ? $"● Nicht gespeichert ({count:N0} Änderungen)" : "● Nicht gespeichert";
                statusLabelSave.ForeColor = Color.DarkOrange;
            }
            else
            {
                string mode = Settings.AutoSave ? "Automatisch gespeichert" : "Gespeichert";
                statusLabelSave.Text = TerrainSession.LastSaved == null ? $"✓ {mode}" : $"✓ {mode} {TerrainSession.LastSaved:HH:mm:ss}";
                statusLabelSave.ForeColor = Color.DarkGreen;
            }

            if (form != null)
            {
                form.Text = $"{title} - {Lib.Name}{(unsaved ? " *" : string.Empty)}";
            }
        }

        #endregion

        #region [ Filter ]

        private bool IsFilterActive =>
            !string.IsNullOrWhiteSpace(_searchText) || _categoryFilter != AllCategories || _tagFilter != null
            || _favoritesOnly || _withPartnerOnly || _withSettingsOnly;

        /// <summary>True when the grid shows <see cref="_viewIds"/> instead of all slots.</summary>
        private bool IsListMode => !ShowFreeSlots || IsFilterActive;

        private bool PassesFilter(int id)
        {
            if (!IsFilterActive)
            {
                return true;
            }

            TerrainSlotData slot = Meta?.GetSlot(id);
            ArtLibraryItemInfo info = Meta?.Catalog.Get(id);

            if (_favoritesOnly && (slot == null || !slot.Favorite))
            {
                return false;
            }

            if (_withPartnerOnly && !PartnerStore.IsOccupied(id))
            {
                return false;
            }

            if (_withSettingsOnly && (slot == null || !slot.HasSettings))
            {
                return false;
            }

            if (_categoryFilter == NoCategory && info?.CategoryId != null)
            {
                return false;
            }

            if (_categoryFilter >= 0 && (info?.CategoryId == null || !_categoryFilterIds.Contains(info.CategoryId.Value)))
            {
                return false;
            }

            if (_tagFilter != null && (info == null || !info.Tags.Any(t => string.Equals(t, _tagFilter, StringComparison.OrdinalIgnoreCase))))
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(_searchText) || (Meta?.Matches(id, _searchText, _searchId) ?? _searchId == id);
        }

        private void ResetFilterState()
        {
            _searchText = string.Empty;
            _searchId = null;
            _categoryFilter = AllCategories;
            _categoryFilterIds = new HashSet<int>();
            _tagFilter = null;
            _favoritesOnly = _withPartnerOnly = _withSettingsOnly = false;

            _updatingFilter = true;
            toolStripTextBoxSearch.Text = string.Empty;
            toolStripButtonFavorites.Checked = false;
            toolStripButtonWithPartner.Checked = false;
            toolStripButtonWithSettings.Checked = false;
            _updatingFilter = false;
        }

        private void ClearFilters()
        {
            int keep = _selectedId;
            ResetFilterState();
            FillFilterCombos();
            ReloadList();
            RestoreSelection(keep);
        }

        private sealed class ComboItem
        {
            public ComboItem(int? id, string text)
            {
                Id = id;
                Text = text;
            }

            public int? Id { get; }
            public string Text { get; }

            public override string ToString() => Text;
        }

        private void FillFilterCombos()
        {
            _updatingFilter = true;
            try
            {
                toolStripComboBoxCategory.Items.Clear();
                toolStripComboBoxCategory.Items.Add(new ComboItem(AllCategories, "(Alle Kategorien)"));
                toolStripComboBoxCategory.Items.Add(new ComboItem(NoCategory, "(Ohne Kategorie)"));
                int selectIndex = _categoryFilter == NoCategory ? 1 : 0;
                if (Meta != null)
                {
                    foreach (ArtLibraryCategory category in Meta.Catalog.GetCategoriesSorted())
                    {
                        toolStripComboBoxCategory.Items.Add(new ComboItem(category.Id, Meta.Catalog.GetCategoryPath(category.Id)));
                        if (category.Id == _categoryFilter)
                        {
                            selectIndex = toolStripComboBoxCategory.Items.Count - 1;
                        }
                    }
                }

                if (_categoryFilter >= 0 && selectIndex == 0)
                {
                    _categoryFilter = AllCategories;
                }

                toolStripComboBoxCategory.SelectedIndex = selectIndex;
                _categoryFilterIds = _categoryFilter >= 0 && Meta != null
                    ? Meta.Catalog.GetCategoryWithChildren(_categoryFilter)
                    : new HashSet<int>();

                toolStripComboBoxTag.Items.Clear();
                toolStripComboBoxTag.Items.Add("(Alle Tags)");
                int tagIndex = 0;
                if (Meta != null)
                {
                    foreach (string tag in Meta.Catalog.Tags)
                    {
                        toolStripComboBoxTag.Items.Add(tag);
                        if (_tagFilter != null && string.Equals(tag, _tagFilter, StringComparison.OrdinalIgnoreCase))
                        {
                            tagIndex = toolStripComboBoxTag.Items.Count - 1;
                        }
                    }
                }

                if (tagIndex == 0)
                {
                    _tagFilter = null;
                }

                toolStripComboBoxTag.SelectedIndex = tagIndex;
            }
            finally
            {
                _updatingFilter = false;
            }
        }

        private void ApplyFilterChange()
        {
            if (_updatingFilter)
            {
                return;
            }

            int keep = _selectedId;
            ReloadList();
            RestoreSelection(keep);
        }

        private void RunSearch()
        {
            _searchText = toolStripTextBoxSearch.Text.Trim();
            _searchId = ArtIdParser.TryParse(_searchText, out int id) ? id : (int?)null;
            ApplyFilterChange();
        }

        #endregion

        #region [ List / index mapping ]

        private void ReloadList()
        {
            if (!_initialized)
            {
                return;
            }

            if (Lib == null)
            {
                _viewIds = new List<int>();
                LibraryTileView.VirtualListSize = 0;
                LibraryTileView.Invalidate();
                UpdateAll();
                return;
            }

            if (IsListMode)
            {
                IEnumerable<int> ids = ShowFreeSlots ? Enumerable.Range(0, Lib.Capacity) : Store.GetOccupiedIds();
                _viewIds = IsFilterActive ? ids.Where(PassesFilter).ToList() : ids.ToList();
                LibraryTileView.VirtualListSize = _viewIds.Count;
            }
            else
            {
                LibraryTileView.VirtualListSize = Lib.Capacity;
            }

            LibraryTileView.Invalidate();
            UpdateAll();
        }

        private int GetIdAtIndex(int index)
        {
            if (Lib == null || index < 0)
            {
                return -1;
            }

            if (IsListMode)
            {
                return index < _viewIds.Count ? _viewIds[index] : -1;
            }

            return index < Lib.Capacity ? index : -1;
        }

        private int GetIndexOfId(int id)
        {
            if (Lib == null || !Lib.IsValidId(id))
            {
                return -1;
            }

            if (IsListMode)
            {
                int index = _viewIds.BinarySearch(id);
                return index >= 0 ? index : -1;
            }

            return id;
        }

        private void SetShowFreeSlots(bool value)
        {
            if (ShowFreeSlots == value && toolStripButtonShowFreeSlots.Checked == value)
            {
                return;
            }

            int keep = _selectedId;
            ShowFreeSlots = value;
            Settings.Save();
            toolStripButtonShowFreeSlots.Checked = value;
            ctxShowFreeSlots.Checked = value;
            ReloadList();
            RestoreSelection(keep);
        }

        private void RestoreSelection(int id)
        {
            if (Lib == null || LibraryTileView.VirtualListSize == 0)
            {
                SetSelectedId(-1);
                return;
            }

            int index = GetIndexOfId(id);
            if (index < 0 && IsListMode)
            {
                index = ~_viewIds.BinarySearch(Math.Max(id, 0));
                if (index >= _viewIds.Count)
                {
                    index = _viewIds.Count - 1;
                }
            }

            if (index < 0)
            {
                index = 0;
            }

            LibraryTileView.FocusIndex = index;
            SetSelectedId(GetIdAtIndex(index));
        }

        private void UpdateViewMembership(int id)
        {
            bool shouldShow = (ShowFreeSlots || Store.IsOccupied(id)) && PassesFilter(id);
            int index = _viewIds.BinarySearch(id);
            if (shouldShow == index >= 0)
            {
                return;
            }

            if (shouldShow)
            {
                _viewIds.Insert(~index, id);
            }
            else
            {
                _viewIds.RemoveAt(index);
            }

            int keep = _selectedId;
            LibraryTileView.VirtualListSize = _viewIds.Count;
            RestoreSelection(keep);
        }

        private void BeginBatch()
        {
            TerrainSession.BeginBatch();
            Cursor.Current = Cursors.WaitCursor;
        }

        private void EndBatch(int selectId = -1)
        {
            Cursor.Current = Cursors.Default;
            TerrainSession.EndBatch(selectId);
            if (selectId >= 0)
            {
                SelectSlot(selectId);
            }
        }

        #endregion

        #region [ Selection / detail / status ]

        private void SetSelectedId(int id)
        {
            if (id != _selectedId)
            {
                ApplyDetailIfDirty();
            }

            _selectedId = id;
            UpdateDetail();
            UpdateStatus();

            if (!_followingSelection && id >= 0)
            {
                TerrainSession.NotifySlotSelected(this, id);
            }
        }

        private void UpdateAll()
        {
            UpdateDetail();
            UpdateStatus();
            UpdateUiState();
        }

        private void UpdateUiState()
        {
            bool open = Lib != null;
            foreach (ToolStripItem item in new ToolStripItem[]
                     {
                         toolStripButtonReload, toolStripButtonSave, toolStripTextBoxId, toolStripButtonGoTo,
                         toolStripButtonShowFreeSlots, toolStripButtonCopy, toolStripButtonPaste,
                         toolStripDropDownButtonOriginal, toolStripDropDownButtonEdit, libraryPropertiesMenu,
                         manageCategoriesMenu, managePresetsMenu, growMenu, compactMenu, openFolderMenu
                     })
            {
                item.Enabled = open;
            }

            toolStripFilter.Enabled = open;
            tabControlDetail.Enabled = open;
            LibraryTileView.Enabled = open;
            UpdateSaveStatus();
        }

        private void FillDetailCombos()
        {
            bool updating = _updatingDetail;
            _updatingDetail = true;
            try
            {
                comboBoxCategory.Items.Clear();
                comboBoxCategory.Items.Add(new ComboItem(null, "(keine)"));
                if (Meta != null)
                {
                    foreach (ArtLibraryCategory category in Meta.Catalog.GetCategoriesSorted())
                    {
                        comboBoxCategory.Items.Add(new ComboItem(category.Id, Meta.Catalog.GetCategoryPath(category.Id)));
                    }
                }

                comboBoxCategory.SelectedIndex = 0;

                comboBoxPreset.Items.Clear();
                if (Meta != null)
                {
                    foreach (TerrainPreset preset in Meta.GetAllPresets(Settings.UseBuiltInPresets))
                    {
                        comboBoxPreset.Items.Add(preset);
                    }
                }

                if (comboBoxPreset.Items.Count > 0)
                {
                    comboBoxPreset.SelectedIndex = 0;
                }
            }
            finally
            {
                _updatingDetail = updating;
            }
        }

        private void UpdateDetail()
        {
            _updatingDetail = true;
            try
            {
                if (Lib == null || _selectedId < 0)
                {
                    pictureBoxPreview.Image = null;
                    pictureBoxPartner.Image = null;
                    richTextBoxDetail.Clear();
                    textBoxName.Text = textBoxDescription.Text = textBoxTags.Text = string.Empty;
                    textBoxTileName.Text = textBoxTextureId.Text = textBoxColor.Text = string.Empty;
                    TerrainFlags.Set(checkedListBoxFlags, 0);
                    _editColor = null;
                    ShowColor(null);
                    return;
                }

                Bitmap bitmap = Store.GetBitmap(_selectedId);
                Bitmap partner = PartnerStore.GetBitmap(_selectedId);
                pictureBoxPreview.BackColor = _backgroundColor;
                pictureBoxPreview.Image = bitmap;
                pictureBoxPartner.Image = partner;

                ArtLibraryItemInfo info = Meta.Catalog.Get(_selectedId);
                TerrainSlotData slot = Meta.GetSlot(_selectedId);

                textBoxName.Text = info?.Name ?? string.Empty;
                textBoxDescription.Text = info?.Description ?? string.Empty;
                textBoxTags.Text = info == null ? string.Empty : string.Join(", ", info.Tags);
                SelectComboItem(comboBoxCategory, info?.CategoryId);
                checkBoxFavorite.Checked = slot?.Favorite ?? false;

                textBoxTileName.Text = slot?.TileName ?? string.Empty;
                textBoxTextureId.Text = slot?.TextureId == null ? string.Empty : $"0x{slot.TextureId.Value:X4}";
                TerrainFlags.Set(checkedListBoxFlags, slot?.TileFlags ?? 0);

                _editColor = slot?.RadarColor;
                ShowColor(_editColor);
                textBoxColor.Text = _editColor == null ? string.Empty : TerrainColor.ToHex(_editColor.Value);

                var text = new StringBuilder();
                text.AppendLine($"Slot:        {_selectedId}  ({ArtIdParser.ToHex(_selectedId)})");
                text.AppendLine($"{_layer.SingleName(),-12} {(bitmap == null ? "frei" : $"{bitmap.Width}x{bitmap.Height}, {Store.GetEntry(_selectedId).Length:N0} Bytes")}");
                text.AppendLine($"{_layer.Other().SingleName(),-12} {(partner == null ? "frei" : $"{partner.Width}x{partner.Height}")}");
                if (slot?.SourceLandId != null)
                {
                    text.AppendLine($"Herkunft:    LandTile {ArtIdParser.ToHex(slot.SourceLandId.Value)}");
                }

                if (slot?.SourceTextureId != null)
                {
                    text.AppendLine($"Herkunft:    Textur {ArtIdParser.ToHex(slot.SourceTextureId.Value)}");
                }

                if (slot?.HasTileData == true)
                {
                    text.AppendLine($"tiledata:    \"{slot.TileName}\" Flags {(TileFlag)(slot.TileFlags ?? 0)}");
                }

                if (slot?.RadarColor != null)
                {
                    text.AppendLine($"RadarColor:  {TerrainColor.ToHex(slot.RadarColor.Value)}  {TerrainColor.ToHtml(slot.RadarColor.Value)}");
                }

                if (!string.IsNullOrEmpty(slot?.PresetName))
                {
                    text.AppendLine($"Vorlage:     {slot.PresetName}");
                }

                richTextBoxDetail.Text = text.ToString();
            }
            finally
            {
                _updatingDetail = false;
                _infoDirty = _tileDirty = _colorDirty = false;
            }
        }

        private void ShowColor(ushort? value)
        {
            Color color = value == null ? SystemColors.Control : TerrainColor.ToColor(value.Value);
            panelColorSwatch.BackColor = color;
            panelColorBig.BackColor = color;
            labelColorSwatch.Text = value == null ? "keine Farbe" : $"{TerrainColor.ToHex(value.Value)}\n{TerrainColor.ToHtml(value.Value)}";
            labelColorValue.Text = value == null
                ? "Keine RadarColor im Slot gespeichert."
                : $"Wert {TerrainColor.ToHex(value.Value)} ({value.Value})   Web {TerrainColor.ToHtml(value.Value)}";
        }

        private static void SelectComboItem(ComboBox combo, int? id)
        {
            for (int i = 0; i < combo.Items.Count; ++i)
            {
                if (combo.Items[i] is ComboItem item && item.Id == id)
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }

            if (combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private void UpdateStatus()
        {
            if (Lib == null)
            {
                statusLabelLibrary.Text = "Keine Terrain-Bibliothek geöffnet";
                statusLabelCount.Text = statusLabelView.Text = statusLabelSelected.Text = string.Empty;
                return;
            }

            statusLabelLibrary.Text = Lib.Name;
            statusLabelCount.Text = $"{Lib.Land.OccupiedCount:N0} LandTiles · {Lib.Textures.OccupiedCount:N0} Texturen / {Lib.Capacity:N0} Slots";
            statusLabelView.Text = IsFilterActive ? $"Filter: {_viewIds.Count:N0} Treffer" : string.Empty;
            statusLabelSelected.Text = _selectedId >= 0 ? $"Slot: {ArtIdParser.ToDisplay(_selectedId)}" : string.Empty;
        }

        private void ShowStatusMessage(string message)
        {
            statusLabelSelected.Text = message;
        }

        #endregion

        #region [ Detail editing ]

        private void ApplyDetailIfDirty()
        {
            if (_infoDirty)
            {
                ApplyInfo();
            }

            if (_tileDirty)
            {
                ApplyTileData();
            }

            if (_colorDirty)
            {
                ApplyColor();
            }
        }

        /// <summary>Name, category, tags, description and favorite of the slot.</summary>
        private void ApplyInfo()
        {
            _infoDirty = false;
            if (Meta == null || _selectedId < 0)
            {
                return;
            }

            ArtLibraryItemInfo info = Meta.Catalog.Get(_selectedId)?.Clone() ?? new ArtLibraryItemInfo();
            info.Name = textBoxName.Text.Trim();
            info.Description = textBoxDescription.Text.Trim();
            info.CategoryId = (comboBoxCategory.SelectedItem as ComboItem)?.Id;
            info.Tags = ArtLibraryMetadata.ParseTags(textBoxTags.Text);
            int tagCount = Meta.Catalog.Tags.Count;
            Meta.Catalog.Set(_selectedId, info);

            TerrainSlotData slot = Meta.GetOrCreateSlot(_selectedId);
            slot.Favorite = checkBoxFavorite.Checked;

            if (Meta.Catalog.Tags.Count != tagCount)
            {
                FillFilterCombos();
            }

            AfterMetadataChanged();
        }

        /// <summary>tiledata values (name, texture id, flags) of the slot - only in the library.</summary>
        private void ApplyTileData()
        {
            _tileDirty = false;
            if (Meta == null || _selectedId < 0)
            {
                return;
            }

            TerrainSlotData slot = Meta.GetOrCreateSlot(_selectedId);
            slot.TileName = string.IsNullOrWhiteSpace(textBoxTileName.Text) ? null : textBoxTileName.Text.Trim();
            slot.TextureId = ArtIdParser.TryParse(textBoxTextureId.Text, out int texId) && texId <= ushort.MaxValue ? (ushort)texId : (ushort?)null;
            ulong flags = TerrainFlags.Get(checkedListBoxFlags);
            slot.TileFlags = flags == 0 && slot.TileName == null && slot.TextureId == null ? (ulong?)null : flags;
            AfterMetadataChanged();
        }

        private void ApplyColor()
        {
            _colorDirty = false;
            if (Meta == null || _selectedId < 0)
            {
                return;
            }

            Meta.GetOrCreateSlot(_selectedId).RadarColor = _editColor;
            AfterMetadataChanged();
        }

        private void AfterMetadataChanged()
        {
            TerrainSession.NotifyMetadataChanged(this);
            if (IsListMode && IsFilterActive && _selectedId >= 0)
            {
                UpdateViewMembership(_selectedId);
            }

            LibraryTileView.Invalidate();
            UpdateSaveStatus();
        }

        private void SetEditColor(ushort? color)
        {
            _editColor = color;
            ShowColor(color);
            textBoxColor.Text = color == null ? string.Empty : TerrainColor.ToHex(color.Value);
            ApplyColor();
            UpdateDetail();
        }

        #endregion

        #region [ TileView ]

        private void LibraryTileView_DrawItem(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            if (Lib == null)
            {
                return;
            }

            int id = GetIdAtIndex(e.Index);
            if (id < 0)
            {
                return;
            }

            var point = new Point(e.Bounds.X + LibraryTileView.TilePadding.Left, e.Bounds.Y + LibraryTileView.TilePadding.Top);
            var rect = new Rectangle(point, LibraryTileView.TileSize);
            Region previousClip = e.Graphics.Clip;
            e.Graphics.Clip = new Region(rect);

            if (!LibraryTileView.SelectedIndices.Contains(e.Index))
            {
                e.Graphics.Clear(_backgroundColor);
            }

            Bitmap bitmap = Store.GetBitmap(id);
            if (bitmap == null)
            {
                // free slot like the original tabs ("Show Free Slots"): red square with the hex id
                var red = new Rectangle(rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 10);
                e.Graphics.FillRectangle(Brushes.Red, red);
                e.Graphics.DrawString(ArtIdParser.ToHex(id).Substring(2), _freeSlotFont, Brushes.White, red.X + 1, red.Y + 1);
            }
            else
            {
                Size size = LibraryTileView.TileSize;
                float scale = Math.Min(1f, Math.Min((float)size.Width / bitmap.Width, (float)size.Height / bitmap.Height));
                var target = new Rectangle(point.X, point.Y, (int)(bitmap.Width * scale), (int)(bitmap.Height * scale));
                e.Graphics.DrawImage(bitmap, target);
            }

            // small marks: partner graphic present (blue) / settings present (green)
            if (PartnerStore.IsOccupied(id))
            {
                e.Graphics.FillRectangle(Brushes.RoyalBlue, rect.Right - 6, rect.Top + 1, 5, 5);
            }

            TerrainSlotData slot = Meta.GetSlot(id);
            if (slot?.RadarColor != null)
            {
                using (var brush = new SolidBrush(TerrainColor.ToColor(slot.RadarColor.Value)))
                {
                    e.Graphics.FillRectangle(brush, rect.Right - 6, rect.Bottom - 6, 5, 5);
                }
            }
            else if (slot?.HasTileData == true)
            {
                e.Graphics.FillRectangle(Brushes.LimeGreen, rect.Right - 6, rect.Bottom - 6, 5, 5);
            }

            e.Graphics.Clip = previousClip;
        }

        private void LibraryTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                SetSelectedId(GetIdAtIndex(e.ItemIndex));
            }
        }

        private void LibraryTileView_FocusSelectionChanged(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.IsFocused && e.FocusedItemIndex >= 0)
            {
                SetSelectedId(GetIdAtIndex(e.FocusedItemIndex));
            }
        }

        private void LibraryTileView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedGraphics();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedToClipboard();
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                PasteFromClipboard(false);
            }
            else
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.S:
                    SaveAll(true);
                    return true;
                case Keys.F1:
                    ShowHelp();
                    return true;
                case Keys.Control | Keys.G:
                    toolStripTextBoxId.Focus();
                    toolStripTextBoxId.SelectAll();
                    return true;
                case Keys.Control | Keys.F:
                    toolStripTextBoxSearch.Focus();
                    toolStripTextBoxSearch.SelectAll();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private List<int> GetSelectedIds()
        {
            List<int> ids = LibraryTileView.SelectedIndices.Select(GetIdAtIndex).Where(id => id >= 0).Distinct().OrderBy(id => id).ToList();
            if (ids.Count == 0 && _selectedId >= 0)
            {
                ids.Add(_selectedId);
            }

            return ids;
        }

        private List<int> GetSelectedOccupiedIds()
        {
            return Lib == null ? new List<int>() : GetSelectedIds().Where(Store.IsOccupied).ToList();
        }

        #endregion

        #region [ Clipboard / import / export ]

        private void CopySelectedToClipboard()
        {
            List<int> ids = GetSelectedOccupiedIds();
            if (ids.Count == 0)
            {
                return;
            }

            var items = ids.Select(id => new TerrainClipboardItem
            {
                SourceId = id,
                Data = Store.GetRaw(id),
                Name = Meta.Catalog.GetName(id)
            }).Where(i => i.Data != null).ToList();

            try
            {
                TerrainClipboard.Copy(_layer, items);
                ShowStatusMessage(items.Count == 1
                    ? $"{_layer.SingleName()} {ArtIdParser.ToHex(items[0].SourceId)} kopiert"
                    : $"{items.Count} {_layer.DisplayName()} kopiert");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Owner, $"Zwischenablage nicht verfügbar:\n{ex.Message}", "Kopieren", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PasteFromClipboard(bool keepIds)
        {
            if (Lib == null)
            {
                return;
            }

            List<TerrainClipboardItem> items = TerrainClipboard.GetItems(_layer);
            if (items.Count > 0)
            {
                PlaceRaw(items, keepIds);
                return;
            }

            List<string> files = ArtLibraryClipboard.GetImageFiles();
            if (files.Count > 0)
            {
                ImportFiles(files);
                return;
            }

            using (Bitmap image = TerrainClipboard.GetImage())
            {
                if (image == null)
                {
                    MessageBox.Show(Owner, "Die Zwischenablage enthält keine Grafik.", "Einfügen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                PlaceImage(image, string.Empty);
            }
        }

        /// <summary>Puts an external image on the selected slot (scaled if allowed), asks before overwriting.</summary>
        private void PlaceImage(Image image, string name)
        {
            int target = _selectedId >= 0 ? _selectedId : Store.FindNextFree(0);
            if (target < 0)
            {
                MessageBox.Show(Owner, "Die Bibliothek ist voll. Bitte erweitern.", "Einfügen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (Store.IsOccupied(target) && !ConfirmOverwrite(target))
            {
                return;
            }

            try
            {
                using (Bitmap prepared = TerrainImageTools.PrepareForLayer(image, _layer, Settings.AutoScale, out bool scaled))
                {
                    Store.SetBitmap(target, prepared);
                    StoreName(target, name);
                    SelectSlot(target);
                    ShowStatusMessage(scaled
                        ? $"{_layer.SingleName()} auf {ArtIdParser.ToHex(target)} gespeichert (von {image.Width}x{image.Height} skaliert)"
                        : $"{_layer.SingleName()} auf {ArtIdParser.ToHex(target)} gespeichert");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Owner, ex.Message, "Einfügen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ConfirmOverwrite(int id)
        {
            return MessageBox.Show(Owner, $"{_layer.SingleName()} {ArtIdParser.ToDisplay(id)} ist bereits belegt. Ersetzen?",
                "Ersetzen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private void PlaceRaw(List<TerrainClipboardItem> items, bool keepIds)
        {
            if (items.Count == 1 && !keepIds)
            {
                int target = _selectedId >= 0 ? _selectedId : Store.FindNextFree(0);
                if (target < 0 || (Store.IsOccupied(target) && !ConfirmOverwrite(target)))
                {
                    return;
                }

                Store.SetRaw(target, items[0].Data);
                StoreName(target, items[0].Name);
                SelectSlot(target);
                return;
            }

            bool overwrite = false;
            if (keepIds)
            {
                int occupied = items.Count(i => Lib.IsValidId(i.SourceId) && Store.IsOccupied(i.SourceId));
                if (occupied > 0)
                {
                    DialogResult answer = MessageBox.Show(Owner,
                        $"{occupied} der Ziel-Slots sind bereits belegt.\n\nJa = ersetzen\nNein = diese überspringen",
                        "Einfügen", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (answer == DialogResult.Cancel)
                    {
                        return;
                    }

                    overwrite = answer == DialogResult.Yes;
                }
            }

            int[] targets = ArtLibraryTransfer.AssignTargetIds(items.Select(i => i.SourceId).ToList(),
                _selectedId >= 0 ? _selectedId : 0, keepIds, overwrite, Store.IsOccupied, Lib.MaxId);

            int done = 0, first = -1;
            BeginBatch();
            try
            {
                for (int i = 0; i < items.Count; ++i)
                {
                    if (targets[i] < 0)
                    {
                        continue;
                    }

                    Store.SetRaw(targets[i], items[i].Data);
                    StoreName(targets[i], items[i].Name);
                    first = first < 0 ? targets[i] : Math.Min(first, targets[i]);
                    ++done;
                }
            }
            finally
            {
                EndBatch(first);
            }

            ShowStatusMessage($"{done} von {items.Count} eingefügt" + (done < items.Count ? " (Rest übersprungen)" : string.Empty));
        }

        private void StoreName(int id, string name)
        {
            if (Meta == null || string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            ArtLibraryItemInfo info = Meta.Catalog.GetOrCreate(id);
            if (string.IsNullOrWhiteSpace(info.Name))
            {
                info.Name = name.Trim();
                Meta.Catalog.MarkDirty();
            }
        }

        private void ImportFilesWithDialog()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = $"{_layer.DisplayName()} importieren";
                dialog.Filter = "Bilder (*.bmp;*.png;*.tif;*.tiff;*.gif;*.jpg)|*.bmp;*.png;*.tif;*.tiff;*.gif;*.jpg;*.jpeg|Alle Dateien (*.*)|*.*";
                dialog.Multiselect = true;
                if (dialog.ShowDialog(Owner) == DialogResult.OK)
                {
                    ImportFiles(dialog.FileNames.ToList());
                }
            }
        }

        private void ImportFolderWithDialog()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Ordner mit Bildern wählen";
                dialog.UseDescriptionForTitle = true;
                if (dialog.ShowDialog(Owner) != DialogResult.OK)
                {
                    return;
                }

                List<string> files = Directory.GetFiles(dialog.SelectedPath).Where(ArtLibraryImageConverter.IsSupportedFile).ToList();
                if (files.Count == 0)
                {
                    MessageBox.Show(Owner, "Im Ordner wurden keine Bilder gefunden.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ImportFiles(files);
            }
        }

        private void ImportFiles(List<string> files)
        {
            if (Lib == null || files.Count == 0)
            {
                return;
            }

            files = files.OrderBy(f => ArtLibraryTransfer.ParseIdFromFileName(f) < 0 ? int.MaxValue : ArtLibraryTransfer.ParseIdFromFileName(f))
                .ThenBy(f => f, StringComparer.CurrentCultureIgnoreCase).ToList();

            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = $"{_layer.DisplayName()} importieren";
                dialog.InfoText = (files.Count == 1 ? $"Datei: {Path.GetFileName(files[0])}" : $"{files.Count} Dateien")
                                  + $"\nAuto-Skalieren ist {(Settings.AutoScale ? "an" : "aus")} (Menü Optionen).";
                dialog.ShowSource = false;
                dialog.ShowTransparencyOptions = false;
                dialog.KeepIdsText = $"ID aus dem Dateinamen verwenden (z. B. \"{_layer.ExportPrefix()} 0x0003.bmp\")";
                dialog.MaxTargetId = Lib.MaxId;
                dialog.TargetId = _selectedId >= 0 ? _selectedId : Math.Max(0, Store.FindNextFree(0));
                dialog.KeepIds = files.Count > 1 && files.All(f => ArtLibraryTransfer.ParseIdFromFileName(f) >= 0);
                dialog.Overwrite = files.Count == 1;
                if (dialog.ShowDialog(Owner) != DialogResult.OK)
                {
                    return;
                }

                List<int> sourceIds = files.Select(ArtLibraryTransfer.ParseIdFromFileName).ToList();
                int[] targets = ArtLibraryTransfer.AssignTargetIds(sourceIds, dialog.TargetId, dialog.KeepIds, dialog.Overwrite, Store.IsOccupied, Lib.MaxId);

                var errors = new List<string>();
                int done = 0, first = -1, scaledCount = 0;
                BeginBatch();
                try
                {
                    for (int i = 0; i < files.Count; ++i)
                    {
                        if (targets[i] < 0)
                        {
                            continue;
                        }

                        try
                        {
                            using (Bitmap loaded = ArtLibraryImageConverter.LoadFile(files[i]))
                            using (Bitmap prepared = TerrainImageTools.PrepareForLayer(loaded, _layer, Settings.AutoScale, out bool scaled))
                            {
                                Store.SetBitmap(targets[i], prepared);
                                scaledCount += scaled ? 1 : 0;
                            }

                            StoreName(targets[i], sourceIds[i] < 0 ? Path.GetFileNameWithoutExtension(files[i]) : string.Empty);
                            first = first < 0 ? targets[i] : Math.Min(first, targets[i]);
                            ++done;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{Path.GetFileName(files[i])}: {ex.Message.Replace("\n", " ")}");
                        }
                    }
                }
                finally
                {
                    EndBatch(first);
                }

                ReportResult("Import", done, files.Count, errors, scaledCount > 0 ? $"{scaledCount} Bild(er) wurden skaliert." : null);
            }
        }

        private void ExportGraphics(ImageFormat format, string extension, bool all)
        {
            if (Lib == null)
            {
                return;
            }

            List<int> ids = all ? Store.GetOccupiedIds() : GetSelectedOccupiedIds();
            if (ids.Count == 0)
            {
                MessageBox.Show(Owner, "Keine Grafik ausgewählt.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = $"Zielordner für {ids.Count} {_layer.DisplayName()}";
                dialog.UseDescriptionForTitle = true;
                if (Directory.Exists(Options.OutputPath))
                {
                    dialog.SelectedPath = Options.OutputPath;
                }

                if (dialog.ShowDialog(Owner) != DialogResult.OK)
                {
                    return;
                }

                var errors = new List<string>();
                int done = 0;
                Cursor.Current = Cursors.WaitCursor;
                foreach (int id in ids)
                {
                    try
                    {
                        Bitmap bitmap = Store.GetBitmap(id);
                        if (bitmap == null)
                        {
                            continue;
                        }

                        using (Bitmap export = ArtLibraryImageConverter.ToExportBitmap(bitmap))
                        {
                            export.Save(Path.Combine(dialog.SelectedPath, TerrainTransfer.GetExportFileName(_layer, id, extension)), format);
                        }

                        ++done;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{ArtIdParser.ToHex(id)}: {ex.Message}");
                    }
                }

                Cursor.Current = Cursors.Default;
                ReportResult("Export", done, ids.Count, errors, $"Ordner: {dialog.SelectedPath}");
            }
        }

        private void ReportResult(string title, int done, int total, List<string> errors, string extra = null)
        {
            var text = new StringBuilder();
            text.AppendLine($"{done} von {total} verarbeitet.");
            if (done < total && errors.Count == 0)
            {
                text.AppendLine("Übersprungen wurden belegte, leere oder ungültige IDs.");
            }

            if (!string.IsNullOrEmpty(extra))
            {
                text.AppendLine(extra);
            }

            if (errors.Count > 0)
            {
                text.AppendLine();
                text.AppendLine("Fehler:");
                foreach (string error in errors.Take(15))
                {
                    text.AppendLine(error);
                }

                if (errors.Count > 15)
                {
                    text.AppendLine($"... und {errors.Count - 15} weitere");
                }
            }

            MessageBox.Show(Owner, text.ToString(), title, MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        #endregion

        #region [ Original files ]

        private bool EnsureOriginal(TerrainLayer layer)
        {
            if (TerrainTransfer.IsAvailable(layer))
            {
                return true;
            }

            MessageBox.Show(Owner, layer == TerrainLayer.LandTile
                    ? "Es ist keine art.mul geladen. Bitte zuerst den Land-Tiles-Tab öffnen."
                    : "Es ist keine texmaps.mul geladen. Bitte zuerst den Textures-Tab öffnen.",
                "Original", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        /// <summary>
        /// Asks once what to do with target slots that already have settings. Returns null when cancelled,
        /// true = overwrite, false = keep the existing settings.
        /// </summary>
        private bool? AskSettingsOverwrite(IEnumerable<int> targetSlots, string what)
        {
            int count = targetSlots.Count(id => Meta.GetSlot(id)?.HasSettings == true);
            if (count == 0)
            {
                return true;
            }

            DialogResult answer = MessageBox.Show(Owner,
                $"{count} Ziel-Slot(s) haben bereits eigene Einstellungen (tiledata-Werte / Farbe).\n\n" +
                $"Ja = mit {what} überschreiben\nNein = vorhandene Einstellungen behalten\n\n" +
                "Die Werte kannst du danach im Reiter \"tiledata\" bzw. \"Farbe\" von Hand anpassen.",
                "Einstellungen vorhanden", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (answer == DialogResult.Cancel)
            {
                return null;
            }

            return answer == DialogResult.Yes;
        }

        /// <summary>Takes graphics (and for land tiles optionally tiledata, color and the linked texture) from the original files.</summary>
        private void FetchFromOriginal(List<int> presetIds)
        {
            if (Lib == null || !EnsureOriginal(_layer))
            {
                return;
            }

            int maxOriginal = TerrainTransfer.OriginalMaxId(_layer);
            bool land = _layer == TerrainLayer.LandTile;

            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = $"{_layer.DisplayName()} aus dem Original holen";
                var info = new StringBuilder($"{_layer.DisplayName()} aus der Originaldatei in die Bibliothek kopieren.\nQuell-IDs: z. B. 0x0003-0x0010, 0x00A8");
                if (land)
                {
                    info.Append("\n\nMit übernehmen (Menü Optionen): ");
                    info.Append($"tiledata-Werte {(Settings.AutoTileData ? "✓" : "✗")}, ");
                    info.Append($"RadarColor {(Settings.AutoRadarColor ? "✓" : "✗")}, ");
                    info.Append($"zugehörige Textur {(Settings.FetchLinkedTexture ? "✓" : "✗")}");
                }

                dialog.InfoText = info.ToString();
                dialog.SourceLabel = "Original-IDs:";
                dialog.SourceText = presetIds == null || presetIds.Count == 0 ? string.Empty : string.Join(", ", presetIds.Select(ArtIdParser.ToHex));
                dialog.MaxSourceId = maxOriginal;
                dialog.ShowTransparencyOptions = false;
                dialog.KeepIdsText = "Gleiche IDs wie im Original (Slot = Original-ID)";
                dialog.KeepIds = true;
                dialog.MaxTargetId = Lib.MaxId;
                dialog.TargetId = _selectedId >= 0 ? _selectedId : 0;
                if (dialog.ShowDialog(Owner) != DialogResult.OK)
                {
                    return;
                }

                List<int> sources = dialog.SourceIds.Where(id => TerrainTransfer.IsOriginalOccupied(_layer, id)).ToList();
                int empty = dialog.SourceIds.Count - sources.Count;
                if (sources.Count == 0)
                {
                    MessageBox.Show(Owner, "Die angegebenen Original-IDs sind alle leer.", dialog.Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int[] targets = ArtLibraryTransfer.AssignTargetIds(sources, dialog.TargetId, dialog.KeepIds, dialog.Overwrite, Store.IsOccupied, Lib.MaxId);

                bool takeSettings = land && (Settings.AutoTileData || Settings.AutoRadarColor);
                bool overwriteSettings = true;
                if (takeSettings)
                {
                    bool? answer = AskSettingsOverwrite(targets.Where(t => t >= 0), "den Werten aus dem Original");
                    if (answer == null)
                    {
                        return;
                    }

                    overwriteSettings = answer.Value;
                }

                var errors = new List<string>();
                int done = 0, first = -1, textures = 0;
                BeginBatch();
                try
                {
                    for (int i = 0; i < sources.Count; ++i)
                    {
                        int target = targets[i];
                        if (target < 0)
                        {
                            continue;
                        }

                        try
                        {
                            Store.SetBitmap(target, TerrainTransfer.GetOriginal(_layer, sources[i]));
                            TerrainSlotData slot = Meta.GetOrCreateSlot(target);
                            if (land)
                            {
                                slot.SourceLandId = sources[i];
                                bool mayWrite = overwriteSettings || !slot.HasSettings;
                                if (Settings.AutoTileData && mayWrite)
                                {
                                    TerrainTransfer.ReadTileDataInto(slot, sources[i]);
                                }

                                if (Settings.AutoRadarColor && mayWrite)
                                {
                                    slot.RadarColor = TerrainTransfer.ReadRadarColor(sources[i]) ?? slot.RadarColor;
                                }

                                if (Settings.FetchLinkedTexture && TerrainTransfer.TryReadTileData(sources[i], out _, out _, out ushort texId)
                                    && TerrainTransfer.IsOriginalOccupied(TerrainLayer.Texture, texId)
                                    && (dialog.Overwrite || !Lib.Textures.IsOccupied(target)))
                                {
                                    Lib.Textures.SetBitmap(target, TerrainTransfer.GetOriginal(TerrainLayer.Texture, texId));
                                    slot.SourceTextureId = texId;
                                    ++textures;
                                }

                                StoreName(target, TerrainTransfer.TryReadTileData(sources[i], out string name, out _, out _) ? name : string.Empty);
                            }
                            else
                            {
                                slot.SourceTextureId = sources[i];
                            }

                            Meta.MarkDirty();
                            first = first < 0 ? target : Math.Min(first, target);
                            ++done;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{ArtIdParser.ToHex(sources[i])}: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    EndBatch(first);
                }

                TerrainSession.NotifyMetadataChanged(this);
                string extra = (empty > 0 ? $"{empty} leere Original-IDs ignoriert.\n" : string.Empty)
                               + (textures > 0 ? $"{textures} zugehörige Textur(en) in dieselben Slots übernommen." : string.Empty);
                ReportResult(dialog.Title, done, sources.Count, errors, extra);
            }
        }

        private void FetchFromOriginalTab()
        {
            int id = -1;
            if (_layer == TerrainLayer.LandTile && TerrainWindows.LandSource != null && TerrainWindows.LandSource.IsLoaded)
            {
                id = TerrainWindows.LandSource.SelectedGraphicId;
            }
            else if (_layer == TerrainLayer.Texture && TerrainWindows.TextureSource != null)
            {
                id = TerrainWindows.TextureSource.SelectedTextureId;
            }

            if (id < 0)
            {
                MessageBox.Show(Owner, $"Im {(_layer == TerrainLayer.LandTile ? "Land-Tiles" : "Textures")}-Tab ist nichts ausgewählt " +
                                       "(oder das Fenster wurde nicht aus dem Tab geöffnet).", "Original", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FetchFromOriginal(new List<int> { id });
        }

        /// <summary>Copies (or moves) the selected graphics into the original file.</summary>
        private void TransferToOriginal(bool move)
        {
            if (Lib == null || !EnsureOriginal(_layer))
            {
                return;
            }

            List<int> ids = GetSelectedOccupiedIds();
            if (ids.Count == 0)
            {
                MessageBox.Show(Owner, "Bitte zuerst belegte Slots auswählen.", "Original", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int maxOriginal = TerrainTransfer.OriginalMaxId(_layer);
            bool land = _layer == TerrainLayer.LandTile;
            string action = move ? "verschieben" : "kopieren";

            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = $"Ins Original {action}";
                dialog.InfoText = $"{ids.Count} {_layer.DisplayName()} in die Originaldatei {action} (0 bis {ArtIdParser.ToHex(maxOriginal)}).\n"
                                  + (land
                                      ? $"RadarColor mitschreiben: {(Settings.WriteRadarColorToOriginal ? "AN" : "AUS (nur in der Bibliothek)")} - Menü Optionen.\n"
                                      : string.Empty)
                                  + "Danach im Original-Tab speichern!";
                dialog.ShowSource = false;
                dialog.ShowTransparencyOptions = false;
                dialog.KeepIdsText = "Gleiche IDs wie die Slots (bzw. Herkunfts-ID)";
                dialog.MaxTargetId = maxOriginal;
                TerrainSlotData firstSlot = Meta.GetSlot(ids[0]);
                int firstSource = (land ? firstSlot?.SourceLandId : firstSlot?.SourceTextureId) ?? ids[0];
                dialog.TargetId = Math.Min(firstSource, maxOriginal);
                dialog.KeepIds = ids.Count > 1 && ids.All(id => id <= maxOriginal);
                if (dialog.ShowDialog(Owner) != DialogResult.OK)
                {
                    return;
                }

                int[] targets = ArtLibraryTransfer.AssignTargetIds(ids, dialog.TargetId, dialog.KeepIds, dialog.Overwrite,
                    id => TerrainTransfer.IsOriginalOccupied(_layer, id), maxOriginal);

                if (ids.Count == 1 && !dialog.KeepIds)
                {
                    int target = dialog.TargetId;
                    if (TerrainTransfer.IsOriginalOccupied(_layer, target) && !dialog.Overwrite &&
                        MessageBox.Show(Owner, $"Im Original ist {ArtIdParser.ToDisplay(target)} belegt. Ersetzen?", dialog.Title,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }

                    targets[0] = target;
                }

                var errors = new List<string>();
                int done = 0, colors = 0, firstTarget = -1;
                BeginBatch();
                try
                {
                    for (int i = 0; i < ids.Count; ++i)
                    {
                        if (targets[i] < 0)
                        {
                            continue;
                        }

                        try
                        {
                            TerrainTransfer.WriteGraphicToOriginal(_layer, targets[i], Store.GetBitmap(ids[i]), this);
                            ushort? color = Meta.GetSlot(ids[i])?.RadarColor;
                            if (land && Settings.WriteRadarColorToOriginal && color != null && TerrainTransfer.WriteRadarColor(targets[i], color.Value))
                            {
                                ++colors;
                            }

                            if (move)
                            {
                                Store.Remove(ids[i]);
                            }

                            firstTarget = firstTarget < 0 ? targets[i] : firstTarget;
                            ++done;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{ArtIdParser.ToHex(ids[i])}: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    EndBatch();
                }

                if (firstTarget >= 0)
                {
                    try
                    {
                        if (land)
                        {
                            UserControls.LandTilesControl.SearchGraphic(firstTarget);
                        }
                    }
                    catch
                    {
                        // tab not loaded
                    }
                }

                string extra = (colors > 0 ? $"{colors} RadarColor(s) in radarcol.mul geschrieben (im RadarColor-Tab speichern).\n" : string.Empty)
                               + (land ? "Nicht vergessen: im Land-Tiles-Tab speichern (art.mul)." : "Nicht vergessen: im Textures-Tab speichern (texmaps.mul).");
                ReportResult($"Ins Original {action}", done, ids.Count, errors, extra);
            }
        }

        /// <summary>Sends the stored tiledata values (or colors) of the selected slots to tiledata.mul / radarcol.mul.</summary>
        private void WriteSettingsToOriginal(bool colors)
        {
            if (Lib == null)
            {
                return;
            }

            if (colors ? !TerrainTransfer.IsRadarColAvailable : !TerrainTransfer.IsTileDataAvailable)
            {
                MessageBox.Show(Owner, colors ? "radarcol.mul ist nicht geladen." : "tiledata.mul ist nicht geladen.", "Original",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ApplyDetailIfDirty();
            List<int> ids = GetSelectedIds().Where(id => colors ? Meta.GetSlot(id)?.RadarColor != null : Meta.GetSlot(id)?.HasTileData == true).ToList();
            if (ids.Count == 0)
            {
                MessageBox.Show(Owner, colors ? "Die ausgewählten Slots haben keine RadarColor." : "Die ausgewählten Slots haben keine tiledata-Werte.",
                    "Original", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string what = colors ? "RadarColor → radarcol.mul" : "tiledata-Werte → tiledata.mul";
            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = what;
                dialog.InfoText = $"{ids.Count} Slot(s): {what} schreiben (LandTile-IDs 0 bis 0x3FFF).\n" +
                                  "Vorhandene Werte im Original werden ersetzt. Danach im " + (colors ? "RadarColor" : "TileData") + "-Tab speichern!";
                dialog.ShowSource = false;
                dialog.ShowOverwrite = false;
                dialog.ShowTransparencyOptions = false;
                dialog.KeepIdsText = "Herkunfts-LandTile-ID bzw. Slot-Nummer verwenden";
                dialog.KeepIds = true;
                dialog.MaxTargetId = TerrainTransfer.OriginalMaxLandId;
                dialog.TargetId = Math.Min(Meta.GetSlot(ids[0])?.SourceLandId ?? ids[0], TerrainTransfer.OriginalMaxLandId);
                if (dialog.ShowDialog(Owner) != DialogResult.OK)
                {
                    return;
                }

                int done = 0;
                int next = dialog.TargetId;
                foreach (int id in ids)
                {
                    TerrainSlotData slot = Meta.GetSlot(id);
                    int target = dialog.KeepIds ? slot.SourceLandId ?? id : next++;
                    if (target < 0 || target > TerrainTransfer.OriginalMaxLandId)
                    {
                        continue;
                    }

                    bool ok = colors ? TerrainTransfer.WriteRadarColor(target, slot.RadarColor.Value) : TerrainTransfer.WriteTileData(slot, target, this);
                    done += ok ? 1 : 0;
                }

                ReportResult(what, done, ids.Count, new List<string>(),
                    colors ? "Im RadarColor-Tab speichern (radarcol.mul)." : "Im TileData-Tab speichern (tiledata.mul).");
            }
        }

        /// <summary>Reads the tiledata values of the slot's original land tile (button in the tiledata tab).</summary>
        private void ReadTileDataForSelected()
        {
            if (Meta == null || _selectedId < 0)
            {
                return;
            }

            if (!TerrainTransfer.IsTileDataAvailable)
            {
                MessageBox.Show(Owner, "tiledata.mul ist nicht geladen.", "tiledata", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            TerrainSlotData slot = Meta.GetOrCreateSlot(_selectedId);
            int landId = slot.SourceLandId ?? _selectedId;
            if (landId > TerrainTransfer.OriginalMaxLandId)
            {
                MessageBox.Show(Owner, $"Slot {ArtIdParser.ToHex(_selectedId)} hat keine Herkunfts-LandTile-ID und liegt über 0x3FFF.",
                    "tiledata", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (slot.HasTileData && MessageBox.Show(Owner,
                    $"Der Slot hat bereits tiledata-Werte (\"{slot.TileName}\").\nMit den Werten von LandTile {ArtIdParser.ToHex(landId)} aus tiledata.mul ersetzen?",
                    "tiledata", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            TerrainTransfer.ReadTileDataInto(slot, landId);
            AfterMetadataChanged();
            UpdateDetail();
            ShowStatusMessage($"tiledata-Werte von {ArtIdParser.ToHex(landId)} übernommen (nur in der Bibliothek)");
        }

        #endregion

        #region [ Templates (presets) ]

        private void ApplyPreset(TerrainPreset preset)
        {
            List<int> ids = GetSelectedIds();
            if (Meta == null || preset == null || ids.Count == 0)
            {
                return;
            }

            ApplyDetailIfDirty();
            bool? overwrite = AskSettingsOverwrite(ids, $"der Vorlage \"{preset.Name}\"");
            if (overwrite == null)
            {
                return;
            }

            int applied = ids.Count(id => Meta.ApplyPreset(id, preset, overwrite.Value));
            AfterMetadataChanged();
            UpdateDetail();
            ShowStatusMessage($"Vorlage \"{preset.Name}\" auf {applied} Slot(s) angewendet" + (applied < ids.Count ? $", {ids.Count - applied} behalten" : string.Empty));
        }

        private void SaveSlotAsPreset()
        {
            if (Meta == null || _selectedId < 0)
            {
                return;
            }

            ApplyDetailIfDirty();
            string name = Meta.Catalog.GetName(_selectedId);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = Meta.GetSlot(_selectedId)?.TileName;
            }

            using (var dialog = new TerrainPresetForm(Meta, _selectedId, string.IsNullOrWhiteSpace(name) ? $"Vorlage {ArtIdParser.ToHex(_selectedId)}" : name))
            {
                dialog.ShowDialog(Owner);
                if (dialog.Changed)
                {
                    TerrainSession.NotifyMetadataChanged(this);
                    FillDetailCombos();
                }
            }
        }

        private void ManagePresets()
        {
            if (Meta == null)
            {
                return;
            }

            using (var dialog = new TerrainPresetForm(Meta, _selectedId, null))
            {
                dialog.ShowDialog(Owner);
                if (dialog.Changed)
                {
                    TerrainSession.NotifyMetadataChanged(this);
                    FillDetailCombos();
                }
            }
        }

        #endregion

        #region [ Edit graphics ]

        private void TransformSelected(Func<Bitmap, Bitmap> transform, string what)
        {
            List<int> ids = GetSelectedOccupiedIds();
            if (ids.Count == 0)
            {
                return;
            }

            BeginBatch();
            try
            {
                foreach (int id in ids)
                {
                    using (Bitmap result = transform(Store.GetBitmap(id)))
                    {
                        Store.SetBitmap(id, result);
                    }
                }
            }
            finally
            {
                EndBatch();
            }

            ShowStatusMessage($"{ids.Count} {_layer.DisplayName()} {what}");
        }

        /// <summary>Creates the land tile of each selected slot from the slot's texture.</summary>
        private void CreateLandFromTexture()
        {
            if (Lib == null)
            {
                return;
            }

            List<int> ids = GetSelectedIds().Where(Lib.Textures.IsOccupied).ToList();
            if (ids.Count == 0)
            {
                MessageBox.Show(Owner, "Die ausgewählten Slots haben keine Textur.", "LandTile erzeugen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int existing = ids.Count(Lib.Land.IsOccupied);
            bool overwrite = true;
            if (existing > 0)
            {
                DialogResult answer = MessageBox.Show(Owner,
                    $"{existing} Slot(s) haben bereits ein LandTile.\n\nJa = ersetzen\nNein = nur leere Slots füllen",
                    "LandTile erzeugen", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (answer == DialogResult.Cancel)
                {
                    return;
                }

                overwrite = answer == DialogResult.Yes;
            }

            int done = 0;
            BeginBatch();
            try
            {
                foreach (int id in ids)
                {
                    if (!overwrite && Lib.Land.IsOccupied(id))
                    {
                        continue;
                    }

                    using (Bitmap tile = TerrainImageTools.LandFromTexture(Lib.Textures.GetBitmap(id), Settings.LandConversion))
                    {
                        Lib.Land.SetBitmap(id, tile);
                    }

                    ++done;
                }
            }
            finally
            {
                EndBatch();
            }

            ShowStatusMessage($"{done} LandTile(s) aus Texturen erzeugt ({(Settings.LandConversion == LandConversionMode.Isometric ? "isometrisch" : "zeilenweise")})");
        }

        #endregion

        #region [ Slots inside the library ]

        private void RemoveSelectedGraphics()
        {
            List<int> ids = GetSelectedOccupiedIds();
            if (ids.Count == 0)
            {
                return;
            }

            if (MessageBox.Show(Owner, ids.Count == 1
                        ? $"{_layer.SingleName()} {ArtIdParser.ToDisplay(ids[0])} entfernen?\n(Einstellungen und {_layer.Other().SingleName()} des Slots bleiben.)"
                        : $"{ids.Count} {_layer.DisplayName()} entfernen?\n(Einstellungen und Partner bleiben.)",
                    "Entfernen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            BeginBatch();
            try
            {
                ids.ForEach(Store.Remove);
            }
            finally
            {
                EndBatch();
            }
        }

        /// <summary>Clears the whole slot: land tile, texture, settings, name, category, tags.</summary>
        private void RemoveSelectedSlots()
        {
            List<int> ids = GetSelectedIds().Where(Lib.IsSlotUsed).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            if (MessageBox.Show(Owner, $"{ids.Count} Slot(s) komplett leeren?\nLandTile, Textur, Einstellungen, Name, Kategorie und Tags werden entfernt.",
                    "Slot leeren", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            BeginBatch();
            try
            {
                foreach (int id in ids)
                {
                    Lib.Land.Remove(id);
                    Lib.Textures.Remove(id);
                    Meta.RemoveSlot(id);
                }
            }
            finally
            {
                EndBatch();
            }

            TerrainSession.NotifyMetadataChanged(this);
        }

        /// <summary>Copies or moves whole slots (land tile, texture and settings) to other slot numbers.</summary>
        private void CopyOrMoveSlots(bool move)
        {
            List<int> ids = GetSelectedIds().Where(Lib.IsSlotUsed).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            string action = move ? "verschieben" : "kopieren";
            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = $"Slots {action}";
                dialog.InfoText = $"{ids.Count} Slot(s) mit LandTile, Textur und Einstellungen fortlaufend ab der Ziel-ID {action}.\n" +
                                  "Belegte Ziel-Slots werden übersprungen, außer \"ersetzen\" ist angehakt.";
                dialog.ShowSource = false;
                dialog.ShowModeOptions = false;
                dialog.ShowTransparencyOptions = false;
                dialog.MaxTargetId = Lib.MaxId;
                dialog.TargetId = Math.Max(0, Store.FindNextFree(ids[ids.Count - 1]));
                if (dialog.ShowDialog(Owner) != DialogResult.OK)
                {
                    return;
                }

                int[] targets = ArtLibraryTransfer.AssignTargetIds(ids, dialog.TargetId, false, dialog.Overwrite, Lib.IsSlotUsed, Lib.MaxId);
                if (ids.Count == 1 && targets[0] != dialog.TargetId)
                {
                    if (Lib.IsSlotUsed(dialog.TargetId) && MessageBox.Show(Owner,
                            $"Slot {ArtIdParser.ToDisplay(dialog.TargetId)} ist belegt (Grafik oder Einstellungen). Ersetzen?",
                            dialog.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }

                    targets[0] = dialog.TargetId;
                }

                // read everything first: targets may overlap sources
                var land = ids.Select(Lib.Land.GetRaw).ToList();
                var tex = ids.Select(Lib.Textures.GetRaw).ToList();
                var slots = ids.Select(id => Meta.GetSlot(id)?.Clone()).ToList();
                var infos = ids.Select(id => Meta.Catalog.Get(id)?.Clone()).ToList();

                int done = 0, first = -1;
                BeginBatch();
                try
                {
                    if (move)
                    {
                        var targetSet = new HashSet<int>(targets.Where(t => t >= 0));
                        for (int i = 0; i < ids.Count; ++i)
                        {
                            if (targets[i] >= 0 && !targetSet.Contains(ids[i]))
                            {
                                Lib.Land.Remove(ids[i]);
                                Lib.Textures.Remove(ids[i]);
                                Meta.RemoveSlot(ids[i]);
                            }
                        }
                    }

                    for (int i = 0; i < ids.Count; ++i)
                    {
                        int t = targets[i];
                        if (t < 0 || t == ids[i])
                        {
                            continue;
                        }

                        WriteOrRemove(Lib.Land, t, land[i]);
                        WriteOrRemove(Lib.Textures, t, tex[i]);
                        Meta.RemoveSlot(t);
                        if (slots[i] != null)
                        {
                            Meta.Slots[t] = slots[i];
                        }

                        if (infos[i] != null)
                        {
                            Meta.Catalog.Set(t, infos[i]);
                        }

                        Meta.MarkDirty();
                        first = first < 0 ? t : Math.Min(first, t);
                        ++done;
                    }
                }
                finally
                {
                    EndBatch(first);
                }

                TerrainSession.NotifyMetadataChanged(this);
                ShowStatusMessage($"{done} Slot(s) {(move ? "verschoben" : "kopiert")}");
            }
        }

        private static void WriteOrRemove(TerrainLayerStorage storage, int id, byte[] data)
        {
            if (data == null)
            {
                storage.Remove(id);
            }
            else
            {
                storage.SetRaw(id, data);
            }
        }

        private void SwapSlots()
        {
            List<int> ids = GetSelectedIds();
            if (Lib == null || ids.Count != 2)
            {
                return;
            }

            BeginBatch();
            try
            {
                Lib.Land.Swap(ids[0], ids[1]);
                Lib.Textures.Swap(ids[0], ids[1]);
                Meta.SwapSlots(ids[0], ids[1]);
            }
            finally
            {
                EndBatch();
            }

            TerrainSession.NotifyMetadataChanged(this);
            ShowStatusMessage($"Slots {ArtIdParser.ToHex(ids[0])} und {ArtIdParser.ToHex(ids[1])} getauscht");
        }

        private void FindNextFree()
        {
            if (Lib == null)
            {
                return;
            }

            int id = Store.FindNextFree(Math.Max(0, _selectedId));
            if (id < 0)
            {
                MessageBox.Show(Owner, "Ab der Auswahl gibt es keinen freien Slot mehr.", "Nächster freier Slot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SelectSlot(id);
        }

        private void GoToIdFromTextBox()
        {
            if (Lib == null)
            {
                return;
            }

            if (!ArtIdParser.TryParse(toolStripTextBoxId.Text, out int id) || !Lib.IsValidId(id))
            {
                MessageBox.Show(Owner, $"Ungültige ID. Erlaubt sind 0 bis {ArtIdParser.ToDisplay(Lib.MaxId)}, dezimal oder hex (0x4D2, 4D2h).",
                    "Gehe zu ID", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SelectSlot(id);
            LibraryTileView.Focus();
        }

        private void SetCategoryForSelection(int? categoryId)
        {
            List<int> ids = GetSelectedIds();
            foreach (int id in ids)
            {
                Meta.Catalog.GetOrCreate(id).CategoryId = categoryId;
            }

            Meta.Catalog.MarkDirty();
            AfterSelectionMetadataChanged();
        }

        private void AddTagToSelection(string tag)
        {
            foreach (int id in GetSelectedIds())
            {
                ArtLibraryItemInfo info = Meta.Catalog.GetOrCreate(id);
                info.Tags = ArtLibraryMetadata.NormalizeTags(info.Tags.Concat(new[] { tag }));
            }

            Meta.Catalog.AddTag(tag);
            Meta.Catalog.MarkDirty();
            AfterSelectionMetadataChanged();
        }

        private void RemoveTagFromSelection(string tag)
        {
            foreach (int id in GetSelectedIds())
            {
                Meta.Catalog.Get(id)?.Tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
            }

            Meta.Catalog.MarkDirty();
            AfterSelectionMetadataChanged();
        }

        private void ToggleFavorite()
        {
            List<int> ids = GetSelectedIds();
            if (ids.Count == 0)
            {
                return;
            }

            bool value = !(Meta.GetSlot(ids[0])?.Favorite ?? false);
            ids.ForEach(id => Meta.GetOrCreateSlot(id).Favorite = value);
            AfterSelectionMetadataChanged();
        }

        private void AfterSelectionMetadataChanged()
        {
            TerrainSession.NotifyMetadataChanged(this);
            if (IsFilterActive)
            {
                int keep = _selectedId;
                ReloadList();
                RestoreSelection(keep);
            }
            else
            {
                LibraryTileView.Invalidate();
                UpdateDetail();
            }
        }

        #endregion

        #region [ Library menu ]

        private void ShowProperties()
        {
            if (Lib == null)
            {
                return;
            }

            string text =
                $"Name:          {Lib.Name}\n" +
                $"Dateien:       {Lib.BasePath}.landlib / .texlib / .terrain.json\n" +
                $"Slots:         {Lib.Capacity:N0} (0 bis {ArtIdParser.ToHex(Lib.MaxId)})\n" +
                $"LandTiles:     {Lib.Land.OccupiedCount:N0}\n" +
                $"Texturen:      {Lib.Textures.OccupiedCount:N0}\n" +
                $"Einstellungen: {Meta.Slots.Count(p => p.Value.HasSettings):N0} Slots\n" +
                $"Daten:         {(Lib.Land.MulLength + Lib.Textures.MulLength) / (1024.0 * 1024.0):N2} MB\n" +
                $"Ungenutzt:     {Lib.GetUnusedBytes() / (1024.0 * 1024.0):N2} MB (Komprimieren gibt ihn frei)\n" +
                $"Kategorien:    {Meta.Catalog.Categories.Count}   Tags: {Meta.Catalog.Tags.Count}   Vorlagen: {Meta.Presets.Count}";
            MessageBox.Show(Owner, text, "Eigenschaften der Terrain-Bibliothek", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ManageCategories()
        {
            if (Meta == null)
            {
                return;
            }

            ApplyDetailIfDirty();
            using (var dialog = new ArtLibraryCategoryForm(Meta.Catalog))
            {
                dialog.ShowDialog(Owner);
                if (!dialog.Changed)
                {
                    return;
                }
            }

            TerrainSession.NotifyMetadataChanged(this);
            FillFilterCombos();
            FillDetailCombos();
            int keep = _selectedId;
            ReloadList();
            RestoreSelection(keep);
        }

        private void GrowLibrary()
        {
            using (var dialog = new ArtLibraryGrowForm())
            {
                dialog.CurrentCapacity = Lib.Capacity;
                if (dialog.ShowDialog(Owner) != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    Lib.Grow(dialog.NewCapacity);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Owner, ex.Message, "Erweitern", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void CompactLibrary()
        {
            if (MessageBox.Show(Owner, $"Ungenutzter Platz: {Lib.GetUnusedBytes() / (1024.0 * 1024.0):N2} MB.\n\n" +
                                       "Die Datendateien werden neu geschrieben (ungespeicherte Änderungen werden dabei gespeichert). Fortfahren?",
                    "Komprimieren", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                long saved = Lib.Compact();
                MessageBox.Show(Owner, $"Fertig. {saved / (1024.0 * 1024.0):N2} MB freigegeben.", "Komprimieren", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Owner, ex.Message, "Komprimieren", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ShowHelp()
        {
            using (var help = new TerrainHelpForm())
            {
                help.ShowDialog(Owner);
            }
        }

        #endregion

        #region [ Event handlers: main toolbar ]

        private void ToolStripComboBoxLibrary_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingLibraryCombo || !(toolStripComboBoxLibrary.SelectedItem is RecentItem item))
            {
                return;
            }

            if (Lib != null && item.Matches(Lib.MainPath))
            {
                return;
            }

            OpenLibrary(item.Path);
        }

        private void ToolStripButtonNew_Click(object sender, EventArgs e) => CreateLibrary();

        private void ToolStripButtonOpen_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Terrain-Bibliothek öffnen";
                dialog.Filter = "Terrain-Bibliothek (*.landlib.idx;*.texlib.idx;*.terrain.json)|*.landlib.idx;*.texlib.idx;*.terrain.json|Alle Dateien (*.*)|*.*";
                if (Directory.Exists(Settings.LastDirectory))
                {
                    dialog.InitialDirectory = Settings.LastDirectory;
                }

                if (dialog.ShowDialog(Owner) == DialogResult.OK)
                {
                    OpenLibrary(dialog.FileName);
                }
            }
        }

        private void ToolStripButtonReload_Click(object sender, EventArgs e)
        {
            if (Lib == null)
            {
                return;
            }

            if (TerrainSession.HasUnsavedChanges && !Settings.AutoSave &&
                MessageBox.Show(Owner, "Ungespeicherte Änderungen verwerfen und neu laden?", "Neu laden", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _infoDirty = _tileDirty = _colorDirty = false;
            string path = Lib.MainPath;
            int keep = _selectedId;
            TerrainSession.CloseLibrary();
            try
            {
                TerrainSession.SetLibrary(TerrainLibraryFile.Open(path));
                SelectSlot(keep);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Owner, ex.Message, "Neu laden", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToolStripButtonSave_Click(object sender, EventArgs e) => SaveAll(true);

        private void ToolStripButtonAutoSave_Click(object sender, EventArgs e)
        {
            ApplyDetailIfDirty();
            TerrainSession.SetAutoSave(toolStripButtonAutoSave.Checked);
            ShowStatusMessage(toolStripButtonAutoSave.Checked
                ? "Auto-Speichern an: jede Änderung wird sofort gespeichert"
                : "Auto-Speichern aus: Änderungen erst mit \"Speichern\" (Strg+S) sichern");
        }

        private void LibraryPropertiesMenu_Click(object sender, EventArgs e) => ShowProperties();

        private void ManageCategoriesMenu_Click(object sender, EventArgs e) => ManageCategories();

        private void ManagePresetsMenu_Click(object sender, EventArgs e) => ManagePresets();

        private void GrowMenu_Click(object sender, EventArgs e) => GrowLibrary();

        private void CompactMenu_Click(object sender, EventArgs e) => CompactLibrary();

        private void OpenFolderMenu_Click(object sender, EventArgs e)
        {
            if (Lib == null)
            {
                return;
            }

            try
            {
                Process.Start("explorer.exe", $"/select,\"{Lib.MainPath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Owner, ex.Message, "Ordner öffnen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveFromRecentMenu_Click(object sender, EventArgs e)
        {
            if (toolStripComboBoxLibrary.SelectedItem is RecentItem item)
            {
                Settings.RemoveRecent(item.Path);
                Settings.Save();
                FillLibraryCombo();
            }
        }

        private void ToolStripTextBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                GoToIdFromTextBox();
            }
        }

        private void ToolStripButtonGoTo_Click(object sender, EventArgs e) => GoToIdFromTextBox();

        private void ToolStripButtonShowFreeSlots_Click(object sender, EventArgs e) => SetShowFreeSlots(toolStripButtonShowFreeSlots.Checked);

        private void CtxShowFreeSlots_Click(object sender, EventArgs e) => SetShowFreeSlots(!ShowFreeSlots);

        private void ToolStripButtonCopy_Click(object sender, EventArgs e) => CopySelectedToClipboard();

        private void ToolStripButtonPaste_Click(object sender, EventArgs e) => PasteFromClipboard(false);

        private void OrigFetchSelection_Click(object sender, EventArgs e) => FetchFromOriginalTab();

        private void OrigFetchIds_Click(object sender, EventArgs e) => FetchFromOriginal(null);

        private void OrigCopyTo_Click(object sender, EventArgs e) => TransferToOriginal(false);

        private void OrigMoveTo_Click(object sender, EventArgs e) => TransferToOriginal(true);

        private void OrigWriteTileData_Click(object sender, EventArgs e) => WriteSettingsToOriginal(false);

        private void OrigWriteRadarColor_Click(object sender, EventArgs e) => WriteSettingsToOriginal(true);

        private void EditRotateLeft_Click(object sender, EventArgs e) => TransformSelected(b => TerrainImageTools.Rotate(b, false), "nach links gedreht");

        private void EditRotateRight_Click(object sender, EventArgs e) => TransformSelected(b => TerrainImageTools.Rotate(b, true), "nach rechts gedreht");

        private void EditMirrorH_Click(object sender, EventArgs e) => TransformSelected(b => TerrainImageTools.Mirror(b, true), "horizontal gespiegelt");

        private void EditMirrorV_Click(object sender, EventArgs e) => TransformSelected(b => TerrainImageTools.Mirror(b, false), "vertikal gespiegelt");

        private void EditLandFromTexture_Click(object sender, EventArgs e) => CreateLandFromTexture();

        private void OptAutoScale_Click(object sender, EventArgs e) => ToggleOption(() => Settings.AutoScale = !Settings.AutoScale);

        private void OptAutoTileData_Click(object sender, EventArgs e) => ToggleOption(() => Settings.AutoTileData = !Settings.AutoTileData);

        private void OptFetchLinkedTexture_Click(object sender, EventArgs e) => ToggleOption(() => Settings.FetchLinkedTexture = !Settings.FetchLinkedTexture);

        private void OptAutoRadarColor_Click(object sender, EventArgs e) => ToggleOption(() => Settings.AutoRadarColor = !Settings.AutoRadarColor);

        private void OptWriteRadarColor_Click(object sender, EventArgs e)
        {
            ToggleOption(() => Settings.WriteRadarColorToOriginal = !Settings.WriteRadarColorToOriginal);
            MessageBox.Show(Owner, Settings.WriteRadarColorToOriginal
                    ? "AN: Beim Übertragen von LandTiles ins Original wird die RadarColor des Slots in radarcol.mul geschrieben.\n" +
                      "Gespeichert wird die Datei im RadarColor-Tab."
                    : "AUS: Die RadarColor bleibt nur in der Bibliothek gespeichert.\n" +
                      "Mit \"RadarColor → radarcol.mul\" kannst du sie jederzeit von Hand übertragen.",
                "RadarColor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OptBuiltInPresets_Click(object sender, EventArgs e)
        {
            ToggleOption(() => Settings.UseBuiltInPresets = !Settings.UseBuiltInPresets);
            FillDetailCombos();
        }

        private void OptLandIso_Click(object sender, EventArgs e) => ToggleOption(() => Settings.LandConversion = LandConversionMode.Isometric);

        private void OptLandStretch_Click(object sender, EventArgs e) => ToggleOption(() => Settings.LandConversion = LandConversionMode.RowStretch);

        private void OptSyncSelection_Click(object sender, EventArgs e) => ToggleOption(() => Settings.SyncSelection = !Settings.SyncSelection);

        private void ToggleOption(Action change)
        {
            change();
            Settings.Save();
            ApplyOptionChecks();
        }

        private void ToolStripButtonPartner_Click(object sender, EventArgs e) => TerrainWindows.ShowPartner(_layer, _selectedId);

        private void ToolStripButtonDock_Click(object sender, EventArgs e) => TerrainWindows.SetDocked(toolStripButtonDock.Checked);

        private void ToolStripButtonHelp_Click(object sender, EventArgs e) => ShowHelp();

        #endregion

        #region [ Event handlers: filter toolbar ]

        private void ToolStripTextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                RunSearch();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                toolStripTextBoxSearch.Text = string.Empty;
                RunSearch();
            }
        }

        private void ToolStripButtonSearch_Click(object sender, EventArgs e) => RunSearch();

        private void ToolStripButtonClearFilter_Click(object sender, EventArgs e) => ClearFilters();

        private void ToolStripComboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingFilter)
            {
                return;
            }

            _categoryFilter = (toolStripComboBoxCategory.SelectedItem as ComboItem)?.Id ?? AllCategories;
            _categoryFilterIds = _categoryFilter >= 0 && Meta != null ? Meta.Catalog.GetCategoryWithChildren(_categoryFilter) : new HashSet<int>();
            ApplyFilterChange();
        }

        private void ToolStripComboBoxTag_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingFilter)
            {
                return;
            }

            _tagFilter = toolStripComboBoxTag.SelectedIndex <= 0 ? null : toolStripComboBoxTag.SelectedItem as string;
            ApplyFilterChange();
        }

        private void ToolStripButtonFavorites_Click(object sender, EventArgs e)
        {
            _favoritesOnly = toolStripButtonFavorites.Checked;
            ApplyFilterChange();
        }

        private void ToolStripButtonWithPartner_Click(object sender, EventArgs e)
        {
            _withPartnerOnly = toolStripButtonWithPartner.Checked;
            ApplyFilterChange();
        }

        private void ToolStripButtonWithSettings_Click(object sender, EventArgs e)
        {
            _withSettingsOnly = toolStripButtonWithSettings.Checked;
            ApplyFilterChange();
        }

        #endregion

        #region [ Event handlers: context menu ]

        private void ContextMenuStripTileView_Opening(object sender, CancelEventArgs e)
        {
            bool open = Lib != null;
            List<int> occupied = open ? GetSelectedOccupiedIds() : new List<int>();
            bool any = occupied.Count > 0;
            bool canPaste = open && TerrainClipboard.ContainsGraphics(_layer);

            ctxPaste.Enabled = ctxPasteKeepIds.Enabled = canPaste;
            ctxCopy.Enabled = any;
            ctxImportFiles.Enabled = ctxImportFolder.Enabled = ctxExport.Enabled = open;
            ctxOriginal.Enabled = ctxEdit.Enabled = open;
            ctxOrigCopyTo.Enabled = ctxOrigMoveTo.Enabled = any;
            ctxRotateLeft.Enabled = ctxRotateRight.Enabled = ctxMirrorH.Enabled = ctxMirrorV.Enabled = any;
            ctxCopyToId.Enabled = ctxMoveToId.Enabled = open && _selectedId >= 0;
            ctxSwap.Enabled = open && GetSelectedIds().Count == 2;
            ctxFavorite.Enabled = ctxCategory.Enabled = ctxPreset.Enabled = open && _selectedId >= 0;
            ctxRemove.Enabled = any;
            ctxRemoveSlot.Enabled = open && _selectedId >= 0;
            ctxFindNextFree.Enabled = open;
            ctxCopyIdHex.Enabled = ctxCopyIdDec.Enabled = open && _selectedId >= 0;

            BuildDynamicMenus(GetSelectedIds());
        }

        private void BuildDynamicMenus(List<int> ids)
        {
            ctxCategory.DropDownItems.Clear();
            ctxTagAdd.DropDownItems.Clear();
            ctxTagRemove.DropDownItems.Clear();
            ctxPreset.DropDownItems.Clear();
            if (Meta == null)
            {
                ctxTagAdd.Enabled = ctxTagRemove.Enabled = false;
                return;
            }

            var none = new ToolStripMenuItem("(keine)");
            none.Click += (s, a) => SetCategoryForSelection(null);
            ctxCategory.DropDownItems.Add(none);
            foreach (ArtLibraryCategory category in Meta.Catalog.GetCategoriesSorted())
            {
                int id = category.Id;
                var item = new ToolStripMenuItem(Meta.Catalog.GetCategoryPath(id));
                item.Click += (s, a) => SetCategoryForSelection(id);
                ctxCategory.DropDownItems.Add(item);
            }

            ctxCategory.DropDownItems.Add(new ToolStripSeparator());
            var manage = new ToolStripMenuItem("Kategorien verwalten...");
            manage.Click += (s, a) => ManageCategories();
            ctxCategory.DropDownItems.Add(manage);

            foreach (string tag in Meta.Catalog.Tags)
            {
                string value = tag;
                var item = new ToolStripMenuItem(value);
                item.Click += (s, a) => AddTagToSelection(value);
                ctxTagAdd.DropDownItems.Add(item);
            }

            ctxTagAdd.Enabled = ids.Count > 0 && Meta.Catalog.Tags.Count > 0;

            List<string> used = ids.Select(id => Meta.Catalog.Get(id)).Where(i => i != null).SelectMany(i => i.Tags)
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t, StringComparer.CurrentCultureIgnoreCase).ToList();
            foreach (string tag in used)
            {
                string value = tag;
                var item = new ToolStripMenuItem(value);
                item.Click += (s, a) => RemoveTagFromSelection(value);
                ctxTagRemove.DropDownItems.Add(item);
            }

            ctxTagRemove.Enabled = used.Count > 0;

            foreach (TerrainPreset preset in Meta.GetAllPresets(Settings.UseBuiltInPresets))
            {
                TerrainPreset value = preset;
                var item = new ToolStripMenuItem(preset.ToString());
                if (preset.RadarColor != null)
                {
                    var swatch = new Bitmap(12, 12);
                    using (Graphics g = Graphics.FromImage(swatch))
                    {
                        g.Clear(TerrainColor.ToColor(preset.RadarColor.Value));
                    }

                    item.Image = swatch;
                }

                item.Click += (s, a) => ApplyPreset(value);
                ctxPreset.DropDownItems.Add(item);
            }

            ctxPreset.DropDownItems.Add(new ToolStripSeparator());
            var managePresets = new ToolStripMenuItem("Vorlagen verwalten...");
            managePresets.Click += (s, a) => ManagePresets();
            ctxPreset.DropDownItems.Add(managePresets);
        }

        private void CtxPaste_Click(object sender, EventArgs e) => PasteFromClipboard(false);

        private void CtxPasteKeepIds_Click(object sender, EventArgs e) => PasteFromClipboard(true);

        private void CtxCopy_Click(object sender, EventArgs e) => CopySelectedToClipboard();

        private void CtxImportFiles_Click(object sender, EventArgs e) => ImportFilesWithDialog();

        private void CtxImportFolder_Click(object sender, EventArgs e) => ImportFolderWithDialog();

        private void CtxExportBmp_Click(object sender, EventArgs e) => ExportGraphics(ImageFormat.Bmp, "bmp", false);

        private void CtxExportPng_Click(object sender, EventArgs e) => ExportGraphics(ImageFormat.Png, "png", false);

        private void CtxExportTiff_Click(object sender, EventArgs e) => ExportGraphics(ImageFormat.Tiff, "tiff", false);

        private void CtxExportAllBmp_Click(object sender, EventArgs e) => ExportGraphics(ImageFormat.Bmp, "bmp", true);

        private void CtxExportAllPng_Click(object sender, EventArgs e) => ExportGraphics(ImageFormat.Png, "png", true);

        private void CtxCopyToId_Click(object sender, EventArgs e) => CopyOrMoveSlots(false);

        private void CtxMoveToId_Click(object sender, EventArgs e) => CopyOrMoveSlots(true);

        private void CtxSwap_Click(object sender, EventArgs e) => SwapSlots();

        private void CtxFavorite_Click(object sender, EventArgs e) => ToggleFavorite();

        private void CtxFindNextFree_Click(object sender, EventArgs e) => FindNextFree();

        private void CtxCopyIdHex_Click(object sender, EventArgs e)
        {
            if (_selectedId >= 0)
            {
                Clipboard.SetText(ArtIdParser.ToHex(_selectedId));
            }
        }

        private void CtxCopyIdDec_Click(object sender, EventArgs e)
        {
            if (_selectedId >= 0)
            {
                Clipboard.SetText(_selectedId.ToString());
            }
        }

        private void CtxRemove_Click(object sender, EventArgs e) => RemoveSelectedGraphics();

        private void CtxRemoveSlot_Click(object sender, EventArgs e) => RemoveSelectedSlots();

        private void CtxBackground_Click(object sender, EventArgs e)
        {
            colorDialog.Color = _backgroundColor;
            if (colorDialog.ShowDialog(Owner) == DialogResult.OK)
            {
                _backgroundColor = colorDialog.Color;
                LibraryTileView.BackColor = _backgroundColor;
                LibraryTileView.Invalidate();
                UpdateDetail();
            }
        }

        #endregion

        #region [ Event handlers: detail tabs ]

        private void InfoField_Changed(object sender, EventArgs e)
        {
            if (!_updatingDetail && !_infoDirty)
            {
                _infoDirty = true;
                UpdateSaveStatus();
            }
        }

        private void TileField_Changed(object sender, EventArgs e)
        {
            if (!_updatingDetail && !_tileDirty)
            {
                _tileDirty = true;
                UpdateSaveStatus();
            }
        }

        private void CheckedListBoxFlags_ItemCheck(object sender, ItemCheckEventArgs e) => TileField_Changed(sender, e);

        private void ButtonSaveInfo_Click(object sender, EventArgs e)
        {
            ApplyInfo();
            ShowStatusMessage(Settings.AutoSave ? "Angaben gespeichert" : "Angaben übernommen - mit \"Speichern\" sichern");
        }

        private void TextBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ButtonSaveInfo_Click(sender, e);
            }
        }

        private void CheckBoxFavorite_CheckedChanged(object sender, EventArgs e) => InfoField_Changed(sender, e);

        private void ButtonApplyTileData_Click(object sender, EventArgs e)
        {
            ApplyTileData();
            UpdateDetail();
            ShowStatusMessage("tiledata-Werte in der Bibliothek gespeichert (noch nicht in tiledata.mul)");
        }

        private void ButtonTileDataFromOriginal_Click(object sender, EventArgs e) => ReadTileDataForSelected();

        private void ButtonTileDataToOriginal_Click(object sender, EventArgs e) => WriteSettingsToOriginal(false);

        private void ButtonApplyPreset_Click(object sender, EventArgs e) => ApplyPreset(comboBoxPreset.SelectedItem as TerrainPreset);

        private void ButtonSaveAsPreset_Click(object sender, EventArgs e) => SaveSlotAsPreset();

        private void ButtonColorAverage_Click(object sender, EventArgs e)
        {
            Bitmap source = Lib?.Textures.GetBitmap(_selectedId) ?? Lib?.Land.GetBitmap(_selectedId);
            ushort? color = TerrainColor.AverageValue(source);
            if (color == null)
            {
                MessageBox.Show(Owner, "Der Slot hat keine Grafik, aus der eine Farbe berechnet werden kann.", "Mittelwert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetEditColor(color);
            ShowStatusMessage("Mittelwert-Farbe berechnet und im Slot gespeichert");
        }

        private void ButtonColorFromOriginal_Click(object sender, EventArgs e)
        {
            int landId = Meta?.GetSlot(_selectedId)?.SourceLandId ?? _selectedId;
            ushort? color = TerrainTransfer.ReadRadarColor(landId);
            if (color == null)
            {
                MessageBox.Show(Owner, "radarcol.mul ist nicht geladen oder die ID liegt über 0x3FFF.", "RadarColor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_editColor != null && _editColor != color && MessageBox.Show(Owner,
                    $"Der Slot hat bereits die Farbe {TerrainColor.ToHex(_editColor.Value)}. Durch {TerrainColor.ToHex(color.Value)} aus radarcol.mul ersetzen?",
                    "RadarColor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            SetEditColor(color);
        }

        private void ButtonColorToOriginal_Click(object sender, EventArgs e) => WriteSettingsToOriginal(true);

        private void ButtonColorPick_Click(object sender, EventArgs e)
        {
            colorDialog.Color = _editColor == null ? Color.Gray : TerrainColor.ToColor(_editColor.Value);
            if (colorDialog.ShowDialog(Owner) == DialogResult.OK)
            {
                SetEditColor(TerrainColor.FromColor(colorDialog.Color));
            }
        }

        private void ButtonColorClear_Click(object sender, EventArgs e) => SetEditColor(null);

        private void TextBoxColor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            if (TerrainColor.TryParse(textBoxColor.Text, out ushort value))
            {
                SetEditColor(value);
            }
            else
            {
                MessageBox.Show(Owner, "Farbe als 0x1234 (radarcol-Wert) oder #RRGGBB eingeben.", "RadarColor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region [ Drag & drop on the preview ]

        private void PictureBoxPreview_DragEnter(object sender, DragEventArgs e)
        {
            bool files = e.Data.GetData(DataFormats.FileDrop) is string[] paths && paths.Any(ArtLibraryImageConverter.IsSupportedFile);
            e.Effect = Lib != null && (files || e.Data.GetDataPresent(DataFormats.Bitmap)) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void PictureBoxPreview_DragDrop(object sender, DragEventArgs e)
        {
            if (Lib == null)
            {
                return;
            }

            if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            {
                List<string> files = paths.Where(ArtLibraryImageConverter.IsSupportedFile).ToList();
                if (files.Count == 1)
                {
                    try
                    {
                        using (Bitmap loaded = ArtLibraryImageConverter.LoadFile(files[0]))
                        {
                            PlaceImage(loaded, ArtLibraryTransfer.ParseIdFromFileName(files[0]) < 0 ? Path.GetFileNameWithoutExtension(files[0]) : string.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Owner, ex.Message, "Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (files.Count > 1)
                {
                    ImportFiles(files);
                }

                return;
            }

            if (e.Data.GetData(DataFormats.Bitmap) is Image image)
            {
                PlaceImage(image, string.Empty);
            }
        }

        #endregion

        private void TimerSave_Tick(object sender, EventArgs e)
        {
            if (Lib != null)
            {
                TerrainSession.SaveMetadataIfAuto();
            }
        }
    }
}
