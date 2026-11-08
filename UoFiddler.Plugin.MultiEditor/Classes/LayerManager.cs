/***************************************************************************
 *
 * MultiEditor – layers (Z ranges) that can be hidden, ghosted or locked
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    public sealed class EditorLayer
    {
        public string Name { get; set; }

        /// <summary>
        /// Inclusive
        /// </summary>
        public int ZMin { get; set; }

        /// <summary>
        /// Inclusive
        /// </summary>
        public int ZMax { get; set; }

        public bool Visible { get; set; } = true;

        /// <summary>
        /// Drawn semi transparent
        /// </summary>
        public bool Ghost { get; set; }

        /// <summary>
        /// Can't be picked / changed with the mouse
        /// </summary>
        public bool Locked { get; set; }

        public bool Contains(int z) => z >= ZMin && z <= ZMax;

        public override string ToString() => $"{Name}  (Z {ZMin} … {ZMax})";
    }

    public sealed class LayerManager
    {
        public List<EditorLayer> Layers { get; } = new List<EditorLayer>();

        /// <summary>
        /// Decoration tiles (addon only) as an own switch
        /// </summary>
        public bool DecoVisible { get; set; } = true;

        public bool DecoGhost { get; set; }

        public bool DecoLocked { get; set; }

        public int FloorHeight { get; set; } = 20;

        public EditorLayer GetLayer(int z)
        {
            // the last matching layer wins (custom layers can overlay)
            for (int i = Layers.Count - 1; i >= 0; --i)
            {
                if (Layers[i].Contains(z))
                {
                    return Layers[i];
                }
            }

            return null;
        }

        public bool IsHidden(MultiTile tile)
        {
            if (tile.IsDeco && !DecoVisible)
            {
                return true;
            }

            EditorLayer l = GetLayer(tile.Z);
            return l != null && !l.Visible;
        }

        public bool IsGhost(MultiTile tile)
        {
            if (tile.IsDeco && DecoGhost)
            {
                return true;
            }

            EditorLayer l = GetLayer(tile.Z);
            return l != null && l.Ghost;
        }

        public bool IsLocked(MultiTile tile)
        {
            if (tile.IsDeco && DecoLocked)
            {
                return true;
            }

            EditorLayer l = GetLayer(tile.Z);
            return l != null && l.Locked;
        }

        /// <summary>
        /// One layer per floor (every <see cref="FloorHeight"/> Z, at least 4 floors). Visibility of existing floors is kept.
        /// </summary>
        public void CreateFloors(int zMin, int zMax, Func<int, string> name)
        {
            var old = Layers.GroupBy(l => l.Name ?? string.Empty).ToDictionary(g => g.Key, g => g.First());
            Layers.Clear();
            int h = Math.Max(1, FloorHeight);
            int start = (int)Math.Floor(Math.Min(zMin, 0) / (double)h) * h;
            int end = Math.Max(zMax, start + (3 * h));
            int i = 0;
            for (int z = start; z <= end; z += h)
            {
                string n = name(i++);
                var layer = new EditorLayer { Name = n, ZMin = z, ZMax = z + h - 1 };
                if (old.TryGetValue(n, out EditorLayer before))
                {
                    layer.Visible = before.Visible;
                    layer.Ghost = before.Ghost;
                    layer.Locked = before.Locked;
                }

                Layers.Add(layer);
            }

            // the first / last layer take everything below / above
            if (Layers.Count > 0)
            {
                Layers[Layers.Count - 1].ZMax = 127;
                Layers[0].ZMin = -128;
            }
        }

        /// <summary>
        /// true if tiles reach more than one floor into the top layer
        /// </summary>
        public bool NeedsMoreFloors(int zMax)
        {
            if (Layers.Count == 0)
            {
                return true;
            }

            EditorLayer top = Layers.OrderBy(l => l.ZMin).Last();
            return zMax >= top.ZMin + Math.Max(1, FloorHeight);
        }

        public void ShowOnly(EditorLayer layer, bool ghostBelow)
        {
            foreach (EditorLayer l in Layers)
            {
                bool below = l.ZMax < layer.ZMin;
                l.Visible = l == layer || (ghostBelow && below);
                l.Ghost = ghostBelow && below;
            }
        }

        public void ShowAll()
        {
            foreach (EditorLayer l in Layers)
            {
                l.Visible = true;
                l.Ghost = false;
            }
        }

        public bool AnyFilter => !DecoVisible || DecoGhost || DecoLocked || Layers.Any(l => !l.Visible || l.Ghost || l.Locked);
    }
}
