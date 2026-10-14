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
| `Metadata/` | (Schritt 4) Namen, Beschreibung, Kategorien, Tags |
| `Helpers/` | `ArtIdParser` (dezimal/hex), `ArtLibrarySettings` (zuletzt geöffnete Bibliotheken); (Schritt 3) Zwischenablage, Transfer |
| `Forms/` | `ArtLibraryForm` (Hauptfenster), `ArtLibraryNewForm` (Dialog „Neue Bibliothek“), jeweils `.cs` + `.Designer.cs` + `.resx` |

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

## Fenster (Schritt 2)

* Geöffnet über den Button **Art-Bibliothek** in der Toolbar des Items-Tabs (`ItemsControl`), eigenständiges Fenster.
* Toolbar: zuletzt geöffnete Bibliotheken (ComboBox), Neu, Öffnen, Neu laden, ID-Sprung (dezimal/hex, Enter),
  Umschalter **Nur belegte** (aus = alle IDs von 0 bis Ende durchscrollen).
* Kachelansicht wie im Items-Tab, rechts Vorschau + Details, unten Statusleiste.
* Rechtsklick: Nächste freie ID, ID kopieren (Hex/Dezimal), Entfernen (auch Taste Entf), Hintergrundfarbe.
* Tasten: `Entf` entfernt die Auswahl, `Strg+G` springt ins ID-Feld.
* Einstellungen (zuletzt geöffnete Bibliotheken, Anzeigemodus) liegen in `%AppData%\UoFiddler\ArtLibrarySettings.json`.
