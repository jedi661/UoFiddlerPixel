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
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.UserControls
{
    public sealed partial class HuePreviewPanel : UserControl
    {
        private enum Source
        {
            Item,
            Animation,
            Gump
        }

        // the own test graphic: shared by all previews (hues tab, compare, dialogs)
        private static Source _source = Source.Item;
        private static int _itemId = 0x1F03;
        private static int _animId = 0x190;
        private static int _gumpId = 0x0C; // paperdoll body (male)
        private static bool _partial;
        private static bool _animate = true;
        private static int _direction = 1;
        private static bool _darkBack = true;

        private Bitmap[] _frames = Array.Empty<Bitmap>();
        private Point[] _centers = Array.Empty<Point>();
        private Bitmap[] _hued1;
        private Bitmap[] _hued2;
        private int _frame;
        private ushort[] _colors;
        private ushort[] _secondColors;
        private bool _loading;

        // what is shown now: the own test graphic or a graphic chosen by the program (e.g. the usage of a hue)
        private Source _curSource;
        private int _curId;
        private bool _overridden;
        private bool _effectivePartial;

        public HuePreviewPanel()
        {
            InitializeComponent();

            _curSource = _source;
            _curId = UserId(_source);
            _loading = true;
            _cmbSource.SelectedIndex = (int)_source;
            _chkPartial.Checked = _partial;
            _chkAnimate.Checked = _animate;
            _numDir.Value = _direction;
            _chkDark.Checked = _darkBack;
            _txtId.Text = $"0x{_curId:X}";
            _loading = false;

            if (!IsDesign())
            {
                LoadSource();
            }
        }

        #region Designer events

        private void OnSourceChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            // the user chooses: this becomes the own test graphic again
            _source = (Source)_cmbSource.SelectedIndex;
            _overridden = false;
            _curSource = _source;
            _curId = UserId(_source);
            _txtId.Text = $"0x{_curId:X}";
            LoadSource();
        }

        private void OnIdKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyId();
            }
        }

        private void OnIdLeave(object sender, EventArgs e) => ApplyId();

        private void OnPartialChanged(object sender, EventArgs e)
        {
            if (_loading || _curSource == Source.Item)
            {
                return;
            }

            _partial = _chkPartial.Checked;
            _effectivePartial = _partial;
            Rehue();
        }

        private void OnAnimateChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            _animate = _chkAnimate.Checked;
            UpdateTimer();
        }

        private void OnDirectionChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            _direction = (int)_numDir.Value;
            if (_curSource == Source.Animation)
            {
                LoadSource();
            }
        }

        private void OnDarkChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            _darkBack = _chkDark.Checked;
            _box.Invalidate();
        }

        private void OnOwnGraphicClicked(object sender, LinkLabelLinkClickedEventArgs e) => ShowOwnGraphic();

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_frames.Length > 1)
            {
                _frame = (_frame + 1) % _frames.Length;
                _box.Invalidate();
            }
        }

        private void OnBoxResize(object sender, EventArgs e) => _box.Invalidate();

        #endregion

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

        private static int UserId(Source source) => source switch
        {
            Source.Animation => _animId,
            Source.Gump => _gumpId,
            _ => _itemId
        };

        private static void SetUserId(Source source, int value)
        {
            switch (source)
            {
                case Source.Animation: _animId = value; break;
                case Source.Gump: _gumpId = value; break;
                default: _itemId = value; break;
            }
        }

        /// <summary>
        /// Shows a certain graphic (e.g. the item that uses the hue). The own test graphic is kept and comes back with
        /// <see cref="ShowOwnGraphic"/> or the link in the bar.
        /// </summary>
        public void ShowGraphic(HuePreviewKind kind, int id)
        {
            var source = (Source)(int)kind;
            if (_overridden && source == _curSource && id == _curId)
            {
                return;
            }

            _overridden = true;
            _curSource = source;
            _curId = id;
            LoadSource();
        }

        /// <summary>Back to the own test graphic</summary>
        public void ShowOwnGraphic()
        {
            if (!_overridden)
            {
                return;
            }

            _overridden = false;
            _curSource = _source;
            _curId = UserId(_source);
            LoadSource();
        }

        /// <summary>true while a graphic chosen by the program is shown</summary>
        [Browsable(false)]
        public bool ShowsOtherGraphic => _overridden;

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
            if (id == _curId && _frames.Length > 0)
            {
                return;
            }

            // typed by the user: becomes the own test graphic
            _source = _curSource;
            SetUserId(_curSource, id);
            _overridden = false;
            _curId = id;
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
                switch (_curSource)
                {
                    case Source.Item:
                        if (Art.IsValidStatic(_curId))
                        {
                            Bitmap bmp = Art.GetStatic(_curId);
                            if (bmp != null)
                            {
                                _frames = new[] { bmp };
                                _centers = new[] { Point.Empty };
                            }
                        }

                        break;
                    case Source.Gump:
                        if (Gumps.IsValidIndex(_curId))
                        {
                            Bitmap bmp = Gumps.GetGump(_curId);
                            if (bmp != null)
                            {
                                _frames = new[] { bmp };
                                _centers = new[] { Point.Empty };
                            }
                        }

                        break;
                    case Source.Animation:
                        int hue = 0;
                        AnimationFrame[] frames = Animations.GetAnimation(_curId, 0, _direction, ref hue, false, false);
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

            // update the bar (without events)
            _loading = true;
            _cmbSource.SelectedIndex = (int)_curSource;
            _txtId.Text = $"0x{_curId:X}";
            _loading = false;
            _lnkOwn.Visible = _overridden;
            _txtId.ForeColor = _frames.Length > 0 ? SystemColors.WindowText : Color.Red;

            // items: "partial hue" comes from the tiledata flag (as in the game), otherwise from the check box
            if (_curSource == Source.Item)
            {
                bool flag = false;
                try
                {
                    flag = TileData.ItemTable != null && _curId < TileData.ItemTable.Length &&
                           (TileData.ItemTable[_curId].Flags & TileFlag.PartialHue) != 0;
                }
                catch (Exception)
                {
                    // ignored
                }

                _effectivePartial = flag;
                _loading = true;
                _chkPartial.Checked = flag;
                _chkPartial.Enabled = false;
                _chkPartial.Text = "Only grey (tiledata)";
                _loading = false;
            }
            else
            {
                _effectivePartial = _partial;
                _loading = true;
                _chkPartial.Checked = _partial;
                _chkPartial.Enabled = true;
                _chkPartial.Text = "Only grey (partial hue)";
                _loading = false;
            }

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
                        Hues.ApplyTo(bmp, colors, _effectivePartial);
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
            if (_curSource == Source.Animation)
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

    }
}
