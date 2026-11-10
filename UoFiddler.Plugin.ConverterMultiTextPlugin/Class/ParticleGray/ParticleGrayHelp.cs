using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ParticleGray
{
    /// <summary>Help window of the particle gray tool (English / German).</summary>
    internal static class ParticleGrayHelp
    {
        public const string English =
@"PARTICLE GRAY – HELP

PURPOSE
Ultima Online recolors (hues) graphics with a hue from hues.mul. For items with the tiledata flag ""Partial Hue""
the client only hues pixels that are exactly gray in 15 bit color (red = green = blue in 5 bit). The gray level
0 … 31 picks one of the 32 colors of the hue. Pure black is transparent.
This tool turns colored images – or only the parts you mark – into exactly this gray, so UO hues just these parts.

WORKFLOW
1. Load an image: file (Ctrl+O), clipboard (Ctrl+V) or an item / land tile from art.mul (tab Art).
2. Mark the area that should be hued (red overlay): mask brush, magic wand, stamps or a selection.
3. Tab Gray: ""Convert mask → gray"" (G). Without mask the whole image is converted (after a question).
4. Tab Hue preview: check the result with any hue (H shows it on the canvas).
5. Save / copy the image or write it back into art.mul (tab Art).

TOOLS (toolbar)
• Mask brush (B): left = mark, right = unmark. Brush size, Round / Square and Mirror in the toolbar.
• Wand (W): marks connected pixels of similar color (tolerance in tab Mask). Shift = all similar pixels of the image.
• Stamp (T): marks the shape chosen in tab Mask at the mouse. Drag to stamp repeatedly, right = unmark.
• Select (M): rectangle, ellipse, freehand, polygon – then ""Selection → mask +"" or ""−"".
• Pencil (P), Eraser (E), Pipette (I: left = paint color, right = dye color) to retouch pixels.
• Zoom: – / + , Ctrl + mouse wheel, Fit (Ctrl+F). Up to 16× – every pixel can be marked exactly.
• ""Show mask"" switches the red overlay on / off. ↶ / ↷ undo and redo image and mask (30 steps).

TAB MASK
• Selection → mask + / −, All, Clear, Invert, Grow, Shrink.
• Mark colored pixels, mark hued (gray) pixels, mark the paint color (with tolerance).
• Mask color: color of the overlay.
• Stamps: built-in shapes (circle, square, diamond, ring, star, cross, triangle, heart) in any size and own stamps.
  ""Mask → new stamp"" saves the marked area as new shape (folder ParticleGrayStamps next to UoFiddler).

TAB GRAY
• Mode: Luminance (natural), Average, Lightest or Darkest channel.
• ""Use all 32 gray levels"" spreads the area over the whole hue – the hue shows all its colors.
• Brightness / Contrast, ""Avoid black"" (black would be transparent in UO).
• Restore original colors in the mask (old ""Restore Color"").
• Protect colors outside the mask: colored areas can contain pixels that are exactly gray by chance – UO would hue
  them too. They get an invisible blue shift of one step.
• Dyeing: gray pixels get a color, their brightness stays (old ""Dyeing in""). Hex code, color dialog, RGB mixer.

TAB HUE PREVIEW
• Hue number (◀ ▶ Random), name and the 32 colors of the hue.
• Preview with hue, the hued pixels in magenta, or the image. ""All pixels"" = like an item without partial hue.
• ""On canvas"" (H) shows the hued image in the editor, ""Apply hue to image"" burns it in.

TAB ART
• Items or land tiles, filter All / Used / Free, ID + Go, Next free. Double click loads into the editor.
• ""Image → ID"" writes the edited image into the selected ID. For items the flag ""Partial Hue"" can be set in tiledata.
• Everything is in memory: ""Save art.mul"" here (or in UoFiddler), tiledata.mul in UoFiddler.

TAB ANALYSIS
• Histogram of the 32 gray levels (in the colors of the current hue). Gaps = hue colors that never appear.
• Pixel analysis: zoomed image + list of every pixel with hex code; click a pixel to find it and copy its code.
• Gray pixel list: show or save as text.

KEYBOARD
Ctrl+O open · Ctrl+V paste · Ctrl+S save · Ctrl+C copy · Ctrl+Z / Ctrl+Y undo / redo · Ctrl+A mark all ·
Ctrl+D clear mask · Ctrl+I invert mask · Ctrl+F fit · Ctrl+ + / – zoom · B W T M P E I tools · G convert ·
H hue view · Esc clear selection · F1 help";

        public const string German =
@"PARTICLE GRAY – HILFE

ZWECK
Ultima Online färbt Grafiken mit einem Hue aus der hues.mul ein. Bei Items mit dem tiledata-Flag „Partial Hue“
färbt der Client nur Pixel, die in 15-Bit-Farbe exakt grau sind (Rot = Grün = Blau in 5 Bit). Die Graustufe
0 … 31 wählt eine der 32 Farben des Hues. Reines Schwarz ist transparent.
Dieses Werkzeug wandelt farbige Bilder – oder nur die markierten Teile – in genau dieses Grau, damit UO nur diese Teile einfärbt.

ABLAUF
1. Bild laden: Datei (Strg+O), Zwischenablage (Strg+V) oder ein Item / eine Landkachel aus der art.mul (Reiter Art).
2. Den Bereich markieren, der eingefärbt werden soll (rote Fläche): Maskenpinsel, Zauberstab, Stempel oder Auswahl.
3. Reiter Gray: „Convert mask → gray“ (G). Ohne Maske wird nach einer Rückfrage das ganze Bild umgewandelt.
4. Reiter Hue preview: das Ergebnis mit beliebigem Hue prüfen (H zeigt es im Editor).
5. Bild speichern / kopieren oder in die art.mul zurückschreiben (Reiter Art).

WERKZEUGE (Leiste)
• Maskenpinsel (B): links = markieren, rechts = entfernen. Pinselgröße, Round / Square und Mirror in der Leiste.
• Zauberstab (W): markiert zusammenhängende Pixel ähnlicher Farbe (Toleranz im Reiter Mask). Umschalt = alle ähnlichen Pixel im Bild.
• Stempel (T): markiert die im Reiter Mask gewählte Form an der Maus. Ziehen = mehrfach stempeln, rechts = entfernen.
• Auswahl (M): Rechteck, Ellipse, Freihand, Polygon – dann „Selection → mask +“ oder „−“.
• Stift (P), Radierer (E), Pipette (I: links = Malfarbe, rechts = Färbefarbe) zum Nachbessern.
• Zoom: – / +, Strg + Mausrad, Fit (Strg+F). Bis 16× – jeder Pixel lässt sich genau markieren.
• „Show mask“ blendet die rote Fläche ein / aus. ↶ / ↷ machen Bild und Maske rückgängig / wieder her (30 Schritte).

REITER MASK
• Auswahl → Maske + / −, All (alles), Clear (leeren), Invert, Grow (wachsen), Shrink (schrumpfen).
• Farbige Pixel markieren, eingefärbte (graue) Pixel markieren, die Malfarbe markieren (mit Toleranz).
• Mask color: Farbe der Maskenanzeige.
• Stempel: eingebaute Formen (Kreis, Quadrat, Raute, Ring, Stern, Kreuz, Dreieck, Herz) in beliebiger Größe und eigene Stempel.
  „Mask → new stamp“ speichert den markierten Bereich als neue Form (Ordner ParticleGrayStamps neben UoFiddler).

REITER GRAY
• Mode: Luminance (natürlich), Durchschnitt, hellster oder dunkelster Kanal.
• „Use all 32 gray levels“ verteilt den Bereich über den ganzen Hue – der Hue zeigt alle seine Farben.
• Helligkeit / Kontrast, „Avoid black“ (Schwarz wäre in UO transparent).
• Originalfarben in der Maske zurückholen (früher „Restore Color“).
• Farben außerhalb der Maske schützen: farbige Bereiche können zufällig exakt graue Pixel enthalten – UO würde sie
  mit einfärben. Sie bekommen eine unsichtbare Blauverschiebung um eine Stufe.
• Dyeing: graue Pixel bekommen eine Farbe, ihre Helligkeit bleibt (früher „Dyeing in“). Hex-Code, Farbdialog, RGB-Mischer.

REITER HUE PREVIEW
• Hue-Nummer (◀ ▶ Random), Name und die 32 Farben des Hues.
• Vorschau mit Hue, die eingefärbten Pixel in Magenta, oder das Bild. „All pixels“ = wie ein Item ohne Partial Hue.
• „On canvas“ (H) zeigt das eingefärbte Bild im Editor, „Apply hue to image“ brennt es ein.

REITER ART
• Items oder Landkacheln, Filter All / Used / Free, ID + Go, Next free (nächste freie). Doppelklick lädt in den Editor.
• „Image → ID“ schreibt das bearbeitete Bild in die gewählte ID. Bei Items kann das Flag „Partial Hue“ in tiledata gesetzt werden.
• Alles ist im Speicher: „Save art.mul“ hier (oder in UoFiddler), tiledata.mul in UoFiddler speichern.

REITER ANALYSIS
• Histogramm der 32 Graustufen (in den Farben des aktuellen Hues). Lücken = Hue-Farben, die nie vorkommen.
• Pixel analysis: vergrößertes Bild + Liste aller Pixel mit Hex-Code; Pixel anklicken = finden und Code kopieren.
• Gray pixel list: anzeigen oder als Text speichern.

TASTEN
Strg+O öffnen · Strg+V einfügen · Strg+S speichern · Strg+C kopieren · Strg+Z / Strg+Y rückgängig / wiederholen ·
Strg+A alles markieren · Strg+D Maske leeren · Strg+I Maske invertieren · Strg+F einpassen · Strg + / – Zoom ·
B W T M P E I Werkzeuge · G umwandeln · H Hue-Ansicht · Esc Auswahl aufheben · F1 Hilfe";

        public static void Show(IWin32Window owner, bool german)
        {
            var form = new Form
            {
                Text = "Help / Hilfe – Particle Gray",
                Size = new Size(880, 760),
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
