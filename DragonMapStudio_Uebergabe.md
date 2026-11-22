# Dragon Map Studio – Übergabe für ein neues Gespräch

**Bitte zu Beginn des neuen Gesprächs diese Datei mitgeben.**

## Rahmen

* **Repo:** `jedi661/UoFiddlerPixel`. Gearbeitet wird auf dem Branch `claude/zen-bell-vjuuhp`. Bitte dort weiterarbeiten und pushen.
* **Mein Teil:** nur `UoFiddler.Plugin.ConverterMultiTextPlugin/DragonMapStudio/**` und die `.csproj` des Plugins. Am restlichen UoFiddler ändere ich nichts.
  * Alles bis einschließlich Commit `4120625` („New design“) stammt vom Besitzer selbst.
  * Alles danach stammt von Claude.
* **Stand:** siehe letzter Commit auf dem Branch. Im Fenstertitel steht **„Stand 22.11.2026 – Sprung 18“**. Das ist `BuildMark` in `DragonMapStudioForm.CgiLayout.cs` und wird bei jeder Änderung hochgezählt, damit man sieht, ob die neue DLL läuft.
* **Arbeitsweise:**
  * Antworten auf Deutsch.
  * Nach jeder Änderung `DragonMapStudio_Hoehleneingang.zip` im Repo-Hauptordner neu bauen.
  * Ins ZIP kommen **nur Dateien, die seit `4120625` geändert wurden**: `git diff --name-only 4120625 HEAD`, nur Dateien, die existieren. Die Pfade sind relativ zum Repo, die `.csproj` ist dabei.
  * Danach committen, auf den Branch pushen und dem Besitzer das ZIP schicken.
  * Der Besitzer baut das ZIP selbst ein.
* **Doku für den Benutzer:** `DragonMapStudio/LIESMICH_DragonMapStudio.md`. Neue Funktionen bitte dort beschreiben.
* **Prüfen ohne Oberfläche:**
  * Kompilieren mit `dotnet build`. Das Projekt zielt auf Windows Forms, unter Linux reicht dafür ein Prüfprojekt, das die Quellen einbindet.
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
    * `CgiAreaAnalyzer.cs`: Bereichs-Auswertung als Log.
    * `MapData.cs`: CGI-Daten mit `StrokeGroup` und `UndoWholeGroup`.
* **Controls:**
  * `IsoMapView.cs`: CGI-Ansicht.
  * `MapCanvas.cs`: 2D-Karten-Editor.
  * `StampBrowser.cs`: Stempel-Menü, andockbar für mehrere Monitore.
  * `CollapsibleSection.cs`, `MapNavigator.cs`
* **Dialogs:** `CgiStampDialog.cs` (CGI → Farb-Stempel), `LearnRulesDialog.cs`, `SaveTemplateDialog.cs`
* **Formular-Teile `DragonMapStudioForm.*.cs`**, in der `.csproj` mit `DependentUpon` eingetragen:
  * `Cgi`: CGI-Bau.
  * `CgiLayout`: Seitenleiste, Werkzeugleisten, `BuildMark`.
  * `CgiToColor`: CGI ↔ Farbkarte, Aktualisieren.
  * `Caves`: Höhlen.
  * `EditorTools`: Karten-Editor mit Pinsel-Katalog, Bereichs-CGI-Vorschau und Einmal-Stempel.
  * `Pattern`: Muster-Pinsel, Live-CGI und Höhen-Ebene.
  * `Tools`: Karte bauen über `StartBuild`.
  * `Templates`: Vorlagen und Multis als Stempel.
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
   * Ein Klick mit dieser Taste setzt `_livePin`: Live-CGI wird eingeschaltet, „folgt Maus“ ausgeschaltet. Erst beim nächsten Zeichnen folgt das Live-CGI wieder dem Stift.
   * Code: `MapCanvas` hat `JumpButton`, `JumpModifiers`, `JumpClicked`, `CaptureNextButton` und `ButtonCaptured`, das Formular `AddLiveJumpButton` in `DragonMapStudioForm.Pattern.cs`.
   * Nur kompiliert, in der Oberfläche noch nicht getestet.

## Offen / als Nächstes

* Höhen-Ebene und Sprungtaste in der Oberfläche testen lassen. Schon gemalte Böden muss der Besitzer einmal neu malen.
* Mögliche Erweiterung: die Höhen-Ebene auch beim normalen Farb-Pinsel nutzen, nicht nur bei Muster und Stempel. Außerdem die Höhen-Ebene im 2D-Editor anzeigen.
