/***************************************************************************
 *
 *  * Dragon Map Studio – "Log + Bild": a picture of a design view and a text with every piece
 *  *  (position, height, id, name, item height, flags, direction) – to send to the developer.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public static class DesignLog
    {
        public static string Folder => Path.Combine(StudioSettings.SettingsFolder, "Logs");

        /// <summary>Saves picture + text of the area, puts the text into the clipboard and shows the files. Returns the text file.</summary>
        public static string Export(IWin32Window owner, IsoMapView view, MapData map, Rectangle area, string title, string extra = null)
        {
            Directory.CreateDirectory(Folder);
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string safe = string.Concat(title.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
            string png = Path.Combine(Folder, $"{safe}_{stamp}.png");
            string txt = Path.Combine(Folder, $"{safe}_{stamp}.txt");
            try
            {
                using var bmp = new Bitmap(Math.Max(1, view.Width), Math.Max(1, view.Height));
                view.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                bmp.Save(png, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                png = "(Bild fehlgeschlagen: " + ex.Message + ")";
            }
            var sb = new StringBuilder();
            sb.AppendLine($"DRAGON MAP STUDIO – {title}  {DateTime.Now:yyyy-MM-dd HH:mm}");
            if (area.IsEmpty)
            {
                area = new Rectangle(0, 0, map.Width, map.Height);
            }
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            int ground = map.LandZ(area.X, area.Y);
            sb.AppendLine($"Bereich ({area.X}, {area.Y}) {area.Width}×{area.Height}, Boden z {ground}; Positionen relativ zur Ecke oben links, z absolut und relativ zum Boden");
            if (!string.IsNullOrEmpty(extra))
            {
                sb.AppendLine(extra);
            }
            sb.AppendLine();
            sb.AppendLine("x  y   z (rel)  id    Name                  Höhe  Richtung  Treppe hinauf  Flags");
            foreach (StaticItem s in Enumerable.Range(area.Top, area.Height).SelectMany(y => Enumerable.Range(area.Left, area.Width).SelectMany(x => map.StaticsAt(x, y)))
                         .OrderBy(s => s.Y).ThenBy(s => s.X).ThenBy(s => s.Z))
            {
                string dir, up, flags;
                try
                {
                    int o = BridgeKit.Orientation(s.Id);
                    dir = o > 0 ? "W-O" : o < 0 ? "N-S" : "-";
                    up = BridgeKit.StairUp(s.Id, out _)?.ToString() ?? "-";
                    flags = TileData.ItemTable[s.Id].Flags.ToString();
                }
                catch (Exception)
                {
                    dir = up = flags = "?";
                }
                sb.AppendLine($"{s.X - area.X,2} {s.Y - area.Y,2}  {s.Z,4} ({s.Z - ground,4})  {s.Id:x4}  {UoArtCache.ItemName(s.Id),-20}  h{UoArtCache.ItemHeight(s.Id),-3}  {dir,-8}  {up,-13}  {flags}" +
                              (s.Hue != 0 ? $"  hue {s.Hue}" : string.Empty));
            }
            sb.AppendLine();
            sb.AppendLine("Land (id@z) je Zeile:");
            for (int y = area.Top; y < area.Bottom; y++)
            {
                sb.AppendLine(string.Join(" ", Enumerable.Range(area.Left, area.Width).Select(x => $"{map.LandId(x, y):x4}@{map.LandZ(x, y)}")));
            }
            sb.AppendLine();
            sb.AppendLine("Bild: " + png);
            File.WriteAllText(txt, sb.ToString());
            try
            {
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception)
            {
                // clipboard busy – the file is there
            }
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + txt + "\"");
            }
            catch (Exception)
            {
                // no explorer
            }
            MessageBox.Show(owner, L.T("Log und Bild gespeichert (der Text ist auch in der Zwischenablage):") + Environment.NewLine + txt, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return txt;
        }
    }
}
