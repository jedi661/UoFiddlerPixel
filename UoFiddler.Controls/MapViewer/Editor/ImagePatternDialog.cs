/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.MapViewer.Editor.Tools;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Makes a brush pattern from a picture: 1 pixel = 1 map cell, every color gets a land tile
    /// (suggested by the radar color, changeable). Transparent pixels = empty cells.
    /// </summary>
    public class ImagePatternDialog : Form
    {
        public const int MaxSize = 64;

        private readonly LandPaletteControl _palette;
        private readonly ListView _colors = new ListView();
        private readonly ImageList _swatches = new ImageList { ImageSize = new Size(24, 16) };
        private readonly Label _info = new Label();
        private readonly PictureBox _picture = new PictureBox();
        private Bitmap _image;
        private readonly Dictionary<int, ushort?> _mapping = new Dictionary<int, ushort?>();
        private List<int> _colorOrder = new List<int>();

        public ImagePatternDialog(ArtPixelCache art)
        {
            Text = "Import picture as pattern";
            Icon = Options.GetFiddlerIcon();
            ClientSize = new Size(1000, 640);
            StartPosition = FormStartPosition.CenterParent;

            _palette = new LandPaletteControl(art) { Dock = DockStyle.Right, Width = 360 };

            var top = new Panel { Dock = DockStyle.Top, Height = 40 };
            var buttonOpen = new Button { Text = "Open picture...", Left = 6, Top = 7, Width = 120, Height = 26 };
            buttonOpen.Click += OnClickOpen;
            _info.SetBounds(134, 4, 500, 34);
            _info.Text = $"PNG/BMP/GIF, max. {MaxSize} x {MaxSize} pixels (larger pictures are scaled down). 1 pixel = 1 tile.";
            top.Controls.Add(buttonOpen);
            top.Controls.Add(_info);

            _picture.Dock = DockStyle.Left;
            _picture.Width = 200;
            _picture.SizeMode = PictureBoxSizeMode.Zoom;
            _picture.BackColor = Color.FromArgb(30, 30, 34);

            _colors.Dock = DockStyle.Fill;
            _colors.View = View.Details;
            _colors.FullRowSelect = true;
            _colors.SmallImageList = _swatches;
            _colors.Columns.Add("Color", 110);
            _colors.Columns.Add("Pixels", 60);
            _colors.Columns.Add("Tile", 90);
            _colors.Columns.Add("Name", 140);

            var assign = new Panel { Dock = DockStyle.Bottom, Height = 70 };
            var buttonAssign = new Button { Text = "Selected colors = palette tile(s)", Left = 6, Top = 6, Width = 230, Height = 26 };
            buttonAssign.Click += (s, e) => Assign(_palette.SelectedIds.Count > 0 ? _palette.SelectedIds[0] : (ushort?)null, true);
            var buttonEmpty = new Button { Text = "Selected colors = empty", Left = 242, Top = 6, Width = 170, Height = 26 };
            buttonEmpty.Click += (s, e) => Assign(null, false);
            var buttonAuto = new Button { Text = "Suggest again (radar colors)", Left = 6, Top = 36, Width = 230, Height = 26 };
            buttonAuto.Click += (s, e) => Suggest();
            assign.Controls.AddRange(new Control[] { buttonAssign, buttonEmpty, buttonAuto });

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 38, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
            var buttonCancel = new Button { Text = "Cancel", Width = 90, DialogResult = DialogResult.Cancel };
            var buttonOk = new Button { Text = "Create pattern", Width = 120 };
            buttonOk.Click += (s, e) =>
            {
                Result = Build();
                if (Result != null)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            };
            bottom.Controls.Add(buttonCancel);
            bottom.Controls.Add(buttonOk);
            CancelButton = buttonCancel;

            var middle = new Panel { Dock = DockStyle.Fill };
            middle.Controls.Add(_colors);
            middle.Controls.Add(assign);
            middle.Controls.Add(_picture);

            Controls.Add(middle);
            Controls.Add(_palette);
            Controls.Add(top);
            Controls.Add(bottom);

            Shown += (s, e) => _palette.Reload();
            FormClosed += (s, e) =>
            {
                _image?.Dispose();
                _swatches.Dispose();
            };
        }

        public BrushPattern Result { get; private set; }

        private void OnClickOpen(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "Pictures (*.png;*.bmp;*.gif;*.jpg)|*.png;*.bmp;*.gif;*.jpg|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var loaded = new Bitmap(dialog.FileName))
                    {
                        _image?.Dispose();
                        _image = Shrink(loaded);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The picture can not be read:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            _picture.Image = _image;
            ReadColors();
            Suggest();
        }

        /// <summary>Scales a picture down to max. 64 x 64 (nearest pixel, no new colors)</summary>
        private static Bitmap Shrink(Bitmap src)
        {
            double scale = Math.Min(1.0, Math.Min(MaxSize / (double)src.Width, MaxSize / (double)src.Height));
            int w = Math.Max(1, (int)Math.Round(src.Width * scale)), h = Math.Max(1, (int)Math.Round(src.Height * scale));
            var result = new Bitmap(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    result.SetPixel(x, y, src.GetPixel(Math.Min(src.Width - 1, (int)(x / scale)), Math.Min(src.Height - 1, (int)(y / scale))));
                }
            }

            return result;
        }

        private static int Key(Color c) => c.A < 128 ? -1 : c.ToArgb() & 0xFFFFFF;

        private void ReadColors()
        {
            var counts = new Dictionary<int, int>();
            for (int y = 0; y < _image.Height; y++)
            {
                for (int x = 0; x < _image.Width; x++)
                {
                    int k = Key(_image.GetPixel(x, y));
                    counts.TryGetValue(k, out int n);
                    counts[k] = n + 1;
                }
            }

            _colorOrder = counts.OrderByDescending(p => p.Value).Select(p => p.Key).ToList();
            _mapping.Clear();
            _swatches.Images.Clear();
            _colors.Items.Clear();
            foreach (int k in _colorOrder)
            {
                var swatch = new Bitmap(24, 16);
                using (Graphics g = Graphics.FromImage(swatch))
                {
                    if (k < 0)
                    {
                        g.Clear(Color.White);
                        g.DrawLine(Pens.Red, 0, 0, 24, 16);
                    }
                    else
                    {
                        g.Clear(Color.FromArgb(255, Color.FromArgb(k)));
                    }
                }

                _swatches.Images.Add(swatch);
                var item = new ListViewItem(k < 0 ? "transparent" : $"#{k:X6}", _swatches.Images.Count - 1) { Tag = k };
                item.SubItems.Add(counts[k].ToString());
                item.SubItems.Add("-");
                item.SubItems.Add(string.Empty);
                _colors.Items.Add(item);
            }

            _info.Text = $"{_image.Width} x {_image.Height} pixels, {_colorOrder.Count} colors. Select colors, choose a tile in the palette, \"Selected colors = palette tile\".";
        }

        /// <summary>Nearest land tile by radar color for every color</summary>
        private void Suggest()
        {
            if (_image == null)
            {
                return;
            }

            LandData[] table = TileData.LandTable;
            var candidates = new List<(ushort Id, int R, int G, int B)>();
            for (int id = 3; id < Math.Min(0x4000, table?.Length ?? 0); id++)
            {
                if (table[id].TextureId == 0)
                {
                    continue;
                }

                ushort c;
                try
                {
                    c = RadarCol.GetLandColor(id);
                }
                catch (Exception)
                {
                    continue;
                }

                candidates.Add(((ushort)id, ((c >> 10) & 0x1F) * 255 / 31, ((c >> 5) & 0x1F) * 255 / 31, (c & 0x1F) * 255 / 31));
            }

            foreach (int k in _colorOrder)
            {
                if (k < 0 || candidates.Count == 0)
                {
                    _mapping[k] = null;
                    continue;
                }

                int r = (k >> 16) & 0xFF, g = (k >> 8) & 0xFF, b = k & 0xFF;
                (ushort Id, int R, int G, int B) best = candidates.OrderBy(c => ((c.R - r) * (c.R - r)) + ((c.G - g) * (c.G - g)) + ((c.B - b) * (c.B - b))).First();
                _mapping[k] = best.Id;
            }

            UpdateList();
        }

        private void Assign(ushort? id, bool needTile)
        {
            if (needTile && id == null)
            {
                MessageBox.Show("Select a tile in the palette first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (ListViewItem item in _colors.SelectedItems)
            {
                _mapping[(int)item.Tag] = id;
            }

            UpdateList();
        }

        private void UpdateList()
        {
            LandData[] table = TileData.LandTable;
            foreach (ListViewItem item in _colors.Items)
            {
                _mapping.TryGetValue((int)item.Tag, out ushort? id);
                item.SubItems[2].Text = id.HasValue ? $"0x{id.Value:X4}" : "empty";
                item.SubItems[3].Text = id.HasValue && table != null && id.Value < table.Length ? table[id.Value].Name : string.Empty;
            }
        }

        private BrushPattern Build()
        {
            if (_image == null)
            {
                MessageBox.Show("Open a picture first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            var p = new BrushPattern(_image.Width, _image.Height) { SourceName = "picture" };
            for (int y = 0; y < _image.Height; y++)
            {
                for (int x = 0; x < _image.Width; x++)
                {
                    if (_mapping.TryGetValue(Key(_image.GetPixel(x, y)), out ushort? id) && id.HasValue)
                    {
                        p.SetId(x, y, id.Value);
                    }
                }
            }

            return p;
        }
    }
}
