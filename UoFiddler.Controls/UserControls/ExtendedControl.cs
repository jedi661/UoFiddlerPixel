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
    public class ExtendedControl : UserControl
    {
        private static readonly UoExtensionKind[] _kinds = (UoExtensionKind[])Enum.GetValues(typeof(UoExtensionKind));

        private readonly Dictionary<UoExtensionKind, CheckBox> _use = new Dictionary<UoExtensionKind, CheckBox>();
        private readonly Dictionary<UoExtensionKind, NumericUpDown> _size = new Dictionary<UoExtensionKind, NumericUpDown>();
        private readonly Dictionary<UoExtensionKind, Label> _now = new Dictionary<UoExtensionKind, Label>();
        private readonly Dictionary<UoExtensionKind, Label> _range = new Dictionary<UoExtensionKind, Label>();
        private readonly Dictionary<UoExtensionKind, Label> _classic = new Dictionary<UoExtensionKind, Label>();
        private readonly Dictionary<UoExtensionKind, Label> _memory = new Dictionary<UoExtensionKind, Label>();

        private NumericUpDown _common;
        private Label _totalLabel, _statusLabel, _checkStatus;
        private CheckBox _saveRightAway, _backupFirst;
        private ComboBox _checkWhere, _severityFilter;
        private ListView _issuesList;
        private Button _runButton, _cancelButton;
        private List<ExtIssue> _issues = new List<ExtIssue>();
        private bool _cancel;
        private bool _running;
        private bool _loaded;
        private readonly ToolTip _tips = new ToolTip { AutoPopDelay = 20000 };

        public ExtendedControl()
        {
            Dock = DockStyle.Fill;
            BuildLayout();
            VisibleChanged += (s, e) =>
            {
                if (Visible)
                {
                    OnShownFirstTime();
                }
            };
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

        #region Layout

        private void BuildLayout()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(8), AutoScroll = true };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                AutoSize = true,
                MaximumSize = new Size(1200, 0),
                Text = "Extended mul files: the classic files (art.mul, tiledata.mul, texmaps.mul, hues.mul, radarcol.mul) stay unchanged for the old client. " +
                       "Each extension starts where the classic file ends and holds up to 16,777,216 entries for the new client. " +
                       "Choose the sizes, create everything with one click and check the files for errors.",
                Margin = new Padding(3, 3, 3, 8)
            }, 0, 0);

            root.Controls.Add(BuildSizeGroup(), 0, 1);
            root.Controls.Add(BuildActionGroup(), 0, 2);
            root.Controls.Add(BuildCheckGroup(), 0, 3);
        }

        private GroupBox BuildSizeGroup()
        {
            var group = new GroupBox { Text = "1. Sizes", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            var grid = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 8 };
            string[] heads = { "Use", "Extension", "New size (slots)", "Now", "Ids with the new size", "Old client / new client", "Memory", string.Empty };
            for (int c = 0; c < heads.Length; ++c)
            {
                grid.Controls.Add(new Label { Text = heads[c], AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 3, 12, 3) }, c, 0);
            }

            int row = 1;
            foreach (UoExtensionKind kind in _kinds)
            {
                var use = new CheckBox { AutoSize = true, Checked = true, Margin = new Padding(6, 5, 3, 3) };
                var size = new NumericUpDown
                {
                    Minimum = 1,
                    Maximum = UoExtensions.MaxSlots(kind),
                    Increment = UoExtensions.Increment(kind),
                    ThousandsSeparator = true,
                    Width = 120,
                    Value = UoExtensions.DefaultSlots(kind)
                };
                UoExtensionKind k = kind;
                size.ValueChanged += (s, e) => UpdateRow(k);
                use.CheckedChanged += (s, e) => UpdateTotal();
                _use[kind] = use;
                _size[kind] = size;
                _now[kind] = MakeInfo();
                _range[kind] = MakeInfo();
                _classic[kind] = MakeInfo();
                _memory[kind] = MakeInfo();

                grid.Controls.Add(use, 0, row);
                grid.Controls.Add(new Label { Text = UoExtensions.Title(kind), AutoSize = true, Margin = new Padding(3, 6, 12, 3) }, 1, row);
                grid.Controls.Add(size, 2, row);
                grid.Controls.Add(_now[kind], 3, row);
                grid.Controls.Add(_range[kind], 4, row);
                grid.Controls.Add(_classic[kind], 5, row);
                grid.Controls.Add(_memory[kind], 6, row);
                var max = new Button { Text = "Max", AutoSize = true };
                max.Click += (s, e) => _size[k].Value = _size[k].Maximum;
                grid.Controls.Add(max, 7, row);
                ++row;
            }

            var common = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, WrapContents = true, Margin = new Padding(3, 8, 3, 3) };
            common.Controls.Add(new Label { Text = "Same size for all checked:", AutoSize = true, Margin = new Padding(3, 7, 3, 3) });
            _common = new NumericUpDown { Minimum = 1, Maximum = ExtendedFormat.MaxSlots, Increment = 0x10000, ThousandsSeparator = true, Width = 120, Value = ExtendedFormat.DefaultSlots };
            common.Controls.Add(_common);
            common.Controls.Add(MakeButton("Apply", (s, e) => SetAll(k => (int)_common.Value)));
            common.Controls.Add(new Label { Text = "Presets:", AutoSize = true, Margin = new Padding(16, 7, 3, 3) });
            common.Controls.Add(MakeButton("Old-client limits", (s, e) => SetAll(k => k == UoExtensionKind.Items ? ExtendedFormat.DefaultSlots : UoExtensions.ClassicLimitSlots(k)),
                "Land / textures up to 0xFFFF, hues up to 16,383 (fit the classic 16/14-bit fields), items 1,048,576"));
            common.Controls.Add(MakeButton("65,536", (s, e) => SetAll(k => 0x10000)));
            common.Controls.Add(MakeButton("1,048,576", (s, e) => SetAll(k => 0x100000)));
            common.Controls.Add(MakeButton("16,777,216 (max)", (s, e) => SetAll(k => ExtendedFormat.MaxSlots), "Maximum for everything - needs a lot of memory, see the column 'Memory'"));
            common.Controls.Add(MakeButton("Current sizes", (s, e) => SetAll(k => UoExtensions.CurrentSlots(k) > 0 ? UoExtensions.CurrentSlots(k) : (int)_size[k].Value)));
            _totalLabel = new Label { AutoSize = true, Margin = new Padding(16, 7, 3, 3), Font = new Font(Font, FontStyle.Bold) };
            common.Controls.Add(_totalLabel);

            var outer = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            outer.Controls.Add(grid, 0, 0);
            outer.Controls.Add(common, 0, 1);
            group.Controls.Add(outer);
            return group;
        }

        private GroupBox BuildActionGroup()
        {
            var group = new GroupBox { Text = "2. Create / save / load", Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
            var create = new Button
            {
                Text = "Create / resize checked  (one click)",
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 240, 220),
                Padding = new Padding(6, 2, 6, 2)
            };
            create.Click += (s, e) => CreateChecked();
            _tips.SetToolTip(create, "Backup of the client files, then create or resize every checked extension (items incl. tiledata + radar colours, " +
                                     "land tiles incl. tiledata + radar colours, textures, hues) and - if checked - save the files to the output folder.");
            flow.Controls.Add(create);
            _backupFirst = new CheckBox { Text = "Backup first", AutoSize = true, Checked = true, Margin = new Padding(8, 8, 3, 3) };
            _saveRightAway = new CheckBox { Text = "Save the files right away", AutoSize = true, Checked = true, Margin = new Padding(8, 8, 3, 3) };
            flow.Controls.Add(_backupFirst);
            flow.Controls.Add(_saveRightAway);
            flow.SetFlowBreak(_saveRightAway, true);

            flow.Controls.Add(MakeButton("Save all extensions", (s, e) => SaveAll(), "Writes every existing extension into the output folder"));
            flow.Controls.Add(MakeButton("Load from folder...", (s, e) => LoadFromFolder()));
            flow.Controls.Add(MakeButton("Load from output folder", (s, e) => LoadFrom(Options.OutputPath)));
            flow.Controls.Add(MakeButton("Copy output -> Ultima Online folder", (s, e) => CopyToClient(),
                "Copies the side-car files from the output folder into the client folder (dated backup first) and loads them from there"));
            flow.Controls.Add(MakeButton("Backup now", (s, e) => Backup(true)));
            flow.Controls.Add(MakeButton("Open output folder", (s, e) => OpenFolder(Options.OutputPath)));
            flow.Controls.Add(MakeButton("Open UO folder", (s, e) => OpenFolder(Files.RootDir)));
            flow.Controls.Add(MakeButton("Open backup folder", (s, e) => OpenFolder(UoFileBackup.LastBackupFolder ?? UoFileBackup.GetBackupRoot(Options.OutputPath))));
            flow.Controls.Add(MakeButton("Details window...", (s, e) => ExtendedArtFormatForm.ShowFor(UoExtensionKind.Items)));
            _statusLabel = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 8, 3, 3), MaximumSize = new Size(1200, 0) };
            flow.SetFlowBreak(flow.Controls[flow.Controls.Count - 1], true);
            flow.Controls.Add(_statusLabel);
            group.Controls.Add(flow);
            return group;
        }

        private GroupBox BuildCheckGroup()
        {
            var group = new GroupBox { Text = "3. Error search", Dock = DockStyle.Fill, MinimumSize = new Size(0, 260) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var top = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, WrapContents = true };
            top.Controls.Add(new Label { Text = "Check:", AutoSize = true, Margin = new Padding(3, 7, 3, 3) });
            _checkWhere = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 420 };
            top.Controls.Add(_checkWhere);
            _runButton = MakeButton("Run check", (s, e) => RunCheck());
            _runButton.Font = new Font(Font, FontStyle.Bold);
            top.Controls.Add(_runButton);
            _cancelButton = MakeButton("Cancel", (s, e) => _cancel = true);
            _cancelButton.Enabled = false;
            top.Controls.Add(_cancelButton);
            top.Controls.Add(new Label { Text = "Show:", AutoSize = true, Margin = new Padding(12, 7, 3, 3) });
            _severityFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            _severityFilter.Items.AddRange(new object[] { "Everything", "Warnings + errors", "Errors only", "Repairable only" });
            _severityFilter.SelectedIndex = 0;
            _severityFilter.SelectedIndexChanged += (s, e) => FillIssues();
            top.Controls.Add(_severityFilter);
            _checkStatus = new Label { AutoSize = true, Margin = new Padding(12, 7, 3, 3) };
            top.Controls.Add(_checkStatus);
            layout.Controls.Add(top, 0, 0);

            _issuesList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = true };
            _issuesList.Columns.Add("Severity", 70);
            _issuesList.Columns.Add("Area", 85);
            _issuesList.Columns.Add("Id", 90);
            _issuesList.Columns.Add("File", 150);
            _issuesList.Columns.Add("Problem", 620);
            _issuesList.Columns.Add("Repair", 120);
            _issuesList.DoubleClick += (s, e) => GoToSelected();
            var menu = new ContextMenuStrip();
            menu.Items.Add("Go to (Items / LandTiles / TileData)", null, (s, e) => GoToSelected());
            menu.Items.Add("Repair selected", null, (s, e) => FixSelected(false));
            menu.Items.Add("Copy selected lines", null, (s, e) => CopyText(string.Join(Environment.NewLine, SelectedIssues().Select(i => i.ToString()))));
            _issuesList.ContextMenuStrip = menu;
            layout.Controls.Add(_issuesList, 0, 1);

            var bottom = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
            bottom.Controls.Add(MakeButton("Repair selected", (s, e) => FixSelected(false)));
            bottom.Controls.Add(MakeButton("Repair all repairable", (s, e) => FixSelected(true)));
            bottom.Controls.Add(MakeButton("Go to", (s, e) => GoToSelected()));
            bottom.Controls.Add(MakeButton("Copy report", (s, e) => CopyText(ExtendedValidator.ToText(_issues, $"UOFiddler extension check {DateTime.Now:yyyy-MM-dd HH:mm}"))));
            bottom.Controls.Add(MakeButton("Save report...", (s, e) => SaveReport()));
            bottom.Controls.Add(new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Margin = new Padding(12, 8, 3, 3),
                Text = "Repairs change the loaded data - save afterwards. Double click a line to jump to the id."
            });
            layout.Controls.Add(bottom, 0, 2);
            group.Controls.Add(layout);
            return group;
        }

        private static Label MakeInfo() => new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 6, 12, 3) };

        private Button MakeButton(string text, EventHandler click, string tip = null)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            if (tip != null)
            {
                _tips.SetToolTip(b, tip);
            }

            return b;
        }

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
                    RadarColorControl.Select(issue.Id, land);
                    RadarColorControl.ActivateTab();
                    break;
                case ExtIssueArea.Items:
                case ExtIssueArea.LandTiles:
                    // no graphic (or broken): show the tiledata entry
                    TileDataControl.Select(issue.Id, land);
                    TileDataControl.ActivateTab();
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
    }
}
