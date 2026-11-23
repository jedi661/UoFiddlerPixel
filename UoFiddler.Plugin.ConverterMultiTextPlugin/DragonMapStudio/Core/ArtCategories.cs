/***************************************************************************
 *
 *  * Dragon Map Studio – catalog categories for statics, land tiles and multis
 *  *
 *  *  Own categories (e.g. "Dungeon Wände", "Brücke Holz") are saved in katalog_kategorien.json next to the settings.
 *  *  Built-in categories come from the tiledata flags and names (walls, floors, stairs, bridge, plants, …).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ultima;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum ArtKind
    {
        Static,
        Land,
        Multi
    }

    public sealed class ArtCategory
    {
        public ArtKind Kind { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>Ids as hex, separated by spaces.</summary>
        public string Ids { get; set; } = string.Empty;

        public List<int> IdList() => HexList.Parse(Ids).Select(i => (int)i).Distinct().OrderBy(i => i).ToList();

        public void SetIds(IEnumerable<int> ids) => Ids = string.Join(" ", ids.Distinct().OrderBy(i => i).Select(i => i.ToString("x4")));

        public override string ToString() => Name;
    }

    public static class ArtCategories
    {
        private static List<ArtCategory> _own;

        public static string FilePath => Path.Combine(StudioSettings.SettingsFolder, "katalog_kategorien.json");

        /// <summary>Own categories (loaded once).</summary>
        public static List<ArtCategory> Own
        {
            get
            {
                if (_own == null)
                {
                    _own = new List<ArtCategory>();
                    try
                    {
                        if (File.Exists(FilePath))
                        {
                            _own = JsonSerializer.Deserialize<List<ArtCategory>>(File.ReadAllText(FilePath)) ?? new List<ArtCategory>();
                        }
                    }
                    catch (Exception)
                    {
                        _own = new List<ArtCategory>();
                    }
                    if (_own.Count == 0)
                    {
                        // a start: the pieces of the classic dungeon of the map0 log
                        _own.Add(new ArtCategory { Kind = ArtKind.Static, Name = "Dungeon (map0)", Ids = "0241 0242 0243 053d 053e 0a07 0a08 0a0c 0a0d 07a3 07a4 07a5 07a6 07a7 10ee 10ef 10f0 10f1 10f2 10f3 122a 122b 122c 122d 122e" });
                        _own.Add(new ArtCategory { Kind = ArtKind.Land, Name = "Dungeon / Höhle", Ids = "0002 0244 0245 0246 0247 0248 0249" });
                    }
                }
                return _own;
            }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, JsonSerializer.Serialize(Own, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                // read-only folder – the categories stay for this session
            }
        }

        public static ArtCategory Get(ArtKind kind, string name) => Own.FirstOrDefault(c => c.Kind == kind && c.Name == name);

        public static void AddTo(ArtKind kind, string name, IEnumerable<int> ids)
        {
            ArtCategory c = Get(kind, name);
            if (c == null)
            {
                c = new ArtCategory { Kind = kind, Name = name };
                Own.Add(c);
            }
            c.SetIds(c.IdList().Concat(ids));
            Save();
        }

        public static void RemoveFrom(ArtKind kind, string name, IEnumerable<int> ids)
        {
            ArtCategory c = Get(kind, name);
            if (c == null)
            {
                return;
            }
            var del = new HashSet<int>(ids);
            c.SetIds(c.IdList().Where(i => !del.Contains(i)));
            Save();
        }

        public static void Delete(ArtKind kind, string name)
        {
            Own.RemoveAll(c => c.Kind == kind && c.Name == name);
            Save();
        }

        // ------------------------------------------------------------------ built-in categories

        public static readonly string[] StaticAuto =
        {
            "Wände", "Böden", "Treppen", "Brücke (Bretter)", "Geländer / Zaun", "Pfeiler / Säulen", "Türen", "Dach", "Pflanzen / Bäume",
            "Wasser", "Licht", "Dungeon-Deko", "Möbel"
        };

        public static readonly string[] LandAuto = { "Wasser", "Höhle / Dungeon", "Fels / Berg", "Gras", "Sand", "Erde / Weg", "Schnee", "Lava", "Leere / NoDraw" };

        private static bool Has(string name, params string[] words) => words.Any(name.Contains);

        /// <summary>The item fits a built-in category (tiledata flags and name).</summary>
        public static bool StaticFits(int id, string category)
        {
            ItemData d;
            try
            {
                d = TileData.ItemTable[id];
            }
            catch (Exception)
            {
                return false;
            }
            string n = (d.Name ?? string.Empty).ToLowerInvariant();
            TileFlag f = d.Flags;
            // placeholders of the tiledata are no pieces
            if (id <= 2 || n.Length == 0 || n.Contains("missing_name") || n == "nodraw" || n.StartsWith("void"))
            {
                return false;
            }
            switch (category)
            {
                case "Wände":
                    return (f & TileFlag.Wall) != 0;
                case "Böden":
                    return (f & TileFlag.Surface) != 0 && d.Height <= 3 && !Has(n, "stair", "step");
                case "Treppen":
                    return Has(n, "stair", "steps") || (f & (TileFlag.StairBack | TileFlag.StairRight)) != 0;
                case "Brücke (Bretter)":
                    return (f & TileFlag.Surface) != 0 && Has(n, "bridge", "plank", "board", "dock", "pier");
                case "Geländer / Zaun":
                    return Has(n, "railing", "rail", "fence", "banister", "balustrade");
                case "Pfeiler / Säulen":
                    return Has(n, "pillar", "column", "support", "pylon", "post");
                case "Türen":
                    return (f & TileFlag.Door) != 0;
                case "Dach":
                    return (f & TileFlag.Roof) != 0;
                case "Pflanzen / Bäume":
                    return (f & TileFlag.Foliage) != 0 || Has(n, "tree", "bush", "plant", "grass", "fern", "flower", "weed", "mushroom", "vine");
                case "Wasser":
                    return (f & TileFlag.Wet) != 0;
                case "Licht":
                    return !Has(n, "window") && ((f & TileFlag.LightSource) != 0 || Has(n, "lamp", "torch", "candle", "lantern", "brazier"));
                case "Dungeon-Deko":
                    return Has(n, "bone", "blood", "skull", "skeleton", "debris", "rubble", "garbage", "web", "cobweb", "chain", "spike", "remains", "torch", "slime");
                case "Möbel":
                    return Has(n, "table", "chair", "bench", "bed", "chest", "shelf", "barrel", "crate", "cabinet", "stool", "throne");
            }
            return false;
        }

        public static bool LandFits(int id, string category)
        {
            LandData d;
            try
            {
                d = TileData.LandTable[id & 0x3FFF];
            }
            catch (Exception)
            {
                return false;
            }
            string n = (d.Name ?? string.Empty).ToLowerInvariant();
            switch (category)
            {
                case "Wasser":
                    return (d.Flags & TileFlag.Wet) != 0 || Has(n, "water");
                case "Höhle / Dungeon":
                    return Has(n, "cave", "dungeon") || (id >= 0x0244 && id <= 0x0249);
                case "Fels / Berg":
                    return Has(n, "rock", "mountain", "stone");
                case "Gras":
                    return Has(n, "grass");
                case "Sand":
                    return Has(n, "sand", "beach");
                case "Erde / Weg":
                    return Has(n, "dirt", "mud", "path", "cobble", "road", "furrow");
                case "Schnee":
                    return Has(n, "snow", "ice");
                case "Lava":
                    return Has(n, "lava");
                case "Leere / NoDraw":
                    return id <= 0x0002 || id == 0x0244 || Has(n, "void", "nodraw", "no draw");
            }
            return false;
        }
    }
}
