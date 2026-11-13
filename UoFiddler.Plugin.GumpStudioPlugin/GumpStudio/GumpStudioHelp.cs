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

SCRIPT GALLERY (second tool bar)
• Choose your Scripts folder → Scan: every gump class gets a picture. Search by class, command or file.
• Open in the studio (double click) – or select several and ""Add to templates"" (category).

WRITE BACK
• After an import (or from the gallery) change the gump, then ""Write back…"": only the changed numbers / texts of the Add… calls are
  replaced in the original script – logic, comments, formatting stay. Values from code are adjusted (x + 20 → x + 25), a loop moved as a
  whole changes its numbers once, deleted elements are commented out, new elements are added after the last Add… call of their page.
• You see every changed line (old / new) before writing; a .bak copy is made. What cannot be written is listed (e.g. texts from code).
• ""Compare ▾ → Imported original"" shows the original half transparent over your changes.

LISTS / TABLES (List / table…)
• Columns (Header|width|hue), rows (one per line, columns with ; or Tab – paste from Excel), rows per page, button per row.
• Row 1 is the template: move or change it – all rows and the page buttons follow. The script gets a for loop with string arrays,
  OnResponse gets the row (ButtonID − base). Select an element of the list and press ""List / table…"" again to change it.

THEMES (Themes…)
• Built-in looks (gray, dark stone, marble, parchment, golden, black glass) or your own (GumpThemes folder). Preview shows YOUR gump.
• Changes frames, button set (OK / cancel / next / back), check boxes / radios, text and HTML colors. ""From gump"" makes a theme of your gump.

OWN GRAPHICS (Own graphics…)
• Picture → gump: a PNG / BMP becomes a gump ID (next free ID is found), black = transparent.
• Frame generator: a picture of a box is cut into the 9 parts of AddBackground (margins with lines, preview in any size).
• Both are written into the gump file in memory – save it in UoFiddler (Gumps tab) to keep them. The new graphic is added to the gump.

COMPARE (Compare ▾)
• Current state / imported original / a picture (e.g. a screenshot of a gump to rebuild) half transparent over the work area,
  red tint, opacity 25 … 100 %. Pictures can also be dropped on the work area.

BUTTON ACTIONS (property ""Action"" of a reply button)
• Close, Reopen, OpenGump (class), Teleport (""x y z map""), GiveItem (e.g. Gold(100)), RunCommand, Message, CustomCode
  (+ ""Action reopen""). The script gets the code in its case of OnResponse.

TWO LANGUAGES
• TextDe of labels / HTML / text entries = text for players with client language DEU (gump setting ""SecondLanguage"").
  The script chooses with bool de = from.Language == ""DEU"". Tool bar ""Language"" shows and edits the second texts.

GROUPS AND GUIDES
• Arrange ▾ → Group (Ctrl+G): a click selects the whole group, Alt + click one element. Ungroup: Ctrl+Shift+G.
• Guides: while dragging, edges and centers snap to other elements (magenta lines), distances to the neighbours are shown. Alt = off.

TEST LOG
• Every click in the test (F5) is listed in the tab ""Test log""; ""Write into the script"" puts it as comment above OnResponse.

MODERNUO
• Script target ""ModernUO – DynamicGump"": BuildLayout(ref DynamicGumpBuilder builder) with builder.Add…, Configure() for the command,
  Singleton, SetNoClose …, OnResponse(NetState, in RelayInfo). ModernUO scripts (builder.Add…, tuples, primary constructors) can be imported.

CHECKS (below the script)
• Besides wrong IDs also: labels that run over their background, HTML that does not fit (no scrollbar), text entries without length
  limit, gumps bigger than 800 × 600, item art above 0x3FFF, features only newer clients show, button actions without target.

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
Ctrl+O open · Ctrl+S save · Ctrl+T template library · Ctrl+G / Ctrl+Shift+G group / ungroup · Ctrl+Z / Y undo / redo · Ctrl+C / V / D copy / paste / duplicate · Del delete · Ctrl+A all ·
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

SKRIPT-GALERIE (zweite Werkzeugleiste)
• Scripts-Ordner wählen → Scan: jede Gump-Klasse bekommt ein Bild. Suche nach Klasse, Befehl oder Datei.
• Im Studio öffnen (Doppelklick) – oder mehrere wählen und „Add to templates“ (Kategorie).

ZURÜCKSCHREIBEN (Write back…)
• Nach dem Einlesen (oder aus der Galerie) das Gump ändern, dann „Write back…“: nur die geänderten Zahlen / Texte der Add…-Aufrufe werden
  im Original-Skript ersetzt – Logik, Kommentare, Formatierung bleiben. Werte aus Code werden angepasst (x + 20 → x + 25), eine als Ganzes
  verschobene Schleife ändert ihre Zahlen einmal, gelöschte Elemente werden auskommentiert, neue nach dem letzten Add… ihrer Seite eingefügt.
• Vor dem Schreiben siehst du jede geänderte Zeile (alt / neu); eine .bak-Kopie wird angelegt. Was nicht geht, steht in der Liste (z. B. Texte aus Code).
• „Compare ▾ → Imported original“ zeigt das Original halb durchsichtig über deinen Änderungen.

LISTEN / TABELLEN (List / table…)
• Spalten (Überschrift|Breite|Farbe), Zeilen (eine pro Zeile, Spalten mit ; oder Tab – Einfügen aus Excel geht), Zeilen pro Seite, Button pro Zeile.
• Zeile 1 ist die Vorlage: verschieben oder ändern – alle Zeilen und die Blätter-Buttons folgen. Das Skript bekommt eine for-Schleife mit
  String-Arrays, OnResponse die Zeile (ButtonID − Basis). Ein Element der Liste wählen und erneut „List / table…“ = Liste ändern.

THEMEN (Themes…)
• Mitgelieferte Looks (grau, dunkler Stein, Marmor, Pergament, golden, schwarzes Glas) oder eigene (Ordner GumpThemes). Die Vorschau zeigt DEIN Gump.
• Ändert Rahmen, Button-Satz (OK / Abbrechen / weiter / zurück), Checkboxen / Radios, Text- und HTML-Farben. „From gump“ macht ein Thema aus deinem Gump.

EIGENE GRAFIKEN (Own graphics…)
• Bild → Gump: ein PNG / BMP wird eine Gump-ID (nächste freie ID wird gesucht), Schwarz = durchsichtig.
• Rahmen-Generator: ein Bild eines Kastens wird in die 9 Teile von AddBackground geschnitten (Ränder mit Linien, Vorschau in jeder Größe).
• Beides wird in die Gump-Datei im Speicher geschrieben – in UoFiddler (Reiter Gumps) speichern, um es zu behalten. Die Grafik kommt ins Gump.

VERGLEICHEN (Compare ▾)
• Aktueller Stand / eingelesenes Original / ein Bild (z. B. Screenshot eines Gumps zum Nachbauen) halb durchsichtig über der Arbeitsfläche,
  rot eingefärbt, Deckkraft 25 … 100 %. Bilder kann man auch auf die Arbeitsfläche ziehen.

BUTTON-AKTIONEN (Eigenschaft „Action“ eines Reply-Buttons)
• Close, Reopen, OpenGump (Klasse), Teleport („x y z map“), GiveItem (z. B. Gold(100)), RunCommand, Message, CustomCode
  (+ „Action reopen“). Das Skript bekommt den Code im case von OnResponse.

ZWEI SPRACHEN
• TextDe von Labels / HTML / Eingabefeldern = Text für Spieler mit Client-Sprache DEU (Gump-Einstellung „SecondLanguage“).
  Das Skript wählt mit bool de = from.Language == ""DEU"". Werkzeugleiste „Language“ zeigt und bearbeitet die zweiten Texte.

GRUPPEN UND HILFSLINIEN
• Arrange ▾ → Group (Strg+G): ein Klick wählt die ganze Gruppe, Alt + Klick ein Element. Gruppe lösen: Strg+Umschalt+G.
• Guides: beim Ziehen rasten Kanten und Mitten an anderen Elementen ein (magenta Linien), Abstände zu den Nachbarn werden gezeigt. Alt = aus.

TEST-PROTOKOLL
• Jeder Klick im Test (F5) steht im Reiter „Test log“; „Write into the script“ setzt ihn als Kommentar über OnResponse.

MODERNUO
• Skript-Ziel „ModernUO – DynamicGump“: BuildLayout(ref DynamicGumpBuilder builder) mit builder.Add…, Configure() für den Befehl,
  Singleton, SetNoClose …, OnResponse(NetState, in RelayInfo). ModernUO-Skripte (builder.Add…, Tupel, Primärkonstruktoren) lassen sich einlesen.

PRÜFUNGEN (unter dem Skript)
• Neben falschen IDs auch: Labels, die über ihren Hintergrund laufen, HTML, das nicht passt (ohne Scrollbar), Eingabefelder ohne Längengrenze,
  Gumps größer als 800 × 600, Item-Grafiken über 0x3FFF, Dinge, die nur neuere Clients zeigen, Button-Aktionen ohne Ziel.

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
Strg+O öffnen · Strg+S speichern · Strg+T Vorlagen-Bibliothek · Strg+G / Strg+Umschalt+G Gruppe / lösen · Strg+Z / Y rückgängig / wiederholen · Strg+C / V / D kopieren / einfügen / duplizieren · Entf löschen ·
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
