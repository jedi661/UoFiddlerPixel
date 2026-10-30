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
    public sealed partial class EcArtControl : UserControl, IEcEditor
    {
        private sealed class ArtItem
        {
            public int Id;
            public EcUopEntry Entry;
        }

        private EcClient _client;
        private EcUopPackage _package;
        private EcArtSource _source;
        private List<ArtItem> _items = new List<ArtItem>();
        private bool _loaded;
        private bool _updatingFileBox;

        public EcArtControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;

            // runtime only: art sources, defaults and the image callbacks of the grid
            toolStripComboBoxSource.Items.AddRange(EcClient.ArtSources.Cast<object>().ToArray());
            toolStripComboBoxSize.SelectedIndex = 2;
            toolStripComboBoxFormat.SelectedIndex = 0;
            thumbnailGrid.ImageRequested += GetImage;
            thumbnailGrid.LabelRequested += GetLabel;
            thumbnailGrid.SelectionChanged += OnGridSelectionChanged;
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _package = null;
            _items = new List<ArtItem>();
            thumbnailGrid.ItemCount = 0;
            previewPanel.Image = null;
            labelInfo.Text = string.Empty;
            _loaded = false;
            toolStripButtonSave.Enabled = false;
            toolStripStatusLabel.Text = client == null ? "Choose the Enhanced Client folder on top." : "Ready.";
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

            if (toolStripComboBoxSource.SelectedIndex < 0)
            {
                toolStripComboBoxSource.SelectedIndex = 0; // doesn't load anything yet because _loaded is still false
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
            if (index >= 0 && toolStripComboBoxSource.SelectedIndex != index)
            {
                _loaded = false;
                toolStripComboBoxSource.SelectedIndex = index;
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
            if (_client == null || toolStripComboBoxSource.SelectedItem is not EcArtSource source)
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
                toolStripComboBoxFile.Items.Clear();
                toolStripComboBoxFile.Items.AddRange(files.Cast<object>().ToArray());
                _updatingFileBox = false;

                if (files.Count == 0)
                {
                    thumbnailGrid.ItemCount = 0;
                    toolStripStatusLabel.Text = $"No UOP files found in {_client.Folder}.";
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
                            toolStripStatusLabel.Text = $"Searching '{source.Title}' in {file}...";
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
                toolStripComboBoxFile.SelectedItem = chosen;
                _updatingFileBox = false;

                BuildItems();
                if (!found && preferredFile == null)
                {
                    toolStripStatusLabel.Text = $"'{source.Title}' ({source.Pattern}) was not found in any UOP file of this folder.";
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
                toolStripButtonSave.Enabled = _package.IsDirty;
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
                else if (toolStripButtonUnnamed.Checked)
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
            thumbnailGrid.ItemCount = _items.Count;
            toolStripButtonSave.Enabled = _package.IsDirty;
            toolStripStatusLabel.Text = $"{Path.GetFileName(_package.FilePath)}: {_items.Count - unnamed.Count} images of '{_source.Title}' ({_package.Count} entries in the file, {EcImageHelper.FormatSize(new FileInfo(_package.FilePath).Length)}).";
            if (_items.Count > 0)
            {
                thumbnailGrid.SelectedIndex = Math.Max(0, thumbnailGrid.SelectedIndex);
                ShowSelected();
            }
            else
            {
                previewPanel.Image = null;
                labelInfo.Text = "No images found. Use 'Scan ids...' or choose another file.";
            }
        }

        private ArtItem SelectedItem => thumbnailGrid.SelectedIndex >= 0 && thumbnailGrid.SelectedIndex < _items.Count ? _items[thumbnailGrid.SelectedIndex] : null;

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
                previewPanel.Image = null;
                labelInfo.Text = string.Empty;
                return;
            }

            try
            {
                byte[] data = _package.ReadData(item.Entry);
                Bitmap bitmap = EcImageHelper.TryDecode(data, out string info, item.Entry.Name);
                previewPanel.Image = bitmap;
                string idText = item.Id >= 0 ? $"{item.Id} (0x{item.Id:X})" : "unknown";
                labelInfo.Text =
                    $"Id:    {idText}\r\n" +
                    $"Name:  {item.Entry.DisplayName}\r\n" +
                    $"Image: {info ?? "no image data"}\r\n" +
                    $"Size:  {EcImageHelper.FormatSize(data.Length)}  stored {(item.Entry.Flag == 1 ? "zlib" : item.Entry.Flag == 0 ? "uncompressed" : "flag " + item.Entry.Flag)}" +
                    (item.Entry.IsModified ? "\r\n       modified (not saved)" : string.Empty);
            }
            catch (Exception ex)
            {
                previewPanel.Image = null;
                labelInfo.Text = $"Name:  {item.Entry.DisplayName}\r\nError: {ex.Message}";
            }
        }

        private void Search()
        {
            if (!EcDialogs.TryParseNumber(toolStripTextBoxSearch.Text, out long id))
            {
                return;
            }
            if (!SelectId((int)id))
            {
                toolStripStatusLabel.Text = $"Id {id} not found in this list.";
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
                    thumbnailGrid.SelectedIndex = index;
                }
                return false;
            }
            thumbnailGrid.SelectedIndex = index;
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
                toolStripStatusLabel.Text = $"Scan finished: {hits} names found between {first} and {last}.";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ------------------------------------------------------------------ editing

        private DdsSaveFormat? ChosenFormat()
        {
            return toolStripComboBoxFormat.SelectedIndex switch
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
            if (!toolStripButtonMipmaps.Checked)
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
                thumbnailGrid.InvalidateItem(thumbnailGrid.SelectedIndex);
                ShowSelected();
                toolStripStatusLabel.Text = $"Image {GetLabel(thumbnailGrid.SelectedIndex)} replaced. Save the file to keep the change.";
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
                    thumbnailGrid.SelectedIndex = index;
                }
                toolStripStatusLabel.Text = $"{name} added. Save the file to keep the change.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Adding the image failed.", ex);
            }
        }

        private void RemoveSelected()
        {
            ArtItem item = SelectedItem;
            if (item == null || !EcDialogs.Confirm(this, $"Remove image {GetLabel(thumbnailGrid.SelectedIndex)} from {Path.GetFileName(_package.FilePath)}?"))
            {
                return;
            }

            int index = thumbnailGrid.SelectedIndex;
            _package.Remove(item.Entry);
            BuildItems();
            thumbnailGrid.SelectedIndex = Math.Min(index, _items.Count - 1);
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
                thumbnailGrid.ClearCache();
                thumbnailGrid.Invalidate();
                ShowSelected();
                toolStripStatusLabel.Text = $"Saved {target}.";
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
                            toolStripStatusLabel.Text = $"Exporting... {done + failed} / {_items.Count}";
                            Application.DoEvents();
                        }
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
                toolStripStatusLabel.Text = $"Exported {done} images to {dialog.SelectedPath}" + (failed > 0 ? $" ({failed} failed)." : ".");
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

        // ==================================================================== event handlers (designer)

        private void OnSourceChanged(object sender, EventArgs e)
        {
            if (_loaded)
            {
                LoadSource(null);
            }
        }

        private void OnFileChanged(object sender, EventArgs e)
        {
            if (_loaded && !_updatingFileBox)
            {
                LoadSource(toolStripComboBoxFile.SelectedItem as string);
            }
        }

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Search();
            }
        }

        private void OnClickSearch(object sender, EventArgs e) => Search();

        private void OnSizeChanged(object sender, EventArgs e) => thumbnailGrid.CellSize = int.Parse((string)toolStripComboBoxSize.SelectedItem);

        private void OnUnnamedChanged(object sender, EventArgs e)
        {
            if (_package != null)
            {
                BuildItems();
            }
        }

        private void OnClickScanIds(object sender, EventArgs e) => ScanIds();

        private void OnClickReplace(object sender, EventArgs e) => ReplaceSelected();

        private void OnClickInsert(object sender, EventArgs e) => InsertNew();

        private void OnClickRemove(object sender, EventArgs e) => RemoveSelected();

        private void OnClickExportPng(object sender, EventArgs e) => ExportSelected(ImageFormat.Png, ".png");

        private void OnClickExportBmp(object sender, EventArgs e) => ExportSelected(ImageFormat.Bmp, ".bmp");

        private void OnClickExportDds(object sender, EventArgs e) => ExportSelectedDds();

        private void OnClickExportAll(object sender, EventArgs e) => ExportAll();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnGridSelectionChanged(object sender, EventArgs e) => ShowSelected();

        private void OnClickCopyId(object sender, EventArgs e)
        {
            ArtItem item = SelectedItem;
            if (item != null)
            {
                Clipboard.SetText(item.Id.ToString());
            }
        }

        private void OnClickZoomOut(object sender, EventArgs e) => previewPanel.Zoom /= 2;

        private void OnClickZoom1(object sender, EventArgs e) => previewPanel.Zoom = 1;

        private void OnClickZoomIn(object sender, EventArgs e) => previewPanel.Zoom *= 2;

    }
}
