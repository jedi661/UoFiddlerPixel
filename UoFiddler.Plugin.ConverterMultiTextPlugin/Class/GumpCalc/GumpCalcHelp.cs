using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GumpCalc
{
    /// <summary>Help window of the UO calculator / Gump ID calculator (English / German).</summary>
    internal static class GumpCalcHelp
    {
        public const string English =
@"UO CALCULATOR (GUMP ID) – HELP

NUMBERS
Every input understands hex and decimal: 0x1517, 1517h, &h1517 or #1517 = hex, 5399 = decimal.
""Auto"" reads plain numbers with letters A-F as hex, all others as decimal. In the Gump ID tab you can
force Hex or Decimal (the old tool always read hex). Double click a yellow result box = copy.
Every calculation can be put into the history on the right (""→ History"" or Enter): copy, save, clear.

TAB GUMP ID – paperdoll gumps for men and women
How UO does it: every wearable item has the field ""Animation"" (AnimID) in tiledata.
  male gump   = AnimID + 50000 (0xC350)
  female gump = AnimID + 60000 (0xEA60)
If the female gump is missing, the client shows the male one. Items that only exist for women
(e.g. female leather) often use +50000 – tick ""Female-only item"".
• Input: item ID (AnimID read from tiledata), AnimID or a gump ID (male or female is detected).
• Result: item, art index, name, layer, all items that share the AnimID, AnimID and both gumps in hex / decimal.
• Checks: wearable flag, AnimID 0, missing gumps, gump beyond the end of gumpidx, above 0xFFFF.
• Preview: item, male and female gump – on the body (gump 0x0C / 0x0D) and with any hue.
• New clothing: ""Find free"" gives the next AnimID that no item uses and whose two gump places are empty.
• Script output: ServUO / RunUO class, Sphere ITEMDEF, POL itemdesc, gump.def line or a summary.

TAB WEARABLES
All items with the Wearable flag: ID, name, layer, AnimID, gumps and state. Filter by name / ID (item, AnimID or gump),
layer and state (male missing, female missing …). Red = no gump, orange = female missing. CSV export.
Double click opens the item in the Gump ID tab.

TAB BODY / ANIM
• Body ID → which anim file (bodyconv.def: anim2 … anim5), body number inside the file, type (monster 22 actions,
  animal 13, human 35) and the index in anim*.idx for action and direction. Preview of the first frame with hue.
• Mount items: items on layer 25 whose Animation is this body.
• Reverse: idx index of an anim file → body, action, direction.

TAB HUE / COLOR
• Hue value as in scripts / packets: number, hex, hues.mul entry (= number − 1), name, 32 colors.
  Bit 0x8000 = only gray pixels are hued (partial hue). Preview on any item.
• Color: RGB ↔ web hex ↔ 15 bit UO color (555), with the color as the client really shows it.
  Input: #RRGGBB, 0x7FFF or r,g,b. ""Find nearest hues"" searches all hues for the color.

TAB TILEDATA FLAGS
Tick flags → value in hex (64 / 32 bit) and decimal; or type a value → the flags are ticked.
Load the flags of an item / land tile and write them back (memory – save tiledata.mul in UoFiddler).

TAB MAP
• X / Y → block X / Y, block index, cell, offset in map*.mul (196 bytes per block) and staidx*.mul (12 bytes),
  number of blocks and file size, sextant position (like ServUO).
• Distance between two points: UO range (the larger of dx / dy) and straight line.
• Sextant → X / Y (accuracy about ± 7 tiles because the minutes are rounded).

TAB LAYER / ART INDEX
• All layers with number, hex, script name, German name and examples (layer = Quality field in tiledata).
• Item ID ↔ art index (+0x4000), land tile, multi ID with 0x4000.

TAB CONVERTER
• Number: decimal, hex, binary, octal, base N, 16 bit signed / unsigned, bytes little / big endian
  (UO files are little endian), character, as gump / as AnimID.
• Formula: 0xC350 + 435, (0x1517 - 0x1500) * 2, 0x481 | 0x8000 – Enter puts it into the history.
• Text: character codes and back, hex bytes UTF-8 / UO Unicode, upper / lower / title case.

KEYBOARD
Ctrl+1 … Ctrl+8 tabs · Enter in an input = history · F1 help";

        public const string German =
@"UO-RECHNER (GUMP ID) – HILFE

ZAHLEN
Jede Eingabe versteht Hex und Dezimal: 0x1517, 1517h, &h1517 oder #1517 = Hex, 5399 = Dezimal.
„Auto“ liest Zahlen mit Buchstaben A-F als Hex, alle anderen als Dezimal. Im Reiter Gump ID lässt sich
Hex oder Dezimal erzwingen (das alte Tool las immer Hex). Doppelklick auf ein gelbes Ergebnisfeld = kopieren.
Jede Berechnung kann in den Verlauf rechts („→ History“ oder Enter): kopieren, speichern, leeren.

REITER GUMP ID – Paperdoll-Gumps für Mann und Frau
So macht es UO: jedes tragbare Item hat in der tiledata das Feld „Animation“ (AnimID).
  Gump Mann = AnimID + 50000 (0xC350)
  Gump Frau = AnimID + 60000 (0xEA60)
Fehlt der Frauen-Gump, zeigt der Client den Männer-Gump. Items, die es nur für Frauen gibt
(z. B. Damen-Leder), benutzen oft +50000 – dann „Female-only item“ ankreuzen.
• Eingabe: Item-ID (AnimID wird aus tiledata gelesen), AnimID oder eine Gump-ID (Mann / Frau wird erkannt).
• Ergebnis: Item, Art-Index, Name, Layer, alle Items mit derselben AnimID, AnimID und beide Gumps in Hex / Dezimal.
• Prüfungen: Wearable-Flag, AnimID 0, fehlende Gumps, Gump hinter dem Ende der gumpidx, über 0xFFFF.
• Vorschau: Item, Gump Mann und Frau – auf dem Körper (Gump 0x0C / 0x0D) und mit beliebigem Hue.
• Neue Kleidung: „Find free“ liefert die nächste AnimID, die kein Item benutzt und deren beide Gump-Plätze leer sind.
• Skript-Ausgabe: ServUO / RunUO-Klasse, Sphere ITEMDEF, POL itemdesc, gump.def-Zeile oder Zusammenfassung.

REITER WEARABLES
Alle Items mit Wearable-Flag: ID, Name, Layer, AnimID, Gumps und Zustand. Filter nach Name / ID (Item, AnimID oder Gump),
Layer und Zustand (Mann fehlt, Frau fehlt …). Rot = kein Gump, orange = Frau fehlt. CSV-Export.
Doppelklick öffnet das Item im Reiter Gump ID.

REITER BODY / ANIM
• Body-ID → welche anim-Datei (bodyconv.def: anim2 … anim5), Body-Nummer in der Datei, Typ (Monster 22 Aktionen,
  Tier 13, Mensch 35) und der Index in anim*.idx für Aktion und Richtung. Vorschau des ersten Bildes mit Hue.
• Mount-Items: Items auf Layer 25, deren Animation dieser Body ist.
• Rückwärts: idx-Index einer anim-Datei → Body, Aktion, Richtung.

REITER HUE / COLOR
• Hue-Wert wie in Skripten / Packets: Nummer, Hex, hues.mul-Eintrag (= Nummer − 1), Name, 32 Farben.
  Bit 0x8000 = nur graue Pixel werden eingefärbt (Partial Hue). Vorschau auf einem beliebigen Item.
• Farbe: RGB ↔ Web-Hex ↔ 15-Bit-UO-Farbe (555), mit der Farbe, wie der Client sie wirklich zeigt.
  Eingabe: #RRGGBB, 0x7FFF oder r,g,b. „Find nearest hues“ sucht die Farbe in allen Hues.

REITER TILEDATA FLAGS
Flags ankreuzen → Wert in Hex (64 / 32 Bit) und Dezimal; oder einen Wert eingeben → die Flags werden angekreuzt.
Flags eines Items / einer Landkachel laden und zurückschreiben (Speicher – tiledata.mul in UoFiddler speichern).

REITER MAP
• X / Y → Block X / Y, Block-Index, Zelle, Offset in map*.mul (196 Byte pro Block) und staidx*.mul (12 Byte),
  Anzahl Blöcke und Dateigröße, Sextant-Position (wie ServUO).
• Abstand zweier Punkte: UO-Reichweite (das größere von dx / dy) und Luftlinie.
• Sextant → X / Y (Genauigkeit etwa ± 7 Felder, weil die Minuten gerundet sind).

REITER LAYER / ART INDEX
• Alle Layer mit Nummer, Hex, Skriptname, deutschem Namen und Beispielen (Layer = Feld Quality in tiledata).
• Item-ID ↔ Art-Index (+0x4000), Landkachel, Multi-ID mit 0x4000.

REITER CONVERTER
• Zahl: Dezimal, Hex, Binär, Oktal, Basis N, 16 Bit mit / ohne Vorzeichen, Bytes Little / Big Endian
  (UO-Dateien sind Little Endian), Zeichen, als Gump / als AnimID.
• Formel: 0xC350 + 435, (0x1517 - 0x1500) * 2, 0x481 | 0x8000 – Enter schreibt sie in den Verlauf.
• Text: Zeichencodes und zurück, Hex-Bytes UTF-8 / UO-Unicode, GROSS / klein / Wortanfang groß.

TASTEN
Strg+1 … Strg+8 Reiter · Enter in einer Eingabe = Verlauf · F1 Hilfe";

        public static void Show(IWin32Window owner, bool german)
        {
            var form = new Form
            {
                Text = "Help / Hilfe – UO Calculator",
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
