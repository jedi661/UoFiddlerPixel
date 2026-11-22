/***************************************************************************
 *
 *  * Dragon Map Studio – statics layer of a colour map
 *  *
 *  * A Dragon colour map cannot hold single statics. The layer keeps them next
 *  * to the map file (<map>.statics.bin): position, height above the land,
 *  * item id and hue. The compiler places them 1:1 on the built land height.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public struct LayerStatic
    {
        public int X;
        public int Y;

        /// <summary>Height above the land at (X, Y) – or the absolute height when <see cref="Absolute"/> is set.</summary>
        public int Dz;

        /// <summary>Dz is the absolute height (statics of a whole converted map: exactly like the original, also where the
        /// rebuilt land differs by a few Z).</summary>
        public bool Absolute;

        public ushort Id;
        public ushort Hue;
    }

    public sealed class StaticsLayer
    {
        private const int Magic = 0x4C53_4D44; // "DMSL"

        public List<LayerStatic> Items { get; } = new List<LayerStatic>();

        /// <summary>
        /// Height layer: cells whose colour has no colour of the wanted height in its group (e.g. a wood floor that only
        /// exists with Z 0 painted on grass with Z 5). Key = (x, y); the height is used while the cell still has that colour.
        /// </summary>
        public Dictionary<(int x, int y), (sbyte z, ushort colour)> Heights { get; } = new Dictionary<(int, int), (sbyte, ushort)>();

        /// <summary>Height overrides inside <paramref name="r"/> as index (y * r.Width + x, relative to r) -> (z, colour) for a cut-out build.</summary>
        public Dictionary<int, (sbyte z, ushort colour)> CropHeights(System.Drawing.Rectangle r)
        {
            var d = new Dictionary<int, (sbyte, ushort)>();
            foreach (KeyValuePair<(int x, int y), (sbyte z, ushort colour)> kv in Heights)
            {
                if (r.Contains(kv.Key.x, kv.Key.y))
                {
                    d[(kv.Key.y - r.Y) * r.Width + kv.Key.x - r.X] = kv.Value;
                }
            }
            return d;
        }

        public bool Modified { get; set; }

        /// <summary>The layer holds all statics of the map (converted from a built map): the compiler adds none of its own.</summary>
        public bool Complete { get; set; }

        public static string PathFor(string mapFile) => string.IsNullOrEmpty(mapFile) ? null : mapFile + ".statics.bin";

        public static StaticsLayer Load(string file)
        {
            var layer = new StaticsLayer();
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                return layer;
            }
            using var r = new BinaryReader(File.OpenRead(file));
            if (r.BaseStream.Length < 8 || r.ReadInt32() != Magic)
            {
                return layer;
            }
            int n = r.ReadInt32();
            layer.Complete = (r.ReadInt32() & 1) != 0;
            layer.Items.Capacity = Math.Max(0, n);
            for (int i = 0; i < n && r.BaseStream.Position + 11 <= r.BaseStream.Length; i++)
            {
                var s = new LayerStatic { X = r.ReadUInt16(), Y = r.ReadUInt16(), Dz = r.ReadSByte(), Id = r.ReadUInt16(), Hue = r.ReadUInt16() };
                s.Absolute = (r.ReadUInt16() & 1) != 0; // flags
                layer.Items.Add(s);
            }
            // height layer (since "Höhen 17"): count, then x, y, z, colour
            if (r.BaseStream.Position + 4 <= r.BaseStream.Length)
            {
                int hn = r.ReadInt32();
                for (int i = 0; i < hn && r.BaseStream.Position + 7 <= r.BaseStream.Length; i++)
                {
                    int x = r.ReadUInt16(), y = r.ReadUInt16();
                    sbyte z = r.ReadSByte();
                    ushort c = r.ReadUInt16();
                    layer.Heights[(x, y)] = (z, c);
                }
            }
            return layer;
        }

        public void Save(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return;
            }
            if (Items.Count == 0 && Heights.Count == 0)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                Modified = false;
                return;
            }
            using (var w = new BinaryWriter(File.Create(file)))
            {
                w.Write(Magic);
                w.Write(Items.Count);
                w.Write(Complete ? 1 : 0);
                foreach (LayerStatic s in Items)
                {
                    w.Write((ushort)s.X);
                    w.Write((ushort)s.Y);
                    w.Write((sbyte)Math.Clamp(s.Dz, -128, 127));
                    w.Write(s.Id);
                    w.Write(s.Hue);
                    w.Write((ushort)(s.Absolute ? 1 : 0));
                }
                w.Write(Heights.Count);
                foreach (KeyValuePair<(int x, int y), (sbyte z, ushort colour)> kv in Heights)
                {
                    w.Write((ushort)kv.Key.x);
                    w.Write((ushort)kv.Key.y);
                    w.Write(kv.Value.z);
                    w.Write(kv.Value.colour);
                }
            }
            Modified = false;
        }

        /// <summary>Statics inside <paramref name="r"/>, moved so that r.Location becomes 0,0 (for a cut-out build).</summary>
        public List<LayerStatic> Crop(System.Drawing.Rectangle r)
        {
            var list = new List<LayerStatic>();
            foreach (LayerStatic s in Items)
            {
                if (r.Contains(s.X, s.Y))
                {
                    list.Add(new LayerStatic { X = s.X - r.X, Y = s.Y - r.Y, Dz = s.Dz, Id = s.Id, Hue = s.Hue, Absolute = s.Absolute });
                }
            }
            return list;
        }
    }
}
