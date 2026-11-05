// /***************************************************************************
//  *
//  * Dragon Map Studio – finds typical errors of a built map
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum MapProblemKind
    {
        MissingLandArt,
        MissingStaticArt,
        Cliff,
        StaticBelowGround,
        DuplicateStatic
    }

    public sealed class MapProblem
    {
        public MapProblemKind Kind;
        public int X;
        public int Y;
        public string Text;

        public override string ToString() => $"({X}, {Y})  {Text}";
    }

    public sealed class ProblemScanOptions
    {
        public int CliffHeight = 40;
        public int BelowGround = 5;
        public int MaxResults = 5000;
    }

    /// <summary>Checks land and statics for things that look wrong in the client.</summary>
    public static class MapProblemScanner
    {
        public static List<MapProblem> Scan(MapData map, ProblemScanOptions o, IProgress<int> progress, CancellationToken cancel)
        {
            var list = new List<MapProblem>();
            var landChecked = new Dictionary<ushort, bool>();
            var staticChecked = new Dictionary<ushort, bool>();
            int w = map.Width, h = map.Height;
            var seen = new HashSet<(int, int, ushort, int)>();
            bool checkArt = UoArtCache.ArtAvailableCached; // without client files every tile would be "missing"

            void Add(MapProblemKind k, int x, int y, string text)
            {
                if (list.Count < o.MaxResults)
                {
                    list.Add(new MapProblem { Kind = k, X = x, Y = y, Text = text });
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int i = y * w + x;
                    ushort land = map.Tiles[i];
                    if (!landChecked.TryGetValue(land, out bool ok))
                    {
                        ok = UoArtCache.IsValidLand(land);
                        landChecked[land] = ok;
                    }
                    if (!ok && checkArt)
                    {
                        Add(MapProblemKind.MissingLandArt, x, y, $"Land-Tile {land:x4} hat keine Grafik");
                    }
                    int z = map.Z[i];
                    if (x + 1 < w && Math.Abs(map.Z[i + 1] - z) >= o.CliffHeight)
                    {
                        Add(MapProblemKind.Cliff, x, y, $"Steilkante nach Osten: {z} → {map.Z[i + 1]}");
                    }
                    if (y + 1 < h && Math.Abs(map.Z[i + w] - z) >= o.CliffHeight)
                    {
                        Add(MapProblemKind.Cliff, x, y, $"Steilkante nach Süden: {z} → {map.Z[i + w]}");
                    }
                }
                if ((y & 63) == 0)
                {
                    cancel.ThrowIfCancellationRequested();
                    progress?.Report(y * 60 / h);
                }
            }

            int n = 0, total = Math.Max(1, map.StaticCount);
            int lastX = -1, lastY = -1;
            foreach (StaticItem s in map.AllStatics())
            {
                if (s.X >> 3 != lastX || s.Y >> 3 != lastY)
                {
                    seen.Clear(); // statics are stored per 8×8 block
                    lastX = s.X >> 3;
                    lastY = s.Y >> 3;
                }
                if (!staticChecked.TryGetValue(s.Id, out bool ok))
                {
                    ok = UoArtCache.IsValidStatic(s.Id);
                    staticChecked[s.Id] = ok;
                }
                if (!ok && checkArt)
                {
                    Add(MapProblemKind.MissingStaticArt, s.X, s.Y, $"Static {s.Id:x4} hat keine Grafik");
                }
                int landZ = map.LandZ(s.X, s.Y);
                if (s.Z < landZ - o.BelowGround)
                {
                    Add(MapProblemKind.StaticBelowGround, s.X, s.Y, $"Static {s.Id:x4} liegt im Boden (z {s.Z}, Boden {landZ})");
                }
                if (!seen.Add((s.X, s.Y, s.Id, s.Z)))
                {
                    Add(MapProblemKind.DuplicateStatic, s.X, s.Y, $"Static {s.Id:x4} doppelt auf z {s.Z}");
                }
                if ((++n & 0xFFFF) == 0)
                {
                    cancel.ThrowIfCancellationRequested();
                    progress?.Report(60 + (int)(40L * n / total));
                }
            }
            return list;
        }
    }
}
