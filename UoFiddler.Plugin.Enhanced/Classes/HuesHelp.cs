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
    /// <summary>Help of the "Hues" tab (English / German).</summary>
    public static class HuesHelp
    {
        public const string English = @"HUES – HELP

Edits Hues.uop of the Enhanced Client and converts hues between the Classic Client (hues.mul) and the
Enhanced Client in both directions (like huesmul2uop, but as an update, without loss and with a way back).

WHAT IS IN HUES.UOP
  data/definitions/hues/hue####.bmp  256 x 2 pixels per hue. Line 0 = the color ramp, line 1 = color 31.
                                     Used for animations, gumps and the hue picker.
  build/hues/hues.dds                1024 x 1024, one row of 256 pixels per hue (hue n: column n / 1024,
                                     row n % 1024). Used for the ITEMS (art tiles).
  0xFA5C6A1BC0D8B01B (no name)       64 x 64 overview, one pixel (color 31) per hue, column by column.
  data/definitions/hues/huenames.csv ""number,name"" per line.
  build/hues/classichues.bin         Optional, written by this plugin: the exact classic data (see below).
  Every change in this tab updates the bitmap, hues.dds, the overview and the stored data together,
  so items, animations and the picker always show the same hue.

HUE NUMBERS
  EC hue n = classic hue n = entry n - 1 of hues.mul (hue 1 is the first entry). 3000 hues = 375 groups of 8.

RAMP STYLES
  Original client: color i at pixel 8 * i, linear in between, colors * 8 (31 -> 248).
                   The bitmaps written with this style are byte for byte like the files of the client.
  huesmul2uop:     8 pixels per color, then blurred (bitmap 4, hues.dds 6), colors * 255 / 31.
  When reading, the style of a file is found automatically (64 hues are checked).

EDITOR
  List of all hues with ramp, name and how exact the classic colors are known:
    exact (stored)    from build/hues/classichues.bin and still matching the bitmap
    exact (verified)  calculated back, drawing them again gives the bitmap pixel for pixel
    close / approx.   the bitmap was not drawn from 32 colors (e.g. an imported image)
  The 32 colors below the ramp: click one to change it (the ramp is drawn new).
  Import / export ramp, add a copy, delete, name.
  Colors from classic: draws the marked hues from the loaded hues.mul.
  Colors to classic:   writes the marked hues into the loaded hues.mul (then ""Save hues.mul"").

COMPARE
  Compares every hue of hues.mul with Hues.uop. Filter: differences, colors, names, only in one file,
  EC colors not exact. ""Classic"" / ""Enhanced"" show or hide the columns. Mark rows and copy them in one
  direction (context menu or the buttons). Double click: show the hue in the editor.

CONVERTER
  Classic -> Enhanced
    Update the loaded Hues.uop: only the hues that differ are written (bitmap, hues.dds row, overview,
    name); then ""Save Hues.uop"" (old file kept as .bak). The style of the file is kept.
    New Hues.uop: a complete new file in the output folder with the chosen style.
    Store the exact classic data: the colors, table values, the 20 name bytes and the group headers are
    kept in build/hues/classichues.bin. The client doesn't read it; the way back is then byte exact.
  Enhanced -> Classic
    Update the loaded hues.mul: group headers, table values and all other hues stay as they are.
    New hues.mul: written into the output folder.
    The colors are taken from the stored data or calculated back from the bitmaps and hues.dds
    (least squares plus exact correction) – for bitmaps drawn from 32 colors this is always exact.
    Names longer than 20 characters are cut (listed in the log).
  Backup: before a file is replaced the old one is copied to <backup>\old, the new one to <backup>\new.
    Empty folder = ""UoFiddlerBackups\Hues"" next to the written file. backup.txt lists the origin.
  Analyze: style, how many hues are exact, whether hues.dds and the overview match the bitmaps.
  Rebuild hues.dds + overview: draws both again from all bitmaps (repairs old edits that only changed
    the bitmaps – then items had other colors than animations).

LIMITS
  hues.dds has room for hues 1 to 4095, the overview for 1 to 4095.
  The Enhanced Client must be closed while saving.
";

        public const string German = @"HUES – HILFE

Bearbeitet die Hues.uop des Enhanced Client und wandelt Hues zwischen Classic Client (hues.mul) und
Enhanced Client in beide Richtungen um (wie huesmul2uop, aber als Update, ohne Verlust und mit Rückweg).

WAS IN DER HUES.UOP STECKT
  data/definitions/hues/hue####.bmp  256 x 2 Pixel pro Hue. Zeile 0 = der Farbverlauf, Zeile 1 = Farbe 31.
                                     Für Animationen, Gumps und den Farbwähler.
  build/hues/hues.dds                1024 x 1024, eine Zeile mit 256 Pixeln pro Hue (Hue n: Spalte n / 1024,
                                     Zeile n % 1024). Färbt die ITEMS (Art-Tiles).
  0xFA5C6A1BC0D8B01B (ohne Namen)    64 x 64 Übersicht, ein Pixel (Farbe 31) pro Hue, Spalte für Spalte.
  data/definitions/hues/huenames.csv ""nummer,name"" pro Zeile.
  build/hues/classichues.bin         Optional, von diesem Plugin: die exakten Classic-Daten (siehe unten).
  Jede Änderung in diesem Tab aktualisiert Bitmap, hues.dds, Übersicht und die gespeicherten Daten
  zusammen – Items, Animationen und Farbwähler zeigen immer denselben Hue.

HUE-NUMMERN
  EC-Hue n = Classic-Hue n = Eintrag n - 1 der hues.mul (Hue 1 ist der erste Eintrag).
  3000 Hues = 375 Gruppen zu je 8.

ZEICHENART DER VERLÄUFE
  Original-Client: Farbe i bei Pixel 8 * i, dazwischen linear, Farben * 8 (31 -> 248).
                   So geschriebene Bitmaps sind Byte für Byte wie die Dateien des Clients.
  huesmul2uop:     8 Pixel pro Farbe, dann weichgezeichnet (Bitmap 4, hues.dds 6), Farben * 255 / 31.
  Beim Lesen wird die Zeichenart einer Datei automatisch erkannt (64 Hues werden geprüft).

EDITOR
  Liste aller Hues mit Verlauf, Name und wie genau die Classic-Farben bekannt sind:
    exact (stored)    aus build/hues/classichues.bin und passt noch zur Bitmap
    exact (verified)  zurückgerechnet, neu gezeichnet ergibt es die Bitmap Pixel für Pixel
    close / approx.   die Bitmap wurde nicht aus 32 Farben gezeichnet (z. B. ein importiertes Bild)
  Die 32 Farben unter dem Verlauf: eine anklicken, um sie zu ändern (der Verlauf wird neu gezeichnet).
  Rampe importieren / exportieren, Kopie anlegen, löschen, Name.
  Colors from classic: zeichnet die markierten Hues aus der geladenen hues.mul.
  Colors to classic:   schreibt die markierten Hues in die geladene hues.mul (dann ""Save hues.mul"").

VERGLEICH (Compare)
  Vergleicht jeden Hue der hues.mul mit der Hues.uop. Filter: Unterschiede, Farben, Namen, nur in einer
  Datei, EC-Farben nicht exakt. ""Classic"" / ""Enhanced"" blenden die Spalten ein und aus. Zeilen markieren
  und in eine Richtung übernehmen (Kontextmenü oder Knöpfe). Doppelklick: Hue im Editor zeigen.

KONVERTER
  Classic -> Enhanced
    Geladene Hues.uop aktualisieren: nur Hues, die sich unterscheiden, werden geschrieben (Bitmap,
    hues.dds-Zeile, Übersicht, Name); danach ""Save Hues.uop"" (alte Datei bleibt als .bak).
    Die Zeichenart der Datei bleibt erhalten.
    Neue Hues.uop: eine komplette neue Datei im Ausgabeordner mit der gewählten Zeichenart.
    Exakte Classic-Daten speichern: Farben, Tabellenwerte, die 20 Namens-Bytes und die Gruppen-Header
    kommen in build/hues/classichues.bin. Der Client liest das nicht; der Rückweg ist dann byte-genau.
  Enhanced -> Classic
    Geladene hues.mul aktualisieren: Gruppen-Header, Tabellenwerte und alle anderen Hues bleiben.
    Neue hues.mul: wird in den Ausgabeordner geschrieben.
    Die Farben kommen aus den gespeicherten Daten oder werden aus Bitmap und hues.dds zurückgerechnet
    (kleinste Quadrate plus exakte Korrektur) – für Bitmaps aus 32 Farben ist das immer exakt.
    Namen über 20 Zeichen werden gekürzt (stehen im Log).
  Sicherung: vor dem Ersetzen wird die alte Datei nach <Sicherung>\old kopiert, die neue nach
    <Sicherung>\new. Leerer Ordner = ""UoFiddlerBackups\Hues"" neben der geschriebenen Datei.
    backup.txt zeigt, woher jede Datei stammt.
  Analyze: Zeichenart, wie viele Hues exakt sind, ob hues.dds und Übersicht zu den Bitmaps passen.
  Rebuild hues.dds + overview: zeichnet beides neu aus allen Bitmaps (repariert alte Änderungen, die nur
    die Bitmaps geändert haben – dann hatten Items andere Farben als Animationen).

GRENZEN
  hues.dds hat Platz für Hue 1 bis 4095, die Übersicht für 1 bis 4095.
  Der Enhanced Client muss beim Speichern geschlossen sein.
";
    }
}
