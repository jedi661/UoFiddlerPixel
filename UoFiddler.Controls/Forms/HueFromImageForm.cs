/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * New hue: colours of an image area, sorted by brightness.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class HueFromImageForm : Form
    {
        private Bitmap _bitmap;
        private Rectangle _selection; // image coordinates, empty = whole image
        private Point _dragStart;
        private bool _dragging;

        /// <summary>Only for the designer</summary>
        public HueFromImageForm()
            : this(string.Empty)
        {
        }

        public HueFromImageForm(string name)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _txtName.Text = name ?? string.Empty;
        }

        /// <summary>32 colours (dark to bright)</summary>
        public ushort[] Result { get; private set; } = new ushort[32];

        public string HueName => _txtName.Text.Trim();

        private void SetResult(ushort[] colors)
        {
            Result = colors;
            _strip.Invalidate();
            _preview.Colors = colors;
        }

        private void OnPaintStrip(object sender, PaintEventArgs e)
        {
            HueAnalysis.DrawStrip(e.Graphics, new Rectangle(8, 4, _strip.Width - 16, _strip.Height - 8), Result);
        }

        private void OnStripResize(object sender, EventArgs e) => _strip.Invalidate();

        private void OnClickOpen(object sender, EventArgs e) => OpenImage();

        private void OnClickPaste(object sender, EventArgs e) => PasteImage();

        private void OnClickWhole(object sender, EventArgs e)
        {
            _selection = Rectangle.Empty;
            _image.Invalidate();
            Recalc();
        }

        private void OnSettingChanged(object sender, EventArgs e) => Recalc();

        private void OnImageMouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _dragStart = ToImage(e.Location);
        }

        private void OnImageMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            Point p = ToImage(e.Location);
            _selection = Rectangle.FromLTRB(Math.Min(p.X, _dragStart.X), Math.Min(p.Y, _dragStart.Y), Math.Max(p.X, _dragStart.X) + 1, Math.Max(p.Y, _dragStart.Y) + 1);
            _image.Invalidate();
        }

        private void OnImageMouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
            Recalc();
        }

        private void OnImageResize(object sender, EventArgs e) => _image.Invalidate();

        private void OpenImage()
        {
            using (var dlg = new OpenFileDialog { Filter = "Images|*.png;*.bmp;*.jpg;*.jpeg;*.gif;*.tif;*.tiff|All files|*.*" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var img = Image.FromFile(dlg.FileName))
                    {
                        SetImage(new Bitmap(img));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PasteImage()
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show(this, "There is no image in the clipboard.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (Image img = Clipboard.GetImage())
            {
                SetImage(new Bitmap(img));
            }
        }

        private void SetImage(Bitmap bmp)
        {
            _bitmap?.Dispose();
            _bitmap = bmp;
            _selection = Rectangle.Empty;
            _image.Invalidate();
            Recalc();
        }

        private Rectangle ImageRect()
        {
            if (_bitmap == null)
            {
                return Rectangle.Empty;
            }

            float scale = Math.Min(_image.Width / (float)_bitmap.Width, _image.Height / (float)_bitmap.Height);
            int w = (int)(_bitmap.Width * scale), h = (int)(_bitmap.Height * scale);
            return new Rectangle((_image.Width - w) / 2, (_image.Height - h) / 2, w, h);
        }

        private Point ToImage(Point p)
        {
            Rectangle r = ImageRect();
            if (_bitmap == null || r.Width == 0)
            {
                return Point.Empty;
            }

            int x = (int)((p.X - r.X) * (_bitmap.Width / (float)r.Width));
            int y = (int)((p.Y - r.Y) * (_bitmap.Height / (float)r.Height));
            return new Point(Math.Max(0, Math.Min(_bitmap.Width - 1, x)), Math.Max(0, Math.Min(_bitmap.Height - 1, y)));
        }

        private void OnPaintImage(object sender, PaintEventArgs e)
        {
            if (_bitmap == null)
            {
                TextRenderer.DrawText(e.Graphics, "open an image or paste one from the clipboard", Font, _image.ClientRectangle, Color.Gray,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            Rectangle r = ImageRect();
            e.Graphics.DrawImage(_bitmap, r);
            if (!_selection.IsEmpty)
            {
                float sx = r.Width / (float)_bitmap.Width, sy = r.Height / (float)_bitmap.Height;
                var sel = new RectangleF(r.X + (_selection.X * sx), r.Y + (_selection.Y * sy), _selection.Width * sx, _selection.Height * sy);
                using (var pen = new Pen(Color.DeepSkyBlue, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    e.Graphics.DrawRectangle(pen, sel.X, sel.Y, sel.Width, sel.Height);
                }
            }
        }

        private void Recalc()
        {
            if (_bitmap == null)
            {
                return;
            }

            Rectangle area = _selection.IsEmpty ? new Rectangle(0, 0, _bitmap.Width, _bitmap.Height) : _selection;
            area.Intersect(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height));

            // at most ~40000 samples
            int step = Math.Max(1, (int)Math.Sqrt(area.Width * (double)area.Height / 40000));
            var pixels = new List<Color>();
            BitmapData data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                int stride = data.Stride / 4;
                var buffer = new int[stride * _bitmap.Height];
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                for (int y = area.Top; y < area.Bottom; y += step)
                {
                    for (int x = area.Left; x < area.Right; x += step)
                    {
                        Color c = Color.FromArgb(buffer[(y * stride) + x]);
                        if (_chkIgnoreDark.Checked && (c.A < 128 || (c.R < 8 && c.G < 8 && c.B < 8)))
                        {
                            continue;
                        }

                        pixels.Add(c);
                    }
                }
            }
            finally
            {
                _bitmap.UnlockBits(data);
            }

            SetResult(HueAnalysis.FromPixels(pixels, _trackSmooth.Value));
        }
    }
}
