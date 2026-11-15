/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Undo / redo and "changed since loading" for the TileData tab.
 *
 * Every change stores the old values of the touched item / land entries
 * (not the whole table), so even changes of thousands of tiles are cheap.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    public sealed class TileDataHistoryStep
    {
        public string Description { get; set; }
        public DateTime Time { get; set; }
        internal Dictionary<int, ItemData> Items { get; } = new Dictionary<int, ItemData>();
        internal Dictionary<int, LandData> Lands { get; } = new Dictionary<int, LandData>();

        public IEnumerable<int> ItemIndices => Items.Keys;
        public IEnumerable<int> LandIndices => Lands.Keys;
        public int Count => Items.Count + Lands.Count;

        public override string ToString() => $"{Time:HH:mm:ss}  {Description}";
    }

    public static class TileDataHistory
    {
        private static readonly List<TileDataHistoryStep> _undo = new List<TileDataHistoryStep>();
        private static readonly List<TileDataHistoryStep> _redo = new List<TileDataHistoryStep>();

        private static string _lastKey;
        private static DateTime _lastTime;

        // state of the file (after loading / saving) for "changed tiles" and "revert"
        private static ItemData[] _baseItems;
        private static LandData[] _baseLands;

        public static int Limit { get; set; } = 200;

        /// <summary>raised after every change of the undo / redo lists</summary>
        public static event EventHandler Changed;

        public static bool CanUndo => _undo.Count > 0;
        public static bool CanRedo => _redo.Count > 0;
        public static string UndoText => _undo.LastOrDefault()?.Description;
        public static string RedoText => _redo.LastOrDefault()?.Description;
        public static IReadOnlyList<TileDataHistoryStep> UndoSteps => _undo;
        public static IReadOnlyList<TileDataHistoryStep> RedoSteps => _redo;

        /// <summary>
        /// Call BEFORE a change: stores the current values of the given entries.
        /// Several changes with the same <paramref name="coalesceKey"/> within 2 seconds
        /// (e.g. typing in a text box) are one undo step.
        /// </summary>
        public static void Record(string description, IEnumerable<int> items, IEnumerable<int> lands, string coalesceKey = null)
        {
            if (TileData.ItemTable == null || TileData.LandTable == null)
            {
                return;
            }

            if (coalesceKey != null && coalesceKey == _lastKey && (DateTime.Now - _lastTime).TotalSeconds < 2 && _undo.Count > 0)
            {
                _lastTime = DateTime.Now;
                return;
            }

            var step = new TileDataHistoryStep { Description = description, Time = DateTime.Now };
            foreach (int i in items ?? Enumerable.Empty<int>())
            {
                if (i >= 0 && i < TileData.ItemTable.Length && !step.Items.ContainsKey(i))
                {
                    step.Items[i] = TileData.ItemTable[i];
                }
            }

            foreach (int i in lands ?? Enumerable.Empty<int>())
            {
                if (i >= 0 && i < TileData.LandTable.Length && !step.Lands.ContainsKey(i))
                {
                    step.Lands[i] = TileData.LandTable[i];
                }
            }

            if (step.Count == 0)
            {
                return;
            }

            _undo.Add(step);
            if (_undo.Count > Limit)
            {
                _undo.RemoveAt(0);
            }

            _redo.Clear();
            _lastKey = coalesceKey;
            _lastTime = DateTime.Now;
            Changed?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>the next change is a new undo step even with the same key</summary>
        public static void BreakCoalescing() => _lastKey = null;

        /// <summary>undoes the last step, returns it (for refreshing the view) or null</summary>
        public static TileDataHistoryStep Undo() => Swap(_undo, _redo);

        public static TileDataHistoryStep Redo() => Swap(_redo, _undo);

        private static TileDataHistoryStep Swap(List<TileDataHistoryStep> from, List<TileDataHistoryStep> to)
        {
            if (from.Count == 0 || TileData.ItemTable == null)
            {
                return null;
            }

            TileDataHistoryStep step = from[from.Count - 1];
            from.RemoveAt(from.Count - 1);

            var back = new TileDataHistoryStep { Description = step.Description, Time = step.Time };
            foreach (KeyValuePair<int, ItemData> pair in step.Items)
            {
                back.Items[pair.Key] = TileData.ItemTable[pair.Key];
                TileData.ItemTable[pair.Key] = pair.Value;
            }

            foreach (KeyValuePair<int, LandData> pair in step.Lands)
            {
                back.Lands[pair.Key] = TileData.LandTable[pair.Key];
                TileData.LandTable[pair.Key] = pair.Value;
            }

            to.Add(back);
            _lastKey = null;
            Changed?.Invoke(null, EventArgs.Empty);
            return back;
        }

        public static void Clear()
        {
            _undo.Clear();
            _redo.Clear();
            _lastKey = null;
            Changed?.Invoke(null, EventArgs.Empty);
        }

        #region State of the file

        /// <summary>remembers the current tables as "state of the file" (after loading or saving)</summary>
        public static void SetBaseline()
        {
            _baseItems = TileData.ItemTable == null ? null : (ItemData[])TileData.ItemTable.Clone();
            _baseLands = TileData.LandTable == null ? null : (LandData[])TileData.LandTable.Clone();
        }

        public static bool HasBaseline => _baseItems != null && _baseLands != null;

        public static bool SameItem(ItemData a, ItemData b) =>
            a.Flags == b.Flags && a.Animation == b.Animation && a.Weight == b.Weight && a.Quality == b.Quality && a.Quantity == b.Quantity &&
            a.Value == b.Value && a.Hue == b.Hue && a.StackingOffset == b.StackingOffset && a.Height == b.Height && a.MiscData == b.MiscData &&
            a.Unk2 == b.Unk2 && a.Unk3 == b.Unk3 && string.Equals(a.Name ?? string.Empty, b.Name ?? string.Empty, StringComparison.Ordinal);

        public static bool SameLand(LandData a, LandData b) =>
            a.Flags == b.Flags && a.TextureId == b.TextureId && string.Equals(a.Name ?? string.Empty, b.Name ?? string.Empty, StringComparison.Ordinal);

        public static IEnumerable<int> ChangedItems()
        {
            if (_baseItems == null || TileData.ItemTable == null)
            {
                yield break;
            }

            int count = Math.Min(_baseItems.Length, TileData.ItemTable.Length);
            for (int i = 0; i < count; i++)
            {
                if (!SameItem(_baseItems[i], TileData.ItemTable[i]))
                {
                    yield return i;
                }
            }
        }

        public static IEnumerable<int> ChangedLands()
        {
            if (_baseLands == null || TileData.LandTable == null)
            {
                yield break;
            }

            int count = Math.Min(_baseLands.Length, TileData.LandTable.Length);
            for (int i = 0; i < count; i++)
            {
                if (!SameLand(_baseLands[i], TileData.LandTable[i]))
                {
                    yield return i;
                }
            }
        }

        public static bool TryGetOriginalItem(int index, out ItemData data)
        {
            data = default;
            if (_baseItems == null || index < 0 || index >= _baseItems.Length)
            {
                return false;
            }

            data = _baseItems[index];
            return true;
        }

        public static bool TryGetOriginalLand(int index, out LandData data)
        {
            data = default;
            if (_baseLands == null || index < 0 || index >= _baseLands.Length)
            {
                return false;
            }

            data = _baseLands[index];
            return true;
        }

        /// <summary>sets the given entries back to the state of the file (one undo step)</summary>
        public static int Revert(IEnumerable<int> items, IEnumerable<int> lands, string description)
        {
            List<int> itemList = (items ?? Enumerable.Empty<int>()).Where(i => _baseItems != null && i >= 0 && i < _baseItems.Length).Distinct().ToList();
            List<int> landList = (lands ?? Enumerable.Empty<int>()).Where(i => _baseLands != null && i >= 0 && i < _baseLands.Length).Distinct().ToList();
            Record(description, itemList, landList);
            foreach (int i in itemList)
            {
                TileData.ItemTable[i] = _baseItems[i];
            }

            foreach (int i in landList)
            {
                TileData.LandTable[i] = _baseLands[i];
            }

            return itemList.Count + landList.Count;
        }

        #endregion
    }
}
