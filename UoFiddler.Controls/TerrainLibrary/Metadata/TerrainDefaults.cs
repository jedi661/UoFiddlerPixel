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

using System.Collections.Generic;
using System.Drawing;
using Ultima;
using UoFiddler.Controls.TerrainLibrary.Helpers;

namespace UoFiddler.Controls.TerrainLibrary.Metadata
{
    /// <summary>
    /// Built-in templates ("Standardvorlagen") for quick slot settings. They can be switched off in the
    /// options; own templates are stored in the library and can be based on these.
    /// </summary>
    public static class TerrainDefaults
    {
        private static TerrainPreset Make(string name, string tileName, TileFlag flags, Color color, string description)
        {
            return new TerrainPreset
            {
                Name = name,
                TileName = tileName,
                TileFlags = (ulong)flags,
                RadarColor = TerrainColor.FromColor(color),
                Description = description,
                BuiltIn = true
            };
        }

        public static List<TerrainPreset> Create()
        {
            return new List<TerrainPreset>
            {
                Make("Gras", "grass", TileFlag.None, Color.FromArgb(56, 104, 32), "Begehbares Gras"),
                Make("Wald-Boden", "forest", TileFlag.None, Color.FromArgb(48, 80, 24), "Waldboden, begehbar"),
                Make("Erde", "dirt", TileFlag.None, Color.FromArgb(112, 80, 48), "Erde / Feldweg"),
                Make("Sand", "sand", TileFlag.None, Color.FromArgb(200, 176, 120), "Strand / Wüste"),
                Make("Pflaster", "cobblestones", TileFlag.None, Color.FromArgb(120, 112, 104), "Straße / Platz"),
                Make("Schnee", "snow", TileFlag.None, Color.FromArgb(232, 232, 240), "Schnee, begehbar"),
                Make("Sumpf", "swamp", TileFlag.Wet, Color.FromArgb(64, 72, 40), "Sumpf, nass"),
                Make("Wasser", "water", TileFlag.Wet | TileFlag.Impassable, Color.FromArgb(32, 64, 128), "Wasser, nicht begehbar"),
                Make("Fels / Berg", "rock", TileFlag.Impassable, Color.FromArgb(96, 88, 80), "Berg, nicht begehbar"),
                Make("Lava", "lava", TileFlag.Impassable | TileFlag.Damaging, Color.FromArgb(208, 72, 16), "Lava, schadet"),
                Make("Höhlenboden", "cave", TileFlag.None, Color.FromArgb(72, 64, 56), "Höhle / Dungeon"),
                Make("Leer (void)", "void", TileFlag.Impassable, Color.Black, "Schwarzer Rand / nicht begehbar")
            };
        }
    }
}
