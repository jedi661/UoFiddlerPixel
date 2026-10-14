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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Core;
using UoFiddler.Controls.ArtLibrary.Helpers;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;
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
        private readonly ArtLibrarySettings _settings;

        private ArtLibraryFile _library;

        // ids shown in "only occupied" mode, ascending; in "all ids" mode the tile index is the id itself
        private List<int> _occupiedIds = new List<int>();
        private bool _showOnlyOccupied = true;
        private int _selectedId = -1;
        private Color _backgroundColor = Color.White;
        private bool _updatingLibraryCombo;

        private static readonly Font _freeSlotFont = new Font(FontFamily.GenericSansSerif, 6.5f);

        public ArtLibraryForm()
        {
            InitializeComponent();

            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Icon = Options.GetFiddlerIcon();

            _settings = ArtLibrarySettings.Load();
            _showOnlyOccupied = _settings.ShowOnlyOccupied;
            toolStripButtonOnlyOccupied.Checked = _showOnlyOccupied;

            ApplyTileViewOptions();
            FillLibraryCombo();
            UpdateAll();
        }

        #region [ Public ]

        /// <summary>The currently opened library or null.</summary>
        public ArtLibraryFile Library => _library;

        /// <summary>Selected graphic id or -1.</summary>
        public int SelectedId => _selectedId;

        /// <summary>
        /// Selects <paramref name="id"/> in the library (switches to "all ids" if the id is empty).
        /// </summary>
        public void SelectId(int id)
        {
            if (_library == null || !_library.IsValidId(id))
            {
                return;
            }

            if (_showOnlyOccupied && !_library.IsOccupied(id))
            {
                SetShowOnlyOccupied(false);
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

                _settings.RemoveRecent(path);
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

            _settings.AddRecent(_library.IdxPath);
            _settings.LastLibrary = _library.IdxPath;
            _settings.LastDirectory = Path.GetDirectoryName(_library.BasePath);
            _settings.Save();

            FillLibraryCombo();
            ReloadList();

            if (LibraryTileView.VirtualListSize > 0)
            {
                LibraryTileView.FocusIndex = 0;
                SetSelectedId(GetIdAtIndex(0));
            }
        }

        private void CloseLibrary()
        {
            if (_library != null)
            {
                _library.EntryChanged -= OnLibraryEntryChanged;
                _library.Dispose();
                _library = null;
            }

            _occupiedIds = new List<int>();
            pictureBoxPreview.Image = null;
            SetSelectedId(-1);
            ReloadList();
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

        /// <summary>
        /// Entry of the library combo box (recent libraries).
        /// </summary>
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

        #region [ List / Index mapping ]

        private void ReloadList()
        {
            if (_library == null)
            {
                _occupiedIds = new List<int>();
                LibraryTileView.VirtualListSize = 0;
                LibraryTileView.Invalidate();
                UpdateAll();
                return;
            }

            if (_showOnlyOccupied)
            {
                _occupiedIds = _library.GetOccupiedIds();
                LibraryTileView.VirtualListSize = _occupiedIds.Count;
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

            if (_showOnlyOccupied)
            {
                return index < _occupiedIds.Count ? _occupiedIds[index] : -1;
            }

            return index < _library.Capacity ? index : -1;
        }

        private int GetIndexOfId(int id)
        {
            if (_library == null || !_library.IsValidId(id))
            {
                return -1;
            }

            if (_showOnlyOccupied)
            {
                int index = _occupiedIds.BinarySearch(id);
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
            if (index < 0 && _showOnlyOccupied)
            {
                index = ~_occupiedIds.BinarySearch(Math.Max(id, 0));
                if (index >= _occupiedIds.Count)
                {
                    index = _occupiedIds.Count - 1;
                }
            }

            if (index < 0)
            {
                index = 0;
            }

            LibraryTileView.FocusIndex = index;
            SetSelectedId(GetIdAtIndex(index));
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

            if (!ReferenceEquals(sender, _library))
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

            if (_showOnlyOccupied)
            {
                bool occupied = _library.IsOccupied(e.Id);
                int index = _occupiedIds.BinarySearch(e.Id);
                bool changed = false;

                if (occupied && index < 0)
                {
                    _occupiedIds.Insert(~index, e.Id);
                    changed = true;
                }
                else if (!occupied && index >= 0)
                {
                    _occupiedIds.RemoveAt(index);
                    changed = true;
                }

                if (changed)
                {
                    int keepId = _selectedId;
                    LibraryTileView.VirtualListSize = _occupiedIds.Count;
                    RestoreSelection(keepId);
                }
            }

            LibraryTileView.Invalidate();
            UpdateStatus();

            if (e.Id == _selectedId)
            {
                UpdateDetail();
            }
        }

        #endregion

        #region [ Selection / Detail / Status ]

        private void SetSelectedId(int id)
        {
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
            LibraryTileView.Enabled = open;
            Text = open ? $"Art-Bibliothek - {_library.Name}" : "Art-Bibliothek";
        }

        private void UpdateDetail()
        {
            if (_library == null || _selectedId < 0)
            {
                pictureBoxPreview.Image = null;
                richTextBoxDetail.Clear();
                return;
            }

            Bitmap bitmap = _library.GetBitmap(_selectedId);
            pictureBoxPreview.BackColor = _backgroundColor;
            pictureBoxPreview.Image = bitmap;

            var text = new StringBuilder();
            text.AppendLine($"ID:        {_selectedId}");
            text.AppendLine($"Hex:       {ArtIdParser.ToHex(_selectedId)}");

            if (bitmap != null)
            {
                ArtLibraryEntry entry = _library.GetEntry(_selectedId);
                text.AppendLine($"Größe:     {bitmap.Width} x {bitmap.Height}");
                text.AppendLine($"Daten:     {entry.Length:N0} Bytes");
            }
            else
            {
                text.AppendLine("Status:    frei");
            }

            richTextBoxDetail.Text = text.ToString();
        }

        private void UpdateStatus()
        {
            if (_library == null)
            {
                toolStripStatusLabelLibrary.Text = "Keine Bibliothek geöffnet";
                toolStripStatusLabelCount.Text = string.Empty;
                toolStripStatusLabelSelected.Text = string.Empty;
                return;
            }

            toolStripStatusLabelLibrary.Text = $"{_library.Name}";
            toolStripStatusLabelCount.Text = $"{_library.OccupiedCount:N0} belegt / {_library.Capacity:N0} IDs";
            toolStripStatusLabelSelected.Text = _selectedId >= 0
                ? $"Auswahl: {ArtIdParser.ToDisplay(_selectedId)}"
                : string.Empty;
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
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.G)
            {
                toolStripTextBoxId.Focus();
                toolStripTextBoxId.SelectAll();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Ids of all selected tiles (or the focused one when nothing is selected), ascending.
        /// </summary>
        private List<int> GetSelectedIds()
        {
            var ids = LibraryTileView.SelectedIndices
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

        #endregion

        #region [ Actions ]

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

        private void RemoveSelected()
        {
            if (_library == null)
            {
                return;
            }

            List<int> ids = GetSelectedIds().Where(_library.IsOccupied).ToList();
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

            foreach (int id in ids)
            {
                _library.Remove(id);
            }
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

        #endregion

        #region [ Event handlers: context menu ]

        private void ContextMenuStripTileView_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool open = _library != null;
            bool occupied = open && _selectedId >= 0 && _library.IsOccupied(_selectedId);

            removeToolStripMenuItem.Enabled = occupied;
            findNextFreeToolStripMenuItem.Enabled = open;
            copyIdHexToolStripMenuItem.Enabled = open && _selectedId >= 0;
            copyIdDecToolStripMenuItem.Enabled = open && _selectedId >= 0;
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

        #region [ Form events ]

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
