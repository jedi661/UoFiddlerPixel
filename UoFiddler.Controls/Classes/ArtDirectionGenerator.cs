using System;
using System.Collections.Generic;
using System.Linq;

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
        public int ShadowMaxLuma { get; set; } = 50;

        /// <summary>Max. colour saturation (0..255, max-min channel) of a shadow pixel.</summary>
        public int ShadowMaxSaturation { get; set; } = 24;

        /// <summary>Shadow length relative to the object height.</summary>
        public double ShadowLength { get; set; } = 0.5;

        /// <summary>Angle of the cast shadow in degrees: 0 = straight to the right, positive = upwards.</summary>
        public double ShadowAngle { get; set; } = 14;

        /// <summary>
        /// Automatic repair of every result: fills small enclosed holes, removes single stray pixels and
        /// replaces colour 0 inside the object (would be transparent in the client).
        /// </summary>
        public bool Repair { get; set; } = true;

        /// <summary>Largest enclosed transparent area (pixels) that is treated as a hole and filled.</summary>
        public int RepairMaxHole { get; set; } = 12;

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

            if (options.Repair)
            {
                img = Repair(img, options.RepairMaxHole);
            }

            result.Image = img;
            return result;
        }

        /// <summary>
        /// Automatic repair: 1. small transparent areas enclosed by the object (holes, they show as black dots in
        /// the client) are filled with the average of their neighbours, 2. single stray pixels without any
        /// neighbour are removed, 3. opaque colour 0 (shown transparent by the client) becomes the darkest grey.
        /// </summary>
        public static ArtPixelImage Repair(ArtPixelImage src, int maxHole = 12)
        {
            int w = src.Width, h = src.Height;
            ArtPixelImage img = src.Clone();

            // 1. holes: transparent areas not connected to the image border
            var outside = new bool[w * h];
            var stack = new Stack<int>();
            for (int x = 0; x < w; ++x)
            {
                PushIfTransparent(img, outside, stack, x, 0);
                PushIfTransparent(img, outside, stack, x, h - 1);
            }

            for (int y = 0; y < h; ++y)
            {
                PushIfTransparent(img, outside, stack, 0, y);
                PushIfTransparent(img, outside, stack, w - 1, y);
            }

            while (stack.Count > 0)
            {
                int p = stack.Pop();
                int px = p % w, py = p / w;
                if (px > 0) { PushIfTransparent(img, outside, stack, px - 1, py); }
                if (px < w - 1) { PushIfTransparent(img, outside, stack, px + 1, py); }
                if (py > 0) { PushIfTransparent(img, outside, stack, px, py - 1); }
                if (py < h - 1) { PushIfTransparent(img, outside, stack, px, py + 1); }
            }

            var seen = new bool[w * h];
            var hole = new List<int>();
            for (int i = 0; i < w * h; ++i)
            {
                if (img.Pixels[i] != 0 || outside[i] || seen[i])
                {
                    continue;
                }

                hole.Clear();
                stack.Push(i);
                seen[i] = true;
                while (stack.Count > 0)
                {
                    int p = stack.Pop();
                    hole.Add(p);
                    int px = p % w, py = p / w;
                    foreach (int n in new[] { px > 0 ? p - 1 : -1, px < w - 1 ? p + 1 : -1, py > 0 ? p - w : -1, py < h - 1 ? p + w : -1 })
                    {
                        if (n >= 0 && !seen[n] && img.Pixels[n] == 0 && !outside[n])
                        {
                            seen[n] = true;
                            stack.Push(n);
                        }
                    }
                }

                if (hole.Count > maxHole)
                {
                    continue; // a real opening (window, gap) - keep it
                }

                // fill from the edge inwards with the average of the already opaque neighbours
                var remaining = new HashSet<int>(hole);
                while (remaining.Count > 0)
                {
                    var filled = new List<(int P, ushort C)>();
                    foreach (int p in remaining)
                    {
                        int px = p % w, py = p / w;
                        int r = 0, g = 0, b = 0, n = 0;
                        for (int dy = -1; dy <= 1; ++dy)
                        {
                            for (int dx = -1; dx <= 1; ++dx)
                            {
                                int nx = px + dx, ny = py + dy;
                                if ((dx == 0 && dy == 0) || nx < 0 || ny < 0 || nx >= w || ny >= h)
                                {
                                    continue;
                                }

                                ushort c = img[nx, ny];
                                if (c == 0)
                                {
                                    continue;
                                }

                                r += R5(c);
                                g += G5(c);
                                b += B5(c);
                                ++n;
                            }
                        }

                        if (n > 0)
                        {
                            filled.Add((p, Pack(r / n, g / n, b / n)));
                        }
                    }

                    if (filled.Count == 0)
                    {
                        break;
                    }

                    foreach ((int P, ushort C) f in filled)
                    {
                        img.Pixels[f.P] = f.C;
                        remaining.Remove(f.P);
                    }
                }
            }

            // 2. stray pixels and 3. colour 0
            ArtPixelImage dst = img.Clone();
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    ushort c = img[x, y];
                    if (c == 0)
                    {
                        continue;
                    }

                    bool neighbour = false;
                    for (int dy = -1; dy <= 1 && !neighbour; ++dy)
                    {
                        for (int dx = -1; dx <= 1; ++dx)
                        {
                            int nx = x + dx, ny = y + dy;
                            if ((dx != 0 || dy != 0) && nx >= 0 && ny >= 0 && nx < w && ny < h && img[nx, ny] != 0)
                            {
                                neighbour = true;
                                break;
                            }
                        }
                    }

                    if (!neighbour)
                    {
                        dst[x, y] = 0;
                    }
                    else if ((c & 0x7FFF) == 0)
                    {
                        dst[x, y] = 0x8421;
                    }
                }
            }

            return dst;
        }

        private static void PushIfTransparent(ArtPixelImage img, bool[] outside, Stack<int> stack, int x, int y)
        {
            int i = (y * img.Width) + x;
            if (img.Pixels[i] == 0 && !outside[i])
            {
                outside[i] = true;
                stack.Push(i);
            }
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
                var region = new HashSet<int>();
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
                                region.Add((ny * w) + nx);
                            }
                        }
                    }
                }

                // 4. a drawn shadow is flat (1-2 colours, maybe dithered) and lies on the ground NEXT to the
                //    object, so it borders the transparent background. Dark stone/wood texture inside the object
                //    has many different shades and is surrounded by the object -> not a shadow.
                if (!LooksLikeShadow(src, region, w, h))
                {
                    continue;
                }

                foreach (int p in region)
                {
                    shadow[p] = true;
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

        private static bool LooksLikeShadow(ArtPixelImage src, HashSet<int> region, int w, int h)
        {
            if (region.Count < 8)
            {
                return false;
            }

            var colours = new Dictionary<ushort, int>();
            double sum = 0, sumSq = 0;
            int border = 0;
            foreach (int p in region)
            {
                ushort c = (ushort)(src.Pixels[p] & 0x7FFF);
                colours[c] = colours.TryGetValue(c, out int n) ? n + 1 : 1;
                int l = Luma(c);
                sum += l;
                sumSq += l * l;

                int px = p % w, py = p / w;
                bool touches = px == 0 || py == 0 || px == w - 1 || py == h - 1 ||
                               src.Pixels[p - 1] == 0 || src.Pixels[p + 1] == 0 ||
                               src.Pixels[p - w] == 0 || src.Pixels[p + w] == 0;
                if (touches)
                {
                    ++border;
                }
            }

            double mean = sum / region.Count;
            double std = Math.Sqrt(Math.Max(0, (sumSq / region.Count) - (mean * mean)));

            // the two most used colours must cover nearly the whole area (flat or 2-colour dither)
            int top2 = colours.Values.OrderByDescending(v => v).Take(2).Sum();

            return std <= 8 && top2 * 100 >= region.Count * 85 && border * 100 >= region.Count * 12;
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

            // fill every shadow row between its outermost projected points -> solid shadow without gaps/stairs
            var rowMin = new Dictionary<int, int>();
            var rowMax = new Dictionary<int, int>();
            foreach ((int X, int Y) p in points)
            {
                int x = p.X + grow, y = p.Y + growTop;
                rowMin[y] = rowMin.TryGetValue(y, out int mn) ? Math.Min(mn, x) : x;
                rowMax[y] = rowMax.TryGetValue(y, out int mx) ? Math.Max(mx, x) : x;
            }

            foreach (int y in rowMin.Keys)
            {
                if (y < 0 || y >= dst.Height)
                {
                    continue;
                }

                for (int x = Math.Max(0, rowMin[y]); x <= Math.Min(dst.Width - 1, rowMax[y]); ++x)
                {
                    dst[x, y] = sc;
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
