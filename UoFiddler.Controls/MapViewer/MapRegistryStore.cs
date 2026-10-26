/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Linq;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Loads/saves <see cref="MapRegistry"/> (MapDefinitions.xml in the AppData folder) and keeps
    /// <see cref="Options.MapNames"/> in sync, so older code that uses MapNames[id] keeps working.
    /// </summary>
    public static class MapRegistryStore
    {
        private static bool _loaded;

        public static string FileName => Path.Combine(Options.AppDataPath ?? string.Empty, "MapDefinitions.xml");

        public static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            try
            {
                MapRegistry.Load(FileName, Options.MapNames);
            }
            catch (Exception)
            {
                // broken file: continue with the built-in maps
                MapRegistry.Load(null, Options.MapNames);
            }

            // the profile (FiddlerOptions) is saved on exit and always holds the latest names
            ApplyNamesFromOptions();
            SyncNamesToOptions();
        }

        public static void Save()
        {
            EnsureLoaded();
            SyncNamesToOptions();
            try
            {
                MapRegistry.Save(FileName);
            }
            catch (Exception)
            {
                // ignored, AppData not writeable
            }
        }

        /// <summary>
        /// Writes the names of all maps into <see cref="Options.MapNames"/> (index = map id).
        /// </summary>
        public static void SyncNamesToOptions()
        {
            int maxId = MapRegistry.Definitions.Max(d => d.Id);
            int length = Math.Max(Math.Max(Map.BuiltInCount, maxId + 1), Options.MapNames?.Length ?? 0);

            var names = new string[length];
            for (int i = 0; i < length; i++)
            {
                MapDefinition def = MapRegistry.GetById(i);
                if (def != null)
                {
                    names[i] = def.Name;
                }
                else if (Options.MapNames != null && i < Options.MapNames.Length && !string.IsNullOrEmpty(Options.MapNames[i]))
                {
                    names[i] = Options.MapNames[i];
                }
                else
                {
                    names[i] = $"Map {i}";
                }
            }

            Options.MapNames = names;
        }

        /// <summary>
        /// Takes the names from <see cref="Options.MapNames"/> (e.g. after they were changed in the options dialog).
        /// </summary>
        public static void SyncNamesFromOptions()
        {
            EnsureLoaded();
            if (ApplyNamesFromOptions())
            {
                Save();
                MapRegistry.OnChanged();
            }
        }

        private static bool ApplyNamesFromOptions()
        {
            if (Options.MapNames == null)
            {
                return false;
            }

            bool changed = false;
            foreach (MapDefinition def in MapRegistry.Definitions)
            {
                if (def.Id < Options.MapNames.Length &&
                    !string.IsNullOrWhiteSpace(Options.MapNames[def.Id]) &&
                    def.Name != Options.MapNames[def.Id])
                {
                    def.Name = Options.MapNames[def.Id];
                    changed = true;
                }
            }

            return changed;
        }
    }
}
