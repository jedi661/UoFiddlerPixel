# TileData – Hilfe

Im TileData-Tab bearbeitest du die Eigenschaften der **Items** (Statics) und **Land-Tiles** aus der `tiledata.mul` sowie die **Textdateien von ClassicUO**.

Alle Änderungen landen zuerst **im Speicher**. In die Datei kommen sie erst mit **Save Tiledata** (`tiledata.mul` im Ausgabe-Ordner).

## Aufbau

| Tab | Inhalt |
|---|---|
| Items | Liste aller Items, Bild, Werte (Name, Weight, Height …) und Flags |
| Land Tiles | Liste aller Land-Tiles, Bild, Name, Texture-ID und Flags |
| ClassicUO | Editor für `chair.txt`, `cave.txt`, `tree.txt` … von ClassicUO |

Rote Einträge in den Listen weichen vom Stand der Datei ab. Nach **Save Tiledata**, Rückgängig oder Zurücksetzen werden sie wieder schwarz.

## Mehrfachauswahl

In beiden Listen (Items und Land):

- **Klick** – ein Tile auswählen
- **Strg + Klick** – Tile zur Auswahl hinzufügen oder entfernen
- **Umschalt + Klick** – Bereich vom zuletzt geklickten Tile bis hierher (innerhalb einer Gruppe)
- **Rechtsklick → Select ID range…** – Auswahl über IDs, z. B. `0x0B2C-0x0B40, 0x0C00, 3000..3010`
- **Rechtsklick → Clear selection** – nur noch das angezeigte Tile ist ausgewählt

Ausgewählte Tiles sind hellblau hinterlegt. Die Felder zeigen immer die Werte des zuletzt angeklickten Tiles.

### Gleiche Änderung auf mehrere Tiles

1. Tiles auswählen (Strg/Umschalt).
2. In den Feldern **nur das ändern, was für alle gelten soll**, z. B. Height = 5 und den Haken bei *Surface*.
3. **Save changes** klicken.

**Bei einem Tile** werden alle Felder übernommen.

**Bei mehreren Tiles** werden nur die Felder und Flags übernommen, die du seit dem Anklicken geändert hast. Alle anderen Werte der Tiles (Name, Animation …) bleiben erhalten. Das gilt auch für Vorlagen, *Insert settings* und den Leeren-Knopf: Sie füllen die Felder, und **Save changes** wendet das Geänderte auf alle an.

Mit **Save directly on changes** (Menü) wird jede Änderung sofort auf **alle ausgewählten** Tiles angewendet.

## Mehrfach bearbeiten (Batch edit…)

Für größere Aktionen öffnet **Batch edit…** (Leiste oder Rechtsklick) ein eigenes Fenster.

**Welche Tiles:**
- die ausgewählten Tiles
- ein ID-Bereich (`0x0B2C-0x0B40, 3000..3010`)
- alle Tiles der Liste (mit dem Filter des Tabs)

Zusätzlich kannst du einschränken auf „nur Namen mit …“ und „nur mit Flag …“.

**Werte:** Nur angehakte Felder werden geändert. Ein eingetippter Wert hakt das Feld automatisch an.
- Zahlen: *setzen*, *addieren* oder *abziehen*. Der Wert bleibt in seinem gültigen Bereich (0–255 bzw. ±32767).
- Name: *setzen*, *voranstellen*, *anhängen* oder *ersetzen* (höchstens 20 Zeichen).

**Flags:** *set* setzt das Flag, *remove* entfernt es. Die Spalte *have it* zeigt, wie viele der Tiles das Flag schon haben.

Unten steht, wie viele Tiles sich wirklich ändern. **Apply** führt alles in **einem** Rückgängig-Schritt aus.

## Rückgängig / Wiederholen

- **↶ Undo** / **Strg+Z** und **↷ Redo** / **Strg+Y** (oder Strg+Umschalt+Z)
- Rückgängig geht für alles: Felder, Flags, Save changes, Batch edit, CSV-Import, Vorlagen, Insert settings, Set textures, Zurücksetzen.
- Mehrere Tastendrücke im selben Feld kurz hintereinander zählen als ein Schritt.
- Nach dem Neuladen der Dateien beginnt der Verlauf neu.
- Strg+Y in der Item-Liste bleibt „Push marked“, solange der Merken-Knopf aktiv ist.

## Änderungen und Zurücksetzen

**Changes…** zeigt alle Items und Land-Tiles, die sich vom Stand der Datei unterscheiden, mit den genauen Änderungen (z. B. `Weight 1 → 255; +Wall -Surface`).

- **Show** (oder Doppelklick) springt zum Tile.
- **Revert selected** / **Revert all** setzt Tiles auf den Dateistand zurück. Auch das lässt sich rückgängig machen.
- **Copy list** kopiert die Liste als Text.

**Rechtsklick → Revert to file state** setzt die ausgewählten Tiles direkt zurück.

## ClassicUO-Dateien

ClassicUO liest Zusatz-Infos aus `<ClassicUO>\Data\Client\*.txt`.

Einrichten: Mit **Load** den ClassicUO-Ordner wählen. Der Ordner `Data` oder `Data\Client` geht auch, der richtige Unterordner wird gefunden. Dann die Datei in der Liste wählen.

| Datei | Inhalt | Format |
|---|---|---|
| chair.txt | Sitzmöbel | `GRAPHIC,D1,D2,D3,D4,OFFSET_Y,MIRROR_OFFSET_Y` (Richtung 0/2/4/6, -1 = nicht möglich) |
| cave.txt | Höhlen-Tiles | eine Grafik-ID pro Zeile (dezimal) |
| vegetation.txt | Gras, Büsche … (ausblendbar) | eine Grafik-ID pro Zeile (dezimal) |
| tree.txt | Bäume → Baumstumpf | `GRAPHIC=FLAG` (0/1) |
| containers.txt | Container-Gumps | `GUMP OPEN_SOUND CLOSE_SOUND X Y W H [ICON] [MIN_X] [MIN_Y]` |
| seasons.txt | Jahreszeiten | `SEASON,static/landtile,ORIGINAL,ERSATZ` (nur Kommas, keine Leerzeichen) |
| lights.txt | Licht von Items | `ITEM_ID SHADER` oder `ITEM_ID H<hue>` |
| lightshaders.txt | Lichtfarben | `ID RGB R_CURVE [G_CURVE B_CURVE]` (Kurven Standard, A–E) |

Kommentarzeilen beginnen mit `#` oder `;` (seasons.txt: `#` oder `//`). IDs sind dezimal; nur seasons.txt und lights.txt verstehen auch `0x…`.

**Funktionen im Editor:**
- Rechts steht die Beschreibung der gewählten Datei.
- Unter den Knöpfen steht, worauf sich die Zeile unter dem Cursor bezieht, z. B. `item 0x0B2C "chair"`.
- **Check** prüft die Datei: falsches Format, Grafiken ohne Art, doppelte Einträge. Fehlerhafte Zeilen werden rot markiert.
- **Add selected tiles** fügt die im Items-Tab ausgewählten Tiles als neue Zeilen an, jeweils mit einer Kommentarzeile mit Namen. Was schon drinsteht, wird übersprungen. Danach bei Stühlen die Richtungen, bei Jahreszeiten den Ersatz, bei Licht den Shader eintragen.
- **Rechtsklick in der Item-Liste → Add to ClassicUO file** macht dasselbe direkt aus der Liste (chair, cave, vegetation, tree, seasons, lights). In der Land-Liste: **Add to seasons.txt**.
- **Save Txt** speichert. Vor dem ersten Speichern einer Datei wird `<datei>.txt.bak` angelegt. Bei Formatfehlern wird nachgefragt.
- **Reload** lädt die Datei neu, **Open folder** öffnet den Ordner.
- Beim Wechsel der Datei mit ungespeicherten Änderungen wird gefragt.

ClassicUO liest die Dateien beim Start. Nach dem Speichern den Client neu starten.

## Weitere Funktionen

- **Vorlagen** (Auswahlliste in der Leiste): typische Werte für Wand, Tür, Treppe, Stuhl … Land-Vorlagen („LandTile …“) ändern nur die Land-Flags.
- **Copy / Insert settings** (Rechtsklick): Werte eines Tiles auf ein anderes übertragen. Bei mehreren ausgewählten Tiles danach **Save changes**.
- **Merken / Push marked**: Werte merken und per mittlerer Maustaste oder Strg+Y auf andere Items übertragen.
- **Import / Export CSV**: alle Items bzw. Land-Tiles als Tabelle. Der Import ist rückgängig machbar.
- **Set filter**: die Listen nach Werten und Flags filtern.
- **Set textures**: Texture-IDs der Land-Tiles automatisch setzen.
