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

using System;
using System.Drawing;
using Ultima;
using UoFiddler.Controls.ArtLibrary.Core;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.TerrainLibrary.Core;
using UoFiddler.Controls.TerrainLibrary.Metadata;

namespace UoFiddler.Controls.TerrainLibrary.Helpers
{
    /// <summary>
    /// The only place where the terrain library talks to the original UO files (through the public
    /// methods of Ultima.Art, Ultima.Textures, Ultima.RadarCol and Ultima.TileData).
    /// Changes go into memory like in the original tabs and are saved there:
    /// land tiles in the Land Tiles tab (art.mul), textures in the Textures tab, tiledata in the
    /// TileData tab, radar colors in the RadarColor tab.
    /// </summary>
    public static class TerrainTransfer
    {
        public const int OriginalMaxLandId = 0x3FFF;

        #region [ Availability ]

        public static bool IsAvailable(TerrainLayer layer)
        {
            try
            {
                return layer == TerrainLayer.LandTile ? Art.GetIdxLength() > 0 : Textures.GetIdxLength() > 0;
            }
            catch
            {
                return false;
            }
        }

        public static int OriginalMaxId(TerrainLayer layer)
        {
            if (layer == TerrainLayer.LandTile)
            {
                return OriginalMaxLandId;
            }

            int length = Textures.GetIdxLength();
            return Math.Min(length > 0 ? length - 1 : 0x3FFF, 0x3FFF);
        }

        public static bool IsValidOriginalId(TerrainLayer layer, int id) => id >= 0 && id <= OriginalMaxId(layer);

        public static bool IsOriginalOccupied(TerrainLayer layer, int id)
        {
            if (!IsValidOriginalId(layer, id))
            {
                return false;
            }

            return layer == TerrainLayer.LandTile ? Art.IsValidLand(id) : Textures.TestTexture(id);
        }

        public static bool IsTileDataAvailable
        {
            get
            {
                try
                {
                    return TileData.LandTable != null && TileData.LandTable.Length > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool IsRadarColAvailable
        {
            get
            {
                try
                {
                    return RadarCol.Colors != null && RadarCol.Colors.Length > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion

        #region [ Graphics ]

        /// <summary>Graphic of the original file (including unsaved changes there) or null.</summary>
        public static Bitmap GetOriginal(TerrainLayer layer, int id)
        {
            if (!IsOriginalOccupied(layer, id))
            {
                return null;
            }

            return layer == TerrainLayer.LandTile ? Art.GetLand(id) : Textures.GetTexture(id);
        }

        /// <summary>Writes a graphic into the original file (in memory) and refreshes the original tab.</summary>
        public static void WriteGraphicToOriginal(TerrainLayer layer, int id, Bitmap bitmap, object sender)
        {
            if (!IsValidOriginalId(layer, id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), id,
                    $"Die ID liegt außerhalb der Originaldatei (0 bis 0x{OriginalMaxId(layer):X4}).");
            }

            // independent ARGB1555 copy - the library bitmap is cached and shared
            Bitmap copy = ArtLibraryCodec.ToBitmap(ArtLibraryCodec.ToPixels(bitmap), bitmap.Width, bitmap.Height);

            if (layer == TerrainLayer.LandTile)
            {
                Art.ReplaceLand(id, copy);
                ControlEvents.FireLandTileChangeEvent(sender, id);
                Options.ChangedUltimaClass["Art"] = true;
            }
            else
            {
                Textures.Replace(id, copy);
                ControlEvents.FireTextureChangeEvent(sender, id);
                Options.ChangedUltimaClass["Texture"] = true;
            }
        }

        public static void RemoveFromOriginal(TerrainLayer layer, int id, object sender)
        {
            if (!IsValidOriginalId(layer, id))
            {
                return;
            }

            if (layer == TerrainLayer.LandTile)
            {
                Art.RemoveLand(id);
                ControlEvents.FireLandTileChangeEvent(sender, id);
                Options.ChangedUltimaClass["Art"] = true;
            }
            else
            {
                Textures.Remove(id);
                ControlEvents.FireTextureChangeEvent(sender, id);
                Options.ChangedUltimaClass["Texture"] = true;
            }
        }

        #endregion

        #region [ TileData ]

        /// <summary>Reads name, flags and texture id of an original land tile from tiledata.mul.</summary>
        public static bool TryReadTileData(int landId, out string name, out ulong flags, out ushort textureId)
        {
            name = string.Empty;
            flags = 0;
            textureId = 0;
            if (!IsTileDataAvailable || landId < 0 || landId >= TileData.LandTable.Length)
            {
                return false;
            }

            LandData data = TileData.LandTable[landId];
            name = data.Name ?? string.Empty;
            flags = (ulong)data.Flags;
            textureId = data.TextureId;
            return true;
        }

        /// <summary>Copies the tiledata values of an original land tile into the slot settings.</summary>
        public static bool ReadTileDataInto(TerrainSlotData slot, int landId)
        {
            if (!TryReadTileData(landId, out string name, out ulong flags, out ushort textureId))
            {
                return false;
            }

            slot.TileName = name;
            slot.TileFlags = flags;
            slot.TextureId = textureId;
            return true;
        }

        /// <summary>
        /// Writes the stored tiledata values of a slot into tiledata.mul (in memory) for <paramref name="landId"/>.
        /// Only values that are set are written. Save in the TileData tab.
        /// </summary>
        public static bool WriteTileData(TerrainSlotData slot, int landId, object sender)
        {
            if (slot == null || !slot.HasTileData || !IsTileDataAvailable || landId < 0 || landId >= TileData.LandTable.Length)
            {
                return false;
            }

            LandData data = TileData.LandTable[landId];
            if (slot.TileName != null)
            {
                data.Name = slot.TileName;
            }

            if (slot.TileFlags != null)
            {
                data.Flags = (TileFlag)slot.TileFlags.Value;
            }

            if (slot.TextureId != null)
            {
                data.TextureId = slot.TextureId.Value;
            }

            TileData.LandTable[landId] = data;
            ControlEvents.FireTileDataChangeEvent(sender, landId);
            Options.ChangedUltimaClass["TileData"] = true;
            return true;
        }

        #endregion

        #region [ RadarColor ]

        public static ushort? ReadRadarColor(int landId)
        {
            if (!IsRadarColAvailable || landId < 0 || landId > OriginalMaxLandId)
            {
                return null;
            }

            return RadarCol.GetLandColor(landId);
        }

        /// <summary>Writes a land radar color into radarcol.mul (in memory). Save in the RadarColor tab.</summary>
        public static bool WriteRadarColor(int landId, ushort color)
        {
            if (!IsRadarColAvailable || landId < 0 || landId > OriginalMaxLandId || landId >= RadarCol.Colors.Length)
            {
                return false;
            }

            RadarCol.SetLandColor(landId, color);
            Options.ChangedUltimaClass["RadarColor"] = true;
            return true;
        }

        #endregion

        /// <summary>Export file name like the original tabs: "Landtile 0x0003.bmp" / "Texture 0x0003.bmp".</summary>
        public static string GetExportFileName(TerrainLayer layer, int id, string extension)
        {
            return $"{layer.ExportPrefix()} 0x{id:X4}.{extension.TrimStart('.')}";
        }
    }
}
