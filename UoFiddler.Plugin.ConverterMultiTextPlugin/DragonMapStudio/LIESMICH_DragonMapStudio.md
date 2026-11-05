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
