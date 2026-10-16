// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System.Windows.Forms;

namespace UoFiddler.Controls.GumpLibrary.Forms
{
    /// <summary>Small input dialog (one line of text).</summary>
    public partial class GumpLibraryInputForm : Form
    {
        public GumpLibraryInputForm()
        {
            InitializeComponent();
        }

        public string Prompt
        {
            get => labelPrompt.Text;
            set => labelPrompt.Text = value ?? string.Empty;
        }

        public string Value
        {
            get => textBoxValue.Text.Trim();
            set => textBoxValue.Text = value ?? string.Empty;
        }

        /// <summary>Shows the dialog and returns the text or null when cancelled.</summary>
        public static string Ask(IWin32Window owner, string title, string prompt, string value = "")
        {
            using (var form = new GumpLibraryInputForm())
            {
                form.Text = title;
                form.Prompt = prompt;
                form.Value = value;
                form.textBoxValue.SelectAll();
                return form.ShowDialog(owner) == DialogResult.OK ? form.Value : null;
            }
        }
    }
}
