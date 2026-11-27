/***************************************************************************
 *
 *  * Dragon Map Studio – "Bereich per Koordinaten": mark an area by numbers (corner 1 / corner 2 or start + size),
 *  *  for areas larger than the screen (a whole town of an old map).
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public static class AreaDialog
    {
        /// <summary>Asks for an area. <paramref name="mouse"/> = the tile under the mouse (for "corner = mouse"). Null = cancelled.</summary>
        public static Rectangle? Ask(IWin32Window owner, Rectangle current, Size mapSize, Point mouse)
        {
            using var f = new Form
            {
                Text = L.T("Bereich per Koordinaten"), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent, ClientSize = new Size(470, 268), Font = new Font("Segoe UI", 9f)
            };
            NumericUpDown N(int left, int top, int max, int value)
            {
                var n = new NumericUpDown { Left = left, Top = top, Width = 80, Minimum = 0, Maximum = Math.Max(1, max), Value = Math.Clamp(value, 0, Math.Max(1, max)) };
                f.Controls.Add(n);
                return n;
            }
            void Lbl(string text, int left, int top, int width = 90) => f.Controls.Add(new Label { Text = L.T(text), Left = left, Top = top + 3, Width = width });
            Rectangle r = current.IsEmpty ? new Rectangle(Math.Max(0, mouse.X), Math.Max(0, mouse.Y), 64, 64) : current;
            int mw = mapSize.Width - 1, mh = mapSize.Height - 1;
            f.Controls.Add(new Label
            {
                Left = 12, Top = 8, Width = 446, Height = 34, ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Ecke 1 = oben links (kleinste X / Y), Ecke 2 = unten rechts (einschließlich). Größe und Ecke 2 hängen zusammen.") + $" " + L.T("Karte") + $" {mapSize.Width}×{mapSize.Height}."
            });
            Lbl("Ecke 1  X", 12, 50);
            NumericUpDown x1 = N(100, 50, mw, r.X);
            Lbl("Y", 190, 50, 20);
            NumericUpDown y1 = N(212, 50, mh, r.Y);
            var m1 = new Button { Left = 302, Top = 48, Width = 156, Text = L.T("= Mauszeiger") + $" ({mouse.X}, {mouse.Y})" };
            f.Controls.Add(m1);
            Lbl("Ecke 2  X", 12, 84);
            NumericUpDown x2 = N(100, 84, mw, r.Right - 1);
            Lbl("Y", 190, 84, 20);
            NumericUpDown y2 = N(212, 84, mh, r.Bottom - 1);
            var m2 = new Button { Left = 302, Top = 82, Width = 156, Text = L.T("= Mauszeiger") + $" ({mouse.X}, {mouse.Y})" };
            f.Controls.Add(m2);
            Lbl("Breite", 12, 124);
            NumericUpDown w = N(100, 124, mapSize.Width, r.Width);
            Lbl("Höhe", 190, 124, 40);
            NumericUpDown h = N(232, 124, mapSize.Height, r.Height);
            w.Minimum = 1;
            h.Minimum = 1;
            var info = new Label { Left = 12, Top = 160, Width = 446, Height = 40, ForeColor = Color.FromArgb(30, 64, 175) };
            f.Controls.Add(info);
            bool sync = false;
            void FromCorners()
            {
                if (sync)
                {
                    return;
                }
                sync = true;
                w.Value = Math.Clamp(Math.Abs(x2.Value - x1.Value) + 1, w.Minimum, w.Maximum);
                h.Value = Math.Clamp(Math.Abs(y2.Value - y1.Value) + 1, h.Minimum, h.Maximum);
                sync = false;
                Info();
            }
            void FromSize()
            {
                if (sync)
                {
                    return;
                }
                sync = true;
                x2.Value = Math.Clamp(x1.Value + w.Value - 1, 0, x2.Maximum);
                y2.Value = Math.Clamp(y1.Value + h.Value - 1, 0, y2.Maximum);
                sync = false;
                Info();
            }
            void Info()
            {
                long cells = (long)w.Value * (long)h.Value;
                info.Text = $"{w.Value}×{h.Value} = {cells:N0} " + L.T("Felder") +
                            (cells > 1024 * 1024 ? " – " + L.T("sehr groß: Stempel / Vorlage werden langsam und groß (über 1024×1024 geht es nicht)") : string.Empty);
            }
            x1.ValueChanged += (s, e) => FromCorners();
            y1.ValueChanged += (s, e) => FromCorners();
            x2.ValueChanged += (s, e) => FromCorners();
            y2.ValueChanged += (s, e) => FromCorners();
            w.ValueChanged += (s, e) => FromSize();
            h.ValueChanged += (s, e) => FromSize();
            m1.Click += (s, e) =>
            {
                x1.Value = Math.Clamp(mouse.X, 0, mw);
                y1.Value = Math.Clamp(mouse.Y, 0, mh);
            };
            m2.Click += (s, e) =>
            {
                x2.Value = Math.Clamp(mouse.X, 0, mw);
                y2.Value = Math.Clamp(mouse.Y, 0, mh);
            };
            Info();
            var ok = new Button { Text = L.T("Markieren"), Left = 272, Top = 226, Width = 90, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = L.T("Abbrechen"), Left = 368, Top = 226, Width = 90, DialogResult = DialogResult.Cancel };
            f.Controls.Add(ok);
            f.Controls.Add(cancel);
            f.AcceptButton = ok;
            f.CancelButton = cancel;
            if (f.ShowDialog(owner) != DialogResult.OK)
            {
                return null;
            }
            int ax = (int)Math.Min(x1.Value, x2.Value), ay = (int)Math.Min(y1.Value, y2.Value);
            int bx = (int)Math.Max(x1.Value, x2.Value), by = (int)Math.Max(y1.Value, y2.Value);
            return Rectangle.FromLTRB(ax, ay, bx + 1, by + 1);
        }
    }
}
