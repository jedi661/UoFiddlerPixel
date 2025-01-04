// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace CustomControls
{
    public class VerticalTabControl : TabControl
    {
        public VerticalTabControl()
        {
            // Enable custom drawing for the tabs
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            // Subscribe to the DrawItem event to customize tab drawing
            this.DrawItem += new DrawItemEventHandler(DrawOnTabItem);
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
            // Ensure the tabs are aligned to the left
            if (this.Alignment != TabAlignment.Left)
            {
                this.Alignment = TabAlignment.Left;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!this.DesignMode)
            {
                // Enable multiline to allow for vertical tab stacking
                this.Multiline = true;
            }
            base.OnPaint(e);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            // Update tab sizes whenever a new control is added
            UpdateTabSizes();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            // Update tab sizes whenever a control is removed
            UpdateTabSizes();
        }

        private void UpdateTabSizes()
        {
            foreach (TabPage tabPage in this.TabPages)
            {
                // Force tab page text update to refresh sizes
                tabPage.Text = tabPage.Text;
            }
        }

        private void DrawOnTabItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            TabPage tabPage = this.TabPages[e.Index];
            Rectangle bounds = this.GetTabRect(e.Index);
            Brush textBrush;

            // Determine if the current tab is selected
            if (e.Index == this.SelectedIndex)
            {
                // Use white text on gray background for selected tab
                textBrush = new SolidBrush(Color.White);
                g.FillRectangle(Brushes.Gray, bounds);
            }
            else
            {
                // Use default text color on light gray background for non-selected tabs
                textBrush = new SolidBrush(e.ForeColor);
                g.FillRectangle(new SolidBrush(Color.LightGray), bounds);
            }

            // Draw the tab text
            Font tabFont = new Font("Arial", 10.0f, FontStyle.Bold, GraphicsUnit.Point);
            StringFormat stringFlags = new StringFormat
            {
                // Center the text within the tab bounds
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(tabPage.Text, tabFont, textBrush, bounds, stringFlags);
        }

        // Method to draw tabs with rounded corners
        private void DrawRoundedTab(Graphics g, TabPage tabPage, Rectangle bounds)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int radius = 10; // Radius for the rounded corners
                path.AddArc(bounds.Left, bounds.Top, radius, radius, 180, 90);
                path.AddArc(bounds.Right - radius, bounds.Top, radius, radius, 270, 90);
                path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(bounds.Left, bounds.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();

                // Fill the path with a light blue color
                g.FillPath(Brushes.LightBlue, path);
            }
        }
    }
}
