// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Elevation;
using Terrain;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>Well known terrain group IDs from Data/System/Terrain.xml.</summary>
    internal static class TerrainIds
    {
        public const byte NoDraw = 0;
        public const byte Grass = 1;
        public const byte Sand = 4;
        public const byte DeepWater = 5;
        public const byte ShallowWater = 6;
        public const byte LargeDirt = 7;
        public const byte SmallDirt = 8;
        public const byte Water = 9;
        public const byte Jungle = 10;
        public const byte Forest = 11;
        public const byte Snow = 12;
        public const byte Lava = 13;
        public const byte Rock = 18;
        public const byte Blank = 19;
        public const byte MountainCave = 20;
        public const byte DungeonCave = 21;
        public const byte Swamp = 41;
        public const byte Beach = 80;
        public const byte TestPattern = 254;
    }

    /// <summary>
    /// Fast lookup tables for the terrain palette (index = terrain group ID).
    /// </summary>
    internal sealed class TerrainPalette
    {
        public readonly ClsTerrainTable Table;
        public readonly bool[] Defined = new bool[256];
        public readonly Color[] Colors = new Color[256];
        public readonly int[] Argb = new int[256];
        public readonly string[] Names = new string[256];
        public readonly byte[] BaseAltitudeKey = new byte[256];
        public readonly short[] TileIds = new short[256];
        public readonly bool[] RandomAltitude = new bool[256];

        private readonly Dictionary<int, byte> _nearestCache = new Dictionary<int, byte>();
        private readonly object _cacheLock = new object();

        public TerrainPalette(ClsTerrainTable table, AltitudePalette altitudes)
        {
            Table = table;
            for (int i = 0; i < 256; i++)
            {
                ClsTerrain terrain = table.TerrianGroup(i);
                if (terrain == null)
                {
                    Colors[i] = Color.Black;
                    Names[i] = string.Format("[{0:X2}] (undefined)", i);
                    BaseAltitudeKey[i] = altitudes.DefaultLandKey;
                    continue;
                }

                Defined[i] = true;
                Colors[i] = Color.FromArgb(255, terrain.Colour);
                Names[i] = terrain.Name;
                BaseAltitudeKey[i] = altitudes.IsValid(terrain.AltID) ? terrain.AltID : altitudes.DefaultLandKey;
                TileIds[i] = terrain.TileID;
                RandomAltitude[i] = terrain.RandAlt;
            }

            for (int i = 0; i < 256; i++)
            {
                Argb[i] = Colors[i].ToArgb();
            }
        }

        public static TerrainPalette Load(AltitudePalette altitudes)
        {
            ClsTerrainTable table = new ClsTerrainTable();
            table.Load();
            return new TerrainPalette(table, altitudes);
        }

        public IEnumerable<int> DefinedIds
        {
            get
            {
                for (int i = 0; i < 256; i++)
                {
                    if (Defined[i])
                    {
                        yield return i;
                    }
                }
            }
        }

        public string DisplayName(int id) => string.Format("[{0:X2}] {1}", id, Names[id]);

        /// <summary>True for terrain whose default altitude is of type "Water".</summary>
        public bool IsWater(int id, AltitudePalette altitudes)
        {
            return Defined[id] && altitudes.TypeOf(BaseAltitudeKey[id]) == AltitudeType.Water;
        }

        public ColorPalette CreateColorPalette()
        {
            using (Bitmap tmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                ColorPalette palette = tmp.Palette;
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Colors[i];
                }

                return palette;
            }
        }

        /// <summary>Returns the defined terrain whose colour is nearest to <paramref name="color"/>.</summary>
        public byte Nearest(Color color, bool[] allowed = null)
        {
            int key = color.ToArgb() & 0xFFFFFF;
            if (allowed == null)
            {
                lock (_cacheLock)
                {
                    if (_nearestCache.TryGetValue(key, out byte cached))
                    {
                        return cached;
                    }
                }
            }

            int best = -1;
            long bestDist = long.MaxValue;
            for (int i = 0; i < 256; i++)
            {
                if (!Defined[i] || (allowed != null && !allowed[i]))
                {
                    continue;
                }

                Color c = Colors[i];
                // weighted RGB distance (closer to human perception than plain RGB)
                long dr = c.R - color.R;
                long dg = c.G - color.G;
                long db = c.B - color.B;
                long dist = (2 * dr * dr) + (4 * dg * dg) + (3 * db * db);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                }
            }

            byte result = best < 0 ? TerrainIds.NoDraw : (byte)best;
            if (allowed == null)
            {
                lock (_cacheLock)
                {
                    _nearestCache[key] = result;
                }
            }

            return result;
        }
    }

    internal enum AltitudeType
    {
        Water = 0,
        Land = 1,
        Mountain = 2,
        Other = 3,
        Invalid = 255
    }

    /// <summary>
    /// Lookup tables for the altitude palette (index = altitude key, see Data/System/Altitude.xml).
    /// Not every palette index is defined (e.g. 48..63 are gaps). Using an undefined key in the
    /// altitude image was the reason for "Altitude image needs to be rebuilt".
    /// </summary>
    internal sealed class AltitudePalette
    {
        public readonly ClsElevationTable Table;
        public readonly bool[] Defined = new bool[256];
        public readonly short[] Altitude = new short[256];
        public readonly AltitudeType[] Types = new AltitudeType[256];
        public readonly Color[] Colors = new Color[256];
        public readonly int[] Argb = new int[256];

        // per type: sorted list of (altitude, key)
        private readonly List<KeyValuePair<short, byte>>[] _byType = new List<KeyValuePair<short, byte>>[4];

        public short MinAltitude { get; private set; } = short.MaxValue;
        public short MaxAltitude { get; private set; } = short.MinValue;

        public AltitudePalette(ClsElevationTable table)
        {
            Table = table;
            for (int t = 0; t < 4; t++)
            {
                _byType[t] = new List<KeyValuePair<short, byte>>();
            }

            for (int i = 0; i < 256; i++)
            {
                Types[i] = AltitudeType.Invalid;
                Colors[i] = Color.Black;
                ClsElevation elevation = table.GetAltitude(i);
                if (elevation == null)
                {
                    continue;
                }

                Defined[i] = true;
                Altitude[i] = elevation.GetAltitude;
                Colors[i] = Color.FromArgb(255, elevation.AltitudeColor);
                Types[i] = ParseType(elevation.Type);
                _byType[(int)Types[i]].Add(new KeyValuePair<short, byte>(Altitude[i], (byte)i));
                MinAltitude = Math.Min(MinAltitude, Altitude[i]);
                MaxAltitude = Math.Max(MaxAltitude, Altitude[i]);
            }

            for (int t = 0; t < 4; t++)
            {
                _byType[t].Sort((a, b) => a.Key.CompareTo(b.Key));
            }

            for (int i = 0; i < 256; i++)
            {
                Argb[i] = Colors[i].ToArgb();
            }

            if (MinAltitude > MaxAltitude)
            {
                MinAltitude = -120;
                MaxAltitude = 115;
            }
        }

        public static AltitudePalette Load()
        {
            ClsElevationTable table = new ClsElevationTable();
            table.Load();
            return new AltitudePalette(table);
        }

        private static AltitudeType ParseType(string type)
        {
            switch ((type ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "water":
                    return AltitudeType.Water;
                case "land":
                    return AltitudeType.Land;
                case "mountain":
                    return AltitudeType.Mountain;
                default:
                    return AltitudeType.Other;
            }
        }

        public bool IsValid(int key) => key >= 0 && key < 256 && Defined[key];

        public AltitudeType TypeOf(int key) => IsValid(key) ? Types[key] : AltitudeType.Invalid;

        /// <summary>Key for altitude 0 of type Land (88 in the default table).</summary>
        public byte DefaultLandKey
        {
            get
            {
                byte key = Encode(AltitudeType.Land, 0);
                return key;
            }
        }

        /// <summary>Returns the key of the given type whose altitude is nearest to <paramref name="altitude"/>.</summary>
        public byte Encode(AltitudeType type, int altitude)
        {
            List<KeyValuePair<short, byte>> list = type == AltitudeType.Invalid ? null : _byType[(int)type];
            if (list == null || list.Count == 0)
            {
                // fall back to any type that exists
                for (int t = 0; t < 4; t++)
                {
                    if (_byType[t].Count > 0)
                    {
                        list = _byType[t];
                        break;
                    }
                }

                if (list == null || list.Count == 0)
                {
                    return 0;
                }
            }

            int lo = 0;
            int hi = list.Count - 1;
            if (altitude <= list[lo].Key)
            {
                return list[lo].Value;
            }

            if (altitude >= list[hi].Key)
            {
                return list[hi].Value;
            }

            while (hi - lo > 1)
            {
                int mid = (lo + hi) / 2;
                if (list[mid].Key <= altitude)
                {
                    lo = mid;
                }
                else
                {
                    hi = mid;
                }
            }

            return (altitude - list[lo].Key) <= (list[hi].Key - altitude) ? list[lo].Value : list[hi].Value;
        }

        /// <summary>Same type as <paramref name="key"/>, but with the new altitude.</summary>
        public byte WithAltitude(byte key, int altitude)
        {
            AltitudeType type = TypeOf(key);
            if (type == AltitudeType.Invalid)
            {
                type = AltitudeType.Land;
            }

            return Encode(type, altitude);
        }

        /// <summary>Returns a valid key; invalid keys are snapped to the nearest valid key of type Land.</summary>
        public byte Sanitize(byte key)
        {
            if (IsValid(key))
            {
                return key;
            }

            return DefaultLandKey;
        }

        public ColorPalette CreateColorPalette()
        {
            using (Bitmap tmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                ColorPalette palette = tmp.Palette;
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Colors[i];
                }

                return palette;
            }
        }
    }
}
