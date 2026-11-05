# Dragon Map Studio

Nachbau und Erweiterung von **Dragon 1.03.62 (IMod13)** als UoFiddler-Werkzeug.
Geöffnet wird es im Plugin *ConverterMultiText* → Reiter **Map** → Knopf **Dragon Map Studio** (unter *Uo Map*).

Dragon bestand aus drei Programmen und vielen Skripten:

| Dragon | Dragon Map Studio |
|---|---|
| `Dragon.exe` (BMP → map.mul + Statics als WSC) | Reiter **MUL / UOP erzeugen** (map#.mul, staidx#.mul, statics#.mul, map#LegacyMUL.uop, WSC) |
| `Dragonsp.exe` (Statics in statics.mul einspielen) | Option **Statics übernehmen** (alte staidx/statics mischen) |
| `Dragon_m2b.exe` (map.mul → BMP) | **MUL / UOP → BMP** (auch UOP) |
| Skripte (`groups`, `maptrans`, `betweentrans*`, `statbetweentrans*`, `items`, `unevenly`, Fix-Tabelle) | alle als Tabellen im Reiter **Regeln & Skripte**, Export zurück ins Dragon-Format |
| Photoshop-Farbtabelle / Paletten (`Addon`) | Reiter **Farbtabelle**, Import/Export ACT, PAL, ACO, GPL |

Neu dazu: Karten-Editor mit Undo/Redo, Zufalls-Generator, Umriss-Import aus Bildern, Kartenliste,
eigenes JSON-Format mit allen Informationen, Prüfbericht für fehlende Übergänge.

---

## 1. Reiter

### Farbtabelle
- Zeigt `IMod13_Colortable.bmp`. Jeder Pixel kennt seinen Palettenindex (= **Farbadresse**).
- **Linksklick** kopiert die Adresse in die Zwischenablage. Format oben wählbar:
  `0a`, `0x0A`, `10`, `#RRGGBB`, `r,g,b`, komplette maptrans-Zeile oder alles.
- **Rechtsklick** = Malfarbe 1, **mittlere Maustaste** = Malfarbe 2 im Editor.
- Rechts: 16×16-Palette, Infos (Gruppe, Höhe, Tiles, Tile-Bild aus dem Client) und alle Werte zum Bearbeiten.
- **Neue Farbe definieren** legt auf dem ersten freien Index eine eigene Farbe an
  (Name, Kategorie, Gruppe, Höhe, Tiles, Textur-Bild).
- **Aus Regeln erzeugen** zeichnet eine aktuelle Farbtabelle (auch mit eigenen Farben).

### Karten-Editor
Werkzeuge: Pinsel (B), Radierer (E), Sprühen, Linie (L), Rechteck, Rechteck gefüllt (R), Ellipse,
Ellipse gefüllt, Füllen (F), Pipette (I), Höhe + / Höhe −, Auswahl (S), Hand (H).

- Linke Maus = Farbe 1, rechte Maus = Farbe 2.
- Mausrad = Zoom, mittlere Maus / Leertaste / Pfeiltasten = verschieben, `[` `]` = Pinselgröße.
- Strg+Z / Strg+Y, Auswahl: Strg+C / Strg+X / Strg+V (Stempel), Entf, Strg+F, Strg+A.
- **Höhe + / −** wählt die nächste Farbe derselben Gruppe (die IMod-Farben kodieren die Höhe).
- Kartenliste des Projekt-Ordners: Öffnen, Neu, Duplizieren, Umbenennen, Löschen (Papierkorb).
- Übersicht unten links (klicken = hinspringen). 8×8-Raster = UO-Blöcke.
- Undo/Redo speichert nur geänderte 64×64-Kacheln – auch 7168×4096 bleibt flüssig.

### CGI-Ansicht (neu)
Die gebaute Karte so ansehen, wie sie im Client aussieht, Fehler finden und Statics bearbeiten.

- **Aus Editor-Karte bauen** – baut im Speicher (Übergänge, Items, Unebenheit …) und zeigt das Ergebnis.
  Im Karten-Editor springt **CGI-Ansicht hier** an dieselbe Stelle.
- **MUL / UOP laden** – fertige Karte (`map#.mul` / `map#LegacyMUL.uop` + `staidx#`/`statics#` aus dem Ordner).
- Darstellung: 44×44-Land-Kacheln, Texturen (texmaps) an Hängen mit Licht, Statics mit Hue, Client-Reihenfolge,
  Raster, Max Z (unter Dächer schauen). Ohne Client-Dateien: Dragon-Farben als Ersatz.
- Rechte Leiste: Übersicht (Klick = springen, orange = Auswahl aus dem Karten-Editor), *Gehe zu X/Y*,
  Item-Liste mit Suche (Name / hex), Hue mit Farbfeld, Z-Versatz, *auf Statics stapeln*, Multis mit Vorschau,
  Eigenschaften der Auswahl (ID, Hue, Z, X, Y), Fehlersuche.
- Werkzeuge: Auswahl, Static setzen, Multi setzen, Löschen, Färben. Rechtsklick = ID + Hue übernehmen.

| Taste | Wirkung |
|---|---|
| Pfeile | Auswahl 1 Tile verschieben (↑ Norden y−1, → Osten x+1); ohne Auswahl: Ansicht |
| Strg + Pfeil | Kopie der Auswahl daneben einfügen, Kopie wird ausgewählt (mehrmals = Mauer/Zaun ziehen) |
| Bild↑ / Bild↓ | Z ±1 (Umschalt ±5) |
| Entf / H / Esc | löschen / mit Hue färben / Auswahl aufheben |
| Strg+C, Strg+V | kopieren, einfügen (Linksklick setzt, mehrfach) |
| Strg+A | alle Statics unter der Maus |
| Strg+Z, Strg+Y | Rückgängig, Wiederholen |

- **Probleme**: Land/Statics ohne Grafik, Steilkanten (ab einstellbarer Höhe), Statics im Boden, doppelte Statics.
- **Speichern**: Statics; bei einer im Speicher gebauten Karte auch `map#.mul` (und UOP, wenn in den Build-Optionen an).
  Vorhandene Dateien werden vorher als `.bak` gesichert.

### Generator & Umriss
- **Generator**: Seed, Land-%, Kontinent-Größe, Küsten-Rauheit, Wasserrand, Strand, Gebirge, Schnee,
  Klima (Norden kalt), Höhen. Die Höhe wird über die Farbe der Gruppe gesetzt.
- **Biome ohne Übergang trennen**: zwischen zwei Gruppen ohne Übergangsregel wird automatisch eine
  „Brücken-Gruppe“ gesetzt (z. B. Wald zwischen Gras und GrassBump). Test 2048×2048: Rand-Tiles ohne
  Übergang von 19 497 auf 515.
- **Biome**: Tabelle Temperatur/Feuchte → Gruppe (Reihenfolge = Priorität).
- **Umriss-Bild**: PNG mit Transparenz oder dunkle Silhouette auf hellem Grund.
  - *In Karte zeichnen*: Land + Wasser, nur Land, nur Küstenlinie oder alles.
  - *Als Maske für Generator*: Land nur innerhalb des Umrisses, mit natürlicher Küste.
  - *Maske aus aktueller Karte*: deine gemalte Land/Wasser-Verteilung wird zur Maske.
- Presets speichern / laden (JSON).

### Regeln & Skripte
Gruppen, Farben (maptrans), Übergänge (mit 3×3-Mustervorschau), Items mit Objekt-Gruppen,
Unebenheit, Fix-Tabelle, Farb-Aliase, Hinweise. Gruppen, Indizes und Tiles werden hex angezeigt.
Neue Zeilen unten in einer Tabelle = neue Regel / neues Muster. Eine neue Übergangs- oder Item-Datei
entsteht automatisch, sobald eine Regel mit neuem Dateinamen ausgewählt wird.

### MUL / UOP erzeugen
Ablauf wie Dragon: Farbe → Gruppe/Höhe/Tile, Unebenheit, Map-Übergänge (8 Nachbarn), Static-Übergänge,
Items, Fix-Tabelle, optional alte Statics übernehmen. Ausgabe: `map#.mul`, `staidx#.mul`, `statics#.mul`,
optional `map#LegacyMUL.uop` (über den UOP-Packer von UoFiddler) und `statics#.wsc`.
`build_report.txt` listet Rand-Tiles ohne Übergang:
- **Muster fehlt** → in der Übergangsdatei ergänzen,
- **Regel fehlt** → für die zwei Gruppen eine Übergangsdatei anlegen.

### Einstellungen
Projekt-Ordner (Karten), Ausgabe-Ordner (MUL/UOP), Skript-Ordner, Farbtabelle, Palette, Regelsatz-JSON,
maptrans-Datei, Kopier-Format, Raster, Undo-Speicher, Größe neuer Karten.
Gespeichert in `<UoFiddler AppData>\DragonMapStudio\settings.json`.

---

## 2. Dragon-Format – was das Studio daraus gelesen hat

| Datei | Format | Bedeutung |
|---|---|---|
| `groups.txt` | `II GG` | Palettenindex → Gruppe; Kommentar `// GG = Name` = Gruppenname |
| `maptrans.txt` | `II ZZZ T1 T2 …` | Index, Höhe (dezimal), Land-Tiles (hex, zufällig) |
| `betweentrans.txt` | `AA BB datei` | Zelle Gruppe A neben Gruppe B → `map/datei` |
| `betweentrans2.txt` | `AA datei` | Gruppe A neben *jeder* anderen → `map2/` |
| `statbetweentrans.txt` | `AA BB datei` | wie oben, setzt Statics → `statics/` |
| `statbetweentrans2/3.txt` | `GG datei` | 2: Zelle G neben anderen → `statics2/`, 3: andere neben G → `statics3/` |
| Übergangsdatei | `MUSTER T1 … Z1 Z2` | 8 Zeichen A/B im Uhrzeigersinn ab Nordwest: NW N NO O SO S SW W |
| `items.txt` | `GG Chance L T R B datei` | Chance pro Zelle (Basis 1000), Rechteck, `items/datei` |
| Item-Datei | `Häufigkeit (id z hue dx dy)…` | mehrteilige Objekte, Gewichtung nach Häufigkeit |
| `unevenly.txt` | `GG Freq L T R B Höhe` | zufällige Höhe 0..Höhe für Freq/1000 der Zellen |
| Fix-Tabelle | `X Y ID Z` | setzt einzelne Tiles zum Schluss (`fixtable.fix` im Skript-Ordner) |

Auskommentierte Regeln (`//03 11 roof.txt`) bleiben als **deaktiviert** erhalten.
Beim Laden gefundene Fehler der IMod13-Skripte: `statics/cave2blackRalls.txt` (Tippfehler) und
`statics/grass2wasteland.txt` fehlen; Farben `1e` und `7e` haben keine Gruppe.

**Noch zu prüfen im Spiel:** Bedeutung der zwei Zahlen hinter den Tiles (Z1/Z2). Umgesetzt ist:
Map-Übergang Z1 = Höhe +Z1 (abschaltbar), Static Z = Bodenhöhe + Z1; Z2 wird gespeichert, aber nicht benutzt.
Mehrere IDs in einer Static-Zeile: alle setzen (Standard) oder eine zufällig (Einstellung).

---

## 3. Eigenes Format (JSON)

*Regeln → Regelsatz (JSON) speichern* schreibt **alles** in eine Datei: 256 Farben (RGB, Name, Kategorie,
Gruppe, Höhe, Tiles, Textur, Kommentar), Gruppen, alle Übergangsregeln und -muster, Items, Unebenheit,
Fix-Tabelle, Farb-Aliase und die Kopfkommentare der Dragon-Dateien.
Ist in den Einstellungen ein Regelsatz eingetragen, wird er statt der Skripte geladen.
*Als Dragon-Skripte exportieren* schreibt wieder den kompletten Skript-Ordner (Rundreise geprüft:
gleiche Farben, 154 Regeln, 4 341 Muster, 1 069 Item-Gruppen).

Erweitern: neue Eigenschaft in `Core/DragonModel.cs` anlegen – sie erscheint automatisch in Tabellen,
Eigenschaftsfenstern und im JSON.

---

## 4. Dateien

```
DragonMapStudio/
├─ DragonMapStudioForm.cs / .Designer.cs / .resx   Hauptfenster (VS-Designer)
├─ DragonMapStudioForm.Tools.cs                    Generator, Umriss, Regeln, Build, Einstellungen
├─ DragonMapStudioForm.Cgi.cs                      CGI-Ansicht (Client-Ansicht, Statics bearbeiten)
├─ Controls/
│  ├─ ColorTableView.cs     klickbare Farbtabelle
│  ├─ PaletteGridView.cs    16×16-Palette
│  ├─ MapCanvas.cs          Zeichenfläche mit allen Werkzeugen
│  ├─ MapNavigator.cs       Übersicht
│  └─ IsoMapView.cs         CGI-Ansicht: isometrisch zeichnen, Statics bearbeiten
├─ Dialogs/NewMapDialog.*   Neue Karte (VS-Designer)
├─ Core/
│  ├─ DragonModel.cs        Datenklassen (Farbe, Gruppe, Übergang, Item …)
│  ├─ DragonRuleSet.cs      kompletter Regelsatz + JSON
│  ├─ DragonScriptIO.cs     Dragon-Skripte lesen / schreiben
│  ├─ PaletteIO.cs          ACT, PAL (JASC/RIFF), ACO, GPL, BMP-Palette
│  ├─ IndexedMap.cs         8-Bit-Karte + Undo/Redo (MapHistory)
│  ├─ IndexedBitmapIO.cs    8-Bit-BMP lesen/schreiben, RGB-Bilder umrechnen
│  ├─ MapGenerator.cs       Perlin-Rauschen, Biome, Strand, Brücken-Gruppen
│  ├─ OutlineImporter.cs    Umriss aus Bild → Maske / Karte
│  ├─ DragonCompiler.cs     BMP → MUL (der eigentliche Dragon-Ablauf)
│  ├─ MulMapIO.cs           MUL/UOP lesen/schreiben, MUL → BMP
│  ├─ MapData.cs            gebaute Karte: Land + Statics (bearbeitbar, Undo/Redo)
│  ├─ UoArtCache.cs         Client-Grafiken (Art, Texturen, Hues, Multis, Radarfarben)
│  ├─ MapProblemScanner.cs  Fehlersuche
│  ├─ ColorTableRenderer.cs Farbtabelle aus den Regeln zeichnen
│  ├─ StudioSettings.cs     Einstellungen + Pfade
│  └─ StudioHelp.cs         Hilfetext
└─ DragonData/              Original IMod13: Scripts/ und Addon/ (wird nach plugins\DragonMapStudio\ kopiert)
```

Geändert: `UserControls/ConverterMultiTextControl(.Designer).cs` (Knopf), `Properties/Resources(.resx/.Designer.cs)`
+ `Resources/dragon-map-studio.png` (Symbol), `.csproj` (DragonData kopieren).

## 5. Getestet

- Kompiliert ohne Fehler und Warnungen (C# / .NET 8, wie das Plugin).
- Skripte laden: 249 Farben, 31 Gruppen, 154 Übergänge, 96 Übergangsdateien, 4 341 Muster, 23 Item-Dateien.
- Rundreise Skripte → JSON → Skripte verlustfrei.
- 8-Bit-BMP schreiben/lesen bytegenau. MUL-Größen stimmen (Blöcke × 196, StaIdx 12 Byte/Block, Statics 7 Byte).
- UOP schreiben und wieder lesen: identisch.
- 2048×2048: Generator ≈ 4 s, Build ≈ 3 s. Oberfläche unter Wine gestartet, alle Reiter, Generator,
  Umriss, Undo/Redo, Build.
- **Nicht getestet:** echtes Windows, die erzeugten Dateien im UO-Client.

---

## 6. Ausbau-Plan (zum gemeinsamen Planen)

1. **Im Client prüfen** – Z1/Z2 der Übergänge, Item-Chance, Static-Höhen festlegen.
2. **Übergangs-Editor**: Muster per Klick im 3×3-Raster, Tiles per Drag & Drop aus der UoFiddler-Kachelliste,
   fehlende Muster aus `build_report.txt` mit einem Klick anlegen.
3. ~~Echte Vorschau~~ → erledigt (CGI-Ansicht). Weiter: Land in der CGI-Ansicht bearbeiten (Höhe, Tile), Auswahl per Rahmen ziehen, Statics mit der Maus ziehen.
4. **UOP komplett**: Statics in UOP-Clients, UOP-Karte direkt öffnen/speichern, Client-Ordner direkt beschreiben (mit Backup).
5. **Mehr Generator**: Flüsse, Seen, Straßen, Städte/Dungeon-Bereiche, Höhenkarte als Graustufen-Import/-Export, Erosion.
6. **Ebenen**: getrennte Ebenen für Terrain, Höhe, Statics-Zonen; Höhe frei malen statt über die Farbe.
7. **Mehr als 256 Farben**: eigenes 16-Bit-Kartenformat (Dragon-BMP bleibt als Export).
8. **Bereichsregeln**: Items/Unebenheit per gemaltem Bereich statt Rechteck.
9. **Statics-Import**: vorhandene Statics anzeigen und als Schicht bearbeiten.
