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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Dialog to add or edit a server in the address book.
    /// </summary>
    public class ServerEntryDialog : Form
    {
        private readonly TextBox _name = new();
        private readonly TextBox _host = new();
        private readonly TextBox _ipv4 = new();
        private readonly TextBox _ipv6 = new();
        private readonly NumericUpDown _port = new();
        private readonly ComboBox _group = new();
        private readonly TextBox _notes = new();
        private readonly CheckBox _favorite = new();
        private readonly Button _resolve = new();
        private readonly Label _hint = new();
        private readonly ErrorProvider _errors = new();

        public ServerEntry Entry { get; }

        public ServerEntryDialog(ServerEntry entry, IEnumerable<string> groups, bool isNew)
        {
            Entry = entry;

            Text = isNew ? "Add server" : "Edit server";
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(470, 420);
            _errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                Padding = new Padding(14, 12, 14, 6)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            void AddRow(string label, Control control, Control extra = null)
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
                control.Margin = new Padding(0, 3, 3, 3);
                table.Controls.Add(control, 1, row);
                if (extra != null)
                {
                    table.Controls.Add(extra, 2, row);
                }
                else
                {
                    table.SetColumnSpan(control, 2);
                }
            }

            _resolve.Text = "Resolve";
            _resolve.AutoSize = true;
            _resolve.Margin = new Padding(3, 2, 0, 2);
            _resolve.Click += Resolve_Click;

            _port.Minimum = 1;
            _port.Maximum = 65535;

            _group.DropDownStyle = ComboBoxStyle.DropDown;
            _group.Items.AddRange(groups.Where(g => !string.IsNullOrWhiteSpace(g)).Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g).Cast<object>().ToArray());
            _group.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _group.AutoCompleteSource = AutoCompleteSource.ListItems;

            _notes.Multiline = true;
            _notes.ScrollBars = ScrollBars.Vertical;
            _notes.Height = 90;

            _favorite.Text = "Favorite (always shown at the top)";
            _favorite.AutoSize = true;

            _name.PlaceholderText = "e.g. My Shard (Live)";
            _host.PlaceholderText = "play.myshard.com";
            _ipv4.PlaceholderText = "192.168.0.10";
            _ipv6.PlaceholderText = "2001:db8::10";

            AddRow("Name", _name);
            AddRow("Host / Domain", _host, _resolve);
            AddRow("IPv4", _ipv4);
            AddRow("IPv6", _ipv6);
            AddRow("Port", _port);
            AddRow("Group", _group);
            AddRow("Notes", _notes);
            AddRow("", _favorite);

            _hint.Text = "A host name, IPv4 or IPv6 address is required. \"Resolve\" fills IPv4/IPv6 from the host name.";
            _hint.ForeColor = Color.DimGray;
            _hint.AutoSize = true;
            _hint.MaximumSize = new Size(420, 0);
            int hintRow = table.RowCount++;
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(_hint, 0, hintRow);
            table.SetColumnSpan(_hint, 3);

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

            _name.Text = entry.Name;
            _host.Text = entry.Host;
            _ipv4.Text = entry.IPv4;
            _ipv6.Text = entry.IPv6;
            _port.Value = Math.Clamp(entry.Port, 1, 65535);
            _group.Text = entry.Group;
            _notes.Text = entry.Notes;
            _favorite.Checked = entry.Favorite;
        }

        private async void Resolve_Click(object sender, EventArgs e)
        {
            string host = _host.Text.Trim();
            if (!NetworkTools.IsValidHost(host))
            {
                _errors.SetError(_host, "Enter a valid host name first.");
                return;
            }

            _errors.SetError(_host, "");
            _resolve.Enabled = false;
            UseWaitCursor = true;
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                IPAddress[] all = await NetworkTools.ResolveAllAsync(host, cts.Token);
                IPAddress v4 = all.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                IPAddress v6 = all.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6);
                if (v4 != null)
                {
                    _ipv4.Text = v4.ToString();
                }

                if (v6 != null)
                {
                    _ipv6.Text = v6.ToString();
                }

                if (string.IsNullOrWhiteSpace(_name.Text))
                {
                    _name.Text = host;
                }

                _hint.Text = $"Resolved {host}: {(v4?.ToString() ?? "no IPv4")}, {(v6?.ToString() ?? "no IPv6")}";
            }
            catch (Exception ex)
            {
                _errors.SetError(_host, "Could not resolve: " + ex.Message);
            }
            finally
            {
                _resolve.Enabled = true;
                UseWaitCursor = false;
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            _errors.Clear();
            bool valid = true;

            string name = _name.Text.Trim();
            string host = _host.Text.Trim();
            string v4 = _ipv4.Text.Trim();
            string v6 = _ipv6.Text.Trim();

            if (host.Length > 0 && !NetworkTools.IsValidHost(host))
            {
                _errors.SetError(_host, "Invalid host name.");
                valid = false;
            }

            if (v4.Length > 0 && !NetworkTools.IsIPv4(v4))
            {
                _errors.SetError(_ipv4, "Invalid IPv4 address.");
                valid = false;
            }

            if (v6.Length > 0 && !NetworkTools.IsIPv6(v6))
            {
                _errors.SetError(_ipv6, "Invalid IPv6 address.");
                valid = false;
            }

            if (host.Length == 0 && v4.Length == 0 && v6.Length == 0)
            {
                _errors.SetError(_host, "Enter a host name, IPv4 or IPv6 address.");
                valid = false;
            }

            if (name.Length == 0)
            {
                name = host.Length > 0 ? host : v4.Length > 0 ? v4 : v6;
            }

            if (!valid)
            {
                return;
            }

            Entry.Name = name;
            Entry.Host = host;
            Entry.IPv4 = v4;
            Entry.IPv6 = v6;
            Entry.Port = (int)_port.Value;
            Entry.Group = _group.Text.Trim();
            Entry.Notes = _notes.Text.Trim();
            Entry.Favorite = _favorite.Checked;

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
