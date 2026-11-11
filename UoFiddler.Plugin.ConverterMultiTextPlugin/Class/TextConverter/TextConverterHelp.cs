using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextConverter
{
    /// <summary>Help window of the text converter tab (English / German).</summary>
    internal static class TextConverterHelp
    {
        public const string English =
@"TEXT CONVERTER – HELP

Input left, output right. Choose what to do at the top; with ""Live"" the output follows every change, otherwise press
""Convert"" (F5). ""⇦ Output → input"" uses the result for a second step. The status line shows what was recognized;
click it to see lines that could not be read.

MULTI: CONVERT FORMATS
The main use: old multis (e.g. Sphere / UOA exports with decimal IDs) → the TXT format that UoFiddler's multi import reads.
1. ""Open…"" or ""Paste"" the file. Header lines (… version, … num components), comments and empty lines are skipped.
2. In: ""Auto"" recognizes the format. Plain lines are ""ID X Y Z [Flags]""; IDs without 0x are decimal unless they contain A-F
   (choose ""IDs: hex"" / ""IDs: decimal"" to force it).
3. Out: ""UoFiddler TXT"" gives ""0x06A5 0 -3 7 0"" – exactly what the import needs (single spaces, no empty lines; a single empty
   line makes UoFiddler's import fail).
4. ""Save output…"" (saved without BOM), then in UoFiddler: Multis → Import → TXT.
   Or skip the text step: choose the multi ID and press ""→ multi.mul"", then ""Save multi.mul"".
Input formats: lines (UOA, Sphere, UoFiddler TXT), CSV (Punt), WSC, UOX3, UoFiddler XML, ServUO / RunUO addon scripts
(AddComponent …), Sphere MULTIDEF (COMPONENT=0abc,x,y,z – Sphere hex), world coordinates (""X: 1405, Y: 1709, ID: 1234, Z: 0"").
Output formats: UoFiddler TXT, UOA with header, plain decimal lines, CSV, WSC, UOX3, XML, ServUO addon script with deed
(class name and deed name from the fields), Sphere MULTIDEF, ServUO decoration .cfg (Static 0x… + positions at base X / Y / Z),
C# array.
Tools:
• Origin: keep / center / min corner / manual X Y Z – world coordinates become offsets around 0 / 0.
  (The old ""Test"" button always used 1405 / 1709 – now you choose.)
• Shift X / Y / Z, flags keep / all 0 / all 1, remove duplicates, sort in draw order, replace IDs (""0x6A5=0x6A6; 99=100"").
• Preview: the multi as picture from the client files.
• Batch files…: many files at once with the same settings into an output folder (e.g. all old Sphere multis).

OTHER CONVERSIONS
• Numbers in columns: decimal → hex (the old ""Convert"", for any columns: ""1"", ""1,3"" or ""all""; hex digits) and hex → decimal.
  Tabs, several spaces, commas and empty lines no longer break it.
• Sphere hex (0abc) ↔ C# hex (0xABC) in any text (Sphere reads numbers with a leading 0 as hex).
• Coordinates → ServUO new Point3D(…) (also with Map), Sphere P=x,y,z,map, [go x y z or ""x y z"" – any separators.
• Text ↔ binary, Morse (unknown characters become ?), Base64, hex bytes; random numbers (count, min, max, no repeats).

KEYBOARD
F5 convert · F1 help";

        public const string German =
@"TEXT CONVERTER – HILFE

Links die Eingabe, rechts die Ausgabe. Oben wählen, was passieren soll; mit „Live“ folgt die Ausgabe jeder Änderung, sonst
„Convert“ (F5) drücken. „⇦ Output → input“ nimmt das Ergebnis für einen zweiten Schritt. Die Statuszeile zeigt, was erkannt wurde;
ein Klick darauf listet Zeilen, die nicht gelesen werden konnten.

MULTI: FORMATE UMWANDELN
Der Hauptzweck: alte Multis (z. B. Sphere- / UOA-Exporte mit dezimalen IDs) → das TXT-Format, das der Multi-Import von UoFiddler liest.
1. Datei mit „Open…“ laden oder einfügen („Paste“). Kopfzeilen (… version, … num components), Kommentare und Leerzeilen werden übersprungen.
2. In: „Auto“ erkennt das Format. Einfache Zeilen sind „ID X Y Z [Flags]“; IDs ohne 0x sind dezimal, außer sie enthalten A-F
   („IDs: hex“ / „IDs: decimal“ erzwingt es).
3. Out: „UoFiddler TXT“ ergibt „0x06A5 0 -3 7 0“ – genau das, was der Import braucht (einfache Leerzeichen, keine Leerzeilen;
   schon eine Leerzeile lässt den Import von UoFiddler scheitern).
4. „Save output…“ (ohne BOM gespeichert), dann in UoFiddler: Multis → Import → TXT.
   Oder ohne Textdatei: Multi-ID wählen, „→ multi.mul“ drücken, dann „Save multi.mul“.
Eingabeformate: Zeilen (UOA, Sphere, UoFiddler TXT), CSV (Punt), WSC, UOX3, UoFiddler XML, ServUO- / RunUO-Addon-Skripte
(AddComponent …), Sphere MULTIDEF (COMPONENT=0abc,x,y,z – Sphere-Hex), Weltkoordinaten („X: 1405, Y: 1709, ID: 1234, Z: 0“).
Ausgabeformate: UoFiddler TXT, UOA mit Kopf, einfache dezimale Zeilen, CSV, WSC, UOX3, XML, ServUO-Addon-Skript mit Deed
(Klassenname und Deed-Name aus den Feldern), Sphere MULTIDEF, ServUO-Decoration-.cfg (Static 0x… + Positionen ab Basis X / Y / Z),
C#-Array.
Werkzeuge:
• Origin: behalten / Mitte / kleinste Ecke / X Y Z von Hand – Weltkoordinaten werden zu Offsets um 0 / 0.
  (Der alte „Test“-Knopf nahm immer 1405 / 1709 – jetzt wählst du.)
• Verschieben X / Y / Z, Flags behalten / alle 0 / alle 1, Doppelte entfernen, in Zeichenreihenfolge sortieren, IDs ersetzen („0x6A5=0x6A6; 99=100“).
• Preview: das Multi als Bild aus den Client-Dateien.
• Batch files…: viele Dateien auf einmal mit denselben Einstellungen in einen Ausgabeordner (z. B. alle alten Sphere-Multis).

WEITERE UMWANDLUNGEN
• Zahlen in Spalten: dezimal → hex (das alte „Convert“, für beliebige Spalten: „1“, „1,3“ oder „all“; Hex-Stellen) und hex → dezimal.
  Tabs, mehrere Leerzeichen, Kommas und Leerzeilen bringen es nicht mehr zum Absturz.
• Sphere-Hex (0abc) ↔ C#-Hex (0xABC) in beliebigem Text (Sphere liest Zahlen mit führender 0 als Hex).
• Koordinaten → ServUO new Point3D(…) (auch mit Map), Sphere P=x,y,z,map, [go x y z oder „x y z“ – beliebige Trennzeichen.
• Text ↔ Binär, Morse (unbekannte Zeichen werden ?), Base64, Hex-Bytes; Zufallszahlen (Anzahl, min, max, ohne Wiederholung).

TASTEN
F5 umwandeln · F1 Hilfe";

        public static void Show(IWin32Window owner, bool german)
        {
            var form = new Form
            {
                Text = "Help / Hilfe – Text Converter",
                Size = new Size(940, 760),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                ShowIcon = false
            };
            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(MakePage("English", English));
            tabs.TabPages.Add(MakePage("Deutsch", German));
            tabs.SelectedIndex = german ? 1 : 0;
            form.Controls.Add(tabs);
            form.Show(owner);
        }

        private static TabPage MakePage(string title, string text)
        {
            var page = new TabPage(title);
            page.Controls.Add(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.75f),
                BackColor = SystemColors.Window,
                Text = text.Replace("\r\n", "\n").Replace("\n", "\r\n")
            });
            return page;
        }
    }
}
