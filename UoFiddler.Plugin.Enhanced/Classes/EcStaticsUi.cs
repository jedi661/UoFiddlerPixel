/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.EcUltima.Maps;

namespace UoFiddler.Plugin.Enhanced.Classes
{
    /// <summary>
    /// Export / import of the statics of a map area with dialogs (used by the Map tab and the region viewer).
    /// </summary>
    public static class EcStaticsUi
    {
        /// <summary>Asks for a file and writes the statics of the area; returns a status text or null if cancelled.</summary>
        public static string Export(IWin32Window owner, EcFacet facet, Rectangle area, EcStaticsFormat format)
        {
            string ext = format == EcStaticsFormat.Csv ? "csv" : "txt";
            using (var dialog = new SaveFileDialog
            {
                Filter = format == EcStaticsFormat.Csv ? "CSV (*.csv)|*.csv" : "Text (*.txt)|*.txt",
                FileName = $"facet{facet.FacetId}_{area.X}_{area.Y}_{area.Width}x{area.Height}{(format == EcStaticsFormat.MultiText ? "_multi" : string.Empty)}.{ext}"
            })
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return null;
                }
                var statics = facet.GetStatics(area);
                File.WriteAllText(dialog.FileName, EcStaticsFile.Write(statics, format, area, facet.FacetId), new UTF8Encoding(false));
                return $"{statics.Count} statics exported to {dialog.FileName}";
            }
        }

        /// <summary>
        /// Asks for a file and the position, then adds the statics to the facet (optionally replacing the statics of
        /// the target area). <paramref name="tile"/> = selected tile (or -1, -1). Returns a status text or null if cancelled.
        /// </summary>
        public static string Import(IWin32Window owner, EcFacet facet, Rectangle area, Point tile, out Rectangle target)
        {
            target = Rectangle.Empty;
            using (var dialog = new OpenFileDialog { Filter = "Statics / multis (*.txt;*.csv;*.uoa;*.wsc)|*.txt;*.csv;*.uoa;*.wsc|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return null;
                }

                List<EcStaticsFileEntry> entries;
                bool absolute, centered;
                try
                {
                    entries = EcStaticsFile.Read(dialog.FileName, out absolute, out centered);
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(owner, "The file can't be read.", ex);
                    return null;
                }
                if (entries.Count == 0)
                {
                    EcDialogs.ShowError(owner, "No statics found in the file.");
                    return null;
                }

                // own CSV = map coordinates; multis = around the selected tile / the center of the area; own text = from the origin
                Point origin = absolute ? Point.Empty
                    : tile.X >= 0 ? tile
                    : centered ? new Point(area.X + (area.Width / 2), area.Y + (area.Height / 2))
                    : area.Location;
                string[] values = EcDialogs.Prompt(owner, $"Import {entries.Count} statics",
                    new[] { absolute ? "Move by x,y (the file has map coordinates):" : centered ? "Center x,y (multi coordinates):" : "Origin x,y:", "Add to z:" },
                    new[] { $"{origin.X},{origin.Y}", "0" });
                if (values == null)
                {
                    return null;
                }
                string[] xy = values[0].Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (xy.Length < 2 || !int.TryParse(xy[0], out int ox) || !int.TryParse(xy[1], out int oy) || !int.TryParse(values[1], out int dz))
                {
                    EcDialogs.ShowError(owner, "Invalid position.");
                    return null;
                }

                target = Rectangle.FromLTRB(entries.Min(e => e.X) + ox, entries.Min(e => e.Y) + oy, entries.Max(e => e.X) + ox + 1, entries.Max(e => e.Y) + oy + 1);
                DialogResult replace = MessageBox.Show(owner,
                    $"Remove the statics in the target area {target.X},{target.Y} – {target.Right - 1},{target.Bottom - 1} first?\r\n\r\nYes = replace, No = add to the existing statics",
                    "Import statics", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (replace == DialogResult.Cancel)
                {
                    return null;
                }

                int removed = replace == DialogResult.Yes ? facet.ClearStatics(target) : 0;
                int added = 0, outside = 0;
                foreach (EcStaticsFileEntry e in entries)
                {
                    var st = new EcStatic { ItemId = e.ItemId, Hue = e.Hue, Z = (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, e.Z + dz)) };
                    if (facet.AddStatic(e.X + ox, e.Y + oy, st))
                    {
                        added++;
                    }
                    else
                    {
                        outside++;
                    }
                }
                return $"{added} statics imported" + (removed > 0 ? $", {removed} removed" : string.Empty) +
                       (outside > 0 ? $", {outside} outside the map" : string.Empty) + " – save the map to keep them.";
            }
        }
    }
}
