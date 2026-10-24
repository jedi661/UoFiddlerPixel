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
using System.Threading.Tasks;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>Which tiles a height operation touches.</summary>
    internal sealed class HeightFilter
    {
        /// <summary>null = all altitude types.</summary>
        public AltitudeType? Type { get; set; }

        /// <summary>-1 = all terrains, otherwise only this terrain group.</summary>
        public int TerrainId { get; set; } = -1;

        public bool Matches(MapDocument doc, int i, AltitudePalette altitudes)
        {
            if (TerrainId >= 0 && doc.Terrain[i] != TerrainId)
            {
                return false;
            }

            return Type == null || altitudes.TypeOf(doc.Altitude[i]) == Type.Value;
        }

        public static HeightFilter All => new HeightFilter();
    }

    /// <summary>
    /// Operations on the altitude layer. Altitudes are kept as palette keys, so every result is snapped
    /// to a valid entry of Altitude.xml (steps of 5) and keeps its type (water / land / mountain / other).
    /// </summary>
    internal static class HeightTools
    {
        /// <summary>Altitude = default of the terrain (the old "Make Altitude Image").</summary>
        public static int ResetFromTerrain(MapDocument doc, TerrainPalette terrains, AltitudePalette altitudes, HeightFilter filter)
        {
            int changed = 0;
            for (int i = 0; i < doc.Terrain.Length; i++)
            {
                if (filter != null && !filter.Matches(doc, i, altitudes))
                {
                    continue;
                }

                byte key = altitudes.BaseKeyFor(terrains, doc.Terrain[i]);
                if (doc.Altitude[i] != key)
                {
                    doc.Altitude[i] = key;
                    changed++;
                }
            }

            return changed;
        }

        public static int Offset(MapDocument doc, AltitudePalette altitudes, HeightFilter filter, int offset)
        {
            return Apply(doc, altitudes, filter, (i, a) => a + offset);
        }

        /// <summary>Multiplies the height (0 stays 0). factor 2 = twice as high.</summary>
        public static int Scale(MapDocument doc, AltitudePalette altitudes, HeightFilter filter, double factor)
        {
            return Apply(doc, altitudes, filter, (i, a) => (int)Math.Round(a * factor));
        }

        public static int Clamp(MapDocument doc, AltitudePalette altitudes, HeightFilter filter, int min, int max)
        {
            if (min > max)
            {
                (min, max) = (max, min);
            }

            return Apply(doc, altitudes, filter, (i, a) => Math.Clamp(a, min, max));
        }

        /// <summary>Adds random bumps (amplitude in z, size in tiles).</summary>
        public static int Roughen(MapDocument doc, AltitudePalette altitudes, HeightFilter filter, int amplitude, int size, int seed)
        {
            GradientNoise noise = new GradientNoise(seed);
            double scale = Math.Max(2, size);
            int w = doc.Width;
            return Apply(doc, altitudes, filter, (i, a) =>
            {
                int x = i % w;
                int y = i / w;
                return a + (int)Math.Round(noise.Fbm(x / scale, y / scale, 3) * amplitude);
            });
        }

        /// <summary>3x3 box blur, repeated <paramref name="passes"/> times.</summary>
        public static int Smooth(MapDocument doc, AltitudePalette altitudes, HeightFilter filter, int passes)
        {
            int w = doc.Width;
            int h = doc.Height;
            int changedTotal = 0;
            float[] values = new float[doc.Altitude.Length];
            float[] next = new float[doc.Altitude.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = altitudes.Altitude[doc.Altitude[i]];
            }

            bool[] mask = new bool[values.Length];
            for (int i = 0; i < mask.Length; i++)
            {
                mask[i] = filter == null || filter.Matches(doc, i, altitudes);
            }

            for (int p = 0; p < Math.Max(1, passes); p++)
            {
                Parallel.For(0, h, y =>
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = (y * w) + x;
                        if (!mask[i])
                        {
                            next[i] = values[i];
                            continue;
                        }

                        float sum = 0;
                        int count = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int ny = y + dy;
                            if (ny < 0 || ny >= h)
                            {
                                continue;
                            }

                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx;
                                if (nx < 0 || nx >= w)
                                {
                                    continue;
                                }

                                int j = (ny * w) + nx;
                                if (!mask[j])
                                {
                                    continue;
                                }

                                sum += values[j];
                                count++;
                            }
                        }

                        next[i] = count == 0 ? values[i] : sum / count;
                    }
                });
                (values, next) = (next, values);
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (!mask[i])
                {
                    continue;
                }

                byte key = altitudes.WithAltitude(doc.Altitude[i], (int)Math.Round(values[i]));
                if (key != doc.Altitude[i])
                {
                    doc.Altitude[i] = key;
                    changedTotal++;
                }
            }

            return changedTotal;
        }

        /// <summary>
        /// Rebuilds mountain heights from the distance to the mountain foot:
        /// height = min(maxHeight, distance * slope), so every mountain rises evenly.
        /// </summary>
        public static int MountainProfile(MapDocument doc, AltitudePalette altitudes, int mountainTerrain, int slope, int maxHeight, int seed)
        {
            int w = doc.Width;
            int h = doc.Height;
            int n = w * h;
            int[] dist = new int[n];
            int[] queue = new int[n];
            int head = 0;
            int tail = 0;
            for (int i = 0; i < n; i++)
            {
                if (doc.Terrain[i] == mountainTerrain)
                {
                    dist[i] = int.MaxValue;
                }
                else
                {
                    dist[i] = 0;
                    queue[tail++] = i;
                }
            }

            while (head < tail)
            {
                int c = queue[head++];
                int x = c % w;
                int y = c / w;
                if (x > 0)
                {
                    Visit(c - 1);
                }

                if (x < w - 1)
                {
                    Visit(c + 1);
                }

                if (y > 0)
                {
                    Visit(c - w);
                }

                if (y < h - 1)
                {
                    Visit(c + w);
                }

                void Visit(int nb)
                {
                    if (dist[nb] == int.MaxValue)
                    {
                        dist[nb] = dist[c] + 1;
                        queue[tail++] = nb;
                    }
                }
            }

            GradientNoise noise = new GradientNoise(seed);
            int changed = 0;
            for (int i = 0; i < n; i++)
            {
                if (doc.Terrain[i] != mountainTerrain || dist[i] == int.MaxValue)
                {
                    continue;
                }

                double variation = 0.75 + (0.25 * noise.Fbm((i % w) / 60.0, (i / w) / 60.0, 3));
                int height = (int)Math.Min(maxHeight, dist[i] * slope * variation);
                height = Math.Max(5, MathUtil.RoundTo(height, 5));
                byte key = altitudes.Encode(AltitudeType.Mountain, height);
                if (key != doc.Altitude[i])
                {
                    doc.Altitude[i] = key;
                    changed++;
                }
            }

            return changed;
        }

        private static int Apply(MapDocument doc, AltitudePalette altitudes, HeightFilter filter, Func<int, int, int> change)
        {
            int changed = 0;
            for (int i = 0; i < doc.Altitude.Length; i++)
            {
                if (filter != null && !filter.Matches(doc, i, altitudes))
                {
                    continue;
                }

                byte old = doc.Altitude[i];
                int value = change(i, altitudes.Altitude[old]);
                byte key = altitudes.WithAltitude(old, value);
                if (key != old)
                {
                    doc.Altitude[i] = key;
                    changed++;
                }
            }

            return changed;
        }

        /// <summary>Statistics for the log: min / max / average altitude.</summary>
        public static string Describe(MapDocument doc, AltitudePalette altitudes)
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            long sum = 0;
            for (int i = 0; i < doc.Altitude.Length; i++)
            {
                int a = altitudes.Altitude[doc.Altitude[i]];
                min = Math.Min(min, a);
                max = Math.Max(max, a);
                sum += a;
            }

            return string.Format("Altitude: min {0}, max {1}, average {2:0.0}", min, max, sum / (double)Math.Max(1, doc.Altitude.Length));
        }
    }
}
