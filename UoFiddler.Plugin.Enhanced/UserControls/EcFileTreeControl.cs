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
    /// Folder tree of the UOP files that store real paths (Interface.uop with the UI windows / lua / textures,
    /// GameData.uop with csv tables, MainMisc, Shaders, ...). Text files can be edited directly.
    /// </summary>
    public sealed partial class EcFileTreeControl : UserControl, IEcEditor
    {
        private static readonly string[] _defaultFiles =
        {
            "Interface.uop", "GameData.uop", "MainMisc.uop", "Shaders.uop", "SystemTextures.uop", "LegacyTerrain.uop", "EffectTexture.uop", "TerrainDefinition.uop"
        };

        private EcClient _client;
        private bool _loaded;
        private EcUopPackage _package;
        private EcUopEntry _current;
        private Encoding _currentEncoding;
        private bool _currentBom;

        public EcFileTreeControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _loaded = false;
            _package = null;
            _current = null;
            treeViewFiles.Nodes.Clear();
            toolStripComboBoxFile.Items.Clear();
            toolStripButtonSave.Enabled = false;
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
            foreach (string f in _defaultFiles.Where(f => _client.FindFile(f) != null))
            {
                toolStripComboBoxFile.Items.Add(f);
            }
            if (toolStripComboBoxFile.Items.Count > 0)
            {
                toolStripComboBoxFile.SelectedIndex = 0;
                OpenFile();
            }
            else
            {
                toolStripStatusLabel.Text = "None of the path based UOP files was found.";
            }
        }

        private void OpenFile()
        {
            if (toolStripComboBoxFile.SelectedItem is not string file)
            {
                return;
            }
            Cursor = Cursors.WaitCursor;
            try
            {
                _package = _client.GetPackage(file);
                _package.Changed -= PackageChanged;
                _package.Changed += PackageChanged;
                FillTree();
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, $"{file} can't be read.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void PackageChanged(object sender, EventArgs e)
        {
            toolStripButtonSave.Enabled = _defaultFiles.Any(f => _client.IsOpen(f) && (_client.GetPackage(f)?.IsDirty ?? false));
        }

        private void FillTree()
        {
            if (_package == null)
            {
                return;
            }
            string filter = toolStripTextBoxFilter.Text.Trim();
            treeViewFiles.BeginUpdate();
            treeViewFiles.Nodes.Clear();
            var folders = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);
            int named = 0;

            foreach (EcUopEntry entry in _package.Entries.Where(e => e.Name != null).OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
            {
                if (filter.Length > 0 && entry.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }
                named++;
                string[] parts = entry.Name.Replace('\\', '/').Split('/');
                TreeNodeCollection nodes = treeViewFiles.Nodes;
                string path = string.Empty;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    path += parts[i].ToLowerInvariant() + "/";
                    if (!folders.TryGetValue(path, out TreeNode folder))
                    {
                        folder = new TreeNode(parts[i]) { Tag = path };
                        nodes.Add(folder);
                        folders[path] = folder;
                    }
                    nodes = folder.Nodes;
                }
                nodes.Add(new TreeNode(parts[parts.Length - 1]) { Tag = entry, ForeColor = entry.IsModified ? Color.DarkBlue : SystemColors.WindowText });
            }

            List<EcUopEntry> unnamed = _package.Entries.Where(e => e.Name == null).ToList();
            if (unnamed.Count > 0 && filter.Length == 0)
            {
                var node = new TreeNode($"(without name: {unnamed.Count})") { Tag = "?" };
                foreach (EcUopEntry e in unnamed)
                {
                    node.Nodes.Add(new TreeNode($"{e.Hash:X16}") { Tag = e });
                }
                treeViewFiles.Nodes.Add(node);
            }

            if (treeViewFiles.Nodes.Count == 1)
            {
                treeViewFiles.Nodes[0].Expand();
            }
            treeViewFiles.EndUpdate();
            toolStripStatusLabel.Text = $"{toolStripComboBoxFile.SelectedItem}: {_package.Count} entries, {_package.Entries.Count(e => e.Name != null)} with name" + (filter.Length > 0 ? $", {named} shown" : string.Empty);
        }

        private EcUopEntry SelectedEntry => treeViewFiles.SelectedNode?.Tag as EcUopEntry;

        private void ShowSelected()
        {
            _current = SelectedEntry;
            buttonApply.Enabled = false;
            if (_current == null)
            {
                textBoxText.Visible = false;
                previewPanel.Visible = false;
                labelInfo.Text = treeViewFiles.SelectedNode?.Tag as string ?? string.Empty;
                return;
            }

            try
            {
                byte[] data = _package.ReadData(_current);
                Bitmap image = EcImageHelper.TryDecode(data, out string info, _current.Name);
                if (image != null)
                {
                    previewPanel.Image = image;
                    previewPanel.Visible = true;
                    textBoxText.Visible = false;
                }
                else
                {
                    previewPanel.Image = null;
                    previewPanel.Visible = false;
                    textBoxText.Visible = true;
                    if (EcImageHelper.IsText(data) || (data.Length > 1 && data[0] == 0xFF && data[1] == 0xFE))
                    {
                        textBoxText.Text = DecodeText(data);
                        textBoxText.ReadOnly = false;
                    }
                    else
                    {
                        _currentEncoding = null;
                        textBoxText.Text = HexDump(data, 8192);
                        textBoxText.ReadOnly = true;
                    }
                }
                labelInfo.Text = $"{_current.DisplayName}\r\n{EcImageHelper.FormatSize(data.Length)}  {info ?? EcImageHelper.DescribeData(data)}";
            }
            catch (Exception ex)
            {
                textBoxText.Visible = true;
                textBoxText.ReadOnly = true;
                textBoxText.Text = ex.Message;
            }
        }

        private string DecodeText(byte[] data)
        {
            if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xFE)
            {
                _currentEncoding = Encoding.Unicode;
                _currentBom = true;
                return Encoding.Unicode.GetString(data, 2, data.Length - 2).Replace("\r\n", "\n").Replace("\n", "\r\n");
            }
            bool bom = data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF;
            _currentEncoding = new UTF8Encoding(false);
            _currentBom = bom;
            string text = Encoding.UTF8.GetString(data, bom ? 3 : 0, data.Length - (bom ? 3 : 0));
            _lineEnding = text.Contains("\r\n") ? "\r\n" : "\n";
            return text.Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        private string _lineEnding = "\r\n";

        private void ApplyText()
        {
            if (_current == null || _currentEncoding == null || textBoxText.ReadOnly)
            {
                return;
            }
            string text = textBoxText.Text.Replace("\r\n", "\n");
            if (_lineEnding == "\r\n" || _currentEncoding == Encoding.Unicode)
            {
                text = text.Replace("\n", "\r\n");
            }
            byte[] body = _currentEncoding.GetBytes(text);
            byte[] bom = !_currentBom ? Array.Empty<byte>() : _currentEncoding == Encoding.Unicode ? new byte[] { 0xFF, 0xFE } : new byte[] { 0xEF, 0xBB, 0xBF };
            _package.SetData(_current, bom.Concat(body).ToArray());
            buttonApply.Enabled = false;
            if (treeViewFiles.SelectedNode != null)
            {
                treeViewFiles.SelectedNode.ForeColor = Color.DarkBlue;
            }
            toolStripStatusLabel.Text = $"{_current.DisplayName} changed. Save the file to keep it.";
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

        private void OpenExternally()
        {
            if (_current == null)
            {
                return;
            }
            try
            {
                string folder = Path.Combine(Path.GetTempPath(), "UoFiddlerEnhanced");
                Directory.CreateDirectory(folder);
                string name = _current.Name != null ? Path.GetFileName(_current.Name) : $"{_current.Hash:X16}.bin";
                string file = Path.Combine(folder, name);
                File.WriteAllBytes(file, _package.ReadData(_current));
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "The file can't be opened.", ex);
            }
        }

        private void ExportSelected()
        {
            if (_current == null)
            {
                return;
            }
            string name = _current.Name != null ? Path.GetFileName(_current.Name) : $"{_current.Hash:X16}.bin";
            using (var dialog = new SaveFileDialog { FileName = name, Filter = "All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllBytes(dialog.FileName, _package.ReadData(_current));
                }
            }
        }

        private void ExportFolder()
        {
            TreeNode node = treeViewFiles.SelectedNode;
            if (node == null)
            {
                return;
            }
            using (var dialog = new FolderBrowserDialog { Description = "Export folder (the paths are kept)" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                int count = 0;
                foreach (EcUopEntry entry in Collect(node))
                {
                    string relative = entry.Name ?? $"unknown/{entry.Hash:X16}.bin";
                    string path = Path.Combine(dialog.SelectedPath, relative.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, _package.ReadData(entry));
                    count++;
                }
                toolStripStatusLabel.Text = $"{count} files exported.";
            }
        }

        private static IEnumerable<EcUopEntry> Collect(TreeNode node)
        {
            if (node.Tag is EcUopEntry e)
            {
                yield return e;
            }
            foreach (TreeNode child in node.Nodes)
            {
                foreach (EcUopEntry x in Collect(child))
                {
                    yield return x;
                }
            }
        }

        private void ReplaceSelected()
        {
            if (_current == null)
            {
                return;
            }
            using (var dialog = new OpenFileDialog { Filter = "All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _package.SetData(_current, File.ReadAllBytes(dialog.FileName));
                    ShowSelected();
                }
            }
        }

        private void AddFile()
        {
            if (_package == null)
            {
                return;
            }
            using (var dialog = new OpenFileDialog { Filter = "All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                string folder = treeViewFiles.SelectedNode?.Tag as string;
                if (folder == null && _current?.Name != null)
                {
                    folder = _current.Name.Substring(0, _current.Name.LastIndexOf('/') + 1);
                }
                string name = EcDialogs.Prompt(this, "Add file", "Packed path:", (folder ?? "data/") + Path.GetFileName(dialog.FileName));
                if (string.IsNullOrWhiteSpace(name))
                {
                    return;
                }
                name = name.Trim().Replace('\\', '/');
                if (_package.Contains(name) && !EcDialogs.Confirm(this, $"{name} exists already. Replace it?"))
                {
                    return;
                }
                _package.AddOrReplace(name, File.ReadAllBytes(dialog.FileName), _package.PreferredCompression);
                FillTree();
            }
        }

        private void RemoveSelected()
        {
            if (_current == null || !EcDialogs.Confirm(this, $"Remove {_current.DisplayName}?"))
            {
                return;
            }
            _package.Remove(_current);
            _current = null;
            FillTree();
        }

        private void Save(bool saveAs)
        {
            var packages = _defaultFiles.Where(f => _client != null && _client.IsOpen(f)).Select(f => _client.GetPackage(f));
            if (EcPackageSaver.Save(this, packages, saveAs))
            {
                toolStripButtonSave.Enabled = false;
                FillTree();
                toolStripStatusLabel.Text = "Saved.";
            }
        }

        // ==================================================================== event handlers (designer)

        private void OnFileChanged(object sender, EventArgs e)
        {
            if (_loaded)
            {
                OpenFile();
            }
        }

        private void OnFilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                FillTree();
            }
        }

        private void OnClickApply(object sender, EventArgs e) => FillTree();

        private void OnClickOpen(object sender, EventArgs e) => OpenExternally();

        private void OnClickExportFile(object sender, EventArgs e) => ExportSelected();

        private void OnClickExportFolder(object sender, EventArgs e) => ExportFolder();

        private void OnClickReplace(object sender, EventArgs e) => ReplaceSelected();

        private void OnClickAdd(object sender, EventArgs e) => AddFile();

        private void OnClickRemove(object sender, EventArgs e) => RemoveSelected();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnAfterSelect(object sender, TreeViewEventArgs e) => ShowSelected();

        /// <summary>Right click selects the node first, so the context menu works on it.</summary>
        private void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeViewFiles.SelectedNode = e.Node;
            }
        }

        private void OnClickCopyPath(object sender, EventArgs e)
        {
            string path = (treeViewFiles.SelectedNode?.Tag as EcUopEntry)?.Name ?? treeViewFiles.SelectedNode?.FullPath;
            if (!string.IsNullOrEmpty(path))
            {
                Clipboard.SetText(path);
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (textBoxText.Focused)
            {
                buttonApply.Enabled = true;
            }
        }

        private void OnClickApplyText(object sender, EventArgs e) => ApplyText();

    }
}
