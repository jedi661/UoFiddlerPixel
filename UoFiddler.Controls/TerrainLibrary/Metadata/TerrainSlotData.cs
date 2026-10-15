// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System.Text.Json.Serialization;

namespace UoFiddler.Controls.TerrainLibrary.Metadata
{
    /// <summary>
    /// Terrain specific settings of one slot (stored in Name.terrain.json). They live only in the library
    /// until they are sent to tiledata.mul / radarcol.mul with the buttons in the window.
    /// Names, descriptions, categories and tags of a slot are kept in <see cref="TerrainMetadata.Catalog"/>.
    /// </summary>
    public sealed class TerrainSlotData
    {
        /// <summary>Radar color (ARGB1555 like radarcol.mul), null = not set.</summary>
        public ushort? RadarColor { get; set; }

        /// <summary>tiledata.mul land name, null = not set.</summary>
        public string TileName { get; set; }

        /// <summary>tiledata.mul land flags (TileFlag), null = not set.</summary>
        public ulong? TileFlags { get; set; }

        /// <summary>tiledata.mul texture id of the land tile, null = not set.</summary>
        public ushort? TextureId { get; set; }

        /// <summary>Id in art.mul the land tile came from.</summary>
        public int? SourceLandId { get; set; }

        /// <summary>Id in texmaps.mul the texture came from.</summary>
        public int? SourceTextureId { get; set; }

        public bool Favorite { get; set; }

        /// <summary>Name of the template (preset) that was applied last, for information only.</summary>
        public string PresetName { get; set; }

        /// <summary>True when tiledata values are stored.</summary>
        [JsonIgnore]
        public bool HasTileData => TileName != null || TileFlags != null || TextureId != null;

        /// <summary>True when tiledata values or a color are stored ("Einstellungen").</summary>
        [JsonIgnore]
        public bool HasSettings => HasTileData || RadarColor != null;

        [JsonIgnore]
        public bool IsEmpty => !HasSettings && SourceLandId == null && SourceTextureId == null && !Favorite && PresetName == null;

        public TerrainSlotData Clone()
        {
            return (TerrainSlotData)MemberwiseClone();
        }
    }
}
