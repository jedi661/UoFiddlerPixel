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
- Karte: bauen, laden, speichern, Karte → Farbkarte, Stelle im Karten-Editor zeigen.
- Rückgängig / Wiederholen, Hilfe.",
                En = @"# CGI context menu
Right click with Select, Area, Delete, Hue, Multi, Template or Cave stamp – with all other tools Shift+right click (there the plain right click takes land or the item). The menu key opens it as well.
- Top: the static and the land under the mouse – take item / land, select or delete the static.
- Tool: all tools of the palette.
- View: statics on / off, grid, heights, labels, 2D, translucent water, problems, static ids, zoom.
- Area: all commands for the marked area (fill, coast, smooth coast, water, smooth, mountain, transitions, copy, colour stamp …).
- Coast: smooth in the area, whole map, only error places, show errors.
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
Menu key, Shift+right click | context menu"
            }
        };
    }
}
