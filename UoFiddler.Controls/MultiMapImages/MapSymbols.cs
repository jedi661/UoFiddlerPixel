/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace UoFiddler.Controls.MultiMapImages
{
    /// <summary>Symbols in the style of hand drawn maps</summary>
    public enum MapSymbol
    {
        Mountain,
        Tree,
        Conifer,
        Bush,
        House,
        Rock,
        Tuft,
        Ankh,
        Skull,
        Star,
        Treasure,
        Compass,
        Tower,
        Cave,
        Marker
    }

    /// <summary>
    /// Draws map symbols. (x, y) is the foot point of the symbol (bottom center), size is about the height.
    /// </summary>
    public sealed class MapSymbolPainter : IDisposable
    {
        private readonly Pen _pen;
        private readonly Pen _thin;
        private readonly SolidBrush _paper;
        private readonly SolidBrush _ink;

        public MapSymbolPainter(Color ink, Color paper, float lineWidth)
        {
            _pen = new Pen(ink, Math.Max(1f, lineWidth)) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
            _thin = new Pen(ink, Math.Max(1f, lineWidth * 0.6f));
            _paper = new SolidBrush(paper);
            _ink = new SolidBrush(ink);
        }

        public void Dispose()
        {
            _pen.Dispose();
            _thin.Dispose();
            _paper.Dispose();
            _ink.Dispose();
        }

        /// <summary>Bounds of a symbol (for previews and repainting)</summary>
        public static RectangleF GetBounds(MapSymbol kind, float x, float y, float size)
        {
            float s = size;
            switch (kind)
            {
                case MapSymbol.Mountain:
                    return new RectangleF(x - s * 0.7f, y - s * 1.05f, s * 1.4f, s * 1.1f);
                case MapSymbol.Compass:
                    return new RectangleF(x - s * 0.6f, y - s * 1.25f, s * 1.2f, s * 1.3f);
                default:
                    return new RectangleF(x - s * 0.65f, y - s * 1.2f, s * 1.3f, s * 1.3f);
            }
        }

        public void Draw(Graphics g, MapSymbol kind, float x, float y, float size, int variant = 0)
        {
            switch (kind)
            {
                case MapSymbol.Mountain:
                    Mountain(g, x, y, size, variant);
                    break;
                case MapSymbol.Tree:
                    Tree(g, x, y, size, variant);
                    break;
                case MapSymbol.Conifer:
                    Conifer(g, x, y, size);
                    break;
                case MapSymbol.Bush:
                    Bush(g, x, y, size);
                    break;
                case MapSymbol.House:
                    House(g, x, y, size, variant);
                    break;
                case MapSymbol.Rock:
                    Rock(g, x, y, size);
                    break;
                case MapSymbol.Tuft:
                    Tuft(g, x, y, size);
                    break;
                case MapSymbol.Ankh:
                    Ankh(g, x, y, size);
                    break;
                case MapSymbol.Skull:
                    Skull(g, x, y, size);
                    break;
                case MapSymbol.Star:
                    Star(g, x, y, size);
                    break;
                case MapSymbol.Treasure:
                    Treasure(g, x, y, size);
                    break;
                case MapSymbol.Compass:
                    Compass(g, x, y, size);
                    break;
                case MapSymbol.Tower:
                    Tower(g, x, y, size);
                    break;
                case MapSymbol.Cave:
                    Cave(g, x, y, size);
                    break;
                case MapSymbol.Marker:
                    Marker(g, x, y, size);
                    break;
            }
        }

        private void Mountain(Graphics g, float x, float y, float s, int variant)
        {
            float peakX = x + ((variant % 7) - 3) * s * 0.04f;
            float peakY = y - s * (0.75f + (variant % 5) * 0.05f);
            float left = x - s * 0.6f, right = x + s * 0.6f;
            float shoulderX = x + s * 0.32f, shoulderY = y - s * 0.35f;

            PointF[] outline =
            {
                new PointF(left, y),
                new PointF(x - s * 0.3f, y - s * 0.38f),
                new PointF(peakX, peakY),
                new PointF(shoulderX, shoulderY),
                new PointF(right, y)
            };
            g.FillPolygon(_paper, outline);
            g.DrawLines(_pen, outline);

            // ridge from the peak down
            g.DrawLine(_thin, peakX, peakY, x + s * 0.05f, y - s * 0.05f);

            // hatching on the shadow side (right)
            int lines = Math.Max(2, (int)(s / 3));
            for (int i = 1; i <= lines; i++)
            {
                float t = i / (float)(lines + 1);
                float x1 = peakX + (shoulderX - peakX) * t;
                float y1 = peakY + (shoulderY - peakY) * t;
                float x2 = x + s * 0.05f + (right - x) * t * 0.9f;
                float y2 = y - s * 0.05f;
                g.DrawLine(_thin, x1, y1 + 1, x2 - (x2 - x1) * 0.25f, y2 - (y2 - y1) * 0.25f);
            }
        }

        private void Tree(Graphics g, float x, float y, float s, int variant)
        {
            float trunkTop = y - s * 0.35f;
            g.DrawLine(_pen, x, y, x, trunkTop);

            float r = s * 0.42f;
            float cx = x, cy = trunkTop - r * 0.7f;
            var a = new RectangleF(cx - r, cy - r * 0.6f, r * 1.2f, r * 1.2f);
            var b = new RectangleF(cx - r * 0.2f, cy - r * 0.6f, r * 1.2f, r * 1.2f);
            var c = new RectangleF(cx - r * 0.6f, cy - r * 1.2f, r * 1.2f, r * 1.2f);
            using (var path = new GraphicsPath())
            {
                path.FillMode = FillMode.Winding;
                path.AddEllipse(a);
                path.AddEllipse(b);
                path.AddEllipse(c);
                g.FillPath(_paper, path);
            }

            if (variant % 3 == 0)
            {
                g.DrawEllipse(_pen, cx - r * 0.8f, cy - r, r * 1.6f, r * 1.5f);
            }
            else
            {
                g.DrawArc(_pen, a, 90, 200);
                g.DrawArc(_pen, c, 160, 220);
                g.DrawArc(_pen, b, 250, 200);
            }
        }

        private void Conifer(Graphics g, float x, float y, float s)
        {
            PointF[] tri =
            {
                new PointF(x, y - s * 1.1f),
                new PointF(x + s * 0.38f, y - s * 0.2f),
                new PointF(x - s * 0.38f, y - s * 0.2f)
            };
            g.FillPolygon(_paper, tri);
            g.DrawPolygon(_pen, tri);
            g.DrawLine(_pen, x, y - s * 0.2f, x, y);
        }

        private void Bush(Graphics g, float x, float y, float s)
        {
            var rect = new RectangleF(x - s * 0.5f, y - s * 0.6f, s, s * 0.7f);
            g.FillEllipse(_paper, rect);
            g.DrawArc(_pen, rect, 160, 220);
        }

        private void Rock(Graphics g, float x, float y, float s)
        {
            PointF[] p =
            {
                new PointF(x - s * 0.5f, y),
                new PointF(x - s * 0.3f, y - s * 0.45f),
                new PointF(x + s * 0.2f, y - s * 0.5f),
                new PointF(x + s * 0.5f, y)
            };
            g.FillPolygon(_paper, p);
            g.DrawPolygon(_pen, p);
            g.DrawLine(_thin, x + s * 0.1f, y - s * 0.4f, x + s * 0.3f, y - s * 0.05f);
        }

        private void House(Graphics g, float x, float y, float s, int variant)
        {
            float bw = s * (0.6f + (variant % 3) * 0.1f);
            float bh = s * 0.4f;
            float left = x - bw / 2f, top = y - bh;
            float roofH = s * 0.38f;
            PointF[] roof =
            {
                new PointF(left - 1, top),
                new PointF(x, top - roofH),
                new PointF(left + bw + 1, top)
            };
            g.FillRectangle(_paper, left, top, bw, bh);
            g.FillPolygon(_paper, roof);
            g.DrawRectangle(_pen, left, top, bw, bh);
            g.DrawPolygon(_pen, roof);
            if (variant % 2 == 0 && x - left > 2)
            {
                for (float hx = left + 2; hx < x; hx += 2)
                {
                    float hy = top - (hx - left) / (x - left) * roofH;
                    g.DrawLine(_thin, hx, top, hx, hy + 1);
                }
            }

            float door = Math.Max(1f, bw * 0.18f);
            g.FillRectangle(_ink, x - door / 2f, top + bh * 0.45f, door, bh * 0.55f);
        }

        private void Tuft(Graphics g, float x, float y, float s)
        {
            g.DrawLine(_thin, x - s * 0.3f, y - s * 0.4f, x - s * 0.1f, y);
            g.DrawLine(_thin, x - s * 0.1f, y, x + s * 0.05f, y - s * 0.5f);
            g.DrawLine(_thin, x + s * 0.05f, y - s * 0.5f, x + s * 0.15f, y);
            g.DrawLine(_thin, x + s * 0.15f, y, x + s * 0.35f, y - s * 0.35f);
        }

        private void Ankh(Graphics g, float x, float y, float s)
        {
            float loopW = s * 0.36f, loopH = s * 0.4f;
            float barY = y - s * 0.62f;
            g.DrawEllipse(_pen, x - loopW / 2f, barY - loopH, loopW, loopH);
            g.DrawLine(_pen, x - s * 0.3f, barY, x + s * 0.3f, barY);
            g.DrawLine(_pen, x, barY, x, y);
        }

        private void Skull(Graphics g, float x, float y, float s)
        {
            float w = s * 0.8f, h = s * 0.62f;
            var head = new RectangleF(x - w / 2f, y - s * 0.95f, w, h);
            g.FillEllipse(_paper, head);
            g.DrawEllipse(_pen, head);
            var jaw = new RectangleF(x - w * 0.3f, head.Bottom - h * 0.25f, w * 0.6f, h * 0.45f);
            g.FillRectangle(_paper, jaw);
            g.DrawRectangle(_pen, jaw.X, jaw.Y, jaw.Width, jaw.Height);
            float eye = w * 0.22f;
            g.FillEllipse(_ink, x - w * 0.3f, head.Y + h * 0.38f, eye, eye);
            g.FillEllipse(_ink, x + w * 0.08f, head.Y + h * 0.38f, eye, eye);
            for (int i = 1; i < 3; i++)
            {
                float tx = jaw.X + jaw.Width * i / 3f;
                g.DrawLine(_thin, tx, jaw.Y + jaw.Height * 0.3f, tx, jaw.Bottom);
            }
        }

        private void Star(Graphics g, float x, float y, float s)
        {
            float cx = x, cy = y - s * 0.5f;
            float r = s * 0.22f;
            g.FillEllipse(_paper, cx - r, cy - r, 2 * r, 2 * r);
            g.DrawEllipse(_pen, cx - r, cy - r, 2 * r, 2 * r);
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4;
                float r1 = r * 1.4f, r2 = s * (i % 2 == 0 ? 0.5f : 0.4f);
                g.DrawLine(_thin, cx + (float)Math.Cos(a) * r1, cy + (float)Math.Sin(a) * r1,
                    cx + (float)Math.Cos(a) * r2, cy + (float)Math.Sin(a) * r2);
            }
        }

        private void Treasure(Graphics g, float x, float y, float s)
        {
            float h = s * 0.4f;
            float cy = y - s * 0.5f;
            g.DrawLine(_pen, x - h, cy - h, x + h, cy + h);
            g.DrawLine(_pen, x - h, cy + h, x + h, cy - h);
        }

        private void Compass(Graphics g, float x, float y, float s)
        {
            float cx = x, cy = y - s * 0.6f, r = s * 0.55f;
            g.FillEllipse(_paper, cx - r * 0.75f, cy - r * 0.75f, r * 1.5f, r * 1.5f);
            g.DrawEllipse(_thin, cx - r * 0.75f, cy - r * 0.75f, r * 1.5f, r * 1.5f);
            for (int i = 0; i < 4; i++)
            {
                double a = i * Math.PI / 2 - Math.PI / 2;
                double b1 = a - Math.PI / 4, b2 = a + Math.PI / 4;
                float rl = i == 0 ? r : r * 0.85f;
                var tip = new PointF(cx + (float)Math.Cos(a) * rl, cy + (float)Math.Sin(a) * rl);
                var s1 = new PointF(cx + (float)Math.Cos(b1) * r * 0.18f, cy + (float)Math.Sin(b1) * r * 0.18f);
                var s2 = new PointF(cx + (float)Math.Cos(b2) * r * 0.18f, cy + (float)Math.Sin(b2) * r * 0.18f);
                PointF[] half1 = { new PointF(cx, cy), s1, tip };
                PointF[] half2 = { new PointF(cx, cy), s2, tip };
                g.FillPolygon(_ink, half1);
                g.FillPolygon(_paper, half2);
                g.DrawPolygon(_thin, new[] { s1, tip, s2, new PointF(cx, cy) });
            }

            // N
            float nh = r * 0.35f, nx = cx - nh * 0.3f, ny = cy - r - nh * 1.25f;
            g.DrawLines(_thin, new[]
            {
                new PointF(nx, ny + nh), new PointF(nx, ny), new PointF(nx + nh * 0.6f, ny + nh), new PointF(nx + nh * 0.6f, ny)
            });
        }

        private void Tower(Graphics g, float x, float y, float s)
        {
            float w = s * 0.42f, h = s * 0.8f;
            var body = new RectangleF(x - w / 2f, y - h, w, h);
            g.FillRectangle(_paper, body);
            g.DrawRectangle(_pen, body.X, body.Y, body.Width, body.Height);
            // battlements
            float bw = w / 5f;
            for (int i = 0; i < 5; i += 2)
            {
                var b = new RectangleF(body.X + i * bw, body.Y - bw, bw, bw);
                g.FillRectangle(_paper, b);
                g.DrawRectangle(_pen, b.X, b.Y, b.Width, b.Height);
            }

            g.FillRectangle(_ink, x - w * 0.12f, y - h * 0.3f, w * 0.24f, h * 0.3f);
            g.FillRectangle(_ink, x - w * 0.08f, y - h * 0.75f, w * 0.16f, h * 0.15f);
        }

        private void Cave(Graphics g, float x, float y, float s)
        {
            var hill = new RectangleF(x - s * 0.6f, y - s * 0.7f, s * 1.2f, s * 1.4f);
            g.FillPie(_paper, hill.X, hill.Y, hill.Width, hill.Height, 180, 180);
            g.DrawArc(_pen, hill, 180, 180);
            g.DrawLine(_pen, hill.Left, y, hill.Right, y);
            var hole = new RectangleF(x - s * 0.22f, y - s * 0.32f, s * 0.44f, s * 0.64f);
            g.FillPie(_ink, hole.X, hole.Y, hole.Width, hole.Height, 180, 180);
        }

        private void Marker(Graphics g, float x, float y, float s)
        {
            float r = s * 0.3f;
            float cy = y - s * 0.7f;
            using (var path = new GraphicsPath())
            {
                path.AddArc(x - r, cy - r, 2 * r, 2 * r, 150, 240);
                path.AddLine(x + r * 0.87f, cy + r * 0.5f, x, y);
                path.AddLine(x, y, x - r * 0.87f, cy + r * 0.5f);
                path.CloseFigure();
                g.FillPath(_paper, path);
                g.DrawPath(_pen, path);
            }

            g.FillEllipse(_ink, x - r * 0.35f, cy - r * 0.35f, r * 0.7f, r * 0.7f);
        }
    }
}
