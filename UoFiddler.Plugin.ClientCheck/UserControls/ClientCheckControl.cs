/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Tab "Client check": runs the checks, lists the results, shows details with
 * a preview, fixes what can be fixed automatically, exports a report.
 * All texts in German / English / French (switch in the toolbar).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.ClientCheck.Classes;
using UoFiddler.Plugin.ClientCheck.Forms;

namespace UoFiddler.Plugin.ClientCheck.UserControls
{
    public class ClientCheckControl : UserControl
    {
        private static readonly CheckCategory[] _categories =
        {
            CheckCategory.ArtTileData, CheckCategory.LandTiles, CheckCategory.Equipment, CheckCategory.Bodies,
            CheckCategory.RadarCol, CheckCategory.Multis, CheckCategory.Statics, CheckCategory.Cliloc
        };

        private List<CheckIssue> _issues = new List<CheckIssue>();
        private List<CheckIssue> _view = new List<CheckIssue>();
        private CancellationTokenSource _cts;
        private bool _running;
        private int _sortColumn = -1;
        private readonly Font _boldFont = new Font("Segoe UI", 9f, FontStyle.Bold);
        private bool _sortDescending;

        // toolbar 1
        private ToolStripButton _btnStart;
        private ToolStripButton _btnCancel;
        private ToolStripDropDownButton _ddChecks;
        private readonly Dictionary<CheckCategory, ToolStripMenuItem> _checkItems = new Dictionary<CheckCategory, ToolStripMenuItem>();
        private ToolStripMenuItem _checkAll;
        private ToolStripMenuItem _checkNone;
        private ToolStripLabel _lblLang;
        private ToolStripComboBox _cmbLang;
        private ToolStripButton _btnHelp;

        // toolbar 2
        private ToolStripLabel _lblShow;
        private ToolStripComboBox _cmbSeverity;
        private ToolStripComboBox _cmbCategory;
        private ToolStripLabel _lblSearch;
        private ToolStripTextBox _txtSearch;
        private ToolStripButton _btnHideFixed;
        private ToolStripButton _btnFixAll;
        private ToolStripDropDownButton _ddExport;
        private ToolStripMenuItem _exportHtml;
        private ToolStripMenuItem _exportCsv;
        private ToolStripMenuItem _exportTxt;
        private ToolStripButton _btnSaveRadar;

        // list + details
        private Label _summary;
        private ListView _list;
        private Label _detailTitle;
        private Label _detailMessage;
        private Label _detailHintTitle;
        private Label _detailHint;
        private Label _detailFix;
        private PictureBox _preview;
        private Button _btnFix;
        private Button _btnCopyId;
        private Button _btnCopyLine;

        // status
        private ToolStripProgressBar _progress;
        private ToolStripStatusLabel _status;
        private ToolStripStatusLabel _shown;

        public ClientCheckControl()
        {
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 9f);
            BuildUi();
            ApplyTexts();
            Loc.LanguageChanged += OnLanguageChanged;
            Disposed += (s, e) =>
            {
                Loc.LanguageChanged -= OnLanguageChanged;
                _cts?.Cancel();
            };
        }

        #region UI

        private void BuildUi()
        {
            // ── toolbar 1: run, checks, language, help ─────────────────
            var bar1 = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(4, 2, 4, 2), RenderMode = ToolStripRenderMode.System };
            _btnStart = new ToolStripButton { Font = new Font(Font, FontStyle.Bold) };
            _btnStart.Click += (s, e) => StartCheck();
            _btnCancel = new ToolStripButton { Enabled = false };
            _btnCancel.Click += (s, e) => _cts?.Cancel();

            _ddChecks = new ToolStripDropDownButton();
            CheckCategory saved = LoadSelectedCategories();
            foreach (CheckCategory c in _categories)
            {
                var item = new ToolStripMenuItem { CheckOnClick = true, Checked = (saved & c) != 0 };
                item.CheckedChanged += (s, e) => SaveSelectedCategories();
                _ddChecks.DropDownItems.Add(item);
                _checkItems[c] = item;
            }

            _ddChecks.DropDownItems.Add(new ToolStripSeparator());
            _checkAll = new ToolStripMenuItem();
            _checkAll.Click += (s, e) => { foreach (ToolStripMenuItem i in _checkItems.Values) { i.Checked = true; } };
            _checkNone = new ToolStripMenuItem();
            _checkNone.Click += (s, e) => { foreach (ToolStripMenuItem i in _checkItems.Values) { i.Checked = false; } };
            _ddChecks.DropDownItems.Add(_checkAll);
            _ddChecks.DropDownItems.Add(_checkNone);
            _ddChecks.DropDown.Closing += (s, e) =>
            {
                // keep the menu open while ticking several checks
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                {
                    e.Cancel = true;
                }
            };

            _lblLang = new ToolStripLabel { Alignment = ToolStripItemAlignment.Right };
            _cmbLang = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 95, Alignment = ToolStripItemAlignment.Right };
            _cmbLang.Items.AddRange(Loc.LanguageNames.Cast<object>().ToArray());
            _cmbLang.SelectedIndex = Math.Max(0, Array.IndexOf(Loc.Languages, Loc.Language));
            _cmbLang.SelectedIndexChanged += (s, e) => Loc.Language = Loc.Languages[Math.Max(0, _cmbLang.SelectedIndex)];
            _btnHelp = new ToolStripButton { Alignment = ToolStripItemAlignment.Right };
            _btnHelp.Click += (s, e) => ClientCheckHelpForm.ShowHelp(FindForm());

            bar1.Items.AddRange(new ToolStripItem[] { _btnStart, _btnCancel, new ToolStripSeparator(), _ddChecks, _btnHelp, _cmbLang, _lblLang });

            // ── toolbar 2: filter, fix, export ─────────────────────────
            var bar2 = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(4, 2, 4, 2), RenderMode = ToolStripRenderMode.System };
            _lblShow = new ToolStripLabel();
            _cmbSeverity = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 120 };
            _cmbSeverity.SelectedIndexChanged += (s, e) => ApplyFilter();
            _cmbCategory = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 170 };
            _cmbCategory.SelectedIndexChanged += (s, e) => ApplyFilter();
            _lblSearch = new ToolStripLabel();
            _txtSearch = new ToolStripTextBox { AutoSize = false, Width = 160 };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            _btnHideFixed = new ToolStripButton { CheckOnClick = true };
            _btnHideFixed.CheckedChanged += (s, e) => ApplyFilter();
            _btnFixAll = new ToolStripButton();
            _btnFixAll.Click += (s, e) => FixAll();
            _ddExport = new ToolStripDropDownButton();
            _exportHtml = new ToolStripMenuItem();
            _exportHtml.Click += (s, e) => Export("html");
            _exportCsv = new ToolStripMenuItem();
            _exportCsv.Click += (s, e) => Export("csv");
            _exportTxt = new ToolStripMenuItem();
            _exportTxt.Click += (s, e) => Export("txt");
            _ddExport.DropDownItems.AddRange(new ToolStripItem[] { _exportHtml, _exportCsv, _exportTxt });
            _btnSaveRadar = new ToolStripButton { Enabled = false };
            _btnSaveRadar.Click += (s, e) => SaveRadar();

            bar2.Items.AddRange(new ToolStripItem[]
            {
                _lblShow, _cmbSeverity, _cmbCategory, _lblSearch, _txtSearch, _btnHideFixed,
                new ToolStripSeparator(), _btnFixAll, _btnSaveRadar, new ToolStripSeparator(), _ddExport
            });

            // ── summary ────────────────────────────────────────────────
            _summary = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                Padding = new Padding(8, 5, 0, 0),
                Font = new Font(Font, FontStyle.Bold),
                BackColor = Color.FromArgb(245, 245, 248)
            };

            // ── list ───────────────────────────────────────────────────
            _list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                VirtualMode = true,
                MultiSelect = true,
                GridLines = false
            };
            _list.Columns.Add(string.Empty, 90);
            _list.Columns.Add(string.Empty, 170);
            _list.Columns.Add(string.Empty, 90);
            _list.Columns.Add(string.Empty, 700);
            _list.RetrieveVirtualItem += OnRetrieveItem;
            _list.SelectedIndexChanged += (s, e) => ShowDetails();
            _list.ColumnClick += OnColumnClick;
            _list.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.C)
                {
                    CopySelectedLines();
                    e.Handled = true;
                }
            };

            // ── details ────────────────────────────────────────────────
            var details = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), AutoScroll = true, BackColor = Color.White };
            _detailTitle = new Label { Dock = DockStyle.Top, AutoSize = false, Height = 28, Font = new Font("Segoe UI", 11f, FontStyle.Bold) };
            _detailMessage = new Label { Dock = DockStyle.Top, AutoSize = true, MaximumSize = new Size(380, 0), Padding = new Padding(0, 0, 0, 10) };
            _detailHintTitle = new Label { Dock = DockStyle.Top, AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(0, 4, 0, 2) };
            _detailHint = new Label { Dock = DockStyle.Top, AutoSize = true, MaximumSize = new Size(380, 0), ForeColor = Color.FromArgb(40, 70, 120), Padding = new Padding(0, 0, 0, 10) };
            _detailFix = new Label { Dock = DockStyle.Top, AutoSize = true, MaximumSize = new Size(380, 0), ForeColor = Color.DarkGreen, Padding = new Padding(0, 0, 0, 8) };
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
            _btnFix = new Button { AutoSize = true };
            _btnFix.Click += (s, e) => FixSelected();
            _btnCopyId = new Button { AutoSize = true };
            _btnCopyId.Click += (s, e) => CopyText(SelectedIssue?.IdText);
            _btnCopyLine = new Button { AutoSize = true };
            _btnCopyLine.Click += (s, e) => CopySelectedLines();
            buttons.Controls.AddRange(new Control[] { _btnFix, _btnCopyId, _btnCopyLine });
            _preview = new PictureBox { Dock = DockStyle.Top, Height = 220, BackColor = Color.FromArgb(40, 40, 44), SizeMode = PictureBoxSizeMode.CenterImage };

            // docked top: added in reverse order
            details.Controls.Add(_preview);
            details.Controls.Add(buttons);
            details.Controls.Add(_detailFix);
            details.Controls.Add(_detailHint);
            details.Controls.Add(_detailHintTitle);
            details.Controls.Add(_detailMessage);
            details.Controls.Add(_detailTitle);
            details.Resize += (s, e) =>
            {
                int w = Math.Max(150, details.ClientSize.Width - 30);
                _detailMessage.MaximumSize = new Size(w, 0);
                _detailHint.MaximumSize = new Size(w, 0);
                _detailFix.MaximumSize = new Size(w, 0);
            };

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 5 };
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(details);
            HandleCreated += (s, e) =>
            {
                try
                {
                    split.SplitterDistance = Math.Max(300, Width - 430);
                }
                catch (InvalidOperationException)
                {
                    // ignored
                }
            };

            // ── status ─────────────────────────────────────────────────
            var status = new StatusStrip { SizingGrip = false };
            _progress = new ToolStripProgressBar { Width = 180, Visible = false };
            _status = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _shown = new ToolStripStatusLabel { ForeColor = Color.DimGray };
            status.Items.AddRange(new ToolStripItem[] { _progress, _status, _shown });

            Controls.Add(split);
            Controls.Add(_summary);
            Controls.Add(status);
            Controls.Add(bar2);
            Controls.Add(bar1);
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            ApplyTexts();
        }

        /// <summary>All visible texts in the current language (also after switching)</summary>
        private void ApplyTexts()
        {
            _btnStart.Text = Loc.T("btn.start");
            _btnCancel.Text = Loc.T("btn.cancel");
            _ddChecks.Text = Loc.T("btn.checks");
            foreach (KeyValuePair<CheckCategory, ToolStripMenuItem> pair in _checkItems)
            {
                pair.Value.Text = Loc.T("cat." + pair.Key);
            }

            _checkAll.Text = Loc.T("btn.checks.all");
            _checkNone.Text = Loc.T("btn.checks.none");
            _lblLang.Text = Loc.T("lang.label");
            _btnHelp.Text = Loc.T("btn.help");
            _lblShow.Text = Loc.T("filter.show");
            _lblSearch.Text = Loc.T("filter.search");
            _btnHideFixed.Text = Loc.T("filter.hidefixed");
            _btnFixAll.Text = Loc.T("btn.fixall");
            _ddExport.Text = Loc.T("btn.export");
            _exportHtml.Text = Loc.T("btn.export.html");
            _exportCsv.Text = Loc.T("btn.export.csv");
            _exportTxt.Text = Loc.T("btn.export.txt");
            _btnSaveRadar.Text = Loc.T("btn.saveradar");
            _btnFix.Text = Loc.T("btn.fix");
            _btnCopyId.Text = Loc.T("btn.copyid");
            _btnCopyLine.Text = Loc.T("btn.copyline");
            _detailHintTitle.Text = Loc.T("detail.hint");

            _list.Columns[0].Text = Loc.T("col.severity");
            _list.Columns[1].Text = Loc.T("col.category");
            _list.Columns[2].Text = Loc.T("col.id");
            _list.Columns[3].Text = Loc.T("col.message");

            int sev = Math.Max(0, _cmbSeverity.SelectedIndex);
            _cmbSeverity.Items.Clear();
            _cmbSeverity.Items.AddRange(new object[] { Loc.T("sev.all"), Loc.T("sev.Error"), Loc.T("sev.Warning"), Loc.T("sev.Info") });
            _cmbSeverity.SelectedIndex = sev;

            int cat = Math.Max(0, _cmbCategory.SelectedIndex);
            _cmbCategory.Items.Clear();
            _cmbCategory.Items.Add(Loc.T("filter.allareas"));
            foreach (CheckCategory c in _categories)
            {
                _cmbCategory.Items.Add(Loc.T("cat." + c));
            }

            _cmbCategory.SelectedIndex = cat;

            if (Parent is TabPage page)
            {
                page.Text = Loc.T("tab.title");
            }

            if (!_running)
            {
                _status.Text = _issues.Count == 0 ? Loc.T("status.ready") : _status.Text;
            }

            UpdateSummary();
            _list.Invalidate();
            ShowDetails();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent is TabPage page)
            {
                page.Text = Loc.T("tab.title");
            }
        }

        #endregion

        #region Settings

        private static string ChecksFile => Path.Combine(Options.AppDataPath ?? string.Empty, "ClientCheck.checks");

        private static CheckCategory LoadSelectedCategories()
        {
            try
            {
                if (File.Exists(ChecksFile) && int.TryParse(File.ReadAllText(ChecksFile).Trim(), out int value) && value > 0)
                {
                    return (CheckCategory)value;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return CheckCategory.All;
        }

        private void SaveSelectedCategories()
        {
            try
            {
                File.WriteAllText(ChecksFile, ((int)SelectedCategories).ToString());
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private CheckCategory SelectedCategories
        {
            get
            {
                CheckCategory result = CheckCategory.None;
                foreach (KeyValuePair<CheckCategory, ToolStripMenuItem> pair in _checkItems)
                {
                    if (pair.Value.Checked)
                    {
                        result |= pair.Key;
                    }
                }

                return result;
            }
        }

        #endregion

        #region Run

        private async void StartCheck()
        {
            if (_running)
            {
                return;
            }

            CheckCategory categories = SelectedCategories;
            if (categories == CheckCategory.None)
            {
                _ddChecks.ShowDropDown();
                return;
            }

            _running = true;
            _cts = new CancellationTokenSource();
            _btnStart.Enabled = false;
            _btnCancel.Enabled = true;
            _progress.Visible = true;
            _progress.Value = 0;
            _issues = new List<CheckIssue>();
            ApplyFilter();

            var watch = Stopwatch.StartNew();
            var progress = new Progress<CheckProgress>(p =>
            {
                _progress.Value = p.Percent;
                if (!string.IsNullOrEmpty(p.Step))
                {
                    _status.Text = Loc.F("status.running", p.Step);
                }
            });

            try
            {
                Cursor = Cursors.AppStarting;
                var engine = new CheckEngine();
                _issues = await engine.RunAsync(categories, progress, _cts.Token);
                _status.Text = Loc.F("status.done", watch.Elapsed.TotalSeconds,
                    _issues.Count(i => i.Severity == CheckSeverity.Error),
                    _issues.Count(i => i.Severity == CheckSeverity.Warning),
                    _issues.Count(i => i.Severity == CheckSeverity.Info));
            }
            catch (OperationCanceledException)
            {
                _status.Text = Loc.T("status.cancelled");
            }
            catch (Exception ex)
            {
                _status.Text = Loc.F("status.failed", ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
                _running = false;
                _btnStart.Enabled = true;
                _btnCancel.Enabled = false;
                _progress.Visible = false;
                _cts.Dispose();
                _cts = null;
            }

            ApplyFilter();
        }

        #endregion

        #region List

        private void ApplyFilter()
        {
            if (_cmbSeverity == null || _cmbCategory == null)
            {
                return;
            }

            int sev = _cmbSeverity.SelectedIndex;
            int cat = _cmbCategory.SelectedIndex;
            string search = _txtSearch.Text.Trim();
            bool hideFixed = _btnHideFixed.Checked;

            IEnumerable<CheckIssue> query = _issues;
            if (sev > 0)
            {
                var severity = (CheckSeverity)(sev - 1);
                query = query.Where(i => i.Severity == severity);
            }

            if (cat > 0)
            {
                CheckCategory category = _categories[cat - 1];
                query = query.Where(i => i.Category == category);
            }

            if (hideFixed)
            {
                query = query.Where(i => !i.Fixed);
            }

            if (search.Length > 0)
            {
                query = query.Where(i => (i.IdText ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         i.Message.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _view = query.ToList();
            SortView();
            _list.SelectedIndices.Clear();
            _list.VirtualListSize = _view.Count;
            _list.Invalidate();
            _shown.Text = Loc.F("status.shown", _view.Count, _issues.Count);
            UpdateSummary();
            UpdateButtons();
            ShowDetails();
        }

        private void SortView()
        {
            Comparison<CheckIssue> cmp = _sortColumn switch
            {
                1 => (a, b) => string.Compare(a.CategoryText, b.CategoryText, StringComparison.CurrentCulture),
                2 => (a, b) => a.Id.CompareTo(b.Id),
                3 => (a, b) => string.Compare(a.Message, b.Message, StringComparison.CurrentCulture),
                _ => (a, b) => a.Severity != b.Severity ? a.Severity.CompareTo(b.Severity)
                    : a.Category != b.Category ? a.Category.CompareTo(b.Category)
                    : a.Id.CompareTo(b.Id)
            };

            _view.Sort((a, b) => _sortDescending ? cmp(b, a) : cmp(a, b));
        }

        private void OnColumnClick(object sender, ColumnClickEventArgs e)
        {
            _sortDescending = _sortColumn == e.Column && !_sortDescending;
            _sortColumn = e.Column;
            SortView();
            _list.Invalidate();
            ShowDetails();
        }

        private void OnRetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                e.Item = new ListViewItem(new[] { string.Empty, string.Empty, string.Empty, string.Empty });
                return;
            }

            CheckIssue i = _view[e.ItemIndex];
            var item = new ListViewItem(new[]
            {
                (i.Fixed ? "✓ " : string.Empty) + i.SeverityText,
                i.CategoryText,
                i.IdText,
                i.Message
            });

            item.ForeColor = i.Fixed ? Color.Gray : i.Severity switch
            {
                CheckSeverity.Error => Color.FromArgb(176, 0, 32),
                CheckSeverity.Warning => Color.FromArgb(190, 100, 0),
                _ => Color.FromArgb(21, 101, 192)
            };
            e.Item = item;
        }

        private void UpdateSummary()
        {
            if (_issues.Count == 0)
            {
                _summary.Text = string.Empty;
                return;
            }

            var parts = new List<string> { CheckReport.Summary(_issues) };
            foreach (IGrouping<CheckCategory, CheckIssue> g in _issues.GroupBy(i => i.Category).OrderBy(g => g.Key))
            {
                parts.Add($"{Loc.T("cat." + g.Key)}: {g.Count()}");
            }

            _summary.Text = string.Join("    |    ", parts);
            _summary.ForeColor = _issues.Any(i => i.Severity == CheckSeverity.Error && !i.Fixed) ? Color.FromArgb(176, 0, 32) : Color.FromArgb(30, 110, 40);
        }

        private CheckIssue SelectedIssue => _list.SelectedIndices.Count > 0 && _list.SelectedIndices[0] < _view.Count ? _view[_list.SelectedIndices[0]] : null;

        private List<CheckIssue> SelectedIssues =>
            _list.SelectedIndices.Cast<int>().Where(i => i < _view.Count).Select(i => _view[i]).ToList();

        #endregion

        #region Details

        private void ShowDetails()
        {
            CheckIssue i = SelectedIssue;
            if (i == null)
            {
                _detailTitle.Text = string.Empty;
                _detailMessage.Text = Loc.T("detail.select");
                _detailHint.Text = string.Empty;
                _detailHintTitle.Visible = false;
                _detailFix.Text = string.Empty;
                SetPreview(null);
                UpdateButtons();
                return;
            }

            _detailTitle.Text = $"{i.SeverityText} – {i.CategoryText} – {i.IdText}";
            _detailTitle.ForeColor = i.Severity switch
            {
                CheckSeverity.Error => Color.FromArgb(176, 0, 32),
                CheckSeverity.Warning => Color.FromArgb(190, 100, 0),
                _ => Color.FromArgb(21, 101, 192)
            };
            _detailMessage.Text = i.Message;
            _detailHintTitle.Visible = true;
            _detailHint.Text = i.Hint;
            _detailFix.Text = i.Fixed ? Loc.T("detail.fixed")
                : i.Fix != FixKind.None ? $"{Loc.T("detail.autofix")} {CheckFixes.FixDescription(i.Fix)}"
                : string.Empty;
            SetPreview(i);
            UpdateButtons();
        }

        private void SetPreview(CheckIssue issue)
        {
            Image old = _preview.Image;
            _preview.Image = null;
            old?.Dispose();

            if (issue == null || issue.Preview == PreviewKind.None)
            {
                return;
            }

            Bitmap bmp = null;
            try
            {
                switch (issue.Preview)
                {
                    case PreviewKind.Item:
                        if (Art.IsValidStatic(issue.PreviewId))
                        {
                            bmp = Art.GetStatic(issue.PreviewId);
                        }

                        break;
                    case PreviewKind.Land:
                        if (Art.IsValidLand(issue.PreviewId))
                        {
                            bmp = Art.GetLand(issue.PreviewId);
                        }

                        break;
                    case PreviewKind.Gump:
                        if (Gumps.IsValidIndex(issue.PreviewId))
                        {
                            bmp = Gumps.GetGump(issue.PreviewId);
                        }

                        break;
                    case PreviewKind.Animation:
                        int hue = 0;
                        AnimationFrame[] frames = Animations.GetAnimation(issue.PreviewId, 0, 1, ref hue, false, true);
                        bmp = frames != null && frames.Length > 0 ? frames[0]?.Bitmap : null;
                        break;
                }
            }
            catch (Exception)
            {
                bmp = null;
            }

            if (bmp == null)
            {
                return;
            }

            // own copy (the SDK caches its bitmaps), scaled down if needed
            int maxW = Math.Max(16, _preview.Width - 8), maxH = Math.Max(16, _preview.Height - 8);
            float scale = Math.Min(1f, Math.Min(maxW / (float)bmp.Width, maxH / (float)bmp.Height));
            var copy = new Bitmap(Math.Max(1, (int)(bmp.Width * scale)), Math.Max(1, (int)(bmp.Height * scale)));
            using (Graphics g = Graphics.FromImage(copy))
            {
                g.InterpolationMode = scale < 1f ? System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic : System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(bmp, 0, 0, copy.Width, copy.Height);
            }

            _preview.Image = copy;
        }

        private void UpdateButtons()
        {
            CheckIssue i = SelectedIssue;
            _btnFix.Enabled = SelectedIssues.Any(CheckFixes.CanFix);
            _btnCopyId.Enabled = i != null;
            _btnCopyLine.Enabled = i != null;
            _btnFixAll.Enabled = !_running && _view.Any(CheckFixes.CanFix);
            _btnSaveRadar.Enabled = CheckFixes.RadarChanged;
            _btnSaveRadar.Font = CheckFixes.RadarChanged ? _boldFont : Font;
            _btnSaveRadar.ForeColor = CheckFixes.RadarChanged ? Color.DarkOrange : SystemColors.ControlText;
            _ddExport.Enabled = _issues.Count > 0;
        }

        #endregion

        #region Fix / export / copy

        private void FixSelected()
        {
            List<CheckIssue> list = SelectedIssues.Where(CheckFixes.CanFix).ToList();
            if (list.Count == 0)
            {
                MessageBox.Show(this, Loc.T("msg.nofix"), Loc.T("tab.title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            RunFixes(list, list.Count > 1);
        }

        private void FixAll()
        {
            List<CheckIssue> list = _view.Where(CheckFixes.CanFix).ToList();
            if (list.Count == 0)
            {
                return;
            }

            RunFixes(list, true);
        }

        private void RunFixes(List<CheckIssue> list, bool confirm)
        {
            if (confirm)
            {
                string kinds = string.Join("\n", list.Select(i => i.Fix).Distinct().Select(k => "• " + CheckFixes.FixDescription(k)));
                if (MessageBox.Show(this, Loc.F("msg.fixconfirm", list.Count, kinds), Loc.T("tab.title"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }

            int done = 0;
            string lastError = null;
            Cursor = Cursors.WaitCursor;
            try
            {
                foreach (CheckIssue issue in list)
                {
                    if (CheckFixes.Apply(issue, out string error))
                    {
                        issue.Fixed = true;
                        done++;
                    }
                    else
                    {
                        lastError = error;
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            _status.Text = Loc.F("status.fixed", done) + (lastError != null ? $" – {lastError}" : string.Empty);
            if (_btnHideFixed.Checked)
            {
                ApplyFilter();
            }
            else
            {
                _list.Invalidate();
                UpdateSummary();
                ShowDetails();
            }

            if (CheckFixes.RadarChanged && list.Count > 1 &&
                MessageBox.Show(this, Loc.T("msg.radarunsaved"), Loc.T("tab.title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SaveRadar();
            }
        }

        private void SaveRadar()
        {
            try
            {
                string file = CheckFixes.SaveRadarCol();
                MessageBox.Show(this, Loc.F("msg.radarsaved", file), Loc.T("tab.title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Loc.T("tab.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            UpdateButtons();
        }

        private void Export(string kind)
        {
            string filter = kind switch
            {
                "csv" => "CSV (*.csv)|*.csv",
                "txt" => "Text (*.txt)|*.txt",
                _ => "HTML (*.html)|*.html"
            };

            using (var dlg = new SaveFileDialog { Filter = filter, FileName = $"ClientCheck_{DateTime.Now:yyyyMMdd_HHmm}.{kind}" })
            {
                if (!string.IsNullOrEmpty(Options.OutputPath) && Directory.Exists(Options.OutputPath))
                {
                    dlg.InitialDirectory = Options.OutputPath;
                }

                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                // the shown (filtered) entries are exported
                List<CheckIssue> list = _view.Count > 0 ? _view : _issues;
                try
                {
                    switch (kind)
                    {
                        case "csv":
                            CheckReport.SaveCsv(list, dlg.FileName);
                            break;
                        case "txt":
                            CheckReport.SaveText(list, dlg.FileName);
                            break;
                        default:
                            CheckReport.SaveHtml(list, dlg.FileName);
                            break;
                    }

                    if (MessageBox.Show(this, Loc.F("msg.exported", dlg.FileName), Loc.T("tab.title"), MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Information) == DialogResult.OK && kind == "html")
                    {
                        Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Loc.T("tab.title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CopySelectedLines()
        {
            List<CheckIssue> list = SelectedIssues;
            if (list.Count == 0)
            {
                return;
            }

            CopyText(string.Join(Environment.NewLine, list.Select(i => $"[{i.SeverityText}] {i.CategoryText} {i.IdText}: {i.Message}")));
        }

        private static void CopyText(string text)
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
