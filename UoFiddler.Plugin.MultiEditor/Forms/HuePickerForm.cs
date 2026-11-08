/***************************************************************************
 *
 * MultiEditor – hue picker with color preview and tile preview
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Forms
{
    public sealed class HuePickerForm : Form
    {
        private static readonly List<int> _recent = new List<int>();

        private readonly ListBox _list;
        private readonly TextBox _search;
        private readonly PictureBox _preview;
        private readonly Label _info;
        private readonly FlowLayoutPanel _recentPanel;
        private readonly ushort _tileId;
        private List<int> _items = new List<int>();

        public int SelectedHue { get; private set; }

        public HuePickerForm(int currentHue, ushort previewTile)
        {
            _tileId = previewTile;
            SelectedHue = currentHue;

            Text = L.T("Hue auswählen", "Choose hue");
            Size = new Size(640, 560);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimizeBox = false;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
            _search = new TextBox { Width = 180 };
            top.Controls.Add(new Label { Text = L.T("Suche (Nummer / Name):", "Search (number / name):"), AutoSize = true, Margin = new Padding(3, 7, 3, 3) });
            top.Controls.Add(_search);
            var none = new Button { Text = L.T("Keine (0)", "None (0)"), AutoSize = true };
            none.Click += (_, _) => Select(0);
            top.Controls.Add(none);

            _recentPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4, 0, 4, 4) };

            _list = new ListBox
            {
                Dock = DockStyle.Fill,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 20,
                IntegralHeight = false
            };
            _list.DrawItem += List_DrawItem;
            _list.SelectedIndexChanged += (_, _) =>
            {
                if (_list.SelectedIndex >= 0)
                {
                    SelectedHue = _items[_list.SelectedIndex];
                    UpdatePreview();
                }
            };
            _list.DoubleClick += (_, _) => DialogResult = DialogResult.OK;

            var right = new Panel { Dock = DockStyle.Right, Width = 200, Padding = new Padding(6) };
            _preview = new PictureBox { Dock = DockStyle.Top, Height = 220, SizeMode = PictureBoxSizeMode.CenterImage, BackColor = Color.FromArgb(40, 40, 40) };
            _info = new Label { Dock = DockStyle.Top, Height = 60 };
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = L.T("Abbrechen", "Cancel"), DialogResult = DialogResult.Cancel, AutoSize = true };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            right.Controls.Add(_info);
            right.Controls.Add(_preview);
            right.Controls.Add(buttons);
            AcceptButton = ok;
            CancelButton = cancel;

            Controls.Add(_list);
            Controls.Add(right);
            Controls.Add(_recentPanel);
            Controls.Add(top);

            _search.TextChanged += (_, _) => Filter();
            FormClosed += (_, _) =>
            {
                if (DialogResult == DialogResult.OK && SelectedHue > 0)
                {
                    _recent.Remove(SelectedHue);
                    _recent.Insert(0, SelectedHue);
                    if (_recent.Count > 16)
                    {
                        _recent.RemoveAt(_recent.Count - 1);
                    }
                }
            };

            BuildRecent();
            Filter();
            UpdatePreview();
        }

        private static Hue GetHue(int hue)
        {
            try
            {
                return hue > 0 ? Hues.GetHue(hue - 1) : null;
            }
            catch
            {
                return null;
            }
        }

        private void BuildRecent()
        {
            _recentPanel.Controls.Clear();
            if (_recent.Count == 0)
            {
                return;
            }

            _recentPanel.Controls.Add(new Label { Text = L.T("Zuletzt:", "Recent:"), AutoSize = true, Margin = new Padding(3, 6, 3, 3) });
            foreach (int h in _recent)
            {
                int hue = h;
                Hue hu = GetHue(hue);
                var b = new Button { Text = hue.ToString(), Width = 52, Height = 24, BackColor = hu?.GetColor(20) ?? SystemColors.Control };
                b.ForeColor = b.BackColor.GetBrightness() < 0.45f ? Color.White : Color.Black;
                b.Click += (_, _) => Select(hue);
                _recentPanel.Controls.Add(b);
            }
        }

        private void Filter()
        {
            string term = _search.Text.Trim();
            _items = new List<int>();
            int count = Hues.List?.Length ?? 0;
            for (int i = 1; i <= count; ++i)
            {
                if (term.Length > 0)
                {
                    Hue h = GetHue(i);
                    bool match = i.ToString().StartsWith(term, StringComparison.Ordinal) ||
                                 $"0x{i:X}".StartsWith(term, StringComparison.OrdinalIgnoreCase) ||
                                 (h?.Name?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!match)
                    {
                        continue;
                    }
                }

                _items.Add(i);
            }

            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (int i in _items)
            {
                _list.Items.Add(i);
            }

            _list.EndUpdate();
            int idx = _items.IndexOf(SelectedHue);
            if (idx >= 0)
            {
                _list.SelectedIndex = idx;
            }
        }

        private void Select(int hue)
        {
            SelectedHue = hue;
            int idx = _items.IndexOf(hue);
            if (idx >= 0)
            {
                _list.SelectedIndex = idx;
            }

            UpdatePreview();
        }

        private void List_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _items.Count)
            {
                return;
            }

            e.DrawBackground();
            int hueIndex = _items[e.Index];
            Hue hue = GetHue(hueIndex);
            var text = $"{hueIndex,5}  {hue?.Name}";
            using (var brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(text, e.Font, brush, e.Bounds.X + 2, e.Bounds.Y + 3);
            }

            if (hue == null)
            {
                return;
            }

            // gradient of the 32 colors
            int x0 = e.Bounds.X + 170;
            float w = Math.Max(1f, (e.Bounds.Width - 175) / 32f);
            for (int i = 0; i < 32; ++i)
            {
                using var b = new SolidBrush(hue.GetColor(i));
                e.Graphics.FillRectangle(b, x0 + (i * w), e.Bounds.Y + 2, w + 1, e.Bounds.Height - 4);
            }
        }

        private void UpdatePreview()
        {
            Hue hue = GetHue(SelectedHue);
            _info.Text = SelectedHue <= 0
                ? L.T("Keine Hue", "No hue")
                : $"Hue {SelectedHue} (0x{SelectedHue:X})\n{hue?.Name}";

            Image old = _preview.Image;
            _preview.Image = null;
            old?.Dispose();
            if (_tileId == 0xFFFF)
            {
                return;
            }

            Bitmap art = Art.GetStatic(_tileId);
            if (art == null)
            {
                return;
            }

            Bitmap shown = SelectedHue > 0 ? MultiTile.GetHuedBitmap(_tileId, SelectedHue, art) : art;
            _preview.Image = new Bitmap(shown);
        }
    }
}
