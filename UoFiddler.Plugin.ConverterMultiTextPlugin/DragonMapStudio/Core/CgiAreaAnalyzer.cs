/***************************************************************************
 *
 *  * Dragon Map Studio – CGI view: analysis of a marked area as a text log
 *  *
 *  * For learning from the original map: mark a cave entrance, a road or a
 *  * mountain edge and write every tile with land id, height, texture id,
 *  * radar colour, tiledata name / flags and its class (mountain / ground /
 *  * entrance / invisible), the A/B neighbour pattern of entrance tiles
 *  * (like Dragon transition files) and all statics in the area.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public static class CgiAreaAnalyzer
    {
        private static readonly int[] NbDx = { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static readonly int[] NbDy = { -1, -1, -1, 0, 1, 1, 1, 0 };

        /// <summary>One letter per tile: M = mountain, E = cave entrance (01D3–01DB), N = invisible (0000–0002), . = ground.</summary>
        public static char ClassOf(ushort id, Func<ushort, bool> isMountain)
        {
            if (CaveEntranceBuilder.IsNoDraw(id))
            {
                return 'N';
            }
            if (id >= 0x01D3 && id <= 0x01DB)
            {
                return 'E';
            }
            return isMountain != null && isMountain(id) ? 'M' : '.';
        }

        public static string Analyze(MapData map, Rectangle area, Func<ushort, bool> isMountain, string source)
        {
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            var sb = new StringBuilder();
            sb.AppendLine("Dragon Map Studio – Bereichs-Auswertung (CGI-Ansicht)");
            sb.AppendLine($"Karte: {source}");
            sb.AppendLine($"Bereich: x {area.X}–{area.Right - 1}, y {area.Y}–{area.Bottom - 1} ({area.Width}×{area.Height}), erstellt {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine("Spalten = x (Westen → Osten), Zeilen = y (Norden → Süden). Die Kamera schaut von Südosten.");
            sb.AppendLine();
            if (area.IsEmpty)
            {
                sb.AppendLine("(leerer Bereich)");
                return sb.ToString();
            }

            // ------------------------------------------------ legend
            var ids = new SortedDictionary<ushort, int>();
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    ushort id = map.LandId(x, y);
                    ids[id] = ids.TryGetValue(id, out int n) ? n + 1 : 1;
                }
            }
            sb.AppendLine("== Land-Tiles im Bereich ==");
            sb.AppendLine("ID     Anzahl  Klasse  Textur  Farbe     Flags                 Name");
            foreach (KeyValuePair<ushort, int> e in ids)
            {
                Color c = Safe(() => UoArtCache.LandColor(e.Key), Color.Gray);
                int tex = TexId(e.Key);
                string texText = tex < 0 ? "?" : tex == 0 ? "-" : tex.ToString("x4");
                sb.AppendLine($"{e.Key:x4}   {e.Value,6}  {ClassOf(e.Key, isMountain),6}  {texText,6}  #{c.R:x2}{c.G:x2}{c.B:x2}   {Safe(() => UoArtCache.LandFlagsText(e.Key), "?"),-20}  {Safe(() => UoArtCache.LandName(e.Key), string.Empty)}");
            }
            sb.AppendLine("Klasse: M = Berg, E = Höhleneingang 01D3–01DB, N = unsichtbar (0000–0002, der Client zeichnet es nicht), . = Boden");
            sb.AppendLine();

            // ------------------------------------------------ grids
            void Grid(string title, Func<int, int, string> cell, int w)
            {
                sb.AppendLine($"== {title} ==");
                sb.Append(new string(' ', 6));
                for (int x = area.Left; x < area.Right; x++)
                {
                    sb.Append(x.ToString().PadLeft(w + 1));
                }
                sb.AppendLine();
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    sb.Append(y.ToString().PadLeft(5)).Append(' ');
                    for (int x = area.Left; x < area.Right; x++)
                    {
                        sb.Append(cell(x, y).PadLeft(w + 1));
                    }
                    sb.AppendLine();
                }
                sb.AppendLine();
            }
            Grid("Land-ID (hex)", (x, y) => map.LandId(x, y).ToString("x4"), 4);
            Grid("Höhe Z", (x, y) => map.LandZ(x, y).ToString(), 4);
            Grid("Textur-ID (hex, - = keine)", (x, y) =>
            {
                int t = TexId(map.LandId(x, y));
                return t < 0 ? "?" : t == 0 ? "-" : t.ToString("x4");
            }, 4);
            Grid("Klasse (M Berg, E Eingang, N unsichtbar, . Boden)", (x, y) => ClassOf(map.LandId(x, y), isMountain).ToString(), 3);
            Grid("Gezeichnet als (T = Textur am Hang, A = Land-Bild flach, - = gar nicht)", (x, y) =>
            {
                ushort id = map.LandId(x, y);
                if (CaveEntranceBuilder.IsNoDraw(id))
                {
                    return "-";
                }
                return TexId(id) > 0 && !NeighbourhoodFlat(map, x, y) ? "T" : "A";
            }, 3);

            // ------------------------------------------------ entrance patterns (like map/*.txt)
            var entrance = new List<string>();
            int groundRef = int.MinValue;
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    ushort id = map.LandId(x, y);
                    char cls = ClassOf(id, isMountain);
                    if (cls == '.' && groundRef == int.MinValue)
                    {
                        groundRef = map.LandZ(x, y);
                    }
                    if (cls != 'E' && cls != 'N')
                    {
                        continue;
                    }
                    var pattern = new char[8];
                    for (int k = 0; k < 8; k++)
                    {
                        ushort nid = map.LandId(x + NbDx[k], y + NbDy[k]);
                        char nc = ClassOf(nid, isMountain);
                        pattern[k] = nc == 'M' ? 'B' : 'A';
                    }
                    entrance.Add($"{x,5} {y,5}  {id:x4}  z {map.LandZ(x, y),4}  {new string(pattern)}");
                }
            }
            if (entrance.Count > 0)
            {
                sb.AppendLine("== Eingangs-/unsichtbare Tiles mit Nachbar-Muster ==");
                sb.AppendLine("Muster wie in den Dragon-Dateien: Reihenfolge NW N NO O SO S SW W, B = Berg-Nachbar, A = alles andere.");
                sb.AppendLine($"Boden-Höhe (erstes Boden-Tile im Bereich): {(groundRef == int.MinValue ? "-" : groundRef.ToString())}");
                sb.AppendLine("    x     y  ID    Höhe  Muster");
                entrance.ForEach(l => sb.AppendLine(l));
                sb.AppendLine();
            }

            // ------------------------------------------------ statics
            var statics = new List<StaticItem>();
            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    statics.AddRange(map.StaticsAt(x, y));
                }
            }
            sb.AppendLine($"== Statics im Bereich ({statics.Count}) ==");
            if (statics.Count > 0)
            {
                sb.AppendLine("    x     y     z  dx  dy  dz    ID    Hue  Höhe  Flags                          Name");
                foreach (StaticItem s in statics.OrderBy(s => s.Y).ThenBy(s => s.X).ThenBy(s => s.Z))
                {
                    int dz = s.Z - map.LandZ(s.X, s.Y);
                    sb.AppendLine($"{s.X,5} {s.Y,5} {s.Z,5} {s.X - area.X,3} {s.Y - area.Y,3} {dz,3}  {s.Id:x4}  {s.Hue,5}  {Safe(() => UoArtCache.ItemHeight(s.Id), 0),4}  {Safe(() => UoArtCache.ItemFlagsText(s.Id), "?"),-30} {Safe(() => UoArtCache.ItemName(s.Id), string.Empty)}");
                }
                sb.AppendLine("dx/dy = Lage im Bereich, dz = Höhe über dem Land darunter.");
            }
            return sb.ToString();
        }

        /// <summary>Client files missing or not readable: the log is still written, with "?" where data is missing.</summary>
        private static T Safe<T>(Func<T> f, T fallback)
        {
            try
            {
                return f();
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        private static int TexId(int id) => Safe(() => UoArtCache.LandTextureId(id), -1);

        /// <summary>Same rule as ClassicUO (Land.ApplyStretch): all 12 heights around the tile equal = not stretched.</summary>
        public static bool NeighbourhoodFlat(MapData map, int x, int y)
        {
            int z = map.LandZ(x, y);
            int Z(int dx, int dy) => map.LandZ(Math.Clamp(x + dx, 0, map.Width - 1), Math.Clamp(y + dy, 0, map.Height - 1));
            return Z(0, -1) == z && Z(1, -1) == z && Z(-1, 0) == z && Z(1, 0) == z && Z(2, 0) == z && Z(-1, 1) == z &&
                   Z(0, 1) == z && Z(1, 1) == z && Z(2, 1) == z && Z(0, 2) == z && Z(1, 2) == z;
        }
    }
}
