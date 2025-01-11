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

namespace CustomControls
{
    public class VerticalTabControl2 : TabControl
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            if (this.Alignment != TabAlignment.Left)
            {
                this.Alignment = TabAlignment.Left;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!this.DesignMode)
            {
                this.Multiline = true;
            }
            base.OnPaint(e);
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            UpdateTabSizes();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            UpdateTabSizes();
        }

        private void UpdateTabSizes()
        {
            foreach (TabPage tabPage in this.TabPages)
            {
                tabPage.Text = tabPage.Text;
            }
        }
    }
}