/***************************************************************************
 *
 *  * Dragon Map Studio – cave presets: named sets of floor tiles for the tool "Höhle" (own textures can be added) and
 *  *  saved cave entrances (land + statics, anchored on the ground in front) for the tool "Eingang".
 *  *  Entrances: SettingsFolder\Hoehleneingaenge\*.dmsv.json, shipped ones in DragonData\Hoehleneingaenge.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>A named set of cave floor tiles.</summary>
    public sealed class CaveTileSet
    {
        public string Name { get; set; } = "Höhlenboden";

        /// <summary>Land tiles (hex, space separated).</summary>
        public string Ids { get; set; } = "0245 0246 0247 0248 0249";

        public override string ToString() => Name;

        public static List<CaveTileSet> Defaults() => new List<CaveTileSet>
        {
            new CaveTileSet { Name = "Höhlenboden (Original 0245–0249)", Ids = "0245 0246 0247 0248 0249" },
            new CaveTileSet { Name = "Höhlenboden dunkel (0244)", Ids = "0244" },
            new CaveTileSet { Name = "Erde (0071–0078)", Ids = "0071 0072 0073 0074 0075 0076 0077 0078" },
            new CaveTileSet { Name = "Unsichtbar / begehbar (0002)", Ids = "0002" }
        };
    }

    public static class CaveEntrancePresets
    {
        public static string Folder => Path.Combine(StudioSettings.SettingsFolder, "Hoehleneingaenge");

        public static string ShippedFolder => StudioPaths.DataFolder == null ? null : Path.Combine(StudioPaths.DataFolder, "Hoehleneingaenge");

        private static List<MapTemplate> _cache;

        /// <summary>Shipped and own entrances (own ones with the same name win), sorted by name.</summary>
        public static List<MapTemplate> All
        {
            get
            {
                if (_cache == null)
                {
                    var byName = new Dictionary<string, MapTemplate>(StringComparer.OrdinalIgnoreCase);
                    foreach (string folder in new[] { ShippedFolder, Folder })
                    {
                        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                        {
                            continue;
                        }
                        foreach (string f in Directory.GetFiles(folder, "*" + TemplateIO.DragonExtension))
                        {
                            try
                            {
                                MapTemplate t = TemplateIO.LoadJson(f);
                                t.File = f;
                                byName[t.Name] = t;
                            }
                            catch (Exception)
                            {
                                // broken file
                            }
                        }
                    }
                    _cache = byName.Values.OrderBy(t => t.Name).ToList();
                }
                return _cache;
            }
        }

        public static void Reload() => _cache = null;

        public static string Save(MapTemplate t)
        {
            string path = TemplateIO.Save(t, Folder, TemplateFormat.Dragon);
            Reload();
            return path;
        }

        public static bool Delete(MapTemplate t)
        {
            if (t?.File == null || !t.File.StartsWith(Folder, StringComparison.OrdinalIgnoreCase) || !File.Exists(t.File))
            {
                return false;
            }
            File.Delete(t.File);
            Reload();
            return true;
        }
    }
}
