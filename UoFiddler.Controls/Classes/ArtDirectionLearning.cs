using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UoFiddler.Controls.Classes
{
    /// <summary>What the advisor thinks the drawn item is.</summary>
    public sealed class ArtSuggestion
    {
        public ArtObjectKind Kind { get; set; }
        public ArtDirection Direction { get; set; }

        /// <summary>0..1</summary>
        public double Confidence { get; set; }

        public string Reason { get; set; }
    }

    /// <summary>Relation between two direction variants of one object in art.mul.</summary>
    public enum ArtVariantRelation
    {
        Copy,       // identical image (front = back, e.g. walls)
        Mirror,     // horizontally mirrored
        HandDrawn   // a real new drawing (the artist turned the object)
    }

    /// <summary>
    /// Compact description of an image for similarity search: size, mass, axis, 8x8 shape and brightness
    /// thumbnail and a 27-bin colour histogram. Mirror-tolerant comparison via <see cref="Distance"/>.
    /// </summary>
    public sealed class ArtFeatures
    {
        public const int Grid = 8;

        public int Id { get; set; } = -1;
        public int Width { get; set; }
        public int Height { get; set; }
        public int Count { get; set; }
        public double Axis { get; set; }
        public byte[] Mask { get; set; } = new byte[Grid * Grid];      // coverage 0..255 per cell
        public byte[] Luma { get; set; } = new byte[Grid * Grid];      // mean brightness per cell
        public byte[] Hist { get; set; } = new byte[27];               // colour histogram, sums to ~255

        public static ArtFeatures Compute(ArtPixelImage img, int id = -1)
        {
            var f = new ArtFeatures { Id = id, Width = img.Width, Height = img.Height };
            if (img.Width == 0 || img.Height == 0)
            {
                return f;
            }

            var cov = new int[Grid * Grid];
            var lum = new long[Grid * Grid];
            var area = new int[Grid * Grid];
            var hist = new int[27];
            for (int y = 0; y < img.Height; ++y)
            {
                int gy = Math.Min(Grid - 1, y * Grid / img.Height);
                for (int x = 0; x < img.Width; ++x)
                {
                    int gx = Math.Min(Grid - 1, x * Grid / img.Width);
                    int cell = (gy * Grid) + gx;
                    area[cell]++;
                    ushort c = img[x, y];
                    if (c == 0)
                    {
                        continue;
                    }

                    f.Count++;
                    cov[cell]++;
                    lum[cell] += ArtDirectionGenerator.Luma(c);
                    int r = ArtDirectionGenerator.R5(c) * 3 / 32, g = ArtDirectionGenerator.G5(c) * 3 / 32, b = ArtDirectionGenerator.B5(c) * 3 / 32;
                    hist[(r * 9) + (g * 3) + b]++;
                }
            }

            for (int i = 0; i < Grid * Grid; ++i)
            {
                f.Mask[i] = (byte)(area[i] == 0 ? 0 : cov[i] * 255 / area[i]);
                f.Luma[i] = (byte)(cov[i] == 0 ? 0 : lum[i] / cov[i]);
            }

            for (int i = 0; i < 27; ++i)
            {
                f.Hist[i] = (byte)(f.Count == 0 ? 0 : hist[i] * 255 / f.Count);
            }

            f.Axis = ArtDirectionGenerator.MeasureTopSlope(img);
            return f;
        }

        /// <summary>Distance 0 (equal) .. ~1+; <paramref name="mirrored"/> compares b mirrored left/right.</summary>
        public static double Distance(ArtFeatures a, ArtFeatures b, bool mirrored = false)
        {
            if (a.Count == 0 || b.Count == 0)
            {
                return 9;
            }

            double size = (Math.Abs(a.Width - b.Width) / (double)Math.Max(a.Width, b.Width)) +
                          (Math.Abs(a.Height - b.Height) / (double)Math.Max(a.Height, b.Height));
            double mask = 0, luma = 0;
            for (int y = 0; y < Grid; ++y)
            {
                for (int x = 0; x < Grid; ++x)
                {
                    int i = (y * Grid) + x;
                    int j = (y * Grid) + (mirrored ? Grid - 1 - x : x);
                    mask += Math.Abs(a.Mask[i] - b.Mask[j]);
                    luma += Math.Abs(a.Luma[i] - b.Luma[j]);
                }
            }

            double hist = 0;
            for (int i = 0; i < 27; ++i)
            {
                hist += Math.Abs(a.Hist[i] - b.Hist[i]);
            }

            mask /= 255.0 * Grid * Grid;
            luma /= 255.0 * Grid * Grid;
            hist /= 2 * 255.0;
            return (0.6 * size) + (1.4 * mask) + (0.5 * luma) + (1.0 * hist);
        }

        /// <summary>Distance that ignores which way the object faces (min of normal and mirrored).</summary>
        public static double DistanceAnyFacing(ArtFeatures a, ArtFeatures b)
        {
            return Math.Min(Distance(a, b), Distance(a, b, true));
        }

        /// <summary>0..100 similarity for display.</summary>
        public static int Similarity(double distance) => (int)Math.Round(Math.Max(0, 1 - (distance / 1.2)) * 100);

        #region Binary cache
        public void Write(BinaryWriter bw)
        {
            bw.Write(Id);
            bw.Write((short)Width);
            bw.Write((short)Height);
            bw.Write(Count);
            bw.Write((float)Axis);
            bw.Write(Mask);
            bw.Write(Luma);
            bw.Write(Hist);
        }

        public static ArtFeatures Read(BinaryReader br)
        {
            return new ArtFeatures
            {
                Id = br.ReadInt32(),
                Width = br.ReadInt16(),
                Height = br.ReadInt16(),
                Count = br.ReadInt32(),
                Axis = br.ReadSingle(),
                Mask = br.ReadBytes(Grid * Grid),
                Luma = br.ReadBytes(Grid * Grid),
                Hist = br.ReadBytes(27)
            };
        }
        #endregion
    }

    /// <summary>
    /// "Learning" for the direction generator:
    ///  1. <see cref="Suggest"/>: object type and drawn direction from TileData flags and the geometry,
    ///  2. <see cref="Relation"/>: how two existing variants of an object relate (copy / mirror / hand drawn),
    ///     used to learn the object type from groups that already exist in art.mul,
    ///  3. <see cref="ArtDirectionRuleStore"/>: the user's own corrections, applied again to similar items.
    /// </summary>
    public static class ArtDirectionLearning
    {
        /// <summary>
        /// Suggests object type and drawn direction.
        /// Flags (if the item comes from art.mul): Wall/Door/Window -> wall, Surface/height 0 -> flat.
        /// Geometry: the diagonal of the object axis ("\" or "/"); for walls "\" = South, "/" = East;
        /// for pointing objects the lighter half along the axis is the front (cannon muzzle, chair seat).
        /// </summary>
        public static ArtSuggestion Suggest(ArtPixelImage img, bool hasFlags, bool isWall, bool isDoor, bool isWindow,
            bool isSurface, bool isFoliage, int height, bool isContainer = false)
        {
            double k = ArtDirectionGenerator.MeasureTopSlope(img);
            bool backslash = k > 0.15;  // "\" (y grows with x)
            bool slash = k < -0.15;     // "/"
            var s = new ArtSuggestion { Confidence = 0.5 };
            var why = new List<string>();

            if (hasFlags && (isWall || isDoor || isWindow))
            {
                s.Kind = ArtObjectKind.Symmetric;
                s.Confidence = 0.9;
                why.Add(isDoor ? "TileData: door" : isWindow ? "TileData: window" : "TileData: wall");
            }
            else if (hasFlags && isContainer)
            {
                s.Kind = ArtObjectKind.Box;
                s.Confidence = 0.85;
                why.Add("TileData: container");
            }
            else if (hasFlags && (isSurface || height == 0) && !isFoliage && BoundsHeight(img) <= 48)
            {
                // really flat: lies inside its tile (rug, floor decal) - not a table or chair with the surface flag
                s.Kind = ArtObjectKind.Flat;
                s.Confidence = 0.75;
                why.Add(isSurface ? "TileData: surface, low" : "TileData: height 0");
            }
            else if (hasFlags && isSurface)
            {
                // tables, chairs, benches: something to sit / put things on, standing on its footprint
                s.Kind = ArtObjectKind.Box;
                s.Confidence = 0.7;
                why.Add("TileData: surface (furniture)");
            }
            else
            {
                GetBounds(img, out int minX, out int minY, out int maxX, out int maxY);
                int w = maxX - minX + 1, h = maxY - minY + 1;
                double fill = FillRatio(img);
                bool tallThin = w > 0 && h > w * 1.6 && (backslash || slash) && fill > 0.6;
                bool big = w > 110 || h > 130;
                bool lowLong = w > 0 && h <= w * 1.3;
                if (tallThin)
                {
                    // narrow upright slab on a diagonal: looks like a wall piece
                    s.Kind = ArtObjectKind.Symmetric;
                    s.Confidence = 0.6;
                    why.Add("shape: upright slab on a diagonal");
                }
                else if (big)
                {
                    // multi-tile objects (wagon, ship parts): turning them by pixels gives no usable result
                    s.Kind = ArtObjectKind.Box;
                    s.Confidence = 0.5;
                    why.Add("shape: big object (several tiles)");
                }
                else if ((backslash || slash) && fill < 0.5 && lowLong)
                {
                    // low, elongated, open shape (cannon): points somewhere
                    s.Kind = ArtObjectKind.Directional;
                    s.Confidence = hasFlags ? 0.65 : 0.55;
                    why.Add("shape: low slim object along a diagonal");
                }
                else if (backslash || slash)
                {
                    // block or furniture (chest, crate, table, chair): stands on its footprint
                    s.Kind = ArtObjectKind.Box;
                    s.Confidence = 0.55;
                    why.Add(fill >= 0.5 ? "shape: compact block on a diagonal" : "shape: furniture on a diagonal");
                }
                else
                {
                    s.Kind = ArtObjectKind.Solid;
                    s.Confidence = 0.4;
                    why.Add("shape: no clear axis");
                }
            }

            // direction
            if (s.Kind == ArtObjectKind.Directional)
            {
                bool frontFirst = LighterHalfIsFirst(img); // first = left part of the image
                if (backslash)
                {
                    s.Direction = frontFirst ? ArtDirection.West : ArtDirection.East;
                }
                else
                {
                    s.Direction = frontFirst ? ArtDirection.South : ArtDirection.North;
                }

                why.Add($"axis {(backslash ? "\\" : "/")}, front {(frontFirst ? "left" : "right")} -> {s.Direction}");
            }
            else if (backslash || slash)
            {
                s.Direction = backslash ? ArtDirection.South : ArtDirection.East;
                why.Add($"axis {(backslash ? "\\" : "/")} -> {s.Direction}");
            }
            else
            {
                s.Direction = ArtDirection.South;
            }

            s.Reason = string.Join(", ", why);
            return s;
        }

        /// <summary>True if the left half of the object (along its width) has less pixels than the right half.</summary>
        private static bool LighterHalfIsFirst(ArtPixelImage img)
        {
            GetBounds(img, out int minX, out _, out int maxX, out _);
            double mid = (minX + maxX) / 2.0;
            int left = 0, right = 0;
            for (int y = 0; y < img.Height; ++y)
            {
                for (int x = minX; x <= maxX; ++x)
                {
                    if (!img.IsOpaque(x, y))
                    {
                        continue;
                    }

                    if (x < mid)
                    {
                        ++left;
                    }
                    else if (x > mid)
                    {
                        ++right;
                    }
                }
            }

            return left < right;
        }

        private static int BoundsHeight(ArtPixelImage img)
        {
            GetBounds(img, out _, out int minY, out _, out int maxY);
            return maxY < minY ? 0 : maxY - minY + 1;
        }

        private static double FillRatio(ArtPixelImage img)
        {
            GetBounds(img, out int minX, out int minY, out int maxX, out int maxY);
            if (maxX < minX)
            {
                return 0;
            }

            int n = img.Pixels.Count(p => p != 0);
            return n / (double)((maxX - minX + 1) * (maxY - minY + 1));
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

        /// <summary>
        /// How <paramref name="b"/> relates to <paramref name="a"/>: identical, mirrored or a new drawing.
        /// Compares the pixels (allowing a few pixels of offset / small differences).
        /// </summary>
        public static ArtVariantRelation Relation(ArtPixelImage a, ArtPixelImage b, out double score)
        {
            double same = PixelMatch(a, b, false);
            double mirror = PixelMatch(a, b, true);
            score = Math.Max(same, mirror);
            if (same >= 0.85 && same >= mirror)
            {
                return ArtVariantRelation.Copy;
            }

            return mirror >= 0.8 ? ArtVariantRelation.Mirror : ArtVariantRelation.HandDrawn;
        }

        /// <summary>Share of matching opaque pixels (bottom-centre aligned, best of small x/y offsets).</summary>
        private static double PixelMatch(ArtPixelImage a, ArtPixelImage b, bool mirror)
        {
            int na = a.Pixels.Count(p => p != 0), nb = b.Pixels.Count(p => p != 0);
            if (na == 0 || nb == 0)
            {
                return 0;
            }

            double best = 0;
            int baseDx = (b.Width - a.Width) / 2, baseDy = b.Height - a.Height;
            for (int oy = -2; oy <= 2; ++oy)
            {
                for (int ox = -2; ox <= 2; ++ox)
                {
                    int hit = 0;
                    for (int y = 0; y < a.Height; ++y)
                    {
                        for (int x = 0; x < a.Width; ++x)
                        {
                            ushort ca = a[x, y];
                            if (ca == 0)
                            {
                                continue;
                            }

                            int ax = mirror ? a.Width - 1 - x : x;
                            int bx = ax + baseDx + ox, by = y + baseDy + oy;
                            if (bx < 0 || by < 0 || bx >= b.Width || by >= b.Height)
                            {
                                continue;
                            }

                            ushort cb = b[bx, by];
                            if (cb != 0 && Math.Abs(ArtDirectionGenerator.Luma(ca) - ArtDirectionGenerator.Luma(cb)) <= 40)
                            {
                                ++hit;
                            }
                        }
                    }

                    best = Math.Max(best, hit / (double)Math.Max(na, nb));
                }
            }

            return best;
        }

        /// <summary>
        /// Object type learned from an existing group of variants: copies and mirrors only -> the object looks the
        /// same from both sides: a wall/door piece (on the tile edge) if <paramref name="wallLike"/>, otherwise a
        /// box / furniture (same footprint); at least one real new drawing -> pointing object (directional).
        /// </summary>
        public static ArtObjectKind KindFromRelations(IEnumerable<ArtVariantRelation> relations, bool wallLike = false)
        {
            var list = relations.ToList();
            if (list.Count == 0)
            {
                return ArtObjectKind.Directional;
            }

            if (list.Any(r => r == ArtVariantRelation.HandDrawn))
            {
                return ArtObjectKind.Directional;
            }

            return wallLike ? ArtObjectKind.Symmetric : ArtObjectKind.Box;
        }

        /// <summary>
        /// Learns how strong the face light has to be swapped: <paramref name="mirrored"/> is a real, hand made
        /// mirror variant of <paramref name="source"/> (e.g. the East chest of a South chest in art.mul). The mirror
        /// is generated with 0, 25, 50, 75 and 100 % light swap; the strength whose result is closest to the real
        /// variant (mean brightness difference on common pixels) wins.
        /// </summary>
        /// <returns>Strength 0..1, or -1 if the two images can not be compared.</returns>
        public static double LearnRelightStrength(ArtPixelImage source, ArtPixelImage mirrored, ArtObjectKind kind, out double bestDiff)
        {
            bestDiff = double.MaxValue;
            double best = -1;
            if (source == null || mirrored == null || source.Width != mirrored.Width || source.Height != mirrored.Height)
            {
                return best;
            }

            foreach (double strength in new[] { 0.0, 0.25, 0.5, 0.75, 1.0 })
            {
                var o = new ArtDirectionOptions
                {
                    Kind = kind,
                    Shadow = ArtShadowMode.KeepAsIs,
                    Repair = false,
                    Relight = strength > 0,
                    RelightStrength = strength
                };
                ArtPixelImage gen = ArtDirectionGenerator.Generate(source, ArtDirection.South, ArtDirection.East, o).Image;
                if (gen.Width != mirrored.Width || gen.Height != mirrored.Height)
                {
                    continue;
                }

                double d = 0;
                int n = 0;
                for (int i = 0; i < gen.Pixels.Length; ++i)
                {
                    if (gen.Pixels[i] != 0 && mirrored.Pixels[i] != 0)
                    {
                        d += Math.Abs(ArtDirectionGenerator.Luma(gen.Pixels[i]) - ArtDirectionGenerator.Luma(mirrored.Pixels[i]));
                        ++n;
                    }
                }

                if (n == 0)
                {
                    continue;
                }

                d /= n;
                if (d < bestDiff - 0.05)
                {
                    bestDiff = d;
                    best = strength;
                }
            }

            return best;
        }

        /// <summary>Decodes raw static art (art.mul / artext.mul encoding) without GDI.</summary>
        public static ArtPixelImage DecodeStaticRaw(byte[] data)
        {
            if (data == null || data.Length < 8)
            {
                return null;
            }

            int Word(int index) => index * 2 + 1 < data.Length ? data[index * 2] | (data[(index * 2) + 1] << 8) : 0;

            int width = Word(2), height = Word(3);
            if (width <= 0 || height <= 0 || width > 4096 || height > 4096 || data.Length < 8 + (height * 2))
            {
                return null;
            }

            var img = new ArtPixelImage(width, height);
            int start = height + 4;
            int words = data.Length / 2;
            for (int y = 0; y < height; ++y)
            {
                int pos = start + Word(4 + y);
                int x = 0;
                while (pos + 1 < words)
                {
                    int xOffset = Word(pos++), run = Word(pos++);
                    if (xOffset + run == 0)
                    {
                        break;
                    }

                    x += xOffset;
                    for (int i = 0; i < run && pos < words; ++i, ++x)
                    {
                        ushort c = (ushort)(Word(pos++) ^ 0x8000);
                        if (x >= 0 && x < width)
                        {
                            img[x, y] = (c & 0x7FFF) == 0 ? (ushort)0x8000 : c;
                        }
                    }
                }
            }

            return img;
        }
    }

    #region Learned rules (the user's corrections)
    public sealed class ArtDirectionRule
    {
        public string Name { get; set; } = string.Empty;
        public int SourceId { get; set; } = -1;
        public DateTime Saved { get; set; }
        public int Uses { get; set; }

        public int Kind { get; set; }
        public int Direction { get; set; }

        /// <summary>Light swap strength 0..100 (-1 = not stored, older rules).</summary>
        public int RelightStrength { get; set; } = -1;

        // per target direction (index = ArtDirection): manual method, axis, offset
        public int[] Method { get; set; } = new int[4];
        public double[] Axis { get; set; } = new double[4];
        public int[] Dx { get; set; } = new int[4];
        public int[] Dy { get; set; } = new int[4];

        // signature
        public int Width { get; set; }
        public int Height { get; set; }
        public int Count { get; set; }
        public double AxisSlope { get; set; }
        public byte[] Mask { get; set; }
        public byte[] Luma { get; set; }
        public byte[] Hist { get; set; }

        public ArtFeatures ToFeatures() => new ArtFeatures
        {
            Width = Width,
            Height = Height,
            Count = Count,
            Axis = AxisSlope,
            Mask = Mask ?? new byte[64],
            Luma = Luma ?? new byte[64],
            Hist = Hist ?? new byte[27]
        };

        public void SetFeatures(ArtFeatures f)
        {
            Width = f.Width;
            Height = f.Height;
            Count = f.Count;
            AxisSlope = f.Axis;
            Mask = f.Mask;
            Luma = f.Luma;
            Hist = f.Hist;
        }
    }

    /// <summary>The user's corrections, stored as JSON; the closest rule is applied to similar items.</summary>
    public sealed class ArtDirectionRuleStore
    {
        public const double MatchDistance = 0.35;

        private readonly string _path;
        public List<ArtDirectionRule> Rules { get; private set; } = new List<ArtDirectionRule>();

        public ArtDirectionRuleStore(string path)
        {
            _path = path;
            Load();
        }

        public void Load()
        {
            try
            {
                if (_path != null && File.Exists(_path))
                {
                    Rules = JsonSerializer.Deserialize<List<ArtDirectionRule>>(File.ReadAllText(_path)) ?? new List<ArtDirectionRule>();
                }
            }
            catch (Exception)
            {
                Rules = new List<ArtDirectionRule>(); // damaged file: start fresh, never block the generator
            }
        }

        public void Save()
        {
            if (_path == null)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            File.WriteAllText(_path, JsonSerializer.Serialize(Rules, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>Best matching rule (or null) and its distance. A rule for the same item id always wins.</summary>
        public ArtDirectionRule Match(ArtFeatures f, int sourceId, out double distance)
        {
            distance = double.MaxValue;
            ArtDirectionRule best = null;
            foreach (ArtDirectionRule r in Rules)
            {
                double d = sourceId >= 0 && r.SourceId == sourceId ? 0 : ArtFeatures.Distance(f, r.ToFeatures());
                if (d < distance)
                {
                    distance = d;
                    best = r;
                }
            }

            return distance <= MatchDistance ? best : null;
        }

        /// <summary>Adds the rule, or replaces a nearly identical one (same item or almost the same look).</summary>
        public void Remember(ArtDirectionRule rule)
        {
            ArtFeatures f = rule.ToFeatures();
            Rules.RemoveAll(r => (rule.SourceId >= 0 && r.SourceId == rule.SourceId) || ArtFeatures.Distance(f, r.ToFeatures()) < 0.05);
            rule.Saved = DateTime.Now;
            Rules.Add(rule);
            Save();
        }
    }
    #endregion
}
