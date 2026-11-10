using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GumpCalc
{
    /// <summary>How a typed number is read.</summary>
    internal enum NumberStyle { Auto, Hex, Decimal }

    /// <summary>Pure Ultima Online calculations (no UI, no files) used by the Gump ID calculator.</summary>
    internal static class UoCalc
    {
        // ------------------------------------------------------------------ numbers

        /// <summary>
        /// Reads a number. Auto: "0x…", "&amp;h…", "…h" or letters A-F = hex, otherwise decimal.
        /// Spaces and "_" are ignored, a leading "-" is allowed.
        /// </summary>
        public static bool TryParse(string text, NumberStyle style, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            string t = text.Trim().Replace(" ", string.Empty).Replace("_", string.Empty);
            bool negative = t.StartsWith("-", StringComparison.Ordinal);
            if (negative)
            {
                t = t.Substring(1);
            }
            bool hex = style == NumberStyle.Hex;
            if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || t.StartsWith("&h", StringComparison.OrdinalIgnoreCase))
            {
                t = t.Substring(2);
                hex = style != NumberStyle.Decimal;
            }
            else if (t.StartsWith("#", StringComparison.Ordinal))
            {
                t = t.Substring(1);
                hex = style != NumberStyle.Decimal;
            }
            else if (style != NumberStyle.Decimal && t.Length > 1 && (t.EndsWith("h", StringComparison.OrdinalIgnoreCase)))
            {
                t = t.Substring(0, t.Length - 1);
                hex = true;
            }
            else if (style == NumberStyle.Auto)
            {
                foreach (char c in t)
                {
                    if ((c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                    {
                        hex = true;
                        break;
                    }
                }
            }
            if (t.Length == 0)
            {
                return false;
            }
            bool ok = hex
                ? long.TryParse(t, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value)
                : long.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out value);
            if (ok && negative)
            {
                value = -value;
            }
            return ok;
        }

        public static bool TryParseInt(string text, out int value, NumberStyle style = NumberStyle.Auto)
        {
            value = 0;
            if (!TryParse(text, style, out long v) || v < int.MinValue || v > int.MaxValue)
            {
                return false;
            }
            value = (int)v;
            return true;
        }

        public static string Hex(long value, int digits = 4) => value < 0 ? "-0x" + (-value).ToString("X" + digits) : "0x" + value.ToString("X" + digits);

        public static string Both(long value, int digits = 4) => $"{Hex(value, digits)} ({value})";

        public static string ToBase(long value, int radix)
        {
            if (radix < 2 || radix > 36)
            {
                throw new ArgumentOutOfRangeException(nameof(radix));
            }
            if (value == 0)
            {
                return "0";
            }
            const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            bool negative = value < 0;
            ulong v = negative ? (ulong)(-(value + 1)) + 1 : (ulong)value;
            var sb = new StringBuilder();
            while (v > 0)
            {
                sb.Insert(0, digits[(int)(v % (ulong)radix)]);
                v /= (ulong)radix;
            }
            return negative ? "-" + sb : sb.ToString();
        }

        public static bool TryFromBase(string text, int radix, out long value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text) || radix < 2 || radix > 36)
            {
                return false;
            }
            string t = text.Trim().Replace(" ", string.Empty).Replace("_", string.Empty).ToUpperInvariant();
            bool negative = t.StartsWith("-", StringComparison.Ordinal);
            if (negative)
            {
                t = t.Substring(1);
            }
            if (t.Length == 0)
            {
                return false;
            }
            const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            try
            {
                checked
                {
                    foreach (char c in t)
                    {
                        int d = digits.IndexOf(c);
                        if (d < 0 || d >= radix)
                        {
                            return false;
                        }
                        value = value * radix + d;
                    }
                }
            }
            catch (OverflowException)
            {
                return false;
            }
            if (negative)
            {
                value = -value;
            }
            return true;
        }

        /// <summary>Binary in groups of 4 bits: 0101 1100.</summary>
        public static string Binary(long value)
        {
            string b = Convert.ToString(value, 2);
            int pad = (4 - b.Length % 4) % 4;
            b = new string('0', pad) + b;
            var sb = new StringBuilder();
            for (int i = 0; i < b.Length; i += 4)
            {
                if (i > 0)
                {
                    sb.Append(' ');
                }
                sb.Append(b, i, 4);
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------ paperdoll gumps

        public const int MaleGumpOffset = 50000;   // 0xC350
        public const int FemaleGumpOffset = 60000; // 0xEA60

        public static int MaleGump(int animId) => animId + MaleGumpOffset;

        public static int FemaleGump(int animId) => animId + FemaleGumpOffset;

        /// <summary>Which kind of gump an ID is: 0 = none, 1 = male paperdoll gump, 2 = female paperdoll gump.</summary>
        public static int GumpGender(int gumpId, out int animId)
        {
            if (gumpId >= FemaleGumpOffset)
            {
                animId = gumpId - FemaleGumpOffset;
                return 2;
            }
            if (gumpId >= MaleGumpOffset)
            {
                animId = gumpId - MaleGumpOffset;
                return 1;
            }
            animId = -1;
            return 0;
        }

        // ------------------------------------------------------------------ art index

        public const int StaticArtOffset = 0x4000;

        public static int ItemToArtIndex(int itemId) => itemId + StaticArtOffset;

        // ------------------------------------------------------------------ animations

        public static string AnimFileName(int fileType) => fileType <= 1 ? "anim.mul" : $"anim{fileType}.mul";

        /// <summary>Kind of body by its slot in the anim file: 0 = monster (22 actions), 1 = animal (13), 2 = human / equipment (35).</summary>
        public static int AnimGroup(int body, int fileType)
        {
            switch (fileType)
            {
                case 2:
                    return body < 200 ? 0 : 1;
                case 3:
                    return body < 300 ? 1 : body < 400 ? 0 : 2;
                case 5:
                    return body < 200 && body != 34 ? 0 : body < 400 ? 1 : 2;
                default:
                    return body < 200 ? 0 : body < 400 ? 1 : 2;
            }
        }

        public static readonly string[] AnimGroupNames = { "Monster (high detail, 22 actions)", "Animal (low detail, 13 actions)", "Human / equipment (35 actions)" };

        public static int ActionCount(int group) => group == 0 ? 22 : group == 1 ? 13 : 35;

        /// <summary>Index in anim*.idx – same rules as the UoFiddler animation loader.</summary>
        public static int AnimIndex(int body, int action, int direction, int fileType)
        {
            int index;
            switch (fileType)
            {
                case 2:
                    index = body < 200 ? body * 110 : 22000 + (body - 200) * 65;
                    break;
                case 3:
                    index = body < 300 ? body * 65 : body < 400 ? 33000 + (body - 300) * 110 : 35000 + (body - 400) * 175;
                    break;
                case 5:
                    index = body < 200 && body != 34 ? body * 110 : body < 400 ? 22000 + (body - 200) * 65 : 35000 + (body - 400) * 175;
                    break;
                default:
                    index = body < 200 ? body * 110 : body < 400 ? 22000 + (body - 200) * 65 : 35000 + (body - 400) * 175;
                    break;
            }
            index += action * 5;
            index += direction <= 4 ? direction : direction - (direction - 4) * 2;
            return index;
        }

        public static readonly string[] DirectionNames = { "0 North (up)", "1 North-East (right)", "2 East", "3 South-East (down)", "4 South", "5 South-West (left)", "6 West", "7 North-West" };

        // ------------------------------------------------------------------ colors and hues

        /// <summary>RGB → 15 bit UO color (0RRRRRGGGGGBBBBB).</summary>
        public static int RgbTo555(Color c) => ((c.R >> 3) << 10) | ((c.G >> 3) << 5) | (c.B >> 3);

        /// <summary>15 bit UO color → RGB, scaled like the client ((v &lt;&lt; 3) | (v &gt;&gt; 2)).</summary>
        public static Color Color555ToRgb(int v)
        {
            int r = (v >> 10) & 0x1F, g = (v >> 5) & 0x1F, b = v & 0x1F;
            return Color.FromArgb((r << 3) | (r >> 2), (g << 3) | (g >> 2), (b << 3) | (b >> 2));
        }

        public static string HtmlHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        public const int HuePartialFlag = 0x8000;
        public const int HueIndexMask = 0x3FFF;

        /// <summary>Hue value as used in scripts and packets → plain hue number (bit 0x8000 = only gray pixels).</summary>
        public static int HueNumber(int value, out bool partial)
        {
            partial = (value & HuePartialFlag) != 0;
            return value & HueIndexMask;
        }

        public static int ColorDistance(Color a, Color b)
        {
            int dr = a.R - b.R, dg = a.G - b.G, db = a.B - b.B;
            return dr * dr * 3 + dg * dg * 4 + db * db * 2;
        }

        // ------------------------------------------------------------------ maps

        public sealed class MapInfo
        {
            public MapInfo(string name, int width, int height)
            {
                Name = name;
                Width = width;
                Height = height;
            }

            public string Name { get; }
            public int Width { get; set; }
            public int Height { get; set; }
            public override string ToString() => $"{Name} ({Width} × {Height})";
        }

        /// <summary>Usual map sizes (the real size of the loaded client is used when available).</summary>
        public static List<MapInfo> DefaultMaps() => new List<MapInfo>
        {
            new MapInfo("Felucca (map0)", 7168, 4096),
            new MapInfo("Trammel (map1)", 7168, 4096),
            new MapInfo("Ilshenar (map2)", 2304, 1600),
            new MapInfo("Malas (map3)", 2560, 2048),
            new MapInfo("Tokuno (map4)", 1448, 1448),
            new MapInfo("Ter Mur (map5)", 1280, 4096),
            new MapInfo("Custom", 6144, 4096)
        };

        public const int MapBlockBytes = 196;  // 4 byte header + 64 cells × 3 byte
        public const int StaIdxBytes = 12;     // lookup, length, extra

        public static int BlockIndex(int x, int y, int mapHeight) => (x >> 3) * (mapHeight >> 3) + (y >> 3);

        public static int CellInBlock(int x, int y) => ((y & 7) << 3) + (x & 7);

        /// <summary>
        /// Sextant position like RunUO / ServUO (Sextant.Format). mapIndex 0/1 = Felucca/Trammel with the dungeon area
        /// right of x 5120, other maps use the same center.
        /// </summary>
        public static bool ToSextant(int mapIndex, int x, int y, int mapWidth, int mapHeight, out string text, out int latDeg, out int latMin, out bool south, out int lonDeg, out int lonMin, out bool east)
        {
            text = null;
            latDeg = latMin = lonDeg = lonMin = 0;
            south = east = false;
            if (!SextantCenter(mapIndex, x, y, mapWidth, mapHeight, out int xCenter, out int yCenter))
            {
                return false;
            }
            const int xWidth = 5120, yHeight = 4096;
            double absLong = (double)(x - xCenter) * 360 / xWidth;
            double absLat = (double)(y - yCenter) * 360 / yHeight;
            if (absLong > 180.0)
            {
                absLong = -180.0 + absLong % 180.0;
            }
            if (absLat > 180.0)
            {
                absLat = -180.0 + absLat % 180.0;
            }
            east = absLong >= 0;
            south = absLat >= 0;
            absLong = Math.Abs(absLong);
            absLat = Math.Abs(absLat);
            lonDeg = (int)absLong;
            latDeg = (int)absLat;
            lonMin = (int)(absLong % 1.0 * 60);
            latMin = (int)(absLat % 1.0 * 60);
            text = $"{latDeg}° {latMin}'{(south ? "S" : "N")}, {lonDeg}° {lonMin}'{(east ? "E" : "W")}";
            return true;
        }

        private static bool SextantCenter(int mapIndex, int x, int y, int mapWidth, int mapHeight, out int xCenter, out int yCenter)
        {
            xCenter = 1323;
            yCenter = 1624;
            if (mapIndex == 0 || mapIndex == 1)
            {
                if (x >= 0 && y >= 0 && x < 5120 && y < mapHeight)
                {
                    return true;
                }
                if (x >= 5120 && y >= 2304 && x < 6144 && y < mapHeight)
                {
                    xCenter = 5936;
                    yCenter = 3112;
                    return true;
                }
                return false;
            }
            return x >= 0 && y >= 0 && x < mapWidth && y < mapHeight;
        }

        /// <summary>Sextant → map position (center of the minute, can be off by a few tiles because minutes are rounded).</summary>
        public static Point FromSextant(int latDeg, int latMin, bool south, int lonDeg, int lonMin, bool east, bool dungeonArea, int mapWidth, int mapHeight)
        {
            const int xWidth = 5120, yHeight = 4096;
            int xCenter = dungeonArea ? 5936 : 1323, yCenter = dungeonArea ? 3112 : 1624;
            double lon = (lonDeg + (lonMin + 0.5) / 60.0) * (east ? 1 : -1);
            double lat = (latDeg + (latMin + 0.5) / 60.0) * (south ? 1 : -1);
            int x = xCenter + (int)Math.Round(lon * xWidth / 360.0);
            int y = yCenter + (int)Math.Round(lat * yHeight / 360.0);
            int wrapW = dungeonArea ? xWidth : Math.Min(mapWidth, xWidth);
            if (!dungeonArea)
            {
                x = ((x % wrapW) + wrapW) % wrapW;
            }
            y = ((y % yHeight) + yHeight) % Math.Max(1, Math.Min(yHeight, mapHeight));
            return new Point(x, y);
        }

        // ------------------------------------------------------------------ layers

        public sealed class LayerInfo
        {
            public LayerInfo(int id, string name, string german, string example)
            {
                Id = id;
                Name = name;
                German = german;
                Example = example;
            }

            public int Id { get; }
            public string Name { get; }
            public string German { get; }
            public string Example { get; }
        }

        public static readonly LayerInfo[] Layers =
        {
            new LayerInfo(0x00, "Invalid", "Kein Layer", "-"),
            new LayerInfo(0x01, "OneHanded", "Einhändig (rechte Hand)", "Dolch, Schwert"),
            new LayerInfo(0x02, "TwoHanded", "Zweihändig / Schild (linke Hand)", "Schild, Bogen, Hellebarde"),
            new LayerInfo(0x03, "Shoes", "Schuhe", "Stiefel, Sandalen"),
            new LayerInfo(0x04, "Pants", "Hose", "Lange Hose"),
            new LayerInfo(0x05, "Shirt", "Hemd", "Hemd, Fancy Shirt"),
            new LayerInfo(0x06, "Helm", "Helm / Hut", "Helm, Hut"),
            new LayerInfo(0x07, "Gloves", "Handschuhe", "Handschuhe"),
            new LayerInfo(0x08, "Ring", "Ring", "Ring"),
            new LayerInfo(0x09, "Talisman", "Talisman", "Talisman"),
            new LayerInfo(0x0A, "Neck", "Hals", "Kette, Halsberge"),
            new LayerInfo(0x0B, "Hair", "Haare", "Haare"),
            new LayerInfo(0x0C, "Waist", "Hüfte", "Halbe Schürze"),
            new LayerInfo(0x0D, "InnerTorso", "Oberkörper innen", "Brustpanzer, Kleid"),
            new LayerInfo(0x0E, "Bracelet", "Armband", "Armband"),
            new LayerInfo(0x0F, "Face", "Gesicht (neuere Clients)", "Gesicht"),
            new LayerInfo(0x10, "FacialHair", "Bart", "Bart"),
            new LayerInfo(0x11, "MiddleTorso", "Oberkörper Mitte", "Wappenrock, Schärpe"),
            new LayerInfo(0x12, "Earrings", "Ohrringe", "Ohrringe"),
            new LayerInfo(0x13, "Arms", "Arme", "Armschienen"),
            new LayerInfo(0x14, "Cloak", "Umhang", "Umhang, Köcher"),
            new LayerInfo(0x15, "Backpack", "Rucksack", "Rucksack"),
            new LayerInfo(0x16, "OuterTorso", "Oberkörper außen", "Robe, Mantel"),
            new LayerInfo(0x17, "OuterLegs", "Beine außen", "Rock, Kilt"),
            new LayerInfo(0x18, "InnerLegs", "Beine innen", "Beinschienen"),
            new LayerInfo(0x19, "Mount", "Reittier", "Mount-Item"),
            new LayerInfo(0x1A, "ShopBuy", "Händler: kauft", "-"),
            new LayerInfo(0x1B, "ShopResale", "Händler: Wiederverkauf", "-"),
            new LayerInfo(0x1C, "ShopSell", "Händler: verkauft", "-"),
            new LayerInfo(0x1D, "Bank", "Bankfach", "-")
        };

        public static string LayerName(int layer) => layer >= 0 && layer < Layers.Length ? Layers[layer].Name : $"0x{layer:X2}";

        // ------------------------------------------------------------------ scripts

        public static readonly string[] ScriptKinds = { "ServUO / RunUO (C#)", "Sphere (.scp)", "POL (itemdesc.cfg)", "gump.def line", "Summary" };

        public static string MakeScript(int kind, int itemId, string name, int layer, int animId, int hue)
        {
            string cls = MakeClassName(name);
            string layerName = LayerName(layer);
            switch (kind)
            {
                case 0:
                    return
$@"public class {cls} : BaseClothing   // or BaseArmor / BaseWeapon …
{{
    [Constructable]
    public {cls}() : this({hue}) {{ }}

    [Constructable]
    public {cls}(int hue) : base(0x{itemId:X4}, Layer.{layerName}, hue)
    {{
        Name = ""{name}"";
    }}

    public {cls}(Serial serial) : base(serial) {{ }}

    public override void Serialize(GenericWriter writer) {{ base.Serialize(writer); writer.Write(0); }}
    public override void Deserialize(GenericReader reader) {{ base.Deserialize(reader); reader.ReadInt(); }}
}}
// tiledata: Animation = {animId} (0x{animId:X}) → gump male {MaleGump(animId)} (0x{MaleGump(animId):X4}), female {FemaleGump(animId)} (0x{FemaleGump(animId):X4})";
                case 1:
                    return
$@"[ITEMDEF 0{itemId:X4}]
DEFNAME=i_{cls.ToLowerInvariant()}
NAME={name}
ID=0{itemId:X4}
TYPE=t_clothing
LAYER={layer}
//COLOR={hue}
// anim {animId} → gump male 0{MaleGump(animId):X4}, female 0{FemaleGump(animId):X4}";
                case 2:
                    return
$@"Item 0x{itemId:X4}
{{
    Name        {cls}
    Desc        {name}
    Graphic     0x{itemId:X4}
    Color       {hue}
    // layer {layer} ({layerName}), anim {animId}
}}";
                case 3:
                    return
$@"# gump.def: <gump id> {{<replacement gump id>}} <hue>
# female gump missing → show the male one:
{FemaleGump(animId)} {{{MaleGump(animId)}}} 0";
                default:
                    return
$@"Item ID     : 0x{itemId:X4} ({itemId})  art index 0x{ItemToArtIndex(itemId):X4}
Name        : {name}
Layer       : {layer} (0x{layer:X2}) {layerName}
AnimID      : {animId} (0x{animId:X})
Gump male   : {MaleGump(animId)} (0x{MaleGump(animId):X4})
Gump female : {FemaleGump(animId)} (0x{FemaleGump(animId):X4})";
            }
        }

        private static string MakeClassName(string name)
        {
            var sb = new StringBuilder();
            bool upper = true;
            foreach (char c in name ?? string.Empty)
            {
                if (char.IsLetterOrDigit(c) && c < 128)
                {
                    sb.Append(upper ? char.ToUpperInvariant(c) : c);
                    upper = false;
                }
                else
                {
                    upper = true;
                }
            }
            if (sb.Length == 0 || char.IsDigit(sb[0]))
            {
                sb.Insert(0, "Item");
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------ formula

        /// <summary>
        /// Evaluates an integer formula: + - * / % ( ) &lt;&lt; &gt;&gt; &amp; | ^ ~, numbers as decimal, 0x… hex, 0b… binary, …h hex.
        /// Example: "0x4000 + 1234 * 2", "(0xC350 - 50000) | 0x8000".
        /// </summary>
        public static long Evaluate(string formula)
        {
            var p = new FormulaParser(formula ?? string.Empty);
            long v = p.ParseOr();
            p.SkipSpaces();
            if (!p.End)
            {
                throw new FormatException($"Unexpected \"{p.Rest}\"");
            }
            return v;
        }

        private sealed class FormulaParser
        {
            private readonly string _s;
            private int _i;

            public FormulaParser(string s) => _s = s;

            public bool End => _i >= _s.Length;

            public string Rest => _s.Substring(_i);

            public void SkipSpaces()
            {
                while (_i < _s.Length && char.IsWhiteSpace(_s[_i]))
                {
                    _i++;
                }
            }

            private bool Take(string op)
            {
                SkipSpaces();
                if (string.CompareOrdinal(_s, _i, op, 0, op.Length) == 0)
                {
                    _i += op.Length;
                    return true;
                }
                return false;
            }

            public long ParseOr()
            {
                long v = ParseXor();
                while (true)
                {
                    SkipSpaces();
                    if (_i < _s.Length && _s[_i] == '|')
                    {
                        _i++;
                        v |= ParseXor();
                    }
                    else
                    {
                        return v;
                    }
                }
            }

            private long ParseXor()
            {
                long v = ParseAnd();
                while (Take("^"))
                {
                    v ^= ParseAnd();
                }
                return v;
            }

            private long ParseAnd()
            {
                long v = ParseShift();
                while (Take("&"))
                {
                    v &= ParseShift();
                }
                return v;
            }

            private long ParseShift()
            {
                long v = ParseAdd();
                while (true)
                {
                    if (Take("<<"))
                    {
                        v <<= (int)ParseAdd();
                    }
                    else if (Take(">>"))
                    {
                        v >>= (int)ParseAdd();
                    }
                    else
                    {
                        return v;
                    }
                }
            }

            private long ParseAdd()
            {
                long v = ParseMul();
                while (true)
                {
                    if (Take("+"))
                    {
                        v += ParseMul();
                    }
                    else if (Take("-"))
                    {
                        v -= ParseMul();
                    }
                    else
                    {
                        return v;
                    }
                }
            }

            private long ParseMul()
            {
                long v = ParseUnary();
                while (true)
                {
                    if (Take("*"))
                    {
                        v *= ParseUnary();
                    }
                    else if (Take("/"))
                    {
                        long d = ParseUnary();
                        if (d == 0)
                        {
                            throw new DivideByZeroException("Division by 0");
                        }
                        v /= d;
                    }
                    else if (Take("%"))
                    {
                        long d = ParseUnary();
                        if (d == 0)
                        {
                            throw new DivideByZeroException("Division by 0");
                        }
                        v %= d;
                    }
                    else
                    {
                        return v;
                    }
                }
            }

            private long ParseUnary()
            {
                if (Take("-"))
                {
                    return -ParseUnary();
                }
                if (Take("+"))
                {
                    return ParseUnary();
                }
                if (Take("~"))
                {
                    return ~ParseUnary();
                }
                if (Take("("))
                {
                    long v = ParseOr();
                    if (!Take(")"))
                    {
                        throw new FormatException("Missing )");
                    }
                    return v;
                }
                return ParseNumber();
            }

            private long ParseNumber()
            {
                SkipSpaces();
                int start = _i;
                while (_i < _s.Length && (char.IsLetterOrDigit(_s[_i]) || _s[_i] == '_'))
                {
                    _i++;
                }
                string t = _s.Substring(start, _i - start).Replace("_", string.Empty);
                if (t.Length == 0)
                {
                    throw new FormatException(End ? "Number missing at the end" : $"Unexpected \"{Rest}\"");
                }
                if (t.StartsWith("0b", StringComparison.OrdinalIgnoreCase) && TryFromBase(t.Substring(2), 2, out long bin))
                {
                    return bin;
                }
                bool hex = t.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || (t.Length > 1 && (t.EndsWith("h") || t.EndsWith("H")));
                if (TryParse(t, hex ? NumberStyle.Hex : NumberStyle.Decimal, out long v))
                {
                    return v;
                }
                throw new FormatException($"\"{t}\" is not a number (hex needs 0x)");
            }
        }
    }
}
