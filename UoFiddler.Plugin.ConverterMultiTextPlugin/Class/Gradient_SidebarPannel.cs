using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Media;
//using System.Windows.Controls;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    public class Gradient_SidebarPannel : System.Windows.Forms.Panel
    {
        public System.Drawing.Color grandientTop { get; set; }
        public System.Drawing.Color grandientButton { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            System.Drawing.Drawing2D.LinearGradientBrush linear = new System.Drawing.Drawing2D.LinearGradientBrush(this.ClientRectangle, this.grandientTop, this.grandientButton, 90F);
            Graphics g = e.Graphics;
            g.FillRectangle(linear, this.ClientRectangle);
            base.OnPaint(e);
        }
    }
}
