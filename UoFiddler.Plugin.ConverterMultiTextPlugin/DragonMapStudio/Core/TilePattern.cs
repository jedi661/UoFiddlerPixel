/***************************************************************************
 *
 *  * Dragon Map Studio – pattern of built land tiles + statics for the CGI pattern brush
 *
 ***************************************************************************/

using System.Collections.Generic;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>Which height the pattern brush draws.</summary>
    public enum PatternHeightMode
    {
        /// <summary>Height of the new map + the heights in the stamp (relative to its middle) – like the original.</summary>
        Original,

        /// <summary>The height of the new map stays (road / floor follows the terrain).</summary>
        Terrain,

        /// <summary>A fixed height (e.g. a plateau, a floor of a house).</summary>
        Fixed
    }

    /// <summary>
    /// What the CGI pattern brush draws: land tiles with heights relative to the pattern (from a stamp made in the CGI view)
    /// and statics with their height above the land. Freehand = repeats like a texture along the mouse, otherwise once.
    /// </summary>
    public sealed class TilePattern
    {
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>Land tiles (null = the stamp has no built tiles: only statics can be drawn).</summary>
        public ushort[] Tiles { get; set; }

        /// <summary>Land height of every cell minus the median height of the pattern (a road stays on the terrain, a river bed is carved in).</summary>
        public int[] RelativeZ { get; set; }

        public Dictionary<int, List<LayerStatic>> Statics { get; } = new Dictionary<int, List<LayerStatic>>();

        public bool Land { get; set; } = true;
        public bool StaticsOn { get; set; } = true;
        public bool Freehand { get; set; } = true;

        /// <summary>Dragon transitions around the stroke afterwards (soft edges road / grass).</summary>
        public bool Transitions { get; set; } = true;

        /// <summary>Undo per part: new undo step after this many cells (0 = the whole stroke).</summary>
        public int StepCells { get; set; }

        public PatternHeightMode HeightMode { get; set; } = PatternHeightMode.Terrain;
        public int HeightValue { get; set; }
        public int HeightOffset { get; set; }

        /// <summary>After the stroke: smooth height spikes around it.</summary>
        public bool AutoFix { get; set; } = true;

        /// <summary>New land height of a cell (current height, pattern cell k).</summary>
        public int HeightFor(int current, int k)
        {
            int z = HeightMode switch
            {
                PatternHeightMode.Original => current + (RelativeZ != null && k < RelativeZ.Length ? RelativeZ[k] : 0),
                PatternHeightMode.Fixed => HeightValue,
                _ => current
            };
            return System.Math.Clamp(z + HeightOffset, -128, 127);
        }

        public bool HasLand => Tiles != null && RelativeZ != null && Tiles.Length == Width * Height;

        public static TilePattern FromStamp(ColorStamp s)
        {
            var p = new TilePattern
            {
                Name = s.Name,
                Width = s.Width,
                Height = s.Height,
                Freehand = s.DrawMode == StampDrawMode.Freehand,
                Land = s.Layers != StampLayers.StaticsOnly,
                StaticsOn = s.Layers != StampLayers.LandOnly
            };
            if (s.CanLearn)
            {
                p.Tiles = s.LandTiles;
                List<int> z = s.LandZ.Select(v => (int)v).OrderBy(v => v).ToList();
                int median = z[z.Count / 2];
                p.RelativeZ = s.LandZ.Select(v => v - median).ToArray();
            }
            foreach (LayerStatic st in s.Statics ?? new List<LayerStatic>())
            {
                int k = st.Y * s.Width + st.X;
                if (!p.Statics.TryGetValue(k, out List<LayerStatic> l))
                {
                    p.Statics[k] = l = new List<LayerStatic>();
                }
                // absolute heights (whole converted maps) become heights above the land of the cell
                int dz = st.Absolute && s.LandZ != null && k < s.LandZ.Length ? st.Dz - s.LandZ[k] : st.Dz;
                l.Add(new LayerStatic { X = st.X, Y = st.Y, Dz = dz, Id = st.Id, Hue = st.Hue });
            }
            return p;
        }
    }
}
