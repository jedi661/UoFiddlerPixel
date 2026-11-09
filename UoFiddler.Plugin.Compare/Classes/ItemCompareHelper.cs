using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    /// <summary>
    /// Result of comparing one item id of the original and the second file.
    /// </summary>
    internal enum ItemDiffState : byte
    {
        Unknown = 0,
        Same,
        BothEmpty,
        Different,
        OnlyOriginal,
        OnlySecond
    }

    internal static class ItemCompareHelper
    {
        /// <summary>
        /// Compares two bitmaps pixel by pixel (in 16bpp Argb1555, the format the client uses).
        /// Much faster than encoding both images and hashing them.
        /// </summary>
        public static unsafe bool BitmapsEqual(Bitmap a, Bitmap b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Size != b.Size)
            {
                return false;
            }

            Rectangle rect = new Rectangle(0, 0, a.Width, a.Height);
            BitmapData da = a.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            try
            {
                BitmapData db = b.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                try
                {
                    for (int y = 0; y < a.Height; ++y)
                    {
                        ReadOnlySpan<ushort> rowA = new ReadOnlySpan<ushort>((byte*)da.Scan0 + (y * da.Stride), a.Width);
                        ReadOnlySpan<ushort> rowB = new ReadOnlySpan<ushort>((byte*)db.Scan0 + (y * db.Stride), a.Width);

                        for (int x = 0; x < rowA.Length; ++x)
                        {
                            ushort pa = rowA[x];
                            ushort pb = rowB[x];
                            if (pa == pb)
                            {
                                continue;
                            }

                            // Two fully transparent pixels are equal, whatever their color bits are.
                            if ((pa & 0x8000) == 0 && (pb & 0x8000) == 0)
                            {
                                continue;
                            }

                            return false;
                        }
                    }

                    return true;
                }
                finally
                {
                    b.UnlockBits(db);
                }
            }
            finally
            {
                a.UnlockBits(da);
            }
        }

        /// <summary>
        /// Creates an independent copy of a bitmap in 16bpp Argb1555.
        /// </summary>
        public static Bitmap CloneArt(Bitmap source)
        {
            if (source == null)
            {
                return null;
            }

            return source.Clone(new Rectangle(0, 0, source.Width, source.Height), PixelFormat.Format16bppArgb1555);
        }

        public static bool ItemDataEquals(ItemData a, ItemData b)
        {
            return string.Equals(a.Name ?? string.Empty, b.Name ?? string.Empty, StringComparison.Ordinal)
                && a.Flags == b.Flags
                && a.Weight == b.Weight
                && a.Quality == b.Quality
                && a.Quantity == b.Quantity
                && a.Value == b.Value
                && a.Height == b.Height
                && a.Animation == b.Animation
                && a.Hue == b.Hue
                && a.StackingOffset == b.StackingOffset
                && a.MiscData == b.MiscData
                && a.Unk2 == b.Unk2
                && a.Unk3 == b.Unk3;
        }

        /// <summary>
        /// True when the tiledata entry contains no information at all.
        /// </summary>
        public static bool IsEmpty(ItemData d)
        {
            return string.IsNullOrEmpty(d.Name) && d.Flags == TileFlag.None && d.Height == 0 && d.Weight == 0 && d.Animation == 0;
        }

        /// <summary>
        /// Short one line description used in the preview labels.
        /// </summary>
        public static string Describe(ItemData d)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.IsNullOrEmpty(d.Name) ? "(no name)" : d.Name);
            sb.Append($"  | H:{d.Height} W:{d.Weight} Anim:{d.Animation}");

            List<string> flags = GetFlagNames(d.Flags);
            if (flags.Count > 0)
            {
                sb.Append("  | ");
                sb.Append(string.Join(", ", flags.Count > 6 ? flags.GetRange(0, 6) : flags));
                if (flags.Count > 6)
                {
                    sb.Append($" (+{flags.Count - 6})");
                }
            }
            return sb.ToString();
        }

        public static List<string> GetFlagNames(TileFlag flags)
        {
            List<string> result = new List<string>();
            foreach (TileFlag f in Enum.GetValues(typeof(TileFlag)))
            {
                if (f != TileFlag.None && (flags & f) == f)
                {
                    result.Add(f.ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// Parses "0x1A2B", "1A2B" (hex) or a decimal number depending on <paramref name="hex"/>.
        /// A "0x" prefix always means hex.
        /// </summary>
        public static bool TryParseId(string text, bool hex, out int id)
        {
            id = -1;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(2);
                hex = true;
            }

            return hex
                ? int.TryParse(text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out id) && id >= 0
                : int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out id) && id >= 0;
        }
    }
}
