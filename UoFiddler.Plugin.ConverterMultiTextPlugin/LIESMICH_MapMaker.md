# Map Maker – Anleitung und Änderungen

Der Map Maker (`Old Tools Imported/MapMaker.cs`) erstellt eigene Ultima-Online-Karten.
Eine Karte besteht aus zwei 8-Bit-Bildern im Projektordner:

| Datei | Inhalt |
|---|---|
| `Terrain.bmp` | jede Palettenfarbe = ein Terrain aus `Data/System/Terrain.xml` (Gras, Wasser, Fels …) |
| `Altitude.bmp` | jede Palettenfarbe = eine Höhe aus `Data/System/Altitude.xml` (Schritte von 5, −120 … 115) |

Daraus baut der Map Maker die Client-Dateien `Map#.mul`, `StaIdx#.mul` und `Statics#.mul`.
Ein Pixel = ein Tile. Breite und Höhe müssen durch 8 teilbar sein (UO speichert 8×8-Blöcke).

Die gleiche Anleitung steht im Programm im Tab **7. Help** (Taste F1).

---

## 1. Typischer Ablauf

1. **Projektleiste** oben: Projektordner wählen, Kartengröße wählen (Felucca, Trammel, Ilshenar, Malas, Tokuno, Ter Mur, Testkarte oder *Custom size*).
2. Karte erzeugen:
   - **Tab 1 Blank map** – leere Karte mit einem Terrain
   - **Tab 2 Generator** – Zufallswelt mit Kontinenten, Inseln, Bergen, Biomen, Dungeon
   - **Tab 3 Templates** – aus einem Bild (Vorlage / Muster aus einem beliebigen Ordner)
3. Im **Editor** nachbearbeiten (Knopf *Open in editor* oder Doppelklick auf die Vorschau).
4. **Tab 4 Heights** – Höhen anpassen.
5. **Tab 5 Build MUL** – *Check transitions* → *Repair transitions* → *Build MUL files*.
6. Die mul-Dateien in den Client-/Server-Ordner kopieren und testen.

## 2. Oberfläche

```
┌ Menü: File | Tools | Help ─────────────────────────────────────────────┐
├ Projektleiste: Ordner · Kartengröße · Map # · Terrain.bmp · Altitude.bmp┤
├ Tabs (1..7)                                   │ Vorschau der Karte      │
│                                               │ Terrain/Altitude/Relief │
│                                               │ Editor · Speichern ...  │
├ Log ───────────────────────────────────────────────────── [Cancel] ─────┤
└ Status + Fortschrittsbalken ────────────────────────────────────────────┘
```

Alle Tabs benutzen dieselbe Projektleiste – es gibt keine doppelten Pfad- und Dateinamen-Felder mehr.
Lange Aufgaben laufen im Hintergrund, das Fenster bleibt bedienbar, **Cancel** bricht ab.

## 3. Generator

Links stehen alle Einstellungen in Kategorien. Unten im Feld wird jede Einstellung erklärt.

| Kategorie | Wichtigste Einstellungen |
|---|---|
| General | Seed (gleiche Zahl = gleiche Karte) |
| Land and water | Land %, Kontinente, Inseln, Küsten-Rauheit, Wasserring, Mindestgröße Inseln / Seen, Flach-/Tiefwasser |
| Coast | Strand-Terrain, Breite, Anteil (100 % vermeidet Ecken ohne Transition) |
| Land | Grundterrain (Gras), Hügelhöhe und -größe |
| Mountains | Anteil, Größe der Gebirgszüge, min/max Höhe, Steilheit, Schnee auf Gipfeln |
| Biomes | Biome trennen (Grasstreifen zwischen zwei Biomen) |
| Dungeon | Dungeon-Bereich ab X (5120 bei Felucca/Trammel, 0 = keiner), Anzahl / Größe / Offenheit der Höhlen, Lava- und Wasserbecken |
| Sketch / template | Skizzen-Modus, *Unpainted = water*, Meeresspiegel der Höhenkarte |

**Biom-Tabelle** (rechts): jede Zeile verteilt ein Terrain in zufälligen Flächen auf dem Land –
Terrain, Anteil an der Landfläche, typische Flächengröße, Lage (*Anywhere, Inland, Coast, NearMountains, Lowlands, North, South, West, East*).
Reihenfolge = Priorität. Was übrig bleibt, wird Grundterrain.

**Skizze** – ein Bild in Terrain-Farben, damit legst du fest, *wo* was sein soll:

- transparent, schwarz (Blank) oder `[00] No Draw` = nicht bemalt
- Modus **Smart** (Standard): gemaltes Gras = Land mit automatischen Biomen, andere gemalte Terrains bleiben genau so, Fels = Gebirge
- **UseTerrains**: alles Gemalte bleibt genau so · **LandWaterOnly**: nur Land / Wasser / Berge
- **Unpainted = water** (Standard an): Unbemaltes wird Meer, deine Landmassen sind von Wasser umgeben
- die Skizze darf kleiner sein (z. B. 768×512 für 6144×4096), die Küsten werden natürlich verwirbelt

**Höhenkarte** – Graustufenbild: dunkel = Meer, hell = Land / Berge.

*Quick preview* rechnet in ca. 1 s eine kleine Vorschau, *Generate full map* die volle Karte
(6144×4096: ca. 10 s im Release-Build, 30 s im Debug-Build). Einstellungen lassen sich als Preset speichern
(`Data/System/GeneratorPresets`).

## 4. Vorlagen (Templates)

Ordner wählen (Standard `Data/MapTemplates`, Unterordner werden mit gelesen; PNG, BMP, JPG, GIF, TIF).
Das gewählte Bild kann verwendet werden als

- **Map** – Bild wird die Karte. Bilder in den Farben von `Terrain.xml` werden 1:1 übernommen, andere Farben dem nächsten Terrain zugeordnet (*Only natural terrains* für Fotos / Zeichnungen). Skalierung: Stretch, Fit, Tile, Original.
- **Sketch** / **Height map** – für den Generator
- **Stamp** – Muster für das Stempel-Werkzeug im Editor

Mitgeliefert sind Beispiele: `Sketch_TwoIslands.png`, `Heightmap_Archipelago.png`, `Map_Example_TerrainColours.png`, `Stamp_Lake.png`, `Stamp_ForestGrove.png`, `Stamp_Mountain.png`.
Die Terrain-Farben für ein Malprogramm: *Tools → Export palettes (ACT / ACO)*.

## 5. Höhen (Tab 4 und Editor)

Filter nach Höhen-Typ (Water / Land / Mountain / Other) und / oder Terrain. Operationen:
anheben / absenken, multiplizieren, glätten, aufrauen, begrenzen, **Bergprofil** (Höhe steigt vom Bergfuß
um *slope* pro Tile bis *max*), Höhe aus Terrain-Standard (das alte *Make Altitude Image*).
*Undo last height change* macht die letzte Änderung rückgängig.

## 6. Editor

| Werkzeug | Taste | Funktion |
|---|---|---|
| Brush | B | malen (`[` `]` = Pinselgröße) |
| Fill | F | zusammenhängende Fläche füllen |
| Rect | R | Rechteck aufziehen und füllen |
| Picker | I | Terrain und Höhe übernehmen |
| Raise / Lower / Set z / Smooth | | Höhenwerkzeuge |
| Stamp | | Muster stempeln (transparent wird nicht gemalt) |

Mausrad = Zoom · rechte / mittlere Maustaste oder Pfeiltasten = verschieben · Strg+Z / Strg+Y / Strg+S.
Ansichten: Terrain, Altitude, Relief. *8x8 grid* zeigt die UO-Blöcke.
*New…* legt eine leere Karte oder Skizze an (für Skizzen mit `[00] No Draw` füllen).

## 7. Transitions – warum harte Kanten?

Übergänge zwischen zwei Terrains kommen aus `Data/Transitions` und sind nur aus Sicht eines Terrains definiert
(z. B. zeichnet Gras den Übergang zu Sand). Probleme machen einzelne Tiles, 1 Tile breite Streifen und drei
verschiedene Terrains auf engem Raum. *Check transitions* zeigt nur echte Probleme, *Repair transitions*
behebt die meisten automatisch. Test mit einer generierten 6144×4096-Karte: ca. 2 000 von 25 Mio. Tiles
bleiben übrig (meist Gras + Sand + Wasser an kleinen Seen).

---

## 8. Behobene Fehler

### MapMaker.cs (alt)

| # | Fehler | Folge |
|---|---|---|
| 1 | *Make UO Map* lief in einem eigenen Thread und griff von dort auf Oberfläche und Logger zu | `InvalidOperationException` / Hänger, Thread lief nach dem Schließen weiter |
| 2 | kein Fehlerschutz in diesem Thread (falsches BMP-Format, unbekannte Terrain-Gruppe) | ganz UoFiddler stürzte ab |
| 3 | Zufallshöhen: `10f * Rnd()` (Kommazahl) mit ganzen Zahlen verglichen | Berg- und Zufallshöhen fast wirkungslos |
| 4 | Tab *New Map*: `tbProjectPath` statt `ProjectPath` vorbelegt | Ordner wurde im Laufwerks-Hauptverzeichnis angelegt |
| 5 | Tab 3 und 4 lasen die Dateinamen aus Tab 1 | eigene Eingaben wurden ignoriert |
| 6 | Kartennummer nur für 3 Breiten erkannt | andere Größen überschrieben `Map0.mul` |
| 7 | Altitude → Save rief `i_Terrain.SaveACT()` auf | falsche Datei gespeichert |
| 8 | Load-Event doppelt verbunden, zwei Logger-Fenster, Tabellen bis zu 6× geladen | langsam, Speicher |
| 9 | Ohne Transition bekam eine Zelle die Höhe der *vorigen* Transition-Zelle | Höhenkarte wurde weitgehend ignoriert |
| 10 | Transition ohne Kartenkacheln: Paletten-**Schlüssel** des Terrains (z. B. 88) als Höhe addiert | Höhen-Spitzen (z. B. in Wäldern) |
| 11 | 29 Mio. `MapCell`-Objekte bei 7168×4096 | ca. 1,2 GB Speicher |
| 12 | Bitmaps / FileStreams ohne `using`, `LockBits` ohne `UnlockBits` bei Fehlern | Dateien blieben gesperrt |
| 13 | Stride der Bitmap ignoriert | falsche Daten bei Breiten, die nicht durch 4 teilbar sind |
| 14 | Dungeon-Grenze fest bei 5119 | falsch für andere Kartengrößen |

### DLLs und Daten

| Datei | Fehler |
|---|---|
| `DLL/Transition/StaticCell.cs` | TileID und Hue als 4-Byte-`int` geschrieben → 11 statt 7 Byte pro Static → **`Statics.mul` unbrauchbar**; Z-Überlauf warf Exception |
| `DLL/Transition/RandomStatics.cs` | `VBMath.Randomize()` bei jedem Aufruf → gleiche „Zufallswerte“ hintereinander, Statics verklumpt, langsam; Endlosschleife bei Freq = 255 |
| `DLL/Transition/RandomStaticCollection.cs` | Statics außerhalb der Karte → Exception, Abbruch |
| `DLL/Transition/MapTile-/StaticTileCollection.cs` | erste und letzte Kachel nur halb so oft gewählt |
| `DLL/Transition/TransitionTable.cs` | doppelter Hash-Key brach das Laden der restlichen Datei ab; Meldungsfenster pro Datei (jetzt optional + Fehlerliste) |
| `DLL/Logger/LoggerForm.cs` | nicht threadsicher, sehr langsam bei langen Logs, Endzeit immer 00:00:00, Absturz nach dem Schließen |
| `DLL/Terrain`, `DLL/Elevation` | Laden aus Programmordner, Speichern ins *aktuelle* Verzeichnis (zwei verschiedene Orte); fehlender Photoshop-Ordner; Palettenindex 255 |
| `Class/RoughEdge.cs` | gleicher Float-Vergleichsfehler, `Randomize()` pro Aufruf, doppelte Einträge warfen Exception |
| `Class/CreateElevationBitmapHelper.cs` | NullReference nach Ladefehler, `int`-Überlauf des Fortschritts bei großen Karten (Exception), unbekannte Terrains |
| `Data/Transitions/Common.xml` | 3 Hash-Keys mit 8 statt 9 Gruppen → Laden brach ab, **alle folgenden Grund-Kacheln fehlten** |
| `Data/System/MapInfo.xml` | Trammel 6144 hatte `Num="0"`, Ilshenar-Name „2034“; neu: `DungeonX`, Testkarte 1024×1024, *Custom size* |

## 9. Neue Dateien

```
Old Tools Imported/
├─ MapMaker.cs / .Designer.cs        neu gestaltetes Hauptfenster
└─ MapMakerKit/
   ├─ MapDataPaths.cs                ein Ort für alle Datenpfade
   ├─ MapPalettes.cs                 Terrain- und Höhenpalette (schnelle Tabellen)
   ├─ MapDocument.cs                 Karte im Speicher, BMP laden/speichern, Vorschau
   ├─ Noise.cs                       Perlin-Rauschen mit Seed
   ├─ GeneratorSettings.cs           Einstellungen + Biom-Regeln (als Preset speicherbar)
   ├─ MapGenerator.cs                Zufallswelt
   ├─ DungeonGenerator.cs            Höhlen (zellulärer Automat), Lava-/Wasserbecken
   ├─ TemplateImporter.cs            Vorlagen: Farbbild, Höhenkarte, Skizze, Stempel
   ├─ HeightTools.cs                 Höhen-Operationen
   ├─ TransitionRepair.cs            Transition-Prüfung und automatische Reparatur
   ├─ UoMapCompiler.cs               MUL-Erzeugung (ersetzt das alte Make())
   ├─ MapCanvas.cs                   Zeichenfläche (Zoom, Verschieben, Raster)
   ├─ MapEditorForm.cs / .Designer   Editor
   ├─ MapMakerUi.cs                  Terrain-Auswahl, Undo-Puffer
   ├─ MapMakerConfig.cs              merkt sich die letzten Einstellungen
   ├─ MapMakerHelp.cs                Hilfe-Text (Tab 7)
   └─ MapMakerKit_Anleitung.md       diese Datei
Data/MapTemplates/ und UoFiddler/Data/MapTemplates/   Beispiel-Vorlagen
```

## 10. Einbinden

- Das Plugin-Projekt (SDK-Stil) nimmt alle neuen `.cs`-Dateien automatisch auf – die `.csproj` muss nicht geändert werden.
- In den DLL-Projekten wurden nur bestehende Dateien geändert (keine neuen Dateien), die alten `.csproj` bleiben gültig. Alle Änderungen sind C# 7.3-kompatibel (.NET Framework 4.8).
- Der Map Maker wird wie bisher über den Knopf *Map Maker* in `ConverterMultiTextControl` geöffnet.
- `Data/MapTemplates` muss wie die übrigen `Data`-Ordner neben `UoFiddler.exe` liegen.

## 11. Getestet

- Build des Plugins: 0 Fehler, 0 Warnungen in den geänderten Dateien; DLL-Quellen zusätzlich mit C# 7.3.
- Generator + Reparatur + MUL-Build für 6144×4096 und 7168×4096: Dateigrößen und Static-Index bytegenau geprüft (Map: Blöcke × 196 Byte, StaIdx 12 Byte pro Block, Statics 7 Byte pro Eintrag).
- Oberfläche unter Wine gestartet: alle Tabs, Vorschau, Skizze, Höhenkarte, Generieren + Speichern, Glätten, Vorlagen-Liste, Editor mit allen Werkzeugen, Undo/Redo – ohne Exceptions.
- **Nicht getestet:** echtes Windows, die erzeugten mul-Dateien im UO-Client.
