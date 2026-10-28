/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Ultima;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// A static of a <see cref="MapClip"/>: position relative to the clip, z absolute (as on the source map)
    /// </summary>
    public struct ClipStatic
    {
        public ClipStatic(int x, int y, ushort id, sbyte z, short hue)
        {
            X = x;
            Y = y;
            Id = id;
            Z = z;
            Hue = hue;
        }

        public int X;
        public int Y;
        public ushort Id;
        public sbyte Z;
        public short Hue;
    }

    /// <summary>
    /// A copied rectangle of a map: land (id + height of every cell) and the statics.
    /// Independent of the source map, so it can be pasted on any map.
    /// </summary>
    public sealed class MapClip
    {
        public MapClip(int width, int height)
        {
            Width = width;
            Height = height;
            LandIds = new ushort[width * height];
            LandZ = new sbyte[width * height];
        }

        public string SourceName { get; set; } = string.Empty;

        /// <summary>source rectangle on the source map</summary>
        public Rectangle SourceArea { get; set; }

        public int Width { get; }
        public int Height { get; }

        public ushort[] LandIds { get; }
        public sbyte[] LandZ { get; }

        public List<ClipStatic> Statics { get; } = new List<ClipStatic>();

        public ushort GetLandId(int x, int y) => LandIds[(y * Width) + x];

        public sbyte GetLandZ(int x, int y) => LandZ[(y * Width) + x];

        public void SetLand(int x, int y, ushort id, sbyte z)
        {
            LandIds[(y * Width) + x] = id;
            LandZ[(y * Width) + x] = z;
        }

        public int MinLandZ => LandZ.Length == 0 ? 0 : LandZ.Min(z => (int)z);
        public int MaxLandZ => LandZ.Length == 0 ? 0 : LandZ.Max(z => (int)z);

        /// <summary>
        /// Copies a rectangle of a map (land + statics including changes in memory of the session)
        /// </summary>
        public static MapClip FromMap(Map map, Rectangle area, RegionEditSession session, string sourceName)
        {
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (area.Width <= 0 || area.Height <= 0)
            {
                throw new ArgumentException("The area is outside of the map.");
            }

            var clip = new MapClip(area.Width, area.Height) { SourceArea = area, SourceName = sourceName };
            TileMatrix tiles = map.Tiles;
            for (int y = 0; y < area.Height; y++)
            {
                for (int x = 0; x < area.Width; x++)
                {
                    Tile t = tiles.GetLandTile(area.X + x, area.Y + y);
                    clip.SetLand(x, y, t.Id, t.Z);
                }
            }

            RegionEditSession reader = session ?? new RegionEditSession(map);
            foreach (MapStatic s in reader.GetStaticsInArea(area.Left, area.Top, area.Right - 1, area.Bottom - 1))
            {
                clip.Statics.Add(new ClipStatic(s.X - area.X, s.Y - area.Y, s.Id, s.Z, s.Hue));
            }

            tiles.CloseStreams();
            return clip;
        }

        #region Transform (positions; directional item ids are not exchanged)

        /// <summary>Turns the clip by 90 degrees clockwise (map view)</summary>
        public MapClip Rotate90()
        {
            var result = new MapClip(Height, Width) { SourceName = SourceName, SourceArea = SourceArea };
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int nx = Height - 1 - y, ny = x;
                    result.SetLand(nx, ny, GetLandId(x, y), GetLandZ(x, y));
                }
            }

            foreach (ClipStatic s in Statics)
            {
                result.Statics.Add(new ClipStatic(Height - 1 - s.Y, s.X, s.Id, s.Z, s.Hue));
            }

            return result;
        }

        /// <summary>Mirrors the clip horizontally (x)</summary>
        public MapClip MirrorX()
        {
            var result = new MapClip(Width, Height) { SourceName = SourceName, SourceArea = SourceArea };
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    result.SetLand(Width - 1 - x, y, GetLandId(x, y), GetLandZ(x, y));
                }
            }

            foreach (ClipStatic s in Statics)
            {
                result.Statics.Add(new ClipStatic(Width - 1 - s.X, s.Y, s.Id, s.Z, s.Hue));
            }

            return result;
        }

        /// <summary>Mirrors the clip vertically (y)</summary>
        public MapClip MirrorY()
        {
            var result = new MapClip(Width, Height) { SourceName = SourceName, SourceArea = SourceArea };
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    result.SetLand(x, Height - 1 - y, GetLandId(x, y), GetLandZ(x, y));
                }
            }

            foreach (ClipStatic s in Statics)
            {
                result.Statics.Add(new ClipStatic(s.X, Height - 1 - s.Y, s.Id, s.Z, s.Hue));
            }

            return result;
        }

        #endregion

        public override string ToString()
        {
            return $"{SourceName} {SourceArea.X},{SourceArea.Y} ({Width} x {Height}, {Statics.Count} statics)";
        }
    }
}
