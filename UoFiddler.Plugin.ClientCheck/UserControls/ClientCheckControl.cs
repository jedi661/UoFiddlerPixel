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
using System.ComponentModel;
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
    public partial class ClientCheckControl : UserControl
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
        private bool _loadingSettings;
        private int _sortColumn = -1;
        private bool _sortDescending;
        private readonly Font _boldFont = new Font("Segoe UI", 9f, FontStyle.Bold);
        private readonly Dictionary<CheckCategory, ToolStripMenuItem> _checkItems = new Dictionary<CheckCategory, ToolStripMenuItem>();

        public ClientCheckControl()
        {
            InitializeComponent();

            // the designer shows the German texts; at runtime the chosen language is used
            if (IsDesignTime())
            {
                return;
            }

            _checkItems[CheckCategory.ArtTileData] = checkArtTileDataMenuItem;
            _checkItems[CheckCategory.LandTiles] = checkLandTilesMenuItem;
            _checkItems[CheckCategory.Equipment] = checkEquipmentMenuItem;
            _checkItems[CheckCategory.Bodies] = checkBodiesMenuItem;
            _checkItems[CheckCategory.RadarCol] = checkRadarColMenuItem;
            _checkItems[CheckCategory.Multis] = checkMultisMenuItem;
            _checkItems[CheckCategory.Statics] = checkStaticsMenuItem;
            _checkItems[CheckCategory.Cliloc] = checkClilocMenuItem;

            _loadingSettings = true;
            CheckCategory saved = LoadSelectedCategories();
            foreach (KeyValuePair<CheckCategory, ToolStripMenuItem> pair in _checkItems)
            {
                pair.Value.Checked = (saved & pair.Key) != 0;
            }

            _cmbLang.SelectedIndex = Math.Max(0, Array.IndexOf(Loc.Languages, Loc.Language));
            _loadingSettings = false;

            // keep the menu open while ticking several checks
            _ddChecks.DropDown.Closing += OnChecksDropDownClosing;

            ApplyTexts();
            Loc.LanguageChanged += OnLanguageChanged;
        }

        private bool IsDesignTime() => LicenseManager.UsageMode == LicenseUsageMode.Designtime || (Site?.DesignMode ?? false);

        #region UI events

        private void OnClickStart(object sender, EventArgs e) => StartCheck();

        private void OnClickCancel(object sender, EventArgs e) => _cts?.Cancel();

        private void OnClickHelp(object sender, EventArgs e) => ClientCheckHelpForm.ShowHelp(FindForm());

        private void OnLanguageSelected(object sender, EventArgs e)
        {
            if (!_loadingSettings && _cmbLang.SelectedIndex >= 0)
            {
                Loc.Language = Loc.Languages[_cmbLang.SelectedIndex];
            }
        }

        private void OnCheckItemChanged(object sender, EventArgs e)
        {
            if (!_loadingSettings)
            {
                SaveSelectedCategories();
            }
        }

        private void OnClickCheckAll(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in _checkItems.Values)
            {
                item.Checked = true;
            }
        }

        private void OnClickCheckNone(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in _checkItems.Values)
            {
                item.Checked = false;
            }
        }

        private void OnChecksDropDownClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
            }
        }

        private void OnFilterChanged(object sender, EventArgs e) => ApplyFilter();

        private void OnClickFixAll(object sender, EventArgs e) => FixAll();

        private void OnClickSaveRadar(object sender, EventArgs e) => SaveRadar();

        private void OnClickExportHtml(object sender, EventArgs e) => Export("html");

        private void OnClickExportCsv(object sender, EventArgs e) => Export("csv");

        private void OnClickExportTxt(object sender, EventArgs e) => Export("txt");

        private void OnListSelectionChanged(object sender, EventArgs e) => ShowDetails();

        private void OnListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedLines();
                e.Handled = true;
            }
        }

        private void OnClickFix(object sender, EventArgs e) => FixSelected();

        private void OnClickCopyId(object sender, EventArgs e) => CopyText(SelectedIssue?.IdText);

        private void OnClickCopyLine(object sender, EventArgs e) => CopySelectedLines();

        private void OnDetailsResize(object sender, EventArgs e)
        {
            int w = Math.Max(150, detailsPanel.ClientSize.Width - 30);
            _detailMessage.MaximumSize = new Size(w, 0);
            _detailHint.MaximumSize = new Size(w, 0);
            _detailFix.MaximumSize = new Size(w, 0);
        }

        #endregion

        #region Texts

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

            columnSeverity.Text = Loc.T("col.severity");
            columnCategory.Text = Loc.T("col.category");
            columnId.Text = Loc.T("col.id");
            columnMessage.Text = Loc.T("col.message");

            _loadingSettings = true;
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
            _loadingSettings = false;

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
            if (_loadingSettings || _cmbSeverity.SelectedIndex < 0 || _cmbCategory.SelectedIndex < 0)
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
