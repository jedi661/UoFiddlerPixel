/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.MapViewer.Editor.Transitions;

namespace UoFiddler.Controls.MapViewer.Editor.Tools
{
    public enum BrushMode
    {
        /// <summary>random variants of a list of land tiles</summary>
        Textures,

        /// <summary>a copied piece of a map (land, heights, statics) that is repeated</summary>
        Pattern
    }

    public enum PatternHeight
    {
        /// <summary>the ground keeps its height</summary>
        KeepGround,

        /// <summary>heights of the pattern as copied</summary>
        PatternAbsolute,

        /// <summary>heights of the pattern, moved to the ground height where the stroke starts</summary>
        PatternRelative
    }

    public enum PatternAnchor
    {
        /// <summary>the pattern starts at the cell where the stroke starts (one click = the whole pattern)</summary>
        StrokeStart,

        /// <summary>the pattern is repeated on a fixed grid of the map (seamless over several strokes)</summary>
        MapGrid
    }

    /// <summary>
    /// What the paint brush paints: textures (random variants) or a pattern (cells with fixed tiles, heights, statics).
    /// </summary>
    public sealed class BrushPreset
    {
        private BrushPattern _pattern;

        public string Name { get; set; } = "New brush";

        /// <summary>group in the brush list, e.g. "Floors", "Coast"</summary>
        public string Category { get; set; } = string.Empty;

        public BrushMode Mode { get; set; } = BrushMode.Textures;
        public List<ushort> Tiles { get; private set; } = new List<ushort>();
        public int Size { get; set; } = 3;
        public BrushShape Shape { get; set; } = BrushShape.Square;

        public bool PaintLand { get; set; } = true;
        public bool PaintStatics { get; set; } = true;

        /// <summary>statics on the painted cells are removed before the statics of the pattern are set</summary>
        public bool ReplaceStatics { get; set; } = true;

        public PatternHeight Height { get; set; } = PatternHeight.KeepGround;
        public PatternAnchor Anchor { get; set; } = PatternAnchor.StrokeStart;

        /// <summary>changes when the pattern or the tiles change (for cached preview images)</summary>
        public int Version { get; private set; }

        public BrushPattern Pattern
        {
            get => _pattern;
            set
            {
                _pattern = value;
                Touch();
            }
        }

        public bool HasPattern => _pattern != null;

        public void Touch()
        {
            Version++;
            _pattern?.StaticsChanged();
        }

        public IReadOnlyList<ClipStatic> StaticsAt(int x, int y) => _pattern?.StaticsAt(x, y) ?? (IReadOnlyList<ClipStatic>)Array.Empty<ClipStatic>();

        public BrushPreset Clone()
        {
            var copy = (BrushPreset)MemberwiseClone();
            copy.Tiles = new List<ushort>(Tiles);
            copy._pattern = _pattern?.Clone();
            return copy;
        }

        public string DisplayName => string.IsNullOrEmpty(Category) ? Name : $"{Category} / {Name}";

        public override string ToString() => DisplayName;

        public string Describe()
        {
            if (Mode == BrushMode.Textures)
            {
                return Tiles.Count == 0 ? "No textures - click tiles in the palette" : $"{Tiles.Count} texture(s): " + string.Join(" ", Tiles.Take(6).Select(t => $"0x{t:X}")) + (Tiles.Count > 6 ? " ..." : string.Empty);
            }

            return !HasPattern
                ? "No pattern - \"Edit...\", \"Copy from map\" or \"Clip -> brush\""
                : $"Pattern {_pattern.Width} x {_pattern.Height}, {_pattern.FilledCells} cells set, z {_pattern.MinZ}..{_pattern.MaxZ}, {_pattern.Statics.Count} statics";
        }
    }

    /// <summary>
    /// Saved brushes (default: <c>&lt;Output&gt;\UoFiddler_Brushes\Brushes.xml</c>)
    /// </summary>
    public sealed class BrushLibrary
    {
        public const string FolderName = "UoFiddler_Brushes";

        public string FileName { get; set; }

        public List<BrushPreset> Brushes { get; } = new List<BrushPreset>();

        private static BrushLibrary _default;

        public static BrushLibrary Default
        {
            get
            {
                if (_default == null)
                {
                    string baseDir = !string.IsNullOrEmpty(Options.OutputPath) && Directory.Exists(Options.OutputPath)
                        ? Options.OutputPath
                        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UoFiddler");
                    _default = new BrushLibrary { FileName = Path.Combine(baseDir, FolderName, "Brushes.xml") };
                    try
                    {
                        if (File.Exists(_default.FileName))
                        {
                            _default.Load(_default.FileName);
                        }
                    }
                    catch (Exception)
                    {
                        // broken file: start empty, the file is replaced only when saved
                    }
                }

                return _default;
            }
        }

        public void Load(string fileName)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);
            Brushes.Clear();
            foreach (XmlElement e in doc.DocumentElement?.SelectNodes("Brush").OfType<XmlElement>() ?? Enumerable.Empty<XmlElement>())
            {
                var b = new BrushPreset
                {
                    Name = e.GetAttribute("name"),
                    Category = e.GetAttribute("category"),
                    Mode = Enum.TryParse(e.GetAttribute("mode"), out BrushMode m) ? m : BrushMode.Textures,
                    Size = int.TryParse(e.GetAttribute("size"), out int size) ? size : 3,
                    Shape = Enum.TryParse(e.GetAttribute("shape"), out BrushShape sh) ? sh : BrushShape.Square,
                    PaintLand = e.GetAttribute("land") != "0",
                    PaintStatics = e.GetAttribute("statics") != "0",
                    ReplaceStatics = e.GetAttribute("replaceStatics") != "0",
                    Height = Enum.TryParse(e.GetAttribute("height"), out PatternHeight h) ? h : PatternHeight.KeepGround,
                    Anchor = Enum.TryParse(e.GetAttribute("anchor"), out PatternAnchor a) ? a : PatternAnchor.StrokeStart
                };
                b.Tiles.AddRange(TransitionSet.ParseIds(e.GetAttribute("tiles")));

                if (e.SelectSingleNode("Pattern") is XmlElement p &&
                    int.TryParse(p.GetAttribute("w"), out int w) && int.TryParse(p.GetAttribute("h"), out int hh) && w > 0 && hh > 0)
                {
                    var pattern = new BrushPattern(w, hh) { SourceName = p.GetAttribute("source") };
                    string[] cells = p.SelectSingleNode("Land")?.InnerText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    for (int i = 0; i < cells.Length && i < w * hh; i++)
                    {
                        // "16|17:0" = variants 0x16, 0x17 at z 0, "-:0" = empty cell
                        string[] parts = cells[i].Split(':');
                        int x = i % w, y = i / w;
                        if (parts[0] != "-")
                        {
                            pattern.SetIds(x, y, parts[0].Split('|').Select(v => ushort.Parse(v, NumberStyles.HexNumber, CultureInfo.InvariantCulture)));
                        }

                        pattern.SetZ(x, y, parts.Length > 1 ? sbyte.Parse(parts[1], CultureInfo.InvariantCulture) : (sbyte)0);
                    }

                    foreach (string st in (p.SelectSingleNode("Statics")?.InnerText ?? string.Empty).Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] v = st.Trim().Split(',');
                        if (v.Length >= 5)
                        {
                            pattern.Statics.Add(new ClipStatic(
                                int.Parse(v[0], CultureInfo.InvariantCulture), int.Parse(v[1], CultureInfo.InvariantCulture),
                                ushort.Parse(v[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                                sbyte.Parse(v[3], CultureInfo.InvariantCulture), short.Parse(v[4], CultureInfo.InvariantCulture)));
                        }
                    }

                    b.Pattern = pattern;
                }

                Brushes.Add(b);
            }

            FileName = fileName;
        }

        public void Save(string fileName = null)
        {
            fileName ??= FileName;
            Directory.CreateDirectory(Path.GetDirectoryName(fileName) ?? ".");
            var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false) };
            string temp = fileName + ".tmp";
            using (XmlWriter w = XmlWriter.Create(temp, settings))
            {
                w.WriteStartElement("Brushes");
                w.WriteAttributeString("version", "1");
                foreach (BrushPreset b in Brushes)
                {
                    w.WriteStartElement("Brush");
                    w.WriteAttributeString("name", b.Name);
                    w.WriteAttributeString("category", b.Category ?? string.Empty);
                    w.WriteAttributeString("mode", b.Mode.ToString());
                    w.WriteAttributeString("size", b.Size.ToString(CultureInfo.InvariantCulture));
                    w.WriteAttributeString("shape", b.Shape.ToString());
                    w.WriteAttributeString("land", b.PaintLand ? "1" : "0");
                    w.WriteAttributeString("statics", b.PaintStatics ? "1" : "0");
                    w.WriteAttributeString("replaceStatics", b.ReplaceStatics ? "1" : "0");
                    w.WriteAttributeString("height", b.Height.ToString());
                    w.WriteAttributeString("anchor", b.Anchor.ToString());
                    w.WriteAttributeString("tiles", TransitionSet.FormatIds(b.Tiles));
                    if (b.HasPattern)
                    {
                        BrushPattern p = b.Pattern;
                        w.WriteStartElement("Pattern");
                        w.WriteAttributeString("w", p.Width.ToString(CultureInfo.InvariantCulture));
                        w.WriteAttributeString("h", p.Height.ToString(CultureInfo.InvariantCulture));
                        w.WriteAttributeString("source", p.SourceName ?? string.Empty);
                        var land = new StringBuilder();
                        for (int y = 0; y < p.Height; y++)
                        {
                            for (int x = 0; x < p.Width; x++)
                            {
                                IReadOnlyList<ushort> ids = p.GetIds(x, y);
                                land.Append(ids.Count == 0 ? "-" : string.Join("|", ids.Select(i => i.ToString("X4", CultureInfo.InvariantCulture))))
                                    .Append(':').Append(p.GetZ(x, y).ToString(CultureInfo.InvariantCulture)).Append(x == p.Width - 1 ? "\n" : " ");
                            }
                        }

                        w.WriteElementString("Land", land.ToString());
                        w.WriteElementString("Statics", string.Join(";", p.Statics.Select(s =>
                            string.Format(CultureInfo.InvariantCulture, "{0},{1},{2:X4},{3},{4}", s.X, s.Y, s.Id, s.Z, s.Hue))));
                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                }

                w.WriteEndElement();
            }

            File.Move(temp, fileName, true);
            FileName = fileName;
        }
    }
}
