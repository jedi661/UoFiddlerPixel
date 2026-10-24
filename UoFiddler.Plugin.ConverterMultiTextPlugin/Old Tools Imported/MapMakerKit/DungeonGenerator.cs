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
using System.Drawing;
using System.Threading;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>
    /// Carves natural looking caves (cellular automaton) into the dungeon area.
    /// Floor = Dungeon Cave, surrounding = Blank (the transitions Blank &lt;-&gt; Dungeon Cave exist).
    /// Optional lava and water pools inside the caves.
    /// </summary>
    internal sealed class DungeonGenerator
    {
        private readonly TerrainPalette _terrain;
        private readonly AltitudePalette _altitude;

        public DungeonGenerator(TerrainPalette terrain, AltitudePalette altitude)
        {
            _terrain = terrain;
            _altitude = altitude;
        }

        /// <summary>Carves caves into the given rectangle (document cells). Returns the number of caves.</summary>
        public int Carve(MapDocument doc, int areaX, int areaY, int areaW, int areaH, GeneratorSettings s, int step, Random rng, CancellationToken ct)
        {
            step = Math.Max(1, step);
            int minSize = Math.Max(12, Math.Min(s.DungeonMinSize, s.DungeonMaxSize) / step);
            int maxSize = Math.Max(minSize, Math.Max(s.DungeonMinSize, s.DungeonMaxSize) / step);
            int margin = Math.Max(2, 6 / step);
            byte floor = _terrain.Defined[s.DungeonFloorTerrain] ? (byte)s.DungeonFloorTerrain : TerrainIds.DungeonCave;
            byte floorKey = _altitude.BaseKeyFor(_terrain, floor);
            double openness = Math.Clamp(s.DungeonOpenness / 100.0, 0, 1);

            List<Rectangle> placed = new List<Rectangle>();
            for (int c = 0; c < Math.Max(0, s.DungeonCount); c++)
            {
                ct.ThrowIfCancellationRequested();
                Rectangle box = Rectangle.Empty;
                for (int attempt = 0; attempt < 200; attempt++)
                {
                    int w = rng.Next(minSize, maxSize + 1);
                    int h = (int)(w * (0.6 + (rng.NextDouble() * 0.8)));
                    h = Math.Clamp(h, minSize, maxSize);
                    if (w + (2 * margin) >= areaW || h + (2 * margin) >= areaH)
                    {
                        w = Math.Min(w, areaW - (2 * margin) - 1);
                        h = Math.Min(h, areaH - (2 * margin) - 1);
                        if (w < 8 || h < 8)
                        {
                            break;
                        }
                    }

                    int x = areaX + margin + rng.Next(Math.Max(1, areaW - w - (2 * margin)));
                    int y = areaY + margin + rng.Next(Math.Max(1, areaH - h - (2 * margin)));
                    Rectangle candidate = new Rectangle(x, y, w, h);
                    Rectangle grown = Rectangle.Inflate(candidate, margin, margin);
                    bool overlaps = false;
                    foreach (Rectangle r in placed)
                    {
                        if (r.IntersectsWith(grown))
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    if (!overlaps)
                    {
                        box = candidate;
                        break;
                    }
                }

                if (box.IsEmpty)
                {
                    break;
                }

                bool[] cave = GrowCave(box.Width, box.Height, openness, rng);
                if (cave == null)
                {
                    continue;
                }

                placed.Add(box);
                for (int y = 0; y < box.Height; y++)
                {
                    for (int x = 0; x < box.Width; x++)
                    {
                        if (cave[(y * box.Width) + x])
                        {
                            int i = doc.Index(box.X + x, box.Y + y);
                            doc.Terrain[i] = floor;
                            doc.Altitude[i] = floorKey;
                        }
                    }
                }

                if (rng.NextDouble() * 100 < s.DungeonLavaPercent)
                {
                    AddPool(doc, box, cave, TerrainIds.Lava, step, rng);
                }

                if (rng.NextDouble() * 100 < s.DungeonWaterPercent)
                {
                    AddPool(doc, box, cave, TerrainIds.ShallowWater, step, rng);
                }
            }

            return placed.Count;
        }

        /// <summary>Cellular automaton cave inside an ellipse; only the largest connected part is kept.</summary>
        private static bool[] GrowCave(int w, int h, double openness, Random rng)
        {
            bool[] floor = new bool[w * h];
            double fill = 0.40 + (openness * 0.16); // chance of floor at start
            double cx = (w - 1) / 2.0;
            double cy = (h - 1) / 2.0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double dx = (x - cx) / (w / 2.0);
                    double dy = (y - cy) / (h / 2.0);
                    double d = (dx * dx) + (dy * dy);
                    floor[(y * w) + x] = d < 0.85 && rng.NextDouble() < fill + (0.15 * (1 - d));
                }
            }

            bool[] next = new bool[w * h];
            for (int it = 0; it < 5; it++)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int walls = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (dx == 0 && dy == 0)
                                {
                                    continue;
                                }

                                int nx = x + dx;
                                int ny = y + dy;
                                if (nx < 1 || ny < 1 || nx >= w - 1 || ny >= h - 1 || !floor[(ny * w) + nx])
                                {
                                    walls++;
                                }
                            }
                        }

                        next[(y * w) + x] = x > 0 && y > 0 && x < w - 1 && y < h - 1 && walls < 5;
                    }
                }

                (floor, next) = (next, floor);
            }

            // keep the largest region
            int[] label = new int[w * h];
            int bestLabel = 0;
            int bestSize = 0;
            int current = 0;
            Queue<int> queue = new Queue<int>();
            for (int i = 0; i < floor.Length; i++)
            {
                if (!floor[i] || label[i] != 0)
                {
                    continue;
                }

                current++;
                int size = 0;
                label[i] = current;
                queue.Enqueue(i);
                while (queue.Count > 0)
                {
                    int c = queue.Dequeue();
                    size++;
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
                }

                if (size > bestSize)
                {
                    bestSize = size;
                    bestLabel = current;
                }

                void Visit(int nb)
                {
                    if (floor[nb] && label[nb] == 0)
                    {
                        label[nb] = current;
                        queue.Enqueue(nb);
                    }
                }
            }

            if (bestSize < 16)
            {
                return null;
            }

            for (int i = 0; i < floor.Length; i++)
            {
                floor[i] = label[i] == bestLabel;
            }

            return floor;
        }

        private void AddPool(MapDocument doc, Rectangle box, bool[] cave, byte terrain, int step, Random rng)
        {
            if (!_terrain.Defined[terrain])
            {
                return;
            }

            int w = box.Width;
            int h = box.Height;
            int radius = Math.Max(2, rng.Next(3, 9) / step);
            int keepAway = radius + Math.Max(2, 3 / step);

            // find cave cells that are far enough from the cave wall
            List<int> candidates = new List<int>();
            for (int y = keepAway; y < h - keepAway; y++)
            {
                for (int x = keepAway; x < w - keepAway; x++)
                {
                    bool ok = true;
                    for (int dy = -keepAway; dy <= keepAway && ok; dy += Math.Max(1, keepAway / 2))
                    {
                        for (int dx = -keepAway; dx <= keepAway; dx += Math.Max(1, keepAway / 2))
                        {
                            if (!cave[((y + dy) * w) + x + dx])
                            {
                                ok = false;
                                break;
                            }
                        }
                    }

                    if (ok)
                    {
                        candidates.Add((y * w) + x);
                    }
                }
            }

            if (candidates.Count == 0)
            {
                return;
            }

            int center = candidates[rng.Next(candidates.Count)];
            int cxp = center % w;
            int cyp = center / w;
            byte key = _altitude.BaseKeyFor(_terrain, terrain);
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    double d = Math.Sqrt((dx * dx) + (dy * dy)) + (rng.NextDouble() * 0.8);
                    int x = cxp + dx;
                    int y = cyp + dy;
                    if (d <= radius && x >= 0 && y >= 0 && x < w && y < h && cave[(y * w) + x])
                    {
                        int i = doc.Index(box.X + x, box.Y + y);
                        doc.Terrain[i] = terrain;
                        doc.Altitude[i] = key;
                    }
                }
            }
        }
    }
}
