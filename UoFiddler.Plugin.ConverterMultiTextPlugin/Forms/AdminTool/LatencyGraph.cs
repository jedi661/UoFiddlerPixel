/***************************************************************************
 *
 * $Author: Nikodemus
 * Coder: Nikodemus
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Small live chart for ping round trip times. A null sample is a lost packet.
    /// </summary>
    public class LatencyGraph : Control
    {
        private readonly List<long?> _samples = new();

        public int MaxSamples { get; set; } = 120;

        public LatencyGraph()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Color.FromArgb(24, 26, 31);
            ForeColor = Color.Gainsboro;
            Font = new Font("Segoe UI", 8.25F);
        }

        public void AddSample(long? ms)
        {
            _samples.Add(ms);
            if (_samples.Count > MaxSamples)
            {
                _samples.RemoveAt(0);
            }

            Invalidate();
        }

        public void Clear()
        {
            _samples.Clear();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            var plot = new Rectangle(44, 10, Width - 54, Height - 30);
            if (plot.Width < 20 || plot.Height < 20)
            {
                return;
            }

            long maxValue = _samples.Where(s => s.HasValue).Select(s => s.Value).DefaultIfEmpty(0).Max();
            long scale = NiceCeiling(Math.Max(maxValue, 10));

            using var gridPen = new Pen(Color.FromArgb(50, 54, 62));
            using var textBrush = new SolidBrush(Color.FromArgb(150, 155, 165));

            for (int i = 0; i <= 4; i++)
            {
                int y = plot.Bottom - plot.Height * i / 4;
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                string label = (scale * i / 4) + " ms";
                SizeF size = g.MeasureString(label, Font);
                g.DrawString(label, Font, textBrush, plot.Left - size.Width - 2, y - size.Height / 2);
            }

            if (_samples.Count == 0)
            {
                const string hint = "Start a ping to see the latency graph";
                SizeF hs = g.MeasureString(hint, Font);
                g.DrawString(hint, Font, textBrush, plot.Left + (plot.Width - hs.Width) / 2, plot.Top + (plot.Height - hs.Height) / 2);
                return;
            }

            float step = MaxSamples > 1 ? plot.Width / (float)(MaxSamples - 1) : plot.Width;
            float x0 = plot.Right - (_samples.Count - 1) * step;

            var points = new List<PointF>();
            using var linePen = new Pen(Color.FromArgb(64, 169, 255), 2f);
            using var fillBrush = new SolidBrush(Color.FromArgb(40, 64, 169, 255));
            using var lostPen = new Pen(Color.FromArgb(230, 80, 80), 2f);

            void FlushSegment()
            {
                if (points.Count >= 2)
                {
                    var poly = new List<PointF>(points)
                    {
                        new PointF(points[^1].X, plot.Bottom),
                        new PointF(points[0].X, plot.Bottom)
                    };
                    g.FillPolygon(fillBrush, poly.ToArray());
                    g.DrawLines(linePen, points.ToArray());
                }
                else if (points.Count == 1)
                {
                    g.FillEllipse(Brushes.DeepSkyBlue, points[0].X - 2, points[0].Y - 2, 4, 4);
                }

                points.Clear();
            }

            for (int i = 0; i < _samples.Count; i++)
            {
                float x = x0 + i * step;
                if (_samples[i] is long v)
                {
                    float y = plot.Bottom - Math.Min(1f, v / (float)scale) * plot.Height;
                    points.Add(new PointF(x, y));
                }
                else
                {
                    FlushSegment();
                    g.DrawLine(lostPen, x, plot.Top, x, plot.Bottom);
                }
            }

            FlushSegment();

            string footer = $"{_samples.Count} samples   red = lost";
            g.DrawString(footer, Font, textBrush, plot.Left, plot.Bottom + 4);
        }

        private static long NiceCeiling(long value)
        {
            long[] steps = { 10, 20, 50, 100, 200, 300, 500, 1000, 2000, 5000 };
            foreach (long s in steps)
            {
                if (value <= s)
                {
                    return s;
                }
            }

            return (value / 1000 + 1) * 1000;
        }
    }
}
