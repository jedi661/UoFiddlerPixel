// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
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
    public class TriangleButton : Button
    {
        private Color _borderColor = Color.White; // Default border color
        private Color _backColor = Color.Gray; // Default background color
        private Color _textColor = Color.White; // Default text color
        private int borderThickness = 2; // Default borderThickness
        private ImageLayout imageLayout = ImageLayout.Zoom; // Default ImageLayout
        private Image _buttonImage;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the border color of the triangle button.")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the background color of the triangle button.")]
        public override Color BackColor
        {
            get { return _backColor; }
            set { _backColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the text color of the triangle button.")]
        public override Color ForeColor
        {
            get { return _textColor; }
            set { _textColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the image of the triangle button.")]
        public Image ButtonImage
        {
            get { return _buttonImage; }
            set { _buttonImage = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the thickness of the border.")]
        public int BorderThickness
        {
            get { return borderThickness; }
            set { borderThickness = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Sets the layout of the button image.")]
        public ImageLayout ImageLayout
        {
            get { return imageLayout; }
            set { imageLayout = value; Invalidate(); }
        }

        public TriangleButton()
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

            // Define triangular area
            PointF point1 = new PointF(this.Width / 2, 0);
            PointF point2 = new PointF(0, this.Height);
            PointF point3 = new PointF(this.Width, this.Height);
            PointF[] trianglePoints = { point1, point2, point3 };
            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(trianglePoints);

            // Draw background
            using (Brush brush = new SolidBrush(this.BackColor))
            {
                pevent.Graphics.FillPath(brush, path);
            }

            // Draw the button image
            if (this.ButtonImage != null)
            {
                // Draw the image within the triangle
                Region previousClip = pevent.Graphics.Clip;
                pevent.Graphics.SetClip(path);
                pevent.Graphics.DrawImage(this.ButtonImage, 0, 0, this.Width, this.Height);
                pevent.Graphics.Clip = previousClip;
            }

            // Draw text
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            pevent.Graphics.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), new Rectangle(0, 0, this.Width, this.Height), sf);

            // Draw border
            using (Pen pen = new Pen(this.BorderColor, this.BorderThickness))
            {
                pevent.Graphics.DrawPolygon(pen, trianglePoints);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Ensures the button remains triangular by setting the region
            GraphicsPath path = new GraphicsPath();
            PointF point1 = new PointF(this.Width / 2, 0);
            PointF point2 = new PointF(0, this.Height);
            PointF point3 = new PointF(this.Width, this.Height);
            path.AddPolygon(new PointF[] { point1, point2, point3 });
            this.Region = new Region(path);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            this.BackColor = Color.LightGray; // Hover color
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.BackColor = Color.Gray; // Reset to default
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            this.BackColor = Color.DarkGray; // Click color
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            this.BackColor = Color.Gray; // Reset after click
            base.OnMouseUp(mevent);
        }
    }
}
