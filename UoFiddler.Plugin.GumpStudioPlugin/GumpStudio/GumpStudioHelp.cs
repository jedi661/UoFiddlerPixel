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
• New ▾: a built-in template (empty window, yes / no question, text input form, teleporter menu with pages, choice list, info text)
  or one of your own templates (sub menu per category).
• Open / Save: the project as *.gump.json (everything, can be edited again later).
• Import ServUO…: read an existing gump script (.cs) – see IMPORT.

TEMPLATES (Templates ▾, Ctrl+T)
• ""Save gump as template…"": name, category and a note – saved as GumpTemplates\<Category>\<Name>.gump.json next to UoFiddler.
  ""/"" in the category makes sub categories (e.g. Shops/Vendors).
• ""Save selection as building block…"": only the selected elements (e.g. title bar, input field with frame, list row).
• Template library: categories left, pictures in the middle, big picture and note right.
  - ""New gump from it"" (double click) starts a new gump, ""Insert into current gump"" adds the elements to the open gump
    (same position, button / switch / entry IDs are moved behind the used ones, radio groups stay apart).
  - + Category / – Category (only empty ones), click a selected category again or F2 = rename it.
  - Rename / move (F2), Copy (also of built-in templates, to change them), Delete (Del), Export, search over all categories.
  - Drag a template onto a category = move it. ""Add files…"" or drag files from Explorer onto the list:
    Gump Studio projects or ServUO scripts (.cs – every gump class of the file becomes a template).
  - The folder can be copied to other PCs or shared with your team.

IMPORT (ServUO / RunUO / ModernUO scripts)
• The import window shows the gump before it is taken: gump classes of the file (left), picture per page, hints.
• The constructor is really run: variables (int y = 50; y += 25; line++), arrays, for / foreach / while loops, if / else, switch,
  helper methods of the class (also with return value, e.g. Color() / Center()), String.Format, $""…"" strings, constants, enums,
  constructors that call each other (: this(…)).
• Values of other classes (e.g. PropsConfig.EntryHeight) are read from the scripts around the file (its folder up to the
  Scripts folder) – switch it off with the check box.
• What only exists in the game becomes a placeholder: texts {from.Name}, numbers 0. Lists of the game (list.Count) get 3 sample
  rows, sizes from parameters (width / height) 400 × 300. if (…) that only the game knows: the ""if"" part is shown;
  guard clauses like if (state == null) return; are skipped. Everything is listed as hint below the script.

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
Ctrl+O open · Ctrl+S save · Ctrl+T template library · Ctrl+Z / Y undo / redo · Ctrl+C / V / D copy / paste / duplicate · Del delete · Ctrl+A all ·
Esc nothing selected · arrows move · PageUp / PageDown pages · Ctrl + mouse wheel zoom · F5 test · F1 help";

        public const string German =
@"GUMP STUDIO – HILFE

ZWECK
ServUO- / RunUO-Gumps (die Dialogfenster im Spiel) mit den echten Grafiken deines Clients entwerfen und das fertige Skript bekommen.
Oben: das Gump, wie der Client es zeigt. Unten: das Skript, live geschrieben. Rechts: Eigenschaften, Elementliste, Gump-Browser.

START
• New ▾: eine mitgelieferte Vorlage (leeres Fenster, Ja / Nein-Frage, Eingabeformular, Teleporter-Menü mit Seiten, Auswahlliste, Infotext)
  oder eine deiner eigenen Vorlagen (Untermenü pro Kategorie).
• Open / Save: das Projekt als *.gump.json (alles, später weiter bearbeitbar).
• Import ServUO…: ein vorhandenes Gump-Skript (.cs) einlesen – siehe EINLESEN.

VORLAGEN (Templates ▾, Strg+T)
• „Save gump as template…“: Name, Kategorie und Notiz – gespeichert als GumpTemplates\<Kategorie>\<Name>.gump.json neben UoFiddler.
  „/“ in der Kategorie macht Unterkategorien (z. B. Shops/Händler).
• „Save selection as building block…“: nur die gewählten Elemente (z. B. Titelleiste, Eingabefeld mit Rahmen, Listenzeile).
• Vorlagen-Bibliothek: links Kategorien, in der Mitte Bilder, rechts großes Bild und Notiz.
  - „New gump from it“ (Doppelklick) beginnt ein neues Gump, „Insert into current gump“ fügt die Elemente in das offene Gump ein
    (gleiche Position, Button- / Schalter- / Eingabe-IDs werden hinter die benutzten gesetzt, Radio-Gruppen bleiben getrennt).
  - + Category / – Category (nur leere), gewählte Kategorie nochmals anklicken oder F2 = umbenennen.
  - Umbenennen / verschieben (F2), Kopieren (auch mitgelieferte Vorlagen, um sie zu ändern), Löschen (Entf), Export, Suche über alle Kategorien.
  - Vorlage auf eine Kategorie ziehen = verschieben. „Add files…“ oder Dateien aus dem Explorer auf die Liste ziehen:
    Gump-Studio-Projekte oder ServUO-Skripte (.cs – jede Gump-Klasse der Datei wird eine Vorlage).
  - Den Ordner kannst du auf andere PCs kopieren oder mit deinem Team teilen.

EINLESEN (ServUO- / RunUO- / ModernUO-Skripte)
• Das Import-Fenster zeigt das Gump, bevor es übernommen wird: Gump-Klassen der Datei (links), Bild pro Seite, Hinweise.
• Der Konstruktor wird wirklich ausgeführt: Variablen (int y = 50; y += 25; line++), Arrays, for- / foreach- / while-Schleifen, if / else,
  switch, Hilfsmethoden der Klasse (auch mit Rückgabewert, z. B. Color() / Center()), String.Format, $""…""-Texte, Konstanten, Enums,
  Konstruktoren, die sich gegenseitig aufrufen (: this(…)).
• Werte anderer Klassen (z. B. PropsConfig.EntryHeight) werden aus den Skripten rund um die Datei gelesen (ihr Ordner bis hoch zum
  Scripts-Ordner) – mit dem Häkchen abschaltbar.
• Was es nur im Spiel gibt, wird ein Platzhalter: Texte {from.Name}, Zahlen 0. Listen des Spiels (list.Count) bekommen 3 Beispielzeilen,
  Größen aus Parametern (width / height) 400 × 300. if (…), das nur das Spiel kennt: der „if“-Teil wird gezeigt; Schutzabfragen wie
  if (state == null) return; werden übersprungen. Alles steht als Hinweis unter dem Skript.

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
Strg+O öffnen · Strg+S speichern · Strg+T Vorlagen-Bibliothek · Strg+Z / Y rückgängig / wiederholen · Strg+C / V / D kopieren / einfügen / duplizieren · Entf löschen ·
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
