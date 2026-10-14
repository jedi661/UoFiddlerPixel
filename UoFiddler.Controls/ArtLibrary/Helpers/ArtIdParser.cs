// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Globalization;

namespace UoFiddler.Controls.ArtLibrary.Helpers
{
    /// <summary>
    /// Parses and formats graphic ids in decimal and hexadecimal notation.
    /// <para>Accepted input:</para>
    /// <code>
    /// 6699        decimal
    /// 0x1A2B      hex (prefix 0x)
    /// $1A2B       hex (prefix $)
    /// 1A2Bh       hex (suffix h)
    /// 1A2B        hex (contains A-F)
    /// </code>
    /// </summary>
    public static class ArtIdParser
    {
        public static bool TryParse(string text, out int id)
        {
            id = -1;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string value = text.Trim().Replace("_", string.Empty).Replace(" ", string.Empty);
            bool hex = false;

            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(2);
                hex = true;
            }
            else if (value.StartsWith("$", StringComparison.Ordinal))
            {
                value = value.Substring(1);
                hex = true;
            }
            else if (value.EndsWith("h", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0, value.Length - 1);
                hex = true;
            }
            else if (value.IndexOfAny("abcdefABCDEF".ToCharArray()) >= 0)
            {
                hex = true;
            }
            else
            {
                // allow thousands separators in decimal input (1.000.000 or 1,000,000)
                value = value.Replace(".", string.Empty).Replace(",", string.Empty);
            }

            if (value.Length == 0)
            {
                return false;
            }

            bool ok = hex
                ? int.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out id)
                : int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id);

            if (!ok || id < 0)
            {
                id = -1;
                return false;
            }

            return true;
        }

        /// <summary>Hex notation with at least 4 digits, e.g. 0x04D2.</summary>
        public static string ToHex(int id) => $"0x{id:X4}";

        /// <summary>Both notations, e.g. 1234 (0x04D2).</summary>
        public static string ToDisplay(int id) => $"{id} ({ToHex(id)})";
    }
}
