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
• Right: tabs Tiles, Colors, Replace, Layers, Blend, texmaps.
• Menu Tools / toolbar: Terrain preview (Ctrl+P), Batch (Ctrl+B), pattern brush.
• Status bar: pixel position, color under the mouse (hex and RGB), image size, selection, messages.

LOAD
• File > Open image (Ctrl+O): PNG, BMP, GIF, JPG, TIFF. The file is not locked.
• Paste (Ctrl+V): from the clipboard (PNG keeps transparency).
• Tab texmaps: double click a texture or ""→ Editor"".
• Tab Tiles > ""Load land tile"": a land tile from art.mul.
• Image > New empty image: transparent image in the tile size.

TOOLS (shortcut)
Select (M) · Pencil (B) · Eraser (E) · Line (L) · Rectangle (R) · Fill (G) · Pipette (I) · Move layer (O)
• Left mouse = color 1, right mouse = color 2. X swaps the colors. Brush size in the toolbar.
• ""Mirror draw"" paints every stroke mirrored left / right as well.
• Eraser makes pixels transparent (option on the Replace tab: erase to white).
• Fill uses the tolerance from the Replace tab.
• Selection shapes: rectangle, ellipse, freehand, polygon (click points, double or right click closes).
  Shift = square / circle. Ctrl+A = all, Esc = remove, Del = make the selection transparent.
• ""Only selection"" (toolbar): filters, color changes and replacing only act inside the selection.
• ""Protect B/W"": pure black and white (UO transparency) stay unchanged by color changes.
• Pattern brush (toolbar ""Pattern"", arrow = choose): pencil, line, rectangle and fill paint with an image
  (from a layer, the selection, a texture, a file or the clipboard) instead of a color. Click = on / off.

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
• Palette: left click = color 1, right click = color 2, Ctrl + click = remove. ""+ Color 1"", ""+ From image"" (16 most
  common colors), ""+ Hue"" (the 32 colors of a UO hue from hues.mul), Load / Save (JASC .pal, GIMP .gpl, hex list).
  The palette is kept automatically between sessions.
• From / To color: click the color field, type a hex code or use ""Pick"" and click a pixel.
• Tolerance in % (0 = exact). ""Keep shading"" keeps light and dark of the replaced pixels.
• ""Replace"", ""Erase → transparent"", ""→ white"" (old ""Delete"").
• Colors in the image: all colors with pixel count and %. Click = ""From"" color, double click = copy hex code.

TAB LAYERS (graphics on top)
• A layer is an image above the base image that stays movable: from a file, the clipboard, a texture or an
  art.mul item / land tile. ""Black / white transparent"" removes the UO background of art.
• List: top layer first, checkbox = visible, selected = the layer you move and change. ▲ ▼ order, Duplicate, Remove.
• Move with the tool ""Move layer"" (click a layer to select it, drag, arrow keys, Shift = 10 px),
  opacity, scale, X / Y, center, fit.
• Visible layers are part of everything that leaves the editor: save, copy, tiles, texmaps / art.mul, terrain preview.
  Filters and color changes only change the base image. ""Burn in"" draws layers into the base image for good.
• Fill the selection with an image file or the selected layer: repeat (tile) or stretch.

TAB BLEND (transitions)
• Texture A = the edited image, texture B = from texmaps, a layer, a file or the clipboard.
• Shape: left → right, top → bottom, two diagonals, four corners, island. Softness = width of the soft border,
  Rough = irregular natural edge, Pattern = which irregular edge.
• ""Show preview"", then ""Apply"". ""All 9 → files"" or ""All 9 → texmaps"" creates the whole set at once.
• ""Check in terrain preview"" shows the result on the ground next to other textures.

TAB TEXMAPS (load in and save out)
• All textures of texmaps.mul: filter All / Used / Free (free = red), ID box + Go, ""Next free"".
• The preview shows the texture and the land tile made from it, plus which land tiles use it (tiledata).
• ""→ Editor"", ""→ Layer"" (show it as a layer over the image to compare), ""Copy texture"".
• ""Image → ID"" writes the image into the selected ID. Not 64 / 128? It asks whether to scale.
• ""Image → its land tiles"": the image becomes a 44 × 44 land tile and is written into every land tile that uses the
  selected texture according to tiledata.mul (save art.mul in UoFiddler afterwards).
• ""Remove ID"", ""Reload list"". All changes are in memory until ""Save texmaps.mul""
  (also File menu) writes texidx.mul and texmaps.mul into the UoFiddler output folder.

TERRAIN PREVIEW (Ctrl+P) – how the texture looks on the ground
• 3D (isometric, with heights) like the client: flat tiles show the 44 × 44 land tile, sloped tiles the stretched texture
  (two triangles), light on the slopes. Or 2D from above. Zoom, grid, heights, save PNG, copy.
• ""Editor image"" is always the first ground and follows every change in the editor (with layers).
• Grounds: + texmaps ID, + land ID (art.mul, with its texture from tiledata), + file. Selected = brush, red = picked.
• Tools: Paint, Raise, Lower (right click = the other way), Flatten (to Z), Smooth, Pick (also right click while painting);
  radius and step. Area: size 8 – 64, new, random hills, flatten / smooth all, checker A / B (shows every edge).
• Map area: choose a map and X / Y, ""Load area"" reads land tiles and heights of the real map.
  Pick a ground and ""Replace picked → brush"" shows your texture in its place – with the real neighbours and slopes.

BATCH (Ctrl+B) – many images at once
• Add files or a whole folder (or drop them on the list). Steps: resize, make seamless, UO 16-bit, texture → land tile.
• Targets: files into a folder, texmaps.mul from an ID, art.mul land tiles from an ID (used IDs skipped if wanted).

SAVE
• Save (Ctrl+S): into tempGrafic next to UoFiddler, format from the toolbar.
• Save as (Ctrl+Shift+S): own name and folder. Copy (Ctrl+C), copy selection (Ctrl+Shift+C).

UNDO
Ctrl+Z / Ctrl+Y, 30 steps of the base image. Layers and writing to texmaps.mul / art.mul are not part of it.

KEYBOARD
Ctrl+O open · Ctrl+V paste · Ctrl+S save · Ctrl+C copy · Ctrl+Z / Y undo / redo · Ctrl+A select all ·
Ctrl+G grid · Ctrl+T 3 × 3 view · Ctrl+F fit · Ctrl+0 1:1 · Ctrl + / – zoom · Ctrl+P terrain · Ctrl+B batch · Enter apply preview ·
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
• Rechts: Reiter Tiles, Colors, Replace, Layers, Blend, texmaps.
• Menü Tools / Werkzeugleiste: Gelände-Vorschau (Strg+P), Batch (Strg+B), Muster-Pinsel.
• Statusleiste: Pixelposition, Farbe unter der Maus (Hex und RGB), Bildgröße, Auswahl, Meldungen.

LADEN
• File > Open image (Strg+O): PNG, BMP, GIF, JPG, TIFF. Die Datei wird nicht gesperrt.
• Einfügen (Strg+V): aus der Zwischenablage (PNG behält die Transparenz).
• Reiter texmaps: Doppelklick auf eine Textur oder „→ Editor“.
• Reiter Tiles > „Load land tile“: eine Landkachel aus der art.mul.
• Image > New empty image: transparentes Bild in Kachelgröße.

WERKZEUGE (Taste)
Auswahl (M) · Stift (B) · Radierer (E) · Linie (L) · Rechteck (R) · Füllen (G) · Pipette (I) · Ebene verschieben (O)
• Linke Maustaste = Farbe 1, rechte = Farbe 2. X tauscht die Farben. Pinselgröße in der Werkzeugleiste.
• „Mirror draw“ zeichnet jeden Strich zusätzlich links / rechts gespiegelt.
• Der Radierer macht Pixel transparent (Option im Reiter Replace: zu Weiß radieren).
• Füllen benutzt die Toleranz aus dem Reiter Replace.
• Auswahlformen: Rechteck, Ellipse, Freihand, Polygon (Punkte klicken, Doppel- oder Rechtsklick schließt).
  Umschalt = Quadrat / Kreis. Strg+A = alles, Esc = entfernen, Entf = Auswahl transparent machen.
• „Only selection“ (Werkzeugleiste): Filter, Farbänderungen und Ersetzen wirken nur in der Auswahl.
• „Protect B/W“: reines Schwarz und Weiß (UO-Transparenz) bleiben bei Farbänderungen unverändert.
• Muster-Pinsel (Werkzeugleiste „Pattern“, Pfeil = wählen): Stift, Linie, Rechteck und Füllen malen mit einem Bild
  (aus einer Ebene, der Auswahl, einer Textur, einer Datei oder der Zwischenablage) statt mit einer Farbe. Klick = an / aus.

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
• Palette: Linksklick = Farbe 1, Rechtsklick = Farbe 2, Strg + Klick = entfernen. „+ Color 1“, „+ From image“ (16 häufigste
  Farben), „+ Hue“ (die 32 Farben eines UO-Hues aus hues.mul), Load / Save (JASC .pal, GIMP .gpl, Hex-Liste).
  Die Palette bleibt automatisch bis zum nächsten Start erhalten.
• From- / To-Farbe: Farbfeld anklicken, Hex-Code eintippen oder „Pick“ und ein Pixel anklicken.
• Toleranz in % (0 = genau). „Keep shading“ behält Hell und Dunkel der ersetzten Pixel.
• „Replace“, „Erase → transparent“, „→ white“ (früher „Delete“).
• Farben im Bild: alle Farben mit Pixelanzahl und %. Klick = „From“-Farbe, Doppelklick = Hex-Code kopieren.

REITER LAYERS (Grafiken darauflegen)
• Eine Ebene ist ein Bild über dem Grundbild, das verschiebbar bleibt: aus Datei, Zwischenablage, Textur oder einem
  art.mul-Item / einer Landkachel. „Black / white transparent“ entfernt den UO-Hintergrund von Art.
• Liste: oberste Ebene zuerst, Häkchen = sichtbar, ausgewählt = die Ebene, die du verschiebst und änderst.
  ▲ ▼ Reihenfolge, Duplicate, Remove.
• Verschieben mit dem Werkzeug „Move layer“ (Ebene anklicken, ziehen, Pfeiltasten, Umschalt = 10 px),
  Deckkraft, Skalierung, X / Y, zentrieren, einpassen.
• Sichtbare Ebenen gehören zu allem, was den Editor verlässt: Speichern, Kopieren, Kacheln, texmaps / art.mul,
  Gelände-Vorschau. Filter und Farbänderungen ändern nur das Grundbild. „Burn in“ zeichnet Ebenen fest ins Grundbild.
• Auswahl füllen mit einer Bilddatei oder der gewählten Ebene: wiederholen (Kacheln) oder strecken.

REITER BLEND (Übergänge)
• Textur A = das bearbeitete Bild, Textur B = aus texmaps, einer Ebene, einer Datei oder der Zwischenablage.
• Form: links → rechts, oben → unten, zwei Diagonalen, vier Ecken, Insel. Softness = Breite des weichen Übergangs,
  Rough = unregelmäßige natürliche Kante, Pattern = welche unregelmäßige Kante.
• „Show preview“, dann „Apply“. „All 9 → files“ oder „All 9 → texmaps“ erzeugt den ganzen Satz auf einmal.
• „Check in terrain preview“ zeigt das Ergebnis auf dem Boden neben anderen Texturen.

REITER TEXMAPS (einladen und speichern)
• Alle Texturen der texmaps.mul: Filter All / Used / Free (frei = rot), ID-Feld + Go, „Next free“.
• Die Vorschau zeigt die Textur und die daraus entstehende Landkachel, dazu welche Landkacheln sie benutzen (tiledata).
• „→ Editor“, „→ Layer“ (als Ebene über dem Bild einblenden, z. B. zum Vergleichen), „Copy texture“.
• „Image → ID“ schreibt das Bild in die gewählte ID. Nicht 64 / 128? Es wird gefragt, ob skaliert werden soll.
• „Image → its land tiles“: das Bild wird eine 44 × 44 Landkachel und in jede Landkachel geschrieben, die laut
  tiledata.mul die gewählte Textur benutzt (danach art.mul in UoFiddler speichern).
• „Remove ID“, „Reload list“. Alle Änderungen sind im Speicher, bis „Save texmaps.mul“
  (auch im File-Menü) texidx.mul und texmaps.mul in den UoFiddler-Ausgabeordner schreibt.

GELÄNDE-VORSCHAU (Strg+P) – wie die Textur auf dem Boden aussieht
• 3D (isometrisch, mit Höhen) wie im Client: flache Kacheln zeigen die 44 × 44 Landkachel, schräge die gestreckte
  Textur (zwei Dreiecke), Licht auf den Hängen. Oder 2D von oben. Zoom, Raster, Höhen, PNG speichern, kopieren.
• „Editor image“ ist immer der erste Boden und folgt jeder Änderung im Editor (mit Ebenen).
• Böden: + texmaps-ID, + Land-ID (art.mul, mit ihrer Textur aus tiledata), + Datei. Ausgewählt = Pinsel, rot = aufgenommen.
• Werkzeuge: Malen, Anheben, Absenken (Rechtsklick = umgekehrt), Ebnen (auf Z), Glätten, Aufnehmen (auch Rechtsklick
  beim Malen); Radius und Schritt. Fläche: Größe 8 – 64, neu, zufällige Hügel, alles ebnen / glätten,
  Schachbrett A / B (zeigt jede Kante).
• Kartenbereich: Karte und X / Y wählen, „Load area“ liest Landkacheln und Höhen der echten Karte.
  Einen Boden aufnehmen und „Replace picked → brush“ zeigt deine Textur an seiner Stelle – mit den echten Nachbarn und Hängen.

BATCH (Strg+B) – viele Bilder auf einmal
• Dateien oder einen ganzen Ordner hinzufügen (oder auf die Liste ziehen). Schritte: Größe ändern, nahtlos machen,
  UO 16-Bit, Textur → Landkachel.
• Ziele: Dateien in einen Ordner, texmaps.mul ab einer ID, art.mul-Landkacheln ab einer ID (belegte IDs auf Wunsch überspringen).

SPEICHERN
• Save (Strg+S): in tempGrafic neben UoFiddler, Format aus der Werkzeugleiste.
• Save as (Strg+Umschalt+S): eigener Name und Ordner. Kopieren (Strg+C), Auswahl kopieren (Strg+Umschalt+C).

RÜCKGÄNGIG
Strg+Z / Strg+Y, 30 Schritte des Grundbildes. Ebenen und Schreiben in texmaps.mul / art.mul gehören nicht dazu.

TASTENKÜRZEL
Strg+O öffnen · Strg+V einfügen · Strg+S speichern · Strg+C kopieren · Strg+Z / Y rückgängig / wiederholen ·
Strg+A alles auswählen · Strg+G Raster · Strg+T 3 × 3 Ansicht · Strg+F einpassen · Strg+0 1:1 · Strg + / – Zoom ·
Strg+P Gelände · Strg+B Batch ·
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
