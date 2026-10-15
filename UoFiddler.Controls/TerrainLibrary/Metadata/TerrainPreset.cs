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
    /// A template ("Vorlage") of slot settings: tiledata values and radar color that can be applied to
    /// one or many slots with one click. Built-in templates come from TerrainDefaults, own templates are
    /// stored in the library.
    /// </summary>
    public sealed class TerrainPreset
    {
        public string Name { get; set; } = string.Empty;

        public string TileName { get; set; }

        public ulong? TileFlags { get; set; }

        /// <summary>Texture id to set, null = keep the slot's texture id.</summary>
        public ushort? TextureId { get; set; }

        public ushort? RadarColor { get; set; }

        /// <summary>Category to assign, null = keep.</summary>
        public int? CategoryId { get; set; }

        public string Description { get; set; }

        /// <summary>True for the templates delivered with the tool (not saved in the library).</summary>
        [JsonIgnore]
        public bool BuiltIn { get; set; }

        public TerrainPreset Clone() => (TerrainPreset)MemberwiseClone();

        public override string ToString() => BuiltIn ? $"{Name} (Standard)" : Name;
    }
}
