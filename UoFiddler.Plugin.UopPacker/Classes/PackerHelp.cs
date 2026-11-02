/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/


namespace UoFiddler.Plugin.UopPacker.Classes
{
    /// <summary>Help texts of the UOP packer tab (English and German).</summary>
    public static class PackerHelp
    {
        public const string English = @"UOP PACKER – HELP

The classic client (since 7.0.24) reads some of its files from UOP packages instead of MUL / IDX files.
This tab converts them in both directions, so the MUL files can be edited with UoFiddler and packed again.

FILES
  Art      artLegacyMUL.uop       <->  art.mul + artidx.mul
  Gumps    gumpartLegacyMUL.uop   <->  gumpart.mul + gumpidx.mul
  Sounds   soundLegacyMUL.uop     <->  sound.mul + soundidx.mul
  Maps     map0..5LegacyMUL.uop, map0x..5xLegacyMUL.uop  <->  map0..5.mul / map0x..5x.mul (no idx)
  Multis   MultiCollection.uop    <->  multi.mul + multi.idx + housing.bin   (tab ""Multis"")

TAB ""ONE FILE""
  MUL -> UOP: choose the MUL; with ""Autofill"" the IDX, the type, the map number and the output name are filled in
  from the file name (e.g. map1x.mul -> map 1, map1xLegacyMUL.uop). Compression:
    None   – like most client files (art, sounds, maps)
    Zlib   – smaller, the client can read it
    Mythic – only for gumps (newer gumpartLegacyMUL.uop)
  ""Overwrite .uop"": allows replacing an existing UOP (the old one goes into the backup folder first, if the backup is on).
  UOP -> MUL: choose the UOP; type, map number and output names (art.mul / artidx.mul, ...) are filled in.
  ""Overwrite .mul"" allows replacing existing MUL / IDX files.

TAB ""EVERY FILE""
  Choose the client folder, tick the files and press Start.
  ""Check folder"" shows for every file whether the source exists and whether the output would be replaced.
  Extract writes the MUL files next to the UOPs, Pack writes the UOPs next to the MULs.
  Missing files are skipped; everything is written into the log (no message boxes). ""Cancel"" stops after the current file.
  ""Single Uop Extract"" unpacks one UOP chosen in a dialog (type found by the name).

TAB ""MULTIS""
  Pack multi.mul + multi.idx into MultiCollection.uop. The client also needs ""housing.bin"" inside it:
  unpack an original MultiCollection.uop first (that writes housing.bin) and use that file.
  multi.mul in the newer format (16 bytes per part, Stygian Abyss and later) and the old format (12 bytes) are read.
  Unpack MultiCollection.uop into multi.mul / multi.idx / housing.bin.

TAB ""UOP INFO""
  Shows header, tables, compression, sizes and the list of all entries with their names.
  ""Check checksums"" reads all data and compares the Adler-32 checksums. Problems are listed at the end.

TAB ""BACKUP""
  When the backup is on, every file that is replaced is copied first to
    <backup folder>\old\<file name>
  and (optionally) the new file to
    <backup folder>\new\<file name>.
  There is one backup per file name – the next backup of the same file replaces it.
  Backup folder: ""UoFiddlerBackups\UopPacker"" next to the written files, or one folder of your choice.
  The list shows the saved files; ""Restore old"" copies the old version back to where it was.

SAFETY
  All outputs are written to a temporary file first and replace the target only when the conversion worked,
  so a failed conversion never leaves a half written file. Close the client before replacing its files.
";

        public const string German = @"UOP PACKER – HILFE

Der klassische Client (ab 7.0.24) liest einige Dateien aus UOP-Paketen statt aus MUL / IDX.
Dieser Reiter wandelt sie in beide Richtungen um – so können die MUL-Dateien mit UoFiddler bearbeitet und wieder gepackt werden.

DATEIEN
  Art      artLegacyMUL.uop       <->  art.mul + artidx.mul
  Gumps    gumpartLegacyMUL.uop   <->  gumpart.mul + gumpidx.mul
  Sounds   soundLegacyMUL.uop     <->  sound.mul + soundidx.mul
  Maps     map0..5LegacyMUL.uop, map0x..5xLegacyMUL.uop  <->  map0..5.mul / map0x..5x.mul (ohne idx)
  Multis   MultiCollection.uop    <->  multi.mul + multi.idx + housing.bin   (Reiter „Multis“)

REITER „ONE FILE“
  MUL -> UOP: MUL wählen; mit „Autofill“ werden IDX, Typ, Map-Nummer und Ausgabename aus dem Dateinamen gesetzt
  (z. B. map1x.mul -> Map 1, map1xLegacyMUL.uop). Kompression:
    None   – wie die meisten Client-Dateien (Art, Sounds, Maps)
    Zlib   – kleiner, der Client kann es lesen
    Mythic – nur für Gumps (neuere gumpartLegacyMUL.uop)
  „Overwrite .uop“: erlaubt das Ersetzen einer vorhandenen UOP (die alte kommt vorher in den Sicherungsordner, wenn die Sicherung an ist).
  UOP -> MUL: UOP wählen; Typ, Map-Nummer und Ausgabenamen (art.mul / artidx.mul, ...) werden gesetzt.
  „Overwrite .mul“ erlaubt das Ersetzen vorhandener MUL / IDX.

REITER „EVERY FILE“
  Client-Ordner wählen, Dateien abhaken und Start drücken.
  „Check folder“ zeigt für jede Datei, ob die Quelle da ist und ob die Ausgabe ersetzt würde.
  Extract schreibt die MUL-Dateien neben die UOPs, Pack schreibt die UOPs neben die MULs.
  Fehlende Dateien werden übersprungen; alles steht im Protokoll (keine Meldungsfenster). „Cancel“ hält nach der aktuellen Datei an.
  „Single Uop Extract“ entpackt eine UOP aus einem Dialog (Typ wird am Namen erkannt).

REITER „MULTIS“
  multi.mul + multi.idx in MultiCollection.uop packen. Der Client braucht darin auch „housing.bin“:
  zuerst eine originale MultiCollection.uop entpacken (das schreibt housing.bin) und diese Datei verwenden.
  multi.mul im neuen Format (16 Bytes pro Teil, ab Stygian Abyss) und im alten Format (12 Bytes) wird gelesen.
  MultiCollection.uop in multi.mul / multi.idx / housing.bin entpacken.

REITER „UOP INFO“
  Zeigt Kopf, Tabellen, Kompression, Größen und die Liste aller Einträge mit Namen.
  „Check checksums“ liest alle Daten und prüft die Adler-32-Prüfsummen. Probleme stehen am Ende.

REITER „BACKUP“
  Ist die Sicherung an, wird jede Datei, die ersetzt wird, vorher kopiert nach
    <Sicherungsordner>\old\<Dateiname>
  und (wahlweise) die neue Datei nach
    <Sicherungsordner>\new\<Dateiname>.
  Pro Dateiname gibt es eine Sicherung – die nächste Sicherung derselben Datei ersetzt sie.
  Sicherungsordner: „UoFiddlerBackups\UopPacker“ neben den geschriebenen Dateien oder ein eigener Ordner.
  Die Liste zeigt die gesicherten Dateien; „Restore old“ kopiert die alte Fassung zurück an ihren Platz.

SICHERHEIT
  Alle Ausgaben werden zuerst in eine temporäre Datei geschrieben und ersetzen das Ziel erst, wenn die Umwandlung
  geklappt hat – eine fehlgeschlagene Umwandlung hinterlässt nie eine halbe Datei. Vor dem Ersetzen den Client schließen.
";
    }
}
