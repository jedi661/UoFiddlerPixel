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
using System.Globalization;
using System.Linq;

namespace UoFiddler.Controls.MapViewer.Editor.Transitions
{
    /// <summary>
    /// A group of land tiles that count as the same terrain (e.g. Sand = 0x16..0x19)
    /// </summary>
    public sealed class TerrainGroup
    {
        public string Name { get; set; } = string.Empty;
        public List<ushort> Ids { get; } = new List<ushort>();

        public override string ToString() => $"{Name} ({Ids.Count})";
    }

    /// <summary>
    /// Positions of the transition graphics. Like the "Neue Transition" window of UO-Caeri:
    /// <list type="bullet">
    /// <item>Edge* / Corner*: ring around the source texture A (row 1 = edges 1-1..1-4, row 2 = corners 2-1..2-4).
    /// The name is the position of the cell relative to A: EdgeN = the cell north of A (A lies at its south side).</item>
    /// <item>Inner*: row 3 around the target texture B (3-1..3-4): a cell with A on two sides,
    /// InnerSE = A lies south and east of the cell (B only remains in its north west corner).</item>
    /// <item>BEdge*: optional edges around B (stored, not used by the automatic yet).</item>
    /// </list>
    /// </summary>
    public enum TransitionSlot
    {
        EdgeN,
        EdgeE,
        EdgeS,
        EdgeW,
        CornerNW,
        CornerNE,
        CornerSE,
        CornerSW,
        InnerSE,
        InnerSW,
        InnerNW,
        InnerNE,
        BEdgeN,
        BEdgeE,
        BEdgeS,
        BEdgeW
    }

    /// <summary>
    /// Transition from the terrain A (inside, e.g. sand) to the terrain B (outside, e.g. grass).
    /// Every slot can have several graphics (variants, the "2. Reihe"), one of them is chosen by position.
    /// </summary>
    public sealed class TransitionSet
    {
        public static readonly TransitionSlot[] AllSlots = (TransitionSlot[])Enum.GetValues(typeof(TransitionSlot));

        public string Name { get; set; } = string.Empty;

        /// <summary>terrain inside (the "script texture")</summary>
        public string GroupA { get; set; } = string.Empty;

        /// <summary>terrain outside (the "target texture")</summary>
        public string GroupB { get; set; } = string.Empty;

        public Dictionary<TransitionSlot, List<ushort>> Slots { get; } = AllSlots.ToDictionary(s => s, _ => new List<ushort>());

        public string Notes { get; set; } = string.Empty;

        /// <summary>Graphic of a slot/variant or null</summary>
        public ushort? Get(TransitionSlot slot, int variant)
        {
            List<ushort> list = Slots[slot];
            return variant >= 0 && variant < list.Count ? list[variant] : (ushort?)null;
        }

        /// <summary>Sets a variant (the list is filled up, removing the last variant shortens it)</summary>
        public void Set(TransitionSlot slot, int variant, ushort? id)
        {
            List<ushort> list = Slots[slot];
            if (id.HasValue)
            {
                if (variant < list.Count)
                {
                    list[variant] = id.Value;
                }
                else
                {
                    list.Add(id.Value);
                }
            }
            else if (variant < list.Count)
            {
                list.RemoveAt(variant);
            }
        }

        /// <summary>All graphics of the set (they count as terrain B when transitions are calculated again)</summary>
        public HashSet<ushort> AllTiles()
        {
            return new HashSet<ushort>(Slots.Values.SelectMany(l => l));
        }

        /// <summary>Slots of the automatic (edges, corners, inner corners) that have no graphic</summary>
        public List<TransitionSlot> MissingSlots()
        {
            return AllSlots.Where(s => s < TransitionSlot.BEdgeN && Slots[s].Count == 0).ToList();
        }

        public TransitionSet Clone()
        {
            var copy = new TransitionSet { Name = Name, GroupA = GroupA, GroupB = GroupB, Notes = Notes };
            foreach (KeyValuePair<TransitionSlot, List<ushort>> pair in Slots)
            {
                copy.Slots[pair.Key].AddRange(pair.Value);
            }

            return copy;
        }

        /// <summary>Label like in UO-Caeri (1-1 .. 3-4)</summary>
        public static string Label(TransitionSlot slot)
        {
            return slot switch
            {
                TransitionSlot.EdgeN => "1-1",
                TransitionSlot.EdgeE => "1-2",
                TransitionSlot.EdgeS => "1-3",
                TransitionSlot.EdgeW => "1-4",
                TransitionSlot.CornerNW => "2-1",
                TransitionSlot.CornerNE => "2-2",
                TransitionSlot.CornerSE => "2-3",
                TransitionSlot.CornerSW => "2-4",
                TransitionSlot.InnerSE => "3-1",
                TransitionSlot.InnerSW => "3-2",
                TransitionSlot.InnerNW => "3-3",
                TransitionSlot.InnerNE => "3-4",
                _ => "optional"
            };
        }

        public static string Describe(TransitionSlot slot)
        {
            return slot switch
            {
                TransitionSlot.EdgeN => "Edge: cell north of A (A at its south side)",
                TransitionSlot.EdgeE => "Edge: cell east of A (A at its west side)",
                TransitionSlot.EdgeS => "Edge: cell south of A (A at its north side)",
                TransitionSlot.EdgeW => "Edge: cell west of A (A at its east side)",
                TransitionSlot.CornerNW => "Outer corner: cell north-west of A",
                TransitionSlot.CornerNE => "Outer corner: cell north-east of A",
                TransitionSlot.CornerSE => "Outer corner: cell south-east of A",
                TransitionSlot.CornerSW => "Outer corner: cell south-west of A",
                TransitionSlot.InnerSE => "Inner corner: A south and east of the cell (B left in the north-west corner)",
                TransitionSlot.InnerSW => "Inner corner: A south and west of the cell (B left in the north-east corner)",
                TransitionSlot.InnerNW => "Inner corner: A north and west of the cell (B left in the south-east corner)",
                TransitionSlot.InnerNE => "Inner corner: A north and east of the cell (B left in the south-west corner)",
                _ => "Optional edge around B (stored, not used by the automatic)"
            };
        }

        public override string ToString() => string.IsNullOrEmpty(Name) ? $"{GroupA} -> {GroupB}" : Name;

        #region Id lists

        public static string FormatIds(IEnumerable<ushort> ids)
        {
            return string.Join(",", ids.Select(i => $"0x{i:X4}"));
        }

        /// <summary>Reads "0x16,0x17" / "16 17" (hex with 0x or letters, otherwise decimal) / ranges "0x16-0x19"</summary>
        public static List<ushort> ParseIds(string text, bool defaultHex = false)
        {
            var result = new List<ushort>();
            if (string.IsNullOrWhiteSpace(text))
            {
                return result;
            }

            foreach (string raw in text.Split(new[] { ',', ';', ' ', '\t', ':' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string part = raw.Trim();
                int dash = part.IndexOf('-', 1);
                if (dash > 0)
                {
                    if (TryParse(part.Substring(0, dash), defaultHex, out int a) && TryParse(part.Substring(dash + 1), defaultHex, out int b))
                    {
                        for (int i = Math.Min(a, b); i <= Math.Max(a, b) && i <= ushort.MaxValue; i++)
                        {
                            result.Add((ushort)i);
                        }

                        continue;
                    }

                    throw new FormatException($"Invalid range: {part}");
                }

                if (!TryParse(part, defaultHex, out int v) || v < 0 || v > ushort.MaxValue)
                {
                    throw new FormatException($"Invalid id: {part}");
                }

                result.Add((ushort)v);
            }

            return result;
        }

        private static bool TryParse(string s, bool defaultHex, out int value)
        {
            s = s.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }

            if (defaultHex || s.Any(c => char.IsLetter(c)))
            {
                return int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }

            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        #endregion
    }
}
