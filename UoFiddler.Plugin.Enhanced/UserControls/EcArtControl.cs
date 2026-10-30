/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
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
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Viewer / editor for the DDS images of the Enhanced Client (Texture.uop, LegacyTexture.uop).
    /// </summary>
    public sealed class EcArtControl : UserControl, IEcEditor
    {
        private sealed class ArtItem
        {
            public int Id;
            public EcUopEntry Entry;
        }

        private readonly ToolStripComboBox _sourceBox;
        private readonly ToolStripComboBox _fileBox;
        private readonly ToolStripTextBox _searchBox;
        private readonly ToolStripComboBox _sizeBox;
        private readonly ToolStripButton _unnamedButton;
        private readonly ToolStripComboBox _formatBox;
        private readonly ToolStripButton _mipButton;
        private readonly ToolStripButton _saveButton;
        private readonly EcThumbnailGrid _grid;
        private readonly EcPreviewPanel _preview;
        private readonly Label _infoLabel;
        private readonly ToolStripStatusLabel _statusLabel;

        private EcClient _client;
        private EcUopPackage _package;
        private EcArtSource _source;
        private List<ArtItem> _items = new List<ArtItem>();
        private bool _loaded;
        private bool _updatingFileBox;

        public EcArtControl()
        {
            Dock = DockStyle.Fill;

            // ---------------- tool bar
            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _sourceBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230, ToolTipText = "Kind of image" };
            _sourceBox.Items.AddRange(EcClient.ArtSources.Cast<object>().ToArray());
            _sourceBox.SelectedIndexChanged += (s, e) => { if (_loaded) { LoadSource(null); } };

            _fileBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160, ToolTipText = "UOP file the images are read from" };
            _fileBox.SelectedIndexChanged += (s, e) => { if (_loaded && !_updatingFileBox) { LoadSource(_fileBox.SelectedItem as string); } };

            _searchBox = new ToolStripTextBox { Width = 90, ToolTipText = "Id (decimal or 0x hex) – Enter to search" };
            _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Search(); } };

            _sizeBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 60, ToolTipText = "Thumbnail size" };
            _sizeBox.Items.AddRange(new object[] { "48", "64", "96", "128", "192" });
            _sizeBox.SelectedIndex = 2;
            _sizeBox.SelectedIndexChanged += (s, e) => _grid.CellSize = int.Parse((string)_sizeBox.SelectedItem);

            _unnamedButton = new ToolStripButton("Unnamed") { CheckOnClick = true, ToolTipText = "Also show entries whose name (id) is unknown" };
            _unnamedButton.CheckedChanged += (s, e) => { if (_package != null) { BuildItems(); } };

            toolStrip.Items.Add(new ToolStripLabel("Source:"));
            toolStrip.Items.Add(_sourceBox);
            toolStrip.Items.Add(_fileBox);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripLabel("Id:"));
            toolStrip.Items.Add(_searchBox);
            toolStrip.Items.Add(new ToolStripButton("Search", null, (s, e) => Search()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripLabel("Size:"));
            toolStrip.Items.Add(_sizeBox);
            toolStrip.Items.Add(_unnamedButton);
            toolStrip.Items.Add(new ToolStripButton("Scan ids...", null, (s, e) => ScanIds()) { ToolTipText = "Search an additional id range for images" });

            // ---------------- edit bar
            var editStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _formatBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120, ToolTipText = "DDS format used when importing images" };
            _formatBox.Items.AddRange(new object[] { "Same as original", "DXT5", "DXT1", "DXT3", "ARGB 32 bit" });
            _formatBox.SelectedIndex = 0;
            _mipButton = new ToolStripButton("Mipmaps") { CheckOnClick = true, Checked = true, ToolTipText = "Generate mipmaps when the original image has them" };
            _saveButton = new ToolStripButton("Save file", null, (s, e) => Save(false)) { Enabled = false };

            editStrip.Items.Add(new ToolStripButton("Replace...", null, (s, e) => ReplaceSelected()) { ToolTipText = "Replace the selected image with a picture (png, bmp, gif, jpg, tif, dds)" });
            editStrip.Items.Add(new ToolStripButton("Insert...", null, (s, e) => InsertNew()) { ToolTipText = "Add an image with a new id" });
            editStrip.Items.Add(new ToolStripButton("Remove", null, (s, e) => RemoveSelected()));
            editStrip.Items.Add(new ToolStripSeparator());
            editStrip.Items.Add(new ToolStripLabel("Import as:"));
            editStrip.Items.Add(_formatBox);
            editStrip.Items.Add(_mipButton);
            editStrip.Items.Add(new ToolStripSeparator());
            var exportButton = new ToolStripDropDownButton("Export");
            exportButton.DropDownItems.Add("Selected as PNG...", null, (s, e) => ExportSelected(ImageFormat.Png, ".png"));
            exportButton.DropDownItems.Add("Selected as BMP...", null, (s, e) => ExportSelected(ImageFormat.Bmp, ".bmp"));
            exportButton.DropDownItems.Add("Selected as original data (DDS / TGA)...", null, (s, e) => ExportSelectedDds());
            exportButton.DropDownItems.Add(new ToolStripSeparator());
            exportButton.DropDownItems.Add("All as PNG to folder...", null, (s, e) => ExportAll());
            editStrip.Items.Add(exportButton);
            editStrip.Items.Add(new ToolStripSeparator());
            editStrip.Items.Add(_saveButton);
            editStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            // ---------------- content
            _grid = new EcThumbnailGrid { Dock = DockStyle.Fill };
            _grid.ImageRequested += GetImage;
            _grid.LabelRequested += GetLabel;
            _grid.SelectionChanged += (s, e) => ShowSelected();

            var menu = new ContextMenuStrip();
            menu.Items.Add("Replace...", null, (s, e) => ReplaceSelected());
            menu.Items.Add("Export as PNG...", null, (s, e) => ExportSelected(ImageFormat.Png, ".png"));
            menu.Items.Add("Export DDS...", null, (s, e) => ExportSelectedDds());
            menu.Items.Add("Copy id", null, (s, e) => { ArtItem item = SelectedItem; if (item != null) { Clipboard.SetText(item.Id.ToString()); } });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Remove", null, (s, e) => RemoveSelected());
            _grid.ContextMenuStrip = menu;

            _preview = new EcPreviewPanel { Dock = DockStyle.Fill };
            _infoLabel = new Label { Dock = DockStyle.Top, Height = 92, Padding = new Padding(4), Font = EcDialogs.MonoFont(8.5f) };

            var zoomStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Bottom };
            zoomStrip.Items.Add(new ToolStripButton("−", null, (s, e) => _preview.Zoom /= 2));
            zoomStrip.Items.Add(new ToolStripButton("1:1", null, (s, e) => _preview.Zoom = 1));
            zoomStrip.Items.Add(new ToolStripButton("+", null, (s, e) => _preview.Zoom *= 2));
            zoomStrip.Items.Add(new ToolStripLabel("Ctrl + mouse wheel = zoom"));

            var rightPanel = new Panel { Dock = DockStyle.Fill };
            rightPanel.Controls.Add(_preview);
            rightPanel.Controls.Add(zoomStrip);
            rightPanel.Controls.Add(_infoLabel);

            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2 };
            split.Panel1.Controls.Add(_grid);
            split.Panel2.Controls.Add(rightPanel);
            split.HandleCreated += (s, e) => { try { split.SplitterDistance = Math.Max(200, split.Width - 360); } catch (InvalidOperationException) { } };

            var status = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Choose the Enhanced Client folder on top.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            status.Items.Add(_statusLabel);

            Controls.Add(split);
            Controls.Add(editStrip);
            Controls.Add(toolStrip);
            Controls.Add(status);
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _package = null;
            _items = new List<ArtItem>();
            _grid.ItemCount = 0;
            _preview.Image = null;
            _infoLabel.Text = string.Empty;
            _loaded = false;
            _saveButton.Enabled = false;
            _statusLabel.Text = client == null ? "Choose the Enhanced Client folder on top." : "Ready.";
            if (Visible && client != null)
            {
                OnActivated();
            }
        }

        public void OnActivated()
        {
            if (_loaded || _client == null)
            {
                return;
            }

            if (_sourceBox.SelectedIndex < 0)
            {
                _sourceBox.SelectedIndex = 0; // doesn't load anything yet because _loaded is still false
            }
            _loaded = true;
            LoadSource(null);
        }

        /// <summary>
        /// Shows the image of a tileart texture reference (used by the TileArt editor).
        /// </summary>
        public void ShowImage(EcArtSource source, int id)
        {
            if (_client == null)
            {
                return;
            }

            _loaded = true;
            int index = Array.IndexOf(EcClient.ArtSources, source);
            if (index >= 0 && _sourceBox.SelectedIndex != index)
            {
                _loaded = false;
                _sourceBox.SelectedIndex = index;
                _loaded = true;
                LoadSource(null);
            }
            else if (_package == null)
            {
                LoadSource(null);
            }
            SelectId(id);
        }

        private void LoadSource(string preferredFile)
        {
            if (_client == null || _sourceBox.SelectedItem is not EcArtSource source)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                _source = source;
                _package = null;

                List<string> files = source.FileNames.Where(f => _client.FindFile(f) != null).ToList();
                foreach (string other in _client.GetUopFiles().Select(Path.GetFileName))
                {
                    if (!files.Contains(other, StringComparer.OrdinalIgnoreCase))
                    {
                        files.Add(other);
                    }
                }

                _updatingFileBox = true;
                _fileBox.Items.Clear();
                _fileBox.Items.AddRange(files.Cast<object>().ToArray());
                _updatingFileBox = false;

                if (files.Count == 0)
                {
                    _grid.ItemCount = 0;
                    _statusLabel.Text = $"No UOP files found in {_client.Folder}.";
                    return;
                }

                List<int> dictionaryIds = _client.GetDictionaryIds(source.DictionaryFolder).ToList();
                string chosen = null;

                int CountHits(string file)
                {
                    EcUopPackage package = _client.GetPackage(file);
                    if (package == null)
                    {
                        return 0;
                    }
                    if (source.IsNameBased)
                    {
                        return package.Entries.Count(e => source.Matches(e.Name));
                    }
                    int hits = package.ResolveNames(source.Pattern, 0, source.DefaultLastId);
                    hits += package.ResolveNames(source.Pattern, dictionaryIds.Where(id => id > source.DefaultLastId));
                    return hits;
                }

                if (preferredFile != null)
                {
                    CountHits(preferredFile);
                    chosen = preferredFile;
                }
                else
                {
                    // first the usual files of this kind, then every other UOP file of the folder
                    var defaults = source.FileNames.Where(f => _client.FindFile(f) != null).ToList();
                    chosen = defaults.FirstOrDefault(f => CountHits(f) > 0);
                    if (chosen == null)
                    {
                        foreach (string file in files.Except(defaults, StringComparer.OrdinalIgnoreCase))
                        {
                            _statusLabel.Text = $"Searching '{source.Title}' in {file}...";
                            Application.DoEvents();
                            try
                            {
                                if (CountHits(file) > 0)
                                {
                                    chosen = file;
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                // not a readable UOP file
                            }
                        }
                    }
                }

                bool found = chosen != null;
                chosen ??= files[0];
                _package = _client.GetPackage(chosen);
                _package.Changed -= PackageChanged;
                _package.Changed += PackageChanged;

                _updatingFileBox = true;
                _fileBox.SelectedItem = chosen;
                _updatingFileBox = false;

                BuildItems();
                if (!found && preferredFile == null)
                {
                    _statusLabel.Text = $"'{source.Title}' ({source.Pattern}) was not found in any UOP file of this folder.";
                }
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Loading the images failed.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void PackageChanged(object sender, EventArgs e)
        {
            if (sender == _package)
            {
                _saveButton.Enabled = _package.IsDirty;
            }
        }

        private void BuildItems()
        {
            var items = new List<ArtItem>();
            var unnamed = new List<ArtItem>();
            foreach (EcUopEntry entry in _package.Entries)
            {
                if (entry.Name != null)
                {
                    if (_source.IsNameBased)
                    {
                        if (_source.Matches(entry.Name))
                        {
                            items.Add(new ArtItem { Id = LeadingNumber(Path.GetFileName(entry.Name)), Entry = entry });
                        }
                    }
                    else if (_source.Matches(entry.Name) && EcClient.TryGetIdFromName(entry.Name, out int id))
                    {
                        items.Add(new ArtItem { Id = id, Entry = entry });
                    }
                }
                else if (_unnamedButton.Checked)
                {
                    unnamed.Add(new ArtItem { Id = -1, Entry = entry });
                }
            }

            if (_source.IsNameBased)
            {
                items.Sort((a, b) => string.Compare(a.Entry.Name, b.Entry.Name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                items.Sort((a, b) => a.Id.CompareTo(b.Id));
            }
            items.AddRange(unnamed);
            _items = items;
            _grid.ItemCount = _items.Count;
            _saveButton.Enabled = _package.IsDirty;
            _statusLabel.Text = $"{Path.GetFileName(_package.FilePath)}: {_items.Count - unnamed.Count} images of '{_source.Title}' ({_package.Count} entries in the file, {EcImageHelper.FormatSize(new FileInfo(_package.FilePath).Length)}).";
            if (_items.Count > 0)
            {
                _grid.SelectedIndex = Math.Max(0, _grid.SelectedIndex);
                ShowSelected();
            }
            else
            {
                _preview.Image = null;
                _infoLabel.Text = "No images found. Use 'Scan ids...' or choose another file.";
            }
        }

        private ArtItem SelectedItem => _grid.SelectedIndex >= 0 && _grid.SelectedIndex < _items.Count ? _items[_grid.SelectedIndex] : null;

        private Bitmap GetImage(int index)
        {
            if (index < 0 || index >= _items.Count)
            {
                return null;
            }

            byte[] data = _package.ReadData(_items[index].Entry);
            return EcImageHelper.TryDecode(data, out _, _items[index].Entry.Name);
        }

        private string GetLabel(int index)
        {
            if (index < 0 || index >= _items.Count)
            {
                return string.Empty;
            }
            ArtItem item = _items[index];
            string mark = item.Entry.IsModified ? "*" : string.Empty;
            if (_source != null && _source.IsNameBased && item.Entry.Name != null)
            {
                return Path.GetFileNameWithoutExtension(item.Entry.Name) + mark;
            }
            return item.Id >= 0 ? $"{item.Id}{mark}" : $"#{item.Entry.Hash:X8}{mark}";
        }

        /// <summary>
        /// "03000002_fire.tga" -> 3000002 (-1 without leading digits).
        /// </summary>
        private static int LeadingNumber(string fileName)
        {
            int length = 0;
            while (length < fileName.Length && length < 9 && char.IsDigit(fileName[length]))
            {
                length++;
            }
            return length > 0 && int.TryParse(fileName.Substring(0, length), out int value) ? value : -1;
        }

        private void ShowSelected()
        {
            ArtItem item = SelectedItem;
            if (item == null)
            {
                _preview.Image = null;
                _infoLabel.Text = string.Empty;
                return;
            }

            try
            {
                byte[] data = _package.ReadData(item.Entry);
                Bitmap bitmap = EcImageHelper.TryDecode(data, out string info, item.Entry.Name);
                _preview.Image = bitmap;
                string idText = item.Id >= 0 ? $"{item.Id} (0x{item.Id:X})" : "unknown";
                _infoLabel.Text =
                    $"Id:    {idText}\r\n" +
                    $"Name:  {item.Entry.DisplayName}\r\n" +
                    $"Image: {info ?? "no image data"}\r\n" +
                    $"Size:  {EcImageHelper.FormatSize(data.Length)}  stored {(item.Entry.Flag == 1 ? "zlib" : item.Entry.Flag == 0 ? "uncompressed" : "flag " + item.Entry.Flag)}" +
                    (item.Entry.IsModified ? "\r\n       modified (not saved)" : string.Empty);
            }
            catch (Exception ex)
            {
                _preview.Image = null;
                _infoLabel.Text = $"Name:  {item.Entry.DisplayName}\r\nError: {ex.Message}";
            }
        }

        private void Search()
        {
            if (!EcDialogs.TryParseNumber(_searchBox.Text, out long id))
            {
                return;
            }
            if (!SelectId((int)id))
            {
                _statusLabel.Text = $"Id {id} not found in this list.";
            }
        }

        private bool SelectId(int id)
        {
            int index = _items.FindIndex(i => i.Id == id);
            if (index < 0)
            {
                // nearest following id
                index = _items.FindIndex(i => i.Id > id);
                if (index >= 0)
                {
                    _grid.SelectedIndex = index;
                }
                return false;
            }
            _grid.SelectedIndex = index;
            return true;
        }

        private void ScanIds()
        {
            if (_package == null)
            {
                return;
            }

            string[] values = EcDialogs.Prompt(this, "Scan ids", new[] { "First id:", "Last id:" }, new[] { "0", "0x200000" });
            if (values == null || !EcDialogs.TryParseNumber(values[0], out long first) || !EcDialogs.TryParseNumber(values[1], out long last) || last < first)
            {
                return;
            }

            if (last - first > 50_000_000)
            {
                EcDialogs.ShowError(this, "Please scan at most 50 000 000 ids at once.");
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                int hits = _package.ResolveNames(_source.Pattern, (int)first, (int)last);
                BuildItems();
                _statusLabel.Text = $"Scan finished: {hits} names found between {first} and {last}.";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ------------------------------------------------------------------ editing

        private DdsSaveFormat? ChosenFormat()
        {
            return _formatBox.SelectedIndex switch
            {
                1 => DdsSaveFormat.Dxt5,
                2 => DdsSaveFormat.Dxt1,
                3 => DdsSaveFormat.Dxt3,
                4 => DdsSaveFormat.A8R8G8B8,
                _ => null
            };
        }

        /// <summary>
        /// Builds the DDS data for a picture file. DDS files are taken as they are.
        /// </summary>
        /// <summary>
        /// Builds the data to store for a picture file: TGA for ".tga" entries (effect textures), DDS otherwise.
        /// </summary>
        private byte[] CreateImageData(string file, byte[] originalData, string targetName)
        {
            if (targetName != null && targetName.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
            {
                byte[] bytes = File.ReadAllBytes(file);
                if (file.EndsWith(".tga", StringComparison.OrdinalIgnoreCase) && TgaImage.LooksLikeTga(bytes))
                {
                    return bytes;
                }
                using (Bitmap bitmap = EcImageHelper.LoadImageFile(file))
                {
                    byte[] pixels = EcImageHelper.ToBgra(bitmap, out int w, out int h);
                    return TgaImage.Encode(pixels, w, h);
                }
            }
            return CreateDds(file, originalData);
        }

        private byte[] CreateDds(string file, byte[] originalData)
        {
            byte[] fileBytes = File.ReadAllBytes(file);
            if (DdsImage.IsDds(fileBytes) && ChosenFormat() == null)
            {
                return fileBytes;
            }

            DdsSaveFormat format = DdsSaveFormat.Dxt5;
            int mipMaps = 1;
            if (originalData != null && DdsImage.IsDds(originalData))
            {
                DdsImage.TryReadInfo(originalData, out _, out _, out _, out int originalMips);
                format = DdsImage.Decode(originalData).SuggestedSaveFormat;
                mipMaps = originalMips > 1 ? 0 : 1;
            }

            format = ChosenFormat() ?? format;
            if (!_mipButton.Checked)
            {
                mipMaps = 1;
            }

            using (Bitmap bitmap = EcImageHelper.LoadImageFile(file))
            {
                byte[] pixels = EcImageHelper.ToBgra(bitmap, out int w, out int h);
                return DdsImage.Encode(pixels, w, h, format, mipMaps);
            }
        }

        private static string ChooseImageFile()
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = "Images (*.png;*.bmp;*.gif;*.jpg;*.tif;*.dds;*.tga)|*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff;*.dds;*.tga|All files (*.*)|*.*",
                Title = "Choose image"
            })
            {
                return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
            }
        }

        private void ReplaceSelected()
        {
            ArtItem item = SelectedItem;
            if (item == null)
            {
                return;
            }

            string file = ChooseImageFile();
            if (file == null)
            {
                return;
            }

            try
            {
                byte[] original = _package.ReadData(item.Entry);
                _package.SetData(item.Entry, CreateImageData(file, original, item.Entry.Name));
                _grid.InvalidateItem(_grid.SelectedIndex);
                ShowSelected();
                _statusLabel.Text = $"Image {GetLabel(_grid.SelectedIndex)} replaced. Save the file to keep the change.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Replacing the image failed.", ex);
            }
        }

        private void InsertNew()
        {
            if (_package == null || _source == null)
            {
                return;
            }

            string name;
            int id = -1;
            if (_source.IsNameBased)
            {
                string fileName = EcDialogs.Prompt(this, "Insert image", "File name (e.g. 03009999_myeffect.tga or .dds):", "03009999_new.tga");
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }
                name = _source.Prefix + fileName.Trim().Replace('\\', '/').Split('/').Last();
                if (_package.Contains(name) && !EcDialogs.Confirm(this, $"{name} already exists. Replace it?"))
                {
                    return;
                }
            }
            else
            {
                int suggestion = _items.Count > 0 ? _items.Where(i => i.Id >= 0).Select(i => i.Id).DefaultIfEmpty(-1).Max() + 1 : 0;
                string text = EcDialogs.Prompt(this, "Insert image", "New id (decimal or 0x hex):", suggestion.ToString());
                if (text == null || !EcDialogs.TryParseNumber(text, out long value) || value < 0 || value > 99_999_999)
                {
                    return;
                }
                id = (int)value;
                name = _source.GetName(id);
                if (_package.Contains(name) && !EcDialogs.Confirm(this, $"Id {id} already exists. Replace it?"))
                {
                    return;
                }
            }

            string file = ChooseImageFile();
            if (file == null)
            {
                return;
            }

            try
            {
                // use an existing image of the list as template for format / mipmaps
                byte[] template = _items.Count > 0 ? _package.ReadData(_items[0].Entry) : null;
                EcUopEntry entry = _package.AddOrReplace(name, CreateImageData(file, template, name), _package.Count > 0 ? _package.PreferredCompression : EcUopCompression.None);
                BuildItems();
                int index = _items.FindIndex(i => i.Entry == entry);
                if (index >= 0)
                {
                    _grid.SelectedIndex = index;
                }
                _statusLabel.Text = $"{name} added. Save the file to keep the change.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Adding the image failed.", ex);
            }
        }

        private void RemoveSelected()
        {
            ArtItem item = SelectedItem;
            if (item == null || !EcDialogs.Confirm(this, $"Remove image {GetLabel(_grid.SelectedIndex)} from {Path.GetFileName(_package.FilePath)}?"))
            {
                return;
            }

            int index = _grid.SelectedIndex;
            _package.Remove(item.Entry);
            BuildItems();
            _grid.SelectedIndex = Math.Min(index, _items.Count - 1);
        }

        private void Save(bool saveAs)
        {
            if (_package == null)
            {
                return;
            }

            string target = _package.FilePath;
            if (saveAs)
            {
                using (var dialog = new SaveFileDialog { Filter = "UOP files (*.uop)|*.uop", FileName = Path.GetFileName(_package.FilePath), Title = "Save UOP as" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    target = dialog.FileName;
                }
            }
            else if (!EcDialogs.Confirm(this, $"Save {Path.GetFileName(target)}?\r\nThe old file is kept as .bak next to it.\r\n\r\nThe Enhanced Client must be closed."))
            {
                return;
            }

            if (!EcDialogs.ConfirmSignedSave(this, _package))
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                _package.Save(target);
                _grid.ClearCache();
                _grid.Invalidate();
                ShowSelected();
                _statusLabel.Text = $"Saved {target}.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Saving failed (is the file opened by the client?).", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ------------------------------------------------------------------ export

        private void ExportSelected(ImageFormat format, string extension)
        {
            ArtItem item = SelectedItem;
            if (item == null)
            {
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = $"{extension.TrimStart('.').ToUpperInvariant()}|*{extension}", FileName = FileBaseName(item) + extension })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                using (Bitmap bitmap = EcImageHelper.TryDecode(_package.ReadData(item.Entry), out string info))
                {
                    if (bitmap == null)
                    {
                        EcDialogs.ShowError(this, info ?? "The entry is no image.");
                        return;
                    }
                    bitmap.Save(dialog.FileName, format);
                }
            }
        }

        private void ExportSelectedDds()
        {
            ArtItem item = SelectedItem;
            if (item == null)
            {
                return;
            }

            string extension = item.Entry.Name != null && item.Entry.Name.EndsWith(".tga", StringComparison.OrdinalIgnoreCase) ? ".tga" : ".dds";
            using (var dialog = new SaveFileDialog { Filter = $"{extension.TrimStart('.').ToUpperInvariant()}|*{extension}", FileName = FileBaseName(item) + extension })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllBytes(dialog.FileName, _package.ReadData(item.Entry));
                }
            }
        }

        private void ExportAll()
        {
            if (_items.Count == 0)
            {
                return;
            }

            using (var dialog = new FolderBrowserDialog { Description = "Folder for the PNG files" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Cursor = Cursors.WaitCursor;
                int done = 0, failed = 0;
                try
                {
                    foreach (ArtItem item in _items)
                    {
                        try
                        {
                            using (Bitmap bitmap = EcImageHelper.TryDecode(_package.ReadData(item.Entry), out _))
                            {
                                if (bitmap == null)
                                {
                                    failed++;
                                    continue;
                                }
                                bitmap.Save(Path.Combine(dialog.SelectedPath, FileBaseName(item) + ".png"), ImageFormat.Png);
                                done++;
                            }
                        }
                        catch (Exception)
                        {
                            failed++;
                        }

                        if ((done + failed) % 200 == 0)
                        {
                            _statusLabel.Text = $"Exporting... {done + failed} / {_items.Count}";
                            Application.DoEvents();
                        }
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
                _statusLabel.Text = $"Exported {done} images to {dialog.SelectedPath}" + (failed > 0 ? $" ({failed} failed)." : ".");
            }
        }

        private string FileBaseName(ArtItem item)
        {
            if (_source != null && _source.IsNameBased && item.Entry.Name != null)
            {
                return Path.GetFileNameWithoutExtension(item.Entry.Name);
            }
            string kind = _source?.DictionaryFolder ?? "image";
            return item.Id >= 0 ? $"{kind}_{item.Id:D8}" : $"{kind}_{item.Entry.Hash:X16}";
        }
    }
}
