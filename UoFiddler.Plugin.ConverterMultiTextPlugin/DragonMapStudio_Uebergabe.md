# Dragon Map Studio – Übergabe für ein neues Gespräch

**Bitte zu Beginn des neuen Gesprächs diese Datei mitgeben.**

## Rahmen

* **Repo:** `jedi661/UoFiddlerPixel`. Bis „Sprung 19“ lief die Arbeit auf `claude/zen-bell-vjuuhp`, der Besitzer hat das nach `master` übernommen. „Werkzeuge 20“ liegt auf `claude/relaxed-ramanujan-07n3sw`, abgezweigt von `master`. Im neuen Gespräch auf dem Branch arbeiten, den die Sitzung vorgibt.
* **Mein Teil:** nur `UoFiddler.Plugin.ConverterMultiTextPlugin/DragonMapStudio/**` und die `.csproj` des Plugins. Am restlichen UoFiddler ändere ich nichts.
  * Alles bis einschließlich Commit `4120625` („New design“) stammt vom Besitzer selbst.
  * Alles danach stammt von Claude.
* **Stand:** siehe letzter Commit auf dem Branch. Im Fenstertitel steht **„Stand 23.11.2026 – Werkstatt 34“**. Das ist `BuildMark` in `DragonMapStudioForm.CgiLayout.cs` und wird bei jeder Änderung hochgezählt, damit man sieht, ob die neue DLL läuft.
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

19. **Neu (Werkstatt 29): Test-Assistent**
   * `Core/SelfTests.cs`: 17 automatische Tests auf Karten im Speicher mit den Client-Dateien des Besitzers, jeder Wert im Protokoll.
   * `Dialogs/TestAssistantForm.cs`: Prüfliste (21 Schritte) mit Vorbereiten, 📷 Aufnehmen (Bildschirmfoto aller Fenster, Werkzeug + Einstellungen, alle Änderungen seit Schritt-Beginn über `MapData.RecentSteps`), ✔ / ✖ / ? + Kommentar, Fehler-Mitschnitt (FirstChanceException), Bericht-ZIP in „Dokumente“.
   * `Workshop.ConnectFloors` (Treppen zwischen Etagen) aus der Werkstatt in den Kern verschoben, damit es testbar ist.
   * **Wenn der Besitzer einen Testbericht schickt:** `bericht.txt` zuerst, dann `automatisch_details.txt`, dann die `schrittNN_daten.txt` der ✖-Schritte.

20. **Neu (Werkstatt 30) nach dem ersten Testbericht:**
   * Bericht zeigte: Brücken-Bretter W-O / N-S gemischt → `BridgeGenSettings.DeckY`, `AutoPieces` wählt Bretter nach Bild-Richtung (z. B. acb9 / acba „log bridge“).
   * Wand-Set des Besitzers „Mauer“ hatte X = 0001 (nodraw) → `BridgeKit.AutoWall` + Knopf „Wand-Set aus TileData“ (z. B. 0025 / 0026 stone wall), Test warnt bei kaputten Sets.
   * `AutoStairs` nur Stufen mit Höhe 5 (03ee ist h10). Kategorien ohne Missing_Name / nodraw / Fenster im Licht.
   * Automatische Tests machen jetzt Bilder (`TestPicture`, gezeichnet mit `IsoMapView` und der echten Art, `autoNN_*.png`), Knopf „Automatisch + Bericht“.
   * Daten aus dem Bericht (Client des Besitzers): TileData 1 114 112 Items (erweitert), höchste ID ffdc; Geländer 07d7 / 07d8 „railing“, Pfosten 0013, Pfeiler 00db, Treppen 03ef–0402 „stone stairs“, 070a… „marble stairs“.

21. **Neu (Werkstatt 31):** Treppen-Neigung aus dem Art-Bild (`BridgeKit.StairUp`: Oberkante links / Mitte / rechts gegen einen flachen Block, mit künstlichen Bildern geprüft), `AutoStairs` ordnet danach zu (Fallback: alte Reihenfolge, als „geraten“ markiert), `FixStairs` + Knopf „Richtungen aus Bild“ beim Werkzeug Treppe. Test protokolliert die Messwerte je Stufe; Testbilder je Richtung / Wende einzeln und größer. Anlass: Testbericht 20261123_2155 zeigte falsch geneigte Treppen (03ef–03f2 nur der Reihe nach zugeordnet).

22. **Neu (Werkstatt 32):** Treppen-Designer (`Dialogs/StairDesignerForm.cs`) mit Modellen `Core/StairModel.cs` (`SettingsFolder/Treppen/<Kategorie>`; Drehen: Positionen + Stufen nach `StairUp`, sonst `Turned`), Werkzeug Treppe mit „Modell“ und „Modell mehrmals“ (`StaircaseSettings.Model/Repeats`, `IsoMapView.StairPieces`). „📋 Log + Bild“ (`Dialogs/DesignLog.cs`, `SettingsFolder/Logs`) in Treppen- und Brücken-Designer. Selbsttest „Treppen-Modell“, Prüflisten-Schritt Treppen-Designer.

23. **Neu (Werkstatt 33):** Treppen-Designer „Höhe: auf den Stapel (TileData-Höhe)“ / „feste Höhe“ (vorher addierten sich Z + und Stapeln → Stufe 5 zu hoch), Teil-Höhe aus der TileData angezeigt, Auswahl ▲ h / ▼ h / ±1 / z = …; `StaircaseSettings.HeightFromTileData` (Stufen-, Block-, Absatz-Höhe aus der TileData, Blöcke von unten gestapelt).

24. **Neu (Werkstatt 34)** nach dem Log des Besitzers (Treppe_20261123_225537): Steinblock `03ee` h10 / Holz `0721` h10 haben das Flag Bridge → der Client stapelt sie mit halber Höhe (5). Neu `UoArtCache.StackHeight` (= `ItemData.CalcHeight`) für das Stapeln in der CGI-Ansicht und im Treppenbau. Erkannte Neigungen stimmen mit den Treppen des Besitzers überein (03ef N, 03f0 W, 03f1 S, 03f2 O; Holz 0722 N, 0723 W; Ecken 03f3–03f6). Treppen-Modelle haben eine Dreh-Tabelle (`StairTurn4`, auto + von Hand), `GuessUp`, Blöcke werden beim Drehen nicht mehr getauscht. Werkzeug Treppe: Ziehen = `Staircase.Run` (je Feld eine Stufe, Blöcke darunter). Designer: Auswahl verschieben / löschen, Rückgängig mit Text + Strg+Z/Y, „Markierte Treppe speichern“, Modell aus Auswahl.
25. **Neu (Werkstatt 35) Muster-Baukasten:** `Core/Motif.cs` (Motif mit North/East/South/West-Listen von `PieceStatic`, `Capture`, `Turn`, `AutoVariants`, `StairRise`, `HeightOf`, `Layout` – Wiederholung entlang der Zieh-Achse mit Periode = Breite/Tiefe + `Gap`, Höhe Level/Ground/Rise, Etagen –, `Build` für die CGI-Karte, Speichern in `SettingsFolder/Muster/<Kategorie>`). `MotifDrawSettings` in `CgiBrushSettings.MotifDraw` (+ `Motif` = „Kategorie / Name“). Fenster `Dialogs/MotifDesignerForm.cs` (Testfeld wie Treppen-Designer, Baum Kategorie ▸ Name ▸ Richtung, Richtung wählen, übernehmen, automatisch drehen, Probe ziehen, Log + Bild). Werkzeug `IsoTool.Motif` (Klick/Ziehen, R dreht, Geist-Vorschau) und `MapTool.Motif` im Karten-Editor (`MapCanvas.MotifRequested` / `MotifPreview` → `DragonMapStudioForm.Motifs.cs` schreibt in die Statics-Ebene mit Undo über `_history.AddExtra`). Selbsttest „Muster“, Hilfe-Thema `motif`, en/fr.
26. **Neu (Werkstatt 36)** nach dem ersten Test: `MotifDrawSettings.Repeat` (`Along` = Reihe fortsetzen, Standard / `Across` = Kopien nebeneinander). `Motif.ChooseVariant(want, alongDrag)` wählt eine erlaubte, gebaute Version, deren Lauf-Achse (`AxisOf`: Fußabdruck, bei Quadrat Bild-Richtung über `BridgeKit.Orientation`) zur Zieh-Achse passt; sonst wird entlang der Achse des Musters gesetzt. `AllowNorth/East/South/West` + `Allowed`, `SetAllowed`, `NextAllowed` (R). Designer: Achsen-Prüfung beim Übernehmen, Häkchen erlaubte Richtungen. Fix: `ClearQuickItems` (erst alle entfernen, dann per BeginInvoke entsorgen) gegen ObjectDisposedException in der Überlauf-Leiste.
27. **Neu (Werkstatt 37)** Muster: `MotifPart` (Middle/Start/End/Corner, Extra-Dictionary „Start.East“ …, Ecken unter Nord=NO, Ost=SO, Süd=SW, West=NW; `CornerOf(in, out)`), `Path(points)` (Strecken achsen-gerastert, Ecke am Knick-Feld, Strecken davor immer genau angepasst), `FitLast`, `MotifAlt` (Zufalls-Varianten mit Gewicht, `MainWeight`), `MotifMode` Line/Points/Radius (`Scatter` mit Seed – nach jedem Setzen neuer Seed), `MotifFlatten` + `FlattenLand` / `Level` (Land + Statics mitnehmen), `Build(..., out land)` / `BuildPath`. Projekt-Muster `DragonData\Muster` (Shipped, eigene gewinnt), Export .json/.zip, Import, `ExportToFolder`. CGI: Punkt-Modus im Werkzeug Muster, Begradigen-Dialog (`LevelGroundDialog`), „→ Muster“. Karten-Editor: `MapCanvas.MotifPointMode` / `MotifPathRequested` / `MotifPathPreview`. Neu `Core/DirectionGroups.cs` + `Dialogs/DirectionGroupsForm.cs` (Richtungs-Tabelle Item/Land, Treppe = hinauf nach; Haken in `BridgeKit.StairUp`, `StairModel.UpOf/TurnedPiece` (+ `ClearCaches`), `CgiBrushes.TurnedId`, `TransformArea` für Land). Selbsttest „Burgmauer Punkt zu Punkt“.
28. **Neu (Werkstatt 38)** Gebiete als Stempel: `Controls/IsoMapView.AreaMark.cs` (Bereich: Strg+Klick ×2 = Ecken mit Scrollen dazwischen, Umschalt+Ziehen = Vereinigung mit dem alten Bereich, Rand-Scrollen per Timer beim Ziehen, `SetAreaAndShow`, `AskArea`, `LastTile`), `Dialogs/AreaDialog.cs` (Koordinaten). `ForeignMapForm(configure, workshop: false)` = Quelle für Gebiets-Stempel: Events `TemplateTaken` (→ `SaveTemplate`) und `ColorStampRequested` (→ `AreaToColorStamp(map, area)`, aus `CgiAreaToColorStamp` herausgelöst), Grenze 1024×1024. `MapTemplate.OriginX/Y/Z` (von `CaptureArea` gesetzt) + `IsoMapView.PlaceTemplateAtOrigin(originalHeight)`, Knöpfe beim Werkzeug Vorlage. Palette „Gebiet aus fremder Karte“, „Bereich per Koordinaten“; Hilfe-Thema `areastamp`; `DragonMapStudioForm.AreaStamps.cs`.
29. **Neu (Werkstatt 39)** Bereichs-Fenster: `DragonMapStudioForm.Region.cs` (`OpenRegion`: `CopyRegion(area ± 8)`, die Controls von `tabCgi` (toolStripCgi, toolStripCgiLand, splitCgi) wandern in `RegionEditorForm.WorkspaceHost`, Platzhalter im Tab, `SetCgiMap(copy)`; `EndRegion(apply)`: Controls zurück, `WriteBack` = ein Undo-Schritt, `SetCgiMap(main)`; `RegionBlocks()` sperrt `ConfirmDiscardCgi` / `SaveCgi`). `Dialogs/RegionEditorForm.cs` (Schnell-Werkzeuge, Ebenen-Ansicht = zweite IsoMapView mit `UnderView = !main.UnderView`, folgt `ViewChanged` / `MapChanged`; Gebäude-Liste; Panes per „⧉ lösen“ als eigene Fenster). `Core/TownTools.cs` (`FindBuildings` = 8-Nachbarn-Gruppen von Wand / Dach / Boden-Statics (`UoArtCache.ItemKind`), `Rearrange` = Regal-Packen in Reihen, Boden mitnehmen / begradigen / Freiflächen-Tile). `Controls/IsoMapView.Lines.cs` (`CoordLines`, `TileInfo`, `CoordOffset`). Selbsttest „Bereich“, Hilfe-Thema `region`.
30. **Neu (Werkstatt 40)** `DragonMapStudioForm.CaveTools.cs` (Schnell-Optionen Höhle / Eingang als Spiegel der Seitenleisten-Felder, Anleitung, Höhlen-Boden/-Items-Fenster, Ansicht-Menü). `IsoMapView.SurfaceHidden` (Land nur Umriss, Statics ab Bodenhöhe aus) und `HeightScale` (in `IsoMapView.Lines.cs`, Bildschirm-Koordinaten). `DragonMapStudioForm.Layout.cs` (`SaveLayoutSoon` = Settings nach 1,5 s schreiben; Paletten / Seite aus, Seitenbreite; Bereichs-Fenster `RegionWindow`, Teiler, gelöste Panes). `DragonMapStudioForm.Sync.cs` (`MapHistory.Edited` + `MapData.Edited` liefern das geänderte Rechteck; `_editorPending` / `_cgiPending`; Tab-Wechsel → `RebuildCgiArea` (aus `RefreshCgiFromEditor` herausgelöst, ohne Rückfrage) bzw. `CgiIntoEditorQuiet`; Konflikt-Frage; `WarnPending` in SaveMap / SaveCgi / FormClosing; Modus `SyncMode`; `_viewSyncing` unterdrückt eigene Änderungen).

## Offen / als Nächstes

* Werkstatt 40 testen lassen: Abgleich beim Tab-Wechsel mit echter Karte (Dauer, Farben beim CGI → Farbkarte), Oberfläche aus + Höhen-Skala beim Keller, gemerkte Fensterlage nach Neustart.

* Werkstatt 39 testen lassen: Bereichs-Fenster mit einer echten Stadt (Arbeitsfläche zieht korrekt um? Palette / Seitenleiste abdocken im Fenster, Fertig / Abbrechen, Strg+Z danach), Gebäude-Erkennung mit echten Häusern (Wände / Dächer aus der TileData), Keller unter einem Haus mit der Ebenen-Ansicht.

* Werkstatt 38 testen lassen: eine Stadt aus einer alten map#.mul als Vorlage und als Farb-Stempel übernehmen und in eine neue Karte setzen (Höhen, Statics, Übergänge am Rand).

* Werkstatt 37 testen lassen: Burgmauer mit Türmen / Ecken im Client, Bäume im Radius, Boden begradigen unter einem echten Haus, Richtungs-Tabelle für die Treppen des Besitzers (03ef–03f2) anlegen und ins Projekt (DragonData\Richtungen) übernehmen.

* Werkstatt 35 testen lassen: Muster-Baukasten mit echten Zinnen / Fenster-Mauern – dreht „automatisch“ die Teile richtig? Wiederholt das Ziehen lückenlos? Karten-Editor → Statics-Ebene → CGI-Vorschau.

* Werkstatt 26 testen lassen: Findet die TileData-Suche die richtigen Brücken-Teile? Stimmt die Richtungserkennung (W-O / N-S) der Geländer? Treppen-Gruppe von `BridgeKit.AutoStairs` (Reihenfolge geraten) prüfen.
* Werkstatt 25/26 in der Oberfläche testen lassen (fremde Karte laden – ganze Karte im Speicher, Vorlagen, Katalog).
* Dungeon 24: Ebenen z 0 / 22 mit Treppen (07a3–07a7) und Einrichtung je Raum lernen.
* Dungeon 23: Besitzer soll Deceit / Doom o. ä. lernen und den Log schicken → Regeln verfeinern (z. B. Stockwerke, Türen, Treppen zwischen Ebenen, Raumformen aus dem Original).
* Bauen 22 testen: Keller im Client begehbar? Treppen-Richtung passend? NoDraw-Verhalten auf dem Shard prüfen.
* Pinsel 21 in der Oberfläche testen lassen, vor allem Weg (Doppelklick), Lasso-Maske, Statics ziehen, Rahmen-Auswahl und Streu-Sets.
* Ausbau-Ideen: Wand-Sets mit Vorlagen ausliefern, Ecken automatisch (L-Formen), Streu-Pinsel mit mehreren Sets und Rand-Ausdünnung, Drehen mit Austausch der gerichteten IDs, Lesezeichen in der Kartenübersicht anzeigen.
* Werkzeuge 20 in der Oberfläche testen lassen: Palette, Abdocken, Optionsleiste, Kontextmenü, F1, Höhe behalten, Küste glätten (sieht die Küste wirklich ohne Zacken aus?).
* Danach evtl. `CoastSoft` ab Werk einschalten.
