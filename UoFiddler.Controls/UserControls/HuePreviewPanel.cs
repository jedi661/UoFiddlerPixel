/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Preview of one or two hues on an item (art), an animation or a gump.
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Controls.UserControls
{
    public sealed class HuePreviewPanel : UserControl
    {
        private enum Source
        {
            Item,
            Animation,
            Gump
        }

        // settings are shared by all previews (hues tab, compare, dialogs)
        private static Source _source = Source.Item;
        private static int _itemId = 0x1F03;
        private static int _animId = 0x190;
        private static int _gumpId = 0x0C; // paperdoll body (male)
        private static bool _partial;
        private static bool _animate = true;
        private static int _direction = 1;
        private static bool _darkBack = true;

        private readonly ComboBox _cmbSource;
        private readonly TextBox _txtId;
        private readonly CheckBox _chkPartial;
        private readonly CheckBox _chkAnimate;
        private readonly NumericUpDown _numDir;
        private readonly CheckBox _chkDark;
        private readonly PictureBox _box;
        private readonly Timer _timer = new Timer { Interval = 150 };

        private Bitmap[] _frames = Array.Empty<Bitmap>();
        private Point[] _centers = Array.Empty<Point>();
        private Bitmap[] _hued1;
        private Bitmap[] _hued2;
        private int _frame;
        private ushort[] _colors;
        private ushort[] _secondColors;
        private bool _loading;

        public HuePreviewPanel()
        {
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(0, 2, 0, 2) };

            _cmbSource = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
            _cmbSource.Items.AddRange(new object[] { "Item (art)", "Animation", "Gump" });
            _txtId = new TextBox { Width = 64 };
            _chkPartial = new CheckBox { Text = "Only grey (partial hue)", AutoSize = true, Margin = new Padding(3, 5, 3, 0) };
            _chkAnimate = new CheckBox { Text = "Animate", AutoSize = true, Margin = new Padding(3, 5, 3, 0) };
            _numDir = new NumericUpDown { Minimum = 0, Maximum = 7, Width = 38 };
            _chkDark = new CheckBox { Text = "Dark", AutoSize = true, Margin = new Padding(3, 5, 3, 0) };

            var tip = new ToolTip();
            tip.SetToolTip(_txtId, "Id (decimal or 0x..), Enter = show");
            tip.SetToolTip(_chkPartial, "Only grey pixels are coloured (like items with the \"partial hue\" flag)");
            tip.SetToolTip(_numDir, "Direction of the animation");

            bar.Controls.AddRange(new Control[]
            {
                _cmbSource, _txtId, _chkPartial, _chkAnimate,
                new Label { Text = "Dir", AutoSize = true, Margin = new Padding(3, 6, 0, 0) }, _numDir, _chkDark
            });

            _box = new PictureBox { Dock = DockStyle.Fill };
            _box.Paint += OnPaintBox;
            _box.Resize += (s, e) => _box.Invalidate();

            Controls.Add(_box);
            Controls.Add(bar);

            _loading = true;
            _cmbSource.SelectedIndex = (int)_source;
            _chkPartial.Checked = _partial;
            _chkAnimate.Checked = _animate;
            _numDir.Value = _direction;
            _chkDark.Checked = _darkBack;
            _txtId.Text = $"0x{CurrentId:X}";
            _loading = false;

            _cmbSource.SelectedIndexChanged += (s, e) =>
            {
                if (_loading)
                {
                    return;
                }

                _source = (Source)_cmbSource.SelectedIndex;
                _txtId.Text = $"0x{CurrentId:X}";
                LoadSource();
            };
            _txtId.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplyId();
                }
            };
            _txtId.Leave += (s, e) => ApplyId();
            _chkPartial.CheckedChanged += (s, e) => { _partial = _chkPartial.Checked; Rehue(); };
            _chkAnimate.CheckedChanged += (s, e) => { _animate = _chkAnimate.Checked; UpdateTimer(); };
            _numDir.ValueChanged += (s, e) => { _direction = (int)_numDir.Value; if (_source == Source.Animation) { LoadSource(); } };
            _chkDark.CheckedChanged += (s, e) => { _darkBack = _chkDark.Checked; _box.Invalidate(); };
            _timer.Tick += (s, e) =>
            {
                if (_frames.Length > 1)
                {
                    _frame = (_frame + 1) % _frames.Length;
                    _box.Invalidate();
                }
            };

            if (!IsDesign())
            {
                LoadSource();
            }
        }

        /// <summary>Hue colours of the first (or only) preview</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort[] Colors
        {
            get => _colors;
            set
            {
                _colors = value == null ? null : (ushort[])value.Clone();
                Rehue();
            }
        }

        /// <summary>Hue colours of the second preview (null = only one preview)</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort[] SecondColors
        {
            get => _secondColors;
            set
            {
                _secondColors = value == null ? null : (ushort[])value.Clone();
                Rehue();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Caption1 { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Caption2 { get; set; }

        /// <summary>Sets both hues at once (one repaint)</summary>
        public void SetHues(ushort[] first, ushort[] second, string caption1 = null, string caption2 = null)
        {
            _colors = first == null ? null : (ushort[])first.Clone();
            _secondColors = second == null ? null : (ushort[])second.Clone();
            Caption1 = caption1;
            Caption2 = caption2;
            Rehue();
        }

        private bool IsDesign() => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private int CurrentId
        {
            get => _source switch
            {
                Source.Animation => _animId,
                Source.Gump => _gumpId,
                _ => _itemId
            };
            set
            {
                switch (_source)
                {
                    case Source.Animation: _animId = value; break;
                    case Source.Gump: _gumpId = value; break;
                    default: _itemId = value; break;
                }
            }
        }

        private void ApplyId()
        {
            string text = _txtId.Text.Trim();
            bool ok = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int id)
                : int.TryParse(text, out id);
            if (!ok || id < 0)
            {
                _txtId.ForeColor = Color.Red;
                return;
            }

            _txtId.ForeColor = SystemColors.WindowText;
            if (id == CurrentId && _frames.Length > 0)
            {
                return;
            }

            CurrentId = id;
            LoadSource();
        }

        private void LoadSource()
        {
            DisposeHued();
            _frames = Array.Empty<Bitmap>();
            _centers = Array.Empty<Point>();
            _frame = 0;

            try
            {
                switch (_source)
                {
                    case Source.Item:
                        if (Art.IsValidStatic(_itemId))
                        {
                            Bitmap bmp = Art.GetStatic(_itemId);
                            if (bmp != null)
                            {
                                _frames = new[] { bmp };
                                _centers = new[] { Point.Empty };
                            }
                        }

                        break;
                    case Source.Gump:
                        if (Gumps.IsValidIndex(_gumpId))
                        {
                            Bitmap bmp = Gumps.GetGump(_gumpId);
                            if (bmp != null)
                            {
                                _frames = new[] { bmp };
                                _centers = new[] { Point.Empty };
                            }
                        }

                        break;
                    case Source.Animation:
                        int hue = 0;
                        AnimationFrame[] frames = Animations.GetAnimation(_animId, 0, _direction, ref hue, false, false);
                        if (frames != null)
                        {
                            int count = 0;
                            foreach (AnimationFrame f in frames)
                            {
                                if (f?.Bitmap != null)
                                {
                                    count++;
                                }
                            }

                            _frames = new Bitmap[count];
                            _centers = new Point[count];
                            int n = 0;
                            foreach (AnimationFrame f in frames)
                            {
                                if (f?.Bitmap != null)
                                {
                                    _frames[n] = f.Bitmap;
                                    _centers[n] = f.Center;
                                    n++;
                                }
                            }
                        }

                        break;
                }
            }
            catch (Exception)
            {
                _frames = Array.Empty<Bitmap>();
            }

            _txtId.ForeColor = _frames.Length > 0 ? SystemColors.WindowText : Color.Red;
            Rehue();
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            if (_animate && _frames.Length > 1 && Visible)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }

        private void Rehue()
        {
            DisposeHued();
            _hued1 = HueFrames(_colors);
            _hued2 = _secondColors == null ? null : HueFrames(_secondColors);
            _box?.Invalidate();
        }

        private Bitmap[] HueFrames(ushort[] colors)
        {
            var result = new Bitmap[_frames.Length];
            for (int i = 0; i < _frames.Length; i++)
            {
                var bmp = new Bitmap(_frames[i]);
                if (colors != null)
                {
                    try
                    {
                        Hues.ApplyTo(bmp, colors, _partial);
                    }
                    catch (Exception)
                    {
                        // keep the unhued picture
                    }
                }

                result[i] = bmp;
            }

            return result;
        }

        private void DisposeHued()
        {
            foreach (Bitmap[] list in new[] { _hued1, _hued2 })
            {
                if (list == null)
                {
                    continue;
                }

                foreach (Bitmap b in list)
                {
                    b?.Dispose();
                }
            }

            _hued1 = null;
            _hued2 = null;
        }

        private void OnPaintBox(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(_darkBack ? Color.FromArgb(32, 32, 36) : Color.FromArgb(235, 235, 235));
            Color text = _darkBack ? Color.Gainsboro : Color.DimGray;

            if (_frames.Length == 0 || _hued1 == null)
            {
                TextRenderer.DrawText(g, "no picture for this id", Font, _box.ClientRectangle, text,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            int frame = Math.Min(_frame, _frames.Length - 1);
            bool two = _hued2 != null;
            int width = two ? _box.Width / 2 : _box.Width;

            DrawOne(g, _hued1[frame], _centers[frame], new Rectangle(0, 0, width, _box.Height), Caption1, text);
            if (two)
            {
                using (var pen = new Pen(Color.FromArgb(90, 128, 128, 128)))
                {
                    g.DrawLine(pen, width, 0, width, _box.Height);
                }

                DrawOne(g, _hued2[frame], _centers[frame], new Rectangle(width, 0, _box.Width - width, _box.Height), Caption2, text);
            }
        }

        private void DrawOne(Graphics g, Bitmap bmp, Point center, Rectangle area, string caption, Color text)
        {
            int top = string.IsNullOrEmpty(caption) ? 0 : 18;
            Rectangle inner = new Rectangle(area.X, area.Y + top, area.Width, area.Height - top);

            // scale down big pictures (gumps), never up
            float scale = Math.Min(1f, Math.Min(inner.Width / (float)Math.Max(1, bmp.Width), inner.Height / (float)Math.Max(1, bmp.Height)));
            int w = (int)(bmp.Width * scale), h = (int)(bmp.Height * scale);
            int x, y;
            if (_source == Source.Animation)
            {
                x = inner.X + (inner.Width / 2) - (int)(center.X * scale);
                y = inner.Y + (inner.Height * 3 / 4) - (int)((center.Y + bmp.Height) * scale);
            }
            else
            {
                x = inner.X + ((inner.Width - w) / 2);
                y = inner.Y + ((inner.Height - h) / 2);
            }

            g.InterpolationMode = scale < 1f ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
            g.DrawImage(bmp, new Rectangle(x, y, w, h));

            if (!string.IsNullOrEmpty(caption))
            {
                TextRenderer.DrawText(g, caption, Font, new Rectangle(area.X, area.Y + 2, area.Width, 16), text,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UpdateTimer();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                DisposeHued();
            }

            base.Dispose(disposing);
        }
    }
}
