// /***************************************************************************
//  *
//  * Dragon Map Studio – reads heights / tiles / statics of a map (e.g. the original OSI map)
//  * and writes a report, so the rules (coast, mountains, statics) can be compared with the original.
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public static class MapAnalyzer
    {
        private const int MaxDistance = 10;

        /// <summary>Water land tiles: wet flag of the tile data, or the classic water ids.</summary>
        private static bool[] WaterTable()
        {
            var water = new bool[0x10000];
            for (int id = 0; id < 0x4000; id++)
            {
                bool wet;
                try
                {
                    wet = (Ultima.TileData.LandTable[id].Flags & Ultima.TileFlag.Wet) != 0;
                }
                catch (Exception)
                {
                    wet = false;
                }
                water[id] = wet;
            }
            foreach (int id in new[] { 0xA8, 0xA9, 0xAA, 0xAB, 0x136, 0x137 })
            {
                water[id] = true;
            }
            return water;
        }

        private sealed class Stat
        {
            public long N;
            public long Sum;
            public int Min = int.MaxValue;
            public int Max = int.MinValue;
            public readonly Dictionary<int, long> Hist = new Dictionary<int, long>();

            public void Add(int v)
            {
                N++;
                Sum += v;
                Min = Math.Min(Min, v);
                Max = Math.Max(Max, v);
                Hist.TryGetValue(v, out long c);
                Hist[v] = c + 1;
            }

            public override string ToString()
            {
                if (N == 0)
                {
                    return "-";
                }
                string top = string.Join(" ", Hist.OrderByDescending(k => k.Value).Take(6).Select(k => $"{k.Key}:{k.Value * 100.0 / N:0}%"));
                return $"n={N,8:N0}  min {Min,4}  Ø {Sum / (double)N,6:0.0}  max {Max,4}   häufig/common: {top}";
            }
        }

        public static string Analyze(MapData map, Rectangle area, string source, IProgress<int> progress, CancellationToken cancel)
        {
            bool[] isWater = WaterTable();
            int w = map.Width, h = map.Height, n = w * h;
            var sb = new StringBuilder();
            sb.AppendLine("==========================================================================");
            sb.AppendLine(" Dragon Map Studio – Karten-Analyse / map analysis");
            sb.AppendLine($" {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}");
            sb.AppendLine($" Quelle / source: {source}  ({w}x{h}, {map.StaticCount:N0} Statics)");
            sb.AppendLine("==========================================================================");

            // distance to water (land) and to land (water), 8 neighbours, up to MaxDistance
            var dist = new byte[n];
            Array.Fill(dist, (byte)255);
            var q = new Queue<int>();
            for (int i = 0; i < n; i++)
            {
                bool wi = isWater[map.Tiles[i]];
                // a tile is at distance 1 when an 8-neighbour has the other kind
                int x = i % w, y = i / w;
                for (int dy = -1; dy <= 1 && dist[i] == 255; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < w && ny < h && isWater[map.Tiles[ny * w + nx]] != wi)
                        {
                            dist[i] = 1;
                            q.Enqueue(i);
                            break;
                        }
                    }
                }
                if ((i & 0x3FFFF) == 0)
                {
                    cancel.ThrowIfCancellationRequested();
                    progress?.Report((int)(i * 30L / n));
                }
            }
            while (q.Count > 0)
            {
                int i = q.Dequeue();
                if (dist[i] >= MaxDistance)
                {
                    continue;
                }
                bool wi = isWater[map.Tiles[i]];
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
                        int j = ny * w + nx;
                        if (dist[j] == 255 && isWater[map.Tiles[j]] == wi)
                        {
                            dist[j] = (byte)(dist[i] + 1);
                            q.Enqueue(j);
                        }
                    }
                }
            }

            var landByDist = new Stat[MaxDistance + 1];
            var waterByDist = new Stat[MaxDistance + 1];
            for (int d = 0; d <= MaxDistance; d++)
            {
                landByDist[d] = new Stat();
                waterByDist[d] = new Stat();
            }
            var landTiles = new Dictionary<ushort, Stat>();
            var shoreTiles = new Dictionary<ushort, Stat>();
            var steps = new Stat();
            for (int i = 0; i < n; i++)
            {
                ushort id = map.Tiles[i];
                int z = map.Z[i];
                int d = dist[i] == 255 ? 0 : dist[i];
                if (isWater[id])
                {
                    if (d > 0)
                    {
                        waterByDist[d].Add(z);
                    }
                }
                else
                {
                    if (d > 0)
                    {
                        landByDist[d].Add(z);
                    }
                    if (d == 1)
                    {
                        if (!shoreTiles.TryGetValue(id, out Stat st))
                        {
                            shoreTiles[id] = st = new Stat();
                        }
                        st.Add(z);
                    }
                }
                if (!landTiles.TryGetValue(id, out Stat lt))
                {
                    landTiles[id] = lt = new Stat();
                }
                lt.Add(z);
                int x = i % w;
                if (x + 1 < w && !isWater[id] && !isWater[map.Tiles[i + 1]])
                {
                    steps.Add(Math.Abs(map.Z[i + 1] - z));
                }
                if ((i & 0x3FFFF) == 0)
                {
                    cancel.ThrowIfCancellationRequested();
                    progress?.Report(30 + (int)(i * 40L / n));
                }
            }

            sb.AppendLine();
            sb.AppendLine("--- Küste: Land-Höhe nach Abstand zum Wasser / coast: land z by distance to water ---");
            for (int d = 1; d <= MaxDistance; d++)
            {
                sb.AppendLine($"d={d,2}: {landByDist[d]}");
            }
            sb.AppendLine();
            sb.AppendLine("--- Küste: Wasser-Höhe nach Abstand zum Land / coast: water z by distance to land ---");
            for (int d = 1; d <= MaxDistance; d++)
            {
                sb.AppendLine($"d={d,2}: {waterByDist[d]}");
            }
            sb.AppendLine();
            sb.AppendLine("--- Ufer-Tiles (Land direkt am Wasser) / shore tiles (land next to water): id name : z ---");
            foreach (KeyValuePair<ushort, Stat> kv in shoreTiles.OrderByDescending(k => k.Value.N).Take(40))
            {
                sb.AppendLine($"{kv.Key:x4} {UoArtCache.LandName(kv.Key),-16} {kv.Value}");
            }

            // statics near the coast and in general
            var nearCoast = new Dictionary<ushort, (Stat abs, Stat rel)>();
            var all = new Dictionary<ushort, (Stat abs, Stat rel)>();
            long k = 0;
            foreach (StaticItem s in map.AllStatics())
            {
                int i = s.Y * w + s.X;
                int landZ = map.Z[i];
                bool coast = dist[i] != 255 && dist[i] <= 2;
                Dictionary<ushort, (Stat abs, Stat rel)> target = coast ? nearCoast : all;
                if (!target.TryGetValue(s.Id, out var st))
                {
                    target[s.Id] = st = (new Stat(), new Stat());
                }
                st.abs.Add(s.Z);
                st.rel.Add(s.Z - landZ);
                if ((++k & 0xFFFF) == 0)
                {
                    cancel.ThrowIfCancellationRequested();
                }
            }
            progress?.Report(85);
            sb.AppendLine();
            sb.AppendLine("--- Statics an der Küste (≤ 2 Tiles) / statics at the coast: id name | z absolut | z über Land / above land ---");
            foreach (var kv in nearCoast.OrderByDescending(x => x.Value.abs.N).Take(40))
            {
                sb.AppendLine($"{kv.Key:x4} {UoArtCache.ItemName(kv.Key),-18} abs: {kv.Value.abs}");
                sb.AppendLine($"{"",23} rel: {kv.Value.rel}");
            }
            sb.AppendLine();
            sb.AppendLine("--- Häufigste Statics sonst / most common other statics: z über Land / above land ---");
            foreach (var kv in all.OrderByDescending(x => x.Value.abs.N).Take(25))
            {
                sb.AppendLine($"{kv.Key:x4} {UoArtCache.ItemName(kv.Key),-18} rel: {kv.Value.rel}");
            }
            sb.AppendLine();
            sb.AppendLine("--- Höhensprung zwischen Land-Nachbarn (Ost) / height step between land neighbours ---");
            sb.AppendLine(steps.ToString());
            sb.AppendLine();
            sb.AppendLine("--- Land-Tiles: id name : z (häufigste 60) / land tiles ---");
            foreach (KeyValuePair<ushort, Stat> kv in landTiles.OrderByDescending(x => x.Value.N).Take(60))
            {
                sb.AppendLine($"{kv.Key:x4} {UoArtCache.LandName(kv.Key),-16} {kv.Value}");
            }

            if (!area.IsEmpty)
            {
                AppendArea(sb, map, area);
            }
            progress?.Report(100);
            return sb.ToString();
        }

        /// <summary>Raster of a marked area: land id, z and statics (max. 64×64).</summary>
        private static void AppendArea(StringBuilder sb, MapData map, Rectangle area)
        {
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            area.Width = Math.Min(area.Width, 64);
            area.Height = Math.Min(area.Height, 64);
            sb.AppendLine();
            sb.AppendLine($"--- Bereich / area ({area.X},{area.Y}) {area.Width}x{area.Height}: Land-Höhe z / land z ---");
            sb.Append("      ");
            for (int x = area.Left; x < area.Right; x++)
            {
                sb.Append($"{x % 1000,5}");
            }
            sb.AppendLine();
            for (int y = area.Top; y < area.Bottom; y++)
            {
                sb.Append($"{y,5} ");
                for (int x = area.Left; x < area.Right; x++)
                {
                    sb.Append($"{map.LandZ(x, y),5}");
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("--- Bereich / area: Land-ID (hex) ---");
            for (int y = area.Top; y < area.Bottom; y++)
            {
                sb.Append($"{y,5} ");
                for (int x = area.Left; x < area.Right; x++)
                {
                    sb.Append($" {map.LandId(x, y):x4}");
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("--- Bereich / area: Statics (x, y, z, id, hue, Land-z) ---");
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    foreach (StaticItem s in map.StaticsAt(x, y))
                    {
                        sb.AppendLine($"({x},{y}) z {s.Z,4}  {s.Id:x4} {UoArtCache.ItemName(s.Id),-18} hue {s.Hue}  land z {map.LandZ(x, y)}");
                    }
                }
            }
        }
    }
}
