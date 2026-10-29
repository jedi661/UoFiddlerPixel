/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MultiMapImages
{
    /// <summary>
    /// Text for a label on the map: font, color, light border (halo) and rotation.
    /// </summary>
    public class MapTextDialog : Form
    {
        private static string _lastText = string.Empty;
        private static Font _lastFont = new Font("Georgia", 20f, FontStyle.Italic);
        private static bool _lastHalo = true;
        private static int _lastAngle;

        private readonly TextBox _text = new TextBox();
        private readonly Button _font = new Button();
        private readonly Button _color = new Button();
        private readonly CheckBox _halo = new CheckBox();
        private readonly NumericUpDown _angle = new NumericUpDown();
        private readonly PictureBox _preview = new PictureBox();
        private Font _selectedFont;
        private Color _textColor;
        private readonly Color _haloColor;

        public MapTextDialog(Color textColor, Color haloColor)
        {
            _textColor = textColor;
            _haloColor = haloColor;
            _selectedFont = (Font)_lastFont.Clone();

            Text = "Label";
            Icon = Options.GetFiddlerIcon();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 360);

            var labelText = new Label { Text = "Text (Enter = new line):", Left = 12, Top = 10, Width = 300 };
            _text.SetBounds(12, 30, 496, 70);
            _text.Multiline = true;
            _text.AcceptsReturn = true;
            _text.ScrollBars = ScrollBars.Vertical;
            _text.Text = _lastText;
            _text.TextChanged += (s, e) => UpdatePreview();

            _font.SetBounds(12, 108, 240, 27);
            _font.Click += OnFont;
            _color.SetBounds(258, 108, 110, 27);
            _color.Text = "Color...";
            _color.Click += OnColor;
            _halo.Text = "Light border";
            _halo.SetBounds(378, 111, 130, 22);
            _halo.Checked = _lastHalo;
            _halo.CheckedChanged += (s, e) => UpdatePreview();

            var labelAngle = new Label { Text = "Rotation (degrees):", Left = 12, Top = 147, Width = 120 };
            _angle.SetBounds(134, 144, 70, 23);
            _angle.Minimum = -180;
            _angle.Maximum = 180;
            _angle.Value = _lastAngle;
            _angle.ValueChanged += (s, e) => UpdatePreview();

            _preview.SetBounds(12, 176, 496, 136);
            _preview.BorderStyle = BorderStyle.FixedSingle;
            _preview.BackColor = Color.White;
            _preview.SizeMode = PictureBoxSizeMode.Zoom;

            var hint = new Label { Text = "After OK the text hangs at the mouse: click to place it, Esc cancels.", Left = 12, Top = 322, Width = 320, Height = 30, ForeColor = SystemColors.GrayText };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK };
            ok.SetBounds(342, 324, 80, 27);
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            cancel.SetBounds(428, 324, 80, 27);

            Controls.AddRange(new Control[] { labelText, _text, _font, _color, _halo, labelAngle, _angle, _preview, hint, ok, cancel });
            CancelButton = cancel;
            UpdateFontButton();
            UpdatePreview();
        }

        /// <summary>The rendered text (transparent background). Null if empty.</summary>
        public Bitmap CreateBitmap()
        {
            return Render(_text.Text, _selectedFont, _textColor, _halo.Checked ? _haloColor : Color.Empty, (float)_angle.Value);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                _lastText = _text.Text;
                _lastFont = (Font)_selectedFont.Clone();
                _lastHalo = _halo.Checked;
                _lastAngle = (int)_angle.Value;
            }

            base.OnFormClosed(e);
        }

        private void OnFont(object sender, EventArgs e)
        {
            using (var dialog = new FontDialog { Font = _selectedFont, ShowEffects = true })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedFont = dialog.Font;
                    UpdateFontButton();
                    UpdatePreview();
                }
            }
        }

        private void OnColor(object sender, EventArgs e)
        {
            using (var dialog = new ColorDialog { Color = _textColor, FullOpen = true })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _textColor = dialog.Color;
                    UpdatePreview();
                }
            }
        }

        private void UpdateFontButton()
        {
            _font.Text = $"Font: {_selectedFont.Name}, {_selectedFont.SizeInPoints:0} pt";
        }

        private void UpdatePreview()
        {
            Image old = _preview.Image;
            _preview.Image = CreateBitmap();
            old?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _preview.Image?.Dispose();
                _preview.Image = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>Renders text into a transparent bitmap</summary>
        public static Bitmap Render(string text, Font font, Color color, Color halo, float angle)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            text = text.Replace("\r\n", "\n");
            float emSize = font.Unit == GraphicsUnit.Pixel ? font.Size : font.SizeInPoints * 96f / 72f;
            using (var path = new GraphicsPath())
            {
                path.AddString(text, font.FontFamily, (int)font.Style, emSize, PointF.Empty, StringFormat.GenericTypographic);
                if (Math.Abs(angle) > 0.01f)
                {
                    using (var m = new Matrix())
                    {
                        m.Rotate(angle);
                        path.Transform(m);
                    }
                }

                RectangleF b = path.GetBounds();
                if (b.Width < 1 || b.Height < 1)
                {
                    return null;
                }

                float haloWidth = halo.IsEmpty ? 0 : Math.Max(3f, emSize / 5f);
                int pad = (int)Math.Ceiling(haloWidth) + 2;
                var bmp = new Bitmap((int)Math.Ceiling(b.Width) + 2 * pad, (int)Math.Ceiling(b.Height) + 2 * pad, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TranslateTransform(pad - b.X, pad - b.Y);
                    if (!halo.IsEmpty)
                    {
                        using (var pen = new Pen(halo, haloWidth) { LineJoin = LineJoin.Round })
                        {
                            g.DrawPath(pen, path);
                        }

                        using (var brush = new SolidBrush(halo))
                        {
                            g.FillPath(brush, path);
                        }
                    }

                    using (var brush = new SolidBrush(color))
                    {
                        g.FillPath(brush, path);
                    }
                }

                return bmp;
            }
        }
    }
}
