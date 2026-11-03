/***************************************************************************
 *
 * $Author: Nikodemus
 * Coder: Nikodemus
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Dialog to add or edit a router / provider / DNS / internet reference address.
    /// </summary>
    public class ReferenceHostDialog : Form
    {
        private readonly TextBox _name = new();
        private readonly TextBox _address = new();
        private readonly ComboBox _kind = new();
        private readonly TextBox _notes = new();
        private readonly ErrorProvider _errors = new();

        public ReferenceHost Host { get; }

        public ReferenceHostDialog(ReferenceHost host, bool isNew)
        {
            Host = host;

            Text = isNew ? "Add connection address" : "Edit connection address";
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(440, 300);
            _errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(14, 12, 24, 6)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            void AddRow(string label, Control control)
            {
                int row = table.RowCount++;
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.Controls.Add(new Label
                {
                    Text = label,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 6, 6, 6)
                }, 0, row);
                control.Dock = DockStyle.Fill;
                control.Margin = new Padding(0, 3, 0, 3);
                table.Controls.Add(control, 1, row);
            }

            _kind.DropDownStyle = ComboBoxStyle.DropDownList;
            _kind.Items.AddRange(ReferenceHost.Kinds);
            _notes.Multiline = true;
            _notes.Height = 60;
            _name.PlaceholderText = "e.g. FritzBox, Telekom node, Cloudflare";
            _address.PlaceholderText = "192.168.178.1, 2001:db8::1 or host name";

            AddRow("Type", _kind);
            AddRow("Name", _name);
            AddRow("Address", _address);
            AddRow("Notes", _notes);

            var hint = new Label
            {
                Text = "Router = your own router (gateway). Provider = first node of your internet provider. " +
                       "DNS = name server. Internet = a big public host (e.g. 1.1.1.1) to test the internet.",
                ForeColor = Color.DimGray,
                AutoSize = true,
                MaximumSize = new Size(400, 0),
                Margin = new Padding(0, 6, 0, 0)
            };
            int hintRow = table.RowCount++;
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(hint, 0, hintRow);
            table.SetColumnSpan(hint, 2);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 44,
                Padding = new Padding(10, 6, 10, 6)
            };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90 };
            var ok = new Button { Text = "Save", Width = 90 };
            ok.Click += Ok_Click;
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);

            Controls.Add(table);
            Controls.Add(buttons);
            AcceptButton = ok;
            CancelButton = cancel;

            int kindIndex = Array.IndexOf(ReferenceHost.Kinds, host.Kind);
            _kind.SelectedIndex = kindIndex < 0 ? 1 : kindIndex;
            _name.Text = host.Name;
            _address.Text = host.Address;
            _notes.Text = host.Notes;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            _errors.Clear();
            string address = _address.Text.Trim();
            if (!NetworkTools.IsValidHost(address))
            {
                _errors.SetError(_address, "Enter a valid IPv4 / IPv6 address or host name.");
                return;
            }

            Host.Kind = _kind.SelectedItem?.ToString() ?? "Other";
            Host.Address = address;
            Host.Name = _name.Text.Trim().Length > 0 ? _name.Text.Trim() : Host.Kind + " " + address;
            Host.Notes = _notes.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _errors.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
