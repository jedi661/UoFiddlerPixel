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
    /// <summary>Waveform display with mouse selection and play cursor.</summary>
    public class WaveformView : Control
    {
        private float[] _s = Array.Empty<float>();
        private int _rate = 22050;
        private int _selA = -1, _selB = -1;
        private int _play = -1;
        private bool _mouse;

        public WaveformView()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(24, 26, 32);
            Cursor = Cursors.IBeam;
        }

        public event EventHandler SelectionChanged;

        [Browsable(false)]
        public bool HasSelection => _selA >= 0 && _selB >= 0 && Math.Abs(_selB - _selA) > 1;
        [Browsable(false)]
        public int SelStart => Math.Max(0, Math.Min(_selA, _selB));
        [Browsable(false)]
        public int SelEnd => Math.Min(_s.Length, Math.Max(_selA, _selB));

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PlayPosition
        {
            get => _play;
            set
            {
                _play = value;
                Invalidate();
            }
        }

        public void SetSamples(float[] s, int rate)
        {
            _s = s ?? Array.Empty<float>();
            _rate = rate;
            if (_selA > _s.Length || _selB > _s.Length)
            {
                _selA = _selB = -1;
            }
            Invalidate();
        }

        public void SelectAll()
        {
            _selA = 0;
            _selB = _s.Length;
            Invalidate();
        }

        private int ToSample(int x) => _s.Length == 0 ? 0 : (int)Math.Max(0, Math.Min(_s.Length, (long)x * _s.Length / Math.Max(1, Width)));

        private int ToX(int sample) => _s.Length == 0 ? 0 : (int)((long)sample * Width / _s.Length);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _mouse = true;
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
            if (_mouse)
            {
                _mouse = false;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
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
                // second ticks
                if (_rate > 0 && _s.Length > 0)
                {
                    double secs = _s.Length / (double)_rate;
                    double step = secs > 20 ? 5 : secs > 5 ? 1 : secs > 1 ? 0.25 : 0.1;
                    for (double t = step; t < secs; t += step)
                    {
                        int x = ToX((int)(t * _rate));
                        g.DrawLine(grid, x, h - 12, x, h);
                        g.DrawString($"{t:0.##}s", Font, Brushes.Gray, x + 2, h - 14);
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
                int a = (int)((long)x * _s.Length / w);
                int b = Math.Max(a + 1, (int)((long)(x + 1) * _s.Length / w));
                float min = 0, max = 0;
                for (int i = a; i < b && i < _s.Length; i++)
                {
                    if (_s[i] < min) { min = _s[i]; }
                    if (_s[i] > max) { max = _s[i]; }
                }
                bool clipped = max >= 0.999f || min <= -0.999f;
                g.DrawLine(clipped ? clip : pen, x, mid - (int)(max * (mid - 2)), x, mid - (int)(min * (mid - 2)));
            }

            if (_play >= 0)
            {
                int px = ToX(_play);
                g.DrawLine(Pens.Yellow, px, 0, px, h);
            }
        }
    }
}
