/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System.Collections.Generic;

namespace UoFiddler.Controls.MapViewer.Help
{
    /// <summary>A help page. Text format: "# " heading, "## " sub heading, "- " list, "> " hint, other lines = text.</summary>
    public sealed class HelpTopic
    {
        public HelpTopic(string key, string title, string text, params HelpTopic[] children)
        {
            Key = key;
            Title = title;
            Text = text;
            Children = new List<HelpTopic>(children);
        }

        public string Key { get; }
        public string Title { get; }
        public string Text { get; }
        public List<HelpTopic> Children { get; }
    }

    /// <summary>
    /// Help texts of the map tools (map tab, Region Viewer, building, Map Editor, transitions).
    /// New pages: add a <see cref="HelpTopic"/> here; the key is used by F1 in the windows.
    /// </summary>
    public static class MapHelpContent
    {
        public const string Overview = "overview";
        public const string MapTab = "maptab";
        public const string ManageMaps = "maptab.manage";
        public const string RegionViewer = "viewer";
        public const string ViewerTile = "viewer.tile";
        public const string ViewerErrors = "viewer.errors";
        public const string ViewerBuild = "viewer.build";
        public const string ViewerSettings = "viewer.settings";
        public const string Save = "save";
        public const string Multis = "multis";
        public const string ExportArea = "exportarea";
        public const string Editor = "editor";
        public const string EditorSource = "editor.source";
        public const string EditorClip = "editor.clip";
        public const string EditorTarget = "editor.target";
        public const string EditorTools = "editor.tools";
        public const string Palette = "editor.palette";
        public const string BrushHelp = "editor.brush";
        public const string BrushEditor = "editor.brusheditor";
        public const string Shapes = "editor.shapes";
        public const string Generator = "editor.generator";
        public const string Transitions = "transitions";
        public const string TerrainGroups = "transitions.groups";
        public const string Keys = "keys";
        public const string Files = "files";
        public const string Problems = "problems";
        public const string MultiMapImages = "multimap.images";

        public static List<HelpTopic> Topics { get; } = new List<HelpTopic>
        {
            new HelpTopic(Overview, "Überblick", @"
# Überblick – Karten bearbeiten mit UOFiddler
Die Kartenwerkzeuge bestehen aus vier Teilen, die zusammenarbeiten:
- Kartenreiter: Karte ansehen, Bereich markieren, Karten verwalten.
- Region Viewer: einen Bereich wie im Client (isometrisch) ansehen, Fehler suchen und reparieren, Gebäude setzen.
- Map Editor: Bereiche kopieren und auf derselben oder einer anderen Karte einfügen, mit Pinseln malen, Übergänge setzen.
- Multis / Bibliothek: Gebäude aus multi.mul, eigener Bibliothek oder Dateien laden, speichern und exportieren.

## Grundprinzip
Alle Änderungen passieren zuerst nur im Speicher. Die Kartendateien werden erst bei ""Save..."" geschrieben.
Bis dahin kann jeder Schritt mit Strg+Z rückgängig gemacht werden.
> Tipp: Zum Ausprobieren in einen anderen Ordner speichern (z. B. den Output-Ordner). Die Originaldateien bleiben dann unberührt.

## Typischer Ablauf
- Im Kartenreiter mit ""Mark"" einen Bereich markieren.
- ""View Region"" zum Prüfen und Reparieren, ""Map Editor..."" zum Kopieren und Malen.
- Ergebnis ansehen, dann ""Save..."": Ordner, Dateinummer, Land/Statics und Backup wählen.
- Fertige Dateien manuell in den Client- bzw. Server-Ordner kopieren (Client und Server vorher schließen).

F1 öffnet in jedem Fenster die passende Hilfeseite."),

            new HelpTopic(MapTab, "Kartenreiter", @"
# Kartenreiter (Map)
## Karte wählen
- ""Map: …"" in der Werkzeugleiste oder Rechtsklick auf die Karte: zeigt alle Karten aus der Kartenliste.
- Fehlende Karten stehen mit ""(missing)"" in der Liste. Beim Anklicken wird erklärt, wie man die Dateien einbindet; das Programm stürzt nicht ab.

## Bereich markieren
- ""Mark"" einschalten, dann mit der Maus einen Rahmen aufziehen. Der Bereich bleibt rot markiert.
- ""View Region"": öffnet den markierten Bereich im Region Viewer (max. 512 x 512).
- ""Export Area..."": übernimmt die Statics des Bereichs als Gebäude (Datei, Bibliothek, multi.mul, ServUO).
- ""Map Editor..."": öffnet den Map Editor, der markierte Bereich wird sofort kopiert.
- ""Clear"": Markierung entfernen.
- Rechtsklick ""View Area Here"": öffnet 64 x 64 Felder um die Maus im Region Viewer.",
                new HelpTopic(ManageMaps, "Karten verwalten", @"
# Karten verwalten (Manage Maps…)
Im Menü ""Map: …"" -> ""Manage Maps…"".
- Eingebaute Karten (Felucca … Map 8) können nur umbenannt oder ausgeblendet werden.
- ""New map"": eigene Karte hinzufügen (ID, Name, Dateinummer, Breite, Höhe, optional eigener Ordner).
- ""Search Directory"": durchsucht einen Ordner nach mapX.mul / mapXLegacyMUL.uop und schlägt die Größe vor.
  Die Größe wird aus der Dateigröße berechnet – bitte prüfen.
- ""Show in map list"": nur angehakte Karten erscheinen in der Auswahl.
Die Liste wird in MapDefinitions.xml (AppData) gespeichert.")),

            new HelpTopic(RegionViewer, "Region Viewer", @"
# Region Viewer
Zeigt einen Bereich wie im Client: Land mit Texturen und Höhen, Statics mit Hue.

## Bedienung der Ansicht
- Linke Maus ziehen: verschieben. Mausrad: zoomen. Klick: Feld auswählen (Reiter ""Tile"" zeigt den Inhalt).
- Werkzeugleiste: Zoom, ""Fit"" (alles zeigen), Land/Statics ein- und ausblenden, gestreckte Texturen, Höhenfarben, Raster, Fehleranzeige.
- ""Reload"": liest den Bereich neu. ""Show on map"": springt im Kartenreiter zu dieser Stelle.
- Rechtsklick in der Ansicht: Fehler markieren, Static entfernen, Koordinaten kopieren.

## Rückgängig und Speichern
- ""Undo"" (Strg+Z) nimmt den letzten Schritt zurück, ""Redo"" (Strg+Y) macht ihn wieder. ""Undo all"" nimmt alle Schritte dieses Fensters zurück.
- ""Save"" und ""Backups..."" siehe Seite ""Speichern und Backups"".
- Beim Schließen mit ungespeicherten Änderungen: Ja = speichern, Nein = verwerfen, Abbrechen = offen lassen.",
                new HelpTopic(ViewerTile, "Reiter Tile", @"
# Reiter ""Tile""
Zeigt alles auf dem angeklickten Feld: Land (ID, z, Name) und alle Statics (ID, z, Hue, Flags).
- ""Remove static"": entfernt den ausgewählten Static.
- ""Z +1"" / ""Z -1"": verschiebt ihn in der Höhe (Shift = 5).
- ""Mark error..."": trägt einen eigenen Fehler mit Notiz ins Protokoll ein.
- Unten: Vorschaubild des Statics."),
                new HelpTopic(ViewerErrors, "Reiter Errors / Protocol", @"
# Reiter ""Errors / Protocol""
Beim Öffnen wird der Bereich geprüft. Die Fehler stehen in der Liste und sind in der Ansicht farbig markiert.
## Geprüft wird
- Ungültige Land-/Static-IDs.
- Doppelte Statics (gleiche ID, Position, z).
- Statics unter dem Land (unsichtbar).
- Steiles Land (Höhensprung größer als der eingestellte Wert).
- Schwebende Statics (Lücke größer als der eingestellte Wert).
## Bearbeiten
- Doppelklick: springt zum Fehler.
- ""Fix all (safe)"": repariert alles, was sicher automatisch geht. ""Check fixable"" + ""Fix checked"": nur angehakte.
- ""Ignore"" / ""Reopen"" / ""Note..."" / ""Delete"": Status und Notizen.
- ""Export CSV..."", ""Save protocol..."", ""Load protocol..."": Protokoll speichern und wieder laden.
- Filter oben: Open, All, Errors only, Fixed, Ignored, Marked by hand."),
                new HelpTopic(ViewerBuild, "Reiter Build (Gebäude setzen)", @"
# Reiter ""Build""
## Gebäude wählen
- ""Multis / library..."": Fenster mit den Client-Multis (multi.mul) und der eigenen Bibliothek. Doppelklick oder ""Place on map"" übernimmt.
- ""Import file..."": TXT (dezimal oder hex wird erkannt), UOA, WSC, CSV.
- ""Normalize Z"": setzt das unterste Teil auf z 0.
## Platzieren
- ""Start placing"": das Gebäude folgt der Maus (halbtransparent). Klick fixiert, nochmal Klick folgt wieder.
- Pfeiltasten: X/Y, Bild hoch/runter: Z (Shift = 5), Enter: setzen, Esc: abbrechen.
- Anchor: Mitte, obere linke Ecke oder Ursprung des Multis.
- Height: Boden am Anker / niedrigster / höchster / mittlerer Boden + Offset oder feste Höhe.
- ""Remove in the way"": alle Statics, nur Bäume/Pflanzen/Steine oder in der Liste auswählen.
- ""Flatten the ground below the building to the base z"": ebnet den Boden unter dem Gebäude.
- Grün = passt, rot = Fehler (siehe Liste ""Checks"").
## Bereich der Karte
- ""Select area"": Rahmen in der Ansicht aufziehen.
- ""Remove statics"" (alle oder nur Pflanzen), ""Flatten land to z:"".
- ""Export / script..."": Statics des Bereichs als Gebäude exportieren.
Jeder Vorgang ist ein Undo-Schritt."),
                new HelpTopic(ViewerSettings, "Reiter Settings", @"
# Reiter ""Settings""
- Fehlerprüfung: Grenzwerte für steiles Land und schwebende Statics, welche Prüfungen laufen.
- Anzeige: nodraw-Tiles zeigen, Licht auf Hügeln, Statics über einer Höhe ausblenden (z. B. um in Häuser zu sehen).
- Cache: Speicher für gerenderte Bilder (MB) und Auslagern in den Temp-Ordner. Bei großen Bereichen mehr Speicher geben.")),

            new HelpTopic(Save, "Speichern und Backups", @"
# Speichern und Backups
""Save..."" öffnet den Speichern-Dialog.
## Ziel
- Output-Ordner (aus den Optionen), Ordner der geladenen Karte (ersetzt die Originale) oder ein anderer Ordner.
## Was wird geschrieben
- ""Land (map.mul)"": map{Nummer}.mul. ""Statics"": staidx{Nummer}.mul + statics{Nummer}.mul.
- ""File number (mapN)"": eigene Dateinummer, z. B. 20 -> map20.mul / staidx20.mul / statics20.mul.
  So kann eine bearbeitete Karte als neue Karte gespeichert und später von Hand ausgetauscht werden.
- ""Complete set"": schreibt auch unveränderte Teile (immer alle drei Dateien).
- Nur Land oder nur Statics ist möglich. Ersetzt man die geladene Karte, müssen Land und Statics zusammen gespeichert werden.
## Backup
- Ersetzte Dateien werden nach <Ziel>\UoFiddlerBackups\<Datum>_<Karte>_mapN kopiert.
- ""Backups..."" bzw. ""Restore backup..."" im Dialog: Backup wählen, ""Restore"".
  ""Back up the current files before restoring"" sichert vorher die aktuellen Dateien. ""Delete backup"" löscht ein Backup.
> Vor dem Ersetzen der Originale Client und Server schließen.
> Geladene UOP-Karten: es wird map{N}.mul daneben geschrieben, die dann benutzt wird. Für den Client mit einem UOP-Packer packen."),

            new HelpTopic(Multis, "Multis und Bibliothek", @"
# Multis / Building library
- Links ""Client"": Multis aus multi.mul. Rechts ""Library"": eigene Bibliothek (für sehr viele Einträge, mit Suche nach Name oder ID).
- ""Import files..."" / ""Import folder..."": TXT, UOA, WSC, CSV in die Bibliothek.
- ""Copy to library"", ""Name / category..."", ""Delete"", ""Compact"" (entfernt gelöschte Einträge aus der Datei).
- ""Export..."": TXT hex/dezimal, UOA binär/Text, WSC, CSV, UOX3, XML.
- ""Save into multi.mul..."": schreibt ein Gebäude in eine Nummer der multi.mul.
- ""Library file..."": andere Bibliothek öffnen oder neu anlegen. ""Move library..."": Bibliothek verschieben. ""Open folder"".
Standardort: <Output-Ordner>\UoFiddler_BuildingLibrary\BuildingLibrary.uml"),

            new HelpTopic(ExportArea, "Export Area", @"
# Export Area
Übernimmt die Statics eines Kartenbereichs als Gebäude.
- Liste: alle Statics; abgehakte werden nicht übernommen. Filter: alle / ohne Bäume / nur Bäume, nodraw überspringen, z-Bereich.
- ""Origin 0,0"" und ""Z 0 is"": Bezugspunkt des Gebäudes.
- ""Remove exact duplicates"": entfernt doppelte Teile.
- Ziele: ""Save as file..."", ""To library"", ""Into multi.mul..."", ""ServUO addon script..."" (mit Hue),
  ""ServUO decoration (.cfg)..."" (absolute Positionen), ""Open in Multi Script""."),

            new HelpTopic(Editor, "Map Editor", @"
# Map Editor
Kopiert Bereiche (Land und/oder Statics) und fügt sie auf jeder Karte ein; dazu Pinsel, Höhenwerkzeuge und Übergänge.
Aufruf: Kartenreiter -> ""Map Editor..."" (ein markierter Bereich wird gleich kopiert).

## Aufbau
- Links vier Reiter: 1. Source (Quelle), 2. Clip (kopierter Bereich), 3. Target (Ziel), 4. Tools (Werkzeuge).
- Rechts: der Arbeitsbereich der Zielkarte in Client-Ansicht. Der Fenstertitel zeigt Zielkarte, Position und Größe.
- Oben: Undo, Redo, Undo all, Save..., Backups..., Zoom, Raster, Statics, ""Edit in Region Viewer..."", Help.

## Was ist der Arbeitsbereich?
Der Ausschnitt der Zielkarte (Standard 160 x 160 Felder), der rechts angezeigt und bearbeitet wird.
Im Reiter ""3. Target"" auf die Übersichtskarte klicken = anderen Ausschnitt laden.

## Mehrere Karten
Jede Karte hat ihre eigenen Undo-Schritte. Beim Schließen wird für jede geänderte Karte gefragt (speichern / verwerfen).
""Edit in Region Viewer..."" öffnet den Arbeitsbereich im Region Viewer mit denselben Undo-Schritten (Fehlerprüfung, Statics, Bauen).",
                new HelpTopic(EditorSource, "1. Source – Bereich kopieren", @"
# 1. Source – Bereich kopieren
- ""Map"": Quellkarte wählen. ""External..."": Karte aus einem beliebigen Ordner öffnen (nur für diese Sitzung, siehe unten).
- Übersicht: linke Maus ziehen = Bereich markieren, rechte Maus = verschieben, Mausrad = zoomen.
  Beim ersten Öffnen wird die Übersicht im Hintergrund aufgebaut (Anzeige ""Loading map overview…"").
- X, Y, W, H: Bereich als Zahlen (max. 1024 x 1024).
- ""Use mark of the map tab"": übernimmt die Markierung aus dem Kartenreiter.
- Land / Statics: was kopiert wird.
- ""Copy area to the clip"": kopiert. Änderungen, die noch nicht gespeichert sind, werden mitkopiert.
## Externe Karte
Ordner wählen, der mapX.mul (oder mapXLegacyMUL.uop) und staidxX/staticsX enthält. Die Größe wird vorgeschlagen.
Die Karte erscheint dann in beiden Kartenlisten mit ""(external)""."),
                new HelpTopic(EditorClip, "2. Clip – vor dem Einfügen bearbeiten", @"
# 2. Clip – vor dem Einfügen bearbeiten
- Oben: Herkunft, Größe, Höhen, Anzahl Statics und Vorschaubild.
- ""Rotate 90°"", ""Mirror X"", ""Mirror Y"": dreht/spiegelt die Positionen.
  > Richtungsabhängige Grafiken (Wände, Türen, Treppen) behalten ihre Grafik.
- Liste: alle Static-IDs mit Anzahl. Abgehakt = wird nicht eingefügt. ""All"" / ""None"".
- ""Statics z from … to …"": nur Statics in diesem Höhenbereich.
- ""Delete unchecked from the clip"": entfernt die abgewählten endgültig aus dem Clip.
- ""Add static to the clip..."": Eingabe ""ID X Y [Z] [Hue]"", X/Y innerhalb des Clips, ID dezimal oder 0x hex, ohne Z = Bodenhöhe.
- ""Export statics..."": Statics des Clips als TXT, UOA, WSC, CSV, UOX3 oder XML speichern.
- ""Clear"": Clip löschen."),
                new HelpTopic(EditorTarget, "3. Target – einfügen", @"
# 3. Target – einfügen
- ""Map"": Zielkarte (auch extern). ""Working area"": Größe des Arbeitsbereichs. Klick auf die Übersicht lädt ihn.
## Positionieren
- ""Position clip"": der Clip folgt der Maus (halbtransparent, grüner Umriss).
- Klick: Position fixieren. Ist ""Show the real result…"" an, wird das echte Ergebnis gezeigt.
- Pfeiltasten: verschieben (Shift = 5), Bild hoch/runter: z, Enter: einfügen, Esc: abbrechen.
## Höhe
- ""Keep z of the source"": Höhen wie auf der Quellkarte (+ Z offset).
- ""Match the border (auto)"": der Clip wird so verschoben, dass sein Rand im Mittel zur Zielhöhe passt.
- ""Edge blend"": Anzahl Randfelder, die weich von der Zielhöhe zur Clip-Höhe übergehen (0 = harte Kante).
- Rot markierte Randfelder: Höhensprung größer als 10. Dann ""Edge blend"" erhöhen oder Z offset ändern.
- ""Statics follow the ground"": Statics behalten ihren Abstand zum Boden.
## Vorhandene Statics
- ""Remove existing"": alle Statics im Zielbereich, nur Bäume/Pflanzen/Steine oder nichts entfernen.
## Ergebnis
Einfügen ist ein Undo-Schritt. Danach ""Save..."" (siehe ""Speichern und Backups"")."),
                new HelpTopic(EditorTools, "4. Tools – Pinsel und Werkzeuge", @"
# 4. Tools – Pinsel und Werkzeuge
""Tool active (T)"" einschalten. Dann gilt im Arbeitsbereich:
- Linke Maus: Werkzeug benutzen, ziehen = malen. Ein Mausstrich ist ein Undo-Schritt.
- Rechte oder mittlere Maus: Ansicht verschieben. Mausrad: zoomen.
- Alt+Klick: Pipette. [ und ]: Pinselgröße. Esc: Werkzeug aus.
- Pinsel 1 bis 32 Felder, eckig oder rund. Der Umriss (türkis) zeigt, was getroffen wird.
## Werkzeuge
- Paint brush: malt mit dem Pinsel – Texturen oder ein kopiertes Muster (siehe ""Pinsel festlegen"").
- Copy to brush: Klick kopiert den Boden (mit Statics) unter dem Pinsel-Umriss in den Pinsel. Danach wird automatisch ""Paint brush"" gewählt.
- Terrain + transitions: malt Gelände A oder B des gewählten Übergangs und setzt Kanten und Ecken automatisch.
- Replace texture: ersetzt nur die Tiles aus ""Replace"" (Pipette oder ""< palette"") durch die Palettenauswahl.
- Raise ground / Lower ground: hebt bzw. senkt um ""Step"" (pro Strich nur einmal je Feld).
- Flatten: ebnet auf die Höhe des Feldes, an dem der Strich beginnt.
- Set height: setzt auf ""Set height z"".
- Smooth: Durchschnitt der Nachbarn; mehrmals überstreichen = weichere Hügel.
- Erase statics: alle oder nur Bäume/Pflanzen/Steine löschen.
- Eyedropper: übernimmt das Land-Tile in Palette und Replace-Liste und seine Höhe in ""Set height z"".
- Line / curve: Punkte auf die Karte klicken, Enter zeichnet (siehe ""Linien, Kurven, Formen"").
- Shape: Rechteck, Ellipse/Kreis, Dreieck oder Linie aufziehen – gefüllt oder als Umriss.
- Select area: Bereich für den Generator aufziehen.
- ""Statics follow"": Statics wandern bei Höhenänderungen mit.
## Übergänge
- Übergang wählen, ""paint A"" oder ""paint B"", mit ""Terrain + transitions"" malen.
- ""Set transitions in the working area"": setzt die Übergänge im ganzen Arbeitsbereich neu.
- ""Transitions..."": Übergänge und Geländegruppen bearbeiten."),
                new HelpTopic(BrushHelp, "Pinsel festlegen (Texturen / Muster)", @"
# Pinsel festlegen
Der Bereich ""Brush"" im Reiter ""4. Tools"" legt fest, was ""Paint brush"" malt.
## Das Raster
- Das Raster im Bereich ""Brush"" zeigt den Pinsel von oben, so wie er auf der Karte malt (Größe und Form = Pinselgröße / Square / Round).
  Schraffierte Zellen liegen außerhalb eines runden Pinsels. Blau umrandet = die Zelle unter der Maus.
- ""Numbers"": Nummer jeder Zelle – dieselben Nummern erscheinen in der Vorschau auf der Karte.
- Muster: Linke Maus = Palettenauswahl in die Zelle, rechte Maus = leer, Strg+Klick = Zelle in die Palette.
- ""Move cells"" (oder Shift + Ziehen): eine Zelle auf eine andere ziehen = die beiden tauschen den Platz.
- Pfeile ◄ ► ▲ ▼: das ganze Muster um eine Zelle verschieben.
- Pinselgröße ändern: ein quadratisches Muster in Pinselgröße wächst/schrumpft mit (der Inhalt bleibt in der Mitte).
## Texturen
- ""Textures"" wählen. Klick auf ein Tile in der Palette = der Pinsel malt dieses Tile.
- Das Raster zeigt dann ein Beispiel der zufälligen Verteilung. ""Fix positions"" macht daraus ein Muster:
  jede Textur bekommt einen festen Platz, den man verschieben kann.
  Strg+Klick / Shift+Klick = mehrere Tiles, sie werden zufällig (aber fest pro Feld) verteilt.
- Die Texturen des Pinsels stehen als Bilder im Bereich ""Brush"". Klick auf ein Bild entfernt es.
## Muster
- ""Pattern"" wählen. Mit ""Edit..."" legst du im Pinsel-Editor fest, welche Textur an welcher Stelle liegt (siehe ""Pinsel-Editor""). Das Muster ist ein Stück Karte: Land, Höhen und Statics (z. B. ein Küstenabschnitt).
- ""Copy from map"": schaltet auf das Werkzeug ""Copy to brush"". Pinselgröße einstellen, dann auf die Karte klicken –
  der Bereich unter dem Umriss wird in den Pinsel kopiert (bis 64 x 64).
- ""Clip -> brush"": nimmt den Clip aus Reiter ""2. Clip"" (nur die angehakten Statics, max. 64 x 64).
- ""Brush = pattern size"": Pinselgröße = Mustergröße, eckig – ein Klick stempelt das ganze Muster.
- Land / Statics: was gemalt wird. ""Replace"": Statics auf den bemalten Feldern vorher entfernen.
  Jedes Feld bekommt die Statics pro Mausstrich nur einmal, auch wenn man mehrfach darüber fährt.
- Höhe: ""keep the ground"" (nur Texturen/Statics), ""as copied"" (z wie im Original), ""as copied, moved to the ground""
  (das Muster wird auf die Bodenhöhe am Startpunkt des Strichs verschoben).
- Lage: ""starts where the stroke starts"" (Stempel) oder ""repeats on the map grid"" (nahtlos über mehrere Striche).
## Vorschau
- ""Preview"": zeigt halbtransparent unter der Maus, was gemalt würde (Land und Statics).
## Speichern
- ""Save..."": Pinsel mit Namen speichern (gleicher Name = ersetzen). Auswahl oben lädt einen gespeicherten Pinsel.
- ""New"": leerer Pinsel. ""Delete"": gespeicherten Pinsel löschen.
- Datei: <Output-Ordner>\UoFiddler_Brushes\Brushes.xml"),
                new HelpTopic(BrushEditor, "Pinsel-Editor (Muster selbst festlegen)", @"
# Pinsel-Editor
Aufruf: Reiter ""4. Tools"" -> Bereich ""Brush"" -> ""Edit..."".
## Links: Pinsel-Liste
- Nach Kategorien sortiert (z. B. Floors, Coast, Roads), Suche oben nach Name oder Kategorie.
- ""New"", ""Duplicate"" (Kopie zum Weiterbearbeiten), ""Rename..."", ""Category..."", ""Delete"", ""Save"".
- Beim Wechseln wird gefragt, ob Änderungen gespeichert werden sollen.
## Mitte: Pinsel bearbeiten
- Name und Kategorie, ""Brush size"" und Form (Square/Round), ""Textures (random)"" oder ""Pattern (tile per position)"".
- Links das Raster von oben, rechts derselbe Pinsel wie auf der Karte (Rauten, Höhen, Statics), beide mit Nummern.
- ""Numbers"" / ""Tile ids"": Nummer bzw. Tile-ID in jeder Zelle einblenden.
- ""Move"" (oder Shift + Ziehen): Zelle auf eine andere ziehen = Plätze tauschen. ◄ ► ▲ ▼ verschiebt das ganze Muster.
- Bei ""Textures"": ""Fix positions (as shown)"" bzw. ""Place in order"" macht aus den Texturen ein Muster zum Anordnen.
- Muster-Raster: jede Zelle ist eine Position des Pinsels.
  - Linke Maus: die in der Palette gewählte(n) Textur(en) in die Zelle. Mehrere gewählt = die Zelle wechselt zufällig (fest pro Kartenfeld).
  - Rechte Maus: Zelle leeren – dort bleibt der Boden unverändert.
  - Strg+Klick: Zelle in die Palette übernehmen. Ziehen mit gedrückter Maus malt mehrere Zellen.
  - Tile aus der Palette auf eine Zelle ziehen (Drag & Drop).
  - ""Click sets height z"": Klick setzt stattdessen die Höhe der Zelle.
  - Blau umrandet: die Zelle, die unter der Maus liegt, wenn der Strich beginnt.
- Size + ""Resize"", ""Rotate"", ""Mirror X/Y"", ""Fill all"", ""Fill empty"", ""Clear"", ""No statics"".
- ""Import picture..."": ein Bild (max. 64 x 64) wird zum Muster, 1 Pixel = 1 Feld.
  Jede Farbe bekommt ein Tile (Vorschlag nach Radarfarbe, änderbar). Transparente Pixel = leer.
- Land / Statics / Replace statics, Höhe und Lage wie im Bereich ""Brush"".
## Unten
- ""Use brush"": übernimmt den Pinsel in den Map Editor (Pinselgröße = Mustergröße, ein Klick stempelt das Muster).
## Muster von der Karte übernehmen
Im Map Editor ""Copy from map"" (Werkzeug ""Copy to brush""), dann ""Edit..."" -> anpassen -> Name/Kategorie -> ""Save"".
Mit ""Duplicate"" lassen sich Varianten davon anlegen."),
                new HelpTopic(Shapes, "Linien, Kurven, Formen", @"
# Linien, Kurven, Formen
Bereich ""Lines, shapes, generator"" im Reiter ""4. Tools"". ""Draw with"" legt fest, welches Werkzeug benutzt wird:
Paint brush (Texturen/Muster), Terrain + transitions (z. B. Straßen mit Übergängen), Replace, Raise/Lower, Flatten, Set height, Smooth, Erase statics.
## Linie / Kurve
- Werkzeug ""Line / curve"". Punkte auf die Karte klicken (rote Felder). Der Weg zur Maus wird in Pinselbreite angezeigt.
- ""Line as curve"": weiche Kurve durch die Punkte, sonst gerade Stücke.
- Enter oder ""Draw line"": zeichnet mit der aktuellen Pinselgröße von Punkt zu Punkt.
- Danach die Frage ""Continue the line…?"": Ja = vom Endpunkt aus weiter, Nein = fertig.
- Backspace: letzten Punkt entfernen. Esc oder ""Clear points"": alle Punkte löschen.
## Formen
- Werkzeug ""Shape"", Form wählen (Rechteck, Ellipse/Kreis, Dreieck, Linie), mit der Maus aufziehen, loslassen = zeichnen.
- ""Filled"": ganze Fläche, sonst nur der Umriss (Breite = Pinselgröße).
- Ein Muster-Pinsel beginnt in der oberen linken Ecke der Form – ideal für Böden in Häusern (Schachbrett, Fliesen, Holz).
Jede Linie und Form ist ein Undo-Schritt (Strg+Z / Strg+Y)."),
                new HelpTopic(Generator, "Generator (Siedlung, See, Bäume)", @"
# Generator
Erzeugt in einem Bereich Grundstücke mit Böden, eine Straße, einen See, unebenen Boden und Bäume/Pflanzen.
## Bereich
- Werkzeug ""Select area"" und auf der Karte aufziehen. Ohne Auswahl: der ganze Arbeitsbereich.
- Das Generator-Fenster kann offen bleiben, eine neue Auswahl wird übernommen.
## Reiter Settlement
- Anzahl Grundstücke, Größe, Abstand untereinander und zur Straße.
- Boden: aktueller Pinsel, Tiles aus dem Feld oder ein gespeicherter Pinsel (ein Muster beginnt an der Ecke jedes Grundstücks).
- ""Flatten plots"": Grundstück auf die mittlere Höhe ebnen. ""Remove statics"": Grundstücke und Straße freiräumen.
- Straße: Richtung (West-Ost / Nord-Süd), Breite, Pinsel oder Tiles, Wege von den Grundstücken zur Straße.
## Reiter Ground
- Zufällige Höhen (bis +N, optional auch nach unten) mit Wahrscheinlichkeit pro Feld, auf Straße und/oder freiem Boden.
## Reiter Lake
- Mitte, Radien, Unregelmäßigkeit des Ufers, Wasser-Tiles und Wasserhöhe, Uferneigung.
- Optional ein Übergang für das Ufer (Gelände A = Wasser, B = Boden).
## Reiter Vegetation
- Baum-IDs, Bäume pro 1000 Felder, Mindestabstand. Pflanzen-IDs und Dichte.
- ""Statics of the clip"": übernimmt die Static-IDs eines kopierten Waldstücks.
- ""Only on land"": nur auf bestimmten Boden-Tiles (z. B. Gras).
## Erzeugen
- ""Generate"": alles in einem Undo-Schritt. ""Replace the last result"": erneutes Erzeugen ersetzt das letzte Ergebnis.
- ""Seed"": gleicher Wert = gleiches Ergebnis, ""New seed"" = neue Variante."),
                new HelpTopic(Palette, "Textur-Palette", @"
# Textur-Palette
Zeigt alle Land-Tiles. ""Tex"" an = Textur (texmaps), aus = Land-Grafik.
- Suche: 0x16 (hex), 22 (dezimal), 0x16-0x19 (Bereich), oder ein Name (grass, sand…).
- Auswahl: ""All valid"", ""With texture"", ""Selected"" (nur die gewählten).
- Klick = ein Tile, Strg+Klick = hinzufügen/entfernen, Shift+Klick = Bereich. Mehrere = Varianten beim Malen.
- Ziehen: Drag & Drop (z. B. in das Transition-Fenster).
- Unten: ID, Name und Textur-Nummer des Tiles unter der Maus.")),

            new HelpTopic(Transitions, "Übergänge (Transitions)", @"
# Übergänge (Transitions)
Ein Übergang beschreibt, welche Grafiken zwischen Gelände A (innen, z. B. Sand) und Gelände B (außen, z. B. Gras) liegen.
Aufbau wie ""Neue Transition"" in UO-Caeri.

## Die Felder
Oben – Ring um A (die Mitte zeigt A):
- Reihe 1 = Kanten: 1-1 oben, 1-2 rechts, 1-3 unten, 1-4 links.
- Reihe 2 = Außenecken: 2-1 oben links, 2-2 oben rechts, 2-3 unten rechts, 2-4 unten links.
Unten – Ring um B (die Mitte zeigt B):
- Reihe 3 = Innenecken: 3-3 oben links, 3-4 oben rechts, 3-1 unten rechts, 3-2 unten links.
- ""optional"": Kanten um B (werden gespeichert, von der Automatik noch nicht benutzt).
Ein Feld zeigt immer die Grafik, die an genau dieser Position neben der Mitte liegt.
Beispiel 1-1 (oben): das Feld nördlich von A – unten Sand, oben Gras.

## Belegen
- Tile aus der Palette auf ein Feld ziehen, oder Tile in der Palette wählen und Feld anklicken.
- Rechtsklick oder Feld aus dem Fenster ziehen = löschen.
- Klick auf die Mitte A oder B: die Palettenauswahl wird zu den Tiles der Geländegruppe.
- ""2nd row"": zweite Umrundung für weitere Varianten (bis 4 je Kante, 2 je Ecke). Varianten werden nach Position verteilt.
- Rechts: welche Felder noch leer sind. Leere Felder = diese Stellen bleiben beim automatischen Setzen unverändert.
- ""New"", ""Copy"", ""Rename"", ""Delete"": Übergänge verwalten. ""Save"": speichert alles.

## So arbeitet die Automatik
- Die Grafiken kommen auf die B-Felder direkt neben A.
- A auf einer Seite = Kante, A nur diagonal = Außenecke, A auf zwei Seiten (über Eck) = Innenecke.
- B-Felder mit A auf gegenüberliegenden Seiten oder auf drei Seiten werden zu A (keine Grafik passt).
- Höhen werden nicht verändert. Die Varianten sind fest pro Position (gleiches Feld = gleiche Grafik).",
                new HelpTopic(TerrainGroups, "Geländegruppen", @"
# Geländegruppen (Reiter ""Terrain groups"")
Eine Geländegruppe sind die normalen Tiles eines Geländes, z. B. Sand = 0x16-0x19.
- Liste links: alle Gruppen. Name und IDs ändern, ""Apply"".
- ""New group"": neue Gruppe aus der Palettenauswahl. ""Palette selection -> ids"", ""Show in palette"".
- ""Create groups from tile names"": schlägt Gruppen aus den tiledata-Namen vor.
  > Bitte prüfen: Übergangs-Tiles haben oft denselben Namen wie das Gelände und gehören nicht in die Gruppe.
- Wird eine Gruppe umbenannt, werden die Übergänge mit angepasst.")),

            new HelpTopic(Keys, "Tastenkürzel", @"
# Tastenkürzel
## Überall
- F1: Hilfe. Strg+Z: letzten Schritt rückgängig. Strg+Y (oder Strg+Shift+Z): wiederherstellen.
## Region Viewer – Gebäude platzieren / Map Editor – Clip positionieren
- Pfeiltasten: verschieben (Shift = 5 Felder).
- Bild hoch / Bild runter: Höhe (Shift = 5).
- Enter: setzen / einfügen. Esc: abbrechen.
- Klick: Position fixieren / wieder der Maus folgen.
## Map Editor – Werkzeuge
- T: Werkzeug an/aus. [ / ]: Pinsel kleiner/größer. Alt+Klick: Pipette. Esc: Werkzeug aus.
- Line / curve: Enter = zeichnen, Backspace = letzter Punkt weg, Esc = Punkte löschen.
- Rechte oder mittlere Maus: Ansicht verschieben.
## Ansichten
- Mausrad: zoomen. Linke Maus ziehen: verschieben (ohne aktives Werkzeug)."),

            new HelpTopic(Files, "Dateien und Speicherorte", @"
# Dateien und Speicherorte
- Kartenliste: MapDefinitions.xml in AppData\UoFiddler.
- Gespeicherte Pinsel: <Output-Ordner>\UoFiddler_Brushes\Brushes.xml.
- Gebäudebibliothek: <Output-Ordner>\UoFiddler_BuildingLibrary\BuildingLibrary.uml (+ .umx Index, README.txt).
- Übergänge und Geländegruppen: <Output-Ordner>\UoFiddler_Transitions\Transitions.xml (+ README.txt).
  Ohne Output-Ordner: AppData\UoFiddler\UoFiddler_Transitions.
- Backups: <Ziel-Ordner>\UoFiddlerBackups\<Datum>_<Karte>_mapN.
- Fehlerprotokolle: RegionProtocol_*.xml / .csv im gewählten Ordner.
- Kartendateien: map{N}.mul, staidx{N}.mul, statics{N}.mul."),

            new HelpTopic(Problems, "Probleme und Tipps", @"
# Probleme und Tipps
- Das Öffnen des Map Editors dauert: die Übersichtskarten werden beim ersten Mal im Hintergrund aufgebaut
  (""Loading map overview…""). Man kann währenddessen schon arbeiten; danach geht es schnell.
- ""The files … were not found"": Karte im Manage-Maps-Fenster prüfen (Ordner, Dateinummer, Größe).
- Falsche Größe einer externen Karte: das Bild sieht verschoben/gestreift aus -> Breite/Höhe im Dialog korrigieren.
- Höhensprünge nach dem Einfügen: ""Edge blend"" erhöhen oder mit ""Smooth"" nacharbeiten.
- Übergang setzt nichts: Geländegruppen A und B prüfen (enthalten sie die Tiles der Karte?) und leere Felder füllen.
- Speichern in den Ordner der geladenen Karte geht nicht, solange Client oder Server die Dateien geöffnet haben.
- Etwas ist schiefgegangen: ""Backups..."" stellt die vorherigen Dateien wieder her."),

            new HelpTopic(MultiMapImages, "MultiMap-Bilder", @"
# MultiMap-Bilder erstellen, bearbeiten, drucken
Im Reiter MultiMap kann man aus jeder Karte (eingebaut, aus der Kartenliste oder externe Kartendateien)
oder aus einer beliebigen Bilddatei ein Kartenbild erzeugen, es bearbeiten und speichern.

## Load..
- MultiMap / Facet00-05: die Bilder des Clients.
- Darunter: alle eigenen Bilder der Liste (erzeugte, gespeicherte und hinzugefügte Bilder).
- ""Open image file..."": ein Bild nur ansehen (PNG, JPG, BMP, GIF, TIFF, ICO, EMF, WMF).
- ""Add image file to list..."": ein Bild dauerhaft in die Liste aufnehmen (verknüpfen oder in die App kopieren).
- ""From clipboard"": ein Bild aus der Zwischenablage anzeigen.
- ""Manage list..."": Einträge umbenennen, entfernen, Ordner öffnen.

## Create image...
- Quelle: Karte, ""External map files..."" (Ordner mit mapX.mul / staidxX.mul / staticsX.mul), Bilddatei
  oder das gerade angezeigte Bild.
- Bereich: nur einen Ausschnitt der Karte in Tiles (X, Y, Breite, Höhe).
- Stil ""Ink map"": gezeichnete Karte wie eine Schatzkarte - Küstenlinien mit Wellen, Berge, Bäume, Häuser,
  gepunktete Wege, Sand (Punkte), Sumpf (Striche), Hänge (Schraffur).
- Weitere Stile: Radar-Farben, Graustufen, Schwarz/Weiß (gerastert wie MultiMap.rle), Skizze (nur Umrisse).
- Optionen: Statics, Pergament-Look, Rahmen, Gitter mit Koordinaten, Symbolgröße, Dichte, Zufallsmuster (Seed).
- ""2560 x 2048 (MultiMap)"" erzeugt genau die Größe, die ""Generate.. > MultiMap from current image"" braucht.

## Bearbeiten (Edit...)
- Werkzeuge: Hand (H), Auswahl (M), Stift (P), Radierer (E), Linie (L), Pfeil (A), Rechteck (R), Ellipse (O),
  Text (T), Symbol (S), Füllen (F), Farbpipette (I).
- Linke Maustaste = Zeichenfarbe, rechte Maustaste = Papierfarbe. Shift = 45°-Linien, Quadrat, Kreis.
- Mausrad = Zoom an der Mausposition, mittlere Maustaste oder Leertaste + Ziehen = verschieben.
- Text: Schrift, Farbe, heller Rand (gut lesbar auf der Karte) und Drehung; danach mit Klick platzieren.
- Symbole: Berg, Baum, Nadelbaum, Busch, Haus, Fels, Gras, Ankh, Totenkopf, Stern, Schatz (X), Kompass,
  Turm, Höhle, Markierung.
- Strg+Z / Strg+Y rückgängig / wiederholen, Strg+C kopieren (Auswahl oder ganzes Bild), Strg+V einfügen
  und mit Klick platzieren, Strg+Umschalt+V als neues Bild einfügen.
- Bild: drehen, spiegeln, Größe ändern, zuschneiden, Graustufen, Schwarz/Weiß, invertieren, Sepia.

## Speichern
- ""Inside the app"": das Bild kommt in die Liste ""Load.."" (Ordner AppData\UoFiddler\MultiMapImages).
- ""In a folder of my choice"": beliebiger Ordner und Format (PNG, JPEG, BMP, GIF, TIFF, ICO),
  auf Wunsch zusätzlich in der Liste.
- Save (Strg+S) überschreibt danach dieselbe Datei.

## Drucken und Zwischenablage
- ""Print..."": Drucker wählen; auf eine Seite eingepasst oder als Poster über mehrere Seiten (2 x 2, 3 x 3 ...).
- ""Print preview..."": Vorschau mit Druckknopf.
- ""Copy"": Bild in die Zwischenablage.")
        };

        /// <summary>Finds a topic by key (also in the sub topics)</summary>
        public static HelpTopic Find(string key)
        {
            return Find(Topics, key);
        }

        private static HelpTopic Find(IEnumerable<HelpTopic> topics, string key)
        {
            foreach (HelpTopic t in topics)
            {
                if (t.Key == key)
                {
                    return t;
                }

                HelpTopic child = Find(t.Children, key);
                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
