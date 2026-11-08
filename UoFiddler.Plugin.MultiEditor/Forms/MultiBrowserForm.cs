/***************************************************************************
 *
 * MultiEditor – browse all multis of multi.mul as thumbnails
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Forms
{
    public sealed class MultiBrowserForm : Form
    {
        private readonly ListView _list;
        private readonly ImageList _images;
        private readonly TextBox _filter;
        private readonly ComboBox _size;
        private readonly Label _status;
        private readonly Timer _timer;
        private readonly Action<int> _open;
        private readonly Dictionary<int, string> _names;
        private readonly Dictionary<int, Bitmap> _thumbCache = new Dictionary<int, Bitmap>();
        private List<int> _ids = new List<int>();
        private int _loadIndex;
        private int _thumbSize = 96;

        public MultiBrowserForm(Dictionary<int, string> names, Action<int> open)
        {
            _names = names ?? new Dictionary<int, string>();
            _open = open;

            Text = L.T("Multi-Browser (multi.mul)", "Multi browser (multi.mul)");
            Size = new Size(1000, 700);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
            _filter = new TextBox { Width = 200 };
            _size = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };
            _size.Items.AddRange(new object[] { "64", "96", "128", "192" });
            _size.SelectedIndex = 1;
            _status = new Label { AutoSize = true, Margin = new Padding(12, 7, 3, 3) };
            top.Controls.Add(new Label { Text = L.T("Filter (ID / Name):", "Filter (id / name):"), AutoSize = true, Margin = new Padding(3, 7, 3, 3) });
            top.Controls.Add(_filter);
            top.Controls.Add(new Label { Text = L.T("Größe:", "Size:"), AutoSize = true, Margin = new Padding(12, 7, 3, 3) });
            top.Controls.Add(_size);
            top.Controls.Add(_status);

            _images = new ImageList { ImageSize = new Size(_thumbSize, _thumbSize), ColorDepth = ColorDepth.Depth32Bit };
            _list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.LargeIcon,
                LargeImageList = _images,
                MultiSelect = false,
                ShowItemToolTips = true,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White
            };
            _list.ItemActivate += (_, _) =>
            {
                if (_list.SelectedItems.Count > 0)
                {
                    _open((int)_list.SelectedItems[0].Tag);
                }
            };

            var bottom = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                ForeColor = SystemColors.GrayText,
                Text = L.T("Doppelklick = im Editor öffnen", "Double click = open in the editor")
            };

            Controls.Add(_list);
            Controls.Add(bottom);
            Controls.Add(top);

            _timer = new Timer { Interval = 15 };
            _timer.Tick += (_, _) => LoadSome();
            _filter.TextChanged += (_, _) => Rebuild();
            _size.SelectedIndexChanged += (_, _) =>
            {
                _thumbSize = int.Parse(_size.SelectedItem.ToString());
                ClearThumbs();
                _images.ImageSize = new Size(_thumbSize, _thumbSize);
                Rebuild();
            };
            FormClosed += (_, _) =>
            {
                _timer.Stop();
                ClearThumbs();
            };

            Rebuild();
        }

        private void ClearThumbs()
        {
            foreach (Bitmap b in _thumbCache.Values)
            {
                b.Dispose();
            }

            _thumbCache.Clear();
        }

        private void Rebuild()
        {
            _timer.Stop();
            string term = _filter.Text.Trim();
            _ids = new List<int>();
            for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
            {
                if (Multis.GetComponents(i) == MultiComponentList.Empty)
                {
                    continue;
                }

                if (term.Length > 0)
                {
                    _names.TryGetValue(i, out string name);
                    bool match = i.ToString().StartsWith(term, StringComparison.Ordinal) ||
                                 $"0x{i:X}".StartsWith(term, StringComparison.OrdinalIgnoreCase) ||
                                 (name?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!match)
                    {
                        continue;
                    }
                }

                _ids.Add(i);
            }

            _list.BeginUpdate();
            _list.Items.Clear();
            _images.Images.Clear();
            _images.Images.Add(Placeholder());
            foreach (int id in _ids)
            {
                _names.TryGetValue(id, out string name);
                MultiComponentList m = Multis.GetComponents(id);
                _list.Items.Add(new ListViewItem($"0x{id:X} {name}".Trim(), 0)
                {
                    Tag = id,
                    ToolTipText = $"{id} (0x{id:X}) {name}\n{m.Width} x {m.Height}, {m.SortedTiles.Length} " + L.T("Kacheln", "tiles")
                });
            }

            _list.EndUpdate();
            _loadIndex = 0;
            _status.Text = $"{_ids.Count} Multis";
            _timer.Start();
        }

        private Bitmap Placeholder()
        {
            var b = new Bitmap(_thumbSize, _thumbSize);
            using Graphics g = Graphics.FromImage(b);
            g.Clear(Color.FromArgb(60, 60, 64));
            return b;
        }

        private void LoadSome()
        {
            DateTime start = DateTime.Now;
            while (_loadIndex < _list.Items.Count && (DateTime.Now - start).TotalMilliseconds < 40)
            {
                ListViewItem item = _list.Items[_loadIndex++];
                int id = (int)item.Tag;
                if (!_thumbCache.TryGetValue(id, out Bitmap thumb))
                {
                    thumb = CreateThumb(id);
                    _thumbCache[id] = thumb;
                }

                _images.Images.Add(thumb);
                item.ImageIndex = _images.Images.Count - 1;
            }

            _status.Text = $"{_ids.Count} Multis" + (_loadIndex < _list.Items.Count ? $"  ({_loadIndex}/{_list.Items.Count})" : string.Empty);
            if (_loadIndex >= _list.Items.Count)
            {
                _timer.Stop();
            }
        }

        private Bitmap CreateThumb(int id)
        {
            var thumb = new Bitmap(_thumbSize, _thumbSize);
            try
            {
                using Bitmap full = Multis.GetComponents(id).GetImage();
                using Graphics g = Graphics.FromImage(thumb);
                g.Clear(Color.FromArgb(60, 60, 64));
                if (full != null)
                {
                    double scale = Math.Min(1.0, Math.Min((double)_thumbSize / full.Width, (double)_thumbSize / full.Height));
                    int w = Math.Max(1, (int)(full.Width * scale));
                    int h = Math.Max(1, (int)(full.Height * scale));
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(full, (_thumbSize - w) / 2, (_thumbSize - h) / 2, w, h);
                }
            }
            catch
            {
                // broken multi
            }

            return thumb;
        }
    }
}
