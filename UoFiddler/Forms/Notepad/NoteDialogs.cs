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
using System.Globalization;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Forms.Notepad
{
    /// <summary>
    /// Access to UO graphics for notes (safe when the client files are not loaded).
    /// </summary>
    internal static class UoGraphics
    {
        public static string ItemName(int id)
        {
            try
            {
                return id >= 0 && id < TileData.ItemTable.Length ? (TileData.ItemTable[id].Name ?? "").Trim() : "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// New bitmap of an item (static) or gump with optional hue (game hue number), or null.
        /// </summary>
        public static Bitmap Get(bool gump, int id, int hue)
        {
            try
            {
                Bitmap source = gump ? Gumps.GetGump(id) : Art.GetStatic(id);
                if (source == null)
                {
                    return null;
                }

                var bmp = new Bitmap(source);
                if (hue > 0)
                {
                    bool partial = !gump && id < TileData.ItemTable.Length && (TileData.ItemTable[id].Flags & TileFlag.PartialHue) != 0;
                    Hues.GetHue(hue - 1).ApplyTo(bmp, partial);
                }

                return bmp;
            }
            catch
            {
                return null;
            }
        }

        public static bool TryParseId(string text, out int id)
        {
            text = (text ?? "").Trim();
            id = -1;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id) && id >= 0;
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out id) && id >= 0;
        }
    }

    /// <summary>
    /// Insert an item or gump image (optionally hued) into a note.
    /// </summary>
    internal sealed class UoGraphicDialog : Form
    {
        private readonly ComboBox _kind = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _id = new() { PlaceholderText = "0x1234 or 4660" };
        private readonly NumericUpDown _hue = new() { Maximum = 3000 };
        private readonly ComboBox _scale = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox _caption = new() { Text = "Add id and name as caption", AutoSize = true, Checked = true };
        private readonly PictureBox _preview = new() { BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(40, 42, 48), SizeMode = PictureBoxSizeMode.Zoom };
        private readonly Label _info = new() { AutoSize = false, ForeColor = Color.DimGray };
        private readonly Func<int> _selectedItem;
        private readonly Func<int> _selectedGump;

        public Bitmap ResultImage { get; private set; }
        public string ResultCaption { get; private set; } = "";

        public UoGraphicDialog(Func<int> selectedItem, Func<int> selectedGump)
        {
            _selectedItem = selectedItem;
            _selectedGump = selectedGump;
            Text = "Insert UO graphic";
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(470, 300);

            Label L(string t, int y) => new() { Text = t, AutoSize = true, Location = new Point(14, y + 3) };
            _kind.Items.AddRange(new object[] { "Item (art)", "Gump" });
            _kind.SelectedIndex = 0;
            _scale.Items.AddRange(new object[] { "50 %", "100 %", "150 %", "200 %" });
            _scale.SelectedIndex = 1;

            Controls.Add(L("Type", 14));
            _kind.SetBounds(90, 12, 130, 23);
            Controls.Add(L("ID", 46));
            _id.SetBounds(90, 44, 130, 23);
            Controls.Add(L("Hue", 78));
            _hue.SetBounds(90, 76, 130, 23);
            Controls.Add(L("Size", 110));
            _scale.SetBounds(90, 108, 130, 23);
            _caption.Location = new Point(90, 142);
            var fromFiddler = new Button { Text = "Take selected from UoFiddler", Bounds = new Rectangle(90, 170, 190, 28) };
            fromFiddler.Click += (_, _) =>
            {
                int id = _kind.SelectedIndex == 1 ? _selectedGump?.Invoke() ?? -1 : _selectedItem?.Invoke() ?? -1;
                if (id >= 0)
                {
                    _id.Text = "0x" + id.ToString("X4");
                }
                else
                {
                    _info.Text = "Nothing selected in the " + (_kind.SelectedIndex == 1 ? "Gumps" : "Items") + " tab.";
                }
            };
            _preview.SetBounds(300, 12, 154, 186);
            _info.SetBounds(14, 208, 440, 40);

            var ok = new Button { Text = "Insert", Bounds = new Rectangle(270, 258, 90, 30) };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(366, 258, 90, 30) };
            ok.Click += Ok_Click;

            Controls.AddRange(new Control[] { _kind, _id, _hue, _scale, _caption, fromFiddler, _preview, _info, ok, cancel });
            AcceptButton = ok;
            CancelButton = cancel;

            _kind.SelectedIndexChanged += (_, _) => UpdatePreview();
            _id.TextChanged += (_, _) => UpdatePreview();
            _hue.ValueChanged += (_, _) => UpdatePreview();
        }

        public void SetId(int id) => _id.Text = "0x" + id.ToString("X4");

        private void UpdatePreview()
        {
            Image old = _preview.Image;
            _preview.Image = null;
            old?.Dispose();
            if (!UoGraphics.TryParseId(_id.Text, out int id))
            {
                _info.Text = "";
                return;
            }

            bool gump = _kind.SelectedIndex == 1;
            _preview.Image = UoGraphics.Get(gump, id, (int)_hue.Value);
            string name = gump ? "" : UoGraphics.ItemName(id);
            _info.Text = _preview.Image == null
                ? "Not found (or the client files are not loaded)."
                : $"{(gump ? "Gump" : "Item")} 0x{id:X4} ({id}) {name}  ·  {_preview.Image.Width}×{_preview.Image.Height}";
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (!UoGraphics.TryParseId(_id.Text, out int id))
            {
                _info.Text = "Enter an id.";
                return;
            }

            bool gump = _kind.SelectedIndex == 1;
            Bitmap bmp = UoGraphics.Get(gump, id, (int)_hue.Value);
            if (bmp == null)
            {
                _info.Text = "Graphic not found.";
                return;
            }

            float scale = _scale.SelectedIndex switch { 0 => 0.5f, 2 => 1.5f, 3 => 2f, _ => 1f };
            if (Math.Abs(scale - 1f) > 0.01f)
            {
                var scaled = new Bitmap(Math.Max(1, (int)(bmp.Width * scale)), Math.Max(1, (int)(bmp.Height * scale)));
                using (Graphics g = Graphics.FromImage(scaled))
                {
                    g.InterpolationMode = scale > 1 ? System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor
                        : System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                    g.DrawImage(bmp, 0, 0, scaled.Width, scaled.Height);
                }

                bmp.Dispose();
                bmp = scaled;
            }

            ResultImage = bmp;
            string name = gump ? "" : UoGraphics.ItemName(id);
            ResultCaption = _caption.Checked
                ? $"{(gump ? "Gump" : "Item")} 0x{id:X4}" + (name.Length > 0 ? " – " + name : "") + (_hue.Value > 0 ? $" (hue {_hue.Value})" : "")
                : "";
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _preview.Image?.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Hex / decimal converter that can insert the result into the note.
    /// </summary>
    internal sealed class HexDecDialog : Form
    {
        private readonly TextBox _input = new();
        private readonly Label _dec = new() { AutoSize = true, Font = new Font("Consolas", 11F) };
        private readonly Label _hex = new() { AutoSize = true, Font = new Font("Consolas", 11F) };

        public string Result { get; private set; }

        public HexDecDialog(string initial)
        {
            Text = "Hex ⇄ Dec";
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(360, 170);

            Controls.Add(new Label { Text = "Number (0x1A2B, 1A2Bh or 6699):", AutoSize = true, Location = new Point(14, 12) });
            _input.SetBounds(14, 34, 330, 23);
            Controls.Add(new Label { Text = "Dec:", AutoSize = true, Location = new Point(14, 72) });
            _dec.Location = new Point(60, 69);
            Controls.Add(new Label { Text = "Hex:", AutoSize = true, Location = new Point(14, 100) });
            _hex.Location = new Point(60, 97);

            var insertDec = new Button { Text = "Insert dec", Bounds = new Rectangle(14, 130, 100, 28) };
            var insertHex = new Button { Text = "Insert hex", Bounds = new Rectangle(120, 130, 100, 28) };
            var close = new Button { Text = "Close", DialogResult = DialogResult.Cancel, Bounds = new Rectangle(254, 130, 90, 28) };
            insertDec.Click += (_, _) => Finish(_dec.Text);
            insertHex.Click += (_, _) => Finish(_hex.Text);
            Controls.AddRange(new Control[] { _input, _dec, _hex, insertDec, insertHex, close });
            CancelButton = close;

            _input.TextChanged += (_, _) => Convert();
            _input.Text = initial ?? "";
        }

        private void Finish(string value)
        {
            if (value.Length == 0 || value == "-")
            {
                return;
            }

            Result = value;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Convert()
        {
            string t = _input.Text.Trim();
            long v;
            bool ok = t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? long.TryParse(t.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v)
                : t.EndsWith("h", StringComparison.OrdinalIgnoreCase)
                    ? long.TryParse(t.Substring(0, t.Length - 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v)
                    : long.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);

            _dec.Text = ok ? v.ToString(CultureInfo.InvariantCulture) : "-";
            _hex.Text = ok && v >= 0 ? "0x" + v.ToString("X4", CultureInfo.InvariantCulture) : "-";
        }
    }

    /// <summary>
    /// Small preview window for an item id under the mouse.
    /// </summary>
    internal sealed class IdPreviewPopup : Form
    {
        private readonly PictureBox _picture = new() { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.CenterImage, BackColor = Color.FromArgb(40, 42, 48) };
        private readonly Label _label = new() { Dock = DockStyle.Bottom, Height = 36, ForeColor = Color.White, BackColor = Color.FromArgb(32, 36, 44), TextAlign = ContentAlignment.MiddleCenter };

        public IdPreviewPopup()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Size = new Size(180, 190);
            Font = new Font("Segoe UI", 8.25F);
            Controls.Add(_picture);
            Controls.Add(_label);
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000 | 0x80; // WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW
                return cp;
            }
        }

        public bool ShowId(int id, Point screen)
        {
            Bitmap bmp = UoGraphics.Get(false, id, 0);
            if (bmp == null)
            {
                Hide();
                return false;
            }

            Image old = _picture.Image;
            _picture.SizeMode = bmp.Width > 176 || bmp.Height > 150 ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage;
            _picture.Image = bmp;
            old?.Dispose();
            string name = UoGraphics.ItemName(id);
            _label.Text = $"0x{id:X4} ({id})" + (name.Length > 0 ? "\n" + name : "") + "\nCtrl+Click: show in UoFiddler";
            _label.Height = name.Length > 0 ? 48 : 34;

            Rectangle screenArea = Screen.FromPoint(screen).WorkingArea;
            Location = new Point(Math.Min(screen.X, screenArea.Right - Width), Math.Min(screen.Y, screenArea.Bottom - Height));
            if (!Visible)
            {
                Show();
            }

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _picture.Image?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
