# Regionen-Editor

Mit dem Regionen-Editor zeichnest du Regionen (Städte, Dungeons, Wachzonen, Bauverbote …) direkt auf den Karten deines Clients und schreibst sie für **ServUO/RunUO**, **Sphere** oder **POL**. Die Karten kommen aus derselben Kartenliste wie im Karten-Tab (auch eigene Karten aus „Karten verwalten“).

## Aufbau

- **Oben:** Datei-Menü, Rückgängig/Wiederholen, Kartenauswahl, Prüfen, Einstellungen, Hilfe, Sprache, Design (hell/dunkel).
- **Zweite Leiste:** Werkzeuge, Raster, Zoom, Ansicht, Client.
- **Links:** Baum mit allen Regionen der gewählten Karte (Unterregionen eingerückt), Suche (Strg+F).
- **Mitte:** Karte mit den Regionen. Unten rechts die Minikarte – ein Klick springt dorthin.
- **Rechts:** Tabs **Eigenschaften**, **Flächen**, **Prüfung**, **Verlauf**.
- **Unten:** Position, Land-Tile und Höhe unter der Maus, Regionen unter der Maus, Meldungen.

## Werkzeuge

| Werkzeug | Taste | Funktion |
|---|---|---|
| Auswahl | V | Region anklicken; Rechteck ziehen = verschieben; Griffe = Größe ändern; Umschalt = ganze Region verschieben; Alt = nächste Region darunter |
| Verschieben | H | Karte ziehen (mittlere Maustaste geht immer) |
| Rechteck | R | Fläche aufziehen = neue Region |
| Polygon | P | Ecken klicken, Doppelklick/Enter = fertig, Rücktaste = Punkt zurück, Esc = abbrechen |
| Kreis | C | vom Mittelpunkt aus ziehen |
| Füllen | F | zusammenhängendes Land gleicher Art im sichtbaren Bereich (Insel, Wald, Wüste, See …) |
| Go | G | Go-Punkt der gewählten Region setzen |
| Eingang | E | Eingang der gewählten Region setzen (DungeonRegion) |

Beim Zeichnen gilt immer:

- **ohne Taste** = neue Region (hat die gewählte Region noch keine Fläche, bekommt sie die Fläche)
- **Umschalt** = Fläche zur gewählten Region hinzufügen
- **Alt** = Fläche aus der gewählten Region ausschneiden

**Raster** (1, 2, 4, 8, 16) richtet Rechtecke und Polygonpunkte aus – 8 entspricht den Kartenblöcken.

Mausrad = Zoom, Pfeiltasten = Karte bewegen, Entf = gewählte Fläche bzw. Region löschen.

## Regionen bearbeiten

- **＋ Region / ＋ Unterregion** (Einfg / Umschalt+Einfg). Danach die Fläche zeichnen.
- **Umbenennen:** F2 im Baum oder Feld „Name“.
- **Verschachteln:** Region im Baum auf eine andere ziehen = wird Unterregion. Mit Umschalt beim Loslassen = davor einsortieren. Auf freie Fläche ziehen = oberste Ebene.
- **Kontextmenü im Baum:** duplizieren, auf der Karte zeigen, Client zum Go-Punkt schicken, als ServUO-XML kopieren, Rechtecke optimieren, Polygone umwandeln, ausblenden, sperren.
- **Kontextmenü auf der Karte:** Region hier auswählen, Go/Eingang hierher, Fläche löschen, Client hierher schicken, Position kopieren.
- **Gesperrte** Regionen lassen sich auf der Karte nicht verschieben. **Ausgeblendete** werden nicht gezeichnet (und auf Wunsch nicht exportiert).

### Eigenschaften

| Feld | Bedeutung |
|---|---|
| Name | Name der Region (ServUO: `name`) |
| Typ | C#-Klasse des Servers: TownRegion, DungeonRegion, GuardedRegion, NoHousingRegion, Jail … – eigene Typen sind erlaubt |
| Priorität | bei Überlappung gewinnt die höhere (Standard 50); Unterregionen erben sie |
| Musik | ServUO `MusicName` (POL: midi-Nummer, wird umgerechnet) |
| Z-Bereich | `<zrange>` der ganzen Region; je Rechteck auch einzeln im Tab Flächen |
| Go | Zielpunkt für [go und Runen; ⌖ = auf der Karte wählen |
| Eingang | `<entrance>` einer DungeonRegion, optional mit anderer Karte |
| Wachen | `<guards disabled>` und eigener Wachen-Typ |
| Rune | `<rune name>` |
| Optionen | `smartNoHousing`, `logoutDelay` (grau = nicht schreiben), ausgeblendet, gesperrt |
| Farbe | eigene Farbe im Editor, sonst nach Typ |
| Notizen | nur im Projekt |
| Server-Werte | weitere Werte für Sphere (`EVENTS`, `FLAGS`, `GROUP`, `TAG.…`) und POL (`POL.EnterText`, `POL.NoCombat` …) |

### Flächen

Liste aller Rechtecke (1, 2, …) und Polygone (P1, P2 …). Werte unten eintragen und **Hinzufügen** bzw. **Übernehmen**. **Optimieren** fasst Rechtecke zu möglichst wenigen zusammen. Server kennen nur Rechtecke – Polygone und Kreise werden beim Export automatisch in Rechtecke zerlegt.

## Rückgängig / Verlauf

Jede Änderung (auch Ziehen auf der Karte, Import, automatische Korrekturen) lässt sich mit **Strg+Z** rückgängig machen und mit **Strg+Y** wiederholen. Im Tab **Verlauf** stehen alle Schritte; „Bis hierher zurück“ springt mehrere Schritte zurück.

## Prüfung

Der Tab **Prüfung** findet u. a.: Regionen ohne Namen oder Fläche, doppelte Namen, Rechtecke außerhalb der Karte oder ohne Größe, falsche Z-Werte, Unterregionen außerhalb ihrer Region, Überlappungen mit gleicher Priorität, Go-Punkt außerhalb der Region oder im Wasser, Eingang im Wasser, unbekannte Musik, eigene Regionstypen. Probleme mit 🔧 lassen sich automatisch beheben. Doppelklick zeigt die Region auf der Karte. Vor dem Schreiben in einen Server wird automatisch geprüft.

## Dateien und Formate

| Format | Lesen | Schreiben |
|---|---|---|
| Projekt `.uoregions` (JSON) | ✔ | ✔ – alles inkl. Polygone, Farben, Notizen |
| ServUO / RunUO `Data\Regions.xml` | ✔ | ✔ – unbekannte Elemente (z. B. `<spawning>`) und Kommentare bleiben erhalten |
| Sphere `mapN_areas.scp` (`[AREADEF]`, `[ROOMDEF]`) | ✔ | ✔ |
| POL `regions.cfg`, `justice.cfg`, `music.cfg` | ✔ | ✔ |
| CSV | ✔ | ✔ |
| Text, HTML-Übersicht, PNG-Kartenbild | – | ✔ |
| C#-Klassen eigener Regionstypen (ServUO) | – | ✔ |

## Server-Ordner festlegen

Unter **⚙ Einstellungen**:

- **ServUO-/RunUO-Ordner** – der Hauptordner des Servers (oder direkt `Data` bzw. die `Regions.xml`).
- **Sphere-Areas-Datei** – z. B. `scripts\maps\map0\map0_areas.scp`. Heißt die Datei `mapN…`, werden nur die Regionen dieser Karte geschrieben.
- **POL-Ordner regions** – Ordner mit `regions.cfg` (und ggf. `justice.cfg`, `music.cfg`).
- **Export-Ordner** – Ziel für „Alles in den Export-Ordner“. Ohne Angabe: `<AppData>\RegionExport`.

Dann:

- **Datei → Vom Server laden** liest die Regionen direkt aus dem eingestellten Server.
- **Datei → In den Server speichern** schreibt sie zurück. Vorher wird immer eine Sicherung `Datei.JJJJMMTT-HHMMSS.bak` angelegt. Danach den Server neu starten.
- **Vorhandene Regions.xml:** *ganz ersetzen*, *nur die Facets des Projekts ersetzen* (andere Facets bleiben) oder *Regionen nach Namen ersetzen/ergänzen* (alles andere bleibt unverändert).
- **Datei → Exportieren → Alles in den Export-Ordner** erzeugt:
  - `servuo\Data\Regions.xml` (+ `servuo\Scripts\Regions\*.cs` für eigene Typen)
  - `sphere\mapN\mapN_areas.scp` je Karte
  - `pol\regions\regions.cfg`, `justice.cfg`, `music.cfg`
  - `regions.csv`, `regions.txt`, `regions.html`, `regions.uoregions`

  Von dort kopierst du die Dateien in deinen ServUO-, Sphere- oder POL-Server.

## Hinweise zu den Formaten

- **ServUO:** `<rect>` wird als `x/y/width/height` oder `x1/y1/x2/y2` geschrieben (Einstellung). x2/y2 ist das Ende (nicht mehr enthalten). Unterregionen bekommen nur dann eine eigene Priorität/Musik, wenn sie von der übergeordneten abweicht.
- **Sphere:** `RECT=x1,y1,x2,y2,map` – x2/y2 wird wie bei ServUO als Ende geschrieben. Unterregionen werden als eigene `AREADEF` mit `GROUP=` der übergeordneten Region geschrieben. Die FLAGS werden aus dem Typ ergänzt (Town/Guarded → `REGION_FLAG_GUARDED`, Dungeon → `REGION_FLAG_UNDERGROUND`, alle drei → `REGION_FLAG_NOBUILDING`).
- **POL:** `Range x1 y1 x2 y2` mit eingeschlossenen Ecken. Regionen aus mehreren Rechtecken bekommen mehrere `Range`-Zeilen in einem Block. Die Realms heißen britannia, britannia_alt, ilshenar, malas, tokuno, termur.
- Facets, die nicht in der Kartenliste sind, bleiben im Projekt und werden mit exportiert.

## Client

- **⌖ Client** springt zur Position des laufenden Clients, **⟳ Folgen** folgt ihr.
- „Client hierher schicken“ benutzt denselben Befehl wie der Karten-Tab (Optionen: Map-Befehl und Argumente, z. B. `.goforce`).
