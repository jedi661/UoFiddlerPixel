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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Core;
using UoFiddler.Controls.ArtLibrary.Helpers;
using UoFiddler.Controls.ArtLibrary.Metadata;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.UserControls;
using UoFiddler.Controls.UserControls.TileView;

namespace UoFiddler.Controls.ArtLibrary.Forms
{
    /// <summary>
    /// Window of the art library ("benutzerdefinierte Art-MUL-Bibliothek"). Looks and works like the
    /// items tab (ItemsControl), but shows an <see cref="ArtLibraryFile"/> instead of the original art.mul.
    /// Opened from the ItemsControl toolbar, runs independently next to it.
    /// </summary>
    public partial class ArtLibraryForm : Form
    {
        private static readonly Font _freeSlotFont = new Font(FontFamily.GenericSansSerif, 6.5f);

        private readonly ArtLibrarySettings _settings;
        private readonly ArtImportOptions _importOptions = ArtImportOptions.Default;

        private ArtLibraryFile _library;
        private ArtLibraryMetadata _metadata;

        // ids shown in list mode (only occupied and/or filter), ascending; in "all ids" mode the tile index is the id
        private List<int> _viewIds = new List<int>();
        private bool _showOnlyOccupied = true;
        private int _selectedId = -1;
        private Color _backgroundColor = Color.White;
        private int _batchDepth;

        private bool _updatingLibraryCombo;
        private bool _updatingFilter;
        private bool _updatingDetail;
        private bool _detailDirty;

        // filter
        private string _searchText = string.Empty;
        private int? _searchId;
        private CategoryFilterItem _categoryFilter = CategoryFilterItem.All;
        private HashSet<int> _categoryFilterIds = new HashSet<int>();
        private string _tagFilter;
        private bool _favoritesOnly;

        public ArtLibraryForm()
        {
            InitializeComponent();

            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Icon = Options.GetFiddlerIcon();
            pictureBoxPreview.AllowDrop = true;

            _settings = ArtLibrarySettings.Load();
            _showOnlyOccupied = _settings.ShowOnlyOccupied;
            toolStripButtonOnlyOccupied.Checked = _showOnlyOccupied;

            ApplyTileViewOptions();
            FillLibraryCombo();
            FillFilterCombos();
            FillDetailCategoryCombo();
            UpdateAll();
        }

        #region [ Public ]

        /// <summary>The currently opened library or null.</summary>
        public ArtLibraryFile Library => _library;

        /// <summary>Metadata of the currently opened library or null.</summary>
        public ArtLibraryMetadata Metadata => _metadata;

        /// <summary>Selected graphic id or -1.</summary>
        public int SelectedId => _selectedId;

        /// <summary>
        /// Selects <paramref name="id"/> in the library (clears filters / switches to "all ids" if needed).
        /// </summary>
        public void SelectId(int id)
        {
            if (_library == null || !_library.IsValidId(id))
            {
                return;
            }

            if (IsListMode && GetIndexOfId(id) < 0)
            {
                if (IsFilterActive)
                {
                    ClearFilters();
                }

                if (_showOnlyOccupied && !_library.IsOccupied(id))
                {
                    SetShowOnlyOccupied(false);
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

        #endregion

        #region [ Open / Create / Close ]

        private void OpenLibrary(string path, bool silent = false)
        {
            ArtLibraryFile library;
            try
            {
                library = ArtLibraryFile.Open(path);
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    MessageBox.Show(this, $"Die Bibliothek konnte nicht geöffnet werden:\n{ex.Message}",
                        "Art-Bibliothek", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (!File.Exists(ArtLibraryFile.GetBasePath(path) + ArtLibraryFile.IdxExtension))
                {
                    _settings.RemoveRecent(path);
                    _settings.Save();
                }

                FillLibraryCombo();
                return;
            }

            SetLibrary(library);
        }

        private void SetLibrary(ArtLibraryFile library)
        {
            CloseLibrary();

            _library = library;
            _library.EntryChanged += OnLibraryEntryChanged;
            _metadata = LoadMetadata(_library);

            _settings.AddRecent(_library.IdxPath);
            _settings.LastLibrary = _library.IdxPath;
            _settings.LastDirectory = Path.GetDirectoryName(_library.BasePath);
            _settings.Save();

            FillLibraryCombo();
            ResetFilterState();
            FillFilterCombos();
            FillDetailCategoryCombo();
            ReloadList();

            if (LibraryTileView.VirtualListSize > 0)
            {
                LibraryTileView.FocusIndex = 0;
                SetSelectedId(GetIdAtIndex(0));
            }
        }

        private ArtLibraryMetadata LoadMetadata(ArtLibraryFile library)
        {
            try
            {
                return ArtLibraryMetadata.Load(library.MetadataPath);
            }
            catch (Exception ex)
            {
                // keep the damaged file, start with empty metadata
                string backup = library.MetadataPath + $".defekt-{DateTime.Now:yyyyMMdd-HHmmss}";
                try
                {
                    File.Copy(library.MetadataPath, backup, true);
                }
                catch
                {
                    backup = "(Sicherung fehlgeschlagen)";
                }

                MessageBox.Show(this,
                    $"Die Metadaten (Namen, Kategorien, Tags) konnten nicht gelesen werden:\n{ex.Message}\n\n" +
                    $"Die Datei wurde gesichert als:\n{backup}",
                    "Art-Bibliothek", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                File.Delete(library.MetadataPath);
                return ArtLibraryMetadata.Load(library.MetadataPath);
            }
        }

        private void CloseLibrary()
        {
            ApplyDetailIfDirty();
            SaveMetadata();

            if (_library != null)
            {
                _library.EntryChanged -= OnLibraryEntryChanged;
                _library.Dispose();
                _library = null;
            }

            _metadata = null;
            _viewIds = new List<int>();
            pictureBoxPreview.Image = null;
            SetSelectedId(-1);
            ReloadList();
        }

        private void SaveMetadata()
        {
            if (_metadata == null || !_metadata.IsDirty)
            {
                return;
            }

            try
            {
                _metadata.Save();
            }
            catch (Exception ex)
            {
                toolStripStatusLabelSelected.Text = $"Metadaten konnten nicht gespeichert werden: {ex.Message}";
            }
        }

        private void CreateLibrary()
        {
            using (var dialog = new ArtLibraryNewForm())
            {
                dialog.LibraryDirectory = Directory.Exists(_settings.LastDirectory)
                    ? _settings.LastDirectory
                    : Options.OutputPath ?? string.Empty;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                ArtLibraryFile library;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    library = ArtLibraryFile.Create(dialog.LibraryDirectory, dialog.LibraryName, dialog.Capacity);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Die Bibliothek konnte nicht angelegt werden:\n{ex.Message}",
                        "Neue Art-Bibliothek", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

                SetLibrary(library);
            }
        }

        private void FillLibraryCombo()
        {
            _updatingLibraryCombo = true;
            try
            {
                toolStripComboBoxLibrary.Items.Clear();
                foreach (string path in _settings.RecentLibraries)
                {
                    toolStripComboBoxLibrary.Items.Add(new RecentLibraryItem(path));
                }

                toolStripComboBoxLibrary.SelectedIndex = -1;
                if (_library != null)
                {
                    for (int i = 0; i < toolStripComboBoxLibrary.Items.Count; ++i)
                    {
                        if (((RecentLibraryItem)toolStripComboBoxLibrary.Items[i]).Matches(_library.IdxPath))
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

        /// <summary>Entry of the library combo box (recent libraries).</summary>
        private sealed class RecentLibraryItem
        {
            public RecentLibraryItem(string path)
            {
                Path = path;
            }

            public string Path { get; }

            public bool Matches(string other) => string.Equals(
                ArtLibraryFile.GetBasePath(Path), ArtLibraryFile.GetBasePath(other), StringComparison.OrdinalIgnoreCase);

            public override string ToString()
            {
                string basePath = ArtLibraryFile.GetBasePath(Path);
                return $"{System.IO.Path.GetFileName(basePath)}  ({System.IO.Path.GetDirectoryName(basePath)})";
            }
        }

        #endregion

        #region [ Filter ]

        /// <summary>Entry of the category filter combo box.</summary>
        private sealed class CategoryFilterItem
        {
            public enum FilterKind
            {
                All,
                None,
                Category
            }

            public static readonly CategoryFilterItem All = new CategoryFilterItem(FilterKind.All, 0, "(Alle Kategorien)");
            public static readonly CategoryFilterItem None = new CategoryFilterItem(FilterKind.None, 0, "(Ohne Kategorie)");

            public CategoryFilterItem(FilterKind kind, int id, string text)
            {
                Kind = kind;
                Id = id;
                Text = text;
            }

            public FilterKind Kind { get; }
            public int Id { get; }
            public string Text { get; }

            public override string ToString() => Text;
        }

        /// <summary>Entry of the category combo box in the detail panel.</summary>
        private sealed class CategoryChoiceItem
        {
            public CategoryChoiceItem(int? id, string text)
            {
                Id = id;
                Text = text;
            }

            public int? Id { get; }
            public string Text { get; }

            public override string ToString() => Text;
        }

        private const string AllTagsText = "(Alle Tags)";

        private bool IsFilterActive =>
            !string.IsNullOrWhiteSpace(_searchText)
            || _categoryFilter.Kind != CategoryFilterItem.FilterKind.All
            || _tagFilter != null
            || _favoritesOnly;

        /// <summary>True when the tile view shows <see cref="_viewIds"/> instead of all ids.</summary>
        private bool IsListMode => _showOnlyOccupied || IsFilterActive;

        private bool PassesFilter(int id)
        {
            if (!IsFilterActive)
            {
                return true;
            }

            if (_favoritesOnly && ((ArtLibraryEntryFlags)_library.GetEntry(id).Flags & ArtLibraryEntryFlags.Favorite) == 0)
            {
                return false;
            }

            ArtLibraryItemInfo info = _metadata?.Get(id);

            switch (_categoryFilter.Kind)
            {
                case CategoryFilterItem.FilterKind.None when info?.CategoryId != null:
                    return false;
                case CategoryFilterItem.FilterKind.Category when info?.CategoryId == null || !_categoryFilterIds.Contains(info.CategoryId.Value):
                    return false;
            }

            if (_tagFilter != null && (info == null || !info.Tags.Any(t => string.Equals(t, _tagFilter, StringComparison.OrdinalIgnoreCase))))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                return _metadata != null ? _metadata.Matches(id, _searchText, _searchId) : _searchId == id;
            }

            return true;
        }

        private void ResetFilterState()
        {
            _searchText = string.Empty;
            _searchId = null;
            _categoryFilter = CategoryFilterItem.All;
            _categoryFilterIds = new HashSet<int>();
            _tagFilter = null;
            _favoritesOnly = false;

            _updatingFilter = true;
            toolStripTextBoxSearch.Text = string.Empty;
            toolStripButtonFavorites.Checked = false;
            _updatingFilter = false;
        }

        private void ClearFilters()
        {
            int keepId = _selectedId;
            ResetFilterState();
            FillFilterCombos();
            ReloadList();
            RestoreSelection(keepId);
        }

        private void FillFilterCombos()
        {
            _updatingFilter = true;
            try
            {
                toolStripComboBoxCategory.Items.Clear();
                toolStripComboBoxCategory.Items.Add(CategoryFilterItem.All);
                toolStripComboBoxCategory.Items.Add(CategoryFilterItem.None);

                CategoryFilterItem select = CategoryFilterItem.All;
                if (_metadata != null)
                {
                    foreach (ArtLibraryCategory category in _metadata.GetCategoriesSorted())
                    {
                        var item = new CategoryFilterItem(CategoryFilterItem.FilterKind.Category, category.Id, _metadata.GetCategoryPath(category.Id));
                        toolStripComboBoxCategory.Items.Add(item);
                        if (_categoryFilter.Kind == CategoryFilterItem.FilterKind.Category && _categoryFilter.Id == category.Id)
                        {
                            select = item;
                        }
                    }
                }

                if (_categoryFilter.Kind == CategoryFilterItem.FilterKind.None)
                {
                    select = CategoryFilterItem.None;
                }

                // category deleted meanwhile -> back to "all"
                if (_categoryFilter.Kind == CategoryFilterItem.FilterKind.Category && select == CategoryFilterItem.All)
                {
                    _categoryFilter = CategoryFilterItem.All;
                    _categoryFilterIds = new HashSet<int>();
                }

                toolStripComboBoxCategory.SelectedItem = select;
                if (_categoryFilter.Kind == CategoryFilterItem.FilterKind.Category && _metadata != null)
                {
                    _categoryFilterIds = _metadata.GetCategoryWithChildren(_categoryFilter.Id);
                }

                toolStripComboBoxTag.Items.Clear();
                toolStripComboBoxTag.Items.Add(AllTagsText);
                string selectTag = AllTagsText;
                if (_metadata != null)
                {
                    foreach (string tag in _metadata.Tags)
                    {
                        toolStripComboBoxTag.Items.Add(tag);
                        if (_tagFilter != null && string.Equals(tag, _tagFilter, StringComparison.OrdinalIgnoreCase))
                        {
                            selectTag = tag;
                        }
                    }
                }

                if (selectTag == AllTagsText)
                {
                    _tagFilter = null;
                }

                toolStripComboBoxTag.SelectedItem = selectTag;
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

            int keepId = _selectedId;
            ReloadList();
            RestoreSelection(keepId);
        }

        private void RunSearch()
        {
            _searchText = toolStripTextBoxSearch.Text.Trim();
            _searchId = ArtIdParser.TryParse(_searchText, out int id) ? id : (int?)null;
            ApplyFilterChange();
        }

        #endregion

        #region [ List / Index mapping ]

        private void ReloadList()
        {
            if (_library == null)
            {
                _viewIds = new List<int>();
                LibraryTileView.VirtualListSize = 0;
                LibraryTileView.Invalidate();
                UpdateAll();
                return;
            }

            if (IsListMode)
            {
                List<int> ids = _library.GetOccupiedIds();
                _viewIds = IsFilterActive ? ids.Where(PassesFilter).ToList() : ids;
                LibraryTileView.VirtualListSize = _viewIds.Count;
            }
            else
            {
                LibraryTileView.VirtualListSize = _library.Capacity;
            }

            LibraryTileView.Invalidate();
            UpdateAll();
        }

        private int GetIdAtIndex(int index)
        {
            if (_library == null || index < 0)
            {
                return -1;
            }

            if (IsListMode)
            {
                return index < _viewIds.Count ? _viewIds[index] : -1;
            }

            return index < _library.Capacity ? index : -1;
        }

        private int GetIndexOfId(int id)
        {
            if (_library == null || !_library.IsValidId(id))
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

        private void SetShowOnlyOccupied(bool value)
        {
            if (_showOnlyOccupied == value)
            {
                return;
            }

            int keepId = _selectedId;

            _showOnlyOccupied = value;
            toolStripButtonOnlyOccupied.Checked = value;
            _settings.ShowOnlyOccupied = value;

            ReloadList();
            RestoreSelection(keepId);
        }

        /// <summary>
        /// Focuses <paramref name="id"/> again after the list was rebuilt, or the nearest following id.
        /// </summary>
        private void RestoreSelection(int id)
        {
            if (_library == null || LibraryTileView.VirtualListSize == 0)
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

        /// <summary>
        /// Many changes at once (import, paste, transfer): list updates are done once at the end.
        /// </summary>
        private void BeginBatch()
        {
            ++_batchDepth;
            Cursor.Current = Cursors.WaitCursor;
        }

        private void EndBatch(int selectId = -1)
        {
            if (--_batchDepth > 0)
            {
                return;
            }

            _batchDepth = 0;
            Cursor.Current = Cursors.Default;

            int keepId = selectId >= 0 ? selectId : _selectedId;
            ReloadList();
            if (selectId >= 0)
            {
                SelectId(selectId);
            }
            else
            {
                RestoreSelection(keepId);
            }
        }

        #endregion

        #region [ Library events ]

        private void OnLibraryEntryChanged(object sender, ArtLibraryChangedEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnLibraryEntryChanged(sender, e)));
                return;
            }

            if (!ReferenceEquals(sender, _library) || _batchDepth > 0)
            {
                return;
            }

            if (e.IsAll)
            {
                int keepId = _selectedId;
                ReloadList();
                RestoreSelection(keepId);
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

        /// <summary>Adds/removes one id in list mode after it changed.</summary>
        private void UpdateViewMembership(int id)
        {
            bool shouldShow = _library.IsOccupied(id) && PassesFilter(id);
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

            int keepId = _selectedId;
            LibraryTileView.VirtualListSize = _viewIds.Count;
            RestoreSelection(keepId);
        }

        #endregion

        #region [ Selection / Detail / Status ]

        private void SetSelectedId(int id)
        {
            if (id != _selectedId)
            {
                ApplyDetailIfDirty();
            }

            _selectedId = id;
            UpdateDetail();
            UpdateStatus();
        }

        private void UpdateAll()
        {
            UpdateDetail();
            UpdateStatus();
            UpdateUiState();
        }

        private void UpdateUiState()
        {
            bool open = _library != null;
            toolStripTextBoxId.Enabled = open;
            toolStripButtonGoTo.Enabled = open;
            toolStripButtonReload.Enabled = open;
            toolStripButtonOnlyOccupied.Enabled = open;
            toolStripDropDownButtonOriginal.Enabled = open;
            toolStripButtonCopy.Enabled = open;
            toolStripButtonPaste.Enabled = open;
            libraryPropertiesToolStripMenuItem.Enabled = open;
            manageCategoriesToolStripMenuItem.Enabled = open;
            growToolStripMenuItem.Enabled = open;
            compactToolStripMenuItem.Enabled = open;
            openFolderToolStripMenuItem.Enabled = open;
            toolStripFilter.Enabled = open;
            tableLayoutPanelInfo.Enabled = open;
            LibraryTileView.Enabled = open;
            Text = open ? $"Art-Bibliothek - {_library.Name}" : "Art-Bibliothek";
        }

        private void FillDetailCategoryCombo()
        {
            bool updating = _updatingDetail;
            _updatingDetail = true;
            try
            {
                comboBoxCategory.Items.Clear();
                comboBoxCategory.Items.Add(new CategoryChoiceItem(null, "(keine)"));
                if (_metadata != null)
                {
                    foreach (ArtLibraryCategory category in _metadata.GetCategoriesSorted())
                    {
                        comboBoxCategory.Items.Add(new CategoryChoiceItem(category.Id, _metadata.GetCategoryPath(category.Id)));
                    }
                }

                comboBoxCategory.SelectedIndex = 0;
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
                bool occupied = _library != null && _selectedId >= 0 && _library.IsOccupied(_selectedId);

                if (_library == null || _selectedId < 0)
                {
                    pictureBoxPreview.Image = null;
                    richTextBoxDetail.Clear();
                    ClearDetailFields();
                    return;
                }

                Bitmap bitmap = occupied ? _library.GetBitmap(_selectedId) : null;
                pictureBoxPreview.BackColor = _backgroundColor;
                pictureBoxPreview.Image = bitmap;

                ArtLibraryItemInfo info = _metadata?.Get(_selectedId);
                ArtLibraryEntry entry = _library.GetEntry(_selectedId);

                textBoxName.Text = info?.Name ?? string.Empty;
                textBoxDescription.Text = info?.Description ?? string.Empty;
                textBoxTags.Text = info == null ? string.Empty : string.Join(", ", info.Tags);
                SelectDetailCategory(info?.CategoryId);
                checkBoxFavorite.Checked = ((ArtLibraryEntryFlags)entry.Flags & ArtLibraryEntryFlags.Favorite) != 0;

                textBoxName.Enabled = textBoxDescription.Enabled = textBoxTags.Enabled = comboBoxCategory.Enabled =
                    checkBoxFavorite.Enabled = buttonSaveInfo.Enabled = occupied;

                var text = new StringBuilder();
                text.AppendLine($"ID:          {_selectedId}");
                text.AppendLine($"Hex:         {ArtIdParser.ToHex(_selectedId)}");

                if (bitmap != null)
                {
                    text.AppendLine($"Größe:       {bitmap.Width} x {bitmap.Height}");
                    text.AppendLine($"Daten:       {entry.Length:N0} Bytes");
                    if (info?.SourceId != null)
                    {
                        text.AppendLine($"Herkunft:    Original {ArtIdParser.ToDisplay(info.SourceId.Value)}");
                    }

                    if (info != null)
                    {
                        text.AppendLine($"Hinzugefügt: {info.Added:g}");
                    }
                }
                else
                {
                    text.AppendLine("Status:      frei");
                }

                richTextBoxDetail.Text = text.ToString();
            }
            finally
            {
                _updatingDetail = false;
                _detailDirty = false;
            }
        }

        private void ClearDetailFields()
        {
            textBoxName.Text = string.Empty;
            textBoxDescription.Text = string.Empty;
            textBoxTags.Text = string.Empty;
            checkBoxFavorite.Checked = false;
            if (comboBoxCategory.Items.Count > 0)
            {
                comboBoxCategory.SelectedIndex = 0;
            }
        }

        private void SelectDetailCategory(int? categoryId)
        {
            for (int i = 0; i < comboBoxCategory.Items.Count; ++i)
            {
                if (((CategoryChoiceItem)comboBoxCategory.Items[i]).Id == categoryId)
                {
                    comboBoxCategory.SelectedIndex = i;
                    return;
                }
            }

            if (comboBoxCategory.Items.Count > 0)
            {
                comboBoxCategory.SelectedIndex = 0;
            }
        }

        /// <summary>Writes name/category/tags/description of the detail panel into the metadata.</summary>
        private void ApplyDetail()
        {
            _detailDirty = false;
            if (_library == null || _metadata == null || _selectedId < 0 || !_library.IsOccupied(_selectedId))
            {
                return;
            }

            ArtLibraryItemInfo info = _metadata.Get(_selectedId)?.Clone() ?? new ArtLibraryItemInfo();
            info.Name = textBoxName.Text.Trim();
            info.Description = textBoxDescription.Text.Trim();
            info.CategoryId = (comboBoxCategory.SelectedItem as CategoryChoiceItem)?.Id;
            info.Tags = ArtLibraryMetadata.ParseTags(textBoxTags.Text);

            int tagCount = _metadata.Tags.Count;
            _metadata.Set(_selectedId, info);

            if (_metadata.Tags.Count != tagCount)
            {
                FillFilterCombos();
            }

            if (IsListMode && IsFilterActive)
            {
                UpdateViewMembership(_selectedId);
            }
        }

        private void ApplyDetailIfDirty()
        {
            if (_detailDirty)
            {
                ApplyDetail();
            }
        }

        private void UpdateStatus()
        {
            if (_library == null)
            {
                toolStripStatusLabelLibrary.Text = "Keine Bibliothek geöffnet";
                toolStripStatusLabelCount.Text = string.Empty;
                toolStripStatusLabelView.Text = string.Empty;
                toolStripStatusLabelSelected.Text = string.Empty;
                return;
            }

            toolStripStatusLabelLibrary.Text = _library.Name;
            toolStripStatusLabelCount.Text = $"{_library.OccupiedCount:N0} belegt / {_library.Capacity:N0} IDs";
            toolStripStatusLabelView.Text = IsFilterActive ? $"Filter: {_viewIds.Count:N0} Treffer" : string.Empty;
            toolStripStatusLabelSelected.Text = _selectedId >= 0
                ? $"Auswahl: {ArtIdParser.ToDisplay(_selectedId)}"
                : string.Empty;
        }

        private void ShowStatusMessage(string message)
        {
            toolStripStatusLabelSelected.Text = message;
        }

        #endregion

        #region [ TileView ]

        private void ApplyTileViewOptions()
        {
            LibraryTileView.TileSize = new Size(Options.ArtItemSizeWidth, Options.ArtItemSizeHeight);
            LibraryTileView.TileBorderColor = Options.RemoveTileBorder ? Color.Transparent : Color.Gray;
            LibraryTileView.TileFocusColor = Options.TileFocusColor;
            LibraryTileView.TileHighlightColor = Options.TileSelectionColor;
        }

        private void LibraryTileView_DrawItem(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            int id = GetIdAtIndex(e.Index);
            if (id < 0)
            {
                return;
            }

            var itemPoint = new Point(e.Bounds.X + LibraryTileView.TilePadding.Left, e.Bounds.Y + LibraryTileView.TilePadding.Top);
            var rect = new Rectangle(itemPoint, LibraryTileView.TileSize);

            Region previousClip = e.Graphics.Clip;
            e.Graphics.Clip = new Region(rect);

            bool selected = LibraryTileView.SelectedIndices.Contains(e.Index);
            if (!selected)
            {
                e.Graphics.Clear(_backgroundColor);
            }

            Bitmap bitmap = _library.GetBitmap(id);
            if (bitmap == null)
            {
                // free slot: show the id in small gray text so the user can orientate while scrolling
                e.Graphics.DrawString(ArtIdParser.ToHex(id), _freeSlotFont, Brushes.LightGray, itemPoint);
            }
            else if (Options.ArtItemClip)
            {
                e.Graphics.DrawImage(bitmap, itemPoint);
            }
            else
            {
                int width = bitmap.Width;
                int height = bitmap.Height;
                if (width > LibraryTileView.TileSize.Width)
                {
                    width = LibraryTileView.TileSize.Width;
                    height = LibraryTileView.TileSize.Height * bitmap.Height / bitmap.Width;
                }

                if (height > LibraryTileView.TileSize.Height)
                {
                    height = LibraryTileView.TileSize.Height;
                    width = LibraryTileView.TileSize.Width * bitmap.Width / bitmap.Height;
                }

                e.Graphics.DrawImage(bitmap, new Rectangle(itemPoint, new Size(width, height)));
            }

            e.Graphics.Clip = previousClip;
        }

        private void LibraryTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            SetSelectedId(GetIdAtIndex(e.ItemIndex));
        }

        private void LibraryTileView_FocusSelectionChanged(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (!e.IsFocused || e.FocusedItemIndex < 0)
            {
                return;
            }

            SetSelectedId(GetIdAtIndex(e.FocusedItemIndex));
        }

        private void LibraryTileView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelected();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedToClipboard();
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                PasteFromClipboard(keepIds: false);
            }
            else if (e.Control && e.KeyCode == Keys.G)
            {
                toolStripTextBoxId.Focus();
                toolStripTextBoxId.SelectAll();
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                toolStripTextBoxSearch.Focus();
                toolStripTextBoxSearch.SelectAll();
            }
            else
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        /// <summary>
        /// Ids of all selected tiles (or the focused one when nothing is selected), ascending.
        /// </summary>
        private List<int> GetSelectedIds()
        {
            List<int> ids = LibraryTileView.SelectedIndices
                .Select(GetIdAtIndex)
                .Where(id => id >= 0)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            if (ids.Count == 0 && _selectedId >= 0)
            {
                ids.Add(_selectedId);
            }

            return ids;
        }

        private List<int> GetSelectedOccupiedIds()
        {
            return _library == null ? new List<int>() : GetSelectedIds().Where(_library.IsOccupied).ToList();
        }

        #endregion

        #region [ Clipboard ]

        private void CopySelectedToClipboard()
        {
            List<int> ids = GetSelectedOccupiedIds();
            if (ids.Count == 0)
            {
                return;
            }

            var items = ids.Select(id => new ArtClipboardItem
            {
                SourceId = id,
                Data = _library.GetRaw(id),
                Name = _metadata?.GetName(id) ?? string.Empty
            }).Where(item => item.Data != null).ToList();

            try
            {
                ArtLibraryClipboard.Copy(items);
                ShowStatusMessage(items.Count == 1
                    ? $"{ArtIdParser.ToDisplay(items[0].SourceId)} in die Zwischenablage kopiert"
                    : $"{items.Count} Grafiken in die Zwischenablage kopiert");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Zwischenablage nicht verfügbar:\n{ex.Message}", "Kopieren", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Pastes graphics: private format (several, lossless), image files from the Explorer or a normal image.
        /// <paramref name="keepIds"/>: graphics copied with ids are placed on the same ids.
        /// </summary>
        private void PasteFromClipboard(bool keepIds)
        {
            if (_library == null)
            {
                return;
            }

            List<ArtClipboardItem> items = ArtLibraryClipboard.GetItems();
            if (items.Count > 0)
            {
                PlaceItems(items.Select(i => (i.SourceId, i.Name, (Func<byte[]>)(() => i.Data))).ToList(), keepIds);
                return;
            }

            List<string> files = ArtLibraryClipboard.GetImageFiles();
            if (files.Count > 0)
            {
                ImportFiles(files);
                return;
            }

            Bitmap image = ArtLibraryClipboard.GetImage(_importOptions);
            if (image == null)
            {
                MessageBox.Show(this, "Die Zwischenablage enthält keine Grafik.", "Einfügen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (image)
            {
                PlaceSingleBitmap(image, string.Empty, -1);
            }
        }

        /// <summary>
        /// Places one bitmap on the selected id (or the next free one), asks before overwriting.
        /// </summary>
        private void PlaceSingleBitmap(Bitmap bitmap, string name, int sourceId)
        {
            int target = _selectedId >= 0 ? _selectedId : _library.FindNextFree(0);
            if (target < 0)
            {
                MessageBox.Show(this, "Die Bibliothek ist voll. Bitte erweitern.", "Einfügen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_library.IsOccupied(target) && !ConfirmOverwrite(target))
            {
                return;
            }

            try
            {
                _library.SetBitmap(target, bitmap);
                StoreImportedInfo(target, name, sourceId);
                SelectId(target);
                ShowStatusMessage($"Grafik auf {ArtIdParser.ToDisplay(target)} gespeichert");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Einfügen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ConfirmOverwrite(int id)
        {
            return MessageBox.Show(this, $"{ArtIdParser.ToDisplay(id)} ist bereits belegt. Überschreiben?", "Überschreiben",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Places encoded graphics. One graphic goes to the selected id; several go one after another from
        /// the selected id (occupied ids are skipped) or, with <paramref name="keepIds"/>, to their own ids.
        /// </summary>
        private void PlaceItems(List<(int SourceId, string Name, Func<byte[]> GetData)> items, bool keepIds)
        {
            if (items.Count == 1 && !keepIds)
            {
                byte[] data = items[0].GetData();
                int target = _selectedId >= 0 ? _selectedId : _library.FindNextFree(0);
                if (target < 0 || (_library.IsOccupied(target) && !ConfirmOverwrite(target)))
                {
                    return;
                }

                _library.SetRaw(target, data);
                StoreImportedInfo(target, items[0].Name, -1);
                SelectId(target);
                ShowStatusMessage($"Grafik auf {ArtIdParser.ToDisplay(target)} eingefügt");
                return;
            }

            bool overwrite = false;
            if (keepIds)
            {
                int occupied = items.Count(i => _library.IsValidId(i.SourceId) && _library.IsOccupied(i.SourceId));
                if (occupied > 0)
                {
                    DialogResult answer = MessageBox.Show(this,
                        $"{occupied} der Ziel-IDs sind bereits belegt.\n\nJa = überschreiben\nNein = diese überspringen",
                        "Einfügen", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (answer == DialogResult.Cancel)
                    {
                        return;
                    }

                    overwrite = answer == DialogResult.Yes;
                }
            }

            int start = _selectedId >= 0 ? _selectedId : 0;
            int[] targets = ArtLibraryTransfer.AssignTargetIds(items.Select(i => i.SourceId).ToList(), start, keepIds, overwrite,
                _library.IsOccupied, _library.MaxId);

            int done = 0;
            int first = -1;
            BeginBatch();
            try
            {
                for (int i = 0; i < items.Count; ++i)
                {
                    if (targets[i] < 0)
                    {
                        continue;
                    }

                    _library.SetRaw(targets[i], items[i].GetData());
                    StoreImportedInfo(targets[i], items[i].Name, -1);
                    first = first < 0 ? targets[i] : Math.Min(first, targets[i]);
                    ++done;
                }
            }
            finally
            {
                EndBatch(first);
            }

            ShowStatusMessage($"{done} von {items.Count} Grafiken eingefügt" + (done < items.Count ? " (Rest übersprungen)" : string.Empty));
        }

        /// <summary>Remembers name/source of a newly stored graphic (only fills empty fields).</summary>
        private void StoreImportedInfo(int id, string name, int sourceId)
        {
            if (_metadata == null)
            {
                return;
            }

            ArtLibraryItemInfo info = _metadata.GetOrCreate(id);
            info.Added = DateTime.Now;
            if (string.IsNullOrWhiteSpace(info.Name) && !string.IsNullOrWhiteSpace(name))
            {
                info.Name = name.Trim();
            }

            if (sourceId >= 0)
            {
                info.SourceId = sourceId;
            }

            _metadata.MarkDirty();
        }

        #endregion

        #region [ Import / Export files ]

        private void ImportFilesWithDialog()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Bilder in die Bibliothek importieren";
                dialog.Filter = "Bilder (*.bmp;*.png;*.tif;*.tiff;*.gif;*.jpg)|*.bmp;*.png;*.tif;*.tiff;*.gif;*.jpg;*.jpeg|Alle Dateien (*.*)|*.*";
                dialog.Multiselect = true;
                if (dialog.ShowDialog(this) == DialogResult.OK)
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
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<string> files = Directory.GetFiles(dialog.SelectedPath)
                    .Where(ArtLibraryImageConverter.IsSupportedFile)
                    .ToList();

                if (files.Count == 0)
                {
                    MessageBox.Show(this, "Im Ordner wurden keine Bilder gefunden.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ImportFiles(files);
            }
        }

        /// <summary>
        /// Imports image files after asking for the target (fixed start id or id from the file name).
        /// </summary>
        private void ImportFiles(List<string> files)
        {
            if (_library == null || files.Count == 0)
            {
                return;
            }

            // sort by id in the file name, then by name, so "fortlaufend" keeps a sensible order
            files = files
                .OrderBy(f => ArtLibraryTransfer.ParseIdFromFileName(f) < 0 ? int.MaxValue : ArtLibraryTransfer.ParseIdFromFileName(f))
                .ThenBy(f => f, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = "Bilder importieren";
                dialog.InfoText = files.Count == 1
                    ? $"Datei: {Path.GetFileName(files[0])}"
                    : $"{files.Count} Dateien werden in die Bibliothek importiert.";
                dialog.ShowSource = false;
                dialog.KeepIdsText = "ID aus dem Dateinamen verwenden (z. B. \"Item 0x04D2.bmp\")";
                dialog.MaxTargetId = _library.MaxId;
                dialog.TargetId = _selectedId >= 0 ? _selectedId : Math.Max(0, _library.FindNextFree(0));
                dialog.ImportOptions = _importOptions;
                dialog.KeepIds = files.Count > 1 && files.All(f => ArtLibraryTransfer.ParseIdFromFileName(f) >= 0);
                dialog.Overwrite = files.Count == 1;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                ArtImportOptions options = dialog.ImportOptions;
                _importOptions.BlackTransparent = options.BlackTransparent;
                _importOptions.WhiteTransparent = options.WhiteTransparent;
                _importOptions.LightGrayTransparent = options.LightGrayTransparent;

                List<int> sourceIds = files.Select(ArtLibraryTransfer.ParseIdFromFileName).ToList();
                int[] targets = ArtLibraryTransfer.AssignTargetIds(sourceIds, dialog.TargetId, dialog.KeepIds, dialog.Overwrite,
                    _library.IsOccupied, _library.MaxId);

                var errors = new List<string>();
                int done = 0;
                int first = -1;

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
                            using (Bitmap bitmap = ArtLibraryImageConverter.LoadFileForArt(files[i], options))
                            {
                                _library.SetBitmap(targets[i], bitmap);
                            }

                            // a descriptive file name becomes the name; "Item 0x04D2" style names do not
                            string name = sourceIds[i] < 0 ? Path.GetFileNameWithoutExtension(files[i]) : string.Empty;
                            StoreImportedInfo(targets[i], name, -1);
                            first = first < 0 ? targets[i] : Math.Min(first, targets[i]);
                            ++done;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{Path.GetFileName(files[i])}: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    EndBatch(first);
                }

                ReportResult("Import", done, files.Count, errors);
            }
        }

        private void ExportSelected(ImageFormat format, string extension, bool all)
        {
            if (_library == null)
            {
                return;
            }

            List<int> ids = all ? _library.GetOccupiedIds() : GetSelectedOccupiedIds();
            if (ids.Count == 0)
            {
                MessageBox.Show(this, "Keine Grafik ausgewählt.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = $"Zielordner für {ids.Count} Grafik(en) wählen";
                dialog.UseDescriptionForTitle = true;
                if (Directory.Exists(Options.OutputPath))
                {
                    dialog.SelectedPath = Options.OutputPath;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var errors = new List<string>();
                int done = 0;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    foreach (int id in ids)
                    {
                        try
                        {
                            Bitmap art = _library.GetBitmap(id);
                            if (art == null)
                            {
                                continue;
                            }

                            using (Bitmap export = ArtLibraryImageConverter.ToExportBitmap(art))
                            {
                                export.Save(Path.Combine(dialog.SelectedPath, ArtLibraryTransfer.GetExportFileName(id, extension)), format);
                            }

                            ++done;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{ArtIdParser.ToHex(id)}: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

                ReportResult("Export", done, ids.Count, errors, $"Ordner: {dialog.SelectedPath}");
            }
        }

        private void ReportResult(string title, int done, int total, List<string> errors, string extra = null)
        {
            var text = new StringBuilder();
            text.AppendLine($"{done} von {total} Grafiken verarbeitet.");
            if (done < total && errors.Count == 0)
            {
                text.AppendLine("Übersprungen wurden belegte oder ungültige Ziel-IDs.");
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

            MessageBox.Show(this, text.ToString(), title, MessageBoxButtons.OK,
                errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        #endregion

        #region [ Original art.mul ]

        private bool EnsureOriginal()
        {
            if (ArtLibraryTransfer.IsOriginalAvailable)
            {
                return true;
            }

            MessageBox.Show(this, "Es ist keine originale art.mul geladen.\nBitte zuerst den Items-Tab öffnen.",
                "Original", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        /// <summary>
        /// Copies (or moves) the selected library graphics into the original art.mul.
        /// </summary>
        private void TransferToOriginal(bool move)
        {
            if (_library == null || !EnsureOriginal())
            {
                return;
            }

            List<int> ids = GetSelectedOccupiedIds();
            if (ids.Count == 0)
            {
                MessageBox.Show(this, "Bitte zuerst belegte Grafiken auswählen.", "Original", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int maxOriginal = ArtLibraryTransfer.OriginalMaxId;
            string action = move ? "verschieben" : "kopieren";

            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = $"Ins Original {action}";
                dialog.InfoText = $"{ids.Count} Grafik(en) in die originale art.mul {action} (0 bis {ArtIdParser.ToHex(maxOriginal)}).\n" +
                                  "Die Änderung muss danach im Items-Tab gespeichert werden.";
                dialog.ShowSource = false;
                dialog.ShowTransparencyOptions = false;
                dialog.KeepIdsText = "Gleiche IDs wie in der Bibliothek";
                dialog.MaxTargetId = maxOriginal;

                // prefer the original id the graphic came from
                int firstSource = _metadata?.Get(ids[0])?.SourceId ?? ids[0];
                dialog.TargetId = Math.Min(firstSource, maxOriginal);
                dialog.KeepIds = ids.Count > 1 && ids.All(id => id <= maxOriginal);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                bool overwrite = dialog.Overwrite;
                int[] targets = ArtLibraryTransfer.AssignTargetIds(ids, dialog.TargetId, dialog.KeepIds, overwrite,
                    ArtLibraryTransfer.IsOriginalOccupied, maxOriginal);

                // one graphic on an explicit id: use exactly that id and ask before overwriting
                if (ids.Count == 1 && !dialog.KeepIds)
                {
                    int target = dialog.TargetId;
                    if (ArtLibraryTransfer.IsOriginalOccupied(target) && !overwrite && !ConfirmOverwriteOriginal(target))
                    {
                        return;
                    }

                    targets[0] = target;
                }

                var errors = new List<string>();
                int done = 0;
                int firstTarget = -1;

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
                            ArtLibraryTransfer.WriteToOriginal(targets[i], _library.GetBitmap(ids[i]), this);
                            if (move)
                            {
                                _library.Remove(ids[i]);
                                _metadata?.Remove(ids[i]);
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

                if (firstTarget >= 0 && ItemsControl.RefMarker != null && ItemsControl.RefMarker.IsLoaded)
                {
                    ItemsControl.SearchGraphic(firstTarget);
                }

                ReportResult($"Ins Original {action}", done, ids.Count, errors,
                    done > 0 ? "Nicht vergessen: im Items-Tab speichern (art.mul)." : null);
            }
        }

        private bool ConfirmOverwriteOriginal(int id)
        {
            return MessageBox.Show(this, $"Im Original ist {ArtIdParser.ToDisplay(id)} bereits belegt. Überschreiben?",
                "Original", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Copies graphics from the original art.mul into the library. <paramref name="presetIds"/> fills the
        /// source field (e.g. the selection of the items tab).
        /// </summary>
        private void FetchFromOriginal(List<int> presetIds)
        {
            if (_library == null || !EnsureOriginal())
            {
                return;
            }

            int maxOriginal = ArtLibraryTransfer.OriginalMaxId;

            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = "Aus dem Original holen";
                dialog.InfoText = "Grafiken aus der originalen art.mul in die Bibliothek kopieren.\n" +
                                  "Quell-IDs: einzelne IDs und Bereiche, z. B. 0x1000-0x10FF, 1234";
                dialog.SourceLabel = "Original-IDs:";
                dialog.SourceText = presetIds == null || presetIds.Count == 0 ? string.Empty : ToRangeText(presetIds);
                dialog.MaxSourceId = maxOriginal;
                dialog.ShowTransparencyOptions = false;
                dialog.KeepIdsText = "Gleiche IDs wie im Original";
                dialog.MaxTargetId = _library.MaxId;
                dialog.TargetId = _selectedId >= 0 && !_library.IsOccupied(_selectedId)
                    ? _selectedId
                    : Math.Max(0, _library.FindNextFree(_selectedId >= 0 ? _selectedId : 0));

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<int> sources = dialog.SourceIds.Where(ArtLibraryTransfer.IsOriginalOccupied).ToList();
                int empty = dialog.SourceIds.Count - sources.Count;
                if (sources.Count == 0)
                {
                    MessageBox.Show(this, "Die angegebenen Original-IDs sind alle leer.", "Aus dem Original holen",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int[] targets = ArtLibraryTransfer.AssignTargetIds(sources, dialog.TargetId, dialog.KeepIds, dialog.Overwrite,
                    _library.IsOccupied, _library.MaxId);

                var errors = new List<string>();
                int done = 0;
                int first = -1;

                BeginBatch();
                try
                {
                    for (int i = 0; i < sources.Count; ++i)
                    {
                        if (targets[i] < 0)
                        {
                            continue;
                        }

                        try
                        {
                            if (ArtLibraryTransfer.ImportFromOriginal(_library, _metadata, sources[i], targets[i]))
                            {
                                first = first < 0 ? targets[i] : Math.Min(first, targets[i]);
                                ++done;
                            }
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

                ReportResult("Aus dem Original holen", done, sources.Count, errors,
                    empty > 0 ? $"{empty} leere Original-IDs wurden ignoriert." : null);
            }
        }

        /// <summary>Selection of the items tab (original art.mul).</summary>
        private void FetchItemsTabSelection()
        {
            ItemsControl items = ItemsControl.RefMarker;
            if (items == null || !items.IsLoaded)
            {
                MessageBox.Show(this, "Der Items-Tab ist noch nicht geladen.", "Original", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var ids = new List<int>();
            IReadOnlyList<int> list = items.ItemList;
            foreach (int index in ItemsControl.TileView.SelectedIndices)
            {
                if (index >= 0 && index < list.Count)
                {
                    ids.Add(list[index]);
                }
            }

            if (ids.Count == 0 && items.SelectedGraphicId >= 0)
            {
                ids.Add(items.SelectedGraphicId);
            }

            if (ids.Count == 0)
            {
                MessageBox.Show(this, "Im Items-Tab ist nichts ausgewählt.", "Original", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FetchFromOriginal(ids.Distinct().OrderBy(id => id).ToList());
        }

        /// <summary>"0x10-0x1F, 0x30" for a list of ids.</summary>
        private static string ToRangeText(List<int> ids)
        {
            var parts = new List<string>();
            List<int> sorted = ids.Distinct().OrderBy(id => id).ToList();
            int i = 0;
            while (i < sorted.Count)
            {
                int start = sorted[i];
                int end = start;
                while (i + 1 < sorted.Count && sorted[i + 1] == end + 1)
                {
                    ++i;
                    end = sorted[i];
                }

                parts.Add(start == end ? ArtIdParser.ToHex(start) : $"{ArtIdParser.ToHex(start)}-{ArtIdParser.ToHex(end)}");
                ++i;
            }

            return string.Join(", ", parts);
        }

        #endregion

        #region [ Inside the library ]

        private void RemoveSelected()
        {
            if (_library == null)
            {
                return;
            }

            List<int> ids = GetSelectedOccupiedIds();
            if (ids.Count == 0)
            {
                return;
            }

            string question = ids.Count == 1
                ? $"Grafik {ArtIdParser.ToDisplay(ids[0])} aus der Bibliothek entfernen?"
                : $"{ids.Count} Grafiken aus der Bibliothek entfernen?";

            if (MessageBox.Show(this, question, "Entfernen", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            BeginBatch();
            try
            {
                foreach (int id in ids)
                {
                    _library.Remove(id);
                    _metadata?.Remove(id);
                }
            }
            finally
            {
                EndBatch();
            }
        }

        /// <summary>Copies or moves the selection to another id inside the library.</summary>
        private void CopyOrMoveInside(bool move)
        {
            List<int> ids = GetSelectedOccupiedIds();
            if (_library == null || ids.Count == 0)
            {
                return;
            }

            string action = move ? "verschieben" : "kopieren";
            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = $"Innerhalb der Bibliothek {action}";
                dialog.InfoText = $"{ids.Count} Grafik(en) fortlaufend ab der Ziel-ID {action}.";
                dialog.ShowSource = false;
                dialog.ShowModeOptions = false;
                dialog.ShowTransparencyOptions = false;
                dialog.MaxTargetId = _library.MaxId;
                dialog.TargetId = _library.FindNextFree(ids[ids.Count - 1]);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                int[] targets = ArtLibraryTransfer.AssignTargetIds(ids, dialog.TargetId, false, dialog.Overwrite,
                    _library.IsOccupied, _library.MaxId);

                if (ids.Count == 1 && targets[0] != dialog.TargetId)
                {
                    if (_library.IsOccupied(dialog.TargetId) && !ConfirmOverwrite(dialog.TargetId))
                    {
                        return;
                    }

                    targets[0] = dialog.TargetId;
                }

                // read everything first: targets may overlap the sources
                var data = ids.Select(id => _library.GetRaw(id)).ToList();
                var infos = ids.Select(id => _metadata?.Get(id)?.Clone()).ToList();

                int done = 0;
                int first = -1;
                BeginBatch();
                try
                {
                    if (move)
                    {
                        var targetSet = new HashSet<int>(targets.Where(t => t >= 0));
                        for (int i = 0; i < ids.Count; ++i)
                        {
                            if (targets[i] >= 0 && targets[i] != ids[i] && !targetSet.Contains(ids[i]))
                            {
                                _library.Remove(ids[i]);
                                _metadata?.Remove(ids[i]);
                            }
                        }
                    }

                    for (int i = 0; i < ids.Count; ++i)
                    {
                        if (targets[i] < 0 || data[i] == null || targets[i] == ids[i])
                        {
                            continue;
                        }

                        _library.SetRaw(targets[i], data[i]);
                        if (_metadata != null)
                        {
                            if (infos[i] != null)
                            {
                                _metadata.Set(targets[i], infos[i].Clone());
                            }
                            else
                            {
                                _metadata.Remove(targets[i]);
                            }
                        }

                        first = first < 0 ? targets[i] : Math.Min(first, targets[i]);
                        ++done;
                    }
                }
                finally
                {
                    EndBatch(first);
                }

                ShowStatusMessage($"{done} Grafik(en) {(move ? "verschoben" : "kopiert")}");
            }
        }

        private void SwapSelected()
        {
            List<int> ids = GetSelectedIds();
            if (_library == null || ids.Count != 2)
            {
                return;
            }

            _library.Swap(ids[0], ids[1]);
            _metadata?.Swap(ids[0], ids[1]);
            UpdateDetail();
            ShowStatusMessage($"{ArtIdParser.ToHex(ids[0])} und {ArtIdParser.ToHex(ids[1])} getauscht");
        }

        private void FindNextFree()
        {
            if (_library == null)
            {
                return;
            }

            int id = _library.FindNextFree(Math.Max(0, _selectedId));
            if (id < 0)
            {
                MessageBox.Show(this, "Ab der Auswahl gibt es keine freie ID mehr.", "Nächste freie ID",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SelectId(id);
        }

        private void GoToIdFromTextBox()
        {
            if (_library == null)
            {
                return;
            }

            if (!ArtIdParser.TryParse(toolStripTextBoxId.Text, out int id) || !_library.IsValidId(id))
            {
                MessageBox.Show(this,
                    $"Ungültige ID. Erlaubt sind 0 bis {_library.MaxId:N0} ({ArtIdParser.ToHex(_library.MaxId)}),\n" +
                    "dezimal (1234) oder hexadezimal (0x4D2, 4D2h).",
                    "Gehe zu ID", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SelectId(id);
            LibraryTileView.Focus();
        }

        private void ToggleFavorite()
        {
            List<int> ids = GetSelectedOccupiedIds();
            if (_library == null || ids.Count == 0)
            {
                return;
            }

            bool makeFavorite = ((ArtLibraryEntryFlags)_library.GetEntry(ids[0]).Flags & ArtLibraryEntryFlags.Favorite) == 0;
            BeginBatch();
            try
            {
                foreach (int id in ids)
                {
                    var flags = (ArtLibraryEntryFlags)_library.GetEntry(id).Flags;
                    flags = makeFavorite ? flags | ArtLibraryEntryFlags.Favorite : flags & ~ArtLibraryEntryFlags.Favorite;
                    _library.SetFlags(id, flags);
                }
            }
            finally
            {
                EndBatch();
            }
        }

        private void AssignCategory(int? categoryId)
        {
            List<int> ids = GetSelectedOccupiedIds();
            if (_metadata == null || ids.Count == 0)
            {
                return;
            }

            foreach (int id in ids)
            {
                _metadata.GetOrCreate(id).CategoryId = categoryId;
            }

            _metadata.MarkDirty();
            AfterMetadataChangedForSelection();
        }

        private void AddTagToSelection(string tag)
        {
            List<int> ids = GetSelectedOccupiedIds();
            if (_metadata == null || ids.Count == 0)
            {
                return;
            }

            foreach (int id in ids)
            {
                ArtLibraryItemInfo info = _metadata.GetOrCreate(id);
                info.Tags = ArtLibraryMetadata.NormalizeTags(info.Tags.Concat(new[] { tag }));
            }

            _metadata.AddTag(tag);
            _metadata.MarkDirty();
            AfterMetadataChangedForSelection();
        }

        private void RemoveTagFromSelection(string tag)
        {
            List<int> ids = GetSelectedOccupiedIds();
            if (_metadata == null || ids.Count == 0)
            {
                return;
            }

            foreach (int id in ids)
            {
                _metadata.Get(id)?.Tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
            }

            _metadata.MarkDirty();
            AfterMetadataChangedForSelection();
        }

        private void AfterMetadataChangedForSelection()
        {
            if (IsFilterActive)
            {
                int keepId = _selectedId;
                ReloadList();
                RestoreSelection(keepId);
            }
            else
            {
                UpdateDetail();
            }
        }

        #endregion

        #region [ Library menu ]

        private void ShowProperties()
        {
            if (_library == null)
            {
                return;
            }

            long unused = _library.GetUnusedBytes();
            string text =
                $"Name:          {_library.Name}\n" +
                $"Datei:         {_library.IdxPath}\n" +
                $"Erstellt:      {_library.Created.ToLocalTime():g}\n" +
                $"IDs:           {_library.Capacity:N0} (0 bis {ArtIdParser.ToHex(_library.MaxId)})\n" +
                $"Belegt:        {_library.OccupiedCount:N0}\n" +
                $"Datendatei:    {_library.MulLength / (1024.0 * 1024.0):N2} MB\n" +
                $"Ungenutzt:     {unused / (1024.0 * 1024.0):N2} MB (mit \"Komprimieren\" freigeben)\n" +
                $"Kategorien:    {_metadata?.Categories.Count ?? 0}\n" +
                $"Tags:          {_metadata?.Tags.Count ?? 0}";

            MessageBox.Show(this, text, "Eigenschaften der Bibliothek", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ManageCategories()
        {
            if (_metadata == null)
            {
                return;
            }

            ApplyDetailIfDirty();
            using (var dialog = new ArtLibraryCategoryForm(_metadata))
            {
                dialog.ShowDialog(this);
                if (!dialog.Changed)
                {
                    return;
                }
            }

            SaveMetadata();
            FillFilterCombos();
            FillDetailCategoryCombo();
            int keepId = _selectedId;
            ReloadList();
            RestoreSelection(keepId);
        }

        private void GrowLibrary()
        {
            if (_library == null)
            {
                return;
            }

            using (var dialog = new ArtLibraryGrowForm())
            {
                dialog.CurrentCapacity = _library.Capacity;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    _library.Grow(dialog.NewCapacity);
                    ShowStatusMessage($"Bibliothek auf {_library.Capacity:N0} IDs erweitert");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Erweitern", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void CompactLibrary()
        {
            if (_library == null)
            {
                return;
            }

            long unused = _library.GetUnusedBytes();
            if (MessageBox.Show(this,
                    $"Ungenutzter Platz: {unused / (1024.0 * 1024.0):N2} MB.\n\nDie Datendatei wird neu geschrieben. Fortfahren?",
                    "Komprimieren", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                long saved = _library.Compact();
                MessageBox.Show(this, $"Fertig. {saved / (1024.0 * 1024.0):N2} MB freigegeben.", "Komprimieren",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Komprimieren", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        #region [ Event handlers: toolbar ]

        private void ToolStripButtonNew_Click(object sender, EventArgs e)
        {
            CreateLibrary();
        }

        private void ToolStripButtonOpen_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Art-Bibliothek öffnen";
                dialog.Filter = "Art-Bibliothek (*.artlib.idx)|*.artlib.idx|Alle Dateien (*.*)|*.*";
                dialog.CheckFileExists = true;
                if (Directory.Exists(_settings.LastDirectory))
                {
                    dialog.InitialDirectory = _settings.LastDirectory;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    OpenLibrary(dialog.FileName);
                }
            }
        }

        private void ToolStripButtonReload_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            // the files are opened exclusively for writing, so the current instance must be closed first
            int keepId = _selectedId;
            string path = _library.IdxPath;
            CloseLibrary();
            OpenLibrary(path);
            RestoreSelection(keepId);
        }

        private void ToolStripComboBoxLibrary_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingLibraryCombo || !(toolStripComboBoxLibrary.SelectedItem is RecentLibraryItem item))
            {
                return;
            }

            if (_library != null && item.Matches(_library.IdxPath))
            {
                return;
            }

            OpenLibrary(item.Path);
        }

        private void ToolStripTextBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            GoToIdFromTextBox();
        }

        private void ToolStripButtonGoTo_Click(object sender, EventArgs e)
        {
            GoToIdFromTextBox();
        }

        private void ToolStripButtonOnlyOccupied_Click(object sender, EventArgs e)
        {
            SetShowOnlyOccupied(toolStripButtonOnlyOccupied.Checked);
        }

        private void ToolStripButtonCopy_Click(object sender, EventArgs e)
        {
            CopySelectedToClipboard();
        }

        private void ToolStripButtonPaste_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(keepIds: false);
        }

        private void LibraryPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowProperties();
        }

        private void ManageCategoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ManageCategories();
        }

        private void GrowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GrowLibrary();
        }

        private void CompactToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CompactLibrary();
        }

        private void OpenFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            try
            {
                Process.Start("explorer.exe", $"/select,\"{_library.IdxPath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ordner öffnen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveFromRecentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(toolStripComboBoxLibrary.SelectedItem is RecentLibraryItem item))
            {
                return;
            }

            _settings.RemoveRecent(item.Path);
            _settings.Save();
            FillLibraryCombo();
        }

        private void OriginalFetchSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FetchItemsTabSelection();
        }

        private void OriginalFetchIdsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FetchFromOriginal(null);
        }

        private void OriginalCopyToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TransferToOriginal(move: false);
        }

        private void OriginalMoveToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TransferToOriginal(move: true);
        }

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

        private void ToolStripButtonSearch_Click(object sender, EventArgs e)
        {
            RunSearch();
        }

        private void ToolStripButtonClearFilter_Click(object sender, EventArgs e)
        {
            ClearFilters();
        }

        private void ToolStripComboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingFilter)
            {
                return;
            }

            _categoryFilter = toolStripComboBoxCategory.SelectedItem as CategoryFilterItem ?? CategoryFilterItem.All;
            _categoryFilterIds = _categoryFilter.Kind == CategoryFilterItem.FilterKind.Category && _metadata != null
                ? _metadata.GetCategoryWithChildren(_categoryFilter.Id)
                : new HashSet<int>();
            ApplyFilterChange();
        }

        private void ToolStripComboBoxTag_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingFilter)
            {
                return;
            }

            string tag = toolStripComboBoxTag.SelectedItem as string;
            _tagFilter = tag == null || tag == AllTagsText ? null : tag;
            ApplyFilterChange();
        }

        private void ToolStripButtonFavorites_Click(object sender, EventArgs e)
        {
            if (_updatingFilter)
            {
                return;
            }

            _favoritesOnly = toolStripButtonFavorites.Checked;
            ApplyFilterChange();
        }

        #endregion

        #region [ Event handlers: context menu ]

        private void ContextMenuStripTileView_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool open = _library != null;
            List<int> occupiedIds = open ? GetSelectedOccupiedIds() : new List<int>();
            bool anyOccupied = occupiedIds.Count > 0;
            bool canPaste = open && ArtLibraryClipboard.ContainsArt();

            pasteToolStripMenuItem.Enabled = canPaste;
            pasteKeepIdsToolStripMenuItem.Enabled = canPaste;
            copyToolStripMenuItem.Enabled = anyOccupied;
            importFilesToolStripMenuItem.Enabled = open;
            importFolderToolStripMenuItem.Enabled = open;
            exportToolStripMenuItem.Enabled = open;
            originalToolStripMenuItem.Enabled = open;
            ctxOriginalCopyToToolStripMenuItem.Enabled = anyOccupied;
            ctxOriginalMoveToToolStripMenuItem.Enabled = anyOccupied;
            copyToIdToolStripMenuItem.Enabled = anyOccupied;
            moveToIdToolStripMenuItem.Enabled = anyOccupied;
            swapToolStripMenuItem.Enabled = open && GetSelectedIds().Count == 2;
            favoriteToolStripMenuItem.Enabled = anyOccupied;
            removeToolStripMenuItem.Enabled = anyOccupied;
            findNextFreeToolStripMenuItem.Enabled = open;
            copyIdHexToolStripMenuItem.Enabled = open && _selectedId >= 0;
            copyIdDecToolStripMenuItem.Enabled = open && _selectedId >= 0;

            BuildCategoryMenu(anyOccupied);
            BuildTagMenus(occupiedIds);
        }

        private void BuildCategoryMenu(bool enabled)
        {
            assignCategoryToolStripMenuItem.DropDownItems.Clear();
            assignCategoryToolStripMenuItem.Enabled = enabled && _metadata != null;
            if (_metadata == null)
            {
                return;
            }

            var none = new ToolStripMenuItem("(keine)");
            none.Click += (s, args) => AssignCategory(null);
            assignCategoryToolStripMenuItem.DropDownItems.Add(none);

            foreach (ArtLibraryCategory category in _metadata.GetCategoriesSorted())
            {
                int id = category.Id;
                var item = new ToolStripMenuItem(_metadata.GetCategoryPath(id));
                item.Click += (s, args) => AssignCategory(id);
                assignCategoryToolStripMenuItem.DropDownItems.Add(item);
            }

            assignCategoryToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            var manage = new ToolStripMenuItem("Kategorien verwalten...");
            manage.Click += (s, args) => ManageCategories();
            assignCategoryToolStripMenuItem.DropDownItems.Add(manage);
        }

        private void BuildTagMenus(List<int> ids)
        {
            addTagToolStripMenuItem.DropDownItems.Clear();
            removeTagToolStripMenuItem.DropDownItems.Clear();
            addTagToolStripMenuItem.Enabled = ids.Count > 0 && _metadata != null && _metadata.Tags.Count > 0;

            if (_metadata == null)
            {
                removeTagToolStripMenuItem.Enabled = false;
                return;
            }

            foreach (string tag in _metadata.Tags)
            {
                string value = tag;
                var item = new ToolStripMenuItem(value);
                item.Click += (s, args) => AddTagToSelection(value);
                addTagToolStripMenuItem.DropDownItems.Add(item);
            }

            List<string> used = ids
                .Select(id => _metadata.Get(id))
                .Where(info => info != null)
                .SelectMany(info => info.Tags)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            foreach (string tag in used)
            {
                string value = tag;
                var item = new ToolStripMenuItem(value);
                item.Click += (s, args) => RemoveTagFromSelection(value);
                removeTagToolStripMenuItem.DropDownItems.Add(item);
            }

            removeTagToolStripMenuItem.Enabled = used.Count > 0;
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(keepIds: false);
        }

        private void PasteKeepIdsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(keepIds: true);
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopySelectedToClipboard();
        }

        private void ImportFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportFilesWithDialog();
        }

        private void ImportFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportFolderWithDialog();
        }

        private void ExportBmpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Bmp, "bmp", all: false);
        }

        private void ExportPngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Png, "png", all: false);
        }

        private void ExportTiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Tiff, "tiff", all: false);
        }

        private void ExportAllBmpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Bmp, "bmp", all: true);
        }

        private void ExportAllPngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Png, "png", all: true);
        }

        private void CopyToIdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyOrMoveInside(move: false);
        }

        private void MoveToIdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyOrMoveInside(move: true);
        }

        private void SwapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwapSelected();
        }

        private void FavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleFavorite();
        }

        private void RemoveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveSelected();
        }

        private void FindNextFreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindNextFree();
        }

        private void CopyIdHexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedId >= 0)
            {
                Clipboard.SetText(ArtIdParser.ToHex(_selectedId));
            }
        }

        private void CopyIdDecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedId >= 0)
            {
                Clipboard.SetText(_selectedId.ToString());
            }
        }

        private void BackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog.Color = _backgroundColor;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _backgroundColor = colorDialog.Color;
            LibraryTileView.BackColor = _backgroundColor;
            LibraryTileView.Invalidate();
            UpdateDetail();
        }

        #endregion

        #region [ Event handlers: detail panel ]

        private void DetailField_Changed(object sender, EventArgs e)
        {
            if (!_updatingDetail)
            {
                _detailDirty = true;
            }
        }

        private void ButtonSaveInfo_Click(object sender, EventArgs e)
        {
            ApplyDetail();
            SaveMetadata();
            ShowStatusMessage($"Angaben zu {ArtIdParser.ToDisplay(_selectedId)} gespeichert");
        }

        private void TextBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ButtonSaveInfo_Click(sender, e);
            }
        }

        private void CheckBoxFavorite_CheckedChanged(object sender, EventArgs e)
        {
            if (_updatingDetail || _library == null || _selectedId < 0 || !_library.IsOccupied(_selectedId))
            {
                return;
            }

            var flags = (ArtLibraryEntryFlags)_library.GetEntry(_selectedId).Flags;
            flags = checkBoxFavorite.Checked ? flags | ArtLibraryEntryFlags.Favorite : flags & ~ArtLibraryEntryFlags.Favorite;
            _library.SetFlags(_selectedId, flags);
        }

        #endregion

        #region [ Drag & drop on the preview ]

        private void PictureBoxPreview_DragEnter(object sender, DragEventArgs e)
        {
            bool files = e.Data.GetDataPresent(DataFormats.FileDrop)
                         && e.Data.GetData(DataFormats.FileDrop) is string[] paths
                         && paths.Any(ArtLibraryImageConverter.IsSupportedFile);

            e.Effect = _library != null && (files || e.Data.GetDataPresent(DataFormats.Bitmap))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void PictureBoxPreview_DragDrop(object sender, DragEventArgs e)
        {
            if (_library == null)
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
                        using (Bitmap bitmap = ArtLibraryImageConverter.LoadFileForArt(files[0], _importOptions))
                        {
                            string name = ArtLibraryTransfer.ParseIdFromFileName(files[0]) < 0
                                ? Path.GetFileNameWithoutExtension(files[0])
                                : string.Empty;
                            PlaceSingleBitmap(bitmap, name, -1);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                using (Bitmap bitmap = ArtLibraryImageConverter.PrepareForArt(image, _importOptions))
                {
                    PlaceSingleBitmap(bitmap, string.Empty, -1);
                }
            }
        }

        #endregion

        #region [ Form events ]

        private void TimerSaveMetadata_Tick(object sender, EventArgs e)
        {
            SaveMetadata();
        }

        private void ArtLibraryForm_Shown(object sender, EventArgs e)
        {
            if (_library == null && !string.IsNullOrEmpty(_settings?.LastLibrary)
                && File.Exists(ArtLibraryFile.GetBasePath(_settings.LastLibrary) + ArtLibraryFile.IdxExtension))
            {
                OpenLibrary(_settings.LastLibrary, silent: true);
            }
        }

        private void ArtLibraryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerSaveMetadata.Stop();
            ApplyDetailIfDirty();
            SaveMetadata();

            if (_settings != null)
            {
                _settings.ShowOnlyOccupied = _showOnlyOccupied;
                _settings.LastLibrary = _library?.IdxPath;
                _settings.Save();
            }

            if (_library != null)
            {
                _library.EntryChanged -= OnLibraryEntryChanged;
                _library.Dispose();
                _library = null;
            }
        }

        #endregion
    }
}
