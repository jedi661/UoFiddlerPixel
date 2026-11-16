/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Help of the TileData tab (Help\TileData_Help.<lang>.md, embedded in
 * UoFiddler.Controls), German / English / French.
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class TileDataHelpForm : Form
    {
        private static readonly string[] _languages = { "de", "en", "fr" };
        private static string _language;
        private static readonly System.Collections.Generic.Dictionary<string, TileDataHelpForm> _instances =
            new System.Collections.Generic.Dictionary<string, TileDataHelpForm>();

        private readonly string _topic = "TileData";
        private bool _loading;

        public static void ShowHelp(IWin32Window owner) => ShowHelp(owner, "TileData", "TileData – help");

        /// <summary>help of a topic: the embedded files "&lt;topic&gt;.Help.de/en/fr.md"</summary>
        public static void ShowHelp(IWin32Window owner, string topic, string title)
        {
            if (!_instances.TryGetValue(topic, out TileDataHelpForm form) || form.IsDisposed)
            {
                form = new TileDataHelpForm(topic) { Text = title };
                _instances[topic] = form;
                form.Show(owner);
            }
            else
            {
                form.WindowState = FormWindowState.Normal;
                form.Activate();
            }
        }

        public TileDataHelpForm()
            : this("TileData")
        {
        }

        public TileDataHelpForm(string topic)
        {
            _topic = topic;
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            Icon = Options.GetFiddlerIcon();
            if (_language == null)
            {
                string ui = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                _language = ui == "de" || ui == "fr" ? ui : "en";
            }

            _loading = true;
            _cmbLang.SelectedIndex = Math.Max(0, Array.IndexOf(_languages, _language));
            _loading = false;
            Render(ReadHelp(_topic, _language));
        }

        private void OnLanguageSelected(object sender, EventArgs e)
        {
            if (_loading || _cmbLang.SelectedIndex < 0)
            {
                return;
            }

            _language = _languages[_cmbLang.SelectedIndex];
            Render(ReadHelp(_topic, _language));
        }

        public static string ReadHelp(string language) => ReadHelp("TileData", language);

        public static string ReadHelp(string topic, string language)
        {
            Assembly asm = typeof(TileDataHelpForm).Assembly;
            using (Stream stream = asm.GetManifestResourceStream($"{topic}.Help.{language}.md") ?? asm.GetManifestResourceStream($"{topic}.Help.en.md"))
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

        /// <summary>small markdown renderer: headings, lists, tables, **bold**, `code`</summary>
        private void Render(string markdown)
        {
            _text.Clear();
            var normal = new Font("Segoe UI", 10f);
            var bold = new Font("Segoe UI", 10f, FontStyle.Bold);
            var h1 = new Font("Segoe UI", 16f, FontStyle.Bold);
            var h2 = new Font("Segoe UI", 13f, FontStyle.Bold);
            var h3 = new Font("Segoe UI", 11f, FontStyle.Bold);
            var code = new Font("Consolas", 9.5f);
            Color head = Color.FromArgb(20, 60, 120);
            bool inCode = false;

            foreach (string raw in markdown.Replace("\r\n", "\n").Split('\n'))
            {
                string line = raw.TrimEnd();
                if (line.StartsWith("```"))
                {
                    inCode = !inCode;
                    continue;
                }

                if (inCode)
                {
                    Append("    " + line + "\n", code, Color.FromArgb(150, 40, 40));
                }
                else if (line.StartsWith("# "))
                {
                    Append(line.Substring(2) + "\n", h1, head);
                }
                else if (line.StartsWith("## "))
                {
                    Append("\n" + line.Substring(3) + "\n", h2, head);
                }
                else if (line.StartsWith("### "))
                {
                    Append(line.Substring(4) + "\n", h3, Color.FromArgb(40, 40, 40));
                }
                else if (line.StartsWith("|"))
                {
                    if (line.Replace("|", string.Empty).Replace("-", string.Empty).Replace(":", string.Empty).Trim().Length > 0)
                    {
                        string[] cells = line.Trim('|').Split('|').Select(c => c.Trim()).ToArray();
                        Inline("   " + string.Join("   │   ", cells) + "\n", normal, bold, code);
                    }
                }
                else if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    Inline("   •  " + line.Substring(2) + "\n", normal, bold, code);
                }
                else if (line.StartsWith("  - "))
                {
                    Inline("         ◦  " + line.Substring(4) + "\n", normal, bold, code);
                }
                else
                {
                    Inline(line + "\n", normal, bold, code);
                }
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

        private void Inline(string text, Font normal, Font bold, Font code)
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

                Append(text.Substring(next + marker.Length, end - next - marker.Length), isBold ? bold : code,
                    isBold ? Color.Black : Color.FromArgb(150, 40, 40));
                pos = end + marker.Length;
            }
        }
    }
}
