/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Help of the client check. The texts are the files Help\ClientCheck_Help.<lang>.md
 * (embedded in the plugin and copied to plugins\ClientCheckHelp\).
 *
 ***************************************************************************/

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.ClientCheck.Classes;

namespace UoFiddler.Plugin.ClientCheck.Forms
{
    public class ClientCheckHelpForm : Form
    {
        private static ClientCheckHelpForm _instance;

        private readonly RichTextBox _text;
        private readonly ComboBox _cmbLang;
        private readonly Button _btnFolder;
        private readonly Button _btnClose;

        public static void ShowHelp(IWin32Window owner)
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new ClientCheckHelpForm();
                _instance.Show(owner);
            }
            else
            {
                _instance.WindowState = FormWindowState.Normal;
                _instance.Activate();
            }
        }

        public ClientCheckHelpForm()
        {
            Icon = Options.GetFiddlerIcon();
            Size = new Size(820, 720);
            MinimumSize = new Size(500, 400);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9.5f);

            _text = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                DetectUrls = false,
                Padding = new Padding(10)
            };

            var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
            _btnClose = new Button { AutoSize = true };
            _btnClose.Click += (s, e) => Close();
            _btnFolder = new Button { AutoSize = true, Text = "📁" };
            _btnFolder.Click += (s, e) => OpenFolder();
            _cmbLang = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
            _cmbLang.Items.AddRange(Loc.LanguageNames.Cast<object>().ToArray());
            _cmbLang.SelectedIndex = Math.Max(0, Array.IndexOf(Loc.Languages, Loc.Language));
            _cmbLang.SelectedIndexChanged += (s, e) => Loc.Language = Loc.Languages[Math.Max(0, _cmbLang.SelectedIndex)];
            bar.Controls.AddRange(new Control[] { _btnClose, _btnFolder, _cmbLang });

            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14, 10, 8, 4), BackColor = Color.White };
            host.Controls.Add(_text);
            Controls.Add(host);
            Controls.Add(bar);

            Loc.LanguageChanged += OnLanguageChanged;
            FormClosed += (s, e) => Loc.LanguageChanged -= OnLanguageChanged;
            LoadText();
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(Loc.Languages, Loc.Language);
            if (_cmbLang.SelectedIndex != index)
            {
                _cmbLang.SelectedIndex = index;
            }

            LoadText();
        }

        /// <summary>Help text of the current language (embedded file)</summary>
        public static string ReadHelp(string language)
        {
            Assembly asm = typeof(ClientCheckHelpForm).Assembly;
            using (Stream stream = asm.GetManifestResourceStream($"ClientCheck.Help.{language}.md") ?? asm.GetManifestResourceStream("ClientCheck.Help.en.md"))
            {
                if (stream == null)
                {
                    return "# Help\n\n(help file not found)";
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private void LoadText()
        {
            Text = $"{Loc.T("tab.title")} – {Loc.T("btn.help").TrimStart('?', ' ')}";
            _btnClose.Text = "OK";
            RenderMarkdown(ReadHelp(Loc.Language));
        }

        /// <summary>Very small markdown renderer: headings, lists, **bold**, `code`, tables as text</summary>
        private void RenderMarkdown(string markdown)
        {
            _text.Clear();
            var normal = new Font("Segoe UI", 10f);
            var bold = new Font("Segoe UI", 10f, FontStyle.Bold);
            var h1 = new Font("Segoe UI", 16f, FontStyle.Bold);
            var h2 = new Font("Segoe UI", 13f, FontStyle.Bold);
            var h3 = new Font("Segoe UI", 11f, FontStyle.Bold);
            var code = new Font("Consolas", 9.5f);

            foreach (string raw in markdown.Replace("\r\n", "\n").Split('\n'))
            {
                string line = raw.TrimEnd();
                if (line.StartsWith("# "))
                {
                    Append(line.Substring(2) + "\n", h1, Color.FromArgb(20, 60, 120));
                    continue;
                }

                if (line.StartsWith("## "))
                {
                    Append("\n" + line.Substring(3) + "\n", h2, Color.FromArgb(20, 60, 120));
                    continue;
                }

                if (line.StartsWith("### "))
                {
                    Append(line.Substring(4) + "\n", h3, Color.FromArgb(40, 40, 40));
                    continue;
                }

                if (line.StartsWith("|"))
                {
                    if (line.Replace("|", string.Empty).Replace("-", string.Empty).Replace(":", string.Empty).Trim().Length == 0)
                    {
                        continue; // table separator line
                    }

                    string[] cells = line.Trim('|').Split('|').Select(c => c.Trim()).ToArray();
                    AppendInline("   " + string.Join("   │   ", cells) + "\n", normal, bold, code);
                    continue;
                }

                if (line.StartsWith("```"))
                {
                    continue;
                }

                string text = line;
                if (text.StartsWith("- ") || text.StartsWith("* "))
                {
                    text = "   •  " + text.Substring(2);
                }
                else if (text.StartsWith("  - "))
                {
                    text = "         ◦  " + text.Substring(4);
                }

                AppendInline(text + "\n", normal, bold, code);
            }

            _text.SelectionStart = 0;
            _text.ScrollToCaret();
        }

        private void Append(string text, Font font, Color color)
        {
            _text.SelectionStart = _text.TextLength;
            _text.SelectionLength = 0;
            _text.SelectionFont = font;
            _text.SelectionColor = color;
            _text.AppendText(text);
        }

        /// <summary>**bold** and `code` inside a line</summary>
        private void AppendInline(string text, Font normal, Font bold, Font code)
        {
            int pos = 0;
            while (pos < text.Length)
            {
                int b = text.IndexOf("**", pos, StringComparison.Ordinal);
                int c = text.IndexOf('`', pos);
                int next = b < 0 ? c : c < 0 ? b : Math.Min(b, c);
                if (next < 0)
                {
                    Append(text.Substring(pos), normal, Color.Black);
                    return;
                }

                Append(text.Substring(pos, next - pos), normal, Color.Black);
                bool isBold = next == b;
                string marker = isBold ? "**" : "`";
                int end = text.IndexOf(marker, next + marker.Length, StringComparison.Ordinal);
                if (end < 0)
                {
                    Append(text.Substring(next), normal, Color.Black);
                    return;
                }

                string inner = text.Substring(next + marker.Length, end - next - marker.Length);
                Append(inner, isBold ? bold : code, isBold ? Color.Black : Color.FromArgb(150, 40, 40));
                pos = end + marker.Length;
            }
        }

        private static void OpenFolder()
        {
            string dir = Path.Combine(Path.GetDirectoryName(typeof(ClientCheckHelpForm).Assembly.Location) ?? string.Empty, "ClientCheckHelp");
            if (Directory.Exists(dir))
            {
                Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true });
            }
        }
    }
}
