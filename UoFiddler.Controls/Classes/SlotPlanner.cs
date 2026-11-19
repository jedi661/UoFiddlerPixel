using System;
using System.Collections.Generic;

namespace UoFiddler.Controls.Classes
{
    /// <summary>How the target ids of several graphics are chosen.</summary>
    public enum SlotPlacementMode
    {
        /// <summary>Exactly from the start address on: start, start+1, ... (used slots would be replaced).</summary>
        Consecutive = 0,

        /// <summary>The next free slots from the start address on, used ones are skipped (may have gaps).</summary>
        NextFree = 1,

        /// <summary>The first block of N free slots in a row from the start address on.</summary>
        FreeBlock = 2
    }

    /// <summary>
    /// Finds target ids for a number of graphics. GDI free: the "is free" / "is a legal id" checks are passed in
    /// (Art.IsValidStatic / Art.IsLegalStaticId in the tool, simple sets in tests).
    /// </summary>
    public sealed class SlotPlanner
    {
        private readonly Func<int, bool> _isFree;
        private readonly Func<int, bool> _isLegal;
        private readonly int _maxId;

        public SlotPlanner(Func<int, bool> isFree, Func<int, bool> isLegal, int maxId)
        {
            _isFree = isFree;
            _isLegal = isLegal;
            _maxId = maxId;
        }

        private bool Free(int id) => id >= 0 && id <= _maxId && _isLegal(id) && _isFree(id);

        /// <summary>Target ids for <paramref name="count"/> graphics, or null if there is no room.</summary>
        public List<int> Plan(SlotPlacementMode mode, int start, int count)
        {
            start = Math.Max(0, start);
            switch (mode)
            {
                case SlotPlacementMode.Consecutive:
                {
                    var ids = new List<int>();
                    for (int id = start; ids.Count < count && id <= _maxId; ++id)
                    {
                        if (!_isLegal(id))
                        {
                            return null; // the row would cross ids that can not hold art (gap before the extension)
                        }

                        ids.Add(id);
                    }

                    return ids.Count == count ? ids : null;
                }

                case SlotPlacementMode.NextFree:
                {
                    var ids = new List<int>();
                    for (int id = start; ids.Count < count && id <= _maxId; ++id)
                    {
                        if (Free(id))
                        {
                            ids.Add(id);
                        }
                    }

                    return ids.Count == count ? ids : null;
                }

                default:
                {
                    int block = NextBlock(start - 1, count);
                    return block < 0 ? null : Range(block, count);
                }
            }
        }

        /// <summary>First id of the next block of <paramref name="count"/> free slots in a row AFTER <paramref name="after"/> (-1 = none).</summary>
        public int NextBlock(int after, int count)
        {
            int run = 0;
            for (int id = Math.Max(0, after + 1); id <= _maxId; ++id)
            {
                run = Free(id) ? run + 1 : 0;
                if (run == count)
                {
                    return id - count + 1;
                }
            }

            return -1;
        }

        /// <summary>First id of the previous block of <paramref name="count"/> free slots in a row BEFORE <paramref name="before"/> (-1 = none).</summary>
        public int PreviousBlock(int before, int count)
        {
            int run = 0;
            for (int id = Math.Min(_maxId, before - 1); id >= 0; --id)
            {
                run = Free(id) ? run + 1 : 0;
                if (run == count)
                {
                    return id; // lowest id of the block
                }
            }

            return -1;
        }

        public static List<int> Range(int start, int count)
        {
            var ids = new List<int>(count);
            for (int i = 0; i < count; ++i)
            {
                ids.Add(start + i);
            }

            return ids;
        }
    }
}
