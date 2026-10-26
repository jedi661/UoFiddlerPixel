/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Small text input dialog (notes for the protocol)
    /// </summary>
    public static class InputDialog
    {
        /// <summary>
        /// Returns the entered text or null if canceled
        /// </summary>
        public static string Show(IWin32Window owner, string title, string prompt, string text, bool multiline = true)
        {
            using (var form = new Form())
            using (var label = new Label())
            using (var textBox = new TextBox())
            using (var ok = new Button())
            using (var cancel = new Button())
            {
                form.Text = title;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ShowInTaskbar = false;
                form.ClientSize = new Size(420, multiline ? 190 : 110);

                label.Text = prompt;
                label.SetBounds(12, 10, 396, 20);

                textBox.Text = text ?? string.Empty;
                textBox.Multiline = multiline;
                textBox.ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None;
                textBox.SetBounds(12, 34, 396, multiline ? 106 : 23);

                ok.Text = "OK";
                ok.DialogResult = DialogResult.OK;
                ok.SetBounds(252, form.ClientSize.Height - 38, 75, 27);

                cancel.Text = "Cancel";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.SetBounds(333, form.ClientSize.Height - 38, 75, 27);

                form.Controls.AddRange(new Control[] { label, textBox, ok, cancel });
                form.AcceptButton = multiline ? null : ok;
                form.CancelButton = cancel;

                return form.ShowDialog(owner) == DialogResult.OK ? textBox.Text : null;
            }
        }
    }
}
