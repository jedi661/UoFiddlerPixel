using System;
using System.Collections.Generic;

namespace UoFiddler.Controls.Classes
{
    /// <summary>Facing of an item in the Ultima Online isometric view.</summary>
    public enum ArtDirection
    {
        South = 0, // front shows to the lower left  ("\" wall running along the X axis)
        East = 1,  // front shows to the lower right ("/" wall running along the Y axis)
        North = 2, // back side of South
        West = 3   // back side of East
    }

    /// <summary>How the other side of an object can be derived.</summary>
    public enum ArtObjectKind
    {
        /// <summary>Walls, fences, windows, doors in frame: front and back look the same (N = S, W = E). Exact.</summary>
        Symmetric = 0,

        /// <summary>Lies flat on the ground (rugs, floor decals, puddles): the opposite side is a 180 degree turn. Exact.</summary>
        Flat = 1,

        /// <summary>Furniture, statues ...: the back side cannot be computed - a shaded template is created to paint over.</summary>
        Solid = 2
    }

    public enum ArtShadowMode
    {
        /// <summary>Treat dark shadow pixels like the object (they get mirrored with it).</summary>
        KeepAsIs = 0,

        /// <summary>Detect a drawn shadow, remove it and cast it again in the UO light direction for every result.</summary>
        Regenerate = 1,

        /// <summary>Like Regenerate, but cast a shadow even if the source has none.</summary>
        AlwaysCast = 2,

        /// <summary>Remove detected shadow pixels and cast none.</summary>
        Remove = 3
    }

    /// <summary>Per pixel classification used for re-lighting (and shown as overlay in the form).</summary>
    public enum ArtFaceClass : byte
    {
        Transparent = 0,
        LeftFace = 1,  // faces to the lower left (south face)
        RightFace = 2, // faces to the lower right (east face)
        Top = 3,
        Edge = 4,      // the front corner column
        Shadow = 5
    }

    /// <summary>16-bit ARGB1555 image exactly like art.mul pixels (0 = transparent).</summary>
    public sealed class ArtPixelImage
    {
        public ArtPixelImage(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new ushort[width * height];
        }

        public int Width { get; }
        public int Height { get; }
        public ushort[] Pixels { get; }

        public ushort this[int x, int y]
        {
            get => Pixels[(y * Width) + x];
            set => Pixels[(y * Width) + x] = value;
        }

        public bool IsOpaque(int x, int y) => Pixels[(y * Width) + x] != 0;

        public ArtPixelImage Clone()
        {
            var c = new ArtPixelImage(Width, Height);
            Array.Copy(Pixels, c.Pixels, Pixels.Length);
            return c;
        }
    }

    public sealed class ArtDirectionOptions
    {
        public ArtObjectKind Kind { get; set; } = ArtObjectKind.Symmetric;

        /// <summary>Swap the brightness of the left and right faces when the image is mirrored.</summary>
        public bool Relight { get; set; } = true;

        /// <summary>0..1 how much of the computed re-lighting is applied.</summary>
        public double RelightStrength { get; set; } = 1.0;

        /// <summary>
        /// Brightness of the lower-right (east) faces relative to the lower-left (south) faces, used when the
        /// source does not show both face types. UO art is lit from the left: east faces are darker.
        /// </summary>
        public double DefaultLightRatio { get; set; } = 0.78;

        public ArtShadowMode Shadow { get; set; } = ArtShadowMode.Regenerate;

        /// <summary>Max. brightness (0..255) of a pixel to count as shadow.</summary>
        public int ShadowMaxLuma { get; set; } = 70;

        /// <summary>Max. colour saturation (0..255, max-min channel) of a shadow pixel.</summary>
        public int ShadowMaxSaturation { get; set; } = 24;

        /// <summary>Shadow length relative to the object height.</summary>
        public double ShadowLength { get; set; } = 0.5;

        /// <summary>Angle of the cast shadow in degrees: 0 = straight to the right, positive = upwards.</summary>
        public double ShadowAngle { get; set; } = 14;

        /// <summary>Colour used for a cast shadow when none was detected (ARGB1555 without alpha bit).</summary>
        public ushort DefaultShadowColor { get; set; } = 0x0C63; // dark grey (3,3,3)
    }

    public sealed class ArtDirectionResult
    {
        public ArtDirection Direction { get; set; }
        public ArtPixelImage Image { get; set; }

        /// <summary>True if the result is exact (pure pixel transform), false for a template to paint over.</summary>
        public bool IsExact { get; set; }

        public string Note { get; set; }
    }

    /// <summary>
    /// Creates the other facings (N/E/S/W) of a UO static from one drawn facing.
    ///
    /// Geometry (all transforms keep the art anchor: bottom center of the image):
    ///   South &lt;-&gt; East and North &lt;-&gt; West are mirror images: horizontal flip, exact.
    ///   South &lt;-&gt; North (and East &lt;-&gt; West) depend on the object:
    ///      Symmetric (walls): identical image,  Flat: rotated 180 degrees,  Solid: shaded back template.
    ///
    /// Light: UO light comes from the left, so faces looking to the lower left are brighter than faces looking
    /// to the lower right. A plain mirror would put the bright face on the right. The generator classifies the
    /// pixels into left face / right face / top (from the bottom silhouette: left and right of the front corner,
    /// top face above an automatically detected seam) and swaps the brightness of the two face types.
    /// Shadow: dark grey blobs are detected as drawn shadow, removed and cast again behind the new object.
    /// </summary>
    public static class ArtDirectionGenerator
    {
        public static ArtDirection MirrorOf(ArtDirection d) => d switch
        {
            ArtDirection.South => ArtDirection.East,
            ArtDirection.East => ArtDirection.South,
            ArtDirection.North => ArtDirection.West,
            _ => ArtDirection.North
        };

        public static ArtDirection OppositeOf(ArtDirection d) => (ArtDirection)(((int)d + 2) % 4);

        #region Public API
        public static List<ArtDirectionResult> GenerateAll(ArtPixelImage source, ArtDirection sourceDirection,
            IEnumerable<ArtDirection> targets, ArtDirectionOptions options)
        {
            var list = new List<ArtDirectionResult>();
            foreach (ArtDirection t in targets)
            {
                list.Add(Generate(source, sourceDirection, t, options));
            }

            return list;
        }

        public static ArtDirectionResult Generate(ArtPixelImage source, ArtDirection sourceDirection,
            ArtDirection target, ArtDirectionOptions options)
        {
            options ??= new ArtDirectionOptions();

            // 1. split object / drawn shadow
            SplitShadow(source, options, out ArtPixelImage obj, out ushort shadowColor, out bool hadShadow);

            bool castShadow = options.Shadow == ArtShadowMode.AlwaysCast ||
                              (options.Shadow == ArtShadowMode.Regenerate && hadShadow);
            if (!hadShadow)
            {
                shadowColor = options.DefaultShadowColor;
            }

            // 2. geometry
            bool mirror = target == MirrorOf(sourceDirection) || target == MirrorOf(OppositeOf(sourceDirection));
            bool opposite = target == OppositeOf(sourceDirection) || target == MirrorOf(OppositeOf(sourceDirection));

            var result = new ArtDirectionResult { Direction = target, IsExact = true };
            ArtPixelImage img = obj;

            if (opposite)
            {
                switch (options.Kind)
                {
                    case ArtObjectKind.Symmetric:
                        result.Note = "Symmetric object: back side = front side.";
                        break;
                    case ArtObjectKind.Flat:
                        img = Rotate180(img);
                        result.Note = "Flat object: rotated 180°.";
                        break;
                    default:
                        img = BackTemplate(img, options);
                        result.IsExact = false;
                        result.Note = "Back side can not be computed - shaded template, please paint the details.";
                        break;
                }
            }

            if (mirror)
            {
                ArtFaceClass[] classes = options.Kind == ArtObjectKind.Flat ? null : Classify(img, out _);
                img = FlipX(img);
                if (classes != null && options.Relight && options.RelightStrength > 0)
                {
                    img = RelightAfterMirror(img, FlipX(classes, img.Width, img.Height), options);
                }

                result.Note = (result.Note == null ? string.Empty : result.Note + " ") + "Mirrored" +
                              (options.Relight && options.Kind != ArtObjectKind.Flat ? " and re-lit." : ".");
            }

            if (!mirror && !opposite)
            {
                result.Note = "Source direction.";
            }

            // 3. shadow
            if (castShadow)
            {
                img = CastShadow(img, shadowColor, options);
            }

            result.Image = img;
            return result;
        }

        /// <summary>Face classification of an image (for the overlay preview).</summary>
        public static ArtFaceClass[] ClassifyForPreview(ArtPixelImage source, ArtDirectionOptions options)
        {
            SplitShadow(source, options ?? new ArtDirectionOptions(), out ArtPixelImage obj, out _, out _);
            ArtFaceClass[] classes = Classify(obj, out _);
            for (int i = 0; i < classes.Length; ++i)
            {
                if (source.Pixels[i] != 0 && obj.Pixels[i] == 0)
                {
                    classes[i] = ArtFaceClass.Shadow;
                }
            }

            return classes;
        }
        #endregion

        #region Colour helpers
        public static int R5(ushort c) => (c >> 10) & 0x1F;
        public static int G5(ushort c) => (c >> 5) & 0x1F;
        public static int B5(ushort c) => c & 0x1F;

        public static int Luma(ushort c)
        {
            // 0..255
            int r = R5(c) * 255 / 31, g = G5(c) * 255 / 31, b = B5(c) * 255 / 31;
            return ((r * 299) + (g * 587) + (b * 114)) / 1000;
        }

        private static int Saturation(ushort c)
        {
            int r = R5(c), g = G5(c), b = B5(c);
            return (Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b))) * 255 / 31;
        }

        private static ushort Pack(int r, int g, int b)
        {
            r = Math.Clamp(r, 0, 31);
            g = Math.Clamp(g, 0, 31);
            b = Math.Clamp(b, 0, 31);
            if (r == 0 && g == 0 && b == 0)
            {
                // colour 0 would turn the pixel transparent in the client
                r = g = b = 1;
            }

            return (ushort)(0x8000 | (r << 10) | (g << 5) | b);
        }

        private static ushort Scale(ushort c, double f)
        {
            return Pack((int)Math.Round(R5(c) * f), (int)Math.Round(G5(c) * f), (int)Math.Round(B5(c) * f));
        }
        #endregion

        #region Geometry
        public static ArtPixelImage FlipX(ArtPixelImage src)
        {
            var dst = new ArtPixelImage(src.Width, src.Height);
            for (int y = 0; y < src.Height; ++y)
            {
                for (int x = 0; x < src.Width; ++x)
                {
                    dst[src.Width - 1 - x, y] = src[x, y];
                }
            }

            return dst;
        }

        private static ArtFaceClass[] FlipX(ArtFaceClass[] src, int w, int h)
        {
            var dst = new ArtFaceClass[src.Length];
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    ArtFaceClass c = src[(y * w) + x];
                    // a left face becomes a right face when mirrored
                    c = c == ArtFaceClass.LeftFace ? ArtFaceClass.RightFace : c == ArtFaceClass.RightFace ? ArtFaceClass.LeftFace : c;
                    dst[(y * w) + (w - 1 - x)] = c;
                }
            }

            return dst;
        }

        public static ArtPixelImage Rotate180(ArtPixelImage src)
        {
            // rotate inside the opaque bounding box, so the object stays on the ground (bottom anchor) and centered
            GetBounds(src, out int minX, out int minY, out int maxX, out int maxY);
            var dst = new ArtPixelImage(src.Width, src.Height);
            if (maxX < minX)
            {
                return dst;
            }

            for (int y = minY; y <= maxY; ++y)
            {
                for (int x = minX; x <= maxX; ++x)
                {
                    dst[minX + maxX - x, minY + maxY - y] = src[x, y];
                }
            }

            return dst;
        }

        private static void GetBounds(ArtPixelImage img, out int minX, out int minY, out int maxX, out int maxY)
        {
            minX = img.Width;
            minY = img.Height;
            maxX = -1;
            maxY = -1;
            for (int y = 0; y < img.Height; ++y)
            {
                for (int x = 0; x < img.Width; ++x)
                {
                    if (!img.IsOpaque(x, y))
                    {
                        continue;
                    }

                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
            }
        }
        #endregion

        #region Shadow
        private static bool IsShadowCandidate(ushort c, ArtDirectionOptions o)
        {
            return c != 0 && Luma(c) <= o.ShadowMaxLuma && Saturation(c) <= o.ShadowMaxSaturation;
        }

        /// <summary>
        /// Shadow = connected dark grey area that is "thick" (most of its pixels are surrounded by shadow pixels).
        /// Thin dark outlines - typical for UO art - are kept as part of the object.
        /// </summary>
        public static void SplitShadow(ArtPixelImage src, ArtDirectionOptions o, out ArtPixelImage obj, out ushort shadowColor, out bool hadShadow)
        {
            obj = src.Clone();
            shadowColor = o.DefaultShadowColor;
            hadShadow = false;

            if (o.Shadow == ArtShadowMode.KeepAsIs)
            {
                return;
            }

            int w = src.Width, h = src.Height;

            // 1. core = dark grey pixels whose 4 neighbours are dark grey too (thin outlines have no core)
            var candidate = new bool[w * h];
            for (int i = 0; i < candidate.Length; ++i)
            {
                candidate[i] = IsShadowCandidate(src.Pixels[i], o);
            }

            var core = new bool[w * h];
            for (int y = 1; y < h - 1; ++y)
            {
                for (int x = 1; x < w - 1; ++x)
                {
                    int i = (y * w) + x;
                    core[i] = candidate[i] && candidate[i - 1] && candidate[i + 1] && candidate[i - w] && candidate[i + w];
                }
            }

            GetBounds(src, out _, out int boundTop, out _, out int boundBottom);
            int groundLimit = boundBottom - (int)((boundBottom - boundTop) * 0.4);

            // 2. core components that lie on the ground (lower 40%) are shadows - dark windows higher up are not
            var visited = new bool[w * h];
            var stack = new Stack<int>();
            var component = new List<int>();
            var shadow = new bool[w * h];
            for (int start = 0; start < w * h; ++start)
            {
                if (!core[start] || visited[start])
                {
                    continue;
                }

                component.Clear();
                stack.Push(start);
                visited[start] = true;
                int maxY = 0;
                while (stack.Count > 0)
                {
                    int p = stack.Pop();
                    component.Add(p);
                    int px = p % w, py = p / w;
                    maxY = Math.Max(maxY, py);
                    for (int d = 0; d < 4; ++d)
                    {
                        int nx = px + (d == 0 ? 1 : d == 1 ? -1 : 0);
                        int ny = py + (d == 2 ? 1 : d == 3 ? -1 : 0);
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                        {
                            continue;
                        }

                        int n = (ny * w) + nx;
                        if (core[n] && !visited[n])
                        {
                            visited[n] = true;
                            stack.Push(n);
                        }
                    }
                }

                if (component.Count < 6 || maxY < groundLimit)
                {
                    continue;
                }

                // 3. core + its dark rim (8 neighbours)
                foreach (int p in component)
                {
                    int px = p % w, py = p / w;
                    for (int dy = -1; dy <= 1; ++dy)
                    {
                        for (int dx = -1; dx <= 1; ++dx)
                        {
                            int nx = px + dx, ny = py + dy;
                            if (nx >= 0 && ny >= 0 && nx < w && ny < h && candidate[(ny * w) + nx])
                            {
                                shadow[(ny * w) + nx] = true;
                            }
                        }
                    }
                }
            }

            long sr = 0, sg = 0, sb = 0, sn = 0;
            for (int i = 0; i < shadow.Length; ++i)
            {
                if (!shadow[i])
                {
                    continue;
                }

                hadShadow = true;
                ushort c = src.Pixels[i];
                sr += R5(c);
                sg += G5(c);
                sb += B5(c);
                ++sn;
                obj.Pixels[i] = 0;
            }

            if (sn > 0)
            {
                shadowColor = (ushort)(Pack((int)(sr / sn), (int)(sg / sn), (int)(sb / sn)) & 0x7FFF);
            }
        }

        /// <summary>
        /// Casts the object silhouette onto the ground: every object pixel at height h above the bottom of its
        /// column throws a shadow point h*length away in the light direction. Drawn only behind the object.
        /// The canvas grows symmetrically left/right and at the top so the art anchor (bottom center) stays.
        /// </summary>
        public static ArtPixelImage CastShadow(ArtPixelImage obj, ushort shadowColor, ArtDirectionOptions o)
        {
            int w = obj.Width, h = obj.Height;
            var bottom = new int[w];
            for (int x = 0; x < w; ++x)
            {
                bottom[x] = -1;
                for (int y = h - 1; y >= 0; --y)
                {
                    if (obj.IsOpaque(x, y))
                    {
                        bottom[x] = y;
                        break;
                    }
                }
            }

            double rad = o.ShadowAngle * Math.PI / 180.0;
            double dx = Math.Cos(rad) * o.ShadowLength;
            double dy = -Math.Sin(rad) * o.ShadowLength;

            var points = new List<(int X, int Y)>();
            int minX = 0, maxX = w - 1, minY = 0;
            for (int x = 0; x < w; ++x)
            {
                if (bottom[x] < 0)
                {
                    continue;
                }

                for (int y = 0; y <= bottom[x]; ++y)
                {
                    if (!obj.IsOpaque(x, y))
                    {
                        continue;
                    }

                    int height = bottom[x] - y;
                    int sx = x + (int)Math.Round(height * dx);
                    int sy = bottom[x] + (int)Math.Round(height * dy);
                    points.Add((sx, sy));
                    minX = Math.Min(minX, sx);
                    maxX = Math.Max(maxX, sx);
                    minY = Math.Min(minY, sy);
                }
            }

            if (points.Count == 0)
            {
                return obj;
            }

            int grow = Math.Max(-minX, maxX - (w - 1));
            int growTop = Math.Max(0, -minY);
            var dst = new ArtPixelImage(w + (2 * grow), h + growTop);
            ushort sc = (ushort)(0x8000 | (shadowColor == 0 ? 0x0421 : shadowColor));

            foreach ((int X, int Y) p in points)
            {
                int x = p.X + grow, y = p.Y + growTop;
                if (x >= 0 && y >= 0 && x < dst.Width && y < dst.Height)
                {
                    dst[x, y] = sc;
                    // close gaps of the projection (1 px dilation to the left)
                    if (x > 0 && dst[x - 1, y] == 0)
                    {
                        dst[x - 1, y] = sc;
                    }
                }
            }

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    ushort c = obj[x, y];
                    if (c != 0)
                    {
                        dst[x + grow, y + growTop] = c;
                    }
                }
            }

            return dst;
        }
        #endregion

        #region Faces / light
        /// <summary>
        /// Classifies the opaque pixels: columns left of the front corner (lowest bottom point) are the left (south)
        /// face, columns right of it the right (east) face. Pixels higher than the detected top seam are top face.
        /// </summary>
        public static ArtFaceClass[] Classify(ArtPixelImage img, out int seam)
        {
            int w = img.Width, h = img.Height;
            var classes = new ArtFaceClass[w * h];
            var bottom = new int[w];
            int lowest = -1;
            for (int x = 0; x < w; ++x)
            {
                bottom[x] = -1;
                for (int y = h - 1; y >= 0; --y)
                {
                    if (img.IsOpaque(x, y))
                    {
                        bottom[x] = y;
                        break;
                    }
                }

                lowest = Math.Max(lowest, bottom[x]);
            }

            seam = 0;
            if (lowest < 0)
            {
                return classes;
            }

            // front corner: middle of the columns that reach (almost) the lowest point
            int first = -1, last = -1;
            for (int x = 0; x < w; ++x)
            {
                if (bottom[x] >= lowest - 1)
                {
                    if (first < 0)
                    {
                        first = x;
                    }

                    last = x;
                }
            }


            // top seam: offset d above the bottom silhouette with the strongest brightness step
            double bestScore = 0;
            int columns = 0;
            for (int x = 0; x < w; ++x)
            {
                if (bottom[x] >= 0)
                {
                    ++columns;
                }
            }

            for (int d = 2; d < h - 1; ++d)
            {
                double sum = 0;
                int n = 0;
                for (int x = 0; x < w; ++x)
                {
                    int y = bottom[x] - d;
                    if (bottom[x] < 0 || y - 1 < 0 || !img.IsOpaque(x, y) || !img.IsOpaque(x, y - 1))
                    {
                        continue;
                    }

                    sum += Math.Abs(Luma(img[x, y]) - Luma(img[x, y - 1]));
                    ++n;
                }

                if (n < Math.Max(3, columns / 4))
                {
                    continue;
                }

                double score = sum / n;
                if (score > bestScore)
                {
                    bestScore = score;
                    seam = d;
                }
            }

            if (bestScore < 12)
            {
                seam = 0; // no clear top face
            }

            // Front corner. Coordinates are doubled (corner2 = 2 * x) so the split is exact between two columns and
            // a mirrored image gets exactly the mirrored classes. First guess: middle of the lowest columns;
            // refined to the strongest brightness step between two neighbouring columns near it (the drawn edge).
            int corner2 = first + last;
            double bestEdge = 0;
            for (int b = Math.Max(1, first - 3); b <= Math.Min(w - 1, last + 4); ++b)
            {
                double sum = 0;
                int n = 0;
                for (int y = 0; y < h; ++y)
                {
                    bool face = bottom[b] >= 0 && bottom[b - 1] >= 0 &&
                                (seam == 0 || (y >= bottom[b] - seam && y >= bottom[b - 1] - seam));
                    if (!face || !img.IsOpaque(b - 1, y) || !img.IsOpaque(b, y))
                    {
                        continue;
                    }

                    sum += Math.Abs(Luma(img[b - 1, y]) - Luma(img[b, y]));
                    ++n;
                }

                if (n < 3)
                {
                    continue;
                }

                double score = sum / n;
                if (score > bestEdge + 0.001)
                {
                    bestEdge = score;
                    corner2 = (2 * b) - 1; // split between column b-1 and b
                }
            }

            if (bestEdge < 10)
            {
                corner2 = first + last; // no drawn edge: silhouette guess
            }

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    if (!img.IsOpaque(x, y))
                    {
                        continue;
                    }

                    ArtFaceClass c;
                    if (seam > 0 && bottom[x] >= 0 && y < bottom[x] - seam)
                    {
                        c = ArtFaceClass.Top;
                    }
                    else if (2 * x < corner2)
                    {
                        c = ArtFaceClass.LeftFace;
                    }
                    else if (2 * x > corner2)
                    {
                        c = ArtFaceClass.RightFace;
                    }
                    else
                    {
                        c = ArtFaceClass.Edge;
                    }

                    classes[(y * w) + x] = c;
                }
            }

            return classes;
        }

        /// <summary>Mean brightness of a face class, -1 if (almost) not present.</summary>
        private static double MeanLuma(ArtPixelImage img, ArtFaceClass[] classes, ArtFaceClass which)
        {
            long sum = 0;
            int n = 0;
            for (int i = 0; i < classes.Length; ++i)
            {
                if (classes[i] == which)
                {
                    sum += Luma(img.Pixels[i]);
                    ++n;
                }
            }

            return n < 10 ? -1 : (double)sum / n;
        }

        /// <summary>
        /// <paramref name="mirrored"/> is the already flipped image, <paramref name="classes"/> its (flipped)
        /// classes. Before the flip the current right faces were left faces - their brightness is swapped.
        /// </summary>
        private static ArtPixelImage RelightAfterMirror(ArtPixelImage mirrored, ArtFaceClass[] classes, ArtDirectionOptions o)
        {
            // brightness of the faces as they were in the source (source left = now right)
            double sourceLeft = MeanLuma(mirrored, classes, ArtFaceClass.RightFace);
            double sourceRight = MeanLuma(mirrored, classes, ArtFaceClass.LeftFace);

            double ratio = sourceLeft > 0 && sourceRight > 0 ? sourceRight / sourceLeft : o.DefaultLightRatio;
            ratio = Math.Clamp(ratio, 0.3, 1.6);
            double s = Math.Clamp(o.RelightStrength, 0, 1);

            // pixels now on the right (were the left face) get the right-face brightness and vice versa
            double toRight = 1 + (s * (ratio - 1));
            double toLeft = 1 + (s * ((1 / ratio) - 1));

            ArtPixelImage dst = mirrored.Clone();
            for (int i = 0; i < classes.Length; ++i)
            {
                ushort c = mirrored.Pixels[i];
                if (c == 0)
                {
                    continue;
                }

                if (classes[i] == ArtFaceClass.RightFace)
                {
                    dst.Pixels[i] = Scale(c, toRight);
                }
                else if (classes[i] == ArtFaceClass.LeftFace)
                {
                    dst.Pixels[i] = Scale(c, toLeft);
                }
            }

            return dst;
        }

        /// <summary>Back-side template: same footprint, every face filled with its average colour.</summary>
        private static ArtPixelImage BackTemplate(ArtPixelImage src, ArtDirectionOptions o)
        {
            ArtFaceClass[] classes = Classify(src, out _);
            var sums = new long[6, 4];
            for (int i = 0; i < classes.Length; ++i)
            {
                ushort c = src.Pixels[i];
                if (c == 0)
                {
                    continue;
                }

                int k = (int)classes[i];
                sums[k, 0] += R5(c);
                sums[k, 1] += G5(c);
                sums[k, 2] += B5(c);
                sums[k, 3]++;
            }

            var dst = new ArtPixelImage(src.Width, src.Height);
            for (int i = 0; i < classes.Length; ++i)
            {
                if (src.Pixels[i] == 0)
                {
                    continue;
                }

                int k = (int)classes[i];
                long n = Math.Max(1, sums[k, 3]);
                ushort avg = Pack((int)(sums[k, 0] / n), (int)(sums[k, 1] / n), (int)(sums[k, 2] / n));
                // keep the outline so the template is easy to paint over
                bool outline = Luma(src.Pixels[i]) < 40;
                dst.Pixels[i] = outline ? src.Pixels[i] : avg;
            }

            return dst;
        }
        #endregion
    }
}
