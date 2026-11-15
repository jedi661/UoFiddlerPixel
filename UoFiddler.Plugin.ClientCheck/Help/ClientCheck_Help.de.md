# Client-Prüfung – Hilfe

Die Client-Prüfung vergleicht **alle Dateien des klassischen Clients miteinander** und zeigt, was nicht zusammenpasst. Das sind Fehler, die man sonst erst im Spiel bemerkt: unsichtbare Items, schwarze Flecken auf der Karte, fehlende Namen, Monster mit falschen Bewegungen.

Geprüft werden die Dateien, die in UoFiddler gerade geladen sind (Client-Ordner aus den Optionen, inklusive deiner Änderungen im Speicher).

## So benutzt du die Prüfung

1. Unter **Prüfungen** auswählen, was geprüft werden soll (Standard: alles). Die Auswahl wird gespeichert.
2. **▶ Prüfung starten** klicken. Der Fortschritt steht unten in der Statusleiste. UoFiddler bleibt bedienbar, mit **■ Abbrechen** stoppst du die Prüfung.
3. Die Ergebnisse erscheinen in der Liste. Oben steht eine Zusammenfassung (Fehler, Warnungen, Hinweise, Anzahl je Bereich).
4. Einen Eintrag anklicken: rechts stehen **Beschreibung**, **So beheben** und eine **Vorschau** (Item, Land-Tile, Gump oder Animation).
5. Behebbare Einträge mit **Beheben** korrigieren (mehrere markieren geht auch) oder mit **Alle behebbaren beheben…** alle angezeigten auf einmal.
6. Mit **Bericht exportieren** speicherst du die angezeigten Einträge als HTML-Seite, CSV-Tabelle oder Text.

Die Sprache stellst du oben rechts um (Deutsch, English, Français). Sie gilt sofort für alle Texte, auch für die Ergebnisse und diese Hilfe.

## Stufen

| Stufe | Bedeutung |
|---|---|
| **Fehler** | Etwas fehlt sicher im Spiel (unsichtbar, kaputt). Sollte behoben werden. |
| **Warnung** | Funktioniert, sieht aber falsch aus oder verhält sich falsch (z. B. schwarz auf der Karte). |
| **Hinweis** | Unvollständig, aber meist harmlos (z. B. Grafik ohne Namen). |

## Filter und Bedienung

- **Anzeigen:** nach Stufe und nach Bereich filtern.
- **Suche:** sucht in ID und Beschreibung (z. B. `0x1F03`, `Body 400`, `Felucca`).
- **Behobene ausblenden:** blendet Einträge aus, die du schon behoben hast (sie sind mit ✓ markiert).
- Klick auf einen **Spaltenkopf** sortiert, ein zweiter Klick dreht die Reihenfolge um.
- **Strg+C** oder **Zeile kopieren** kopiert die markierten Einträge, **ID kopieren** nur die ID.

## Die Prüfungen im Einzelnen

### Art ↔ tiledata
- **Grafik ohne Namen** (Hinweis): Das Item hat eine Grafik, aber keinen Namen in der tiledata. Lösung: Tab „TileData“, Namen und Flags setzen, tiledata.mul speichern.
- **tiledata ohne Grafik** (Hinweis): Ein tiledata-Eintrag hat einen Namen, aber keine Grafik. Lösung: Grafik im Tab „Items“ einfügen oder den Eintrag leeren.

### Land-Tiles
- **Land-Tile ohne Namen** (Hinweis).
- **Fehlende Textur** (Warnung): Die tiledata verweist auf eine Textur, die nicht existiert. Schräges Gelände sieht dann falsch aus. Lösung: Textur im Tab „Texturen“ einfügen oder TextureId auf 0 setzen.

### Ausrüstung
Geprüft werden alle Items mit dem Flag **Wearable**.
- **Kein Layer** (Warnung): `Quality` ist 0, der Server weiß nicht, wo das Item getragen wird.
- **Animation fehlt** (Fehler): Das tiledata-Feld `Animation` zeigt auf eine Animation, die es nicht gibt. Dann ist das Item am Körper unsichtbar.
- **Paperdoll-Gump fehlt** (Warnung): Gump **50000 + Animation** (männlich) fehlt und es gibt keinen Ersatz in gump.def. Dann fehlt das Item auf dem Paperdoll.
- **Weiblicher Gump fehlt** (Hinweis): Gump **60000 + Animation** fehlt. Der Client nimmt dann den männlichen, das ist oft gewollt.
- Rucksack (Layer 21) und Reittiere (Layer 25) haben keinen eigenen Paperdoll-Gump und werden dabei übersprungen.

### Bodies / Animationen
Geprüft werden die Bodies 1 bis 4095.
- **Kein Eintrag in mobtypes.txt** (Warnung): Es gibt eine Animation, aber der Client kennt den Typ nicht (MONSTER, SEA_MONSTER, ANIMAL, HUMAN, EQUIPMENT) und spielt falsche Aktionen ab. **Behebbar:** Die vorgeschlagene Zeile wird in eine Kopie von mobtypes.txt im **Output-Ordner** geschrieben (Backup `.bak`). Danach die Datei in den Client kopieren.
- **mobtypes.txt ohne Animation** (Hinweis): Zeile ohne passende Animation, meist eine harmlose Altlast.
- **Bodyconv.def zeigt ins Leere** (Fehler): Ein Body wird auf anim2–anim5 umgeleitet, dort sind aber keine Daten. Dann ist das Monster unsichtbar.
- **body.def zeigt ins Leere** (Warnung): Ein Body wird auf einen Body ohne Animation umgeleitet.

### radarcol (Kartenfarben)
radarcol.mul enthält für jedes Land-Tile und jedes Item **eine Farbe** für Radar und Karte. Fehlt sie, ist das Item auf der Karte **schwarz**.
- **Keine Radarfarbe** (Warnung): **Behebbar.** „Beheben“ setzt die Durchschnittsfarbe der Grafik (im Speicher). Danach **radarcol.mul speichern** klicken. Die Datei landet im Output-Ordner und muss in den Client kopiert werden. Der Knopf ist orange, solange etwas ungespeichert ist.
- **radarcol.mul zu kurz** (Warnung): Die Datei hat keinen Platz für erweiterte Item-IDs. Sie muss vergrößert werden.
- Kacheln mit „NoDraw“ im Namen werden übersprungen, die sind absichtlich unsichtbar.

### Multis
- **Bauteil ohne Grafik** (Fehler): Ein Haus oder Schiff enthält Item-IDs, die keine Grafik haben. Diese Teile fehlen im Spiel. Lösung: Grafik ergänzen oder das Bauteil im Multi-Editor ersetzen.

### Statics auf Karten
Alle Karten werden gelesen (staidx/statics). Karten, die dieselben Dateien benutzen (z. B. Felucca und Trammel), werden nur einmal gelesen.
- **Item existiert nicht** (Fehler): Auf der Karte stehen Statics mit einer Item-ID ohne Grafik. Pro ID ein Eintrag mit Anzahl und Beispiel-Koordinate (`Karte X,Y,Z`).
- **Ungültige Hue** (Warnung): Statics mit einer Hue-Nummer größer als die Anzahl der Hues in hues.mul.
- **Lesefehler** (Fehler): staidx und statics passen nicht zusammen oder sind beschädigt.

### Cliloc (Namen)
Der Client zeigt Item-Namen über Cliloc: **1020000 + ID**, ab ID 0x4000 **1078872 + ID** (wie ServUO `Item.LabelNumber`).
- **Kein Cliloc-Name** (Hinweis): Für das Item gibt es keinen Eintrag in Cliloc.enu. Lösung: Eintrag im Tab „Cliloc“ anlegen.

## Automatisch beheben – was genau passiert

| Was | Wo wird geschrieben |
|---|---|
| Radarfarbe | im Speicher, dann **radarcol.mul speichern** → Output-Ordner |
| mobtypes.txt-Zeile | Kopie `mobtypes.txt` im Output-Ordner (beim ersten Mal aus dem Client kopiert), Backup `.bak` |

Es wird **nie direkt in den Client-Ordner** geschrieben. Kopiere die geänderten Dateien selbst in den Client, wenn alles passt.

## Tipps

- Nach dem Beheben die Prüfung einfach noch einmal starten: behobene Probleme verschwinden.
- Für eine schnelle Kontrolle nach dem Hinzufügen neuer Items reichen die Prüfungen **Art ↔ tiledata**, **Ausrüstung**, **radarcol** und **Cliloc**.
- Die Karten-Prüfung dauert bei großen Karten ein paar Sekunden pro Karte.
- Der HTML-Bericht ist gut zum Weitergeben an das Team: er ist nach Bereichen gegliedert und enthält zu jedem Eintrag den Lösungsweg.

## Dateien dieser Erweiterung

- `ClientCheckPlugin.dll` im Ordner `plugins` von UoFiddler
- Hilfe: `plugins\ClientCheckHelp\ClientCheck_Help.de.md`, `…en.md`, `…fr.md`
- Einstellungen (Sprache, gewählte Prüfungen): `ClientCheck.lang` und `ClientCheck.checks` im AppData-Ordner von UoFiddler
