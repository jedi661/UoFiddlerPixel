// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.IO;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>
    /// One place that decides where the Map Maker data files live.
    /// The old code mixed Directory.GetCurrentDirectory() and AppDomain.BaseDirectory,
    /// which could point to two different folders.
    /// </summary>
    internal static class MapDataPaths
    {
        private static string _dataRoot;

        /// <summary>The "Data" folder next to UoFiddler.exe (fallback: current directory).</summary>
        public static string DataRoot
        {
            get
            {
                if (_dataRoot != null)
                {
                    return _dataRoot;
                }

                string[] candidates =
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Data")
                };

                foreach (string candidate in candidates)
                {
                    if (File.Exists(Path.Combine(candidate, "System", "Terrain.xml")))
                    {
                        _dataRoot = candidate;
                        return _dataRoot;
                    }
                }

                _dataRoot = candidates[0];
                return _dataRoot;
            }
        }

        public static string SystemDir => Path.Combine(DataRoot, "System");

        public static string MapInfoFile => Path.Combine(SystemDir, "MapInfo.xml");

        public static string TransitionsDir => Path.Combine(DataRoot, "Transitions");

        public static string RoughEdgeDir => Path.Combine(SystemDir, "RoughEdge");

        /// <summary>Default folder for map templates / patterns (created on demand).</summary>
        public static string TemplatesDir => Path.Combine(DataRoot, "MapTemplates");

        /// <summary>Folder for saved generator presets (created on demand).</summary>
        public static string PresetsDir => Path.Combine(SystemDir, "GeneratorPresets");

        public static string EnsureDirectory(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}
