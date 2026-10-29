/***************************************************************************
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
    public sealed class EcUopBrowserControl : UserControl, IEcEditor
    {
        private readonly ListBox _fileList;
        private readonly ListView _entryList;
        private readonly ToolStripTextBox _filterBox;
        private readonly ToolStripComboBox _folderBox;
        private bool _updatingFolders;
        private readonly EcPreviewPanel _preview;
        private readonly TextBox _hexBox;
        private readonly Label _infoLabel;
        private readonly ToolStripStatusLabel _statusLabel;
        private readonly ToolStripButton _saveButton;

        private EcClient _client;
        private EcUopPackage _package;
        private string _packageFileName;
        private List<EcUopEntry> _view = new List<EcUopEntry>();
        private bool _loaded;

        public EcUopBrowserControl()
        {
            Dock = DockStyle.Fill;

            _fileList = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
            _fileList.SelectedIndexChanged += (s, e) => OpenSelectedFile();

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _filterBox = new ToolStripTextBox { Width = 160, ToolTipText = "Filter by name or hash – Enter" };
            _filterBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplyFilter(); } };
            _saveButton = new ToolStripButton("Save file", null, (s, e) => Save(false)) { Enabled = false };

            _folderBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260, ToolTipText = "Show only one folder", MaxDropDownItems = 30 };
            _folderBox.SelectedIndexChanged += (s, e) => { if (!_updatingFolders) { ApplyFilter(); } };

            toolStrip.Items.Add(new ToolStripLabel("Folder:"));
            toolStrip.Items.Add(_folderBox);
            toolStrip.Items.Add(new ToolStripLabel("Filter:"));
            toolStrip.Items.Add(_filterBox);
            toolStrip.Items.Add(new ToolStripButton("Apply", null, (s, e) => ApplyFilter()));
            toolStrip.Items.Add(new ToolStripButton("Open", null, (s, e) => OpenExternally()) { ToolTipText = "Open the selected entry with the Windows program for its type (play mp3/wav, edit xml/lua, ...)" });
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Resolve names...", null, (s, e) => ResolveNames()) { ToolTipText = "Find the file names of the entries with a name pattern like build/worldart/{0:D8}.dds" });
            toolStrip.Items.Add(new ToolStripSeparator());
            var exportButton = new ToolStripDropDownButton("Export");
            exportButton.DropDownItems.Add("Selected entries...", null, (s, e) => ExportSelected());
            exportButton.DropDownItems.Add("All entries...", null, (s, e) => Export(_view));
            exportButton.DropDownItems.Add("Entry list (csv)...", null, (s, e) => ExportList());
            toolStrip.Items.Add(exportButton);
            toolStrip.Items.Add(new ToolStripButton("Replace...", null, (s, e) => ReplaceSelected()) { ToolTipText = "Replace the data of the selected entry with a file" });
            toolStrip.Items.Add(new ToolStripButton("Add...", null, (s, e) => AddEntry()) { ToolTipText = "Add a file under a packed name" });
            toolStrip.Items.Add(new ToolStripButton("Remove", null, (s, e) => RemoveSelected()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            _entryList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                VirtualMode = true
            };
            _entryList.Columns.Add("#", 60);
            _entryList.Columns.Add("Name / hash", 280);
            _entryList.Columns.Add("Stored", 70);
            _entryList.Columns.Add("Size", 80, HorizontalAlignment.Right);
            _entryList.Columns.Add("Packed", 80, HorizontalAlignment.Right);
            _entryList.RetrieveVirtualItem += RetrieveItem;
            _entryList.SelectedIndexChanged += (s, e) => ShowSelected();

            _preview = new EcPreviewPanel { Dock = DockStyle.Fill };
            _hexBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = EcDialogs.MonoFont(8.5f), Visible = false };
            _infoLabel = new Label { Dock = DockStyle.Top, Height = 64, Padding = new Padding(4), Font = EcDialogs.MonoFont(8.5f) };

            var previewPanel = new Panel { Dock = DockStyle.Fill };
            previewPanel.Controls.Add(_preview);
            previewPanel.Controls.Add(_hexBox);
            previewPanel.Controls.Add(_infoLabel);

            var listAndPreview = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2 };
            listAndPreview.Panel1.Controls.Add(_entryList);
            listAndPreview.Panel2.Controls.Add(previewPanel);
            listAndPreview.HandleCreated += (s, e) => { try { listAndPreview.SplitterDistance = Math.Max(200, listAndPreview.Width - 380); } catch (InvalidOperationException) { } };

            var right = new Panel { Dock = DockStyle.Fill };
            right.Controls.Add(listAndPreview);
            right.Controls.Add(toolStrip);

            var fileGroup = new GroupBox { Text = "UOP files", Dock = DockStyle.Fill };
            fileGroup.Controls.Add(_fileList);

            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };
            split.Panel1.Controls.Add(fileGroup);
            split.Panel2.Controls.Add(right);
            split.HandleCreated += (s, e) => { try { split.SplitterDistance = 220; } catch (InvalidOperationException) { } };

            var status = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Choose the Enhanced Client folder on top.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            status.Items.Add(_statusLabel);

            Controls.Add(split);
            Controls.Add(status);
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _package = null;
            _view = new List<EcUopEntry>();
            _entryList.VirtualListSize = 0;
            _fileList.Items.Clear();
            _preview.Image = null;
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

            _fileList.BeginUpdate();
            foreach (string file in _client.GetUopFiles())
            {
                _fileList.Items.Add($"{Path.GetFileName(file)}  ({EcImageHelper.FormatSize(new FileInfo(file).Length)})");
            }
            _fileList.EndUpdate();
            _statusLabel.Text = $"{_fileList.Items.Count} UOP files in {_client.Folder}";
        }

        private void OpenSelectedFile()
        {
            if (_fileList.SelectedItem is not string text)
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
                _saveButton.Enabled = _package.IsDirty;
            }
            catch (Exception ex)
            {
                _package = null;
                _view = new List<EcUopEntry>();
                _entryList.VirtualListSize = 0;
                _statusLabel.Text = $"{fileName}: {ex.Message}";
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

        private const string UnnamedFolder = "(entries without name)";

        /// <summary>
        /// Fills the folder list with the folders of the known names (up to 3 levels).
        /// </summary>
        private void UpdateFolders()
        {
            string selected = _folderBox.SelectedItem as string;
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
            _folderBox.Items.Clear();
            _folderBox.Items.Add("(all)");
            if (_package.Entries.Any(e => e.Name == null))
            {
                _folderBox.Items.Add(UnnamedFolder);
            }
            foreach (string f in folders.Take(3000))
            {
                _folderBox.Items.Add(f);
            }
            int index = selected != null ? _folderBox.Items.IndexOf(selected) : -1;
            _folderBox.SelectedIndex = index >= 0 ? index : 0;
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

            string filter = _filterBox.Text.Trim();
            IEnumerable<EcUopEntry> entries = _package.Entries;

            string folder = _folderBox.SelectedIndex > 0 ? _folderBox.SelectedItem as string : null;
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
            _entryList.VirtualListSize = _view.Count;
            _entryList.Invalidate();

            int named = _package.Entries.Count(e => e.Name != null);
            _statusLabel.Text = $"{_packageFileName}: {_package.Count} entries, {named} with known name, version {_package.Version}, block size {_package.BlockCapacity}" +
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
            foreach (int index in _entryList.SelectedIndices)
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
                _preview.Image = null;
                _infoLabel.Text = selected.Count > 1 ? $"{selected.Count} entries selected" : string.Empty;
                return;
            }

            EcUopEntry entry = selected[0];
            try
            {
                byte[] data = _package.ReadData(entry);
                Bitmap bitmap = EcImageHelper.TryDecode(data, out string info, entry.Name);
                _preview.Image = bitmap;
                _preview.Visible = bitmap != null;
                _hexBox.Visible = bitmap == null;
                if (bitmap == null)
                {
                    _hexBox.Text = EcImageHelper.IsText(data)
                        ? Encoding.Latin1.GetString(data, 0, Math.Min(data.Length, 200_000)).Replace("\r\n", "\n").Replace("\n", "\r\n")
                        : HexDump(data, 4096);
                }
                _infoLabel.Text = $"{entry.DisplayName}\r\nHash 0x{entry.Hash:X16}  block {entry.Block} slot {entry.Slot}\r\n{EcImageHelper.FormatSize(data.Length)}  {info ?? EcImageHelper.DescribeData(data)}";
            }
            catch (Exception ex)
            {
                _preview.Image = null;
                _hexBox.Visible = true;
                _preview.Visible = false;
                _hexBox.Text = ex.Message;
                _infoLabel.Text = entry.DisplayName;
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
                            _statusLabel.Text = $"Exporting... {done + failed} / {entries.Count}";
                            Application.DoEvents();
                        }
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
                _statusLabel.Text = $"Exported {done} entries to {dialog.SelectedPath}" + (failed > 0 ? $" ({failed} failed – unknown compression)." : ".");
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
                    _entryList.Invalidate();
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
                _filterBox.Text = string.Empty;
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
            _entryList.SelectedIndices.Clear();
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
                _statusLabel.Text = $"Saved {target}";
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
    }
}
