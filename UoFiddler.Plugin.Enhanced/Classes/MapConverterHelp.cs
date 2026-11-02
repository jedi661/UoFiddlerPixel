/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.Enhanced.Classes
{
    /// <summary>Help of the "Map Converter" tab (English / German).</summary>
    public static class MapConverterHelp
    {
        public const string English = @"MAP CONVERTER – HELP

Converts maps between the Classic Client and the Enhanced Client (like UOEMC, but in both directions),
analyses and repairs the files and creates new maps.

FILES (they follow the map number on top; ""x files"" uses the x maps)
  Classic:  map#.mul (or map#LegacyMUL.uop), staidx#.mul, statics#.mul, radarcol.mul (radar colours)
  Enhanced: facet#.uop (64 x 64 sectors), facet0#.dds (radar image, 1 pixel per tile)
  map1x.mul <-> facet1x.uop; the statics of the x maps are the same files (staidx1.mul / statics1.mul).
  The list shows which files were found and what will be written into the output folder.
  Existing outputs are kept as <name>.<date>.bak. Write into an extra output folder first and test it.

SIZE
  Width and height in tiles. Set by the map number (0/1 7168 x 4096, 2 2304 x 1600, 3 2560 x 2048, 4 1448 x 1448,
  5 1280 x 4096); ""Detect size"" reads it from the length of map#.mul or from the facet (entry data.map).
  Custom maps: enter the size (multiple of 8).

CONVERT
  Classic -> Enhanced: every 64 x 64 sector with land, height and statics, the border tiles also store the land of
  their neighbours in the next sector (the client blends the terrain with it), plus the size entry data.map.
  Optional: the radar image facet0#.dds from radarcol.mul (hued statics with the hues of the loaded classic client).
  Enhanced -> Classic: map#.mul (+ staidx#.mul / statics#.mul).
  Land table (optional): a text file with lines ""old new"" (decimal or 0x hex) – land tiles are replaced while
  converting, e.g. the season tables of UOEMC (grass -> snow).
  Limits: the EC stores at most 255 statics per tile; the classic files store item ids and hues up to 0xFFFF.

ANALYZE / REPAIR
  Classic: file sizes, broken statics index entries, statics outside their block, duplicates, invalid item ids,
  tiles with more than 255 statics. Repair writes staidx / statics again without the broken entries
  (optionally without duplicates and invalid ids).
  Enhanced: size entry, missing / extra sectors, sector headers, border tiles that don't match their neighbours,
  duplicate statics, unreadable sectors. Repair fixes the chosen points and saves the facet in place (.bak).

NEW MAP
  A flat map (one land tile and height) with the map number and the size from above, as EC facet and / or as
  classic files with empty statics.

Note: the Enhanced Client files are written without the signatures of the original files. Test the result with
the client on a copy of the client folder.
";

        public const string German = @"MAP CONVERTER – HILFE

Wandelt Karten zwischen Classic Client und Enhanced Client um (wie UOEMC, aber in beide Richtungen),
prüft und repariert die Dateien und legt neue Karten an.

DATEIEN (folgen der Map-Nummer oben; ""x files"" nimmt die x-Karten)
  Classic:  map#.mul (oder map#LegacyMUL.uop), staidx#.mul, statics#.mul, radarcol.mul (Radarfarben)
  Enhanced: facet#.uop (Sektoren 64 x 64), facet0#.dds (Radarbild, 1 Pixel pro Kachel)
  map1x.mul <-> facet1x.uop; die Statics der x-Karten sind dieselben Dateien (staidx1.mul / statics1.mul).
  Die Liste zeigt, welche Dateien gefunden wurden und was in den Ausgabeordner geschrieben wird.
  Vorhandene Ausgaben bleiben als <Name>.<Datum>.bak erhalten. Zuerst in einen eigenen Ausgabeordner schreiben und testen.

GRÖSSE
  Breite und Höhe in Kacheln. Wird durch die Map-Nummer gesetzt (0/1 7168 x 4096, 2 2304 x 1600, 3 2560 x 2048,
  4 1448 x 1448, 5 1280 x 4096); ""Detect size"" liest sie aus der Länge von map#.mul oder aus der facet (Eintrag data.map).
  Eigene Karten: Größe eingeben (Vielfaches von 8).

UMWANDELN
  Classic -> Enhanced: jeder Sektor 64 x 64 mit Land, Höhe und Statics; die Randkacheln speichern zusätzlich das Land
  ihrer Nachbarn im nächsten Sektor (damit blendet der Client das Gelände), dazu der Größen-Eintrag data.map.
  Wahlweise das Radarbild facet0#.dds aus radarcol.mul (gefärbte Statics mit den Hues des geladenen Classic-Clients).
  Enhanced -> Classic: map#.mul (+ staidx#.mul / statics#.mul).
  Land-Tabelle (optional): Textdatei mit Zeilen ""alt neu"" (dezimal oder 0x hex) – Landkacheln werden beim Umwandeln
  ersetzt, z. B. die Jahreszeiten-Tabellen von UOEMC (Gras -> Schnee).
  Grenzen: der EC speichert höchstens 255 Statics pro Kachel; die Classic-Dateien Item-Ids und Hues bis 0xFFFF.

ANALYSE / REPARATUR
  Classic: Dateigrößen, kaputte Statics-Indexeinträge, Statics außerhalb ihres Blocks, doppelte Statics, ungültige
  Item-Ids, Kacheln mit mehr als 255 Statics. Die Reparatur schreibt staidx / statics neu ohne die kaputten Einträge
  (wahlweise ohne doppelte und ungültige).
  Enhanced: Größen-Eintrag, fehlende / überzählige Sektoren, Sektor-Köpfe, Randkacheln, die nicht zu ihren Nachbarn
  passen, doppelte Statics, unlesbare Sektoren. Die Reparatur behebt die gewählten Punkte und speichert die facet (.bak).

NEUE KARTE
  Eine flache Karte (eine Landkachel und Höhe) mit Map-Nummer und Größe von oben, als EC-facet und / oder als
  Classic-Dateien mit leeren Statics.

Hinweis: Die Enhanced-Client-Dateien werden ohne die Signaturen der Originaldateien geschrieben. Das Ergebnis mit dem
Client an einer Kopie des Client-Ordners testen.
";
    }
}
