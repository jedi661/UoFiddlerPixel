/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Maps
{
    /// <summary>
    /// Additional terrain layer of a map tile (the Enhanced Client blends several terrains on one tile).
    /// </summary>
    public sealed class EcLandLayer
    {
        /// <summary>Blend / edge index (0..7 in the original data).</summary>
        public byte Blend { get; set; }
        public sbyte Z { get; set; }
        public ushort LandId { get; set; }
        /// <summary>Always 0 in the original data, kept as it is.</summary>
        public ushort Unknown { get; set; }

        public EcLandLayer Clone() => (EcLandLayer)MemberwiseClone();
    }

    /// <summary>
    /// Static item placed on a map tile.
    /// </summary>
    public sealed class EcStatic
    {
        public uint ItemId { get; set; }
        public sbyte Z { get; set; }
        public uint Hue { get; set; }

        public EcStatic Clone() => (EcStatic)MemberwiseClone();

        public override string ToString() => $"0x{ItemId:X4} z {Z}" + (Hue != 0 ? $" hue {Hue}" : string.Empty);
    }

    /// <summary>
    /// One map tile: land (z, id), optional blend layers and statics.
    /// </summary>
    public sealed class EcMapTile
    {
        public sbyte Z { get; set; }
        public ushort LandId { get; set; }
        public List<EcLandLayer> Layers { get; } = new List<EcLandLayer>();
        public List<EcStatic> Statics { get; } = new List<EcStatic>();

        public EcMapTile Clone()
        {
            var copy = new EcMapTile { Z = Z, LandId = LandId };
            foreach (EcLandLayer l in Layers)
            {
                copy.Layers.Add(l.Clone());
            }
            foreach (EcStatic s in Statics)
            {
                copy.Statics.Add(s.Clone());
            }
            return copy;
        }
    }

    /// <summary>
    /// A 64 x 64 tile block of an Enhanced Client map ("build/sectors/facet_{facet:D2}/{index:D8}.bin" in facetN.uop).
    ///
    /// Layout: byte facet, ushort sector index, then 4096 tiles column by column (tile i is at x = i / 64, y = i % 64):
    /// sbyte z, ushort land id, byte layer count, layers (byte blend, sbyte z, ushort land id, ushort 0),
    /// byte static count, statics (uint item id, sbyte z, uint hue).
    /// Sectors are numbered column by column too: x = index / (map height / 64), y = index % (map height / 64).
    /// Reading and writing reproduces the original data byte by byte.
    /// </summary>
    public sealed class EcSector
    {
        public const int Size = 64;
        public const int TileCount = Size * Size;

        public byte Facet { get; set; }
        public ushort Index { get; set; }
        public EcMapTile[] Tiles { get; } = new EcMapTile[TileCount];

        /// <summary>Tile at the position inside the sector (0..63).</summary>
        public EcMapTile this[int x, int y] => Tiles[(x * Size) + y];

        public static EcSector Read(byte[] data)
        {
            var sector = new EcSector { Facet = data[0], Index = BitConverter.ToUInt16(data, 1) };
            int p = 3;
            for (int i = 0; i < TileCount; i++)
            {
                var tile = new EcMapTile
                {
                    Z = (sbyte)data[p],
                    LandId = BitConverter.ToUInt16(data, p + 1)
                };
                int layers = data[p + 3];
                p += 4;
                for (int l = 0; l < layers; l++)
                {
                    tile.Layers.Add(new EcLandLayer
                    {
                        Blend = data[p],
                        Z = (sbyte)data[p + 1],
                        LandId = BitConverter.ToUInt16(data, p + 2),
                        Unknown = BitConverter.ToUInt16(data, p + 4)
                    });
                    p += 6;
                }

                int statics = data[p++];
                for (int s = 0; s < statics; s++)
                {
                    tile.Statics.Add(new EcStatic
                    {
                        ItemId = BitConverter.ToUInt32(data, p),
                        Z = (sbyte)data[p + 4],
                        Hue = BitConverter.ToUInt32(data, p + 5)
                    });
                    p += 9;
                }
                sector.Tiles[i] = tile;
            }

            if (p != data.Length)
            {
                throw new InvalidDataException($"Sector {sector.Index}: {data.Length - p} bytes left after the tiles.");
            }
            return sector;
        }

        public byte[] Write()
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(Facet);
                w.Write(Index);
                foreach (EcMapTile tile in Tiles)
                {
                    if (tile.Layers.Count > 255 || tile.Statics.Count > 255)
                    {
                        throw new InvalidOperationException("A tile can hold at most 255 layers and 255 statics.");
                    }
                    w.Write(tile.Z);
                    w.Write(tile.LandId);
                    w.Write((byte)tile.Layers.Count);
                    foreach (EcLandLayer l in tile.Layers)
                    {
                        w.Write(l.Blend);
                        w.Write(l.Z);
                        w.Write(l.LandId);
                        w.Write(l.Unknown);
                    }
                    w.Write((byte)tile.Statics.Count);
                    foreach (EcStatic s in tile.Statics)
                    {
                        w.Write(s.ItemId);
                        w.Write(s.Z);
                        w.Write(s.Hue);
                    }
                }
                w.Flush();
                return ms.ToArray();
            }
        }
    }
}
