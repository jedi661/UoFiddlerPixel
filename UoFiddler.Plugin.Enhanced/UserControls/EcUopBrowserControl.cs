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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Generic browser for every UOP file of the client folder: list entries, show images / hex, export, replace, add, remove, save.
    /// </summary>
    public sealed partial class EcUopBrowserControl : UserControl, IEcEditor
    {
        private bool _updatingFolders;

        private EcClient _client;
        private EcUopPackage _package;
        private string _packageFileName;
        private List<EcUopEntry> _view = new List<EcUopEntry>();
        private bool _loaded;

        public EcUopBrowserControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _package = null;
            _view = new List<EcUopEntry>();
            listViewEntries.VirtualListSize = 0;
            listBoxFiles.Items.Clear();
            previewPanel.Image = null;
            _loaded = false;
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
            _loaded = true;

            listBoxFiles.BeginUpdate();
            foreach (string file in _client.GetUopFiles())
            {
                listBoxFiles.Items.Add($"{Path.GetFileName(file)}  ({EcImageHelper.FormatSize(new FileInfo(file).Length)})");
            }
            listBoxFiles.EndUpdate();
            toolStripStatusLabel.Text = $"{listBoxFiles.Items.Count} UOP files in {_client.Folder}";
        }

        private void OpenSelectedFile()
        {
            if (listBoxFiles.SelectedItem is not string text)
            {
                return;
            }

            string fileName = text.Substring(0, text.IndexOf("  (", StringComparison.Ordinal));
            Cursor = Cursors.WaitCursor;
            try
            {
                if (_package != null)
                {
                    _package.Changed -= PackageChanged;
                }

                _package = _client.GetPackage(fileName);
                _packageFileName = fileName;
                _package.Changed += PackageChanged;
                UpdateFolders();
                ApplyFilter();
                toolStripButtonSave.Enabled = _package.IsDirty;
            }
            catch (Exception ex)
            {
                _package = null;
                _view = new List<EcUopEntry>();
                listViewEntries.VirtualListSize = 0;
                toolStripStatusLabel.Text = $"{fileName}: {ex.Message}";
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

        private const string UnnamedFolder = "(entries without name)";

        /// <summary>
        /// Fills the folder list with the folders of the known names (up to 3 levels).
        /// </summary>
        private void UpdateFolders()
        {
            string selected = toolStripComboBoxFolder.SelectedItem as string;
            var folders = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (EcUopEntry entry in _package.Entries.Where(e => e.Name != null))
            {
                string[] parts = entry.Name.Replace('\\', '/').Split('/');
                string path = string.Empty;
                for (int i = 0; i < parts.Length - 1 && i < 5; i++)
                {
                    path += parts[i] + "/";
                    folders.Add(path);
                }
            }

            _updatingFolders = true;
            toolStripComboBoxFolder.Items.Clear();
            toolStripComboBoxFolder.Items.Add("(all)");
            if (_package.Entries.Any(e => e.Name == null))
            {
                toolStripComboBoxFolder.Items.Add(UnnamedFolder);
            }
            foreach (string f in folders.Take(3000))
            {
                toolStripComboBoxFolder.Items.Add(f);
            }
            int index = selected != null ? toolStripComboBoxFolder.Items.IndexOf(selected) : -1;
            toolStripComboBoxFolder.SelectedIndex = index >= 0 ? index : 0;
            _updatingFolders = false;
        }

        /// <summary>
        /// Writes the entry into the temp folder and opens it with the program Windows uses for the file type.
        /// </summary>
        private void OpenExternally()
        {
            List<EcUopEntry> selected = SelectedEntries();
            if (selected.Count != 1)
            {
                return;
            }

            try
            {
                EcUopEntry entry = selected[0];
                byte[] data = _package.ReadData(entry);
                string extension = entry.Name != null ? Path.GetExtension(entry.Name) : GuessExtension(data);
                string name = entry.Name != null ? Path.GetFileNameWithoutExtension(entry.Name) : $"{entry.Hash:X16}";
                string folder = Path.Combine(Path.GetTempPath(), "UoFiddlerEnhanced");
                Directory.CreateDirectory(folder);
                string file = Path.Combine(folder, name + extension);
                File.WriteAllBytes(file, data);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "The entry can't be opened.", ex);
            }
        }

        private static string GuessExtension(byte[] data)
        {
            string type = UoFiddler.Plugin.Enhanced.EcUltima.Analysis.EcFolderAnalyzer.Describe(data);
            if (type.StartsWith("DDS", StringComparison.Ordinal)) return ".dds";
            if (type.StartsWith("TGA", StringComparison.Ordinal)) return ".tga";
            if (type.StartsWith("PNG", StringComparison.Ordinal)) return ".png";
            if (type.StartsWith("JPEG", StringComparison.Ordinal)) return ".jpg";
            if (type.StartsWith("BMP", StringComparison.Ordinal)) return ".bmp";
            if (type.StartsWith("RIFF", StringComparison.Ordinal)) return ".wav";
            if (type.StartsWith("MP3", StringComparison.Ordinal)) return ".mp3";
            if (type.StartsWith("OGG", StringComparison.Ordinal)) return ".ogg";
            if (type.StartsWith("PSD", StringComparison.Ordinal)) return ".psd";
            if (type.StartsWith("Gamebryo", StringComparison.Ordinal)) return ".nif";
            if (type.StartsWith("text (XML)", StringComparison.Ordinal)) return ".xml";
            if (type.StartsWith("text", StringComparison.Ordinal)) return ".txt";
            if (data.Length > 4 && data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0) return ".xls";
            return ".bin";
        }

        private void ApplyFilter()
        {
            if (_package == null)
            {
                return;
            }

            string filter = toolStripTextBoxFilter.Text.Trim();
            IEnumerable<EcUopEntry> entries = _package.Entries;

            string folder = toolStripComboBoxFolder.SelectedIndex > 0 ? toolStripComboBoxFolder.SelectedItem as string : null;
            if (folder == UnnamedFolder)
            {
                entries = entries.Where(e => e.Name == null);
            }
            else if (folder != null)
            {
                entries = entries.Where(e => e.Name != null && e.Name.Replace('\\', '/').StartsWith(folder, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.Length > 0)
            {
                entries = entries.Where(e => e.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _view = entries.OrderBy(e => e.Name == null).ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToList();
            listViewEntries.VirtualListSize = _view.Count;
            listViewEntries.Invalidate();

            int named = _package.Entries.Count(e => e.Name != null);
            toolStripStatusLabel.Text = $"{_packageFileName}: {_package.Count} entries, {named} with known name, version {_package.Version}, block size {_package.BlockCapacity}" +
                                (filter.Length > 0 ? $" – {_view.Count} shown" : string.Empty);
        }

        private void RetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }

            EcUopEntry entry = _view[e.ItemIndex];
            string stored = entry.Flag switch { 0 => "raw", 1 => "zlib", _ => "flag " + entry.Flag };
            var item = new ListViewItem((e.ItemIndex + 1).ToString());
            item.SubItems.Add(entry.DisplayName + (entry.IsModified ? " *" : string.Empty));
            item.SubItems.Add(stored);
            item.SubItems.Add(EcImageHelper.FormatSize(entry.DecompressedSize));
            item.SubItems.Add(entry.IsModified ? "-" : EcImageHelper.FormatSize(entry.Flag == 0 ? entry.DecompressedSize : entry.CompressedSize));
            if (entry.IsModified)
            {
                item.ForeColor = Color.DarkBlue;
            }
            e.Item = item;
        }

        private List<EcUopEntry> SelectedEntries()
        {
            var list = new List<EcUopEntry>();
            foreach (int index in listViewEntries.SelectedIndices)
            {
                if (index >= 0 && index < _view.Count)
                {
                    list.Add(_view[index]);
                }
            }
            return list;
        }

        private void ShowSelected()
        {
            List<EcUopEntry> selected = SelectedEntries();
            if (selected.Count != 1)
            {
                previewPanel.Image = null;
                labelInfo.Text = selected.Count > 1 ? $"{selected.Count} entries selected" : string.Empty;
                return;
            }

            EcUopEntry entry = selected[0];
            try
            {
                byte[] data = _package.ReadData(entry);
                Bitmap bitmap = EcImageHelper.TryDecode(data, out string info, entry.Name);
                previewPanel.Image = bitmap;
                previewPanel.Visible = bitmap != null;
                textBoxHex.Visible = bitmap == null;
                if (bitmap == null)
                {
                    textBoxHex.Text = EcImageHelper.IsText(data)
                        ? Encoding.Latin1.GetString(data, 0, Math.Min(data.Length, 200_000)).Replace("\r\n", "\n").Replace("\n", "\r\n")
                        : HexDump(data, 4096);
                }
                labelInfo.Text = $"{entry.DisplayName}\r\nHash 0x{entry.Hash:X16}  block {entry.Block} slot {entry.Slot}\r\n{EcImageHelper.FormatSize(data.Length)}  {info ?? EcImageHelper.DescribeData(data)}";
            }
            catch (Exception ex)
            {
                previewPanel.Image = null;
                textBoxHex.Visible = true;
                previewPanel.Visible = false;
                textBoxHex.Text = ex.Message;
                labelInfo.Text = entry.DisplayName;
            }
        }

        private static string HexDump(byte[] data, int max)
        {
            var sb = new StringBuilder();
            int length = Math.Min(data.Length, max);
            for (int i = 0; i < length; i += 16)
            {
                sb.Append(i.ToString("X8")).Append("  ");
                for (int j = 0; j < 16; j++)
                {
                    sb.Append(i + j < length ? data[i + j].ToString("X2") + " " : "   ");
                }
                sb.Append(' ');
                for (int j = 0; j < 16 && i + j < length; j++)
                {
                    byte b = data[i + j];
                    sb.Append(b >= 32 && b < 127 ? (char)b : '.');
                }
                sb.AppendLine();
            }
            if (data.Length > max)
            {
                sb.AppendLine($"... {data.Length - max} more bytes");
            }
            return sb.ToString();
        }

        private void ResolveNames()
        {
            if (_package == null)
            {
                return;
            }

            string[] values = EcDialogs.Prompt(this, "Resolve names",
                new[] { "Pattern ({0} = id):", "First id:", "Last id:" },
                new[] { "build/worldart/{0:D8}.dds", "0", "0x20000" });
            if (values == null || !EcDialogs.TryParseNumber(values[1], out long first) || !EcDialogs.TryParseNumber(values[2], out long last) || last < first)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                int hits = values[0].Contains("{0") ? _package.ResolveNames(values[0], (int)first, (int)Math.Min(last, int.MaxValue)) : (_package.ResolveName(values[0]) ? 1 : 0);
                UpdateFolders();
                ApplyFilter();
                MessageBox.Show(this, $"{hits} names found.", "Resolve names");
            }
            catch (FormatException ex)
            {
                EcDialogs.ShowError(this, "Invalid pattern.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ExportSelected()
        {
            Export(SelectedEntries());
        }

        private void Export(IList<EcUopEntry> entries)
        {
            if (_package == null || entries.Count == 0)
            {
                return;
            }

            using (var dialog = new FolderBrowserDialog { Description = "Export folder (the packed folder structure is kept)" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Cursor = Cursors.WaitCursor;
                int done = 0, failed = 0;
                try
                {
                    foreach (EcUopEntry entry in entries)
                    {
                        try
                        {
                            string relative = entry.Name ?? $"unknown/{entry.Hash:X16}.bin";
                            string path = Path.Combine(dialog.SelectedPath, relative.Replace('/', Path.DirectorySeparatorChar));
                            Directory.CreateDirectory(Path.GetDirectoryName(path));
                            File.WriteAllBytes(path, _package.ReadData(entry));
                            done++;
                        }
                        catch (Exception)
                        {
                            failed++;
                        }

                        if ((done + failed) % 500 == 0)
                        {
                            toolStripStatusLabel.Text = $"Exporting... {done + failed} / {entries.Count}";
                            Application.DoEvents();
                        }
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
                toolStripStatusLabel.Text = $"Exported {done} entries to {dialog.SelectedPath}" + (failed > 0 ? $" ({failed} failed – unknown compression)." : ".");
            }
        }

        private void ExportList()
        {
            if (_package == null)
            {
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = "CSV|*.csv", FileName = Path.GetFileNameWithoutExtension(_packageFileName) + "_entries.csv" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine("Name;Hash;Flag;Size;Packed;Block;Slot");
                foreach (EcUopEntry e in _package.Entries)
                {
                    sb.AppendLine($"{e.Name};0x{e.Hash:X16};{e.Flag};{e.DecompressedSize};{e.CompressedSize};{e.Block};{e.Slot}");
                }
                File.WriteAllText(dialog.FileName, sb.ToString());
            }
        }

        private void ReplaceSelected()
        {
            List<EcUopEntry> selected = SelectedEntries();
            if (selected.Count != 1)
            {
                return;
            }

            using (var dialog = new OpenFileDialog { Title = $"Replace {selected[0].DisplayName}" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    _package.SetData(selected[0], File.ReadAllBytes(dialog.FileName));
                    listViewEntries.Invalidate();
                    ShowSelected();
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "Replacing failed.", ex);
                }
            }
        }

        private void AddEntry()
        {
            if (_package == null)
            {
                return;
            }

            using (var dialog = new OpenFileDialog { Title = "File to add" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string example = _package.Entries.FirstOrDefault(e => e.Name != null)?.Name ?? "build/folder/00000000.bin";
                string[] values = EcDialogs.Prompt(this, "Add entry", new[] { "Packed name:", "Compress (zlib) yes/no:" },
                    new[] { example, _package.PreferredCompression == EcUopCompression.Zlib ? "yes" : "no" });
                if (values == null || string.IsNullOrWhiteSpace(values[0]))
                {
                    return;
                }

                string name = values[0].Trim().Replace('\\', '/').ToLowerInvariant();
                if (_package.Contains(name) && !EcDialogs.Confirm(this, $"{name} already exists. Replace it?"))
                {
                    return;
                }

                bool zlib = values[1].Trim().StartsWith("y", StringComparison.OrdinalIgnoreCase) || values[1].Trim().StartsWith("j", StringComparison.OrdinalIgnoreCase);
                _package.AddOrReplace(name, File.ReadAllBytes(dialog.FileName), zlib ? EcUopCompression.Zlib : EcUopCompression.None);
                toolStripTextBoxFilter.Text = string.Empty;
                UpdateFolders();
                ApplyFilter();
            }
        }

        private void RemoveSelected()
        {
            List<EcUopEntry> selected = SelectedEntries();
            if (selected.Count == 0 || !EcDialogs.Confirm(this, $"Remove {selected.Count} entries from {_packageFileName}?"))
            {
                return;
            }

            foreach (EcUopEntry entry in selected)
            {
                _package.Remove(entry);
            }
            listViewEntries.SelectedIndices.Clear();
            ApplyFilter();
        }

        private void Save(bool saveAs)
        {
            if (_package == null)
            {
                return;
            }

            if (string.Equals(_packageFileName, EcClient.TileArtFileName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_packageFileName, EcClient.StringDictionaryFileName, StringComparison.OrdinalIgnoreCase))
            {
                // pending editor changes belong into the file before it is written
                _client.ApplyPendingChanges();
            }

            string target = _package.FilePath;
            if (saveAs)
            {
                using (var dialog = new SaveFileDialog { Filter = "UOP files (*.uop)|*.uop", FileName = _packageFileName })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    target = dialog.FileName;
                }
            }
            else if (!EcDialogs.Confirm(this, $"Save {_packageFileName}?\r\nThe old file is kept as .bak next to it.\r\n\r\nThe Enhanced Client must be closed."))
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
                ApplyFilter();
                toolStripStatusLabel.Text = $"Saved {target}";
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

        // ==================================================================== event handlers (designer)

        private void OnFileSelectedIndexChanged(object sender, EventArgs e) => OpenSelectedFile();

        private void OnFolderChanged(object sender, EventArgs e)
        {
            if (!_updatingFolders)
            {
                ApplyFilter();
            }
        }

        private void OnFilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyFilter();
            }
        }

        private void OnClickApply(object sender, EventArgs e) => ApplyFilter();

        private void OnClickOpen(object sender, EventArgs e) => OpenExternally();

        private void OnClickResolveNames(object sender, EventArgs e) => ResolveNames();

        private void OnClickExportSelected(object sender, EventArgs e) => ExportSelected();

        private void OnClickExportAll(object sender, EventArgs e) => Export(_view);

        private void OnClickExportList(object sender, EventArgs e) => ExportList();

        private void OnClickReplace(object sender, EventArgs e) => ReplaceSelected();

        private void OnClickAdd(object sender, EventArgs e) => AddEntry();

        private void OnClickRemove(object sender, EventArgs e) => RemoveSelected();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnEntrySelectedIndexChanged(object sender, EventArgs e) => ShowSelected();

        private EcUopEntry FirstSelectedEntry =>
            listViewEntries.SelectedIndices.Count > 0 && listViewEntries.SelectedIndices[0] < _view.Count ? _view[listViewEntries.SelectedIndices[0]] : null;

        private void OnClickCopyName(object sender, EventArgs e)
        {
            EcUopEntry entry = FirstSelectedEntry;
            if (entry?.Name != null)
            {
                Clipboard.SetText(entry.Name);
            }
        }

        private void OnClickCopyHash(object sender, EventArgs e)
        {
            EcUopEntry entry = FirstSelectedEntry;
            if (entry != null)
            {
                Clipboard.SetText($"0x{entry.Hash:X16}");
            }
        }

    }
}
