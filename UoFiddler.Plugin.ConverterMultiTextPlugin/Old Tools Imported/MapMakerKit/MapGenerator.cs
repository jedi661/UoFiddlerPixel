// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>Input for <see cref="MapGenerator"/>.</summary>
    internal sealed class GeneratorInput
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public GeneratorSettings Settings { get; set; }

        /// <summary>
        /// Optional sketch (terrain IDs, 0 = "No Draw" = let the generator decide).
        /// Can have any size, it is scaled to the map.
        /// </summary>
        public byte[] Sketch { get; set; }
        public int SketchWidth { get; set; }
        public int SketchHeight { get; set; }

        /// <summary>Optional grey scale height template (0..255), any size.</summary>
        public byte[] HeightTemplate { get; set; }
        public int TemplateWidth { get; set; }
        public int TemplateHeight { get; set; }

        /// <summary>1 = full resolution. 2, 4, 8 ... = fast preview (one cell = step x step tiles).</summary>
        public int Step { get; set; } = 1;
    }

    /// <summary>
    /// Random world generator: continents and islands surrounded by water, beaches, mountains
    /// with snow, biome patches (forest, jungle, ...), hills and a dungeon area with caves.
    /// The result is a <see cref="MapDocument"/> (= Terrain.bmp + Altitude.bmp).
    /// </summary>
    internal sealed class MapGenerator
    {
        private const byte KindNone = 0;
        private const byte KindLand = 1;
        private const byte KindWater = 2;
        private const byte KindMountain = 3;

        private readonly TerrainPalette _terrain;
        private readonly AltitudePalette _altitude;

        public MapGenerator(TerrainPalette terrain, AltitudePalette altitude)
        {
            _terrain = terrain;
            _altitude = altitude;
        }

        public MapDocument Generate(GeneratorInput input, IProgress<int> progress, Action<string> log, CancellationToken ct)
        {
            GeneratorSettings s = input.Settings ?? GeneratorSettings.CreateDefault();
            int step = Math.Max(1, input.Step);
            int tilesW = input.Width;
            int tilesH = input.Height;
            int gw = (tilesW + step - 1) / step;
            int gh = (tilesH + step - 1) / step;
            int n = gw * gh;

            Action<string> rawLog = log ?? (_ => { });
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            log = message => rawLog(string.Format("[{0,5:0.0}s] {1}", watch.Elapsed.TotalSeconds, message));
            progress?.Report(0);

            // ------------------------------------------------ areas
            bool hasDungeonArea = s.DungeonStartX > 0 && s.DungeonStartX <= tilesW - 64;
            int worldTilesW = hasDungeonArea ? s.DungeonStartX : tilesW;
            int rw = Math.Min(gw, (worldTilesW + step - 1) / step); // overworld width in cells

            ValidateTerrain(s, log);

            MapDocument doc = new MapDocument(gw, gh);
            byte[] terrain = doc.Terrain;
            byte[] alt = doc.Altitude;
            ParallelOptions po = new ParallelOptions { CancellationToken = ct };

            Random rng = new Random(s.Seed);
            GradientNoise warpX = new GradientNoise(s.Seed + 1);
            GradientNoise warpY = new GradientNoise(s.Seed + 2);
            GradientNoise detail = new GradientNoise(s.Seed + 3);
            GradientNoise fine = new GradientNoise(s.Seed + 4);
            GradientNoise mountainNoise = new GradientNoise(s.Seed + 5);
            GradientNoise hillNoise = new GradientNoise(s.Seed + 6);
            GradientNoise beachNoise = new GradientNoise(s.Seed + 7);

            double rough = Math.Clamp(s.CoastRoughness / 100.0, 0, 1);

            // ------------------------------------------------ 1. height field
            log("Generator: height field ...");
            float[] h = new float[n];
            double landFraction = Math.Clamp(s.LandPercent / 100.0, 0.01, 0.95);
            double worldArea = (double)worldTilesW * tilesH;
            int continents = Math.Max(0, s.Continents);
            int islands = Math.Max(0, s.Islands);
            double border = s.SurroundedByWater ? Math.Max(0, s.BorderWidth) : 0;

            List<(double X, double Y, double R)> blobs = new List<(double, double, double)>();
            double baseRadius = continents > 0
                ? Math.Sqrt(worldArea * landFraction * 0.85 / continents / Math.PI)
                : Math.Sqrt(worldArea * landFraction / Math.Max(1, islands) / Math.PI);
            baseRadius = Math.Max(16, baseRadius);
            for (int i = 0; i < continents; i++)
            {
                // a continent = several overlapping blobs -> irregular instead of round shapes
                double r = baseRadius * (0.75 + (0.5 * rng.NextDouble()));
                double margin = Math.Min(border + (r * 0.9), Math.Min(worldTilesW, tilesH) * 0.45);
                double cx = Lerp(margin, worldTilesW - margin, rng.NextDouble());
                double cy = Lerp(margin, tilesH - margin, rng.NextDouble());
                blobs.Add((cx, cy, r * 0.8));
                int parts = 3 + rng.Next(4);
                for (int k = 0; k < parts; k++)
                {
                    double angle = rng.NextDouble() * Math.PI * 2;
                    double distance = r * (0.35 + (0.4 * rng.NextDouble()));
                    double px = Math.Clamp(cx + (Math.Cos(angle) * distance), margin * 0.8, worldTilesW - (margin * 0.8));
                    double py = Math.Clamp(cy + (Math.Sin(angle) * distance), margin * 0.8, tilesH - (margin * 0.8));
                    blobs.Add((px, py, r * (0.4 + (0.3 * rng.NextDouble()))));
                }
            }

            for (int i = 0; i < islands; i++)
            {
                double r = baseRadius * (continents > 0 ? 0.09 : 0.5) * (0.5 + rng.NextDouble());
                double margin = Math.Min(border + (r * 1.2), Math.Min(worldTilesW, tilesH) * 0.45);
                blobs.Add((Lerp(margin, worldTilesW - margin, rng.NextDouble()), Lerp(margin, tilesH - margin, rng.NextDouble()), Math.Max(6, r)));
            }

            (double X, double Y, double R)[] blobArray = blobs.ToArray();
            double warpScale = Math.Max(24, baseRadius * 0.8);
            double warpAmp = baseRadius * 0.45 * (0.3 + rough);
            double detailScale = Math.Max(20, baseRadius * 0.6);
            bool useTemplate = input.HeightTemplate != null && input.TemplateWidth > 0 && input.TemplateHeight > 0;

            // The large shapes (continent mask, warp, detail) are smooth: compute them on a coarse grid
            // (every 4th tile) and interpolate. Only the fine coast detail is computed per tile.
            int coarse = Math.Max(1, 4 / step);
            int cw = (rw / coarse) + 2;
            int ch = (gh / coarse) + 2;
            float[] large = new float[cw * ch];
            if (!useTemplate)
            {
                Parallel.For(0, ch, po, cy =>
                {
                    double ty = cy * coarse * step;
                    for (int cx = 0; cx < cw; cx++)
                    {
                        double tx = cx * coarse * step;
                        double value;
                        if (blobArray.Length == 0)
                        {
                            value = detail.Fbm(tx / detailScale, ty / detailScale, 5) * 0.8;
                        }
                        else
                        {
                            double wx = tx + (warpAmp * warpX.Fbm(tx / warpScale, ty / warpScale, 3));
                            double wy = ty + (warpAmp * warpY.Fbm(tx / warpScale, ty / warpScale, 3));
                            double mask = -1;
                            for (int b = 0; b < blobArray.Length; b++)
                            {
                                double dx = wx - blobArray[b].X;
                                double dy = wy - blobArray[b].Y;
                                double m = 1.0 - (Math.Sqrt((dx * dx) + (dy * dy)) / blobArray[b].R);
                                if (m > mask)
                                {
                                    mask = m;
                                }
                            }

                            value = mask + (detail.Fbm(tx / detailScale, ty / detailScale, 5) * 0.42);
                        }

                        large[(cy * cw) + cx] = (float)value;
                    }
                });
            }

            Parallel.For(0, gh, po, gy =>
            {
                double ty = gy * step;
                int cy0 = gy / coarse;
                float fy = (gy - (cy0 * coarse)) / (float)coarse;
                for (int gx = 0; gx < rw; gx++)
                {
                    double tx = gx * step;
                    double value;
                    double fineValue = fine.Fbm(tx / 40.0, ty / 40.0, 3);
                    if (useTemplate)
                    {
                        double grey = SampleBilinear(input.HeightTemplate, input.TemplateWidth, input.TemplateHeight, tx / worldTilesW, ty / tilesH);
                        value = ((grey - s.TemplateSeaLevel) / 255.0) + (fineValue * 0.03 * rough);
                    }
                    else
                    {
                        int cx0 = gx / coarse;
                        float fx = (gx - (cx0 * coarse)) / (float)coarse;
                        int c00 = (cy0 * cw) + cx0;
                        float top = large[c00] + ((large[c00 + 1] - large[c00]) * fx);
                        float bottom = large[c00 + cw] + ((large[c00 + cw + 1] - large[c00 + cw]) * fx);
                        value = top + ((bottom - top) * fy);
                        value += blobArray.Length == 0 ? fineValue * 0.15 * rough : fineValue * 0.08 * (0.3 + rough);
                    }

                    if (border > 0)
                    {
                        // wavy border so land is not cut along a straight line
                        double e = Math.Min(Math.Min(tx, ty), Math.Min(worldTilesW - 1 - tx, tilesH - 1 - ty));
                        e += (warpX.Fbm(tx / (border * 2.5), ty / (border * 2.5), 2) * border * 0.5) + (fineValue * border * 0.15);
                        double f = MathUtil.SmoothStep(border * 0.2, border * 1.4, e);
                        value = Lerp(-1.5, value, f) - ((1 - f) * 0.3);
                    }

                    h[(gy * gw) + gx] = (float)value;
                }
            });
            progress?.Report(10);

            float seaLevel = useTemplate
                ? 0f
                : MathUtil.ThresholdForTopFraction(h, i => (i % gw) < rw, landFraction, n);

            byte[] kind = new byte[n]; // per cell: land / water / mountain
            for (int i = 0; i < n; i++)
            {
                if ((i % gw) < rw)
                {
                    kind[i] = h[i] > seaLevel ? KindLand : KindWater;
                }
            }

            // ------------------------------------------------ 2. sketch
            byte[] forced = null;
            bool[] painted = null;
            if (input.Sketch != null && input.SketchWidth > 0 && input.SketchHeight > 0)
            {
                log("Generator: applying sketch ...");
                painted = new bool[n];
                forced = ApplySketch(input, s, kind, painted, gw, gh, rw, step, rough, warpX, warpY, po);
            }

            progress?.Report(15);
            ct.ThrowIfCancellationRequested();

            // ------------------------------------------------ 3. remove tiny islands / lakes
            log("Generator: remove tiny islands / lakes ...");
            int[] queue = new int[n];
            bool[] visited = new bool[n];
            int minIsland = Math.Max(0, s.MinIslandSize / (step * step));
            int minLake = Math.Max(0, s.MinLakeSize / (step * step));
            CleanComponents(kind, painted, gw, gh, rw, minIsland, minLake, queue, visited);
            progress?.Report(25);
            ct.ThrowIfCancellationRequested();

            // ------------------------------------------------ 4. distances
            log("Generator: distances ...");
            ushort[] distWater = new ushort[n]; // land cells: distance to water
            ushort[] distOther = new ushort[n]; // reused for several distance fields
            Bfs(gw, gh, rw, i => kind[i] == KindWater, i => kind[i] != KindWater, distWater, queue);
            Bfs(gw, gh, rw, i => kind[i] != KindWater, i => kind[i] == KindWater, distOther, queue);
            progress?.Report(35);

            // ------------------------------------------------ 5. water
            int shallowCells = Math.Max(1, (int)Math.Ceiling(Math.Max(2, s.ShallowWidth) / (double)step));
            byte shallow = (byte)s.ShallowWaterTerrain;
            byte deep = (byte)s.DeepWaterTerrain;
            for (int i = 0; i < n; i++)
            {
                if ((i % gw) >= rw || kind[i] != KindWater)
                {
                    continue;
                }

                byte t;
                if (forced != null && forced[i] != 0 && _terrain.IsWater(forced[i], _altitude))
                {
                    t = forced[i];
                }
                else
                {
                    t = distOther[i] <= shallowCells ? shallow : deep;
                }

                terrain[i] = t;
                alt[i] = _altitude.BaseKeyFor(_terrain, t);
            }

            // ------------------------------------------------ 6. mountains
            log("Generator: mountains ...");
            long landCount = 0;
            for (int i = 0; i < n; i++)
            {
                if (kind[i] == KindLand)
                {
                    landCount++;
                }
            }

            int beachCells = s.BeachWidth <= 0 ? 0 : Math.Max(1, (int)Math.Round(s.BeachWidth / (double)step));
            GradientNoise massNoise = new GradientNoise(s.Seed + 8);
            double mountainScale = Math.Max(16, s.MountainSize);
            float[] score = h; // reuse buffer
            Parallel.For(0, gh, po, gy =>
            {
                for (int gx = 0; gx < rw; gx++)
                {
                    int i = (gy * gw) + gx;
                    if (kind[i] != KindLand)
                    {
                        score[i] = float.NegativeInfinity;
                        continue;
                    }

                    double tx = gx * step;
                    double ty = gy * step;
                    if (forced != null && forced[i] == s.MountainTerrain)
                    {
                        // painted mountain: always a mountain, height varies with the ridge noise (5..6)
                        score[i] = (float)(5 + mountainNoise.Ridged(tx / mountainScale, ty / mountainScale, 5));
                        continue;
                    }

                    if ((forced != null && forced[i] != 0) || distWater[i] <= beachCells + 1)
                    {
                        score[i] = float.NegativeInfinity;
                        continue;
                    }

                    double r = mountainNoise.Ridged(tx / mountainScale, ty / mountainScale, 5);

                    // large scale mask: mountains gather in ranges instead of thin veins everywhere
                    double mass = massNoise.Fbm(tx / (mountainScale * 3), ty / (mountainScale * 3), 3);
                    r *= 0.25 + (0.75 * MathUtil.SmoothStep(-0.15, 0.45, mass));
                    r *= 0.6 + (0.4 * MathUtil.SmoothStep(0, 40, distWater[i] * step));
                    score[i] = (float)r;
                }
            });

            double mountainFraction = landCount == 0 ? 0 : Math.Clamp(s.MountainPercent / 100.0, 0, 0.9);
            long eligible = 0;
            for (int i = 0; i < n; i++)
            {
                if (!float.IsNegativeInfinity(score[i]) && (i % gw) < rw)
                {
                    eligible++;
                }
            }

            double fractionOfEligible = eligible == 0 ? 0 : Math.Min(1.0, mountainFraction * landCount / eligible);
            float mountainThreshold = MathUtil.ThresholdForTopFraction(score, i => (i % gw) < rw && !float.IsNegativeInfinity(score[i]), fractionOfEligible, n);
            float maxScore = 0;
            for (int i = 0; i < n; i++)
            {
                if (kind[i] == KindLand && !float.IsNegativeInfinity(score[i]) && (score[i] >= mountainThreshold || score[i] >= 5f))
                {
                    kind[i] = KindMountain;
                    if (score[i] < 5f && score[i] > maxScore)
                    {
                        maxScore = score[i];
                    }
                }
            }

            // distance from the mountain foot -> slopes instead of cliffs
            Bfs(gw, gh, rw, i => kind[i] != KindMountain, i => kind[i] == KindMountain, distOther, queue);
            byte mountainTerrain = (byte)s.MountainTerrain;
            int minH = Math.Min(s.MountainMinHeight, s.MountainMaxHeight);
            int maxH = Math.Max(s.MountainMinHeight, s.MountainMaxHeight);
            double range = Math.Max(1e-6, maxScore - mountainThreshold);
            int slope = Math.Max(1, s.MountainSlope);
            for (int i = 0; i < n; i++)
            {
                if (kind[i] != KindMountain)
                {
                    continue;
                }

                double t = score[i] >= 5f ? Math.Clamp(0.25 + (0.75 * (score[i] - 5f)), 0, 1) : Math.Clamp((score[i] - mountainThreshold) / range, 0, 1);
                int height = (int)Math.Round(minH + (t * (maxH - minH)));
                height = Math.Min(height, distOther[i] * step * slope);
                height = Math.Max(5, MathUtil.RoundTo(height, 5));
                bool snow = s.SnowOnPeaks && height >= s.SnowLine;
                terrain[i] = snow ? (byte)s.SnowTerrain : mountainTerrain;
                alt[i] = _altitude.Encode(AltitudeType.Mountain, height);
            }

            // distance to the next mountain (for the "NearMountains" biome placement)
            Bfs(gw, gh, rw, i => kind[i] == KindMountain, i => kind[i] != KindMountain, distOther, queue);
            progress?.Report(50);
            ct.ThrowIfCancellationRequested();

            // ------------------------------------------------ 7. beaches
            log("Generator: beaches ...");
            // assigned[] marks land cells that already have their final terrain
            bool[] assigned = visited;
            Array.Clear(assigned, 0, assigned.Length);
            if (beachCells > 0 && s.BeachCoverage > 0 && _terrain.Defined[s.BeachTerrain])
            {
                Parallel.For(0, gh, po, gy =>
                {
                    for (int gx = 0; gx < rw; gx++)
                    {
                        int i = (gy * gw) + gx;
                        score[i] = kind[i] == KindLand && distWater[i] <= beachCells && !(forced != null && forced[i] != 0 && forced[i] != s.MountainTerrain)
                            ? (float)beachNoise.Fbm(gx * step / 180.0, gy * step / 180.0, 2)
                            : float.NegativeInfinity;
                    }
                });

                float beachThreshold = MathUtil.ThresholdForTopFraction(score, i => !float.IsNegativeInfinity(score[i]), s.BeachCoverage / 100.0, n);
                byte beach = (byte)s.BeachTerrain;
                for (int i = 0; i < n; i++)
                {
                    if (!float.IsNegativeInfinity(score[i]) && score[i] >= beachThreshold)
                    {
                        terrain[i] = beach;
                        alt[i] = _altitude.Encode(AltitudeType.Land, 0);
                        assigned[i] = true;
                    }
                }
            }

            // forced (painted) land terrain
            if (forced != null)
            {
                for (int i = 0; i < n; i++)
                {
                    if (kind[i] == KindLand && forced[i] != 0 && forced[i] != s.MountainTerrain && !assigned[i])
                    {
                        terrain[i] = forced[i];
                        assigned[i] = true;
                    }
                }
            }

            progress?.Report(55);

            // ------------------------------------------------ 8. biomes
            log("Generator: biomes ...");
            byte baseTerrain = (byte)s.BaseTerrain;
            bool[] isBiome = new bool[256];
            bool[] fromRule = new bool[n]; // placed by a biome rule (only those are separated)
            List<BiomeRule> rules = (s.Biomes ?? new List<BiomeRule>()).Where(r => r.Enabled && r.Coverage > 0 && r.TerrainId > 0 && r.TerrainId < 256 && _terrain.Defined[r.TerrainId]).ToList();
            int ruleIndex = 0;
            foreach (BiomeRule rule in rules)
            {
                ct.ThrowIfCancellationRequested();
                GradientNoise noise = new GradientNoise(s.Seed + 100 + (ruleIndex * 7));
                double patch = Math.Max(8, rule.PatchSize);
                BiomePlacement placement = rule.Placement;
                Parallel.For(0, gh, po, gy =>
                {
                    for (int gx = 0; gx < rw; gx++)
                    {
                        int i = (gy * gw) + gx;
                        if (kind[i] != KindLand || assigned[i])
                        {
                            score[i] = float.NegativeInfinity;
                            continue;
                        }

                        double tx = gx * step;
                        double ty = gy * step;
                        double v = noise.Fbm(tx / patch, ty / patch, 4);
                        v += PlacementBias(placement, tx, ty, worldTilesW, tilesH, distWater[i] * step, distOther[i] * step);
                        score[i] = (float)v;
                    }
                });

                long free = 0;
                for (int i = 0; i < n; i++)
                {
                    if (!float.IsNegativeInfinity(score[i]))
                    {
                        free++;
                    }
                }

                double wanted = rule.Coverage / 100.0 * landCount;
                double fraction = free == 0 ? 0 : Math.Min(1.0, wanted / free);
                float threshold = MathUtil.ThresholdForTopFraction(score, i => !float.IsNegativeInfinity(score[i]), fraction, n);
                byte t = (byte)rule.TerrainId;
                isBiome[t] = true;
                long placed = 0;
                for (int i = 0; i < n; i++)
                {
                    if (!float.IsNegativeInfinity(score[i]) && score[i] >= threshold)
                    {
                        terrain[i] = t;
                        assigned[i] = true;
                        fromRule[i] = true;
                        placed++;
                    }
                }

                log(string.Format("  {0}: {1:0.0}% of the land", _terrain.DisplayName(t), landCount == 0 ? 0 : placed * 100.0 / landCount));
                ruleIndex++;
                progress?.Report(55 + (20 * ruleIndex / Math.Max(1, rules.Count)));
            }

            for (int i = 0; i < n; i++)
            {
                if (kind[i] == KindLand && !assigned[i])
                {
                    terrain[i] = baseTerrain;
                }
            }

            // ------------------------------------------------ 9. separate biomes
            log("Generator: separate biomes ...");
            if (s.SeparateBiomes && s.SeparationWidth > 0 && rules.Count > 1)
            {
                int iterations = Math.Max(1, (int)Math.Round(s.SeparationWidth / (2.0 * step)));
                SeparateBiomes(terrain, fromRule, baseTerrain, gw, gh, rw, iterations);
            }

            progress?.Report(80);

            // ------------------------------------------------ 10. hills
            log("Generator: hills ...");
            int hillHeight = Math.Max(0, s.HillHeight);
            double hillScale = Math.Max(8, s.HillSize);
            byte flatLand = _altitude.Encode(AltitudeType.Land, 0);
            Parallel.For(0, gh, po, gy =>
            {
                for (int gx = 0; gx < rw; gx++)
                {
                    int i = (gy * gw) + gx;
                    if (kind[i] != KindLand)
                    {
                        continue;
                    }

                    byte t = terrain[i];
                    if (_terrain.IsWater(t, _altitude))
                    {
                        alt[i] = _altitude.BaseKeyFor(_terrain, t);
                        continue;
                    }

                    if (hillHeight == 0 || t == s.BeachTerrain || t == TerrainIds.Swamp)
                    {
                        alt[i] = flatLand;
                        continue;
                    }

                    double v = Math.Max(0, hillNoise.Fbm(gx * step / hillScale, gy * step / hillScale, 3)) * 1.6;
                    v *= MathUtil.SmoothStep(3, 25, distWater[i] * step);
                    int height = Math.Min(hillHeight, MathUtil.RoundTo((int)Math.Round(v * hillHeight), 5));
                    alt[i] = _altitude.Encode(AltitudeType.Land, height);
                }
            });
            progress?.Report(88);

            // ------------------------------------------------ 11. dungeon area
            if (hasDungeonArea && rw < gw)
            {
                log("Generator: dungeon area ...");
                byte fill = _terrain.Defined[s.DungeonFillTerrain] ? (byte)s.DungeonFillTerrain : TerrainIds.Blank;
                byte fillKey = _altitude.Encode(AltitudeType.Land, -80);
                for (int y = 0; y < gh; y++)
                {
                    for (int x = rw; x < gw; x++)
                    {
                        int i = (y * gw) + x;
                        terrain[i] = fill;
                        alt[i] = fillKey;
                    }
                }

                if (s.DungeonEnabled)
                {
                    DungeonGenerator dungeon = new DungeonGenerator(_terrain, _altitude);
                    int caves = dungeon.Carve(doc, rw, 0, gw - rw, gh, s, step, new Random(s.Seed + 999), ct);
                    log(string.Format("  {0} caves created.", caves));
                }
            }

            progress?.Report(100);
            log(string.Format("Generator: done. Land {0:0.0}% of the overworld.", rw * gh == 0 ? 0 : (landCount * 100.0) / (rw * gh)));
            return doc;
        }

        // ================================================================ helpers

        private void ValidateTerrain(GeneratorSettings s, Action<string> log)
        {
            void Check(string name, int id)
            {
                if (id < 0 || id > 255 || !_terrain.Defined[id])
                {
                    log(string.Format("Warning: {0} uses terrain {1} which is not defined in Terrain.xml.", name, id));
                }
            }

            Check("Base terrain", s.BaseTerrain);
            Check("Shallow water", s.ShallowWaterTerrain);
            Check("Deep water", s.DeepWaterTerrain);
            Check("Mountain", s.MountainTerrain);
            if (s.BeachWidth > 0)
            {
                Check("Beach", s.BeachTerrain);
            }

            if (s.SnowOnPeaks)
            {
                Check("Snow", s.SnowTerrain);
            }
        }

        private byte[] ApplySketch(GeneratorInput input, GeneratorSettings s, byte[] kind, bool[] painted, int gw, int gh, int rw, int step, double rough, GradientNoise warpX, GradientNoise warpY, ParallelOptions po)
        {
            byte[] forced = new byte[gw * gh];
            int sw = input.SketchWidth;
            int sh = input.SketchHeight;
            byte[] sketch = input.Sketch;

            // the sketch covers the whole map width; the overworld part is used here.
            // The lookup position is warped with noise so painted shapes get natural coast lines.
            double pixelTiles = Math.Max((double)input.Width / sw, (double)input.Height / sh);
            double mapSize = Math.Min(input.Width, input.Height);
            double warp = Math.Max(pixelTiles * 3, mapSize * 0.03) * (0.3 + rough);
            double warpScale = Math.Max(pixelTiles * 8, mapSize * 0.08);
            byte mountain = (byte)s.MountainTerrain;
            byte baseTerrain = (byte)s.BaseTerrain;
            bool unpaintedWater = s.SketchUnpaintedIsWater;

            Parallel.For(0, gh, po, gy =>
            {
                double ty = gy * step;
                for (int gx = 0; gx < rw; gx++)
                {
                    double tx = gx * step;
                    double wx = tx + (warp * warpX.Fbm(tx / warpScale, ty / warpScale, 4));
                    double wy = ty + (warp * warpY.Fbm(tx / warpScale, ty / warpScale, 4));
                    int sx = Math.Clamp((int)(wx * sw / input.Width), 0, sw - 1);
                    int sy = Math.Clamp((int)(wy * sh / input.Height), 0, sh - 1);
                    byte v = sketch[(sy * sw) + sx];
                    int i = (gy * gw) + gx;

                    // No Draw, Blank (black) and transparent pixels = not painted
                    if (v == TerrainIds.NoDraw || v == TerrainIds.Blank || !_terrain.Defined[v])
                    {
                        if (unpaintedWater)
                        {
                            kind[i] = KindWater;
                        }

                        continue;
                    }

                    painted[i] = true;
                    bool water = _terrain.IsWater(v, _altitude);
                    kind[i] = water ? KindWater : KindLand;
                    switch (s.SketchUsage)
                    {
                        case SketchMode.UseTerrains:
                            forced[i] = v;
                            break;
                        case SketchMode.LandWaterOnly:
                            forced[i] = v == mountain ? v : (byte)0;
                            break;
                        default: // Smart
                            forced[i] = water || v == baseTerrain ? (byte)0 : v;
                            break;
                    }
                }
            });

            // in LandWaterOnly mode forced[] only carries mountains; painted[] protects all painted cells
            return forced;
        }

        private static void CleanComponents(byte[] kind, bool[] painted, int gw, int gh, int rw, int minIsland, int minLake, int[] queue, bool[] visited)
        {
            Array.Clear(visited, 0, visited.Length);
            for (int start = 0; start < kind.Length; start++)
            {
                if (visited[start] || (start % gw) >= rw)
                {
                    continue;
                }

                bool water = kind[start] == KindWater;
                int head = 0;
                int tail = 0;
                queue[tail++] = start;
                visited[start] = true;
                bool touchesBorder = false;
                bool hasForced = false;
                while (head < tail)
                {
                    int c = queue[head++];
                    int x = c % gw;
                    int y = c / gw;
                    if (x == 0 || y == 0 || x == rw - 1 || y == gh - 1)
                    {
                        touchesBorder = true;
                    }

                    if (painted != null && painted[c])
                    {
                        hasForced = true;
                    }

                    if (x > 0)
                    {
                        Visit(c - 1);
                    }

                    if (x < rw - 1)
                    {
                        Visit(c + 1);
                    }

                    if (y > 0)
                    {
                        Visit(c - gw);
                    }

                    if (y < gh - 1)
                    {
                        Visit(c + gw);
                    }
                }

                if (hasForced)
                {
                    continue;
                }

                if (!water && tail < minIsland)
                {
                    for (int k = 0; k < tail; k++)
                    {
                        kind[queue[k]] = KindWater;
                    }
                }
                else if (water && !touchesBorder && tail < minLake)
                {
                    for (int k = 0; k < tail; k++)
                    {
                        kind[queue[k]] = KindLand;
                    }
                }

                void Visit(int nb)
                {
                    if (!visited[nb] && (kind[nb] == KindWater) == water)
                    {
                        visited[nb] = true;
                        queue[tail++] = nb;
                    }
                }
            }
        }

        /// <summary>Multi source breadth first search (4-neighbour) inside the overworld.</summary>
        private static void Bfs(int gw, int gh, int rw, Func<int, bool> isSource, Func<int, bool> isPassable, ushort[] dist, int[] queue)
        {
            int head = 0;
            int tail = 0;
            for (int i = 0; i < dist.Length; i++)
            {
                if ((i % gw) < rw && isSource(i))
                {
                    dist[i] = 0;
                    queue[tail++] = i;
                }
                else
                {
                    dist[i] = ushort.MaxValue;
                }
            }

            if (tail == 0)
            {
                // no source at all (e.g. no water): everything is "far away"
                return;
            }

            while (head < tail)
            {
                int c = queue[head++];
                int x = c % gw;
                int y = c / gw;
                ushort next = dist[c] == ushort.MaxValue - 1 ? dist[c] : (ushort)(dist[c] + 1);
                if (x > 0)
                {
                    Try(c - 1);
                }

                if (x < rw - 1)
                {
                    Try(c + 1);
                }

                if (y > 0)
                {
                    Try(c - gw);
                }

                if (y < gh - 1)
                {
                    Try(c + gw);
                }

                void Try(int nb)
                {
                    if (dist[nb] == ushort.MaxValue && isPassable(nb))
                    {
                        dist[nb] = next;
                        queue[tail++] = nb;
                    }
                }
            }
        }

        private static double PlacementBias(BiomePlacement placement, double tx, double ty, int worldW, int worldH, double coastDist, double mountainDist)
        {
            switch (placement)
            {
                case BiomePlacement.Inland:
                    return (0.7 * MathUtil.SmoothStep(15, 180, coastDist)) - 0.35;
                case BiomePlacement.Coast:
                    return (0.7 * (1 - MathUtil.SmoothStep(4, 70, coastDist))) - 0.35;
                case BiomePlacement.NearMountains:
                    return (0.7 * (1 - MathUtil.SmoothStep(4, 110, mountainDist))) - 0.35;
                case BiomePlacement.Lowlands:
                    return (0.35 * (1 - MathUtil.SmoothStep(4, 90, coastDist))) + (0.35 * MathUtil.SmoothStep(20, 150, mountainDist)) - 0.35;
                case BiomePlacement.North:
                    return (0.9 * (1 - (ty / worldH))) - 0.45;
                case BiomePlacement.South:
                    return (0.9 * (ty / worldH)) - 0.45;
                case BiomePlacement.West:
                    return (0.9 * (1 - (tx / worldW))) - 0.45;
                case BiomePlacement.East:
                    return (0.9 * (tx / worldW)) - 0.45;
                default:
                    return 0;
            }
        }

        private static void SeparateBiomes(byte[] terrain, bool[] fromRule, byte baseTerrain, int gw, int gh, int rw, int iterations)
        {
            bool[] clear = new bool[terrain.Length];
            for (int it = 0; it < iterations; it++)
            {
                Array.Clear(clear, 0, clear.Length);
                bool any = false;
                for (int y = 0; y < gh; y++)
                {
                    for (int x = 0; x < rw; x++)
                    {
                        int i = (y * gw) + x;
                        byte t = terrain[i];
                        if (!fromRule[i])
                        {
                            continue;
                        }

                        for (int dy = -1; dy <= 1 && !clear[i]; dy++)
                        {
                            int ny = y + dy;
                            if (ny < 0 || ny >= gh)
                            {
                                continue;
                            }

                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx;
                                if (nx < 0 || nx >= rw)
                                {
                                    continue;
                                }

                                int j = (ny * gw) + nx;
                                if (fromRule[j] && terrain[j] != t)
                                {
                                    clear[i] = true;
                                    any = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!any)
                {
                    return;
                }

                for (int i = 0; i < terrain.Length; i++)
                {
                    if (clear[i])
                    {
                        terrain[i] = baseTerrain;
                        fromRule[i] = false;
                    }
                }
            }
        }

        private static double Lerp(double a, double b, double t) => a + ((b - a) * t);

        private static double SampleBilinear(byte[] data, int w, int h, double u, double v)
        {
            double x = Math.Clamp(u, 0, 1) * (w - 1);
            double y = Math.Clamp(v, 0, 1) * (h - 1);
            int x0 = (int)x;
            int y0 = (int)y;
            int x1 = Math.Min(w - 1, x0 + 1);
            int y1 = Math.Min(h - 1, y0 + 1);
            double fx = x - x0;
            double fy = y - y0;
            double a = Lerp(data[(y0 * w) + x0], data[(y0 * w) + x1], fx);
            double b = Lerp(data[(y1 * w) + x0], data[(y1 * w) + x1], fx);
            return Lerp(a, b, fy);
        }
    }
}
