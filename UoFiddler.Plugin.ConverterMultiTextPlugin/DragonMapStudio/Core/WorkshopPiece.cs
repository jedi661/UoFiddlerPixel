/***************************************************************************
 *
 *  * Dragon Map Studio – saved pieces of the dungeon workshop (dungeons, cellars, rooms)
 *  *
 *  *  A piece is land (id + z) and statics of a rectangle, relative to its top left corner and to its ground
 *  *  height. Saved as JSON by category; loaded into the workshop on the chosen level.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public sealed class PieceStatic
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public ushort Id { get; set; }
        public ushort Hue { get; set; }
    }

    public sealed class WorkshopPiece
    {
        public string Name { get; set; } = "Dungeon";
        public string Category { get; set; } = "Dungeon";
        public string Source { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>Ground height of the source: land z and static z are stored relative to it.</summary>
        public int Ground { get; set; }

        public ushort[] Land { get; set; } = Array.Empty<ushort>();
        public int[] LandZ { get; set; } = Array.Empty<int>();
        public List<PieceStatic> Statics { get; set; } = new List<PieceStatic>();

        public override string ToString() => Category + " / " + Name + $" ({Width}×{Height})";

        /// <summary>Cuts a rectangle out of a map.</summary>
        public static WorkshopPiece From(MapData map, Rectangle r, string name, string category)
        {
            var p = new WorkshopPiece
            {
                Name = name, Category = category, Width = r.Width, Height = r.Height, Ground = Workshop.GroundLevel(map, r),
                Land = new ushort[r.Width * r.Height], LandZ = new int[r.Width * r.Height], Source = $"{map.Source} ({r.X}, {r.Y}, {r.Width}×{r.Height})"
            };
            for (int y = 0; y < r.Height; y++)
            {
                for (int x = 0; x < r.Width; x++)
                {
                    p.Land[y * r.Width + x] = map.LandId(r.X + x, r.Y + y);
                    p.LandZ[y * r.Width + x] = map.LandZ(r.X + x, r.Y + y) - p.Ground;
                    foreach (StaticItem s in map.StaticsAt(r.X + x, r.Y + y))
                    {
                        p.Statics.Add(new PieceStatic { X = x, Y = y, Z = s.Z - p.Ground, Id = s.Id, Hue = s.Hue });
                    }
                }
            }
            return p;
        }

        /// <summary>
        /// Writes the piece into a map at <paramref name="at"/> with its ground on <paramref name="level"/>: land and statics of
        /// the covered cells are replaced. Inside a land stroke of the caller (BeginLand / EndLand). Returns the covered area.
        /// </summary>
        public Rectangle WriteTo(MapData map, Point at, int level)
        {
            var statics = new List<StaticChange>();
            var byCell = Statics.GroupBy(s => s.Y * Width + s.X).ToDictionary(g => g.Key, g => g.ToList());
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int tx = at.X + x, ty = at.Y + y;
                    if (!map.InBounds(tx, ty))
                    {
                        continue;
                    }
                    statics.AddRange(map.StaticsAt(tx, ty).ToList().Select(MapData.Remove));
                    int i = y * Width + x;
                    map.SetLand(tx, ty, Land[i], Math.Clamp(LandZ[i] + level, -128, 127));
                    if (byCell.TryGetValue(i, out List<PieceStatic> list))
                    {
                        foreach (PieceStatic s in list)
                        {
                            statics.Add(MapData.Add(new StaticItem { Id = s.Id, Hue = s.Hue, X = tx, Y = ty, Z = Math.Clamp(s.Z + level, -128, 127) }));
                        }
                    }
                }
            }
            map.ApplyStatics(statics);
            return new Rectangle(at.X, at.Y, Math.Min(Width, map.Width - at.X), Math.Min(Height, map.Height - at.Y));
        }

        // ------------------------------------------------------------------ files

        public static string Folder => Path.Combine(StudioSettings.SettingsFolder, "Werkstatt");

        private static string Safe(string s) => string.Concat((s ?? "x").Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)).Trim();

        public string Save()
        {
            string dir = Path.Combine(Folder, Safe(string.IsNullOrWhiteSpace(Category) ? "Dungeon" : Category));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, Safe(Name) + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(this));
            return path;
        }

        /// <summary>All saved pieces (only name, category, size are needed for the list – the file is read again on load).</summary>
        public static List<(string path, string title)> List()
        {
            var list = new List<(string, string)>();
            if (!Directory.Exists(Folder))
            {
                return list;
            }
            foreach (string f in Directory.GetFiles(Folder, "*.json", SearchOption.AllDirectories).OrderBy(f => f))
            {
                list.Add((f, Path.GetFileName(Path.GetDirectoryName(f)) + " / " + Path.GetFileNameWithoutExtension(f)));
            }
            return list;
        }

        public static WorkshopPiece Load(string path) => JsonSerializer.Deserialize<WorkshopPiece>(File.ReadAllText(path));
    }
}
