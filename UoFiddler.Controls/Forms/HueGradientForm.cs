/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * New hue: gradient through 2 or 3 colours (dark / middle / bright).
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
    public partial class HueGradientForm : Form
    {
        private Color _dark, _middle, _bright;

        /// <summary>Only for the designer</summary>
        public HueGradientForm()
            : this(null, string.Empty)
        {
        }

        /// <param name="start">current colours (the stops are taken from them) or null</param>
        public HueGradientForm(ushort[] start, string name)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _txtName.Text = name ?? string.Empty;

            bool empty = start == null || Array.TrueForAll(start, c => (c & 0x7FFF) == 0);
            _dark = empty ? Color.FromArgb(16, 8, 8) : HueAnalysis.ToColor(start[1]);
            _middle = empty ? Color.FromArgb(160, 40, 40) : HueAnalysis.ToColor(start[16]);
            _bright = empty ? Color.FromArgb(255, 220, 200) : HueAnalysis.ToColor(start[31]);

            PaintButtons();
            Recalc();
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

        private void PaintButtons()
        {
            PaintColorButton(_btnDark, _dark);
            PaintColorButton(_btnMiddle, _middle);
            PaintColorButton(_btnBright, _bright);
        }

        private static void PaintColorButton(Button button, Color color)
        {
            button.BackColor = color;
            button.ForeColor = color.GetBrightness() > 0.55f ? Color.Black : Color.White;
        }

        private void OnClickColorButton(object sender, EventArgs e)
        {
            Color current = sender == _btnDark ? _dark : sender == _btnMiddle ? _middle : _bright;
            using (var dlg = new ColorDialog { Color = current, FullOpen = true })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                if (sender == _btnDark)
                {
                    _dark = dlg.Color;
                }
                else if (sender == _btnMiddle)
                {
                    _middle = dlg.Color;
                }
                else
                {
                    _bright = dlg.Color;
                }
            }

            PaintButtons();
            Recalc();
        }

        private void OnMiddleChanged(object sender, EventArgs e)
        {
            _btnMiddle.Enabled = _chkMiddle.Checked;
            Recalc();
        }

        private void OnGammaChanged(object sender, EventArgs e) => Recalc();

        private void Recalc()
        {
            double gamma = _trackGamma.Value / 10.0;
            _lblGamma.Text = gamma < 0.95 ? $"brighter ({gamma:0.0})" : gamma > 1.05 ? $"darker ({gamma:0.0})" : "linear";
            SetResult(HueAnalysis.Gradient(_dark, _chkMiddle.Checked ? _middle : (Color?)null, _bright, gamma));
        }
    }
}
