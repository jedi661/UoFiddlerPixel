# MapViewer – Kartenverwaltung und Region-Viewer

Alles in diesem Ordner gehört zur Kartenverwaltung und zum Region-Viewer, die aus `MapControl` aufgerufen werden.

## Dateien

| Datei | Aufgabe |
|---|---|
| `MapRegistryStore.cs` | Lädt/speichert `MapDefinitions.xml` (AppData) und hält `Options.MapNames` synchron |
| `MapManagerForm.cs` | Formular "Manage Maps": Karten hinzufügen, bearbeiten, ausblenden, entfernen, Ordner durchsuchen |
| `RegionData.cs` | Liest Land + Statics eines Bereichs, Iso-Projektion, Zeichenreihenfolge, Aufteilung in Kacheln |
| `ArtPixelCache.cs` | Art/Texturen/Hues als Pixel-Arrays (für den Software-Renderer) |
| `IsoRenderer.cs` | Rendert 256x256-Kacheln (Land-Art, gestreckte Texturen mit Licht, Statics, Hue, Transparenz), LRU-Cache im Speicher, optional Auslagerung in den Temp-Ordner, Zoomstufen (LOD) |
| `IsoViewControl.cs` | Anzeige: Verschieben, Zoom, Auswahl, Raster/Fehler-Overlay, schrittweises Rendern |
| `RegionAnalyzer.cs` | Fehlerprüfung (ungültige IDs, doppelte Statics, Statics unter Land, steiles Land, schwebende Statics) |
| `RegionIssue.cs` | Protokolleintrag + CSV-Export + XML speichern/laden |
| `RegionEditSession.cs` | Änderungen im Speicher (Undo), Reparaturen, Speichern in den Output-Ordner |
| `MapRegionViewerForm.cs` | Das Fenster "Region Viewer" |
| `InputDialog.cs` | Kleiner Eingabedialog für Notizen |

Im Ultima SDK dazu: `MapDefinition`, `MapRegistry`, sowie in `TileMatrix` die Funktionen zum Ersetzen von
Static-Blöcken und Ändern von Land-Blöcken im Speicher (`SetStaticBlockOverride`, `SetModifiedLandBlock`)
und in `Map` `SaveLand` / `DefragStatics` (schreibt ersetzte Blöcke mit).

## Ablauf

1. Im Kartenreiter "Mark" anklicken, Bereich aufziehen (bleibt rot markiert).
2. "View Region" öffnet den Bereich (max. 512 x 512) im Region-Viewer.
   Alternativ Rechtsklick auf die Karte -> "View Area Here" (64 x 64 um die Maus).
3. Der Bereich wird beim Öffnen geprüft, Fehler stehen im Reiter "Errors / Protocol" und sind in der Ansicht markiert.
4. Reparieren: "Fix all (safe)" oder Einträge abhaken -> "Fix checked". Einzelne Statics löschen/verschieben im Reiter "Tile".
5. "Save" fragt nach dem Ziel (Output-Ordner, Ordner der geladenen Karte oder anderer Ordner) und ob ein Backup gemacht wird.
   Geschrieben werden `map{n}.mul` (nur wenn Land geändert wurde), `staidx{n}.mul`/`statics{n}.mul` und auf Wunsch das Protokoll `RegionProtocol_*.xml`.
   "Backups..." stellt alte Dateien wieder her.

## Bauen (Reiter "Build") und Multis

| Datei | Aufgabe |
|---|---|
| `Build/MultiTemplate.cs` | Gebäudemodell für alle Formate (ID, X, Y, Z, Hue, Flag, Stockwerk) |
| `Build/MultiFormats.cs` | Import: TXT (dezimal/hex automatisch), UOA binär (mit Hue), UOA-Text/WSC/CSV über das SDK. Export: TXT hex/dezimal, UOA binär, UOA-Text, WSC, CSV, UOX3, XML |
| `Build/MultiLibrary.cs` | Eigene Bibliothek (`.uml` + Index `.umx`), nur angehängt, für sehr viele Einträge |
| `Build/MultiBrowserForm.cs` | Fenster "Multis / Building library": Client-Multis aus multi.mul und Bibliothek, Suche, Vorschau, Import, Export, in multi.mul speichern |
| `Build/BuildPlacement.cs` | Platzierung: Anker, Basis-Z, Grundfläche, Hindernisse, Prüfung, Boden ebnen, Bereich als Gebäude |
| `Build/MultiPreview.cs` | Vorschaubild / halbtransparentes Gebäude beim Platzieren |
| `MapRegionViewerForm.Build.cs` | Logik des Reiters "Build" |
| `MapSaveService.cs`, `MapSaveDialog.cs`, `MapBackupForm.cs` | Speichern (Ziel wählen, Backup), Backups wiederherstellen |
| `Build/AreaExportForm.cs` | "Export Area": Statics eines Bereichs prüfen/abwählen und als Datei, in die Bibliothek, in multi.mul oder als ServUO-Script übernehmen |
| `Build/ServUOExport.cs` | ServUO-Addon-Script (mit Hue), Decoration-.cfg (absolute Positionen), Text für das Multi-Script-Fenster |

Die Bibliothek liegt standardmäßig in `<Output-Ordner>\UoFiddler_BuildingLibrary\BuildingLibrary.uml` (mit README.txt).
Eine Bibliothek älterer Versionen in AppData wird weiter benutzt. "Move library..." verschiebt sie, "Library file..." öffnet/erstellt eine andere.

Bedienung beim Platzieren: Maus bewegt das Gebäude, Klick fixiert die Position (nochmal Klick = wieder folgen),
Pfeiltasten X/Y, Bild hoch/runter Z (Shift = 5), Enter setzt das Gebäude, Esc bricht ab. Strg+Z macht den letzten Schritt rückgängig.

Speichern: Output-Ordner, Ordner der geladenen Karte (ersetzt die Originale) oder ein anderer Ordner.
Vorher wird in eine temporäre Datei geschrieben; ersetzte Dateien landen als Backup in `<Ziel>\UoFiddlerBackups\`.

## Erweiterung 3D (Ansatz für später)

`RegionData` ist vom Renderer unabhängig:
- `RegionCell` enthält pro Position Land-ID, die z-Werte aller vier Ecken (`LandZ`, `ZRight`, `ZBottom`, `ZLeft`) und die Statics.
  Daraus lässt sich direkt ein Höhen-Mesh bauen (2 Dreiecke pro Tile, Textur über `TextureId`/`ArtPixelCache.GetTexture`).
- Statics können als Billboards (Art-Bild) an Position (x, y, z) gesetzt werden.
- `IRegionRenderer` (in `IsoRenderer.cs`) beschreibt die Schnittstelle. Ein 3D-Renderer (z. B. OpenTK `GLControl`)
  implementiert sie und wird statt `IsoViewControl` in `MapRegionViewerForm` eingesetzt.
  Prüfung, Protokoll und Bearbeitung (`RegionAnalyzer`, `RegionEditSession`) bleiben unverändert.
