/***************************************************************************
 *
 *  * Dragon Map Studio – land transitions in the CGI view
 *  *
 *  * Applies the map transition rules of the Dragon rule set (betweentrans:
 *  * grass2dirt, cave2dirt, mountain2dirt …) to already built land tiles:
 *  * every tile is mapped back to its group (palette tiles of the group, or the
 *  * tiles of a transition file of the group), then the 8 neighbours give the
 *  * A/B pattern exactly like the compiler does. Cells without a different
 *  * neighbour get a plain tile of their group again, so the tool can be run
 *  * as often as needed (e.g. after painting a road or stamping a cave).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public sealed class LandTransitionEngine
    {
        private static readonly int[] NbDx = { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static readonly int[] NbDy = { -1, -1, -1, 0, 1, 1, 1, 0 };

        private sealed class Rule
        {
            public int GroupA;
            public int GroupB;
            public Dictionary<byte, TransitionPattern> Patterns;
        }

        private readonly Dictionary<ushort, int> _groupOfTile = new Dictionary<ushort, int>();
        private readonly HashSet<ushort> _transitionTiles = new HashSet<ushort>();
        private readonly Dictionary<int, ushort[]> _baseTiles = new Dictionary<int, ushort[]>();
        private readonly Dictionary<int, List<Rule>> _rulesOf = new Dictionary<int, List<Rule>>();

        public LandTransitionEngine(DragonRuleSet rules)
        {
            // plain tiles of every group (palette)
            foreach (IGrouping<int, DragonPaletteEntry> g in rules.Palette.Where(p => p.Defined && p.Group >= 0).GroupBy(p => p.Group))
            {
                ushort[] ids = g.SelectMany(e => e.GetTileIds()).Distinct().ToArray();
                _baseTiles[g.Key] = ids;
                foreach (ushort id in ids)
                {
                    _groupOfTile.TryAdd(id, g.Key);
                }
            }
            var all = new List<Rule>();
            foreach (TransitionRule r in rules.Transitions.Where(t => t.Enabled && t.Layer == TransitionLayer.Map))
            {
                TransitionFile f = rules.FindTransitionFile(r.File);
                if (f == null)
                {
                    continue;
                }
                var dict = new Dictionary<byte, TransitionPattern>();
                foreach (TransitionPattern p in f.Patterns.Where(p => p.GetTileIds().Length > 0))
                {
                    dict.TryAdd(p.Mask, p);
                    if (r.GroupA >= 0)
                    {
                        // a transition tile belongs to the cell group of its rule
                        foreach (ushort id in p.GetTileIds())
                        {
                            _groupOfTile.TryAdd(id, r.GroupA);
                            _transitionTiles.Add(id);
                        }
                    }
                }
                if (dict.Count > 0)
                {
                    all.Add(new Rule { GroupA = r.GroupA, GroupB = r.GroupB, Patterns = dict });
                }
            }
            foreach (int g in _baseTiles.Keys.Concat(all.Where(r => r.GroupA >= 0).Select(r => r.GroupA)).Distinct())
            {
                List<Rule> l = all.Where(r => r.GroupA == g || (r.GroupA < 0 && r.GroupB != g)).ToList();
                if (l.Count > 0)
                {
                    _rulesOf[g] = l;
                }
            }
        }

        /// <summary>Tiles that are never changed (e.g. the cave entrance itself). Default: 0000–0002 and 01D3–01E7.</summary>
        public Func<ushort, bool> Keep { get; set; } = id => id <= 2 || (id >= 0x01D3 && id <= 0x01E7);

        /// <summary>Group of a land tile, -1 = unknown.</summary>
        public int GroupOf(ushort id) => _groupOfTile.TryGetValue(id, out int g) ? g : -1;

        public bool IsTransitionTile(ushort id) => _transitionTiles.Contains(id);

        /// <summary>
        /// Re-tiles every cell of <paramref name="area"/> like the Dragon compiler. Heights stay.
        /// Call inside BeginLand / EndLand for one undo step. Returns the number of changed tiles.
        /// </summary>
        public int Apply(MapData map, Rectangle area, Random rnd)
        {
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (area.IsEmpty)
            {
                return 0;
            }
            // groups before the change (the result of one cell must not influence its neighbours)
            Rectangle ring = Rectangle.Inflate(area, 1, 1);
            ring.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            var groups = new Dictionary<int, int>();
            for (int y = ring.Top; y < ring.Bottom; y++)
            {
                for (int x = ring.Left; x < ring.Right; x++)
                {
                    groups[y * map.Width + x] = GroupOf(map.LandId(x, y));
                }
            }
            int G(int x, int y, int own) => map.InBounds(x, y) && groups.TryGetValue(y * map.Width + x, out int g) && g >= 0 ? g : own;

            int changed = 0;
            var ng = new int[8];
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    ushort id = map.LandId(x, y);
                    int g = G(x, y, -1);
                    if (g < 0 || Keep(id))
                    {
                        continue;
                    }
                    bool edge = false;
                    for (int k = 0; k < 8; k++)
                    {
                        ng[k] = G(x + NbDx[k], y + NbDy[k], g);
                        edge |= ng[k] != g;
                    }
                    ushort tile = 0;
                    bool found = false;
                    if (edge && _rulesOf.TryGetValue(g, out List<Rule> rules))
                    {
                        foreach (Rule r in rules)
                        {
                            byte mask = 0;
                            for (int k = 0; k < 8; k++)
                            {
                                if (r.GroupB < 0 ? ng[k] != g : ng[k] == r.GroupB)
                                {
                                    mask |= (byte)(1 << k);
                                }
                            }
                            if (mask != 0 && r.Patterns.TryGetValue(mask, out TransitionPattern p))
                            {
                                ushort[] ids = p.GetTileIds();
                                // keep the tile when it already is one of the pattern (no reshuffling)
                                tile = Array.IndexOf(ids, id) >= 0 ? id : ids[rnd.Next(ids.Length)];
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        // no transition here: a transition tile of an earlier run becomes a plain tile of the group again
                        if (!_transitionTiles.Contains(id) || !_baseTiles.TryGetValue(g, out ushort[] plain) || plain.Length == 0)
                        {
                            continue;
                        }
                        tile = plain[rnd.Next(plain.Length)];
                    }
                    if (tile != id)
                    {
                        map.SetLand(x, y, tile, map.LandZ(x, y));
                        changed++;
                    }
                }
            }
            return changed;
        }
    }
}
