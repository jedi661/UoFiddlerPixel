/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Ultima;

namespace UoFiddler.Controls.MapViewer.Editor.Transitions
{
    public sealed class TransitionResult
    {
        public List<(int X, int Y, ushort Id, sbyte Z)> Land { get; } = new List<(int, int, ushort, sbyte)>();

        /// <summary>B cells that were filled with A (A on opposite sides or on three sides)</summary>
        public int Filled { get; set; }

        /// <summary>cells that need a graphic of an empty slot (unchanged)</summary>
        public Dictionary<TransitionSlot, int> Missing { get; } = new Dictionary<TransitionSlot, int>();
    }

    /// <summary>
    /// Sets the transition graphics between terrain A and B automatically.
    /// The graphics are placed on the B cells next to A (like the ring around A in UO-Caeri),
    /// inner corners on B cells with A on two sides. Heights are not changed.
    /// </summary>
    public static class TransitionEngine
    {
        private enum Kind : byte
        {
            Other,
            A,
            B
        }

        /// <summary>
        /// Calculates the land changes inside <paramref name="area"/> (the cells around it are only read)
        /// </summary>
        /// <param name="overrides">land ids that are used instead of the map (e.g. not applied paint), can be null</param>
        public static TransitionResult Compute(Map map, Rectangle area, TransitionSet set, IReadOnlyList<ushort> idsA, IReadOnlyList<ushort> idsB,
            IDictionary<Point, ushort> overrides = null)
        {
            var result = new TransitionResult();
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (area.IsEmpty || idsA.Count == 0)
            {
                return result;
            }

            var setA = new HashSet<ushort>(idsA);
            var setB = new HashSet<ushort>(idsB);
            HashSet<ushort> transitionTiles = set.AllTiles();
            transitionTiles.ExceptWith(setA);

            // working grid with one cell margin
            var grid = new Rectangle(area.X - 1, area.Y - 1, area.Width + 2, area.Height + 2);
            int gw = grid.Width, gh = grid.Height;
            var ids = new ushort[gw * gh];
            var zs = new sbyte[gw * gh];
            var kinds = new Kind[gw * gh];
            var original = new ushort[gw * gh];
            TileMatrix tiles = map.Tiles;

            for (int gy = 0; gy < gh; gy++)
            {
                for (int gx = 0; gx < gw; gx++)
                {
                    int mx = grid.X + gx, my = grid.Y + gy, i = (gy * gw) + gx;
                    if (mx < 0 || my < 0 || mx >= map.Width || my >= map.Height)
                    {
                        kinds[i] = Kind.Other;
                        continue;
                    }

                    Tile t = tiles.GetLandTile(mx, my);
                    ushort id = t.Id;
                    if (overrides != null && overrides.TryGetValue(new Point(mx, my), out ushort o))
                    {
                        id = o;
                    }

                    ids[i] = id;
                    original[i] = t.Id;
                    zs[i] = t.Z;
                    kinds[i] = setA.Contains(id) ? Kind.A : setB.Contains(id) || transitionTiles.Contains(id) ? Kind.B : Kind.Other;
                }
            }

            bool Inside(int gx, int gy) => gx >= 1 && gy >= 1 && gx <= gw - 2 && gy <= gh - 2;
            bool IsA(int gx, int gy) => gx >= 0 && gy >= 0 && gx < gw && gy < gh && kinds[(gy * gw) + gx] == Kind.A;

            // 1. B cells with A on opposite sides or on three sides can not get a graphic: they become A
            for (int pass = 0; pass < 16; pass++)
            {
                bool changed = false;
                for (int gy = 1; gy < gh - 1; gy++)
                {
                    for (int gx = 1; gx < gw - 1; gx++)
                    {
                        int i = (gy * gw) + gx;
                        if (kinds[i] != Kind.B)
                        {
                            continue;
                        }

                        bool n = IsA(gx, gy - 1), s = IsA(gx, gy + 1), w = IsA(gx - 1, gy), e = IsA(gx + 1, gy);
                        int count = (n ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0) + (e ? 1 : 0);
                        bool oppositeDiagonals = !n && !s && !w && !e &&
                                                 ((IsA(gx - 1, gy - 1) && IsA(gx + 1, gy + 1) && !IsA(gx + 1, gy - 1) && !IsA(gx - 1, gy + 1)) ||
                                                  (IsA(gx + 1, gy - 1) && IsA(gx - 1, gy + 1) && !IsA(gx - 1, gy - 1) && !IsA(gx + 1, gy + 1)));
                        if (count >= 3 || (n && s) || (w && e) || oppositeDiagonals)
                        {
                            kinds[i] = Kind.A;
                            ids[i] = Pick(idsA, grid.X + gx, grid.Y + gy);
                            result.Filled++;
                            changed = true;
                        }
                    }
                }

                if (!changed)
                {
                    break;
                }
            }

            // 2. graphics for the B cells
            for (int gy = 1; gy < gh - 1; gy++)
            {
                for (int gx = 1; gx < gw - 1; gx++)
                {
                    int i = (gy * gw) + gx;
                    if (kinds[i] != Kind.B)
                    {
                        continue;
                    }

                    int mx = grid.X + gx, my = grid.Y + gy;
                    TransitionSlot? slot = SlotFor(
                        IsA(gx, gy - 1), IsA(gx + 1, gy), IsA(gx, gy + 1), IsA(gx - 1, gy),
                        IsA(gx - 1, gy - 1), IsA(gx + 1, gy - 1), IsA(gx + 1, gy + 1), IsA(gx - 1, gy + 1));

                    if (slot == null)
                    {
                        // no A around: a transition graphic left from before becomes B again
                        if (transitionTiles.Contains(ids[i]) && !setB.Contains(ids[i]) && idsB.Count > 0)
                        {
                            ids[i] = Pick(idsB, mx, my);
                        }

                        continue;
                    }

                    List<ushort> variants = set.Slots[slot.Value];
                    if (variants.Count == 0)
                    {
                        result.Missing.TryGetValue(slot.Value, out int c);
                        result.Missing[slot.Value] = c + 1;
                        continue;
                    }

                    ids[i] = Pick(variants, mx, my);
                }
            }

            for (int gy = 1; gy < gh - 1; gy++)
            {
                for (int gx = 1; gx < gw - 1; gx++)
                {
                    int i = (gy * gw) + gx;
                    if (Inside(gx, gy) && ids[i] != original[i])
                    {
                        result.Land.Add((grid.X + gx, grid.Y + gy, ids[i], zs[i]));
                    }
                }
            }

            tiles.CloseStreams();
            return result;
        }

        /// <summary>Graphic position of a B cell from the A neighbors (null = no A around)</summary>
        public static TransitionSlot? SlotFor(bool n, bool e, bool s, bool w, bool nw, bool ne, bool se, bool sw)
        {
            // two sides: inner corner (the name says where A is)
            if (s && e)
            {
                return TransitionSlot.InnerSE;
            }

            if (s && w)
            {
                return TransitionSlot.InnerSW;
            }

            if (n && w)
            {
                return TransitionSlot.InnerNW;
            }

            if (n && e)
            {
                return TransitionSlot.InnerNE;
            }

            // one side: edge (the name says where the cell is relative to A)
            if (s)
            {
                return TransitionSlot.EdgeN;
            }

            if (n)
            {
                return TransitionSlot.EdgeS;
            }

            if (e)
            {
                return TransitionSlot.EdgeW;
            }

            if (w)
            {
                return TransitionSlot.EdgeE;
            }

            // only diagonal
            if (se && sw)
            {
                return TransitionSlot.EdgeN;
            }

            if (ne && nw)
            {
                return TransitionSlot.EdgeS;
            }

            if (ne && se)
            {
                return TransitionSlot.EdgeW;
            }

            if (nw && sw)
            {
                return TransitionSlot.EdgeE;
            }

            if (se)
            {
                return TransitionSlot.CornerNW;
            }

            if (sw)
            {
                return TransitionSlot.CornerNE;
            }

            if (nw)
            {
                return TransitionSlot.CornerSE;
            }

            if (ne)
            {
                return TransitionSlot.CornerSW;
            }

            return null;
        }

        /// <summary>Variant by position (the same cell always gets the same graphic)</summary>
        public static ushort Pick(IReadOnlyList<ushort> ids, int x, int y)
        {
            if (ids.Count == 1)
            {
                return ids[0];
            }

            unchecked
            {
                uint h = (uint)((x * 73856093) ^ (y * 19349663));
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                return ids[(int)(h % (uint)ids.Count)];
            }
        }
    }
}
