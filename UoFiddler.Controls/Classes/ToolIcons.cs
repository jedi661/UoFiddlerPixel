using System.Drawing;
using System.Drawing.Drawing2D;

namespace UoFiddler.Controls.Classes
{
    /// <summary>Small 16x16 toolbar icons drawn in code (no resource files needed).</summary>
    public static class ToolIcons
    {
        private static readonly Color Gold = Color.FromArgb(230, 170, 30);

        /// <summary>UO tile diamond with four arrows: generate north / east / south / west.</summary>
        public static Bitmap Directions()
        {
            var bmp = new Bitmap(16, 16);
            using Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.None;
            using var pen = new Pen(Gold);
            g.DrawPolygon(pen, new[] { new Point(8, 4), new Point(12, 8), new Point(8, 12), new Point(4, 8) });
            using var arrow = new SolidBrush(Color.FromArgb(40, 90, 180));
            g.FillPolygon(arrow, new[] { new Point(13, 1), new Point(15, 1), new Point(15, 3) });  // NE
            g.FillPolygon(arrow, new[] { new Point(13, 15), new Point(15, 15), new Point(15, 13) }); // SE
            g.FillPolygon(arrow, new[] { new Point(1, 15), new Point(3, 15), new Point(1, 13) });   // SW
            g.FillPolygon(arrow, new[] { new Point(1, 1), new Point(3, 1), new Point(1, 3) });     // NW
            return bmp;
        }

        /// <summary>Skewed frame with corner points: transform / distort.</summary>
        public static Bitmap Transform()
        {
            var bmp = new Bitmap(16, 16);
            using Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.None;
            using var pen = new Pen(Color.FromArgb(40, 90, 180));
            Point[] quad = { new Point(4, 2), new Point(14, 4), new Point(12, 13), new Point(1, 11) };
            g.DrawPolygon(pen, quad);
            foreach (Point p in quad)
            {
                g.FillRectangle(Brushes.White, p.X - 1, p.Y - 1, 3, 3);
                g.DrawRectangle(Pens.Black, p.X - 1, p.Y - 1, 2, 2);
            }

            using var gold = new Pen(Gold);
            g.DrawLine(gold, 4, 8, 8, 6);
            g.DrawLine(gold, 8, 6, 11, 8);
            return bmp;
        }
    }
}
