# Sprite-Sheet Import / Export für Ultima-Online-Art – Bauanleitung für eine KI

> **An die KI, die dieses Dokument bekommt:** Baue die hier beschriebenen Funktionen in das Tool deiner
> Nutzerin ein. Das Dokument ist **sprachunabhängig** (C#, Python, JS … egal). Die Funktionen sind in
> **Module** aufgeteilt (Kapitel 2) – die Nutzerin wählt, welche sie braucht; die Abhängigkeiten stehen dabei.
> Alles, was als **verbindlich** markiert ist, muss genau so umgesetzt werden, damit Sheets zwischen
> UoFiddlerPixel und ihrem Tool **austauschbar** sind (ein Sheet aus dem einen Tool lässt sich im anderen importieren).
> Referenz-Implementierung (C#, getestet): `jedi661/UoFiddlerPixel`, Dateien
> `UoFiddler.Controls/Classes/ArtSpriteSheet.cs`, `SlotPlanner.cs`, `ArtTransform.cs` (Klasse `UoTile`),
> Oberflächen `Forms/SpriteSheetBatchForm.cs`, `Forms/SpriteSheetPlacementForm.cs`, `Forms/ArtDirectionForm.cs`.

---

## 1. Grundlagen (verbindlich)

### 1.1 Pixel und Transparenz
- UO-Art hat **15-Bit-Farben** (5 Bit je Kanal, ARGB1555). Im PNG werden sie als normale RGB-Farben gespeichert
  (`r8 = r5 * 255 / 31` usw.); beim Einlesen `r5 = r8 >> 3`.
- **Reines Schwarz `#000000` = transparent.** Sichtbare schwarze Pixel gibt es in UO nicht (Farbe 0 ist im
  Client durchsichtig) – beim Export werden transparente Pixel schwarz, beim Import wird Schwarz transparent.
- Beim Einlesen eines PNG mit Alphakanal: Alpha < 128 → transparent.

### 1.2 Lage einer Grafik im UO-Feld (der wichtigste Punkt)
- Ein Feld (Tile) ist eine **Raute 44 x 44** mit **45-Grad-Kanten**: oben (22,0), rechts (44,22),
  unten (22,44), links (0,22).
- Eine Grafik der Größe w x h wird mit ihrem **ganzen Rahmen** (nicht nur den farbigen Pixeln) gesetzt:
  `drawX = tx + 22 - floor(w/2)`, `drawY = ty + 44 - h`.
- **Fixpunkt** = mittlerer Pixel der untersten Bildreihe `(floor(w/2), h-1)` = untere Ecke der Raute.
- Deshalb: **Breite und Höhe einer Grafik gehören zu ihrer Lage.** Beim Ausschneiden darf der Fixpunkt nicht
  verrutschen (Kapitel 4.3). Bei ungerader Breite immer `floor(w/2)`.

### 1.3 Gültige Item-IDs
- Klassisch 0x0000 bis Max (High Seas: 0xFFDC). Optional Erweiterung ab 0x10000 (UoFiddlerPixel-Format).
- IDs **zwischen** klassischem Max und 0x10000 sind **ungültig** und dürfen nie beschrieben werden.

---

## 2. Module – zum Auswählen

| Modul | Funktion | braucht |
|---|---|---|
| **A** | Sheet-Format: zeichnen (Export) | – |
| **B** | JSON-Begleitdatei schreiben / lesen | A |
| **C** | Sheet zerschneiden (Import) mit JSON | A, B |
| **D** | Sheet zerschneiden ohne JSON (Zellen an den Magenta-Linien finden) | A |
| **E** | KI-/Mensch-Prompt erzeugen, Knopf „in die Zwischenablage" | B |
| **F** | Bereich exportieren „von ID bis ID", automatisch auf mehrere Sheets aufteilen | A, B |
| **G** | Planung vorab: Anzahl Sheets, größtes Bild, Warnungen, geschätzte Dauer | F |
| **H** | Fortschritt: verstrichene Zeit, Restzeit, Pause/Weiter, Abbrechen | F oder I |
| **I** | Mehrere Sheets importieren (Dateien / ganzer Ordner) | C oder D |
| **J** | Platzierung: Original-IDs, ab Adresse, nächste freie, Block freier Plätze, Auswahl, von Hand | C oder D |
| **K** | Einzel-Sheet einer Grafik mit ihren 4 Richtungen (Source, North, East, South, West) | A, B, C |

Minimal sinnvoll: **A + B + C** (austauschbar mit UoFiddlerPixel). Für „ganzer Bereich": zusätzlich **F, G, H, I, J**.

---

## 3. Modul A – Sheet-Format (verbindlich)

Eingabe: Liste von Sprites (Label, Item-ID oder -1, Bild oder leer), Spaltenzahl `columns`, Rand `padding` (UoFiddlerPixel: 4).

```
maxW = größte Bildbreite, maxH = größte Bildhöhe (leere Zellen zählen nicht, mindestens 1)
cw = maxW + 2*padding          // Zellbreite (innen)
ch = maxH + padding            // Zellhöhe (innen) – KEIN Rand unten: die unterste Zellreihe ist der Boden
cols = min(columns, Anzahl)    rows = ceil(Anzahl / cols)
sheetW = cols*cw + cols + 1    sheetH = rows*ch + rows + 1

Trennlinien (1 px, Farbe #FF00FF magenta):
  waagerecht: y = r*(ch+1)  für r = 0..rows
  senkrecht:  x = c*(cw+1)  für c = 0..cols
Zelle i: col = i % cols, row = i / cols
  Innenbereich: cx = col*(cw+1) + 1, cy = row*(ch+1) + 1, Breite cw, Höhe ch
Sprite in der Zelle (Fixpunkt auf Fixpunkt):
  sx = cx + floor(cw/2) - floor(w/2)
  sy = cy + ch - h
Hintergrund: schwarz (= transparent). Ein Sprite-Pixel, das exakt #FF00FF wäre, wird minimal verändert
(z. B. #FF00F7), damit es nie als Trennlinie gilt.
```

Ausgabe: PNG (24 Bit RGB reicht, Schwarz = transparent). Dateiname frei, z. B. `name.png` / `name_001.png`.

---

## 4. Module B, C, D – JSON und Import

### 4.1 Modul B – JSON-Begleitdatei (verbindlich, gleicher Name, Endung `.json`)

```json
{
  "Format": "UOFiddler-SpriteSheet",
  "Version": 1,
  "Name": "chest_0xD215",
  "Columns": 5, "Rows": 1,
  "CellWidth": 60, "CellHeight": 74,
  "Separator": 1, "SeparatorColor": "#FF00FF", "TransparentColor": "#000000",
  "Anchor": "Each sprite stands bottom-centred in its cell ...",
  "Cells": [
    { "Index": 0, "Label": "Source", "ItemId": 53781, "Column": 0, "Row": 0,
      "X": 1, "Y": 1, "Width": 60, "Height": 74,
      "SpriteX": 5, "SpriteY": 5, "SpriteWidth": 52, "SpriteHeight": 70 }
  ]
}
```
- `X, Y, Width, Height` = Innenbereich der Zelle im Sheet. `SpriteX/Y/Width/Height` = Original-Rahmen der Grafik
  (0 bei leerer Zelle). `ItemId` = -1 wenn unbekannt. `Label` frei (UoFiddlerPixel: `Source`, `North`, `East`,
  `South`, `West` bzw. `0x1234 name`).
- Feldnamen in PascalCase wie oben (Groß-/Kleinschreibung beim Lesen tolerant behandeln).

### 4.2 Modul D – Zellen ohne JSON finden
- Eine Spalte x ist Trennlinie, wenn **≥ 90 %** ihrer Pixel „magenta" sind; ebenso Zeilen.
- „Magenta" tolerant (Bildprogramme/KIs verfälschen leicht): in 5 Bit `R ≥ 28, B ≥ 28, G ≤ 3`.
- Zellen = Bereiche zwischen benachbarten Trennlinien (Spalten- × Zeilenbereiche), Reihenfolge zeilenweise.
- Gibt es eine JSON, deren Zellen ins Bild passen → JSON benutzen, sonst Linien suchen.

### 4.3 Modul C – Zelle ausschneiden (verbindlich, sonst verrutscht die Grafik im Feld)
```
c      = cell.X + floor(cell.Width/2)          // Fixpunkt-Spalte der Zelle
bottom = cell.Y + cell.Height - 1              // Bodenreihe der Zelle
Inhalt = alle Pixel der Zelle, die nicht schwarz und nicht EXAKT #FF00FF sind
          (innerhalb der Zelle nur exaktes Magenta als Linie werten!)
leer   -> kein Bild
minX, maxX, minY = Grenzen des Inhalts
w = max(1, SpriteWidth aus JSON oder 0)
solange (c - floor(w/2) > minX) oder (c - floor(w/2) + w - 1 < maxX):  w = w + 1
h = max(bottom - minY + 1, SpriteHeight aus JSON falls <= cell.Height, sonst 0)
Bild w x h:  Pixel (x,y) = Sheet(c - floor(w/2) + x, bottom - h + 1 + y), nur innerhalb der Zelle
```
Damit ergibt Export → Import **pixelgenau** das Original (auch bei ungerader Breite), und bearbeitete Sheets
behalten ihren Fixpunkt.

---

## 5. Modul E – Prompt für Mensch/KI (Knopf „Copy AI prompt")
Text in die Zwischenablage (und als `name_prompt.txt` speichern). Inhalt (sinngemäß, Werte einsetzen):
```
You get a sprite sheet with Ultima Online item graphics (pixel art). Please edit it and send it back in EXACTLY the same layout.
SHEET LAYOUT: picture size {sheetW} x {sheetH}, {cols} columns x {rows} rows, each cell {cw} x {ch} px.
Cells are separated by 1 px lines in pure magenta #FF00FF - do not move/recolour them, never use #FF00FF elsewhere.
Pure black #000000 = transparent; do not use pure black for visible pixels (use #080808).
UO ANCHOR: every sprite stands bottom-centred in its cell: the middle pixel of its lowest row is the middle pixel of
the lowest cell row = bottom corner of the 44x44 UO tile diamond (45 degree edges). Do not move sprites unless asked.
Directions: South = front lower left, East = lower right, North = upper right, West = upper left.
PIXEL RULES: no anti-aliasing, blur, soft shadows, semi-transparency or rescaling; 15-bit colours; light from the upper left.
CELLS: (eine Zeile pro Zelle: Index, Spalte, Zeile, Label, Item-ID, Zellbereich, Sprite-Rahmen oder "empty")
Return the edited sheet as PNG with the same size and cells.
```

---

## 6. Module F, G, H – Bereich exportieren

### 6.1 Modul F – von ID bis ID, Aufteilung
- Eingaben: `from`, `to` (Hex `0x…` oder dezimal), „nur Items mit Grafik", `columns` (16), **max. Sheet-Größe**
  in px (4096), **max. Items pro Sheet** (256), Ordner (merken), Basisname.
- IDs sammeln: gültig (1.3) und – falls gewählt – mit Grafik. Reihenfolge = ID-Reihenfolge.
- **Spalten pro Sheet** an das Limit anpassen: `effCols = clamp(columns, 1, floor((maxSize-1) / (maxW + 2*padding + 1)))`.
- **Aufteilen** (der Reihe nach, Reihenfolge bleibt):
```
current = [], maxW = maxH = 0
für jedes Sprite i (w,h):
   wenn Sprite allein (1 Spalte) breiter/höher als maxSize -> in "oversize" merken (Warnung)
   nw = max(maxW,w), nh = max(maxH,h)
   (tw,th) = Sheetgröße(current+1 Zellen, effCols(nw), nw, nh)
   wenn current nicht leer und (tw > maxSize oder th > maxSize oder current+1 > maxCells):
        Sheet abschließen, current = [], nw = w, nh = h
   current += i, maxW = nw, maxH = nh
am Ende letztes Sheet abschließen
```
- Dateien: bei 1 Sheet `name.png`, sonst `name_001.png`, `name_002.png` … jeweils mit `.json`; einmal `name_prompt.txt`.

### 6.2 Modul G – Planung vor dem Export
- Größen schnell aus dem Art-Header lesen (Static-Art: Bytes 4–5 = Breite, 6–7 = Höhe, little endian) –
  ohne alle Pixel zu dekodieren.
- Zusammenfassung anzeigen und bestätigen lassen: Anzahl Items, Anzahl Sheets, größtes Sheet (B x H),
  **Warnungen**: „wird in N Sheets aufgeteilt", „X Items allein größer als das Limit (eigenes Sheet)",
  „größtes Sheet über 64 Mio. Pixel – viele Programme können es nicht öffnen".
- **Geschätzte Dauer**: an ~40 echten Items die Zeit pro Item messen → `Zeit/Item * Anzahl + Sheets * ~0,3 s`.

### 6.3 Modul H – Fortschritt, Pause, Abbrechen
- Arbeit in **kleinen Portionen** (z. B. Timer, pro Tick ~60 ms arbeiten), damit die Oberfläche bedienbar bleibt
  (Art-Zugriffe sind meist nicht thread-sicher – nicht parallel aus Hintergrund-Threads lesen).
- Anzeige: Balken, `fertig / gesamt`, **verstrichen**, **Rest ≈ (gesamt − fertig) / Rate**, Rate pro Sekunde.
  Die Stoppuhr läuft während Pause nicht.
- **Pause/Weiter** (Timer anhalten), **Abbrechen** (Export: angefangenes Sheet noch schreiben, fertige bleiben;
  Import: bisher Eingefügtes bleibt – Hinweis: erst Speichern schreibt die Dateien).

---

## 7. Module I, J – Import und Platzierung

### 7.1 Modul I – mehrere Sheets
- Dateien wählen (Mehrfachauswahl) oder ganzen Ordner (`*.png`), Reihenfolge nach Dateiname.
- Pro Sheet: Bild laden, JSON (falls vorhanden) lesen, zerschneiden (C oder D), Zellen der Reihe nach platzieren.
- Ohne JSON ist „Original-IDs" nicht möglich → Hinweis „ab Adresse" wählen.

### 7.2 Modul J – Platzierung (wähle aus)
| Modus | Regel |
|---|---|
| **Original-IDs** | `ItemId` aus der JSON (nur gültige IDs) – schneller Rück-Import |
| **Ab Adresse, hintereinander** | start, start+1, … (belegte werden ersetzt; nie über ungültige IDs hinweg) |
| **Ab Adresse, nächste freie** | die nächsten freien gültigen IDs ab start (belegte überspringen) |
| **Block N freier am Stück** | erster Block von N freien gültigen IDs ab start; mit ◀ ▶ zum vorherigen/nächsten Block |
| **Auswahl** | die in der Item-Liste markierten Slots (sortiert) |
| **Von Hand** | Ziel-ID pro Grafik eintragen |

- Vor dem Einfügen: **Anzahl belegter Ziele** anzeigen („werden ersetzt") und bestätigen lassen;
  doppelt vergebene IDs und ungültige IDs blockieren.
- Viele Items: ohne Neuzeichnen pro Item einfügen, am Ende einmal aktualisieren.

---

## 8. Modul K – Einzel-Sheet mit Richtungen
- 5 Zellen in einer Zeile: `Source`, `North`, `East`, `South`, `West` (Labels so schreiben).
- Import ordnet nach Label, ohne Labels nach Position in dieser Reihenfolge.
- Mehr als 5 Zellen → als „mehrere neue Items" behandeln (Modul J).

---

## 9. Abnahmetests (bitte alle umsetzen)
1. Export → Import mit JSON ergibt jede Grafik **pixelgleich** (Größe und Pixel), auch bei **ungerader Breite** (z. B. 45 px).
2. Leere Zelle bleibt leer.
3. Import **ohne** JSON findet alle Zellen über die Magenta-Linien und jede Grafik steht auf der Bodenreihe.
4. Ein Pixel exakt `#FF00FF` in einer Grafik verschwindet nicht (wird beim Export minimal verändert).
5. Aufteilung: jedes Item genau einmal, Reihenfolge erhalten, jedes Sheet ≤ Limit und ≤ max. Items; zu große Items allein.
6. Platzierung: „nächste freie" überspringt Belegte; „Block" findet N freie am Stück; nie ungültige IDs.
7. Sheet aus UoFiddlerPixel lässt sich importieren – und umgekehrt (Austauschbarkeit).

## 10. Typische Fehler
- Nur die farbigen Pixel betrachten statt des ganzen Rahmens → Grafik sitzt danach falsch im Feld.
- `w/2` gerundet statt `floor(w/2)` → 1 px Versatz bei ungerader Breite.
- Unter der Grafik Rand lassen → Boden verschoben (Zelle hat unten **keinen** Rand).
- Bilder mit Glättung skalieren → neue Mischfarben; immer Nearest Neighbour / 1:1.
- Zwischengespeicherte Bilder des Tools beim Export freigeben (dispose) → Absturz in der Anzeige.
