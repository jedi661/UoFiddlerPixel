/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Region editor – tab "Check" (validator, automatic fixes) and tab "History".
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.RegionEditor.Classes;

namespace UoFiddler.Plugin.RegionEditor.UserControls
{
    public partial class RegionEditorControl
    {
        private List<RegionIssue> _issues = new List<RegionIssue>();

        #region Check

        private void OnTabChanged(object sender, EventArgs e)
        {
            if (_tabs.SelectedTab == _tabCheck)
            {
                RunCheck();
            }
            else if (_tabs.SelectedTab == _tabHistory)
            {
                FillHistory();
            }
        }

        private void OnClickRunCheck(object sender, EventArgs e) => ShowIssues();

        private void ShowIssues()
        {
            if (_tabs.SelectedTab != _tabCheck)
            {
                _tabs.SelectedTab = _tabCheck; // runs the check
            }
            else
            {
                RunCheck();
            }
        }

        private void RunCheck()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                _issues = Validator.Check(_project, _mapInfo);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            ThemePalette p = ThemeManager.Palette;
            _listIssues.BeginUpdate();
            _listIssues.Items.Clear();
            foreach (RegionIssue issue in _issues)
            {
                string sev = issue.Severity == IssueSeverity.Error ? "✖" : issue.Severity == IssueSeverity.Warning ? "⚠" : "ℹ";
                string where = issue.MapId == _mapView.MapId ? issue.RegionName : $"{issue.RegionName} ({FacetName(issue.MapId)})";
                var item = new ListViewItem(new[] { sev, where, issue.Message + (issue.Fix != null ? "  🔧" : string.Empty) })
                {
                    Tag = issue,
                    ToolTipText = issue.Message,
                    ForeColor = issue.Severity == IssueSeverity.Error ? p.Error : issue.Severity == IssueSeverity.Warning ? p.Warning : p.SubText
                };
                _listIssues.Items.Add(item);
            }

            _listIssues.EndUpdate();
            int errors = _issues.Count(i => i.Severity == IssueSeverity.Error);
            int warnings = _issues.Count(i => i.Severity == IssueSeverity.Warning);
            _lblCheckSummary.Text = _issues.Count == 0 ? Loc.T("check.ok") : Loc.F("check.summary", errors, warnings, _issues.Count - errors - warnings);
            _btnFixAll.Enabled = _issues.Any(i => i.Fix != null);
            _btnFixSelected.Enabled = false;
            _tabCheck.Text = _issues.Count == 0 ? Loc.T("tab.check") : $"{Loc.T("tab.check")} ({errors + warnings})";
        }

        private string FacetName(int mapId) => _project.Facets.FirstOrDefault(f => f.MapId == mapId)?.Name ?? mapId.ToString();

        private void OnIssueSelected(object sender, EventArgs e)
        {
            _btnFixSelected.Enabled = _listIssues.SelectedItems.Cast<ListViewItem>().Any(i => ((RegionIssue)i.Tag).Fix != null);
        }

        private void OnIssueActivated(object sender, EventArgs e)
        {
            if (_listIssues.SelectedItems.Count == 0 || !(_listIssues.SelectedItems[0].Tag is RegionIssue issue))
            {
                return;
            }

            SwitchToMap(issue.MapId);
            RegionNode region = issue.RegionId == null ? null : _project.FindById(issue.RegionId, out FacetRegions _, out RegionNode _);
            if (region != null)
            {
                SelectRegion(region, -1, false);
            }

            if (issue.Location.HasValue)
            {
                if (region != null && !region.Bounds.IsEmpty && region.Bounds.Width < 2000)
                {
                    _mapView.ZoomTo(region.Bounds);
                }
                else
                {
                    _mapView.CenterOn(issue.Location.Value.X, issue.Location.Value.Y);
                }
            }
        }

        private void OnClickFixSelected(object sender, EventArgs e)
        {
            List<RegionIssue> selected = _listIssues.SelectedItems.Cast<ListViewItem>().Select(i => (RegionIssue)i.Tag).Where(i => i.Fix != null).ToList();
            Fix(selected);
        }

        private void OnClickFixAll(object sender, EventArgs e) => Fix(_issues.Where(i => i.Fix != null).ToList());

        private void Fix(List<RegionIssue> issues)
        {
            if (issues.Count == 0)
            {
                return;
            }

            int count = 0;
            Change(Loc.F("hist.fix", issues.Count), () => count = Validator.FixAll(issues));
            SetMessage(Loc.F("msg.fixed", count));
            RunCheck();
        }

        #endregion

        #region History

        private void FillHistory()
        {
            if (_tabs.SelectedTab != _tabHistory)
            {
                return;
            }

            _listHistory.BeginUpdate();
            _listHistory.Items.Clear();
            foreach (HistoryEntry entry in _history.UndoEntries)
            {
                _listHistory.Items.Add(entry);
            }

            _listHistory.Items.Add("● " + Loc.T("hist.current"));
            foreach (HistoryEntry entry in _history.RedoEntries.Reverse())
            {
                _listHistory.Items.Add("   ↷ " + entry);
            }

            _listHistory.SelectedIndex = _history.UndoEntries.Count;
            _listHistory.EndUpdate();
        }

        /// <summary>undo all steps after the selected entry (the selected step is undone too)</summary>
        private void OnClickHistoryJump(object sender, EventArgs e)
        {
            int index = _listHistory.SelectedIndex;
            if (index < 0 || index >= _history.UndoEntries.Count)
            {
                return;
            }

            int steps = _history.UndoEntries.Count - index;
            ReplaceProject(_history.UndoTo(_project, index));
            SetMessage(Loc.F("msg.undosteps", steps));
        }

        private void OnClickHistoryClear(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, Loc.T("ask.clearhistory"), Loc.T("tab.title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _history.Clear();
            }
        }

        #endregion
    }
}
