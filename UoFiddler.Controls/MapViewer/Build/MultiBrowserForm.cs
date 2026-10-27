/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MapViewer.Build
{
    /// <summary>
    /// Browses the multis of the client (multi.mul) and the own building library, imports/exports
    /// buildings and hands a building over to the region viewer for placing.
    /// </summary>
    public partial class MultiBrowserForm : Form
    {
        private sealed class ClientMulti
        {
            public int Index;
            public string Name;
            public int Parts;
            public string Size;
        }

        private readonly Action<MultiTemplate> _placeAction;
        private readonly ArtPixelCache _art = new ArtPixelCache();
        private List<ClientMulti> _clientAll;
        private List<ClientMulti> _clientShown = new List<ClientMulti>();
        private List<MultiLibraryEntry> _libraryShown = new List<MultiLibraryEntry>();
        private MultiLibrary _library;
        private MultiTemplate _current;
        private bool _busy;

        /// <summary>Only for the designer</summary>
        public MultiBrowserForm()
        {
            InitializeComponent();
        }

        /// <param name="placeAction">called with a copy of the template when "Place on map" is clicked (null = no placing)</param>
        public MultiBrowserForm(Action<MultiTemplate> placeAction)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _placeAction = placeAction;
            buttonPlace.Enabled = placeAction != null;
            comboBoxTxtIds.SelectedIndex = 0;
        }

        /// <summary>Opens the library tab first</summary>
        public void ShowLibrary()
        {
            tabControl.SelectedTab = tabPageLibrary;
        }

        private void DisposeResources()
        {
            pictureBoxPreview?.Image?.Dispose();
            _library?.Dispose();
            _library = null;
        }

        private void OnShown(object sender, EventArgs e)
        {
            OpenLibrary(MultiLibraryLocation.CurrentFile);
            if (tabControl.SelectedTab == tabPageClient)
            {
                LoadClientMultis();
            }

            UpdateButtons();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                _library?.Flush();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool IsLibraryTab => tabControl.SelectedTab == tabPageLibrary;

        private void OnTabChanged(object sender, EventArgs e)
        {
            if (!IsLibraryTab)
            {
                LoadClientMultis();
            }

            SetCurrent(null);
            UpdateButtons();
        }

        #region Client multis

        private void LoadClientMultis()
        {
            if (_clientAll != null)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Dictionary<int, string> names = LoadMultiNames();
                _clientAll = new List<ClientMulti>();
                for (int i = 0; i < Multis.MaximumMultiIndex; i++)
                {
                    MultiComponentList list = Multis.GetComponents(i);
                    if (list == null || list == MultiComponentList.Empty || list.SortedTiles == null || list.SortedTiles.Length == 0)
                    {
                        continue;
                    }

                    _clientAll.Add(new ClientMulti
                    {
                        Index = i,
                        Name = names.TryGetValue(i, out string n) ? n : string.Empty,
                        Parts = list.SortedTiles.Length,
                        Size = $"{list.Width}x{list.Height}"
                    });
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            FilterClient();
        }

        private static Dictionary<int, string> LoadMultiNames()
        {
            var names = new Dictionary<int, string>();
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Multilist.xml");
            if (!File.Exists(file))
            {
                return names;
            }

            try
            {
                var doc = new XmlDocument();
                doc.Load(file);
                foreach (XmlElement e in doc.SelectNodes("/Multis/Multi"))
                {
                    if (int.TryParse(e.GetAttribute("id"), out int id))
                    {
                        names[id] = e.GetAttribute("name");
                    }
                }
            }
            catch (Exception)
            {
                // names are optional
            }

            return names;
        }

        private void FilterClient()
        {
            if (_clientAll == null)
            {
                return;
            }

            string search = textBoxClientSearch.Text.Trim();
            IEnumerable<ClientMulti> query = _clientAll;
            if (search.Length > 0)
            {
                int? id = ParseId(search);
                query = query.Where(m => (id.HasValue && m.Index == id.Value) ||
                                         m.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _clientShown = query.ToList();
            listViewClient.SelectedIndices.Clear();
            listViewClient.VirtualListSize = _clientShown.Count;
            listViewClient.Invalidate();
            UpdateStatus();
        }

        private void OnRetrieveClientItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            ClientMulti m = _clientShown[e.ItemIndex];
            var item = new ListViewItem($"{m.Index} (0x{m.Index:X})");
            item.SubItems.Add(m.Name);
            item.SubItems.Add(m.Parts.ToString());
            item.SubItems.Add(m.Size);
            e.Item = item;
        }

        private static int? ParseId(string text)
        {
            text = text.TrimStart('#');
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hex))
            {
                return hex;
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int dec) ? dec : null;
        }

        #endregion

        #region Library

        private void OpenLibrary(string file)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                _library?.Dispose();
                _library = new MultiLibrary(file);

                MultiLibraryLocation.CurrentFile = file;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The library could not be opened:\n{file}\n\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!string.Equals(file, MultiLibrary.DefaultFile, StringComparison.OrdinalIgnoreCase))
                {
                    OpenLibrary(MultiLibrary.DefaultFile);
                }

                return;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            RefreshLibrary();
        }

        private void RefreshLibrary()
        {
            if (_library == null)
            {
                return;
            }

            string selectedCategory = comboBoxCategory.SelectedItem as string;
            comboBoxCategory.BeginUpdate();
            comboBoxCategory.Items.Clear();
            comboBoxCategory.Items.Add("(all)");
            foreach (string c in _library.GetCategories())
            {
                comboBoxCategory.Items.Add(c);
            }

            comboBoxCategory.EndUpdate();
            int index = selectedCategory == null ? 0 : comboBoxCategory.Items.IndexOf(selectedCategory);
            comboBoxCategory.SelectedIndex = index < 0 ? 0 : index;

            FilterLibrary();
        }

        private void FilterLibrary()
        {
            if (_library == null)
            {
                return;
            }

            string search = textBoxLibrarySearch.Text.Trim();
            string category = comboBoxCategory.SelectedIndex > 0 ? comboBoxCategory.SelectedItem as string : null;
            int? id = search.StartsWith("#") ? ParseId(search) : null;

            IEnumerable<MultiLibraryEntry> query = _library.GetEntries();
            if (category != null)
            {
                query = query.Where(e => string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase));
            }

            if (id.HasValue)
            {
                query = query.Where(e => e.Id == id.Value);
            }
            else if (search.Length > 0)
            {
                query = query.Where(e => e.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         e.Category.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         e.Author.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _libraryShown = query.ToList();
            listViewLibrary.SelectedIndices.Clear();
            listViewLibrary.VirtualListSize = _libraryShown.Count;
            listViewLibrary.Invalidate();
            UpdateStatus();
        }

        private void OnRetrieveLibraryItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            MultiLibraryEntry entry = _libraryShown[e.ItemIndex];
            var item = new ListViewItem(entry.Id.ToString());
            item.SubItems.Add(entry.Name);
            item.SubItems.Add(entry.Category);
            item.SubItems.Add(entry.Count.ToString());
            item.SubItems.Add($"{entry.Width}x{entry.Height}");
            item.SubItems.Add(entry.Author);
            e.Item = item;
        }

        private List<MultiLibraryEntry> SelectedLibraryEntries()
        {
            return listViewLibrary.SelectedIndices.Cast<int>()
                .Where(i => i >= 0 && i < _libraryShown.Count)
                .Select(i => _libraryShown[i]).ToList();
        }

        #endregion

        #region Search / selection

        private void OnSearchChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private void OnSearchTimer(object sender, EventArgs e)
        {
            searchTimer.Stop();
            if (IsLibraryTab)
            {
                FilterLibrary();
            }
            else
            {
                FilterClient();
            }
        }

        private void OnSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            OnSelectionChanged(sender, e);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            MultiTemplate t = null;
            try
            {
                if (IsLibraryTab)
                {
                    List<MultiLibraryEntry> selected = SelectedLibraryEntries();
                    if (selected.Count == 1)
                    {
                        t = _library.Load(selected[0].Id);
                    }
                }
                else if (listViewClient.SelectedIndices.Count == 1)
                {
                    ClientMulti m = _clientShown[listViewClient.SelectedIndices[0]];
                    t = MultiFormats.FromComponentList(Multis.GetComponents(m.Index),
                        string.IsNullOrEmpty(m.Name) ? $"Multi {m.Index}" : m.Name);
                    t.Source = $"multi.mul #{m.Index}";
                }
            }
            catch (Exception ex)
            {
                labelInfo.Text = ex.Message;
            }

            SetCurrent(t);
            UpdateButtons();
        }

        private void SetCurrent(MultiTemplate t)
        {
            _current = t;
            UpdatePreview();

            labelInfo.Text = t == null
                ? "-"
                : $"{t.Name}   ({t.Source})\n{t.Components.Count} parts, {t.Width} x {t.Height}, z {t.MinZ}..{t.MaxZ}" +
                  (t.HasInvisibleParts ? $", {t.Components.Count(c => !c.IsVisible)} invisible" : string.Empty) +
                  (t.HasHues ? ", with hues" : string.Empty) +
                  (string.IsNullOrEmpty(t.Category) ? string.Empty : $"\nCategory: {t.Category}") +
                  (string.IsNullOrEmpty(t.Author) ? string.Empty : $"   Author: {t.Author}");
        }

        private void OnPreviewResize(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            Image old = pictureBoxPreview.Image;
            pictureBoxPreview.Image = _current == null || pictureBoxPreview.Width < 8 || pictureBoxPreview.Height < 8
                ? null
                : MultiPreview.Thumbnail(_current, _art, pictureBoxPreview.ClientSize, pictureBoxPreview.BackColor);
            old?.Dispose();
        }

        private void UpdateButtons()
        {
            bool library = IsLibraryTab;
            int libSelected = library ? listViewLibrary.SelectedIndices.Count : 0;

            buttonPlace.Enabled = _placeAction != null && _current != null && !_busy;
            buttonExport.Enabled = _current != null && !_busy;
            buttonSaveToMulti.Enabled = _current != null && !_busy;
            buttonAddToLibrary.Enabled = !library && _current != null && _library != null && !_busy;
            buttonEdit.Enabled = library && libSelected == 1 && !_busy;
            buttonDelete.Enabled = library && libSelected > 0 && !_busy;
            groupBoxImport.Enabled = !_busy;
        }

        private void UpdateStatus()
        {
            string lib = _library == null
                ? "no library"
                : $"Library: {_library.Count:N0} entries ({_library.DataSize / (1024.0 * 1024.0):0.0} MB) - {_library.DataFile}";
            string shown = IsLibraryTab ? $"{_libraryShown.Count:N0} shown" : $"{_clientShown.Count:N0} client multis shown";
            statusLabel.Text = $"{shown} | {lib}";
        }

        #endregion

        #region Actions

        private void OnClickPlace(object sender, EventArgs e)
        {
            if (_current == null || _placeAction == null)
            {
                return;
            }

            _placeAction(_current.Clone());
        }

        private void OnClickAddToLibrary(object sender, EventArgs e)
        {
            if (_current == null || _library == null)
            {
                return;
            }

            string name = InputDialog.Show(this, "Copy to library", "Name in the library:", _current.Name, false);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            MultiTemplate copy = _current.Clone();
            copy.Name = name.Trim();
            if (string.IsNullOrEmpty(copy.Category))
            {
                copy.Category = "Client multis";
            }

            MultiLibraryEntry entry = _library.Add(copy);
            RefreshLibrary();
            statusLabel.Text = $"Added to the library as #{entry.Id}";
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            if (_current != null)
            {
                ExportTemplate(this, _current);
            }
        }

        /// <summary>
        /// Save dialog with all export formats
        /// </summary>
        public static void ExportTemplate(IWin32Window owner, MultiTemplate t)
        {
            MultiExportFormat[] formats = (MultiExportFormat[])Enum.GetValues(typeof(MultiExportFormat));
            using (var dialog = new SaveFileDialog
            {
                Title = $"Export {t.Name}",
                Filter = string.Join("|", formats.Select(f => MultiFormats.FormatName(f) + "|" + MultiFormats.ExportFilter(f).Split('|')[1])),
                FileName = MakeFileName(t.Name),
                InitialDirectory = Options.OutputPath
            })
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return;
                }

                MultiExportFormat format = formats[Math.Max(0, dialog.FilterIndex - 1)];
                try
                {
                    MultiFormats.Export(t, dialog.FileName, format);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed:\n{ex.Message}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string hint = t.HasHues && format != MultiExportFormat.UoaBinary && format != MultiExportFormat.TxtHex && format != MultiExportFormat.TxtDecimal
                    ? "\n\nNote: this format has no hues, the colors are lost."
                    : string.Empty;
                MessageBox.Show($"Exported to\n{dialog.FileName}{hint}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string MakeFileName(string name)
        {
            name = string.IsNullOrWhiteSpace(name) ? "building" : name;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name;
        }

        private void OnClickSaveToMulti(object sender, EventArgs e)
        {
            if (_current != null && SaveToMultiMul(this, _current))
            {
                _clientAll = null;
                if (!IsLibraryTab)
                {
                    LoadClientMultis();
                }
            }
        }

        /// <summary>
        /// Asks for a free multi id, stores the building in memory and writes multi.mul/multi.idx into the output directory
        /// </summary>
        public static bool SaveToMultiMul(IWin32Window owner, MultiTemplate t)
        {
            int suggestion = Enumerable.Range(0, Multis.MaximumMultiIndex).Reverse()
                .FirstOrDefault(i => Multis.GetComponents(i) == MultiComponentList.Empty);

            string text = InputDialog.Show(owner, "Save into multi.mul",
                $"Multi id (0 - {Multis.MaximumMultiIndex - 1}, decimal or 0x hex):", suggestion.ToString(), false);
            if (text == null)
            {
                return false;
            }

            int? id = ParseId(text.Trim());
            if (!id.HasValue || id.Value < 0 || id.Value >= Multis.MaximumMultiIndex)
            {
                MessageBox.Show("Invalid multi id.", "multi.mul", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Multis.GetComponents(id.Value) != MultiComponentList.Empty &&
                MessageBox.Show($"Multi {id.Value} (0x{id.Value:X}) is already used. Replace it?", "multi.mul",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return false;
            }

            if (string.IsNullOrEmpty(Options.OutputPath) || !Directory.Exists(Options.OutputPath))
            {
                MessageBox.Show("The output directory does not exist. Please set it in the options.", "multi.mul", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string hueHint = t.HasHues ? "\nmulti.mul has no hues, the colors are lost." : string.Empty;
            if (MessageBox.Show($"Store \"{t.Name}\" as multi {id.Value} (0x{id.Value:X}) and write multi.mul / multi.idx into\n{Options.OutputPath}?{hueHint}",
                    "multi.mul", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return false;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                Multis.Add(id.Value, MultiFormats.ToComponentList(t));
                Multis.Save(Options.OutputPath);
                ControlEvents.FireMultiChangeEvent(owner, id.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Saving failed:\n{ex.Message}", "multi.mul", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            MessageBox.Show($"Saved as multi {id.Value} (0x{id.Value:X}) in\n{Options.OutputPath}", "multi.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        private void OnClickEdit(object sender, EventArgs e)
        {
            List<MultiLibraryEntry> selected = SelectedLibraryEntries();
            if (selected.Count != 1)
            {
                return;
            }

            MultiLibraryEntry entry = selected[0];
            string name = InputDialog.Show(this, "Library entry", "Name:", entry.Name, false);
            if (name == null)
            {
                return;
            }

            string category = InputDialog.Show(this, "Library entry", "Category:", entry.Category, false);
            if (category == null)
            {
                return;
            }

            string author = InputDialog.Show(this, "Library entry", "Author:", entry.Author, false);
            if (author == null)
            {
                return;
            }

            _library.UpdateMeta(entry.Id, name.Trim(), author.Trim(), category.Trim());
            RefreshLibrary();
        }

        private void OnClickDelete(object sender, EventArgs e)
        {
            List<MultiLibraryEntry> selected = SelectedLibraryEntries();
            if (selected.Count == 0 ||
                MessageBox.Show($"Delete {selected.Count} entr{(selected.Count == 1 ? "y" : "ies")} from the library?", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _library.Delete(selected.Select(s => s.Id));
            SetCurrent(null);
            RefreshLibrary();
        }

        private TxtIdFormat TxtFormat => comboBoxTxtIds.SelectedIndex switch
        {
            1 => TxtIdFormat.Hex,
            2 => TxtIdFormat.Decimal,
            _ => TxtIdFormat.Auto
        };

        private void OnClickImportFiles(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = MultiFormats.ImportFilter, Multiselect = true, Title = "Import buildings into the library" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    ImportFiles(dialog.FileNames.Select(f => (f, string.Empty)).ToList());
                }
            }
        }

        private void OnClickImportFolder(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Folder with building files (sub folders are included, sub folder name = category)" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string root = dialog.SelectedPath;
                var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                    .Where(f => MultiFormats.ImportExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .Select(f =>
                    {
                        string dir = Path.GetDirectoryName(f);
                        string category = MapSaveService.IsSameDirectory(dir, root) ? string.Empty : Path.GetRelativePath(root, dir);
                        return (f, category);
                    })
                    .ToList();

                if (files.Count == 0)
                {
                    MessageBox.Show("No building files found.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show($"Import {files.Count:N0} files into the library?", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    ImportFiles(files);
                }
            }
        }

        private async void ImportFiles(List<(string file, string category)> files)
        {
            if (_library == null || _busy)
            {
                return;
            }

            _busy = true;
            UpdateButtons();
            TxtIdFormat format = TxtFormat;
            var errors = new List<string>();
            int imported = 0;
            var progress = new Progress<int>(n => statusLabel.Text = $"Importing {n:N0} / {files.Count:N0} ...");
            MultiLibrary library = _library;

            try
            {
                await Task.Run(() =>
                {
                    IProgress<int> report = progress;
                    for (int i = 0; i < files.Count; i++)
                    {
                        (string file, string category) = files[i];
                        try
                        {
                            MultiTemplate t = MultiFormats.Load(file, format);
                            if (!string.IsNullOrEmpty(category) && string.IsNullOrEmpty(t.Category))
                            {
                                t.Category = category;
                            }

                            library.Add(t);
                            imported++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                        }

                        if (i % 50 == 0)
                        {
                            report.Report(i);
                        }
                    }

                    library.Flush();
                });
            }
            finally
            {
                _busy = false;
            }

            if (IsDisposed)
            {
                return;
            }

            tabControl.SelectedTab = tabPageLibrary;
            RefreshLibrary();
            UpdateButtons();

            string message = $"{imported:N0} building(s) imported.";
            if (errors.Count > 0)
            {
                message += $"\n\n{errors.Count} file(s) could not be read:\n" + string.Join("\n", errors.Take(15)) + (errors.Count > 15 ? "\n..." : string.Empty);
            }

            MessageBox.Show(message, Text, MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void OnClickCompact(object sender, EventArgs e)
        {
            if (_library == null ||
                MessageBox.Show("Rewrite the library file without deleted entries?\nThe old file is kept as .bak.", Text,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            long before = _library.DataSize;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _library.Compact();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Compacting failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            RefreshLibrary();
            statusLabel.Text = $"Library compacted: {before / 1024:N0} KB -> {_library.DataSize / 1024:N0} KB";
        }

        private void OnClickLibraryFile(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog
            {
                Title = "Open or create a building library",
                Filter = "Building library (*.uml)|*.uml",
                OverwritePrompt = false,
                FileName = Path.GetFileName(_library?.DataFile ?? MultiLibrary.DefaultFile),
                InitialDirectory = Path.GetDirectoryName(_library?.DataFile ?? MultiLibrary.DefaultFile)
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    OpenLibrary(dialog.FileName);
                    tabControl.SelectedTab = tabPageLibrary;
                }
            }
        }

        private void OnClickMoveLibrary(object sender, EventArgs e)
        {
            if (_library == null)
            {
                return;
            }

            using (var dialog = new FolderBrowserDialog
            {
                Description = "New folder for the building library (the files are moved)",
                SelectedPath = Path.GetDirectoryName(_library.DataFile)
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string current = _library.DataFile;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    _library.Dispose();
                    _library = null;
                    string moved = MultiLibraryLocation.Move(current, dialog.SelectedPath);
                    OpenLibrary(moved);
                    statusLabel.Text = $"Library moved to {moved}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The library could not be moved:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    OpenLibrary(current);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void OnClickOpenLibraryFolder(object sender, EventArgs e)
        {
            string folder = Path.GetDirectoryName(_library?.DataFile ?? MultiLibraryLocation.CurrentFile);
            if (Directory.Exists(folder))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
        }

        private void OnActivated(object sender, EventArgs e)
        {
            // other windows (area export) may have added entries
            if (_library != null && !_busy && _library.Refresh())
            {
                RefreshLibrary();
            }
        }

        /// <summary>
        /// Adds a template to the opened library (e.g. an area of the map). Returns the new id or -1.
        /// </summary>
        public int AddToLibrary(MultiTemplate t)
        {
            if (_library == null)
            {
                OpenLibrary(MultiLibraryLocation.CurrentFile);
            }

            if (_library == null)
            {
                return -1;
            }

            MultiLibraryEntry entry = _library.Add(t);
            _library.Flush();
            tabControl.SelectedTab = tabPageLibrary;
            RefreshLibrary();
            return entry.Id;
        }

        #endregion
    }
}
