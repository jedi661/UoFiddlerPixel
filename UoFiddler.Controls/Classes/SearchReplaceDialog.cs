using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    // Standalone "Find and Replace" dialog.
    // Opened via `searchAndReplaceToolStripMenuItem` from EditorXML.
    // </summary>
    public class SearchReplaceDialog : Form
    {
        private readonly RichTextBox _target;
        private readonly Stack<string> _localUndo = new Stack<string>();

        // Controls
        private TextBox tbSearch;
        private TextBox tbReplace;
        private CheckBox chkCase;
        private CheckBox chkStep;
        private Label lblMatches;
        private Label lblReplaced;
        private Button btnFind;
        private Button btnReplace;
        private Button btnUndo;

        public SearchReplaceDialog(RichTextBox target)
        {
            _target = target;
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "Find and replace";
            Width = 480;
            Height = 320;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            int y = 15;

            // ── Search ──────────────────────────────────────────────────
            Controls.Add(new Label { Text = "Search:", Top = y, Left = 10, Width = 100 });
            tbSearch = new TextBox { Top = y, Left = 115, Width = 340 };
            Controls.Add(tbSearch);
            y += 35;

            // ── Replace ────────────────────────────────────────────────
            Controls.Add(new Label { Text = "Replace by:", Top = y, Left = 10, Width = 100 });
            tbReplace = new TextBox { Top = y, Left = 115, Width = 340 };
            Controls.Add(tbReplace);
            y += 35;

            // ── Optionen ────────────────────────────────────────────────
            chkCase = new CheckBox { Text = "Pay attention to capitalization", Top = y, Left = 115, Width = 230 };
            Controls.Add(chkCase);
            y += 28;

            chkStep = new CheckBox { Text = "Replace step by step", Top = y, Left = 115, Width = 230 };
            Controls.Add(chkStep);
            y += 35;

            // ── Status ──────────────────────────────────────────────────
            lblMatches = new Label { Text = "Hit: –", Top = y, Left = 10, Width = 200, ForeColor = Color.Gray };
            Controls.Add(lblMatches);
            y += 22;

            lblReplaced = new Label { Text = "Replace: –", Top = y, Left = 10, Width = 200, ForeColor = Color.Gray };
            Controls.Add(lblReplaced);
            y += 30;

            // ── Buttons ─────────────────────────────────────────────────
            btnFind = new Button { Text = "Hits count", Top = y, Left = 10, Width = 130, Height = 28 };
            btnFind.Click += BtnFind_Click;
            Controls.Add(btnFind);

            btnReplace = new Button { Text = "Replace", Top = y, Left = 155, Width = 130, Height = 28 };
            btnReplace.Click += BtnReplace_Click;
            Controls.Add(btnReplace);

            btnUndo = new Button { Text = "Undo", Top = y, Left = 300, Width = 130, Height = 28, Enabled = false };
            btnUndo.Click += BtnUndo_Click;
            Controls.Add(btnUndo);
        }

        private StringComparison Comparison =>
            chkCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        private void BtnFind_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbSearch.Text))
            {
                MessageBox.Show("Please enter search term.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int count = 0, idx = 0;
            while ((idx = _target.Text.IndexOf(tbSearch.Text, idx, Comparison)) != -1)
            {
                count++;
                idx += tbSearch.Text.Length;
            }
            lblMatches.Text = $"Hit: {count}";
            lblMatches.ForeColor = count > 0 ? Color.DarkGreen : Color.DarkRed;
        }

        private void BtnReplace_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbSearch.Text))
            {
                MessageBox.Show("Please enter search term.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _localUndo.Push(_target.Text);
            btnUndo.Enabled = true;

            string content = _target.Text;
            string search = tbSearch.Text;
            string replace = tbReplace.Text;
            int matches = 0, replaced = 0;

            if (chkStep.Checked)
            {
                int idx = 0;
                while ((idx = content.IndexOf(search, idx, Comparison)) != -1)
                {
                    matches++;
                    _target.Select(idx, search.Length);
                    _target.ScrollToCaret();

                    var result = MessageBox.Show(
                        $"Hit in position {idx + 1}. Replace?",
                        "Replace gradually",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        content = content.Remove(idx, search.Length).Insert(idx, replace);
                        replaced++;
                        idx += replace.Length;
                    }
                    else if (result == DialogResult.No)
                        idx += search.Length;
                    else
                        break;
                }
            }
            else
            {
                matches = Regex.Matches(content,
                    Regex.Escape(search),
                    chkCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase).Count;

                content = chkCase.Checked
                    ? content.Replace(search, replace)
                    : Regex.Replace(content, Regex.Escape(search), Regex.Escape(replace).Replace(@"\", "").Replace("$", "$$"), RegexOptions.IgnoreCase);

                replaced = matches;
            }

            _target.Text = content;
            lblMatches.Text = $"Hit: {matches}";
            lblReplaced.Text = $"Replace: {replaced}";
        }

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            if (_localUndo.Count > 0)
            {
                _target.Text = _localUndo.Pop();
                btnUndo.Enabled = _localUndo.Count > 0;
            }
        }
    }
}
