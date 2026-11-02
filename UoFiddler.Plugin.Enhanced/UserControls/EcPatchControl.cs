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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Patch packages: collect changed entries of the client (or files from disk), save them as one .ecpatch file
    /// and apply such a file to any client folder (with backup).
    /// </summary>
    public sealed partial class EcPatchControl : UserControl, IEcEditor
    {
        private EcClient _client;
        private EcPatch _patch = new EcPatch();
        private bool _updating;

        public EcPatchControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            ShowPatch();
            ShowHelp();
        }

        public void SetClient(EcClient client) => _client = client;

        public void OnActivated()
        {
        }

        private void ShowPatch()
        {
            _updating = true;
            textBoxTitle.Text = _patch.Title;
            textBoxAuthor.Text = _patch.Author;
            textBoxDescription.Text = _patch.Description;
            _updating = false;
            listViewEntries.BeginUpdate();
            listViewEntries.Items.Clear();
            foreach (EcPatchEntry e in _patch.Entries.OrderBy(x => x.File, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase))
            {
                var item = new ListViewItem(e.File) { Tag = e };
                item.SubItems.Add(e.DisplayName);
                item.SubItems.Add(e.Action == EcPatchAction.Remove ? "remove" : "add / replace");
                item.SubItems.Add(e.Data == null ? string.Empty : EcImageHelper.FormatSize(e.Data.Length));
                listViewEntries.Items.Add(item);
            }
            listViewEntries.EndUpdate();
            toolStripStatusLabel.Text = $"{_patch.Entries.Count} entries in {_patch.Files.Count()} files.";
        }

        private void NewPatch()
        {
            if (_patch.Entries.Count > 0 && !EcDialogs.Confirm(this, "Start a new, empty patch?"))
            {
                return;
            }
            _patch = new EcPatch();
            ShowPatch();
        }

        private void OpenPatch()
        {
            using (var dialog = new OpenFileDialog { Filter = $"Patch (*{EcPatch.Extension})|*{EcPatch.Extension}|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    _patch = EcPatch.Load(dialog.FileName);
                    ShowPatch();
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "The patch can't be read.", ex);
                }
            }
        }

        private void SavePatch()
        {
            if (_patch.Entries.Count == 0)
            {
                EcDialogs.ShowError(this, "The patch is empty.");
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = $"Patch (*{EcPatch.Extension})|*{EcPatch.Extension}", FileName = MakeFileName(_patch.Title) + EcPatch.Extension })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    _patch.Created = DateTime.Now;
                    _patch.Save(dialog.FileName);
                    toolStripStatusLabel.Text = $"Saved {dialog.FileName} ({EcImageHelper.FormatSize(new FileInfo(dialog.FileName).Length)}).";
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "The patch can't be written.", ex);
                }
            }
        }

        private static string MakeFileName(string title)
        {
            string name = string.Concat((title ?? "patch").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();
            return name.Length == 0 ? "patch" : name;
        }

        private void AddChanges()
        {
            if (_client == null)
            {
                EcDialogs.ShowError(this, "Choose the Enhanced Client folder on top first.");
                return;
            }
            _client.ApplyPendingChanges();
            int count = 0;
            foreach (EcUopPackage package in _client.OpenPackages)
            {
                if (package.FilePath != null)
                {
                    count += _patch.AddChanges(package, Path.GetFileName(package.FilePath));
                }
            }
            ShowPatch();
            toolStripStatusLabel.Text = count == 0
                ? "No unsaved changes in the open files. (Saved changes are not known any more – add those files with \"Add file...\".)"
                : $"{count} changed entries added.";
        }

        private void AddFile()
        {
            string[] files = _client?.GetUopFiles().Select(Path.GetFileName).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray() ?? Array.Empty<string>();
            using (var dialog = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "File that becomes the entry" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                string[] values = EcDialogs.Prompt(this, "Add file", new[] { $"UOP file ({string.Join(", ", files.Take(6))}{(files.Length > 6 ? ", ..." : string.Empty)}):", "Packed name (e.g. build/worldart/00012345.dds):" },
                    new[] { files.FirstOrDefault(f => f.Equals("Texture.uop", StringComparison.OrdinalIgnoreCase)) ?? "Texture.uop", string.Empty });
                if (values == null || values[0].Trim().Length == 0 || values[1].Trim().Length == 0)
                {
                    return;
                }
                string name = values[1].Trim().Replace('\\', '/').ToLowerInvariant();
                _patch.Set(new EcPatchEntry
                {
                    File = values[0].Trim(),
                    Name = name,
                    Hash = EcUopHash.HashFileName(name),
                    Action = EcPatchAction.Replace,
                    Data = File.ReadAllBytes(dialog.FileName)
                });
                ShowPatch();
            }
        }

        private void RemoveSelected()
        {
            foreach (ListViewItem item in listViewEntries.SelectedItems)
            {
                _patch.Entries.Remove((EcPatchEntry)item.Tag);
            }
            ShowPatch();
        }

        private async void Apply()
        {
            if (_patch.Entries.Count == 0)
            {
                EcDialogs.ShowError(this, "The patch is empty.");
                return;
            }
            using (var dialog = new FolderBrowserDialog { Description = "Enhanced Client folder the patch is written into", SelectedPath = _client?.Folder ?? string.Empty })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                string folder = dialog.SelectedPath;
                string files = string.Join(", ", _patch.Files);
                if (!EcDialogs.Confirm(this, $"Write {_patch.Entries.Count} entries into {files} in\r\n{folder}?\r\n\r\nThe Enhanced Client must be closed."))
                {
                    return;
                }
                bool loaded = _client != null && string.Equals(Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(_client.Folder).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
                if (loaded && _client.HasUnsavedChanges && !EcDialogs.Confirm(this, "This is the loaded client and it has unsaved changes. They are lost when the patched files are read again. Continue?"))
                {
                    return;
                }
                var backup = new EcBackup("Patches") { Enabled = checkBoxBackup.Checked };
                var progress = new Progress<int>(p => toolStripProgressBar.Value = Math.Max(0, Math.Min(100, p)));
                Cursor = Cursors.WaitCursor;
                Enabled = false;
                try
                {
                    // the loaded client keeps its files open: close them first
                    if (loaded)
                    {
                        foreach (string file in _patch.Files)
                        {
                            if (_client.IsOpen(file))
                            {
                                _client.GetPackage(file).Dispose();
                            }
                        }
                    }
                    string report = await Task.Run(() => _patch.Apply(folder, p => backup.BackupExisting(p), p => backup.CopyNew(p), progress));
                    if (loaded)
                    {
                        foreach (string file in _patch.Files.Where(f => _client.IsOpen(f)))
                        {
                            _client.ReloadPackage(file);
                        }
                    }
                    MessageBox.Show(this, report + (checkBoxBackup.Checked ? "\r\nBackup: UoFiddlerBackups\\Patches next to the files." : string.Empty) +
                                          (loaded ? "\r\nPress \"Reload\" on top to see the changes in every tab." : string.Empty), "Patch applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    toolStripStatusLabel.Text = "Patch applied.";
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "The patch could not be applied.", ex);
                }
                finally
                {
                    Enabled = true;
                    Cursor = Cursors.Default;
                    toolStripProgressBar.Value = 0;
                }
            }
        }

        private void ShowHelp() => richTextBoxHelp.Text = radioButtonHelpGerman.Checked ? ToolsHelp.PatchGerman : ToolsHelp.PatchEnglish;

        // ==================================================================== event handlers (designer)

        private void OnClickNew(object sender, EventArgs e) => NewPatch();

        private void OnClickOpen(object sender, EventArgs e) => OpenPatch();

        private void OnClickSave(object sender, EventArgs e) => SavePatch();

        private void OnClickAddChanges(object sender, EventArgs e) => AddChanges();

        private void OnClickAddFile(object sender, EventArgs e) => AddFile();

        private void OnClickRemove(object sender, EventArgs e) => RemoveSelected();

        private void OnClickApply(object sender, EventArgs e) => Apply();

        private void OnInfoChanged(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }
            _patch.Title = textBoxTitle.Text;
            _patch.Author = textBoxAuthor.Text;
            _patch.Description = textBoxDescription.Text;
        }

        private void OnEntriesKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelected();
            }
        }

        private void OnHelpLanguageChanged(object sender, EventArgs e) => ShowHelp();
    }
}
