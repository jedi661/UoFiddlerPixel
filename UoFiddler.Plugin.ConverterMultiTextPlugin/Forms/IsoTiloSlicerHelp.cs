// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class IsoTiloSlicerHelp : Form
    {
        public IsoTiloSlicerHelp()
        {
            InitializeComponent();
            rtbHelp.Rtf = BuildRtf();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private static string Esc(string t)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in t)
            {
                if (c == '\\' || c == '{' || c == '}')
                {
                    sb.Append('\\').Append(c);
                }
                else if (c == '\n')
                {
                    sb.Append("\\par\n");
                }
                else if (c > 127)
                {
                    sb.Append("\\u").Append((int)c).Append('?');
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private string BuildRtf()
        {
            return "{\\rtf1\\ansi\\deff0{\\fonttbl{\\f0\\fswiss Segoe UI;}{\\f1\\fmodern Consolas;}}{\\colortbl;\\red0\\green0\\blue0;\\red0\\green100\\blue180;}\\f0\\fs20 "
                   + "{\\fs28\\b\\cf2 " + Esc("Iso Tile Slicer – Help / Hilfe") + "}\\par\\par "
                   + Esc(English) + "\\par\\par{\\fs28\\b\\cf2 Deutsch}\\par\\par " + Esc(German) + "}";
        }

        private const string English =
@"PURPOSE
Cuts a large picture into tiles: UO land tile diamonds (44 × 44, every second row shifted by half a tile) or straight
rectangles (textures, gumps …). The tiles go into a folder (with layout.html, layout.csv, grid.png) or directly into art.mul.

STEPS
1. Open (Ctrl+O) or paste (Ctrl+V) a picture.
2. Cut: choose the mode or a preset (UO land tile 44 × 44, texture 64 / 128 …). ""Grid shift X / Y"" moves the cut grid.
3. Check in the views (below), then ""Slice & save"" (Ctrl+S) or ""Tiles → art.mul"".

VIEWS
• 2D + cut grid: the picture with every cut (green), empty tiles hatched red, numbers, click = select a tile.
  The status bar shows pixel, color, tile column / row and map position.
• Assembled: the cut tiles put back together. ""Gap"" pulls them apart so every cut is visible.
• Map 3D / 2D: the tiles as land on a map – isometric like the client or from above, flat or with heights
  (brightness, darkness, rolling hills), light and grid. Shows how the tiles look in the game.
• Tiles: all tiles; double click shows the tile in the 2D view.
Mouse wheel = zoom, right mouse = move, ""Fit"" = whole picture.

IMAGE
Mirror, rotate 90°, scale in %, ""Fit width to N tiles"" (the picture becomes exactly N tiles wide), ""Original"" = undo.
Outside: what parts outside the picture become – black (transparent in UO), transparent or a color.

OUTPUT
Folder (default tempGrafic), format BMP / PNG / TIFF, name ({0} number, {1} column, {2} row, {3} map X, {4} map Y),
start number, skip empty tiles. layout.html shows the tiles at their place, layout.csv lists number, file, column,
row, position and map position of every tile.

ART.MUL
Land tiles (44 × 44 iso) or items from an ID; ""Free"" finds enough free IDs in a row; used IDs can be skipped.
Afterwards the numbers in the views are the IDs. ""Save art.mul"" writes the file.

KEYS
Ctrl+O open · Ctrl+V paste · Ctrl+S slice & save · Ctrl+C copy tile · + / − / 0 zoom · F1 help";

        private const string German =
@"ZWECK
Zerschneidet ein großes Bild in Kacheln: UO-Landkachel-Rauten (44 × 44, jede zweite Reihe um eine halbe Kachel versetzt)
oder gerade Rechtecke (Texturen, Gumps …). Die Kacheln kommen in einen Ordner (mit layout.html, layout.csv, grid.png)
oder direkt in die art.mul.

ABLAUF
1. Bild öffnen (Strg+O) oder einfügen (Strg+V).
2. Schnitt: Modus oder Vorgabe wählen (UO-Landkachel 44 × 44, Textur 64 / 128 …). „Grid shift X / Y“ verschiebt das Raster.
3. In den Ansichten prüfen (unten), dann „Slice & save“ (Strg+S) oder „Tiles → art.mul“.

ANSICHTEN
• 2D + Schnittraster: das Bild mit jedem Schnitt (grün), leere Kacheln rot schraffiert, Nummern, Klick = Kachel wählen.
  Die Statusleiste zeigt Pixel, Farbe, Spalte / Zeile der Kachel und Kartenposition.
• Assembled: die geschnittenen Kacheln wieder zusammengesetzt. „Gap“ zieht sie auseinander, damit jeder Schnitt sichtbar wird.
• Map 3D / 2D: die Kacheln als Land auf einer Karte – isometrisch wie im Client oder von oben, flach oder mit Höhen
  (Helligkeit, Dunkelheit, Hügel), Licht und Raster. Zeigt, wie die Kacheln im Spiel aussehen.
• Tiles: alle Kacheln; Doppelklick zeigt die Kachel in der 2D-Ansicht.
Mausrad = Zoom, rechte Maustaste = verschieben, „Fit“ = ganzes Bild.

BILD
Spiegeln, 90° drehen, skalieren in %, „Fit width to N tiles“ (das Bild wird genau N Kacheln breit), „Original“ = zurück.
Outside: was Teile außerhalb des Bildes werden – schwarz (in UO durchsichtig), transparent oder eine Farbe.

AUSGABE
Ordner (Standard tempGrafic), Format BMP / PNG / TIFF, Name ({0} Nummer, {1} Spalte, {2} Zeile, {3} Karte X, {4} Karte Y),
Startnummer, leere Kacheln überspringen. layout.html zeigt die Kacheln an ihrem Platz, layout.csv listet Nummer, Datei,
Spalte, Zeile, Position und Kartenposition jeder Kachel.

ART.MUL
Landkacheln (44 × 44 iso) oder Items ab einer ID; „Free“ sucht genug freie IDs am Stück; belegte IDs lassen sich überspringen.
Danach sind die Nummern in den Ansichten die IDs. „Save art.mul“ schreibt die Datei.

TASTEN
Strg+O öffnen · Strg+V einfügen · Strg+S schneiden & speichern · Strg+C Kachel kopieren · + / − / 0 Zoom · F1 Hilfe";
    }
}