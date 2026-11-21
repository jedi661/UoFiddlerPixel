/***************************************************************************
 *
 *  * Dragon Map Studio – mountain shaper for the CGI view (brush and marked area)
 *  *
 *  * Builds a mountain like the original map: from a foot height ("ab Z") the
 *  * land rises with the distance to the edge, capped at the peak height, with
 *  * hills (smooth noise) and a rough surface (per tile noise). Never lowers the
 *  * land when "Nur anheben" is on.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public sealed class MountainSettings
    {
        [Category("1 Höhe"), DisplayName("Fuß automatisch"), Description("An: der Berg beginnt auf der Bodenhöhe (Pinsel: Höhe beim ersten Klick, Bereich: niedrigste Höhe am Rand). Aus: ab 'Fuß-Z'.")]
        public bool AutoFoot { get; set; } = true;

        [Category("1 Höhe"), DisplayName("Fuß-Z (ab)"), Description("Ab dieser Höhe steigt der Berg an (wenn 'Fuß automatisch' aus ist).")]
        public int FootZ { get; set; }

        [Category("1 Höhe"), DisplayName("Gipfel-Z"), Description("Höchste Höhe (Original: bis etwa 90–100).")]
        public int PeakZ { get; set; } = 90;

        [Category("1 Höhe"), DisplayName("Steilheit"), Description("Z pro Tile vom Rand nach innen. 8–12 = steile Felswand wie im Original, 3–5 = sanfter Hang.")]
        public int Steepness { get; set; } = 10;

        [Category("2 Form"), DisplayName("Hügel-Größe"), Description("Größe der Kuppen und Täler auf dem Berg in Tiles (glattes Rauschen).")]
        public int HillSize { get; set; } = 6;

        [Category("2 Form"), DisplayName("Hügel-Stärke %"), Description("Wie stark die Kuppen den Gipfel absenken (0 = flaches Plateau auf Gipfel-Z).")]
        public int HillPercent { get; set; } = 40;

        [Category("2 Form"), DisplayName("Rauheit"), Description("Zufällige Höhe ± pro Tile – die Felskanten. 0 = glatt.")]
        public int Roughness { get; set; } = 5;

        [Category("2 Form"), DisplayName("Rand ausfransen %"), Description("Der Rand folgt nicht genau dem Rechteck / Kreis: so viel Prozent der Breite wird zufällig abgeschnitten.")]
        public int EdgeNoise { get; set; } = 35;

        [Category("2 Form"), DisplayName("Nur anheben"), Description("Vorhandenes Land wird nie abgesenkt (mehrere Züge bauen einen Grat auf).")]
        public bool OnlyRaise { get; set; } = true;

        [Category("3 Tiles"), DisplayName("Berg-Tiles"), Description("Land-Tiles (hex) des Berges, zufällig gemischt. Leer = Tiles der Berg-Gruppe der Farbtabelle.")]
        public string Tiles { get; set; } = "022c 022d 022e 022f";

        [Category("3 Tiles"), DisplayName("Fuß-Tiles"), Description("Optional: Tiles für die äußerste Reihe (z. B. Erde 0071–0078 wie der Weg am Bergfuß). Leer = auch Berg.")]
        public string FootTiles { get; set; } = string.Empty;

        [Category("3 Tiles"), DisplayName("Zufalls-Zahl"), Description("Startwert für Rauschen und Tiles – gleiche Zahl = gleicher Berg.")]
        public int Seed { get; set; } = 1234;
    }

    public static class MountainShaper
    {
        /// <summary>Stable 0..1 value for a cell.</summary>
        public static double Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 144269504);
                h = (h ^ (h >> 13)) * 1274126177;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (double)0xFFFFFF;
            }
        }

        /// <summary>Smooth value noise 0..1 with the given cell size.</summary>
        public static double Smooth(int x, int y, int size, int seed)
        {
            size = Math.Max(1, size);
            int gx = (int)Math.Floor(x / (double)size), gy = (int)Math.Floor(y / (double)size);
            double fx = x / (double)size - gx, fy = y / (double)size - gy;
            fx = fx * fx * (3 - 2 * fx);
            fy = fy * fy * (3 - 2 * fy);
            double a = Hash(gx, gy, seed), b = Hash(gx + 1, gy, seed), c = Hash(gx, gy + 1, seed), d = Hash(gx + 1, gy + 1, seed);
            return (a * (1 - fx) + b * fx) * (1 - fy) + (c * (1 - fx) + d * fx) * fy;
        }

        /// <summary>Height of a mountain cell that lies <paramref name="dist"/> tiles inside the edge (dist 1 = outermost row).</summary>
        public static int Height(MountainSettings s, int x, int y, double dist, int footZ)
        {
            double hill = 1.0 - Math.Clamp(s.HillPercent, 0, 100) / 100.0 * Smooth(x, y, s.HillSize, s.Seed + 11);
            double cap = footZ + Math.Max(0, s.PeakZ - footZ) * hill;
            double h = Math.Min(footZ + (dist - 0.5) * Math.Max(1, s.Steepness), cap);
            if (s.Roughness > 0 && dist > 1)
            {
                h += (Hash(x, y, s.Seed + 23) * 2 - 1) * s.Roughness;
            }
            return (int)Math.Round(Math.Clamp(h, -128, 127));
        }

        public static ushort Tile(ushort[] tiles, int x, int y, int seed)
        {
            return tiles[(int)(Hash(x, y, seed + 37) * tiles.Length) % tiles.Length];
        }

        /// <summary>
        /// Mountain over a marked area: distance to the edge (8 neighbours), the edge frayed by noise.
        /// Returns the land changes (x, y, id, z) without applying them.
        /// </summary>
        public static List<(int x, int y, ushort id, int z)> ShapeArea(MapData map, Rectangle area, MountainSettings s, ushort[] tiles)
        {
            var result = new List<(int, int, ushort, int)>();
            if (tiles == null || tiles.Length == 0 || area.Width <= 0 || area.Height <= 0)
            {
                return result;
            }
            ushort[] foot = HexList.Parse(s.FootTiles);
            int w = area.Width, h = area.Height;
            // frayed edge: a cell is mountain when its distance to the rectangle edge is larger than a noisy margin
            double maxMargin = Math.Min(w, h) / 2.0 * Math.Clamp(s.EdgeNoise, 0, 90) / 100.0;
            var inside = new bool[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int edge = Math.Min(Math.Min(x, w - 1 - x), Math.Min(y, h - 1 - y));
                    double margin = maxMargin * Smooth(area.X + x, area.Y + y, Math.Max(2, s.HillSize), s.Seed + 51);
                    inside[y * w + x] = edge >= margin;
                }
            }
            // distance to the nearest cell outside (BFS, 8 neighbours)
            var dist = new int[w * h];
            var queue = new Queue<int>();
            for (int i = 0; i < w * h; i++)
            {
                int x = i % w, y = i / w;
                bool border = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                if (!inside[i])
                {
                    dist[i] = 0;
                    queue.Enqueue(i);
                }
                else if (border)
                {
                    dist[i] = 1;
                    queue.Enqueue(i);
                }
                else
                {
                    dist[i] = int.MaxValue;
                }
            }
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                int x = i % w, y = i / w;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                        {
                            continue;
                        }
                        int n = ny * w + nx;
                        if (dist[n] > dist[i] + 1)
                        {
                            dist[n] = dist[i] + 1;
                            queue.Enqueue(n);
                        }
                    }
                }
            }
            int footZ = s.FootZ;
            if (s.AutoFoot)
            {
                footZ = int.MaxValue;
                for (int x = area.Left; x < area.Right; x++)
                {
                    footZ = Math.Min(footZ, Math.Min(map.LandZ(x, area.Top), map.LandZ(x, area.Bottom - 1)));
                }
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    footZ = Math.Min(footZ, Math.Min(map.LandZ(area.Left, y), map.LandZ(area.Right - 1, y)));
                }
            }
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int d = dist[y * w + x];
                    int mx = area.X + x, my = area.Y + y;
                    if (d <= 0 || !map.InBounds(mx, my))
                    {
                        continue;
                    }
                    int z = Height(s, mx, my, d, footZ);
                    int old = map.LandZ(mx, my);
                    if (s.OnlyRaise && z < old)
                    {
                        z = old;
                    }
                    ushort id = d == 1 && foot.Length > 0 ? Tile(foot, mx, my, s.Seed) : Tile(tiles, mx, my, s.Seed);
                    result.Add((mx, my, id, z));
                }
            }
            return result;
        }
    }
}
