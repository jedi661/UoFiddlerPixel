/***************************************************************************
 *
 *  * Dragon Map Studio – templates with pictures: workshop pieces (dungeons, cellars, rooms) and bridge plans
 *  *
 *  *  Every template as a small picture from above (land colour + colour of the top static), grouped by category,
 *  *  search, double click = take it.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class TemplateBrowserForm : Form
    {
        private const int Thumb = 128;
        private readonly ListView _list = new ListView { Dock = DockStyle.Fill, View = View.LargeIcon, MultiSelect = false };
        private readonly ImageList _images = new ImageList { ImageSize = new Size(Thumb, Thumb), ColorDepth = ColorDepth.Depth24Bit };
        private readonly TextBox _search = new TextBox { Width = 200 };
        private readonly bool _bridges;
        private readonly List<(string title, string category, object item, Func<Bitmap> picture)> _all = new List<(string, string, object, Func<Bitmap>)>();

        /// <param name="bridges">true = bridge plans, false = workshop pieces.</param>
        public TemplateBrowserForm(bool bridges)
        {
            _bridges = bridges;
            Text = L.T(bridges ? "Brücken-Pläne" : "Werkstatt-Vorlagen") + " – " + L.T("mit Bild");
            Size = new Size(900, 640);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            _list.LargeImageList = _images;
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
            top.Controls.Add(new Label { Text = "🔍", AutoSize = true, Margin = new Padding(0, 6, 0, 0) });
            top.Controls.Add(_search);
            top.Controls.Add(new Label { Text = L.T("Doppelklick = übernehmen"), AutoSize = true, Margin = new Padding(12, 6, 0, 0), ForeColor = Color.FromArgb(100, 116, 139) });
            var ok = new Button { Text = L.T("Übernehmen"), Dock = DockStyle.Bottom, Height = 30 };
            ok.Click += (s, e) => Take();
            _list.DoubleClick += (s, e) => Take();
            _search.TextChanged += (s, e) => Fill();
            Controls.Add(_list);
            Controls.Add(top);
            Controls.Add(ok);
            LoadItems();
            Fill();
        }

        /// <summary>Path of the workshop piece, or the bridge plan.</summary>
        public object Selected { get; private set; }

        private void LoadItems()
        {
            if (_bridges)
            {
                foreach (BridgeTemplate t in BridgeKit.LoadAll())
                {
                    BridgeTemplate c = t;
                    _all.Add((t.Name, t.Category, t, () => BridgePicture(c)));
                }
                return;
            }
            foreach ((string path, string title) in WorkshopPiece.List())
            {
                string p = path;
                string[] parts = title.Split(new[] { " / " }, 2, StringSplitOptions.None);
                _all.Add((parts.Length > 1 ? parts[1] : title, parts[0], p, () =>
                {
                    try
                    {
                        return PiecePicture(WorkshopPiece.Load(p));
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }));
            }
        }

        private void Fill()
        {
            string q = _search.Text.Trim().ToLowerInvariant();
            _list.BeginUpdate();
            _list.Items.Clear();
            _list.Groups.Clear();
            _images.Images.Clear();
            var groups = new Dictionary<string, ListViewGroup>();
            foreach ((string title, string category, object item, Func<Bitmap> picture) in _all.OrderBy(a => a.category).ThenBy(a => a.title))
            {
                if (q.Length > 0 && !title.ToLowerInvariant().Contains(q) && !category.ToLowerInvariant().Contains(q))
                {
                    continue;
                }
                if (!groups.TryGetValue(category, out ListViewGroup g))
                {
                    groups[category] = g = new ListViewGroup(category);
                    _list.Groups.Add(g);
                }
                Bitmap bmp = picture() ?? new Bitmap(Thumb, Thumb);
                _images.Images.Add(bmp);
                _list.Items.Add(new ListViewItem(title, _images.Images.Count - 1) { Group = g, Tag = item });
            }
            _list.EndUpdate();
        }

        private void Take()
        {
            if (_list.SelectedItems.Count == 0)
            {
                return;
            }
            Selected = _list.SelectedItems[0].Tag;
            DialogResult = DialogResult.OK;
        }

        // ------------------------------------------------------------------ pictures from above

        private static Bitmap Square(int w, int h, Func<int, int, Color> colour)
        {
            var bmp = new Bitmap(Thumb, Thumb);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(24, 24, 27));
            float k = Math.Min((float)Thumb / Math.Max(1, w), (float)Thumb / Math.Max(1, h));
            float ox = (Thumb - w * k) / 2, oy = (Thumb - h * k) / 2;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = colour(x, y);
                    if (c.A == 0)
                    {
                        continue;
                    }
                    using var b = new SolidBrush(c);
                    g.FillRectangle(b, ox + x * k, oy + y * k, Math.Max(1, k), Math.Max(1, k));
                }
            }
            return bmp;
        }

        public static Bitmap PiecePicture(WorkshopPiece p)
        {
            var top = p.Statics.GroupBy(s => s.Y * p.Width + s.X).ToDictionary(g => g.Key, g => g.OrderBy(s => s.Z).Last().Id);
            return Square(p.Width, p.Height, (x, y) =>
            {
                int i = y * p.Width + x;
                if (top.TryGetValue(i, out ushort id))
                {
                    return UoArtCache.ItemColor(id);
                }
                ushort land = p.Land[i];
                return land <= 2 ? Color.Black : UoArtCache.LandColor(land);
            });
        }

        public static Bitmap BridgePicture(BridgeTemplate t)
        {
            List<BridgePart> parts = t.PartsX.Count > 0 ? t.PartsX : t.PartsY;
            int len = Math.Max(1, parts.Count == 0 ? t.Length : parts.Max(p => p.Along) + 1);
            int w = Math.Max(1, parts.Count == 0 ? t.Width : parts.Max(p => p.Across) + 1);
            var top = parts.GroupBy(p => (p.Along, p.Across)).ToDictionary(g => g.Key, g => g.OrderBy(p => p.Dz).Last().Id);
            // water around, the bridge in the middle
            return Square(len + 2, w + 4, (x, y) =>
            {
                if (top.TryGetValue((x - 1, y - 2), out ushort id))
                {
                    return UoArtCache.ItemColor(id);
                }
                return x == 0 || x == len + 1 ? Color.FromArgb(70, 120, 50) : Color.FromArgb(40, 80, 160);
            });
        }
    }
}
