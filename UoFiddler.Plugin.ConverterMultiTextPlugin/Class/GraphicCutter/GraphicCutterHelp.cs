using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter
{
    /// <summary>Help window of the Graphic Cutter (English / German).</summary>
    internal static class GraphicCutterHelp
    {
        public const string English =
@"GRAPHIC CUTTER – HELP

PURPOSE
Place an image on a 352 × 352 stage with the isometric UO tile grid, cut an area out of it
(for example a 44 px wide art piece), edit the result and save it, copy it, collect it or write it
straight into art.mul.

LAYOUT
• Left: the stage. The image, the red image border, the grid and the cut area (cyan frame).
• Middle: the result = the cut area. Zoom, selections, mask, grid.
• Right: panel with the tabs ""art.mul"" (load art in / write art out) and ""Collection"".
  Show / hide it with View > Art / collection panel (Ctrl+B).
• Bottom: Move image, Cut area, Move cut area, Options.
• Status bar: mouse position, image / cut / result size, messages.

LOAD AN IMAGE
• File > Open image (Ctrl+O): PNG, BMP, GIF, JPG, TIFF.
• File > Paste image (Ctrl+V): from the clipboard (PNG keeps transparency).
• From art.mul: panel ""art.mul"" > select an item or land tile > ""→ Stage"" (or double click).
• ""Open image into result"" / ""→ Result"" puts an image directly into the result.
The image is placed bottom centered; the cut area is set to the image.

MOVE THE IMAGE
• Arrow buttons (hold = repeat), arrow keys (Shift = 10 px), or drag with the left mouse button.
• ""Cut area follows the image"": the cut area moves with the image.
• Image > Reset position puts it bottom centered again.

CUT AREA
• Width, height, X, Y in pixels on the stage. Presets: image size, 44 × 44 (tile), 44 × 105, 44 × 123 (mask).
• Move it with the right arrow block, Ctrl + arrow keys or by dragging with the right mouse button
  (or Ctrl + left mouse button).
• ""Snap to grid"": the cut area jumps to the corners of the diamond tiles (left edge and bottom edge).
  Arrow moves then go one tile corner (about 22 px) at a time.

TRANSPARENCY
• ""Black / white transparent"" with tolerance (0 = only pure black / white).
• Pipette: press ""Pipette"" in the toolbar, then click a color on the stage or in the result.
  This color becomes transparent as well (same tolerance). ""Clear"" removes all picked colors.
• Image > Remove transparent border (auto trim) cuts away the empty border of the image.

RESULT
• Zoom with + / – or Ctrl + mouse wheel (pixel exact). Reset = 1x and no selection.
• Selection: rectangle, circle or freehand (drag in the result, Shift = square / circle). Esc removes it.
• Copy result (Ctrl+C), copy selection (Ctrl+Shift+C). The clipboard gets bitmap and PNG.
• Mirror, rotate 90° left, fill the selection with a texture.
• Alignment mask 44 × 123 (by Otimpyre) and grid on the result are only view aids, they are not saved.
• ""Cut with grid and background"" puts background color, background image and grid into the cut.
• Note: moving the image or the cut area creates the result again – edits like a texture fill are then lost
  (use Undo).

UNDO / REDO
Ctrl+Z / Ctrl+Y (or the arrows in the toolbar). Covers position, cut area, mirror, rotation and result edits
(60 steps). Writing to art.mul is not part of it.

SAVE
• Save (Ctrl+S): into the folder tempGrafic next to UoFiddler, format from the toolbar.
• Save as (Ctrl+Shift+S): own name and folder.
• ""Transparent:"" in the toolbar: keep transparency, or fill it black or white (for BMP / JPEG).

SPLIT INTO ART PIECES (Result > Split into art pieces)
Cuts the result into vertical strips (default 44 px). Options: fill of transparent pixels, remove empty rows
at the top, skip empty pieces. Targets: files in tempGrafic (own subfolder), art.mul items from a start ID
(""Find free block"" finds enough free IDs in a row), and / or the collection.

COLLECTION
Result > Add to collection (Ctrl+Enter or + in the toolbar) keeps a copy of the current result.
In the tab ""Collection"": → Result / → Stage, order, remove, ""Save all"" as files,
""Write all to art"" as items from an ID (used IDs skipped or replaced).

ART.MUL – LOAD IN AND WRITE OUT
Tab ""art.mul"" works with the art.mul loaded in UoFiddler.
• Items or Land tiles, filter All / Used / Free (free IDs have a red label), ID box (hex 0x1A2B or decimal) + Go,
  ""Next free"" jumps to the next empty ID.
• Load in: ""→ Stage"", ""→ Result"", ""Copy art"" (clipboard).
• Write out: ""Result → ID"" writes the current result, ""Clipboard → ID"" the clipboard image into the selected ID.
  A used ID is only replaced after a question. Land tiles must be 44 × 44.
• ""Remove ID"" deletes the graphic of an ID. ""Reload list"" reads used / free again.
• ""Use as background"" (land tiles): the stage ground is filled with this tile.
• All changes are in memory first (like the Items tab). ""Save art.mul"" (or File > Save art.mul) writes
  artidx.mul and art.mul into the UoFiddler output folder.

BACKGROUND OF THE STAGE
View > Stage background: black, white, any color or hex code; images grass / water;
""land tile"" = a land tile from art.mul drawn on the whole diamond (choose it in the art.mul tab).

KEYBOARD
Ctrl+O open · Ctrl+V paste · Ctrl+S save · Ctrl+Shift+S save as · Ctrl+C copy result ·
Ctrl+Shift+C copy selection · Ctrl+Z / Ctrl+Y undo / redo · Ctrl+Enter add to collection ·
Arrows move image · Ctrl+arrows move cut area · Shift = 10 px · Ctrl+G grid · Ctrl+B panel ·
Ctrl + / Ctrl – zoom · Esc remove selection · F1 help";

        public const string German =
@"GRAPHIC CUTTER – HILFE

ZWECK
Ein Bild auf einer 352 × 352 Arbeitsfläche mit dem isometrischen UO-Kachelraster ausrichten, einen Bereich
ausschneiden (z. B. ein 44 px breites Art-Teil), das Ergebnis bearbeiten und speichern, kopieren, sammeln
oder direkt in die art.mul schreiben.

AUFBAU
• Links: die Arbeitsfläche. Bild, roter Bildrahmen, Raster und Ausschnitt (türkiser Rahmen).
• Mitte: das Ergebnis = der Ausschnitt. Zoom, Auswahl, Maske, Raster.
• Rechts: Panel mit den Reitern „art.mul“ (Art einladen / rausschreiben) und „Collection“ (Sammlung).
  Ein- / ausblenden mit View > Art / collection panel (Strg+B).
• Unten: Bild verschieben, Ausschnitt, Ausschnitt verschieben, Optionen.
• Statusleiste: Mausposition, Größe von Bild / Ausschnitt / Ergebnis, Meldungen.

BILD LADEN
• File > Open image (Strg+O): PNG, BMP, GIF, JPG, TIFF.
• File > Paste image (Strg+V): aus der Zwischenablage (PNG behält die Transparenz).
• Aus der art.mul: Panel „art.mul“ > Item oder Landkachel wählen > „→ Stage“ (oder Doppelklick).
• „Open image into result“ / „→ Result“ legt ein Bild direkt ins Ergebnis.
Das Bild wird unten mittig platziert, der Ausschnitt wird auf das Bild gesetzt.

BILD VERSCHIEBEN
• Pfeil-Buttons (gedrückt halten = wiederholen), Pfeiltasten (Umschalt = 10 px) oder mit der linken Maustaste ziehen.
• „Cut area follows the image“: der Ausschnitt wandert mit dem Bild mit.
• Image > Reset position setzt es wieder unten mittig.

AUSSCHNITT
• Breite, Höhe, X, Y in Pixeln auf der Arbeitsfläche. Vorlagen: Bildgröße, 44 × 44 (Kachel), 44 × 105, 44 × 123 (Maske).
• Verschieben mit dem rechten Pfeilblock, Strg + Pfeiltasten oder mit der rechten Maustaste ziehen
  (oder Strg + linke Maustaste).
• „Snap to grid“ (am Raster einrasten): der Ausschnitt springt auf die Ecken der Rautenkacheln
  (linke Kante und Unterkante). Pfeile bewegen ihn dann um eine Kachelecke (ca. 22 px).

TRANSPARENZ
• „Black / white transparent“ mit Toleranz (0 = nur reines Schwarz / Weiß).
• Pipette: „Pipette“ in der Werkzeugleiste drücken, dann eine Farbe auf der Arbeitsfläche oder im Ergebnis anklicken.
  Diese Farbe wird ebenfalls transparent (gleiche Toleranz). „Clear“ entfernt alle gewählten Farben.
• Image > Remove transparent border (auto trim) schneidet den leeren Rand des Bildes ab.

ERGEBNIS
• Zoom mit + / – oder Strg + Mausrad (pixelgenau). Reset = 1x und keine Auswahl.
• Auswahl: Rechteck, Kreis oder Freihand (im Ergebnis ziehen, Umschalt = Quadrat / Kreis). Esc entfernt sie.
• Ergebnis kopieren (Strg+C), Auswahl kopieren (Strg+Umschalt+C). In die Zwischenablage kommt Bitmap und PNG.
• Spiegeln, um 90° nach links drehen, Auswahl mit einer Textur füllen.
• Ausrichtungsmaske 44 × 123 (von Otimpyre) und Raster im Ergebnis sind nur Anzeigehilfen und werden nicht gespeichert.
• „Cut with grid and background“ nimmt Hintergrundfarbe, Hintergrundbild und Raster mit in den Ausschnitt.
• Hinweis: Verschieben von Bild oder Ausschnitt erzeugt das Ergebnis neu – Bearbeitungen wie Textur füllen gehen
  dann verloren (Rückgängig benutzen).

RÜCKGÄNGIG / WIEDERHOLEN
Strg+Z / Strg+Y (oder die Pfeile in der Werkzeugleiste). Umfasst Position, Ausschnitt, Spiegeln, Drehen und
Bearbeitungen am Ergebnis (60 Schritte). Schreiben in die art.mul gehört nicht dazu.

SPEICHERN
• Save (Strg+S): in den Ordner tempGrafic neben UoFiddler, Format aus der Werkzeugleiste.
• Save as (Strg+Umschalt+S): eigener Name und Ordner.
• „Transparent:“ in der Werkzeugleiste: Transparenz behalten oder schwarz / weiß füllen (für BMP / JPEG).

IN ART-TEILE ZERLEGEN (Result > Split into art pieces)
Schneidet das Ergebnis in senkrechte Streifen (Standard 44 px). Optionen: Füllung transparenter Pixel, leere Zeilen
oben entfernen, leere Teile überspringen. Ziele: Dateien in tempGrafic (eigener Unterordner), art.mul-Items ab einer
Start-ID („Find free block“ sucht genug freie IDs am Stück) und / oder die Sammlung.

SAMMLUNG
Result > Add to collection (Strg+Enter oder + in der Werkzeugleiste) merkt sich eine Kopie des aktuellen Ergebnisses.
Im Reiter „Collection“: → Result / → Stage, Reihenfolge, entfernen, „Save all“ als Dateien,
„Write all to art“ als Items ab einer ID (belegte IDs überspringen oder ersetzen).

ART.MUL – EINLADEN UND RAUSSCHREIBEN
Der Reiter „art.mul“ arbeitet mit der in UoFiddler geladenen art.mul.
• Items oder Land tiles, Filter All / Used / Free (freie IDs haben eine rote Beschriftung), ID-Feld (hex 0x1A2B oder
  dezimal) + Go, „Next free“ springt zur nächsten leeren ID.
• Einladen: „→ Stage“, „→ Result“, „Copy art“ (Zwischenablage).
• Rausschreiben: „Result → ID“ schreibt das aktuelle Ergebnis, „Clipboard → ID“ das Bild aus der Zwischenablage
  in die gewählte ID. Eine belegte ID wird nur nach Rückfrage ersetzt. Landkacheln müssen 44 × 44 sein.
• „Remove ID“ löscht die Grafik einer ID. „Reload list“ liest belegt / frei neu ein.
• „Use as background“ (Landkacheln): der Boden der Arbeitsfläche wird mit dieser Kachel gefüllt.
• Alle Änderungen sind zuerst nur im Speicher (wie im Items-Tab). „Save art.mul“ (oder File > Save art.mul) schreibt
  artidx.mul und art.mul in den UoFiddler-Ausgabeordner.

HINTERGRUND DER ARBEITSFLÄCHE
View > Stage background: Schwarz, Weiß, beliebige Farbe oder Hex-Code; Bilder Gras / Wasser;
„land tile“ = eine Landkachel aus der art.mul auf der ganzen Raute (im Reiter art.mul auswählen).

TASTENKÜRZEL
Strg+O öffnen · Strg+V einfügen · Strg+S speichern · Strg+Umschalt+S speichern unter · Strg+C Ergebnis kopieren ·
Strg+Umschalt+C Auswahl kopieren · Strg+Z / Strg+Y rückgängig / wiederholen · Strg+Enter zur Sammlung ·
Pfeile Bild verschieben · Strg+Pfeile Ausschnitt verschieben · Umschalt = 10 px · Strg+G Raster · Strg+B Panel ·
Strg + / Strg – Zoom · Esc Auswahl entfernen · F1 Hilfe";

        public static void Show(IWin32Window owner, bool german)
        {
            var form = new Form
            {
                Text = "Help / Hilfe – Graphic Cutter",
                Size = new Size(860, 740),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                ShowIcon = false,
                TopMost = owner is Form f && f.TopMost
            };
            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(MakePage("English", English));
            tabs.TabPages.Add(MakePage("Deutsch", German));
            tabs.SelectedIndex = german ? 1 : 0;
            form.Controls.Add(tabs);
            form.Show(owner);
        }

        private static TabPage MakePage(string title, string text)
        {
            var page = new TabPage(title);
            page.Controls.Add(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.75f),
                BackColor = SystemColors.Window,
                Text = text.Replace("\r\n", "\n").Replace("\n", "\r\n")
            });
            return page;
        }
    }
}
