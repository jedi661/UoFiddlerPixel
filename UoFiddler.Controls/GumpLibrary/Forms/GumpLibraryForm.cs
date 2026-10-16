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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Forms;
using UoFiddler.Controls.ArtLibrary.Helpers;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.GumpLibrary.Core;
using UoFiddler.Controls.GumpLibrary.Helpers;
using UoFiddler.Controls.GumpLibrary.Metadata;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.GumpLibrary.Forms
{
    /// <summary>
    /// Gump library window. Looks and works like the Gumps tab (list with 60 px rows, top menu with
    /// Index search and Misc, bottom menu with ID and Size, same context menu) and adds the library
    /// functions: categories (several per gump), own sort order, names, descriptions, tags, own preview
    /// images, full text search and the exchange with the original gumps.
    /// </summary>
    public partial class GumpLibraryForm : Form
    {
        private const string Title = "Gump-Bibliothek";
        private const string DragFormat = "UoFiddler.GumpLibrary.DragIds";
        private const int RowHeight = 60;
        private const int ThumbWidth = 100;
        private const int ThumbHeight = 54;
        private const int TextLeft = 105;
        private const int MaxClipboardItems = 2000;

        private static GumpLibraryForm _instance;

        private readonly GumpLibrarySettings _settings = GumpLibrarySettings.Load();
        private readonly Font _smallFont;
        private GumpLibraryFile _library;
        private List<int> _view = new List<int>();
        private DateTime? _lastSaved;

        private bool _updatingDetails;
        private bool _updatingFilters;
        private bool _updatingRecent;
        private bool _detailsDirty;
        private int _detailId = -1;

        private Point _dragStart;
        private int _dragIndex = -1;

        public GumpLibraryForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _smallFont = new Font(Font.FontFamily, 7.5f);
            listBox.ItemHeight = RowHeight;
        }

        #region [ Static entry points ]

        /// <summary>The open window or null.</summary>
        public static GumpLibraryForm Instance => _instance != null && !_instance.IsDisposed ? _instance : null;

        /// <summary>Opens (or activates) the gump library window.</summary>
        public static GumpLibraryForm ShowLibrary()
        {
            if (Instance == null)
            {
                _instance = new GumpLibraryForm();
                _instance.Show();
            }
            else
            {
                if (_instance.WindowState == FormWindowState.Minimized)
                {
                    _instance.WindowState = FormWindowState.Normal;
                }

                _instance.Activate();
            }

            return _instance;
        }

        /// <summary>
        /// Adds original gumps (e.g. the selection of the Gumps tab) to the open library: same id when it
        /// is free, otherwise the user decides (replace / next free id).
        /// </summary>
        public static void AddOriginalGumps(IReadOnlyList<int> originalIds)
        {
            GumpLibraryForm form = ShowLibrary();
            if (originalIds == null || originalIds.Count == 0)
            {
                return;
            }

            if (form._library == null)
            {
                MessageBox.Show(form, "Bitte zuerst eine Gump-Bibliothek öffnen oder neu anlegen (Buttons oben links).",
                    Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            form.FetchFromOriginal(originalIds, keepIds: true, startId: -1, askWhenOccupied: true);
        }

        #endregion

        #region [ Load / Close ]

        private void GumpLibraryForm_Load(object sender, EventArgs e)
        {
            _updatingFilters = true;
            foreach (GumpSortMode mode in Enum.GetValues(typeof(GumpSortMode)))
            {
                toolStripComboBoxSort.Items.Add(new SortItem(mode));
            }

            toolStripComboBoxSort.SelectedIndex = Math.Max(0, toolStripComboBoxSort.Items.Cast<SortItem>().ToList().FindIndex(i => i.Mode == _settings.SortMode));
            toolStripButtonSortDirection.Text = _settings.SortDescending ? "▼" : "▲";
            _updatingFilters = false;

            toolStripButtonAutoSave.Checked = _settings.AutoSave;
            toolStripButtonDetails.Checked = _settings.ShowDetails;
            splitContainerDetail.Panel2Collapsed = !_settings.ShowDetails;
            miscShowFreeSlots.Checked = _settings.ShowFreeSlots;
            showFreeSlotsMenuItem.Checked = _settings.ShowFreeSlots;
            miscUsePreview.Checked = _settings.UsePreviewInList;
            miscTakeNames.Checked = _settings.TakeNamesFromXml;
            miscImportBlack.Checked = _settings.ImportBlackTransparent;
            miscImportWhite.Checked = _settings.ImportWhiteTransparent;
            miscImportGray.Checked = _settings.ImportLightGrayTransparent;
            if (_settings.BackgroundArgb != null)
            {
                pictureBox.BackColor = Color.FromArgb(_settings.BackgroundArgb.Value);
            }

            try
            {
                if (_settings.ListSplitterDistance > 100 && _settings.ListSplitterDistance < splitContainer1.Width - 100)
                {
                    splitContainer1.SplitterDistance = _settings.ListSplitterDistance;
                }

                if (_settings.DetailSplitterDistance > 100 && _settings.DetailSplitterDistance < splitContainerDetail.Width - 100)
                {
                    splitContainerDetail.SplitterDistance = _settings.DetailSplitterDistance;
                }
            }
            catch
            {
                // window too small - keep the designer values
            }

            FillRecent();
            UpdateLibraryState();

            if (!string.IsNullOrEmpty(_settings.LastLibrary) && File.Exists(_settings.LastLibrary))
            {
                try
                {
                    SetLibrary(GumpLibraryFile.Open(_settings.LastLibrary));
                }
                catch (Exception ex)
                {
                    Options.Logger?.Warning(ex, "GumpLibrary: could not open {Path}", _settings.LastLibrary);
                }
            }
        }

        private void GumpLibraryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ApplyDetails();
            if (!ConfirmCloseLibrary())
            {
                e.Cancel = true;
                return;
            }

            _settings.ListSplitterDistance = splitContainer1.SplitterDistance;
            if (!splitContainerDetail.Panel2Collapsed)
            {
                _settings.DetailSplitterDistance = splitContainerDetail.SplitterDistance;
            }

            _settings.Save();
            CloseLibrary();
            if (_instance == this)
            {
                _instance = null;
            }

            _smallFont.Dispose();
        }

        /// <summary>Asks for unsaved changes. False = cancel.</summary>
        private bool ConfirmCloseLibrary()
        {
            if (_library == null)
            {
                return true;
            }

            ApplyDetails();
            if (_settings.AutoSave)
            {
                TrySaveMetadata();
                return true;
            }

            if (!_library.HasUnsavedChanges)
            {
                return true;
            }

            DialogResult result = MessageBox.Show(this,
                $"Die Gump-Bibliothek \"{_library.Name}\" hat ungespeicherte Änderungen.\n\nJetzt speichern?",
                Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Cancel)
            {
                return false;
            }

            if (result == DialogResult.Yes)
            {
                return SaveLibrary();
            }

            return true;
        }

        #endregion

        #region [ Library open / close ]

        private sealed class RecentItem
        {
            public string Path;

            public override string ToString()
            {
                string name = System.IO.Path.GetFileName(GumpLibraryFile.GetBasePath(Path));
                return $"{name}   ({System.IO.Path.GetDirectoryName(Path)})";
            }
        }

        private void FillRecent()
        {
            _updatingRecent = true;
            toolStripComboBoxLibrary.Items.Clear();
            foreach (string path in _settings.RecentLibraries)
            {
                int index = toolStripComboBoxLibrary.Items.Add(new RecentItem { Path = path });
                if (_library != null && string.Equals(path, _library.MainPath, StringComparison.OrdinalIgnoreCase))
                {
                    toolStripComboBoxLibrary.SelectedIndex = index;
                }
            }

            _updatingRecent = false;
        }

        private void ToolStripComboBoxLibrary_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingRecent || !(toolStripComboBoxLibrary.SelectedItem is RecentItem item))
            {
                return;
            }

            if (_library != null && string.Equals(item.Path, _library.MainPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!File.Exists(item.Path))
            {
                if (MessageBox.Show(this, $"Die Datei gibt es nicht mehr:\n{item.Path}\n\nAus der Liste entfernen?", Title,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _settings.RemoveRecent(item.Path);
                    _settings.Save();
                }

                FillRecent();
                return;
            }

            OpenLibrary(item.Path);
        }

        private void OpenLibrary(string path)
        {
            if (!ConfirmCloseLibrary())
            {
                FillRecent();
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                SetLibrary(GumpLibraryFile.Open(path));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Die Gump-Bibliothek konnte nicht geöffnet werden:\n{ex.Message}", Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                FillRecent();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void SetLibrary(GumpLibraryFile library)
        {
            CloseLibrary();
            _library = library;
            _library.AutoSave = _settings.AutoSave;
            _lastSaved = null;
            _settings.AddRecent(_library.MainPath);
            _settings.LastLibrary = _library.MainPath;
            _settings.LastDirectory = Path.GetDirectoryName(_library.BasePath);
            _settings.Save();

            FillRecent();
            FillCategoryFilter();
            UpdateLibraryState();
            RebuildView(0);
        }

        private void CloseLibrary()
        {
            if (_library == null)
            {
                return;
            }

            if (_settings.AutoSave)
            {
                TrySaveMetadata();
            }

            pictureBox.BackgroundImage = null;
            pictureBoxPreview.Image = null;
            _library.Dispose();
            _library = null;
            _detailId = -1;
            _detailsDirty = false;
            _view = new List<int>();
            listBox.Items.Clear();
            UpdateLibraryState();
        }

        private void UpdateLibraryState()
        {
            bool open = _library != null;
            Text = open ? $"{Title} - {_library.Name}" : $"{Title} - keine Bibliothek geöffnet";
            listBox.Enabled = open;
            topMenuToolStrip.Enabled = open;
            toolStripDropDownButtonLibrary.Enabled = open;
            toolStripTextBoxFilter.Enabled = open;
            toolStripComboBoxCategory.Enabled = open;
            toolStripComboBoxSort.Enabled = open;
            tableLayoutPanelDetail.Enabled = open;
            if (!open)
            {
                IDLabel.Text = "ID:";
                SizeLabel.Text = "Size:";
                CategoryLabel.Text = "Neue Bibliothek: ＋   Öffnen: Ordner-Button";
                CountLabel.Text = string.Empty;
                ClearDetails();
            }

            UpdateSaveState();
        }

        private void ToolStripButtonNew_Click(object sender, EventArgs e)
        {
            using (var dialog = new GumpLibraryNewForm())
            {
                dialog.LibraryDirectory = !string.IsNullOrEmpty(_settings.LastDirectory) ? _settings.LastDirectory : Options.AppDataPath ?? string.Empty;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                if (!ConfirmCloseLibrary())
                {
                    return;
                }

                try
                {
                    Cursor = Cursors.WaitCursor;
                    SetLibrary(GumpLibraryFile.Create(dialog.LibraryDirectory, dialog.LibraryName, dialog.Capacity));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Die Gump-Bibliothek konnte nicht angelegt werden:\n{ex.Message}", Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void ToolStripButtonOpen_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Gump-Bibliothek öffnen";
                dialog.Filter = "Gump-Bibliothek (*.gumplib.idx)|*.gumplib.idx|Alle Dateien (*.*)|*.*";
                if (!string.IsNullOrEmpty(_settings.LastDirectory) && Directory.Exists(_settings.LastDirectory))
                {
                    dialog.InitialDirectory = _settings.LastDirectory;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    OpenLibrary(dialog.FileName);
                }
            }
        }

        private void LibReload_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            string path = _library.MainPath;
            if (_library.HasUnsavedChanges && !_settings.AutoSave &&
                MessageBox.Show(this, "Ungespeicherte Änderungen gehen verloren. Trotzdem neu laden?", Title,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            _detailsDirty = false;
            _library.Metadata.AcceptChanges();
            _library.Dispose();
            _library = null;
            OpenLibrary(path);
        }

        private void LibClose_Click(object sender, EventArgs e)
        {
            if (!ConfirmCloseLibrary())
            {
                return;
            }

            CloseLibrary();
            _settings.LastLibrary = null;
            _settings.Save();
            FillRecent();
        }

        private void LibOpenFolder_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            try
            {
                Process.Start("explorer.exe", $"/select,\"{_library.MainPath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LibRemoveRecent_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            _settings.RemoveRecent(_library.MainPath);
            _settings.Save();
            FillRecent();
        }

        private void LibGrow_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            using (var dialog = new ArtLibraryGrowForm())
            {
                dialog.CurrentCapacity = _library.Capacity;
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.NewCapacity <= _library.Capacity)
                {
                    return;
                }

                try
                {
                    Cursor = Cursors.WaitCursor;
                    _library.Grow(dialog.NewCapacity);
                    MarkGraphicsChanged();
                    RebuildView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void LibCompact_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            long unused = _library.GetUnusedBytes();
            if (unused <= 0)
            {
                MessageBox.Show(this, "Es gibt keinen ungenutzten Platz.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, $"Ungenutzter Platz: {FormatBytes(unused)}.\nJetzt komprimieren? (Ungespeicherte Änderungen werden dabei gespeichert.)",
                    Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                ApplyDetails();
                _library.Save();
                long saved = _library.Compact();
                _lastSaved = DateTime.Now;
                MessageBox.Show(this, $"Fertig - {FormatBytes(saved)} freigegeben.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                UpdateSaveState();
                listBox.Invalidate();
            }
        }

        private void LibStatistics_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            ApplyDetails();
            List<int> ids = _library.GetOccupiedIds();
            GumpLibraryMetadata meta = _library.Metadata;
            int named = ids.Count(id => !string.IsNullOrWhiteSpace(meta.GetName(id)));
            int withCategory = ids.Count(id => meta.Get(id)?.CategoryIds.Count > 0);
            int withTags = ids.Count(id => meta.Get(id)?.Tags.Count > 0);
            int withDescription = ids.Count(id => !string.IsNullOrWhiteSpace(meta.Get(id)?.Description));
            int favorites = ids.Count(id => meta.Get(id)?.Favorite == true);
            int previews = ids.Count(_library.HasPreview);
            int withSource = ids.Count(id => meta.Get(id)?.SourceId != null);

            var text = new StringBuilder();
            text.AppendLine($"Bibliothek: {_library.Name}");
            text.AppendLine($"Ordner: {Path.GetDirectoryName(_library.BasePath)}");
            text.AppendLine($"Gumps: {ids.Count:N0} von {_library.Capacity:N0} IDs");
            text.AppendLine($"Daten: {FormatBytes(_library.DataLength)} (ungenutzt {FormatBytes(_library.GetUnusedBytes())})");
            text.AppendLine();
            text.AppendLine($"Mit Namen: {named:N0}   mit Beschreibung: {withDescription:N0}");
            text.AppendLine($"Mit Kategorie: {withCategory:N0}   mit Tags: {withTags:N0}");
            text.AppendLine($"Favoriten: {favorites:N0}   eigene Vorschaubilder: {previews:N0}   mit Original-ID: {withSource:N0}");
            text.AppendLine();
            text.AppendLine($"Kategorien ({meta.Categories.Count}):");
            Dictionary<int, int> counts = meta.CountPerCategory(ids);
            foreach ((GumpLibraryCategory category, int depth) in meta.GetCategoryTree())
            {
                text.AppendLine($"{new string(' ', 2 + (depth * 3))}{category.Name}: {counts[category.Id]:N0}");
            }

            text.AppendLine();
            text.AppendLine($"Tags: {meta.GetAllTags().Count:N0}");
            MessageBox.Show(this, text.ToString(), "Statistik", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ManageCategories_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            ApplyDetails();
            using (var form = new GumpLibraryCategoryForm(_library.Metadata, _library.GetOccupiedIds()))
            {
                form.ShowDialog(this);
                if (form.Changed)
                {
                    MetadataChanged();
                    FillCategoryFilter();
                    RebuildView();
                }
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024L)
            {
                return $"{bytes / (1024.0 * 1024.0):N1} MB";
            }

            return bytes >= 1024 ? $"{bytes / 1024.0:N1} KB" : $"{bytes} Bytes";
        }

        #endregion

        #region [ Save ]

        private bool SaveLibrary()
        {
            if (_library == null)
            {
                return true;
            }

            ApplyDetails();
            try
            {
                Cursor = Cursors.WaitCursor;
                _library.Save();
                _lastSaved = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Speichern fehlgeschlagen:\n{ex.Message}", Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor = Cursors.Default;
                UpdateSaveState();
                listBox.Invalidate();
            }
        }

        private bool TrySaveMetadata()
        {
            try
            {
                if (_library != null && _library.Metadata.IsDirty)
                {
                    _library.SaveMetadata();
                    _lastSaved = DateTime.Now;
                }

                return true;
            }
            catch (Exception ex)
            {
                Options.Logger?.Warning(ex, "GumpLibrary: could not save metadata");
                return false;
            }
        }

        /// <summary>Call after names, categories, tags, ... changed.</summary>
        private void MetadataChanged()
        {
            if (_library == null)
            {
                return;
            }

            _library.Metadata.MarkDirty();
            if (_settings.AutoSave)
            {
                TrySaveMetadata();
            }

            UpdateSaveState();
        }

        /// <summary>Call after gumps or previews changed (the storage writes through itself with auto save).</summary>
        private void MarkGraphicsChanged()
        {
            if (_settings.AutoSave)
            {
                TrySaveMetadata();
                _lastSaved = DateTime.Now;
            }

            UpdateSaveState();
        }

        private void UpdateSaveState()
        {
            if (_library == null)
            {
                SaveStateLabel.Text = string.Empty;
                saveToolStripButton.Enabled = false;
                return;
            }

            bool unsaved = _library.HasUnsavedChanges;
            saveToolStripButton.Enabled = true;
            if (unsaved)
            {
                int count = _library.UnsavedCount;
                SaveStateLabel.Text = count > 0 ? $"● Ungespeichert ({count})" : "● Ungespeichert";
                SaveStateLabel.ForeColor = Color.DarkOrange;
                SaveStateLabel.ToolTipText = "Speichern mit der Diskette oder Strg+S";
            }
            else
            {
                SaveStateLabel.Text = _lastSaved == null ? "✓ Gespeichert" : $"✓ Gespeichert {_lastSaved.Value:HH:mm:ss}";
                SaveStateLabel.ForeColor = Color.ForestGreen;
                SaveStateLabel.ToolTipText = _settings.AutoSave ? "Auto-Speichern ist an" : "Alles gespeichert";
            }

            string title = $"{Title} - {_library.Name}";
            Text = unsaved && !_settings.AutoSave ? title + " *" : title;
        }

        private void SaveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveLibrary();
        }

        private void ToolStripButtonAutoSave_Click(object sender, EventArgs e)
        {
            _settings.AutoSave = toolStripButtonAutoSave.Checked;
            _settings.Save();
            if (_library != null)
            {
                ApplyDetails();
                _library.AutoSave = _settings.AutoSave; // switching on writes pending gumps
                if (_settings.AutoSave)
                {
                    TrySaveMetadata();
                    _lastSaved = DateTime.Now;
                }
            }

            UpdateSaveState();
            listBox.Invalidate();
            CategoryLabel.Text = _settings.AutoSave
                ? "Auto-Speichern an: jede Änderung wird sofort gespeichert."
                : "Auto-Speichern aus: erst mit Speichern (Strg+S) wird geschrieben.";
        }

        private void TimerSave_Tick(object sender, EventArgs e)
        {
            if (_library != null && _settings.AutoSave && _library.Metadata.IsDirty)
            {
                TrySaveMetadata();
                UpdateSaveState();
            }
        }

        #endregion

        #region [ Filter / sort ]

        private sealed class SortItem
        {
            public SortItem(GumpSortMode mode)
            {
                Mode = mode;
            }

            public GumpSortMode Mode { get; }

            public override string ToString() => GumpLibrarySearch.GetSortModeText(Mode);
        }

        private enum CategoryFilterKind
        {
            All,
            None,
            Category
        }

        private sealed class CategoryFilterItem
        {
            public CategoryFilterKind Kind;
            public int Id;
            public string Text;

            public override string ToString() => Text;
        }

        private void FillCategoryFilter()
        {
            _updatingFilters = true;
            var selected = toolStripComboBoxCategory.SelectedItem as CategoryFilterItem;
            toolStripComboBoxCategory.Items.Clear();
            toolStripComboBoxCategory.Items.Add(new CategoryFilterItem { Kind = CategoryFilterKind.All, Text = "(Alle)" });
            toolStripComboBoxCategory.Items.Add(new CategoryFilterItem { Kind = CategoryFilterKind.None, Text = "(Ohne Kategorie)" });
            int select = 0;
            if (_library != null)
            {
                foreach ((GumpLibraryCategory category, int depth) in _library.Metadata.GetCategoryTree())
                {
                    int index = toolStripComboBoxCategory.Items.Add(new CategoryFilterItem
                    {
                        Kind = CategoryFilterKind.Category,
                        Id = category.Id,
                        Text = new string(' ', depth * 3) + category.Name
                    });
                    if (selected?.Kind == CategoryFilterKind.Category && selected.Id == category.Id)
                    {
                        select = index;
                    }
                }
            }

            if (selected?.Kind == CategoryFilterKind.None)
            {
                select = 1;
            }

            toolStripComboBoxCategory.SelectedIndex = select;
            _updatingFilters = false;
        }

        private GumpSortMode SortMode => (toolStripComboBoxSort.SelectedItem as SortItem)?.Mode ?? GumpSortMode.Id;

        private bool IsFiltered =>
            toolStripTextBoxFilter.Text.Trim().Length > 0 || toolStripButtonFavorites.Checked ||
            (toolStripComboBoxCategory.SelectedItem as CategoryFilterItem)?.Kind != CategoryFilterKind.All;

        private void ToolStripTextBoxFilter_TextChanged(object sender, EventArgs e)
        {
            timerFilter.Stop();
            timerFilter.Start();
        }

        private void TimerFilter_Tick(object sender, EventArgs e)
        {
            timerFilter.Stop();
            RebuildView();
        }

        private void ToolStripTextBoxFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                timerFilter.Stop();
                RebuildView();
                e.SuppressKeyPress = true;
                listBox.Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                toolStripTextBoxFilter.Text = string.Empty;
                e.SuppressKeyPress = true;
            }
        }

        private void ToolStripButtonClearFilter_Click(object sender, EventArgs e)
        {
            _updatingFilters = true;
            toolStripTextBoxFilter.Text = string.Empty;
            toolStripButtonFavorites.Checked = false;
            if (toolStripComboBoxCategory.Items.Count > 0)
            {
                toolStripComboBoxCategory.SelectedIndex = 0;
            }

            _updatingFilters = false;
            timerFilter.Stop();
            RebuildView();
        }

        private void ToolStripComboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updatingFilters)
            {
                RebuildView();
            }
        }

        private void ToolStripComboBoxSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingFilters)
            {
                return;
            }

            _settings.SortMode = SortMode;
            _settings.Save();
            RebuildView();
        }

        private void ToolStripButtonSortDirection_Click(object sender, EventArgs e)
        {
            _settings.SortDescending = !_settings.SortDescending;
            _settings.Save();
            toolStripButtonSortDirection.Text = _settings.SortDescending ? "▼" : "▲";
            RebuildView();
        }

        private void ToolStripButtonFavorites_Click(object sender, EventArgs e)
        {
            RebuildView();
        }

        private void SetSortMode(GumpSortMode mode, bool descending)
        {
            _updatingFilters = true;
            toolStripComboBoxSort.SelectedIndex = toolStripComboBoxSort.Items.Cast<SortItem>().ToList().FindIndex(i => i.Mode == mode);
            _updatingFilters = false;
            _settings.SortMode = mode;
            _settings.SortDescending = descending;
            _settings.Save();
            toolStripButtonSortDirection.Text = descending ? "▼" : "▲";
        }

        /// <summary>
        /// Builds the list: occupied gumps that match search, category and favorites, sorted;
        /// with "Show Free Slots" (and no filter) every id from 0 on like the Gumps tab.
        /// </summary>
        private void RebuildView(int selectId = -1, IReadOnlyCollection<int> selectIds = null)
        {
            if (_library == null)
            {
                return;
            }

            ApplyDetails();
            if (selectId < 0 && selectIds == null)
            {
                selectIds = SelectedIds;
                selectId = SelectedId;
            }

            List<int> occupied = _library.GetOccupiedIds();
            List<int> view;
            bool freeSlots = _settings.ShowFreeSlots && !IsFiltered;
            if (freeSlots)
            {
                int last = occupied.Count > 0 ? occupied[occupied.Count - 1] : 0;
                int upper = Math.Min(_library.MaxId, Math.Max(last + 256, 0xFFFF));
                view = Enumerable.Range(0, upper + 1).ToList();
                if (_settings.SortDescending)
                {
                    view.Reverse();
                }
            }
            else
            {
                GumpLibraryMetadata meta = _library.Metadata;
                var search = GumpLibrarySearch.Parse(toolStripTextBoxFilter.Text);
                HashSet<int> categoryIds = null;
                var categoryFilter = toolStripComboBoxCategory.SelectedItem as CategoryFilterItem;
                if (categoryFilter?.Kind == CategoryFilterKind.Category)
                {
                    categoryIds = meta.GetCategoryWithChildren(categoryFilter.Id);
                }

                bool favoritesOnly = toolStripButtonFavorites.Checked;
                IEnumerable<int> filtered = occupied.Where(id =>
                {
                    GumpLibraryItemInfo info = meta.Get(id);
                    if (favoritesOnly && info?.Favorite != true)
                    {
                        return false;
                    }

                    if (categoryFilter?.Kind == CategoryFilterKind.None && info?.CategoryIds.Count > 0)
                    {
                        return false;
                    }

                    if (categoryIds != null && (info == null || !info.CategoryIds.Any(categoryIds.Contains)))
                    {
                        return false;
                    }

                    return search.Matches(id, meta, _library.HasPreview);
                });

                view = GumpLibrarySearch.Sort(filtered, meta, SortMode, _settings.SortDescending);
            }

            _view = view;
            listBox.BeginUpdate();
            listBox.Items.Clear();
            var items = new object[view.Count];
            for (int i = 0; i < view.Count; ++i)
            {
                items[i] = view[i];
            }

            listBox.Items.AddRange(items);

            // restore selection
            var wanted = new HashSet<int>(selectIds ?? Array.Empty<int>());
            int focus = selectId >= 0 ? view.IndexOf(selectId) : -1;
            if (wanted.Count > 1)
            {
                for (int i = 0; i < view.Count; ++i)
                {
                    if (wanted.Contains(view[i]))
                    {
                        listBox.SetSelected(i, true);
                    }
                }
            }

            if (focus >= 0)
            {
                if (wanted.Count <= 1)
                {
                    listBox.SetSelected(focus, true);
                }

                listBox.TopIndex = Math.Max(0, focus - 2);
            }
            else if (view.Count > 0 && listBox.SelectedIndices.Count == 0)
            {
                listBox.SetSelected(0, true);
            }

            listBox.EndUpdate();

            int total = occupied.Count;
            CountLabel.Text = freeSlots
                ? $"{total:N0} Gumps (IDs 0-{ArtIdParser.ToHex(view.Count - 1)})"
                : view.Count == total ? $"{total:N0} Gumps" : $"{view.Count:N0} von {total:N0}";
            ShowSelected();
        }

        #endregion

        #region [ Selection ]

        private int SelectedId
        {
            get
            {
                // with several selected rows the row with the focus rectangle counts
                int index = listBox.SelectedIndex;
                int caret = CaretIndex;
                if (caret >= 0 && listBox.SelectedIndices.Contains(caret))
                {
                    index = caret;
                }

                return index >= 0 && index < _view.Count ? _view[index] : -1;
            }
        }

        private int CaretIndex => listBox.IsHandleCreated ? (int)SendMessage(listBox.Handle, LbGetCaretIndex, IntPtr.Zero, IntPtr.Zero) : -1;

        private const int LbGetCaretIndex = 0x019F;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private List<int> SelectedIds
        {
            get
            {
                var result = new List<int>();
                foreach (int index in listBox.SelectedIndices)
                {
                    if (index >= 0 && index < _view.Count)
                    {
                        result.Add(_view[index]);
                    }
                }

                return result;
            }
        }

        /// <summary>Selected ids that hold a gump.</summary>
        private List<int> SelectedOccupiedIds => _library == null ? new List<int>() : SelectedIds.Where(_library.IsOccupied).ToList();

        private void SelectId(int id)
        {
            int index = _view.IndexOf(id);
            if (index < 0 && _library != null && _library.IsValidId(id))
            {
                // not visible with the current filter: show everything
                _updatingFilters = true;
                toolStripTextBoxFilter.Text = string.Empty;
                toolStripButtonFavorites.Checked = false;
                if (toolStripComboBoxCategory.Items.Count > 0)
                {
                    toolStripComboBoxCategory.SelectedIndex = 0;
                }

                _updatingFilters = false;
                if (!_library.IsOccupied(id) && !_settings.ShowFreeSlots)
                {
                    SetShowFreeSlots(true, false);
                }

                RebuildView(id, new[] { id });
                index = _view.IndexOf(id);
            }

            if (index < 0)
            {
                return;
            }

            listBox.ClearSelected();
            listBox.SetSelected(index, true);
            listBox.TopIndex = Math.Max(0, index - 2);
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelected();
        }

        private void ShowSelected()
        {
            int id = SelectedId;
            if (id != _detailId)
            {
                ApplyDetails();
            }

            listBox.Invalidate();
            if (_library == null || id < 0)
            {
                pictureBox.BackgroundImage = null;
                IDLabel.Text = "ID:";
                SizeLabel.Text = "Size:";
                CategoryLabel.Text = string.Empty;
                ClearDetails();
                return;
            }

            GumpLibraryItemInfo info = _library.Metadata.Get(id);
            Bitmap bmp = _library.GetGump(id);
            pictureBox.BackgroundImage = bmp;
            string name = info?.Name ?? string.Empty;
            IDLabel.Text = $"ID: 0x{id:X} ({id}) {name}";
            SizeLabel.Text = bmp != null ? $"Size: {bmp.Width},{bmp.Height}" : "Size: frei";
            int count = listBox.SelectedIndices.Count;
            string categories = _library.Metadata.GetCategoryText(info);
            CategoryLabel.Text = count > 1 ? $"{count} ausgewählt" : categories;
            CategoryLabel.ToolTipText = categories;
            JumpToMaleFemaleInvalidate();
            if (id != _detailId || !_detailsDirty)
            {
                LoadDetails(id);
            }
        }

        private void JumpToMaleFemaleInvalidate()
        {
            int gumpId = GetGumpIdForGender(SelectedId);
            if (gumpId >= GumpLibraryTransfer.MaleGumpStart)
            {
                jumpToMaleFemale.Text = gumpId >= GumpLibraryTransfer.FemaleGumpStart ? "Jump to Male" : "Jump to Female";
                jumpToMaleFemale.Enabled = true;
            }
            else
            {
                jumpToMaleFemale.Text = "Jump to Male/Female";
                jumpToMaleFemale.Enabled = false;
            }
        }

        /// <summary>The gump id that decides male/female: the original id if known, otherwise the library id.</summary>
        private int GetGumpIdForGender(int id)
        {
            if (_library == null || id < 0)
            {
                return -1;
            }

            return _library.Metadata.Get(id)?.SourceId ?? id;
        }

        #endregion

        #region [ Drawing ]

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _view.Count || _library == null)
            {
                return;
            }

            int id = _view[e.Index];
            Rectangle bounds = e.Bounds;
            bool selected = (e.State & DrawItemState.Selected) != 0;
            e.Graphics.FillRectangle(SystemBrushes.Window, bounds);

            Brush fontBrush = Brushes.Gray;
            GumpLibraryItemInfo info = _library.Metadata.Get(id);
            if (_library.IsOccupied(id))
            {
                if (selected)
                {
                    e.Graphics.FillRectangle(Brushes.LightSteelBlue, bounds.X, bounds.Y, TextLeft, RowHeight);
                }
                else if (_library.IsPending(id))
                {
                    e.Graphics.FillRectangle(Brushes.LightCoral, bounds.X, bounds.Y, TextLeft, RowHeight);
                }

                Bitmap bmp = _settings.UsePreviewInList ? _library.GetThumbnailSource(id) : _library.GetGump(id);
                if (bmp != null)
                {
                    Size size = GumpImageTools.FitInto(bmp.Size, ThumbWidth, ThumbHeight);
                    e.Graphics.DrawImage(bmp, new Rectangle(bounds.X + 3, bounds.Y + 3, size.Width, size.Height));
                }
                else
                {
                    fontBrush = Brushes.Red;
                }

                // color of the first colored category
                GumpLibraryCategory colored = info?.CategoryIds.Select(c => _library.Metadata.GetCategory(c)).FirstOrDefault(c => c?.Color != null);
                if (colored != null)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(colored.Color.Value)))
                    {
                        e.Graphics.FillRectangle(brush, bounds.X, bounds.Y, 3, RowHeight);
                    }
                }
            }
            else
            {
                if (selected)
                {
                    e.Graphics.FillRectangle(Brushes.LightSteelBlue, bounds.X, bounds.Y, TextLeft, RowHeight);
                }

                fontBrush = Brushes.Red;
            }

            if (selected)
            {
                fontBrush = SystemBrushes.ControlText;
            }

            string name = info?.Name ?? string.Empty;
            string line1 = $"0x{id:X} ({id}) {name}";
            string line2 = BuildSecondLine(id, info);
            if (line2.Length == 0)
            {
                SizeF textSize = e.Graphics.MeasureString(line1, Font);
                e.Graphics.DrawString(line1, Font, fontBrush, new PointF(bounds.X + TextLeft, bounds.Y + ((bounds.Height / 2) - (textSize.Height / 2))));
            }
            else
            {
                e.Graphics.DrawString(line1, Font, fontBrush, new PointF(bounds.X + TextLeft, bounds.Y + 10));
                e.Graphics.DrawString(line2, _smallFont, Brushes.DimGray,
                    new RectangleF(bounds.X + TextLeft, bounds.Y + 30, Math.Max(10, bounds.Width - TextLeft - 18), 26));
            }

            if (info?.Favorite == true)
            {
                e.Graphics.DrawString("★", Font, Brushes.Goldenrod, new PointF(bounds.Right - 18, bounds.Y + 4));
            }

            if (_library.HasPreview(id))
            {
                e.Graphics.DrawString("▣", _smallFont, Brushes.SteelBlue, new PointF(bounds.Right - 16, bounds.Bottom - 16));
            }

            e.Graphics.DrawLine(Pens.Gainsboro, bounds.X, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
        }

        private string BuildSecondLine(int id, GumpLibraryItemInfo info)
        {
            if (info == null || !_library.IsOccupied(id))
            {
                return string.Empty;
            }

            var parts = new List<string>();
            if (info.SourceId != null)
            {
                string gender = GumpLibraryTransfer.GetGenderText(info.SourceId.Value);
                parts.Add($"Orig. 0x{info.SourceId.Value:X}{(gender.Length > 0 ? " " + gender : string.Empty)}");
            }

            string categories = _library.Metadata.GetCategoryText(info);
            if (categories.Length > 0)
            {
                parts.Add(categories);
            }

            if (info.Tags.Count > 0)
            {
                parts.Add("#" + string.Join(" #", info.Tags));
            }

            return string.Join("  ·  ", parts);
        }

        #endregion

        #region [ Details (name, categories, tags, description, preview) ]

        private sealed class CategoryListItem
        {
            public int Id;
            public string Text;

            public override string ToString() => Text;
        }

        private void ClearDetails()
        {
            _updatingDetails = true;
            _detailId = -1;
            _detailsDirty = false;
            textBoxName.Text = string.Empty;
            textBoxSource.Text = string.Empty;
            numericUpDownSortOrder.Value = 0;
            checkBoxFavorite.Checked = false;
            checkedListBoxCategories.Items.Clear();
            textBoxTags.Text = string.Empty;
            textBoxDescription.Text = string.Empty;
            pictureBoxPreview.Image = null;
            labelPreviewInfo.Text = string.Empty;
            labelDetailInfo.Text = string.Empty;
            tableLayoutPanelDetail.Enabled = false;
            _updatingDetails = false;
        }

        private void LoadDetails(int id)
        {
            if (_library == null || id < 0)
            {
                ClearDetails();
                return;
            }

            _updatingDetails = true;
            try
            {
                _detailId = id;
                _detailsDirty = false;
                bool occupied = _library.IsOccupied(id);
                tableLayoutPanelDetail.Enabled = occupied;
                GumpLibraryItemInfo info = _library.Metadata.Get(id);
                textBoxName.Text = info?.Name ?? string.Empty;
                textBoxSource.Text = info?.SourceId != null ? ArtIdParser.ToHex(info.SourceId.Value) : string.Empty;
                numericUpDownSortOrder.Value = info?.SortOrder ?? 0;
                checkBoxFavorite.Checked = info?.Favorite == true;
                textBoxTags.Text = info == null ? string.Empty : string.Join(", ", info.Tags);
                textBoxDescription.Text = info?.Description ?? string.Empty;

                checkedListBoxCategories.BeginUpdate();
                checkedListBoxCategories.Items.Clear();
                foreach ((GumpLibraryCategory category, int depth) in _library.Metadata.GetCategoryTree())
                {
                    int index = checkedListBoxCategories.Items.Add(new CategoryListItem { Id = category.Id, Text = new string(' ', depth * 4) + category.Name });
                    checkedListBoxCategories.SetItemChecked(index, info?.CategoryIds.Contains(category.Id) == true);
                }

                checkedListBoxCategories.EndUpdate();

                comboBoxAddTag.Items.Clear();
                comboBoxAddTag.Items.Add("+ Tag hinzufügen...");
                foreach (string tag in _library.Metadata.GetAllTags())
                {
                    comboBoxAddTag.Items.Add(tag);
                }

                comboBoxAddTag.SelectedIndex = 0;

                Bitmap preview = _library.GetPreview(id);
                pictureBoxPreview.Image = preview;
                labelPreviewInfo.Text = !occupied
                    ? "freie ID"
                    : preview != null ? $"eigenes Vorschaubild {preview.Width}x{preview.Height}" : "kein eigenes Vorschaubild (Liste zeigt das Gump)";
                labelDetailInfo.Text = occupied && info != null
                    ? $"hinzugefügt {info.Added:dd.MM.yyyy}, geändert {info.Modified:dd.MM.yyyy HH:mm}"
                    : string.Empty;
                labelDetailInfo.ForeColor = SystemColors.GrayText;
            }
            finally
            {
                _updatingDetails = false;
            }
        }

        private void MarkDetailsDirty()
        {
            if (_updatingDetails || _detailId < 0)
            {
                return;
            }

            _detailsDirty = true;
            labelDetailInfo.Text = "geändert - Übernehmen (Strg+Enter)";
            labelDetailInfo.ForeColor = Color.DarkOrange;
        }

        private void DetailField_Changed(object sender, EventArgs e)
        {
            MarkDetailsDirty();
        }

        private void CheckedListBoxCategories_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            MarkDetailsDirty();
        }

        private void DetailTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyDetails();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                LoadDetails(_detailId);
                e.SuppressKeyPress = true;
            }
        }

        private void ComboBoxAddTag_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxAddTag.SelectedIndex <= 0)
            {
                return;
            }

            string tag = comboBoxAddTag.SelectedItem.ToString();
            List<string> tags = GumpLibraryMetadata.ParseTags(textBoxTags.Text);
            if (!tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                tags.Add(tag);
                textBoxTags.Text = string.Join(", ", tags);
            }

            BeginInvoke(new Action(() => comboBoxAddTag.SelectedIndex = 0));
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            ApplyDetails();
        }

        private void ButtonRevert_Click(object sender, EventArgs e)
        {
            _detailsDirty = false;
            LoadDetails(_detailId);
        }

        /// <summary>Writes the detail fields into the metadata of the gump shown there (if changed).</summary>
        private void ApplyDetails()
        {
            if (!_detailsDirty || _library == null || _detailId < 0)
            {
                return;
            }

            int id = _detailId;
            _detailsDirty = false;
            if (!_library.IsOccupied(id))
            {
                return;
            }

            GumpLibraryItemInfo info = _library.Metadata.GetOrCreate(id);
            info.Name = textBoxName.Text.Trim();
            string source = textBoxSource.Text.Trim();
            if (source.Length == 0)
            {
                info.SourceId = null;
            }
            else if (ArtIdParser.TryParse(source, out int sourceId))
            {
                info.SourceId = sourceId;
            }
            else
            {
                MessageBox.Show(this, $"\"{source}\" ist keine gültige Original-ID - sie bleibt unverändert.", Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            info.SortOrder = (int)numericUpDownSortOrder.Value;
            info.Favorite = checkBoxFavorite.Checked;
            info.CategoryIds = checkedListBoxCategories.CheckedItems.Cast<CategoryListItem>().Select(c => c.Id).ToList();
            info.Description = textBoxDescription.Text.Trim();
            _library.Metadata.SetTags(id, GumpLibraryMetadata.ParseTags(textBoxTags.Text));
            info.Modified = DateTime.Now;
            MetadataChanged();

            labelDetailInfo.Text = "übernommen";
            labelDetailInfo.ForeColor = Color.ForestGreen;
            listBox.Invalidate();
            if (SelectedId == id)
            {
                IDLabel.Text = $"ID: 0x{id:X} ({id}) {info.Name}";
                CategoryLabel.Text = _library.Metadata.GetCategoryText(info);
            }
        }

        private void EditProperties_Click(object sender, EventArgs e)
        {
            if (!toolStripButtonDetails.Checked)
            {
                toolStripButtonDetails.Checked = true;
                ToolStripButtonDetails_Click(sender, e);
            }

            textBoxName.Focus();
            textBoxName.SelectAll();
        }

        private void ToolStripButtonDetails_Click(object sender, EventArgs e)
        {
            _settings.ShowDetails = toolStripButtonDetails.Checked;
            _settings.Save();
            splitContainerDetail.Panel2Collapsed = !_settings.ShowDetails;
        }

        #endregion

        #region [ Import helpers ]

        private ArtImportOptions ImportOptions => new ArtImportOptions
        {
            BlackTransparent = _settings.ImportBlackTransparent,
            WhiteTransparent = _settings.ImportWhiteTransparent,
            LightGrayTransparent = _settings.ImportLightGrayTransparent
        };

        private static void CheckGumpSize(Image image)
        {
            if (image.Width > GumpLibraryCodec.MaxSize || image.Height > GumpLibraryCodec.MaxSize)
            {
                throw new InvalidOperationException($"Das Bild ist {image.Width}x{image.Height} groß - erlaubt sind höchstens {GumpLibraryCodec.MaxSize}x{GumpLibraryCodec.MaxSize}.");
            }
        }

        /// <summary>Loads an image file for the gump format (background colors transparent).</summary>
        private Bitmap LoadImageFile(string path)
        {
            using (Bitmap loaded = ArtLibraryImageConverter.LoadFile(path))
            {
                CheckGumpSize(loaded);
                return ArtLibraryImageConverter.PrepareForArt(loaded, ImportOptions);
            }
        }

        /// <summary>True when the id may be written: free, or occupied and the user agrees.</summary>
        private bool ConfirmReplace(int id, string what = "Gump")
        {
            if (!_library.IsOccupied(id))
            {
                return true;
            }

            string name = _library.Metadata.GetName(id);
            return MessageBox.Show(this,
                $"ID {ArtIdParser.ToDisplay(id)}{(name.Length > 0 ? $" \"{name}\"" : string.Empty)} ist belegt.\n\n{what} ersetzen? (Name, Kategorien und Tags bleiben erhalten.)",
                Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        /// <summary>Stores a gump and marks the change; the bitmap stays owned by the caller.</summary>
        private void StoreGump(int id, Bitmap bitmap)
        {
            _library.SetGump(id, bitmap);
        }

        private string[] ChooseImageFiles(bool multiselect, string title)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = multiselect;
                dialog.Title = title;
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png;*.gif;*.jpg)|*.tif;*.tiff;*.bmp;*.png;*.gif;*.jpg;*.jpeg|Alle Dateien (*.*)|*.*";
                if (!string.IsNullOrEmpty(_settings.LastImportDirectory) && Directory.Exists(_settings.LastImportDirectory))
                {
                    dialog.InitialDirectory = _settings.LastImportDirectory;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return null;
                }

                _settings.LastImportDirectory = Path.GetDirectoryName(dialog.FileName);
                _settings.Save();
                return dialog.FileNames;
            }
        }

        /// <summary>Imports image files with the target dialog (start id / ids from the file names).</summary>
        private void ImportFiles(IReadOnlyList<string> files, int preferredId = -1)
        {
            if (_library == null || files == null || files.Count == 0)
            {
                return;
            }

            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = "Gumps importieren";
                dialog.InfoText = files.Count == 1 ? $"Datei: {Path.GetFileName(files[0])}" : $"{files.Count} Dateien";
                dialog.ShowSource = false;
                dialog.ShowTransparencyOptions = true;
                dialog.ImportOptions = ImportOptions;
                dialog.KeepIdsText = "ID aus dem Dateinamen verwenden (z. B. \"0xC350.png\")";
                dialog.MaxTargetId = _library.MaxId;
                int start = preferredId >= 0 ? preferredId : SelectedId;
                dialog.TargetId = start >= 0 ? start : Math.Max(0, _library.FindNextFree(0));
                dialog.KeepIds = files.Count > 1 && files.All(f => ArtLibraryTransfer.ParseIdFromFileName(f) >= 0);
                dialog.Overwrite = files.Count == 1;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                ArtImportOptions options = dialog.ImportOptions;
                _settings.ImportBlackTransparent = options.BlackTransparent;
                _settings.ImportWhiteTransparent = options.WhiteTransparent;
                _settings.ImportLightGrayTransparent = options.LightGrayTransparent;
                _settings.Save();
                miscImportBlack.Checked = options.BlackTransparent;
                miscImportWhite.Checked = options.WhiteTransparent;
                miscImportGray.Checked = options.LightGrayTransparent;

                List<int> sourceIds = files.Select(ArtLibraryTransfer.ParseIdFromFileName).ToList();
                int[] targets = ArtLibraryTransfer.AssignTargetIds(sourceIds, dialog.TargetId, dialog.KeepIds, dialog.Overwrite, _library.IsOccupied, _library.MaxId);
                RunImport(files, targets);
            }
        }

        private void RunImport(IReadOnlyList<string> files, IReadOnlyList<int> targets)
        {
            var errors = new List<string>();
            int done = 0, first = -1;
            BeginProgress(files.Count);
            try
            {
                for (int i = 0; i < files.Count; ++i)
                {
                    StepProgress(i);
                    if (targets[i] < 0)
                    {
                        errors.Add($"{Path.GetFileName(files[i])}: keine freie / gültige Ziel-ID");
                        continue;
                    }

                    try
                    {
                        using (Bitmap bitmap = LoadImageFile(files[i]))
                        {
                            StoreGump(targets[i], bitmap);
                        }

                        GumpLibraryItemInfo info = _library.Metadata.GetOrCreate(targets[i]);
                        if (string.IsNullOrEmpty(info.Name))
                        {
                            info.Name = Path.GetFileNameWithoutExtension(files[i]);
                        }

                        if (first < 0)
                        {
                            first = targets[i];
                        }

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
                EndProgress();
                MetadataChanged();
                MarkGraphicsChanged();
            }

            RebuildView(first, first >= 0 ? new[] { first } : null);
            ReportResult($"{done} Gump(s) importiert.", errors);
        }

        private void ReportResult(string message, IReadOnlyCollection<string> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                CategoryLabel.Text = message;
                return;
            }

            MessageBox.Show(this, message + "\n\nNicht übernommen:\n" + string.Join("\n", errors.Take(25)) + (errors.Count > 25 ? $"\n... und {errors.Count - 25} weitere" : string.Empty),
                Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BeginProgress(int maximum)
        {
            ProgressBar.Maximum = Math.Max(1, maximum);
            ProgressBar.Value = 0;
            ProgressBar.Visible = maximum > 20;
            Cursor = Cursors.WaitCursor;
        }

        private void StepProgress(int value)
        {
            if (!ProgressBar.Visible)
            {
                return;
            }

            ProgressBar.Value = Math.Min(ProgressBar.Maximum, value);
            if (value % 25 == 0)
            {
                Application.DoEvents();
            }
        }

        private void EndProgress()
        {
            ProgressBar.Visible = false;
            Cursor = Cursors.Default;
        }

        private void MiscImportFiles_Click(object sender, EventArgs e)
        {
            string[] files = ChooseImageFiles(true, "Bilder als Gumps importieren");
            if (files != null)
            {
                ImportFiles(files);
            }
        }

        private void MiscImportFolder_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Ordner mit Bildern wählen (bmp, png, tif, gif, jpg)";
                dialog.UseDescriptionForTitle = true;
                if (!string.IsNullOrEmpty(_settings.LastImportDirectory) && Directory.Exists(_settings.LastImportDirectory))
                {
                    dialog.SelectedPath = _settings.LastImportDirectory;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                _settings.LastImportDirectory = dialog.SelectedPath;
                _settings.Save();
                List<string> files = Directory.GetFiles(dialog.SelectedPath)
                    .Where(ArtLibraryImageConverter.IsSupportedFile)
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (files.Count == 0)
                {
                    MessageBox.Show(this, "Im Ordner sind keine Bilder.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ImportFiles(files);
            }
        }

        private void Replace_Click(object sender, EventArgs e)
        {
            int id = SelectedId;
            if (_library == null || id < 0)
            {
                return;
            }

            string[] files = ChooseImageFiles(false, "Choose image file to replace");
            if (files == null)
            {
                return;
            }

            try
            {
                using (Bitmap bitmap = LoadImageFile(files[0]))
                {
                    StoreGump(id, bitmap);
                }

                MarkGraphicsChanged();
                MetadataChanged();
                RebuildView(id, new[] { id });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InsertText_TextChanged(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            bool valid = ArtIdParser.TryParse(InsertText.Text, out int id) && _library.IsValidId(id);
            InsertText.ForeColor = !valid ? Color.Red : _library.IsOccupied(id) ? Color.DarkOrange : Color.Green;
        }

        /// <summary>Insert At..: Enter = choose an image and put it on the typed id.</summary>
        private void InsertText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || _library == null)
            {
                return;
            }

            e.SuppressKeyPress = true;
            if (!ArtIdParser.TryParse(InsertText.Text, out int id) || !_library.IsValidId(id))
            {
                MessageBox.Show(this, $"Ungültige ID (0 bis {ArtIdParser.ToHex(_library.MaxId)}).", Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            contextMenuStrip.Close();
            if (!ConfirmReplace(id))
            {
                return;
            }

            string[] files = ChooseImageFiles(false, $"Choose image file to insert at {ArtIdParser.ToHex(id)}");
            if (files == null)
            {
                return;
            }

            try
            {
                using (Bitmap bitmap = LoadImageFile(files[0]))
                {
                    StoreGump(id, bitmap);
                }

                GumpLibraryItemInfo info = _library.Metadata.GetOrCreate(id);
                if (string.IsNullOrEmpty(info.Name))
                {
                    info.Name = Path.GetFileNameWithoutExtension(files[0]);
                }

                MetadataChanged();
                MarkGraphicsChanged();
                InsertText.Text = string.Empty;
                RebuildView(id, new[] { id });
                SelectId(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Insert Starting From: Enter = choose several images, they go to the free ids from the typed id on.</summary>
        private void InsertStartingFromTb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || _library == null)
            {
                return;
            }

            e.SuppressKeyPress = true;
            if (!ArtIdParser.TryParse(InsertStartingFromTb.Text, out int start) || !_library.IsValidId(start))
            {
                MessageBox.Show(this, $"Ungültige ID (0 bis {ArtIdParser.ToHex(_library.MaxId)}).", Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            contextMenuStrip.Close();
            string[] files = ChooseImageFiles(true, $"Bilder ab {ArtIdParser.ToHex(start)} einfügen (nächste freie IDs)");
            if (files == null)
            {
                return;
            }

            int[] targets = ArtLibraryTransfer.AssignTargetIds(files.Select(_ => -1).ToList(), start, false, false, _library.IsOccupied, _library.MaxId);
            RunImport(files, targets);
            InsertStartingFromTb.Text = string.Empty;
        }

        #endregion

        #region [ Export ]

        private void ExportSelected(ImageFormat format, string extension)
        {
            List<int> ids = SelectedOccupiedIds;
            if (ids.Count == 0)
            {
                return;
            }

            string folder = Options.OutputPath;
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                folder = ChooseExportFolder();
                if (folder == null)
                {
                    return;
                }
            }

            int done = ExportIds(ids, folder, format, extension);
            MessageBox.Show(this, done == 1
                    ? $"Gump saved to {Path.Combine(folder, GumpLibraryTransfer.GetExportFileName(ids[0], extension))}"
                    : $"{done} Gumps saved to {folder}",
                "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private int ExportIds(IReadOnlyList<int> ids, string folder, ImageFormat format, string extension)
        {
            int done = 0;
            BeginProgress(ids.Count);
            try
            {
                for (int i = 0; i < ids.Count; ++i)
                {
                    StepProgress(i);
                    Bitmap gump = _library.GetGump(ids[i]);
                    if (gump == null)
                    {
                        continue;
                    }

                    using (Bitmap export = ArtLibraryImageConverter.ToExportBitmap(gump))
                    {
                        export.Save(Path.Combine(folder, GumpLibraryTransfer.GetExportFileName(ids[i], extension)), format);
                    }

                    ++done;
                }
            }
            finally
            {
                EndProgress();
            }

            return done;
        }

        private string ChooseExportFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.UseDescriptionForTitle = true;
                string start = !string.IsNullOrEmpty(_settings.LastExportDirectory) ? _settings.LastExportDirectory : Options.OutputPath;
                if (!string.IsNullOrEmpty(start) && Directory.Exists(start))
                {
                    dialog.SelectedPath = start;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return null;
                }

                _settings.LastExportDirectory = dialog.SelectedPath;
                _settings.Save();
                return dialog.SelectedPath;
            }
        }

        private void ExportAll(ImageFormat format, string extension)
        {
            if (_library == null)
            {
                return;
            }

            // with a filter only the visible gumps are exported
            List<int> ids = IsFiltered ? _view.Where(_library.IsOccupied).ToList() : _library.GetOccupiedIds();
            if (ids.Count == 0)
            {
                return;
            }

            string folder = ChooseExportFolder();
            if (folder == null)
            {
                return;
            }

            int done = ExportIds(ids, folder, format, extension);
            MessageBox.Show(this, $"All Gumps saved to {folder} ({done})", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExtractImageBmp_Click(object sender, EventArgs e) => ExportSelected(ImageFormat.Bmp, "bmp");

        private void ExtractImageTiff_Click(object sender, EventArgs e) => ExportSelected(ImageFormat.Tiff, "tiff");

        private void ExtractImageJpg_Click(object sender, EventArgs e) => ExportSelected(ImageFormat.Jpeg, "jpg");

        private void ExtractImagePng_Click(object sender, EventArgs e) => ExportSelected(ImageFormat.Png, "png");

        private void MiscExportAllBmp_Click(object sender, EventArgs e) => ExportAll(ImageFormat.Bmp, "bmp");

        private void MiscExportAllTiff_Click(object sender, EventArgs e) => ExportAll(ImageFormat.Tiff, "tiff");

        private void MiscExportAllJpg_Click(object sender, EventArgs e) => ExportAll(ImageFormat.Jpeg, "jpg");

        private void MiscExportAllPng_Click(object sender, EventArgs e) => ExportAll(ImageFormat.Png, "png");

        private string ChooseSaveFile(string title, string filter, string fileName)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = filter;
                dialog.FileName = fileName;
                if (!string.IsNullOrEmpty(_settings.LastExportDirectory) && Directory.Exists(_settings.LastExportDirectory))
                {
                    dialog.InitialDirectory = _settings.LastExportDirectory;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return null;
                }

                _settings.LastExportDirectory = Path.GetDirectoryName(dialog.FileName);
                _settings.Save();
                return dialog.FileName;
            }
        }

        private void MiscExportIds_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            string path = ChooseSaveFile("Export all IDs to Text", "Text (*.txt)|*.txt", $"{_library.Name}_GumpIDs.txt");
            if (path == null)
            {
                return;
            }

            var text = new StringBuilder();
            foreach (int id in _library.GetOccupiedIds())
            {
                text.AppendLine($"0x{id:X4}\t{id}\t{_library.Metadata.GetName(id)}");
            }

            File.WriteAllText(path, text.ToString(), Encoding.UTF8);
            MessageBox.Show(this, $"Saved to {path}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MiscExportCsv_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            ApplyDetails();
            string path = ChooseSaveFile("Metadaten als CSV exportieren", "CSV (*.csv)|*.csv", $"{_library.Name}.csv");
            if (path == null)
            {
                return;
            }

            static string Csv(string value)
            {
                value ??= string.Empty;
                return value.IndexOfAny(new[] { ';', '"', '\n', '\r' }) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
            }

            GumpLibraryMetadata meta = _library.Metadata;
            var text = new StringBuilder("ID;Hex;Name;Original-ID;Kategorien;Tags;Favorit;Position;Breite;Höhe;Vorschaubild;Beschreibung\r\n");
            foreach (int id in GumpLibrarySearch.Sort(_library.GetOccupiedIds(), meta, SortMode, _settings.SortDescending))
            {
                GumpLibraryItemInfo info = meta.Get(id);
                _library.TryGetSize(id, out int width, out int height);
                text.Append(id).Append(';').Append(ArtIdParser.ToHex(id)).Append(';')
                    .Append(Csv(info?.Name)).Append(';')
                    .Append(info?.SourceId != null ? ArtIdParser.ToHex(info.SourceId.Value) : string.Empty).Append(';')
                    .Append(Csv(meta.GetCategoryText(info))).Append(';')
                    .Append(Csv(info == null ? string.Empty : string.Join(", ", info.Tags))).Append(';')
                    .Append(info?.Favorite == true ? "ja" : string.Empty).Append(';')
                    .Append(info?.SortOrder ?? 0).Append(';')
                    .Append(width).Append(';').Append(height).Append(';')
                    .Append(_library.HasPreview(id) ? "ja" : string.Empty).Append(';')
                    .Append(Csv(info?.Description)).Append("\r\n");
            }

            File.WriteAllText(path, text.ToString(), new UTF8Encoding(true));
            MessageBox.Show(this, $"Saved to {path}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region [ Clipboard ]

        private void Copy_Click(object sender, EventArgs e)
        {
            List<int> ids = SelectedOccupiedIds;
            if (ids.Count == 0)
            {
                CategoryLabel.Text = "No image to copy!";
                return;
            }

            if (ids.Count > MaxClipboardItems)
            {
                MessageBox.Show(this, $"Es können höchstens {MaxClipboardItems} Gumps auf einmal kopiert werden.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                GumpLibraryClipboard.Copy(ids.Select(id => GumpLibraryClipboard.CreateItem(_library, id)).Where(i => i != null).ToList());
                CategoryLabel.Text = ids.Count == 1 ? "The image has been copied to the clipboard!" : $"{ids.Count} Gumps in die Zwischenablage kopiert.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Kopieren fehlgeschlagen:\n{ex.Message}", Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Import (Strg+V): own clipboard data (with name, categories, tags, preview) or a normal image
        /// (black / white / #D3D3D3 become transparent like the Gumps tab) or copied image files.
        /// </summary>
        private void Import_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            int target = SelectedId >= 0 ? SelectedId : Math.Max(0, _library.FindNextFree(0));
            try
            {
                List<GumpClipboardItem> items = GumpLibraryClipboard.GetItems();
                if (items.Count > 0)
                {
                    PasteItems(items, target);
                    return;
                }

                Bitmap image = GumpLibraryClipboard.GetImage(ImportOptions);
                if (image != null)
                {
                    using (image)
                    {
                        CheckGumpSize(image);
                        if (!ConfirmReplace(target))
                        {
                            target = _library.FindNextFree(target);
                            if (target < 0)
                            {
                                return;
                            }
                        }

                        StoreGump(target, image);
                    }

                    MarkGraphicsChanged();
                    MetadataChanged();
                    RebuildView(target, new[] { target });
                    SelectId(target);
                    return;
                }

                List<string> files = GumpLibraryClipboard.GetImageFiles();
                if (files.Count > 0)
                {
                    ImportFiles(files, target);
                    return;
                }

                MessageBox.Show(this, "No image in the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Einfügen fehlgeschlagen:\n{ex.Message}", Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PasteItems(IReadOnlyList<GumpClipboardItem> items, int target)
        {
            int[] targets;
            if (items.Count == 1)
            {
                if (!ConfirmReplace(target))
                {
                    target = _library.FindNextFree(target);
                }

                targets = new[] { target };
            }
            else
            {
                DialogResult answer = MessageBox.Show(this,
                    $"{items.Count} Gumps einfügen.\n\nJa = auf ihre ursprünglichen IDs (belegte werden übersprungen)\nNein = auf freie IDs ab {ArtIdParser.ToHex(target)}",
                    Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (answer == DialogResult.Cancel)
                {
                    return;
                }

                targets = ArtLibraryTransfer.AssignTargetIds(items.Select(i => i.SourceId).ToList(), target, answer == DialogResult.Yes, false, _library.IsOccupied, _library.MaxId);
            }

            var known = new HashSet<int>(_library.Metadata.Categories.Select(c => c.Id));
            var errors = new List<string>();
            var pasted = new List<int>();
            BeginProgress(items.Count);
            try
            {
                for (int i = 0; i < items.Count; ++i)
                {
                    StepProgress(i);
                    if (targets[i] < 0)
                    {
                        errors.Add($"{ArtIdParser.ToHex(items[i].SourceId)}: keine freie Ziel-ID");
                        continue;
                    }

                    using (Bitmap gump = items[i].ToBitmap())
                    {
                        StoreGump(targets[i], gump);
                    }

                    using (Bitmap preview = items[i].PreviewToBitmap())
                    {
                        if (preview != null)
                        {
                            _library.SetPreview(targets[i], preview);
                        }
                    }

                    if (items[i].Info != null)
                    {
                        GumpLibraryItemInfo info = _library.Metadata.GetOrCreate(targets[i]);
                        GumpLibraryItemInfo source = items[i].Info;
                        if (string.IsNullOrEmpty(info.Name))
                        {
                            info.Name = source.Name ?? string.Empty;
                        }

                        if (string.IsNullOrEmpty(info.Description))
                        {
                            info.Description = source.Description ?? string.Empty;
                        }

                        info.SourceId ??= source.SourceId;
                        info.Favorite |= source.Favorite;
                        info.CategoryIds = info.CategoryIds.Concat(source.CategoryIds ?? new List<int>()).Where(known.Contains).Distinct().ToList();
                        _library.Metadata.SetTags(targets[i], info.Tags.Concat(source.Tags ?? new List<string>()));
                    }

                    pasted.Add(targets[i]);
                }
            }
            finally
            {
                EndProgress();
                MetadataChanged();
                MarkGraphicsChanged();
            }

            RebuildView(pasted.Count > 0 ? pasted[0] : -1, pasted);
            ReportResult($"{pasted.Count} Gump(s) eingefügt.", errors);
        }

        private void CopyIdHex_Click(object sender, EventArgs e)
        {
            if (SelectedId >= 0)
            {
                Clipboard.SetText(string.Join(", ", SelectedIds.Select(ArtIdParser.ToHex)));
            }
        }

        private void CopyIdDec_Click(object sender, EventArgs e)
        {
            if (SelectedId >= 0)
            {
                Clipboard.SetText(string.Join(", ", SelectedIds));
            }
        }

        #endregion

        #region [ Original gumps (gumpart.mul) ]

        private void FetchSelection_Click(object sender, EventArgs e)
        {
            int id = GumpControl.GetSelectedGumpId();
            if (id < 0)
            {
                MessageBox.Show(this, "Im Gumps-Tab ist kein Gump ausgewählt.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FetchFromOriginal(new[] { id }, keepIds: false, startId: SelectedId >= 0 ? SelectedId : -1, askWhenOccupied: true);
        }

        private void MiscFetchOriginal_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            if (!GumpLibraryTransfer.IsOriginalAvailable)
            {
                MessageBox.Show(this, "Es sind keine Original-Gumps geladen.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = "Gumps aus dem Original übernehmen";
                dialog.InfoText = "Gumps aus gumpart.mul in die Bibliothek kopieren.\nQuell-IDs: z. B. 0xC350-0xC360, 50000, 0x1F4" +
                                  (_settings.TakeNamesFromXml ? "\nNamen werden aus IDGumpNames.xml übernommen (Misc-Menü)." : string.Empty);
                dialog.SourceLabel = "Original-IDs:";
                int original = GumpControl.GetSelectedGumpId();
                dialog.SourceText = original >= 0 ? ArtIdParser.ToHex(original) : string.Empty;
                dialog.MaxSourceId = GumpLibraryTransfer.OriginalMaxId;
                dialog.ShowTransparencyOptions = false;
                dialog.KeepIdsText = "Gleiche IDs wie im Original";
                dialog.KeepIds = true;
                dialog.MaxTargetId = _library.MaxId;
                dialog.TargetId = SelectedId >= 0 ? SelectedId : 0;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                FetchFromOriginal(dialog.SourceIds, dialog.KeepIds, dialog.TargetId, askWhenOccupied: false, overwrite: dialog.Overwrite);
            }
        }

        /// <summary>
        /// Copies original gumps into the library. <paramref name="keepIds"/>: library id = original id.
        /// With <paramref name="askWhenOccupied"/> the user decides per occupied id (replace / next free).
        /// </summary>
        private void FetchFromOriginal(IReadOnlyList<int> originalIds, bool keepIds, int startId, bool askWhenOccupied, bool overwrite = false)
        {
            if (_library == null)
            {
                return;
            }

            ApplyDetails();
            List<int> sources = originalIds.Where(GumpLibraryTransfer.IsOriginalOccupied).ToList();
            int empty = originalIds.Count - sources.Count;
            if (sources.Count == 0)
            {
                MessageBox.Show(this, "Die angegebenen Original-IDs sind leer.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int[] targets;
            if (askWhenOccupied)
            {
                targets = new int[sources.Count];
                int next = startId >= 0 ? startId : 0;
                for (int i = 0; i < sources.Count; ++i)
                {
                    int target = keepIds || startId < 0 ? sources[i] : next++;
                    if (!_library.IsValidId(target))
                    {
                        target = _library.FindNextFree(0);
                    }
                    else if (_library.IsOccupied(target))
                    {
                        DialogResult answer = MessageBox.Show(this,
                            $"ID {ArtIdParser.ToDisplay(target)} ist in der Bibliothek belegt (\"{_library.Metadata.GetName(target)}\").\n\n" +
                            "Ja = ersetzen\nNein = auf die nächste freie ID\nAbbrechen = diesen Gump auslassen",
                            Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        target = answer == DialogResult.Yes ? target : answer == DialogResult.No ? _library.FindNextFree(target) : -1;
                    }

                    targets[i] = target;
                }
            }
            else
            {
                targets = ArtLibraryTransfer.AssignTargetIds(sources, startId, keepIds, overwrite, _library.IsOccupied, _library.MaxId);
            }

            var errors = new List<string>();
            var fetched = new List<int>();
            BeginProgress(sources.Count);
            try
            {
                for (int i = 0; i < sources.Count; ++i)
                {
                    StepProgress(i);
                    if (targets[i] < 0)
                    {
                        errors.Add($"{ArtIdParser.ToHex(sources[i])}: ausgelassen / keine freie Ziel-ID");
                        continue;
                    }

                    try
                    {
                        Bitmap original = GumpLibraryTransfer.GetOriginal(sources[i]);
                        if (original == null)
                        {
                            continue;
                        }

                        StoreGump(targets[i], original);
                        GumpLibraryItemInfo info = _library.Metadata.GetOrCreate(targets[i]);
                        info.SourceId = sources[i];
                        if (_settings.TakeNamesFromXml && string.IsNullOrEmpty(info.Name))
                        {
                            info.Name = GumpLibraryTransfer.GetOriginalName(sources[i]);
                        }

                        fetched.Add(targets[i]);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{ArtIdParser.ToHex(sources[i])}: {ex.Message}");
                    }
                }
            }
            finally
            {
                EndProgress();
                MetadataChanged();
                MarkGraphicsChanged();
            }

            if (empty > 0)
            {
                errors.Add($"{empty} leere Original-ID(s) übersprungen");
            }

            RebuildView(fetched.Count > 0 ? fetched[0] : -1, fetched);
            if (fetched.Count > 0)
            {
                SelectId(fetched[0]);
            }

            ReportResult($"{fetched.Count} Gump(s) aus dem Original übernommen.", errors);
        }

        private void WriteToOriginal_Click(object sender, EventArgs e)
        {
            List<int> ids = SelectedOccupiedIds;
            if (ids.Count == 0 || !GumpLibraryTransfer.IsOriginalAvailable)
            {
                return;
            }

            ApplyDetails();
            int maxOriginal = GumpLibraryTransfer.OriginalMaxId;
            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = "Ins Original schreiben";
                dialog.InfoText = $"{ids.Count} Gump(s) in die Original-Gumps schreiben (0 bis {ArtIdParser.ToHex(maxOriginal)}).\nDanach im Gumps-Tab speichern (Save)!";
                dialog.ShowSource = false;
                dialog.ShowTransparencyOptions = false;
                dialog.KeepIdsText = "Original-ID der Einträge verwenden (sonst gleiche ID wie in der Bibliothek)";
                dialog.MaxTargetId = maxOriginal;
                int first = _library.Metadata.Get(ids[0])?.SourceId ?? ids[0];
                dialog.TargetId = Math.Min(first, maxOriginal);
                dialog.KeepIds = ids.Count > 1;
                dialog.Overwrite = true;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<int> wanted = ids.Select(id => _library.Metadata.Get(id)?.SourceId ?? id).ToList();
                int[] targets = ArtLibraryTransfer.AssignTargetIds(wanted, dialog.TargetId, dialog.KeepIds, dialog.Overwrite,
                    GumpLibraryTransfer.IsOriginalOccupied, maxOriginal);
                WriteOriginal(ids, targets);
            }
        }

        private void WriteToSource_Click(object sender, EventArgs e)
        {
            List<int> ids = SelectedOccupiedIds.Where(id => _library.Metadata.Get(id)?.SourceId != null).ToList();
            if (ids.Count == 0)
            {
                MessageBox.Show(this, "Die ausgewählten Einträge haben keine Original-ID.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ApplyDetails();
            int[] targets = ids.Select(id => _library.Metadata.Get(id).SourceId.Value).ToArray();
            int occupied = targets.Count(GumpLibraryTransfer.IsOriginalOccupied);
            if (MessageBox.Show(this,
                    $"{ids.Count} Gump(s) auf ihre Original-ID zurückschreiben?\n{string.Join(", ", targets.Take(12).Select(ArtIdParser.ToHex))}{(targets.Length > 12 ? " ..." : string.Empty)}\n\n" +
                    (occupied > 0 ? $"{occupied} davon ersetzen einen vorhandenen Gump.\n" : string.Empty) + "Danach im Gumps-Tab speichern (Save).",
                    Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            WriteOriginal(ids, targets);
        }

        private void WriteOriginal(IReadOnlyList<int> ids, IReadOnlyList<int> targets)
        {
            var errors = new List<string>();
            int done = 0;
            BeginProgress(ids.Count);
            try
            {
                for (int i = 0; i < ids.Count; ++i)
                {
                    StepProgress(i);
                    if (targets[i] < 0)
                    {
                        errors.Add($"{ArtIdParser.ToHex(ids[i])}: keine gültige / freie Ziel-ID");
                        continue;
                    }

                    try
                    {
                        GumpLibraryTransfer.WriteToOriginal(targets[i], _library.GetGump(ids[i]), this);
                        _library.Metadata.GetOrCreate(ids[i]).SourceId = targets[i];
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
                EndProgress();
                MetadataChanged();
            }

            listBox.Invalidate();
            if (errors.Count == 0)
            {
                MessageBox.Show(this, $"{done} Gump(s) ins Original geschrieben.\nZum dauerhaften Speichern im Gumps-Tab \"Save\" klicken.", Title,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ReportResult($"{done} Gump(s) ins Original geschrieben.", errors);
            }
        }

        private void ShowInGumpsTab_Click(object sender, EventArgs e)
        {
            int id = SelectedId;
            int? source = id >= 0 ? _library?.Metadata.Get(id)?.SourceId : null;
            int target = source ?? id;
            if (target < 0 || !GumpLibraryTransfer.IsOriginalOccupied(target))
            {
                MessageBox.Show(this, "Zu diesem Eintrag gibt es keinen Original-Gump.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                GumpControl.Select(target);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Der Gumps-Tab ist nicht verfügbar:\n{ex.Message}", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Jump to Male/Female: paperdoll gumps 50000+ (male) and 60000+ (female). Looks for the partner in
        /// the library (by original id, then by library id); offers to take it over from the original.
        /// </summary>
        private void JumpToMaleFemale_Click(object sender, EventArgs e)
        {
            int id = SelectedId;
            int gumpId = GetGumpIdForGender(id);
            int partner = GumpLibraryTransfer.GetGenderPartner(gumpId);
            if (partner < 0)
            {
                return;
            }

            bool bySource = _library.Metadata.Get(id)?.SourceId != null;
            int found = bySource
                ? _library.GetOccupiedIds().FirstOrDefault(x => _library.Metadata.Get(x)?.SourceId == partner, -1)
                : _library.IsOccupied(partner) ? partner : -1;
            if (found >= 0)
            {
                SelectId(found);
                return;
            }

            if (GumpLibraryTransfer.IsOriginalOccupied(partner))
            {
                if (MessageBox.Show(this, $"Der Partner-Gump {ArtIdParser.ToDisplay(partner)} ist nicht in der Bibliothek.\nAus dem Original übernehmen?",
                        Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    FetchFromOriginal(new[] { partner }, keepIds: true, startId: -1, askWhenOccupied: true);
                }

                return;
            }

            MessageBox.Show(this, $"Es gibt keinen Partner-Gump {ArtIdParser.ToDisplay(partner)}.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MiscNamesFromXml_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            ApplyDetails();
            bool overwrite = MessageBox.Show(this, "Namen aus IDGumpNames.xml (Gumps-Tab \"Add ID Names\") übernehmen.\n" +
                                                   "Benutzt wird die Original-ID der Einträge (ohne Original-ID die eigene ID).\n\nAuch vorhandene Namen überschreiben?",
                Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
            IReadOnlyDictionary<int, string> names = GumpLibraryTransfer.GetOriginalNames();
            int count = 0;
            foreach (int id in _library.GetOccupiedIds())
            {
                GumpLibraryItemInfo info = _library.Metadata.Get(id);
                int key = info?.SourceId ?? id;
                if (names.TryGetValue(key, out string name) && !string.IsNullOrWhiteSpace(name) && (overwrite || string.IsNullOrEmpty(info?.Name)))
                {
                    _library.Metadata.GetOrCreate(id).Name = name;
                    ++count;
                }
            }

            MetadataChanged();
            RebuildView();
            MessageBox.Show(this, $"{count} Name(n) übernommen.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MiscNamesToXml_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            ApplyDetails();
            var names = new Dictionary<int, string>();
            foreach (int id in _library.GetOccupiedIds())
            {
                GumpLibraryItemInfo info = _library.Metadata.Get(id);
                if (info?.SourceId != null && !string.IsNullOrWhiteSpace(info.Name))
                {
                    names[info.SourceId.Value] = info.Name;
                }
            }

            if (names.Count == 0)
            {
                MessageBox.Show(this, "Keine Einträge mit Name und Original-ID.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, $"{names.Count} Name(n) in IDGumpNames.xml schreiben (vorhandene Namen dieser IDs werden ersetzt)?\n{GumpLibraryTransfer.NamesPath}\n\nDer Gumps-Tab zeigt sie nach dem nächsten Neuladen.",
                    Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int count = GumpLibraryTransfer.WriteOriginalNames(names);
                MessageBox.Show(this, $"{count} Name(n) geschrieben.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region [ Edit image (mirror, rotate, trim) ]

        private void TransformSelected(Func<Bitmap, Bitmap> transform)
        {
            List<int> ids = SelectedOccupiedIds;
            if (ids.Count == 0)
            {
                return;
            }

            try
            {
                foreach (int id in ids)
                {
                    Bitmap gump = _library.GetGump(id);
                    if (gump == null)
                    {
                        continue;
                    }

                    using (Bitmap result = transform(gump))
                    {
                        StoreGump(id, result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MetadataChanged();
            MarkGraphicsChanged();
            ShowSelected();
        }

        private void MirrorH_Click(object sender, EventArgs e) => TransformSelected(b => GumpImageTools.Mirror(b, true));

        private void MirrorV_Click(object sender, EventArgs e) => TransformSelected(b => GumpImageTools.Mirror(b, false));

        private void RotateRight_Click(object sender, EventArgs e) => TransformSelected(b => GumpImageTools.Rotate(b, true));

        private void RotateLeft_Click(object sender, EventArgs e) => TransformSelected(b => GumpImageTools.Rotate(b, false));

        private void Trim_Click(object sender, EventArgs e) => TransformSelected(GumpImageTools.Trim);

        #endregion

        #region [ Preview images ]

        private void SetPreviewFor(int id, Bitmap preview)
        {
            if (_library == null || id < 0 || !_library.IsOccupied(id))
            {
                return;
            }

            CheckGumpSize(preview);
            _library.SetPreview(id, preview);
            MetadataChanged();
            MarkGraphicsChanged();
            LoadDetails(id);
            listBox.Invalidate();
        }

        private void PreviewFromFile_Click(object sender, EventArgs e)
        {
            int id = SelectedId;
            if (_library == null || !_library.IsOccupied(id))
            {
                return;
            }

            string[] files = ChooseImageFiles(false, "Vorschaubild wählen");
            if (files == null)
            {
                return;
            }

            try
            {
                using (Bitmap preview = LoadImageFile(files[0]))
                {
                    SetPreviewFor(id, preview);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PreviewFromClipboard_Click(object sender, EventArgs e)
        {
            int id = SelectedId;
            if (_library == null || !_library.IsOccupied(id))
            {
                return;
            }

            try
            {
                Bitmap image = GumpLibraryClipboard.GetItems().FirstOrDefault()?.ToBitmap() ?? GumpLibraryClipboard.GetImage(ImportOptions);
                if (image == null)
                {
                    List<string> files = GumpLibraryClipboard.GetImageFiles();
                    image = files.Count > 0 ? LoadImageFile(files[0]) : null;
                }

                if (image == null)
                {
                    MessageBox.Show(this, "No image in the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (image)
                {
                    SetPreviewFor(id, image);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Creates scaled down previews (largest side = setting) for all selected gumps.</summary>
        private void PreviewFromGump_Click(object sender, EventArgs e)
        {
            List<int> ids = SelectedOccupiedIds;
            if (ids.Count == 0)
            {
                return;
            }

            int replaced = ids.Count(_library.HasPreview);
            if (replaced > 0 && MessageBox.Show(this, $"{replaced} der ausgewählten Gumps haben schon ein eigenes Vorschaubild. Ersetzen?",
                    Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                ids = ids.Where(id => !_library.HasPreview(id)).ToList();
            }

            foreach (int id in ids)
            {
                Bitmap gump = _library.GetGump(id);
                if (gump == null)
                {
                    continue;
                }

                using (Bitmap preview = GumpImageTools.CreatePreview(GumpImageTools.Trim(gump), _settings.PreviewMaxSize))
                {
                    _library.SetPreview(id, preview);
                }
            }

            MetadataChanged();
            MarkGraphicsChanged();
            LoadDetails(SelectedId);
            listBox.Invalidate();
        }

        private void PreviewCopy_Click(object sender, EventArgs e)
        {
            Bitmap preview = _library?.GetPreview(SelectedId);
            if (preview == null)
            {
                MessageBox.Show(this, "Der Eintrag hat kein eigenes Vorschaubild.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            GumpLibraryClipboard.CopyImage(preview);
        }

        private void PreviewRemove_Click(object sender, EventArgs e)
        {
            List<int> ids = SelectedOccupiedIds.Where(_library.HasPreview).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            if (ids.Count > 1 && MessageBox.Show(this, $"Eigene Vorschaubilder von {ids.Count} Gumps entfernen?", Title,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            foreach (int id in ids)
            {
                _library.RemovePreview(id);
            }

            MarkGraphicsChanged();
            LoadDetails(SelectedId);
            listBox.Invalidate();
        }

        private void PictureBoxPreview_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = GetDroppedImageFiles(e);
            if (files.Length == 0)
            {
                return;
            }

            try
            {
                using (Bitmap preview = LoadImageFile(files[0]))
                {
                    SetPreviewFor(SelectedId, preview);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region [ Remove / free slots / copy / move ]

        private void Remove_Click(object sender, EventArgs e)
        {
            List<int> ids = SelectedOccupiedIds;
            if (ids.Count == 0)
            {
                return;
            }

            string text = ids.Count == 1
                ? $"Are you sure to remove {ArtIdParser.ToDisplay(ids[0])} {_library.Metadata.GetName(ids[0])}?\n(Gump, Vorschaubild und Metadaten)"
                : $"{ids.Count} Gumps aus der Bibliothek entfernen?\n(Gump, Vorschaubild und Metadaten)";
            if (MessageBox.Show(this, text, "Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            int index = listBox.SelectedIndex;
            _detailsDirty = false;
            foreach (int id in ids)
            {
                _library.Remove(id);
            }

            MetadataChanged();
            MarkGraphicsChanged();
            RebuildView(-1, Array.Empty<int>());
            if (_view.Count > 0)
            {
                listBox.ClearSelected();
                listBox.SetSelected(Math.Min(Math.Max(0, index), _view.Count - 1), true);
            }
        }

        private void SetShowFreeSlots(bool value, bool rebuild = true)
        {
            _settings.ShowFreeSlots = value;
            _settings.Save();
            miscShowFreeSlots.Checked = value;
            showFreeSlotsMenuItem.Checked = value;
            if (rebuild)
            {
                RebuildView();
            }
        }

        private void MiscShowFreeSlots_Click(object sender, EventArgs e)
        {
            SetShowFreeSlots(!_settings.ShowFreeSlots);
            if (_settings.ShowFreeSlots && IsFiltered)
            {
                CategoryLabel.Text = "Freie IDs werden nur ohne Suche/Filter angezeigt.";
            }
        }

        private void FindNextFreeSlot_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            int start = Math.Max(0, SelectedId + 1);
            int free = _library.FindNextFree(start);
            if (free < 0)
            {
                free = _library.FindNextFree(0);
            }

            if (free < 0)
            {
                MessageBox.Show(this, "Die Bibliothek ist voll (Bibliothek > Erweitern).", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_settings.ShowFreeSlots)
            {
                SetShowFreeSlots(true, false);
            }

            SelectId(free);
        }

        private void Duplicate_Click(object sender, EventArgs e)
        {
            List<int> ids = SelectedOccupiedIds;
            if (ids.Count == 0)
            {
                return;
            }

            var created = new List<int>();
            foreach (int id in ids)
            {
                int target = _library.FindNextFree(id + 1);
                if (target < 0)
                {
                    target = _library.FindNextFree(0);
                }

                if (target < 0)
                {
                    break;
                }

                _library.Copy(id, target);
                GumpLibraryItemInfo info = _library.Metadata.Get(target);
                if (info != null && info.Name.Length > 0)
                {
                    info.Name += " (Kopie)";
                }

                created.Add(target);
            }

            MetadataChanged();
            MarkGraphicsChanged();
            RebuildView(created.Count > 0 ? created[0] : -1, created);
        }

        private void CopyOrMove(bool move)
        {
            List<int> ids = SelectedOccupiedIds;
            if (ids.Count == 0)
            {
                return;
            }

            ApplyDetails();
            using (var dialog = new ArtLibraryTargetIdForm())
            {
                dialog.Title = move ? "Verschieben nach ID" : "Kopieren nach ID";
                dialog.InfoText = $"{ids.Count} Eintrag/Einträge mit Gump, Vorschaubild und Metadaten {(move ? "verschieben" : "kopieren")}.";
                dialog.ShowSource = false;
                dialog.ShowTransparencyOptions = false;
                dialog.ShowModeOptions = false;
                dialog.MaxTargetId = _library.MaxId;
                dialog.TargetId = Math.Max(0, _library.FindNextFree(ids[ids.Count - 1] + 1));
                dialog.Overwrite = false;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var source = new HashSet<int>(ids);
                int[] targets = ArtLibraryTransfer.AssignTargetIds(ids, dialog.TargetId, false, dialog.Overwrite,
                    id => _library.IsOccupied(id) && !(move && source.Contains(id)), _library.MaxId);
                var done = new List<int>();
                var errors = new List<string>();
                for (int i = 0; i < ids.Count; ++i)
                {
                    if (targets[i] < 0)
                    {
                        errors.Add($"{ArtIdParser.ToHex(ids[i])}: keine freie Ziel-ID");
                        continue;
                    }

                    if (targets[i] == ids[i])
                    {
                        done.Add(ids[i]);
                        continue;
                    }

                    if (move)
                    {
                        _library.Move(ids[i], targets[i]);
                    }
                    else
                    {
                        _library.Copy(ids[i], targets[i]);
                    }

                    done.Add(targets[i]);
                }

                _detailsDirty = false;
                MetadataChanged();
                MarkGraphicsChanged();
                RebuildView(done.Count > 0 ? done[0] : -1, done);
                ReportResult($"{done.Count} Eintrag/Einträge {(move ? "verschoben" : "kopiert")}.", errors);
            }
        }

        private void CopyToId_Click(object sender, EventArgs e) => CopyOrMove(false);

        private void MoveToId_Click(object sender, EventArgs e) => CopyOrMove(true);

        private void Swap_Click(object sender, EventArgs e)
        {
            List<int> ids = SelectedIds;
            int first, second;
            if (ids.Count == 2)
            {
                first = ids[0];
                second = ids[1];
            }
            else if (ids.Count == 1)
            {
                string text = GumpLibraryInputForm.Ask(this, "Tauschen", $"{ArtIdParser.ToDisplay(ids[0])} tauschen mit ID:");
                if (text == null)
                {
                    return;
                }

                if (!ArtIdParser.TryParse(text, out second) || !_library.IsValidId(second))
                {
                    MessageBox.Show(this, "Ungültige ID.", Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                first = ids[0];
            }
            else
            {
                MessageBox.Show(this, "Bitte einen oder zwei Einträge auswählen.", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ApplyDetails();
            _detailsDirty = false;
            _library.Swap(first, second);
            MetadataChanged();
            MarkGraphicsChanged();
            RebuildView(second, new[] { second });
        }

        #endregion

        #region [ Favorites, categories, tags ]

        private void Favorite_Click(object sender, EventArgs e)
        {
            List<int> ids = SelectedOccupiedIds;
            if (ids.Count == 0)
            {
                return;
            }

            ApplyDetails();
            bool value = !ids.All(id => _library.Metadata.Get(id)?.Favorite == true);
            foreach (int id in ids)
            {
                _library.Metadata.GetOrCreate(id).Favorite = value;
            }

            MetadataChanged();
            LoadDetails(SelectedId);
            listBox.Invalidate();
        }

        private void BuildCategoryMenu()
        {
            assignCategoryMenuItem.DropDownItems.Clear();
            List<int> ids = SelectedOccupiedIds;
            if (_library == null)
            {
                return;
            }

            foreach ((GumpLibraryCategory category, int depth) in _library.Metadata.GetCategoryTree())
            {
                int with = ids.Count(id => _library.Metadata.Get(id)?.CategoryIds.Contains(category.Id) == true);
                var item = new ToolStripMenuItem(new string(' ', depth * 4) + category.Name)
                {
                    Tag = category.Id,
                    CheckState = with == 0 ? CheckState.Unchecked : with == ids.Count ? CheckState.Checked : CheckState.Indeterminate,
                    ToolTipText = category.Description
                };
                if (category.Color != null)
                {
                    item.ForeColor = Color.FromArgb(category.Color.Value);
                }

                item.Click += AssignCategoryItem_Click;
                assignCategoryMenuItem.DropDownItems.Add(item);
            }

            if (assignCategoryMenuItem.DropDownItems.Count > 0)
            {
                assignCategoryMenuItem.DropDownItems.Add(new ToolStripSeparator());
            }

            var add = new ToolStripMenuItem("Neue Kategorie...") { Image = Properties.Resources.Add };
            add.Click += NewCategoryAndAssign_Click;
            assignCategoryMenuItem.DropDownItems.Add(add);
            var manage = new ToolStripMenuItem("Kategorien verwalten...") { Image = Properties.Resources.Edit };
            manage.Click += ManageCategories_Click;
            assignCategoryMenuItem.DropDownItems.Add(manage);
        }

        private void AssignCategoryItem_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem item) || !(item.Tag is int categoryId))
            {
                return;
            }

            ApplyDetails();
            List<int> ids = SelectedOccupiedIds;
            bool assign = item.CheckState != CheckState.Checked;
            _library.Metadata.AssignCategory(ids, categoryId, assign);
            MetadataChanged();
            _detailsDirty = false;
            RebuildView();
        }

        private void NewCategoryAndAssign_Click(object sender, EventArgs e)
        {
            string name = GumpLibraryInputForm.Ask(this, "Neue Kategorie", "Name der neuen Kategorie (die ausgewählten Gumps kommen hinein):");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            ApplyDetails();
            GumpLibraryCategory category = _library.Metadata.AddCategory(name);
            _library.Metadata.AssignCategory(SelectedOccupiedIds, category.Id, true);
            MetadataChanged();
            FillCategoryFilter();
            _detailsDirty = false;
            RebuildView();
        }

        private void BuildRemoveTagMenu()
        {
            removeTagMenuItem.DropDownItems.Clear();
            if (_library == null)
            {
                return;
            }

            List<string> tags = SelectedOccupiedIds
                .SelectMany(id => _library.Metadata.Get(id)?.Tags ?? new List<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            foreach (string tag in tags)
            {
                var item = new ToolStripMenuItem(tag) { Tag = tag };
                item.Click += RemoveTagItem_Click;
                removeTagMenuItem.DropDownItems.Add(item);
            }

            removeTagMenuItem.Enabled = tags.Count > 0;
        }

        private void RemoveTagItem_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem item) || !(item.Tag is string tag))
            {
                return;
            }

            ApplyDetails();
            foreach (int id in SelectedOccupiedIds)
            {
                GumpLibraryItemInfo info = _library.Metadata.Get(id);
                if (info != null && info.Tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)) > 0)
                {
                    info.Modified = DateTime.Now;
                }
            }

            MetadataChanged();
            _detailsDirty = false;
            RebuildView();
        }

        private void AddTags_Click(object sender, EventArgs e)
        {
            List<int> ids = SelectedOccupiedIds;
            if (ids.Count == 0)
            {
                return;
            }

            ApplyDetails();
            string text = GumpLibraryInputForm.Ask(this, "Tags hinzufügen", $"Tags für {ids.Count} Gump(s), mehrere mit Komma trennen:");
            List<string> tags = GumpLibraryMetadata.ParseTags(text);
            if (tags.Count == 0)
            {
                return;
            }

            foreach (int id in ids)
            {
                GumpLibraryItemInfo info = _library.Metadata.GetOrCreate(id);
                _library.Metadata.SetTags(id, info.Tags.Concat(tags));
            }

            MetadataChanged();
            _detailsDirty = false;
            RebuildView();
        }

        #endregion

        #region [ Own sort order ]

        /// <summary>Switches the list to "Eigene Reihenfolge" (ascending) if needed. True when it was switched.</summary>
        private bool EnsureCustomSort()
        {
            if (SortMode == GumpSortMode.Custom && !_settings.SortDescending)
            {
                return false;
            }

            SetSortMode(GumpSortMode.Custom, false);
            RebuildView();
            CategoryLabel.Text = "Sortierung auf \"Eigene Reihenfolge\" umgestellt.";
            return true;
        }

        /// <summary>
        /// Applies a new order of the visible entries. Hidden entries (filter) keep their places: the visible
        /// ones are written back into the positions they had in the complete custom order.
        /// </summary>
        private void ApplyViewOrder(IReadOnlyList<int> newViewOrder)
        {
            GumpLibraryMetadata meta = _library.Metadata;
            List<int> full = GumpLibrarySearch.Sort(_library.GetOccupiedIds(), meta, GumpSortMode.Custom, false);
            var visible = new HashSet<int>(newViewOrder);
            var positions = new List<int>();
            for (int i = 0; i < full.Count; ++i)
            {
                if (visible.Contains(full[i]))
                {
                    positions.Add(i);
                }
            }

            for (int i = 0; i < positions.Count && i < newViewOrder.Count; ++i)
            {
                full[positions[i]] = newViewOrder[i];
            }

            meta.ApplyOrder(full);
            MetadataChanged();
        }

        private void MoveSelectionInOrder(int direction, bool toEnd)
        {
            if (_library == null || SelectedOccupiedIds.Count == 0)
            {
                return;
            }

            ApplyDetails();
            _detailsDirty = false;
            EnsureCustomSort();
            List<int> selected = SelectedOccupiedIds;
            var set = new HashSet<int>(selected);
            List<int> view = _view.Where(_library.IsOccupied).ToList();

            if (toEnd)
            {
                List<int> rest = view.Where(id => !set.Contains(id)).ToList();
                List<int> moved = view.Where(set.Contains).ToList();
                view = direction < 0 ? moved.Concat(rest).ToList() : rest.Concat(moved).ToList();
            }
            else if (direction < 0)
            {
                for (int i = 1; i < view.Count; ++i)
                {
                    if (set.Contains(view[i]) && !set.Contains(view[i - 1]))
                    {
                        (view[i - 1], view[i]) = (view[i], view[i - 1]);
                    }
                }
            }
            else
            {
                for (int i = view.Count - 2; i >= 0; --i)
                {
                    if (set.Contains(view[i]) && !set.Contains(view[i + 1]))
                    {
                        (view[i + 1], view[i]) = (view[i], view[i + 1]);
                    }
                }
            }

            ApplyViewOrder(view);
            RebuildView(SelectedId, selected);
        }

        private void OrderUp_Click(object sender, EventArgs e) => MoveSelectionInOrder(-1, false);

        private void OrderDown_Click(object sender, EventArgs e) => MoveSelectionInOrder(1, false);

        private void OrderTop_Click(object sender, EventArgs e) => MoveSelectionInOrder(-1, true);

        private void OrderBottom_Click(object sender, EventArgs e) => MoveSelectionInOrder(1, true);

        /// <summary>Takes the current view (any sort mode / filter) as the own order.</summary>
        private void OrderFromView_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            if (MessageBox.Show(this, $"Die aktuelle Reihenfolge der Liste ({GumpLibrarySearch.GetSortModeText(SortMode)}) als eigene Reihenfolge übernehmen?",
                    Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            ApplyDetails();
            _detailsDirty = false;
            List<int> view = _view.Where(_library.IsOccupied).ToList();
            var set = new HashSet<int>(view);
            List<int> rest = GumpLibrarySearch.Sort(_library.GetOccupiedIds().Where(id => !set.Contains(id)), _library.Metadata, GumpSortMode.Custom, false);
            _library.Metadata.ApplyOrder(view.Concat(rest));
            MetadataChanged();
            SetSortMode(GumpSortMode.Custom, false);
            RebuildView();
        }

        #endregion

        #region [ Context menu / keys ]

        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_library == null)
            {
                e.Cancel = true;
                return;
            }

            int id = SelectedId;
            List<int> occupied = SelectedOccupiedIds;
            bool has = occupied.Count > 0;
            bool single = has && occupied.Count == 1;

            extractImageToolStripMenuItem.Enabled = has;
            mirrorToolStripMenuItem.Enabled = has;
            editImageMenuItem.Enabled = has;
            replaceGumpToolStripMenuItem.Enabled = id >= 0;
            removeToolStripMenuItem.Enabled = has;
            copyToolStripMenuItem.Enabled = has;
            importToolStripMenuItem.Enabled = GumpLibraryClipboard.ContainsGumps();
            favoriteMenuItem.Enabled = has;
            favoriteMenuItem.Checked = has && occupied.All(x => _library.Metadata.Get(x)?.Favorite == true);
            editPropertiesMenuItem.Enabled = single;
            assignCategoryMenuItem.Enabled = has;
            addTagsMenuItem.Enabled = has;
            previewMenuItem.Enabled = has;
            previewFromFileMenuItem.Enabled = single;
            previewFromClipboardMenuItem.Enabled = single;
            previewCopyMenuItem.Enabled = single && _library.HasPreview(id);
            previewRemoveMenuItem.Enabled = occupied.Any(_library.HasPreview);
            orderMenuItem.Enabled = has;
            duplicateMenuItem.Enabled = has;
            copyToIdMenuItem.Enabled = has;
            moveToIdMenuItem.Enabled = has;
            swapMenuItem.Enabled = SelectedIds.Count is 1 or 2;
            bool original = GumpLibraryTransfer.IsOriginalAvailable;
            originalMenuItem.Enabled = original;
            writeToOriginalMenuItem.Enabled = has;
            writeToSourceMenuItem.Enabled = occupied.Any(x => _library.Metadata.Get(x)?.SourceId != null);
            int source = id >= 0 ? _library.Metadata.Get(id)?.SourceId ?? id : -1;
            showInGumpsTabMenuItem.Enabled = original && source >= 0 && GumpLibraryTransfer.IsOriginalOccupied(source);
            showFreeSlotsMenuItem.Checked = _settings.ShowFreeSlots;
            InsertText.Text = id >= 0 && !_library.IsOccupied(id) ? ArtIdParser.ToHex(id) : string.Empty;
            JumpToMaleFemaleInvalidate();
            BuildCategoryMenu();
            BuildRemoveTagMenu();
        }

        private void GumpLibraryForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                ToolStripButtonHelp_Click(sender, e);
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                SaveLibrary();
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                toolStripTextBoxFilter.Focus();
                toolStripTextBoxFilter.SelectAll();
            }
            else if (e.Control && e.KeyCode == Keys.G)
            {
                searchByIdToolStripTextBox.Focus();
                searchByIdToolStripTextBox.SelectAll();
            }
            else if (e.Control && e.KeyCode == Keys.Enter)
            {
                ApplyDetails();
            }
            else
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                Copy_Click(sender, e);
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                Import_Click(sender, e);
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                listBox.BeginUpdate();
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    listBox.SetSelected(i, true);
                }

                listBox.EndUpdate();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                Remove_Click(sender, e);
            }
            else if (e.KeyCode == Keys.F2)
            {
                EditProperties_Click(sender, e);
            }
            else if (e.Alt && e.KeyCode == Keys.Up)
            {
                MoveSelectionInOrder(-1, false);
            }
            else if (e.Alt && e.KeyCode == Keys.Down)
            {
                MoveSelectionInOrder(1, false);
            }
            else
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void ListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox.IndexFromPoint(e.Location);
            if (index < 0 || index >= _view.Count)
            {
                return;
            }

            if (_library.IsOccupied(_view[index]))
            {
                EditProperties_Click(sender, e);
            }
            else
            {
                InsertText.Text = ArtIdParser.ToHex(_view[index]);
                Replace_Click(sender, e);
            }
        }

        private void SearchByIdToolStripTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ToolStripButtonSearch_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>Index search like the Gumps tab: jumps to the id (also free ids).</summary>
        private void ToolStripButtonSearch_Click(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            if (!ArtIdParser.TryParse(searchByIdToolStripTextBox.Text, out int id) || !_library.IsValidId(id))
            {
                searchByIdToolStripTextBox.ForeColor = Color.Red;
                return;
            }

            searchByIdToolStripTextBox.ForeColor = SystemColors.WindowText;
            SelectId(id);
            listBox.Focus();
        }

        private void Background_Click(object sender, EventArgs e)
        {
            colorDialog.Color = pictureBox.BackColor;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            pictureBox.BackColor = colorDialog.Color;
            pictureBoxPreview.BackColor = colorDialog.Color;
            _settings.BackgroundArgb = colorDialog.Color.ToArgb();
            _settings.Save();
        }

        private void ToolStripButtonHelp_Click(object sender, EventArgs e)
        {
            using (var help = new GumpLibraryHelpForm())
            {
                help.ShowDialog(this);
            }
        }

        #endregion

        #region [ Misc options ]

        private void MiscUsePreview_Click(object sender, EventArgs e)
        {
            _settings.UsePreviewInList = !_settings.UsePreviewInList;
            miscUsePreview.Checked = _settings.UsePreviewInList;
            _settings.Save();
            listBox.Invalidate();
        }

        private void MiscTakeNames_Click(object sender, EventArgs e)
        {
            _settings.TakeNamesFromXml = !_settings.TakeNamesFromXml;
            miscTakeNames.Checked = _settings.TakeNamesFromXml;
            _settings.Save();
        }

        private void MiscImportOption_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                item.Checked = !item.Checked;
            }

            _settings.ImportBlackTransparent = miscImportBlack.Checked;
            _settings.ImportWhiteTransparent = miscImportWhite.Checked;
            _settings.ImportLightGrayTransparent = miscImportGray.Checked;
            _settings.Save();
        }

        private void MiscPreviewSize_Click(object sender, EventArgs e)
        {
            string text = GumpLibraryInputForm.Ask(this, "Vorschaubild-Größe",
                "Größte Seite eines erzeugten Vorschaubildes in Pixel (16 bis 1024).\nDie Liste zeigt Bilder bis 100 x 54 Pixel.",
                _settings.PreviewMaxSize.ToString(CultureInfo.InvariantCulture));
            if (text == null)
            {
                return;
            }

            if (!int.TryParse(text, out int size) || size < 16 || size > 1024)
            {
                MessageBox.Show(this, "Bitte eine Zahl von 16 bis 1024 eingeben.", Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _settings.PreviewMaxSize = size;
            _settings.Save();
        }

        #endregion

        #region [ Drag & drop ]

        private static string[] GetDroppedImageFiles(DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] files))
            {
                return Array.Empty<string>();
            }

            return files.Where(f => File.Exists(f) && ArtLibraryImageConverter.IsSupportedFile(f)).ToArray();
        }

        private void PictureBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = _library != null && e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        /// <summary>Image dropped on the big picture: replaces the selected gump (or fills the free id).</summary>
        private void PictureBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = GetDroppedImageFiles(e);
            if (files.Length == 0 || _library == null)
            {
                return;
            }

            if (files.Length > 1)
            {
                ImportFiles(files);
                return;
            }

            int id = SelectedId >= 0 ? SelectedId : _library.FindNextFree(0);
            if (id < 0 || !ConfirmReplace(id))
            {
                return;
            }

            try
            {
                using (Bitmap bitmap = LoadImageFile(files[0]))
                {
                    StoreGump(id, bitmap);
                }

                GumpLibraryItemInfo info = _library.Metadata.GetOrCreate(id);
                if (string.IsNullOrEmpty(info.Name))
                {
                    info.Name = Path.GetFileNameWithoutExtension(files[0]);
                }

                MetadataChanged();
                MarkGraphicsChanged();
                RebuildView(id, new[] { id });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ListBox_MouseDown(object sender, MouseEventArgs e)
        {
            int index = listBox.IndexFromPoint(e.Location);
            _dragIndex = e.Button == MouseButtons.Left ? index : -1;
            _dragStart = e.Location;

            // right click on a row that is not selected selects it (the context menu works on the selection)
            if (e.Button == MouseButtons.Right && index >= 0 && index < _view.Count && !listBox.GetSelected(index))
            {
                listBox.ClearSelected();
                listBox.SetSelected(index, true);
            }
        }

        /// <summary>
        /// Dragging rows = moving them in the own order. Only in the sort mode "Eigene Reihenfolge" -
        /// in the other modes dragging selects several rows as usual.
        /// </summary>
        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragIndex < 0 || e.Button != MouseButtons.Left || _library == null ||
                SortMode != GumpSortMode.Custom || _settings.SortDescending || (_settings.ShowFreeSlots && !IsFiltered))
            {
                return;
            }

            Size drag = SystemInformation.DragSize;
            if (Math.Abs(e.X - _dragStart.X) < drag.Width && Math.Abs(e.Y - _dragStart.Y) < drag.Height * 3)
            {
                return;
            }

            List<int> ids = SelectedOccupiedIds;
            _dragIndex = -1;
            if (ids.Count == 0)
            {
                return;
            }

            var data = new DataObject();
            data.SetData(DragFormat, string.Join(",", ids));
            listBox.DoDragDrop(data, DragDropEffects.Move);
        }

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            ListBox_DragOver(sender, e);
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            if (_library == null)
            {
                e.Effect = DragDropEffects.None;
            }
            else if (e.Data.GetDataPresent(DragFormat))
            {
                e.Effect = DragDropEffects.Move;
                Point point = listBox.PointToClient(new Point(e.X, e.Y));
                if (point.Y < RowHeight / 2 && listBox.TopIndex > 0)
                {
                    listBox.TopIndex--;
                }
                else if (point.Y > listBox.ClientSize.Height - (RowHeight / 2))
                {
                    listBox.TopIndex++;
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            Point point = listBox.PointToClient(new Point(e.X, e.Y));
            int index = listBox.IndexFromPoint(point);
            if (e.Data.GetDataPresent(DragFormat))
            {
                List<int> moved = (e.Data.GetData(DragFormat) as string ?? string.Empty)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();
                int targetId = index >= 0 && index < _view.Count ? _view[index] : -1;
                if (moved.Count == 0 || moved.Contains(targetId))
                {
                    return;
                }

                ApplyDetails();
                _detailsDirty = false;
                EnsureCustomSort();
                var set = new HashSet<int>(moved);
                List<int> view = _view.Where(id => _library.IsOccupied(id) && !set.Contains(id)).ToList();
                int insert = targetId >= 0 ? view.IndexOf(targetId) : view.Count;
                if (insert < 0)
                {
                    insert = view.Count;
                }

                // dropped on the lower half of a row = behind it
                if (index >= 0 && targetId >= 0 && point.Y - listBox.GetItemRectangle(index).Top > RowHeight / 2)
                {
                    ++insert;
                }

                view.InsertRange(Math.Min(insert, view.Count), moved);
                ApplyViewOrder(view);
                RebuildView(moved[0], moved);
                return;
            }

            string[] files = GetDroppedImageFiles(e);
            if (files.Length > 0)
            {
                int target = index >= 0 && index < _view.Count ? _view[index] : -1;
                ImportFiles(files, target);
            }
        }

        #endregion
    }
}
