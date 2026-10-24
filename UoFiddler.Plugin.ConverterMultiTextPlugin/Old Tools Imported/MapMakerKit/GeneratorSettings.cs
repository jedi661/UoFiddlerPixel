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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>Where a biome prefers to grow.</summary>
    public enum BiomePlacement
    {
        Anywhere,
        Inland,
        Coast,
        NearMountains,
        Lowlands,
        North,
        South,
        West,
        East
    }

    /// <summary>One terrain type the generator spreads over the land in random patches.</summary>
    public class BiomeRule : INotifyPropertyChanged
    {
        private bool _enabled = true;
        private int _terrainId = TerrainIds.Forest;
        private double _coverage = 20;
        private int _patchSize = 160;
        private BiomePlacement _placement = BiomePlacement.Anywhere;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Enabled
        {
            get => _enabled;
            set { _enabled = value; OnChanged(nameof(Enabled)); }
        }

        /// <summary>Terrain group ID (see Terrain.xml).</summary>
        public int TerrainId
        {
            get => _terrainId;
            set { _terrainId = value; OnChanged(nameof(TerrainId)); }
        }

        /// <summary>Percent of the land area (0..100).</summary>
        public double Coverage
        {
            get => _coverage;
            set { _coverage = Math.Clamp(value, 0, 100); OnChanged(nameof(Coverage)); }
        }

        /// <summary>Typical patch size in tiles (bigger = fewer, larger areas).</summary>
        public int PatchSize
        {
            get => _patchSize;
            set { _patchSize = Math.Clamp(value, 8, 4000); OnChanged(nameof(PatchSize)); }
        }

        public BiomePlacement Placement
        {
            get => _placement;
            set { _placement = value; OnChanged(nameof(Placement)); }
        }

        public BiomeRule()
        {
        }

        public BiomeRule(int terrainId, double coverage, int patchSize, BiomePlacement placement)
        {
            _terrainId = terrainId;
            _coverage = coverage;
            _patchSize = patchSize;
            _placement = placement;
        }

        private void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>All generator options. Serializable so presets can be saved as XML.</summary>
    public class GeneratorSettings
    {
        // ---------------- general
        [Category("1 General"), DisplayName("Seed"), Description("Same seed + same settings = same map. Change it to get a new world.")]
        public int Seed { get; set; } = 12345;

        // ---------------- land / water
        [Category("2 Land and water"), DisplayName("Land %"), Description("Percent of the overworld that becomes land.")]
        public double LandPercent { get; set; } = 35;

        [Category("2 Land and water"), DisplayName("Continents"), Description("Number of big land masses. 0 = only noise (archipelago).")]
        public int Continents { get; set; } = 3;

        [Category("2 Land and water"), DisplayName("Islands"), Description("Number of additional small islands.")]
        public int Islands { get; set; } = 12;

        [Category("2 Land and water"), DisplayName("Coast roughness"), Description("0..100 - how ragged the coast line is.")]
        public double CoastRoughness { get; set; } = 55;

        [Category("2 Land and water"), DisplayName("Surrounded by water"), Description("Keep a ring of water around the whole overworld.")]
        public bool SurroundedByWater { get; set; } = true;

        [Category("2 Land and water"), DisplayName("Water border (tiles)"), Description("Width of the water ring around the map.")]
        public int BorderWidth { get; set; } = 120;

        [Category("2 Land and water"), DisplayName("Min. island size (tiles)"), Description("Land pieces smaller than this are removed.")]
        public int MinIslandSize { get; set; } = 400;

        [Category("2 Land and water"), DisplayName("Min. lake size (tiles)"), Description("Lakes smaller than this are filled with land.")]
        public int MinLakeSize { get; set; } = 150;

        [Category("2 Land and water"), DisplayName("Shallow water terrain"), Description("Terrain along the coast (Shallow Water has the most transitions).")]
        [TypeConverter(typeof(TerrainIdConverter))]
        public int ShallowWaterTerrain { get; set; } = TerrainIds.ShallowWater;
        [Category("2 Land and water"), DisplayName("Deep water terrain"), Description("Terrain of the open sea.")]
        [TypeConverter(typeof(TerrainIdConverter))]
        public int DeepWaterTerrain { get; set; } = TerrainIds.DeepWater;

        [Category("2 Land and water"), DisplayName("Shallow water width"), Description("Width of the shallow water along the coast (min. 2).")]
        public int ShallowWidth { get; set; } = 6;

        // ---------------- coast
        [Category("3 Coast"), DisplayName("Beach terrain"), Description("Terrain of the beaches.")]
        [TypeConverter(typeof(TerrainIdConverter))]
        public int BeachTerrain { get; set; } = TerrainIds.Sand;
        [Category("3 Coast"), DisplayName("Beach width"), Description("Width of the beach in tiles (0 = no beach).")]
        public int BeachWidth { get; set; } = 3;

        [Category("3 Coast"), DisplayName("Beach coverage %"), Description("Percent of the coast with beach. 100 avoids Grass/Sand/Water corners which have few transitions.")]
        public double BeachCoverage { get; set; } = 100;

        // ---------------- base land + hills
        [Category("4 Land"), DisplayName("Base terrain"), Description("Terrain of all land that is not covered by a biome.")]
        [TypeConverter(typeof(TerrainIdConverter))]
        public int BaseTerrain { get; set; } = TerrainIds.Grass;

        [Category("4 Land"), DisplayName("Hill height (z)"), Description("Maximum height of hills on normal land. 0 = flat.")]
        public int HillHeight { get; set; } = 10;
        [Category("4 Land"), DisplayName("Hill size (tiles)"), Description("Typical size of the hills.")]
        public int HillSize { get; set; } = 90;

        // ---------------- mountains
        [Category("5 Mountains"), DisplayName("Mountain terrain"), Description("Terrain of the mountains (Rock).")]
        [TypeConverter(typeof(TerrainIdConverter))]
        public int MountainTerrain { get; set; } = TerrainIds.Rock;

        [Category("5 Mountains"), DisplayName("Mountains %"), Description("Percent of the land covered by mountains.")]
        public double MountainPercent { get; set; } = 10;
        [Category("5 Mountains"), DisplayName("Mountain range size"), Description("Typical size of the mountain ranges in tiles.")]
        public int MountainSize { get; set; } = 220;
        [Category("5 Mountains"), DisplayName("Min. height (z)"), Description("Height of low mountains.")]
        public int MountainMinHeight { get; set; } = 10;
        [Category("5 Mountains"), DisplayName("Max. height (z)"), Description("Height of the highest peaks (max 115).")]
        public int MountainMaxHeight { get; set; } = 70;

        [Category("5 Mountains"), DisplayName("Slope (z per tile)"), Description("Height gained per tile from the mountain foot. Small = gentle, big = steep.")]
        public int MountainSlope { get; set; } = 5;

        [Category("5 Mountains"), DisplayName("Snow on peaks"), Description("High mountain tiles get the snow terrain.")]
        public bool SnowOnPeaks { get; set; } = true;
        [Category("5 Mountains"), DisplayName("Snow terrain"), Description("Terrain used for the snow peaks.")]
        [TypeConverter(typeof(TerrainIdConverter))]
        public int SnowTerrain { get; set; } = TerrainIds.Snow;

        [Category("5 Mountains"), DisplayName("Snow line (z)"), Description("Mountain tiles at or above this height get snow.")]
        public int SnowLine { get; set; } = 55;

        // ---------------- biomes
        [Browsable(false)]
        public List<BiomeRule> Biomes { get; set; } = new List<BiomeRule>();

        [Category("6 Biomes"), DisplayName("Separate biomes"), Description("Keep a strip of base terrain between two different biomes (there are few transitions e.g. Forest <-> Jungle).")]
        public bool SeparateBiomes { get; set; } = true;
        [Category("6 Biomes"), DisplayName("Separation width"), Description("Width of that strip in tiles.")]
        public int SeparationWidth { get; set; } = 2;

        // ---------------- dungeon
        [Category("7 Dungeon"), DisplayName("Generate caves"), Description("Carve caves into the dungeon area. Off = the area is only filled with Blank.")]
        public bool DungeonEnabled { get; set; } = true;

        [Category("7 Dungeon"), DisplayName("Dungeon area from X"), Description("X where the dungeon area starts (5120 on Felucca/Trammel). 0 = no dungeon area, the whole map is overworld.")]
        public int DungeonStartX { get; set; } = 5120;
        [Category("7 Dungeon"), DisplayName("Number of caves"), Description("How many caves are placed.")]
        public int DungeonCount { get; set; } = 14;
        [Category("7 Dungeon"), DisplayName("Min. cave size"), Description("Smallest cave size in tiles.")]
        public int DungeonMinSize { get; set; } = 90;
        [Category("7 Dungeon"), DisplayName("Max. cave size"), Description("Biggest cave size in tiles.")]
        public int DungeonMaxSize { get; set; } = 260;

        [Category("7 Dungeon"), DisplayName("Openness"), Description("0..100 - narrow tunnels (0) or big halls (100).")]
        public double DungeonOpenness { get; set; } = 50;
        [Category("7 Dungeon"), DisplayName("Cave floor terrain"), Description("Terrain of the cave floor.")]
        [TypeConverter(typeof(TerrainIdConverter))]
        public int DungeonFloorTerrain { get; set; } = TerrainIds.DungeonCave;
        [Category("7 Dungeon"), DisplayName("Fill terrain"), Description("Terrain around the caves (Blank).")]
        [TypeConverter(typeof(TerrainIdConverter))]
        public int DungeonFillTerrain { get; set; } = TerrainIds.Blank;

        [Category("7 Dungeon"), DisplayName("Lava pools %"), Description("Percent of the caves that get a lava pool.")]
        public double DungeonLavaPercent { get; set; } = 20;

        [Category("7 Dungeon"), DisplayName("Water pools %"), Description("Percent of the caves that get a water pool.")]
        public double DungeonWaterPercent { get; set; } = 20;

        // ---------------- sketch / template
        [Category("8 Sketch / template"), DisplayName("Sketch usage"), Description("Smart: painted base terrain (Grass) = land with generated biomes, other painted terrains stay exactly. UseTerrains: every painted terrain stays exactly. LandWaterOnly: the sketch only decides land, water and mountains.")]
        public SketchMode SketchUsage { get; set; } = SketchMode.Smart;

        [Category("8 Sketch / template"), DisplayName("Unpainted = water"), Description("True: everything that is not painted in the sketch becomes water (your painted land is surrounded by sea). False: the generator adds its own land there.")]
        public bool SketchUnpaintedIsWater { get; set; } = true;

        [Category("8 Sketch / template"), DisplayName("Height map sea level"), Description("For grey scale height maps: grey value (0..255) of the coast line. Darker = water.")]
        public int TemplateSeaLevel { get; set; } = 100;

        public static GeneratorSettings CreateDefault()
        {
            GeneratorSettings settings = new GeneratorSettings();
            settings.Biomes.Add(new BiomeRule(TerrainIds.Forest, 28, 180, BiomePlacement.Anywhere));
            settings.Biomes.Add(new BiomeRule(TerrainIds.Jungle, 8, 220, BiomePlacement.South));
            settings.Biomes.Add(new BiomeRule(TerrainIds.Swamp, 3, 90, BiomePlacement.Lowlands));
            settings.Biomes.Add(new BiomeRule(TerrainIds.Snow, 6, 260, BiomePlacement.North));
            settings.Biomes.Add(new BiomeRule(TerrainIds.Sand, 5, 200, BiomePlacement.Inland));
            settings.Biomes.Add(new BiomeRule(TerrainIds.LargeDirt, 3, 60, BiomePlacement.Anywhere));
            return settings;
        }

        public void Save(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GeneratorSettings));
            using (FileStream stream = File.Create(path))
            {
                serializer.Serialize(stream, this);
            }
        }

        public static GeneratorSettings LoadFrom(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GeneratorSettings));
            using (FileStream stream = File.OpenRead(path))
            {
                return (GeneratorSettings)serializer.Deserialize(stream);
            }
        }
    }

    public enum SketchMode
    {
        /// <summary>Painted base terrain = land with generated biomes, other painted terrains are kept.</summary>
        Smart,

        /// <summary>Painted terrain is used exactly where it was painted.</summary>
        UseTerrains,

        /// <summary>The sketch only decides land / water / mountain; biomes are generated.</summary>
        LandWaterOnly
    }
}
