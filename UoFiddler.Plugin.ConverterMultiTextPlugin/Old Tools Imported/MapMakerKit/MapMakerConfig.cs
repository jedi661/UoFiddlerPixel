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
using System.Xml.Serialization;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>Remembers the Map Maker settings between sessions (Data/System/MapMakerConfig.xml).</summary>
    public class MapMakerConfig
    {
        public string ProjectFolder { get; set; }
        public string TemplateFolder { get; set; }
        public string OutputFolder { get; set; }
        public string ImportFolder { get; set; }
        public string TerrainFile { get; set; } = "Terrain.bmp";
        public string AltitudeFile { get; set; } = "Altitude.bmp";
        public int MapSizeIndex { get; set; } = 1;
        public int CustomWidth { get; set; } = 6144;
        public int CustomHeight { get; set; } = 4096;
        public int MapNumber { get; set; }
        public bool RandomStatics { get; set; } = true;
        public bool ImportStatics { get; set; } = true;
        public GeneratorSettings Generator { get; set; }

        private static string FilePath => Path.Combine(MapDataPaths.SystemDir, "MapMakerConfig.xml");

        public static MapMakerConfig Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MapMakerConfig));
                    using (FileStream stream = File.OpenRead(FilePath))
                    {
                        return (MapMakerConfig)serializer.Deserialize(stream);
                    }
                }
            }
            catch (Exception)
            {
                // broken config: start with defaults
            }

            return new MapMakerConfig();
        }

        public void Save()
        {
            try
            {
                MapDataPaths.EnsureDirectory(MapDataPaths.SystemDir);
                XmlSerializer serializer = new XmlSerializer(typeof(MapMakerConfig));
                using (FileStream stream = File.Create(FilePath))
                {
                    serializer.Serialize(stream, this);
                }
            }
            catch (Exception)
            {
                // not important enough to bother the user
            }
        }
    }
}
