/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Create hue colours:
 *  - HueGradientForm:  gradient through 2 or 3 colours (dark / middle / bright)
 *  - HueFromImageForm: colours of an image area, sorted by brightness
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    /// <summary>Base: name, strip, preview, OK/Cancel</summary>
    public abstract class HueCreateFormBase : Form
    {
        protected readonly TextBox NameBox = new TextBox { Width = 180, MaxLength = 20 };
        protected readonly PictureBox Strip = new PictureBox { Dock = DockStyle.Top, Height = 34 };
        protected readonly HuePreviewPanel Preview = new HuePreviewPanel { Dock = DockStyle.Fill };
        protected readonly Panel Settings = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };

        protected HueCreateFormBase(string title, string name)
        {
            Text = title;
            Icon = Options.GetFiddlerIcon();
            Size = new Size(640, 620);
            MinimumSize = new Size(520, 480);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            NameBox.Text = name ?? string.Empty;

            Strip.Paint += (s, e) => HueAnalysis.DrawStrip(e.Graphics, new Rectangle(8, 4, Strip.Width - 16, Strip.Height - 8), Result);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            var ok = new Button { Text = "Apply", DialogResult = DialogResult.OK, AutoSize = true };
            bottom.Controls.Add(cancel);
            bottom.Controls.Add(ok);
            bottom.Controls.Add(NameBox);
            bottom.Controls.Add(new Label { Text = "Name:", AutoSize = true, Margin = new Padding(3, 7, 0, 0) });
            AcceptButton = ok;
            CancelButton = cancel;

            var previewHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 8, 0) };
            previewHost.Controls.Add(Preview);

            Controls.Add(previewHost);
            Controls.Add(Strip);
            Controls.Add(Settings);
            Controls.Add(bottom);
        }

        /// <summary>32 colours (dark to bright)</summary>
        public ushort[] Result { get; protected set; } = new ushort[32];

        public string HueName => NameBox.Text.Trim();

        protected void SetResult(ushort[] colors)
        {
            Result = colors;
            Strip.Invalidate();
            Preview.Colors = colors;
        }
    }

    public sealed class HueGradientForm : HueCreateFormBase
    {
        private Color _dark, _middle, _bright;
        private readonly Button _btnDark, _btnMiddle, _btnBright;
        private readonly CheckBox _chkMiddle;
        private readonly TrackBar _trackGamma;
        private readonly Label _lblGamma;

        /// <param name="start">current colours (stops are taken from them) or null</param>
        public HueGradientForm(ushort[] start, string name) : base("New hue from a gradient", name)
        {
            bool empty = start == null || Array.TrueForAll(start, c => (c & 0x7FFF) == 0);
            _dark = empty ? Color.FromArgb(16, 8, 8) : HueAnalysis.ToColor(start[1]);
            _middle = empty ? Color.FromArgb(160, 40, 40) : HueAnalysis.ToColor(start[16]);
            _bright = empty ? Color.FromArgb(255, 220, 200) : HueAnalysis.ToColor(start[31]);

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
            _btnDark = ColorButton("Dark", () => _dark, c => _dark = c);
            _btnMiddle = ColorButton("Middle", () => _middle, c => _middle = c);
            _btnBright = ColorButton("Bright", () => _bright, c => _bright = c);
            _chkMiddle = new CheckBox { Text = "use middle colour", Checked = true, AutoSize = true, Margin = new Padding(6, 8, 3, 3) };
            _chkMiddle.CheckedChanged += (s, e) => { _btnMiddle.Enabled = _chkMiddle.Checked; Recalc(); };
            _trackGamma = new TrackBar { Minimum = 4, Maximum = 25, Value = 10, Width = 150, TickStyle = TickStyle.None, AutoSize = false, Height = 28 };
            _lblGamma = new Label { AutoSize = true, Margin = new Padding(0, 8, 3, 3) };
            _trackGamma.ValueChanged += (s, e) => Recalc();

            flow.Controls.AddRange(new Control[]
            {
                _btnDark, _btnMiddle, _btnBright, _chkMiddle,
                new Label { Text = "Curve:", AutoSize = true, Margin = new Padding(10, 8, 0, 3) }, _trackGamma, _lblGamma
            });
            Settings.Controls.Add(flow);
            Recalc();
        }

        private Button ColorButton(string text, Func<Color> get, Action<Color> set)
        {
            var b = new Button { Text = text, Width = 90, Height = 28, FlatStyle = FlatStyle.Flat };
            void Paint()
            {
                Color c = get();
                b.BackColor = c;
                b.ForeColor = c.GetBrightness() > 0.55f ? Color.Black : Color.White;
            }

            Paint();
            b.Click += (s, e) =>
            {
                using (var dlg = new ColorDialog { Color = get(), FullOpen = true })
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        set(dlg.Color);
                        Paint();
                        Recalc();
                    }
                }
            };
            return b;
        }

        private void Recalc()
        {
            double gamma = _trackGamma.Value / 10.0;
            _lblGamma.Text = gamma < 0.95 ? $"brighter ({gamma:0.0})" : gamma > 1.05 ? $"darker ({gamma:0.0})" : "linear";
            SetResult(HueAnalysis.Gradient(_dark, _chkMiddle.Checked ? _middle : (Color?)null, _bright, gamma));
        }
    }

    public sealed class HueFromImageForm : HueCreateFormBase
    {
        private readonly PictureBox _image;
        private readonly TrackBar _trackSmooth;
        private readonly CheckBox _chkIgnoreDark;
        private Bitmap _bitmap;
        private Rectangle _selection; // image coordinates, empty = whole image
        private Point _dragStart;
        private bool _dragging;

        public HueFromImageForm(string name) : base("New hue from an image", name)
        {
            Size = new Size(760, 760);

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
            var open = new Button { Text = "Open image…", AutoSize = true };
            open.Click += (s, e) => OpenImage();
            var paste = new Button { Text = "Paste from clipboard", AutoSize = true };
            paste.Click += (s, e) => PasteImage();
            var all = new Button { Text = "Whole image", AutoSize = true };
            all.Click += (s, e) => { _selection = Rectangle.Empty; _image.Invalidate(); Recalc(); };
            _chkIgnoreDark = new CheckBox { Text = "ignore black/transparent", Checked = true, AutoSize = true, Margin = new Padding(6, 8, 3, 3) };
            _chkIgnoreDark.CheckedChanged += (s, e) => Recalc();
            _trackSmooth = new TrackBar { Minimum = 0, Maximum = 4, Value = 1, Width = 100, TickStyle = TickStyle.None, AutoSize = false, Height = 28 };
            _trackSmooth.ValueChanged += (s, e) => Recalc();
            flow.Controls.AddRange(new Control[]
            {
                open, paste, all, _chkIgnoreDark, new Label { Text = "Smooth:", AutoSize = true, Margin = new Padding(8, 8, 0, 3) }, _trackSmooth
            });

            _image = new PictureBox { Dock = DockStyle.Top, Height = 260, BackColor = Color.FromArgb(40, 40, 44), Cursor = Cursors.Cross };
            _image.Paint += OnPaintImage;
            _image.MouseDown += (s, e) => { _dragging = true; _dragStart = ToImage(e.Location); };
            _image.MouseMove += (s, e) =>
            {
                if (!_dragging)
                {
                    return;
                }

                Point p = ToImage(e.Location);
                _selection = Rectangle.FromLTRB(Math.Min(p.X, _dragStart.X), Math.Min(p.Y, _dragStart.Y), Math.Max(p.X, _dragStart.X) + 1, Math.Max(p.Y, _dragStart.Y) + 1);
                _image.Invalidate();
            };
            _image.MouseUp += (s, e) => { _dragging = false; Recalc(); };
            _image.Resize += (s, e) => _image.Invalidate();

            Settings.Controls.Add(_image);
            Settings.Controls.Add(flow);
            Settings.Controls.Add(new Label
            {
                Text = "Mark an area in the image with the mouse (e.g. a piece of cloth). Its colours are sorted from dark to bright.",
                Dock = DockStyle.Top,
                Height = 34
            });
        }

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bitmap?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
