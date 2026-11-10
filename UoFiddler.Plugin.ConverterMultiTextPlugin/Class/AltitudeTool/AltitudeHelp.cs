using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AltitudeTool
{
    /// <summary>Help window of the altitude tool (English / German).</summary>
    internal static class AltitudeHelp
    {
        public const string English =
@"ALTITUDE TOOL – HELP

PURPOSE
Turns the land heights of a UO map into an image and back. It replaces AltitudeTool.exe ((c) 2007 Andreas Schneider):
its three commands extract, apply and modify are built in – the external program is no longer needed.
The image: 1 pixel per tile, 8 bit gray, gray = z + 128 (black = z −128, 128 = z 0, 255 = z 127).

WORKFLOW
1. Tab Map: open a map*.mul (the height in blocks is detected) or load the map that UoFiddler uses (also UOP).
2. Extract the heights as an image, edit it in any image program, and apply it again – or edit directly here.
3. Tab Map: ""Save map"" writes everything into map*.mul (and moves the statics). Reload the map in UoFiddler afterwards.
All changes stay in memory until you save, with undo / redo. Changed tiles are tinted red (""Changes"").

VIEW
Mouse wheel = zoom (powers of two), right or middle mouse = move, ""Fit"" = whole map, ""Sel"" = zoom to selection.
Colors: gray like UO, gray stretched, terrain colors, hillshade, terrain + shade; contour lines every 5 / 10 / 20 z;
8 × 8 block grid from zoom 4×; from zoom 24× every tile shows its altitude. Status bar: X, Y, Z (and the old z).

TOOLS (toolbar)
• Select (S): drag an area – export and all operations use it (no selection = whole map).
• Raise (R), Lower (L), Smooth (M), Flatten (F): round brushes with radius r and strength str.
• Pipette (I): picks an altitude for Flatten and ""Set to"".

TAB MAP
• Map file: map*.mul + height in blocks (512 Felucca / Trammel / Ter Mur, 200 Ilshenar, 256 Malas, 181 Tokuno).
  staidx / statics next to it are found automatically.
• Map loaded in UoFiddler: reads map*.mul or map*LegacyMUL.uop; saving then writes a new map*.mul.
• Statics: ""Move statics with the land"" gives every static the same height change as the land below it.
• Save: into the output folder (default) or over the source file with a .bak backup. ""Revert all"" = back to the loaded state.

TAB EXTRACT (original ""extract"")
Area: whole map, selection or visible view. Formats: BMP 8 bit (exactly like the original), PNG 8 bit gray,
PNG as shown (colors / shade / lines, e.g. for a world map), RAW 8 bit for terrain programs. ""Copy as shown"" = clipboard.

TAB APPLY (original ""apply"")
Image → heights: z = gray − 128 (color images use their brightness). New: the image may be smaller than the map and is put at X / Y
(""= selection"" takes the selection corner); transparent PNG pixels keep the map; ""Relative"" adds (gray − 128) instead of setting.

TAB MODIFY (original ""modify"")
Works on the selection or the whole map: offset (raise / lower, kept within −128 … 127 like the original), set to z, smooth,
scale around a base height (steeper / flatter), limit to min … max, terraces (steps), natural random hills.

TAB INFO
Map size and blocks, min / max / average altitude, unsaved changes, histogram of the selection, selection by numbers,
go to X / Y, jump to the next changed block.

KEYBOARD
Ctrl+O open · Ctrl+S save · Ctrl+Z / Ctrl+Y undo / redo · Ctrl+A select all · Esc no selection ·
S R L M F I tools · + / − zoom · 0 whole map · F1 help";

        public const string German =
@"ALTITUDE TOOL – HILFE

ZWECK
Wandelt die Landhöhen einer UO-Karte in ein Bild und zurück. Ersetzt AltitudeTool.exe ((c) 2007 Andreas Schneider):
seine drei Befehle extract, apply und modify sind eingebaut – das externe Programm wird nicht mehr gebraucht.
Das Bild: 1 Pixel pro Kachel, 8-Bit-Grau, Grau = z + 128 (schwarz = z −128, 128 = z 0, 255 = z 127).

ABLAUF
1. Reiter Map: eine map*.mul öffnen (die Höhe in Blöcken wird erkannt) oder die Karte laden, die UoFiddler benutzt (auch UOP).
2. Höhen als Bild extrahieren, in einem Malprogramm bearbeiten und wieder anwenden – oder direkt hier bearbeiten.
3. Reiter Map: „Save map“ schreibt alles in die map*.mul (und bewegt die Statics mit). Danach die Karte in UoFiddler neu laden.
Alle Änderungen bleiben bis zum Speichern im Speicher, mit Rückgängig / Wiederholen. Geänderte Kacheln sind rot getönt („Changes“).

ANSICHT
Mausrad = Zoom (Zweierpotenzen), rechte oder mittlere Maus = verschieben, „Fit“ = ganze Karte, „Sel“ = auf die Auswahl zoomen.
Farben: Grau wie UO, Grau gestreckt, Geländefarben, Schummerung, Gelände + Schummerung; Höhenlinien alle 5 / 10 / 20 z;
8 × 8 Blockraster ab Zoom 4×; ab Zoom 24× zeigt jede Kachel ihre Höhe. Statusleiste: X, Y, Z (und das alte z).

WERKZEUGE (Leiste)
• Select (S): Bereich aufziehen – Export und alle Bearbeitungen benutzen ihn (keine Auswahl = ganze Karte).
• Raise (R), Lower (L), Smooth (M), Flatten (F): runde Pinsel mit Radius r und Stärke str.
• Pipette (I): nimmt eine Höhe für Flatten und „Set to“ auf.

REITER MAP
• Kartendatei: map*.mul + Höhe in Blöcken (512 Felucca / Trammel / Ter Mur, 200 Ilshenar, 256 Malas, 181 Tokuno).
  staidx / statics daneben werden automatisch gefunden.
• Karte aus UoFiddler: liest map*.mul oder map*LegacyMUL.uop; Speichern schreibt dann eine neue map*.mul.
• Statics: „Move statics with the land“ gibt jedem Static dieselbe Höhenänderung wie dem Land darunter.
• Speichern: in den Ausgabeordner (Standard) oder über die Quelldatei mit .bak-Sicherung. „Revert all“ = zurück zum geladenen Stand.

REITER EXTRACT (Original „extract“)
Bereich: ganze Karte, Auswahl oder sichtbare Ansicht. Formate: BMP 8 Bit (genau wie das Original), PNG 8 Bit Grau,
PNG wie angezeigt (Farben / Schummerung / Linien, z. B. für eine Weltkarte), RAW 8 Bit für Geländeprogramme. „Copy as shown“ = Zwischenablage.

REITER APPLY (Original „apply“)
Bild → Höhen: z = Grau − 128 (Farbbilder benutzen ihre Helligkeit). Neu: das Bild darf kleiner als die Karte sein und kommt an X / Y
(„= selection“ übernimmt die Auswahlecke); transparente PNG-Pixel behalten die Karte; „Relative“ addiert (Grau − 128), statt zu setzen.

REITER MODIFY (Original „modify“)
Wirkt auf die Auswahl oder die ganze Karte: Offset (heben / senken, bleibt in −128 … 127 wie im Original), auf z setzen, glätten,
um eine Basishöhe skalieren (steiler / flacher), auf min … max begrenzen, Terrassen (Stufen), natürliche Zufallshügel.

REITER INFO
Kartengröße und Blöcke, min / max / mittlere Höhe, ungespeicherte Änderungen, Histogramm der Auswahl, Auswahl per Zahlen,
zu X / Y springen, zum nächsten geänderten Block springen.

TASTEN
Strg+O öffnen · Strg+S speichern · Strg+Z / Strg+Y rückgängig / wiederholen · Strg+A alles auswählen · Esc keine Auswahl ·
S R L M F I Werkzeuge · + / − Zoom · 0 ganze Karte · F1 Hilfe";

        public static void Show(IWin32Window owner, bool german)
        {
            var form = new Form
            {
                Text = "Help / Hilfe – Altitude Tool",
                Size = new Size(900, 760),
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
