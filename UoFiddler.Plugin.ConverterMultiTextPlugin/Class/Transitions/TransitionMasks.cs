using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.Transitions
{
    /// <summary>The six edge types of CentrED+ (TilesBrush.xml).</summary>
    internal enum EdgeType { DR, DL, UU, LL, UR, UL }

    /// <summary>
    /// One of the 12 transition pieces. "A_" pieces belong to brush A (B comes in at the edge / corner),
    /// "B_" pieces belong to brush B (A comes in). Mask: white = texture A, black = texture B.
    /// </summary>
    internal sealed class TransitionSlot
    {
        public TransitionSlot(bool isA, EdgeType type)
        {
            IsA = isA;
            Type = type;
        }

        public bool IsA { get; }

        public EdgeType Type { get; }

        public string Name => (IsA ? "A_" : "B_") + Type;

        public string Description
        {
            get
            {
                string other = IsA ? "B" : "A";
                switch (Type)
                {
                    case EdgeType.UU: return $"{other} at the top edge (north)";
                    case EdgeType.LL: return $"{other} at the left edge (west)";
                    case EdgeType.UL: return $"{other} in the top left corner";
                    case EdgeType.UR: return $"{other} in the top right corner";
                    case EdgeType.DL: return $"{other} in the bottom left corner";
                    default: return $"{other} in the bottom right corner";
                }
            }
        }

        public override string ToString() => Name;
    }

    /// <summary>Settings of the mask generator (saved with a pattern).</summary>
    internal sealed class MaskSettings
    {
        public int Softness { get; set; } = 35;     // width of the soft border, 0..100
        public int Roughness { get; set; } = 40;    // how much the border wanders, 0..100
        public int Seed { get; set; } = 1;
        public int EdgeShift { get; set; }          // -40..40: moves the border into A (-) or B (+)
        public int CornerSize { get; set; } = 50;   // corner shape: 20..25 = cut off straight, 50 = round, 100 = almost square
        public int Blur { get; set; }               // extra blur of the masks when blending, 0..8 px
        public int Contrast { get; set; }           // -100..100: harder / softer border when blending
    }

    internal static class TransitionMasks
    {
        public static readonly TransitionSlot[] Slots =
        {
            new TransitionSlot(true, EdgeType.DR), new TransitionSlot(true, EdgeType.DL), new TransitionSlot(true, EdgeType.UU),
            new TransitionSlot(true, EdgeType.LL), new TransitionSlot(true, EdgeType.UR), new TransitionSlot(true, EdgeType.UL),
            new TransitionSlot(false, EdgeType.DR), new TransitionSlot(false, EdgeType.DL), new TransitionSlot(false, EdgeType.UU),
            new TransitionSlot(false, EdgeType.LL), new TransitionSlot(false, EdgeType.UR), new TransitionSlot(false, EdgeType.UL)
        };

        public static int IndexOf(string name) => Array.FindIndex(Slots, s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

        #region Noise

        /// <summary>Smooth value noise 0..1 that repeats with the tile, so neighbouring pieces fit together.</summary>
        private static double[] Noise(int size, int seed)
        {
            var rnd = new Random(seed);
            double[] Grid(int cells)
            {
                var g = new double[cells * cells];
                for (int i = 0; i < g.Length; i++)
                {
                    g[i] = rnd.NextDouble();
                }
                return g;
            }
            double Sample(double[] g, int cells, double u, double v)
            {
                double x = u * cells, y = v * cells;
                int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
                double fx = x - x0, fy = y - y0;
                fx = fx * fx * (3 - 2 * fx);
                fy = fy * fy * (3 - 2 * fy);
                int W(int i) => ((i % cells) + cells) % cells;
                double a = g[W(y0) * cells + W(x0)], b = g[W(y0) * cells + W(x0 + 1)];
                double c = g[W(y0 + 1) * cells + W(x0)], d = g[W(y0 + 1) * cells + W(x0 + 1)];
                return (a * (1 - fx) + b * fx) * (1 - fy) + (c * (1 - fx) + d * fx) * fy;
            }
            double[] g1 = Grid(4), g2 = Grid(8), g3 = Grid(16);
            var n = new double[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double u = (double)x / size, v = (double)y / size;
                    n[y * size + x] = Sample(g1, 4, u, v) * 0.55 + Sample(g2, 8, u, v) * 0.3 + Sample(g3, 16, u, v) * 0.15;
                }
            }
            return n;
        }

        #endregion

        #region Generate

        /// <summary>
        /// How much the "other" texture comes in at (u, v): 1 at its side / corner, 0.5 on the border.
        /// Every border meets the tile edges exactly in the middle, so all 12 pieces fit together without seams.
        /// cornerPower: 1 = corner cut off straight, 2 = round, higher = more square.
        /// </summary>
        private static double Shape(EdgeType type, double u, double v, double cornerPower)
        {
            double Corner(double cu, double cv)
            {
                double d = Math.Pow(Math.Pow(Math.Abs(u - cu), cornerPower) + Math.Pow(Math.Abs(v - cv), cornerPower), 1 / cornerPower);
                return 1 - d;
            }
            switch (type)
            {
                case EdgeType.UU: return 1 - v;
                case EdgeType.LL: return 1 - u;
                case EdgeType.UL: return Corner(0, 0);
                case EdgeType.UR: return Corner(1, 0);
                case EdgeType.DL: return Corner(0, 1);
                default: return Corner(1, 1);
            }
        }

        /// <summary>Creates the mask of one slot (white = A, black = B).</summary>
        public static Bitmap Generate(TransitionSlot slot, MaskSettings s, int size)
        {
            double[] noise = s.Roughness > 0 ? Noise(size, s.Seed) : null;
            double width = Math.Max(0.004, s.Softness / 100.0 * 0.45);
            double rough = s.Roughness / 100.0 * 0.5;
            double shift = s.EdgeShift / 100.0;
            double power = Math.Max(1.0, s.CornerSize / 25.0);
            // B_ pieces describe the same border from the other side: shift and noise work the other way round.
            double sign = slot.IsA ? 1 : -1;
            var px = new int[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int i = y * size + x;
                    double val = Shape(slot.Type, (x + 0.5) / size, (y + 0.5) / size, power) + sign * shift;
                    if (noise != null)
                    {
                        val += sign * (noise[i] - 0.5) * rough;
                    }
                    double t = Math.Max(0, Math.Min(1, (val - 0.5) / width + 0.5));
                    t = t * t * (3 - 2 * t);                // amount of the texture that comes in
                    double a = slot.IsA ? 1 - t : t;         // amount of texture A
                    int g = (int)Math.Round(a * 255);
                    px[i] = unchecked((int)0xFF000000) | (g << 16) | (g << 8) | g;
                }
            }
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            CutterImaging.SetPixels(bmp, px);
            return bmp;
        }

        #endregion

        #region Mask helpers

        private static double[] ToValues(Bitmap mask, int size)
        {
            using (Bitmap m = mask.Width == size && mask.Height == size ? CutterImaging.To32Bpp(mask) : TextureImaging.Resize(mask, size, size, TextureImaging.ResizeQuality.Smooth))
            {
                int[] px = CutterImaging.GetPixels(m);
                var v = new double[px.Length];
                for (int i = 0; i < px.Length; i++)
                {
                    int p = px[i];
                    v[i] = (((p >> 16) & 0xFF) * 0.3 + ((p >> 8) & 0xFF) * 0.59 + (p & 0xFF) * 0.11) / 255.0;
                }
                return v;
            }
        }

        private static Bitmap FromValues(double[] v, int size)
        {
            var px = new int[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                int g = (int)Math.Round(Math.Max(0, Math.Min(1, v[i])) * 255);
                px[i] = unchecked((int)0xFF000000) | (g << 16) | (g << 8) | g;
            }
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            CutterImaging.SetPixels(bmp, px);
            return bmp;
        }

        /// <summary>Box blur (wrapping at the edges, so the tile stays repeatable).</summary>
        private static double[] BoxBlur(double[] v, int size, int radius)
        {
            if (radius <= 0)
            {
                return v;
            }
            var tmp = new double[v.Length];
            var dst = new double[v.Length];
            int W(int i) => ((i % size) + size) % size;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double sum = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        sum += v[y * size + W(x + k)];
                    }
                    tmp[y * size + x] = sum / (2 * radius + 1);
                }
            }
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double sum = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        sum += tmp[W(y + k) * size + x];
                    }
                    dst[y * size + x] = sum / (2 * radius + 1);
                }
            }
            return dst;
        }

        /// <summary>Blur and contrast of the old tool, done right: contrast &gt; 0 = harder border, &lt; 0 = softer.</summary>
        public static double[] Prepare(Bitmap mask, int size, int blur, int contrast)
        {
            double[] v = ToValues(mask, size);
            v = BoxBlur(v, size, blur);
            if (contrast != 0)
            {
                double k = contrast > 0 ? 1 + contrast / 25.0 : 1 / (1 - contrast / 25.0);
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] = Math.Max(0, Math.Min(1, (v[i] - 0.5) * k + 0.5));
                }
            }
            return v;
        }

        public static Bitmap Invert(Bitmap mask)
        {
            double[] v = ToValues(mask, mask.Width);
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = 1 - v[i];
            }
            return FromValues(v, mask.Width);
        }

        public static Bitmap SmoothMask(Bitmap mask, int radius) => FromValues(BoxBlur(ToValues(mask, mask.Width), mask.Width, radius), mask.Width);

        #endregion

        #region Blend

        /// <summary>A × mask + B × (1 − mask) at the output size.</summary>
        public static Bitmap Blend(Bitmap a, Bitmap b, Bitmap mask, int size, int blur, int contrast)
        {
            using (Bitmap ra = TextureImaging.Resize(a, size, size, TextureImaging.ResizeQuality.Smooth))
            using (Bitmap rb = TextureImaging.Resize(b, size, size, TextureImaging.ResizeQuality.Smooth))
            {
                int[] pa = CutterImaging.GetPixels(ra), pb = CutterImaging.GetPixels(rb);
                double[] m = Prepare(mask, size, blur, contrast);
                var dst = new int[pa.Length];
                for (int i = 0; i < dst.Length; i++)
                {
                    double t = m[i];
                    int p = pa[i], q = pb[i];
                    int Mix(int shift) => (int)Math.Round(((p >> shift) & 0xFF) * t + ((q >> shift) & 0xFF) * (1 - t));
                    dst[i] = unchecked((int)0xFF000000) | (Mix(16) << 16) | (Mix(8) << 8) | Mix(0);
                }
                var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                CutterImaging.SetPixels(bmp, dst);
                return bmp;
            }
        }

        #endregion

        #region Learn: extract a mask from an existing transition

        /// <summary>
        /// Calculates how much of A and B is in an existing transition T (white = A).
        /// averageColors: compare with the average colors of A and B (robust when the pixels do not match exactly);
        /// otherwise pixel by pixel. detail = blur radius before comparing (0..6).
        /// </summary>
        public static Bitmap Extract(Bitmap a, Bitmap b, Bitmap t, bool averageColors, int detail, out double reliability)
        {
            int size = t.Width;
            using (Bitmap ra = TextureImaging.Resize(a, size, t.Height, TextureImaging.ResizeQuality.Smooth))
            using (Bitmap rb = TextureImaging.Resize(b, size, t.Height, TextureImaging.ResizeQuality.Smooth))
            using (Bitmap rt = TextureImaging.Resize(t, size, size, TextureImaging.ResizeQuality.Smooth))
            using (Bitmap sa = TextureImaging.Resize(ra, size, size, TextureImaging.ResizeQuality.Smooth))
            using (Bitmap sb = TextureImaging.Resize(rb, size, size, TextureImaging.ResizeQuality.Smooth))
            {
                double[][] Channels(Bitmap bmp)
                {
                    int[] px = CutterImaging.GetPixels(bmp);
                    var c = new[] { new double[px.Length], new double[px.Length], new double[px.Length] };
                    for (int i = 0; i < px.Length; i++)
                    {
                        c[0][i] = (px[i] >> 16) & 0xFF;
                        c[1][i] = (px[i] >> 8) & 0xFF;
                        c[2][i] = px[i] & 0xFF;
                    }
                    return c;
                }
                double[][] A = Channels(sa), B = Channels(sb), T = Channels(rt);
                for (int ch = 0; ch < 3; ch++)
                {
                    T[ch] = BoxBlur(T[ch], size, detail);
                    if (averageColors)
                    {
                        double ma = A[ch].Average(), mb = B[ch].Average();
                        for (int i = 0; i < A[ch].Length; i++)
                        {
                            A[ch][i] = ma;
                            B[ch][i] = mb;
                        }
                    }
                    else
                    {
                        A[ch] = BoxBlur(A[ch], size, detail);
                        B[ch] = BoxBlur(B[ch], size, detail);
                    }
                }
                int n = size * size;
                var w = new double[n];
                var conf = new double[n];
                for (int i = 0; i < n; i++)
                {
                    double dot = 0, len = 0;
                    for (int ch = 0; ch < 3; ch++)
                    {
                        double ab = A[ch][i] - B[ch][i];
                        dot += (T[ch][i] - B[ch][i]) * ab;
                        len += ab * ab;
                    }
                    conf[i] = Math.Min(1, len / 900.0); // ~30 color steps difference = fully reliable
                    w[i] = len < 1 ? 0.5 : Math.Max(0, Math.Min(1, dot / len));
                }
                // Where A and B have nearly the same color the result is unsure: fill from the surroundings.
                var wc = new double[n];
                for (int i = 0; i < n; i++)
                {
                    wc[i] = w[i] * conf[i];
                }
                double[] bw = BoxBlur(BoxBlur(wc, size, 3), size, 3), bc = BoxBlur(BoxBlur(conf, size, 3), size, 3);
                for (int i = 0; i < n; i++)
                {
                    double fill = bc[i] > 0.001 ? bw[i] / bc[i] : 0.5;
                    w[i] = w[i] * conf[i] + fill * (1 - conf[i]);
                }
                reliability = conf.Average();
                return FromValues(BoxBlur(w, size, 1), size);
            }
        }

        #endregion

        #region Auto tiling (assembled preview)

        /// <summary>
        /// For an A / B map (true = B) the piece each cell needs: -1 = pure A, -2 = pure B, otherwise the slot index.
        /// </summary>
        public static int PieceFor(bool[,] isB, int x, int y)
        {
            int w = isB.GetLength(0), h = isB.GetLength(1);
            bool B(int dx, int dy)
            {
                int xx = Math.Max(0, Math.Min(w - 1, x + dx)), yy = Math.Max(0, Math.Min(h - 1, y + dy));
                return isB[xx, yy];
            }
            if (B(0, 0))
            {
                return -2;
            }
            bool n = B(0, -1), s = B(0, 1), we = B(-1, 0), e = B(1, 0);
            int Slot(bool isA, EdgeType t) => Array.FindIndex(Slots, sl => sl.IsA == isA && sl.Type == t);
            if (n && we) return Slot(false, EdgeType.DR);
            if (n && e) return Slot(false, EdgeType.DL);
            if (s && we) return Slot(false, EdgeType.UR);
            if (s && e) return Slot(false, EdgeType.UL);
            if (n) return Slot(true, EdgeType.UU);
            if (we) return Slot(true, EdgeType.LL);
            if (s) return Slot(false, EdgeType.UU);
            if (e) return Slot(false, EdgeType.LL);
            if (B(-1, -1)) return Slot(true, EdgeType.UL);
            if (B(1, -1)) return Slot(true, EdgeType.UR);
            if (B(-1, 1)) return Slot(true, EdgeType.DL);
            if (B(1, 1)) return Slot(true, EdgeType.DR);
            return -1;
        }

        /// <summary>Removes B areas that are too thin for the 12 pieces (an A cell between two B cells becomes B).</summary>
        public static void Clean(bool[,] isB)
        {
            int w = isB.GetLength(0), h = isB.GetLength(1);
            for (int pass = 0; pass < 4; pass++)
            {
                bool changed = false;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        if (isB[x, y])
                        {
                            continue;
                        }
                        bool n = y > 0 && isB[x, y - 1], s = y < h - 1 && isB[x, y + 1], we = x > 0 && isB[x - 1, y], e = x < w - 1 && isB[x + 1, y];
                        bool diagOpposite = (x > 0 && y > 0 && isB[x - 1, y - 1] && x < w - 1 && y < h - 1 && isB[x + 1, y + 1])
                                            || (x < w - 1 && y > 0 && isB[x + 1, y - 1] && x > 0 && y < h - 1 && isB[x - 1, y + 1]);
                        if ((n && s) || (we && e) || diagOpposite)
                        {
                            isB[x, y] = true;
                            changed = true;
                        }
                    }
                }
                if (!changed)
                {
                    break;
                }
            }
        }

        public static readonly string[] PatternNames = { "Lake", "Road (straight)", "Road (curve)", "Islands (random)", "Half / half" };

        public static bool[,] MakePattern(int index, int size, int seed)
        {
            var b = new bool[size, size];
            var rnd = new Random(seed);
            double c = (size - 1) / 2.0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    switch (index)
                    {
                        case 0:
                            b[x, y] = (x - c) * (x - c) / (size * size / 9.0) + (y - c) * (y - c) / (size * size / 14.0) < 1;
                            break;
                        case 1:
                            b[x, y] = y >= size / 2 - 1 && y <= size / 2;
                            break;
                        case 2:
                            b[x, y] = (y >= size / 2 - 1 && y <= size / 2 && x <= size / 2) || (x >= size / 2 - 1 && x <= size / 2 && y <= size / 2);
                            break;
                        case 3:
                            break;
                        default:
                            b[x, y] = x + y > size;
                            break;
                    }
                }
            }
            if (index == 3)
            {
                for (int i = 0; i < size / 2; i++)
                {
                    int bx = rnd.Next(1, size - 2), by = rnd.Next(1, size - 2), r = rnd.Next(1, 3);
                    for (int y = by - r; y <= by + r; y++)
                    {
                        for (int x = bx - r; x <= bx + r; x++)
                        {
                            if (x >= 0 && y >= 0 && x < size && y < size)
                            {
                                b[x, y] = true;
                            }
                        }
                    }
                }
            }
            Clean(b);
            return b;
        }

        #endregion

        #region Pattern library

        public static string LibraryRoot => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TransitionLibrary");

        internal sealed class LibraryEntry
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public string Folder { get; set; }
            public Bitmap Preview { get; set; }

            public override string ToString() => $"{Category} / {Name}";
        }

        private static string Safe(string text)
        {
            string s = string.IsNullOrWhiteSpace(text) ? "Unnamed" : text.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }
            return s;
        }

        /// <summary>Saves the 12 masks, the settings and a preview as a pattern. Returns the folder.</summary>
        public static string SavePattern(string category, string name, IReadOnlyList<Bitmap> masks, MaskSettings settings, Bitmap preview)
        {
            string folder = Path.Combine(LibraryRoot, Safe(category), Safe(name));
            Directory.CreateDirectory(folder);
            for (int i = 0; i < Slots.Length; i++)
            {
                if (masks[i] != null)
                {
                    masks[i].Save(Path.Combine(folder, Slots[i].Name + ".png"), ImageFormat.Png);
                }
            }
            File.WriteAllText(Path.Combine(folder, "settings.json"), JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            preview?.Save(Path.Combine(folder, "preview.png"), ImageFormat.Png);
            return folder;
        }

        public static List<LibraryEntry> ListPatterns()
        {
            var list = new List<LibraryEntry>();
            if (!Directory.Exists(LibraryRoot))
            {
                return list;
            }
            foreach (string cat in Directory.GetDirectories(LibraryRoot).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
            {
                foreach (string set in Directory.GetDirectories(cat).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
                {
                    var entry = new LibraryEntry { Category = Path.GetFileName(cat), Name = Path.GetFileName(set), Folder = set };
                    string preview = Path.Combine(set, "preview.png");
                    if (File.Exists(preview))
                    {
                        try
                        {
                            entry.Preview = CutterImaging.LoadBitmap(preview);
                        }
                        catch (Exception)
                        {
                            // no preview
                        }
                    }
                    list.Add(entry);
                }
            }
            return list;
        }

        /// <summary>Reads masks named A_DR, B_UL, ... (any image format, name may have more after the type) from a folder.</summary>
        public static Bitmap[] LoadMasksFromFolder(string folder, out int found)
        {
            var masks = new Bitmap[Slots.Length];
            found = 0;
            string[] ext = { ".png", ".bmp", ".gif", ".jpg", ".jpeg", ".tif", ".tiff" };
            foreach (string file in Directory.GetFiles(folder).OrderBy(f => f))
            {
                if (!ext.Contains(Path.GetExtension(file).ToLowerInvariant()))
                {
                    continue;
                }
                string name = Path.GetFileNameWithoutExtension(file);
                if (name.Length < 4)
                {
                    continue;
                }
                int index = IndexOf(name.Substring(0, 4));
                if (index < 0 || masks[index] != null)
                {
                    continue;
                }
                try
                {
                    masks[index] = CutterImaging.LoadBitmap(file);
                    found++;
                }
                catch (Exception)
                {
                    // unreadable file
                }
            }
            return masks;
        }

        public static MaskSettings LoadSettings(string folder)
        {
            string file = Path.Combine(folder, "settings.json");
            try
            {
                return File.Exists(file) ? JsonSerializer.Deserialize<MaskSettings>(File.ReadAllText(file)) ?? new MaskSettings() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region CentrED+ XML

        /// <summary>TilesBrush.xml for CentrED+: brush A with its land tiles and its edge to B, and the same for B.</summary>
        public static string BuildXml(string root, string brushIdA, string nameA, IEnumerable<int> landA, string brushIdB, string nameB, IEnumerable<int> landB, IReadOnlyList<int> pieceLandIds)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine($"<{root}>");
            void Brush(string id, string name, IEnumerable<int> lands, string to, bool isA)
            {
                sb.AppendLine($"  <Brush Id=\"{id}\" Name=\"{System.Security.SecurityElement.Escape(name)}\">");
                foreach (int land in lands)
                {
                    sb.AppendLine($"    <Land ID=\"0x{land:X4}\" />");
                }
                sb.AppendLine($"    <Edge To=\"{to}\">");
                for (int i = 0; i < Slots.Length; i++)
                {
                    if (Slots[i].IsA == isA && pieceLandIds[i] >= 0)
                    {
                        sb.AppendLine($"      <Land Type=\"{Slots[i].Type}\" ID=\"0x{pieceLandIds[i]:X4}\" />");
                    }
                }
                sb.AppendLine("    </Edge>");
                sb.AppendLine("  </Brush>");
            }
            Brush(brushIdA, nameA, landA, brushIdB, true);
            Brush(brushIdB, nameB, landB, brushIdA, false);
            sb.AppendLine($"</{root}>");
            return sb.ToString();
        }

        #endregion
    }
}
