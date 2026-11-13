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
    /// <summary>Vertical peak / RMS meter in dBFS.</summary>
    public class LevelMeter : Control
    {
        private float _peak;
        private double _rms;

        public LevelMeter()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(30, 30, 34);
        }

        public void SetLevels(float peak, double rms)
        {
            _peak = peak;
            _rms = rms;
            Invalidate();
        }

        private int Y(double db) => (int)((Height - 20) * Math.Min(1, Math.Max(0, -db / 60.0))) + 4;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            int bottom = Height - 16;
            double pdb = StudioAudio.ToDb(_peak), rdb = StudioAudio.ToDb(_rms);

            using (var green = new SolidBrush(Color.FromArgb(60, 180, 90)))
            using (var yellow = new SolidBrush(Color.FromArgb(220, 200, 60)))
            using (var red = new SolidBrush(Color.FromArgb(220, 60, 60)))
            {
                int top = Y(pdb);
                for (int y = top; y < bottom; y += 3)
                {
                    double db = -60.0 * (y - 4) / (Height - 20);
                    Brush b = db > -3 ? red : db > -12 ? yellow : green;
                    g.FillRectangle(b, 8, y, 22, 2);
                }
                int rt = Y(rdb);
                using var rb = new SolidBrush(Color.FromArgb(120, 160, 255));
                g.FillRectangle(rb, 36, rt, 10, Math.Max(0, bottom - rt));
            }

            using var f = new Font(Font.FontFamily, 7f);
            foreach (int db in new[] { 0, -6, -12, -24, -48 })
            {
                int y = Y(db);
                g.DrawLine(Pens.Gray, 4, y, 50, y);
                g.DrawString(db.ToString(), f, Brushes.Silver, 48, y - 6);
            }
            g.DrawString($"{pdb:0}", f, Brushes.White, 6, bottom + 1);
        }
    }
}
