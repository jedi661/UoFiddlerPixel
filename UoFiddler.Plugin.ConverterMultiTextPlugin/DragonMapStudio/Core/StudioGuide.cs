/***************************************************************************
 *
 *  * Dragon Map Studio – help window content: guide and command reference in German and English
 *  *
 *  *  Simple markup, shown by the help window (and exported as HTML):
 *  *    "# "  heading           "## " sub heading       "- " point
 *  *    "Key | text"  row of a command table (key or mouse, then what it does)
 *  *    empty line = new paragraph
 *
 ***************************************************************************/

using System.Collections.Generic;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public sealed class GuideTopic
    {
        public string Key;
        public string Group;
        public string TitleDe;
        public string TitleEn;
        public string De;
        public string En;

        public string Title(bool en) => en ? TitleEn : TitleDe;

        public string Text(bool en) => en ? En : De;
    }

    public static class StudioGuide
    {
        public static readonly List<GuideTopic> Topics = new List<GuideTopic>
        {
            // ============================================================ start
            new GuideTopic
            {
                Key = "start", Group = "Anleitung|Guide", TitleDe = "Erste Karte in 10 Schritten", TitleEn = "First map in 10 steps",
                De = @"# Erste Karte in 10 Schritten
Dragon Map Studio malt die Karte als Farbbild: jede Farbe steht für Gelände + Höhe. Daraus baut es Land, Übergänge und Statics (map#.mul / statics#.mul / UOP).

- 1. Datei → Neue Karte: Größe wählen (z. B. 1448×1448 für eine kleine Welt).
- 2. Links die Werkzeug-Palette: Pinsel (B) wählen. Oben in der Optionsleiste die Pinselgröße einstellen.
- 3. Links im Farben-Bereich oder im Pinsel-Katalog eine Farbe wählen: linke Maustaste = Farbe 1, rechte = Farbe 2.
- 4. Erst alles mit Wasser füllen (Füllen, F), dann Kontinente mit Gras malen.
- 5. Hügel in Stufen malen (Gras 5, darin Gras 10 …), Berge in Ringen. Übergänge brauchen mindestens 2 Tiles.
- 6. Live-CGI (Leiste oben) einschalten: ein kleines Fenster zeigt beim Malen, wie es im Client aussieht.
- 7. Einen Bereich markieren (Auswahl, S) und V drücken: schnelle CGI-Vorschau nur dieses Bereichs.
- 8. Bearbeiten → Karte prüfen (Log) zeigt Fehler, Karte reparieren behebt viele davon.
- 9. Reiter MUL / UOP erzeugen: Karte bauen. In der CGI-Ansicht weiter bearbeiten (Statics, Küste glätten, Höhlen).
- 10. Speichern. Rechtsklick auf ein Werkzeug der Palette zeigt jederzeit seine Hilfe, F1 öffnet dieses Fenster.

## Tipps
- Jede Farbe hat genau eine Höhe. Soll ein Boden höher liegen als seine Farbe, „Höhe behalten“ einschalten – die Höhen-Ebene merkt sich die Höhe.
- Strg+Z nimmt jeden Schritt zurück, Strg+Umschalt+Z einen ganzen Strich.
- Rechtsklick (mit Auswahl / Hand / Pipette) oder Umschalt+Rechtsklick öffnet das Kontextmenü mit den meisten Befehlen.",
                En = @"# First map in 10 steps
Dragon Map Studio paints the map as a colour picture: every colour stands for terrain + height. From it the studio builds land, transitions and statics (map#.mul / statics#.mul / UOP).

- 1. File → New map: choose a size (e.g. 1448×1448 for a small world).
- 2. Tool palette on the left: choose the brush (B). Set the brush size in the options bar at the top.
- 3. Choose a colour in the colours section or the brush catalogue: left mouse = colour 1, right = colour 2.
- 4. First fill everything with water (Fill, F), then paint the continents with grass.
- 5. Paint hills in steps (grass 5, inside grass 10 …), mountains in rings. Transitions need at least 2 tiles.
- 6. Switch on Live CGI (bar at the top): a small window shows while painting how it looks in the client.
- 7. Mark an area (Select, S) and press V: quick CGI preview of only this area.
- 8. Edit → Check map (log) lists errors, Repair map fixes many of them.
- 9. Tab MUL / UOP: build the map. Keep editing in the CGI view (statics, smooth coast, caves).
- 10. Save. Right click on a tool of the palette always shows its help, F1 opens this window.

## Tips
- Every colour has exactly one height. If a floor should lie higher than its colour, switch on “Keep height” – the height layer remembers the height.
- Ctrl+Z undoes every step, Ctrl+Shift+Z a whole stroke.
- Right click (with Select / Hand / Picker) or Shift+right click opens the context menu with most commands."
            },
            new GuideTopic
            {
                Key = "layout", Group = "Anleitung|Guide", TitleDe = "Aufbau des Fensters", TitleEn = "Window layout",
                De = @"# Aufbau des Fensters
## Werkzeug-Palette (links)
Wie bei Photoshop: alle Werkzeuge als Bilder in einer Spalte links neben der Karte, in Gruppen. Der kleine Buchstabe unten rechts ist die Taste.
- Linksklick = Werkzeug wählen.
- Rechtsklick = Hilfe zu diesem Werkzeug.
- Maus darüber = Name, Taste und Kurz-Hilfe.
- ⧉ oben = Palette in ein eigenes Fenster (z. B. auf den zweiten Monitor). Fenster schließen oder ⇤ = wieder links andocken. Die Lage wird gemerkt.
## Optionsleiste (oben)
Zeigt nur, was das gewählte Werkzeug noch hat: Pinselgröße, eckig / rund, „Höhe behalten“, Befehle der Auswahl … Rechts stehen immer Rückgängig, Ansicht, Live-CGI und CGI.
## Seitenleisten
Kartenübersicht (zoomen, abdocken), Pinsel-Katalog, Stempel, Farben – jeder Bereich lässt sich ein- und ausklappen.
## Kontextmenü
Karten-Editor: Rechtsklick mit Auswahl / Hand / Pipette, sonst Umschalt+Rechtsklick. CGI-Ansicht: Rechtsklick mit Auswahl / Bereich / Löschen / Färben / Multi / Vorlage, sonst Umschalt+Rechtsklick oder die Menütaste.",
                En = @"# Window layout
## Tool palette (left)
Like Photoshop: every tool as a picture in a column left of the map, in groups. The small letter at the bottom right is the key.
- Left click = choose the tool.
- Right click = help for this tool.
- Mouse over = name, key and short help.
- ⧉ at the top = palette in its own window (e.g. on the second monitor). Close the window or ⇤ = dock it back at the left. The place is remembered.
## Options bar (top)
Shows only what the chosen tool offers: brush size, square / round, “Keep height”, commands of the selection … On the right there are always undo, view, live CGI and CGI.
## Side panels
Overview map (zoom, undock), brush catalogue, stamps, colours – every section folds open and closed.
## Context menu
Map editor: right click with Select / Hand / Picker, otherwise Shift+right click. CGI view: right click with Select / Area / Delete / Hue / Multi / Template, otherwise Shift+right click or the menu key."
            },

            // ============================================================ map editor
            new GuideTopic
            {
                Key = "editor", Group = "Karten-Editor|Map editor", TitleDe = "Karten-Editor: Werkzeuge", TitleEn = "Map editor: tools",
                De = @"# Karten-Editor: Werkzeuge
Linke Maus = Farbe 1, rechte Maus = Farbe 2. Mausrad = Zoom, mittlere Maus oder Leertaste + Ziehen = verschieben.
B | Pinsel: rund oder eckig (Optionsleiste), Größe mit [ und ].
E | Radierer: malt Farbe 2.
– | Sprühen: einzelne Punkte, Dichte in den Einstellungen.
L | Linie: Dicke = Pinselgröße.
– | Rechteck, Ellipse: Rahmen mit Pinselgröße als Dicke.
R | Rechteck gefüllt. Auch Ellipse gefüllt.
F | Füllen: zusammenhängende gleiche Farbe.
I | Pipette: Farbe aufnehmen (links = Farbe 1, rechts = Farbe 2).
– | Höhe + / Höhe −: nächste Farbe der Gruppe höher / tiefer (rechte Maus umgekehrt).
S | Auswahl: Bereich aufziehen. Dann Strg+C / X / V, Entf (Farbe 2), Strg+F (Farbe 1), V = CGI-Vorschau.
H | Hand: Ansicht verschieben.
– | Höhleneingang: Klick neben den Berg = Eingang, Umschalt = unsichtbar (1d), Rechtsklick = wieder Berg.
– | Muster-Pinsel: ein Stempel als Pinsel (Straßen, Flüsse) – aus dem Stempel-Menü.
## Optionsleiste
- Pinselgröße, eckig, Höhe behalten (bei Pinsel, Radierer, Sprühen, Linie, Formen, Füllen).
- Bei der Auswahl: Kopieren, Ausschneiden, Einfügen, Füllen, Löschen, CGI-Vorschau, als Stempel speichern.
## Tasten
Strg+Z / Strg+Y | Rückgängig / Wiederholen
Strg+Umschalt+Z | ganzen Strich rückgängig (Muster-Pinsel schrittweise)
Strg+A | alles auswählen
+ / − | Zoom
Pfeile | Ansicht verschieben
Esc | Auswahl / Einfügen beenden",
                En = @"# Map editor: tools
Left mouse = colour 1, right mouse = colour 2. Wheel = zoom, middle mouse or space + drag = pan.
B | Brush: round or square (options bar), size with [ and ].
E | Eraser: paints colour 2.
– | Spray: single dots, density in the settings.
L | Line: thickness = brush size.
– | Rectangle, ellipse: outline with the brush size as thickness.
R | Filled rectangle. Also filled ellipse.
F | Fill: connected cells of the same colour.
I | Picker: take a colour (left = colour 1, right = colour 2).
– | Height + / height −: next colour of the group higher / lower (right mouse the other way).
S | Select: drag an area. Then Ctrl+C / X / V, Del (colour 2), Ctrl+F (colour 1), V = CGI preview.
H | Hand: pan the view.
– | Cave entrance: click next to the mountain = entrance, Shift = invisible (1d), right click = mountain again.
– | Pattern brush: a stamp as brush (roads, rivers) – from the stamp menu.
## Options bar
- Brush size, square, keep height (brush, eraser, spray, line, shapes, fill).
- With Select: copy, cut, paste, fill, delete, CGI preview, save as stamp.
## Keys
Ctrl+Z / Ctrl+Y | Undo / redo
Ctrl+Shift+Z | undo the whole stroke (pattern brush stepwise)
Ctrl+A | select all
+ / − | zoom
Arrows | pan the view
Esc | end selection / paste"
            },
            new GuideTopic
            {
                Key = "heights", Group = "Karten-Editor|Map editor", TitleDe = "Höhen und Höhen-Ebene", TitleEn = "Heights and height layer",
                De = @"# Höhen und Höhen-Ebene
In einer Dragon-Farbkarte hat jede Farbe genau eine Höhe (maptrans). Holzboden 1f gibt es z. B. nur auf Z 0. Die Palette hat kaum noch freie Plätze für weitere Höhen.
## Höhen-Ebene
Die Höhen-Ebene liegt in der Datei <Karte>.statics.bin. Für einzelne Zellen merkt sie eine eigene Höhe. Der Build setzt diese Höhe, solange die Zelle noch die gemerkte Farbe hat.
## Höhe behalten (Optionsleiste, Kontextmenü)
- An: Pinsel, Radierer, Sprühen, Linie, Formen, Füllen und Auswahl füllen nehmen die Farbe der Gruppe, die auf der Höhe des Bodens liegt. Gibt es sie nicht (Holzboden auf Gras Z 4), kommt die Höhe in die Höhen-Ebene. Ergebnis: Holzboden auf Z 4.
- Aus: jede Farbe hat ihre eigene Höhe. Ein übermalter Eintrag der Höhen-Ebene wird entfernt.
- Muster-Pinsel und Stempel nutzen die Höhen-Ebene immer (Höhen-Modus im Stempel-Menü).
- Alles geht mit Strg+Z zurück.
## Anzeigen
- „Höhen-Ebene zeigen“ (Ansicht oder Kontextmenü): türkis = die Höhe gilt, grau = alter Eintrag, die Farbe wurde übermalt (zählt nicht). Ab Zoom 12 steht die Höhe darin.
- „Höhen“ zeigt ab Zoom 12 jede Höhe; Werte aus der Höhen-Ebene sind gelb.
- Die Statuszeile zeigt die Höhe unter der Maus mit „(Höhen-Ebene)“.
## Löschen
Kontextmenü → Höhen-Ebene: Eintrag hier, in der Auswahl oder auf der ganzen Karte löschen.",
                En = @"# Heights and height layer
In a Dragon colour map every colour has exactly one height (maptrans). Wood floor 1f, for example, only exists at Z 0, and the palette has hardly any free places for more heights.
## Height layer
The height layer is stored in the file <map>.statics.bin. It remembers its own height for single cells. The build uses this height as long as the cell still has the remembered colour.
## Keep height (options bar, context menu)
- On: brush, eraser, spray, line, shapes, fill and fill selection take the colour of the group at the height of the ground. If there is none (wood floor on grass Z 4) the height goes into the height layer. Result: wood floor at Z 4.
- Off: every colour has its own height. An entry of the height layer that is painted over is removed.
- Pattern brush and stamps always use the height layer (height mode in the stamp menu).
- Everything can be undone with Ctrl+Z.
## Show
- “Show height layer” (view or context menu): cyan = the height counts, grey = old entry, the colour was painted over (does not count). From zoom 12 the height is written inside.
- “Heights” shows every height from zoom 12; values from the height layer are yellow.
- The status line shows the height under the mouse with “(height layer)”.
## Delete
Context menu → Height layer: delete the entry here, in the selection or on the whole map."
            },
            new GuideTopic
            {
                Key = "stamps", Group = "Karten-Editor|Map editor", TitleDe = "Stempel und Muster-Pinsel", TitleEn = "Stamps and pattern brush",
                De = @"# Stempel und Muster-Pinsel
Ein Farb-Stempel (*.dmstamp.json) enthält Farben, Höhen und Statics eines Bereichs.
- In der CGI-Ansicht: Bereich markieren → P oder „Bereich → Farb-Stempel“.
- Im Karten-Editor: Auswahl → „Auswahl → Stempel speichern“.
- Vorlagen und Multis lassen sich direkt als Stempel nutzen.
## Setzen
- Stempel-Menü (Leiste oben) oder Seitenleiste: einmal oder mehrfach setzen.
- Freihand-Stempel = Muster-Pinsel: Ziehen malt das Muster wie eine Textur (Straßen, Flüsse).
- Ebenen: Land + Statics, nur Land, nur Statics.
- Höhe: Original (aus dem Stempel), Gelände behalten, feste Höhe, dazu ± Versatz.
- Automatisch nachbessern glättet Höhen-Spitzen am Strich.
- Rückgängig schrittweise (je 32 Felder) oder der ganze Strich (Strg+Umschalt+Z).
## Regeln lernen
Aus einem Stempel oder Bereich lernt das Studio Map-Übergänge, Items und Static-Übergänge und schreibt neue Dragon-Skripte.",
                En = @"# Stamps and pattern brush
A colour stamp (*.dmstamp.json) holds colours, heights and statics of an area.
- In the CGI view: mark an area → P or “Area → colour stamp”.
- In the map editor: selection → “Selection → save stamp”.
- Templates and multis can be used as stamps directly.
## Placing
- Stamp menu (bar at the top) or side panel: place once or several times.
- Freehand stamp = pattern brush: dragging paints the pattern like a texture (roads, rivers).
- Layers: land + statics, land only, statics only.
- Height: original (from the stamp), keep terrain, fixed height, plus ± offset.
- Automatic repair smooths height spikes along the stroke.
- Undo stepwise (every 32 cells) or the whole stroke (Ctrl+Shift+Z).
## Learn rules
From a stamp or an area the studio learns map transitions, items and static transitions and writes new Dragon scripts."
            },
            new GuideTopic
            {
                Key = "live", Group = "Karten-Editor|Map editor", TitleDe = "Live-CGI und Sprungtaste", TitleEn = "Live CGI and jump button",
                De = @"# Live-CGI und Sprungtaste
- ◉ Live-CGI: beim Zeichnen mit jedem Werkzeug zeigt ein kleines Fenster die Umgebung der Maus fertig gebaut.
- „folgt Maus“: auch beim bloßen Bewegen der Maus.
- ⌖ Sprung: eine freie Maustaste belegen (Maustaste 4 / 5, Strg oder Alt + Mitteltaste). Ein Klick damit lässt das Live-CGI an diese Stelle springen. Live-Zeichnen und „folgt Maus“ bleiben an.
- Option „Sprung schaltet 'folgt Maus' aus“: die Ansicht bleibt stehen, bis du wieder zeichnest.
- ▶ CGI-Vorschau (V): die Auswahl nur im Speicher bauen und zeigen.
- ⟳ CGI aktualisieren: den sichtbaren Teil (oder die Auswahl) neu bauen und in die CGI-Ansicht übernehmen – der Rest bleibt.",
                En = @"# Live CGI and jump button
- ◉ Live CGI: while drawing with any tool a small window shows the surroundings of the mouse fully built.
- “follows mouse”: also when only moving the mouse.
- ⌖ Jump: assign a free mouse button (button 4 / 5, Ctrl or Alt + middle button). A click with it makes the live CGI jump to that place. Live drawing and “follows mouse” stay on.
- Option “Jump switches 'follows mouse' off”: the view stays until you draw again.
- ▶ CGI preview (V): build only the selection in memory and show it.
- ⟳ Refresh CGI: build the visible part (or the selection) again and take it into the CGI view – the rest stays."
            },

            // ============================================================ CGI view
            new GuideTopic
            {
                Key = "cgi", Group = "CGI-Ansicht|CGI view", TitleDe = "CGI-Ansicht: Werkzeuge", TitleEn = "CGI view: tools",
                De = @"# CGI-Ansicht: Werkzeuge
Die CGI-Ansicht zeigt die gebaute Karte wie der Client. Links die Werkzeug-Palette, rechts die Reiter (Items, Multis, Vorlagen, Land, Küste, Berg, Höhlen, Prüfen).
## Statics
Auswahl | Klick = wählen, Strg = dazu, Umschalt = alle auf dem Tile. Pfeile verschieben, Bild↑/↓ = Z, Entf, H = färben, I = Item übernehmen.
Static setzen | Item aus dem Katalog setzen. Rechtsklick auf ein Static = ID + Hue aufnehmen.
Multi setzen | Multi als Statics setzen.
Vorlage setzen | Gebäude / Böden aus dem Vorlagen-Ordner.
Löschen | Klick löscht ein Static.
Färben | Klick färbt mit der Hue.
## Land
Land setzen | Land-Tile malen, Rechtsklick nimmt ID + Höhe auf.
Höhe + / − / = | heben, senken, feste Höhe.
Glätten | Höhen an die Nachbarn angleichen.
Bereich | Rechteck markieren – siehe „Bereich-Befehle“.
Muster-Pinsel | Stempel mit Land-Tiles / Statics als Pinsel.
## Gelände
Küste | runder Pinsel: Küste wie das Original (Meeresgrund, Wasser-Statics, Strand).
Küste glätten | runder Pinsel: Ufer ohne Zacken, siehe „Küste und Strand“.
Wasser | Wasser-Statics malen, Strg = entfernen.
Berg | Berg ab der Bodenhöhe aufbauen.
Höhle / Eingang / Höhlen-Stempel | siehe „Höhlen und Berge“.
## Ansicht
T | 2D / CGI umschalten
Mausrad | Zoom
Mitte / Leertaste + Ziehen | verschieben
Strg+Z / Strg+Y | Rückgängig / Wiederholen
Strg+Umschalt+Z | ganzer Strich
Umschalt+Rechtsklick / Menütaste | Kontextmenü",
                En = @"# CGI view: tools
The CGI view shows the built map like the client. On the left the tool palette, on the right the tabs (items, multis, templates, land, coast, mountain, caves, check).
## Statics
Select | click = choose, Ctrl = add, Shift = all on the tile. Arrows move, Page↑/↓ = Z, Del, H = hue, I = take the item.
Place static | place the item from the catalogue. Right click on a static = take id + hue.
Place multi | place a multi as statics.
Place template | buildings / floors from the template folder.
Delete | click deletes a static.
Hue | click colours with the hue.
## Land
Set land | paint a land tile, right click takes id + height.
Height + / − / = | raise, lower, fixed height.
Smooth | heights like the neighbours.
Area | mark a rectangle – see “Area commands”.
Pattern brush | stamp with land tiles / statics as brush.
## Terrain
Coast | round brush: coast like the original (sea floor, water statics, beach).
Smooth coast | round brush: shore without teeth, see “Coast and beach”.
Water | paint water statics, Ctrl = remove.
Mountain | build a mountain from the ground height.
Cave / entrance / cave stamp | see “Caves and mountains”.
## View
T | switch 2D / CGI
Wheel | zoom
Middle / space + drag | pan
Ctrl+Z / Ctrl+Y | undo / redo
Ctrl+Shift+Z | whole stroke
Shift+right click / menu key | context menu"
            },
            new GuideTopic
            {
                Key = "cgimenu", Group = "CGI-Ansicht|CGI view", TitleDe = "CGI-Kontextmenü", TitleEn = "CGI context menu",
                De = @"# CGI-Kontextmenü
Rechtsklick mit Auswahl, Bereich, Löschen, Färben, Multi, Vorlage oder Höhlen-Stempel – bei allen anderen Werkzeugen Umschalt+Rechtsklick (dort nimmt der normale Rechtsklick Land oder Item auf). Auch die Menütaste öffnet es.
- Oben: das Static und das Land unter der Maus – Item / Land übernehmen, Static auswählen oder löschen.
- Werkzeug: alle Werkzeuge der Palette.
- Ansicht: Statics ein / aus, Raster, Höhen, Beschriftung, 2D, Wasser durchsichtig, Probleme, Static-IDs, Zoom.
- Bereich: alle Befehle für den markierten Bereich (füllen, Küste, Küste glätten, Wasser, glätten, Berg, Übergänge, kopieren, Farb-Stempel …).
- Küste: glätten im Bereich, ganze Karte, nur Fehlerstellen, Fehler anzeigen.
- Auswahl (wenn Statics gewählt sind): drehen, spiegeln, auf den Boden, färben, als Streu-Set speichern, löschen.
- Bearbeiten: Bereich drehen / spiegeln / auf den Boden, Suchen & Ersetzen, Lasso, Zauberstab, Messen.
- Ansicht-Filter, Lesezeichen, Verlauf.
- Karte: bauen, laden, speichern, Karte → Farbkarte, Stelle im Karten-Editor zeigen.
- Rückgängig / Wiederholen, Hilfe.",
                En = @"# CGI context menu
Right click with Select, Area, Delete, Hue, Multi, Template or Cave stamp – with all other tools Shift+right click (there the plain right click takes land or the item). The menu key opens it as well.
- Top: the static and the land under the mouse – take item / land, select or delete the static.
- Tool: all tools of the palette.
- View: statics on / off, grid, heights, labels, 2D, translucent water, problems, static ids, zoom.
- Area: all commands for the marked area (fill, coast, smooth coast, water, smooth, mountain, transitions, copy, colour stamp …).
- Coast: smooth in the area, whole map, only error places, show errors.
- Selection (when statics are chosen): rotate, mirror, to the ground, hue, save as scatter set, delete.
- Edit: rotate / mirror the area, to the ground, search & replace, lasso, magic wand, measure.
- View filter, bookmarks, history.
- Map: build, load, save, map → colour map, show the place in the map editor.
- Undo / redo, help."
            },
            new GuideTopic
            {
                Key = "area", Group = "CGI-Ansicht|CGI view", TitleDe = "Bereich-Befehle", TitleEn = "Area commands",
                De = @"# Bereich-Befehle
Werkzeug „Bereich“: Rechteck aufziehen, dann eine Taste (oder Kontextmenü → Bereich):
F | mit dem Land-Tile füllen
K | Küste anpassen (klassisch wie das Original)
J | Küste glätten (ohne Zacken, Statics gehen mit)
W | Wasser-Statics setzen, Umschalt+W = entfernen
G | Höhen glätten
M | Berg formen (Reiter Berg)
T | Übergänge neu berechnen
L | Bereich auswerten (Log + Zwischenablage)
P | als Farb-Stempel speichern
S | Statics darin auswählen
R | Bereich drehen 90° (Land + Statics)
X / Y | Bereich spiegeln links ↔ rechts / oben ↕ unten
E | Suchen & Ersetzen
D | Statics auf den Boden setzen
Bild↑ / Bild↓ | Land + Statics heben / senken (Umschalt = 5)
Strg+C / Strg+V | Land + Statics kopieren / einfügen
Entf | Statics im Bereich löschen
Esc | Markierung weg",
                En = @"# Area commands
Tool “Area”: drag a rectangle, then press a key (or context menu → Area):
F | fill with the land tile
K | shape the coast (classic, like the original)
J | smooth the coast (no teeth, statics go along)
W | set water statics, Shift+W = remove
G | smooth heights
M | form a mountain (tab Mountain)
T | compute transitions again
L | analyse the area (log + clipboard)
P | save as colour stamp
S | select the statics inside
R | rotate the area 90° (land + statics)
X / Y | mirror the area left ↔ right / top ↕ bottom
E | search & replace
D | put the statics on the ground
Page↑ / Page↓ | raise / lower land + statics (Shift = 5)
Ctrl+C / Ctrl+V | copy / paste land + statics
Del | delete the statics in the area
Esc | remove the mark"
            },
            new GuideTopic
            {
                Key = "coast", Group = "CGI-Ansicht|CGI view", TitleDe = "Küste und Strand", TitleEn = "Coast and beach",
                De = @"# Küste und Strand
## Woher kommen die Zacken?
Eine schräge oder unruhige Küste ist in der Farbkarte eine Treppe aus Land- und Wasser-Zellen. Mit dem klassischen Profil liegt der Strand auf -3 und direkt daneben der Meeresgrund auf -15. Jedes Strand-Tile bekommt dann eine Ecke bis -15 gezogen, und der Client zeichnet eine Reihe von Sand-Zacken.
## Küste glätten
Die Höhen folgen dem echten Abstand zur Küstenlinie statt den Zell-Stufen. Der Strand sinkt weich bis zur Wasserlinie, der Meeresgrund fällt über einige Tiles auf seine Tiefe. Die Tiefe bleibt also, sie verschiebt sich nur mit der Küste.
- Pinsel „Küste glätten“ (Palette): rund, Größe oben in der Optionsleiste. Zum Überarbeiten von Hand.
- Bereich markieren + J: der ganze Bereich.
- Ganze Karte glätten: alle Küsten (dauert bei großen Karten etwas).
- Nur Fehlerstellen: sucht Zacken, Spitzen und 1-Tile-Buchten und glättet nur dort (Umkreis einstellbar).
- Fehler anzeigen: markiert die Stellen rot und listet sie unter Prüfen → Probleme.
## Einstellungen (Reiter Küste / Wasser)
Abfall-Breite | Tiles von der Wasserlinie bis zur vollen Tiefe (Standard 3).
Weichheit | Glättungs-Durchgänge (1 = keine Zacken an schrägen Küsten).
Küstenlinie begradigen | entfernt einzelne Land-Spitzen und 1-Tile-Buchten (0 = Land / Wasser bleibt).
Statics mitnehmen | Statics gehen mit dem Boden; Wasser-Statics bleiben auf der Wasserhöhe; neues Wasser bekommt Wasser-Statics.
Fehler: Höhensprung ab | ab diesem Abfall zum Wasser gilt ein Strand-Tile als Zacke.
## Beim Bauen
Build-Optionen → 6 Küste → „Küste weich (ohne Zacken)“: der Build macht dasselbe für die ganze Karte.
## Klassisch
Pinsel „Küste“ und Bereich + K formen die Küste wie die Original-Karte: Meeresgrund 0064 auf -15, Wasser-Statics auf -5, Strand -3 / -1 / 0. Klippen über „Klippe ab z“ bleiben.",
                En = @"# Coast and beach
## Where do the teeth come from?
A diagonal or ragged coast is a staircase of land and water cells in the colour map. With the classic profile the beach lies at -3 and the sea floor right next to it at -15. Every beach tile then gets one corner pulled down to -15, and the client draws a row of sand teeth.
## Smooth coast
The heights follow the real distance to the shore line instead of the cell steps. The beach sinks softly to the waterline, and the sea floor drops over a few tiles to its depth. So the depth stays, it only moves with the coast.
- Brush “Smooth coast” (palette): round, size at the top in the options bar. For reworking by hand.
- Mark an area + J: the whole area.
- Smooth whole map: every coast (takes a moment on big maps).
- Only error places: finds teeth, spikes and 1-tile notches and smooths only there (radius adjustable).
- Show errors: marks the places red and lists them under Check → Problems.
## Settings (tab Coast / water)
Drop width | tiles from the waterline to the full depth (default 3).
Softness | smoothing passes (1 = no teeth on diagonal coasts).
Straighten shore line | removes single land spikes and 1-tile notches (0 = land / water stay).
Move statics | statics go with the ground; water statics stay at the water height; new water gets water statics.
Error: height drop from | from this drop towards the water a beach tile counts as a tooth.
## When building
Build options → 6 Coast → “Soft coast (no teeth)”: the build does the same for the whole map.
## Classic
Brush “Coast” and area + K shape the coast like the original map: sea floor 0064 at -15, water statics at -5, beach -3 / -1 / 0. Cliffs above “cliff from z” stay."
            },
            new GuideTopic
            {
                Key = "caves", Group = "CGI-Ansicht|CGI view", TitleDe = "Höhlen und Berge", TitleEn = "Caves and mountains",
                De = @"# Höhlen und Berge
Höhle | am Eingang anfangen und in den Berg ziehen: Boden auf einer Höhe, der Rand wird Wand.
Eingang | am Bergfuß klicken / ziehen: schwarzer Mund 01DB oder unsichtbar 0002, Rand 01D3–01DA automatisch.
Höhlen-Stempel | Klick vor eine Bergwand (Süd- oder Ostseite): ganzer Eingang. Ziehen = Breite + Höhe, Strg+Klick = entfernen.
Berg | ab der Bodenhöhe aufbauen; Bereich + M = ganzer Berg.
- Karten-Editor: Werkzeug „Höhleneingang“ (Farben, Gruppe 1d) und Höhlen-Farb-Stempel.
- NODRAW-Statics auf der Mund-Reihe setzt der Build automatisch (Build-Optionen).",
                En = @"# Caves and mountains
Cave | start at the entrance and drag into the mountain: floor at one height, the edge becomes wall.
Entrance | click / drag at the foot of the mountain: black mouth 01DB or invisible 0002, edge 01D3–01DA automatically.
Cave stamp | click in front of a mountain face (south or east side): a complete entrance. Drag = width + height, Ctrl+click = remove.
Mountain | build up from the ground height; area + M = a whole mountain.
- Map editor: tool “Cave entrance” (colours, group 1d) and cave colour stamps.
- NODRAW statics on the mouth row are set by the build (build options)."
            },
            new GuideTopic
            {
                Key = "convert", Group = "CGI-Ansicht|CGI view", TitleDe = "CGI ↔ Farbkarte", TitleEn = "CGI ↔ colour map",
                De = @"# CGI ↔ Farbkarte
▣ Bereich → Farb-Stempel | Land-Tiles + Höhen + Statics als Stempel (Taste P).
▧ Bereich → in Farbkarte | an derselben Stelle in die offene Farbkarte schreiben.
▤ Karte → Farbkarte | ganze CGI-Karte als Farbkarte + Statics-Ebene (Nachbauen, Fehlersuche).
✦ Regeln lernen | Übergänge, Items, Static-Übergänge aus dem Bereich lernen.
⟳ CGI aktualisieren | (Karten-Editor) Farbkarte neu bauen, ohne die übrige CGI-Karte zu verlieren.",
                En = @"# CGI ↔ colour map
▣ Area → colour stamp | land tiles + heights + statics as a stamp (key P).
▧ Area → into colour map | write it at the same place into the open colour map.
▤ Map → colour map | the whole CGI map as colour map + statics layer (rebuild, debugging).
✦ Learn rules | learn transitions, items, static transitions from the area.
⟳ Refresh CGI | (map editor) build the colour map again without losing the rest of the CGI map."
            },
            new GuideTopic
            {
                Key = "brushes", Group = "CGI-Ansicht|CGI view", TitleDe = "Pinsel & Bearbeiten (neu)", TitleEn = "Brushes & editing (new)",
                De = @"# Pinsel & Bearbeiten
Alle Werkzeuge stehen in der Palette links, die Einstellungen im Reiter „Pinsel“ (Kategorie PINSEL rechts). Ein Zug mit einem Pinsel ist immer ein Rückgängig-Schritt.
## Statics
Streu-Pinsel | verteilt ein Set: Dichte, Mindestabstand, nur auf bestimmten Land-Tiles, nicht auf Wasser / Statics / steilen Hängen, zufällige Hues. Strg+Ziehen entfernt die Statics des Sets.
Sets | alle Item-Dateien der Dragon-Skripte (forest, jungle, swamp, snowforest …) – Bäume mit Blättern usw. wie beim Build. Eigene Sets: Statics auswählen → „Set aus Auswahl: ein Objekt“ (z. B. ein Baum aus mehreren Teilen) oder „jedes Static einzeln“.
Hue-Pinsel | färbt alle Statics unter dem Pinsel mit der Hue aus dem Reiter Items, Strg = Hue 0.
Wand / Zaun | von einem Ende zum anderen ziehen (West-Ost oder Nord-Süd). Wand-Set: im Reiter Items das Stück wählen und „← West-Ost“, „← Nord-Süd“, „← Pfosten“ klicken. Stockwerke, gleiche Höhe, alte Statics ersetzen.
## Gelände
Fluss | Bett mit Wasser-Tiles auf „Tiefe“ unter dem Startpunkt, Ufer steigt weich an, danach Übergänge. „Spiegel folgt dem Gelände“ für Bäche am Hang.
Weg / Straße | Punkte klicken – eine weiche Kurve mit Breite; Höhe geglättet oder gleichmäßig; Bankett gleicht den Rand an. Doppelklick, Rechtsklick oder Enter = bauen, Rücktaste = letzten Punkt weg, Esc = abbrechen.
Rampe | von unten nach oben ziehen: gleichmäßige Steigung (Strg = bis „Höhe =“). Breite = Pinsel.
Plateau / Terrassen | abflachen auf die Höhe unter dem ersten Klick (oder „Höhe =“) oder Höhen auf Stufen runden.
Unebenheit | leichte Wellen (± Stärke, Größe), jedes Feld einmal pro Zug.
Varianten | mischt die Tiles derselben Farbe der Farbtabelle neu – keine Kachel-Muster.
Übergänge-Pinsel | berechnet die Dragon-Übergänge nur unter dem Pinsel.
Härte | Höhe +/−, Glätten, Plateau, Unebenheit: unter 100 % wird der Rand weich.
## Auswählen und bearbeiten
Auswahl | Rahmen auf freiem Boden ziehen wählt alle Statics darin (Strg = dazu). Ausgewählte Statics mit der Maus ziehen = verschieben.
Alt+Klick | Pipette bei jedem Werkzeug: Item unter der Maus, sonst Land-Tile + Höhe.
Lasso | Punkte um eine Fläche klicken, Doppelklick / Enter schließt. Umschalt = dazu, Strg = weg.
Zauberstab | Klick wählt zusammenhängendes gleiches Land (Tile, Familie = Farbe der Farbtabelle, oder Höhe; Höhen-Toleranz).
Maske | Solange Lasso oder Zauberstab aktiv sind (türkis), wirken ALLE Werkzeuge und Bereich-Befehle nur darin. Esc hebt auf.
R / X / Y | Bereich drehen 90° / spiegeln links ↔ rechts / oben ↕ unten (Land + Statics). Mit Auswahl: R und X drehen / spiegeln die Statics.
E | Suchen & Ersetzen: Statics-IDs (auch nur eine Hue, neue Hue, leer = löschen) und Land-Tiles, im Bereich / Lasso oder auf der ganzen Karte.
D | Statics auf den Boden setzen (Auswahl oder Bereich) – nach dem Ändern des Landes.
Messen | ziehen: Abstand, Schritte, Δz und Steigung (z pro Tile).
## Komfort
- Ansicht-Filter: Bäume / Pflanzen, Wände, Dächer, Wasser, Böden, Sonstige ausblenden (Reiter Pinsel oder Kontextmenü). Ausgeblendetes lässt sich nicht anklicken.
- Lesezeichen: Stellen merken (Kontextmenü, Reiter Pinsel) und hinspringen – in der CGI-Ansicht oder im Karten-Editor.
- Verlauf: Fenster mit allen Schritten, Klick geht dorthin zurück oder wieder vor.
## Hinweise
- Beim Drehen eines nicht quadratischen Bereichs bleiben Reste am alten Platz – am besten quadratisch markieren.
- Die Bilder der Statics drehen nicht mit (eine Südwand bleibt eine Südwand).",
                En = @"# Brushes & editing
All tools are in the palette on the left, the settings in the tab “Brushes” (category BRUSHES on the right). One stroke of a brush is always one undo step.
## Statics
Scatter brush | spreads a set: density, minimum distance, only on certain land tiles, not on water / statics / steep slopes, random hues. Ctrl+drag removes the statics of the set.
Sets | every item file of the Dragon scripts (forest, jungle, swamp, snowforest …) – trees with leaves etc. like the build. Own sets: select statics → “Set from selection: one object” (e.g. a tree of several parts) or “every static on its own”.
Hue brush | colours all statics under the brush with the hue of the tab Items, Ctrl = hue 0.
Wall / fence | drag from one end to the other (west – east or north – south). Wall set: choose the piece in the tab Items and click “← West-East”, “← North-South”, “← Post”. Levels, same height, replace old statics.
## Terrain
River | bed of water tiles at “depth” below the start, the bank rises softly, transitions afterwards. “Level follows terrain” for brooks on slopes.
Road | click points – a smooth curve with width; height smoothed or even; the shoulder blends the edge. Double click, right click or Enter = build, Backspace = remove the last point, Esc = cancel.
Ramp | drag from bottom to top: even slope (Ctrl = up to “Height =”). Width = brush.
Plateau / terraces | flatten to the height under the first click (or “Height =”) or round heights to steps.
Noise | slight waves (± strength, size), every cell once per stroke.
Variants | mixes the tiles of the same colour of the colour table again – no tile patterns.
Transition brush | computes the Dragon transitions only under the brush.
Hardness | height +/−, smooth, plateau, noise: below 100 % the edge becomes soft.
## Select and edit
Select | drag a frame on empty ground to select all statics inside (Ctrl = add). Drag selected statics with the mouse = move.
Alt+click | pipette with every tool: the item under the mouse, otherwise land tile + height.
Lasso | click points around an area, double click / Enter closes. Shift = add, Ctrl = remove.
Magic wand | click selects connected equal land (tile, family = colour of the colour table, or height; height tolerance).
Mask | While lasso or magic wand are active (cyan), ALL tools and area commands work only inside. Esc removes it.
R / X / Y | rotate the area 90° / mirror left ↔ right / top ↕ bottom (land + statics). With a selection: R and X rotate / mirror the statics.
E | Search & replace: static ids (also only one hue, new hue, empty = delete) and land tiles, in the area / lasso or on the whole map.
D | put statics on the ground (selection or area) – after the land was changed.
Measure | drag: distance, steps, Δz and slope (z per tile).
## Comfort
- View filter: hide trees / plants, walls, roofs, water, floors, other (tab Brushes or context menu). Hidden statics cannot be clicked.
- Bookmarks: remember places (context menu, tab Brushes) and jump there – in the CGI view or in the map editor.
- History: window with all steps, a click goes back (or forward) to there.
## Notes
- Rotating a non-square area leaves rests at the old place – better mark a square.
- The pictures of the statics do not turn (a south wall stays a south wall)."
            },
            new GuideTopic
            {
                Key = "shapes", Group = "Karten-Editor|Map editor", TitleDe = "Formen, Neigung, natürlicher Lauf, Dungeon", TitleEn = "Shapes, slope, natural course, dungeon",
                De = @"# Formen im Karten-Editor
Alle Formen malen mit der gewählten Farbe (linke Maus = Farbe 1, rechte = Farbe 2) und beachten „Höhe behalten“.
Linie, Rechteck, Ellipse | wie bisher (Dicke = Pinselgröße), auch gefüllt.
Halbkreis | von der Mitte der geraden Seite zur Rundung ziehen – die Richtung bestimmt, wohin der Bogen zeigt. „Gefüllt“ oben, sonst nur der Rand.
Dreieck | von der Mitte der Grundseite zur Spitze ziehen. „Gefüllt“ oder Rand.
Neigungs-Linie | eine Linie mit Pinselbreite, deren Höhe von der Höhe am Start zur Höhe am Ende läuft: jede Zelle bekommt die Farbe der Gruppe von Farbe 1 in der passenden Höhe – fehlt sie, merkt sich die Höhen-Ebene die Höhe. Für Rampen, Straßen und Flüsse am Hang.
Natürlicher Lauf | von A (Quelle) nach B (Mündung) ziehen: ein geschlängelter Lauf mit Pinselgröße, zum Ende breiter. „mit Muster-Stempel (Statics)“ malt ihn mit dem gewählten Muster-Pinsel (Land + Statics). „Schlängeln …“: wie stark, wie viele Bögen, Breite wächst. Die Vorschau zeigt genau den Lauf, der gemalt wird.
Dungeon | Rechteck aufziehen: Räume und Gänge, Farbe 1 = Boden (z. B. Höhlenboden), Farbe 2 = Wand / Fels. „Dungeon-Einstellungen …“: Räume, Größen, Gang-Breite, Schleifen, höhlige Ränder, Zufalls-Zahl, Fels ganz füllen.
## Tipps
- Fluss: Farbe 1 = Wasser, Pinsel 4–8, „Natürlicher Lauf“ – danach Küste glätten in der CGI-Ansicht.
- Straße am Hang: Farbe 1 = Weg, „Neigungs-Linie“ vom Tal zum Pass.
- Dungeon-Bereich der Karte (Felucca / Trammel x > 5119): mit Schwarz füllen, dann Dungeon aufziehen.",
                En = @"# Shapes in the map editor
All shapes paint with the chosen colour (left mouse = colour 1, right = colour 2) and respect “Keep height”.
Line, rectangle, ellipse | as before (thickness = brush size), also filled.
Half circle | drag from the middle of the straight side to the round side – the direction decides where the arc points. “Filled” at the top, otherwise only the outline.
Triangle | drag from the middle of the base to the tip. “Filled” or outline.
Slope line | a line with brush width whose height runs from the height at the start to the height at the end: every cell gets the colour of the group of colour 1 at the matching height – if it is missing, the height layer remembers the height. For ramps, roads and rivers on slopes.
Natural course | drag from A (source) to B (mouth): a winding course with the brush size, wider towards the end. “with pattern stamp (statics)” paints it with the chosen pattern brush (land + statics). “Meander …”: how strong, how many bends, width grows. The preview shows exactly the course that is painted.
Dungeon | drag a rectangle: rooms and corridors, colour 1 = floor (e.g. cave floor), colour 2 = wall / rock. “Dungeon settings …”: rooms, sizes, corridor width, loops, rough edges, random number, fill all rock.
## Tips
- River: colour 1 = water, brush 4–8, “Natural course” – then smooth the coast in the CGI view.
- Road on a slope: colour 1 = road, “Slope line” from the valley to the pass.
- Dungeon area of the map (Felucca / Trammel x > 5119): fill with black, then drag the dungeon."
            },
            new GuideTopic
            {
                Key = "build", Group = "CGI-Ansicht|CGI view", TitleDe = "Bauen: Brücke, Keller, unter der Karte, Dungeon", TitleEn = "Building: bridge, cellar, under the map, dungeon",
                De = @"# Bauen in der CGI-Ansicht
Die Werkzeuge stehen in der Palette (Gruppe Bauen), die Einstellungen im Reiter Pinsel. Jeder Bau ist ein Rückgängig-Schritt.
## Brücke
Von Ufer zu Ufer ziehen (West-Ost oder Nord-Süd). Bretter auf der ganzen Breite, Geländer an beiden Seiten, Pfosten an den Enden, die Mitte um „Bogen“ höher. Höhe = höheres Ende + „Z +“.
Brücken-Set | „Neu …“, dann im Reiter Items das Stück wählen und „← Brett W-O“, „← Brett N-S“, „← Geländer …“, „← Pfosten“ klicken. Mehrere IDs werden gemischt.
## Keller
Rechteck aufziehen. So entsteht ein begehbarer Keller wie auf Freeshards:
- Das Land im Rechteck wird zum unsichtbaren Tile NoDraw (0002) auf Boden − Tiefe (Standard 20) – der Client zeichnet dann, was darunter liegt.
- Der Rand bekommt Wände (Wand-Set, Name unter „Wände“), innen liegt der Kellerboden (Statics).
- Eine Treppe führt von der Bodenhöhe in Schritten zu 5 Z hinunter, in die Richtung „Treppe hinunter nach“ (Nord, Ost, Süd, West), so breit wie eingestellt.
- „Boden oben“ legt den Hausboden auf Bodenhöhe darüber (außer über der Treppe).
Treppen-Set | je Richtung die passende Stufe: „← ab Nord“ usw. mit dem gewählten Item.
## Unter der Karte bauen
Schalter in der Palette, in der Leiste (⬇ Unter der Karte), im Reiter Pinsel und unter Ansicht:
- Statics, die unter dem Boden liegen, werden über dem Land gezeigt – man sieht, was man unten baut.
- Ein violettes Banner oben zeigt: man baut gerade unter der Karte, mit der Bau-Ebene.
- Static setzen und Wand bauen setzen auf Boden + Bau-Ebene (Standard −20).
## Dungeon-Werkstatt (eigenes Fenster)
Platz mit „Bereich“ markieren (zum Markieren großer Plätze weit herauszoomen – unter 1:4 zeigt die Ansicht eine schnelle Farb-Übersicht), dann Palette → Dungeon-Werkstatt oder Keller (Werkstatt), oder Kontextmenü → Bereich.
- Alles geschieht erst in einer Arbeitskopie: OBEN die Karte, UNTEN die gewählte Etage (Statics unter dem Boden sichtbar, darüber ausgeblendet). Mit den Werkzeugen der Leiste (Auswahl, Static setzen, Löschen, Wand, Loch, Land setzen, Messen) in beiden Ansichten weiterbauen.
- Art: Klassischer Dungeon (Boden-Textur + Wand-Statics, Stil), Keller (nur Statics unter der Karte, Loch auf Kartenhöhe), Berg-Dungeon (Fels-Wände im Gitter), Bergwall (Fels-Labyrinth).
- Etage: Regler in 20er-Schritten von der ermittelten Kartenhöhe (0, −20, −40 … und nach oben) oder z genau.
- Leere außen: NoDraw 0002, Schwarz 0244 oder „Wählen …“ aus dem Land-Katalog (z. B. Schwarz mit Sternen).
- Generieren leert den Platz zuerst (Statics weg, Land = Leere auf seiner Höhe – keine Löcher in die Tiefe) und baut dann. Liegt die Etage unter der Karte, bleibt das Land oben NoDraw und der Dungeon liegt darunter aus Statics.
- 2D-Plan: der Grundriss zum Nachzeichnen (links Boden, rechts Fels, Umschalt Innenwand, Strg Gang) → „Aus Plan bauen“.
- Skript / Stil: der Stil als JSON ansehen, ändern, übernehmen oder als neuen Stil speichern.
- Scannen: einen Dungeon der Karte im Studio markieren → „Bereich scannen + Stil lernen“ (mit Log) oder „1:1 hierher kopieren“.
- Rückgängig / Wiederholen (auch Strg+Z / Strg+Y), Alles zurück, Verschieben N S W O, Höhe ±1 / ±5 / ± Etage, Schnell löschen (Statics, Land, beides).
- „In die Karte übernehmen“ schreibt alles als EINEN Schritt in die Karte (dort Strg+Z).
## Dungeon-Generator
Echte UO-Dungeons (Deceit, Doom, Shame …) bestehen aus Boden-Land mit Dungeon-Textur, schwarzem Void-Land drumherum und Wand-Statics an der Grenze Boden ↔ Leere in Bodenhöhe, dazu Deko (Spinnweben, Knochen, Fackeln). Genau so baut der Generator – nur mit Textur und Statics.
- Stil lernen: auf einer Karte mit Dungeons einen Dungeon mit „Bereich“ markieren (mit etwas schwarzem Rand), Kontextmenü → Bereich → „Dungeon-Stil aus dem Bereich lernen …“ oder Reiter Pinsel → Dungeon. Gelernt werden Boden-Textur, Void-Land, welche Wand-Stapel (id + Höhe) in welcher Lage stehen (Nachbarn N O S W und Außenecken) und die Deko mit Häufigkeit.
- Log: „Dungeon analysieren (Log)“ schreibt alles in eine Datei und in die Zwischenablage: Land und Texturen, Boden-Höhen, jedes Static mit Höhe über dem Boden und Rolle (Wand, Boden, Deko), die Wand-Regeln und einen Plan.
- Rollen im Log: Wand (Wand-Flag oder auf der Leere), Wand-Deko (Fackeln, Bilder, Teppiche – hängen an Wänden), Deko (lose: Schutt, Blut, Knochen, Spinnweben), Einrichtung (Tische, Stühle, Steine, Treppen – wird nicht zufällig gesetzt), Boden.
- UO-Regel (aus map0 gelesen): Nord- und Westwände stehen auf der Leere außen, Süd- und Ostwände auf dem letzten Bodenfeld innen; Wände zwischen zwei Räumen stehen auf Boden-Land (Innenwände) – alles wird gelernt.
- Bauen: Werkzeug Dungeon, Rechteck aufziehen (mindestens 10×10). Grundriss „Gitter“ (Standard, wie Deceit): der Bereich wird in Räume geteilt, eine Wand-Zeile auf Boden-Land dazwischen (Wand-Dicke 1) und eine Tür (Tür-Breite), manchmal eine zweite. Grundriss „Räume + Gänge“: verstreute Räume, verbunden über einen minimalen Spannbaum + „Schleifen“ %, Gänge in L-Form. Mit gelerntem Stil steht jedes Wandstück wie im Original, Fackeln hängen an den Wänden, lose Deko liegt auf dem freien Boden; ohne Stil nimmt er das Wand-Set.
- Unter der Karte | wie ein Keller: NoDraw-Land auf Boden − Tiefe, Statics darunter.
- Zufalls-Zahl 0 = jedes Mal neu; die benutzte Zahl steht in der Statuszeile.
## Bergwall
Rechteck aufziehen: dieselben Räume und Gänge, aber die Wände sind hohes Fels-Land (Berg-Tiles) – ein Labyrinth aus Bergwänden mit unregelmäßigen Rändern (der frühere „Höhle“-Dungeon).
## Fluss-Lauf
Von der Quelle zur Mündung ziehen: geschlängelter Fluss mit den Fluss-Einstellungen (Tiefe, Ufer, Übergänge), zum Ende breiter. Breite am Anfang / Ende in Tiles („Fluss: Breite / Schlängeln …“, 0 = Pinselgröße). Die Mitte des Flusses bleibt immer Wasser (auch nach den Übergängen), „Wasser-Statics dazu“ legt zusätzlich Wasser-Statics darauf.",
                En = @"# Building in the CGI view
The tools are in the palette (group Build), the settings in the tab Brushes. Every build is one undo step.
## Bridge
Drag from bank to bank (west – east or north – south). Planks over the whole width, railings on both sides, posts at the ends, the middle higher by “arch”. Height = higher end + “Z +”.
Bridge set | “New …”, then choose the piece in the tab Items and click “← Plank W-E”, “← Plank N-S”, “← Railing …”, “← Post”. Several ids are mixed.
## Cellar
Drag a rectangle. This makes a walkable cellar like on free shards:
- The land in the rectangle becomes the invisible tile NoDraw (0002) at ground − depth (default 20) – the client then draws what lies below.
- The border gets walls (wall set, name under “Walls”), inside lies the cellar floor (statics).
- Stairs lead from the ground down in steps of 5 Z, in the direction “stairs down towards” (north, east, south, west), as wide as set.
- “Floor above” puts the house floor at ground height on top (except above the stairs).
Stair set | for every direction the matching step: “← down N” etc. with the chosen item.
## Building under the map
Switch in the palette, in the bar (⬇ Under the map), in the tab Brushes and under View:
- Statics below the ground are shown above the land – you see what you build down there.
- A violet banner at the top shows that you are building under the map, with the building level.
- Place static and build wall put things at ground + building level (default −20).
## Dungeon workshop (own window)
Mark a place with “Area” (zoom far out to mark big places – below 1:4 the view shows a fast colour overview), then palette → Dungeon workshop or Cellar (workshop), or context menu → Area.
- Everything happens in a working copy first: ABOVE the map, BELOW the chosen level (statics below the ground visible, above it hidden). Keep building in both views with the tools of the bar (select, place static, delete, wall, hole, set land, measure).
- Kind: classic dungeon (floor texture + wall statics, style), cellar (only statics under the map, hole at map height), mountain dungeon (rock walls in a grid), mountain wall (rock labyrinth).
- Level: slider in steps of 20 from the detected map height (0, −20, −40 … and up) or exact z.
- Void outside: NoDraw 0002, black 0244 or “Choose …” from the land catalogue (e.g. black with stars).
- Generate empties the place first (statics away, land = void at its own height – no holes into the depth) and then builds. If the level is under the map the land above stays NoDraw and the dungeon lies below as statics.
- 2D plan: the floor plan to draw by hand (left floor, right rock, Shift inner wall, Ctrl corridor) → “Build from plan”.
- Script / style: see the style as JSON, change it, apply it or save it as a new style.
- Scan: mark a dungeon of the map in the studio → “Scan area + learn style” (with log) or “Copy 1:1 to here”.
- Undo / redo (also Ctrl+Z / Ctrl+Y), all back, move N S W E, height ±1 / ±5 / ± level, quick delete (statics, land, both).
- “Apply to the map” writes everything as ONE step into the map (Ctrl+Z there).
## Dungeon generator
Real UO dungeons (Deceit, Doom, Shame …) consist of floor land with a dungeon texture, black void land around it and wall statics at the border floor ↔ void at floor height, plus decoration (webs, bones, torches). The generator builds exactly like that – only texture and statics.
- Learn a style: on a map with dungeons mark a dungeon with “Area” (with some black border), context menu → Area → “Learn dungeon style from the area …” or tab Brushes → Dungeon. Learned are the floor texture, the void land, which wall stacks (id + height) stand in which situation (neighbours N E S W and outer corners) and the decoration with its frequency.
- Log: “Analyse dungeon (log)” writes everything into a file and the clipboard: land and textures, floor heights, every static with its height above the floor and its role (wall, floor, decor), the wall rules and a plan.
- Roles in the log: wall (wall flag or on the void), wall decor (torches, paintings, tapestries – hang on walls), decor (loose: garbage, blood, bones, webs), furniture (tables, chairs, stones, stairs – not placed at random), floor.
- UO rule (read from map0): north and west walls stand on the void outside, south and east walls on the last floor cell inside; walls between two rooms stand on floor land (inner walls) – all of it is learned.
- Build: tool Dungeon, drag a rectangle (at least 10×10). Layout “grid” (default, like Deceit): the area is split into rooms, a wall line on floor land between them (wall thickness 1) and a door (door width), sometimes a second one. Layout “rooms + corridors”: scattered rooms, connected by a minimum spanning tree + “loops” %, L-shaped corridors. With a learned style every wall piece stands like in the original, torches hang on the walls, loose decor lies on the free floor; without a style it uses the wall set.
- Under the map | like a cellar: NoDraw land at ground − depth, statics below.
- Random number 0 = new every time; the number used is shown in the status line.
## Mountain wall
Drag a rectangle: the same rooms and corridors, but the walls are high rock land (mountain tiles) – a labyrinth of mountain walls with rough edges (the former “cave” dungeon).
## River course
Drag from the source to the mouth: winding river with the river settings (depth, banks, transitions), wider towards the end. Width at the start / end in tiles (“River: width / winding …”, 0 = brush size). The middle of the river always stays water (also after the transitions), “Add water statics” puts water statics on it as well."
            },
            new GuideTopic
            {
                Key = "bridge", Group = "Bauen|Building", TitleDe = "Brücken und Brücken-Designer", TitleEn = "Bridges and bridge designer",
                De = @"# Brücken aus Statics
Eine Brücke besteht nur aus Statics: Bretter (Boden), Geländer an beiden Seiten, Pfosten, Laternen, Pfeiler bis zum Wasser und Treppen an den Enden.

## Schnell: Werkzeug Brücke
- Palette → Brücke, von Ufer zu Ufer ziehen (West-Ost oder Nord-Süd). Beim Ziehen sieht man die echten Teile.
- Ohne Plan und ohne Set sucht das Studio die Teile selbst in der TileData (Namen wie bridge, plank, railing, post, pillar, stairs) und erkennt am Bild, ob ein Geländer West-Ost oder Nord-Süd läuft.
- Höhe = höheres Ufer + „Z +“, die Mitte um „Bogen“ höher. „Treppen an den Enden“ setzt Stufen, wenn die Brücke höher liegt als das Ufer.

## Brücken-Designer (eigenes Fenster)
- 1 Plan: gespeicherte Pläne nach Kategorie (Holz, Stein, Dungeon …), Speichern, Löschen.
- 2 Teile: Knopf (Bretter, Geländer W-O, Geländer N-S, Pfosten, Pfeiler, Lampe, Treppe N/O/S/W) zeigt unten im Katalog alle passenden Teile der TileData als Bild – Doppelklick ordnet zu. „Aus Teilen erzeugen“ baut den Plan neu.
- 3 Testfeld: zwei Ufer und Wasser. „Muster“ = die Brücke einmal, flach, zum Bearbeiten; „Vorschau lang“ = so lang wie eingestellt, mit Bogen, Pfeilern und Treppen; „W-O / N-S“ dreht.
- 4 Von Hand: im Muster mit Katalog + Static setzen / Löschen / Radierer bauen, dann „Muster übernehmen“. „Andere Richtung ableiten“ macht aus West-Ost die Nord-Süd-Version. „Brücke aus der Karte“ übernimmt eine echte Brücke (in der CGI-Ansicht mit Bereich markiert). „Multi laden“ macht eine Multi zum Plan.
- Anfang / Ende (Plan): so viele Tiles am Anfang und Ende werden einmal gebaut, die Mitte wird bei langen Brücken wiederholt (z. B. Pfosten alle 2 Tiles, Laterne alle 6).
- 5 „Im Studio benutzen“: der Plan wird im Brücken-Werkzeug gewählt (Reiter Pinsel → Brücke → Plan).",
                En = @"# Bridges of statics
A bridge consists of statics only: planks (deck), railings on both sides, posts, lamps, pillars down to the water and stairs at the ends.

## Quick: tool Bridge
- Palette → Bridge, drag from bank to bank (west – east or north – south). While dragging you see the real pieces.
- Without a plan and without a set the studio finds the pieces in the tiledata (names like bridge, plank, railing, post, pillar, stairs) and tells from the art whether a railing runs west – east or north – south.
- Height = higher bank + “Z +”, the middle higher by “Arch”. “Stairs at the ends” places steps when the bridge lies higher than the bank.

## Bridge designer (own window)
- 1 Plan: saved plans by category (wood, stone, dungeon …), save, delete.
- 2 Pieces: a button (planks, railing W-E, railing N-S, post, pillar, lamp, stairs N/E/S/W) shows all fitting pieces of the tiledata as pictures in the catalog below – double click assigns. “Generate from pieces” rebuilds the plan.
- 3 Test field: two banks and water. “Sample” = the bridge once, flat, for editing; “Long preview” = as long as set, with arch, pillars and stairs; “W-E / N-S” turns.
- 4 By hand: build in the sample with catalog + place static / delete / eraser, then “Take sample”. “Derive other direction” makes the north – south version. “Bridge from the map” takes a real bridge (marked with Area in the CGI view). “Load multi” turns a multi into a plan.
- Start / end (plan): these tiles are built once, the middle repeats on long bridges (e.g. a post every 2 tiles, a lamp every 6).
- 5 “Use in studio”: the plan is chosen in the bridge tool (tab Brushes → Bridge → Plan)."
            },
            new GuideTopic
            {
                Key = "ws-generate", Group = "Bauen|Building", TitleDe = "Werkstatt: Generieren", TitleEn = "Workshop: generate",
                De = @"# Dungeon-Werkstatt: Generieren
- 1. In der CGI-Ansicht mit „Bereich“ den Platz markieren (gern großzügig), dann Palette → Dungeon-Werkstatt (oder Keller).
- 2. Art wählen: Klassischer Dungeon (nur Boden-Textur + Wand-Statics), Keller (nur Statics unter der Karte), Berg-Dungeon, Bergwall.
- 3. Etage: Regler 0 / −20 / −40 … oder nach oben, „z genau“. Unter der Karte bleibt das Land oben NoDraw auf Kartenhöhe, gebaut wird darunter.
- 4. Leere außen: NoDraw (0002), Schwarz (0244) oder Wählen (z. B. Schwarz mit Sternen).
- 5. Stil: „Klassisch (map0-Log)“ oder ein gelernter Stil (Bereich scannen + Stil lernen).
- 6. Grundriss: Räume + Gänge oder Gitter, Raumgröße min / max, Türbreite, Zufalls-Zahl.
- 7. ▶ Generieren: der Platz wird geleert und neu gebaut. 🎲 = neue Zufalls-Zahl. Rückgängig / Wiederholen, „Alles zurück“.
- 8. ✔ In die Karte übernehmen – ein Schritt, in der Karte mit Strg+Z zurück.

## Vorlagen
- Fremde Karte öffnen: eine andere map#.mul (z. B. Original-Felucca) im eigenen Fenster, zum Dungeon springen, Bereich markieren, „Bereich in die Werkstatt“.
- Platz als Vorlage speichern (Kategorie + Name) und später mit „Vorlage laden“ auf jeder Etage wieder einsetzen.",
                En = @"# Dungeon workshop: generate
- 1. Mark the place with “Area” in the CGI view (with some margin), then palette → Dungeon workshop (or Cellar).
- 2. Choose the kind: classic dungeon (floor texture + wall statics only), cellar (statics under the map only), mountain dungeon, mountain wall.
- 3. Level: slider 0 / −20 / −40 … or upwards, “exact z”. Under the map the land above stays NoDraw at map height, the building is below.
- 4. Void outside: NoDraw (0002), black (0244) or choose (e.g. black with stars).
- 5. Style: “Classic (map0 log)” or a learned style (scan area + learn style).
- 6. Layout: rooms + corridors or grid, room size min / max, door width, random number.
- 7. ▶ Generate: the place is cleared and built anew. 🎲 = new random number. Undo / redo, “Reset all”.
- 8. ✔ Apply to map – one step, Ctrl+Z in the map undoes it.

## Templates
- Open other map: another map#.mul (e.g. original Felucca) in its own window, jump to the dungeon, mark an area, “Area into workshop”.
- Save place as template (category + name) and insert it later on any level with “Load template”."
            },
            new GuideTopic
            {
                Key = "ws-2d", Group = "Bauen|Building", TitleDe = "Werkstatt: 2D-Plan", TitleEn = "Workshop: 2D plan",
                De = @"# Dungeon-Werkstatt: im 2D-Plan bauen
Der Reiter „2D-Plan“ zeigt den Grundriss von oben: jedes Kästchen ist ein Tile.

Links | Raum (Boden)
Rechts | Fels / Leere
Umschalt + Links | Innenwand (Wandlinie auf dem Boden zwischen zwei Räumen)
Strg + Links | Gang
Pinsel 1–3 | Größe des Stifts

- „Leerer Plan“ beginnt ohne Räume, „Neu generieren“ macht einen zufälligen Plan zum Nacharbeiten.
- „Aus Plan bauen“ baut den Plan im gewählten Stil: Boden-Textur, Wände an den Rändern (Nord / West auf der Leere, Süd / Ost auf dem letzten Boden-Tile wie im Original), Ecken, Innenwände, Deko.
- Räume vergrößern: einfach mit Links weitermalen und neu bauen.",
                En = @"# Dungeon workshop: build in the 2D plan
The tab “2D plan” shows the layout from above: every square is a tile.

Left | room (floor)
Right | rock / void
Shift + left | inner wall (wall line on the floor between two rooms)
Ctrl + left | corridor
Brush 1–3 | pen size

- “Empty plan” starts without rooms, “Generate new” makes a random plan to rework.
- “Build from plan” builds the plan in the chosen style: floor texture, walls at the edges (north / west on the void, south / east on the last floor tile like the original), corners, inner walls, decoration.
- Enlarge rooms: simply paint on with left and build again."
            },
            new GuideTopic
            {
                Key = "ws-3d", Group = "Bauen|Building", TitleDe = "Werkstatt: 3D bauen (Ansichten, Katalog)", TitleEn = "Workshop: build in 3D (views, catalog)",
                De = @"# Dungeon-Werkstatt: in den CGI-Ansichten bauen
Oben die Karte, unten die gewählte Etage (Statics unter dem Boden sichtbar, darüber ausgeblendet). Beide Ansichten bearbeiten dieselbe Arbeitskopie.

## Werkzeuge (Leiste über den Ansichten)
Auswahl / Bereich | wählen, kleineren Bereich für Schnell-Löschen markieren
Static setzen | das Teil aus dem Katalog setzen (unten auf der Etage, oben auf der Karte)
Löschen | Klick löscht ein Static
Radierer | Pinsel löscht alle Statics darunter – Größe mit „Pinsel“ (1 … 17); Strg = Land zur Leere
Wand / Zaun | von Ende zu Ende ziehen (Wand-Set)
Loch (NoDraw) | Land unsichtbar auf seiner Höhe – einzeln oder in einem Zug, Größe mit „Pinsel“
Land setzen | Land / Textur aus dem Katalog malen
Höhe + / Höhe − / Rampe / Glätten | Gefälle und Steigungen
Treppe | Klick: Stufen nach unten (R dreht die Richtung, „Treppe …“ = Stufen und Breite)
Multi setzen | Multi aus dem Katalog
Gitter / Statics | Gitter ein / aus, Statics ein- / ausblenden
Katalog | unten ein- / ausblenden

## Katalog
- Statics, Land / Textur oder Multis, Suche nach Name oder Hex-ID.
- Kategorien: eigene (z. B. „Dungeon (map0)“, Rechtsklick → In Kategorie …, + Kategorie) und eingebaute (◆ Wände, Böden, Treppen, Brücke, Geländer, Licht, Dungeon-Deko …).
- Klick wählt das Teil und das passende Werkzeug.",
                En = @"# Dungeon workshop: build in the CGI views
Above the map, below the chosen level (statics under the ground visible, above hidden). Both views edit the same working copy.

## Tools (bar above the views)
Select / Area | select, mark a smaller area for quick clear
Place static | place the piece from the catalog (below on the level, above on the map)
Delete | click deletes a static
Eraser | brush deletes every static under it – size with “Brush” (1 … 17); Ctrl = land to void
Wall / fence | drag from end to end (wall set)
Hole (NoDraw) | land invisible at its height – one by one or in one stroke, size with “Brush”
Set land | paint land / texture from the catalog
Height + / Height − / Ramp / Smooth | slopes
Stairs | click: steps down (R turns the direction, “Stairs …” = steps and width)
Place multi | multi from the catalog
Grid / Statics | grid on / off, show / hide statics
Catalog | show / hide below

## Catalog
- Statics, land / texture or multis, search by name or hex id.
- Categories: own (e.g. “Dungeon (map0)”, right click → Into category …, + Category) and built-in (◆ walls, floors, stairs, bridge, railings, light, dungeon decoration …).
- A click chooses the piece and the fitting tool."
            },
            new GuideTopic
            {
                Key = "tooloptions", Group = "CGI-Ansicht|CGI view", TitleDe = "Werkzeug-Einstellungen, Seitenleiste, Hue", TitleEn = "Tool settings, side panel, hue",
                De = @"# Werkzeug-Einstellungen direkt in der Leiste
Jedes Werkzeug zeigt oben in der Optionsleiste seine wichtigsten Werte – Höhe, Tiefe, Breite – zum direkten Ändern. ⚙ öffnet alle Einstellungen.

Küste | Wasserspiegel, Meeresgrund, Wasser-Breite, höchstes Land, Grund setzen
Küste glätten | Spiegel, Grund, Breite, Abfall-Breite, Glätten, Statics mit
Fluss / Fluss-Lauf | Tiefe, Ufer-Breite, Ufer-Steigung, Grund tiefer (wie Küste: Grund unter den Wasser-Statics), Wasser-Statics; Lauf: Breite Anfang / Ende, Schlängeln
Weg | Weg-Vorlage (gespeicherte Wege), aus Tiles oder mit Stempel (z. B. aus der Karte gelernt), Breite, Bankett, Glättung
Unebenheit | Muster (Wellen, Hügel, Mulden, Grate, Rillen, Stufen, grob), Stärke, Größe, Feinheit, Richtung – mit Vorschau-Bild
Varianten | eigenes Muster (Land-Tiles, „Neu aus Bereich“, „Muster bearbeiten“) oder die Familie der Farbtabelle
Wand | Set, Sets bearbeiten (auch aus der Auswahl), Dicke, Stockwerke, Punkt zu Punkt, gleiche Höhe
Färben / Hue-Pinsel | Hue-Nummer mit Farbfeld und Name, „🎨 Wählen“ (alle Hues mit Farbverlauf, Suche), ☆ merken, ★ Favoriten
Radierer | Statics / Land, nur sichtbare, nur bestimmte IDs („Auswahl merken“)
Multi setzen | ✖ Multi-Auswahl löschen; im Reiter Multis Bild sofort + Katalog mit Suche
Berg / Brücke / Treppe | Gipfel, Steilheit … / Breite, Bogen, Z, Treppen / Richtung, Stufen, Breite

## Seitenleiste und Palette
◧ Werkzeuge (F7) | Werkzeug-Palette links ein / aus
◨ Seite (F8) | Seitenleiste rechts ein / aus
⧉ Seite lösen | Seitenleiste (Items, Multis, Land, Pinsel …) als eigenes Fenster, mit Rückgängig / Wiederholen; Schließen = wieder andocken

## Natürlicher Lauf im Karten-Editor
- Oben „Art“: Fluss, Bach, Berg-Kamm, Weg, Eigene, Stempel. Die Farben werden beim ersten Mal in der Farbtabelle gesucht (Wasser / Sand, Fels, Weg).
- „Lauf-Arten …“: Kern-, Rand- und Außen-Farbe (links Farbe 1 wählen → „Farbe 1 → Kern / Rand / Außen“), Breiten, Schlängeln, Bögen. Neue Arten anlegen, kopieren, löschen.",
                En = @"# Tool settings right in the bar
Every tool shows its most important values – height, depth, width – in the options bar at the top. ⚙ opens all settings.

Coast | water level, sea floor, water width, highest land, set floor
Smooth coast | level, floor, width, drop width, blur, statics along
River / river course | depth, bank width, bank slope, deeper bed (like the coast: bed under the water statics), water statics; course: width start / end, winding
Road | road preset (saved roads), from tiles or with a stamp (e.g. learned from the map), width, shoulder, smoothing
Noise | pattern (waves, hills, hollows, ridges, furrows, steps, rough), strength, size, detail, direction – with preview picture
Variants | own pattern (land tiles, “New from area”, “Edit patterns”) or the family of the colour table
Wall | set, edit sets (also from the selection), thickness, floors, point to point, same height
Hue / hue brush | hue number with colour field and name, “🎨 Choose” (all hues with colour strip, search), ☆ remember, ★ favourites
Eraser | statics / land, only visible, only certain ids (“Remember selection”)
Place multi | ✖ clear multi choice; tab Multis: picture right away + catalog with search
Mountain / bridge / stairs | peak, steepness … / width, arch, Z, stairs / direction, steps, width

## Side panel and palette
◧ Tools (F7) | tool palette on the left on / off
◨ Side (F8) | side panel on the right on / off
⧉ Detach side | side panel (items, multis, land, brushes …) in its own window, with undo / redo; closing docks it back

## Natural course in the map editor
- “Kind” at the top: river, brook, mountain ridge, road, own, stamp. The colours are looked up in the colour table the first time (water / sand, rock, road).
- “Course kinds …”: core, edge and outer colour (choose colour 1 on the left → “Colour 1 → core / edge / outer”), widths, winding, bends. Add, copy, delete kinds."
            },
            new GuideTopic
            {
                Key = "stairs", Group = "Bauen|Building", TitleDe = "Treppen und Treppenhaus", TitleEn = "Stairs and staircase",
                De = @"# Treppen wie im Original
Eine Treppe hat je Lauf 4 Stufen zu 5 Z – zusammen 20 Z = eine Etage. Nach jedem Lauf kommt ein flaches Feld (Absatz), dann der nächste Lauf. Alles ist bis unten gefüllt, die Treppe steht massiv.

Klick | Fuß der Treppe (mit „Nach unten bauen“: oberes Ende, z. B. vom Haus in den Keller)
R | Richtung (hinauf) drehen
Etagen | so viele Läufe übereinander
Wende | gerade weiter, rechts herum, links herum (mehrere Etagen = Wendeltreppe), zurück (U neben dem Lauf)
Breite | so viele Tiles breit, der Absatz wächst mit

## Treppe lernen
- Eine Treppe von Hand bauen (Stufen in mehreren Richtungen, Blöcke darunter, Absatz), alle Stücke auswählen und „Treppe aus Auswahl lernen“: Stufe je Richtung, Füll-Block und Absatz werden erkannt und als Treppen-Set gespeichert.
- Eine fertige Treppe lässt sich in der Werkstatt als Vorlage speichern und überall wieder einsetzen.

## Treppen-Designer (eigenes Fenster)
- Eine Treppe besteht aus viereckigen Blöcken, stufig übereinander (je 5 Z), und oben auf jeder Stufe dem geneigten Treppenstück.
- Im Testfeld von Hand bauen: im Katalog Block / Stufe wählen, „Z +“ (0, 5, 10, 15) einstellen, klicken. Oder „Aus Treppen-Set erzeugen“ und ändern.
- Mit „Bereich“ markieren → „Als Modell übernehmen“ (Richtung „Gebaut hinauf nach“ prüfen), „Gedreht ansehen“ N / O / S / W, speichern nach Kategorie.
- „Treppe aus der Karte“ übernimmt eine echte Treppe. „📋 Log + Bild“ speichert Bild und jede Stufe mit ID, Höhe und Neigung – zum Schicken.
- „Im Studio benutzen“: das Werkzeug Treppe setzt das Modell (R dreht, „Modell mehrmals“, „Nach unten bauen“, unter der Karte mit „Unter der Karte bauen“).

## Brücke mit Bogen aus Stufen
Im Brücken-Designer „Bogen aus Stufen“ und den Regler „Bogen“ (z. B. 20 = 4 Stufen) einstellen: die Brücke steigt in Stufen an, ist oben flach und geht auf der anderen Seite wieder hinunter. Regler auch für Länge und Breite.

## Dungeon mit Etagen
Werkstatt → Grundriss → „Etagen“: so viele Ebenen untereinander (je 20 Z), jede mit eigenem Grundriss, verbunden mit einer Treppe (4 Stufen) in einem Raum, der auf beiden Ebenen frei ist. Über der Treppe bekommt die obere Ebene ein Loch.",
                En = @"# Stairs like the original
A flight has 4 steps of 5 Z – together 20 Z = one floor. After every flight comes a flat field (landing), then the next flight. Everything is filled down to the bottom, the stairs stand solid.

Click | foot of the stairs (with “Build downwards”: top end, e.g. from the house into the cellar)
R | turn the direction (up)
Floors | this many flights on top of each other
Turn | straight on, right, left (several floors = spiral staircase), back (U beside the flight)
Width | this many tiles wide, the landing grows with it

## Learn stairs
- Build a stair by hand (steps in several directions, blocks below, landing), select all pieces and “Learn stairs from selection”: the step per direction, filling block and landing are found and saved as a stair set.
- A finished stair can be saved as a template in the workshop and placed anywhere again.

## Stair designer (own window)
- Stairs are square blocks stepped on top of each other (5 Z each) with the sloped stair piece on every step.
- Build by hand in the test field: choose block / step in the catalog, set “Z +” (0, 5, 10, 15), click. Or “Generate from stair set” and change it.
- Mark with “Area” → “Take as model” (check “Built going up towards”), “View turned” N / E / S / W, save by category.
- “Stairs from the map” takes real stairs. “📋 Log + picture” saves a picture and every step with id, height and slope – to send.
- “Use in the studio”: the tool Stairs places the model (R turns, “Model several times”, “Build downwards”, under the map with “Build under the map”).

## Bridge with an arch of steps
In the bridge designer switch on “Arch of steps” and set the slider “Arch” (e.g. 20 = 4 steps): the bridge rises in steps, is flat on top and goes down on the other side. Sliders for length and width too.

## Dungeon with floors
Workshop → layout → “Floors”: this many levels below each other (20 Z each), every one with its own layout, connected by stairs (4 steps) in a room that is open on both levels. Above the stairs the upper level gets a hole."
            },
            new GuideTopic
            {
                Key = "test", Group = "Anleitung|Guide", TitleDe = "Test-Assistent (Bericht für den Entwickler)", TitleEn = "Test assistant (report for the developer)",
                De = @"# Test-Assistent
Menü Hilfe → 🧪 Test-Assistent oder der Knopf „🧪 Test“ oben rechts in der CGI-Ansicht.

## 1. Automatisch prüfen
„⚙ Automatisch prüfen (alle)“ baut auf kleinen Karten im Speicher Brücke, Treppen, Fluss, Wand, Dungeon mit Etagen, Vorlage usw. – mit deinen Client-Dateien – und schreibt alle Werte ins Protokoll (welche Teile die TileData-Suche findet, welche Richtung erkannt wird, ob die Treppe 20 Z hat …). Die echte Karte bleibt unverändert.

## 2. Prüfliste
Links die Schritte. Je Schritt:
▶ Vorbereiten | wählt das Werkzeug / öffnet das Fenster
So geht's | was du tun sollst
Das solltest du sehen | was herauskommen soll
📷 Aufnehmen | Bildschirmfoto aller Studio-Fenster + Werkzeug mit Einstellungen + jedes Static / Land-Tile, das das Werkzeug seit Beginn des Schritts gesetzt hat (ID, Name, Position, Höhe, Richtung)
✔ / ✖ / ? | passt, falsch, unklar – mit Kommentar
Weiter | nächster Schritt

## 3. Bericht
„💾 Bericht speichern“ packt alles in eine ZIP in „Dokumente“ (Bericht, Bilder, Daten, Fehler des Studios, Einstellungen). Diese Datei schickst du dem Entwickler.",
                En = @"# Test assistant
Menu Help → 🧪 Test assistant or the button “🧪 Test” at the top right of the CGI view.

## 1. Automatic checks
“⚙ Check automatically (all)” builds bridge, stairs, river, wall, dungeon with floors, template etc. on small maps in memory – with your client files – and writes every value into the log (which pieces the tiledata search finds, which direction is detected, whether the stairs have 20 Z …). The real map stays unchanged.

## 2. Check list
The steps on the left. For every step:
▶ Prepare | chooses the tool / opens the window
How to | what you should do
You should see | what should come out
📷 Capture | screenshot of all studio windows + tool with settings + every static / land tile the tool placed since the start of the step (id, name, position, height, direction)
✔ / ✖ / ? | OK, wrong, unclear – with a comment
Next | next step

## 3. Report
“💾 Save report” packs everything into one ZIP in “Documents” (report, pictures, data, errors of the studio, settings). Send this file to the developer."
            },
            new GuideTopic
            {
                Key = "keys", Group = "Übersicht|Overview", TitleDe = "Alle Tasten", TitleEn = "All keys",
                De = @"# Alle Tasten
## Überall
F1 | Hilfe (dieses Fenster)
## Karten-Editor
B / E / L / R / F / I / S / H | Pinsel / Radierer / Linie / Rechteck gefüllt / Füllen / Pipette / Auswahl / Hand
[ / ] | Pinsel kleiner / größer
+ / − | Zoom
V | Auswahl → CGI-Vorschau
Entf | Auswahl mit Farbe 2
Strg+Z / Strg+Y / Strg+Umschalt+Z | Rückgängig / Wiederholen / ganzer Strich
Strg+C / X / V / A / F | Kopieren / Ausschneiden / Einfügen / Alles / Füllen
Leertaste + Ziehen, Mitteltaste | verschieben
Esc | Auswahl / Einfügen beenden
## CGI-Ansicht
T | 2D / CGI
Pfeile | Auswahl verschieben (ohne Auswahl: Ansicht, Umschalt = 8)
Strg+Pfeil | Kopie daneben
Bild↑ / Bild↓ | Z ±1 (Umschalt ±5)
Entf / H / I | löschen / färben / im Item-Katalog zeigen
Strg+C / Strg+V / Strg+A | kopieren / einfügen / alles auf dem Tile
Bereich: F K J W G M T L P S | füllen, Küste, Küste glätten, Wasser, glätten, Berg, Übergänge, Log, Stempel, Statics wählen
Bereich / Lasso: R X Y E D | drehen, spiegeln ↔ / ↕, ersetzen, auf den Boden
Auswahl: R X D | Statics drehen, spiegeln, auf den Boden
Alt+Klick | Pipette (Item oder Land) bei jedem Werkzeug
Weg / Lasso: Enter, Rücktaste, Esc | fertig, letzten Punkt weg, abbrechen
Menütaste, Umschalt+Rechtsklick | Kontextmenü",
                En = @"# All keys
## Everywhere
F1 | help (this window)
## Map editor
B / E / L / R / F / I / S / H | brush / eraser / line / filled rectangle / fill / picker / select / hand
[ / ] | brush smaller / bigger
+ / − | zoom
V | selection → CGI preview
Del | selection with colour 2
Ctrl+Z / Ctrl+Y / Ctrl+Shift+Z | undo / redo / whole stroke
Ctrl+C / X / V / A / F | copy / cut / paste / all / fill
Space + drag, middle button | pan
Esc | end selection / paste
## CGI view
T | 2D / CGI
Arrows | move the selection (without selection: view, Shift = 8)
Ctrl+arrow | copy next to it
Page↑ / Page↓ | Z ±1 (Shift ±5)
Del / H / I | delete / hue / show in the item catalogue
Ctrl+C / Ctrl+V / Ctrl+A | copy / paste / all on the tile
Area: F K J W G M T L P S | fill, coast, smooth coast, water, smooth, mountain, transitions, log, stamp, select statics
Area / lasso: R X Y E D | rotate, mirror ↔ / ↕, replace, to the ground
Selection: R X D | rotate, mirror, to the ground
Alt+click | pipette (item or land) with every tool
Road / lasso: Enter, Backspace, Esc | finish, remove the last point, cancel
Menu key, Shift+right click | context menu"
            }
        };
    }
}
