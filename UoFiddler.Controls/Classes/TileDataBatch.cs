/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Batch editing of the TileData: ID ranges, field operations and flags for
 * many items / land tiles at once (used by the TileData tab and the batch
 * edit dialog).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    public enum TileField
    {
        Name,
        Weight,
        Quality,
        Animation,
        Quantity,
        Hue,
        StackingOffset,
        Value,
        Height,
        MiscData,
        Unk2,
        Unk3,
        TextureId
    }

    public enum FieldOperation
    {
        Set,
        Add,
        Subtract,
        Prefix,
        Suffix,
        Replace
    }

    public sealed class FieldChange
    {
        public TileField Field { get; set; }
        public FieldOperation Operation { get; set; }
        public string Value { get; set; }

        /// <summary>Replace: the new text</summary>
        public string With { get; set; }
    }

    public sealed class TileDataBatch
    {
        public List<FieldChange> Fields { get; } = new List<FieldChange>();
        public TileFlag SetFlags { get; set; }
        public TileFlag ClearFlags { get; set; }

        public bool IsEmpty => Fields.Count == 0 && SetFlags == TileFlag.None && ClearFlags == TileFlag.None;

        public static readonly TileField[] ItemFields =
        {
            TileField.Name, TileField.Weight, TileField.Quality, TileField.Animation, TileField.Quantity, TileField.Hue, TileField.StackingOffset,
            TileField.Value, TileField.Height, TileField.MiscData, TileField.Unk2, TileField.Unk3
        };

        public static readonly TileField[] LandFields = { TileField.Name, TileField.TextureId };

        #region ID ranges

        /// <summary>
        /// Reads "0x0B2C-0x0B40, 3000, 0x1000..0x1010" (decimal or hex, ranges with - or ..).
        /// Returns false with an error text when a part can not be read.
        /// </summary>
        public static bool TryParseRanges(string text, int max, out List<int> ids, out string error)
        {
            ids = new List<int>();
            error = null;
            var set = new SortedSet<int>();
            foreach (string rawPart in (text ?? string.Empty).Split(new[] { ',', ';', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string part = rawPart.Trim();
                string[] bounds = part.Contains("..") ? part.Split(new[] { ".." }, StringSplitOptions.None) : SplitDash(part);
                if (bounds.Length == 1)
                {
                    if (!TryParseId(bounds[0], out int id))
                    {
                        error = $"\"{part}\" is no id";
                        return false;
                    }

                    if (id >= 0 && id < max)
                    {
                        set.Add(id);
                    }

                    continue;
                }

                if (bounds.Length != 2 || !TryParseId(bounds[0], out int from) || !TryParseId(bounds[1], out int to))
                {
                    error = $"\"{part}\" is no range (e.g. 0x0B2C-0x0B40)";
                    return false;
                }

                if (from > to)
                {
                    (from, to) = (to, from);
                }

                for (int i = Math.Max(0, from); i <= to && i < max; i++)
                {
                    set.Add(i);
                }
            }

            ids = set.ToList();
            return true;
        }

        private static string[] SplitDash(string part)
        {
            // a leading '-' would be a negative number, ids are never negative
            int dash = part.IndexOf('-', 1);
            return dash < 0 ? new[] { part } : new[] { part.Substring(0, dash), part.Substring(dash + 1) };
        }

        public static bool TryParseId(string s, out int id)
        {
            s = (s ?? string.Empty).Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id);
            }

            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }

        /// <summary>"0x0B2C-0x0B40, 0x0C00" for a list of ids (ranges are joined)</summary>
        public static string FormatRanges(IEnumerable<int> ids)
        {
            var parts = new List<string>();
            int start = -1, last = -1;
            foreach (int id in ids.Distinct().OrderBy(i => i))
            {
                if (start < 0)
                {
                    start = last = id;
                    continue;
                }

                if (id == last + 1)
                {
                    last = id;
                    continue;
                }

                parts.Add(start == last ? $"0x{start:X4}" : $"0x{start:X4}-0x{last:X4}");
                start = last = id;
            }

            if (start >= 0)
            {
                parts.Add(start == last ? $"0x{start:X4}" : $"0x{start:X4}-0x{last:X4}");
            }

            return string.Join(", ", parts);
        }

        #endregion

        #region Validation / applying

        /// <summary>checks all field values; returns null or an error text</summary>
        public string Validate()
        {
            foreach (FieldChange f in Fields)
            {
                if (f.Field == TileField.Name)
                {
                    if (f.Operation == FieldOperation.Add || f.Operation == FieldOperation.Subtract)
                    {
                        return "Name: only set, prefix, suffix or replace";
                    }

                    if (f.Operation == FieldOperation.Replace && string.IsNullOrEmpty(f.Value))
                    {
                        return "Name: enter the text to replace";
                    }

                    continue;
                }

                if (f.Operation != FieldOperation.Set && f.Operation != FieldOperation.Add && f.Operation != FieldOperation.Subtract)
                {
                    return $"{f.Field}: only set, add or subtract";
                }

                if (!TryParseId(f.Value, out int v))
                {
                    return $"{f.Field}: \"{f.Value}\" is no number";
                }

                (int min, int max) = Range(f.Field);
                if (f.Operation == FieldOperation.Set && (v < min || v > max))
                {
                    return $"{f.Field}: {v} is outside {min}..{max}";
                }
            }

            if ((SetFlags & ClearFlags) != TileFlag.None)
            {
                return "A flag can not be set and removed at the same time";
            }

            return null;
        }

        private static (int Min, int Max) Range(TileField field)
        {
            switch (field)
            {
                case TileField.Animation:
                case TileField.MiscData:
                    return (short.MinValue, short.MaxValue);
                case TileField.TextureId:
                    return (0, ushort.MaxValue);
                default:
                    return (0, byte.MaxValue);
            }
        }

        private static int Calc(int old, FieldChange f)
        {
            TryParseId(f.Value, out int v);
            int result = f.Operation == FieldOperation.Add ? old + v : f.Operation == FieldOperation.Subtract ? old - v : v;
            (int min, int max) = Range(f.Field);
            return Math.Max(min, Math.Min(max, result));
        }

        private static string CalcName(string old, FieldChange f)
        {
            old = old ?? string.Empty;
            string result;
            switch (f.Operation)
            {
                case FieldOperation.Prefix:
                    result = (f.Value ?? string.Empty) + old;
                    break;
                case FieldOperation.Suffix:
                    result = old + (f.Value ?? string.Empty);
                    break;
                case FieldOperation.Replace:
                    result = string.IsNullOrEmpty(f.Value) ? old : old.Replace(f.Value, f.With ?? string.Empty);
                    break;
                default:
                    result = f.Value ?? string.Empty;
                    break;
            }

            return result.Length > 20 ? result.Substring(0, 20) : result;
        }

        public ItemData Apply(ItemData item)
        {
            foreach (FieldChange f in Fields)
            {
                switch (f.Field)
                {
                    case TileField.Name: item.Name = CalcName(item.Name, f); break;
                    case TileField.Weight: item.Weight = (byte)Calc(item.Weight, f); break;
                    case TileField.Quality: item.Quality = (byte)Calc(item.Quality, f); break;
                    case TileField.Animation: item.Animation = (short)Calc(item.Animation, f); break;
                    case TileField.Quantity: item.Quantity = (byte)Calc(item.Quantity, f); break;
                    case TileField.Hue: item.Hue = (byte)Calc(item.Hue, f); break;
                    case TileField.StackingOffset: item.StackingOffset = (byte)Calc(item.StackingOffset, f); break;
                    case TileField.Value: item.Value = (byte)Calc(item.Value, f); break;
                    case TileField.Height: item.Height = (byte)Calc(item.Height, f); break;
                    case TileField.MiscData: item.MiscData = (short)Calc(item.MiscData, f); break;
                    case TileField.Unk2: item.Unk2 = (byte)Calc(item.Unk2, f); break;
                    case TileField.Unk3: item.Unk3 = (byte)Calc(item.Unk3, f); break;
                }
            }

            item.Flags = (item.Flags | SetFlags) & ~ClearFlags;
            return item;
        }

        public LandData Apply(LandData land)
        {
            foreach (FieldChange f in Fields)
            {
                switch (f.Field)
                {
                    case TileField.Name: land.Name = CalcName(land.Name, f); break;
                    case TileField.TextureId: land.TextureId = (ushort)Calc(land.TextureId, f); break;
                }
            }

            land.Flags = (land.Flags | SetFlags) & ~ClearFlags;
            return land;
        }

        /// <summary>applies the batch to items (one undo step), returns the indices that really changed</summary>
        public List<int> ApplyToItems(IEnumerable<int> indices, string description)
        {
            List<int> list = indices.Where(i => i >= 0 && i < TileData.ItemTable.Length).Distinct().ToList();
            List<int> changed = list.Where(i => !TileDataHistory.SameItem(TileData.ItemTable[i], Apply(TileData.ItemTable[i]))).ToList();
            if (changed.Count == 0)
            {
                return changed;
            }

            TileDataHistory.Record(description, changed, null);
            foreach (int i in changed)
            {
                TileData.ItemTable[i] = Apply(TileData.ItemTable[i]);
            }

            return changed;
        }

        public List<int> ApplyToLands(IEnumerable<int> indices, string description)
        {
            List<int> list = indices.Where(i => i >= 0 && i < TileData.LandTable.Length).Distinct().ToList();
            List<int> changed = list.Where(i => !TileDataHistory.SameLand(TileData.LandTable[i], Apply(TileData.LandTable[i]))).ToList();
            if (changed.Count == 0)
            {
                return changed;
            }

            TileDataHistory.Record(description, null, changed);
            foreach (int i in changed)
            {
                TileData.LandTable[i] = Apply(TileData.LandTable[i]);
            }

            return changed;
        }

        #endregion

        /// <summary>short text of the differences between two items (changes list)</summary>
        public static string Diff(ItemData a, ItemData b)
        {
            var d = new List<string>();
            if (a.Name != b.Name)
            {
                d.Add($"Name \"{a.Name}\" → \"{b.Name}\"");
            }

            void N(string n, int x, int y)
            {
                if (x != y)
                {
                    d.Add($"{n} {x} → {y}");
                }
            }

            N("Weight", a.Weight, b.Weight);
            N("Quality", a.Quality, b.Quality);
            N("Anim", a.Animation, b.Animation);
            N("Quantity", a.Quantity, b.Quantity);
            N("Hue", a.Hue, b.Hue);
            N("StackOff", a.StackingOffset, b.StackingOffset);
            N("Value", a.Value, b.Value);
            N("Height", a.Height, b.Height);
            N("Misc", a.MiscData, b.MiscData);
            N("Unk2", a.Unk2, b.Unk2);
            N("Unk3", a.Unk3, b.Unk3);
            AddFlags(d, a.Flags, b.Flags);
            return string.Join("; ", d);
        }

        public static string Diff(LandData a, LandData b)
        {
            var d = new List<string>();
            if (a.Name != b.Name)
            {
                d.Add($"Name \"{a.Name}\" → \"{b.Name}\"");
            }

            if (a.TextureId != b.TextureId)
            {
                d.Add($"TexID {a.TextureId} → {b.TextureId}");
            }

            AddFlags(d, a.Flags, b.Flags);
            return string.Join("; ", d);
        }

        private static void AddFlags(List<string> d, TileFlag before, TileFlag after)
        {
            TileFlag added = after & ~before, removed = before & ~after;
            if (added != TileFlag.None)
            {
                d.Add("+" + string.Join(" +", FlagNames(added)));
            }

            if (removed != TileFlag.None)
            {
                d.Add("-" + string.Join(" -", FlagNames(removed)));
            }
        }

        public static IEnumerable<string> FlagNames(TileFlag flags)
        {
            foreach (TileFlag f in Enum.GetValues(typeof(TileFlag)))
            {
                if (f != TileFlag.None && (flags & f) == f)
                {
                    yield return f.ToString();
                }
            }
        }
    }
}
