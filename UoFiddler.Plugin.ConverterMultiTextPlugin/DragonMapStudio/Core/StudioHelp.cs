// /***************************************************************************
//  *
//  * Dragon Map Studio – help text (tab "Hilfe", F1) in German and English
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public static class StudioHelp
    {
        public static string Text => TextDe;

        public const string TextDe =
@"DRAGON MAP STUDIO – ANLEITUNG (Deutsch)
=======================================
Nachbau und Erweiterung von Dragon 1.03.62 (IMod13): Karte als 8-Bit-Bild malen,
daraus map#.mul / staidx#.mul / statics#.mul (und map#LegacyMUL.uop) erzeugen.

GRUNDIDEE
---------
Jedes Pixel ist ein Tile. Der Pixelwert (Palettenindex 00..ff) ist die ""Farbadresse"".
maptrans.txt: Adresse -> Höhe (Z) + Land-Tiles (eins davon zufällig).
groups.txt:   Adresse -> Gruppe (Gras, Wald, Wasser …).
Zwischen Gruppen setzt der Build Übergänge (map/, statics/ …) und verteilt Items (items/).

1. FARBTABELLE
--------------
* Linksklick = Adresse in die Zwischenablage (Format oben wählbar).
* Rechtsklick = Farbe 1 im Editor, mittlere Maustaste = Farbe 2.
* Rechts: alle 256 Farben, Infos und alle Werte zum Bearbeiten.
* ""Neue Farbe definieren"" nimmt den ersten freien Index (eigene Terrains / Texturen).
* ""Aus Regeln erzeugen"" zeichnet eine aktuelle Farbtabelle.
* Die Zahl in jedem Feld der Tabelle ist die Höhe (Z): ""Grass 10"" = Gras auf Z 10.

2. KARTEN-EDITOR
----------------
Werkzeuge: Pinsel (B), Radierer (E), Sprühen, Linie (L), Rechteck, Rechteck gefüllt (R),
Ellipse, Füllen (F), Pipette (I), Höhe + / −, Auswahl (S), Hand (H).
Linke Maus = Farbe 1, rechte Maus = Farbe 2. Mausrad = Zoom, mittlere Maus / Leertaste
= verschieben. [ ] = Pinselgröße. Strg+Z / Strg+Y. Auswahl: Strg+C / X / V, Entf, Strg+F, Strg+A.
""CGI-Ansicht hier"" zeigt dieselbe Stelle wie im Client.
Bearbeiten -> ""Karte prüfen (Log)"" und ""Karte automatisch reparieren"".

3. TIPPS AUS DEM DRAGON-TUTORIAL (Ryandor, 2002)
------------------------------------------------
* Anfang: ganze Karte mit ""Water -5"" füllen, dann Kontinente mit ""Grass 0"" malen.
  Dungeon-Bereich (Felucca/Trammel x > 5119) mit ""Black 0"" füllen.
* Hügel: ""Grass 5"" Fläche, darin kleiner ""Grass 10"" usw. – Höhen in Stufen malen
  (5, 10, 15, 20 …), keine großen Sprünge.
* Übergänge brauchen MINDESTENS 2 TILES nebeneinander. Einzelne Pixel und 1 Tile
  breite Streifen erzeugen Fehler (""Contract selection by at least 2"").
* Sumpf (Swamp) muss IMMER von mindestens 2 Tiles Gras umgeben sein – Dragon hat für Sumpf
  nur Übergänge zu Gras.
* Berge in Ringen: Rand ""Rock 20"", 1 Tile nach innen ""Rock 30"", dann 50, 60, zufällig 65, 70.
  Nicht über 80 in kleinen Flächen (OSI-Berge liegen meist bei 60–70).
* Gemischtes Gelände (z. B. Wald über einen Hügel): für jede Höhe die passende Farbe
  derselben Höhe nehmen (Forest 5 auf Grass 5, Forest 10 auf Grass 10 …).
* Flüsse: ""Water -5"" malen, Ufer mindestens 2 Tiles breit mit Land-Höhen ansteigen lassen.
* Grass-""Cliffs"" (GrassBump): immer TIEFER als das angrenzende Gras malen.
* Dungeons: Gänge/Räume mit Cave- oder Floorplate-Farben – die Wände entstehen automatisch.
  Für Treppen 1–2 Tiles Lücke mit passender ""Black""-Höhe lassen.
* Städte/Wege: Böden lieber als Map-Tiles (z. B. Cobblestone) – spart Statics.
* Übergangs-Zeile ""AAAABBBB 0238 005 007"": das Tile bekommt die alte Höhe + Zufall 5..7
  (damit Gras weich in Berge übergeht). Statics: höchste Höhe der Nachbarn der Quellgruppe + Z.
* Unebenheit (unevenly.txt): nicht gewünschte Zeilen mit // auskommentieren.
* Lava im Original-Dragon funktioniert nicht zuverlässig.
* Berge gehen IMMER vom Boden aus nach oben: Rock-Höhe = Höhe des Grases daneben + 20, 30 …
  Die Reparatur und der Generator rechnen das Berg-Profil relativ zum Boden.
* Küste wie die Original-Karte (aus map0 ausgelesen, Build-Optionen ""6 Küste""):
  Ufer-Wasser ist KEIN Wasser-Tile, sondern Meeresgrund (Tile 0064) auf -15 mit
  Wasser-Statics (1797–179c) auf -5 darüber, 6 Tiles breit; der äußere Ring liegt auf -5
  und schließt an das offene Wasser (a8–ab, -5) an. Strand: -3, -1, dann 0.
  Ufer-Statics (1796, 17a9, 17ab, 179f …) liegen auf -5 = Wasserhöhe.
  Klippen-Küsten (Land höher als 10) bleiben unverändert.
* Höhleneingänge: Gruppe 1d (Invisible Cave Entrance) am Bergrand; die Regel
  1d -> Berg (map/cave-enti2mountain.txt) aktiviert der Generator automatisch.

4. KARTE PRÜFEN UND REPARIEREN
------------------------------
""Karte prüfen (Log)"" schreibt map_check_log.txt (Deutsch + Englisch, mit Koordinaten) –
diese Datei kann man zur Analyse weiterschicken. Gefunden werden:
  Farben ohne maptrans-Eintrag, Einzel-Tiles / 1-Tile-Streifen, Sumpf ohne Gras-Ring,
  Gruppen ohne Übergang, zu steile Berge, große Höhensprünge, GrassBump nicht tiefer
  als Gras, Paare ohne Übergang, die nicht repariert werden können.
""Karte reparieren"" behebt (mit Rückgängig):
  * Einzel-Tiles und 1er-Streifen -> häufigste Nachbar-Gruppe
  * Umgebungs-Regeln (Standard 18:00:2 = Sumpf mit 2 Tiles Gras umgeben)
  * Brücken-Gruppen zwischen Gruppen ohne Übergang
  * Berg-Profil (Ringe 20 30 50 60 65 70 80 90)
  * Höhensprünge begrenzen (Land 10, Berg 20)
Einstellungen: Reiter ""MUL / UOP erzeugen"" -> ""5 Reparatur"". ""Vor dem Build reparieren""
repariert nur eine Kopie beim Build.

5. CGI-ANSICHT (Client-Ansicht)
-------------------------------
Zeigt die gebaute Karte wie der Client (Land-Art, Texturen an Hängen, Statics mit Hue).
* ""Aus Editor-Karte bauen"" oder ""MUL / UOP laden"".
* Rechts: Übersicht (Klick = springen, orange = Auswahl aus dem Editor), Gehe zu X/Y,
  Items mit Suche, Hue, Z, Multis, Auswahl-Eigenschaften, Probleme.
* Werkzeuge: Auswahl, Static setzen, Multi setzen, Löschen, Färben. Rechtsklick = übernehmen.
* Tasten: Pfeile = Auswahl verschieben (ohne Auswahl: Ansicht), Strg+Pfeil = Kopie daneben,
  Bild↑/↓ = Z ±1 (Umschalt ±5), Entf, H = färben, Esc, Strg+C/V, Strg+A, Strg+Z/Y.
* Probleme: ""Ganze Karte prüfen"", ""Doppelte entfernen"", ""Log speichern"".
* ""Karte analysieren (Höhen-Log)"" liest Höhen aus (z. B. der Original-map0.mul):
  Land-Höhe nach Abstand zum Wasser, Wasser-Höhe nach Abstand zum Land, Ufer-Tiles,
  Statics an der Küste (absolut und über Land), Höhensprünge, Land-Tiles. Mit ""Bereich""
  markiert: zusätzlich ein Raster mit Höhe, Land-ID und Statics. -> map_analysis_log.txt
* Die Kartengröße beim Laden wird aus der Dateigröße bestimmt (auch 2048×2048 usw.).
* ""Höhen"" blendet die Land-Höhe jedes Tiles ein (auch im Karten-Editor ab Zoom 12).
* Zweite Leiste ""Land"": Land setzen (Tile/Textur aus dem Reiter Land), Höhe +, Höhe −,
  Höhe = (feste Höhe), Glätten – mit Radius und Schritt; ziehen = ein Rückgängig-Schritt.
  Rechtsklick nimmt Land-ID und Höhe auf.
* ""Bereich"" aufziehen: Strg+C / Strg+V kopiert Land + Statics (Höhe passt sich an),
  Entf löscht die Statics darin, Bild↑/↓ hebt / senkt Land + Statics.
* Wie der Client: nasse Kacheln ohne Textur (Wasser) werden flach gezeichnet, alle anderen
  an Hängen mit Textur gestreckt.
* Speichern: Statics, bei geändertem Land auch die Map (UOP laut Build-Option), alte Dateien -> .bak.
* Unter der Ansicht steht immer, welche Tasten und Maus-Aktionen das aktuelle Werkzeug hat.
* ""2D"" (oder Taste T): Ansicht von oben, Land-Tiles als Quadrate mit Schattierung für Hänge.
  Alle Werkzeuge gehen auch hier – gut zum Malen von Land-Tiles und Übergängen (Bögen).
* Reiter Items und Land: Bilder-Katalog statt Liste (Bildgröße wählbar), Doppelklick = malen /
  setzen. ""Großer Katalog…"" öffnet ein eigenes Fenster mit Land-Tiles, Texturen und Items.
* Static auswählen -> es wird im Item-Katalog markiert; ""Im Item-Katalog zeigen"" oder Taste I
  übernimmt ID + Hue zum erneuten Setzen; Kopieren / Einfügen auch als Knöpfe.
* Pinsel ""Küste"": Ziehen über eine Küste formt sie wie das Original (Meeresgrund -15,
  Wasser-Statics -5, Strand -3 / -1 / 0). Werte im Reiter ""Küste / Wasser"".
* Pinsel ""Wasser"": Wasser-Statics malen (Strg = entfernen), auf Wunsch mit Meeresgrund.
  Durchsichtig zeigt der Client nur Items mit dem TileData-Flag ""Translucent"" – die
  Flags stehen im Reiter. ""Wasser durchsichtig"" zeigt den Grund darunter zur Kontrolle.
* Markierter Bereich: F = mit Land-Tile füllen, K = Küste anpassen, W = Wasser
  (Umschalt+W entfernen), G = glätten; dazu Knöpfe in den Reitern Land und Küste / Wasser.
* Pinsel-Größe für alle Land-Werkzeuge oben in der Leiste ""Land"".
* Reiter ""Vorlagen"": Gebäude und Böden aus dem Vorlagen-Ordner (Einstellungen, Standard: Multis).
  Gelesen werden UoFiddler-Multis (.txt, auch alte Skripte mit dezimalen IDs), UO Architect (.uoa),
  Weltdateien (.wsc) und Sphere-Skripte (.scp); jeder Unterordner ist eine Kategorie.
  Kategorie + Suche, Vorschau mit Anzahl der Statics. ""Setzen"" (oder Doppelklick): halbtransparente
  Vorschau an der Maus, Klick setzt auf die Boden-Höhe + ""Z +"". Danach ist alles ausgewählt:
  Pfeile / Bild↑↓ verschieben, Entf löscht, Strg+Z nimmt alles zurück.
* ""Bereich → Vorlage…"": markierten Bereich speichern – Boden (Land-Tiles mit Höhen) und / oder
  Statics, mit Name und Kategorie, als Dragon-Vorlage (.dmsv.json), UoFiddler-.txt oder .uoa.
  ""Auswahl → Vorlage…"" speichert nur die ausgewählten Statics. Beim Setzen wählbar: Statics,
  Boden, Boden mit oder ohne Höhe, Statics an der Stelle vorher löschen.
* Bereich + Taste S: alle Statics darin auswählen (verschieben, kopieren, löschen).
* Pinsel ""Höhle"": am Fuß eines Berges anfangen und hineinziehen – Höhlenboden auf einer Höhe,
  der Rand wird zur Wand angehoben (der Client zeigt die Fels-Textur dahinter). Reiter ""Höhle / Weg"".
* Wege / Gebäude-Böden: Reiter Land -> ""Aus der Farbtabelle"" (cobblestones, marble, Holzboden …),
  dazu ""Höhe vom ersten Klick übernehmen"". Mehrere IDs im Feld = zufällig gemischt.

6. GENERATOR & UMRISS
---------------------
Generator: Seed, Land %, Kontinent-Größe, Gebirge, Schnee, Klima, Strand.
""Nach dem Erzeugen reparieren"" wendet die Tutorial-Regeln an.
Umriss-Bild (PNG mit Transparenz oder dunkle Silhouette): in die Karte zeichnen oder als
Maske für den Generator.

7. REGELN & SKRIPTE
-------------------
Gruppen, Farben, Übergänge (3×3-Vorschau, NW N NO O SO S SW W; A = eigene Gruppe,
B = Nachbar), Items, Unebenheit, Fix-Tabelle, Farb-Aliase. Speichern als JSON oder als
Dragon-Skript-Ordner.

8. MUL / UOP ERZEUGEN
---------------------
Farben -> Tiles, Unebenheit, Map-Übergänge, Static-Übergänge, Items, Fix-Tabelle,
doppelte Statics entfernen. dragon_log.txt (Deutsch + Englisch) listet alles inkl.
fehlender Übergänge mit Koordinaten. ""Log öffnen"" zeigt den Ausgabe-Ordner.
MUL / UOP -> BMP: bestehende Karte einlesen.

Dateien: DragonData\Scripts (IMod13-Skripte), DragonData\Addon (Farbtabelle, Paletten).
Einstellungen: %AppData%\UoFiddler\DragonMapStudio\settings.json
";

        public const string TextEn =
@"DRAGON MAP STUDIO – MANUAL (English)
====================================
Rebuild and extension of Dragon 1.03.62 (IMod13): paint the map as an 8 bit image and
build map#.mul / staidx#.mul / statics#.mul (and map#LegacyMUL.uop) from it.

BASIC IDEA
----------
Every pixel is one tile. The pixel value (palette index 00..ff) is the ""colour address"".
maptrans.txt: address -> altitude (Z) + land tiles (one of them is picked randomly).
groups.txt:   address -> group (grass, forest, water …).
Between groups the build places transitions (map/, statics/ …) and random items (items/).

1. COLOUR TABLE
---------------
* Left click = copy the address to the clipboard (format selectable above).
* Right click = colour 1 in the editor, middle button = colour 2.
* Right side: all 256 colours, details and all values for editing.
* ""Neue Farbe definieren"" (new colour) uses the first free index (own terrains / textures).
* ""Aus Regeln erzeugen"" draws an up to date colour table.
* The number inside each square is the altitude: ""Grass 10"" = grass at Z 10.

2. MAP EDITOR
-------------
Tools: brush (B), eraser (E), spray, line (L), rectangle, filled rectangle (R), ellipse,
fill (F), picker (I), height + / −, selection (S), hand (H).
Left mouse = colour 1, right mouse = colour 2. Wheel = zoom, middle mouse / space = pan.
[ ] = brush size. Ctrl+Z / Ctrl+Y. Selection: Ctrl+C / X / V, Del, Ctrl+F, Ctrl+A.
""CGI-Ansicht hier"" shows the same place like the client does.
Edit -> ""Karte prüfen (Log)"" (check map) and ""Karte automatisch reparieren"" (repair).

3. TIPS FROM THE DRAGON TUTORIAL (Ryandor, 2002)
------------------------------------------------
* Start: flood fill the whole map with ""Water -5"", paint continents with ""Grass 0"".
  Dungeon area (Felucca/Trammel x > 5119): fill with ""Black 0"".
* Hills: an area of ""Grass 5"", a smaller ""Grass 10"" inside etc. – paint heights in steps
  (5, 10, 15, 20 …), no big jumps.
* Transitions need AT LEAST 2 TILES side by side. Single pixels and 1 tile wide strips cause
  errors (""contract selection by at least 2"").
* Swamp must ALWAYS be surrounded by at least 2 tiles of grass – Dragon only has swamp
  transitions to grass.
* Mountains in rings: border ""Rock 20"", one tile inwards ""Rock 30"", then 50, 60, random 65, 70.
  Not over 80 in small spots (OSI mountains are mostly 60–70).
* Mixed terrain (e.g. forest over a hill): use the colour of the same height
  (Forest 5 on Grass 5, Forest 10 on Grass 10 …).
* Rivers: paint ""Water -5"", let the banks rise over at least 2 tiles.
* Grass ""cliffs"" (GrassBump): always LOWER than the adjacent grass.
* Dungeons: corridors / rooms with cave or floorplate colours – walls are created automatically.
  Leave a 1–2 tile gap with the right ""Black"" height for stairs.
* Towns / roads: floors better as map tiles (e.g. cobblestone) – keeps the statics small.
* Transition line ""AAAABBBB 0238 005 007"": the tile gets the old height + random 5..7
  (so grass flows into mountains). Statics: highest z of the neighbours of the source group + Z.
* Unevenly (unevenly.txt): comment out lines you don't want with //.
* Lava did not work reliably in the original Dragon.
* Mountains ALWAYS rise from the ground: rock height = height of the grass next to it + 20, 30 …
  Repair and generator calculate the mountain profile relative to the ground.
* Coast like the original map (read from map0, build options ""6 Küste""): shore water is
  NOT a water tile but sea floor (tile 0064) at -15 with water statics (1797–179c) at -5 on
  top, 6 tiles wide; the outer ring lies at -5 and joins the open water (a8–ab, -5).
  Beach: -3, -1, then 0. Shore statics (1796, 17a9, 17ab, 179f …) lie at -5 = water level.
  Cliff coasts (land higher than 10) stay as they are.
* Cave entrances: group 1d (invisible cave entrance) at the mountain border; the generator
  activates the rule 1d -> mountain (map/cave-enti2mountain.txt) automatically.

4. CHECK AND REPAIR THE MAP
---------------------------
""Karte prüfen (Log)"" writes map_check_log.txt (German + English, with coordinates) – send
this file for analysis. Found are:
  colours without maptrans entry, single tiles / 1 tile strips, swamp without grass ring,
  groups without transition, too steep mountains, big height jumps, GrassBump not lower than
  grass, pairs without transition that cannot be repaired.
""Karte reparieren"" fixes (with undo):
  * single tiles and 1 tile strips -> most frequent neighbour group
  * surround rules (default 18:00:2 = swamp surrounded by 2 tiles of grass)
  * bridge groups between groups without transition
  * mountain profile (rings 20 30 50 60 65 70 80 90)
  * limit height jumps (land 10, mountain 20)
Settings: tab ""MUL / UOP erzeugen"" -> ""5 Reparatur"". ""Vor dem Build reparieren"" repairs
only a copy while building.

5. CGI VIEW (client view)
-------------------------
Shows the built map like the client (land art, textures on slopes, statics with hue).
* ""Aus Editor-Karte bauen"" (build from editor) or ""MUL / UOP laden"" (load).
* Right side: overview (click = jump, orange = editor selection), go to X/Y, items with
  search, hue, Z, multis, selection properties, problems.
* Tools: select, place static, place multi, delete, colour. Right click = take id + hue.
* Keys: arrows = move selection (no selection: view), Ctrl+arrow = copy next to it,
  PgUp/PgDn = Z ±1 (Shift ±5), Del, H = colour, Esc, Ctrl+C/V, Ctrl+A, Ctrl+Z/Y.
* Problems: ""Ganze Karte prüfen"", ""Doppelte entfernen"" (remove duplicates), ""Log speichern"".
* ""Karte analysieren (Höhen-Log)"" reads the heights (e.g. of the original map0.mul):
  land z by distance to water, water z by distance to land, shore tiles, statics at the coast
  (absolute and above land), height steps, land tiles. With a marked ""Bereich"": also a grid
  of height, land id and statics. -> map_analysis_log.txt
* The map size on loading is taken from the file size (also 2048×2048 etc.).
* ""Höhen"" shows the land height of every tile (also in the map editor from zoom 12).
* Second bar ""Land"": set land (tile / texture from the Land tab), height +, height −,
  height = (fixed), smooth – with radius and step; one drag = one undo step.
  Right click takes land id and height.
* ""Bereich"" (area): Ctrl+C / Ctrl+V copies land + statics (height adapts), Del deletes the
  statics inside, PgUp/PgDn raises / lowers land + statics.
* Like the client: wet tiles without texture (water) are drawn flat, all others are stretched
  with their texture on slopes.
* Save: statics, with changed land also the map (UOP per build option), old files -> .bak.
* Below the view the keys and mouse actions of the current tool are always shown.
* ""2D"" (or key T): view from above, land tiles as squares with shading for slopes. All tools
  work here too – good for painting land tiles and transitions (curves).
* Tabs Items and Land: picture catalogue instead of a list (picture size selectable), double
  click = paint / place. ""Großer Katalog…"" opens a window with land tiles, textures and items.
* Select a static -> it is marked in the item catalogue; ""Im Item-Katalog zeigen"" or key I
  takes id + hue to place it again; copy / paste also as buttons.
* Brush ""Küste"" (coast): dragging over a coast shapes it like the original (sea floor -15,
  water statics -5, beach -3 / -1 / 0). Values in the tab ""Küste / Wasser"".
* Brush ""Wasser"" (water): paint water statics (Ctrl = remove), optionally with sea floor.
  The client draws only items with the tiledata flag ""Translucent"" see-through – the flags
  are shown in the tab. ""Wasser durchsichtig"" shows the ground below for checking.
* Marked area: F = fill with land tile, K = adjust coast, W = water (Shift+W removes),
  G = smooth; also buttons in the tabs Land and Küste / Wasser.
* Brush size for all land tools at the top of the bar ""Land"".
* Tab ""Vorlagen"" (templates): buildings and floors from the template folder (settings, default:
  Multis). Reads UoFiddler multis (.txt, also old scripts with decimal ids), UO Architect (.uoa),
  world files (.wsc) and Sphere scripts (.scp); every sub folder is a category. Category + search,
  preview with the number of statics. ""Setzen"" (or double click): see-through preview at the mouse,
  click places it on the ground height + ""Z +"". Afterwards everything is selected: arrows /
  PgUp/PgDn move, Del deletes, Ctrl+Z undoes all.
* ""Bereich → Vorlage…"": save the marked area – floor (land tiles with heights) and / or statics,
  with name and category, as Dragon template (.dmsv.json), UoFiddler .txt or .uoa.
  ""Auswahl → Vorlage…"" saves only the selected statics. When placing: statics, floor, floor with
  or without height, delete the statics there first.
* Area + key S: select all statics inside (move, copy, delete).
* Brush ""Höhle"" (cave): start at the foot of a mountain and drag into it – cave floor at one height,
  the edge is raised to a wall (the client shows the rock texture behind). Tab ""Höhle / Weg"".
* Roads / building floors: tab Land -> ""Aus der Farbtabelle"" (cobblestones, marble, wood floor …),
  plus ""Höhe vom ersten Klick übernehmen"". Several ids in the field = mixed at random.

6. GENERATOR & OUTLINE
----------------------
Generator: seed, land %, continent size, mountains, snow, climate, beach.
""Nach dem Erzeugen reparieren"" applies the tutorial rules.
Outline image (PNG with transparency or dark silhouette): draw it into the map or use it
as mask for the generator.

7. RULES & SCRIPTS
------------------
Groups, colours, transitions (3×3 preview, NW N NE E SE S SW W; A = own group,
B = neighbour), items, unevenly, fix table, colour aliases. Save as JSON or as Dragon
script folder.

8. BUILD MUL / UOP
------------------
Colours -> tiles, unevenly, map transitions, static transitions, items, fix table,
remove duplicate statics. dragon_log.txt (German + English) lists everything including
missing transitions with coordinates. ""Log öffnen"" opens the output folder.
MUL / UOP -> BMP: read an existing map.

Files: DragonData\Scripts (IMod13 scripts), DragonData\Addon (colour table, palettes).
Settings: %AppData%\UoFiddler\DragonMapStudio\settings.json
";

        public const string TextFr =
@"DRAGON MAP STUDIO – MANUEL (Français)
=====================================
Reconstruction et extension de Dragon 1.03.62 (IMod13) : peindre la carte comme image 8 bits
et en créer map#.mul / staidx#.mul / statics#.mul (et map#LegacyMUL.uop).
Langue de l'interface : menu ""Sprache / Language / Langue"" ou Paramètres.

IDÉE DE BASE
------------
Chaque pixel est une tuile. La valeur du pixel (index de palette 00..ff) est ""l'adresse de couleur"".
maptrans.txt : adresse -> altitude (Z) + tuiles de terrain (l'une est choisie au hasard).
groups.txt :   adresse -> groupe (herbe, forêt, eau …).
Entre les groupes, la construction pose des transitions (map/, statics/ …) et des objets (items/).

1. TABLE DES COULEURS
---------------------
* Clic gauche = copier l'adresse dans le presse-papiers (format au-dessus).
* Clic droit = couleur 1 dans l'éditeur, bouton du milieu = couleur 2.
* À droite : les 256 couleurs, détails et toutes les valeurs modifiables.
* ""Définir une nouvelle couleur"" utilise le premier index libre (terrains / textures propres).
* Le nombre dans chaque case est l'altitude : ""Grass 10"" = herbe à Z 10.

2. ÉDITEUR DE CARTE
-------------------
Outils : pinceau (B), gomme (E), aérographe, ligne (L), rectangle, rectangle plein (R), ellipse,
remplir (F), pipette (I), hauteur + / −, sélection (S), main (H).
Bouton gauche = couleur 1, bouton droit = couleur 2. Molette = zoom, milieu / espace = déplacer.
[ ] = taille du pinceau. Ctrl+Z / Ctrl+Y. Sélection : Ctrl+C / X / V, Suppr, Ctrl+F, Ctrl+A.
""Vue CGI ici"" montre le même endroit comme le client.
Édition -> ""Vérifier la carte (journal)"" et ""Réparer la carte automatiquement"".

3. CONSEILS DU TUTORIEL DRAGON (Ryandor, 2002)
----------------------------------------------
* Début : remplir toute la carte avec ""Water -5"", peindre les continents avec ""Grass 0"".
  Zone des donjons (Felucca/Trammel x > 5119) : remplir avec ""Black 0"".
* Collines : une zone ""Grass 5"", une plus petite ""Grass 10"" à l'intérieur etc. – par paliers.
* Les transitions demandent AU MOINS 2 TUILES côte à côte. Les pixels isolés et les bandes
  de 1 tuile provoquent des erreurs.
* Le marais doit TOUJOURS être entouré d'au moins 2 tuiles d'herbe.
* Montagnes en anneaux : bord ""Rock 20"", puis 30, 50, 60, 65, 70 vers l'intérieur.
* Les montagnes montent TOUJOURS depuis le sol : hauteur de roche = herbe voisine + 20, 30 …
* GrassBump (falaises d'herbe) : toujours PLUS BAS que l'herbe voisine.
* Villes / routes : sols plutôt en tuiles de terrain (p. ex. pavés).
* Côte comme l'original (map0) : l'eau du rivage est un fond marin (tuile 0064) à -15 avec des
  statiques d'eau (1797–179c) à -5, sur 6 tuiles ; anneau extérieur à -5 ; plage -3, -1, 0.
* Entrées de grotte : groupe 1d au bord des montagnes (règle 1d -> montagne automatique).

4. VÉRIFIER ET RÉPARER LA CARTE
-------------------------------
""Vérifier la carte (journal)"" écrit map_check_log.txt (allemand + anglais, avec coordonnées).
""Réparer la carte"" corrige (annulable) : tuiles isolées, règles d'entourage, groupes pont,
profil de montagne, sauts de hauteur. Réglages : onglet ""Créer MUL / UOP"" -> ""5 Réparation"".

5. VUE CGI (vue client)
-----------------------
Montre la carte construite comme le client (graphismes du terrain, textures sur les pentes,
statiques avec teinte).
* ""Construire depuis l'éditeur"" ou ""Charger MUL / UOP"".
* Sous la vue : les touches et actions de souris de l'outil actuel sont toujours affichées.
* ""2D"" (touche T) : vue de dessus, tous les outils fonctionnent aussi.
* Outils : sélection, poser une statique, poser un multi, supprimer, teinter.
  Flèches = déplacer la sélection, Ctrl+flèche = copie à côté, PgPréc/PgSuiv = Z ±1,
  Suppr, H = teinter, I = montrer dans le catalogue, Ctrl+C / Ctrl+V, Ctrl+Z / Ctrl+Y.
* Barre ""Terrain"" : poser le terrain, hauteur + / − / =, lisser, côte, grotte, eau, zone ;
  taille du pinceau en haut. Un trait = une étape d'annulation. Clic droit = prendre ID et hauteur.
* Onglets Objets et Terrain : catalogue d'images ; ""Grand catalogue…"" ouvre une fenêtre
  avec tuiles, textures et objets. Terrain -> ""Depuis la table des couleurs"" : pavés,
  marbre, plancher, sol de grotte … (tuiles mélangées au hasard).
* Pinceau ""Côte"" : forme la côte comme l'original. Pinceau ""Eau"" : statiques d'eau
  (Ctrl = retirer) ; transparentes dans le client seulement avec le flag TileData ""Translucent"".
* Pinceau ""Grotte"" : commencer au pied d'une montagne et tirer vers l'intérieur – sol à une
  hauteur, le bord devient mur (texture de roche derrière).
* Zone marquée : F = remplir, K = côte, W = eau (Maj+W retirer), G = lisser, S = choisir les
  statiques, PgPréc/PgSuiv = élever / abaisser, Ctrl+C / Ctrl+V = copier / coller terrain + statiques.
* Onglet ""Modèles"" : bâtiments et sols du dossier des modèles (txt UoFiddler, UO Architect .uoa,
  .wsc, Sphere .scp, .dmsv.json). Chaque sous-dossier est une catégorie ; recherche et aperçu.
  ""Poser"" : aperçu transparent à la souris, le clic pose à la hauteur du sol + ""Z +"".
  Ensuite tout est sélectionné et se déplace avec les flèches / PgPréc / PgSuiv.
* ""Zone → modèle…"" / ""Sélection → modèle…"" : enregistrer sol et / ou statiques comme nouveau
  modèle (nom, catégorie, format).
* ""Analyser la carte"" lit les hauteurs (p. ex. de la map0 originale) -> map_analysis_log.txt.
* Enregistrer : statiques, et la carte si le terrain a changé ; anciens fichiers -> .bak.

6. GÉNÉRATEUR & CONTOUR
-----------------------
Générateur : graine, % de terre, taille des continents, montagnes, neige, climat, plage.
Image de contour (PNG transparent ou silhouette sombre) : dessiner dans la carte ou masque.

7. RÈGLES & SCRIPTS
-------------------
Groupes, couleurs, transitions (aperçu 3×3, NO N NE E SE S SO O ; A = propre groupe,
B = voisin), objets, irrégularité, table de corrections, alias. Enregistrer en JSON ou en scripts Dragon.

8. CRÉER MUL / UOP
------------------
Couleurs -> tuiles, irrégularité, transitions, objets, corrections, doublons supprimés.
dragon_log.txt liste tout, y compris les transitions manquantes avec coordonnées.

Fichiers : DragonData\Scripts (scripts IMod13), DragonData\Addon (table des couleurs),
DragonData\Multis (modèles), DragonData\Lang (traductions en.txt / fr.txt, modifiables).
Paramètres : %AppData%\UoFiddler\DragonMapStudio\settings.json
";
    }
}
