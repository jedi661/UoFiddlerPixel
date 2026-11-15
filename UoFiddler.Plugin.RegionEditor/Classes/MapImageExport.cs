/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Map image with the regions of a facet (PNG export, HTML overview).
 * The map is rendered in pieces of 64×64 blocks so even large maps
 * need little memory.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Ultima;

namespace UoFiddler.Plugin.RegionEditor.Classes
{
    public static class MapImageExport
    {
        /// <param name="scale">pixels per map cell (e.g. 0.25 = a quarter)</param>
        public static Bitmap Render(MapDefinition def, FacetRegions facet, double scale, bool statics, bool labels, int fillAlpha)
        {
            Map map = def.GetMap();
            int width = Math.Max(1, (int)(def.Width * scale)), height = Math.Max(1, (int)(def.Height * scale));
            var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Black);
                g.InterpolationMode = scale < 1 ? InterpolationMode.HighQualityBilinear : InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;

                const int piece = 64; // blocks
                int blocksW = def.Width >> 3, blocksH = def.Height >> 3;
                using (var buffer = new Bitmap(piece << 3, piece << 3, PixelFormat.Format16bppRgb555))
                {
                    for (int by = 0; by < blocksH; by += piece)
                    {
                        for (int bx = 0; bx < blocksW; bx += piece)
                        {
                            int bw = Math.Min(piece, blocksW - bx), bh = Math.Min(piece, blocksH - by);
                            try
                            {
                                map.GetImage(bx, by, bw, bh, buffer, statics);
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            var dest = new RectangleF((float)((bx << 3) * scale), (float)((by << 3) * scale), (float)((bw << 3) * scale), (float)((bh << 3) * scale));
                            g.DrawImage(buffer, dest, new RectangleF(0, 0, bw << 3, bh << 3), GraphicsUnit.Pixel);
                        }
                    }
                }

                g.PixelOffsetMode = PixelOffsetMode.Default;
                if (facet != null)
                {
                    DrawRegions(g, facet, scale, labels, fillAlpha);
                }
            }

            return result;
        }

        private static void DrawRegions(Graphics g, FacetRegions facet, double scale, bool labels, int fillAlpha)
        {
            using (var font = new Font("Segoe UI", Math.Max(7f, (float)(10 * Math.Min(1, scale * 2))), FontStyle.Bold))
            {
                foreach (RegionNode r in facet.AllRegions().Where(r => !r.Hidden))
                {
                    Color c = r.Color.HasValue ? Color.FromArgb(255, Color.FromArgb(r.Color.Value)) : RegionCatalog.TypeColor(r.Type);
                    using (var fill = new SolidBrush(Color.FromArgb(fillAlpha, c)))
                    using (var pen = new Pen(c, 1.5f))
                    {
                        foreach (RegionRect rect in r.Rects)
                        {
                            var s = new RectangleF((float)(rect.X * scale), (float)(rect.Y * scale), (float)(rect.Width * scale), (float)(rect.Height * scale));
                            g.FillRectangle(fill, s);
                            g.DrawRectangle(pen, s.X, s.Y, s.Width, s.Height);
                        }

                        foreach (RegionPolygon p in r.Polygons.Where(p => p.Points.Count >= 3))
                        {
                            PointF[] pts = p.Points.Select(pt => new PointF((float)(pt.X * scale), (float)(pt.Y * scale))).ToArray();
                            g.FillPolygon(fill, pts);
                            g.DrawPolygon(pen, pts);
                        }
                    }

                    if (labels && !r.Bounds.IsEmpty && r.Bounds.Width * scale > 30)
                    {
                        g.DrawString(r.DisplayName, font, Brushes.Black, (float)(r.Bounds.X * scale) + 3, (float)(r.Bounds.Y * scale) + 3);
                        g.DrawString(r.DisplayName, font, Brushes.White, (float)(r.Bounds.X * scale) + 2, (float)(r.Bounds.Y * scale) + 2);
                    }
                }
            }
        }

        public static byte[] ToPng(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
