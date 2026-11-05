// /***************************************************************************
//  *
//  * Dragon Map Studio – access to the client art loaded in UoFiddler (cached, never throws)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Ultima;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>One part of a multi (offsets to the multi centre).</summary>
    public struct MultiPart
    {
        public ushort Id;
        public int Dx;
        public int Dy;
        public int Dz;
        public bool Visible;
    }

    /// <summary>
    /// Land art, textures, static art (with hue), tile data and radar colours of the client files that
    /// UoFiddler has loaded. Everything is cached; if the client files are missing, null / defaults are returned.
    /// </summary>
    public static class UoArtCache
    {
        private static readonly Dictionary<int, Bitmap> _land = new Dictionary<int, Bitmap>();
        private static readonly Dictionary<int, Bitmap> _texture = new Dictionary<int, Bitmap>();
        private static readonly Dictionary<long, Bitmap> _static = new Dictionary<long, Bitmap>();
        private static readonly object _lock = new object();

        /// <summary>Empties the caches (after hues / art were changed in UoFiddler).</summary>
        public static void Clear()
        {
            lock (_lock)
            {
                foreach (Bitmap b in _static.Values)
                {
                    b?.Dispose();
                }
                _land.Clear();
                _texture.Clear();
                _static.Clear();
                _artAvailable = null;
            }
        }

        /// <summary>True when the client art can be read.</summary>
        public static bool ArtAvailable
        {
            get
            {
                try
                {
                    return Art.GetLand(3) != null || Art.GetStatic(0x0E75) != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private static bool? _artAvailable;

        /// <summary><see cref="ArtAvailable"/>, checked once (call <see cref="Clear"/> to check again).</summary>
        public static bool ArtAvailableCached => _artAvailable ??= ArtAvailable;

        public static Bitmap Land(int id)
        {
            lock (_lock)
            {
                if (!_land.TryGetValue(id, out Bitmap b))
                {
                    try
                    {
                        b = Art.GetLand(id);
                    }
                    catch (Exception)
                    {
                        b = null;
                    }
                    _land[id] = b;
                }
                return b;
            }
        }

        /// <summary>Texture (texmaps) of a land tile for sloped terrain, or null.</summary>
        public static Bitmap Texture(int landId)
        {
            lock (_lock)
            {
                if (!_texture.TryGetValue(landId, out Bitmap b))
                {
                    try
                    {
                        int tex = TileData.LandTable[landId & 0x3FFF].TextureId;
                        b = tex > 0 ? Textures.GetTexture(tex) : null;
                    }
                    catch (Exception)
                    {
                        b = null;
                    }
                    _texture[landId] = b;
                }
                return b;
            }
        }

        /// <summary>Static art, coloured with <paramref name="hue"/> (0 = original).</summary>
        public static Bitmap Static(int id, int hue)
        {
            long key = ((long)id << 16) | (uint)(hue & 0xFFFF);
            lock (_lock)
            {
                if (_static.TryGetValue(key, out Bitmap b))
                {
                    return b;
                }
                try
                {
                    Bitmap art = Art.GetStatic(id);
                    if (art != null && (hue & 0x3FFF) != 0)
                    {
                        b = art.Clone(new Rectangle(0, 0, art.Width, art.Height), PixelFormat.Format16bppArgb1555);
                        Hues.GetHue((hue & 0x3FFF) - 1).ApplyTo(b, PartialHue(id));
                    }
                    else
                    {
                        b = art;
                    }
                }
                catch (Exception)
                {
                    b = null;
                }
                _static[key] = b;
                return b;
            }
        }

        /// <summary>Wet land tile without texture: the client draws it flat (water), never stretched.</summary>
        public static bool LandIsWetFlat(int id)
        {
            try
            {
                LandData d = TileData.LandTable[id & 0x3FFF];
                return (d.Flags & TileFlag.Wet) != 0 && d.TextureId == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int MaxLandId => 0x3FFF;

        public static int ItemHeight(int id)
        {
            try
            {
                return TileData.ItemTable[id].Height;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static bool IsBackground(int id)
        {
            try
            {
                return (TileData.ItemTable[id].Flags & TileFlag.Background) != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool PartialHue(int id)
        {
            try
            {
                return (TileData.ItemTable[id].Flags & TileFlag.PartialHue) != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string ItemName(int id)
        {
            try
            {
                return TileData.ItemTable[id].Name ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string LandName(int id)
        {
            try
            {
                return TileData.LandTable[id & 0x3FFF].Name ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static int MaxItemId
        {
            get
            {
                try
                {
                    return Art.GetMaxItemId();
                }
                catch (Exception)
                {
                    return 0x3FFF;
                }
            }
        }

        public static bool IsValidStatic(int id)
        {
            try
            {
                return Art.IsValidStatic(id);
            }
            catch (Exception)
            {
                return true; // unknown without client files – do not report
            }
        }

        public static bool IsValidLand(int id)
        {
            try
            {
                return Art.IsValidLand(id);
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static Color From555(ushort c)
        {
            return Color.FromArgb(((c >> 10) & 0x1F) * 255 / 31, ((c >> 5) & 0x1F) * 255 / 31, (c & 0x1F) * 255 / 31);
        }

        /// <summary>Colours of land tiles from the Dragon rules (used when the client has no radar colour).</summary>
        public static Dictionary<int, Color> RuleLandColors { get; set; } = new Dictionary<int, Color>();

        /// <summary>Radar colour of a land tile (fallback: Dragon colour of the tile, then a colour from the id).</summary>
        public static Color LandColor(int id)
        {
            if (!ArtAvailableCached && RuleLandColors.TryGetValue(id, out Color rule))
            {
                return rule;
            }
            try
            {
                ushort c = RadarCol.GetLandColor(id);
                if (c != 0)
                {
                    return From555(c);
                }
            }
            catch (Exception)
            {
                // no radarcol
            }
            if (RuleLandColors.TryGetValue(id, out Color fromRules))
            {
                return fromRules;
            }
            int h = (int)((id * 2654435761u) >> 8);
            return Color.FromArgb(60 + (h & 0x7F), 60 + ((h >> 7) & 0x7F), 60 + ((h >> 14) & 0x7F));
        }

        public static Color ItemColor(int id)
        {
            try
            {
                ushort c = RadarCol.GetItemColor(id);
                if (c != 0)
                {
                    return From555(c);
                }
            }
            catch (Exception)
            {
                // no radarcol
            }
            return Color.FromArgb(200, 120, 40);
        }

        /// <summary>A typical colour of a hue (for the swatch), black if unknown.</summary>
        public static Color HueColor(int hue)
        {
            if ((hue & 0x3FFF) == 0)
            {
                return Color.Transparent;
            }
            try
            {
                return Hues.GetHue((hue & 0x3FFF) - 1).GetColor(24);
            }
            catch (Exception)
            {
                return Color.Black;
            }
        }

        public static string HueName(int hue)
        {
            if ((hue & 0x3FFF) == 0)
            {
                return "keine";
            }
            try
            {
                return Hues.GetHue((hue & 0x3FFF) - 1).Name;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>Parts of a multi; empty when the multi does not exist.</summary>
        public static List<MultiPart> Multi(int id)
        {
            var list = new List<MultiPart>();
            try
            {
                MultiComponentList m = Multis.GetComponents(id);
                if (m == null || m == MultiComponentList.Empty || m.SortedTiles == null)
                {
                    return list;
                }
                bool anyFlag = false;
                foreach (MultiComponentList.MultiTileEntry t in m.SortedTiles)
                {
                    anyFlag |= t.Flags != 0;
                }
                foreach (MultiComponentList.MultiTileEntry t in m.SortedTiles)
                {
                    list.Add(new MultiPart { Id = t.ItemId, Dx = t.OffsetX, Dy = t.OffsetY, Dz = t.OffsetZ, Visible = !anyFlag || t.Flags != 0 });
                }
            }
            catch (Exception)
            {
                // no multis loaded
            }
            return list;
        }

        public static Bitmap MultiImage(int id)
        {
            try
            {
                return Multis.GetComponents(id)?.GetImage();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
