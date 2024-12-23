// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * 
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace CustomControls
{
    [DesignerCategory("Code")]
    [ToolboxBitmap(typeof(Button))]
    public class RoundButton : Button
    {
        private Color _borderColor = Color.White; // Default border color
        private Color _backColor = Color.Gray; // Default background color
        private Color _textColor = Color.White; // Default text color
        private Image _buttonImage;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the border color of the round button.")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the background color of the round button.")]
        public override Color BackColor
        {
            get { return _backColor; }
            set { _backColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the text color of the round button.")]
        public override Color ForeColor
        {
            get { return _textColor; }
            set { _textColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the image of the round button.")]
        public Image ButtonImage
        {
            get { return _buttonImage; }
            set { _buttonImage = value; Invalidate(); }
        }

        public RoundButton()
        {
            this.BackColor = Color.Gray; // Default background color
            this.ForeColor = Color.White; // Default text color
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            // Enables anti-aliasing for smoother graphics
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Define circular area
            int diameter = Math.Min(this.Width, this.Height);
            Rectangle circleRect = new Rectangle(0, 0, diameter, diameter);

            // Draw background (with optional image)
            using (Brush brush = new SolidBrush(this.BackColor))
            {
                pevent.Graphics.FillEllipse(brush, circleRect);
            }

            // Draw the button image
            if (this.ButtonImage != null)
            {
                float scale = Math.Min((float)circleRect.Width / this.ButtonImage.Width,
                                       (float)circleRect.Height / this.ButtonImage.Height);

                int scaledWidth = (int)(this.ButtonImage.Width * scale);
                int scaledHeight = (int)(this.ButtonImage.Height * scale);

                int offsetX = (circleRect.Width - scaledWidth) / 2;
                int offsetY = (circleRect.Height - scaledHeight) / 2;

                Rectangle destRect = new Rectangle(
                    circleRect.X + offsetX,
                    circleRect.Y + offsetY,
                    scaledWidth,
                    scaledHeight
                );

                pevent.Graphics.DrawImage(this.ButtonImage, destRect);
            }

            // Draw text
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            pevent.Graphics.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), circleRect, sf);

            // Draw border
            using (Pen pen = new Pen(this.BorderColor, 2))
            {
                pevent.Graphics.DrawEllipse(pen, circleRect);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Keeps the button square by setting the width equal to the height
            this.Width = this.Height;

            // Sets the region of the button to a circle, making the background "transparent"
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, this.Width, this.Height);
            this.Region = new Region(path);
        }
    }
}
