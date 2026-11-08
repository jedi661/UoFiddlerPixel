/***************************************************************************
 *
 * MultiEditor – background below the multi (pattern / image / map area),
 * grid and height display.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    public enum BackgroundMode
    {
        None,
        Pattern,
        Image,
        Map
    }

    public sealed class BackgroundPattern
    {
        public BackgroundPattern(string de, string en, params int[] landIds)
        {
            NameDe = de;
            NameEn = en;
            LandIds = landIds;
        }

        public string NameDe { get; }
        public string NameEn { get; }
        public int[] LandIds { get; set; }
        public string Name => L.T(NameDe, NameEn);
    }

    internal sealed class BackgroundLayer
    {
        public static readonly BackgroundPattern[] Patterns =
        {
            new BackgroundPattern("Gras", "Grass", 0x03, 0x04, 0x05, 0x06),
            new BackgroundPattern("Wasser", "Water", 0xA8, 0xA9, 0xAA, 0xAB),
            new BackgroundPattern("Sand", "Sand", 0x16, 0x17, 0x18, 0x19),
            new BackgroundPattern("Erde", "Dirt", 0x71, 0x72, 0x73, 0x74),
            new BackgroundPattern("Schnee", "Snow", 0x11A, 0x11B, 0x11C, 0x11D),
            new BackgroundPattern("Wald", "Forest", 0xC4, 0xC5, 0xC6, 0xC7),
            new BackgroundPattern("Eigene IDs", "Custom ids", 0x03)
        };

        private ImageAttributes _staticsAttributes;
        private ImageAttributes _imageAttributes;
        private float _staticsAlpha = 1f;
        private float _imageAlpha = 0.6f;

        public BackgroundMode Mode { get; set; } = BackgroundMode.None;

        public BackgroundPattern Pattern { get; set; } = Patterns[0];

        /// <summary>
        /// Extra cells drawn around the multi
        /// </summary>
        public int Margin { get; set; } = 6;

        public Bitmap Image { get; set; }
        public string ImagePath { get; set; }
        public int ImageOffsetX { get; set; }
        public int ImageOffsetY { get; set; }
        public float ImageScale { get; set; } = 1f;

        public float ImageAlpha
        {
            get => _imageAlpha;
            set
            {
                _imageAlpha = Math.Max(0.05f, Math.Min(1f, value));
                _imageAttributes = CreateAlpha(_imageAlpha);
            }
        }

        public Map Map { get; set; }
        public int MapIndex { get; set; }

        /// <summary>
        /// World coordinate of multi cell 0,0
        /// </summary>
        public int MapX { get; set; }

        public int MapY { get; set; }

        /// <summary>
        /// World Z that is multi Z 0
        /// </summary>
        public int BaseZ { get; set; }

        public bool ShowStatics { get; set; } = true;

        public bool ShowLand { get; set; } = true;

        public float StaticsAlpha
        {
            get => _staticsAlpha;
            set
            {
                _staticsAlpha = Math.Max(0.05f, Math.Min(1f, value));
                _staticsAttributes = _staticsAlpha >= 0.99f ? null : CreateAlpha(_staticsAlpha);
            }
        }

        /// <summary>
        /// Highlight statics with this id (find static), -1 = off
        /// </summary>
        public int HighlightStaticId { get; set; } = -1;

        public bool HasMap => Mode == BackgroundMode.Map && Map != null;

        /// <summary>
        /// View cell -> cell in original orientation (rotated view), null = identity
        /// </summary>
        public Func<int, int, Point> CellMapper { get; set; }

        /// <summary>
        /// World position of a view cell
        /// </summary>
        public Point CellToWorld(int x, int y)
        {
            Point c = CellMapper?.Invoke(x, y) ?? new Point(x, y);
            return new Point(MapX + c.X, MapY + c.Y);
        }

        private static ImageAttributes CreateAlpha(float alpha)
        {
            var cm = new ColorMatrix { Matrix33 = alpha };
            var ia = new ImageAttributes();
            ia.SetColorMatrix(cm);
            return ia;
        }

        public static Point CellToScreen(MultiEditorComponentList list, int x, int y, int z, int xOff, int yOff)
        {
            int px = ((x - y) * 22) - 22 + MultiEditorComponentList.GapXMod - list.XMin - xOff;
            int py = ((x + y) * 22) - (z * 4) - 44 + MultiEditorComponentList.GapYMod - list.YMin - yOff;
            return new Point(px, py);
        }

        private static int Hash(int x, int y)
        {
            unchecked
            {
                int h = (x * 73856093) ^ (y * 19349663);
                return Math.Abs(h);
            }
        }

        public bool TryGetLand(int x, int y, out int landId, out int z)
        {
            landId = 0;
            z = 0;
            if (!HasMap)
            {
                return false;
            }

            Point w = CellToWorld(x, y);
            int wx = w.X;
            int wy = w.Y;
            if (wx < 0 || wy < 0 || wx >= Map.Width || wy >= Map.Height)
            {
                return false;
            }

            try
            {
                Tile t = Map.Tiles.GetLandTile(wx, wy);
                landId = t.Id & 0x3FFF;
                z = t.Z;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public HuedTile[] GetStatics(int x, int y)
        {
            if (!HasMap)
            {
                return Array.Empty<HuedTile>();
            }

            Point w = CellToWorld(x, y);
            int wx = w.X;
            int wy = w.Y;
            if (wx < 0 || wy < 0 || wx >= Map.Width || wy >= Map.Height)
            {
                return Array.Empty<HuedTile>();
            }

            try
            {
                return Map.Tiles.GetStaticTiles(wx, wy) ?? Array.Empty<HuedTile>();
            }
            catch
            {
                return Array.Empty<HuedTile>();
            }
        }

        /// <summary>
        /// Land Z relative to <see cref="BaseZ"/> (0 without map)
        /// </summary>
        public int GetRelativeGroundZ(int x, int y)
        {
            return TryGetLand(x, y, out _, out int z) ? z - BaseZ : 0;
        }

        /// <summary>
        /// Increase after map statics were changed (redraws the cached background)
        /// </summary>
        public int ContentVersion { get; set; }

        /// <summary>
        /// Shading of sloped land like the client
        /// </summary>
        public bool Lighting { get; set; } = true;

        private Bitmap _cache;
        private string _cacheKey;
        private Point _cacheOrigin;
        private readonly Dictionary<int, Bitmap> _textures = new Dictionary<int, Bitmap>();
        private readonly Dictionary<int, ImageAttributes> _light = new Dictionary<int, ImageAttributes>();

        public void InvalidateCache()
        {
            _cacheKey = null;
        }

        /// <summary>
        /// Draws the background (call before the multi is drawn)
        /// </summary>
        public void Draw(Graphics g, MultiEditorComponentList list, int xOff, int yOff)
        {
            if (list == null || Mode == BackgroundMode.None)
            {
                return;
            }

            switch (Mode)
            {
                case BackgroundMode.Image:
                    DrawImage(g, list, xOff, yOff);
                    return;
                case BackgroundMode.Pattern:
                    DrawPattern(g, list, xOff, yOff);
                    return;
                case BackgroundMode.Map:
                    DrawMapCached(g, list, xOff, yOff);
                    return;
            }
        }

        private void DrawPattern(Graphics g, MultiEditorComponentList list, int xOff, int yOff)
        {
            Rectangle clip = Rectangle.Round(g.ClipBounds);
            int[] ids = Pattern.LandIds;
            if (ids == null || ids.Length == 0)
            {
                return;
            }

            for (int x = -Margin; x <= list.Width + Margin; ++x)
            {
                for (int y = -Margin; y <= list.Height + Margin; ++y)
                {
                    Bitmap bmp = Art.GetLand(ids[Hash(x, y) % ids.Length]);
                    if (bmp == null)
                    {
                        continue;
                    }

                    Point p = CellToScreen(list, x, y, 0, xOff, yOff);
                    var dest = new Rectangle(p.X, p.Y, bmp.Width, bmp.Height);
                    if (clip.IntersectsWith(dest))
                    {
                        g.DrawImageUnscaled(bmp, dest);
                    }
                }
            }
        }

        /// <summary>
        /// Land Z of a cell relative to the base Z (0 outside the map)
        /// </summary>
        private int CornerZ(int x, int y, int fallback)
        {
            return TryGetLand(x, y, out _, out int z) ? z - BaseZ : fallback;
        }

        private void DrawMapCached(Graphics g, MultiEditorComponentList list, int xOff, int yOff)
        {
            string key = string.Join("|", Map?.GetHashCode(), MapX, MapY, BaseZ, list.Width, list.Height, Margin, list.XMin, list.YMin,
                ShowLand, ShowStatics, StaticsAlpha, HighlightStaticId, list.ViewRotation, ContentVersion, Lighting);
            if (key != _cacheKey || _cache == null)
            {
                RenderMapCache(list);
                _cacheKey = key;
            }

            if (_cache != null)
            {
                g.DrawImage(_cache, _cacheOrigin.X - xOff, _cacheOrigin.Y - yOff, _cache.Width, _cache.Height);
            }
        }

        private void RenderMapCache(MultiEditorComponentList list)
        {
            _cache?.Dispose();
            _cache = null;
            if (Map == null)
            {
                return;
            }

            int x0 = -Margin, y0 = -Margin, x1 = list.Width + Margin, y1 = list.Height + Margin;
            int zMin = 0, zMax = 0;
            for (int x = x0; x <= x1 + 1; ++x)
            {
                for (int y = y0; y <= y1 + 1; ++y)
                {
                    int z = CornerZ(x, y, 0);
                    zMin = Math.Min(zMin, z);
                    zMax = Math.Max(zMax, z);
                    foreach (HuedTile st in GetStatics(x, y))
                    {
                        zMax = Math.Max(zMax, st.Z - BaseZ);
                    }
                }
            }

            const int padX = 160;
            const int padTop = 320;
            int minX = ((x0 - y1) * 22) - 22 + MultiEditorComponentList.GapXMod - list.XMin - padX;
            int maxX = ((x1 - y0) * 22) + 22 + MultiEditorComponentList.GapXMod - list.XMin + padX;
            int minY = ((x0 + y0) * 22) - (zMax * 4) - 44 + MultiEditorComponentList.GapYMod - list.YMin - padTop;
            int maxY = ((x1 + y1) * 22) - (zMin * 4) + 44 + MultiEditorComponentList.GapYMod - list.YMin;
            int w = Math.Min(8000, Math.Max(1, maxX - minX));
            int h = Math.Min(8000, Math.Max(1, maxY - minY));

            _cache = new Bitmap(w, h);
            _cacheOrigin = new Point(minX, minY);
            using Graphics g = Graphics.FromImage(_cache);
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.Bilinear;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            try
            {
                DrawMapDirect(g, list, minX, minY, x0, y0, x1, y1);
            }
            catch (Exception)
            {
                // never break the editor because of the background
            }
        }

        private Bitmap GetTexture(int landId)
        {
            if (_textures.TryGetValue(landId, out Bitmap tex))
            {
                return tex;
            }

            try
            {
                int texId = TileData.LandTable[landId].TextureId;
                tex = texId > 0 ? Textures.GetTexture(texId) : null;
            }
            catch
            {
                tex = null;
            }

            _textures[landId] = tex;
            return tex;
        }

        private ImageAttributes LightAttributes(float light)
        {
            int key = (int)Math.Round(light * 40);
            if (_light.TryGetValue(key, out ImageAttributes ia))
            {
                return ia;
            }

            float l = key / 40f;
            var m = new ColorMatrix(new[]
            {
                new[] { l, 0f, 0f, 0f, 0f },
                new[] { 0f, l, 0f, 0f, 0f },
                new[] { 0f, 0f, l, 0f, 0f },
                new[] { 0f, 0f, 0f, 1f, 0f },
                new[] { 0f, 0f, 0f, 0f, 1f }
            });
            ia = new ImageAttributes();
            ia.SetColorMatrix(m);
            ia.SetWrapMode(WrapMode.TileFlipXY);
            _light[key] = ia;
            return ia;
        }

        private static float Light(float nx, float ny)
        {
            // same as the map viewer: light from north west and above
            const float lx = -0.4558f, ly = -0.4558f, lz = 0.7646f;
            float len = (float)Math.Sqrt((nx * nx) + (ny * ny) + 1);
            float dot = ((nx * lx) + (ny * ly) + lz) / len;
            float b = 1f + ((dot - 0.7646f) * 1.4f);
            return Math.Max(0.45f, Math.Min(1.35f, b));
        }

        private static PointF[] Expand(PointF a, PointF b, PointF c, float by)
        {
            var center = new PointF((a.X + b.X + c.X) / 3f, (a.Y + b.Y + c.Y) / 3f);

            PointF E(PointF p)
            {
                float dx = p.X - center.X;
                float dy = p.Y - center.Y;
                float len = (float)Math.Sqrt((dx * dx) + (dy * dy));
                return len < 0.01f ? p : new PointF(p.X + (dx / len * by), p.Y + (dy / len * by));
            }

            return new[] { E(a), E(b), E(c) };
        }

        /// <summary>
        /// Draws a source triangle of an image into a destination triangle (affine)
        /// </summary>
        private void DrawTriangle(Graphics g, Bitmap src, PointF s0, PointF s1, PointF s2, PointF d0, PointF d1, PointF d2, float light)
        {
            float ux1 = s1.X - s0.X, uy1 = s1.Y - s0.Y, ux2 = s2.X - s0.X, uy2 = s2.Y - s0.Y;
            float vx1 = d1.X - d0.X, vy1 = d1.Y - d0.Y, vx2 = d2.X - d0.X, vy2 = d2.Y - d0.Y;
            float det = (ux1 * uy2) - (ux2 * uy1);
            if (Math.Abs(det) < 0.0001f)
            {
                return;
            }

            // destination triangle without area (very steep land): invisible, and GDI+ can't use the matrix
            float destArea = (vx1 * vy2) - (vx2 * vy1);
            if (Math.Abs(destArea) < 1f)
            {
                return;
            }

            float a = ((vx1 * uy2) - (vx2 * uy1)) / det;
            float c = ((-vx1 * ux2) + (vx2 * ux1)) / det;
            float b = ((vy1 * uy2) - (vy2 * uy1)) / det;
            float d = ((-vy1 * ux2) + (vy2 * ux1)) / det;
            float e = d0.X - (a * s0.X) - (c * s0.Y);
            float f = d0.Y - (b * s0.X) - (d * s0.Y);

            if (float.IsNaN(a) || float.IsNaN(b) || float.IsNaN(c) || float.IsNaN(d) || float.IsInfinity(e) || float.IsInfinity(f))
            {
                return;
            }

            GraphicsState state = g.Save();
            try
            {
                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(Expand(d0, d1, d2, 0.75f));
                    g.SetClip(path, CombineMode.Intersect);
                }

                using (var m = new Matrix(a, b, c, d, e, f))
                {
                    if (!m.IsInvertible)
                    {
                        return;
                    }

                    g.MultiplyTransform(m, MatrixOrder.Prepend);
                }

                g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, LightAttributes(light));
            }
            catch (ArgumentException)
            {
                // GDI+ refused this triangle, skip it
            }
            finally
            {
                g.Restore(state);
            }
        }

        /// <summary>
        /// Land diamond corners (top, right, bottom, left) with the heights of the 4 corners like the client
        /// </summary>
        public PointF[] GetLandCorners(MultiEditorComponentList list, int x, int y, int xOff, int yOff, out bool flat)
        {
            int zT = CornerZ(x, y, 0);
            int zR = CornerZ(x + 1, y, zT);
            int zB = CornerZ(x + 1, y + 1, zT);
            int zL = CornerZ(x, y + 1, zT);
            flat = zT == zR && zT == zB && zT == zL;
            Point p = CellToScreen(list, x, y, 0, xOff, yOff);
            return new[]
            {
                new PointF(p.X + 22, p.Y - (zT * 4)),
                new PointF(p.X + 44, p.Y + 22 - (zR * 4)),
                new PointF(p.X + 22, p.Y + 44 - (zB * 4)),
                new PointF(p.X, p.Y + 22 - (zL * 4))
            };
        }

        private void DrawMapDirect(Graphics g, MultiEditorComponentList list, int xOff, int yOff, int x0, int y0, int x1, int y1)
        {
            if (ShowLand)
            {
                for (int sum = x0 + y0; sum <= x1 + y1; ++sum)
                {
                    for (int x = x0; x <= x1; ++x)
                    {
                        int y = sum - x;
                        if (y < y0 || y > y1 || !TryGetLand(x, y, out int landId, out int z))
                        {
                            continue;
                        }

                        PointF[] c = GetLandCorners(list, x, y, xOff, yOff, out bool flat);
                        Bitmap tex = flat ? null : GetTexture(landId);
                        if (flat || tex == null)
                        {
                            Bitmap art = Art.GetLand(landId);
                            if (art == null)
                            {
                                continue;
                            }

                            if (flat)
                            {
                                g.DrawImageUnscaled(art, (int)c[3].X, (int)c[0].Y);
                                continue;
                            }

                            // no texture: stretch the land art
                            DrawTriangle(g, art, new PointF(22, 0), new PointF(44, 22), new PointF(0, 22), c[0], c[1], c[3], 1f);
                            DrawTriangle(g, art, new PointF(44, 22), new PointF(22, 44), new PointF(0, 22), c[1], c[2], c[3], 1f);
                            continue;
                        }

                        int t = tex.Width;
                        float light1 = 1f;
                        float light2 = 1f;
                        if (Lighting)
                        {
                            const float k = 0.125f;
                            float zT = (z - BaseZ) * k;
                            float zR = CornerZ(x + 1, y, z - BaseZ) * k;
                            float zB = CornerZ(x + 1, y + 1, z - BaseZ) * k;
                            float zL = CornerZ(x, y + 1, z - BaseZ) * k;
                            light1 = Light(-(zR - zT), -(zL - zT));
                            light2 = Light(zL - zB, zR - zB);
                        }

                        DrawTriangle(g, tex, new PointF(0, 0), new PointF(t, 0), new PointF(0, t), c[0], c[1], c[3], light1);
                        DrawTriangle(g, tex, new PointF(t, 0), new PointF(t, t), new PointF(0, t), c[1], c[2], c[3], light2);
                    }
                }
            }

            if (!ShowStatics)
            {
                return;
            }

            for (int sum = x0 + y0; sum <= x1 + y1; ++sum)
            {
                for (int x = x0; x <= x1; ++x)
                {
                    int y = sum - x;
                    if (y < y0 || y > y1)
                    {
                        continue;
                    }

                    HuedTile[] statics = GetStatics(x, y);
                    if (statics.Length == 0)
                    {
                        continue;
                    }

                    foreach (HuedTile st in statics.OrderBy(s => s.Z).ThenBy(s => TileData.ItemTable[s.Id].Background ? 0 : 1))
                    {
                        Bitmap bmp = Art.GetStatic(st.Id);
                        if (bmp == null)
                        {
                            continue;
                        }

                        if (st.Hue > 0)
                        {
                            bmp = MultiTile.GetHuedBitmap(st.Id, st.Hue, bmp);
                        }

                        int px = ((x - y) * 22) - (bmp.Width / 2) + MultiEditorComponentList.GapXMod - list.XMin - xOff;
                        int py = ((x + y) * 22) - ((st.Z - BaseZ) * 4) - bmp.Height + MultiEditorComponentList.GapYMod - list.YMin - yOff;
                        var dest = new Rectangle(px, py, bmp.Width, bmp.Height);

                        if (_staticsAttributes != null)
                        {
                            g.DrawImage(bmp, dest, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, _staticsAttributes);
                        }
                        else
                        {
                            g.DrawImageUnscaled(bmp, dest);
                        }

                        if (HighlightStaticId >= 0 && st.Id == HighlightStaticId)
                        {
                            using var pen = new Pen(Color.Magenta, 2);
                            g.DrawRectangle(pen, dest);
                        }
                    }
                }
            }
        }

        private void DrawImage(Graphics g, MultiEditorComponentList list, int xOff, int yOff)
        {
            if (Image == null)
            {
                return;
            }

            // anchored at the top corner of cell 0,0
            Point p = CellToScreen(list, 0, 0, 0, xOff, yOff);
            int w = (int)(Image.Width * ImageScale);
            int h = (int)(Image.Height * ImageScale);
            var dest = new Rectangle(p.X + 22 + ImageOffsetX - (w / 2), p.Y + ImageOffsetY, w, h);
            if (_imageAttributes == null)
            {
                _imageAttributes = CreateAlpha(_imageAlpha);
            }

            g.DrawImage(Image, dest, 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, _imageAttributes);
        }

        /// <summary>
        /// Grid lines for every multi cell (on the virtual floor or following the map ground)
        /// </summary>
        public void DrawGrid(Graphics g, MultiEditorComponentList list, int xOff, int yOff, int floorZ, bool useGround)
        {
            if (list == null)
            {
                return;
            }

            bool ground = useGround && HasMap;
            using var pen = new Pen(Color.FromArgb(110, 255, 255, 255), 1);
            using var originPen = new Pen(Color.FromArgb(220, 255, 64, 64), 2);
            int m = ground ? Math.Min(Margin, 4) : 0;
            for (int x = -m; x < list.Width + m; ++x)
            {
                for (int y = -m; y < list.Height + m; ++y)
                {
                    PointF[] d;
                    if (ground)
                    {
                        d = GetLandCorners(list, x, y, xOff, yOff, out _);
                    }
                    else
                    {
                        Point p = CellToScreen(list, x, y, floorZ, xOff, yOff);
                        d = new[] { new PointF(p.X + 22, p.Y), new PointF(p.X + 44, p.Y + 22), new PointF(p.X + 22, p.Y + 44), new PointF(p.X, p.Y + 22) };
                    }

                    bool inside = x >= 0 && y >= 0 && x < list.Width && y < list.Height;
                    if (!inside)
                    {
                        using var faint = new Pen(Color.FromArgb(45, 255, 255, 255), 1);
                        g.DrawPolygon(faint, d);
                        continue;
                    }

                    g.DrawPolygon(x == list.OriginX && y == list.OriginY ? originPen : pen, d);
                }
            }

            // outer border of the multi
            using var borderPen = new Pen(Color.FromArgb(200, 255, 200, 0), 2) { DashStyle = DashStyle.Dash };
            Point a = CellToScreen(list, 0, 0, floorZ, xOff, yOff);
            Point b = CellToScreen(list, list.Width - 1, 0, floorZ, xOff, yOff);
            Point c = CellToScreen(list, list.Width - 1, list.Height - 1, floorZ, xOff, yOff);
            Point d2 = CellToScreen(list, 0, list.Height - 1, floorZ, xOff, yOff);
            if (!ground)
            {
                g.DrawPolygon(borderPen, new[]
                {
                    new Point(a.X + 22, a.Y),
                    new Point(b.X + 44, b.Y + 22),
                    new Point(c.X + 22, c.Y + 44),
                    new Point(d2.X, d2.Y + 22)
                });
            }
        }

        /// <summary>
        /// Shows the ground height (map) of every cell
        /// </summary>
        public void DrawHeights(Graphics g, MultiEditorComponentList list, int xOff, int yOff, Font font)
        {
            if (list == null || !HasMap)
            {
                return;
            }

            using var shadow = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
            for (int x = -1; x <= list.Width; ++x)
            {
                for (int y = -1; y <= list.Height; ++y)
                {
                    if (!TryGetLand(x, y, out _, out int z))
                    {
                        continue;
                    }

                    PointF[] c = GetLandCorners(list, x, y, xOff, yOff, out _);
                    float cx = (c[0].X + c[1].X + c[2].X + c[3].X) / 4f;
                    float cy = (c[0].Y + c[1].Y + c[2].Y + c[3].Y) / 4f;
                    string text = z.ToString();
                    SizeF size = g.MeasureString(text, font);
                    float tx = cx - (size.Width / 2);
                    float ty = cy - (size.Height / 2);
                    g.DrawString(text, font, shadow, tx + 1, ty + 1);
                    Brush brush = z == BaseZ ? Brushes.White : z > BaseZ ? Brushes.Orange : Brushes.LightSkyBlue;
                    g.DrawString(text, font, brush, tx, ty);
                }
            }
        }

        /// <summary>
        /// Top static at a cell (for pipette), null if none
        /// </summary>
        public HuedTile? GetTopStatic(int x, int y)
        {
            HuedTile[] statics = GetStatics(x, y);
            if (statics.Length == 0)
            {
                return null;
            }

            return statics.OrderByDescending(s => s.Z).First();
        }

        /// <summary>
        /// Statics inside the multi area as import tiles (cell coordinates, Z relative to <see cref="BaseZ"/>)
        /// </summary>
        public List<ImportTile> GetStaticsAsTiles(int width, int height)
        {
            var list = new List<ImportTile>();
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    foreach (HuedTile st in GetStatics(x, y))
                    {
                        list.Add(new ImportTile { Id = st.Id, X = x, Y = y, Z = st.Z - BaseZ, Hue = st.Hue, Flag = 1 });
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Counts statics with given id in the visible area and returns their cells
        /// </summary>
        public List<Point> FindStatic(int id, int width, int height)
        {
            var result = new List<Point>();
            for (int x = -Margin; x < width + Margin; ++x)
            {
                for (int y = -Margin; y < height + Margin; ++y)
                {
                    if (GetStatics(x, y).Any(s => s.Id == id))
                    {
                        result.Add(new Point(x, y));
                    }
                }
            }

            return result;
        }

        public string Describe()
        {
            switch (Mode)
            {
                case BackgroundMode.Pattern:
                    return L.T("Muster: ", "Pattern: ") + Pattern.Name;
                case BackgroundMode.Image:
                    return L.T("Bild: ", "Image: ") + ImagePath;
                case BackgroundMode.Map:
                    return Map == null ? "Map" : $"Map {MapIndex}  {MapX},{MapY}  Z {BaseZ}";
                default:
                    return L.T("Kein Hintergrund", "No background");
            }
        }
    }
}
