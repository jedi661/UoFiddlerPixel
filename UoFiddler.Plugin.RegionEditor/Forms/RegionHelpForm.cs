/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Help of the region editor. The texts are the files Help\RegionEditor_Help.<lang>.md
 * (embedded in the plugin and copied to plugins\RegionEditorHelp\).
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.RegionEditor.Classes;

namespace UoFiddler.Plugin.RegionEditor.Forms
{
    public partial class RegionHelpForm : Form
    {
        private static RegionHelpForm _instance;
        private bool _loading;

        public static void ShowHelp(IWin32Window owner)
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new RegionHelpForm();
                _instance.Show(owner);
            }
            else
            {
                _instance.WindowState = FormWindowState.Normal;
                _instance.Activate();
            }
        }

        public RegionHelpForm()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            Icon = Options.GetFiddlerIcon();
            _loading = true;
            _cmbLang.SelectedIndex = Math.Max(0, Array.IndexOf(Loc.Languages, Loc.Language));
            _loading = false;

            Loc.LanguageChanged += OnLanguageChanged;
            ThemeManager.Apply(this);
            LoadText();
        }

        private void OnClickClose(object sender, EventArgs e) => Close();

        private void OnClickFolder(object sender, EventArgs e) => OpenFolder();

        private void OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.LinkText) { UseShellExecute = true });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void OnLanguageSelected(object sender, EventArgs e)
        {
            if (!_loading && _cmbLang.SelectedIndex >= 0)
            {
                Loc.Language = Loc.Languages[_cmbLang.SelectedIndex];
            }
        }

        private void OnFormClosedHelp(object sender, FormClosedEventArgs e)
        {
            Loc.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            int index = Array.IndexOf(Loc.Languages, Loc.Language);
            if (_cmbLang.SelectedIndex != index)
            {
                _loading = true;
                _cmbLang.SelectedIndex = index;
                _loading = false;
            }

            LoadText();
        }

        /// <summary>help text of a language (embedded file)</summary>
        public static string ReadHelp(string language)
        {
            Assembly asm = typeof(RegionHelpForm).Assembly;
            using (Stream stream = asm.GetManifestResourceStream($"RegionEditor.Help.{language}.md") ?? asm.GetManifestResourceStream("RegionEditor.Help.en.md"))
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
            _lblLang.Text = Loc.T("lbl.language");
            _btnFolder.Text = Loc.T("help.folder");
            _btnClose.Text = Loc.T("btn.close");
            RenderMarkdown(ReadHelp(Loc.Language));
        }

        private Color TextColor => ThemeManager.Palette.Text;
        private Color HeadColor => ThemeManager.Palette.Accent;
        private Color CodeColor => RegionEditorSettings.Current.Theme == EditorTheme.Dark ? Color.FromArgb(255, 170, 120) : Color.FromArgb(150, 40, 40);

        /// <summary>very small markdown renderer: headings, lists, **bold**, `code`, tables as text</summary>
        private void RenderMarkdown(string markdown)
        {
            _text.Clear();
            _text.BackColor = ThemeManager.Palette.Panel;
            var normal = new Font("Segoe UI", 10f);
            var bold = new Font("Segoe UI", 10f, FontStyle.Bold);
            var h1 = new Font("Segoe UI", 16f, FontStyle.Bold);
            var h2 = new Font("Segoe UI", 13f, FontStyle.Bold);
            var h3 = new Font("Segoe UI", 11f, FontStyle.Bold);
            var code = new Font("Consolas", 9.5f);
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
                    Append("    " + line + "\n", code, CodeColor);
                    continue;
                }

                if (line.StartsWith("# "))
                {
                    Append(line.Substring(2) + "\n", h1, HeadColor);
                    continue;
                }

                if (line.StartsWith("## "))
                {
                    Append("\n" + line.Substring(3) + "\n", h2, HeadColor);
                    continue;
                }

                if (line.StartsWith("### "))
                {
                    Append(line.Substring(4) + "\n", h3, TextColor);
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
                    Append(text.Substring(pos), normal, TextColor);
                    return;
                }

                Append(text.Substring(pos, next - pos), normal, TextColor);
                bool isBold = next == b;
                string marker = isBold ? "**" : "`";
                int end = text.IndexOf(marker, next + marker.Length, StringComparison.Ordinal);
                if (end < 0)
                {
                    Append(text.Substring(next), normal, TextColor);
                    return;
                }

                string inner = text.Substring(next + marker.Length, end - next - marker.Length);
                Append(inner, isBold ? bold : code, isBold ? TextColor : CodeColor);
                pos = end + marker.Length;
            }
        }

        private static void OpenFolder()
        {
            string dir = Path.Combine(Path.GetDirectoryName(typeof(RegionHelpForm).Assembly.Location) ?? string.Empty, "RegionEditorHelp");
            if (Directory.Exists(dir))
            {
                Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true });
            }
        }
    }
}
