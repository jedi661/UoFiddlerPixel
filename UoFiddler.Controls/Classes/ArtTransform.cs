using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Geometry of an Ultima Online field (land tile) and the anchor of a static / art graphic in it.
    ///  - A tile is a 44 x 44 diamond: top (22,0), right (44,22), bottom (22,44), left (0,22), centre (22,22);
    ///    its edges run at 45 degrees (world -> screen: sx = (x - y) * 22, sy = (x + y) * 22 - z * 4).
    ///  - A graphic is placed with its WHOLE frame (not only the coloured pixels):
    ///        drawX = tx + 22 - floor(w / 2)      drawY = ty + 44 - h
    ///    i.e. the middle pixel of the lowest pixel row sits on the bottom corner of the tile: the fix point.
    /// In image coordinates of a w x h graphic the main tile therefore starts at (floor(w/2) - 22, h - 44).
    /// </summary>
    public static class UoTile
    {
        public const int Size = 44;
        public const int Half = 22;
        public const int ZStep = 4;

        /// <summary>Left top corner (tx, ty) of the 44x44 box of the main tile in image coordinates.</summary>
        public static (int X, int Y) TileOrigin(int imageWidth, int imageHeight) => ((imageWidth / 2) - Half, imageHeight - Size);

        /// <summary>Origin of the neighbour tile (i, j) (i = +x / south-east, j = +y / south-west).</summary>
        public static (int X, int Y) NeighbourOrigin(int baseX, int baseY, int i, int j) => (baseX + ((i - j) * Half), baseY + ((i + j) * Half));

        /// <summary>The four corners (top, right, bottom, left) of the diamond of a tile box at (tx, ty).</summary>
        public static (int X, int Y)[] Diamond(int tx, int ty) => new[]
        {
            (tx + Half, ty), (tx + Size, ty + Half), (tx + Half, ty + Size), (tx, ty + Half)
        };

        /// <summary>The anchor = fix point of a graphic: middle pixel of the lowest row (floor(w/2), h-1).</summary>
        public static (int X, int Y) Anchor(int imageWidth, int imageHeight) => (imageWidth / 2, imageHeight - 1);

        /// <summary>Where a graphic of size w x h is drawn for the tile box (tx, ty) (+ an own offset).</summary>
        public static (int X, int Y) DrawPosition(int tx, int ty, int w, int h, int offX = 0, int offY = 0) =>
            (tx + Half - (w / 2) + offX, ty + Size - h + offY);
    }

    public enum TransformType
    {
        FlipH,
        FlipV,
        Rot90CW,
        Rot90CCW,
        Rot180,
        Rotate,   // A = degrees (around the fix point)
        SkewX,    // A = horizontal shift per pixel of height (the ground row stays)
        SkewY,    // A = vertical shift per pixel of width (the fix point column stays)
        Scale,    // A = x factor, B = y factor (around the fix point)
        Move,     // A = dx, B = dy
        Distort,  // P[8] = offsets of the frame corners TL, TR, BL, BR (dx, dy each)
        Warp,     // P[18] = offsets of a 3x3 point grid over the frame (row by row, dx, dy each)
        Mesh      // A = spacing of the grid dots, P = offsets of every grid dot (see MeshLattice)
    }

    /// <summary>One step of a transformation (stored as JSON, so a transformation can be remembered).</summary>
    public sealed class TransformStep
    {
        public TransformType Type { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double[] P { get; set; }

        public TransformStep Clone() => new TransformStep { Type = Type, A = A, B = B, P = P == null ? null : (double[])P.Clone() };

        public static TransformStep NewDistort() => new TransformStep { Type = TransformType.Distort, P = new double[8] };

        public static TransformStep NewWarp() => new TransformStep { Type = TransformType.Warp, P = new double[18] };

        public static TransformStep NewMesh(int canvasWidth, int canvasHeight, int spacing)
        {
            (double[] xs, double[] ys) = ArtTransform.MeshLattice(canvasWidth, canvasHeight, spacing);
            return new TransformStep { Type = TransformType.Mesh, A = spacing, P = new double[xs.Length * ys.Length * 2] };
        }

        public override string ToString() => Type switch
        {
            TransformType.FlipH => "Flip \u2194",
            TransformType.FlipV => "Flip \u2195",
            TransformType.Rot90CW => "Rotate 90\u00b0 \u21bb",
            TransformType.Rot90CCW => "Rotate 90\u00b0 \u21ba",
            TransformType.Rot180 => "Rotate 180\u00b0",
            TransformType.Rotate => $"Rotate {A:0.#}\u00b0",
            TransformType.SkewX => $"Skew X {A:0.00}",
            TransformType.SkewY => $"Skew Y {A:0.00}",
            TransformType.Scale => $"Scale {A * 100:0}% x {B * 100:0}%",
            TransformType.Move => $"Move {A:+0;-0;0} / {B:+0;-0;0}",
            TransformType.Distort => "Distort (4 corners)",
            TransformType.Warp => "Warp (3x3 points)",
            TransformType.Mesh => $"Grid dots ({A:0} px)",
            _ => Type.ToString()
        };
    }

    /// <summary>A complete transformation: canvas (frame), steps and the position in the UO field.</summary>
    public sealed class TransformSettings
    {
        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }

        /// <summary>
        /// Optional: move the content so its lowest coloured row stays where it was. Off by default - the
        /// frame (fix point = middle pixel of the lowest row) defines the position in the UO field.
        /// </summary>
        public bool KeepBottom { get; set; }

        /// <summary>Own offset of the graphic in the field (pixels), applied last: drawX += OffsetX, drawY += OffsetY.</summary>
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public List<TransformStep> Steps { get; set; } = new List<TransformStep>();

        public TransformSettings Clone() => new TransformSettings
        {
            CanvasWidth = CanvasWidth,
            CanvasHeight = CanvasHeight,
            KeepBottom = KeepBottom,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            Steps = Steps.Select(s => s.Clone()).ToList()
        };
    }

    /// <summary>
    /// Pixel exact transformations for UO art (Photoshop "skew / distort / warp" without blur):
    /// every target pixel takes exactly ONE source pixel (nearest neighbour by inverse mapping), so the colours
    /// stay identical, nothing gets blended or faded. Flips and 90 degree turns are exact permutations.
    /// Everything works on the WHOLE frame (canvas), never only on the coloured pixels: pivots are the fix point
    /// (middle pixel of the lowest row, see <see cref="UoTile"/>), the ground row or the frame itself.
    /// The steps are applied one after another to the original every time (non-destructive), so a later flip
    /// never resets an earlier skew.
    /// </summary>
    public static class ArtTransform
    {
        /// <summary>Applies all steps to <paramref name="source"/> and returns the result on the canvas.</summary>
        public static ArtPixelImage Apply(ArtPixelImage source, TransformSettings settings, int stepCount = int.MaxValue)
        {
            ArtPixelImage img = PlaceOnCanvas(source, settings);
            int n = Math.Min(stepCount, settings.Steps.Count);
            for (int i = 0; i < n; ++i)
            {
                img = ApplyStep(img, settings.Steps[i]);
            }

            if (settings.KeepBottom)
            {
                GetBounds(source, out _, out _, out _, out int srcMaxY);
                int targetBottom = srcMaxY + (CanvasHeight(source, settings) - source.Height);
                GetBounds(img, out _, out _, out int maxX, out int maxY);
                if (maxX >= 0)
                {
                    img = Translate(img, 0, targetBottom - maxY);
                }
            }

            return Translate(img, settings.OffsetX, settings.OffsetY);
        }

        /// <summary>The image as it is before step <paramref name="index"/> (for the edit points).</summary>
        public static ArtPixelImage ApplyBefore(ArtPixelImage source, TransformSettings settings, int index)
        {
            ArtPixelImage img = PlaceOnCanvas(source, settings);
            for (int i = 0; i < index && i < settings.Steps.Count; ++i)
            {
                img = ApplyStep(img, settings.Steps[i]);
            }

            return img;
        }

        private static int CanvasWidth(ArtPixelImage src, TransformSettings s) => s.CanvasWidth > 0 ? s.CanvasWidth : src.Width;

        private static int CanvasHeight(ArtPixelImage src, TransformSettings s) => s.CanvasHeight > 0 ? s.CanvasHeight : src.Height;

        /// <summary>
        /// Source on the canvas so that both fix points are the same pixel: x offset floor(W/2) - floor(w/2),
        /// lowest rows on each other. With the canvas = source size this is the source itself.
        /// </summary>
        public static ArtPixelImage PlaceOnCanvas(ArtPixelImage source, TransformSettings settings)
        {
            int w = CanvasWidth(source, settings), h = CanvasHeight(source, settings);
            var img = new ArtPixelImage(w, h);
            int ox = (w / 2) - (source.Width / 2), oy = h - source.Height;
            for (int y = 0; y < source.Height; ++y)
            {
                for (int x = 0; x < source.Width; ++x)
                {
                    int nx = x + ox, ny = y + oy;
                    if (nx >= 0 && ny >= 0 && nx < w && ny < h)
                    {
                        img[nx, ny] = source[x, y];
                    }
                }
            }

            return img;
        }

        public static ArtPixelImage ApplyStep(ArtPixelImage img, TransformStep step)
        {
            int w = img.Width, h = img.Height;

            // fix point: the middle pixel of the lowest row; as continuous point its centre column on the ground
            double fx = (w / 2) + 0.5, fy = h;

            // 90 degree turns around a fixed pixel corner in the middle of the frame: exact permutations,
            // four turns give back the original
            int px = w / 2, py = h / 2;

            switch (step.Type)
            {
                case TransformType.Move:
                    return Translate(img, (int)Math.Round(step.A), (int)Math.Round(step.B));
                case TransformType.FlipH:
                    // mirror of the whole frame (like mirroring the bitmap): column c -> w-1-c
                    return Resample(img, (x, y) => (w - x, y));
                case TransformType.FlipV:
                    return Resample(img, (x, y) => (x, h - y));
                case TransformType.Rot180:
                    return Resample(img, (x, y) => (w - x, h - y));
                case TransformType.Rot90CW:
                    // forward: x' = px - (y - py), y' = py + (x - px)
                    return Resample(img, (x, y) => (px + (y - py), py - (x - px)));
                case TransformType.Rot90CCW:
                    return Resample(img, (x, y) => (px - (y - py), py + (x - px)));
                case TransformType.Rotate:
                {
                    double a = -step.A * Math.PI / 180.0, cos = Math.Cos(a), sin = Math.Sin(a);
                    return Resample(img, (x, y) => (fx + ((x - fx) * cos) - ((y - fy) * sin), fy + ((x - fx) * sin) + ((y - fy) * cos)));
                }

                case TransformType.SkewX:
                    // the ground row stays, the top leans by A pixels per pixel of height
                    return Resample(img, (x, y) => (x - (step.A * (fy - y)), y));
                case TransformType.SkewY:
                    return Resample(img, (x, y) => (x, y - (step.A * (x - fx))));
                case TransformType.Scale:
                {
                    double sx = Math.Abs(step.A) < 0.01 ? 1 : step.A, sy = Math.Abs(step.B) < 0.01 ? 1 : step.B;
                    return Resample(img, (x, y) => (fx + ((x - fx) / sx), fy + ((y - fy) / sy)));
                }

                case TransformType.Distort:
                    return Distort(img, step.P ?? new double[8]);
                case TransformType.Warp:
                    return Warp(img, step.P ?? new double[18]);
                case TransformType.Mesh:
                    return Mesh(img, (int)Math.Max(2, step.A), step.P ?? Array.Empty<double>());
                default:
                    return img;
            }
        }

        /// <summary>
        /// Inverse mapping: for every target pixel centre the source position is computed and the pixel there is
        /// taken as it is (no interpolation).
        /// </summary>
        private static ArtPixelImage Resample(ArtPixelImage src, Func<double, double, (double X, double Y)> inverse)
        {
            var dst = new ArtPixelImage(src.Width, src.Height);
            for (int y = 0; y < src.Height; ++y)
            {
                for (int x = 0; x < src.Width; ++x)
                {
                    (double sx, double sy) = inverse(x + 0.5, y + 0.5);
                    int ix = (int)Math.Floor(sx), iy = (int)Math.Floor(sy);
                    if (ix >= 0 && iy >= 0 && ix < src.Width && iy < src.Height)
                    {
                        dst[x, y] = src[ix, iy];
                    }
                }
            }

            return dst;
        }

        public static ArtPixelImage Translate(ArtPixelImage src, int dx, int dy)
        {
            return ArtDirectionGenerator.Translate(src, dx, dy);
        }

        #region Distort / warp / mesh (all on the whole frame)
        /// <summary>Corner positions (TL, TR, BL, BR) of a distort step: the frame corners + offsets.</summary>
        public static (double X, double Y)[] DistortHandles(int w, int h, double[] p) => new[]
        {
            (0 + p[0], 0 + p[1]), (w + p[2], 0 + p[3]),
            (0 + p[4], h + p[5]), (w + p[6], h + p[7])
        };

        /// <summary>Positions of the 3x3 warp points (frame grid + offsets).</summary>
        public static (double X, double Y)[] WarpHandles(int w, int h, double[] p)
        {
            var r = new (double, double)[9];
            for (int j = 0; j < 3; ++j)
            {
                for (int i = 0; i < 3; ++i)
                {
                    int k = (j * 3) + i;
                    r[k] = ((w * i / 2.0) + p[k * 2], (h * j / 2.0) + p[(k * 2) + 1]);
                }
            }

            return r;
        }

        /// <summary>
        /// The grid dots of a mesh: every <paramref name="spacing"/> pixels, anchored at the fix point (column
        /// floor(w/2), ground row h) - the same dots the grid overlay shows. Includes the frame borders.
        /// </summary>
        public static (double[] Xs, double[] Ys) MeshLattice(int w, int h, int spacing)
        {
            spacing = Math.Max(2, spacing);
            var xs = new SortedSet<double> { 0, w };
            for (int x = (w / 2) % spacing; x <= w; x += spacing)
            {
                xs.Add(x);
            }

            var ys = new SortedSet<double> { 0, h };
            for (int y = h; y >= 0; y -= spacing)
            {
                ys.Add(y);
            }

            return (xs.ToArray(), ys.ToArray());
        }

        public static (double X, double Y)[] MeshHandles(int w, int h, int spacing, double[] p)
        {
            (double[] xs, double[] ys) = MeshLattice(w, h, spacing);
            var r = new (double, double)[xs.Length * ys.Length];
            for (int j = 0; j < ys.Length; ++j)
            {
                for (int i = 0; i < xs.Length; ++i)
                {
                    int k = (j * xs.Length) + i;
                    double ox = (k * 2) + 1 < p.Length ? p[k * 2] : 0, oy = (k * 2) + 1 < p.Length ? p[(k * 2) + 1] : 0;
                    r[k] = (xs[i] + ox, ys[j] + oy);
                }
            }

            return r;
        }

        /// <summary>The frame becomes the quad of the moved corners (bilinear, inverse by Newton iteration).</summary>
        private static ArtPixelImage Distort(ArtPixelImage img, double[] p)
        {
            double x1 = img.Width, y1 = img.Height;
            double ax = p[0], ay = p[1];             // TL
            double bx = x1 + p[2], by = p[3];        // TR
            double cx = p[4], cy = y1 + p[5];        // BL
            double dx = x1 + p[6], dy = y1 + p[7];   // BR

            return Resample(img, (px, py) =>
            {
                double s = 0.5, t = 0.5;
                for (int it = 0; it < 12; ++it)
                {
                    double fx = ((1 - s) * (1 - t) * ax) + (s * (1 - t) * bx) + ((1 - s) * t * cx) + (s * t * dx) - px;
                    double fy = ((1 - s) * (1 - t) * ay) + (s * (1 - t) * by) + ((1 - s) * t * cy) + (s * t * dy) - py;
                    double dxs = ((1 - t) * (bx - ax)) + (t * (dx - cx)), dys = ((1 - t) * (by - ay)) + (t * (dy - cy));
                    double dxt = ((1 - s) * (cx - ax)) + (s * (dx - bx)), dyt = ((1 - s) * (cy - ay)) + (s * (dy - by));
                    double det = (dxs * dyt) - (dxt * dys);
                    if (Math.Abs(det) < 1e-9)
                    {
                        break;
                    }

                    s -= ((fx * dyt) - (fy * dxt)) / det;
                    t -= ((dxs * fy) - (dys * fx)) / det;
                }

                if (s < -0.0001 || s > 1.0001 || t < -0.0001 || t > 1.0001 || double.IsNaN(s) || double.IsNaN(t))
                {
                    return (-1, -1);
                }

                return (s * x1, t * y1);
            });
        }

        private static ArtPixelImage Warp(ArtPixelImage img, double[] p)
        {
            double[] xs = { 0, img.Width / 2.0, img.Width }, ys = { 0, img.Height / 2.0, img.Height };
            return LatticeWarp(img, xs, ys, p);
        }

        private static ArtPixelImage Mesh(ArtPixelImage img, int spacing, double[] p)
        {
            (double[] xs, double[] ys) = MeshLattice(img.Width, img.Height, spacing);
            return LatticeWarp(img, xs, ys, p);
        }

        /// <summary>
        /// Moves every point of the frame by the bilinear mix of the offsets of the surrounding lattice points
        /// (piecewise per cell). Inverse by fixed point iteration.
        /// </summary>
        private static ArtPixelImage LatticeWarp(ArtPixelImage img, double[] xs, double[] ys, double[] p)
        {
            int nx = xs.Length;
            double Off(int i, int j, int c)
            {
                int k = (((j * nx) + i) * 2) + c;
                return k < p.Length ? p[k] : 0;
            }

            int Cell(double[] a, double v)
            {
                int i = Array.BinarySearch(a, v);
                if (i < 0)
                {
                    i = ~i - 1;
                }

                return Math.Clamp(i, 0, a.Length - 2);
            }

            (double X, double Y) Offset(double x, double y)
            {
                int i = Cell(xs, x), j = Cell(ys, y);
                double fu = Math.Clamp((x - xs[i]) / Math.Max(1e-9, xs[i + 1] - xs[i]), 0, 1);
                double fv = Math.Clamp((y - ys[j]) / Math.Max(1e-9, ys[j + 1] - ys[j]), 0, 1);
                double ox = 0, oy = 0;
                for (int b = 0; b <= 1; ++b)
                {
                    for (int a = 0; a <= 1; ++a)
                    {
                        double wgt = (a == 0 ? 1 - fu : fu) * (b == 0 ? 1 - fv : fv);
                        ox += wgt * Off(i + a, j + b, 0);
                        oy += wgt * Off(i + a, j + b, 1);
                    }
                }

                return (ox, oy);
            }

            if (p.All(v => v == 0))
            {
                return img;
            }

            return Resample(img, (px, py) =>
            {
                double sx = px, sy = py;
                for (int it = 0; it < 12; ++it)
                {
                    (double ox, double oy) = Offset(sx, sy);
                    sx = px - ox;
                    sy = py - oy;
                }

                return (sx, sy);
            });
        }
        #endregion

        public static void GetBounds(ArtPixelImage img, out int minX, out int minY, out int maxX, out int maxY)
        {
            minX = img.Width;
            minY = img.Height;
            maxX = -1;
            maxY = -1;
            for (int y = 0; y < img.Height; ++y)
            {
                for (int x = 0; x < img.Width; ++x)
                {
                    if (img.IsOpaque(x, y))
                    {
                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minY = Math.Min(minY, y);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }
        }
    }

    #region Memory: remembered transformations
    public sealed class TransformMemoryEntry
    {
        public string Name { get; set; } = string.Empty;
        public int SourceId { get; set; } = -1;
        public DateTime Saved { get; set; }
        public TransformSettings Settings { get; set; } = new TransformSettings();

        // look of the source (to find it again when it is loaded from a file or the clipboard)
        public int Width { get; set; }
        public int Height { get; set; }
        public int Count { get; set; }
        public byte[] Mask { get; set; }
        public byte[] Luma { get; set; }
        public byte[] Hist { get; set; }

        public ArtFeatures ToFeatures() => new ArtFeatures
        {
            Width = Width,
            Height = Height,
            Count = Count,
            Mask = Mask ?? new byte[64],
            Luma = Luma ?? new byte[64],
            Hist = Hist ?? new byte[27]
        };

        public void SetFeatures(ArtFeatures f)
        {
            Width = f.Width;
            Height = f.Height;
            Count = f.Count;
            Mask = f.Mask;
            Luma = f.Luma;
            Hist = f.Hist;
        }
    }

    /// <summary>
    /// Remembers the transformation of an image (JSON). Loading the same image again (same item id, or the same
    /// look from a file/clipboard) gives the transformation back.
    /// </summary>
    public sealed class TransformMemory
    {
        /// <summary>Only (nearly) the same image matches - a transformation belongs to one picture.</summary>
        public const double MatchDistance = 0.08;

        private readonly string _path;
        public List<TransformMemoryEntry> Entries { get; private set; } = new List<TransformMemoryEntry>();

        public TransformMemory(string path)
        {
            _path = path;
            try
            {
                if (path != null && File.Exists(path))
                {
                    Entries = JsonSerializer.Deserialize<List<TransformMemoryEntry>>(File.ReadAllText(path)) ?? new List<TransformMemoryEntry>();
                }
            }
            catch (Exception)
            {
                Entries = new List<TransformMemoryEntry>();
            }
        }

        public TransformMemoryEntry Find(ArtFeatures f, int sourceId, out double distance)
        {
            distance = double.MaxValue;

            // the entry of exactly this item id wins (its image may have changed in the meantime)
            if (sourceId >= 0)
            {
                TransformMemoryEntry byId = Entries.FirstOrDefault(e => e.SourceId == sourceId);
                if (byId != null)
                {
                    distance = 0;
                    return byId;
                }
            }

            TransformMemoryEntry best = null;
            foreach (TransformMemoryEntry e in Entries)
            {
                // an entry for another item id only belongs to that item
                double d = e.SourceId >= 0 && sourceId >= 0 ? double.MaxValue : ArtFeatures.Distance(f, e.ToFeatures());
                if (d < distance)
                {
                    distance = d;
                    best = e;
                }
            }

            return distance <= MatchDistance ? best : null;
        }

        public void Remember(string name, int sourceId, ArtFeatures features, TransformSettings settings)
        {
            Entries.RemoveAll(e => (sourceId >= 0 && e.SourceId == sourceId) ||
                                   (sourceId < 0 && e.SourceId < 0 && ArtFeatures.Distance(features, e.ToFeatures()) < 0.02));
            var entry = new TransformMemoryEntry { Name = name, SourceId = sourceId, Saved = DateTime.Now, Settings = settings.Clone() };
            entry.SetFeatures(features);
            Entries.Add(entry);
            Save();
        }

        public void Forget(int sourceId, ArtFeatures features)
        {
            Entries.RemoveAll(e => (sourceId >= 0 && e.SourceId == sourceId) ||
                                   (sourceId < 0 && ArtFeatures.Distance(features, e.ToFeatures()) <= MatchDistance));
            Save();
        }

        private void Save()
        {
            if (_path == null)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            File.WriteAllText(_path, JsonSerializer.Serialize(Entries, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
    #endregion
}
