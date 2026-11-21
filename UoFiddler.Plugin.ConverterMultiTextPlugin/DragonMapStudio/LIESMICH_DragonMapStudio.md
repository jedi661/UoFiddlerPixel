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
├─ DragonMapStudioForm.Fix.cs                      Prüfen / Reparieren, Logs, Hilfe-Sprache
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
│  ├─ MapProblemScanner.cs  Fehlersuche (CGI)
│  ├─ MapFixer.cs           Prüfen / Reparieren nach Tutorial-Regeln + zweisprachige Logs
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

---

## Prüfen, Reparieren und Logs (Version 3)

Grundlage: *UO Dragon Mod 11 – Map Making Tutorial* (Ryandor, 2002).

### Was aus dem Tutorial übernommen wurde

| Tutorial | Umsetzung |
|---|---|
| Übergangs-Zeile `AAAABBBB 0238 005 007`: neue Höhe = alte Höhe + Zufall 5..7 | Build: Höhe + Zufall zwischen Z1 und Z2 (vorher nur + Z1) |
| Statics: „höchste Höhe der umliegenden Tiles der Quellgruppe + Z“ | Option *Static-Z von den Nachbarn* (Standard an) |
| „itemid1 to x will be used randomly“ | Statics: eine ID zufällig (Standard *RandomOne*, vorher alle IDs → doppelte Statics) |
| Übergänge brauchen mindestens 2 Tiles nebeneinander | Reparatur entfernt Einzel-Tiles und 1 Tile breite Streifen |
| Sumpf immer mit mindestens 2 Tiles Gras umgeben | Umgebungs-Regel `18:00:2` |
| Berge in Ringen: Rock 20, 30, 50, 60, 65, 70, nicht über 80 | Berg-Profil `20 30 50 60 65 70 80 90` |
| Höhen in Stufen 5, 10, 15 … | Höhensprünge begrenzen (Land 10, Berg 20) |
| GrassBump immer tiefer als angrenzendes Gras | Prüfung `19:00` (Hinweis im Log) |
| Remove Statics (sonst doppelte Items) | *Doppelte Statics entfernen* beim Build und in der CGI-Ansicht |

### Knöpfe

- **Bearbeiten → Karte prüfen (Log)** oder Reiter *MUL / UOP erzeugen* → **Karte prüfen (Log)**:
  ändert nichts, schreibt `map_check_log.txt` in den Ausgabe-Ordner.
- **Karte reparieren**: repariert die Editor-Karte (ein Rückgängig-Schritt), schreibt `map_fix_log.txt`.
- **Vor dem Build reparieren** (Build-Optionen → *5 Reparatur*): repariert nur eine Kopie beim Build.
- **Log öffnen**: zeigt das zuletzt geschriebene Log.
- Build schreibt `dragon_log.txt`; die CGI-Ansicht hat *Doppelte entfernen* und *Log speichern* (`cgi_check_log.txt`).

Alle Logs sind **Deutsch + Englisch** und enthalten **Koordinaten** (bis 15 Beispiele je Fehlerart) –
zum Weiterschicken an mich, damit wir gezielt Regeln / Übergänge ergänzen können.

Test 2048×2048 (generiert, ohne Reparatur → mit Reparatur): Rand-Tiles ohne Übergang 19 497 → 64,
Steilkanten (≥ 30 z) 10 → 0, doppelte Statics werden entfernt.

### CGI-Ansicht: Küsten-Darstellung

Land-Kacheln ohne Textur (z. B. Wasser) wurden an Hängen falsch verzerrt – daher die schwarzen Dreiecke an der
Küste. Jetzt wird die Kachel über zwei Dreiecke exakt auf die vier Ecken gelegt; sehr steile, entartete Kacheln
werden einfarbig gefüllt statt die ganze Ansicht abbrechen zu lassen.

---

## English summary

Dragon Map Studio rebuilds Dragon 1.03.62 (IMod13) inside UoFiddler: paint the map as 8-bit image, check and
repair it, build `map#.mul`, `staidx#.mul`, `statics#.mul` (+ `map#LegacyMUL.uop`), look at the result in the
**CGI view** (client like) and edit statics / multis there.

- **Check / repair** follows the Dragon tutorial: no single tiles or 1 tile strips, swamp surrounded by 2 tiles of
  grass, bridge groups between groups without transition, mountains rising in rings (20 30 50 60 65 70),
  limited height jumps, GrassBump lower than grass.
- **Build**: transition height = old height + random Z1..Z2, static z = highest z of the source group neighbours
  + Z, one random static id per line, duplicates removed.
- **Logs** (`dragon_log.txt`, `map_check_log.txt`, `map_fix_log.txt`, `cgi_check_log.txt`) are German + English with
  coordinates – send them for analysis.
- Help tab: language selection German / English.

---

## Version 4: Küsten, Berge, Höhlen, Land bearbeiten

| Problem (deine Bilder) | Ursache | Lösung |
|---|---|---|
| Zacken / „Zelte“ an der Küste | Daten: `sand2water`, `grass2water` setzen Kanten-Tiles auf −10, Ecken bleiben 0 → Zickzack. Zusätzlich hat die Ansicht nasse Kacheln ohne Textur gestreckt. | Build-Option **Küste glätten** (Durchschnitt der Ufer-Höhen); die Ansicht zeichnet nasse Kacheln ohne Textur flach wie der Client. Test 2048²: 0 Sprünge ≥ 8 zwischen Ufer-Nachbarn. |
| Ufer-Welle zu hoch | Static-Z = höchste Wasserhöhe + 10; `water2x` senkt das Ufer-Wasser auf −15…−5 | **Static-Z Basis = SourceLowest** (tiefstes Wasser + 10 ≈ Wasserhöhe). Ø z der Wellen jetzt −7,7. |
| Berg tiefer als Gras („Mulde“) | Berg-Profil 20/30/50… war absolut, Gras lag schon auf 25–30 | Berg-Profil jetzt **relativ zum Boden**: Ring n liegt zwischen Boden + h/2 und Boden + h. Generator ebenso. Test: 0 Berg-Tiles tiefer als das Land daneben. |
| Keine Höhlen | `map/cave-enti2mountain.txt` liegt bei, ist aber nicht in `betweentrans.txt` | Generator setzt Höhleneingänge (Gruppe 1d) am Bergrand und aktiviert die Regel 1d → Berg. Die Reparatur lässt Gruppe 1d in Ruhe. Nicht eingetragene Übergangsdateien werden jetzt mitgeladen. |

**CGI-Ansicht – Land bearbeiten** (zweite Leiste „Land“, Reiter „Land“ mit Land-Grafik und Textur):
Land setzen, Höhe +, Höhe −, Höhe =, Glätten (Radius, Schritt), Rechtsklick nimmt Land-ID + Höhe auf.
**Bereich**: Strg+C / Strg+V (Land + Statics, Höhe passt sich an), Entf (Statics löschen), Bild↑/↓ (heben/senken).
**Höhen** blendet die Z-Werte ein – auch im Karten-Editor ab Zoom 12.

Einstellungen werden beim ersten Start auf Version 4 umgestellt (StaticZ = SourceLowest, Küste glätten an,
Gebirgshöhen 15–70 über Boden). / Settings are migrated to version 4 on first start.

---

## Version 5: Küste wie das Original, Höhen-Analyse, Größen-Erkennung

**Dein CGI-Log:** Alle 2 051 „Steilkanten“ lagen genau an 8er-Blockgrenzen (x mod 8 = 7). Ursache: Eine
2048×2048-MUL hat keine Standardgröße, die CGI-Ansicht hat beim Laden 7168×4096 angenommen und die Blöcke an
falsche Stellen gelegt. **Behoben:** Die Größe kommt jetzt aus Dateigröße + offener Editor-Karte (quadratisch,
übliche Höhen; auch für UOP). Die Steilkanten in diesem Log und vermutlich auch die 2 949 „im Boden“ sind dadurch nicht echt.

**Küste (Build-Optionen „6 Küste“, Standard an):**

| | vorher | jetzt (wie Original-Bilder) |
|---|---|---|
| Wasser am Ufer | −5 / −15 zufällig, 1 Tile breit | gleichmäßig −15, 6 Tiles breit |
| Land am Wasser | −10 oder 0 (Zickzack) | −5, dann −2, dann 0 (Abstand 1, 2, 3) |
| Ufer-Statics (Wellen) | Wasserhöhe + 10 | fest z 0 |

Klippen-Küsten (Land > 10) bleiben. Alle Werte sind einstellbar.

**„Karte analysieren (Höhen-Log)“** (CGI-Ansicht → Probleme): lädt man die **Original-map0.mul**, schreibt das
Werkzeug `map_analysis_log.txt` mit: Land-Höhe nach Abstand zum Wasser, Wasser-Höhe nach Abstand zum Land,
häufigste Ufer-Tiles, Statics an der Küste (z absolut und über Land), Höhensprünge, Land-Tiles mit Höhen.
Mit markiertem Bereich zusätzlich ein Raster (Höhe, Land-ID, Statics). **Diese Datei aus dem Original bitte
schicken** – dann stelle ich Küsten-Profil, Wellen-Höhe und Berg-Regeln genau auf die Originalwerte ein.

Außerdem: Höhleneingänge (1d) erscheinen nicht mehr als „Regel fehlt“ im Log (die offene Seite ist gewollt).

---

## Version 6: Küste nach der Original-map0

Ausgewertet mit „Karte analysieren“ auf der Original-`map0.mul` (7168×4096, 2,79 Mio. Statics):

| Original map0 | Bedeutung | Umsetzung (Build-Optionen „6 Küste“) |
|---|---|---|
| Land-Tile `0064` 564 000×, 88 % auf z −15, in 1–6 Tiles Abstand zum offenen Wasser | Ufer-Wasser ist **Meeresgrund**, kein Wasser-Tile | Wasser in 1–6 Tiles Abstand zum Land → Tile `0064`, z −15 (äußerer Ring −5) |
| Statics `1797`–`179c` je ~100 000×, z −5, 10 über dem Land | **Wasser-Statics** liegen auf dem Grund | auf jedes dieser Tiles ein zufälliges `1797`–`179c` auf z −5 (nicht dort, wo schon ein Ufer-Static liegt) |
| `1796`, `17a9`, `17ab`, `179f`, `17a0` … abs z −5 | Ufer-Statics auf **Wasserhöhe** | „Ufer-Statics Höhe“ = −5 (vorher 0) |
| offenes Wasser `a8`–`ab` z −5 (97 %) | bleibt | bleibt |
| Strand nach dem Grund: 0, −1…−3 | flacher Strand | „Land-Höhen zum Wasser“ = −3 −1 0 |

Vergleich gebaute 2048²-Karte mit derselben Analyse: Meeresgrund −15 (77 %), Wasser-Statics +10 über dem Grund (100 %),
Strand −3/−1/0 – passt zum Original. Der braune „Graben“ (Tile `0066` von `map2/water2x` ohne Wasser darüber)
ist dadurch abgedeckt.

Die Analyse wertet Zellen mit Wasser-Statics jetzt als Wasser und zeigt zusätzlich die Höhe des Meeresgrunds.

---

## Version 7: CGI-Ansicht – 2D, Kataloge, Küsten- und Wasser-Pinsel

| Neu | Bedienung |
|---|---|
| **Tasten-Hilfe** | Leiste unter der Ansicht zeigt immer die Tasten / Maus-Aktionen des aktuellen Werkzeugs (statt kurzem Tooltip). |
| **2D-Ansicht** | Knopf „2D“ oder Taste **T**: Karte von oben, Land-Tiles als Quadrate, Hänge schattiert. Alle Werkzeuge (Land malen, Bereich, Küste, Wasser, Statics) gehen auch hier. |
| **Bilder-Katalog** | Reiter Items und Land zeigen Vorschaubilder statt Liste (klein / mittel / groß). Klick = auswählen, Doppelklick = Werkzeug aktivieren. „Texturen zeigen“ im Reiter Land. |
| **Großer Katalog…** | Eigenes Fenster (`Dialogs/TilePickerForm`, im Designer öffnbar): Land-Tiles, Texturen, Items mit Suche und Vorschau. |
| **Static finden** | Ausgewähltes Static wird im Item-Katalog markiert. „Im Item-Katalog zeigen + übernehmen“ / Taste **I** übernimmt ID + Hue zum erneuten Setzen. Kopieren / Einfügen auch als Knöpfe. |
| **Pinsel „Küste“** | Über eine Küste ziehen: Meeresgrund (`0064`, −15), äußerer Ring −5, Wasser-Statics −5, Strand −3/−1/0 – wie die Original-map0. Ein Zug = ein Rückgängig-Schritt. |
| **Pinsel „Wasser“** | Wasser-Statics malen (Strg = entfernen), wahlweise mit Meeresgrund darunter. Eigene IDs möglich („= Item aus dem Katalog“). |
| **Transparentes Wasser** | Items mit TileData-Flag `Translucent` werden durchsichtig gezeichnet (wie im Client); der Reiter „Küste / Wasser“ zeigt die Flags der Wasser-IDs. „Wasser durchsichtig“ zeigt alle Wasser-Statics durchsichtig zur Kontrolle. |
| **Bereich** | Mit der Maus aufziehen, dann **F** füllen (Land-Tile), **K** Küste anpassen, **W** Wasser (Umschalt+W entfernen), **G** glätten, Bild↑/↓ heben / senken, Strg+C / Strg+V. Auch als Knöpfe. |
| **Pinsel-Größe** | Oben in der Leiste „Land“ (1 Tile bis 33×33). |

Neue Dateien: `Core/CoastShaper.cs`, `Controls/TileGridView.cs`, `Dialogs/TilePickerForm.cs` (+ `.Designer.cs`).

---

## Version 8: Vorlagen (Gebäude, Böden), Höhlen, Wege

**Vorlagen-Ordner** (Einstellungen → „Vorlagen-Ordner“, Standard `DragonData/Multis`, im Plugin-Ordner): jeder Unterordner ist eine Kategorie.
Mitgeliefert sind die 586 Gebäude aus `Multis.rar` (Fiddler, OLDScript, UOArchitect – Arena, Festungen, Haus, Kirchen, Minen, Schiffe …).

| Format | Lesen | Schreiben |
|---|---|---|
| Dragon-Vorlage `.dmsv.json` | Land-Tiles mit Höhen + Statics + Name/Kategorie | ja |
| UoFiddler-Multi `.txt` (`0xID x y z flag`, auch dezimale IDs alter Skripte) | ja | ja |
| UO Architect `.uoa` (Version 1 und 2) | ja (mit Hue) | ja (Version 1) |
| Weltdatei `.wsc` (`SECTION WORLDITEM`) | ja (absolut → zentriert) | – |
| Sphere `.scp` (`ITEMDEF` mit `COMPONENT=`, oder `WORLDITEM` mit `P=`) | ja | – |

**Reiter „Vorlagen“** (CGI-Ansicht): Kategorie, Suche (Name, Kategorie, Autor), Vorschau, Anzahl Statics und Größe.
„Setzen“ / Doppelklick → halbtransparente Vorschau an der Maus, Klick setzt auf die Boden-Höhe + „Z +“; danach ist alles ausgewählt und mit Pfeilen / Bild↑↓ verschiebbar.
Optionen: Statics, Boden, Boden mit Höhe, Statics an der Stelle vorher löschen.

**Neue Vorlage aus der Karte:** Werkzeug „Bereich“ aufziehen → „Bereich → Vorlage…“ (Boden und/oder Statics) oder Statics auswählen → „Auswahl → Vorlage…“. Name, Kategorie (= Unterordner), Format wählbar. Bereich + **S** wählt alle Statics darin aus.

**Höhle:** Pinsel „Höhle“ – am Fuß des Berges anfangen und hineinziehen; Boden (`0245`–`0249`) auf einer Höhe, Rand wird Wand (Fels-Textur dahinter). „Bereich mit Schwarz (0244) füllen“ für den Dungeon-Bereich.

**Wege / Gebäude-Böden:** Reiter Land → „Aus der Farbtabelle“ (alle Gruppen des Regelsatzes, z. B. cobblestones, marble floor, Dark Wood Floorboards) + „Höhe vom ersten Klick übernehmen“. Mehrere IDs werden zufällig gemischt.

Neue Dateien: `Core/MapTemplate.cs`, `Dialogs/SaveTemplateDialog.cs` (+ Designer), `DragonMapStudioForm.Templates.cs`, `DragonData/Multis/…`.

---

## Version 9: Sprache umschalten – Deutsch / English / Français

* Menü **„Sprache / Language / Langue“** (oder Einstellungen → „0 Sprache / Language“) schaltet die ganze Oberfläche sofort um:
  Menüs, Werkzeugleisten, Reiter, Knöpfe, Tooltips, Einstellungs-Namen und -Beschreibungen, Tasten-Hilfe unter der CGI-Ansicht, Dialoge.
  Die Wahl wird gespeichert.
* Hilfe-Reiter: Deutsch, English und neu Français.
* Übersetzungen liegen als Textdateien in `DragonData/Lang/en.txt` und `fr.txt` (eine Zeile pro Text: `Deutsch<TAB>Übersetzung`).
  Fehlende Texte bleiben deutsch und lassen sich dort einfach ergänzen oder verbessern – auch weitere Sprachen nach demselben Muster sind möglich.
* Noch deutsch: Status- und Log-Meldungen mit Zahlen darin (Logs sind ohnehin zweisprachig DE/EN).

**English:** menu „Sprache / Language / Langue“ switches the whole interface (German, English, French); translation tables in `DragonData/Lang` can be edited.
**Français :** le menu „Sprache / Language / Langue“ change toute l'interface (allemand, anglais, français) ; les tables de traduction dans `DragonData/Lang` sont modifiables.

---

## Version 10: 8 Bit (Dragon) oder 16 Bit (bis 65 536 Farben)

| | 8 Bit (klassisch) | 16 Bit (erweitert) |
|---|---|---|
| Farbadressen | 256 (`00`–`ff`) | bis 65 536 (`0000`–`ffff`), in Seiten zu 256 |
| Gruppen | bis `fe` | bis `7fff` |
| Karten-Datei | 8-Bit-BMP | 16-Bit-PNG (Graustufen = Adresse, verlustfrei, mit anderen Programmen lesbar) |
| maptrans / groups / betweentrans … | 2-stellige Adressen | 4-stellige Adressen |
| mit altem Dragon austauschbar | ja | nein (Import alter Daten geht immer) |

* **Umschalten:** Regeln → „Regelsatz auf 16 Bit umstellen“ (jeder weitere Klick = eine neue Seite mit 256 Farben),
  Bearbeiten → „Karte in 16 Bit umwandeln“, „Neue Karte“ → Häkchen „16 Bit“.
  „Neue Farbe definieren“ bietet 16 Bit an, wenn alle 256 Farben belegt sind; eine Farbe über `ff` in einer 8-Bit-Karte fragt nach.
* **Paletten** im Editor und in der Farbtabelle: „Seite“ bzw. Mausrad blättert durch je 256 Farben.
* **Speichern:** `.bmp` = 8 Bit (nur wenn alle Farben ≤ `ff`), `.png` = 16 Bit. Die Kartenliste zeigt beide.
* Intern speichert der Editor jetzt 16 Bit pro Tile (auch bei 8-Bit-Karten) – doppelter Arbeitsspeicher, dafür ein einziger Codepfad.
* 4-stellige Adressen in Skripten stellen den Regelsatz beim Laden automatisch auf 16 Bit.
* Neue Gruppen brauchen eigene Übergangsdateien (sonst „Regel fehlt“ im Log) und ggf. Items – wie bisher im Reiter „Regeln & Skripte“.

---

## Version 11: Regeln-Werkstatt, Skript-Editor, Vorschau beim Überfahren

**Meldungen beim Laden behoben** – es waren Fehler in den IMod13-Skripten:

| Meldung | Ursache | Korrektur |
|---|---|---|
| Farbe 1e: keine Gruppe | Zeile `1e 01` fehlte in `groups.txt` (Wald Z 40) | ergänzt |
| Farbe 7e: keine Gruppe | Tippfehler `7f 08` statt `7e 08` (Dschungel Z 40) | korrigiert (7f bleibt Höhleneingang) |
| statics/cave2blackRalls.txt fehlt | Tippfehler für `cave2blackWalls.txt` | korrigiert |
| statics/grass2wasteland.txt fehlt | Datei gibt es im IMod13-Paket nicht | Zeile auskommentiert |

Für eigene / gespeicherte Regelsätze: **„Hinweise beheben“** ordnet Farben ohne Gruppe zu (gleiche Tiles) und leitet Regeln mit fehlender Datei auf eine ähnlich benannte Datei um bzw. schaltet sie aus.

**Reiter „Regeln & Skripte“** – farbige Leiste oben:
* **Format 8 Bit / 16 Bit** (aktives Format farbig hervorgehoben; bei 16 Bit fügt ein weiterer Klick eine Seite mit 256 Farben hinzu).
* **+ Neue Farbe…** (auch Doppelklick auf eine Farbe): Adresse mit frei/belegt-Prüfung, Name, Kategorie, RGB mit Prüfung auf doppelte Farben („Eindeutig machen“), Gruppe (oder neue), Höhe, Land-Tiles mit Katalog und Vorschau. Die Farbe ist sofort zum Zeichnen verfügbar.
* **+ Neue Gruppe…** (nächste freie Nummer), **+ Neue Übergangsregel…** mit Beispiel-Datei (12 Standard-Muster als Bild erklärt).
* **Prüfen** (doppelte RGB-Farben, fehlende Gruppen/Dateien), **Hinweise beheben**, **Skript-Editor…**
* **Name + Speichern**: der Regelsatz wird unter seinem Namen in `Dokumente\DragonMapStudio\Regeln` gespeichert und beim nächsten Start geladen.
* Rechts: Hilfe mit Beispielen zum gewählten Unter-Reiter.

**Vorschau beim Überfahren** einer Farbe (Farbtabelle, Paletten im Editor und im Reiter Farbtabelle): Land-Tiles/Texturen, Items der Gruppe, Statics der Übergänge – unter der Zeile, damit sie sichtbar bleibt.

**Skript-Editor** (`Dialogs/ScriptEditorForm`, auch Menü Regeln): alle Skript-Dateien als Text, Suche, Speichern mit `.bak`; rechts die Bedeutung der aktuellen Zeile (Farbe, Gruppe, Tiles/Statics als Bilder, Muster als 3×3-Bild), unten Syntax und Beispiel des Dateityps. Nach dem Speichern werden die Regeln auf Wunsch neu eingelesen.

Neue Dateien: `Core/RuleSetTools.cs`, `Controls/ColorPreviewPopup.cs`, `Dialogs/ColorDefinitionDialog`, `Dialogs/GroupDialog`, `Dialogs/TransitionRuleDialog`, `Dialogs/ScriptEditorForm` (jeweils mit `.Designer.cs`), `DragonMapStudioForm.RulesUi.cs`.

## Höhleneingänge in der CGI-Ansicht

So baut Ultima Online einen Höhleneingang (Original-Karte, Dragon/IMod13 `map/cave-enti2mountain.txt`):

| Tile | Bedeutung (A = Eingang, B = Berg) |
|------|-----------------------------------|
| `01DB` | schwarzer Mund (begehbar; Dragon-Gruppe 1a) |
| `0002` | unsichtbarer Mund (Dragon-Gruppe 1d) |
| `01DA` / `01D6` | Berg im Westen / im Osten |
| `01D8` / `01D4` | Berg im Süden / im Norden (`01D4` +5…15 höher) |
| `01D3` `01D5` `01D7` `01D9` | Ecken (SO / SW / NW / NO), teils +5…15 höher |

Die offene Seite (zum flachen Land) bekommt keinen Übergang – das ist gewollt. Eine echte „Unterwelt“ gibt es in UO nicht: dahinter wird entweder mit dem Pinsel **Höhle** weitergegraben (Boden `0245`–`0249`, Rand wird Fels-Wand) oder auf dem Server ein Teleporter in den Dungeon-Bereich gesetzt (x > 5119, Rest mit Schwarz `0244`).

**Pinsel „Eingang“** (Leiste Land, Reiter „Höhle / Weg“): am Fuß des Berges auf der Bodenhöhe klicken und 2–3 Tiles in den Hang ziehen. Jede Zelle wird Mund (`01DB`, oder `0002` mit „Eingang unsichtbar“), danach bekommt jede Eingangs-Zelle mit Berg-Nachbarn ihr Rand-Tile aus der Regel-Datei (fehlt sie: eingebaute Kopie). Als Berg zählt ein Nachbar mit Tile der Berg-Gruppe (Generator → Berg-Gruppe) oder einer, der mind. 5 höher liegt.

**Mund-Höhe** (Standard 20): Der Mund liegt so viel über dem Boden des ersten Klicks. Wie im Original wird das schwarze Tile dadurch als steile Wand in den Hang gezogen (Bogen-Form) statt flach auf dem Boden zu liegen. Sichtbar ist das nur, wenn der Eingang nach **Süden oder Osten** zeigt – die Kamera schaut von Südosten.

**Höhlen-Tiles (Galerie im Reiter „Höhle / Weg“):** alle Land-Tiles, deren TileData-Name „cave“ enthält, dazu die bekannten (`0002`, `01D3`–`01DE`, `0244`–`0249`) und **eigene IDs** (Feld „Eigene IDs“ oder „+ Tile“, wird in den Einstellungen gespeichert). „Texturen zeigen“ zeigt die Textur, mit der das Tile am Hang gezeichnet wird. Doppelklick = Land-Pinsel, „→ Höhlenboden“ nimmt das Tile in den Höhlen-Pinsel, „→ Eingang (Mund)“ nimmt es als Mund für den Eingang-Pinsel.

**Eingangs-Muster:** einen fertigen Eingang (z. B. aus der Original-Karte) mit dem Werkzeug „Bereich“ samt etwas Boden davor markieren, „Bereich → Muster“ speichert ihn als Vorlage im Ordner `Höhleneingang` (Land-Tiles + Höhen relativ zum Anker). „Muster setzen“ / Doppelklick: Klick auf den Boden vor einem Berg setzt ihn mit allen Höhen.

**Vorderkante unsichtbar** (an): Wie im Original (im Client als „NO DRAW“ zu sehen) wird die vordere Reihe des Mundes – die Zellen, die direkt an den flachen Boden grenzen – zum unsichtbaren Tile `0002` auf Bodenhöhe. Dadurch reicht die schwarze Wand dahinter bis auf den Boden. Davor am besten einen Erdweg (Reiter Land, Erde) mit „Höhe vom ersten Klick“ malen.

Korrekturen am Pinsel **Höhle**: nur Land, das schon über dem Boden liegt, wird zur Wand (vorher wurde auch der Boden vor dem Eingang zu einem Wall angehoben), und Boden aus einem früheren Zug auf anderer Höhe wird nicht mehr zur Wand.

## Höhlen-Stempel (Reiter „Höhlen-Stempel“)

**So ist ein Eingang in der Original-Karte gebaut** (ausgewertet aus der CGI-Ansicht mit „ID + Höhe + Textur“, Höhen über dem Boden davor):

Nach Süden (Berg im Norden), Spalten: Rahmen links | Mund × 3 | Rahmen rechts

| Reihe | Tiles | Höhe |
|-------|-------|------|
| oben (2. Bergreihe) | `01D3` · `01D4 01D4 01D4` · `01D5` | +30 … +37 |
| Mund (1. Bergreihe) | `01DA` · `01DB 01DB 01DB` · `01D6` | Rahmen +10 … +20, Mund +17 … +25 |
| vorn (1. Bodenreihe) | `01E6` · `01DF 01DF 01DF` · `01E4` | Boden (Übergang Höhle → Erde, `cave2dirt.txt`) |
| davor | Erde `0071`–`0078` | Boden |

Auf jedem Tile der Mund-Reihe liegt ein unsichtbares Static („NO DRAW“) auf Bodenhöhe. Neben den Rahmen liegt Fels (z. B. `00DF`) mit etwa +12 … +16.

Nach Osten (Berg im Westen) genauso gedreht: oben `01D3` · `01DA …` · `01D9`, Mund `01D4` · `01DB …` · `01D8`, vorn `01E7` · `01DD …` · `01E4`.

Alle Tiles haben eine eigene Textur (Textur-ID = Land-ID), am Hang zeichnet der Client also die Textur.

Der Stempel baut genau dieses Raster. **Ziehen:** unten am Rahmen anfangen und schräg nach oben zur anderen oberen Ecke ziehen. Der Abstand entlang der Wand ergibt die Breite (Start und Ende sind die Rahmen), die Höhe am Ende des Zuges die Höhe der oberen Reihe. Mund und Rahmen werden im selben Verhältnis angepasst.

**Varianten** (Einstellung „Variante“, alle drei kommen in der Original-Karte vor):
* **Original** – schwarzer Mund, Übergangsreihe `01E6/01DF/01E4`, Erdweg davor.
* **Ohne Weg** – schwarzer Mund direkt auf dem Boden, das Gras davor bleibt.
* **Unsichtbar** – Mund `0002` mit Höhlenboden `0245`–`0249` davor (wie innen im Dungeon).

**Übergänge** (`Core/LandTransitions.cs`): Nach dem Stempeln werden die Map-Übergänge des Dragon-Regelsatzes rund um den Eingang neu gesetzt: Gras → Erde (`0083`, `0086`, `0087`, `0089`, `007E` …) und Berg → Erde (`00DF`, `06F2` …), wie im Original. Jedes Land-Tile wird dazu seiner Gruppe zugeordnet (Farbtabelle + Übergangsdateien). Das geht auch für beliebige Bereiche: Werkzeug „Bereich“, markieren, **T** – z. B. nach dem Malen einer Straße.

**Entfernen:** Strg+Klick mit dem Höhlen-Stempel auf einen Eingang – Mund, Rahmen und obere Reihe werden wieder Berg (Höhe aus dem Berg drumherum), die Vorderreihe wird Boden, die NODRAW-Statics werden gelöscht, danach Übergänge neu.

**Eigene Stempel:** einen fertigen Eingang mit „Bereich“ markieren und „Bereich → speichern“ (Vorlagen-Ordner `Höhleneingang`). Die Liste „Gespeicherte Stempel“ setzt ihn mit Land, Höhen (relativ zum Boden) und Statics wieder.

### Höhleneingänge im 2D-Editor und im Generator

* Werkzeug **„Höhle“** in der Leiste des Karten-Editors: neben einen Berg klicken, dessen Süd- oder Ostseite an Boden grenzt. Gemalt werden Farben: obere Reihe + Mund-Reihe = Gruppe „Höhleneingang schwarz (01DB)“ (Höhe Boden + 20), davor Höhlenboden (Gruppe 03), daneben und davor Erde (Gruppe 09). **Umschalt+Klick** = unsichtbare Variante (Gruppe 1d), **Rechtsklick** = Eingang wieder Berg.
* Fehlt die Gruppe „Höhleneingang schwarz (01DB)“, wird sie angelegt (nächste freie Gruppe, 3 Farben mit Höhe 10 / 20 / 30, Tile `01DB`), dazu die Regeln Eingang → Berg (`map/cave-ent2mountain.txt`) und 03 → 09 (`map/cave2dirt.txt`). Mit „JSON speichern“ / „Als Dragon-Skripte exportieren“ dauerhaft machen.
* Beim Bauen entsteht daraus genau das Original-Raster: `01D3/01D4/01D5` (+26…34), `01DA/01DB/01D6` (+20), `01E6/01DF/01E4`, daneben `00DF`, weiche Gras-Erde-Kanten. Die Build-Option **„Höhleneingang: NODRAW-Statics“** setzt das unsichtbare Static `0001` auf jedes Tile der Mund-Reihe.
* **Generator:** „Höhleneingang wie Original“ (an) setzt die Eingänge mit diesem Stempel an geraden Bergseiten nach Süden / Osten, aus = unsichtbare 2×2-Eingänge (Gruppe 1d) wie bisher.

**Bereich auswerten:** Werkzeug „Bereich“, mit der linken Maustaste ein Viereck ziehen, dann **L** drücken (oder den Knopf im Reiter). Das Log wird als Datei gespeichert **und in die Zwischenablage kopiert**.

Ein Klick auf den Boden direkt vor einer Bergwand (Süd- oder Ostseite) setzt einen kompletten Eingang (`Core/CaveEntranceBuilder.cs`):

1. **Boden und Berg werden am Tile erkannt**, nicht an der Höhe: Berg = Berg-Gruppe der Farbtabelle + Berg-Pinsel-Tiles + alle Land-Tiles mit „mountain“ / „rock“ im TileData-Namen. Alles andere (Gras, Erde, Sand …) ist Boden – auch wenn es höher liegt.
2. **Höhlenboden = Höhe des Bodens davor** (Mittelwert über die Breite des Mundes).
3. Vordere Reihe (erste Bergreihe): `0002` auf Bodenhöhe – der Client zeichnet Land `0000`–`0002` nicht.
4. Hintere Reihe(n): Rand-Tiles `01D3`–`01DA` nach `map/cave-enti2mountain.txt`, bis zur **Mund-Höhe** angehoben. Ist der Berg dahinter zu niedrig, wird er angehoben („Berg anheben“).
5. Davor ein Weg (Standard Erde `0071`–`0078`, 2 Tiles) auf Bodenhöhe.

Einstellungen: Richtung (Auto / Süden / Osten), Breite, Tiefe, Mund-Höhe, Berg anheben, Weg-Länge, Weg-Tiles, Weg-Breite. Sie werden gespeichert.

**Rahmen** (an): links und rechts vom Mund je eine Spalte Rahmen-Tiles (`01DA` / `01D6`, hinten die Ecken `01D3` / `01D5`). Die Vorderkante hat dann genau „Breite“ unsichtbare Tiles – wie die Reihe „NO DRAW“ im Original.

**Ziehen statt Klicken:** Maus auf den Boden an einer Seite des Mundes, gedrückt halten, zur anderen Seite ziehen = Breite. Endet der Zug oben an der Bergwand, wird die Höhe des Tiles dort die Mund-Höhe. Während des Ziehens zeigt die Ansicht „Breite x, Höhe y“.

**Infos einblenden:** Leiste oben „Höhen“ + Auswahl daneben: Höhe, Land-ID (hex), ID + Höhe, ID + Höhe + Textur-ID.

**Bereich auswerten → Log** (Reiter Höhlen-Stempel): einen Eingang, eine Straße oder einen Bergrand mit „Bereich“ markieren und auswerten. Das Log `cgi_bereich_<x>_<y>_<b>x<h>.txt` (Log-Ordner) enthält:
* alle Land-IDs mit Anzahl, Klasse (Berg / Boden / Eingang / unsichtbar), Textur-ID, Radar-Farbe, TileData-Flags und Name,
* Raster mit Land-ID, Höhe, Textur-ID, Klasse und „wie gezeichnet“ (Textur am Hang / Land-Bild flach / gar nicht),
* für jedes Eingangs- und unsichtbare Tile das 8-Nachbar-Muster wie in den Dragon-Dateien,
* alle Statics mit Lage, Höhe über dem Land, Hue, Flags und Name.

Damit kann man Eingänge, Straßen und Bergränder aus der Original-Karte auswerten und nachbauen.

**Zeichnen wie der Client** (aus dem ClassicUO-Quellcode, `Land.ApplyStretch`):
* Ein Land-Tile wird mit seiner **Textur** gezeichnet, sobald irgendeine der 12 Höhen um es herum abweicht und die Textur existiert.
* Sonst – auch am Hang, wenn es keine Textur hat – wird das **Land-Bild flach** auf der Tile-Höhe gezeichnet. Vorher hat die Ansicht das Land-Bild am Hang verzerrt.
* Land `0000`–`0002` wird nicht gezeichnet. In der Ansicht ist es gelb gestrichelt umrandet (abschaltbar).
* „Texturen auch auf flachem Land zeigen“ ist eine reine Vorschau: so sieht man die Textur, die am Hang benutzt würde.

Der Höhle-Pinsel macht nur noch **Berg-Tiles** zur Wand. Gras daneben bleibt Boden, auch wenn es höher als der Höhlenboden liegt.

## Berge in der CGI-Ansicht (Reiter „Berg“)

* **Pinsel „Berg“** (Leiste Land): am Fuß klicken und ziehen. Der Berg steigt ab der Bodenhöhe des ersten Klicks (oder „Fuß-Z“, wenn „Fuß automatisch“ aus ist) um „Steilheit“ Z pro Tile bis „Gipfel-Z“. Pinsel-Größe oben = Breite. Mehrmals ziehen baut einen Grat (mit „Nur anheben“ wird nie etwas abgesenkt).
* **Bereich + M** (oder Knopf „Markierten Bereich zum Berg formen“): die ganze markierte Fläche wird ein Berg – der Rand ausgefranst, innen Kuppen und Täler, Felskanten durch „Rauheit“.
* Einstellungen: Gipfel-Z (Standard 90 wie Dragon-Gruppe 06), Steilheit, Hügel-Größe / -Stärke, Rauheit, Rand ausfransen, Berg-Tiles (Standard `022c`–`022f`, leer = Berg-Gruppe der Farbtabelle), optional Fuß-Tiles für die äußerste Reihe, Zufalls-Zahl. Werden in den Einstellungen gespeichert.
* Danach mit „Eingang“ den Höhleneingang an der Süd- oder Ostseite setzen.

Im 2D-Editor: werden Farben der Gruppe 1d von Hand gemalt, aktiviert „MUL erzeugen“ / „CGI-Karte bauen“ jetzt die Regel 1d → Berg selbst (vorher nur der Generator).

## CGI-Ansicht: neue Seitenleiste

* **Kartenübersicht** als aufklappbarer Abschnitt, Knöpfe im Kopf: **−** / **+** zoomen, **↕** Größe klein / mittel / groß, **⧉** in ein eigenes Fenster abdocken (Fenster schließen = wieder andocken). In der Übersicht: Mausrad = Zoom um den Mauszeiger, rechte Taste ziehen = schieben, Doppelklick rechts = ganze Karte, Klick links = hinspringen. Gezoomt folgt sie der Ansicht. Das Übersichtsbild ist jetzt bis 1024 Pixel groß (vorher 320), dadurch bleibt es beim Zoomen scharf. Gilt auch für die Übersicht im Karten-Editor.
* **Gehe zu** aufklappbar.
* Statt zwei Reihen Reiter: **farbige Kategorien mit Bildern** – Bauen (Items, Multis, Vorlagen), Gelände (Land, Küste / Wasser, Berg), Höhlen (Höhle / Weg, Höhlen-Stempel), Prüfen (Auswahl, Probleme). Die Bilder sind Land- / Item-Grafiken aus den Client-Dateien (ohne Client-Dateien ein farbiger Buchstabe).
* Die Reiter **Höhle / Weg**, **Höhlen-Stempel** und **Berg** bestehen aus aufklappbaren Abschnitten (Werkzeug, Einstellungen, Ansicht, Auswerten, Gespeicherte, Hilfe). Die Hilfe ist zu Beginn zugeklappt. Welche Abschnitte offen sind, wird gespeichert.
* **Höhlen-Stempel → Vorschau:** zeigt das Raster der gewählten Variante mit den echten Tile-Bildern, IDs und Höhen.
* Die Seitenleiste ist breiter (der Trenner lässt sich weiter ziehen, mindestens 300 Pixel).

## Karten-Editor: neue Seitenleiste

* **Kartenübersicht** oben wie in der CGI-Ansicht: aufklappbar, **−** / **+** / Mausrad = Zoom, rechte Taste = schieben, **↕** Größe, **⧉** abdocken. Übersichtsbild bis 768 Pixel.
* **Karten (Projekt-Ordner)** aufklappbar.
* **Werkzeug „Höhle“** (zu Beginn zugeklappt): Einstellungen des Höhleneingangs im 2D-Editor (Breite, Mund über Boden, Gruppen, Weg-Länge, unsichtbar).
* **Farben** füllen den Rest; die Kopfzeile klappt sie ein und aus.

## Fehler behoben: Generator → Karten-Editor (aktuelle Karte)

Mit „aktuelle Kartengröße benutzen“ wurde nur die **obere Hälfte** der erzeugten Karte in die geöffnete Karte kopiert. Unten blieb die alte Karte stehen, deshalb war eine harte Kante quer durch die Karte zu sehen. Ursache: `Buffer.BlockCopy` zählt Bytes, die Karte hat aber 2 Byte pro Pixel (16-Bit-Farbadressen). Jetzt wird mit `Array.Copy` kopiert. Derselbe Fehler steckte in **„Karte reparieren“** und ist dort auch behoben. Der Generator selbst rechnet richtig: Vorschau und volle Größe stimmen zu 96–100 % überein, die Abweichungen sind nur feine Details.

## Karten-Editor: Pinsel-Katalog, Info, Farb-Stempel, CGI-Vorschau, Kontextmenü

Seitenleiste von oben nach unten (jeder Abschnitt auf- und zuklappbar):

* **Farbe unter der Maus / Pipette:** Farbfeld, Adresse, Name, Gruppe, Höhe, Tiles und die Land-Bilder dieser Tiles. Folgt der Maus. Mit der Pipette (I) bleibt die aufgenommene Farbe stehen (✔), und sie wird im Katalog markiert.
* **Pinsel-Katalog:** alle definierten Farben nach Gruppe, mit Kategorie (Gras & Wald, Wasser & Küste, Berg & Fels, Wege & Straßen, Hausböden, Höhle & Dungeon, Sonstiges), Farbfeld und Höhe. Auswahl der Kategorie + Suche (Name, Gruppe, Hex). Links = Farbe 1, rechts = Farbe 2, Doppelklick = Pinsel. Beim Überfahren: Gruppe, Höhe, Tiles. Selbst definierte Farben erscheinen automatisch.
* **Bereich: CGI-Vorschau & Farb-Stempel:**
  * „Bereich markieren“ = Werkzeug Auswahl (S); die Markierung wird nicht in die Karte gemalt.
  * **„Auswahl → CGI-Vorschau“** (auch Taste **V**, Kontextmenü oder der braune Knopf **▶ CGI-Vorschau** in der Werkzeugleiste): baut nur den markierten Bereich (+ 8 Tiles Rand für die Übergänge, höchstens 512×512) im Speicher und zeigt ihn in einem eigenen CGI-Fenster (Raster, Infos ID / Höhe / Textur, Zoom, „Neu bauen“). Die Karte und die große CGI-Ansicht bleiben unverändert. Ohne Auswahl: 128×128 um die Bildmitte.
  * **„Auswahl → Stempel speichern“:** speichert den Ausschnitt als Farb-Stempel (`<Vorlagen-Ordner>\Farbstempel\*.dmstamp.json`). Jedes Pixel merkt sich Farbe, Gruppe und Höhe relativ zum Boden des Stempels (Median des Randes, Wasser ausgenommen).
  * **„Stempel setzen“** / Doppelklick: der Stempel hängt an der Maus, jeder Klick setzt ihn (ein Rückgängig-Schritt je Klick), rechte Taste = fertig. Mit **„Höhen an den Boden anpassen“** bekommt jedes Pixel die Farbe seiner Gruppe mit der passenden Höhe zum Boden an der Zielstelle – ein auf Höhe 0 gebauter Hausboden passt so auch auf ein Plateau in Höhe 20.
* Karten (Projekt-Ordner), Werkzeug „Höhle“, Farben (die lange Farbtabelle ist zu Beginn zugeklappt).

**Kontextmenü:** rechte Maustaste mit Auswahl, Hand oder Pipette – oder **Umschalt + rechte Taste** mit jedem Werkzeug. Inhalt: Farbe an dieser Stelle (als Farbe 1 / 2, in der Farbtabelle zeigen), Rückgängig / Wiederholen, CGI-Vorschau, Stempel speichern, Kopieren / Ausschneiden / Einfügen / Füllen, Werkzeug wechseln, hier in der CGI-Ansicht öffnen.

Rückgängig / Wiederholen gab es schon (Strg+Z / Strg+Y, Leiste, Menü Bearbeiten) – jetzt auch im Kontextmenü. Höhen: Werkzeuge „Höhe +“ / „Höhe −“ wechseln innerhalb der Gruppe die Farbe; Straßen, Wege, Hausböden und Küsten malt man mit den Farben aus dem Katalog – die Übergänge macht der Compiler (in der CGI-Vorschau sofort zu sehen).

Im Fenstertitel steht jetzt `[Stand 21.11.2026 – Editor 8]`.

## CGI-Ansicht → Farben: Stempel aus gebauten Karten, ganze Karte zurück in Farben

In der CGI-Ansicht mit den Werkzeugen Land (Land-Tiles / Texturen), Items, Multis und Vorlagen bauen – oder eine andere Karte laden (MUL / UOP) und dort etwas markieren. Die beiden Knöpfe stehen ganz links in der **oberen Werkzeugleiste der CGI-Ansicht** (grün **„▣ Bereich → Farb-Stempel“**, blau **„▤ Karte → Farbkarte“**); mit dem Werkzeug „Bereich“ geht der Stempel auch mit der Taste **P**. Dieselben Knöpfe gibt es zusätzlich im Reiter Höhlen-Stempel → „In Farben umwandeln“:

* **„Markierten Bereich → Farb-Stempel…“** (vorher mit „Bereich“ markieren): Name, Kategorie, Statics-Art, Art für fehlende Farben wählen. Der Stempel landet im Ordner `Farbstempel` und erscheint im Karten-Editor (Stempel-Menü, Abschnitt Stempel). Ein Bericht wird als `cgi_stempel_<Name>.txt` im Log-Ordner gespeichert.
* **„Ganze Karte → Farbkarte (Karten-Editor)…“:** die ganze CGI-Karte wird zur Farbkarte mit Statics-Ebene (z. B. die Original-Karte zum Nachbauen oder zur Fehlersuche). Bericht: `cgi_karte_zu_farben.txt`.

**So wird umgewandelt** (`Core/CgiToColor.cs`), für jedes Land-Tile mit seiner Höhe:
1. Eine Farbe, deren Tiles das Tile enthalten und deren Höhe höchstens 4 abweicht.
2. Ein Übergangs-Tile einer Regel (z. B. Gras → Erde) → die Zellgruppe der Regel (der Compiler macht den Übergang wieder).
3. Tiles, die der Compiler selbst setzt (Meeresgrund `0064` der Küste) → Wasser-Gruppe.
4. Sonst je nach Einstellung **„Fehlende Farben“**:
   * **Vorschlag + Bestätigen (empfohlen):** Liste mit Tile, Höhe (in 5er-Schritten), Anzahl, TileData-Name, Radar-Farbe und Gruppe (eine vorhandene Gruppe mit gleichem Tile-Namen oder eine neue, Name änderbar). Angehakte werden als neue Farben angelegt (Farbe = Radar-Farbe, eindeutig gemacht; Kategorie „Aus CGI“).
   * **Automatisch anlegen:** dasselbe ohne Nachfrage.
   * **Nur vorhandene Farben:** die Farbe, deren Tile die ähnlichste Radar-Farbe hat (und die passende Höhe).
   Neue Farben / Gruppen mit „JSON speichern“ / „Als Dragon-Skripte exportieren“ dauerhaft machen.

**Statics** (Einstellung „Statics“):
* **Statics-Ebene (empfohlen):** alle Statics 1:1 (Lage, Höhe über dem Land, ID, Hue). Beim Stempeln im Karten-Editor kommen sie in die **Statics-Ebene** der Karte (`<Karte>.statics.bin` neben der Karte, wird mit der Karte gespeichert, umbenannt und mit Strg+Z / Strg+Y zurückgenommen). Ab Zoom 2 als kleine orange Quadrate sichtbar. Beim Bauen (MUL erzeugen, CGI-Karte bauen, CGI-Vorschau) setzt der Compiler sie auf die gebaute Landhöhe.
* **Nur Land:** Statics weglassen.
* **Dragon-Item-Regel:** alle Statics als eine Item-Gruppe in `items/stamp_<name>.txt` (relativ zur Mitte) und eine Regel für die häufigste Gruppe des Bereichs (Chance 1) – Dragon setzt sie dann zufällig.

Eine **ganze umgewandelte Karte** bekommt eine *vollständige* Statics-Ebene mit absoluten Höhen: beim Bauen setzt Dragon dann keine eigenen Statics dazu (keine Übergangs-Statics, Items, Wasser-, NODRAW-Statics) – sonst wären sie doppelt.

**Test** (Generator-Karte 512×512 bauen → zurück in Farben → wieder bauen, mit dem Regelsatz im Repo): 97,7 % gleiche Tile-Gruppe, 99,4 % Höhe ±3, alle 32.787 Statics identisch (Lage, Höhe, ID, Hue). Abweichungen nur bei Gruppen mit denselben Tiles (Gras / Wald, Sand / Wüste) – gebaut ergibt das dasselbe Tile.

### Stempel-Menü im Karten-Editor

Knopf **„▦ Stempel-Menü“** in der Werkzeugleiste: alle Farb-Stempel als Bilder (Farben, Statics als orange Punkte), Kategorie-Auswahl, Suche, „Höhen an den Boden anpassen“, Setzen / Löschen / Ordner / Neu laden.
* Mit **mehreren Monitoren** öffnet es sich als **eigenes Fenster** (auf dem anderen Monitor), mit einem Monitor als **Leiste rechts** neben der Karte.
* **⧉** im Kopf: Fenster ↔ Leiste. **Fenster schließen** = es kommt als Leiste rechts zurück. **✕** blendet es aus.

Im Fenstertitel steht jetzt `[Stand 21.11.2026 – Stempel 10]`.
