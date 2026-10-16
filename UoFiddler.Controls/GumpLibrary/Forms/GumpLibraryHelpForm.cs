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

namespace UoFiddler.Controls.GumpLibrary.Forms
{
    public partial class GumpLibraryHelpForm : Form
    {
        private Font _headingFont;
        private Font _textFont;

        public GumpLibraryHelpForm()
        {
            InitializeComponent();
            BuildHelp();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _headingFont?.Dispose();
            _textFont?.Dispose();
            base.OnFormClosed(e);
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

            Heading("Was ist die Gump-Bibliothek?");
            Line("Eine eigene, frei benannte Sammlung von Gumps (bis 1.000.000 IDs, erweiterbar) mit Namen,");
            Line("Beschreibung, Suchbegriffen (Tags), beliebig vielen Kategorien pro Gump, eigener Reihenfolge,");
            Line("Favoriten und eigenen Vorschaubildern. Geöffnet wird sie mit dem Button im Gumps-Tab.");
            Line("Dateien: Name.gumplib.idx/.mul (Gumps), Name.gumpprev.idx/.mul (Vorschaubilder), Name.gumplib.json (Metadaten).");
            Line("Die Original-Gumps (gumpart.mul) werden nur geändert, wenn du \"Ins Original schreiben\" benutzt.");

            Heading("Aufbau wie im Gumps-Tab");
            Line("• Links die Liste (60 Pixel hohe Zeilen, Vorschau, \"0xID (ID) Name\"), darüber Index-Suche, Misc und Save.");
            Line("• Rechts das große Bild, darunter die Leiste mit ID, Size, Kategorien, Speicherstatus und Anzahl.");
            Line("• Rechtsklick-Menü mit allen Funktionen des Gumps-Tabs (Export Image, Free Slots, Jump to Male/Female,");
            Line("  Mirror, Replace, Remove, Insert At, Insert Starting From, Save, Copy, Import) plus den Bibliotheks-Funktionen.");
            Line("• Farben: blau = ausgewählt, rot = noch nicht gespeichert (Auto-Speichern aus), rote Schrift = freie ID.");
            Line("• ★ = Favorit, ▣ = eigenes Vorschaubild, farbiger Streifen links = Farbe der Kategorie.");
            Line("• Zweite Zeile: Original-ID (männlich/weiblich bei Paperdoll-Gumps), Kategorien und #Tags.");

            Heading("Suchen und Filtern");
            Line("• Suche (Strg+F) durchsucht Name, Beschreibung, Tags, Kategorien, ID und Original-ID - schon beim Tippen.");
            Line("• Mehrere Wörter = alle müssen vorkommen.   \"rotes hemd\" = genau diese Wortfolge.   -leder = ohne \"leder\".");
            Line("• Gezielt: name:hemd  desc:alt  tag:rot  cat:kleidung  id:0xC350-0xC360  src:50000");
            Line("• Sonderfilter: is:fav  is:preview  is:nocat  is:notag  is:noname");
            Line("• Kategorie-Auswahl oben zeigt die Kategorie samt Unterkategorien; \"(Ohne Kategorie)\" zeigt nicht zugeordnete.");
            Line("• ★ Favoriten zeigt nur Favoriten. Der Radiergummi setzt alle Filter zurück.");
            Line("• Index (Strg+G) springt zu einer ID (dezimal oder 0x...), auch zu freien IDs.");

            Heading("Sortieren und eigene Reihenfolge");
            Line("• Sortierung: ID, Eigene Reihenfolge, Name, Original-ID, Kategorie, Hinzugefügt, Geändert; ▲/▼ dreht sie um.");
            Line("• Eigene Reihenfolge: Einträge mit der Maus an eine andere Stelle ziehen (nur in dieser Sortierung),");
            Line("  oder Alt+Hoch / Alt+Runter (stellt die Sortierung automatisch um),");
            Line("  oder Rechtsklick > Reihenfolge (nach oben/unten, an den Anfang/ans Ende).");
            Line("• Bei aktivem Filter werden nur die sichtbaren Einträge umsortiert - die übrigen behalten ihren Platz.");
            Line("• \"Aktuelle Ansicht als eigene Reihenfolge übernehmen\" speichert z. B. die Sortierung nach Name als Startpunkt.");
            Line("• Im Detailbereich kann die Position auch direkt als Zahl eingetragen werden.");

            Heading("Details bearbeiten (rechts unten, F2 / Doppelklick)");
            Line("• Name, Original-ID, Position, Favorit, Kategorien (Häkchen, beliebig viele), Tags (mit Komma getrennt");
            Line("  oder aus der Liste wählen), Beschreibung.");
            Line("• Übernehmen mit dem Button, Enter in einem Feld oder Strg+Enter. Beim Wechsel zu einem anderen Eintrag");
            Line("  werden Änderungen automatisch übernommen. Verwerfen bzw. Esc stellt den alten Stand her.");
            Line("• Für mehrere Einträge gleichzeitig: Rechtsklick > Kategorien zuweisen / Tags hinzufügen / Tag entfernen / Mark.");
            Line("• Button \"Details\" blendet den Bereich aus, wenn mehr Platz für das Bild gebraucht wird.");

            Heading("Kategorien und Tags verwalten");
            Line("• Bibliothek > Kategorien und Tags verwalten: neue Kategorien und Unterkategorien, umbenennen,");
            Line("  Beschreibung, Farbe, Reihenfolge (nach oben/unten), per Maus in eine andere Kategorie ziehen, löschen.");
            Line("• Löschen einer Kategorie entfernt nur die Zuordnung - die Gumps bleiben in der Bibliothek.");
            Line("• Tags: neu, umbenennen (ändert alle Gumps), löschen, unbenutzte entfernen.");
            Line("• Beispiel Kleidung: Kategorien \"Kleidung/Hemden\", \"Kleidung/Roben\", \"Männlich\", \"Weiblich\" -");
            Line("  ein Hemd-Gump kann gleichzeitig in \"Kleidung/Hemden\" und \"Männlich\" liegen.");

            Heading("Vorschaubilder");
            Line("• Jeder Gump kann ein eigenes Vorschaubild bekommen (z. B. ein Ausschnitt eines großen Gumps).");
            Line("• Aus Datei, aus der Zwischenablage, per Drag & Drop auf das kleine Vorschaubild oder automatisch aus");
            Line("  dem Gump (verkleinert, transparente Ränder abgeschnitten; Größe im Misc-Menü einstellbar).");
            Line("• Die Liste zeigt das Vorschaubild statt des Gumps (Misc > Eigene Vorschaubilder in der Liste).");
            Line("• Das Vorschaubild wird nie ins Original geschrieben.");

            Heading("Importieren, Exportieren, Zwischenablage");
            Line("• Import (Strg+V): Bilder aus der Zwischenablage - Schwarz, Weiß und #D3D3D3 werden transparent wie im");
            Line("  Gumps-Tab (Misc-Menü). Mehrere aus der Bibliothek kopierte Gumps kommen mit Name, Kategorien, Tags und");
            Line("  Vorschaubild an. Ist die ID belegt, wird gefragt.");
            Line("• Copy (Strg+C): kopiert die Auswahl; das erste Bild auch als normales Bild (schwarzer Hintergrund).");
            Line("• Bilder / Ordner importieren (Misc), Insert At.., Insert Starting From, Drag & Drop von Dateien");
            Line("  auf die Liste oder das große Bild. Dateinamen wie \"0xC350.png\" können ihre ID behalten.");
            Line("• Export Image / Export All als Bmp, Tiff, Jpg, Png (Name \"0xC350.png\" wie im Gumps-Tab).");
            Line("  Export All exportiert bei aktivem Filter nur die sichtbaren Gumps.");
            Line("• Misc: alle IDs als Text, alle Metadaten als CSV (Excel), Namen mit IDGumpNames.xml abgleichen.");

            Heading("Austausch mit den Original-Gumps");
            Line("• Im Gumps-Tab: Rechtsklick > \"Add to Gump Library\" übernimmt den Gump (gleiche ID, sonst Rückfrage).");
            Line("• Hier: Original > \"Aus dem Gumps-Tab übernehmen\" oder \"Aus Original übernehmen (IDs / Bereiche)\".");
            Line("  Die Original-ID wird gemerkt, der Name kommt aus IDGumpNames.xml (abschaltbar im Misc-Menü).");
            Line("• \"Ins Original schreiben\" / \"Auf die Original-ID zurückschreiben\" ersetzt Gumps im Speicher -");
            Line("  dauerhaft wird es erst mit \"Save\" im Gumps-Tab.");
            Line("• Jump to Male/Female: springt zum Partner-Gump (50000+ männlich, 60000+ weiblich) in der Bibliothek;");
            Line("  fehlt er, wird angeboten, ihn aus dem Original zu holen. \"Im Gumps-Tab anzeigen\" springt dorthin.");

            Heading("Bearbeiten");
            Line("• Mirror (horizontal/vertikal), 90° drehen und transparente Ränder abschneiden ändern den Gump in der");
            Line("  Bibliothek (alle ausgewählten). Replace ersetzt das Bild, Name/Kategorien/Tags bleiben.");
            Line("• Duplizieren, Kopieren/Verschieben nach ID und Tauschen nehmen Vorschaubild und Metadaten mit.");
            Line("• Remove (Entf) entfernt Gump, Vorschaubild und Metadaten.");

            Heading("Speichern");
            Line("• Auto-Speichern AN: jede Änderung wird sofort geschrieben (unten grün \"Gespeichert\").");
            Line("• Auto-Speichern AUS: erst mit Save (Diskette / Strg+S). Ungespeichert = orange und * im Titel,");
            Line("  rote Zeilen in der Liste, Rückfrage beim Schließen oder Wechseln der Bibliothek.");
            Line("• Bibliothek > Komprimieren gibt Platz von ersetzten Bildern frei; Erweitern vergrößert die Zahl der IDs.");

            Heading("Tastatur");
            Line("Strg+S speichern · Strg+F Suche · Strg+G Index · Strg+C / Strg+V · Entf entfernen · F2 Details ·");
            Line("Strg+Enter Details übernehmen · Alt+Hoch/Runter Reihenfolge · Strg+A alles auswählen · F1 Hilfe");

            richTextBoxHelp.SelectionStart = 0;
            richTextBoxHelp.ScrollToCaret();
        }
    }
}
