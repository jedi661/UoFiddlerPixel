/***************************************************************************
 *
 *  * Dragon Map Studio – help window (F1): guide and command reference in German and English,
 *  * topics on the left, search, opens at the topic of the tool / view, export as HTML
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class HelpWindow : Form
    {
        private readonly TreeView _tree;
        private readonly RichTextBox _text;
        private readonly TextBox _search;
        private readonly RadioButton _de;
        private readonly RadioButton _en;
        private readonly List<GuideTopic> _topics;
        private GuideTopic _current;

        /// <summary>Language changed (to remember it).</summary>
        public event EventHandler<bool> LanguageChanged;

        public HelpWindow(bool english)
        {
            Text = "Dragon Map Studio – Hilfe / Help";
            Size = new Size(1000, 720);
            MinimumSize = new Size(640, 400);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            KeyPreview = true;
            Font = new Font("Segoe UI", 9f);

            _topics = new List<GuideTopic>(StudioGuide.Topics)
            {
                new GuideTopic
                {
                    Key = "classic", Group = "Ausführlich|Complete", TitleDe = "Ausführliche Anleitung (alle Reiter)", TitleEn = "Complete manual (all tabs)",
                    De = Classic(StudioHelp.TextDe), En = Classic(StudioHelp.TextEn)
                }
            };

            var top = new Panel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(6, 6, 6, 4), BackColor = Color.FromArgb(51, 65, 85) };
            _search = new TextBox { Width = 260, Location = new Point(8, 7), PlaceholderText = "Suchen / search …" };
            _search.TextChanged += (s, e) => FillTree();
            _de = new RadioButton { Text = "Deutsch", ForeColor = Color.White, AutoSize = true, Location = new Point(290, 9), Checked = !english };
            _en = new RadioButton { Text = "English", ForeColor = Color.White, AutoSize = true, Location = new Point(370, 9), Checked = english };
            _de.CheckedChanged += (s, e) => LanguageSwitched();
            var export = new Button { Text = "HTML …", Location = new Point(460, 5), Width = 80, FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            export.Click += (s, e) => ExportHtml();
            top.Controls.AddRange(new Control[] { _search, _de, _en, export });

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 260, FixedPanel = FixedPanel.Panel1 };
            _tree = new TreeView { Dock = DockStyle.Fill, HideSelection = false, FullRowSelect = true, ShowLines = false, ItemHeight = 22, BorderStyle = BorderStyle.None };
            _tree.AfterSelect += (s, e) =>
            {
                if (e.Node?.Tag is GuideTopic t)
                {
                    Show(t);
                }
            };
            _text = new RichTextBox
            {
                Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Color.White,
                Font = new Font("Segoe UI", 10f), DetectUrls = false, Padding = new Padding(10)
            };
            split.Panel1.Controls.Add(_tree);
            split.Panel2.Padding = new Padding(10, 6, 6, 6);
            split.Panel2.BackColor = Color.White;
            split.Panel2.Controls.Add(_text);
            Controls.Add(split);
            Controls.Add(top);
            FillTree();
        }

        private bool English => _en.Checked;

        private static string Classic(string text)
        {
            // the old manual is plain text: keep it, only the underlined headings become headings
            var sb = new StringBuilder();
            string[] lines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i];
                if (i + 1 < lines.Length && lines[i + 1].Length > 2 && lines[i + 1].Trim().All(c => c == '-' || c == '='))
                {
                    sb.Append(lines[i + 1].Trim()[0] == '=' ? "# " : "## ").Append(l).Append('\n');
                    i++;
                    continue;
                }
                sb.Append(l.StartsWith("* ", StringComparison.Ordinal) ? "- " + l.Substring(2) : l.Replace("|", "¦")).Append('\n');
            }
            return sb.ToString();
        }

        private void LanguageSwitched()
        {
            Text = English ? "Dragon Map Studio – Help" : "Dragon Map Studio – Hilfe";
            FillTree();
            LanguageChanged?.Invoke(this, English);
        }

        /// <summary>Opens the topic with this key (editor, cgi, coast, heights …).</summary>
        public void ShowTopic(string key)
        {
            GuideTopic t = _topics.FirstOrDefault(x => x.Key == key) ?? _topics[0];
            if (_search.Text.Length > 0)
            {
                _search.Text = string.Empty;
            }
            foreach (TreeNode g in _tree.Nodes)
            {
                foreach (TreeNode n in g.Nodes)
                {
                    if (n.Tag == t)
                    {
                        _tree.SelectedNode = n;
                        return;
                    }
                }
            }
        }

        private void FillTree()
        {
            string q = _search.Text.Trim();
            bool en = English;
            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            TreeNode select = null;
            foreach (IGrouping<string, GuideTopic> g in _topics.GroupBy(t => t.Group))
            {
                string[] names = g.Key.Split('|');
                var head = new TreeNode(en && names.Length > 1 ? names[1] : names[0]) { NodeFont = new Font("Segoe UI", 9f, FontStyle.Bold) };
                foreach (GuideTopic t in g)
                {
                    if (q.Length > 0 && t.Text(en).IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0 && t.Title(en).IndexOf(q, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }
                    var n = new TreeNode(t.Title(en)) { Tag = t };
                    head.Nodes.Add(n);
                    if (t == _current || select == null)
                    {
                        select = t == _current || select == null ? n : select;
                    }
                }
                if (head.Nodes.Count > 0)
                {
                    _tree.Nodes.Add(head);
                }
            }
            _tree.ExpandAll();
            _tree.EndUpdate();
            if (select != null)
            {
                _tree.SelectedNode = select;
                if (select.Tag is GuideTopic t)
                {
                    Show(t);
                }
            }
            else
            {
                _text.Clear();
                _text.AppendText(en ? "Nothing found." : "Nichts gefunden.");
            }
        }

        private void Show(GuideTopic t)
        {
            _current = t;
            bool en = English;
            _text.SuspendLayout();
            _text.Clear();
            using var h1 = new Font("Segoe UI", 15f, FontStyle.Bold);
            using var h2 = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            using var body = new Font("Segoe UI", 10f);
            using var keyFont = new Font("Consolas", 9.5f, FontStyle.Bold);
            Color accent = Color.FromArgb(37, 99, 235);
            void Add(string s, Font f, Color c, int indent = 0)
            {
                _text.SelectionStart = _text.TextLength;
                _text.SelectionFont = f;
                _text.SelectionColor = c;
                _text.SelectionIndent = indent;
                _text.SelectionHangingIndent = 0;
                _text.AppendText(s);
            }
            foreach (string raw in t.Text(en).Replace("\r\n", "\n").Split('\n'))
            {
                string line = raw.TrimEnd();
                if (line.StartsWith("# ", StringComparison.Ordinal))
                {
                    Add(line.Substring(2) + "\n", h1, Color.FromArgb(15, 23, 42));
                }
                else if (line.StartsWith("## ", StringComparison.Ordinal))
                {
                    Add("\n" + line.Substring(3) + "\n", h2, accent);
                }
                else if (line.StartsWith("- ", StringComparison.Ordinal))
                {
                    Add("•  " + line.Substring(2) + "\n", body, Color.FromArgb(30, 41, 59), 12);
                }
                else if (line.Contains(" | "))
                {
                    int bar = line.IndexOf(" | ", StringComparison.Ordinal);
                    Add(line.Substring(0, bar).PadRight(18) + "  ", keyFont, Color.FromArgb(180, 83, 9), 12);
                    Add(line.Substring(bar + 3) + "\n", body, Color.FromArgb(30, 41, 59));
                }
                else
                {
                    Add(line + "\n", body, Color.FromArgb(30, 41, 59));
                }
            }
            Highlight(_search.Text.Trim());
            _text.SelectionStart = 0;
            _text.ScrollToCaret();
            _text.ResumeLayout();
        }

        private void Highlight(string q)
        {
            if (q.Length < 2)
            {
                return;
            }
            int at = 0;
            while ((at = _text.Find(q, at, RichTextBoxFinds.None)) >= 0)
            {
                _text.SelectionBackColor = Color.FromArgb(254, 240, 138);
                at += q.Length;
                if (at >= _text.TextLength)
                {
                    break;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                _search.Focus();
                _search.SelectAll();
            }
        }

        // ------------------------------------------------------------------ HTML

        /// <summary>All topics of the current language as one HTML page (to print or keep).</summary>
        public static string ToHtml(IEnumerable<GuideTopic> topics, bool en)
        {
            var sb = new StringBuilder();
            sb.Append("<!doctype html><html><head><meta charset=\"utf-8\"><title>Dragon Map Studio – ").Append(en ? "Help" : "Hilfe").Append("</title>");
            sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;max-width:960px;margin:24px auto;padding:0 16px;color:#1e293b;line-height:1.45}" +
                      "h1{border-bottom:2px solid #2563eb;padding-bottom:4px;margin-top:40px}h2{color:#2563eb}" +
                      "table{border-collapse:collapse;margin:6px 0}td{padding:3px 10px;border-bottom:1px solid #e2e8f0;vertical-align:top}" +
                      "td.k{font-family:Consolas,monospace;font-weight:bold;color:#b45309;white-space:nowrap}nav a{display:block}</style></head><body>");
            sb.Append("<nav>");
            foreach (GuideTopic t in topics)
            {
                sb.Append("<a href=\"#").Append(t.Key).Append("\">").Append(WebUtility.HtmlEncode(t.Title(en))).Append("</a>");
            }
            sb.Append("</nav>");
            foreach (GuideTopic t in topics)
            {
                sb.Append("<section id=\"").Append(t.Key).Append("\">");
                bool inTable = false, inList = false;
                void Close()
                {
                    if (inTable)
                    {
                        sb.Append("</table>");
                        inTable = false;
                    }
                    if (inList)
                    {
                        sb.Append("</ul>");
                        inList = false;
                    }
                }
                foreach (string raw in t.Text(en).Replace("\r\n", "\n").Split('\n'))
                {
                    string line = raw.TrimEnd();
                    if (line.Contains(" | ") && !line.StartsWith("#", StringComparison.Ordinal))
                    {
                        if (!inTable)
                        {
                            Close();
                            sb.Append("<table>");
                            inTable = true;
                        }
                        int bar = line.IndexOf(" | ", StringComparison.Ordinal);
                        sb.Append("<tr><td class=\"k\">").Append(WebUtility.HtmlEncode(line.Substring(0, bar))).Append("</td><td>")
                          .Append(WebUtility.HtmlEncode(line.Substring(bar + 3))).Append("</td></tr>");
                        continue;
                    }
                    if (line.StartsWith("- ", StringComparison.Ordinal))
                    {
                        if (!inList)
                        {
                            Close();
                            sb.Append("<ul>");
                            inList = true;
                        }
                        sb.Append("<li>").Append(WebUtility.HtmlEncode(line.Substring(2))).Append("</li>");
                        continue;
                    }
                    Close();
                    if (line.StartsWith("# ", StringComparison.Ordinal))
                    {
                        sb.Append("<h1>").Append(WebUtility.HtmlEncode(line.Substring(2))).Append("</h1>");
                    }
                    else if (line.StartsWith("## ", StringComparison.Ordinal))
                    {
                        sb.Append("<h2>").Append(WebUtility.HtmlEncode(line.Substring(3))).Append("</h2>");
                    }
                    else if (line.Length > 0)
                    {
                        sb.Append("<p>").Append(WebUtility.HtmlEncode(line)).Append("</p>");
                    }
                }
                Close();
                sb.Append("</section>");
            }
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private void ExportHtml()
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "HTML (*.html)|*.html",
                FileName = English ? "DragonMapStudio_Help.html" : "DragonMapStudio_Hilfe.html"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            File.WriteAllText(dlg.FileName, ToHtml(_topics, English), Encoding.UTF8);
            try
            {
                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception)
            {
                // the file is written, opening is only a convenience
            }
        }
    }
}
