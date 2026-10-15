// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Controls.TerrainLibrary.Forms
{
    /// <summary>
    /// Help window of the terrain library. The text is built in <see cref="BuildHelp"/> - edit it there.
    /// </summary>
    public partial class TerrainHelpForm : Form
    {
        private Font _headingFont;
        private Font _textFont;

        public TerrainHelpForm()
        {
            InitializeComponent();
            BuildHelp();
        }

        private void Heading(string text)
        {
            richTextBoxHelp.SelectionFont = _headingFont;
            richTextBoxHelp.SelectionColor = Color.DarkRed;
            richTextBoxHelp.AppendText(Environment.NewLine + text + Environment.NewLine);
        }

        private void Line(string text)
        {
            richTextBoxHelp.SelectionFont = _textFont;
            richTextBoxHelp.SelectionColor = SystemColors.WindowText;
            richTextBoxHelp.AppendText(text + Environment.NewLine);
        }

        private void BuildHelp()
        {
            _headingFont = new Font(richTextBoxHelp.Font.FontFamily, 11f, FontStyle.Bold);
            _textFont = new Font(richTextBoxHelp.Font.FontFamily, 9.5f, FontStyle.Regular);

            richTextBoxHelp.Clear();

            Heading("Was ist die Terrain-Bibliothek?");
            Line("Eine eigene, frei benannte Ablage für LandTiles (44x44) und Texturen (64x64 / 128x128) mit");
            Line("gemeinsamen Slots: Slot 0x0003 kann ein LandTile, eine Textur oder beides enthalten - dazu");
            Line("Einstellungen (tiledata-Name, Flags, Textur-ID, RadarColor), Name, Kategorie, Tags und Beschreibung.");
            Line("Dateien: Name.landlib.idx/.mul (LandTiles), Name.texlib.idx/.mul (Texturen), Name.terrain.json.");
            Line("Die Bibliothek ändert die Originaldateien nur, wenn du es mit einem Button ausdrücklich willst.");

            Heading("Zwei Fenster, eine Bibliothek");
            Line("• LandTile-Fenster: Button im Land-Tiles-Tab.  Textur-Fenster: Button im Textures-Tab.");
            Line("• \"Texturen ▶\" / \"LandTiles ▶\" öffnet das andere Fenster. Beide arbeiten auf derselben Bibliothek.");
            Line("• \"Andocken\": Texturen rechts neben den LandTiles - beide bewegen sich gemeinsam. Aus = frei verschiebbar.");
            Line("• Optionen > Auswahl synchronisieren: ein Klick auf einen Slot wählt ihn in beiden Fenstern.");
            Line("• Rechts unten in jeder Vorschau: das Partnerbild (Textur bzw. LandTile) und die RadarColor des Slots.");
            Line("• Kleine Markierungen im Raster: blau = Partner vorhanden, Farbe/grün = Einstellungen vorhanden.");

            Heading("Speichern");
            Line("• Auto-Speichern AN: jede Änderung landet sofort in den Bibliotheksdateien (Statusleiste grün).");
            Line("• Auto-Speichern AUS: erst mit Speichern (Diskette, Strg+S). Ungespeichert = orange + * im Titel,");
            Line("  Rückfrage beim Schließen des letzten Fensters oder beim Wechseln der Bibliothek.");
            Line("• Bibliotheksdateien und Originaldateien sind getrennt: art.mul, texmaps.mul, tiledata.mul und");
            Line("  radarcol.mul speicherst du wie gewohnt in ihrem Tab.");

            Heading("Raster und freie Slots");
            Line("• \"Freie Slots\" AN: alle Slots von 0 bis Ende, freie als rotes Feld mit Hex-ID - Slot anklicken,");
            Line("  dann einfügen/importieren. AUS: nur belegte Slots dieses Fensters.");
            Line("• ID-Feld: dezimal oder hex (0x4D2, 4D2h), Enter springt hin.");

            Heading("Importieren, Zwischenablage, Export");
            Line("• Strg+C / Strg+V wie in den Original-Tabs; mehrere Grafiken werden verlustfrei übertragen.");
            Line("• Bilder aus anderen Programmen, Dateien aus dem Explorer (auch auf die Vorschau ziehen).");
            Line("• Optionen > Auto-Skalieren: andere Größen werden auf 44x44 bzw. 64/128 skaliert (Hinweis in der");
            Line("  Statusleiste). Aus = solche Bilder werden mit Meldung abgelehnt.");
            Line("• LandTiles und Texturen sind deckend - Schwarz/Weiß werden hier NICHT transparent gemacht.");
            Line("• Export: Rechtsklick > In Ordner exportieren (\"Landtile 0x0003.bmp\" / \"Texture 0x0003.bmp\").");

            Heading("Bearbeiten");
            Line("• Bearbeiten > 90° drehen, spiegeln - für alle markierten Grafiken dieses Fensters.");
            Line("• \"LandTile aus Textur des Slots erzeugen\": macht aus der Textur das 44x44-LandTile im selben Slot.");
            Line("  Optionen: isometrisch (45° wie im Client) oder zeilenweise (wie Texture Window \"Make Tile\").");
            Line("• Slot(s) kopieren/verschieben nach / zwei Slots tauschen: immer mit LandTile, Textur und Einstellungen.");
            Line("  Belegte Ziele werden gemeldet bzw. übersprungen.");

            Heading("Austausch mit dem Original");
            Line("• Original > Auswahl aus dem Original-Tab holen / Aus Original holen (IDs, z. B. 0x0003-0x0010).");
            Line("  Beim LandTile werden je nach Optionen auch tiledata-Werte, RadarColor und die zugehörige Textur");
            Line("  (tiledata-Textur-ID) in denselben Slot übernommen. Hat ein Ziel-Slot schon Einstellungen, wirst du gefragt.");
            Line("• Original > Ins Original kopieren/verschieben: LandTiles nach art.mul, Texturen nach texmaps.mul.");
            Line("  RadarColor wird nur mitgeschrieben, wenn Optionen > \"RadarColor mitschreiben\" an ist.");
            Line("• tiledata-Werte → tiledata.mul und RadarColor → radarcol.mul: nur auf Knopfdruck.");

            Heading("Einstellungen pro Slot und Vorlagen");
            Line("• Reiter \"tiledata\": Name, Textur-ID und Flags des Slots - gespeichert nur in der Bibliothek.");
            Line("  \"Aus tiledata holen\" liest die Werte der Herkunfts-ID, \"→ tiledata.mul\" schreibt sie ins Original.");
            Line("• Reiter \"Farbe\": RadarColor setzen (Mittelwert, Farbwähler, 0x1234 oder #RRGGBB, aus radarcol).");
            Line("• Vorlagen: fertige Einstellungen (z. B. Gras, Wasser, Fels) mit einem Klick auf einen oder viele Slots.");
            Line("  Standardvorlagen lassen sich in Optionen ausblenden; eigene Vorlagen legst du unter");
            Line("  Bibliothek > Vorlagen verwalten an oder mit \"Als Vorlage speichern\" aus einem Slot.");
            Line("• Hat ein Slot bereits Einstellungen, fragt das Programm vor dem Ersetzen.");

            Heading("Ordnen und finden");
            Line("• Name, Kategorie (auch Unterkategorien), Tags, Beschreibung, Favorit im Reiter \"Info\".");
            Line("• Filterleiste: Suche (Name, tiledata-Name, Tag, Kategorie, Vorlage, Farbe, ID), Kategorie, Tag,");
            Line("  Favoriten, \"Mit Partner\" (LandTile + Textur), \"Mit Einstellungen\".");

            Heading("Tastenkürzel");
            Line("Strg+C kopieren  ·  Strg+V einfügen  ·  Entf Grafik entfernen  ·  Strg+S speichern");
            Line("Strg+G ID-Feld  ·  Strg+F Suche  ·  F1 Hilfe");

            richTextBoxHelp.SelectionStart = 0;
            richTextBoxHelp.ScrollToCaret();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _headingFont?.Dispose();
            _textFont?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
