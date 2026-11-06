using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit
{
    /// <summary>Which layer the drawing tools work on.</summary>
    internal enum EditLayer
    {
        Paint,
        Original
    }

    /// <summary>
    /// One animation frame with separate layers:
    /// - Original: the imported artwork / animation frame (own size, placed at <see cref="Offset"/>)
    /// - Paint: the drawing layer (always canvas sized), drawn on top of the original
    /// The background and the reference animation are shared and live in the canvas, not in the frame.
    /// </summary>
    internal sealed class AnimFrame : IDisposable
    {
        public Bitmap Original;
        public Bitmap Paint;
        public Point Offset;

        public bool Enabled = true;
        public bool OriginalVisible = true;
        public bool PaintVisible = true;
        public float OriginalOpacity = 1f;
        public float PaintOpacity = 1f;

        /// <summary>Where the frame came from (shown in the status bar), e.g. "anim.mul Body 400 A0 D2 F3".</summary>
        public string Source = "";

        public bool HasOriginal => Original != null;

        public void Dispose()
        {
            Original?.Dispose();
            Original = null;
            Paint?.Dispose();
            Paint = null;
        }
    }

    /// <summary>Saved layer content of one frame (for undo / redo).</summary>
    internal sealed class FrameState : IDisposable
    {
        public int Index;
        public Bitmap Original;
        public Bitmap Paint;
        public Point Offset;
        public string Source;

        public void Dispose()
        {
            Original?.Dispose();
            Paint?.Dispose();
        }
    }

    /// <summary>One undo step. It can cover several frames (e.g. drawing on all checked frames).</summary>
    internal sealed class HistoryEntry : IDisposable
    {
        public string Name;
        public Size CanvasSize;
        public Point Anchor;
        public List<FrameState> States = new List<FrameState>();
        public List<Overlay> Overlays = new List<Overlay>();
        public DateTime Time = DateTime.Now;

        public override string ToString()
        {
            string frames = States.Count == 0 ? "overlay" : States.Count == 1 ? $"F{States[0].Index + 1}" : $"{States.Count} frames";
            return $"{Time:HH:mm:ss}  {Name} ({frames})";
        }

        public void Dispose()
        {
            foreach (FrameState s in States) s.Dispose();
            States.Clear();
        }
    }

    /// <summary>
    /// The edited animation: 10 frames on a common canvas plus a global undo/redo history.
    /// All frames share the canvas size and an anchor point (the "feet" position), so frames
    /// imported from UO animations line up the same way the client draws them.
    /// </summary>
    internal sealed class AnimDocument : IDisposable
    {
        public const int FrameCount = 10;
        public const int MaxHistory = 60;

        public readonly AnimFrame[] Frames = new AnimFrame[FrameCount];
        public Size CanvasSize { get; private set; }

        /// <summary>Anchor point on the canvas: imported frames are placed with their center here.</summary>
        public Point Anchor { get; set; }

        private readonly List<HistoryEntry> undo = new List<HistoryEntry>();
        private readonly List<HistoryEntry> redo = new List<HistoryEntry>();

        public event EventHandler HistoryChanged;

        /// <summary>
        /// Switchable: shows pure black, white and light gray (#D3D3D3) of the ORIGINAL layer as transparent.
        /// Nothing is changed in the image itself, so it can be switched on and off at any time.
        /// Applies to editor, thumbnails, preview and export.
        /// </summary>
        public bool KeyColorsTransparent { get; set; }

        /// <summary>Set during export: black/white are always transparent there (like in the client).</summary>
        public bool ForceKeyTransparency { get; set; }

        /// <summary>Floating images above the frames (drawn on top, in list order).</summary>
        public List<Overlay> Overlays { get; private set; } = new List<Overlay>();

        public static readonly Color[] KeyColors = { Color.Black, Color.White, Color.FromArgb(211, 211, 211) };

        public AnimDocument(int width = 220, int height = 220)
        {
            CanvasSize = new Size(width, height);
            Anchor = new Point(width / 2, height - 30);
            for (int i = 0; i < FrameCount; i++)
            {
                Frames[i] = new AnimFrame { Paint = NewLayer() };
            }
        }

        public Bitmap NewLayer() => new Bitmap(CanvasSize.Width, CanvasSize.Height, PixelFormat.Format32bppArgb);

        public IReadOnlyList<HistoryEntry> UndoList => undo;
        public IReadOnlyList<HistoryEntry> RedoList => redo;
        public bool CanUndo => undo.Count > 0;
        public bool CanRedo => redo.Count > 0;

        #region History

        private FrameState Capture(int i)
        {
            AnimFrame f = Frames[i];
            return new FrameState
            {
                Index = i,
                Original = f.Original != null ? new Bitmap(f.Original) : null,
                Paint = f.Paint != null ? new Bitmap(f.Paint) : null,
                Offset = f.Offset,
                Source = f.Source
            };
        }

        private HistoryEntry CaptureEntry(string name, IEnumerable<int> frames)
        {
            var e = new HistoryEntry { Name = name, CanvasSize = CanvasSize, Anchor = Anchor };
            e.Overlays = Overlays.Select(o => o.Clone()).ToList();
            foreach (int i in frames.Distinct().Where(i => i >= 0 && i < FrameCount).OrderBy(i => i))
                e.States.Add(Capture(i));
            return e;
        }

        /// <summary>
        /// Call BEFORE changing frames. Saves the current state of the given frames as one undo step.
        /// With coalesce = true a repeated action (e.g. arrow key nudges) within 1.5 s is merged.
        /// </summary>
        public void BeginChange(string name, IEnumerable<int> frames, bool coalesce = false)
        {
            var list = frames.Distinct().ToList();
            if (coalesce && undo.Count > 0)
            {
                HistoryEntry last = undo[undo.Count - 1];
                if (last.Name == name && (DateTime.Now - last.Time).TotalSeconds < 1.5 &&
                    last.States.Select(s => s.Index).OrderBy(x => x).SequenceEqual(list.OrderBy(x => x)))
                {
                    last.Time = DateTime.Now;
                    return;
                }
            }

            undo.Add(CaptureEntry(name, list));
            while (undo.Count > MaxHistory)
            {
                undo[0].Dispose();
                undo.RemoveAt(0);
            }
            ClearList(redo);
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        public void BeginChangeAll(string name) => BeginChange(name, Enumerable.Range(0, FrameCount));

        private void Restore(HistoryEntry e)
        {
            if (e.CanvasSize != CanvasSize)
                CanvasSize = e.CanvasSize;
            Anchor = e.Anchor;

            foreach (Overlay o in Overlays) o.Invalidate();
            Overlays = e.Overlays.Select(o => o.Clone()).ToList();

            foreach (FrameState s in e.States)
            {
                AnimFrame f = Frames[s.Index];
                f.Original?.Dispose();
                f.Paint?.Dispose();
                f.Original = s.Original != null ? new Bitmap(s.Original) : null;
                f.Paint = s.Paint != null ? new Bitmap(s.Paint) : NewLayer();
                f.Offset = s.Offset;
                f.Source = s.Source;
            }

            // Frames that were not part of the step still need a canvas sized paint layer
            foreach (AnimFrame f in Frames)
            {
                if (f.Paint == null || f.Paint.Size != CanvasSize)
                    ResizePaint(f, CanvasSize, Point.Empty);
            }
        }

        /// <summary>Undo the last step. Returns the affected frame indices (null if nothing to undo).</summary>
        public int[] Undo()
        {
            if (undo.Count == 0) return null;
            HistoryEntry e = undo[undo.Count - 1];
            undo.RemoveAt(undo.Count - 1);

            HistoryEntry current = CaptureEntry(e.Name, e.States.Select(s => s.Index));
            current.CanvasSize = CanvasSize;
            current.Anchor = Anchor;
            redo.Add(current);

            Restore(e);
            int[] affected = e.States.Select(s => s.Index).ToArray();
            e.Dispose();
            HistoryChanged?.Invoke(this, EventArgs.Empty);
            return affected;
        }

        public int[] Redo()
        {
            if (redo.Count == 0) return null;
            HistoryEntry e = redo[redo.Count - 1];
            redo.RemoveAt(redo.Count - 1);

            HistoryEntry current = CaptureEntry(e.Name, e.States.Select(s => s.Index));
            current.CanvasSize = CanvasSize;
            current.Anchor = Anchor;
            undo.Add(current);

            Restore(e);
            int[] affected = e.States.Select(s => s.Index).ToArray();
            e.Dispose();
            HistoryChanged?.Invoke(this, EventArgs.Empty);
            return affected;
        }

        public void ClearHistory()
        {
            ClearList(undo);
            ClearList(redo);
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }

        private static void ClearList(List<HistoryEntry> list)
        {
            foreach (HistoryEntry e in list) e.Dispose();
            list.Clear();
        }

        #endregion

        #region Frame operations (call BeginChange first)

        /// <summary>Puts an image into the original layer. center = UO frame center (null = bottom center).</summary>
        public void SetOriginal(int i, Bitmap bmp, Point? center, string source, bool keepPaint = true)
        {
            AnimFrame f = Frames[i];
            f.Original?.Dispose();
            f.Original = bmp != null ? ImageUtil.To32(bmp) : null;
            f.Source = source ?? "";
            if (!keepPaint) ClearPaint(i);
            if (f.Original != null)
                f.Offset = OffsetFor(f.Original.Size, center);
        }

        /// <summary>Position of an image so that its center (UO) or bottom center lies on the anchor.</summary>
        public Point OffsetFor(Size size, Point? center)
        {
            if (center.HasValue)
            {
                // UO client: x = anchorX - centerX ; y = anchorY - centerY - height
                return new Point(Anchor.X - center.Value.X, Anchor.Y - center.Value.Y - size.Height);
            }
            return new Point(Anchor.X - size.Width / 2, Anchor.Y - size.Height);
        }

        public void ClearPaint(int i)
        {
            AnimFrame f = Frames[i];
            f.Paint?.Dispose();
            f.Paint = NewLayer();
        }

        public void ClearOriginal(int i)
        {
            Frames[i].Original?.Dispose();
            Frames[i].Original = null;
            Frames[i].Source = "";
        }

        public void ClearFrame(int i)
        {
            ClearOriginal(i);
            ClearPaint(i);
            Frames[i].Offset = Point.Empty;
        }

        /// <summary>Merges the paint layer into the original layer (original grows to canvas size).</summary>
        public void MergePaintDown(int i)
        {
            AnimFrame f = Frames[i];
            Bitmap merged = NewLayer();
            using (Graphics g = Graphics.FromImage(merged))
            {
                if (f.Original != null)
                    g.DrawImage(f.Original, f.Offset.X, f.Offset.Y, f.Original.Width, f.Original.Height);
                g.DrawImage(f.Paint, 0, 0, f.Paint.Width, f.Paint.Height);
            }
            f.Original?.Dispose();
            f.Original = merged;
            f.Offset = Point.Empty;
            ClearPaint(i);
        }

        public void MirrorFrame(int i)
        {
            AnimFrame f = Frames[i];
            if (f.Original != null)
            {
                f.Original.RotateFlip(RotateFlipType.RotateNoneFlipX);
                // mirror the position around the anchor's vertical axis
                f.Offset = new Point(2 * Anchor.X - f.Offset.X - f.Original.Width, f.Offset.Y);
            }
            // The paint layer is mirrored around the anchor too, so it stays on the figure
            Bitmap p = NewLayer();
            using (Graphics g = Graphics.FromImage(p))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.TranslateTransform(2 * Anchor.X, 0);
                g.ScaleTransform(-1, 1);
                g.DrawImage(f.Paint, 0, 0, f.Paint.Width, f.Paint.Height);
            }
            f.Paint.Dispose();
            f.Paint = p;
        }

        /// <summary>Moves a layer. Original: changes the offset. Paint: shifts the pixels.</summary>
        public void Shift(int i, EditLayer layer, int dx, int dy)
        {
            AnimFrame f = Frames[i];
            if (layer == EditLayer.Original)
            {
                if (f.Original != null) f.Offset = new Point(f.Offset.X + dx, f.Offset.Y + dy);
                return;
            }
            Bitmap p = NewLayer();
            using (Graphics g = Graphics.FromImage(p))
                g.DrawImage(f.Paint, dx, dy, f.Paint.Width, f.Paint.Height);
            f.Paint.Dispose();
            f.Paint = p;
        }

        public void RotateOriginal90(int i)
        {
            AnimFrame f = Frames[i];
            if (f.Original == null) return;
            Point center = new Point(f.Offset.X + f.Original.Width / 2, f.Offset.Y + f.Original.Height / 2);
            f.Original.RotateFlip(RotateFlipType.Rotate270FlipNone);
            f.Offset = new Point(center.X - f.Original.Width / 2, center.Y - f.Original.Height / 2);
        }

        /// <summary>Scales the original layer (nearest neighbour), keeping its bottom center in place.</summary>
        public void ScaleOriginal(int i, double factor)
        {
            AnimFrame f = Frames[i];
            if (f.Original == null || factor <= 0) return;
            int w = Math.Max(1, (int)Math.Round(f.Original.Width * factor));
            int h = Math.Max(1, (int)Math.Round(f.Original.Height * factor));
            Point bottomCenter = new Point(f.Offset.X + f.Original.Width / 2, f.Offset.Y + f.Original.Height);
            Bitmap scaled = ImageUtil.ScaleNearest(f.Original, w, h);
            f.Original.Dispose();
            f.Original = scaled;
            f.Offset = new Point(bottomCenter.X - w / 2, bottomCenter.Y - h);
        }

        /// <summary>Moves the visible content so its bottom center sits on the anchor.</summary>
        public void AlignBottomToAnchor(int i)
        {
            AnimFrame f = Frames[i];
            if (f.Original == null) return;
            Rectangle b = ImageUtil.ContentBounds(f.Original);
            if (b.IsEmpty) return;
            int bottom = f.Offset.Y + b.Bottom;
            int centerX = f.Offset.X + b.Left + b.Width / 2;
            f.Offset = new Point(f.Offset.X + (Anchor.X - centerX), f.Offset.Y + (Anchor.Y - bottom));
        }

        public void CopyFrame(int from, int to)
        {
            if (from == to) return;
            AnimFrame s = Frames[from], d = Frames[to];
            d.Original?.Dispose();
            d.Paint?.Dispose();
            d.Original = s.Original != null ? new Bitmap(s.Original) : null;
            d.Paint = new Bitmap(s.Paint);
            d.Offset = s.Offset;
            d.Source = s.Source;
        }

        public void SwapFrames(int a, int b)
        {
            if (a == b) return;
            AnimFrame t = Frames[a];
            Frames[a] = Frames[b];
            Frames[b] = t;
        }

        #endregion

        #region Canvas

        private static void ResizePaint(AnimFrame f, Size size, Point shift)
        {
            var p = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            if (f.Paint != null)
            {
                using (Graphics g = Graphics.FromImage(p))
                    g.DrawImage(f.Paint, shift.X, shift.Y, f.Paint.Width, f.Paint.Height);
                f.Paint.Dispose();
            }
            f.Paint = p;
        }

        /// <summary>
        /// Changes the canvas size. Content keeps its position relative to the anchor
        /// (anchor stays horizontally centered and at the same distance from the bottom).
        /// Call BeginChangeAll first.
        /// </summary>
        public void ResizeCanvas(int width, int height)
        {
            width = Math.Max(16, Math.Min(2048, width));
            height = Math.Max(16, Math.Min(2048, height));
            int bottomGap = CanvasSize.Height - Anchor.Y;
            SetCanvas(new Size(width, height), new Point(width / 2, Math.Max(0, height - bottomGap)));
        }

        /// <summary>
        /// Sets canvas size and anchor at once; all content moves with the anchor. Call BeginChangeAll first.
        /// </summary>
        public void SetCanvas(Size size, Point newAnchor)
        {
            size = new Size(Math.Max(16, Math.Min(2048, size.Width)), Math.Max(16, Math.Min(2048, size.Height)));
            Point shift = new Point(newAnchor.X - Anchor.X, newAnchor.Y - Anchor.Y);
            CanvasSize = size;
            Anchor = newAnchor;
            foreach (AnimFrame f in Frames)
            {
                ResizePaint(f, CanvasSize, shift);
                f.Offset = new Point(f.Offset.X + shift.X, f.Offset.Y + shift.Y);
            }
            foreach (Overlay o in Overlays)
                o.Position = new Point(o.Position.X + shift.X, o.Position.Y + shift.Y);
            SetAnchor(Anchor);
        }

        /// <summary>Moves the anchor (does not move existing content).</summary>
        public void SetAnchor(Point p) => Anchor = new Point(
            Math.Max(0, Math.Min(CanvasSize.Width - 1, p.X)),
            Math.Max(0, Math.Min(CanvasSize.Height - 1, p.Y)));

        #endregion

        #region Compose

        /// <summary>Flattened frame (original + paint, respecting visibility/opacity) at canvas size.</summary>
        public Bitmap Compose(int i, bool ignoreVisibility = false)
        {
            Bitmap b = NewLayer();
            using (Graphics g = Graphics.FromImage(b))
                DrawFrame(g, i, new Rectangle(Point.Empty, CanvasSize), ignoreVisibility, 1f, null);
            return b;
        }

        /// <summary>Draws a frame into a destination rectangle that represents the whole canvas.</summary>
        public void DrawFrame(Graphics g, int i, Rectangle dest, bool ignoreVisibility, float alpha, Color? tint)
        {
            AnimFrame f = Frames[i];
            float sx = dest.Width / (float)CanvasSize.Width;
            float sy = dest.Height / (float)CanvasSize.Height;

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            if (f.Original != null && (ignoreVisibility || f.OriginalVisible))
            {
                var r = new Rectangle(
                    dest.X + (int)Math.Round(f.Offset.X * sx), dest.Y + (int)Math.Round(f.Offset.Y * sy),
                    (int)Math.Round(f.Original.Width * sx), (int)Math.Round(f.Original.Height * sy));
                float a = (ignoreVisibility ? 1f : f.OriginalOpacity) * alpha;
                if (KeyColorsTransparent || ForceKeyTransparency)
                {
                    using (Bitmap keyed = ImageUtil.MakeTransparent(f.Original, KeyColors))
                        ImageUtil.DrawImage(g, keyed, r, a, tint);
                }
                else
                {
                    ImageUtil.DrawImage(g, f.Original, r, a, tint);
                }
            }
            if (f.Paint != null && (ignoreVisibility || f.PaintVisible))
                ImageUtil.DrawImage(g, f.Paint, dest, (ignoreVisibility ? 1f : f.PaintOpacity) * alpha, tint);

            // floating overlays on top
            foreach (Overlay o in Overlays)
            {
                if (!o.AppliesTo(i)) continue;
                Bitmap img = o.RenderedFor(i);
                var r = new Rectangle(
                    dest.X + (int)Math.Round(o.Position.X * sx), dest.Y + (int)Math.Round(o.Position.Y * sy),
                    (int)Math.Round(img.Width * sx), (int)Math.Round(img.Height * sy));
                if (KeyColorsTransparent || ForceKeyTransparency)
                {
                    using (Bitmap keyed = ImageUtil.MakeTransparent(img, KeyColors))
                        ImageUtil.DrawImage(g, keyed, r, o.Opacity * alpha, tint);
                }
                else
                {
                    ImageUtil.DrawImage(g, img, r, o.Opacity * alpha, tint);
                }
            }
        }

        /// <summary>
        /// Burns an overlay into the paint layer of a frame. Black/white/light gray are always left out
        /// (they are transparent in UO and must not cover the frame). Call BeginChange first.
        /// </summary>
        public void BakeOverlay(Overlay o, int i)
        {
            if (!o.AppliesTo(i)) return;
            using (Bitmap keyed = ImageUtil.MakeTransparent(o.RenderedFor(i), KeyColors))
            using (Graphics g = Graphics.FromImage(Frames[i].Paint))
            {
                var r = new Rectangle(o.Position, keyed.Size);
                ImageUtil.DrawImage(g, keyed, r, o.Opacity);
            }
        }

        /// <summary>Flattened frame cropped to the visible content (for export). Returns null if empty.</summary>
        public Bitmap ComposeCropped(int i, out Point centerInCrop)
        {
            centerInCrop = Point.Empty;
            using (Bitmap full = Compose(i, true))
            {
                Rectangle b = ImageUtil.ContentBounds(full);
                if (b.IsEmpty) return null;
                centerInCrop = new Point(Anchor.X - b.Left, Anchor.Y - b.Top);
                return full.Clone(b, PixelFormat.Format32bppArgb);
            }
        }

        public bool IsEmpty(int i)
        {
            AnimFrame f = Frames[i];
            if (f.Original != null) return false;
            if (Overlays.Any(o => o.AppliesTo(i))) return false;
            return ImageUtil.ContentBounds(f.Paint).IsEmpty;
        }

        #endregion

        public void Dispose()
        {
            foreach (AnimFrame f in Frames) f?.Dispose();
            foreach (Overlay o in Overlays) o.Invalidate();
            ClearList(undo);
            ClearList(redo);
        }
    }
}
