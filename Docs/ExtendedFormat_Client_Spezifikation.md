# UOFiddler Extended Format – Spezifikation & Übergabe für Client/Server

> **Für die KI / den Entwickler, die dieses Dokument bekommen:**
> Dieses Dokument beschreibt vollständig das von *UoFiddlerPixel* eingeführte **erweiterte Item-Format**
> (Item-IDs **ab 0x10000**, standardmäßig bis **0x10FFFF**, maximal 16.777.216 zusätzliche Slots).
> Ziel: einen Client (z. B. **ClassicUO**) und einen Server (z. B. **ServUO / RunUO / ModernUO**) so erweitern,
> dass diese Items angezeigt und benutzt werden können – **ohne** die bestehenden Original-Dateien zu verändern.
> Alles, was hier als *verbindlich* markiert ist, ist im UOFiddler-Code so implementiert und darf nicht abweichen.
> Alles unter *Vorschlag* ist noch nicht implementiert und darf beim Umsetzen gestaltet werden.

Referenz-Implementierung (C#) im Repository `jedi661/UoFiddlerPixel`:

| Datei | Inhalt |
|---|---|
| `Ultima/ExtendedFormat.cs` | Konstanten, Header, Lesen/Schreiben von Index und TileData (ohne GDI, direkt wiederverwendbar) |
| `Ultima/Art.cs` | Laden/Speichern der Bilder (`GetExtended`, `SaveExtension`, `EncodeStatic`, `LoadStatic`) |
| `Ultima/TileData.cs` | `LoadExtension`, `SaveExtension`, `EnsureExtendedCapacity` |
| `Ultima/Files.cs` | Dateinamen in der Pfadliste (alle Zusatzdateien, siehe Kapitel 13) |
| `Ultima/ExtendedBlobStore.cs` | Index + Daten für Land-Tiles und Texturen (Kapitel 13) |
| `Ultima/Textures.cs`, `Ultima/Hues.cs`, `Ultima/RadarCol.cs` | Texturen-, Hue- und Radarfarben-Erweiterung (Kapitel 13) |

> **Neu:** Außer den Items gibt es jetzt auch Erweiterungen für **Land-Tiles, Land-TileData, Texturen, Hues und
> Radarfarben** – nach demselben Prinzip (Original bleibt, Zusatzdateien mit gleichem Header). Siehe **Kapitel 13**.

---

## 1. Grundprinzip (verbindlich)

1. **Die Original-Dateien bleiben unverändert im Client-Layout:**
   `art.mul` + `artidx.mul` (oder `artLegacyMUL.uop`) und `tiledata.mul` enthalten weiterhin genau die
   IDs `0x0000–0xFFFF` mit unveränderter Eintragsanzahl. Ein Original-Client merkt nichts.
2. **Alle IDs ab `0x10000`** stehen ausschließlich in drei **zusätzlichen** Dateien im selben Verzeichnis:
   - `artextidx.mul` – Index der Bilder (64-Bit-Offsets)
   - `artext.mul` – Bilddaten (gleiche Kodierung wie Static-Art in `art.mul`)
   - `tiledataext.mul` – TileData-Einträge (High-Seas-Satzaufbau)
3. **ID-Abbildung:**
   ```
   if (id < 0x10000)  -> klassisch: art.mul / tiledata.mul (Static-Index im artidx = id + 0x4000)
   else               -> k = id - 0x10000  (Slot-Nummer in den Ext-Dateien), gültig wenn k < count
   ```
4. Fehlen die Ext-Dateien oder sind sie ungültig, verhält sich alles wie bisher (**nie** den Start abbrechen).
5. Alle Werte sind **Little Endian**.

Warum Zusatzdateien statt einer größeren `art.mul`: `artidx.mul` hat nur 32-Bit-Offsets (2-GB-Grenze),
Programme erkennen die Version an der Dateigröße (0x4000 / 0xC000 / 0x13FDC Einträge), und das Netzwerk-
protokoll kennt nur 16-Bit-Grafik-IDs.

---

## 2. Gemeinsamer Datei-Header (32 Byte, verbindlich)

Gilt für `artextidx.mul` und `tiledataext.mul` (nicht für `artext.mul`, die hat keinen Header).

| Offset | Typ | Name | Wert |
|---|---|---|---|
| 0 | char[4] | magic | `"UOAX"` (artextidx.mul) bzw. `"UOTX"` (tiledataext.mul), ASCII |
| 4 | int32 | version | `1` |
| 8 | int32 | baseId | `0x10000` (erste ID der Datei) |
| 12 | int32 | count | Anzahl Slots (Standard 1.048.576 = 0x100000; max. 16.777.216 = 0x1000000) |
| 16 | int32 | recordSize | `16` (artextidx.mul) bzw. `41` (tiledataext.mul) |
| 20 | int32[3] | reserved | `0` |

**Prüfregeln beim Lesen (so in `ExtendedFormat.ReadHeader`):** magic stimmt, `1 <= version <= 1`,
`baseId == 0x10000`, `recordSize` passt, `0 <= count <= 0x1000000`. Sonst Datei ignorieren.
Ist die Datei kürzer als `32 + count * recordSize`, gelten die fehlenden Slots als **leer**.

---

## 3. `artextidx.mul` (verbindlich)

Nach dem Header folgen `count` Einträge à 16 Byte:

| Offset | Typ | Name | Bedeutung |
|---|---|---|---|
| 0 | int64 | offset | Byte-Position in `artext.mul`; **-1 = leer** |
| 8 | int32 | length | Länge der Bilddaten in Byte; `<= 0` = leer |
| 12 | int32 | extra | derzeit `0` (reserviert) |

- Eintrag `k` gehört zur Item-ID `0x10000 + k`.
- Gültig nur wenn `offset >= 0 && length > 0` und `offset + length <= Länge(artext.mul)`.
- **Mehrere Einträge dürfen denselben `offset` haben** (identische Bilder werden nur einmal gespeichert).
- Dateigröße = `32 + count * 16` (bei 1.048.576 Slots: 16.777.248 Byte).

## 4. `artext.mul` – Bildkodierung (verbindlich, identisch zu Static-Art in art.mul)

Jedes Bild ab `offset`, `length` Byte:

```
int32   header        // ohne Bedeutung (UOFiddler schreibt 1234) – ignorieren
uint16  width
uint16  height
uint16  lookup[height]            // Start jeder Zeile, in uint16-Einheiten, gezählt ab DATENBEGINN
                                  // DATENBEGINN = direkt hinter lookup[] (also ushort-Index 4 + height)
// pro Zeile, ab DATENBEGINN + lookup[y]:
repeat {
    uint16 xOffset                // Anzahl transparenter Pixel seit dem Ende des vorherigen Laufs
    uint16 runLength              // Anzahl folgender Pixel
    if (xOffset == 0 && runLength == 0) break;   // Zeilenende
    uint16 pixel[runLength]       // Farbe 15 Bit (X1R5G5B5); Bit 15 = 0 in der Datei
}
```

- Lesen genau wie die Static-Art des Original-Clients (Pseudocode aus `Art.LoadStatic`):
  ```
  start = height + 4                     // in ushorts, relativ zum Bildanfang
  for y: pos = start + lookup[y]; x = 0
     loop: xo = d[pos++]; run = d[pos++]; if xo+run == 0 break
           x += xo; for i<run: pixel(x++, y) = d[pos++] ^ 0x8000   // ^0x8000 setzt das Alpha-Bit
  ```
- Pixel außerhalb der Läufe sind transparent. **Farbwert 0x0000 innerhalb eines Laufs** wird von Clients
  üblicherweise als transparent behandelt – UOFiddler vermeidet ihn (Reparatur ersetzt ihn durch 0x0421).
- Readers dürfen **nicht** annehmen, dass die Zeilen lückenlos hintereinander liegen; immer `lookup[]` verwenden.

## 5. `tiledataext.mul` (verbindlich)

Nach dem Header folgen `count` Datensätze à **41 Byte** (High-Seas-Item-Satz, **ohne** die 4-Byte-Gruppenheader
alle 32 Einträge, die `tiledata.mul` hat):

| Offset | Typ | Feld |
|---|---|---|
| 0 | uint64 | Flags (gleiche `TileFlag`-Bits wie High Seas, 64 Bit) |
| 8 | uint8 | Weight |
| 9 | uint8 | Quality (Layer / Light-ID) |
| 10 | int16 | MiscData |
| 12 | uint8 | Unk2 |
| 13 | uint8 | Quantity |
| 14 | int16 | Animation (Body/Gump-ID für Kleidung) |
| 16 | uint8 | Unk3 |
| 17 | uint8 | Hue |
| 18 | uint8 | StackingOffset |
| 19 | uint8 | Value |
| 20 | uint8 | Height |
| 21 | char[20] | Name, ASCII, mit 0 aufgefüllt (nicht zwingend 0-terminiert bei 20 Zeichen) |

- Datensatz `k` gehört zu Item-ID `0x10000 + k`. Dateigröße = `32 + count * 41`.
- `count` von `tiledataext.mul` und `artextidx.mul` *sollte* gleich sein; beim Laden gilt
  `slots = max(countArt, countTileData)` – fehlende Einträge sind leer (Name "", alle Werte 0).

---

## 6. Referenz-Lesecode (C#, kann 1:1 übernommen werden)

```csharp
// vollständig in Ultima/ExtendedFormat.cs (ReadArtIndex, ReadTileData, ReadTileRecord)
var idx  = ExtendedFormat.ReadArtIndex(Path.Combine(dir, "artextidx.mul"));
var tile = ExtendedFormat.ReadTileData(Path.Combine(dir, "tiledataext.mul"));
using var art = File.OpenRead(Path.Combine(dir, "artext.mul"));

bool TryGetArt(int itemId, out byte[] data)
{
    data = null;
    int k = itemId - ExtendedFormat.BaseId;
    if (k < 0 || k >= idx.Length || !idx[k].IsValid) return false;
    if (idx[k].Offset + idx[k].Length > art.Length) return false;
    data = new byte[idx[k].Length];
    art.Seek(idx[k].Offset, SeekOrigin.Begin);
    art.ReadExactly(data);
    return true;                      // danach dekodieren wie Static-Art (Kapitel 4)
}
```

---

## 7. Was im CLIENT (z. B. ClassicUO) zu tun ist

> Die genauen Klassen-/Dateinamen in ClassicUO ändern sich zwischen Versionen – **vor dem Umsetzen im aktuellen
> Quellcode nachsehen**. Die Liste beschreibt *was* zu ändern ist.

**7.1 Laden der Daten (Pflicht)**
1. Art-Loader: zusätzlich `artextidx.mul` + `artext.mul` öffnen (optional; fehlt etwas → nur Legacy).
   Grafik-Lookup: `id < 0x10000` wie bisher, sonst Kapitel 3/4.
2. TileData-Loader: `tiledataext.mul` lesen und die Static-/Item-Tabelle auf `0x10000 + count` vergrößern.
3. Arrays/Caches/Texture-Atlas, die bisher fest auf 0x10000 (oder `ushort.MaxValue`) Items ausgelegt sind,
   dynamisch auf `0x10000 + count` vergrößern (Speicher: nur Referenzen; Texturen weiterhin lazy laden).
4. Radarfarben der Ext-Items stehen in `radarcolext.mul` (Kapitel 13.5). `animdata.mul` und die
   `light`-Zuordnung decken nur die Legacy-IDs ab → für Ext-IDs Standardwerte verwenden (keine Animation).
5. Optional: Land-Tiles ab 0x4000, Texturen ab 0x4000 und Hues ab Nummer 3001 aus den Zusatzdateien in Kapitel 13 laden.

**7.2 Datentyp der Grafik-ID (Pflicht, größter Aufwand)**
- Überall, wo eine Item-/Static-Grafik als `ushort` gespeichert oder übergeben wird (Item, Static, Multi-Teile,
  Gump-Item-Darstellung, Hover/Tooltip, Cursor-Target, Drag & Drop …), auf `uint`/`int` umstellen.
- Grenzprüfungen der Art `graphic >= 0x10000` / `graphic > MaxItemId` anpassen.

**7.3 Netzwerk (Pflicht für Server-Items) – siehe Kapitel 9 (Vorschlag)**

**7.4 Karten/Statics/Multis (optional, später)**
- `statics*.mul` (7 Byte pro Static, Grafik `ushort`), `multi.mul`/`MultiCollection.uop` speichern 16-Bit-IDs.
  Ext-Items können dort erst stehen, wenn ein eigenes Format eingeführt wird (Kapitel 10).

**7.5 Abnahmetests Client**
- [ ] Ohne Ext-Dateien: Client verhält sich exakt wie vorher (gleiche Bilder, gleiche Performance).
- [ ] Mit Ext-Dateien: Item `0x10000` wird gezeichnet, Name/Height/Flags aus `tiledataext.mul` stimmen.
- [ ] Leerer Slot (`offset = -1`) → kein Absturz, Item unsichtbar bzw. Platzhalter.
- [ ] Zwei Slots mit gleichem Offset → beide zeigen dasselbe Bild.
- [ ] Beschädigter Header (falsches magic) → Ext wird ignoriert, Client startet normal.

---

## 8. Was im SERVER (ServUO / RunUO / ModernUO) zu tun ist

1. `TileData` des Servers: `tiledataext.mul` laden (Kapitel 5), `ItemTable` auf `0x10000 + count` vergrößern.
   Flags (Impassable, Surface, Wearable …), Height, Weight, Layer (Quality) werden für Bewegung/Ausrüstung benutzt.
2. `Item.ItemID` und alle Stellen mit `& 0x3FFF` / `& 0xFFFF`-Masken, `ushort`-Casts oder Vergleichen
   gegen `TileData.MaxItemValue` prüfen und für Ext-IDs öffnen.
3. Pakete, die eine Grafik senden, für Ext-IDs mit dem erweiterten Paket (Kapitel 9) senden –
   **aber nur an Clients, die das Feature gemeldet haben**. Alte Clients bekommen Ext-Items nicht
   (oder als Ersatz-Grafik, z. B. eine konfigurierbare Legacy-ID).

---

## 9. Netzwerk-Erweiterung (VORSCHLAG – noch nicht festgelegt)

Das Original-Protokoll überträgt Item-Grafiken als `uint16`. Empfohlener Weg:

1. **Fähigkeit aushandeln:** Client meldet beim Login eine Fähigkeit „ExtendedItemIds" (z. B. über ein eigenes
   Paket direkt nach dem Login oder ein freies Bit in den Client-Flags). Server merkt sich das pro `NetState`.
2. **Eigene Paketvarianten mit 32-Bit-Grafik** für alle Pakete, die Item-Grafiken enthalten, mindestens:
   Welt-Item anzeigen, Container-Inhalt / Item in Container, Ausrüstung (Equip), Mobile-Ausrüstungsliste,
   Drag-Effekte/grafische Effekte (falls Item-Grafiken), Multi-Platzierungsvorschau.
   Aufbau jeweils wie das Original, nur das Grafikfeld als `uint32`.
3. Feste, dokumentierte Paket-IDs wählen und **hier in Kapitel 9 nachtragen**, sobald entschieden
   (damit Client und Server dieselbe Definition nutzen).

---

## 10. Spätere Erweiterungen (VORSCHLAG)

- **Statics mit Ext-IDs:** z. B. zusätzliche `staticsXext.mul`/`staidxXext.mul` mit `uint32`-Grafik pro Static,
  die vom Client zusätzlich zu den Original-Statics gezeichnet werden.
- **Multis mit Ext-IDs:** Multi-Komponenten mit `uint32`-Grafik in einer Zusatzdatei.
- **animdataext.mul / lightext** für animierte oder leuchtende Ext-Items (Radarfarben: inzwischen umgesetzt, Kapitel 13.5).
- Versionsnummer im Header (`version`) erhöhen, wenn sich ein Satzaufbau ändert; alte Versionen weiter lesen.

---

## 11. Kurzfassung für einen Prompt

> „Erweitere <Client/Server> um das UOFiddler-Extended-Format: Item-IDs ab 0x10000 stehen in `artextidx.mul`
> (32-Byte-Header ‚UOAX', v1, baseId 0x10000, count, recordSize 16; dann count × {int64 offset (-1 leer),
> int32 length, int32 extra}), `artext.mul` (Static-Art-Kodierung wie art.mul) und `tiledataext.mul`
> (Header ‚UOTX', recordSize 41; High-Seas-Item-Satz ohne Gruppenheader). Legacy-Dateien unverändert, IDs
> < 0x10000 wie bisher. Grafik-IDs von ushort auf uint umstellen, Arrays dynamisch, fehlende radarcol/animdata
> für Ext-IDs mit Standardwerten. Spezifikation: Docs/ExtendedFormat_Client_Spezifikation.md im Repo
> jedi661/UoFiddlerPixel, Referenzcode Ultima/ExtendedFormat.cs. Optional zusätzlich (Kapitel 13): Land-Tiles ab
> 0x4000 (`artlandextidx.mul` ‚UOLA' + `artlandext.mul`, Land-Kodierung wie art.mul; `tiledatalandext.mul` ‚UOLT',
> 32 Byte), Texturen ab 0x4000 (`texextidx.mul` ‚UOXT' + `texmapsext.mul`), Hues ab Nummer 3001 (`huesext.mul`
> ‚UOHX', 88 Byte), Radarfarben (`radarcolext.mul` ‚UORI' ab 0x10000, `radarcollandext.mul` ‚UORL' ab 0x4000, je 2 Byte)."

---

## 12. Anhang: Lage einer Grafik im UO-Feld (verbindlich, so rechnet UOFiddler)

Implementiert in `UoFiddler.Controls/Classes/ArtTransform.cs` (`UoTile`) und von allen Werkzeugen
(Directions, Transform, Raster) benutzt:

- Ein Feld (Land-Tile) ist eine Raute **44 x 44**: oben (22,0), rechts (44,22), unten (22,44), links (0,22),
  Mitte (22,22). Kanten unter **45 Grad**. Welt -> Bildschirm: `sx = (x - y) * 22`, `sy = (x + y) * 22 - z * 4`.
- Eine Static-/Art-Grafik wird mit ihrem **ganzen Rahmen** (Breite w, Höhe h - nicht nur den farbigen Pixeln)
  gesetzt: `drawX = tx + 22 - floor(w / 2)`, `drawY = ty + 44 - h` (+ eigener Offset `offX`, `offY`).
- **Fixpunkt** = mittlerer Pixel der untersten Bildreihe `(floor(w/2), h-1)` = untere Ecke des Feldes.
- Nachbarfeld (i, j): `tx = baseX + (i - j) * 22`, `ty = baseY + (i + j) * 22`.
- Prüfung: eine 44x44-Bodenplatte füllt die Raute exakt (`drawX = tx`, `drawY = ty`).
- Eine Grafik reicht nie unter die untere Ecke ihres Feldes; Objekte, die zum Betrachter zeigen, stehen auf
  ihrem vordersten Feld.

---

## 13. Weitere Erweiterungen: Land-Tiles, Texturen, Hues, Radarfarben (verbindlich, implementiert)

Gleiches Prinzip wie bei den Items: **die Original-Dateien bleiben exakt im Client-Layout**, alles darüber steht in
Zusatzdateien im selben Ordner. **Jede Erweiterung fasst – wie die Items – bis zu 16.777.216 Einträge** (`count` ≤ 0x1000000)
und **beginnt genau dort, wo die Original-Datei aufhört**. Alte Clients lesen weiter nur die Originale; der neue Client
liest zusätzlich die Zusatzdateien und muss dafür die betroffenen Felder auf 32 Bit erweitern (13.6). Alle Zusatzdateien haben den **32-Byte-Header aus Kapitel 2** (magic, version 1,
baseId, count, recordSize, 3 × reserved). Prüfregeln wie in Kapitel 2, nur mit der jeweiligen magic / baseId /
recordSize. Fehlt eine Datei oder ist sie ungültig → so tun, als gäbe es keine Erweiterung (nie abbrechen).

| Datei | magic | baseId | recordSize | Inhalt |
|---|---|---|---|---|
| `artlandextidx.mul` | `UOLA` | `0x4000` | 16 | Index der Land-Tiles ab 0x4000 (Eintrag wie Kapitel 3) |
| `artlandext.mul` | – (kein Header) | – | – | Land-Tile-Daten, Kodierung wie Land in `art.mul` (13.1) |
| `tiledatalandext.mul` | `UOLT` | `0x4000` | 32 | Land-TileData ab 0x4000 (13.2) |
| `texextidx.mul` | `UOXT` | `0x4000` | 16 | Index der Texturen ab 0x4000; `extra` 0 = 64×64, 1 = 128×128 |
| `texmapsext.mul` | – (kein Header) | – | – | Texturdaten, Kodierung wie `texmaps.mul` (13.3) |
| `huesext.mul` | `UOHX` | `3000` | 88 | Hues ab Index 3000 = Hue-Nummer 3001 (13.4) |
| `radarcollandext.mul` | `UORL` | `0x4000` | 2 | Radarfarbe der Land-Tiles ab 0x4000 (13.5) |
| `radarcolext.mul` | `UORI` | `0x10000` | 2 | Radarfarbe der Items ab 0x10000 (13.5) |

**ID-Abbildung** (allgemein): `k = id - baseId`, gültig wenn `0 <= k < count`; darunter gilt die Original-Datei.

### 13.1 Land-Tiles (`artlandextidx.mul` / `artlandext.mul`)
- `art.mul` behält genau 0x4000 Land-Einträge (0x0000–0x3FFF). Land-IDs ab **0x4000** stehen nur hier.
- **Größe:** bis 16.777.216 Land-Tiles (IDs `0x4000` – `0x1003FFF`). IDs bis 0xFFFF passen noch in die klassische
  `map*.mul` (16-Bit-Land-ID); höhere IDs braucht ein neues Kartenformat im neuen Client.
- Kodierung je Tile: 1.012 Pixel × `uint16` = **2.024 Byte**, Farbe `^ 0x8000` (wie in art.mul):
  22 Zeilen mit Start-X 21,20,…,0 und Länge 2,4,…,44, danach 22 Zeilen mit Start-X 0,1,…,21 und Länge 44,42,…,2.
- Client: im Land-Lookup `id >= 0x4000` → Zusatzdatei statt `id & 0x3FFF` (das alte Maskieren würde auf ein
  falsches Tile zeigen).

### 13.2 Land-TileData (`tiledatalandext.mul`, 32 Byte pro Satz)
| Offset | Typ | Inhalt |
|---|---|---|
| 0 | uint64 | flags (wie High-Seas-Land-Flags) |
| 8 | int32 | textureId (zeigt auf `texmaps.mul` bzw. `texmapsext.mul`; 0 = keine Textur) |
| 12 | char[20] | name (ASCII, mit 0 aufgefüllt) |
Keine Gruppen-Header. `tiledata.mul` behält seine 512 Land-Gruppen unverändert.

### 13.3 Texturen (`texextidx.mul` / `texmapsext.mul`)
- `texidx.mul` / `texmaps.mul` behalten die IDs 0x0000–0x3FFF. Texturen ab **0x4000** stehen hier.
- **Größe:** bis 16.777.216 Texturen (IDs `0x4000` – `0x1003FFF`). `tiledata.mul` speichert die Textur-ID mit 16 Bit
  (bis 0xFFFF), `tiledatalandext.mul` mit **32 Bit** – erweiterte Land-Tiles können also jede erweiterte Textur benutzen.
- Kodierung: Größe² × `uint16`, zeilenweise, Farbe `^ 0x8000`; 64×64 = 8.192 Byte (`extra` 0), 128×128 = 32.768 Byte (`extra` 1).

### 13.4 Hues (`huesext.mul`, 88 Byte pro Satz)
- `hues.mul` behält seine 375 Blöcke = Hue-Nummern **1–3000** (Index 0–2999).
- `huesext.mul` Satz k = Index `3000 + k` = **Hue-Nummer `3001 + k`**. Satz: `uint16[32]` Farben, `uint16` tableStart,
  `uint16` tableEnd, `char[20]` Name – genau wie ein Eintrag in `hues.mul`, aber **ohne** die 4-Byte-Block-Header.
- **Größe:** bis 16.777.216 Zusatz-Hues (Hue-Nummern 3001 – 16.780.216). Der klassische Client nimmt `hue & 0x3FFF`
  (bis 16.383, Bit 0x8000 = „Partial Hue"); höhere Nummern braucht der neue Client mit 32-Bit-Hue-Feld.
- Client: Hue-Tabelle/-Textur auf `3000 + count` Einträge vergrößern (z. B. ClassicUO: Hue-Textur-Atlas).

### 13.5 Radarfarben (`radarcollandext.mul` / `radarcolext.mul`, je 2 Byte)
- `radarcol.mul` bleibt: Land 0x0000–0x3FFF, danach Items (Index = 0x4000 + Item-ID) bis Item 0xFFFF.
- Land-IDs ab 0x4000 → `radarcollandext.mul`, Item-IDs ab 0x10000 → `radarcolext.mul`. Farbe = 15-Bit-RGB (wie radarcol.mul).
- UOFiddler hält beide Dateien automatisch so groß wie die Land- bzw. Item-Erweiterung.

### 13.6 Was der neue Client zusätzlich braucht (über die klassischen Grenzen hinaus)
- Land-ID, Textur-ID und Hue-Nummer intern als `int`/`uint` führen (nicht `ushort`).
- Karten mit Land-IDs > 0xFFFF: eigenes Kartenformat (z. B. 32-Bit-Land-ID je Zelle) – noch festzulegen.
- Hues > 16.383: Hue-Feld in Paketen/Statics auf 32 Bit; Flag „Partial Hue" separat übertragen.
- Arrays/Atlanten dynamisch nach `count` der Zusatzdateien anlegen, Inhalte lazy laden.

### 13.7 Abnahmetests
- [ ] Ohne die Dateien aus Kapitel 13 verhält sich alles wie vorher.
- [ ] Land 0x4000 wird aus `artlandext.mul` gezeichnet; Name/Flags/Textur aus `tiledatalandext.mul`; Textur-ID ≥ 0x4000 wird aus `texmapsext.mul` geladen.
- [ ] Hue-Nummer 3001 färbt mit Satz 0 aus `huesext.mul`.
- [ ] Radarkarte zeigt Land ≥ 0x4000 und Items ≥ 0x10000 mit den Farben aus den Ext-Dateien.
- [ ] Land-Tile 0x10000 (über der alten 16-Bit-Grenze) und Textur 0x10000 werden geladen (count > 0xC000).
