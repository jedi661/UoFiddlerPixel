/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Ultima;

namespace UoFiddler.Plugin.SendItem.Forms
{
    public static class StaticImages
    {
        public static string GetName(int id)
        {
            try
            {
                ItemData[] table = TileData.ItemTable;
                return table != null && id >= 0 && id < table.Length ? (table[id].Name ?? "").Trim() : "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Returns a new bitmap of the static with the hue applied (game hue number, 0 = none), or null.
        /// </summary>
        public static Bitmap GetHued(int id, int hue)
        {
            Bitmap source;
            try
            {
                source = Art.GetStatic(id);
            }
            catch
            {
                return null;
            }

            if (source == null)
            {
                return null;
            }

            var bmp = new Bitmap(source);
            if (hue > 0)
            {
                try
                {
                    bool partial = id < TileData.ItemTable.Length && (TileData.ItemTable[id].Flags & TileFlag.PartialHue) != 0;
                    Hues.GetHue(hue - 1).ApplyTo(bmp, partial);
                }
                catch
                {
                    // invalid hue: show the original colors
                }
            }

            return bmp;
        }

        /// <summary>
        /// Square thumbnail (for buttons), centered and scaled to fit.
        /// </summary>
        public static Bitmap GetThumbnail(int id, int hue, int size)
        {
            using Bitmap img = GetHued(id, hue);
            var thumb = new Bitmap(size, size);
            if (img == null)
            {
                return thumb;
            }

            using Graphics g = Graphics.FromImage(thumb);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            float scale = Math.Min(2f, Math.Min(size / (float)img.Width, size / (float)img.Height));
            int w = Math.Max(1, (int)(img.Width * scale));
            int h = Math.Max(1, (int)(img.Height * scale));
            g.DrawImage(img, (size - w) / 2, (size - h) / 2, w, h);
            return thumb;
        }
    }
}
