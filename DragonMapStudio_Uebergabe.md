# Dragon Map Studio – Übergabe für ein neues Gespräch

**Bitte zu Beginn des neuen Gesprächs diese Datei mitgeben.**

## Rahmen

* **Repo:** `jedi661/UoFiddlerPixel`. Bis „Sprung 19“ lief die Arbeit auf `claude/zen-bell-vjuuhp`, der Besitzer hat das nach `master` übernommen. „Werkzeuge 20“ liegt auf `claude/relaxed-ramanujan-07n3sw`, abgezweigt von `master`. Im neuen Gespräch auf dem Branch arbeiten, den die Sitzung vorgibt.
* **Mein Teil:** nur `UoFiddler.Plugin.ConverterMultiTextPlugin/DragonMapStudio/**` und die `.csproj` des Plugins. Am restlichen UoFiddler ändere ich nichts.
  * Alles bis einschließlich Commit `4120625` („New design“) stammt vom Besitzer selbst.
  * Alles danach stammt von Claude.
* **Stand:** siehe letzter Commit auf dem Branch. Im Fenstertitel steht **„Stand 23.11.2026 – Werkstatt 28“**. Das ist `BuildMark` in `DragonMapStudioForm.CgiLayout.cs` und wird bei jeder Änderung hochgezählt, damit man sieht, ob die neue DLL läuft.
* **Arbeitsweise:**
  * Antworten auf Deutsch.
  * Nach jeder Änderung `DragonMapStudio_Hoehleneingang.zip` im Repo-Hauptordner neu bauen.
  * Ins ZIP kommen **nur Dateien, die seit `4120625` geändert wurden**: `git diff --name-only 4120625 HEAD`, nur Dateien, die existieren. Die Pfade sind relativ zum Repo, die `.csproj` ist dabei.
  * Danach committen, auf den Branch pushen und dem Besitzer das ZIP schicken.
  * Der Besitzer baut das ZIP selbst ein.
* **Doku für den Benutzer:** `DragonMapStudio/LIESMICH_DragonMapStudio.md`. Neue Funktionen bitte dort beschreiben.
* **Prüfen ohne Oberfläche:**
  * Kompilieren mit `dotnet build`. Das Projekt zielt auf Windows Forms. Unter Linux: `apt-get install dotnet-sdk-8.0`, dann ein Prüfprojekt (net8.0) mit `PackageDownload Microsoft.WindowsDesktop.App.Ref 8.0.0` und `Reference`s auf dessen `ref/net8.0/*.dll` (System.Windows.Forms, Primitives, Drawing.Common, Design …). Es bindet `Ultima/**/*.cs`, `UoFiddler.Plugin.UopPacker/Classes/*.cs` und `DragonMapStudio/**/*.cs` ein, dazu einen Stub für `UoFiddler.Controls.Classes.Options.AppDataPath`.
  * Logik lässt sich mit kleinen Konsolenprogrammen testen, die `Core/*` nutzen, z. B. `DragonCompiler.BuildInMemory` mit den Regeln aus `DragonData/Scripts` und `DragonData/Addon/dragon-mod.act`.

## Grundlagen Dragon-Farbkarten

* Eine 8-Bit-Farbkarte, bei der jede Farbe für Gruppe, Höhe und Tiles steht (`maptrans`, `groups`).
* Übergänge über 8-Nachbar-Masken in der Reihenfolge NW N NE E SE S SW W. Dazu `betweentrans`, `statbetweentrans` und `items.txt`.
* **Eine Farbe hat genau eine Höhe.** Holzboden `1f` (0406–0409) gibt es z. B. nur mit Z 0. In der Palette sind nur noch 7 Plätze frei.

## Aufbau (Ordner `DragonMapStudio`)

* **Core:**
  * `DragonCompiler.cs`: Farbkarte → Land und Statics. `Build` und `BuildInMemory` nutzen denselben Weg. Optionen: `ExtraStatics`, `ExtraStaticsComplete`, `ExtraHeights`.
  * `StaticsLayer.cs`: Datei `<karte>.statics.bin` mit Magic „DMSL“. Enthält die Statics (x, y, dz, id, hue, Flag `Absolute`) und danach die **Höhen-Ebene** (x, y, z, Farbe).
  * Weitere Dateien:
    * `ColorStamp.cs`: Farb-Stempel `*.dmstamp.json`, transparent ist `0xFFFF`.
    * `TilePattern.cs`: Muster-Pinsel mit `PatternHeightMode`.
    * `CgiToColor.cs`: CGI → Farben.
    * `RuleLearner.cs`: lernt Regeln und Skripte.
    * `LandTransitions.cs`
    * `CaveEntranceBuilder.cs`, `CaveColorStamp.cs`: Höhlen und Eingänge.
    * `MountainShaper.cs`: Berg-Pinsel.
    * `CoastSmoother.cs`: Küste ohne Zacken (weiches Ufer-Profil nach euklidischem Abstand, Küstenlinie begradigen, Fehlersuche). Arbeitet in Blöcken von 128, wird auch vom Compiler genutzt (`CoastSoft`).
    * `StudioGuide.cs`: Inhalte des Hilfe-Fensters (DE / EN, einfache Auszeichnung).
    * `DungeonStyle.cs`: Dungeon-Stil lernen (`DungeonStyleTools.Analyze`: Lage-Schlüssel Boden/Leere + Nachbar-Maske N1 O2 S4 W8, Außenecken NO16 SO32 SW64 NW128 → Wand-Stapel 'id@dz', Deko, Void/Boden-Land) + Log, `Build` mit gelerntem Stil.
    * `Workshop.cs`: Kartenhöhe ermitteln, Leeren (Land → Leere auf eigener Höhe), Verschieben, Heben, eingebauter Stil `Classic()` (map0-Log).
    * `Builders.cs`: Dungeon-Generator (Räume + MST + Schleifen, L-Gänge), Keller unter der Karte (NoDraw 0002), Brücke, natürlicher Lauf; Sets `BridgeSet`, `StairSet`.
    * `CgiBrushes.cs`: Logik und Einstellungen der neuen CGI-Pinsel (Streuen, Wand, Hue, Fluss, Weg-Kurve, Rampe, Plateau, Unebenheit, Varianten, Drehen/Spiegeln, Ersetzen, auf Boden, Zauberstab, Lasso).
    * `CgiAreaAnalyzer.cs`: Bereichs-Auswertung als Log.
    * `MapData.cs`: CGI-Daten mit `StrokeGroup` und `UndoWholeGroup`.
* **Controls:**
  * `IsoMapView.cs` + `IsoMapView.Tools.cs` (partial): CGI-Ansicht; die neuen Werkzeuge hängen über `ToolMouseDown/Move/Up`, `ToolKeyDown`, `ApplyNewBrush`, `DrawToolOverlay` an.
  * `MapCanvas.cs`: 2D-Karten-Editor.
  * `StampBrowser.cs`: Stempel-Menü, andockbar für mehrere Monitore.
  * `CollapsibleSection.cs`, `MapNavigator.cs`
  * `ToolPalette.cs` (Werkzeug-Palette + `ToolPaletteDock` zum Ab- und Andocken), `ToolIcons.cs` (gezeichnete Symbole)
* **Dialogs:** `DungeonWorkshopForm.cs` (Dungeon-Werkstatt: Arbeitskopie `MapData.CopyRegion` / `WriteBack`, zwei `IsoMapView` oben/unten, `PlanEditor`, Skript-JSON, Log), `ReplaceDialog.cs`,
* **Dialogs (älter):** `CgiStampDialog.cs` (CGI → Farb-Stempel), `LearnRulesDialog.cs`, `SaveTemplateDialog.cs`, `HelpWindow.cs` (F1)
* **Formular-Teile `DragonMapStudioForm.*.cs`**, in der `.csproj` mit `DependentUpon` eingetragen:
  * `Cgi`: CGI-Bau.
  * `CgiLayout`: Seitenleiste, Werkzeugleisten, `BuildMark`.
  * `CgiToColor`: CGI ↔ Farbkarte, Aktualisieren.
  * `Caves`: Höhlen.
  * `EditorTools`: Karten-Editor mit Pinsel-Katalog, Bereichs-CGI-Vorschau und Einmal-Stempel.
  * `Pattern`: Muster-Pinsel, Live-CGI und Höhen-Ebene.
  * `Tools`: Karte bauen über `StartBuild`.
  * `Templates`: Vorlagen und Multis als Stempel.
  * `HeightLayer`: „Höhe behalten“ für die normalen Farb-Werkzeuge (`MapCanvas.CellColour`, `StepClosing`), Anzeige und Löschen der Höhen-Ebene.
  * `Workspace`: Paletten, Optionsleisten, 👁-Ansicht-Menüs, CGI-Kontextmenü, Küste-glätten-Bedienung, F1.
  * `Builders`: Formen im Karten-Editor (Halbkreis, Dreieck, Neigungs-Linie, natürlicher Lauf, Dungeon in Farben), CGI-Bauen (Brücke, Keller, Dungeon, Fluss-Lauf, „Unter der Karte“), Brücken-/Treppen-Sets.
  * `CgiBrushes`: Reiter „Pinsel“, Streu-/Wand-Sets, Ansicht-Filter, Lesezeichen, Verlauf-Fenster, Suchen & Ersetzen, Menü- und Leisten-Einträge der neuen Werkzeuge.
* **Rückgängig:**
  * 2D: `MapHistory` mit `Begin(name, group)`, `AddExtra(undo, redo)` und `UndoWholeGroup`, also schrittweise oder ganz.
  * CGI: `MapData.StrokeGroup`.

## Was bisher gebaut wurde

1. Höhlen und Höhleneingänge (CGI, 2D-Editor, Generator) mit Varianten, Übergängen, Entfernen und NODRAW-Statics. Dazu der Höhlen-Stempel mit Rahmen und Ziehen.
2. Berg-Pinsel bzw. Bereich zum Berg formen. Tile-IDs und Hex-Werte lassen sich in der CGI einblenden. Die Bereichs-Auswertung geht als Log in die Zwischenablage oder eine Datei.
3. Neue Seitenleisten mit Ein- und Ausklappen in CGI und Karten-Editor. Die Karte lässt sich abdocken und zoomen.
4. Karten-Editor: Rückgängig und Wiederholen, Pipette, Hover-Info, Kontextmenü, Pinsel-Katalog (Übergang, Küste, Höhe, Straße, Boden) und Bereich → schnelle CGI-Vorschau.
5. CGI → Farb-Stempel und ganze CGI-Karte → Farbkarte mit Statics-Ebene. Der Stempel-Ordner liegt im Projekt. Einmal oder mehrfach stempeln. CGI aktualisieren, ohne die Farbkarte zu löschen.
6. Regeln und Skripte lernen aus einem Stempel oder Bereich (Land, Statics, beides). Vorlagen und Multis direkt als Farb-Stempel.
7. Muster-Pinsel aus einem markierten Bereich:
   * einmal oder freihand
   * Land, Statics oder beides
   * Höhen-Modi: Original, Gelände behalten oder manuell
   * automatische Nachbesserung
   * Live-CGI-Fenster beim Zeichnen mit Höhe, IDs und Statics-Schalter, auch für alle normalen Werkzeuge
8. **Neu (Höhen 17), Höhen-Ebene:** Hat eine Farbe die gewünschte Höhe nicht, wird die Höhe pro Zelle in `.statics.bin` gemerkt (mit Rückgängig). Der Compiler setzt sie, solange die Zelle noch diese Farbe hat. Ohne Oberfläche getestet: Holzboden auf Gras Z 4 ergibt Z 4. **In der Oberfläche vom Besitzer noch nicht bestätigt.**

9. **Neu (Sprung 18), Sprung ins Live-CGI:** In der Editor-Werkzeugleiste gibt es den Knopf „⌖ Sprung“ mit „Maustaste belegen …“ und „Belegung löschen“. Belegt werden dürfen nur freie Tasten: Maustaste 4 und 5 sowie Strg oder Alt + Mitteltaste.
   * Speicherort: `StudioSettings.LiveJumpButton` und `LiveJumpModifiers`.
   * Ein Klick mit dieser Taste setzt `_livePin` und schaltet Live-CGI ein. Live-Zeichnen und „folgt Maus“ bleiben dabei an (Wunsch des Besitzers, „Sprung 19“).
   * `_livePin` wird gelöscht bei `MapEdited`, `StrokeEnded`, `Painting` und bei Mausbewegung mit „folgt Maus“.
   * Die Option „Sprung schaltet 'folgt Maus' aus“ (`StudioSettings.LiveJumpStopsFollow`, Standard aus) lässt die Ansicht stehen.
   * Code: `MapCanvas` hat `JumpButton`, `JumpModifiers`, `JumpClicked`, `CaptureNextButton` und `ButtonCaptured`, das Formular `AddLiveJumpButton` in `DragonMapStudioForm.Pattern.cs`.
   * Getestet vom Besitzer: Springen geht. „Sprung 18“ hatte Live-Zeichnen und „folgt Maus“ abgeschaltet, das ist in „Sprung 19“ behoben. Der Fix selbst ist nur kompiliert, in der Oberfläche noch nicht getestet.

10. **Neu (Werkzeuge 20):**
   * Werkzeug-Palette links in Editor und CGI, abdockbar (Lage in `StudioSettings.EditorPaletteWindow` / `CgiPaletteWindow`).
   * Optionsleiste oben je Werkzeug; die alten Werkzeug-Knöpfe sind nur versteckt.
   * „Höhe behalten“ für Pinsel, Formen und Füllen; die Höhen-Ebene wird im 2D-Editor angezeigt.
   * CGI-Kontextmenü.
   * Hilfe-Fenster DE / EN.
   * „Küste glätten“: Pinsel `IsoTool.CoastSmooth`, Bereich J, ganze Karte, nur Fehler, Fehler zeigen, Build-Option `CoastSoft` (Standard aus).
   * Englische Texte stehen in `DragonData/Lang/en.txt`.
   * **Alles nur kompiliert und der Küsten-Kern getestet, die Oberfläche hat der Besitzer noch nicht gesehen.**

11. **Neu (Pinsel 21):**
   * **CGI-Pinsel:** Streu-Pinsel (Sets aus den Dragon-Item-Dateien und eigene), Hue-Pinsel, Wand/Zaun, Fluss, Weg als Kurve, Rampe, Plateau/Terrassen, Unebenheit, Varianten, Übergänge-Pinsel, Härte.
   * **Auswahl:** Rahmen-Auswahl, Statics ziehen, Alt-Pipette, Lasso und Zauberstab als Maske (`MapData.EditMask`).
   * **Bearbeiten:** Bereich und Auswahl drehen/spiegeln (R X Y), Suchen & Ersetzen (E), auf Boden (D), Messen.
   * **Komfort:** Ansicht-Filter, Lesezeichen, Verlauf-Fenster (`MapData` hat jetzt Schritt-Namen).
   * Alles steht in Palette, Optionsleiste, Kontextmenü und Hilfe.
   * Der Kern ist getestet, die Oberfläche hat der Besitzer noch nicht gesehen.

12. **Neu (Bauen 22):**
   * **Karten-Editor:** Halbkreis, Dreieck, Neigungs-Linie, natürlicher Lauf (Pinsel oder Muster-Stempel), Dungeon in Farben.
   * **CGI-Ansicht:** Brücke, Keller unter der Karte mit Treppe, Ansicht „Unter der Karte“ mit Banner und Bau-Ebene, Dungeon-Generator (Höhle, gebaut, unter der Karte), Fluss-Lauf.
   * Der Kern ist getestet, die Oberfläche hat der Besitzer noch nicht gesehen.

13. **Neu (Dungeon 23):**
   * Der frühere „Höhle“-Dungeon heißt jetzt **Bergwall** (eigenes Werkzeug).
   * Der Dungeon-Generator baut nur noch mit Boden-Textur, Void-Land und Wand-Statics.
   * Stil lernen aus einem echten Dungeon und Analyse-Log (Datei + Zwischenablage).
   * Test: 390 von 390 Wandstücken richtig nachgebaut.

14. **Neu (Dungeon 24):**
   * Auswertung des echten map0-Logs.
   * Innenwände gelernt (Masken `WallRule.InnerAlongX/Y/Post`).
   * Rollen Wand / Wand-Deko (`Extras`) / Deko / Einrichtung / Boden.
   * Grundriss „Gitter“ (BSP, Zellart `DungeonPlan.Inner`, Türen).

15. **Neu (Werkstatt 25):**
   * Dungeon-Werkstatt als eigenes Fenster.
   * Zoom bis 1:32 (Farb-Übersicht unter 1:4).
   * Berg-Dungeon und Bergwall als Arten.
   * Keller mit Loch nur auf Kartenhöhe.
   * Etagen-Regler.
   * Leere wählbar (NoDraw / Schwarz / Katalog).

16. **Neu (Werkstatt 26):**
   * Fluss-Lauf: Breite am Anfang / Ende (`CourseSettings.Width/EndWidth/WidthAt`), die Mitte bleibt nach den Übergängen Wasser (`CgiBrushes.River(..., core)` + `RestoreWater`), optional Wasser-Statics.
   * Brücken aus Statics: `Core/BridgeKit.cs` – Teile-Suche in der TileData (Namen + Flags), Richtung aus dem Art-Bild (Kovarianz), `BridgeTemplate` (Anfang / wiederholte Mitte / Ende, Pfeiler bis zum Boden, Treppen), Vorschau der echten Teile beim Ziehen.
   * Brücken-Designer `Dialogs/BridgeDesignerForm.cs` (Testfeld, Muster von Hand, aus Karte / Multi, speichern nach Kategorie unter `SettingsFolder/Bruecken`).
   * Katalog `Controls/ArtCatalog.cs` + `Core/ArtCategories.cs` (Statics / Land / Multis, Suche, eingebaute und eigene Kategorien in `katalog_kategorien.json`).
   * Neue CGI-Werkzeuge `IsoTool.Eraser` (Radierer-Pinsel) und `IsoTool.Stairs` (`Builders.BuildStairs`).
   * Werkstatt: Werkzeugleiste mit Radierer / Treppe / Höhe / Rampe / Multi, Pinselgröße, Gitter, Statics, Katalog unten, Hilfe-Knöpfe (`ws-generate`, `ws-2d`, `ws-3d`), fremde Karte (`Dialogs/ForeignMapForm.cs`), Vorlagen `Core/WorkshopPiece.cs` (`SettingsFolder/Werkstatt`).

17. **Neu (Werkstatt 27):** `DragonMapStudioForm.ToolOptions.cs`
   * Schnell-Einstellungen je CGI-Werkzeug in der Optionsleiste (`UpdateQuickOptions`, per Reflection aus den Settings-Klassen, DisplayName = Beschriftung), ⚙ = alle.
   * Seitenleiste ein / aus (F8), als eigenes Fenster mit Rückgängig / Wiederholen, Paletten ein / aus (F7).
   * Hue: Farbfeld, Name, `Dialogs/HuePickerForm.cs`, Favoriten (`CgiBrushSettings.FavoriteHues`).
   * Radierer `OnlyIds` (Auswahl merken), Multi-Reiter mit Katalog + sofortigem Bild + Auswahl löschen.
   * Weg-Vorlagen (`RoadPreset`), Weg mit Stempel (`IsoMapView.RoadWithStamp`, `FindStampPattern`), Varianten-Muster (`VariantSet`, `VariantsFromSet`), Wand Dicke + Punkt zu Punkt (`CgiBrushes.WallPath`), Unebenheit-Muster (`NoisePattern`, `NoiseValue`), Fluss-Grund wie Küste (`RiverSettings.BedDepth/BedTiles`).
   * 2D: Lauf-Arten (`CoursePreset`, `MapCanvas.CourseColours`: Kern / Rand / Außen), Editor `Dialogs/NamedListEditor.cs`.

18. **Neu (Werkstatt 28):**
   * Treppenhaus `Core/Staircase.cs` (Läufe 4 × 5 Z, Absatz, Wende gerade / rechts / links / U, gefüllt, nach unten) und `Staircase.Learn` (Treppen-Set aus Auswahl, `StairSet.Block/Landing`).
   * Brücke: `BridgeSettings.ArchStairs` (Bogen aus Stufen), Regler Länge / Breite / Bogen im Designer, Testfeld 112×112.
   * Werkzeug-Optionen als eigenes Fenster (Tabs je Einstellung + Skript JSON, Position gemerkt), Seitenleisten-Fenster merkt Position.
   * Drehen 90° tauscht gerichtete Teile (`CgiBrushes.TurnedId` über `BridgeKit.Turned`), Wand-Ecken (`WallSet.Corner`), Streu-Sets mischen + Rand ausdünnen, Übersichtskarte zeigt Lesezeichen + Fehler (`MapNavigator.Markers`).
   * Dungeon mit Etagen (`DungeonSettings.Floors`, `ConnectFloors` in der Werkstatt), Einrichtung lernen und an Wänden setzen (`DungeonLook.Furniture/FurniturePercent`).
   * `CoastSoft` ab Werk an (einmalig über `StudioSettings.SettingsLevel` 28), Vorlagen-Browser mit Bildern `Dialogs/TemplateBrowserForm.cs`.
   * Französisch `DragonData/Lang/fr.txt` jetzt vollständig (alle Texte wie en.txt).

## Offen / als Nächstes

* Werkstatt 26 testen lassen: Findet die TileData-Suche die richtigen Brücken-Teile? Stimmt die Richtungserkennung (W-O / N-S) der Geländer? Treppen-Gruppe von `BridgeKit.AutoStairs` (Reihenfolge geraten) prüfen.
* Werkstatt 25/26 in der Oberfläche testen lassen (fremde Karte laden – ganze Karte im Speicher, Vorlagen, Katalog).
* Dungeon 24: Ebenen z 0 / 22 mit Treppen (07a3–07a7) und Einrichtung je Raum lernen.
* Dungeon 23: Besitzer soll Deceit / Doom o. ä. lernen und den Log schicken → Regeln verfeinern (z. B. Stockwerke, Türen, Treppen zwischen Ebenen, Raumformen aus dem Original).
* Bauen 22 testen: Keller im Client begehbar? Treppen-Richtung passend? NoDraw-Verhalten auf dem Shard prüfen.
* Pinsel 21 in der Oberfläche testen lassen, vor allem Weg (Doppelklick), Lasso-Maske, Statics ziehen, Rahmen-Auswahl und Streu-Sets.
* Ausbau-Ideen: Wand-Sets mit Vorlagen ausliefern, Ecken automatisch (L-Formen), Streu-Pinsel mit mehreren Sets und Rand-Ausdünnung, Drehen mit Austausch der gerichteten IDs, Lesezeichen in der Kartenübersicht anzeigen.
* Werkzeuge 20 in der Oberfläche testen lassen: Palette, Abdocken, Optionsleiste, Kontextmenü, F1, Höhe behalten, Küste glätten (sieht die Küste wirklich ohne Zacken aus?).
* Danach evtl. `CoastSoft` ab Werk einschalten.
