// /***************************************************************************
//  *
//  * Dragon Map Studio – templates: buildings (statics) and floor patterns (land), library and file formats
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>One static of a template, relative to the anchor (X / Y) and the ground (Z).</summary>
    public sealed class TemplateStatic
    {
        public ushort Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public ushort Hue { get; set; }
    }

    /// <summary>File formats of templates.</summary>
    public enum TemplateFormat
    {
        /// <summary>Own format (.json): land + statics, name, category.</summary>
        Dragon,

        /// <summary>UoFiddler multi text: "0xID x y z flag" per line (also decimal ids of old scripts).</summary>
        FiddlerText,

        /// <summary>UO Architect design (.uoa, binary).</summary>
        UoArchitect,

        /// <summary>World save items (.wsc, SECTION WORLDITEM), absolute positions.</summary>
        Wsc,

        /// <summary>Sphere script (.scp): ITEMDEF with COMPONENT= lines, or WORLDITEM with P=.</summary>
        SphereScript
    }

    /// <summary>
    /// A building (statics) and / or a floor pattern (land tiles with relative heights). The anchor (0, 0) is the
    /// centre; when placed, Z 0 is the land height at the anchor.
    /// </summary>
    public sealed class MapTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public List<TemplateStatic> Statics { get; set; } = new List<TemplateStatic>();

        /// <summary>Land grid: top-left corner relative to the anchor.</summary>
        public int LandX { get; set; }
        public int LandY { get; set; }
        public int LandWidth { get; set; }
        public int LandHeight { get; set; }

        /// <summary>Land tile ids, row by row (null = no land).</summary>
        public ushort[] LandTiles { get; set; }

        /// <summary>Land heights relative to the anchor height.</summary>
        public int[] LandZ { get; set; }

        [JsonIgnore]
        public string File { get; set; } = string.Empty;

        [JsonIgnore]
        public TemplateFormat Format { get; set; }

        [JsonIgnore]
        public bool HasLand => LandTiles != null && LandWidth > 0 && LandHeight > 0 && LandTiles.Length == LandWidth * LandHeight;

        /// <summary>Area covered by statics and land, relative to the anchor.</summary>
        [JsonIgnore]
        public Rectangle Bounds
        {
            get
            {
                Rectangle r = HasLand ? new Rectangle(LandX, LandY, LandWidth, LandHeight) : Rectangle.Empty;
                if (Statics.Count > 0)
                {
                    var s = Rectangle.FromLTRB(Statics.Min(t => t.X), Statics.Min(t => t.Y), Statics.Max(t => t.X) + 1, Statics.Max(t => t.Y) + 1);
                    r = r.IsEmpty ? s : Rectangle.Union(r, s);
                }
                return r;
            }
        }

        public string Summary
        {
            get
            {
                Rectangle b = Bounds;
                string land = HasLand ? $", Boden {LandWidth}×{LandHeight}" : string.Empty;
                return $"{Statics.Count:N0} Statics{land}, {b.Width}×{b.Height} Tiles";
            }
        }

        public override string ToString() => Name;

        /// <summary>Moves the statics so that the centre of their area is the anchor (0, 0).</summary>
        public void CenterStatics(bool lowestToGround)
        {
            if (Statics.Count == 0)
            {
                return;
            }
            int cx = (Statics.Min(s => s.X) + Statics.Max(s => s.X)) / 2;
            int cy = (Statics.Min(s => s.Y) + Statics.Max(s => s.Y)) / 2;
            int z0 = lowestToGround ? Statics.Min(s => s.Z) : 0;
            foreach (TemplateStatic s in Statics)
            {
                s.X -= cx;
                s.Y -= cy;
                s.Z -= z0;
            }
        }
    }

    /// <summary>Reads and writes templates in the formats of UoFiddler, UO Architect, world saves and Sphere scripts.</summary>
    public static class TemplateIO
    {
        public const string DragonExtension = ".dmsv.json";

        private static readonly string[] Extensions = { ".json", ".txt", ".uoa", ".wsc", ".scp" };

        public static bool IsTemplateFile(string path) => Extensions.Contains(Path.GetExtension(path).ToLowerInvariant());

        /// <summary>All templates of a file (a Sphere script can hold several multis).</summary>
        public static List<MapTemplate> Load(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            string name = Path.GetFileNameWithoutExtension(path);
            if (name.EndsWith(".dmsv", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - 5);
            }
            List<MapTemplate> list;
            switch (ext)
            {
                case ".json":
                    list = new List<MapTemplate> { LoadJson(path) };
                    break;
                case ".uoa":
                    list = new List<MapTemplate> { LoadUoa(path) };
                    break;
                case ".wsc":
                    list = new List<MapTemplate> { LoadWsc(path) };
                    break;
                case ".scp":
                    list = LoadScp(path);
                    break;
                default:
                    list = new List<MapTemplate> { LoadText(path) };
                    break;
            }
            foreach (MapTemplate t in list)
            {
                t.File = path;
                if (string.IsNullOrWhiteSpace(t.Name))
                {
                    t.Name = name;
                }
            }
            return list.Where(t => t.Statics.Count > 0 || t.HasLand).ToList();
        }

        // ------------------------------------------------------------------ own format

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true };

        public static MapTemplate LoadJson(string path)
        {
            MapTemplate t = JsonSerializer.Deserialize<MapTemplate>(File.ReadAllText(path), JsonOptions) ?? new MapTemplate();
            t.Statics ??= new List<TemplateStatic>();
            t.Format = TemplateFormat.Dragon;
            return t;
        }

        // ------------------------------------------------------------------ UoFiddler / old script text

        /// <summary>"0xID x y z [flag]" or "ID x y z [flag]" (decimal) per line.</summary>
        public static MapTemplate LoadText(string path)
        {
            var t = new MapTemplate { Format = TemplateFormat.FiddlerText };
            foreach (string raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("#") || line.StartsWith(";"))
                {
                    continue;
                }
                string[] p = line.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length < 4 || !TryId(p[0], false, out ushort id) ||
                    !int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x) ||
                    !int.TryParse(p[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) ||
                    !int.TryParse(p[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int z))
                {
                    continue;
                }
                t.Statics.Add(new TemplateStatic { Id = id, X = x, Y = y, Z = z });
            }
            // most files are centred like the client multis; some count from the top-left corner
            if (t.Statics.Count > 0 && t.Statics.Min(q => q.X) >= 0 && t.Statics.Min(q => q.Y) >= 0)
            {
                t.CenterStatics(false);
            }
            return t;
        }

        /// <summary>Item id: "0x" = hex; Sphere style (leading 0) = hex when <paramref name="sphere"/>; otherwise decimal.</summary>
        private static bool TryId(string s, bool sphere, out ushort id)
        {
            id = 0;
            s = s.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return ushort.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id);
            }
            if (sphere)
            {
                return int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int h) && h >= 0 && (id = (ushort)h) == h;
            }
            return ushort.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }

        public static void SaveText(MapTemplate t, string path)
        {
            var sb = new StringBuilder();
            foreach (TemplateStatic s in t.Statics.OrderBy(s => s.X + s.Y).ThenBy(s => s.Z))
            {
                sb.Append("0x").Append(s.Id.ToString("X", CultureInfo.InvariantCulture)).Append(' ')
                  .Append(s.X.ToString(CultureInfo.InvariantCulture)).Append(' ')
                  .Append(s.Y.ToString(CultureInfo.InvariantCulture)).Append(' ')
                  .Append(s.Z.ToString(CultureInfo.InvariantCulture)).Append(" 1").AppendLine();
            }
            File.WriteAllText(path, sb.ToString());
        }

        // ------------------------------------------------------------------ UO Architect

        /// <summary>
        /// UO Architect .uoa: short version; strings (1 byte "present" + .NET string) – v1: name, category, author,
        /// v2: one more in front; int width, height, width, height, count; then per item short id, x, y, z, level, hue.
        /// </summary>
        public static MapTemplate LoadUoa(string path)
        {
            using var r = new BinaryReader(File.OpenRead(path), Encoding.Latin1);
            short version = r.ReadInt16();
            var strings = new List<string>();
            for (int i = 0; i < (version >= 2 ? 4 : 3); i++)
            {
                strings.Add(r.ReadBoolean() ? r.ReadString() : string.Empty);
            }
            int width = r.ReadInt32(), height = r.ReadInt32();
            r.ReadInt32();
            r.ReadInt32();
            int count = r.ReadInt32();
            var t = new MapTemplate { Format = TemplateFormat.UoArchitect };
            int s0 = version >= 2 ? 1 : 0;
            t.Name = strings[s0];
            t.Category = strings.ElementAtOrDefault(s0 + 1) ?? string.Empty;
            t.Author = strings.ElementAtOrDefault(s0 + 2) ?? string.Empty;
            long left = (r.BaseStream.Length - r.BaseStream.Position) / 12;
            for (int i = 0; i < Math.Min(count, left); i++)
            {
                ushort id = r.ReadUInt16();
                short x = r.ReadInt16(), y = r.ReadInt16(), z = r.ReadInt16();
                r.ReadInt16(); // level
                ushort hue = r.ReadUInt16();
                t.Statics.Add(new TemplateStatic { Id = id, X = x, Y = y, Z = z, Hue = hue });
            }
            // UO Architect counts from the top-left corner; z stays (floors of houses start at 7)
            t.CenterStatics(false);
            return t;
        }

        public static void SaveUoa(MapTemplate t, string path)
        {
            Rectangle b = t.Bounds;
            using var w = new BinaryWriter(File.Create(path), Encoding.Latin1);
            w.Write((short)1);
            foreach (string s in new[] { t.Name, string.IsNullOrEmpty(t.Category) ? "Unassigned" : t.Category, string.IsNullOrEmpty(t.Author) ? "Dragon Map Studio" : t.Author })
            {
                w.Write(true);
                w.Write(s ?? string.Empty);
            }
            w.Write(b.Width);
            w.Write(b.Height);
            w.Write(b.Width);
            w.Write(b.Height);
            w.Write(t.Statics.Count);
            foreach (TemplateStatic s in t.Statics)
            {
                w.Write(s.Id);
                w.Write((short)(s.X - b.X));
                w.Write((short)(s.Y - b.Y));
                w.Write((short)s.Z);
                w.Write((short)1);
                w.Write(s.Hue);
            }
        }

        // ------------------------------------------------------------------ world save (.wsc)

        public static MapTemplate LoadWsc(string path)
        {
            var t = new MapTemplate { Format = TemplateFormat.Wsc };
            Dictionary<string, string> item = null;
            foreach (string raw in File.ReadLines(path, Encoding.Latin1))
            {
                string line = raw.Trim();
                if (line.StartsWith("SECTION WORLDITEM", StringComparison.OrdinalIgnoreCase))
                {
                    item = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else if (line == "}" && item != null)
                {
                    AddWorldItem(t, item);
                    item = null;
                }
                else if (item != null && line.Length > 0 && line != "{")
                {
                    int sp = line.IndexOf(' ');
                    if (sp > 0)
                    {
                        item[line.Substring(0, sp)] = line.Substring(sp + 1).Trim();
                    }
                }
            }
            t.CenterStatics(true);
            return t;
        }

        private static void AddWorldItem(MapTemplate t, Dictionary<string, string> v)
        {
            // items inside containers are no statics
            if (v.TryGetValue("CONT", out string cont) && cont != "-1" && cont.Length > 0)
            {
                return;
            }
            if (v.TryGetValue("ID", out string ids) && ushort.TryParse(ids, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort id) &&
                int.TryParse(v.GetValueOrDefault("X"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int x) &&
                int.TryParse(v.GetValueOrDefault("Y"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int y))
            {
                int.TryParse(v.GetValueOrDefault("Z"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int z);
                ushort.TryParse(v.GetValueOrDefault("COLOR"), NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort hue);
                t.Statics.Add(new TemplateStatic { Id = id, X = x, Y = y, Z = z, Hue = hue });
            }
        }

        // ------------------------------------------------------------------ Sphere script (.scp)

        /// <summary>ITEMDEF sections with COMPONENT=id,x,y,z (one template each), or WORLDITEM sections with P=x,y,z.</summary>
        public static List<MapTemplate> LoadScp(string path)
        {
            var multis = new List<MapTemplate>();
            var world = new MapTemplate { Format = TemplateFormat.SphereScript };
            MapTemplate current = null;
            Dictionary<string, string> item = null;
            void FlushItem()
            {
                if (item == null)
                {
                    return;
                }
                if (item.TryGetValue("ID", out string ids) && TryId(ids, true, out ushort id) && item.TryGetValue("P", out string p))
                {
                    string[] c = p.Split(',');
                    if (c.Length >= 2 && int.TryParse(c[0].Trim(), out int x) && int.TryParse(c[1].Trim(), out int y))
                    {
                        int z = c.Length > 2 && int.TryParse(c[2].Trim(), out int zz) ? zz : 0;
                        ushort hue = 0;
                        if (item.TryGetValue("COLOR", out string col))
                        {
                            TryId(col, true, out hue);
                        }
                        world.Statics.Add(new TemplateStatic { Id = id, X = x, Y = y, Z = z, Hue = hue });
                    }
                }
                item = null;
            }
            foreach (string raw in File.ReadLines(path, Encoding.Latin1))
            {
                string line = raw.Trim();
                int comment = line.IndexOf("//", StringComparison.Ordinal);
                if (comment == 0)
                {
                    continue;
                }
                if (line.StartsWith("[", StringComparison.Ordinal))
                {
                    FlushItem();
                    current = null;
                    string head = line.Trim('[', ']').Trim();
                    if (head.StartsWith("WORLDITEM", StringComparison.OrdinalIgnoreCase))
                    {
                        item = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    else if (head.StartsWith("ITEMDEF", StringComparison.OrdinalIgnoreCase))
                    {
                        current = new MapTemplate { Format = TemplateFormat.SphereScript, Name = head.Substring(7).Trim() };
                        multis.Add(current);
                    }
                    continue;
                }
                int eq = line.IndexOf('=');
                if (eq <= 0)
                {
                    continue;
                }
                string key = line.Substring(0, eq).Trim(), value = line.Substring(eq + 1).Trim();
                if (item != null)
                {
                    item[key] = value;
                }
                else if (current != null)
                {
                    if (key.Equals("COMPONENT", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] c = value.Split(',');
                        if (c.Length >= 4 && TryId(c[0], true, out ushort id) && int.TryParse(c[1].Trim(), out int x) &&
                            int.TryParse(c[2].Trim(), out int y) && int.TryParse(c[3].Trim(), out int z))
                        {
                            current.Statics.Add(new TemplateStatic { Id = id, X = x, Y = y, Z = z });
                        }
                    }
                    else if (key.Equals("NAME", StringComparison.OrdinalIgnoreCase) || key.Equals("DESCRIPTION", StringComparison.OrdinalIgnoreCase))
                    {
                        current.Name = value;
                    }
                    else if (key.Equals("SUBSECTION", StringComparison.OrdinalIgnoreCase))
                    {
                        current.Category = value;
                    }
                }
            }
            FlushItem();
            var list = multis.Where(m => m.Statics.Count > 0).ToList();
            if (world.Statics.Count > 0)
            {
                world.CenterStatics(true);
                list.Add(world);
            }
            return list;
        }

        // ------------------------------------------------------------------ save

        /// <summary>Saves in the given format; returns the file name (extension added).</summary>
        public static string Save(MapTemplate t, string folder, TemplateFormat format)
        {
            Directory.CreateDirectory(folder);
            string safe = string.Concat((t.Name ?? "Vorlage").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();
            if (safe.Length == 0)
            {
                safe = "Vorlage";
            }
            string path;
            switch (format)
            {
                case TemplateFormat.FiddlerText:
                    path = Path.Combine(folder, safe + ".txt");
                    SaveText(t, path);
                    break;
                case TemplateFormat.UoArchitect:
                    path = Path.Combine(folder, safe + ".uoa");
                    SaveUoa(t, path);
                    break;
                default:
                    path = Path.Combine(folder, safe + DragonExtension);
                    File.WriteAllText(path, JsonSerializer.Serialize(t, JsonOptions));
                    break;
            }
            t.File = path;
            t.Format = format;
            return path;
        }
    }

    /// <summary>The template folder: every sub folder is a category (e.g. "UOArchitect/Haus").</summary>
    public sealed class TemplateLibrary
    {
        public string Root { get; private set; } = string.Empty;

        public List<MapTemplate> Items { get; } = new List<MapTemplate>();

        public List<string> Errors { get; } = new List<string>();

        public IEnumerable<string> Categories => Items.Select(t => t.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c, StringComparer.OrdinalIgnoreCase);

        /// <summary>Reads all template files below <paramref name="root"/>; the category is the sub folder.</summary>
        public void Load(string root, IProgress<int> progress = null)
        {
            Root = root ?? string.Empty;
            Items.Clear();
            Errors.Clear();
            if (!Directory.Exists(Root))
            {
                return;
            }
            List<string> files = FindFiles(Root).Where(TemplateIO.IsTemplateFile).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
            for (int i = 0; i < files.Count; i++)
            {
                string f = files[i];
                try
                {
                    string category = Path.GetRelativePath(Root, Path.GetDirectoryName(f) ?? Root).Replace('\\', '/');
                    if (category == ".")
                    {
                        category = string.Empty;
                    }
                    foreach (MapTemplate t in TemplateIO.Load(f))
                    {
                        // the folder is the category; the category stored in the file (UOA, own format) is kept as description
                        if (!string.IsNullOrWhiteSpace(t.Category) && !string.Equals(t.Category, category, StringComparison.OrdinalIgnoreCase) &&
                            string.IsNullOrWhiteSpace(t.Description))
                        {
                            t.Description = t.Category;
                        }
                        t.Category = category;
                        Items.Add(t);
                    }
                }
                catch (Exception ex)
                {
                    Errors.Add($"{f}: {ex.Message}");
                }
                progress?.Report(i * 100 / Math.Max(1, files.Count));
            }
        }

        /// <summary>All files below the folder; a folder that cannot be read is reported and skipped.</summary>
        private List<string> FindFiles(string root)
        {
            var result = new List<string>();
            var stack = new Stack<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                string dir = stack.Pop();
                try
                {
                    result.AddRange(Directory.GetFiles(dir));
                    foreach (string sub in Directory.GetDirectories(dir))
                    {
                        stack.Push(sub);
                    }
                }
                catch (Exception ex)
                {
                    Errors.Add($"{dir}: {ex.Message}");
                }
            }
            return result;
        }

        public IEnumerable<MapTemplate> Find(string category, string text)
        {
            IEnumerable<MapTemplate> q = Items;
            if (!string.IsNullOrEmpty(category))
            {
                q = q.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase) || t.Category.StartsWith(category + "/", StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(text))
            {
                string f = text.Trim();
                q = q.Where(t => t.Name.Contains(f, StringComparison.OrdinalIgnoreCase) || t.Category.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                                 (t.Description ?? string.Empty).Contains(f, StringComparison.OrdinalIgnoreCase) || (t.Author ?? string.Empty).Contains(f, StringComparison.OrdinalIgnoreCase));
            }
            return q;
        }
    }

    /// <summary>Small client-like picture of a template (land diamonds + statics in draw order).</summary>
    public static class TemplatePreview
    {
        public static Bitmap Render(MapTemplate t, int maxWidth, int maxHeight)
        {
            const int half = 22, zStep = 4;
            var parts = new List<(int depth, int z, Bitmap img, float x, float y)>();
            if (t.HasLand)
            {
                for (int y = 0; y < t.LandHeight; y++)
                {
                    for (int x = 0; x < t.LandWidth; x++)
                    {
                        int i = y * t.LandWidth + x, mx = t.LandX + x, my = t.LandY + y;
                        Bitmap img = UoArtCache.Land(t.LandTiles[i]);
                        int z = t.LandZ != null && i < t.LandZ.Length ? t.LandZ[i] : 0;
                        parts.Add(((mx + my) * 2, -1000, img, (mx - my) * half - half, (mx + my) * half - z * zStep));
                    }
                }
            }
            foreach (TemplateStatic s in t.Statics)
            {
                Bitmap img = UoArtCache.Static(s.Id, s.Hue);
                if (img == null)
                {
                    continue;
                }
                parts.Add(((s.X + s.Y) * 2 + 1, s.Z, img, (s.X - s.Y) * half - img.Width / 2f, (s.X + s.Y) * half + 2 * half - s.Z * zStep - img.Height));
            }
            if (parts.Count == 0)
            {
                return null;
            }
            float minX = parts.Min(p => p.x), minY = parts.Min(p => p.y);
            float maxX = parts.Max(p => p.x + (p.img?.Width ?? 44)), maxY = parts.Max(p => p.y + (p.img?.Height ?? 44));
            float w = Math.Max(1, maxX - minX), h = Math.Max(1, maxY - minY);
            float k = Math.Min(1f, Math.Min(maxWidth / w, maxHeight / h));
            var bmp = new Bitmap(Math.Max(1, (int)(w * k)), Math.Max(1, (int)(h * k)));
            using Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.ScaleTransform(k, k);
            g.TranslateTransform(-minX, -minY);
            foreach (var p in parts.OrderBy(p => p.depth).ThenBy(p => p.z))
            {
                if (p.img != null)
                {
                    g.DrawImage(p.img, p.x, p.y, p.img.Width, p.img.Height);
                }
                else
                {
                    using var b = new SolidBrush(Color.FromArgb(120, 120, 120));
                    g.FillPolygon(b, new[] { new PointF(p.x + half, p.y), new PointF(p.x + 2 * half, p.y + half), new PointF(p.x + half, p.y + 2 * half), new PointF(p.x, p.y + half) });
                }
            }
            return bmp;
        }
    }
}
