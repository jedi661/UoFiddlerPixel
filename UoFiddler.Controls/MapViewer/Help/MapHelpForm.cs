/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MapViewer.Help
{
    /// <summary>
    /// Help window of the map tools: topics on the left, text on the right, search.
    /// Only one window is open; <see cref="ShowTopic"/> brings it to the front.
    /// </summary>
    public class MapHelpForm : Form
    {
        private static MapHelpForm _instance;

        private readonly TreeView _tree = new TreeView();
        private readonly RichTextBox _text = new RichTextBox();
        private readonly TextBox _search = new TextBox();

        private MapHelpForm()
        {
            Text = "Help - map tools";
            Icon = Options.GetFiddlerIcon();
            ClientSize = new Size(1000, 680);
            MinimumSize = new Size(600, 400);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            _search.Dock = DockStyle.Top;
            _search.PlaceholderText = "Search...";
            _search.TextChanged += (s, e) => FillTree();

            _tree.Dock = DockStyle.Fill;
            _tree.HideSelection = false;
            _tree.FullRowSelect = true;
            _tree.Font = new Font(Font.FontFamily, 10f);
            _tree.AfterSelect += (s, e) => ShowText(e.Node?.Tag as HelpTopic);

            var left = new Panel { Dock = DockStyle.Left, Width = 260 };
            left.Controls.Add(_tree);
            left.Controls.Add(_search);

            _text.Dock = DockStyle.Fill;
            _text.ReadOnly = true;
            _text.BorderStyle = BorderStyle.None;
            _text.BackColor = SystemColors.Window;
            _text.Font = new Font("Segoe UI", 10f);
            _text.DetectUrls = false;

            var textPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14, 8, 8, 8), BackColor = SystemColors.Window };
            textPanel.Controls.Add(_text);

            Controls.Add(textPanel);
            Controls.Add(new Splitter { Dock = DockStyle.Left, Width = 4 });
            Controls.Add(left);

            FillTree();
        }

        /// <summary>Opens the help (or brings it to the front) and shows a topic</summary>
        public static void ShowTopic(IWin32Window owner, string key)
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new MapHelpForm();
                _instance.Show();
            }
            else
            {
                if (_instance.WindowState == FormWindowState.Minimized)
                {
                    _instance.WindowState = FormWindowState.Normal;
                }

                _instance.Activate();
            }

            _instance.Select(key ?? MapHelpContent.Overview);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            if (keyData == (Keys.Control | Keys.F))
            {
                _search.Focus();
                _search.SelectAll();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FillTree()
        {
            string filter = _search.Text.Trim();
            string selected = (_tree.SelectedNode?.Tag as HelpTopic)?.Key;
            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            foreach (HelpTopic topic in MapHelpContent.Topics)
            {
                TreeNode node = CreateNode(topic, filter);
                if (node != null)
                {
                    _tree.Nodes.Add(node);
                }
            }

            _tree.ExpandAll();
            _tree.EndUpdate();

            if (selected != null && FindNode(_tree.Nodes, selected) is TreeNode again)
            {
                _tree.SelectedNode = again;
            }
            else if (_tree.Nodes.Count > 0)
            {
                _tree.SelectedNode = FirstMatch(_tree.Nodes, filter) ?? _tree.Nodes[0];
            }
            else
            {
                _text.Clear();
                _text.AppendText("Nothing found.");
            }
        }

        private static TreeNode CreateNode(HelpTopic topic, string filter)
        {
            var children = topic.Children.Select(c => CreateNode(c, filter)).Where(n => n != null).ToList();
            bool match = filter.Length == 0 || Matches(topic, filter);
            if (!match && children.Count == 0)
            {
                return null;
            }

            var node = new TreeNode(topic.Title) { Tag = topic };
            if (filter.Length > 0 && match)
            {
                node.NodeFont = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
            }

            node.Nodes.AddRange(children.ToArray());
            return node;
        }

        private static bool Matches(HelpTopic t, string filter)
        {
            return t.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   t.Text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static TreeNode FirstMatch(TreeNodeCollection nodes, string filter)
        {
            foreach (TreeNode n in nodes)
            {
                if (filter.Length == 0 || Matches((HelpTopic)n.Tag, filter))
                {
                    return n;
                }

                TreeNode child = FirstMatch(n.Nodes, filter);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        private static TreeNode FindNode(TreeNodeCollection nodes, string key)
        {
            foreach (TreeNode n in nodes)
            {
                if (((HelpTopic)n.Tag).Key == key)
                {
                    return n;
                }

                TreeNode child = FindNode(n.Nodes, key);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        private new void Select(string key)
        {
            if (FindNode(_tree.Nodes, key) == null && _search.Text.Length > 0)
            {
                _search.Text = string.Empty;
            }

            TreeNode node = FindNode(_tree.Nodes, key);
            if (node != null)
            {
                _tree.SelectedNode = node;
                node.EnsureVisible();
            }
        }

        /// <summary>Simple formatting: "# " heading, "## " sub heading, "- " list, "> " hint</summary>
        private void ShowText(HelpTopic topic)
        {
            _text.Clear();
            if (topic == null)
            {
                return;
            }

            Font normal = _text.Font;
            using (var h1 = new Font(normal.FontFamily, 15f, FontStyle.Bold))
            using (var h2 = new Font(normal.FontFamily, 11.5f, FontStyle.Bold))
            using (var hint = new Font(normal.FontFamily, normal.Size, FontStyle.Italic))
            {
                string filter = _search.Text.Trim();
                IEnumerable<string> lines = topic.Text.Replace("\r\n", "\n").Trim('\n').Split('\n');
                foreach (string raw in lines)
                {
                    string line = raw.TrimEnd();
                    if (line.StartsWith("## "))
                    {
                        Append("\n" + line.Substring(3) + "\n", h2, Color.SteelBlue, 0);
                    }
                    else if (line.StartsWith("# "))
                    {
                        Append(line.Substring(2) + "\n", h1, Color.FromArgb(30, 60, 110), 0);
                    }
                    else if (line.TrimStart().StartsWith("- "))
                    {
                        Append("•  " + line.TrimStart().Substring(2) + "\n", normal, SystemColors.WindowText, 14);
                    }
                    else if (line.TrimStart().StartsWith("> "))
                    {
                        Append(line.TrimStart().Substring(2) + "\n", hint, Color.DarkGoldenrod, 14);
                    }
                    else if (line.StartsWith("  "))
                    {
                        // continuation of a list entry
                        Append(line.Trim() + "\n", normal, SystemColors.WindowText, 30);
                    }
                    else
                    {
                        Append(line + "\n", normal, SystemColors.WindowText, 0);
                    }
                }

                if (topic.Children.Count > 0)
                {
                    Append("\nUnterseiten: " + string.Join(", ", topic.Children.Select(c => c.Title)) + "\n", hint, SystemColors.GrayText, 0);
                }

                Highlight(filter);
            }

            _text.SelectionStart = 0;
            _text.ScrollToCaret();
        }

        private void Append(string text, Font font, Color color, int indent)
        {
            _text.SelectionStart = _text.TextLength;
            _text.SelectionLength = 0;
            _text.SelectionFont = font;
            _text.SelectionColor = color;
            _text.SelectionIndent = indent;
            _text.SelectionHangingIndent = indent > 0 ? 14 : 0;
            _text.AppendText(text);
        }

        private void Highlight(string filter)
        {
            if (filter.Length < 2)
            {
                return;
            }

            int start = 0;
            while ((start = _text.Find(filter, start, RichTextBoxFinds.None)) >= 0)
            {
                _text.SelectionBackColor = Color.Khaki;
                start += filter.Length;
                if (start >= _text.TextLength)
                {
                    break;
                }
            }
        }
    }
}
