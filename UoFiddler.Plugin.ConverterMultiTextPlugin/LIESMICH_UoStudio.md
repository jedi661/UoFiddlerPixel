# UO Studio – fünf neue Werkzeuge in der Tool Box

Alle Werkzeuge starten über Buttons im Tab **Tool Box** (Plugin ConverterMultiText).
Jedes Fenster öffnet sich nur einmal; ein zweiter Klick holt es nach vorne.
Gespeichert wird immer in den **Output-Pfad** von UO Fiddler (Optionen), ersatzweise auf den Desktop.

| Tab        | Button                | Form (Ordner `Forms/`)    |
|------------|-----------------------|---------------------------|
| Graphic    | Pixel-Art Konverter   | `PixelArtConverterForm`   |
| Graphic    | Ausrüstungs-Generator | `EquipmentGeneratorForm`  |
| Animation  | Animations-Studio     | `AnimationStudioForm` (+ Dialog `AnimationSheetImportForm`) |
| Animation  | Spell / Effekt Editor | `SpellEffectEditorForm` (+ Dialoge `EffectSheetSplitForm`, `EffectParticleForm`) |
| Sound (neu)| Sound-Studio          | `SoundStudioForm`         |

### Ordnerstruktur

- `Forms/StudioTextForm` – Textfenster für erzeugte Def-Einträge / Scripte (kopieren, speichern, an Datei anhängen)
- `Forms/` – jede Form als `Name.cs` (Logik), `Name.Designer.cs` (Oberfläche, im Visual-Studio-Designer bearbeitbar) und `Name.resx`.
- `Class/UoStudio/` – Hilfsklassen und eigene Controls (Namespace `...Class.UoStudio`):
  - `StudioImaging.cs` – Bildfunktionen (Palette, Isometrie, Outline, Hue, UO-Farben, ARGB1555)
  - `StudioAudio.cs` – WAV/MP3 lesen, Resampling, Normalisieren, UO-Soundformat
  - `StudioUi.cs` – Meldungen, Output-Pfad, Hue-Zugriff, ID-Parser
  - `StudioIcons.cs` – Button-Icons (zur Laufzeit gezeichnet)
  - `NaturalStringComparer.cs` – sortiert frame2 vor frame10
  - `UltimaImages.cs` – sichere Kopien von Art/Land/Gump (repariert beschädigte Cache-Einträge)
  - `ParticleGenerator.cs` – Funken, Feuer, Rauch, Blitz, Magie-Ring, Explosion, Heilung
  - Controls für die Toolbox im Designer: `PixelView` (Pixel-Zoom mit Schachbrett), `WaveformView` (Wellenform mit Auswahl), `LevelMeter` (Pegelanzeige)

## Pixel-Art-Konverter
Bild/Foto laden (Datei, Drag & Drop oder Zwischenablage) → Vorlage wählen
(Item, großes Item, Bodentile 44×44, Wand links/rechts, Gump).
Schritte: Hintergrund entfernen (Klick ins Original wählt die Farbe) → Helligkeit/Kontrast/Sättigung →
Größe → Isometrie (UO-45°-Diamant bzw. Wand-Scherung 1:1) → Schärfen → Palette (Median-Cut, optional Dithering) →
Outline (außen/innen) → UO-15-Bit-Farben (reines Schwarz wird zu 0x0421, da 0 in UO transparent ist).
Export: als Art-Item, Bodentile oder Gump in eine ID setzen, PNG speichern, Art-/Gump-MUL schreiben.

## Animations-Studio
UO speichert 5 Richtungen: **0 ↓, 1 ↙, 2 ←, 3 ↖, 4 ↑**. Der Client spiegelt 1–3 für ↘, →, ↗.
Die Vorschau zeigt alle 8 Richtungen als Kompass (gespiegelte Richtungen automatisch).
Import: Einzelbilder/GIF je Richtung oder ein Sprite-Sheet mit wählbarer Reihenfolge
(UO-Reihenfolge, nur rechte Seite → wird gespiegelt, 8 Spielrichtungen, …).
Export: Body/Action/Datei wählen → „In Animation schreiben“ (gemeinsame 256-Farben-Palette je Richtung,
Fußpunkt unten-Mitte + Versatz) → „Anim-MUL speichern“. Zusätzlich PNG-Frames oder Sprite-Sheet.

## Ausrüstungs-Generator
Item-Bild (Datei oder Art-ID) auf der Paperdoll-Figur (Gump 0x0C/0x0D) mit der Maus platzieren,
skalieren, drehen, spiegeln, Hue-Vorschau. Gump-ID = **50000 + Animation** (männlich) bzw. **60000 + Animation** (weiblich).
Tiledata: Name, Layer (Quality), Animation, Gewicht, Hue, Höhe, Wearable/PartialHue (bei Waffen-Layern auch Weapon).
„Script-Snippet kopieren“ erzeugt eine ServUO/RunUO-Klasse.

## Sound-Studio
WAV (PCM 8/16/24/32 Bit, Float) und MP3 (NLayer) laden oder vorhandenen UO-Sound öffnen.
Auswahl mit der Maus ziehen (Rechtsklick hebt sie auf). Zuschneiden, Löschen, Stille entfernen,
Ein-/Ausblenden, Normalisieren, Verstärkung in dB, DC entfernen, Rückwärts, Rückgängig (Strg+Z).
Pegelanzeige (Peak/RMS in dBFS, rot = Übersteuerung), Vorschau exakt im UO-Format (22050 Hz, 16 Bit, Mono)
mit Vorschau-Lautstärke und Schleife (Leertaste = Play/Stop).
Export: als UO-Sound in eine ID setzen → „sound.mul speichern“, oder als WAV.

## Spell- & Effekt-Editor
Bildfolgen (natürlich sortiert, frame2 vor frame10), GIF oder Sprite-Sheet importieren, oder vorhandenen
animierten Effekt aus Art/Animdata laden. Schwarz → transparent (für leuchtende Effekte), Graustufen
(voll per Hue färbbar), Größe, gemeinsames Zuschneiden, Ping-Pong.
Vorschau additiv auf dunklem Grund oder auf einem Bodentile, mit Hue (auch nur Grautöne).
Export: Frames als aufeinanderfolgende Art-Items ab Start-ID, Tiledata (Animation-Flag auf Frame 0),
Animdata-Eintrag (max. 64 Frames, Intervall ≈ ms/100). Script-Snippet für `Effects.Send…Effect`.

## Erweiterungen (Version 5)

**Pixel-Art-Konverter:** Vorher/Nachher-Schieberegler · Palette aus einem Art-Item oder Palettenbild
(neue Items passen farblich zum Client) · Stapel-Umwandlung vieler Bilder an fortlaufende IDs oder als PNG ·
Schatten (gerastert oder voll, Versatz X/Y).

**Animations-Studio:** Vorschau am Fußpunkt ausgerichtet (wie im Client) · Zwiebelschicht ·
Fußpunkt pro Frame (Zahlen oder Klick in die große Ansicht, „Auf alle Frames“) · beim Laden aus der MUL
werden die Fußpunkte übernommen · Hue-Vorschau · Overlay-Body (z. B. Ausrüstungs-Animation über der Figur) ·
Export des Bodys als VD-Datei · Def-Einträge (mobtypes.txt, bodyconv.def, Body.def).
UOP wird nicht direkt geschrieben – die VD-Datei lässt sich mit den vorhandenen VD-Tools weiterverarbeiten.

**Ausrüstungs-Generator:** Item-Bild zusätzlich als Art-Item an die Item-ID · vorhandene Paperdoll-Gumps
auf der Figur einblenden · weibliche Variante getrennt platzieren · Animations-Studio für die
Mobile-Animation des Kleidungsstücks öffnen (Body = Animation, Typ EQUIPMENT).

**Sound-Studio:** Zoom (Buttons, Mausrad, Shift+Rad = Scrollen), Klick setzt den Abspiel-Cursor ·
Hall, Tempo (ohne Tonhöhe), Tonhöhe (ohne Länge), Geschwindigkeit, Rausch-Gate · Stapel: viele Dateien auf
gleiche Lautheit (RMS) bringen und als UO-Sounds ab einer ID oder als WAV speichern · Sound.def/Script-Einträge.

**Spell-/Effekt-Editor:** Partikel-Generator · Glow (Weichzeichnen + additiv) · Versatz und Dauer pro Frame ·
Ping-Pong und Dauer laufen über die Animdata-Sequenz (keine doppelten Art-Items, max. 64 Einträge) ·
Vorschau freistehend, auf Bodentile, auf einer Figur oder als Projektil von A nach B.
