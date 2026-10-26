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
5. "Save" schreibt `map{n}.mul` (nur wenn Land geändert wurde) und `staidx{n}.mul`/`statics{n}.mul` in den Output-Ordner,
   dazu ein Protokoll `RegionProtocol_*.xml`. Die Client-Dateien werden nicht verändert.

## Erweiterung 3D (Ansatz für später)

`RegionData` ist vom Renderer unabhängig:
- `RegionCell` enthält pro Position Land-ID, die z-Werte aller vier Ecken (`LandZ`, `ZRight`, `ZBottom`, `ZLeft`) und die Statics.
  Daraus lässt sich direkt ein Höhen-Mesh bauen (2 Dreiecke pro Tile, Textur über `TextureId`/`ArtPixelCache.GetTexture`).
- Statics können als Billboards (Art-Bild) an Position (x, y, z) gesetzt werden.
- `IRegionRenderer` (in `IsoRenderer.cs`) beschreibt die Schnittstelle. Ein 3D-Renderer (z. B. OpenTK `GLControl`)
  implementiert sie und wird statt `IsoViewControl` in `MapRegionViewerForm` eingesetzt.
  Prüfung, Protokoll und Bearbeitung (`RegionAnalyzer`, `RegionEditSession`) bleiben unverändert.
