/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace UoFiddler.Plugin.Enhanced.Classes
{
    /// <summary>
    /// Small input dialogs used by the Enhanced Client editors.
    /// </summary>
    public static class EcDialogs
    {
        /// <summary>
        /// Shows a dialog with one text box per label. Returns null when cancelled.
        /// </summary>
        public static string[] Prompt(IWin32Window owner, string title, string[] labels, string[] values)
        {
            using (var form = new Form())
            {
                form.Text = title;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ShowInTaskbar = false;
                form.AutoScaleMode = AutoScaleMode.Font;

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    Padding = new Padding(10),
                    AutoSize = true
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                var boxes = new TextBox[labels.Length];
                for (int i = 0; i < labels.Length; i++)
                {
                    layout.Controls.Add(new Label { Text = labels[i], AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 3, 3) }, 0, i);
                    boxes[i] = new TextBox { Text = values != null && i < values.Length ? values[i] : string.Empty, Width = 360, Anchor = AnchorStyles.Left | AnchorStyles.Right };
                    layout.Controls.Add(boxes[i], 1, i);
                }

                var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
                var ok = new Button { Text = "OK", DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
                buttons.Controls.Add(cancel);
                buttons.Controls.Add(ok);
                layout.Controls.Add(buttons, 0, labels.Length);
                layout.SetColumnSpan(buttons, 2);

                form.Controls.Add(layout);
                form.AcceptButton = ok;
                form.CancelButton = cancel;
                form.AutoSize = true;
                form.AutoSizeMode = AutoSizeMode.GrowAndShrink;

                if (form.ShowDialog(owner) != DialogResult.OK)
                {
                    return null;
                }

                var result = new string[labels.Length];
                for (int i = 0; i < labels.Length; i++)
                {
                    result[i] = boxes[i].Text;
                }
                return result;
            }
        }

        public static string Prompt(IWin32Window owner, string title, string label, string value)
        {
            return Prompt(owner, title, new[] { label }, new[] { value })?[0];
        }

        /// <summary>
        /// Parses decimal or hex ("0x1A2B") numbers.
        /// </summary>
        public static bool TryParseNumber(string text, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return long.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }
            return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public static void ShowError(IWin32Window owner, string message, Exception ex = null)
        {
            MessageBox.Show(owner, ex == null ? message : $"{message}\r\n\r\n{ex.Message}", "Enhanced Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Confirm(IWin32Window owner, string message)
        {
            return MessageBox.Show(owner, message, "Enhanced Client", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public static Font MonoFont(float size = 9f) => new Font(FontFamily.GenericMonospace, size);
    }
}
