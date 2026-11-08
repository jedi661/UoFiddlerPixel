/***************************************************************************
 *
 * MultiEditor – result list of the walkability check
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Forms
{
    public sealed class CheckResultForm : Form
    {
        private readonly ListView _list;
        private readonly Action<WalkIssue> _goTo;

        public CheckResultForm(IList<WalkIssue> issues, string summary, Action<WalkIssue> goTo, Action rerun)
        {
            _goTo = goTo;
            Text = L.T("Begehbarkeits-Prüfung", "Walkability check");
            Size = new Size(680, 460);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            var info = new Label { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6), Text = summary };
            _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
            _list.Columns.Add(L.T("Art", "Kind"), 130);
            _list.Columns.Add("X", 40);
            _list.Columns.Add("Y", 40);
            _list.Columns.Add("Z", 40);
            _list.Columns.Add(L.T("Beschreibung", "Description"), 380);
            foreach (WalkIssue i in issues.OrderBy(i => i.Kind).ThenBy(i => i.Z).ThenBy(i => i.X + i.Y))
            {
                var item = new ListViewItem(i.KindName) { Tag = i, ForeColor = IssueColor(i.Kind) };
                item.SubItems.Add(i.X.ToString());
                item.SubItems.Add(i.Y.ToString());
                item.SubItems.Add(i.Z.ToString());
                item.SubItems.Add(i.Text);
                _list.Items.Add(item);
            }

            _list.SelectedIndexChanged += (_, _) =>
            {
                if (_list.SelectedItems.Count > 0)
                {
                    _goTo((WalkIssue)_list.SelectedItems[0].Tag);
                }
            };

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
            var again = new Button { Text = L.T("Erneut prüfen", "Check again"), AutoSize = true };
            again.Click += (_, _) =>
            {
                rerun();
                Close();
            };
            var close = new Button { Text = L.T("Schließen", "Close"), AutoSize = true };
            close.Click += (_, _) => Close();
            bottom.Controls.Add(again);
            bottom.Controls.Add(close);
            bottom.Controls.Add(new Label
            {
                AutoSize = true,
                Margin = new Padding(10, 8, 3, 3),
                ForeColor = SystemColors.GrayText,
                Text = L.T("Zeile anklicken = Stelle zeigen", "Click a row = show the place")
            });

            Controls.Add(_list);
            Controls.Add(bottom);
            Controls.Add(info);
        }

        public static Color IssueColor(WalkIssueKind kind)
        {
            return kind switch
            {
                WalkIssueKind.StepTooHigh => System.Drawing.Color.OrangeRed,
                WalkIssueKind.MissingFloor => System.Drawing.Color.Red,
                WalkIssueKind.LowCeiling => System.Drawing.Color.DarkOrange,
                _ => System.Drawing.Color.MediumPurple
            };
        }
    }
}
