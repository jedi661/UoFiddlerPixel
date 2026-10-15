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

using System.Drawing;
using System.Globalization;
using Ultima.Helpers;
using UoFiddler.Controls.ArtLibrary.Core;

namespace UoFiddler.Controls.TerrainLibrary.Helpers
{
    /// <summary>
    /// Radar colors: 16 bit values like radarcol.mul (5 bit red/green/blue), conversion to/from
    /// System.Drawing.Color and text, and the average color of a graphic (like "Mean Color" in the
    /// RadarColor tab).
    /// </summary>
    public static class TerrainColor
    {
        public static ushort FromColor(Color color) => HueHelpers.ColorToHue(color);

        public static Color ToColor(ushort value) => HueHelpers.HueToColor(value);

        /// <summary>"0x1234" - the value as stored in radarcol.mul.</summary>
        public static string ToHex(ushort value) => $"0x{value:X4}";

        /// <summary>"#RRGGBB".</summary>
        public static string ToHtml(ushort value)
        {
            Color c = ToColor(value);
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        /// <summary>
        /// Reads "0x1234" (radarcol value), "#RRGGBB" / "RRGGBB" (web color) or a decimal radarcol value.
        /// </summary>
        public static bool TryParse(string text, out ushort value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();
            if (text.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase))
            {
                return ushort.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }

            string web = text.TrimStart('#');
            if ((text.StartsWith("#") || web.Length == 6) && web.Length == 6
                && int.TryParse(web, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgb))
            {
                value = FromColor(Color.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF));
                return true;
            }

            return ushort.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>Average color of all opaque pixels, or null for an empty graphic.</summary>
        public static Color? Average(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }

            ushort[] pixels = ArtLibraryCodec.ToPixels(bitmap);
            long r = 0, g = 0, b = 0, count = 0;
            foreach (ushort pixel in pixels)
            {
                if ((pixel & 0x8000) == 0)
                {
                    continue;
                }

                r += (pixel >> 10) & 0x1F;
                g += (pixel >> 5) & 0x1F;
                b += pixel & 0x1F;
                ++count;
            }

            if (count == 0)
            {
                return null;
            }

            return Color.FromArgb((int)(r * 255 / 31 / count), (int)(g * 255 / 31 / count), (int)(b * 255 / 31 / count));
        }

        public static ushort? AverageValue(Bitmap bitmap)
        {
            Color? color = Average(bitmap);
            return color == null ? (ushort?)null : FromColor(color.Value);
        }
    }
}
