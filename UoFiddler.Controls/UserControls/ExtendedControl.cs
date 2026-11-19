/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * "Extended" tab: all side-car extensions in one place
 *  - sizes of items / land tiles / textures / hues together (one size for all or each its own)
 *  - one click: backup, create / resize, save
 *  - load / copy into the Ultima Online folder
 *  - error search: files on disk (ExtendedValidator) + loaded data (ExtendedDiagnostics), go to, repair
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class ExtendedControl : UserControl
    {
        private static readonly UoExtensionKind[] _kinds = (UoExtensionKind[])Enum.GetValues(typeof(UoExtensionKind));

        // the controls are in ExtendedControl.Designer.cs (one row per kind); these maps make the code per kind
        private readonly Dictionary<UoExtensionKind, CheckBox> _use = new Dictionary<UoExtensionKind, CheckBox>();
        private readonly Dictionary<UoExtensionKind, NumericUpDown> _size = new Dictionary<UoExtensionKind, NumericUpDown>();
        private readonly Dictionary<UoExtensionKind, Label> _now = new Dictionary<UoExtensionKind, Label>();
        private readonly Dictionary<UoExtensionKind, Label> _range = new Dictionary<UoExtensionKind, Label>();
        private readonly Dictionary<UoExtensionKind, Label> _classic = new Dictionary<UoExtensionKind, Label>();
        private readonly Dictionary<UoExtensionKind, Label> _memory = new Dictionary<UoExtensionKind, Label>();
        private readonly Dictionary<UoExtensionKind, CheckBox> _patchUse = new Dictionary<UoExtensionKind, CheckBox>();
        private readonly Dictionary<object, UoExtensionKind> _maxOf = new Dictionary<object, UoExtensionKind>();

        private List<ExtIssue> _issues = new List<ExtIssue>();
        private bool _cancel;
        private bool _running;
        private bool _loaded;
        private PatchTestResult _patchResult;

        public ExtendedControl()
        {
            InitializeComponent();

            Map(UoExtensionKind.Items, _useItems, _sizeItems, _nowItems, _rangeItems, _classicItems, _memoryItems, _maxItems, _patchItems);
            Map(UoExtensionKind.LandTiles, _useLand, _sizeLand, _nowLand, _rangeLand, _classicLand, _memoryLand, _maxLand, _patchLand);
            Map(UoExtensionKind.Textures, _useTextures, _sizeTextures, _nowTextures, _rangeTextures, _classicTextures, _memoryTextures, _maxTextures, _patchTextures);
            Map(UoExtensionKind.Hues, _useHues, _sizeHues, _nowHues, _rangeHues, _classicHues, _memoryHues, _maxHues, _patchHues);

            _severityFilter.SelectedIndex = 0;
            _tips.AutoPopDelay = 20000;
            _tips.SetToolTip(_createButton, "Backup of the client files, then create or resize every checked extension (items incl. tiledata + radar colours, " +
                                            "land tiles incl. tiledata + radar colours, textures, hues) and - if checked - save the files to the output folder.");
            _tips.SetToolTip(_presetClassic, "Land / textures up to 0xFFFF, hues up to 16,383 (fit the classic 16/14-bit fields), items 1,048,576");
            _tips.SetToolTip(_presetMax, "Maximum for everything - needs a lot of memory, see the column 'Memory'");
            _tips.SetToolTip(_saveAllButton, "Writes every existing extension into the output folder");
            _tips.SetToolTip(_copyToClientButton, "Copies the side-car files from the output folder into the client folder (dated backup first) and loads them from there");
            _tips.SetToolTip(_patchRunButton, "Copies a classic entry into a free slot (and the last slot) of every checked extension, writes a test copy and reads it back. " +
                                              "The loaded files are not changed.");
            _tips.SetToolTip(_patchUndoButton, "Removes the test entries from the loaded data again (the state before the test)");

            VisibleChanged += (s, e) =>
            {
                if (Visible)
                {
                    OnShownFirstTime();
                }
            };
        }

        private void Map(UoExtensionKind kind, CheckBox use, NumericUpDown size, Label now, Label range, Label classic, Label memory, Button max, CheckBox patch)
        {
            _use[kind] = use;
            _size[kind] = size;
            _now[kind] = now;
            _range[kind] = range;
            _classic[kind] = classic;
            _memory[kind] = memory;
            _patchUse[kind] = patch;
            _maxOf[size] = kind;
            _maxOf[max] = kind;
            size.Maximum = UoExtensions.MaxSlots(kind);
            size.Increment = UoExtensions.Increment(kind);
        }

        private void OnShownFirstTime()
        {
            if (_loaded || IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            _loaded = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Texture"] = true;
            Options.LoadedUltimaClass["Hues"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;
            UoExtensions.Changed += OnExtensionsChanged;
            ControlEvents.FilePathChangeEvent += RefreshState;
            Disposed += (s, e) =>
            {
                UoExtensions.Changed -= OnExtensionsChanged;
                ControlEvents.FilePathChangeEvent -= RefreshState;
            };

            foreach (UoExtensionKind kind in _kinds)
            {
                int current = UoExtensions.CurrentSlots(kind);
                _size[kind].Value = Math.Clamp(current > 0 ? current : UoExtensions.DefaultSlots(kind), 1, UoExtensions.MaxSlots(kind));
            }

            RefreshState();
        }

        private void OnExtensionsChanged(UoExtensionKind? kind)
        {
            if (IsHandleCreated)
            {
                RefreshState();
            }
        }

        #region Designer event handlers

        private void OnUseChanged(object sender, EventArgs e) => UpdateTotal();

        private void OnSizeChanged(object sender, EventArgs e)
        {
            if (_maxOf.TryGetValue(sender, out UoExtensionKind kind))
            {
                UpdateRow(kind);
            }
        }

        private void OnMaxClick(object sender, EventArgs e)
        {
            if (_maxOf.TryGetValue(sender, out UoExtensionKind kind))
            {
                _size[kind].Value = _size[kind].Maximum;
            }
        }

        private void OnApplyCommon(object sender, EventArgs e) => SetAll(k => (int)_common.Value);

        private void OnPresetClassic(object sender, EventArgs e) =>
            SetAll(k => k == UoExtensionKind.Items ? ExtendedFormat.DefaultSlots : UoExtensions.ClassicLimitSlots(k));

        private void OnPreset64k(object sender, EventArgs e) => SetAll(k => 0x10000);

        private void OnPreset1m(object sender, EventArgs e) => SetAll(k => 0x100000);

        private void OnPresetMax(object sender, EventArgs e) => SetAll(k => ExtendedFormat.MaxSlots);

        private void OnPresetCurrent(object sender, EventArgs e) =>
            SetAll(k => UoExtensions.CurrentSlots(k) > 0 ? UoExtensions.CurrentSlots(k) : (int)_size[k].Value);

        private void OnCreateClick(object sender, EventArgs e) => CreateChecked();

        private void OnSaveAllClick(object sender, EventArgs e) => SaveAll();

        private void OnLoadFolderClick(object sender, EventArgs e) => LoadFromFolder();

        private void OnLoadOutputClick(object sender, EventArgs e) => LoadFrom(Options.OutputPath);

        private void OnCopyToClientClick(object sender, EventArgs e) => CopyToClient();

        private void OnBackupClick(object sender, EventArgs e) => Backup(true);

        private void OnOpenOutputClick(object sender, EventArgs e) => OpenFolder(Options.OutputPath);

        private void OnOpenUoClick(object sender, EventArgs e) => OpenFolder(Files.RootDir);

        private void OnOpenBackupClick(object sender, EventArgs e) => OpenFolder(UoFileBackup.LastBackupFolder ?? UoFileBackup.GetBackupRoot(Options.OutputPath));

        private void OnDetailsClick(object sender, EventArgs e) => ExtendedArtFormatForm.ShowFor(UoExtensionKind.Items);

        private void OnRunCheckClick(object sender, EventArgs e) => RunCheck();

        private void OnCancelCheckClick(object sender, EventArgs e) => _cancel = true;

        private void OnSeverityFilterChanged(object sender, EventArgs e) => FillIssues();

        private void OnIssueDoubleClick(object sender, EventArgs e) => GoToSelected();

        private void OnRepairSelectedClick(object sender, EventArgs e) => FixSelected(false);

        private void OnRepairAllClick(object sender, EventArgs e) => FixSelected(true);

        private void OnCopyIssuesClick(object sender, EventArgs e) => CopyText(string.Join(Environment.NewLine, SelectedIssues().Select(i => i.ToString())));

        private void OnCopyReportClick(object sender, EventArgs e) =>
            CopyText(ExtendedValidator.ToText(_issues, $"UOFiddler extension check {DateTime.Now:yyyy-MM-dd HH:mm}"));

        private void OnSaveReportClick(object sender, EventArgs e) => SaveReport();

        #endregion

        #region Sizes

        private void SetAll(Func<UoExtensionKind, int> value)
        {
            foreach (UoExtensionKind kind in _kinds)
            {
                if (_use[kind].Checked)
                {
                    _size[kind].Value = Math.Clamp(value(kind), 1, UoExtensions.MaxSlots(kind));
                }
            }
        }

        private void UpdateRow(UoExtensionKind kind)
        {
            int slots = (int)_size[kind].Value;
            int b = UoExtensions.BaseId(kind);
            _range[kind].Text = UoExtensions.RangeText(kind, b, b + slots - 1);
            int classic = UoExtensions.ClassicLimitSlots(kind);
            _classic[kind].Text = kind == UoExtensionKind.Items
                ? "all: new client"
                : slots <= classic ? "all fit the old fields" : $"{classic:N0} old / {slots - classic:N0} new client";
            _memory[kind].Text = FormatBytes((long)slots * UoExtensions.EstimatedBytesPerSlot(kind));
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            long total = _kinds.Where(k => _use[k].Checked).Sum(k => (long)_size[k].Value * UoExtensions.EstimatedBytesPerSlot(k));
            _totalLabel.Text = $"Memory for the checked sizes approx. {FormatBytes(total)}";
            _totalLabel.ForeColor = total > (2L << 30) ? Color.DarkRed : Color.Black;
        }

        private static string FormatBytes(long bytes)
        {
            return bytes >= 1L << 30 ? $"{bytes / (double)(1L << 30):0.0} GB" : $"{bytes / (double)(1L << 20):0.0} MB";
        }

        /// <summary>Shows what is loaded now (slots, used, where from) and fills the folder list of the error search.</summary>
        private void RefreshState()
        {
            if (!_loaded)
            {
                return;
            }

            foreach (UoExtensionKind kind in _kinds)
            {
                int current = UoExtensions.CurrentSlots(kind);
                string where = kind switch
                {
                    UoExtensionKind.Items => Art.ExtendedDataPath,
                    UoExtensionKind.LandTiles => Art.LandExtension.DataPath,
                    UoExtensionKind.Textures => Textures.Extension.DataPath,
                    _ => Hues.ExtensionPath
                };
                _now[kind].Text = current == 0 ? "none" : $"{current:N0} ({UoExtensions.CountUsed(kind):N0} used)" + (where == null ? " - not saved" : string.Empty);
                _now[kind].ForeColor = current == 0 ? Color.DimGray : where == null ? Color.DarkOrange : Color.DarkGreen;
                _tips.SetToolTip(_now[kind], where ?? "only in memory");
                UpdateRow(kind);
            }

            string previous = _checkWhere.SelectedItem as string;
            _checkWhere.Items.Clear();
            _checkWhere.Items.Add("Loaded data (memory) only");
            foreach ((string label, string folder) in ExtendedCheck.GetFolders())
            {
                _checkWhere.Items.Add($"{label}: {folder}");
            }

            _checkWhere.Items.Add("Other folder...");
            int keep = previous != null ? _checkWhere.Items.IndexOf(previous) : -1;
            _checkWhere.SelectedIndex = keep >= 0 ? keep : Math.Min(1, _checkWhere.Items.Count - 1);
        }

        #endregion

        #region Create / save / load

        private bool Backup(bool show)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string folder = UoFileBackup.CreateBackup(Options.OutputPath, out List<string> copied);
                _statusLabel.Text = $"Backup: {folder} ({copied.Count} file(s))";
                if (show)
                {
                    MessageBox.Show(this, $"Backup created:{Environment.NewLine}{folder}{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, copied)}",
                        "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return true;
            }
            catch (Exception ex)
            {
                return MessageBox.Show(this, $"The backup failed:{Environment.NewLine}{ex.Message}{Environment.NewLine}{Environment.NewLine}Continue without backup?",
                    "Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void CreateChecked()
        {
            List<UoExtensionKind> kinds = _kinds.Where(k => _use[k].Checked).ToList();
            if (kinds.Count == 0)
            {
                MessageBox.Show(this, "Check at least one extension.", "Create", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_saveRightAway.Checked && (string.IsNullOrEmpty(Options.OutputPath) || !Directory.Exists(Options.OutputPath)))
            {
                MessageBox.Show(this, "The output folder is not set or does not exist (Options).", "Create", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // what gets lost when shrinking
            var lines = new List<string>();
            int lostTotal = 0;
            foreach (UoExtensionKind kind in kinds)
            {
                int slots = (int)_size[kind].Value;
                int current = UoExtensions.CurrentSlots(kind);
                string action = current == 0 ? "create" : slots == current ? "keep" : slots > current ? "grow" : "SHRINK";
                int lost = current > slots ? UoExtensions.CountUsedAbove(kind, slots) : 0;
                lostTotal += lost;
                lines.Add($"{UoExtensions.Title(kind),-28} {action,-7} {current:N0} -> {slots:N0}" + (lost > 0 ? $"   ({lost:N0} used entries are LOST)" : string.Empty));
            }

            long memory = kinds.Sum(k => (long)_size[k].Value * UoExtensions.EstimatedBytesPerSlot(k));
            if (MessageBox.Show(this,
                    string.Join(Environment.NewLine, lines) + Environment.NewLine + Environment.NewLine +
                    $"Memory approx. {FormatBytes(memory)}." + Environment.NewLine +
                    (_backupFirst.Checked ? "A backup of the client files is made first." + Environment.NewLine : string.Empty) +
                    (_saveRightAway.Checked ? $"The files are saved to {Options.OutputPath}." : "The files are NOT saved yet (use 'Save all extensions').") +
                    Environment.NewLine + Environment.NewLine + "Continue?",
                    "Create / resize", MessageBoxButtons.YesNo, lostTotal > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            if (_backupFirst.Checked && !Backup(false))
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            var done = new List<string>();
            try
            {
                foreach (UoExtensionKind kind in kinds)
                {
                    _statusLabel.Text = $"{UoExtensions.Title(kind)} ...";
                    _statusLabel.Refresh();
                    UoExtensions.CreateOrResize(kind, (int)_size[kind].Value);
                    if (_saveRightAway.Checked)
                    {
                        UoExtensions.Save(kind, Options.OutputPath);
                    }

                    done.Add(UoExtensions.Title(kind));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error after {string.Join(", ", done)}:{Environment.NewLine}{ex.Message}", "Create", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            UoExtensions.RaiseChanged(null); // every tab shows the new sizes
            RefreshState();
            _statusLabel.Text = $"Done: {string.Join(", ", done)}" + (_saveRightAway.Checked ? $" - saved to {Options.OutputPath}" : " - not saved yet");
            if (_saveRightAway.Checked)
            {
                MessageBox.Show(this, $"Created and saved to {Options.OutputPath}:{Environment.NewLine}{Environment.NewLine}{UoExtensions.DiskSummary(Options.OutputPath)}" +
                                      Environment.NewLine + "Next: run the error search on the output folder, then 'Copy output -> Ultima Online folder'.",
                    "Create / resize", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SaveAll()
        {
            if (!_kinds.Any(UoExtensions.Exists))
            {
                MessageBox.Show(this, "There is no extension yet - create one first.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                UoExtensions.SaveAll(Options.OutputPath);
                if (!Art.LegacyModified)
                {
                    Options.ChangedUltimaClass["Art"] = false;
                }

                if (TileData.IsLegacyUnchanged())
                {
                    Options.ChangedUltimaClass["TileData"] = false;
                }

                _statusLabel.Text = $"Saved to {Options.OutputPath}";
                MessageBox.Show(this, $"Saved to {Options.OutputPath}:{Environment.NewLine}{Environment.NewLine}{UoExtensions.DiskSummary(Options.OutputPath)}",
                    "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error: {ex.Message}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            RefreshState();
        }

        private void LoadFromFolder()
        {
            using var dlg = new FolderBrowserDialog { Description = "Folder with the extension files", SelectedPath = Options.OutputPath };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                LoadFrom(dlg.SelectedPath);
            }
        }

        private void LoadFrom(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show(this, $"The folder does not exist:{Environment.NewLine}{folder}", "Load", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(this, "Unsaved changes in the extended ranges are discarded. Load the extension files from" + Environment.NewLine + folder + " ?",
                    "Load", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            List<string> found;
            try
            {
                found = ExtendedCheck.LoadFrom(folder); // remembers the paths and reloads every extension
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if (found.Count == 0)
            {
                MessageBox.Show(this, $"No extension files in{Environment.NewLine}{folder}", "Load", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UoExtensions.RaiseChanged(null);
            foreach (UoExtensionKind kind in _kinds)
            {
                int current = UoExtensions.CurrentSlots(kind);
                if (current > 0)
                {
                    _size[kind].Value = Math.Clamp(current, 1, UoExtensions.MaxSlots(kind));
                }
            }

            RefreshState();
            _statusLabel.Text = $"Loaded {found.Count} file(s) from {folder}";
        }

        private void CopyToClient()
        {
            if (MessageBox.Show(this,
                    $"Copy the extension files from{Environment.NewLine}{Options.OutputPath}{Environment.NewLine}to{Environment.NewLine}{Files.RootDir}{Environment.NewLine}{Environment.NewLine}" +
                    "The files there are backed up first. The classic files (art.mul, tiledata.mul ...) are NOT copied. Continue?",
                    "Copy to Ultima Online folder", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                string backup = ExtendedCheck.CopyToClientFolder(Options.OutputPath, out List<string> copied);
                UoExtensions.RaiseChanged(null);
                RefreshState();
                _statusLabel.Text = $"Copied {copied.Count} file(s), backup {backup}";
                MessageBox.Show(this, $"Copied: {string.Join(", ", copied)}{Environment.NewLine}Backup: {backup}", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Copy failed: {ex.Message}", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void OpenFolder(string path)
        {
            try
            {
                UoFileBackup.OpenFolder(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Can not open the folder:{Environment.NewLine}{path}{Environment.NewLine}{ex.Message}", "Open folder",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Error search

        private async void RunCheck()
        {
            if (_running)
            {
                return;
            }

            string folder = null;
            int index = _checkWhere.SelectedIndex;
            if (index == _checkWhere.Items.Count - 1)
            {
                using var dlg = new FolderBrowserDialog { Description = "Folder to check", SelectedPath = Options.OutputPath };
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                folder = dlg.SelectedPath;
            }
            else if (index > 0)
            {
                string text = (string)_checkWhere.SelectedItem;
                folder = text.Substring(text.IndexOf(": ", StringComparison.Ordinal) + 2);
            }

            _running = true;
            _cancel = false;
            _runButton.Enabled = false;
            _cancelButton.Enabled = true;
            var result = new List<ExtIssue>();
            try
            {
                if (folder != null)
                {
                    // files on disk: pure file reading, runs in the background
                    List<ExtIssue> disk = await Task.Run(() => ExtendedValidator.ValidateFolder(folder,
                        p => BeginInvoke(new Action(() => _checkStatus.Text = p)), () => _cancel));
                    foreach (ExtIssue i in disk)
                    {
                        i.File = i.File ?? folder;
                    }

                    result.AddRange(disk);
                }

                if (!_cancel)
                {
                    // loaded data: uses the Ultima classes (not thread safe) -> UI thread
                    _checkStatus.Text = "Checking the loaded data ...";
                    _checkStatus.Refresh();
                    Cursor.Current = Cursors.WaitCursor;
                    result.AddRange(ExtendedDiagnostics.CheckMemory());
                }
            }
            catch (Exception ex)
            {
                result.Add(new ExtIssue { Severity = ExtIssueSeverity.Error, Message = $"Check failed: {ex.Message}" });
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                _running = false;
                _runButton.Enabled = true;
                _cancelButton.Enabled = false;
            }

            _issues = result.OrderByDescending(i => i.Severity).ThenBy(i => i.Area).ThenBy(i => i.Id).ToList();
            FillIssues();
            _checkStatus.Text = (_cancel ? "Cancelled - " : string.Empty) + ExtendedDiagnostics.CountText(_issues) +
                                $"  ({_issues.Count(ExtendedDiagnostics.CanFix):N0} repairable)";
        }

        private void FillIssues()
        {
            int filter = _severityFilter.SelectedIndex;
            IEnumerable<ExtIssue> shown = _issues.Where(i =>
                filter == 0 ||
                (filter == 1 && i.Severity >= ExtIssueSeverity.Warning) ||
                (filter == 2 && i.Severity == ExtIssueSeverity.Error) ||
                (filter == 3 && ExtendedDiagnostics.CanFix(i)));

            _issuesList.BeginUpdate();
            try
            {
                _issuesList.Items.Clear();
                foreach (ExtIssue i in shown)
                {
                    var item = new ListViewItem(i.Severity.ToString()) { Tag = i };
                    item.SubItems.Add(i.Area.ToString());
                    item.SubItems.Add(i.Id >= 0 ? $"0x{i.Id:X}" : string.Empty);
                    item.SubItems.Add(i.File ?? string.Empty);
                    item.SubItems.Add(i.Message);
                    item.SubItems.Add(FixText(i.Fix));
                    item.ForeColor = i.Severity == ExtIssueSeverity.Error ? Color.DarkRed : i.Severity == ExtIssueSeverity.Warning ? Color.DarkOrange : SystemColors.WindowText;
                    _issuesList.Items.Add(item);
                }
            }
            finally
            {
                _issuesList.EndUpdate();
            }
        }

        private static string FixText(ExtIssueFix fix) => fix switch
        {
            ExtIssueFix.SyncSizes => "sync sizes",
            ExtIssueFix.RemoveEntry => "remove entry",
            ExtIssueFix.Save => "save",
            ExtIssueFix.RadarFromGraphic => "colour from graphic",
            _ => string.Empty
        };

        private List<ExtIssue> SelectedIssues() => _issuesList.SelectedItems.Cast<ListViewItem>().Select(i => (ExtIssue)i.Tag).ToList();

        private void FixSelected(bool all)
        {
            List<ExtIssue> todo = (all ? _issues : SelectedIssues()).Where(ExtendedDiagnostics.CanFix).ToList();
            if (todo.Count == 0)
            {
                MessageBox.Show(this, "Nothing repairable selected.", "Repair", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool removes = todo.Any(i => i.Fix == ExtIssueFix.RemoveEntry);
            if (MessageBox.Show(this,
                    $"Repair {todo.Count:N0} problem(s)?" + Environment.NewLine +
                    (removes ? "Broken entries are removed from the LOADED extension (only useful if the checked folder is the loaded one)." + Environment.NewLine : string.Empty) +
                    "Save the extensions afterwards.",
                    "Repair", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            var results = new List<string>();
            bool syncedItems = false, syncedLand = false;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (ExtIssue issue in todo)
                {
                    // one sync / one save per kind is enough
                    if (issue.Fix == ExtIssueFix.SyncSizes)
                    {
                        bool land = issue.Area == ExtIssueArea.LandTiles;
                        if ((land && syncedLand) || (!land && syncedItems))
                        {
                            continue;
                        }

                        syncedLand |= land;
                        syncedItems |= !land;
                    }

                    try
                    {
                        results.Add(ExtendedDiagnostics.ApplyFix(issue, Options.OutputPath));
                    }
                    catch (Exception ex)
                    {
                        results.Add($"0x{issue.Id:X}: {ex.Message}");
                    }
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            UoExtensions.RaiseChanged(null);
            RefreshState();
            List<string> distinct = results.Distinct().ToList();
            MessageBox.Show(this, string.Join(Environment.NewLine, distinct.Take(25)) + (distinct.Count > 25 ? $"{Environment.NewLine}... ({distinct.Count - 25} more)" : string.Empty) +
                                  Environment.NewLine + Environment.NewLine + "Run the check again to see what is left.",
                "Repair", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GoToSelected()
        {
            ExtIssue issue = SelectedIssues().FirstOrDefault();
            if (issue == null || issue.Id < 0)
            {
                return;
            }

            bool land = issue.Area == ExtIssueArea.LandTiles || (issue.Area == ExtIssueArea.RadarColors && issue.Message.StartsWith("land", StringComparison.Ordinal));
            switch (issue.Area)
            {
                case ExtIssueArea.Items when Art.IsValidStatic(issue.Id) && ItemsControl.SearchGraphic(issue.Id):
                    ItemsControl.ActivateTab();
                    break;
                case ExtIssueArea.LandTiles when Art.IsValidLand(issue.Id) && LandTilesControl.SearchGraphic(issue.Id):
                    LandTilesControl.ActivateTab();
                    break;
                case ExtIssueArea.RadarColors:
                    RadarColorControl.ActivateTab(); // show the tab first, then select (so the list scrolls to the entry)
                    RadarColorControl.Select(issue.Id, land);
                    break;
                case ExtIssueArea.Items:
                case ExtIssueArea.LandTiles:
                    // no graphic (or broken): show the tiledata entry
                    TileDataControl.ActivateTab(); // show the tab first, then select (so the list scrolls to the entry)
                    TileDataControl.Select(issue.Id, land);
                    break;
                default:
                    CopyText($"0x{issue.Id:X}");
                    _checkStatus.Text = $"No tab to jump to - id 0x{issue.Id:X} copied.";
                    break;
            }
        }

        private void SaveReport()
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "Text (*.txt)|*.txt",
                FileName = $"extension_check_{DateTime.Now:yyyy-MM-dd_HH-mm}.txt",
                InitialDirectory = Directory.Exists(Options.OutputPath) ? Options.OutputPath : string.Empty
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(dlg.FileName, ExtendedValidator.ToText(_issues, $"UOFiddler extension check {DateTime.Now:yyyy-MM-dd HH:mm}"));
            }
        }

        private void CopyText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception)
            {
                // clipboard busy
            }
        }

        #endregion

        #region Patch test

        private void OnPatchRunClick(object sender, EventArgs e)
        {
            List<UoExtensionKind> kinds = _kinds.Where(k => _patchUse[k].Checked).ToList();
            List<UoExtensionKind> missing = kinds.Where(k => !UoExtensions.Exists(k)).ToList();
            if (kinds.Count == 0 || missing.Count == kinds.Count)
            {
                MessageBox.Show(this, "No extension to test - create the extensions first (section 1 + 2).", "Patch test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(Options.OutputPath) || !Directory.Exists(Options.OutputPath))
            {
                MessageBox.Show(this, "The output folder is not set or does not exist (Options).", "Patch test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_patchResult != null && !_patchResult.Undone && _patchResult.Entries.Any(x => x.Patched))
            {
                if (MessageBox.Show(this, "The entries of the last patch test are still in the loaded data. Remove them first?", "Patch test",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ExtendedPatchTest.Undo(_patchResult);
                }
            }

            int unsaved = (Art.HasExtension ? Art.GetUnsavedExtendedIds().Count : 0) +
                          (Art.LandExtension.Exists ? Art.LandExtension.GetUnsavedIds().Count : 0) +
                          (Textures.Extension.Exists ? Textures.Extension.GetUnsavedIds().Count : 0);
            if (unsaved > 0 && MessageBox.Show(this,
                    $"{unsaved:N0} changed extended entries are not saved yet. The test writes only a separate test copy and does not change your files, " +
                    "but your unsaved entries are included in that copy. Continue?",
                    "Patch test", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            _patchRunButton.Enabled = false;
            try
            {
                _patchResult = ExtendedPatchTest.Run(kinds, _patchLastSlot.Checked, Options.OutputPath, p =>
                {
                    _patchStatus.Text = p;
                    _patchStatus.Refresh();
                });
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                _patchRunButton.Enabled = true;
            }

            FillPatchList();
            UoExtensions.RaiseChanged(null); // the tabs show the patched entries
            _patchUndoButton.Enabled = _patchResult.Entries.Any(x => x.Patched);
            _patchOpenLogButton.Enabled = _patchResult.LogPath != null;
            _patchCopyLogButton.Enabled = true;
            int ok = _patchResult.Entries.Count(x => x.Ok);
            _patchStatus.Text = _patchResult.Entries.Count == 0
                ? "Nothing tested - see the log."
                : _patchResult.AllOk ? $"ALL OK ({ok} of {_patchResult.Entries.Count})" : $"ERRORS: {ok} of {_patchResult.Entries.Count} ok - please send the log";
            _patchStatus.ForeColor = _patchResult.AllOk ? Color.DarkGreen : Color.DarkRed;

            if (!_patchResult.AllOk)
            {
                MessageBox.Show(this, $"The patch test found problems.{Environment.NewLine}Log: {_patchResult.LogPath}{Environment.NewLine}{Environment.NewLine}" +
                                      "Use 'Copy log' and send it.", "Patch test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FillPatchList()
        {
            _patchList.BeginUpdate();
            try
            {
                _patchList.Items.Clear();
                if (_patchResult == null)
                {
                    return;
                }

                foreach (PatchTestEntry e in _patchResult.Entries)
                {
                    bool hue = e.Kind == UoExtensionKind.Hues;
                    var item = new ListViewItem(UoExtensions.Title(e.Kind)) { Tag = e };
                    item.SubItems.Add(hue ? $"hue {e.SourceId + 1}" : $"0x{e.SourceId:X}");
                    item.SubItems.Add(hue ? $"hue {e.TargetId + 1} (0x{e.TargetId + 1:X})" : $"0x{e.TargetId:X}");
                    item.SubItems.Add(e.MemoryOk ? "ok" : "FAIL");
                    item.SubItems.Add(e.DiskOk ? "ok" : "FAIL");
                    item.SubItems.Add(e.ReloadOk ? "ok" : "FAIL");
                    item.SubItems.Add(_patchResult.Undone ? e.Result + " (test entry removed again)" : e.Result);
                    item.ForeColor = e.Ok ? Color.DarkGreen : Color.DarkRed;
                    _patchList.Items.Add(item);
                }
            }
            finally
            {
                _patchList.EndUpdate();
            }
        }

        private void OnPatchUndoClick(object sender, EventArgs e)
        {
            if (_patchResult == null)
            {
                return;
            }

            int n = ExtendedPatchTest.Undo(_patchResult);
            UoExtensions.RaiseChanged(null);
            FillPatchList();
            _patchUndoButton.Enabled = false;
            _patchStatus.Text = $"{n} test entr(y/ies) removed from the loaded data.";
            _patchStatus.ForeColor = Color.Black;
        }

        private void OnPatchOpenLogClick(object sender, EventArgs e)
        {
            if (_patchResult?.LogPath == null || !File.Exists(_patchResult.LogPath))
            {
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_patchResult.LogPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Open log", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnPatchCopyLogClick(object sender, EventArgs e)
        {
            CopyText(_patchResult?.Log);
            _patchStatus.Text = "Log copied to the clipboard.";
        }

        private PatchTestEntry SelectedPatchEntry() =>
            _patchList.SelectedItems.Count > 0 ? (PatchTestEntry)_patchList.SelectedItems[0].Tag : null;

        private void OnPatchGoGraphicClick(object sender, EventArgs e)
        {
            PatchTestEntry entry = SelectedPatchEntry();
            if (entry != null)
            {
                ShowPatchEntry(entry, true);
            }
        }

        /// <summary>Single click: mark the entry in all its tabs (graphic tab, TileData, RadarColor) without switching.</summary>
        private void OnPatchSelectionChanged(object sender, EventArgs e)
        {
            PatchTestEntry entry = SelectedPatchEntry();
            if (entry != null && entry.Patched)
            {
                ShowPatchEntry(entry, false);
            }
        }

        /// <summary>
        /// Marks the patched entry in every tab it belongs to (graphic / hue, TileData, RadarColor).
        /// <paramref name="open"/> = true brings the graphic tab to the front (first the tab, then the selection, so the
        /// view scrolls to the entry).
        /// </summary>
        private void ShowPatchEntry(PatchTestEntry entry, bool open)
        {
            int t = entry.TargetId;
            bool found;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                switch (entry.Kind)
                {
                    case UoExtensionKind.Items:
                        if (open)
                        {
                            ItemsControl.ActivateTab();
                        }

                        found = ItemsControl.SearchGraphic(t);
                        TileDataControl.Select(t, false);
                        RadarColorControl.Select(t, false);
                        if (open)
                        {
                            ItemsControl.ActivateTab(); // TileData / RadarColor may have loaded in between
                        }

                        break;
                    case UoExtensionKind.LandTiles:
                        if (open)
                        {
                            LandTilesControl.ActivateTab();
                        }

                        found = LandTilesControl.SearchGraphic(t);
                        TileDataControl.Select(t, true);
                        RadarColorControl.Select(t, true);
                        if (open)
                        {
                            LandTilesControl.ActivateTab();
                        }

                        break;
                    case UoExtensionKind.Textures:
                        found = TexturesControl.SelectInTab(t, open);
                        break;
                    default:
                        found = HuesControl.SelectInTab(t, open);
                        break;
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            string name = entry.Kind == UoExtensionKind.Hues ? $"hue {t + 1}" : $"0x{t:X}";
            if (!found)
            {
                _patchStatus.Text = entry.Patched
                    ? $"{name} not found in its tab - is the tab loaded / the view filtered?"
                    : $"{name}: the test entry was removed again.";
                _patchStatus.ForeColor = Color.DarkOrange;
                return;
            }

            _patchStatus.ForeColor = Color.DarkGreen;
            _patchStatus.Text = entry.Kind switch
            {
                UoExtensionKind.Items => $"{name} marked in Items, TileData and RadarColor" + (open ? string.Empty : " - double click opens the Items tab"),
                UoExtensionKind.LandTiles => $"{name} marked in LandTiles, TileData and RadarColor" + (open ? string.Empty : " - double click opens the LandTiles tab"),
                UoExtensionKind.Textures => $"{name} marked in Textures" + (open ? string.Empty : " - double click opens the Textures tab"),
                _ => $"{name} marked in Hues" + (open ? string.Empty : " - double click opens the Hues tab")
            };
        }

        private void OnPatchGoTileDataClick(object sender, EventArgs e)
        {
            PatchTestEntry entry = SelectedPatchEntry();
            if (entry == null || (entry.Kind != UoExtensionKind.Items && entry.Kind != UoExtensionKind.LandTiles))
            {
                _patchStatus.Text = "TileData exists for items and land tiles only.";
                return;
            }

            TileDataControl.ActivateTab(); // show the tab first, then select (so the list scrolls to the entry)
            TileDataControl.Select(entry.TargetId, entry.Kind == UoExtensionKind.LandTiles);
        }

        private void OnPatchGoRadarClick(object sender, EventArgs e)
        {
            PatchTestEntry entry = SelectedPatchEntry();
            if (entry == null || (entry.Kind != UoExtensionKind.Items && entry.Kind != UoExtensionKind.LandTiles))
            {
                _patchStatus.Text = "Radar colours exist for items and land tiles only.";
                return;
            }

            RadarColorControl.ActivateTab(); // show the tab first, then select (so the list scrolls to the entry)
            RadarColorControl.Select(entry.TargetId, entry.Kind == UoExtensionKind.LandTiles);
        }

        #endregion
    }
}
