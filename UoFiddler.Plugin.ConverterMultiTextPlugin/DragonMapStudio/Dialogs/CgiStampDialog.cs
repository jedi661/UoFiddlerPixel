/***************************************************************************
 *
 *  * Dragon Map Studio – CGI area / whole map -> colour stamp: name, category, modes,
 *  * and the list of suggested new colours
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    /// <summary>Name, category, statics mode and missing colour mode for a conversion (built in code, no designer file).</summary>
    public sealed class CgiStampDialog : Form
    {
        private readonly TextBox _name;
        private readonly ComboBox _category;
        private readonly ComboBox _statics;
        private readonly ComboBox _colors;

        public CgiStampDialog(string title, string summary, IEnumerable<string> categories, bool askName, string defaultName)
        {
            Text = L.T(title);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, askName ? 300 : 230);
            Font = new Font("Segoe UI", 9f);
            int y = 12;
            Label L1(string t) => new Label { Text = L.T(t), Location = new Point(12, y + 3), AutoSize = true };
            _name = new TextBox { Location = new Point(170, y), Width = 330, Text = defaultName };
            _category = new ComboBox { Location = new Point(170, y + 32), Width = 330, DropDownStyle = ComboBoxStyle.DropDown };
            _category.Items.AddRange(categories.Distinct(StringComparer.OrdinalIgnoreCase).Cast<object>().ToArray());
            _category.Text = "Eigene";
            if (askName)
            {
                Controls.Add(L1("Name:"));
                Controls.Add(_name);
                y += 32;
                Controls.Add(L1("Kategorie:"));
                Controls.Add(_category);
                y += 40;
            }
            Controls.Add(L1("Statics:"));
            _statics = EnumCombo(typeof(StaticsMode), new Point(170, y));
            Controls.Add(_statics);
            y += 32;
            Controls.Add(L1("Fehlende Farben:"));
            _colors = EnumCombo(typeof(MissingColorMode), new Point(170, y));
            Controls.Add(_colors);
            y += 38;
            Controls.Add(new Label { Text = summary, Location = new Point(12, y), Size = new Size(496, 64), ForeColor = Color.FromArgb(71, 85, 105) });
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(ClientSize.Width - 180, ClientSize.Height - 38), Width = 80 };
            var cancel = new Button { Text = L.T("Abbrechen"), DialogResult = DialogResult.Cancel, Location = new Point(ClientSize.Width - 92, ClientSize.Height - 38), Width = 80 };
            Controls.Add(ok);
            Controls.Add(cancel);
            AcceptButton = ok;
            CancelButton = cancel;
        }

        private static ComboBox EnumCombo(Type t, Point at)
        {
            var c = new ComboBox { Location = at, Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (object v in Enum.GetValues(t))
            {
                string text = t.GetField(v.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? v.ToString();
                c.Items.Add(L.T(text));
            }
            c.SelectedIndex = 0;
            return c;
        }

        public string StampName => _name.Text.Trim();

        public string Category => _category.Text.Trim();

        public StaticsMode Statics => (StaticsMode)Math.Max(0, _statics.SelectedIndex);

        public MissingColorMode Colors => (MissingColorMode)Math.Max(0, _colors.SelectedIndex);
    }

    /// <summary>Suggested new colours (tile, height, tiledata name, radar colour, group): tick what should be created.</summary>
    public sealed class ColorSuggestionDialog : Form
    {
        private readonly DataGridView _grid;
        private readonly List<ColorSuggestion> _items;

        public ColorSuggestionDialog(List<ColorSuggestion> items, DragonRuleSet rules)
        {
            _items = items;
            Text = L.T("Neue Farben für die Farbtabelle");
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(900, 560);
            Font = new Font("Segoe UI", 9f);
            var info = new Label
            {
                Dock = DockStyle.Top,
                Height = 46,
                Padding = new Padding(8, 6, 8, 0),
                Text = L.T("Diese Land-Tiles / Höhen haben noch keine Farbe. Angehakte werden als neue Farben angelegt (Name und Radar-Farbe aus TileData / radarcol, " +
                           "Gruppe: eine vorhandene Gruppe mit gleichem Tile-Namen oder eine neue). Nicht angehakte bekommen die ähnlichste vorhandene Farbe.")
            };
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = L.T("Anlegen"), FillWeight = 30 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tile", ReadOnly = true, FillWeight = 35 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Z", ReadOnly = true, FillWeight = 25 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L.T("Anzahl"), ReadOnly = true, FillWeight = 35 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L.T("TileData-Name"), ReadOnly = true, FillWeight = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L.T("Radar"), ReadOnly = true, FillWeight = 40 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L.T("Gruppe (Name änderbar)"), FillWeight = 110 });
            foreach (ColorSuggestion s in items)
            {
                int r = _grid.Rows.Add(s.Accept, s.Tile.ToString("x4"), s.Altitude, s.Count, s.TileName, $"#{s.Radar.R:x2}{s.Radar.G:x2}{s.Radar.B:x2}",
                    (s.NewGroup ? L.T("neu: ") : rules.HexGroup(s.Group) + " ") + s.GroupName);
                _grid.Rows[r].Cells[5].Style.BackColor = s.Radar;
                _grid.Rows[r].Cells[5].Style.ForeColor = s.Radar.GetBrightness() > 0.55f ? Color.Black : Color.White;
                if (!s.NewGroup)
                {
                    _grid.Rows[r].Cells[6].ReadOnly = true;
                }
            }
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
            var ok = new Button { Text = L.T("Anlegen + umwandeln"), DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = L.T("Abbrechen"), DialogResult = DialogResult.Cancel, AutoSize = true };
            var all = new Button { Text = L.T("Alle"), AutoSize = true };
            all.Click += (s, e) => SetAll(true);
            var none = new Button { Text = L.T("Keine"), AutoSize = true };
            none.Click += (s, e) => SetAll(false);
            bottom.Controls.AddRange(new Control[] { ok, cancel, none, all });
            Controls.Add(_grid);
            Controls.Add(bottom);
            Controls.Add(info);
            AcceptButton = ok;
            CancelButton = cancel;
        }

        private void SetAll(bool on)
        {
            foreach (DataGridViewRow r in _grid.Rows)
            {
                r.Cells[0].Value = on;
            }
        }

        /// <summary>Writes the ticks and changed group names back into the suggestions.</summary>
        public void ApplyTo()
        {
            _grid.EndEdit();
            for (int i = 0; i < _items.Count && i < _grid.Rows.Count; i++)
            {
                _items[i].Accept = _grid.Rows[i].Cells[0].Value is bool b && b;
                if (_items[i].NewGroup)
                {
                    string g = (_grid.Rows[i].Cells[6].Value as string ?? string.Empty).Trim();
                    if (g.StartsWith(L.T("neu: "), StringComparison.OrdinalIgnoreCase))
                    {
                        g = g.Substring(L.T("neu: ").Length);
                    }
                    if (g.Length > 0)
                    {
                        _items[i].GroupName = g;
                    }
                }
            }
        }
    }
}
