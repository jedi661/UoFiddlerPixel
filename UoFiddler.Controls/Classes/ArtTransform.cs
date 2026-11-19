using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UoFiddler.Controls.Classes
{
    public enum TransformType
    {
        FlipH,
        FlipV,
        Rot90CW,
        Rot90CCW,
        Rot180,
        Rotate,   // A = degrees
        SkewX,    // A = horizontal shift per pixel of height (base stays)
        SkewY,    // A = vertical shift per pixel of width (centre stays)
        Scale,    // A = x factor, B = y factor (bottom centre stays)
        Move,     // A = dx, B = dy
        Distort,  // P[8] = offsets of the corners TL, TR, BL, BR (dx, dy each)
        Warp      // P[18] = offsets of a 3x3 point grid (row by row, dx, dy each)
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

        public override string ToString() => Type switch
        {
            TransformType.FlipH => "Flip ↔",
            TransformType.FlipV => "Flip ↕",
            TransformType.Rot90CW => "Rotate 90° ↻",
            TransformType.Rot90CCW => "Rotate 90° ↺",
            TransformType.Rot180 => "Rotate 180°",
            TransformType.Rotate => $"Rotate {A:0.#}°",
            TransformType.SkewX => $"Skew X {A:0.00}",
            TransformType.SkewY => $"Skew Y {A:0.00}",
            TransformType.Scale => $"Scale {A * 100:0}% x {B * 100:0}%",
            TransformType.Move => $"Move {A:+0;-0;0} / {B:+0;-0;0}",
            TransformType.Distort => "Distort (4 corners)",
            TransformType.Warp => "Warp (3x3 points)",
            _ => Type.ToString()
        };
    }

    /// <summary>A complete transformation: canvas + steps.</summary>
    public sealed class TransformSettings
    {
        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }

        /// <summary>Keep the lowest pixel row where it was in the source (UO anchor: the item stands on the ground).</summary>
        public bool KeepBottom { get; set; } = true;

        public List<TransformStep> Steps { get; set; } = new List<TransformStep>();

        public TransformSettings Clone() => new TransformSettings
        {
            CanvasWidth = CanvasWidth,
            CanvasHeight = CanvasHeight,
            KeepBottom = KeepBottom,
            Steps = Steps.Select(s => s.Clone()).ToList()
        };
    }

    /// <summary>
    /// Pixel exact transformations for UO art (Photoshop "skew / distort / warp" without blur):
    /// every target pixel takes exactly ONE source pixel (nearest neighbour by inverse mapping), so the colours
    /// stay identical, nothing gets blended or faded. Flips and 90 degree turns are exact permutations.
    /// The steps are applied one after another to the original image every time (non-destructive), so a later
    /// flip never resets an earlier skew. Pivots: the bounding box of the image before the step.
    /// </summary>
    public static class ArtTransform
    {
        /// <summary>Applies all steps to <paramref name="source"/> and returns the result on the canvas.</summary>
        public static ArtPixelImage Apply(ArtPixelImage source, TransformSettings settings, int stepCount = int.MaxValue)
        {
            ArtPixelImage img = PlaceOnCanvas(source, settings);
            (int px, int py) = FixedPivot(img);
            int n = Math.Min(stepCount, settings.Steps.Count);
            for (int i = 0; i < n; ++i)
            {
                img = ApplyStep(img, settings.Steps[i], px, py);
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

            return img;
        }

        /// <summary>The image as it is before step <paramref name="index"/> (for the edit points of distort / warp).</summary>
        public static ArtPixelImage ApplyBefore(ArtPixelImage source, TransformSettings settings, int index)
        {
            ArtPixelImage img = PlaceOnCanvas(source, settings);
            (int px, int py) = FixedPivot(img);
            for (int i = 0; i < index && i < settings.Steps.Count; ++i)
            {
                img = ApplyStep(img, settings.Steps[i], px, py);
            }

            return img;
        }

        private static int CanvasWidth(ArtPixelImage src, TransformSettings s) => s.CanvasWidth > 0 ? s.CanvasWidth : src.Width;

        private static int CanvasHeight(ArtPixelImage src, TransformSettings s) => s.CanvasHeight > 0 ? s.CanvasHeight : src.Height;

        /// <summary>Source on the canvas, bottom centre aligned (UO anchor).</summary>
        public static ArtPixelImage PlaceOnCanvas(ArtPixelImage source, TransformSettings settings)
        {
            int w = CanvasWidth(source, settings), h = CanvasHeight(source, settings);
            var img = new ArtPixelImage(w, h);
            int ox = (w - source.Width) / 2, oy = h - source.Height;
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

        /// <summary>
        /// Integer pivot (a pixel corner near the centre of the placed source) for the 90/180 degree turns: turning
        /// around a pixel corner maps pixel centres exactly onto pixel centres, and because the pivot does not move
        /// between the steps, four 90 degree turns give back exactly the original.
        /// </summary>
        public static (int X, int Y) FixedPivot(ArtPixelImage placed)
        {
            GetBounds(placed, out int minX, out int minY, out int maxX, out int maxY);
            return maxX < minX
                ? (placed.Width / 2, placed.Height / 2)
                : ((minX + maxX + 1) / 2, (minY + maxY + 1) / 2);
        }

        public static ArtPixelImage ApplyStep(ArtPixelImage img, TransformStep step)
        {
            (int px, int py) = FixedPivot(img);
            return ApplyStep(img, step, px, py);
        }

        public static ArtPixelImage ApplyStep(ArtPixelImage img, TransformStep step, int pivotX, int pivotY)
        {
            GetBounds(img, out int minX, out int minY, out int maxX, out int maxY);
            if (maxX < minX)
            {
                return img;
            }

            // bounding box in continuous coordinates (pixel edges)
            double x0 = minX, y0 = minY, x1 = maxX + 1, y1 = maxY + 1;
            double cx = (x0 + x1) / 2, cy = (y0 + y1) / 2;

            switch (step.Type)
            {
                case TransformType.Move:
                    return Translate(img, (int)Math.Round(step.A), (int)Math.Round(step.B));
                case TransformType.FlipH:
                    return Resample(img, (x, y) => (2 * cx - x, y));
                case TransformType.FlipV:
                    return Resample(img, (x, y) => (x, 2 * cy - y));
                case TransformType.Rot180:
                    return Resample(img, (x, y) => ((2 * pivotX) - x, (2 * pivotY) - y));
                case TransformType.Rot90CW:
                    // forward: x' = px - (y - py), y' = py + (x - px)   (pivot = fixed pixel corner -> exact)
                    return Resample(img, (x, y) => (pivotX + (y - pivotY), pivotY - (x - pivotX)));
                case TransformType.Rot90CCW:
                    return Resample(img, (x, y) => (pivotX - (y - pivotY), pivotY + (x - pivotX)));
                case TransformType.Rotate:
                {
                    double a = -step.A * Math.PI / 180.0, cos = Math.Cos(a), sin = Math.Sin(a);
                    return Resample(img, (x, y) => (cx + ((x - cx) * cos) - ((y - cy) * sin), cy + ((x - cx) * sin) + ((y - cy) * cos)));
                }

                case TransformType.SkewX:
                    // base line (bottom) stays, the top leans by A pixels per pixel of height
                    return Resample(img, (x, y) => (x - (step.A * (y1 - y)), y));
                case TransformType.SkewY:
                    return Resample(img, (x, y) => (x, y - (step.A * (x - cx))));
                case TransformType.Scale:
                {
                    double sx = Math.Abs(step.A) < 0.01 ? 1 : step.A, sy = Math.Abs(step.B) < 0.01 ? 1 : step.B;
                    return Resample(img, (x, y) => (cx + ((x - cx) / sx), y1 + ((y - y1) / sy)));
                }

                case TransformType.Distort:
                    return Distort(img, step.P ?? new double[8], x0, y0, x1, y1);
                case TransformType.Warp:
                    return Warp(img, step.P ?? new double[18], x0, y0, x1, y1);
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

        #region Distort / warp
        /// <summary>Corner positions (TL, TR, BL, BR) of a distort step on the image before it.</summary>
        public static (double X, double Y)[] DistortHandles(ArtPixelImage before, double[] p)
        {
            GetBounds(before, out int minX, out int minY, out int maxX, out int maxY);
            if (maxX < minX)
            {
                return Array.Empty<(double, double)>();
            }

            double x0 = minX, y0 = minY, x1 = maxX + 1, y1 = maxY + 1;
            return new[]
            {
                (x0 + p[0], y0 + p[1]), (x1 + p[2], y0 + p[3]),
                (x0 + p[4], y1 + p[5]), (x1 + p[6], y1 + p[7])
            };
        }

        /// <summary>Positions of the 3x3 warp points on the image before the step.</summary>
        public static (double X, double Y)[] WarpHandles(ArtPixelImage before, double[] p)
        {
            GetBounds(before, out int minX, out int minY, out int maxX, out int maxY);
            if (maxX < minX)
            {
                return Array.Empty<(double, double)>();
            }

            double x0 = minX, y0 = minY, x1 = maxX + 1, y1 = maxY + 1;
            var h = new (double, double)[9];
            for (int j = 0; j < 3; ++j)
            {
                for (int i = 0; i < 3; ++i)
                {
                    int k = (j * 3) + i;
                    h[k] = (x0 + ((x1 - x0) * i / 2.0) + p[k * 2], y0 + ((y1 - y0) * j / 2.0) + p[(k * 2) + 1]);
                }
            }

            return h;
        }

        /// <summary>
        /// The bounding box becomes the quad given by the moved corners (bilinear). Inverse by Newton iteration;
        /// pixels outside the quad stay transparent.
        /// </summary>
        private static ArtPixelImage Distort(ArtPixelImage img, double[] p, double x0, double y0, double x1, double y1)
        {
            double ax = x0 + p[0], ay = y0 + p[1];   // TL
            double bx = x1 + p[2], by = y0 + p[3];   // TR
            double cx = x0 + p[4], cy = y1 + p[5];   // BL
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

                return (x0 + (s * (x1 - x0)), y0 + (t * (y1 - y0)));
            });
        }

        /// <summary>
        /// 3x3 point warp: every point of the bounding box is moved by the bilinear mix of the offsets of the
        /// surrounding grid points. Inverse by fixed point iteration.
        /// </summary>
        private static ArtPixelImage Warp(ArtPixelImage img, double[] p, double x0, double y0, double x1, double y1)
        {
            double w = Math.Max(1, x1 - x0), h = Math.Max(1, y1 - y0);

            (double X, double Y) Offset(double x, double y)
            {
                double u = Math.Clamp((x - x0) / w * 2, 0, 2), v = Math.Clamp((y - y0) / h * 2, 0, 2);
                int i = Math.Min(1, (int)u), j = Math.Min(1, (int)v);
                double fu = u - i, fv = v - j;
                double ox = 0, oy = 0;
                for (int b = 0; b <= 1; ++b)
                {
                    for (int a = 0; a <= 1; ++a)
                    {
                        double wgt = (a == 0 ? 1 - fu : fu) * (b == 0 ? 1 - fv : fv);
                        int k = ((j + b) * 3) + i + a;
                        ox += wgt * p[k * 2];
                        oy += wgt * p[(k * 2) + 1];
                    }
                }

                return (ox, oy);
            }

            return Resample(img, (px, py) =>
            {
                double sx = px, sy = py;
                for (int it = 0; it < 10; ++it)
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
