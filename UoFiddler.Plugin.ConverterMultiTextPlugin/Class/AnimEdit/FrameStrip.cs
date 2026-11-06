using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit
{
    /// <summary>
    /// Shows all 10 frames as thumbnails. Click = select frame, click on the small box = include
    /// the frame in playback / batch actions ("checked"), files can be dropped onto a frame.
    /// </summary>
    internal sealed class FrameStrip : Control
    {
        private const int CheckSize = 14;
        private readonly Bitmap[] thumbs = new Bitmap[AnimDocument.FrameCount];
        private AnimDocument document;
        private int selected;
        private int playing = -1;
        private int hover = -1;

        public event Action<int> FrameSelected;
        public event Action<int> FrameCheckedChanged;
        public event Action<int, string[]> FilesDropped;
        public event Action<int> FrameDoubleClicked;

        /// <summary>Background used behind the thumbnails (null = checkerboard).</summary>
        public Func<int, Bitmap> ThumbnailRenderer { get; set; }

        public FrameStrip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            AllowDrop = true;
            Height = 118;
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.White;
        }

        public AnimDocument Document
        {
            get => document;
            set { document = value; RefreshAll(); }
        }

        public int Selected
        {
            get => selected;
            set { selected = value; Invalidate(); }
        }

        /// <summary>Frame that is currently shown by the playback (-1 = none).</summary>
        public int Playing
        {
            get => playing;
            set { if (playing != value) { playing = value; Invalidate(); } }
        }

        private Rectangle CellRect(int i)
        {
            int n = AnimDocument.FrameCount;
            int w = Math.Max(40, (Width - 4) / n);
            return new Rectangle(2 + i * w, 2, w - 4, Height - 4);
        }

        private static Rectangle CheckRect(Rectangle cell) => new Rectangle(cell.X + 3, cell.Y + 3, CheckSize, CheckSize);

        private int HitTest(Point p)
        {
            for (int i = 0; i < AnimDocument.FrameCount; i++)
                if (CellRect(i).Contains(p)) return i;
            return -1;
        }

        public void RefreshFrame(int i)
        {
            if (document == null || i < 0 || i >= AnimDocument.FrameCount) return;
            thumbs[i]?.Dispose();
            thumbs[i] = ThumbnailRenderer != null ? ThumbnailRenderer(i) : document.Compose(i);
            Invalidate(CellRect(i));
        }

        public void RefreshAll()
        {
            for (int i = 0; i < AnimDocument.FrameCount; i++) RefreshFrame(i);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (document == null) return;

            for (int i = 0; i < AnimDocument.FrameCount; i++)
            {
                Rectangle cell = CellRect(i);
                AnimFrame f = document.Frames[i];

                Rectangle img = new Rectangle(cell.X + 2, cell.Y + 20, cell.Width - 4, cell.Height - 24);
                ImageUtil.DrawChecker(g, img, 6);

                Bitmap t = thumbs[i];
                if (t != null)
                {
                    float s = Math.Min(img.Width / (float)t.Width, img.Height / (float)t.Height);
                    int w = (int)(t.Width * s), h = (int)(t.Height * s);
                    g.InterpolationMode = s >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(t, img.X + (img.Width - w) / 2, img.Y + (img.Height - h) / 2, w, h);
                }

                if (!f.Enabled)
                {
                    using (var veil = new SolidBrush(Color.FromArgb(140, 30, 30, 30)))
                        g.FillRectangle(veil, img);
                }

                Color border = i == selected ? Color.Orange : i == hover ? Color.Silver : Color.FromArgb(90, 90, 90);
                using (var pen = new Pen(border, i == selected ? 3 : 1))
                    g.DrawRectangle(pen, cell);
                if (i == playing)
                {
                    using (var pen = new Pen(Color.LimeGreen, 2))
                        g.DrawRectangle(pen, Rectangle.Inflate(cell, -3, -3));
                }

                Rectangle cr = CheckRect(cell);
                ControlPaint.DrawCheckBox(g, cr, f.Enabled ? ButtonState.Checked | ButtonState.Flat : ButtonState.Flat);

                using (var font = new Font(Font.FontFamily, 9f, FontStyle.Bold))
                using (var brush = new SolidBrush(ForeColor))
                    g.DrawString((i + 1).ToString(), font, brush, cell.X + CheckSize + 6, cell.Y + 2);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int i = HitTest(e.Location);
            if (i < 0 || document == null) return;

            if (e.Button == MouseButtons.Left && CheckRect(CellRect(i)).Contains(e.Location))
            {
                document.Frames[i].Enabled = !document.Frames[i].Enabled;
                Invalidate();
                FrameCheckedChanged?.Invoke(i);
                return;
            }

            selected = i;
            Invalidate();
            FrameSelected?.Invoke(i);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int i = HitTest(e.Location);
            if (i >= 0 && !CheckRect(CellRect(i)).Contains(e.Location))
                FrameDoubleClicked?.Invoke(i);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = HitTest(e.Location);
            if (i != hover) { hover = i; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hover = -1;
            Invalidate();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            int i = HitTest(PointToClient(new Point(e.X, e.Y)));
            if (i != hover) { hover = i; Invalidate(); }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            int i = HitTest(PointToClient(new Point(e.X, e.Y)));
            if (i >= 0 && e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                FilesDropped?.Invoke(i, files);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 0; i < thumbs.Length; i++)
                {
                    thumbs[i]?.Dispose();
                    thumbs[i] = null;
                }
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>Simple pixel exact image view with zoom, background and optional mirrored copy.</summary>
    internal sealed class PreviewBox : Control
    {
        private Image image;

        public PreviewBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(30, 30, 30);
        }

        /// <summary>The shown image. The caller keeps ownership.</summary>
        public Image Image
        {
            get => image;
            set { image = value; Invalidate(); }
        }

        public int Zoom { get; set; } = 1;
        public bool ShowMirrored { get; set; }
        public bool Checker { get; set; } = true;
        public Image BackgroundPicture { get; set; }
        public string Caption { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);

            if (BackgroundPicture != null)
            {
                using (var tb = new TextureBrush(BackgroundPicture, WrapMode.Tile))
                    g.FillRectangle(tb, ClientRectangle);
            }
            else if (Checker)
            {
                ImageUtil.DrawChecker(g, ClientRectangle, 8);
            }

            if (image != null)
            {
                int z = Math.Max(1, Zoom);
                int w = image.Width * z, h = image.Height * z;
                int count = ShowMirrored ? 2 : 1;
                int totalW = w * count + (count - 1) * 8;
                int x = (Width - totalW) / 2, y = (Height - h) / 2;

                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(image, new Rectangle(x, y, w, h), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                if (ShowMirrored)
                {
                    var state = g.Save();
                    g.TranslateTransform(x + totalW, y);
                    g.ScaleTransform(-1, 1);
                    g.DrawImage(image, new Rectangle(0, 0, w, h), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                    g.Restore(state);
                }
            }

            if (!string.IsNullOrEmpty(Caption))
            {
                using (var b = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                    g.FillRectangle(b, 0, Height - 18, Width, 18);
                TextRenderer.DrawText(g, Caption, Font, new Point(4, Height - 16), Color.White);
            }
        }
    }
}
