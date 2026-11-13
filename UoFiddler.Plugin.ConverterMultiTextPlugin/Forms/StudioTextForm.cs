/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Shows generated text (def file entries, scripts) of the UO Studio tools: copy, save as
    /// or append to an existing file (e.g. mobtypes.txt, Body.def, Sound.def).
    /// </summary>
    public partial class StudioTextForm : Form
    {
        private readonly string _defaultFileName;

        /// <summary>Designer constructor.</summary>
        public StudioTextForm()
        {
            InitializeComponent();
        }

        public StudioTextForm(string title, string text, string defaultFileName) : this()
        {
            Text = title;
            txtContent.Text = text.Replace("\r\n", "\n").Replace("\n", "\r\n");
            txtContent.SelectionStart = 0;
            _defaultFileName = defaultFileName;
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (txtContent.TextLength > 0)
            {
                Clipboard.SetText(txtContent.Text);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "Text|*.txt;*.def;*.cs|Alle Dateien|*.*",
                FileName = _defaultFileName ?? "uostudio.txt",
                InitialDirectory = StudioUi.OutputPath
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(dlg.FileName, txtContent.Text, new UTF8Encoding(false));
                StudioUi.Info(this, "Gespeichert:\n" + dlg.FileName);
            }
        }

        private void BtnAppend_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Text|*.txt;*.def|Alle Dateien|*.*",
                FileName = _defaultFileName ?? string.Empty,
                InitialDirectory = StudioUi.OutputPath,
                CheckFileExists = false,
                Title = "Datei wählen, an die der Text angehängt wird"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            string prefix = File.Exists(dlg.FileName) && new FileInfo(dlg.FileName).Length > 0 ? "\r\n" : string.Empty;
            File.AppendAllText(dlg.FileName, prefix + txtContent.Text + "\r\n", new UTF8Encoding(false));
            StudioUi.Info(this, "Angehängt an:\n" + dlg.FileName);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>Shows the text window (non modal).</summary>
        public static void ShowText(IWin32Window owner, string title, string text, string defaultFileName)
        {
            var form = new StudioTextForm(title, text, defaultFileName);
            form.Show(owner);
        }
    }
}
