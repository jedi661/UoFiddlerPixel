/***************************************************************************
 *
 *  * Dragon Map Studio – choose a hue: every hue of hues.mul with its colour strip, search, favourites
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class HuePickerForm : Form
    {
        private readonly ListBox _list = new ListBox { Dock = DockStyle.Fill, DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 22, IntegralHeight = false };
        private readonly TextBox _search = new TextBox { Width = 160 };
        private readonly CheckBox _onlyFav = new CheckBox { Text = L.T("nur Favoriten"), AutoSize = true, Margin = new Padding(8, 5, 0, 0) };
        private readonly List<ushort> _favorites;
        private readonly int _count;

        public HuePickerForm(ushort current, List<ushort> favorites)
        {
            _favorites = favorites ?? new List<ushort>();
            Text = L.T("Hue wählen");
            Size = new Size(520, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Font = new Font("Segoe UI", 9f);
            try
            {
                _count = Hues.List?.Length ?? 3000;
            }
            catch (Exception)
            {
                _count = 3000;
            }
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
            top.Controls.Add(new Label { Text = "🔍", AutoSize = true, Margin = new Padding(0, 5, 0, 0) });
            top.Controls.Add(_search);
            top.Controls.Add(_onlyFav);
            var none = new Button { Text = L.T("Hue 0 (Original)"), AutoSize = true, FlatStyle = FlatStyle.System };
            none.Click += (s, e) =>
            {
                Hue = 0;
                DialogResult = DialogResult.OK;
            };
            top.Controls.Add(none);
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80 };
            var cancel = new Button { Text = L.T("Abbrechen"), DialogResult = DialogResult.Cancel, Width = 80 };
            var fav = new Button { Text = "★ " + L.T("merken / vergessen"), AutoSize = true };
            fav.Click += (s, e) =>
            {
                if (_list.SelectedItem is ushort h)
                {
                    if (!_favorites.Remove(h))
                    {
                        _favorites.Add(h);
                    }
                    _list.Invalidate();
                }
            };
            bottom.Controls.AddRange(new Control[] { cancel, ok, fav });
            AcceptButton = ok;
            CancelButton = cancel;
            _list.DrawItem += DrawHue;
            _list.DoubleClick += (s, e) => DialogResult = DialogResult.OK;
            _list.SelectedIndexChanged += (s, e) =>
            {
                if (_list.SelectedItem is ushort h)
                {
                    Hue = h;
                }
            };
            _search.TextChanged += (s, e) => Fill();
            _onlyFav.CheckedChanged += (s, e) => Fill();
            Controls.Add(_list);
            Controls.Add(top);
            Controls.Add(bottom);
            Hue = current;
            Fill();
        }

        public ushort Hue { get; private set; }

        private void Fill()
        {
            string q = _search.Text.Trim().ToLowerInvariant();
            _list.BeginUpdate();
            _list.Items.Clear();
            IEnumerable<int> ids = _onlyFav.Checked ? _favorites.Select(f => (int)f) : Enumerable.Range(1, Math.Max(1, _count));
            foreach (int h in ids)
            {
                if (q.Length > 0)
                {
                    string name = UoArtCache.HueName(h).ToLowerInvariant();
                    if (!h.ToString().Contains(q) && !h.ToString("x").Contains(q) && !name.Contains(q))
                    {
                        continue;
                    }
                }
                _list.Items.Add((ushort)h);
            }
            _list.EndUpdate();
            int i = _list.Items.IndexOf(Hue);
            if (i >= 0)
            {
                _list.SelectedIndex = i;
            }
        }

        private void DrawHue(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0)
            {
                return;
            }
            var h = (ushort)_list.Items[e.Index];
            Graphics g = e.Graphics;
            Rectangle r = e.Bounds;
            bool fav = _favorites.Contains(h);
            using var text = new SolidBrush(e.ForeColor);
            g.DrawString((fav ? "★ " : "   ") + h.ToString().PadLeft(4) + $"  0x{h:x4}", Font, text, r.X + 2, r.Y + 3);
            // colour strip: 32 shades of the hue
            int x0 = r.X + 140, w = 160;
            try
            {
                Hue hue = Hues.GetHue(h - 1);
                for (int k = 0; k < 32; k++)
                {
                    using var b = new SolidBrush(hue.GetColor(k));
                    g.FillRectangle(b, x0 + k * w / 32, r.Y + 3, w / 32 + 1, r.Height - 6);
                }
            }
            catch (Exception)
            {
                g.FillRectangle(Brushes.Gray, x0, r.Y + 3, w, r.Height - 6);
            }
            g.DrawRectangle(Pens.Black, x0, r.Y + 3, w, r.Height - 6);
            g.DrawString(UoArtCache.HueName(h), Font, text, x0 + w + 8, r.Y + 3);
            e.DrawFocusRectangle();
        }
    }
}
