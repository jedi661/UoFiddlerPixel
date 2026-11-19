/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 * 
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using Ultima.Helpers;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Forms
{
    /// <summary>
    /// Path settings: Ultima Online folder with a quick list (favourites + last used folders), client info,
    /// the UOFiddler extension files (found and loaded automatically) and all client files with filter.
    /// </summary>
    public partial class PathSettingsForm : Form
    {
        private bool _fillingDirs;

        public PathSettingsForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _showBox.SelectedIndex = 0;

            _tips.SetToolTip(_dirBox, "Type a folder or pick one from the quick list (favourites first, then the last used folders). Enter = use it.");
            _tips.SetToolTip(_resetCheck, "On: every file is taken from the new folder.\nOff: files with their own path outside the old folder keep it.");
            _tips.SetToolTip(_saveFavButton, "Saves the folder with a name in the quick list (stored in UoDirectories.json, for every profile).");
            _tips.SetToolTip(_reloadButton, "Loads all files of the loaded tabs again from the current paths (like 'Reload files' in the main menu).");
            _tips.SetToolTip(_extList, "artext*, artlandext*, tiledata*ext, texmapsext, huesext, radarcol*ext - double click opens the folder.");
        }

        /// <summary>Kept for older code: the folder text box of the old window.</summary>
        public ComboBox tsTbRootPath => _dirBox;

        private void DisposeImages()
        {
            // nothing to dispose (the designer template calls it)
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            FillDirBox(Files.RootDir);
            RefreshAll();
            DetectExtended(true);
        }

        private void RefreshAll()
        {
            UpdateClientInfo();
            FillExtList();
            FillFileList();
        }

        #region [ Folder + quick list ]

        private void FillDirBox(string current)
        {
            _fillingDirs = true;
            try
            {
                _dirBox.Items.Clear();
                foreach (UoDirectoryEntry entry in UoDirectoryList.Entries)
                {
                    _dirBox.Items.Add(entry);
                }

                _dirBox.Text = current ?? string.Empty;
                UoDirectoryEntry fav = string.IsNullOrEmpty(current) ? null : UoDirectoryList.Get(current);
                _favNameBox.Text = fav?.Favourite == true ? fav.Name : (string.IsNullOrEmpty(current) ? string.Empty : Path.GetFileName(current.TrimEnd('\\', '/')));
            }
            finally
            {
                _fillingDirs = false;
            }
        }

        /// <summary>The folder in the box (a quick list entry or typed text).</summary>
        private string SelectedDir => _dirBox.SelectedItem is UoDirectoryEntry e && _dirBox.Text == e.ToString() ? e.Path : _dirBox.Text.Trim().Trim('"');

        private void OnDirSelected(object sender, EventArgs e)
        {
            if (_fillingDirs || !(_dirBox.SelectedItem is UoDirectoryEntry entry))
            {
                return;
            }

            ApplyRoot(entry.Path);
        }

        private void OnDirKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            ApplyRoot(SelectedDir);
        }

        private void OnApply(object sender, EventArgs e) => ApplyRoot(SelectedDir);

        private void OnBrowse(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Select the directory containing the client files",
                ShowNewFolderButton = false,
                SelectedPath = Directory.Exists(SelectedDir) ? SelectedDir : Files.RootDir
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                ApplyRoot(dialog.SelectedPath);
            }
        }

        private void OnDetectRegistry(object sender, EventArgs e)
        {
            // the old "Reload paths": client folder from the registry, all files from there
            Files.ReLoadDirectory();
            Files.LoadMulPath();
            Files.ResolveExtendedFilePaths();
            MapHelper.CheckForNewMapSize();
            AfterRootChanged(Files.RootDir);
        }

        private void OnOpenDir(object sender, EventArgs e) => OpenInExplorer(SelectedDir, false);

        private void OnSaveFavourite(object sender, EventArgs e)
        {
            string dir = SelectedDir;
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                MessageBox.Show(this, "The folder does not exist.", "Quick list", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UoDirectoryList.SaveFavourite(dir, _favNameBox.Text);
            FillDirBox(dir);
            _statusLabel.Text = $"Saved in the quick list: {dir}";
        }

        private void OnRemoveFavourite(object sender, EventArgs e)
        {
            string dir = SelectedDir;
            if (UoDirectoryList.Remove(dir))
            {
                FillDirBox(Files.RootDir);
                _statusLabel.Text = $"Removed from the quick list: {dir}";
            }
        }

        /// <summary>Switches the Ultima Online folder (files, extensions, quick list) and offers to reload everything.</summary>
        private void ApplyRoot(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show(this, $"The folder does not exist:{Environment.NewLine}{path}", "Ultima Online folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string oldRoot = string.IsNullOrEmpty(Files.RootDir) ? null : Path.GetFullPath(Files.RootDir).TrimEnd('\\', '/');
            foreach (string key in Files.MulPath.Keys.ToList())
            {
                string value = Files.MulPath[key];
                if (string.IsNullOrEmpty(value) || !Path.IsPathRooted(value))
                {
                    continue;
                }

                // absolute paths are kept by Files.SetMulPath - clear the ones that belong to the old folder
                // (or all of them) so the files of the new folder are taken
                string folder = Path.GetDirectoryName(Path.GetFullPath(value))?.TrimEnd('\\', '/');
                if (_resetCheck.Checked || (oldRoot != null && string.Equals(folder, oldRoot, StringComparison.OrdinalIgnoreCase)))
                {
                    Files.MulPath[key] = string.Empty;
                }
            }

            Files.SetMulPath(path);
            Files.ResolveExtendedFilePaths();
            MapHelper.CheckForNewMapSize();
            AfterRootChanged(path);
        }

        private void AfterRootChanged(string path)
        {
            UoDirectoryList.MarkUsed(path);
            FillDirBox(Files.RootDir);
            RefreshAll();

            if (MessageBox.Show(this,
                    $"Ultima Online folder:{Environment.NewLine}{Files.RootDir}{Environment.NewLine}{Environment.NewLine}Load all files (and the extension files) from there now?",
                    "Ultima Online folder", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ReloadAllFiles();
            }
            else
            {
                DetectExtended(false);
                _statusLabel.Text = "Paths changed - use 'Reload all files now' to load them.";
            }
        }

        private void UpdateClientInfo()
        {
            string root = Files.RootDir;
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                _clientInfo.Text = "No Ultima Online folder set.";
                _clientInfo.ForeColor = Color.DarkRed;
                return;
            }

            int found = 0, total = 0;
            foreach (string key in Files.MulPath.Keys)
            {
                ++total;
                if (Files.GetFilePath(key) != null)
                {
                    ++found;
                }
            }

            string art;
            string artIdx = Path.Combine(root, "artidx.mul");
            if (File.Exists(Path.Combine(root, "artLegacyMUL.uop")))
            {
                art = "art: artLegacyMUL.uop (UOP)";
            }
            else if (File.Exists(artIdx))
            {
                long entries = new FileInfo(artIdx).Length / 12;
                art = $"art: artidx.mul {entries:N0} entries ({(entries >= 0x13FDC ? "High Seas" : entries >= 0xC000 ? "Stygian Abyss" : "Mondain's Legacy or older")})";
            }
            else
            {
                art = "art: MISSING";
            }

            var maps = new List<string>();
            for (int i = 0; i <= 5; ++i)
            {
                if (File.Exists(Path.Combine(root, $"map{i}.mul")) || File.Exists(Path.Combine(root, $"map{i}LegacyMUL.uop")))
                {
                    maps.Add(i.ToString());
                }
            }

            string client = string.Empty;
            string exe = Path.Combine(root, "client.exe");
            if (File.Exists(exe))
            {
                try
                {
                    client = $"   client.exe {FileVersionInfo.GetVersionInfo(exe).FileVersion}";
                }
                catch (Exception)
                {
                    client = "   client.exe";
                }
            }

            int extFound = ExtendedFormat.AllFileNames.Count(n => Files.GetFilePath(n) != null);
            _clientInfo.ForeColor = found == 0 ? Color.DarkRed : Color.Black;
            _clientInfo.Text =
                $"{found:N0} of {total:N0} known files found   |   {art}   |   maps: {(maps.Count == 0 ? "none" : string.Join(" ", maps))}{client}" + Environment.NewLine +
                $"UOFiddler extension files: {extFound} of {ExtendedFormat.AllFileNames.Length}   |   output folder: {Options.OutputPath}";
        }

        #endregion

        #region [ Extension files ]

        private static UoExtensionKind KindOf(string name)
        {
            if (name == ExtendedFormat.LandIndexFileName || name == ExtendedFormat.LandDataFileName || name == ExtendedFormat.LandTileDataFileName || name == ExtendedFormat.RadarLandFileName)
            {
                return UoExtensionKind.LandTiles;
            }

            if (name == ExtendedFormat.TextureIndexFileName || name == ExtendedFormat.TextureDataFileName)
            {
                return UoExtensionKind.Textures;
            }

            return name == ExtendedFormat.HueFileName ? UoExtensionKind.Hues : UoExtensionKind.Items;
        }

        private static string LoadedPath(UoExtensionKind kind) => kind switch
        {
            UoExtensionKind.Items => Art.ExtendedDataPath,
            UoExtensionKind.LandTiles => Art.LandExtension.DataPath,
            UoExtensionKind.Textures => Textures.Extension.DataPath,
            _ => Hues.ExtensionPath
        };

        private static string FolderOf(string path) => string.IsNullOrEmpty(path) ? null : Path.GetDirectoryName(Path.GetFullPath(path))?.TrimEnd('\\', '/');

        private static bool SameFolder(string a, string b) => a != null && b != null && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private void FillExtList()
        {
            _extList.BeginUpdate();
            try
            {
                _extList.Items.Clear();
                int found = 0, loaded = 0;
                foreach (string name in ExtendedFormat.AllFileNames)
                {
                    UoExtensionKind kind = KindOf(name);
                    string stored = Files.GetFilePath(name);
                    string inRoot = string.IsNullOrEmpty(Files.RootDir) ? null : Path.Combine(Files.RootDir, name);
                    string inOutput = string.IsNullOrEmpty(Options.OutputPath) ? null : Path.Combine(Options.OutputPath, name);
                    string path = stored ?? (inRoot != null && File.Exists(inRoot) ? inRoot : null) ?? (inOutput != null && File.Exists(inOutput) ? inOutput : null);

                    string state;
                    Color color;
                    if (path == null)
                    {
                        state = "missing";
                        color = Color.DimGray;
                    }
                    else
                    {
                        ++found;
                        bool isLoaded = SameFolder(FolderOf(LoadedPath(kind)), FolderOf(path));
                        if (isLoaded)
                        {
                            ++loaded;
                            state = "loaded";
                            color = Color.DarkGreen;
                        }
                        else if (stored == null && path == inOutput)
                        {
                            state = "only in the output folder";
                            color = Color.DarkOrange;
                        }
                        else
                        {
                            state = "found, not loaded";
                            color = Color.DarkOrange;
                        }
                    }

                    string size = string.Empty, slots = string.Empty;
                    if (path != null)
                    {
                        var fi = new FileInfo(path);
                        size = $"{fi.Length:N0}";
                        bool data = name == ExtendedFormat.ArtDataFileName || name == ExtendedFormat.LandDataFileName || name == ExtendedFormat.TextureDataFileName;
                        if (data)
                        {
                            slots = "data";
                        }
                        else if (ExtendedFormat.TryReadHeader(path, out _, out _, out int count, out _, out string error))
                        {
                            slots = $"{count:N0}" + (error != null ? " (short)" : string.Empty);
                        }
                        else
                        {
                            slots = "UNREADABLE";
                            color = Color.DarkRed;
                        }
                    }

                    var item = new ListViewItem(name) { Tag = path, ForeColor = color };
                    item.SubItems.Add(UoExtensions.Title(kind));
                    item.SubItems.Add(state);
                    item.SubItems.Add(path == null ? string.Empty : FolderOf(path));
                    item.SubItems.Add(size);
                    item.SubItems.Add(slots);
                    _extList.Items.Add(item);
                }

                _extStatus.Text = found == 0 ? "No extension files - create them in the Extended tab." : $"{found} of {ExtendedFormat.AllFileNames.Length} found, {loaded} loaded";
                _extStatus.ForeColor = found == 0 ? Color.DimGray : loaded == found ? Color.DarkGreen : Color.DarkOrange;
            }
            finally
            {
                _extList.EndUpdate();
            }
        }

        /// <summary>
        /// Finds the extended files (stored path or the Ultima Online directory) and, if they are there and not
        /// loaded yet, loads them right away.
        /// </summary>
        private void DetectExtended(bool load)
        {
            List<string> found = Files.ResolveExtendedFilePaths();

            // loaded = every data file that is there is the one in memory (items, land tiles, textures, hues)
            bool Same(string loaded, string name)
            {
                string onDisk = Files.GetFilePath(name);
                return onDisk == null || (loaded != null && string.Equals(Path.GetFullPath(loaded), Path.GetFullPath(onDisk), StringComparison.OrdinalIgnoreCase));
            }

            bool alreadyLoaded = Same(Art.ExtendedDataPath, ExtendedFormat.ArtDataFileName) &&
                                 Same(Art.LandExtension.DataPath, ExtendedFormat.LandDataFileName) &&
                                 Same(Textures.Extension.DataPath, ExtendedFormat.TextureDataFileName) &&
                                 Same(Hues.ExtensionPath, ExtendedFormat.HueFileName);

            if (load && found.Count > 0 && !alreadyLoaded)
            {
                LoadExtended();
            }

            FillExtList();
            UpdateClientInfo();
        }

        private void LoadExtended()
        {
            if (Options.ChangedUltimaClass.Any(p => p.Value) &&
                MessageBox.Show(this, "Load the extended files? Unsaved changes in the extended ranges (items, land tiles, textures, hues, radar colours) are discarded.",
                    "Extended files", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                UoExtensions.ReloadAll();
                ControlEvents.FireFilePathChangeEvent(); // the tabs reload their lists
                UoExtensions.RaiseChanged(null);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void LoadExtendedFrom(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show(this, $"The folder does not exist:{Environment.NewLine}{folder}", "Extended files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int count = 0;
            foreach (string name in ExtendedFormat.AllFileNames)
            {
                string path = Path.Combine(folder, name);
                if (File.Exists(path))
                {
                    Files.SetMulPath(path, name);
                    ++count;
                }
            }

            if (count == 0)
            {
                MessageBox.Show(this, $"No extended files in{Environment.NewLine}{folder}", "Extended files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadExtended();
            DetectExtended(false);
            FillFileList();
            _statusLabel.Text = $"{count} extension file(s) taken from {folder}.";
        }

        private void OnExtSearch(object sender, EventArgs e)
        {
            DetectExtended(true);
            FillFileList();
        }

        private void OnExtFolder(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Folder with the extension files (artext*, artlandext*, tiledata*ext, texmapsext, huesext, radarcol*ext)",
                ShowNewFolderButton = false,
                SelectedPath = Directory.Exists(Options.OutputPath) ? Options.OutputPath : Files.RootDir
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                LoadExtendedFrom(dialog.SelectedPath);
            }
        }

        private void OnExtOutput(object sender, EventArgs e) => LoadExtendedFrom(Options.OutputPath);

        private void OnExtTab(object sender, EventArgs e)
        {
            Application.OpenForms.OfType<MainForm>().FirstOrDefault()?.SelectTabByName("ExtendedTab");
        }

        private void OnExtOpenFolder(object sender, EventArgs e)
        {
            if (_extList.SelectedItems.Count > 0 && _extList.SelectedItems[0].Tag is string path)
            {
                OpenInExplorer(path, true);
            }
        }

        #endregion

        #region [ All files ]

        private void OnFilterChanged(object sender, EventArgs e) => FillFileList();

        private void FillFileList()
        {
            string filter = _filterBox.Text.Trim();
            int mode = Math.Max(0, _showBox.SelectedIndex);
            string root = string.IsNullOrEmpty(Files.RootDir) ? null : Path.GetFullPath(Files.RootDir).TrimEnd('\\', '/');
            var ext = new HashSet<string>(ExtendedFormat.AllFileNames, StringComparer.OrdinalIgnoreCase);
            int shown = 0, found = 0;

            _fileList.BeginUpdate();
            try
            {
                _fileList.Items.Clear();
                foreach (string key in Files.MulPath.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
                {
                    string path = Files.GetFilePath(key);
                    bool exists = path != null;
                    if (exists)
                    {
                        ++found;
                    }

                    bool otherFolder = exists && root != null && !SameFolder(FolderOf(path), root);
                    bool show = mode switch
                    {
                        1 => exists,
                        2 => !exists,
                        3 => otherFolder,
                        4 => ext.Contains(key),
                        _ => true
                    };
                    if (!show || (filter.Length > 0 && key.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0 &&
                                  (path == null || path.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)))
                    {
                        continue;
                    }

                    var item = new ListViewItem(key) { Tag = key };
                    if (exists)
                    {
                        var fi = new FileInfo(path);
                        item.SubItems.Add(otherFolder ? "other folder" : "found");
                        item.SubItems.Add(path);
                        item.SubItems.Add($"{fi.Length:N0}");
                        item.SubItems.Add($"{fi.LastWriteTime:yyyy-MM-dd HH:mm}");
                        item.ForeColor = otherFolder ? Color.DarkOrange : SystemColors.WindowText;
                    }
                    else
                    {
                        item.SubItems.Add("missing");
                        item.SubItems.Add(Files.MulPath[key] ?? string.Empty);
                        item.SubItems.Add(string.Empty);
                        item.SubItems.Add(string.Empty);
                        item.ForeColor = Color.Gray;
                    }

                    _fileList.Items.Add(item);
                    ++shown;
                }
            }
            finally
            {
                _fileList.EndUpdate();
            }

            _countLabel.Text = $"{shown:N0} shown - {found:N0} of {Files.MulPath.Count:N0} files found";
        }

        private string SelectedKey => _fileList.SelectedItems.Count > 0 ? _fileList.SelectedItems[0].Tag as string : null;

        private void OnChooseFile(object sender, EventArgs e)
        {
            string key = SelectedKey;
            if (key == null)
            {
                return;
            }

            string current = Files.GetFilePath(key);
            using var dialog = new OpenFileDialog
            {
                Title = $"File for {key}",
                Filter = "Client files (*.mul;*.uop;*.idx;*.def;*.txt;*.enu;*.*)|*.*",
                FileName = key,
                InitialDirectory = current != null ? Path.GetDirectoryName(current) : (Directory.Exists(Files.RootDir) ? Files.RootDir : string.Empty)
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Files.SetMulPath(dialog.FileName, key);
            AfterSingleChange($"{key} -> {dialog.FileName}");
        }

        private void OnTakeFolder(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog { Description = "Take every known file that exists in this folder", ShowNewFolderButton = false };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            int count = 0;
            foreach (string key in Files.MulPath.Keys.ToList())
            {
                string path = Path.Combine(dialog.SelectedPath, key);
                if (File.Exists(path))
                {
                    Files.SetMulPath(path, key);
                    ++count;
                }
            }

            AfterSingleChange($"{count} file(s) taken from {dialog.SelectedPath}");
        }

        private void OnClearPath(object sender, EventArgs e)
        {
            string key = SelectedKey;
            if (key != null)
            {
                Files.SetMulPath(string.Empty, key);
                AfterSingleChange($"{key}: path cleared (the file is not used)");
            }
        }

        private void OnOpenFileFolder(object sender, EventArgs e)
        {
            string key = SelectedKey;
            string path = key == null ? null : Files.GetFilePath(key);
            OpenInExplorer(path ?? Files.RootDir, path != null);
        }

        private void OnCopyPath(object sender, EventArgs e)
        {
            string key = SelectedKey;
            string path = key == null ? null : Files.GetFilePath(key) ?? Files.MulPath[key];
            if (!string.IsNullOrEmpty(path))
            {
                Clipboard.SetText(path);
                _statusLabel.Text = $"Copied: {path}";
            }
        }

        private void AfterSingleChange(string text)
        {
            FillFileList();
            FillExtList();
            UpdateClientInfo();
            _statusLabel.Text = text + " - use 'Reload all files now' to load it.";
        }

        #endregion

        #region [ Bottom buttons ]

        private void OnReloadFiles(object sender, EventArgs e) => ReloadAllFiles();

        private void ReloadAllFiles()
        {
            MainForm main = Application.OpenForms.OfType<MainForm>().FirstOrDefault();
            if (main == null)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                main.ReloadAllFiles();
                UoExtensions.RaiseChanged(null);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            DetectExtended(false);
            FillFileList();
            _statusLabel.Text = $"All files loaded again from {Files.RootDir}.";
        }

        private void OnClose(object sender, EventArgs e) => Close();

        private void OpenInExplorer(string path, bool selectFile)
        {
            try
            {
                if (selectFile && File.Exists(path))
                {
                    Process.Start("explorer.exe", $"/select,\"{path}\"");
                }
                else if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Open folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnBackupFolder(object sender, EventArgs e)
        {
            string sourceDir = Files.RootDir;
            if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
            {
                MessageBox.Show(this, "No valid Ultima Online folder is set.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var dialog = new FolderBrowserDialog { Description = "Select the destination directory for the backup", ShowNewFolderButton = true };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (MessageBox.Show(this, $"Copy all files and folders of{Environment.NewLine}{sourceDir}{Environment.NewLine}to{Environment.NewLine}{dialog.SelectedPath} ?",
                    "Backup", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                CopyDirectory(sourceDir, dialog.SelectedPath);
                _statusLabel.Text = $"Backup finished: {dialog.SelectedPath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"An error occurred during the backup: {ex.Message}", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            var dir = new DirectoryInfo(sourceDir);
            Directory.CreateDirectory(destDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                file.CopyTo(Path.Combine(destDir, file.Name), false);
            }

            foreach (DirectoryInfo sub in dir.GetDirectories())
            {
                CopyDirectory(sub.FullName, Path.Combine(destDir, sub.Name));
            }
        }

        #endregion
    }

    #region DictionaryPropertyGridAdapter
    internal class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        private readonly IDictionary _dictionary;

        public DictionaryPropertyGridAdapter(IDictionary d)
        {
            _dictionary = d;
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection
            ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(Array.Empty<Attribute>());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
            foreach (DictionaryEntry e in _dictionary)
            {
                properties.Add(new DictionaryPropertyDescriptor(_dictionary, e.Key));
            }

            PropertyDescriptor[] props = properties.ToArray();

            return new PropertyDescriptorCollection(props);
        }
    }
    #endregion

    #region DictionaryPropertyDescriptor
    internal class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        private readonly IDictionary _dictionary;
        private readonly object _key;

        internal DictionaryPropertyDescriptor(IDictionary d, object key)
            : base(key.ToString(), null)
        {
            _dictionary = d;
            _key = key;
        }

        public override Type PropertyType => typeof(string);

        public override void SetValue(object component, object value)
        {
            _dictionary[_key] = value;
        }

        public override object GetValue(object component)
        {
            return _dictionary[_key];
        }

        public override bool IsReadOnly => false;

        public override Type ComponentType => null;

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
    #endregion
}