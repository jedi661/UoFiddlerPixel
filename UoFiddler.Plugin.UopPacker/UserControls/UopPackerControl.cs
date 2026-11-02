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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.UopPacker.Classes;

namespace UoFiddler.Plugin.UopPacker.UserControls
{
    #region [ class UopPackerControl ]
    /// <summary>
    /// UOP packer tab: MUL ↔ UOP for one file, every file of a folder, multis (with housing.bin), UOP info,
    /// automatic backup and help (English / German).
    /// </summary>
    public partial class UopPackerControl : UserControl
    {
        /// <summary>Item of the "Every file" list: the file set and its status after "Check folder".</summary>
        private sealed class SetItem
        {
            public UopFileSet Set;
            public string Status = string.Empty;
            public override string ToString() => Status.Length > 0 ? $"{Set.Title}  –  {Status}" : Set.Title;
        }

        private readonly PackerSettings _settings;
        private readonly PackerBackup _backup;
        private CancellationTokenSource _cancel;
        private UopInfo _info;
        private bool _loading;
        private List<BackupEntry> _backupEntries = new List<BackupEntry>();

        #region [ UopPackerControl ]
        private UopPackerControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;

            _settings = PackerSettings.Load();
            _backup = new PackerBackup(_settings);
            _backup.Logged += Log;

            _loading = true;

            // MUL -> UOP: multis have their own tab (they need housing.bin)
            multype.DataSource = new[] { FileType.ArtLegacyMul, FileType.GumpartLegacyMul, FileType.MapLegacyMul, FileType.SoundLegacyMul };
            uoptype.DataSource = Enum.GetValues(typeof(FileType));
            mulMapIndex.ReadOnly = uopMapIndex.ReadOnly = true;
            mulMapIndex.Maximum = uopMapIndex.Maximum = 9;

            int compression = compressionBox.Items.IndexOf(_settings.Compression);
            compressionBox.SelectedIndex = compression >= 0 ? compression : 0;
            comboBoxMultiCompression.SelectedIndex = 0;

            checkBoxOverwriteSaveUop.Checked = _settings.OverwriteUop;
            checkBoxOverwriteSaveUop.CheckedChanged += (s, e) => SaveSettings();
            checkBoxOverwriteMul.Checked = _settings.OverwriteMul;
            CheckBoxAutoFill.Checked = _settings.AutoFill;
            compressionBox.SelectedIndexChanged += (s, e) => SaveSettings();
            multype.SelectedIndexChanged += (s, e) => { if (!_loading && CheckBoxAutoFill.Checked) { AutoFillTextBoxes(); } };

            inputfolder.Text = _settings.LastFolder ?? string.Empty;
            extract.Checked = true;
            foreach (UopFileSet set in UopFileTypes.AllSets())
            {
                checkedListBoxSets.Items.Add(new SetItem { Set = set }, true);
            }

            checkBoxBackupEnabled.Checked = _settings.BackupEnabled;
            radioButtonBackupNextTo.Checked = _settings.BackupLocation == BackupLocation.NextToFile;
            radioButtonBackupFixed.Checked = _settings.BackupLocation == BackupLocation.FixedFolder;
            textBoxBackupFolder.Text = _settings.BackupFolder ?? string.Empty;
            textBoxBackupFolder.Leave += (s, e) => { _settings.BackupFolder = textBoxBackupFolder.Text.Trim(); SaveSettings(); RefreshBackupList(); };
            checkBoxBackupCopyNew.Checked = _settings.BackupCopyNew;

            radioButtonHelpGerman.Checked = _settings.HelpLanguage == "de";
            radioButtonHelpEnglish.Checked = _settings.HelpLanguage != "de";
            ShowHelp();

            _loading = false;
            RefreshBackupList();
            Log("Ready. Backup is " + (_settings.BackupEnabled ? "on" : "off") + ".");
        }
        #endregion

        #region UopPackerControl
        public UopPackerControl(string version) : this()
        {
            VersionLabel.Text = version;
        }
        #endregion

        // ==================================================================== helpers

        private void SaveSettings()
        {
            if (_loading)
            {
                return;
            }
            _settings.OverwriteUop = checkBoxOverwriteSaveUop.Checked;
            _settings.OverwriteMul = checkBoxOverwriteMul.Checked;
            _settings.AutoFill = CheckBoxAutoFill.Checked;
            _settings.Compression = compressionBox.SelectedItem?.ToString() ?? "None";
            _settings.LastFolder = inputfolder.Text;
            _settings.BackupEnabled = checkBoxBackupEnabled.Checked;
            _settings.BackupLocation = radioButtonBackupFixed.Checked ? BackupLocation.FixedFolder : BackupLocation.NextToFile;
            _settings.BackupFolder = textBoxBackupFolder.Text.Trim();
            _settings.BackupCopyNew = checkBoxBackupCopyNew.Checked;
            _settings.HelpLanguage = radioButtonHelpGerman.Checked ? "de" : "en";
            _settings.Save();
        }

        /// <summary>Adds a line to the log of the "Every file" tab (thread safe).</summary>
        private void Log(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(Log), text);
                return;
            }
            textBoxLog.AppendText($"{DateTime.Now:HH:mm:ss}  {text}{Environment.NewLine}");
        }

        private void Status(string text)
        {
            toolStripStatusLabel1.Text = text;
            statustext.Text = text;
        }

        private static CompressionFlag ParseCompression(string text) =>
            Enum.TryParse(text, true, out CompressionFlag flag) ? flag : CompressionFlag.None;

        private static string Ask(string title, string message, MessageBoxIcon icon = MessageBoxIcon.Warning)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
            return null;
        }

        /// <summary>Shows the result of a conversion (warnings in a message box, everything in the log).</summary>
        private void Report(string what, ConversionResult result)
        {
            Log($"{what}: {result}");
            foreach (string w in result.Warnings)
            {
                Log("  " + w);
            }
            Status($"{what}: {result}");
            if (result.Warnings.Count > 0)
            {
                MessageBox.Show($"{what}: {result}\r\n\r\n" + string.Join("\r\n", result.Warnings.Take(15)), "UOP Packer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SetBusy(bool busy)
        {
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            foreach (Button b in new[] { multouop, uoptomul, StartFolderButton, buttonMultiPack, buttonMultiUnpack, SingleFileExtractButton, SingleFileUopArtExtractButton, buttonInfoAnalyze })
            {
                b.Enabled = !busy;
            }
            buttonCancelFolder.Enabled = busy;
        }

        private Progress<int> MakeProgress() => new Progress<int>(p => progressBarFolder.Value = Math.Max(0, Math.Min(100, p)));

        private string BrowseFile(string filter, string fileName, bool save)
        {
            using (FileDialog dialog = save ? new SaveFileDialog() : new OpenFileDialog())
            {
                dialog.Filter = filter;
                dialog.FileName = fileName ?? string.Empty;
                dialog.CheckFileExists = !save;
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
            }
        }

        private const string MulFilter = "MUL files (*.mul)|*.mul|All files (*.*)|*.*";
        private const string UopFilter = "UOP files (*.uop)|*.uop|All files (*.*)|*.*";
        private const string IdxFilter = "IDX files (*.idx;*.mul)|*.idx;*.mul|All files (*.*)|*.*";
        private const string BinFilter = "housing.bin|housing.bin;*.bin|All files (*.*)|*.*";

        // ==================================================================== tab "One file"

        #region InputMulSelect
        private void InputMulSelect(object sender, EventArgs e)
        {
            string file = BrowseFile(MulFilter, Path.GetFileName(inmul.Text), false);
            if (file == null)
            {
                return;
            }
            inmul.Text = file;
            if (CheckBoxAutoFill.Checked)
            {
                AutoFillTextBoxes();
            }
        }
        #endregion

        #region InputIdxSelect
        private void InputIdxSelect(object sender, EventArgs e)
        {
            string file = BrowseFile(IdxFilter, Path.GetFileName(inidx.Text), false);
            if (file != null)
            {
                inidx.Text = file;
            }
        }
        #endregion

        #region OutputUopSelect
        private void OutputUopSelect(object sender, EventArgs e)
        {
            string file = BrowseFile(UopFilter, string.IsNullOrEmpty(outuop.Text) ? "artLegacyMUL.uop" : Path.GetFileName(outuop.Text), true);
            if (file != null)
            {
                outuop.Text = file;
            }
        }
        #endregion

        #region ToUop
        private async void ToUop(object sender, EventArgs e)
        {
            if (multype.SelectedItem is not FileType fileType)
            {
                Ask("MUL to UOP", "You must specify the input type.");
                return;
            }
            bool map = fileType == FileType.MapLegacyMul;
            if (string.IsNullOrWhiteSpace(inmul.Text) || !File.Exists(inmul.Text))
            {
                Ask("MUL to UOP", "The input MUL does not exist.");
                return;
            }
            if (!map && (string.IsNullOrWhiteSpace(inidx.Text) || !File.Exists(inidx.Text)))
            {
                Ask("MUL to UOP", "The input IDX does not exist (maps don't need one).");
                return;
            }
            if (string.IsNullOrWhiteSpace(outuop.Text))
            {
                Ask("MUL to UOP", "You must specify the output UOP.");
                return;
            }
            if (File.Exists(outuop.Text) && !checkBoxOverwriteSaveUop.Checked)
            {
                Ask("MUL to UOP", "The output file already exists. Enable \"Overwrite .uop\" to replace it.");
                return;
            }

            CompressionFlag compression = ParseCompression(compressionBox.SelectedItem?.ToString());
            if (compression == CompressionFlag.Mythic && fileType != FileType.GumpartLegacyMul)
            {
                Ask("MUL to UOP", "Mythic compression is only used for gumps. Choose None or Zlib.");
                return;
            }

            string mul = inmul.Text, idx = map ? null : inidx.Text, uop = outuop.Text;
            int index = (int)mulMapIndex.Value;
            SetBusy(true);
            multouop.Text = "Converting...";
            try
            {
                ConversionResult result = await Task.Run(() => LegacyMulFileConverter.ToUop(mul, idx, uop, fileType, index, compression, null, _backup, MakeProgress()));
                Report($"Packed {Path.GetFileName(uop)}", result);
            }
            catch (Exception ex)
            {
                Log($"Error packing {Path.GetFileName(uop)}: {ex.Message}");
                Ask("MUL to UOP", $"An error occurred: {ex.Message}", MessageBoxIcon.Error);
            }
            finally
            {
                multouop.Text = "Convert";
                SetBusy(false);
            }
        }
        #endregion

        #region OutMulSelect
        private void OutMulSelect(object sender, EventArgs e)
        {
            string file = BrowseFile(MulFilter, Path.GetFileName(outmul.Text), true);
            if (file != null)
            {
                outmul.Text = file;
            }
        }
        #endregion

        #region  OutputIdxSelect
        private void OutputIdxSelect(object sender, EventArgs e)
        {
            string file = BrowseFile(IdxFilter, Path.GetFileName(outidx.Text), true);
            if (file != null)
            {
                outidx.Text = file;
            }
        }
        #endregion

        #region InputUopSelect
        private void InputUopSelect(object sender, EventArgs e)
        {
            string file = BrowseFile(UopFilter, Path.GetFileName(inuop.Text), false);
            if (file == null)
            {
                return;
            }
            inuop.Text = file;
            if (CheckBoxAutoFill.Checked)
            {
                AutoFillUopSide();
            }
        }
        #endregion

        #region ToMul
        private async void ToMul(object sender, EventArgs e)
        {
            if (uoptype.SelectedItem is not FileType fileType)
            {
                Ask("UOP to MUL", "You must specify the input type.");
                return;
            }
            bool map = fileType == FileType.MapLegacyMul;
            if (string.IsNullOrWhiteSpace(inuop.Text) || !File.Exists(inuop.Text))
            {
                Ask("UOP to MUL", "The input UOP does not exist.");
                return;
            }
            if (string.IsNullOrWhiteSpace(outmul.Text))
            {
                Ask("UOP to MUL", "You must specify the output MUL.");
                return;
            }
            if (!map && string.IsNullOrWhiteSpace(outidx.Text))
            {
                Ask("UOP to MUL", "You must specify the output IDX.");
                return;
            }
            bool exists = File.Exists(outmul.Text) || (!map && File.Exists(outidx.Text));
            if (exists && !checkBoxOverwriteMul.Checked)
            {
                Ask("UOP to MUL", "The output MUL / IDX already exists. Enable \"Overwrite .mul\" to replace it.");
                return;
            }

            string uop = inuop.Text, mul = outmul.Text, idx = map ? null : outidx.Text;
            string housing = fileType == FileType.MultiCollection ? Path.Combine(Path.GetDirectoryName(mul) ?? string.Empty, "housing.bin") : null;
            int index = (int)uopMapIndex.Value;
            SetBusy(true);
            uoptomul.Text = "Converting...";
            try
            {
                ConversionResult result = await Task.Run(() => LegacyMulFileConverter.FromUop(uop, mul, idx, fileType, index, housing, _backup, MakeProgress()));
                Report($"Unpacked {Path.GetFileName(uop)}", result);
            }
            catch (Exception ex)
            {
                Log($"Error unpacking {Path.GetFileName(uop)}: {ex.Message}");
                Ask("UOP to MUL", $"An error occurred: {ex.Message}", MessageBoxIcon.Error);
            }
            finally
            {
                uoptomul.Text = "Convert";
                SetBusy(false);
            }
        }
        #endregion

        #region [ CheckBoxAutoFill_CheckedChanged ]
        private void CheckBoxAutoFill_CheckedChanged(object sender, EventArgs e)
        {
            SaveSettings();
            if (CheckBoxAutoFill.Checked)
            {
                AutoFillTextBoxes();
                AutoFillUopSide();
            }
        }
        #endregion

        #region [ AutoFillTextBoxes ]
        /// <summary>MUL → UOP: type, map number, IDX and output name from the name of the input MUL.</summary>
        private void AutoFillTextBoxes()
        {
            if (string.IsNullOrEmpty(inmul.Text))
            {
                return;
            }

            string directory = Path.GetDirectoryName(inmul.Text) ?? string.Empty;
            UopFileSet set = UopFileTypes.Detect(inmul.Text);
            if (set == null || set.Type == FileType.MultiCollection)
            {
                // unknown name (or multis: own tab): use the chosen type
                if (multype.SelectedItem is FileType chosen)
                {
                    set = UopFileTypes.AllSets().Find(s => s.Type == chosen && (chosen != FileType.MapLegacyMul || s.TypeIndex == (int)mulMapIndex.Value));
                }
                if (set == null)
                {
                    return;
                }
            }

            _loading = true;
            multype.SelectedItem = set.Type;
            mulMapIndex.Value = set.TypeIndex;
            _loading = false;
            inidx.Text = set.Idx != null ? Path.Combine(directory, set.Idx) : string.Empty;
            outuop.Text = Path.Combine(directory, set.Uop);
        }

        /// <summary>UOP → MUL: type, map number and the standard output names from the name of the input UOP.</summary>
        private void AutoFillUopSide()
        {
            if (string.IsNullOrEmpty(inuop.Text))
            {
                return;
            }
            UopFileSet set = UopFileTypes.Detect(inuop.Text);
            if (set == null)
            {
                return;
            }
            string directory = Path.GetDirectoryName(inuop.Text) ?? string.Empty;
            uoptype.SelectedItem = set.Type;
            uopMapIndex.Value = set.TypeIndex;
            outmul.Text = Path.Combine(directory, set.Mul);
            outidx.Text = set.Idx != null ? Path.Combine(directory, set.Idx) : string.Empty;
        }
        #endregion

        private void OnOverwriteMulChanged(object sender, EventArgs e) => SaveSettings();

        // ==================================================================== tab "Every file"

        #region SelectFolder
        private void SelectFolder_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(inputfolder.Text))
            {
                FolderDialog.SelectedPath = inputfolder.Text;
            }
            if (FolderDialog.ShowDialog() == DialogResult.OK)
            {
                inputfolder.Text = FolderDialog.SelectedPath;
                SaveSettings();
                CheckFolder();
                RefreshBackupList();
            }
        }
        #endregion

        private void OnOverwriteBatchChanged(object sender, EventArgs e) => CheckFolder();

        private void OnClickCheckFolder(object sender, EventArgs e) => CheckFolder();

        private void OnClickCancelFolder(object sender, EventArgs e)
        {
            _cancel?.Cancel();
            Log("Cancel requested – stopping after the current file.");
        }

        private string InFolder(string name) => name == null ? null : Path.Combine(inputfolder.Text, name);

        /// <summary>Status of every file set for the chosen direction: source found / missing, output would be replaced.</summary>
        private void CheckFolder()
        {
            if (!Directory.Exists(inputfolder.Text))
            {
                return;
            }
            bool packing = pack.Checked;
            for (int i = 0; i < checkedListBoxSets.Items.Count; i++)
            {
                var item = (SetItem)checkedListBoxSets.Items[i];
                UopFileSet set = item.Set;
                bool source = packing
                    ? File.Exists(InFolder(set.Mul)) && (set.Idx == null || File.Exists(InFolder(set.Idx)))
                    : File.Exists(InFolder(set.Uop));
                bool output = packing ? File.Exists(InFolder(set.Uop)) : File.Exists(InFolder(set.Mul));
                item.Status = !source ? "source missing"
                    : output ? (checkBoxOverwriteBatch.Checked ? "ready (replaces the output)" : "output exists – skipped")
                    : "ready";
                checkedListBoxSets.Items[i] = item;
            }
            int ready = checkedListBoxSets.Items.Cast<SetItem>().Count(s => s.Status.StartsWith("ready", StringComparison.Ordinal));
            statustext.Text = $"{ready} of {checkedListBoxSets.Items.Count} files ready to {(packing ? "pack" : "extract")}";
        }

        #region StartFolderButtonClick
        private async void StartFolderButtonClick(object sender, EventArgs e)
        {
            if (inputfolder.Text.Length == 0 || !Directory.Exists(inputfolder.Text))
            {
                Ask("Every file", "You must specify an existing input folder.");
                return;
            }
            if (!extract.Checked && !pack.Checked)
            {
                Ask("Every file", "You must select an option.");
                return;
            }
            var sets = checkedListBoxSets.CheckedItems.Cast<SetItem>().Select(i => i.Set).ToList();
            if (sets.Count == 0)
            {
                Ask("Every file", "Tick the files that should be converted.");
                return;
            }
            SaveSettings();
            await RunSets(sets, pack.Checked);
        }
        #endregion

        /// <summary>Converts the file sets one after the other (in the background, with log and progress).</summary>
        private async Task RunSets(List<UopFileSet> sets, bool packing)
        {
            bool overwrite = checkBoxOverwriteBatch.Checked;
            CompressionFlag compression = ParseCompression(compressionBox.SelectedItem?.ToString());
            string folder = inputfolder.Text;
            int total = 0, success = 0, skipped = 0;

            _cancel = new CancellationTokenSource();
            CancellationToken token = _cancel.Token;
            SetBusy(true);
            Log($"{(packing ? "Pack" : "Extract")} {sets.Count} file sets in {folder}");
            try
            {
                foreach (UopFileSet set in sets)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    string uop = Path.Combine(folder, set.Uop);
                    string mul = Path.Combine(folder, set.Mul);
                    string idx = set.Idx != null ? Path.Combine(folder, set.Idx) : null;
                    string extra = set.Extra != null ? Path.Combine(folder, set.Extra) : null;

                    bool sourceOk = packing ? File.Exists(mul) && (idx == null || File.Exists(idx)) : File.Exists(uop);
                    if (!sourceOk)
                    {
                        continue; // not in this folder – not an error
                    }
                    total++;
                    bool outputExists = packing ? File.Exists(uop) : File.Exists(mul) || (idx != null && File.Exists(idx));
                    if (outputExists && !overwrite)
                    {
                        skipped++;
                        Log($"{set.Title}: output exists – skipped (tick \"Overwrite outputs\").");
                        continue;
                    }

                    statustext.Text = $"{set.Title}...";
                    try
                    {
                        // Mythic only for gumps; the other files get Zlib or None
                        CompressionFlag flag = compression == CompressionFlag.Mythic && set.Type != FileType.GumpartLegacyMul ? CompressionFlag.None : compression;
                        ConversionResult result = packing
                            ? await Task.Run(() => LegacyMulFileConverter.ToUop(mul, idx, uop, set.Type, set.TypeIndex, flag, extra, _backup, MakeProgress(), token))
                            : await Task.Run(() => LegacyMulFileConverter.FromUop(uop, mul, idx, set.Type, set.TypeIndex, extra, _backup, MakeProgress(), token));
                        success++;
                        Log($"{set.Title}: {result}");
                        foreach (string w in result.Warnings)
                        {
                            Log("  " + w);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Log($"{set.Title}: cancelled.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log($"{set.Title}: ERROR {ex.Message}");
                    }
                }
            }
            finally
            {
                SetBusy(false);
                progressBarFolder.Value = 0;
            }
            string done = $"Done ({success}/{total} files {(packing ? "packed" : "extracted")}" + (skipped > 0 ? $", {skipped} skipped" : string.Empty) + ")";
            statustext.Text = done;
            Log(done);
            CheckFolder();
            RefreshBackupList();
        }

        #region SingleFileUopArtExtractButton
        private async void SingleFileUopArtExtractButtonClick(object sender, EventArgs e)
        {
            if (inputfolder.Text.Length == 0)
            {
                Ask("Single Uop Art Extract", "You must specify the input folder.");
                return;
            }
            await RunSets(new List<UopFileSet> { UopFileTypes.AllSets()[0] }, false);
        }
        #endregion

        #region SingleFileExtractButton
        private async void SingleFileExtractButtonClick(object sender, EventArgs e)
        {
            string inFile = BrowseFile("UOP files (*.uop)|*.uop", string.Empty, false);
            if (inFile == null)
            {
                return;
            }

            UopFileSet set = UopFileTypes.Detect(inFile);
            if (set == null)
            {
                Ask("Single Uop Extract", "Unknown file type (the name must be like artLegacyMUL.uop, map1xLegacyMUL.uop, MultiCollection.uop, ...).");
                return;
            }

            string dir = Path.GetDirectoryName(inFile) ?? string.Empty;
            string mul = Path.Combine(dir, set.Mul);
            string idx = set.Idx != null ? Path.Combine(dir, set.Idx) : null;
            string extra = set.Extra != null ? Path.Combine(dir, set.Extra) : null;
            if ((File.Exists(mul) || (idx != null && File.Exists(idx))) &&
                MessageBox.Show($"{set.Mul} already exists in the folder. Replace it?" + (_settings.BackupEnabled ? "\r\n(The old file goes into the backup folder.)" : string.Empty),
                    "Single Uop Extract", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            SetBusy(true);
            try
            {
                ConversionResult result = await Task.Run(() => LegacyMulFileConverter.FromUop(inFile, mul, idx, set.Type, set.TypeIndex, extra, _backup, MakeProgress()));
                Report($"Unpacked {Path.GetFileName(inFile)}", result);
            }
            catch (Exception ex)
            {
                Log($"Error unpacking {Path.GetFileName(inFile)}: {ex.Message}");
                Ask("Single Uop Extract", $"An error occurred.\r\n{ex.Message}", MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
                progressBarFolder.Value = 0;
            }
        }
        #endregion

        // ==================================================================== tab "Multis"

        /// <summary>Fills the empty file boxes of the multis tab from the folder of a chosen file.</summary>
        private void FillMultiFolder(string chosen)
        {
            string dir = Path.GetDirectoryName(chosen) ?? string.Empty;
            void Fill(TextBox box, string name)
            {
                if (string.IsNullOrWhiteSpace(box.Text))
                {
                    box.Text = Path.Combine(dir, name);
                }
            }
            Fill(textBoxMultiMul, "multi.mul");
            Fill(textBoxMultiIdx, "multi.idx");
            Fill(textBoxMultiHousing, "housing.bin");
            Fill(textBoxMultiOutUop, "MultiCollection.uop");
            Fill(textBoxMultiInUop, "MultiCollection.uop");
            Fill(textBoxMultiOutMul, "multi.mul");
            Fill(textBoxMultiOutIdx, "multi.idx");
            Fill(textBoxMultiOutHousing, "housing.bin");
        }

        private void Browse(TextBox box, string filter, string name, bool save)
        {
            string file = BrowseFile(filter, string.IsNullOrEmpty(box.Text) ? name : Path.GetFileName(box.Text), save);
            if (file != null)
            {
                box.Text = file;
                FillMultiFolder(file);
            }
        }

        private void OnClickBrowseMultiMul(object sender, EventArgs e) => Browse(textBoxMultiMul, MulFilter, "multi.mul", false);

        private void OnClickBrowseMultiIdx(object sender, EventArgs e) => Browse(textBoxMultiIdx, IdxFilter, "multi.idx", false);

        private void OnClickBrowseMultiHousing(object sender, EventArgs e) => Browse(textBoxMultiHousing, BinFilter, "housing.bin", false);

        private void OnClickBrowseMultiOutUop(object sender, EventArgs e) => Browse(textBoxMultiOutUop, UopFilter, "MultiCollection.uop", true);

        private void OnClickBrowseMultiInUop(object sender, EventArgs e) => Browse(textBoxMultiInUop, UopFilter, "MultiCollection.uop", false);

        private void OnClickBrowseMultiOutMul(object sender, EventArgs e) => Browse(textBoxMultiOutMul, MulFilter, "multi.mul", true);

        private void OnClickBrowseMultiOutIdx(object sender, EventArgs e) => Browse(textBoxMultiOutIdx, IdxFilter, "multi.idx", true);

        private void OnClickBrowseMultiOutHousing(object sender, EventArgs e) => Browse(textBoxMultiOutHousing, BinFilter, "housing.bin", true);

        private async void OnClickMultiPack(object sender, EventArgs e)
        {
            string mul = textBoxMultiMul.Text, idx = textBoxMultiIdx.Text, housing = textBoxMultiHousing.Text, uop = textBoxMultiOutUop.Text;
            if (!File.Exists(mul) || !File.Exists(idx))
            {
                Ask("Pack multis", "multi.mul and multi.idx must exist.");
                return;
            }
            if (string.IsNullOrWhiteSpace(uop))
            {
                Ask("Pack multis", "You must specify the output UOP.");
                return;
            }
            if (!File.Exists(housing) &&
                MessageBox.Show("housing.bin was not found. MultiCollection.uop without it will probably not work in the client.\r\n\r\nPack anyway?",
                    "Pack multis", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            if (File.Exists(uop) && !checkBoxOverwriteSaveUop.Checked)
            {
                Ask("Pack multis", "The output file already exists. Enable \"Overwrite .uop\" (tab \"One file\") to replace it.");
                return;
            }

            CompressionFlag compression = ParseCompression(comboBoxMultiCompression.SelectedItem?.ToString());
            SetBusy(true);
            try
            {
                ConversionResult result = await Task.Run(() => LegacyMulFileConverter.ToUop(mul, idx, uop, FileType.MultiCollection, 0, compression, housing, _backup, MakeProgress()));
                Report($"Packed {Path.GetFileName(uop)}", result);
            }
            catch (Exception ex)
            {
                Log($"Error packing multis: {ex.Message}");
                Ask("Pack multis", $"An error occurred: {ex.Message}", MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
                progressBarFolder.Value = 0;
            }
        }

        private async void OnClickMultiUnpack(object sender, EventArgs e)
        {
            string uop = textBoxMultiInUop.Text, mul = textBoxMultiOutMul.Text, idx = textBoxMultiOutIdx.Text, housing = textBoxMultiOutHousing.Text;
            if (!File.Exists(uop))
            {
                Ask("Unpack multis", "The input UOP does not exist.");
                return;
            }
            if (string.IsNullOrWhiteSpace(mul) || string.IsNullOrWhiteSpace(idx))
            {
                Ask("Unpack multis", "You must specify the output MUL and IDX.");
                return;
            }
            if ((File.Exists(mul) || File.Exists(idx)) && !checkBoxOverwriteMul.Checked)
            {
                Ask("Unpack multis", "The output MUL / IDX already exists. Enable \"Overwrite .mul\" (tab \"One file\") to replace it.");
                return;
            }

            SetBusy(true);
            try
            {
                ConversionResult result = await Task.Run(() => LegacyMulFileConverter.FromUop(uop, mul, idx, FileType.MultiCollection, 0,
                    string.IsNullOrWhiteSpace(housing) ? null : housing, _backup, MakeProgress()));
                Report($"Unpacked {Path.GetFileName(uop)}", result);
            }
            catch (Exception ex)
            {
                Log($"Error unpacking multis: {ex.Message}");
                Ask("Unpack multis", $"An error occurred: {ex.Message}", MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
                progressBarFolder.Value = 0;
            }
        }

        // ==================================================================== tab "UOP Info"

        private void OnClickInfoBrowse(object sender, EventArgs e)
        {
            string file = BrowseFile(UopFilter, Path.GetFileName(textBoxInfoFile.Text), false);
            if (file != null)
            {
                textBoxInfoFile.Text = file;
                OnClickInfoAnalyze(sender, e);
            }
        }

        private async void OnClickInfoAnalyze(object sender, EventArgs e)
        {
            string path = textBoxInfoFile.Text;
            if (!File.Exists(path))
            {
                Ask("UOP Info", "Choose a UOP file.");
                return;
            }
            bool verify = checkBoxInfoVerify.Checked;
            SetBusy(true);
            try
            {
                _info = await Task.Run(() => UopInspector.Read(path, verify, MakeProgress()));
                textBoxInfoSummary.Text = _info.Summary().Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
                listViewInfo.VirtualListSize = _info.Entries.Count;
                listViewInfo.Invalidate();
                Status($"{Path.GetFileName(path)}: {_info.Entries.Count} entries, {_info.Problems.Count} problems");
            }
            catch (Exception ex)
            {
                Ask("UOP Info", $"The file can't be read: {ex.Message}", MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
                progressBarFolder.Value = 0;
            }
        }

        private void OnRetrieveInfoItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (_info == null || e.ItemIndex < 0 || e.ItemIndex >= _info.Entries.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }
            UopInfoEntry entry = _info.Entries[e.ItemIndex];
            var item = new ListViewItem(entry.Index.ToString());
            item.SubItems.Add(entry.Name ?? $"{entry.Hash:X16}");
            item.SubItems.Add($"0x{entry.Offset:X}");
            item.SubItems.Add(entry.Size.ToString("N0"));
            item.SubItems.Add(entry.DecompressedSize.ToString("N0"));
            item.SubItems.Add(entry.Compression switch { 0 => "none", 1 => "zlib", 3 => "Mythic", _ => entry.Compression.ToString() });
            item.SubItems.Add(entry.ChecksumOk == null ? $"{entry.Checksum:X8}" : entry.ChecksumOk == true ? "ok" : "WRONG");
            if (entry.ChecksumOk == false)
            {
                item.ForeColor = Color.Firebrick;
            }
            else if (entry.Name == null)
            {
                item.ForeColor = Color.Gray;
            }
            e.Item = item;
        }

        // ==================================================================== tab "Backup"

        private void OnBackupSettingChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }
            SaveSettings();
            RefreshBackupList();
        }

        private void OnClickBackupFolder(object sender, EventArgs e)
        {
            if (Directory.Exists(textBoxBackupFolder.Text))
            {
                FolderDialog.SelectedPath = textBoxBackupFolder.Text;
            }
            if (FolderDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxBackupFolder.Text = FolderDialog.SelectedPath;
                radioButtonBackupFixed.Checked = true;
                SaveSettings();
                RefreshBackupList();
            }
        }

        /// <summary>Backup folder that is shown: the fixed folder, or the one next to the chosen client folder.</summary>
        private string ShownBackupFolder()
        {
            string reference = !string.IsNullOrWhiteSpace(inputfolder.Text) ? inputfolder.Text
                : !string.IsNullOrWhiteSpace(outuop.Text) ? Path.GetDirectoryName(outuop.Text)
                : AppDomain.CurrentDomain.BaseDirectory;
            return _backup.FolderFor(Path.Combine(reference ?? string.Empty, "file"));
        }

        private void RefreshBackupList()
        {
            string folder = ShownBackupFolder();
            labelBackupInfo.Text = (_settings.BackupEnabled ? "Backup is ON. " : "Backup is OFF. ") +
                                   $"Backup folder: {folder}\r\nold = files before they were replaced" + (_settings.BackupCopyNew ? ", new = the new files" : string.Empty);
            _backupEntries = PackerBackup.List(folder);
            listViewBackup.BeginUpdate();
            listViewBackup.Items.Clear();
            foreach (BackupEntry entry in _backupEntries)
            {
                var item = new ListViewItem(entry.FileName) { Tag = entry };
                item.SubItems.Add(entry.OriginalPath ?? "?");
                item.SubItems.Add(entry.Time == default ? string.Empty : entry.Time.ToString("yyyy-MM-dd HH:mm"));
                item.SubItems.Add((entry.OldCopy != null ? "old" : string.Empty) + (entry.OldCopy != null && entry.NewCopy != null ? " + " : string.Empty) + (entry.NewCopy != null ? "new" : string.Empty));
                listViewBackup.Items.Add(item);
            }
            listViewBackup.EndUpdate();
        }

        private void OnClickBackupRefresh(object sender, EventArgs e) => RefreshBackupList();

        private void OnClickBackupOpen(object sender, EventArgs e)
        {
            string folder = ShownBackupFolder();
            try
            {
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Ask("Backup", $"The folder can't be opened: {ex.Message}", MessageBoxIcon.Error);
            }
        }

        private void OnClickBackupRestore(object sender, EventArgs e)
        {
            if (listViewBackup.SelectedItems.Count != 1 || listViewBackup.SelectedItems[0].Tag is not BackupEntry entry)
            {
                Ask("Backup", "Select a file in the list.", MessageBoxIcon.Information);
                return;
            }
            if (entry.OldCopy == null)
            {
                Ask("Backup", "There is no old copy of this file (only the new one).", MessageBoxIcon.Information);
                return;
            }
            string target = entry.OriginalPath;
            if (string.IsNullOrWhiteSpace(target))
            {
                target = BrowseFile("All files (*.*)|*.*", entry.FileName, true);
                if (target == null)
                {
                    return;
                }
            }
            if (MessageBox.Show($"Copy the old {entry.FileName} back to\r\n{target}?\r\n\r\nThe current file there is replaced (close the client first).",
                    "Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                PackerBackup.Restore(entry, target);
                Log($"Restored {entry.FileName} → {target}");
                Status($"{entry.FileName} restored.");
            }
            catch (Exception ex)
            {
                Ask("Restore", $"Restoring failed: {ex.Message}", MessageBoxIcon.Error);
            }
        }

        // ==================================================================== tab "Help"

        private void ShowHelp()
        {
            richTextBoxHelp.Text = radioButtonHelpGerman.Checked ? PackerHelp.German : PackerHelp.English;
        }

        private void OnHelpLanguageChanged(object sender, EventArgs e)
        {
            ShowHelp();
            SaveSettings();
        }
    }
    #endregion
}
