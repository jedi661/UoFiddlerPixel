// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>Text of the help tab (German, same content as MapMakerKit_Anleitung.md).</summary>
    internal static class MapMakerHelp
    {
        public const string Text =
@"MAP MAKER - ANLEITUNG
=====================

Der Map Maker erstellt eigene Ultima Online Karten. Eine Karte besteht aus zwei Bildern im Projektordner:

  Terrain.bmp   - 8 Bit Bild, jede Palettenfarbe = ein Terrain aus Data/System/Terrain.xml (Gras, Wasser, Fels ...)
  Altitude.bmp  - 8 Bit Bild, jede Palettenfarbe = eine Höhe aus Data/System/Altitude.xml (Schritte von 5)

Daraus baut der Map Maker die Client-Dateien Map#.mul, StaIdx#.mul und Statics#.mul.
Ein Pixel = ein Tile. Breite und Höhe müssen durch 8 teilbar sein (UO speichert 8x8 Blöcke).


TYPISCHER ABLAUF
----------------
 1. Projektleiste oben: Projektordner wählen, Kartengröße wählen (Felucca, Malas, Tokuno, Ter Mur ... oder 'Custom size').
 2. Karte erzeugen: leer (Tab 1), per Zufallsgenerator (Tab 2) oder aus einer Vorlage (Tab 3).
 3. Im Editor nachbearbeiten (Knopf 'Open in editor' oder Doppelklick auf die Vorschau).
 4. Höhen anpassen (Tab 4 oder Höhen-Werkzeuge im Editor).
 5. Tab 5: 'Check transitions' / 'Repair transitions', dann 'Build MUL files'.
 6. Die mul Dateien in den Client- / Server-Ordner kopieren und testen.


PROJEKTLEISTE
-------------
  Project folder  - hier liegen Terrain.bmp, Altitude.bmp und (standardmäßig) die mul Dateien.
  Map size        - Größe aus Data/System/MapInfo.xml. 'Custom size ...' erlaubt jede Größe (Vielfache von 8).
  Map # (mul)     - Nummer der Dateien: 0 = Map0.mul (Felucca), 1 = Trammel, 2 = Ilshenar, 3 = Malas, 4 = Tokuno, 5 = Ter Mur.
  Terrain/Altitude image - Dateinamen der beiden Bilder.
Eigene Größen können in MapInfo.xml eingetragen werden (Name, Num, XSize, YSize, DungeonX).

Rechts ist die Vorschau der aktuellen Karte (Terrain, Höhe oder Relief).
'Save to project' speichert die Bilder, 'Load from project' lädt sie, 'Export as PNG' speichert ein Vorschaubild.


TAB 1 - BLANK MAP (LEERE KARTE)
-------------------------------
Füllt die ganze Karte mit einem Terrain (z. B. Deep Water). Optional wird der Dungeon-Bereich
(ab X = 5120 bei Felucca/Trammel) mit 'Blank' gefüllt - wie im alten Tool.


TAB 2 - GENERATOR (ZUFALLSKARTE)
-------------------------------
Erzeugt eine komplette Welt: Kontinente und Inseln, umgeben von Wasser, Strände, Gebirge mit Schnee,
Biome (Wald, Dschungel, Sumpf ...), Hügel und einen Dungeon-Bereich mit Höhlen.

Links stehen alle Einstellungen (unten wird jede Einstellung erklärt):
  Seed               - gleiche Zahl = gleiche Karte. 'New random seed' für eine neue Welt.
  Land %             - wie viel der Oberwelt Land ist.
  Continents/Islands - Anzahl grosser Landmassen / kleiner Inseln.
  Coast roughness    - wie zerklüftet die Küste ist.
  Surrounded by water + Water border - Wasserring um die Karte.
  Beach ...          - Strand (100 % vermeidet Ecken ohne Transition).
  Mountains ...      - Anteil, Größe, Höhe, Steilheit, Schnee auf den Gipfeln.
  Hill height        - kleine Hügel auf normalem Land.
  Dungeon ...        - Höhlen im Dungeon-Bereich (Anzahl, Größe, Offenheit, Lava-/Wasserbecken).
                       'Dungeon area from X' = 0 -> die ganze Karte ist Oberwelt.

Rechts die BIOM-Tabelle: jede Zeile verteilt ein Terrain in zufälligen Flächen auf dem Land.
  Terrain    - welches Terrain
  Land %     - Anteil an der Landfläche
  Patch size - typische Größe einer Fläche in Tiles (gross = wenige grosse Flächen)
  Where      - Anywhere, Inland, Coast, NearMountains, Lowlands, North, South, West, East
Die Reihenfolge zählt (oben zuerst). Was übrig bleibt, wird 'Base terrain' (Gras).
'Separate biomes' lässt einen Streifen Gras zwischen zwei Biomen - dafür gibt es Transitions.

Skizze und Höhenkarte (optional):
  Sketch     - ein Bild in Terrain-Farben. So legst du fest WO Land, Wasser, Berge, Wälder ... sein sollen.
               Transparent, schwarz (Blank) oder [00] No Draw = nicht bemalt.
               'Sketch usage':
                 Smart (Standard) - gemaltes Gras = Land, Biome (Wald, Dschungel ...) erzeugt der Generator.
                                    Andere gemalte Terrains (Wald, Sand, Schnee ...) bleiben genau so. Fels = Gebirge.
                 UseTerrains      - jedes gemalte Terrain bleibt genau so (auch Gras).
                 LandWaterOnly    - die Skizze bestimmt nur Land / Wasser / Berge.
               'Unpainted = water' (Standard an): nicht bemalte Flächen werden Meer - deine Landmassen
               sind von Wasser umgeben. Aus: der Generator setzt dort eigene Kontinente.
               Die Skizze darf kleiner sein (z. B. 768x512 für 6144x4096) - sie wird hochskaliert und die
               Küsten werden natürlich verwirbelt ('Coast roughness').
  Height map - Graustufenbild: dunkel = Meer, hell = Land/Berge. 'Height map sea level' = Grauwert der Küste.

'Quick preview' rechnet schnell eine kleine Vorschau. 'Generate full map' erzeugt die Karte in voller Größe
(6144x4096 dauert je nach PC 10 - 30 Sekunden). 'Repair transitions' entfernt danach einzelne Tiles und
dünne Streifen ohne Transition. Einstellungen können als Preset (XML) gespeichert werden.

Tipp: Skizze erstellen = Editor -> New... -> 'Fill with [00] No Draw' -> grob malen -> speichern -> 'Load sketch'.
Oder in einem Malprogramm: transparenter Hintergrund, mit den Terrain-Farben malen (Farben: Tools -> Export palettes).
Beispiele liegen in Data/MapTemplates (Sketch_..., Heightmap_..., Map_..., Stamp_...).


TAB 3 - TEMPLATES (VORLAGEN / MUSTER)
-------------------------------------
Wähle einen Ordner mit Bildern (PNG, BMP, JPG, GIF, auch Unterordner). Standard: Data/MapTemplates.
Verwendung des ausgewählten Bildes:
  Map         - Bild wird zur Karte. Bilder in den Farben von Terrain.xml werden 1:1 übernommen,
                andere Farben werden dem nächsten Terrain zugeordnet ('Only natural terrains' für Fotos/Zeichnungen).
                Scaling: Stretch (ganze Karte), Fit (zentriert), Tile (wiederholt), Original (1:1 oben links).
  Sketch      - als Skizze für den Generator.
  Height map  - als Höhenkarte für den Generator.
  Stamp       - als Stempel im Editor (Muster per Klick auf die Karte setzen).
So kannst du vorhandene Karten oder gemalte Muster aus einem beliebigen Verzeichnis zu einer UO Karte machen.


TAB 4 - HEIGHTS (HÖHEN)
------------------------
Ändert die Höhen der aktuellen Karte. Filter: nur ein Höhen-Typ (Water/Land/Mountain/Other) und/oder ein Terrain.
  Raise / lower      - alle Höhen +/- z
  Multiply           - Höhen mal Faktor (1.5 = 50 % höher)
  Smooth             - glätten (Durchgänge)
  Roughen            - zufällige Unebenheiten (Stärke z / Größe in Tiles)
  Limit              - auf Minimum/Maximum begrenzen
  Mountain profile   - Berge neu aufbauen: Höhe steigt vom Bergfuss um 'slope' pro Tile bis 'max'
  Altitude from terrain - jede Höhe = Standardhöhe des Terrains (das alte 'Make Altitude Image')
'Undo last height change' macht die letzte Änderung rückgängig.
Höhen sind immer Schritte von 5 zwischen -120 und 115 (siehe Altitude.xml).


TAB 5 - BUILD MUL
-----------------
  Output folder   - wohin die mul Dateien geschrieben werden (leer = Projektordner).
  Random statics  - zufällige Bäume, Steine ... aus Data/Statics.
  Import static XML files - Statics aus XML Dateien (Format von 'Import Files': <Static_Tiles><Tile TileID X Y Z Hue/>).
  Random seed     - 0 = jedes Mal anders, sonst reproduzierbar.
  Check transitions  - listet Terrain-Kombinationen ohne Transition (harte Kanten im Client).
  Repair transitions - repariert einzelne Tiles / dünne Streifen automatisch.
Es werden Map#.mul, StaIdx#.mul und Statics#.mul geschrieben (# = Map # der Projektleiste).


EDITOR
------
Werkzeuge (linke Seite):
  Brush (B)      - malen mit dem gewählten Terrain ([ und ] ändern die Pinselgröße)
  Fill (F)       - zusammenhängende Fläche füllen
  Rect (R)       - Rechteck aufziehen und füllen
  Picker (I)     - Terrain und Höhe unter dem Mauszeiger übernehmen
  Raise / Lower  - Höhe anheben / absenken (Stärke unter 'Height tools')
  Set height     - Höhe auf einen Wert setzen
  Smooth         - Höhen glätten
  Stamp          - Muster aus dem Vorlagen-Ordner stempeln (No Draw / transparent wird nicht gemalt)
'Also set the terrain's default altitude' setzt beim Malen die Standardhöhe des Terrains.
Mausrad = Zoom, rechte oder mittlere Maustaste = verschieben, Pfeiltasten = verschieben.
Strg+Z / Strg+Y = Rückgängig / Wiederholen, Strg+S = Speichern.
Ansicht: Terrain, Altitude (Höhenfarben) oder Relief (Terrain mit Schattierung).
'8x8 grid' zeigt die UO Blöcke. 'Check/Repair transitions' wie in Tab 5.


TRANSITIONS - WARUM HARTE KANTEN?
--------------------------------
Übergänge zwischen zwei Terrains (z. B. Gras -> Sand) kommen aus Data/Transitions. Es gibt sie nur für
bestimmte Kombinationen und Formen. Probleme machen: einzelne Tiles, 1 Tile breite Streifen und drei
verschiedene Terrains auf engem Raum. 'Repair transitions' behebt das meiste automatisch. Sonst einen
Streifen eines passenden Terrains (meist Gras oder Sand) dazwischen malen.
Gut funktionierende Kombinationen: Gras <-> Sand / Wald / Dschungel / Schnee / Fels / Sumpf,
Sand <-> Flachwasser, Flachwasser <-> Tiefwasser, Fels <-> Schnee, Blank <-> Dungeon Cave.


DATEIEN
-------
  Data/System/Terrain.xml   - Terrains (Name, ID, TileID, Farbe, Standardhöhe 'Base')
  Data/System/Altitude.xml  - Höhenpalette
  Data/System/MapInfo.xml   - Kartengrößen
  Data/Transitions          - Transitions (ein Fehler in Common.xml wurde korrigiert)
  Data/Statics              - zufällige Statics
  Data/MapTemplates         - Standardordner für Vorlagen
  Data/System/GeneratorPresets - gespeicherte Generator-Presets
  Data/System/MapMakerConfig.xml - zuletzt benutzte Einstellungen
";
    }
}
