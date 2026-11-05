// /***************************************************************************
//  *
//  * Dragon Map Studio – help text (tab "Hilfe", F1)
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public static class StudioHelp
    {
        public const string Text =
@"DRAGON MAP STUDIO
=================
Nachbau und Erweiterung von Dragon 1.03.62 (IMod13): Karten als 8-Bit-Bild malen,
daraus map#.mul / staidx#.mul / statics#.mul (und map#LegacyMUL.uop) erzeugen.

Grundidee
---------
Jedes Pixel der Karte ist ein Tile. Der Pixelwert (Palettenindex 00..ff) ist die
""Farbadresse"". maptrans.txt sagt für jede Adresse: Höhe (Z) + Land-Tiles.
groups.txt ordnet jede Adresse einer Gruppe zu (Gras, Wald, Wasser …).
Zwischen Gruppen setzt der Build automatisch Übergänge (map/, statics/ …)
und verteilt Items (Bäume, Steine …) aus items/.

1. Farbtabelle
--------------
* Linksklick auf eine Farbe  = Adresse in die Zwischenablage (Format oben wählbar).
* Rechtsklick                = Farbe 1 (Malfarbe links) im Editor.
* Mittlere Maustaste         = Farbe 2 (Malfarbe rechts).
* Rechts: alle 256 Farben, Infos (Gruppe, Höhe, Tiles, Tile-Bild wenn Client geladen)
  und alle Werte zum Bearbeiten.
* ""Neue Farbe definieren"" nimmt den ersten freien Index – damit kannst du eigene
  Farben/Terrains für Bilder und Texturen anlegen (Tiles, Gruppe, Höhe, Textur-Bild).
* ""Aus Regeln erzeugen"" zeichnet eine aktuelle Farbtabelle aus deinen Regeln
  (Menü Regeln → Farbtabelle als Bild speichern).

2. Karten-Editor
----------------
Werkzeuge: Pinsel (B), Radierer = Farbe 2 (E), Sprühen, Linie (L), Rechteck,
Rechteck gefüllt (R), Ellipse, Füllen (F), Pipette (I), Höhe + / Höhe −
(nächste Farbe derselben Gruppe), Auswahl (S), Hand.
Linke Maus = Farbe 1, rechte Maus = Farbe 2.
Mausrad = Zoom, mittlere Maus / Leertaste = verschieben, Pfeiltasten = verschieben.
[ und ] = Pinselgröße.  Strg+Z / Strg+Y = Rückgängig / Wiederholen.
Auswahl: Strg+C, Strg+X, Strg+V (Stempel – Linksklick setzt, Esc beendet),
Entf = mit Farbe 2 füllen, Strg+F = mit Farbe 1 füllen, Strg+A = alles.
Links: Karten im Projekt-Ordner (öffnen, neu, duplizieren, umbenennen, löschen →
Papierkorb), Farbauswahl und Übersicht (klicken = dorthin springen).
Gespeichert wird immer ein 8-Bit-BMP mit Palette – genau das Dragon-Format.

3. Generator & Umriss
---------------------
Generator: Zufallswelt (Seed, Land %, Kontinent-Größe, Gebirge, Schnee, Klima,
Strand). Die Höhe wird über die Farbe der Gruppe kodiert (wie in der IMod-Tabelle).
Biome: Tabelle Klima → Gruppe (Reihenfolge = Priorität).
Umriss-Bild: Bild eines Landes laden (PNG mit Transparenz oder dunkle Silhouette
auf hellem Grund).
* ""In Karte zeichnen"": Land/Wasser oder nur die Küstenlinie in die Karte malen.
* ""Als Maske für Generator"": der Generator erzeugt Land nur innerhalb des Umrisses
  (mit natürlicher Küste, Gebirgen und Biomen).
* ""Maske aus aktueller Karte"": deine gemalte Land/Wasser-Verteilung als Maske.

4. Regeln & Skripte
-------------------
Alle Dragon-Daten in Tabellen: Gruppen, Farben (maptrans), Übergänge mit Muster-
Vorschau (3×3, Mitte = Zelle, A = eigene Gruppe, B = Nachbar), Items mit
Objekt-Gruppen, Unebenheit, Fix-Tabelle und Farb-Aliase (fremde RGB-Farben →
Adresse, z. B. für Bilder aus anderen Programmen).
Speichern: als eigenes JSON (alles in einer Datei, erweiterbar) oder zurück als
Dragon-Skript-Ordner. Auskommentierte Zeilen bleiben als ""deaktiviert"" erhalten.
Muster-Reihenfolge: NW N NO O SO S SW W  (Uhrzeigersinn, Nordwesten zuerst).

5. MUL / UOP erzeugen
---------------------
Ausgabe-Ordner, Karten-Nummer, MUL und/oder UOP, Statics, WSC (UOX3).
Schritte: Farben → Tiles, Unebenheit, Map-Übergänge, Static-Übergänge, Items,
Fix-Tabelle, optional alte Statics übernehmen (wie Dragonsp).
build_report.txt listet Rand-Tiles ohne Übergang:
  ""Muster fehlt"" → Muster in der Übergangsdatei ergänzen,
  ""Regel fehlt""  → für diese zwei Gruppen eine Übergangsdatei anlegen.
MUL / UOP → BMP: bestehende Karte einlesen (wie Dragon_m2b) und weiter bearbeiten.

6. Einstellungen
----------------
Projekt-Ordner (Karten), Ausgabe-Ordner (MUL/UOP), Skript-Ordner, Farbtabelle,
Palette, Regelsatz-JSON, Kopier-Format, Undo-Speicher.
Die Einstellungen liegen in %AppData%\UoFiddler\DragonMapStudio\settings.json
(bzw. im UoFiddler-Datenordner).

Dateien
-------
DragonData\Scripts  = Original IMod13-Skripte (groups, maptrans, map, statics, items …)
DragonData\Addon    = Farbtabelle, Paletten (ACT/ACO/PAL), Photoshop-Pinsel/Aktionen
";
    }
}
