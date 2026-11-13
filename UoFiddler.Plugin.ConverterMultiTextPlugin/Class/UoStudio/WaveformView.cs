/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio
{
    /// <summary>
    /// Waveform display: mouse drag = selection, click = play cursor, right click = clear selection,
    /// mouse wheel = zoom at the mouse position, Shift + wheel = scroll.
    /// </summary>
    public class WaveformView : Control
    {
        private float[] _s = Array.Empty<float>();
        private int _rate = 22050;
        private int _selA = -1, _selB = -1;
        private int _play = -1;
        private int _cursor = -1;
        private int _viewStart;
        private int _viewLength;
        private bool _mouse;
        private int _mouseDownX;

        public WaveformView()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(24, 26, 32);
            Cursor = Cursors.IBeam;
        }

        /// <summary>Selection or cursor changed.</summary>
        public event EventHandler SelectionChanged;

        /// <summary>Visible part changed (zoom / scroll).</summary>
        public event EventHandler ViewChanged;

        [Browsable(false)]
        public bool HasSelection => _selA >= 0 && _selB >= 0 && Math.Abs(_selB - _selA) > 1;

        [Browsable(false)]
        public int SelStart => Math.Max(0, Math.Min(_selA, _selB));

        [Browsable(false)]
        public int SelEnd => Math.Min(_s.Length, Math.Max(_selA, _selB));

        /// <summary>Play cursor (sample index) or -1.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CursorPosition
        {
            get => _cursor;
            set
            {
                _cursor = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PlayPosition
        {
            get => _play;
            set
            {
                _play = value;
                // follow the play head when zoomed in
                if (_play >= 0 && _viewLength < _s.Length && (_play < _viewStart || _play > _viewStart + _viewLength))
                {
                    SetView(_play - (_viewLength / 10), _viewLength);
                }
                Invalidate();
            }
        }

        /// <summary>Zoom factor (1 = whole sound visible).</summary>
        [Browsable(false)]
        public double ZoomFactor => _viewLength <= 0 ? 1 : _s.Length / (double)_viewLength;

        public void SetSamples(float[] s, int rate)
        {
            bool sameLength = s != null && s.Length == _s.Length;
            _s = s ?? Array.Empty<float>();
            _rate = rate;
            if (_selA > _s.Length || _selB > _s.Length)
            {
                _selA = _selB = -1;
            }
            if (_cursor > _s.Length)
            {
                _cursor = -1;
            }
            if (!sameLength)
            {
                _viewStart = 0;
                _viewLength = _s.Length;
            }
            SetView(_viewStart, _viewLength);
        }

        public void SelectAll()
        {
            _selA = 0;
            _selB = _s.Length;
            Invalidate();
        }

        public void ZoomIn() => ZoomAround(_cursor >= 0 ? _cursor : _viewStart + (_viewLength / 2), 0.5);

        public void ZoomOut() => ZoomAround(_viewStart + (_viewLength / 2), 2.0);

        public void ZoomFit() => SetView(0, _s.Length);

        /// <summary>Zooms to the selection (if there is one).</summary>
        public void ZoomSelection()
        {
            if (HasSelection)
            {
                SetView(SelStart, SelEnd - SelStart);
            }
        }

        private void ZoomAround(int sample, double factor)
        {
            if (_s.Length == 0)
            {
                return;
            }
            int len = (int)Math.Max(Math.Min(64, _s.Length), Math.Min(_s.Length, _viewLength * factor));
            double rel = _viewLength <= 0 ? 0.5 : (sample - _viewStart) / (double)_viewLength;
            SetView((int)(sample - (rel * len)), len);
        }

        private void SetView(int start, int length)
        {
            length = Math.Max(1, Math.Min(_s.Length == 0 ? 1 : _s.Length, length));
            start = Math.Max(0, Math.Min(Math.Max(0, _s.Length - length), start));
            _viewStart = start;
            _viewLength = _s.Length == 0 ? 0 : length;
            Invalidate();
            ViewChanged?.Invoke(this, EventArgs.Empty);
        }

        private int ToSample(int x)
        {
            if (_s.Length == 0 || _viewLength <= 0)
            {
                return 0;
            }
            long v = _viewStart + ((long)x * _viewLength / Math.Max(1, Width));
            return (int)Math.Max(0, Math.Min(_s.Length, v));
        }

        private int ToX(int sample) => _viewLength <= 0 ? 0 : (int)((long)(sample - _viewStart) * Width / _viewLength);

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_s.Length == 0)
            {
                return;
            }
            if ((ModifierKeys & Keys.Shift) != 0)
            {
                SetView(_viewStart - (Math.Sign(e.Delta) * Math.Max(1, _viewLength / 8)), _viewLength);
            }
            else
            {
                ZoomAround(ToSample(e.X), e.Delta > 0 ? 0.7 : 1.4);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button == MouseButtons.Left)
            {
                _mouse = true;
                _mouseDownX = e.X;
                _selA = _selB = ToSample(e.X);
                Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                _selA = _selB = -1;
                Invalidate();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_mouse)
            {
                _selB = ToSample(e.X);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!_mouse)
            {
                return;
            }
            _mouse = false;
            if (Math.Abs(e.X - _mouseDownX) < 3)
            {
                // simple click: set the play cursor, no selection
                _cursor = ToSample(e.X);
                _selA = _selB = -1;
                Invalidate();
            }
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            int w = Width, h = Height, mid = h / 2;

            if (HasSelection)
            {
                using var sb = new SolidBrush(Color.FromArgb(70, 80, 140, 255));
                int x1 = ToX(SelStart), x2 = ToX(SelEnd);
                g.FillRectangle(sb, x1, 0, Math.Max(1, x2 - x1), h);
            }

            using (var grid = new Pen(Color.FromArgb(50, 255, 255, 255)))
            {
                g.DrawLine(grid, 0, mid, w, mid);
                // -6 dB lines
                g.DrawLine(grid, 0, mid - (h / 4), w, mid - (h / 4));
                g.DrawLine(grid, 0, mid + (h / 4), w, mid + (h / 4));
                // time ticks
                if (_rate > 0 && _viewLength > 0)
                {
                    double secs = _viewLength / (double)_rate;
                    double step = secs > 20 ? 5 : secs > 5 ? 1 : secs > 1 ? 0.25 : secs > 0.2 ? 0.05 : 0.01;
                    double t0 = Math.Ceiling(_viewStart / (double)_rate / step) * step;
                    for (double t = t0; t < (_viewStart + _viewLength) / (double)_rate; t += step)
                    {
                        int x = ToX((int)(t * _rate));
                        g.DrawLine(grid, x, h - 12, x, h);
                        g.DrawString($"{t:0.###}s", Font, Brushes.Gray, x + 2, h - 14);
                    }
                }
            }

            if (_s.Length == 0)
            {
                g.DrawString("Kein Sound geladen – WAV / MP3 laden oder UO-Sound öffnen.", Font, Brushes.Gray, 10, 10);
                return;
            }

            using var pen = new Pen(Color.FromArgb(90, 210, 140));
            using var clip = new Pen(Color.FromArgb(255, 80, 80));
            for (int x = 0; x < w; x++)
            {
                int a = ToSample(x);
                int b = Math.Max(a + 1, ToSample(x + 1));
                float min = 0, max = 0;
                for (int i = a; i < b && i < _s.Length; i++)
                {
                    if (_s[i] < min) { min = _s[i]; }
                    if (_s[i] > max) { max = _s[i]; }
                }
                bool clipped = max >= 0.999f || min <= -0.999f;
                g.DrawLine(clipped ? clip : pen, x, mid - (int)(max * (mid - 2)), x, mid - (int)(min * (mid - 2)));
            }

            if (_cursor >= 0)
            {
                using var cp = new Pen(Color.FromArgb(200, 255, 255, 255)) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                int cx = ToX(_cursor);
                g.DrawLine(cp, cx, 0, cx, h);
            }

            if (_play >= 0)
            {
                int px = ToX(_play);
                g.DrawLine(Pens.Yellow, px, 0, px, h);
            }

            if (_viewLength < _s.Length)
            {
                // overview bar: visible part of the whole sound
                int bx = (int)((long)_viewStart * w / _s.Length);
                int bw = Math.Max(4, (int)((long)_viewLength * w / _s.Length));
                using var bar = new SolidBrush(Color.FromArgb(120, 120, 160, 255));
                g.FillRectangle(bar, bx, 0, bw, 4);
                g.DrawString($"Zoom {ZoomFactor:0.#}x", Font, Brushes.Silver, 4, 6);
            }
        }
    }
}
