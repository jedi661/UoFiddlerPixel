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
        Solid = 2,

        /// <summary>
        /// Objects that POINT somewhere (cannon, chair, bench, statue facing a way): the opposite direction is a
        /// 180 degree turn on the ground - the ground line is reversed, the height of every column is kept.
        /// Pixel based approximation (the back side uses the colours of the front side).
        /// </summary>
        Directional = 3
    }

    /// <summary>Manual transformation for one target direction (overrides the automatic one).</summary>
    public enum ArtManualMethod
    {
        Auto = 0,         // what the object type does
        Copy = 1,         // same as the drawn image
        Mirror = 2,       // horizontal flip  (south <-> east, north <-> west)
        FlipVertical = 3, // upside down inside the object's bounds
        Rotate180 = 4,    // turned 180 degrees in the picture plane
        TurnCrossAxis = 5,// reflection across the line perpendicular to the object axis (keeps round shapes)
        TurnShear = 6     // column shift along the axis (keeps vertical parts vertical)
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

            // 2. geometry. The face classes travel with the pixels; "odd" counts the horizontal flips - after an odd
            //    number the lit left face is on the right and the face light has to be swapped.
            bool mirror = target == MirrorOf(sourceDirection) || target == MirrorOf(OppositeOf(sourceDirection));
            bool opposite = target == OppositeOf(sourceDirection) || target == MirrorOf(OppositeOf(sourceDirection));

            var result = new ArtDirectionResult { Direction = target, IsExact = true };
            ArtPixelImage img = obj;
            ArtFaceClass[] cls = options.Kind == ArtObjectKind.Flat ? null : Classify(img, out _);
            bool odd = false;
            var notes = new List<string>();

            if (opposite)
            {
                switch (options.Kind)
                {
                    case ArtObjectKind.Symmetric:
                    {
                        // walls / doors: same picture on the opposite (back) edge of the tile. North and West sit on
                        // the upper edges, 22 px higher - the frame grows instead of cutting the graphic off.
                        (int dx, int dy) = UoTile.OppositeEdgeShift((int)sourceDirection);
                        (int nw, int nh, int[] smap) = UoTile.ShiftInFieldMap(img, dx, dy);
                        var shifted = new ArtPixelImage(nw, nh);
                        for (int i = 0; i < smap.Length; ++i)
                        {
                            if (smap[i] >= 0)
                            {
                                shifted.Pixels[i] = img.Pixels[smap[i]];
                            }
                        }

                        img = shifted;
                        cls = ApplyMap(cls, smap);
                        notes.Add($"Wall/door: moved to the opposite edge of the tile ({dx:+0;-0}/{dy:+0;-0} px, frame {nw}x{nh}).");
                        break;
                    }
                    case ArtObjectKind.Flat:
                        img = Rotate180(img);
                        notes.Add("Flat object: rotated 180°.");
                        break;
                    case ArtObjectKind.Directional:
                        // drawn pointing West/East lies on a "\\" diagonal, North/South on "/"
                        int slope2 = sourceDirection == ArtDirection.West || sourceDirection == ArtDirection.East ? 1 : -1;
                        int[] map = TurnOnGroundMap(img, slope2, 0, out _, out bool axisMismatch);
                        if (axisMismatch)
                        {
                            notes.Add("WARNING: the object lies on the other diagonal - check 'Drawn direction' " +
                                      "(walls/doors: use 'Wall / fence') or use Manual.");
                        }

                        img = ApplyMap(img, map);
                        cls = ApplyMap(cls, map);
                        odd = !odd; // the turn contains a horizontal flip
                        result.IsExact = false; // back side = front colours, round parts get a little sheared
                        notes.Add("Turned 180° on the ground (points the other way) - approximation, please retouch the details.");
                        break;
                    default:
                        img = BackTemplate(img, options);
                        cls = Classify(img, out _);
                        result.IsExact = false;
                        notes.Add("Back side can not be computed - shaded template, please paint the details.");
                        break;
                }
            }

            if (mirror)
            {
                int[] map = FlipXMap(img.Width, img.Height);
                img = ApplyMap(img, map);
                cls = ApplyMap(cls, map);
                odd = !odd;
                notes.Add("Mirrored.");
            }

            if (odd && cls != null && options.Relight && options.RelightStrength > 0)
            {
                // RelightAfterMirror expects RightFace = "was the (lit) left face in the source"
                img = RelightAfterMirror(img, SwapFaces(cls), options);
                notes.Add("Re-lit.");
            }

            result.Note = notes.Count == 0 ? "Source direction." : string.Join(" ", notes);

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
            return Repair(src, maxHole, true);
        }

        private static ArtPixelImage Repair(ArtPixelImage src, int maxHole, bool fixPixels)
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

            if (!fixPixels)
            {
                return img;
            }

            // 2. stray pixels, 3. colour 0, 4. white halo on the outline (from images on white background)
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

                    bool edge = (x == 0 || img[x - 1, y] == 0) || (x == w - 1 || img[x + 1, y] == 0) ||
                                (y == 0 || img[x, y - 1] == 0) || (y == h - 1 || img[x, y + 1] == 0);

                    if (!neighbour || (edge && IsWhiteEdge(c)))
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

        /// <summary>Kinds of problems that show up in the client as dots/holes on black or white ground.</summary>
        [Flags]
        public enum ArtError : byte
        {
            None = 0,
            Hole = 1,       // transparent pixel enclosed by the object
            Stray = 2,      // single pixel without neighbours
            Black = 4,      // opaque colour 0 -> transparent in the client
            WhiteEdge = 8   // near-white pixel on the outline (white halo from a white background)
        }

        /// <summary>Marks every error pixel. <paramref name="maxHole"/>: larger enclosed areas are openings, no error.</summary>
        public static ArtError[] FindErrors(ArtPixelImage img, int maxHole, out int holes, out int strays, out int blacks, out int whiteEdges)
        {
            int w = img.Width, h = img.Height;
            var errors = new ArtError[w * h];
            holes = strays = blacks = whiteEdges = 0;

            // holes: compare with the repaired image - every transparent pixel that the repair fills is a hole
            ArtPixelImage repaired = Repair(img, maxHole, false);
            for (int i = 0; i < errors.Length; ++i)
            {
                if (img.Pixels[i] == 0 && repaired.Pixels[i] != 0)
                {
                    errors[i] |= ArtError.Hole;
                    ++holes;
                }
            }

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    ushort c = img[x, y];
                    if (c == 0)
                    {
                        continue;
                    }

                    int i = (y * w) + x;
                    int neighbours = 0;
                    bool edge = false;
                    for (int dy = -1; dy <= 1; ++dy)
                    {
                        for (int dx = -1; dx <= 1; ++dx)
                        {
                            if (dx == 0 && dy == 0)
                            {
                                continue;
                            }

                            int nx = x + dx, ny = y + dy;
                            bool opaque = nx >= 0 && ny >= 0 && nx < w && ny < h && img[nx, ny] != 0;
                            if (opaque)
                            {
                                ++neighbours;
                            }
                            else if (dx == 0 || dy == 0)
                            {
                                edge = true;
                            }
                        }
                    }

                    if (neighbours == 0)
                    {
                        errors[i] |= ArtError.Stray;
                        ++strays;
                    }

                    if ((c & 0x7FFF) == 0)
                    {
                        errors[i] |= ArtError.Black;
                        ++blacks;
                    }

                    if (edge && IsWhiteEdge(c))
                    {
                        errors[i] |= ArtError.WhiteEdge;
                        ++whiteEdges;
                    }
                }
            }

            return errors;
        }

        private static bool IsWhiteEdge(ushort c) => Luma(c) >= 235 && Saturation(c) <= 40;

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

        #region Manual
        /// <summary>
        /// One direction with a manually chosen transformation (instead of the automatic one of the object type).
        /// <paramref name="axisSlope"/>: slope of the object axis on screen for the turn methods (0 = measure,
        /// e.g. 1 = "\" UO tile diagonal (45 degrees), -1 = "/"). Shadow, re-light and repair work like in <see cref="Generate"/>.
        /// </summary>
        public static ArtDirectionResult GenerateManual(ArtPixelImage source, ArtDirection target, ArtManualMethod method,
            double axisSlope, ArtDirectionOptions options)
        {
            options ??= new ArtDirectionOptions();
            SplitShadow(source, options, out ArtPixelImage obj, out ushort shadowColor, out bool hadShadow);
            bool castShadow = options.Shadow == ArtShadowMode.AlwaysCast || (options.Shadow == ArtShadowMode.Regenerate && hadShadow);
            if (!hadShadow)
            {
                shadowColor = options.DefaultShadowColor;
            }

            ArtPixelImage img = obj;
            ArtFaceClass[] cls = Classify(img, out _);
            int[] map = null;
            bool odd = false;
            string note;
            int slope2 = (axisSlope != 0 ? axisSlope : MeasureTopSlope(img)) < 0 ? -1 : 1;

            switch (method)
            {
                case ArtManualMethod.Mirror:
                    map = FlipXMap(img.Width, img.Height);
                    odd = true;
                    note = "Manual: mirrored.";
                    break;
                case ArtManualMethod.FlipVertical:
                    map = FlipYInBoundsMap(img);
                    note = "Manual: flipped vertically.";
                    break;
                case ArtManualMethod.Rotate180:
                    map = Rotate180Map(img);
                    odd = true;
                    note = "Manual: rotated 180°.";
                    break;
                case ArtManualMethod.TurnCrossAxis:
                    map = TurnOnGroundMap(img, slope2, axisSlope != 0 ? axisSlope : MeasureTopSlope(img), out double k1, out _);
                    odd = true;
                    note = $"Manual: turned across the cross axis (axis slope {k1:0.00}).";
                    break;
                case ArtManualMethod.TurnShear:
                    double k2 = axisSlope != 0 ? axisSlope : MeasureTopSlope(img);
                    map = TurnShearMap(img, k2);
                    odd = true;
                    note = $"Manual: turned by column shift (axis slope {k2:0.00}).";
                    break;
                default:
                    note = "Manual: copy of the drawn image.";
                    break;
            }

            if (map != null)
            {
                img = ApplyMap(img, map);
                cls = ApplyMap(cls, map);
            }

            if (odd && cls != null && options.Relight && options.RelightStrength > 0)
            {
                img = RelightAfterMirror(img, SwapFaces(cls), options);
                note += " Re-lit.";
            }

            if (castShadow)
            {
                img = CastShadow(img, shadowColor, options);
            }

            if (options.Repair)
            {
                img = Repair(img, options.RepairMaxHole);
            }

            return new ArtDirectionResult { Direction = target, Image = img, IsExact = method != ArtManualMethod.TurnCrossAxis && method != ArtManualMethod.TurnShear, Note = note };
        }

        /// <summary>Moves the whole image by dx/dy pixels (same size, pixels leaving the image are cut off).</summary>
        public static ArtPixelImage Translate(ArtPixelImage src, int dx, int dy)
        {
            if (dx == 0 && dy == 0)
            {
                return src;
            }

            var dst = new ArtPixelImage(src.Width, src.Height);
            for (int y = 0; y < src.Height; ++y)
            {
                for (int x = 0; x < src.Width; ++x)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && ny >= 0 && nx < src.Width && ny < src.Height)
                    {
                        dst[nx, ny] = src[x, y];
                    }
                }
            }

            return dst;
        }

        /// <summary>Robust slope of the upper outline (0 if there is none).</summary>
        public static double MeasureTopSlope(ArtPixelImage img)
        {
            GetBounds(img, out int minX, out int minY, out int maxX, out int maxY);
            if (maxX < minX)
            {
                return 0;
            }

            var top = new int[img.Width];
            Array.Fill(top, -1);
            for (int x = minX; x <= maxX; ++x)
            {
                for (int y = minY; y <= maxY; ++y)
                {
                    if (img.IsOpaque(x, y))
                    {
                        top[x] = y;
                        break;
                    }
                }
            }

            return MedianSlope(top, minX, maxX);
        }

        private static int[] FlipYInBoundsMap(ArtPixelImage img)
        {
            int w = img.Width, h = img.Height;
            var map = new int[w * h];
            Array.Fill(map, -1);
            GetBounds(img, out _, out int minY, out _, out int maxY);
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    if (img.IsOpaque(x, y))
                    {
                        map[((minY + maxY - y) * w) + x] = (y * w) + x;
                    }
                }
            }

            return map;
        }

        private static int[] Rotate180Map(ArtPixelImage img)
        {
            int w = img.Width, h = img.Height;
            var map = new int[w * h];
            Array.Fill(map, -1);
            GetBounds(img, out int minX, out int minY, out int maxX, out int maxY);
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    if (img.IsOpaque(x, y))
                    {
                        map[((minY + maxY - y) * w) + (minX + maxX - x)] = (y * w) + x;
                    }
                }
            }

            return map;
        }

        /// <summary>
        /// Turn by column shift: x' = minX + maxX - x, y' = y + k * (minX + maxX - 2x). Keeps vertical parts
        /// vertical (good for poles, legs), round parts get sheared. Lowest row kept.
        /// </summary>
        public static int[] TurnShearMap(ArtPixelImage img, double k)
        {
            int w = img.Width, h = img.Height;
            var map = new int[w * h];
            Array.Fill(map, -1);
            GetBounds(img, out int minX, out int minY, out int maxX, out int maxY);
            if (maxX < minX)
            {
                return map;
            }

            int c2 = minX + maxX;
            int Shift(int x) => (int)Math.Round(k * (c2 - (2 * x)), MidpointRounding.AwayFromZero);

            int newMax = int.MinValue;
            for (int y = minY; y <= maxY; ++y)
            {
                for (int x = minX; x <= maxX; ++x)
                {
                    if (img.IsOpaque(x, y))
                    {
                        newMax = Math.Max(newMax, y + Shift(x));
                    }
                }
            }

            int anchor = maxY - newMax;
            for (int y = minY; y <= maxY; ++y)
            {
                for (int x = minX; x <= maxX; ++x)
                {
                    int ny = y + Shift(x) + anchor;
                    if (img.IsOpaque(x, y) && ny >= 0 && ny < h)
                    {
                        map[(ny * w) + (c2 - x)] = (y * w) + x;
                    }
                }
            }

            return map;
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

        #region Pixel maps (dst index -> src index, -1 = transparent)
        private static int[] FlipXMap(int w, int h)
        {
            var map = new int[w * h];
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    map[(y * w) + (w - 1 - x)] = (y * w) + x;
                }
            }

            return map;
        }

        /// <summary>
        /// 180 degree turn on the ground for objects that point somewhere (cannon, chair ...): the image is
        /// reflected across the line that runs through the centre of the object PERPENDICULAR to its own axis.
        /// That reverses the pointing direction (a cannon pointing to the upper left points to the lower right)
        /// while everything keeps its side of the axis - the highlight stays on top of the barrel, the carriage
        /// stays below it - and, being a mirror, it does not distort shapes: round muzzles and knobs stay round.
        /// The axis is the robust (Theil-Sen) slope of the upper outline; if it does not lie on the expected
        /// diagonal (<paramref name="slope2"/>: +1 = "\" for West/East, -1 = "/" for North/South) the UO tile
        /// diagonal (45 degrees, slope 1) is used. Nearest-neighbour sampling (every target pixel takes exactly one source pixel, no holes).
        /// Same image size, the lowest pixel keeps its row (UO anchor).
        /// </summary>
        public static int[] TurnOnGroundMap(ArtPixelImage img, int slope2)
        {
            return TurnOnGroundMap(img, slope2, 0, out _, out _);
        }

        /// <param name="kOverride">axis slope to use instead of the measured one (0 = measure).</param>
        /// <param name="usedK">the axis slope that was used.</param>
        /// <param name="mismatch">true if the measured axis does not lie on the expected diagonal.</param>
        public static int[] TurnOnGroundMap(ArtPixelImage img, int slope2, double kOverride, out double usedK, out bool mismatch)
        {
            usedK = slope2; // UO tiles: 45 degree diagonals
            mismatch = false;
            int w = img.Width, h = img.Height;
            var map = new int[w * h];
            Array.Fill(map, -1);

            GetBounds(img, out int minX, out int minY, out int maxX, out int maxY);
            if (maxX < minX)
            {
                return map;
            }

            var top = new int[w];
            Array.Fill(top, -1);
            for (int x = minX; x <= maxX; ++x)
            {
                for (int y = minY; y <= maxY; ++y)
                {
                    if (img.IsOpaque(x, y))
                    {
                        top[x] = y;
                        break;
                    }
                }
            }

            double k = MedianSlope(top, minX, maxX);
            if (kOverride != 0)
            {
                k = kOverride;
            }
            else if (Math.Sign(k) != Math.Sign(slope2) || Math.Abs(k) < 0.25 || Math.Abs(k) > 2.5)
            {
                mismatch = Math.Abs(k) >= 0.25 && Math.Abs(k) <= 2.5; // a clear axis, but on the other diagonal
                k = slope2; // UO diagonal (45 degrees)
            }

            usedK = k;

            // reflection across the line through the centre of the object with angle axis + 90 degrees
            // (an involution). Afterwards the turned object is seated on the ground again (see below).
            double cx = (minX + maxX + 1) / 2.0, cy = (minY + maxY + 1) / 2.0;
            double phi = Math.Atan(k) + (Math.PI / 2);
            double c2 = Math.Cos(2 * phi), s2 = Math.Sin(2 * phi);
            (double X, double Y) Reflect(double x, double y)
            {
                double dx = x - cx, dy = y - cy;
                return (cx + (c2 * dx) + (s2 * dy), cy + (s2 * dx) - (c2 * dy));
            }

            // UO: a graphic can never reach below the bottom corner of its tile (drawY = ty + 44 - h), an object
            // that now points towards the viewer stands on its FRONT tile. So the turned object is seated on the
            // ground row again: its lowest pixel row keeps the distance to the fix point row it had before.
            double newMax = double.MinValue;
            for (int y = minY; y <= maxY; ++y)
            {
                for (int x = minX; x <= maxX; ++x)
                {
                    if (img.IsOpaque(x, y))
                    {
                        newMax = Math.Max(newMax, Reflect(x + 0.5, y + 0.5).Y);
                    }
                }
            }

            int shift = maxY - (int)Math.Floor(newMax);

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    (double sx, double sy) = Reflect(x + 0.5, y + 0.5 - shift);
                    int ix = (int)Math.Floor(sx), iy = (int)Math.Floor(sy);
                    if (ix >= 0 && iy >= 0 && ix < w && iy < h && img.IsOpaque(ix, iy))
                    {
                        map[(y * w) + x] = (iy * w) + ix;
                    }
                }
            }

            return map;
        }

        /// <summary>Theil-Sen slope (median of the slopes of all point pairs at least 3 columns apart).</summary>
        private static double MedianSlope(int[] values, int from, int to)
        {
            var slopes = new List<double>();
            for (int a = from; a <= to; ++a)
            {
                if (values[a] < 0)
                {
                    continue;
                }

                for (int b = a + 3; b <= to; ++b)
                {
                    if (values[b] >= 0)
                    {
                        slopes.Add((values[b] - values[a]) / (double)(b - a));
                    }
                }
            }

            if (slopes.Count == 0)
            {
                return 0;
            }

            slopes.Sort();
            return slopes[slopes.Count / 2];
        }

        private static ArtPixelImage ApplyMap(ArtPixelImage src, int[] map)
        {
            var dst = new ArtPixelImage(src.Width, src.Height);
            for (int i = 0; i < map.Length; ++i)
            {
                if (map[i] >= 0)
                {
                    dst.Pixels[i] = src.Pixels[map[i]];
                }
            }

            return dst;
        }

        private static ArtFaceClass[] ApplyMap(ArtFaceClass[] src, int[] map)
        {
            if (src == null)
            {
                return null;
            }

            var dst = new ArtFaceClass[map.Length]; // the map may describe a bigger frame (shift in the field)
            for (int i = 0; i < map.Length; ++i)
            {
                if (map[i] >= 0)
                {
                    dst[i] = src[map[i]];
                }
            }

            return dst;
        }

        private static ArtFaceClass[] SwapFaces(ArtFaceClass[] src)
        {
            var dst = new ArtFaceClass[src.Length];
            for (int i = 0; i < src.Length; ++i)
            {
                ArtFaceClass c = src[i];
                dst[i] = c == ArtFaceClass.LeftFace ? ArtFaceClass.RightFace : c == ArtFaceClass.RightFace ? ArtFaceClass.LeftFace : c;
            }

            return dst;
        }
        #endregion

        /// <summary>
        /// 180 degree turn of a flat object around the centre of its UO tile. The tile of a w x h graphic is the
        /// 44x44 diamond whose bottom corner is the fix point (middle pixel of the lowest row), so its centre is
        /// the pixel corner (floor(w/2), h - 22). Turning around a pixel corner is an exact permutation; a 44x44
        /// floor tile is turned onto itself.
        /// </summary>
        public static ArtPixelImage Rotate180(ArtPixelImage src)
        {
            int cx = src.Width / 2, cy = src.Height - UoTile.Half;
            var dst = new ArtPixelImage(src.Width, src.Height);
            for (int y = 0; y < src.Height; ++y)
            {
                for (int x = 0; x < src.Width; ++x)
                {
                    int sx = (2 * cx) - x - 1, sy = (2 * cy) - y - 1;
                    if (sx >= 0 && sy >= 0 && sx < src.Width && sy < src.Height)
                    {
                        dst[x, y] = src[sx, sy];
                    }
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
        /// The image keeps its size - parts of the shadow outside are clipped.
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

            // the image size never changes (same size as the source, UO anchor bottom center stays):
            // shadow parts outside the canvas are clipped
            const int grow = 0, growTop = 0;
            var dst = new ArtPixelImage(w, h);
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
