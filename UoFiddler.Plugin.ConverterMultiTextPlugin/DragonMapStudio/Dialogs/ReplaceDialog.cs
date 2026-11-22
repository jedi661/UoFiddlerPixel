/***************************************************************************
 *
 *  * Dragon Map Studio – CGI view: search & replace statics / land tiles (area, lasso or whole map)
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class ReplaceDialog : Form
    {
        private readonly TextBox _fromStatic = new TextBox();
        private readonly TextBox _toStatic = new TextBox();
        private readonly TextBox _onlyHue = new TextBox { Text = string.Empty };
        private readonly TextBox _newHue = new TextBox { Text = string.Empty };
        private readonly TextBox _fromLand = new TextBox();
        private readonly TextBox _toLand = new TextBox();
        private readonly RadioButton _area = new RadioButton { AutoSize = true, Checked = true };
        private readonly RadioButton _all = new RadioButton { AutoSize = true };

        public ReplaceDialog(bool hasArea, ushort itemNow, ushort landNow)
        {
            Text = L.T("Suchen & Ersetzen");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 330);
            Font = new Font("Segoe UI", 9f);
            int y = 12;
            void Row(string label, Control box, string hint)
            {
                Controls.Add(new Label { Text = L.T(label), Location = new Point(12, y + 3), AutoSize = true });
                box.Location = new Point(170, y);
                box.Width = 150;
                Controls.Add(box);
                Controls.Add(new Label { Text = L.T(hint), Location = new Point(328, y + 3), Size = new Size(185, 34), ForeColor = Color.FromArgb(100, 116, 139) });
                y += 34;
            }
            Controls.Add(new Label { Text = L.T("Statics"), Location = new Point(12, y), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            y += 22;
            Row("Suchen (IDs hex):", _fromStatic, "z. B. 0ccd 0cce");
            Row("Ersetzen durch:", _toStatic, "gleich viele = 1:1, sonst zufällig, leer = löschen");
            Row("Nur mit Hue (dez.):", _onlyHue, "leer = jede Hue");
            Row("Neue Hue (dez.):", _newHue, "leer = Hue bleibt");
            Controls.Add(new Label { Text = L.T("Land"), Location = new Point(12, y), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) });
            y += 22;
            Row("Suchen (Tiles hex):", _fromLand, "z. B. 0003 0004");
            Row("Ersetzen durch:", _toLand, "gleich viele = 1:1, sonst zufällig");
            _area.Text = L.T("Im markierten Bereich / Lasso");
            _area.Enabled = hasArea;
            _area.Checked = hasArea;
            _all.Text = L.T("Ganze Karte");
            _all.Checked = !hasArea;
            _area.Location = new Point(12, y + 4);
            _all.Location = new Point(240, y + 4);
            Controls.Add(_area);
            Controls.Add(_all);
            var ok = new Button { Text = L.T("Ersetzen"), DialogResult = DialogResult.OK, Location = new Point(320, ClientSize.Height - 40), Size = new Size(95, 28) };
            var cancel = new Button { Text = L.T("Abbrechen"), DialogResult = DialogResult.Cancel, Location = new Point(420, ClientSize.Height - 40), Size = new Size(90, 28) };
            Controls.Add(ok);
            Controls.Add(cancel);
            AcceptButton = ok;
            CancelButton = cancel;
            if (itemNow != 0)
            {
                _fromStatic.Text = itemNow.ToString("x4");
            }
            if (landNow != 0)
            {
                _fromLand.PlaceholderText = landNow.ToString("x4");
            }
        }

        public bool WholeMap => _all.Checked;
        public ushort[] FromStatic => HexList.Parse(_fromStatic.Text);
        public ushort[] ToStatic => HexList.Parse(_toStatic.Text);
        public ushort[] FromLand => HexList.Parse(_fromLand.Text);
        public ushort[] ToLand => HexList.Parse(_toLand.Text);
        public int OnlyHue => int.TryParse(_onlyHue.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : -1;
        public int NewHue => int.TryParse(_newHue.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : -1;
    }
}
