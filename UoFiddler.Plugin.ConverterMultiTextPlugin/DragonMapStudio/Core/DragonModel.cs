// /***************************************************************************
//  *
//  * Dragon Map Studio – data model
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>
    /// A terrain group (groups.txt). Every palette colour belongs to one group,
    /// transitions and items are defined between / for groups.
    /// </summary>
    public sealed class DragonGroup
    {
        [Category("Gruppe"), Description("Gruppen-Nummer (hex in den Dragon-Skripten, 00..FF).")]
        [DisplayName("Nr")]
        public int Id { get; set; }

        [Category("Gruppe"), Description("Name der Gruppe (aus dem Kommentar in groups.txt).")]
        [DisplayName("Name")]
        public string Name { get; set; } = string.Empty;

        [Category("Gruppe"), Description("Freier Kommentar / Notiz.")]
        [DisplayName("Kommentar")]
        public string Comment { get; set; } = string.Empty;

        [JsonIgnore, Browsable(false)]
        public string HexId => Id.ToString("x2");

        public override string ToString() => $"{HexId} {Name}";
    }

    /// <summary>
    /// One of the 256 colours of the Dragon BMP (maptrans.txt + groups.txt + palette).
    /// The palette index is the "colour address" that Dragon reads from the 8-bit BMP.
    /// </summary>
    public sealed class DragonPaletteEntry
    {
        private string _tiles = string.Empty;
        private ushort[] _tileCache;

        [Category("Adresse"), ReadOnly(true), Description("Palettenindex 0..255 (= Pixelwert im 8-Bit-BMP).")]
        [DisplayName("Index")]
        public int Index { get; set; }

        [Category("Farbe"), Description("Rot-Anteil der Palettenfarbe.")]
        [DisplayName("Rot")]
        public byte R { get; set; }

        [Category("Farbe"), Description("Grün-Anteil der Palettenfarbe.")]
        [DisplayName("Grün")]
        public byte G { get; set; }

        [Category("Farbe"), Description("Blau-Anteil der Palettenfarbe.")]
        [DisplayName("Blau")]
        public byte B { get; set; }

        [Category("Beschreibung"), Description("Anzeigename (frei wählbar).")]
        [DisplayName("Name")]
        public string Name { get; set; } = string.Empty;

        [Category("Beschreibung"), Description("Kategorie für eigene Sortierung, z. B. Natur, Dungeon, Stadt.")]
        [DisplayName("Kategorie")]
        public string Category { get; set; } = string.Empty;

        [Category("Dragon"), Description("Gruppe (groups.txt). -1 = keine Gruppe.")]
        [DisplayName("Gruppe")]
        public int Group { get; set; } = -1;

        [Category("Dragon"), Description("Ist die Farbe in maptrans.txt definiert? Nur dann wird daraus eine Karte erzeugt.")]
        [DisplayName("Definiert")]
        public bool Defined { get; set; }

        [Category("Dragon"), Description("Höhe (Z) des Bodens für diese Farbe, -128..127.")]
        [DisplayName("Höhe (Z)")]
        public int Altitude { get; set; }

        [Category("Dragon"), Description("Land-Tile-IDs (hex, durch Leerzeichen getrennt). Eine davon wird zufällig gewählt.")]
        [DisplayName("Tiles (hex)")]
        public string Tiles
        {
            get => _tiles;
            set
            {
                _tiles = value ?? string.Empty;
                _tileCache = null;
            }
        }

        [Category("Erweiterung"), Description("Optionales Bild / Textur (PNG, BMP) als Vorschau oder für eigene Textur-Erweiterungen.")]
        [DisplayName("Textur / Bild")]
        public string TexturePath { get; set; } = string.Empty;

        [Category("Beschreibung"), Description("Kommentar (wird beim Export als // Zeile geschrieben).")]
        [DisplayName("Kommentar")]
        public string Comment { get; set; } = string.Empty;

        [JsonIgnore, Browsable(false)]
        public Color Color => Color.FromArgb(R, G, B);

        [JsonIgnore, Browsable(false)]
        public string HexIndex => Index.ToString("x2");

        [JsonIgnore, Browsable(false)]
        public string HtmlColor => $"#{R:X2}{G:X2}{B:X2}";

        public ushort[] GetTileIds()
        {
            return _tileCache ??= HexList.Parse(_tiles);
        }

        public void SetTileIds(IEnumerable<ushort> ids)
        {
            Tiles = HexList.Format(ids);
        }

        public void SetColor(Color c)
        {
            R = c.R;
            G = c.G;
            B = c.B;
        }

        public override string ToString() => $"{HexIndex} {Name}";
    }

    /// <summary>One line of a transition file: neighbour pattern -> tile(s).</summary>
    public sealed class TransitionPattern
    {
        private string _pattern = "AAAAAAAA";
        private string _tiles = string.Empty;
        private ushort[] _tileCache;

        /// <summary>
        /// 8 characters, A = same group as the cell, B = other group.
        /// Order (clockwise from north west): NW, N, NE, E, SE, S, SW, W.
        /// </summary>
        [Description("8 Zeichen A/B. Reihenfolge im Uhrzeigersinn: NW, N, NO, O, SO, S, SW, W. A = eigene Gruppe, B = Nachbar-Gruppe.")]
        [DisplayName("Muster")]
        public string Pattern
        {
            get => _pattern;
            set => _pattern = NormalizePattern(value);
        }

        [Description("Tile-IDs hex (Map: eine wird zufällig gewählt, Statics: Einstellung im Build).")]
        [DisplayName("Tiles (hex)")]
        public string Tiles
        {
            get => _tiles;
            set
            {
                _tiles = value ?? string.Empty;
                _tileCache = null;
            }
        }

        [Description("1. Zahl nach den Tiles (Z-Versatz).")]
        [DisplayName("Z1")]
        public int Z1 { get; set; }

        [Description("2. Zahl nach den Tiles (bei Statics: zusätzlicher Wert, siehe Anleitung).")]
        [DisplayName("Z2")]
        public int Z2 { get; set; }

        [Description("Abschnitts-Kommentar (// Zeile davor).")]
        [DisplayName("Kommentar")]
        public string Comment { get; set; } = string.Empty;

        [JsonIgnore, Browsable(false)]
        public byte Mask => PatternToMask(_pattern);

        public ushort[] GetTileIds() => _tileCache ??= HexList.Parse(_tiles);

        public static string NormalizePattern(string value)
        {
            var sb = new StringBuilder(8);
            if (value != null)
            {
                foreach (char c in value)
                {
                    char u = char.ToUpperInvariant(c);
                    if (u == 'A' || u == 'B')
                    {
                        sb.Append(u);
                    }
                    if (sb.Length == 8)
                    {
                        break;
                    }
                }
            }
            while (sb.Length < 8)
            {
                sb.Append('A');
            }
            return sb.ToString();
        }

        /// <summary>Bit i is set when character i is 'B'.</summary>
        public static byte PatternToMask(string pattern)
        {
            byte mask = 0;
            for (int i = 0; i < 8 && i < pattern.Length; i++)
            {
                if (pattern[i] == 'B' || pattern[i] == 'b')
                {
                    mask |= (byte)(1 << i);
                }
            }
            return mask;
        }

        public static string MaskToPattern(byte mask)
        {
            var chars = new char[8];
            for (int i = 0; i < 8; i++)
            {
                chars[i] = (mask & (1 << i)) != 0 ? 'B' : 'A';
            }
            return new string(chars);
        }
    }

    /// <summary>A transition file (map/*.txt, statics/*.txt …). Several rules can share one file.</summary>
    public sealed class TransitionFile
    {
        /// <summary>Relative path inside the script folder, e.g. "map/grass2sand.txt".</summary>
        public string Path { get; set; } = string.Empty;

        public string HeaderComment { get; set; } = string.Empty;

        [Browsable(false)]
        public List<TransitionPattern> Patterns { get; set; } = new List<TransitionPattern>();

        public override string ToString() => $"{Path} ({Patterns.Count})";
    }

    /// <summary>Where a transition rule writes to (map tile or static).</summary>
    public enum TransitionLayer
    {
        Map,
        Static
    }

    /// <summary>
    /// A line of betweentrans*.txt / statbetweentrans*.txt.
    /// Script set 1: "AA BB file" (cell of group A next to group B).
    /// Script set 2: "AA file" (cell of group A next to any other group).
    /// Script set 3: "BB file" (cell of any other group next to group B).
    /// </summary>
    public sealed class TransitionRule
    {
        [Description("Map = Boden-Tile wird ersetzt, Static = Statics werden gesetzt.")]
        [DisplayName("Ebene")]
        public TransitionLayer Layer { get; set; }

        [Description("Skript-Satz 1, 2 oder 3 (bestimmt Listen-Datei und Unterordner).")]
        [DisplayName("Satz")]
        public int ScriptSet { get; set; } = 1;

        [Description("Gruppe der Zelle, -1 = jede andere Gruppe als B.")]
        [DisplayName("Gruppe A")]
        public int GroupA { get; set; }

        [Description("Gruppe der Nachbarn, -1 = jede andere Gruppe als A.")]
        [DisplayName("Gruppe B")]
        public int GroupB { get; set; }

        [Description("Datei relativ zum Skript-Ordner, z. B. map/grass2sand.txt (Ordner: map, map2, statics, statics2, statics3).")]
        [DisplayName("Datei")]
        public string File { get; set; } = string.Empty;

        [DisplayName("Aktiv")]

        public bool Enabled { get; set; } = true;

        [DisplayName("Kommentar")]

        public string Comment { get; set; } = string.Empty;

        /// <summary>Name of the list file in the Dragon script folder.</summary>
        [JsonIgnore, Browsable(false)]
        public string ListFileName => GetListFileName(Layer, ScriptSet);

        public static string GetListFileName(TransitionLayer layer, int set)
        {
            string baseName = layer == TransitionLayer.Map ? "betweentrans" : "statbetweentrans";
            return set <= 1 ? baseName + ".txt" : baseName + set + ".txt";
        }

        public static string GetFolder(TransitionLayer layer, int set)
        {
            string baseName = layer == TransitionLayer.Map ? "map" : "statics";
            return set <= 1 ? baseName : baseName + set;
        }

        public override string ToString()
        {
            string a = GroupA < 0 ? "*" : GroupA.ToString("x2");
            string b = GroupB < 0 ? "*" : GroupB.ToString("x2");
            return $"{Layer} {a}->{b} {File}";
        }
    }

    /// <summary>One static of an item group (5 numbers in the item scripts).</summary>
    public sealed class ItemPart
    {
        public ushort Id { get; set; }
        public int Z { get; set; }
        public ushort Hue { get; set; }
        public int Dx { get; set; }
        public int Dy { get; set; }

        public override string ToString() => $"{Id:x4} z{Z} hue{Hue:x4} ({Dx},{Dy})";
    }

    /// <summary>One line of an item file: frequency + statics (multi-tile objects possible).</summary>
    public sealed class ItemGroupDef
    {
        [DisplayName("Häufigkeit")]
        public int Frequency { get; set; } = 1;
        [DisplayName("Kommentar")]
        public string Comment { get; set; } = string.Empty;
        [Browsable(false)]
        public List<ItemPart> Parts { get; set; } = new List<ItemPart>();

        /// <summary>The statics as text (for grids): "id z hue dx dy; …".</summary>
        [JsonIgnore, DisplayName("Statics (id z hue dx dy; …)")]
        public string PartsText
        {
            get
            {
                var sb = new StringBuilder();
                foreach (ItemPart p in Parts)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append(CultureInfo.InvariantCulture, $"{p.Id:x4} {p.Z} {p.Hue:x4} {p.Dx} {p.Dy}");
                }
                return sb.ToString();
            }
            set
            {
                var list = new List<ItemPart>();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    foreach (string chunk in value.Split(';'))
                    {
                        string[] t = chunk.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (t.Length == 0)
                        {
                            continue;
                        }
                        var p = new ItemPart { Id = HexList.ParseHex(t[0]) };
                        if (t.Length > 1) { p.Z = HexList.ParseInt(t[1]); }
                        if (t.Length > 2) { p.Hue = HexList.ParseHex(t[2]); }
                        if (t.Length > 3) { p.Dx = HexList.ParseInt(t[3]); }
                        if (t.Length > 4) { p.Dy = HexList.ParseInt(t[4]); }
                        list.Add(p);
                    }
                }
                Parts = list;
            }
        }
    }

    public sealed class ItemFile
    {
        public string Path { get; set; } = string.Empty;
        public string HeaderComment { get; set; } = string.Empty;
        [Browsable(false)]
        public List<ItemGroupDef> Groups { get; set; } = new List<ItemGroupDef>();

        public override string ToString() => $"{Path} ({Groups.Count})";
    }

    /// <summary>A line of items.txt: place random items of a file on a group inside a rectangle.</summary>
    public sealed class ItemRule
    {
        [DisplayName("Gruppe")]
        public int Group { get; set; }

        [Description("Chance pro Zelle (Basis = Build-Einstellung, Standard 1000 = Promille).")]
        [DisplayName("Chance")]
        public int Chance { get; set; }

        [DisplayName("Links")]

        public int Left { get; set; }
        [DisplayName("Oben")]
        public int Top { get; set; }
        [DisplayName("Rechts")]
        public int Right { get; set; } = 6143;
        [DisplayName("Unten")]
        public int Bottom { get; set; } = 4095;

        [Description("Datei relativ zum Skript-Ordner, z. B. items/grass.txt")]
        [DisplayName("Datei")]
        public string File { get; set; } = string.Empty;

        [DisplayName("Aktiv")]

        public bool Enabled { get; set; } = true;
        [DisplayName("Kommentar")]
        public string Comment { get; set; } = string.Empty;
    }

    /// <summary>A line of unevenly.txt: random altitude for a group.</summary>
    public sealed class UnevenRule
    {
        [DisplayName("Gruppe")]
        public int Group { get; set; }

        [Description("Häufigkeit 1..1000 (Promille).")]
        [DisplayName("Häufigkeit")]
        public int Frequency { get; set; } = 100;

        [DisplayName("Links")]

        public int Left { get; set; }
        [DisplayName("Oben")]
        public int Top { get; set; }
        [DisplayName("Rechts")]
        public int Right { get; set; } = 6143;
        [DisplayName("Unten")]
        public int Bottom { get; set; } = 4095;

        [Description("Zufällige Höhe 0..Amount wird addiert.")]
        [DisplayName("Höhe +")]
        public int Amount { get; set; } = 2;

        [DisplayName("Aktiv")]

        public bool Enabled { get; set; } = true;
        [DisplayName("Kommentar")]
        public string Comment { get; set; } = string.Empty;
    }

    /// <summary>A line of a fix table: sets one map cell after everything else.</summary>
    public sealed class FixEntry
    {
        [DisplayName("X")]
        public int X { get; set; }
        [DisplayName("Y")]
        public int Y { get; set; }
        [DisplayName("Tile")]
        public ushort TileId { get; set; }
        [DisplayName("Z")]
        public int Z { get; set; }
        [DisplayName("Kommentar")]
        public string Comment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maps a foreign RGB colour (from photos, other tools …) to a palette index.
    /// Used when an RGB image is imported – extends the 256 Dragon colours without breaking the BMP format.
    /// </summary>
    public sealed class ColorAlias
    {
        [DisplayName("Name")]
        public string Name { get; set; } = string.Empty;
        [DisplayName("Rot")]
        public byte R { get; set; }
        [DisplayName("Grün")]
        public byte G { get; set; }
        [DisplayName("Blau")]
        public byte B { get; set; }

        [Description("Palettenindex, auf den diese Farbe abgebildet wird.")]
        [DisplayName("Index")]
        public int Index { get; set; }

        [JsonIgnore, Browsable(false)]
        public int Rgb => (R << 16) | (G << 8) | B;
    }

    /// <summary>Small helpers for the hex / decimal numbers of the Dragon scripts.</summary>
    public static class HexList
    {
        public static ushort[] Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Array.Empty<ushort>();
            }
            string[] parts = text.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<ushort>(parts.Length);
            foreach (string p in parts)
            {
                if (TryParseHex(p, out ushort v))
                {
                    list.Add(v);
                }
            }
            return list.ToArray();
        }

        public static string Format(IEnumerable<ushort> ids)
        {
            var sb = new StringBuilder();
            foreach (ushort id in ids)
            {
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }
                sb.Append(id.ToString("x4"));
            }
            return sb.ToString();
        }

        public static bool TryParseHex(string s, out ushort value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }
            s = s.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(2);
            }
            return ushort.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        public static ushort ParseHex(string s) => TryParseHex(s, out ushort v) ? v : (ushort)0;

        public static int ParseHexInt(string s)
        {
            s = s.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(2);
            }
            return int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int v) ? v : 0;
        }

        public static bool TryParseHexByte(string s, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(s) || s.Length > 2)
            {
                return false;
            }
            return int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        public static int ParseInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return 0;
            }
            s = s.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return ParseHexInt(s);
            }
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
        }

        public static bool IsInt(string s) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
    }
}
