# Terrain-Bibliothek (LandTiles + Texturen)

Eigenständiges Klassensystem für eine Bibliothek aus **LandTiles** (art.mul 0x0000–0x3FFF, 44×44 Raute) und
**Texturen** (texmaps.mul, 64×64 / 128×128) mit **gemeinsamen Slots**: Slot `0x0003` kann ein LandTile, eine
Textur oder beides enthalten – dazu Einstellungen (tiledata-Name, Flags, Textur-ID, RadarColor), Name, Kategorie,
Tags, Beschreibung und Favorit.

Die Originaldateien (art.mul, texmaps.mul, tiledata.mul, radarcol.mul) werden **nur auf Knopfdruck** geändert –
und dann wie gewohnt im jeweiligen Tab gespeichert.

## Zusammenhänge im Bestand

| Teil | Datei | Format | Verbindung |
|---|---|---|---|
| LandTiles | art.mul (`Art.GetLand/ReplaceLand`) | 44×44 Raute, 1012 Pixel = 2024 Bytes, ARGB1555 ohne Alpha | `LandTilesControl` |
| Texturen | texmaps.mul (`Textures.GetTexture/Replace`) | 64×64 (8192 B) / 128×128 (32768 B) | `TexturesControl` |
| tiledata | `TileData.LandTable[id]` | Name, Flags (`TileFlag`), **TextureId** | LandTile → Textur |
| RadarColor | radarcol.mul (`RadarCol.Get/SetLandColor`) | 1 Farbe (ushort) pro LandTile | `RadarColorControl` |
| Hues | hues.mul | benannte Farbtabellen | Vorbild für benannte Vorlagen/Farben |

## Ordner

| Ordner | Inhalt |
|---|---|
| `Core/` | `TerrainLayer` (LandTile/Textur), `TerrainCodec` (byte-identisch zu art.mul/texmaps.mul), `TerrainCache` (LRU), `TerrainLayerStorage` (eine Ebene, gleiches Dateiformat wie die Art-Bibliothek), `TerrainLibraryFile` (beide Ebenen + Metadaten) |
| `Metadata/` | `TerrainMetadata` (JSON), `TerrainSlotData` (tiledata-Werte, RadarColor, Herkunft, Favorit), `TerrainPreset` (Vorlagen), `TerrainDefaults` (Standardvorlagen) – Namen/Kategorien/Tags über `ArtLibraryMetadata` |
| `Helpers/` | `TerrainSession` (gemeinsam geöffnete Bibliothek), `TerrainTransfer` (Original-Dateien), `TerrainClipboard`, `TerrainColor`, `TerrainImageTools` (skalieren, drehen, spiegeln, LandTile aus Textur), `TerrainFlags`, `TerrainLibrarySettings` |
| `Forms/` | `LandTileLibraryForm`, `TextureLibraryForm` (beide mit `TerrainLibraryPanel`), `TerrainWindows` (Öffnen + Andocken), `TerrainNewForm`, `TerrainPresetForm`, `TerrainHelpForm` – jeweils `.cs` + `.Designer.cs` + `.resx` |

Wiederverwendet aus der Art-Bibliothek (nur gelesen, nicht verändert): Header/Eintrag des Dateiformats, ID-Parser,
Ziel-ID-Dialog, Kategorien-Dialog, Erweitern-Dialog, Bild laden/exportieren. Einzige Ergänzung dort:
`ArtLibraryMetadata.AcceptChanges()`.

## Dateien einer Terrain-Bibliothek „MeinGelände“

```
MeinGelände.landlib.idx / .landlib.mul   LandTiles  (Header Kind = LandArt)
MeinGelände.texlib.idx  / .texlib.mul    Texturen   (Header Kind = Texture)
MeinGelände.terrain.json                 Einstellungen, Namen, Kategorien, Tags, Vorlagen
```

Index und Daten wie die Art-Bibliothek: 64-Byte-Header + 16 Byte pro Slot (64-Bit-Offset), Daten werden angehängt,
„Komprimieren“ räumt auf, „Erweitern“ vergrößert beide Ebenen gleich.

## Fenster

* **Land-Tiles-Tab** → Button (Gras-Symbol) öffnet die LandTile-Bibliothek, **Textures-Tab** → Button
  (Wasser-Symbol) die Textur-Bibliothek. „Texturen ▶“ / „LandTiles ▶“ öffnet das jeweils andere Fenster.
* **Andocken:** Texturen rechts neben LandTiles, beide bewegen sich gemeinsam; aus = frei.
* **Auswahl synchronisieren** (Optionen): Klick auf einen Slot wählt ihn in beiden Fenstern.
* Rechts: Vorschau, Partnerbild, RadarColor; Reiter **Info** (Name, Kategorie, Tags, Beschreibung, Favorit),
  **tiledata** (Name, Textur-ID, Flags, Vorlagen, Aus tiledata holen, → tiledata.mul),
  **Farbe** (Mittelwert, Farbwähler, 0x1234/#RRGGBB, aus radarcol, → radarcol.mul).
* Speichern / Auto-Speichern mit Statusanzeige, Hilfe (F1).

## Optionen (Menü „Optionen“, werden gemerkt)

| Option | Standard |
|---|---|
| Import: andere Bildgrößen automatisch skalieren | an |
| Aus Original holen: tiledata-Werte / RadarColor / zugehörige Textur mit übernehmen | an |
| Ins Original: RadarColor in radarcol.mul mitschreiben | **aus** |
| Standardvorlagen anzeigen | an |
| Auswahl synchronisieren | an |
| LandTile aus Textur: isometrisch (45°) / zeilenweise (wie Texture Window) | isometrisch |

Einstellungen: `%AppData%\UoFiddler\TerrainLibrarySettings.json`.
