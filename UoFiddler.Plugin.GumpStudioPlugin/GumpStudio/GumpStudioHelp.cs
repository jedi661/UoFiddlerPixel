using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>Help window of the gump studio (English / German).</summary>
    internal static class GumpStudioHelp
    {
        public const string English =
@"GUMP STUDIO – HELP

PURPOSE
Design ServUO / RunUO gumps (the dialog windows of the game) with the real graphics of your client and get the finished
script. Above: the gump as the client shows it. Below: the script, written live. Right: properties, element list, gump browser.

START
• New ▾: a template (empty window, yes / no question, text input form, teleporter menu with pages, choice list, info text).
• Open / Save: the project as *.gump.json (everything, can be edited again later).
• Import ServUO…: read an existing gump script (.cs) – its Add… lines become elements. Lines with variables are listed below the script.

ADDING ELEMENTS
• Toolbox left: background (9 part frame), image, image tiled, alpha region, label, label cropped, HTML, HTML cliloc, button,
  item button, check box, radio, text entry, item, sprite – plus ready parts (OK / Cancel / next page / back / close / input with frame /
  title bar / list row).
• Gumps / items tab right: pictures of the client; drag one onto the gump, or double click = use it for the selected element.
• ""New elements go to"": page 0 (shown on every page, e.g. the frame) or the current page.

EDITING
• Click = select, Shift / Ctrl + click = several, drag on empty space = rubber band. Drag = move (snaps to the grid; Alt = free).
• Handles on frames, tiled images, HTML, text entries … change the size. Arrow keys move by 1 (Shift = grid), Ctrl + arrows = size.
• Double click on a label / text entry / HTML edits the text in place.
• Properties tab: everything of the selected element(s); Gump ID, item ID and hue fields have a … button with pictures.
  ""Gump / script"" shows the settings of the gump: class name, namespace, command, position, closable, dragable …
• Arrange ▾: align, same size, distribute, order, center in the background. Elements tab: list in draw order, up / down,
  hide (designer only), lock, ""Renumber IDs"".
• Copy / Paste / Duplicate, Undo / Redo (100 steps). Copy also puts the ServUO lines into the clipboard; ServUO lines from the
  clipboard can be pasted as elements.

PAGES
Page 0 = always visible. ""+ Page"" starts a new page; Page buttons (type Page, target page) switch between pages.
◀ ▶ or PageUp / PageDown show the pages.

TEST (F5)
The gump works like in the client: buttons press, page buttons switch, check boxes / radios (per group) switch, text entries
take text. The status line shows what OnResponse would get (ButtonID, switches, texts). Right click = close (ButtonID 0).

SCRIPT
• ServUO / RunUO: complete class – constants for named buttons / switches / entries, optional command ([MyGump) with access level,
  Closable / Dragable …, the Add… lines in page order, OnResponse with text entries, switches and a case per button.
• Only the Add… lines – to paste into an existing gump.
• Sphere [DIALOG] with TEXT and BUTTON sections.
• Below the script: problems (same IDs twice, page buttons to empty pages, missing graphics, umlauts in labels …) – double click selects.
• Copy script / Save .cs (e.g. into ServUO\Scripts\Custom). PNG saves a picture of the page.

KEYS
Ctrl+O open · Ctrl+S save · Ctrl+Z / Y undo / redo · Ctrl+C / V / D copy / paste / duplicate · Del delete · Ctrl+A all ·
Esc nothing selected · arrows move · PageUp / PageDown pages · Ctrl + mouse wheel zoom · F5 test · F1 help";

        public const string German =
@"GUMP STUDIO – HILFE

ZWECK
ServUO- / RunUO-Gumps (die Dialogfenster im Spiel) mit den echten Grafiken deines Clients entwerfen und das fertige Skript bekommen.
Oben: das Gump, wie der Client es zeigt. Unten: das Skript, live geschrieben. Rechts: Eigenschaften, Elementliste, Gump-Browser.

START
• New ▾: eine Vorlage (leeres Fenster, Ja / Nein-Frage, Eingabeformular, Teleporter-Menü mit Seiten, Auswahlliste, Infotext).
• Open / Save: das Projekt als *.gump.json (alles, später weiter bearbeitbar).
• Import ServUO…: ein vorhandenes Gump-Skript (.cs) einlesen – seine Add…-Zeilen werden Elemente. Zeilen mit Variablen stehen unter dem Skript.

ELEMENTE HINZUFÜGEN
• Werkzeugkasten links: Hintergrund (9-teiliger Rahmen), Bild, gekacheltes Bild, Alpha-Bereich, Label, abgeschnittenes Label, HTML,
  HTML-Cliloc, Button, Item-Button, Checkbox, Radio, Eingabefeld, Item, Sprite – dazu fertige Teile (OK / Abbrechen / nächste Seite /
  zurück / schließen / Eingabe mit Rahmen / Titelleiste / Listenzeile).
• Reiter Gumps / items rechts: Bilder des Clients; auf das Gump ziehen, oder Doppelklick = für das gewählte Element übernehmen.
• „New elements go to“: Seite 0 (auf jeder Seite, z. B. der Rahmen) oder die aktuelle Seite.

BEARBEITEN
• Klick = wählen, Umschalt / Strg + Klick = mehrere, auf freier Fläche ziehen = Rahmen. Ziehen = verschieben (rastet am Raster ein; Alt = frei).
• Anfasser an Rahmen, gekachelten Bildern, HTML, Eingabefeldern … ändern die Größe. Pfeiltasten verschieben um 1 (Umschalt = Raster),
  Strg + Pfeile = Größe.
• Doppelklick auf Label / Eingabefeld / HTML bearbeitet den Text direkt.
• Reiter Properties: alles zum gewählten Element; Gump-ID-, Item-ID- und Hue-Felder haben einen …-Knopf mit Bildern.
  „Gump / script“ zeigt die Einstellungen des Gumps: Klassenname, Namespace, Befehl, Position, schließbar, verschiebbar …
• Arrange ▾: ausrichten, gleiche Größe, verteilen, Reihenfolge, im Hintergrund zentrieren. Reiter Elements: Liste in Zeichenreihenfolge,
  hoch / runter, ausblenden (nur im Designer), sperren, „Renumber IDs“.
• Kopieren / Einfügen / Duplizieren, Rückgängig / Wiederholen (100 Schritte). Kopieren legt auch die ServUO-Zeilen in die Zwischenablage;
  ServUO-Zeilen aus der Zwischenablage lassen sich als Elemente einfügen.

SEITEN
Seite 0 = immer sichtbar. „+ Page“ beginnt eine neue Seite; Seiten-Buttons (Typ Page, Zielseite) wechseln zwischen den Seiten.
◀ ▶ oder Bild↑ / Bild↓ zeigen die Seiten.

TEST (F5)
Das Gump funktioniert wie im Client: Buttons drücken sich, Seiten-Buttons blättern, Checkboxen / Radios (pro Gruppe) schalten,
Eingabefelder nehmen Text. Die Statuszeile zeigt, was OnResponse bekäme (ButtonID, Schalter, Texte). Rechtsklick = schließen (ButtonID 0).

SKRIPT
• ServUO / RunUO: komplette Klasse – Konstanten für benannte Buttons / Schalter / Eingaben, optionaler Befehl ([MeinGump) mit Zugriffsstufe,
  Closable / Dragable …, die Add…-Zeilen in Seitenreihenfolge, OnResponse mit Eingabefeldern, Schaltern und einem case pro Button.
• Nur die Add…-Zeilen – zum Einfügen in ein vorhandenes Gump.
• Sphere [DIALOG] mit TEXT- und BUTTON-Abschnitt.
• Unter dem Skript: Probleme (gleiche IDs doppelt, Seiten-Buttons auf leere Seiten, fehlende Grafiken, Umlaute in Labels …) – Doppelklick wählt aus.
• Skript kopieren / als .cs speichern (z. B. in ServUO\Scripts\Custom). PNG speichert ein Bild der Seite.

TASTEN
Strg+O öffnen · Strg+S speichern · Strg+Z / Y rückgängig / wiederholen · Strg+C / V / D kopieren / einfügen / duplizieren · Entf löschen ·
Strg+A alles · Esc nichts gewählt · Pfeile verschieben · Bild↑ / Bild↓ Seiten · Strg + Mausrad Zoom · F5 Test · F1 Hilfe";

        public static void Show(IWin32Window owner, bool german)
        {
            var form = new Form
            {
                Text = "Help / Hilfe – Gump Studio",
                Size = new Size(960, 780),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                ShowIcon = false
            };
            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(Page("English", English));
            tabs.TabPages.Add(Page("Deutsch", German));
            tabs.SelectedIndex = german ? 1 : 0;
            form.Controls.Add(tabs);
            form.Show(owner);
        }

        private static TabPage Page(string title, string text)
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
