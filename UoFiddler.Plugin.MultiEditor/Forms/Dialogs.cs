/***************************************************************************
 *
 * MultiEditor – small dialogs and preview helper
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Forms
{
    /// <summary>
    /// Simple row based dialog builder
    /// </summary>
    public sealed class SimpleDialog : Form
    {
        private readonly TableLayoutPanel _table;
        private readonly Dictionary<string, Control> _controls = new Dictionary<string, Control>();

        public SimpleDialog(string title, int width = 380)
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(10);

            _table = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                MinimumSize = new Size(width, 0)
            };
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(_table);
        }

        private void AddRow(string label, Control control)
        {
            int row = _table.RowCount++;
            _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            if (label != null)
            {
                _table.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 6, 3) }, 0, row);
                _table.Controls.Add(control, 1, row);
            }
            else
            {
                _table.Controls.Add(control, 0, row);
                _table.SetColumnSpan(control, 2);
            }
        }

        public SimpleDialog AddText(string key, string label, string value)
        {
            var tb = new TextBox { Text = value ?? string.Empty, Dock = DockStyle.Fill, Width = 220 };
            _controls[key] = tb;
            AddRow(label, tb);
            return this;
        }

        public SimpleDialog AddNumber(string key, string label, decimal value, decimal min, decimal max, int decimals = 0)
        {
            var n = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimals,
                Value = Math.Max(min, Math.Min(max, value)),
                Width = 100,
                Increment = decimals > 0 ? 0.1m : 1m
            };
            _controls[key] = n;
            AddRow(label, n);
            return this;
        }

        public SimpleDialog AddCheck(string key, string label, bool value)
        {
            var c = new CheckBox { Text = label, Checked = value, AutoSize = true };
            _controls[key] = c;
            AddRow(null, c);
            return this;
        }

        public SimpleDialog AddCombo(string key, string label, IEnumerable<object> items, int selected)
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            foreach (object o in items)
            {
                c.Items.Add(o);
            }

            if (c.Items.Count > 0)
            {
                c.SelectedIndex = Math.Max(0, Math.Min(c.Items.Count - 1, selected));
            }

            _controls[key] = c;
            AddRow(label, c);
            return this;
        }

        public SimpleDialog AddInfo(string text)
        {
            var l = new Label { Text = text, AutoSize = true, MaximumSize = new Size(420, 0), ForeColor = SystemColors.GrayText, Margin = new Padding(3, 6, 3, 6) };
            AddRow(null, l);
            return this;
        }

        public bool ShowOk(IWin32Window owner)
        {
            var panel = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Fill };
            var cancel = new Button { Text = L.T("Abbrechen", "Cancel"), DialogResult = DialogResult.Cancel, AutoSize = true };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
            panel.Controls.Add(cancel);
            panel.Controls.Add(ok);
            AddRow(null, panel);
            AcceptButton = ok;
            CancelButton = cancel;
            return ShowDialog(owner) == DialogResult.OK;
        }

        public string GetText(string key) => ((TextBox)_controls[key]).Text;

        public int GetInt(string key) => (int)((NumericUpDown)_controls[key]).Value;

        public float GetFloat(string key) => (float)((NumericUpDown)_controls[key]).Value;

        public bool GetCheck(string key) => ((CheckBox)_controls[key]).Checked;

        public int GetIndex(string key) => ((ComboBox)_controls[key]).SelectedIndex;

        /// <summary>
        /// Parses "0x1234", "1234" (decimal) or "h1234"
        /// </summary>
        public static bool TryParseId(string text, out int value)
        {
            value = -1;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public static string AskText(IWin32Window owner, string title, string label, string value)
        {
            var d = new SimpleDialog(title).AddText("v", label, value);
            return d.ShowOk(owner) ? d.GetText("v") : null;
        }

        public static int? AskNumber(IWin32Window owner, string title, string label, int value, int min, int max)
        {
            var d = new SimpleDialog(title).AddNumber("v", label, value, min, max);
            return d.ShowOk(owner) ? d.GetInt("v") : (int?)null;
        }
    }

    /// <summary>
    /// Renders a list of tiles as small isometric preview
    /// </summary>
    public static class MultiPreview
    {
        public static Bitmap Render(IList<ImportTile> tiles, int maxWidth, int maxHeight)
        {
            return Render(tiles, maxWidth, maxHeight, null);
        }

        private static readonly Dictionary<int, System.Drawing.Imaging.ImageAttributes> _tints = new Dictionary<int, System.Drawing.Imaging.ImageAttributes>();

        private static System.Drawing.Imaging.ImageAttributes Tint(Color c)
        {
            if (_tints.TryGetValue(c.ToArgb(), out System.Drawing.Imaging.ImageAttributes ia))
            {
                return ia;
            }

            const float k = 0.55f;
            var m = new System.Drawing.Imaging.ColorMatrix(new[]
            {
                new[] { 1 - k, 0f, 0f, 0f, 0f },
                new[] { 0f, 1 - k, 0f, 0f, 0f },
                new[] { 0f, 0f, 1 - k, 0f, 0f },
                new[] { 0f, 0f, 0f, 1f, 0f },
                new[] { k * c.R / 255f, k * c.G / 255f, k * c.B / 255f, 0f, 1f }
            });
            ia = new System.Drawing.Imaging.ImageAttributes();
            ia.SetColorMatrix(m);
            _tints[c.ToArgb()] = ia;
            return ia;
        }

        /// <summary>
        /// Render with an optional tint per tile (to show what was learned)
        /// </summary>
        public static Bitmap Render(IList<ImportTile> tiles, int maxWidth, int maxHeight, Func<ImportTile, Color?> tint)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return null;
            }

            var items = new List<(ImportTile t, Bitmap b, int px, int py)>();
            foreach (ImportTile t in tiles)
            {
                Bitmap b = Art.GetStatic(t.Id);
                if (b == null)
                {
                    continue;
                }

                if (t.Hue > 0)
                {
                    b = MultiTile.GetHuedBitmap(t.Id, t.Hue, b);
                }

                int px = ((t.X - t.Y) * 22) - (b.Width / 2);
                int py = ((t.X + t.Y) * 22) - (t.Z * 4) - b.Height;
                items.Add((t, b, px, py));
            }

            if (items.Count == 0)
            {
                return null;
            }

            int minX = items.Min(i => i.px);
            int minY = items.Min(i => i.py);
            int maxX = items.Max(i => i.px + i.b.Width);
            int maxY = items.Max(i => i.py + i.b.Height);
            int w = Math.Max(1, maxX - minX);
            int h = Math.Max(1, maxY - minY);

            var full = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(full))
            {
                g.Clear(Color.Transparent);
                foreach (var i in items
                             .OrderBy(i => i.t.X + i.t.Y)
                             .ThenBy(i => i.t.Z + (TileData.ItemTable[i.t.Id].Background ? 0 : 1) + (TileData.ItemTable[i.t.Id].Height > 0 ? 1 : 0))
                             .ThenBy(i => i.t.X))
                {
                    Color? c = tint?.Invoke(i.t);
                    if (c != null)
                    {
                        g.DrawImage(i.b, new Rectangle(i.px - minX, i.py - minY, i.b.Width, i.b.Height), 0, 0, i.b.Width, i.b.Height, GraphicsUnit.Pixel, Tint(c.Value));
                    }
                    else
                    {
                        g.DrawImageUnscaled(i.b, i.px - minX, i.py - minY);
                    }
                }
            }

            double scale = Math.Min(1.0, Math.Min((double)maxWidth / w, (double)maxHeight / h));
            if (scale >= 1.0)
            {
                return full;
            }

            var scaled = new Bitmap(Math.Max(1, (int)(w * scale)), Math.Max(1, (int)(h * scale)));
            using (Graphics g = Graphics.FromImage(scaled))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(full, 0, 0, scaled.Width, scaled.Height);
            }

            full.Dispose();
            return scaled;
        }
    }
}
