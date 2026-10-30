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
using System.Linq;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Multis
{
    public sealed class EcMultiPart
    {
        public ushort ItemId { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        /// <summary>Flags (0 = background, 1 = none, 256/257 = generic/visible in the classic data).</summary>
        public ushort Flags { get; set; }
        public List<uint> Clilocs { get; } = new List<uint>();

        public EcMultiPart Clone()
        {
            var copy = new EcMultiPart { ItemId = ItemId, X = X, Y = Y, Z = Z, Flags = Flags };
            copy.Clilocs.AddRange(Clilocs);
            return copy;
        }
    }

    /// <summary>
    /// Multi (house, boat, ...) of MultiCollection.uop ("build/multicollection/{id:D6}.bin").
    /// Layout: int id, int count, parts (ushort item, short x, y, z, ushort flags, int cliloc count, uint clilocs).
    /// </summary>
    public sealed class EcMulti
    {
        public const string NamePattern = "build/multicollection/{0:D6}.bin";

        public int Id { get; set; }
        public List<EcMultiPart> Parts { get; } = new List<EcMultiPart>();

        /// <summary>Bytes after the parts (none in the original files).</summary>
        public byte[] Tail { get; set; } = Array.Empty<byte>();

        public static EcMulti Read(byte[] data)
        {
            using (var r = new BinaryReader(new MemoryStream(data)))
            {
                var multi = new EcMulti { Id = r.ReadInt32() };
                int count = r.ReadInt32();
                for (int i = 0; i < count && r.BaseStream.Position + 12 <= data.Length; i++)
                {
                    var part = new EcMultiPart
                    {
                        ItemId = r.ReadUInt16(),
                        X = r.ReadInt16(),
                        Y = r.ReadInt16(),
                        Z = r.ReadInt16(),
                        Flags = r.ReadUInt16()
                    };
                    int clilocs = r.ReadInt32();
                    for (int c = 0; c < clilocs; c++)
                    {
                        part.Clilocs.Add(r.ReadUInt32());
                    }
                    multi.Parts.Add(part);
                }
                multi.Tail = r.ReadBytes((int)(data.Length - r.BaseStream.Position));
                return multi;
            }
        }

        public byte[] Write()
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(Id);
                w.Write(Parts.Count);
                foreach (EcMultiPart p in Parts)
                {
                    w.Write(p.ItemId);
                    w.Write(p.X);
                    w.Write(p.Y);
                    w.Write(p.Z);
                    w.Write(p.Flags);
                    w.Write(p.Clilocs.Count);
                    foreach (uint c in p.Clilocs)
                    {
                        w.Write(c);
                    }
                }
                w.Write(Tail);
                w.Flush();
                return ms.ToArray();
            }
        }

        public (int MinX, int MinY, int MaxX, int MaxY, int MinZ, int MaxZ) Bounds()
        {
            if (Parts.Count == 0)
            {
                return (0, 0, 0, 0, 0, 0);
            }
            return (Parts.Min(p => p.X), Parts.Min(p => p.Y), Parts.Max(p => p.X), Parts.Max(p => p.Y), Parts.Min(p => p.Z), Parts.Max(p => p.Z));
        }
    }
}
