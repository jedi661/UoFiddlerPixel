// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>
    /// All loaded transitions plus the knowledge which terrain "draws" which border.
    /// Transitions are defined from one side only (e.g. Grass draws the Grass/Sand edge, Sand cells just use
    /// the plain sand tile). A pattern without transition is only a problem when its centre terrain normally
    /// draws transitions for that combination of terrains.
    /// </summary>
    internal sealed class TransitionSet
    {
        private readonly HashSet<ulong> _drawn = new HashSet<ulong>();

        public Dictionary<TransitionKey, Transition.Transition> Map { get; }

        public int Count => Map.Count;

        public TransitionSet(Dictionary<TransitionKey, Transition.Transition> map)
        {
            Map = map;
            foreach (TransitionKey key in map.Keys)
            {
                _drawn.Add(Signature(key));
            }
        }

        public bool TryGet(TransitionKey key, out Transition.Transition transition) => Map.TryGetValue(key, out transition);

        /// <summary>True when the pattern is fine (uniform, has a transition, or is drawn by the other side).</summary>
        public bool IsOk(TransitionKey key)
        {
            if (key.IsUniform || Map.ContainsKey(key))
            {
                return true;
            }

            return !_drawn.Contains(Signature(key));
        }

        /// <summary>centre group + sorted set of the other groups, packed into 64 bit.</summary>
        private static ulong Signature(TransitionKey key)
        {
            byte centre = key[4];
            Span<byte> others = stackalloc byte[8];
            int count = 0;
            for (int k = 0; k < 9; k++)
            {
                byte g = key[k];
                if (g == centre)
                {
                    continue;
                }

                bool seen = false;
                for (int j = 0; j < count; j++)
                {
                    if (others[j] == g)
                    {
                        seen = true;
                        break;
                    }
                }

                if (!seen)
                {
                    others[count++] = g;
                }
            }

            others.Slice(0, count).Sort();
            ulong sig = centre;
            for (int j = 0; j < count && j < 7; j++)
            {
                sig = (sig << 8) | others[j];
            }

            return sig | ((ulong)count << 60);
        }
    }

    /// <summary>
    /// Removes terrain patterns that have no transition (1 tile wide strips, single tiles, checker corners ...).
    /// Each bad tile gets the neighbouring terrain that makes the most 3x3 patterns around it valid.
    /// Tiles are only changed when that is a real improvement, so unfixable borders stay as they are.
    /// </summary>
    internal static class TransitionRepair
    {
        public static int CountBad(MapDocument doc, TransitionSet transitions)
        {
            int bad = 0;
            for (int y = 0; y < doc.Height; y++)
            {
                for (int x = 0; x < doc.Width; x++)
                {
                    if (!IsValid(doc, transitions, x, y))
                    {
                        bad++;
                    }
                }
            }

            return bad;
        }

        /// <summary>Returns the number of changed tiles.</summary>
        public static int Repair(MapDocument doc, TerrainPalette terrains, AltitudePalette altitudes, TransitionSet transitions, int passes, IProgress<int> progress, Action<string> log, CancellationToken ct)
        {
            log = log ?? (_ => { });
            if (transitions == null || transitions.Count == 0)
            {
                log("Transition repair: no transitions loaded.");
                return 0;
            }

            int width = doc.Width;
            int height = doc.Height;
            int total = 0;
            int before = -1;
            byte[] candidates = new byte[9];
            for (int pass = 0; pass < Math.Max(1, passes); pass++)
            {
                int changed = 0;
                int bad = 0;
                for (int y = 0; y < height; y++)
                {
                    if ((y & 63) == 0)
                    {
                        ct.ThrowIfCancellationRequested();
                        progress?.Report((int)(100L * ((pass * (long)height) + y) / (Math.Max(1, passes) * (long)height)));
                    }

                    for (int x = 0; x < width; x++)
                    {
                        if (IsValid(doc, transitions, x, y))
                        {
                            continue;
                        }

                        bad++;
                        int i = (y * width) + x;
                        byte original = doc.Terrain[i];
                        int currentScore = LocalScore(doc, transitions, x, y);

                        // candidate terrains = distinct groups in the 3x3 area
                        int count = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                byte g = doc.Terrain[Wrap(y + dy, height) * width + Wrap(x + dx, width)];
                                if (g == original)
                                {
                                    continue;
                                }

                                bool seen = false;
                                for (int k = 0; k < count; k++)
                                {
                                    if (candidates[k] == g)
                                    {
                                        seen = true;
                                        break;
                                    }
                                }

                                if (!seen)
                                {
                                    candidates[count++] = g;
                                }
                            }
                        }

                        int bestScore = currentScore;
                        byte best = original;
                        for (int k = 0; k < count; k++)
                        {
                            doc.Terrain[i] = candidates[k];
                            int score = LocalScore(doc, transitions, x, y);
                            if (score > bestScore)
                            {
                                bestScore = score;
                                best = candidates[k];
                            }
                        }

                        doc.Terrain[i] = best;
                        if (best != original)
                        {
                            changed++;
                            bool wasWater = terrains.IsWater(original, altitudes);
                            bool isWater = terrains.IsWater(best, altitudes);
                            if (wasWater != isWater || !altitudes.Defined[doc.Altitude[i]])
                            {
                                doc.Altitude[i] = altitudes.BaseKeyFor(terrains, best);
                            }
                        }
                    }
                }

                total += changed;
                if (before < 0)
                {
                    before = bad;
                }

                log(string.Format("  repair pass {0}: {1} problem tiles, {2} changed", pass + 1, bad, changed));
                if (changed == 0)
                {
                    break;
                }
            }

            progress?.Report(100);
            return total;
        }

        private static int Wrap(int v, int size) => v < 0 ? v + size : (v >= size ? v - size : v);

        /// <summary>Number of valid 3x3 patterns centred on (x, y) and its 8 neighbours.</summary>
        private static int LocalScore(MapDocument doc, TransitionSet transitions, int x, int y)
        {
            int score = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (IsValid(doc, transitions, Wrap(x + dx, doc.Width), Wrap(y + dy, doc.Height)))
                    {
                        score++;
                    }
                }
            }

            return score;
        }

        public static TransitionKey KeyAt(MapDocument doc, int x, int y)
        {
            int w = doc.Width;
            int h = doc.Height;
            int left = x == 0 ? w - 1 : x - 1;
            int right = x == w - 1 ? 0 : x + 1;
            int up = y == 0 ? h - 1 : y - 1;
            int down = y == h - 1 ? 0 : y + 1;
            byte[] g = doc.Terrain;
            return new TransitionKey(
                g[(up * w) + left], g[(up * w) + x], g[(up * w) + right],
                g[(y * w) + left], g[(y * w) + x], g[(y * w) + right],
                g[(down * w) + left], g[(down * w) + x], g[(down * w) + right]);
        }

        public static bool IsValid(MapDocument doc, TransitionSet transitions, int x, int y)
        {
            return transitions.IsOk(KeyAt(doc, x, y));
        }
    }
}
