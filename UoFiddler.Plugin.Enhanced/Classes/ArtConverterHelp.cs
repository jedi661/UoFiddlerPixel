/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.Enhanced.Classes
{
    /// <summary>Help of the "Art Converter" tab (English / German).</summary>
    public static class ArtConverterHelp
    {
        public const string English = @"ART CONVERTER – HELP

Brings classic items and gumps (from the classic client loaded in UOFiddler) into the Enhanced Client,
puts own images on any id and lists what is missing in the EC.

ITEMS (ART)
  Check: compares every classic item with the EC. State:
    missing in EC   no tileart entry – the EC doesn't know the item (invisible)
    no image        tileart entry, but no texture (invisible)
    legacy only     only the classic size texture, the client enlarges it itself
    EC image        own EC image (world art)
  Filter, search by id (decimal or 0x hex) or name, mark rows (Shift / Ctrl, ""Mark all shown"").
  Convert marked: writes the marked items. Import image: puts an image file on an id – an existing one
  (replaces the image) or a free one (new item; the name comes from the file name).

  What is written (exactly like the files of the client):
    LegacyTexture.uop  build/tileartlegacy/<id>.dds  classic image, at least 44 pixels wide
                       (narrower images centered like the classic client draws them), DXT5
    Texture.uop        build/worldart/<id>.dds       EC image, 1.5 times larger (only with ""+ EC image"")
    tileart.uop        entry with the texture names Data\WorldArt\<id>.tga, Data\TileArtLegacy\<id>.tga,
                       Data\TileArtEnhanced\<id>.tga, image areas and position (bottom center on the tile)
    string_dictionary.uop  the new names / texture names
  New entries get name, flags, weight, height, value, layer, animation and radar color from the classic
  tiledata / radarcol.mul. For existing entries only the images change (unless ""Tiledata from classic"").

  Options
    Legacy texture + EC image: sharper result, you choose how it is enlarged:
      Smooth  Scale3x then halved – keeps the pixel art look with clean diagonal edges (recommended)
      Sharp   nearest pixel – exactly the classic pixels, some 2 pixels wide
      Soft    bilinear – soft, slightly blurred
    Legacy texture only: like most original items without EC art; the client enlarges it.
    The preview ""after converting"" shows the result before anything is written.
    Imported images already EC size: the image is used as EC image, the legacy texture is made from it.
    Uncompressed DDS: lossless colors, 4 times larger files.

GUMPS
  Same list for the classic gumps; written into gumpartLegacyMUL.uop (build/gumpartlegacymul/<id>.tga,
  classic RLE format). Import image: own gump image on any id.

REPORT: MISSING IN EC
  Checks all items and gumps and lists the missing ids as ranges (hex and decimal); can be saved as text.
  The log of every conversion is added below.

SAVE
  ""Save"" writes all changed files (old ones kept as .bak). Texture.uop and LegacyTexture.uop of the client
  are signed (version 5); changed entries are written without signature – test with a COPY of the client.
  The Enhanced Client must be closed. After saving, ""Reload"" on top shows the new items in the other tabs.
";

        public const string German = @"ART CONVERTER – HILFE

Bringt Classic-Items und -Gumps (aus dem in UOFiddler geladenen Classic Client) in den Enhanced Client,
setzt eigene Bilder auf beliebige IDs und zeigt, was im EC fehlt.

ITEMS (ART)
  Check: vergleicht jedes Classic-Item mit dem EC. Zustand:
    missing in EC   kein Tileart-Eintrag – der EC kennt das Item nicht (unsichtbar)
    no image        Tileart-Eintrag, aber keine Textur (unsichtbar)
    legacy only     nur die Textur in Classic-Größe, der Client vergrößert sie selbst
    EC image        eigenes EC-Bild (World Art)
  Filter, Suche nach ID (dezimal oder 0x hex) oder Name, Zeilen markieren (Shift / Strg, ""Mark all shown"").
  Convert marked: schreibt die markierten Items. Import image: setzt eine Bilddatei auf eine ID – eine
  vorhandene (Bild wird ersetzt) oder eine freie (neues Item; der Name kommt vom Dateinamen).

  Was geschrieben wird (genau wie die Dateien des Clients):
    LegacyTexture.uop  build/tileartlegacy/<id>.dds  Classic-Bild, mindestens 44 Pixel breit
                       (schmalere Bilder mittig, wie der Classic Client sie zeichnet), DXT5
    Texture.uop        build/worldart/<id>.dds       EC-Bild, 1,5-mal größer (nur bei ""+ EC image"")
    tileart.uop        Eintrag mit den Texturnamen Data\WorldArt\<id>.tga, Data\TileArtLegacy\<id>.tga,
                       Data\TileArtEnhanced\<id>.tga, Bildbereichen und Position (unten mittig auf der Kachel)
    string_dictionary.uop  die neuen Namen / Texturnamen
  Neue Einträge bekommen Name, Flags, Gewicht, Höhe, Wert, Layer, Animation und Radarfarbe aus der
  Classic-Tiledata / radarcol.mul. Bei vorhandenen Einträgen ändern sich nur die Bilder
  (außer ""Tiledata from classic"" ist an).

  Optionen
    Legacy texture + EC image: schärferes Ergebnis, du wählst, wie vergrößert wird:
      Smooth  Scale3x und dann halbiert – Pixel-Art-Look mit sauberen schrägen Kanten (empfohlen)
      Sharp   nächstes Pixel – genau die Classic-Pixel, manche 2 Pixel breit
      Soft    bilinear – weich, leicht unscharf
    Legacy texture only: wie die meisten Original-Items ohne EC-Grafik; der Client vergrößert selbst.
    Die Vorschau ""after converting"" zeigt das Ergebnis, bevor etwas geschrieben wird.
    Imported images already EC size: das Bild wird als EC-Bild benutzt, die Legacy-Textur daraus erzeugt.
    Uncompressed DDS: verlustfreie Farben, 4-mal größere Dateien.

GUMPS
  Dieselbe Liste für die Classic-Gumps; geschrieben in gumpartLegacyMUL.uop
  (build/gumpartlegacymul/<id>.tga, Classic-RLE-Format). Import image: eigenes Gump-Bild auf jede ID.

REPORT: MISSING IN EC
  Prüft alle Items und Gumps und listet die fehlenden IDs als Bereiche (hex und dezimal); als Text
  speicherbar. Das Log jeder Umwandlung wird unten angehängt.

SPEICHERN
  ""Save"" schreibt alle geänderten Dateien (alte bleiben als .bak). Texture.uop und LegacyTexture.uop des
  Clients sind signiert (Version 5); geänderte Einträge werden ohne Signatur geschrieben – mit einer
  KOPIE des Clients testen. Der Enhanced Client muss geschlossen sein. Nach dem Speichern zeigt
  ""Reload"" oben die neuen Items auch in den anderen Reitern.
";
    }
}
