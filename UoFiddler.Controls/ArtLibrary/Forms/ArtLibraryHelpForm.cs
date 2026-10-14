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

namespace UoFiddler.Controls.ArtLibrary.Forms
{
    /// <summary>
    /// Help window of the art library. The text is built in <see cref="BuildHelp"/> - edit it there.
    /// </summary>
    public partial class ArtLibraryHelpForm : Form
    {
        private Font _headingFont;
        private Font _textFont;

        public ArtLibraryHelpForm()
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

            Heading("Was ist die Art-Bibliothek?");
            Line("Eine eigene, frei benannte \"zweite art.mul\" als Ablage für beliebig viele Item-Grafiken");
            Line("(Standard 1.000.000 IDs, jederzeit erweiterbar). Sie wird vom Client nicht gelesen - sie dient zum");
            Line("Sammeln, Sortieren, Suchen und zum Austausch mit der originalen art.mul im Items-Tab.");
            Line("Eine Bibliothek besteht aus drei Dateien: Name.artlib.idx, Name.artlib.mul und Name.artlib.json.");

            Heading("Bibliothek anlegen und öffnen");
            Line("• Neu (grünes +): Name, Ordner und Anzahl IDs wählen.");
            Line("• Öffnen: eine vorhandene *.artlib.idx auswählen (auch Bibliotheken von anderen).");
            Line("• Die Auswahlliste oben links enthält die zuletzt geöffneten Bibliotheken.");
            Line("• Menü \"Bibliothek\": Eigenschaften, Kategorien/Tags verwalten, Erweitern (mehr IDs),");
            Line("  Komprimieren (Platz von überschriebenen Grafiken freigeben), Ordner im Explorer öffnen.");

            Heading("Speichern");
            Line("• Auto-Speichern AN: jede Änderung wird sofort in die Dateien geschrieben.");
            Line("  Die Statusleiste zeigt \"Gespeichert\" mit Uhrzeit.");
            Line("• Auto-Speichern AUS: Änderungen bleiben im Speicher, bis du auf Speichern (Diskette) klickst.");
            Line("  Ungespeicherte Änderungen: * im Fenstertitel und orange Anzeige in der Statusleiste.");
            Line("  Beim Schließen oder Wechseln der Bibliothek wirst du gefragt.");
            Line("• Speichern (Diskette, Strg+S) geht immer - auch bei Auto-Speichern, zur Sicherheit.");
            Line("• \"Neu laden\" verwirft ungespeicherte Änderungen.");

            Heading("Raster und freie Slots");
            Line("• Das Raster funktioniert wie im Items-Tab: scrollen, anklicken, mit Strg/Shift mehrere markieren.");
            Line("• \"Freie Slots\" AN: alle IDs von 0 bis Ende werden gezeigt, freie Plätze als rotes Feld mit Hex-ID.");
            Line("  So klickst du direkt die ID an, auf die eine Grafik soll.");
            Line("• \"Freie Slots\" AUS: nur belegte IDs.");
            Line("• ID-Feld: dezimal (1234) oder hex (0x4D2, 4D2h) eingeben und Enter - springt zur ID.");

            Heading("Zwischenablage (wie im Items-Tab)");
            Line("• Strg+C kopiert die markierten Grafiken. Mehrere Grafiken werden verlustfrei übertragen,");
            Line("  zusätzlich liegt die erste als normales Bild (schwarzer Hintergrund) in der Zwischenablage.");
            Line("• Strg+V fügt auf der markierten ID ein (belegt: Rückfrage). Mehrere Grafiken: fortlaufend ab der");
            Line("  markierten ID, belegte IDs werden übersprungen. \"Einfügen auf die kopierten IDs\" behält die IDs.");
            Line("• Bilder aus anderen Programmen und Bilddateien aus dem Explorer können ebenfalls eingefügt werden.");
            Line("• Transparenz: Wie im Items-Tab werden reines Schwarz und reines Weiß (und #D3D3D3) beim Einfügen");
            Line("  transparent. Im Menü \"Optionen\" lässt sich das für Schwarz und Weiß getrennt abschalten.");
            Line("• Vom Items-Tab in die Bibliothek: im Items-Tab kopieren, in der Bibliothek Strg+V.");
            Line("• Von der Bibliothek in den Items-Tab: hier Strg+C, im Items-Tab \"Import from clipboard\" (Strg+V).");

            Heading("Austausch mit der originalen art.mul");
            Line("Menü \"Original art.mul\" (auch im Rechtsklick-Menü):");
            Line("• Auswahl aus dem Items-Tab holen - übernimmt die im Items-Tab markierten Grafiken.");
            Line("• Aus Original holen - IDs und Bereiche angeben, z. B. 0x1000-0x10FF, 1234.");
            Line("• Auswahl ins Original kopieren / verschieben - auf gleiche IDs oder fortlaufend ab einer Ziel-ID.");
            Line("  Wichtig: die art.mul danach wie gewohnt im Items-Tab speichern!");

            Heading("Import und Export über Ordner");
            Line("• Rechtsklick > Bilder importieren / Ordner importieren. Die ID wird aus dem Dateinamen gelesen");
            Line("  (Item 0x04D2.bmp, Art_Hex_Adress_4D2.bmp, 0x4D2.png, 1234.bmp) oder fortlaufend vergeben.");
            Line("• Bilddateien auf die Vorschau rechts ziehen: landen auf der markierten ID.");
            Line("• Rechtsklick > In Ordner exportieren: Auswahl oder alle als BMP / PNG / TIFF (Item 0x04D2.bmp).");

            Heading("Ordnen, suchen, finden");
            Line("• Rechts: Name, Kategorie, Tags (mit Komma getrennt), Beschreibung, Favorit - \"Übernehmen\".");
            Line("• Kategorien (auch Unterkategorien) und Tags legst du im Menü \"Bibliothek\" selbst an.");
            Line("• Rechtsklick: Kategorie zuweisen, Tag hinzufügen/entfernen, Favorit - auch für mehrere Grafiken.");
            Line("• Filterleiste: Suche nach Name, Beschreibung, Tag, Kategorie oder ID; Kategorie- und Tag-Auswahl;");
            Line("  \"Nur Favoriten\". Der Button \"Filter zurücksetzen\" hebt alle Filter auf.");
            Line("• Innerhalb der Bibliothek: kopieren/verschieben nach ID, zwei Grafiken tauschen, nächste freie ID.");

            Heading("Tastenkürzel");
            Line("Strg+C kopieren  ·  Strg+V einfügen  ·  Entf entfernen  ·  Strg+S speichern");
            Line("Strg+G ID-Feld  ·  Strg+F Suche  ·  F1 diese Hilfe");

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
