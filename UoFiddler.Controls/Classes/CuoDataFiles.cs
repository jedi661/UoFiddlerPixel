/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * The text files of ClassicUO in <ClassicUO>\Data\Client\ (formats as read
 * by ClassicUO: ChairTable, StaticFilters, ContainerManager, SeasonManager,
 * LightColors):
 *
 *   chair.txt         GRAPHIC,D1,D2,D3,D4,OFFSET_Y,MIRROR_OFFSET_Y
 *   cave.txt          GRAPHIC
 *   vegetation.txt    GRAPHIC
 *   tree.txt          GRAPHIC=FLAG                  (flag 0/1 = stump type)
 *   containers.txt    GUMP OPEN_SOUND CLOSE_SOUND X Y WIDTH HEIGHT [ICONIZED] [MIN_X] [MIN_Y]
 *   seasons.txt       SEASON,static|landtile,ORIGINAL,REPLACEMENT
 *   lights.txt        ITEM_ID SHADER   or   ITEM_ID H<hue>
 *   lightshaders.txt  ID RGB R_CURVE [G_CURVE B_CURVE]
 *
 * Separators: space, tab and comma (seasons.txt: comma only).
 * Comment lines start with '#' or ';' (seasons.txt: '#' or '//').
 * Numbers are decimal; seasons.txt and lights.txt also accept 0x… (hex).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace UoFiddler.Controls.Classes
{
    public enum CuoFileKind
    {
        Chair,
        Cave,
        Vegetation,
        Tree,
        Containers,
        Seasons,
        Lights,
        LightShaders
    }

    /// <summary>what a line of a CUO file refers to</summary>
    public sealed class CuoLineInfo
    {
        public bool IsComment { get; set; }

        /// <summary>null = line is fine</summary>
        public string Error { get; set; }

        /// <summary>static item graphics of the line</summary>
        public List<int> Items { get; } = new List<int>();

        /// <summary>land tile graphics of the line (seasons.txt)</summary>
        public List<int> Lands { get; } = new List<int>();

        /// <summary>gump id (containers.txt)</summary>
        public int? Gump { get; set; }

        /// <summary>the id the line is "about" (to find duplicates)</summary>
        public int? Key { get; set; }
    }

    public sealed class CuoProblem
    {
        public int Line { get; set; }
        public string Message { get; set; }

        public override string ToString() => $"Line {Line}: {Message}";
    }

    public static class CuoDataFiles
    {
        public static readonly CuoFileKind[] All = (CuoFileKind[])Enum.GetValues(typeof(CuoFileKind));

        public static string FileName(CuoFileKind kind) => kind.ToString().ToLowerInvariant() + ".txt";

        public static CuoFileKind? KindOf(string name)
        {
            string n = Path.GetFileNameWithoutExtension(name ?? string.Empty).Trim().ToLowerInvariant();
            foreach (CuoFileKind k in All)
            {
                if (k.ToString().ToLowerInvariant() == n)
                {
                    return k;
                }
            }

            return null;
        }

        /// <summary>
        /// Folder with the txt files: accepts the ClassicUO folder, its Data folder or Data\Client itself.
        /// </summary>
        public static string ResolveFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            path = path.Trim().Trim('"');
            foreach (string candidate in new[] { path, Path.Combine(path, "Client"), Path.Combine(path, "Data", "Client") })
            {
                if (Directory.Exists(candidate) && All.Any(k => File.Exists(Path.Combine(candidate, FileName(k)))))
                {
                    return candidate;
                }
            }

            // no txt file yet: the Data\Client folder when it exists, otherwise the given folder
            if (Directory.Exists(Path.Combine(path, "Data", "Client")))
            {
                return Path.Combine(path, "Data", "Client");
            }

            return Directory.Exists(Path.Combine(path, "Client")) ? Path.Combine(path, "Client") : path;
        }

        /// <summary>short description of the file (shown next to the editor)</summary>
        public static string Description(CuoFileKind kind)
        {
            switch (kind)
            {
                case CuoFileKind.Chair:
                    return "chair.txt – tiles a character can sit on.\n\n" +
                           "Format:  GRAPHIC,D1,D2,D3,D4,OFFSET_Y,MIRROR_OFFSET_Y\n" +
                           "D1..D4 = direction the character looks when sitting down coming from north/east/south/west:\n" +
                           "0 = north, 2 = east, 4 = south, 6 = west, -1 = not possible from this side.\n" +
                           "OFFSET_Y / MIRROR_OFFSET_Y move the sitting character up/down.\n\n" +
                           "Examples:\n" +
                           "2865,0,0,0,0,0,0     chair facing north\n" +
                           "2863,2,2,2,2,6,6     chair facing east\n" +
                           "2910,0,2,4,6,-8,-8   stool: all four directions\n\n" +
                           "All numbers decimal. At least 7 values per line.";
                case CuoFileKind.Cave:
                    return "cave.txt – graphics of cave walls/entrances.\n\n" +
                           "Format: one decimal GRAPHIC per line.\n" +
                           "ClassicUO uses this list for the cave tile handling (e.g. hiding cave walls).\n\nExample:\n1339";
                case CuoFileKind.Vegetation:
                    return "vegetation.txt – grass, bushes, flowers … that ClassicUO can hide\n" +
                           "(option \"hide vegetation\").\n\n" +
                           "Format: one decimal GRAPHIC per line.\n\nExample:\n3397";
                case CuoFileKind.Tree:
                    return "tree.txt – trees that ClassicUO can show as stumps (option \"trees to stumps\").\n\n" +
                           "Format:  GRAPHIC=FLAG\n" +
                           "FLAG 0 = normal stump, 1 = hatched stump.\n" +
                           "Trees that are not impassable belong into vegetation.txt.\n\nExample:\n3226=0";
                case CuoFileKind.Containers:
                    return "containers.txt – container gumps.\n\n" +
                           "Format:  GUMP OPEN_SOUND CLOSE_SOUND X Y WIDTH HEIGHT [ICONIZED_GUMP] [MIN_X] [MIN_Y]\n" +
                           "X/Y/WIDTH/HEIGHT = area in the gump where items are placed.\n" +
                           "The last three values are optional (0 = none).\n\n" +
                           "Examples:\n60 72 88 44 65 186 159 80 105 162\n7 0 0 30 30 270 170";
                case CuoFileKind.Seasons:
                    return "seasons.txt – graphics that are replaced in a season.\n\n" +
                           "Format:  SEASON,static|landtile,ORIGINAL,REPLACEMENT\n" +
                           "SEASON = spring, summer, fall, winter, desolation.\n" +
                           "Numbers decimal or hex (0x…). Separator: comma only, no spaces.\n" +
                           "Comments: lines starting with # or //.\n\n" +
                           "Examples:\nspring,static,0x0CA7,0x0C84\nwinter,landtile,196,282";
                case CuoFileKind.Lights:
                    return "lights.txt – light of items.\n\n" +
                           "Format:  ITEM_ID SHADER   or   ITEM_ID H<hue>\n" +
                           "ITEM_ID decimal or hex (0x…). SHADER = id from lightshaders.txt,\n" +
                           "H<hue> = coloured with a hue (hue number as in the game).\n\n" +
                           "Examples:\n0xE31 35\n0xE31 H1234";
                case CuoFileKind.LightShaders:
                    return "lightshaders.txt – colours of the light shaders.\n\n" +
                           "Format:  ID RGB R_CURVE [G_CURVE B_CURVE]\n" +
                           "RGB = colour as hex (FF7F00). Curves: Standard, A, B, C, D, E\n" +
                           "(A small, B very small/dim, C full, D medium dim, E halo).\n" +
                           "With only R_CURVE, green and blue use the same curve. Max. 63 shaders.\n\n" +
                           "Example:\n30 FF7F00 C C Standard";
                default:
                    return string.Empty;
            }
        }

        /// <summary>true when tiles of the TileData tab can be added to this file</summary>
        public static bool AcceptsItems(CuoFileKind kind) => kind != CuoFileKind.Containers && kind != CuoFileKind.LightShaders;

        /// <summary>new line for an item graphic (land = graphic is a land tile, only seasons.txt)</summary>
        public static string NewLine(CuoFileKind kind, int graphic, bool land)
        {
            switch (kind)
            {
                case CuoFileKind.Chair:
                    return $"{graphic},0,0,0,0,0,0";
                case CuoFileKind.Cave:
                case CuoFileKind.Vegetation:
                    return graphic.ToString(CultureInfo.InvariantCulture);
                case CuoFileKind.Tree:
                    return $"{graphic}=0";
                case CuoFileKind.Seasons:
                    return $"winter,{(land ? "landtile" : "static")},0x{graphic:X4},0x{graphic:X4}";
                case CuoFileKind.Lights:
                    return $"0x{graphic:X4} 1";
                default:
                    return null;
            }
        }

        #region Parsing

        private static bool IsCommentLine(CuoFileKind kind, string trimmed)
        {
            if (trimmed.Length == 0)
            {
                return true;
            }

            return kind == CuoFileKind.Seasons
                ? trimmed.StartsWith("#", StringComparison.Ordinal) || trimmed.StartsWith("//", StringComparison.Ordinal)
                : trimmed.StartsWith("#", StringComparison.Ordinal) || trimmed.StartsWith(";", StringComparison.Ordinal);
        }

        private static string[] Tokens(CuoFileKind kind, string line)
        {
            if (kind == CuoFileKind.Seasons)
            {
                return line.Split(',');
            }

            char[] sep = kind == CuoFileKind.Tree ? new[] { ' ', '\t', ',', '=' } : new[] { ' ', '\t', ',' };

            // an inline comment ends the line
            int hash = line.IndexOfAny(new[] { '#', ';' });
            if (hash >= 0)
            {
                line = line.Substring(0, hash);
            }

            return line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool Dec(string s, out int value) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        private static bool DecOrHex(string s, out int value)
        {
            s = s ?? string.Empty;
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }

            return Dec(s, out value);
        }

        public static CuoLineInfo ParseLine(CuoFileKind kind, string line)
        {
            var info = new CuoLineInfo();
            string trimmed = (line ?? string.Empty).Trim();
            if (IsCommentLine(kind, trimmed))
            {
                info.IsComment = true;
                return info;
            }

            string[] t = Tokens(kind, kind == CuoFileKind.Seasons ? line ?? string.Empty : trimmed);
            if (t.Length == 0)
            {
                info.IsComment = true;
                return info;
            }

            switch (kind)
            {
                case CuoFileKind.Chair:
                    {
                        if (t.Length < 7)
                        {
                            info.Error = $"7 values needed (GRAPHIC,D1,D2,D3,D4,OFFSET_Y,MIRROR_OFFSET_Y), found {t.Length}";
                        }

                        if (!Dec(t[0], out int g) || g < 0 || g > 0xFFFF)
                        {
                            info.Error = $"\"{t[0]}\" is no decimal graphic";
                            break;
                        }

                        info.Items.Add(g);
                        info.Key = g;
                        for (int i = 1; i < Math.Min(5, t.Length); i++)
                        {
                            if (!Dec(t[i], out int d) || !(d == -1 || d == 0 || d == 2 || d == 4 || d == 6))
                            {
                                info.Error = $"direction \"{t[i]}\" must be 0, 2, 4, 6 or -1";
                            }
                        }

                        for (int i = 5; i < Math.Min(7, t.Length); i++)
                        {
                            if (!Dec(t[i], out _))
                            {
                                info.Error = $"offset \"{t[i]}\" is no number";
                            }
                        }

                        break;
                    }

                case CuoFileKind.Cave:
                case CuoFileKind.Vegetation:
                    {
                        if (!Dec(t[0], out int g) || g < 0 || g > 0xFFFF)
                        {
                            info.Error = $"\"{t[0]}\" is no decimal graphic" + (t[0].StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? " (hex is not read by ClassicUO here)" : string.Empty);
                            break;
                        }

                        info.Items.Add(g);
                        info.Key = g;
                        break;
                    }

                case CuoFileKind.Tree:
                    {
                        if (t.Length < 2)
                        {
                            info.Error = "format GRAPHIC=FLAG";
                        }

                        if (!Dec(t[0], out int g) || g < 0 || g > 0xFFFF)
                        {
                            info.Error = $"\"{t[0]}\" is no decimal graphic";
                            break;
                        }

                        info.Items.Add(g);
                        info.Key = g;
                        if (t.Length >= 2 && (!Dec(t[1], out int f) || f < 0 || f > 255))
                        {
                            info.Error = $"flag \"{t[1]}\" must be 0 or 1";
                        }

                        break;
                    }

                case CuoFileKind.Containers:
                    {
                        if (t.Length < 7)
                        {
                            info.Error = $"at least 7 values needed, found {t.Length}";
                        }

                        for (int i = 0; i < t.Length && i < 10; i++)
                        {
                            if (!Dec(t[i], out _))
                            {
                                info.Error = $"\"{t[i]}\" is no decimal number";
                            }
                        }

                        if (Dec(t[0], out int gump))
                        {
                            info.Gump = gump;
                            info.Key = gump;
                        }

                        break;
                    }

                case CuoFileKind.Seasons:
                    {
                        if (t.Length < 4)
                        {
                            info.Error = "format SEASON,static|landtile,ORIGINAL,REPLACEMENT";
                            break;
                        }

                        string season = t[0].ToLowerInvariant();
                        if (season != "spring" && season != "summer" && season != "fall" && season != "winter" && season != "desolation")
                        {
                            info.Error = $"unknown season \"{t[0]}\" (spring, summer, fall, winter, desolation)";
                        }

                        if (t.Any(x => x != x.Trim()))
                        {
                            info.Error = "no spaces allowed (ClassicUO does not trim the values)";
                        }

                        bool isStatic = t[1].StartsWith("static", StringComparison.OrdinalIgnoreCase);
                        if (!isStatic && !t[1].StartsWith("landtile", StringComparison.OrdinalIgnoreCase))
                        {
                            info.Error = $"\"{t[1]}\" must be static or landtile";
                        }

                        for (int i = 2; i < 4; i++)
                        {
                            if (!DecOrHex(t[i].Trim(), out int g))
                            {
                                info.Error = $"\"{t[i]}\" is no graphic";
                                continue;
                            }

                            (isStatic ? info.Items : info.Lands).Add(g);
                        }

                        break;
                    }

                case CuoFileKind.Lights:
                    {
                        if (t.Length < 2)
                        {
                            info.Error = "format ITEM_ID SHADER or ITEM_ID H<hue>";
                        }

                        if (!DecOrHex(t[0], out int g) || g < 0 || g > 0xFFFF)
                        {
                            info.Error = $"\"{t[0]}\" is no graphic";
                            break;
                        }

                        info.Items.Add(g);
                        info.Key = g;
                        if (t.Length >= 2)
                        {
                            string v = t[1];
                            bool hue = v.StartsWith("H", StringComparison.OrdinalIgnoreCase);
                            if (!Dec(hue ? v.Substring(1) : v, out int n) || n < 0 || n > 0xFFFF)
                            {
                                info.Error = $"\"{v}\" must be a shader id or H<hue>";
                            }
                        }

                        break;
                    }

                case CuoFileKind.LightShaders:
                    {
                        if (t.Length < 3)
                        {
                            info.Error = "format ID RGB R_CURVE [G_CURVE B_CURVE]";
                        }

                        if (!Dec(t[0], out int id) || id < 0)
                        {
                            info.Error = $"\"{t[0]}\" is no decimal id";
                        }
                        else
                        {
                            info.Key = id;
                            if (id > 63)
                            {
                                info.Error = "the id must not be larger than 63";
                            }
                        }

                        if (t.Length > 1 && !uint.TryParse(t[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
                        {
                            info.Error = $"\"{t[1]}\" is no hex colour (e.g. FF7F00)";
                        }

                        string[] curves = { "Standard", "A", "B", "C", "D", "E" };
                        for (int i = 2; i < Math.Min(5, t.Length); i++)
                        {
                            if (!curves.Contains(t[i]))
                            {
                                info.Error = $"curve \"{t[i]}\" unknown (Standard, A, B, C, D, E – case sensitive)";
                            }
                        }

                        break;
                    }
            }

            return info;
        }

        /// <summary>
        /// Checks the whole text. itemExists / landExists / gumpExists may be null (then not checked).
        /// </summary>
        public static List<CuoProblem> Check(CuoFileKind kind, string text, Func<int, bool> itemExists, Func<int, bool> landExists, Func<int, bool> gumpExists)
        {
            var problems = new List<CuoProblem>();
            var seen = new Dictionary<string, int>();
            string[] lines = (text ?? string.Empty).Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                CuoLineInfo info = ParseLine(kind, lines[i]);
                if (info.IsComment)
                {
                    continue;
                }

                if (info.Error != null)
                {
                    problems.Add(new CuoProblem { Line = i + 1, Message = info.Error });
                }

                foreach (int g in info.Items)
                {
                    if (itemExists != null && !itemExists(g))
                    {
                        problems.Add(new CuoProblem { Line = i + 1, Message = $"item 0x{g:X4} ({g}) has no art" });
                    }
                }

                foreach (int g in info.Lands)
                {
                    if (landExists != null && !landExists(g))
                    {
                        problems.Add(new CuoProblem { Line = i + 1, Message = $"land tile 0x{g:X4} ({g}) has no art" });
                    }
                }

                if (info.Gump.HasValue && gumpExists != null && !gumpExists(info.Gump.Value))
                {
                    problems.Add(new CuoProblem { Line = i + 1, Message = $"gump 0x{info.Gump:X4} ({info.Gump}) does not exist" });
                }

                // duplicates (seasons: the same original in the same season)
                string key = kind == CuoFileKind.Seasons
                    ? (lines[i].Split(',').FirstOrDefault() ?? string.Empty).ToLowerInvariant() + "|" + string.Join(",", info.Items.Take(1).Concat(info.Lands.Take(1)))
                    : info.Key?.ToString(CultureInfo.InvariantCulture);
                if (key != null && info.Error == null)
                {
                    if (seen.TryGetValue(key, out int first))
                    {
                        problems.Add(new CuoProblem { Line = i + 1, Message = $"duplicate entry (also in line {first}) – ClassicUO uses only one" });
                    }
                    else
                    {
                        seen[key] = i + 1;
                    }
                }
            }

            return problems;
        }

        /// <summary>all keys (graphics) that are in the file already</summary>
        public static HashSet<int> Keys(CuoFileKind kind, string text)
        {
            var keys = new HashSet<int>();
            foreach (string line in (text ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
            {
                CuoLineInfo info = ParseLine(kind, line);
                if (!info.IsComment && info.Key.HasValue)
                {
                    keys.Add(info.Key.Value);
                }
            }

            return keys;
        }

        #endregion
    }
}
