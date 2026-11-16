# Light – Hilfe

Der Light-Tab zeigt und bearbeitet die Lichter aus `light.mul` / `lightidx.mul`. Ein Licht ist ein Graustufenbild: jedes Pixel hat einen Wert von 0 (kein Licht) bis 31 (volle Helligkeit). Der Client legt es über die Spielwelt, wenn ein Item mit dem Flag *LightSource* leuchtet.

Alle Änderungen sind zuerst im Speicher. In die Dateien kommen sie mit **Save light.mul** (Ausgabe-Ordner).

## Liste und Suche

- Die Liste zeigt `ID (Hex) – Name`.
- **Namen** vergeben: F2 oder Rechtsklick → *Name…*. Die Namen stehen in `%AppData%\UoFiddler\LightNames.json`, nicht in der MUL.
- **Suche** oben: ID (`12`), Hex (`0x0C`), Bereich (`10-20`) oder Teil des Namens.
- **▦ Overview** zeigt alle Lichter als Bilder. Doppelklick wählt eines aus.
- **Entf** entfernt das Licht, **F2** benennt es um.

## Vorschau

- Normal siehst du das Licht allein (schwarz = kein Licht, weiß = volle Helligkeit), mit Zoom.
- **In-game preview** zeigt das Licht über Boden-Tiles mit einem Item in der Mitte, dazu:
  - **Ground**: Land-Tile als Boden. **◀ ▶** blättert, **▦ Overview…** wählt aus der Gesamtübersicht.
  - **Light source**: das Item in der Mitte. ◀ ▶ blättert, auf Wunsch nur durch Items mit dem Flag *LightSource*.
  - **Darkness %**: wie dunkel die Szene ohne Licht ist.
  - **Light offset X/Y**: wo das Licht relativ zum Item liegt.
  - **Light colour…**: farbiges Licht testen. ClassicUO kann Lichter über `lights.txt` mit einer Hue färben.
  - **Flicker**: die Helligkeit schwankt wie bei einer Flamme.
- Rechtsklick auf die Vorschau: Vorschau kopieren oder als PNG speichern.

## Mehr als 100 Lichter

Die Original-Dateien haben 100 Einträge. Die Anzahl wird aus der `lightidx.mul` gelesen.

- **Entries: [ ] Set** erweitert die Dateien, z. B. auf 255, oder verkürzt sie.
- Wird ein Licht hinter dem Ende eingefügt, wächst die Datei automatisch.
- ClassicUO liest alle Einträge der `lightidx.mul`. Ob andere Clients mehr als 100 nutzen, bitte selbst testen.
- Welches Licht ein Item benutzt, steht in der Tiledata.

## Rechtsklick in der Liste

| Eintrag | Funktion |
|---|---|
| Export image | als PNG, BMP, TIFF, JPG; oder alle Lichter als PNG in einen Ordner |
| Name… / Copy ID | Namen vergeben, ID kopieren |
| Remove | Licht entfernen |
| Replace from file… | Bild laden (Helligkeit des Bildes = Lichtwert, schwarz = kein Licht) |
| Insert at ID | ID eintippen, Enter, Bild wählen |
| Copy to ID / Move to ID | Licht (mit Namen) auf eine andere ID kopieren oder verschieben |
| Copy image / Import from clipboard | über die Zwischenablage |
| Edit in light designer… | öffnet den Licht-Designer mit diesem Licht |

## Weitere Knöpfe

- **✔ Check…** findet doppelte Lichter (gleiches Bild), leere Lichter, sehr dunkle (Höchstwert unter 4), sehr große (über 512 Pixel) und Lichter mit „negativen“ Werten über 31. Außerdem zeigt es, wie viele Einträge frei sind.
- **Import folder…** liest alle Bilder eines Ordners ein. Die ID steht im Dateinamen: `Light 12 Kerze.png` → Licht 12 mit dem Namen „Kerze“. `12.png` und `0x0C.bmp` gehen auch. Das ist das Gegenstück zu „Export → alle Lichter“.

## Licht-Designer

Öffnen mit **✚ New light…** (neues Licht, erste freie ID) oder **✎ Edit…** (gewähltes Licht).

### Neues Licht entwerfen

- **Vorlagen** (Doppelklick): Kerze, Lampe, großes Licht, Fackel, Fenster (Licht fällt nach links/rechts), Spot, Strahl, Ring.
- **Form**: rund, oval, Kegel (Spot), Fenster (wird nach außen breiter), Strahl (gleich breit), Ring.
- **Bildgröße** und **Position der Lichtquelle** im Bild.
- **Radius**, **Richtung °** (0 = rechts, 90 = unten, 180 = links, 270 = oben), **Öffnungswinkel**, **Länge**, **Öffnungsbreite**.
- **Helligkeit** (1–31), **Abfall** (100 % = gleichmäßig, mehr = schneller dunkel), **weicher Rand**, **Körnung** (wie die Originale) und **Körnungsmuster**.
- **Spiegeln ↔ ↕** und **Drehen** 0/90/180/270°.

### Mit der Maus

- **Links ziehen** in der Vorschau verschiebt die Lichtquelle, bei einem bearbeiteten Bild das ganze Bild.
- **Rechts ziehen** verschiebt das Hilfsbild.

### Hilfsbild

Ein Item (z. B. ein Fenster), ein Land-Tile oder eine Bilddatei wird über das Licht gelegt, damit du das Licht genau daran ausrichten kannst. Position, Größe und Durchsichtigkeit sind einstellbar. Das Hilfsbild wird **nicht** gespeichert.

### Testen

- Ansicht **Light only** oder **In game** (Boden, Item, Dunkelheit, Licht-Versatz)
- **Colour…** für farbiges Licht, **Flicker** zum Flackern
- **◑ Before** gedrückt halten zeigt den Stand beim Öffnen

### Fertiges Bild bearbeiten

Frei drehen (°; 90er-Schritte drehen ohne Verlust), Größe %, Helligkeit %, verschieben X/Y. Danach sind die Formfelder aus. **Back to the design** schaltet zurück.

### Rückgängig

**↶ / ↷** oder Strg+Z / Strg+Y.

### Speichern

- **Into light.mul**: an die eingetragene ID. Rechts daneben steht, ob die ID frei ist oder ersetzt wird. Danach im Light-Tab **Save light.mul**.
- **Export PNG…** speichert das Bild.
- **Save as .uolight…** speichert Entwurf und Bild in einer Datei. Die lässt sich mit **Import…** wieder laden und weiter ändern.
- **Import…** lädt `.uolight`, PNG, BMP oder TIFF.
- **Zwischenspeicher-Liste**:
  - **Keep** legt das aktuelle Licht ab. Die Liste bleibt, solange UoFiddler läuft.
  - **Load** bzw. Doppelklick lädt es wieder.
  - **All into folder…** speichert alle als `.uolight`.
  - **Load folder…** lädt einen Ordner (Standard `%AppData%\UoFiddler\LightLibrary`).
