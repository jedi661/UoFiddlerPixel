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
    /// <summary>Help of the "Check" and "Patches" tabs (English / German).</summary>
    public static class ToolsHelp
    {
        public const string CheckEnglish = @"CHECK – HELP

Checks the whole Enhanced Client in one go and writes a report. Choose the areas on top:
  TileArt          entries whose id differs from the file name, names / texture names behind the end of
                   string_dictionary.uop, entries without any texture (invisible)
  Textures         every texture tileart points to must exist in Texture.uop (EC image) or LegacyTexture.uop
  Multis           multi parts whose item has no tileart entry (invisible parts)
  Item animations  AnimationDefinition frames whose item has no tileart entry
  Hues             like ""Analyze"" in the Hues tab (bitmaps, hues.dds, overview, names)
  Maps (slow)      every facet: size entry, sector headers, border layers, statics (like the Map Converter)
The summary on top shows OK / problems per area, below are the details with the ids as ranges.
""Save report..."" writes it as text. Nothing is changed – repairs are done in the tabs of the areas.
";

        public const string CheckGerman = @"CHECK – HILFE

Prüft den ganzen Enhanced Client in einem Durchgang und schreibt einen Bericht. Oben die Bereiche wählen:
  TileArt          Einträge, deren ID vom Dateinamen abweicht, Namen / Texturnamen hinter dem Ende der
                   string_dictionary.uop, Einträge ohne jede Textur (unsichtbar)
  Textures         jede Textur, auf die Tileart zeigt, muss in Texture.uop (EC-Bild) oder LegacyTexture.uop sein
  Multis           Multi-Teile, deren Item keinen Tileart-Eintrag hat (unsichtbare Teile)
  Item animations  AnimationDefinition-Bilder, deren Item keinen Tileart-Eintrag hat
  Hues             wie ""Analyze"" im Reiter Hues (Bitmaps, hues.dds, Übersicht, Namen)
  Maps (slow)      jede Facet: Größeneintrag, Sektorköpfe, Randschichten, Statics (wie der Map Converter)
Oben steht die Zusammenfassung OK / Probleme je Bereich, darunter die Einzelheiten mit den IDs als Bereiche.
""Save report..."" speichert ihn als Text. Es wird nichts geändert – repariert wird in den Reitern der Bereiche.
";

        public const string PatchEnglish = @"PATCHES – HELP

A patch is one file (.ecpatch) with entries of UOP files that are added, replaced or removed in a client.
Make your changes once and give them to others (or to your other client copies) without sending whole UOP files.

MAKE A PATCH
  1. Change things in the other tabs (art converter, hues, map, multis, ...) and DON'T save yet.
  2. ""Add changes of the client"": every new, changed and removed entry of the open files is added.
  3. ""Add file..."": a file from disk as entry (UOP file + packed name, e.g. Texture.uop + build/worldart/00012345.dds).
  4. Title, author, description; remove entries you don't want (Del key).
  5. ""Save as..."" writes the .ecpatch (a zip: patch.txt, manifest.txt, data/<file>/<hash>.bin).
OPEN / APPLY
  ""Open..."" shows a patch. ""Apply to a client..."" writes it into the UOP files of a folder: existing entries are
  replaced, new ones added, removed ones deleted; missing UOP files are created.
  Backup: before a file is changed it is copied to UoFiddlerBackups\Patches\old, afterwards to ...\new.
  Applying to the loaded client: its files are read again – press ""Reload"" on top afterwards.
The Enhanced Client must be closed while applying. Signed files (version 5) get unsigned entries.
";

        public const string PatchGerman = @"PATCHES – HILFE

Ein Patch ist eine Datei (.ecpatch) mit Einträgen von UOP-Dateien, die in einem Client hinzugefügt, ersetzt oder
entfernt werden. Änderungen einmal machen und an andere (oder an weitere eigene Client-Kopien) weitergeben,
ohne ganze UOP-Dateien zu verschicken.

PATCH ERSTELLEN
  1. In den anderen Reitern ändern (Art Converter, Hues, Map, Multis, ...) und NOCH NICHT speichern.
  2. ""Add changes of the client"": jeder neue, geänderte und entfernte Eintrag der offenen Dateien kommt dazu.
  3. ""Add file..."": eine Datei von der Platte als Eintrag (UOP-Datei + Name, z. B. Texture.uop +
     build/worldart/00012345.dds).
  4. Titel, Autor, Beschreibung; unerwünschte Einträge entfernen (Entf-Taste).
  5. ""Save as..."" schreibt die .ecpatch (ein Zip: patch.txt, manifest.txt, data/<datei>/<hash>.bin).
ÖFFNEN / ANWENDEN
  ""Open..."" zeigt einen Patch. ""Apply to a client..."" schreibt ihn in die UOP-Dateien eines Ordners: vorhandene
  Einträge werden ersetzt, neue hinzugefügt, entfernte gelöscht; fehlende UOP-Dateien werden angelegt.
  Sicherung: vor der Änderung wird jede Datei nach UoFiddlerBackups\Patches\old kopiert, danach nach ...\new.
  Anwenden auf den geladenen Client: seine Dateien werden neu gelesen – danach oben ""Reload"" drücken.
Der Enhanced Client muss beim Anwenden geschlossen sein. Signierte Dateien (Version 5) bekommen unsignierte Einträge.
";
    }
}
