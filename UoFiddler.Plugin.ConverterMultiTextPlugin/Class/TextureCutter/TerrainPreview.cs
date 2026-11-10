using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter
{
    /// <summary>A kind of ground: a texture (for sloped tiles) and a land tile (for flat tiles).</summary>
    internal sealed class TerrainKind : IDisposable
    {
        public string Name { get; set; }

        /// <summary>Square texture, used for sloped tiles and the 2D view.</summary>
        public Bitmap Texture { get; private set; }

        /// <summary>44 × 44 land tile, used for flat tiles (like the client).</summary>
        public Bitmap LandArt { get; private set; }

        public int LandId { get; set; } = -1;

        public bool IsEditor { get; set; }

        /// <summary>Sets the graphics. A 44 × 44 image counts as land tile, anything else as texture. Takes over the bitmap.</summary>
        public void SetImage(Bitmap image)
        {
            Texture?.Dispose();
            LandArt?.Dispose();
            Texture = LandArt = null;
            if (image == null)
            {
                return;
            }
            if (image.Width == TextureImaging.LandTileSize && image.Height == TextureImaging.LandTileSize)
            {
                LandArt = image;
                Texture = TextureImaging.LandTileToTexture(image, 64);
                return;
            }
            if (image.Width != image.Height || image.Width > 256)
            {
                int s = Math.Min(256, Math.Max(image.Width, image.Height));
                Bitmap square = TextureImaging.Resize(image, s, s, TextureImaging.ResizeQuality.Smooth);
                image.Dispose();
                image = square;
            }
            Texture = image;
            LandArt = TextureImaging.TextureToLandTile(image, false);
        }

        /// <summary>Texture and land tile from art.mul / texmaps.mul (both may exist).</summary>
        public void SetImages(Bitmap texture, Bitmap landArt)
        {
            Texture?.Dispose();
            LandArt?.Dispose();
            Texture = texture ?? (landArt != null ? TextureImaging.LandTileToTexture(landArt, 64) : null);
            LandArt = landArt ?? (texture != null ? TextureImaging.TextureToLandTile(texture, false) : null);
        }

        public Bitmap Thumbnail => LandArt ?? Texture;

        public override string ToString() => Name;

        public void Dispose()
        {
            Texture?.Dispose();
            LandArt?.Dispose();
        }
    }

    /// <summary>An N × N piece of land: kind and height per cell.</summary>
    internal sealed class TerrainScene
    {
        public TerrainScene(int size)
        {
            Size = size;
            Kind = new int[size * size];
            Z = new int[size * size];
        }

        public int Size { get; }

        public int[] Kind { get; }

        public int[] Z { get; }

        /// <summary>Map position of cell (0, 0) when the scene was loaded from a map.</summary>
        public Point? MapOrigin { get; set; }

        public string MapName { get; set; }

        public int GetZ(int x, int y)
        {
            x = Math.Max(0, Math.Min(Size - 1, x));
            y = Math.Max(0, Math.Min(Size - 1, y));
            return Z[y * Size + x];
        }

        public bool Inside(int x, int y) => x >= 0 && y >= 0 && x < Size && y < Size;
    }

    internal sealed class TerrainRenderOptions
    {
        public bool Isometric = true;
        public bool Grid = true;
        public bool Light = true;
        public bool Heights;
        public bool ArtForFlat = true;
        public int CellSize2D = 48;
        public Point Highlight = new Point(-1, -1);
    }

    /// <summary>Draws a terrain scene like the UO client (isometric, with heights) or from above.</summary>
    internal static class TerrainRenderer
    {
        public const int Half = 22;
        public const int ZScale = 4;

        public static Size MeasureIso(TerrainScene s, out Point origin)
        {
            int zmax = Math.Max(0, s.Z.Max());
            int zmin = Math.Min(0, s.Z.Min());
            origin = new Point(s.Size * Half + 12, zmax * ZScale + 12);
            return new Size(2 * s.Size * Half + 24, origin.Y + 2 * s.Size * Half - zmin * ZScale + 24);
        }

        public static PointF Vertex(Point origin, int mx, int my, int z) =>
            new PointF(origin.X + (mx - my) * Half, origin.Y + (mx + my) * Half - z * ZScale);

        /// <summary>The four corners of a cell: top, right, bottom, left.</summary>
        public static PointF[] Corners(TerrainScene s, Point origin, int x, int y) => new[]
        {
            Vertex(origin, x, y, s.GetZ(x, y)),
            Vertex(origin, x + 1, y, s.GetZ(x + 1, y)),
            Vertex(origin, x + 1, y + 1, s.GetZ(x + 1, y + 1)),
            Vertex(origin, x, y + 1, s.GetZ(x, y + 1))
        };

        public static Bitmap Render(TerrainScene s, IReadOnlyList<TerrainKind> kinds, TerrainRenderOptions o)
        {
            return o.Isometric ? RenderIso(s, kinds, o) : RenderTop(s, kinds, o);
        }

        private static ImageAttributes Attributes(float brightness)
        {
            var a = new ImageAttributes();
            a.SetWrapMode(WrapMode.TileFlipXY);
            if (Math.Abs(brightness - 1) > 0.01f)
            {
                a.SetColorMatrix(new ColorMatrix(new[]
                {
                    new[] { brightness, 0f, 0f, 0f, 0f },
                    new[] { 0f, brightness, 0f, 0f, 0f },
                    new[] { 0f, 0f, brightness, 0f, 0f },
                    new[] { 0f, 0f, 0f, 1f, 0f },
                    new[] { 0f, 0f, 0f, 0f, 1f }
                }));
            }
            return a;
        }

        private static PointF[] Grow(PointF[] poly, float by)
        {
            float cx = poly.Average(p => p.X), cy = poly.Average(p => p.Y);
            return poly.Select(p =>
            {
                float dx = p.X - cx, dy = p.Y - cy;
                float len = (float)Math.Sqrt(dx * dx + dy * dy);
                return len < 0.01f ? p : new PointF(p.X + dx / len * by, p.Y + dy / len * by);
            }).ToArray();
        }

        /// <summary>
        /// Stretches a texture onto the quad top-right-bottom-left the way the client does: two triangles.
        /// The top left corner of the texture lies on the top corner.
        /// </summary>
        private static void DrawTexturedQuad(Graphics g, Bitmap tex, PointF[] c, float brightness)
        {
            PointF t = c[0], r = c[1], b = c[2], l = c[3];
            var src = new RectangleF(0, 0, tex.Width, tex.Height);
            using (ImageAttributes attributes = Attributes(brightness))
            {
                GraphicsState state = g.Save();
                // Triangle right-bottom-left, drawn over the whole quad (no gaps) ...
                using (var quad = new GraphicsPath())
                {
                    quad.AddPolygon(Grow(c, 0.8f));
                    g.SetClip(quad);
                    var p2 = new[] { new PointF(r.X + l.X - b.X, r.Y + l.Y - b.Y), r, l };
                    g.DrawImage(tex, p2, src, GraphicsUnit.Pixel, attributes);
                }
                // ... then triangle top-right-left on top.
                using (var tri = new GraphicsPath())
                {
                    tri.AddPolygon(Grow(new[] { t, r, l }, 0.5f));
                    g.SetClip(tri);
                    g.DrawImage(tex, new[] { t, r, l }, src, GraphicsUnit.Pixel, attributes);
                }
                g.Restore(state);
            }
        }

        private static float Light(int zt, int zr, int zb, int zl)
        {
            // Light from the upper left: slopes rising to the lower right face the light.
            float f = 1f + 0.03f * ((zr + zb) - (zt + zl));
            return Math.Max(0.55f, Math.Min(1.35f, f));
        }

        private static Bitmap RenderIso(TerrainScene s, IReadOnlyList<TerrainKind> kinds, TerrainRenderOptions o)
        {
            Size size = MeasureIso(s, out Point origin);
            var bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            using (var gridPen = new Pen(Color.FromArgb(110, 255, 255, 255)))
            using (var font = new Font("Segoe UI", 7f))
            {
                g.Clear(Color.FromArgb(30, 30, 30));
                g.InterpolationMode = InterpolationMode.Bilinear;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                int n = s.Size;
                for (int sum = 0; sum <= 2 * (n - 1); sum++)
                {
                    for (int x = Math.Max(0, sum - n + 1); x <= Math.Min(n - 1, sum); x++)
                    {
                        int y = sum - x;
                        int k = s.Kind[y * n + x];
                        TerrainKind kind = k >= 0 && k < kinds.Count ? kinds[k] : null;
                        PointF[] c = Corners(s, origin, x, y);
                        int zt = s.GetZ(x, y), zr = s.GetZ(x + 1, y), zb = s.GetZ(x + 1, y + 1), zl = s.GetZ(x, y + 1);
                        bool flat = zt == zr && zr == zb && zb == zl;
                        float light = o.Light ? Light(zt, zr, zb, zl) : 1f;

                        if (kind == null || (kind.Texture == null && kind.LandArt == null))
                        {
                            using (var brush = new SolidBrush(Color.FromArgb(90, 90, 90)))
                            {
                                g.FillPolygon(brush, c);
                            }
                        }
                        else if (flat && o.ArtForFlat && kind.LandArt != null)
                        {
                            g.DrawImage(kind.LandArt, new RectangleF(c[0].X - Half, c[0].Y, kind.LandArt.Width, kind.LandArt.Height));
                        }
                        else
                        {
                            DrawTexturedQuad(g, kind.Texture ?? kind.LandArt, c, light);
                        }

                        if (o.Grid)
                        {
                            g.DrawPolygon(gridPen, c);
                        }
                        if (o.Heights)
                        {
                            string text = zt.ToString();
                            SizeF ts = g.MeasureString(text, font);
                            float cx = (c[0].X + c[2].X) / 2, cy = (c[0].Y + c[2].Y) / 2;
                            g.DrawString(text, font, Brushes.Black, cx - ts.Width / 2 + 1, cy - ts.Height / 2 + 1);
                            g.DrawString(text, font, Brushes.Yellow, cx - ts.Width / 2, cy - ts.Height / 2);
                        }
                    }
                }
                if (s.Inside(o.Highlight.X, o.Highlight.Y))
                {
                    using (var pen = new Pen(Color.Yellow, 2))
                    {
                        g.DrawPolygon(pen, Corners(s, origin, o.Highlight.X, o.Highlight.Y));
                    }
                }
            }
            return bmp;
        }

        private static Bitmap RenderTop(TerrainScene s, IReadOnlyList<TerrainKind> kinds, TerrainRenderOptions o)
        {
            int cs = o.CellSize2D, n = s.Size;
            var bmp = new Bitmap(n * cs + 1, n * cs + 1, PixelFormat.Format32bppArgb);
            int zmin = s.Z.Min(), zmax = s.Z.Max();
            using (var g = Graphics.FromImage(bmp))
            using (var gridPen = new Pen(Color.FromArgb(110, 255, 255, 255)))
            using (var font = new Font("Segoe UI", 7f))
            {
                g.Clear(Color.FromArgb(30, 30, 30));
                g.InterpolationMode = InterpolationMode.Bilinear;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                for (int y = 0; y < n; y++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        var cell = new Rectangle(x * cs, y * cs, cs, cs);
                        int k = s.Kind[y * n + x];
                        TerrainKind kind = k >= 0 && k < kinds.Count ? kinds[k] : null;
                        Bitmap tex = kind?.Texture ?? kind?.LandArt;
                        int z = s.Z[y * n + x];
                        float light = o.Light && zmax > zmin ? 0.7f + 0.5f * (z - zmin) / (zmax - zmin) : 1f;
                        if (tex == null)
                        {
                            g.FillRectangle(Brushes.DimGray, cell);
                        }
                        else
                        {
                            using (ImageAttributes a = Attributes(light))
                            {
                                g.DrawImage(tex, cell, 0, 0, tex.Width, tex.Height, GraphicsUnit.Pixel, a);
                            }
                        }
                        if (o.Grid)
                        {
                            g.DrawRectangle(gridPen, cell);
                        }
                        if (o.Heights)
                        {
                            g.DrawString(z.ToString(), font, Brushes.Black, cell.X + 3, cell.Y + 3);
                            g.DrawString(z.ToString(), font, Brushes.Yellow, cell.X + 2, cell.Y + 2);
                        }
                    }
                }
                if (s.Inside(o.Highlight.X, o.Highlight.Y))
                {
                    using (var pen = new Pen(Color.Yellow, 2))
                    {
                        g.DrawRectangle(pen, o.Highlight.X * cs, o.Highlight.Y * cs, cs, cs);
                    }
                }
            }
            return bmp;
        }

        /// <summary>The cell under a point of the rendered image, or (-1, -1).</summary>
        public static Point HitTest(TerrainScene s, TerrainRenderOptions o, Point p)
        {
            if (!o.Isometric)
            {
                int x = p.X / o.CellSize2D, y = p.Y / o.CellSize2D;
                return s.Inside(x, y) ? new Point(x, y) : new Point(-1, -1);
            }
            MeasureIso(s, out Point origin);
            int n = s.Size;
            // front to back: the first hit is the visible cell
            for (int sum = 2 * (n - 1); sum >= 0; sum--)
            {
                for (int x = Math.Min(n - 1, sum); x >= Math.Max(0, sum - n + 1); x--)
                {
                    int y = sum - x;
                    if (InConvex(Corners(s, origin, x, y), p))
                    {
                        return new Point(x, y);
                    }
                }
            }
            return new Point(-1, -1);
        }

        private static bool InConvex(PointF[] poly, Point p)
        {
            bool? sign = null;
            for (int i = 0; i < poly.Length; i++)
            {
                PointF a = poly[i], b = poly[(i + 1) % poly.Length];
                float cross = (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
                if (Math.Abs(cross) < 0.001f)
                {
                    continue;
                }
                bool s = cross > 0;
                if (sign == null)
                {
                    sign = s;
                }
                else if (sign != s)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>Shows the rendered terrain zoomed and reports mouse positions in render pixels.</summary>
    internal sealed class TerrainView : Control
    {
        private Bitmap _image;
        private int _zoom = 1;

        public TerrainView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        public Bitmap Image
        {
            get => _image;
            set
            {
                _image = value;
                UpdateSize();
                Invalidate();
            }
        }

        public int Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Max(1, Math.Min(4, value));
                UpdateSize();
                Invalidate();
            }
        }

        private void UpdateSize()
        {
            Size = _image == null ? new Size(1, 1) : new Size(_image.Width * _zoom, _image.Height * _zoom);
        }

        public Point ToImage(Point p) => new Point(p.X / _zoom, p.Y / _zoom);

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_image == null)
            {
                return;
            }
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.DrawImage(_image, new Rectangle(0, 0, _image.Width * _zoom, _image.Height * _zoom));
        }
    }

    /// <summary>
    /// Preview window: shows textures as they look on the ground – isometric with heights (like the client)
    /// or from above – next to other textures or in a real map area.
    /// </summary>
    internal sealed class TerrainPreviewForm : Form
    {
        private enum TerrainTool { Paint, Raise, Lower, Flatten, Smooth, Pick }

        private readonly Func<Bitmap> _editorImage;
        private readonly List<TerrainKind> _kinds = new List<TerrainKind>();
        private TerrainScene _scene;
        private readonly TerrainRenderOptions _options = new TerrainRenderOptions();
        private readonly Panel _viewPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(30, 30, 30) };
        private readonly TerrainView _view = new TerrainView();
        private readonly ArtTileGrid _kindGrid = new ArtTileGrid { Width = 290, Height = 170, CellSize = new Size(64, 74) };
        private readonly Label _status = new Label { AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
        private readonly ToolTip _tip = new ToolTip { AutoPopDelay = 15000, InitialDelay = 500 };
        private readonly Timer _renderTimer = new Timer { Interval = 40 };
        private readonly Timer _editorTimer = new Timer { Interval = 300 };

        private RadioButton _radioIso;
        private CheckBox _chkGrid, _chkLight, _chkHeights, _chkArtFlat;
        private ComboBox _comboZoom, _comboSize, _comboMap;
        private NumericUpDown _numMapX, _numMapY, _numRadius, _numStep, _numFlatZ, _numAmplitude;
        private TextBox _txtTexId, _txtLandId;
        private TerrainTool _tool = TerrainTool.Paint;
        private int _pickedKind = -1;
        private bool _mouseDown;
        private Point _lastCell = new Point(-1, -1);

        public TerrainPreviewForm(Func<Bitmap> editorImage)
        {
            _editorImage = editorImage;
            Text = "Terrain preview – textures on the ground / Gelände-Vorschau";
            Size = new Size(1250, 820);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;
            KeyPreview = true;

            _kinds.Add(new TerrainKind { Name = "Editor image", IsEditor = true });
            RefreshEditorKind();
            NewScene(16, 0);

            _viewPanel.Controls.Add(_view);
            _viewPanel.Resize += (s, e) => CenterView();
            _view.MouseDown += View_MouseDown;
            _view.MouseMove += View_MouseMove;
            _view.MouseUp += (s, e) => { _mouseDown = false; _lastCell = new Point(-1, -1); };
            _view.MouseLeave += (s, e) => { _options.Highlight = new Point(-1, -1); ScheduleRender(); };
            _renderTimer.Tick += (s, e) => { _renderTimer.Stop(); Render(); };
            _editorTimer.Tick += (s, e) => { _editorTimer.Stop(); RefreshEditorKind(); ScheduleRender(); };

            Controls.Add(_viewPanel);
            Controls.Add(BuildSidePanel());
            Render();
        }

        /// <summary>Called by the Texture Cutter when its image changed.</summary>
        public void EditorImageChanged()
        {
            _editorTimer.Stop();
            _editorTimer.Start();
        }

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        #region UI

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 1, 0, 1) };
            row.Controls.AddRange(controls);
            return row;
        }

        private static Label Lbl(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 0) };

        private Button Btn(string text, EventHandler click, string tip = null)
        {
            var b = new Button { Text = text, AutoSize = true, Height = 26, UseVisualStyleBackColor = true, Margin = new Padding(2) };
            b.Click += click;
            if (tip != null)
            {
                _tip.SetToolTip(b, tip);
            }
            return b;
        }

        private static NumericUpDown Num(int min, int max, int value, int width = 60) =>
            new NumericUpDown { Minimum = min, Maximum = max, Value = value, Width = width, Margin = new Padding(3, 3, 3, 0) };

        private static GroupBox Group(string title, params Control[] rows)
        {
            var box = new GroupBox { Text = title, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(6, 4, 6, 6), Margin = new Padding(3, 3, 3, 6) };
            var flow = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Dock = DockStyle.Fill };
            flow.Controls.AddRange(rows);
            box.Controls.Add(flow);
            return box;
        }

        private Control BuildSidePanel()
        {
            var side = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 330, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

            _radioIso = new RadioButton { Text = "3D (isometric, heights)", AutoSize = true, Checked = true };
            var top = new RadioButton { Text = "2D (from above)", AutoSize = true };
            _radioIso.CheckedChanged += (s, e) => { _options.Isometric = _radioIso.Checked; Render(); };
            _comboZoom = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 55 };
            _comboZoom.Items.AddRange(new object[] { "1x", "2x", "3x", "4x" });
            _comboZoom.SelectedIndex = 0;
            _comboZoom.SelectedIndexChanged += (s, e) => { _view.Zoom = _comboZoom.SelectedIndex + 1; CenterView(); };
            _chkGrid = new CheckBox { Text = "Grid", AutoSize = true, Checked = true };
            _chkLight = new CheckBox { Text = "Light", AutoSize = true, Checked = true };
            _chkHeights = new CheckBox { Text = "Heights", AutoSize = true };
            _chkArtFlat = new CheckBox { Text = "Flat tiles as land art (client)", AutoSize = true, Checked = true };
            foreach (CheckBox c in new[] { _chkGrid, _chkLight, _chkHeights, _chkArtFlat })
            {
                c.CheckedChanged += (s, e) => Render();
            }
            _tip.SetToolTip(_chkArtFlat, Tip("Like the client: flat tiles show the 44 × 44 land tile, sloped tiles the stretched texture. Off = always the texture.",
                "Wie der Client: flache Kacheln zeigen die 44 × 44 Landkachel, schräge die gestreckte Textur. Aus = immer die Textur."));
            _tip.SetToolTip(_chkLight, Tip("Slopes facing the upper left are brighter, the others darker.", "Hänge nach oben links sind heller, die anderen dunkler."));
            GroupBox view = Group("View",
                Row(_radioIso, top),
                Row(Lbl("Zoom:"), _comboZoom, _chkGrid, _chkLight, _chkHeights),
                _chkArtFlat,
                Row(Btn("Save PNG...", (s, e) => SavePng()), Btn("Copy", (s, e) => { if (_view.Image != null) CutterImaging.CopyToClipboard(_view.Image); })));

            _comboSize = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
            _comboSize.Items.AddRange(new object[] { "8", "12", "16", "24", "32", "48", "64" });
            _comboSize.SelectedIndex = 2;
            _numAmplitude = Num(1, 60, 10);
            GroupBox area = Group("Area",
                Row(Lbl("Size:"), _comboSize, Btn("New (brush)", (s, e) => NewScene(GridSize, BrushKind), Tip("New flat area filled with the selected kind.", "Neue flache Fläche mit der gewählten Art."))),
                Row(Btn("Random hills", (s, e) => RandomHeights(), Tip("Smooth random heights.", "Weiche zufällige Höhen.")), Lbl("Height:"), _numAmplitude),
                Row(Btn("Flatten all", (s, e) => { PushZ(z => 0); }), Btn("Smooth all", (s, e) => SmoothAll()), Btn("Checker A/B", (s, e) => Checker(), Tip("Editor image and the picked kind as a checkerboard – shows every edge.", "Editorbild und die aufgenommene Art im Schachbrett – zeigt jede Kante."))));

            _comboMap = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
            string[] maps = { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "TerMur", "Forell", "Dragon", "Intermediate" };
            _comboMap.Items.AddRange(maps);
            _comboMap.SelectedIndex = 0;
            _numMapX = Num(0, 7000, 1400, 65);
            _numMapY = Num(0, 7000, 1600, 65);
            GroupBox map = Group("Map area (real ground)",
                Row(_comboMap, Lbl("X:"), _numMapX, Lbl("Y:"), _numMapY),
                Row(Btn("Load area", (s, e) => LoadMapArea(), Tip("Loads land tiles and heights of the map (size = area size). Then pick a ground and replace it with the editor image.",
                    "Lädt Landkacheln und Höhen der Karte (Größe = Flächengröße). Dann einen Boden aufnehmen und durch das Editorbild ersetzen.")),
                    Btn("Replace picked → brush", (s, e) => ReplacePicked(), Tip("All cells of the picked kind get the brush kind (e.g. the editor image).",
                        "Alle Felder der aufgenommenen Art bekommen die Pinsel-Art (z. B. das Editorbild)."))));

            _kindGrid.ImageProvider = i => i >= 0 && i < _kinds.Count ? _kinds[i].Thumbnail : null;
            _kindGrid.LabelProvider = i => i >= 0 && i < _kinds.Count ? _kinds[i].Name : string.Empty;
            _kindGrid.IsMarked = i => i == _pickedKind;
            _kindGrid.Text = "No kinds.";
            RefreshKindGrid(0);
            _txtTexId = new TextBox { Width = 70, Margin = new Padding(3, 4, 3, 0) };
            _txtLandId = new TextBox { Width = 70, Margin = new Padding(3, 4, 3, 0) };
            GroupBox kinds = Group("Grounds (brush = selected, red = picked)",
                _kindGrid,
                Row(Lbl("texmaps:"), _txtTexId, Btn("+", (s, e) => AddTextureKind()), Lbl("land:"), _txtLandId, Btn("+", (s, e) => AddLandKind())),
                Row(Btn("+ File...", (s, e) => AddFileKind()), Btn("Editor image ⟳", (s, e) => { RefreshEditorKind(); Render(); }), Btn("Remove", (s, e) => RemoveKind())));

            var tools = new FlowLayoutPanel { AutoSize = true, Width = 300, WrapContents = true };
            void AddTool(TerrainTool tool, string text, string en, string de)
            {
                var rb = new RadioButton { Text = text, AutoSize = true, Checked = tool == TerrainTool.Paint };
                rb.CheckedChanged += (s, e) => { if (rb.Checked) _tool = tool; };
                _tip.SetToolTip(rb, Tip(en, de));
                tools.Controls.Add(rb);
            }
            AddTool(TerrainTool.Paint, "Paint", "Paint the selected ground.", "Gewählten Boden malen.");
            AddTool(TerrainTool.Raise, "Raise", "Raise the height (right mouse button = lower).", "Höhe anheben (rechte Maustaste = absenken).");
            AddTool(TerrainTool.Lower, "Lower", "Lower the height.", "Höhe absenken.");
            AddTool(TerrainTool.Flatten, "Flatten", "Set the height to the value below.", "Höhe auf den Wert unten setzen.");
            AddTool(TerrainTool.Smooth, "Smooth", "Even out the heights.", "Höhen angleichen.");
            AddTool(TerrainTool.Pick, "Pick", "Take ground and height of a cell (also right click while painting).", "Boden und Höhe eines Feldes aufnehmen (geht auch mit Rechtsklick beim Malen).");
            _numRadius = Num(0, 8, 0, 45);
            _numStep = Num(1, 40, 3, 45);
            _numFlatZ = Num(-128, 127, 0, 55);
            GroupBox toolBox = Group("Tools", tools, Row(Lbl("Radius:"), _numRadius, Lbl("Step:"), _numStep, Lbl("Z:"), _numFlatZ));

            side.Controls.AddRange(new Control[] { view, area, map, kinds, toolBox, _status });
            return side;
        }

        #endregion

        #region Kinds

        private int GridSize => int.Parse((string)_comboSize.SelectedItem);

        private int BrushKind => Math.Max(0, _kindGrid.SelectedItem);

        private void RefreshKindGrid(int select)
        {
            _kindGrid.Items = Enumerable.Range(0, _kinds.Count).ToList();
            _kindGrid.SelectedIndex = Math.Min(select, _kinds.Count - 1);
            _kindGrid.Invalidate();
        }

        private void RefreshEditorKind()
        {
            Bitmap img = null;
            try
            {
                img = _editorImage?.Invoke();
            }
            catch (Exception)
            {
                // editor closed
            }
            _kinds[0].SetImage(img);
            _kindGrid.Invalidate();
        }

        private TerrainKind KindForLand(int landId, Dictionary<int, int> cache)
        {
            if (cache.TryGetValue(landId, out int index))
            {
                return _kinds[index];
            }
            Bitmap art = CutterArt.GetCopy(ArtKind.Land, landId);
            Bitmap tex = null;
            try
            {
                lock (SdkSync.Lock)
                {
                    int texId = TileData.LandTable[landId].TextureId;
                    if (texId > 0 && TextureMul.IsValid(texId))
                    {
                        tex = CutterImaging.To32Bpp(Textures.GetTexture(texId));
                    }
                }
            }
            catch (Exception)
            {
                // tiledata / texmaps not available
            }
            var kind = new TerrainKind { Name = $"0x{landId:X4}", LandId = landId };
            kind.SetImages(tex, art);
            _kinds.Add(kind);
            cache[landId] = _kinds.Count - 1;
            return kind;
        }

        private void AddTextureKind()
        {
            if (!CutterImaging.TryParseId(_txtTexId.Text, out int id) || TextureMul.GetCopy(id) is not Bitmap tex)
            {
                _status.Text = "Texture not found (texmaps.mul loaded?).";
                return;
            }
            var kind = new TerrainKind { Name = $"tex 0x{id:X4}" };
            kind.SetImage(tex);
            _kinds.Add(kind);
            RefreshKindGrid(_kinds.Count - 1);
        }

        private void AddLandKind()
        {
            if (!CutterImaging.TryParseId(_txtLandId.Text, out int id) || id < 0 || id > 0x3FFF)
            {
                _status.Text = "Enter a land tile ID.";
                return;
            }
            KindForLand(id, new Dictionary<int, int>());
            RefreshKindGrid(_kinds.Count - 1);
        }

        private void AddFileKind()
        {
            using (var dialog = new OpenFileDialog { Filter = CutterImaging.ImageFilter })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    var kind = new TerrainKind { Name = Path.GetFileNameWithoutExtension(dialog.FileName) };
                    kind.SetImage(CutterImaging.LoadBitmap(dialog.FileName));
                    _kinds.Add(kind);
                    RefreshKindGrid(_kinds.Count - 1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RemoveKind()
        {
            int k = _kindGrid.SelectedItem;
            if (k <= 0)
            {
                _status.Text = "The editor image cannot be removed.";
                return;
            }
            for (int i = 0; i < _scene.Kind.Length; i++)
            {
                if (_scene.Kind[i] == k)
                {
                    _scene.Kind[i] = 0;
                }
                else if (_scene.Kind[i] > k)
                {
                    _scene.Kind[i]--;
                }
            }
            _kinds[k].Dispose();
            _kinds.RemoveAt(k);
            _pickedKind = -1;
            RefreshKindGrid(k - 1);
            Render();
        }

        #endregion

        #region Scene

        private void NewScene(int size, int kind)
        {
            _scene = new TerrainScene(size);
            for (int i = 0; i < _scene.Kind.Length; i++)
            {
                _scene.Kind[i] = kind;
            }
            _options.CellSize2D = size <= 16 ? 48 : size <= 32 ? 32 : 16;
            if (_view.Image != null)
            {
                Render();
            }
        }

        private void PushZ(Func<int, int> f)
        {
            for (int i = 0; i < _scene.Z.Length; i++)
            {
                _scene.Z[i] = Math.Max(-128, Math.Min(127, f(_scene.Z[i])));
            }
            Render();
        }

        private void RandomHeights()
        {
            int n = _scene.Size, amp = (int)_numAmplitude.Value;
            var rnd = new Random();
            double p1 = rnd.NextDouble() * 6, p2 = rnd.NextDouble() * 6, f1 = 0.25 + rnd.NextDouble() * 0.3, f2 = 0.2 + rnd.NextDouble() * 0.3;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    double v = Math.Sin(x * f1 + p1) * Math.Cos(y * f2 + p2) + 0.5 * Math.Sin((x + y) * 0.4 + p2);
                    _scene.Z[y * n + x] = (int)Math.Round(v * amp / 1.5);
                }
            }
            Render();
        }

        private void SmoothAll()
        {
            int n = _scene.Size;
            var z = (int[])_scene.Z.Clone();
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    z[y * n + x] = SmoothValue(x, y);
                }
            }
            Array.Copy(z, _scene.Z, z.Length);
            Render();
        }

        private int SmoothValue(int x, int y)
        {
            int sum = 0, count = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (_scene.Inside(x + dx, y + dy))
                    {
                        sum += _scene.Z[(y + dy) * _scene.Size + x + dx];
                        count++;
                    }
                }
            }
            return (int)Math.Round((double)sum / count);
        }

        private void Checker()
        {
            int b = _pickedKind >= 0 ? _pickedKind : (_kinds.Count > 1 ? 1 : 0);
            int n = _scene.Size;
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    _scene.Kind[y * n + x] = ((x / 2) + (y / 2)) % 2 == 0 ? 0 : b;
                }
            }
            Render();
        }

        private void LoadMapArea()
        {
            Map map = Map.GetBuiltIn(_comboMap.SelectedIndex);
            if (map == null)
            {
                return;
            }
            int n = GridSize, x0 = (int)_numMapX.Value, y0 = (int)_numMapY.Value;
            var scene = new TerrainScene(n) { MapOrigin = new Point(x0, y0), MapName = (string)_comboMap.SelectedItem };
            var cache = new Dictionary<int, int>();
            for (int i = 0; i < _kinds.Count; i++)
            {
                if (_kinds[i].LandId >= 0 && !cache.ContainsKey(_kinds[i].LandId))
                {
                    cache[_kinds[i].LandId] = i;
                }
            }
            Cursor = Cursors.WaitCursor;
            try
            {
                for (int y = 0; y < n; y++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        Tile tile;
                        lock (SdkSync.Lock)
                        {
                            int mx = Math.Min(map.Width - 1, x0 + x), my = Math.Min(map.Height - 1, y0 + y);
                            tile = map.Tiles.GetLandTile(mx, my);
                        }
                        KindForLand(tile.Id, cache);
                        scene.Kind[y * n + x] = cache[tile.Id];
                        scene.Z[y * n + x] = tile.Z;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The map could not be read (is it loaded in UoFiddler?):\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            _scene = scene;
            _options.CellSize2D = n <= 16 ? 48 : n <= 32 ? 32 : 16;
            RefreshKindGrid(0);
            _status.Text = $"{scene.MapName} {x0},{y0} – {n}×{n} loaded, {cache.Count} different grounds.";
            Render();
        }

        private void ReplacePicked()
        {
            if (_pickedKind < 0)
            {
                _status.Text = "Pick a ground first (tool Pick or right click).";
                return;
            }
            int brush = BrushKind, count = 0;
            for (int i = 0; i < _scene.Kind.Length; i++)
            {
                if (_scene.Kind[i] == _pickedKind)
                {
                    _scene.Kind[i] = brush;
                    count++;
                }
            }
            _status.Text = $"{count} cells of {_kinds[_pickedKind].Name} now show {_kinds[brush].Name}.";
            Render();
        }

        #endregion

        #region Mouse

        private void View_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDown = true;
            _lastCell = new Point(-1, -1);
            ApplyTool(e);
        }

        private void View_MouseMove(object sender, MouseEventArgs e)
        {
            Point cell = TerrainRenderer.HitTest(_scene, _options, _view.ToImage(e.Location));
            if (cell != _options.Highlight)
            {
                _options.Highlight = cell;
                UpdateStatus(cell);
                if (!_mouseDown)
                {
                    ScheduleRender();
                }
            }
            if (_mouseDown && cell != _lastCell)
            {
                ApplyTool(e);
            }
        }

        private void UpdateStatus(Point cell)
        {
            if (!_scene.Inside(cell.X, cell.Y))
            {
                return;
            }
            int i = cell.Y * _scene.Size + cell.X;
            string map = _scene.MapOrigin.HasValue ? $"   map {_scene.MapOrigin.Value.X + cell.X},{_scene.MapOrigin.Value.Y + cell.Y}" : string.Empty;
            int k = _scene.Kind[i];
            _status.Text = $"cell {cell.X},{cell.Y}   z {_scene.Z[i]}   {(k < _kinds.Count ? _kinds[k].Name : "?")}{map}";
        }

        private void ApplyTool(MouseEventArgs e)
        {
            Point cell = TerrainRenderer.HitTest(_scene, _options, _view.ToImage(e.Location));
            if (!_scene.Inside(cell.X, cell.Y))
            {
                return;
            }
            _lastCell = cell;
            int n = _scene.Size;
            TerrainTool tool = _tool;
            bool right = e.Button == MouseButtons.Right;
            if (right)
            {
                tool = tool == TerrainTool.Raise ? TerrainTool.Lower : tool == TerrainTool.Lower ? TerrainTool.Raise : TerrainTool.Pick;
            }
            if (tool == TerrainTool.Pick)
            {
                int i = cell.Y * n + cell.X;
                _pickedKind = _scene.Kind[i];
                _numFlatZ.Value = Math.Max(_numFlatZ.Minimum, Math.Min(_numFlatZ.Maximum, _scene.Z[i]));
                _kindGrid.Invalidate();
                _status.Text = $"Picked {_kinds[_pickedKind].Name}, z {_scene.Z[i]}. \"Replace picked → brush\" swaps it everywhere.";
                return;
            }
            int r = (int)_numRadius.Value, step = (int)_numStep.Value;
            var smooth = new Dictionary<int, int>();
            for (int y = cell.Y - r; y <= cell.Y + r; y++)
            {
                for (int x = cell.X - r; x <= cell.X + r; x++)
                {
                    if (!_scene.Inside(x, y) || (x - cell.X) * (x - cell.X) + (y - cell.Y) * (y - cell.Y) > r * r + r)
                    {
                        continue;
                    }
                    int i = y * n + x;
                    switch (tool)
                    {
                        case TerrainTool.Paint: _scene.Kind[i] = BrushKind; break;
                        case TerrainTool.Raise: _scene.Z[i] = Math.Min(127, _scene.Z[i] + step); break;
                        case TerrainTool.Lower: _scene.Z[i] = Math.Max(-128, _scene.Z[i] - step); break;
                        case TerrainTool.Flatten: _scene.Z[i] = (int)_numFlatZ.Value; break;
                        case TerrainTool.Smooth: smooth[i] = SmoothValue(x, y); break;
                    }
                }
            }
            foreach (KeyValuePair<int, int> kv in smooth)
            {
                _scene.Z[kv.Key] = kv.Value;
            }
            ScheduleRender();
        }

        #endregion

        #region Rendering

        private void ScheduleRender()
        {
            _renderTimer.Stop();
            _renderTimer.Start();
        }

        private void Render()
        {
            _options.Grid = _chkGrid?.Checked ?? true;
            _options.Light = _chkLight?.Checked ?? true;
            _options.Heights = _chkHeights?.Checked ?? false;
            _options.ArtForFlat = _chkArtFlat?.Checked ?? true;
            Bitmap bmp;
            try
            {
                bmp = TerrainRenderer.Render(_scene, _kinds, _options);
            }
            catch (Exception ex)
            {
                _status.Text = "Render error: " + ex.Message;
                return;
            }
            Bitmap old = _view.Image;
            _view.Image = bmp;
            old?.Dispose();
            CenterView();
        }

        private void CenterView()
        {
            Size client = _viewPanel.ClientSize;
            Point scroll = _viewPanel.AutoScrollPosition;
            _view.Location = new Point(Math.Max(0, (client.Width - _view.Width) / 2) + scroll.X, Math.Max(0, (client.Height - _view.Height) / 2) + scroll.Y);
        }

        private void SavePng()
        {
            if (_view.Image == null)
            {
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = "PNG|*.png", FileName = "TerrainPreview_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _view.Image.Save(dialog.FileName, ImageFormat.Png);
                }
            }
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _renderTimer.Stop();
            _editorTimer.Stop();
            _renderTimer.Dispose();
            _editorTimer.Dispose();
            _view.Image?.Dispose();
            _kinds.ForEach(k => k.Dispose());
            _tip.Dispose();
            base.OnFormClosed(e);
        }
    }
}
