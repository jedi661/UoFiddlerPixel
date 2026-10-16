# Gump-Bibliothek

Eigene, frei benannte Gump-Sammlung für den Gumps-Tab: beliebig viele Kategorien pro Gump,
eigene Reihenfolge, Namen, Beschreibungen, Tags, Favoriten, eigene Vorschaubilder und Volltextsuche.
Geöffnet wird sie mit dem Button in der oberen Leiste des Gumps-Tabs; "Add to Gump Library" im
Rechtsklick-Menü des Gumps-Tabs übernimmt den ausgewählten Gump.

## Dateien

| Datei | Inhalt |
|---|---|
| `Name.gumplib.idx` / `.mul` | Gumps (Header + 16-Byte-Einträge wie die Art-Bibliothek, Kind = Gump) |
| `Name.gumpprev.idx` / `.mul` | eigene Vorschaubilder (optional pro Gump) |
| `Name.gumplib.json` | Kategorien, Tags, Name, Beschreibung, Original-ID, Position, Favorit |

Datenformat eines Gumps (`GumpLibraryCodec`): `"GMP1"`, Breite, Höhe, Zeilentabelle, dann Läufe
`(Farbe ARGB1555, Länge)` – verlustfrei, ohne Größenbeschränkung der Zeilentabelle (bis 4096 x 4096).

## Klassen

| Ordner | Klassen |
|---|---|
| `Core` | `GumpLibraryCodec`, `GumpLibraryStorage` (+ `GumpStorageKind`), `GumpLibraryCache`, `GumpLibraryFile` |
| `Metadata` | `GumpLibraryMetadata`, `GumpLibraryCategory`, `GumpLibraryItemInfo`, `GumpLibrarySearch` (+ `GumpSortMode`) |
| `Helpers` | `GumpLibraryTransfer` (gumpart.mul, IDGumpNames.xml), `GumpLibraryClipboard`, `GumpImageTools`, `GumpLibrarySettings` |
| `Forms` | `GumpLibraryForm`, `GumpLibraryCategoryForm`, `GumpLibraryNewForm`, `GumpLibraryInputForm`, `GumpLibraryHelpForm` |

Wiederverwendet aus der Art-Bibliothek: `ArtLibraryHeader/Entry`, `ArtLibraryCodec.ToPixels/ToBitmap`,
`ArtIdParser`, `ArtLibraryImageConverter`, `ArtLibraryTargetIdForm`, `ArtLibraryGrowForm`.

## Änderungen an bestehenden Dateien

* `GumpControl.Designer.cs`: Button `toolStripButtonGumpLibrary`, Menüeintrag `addToGumpLibraryToolStripMenuItem`.
* `GumpControl.cs`: Klick-Handler, `GetSelectedGumpId()`, und `OnGumpChangeEvent` liest die IDs jetzt aus den
  Einträgen `"id - name"` (vorher `int.Parse` des ganzen Textes → Absturz, sobald ein anderes Fenster einen Gump änderte).

## Suche

`hemd rot` (alle Wörter) · `"rotes hemd"` · `-leder` · `name:` `desc:` `tag:` `cat:` `id:0x10-0x20` `src:50000` ·
`is:fav` `is:preview` `is:nocat` `is:notag` `is:noname`
