/***************************************************************************
 *
 * MultiEditor – help (Deutsch / English)
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Forms
{
    public sealed class HelpForm : Form
    {
        private static HelpForm _instance;

        private readonly TreeView _topics;
        private readonly RichTextBox _text;
        private bool _german;

        private static readonly (string de, string en, string textDe, string textEn)[] _pages =
        {
            ("Überblick", "Overview",
@"MULTI EDITOR – ÜBERBLICK

Mit dem Multi Editor baust und bearbeitest du Multis (Häuser, Schiffe, Brücken, Deko-Gruppen) für Ultima Online.

Aufbau des Fensters:
• Links oben: Werkzeug-Knöpfe (Auswählen, Zeichnen, Entfernen, Höhe, Boden, Pipette, Transparent).
• Links Reiter: Tiles (Kachel-Paletten), Design (Größe), Import (Multi.mul und Dateien), Save (Export), Extra (neue Funktionen).
• Mitte: Bauansicht. Oben der Höhenregler (max. sichtbare Höhe).
• Oben: Menü und Werkzeugleiste (Rechteck, Linie, Rahmen-Auswahl, Bereich löschen, Zoom, Hue, Min-Z, Raster, Höhen, Hintergrund).
• Unten: Statuszeile mit Zeichen-ID, ausgewählter Kachel und Koordinaten.

Schnellstart:
1. Reiter Design → Größe einstellen → „Create Blank“ (oder Datei öffnen / Multi aus Import doppelklicken).
2. Reiter Tiles → Gruppe wählen → Kachel anklicken.
3. Werkzeug Zeichnen (Taste 2) → in die Bauansicht klicken.
4. Speichern: Reiter Save „Save to ID“, „In Multi.mul schreiben“ oder als ServUO-Addon exportieren.",
@"MULTI EDITOR – OVERVIEW

The Multi Editor builds and edits multis (houses, boats, bridges, deco groups) for Ultima Online.

Window layout:
• Top left: tool buttons (select, draw, remove, altitude, virtual floor, pipette, transparent).
• Left tabs: Tiles (tile palettes), Design (size), Import (multi.mul and files), Save (export), Extra (new functions).
• Center: building view. Above it the height slider (max. visible height).
• Top: menu and toolbar (rectangle, line, box select, erase area, zoom, hue, min Z, grid, heights, background).
• Bottom: status bar with draw id, selected tile and coordinates.

Quick start:
1. Design tab → set size → ""Create Blank"" (or open a file / double click a multi in Import).
2. Tiles tab → choose group → click a tile.
3. Draw tool (key 2) → click into the building view.
4. Save: Save tab ""Save to ID"", ""Write to Multi.mul"" or export as ServUO addon."),

            ("Bauen", "Building",
@"BAUEN

Werkzeuge (Tasten in Klammern):
• Auswählen (1): Kachel anklicken. Strg+Klick fügt zur Mehrfachauswahl hinzu / entfernt.
• Zeichnen (2): setzt die Zeichen-Kachel. Über einer Kachel wird darauf gestapelt (Z + Höhe).
• Entfernen (3): entfernt die Kachel unter der Maus.
• Höhe (4): ändert Z der Kachel um den Wert neben dem Knopf.
• Pipette (5): übernimmt die Kachel unter der Maus als Zeichen-Kachel.
• Transparent (6): macht eine Kachel durchsichtig (nur Ansicht).
• Rechteck (7): Fläche aufziehen und füllen. Mit Umschalt nur der Rand (z.B. Mauern).
• Linie (8): gerade Linie entlang X oder Y aufziehen (Mauern, Zäune).
• Rahmen-Auswahl (9): Bereich aufziehen = alle Kacheln darin auswählen (Strg = hinzufügen).
• Bereich löschen (0): Bereich aufziehen = alle sichtbaren Kacheln darin löschen.

Virtueller Boden: Knopf „Boden“ + Zahl = Bau-Ebene. Kacheln unter der Ebene sind nicht wählbar.
Höhenregler oben: blendet alles über dieser Höhe aus. „Min Z“ in der Werkzeugleiste blendet alles darunter aus.
Hue: in der Werkzeugleiste. Wird beim Zeichnen gesetzt (nur Vorschau und ServUO-Addon; Multi.mul speichert keine Farben).

Auswahl bearbeiten:
• Pfeiltasten verschieben die Auswahl (X/Y), Bild↑/Bild↓ ändern Z (+Umschalt = 5, +Strg = 20).
• Entf löscht die Auswahl. Strg+A wählt alles.
• Strg+C / Strg+X / Strg+V: Kopieren, Ausschneiden, Einfügen (Einfügen folgt der Maus, Klick setzt, Esc bricht ab).

Weitere Funktionen im Menü Bearbeiten: Drehen 90°, Spiegeln, Zuschneiden, Ursprung setzen, IDs ersetzen.",
@"BUILDING

Tools (keys in brackets):
• Select (1): click a tile. Ctrl+click adds to / removes from the multi selection.
• Draw (2): places the draw tile. Over a tile it is stacked on top (Z + height).
• Remove (3): removes the tile below the mouse.
• Altitude (4): changes Z of the tile by the value next to the button.
• Pipette (5): takes the tile below the mouse as draw tile.
• Transparent (6): makes a tile see-through (view only).
• Rectangle (7): drag an area and fill it. With Shift only the border (e.g. walls).
• Line (8): drag a straight line along X or Y (walls, fences).
• Box select (9): drag an area = select all tiles inside (Ctrl = add).
• Erase area (0): drag an area = delete all visible tiles inside.

Virtual floor: button ""floor"" + number = building level. Tiles below this level can't be picked.
Height slider: hides everything above this height. ""Min Z"" in the toolbar hides everything below.
Hue: in the toolbar. It is used for drawing (preview and ServUO addon only; multi.mul has no hues).

Editing the selection:
• Arrow keys move the selection (X/Y), Page Up/Down change Z (+Shift = 5, +Ctrl = 20).
• Del deletes the selection. Ctrl+A selects everything.
• Ctrl+C / Ctrl+X / Ctrl+V: copy, cut, paste (paste follows the mouse, click places it, Esc cancels).

More in the Edit menu: rotate 90°, mirror, crop, set origin, replace ids."),

            ("Kontextmenü & Tasten", "Context menu & keys",
@"KONTEXTMENÜ UND TASTEN

Rechtsklick (ohne Ziehen) in der Bauansicht öffnet das Kontextmenü:
• Werkzeug wechseln, Kachel als Zeichen-Kachel übernehmen
• Kachel / alle Kacheln der Zelle entfernen
• Z ändern (+1, +5, +20, -1, -5, -20, Wert setzen), Hue setzen, Unsichtbar-Flag
• Alle mit gleicher ID auswählen, ersetzen durch Zeichen-Kachel
• Kopieren / Ausschneiden / Einfügen / Auswahl löschen
• Ursprung (Mitte) des Multis auf diese Zelle setzen
• Hintergrund-Static übernehmen (wenn eine Map geladen ist)
• TileData-Info zur Kachel

Rechts ziehen = Ansicht verschieben. Mittlere Maustaste = alle Kacheln einer Zelle nebeneinander zeigen.

Tasten:
Strg+Z Rückgängig · Strg+Y Wiederholen · Strg+S Projekt speichern · Strg+O Datei öffnen
Strg+C/X/V Kopieren/Ausschneiden/Einfügen · Strg+A alles · Entf löschen · Esc abbrechen
1-9, 0 Werkzeuge · B Struktur · M Map-Auswahl · E Einrichtung an/aus · R Stempel drehen · L Lineal
G Raster · H Höhen · F1 Hilfe
Strg+Mausrad / Strg+Plus / Strg+Minus Zoom · Strg+0 Zoom 100 %
Umschalt+Mausrad = waagrecht scrollen",
@"CONTEXT MENU AND KEYS

Right click (without dragging) in the building view opens the context menu:
• change tool, take tile as draw tile
• remove tile / all tiles of the cell
• change Z (+1, +5, +20, -1, -5, -20, set value), set hue, invisible flag
• select all with the same id, replace by draw tile
• copy / cut / paste / delete selection
• set the origin (center) of the multi to this cell
• take background static (when a map is loaded)
• tiledata info of the tile

Right drag = pan the view. Middle mouse button = show all tiles of a cell side by side.

Keys:
Ctrl+Z undo · Ctrl+Y redo · Ctrl+S save project · Ctrl+O open file
Ctrl+C/X/V copy/cut/paste · Ctrl+A all · Del delete · Esc cancel
1-9, 0 tools · B structure · M map select · E furniture on/off · R rotate stamp · L ruler
G grid · H heights · F1 help
Ctrl+wheel / Ctrl+Plus / Ctrl+Minus zoom · Ctrl+0 zoom 100 %
Shift+wheel = scroll horizontally"),

            ("Rückgängig / Wiederholen", "Undo / Redo",
@"RÜCKGÄNGIG / WIEDERHOLEN

Jede Änderung (Zeichnen, Löschen, Verschieben, Größe, Drehen, Generator …) wird gespeichert – bis zu 200 Schritte.
• Strg+Z = Rückgängig, Strg+Y (oder Strg+Umschalt+Z) = Wiederholen.
• Menü Misc → Undo zeigt die Liste der letzten Aktionen. Ein Klick nimmt alle Schritte bis dort zurück.
• Nach einem neuen Laden / „Create Blank“ wird die Liste geleert.
Achtung: Was bereits in Statics (Map) gespeichert wurde, kann nicht rückgängig gemacht werden.",
@"UNDO / REDO

Every change (draw, delete, move, size, rotate, generator …) is stored – up to 200 steps.
• Ctrl+Z = undo, Ctrl+Y (or Ctrl+Shift+Z) = redo.
• Menu Misc → Undo shows the list of the last actions. A click undoes all steps up to there.
• Loading a multi / ""Create Blank"" clears the list.
Note: what was already saved into statics (map) can't be undone."),

            ("Import", "Import",
@"IMPORT

Menü Datei → Öffnen (Strg+O) erkennt das Format automatisch:
• Text-Listen „ID X Y Z [Flag] [Hue]“ – IDs als 0x.. (hex) oder dezimal. Ohne 0x wird automatisch erkannt
  (Buchstaben A-F = hex, sonst wird geprüft welche Deutung gültige Grafiken ergibt). Über „Text dezimal / hex“ kannst du es erzwingen.
  Beispiel hex: 0xCD3 -27 -6 7 0   Beispiel dezimal: 1711 -17 4 0 0
• UO Architect: .uoa binär (Farben werden übernommen) und UOA-Text.
• WSC / Freeze (SECTION WORLDITEM), UOX3, CSV, XML.
• ServUO / RunUO Addon-Skripte (.cs): Arrays {id, x, y, z}, AddComponent(new AddonComponent(id), x, y, z),
  Variablen (ac = new AddonComponent(..); ac.Hue = ..; AddComponent(ac, ..)) und AddComplexComponent(.. hue ..).
• MultiEditor-Projekt (.mep) – speichert alles: Größe, Ursprung, Flags, Hues.

Reiter Import: Multi.mul-Einträge doppelklicken. Unter „From File“ gibt es die einzelnen Formate,
unter „Zuletzt geöffnet“ die letzten Dateien.",
@"IMPORT

Menu File → Open (Ctrl+O) detects the format:
• Text lists ""ID X Y Z [flag] [hue]"" – ids as 0x.. (hex) or decimal. Without 0x it is detected automatically
  (letters A-F = hex, otherwise the variant with valid art wins). Use ""Text decimal / hex"" to force it.
  Example hex: 0xCD3 -27 -6 7 0   Example decimal: 1711 -17 4 0 0
• UO Architect: .uoa binary (hues are kept) and UOA text.
• WSC / Freeze (SECTION WORLDITEM), UOX3, CSV, XML.
• ServUO / RunUO addon scripts (.cs): arrays {id, x, y, z}, AddComponent(new AddonComponent(id), x, y, z),
  variables (ac = new AddonComponent(..); ac.Hue = ..; AddComponent(ac, ..)) and AddComplexComponent(.. hue ..).
• MultiEditor project (.mep) – stores everything: size, origin, flags, hues.

Import tab: double click multi.mul entries. ""From File"" lists the single formats,
""Recent files"" the last files."),

            ("Speichern & Export", "Save & export",
@"SPEICHERN UND EXPORT

• Save to ID: speichert das Multi im Speicher unter der ID (wie bisher).
• In Multi.mul schreiben: speichert unter der ID und schreibt multi.mul + multi.idx
  – in den Ausgabeordner von UO Fiddler, oder auf Wunsch direkt in den Client-Ordner (mit Sicherungskopie *.bak_Datum).
  Nutzt dein Client MultiCollection.uop, musst du danach mit dem UOP-Packer packen.
• Nächste freie ID: sucht die erste leere Multi-ID ab der eingegebenen.
• ServUO Addon (.cs): erzeugt Addon + AddonDeed. Stil „Array“ (kompakt) oder „Zeilen“. Kacheln mit Hue werden
  über AddComplexComponent geschrieben. Die Datei nach Scripts/Custom kopieren und den Server neu starten.
  Im Spiel: [add <Name>AddonDeed
• Text hex/dezimal, WSC mit Farben, XML mit Farben, Projekt (.mep) und die bisherigen Formate (.txt, .uoa.txt, .wsc, .csv, .uox3, .xml).

Ursprung (Mitte): Alle Exporte rechnen relativ zum Ursprung (rote Raute im Raster).
Mit Rechtsklick → „Ursprung hier setzen“ änderst du ihn.",
@"SAVE AND EXPORT

• Save to ID: stores the multi in memory under the id (as before).
• Write to Multi.mul: stores under the id and writes multi.mul + multi.idx
  – into the UO Fiddler output folder, or directly into the client folder (with backup *.bak_date).
  If your client uses MultiCollection.uop, pack it with the UOP packer afterwards.
• Next free id: finds the first empty multi id starting at the entered one.
• ServUO addon (.cs): creates Addon + AddonDeed. Style ""array"" (compact) or ""lines"". Tiles with hue are written
  via AddComplexComponent. Copy the file to Scripts/Custom and restart the server.
  In game: [add <Name>AddonDeed
• Text hex/decimal, WSC with hues, XML with hues, project (.mep) and the old formats (.txt, .uoa.txt, .wsc, .csv, .uox3, .xml).

Origin (center): all exports are relative to the origin (red diamond in the grid).
Right click → ""Set origin here"" changes it."),

            ("Hintergrund", "Background",
@"HINTERGRUND

Menü Ansicht → Hintergrund (oder Knopf in der Werkzeugleiste):
• Muster: Gras, Wasser, Sand, Erde, Schnee, Wald oder eigene Land-IDs.
• Bild: beliebiges PNG/JPG/BMP, verschiebbar, skalierbar, mit Transparenz (z.B. Grundriss oder Screenshot zum Nachbauen).
• Map-Bereich: siehe „Map“.
• Rand: wie viele Felder um das Multi herum gezeichnet werden.
Raster (G) zeigt jedes Feld und die Multi-Grenze, der rote Rahmen ist der Ursprung.",
@"BACKGROUND

Menu View → Background (or the toolbar button):
• Pattern: grass, water, sand, dirt, snow, forest or your own land ids.
• Image: any PNG/JPG/BMP, movable, scalable, with transparency (e.g. a floor plan or screenshot to rebuild).
• Map area: see ""Map"".
• Margin: how many cells around the multi are drawn.
Grid (G) shows every cell and the multi border, the red diamond is the origin."),

            ("Map", "Map",
@"MAP ALS BAUGRUND

Menü Map → „Map-Bereich laden…“:
1. Map wählen, Bereich mit linker Maus aufziehen (rechts ziehen = verschieben, Strg+Rad = Zoom).
2. Basis-Z: welche Welt-Höhe Z 0 im Multi ist (automatisch = Median des Bodens).
3. Optional: neues leeres Multi in Bereichsgröße und/oder vorhandene Statics als bearbeitbare Kacheln übernehmen.
4. „Static suchen“: ID eingeben, Fundstellen werden markiert.

In der Bauansicht:
• Boden und Statics der Map werden unter dem Multi gezeigt (Statics können halbtransparent sein).
• Höhen (H) zeigt die Boden-Höhe jeder Zelle (weiß = Basis, orange = höher, blau = tiefer).
• „Auf Boden setzen“: beim Zeichnen ohne Kachel unter der Maus wird die Boden-Höhe verwendet.
• Rechtsklick → „Hintergrund-Static als Zeichen-Kachel“ übernimmt das oberste Static der Zelle.
• Menü Map → „Static im Bereich finden“ markiert alle Statics mit einer ID.

In Statics schreiben (Menü Map → „Multi in Statics schreiben…“):
• Jede Kachel wird als Static an Welt-Position (Map-X + Zelle X, Map-Y + Zelle Y, Basis-Z + Z) gesetzt.
• „Sofort speichern“ schreibt statics*.mul / staidx*.mul in den Ausgabeordner. Sonst bleiben sie offen (sofort im Hintergrund sichtbar) und werden mit Menü Map → „Map-Statics speichern…“ geschrieben.
• Zusätzlich kann eine Freeze-Datei (WSC) erzeugt werden.
Vor dem Speichern mit „Map-Änderung rückgängig“ zurücknehmbar, danach nicht mehr – vorher Sicherung machen.",
@"MAP AS BUILDING GROUND

Menu Map → ""Load map area…"":
1. Choose the map, drag the area with the left mouse (right drag = pan, Ctrl+wheel = zoom).
2. Base Z: which world height is Z 0 in the multi (auto = median of the ground).
3. Optional: new blank multi with area size and/or take existing statics as editable tiles.
4. ""Find static"": enter an id, positions are marked.

In the building view:
• Land and statics of the map are drawn below the multi (statics can be semi-transparent).
• Heights (H) shows the ground height of every cell (white = base, orange = higher, blue = lower).
• ""Snap to ground"": drawing without a tile below the mouse uses the ground height.
• Right click → ""Background static as draw tile"" takes the top static of the cell.
• Menu Map → ""Find static in area"" marks all statics with an id.

Write into statics (menu Map → ""Write multi to statics…""):
• Every tile becomes a static at world position (map X + cell X, map Y + cell Y, base Z + Z).
• ""Save now"" writes statics*.mul / staidx*.mul into the output folder. Otherwise they stay pending (visible in the background at once) and are written with menu Map → ""Save map statics…"".
• A freeze file (WSC) can be created too.
Can be undone with ""Undo map change"" before saving, not after – make a backup first."),

            ("Tiles & Gruppen", "Tiles & groups",
@"TILES UND GRUPPEN

Rechtsklick auf den Baum im Reiter Tiles:
• Neue Gruppe / neue Untergruppe, umbenennen, löschen
• Zeichen-Kachel hinzufügen, ID-Bereich hinzufügen (z.B. 0x0400 - 0x0420)
• Alle Kacheln des aktuellen Multis als Untergruppe
• Kacheln suchen (Name oder ID) → Gruppe „Suchergebnis“
• Speichern: schreibt plugins/multieditor.xml (vorher wird eine .bak angelegt), Neu laden

Rechtsklick in die Kachel-Palette: Kachel als Zeichen-Kachel, zu einer Untergruppe hinzufügen, aus der Untergruppe entfernen, TileData-Info.
„Zuletzt benutzt“ enthält deine zuletzt gezeichneten Kacheln.
Im Items-Reiter von UO Fiddler: Rechtsklick → „MultiEditor: Select Item“ übernimmt das Item als Zeichen-Kachel.",
@"TILES AND GROUPS

Right click the tree in the Tiles tab:
• new group / new subgroup, rename, delete
• add draw tile, add id range (e.g. 0x0400 - 0x0420)
• all tiles of the current multi as subgroup
• search tiles (name or id) → group ""Search result""
• Save: writes plugins/multieditor.xml (a .bak is created first), Reload

Right click the tile palette: use as draw tile, add to a subgroup, remove from the subgroup, tiledata info.
""Recently used"" contains the tiles you drew last.
In the Items tab of UO Fiddler: right click → ""MultiEditor: Select Item"" takes the item as draw tile."),

            ("Hausbau-Generator", "House generator",
@"HAUSBAU-GENERATOR

Menü Generator → „Hausbau-Generator…“.
• Stil wählen, Breite/Tiefe/Etagen/Dach/Fenster/Treppen einstellen → Vorschau.
• „Als neues Multi“ erzeugt ein neues Multi, „In aktuelles Multi einfügen“ setzt es dazu (ein Undo-Schritt).
• Teile des Stils (Wand X/Y, Ecke, Pfosten, Boden, Fenster, Dach, Treppe) sind in der Tabelle änderbar.
  Zeile wählen + „Zeichen-Tile → Teil“ übernimmt die aktuelle Zeichen-Kachel.

LERNEN AUS BEISPIELEN:
Der Generator lernt aus vorhandenen Multis. Für jede Kachel wird aus den Nachbarn und den TileData-Flags erkannt,
welche Rolle sie hat (Wand entlang X oder Y, Ecke, Pfosten, Boden, Dachseite, First, Treppe, Fenster).
• „Aktuelles Multi“, „Multi.mul Bereich…“ (z.B. 0x64-0x7F = klassische Häuser) oder „Dateien…“ (alle Importformate, auch ServUO-Skripte).
• Jedes Beispiel ergibt einen Stil und verbessert die Rollen-Erkennung. Das Wissen wird in plugins/MultiEditorStyles.json gespeichert.
• „Zufall“ baut ein Haus aus einem gelernten Stil mit zufälliger Größe.
• „Aktuelles Multi umstylen“ ersetzt Wände/Böden/Dächer des geladenen Multis durch die Teile des gewählten Stils.
• Beim Drehen/Spiegeln werden bekannte Wand-Paare (X ↔ Y) automatisch getauscht.
Es ist keine KI, sondern Statistik: je mehr gute Beispiele, desto besser die Ergebnisse.

VORLAGEN (ab 2.3)
Aus einem gut gebauten Beispielhaus lernt er eine genaue Vorlage: Boden, Etagenhöhe, wo die Wandlinien zur Bodenkante liegen,
die vier Ecken, die vier Seiten mit ihrem Muster (z.B. Wand – Fenster), jede Etage einzeln und das Dach (Art, Überstand, Stufe, Reihen).
Ein Haus in gleicher Größe wird damit exakt nachgebaut, größere Häuser nach denselben Regeln.
• Rechts oben siehst du das Beispiel: rot = Ecken, blau = Nord/Süd-Seiten, grün = West/Ost-Seiten, orange = Dach. So erkennst du, was er gelernt hat.
• Reiter „Gelernt“: alle Werte ansehen und korrigieren (Format 0xID@dz, mehrere Kacheln mit +, Muster mit | getrennt).
• Reiter „Bauteile“: fertige Teile (Treppe, Türrahmen, Zinnen, Möbel) mit Position, Etage und Drehung ins Haus setzen.
• Reiter „Rollen“: falsch erkannte Kacheln korrigieren (gelb = von Hand gesetzt).
• Tipp: nur saubere, fertige Häuser lernen – am besten ein Haus pro Stil über „Aktuelles Multi“.",
@"HOUSE GENERATOR

Menu Generator → ""House generator…"".
• Choose a style, set width/depth/floors/roof/windows/stairs → preview.
• ""Create as new multi"" creates a new multi, ""Insert into current multi"" adds it (one undo step).
• Parts of the style (wall X/Y, corner, post, floor, window, roof, stair) can be changed in the table.
  Select a row + ""Draw tile → part"" takes the current draw tile.

LEARNING FROM EXAMPLES:
The generator learns from existing multis. For every tile its neighbours and tiledata flags tell
which role it has (wall along X or Y, corner, post, floor, roof side, ridge, stair, window).
• ""Current multi"", ""Multi.mul range…"" (e.g. 0x64-0x7F = classic houses) or ""Files…"" (all import formats, also ServUO scripts).
• Every example gives a style and improves the role detection. The knowledge is stored in plugins/MultiEditorStyles.json.
• ""Random"" builds a house from a learned style with random size.
• ""Restyle current multi"" replaces walls/floors/roofs of the loaded multi with the parts of the chosen style.
• Rotating/mirroring swaps known wall pairs (X ↔ Y) automatically.
It is not AI but statistics: the more good examples, the better the results.

TEMPLATES (since 2.3)
From a well built example house an exact template is learned: floor, floor height, where the wall lines lie to the floor edge,
the four corners, the four sides with their pattern (e.g. wall – window), every level and the roof (type, overhang, step, rows).
A house of the same size is rebuilt exactly, bigger houses with the same rules.
• Top right shows the example: red = corners, blue = north/south sides, green = west/east sides, orange = roof. So you see what was learned.
• Tab ""Learned"": see and correct all values (format 0xID@dz, several tiles with +, pattern separated by |).
• Tab ""Parts"": place ready parts (stairs, door frame, battlements, furniture) with position, floor and rotation.
• Tab ""Roles"": correct wrongly detected tiles (yellow = set by hand).
• Tip: learn only clean, finished houses – best one house per style via ""Current multi""."),

            ("Struktur-Werkzeug", "Structure tool",
@"STRUKTUR-WERKZEUG (Taste B)

Zweite Werkzeugleiste: Knopf „Struktur“, daneben was gebaut wird und der Baustil.
• Mauer: eine Linie ziehen – entlang X oder Y wird automatisch erkannt. Am Ende kommt ein Pfosten (Umschalt = ohne).
• Raum (Wände + Boden): ein Viereck ziehen. Die Wände liegen auf dem Rand, der Boden innen.
• Raum + Flachdach / Raum + Satteldach (Schindeln): der First läuft entlang der längeren Seite, Umschalt = andere Richtung.
• Nur Boden, nur Flachdach, nur Satteldach: für Flächen und Dächer auf vorhandene Mauern.
• Gelernte Muster: siehe unten.

Aufstocken (Optionen, Standard an): ziehst du über ein vorhandenes Gebäude, wird automatisch auf der Oberkante der Mauern weitergebaut
– so entsteht Etage für Etage. Dächer werden dabei ignoriert. Ausschalten, wenn du auf der Bau-Ebene bauen willst.
Optionen: Fenster einbauen, Türöffnung im Süden.
Während des Ziehens siehst du eine Vorschau, unten stehen Größe und Z.

Baustile kommen aus dem Hausbau-Generator (gelernt oder selbst angelegt).

LINIEN-MUSTER ANLERNEN:
Ein Stück Mauer auswählen (z.B. 3 Felder: Wand, Wand, Fenster – auch mit Zinnen obendrauf), dann Optionen → „Linien-Muster aus Auswahl lernen“.
Beim Ziehen wird das Muster wiederholt. In die andere Richtung gezogen tauscht er bekannte Wandteile (X ↔ Y).
Die Muster stehen danach in der Auswahlliste und werden in plugins/MultiEditorStructures.json gespeichert.

Für Programmierer: eigene Muster als Klasse mit IStructurePattern in Classes/StructureBuilder.cs anlegen und in BuiltInPatterns eintragen.",
@"STRUCTURE TOOL (key B)

Second toolbar: button ""Structure"", next to it what is built and the building style.
• Wall: drag a line – along X or Y is detected automatically. A post is placed at the end (Shift = without).
• Room (walls + floor): drag a rectangle. The walls lie on the border, the floor inside.
• Room + flat roof / room + gable roof (shingles): the ridge runs along the longer side, Shift = other direction.
• Floor only, flat roof only, gable roof only: for areas and roofs on existing walls.
• Learned patterns: see below.

Stack (options, on by default): dragging over an existing building builds on top of the walls automatically
– floor by floor. Roofs are ignored. Turn it off to build on the build level.
Options: windows, door gap in the south.
While dragging you see a preview, size and Z are shown at the bottom.

Building styles come from the house generator (learned or created by you).

LEARNING LINE PATTERNS:
Select a piece of wall (e.g. 3 cells: wall, wall, window – also with battlements on top), then options → ""Learn line pattern from selection"".
Dragging repeats the pattern. Dragged in the other direction, known wall parts are swapped (X ↔ Y).
The patterns appear in the list and are stored in plugins/MultiEditorStructures.json.

For programmers: write your own pattern class with IStructurePattern in Classes/StructureBuilder.cs and add it to BuiltInPatterns."),

            ("Bauteile", "Building parts",
@"BAUTEILE (Reiter „Bauteile“)

Eine Bibliothek fertiger Teile: Treppen, Türrahmen, Zinnen, Möbel, ganze Räume …
• „+ Auswahl“: die aktuelle Auswahl als Bauteil speichern (Name + Kategorie).
• „+ Map“: die ausgewählten Map-Statics als Bauteil speichern (z.B. eine schöne Baumgruppe oder ein Brunnen von der Map).
• „+ Datei“: Dateien (alle Importformate, auch ServUO-Skripte) als Bauteile übernehmen.
• Klick auf ein Bauteil = Stempel. Klicken setzt es (mehrmals möglich), R dreht es um 90°, Esc beendet.
  Gesetzt wird auf der Bau-Ebene bzw. auf dem Map-Boden.
• Treppen aus gelernten Stilen werden automatisch angelegt (Kategorie Auto).
Gespeichert in plugins/MultiEditorStructures.json.",
@"BUILDING PARTS (tab ""Parts"")

A library of ready parts: stairs, door frames, battlements, furniture, whole rooms …
• ""+ Selection"": store the current selection as part (name + category).
• ""+ Map"": store the selected map statics as part (e.g. a group of trees or a well from the map).
• ""+ File"": take files (all import formats, also ServUO scripts) as parts.
• Click a part = stamp. Clicking places it (several times), R rotates it by 90°, Esc stops.
  It is placed on the build level or the map ground.
• Stairs from learned styles are created automatically (category Auto).
Stored in plugins/MultiEditorStructures.json."),

            ("Ebenen & Lineal", "Layers & ruler",
@"EBENEN UND HÖHENLINEAL

Reiter „Ebenen“: eine Ebene ist ein Z-Bereich. „Auto“ legt eine Ebene pro Etage an (Etagenhöhe einstellbar, Standard 20).
• Haken = sichtbar. „Nur diese“ zeigt nur die gewählte Ebene, die darunter durchscheinend.
• Durchsch. = halbtransparent, Sperren = nicht anklickbar (z.B. die fertige Etage darunter).
• Bau-Ebene (oder Doppelklick) setzt den virtuellen Boden auf diese Ebene.
• Neu / Bearb. für eigene Bereiche (z.B. nur das Dach).
• Die Zeile „Einrichtung“ blendet alle Einrichtungs-Kacheln ein/aus.
• Rechtsklick auf eine Kachel → Ebene ausblenden / nur diese zeigen.

Höhenlineal (L) am linken Rand: Z-Skala an der Zelle unter der Maus (sonst am Ursprung).
Farbige Bänder = Ebenen, grün = Bau-Ebene, orange = Oberkante des Gebäudes mit Gesamthöhe, gelb = Z der Kachel unter der Maus.",
@"LAYERS AND HEIGHT RULER

Tab ""Layers"": a layer is a Z range. ""Auto"" creates one layer per floor (floor height adjustable, default 20).
• Check = visible. ""Only this"" shows only the chosen layer, the ones below ghosted.
• Ghost = semi transparent, lock = not clickable (e.g. the finished floor below).
• Build level (or double click) sets the virtual floor to this layer.
• New / edit for custom ranges (e.g. only the roof).
• The row ""Furniture"" shows/hides all furniture tiles.
• Right click a tile → hide layer / show only this.

Height ruler (L) at the left border: Z scale at the cell below the mouse (otherwise at the origin).
Colored bands = layers, green = build level, orange = top of the building with total height, yellow = Z of the tile below the mouse."),

            ("Einrichtung & Ansicht drehen", "Furniture & rotate view",
@"EINRICHTUNG

Kacheln können als „Einrichtung“ markiert werden (Möbel, Deko, Teppiche …):
• Knopf „Einrichtung“ in der zweiten Werkzeugleiste (Taste E): alles neu Gezeichnete wird Einrichtung.
• Rechtsklick → „Einrichtung (nur Addon)“ oder Menü Bearbeiten für die Auswahl.
• Einrichtung wird NICHT in die Multi.mul geschrieben. Beim ServUO-Export wählst du: alles, nur Einrichtung oder ohne Einrichtung.
  So wird das Haus ein Multi und die Möbel ein eigenes Addon.
• Sie wird leicht gelblich eingefärbt (Menü Ansicht) und hat eine eigene Zeile im Ebenen-Reiter.

ANSICHT UM 90° DREHEN
Knopf „↻ Ansicht“ oder Menü Ansicht. Damit siehst du die Rückseite. Bekannte Wandteile (X ↔ Y) werden getauscht.
Speichern, Export, Multi.mul und Statics nutzen immer die Original-Ausrichtung. Die Drehung ist ein Undo-Schritt.
Hinweis: Grafiken, deren Gegenstück unbekannt ist (z.B. Map-Statics), behalten ihr Aussehen.",
@"FURNITURE

Tiles can be marked as ""furniture"" (furniture, deco, carpets …):
• Button ""Furniture"" in the second toolbar (key E): everything drawn new becomes furniture.
• Right click → ""Furniture (addon only)"" or the Edit menu for the selection.
• Furniture is NOT written into multi.mul. On ServUO export you choose: everything, furniture only or without furniture.
  So the house becomes a multi and the furniture an own addon.
• It is tinted slightly yellow (View menu) and has its own row in the layers tab.

ROTATE VIEW BY 90°
Button ""↻ View"" or the View menu. You see the back side. Known wall parts (X ↔ Y) are swapped.
Save, export, multi.mul and statics always use the original orientation. The rotation is an undo step.
Note: art without a known counterpart (e.g. map statics) keeps its look."),

            ("Map-Statics bearbeiten", "Edit map statics",
@"MAP-STATICS AUSWÄHLEN, KOPIEREN, VERSCHIEBEN

Map-Bereich laden (Menü Map), dann Werkzeug „Map-Auswahl“ (Taste M):
• Klick = oberstes Static der Zelle, ziehen = alle Statics im Bereich (sichtbarer Z-Bereich), Strg = hinzufügen.
• Ausgewählte Statics sind türkis umrandet.
Rechtsklick → Map-Statics:
• Kopieren: als Stempel ins Multi einfügen (R dreht).
• Als Kacheln übernehmen: werden bearbeitbare Kacheln im Multi, die Map bleibt wie sie ist.
• Ins Multi verschieben: wie oben, aber auf der Map entfernt.
• Von Map löschen.
• Als Bauteil speichern.
Alle Map-Änderungen (auch „Multi in Statics schreiben“) kannst du mit „Map-Änderung rückgängig“ zurücknehmen,
solange sie nicht gespeichert sind. „Map-Statics speichern…“ schreibt statics/staidx in den Ausgabeordner.",
@"SELECT, COPY, MOVE MAP STATICS

Load a map area (menu Map), then tool ""Map select"" (key M):
• Click = top static of the cell, drag = all statics in the area (visible Z range), Ctrl = add.
• Selected statics have a cyan border.
Right click → map statics:
• Copy: paste into the multi as stamp (R rotates).
• Take as tiles: they become editable tiles in the multi, the map stays as it is.
• Move into multi: as above, but removed from the map.
• Delete from map.
• Save as building part.
All map changes (also ""write multi to statics"") can be undone with ""Undo map change""
as long as they are not saved. ""Save map statics…"" writes statics/staidx into the output folder."),

            ("ServUO-Haus & Prüfung", "ServUO house & check",
@"SERVUO HAUS-SKRIPT (BaseHouse)

Statt eines Addons wird ein richtiges Haus mit Urkunde erzeugt (Datei → „ServUO Haus-Skript exportieren…“ oder Menü Extras).
1. Rechtsklick in der Bauansicht → Haus (ServUO): Tür Süd / Tür Ost, Hausschild und Ban-Location setzen.
   Die Markierungen werden als farbige Rauten angezeigt (TS/TO = Tür, S = Schild, B = Ban). Sie werden im Projekt (.mep) gespeichert.
2. Multi unter einer ID in die Multi.mul schreiben – Server und Client brauchen dieselbe Datei.
3. Skript exportieren: Klassenname, Urkunden-Name, Multi-ID, Preis, Lockdowns, Secures, Holz- oder Metalltüren.
   Die Hausfläche (Area) wird aus den Kacheln berechnet, Einrichtung zählt nicht.
4. Datei nach Scripts/Custom, Server neu starten, im Spiel [add <Name>Deed.
Namespace Server.Multis.Deeds funktioniert mit ServUO und RunUO.

BEGEHBARKEITS-PRÜFUNG (Knopf „Prüfen“ oder Extras)
• Stufe zu hoch: ein Schritt von mehr als 2 Z ohne Treppe.
• Fehlender Boden: ein Loch, wo die Nachbarn auf gleicher Höhe begehbar sind.
• Decke zu niedrig: weniger als 16 Z Platz über einem Boden.
• Nicht erreichbar: mit Tür- oder Ban-Markierung wird geprüft, welche Flächen man von dort erreicht (das Dach oben wird ausgelassen).
Die Stellen werden farbig markiert, ein Klick in der Liste springt dorthin. Es ist eine Hilfe, keine genaue Server-Simulation.",
@"SERVUO HOUSE SCRIPT (BaseHouse)

Instead of an addon a real house with deed is created (File → ""Export ServUO house script…"" or menu Extras).
1. Right click in the building view → House (ServUO): set south / east door, house sign and ban location.
   The markers are shown as colored diamonds (TS/TO = door, S = sign, B = ban). They are stored in the project (.mep).
2. Write the multi under an id into multi.mul – server and client need the same file.
3. Export the script: class name, deed name, multi id, price, lockdowns, secures, wooden or metal doors.
   The house area is calculated from the tiles, furniture does not count.
4. Put the file into Scripts/Custom, restart the server, in game [add <Name>Deed.
Namespace Server.Multis.Deeds works with ServUO and RunUO.

WALKABILITY CHECK (button ""Check"" or Extras)
• Step too high: a step of more than 2 Z without stairs.
• Missing floor: a hole where the neighbours are walkable at the same height.
• Ceiling too low: less than 16 Z room above a floor.
• Not reachable: with a door or ban marker it checks which surfaces can be reached from there (the roof on top is skipped).
The places are marked in color, a click in the list jumps there. It is a help, not an exact server simulation."),

            ("Symmetrie, Farben, Listen", "Symmetry, hues, lists",
@"SYMMETRIE-MODUS
Knopf „Symmetrie“ in der zweiten Werkzeugleiste: X (links/rechts), Y (vorne/hinten) oder 4-fach.
Alles, was du zeichnest, ziehst (Rechteck, Linie, Struktur) oder einfügst, wird an der Multi-Mitte gespiegelt; Entfernen löscht die Spiegelkachel mit.
Die Achsen werden lila gestrichelt angezeigt. Wände liegen auf der Zellkante und werden passend versetzt, Dachseiten (Nord/Süd, West/Ost) getauscht.

HUE MIT FARBVORSCHAU
Knopf „Farbe…“ oder Rechtsklick → Hue setzen: Liste aller Hues mit Farbverlauf, Suche nach Nummer oder Name,
Vorschau der Kachel mit der Farbe, zuletzt benutzte Farben als Knöpfe.

MATERIAL- UND KACHELLISTE (Extras)
Alle IDs mit Anzahl, Name, Einrichtung, Hue und erkannter Rolle. Spalten zum Sortieren anklicken.
Preis pro Stück setzen (auch für alle mit gleicher Rolle) – die Summe wird berechnet.
Export als CSV, JSON oder C#-Tabelle {id, anzahl} für Crafting- oder Kosten-Skripte. Doppelklick wählt die Kacheln im Multi aus.

MULTI-BROWSER (Datei oder Extras)
Alle Multis der multi.mul als Vorschaubilder, Filter nach ID oder Name, Größe einstellbar. Doppelklick öffnet das Multi im Editor.",
@"SYMMETRY MODE
Button ""Symmetry"" in the second toolbar: X (left/right), Y (front/back) or 4-way.
Everything you draw, drag (rectangle, line, structure) or paste is mirrored at the multi center; removing deletes the mirrored tile too.
The axes are shown as purple dashed lines. Walls lie on the cell border and are moved accordingly, roof sides (north/south, west/east) are swapped.

HUE WITH COLOR PREVIEW
Button ""Color…"" or right click → set hue: list of all hues with color gradient, search by number or name,
preview of the tile with the hue, recently used hues as buttons.

MATERIAL AND TILE LIST (Extras)
All ids with count, name, furniture, hue and detected role. Click columns to sort.
Set a price per piece (also for all with the same role) – the sum is calculated.
Export as CSV, JSON or C# table {id, count} for crafting or cost scripts. Double click selects the tiles in the multi.

MULTI BROWSER (File or Extras)
All multis of multi.mul as thumbnails, filter by id or name, adjustable size. Double click opens the multi in the editor."),
        };

        private HelpForm()
        {
            Text = "MultiEditor – Hilfe / Help";
            Size = new Size(940, 640);
            StartPosition = FormStartPosition.CenterScreen;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
            var de = new Button { Text = "Deutsch", AutoSize = true };
            var en = new Button { Text = "English", AutoSize = true };
            de.Click += (_, _) => SetLanguage(true);
            en.Click += (_, _) => SetLanguage(false);
            top.Controls.Add(de);
            top.Controls.Add(en);

            _topics = new TreeView { Dock = DockStyle.Left, Width = 220, HideSelection = false, Font = new Font(Font.FontFamily, 10f) };
            _text = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = SystemColors.Window, Font = new Font("Segoe UI", 10f), BorderStyle = BorderStyle.None };
            var split = new Splitter { Dock = DockStyle.Left };

            Controls.Add(_text);
            Controls.Add(split);
            Controls.Add(_topics);
            Controls.Add(top);

            _topics.AfterSelect += (_, e) => ShowPage((int)e.Node.Tag);
            SetLanguage(L.German);
        }

        public static void ShowHelp(IWin32Window owner, int page = 0)
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new HelpForm();
            }

            _instance.SelectPage(page);
            if (!_instance.Visible)
            {
                _instance.Show(owner);
            }

            _instance.BringToFront();
        }

        private void SetLanguage(bool german)
        {
            _german = german;
            int sel = _topics.SelectedNode != null ? (int)_topics.SelectedNode.Tag : 0;
            _topics.Nodes.Clear();
            for (int i = 0; i < _pages.Length; ++i)
            {
                _topics.Nodes.Add(new TreeNode(german ? _pages[i].de : _pages[i].en) { Tag = i });
            }

            SelectPage(sel);
        }

        private void SelectPage(int page)
        {
            page = Math.Max(0, Math.Min(_pages.Length - 1, page));
            if (_topics.Nodes.Count > page)
            {
                _topics.SelectedNode = _topics.Nodes[page];
            }

            ShowPage(page);
        }

        private void ShowPage(int page)
        {
            string text = _german ? _pages[page].textDe : _pages[page].textEn;
            _text.Clear();
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];
                bool heading = line.Length > 0 && line == line.ToUpperInvariant() && line.Trim().Length > 3 && !line.StartsWith("•") && char.IsLetter(line.Trim()[0]);
                _text.SelectionFont = heading ? new Font(_text.Font.FontFamily, 12f, FontStyle.Bold) : _text.Font;
                _text.SelectionColor = heading ? Color.SteelBlue : SystemColors.WindowText;
                _text.AppendText(line + (i < lines.Length - 1 ? "\n" : string.Empty));
            }

            _text.SelectionStart = 0;
            _text.ScrollToCaret();
        }
    }
}
