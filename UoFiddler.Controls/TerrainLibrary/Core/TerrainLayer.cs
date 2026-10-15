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

using UoFiddler.Controls.ArtLibrary.Core;

namespace UoFiddler.Controls.TerrainLibrary.Core
{
    /// <summary>
    /// The two graphic layers of a terrain library. Both use the same slot numbers:
    /// slot 0x0001 can hold a land tile, a texture or both.
    /// </summary>
    public enum TerrainLayer
    {
        LandTile = 0,
        Texture = 1
    }

    public static class TerrainLayerExtensions
    {
        /// <summary>File extension of the index file of a layer, e.g. ".landlib.idx".</summary>
        public static string IdxExtension(this TerrainLayer layer) =>
            layer == TerrainLayer.LandTile ? ".landlib.idx" : ".texlib.idx";

        /// <summary>File extension of the data file of a layer, e.g. ".landlib.mul".</summary>
        public static string MulExtension(this TerrainLayer layer) =>
            layer == TerrainLayer.LandTile ? ".landlib.mul" : ".texlib.mul";

        /// <summary>Kind stored in the file header (same header format as the art library).</summary>
        public static ArtLibraryKind Kind(this TerrainLayer layer) =>
            layer == TerrainLayer.LandTile ? ArtLibraryKind.LandArt : ArtLibraryKind.Texture;

        public static string DisplayName(this TerrainLayer layer) =>
            layer == TerrainLayer.LandTile ? "LandTiles" : "Texturen";

        public static string SingleName(this TerrainLayer layer) =>
            layer == TerrainLayer.LandTile ? "LandTile" : "Textur";

        public static TerrainLayer Other(this TerrainLayer layer) =>
            layer == TerrainLayer.LandTile ? TerrainLayer.Texture : TerrainLayer.LandTile;

        /// <summary>File name prefix for exports, same as the original tabs ("Landtile 0x0003.bmp", "Texture 0x0003.bmp").</summary>
        public static string ExportPrefix(this TerrainLayer layer) =>
            layer == TerrainLayer.LandTile ? "Landtile" : "Texture";
    }
}
