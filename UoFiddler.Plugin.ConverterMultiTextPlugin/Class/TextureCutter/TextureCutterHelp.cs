using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter
{
    /// <summary>Help window of the Texture Cutter (English / German).</summary>
    internal static class TextureCutterHelp
    {
        public const string English =
@"TEXTURE CUTTER – HELP

PURPOSE
Load images or UO textures, cut them into tiles, resize them, turn them into 44 × 44 land tiles,
change their colors, paint on them, put other graphics on top and save the result as files or
directly into texmaps.mul (textures) or art.mul (land tiles).

LAYOUT
• Left: the image. Ctrl + mouse wheel = zoom (pixel exact), checkerboard = transparent.
• Toolbar: open, paste, save + format, copy, undo / redo, tools, colors, brush, options, zoom, grid, 3 × 3 view.
• Right: tabs Tiles, Colors, Replace, Overlay, texmaps.
• Status bar: pixel position, color under the mouse (hex and RGB), image size, selection, messages.

LOAD
• File > Open image (Ctrl+O): PNG, BMP, GIF, JPG, TIFF. The file is not locked.
• Paste (Ctrl+V): from the clipboard (PNG keeps transparency).
• Tab texmaps: double click a texture or ""→ Editor"".
• Tab Tiles > ""Load land tile"": a land tile from art.mul.
• Image > New empty image: transparent image in the tile size.

TOOLS (shortcut)
Select (M) · Pencil (B) · Eraser (E) · Line (L) · Rectangle (R) · Fill (G) · Pipette (I) · Move overlay (O)
• Left mouse = color 1, right mouse = color 2. X swaps the colors. Brush size in the toolbar.
• ""Mirror draw"" paints every stroke mirrored left / right as well.
• Eraser makes pixels transparent (option on the Replace tab: erase to white).
• Fill uses the tolerance from the Replace tab.
• Selection shapes: rectangle, ellipse, freehand, polygon (click points, double or right click closes).
  Shift = square / circle. Ctrl+A = all, Esc = remove, Del = make the selection transparent.
• ""Only selection"" (toolbar): filters, color changes and replacing only act inside the selection.
• ""Protect B/W"": pure black and white (UO transparency) stay unchanged by color changes.

TAB TILES
• Tile size: 33, 44, 64, 128, 256 or custom W / H, plus an offset. ""Show tile grid"" draws the tiles.
  Sizes of 1–5 px only with ""Allow very small tiles"".
• ""Cut tiles → files"": every tile into a new folder in tempGrafic (format from the toolbar).
  Incomplete tiles at the edge only when ""Also cut the incomplete tiles"" is on. Empty tiles can be skipped.
• ""Cut tiles → texmaps"": all tiles (64 or 128) into texmaps.mul from the start ID on.
  ""Free block"" finds enough free IDs in a row; used IDs are skipped or replaced.
• Size (old ""Resolution""): any width / height, keep aspect ratio, quality pixel / smooth / smooth + sharpen,
  quick buttons 44 / 64 / 128 / 256.
• UO land tile: ""Texture → land tile"" maps the square texture exactly onto the 44 × 44 diamond
  (top left corner of the texture = top corner of the diamond). ""Old way"" is the former ""Create Tiles"".
  ""Land tile → texture"" is the way back (64 or 128). ""Load / Write land tile"" reads and writes art.mul.
• ""Make seamless"": edges blend into each other so the texture repeats without visible seams.
  Check it with the tiled view 3 × 3 (Ctrl+T).

TAB COLORS
• Preview: sliders, color modes and particle grey show a live preview first.
  ""Apply"" (Enter) puts it into the image, ""Cancel"" (Esc) throws it away.
• Adjust: brightness, contrast, gamma, saturation, hue, grayscale (old ""Modus"" and ""Optimize"").
• Filters: sharpen, white balance (lighter / darker), color enhance, normalize, invert,
  UO 16-bit colors (shows the colors as UO stores them), posterize.
• Color modes: the old buttons R1 / R2 with names. Previous / Next steps through them.
• Particle grey: all gray pixels get a color – flat, or with shadow (keeps their brightness).

TAB REPLACE
• From / To color: click the color field, type a hex code or use ""Pick"" and click a pixel.
• Tolerance in % (0 = exact). ""Keep shading"" keeps light and dark of the replaced pixels.
• ""Replace"", ""Erase → transparent"", ""→ white"" (old ""Delete"").
• Colors in the image: all colors with pixel count and %. Click = ""From"" color, double click = copy hex code.

TAB OVERLAY (graphics on top)
• Load an overlay from a file, the clipboard, the selected texture or an art.mul item / land tile.
  ""Black / white transparent"" removes the UO background of art.
• Move it with the tool ""Move overlay"" (drag, arrow keys, Shift = 10 px), opacity, scale, X / Y, center, fit.
• ""Apply (burn in)"" draws it into the image. ""Remove"" drops it.
• Fill the selection with an image file or the overlay: repeat (tile) or stretch.

TAB TEXMAPS (load in and save out)
• All textures of texmaps.mul: filter All / Used / Free (free = red), ID box + Go, ""Next free"".
• The preview shows the texture and the land tile made from it, plus which land tiles use it (tiledata).
• ""→ Editor"", ""→ Overlay"" (show it over the image to compare), ""Copy texture"".
• ""Image → ID"" writes the image into the selected ID. Not 64 / 128? It asks whether to scale.
• ""Remove ID"", ""Reload list"". All changes are in memory until ""Save texmaps.mul""
  (also File menu) writes texidx.mul and texmaps.mul into the UoFiddler output folder.

SAVE
• Save (Ctrl+S): into tempGrafic next to UoFiddler, format from the toolbar.
• Save as (Ctrl+Shift+S): own name and folder. Copy (Ctrl+C), copy selection (Ctrl+Shift+C).

UNDO
Ctrl+Z / Ctrl+Y, 30 steps. Writing to texmaps.mul / art.mul is not part of it.

KEYBOARD
Ctrl+O open · Ctrl+V paste · Ctrl+S save · Ctrl+C copy · Ctrl+Z / Y undo / redo · Ctrl+A select all ·
Ctrl+G grid · Ctrl+T 3 × 3 view · Ctrl+F fit · Ctrl+0 1:1 · Ctrl + / – zoom · Enter apply preview ·
Esc cancel · Del delete selection · M B E L R G I O tools · X swap colors · F1 help";

        public const string German =
@"TEXTURE CUTTER – HILFE

ZWECK
Bilder oder UO-Texturen laden, in Kacheln schneiden, in der Größe ändern, zu 44 × 44 Landkacheln machen,
Farben ändern, darauf malen, andere Grafiken darüberlegen und das Ergebnis als Datei oder direkt in die
texmaps.mul (Texturen) bzw. art.mul (Landkacheln) speichern.

AUFBAU
• Links: das Bild. Strg + Mausrad = Zoom (pixelgenau), Schachbrett = transparent.
• Werkzeugleiste: öffnen, einfügen, speichern + Format, kopieren, rückgängig / wiederholen, Werkzeuge, Farben,
  Pinsel, Optionen, Zoom, Raster, 3 × 3 Ansicht.
• Rechts: Reiter Tiles, Colors, Replace, Overlay, texmaps.
• Statusleiste: Pixelposition, Farbe unter der Maus (Hex und RGB), Bildgröße, Auswahl, Meldungen.

LADEN
• File > Open image (Strg+O): PNG, BMP, GIF, JPG, TIFF. Die Datei wird nicht gesperrt.
• Einfügen (Strg+V): aus der Zwischenablage (PNG behält die Transparenz).
• Reiter texmaps: Doppelklick auf eine Textur oder „→ Editor“.
• Reiter Tiles > „Load land tile“: eine Landkachel aus der art.mul.
• Image > New empty image: transparentes Bild in Kachelgröße.

WERKZEUGE (Taste)
Auswahl (M) · Stift (B) · Radierer (E) · Linie (L) · Rechteck (R) · Füllen (G) · Pipette (I) · Overlay verschieben (O)
• Linke Maustaste = Farbe 1, rechte = Farbe 2. X tauscht die Farben. Pinselgröße in der Werkzeugleiste.
• „Mirror draw“ zeichnet jeden Strich zusätzlich links / rechts gespiegelt.
• Der Radierer macht Pixel transparent (Option im Reiter Replace: zu Weiß radieren).
• Füllen benutzt die Toleranz aus dem Reiter Replace.
• Auswahlformen: Rechteck, Ellipse, Freihand, Polygon (Punkte klicken, Doppel- oder Rechtsklick schließt).
  Umschalt = Quadrat / Kreis. Strg+A = alles, Esc = entfernen, Entf = Auswahl transparent machen.
• „Only selection“ (Werkzeugleiste): Filter, Farbänderungen und Ersetzen wirken nur in der Auswahl.
• „Protect B/W“: reines Schwarz und Weiß (UO-Transparenz) bleiben bei Farbänderungen unverändert.

REITER TILES
• Kachelgröße: 33, 44, 64, 128, 256 oder eigene B / H, dazu ein Versatz. „Show tile grid“ zeigt die Kacheln.
  Größen von 1–5 px nur mit „Allow very small tiles“.
• „Cut tiles → files“: jede Kachel in einen neuen Ordner in tempGrafic (Format aus der Werkzeugleiste).
  Unvollständige Kacheln am Rand nur mit „Also cut the incomplete tiles“. Leere Kacheln lassen sich überspringen.
• „Cut tiles → texmaps“: alle Kacheln (64 oder 128) ab der Start-ID in die texmaps.mul.
  „Free block“ sucht genug freie IDs am Stück; belegte IDs werden übersprungen oder ersetzt.
• Größe (früher „Resolution“): beliebige Breite / Höhe, Seitenverhältnis halten, Qualität Pixel / weich /
  weich + schärfen, Schnellknöpfe 44 / 64 / 128 / 256.
• UO-Landkachel: „Texture → land tile“ legt die quadratische Textur genau auf die 44 × 44 Raute
  (linke obere Ecke der Textur = obere Spitze der Raute). „Old way“ ist das frühere „Create Tiles“.
  „Land tile → texture“ ist der Rückweg (64 oder 128). „Load / Write land tile“ liest und schreibt die art.mul.
• „Make seamless“: die Ränder gehen ineinander über, die Textur wiederholt sich ohne sichtbare Naht.
  Prüfen mit der Kachelansicht 3 × 3 (Strg+T).

REITER COLORS
• Vorschau: Schieberegler, Farbmodi und Particle grey zeigen zuerst eine Live-Vorschau.
  „Apply“ (Enter) übernimmt sie ins Bild, „Cancel“ (Esc) verwirft sie.
• Adjust: Helligkeit, Kontrast, Gamma, Sättigung, Farbton, Graustufen (früher „Modus“ und „Optimize“).
• Filter: schärfen, Weißabgleich (heller / dunkler), Farben verstärken, normalisieren, invertieren,
  UO 16-Bit-Farben (zeigt die Farben so, wie UO sie speichert), Posterize.
• Farbmodi: die früheren Knöpfe R1 / R2 mit Namen. Previous / Next blättert durch.
• Particle grey: alle grauen Pixel bekommen eine Farbe – flach oder mit Schatten (behält die Helligkeit).

REITER REPLACE
• From- / To-Farbe: Farbfeld anklicken, Hex-Code eintippen oder „Pick“ und ein Pixel anklicken.
• Toleranz in % (0 = genau). „Keep shading“ behält Hell und Dunkel der ersetzten Pixel.
• „Replace“, „Erase → transparent“, „→ white“ (früher „Delete“).
• Farben im Bild: alle Farben mit Pixelanzahl und %. Klick = „From“-Farbe, Doppelklick = Hex-Code kopieren.

REITER OVERLAY (Grafiken darauflegen)
• Overlay laden aus Datei, Zwischenablage, gewählter Textur oder einem art.mul-Item / einer Landkachel.
  „Black / white transparent“ entfernt den UO-Hintergrund von Art.
• Verschieben mit dem Werkzeug „Move overlay“ (ziehen, Pfeiltasten, Umschalt = 10 px), Deckkraft, Skalierung,
  X / Y, zentrieren, einpassen.
• „Apply (burn in)“ zeichnet es ins Bild. „Remove“ verwirft es.
• Auswahl füllen mit einer Bilddatei oder dem Overlay: wiederholen (Kacheln) oder strecken.

REITER TEXMAPS (einladen und speichern)
• Alle Texturen der texmaps.mul: Filter All / Used / Free (frei = rot), ID-Feld + Go, „Next free“.
• Die Vorschau zeigt die Textur und die daraus entstehende Landkachel, dazu welche Landkacheln sie benutzen (tiledata).
• „→ Editor“, „→ Overlay“ (über dem Bild einblenden, z. B. zum Vergleichen), „Copy texture“.
• „Image → ID“ schreibt das Bild in die gewählte ID. Nicht 64 / 128? Es wird gefragt, ob skaliert werden soll.
• „Remove ID“, „Reload list“. Alle Änderungen sind im Speicher, bis „Save texmaps.mul“
  (auch im File-Menü) texidx.mul und texmaps.mul in den UoFiddler-Ausgabeordner schreibt.

SPEICHERN
• Save (Strg+S): in tempGrafic neben UoFiddler, Format aus der Werkzeugleiste.
• Save as (Strg+Umschalt+S): eigener Name und Ordner. Kopieren (Strg+C), Auswahl kopieren (Strg+Umschalt+C).

RÜCKGÄNGIG
Strg+Z / Strg+Y, 30 Schritte. Schreiben in texmaps.mul / art.mul gehört nicht dazu.

TASTENKÜRZEL
Strg+O öffnen · Strg+V einfügen · Strg+S speichern · Strg+C kopieren · Strg+Z / Y rückgängig / wiederholen ·
Strg+A alles auswählen · Strg+G Raster · Strg+T 3 × 3 Ansicht · Strg+F einpassen · Strg+0 1:1 · Strg + / – Zoom ·
Enter Vorschau übernehmen · Esc abbrechen · Entf Auswahl löschen · M B E L R G I O Werkzeuge · X Farben tauschen · F1 Hilfe";

        public static void Show(IWin32Window owner, bool german)
        {
            var form = new Form
            {
                Text = "Help / Hilfe – Texture Cutter",
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
