# Art-MUL-Bibliothek (ArtLibrary)

Eigenständiges Klassensystem für eine externe Item-Art-Bibliothek. Die Bibliothek dient als Auslagerungsspeicher
für beliebig viele Item-Grafiken: importieren, exportieren, suchen, anzeigen, sortieren und zurück in die
originale `art.mul` übertragen.

Die bestehenden Klassen (`Ultima.Art`, `FileIndex`, `ItemsControl` …) werden **nicht** verändert.
Einzige Berührungspunkte sind später der Button im `ItemsControl` und das Lesen/Schreiben über die
öffentlichen Methoden von `Ultima.Art`.

## Ordner

| Ordner | Inhalt |
|---|---|
| `Core/` | Dateiformat: Header, Indexeintrag, Codec, `ArtLibraryFile` |
| `Metadata/` | `ArtLibraryMetadata` (JSON), `ArtLibraryCategory`, `ArtLibraryItemInfo` – Namen, Beschreibung, Kategorien, Tags |
| `Helpers/` | `ArtIdParser` (dezimal/hex, Bereiche), `ArtLibrarySettings`, `ArtLibraryClipboard`, `ArtLibraryTransfer` (Original ⇄ Bibliothek), `ArtLibraryImageConverter` (Transparenz) |
| `Forms/` | `ArtLibraryForm` (Hauptfenster), `ArtLibraryNewForm`, `ArtLibraryTargetIdForm`, `ArtLibraryCategoryForm`, `ArtLibraryGrowForm` – jeweils `.cs` + `.Designer.cs` + `.resx` |

## Dateien einer Bibliothek

Eine Bibliothek `MeineDeko` besteht aus drei Dateien im selben Ordner:

| Datei | Inhalt |
|---|---|
| `MeineDeko.artlib.idx` | Header (64 Bytes) + ein Eintrag (16 Bytes) pro ID |
| `MeineDeko.artlib.mul` | Bilddaten, hintereinander |
| `MeineDeko.artlib.json` | Metadaten (Schritt 4) |

Alle Zahlen sind Little Endian.

### Header (`.artlib.idx`, Byte 0–63)

| Offset | Typ | Feld | Bedeutung |
|---|---|---|---|
| 0 | char[8] | Magic | `UOFARTLB` |
| 8 | int32 | Version | 1 |
| 12 | int32 | EntrySize | 16 |
| 16 | int32 | Capacity | Anzahl IDs (0 … Capacity-1) |
| 20 | int32 | Kind | 0 = Item-Art (1 = Land, 2 = Gump, 3 = Textur – reserviert für spätere Bibliotheken) |
| 24 | int64 | Created | Erstellzeit, Unix-Millisekunden UTC |
| 32 | char[32] | Name | Anzeigename, UTF-8, mit 0 aufgefüllt |

### Indexeintrag (ab Byte 64, Eintrag *i* = ID *i*)

| Offset | Typ | Feld | Bedeutung |
|---|---|---|---|
| 0 | int64 | Offset | Position in `.artlib.mul`, `-1` = leer |
| 8 | int32 | Length | Länge der Daten in Bytes |
| 12 | int32 | Flags | reserviert (1 = Favorit) |

Position eines Eintrags in der Datei: `64 + id * 16`.
Durch den 64-Bit-Offset gibt es keine 2-GB-Grenze wie bei der originalen `art.mul`.

### Bilddaten (`.artlib.mul`)

Byte-identisch zum Item-Format der originalen `art.mul`, damit Grafiken 1:1 (ohne Neukodierung) zwischen
Bibliothek und `art.mul` kopiert werden können:

```
int32    Flag      unbenutzt, wird als 1234 geschrieben
ushort   Width
ushort   Height
ushort[] Lookup    pro Zeile: Start der Zeile in ushorts, gezählt ab Ende dieser Tabelle
Zeilen:  (ushort xOffset, ushort run, ushort[run] Pixel) … abgeschlossen mit (0, 0)
```

Pixel sind ARGB1555. Gespeichert werden nur deckende Pixel (Alpha-Bit `0x8000`), und zwar ohne Alpha-Bit.

## Zum Vergleich: die originale art.mul

* `artidx.mul` hat **keinen Header**, nur Einträge zu 12 Bytes (`int32 Lookup, int32 Length, int32 Extra`).
  Die Anzahl der IDs ergibt sich aus Dateigröße / 12.
* Index `0x0000–0x3FFF` sind Land-Tiles (44×44 Raute, 2044 Bytes roh), ab `0x4000` folgen die Items.
* Die Client-Version wird an der Anzahl erkannt: 0x4000 Items (ML), 0x8000 (SA), 0xFFDC (High Seas).
* Map- und Statics-Dateien speichern Item-IDs als 16 Bit – mehr als 65.535 Items kann der Client nicht nutzen.

## Verhalten von `ArtLibraryFile`

* **Anlegen:** `ArtLibraryFile.Create(ordner, name, capacity)` – Standard 1.000.000 IDs, maximal 100.000.000.
* **Öffnen:** `ArtLibraryFile.Open(pfad)` – Pfad zur `.idx`, `.mul`, `.json` oder ohne Endung.
* **Speichern sofort:** Jede Änderung hängt die Daten an die `.mul` an und überschreibt nur den 16-Byte-Eintrag.
  Es wird nie die ganze Datei neu geschrieben.
* **Erweitern:** `Grow(neueAnzahl)` hängt leere Einträge an und passt den Header an. Bilddaten bleiben unberührt.
* **Komprimieren:** `Compact()` schreibt die `.mul` ohne überschriebene/gelöschte Daten neu. IDs bleiben gleich.
* **Kopieren innerhalb der Bibliothek** (`Copy`) teilt sich die Daten – es wird nichts doppelt gespeichert.
* Mehrere Bibliotheken können gleichzeitig geöffnet sein (jede Instanz ist unabhängig).

## Fenster

Geöffnet über den Button **Art-Bibliothek** in der Toolbar des Items-Tabs (`ItemsControl`), eigenständiges Fenster.

**Obere Toolbar:** zuletzt geöffnete Bibliotheken, Neu, Öffnen, Neu laden, Menü **Bibliothek**
(Eigenschaften, Kategorien und Tags verwalten, Erweitern, Komprimieren, Ordner öffnen), ID-Sprung dezimal/hex,
**Nur belegte** (aus = alle IDs von 0 bis Ende), Kopieren, Einfügen, Menü **Original art.mul**.

**Filterleiste:** Suche (Name, Beschreibung, Tag, Kategorie, ID dezimal/hex), Kategorie (inkl. Unterkategorien),
Tag, **Nur Favoriten**, Filter zurücksetzen.

**Rechts:** Vorschau (Bilder per Drag & Drop aus dem Explorer hineinziehen), darunter Name, Kategorie, Tags,
Beschreibung, Favorit, Infos. „Übernehmen“ (oder Enter im Namen) speichert; Änderungen werden außerdem beim
Auswahlwechsel und alle 3 Sekunden automatisch gesichert.

**Rechtsklick:** Einfügen / Einfügen auf die kopierten IDs / Kopieren, Bilder oder Ordner importieren,
in Ordner exportieren (BMP/PNG/TIFF, Auswahl oder alle), Original art.mul (ins Original kopieren/verschieben,
Auswahl aus dem Items-Tab holen, IDs/Bereiche holen), kopieren/verschieben nach ID, zwei Grafiken tauschen,
Kategorie zuweisen, Tag hinzufügen/entfernen, Favorit, nächste freie ID, ID kopieren, Entfernen, Hintergrundfarbe.

**Tasten:** `Strg+C` kopieren, `Strg+V` einfügen, `Entf` entfernen, `Strg+G` ID-Feld, `Strg+F` Suche.

### Austausch mit dem Original

* **Zwischenablage:** Kopieren legt alle markierten Grafiken verlustfrei (eigenes Format, mehrere auf einmal)
  **und** das erste Bild als normales Bild (schwarzer Hintergrund) ab. Der Items-Tab fügt das normale Bild mit
  seinem bisherigen „Import from clipboard“ ein; umgekehrt fügt die Bibliothek jedes kopierte Bild ein.
  Schwarz und Weiß (+ #D3D3D3) werden dabei – wie im Items-Tab – transparent.
* **Ins Original kopieren/verschieben:** schreibt über `Art.ReplaceStatic` in die geladene art.mul (im Speicher)
  und meldet es per `ControlEvents.FireItemChangeEvent` – der Items-Tab aktualisiert sich selbst.
  **Gespeichert wird wie gewohnt im Items-Tab.**
* **Aus dem Original holen:** Auswahl des Items-Tabs oder ID-Liste/Bereiche (z. B. `0x1000-0x10FF, 1234`).
  Name aus tiledata.mul und Original-ID werden in den Metadaten gemerkt.
* **Ordner:** Export als `Item 0x04D2.bmp` (wie der Items-Tab). Import liest die ID aus dem Dateinamen
  (`Item 0x04D2`, `Art_Hex_Adress_4D2`, `0x4D2`, `4D2h`, `1234`) oder legt fortlaufend ab einer Ziel-ID ab.

### Metadaten (`Name.artlib.json`)

```json
{
  "Version": 1,
  "Categories": [ { "Id": 1, "Name": "Möbel" }, { "Id": 2, "Name": "Tische", "ParentId": 1 } ],
  "Tags": [ "holz", "tisch" ],
  "Items": { "1234": { "Name": "Eichentisch", "Description": "", "CategoryId": 2, "Tags": [ "holz" ], "SourceId": 2874, "Added": "..." } }
}
```

* Einstellungen (zuletzt geöffnete Bibliotheken, Anzeigemodus): `%AppData%\UoFiddler\ArtLibrarySettings.json`.
