/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace UoFiddler.Plugin.Enhanced.Core.TileArt
{
    /// <summary>
    /// Flags of a tileart entry (first flag field). Names taken from the Enhanced Client / UOETE.
    /// </summary>
    [Flags]
    public enum TileArtFlags : ulong
    {
        None = 0x0,
        Background = 0x1,
        Weapon = 0x2,
        Transparent = 0x4,
        Translucent = 0x8,
        Wall = 0x10,
        Damaging = 0x20,
        Impassable = 0x40,
        Wet = 0x80,
        Ignored = 0x100,
        Surface = 0x200,
        Bridge = 0x400,
        Generic = 0x800,
        Window = 0x1000,
        NoShoot = 0x2000,
        ArticleA = 0x4000,
        ArticleAn = 0x8000,
        Mongen = 0x10000,
        Foliage = 0x20000,
        PartialHue = 0x40000,
        UseNewArt = 0x80000,
        Map = 0x100000,
        Container = 0x200000,
        Wearable = 0x400000,
        LightSource = 0x800000,
        Animation = 0x1000000,
        HoverOver = 0x2000000,
        ArtUsed = 0x4000000,
        Armor = 0x8000000,
        Roof = 0x10000000,
        Door = 0x20000000,
        StairBack = 0x40000000,
        StairRight = 0x80000000,
        NoHouse = 0x100000000,
        NoDraw = 0x200000000,
        Unused1 = 0x400000000,
        AlphaBlend = 0x800000000,
        NoShadow = 0x1000000000,
        PixelBleed = 0x2000000000,
        Unused2 = 0x4000000000,
        PlayAnimOnce = 0x8000000000,
        MultiMovable = 0x10000000000
    }

    public enum TileArtPropertyId : byte
    {
        Weight = 0,
        Quality = 1,
        Quantity = 2,
        Height = 3,
        Value = 4,
        AcVc = 5,
        Slot = 6,
        Off_C8 = 7,
        Appearance = 8,
        Race = 9,
        Gender = 10,
        Paperdoll = 11
    }

    public sealed class TileArtProperty
    {
        public TileArtPropertyId Id { get; set; }
        public uint Value { get; set; }

        public override string ToString() => $"{Id} = {Value}";
    }

    public sealed class TileArtStackAlias
    {
        public uint Amount { get; set; }
        public uint ItemId { get; set; }

        public override string ToString() => $"{Amount} -> {ItemId}";
    }

    public struct TileArtImageOffset
    {
        public int XStart { get; set; }
        public int YStart { get; set; }
        public int XEnd { get; set; }
        public int YEnd { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }

        public override string ToString() => $"({XStart},{YStart})-({XEnd},{YEnd}) off ({XOffset},{YOffset})";
    }

    /// <summary>
    /// One image reference inside a texture block. <see cref="NameIndex"/> points into string_dictionary.uop (1 based).
    /// </summary>
    public sealed class TileArtTextureImage
    {
        /// <summary>Offset of <see cref="NameIndex"/> inside <see cref="TileArtEntry.Tail"/> (for in-place changes).</summary>
        internal int TailOffset { get; set; }

        public uint NameIndex { get; set; }
        public byte Unknown1 { get; set; }
        public float Unknown2 { get; set; }
        public uint Unknown3 { get; set; }
        public uint Unknown4 { get; set; }
    }

    /// <summary>
    /// Texture block. There are up to 4 per entry; in the original data slot 0 usually points to Data\WorldArt (EC art),
    /// slot 1 to Data\TileArtLegacy and slot 2 to Data\TileArtEnhanced. Slot 3 is rarely used.
    /// </summary>
    public sealed class TileArtTexture
    {
        public int Slot { get; internal set; }
        public byte Unknown1 { get; internal set; }
        public uint ShaderIndex { get; internal set; }
        public List<TileArtTextureImage> Images { get; } = new List<TileArtTextureImage>();
        public List<int> Unknown8 { get; } = new List<int>();
        public List<float> Unknown9 { get; } = new List<float>();
    }

    /// <summary>
    /// Entry of tileart.uop ("build/tileart/{id:D8}.bin").
    /// Everything up to the radar color is parsed and written back field by field; the data after it
    /// (textures, effects, ...) is kept as raw <see cref="Tail"/> and only patched in place, so nothing gets lost.
    /// </summary>
    public sealed class TileArtEntry
    {
        public ushort Version { get; set; } = 4;
        public uint NameIndex { get; set; }
        public uint TileId { get; set; }
        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }
        public float Unknown3 { get; set; }
        public float Unknown4 { get; set; }
        public int FixedZero { get; set; }
        public int OldId { get; set; }
        public float Unknown5 { get; set; }
        public int Type { get; set; }
        public byte Unknown6 { get; set; }
        public int Unknown7 { get; set; }
        public int Unknown8 { get; set; }
        public float Light1 { get; set; }
        public float Light2 { get; set; }
        public uint Unknown9 { get; set; }
        public TileArtFlags Flags { get; set; }
        public ulong Flags2 { get; set; }
        public int Facing { get; set; }
        public TileArtImageOffset EcImageOffset { get; set; }
        public TileArtImageOffset CcImageOffset { get; set; }
        public List<TileArtProperty> Properties { get; } = new List<TileArtProperty>();
        public List<TileArtProperty> Properties2 { get; } = new List<TileArtProperty>();
        public List<TileArtStackAlias> StackAliases { get; } = new List<TileArtStackAlias>();

        /// <summary>Raw appearance block (count + data), kept as is.</summary>
        public byte[] AppearanceData { get; set; } = new byte[4];

        public bool HasSitting { get; set; }
        public uint[] Sitting { get; } = new uint[4];

        public byte RadarR { get; set; }
        public byte RadarG { get; set; }
        public byte RadarB { get; set; }
        public byte RadarA { get; set; }

        /// <summary>Everything after the radar color (textures and following data).</summary>
        public byte[] Tail { get; set; } = Array.Empty<byte>();

        /// <summary>Texture blocks parsed from <see cref="Tail"/> (read-only view; may be incomplete for unknown layouts).</summary>
        public List<TileArtTexture> Textures { get; } = new List<TileArtTexture>();

        /// <summary>False when the texture blocks couldn't be parsed completely.</summary>
        public bool TexturesComplete { get; private set; }

        public static TileArtEntry Read(byte[] data)
        {
            var e = new TileArtEntry();
            using (var r = new BinaryReader(new MemoryStream(data)))
            {
                e.Version = r.ReadUInt16();
                e.NameIndex = r.ReadUInt32();
                e.TileId = r.ReadUInt32();
                e.Unknown1 = r.ReadByte();
                e.Unknown2 = r.ReadByte();
                e.Unknown3 = r.ReadSingle();
                e.Unknown4 = r.ReadSingle();
                e.FixedZero = r.ReadInt32();
                e.OldId = r.ReadInt32();
                e.Unknown5 = r.ReadSingle();
                e.Type = r.ReadInt32();
                e.Unknown6 = r.ReadByte();
                e.Unknown7 = r.ReadInt32();
                e.Unknown8 = r.ReadInt32();
                e.Light1 = r.ReadSingle();
                e.Light2 = r.ReadSingle();
                e.Unknown9 = r.ReadUInt32();
                e.Flags = (TileArtFlags)r.ReadUInt64();
                e.Flags2 = r.ReadUInt64();
                e.Facing = r.ReadInt32();
                e.EcImageOffset = ReadOffset(r);
                e.CcImageOffset = ReadOffset(r);

                byte count = r.ReadByte();
                for (int i = 0; i < count; i++)
                {
                    e.Properties.Add(new TileArtProperty { Id = (TileArtPropertyId)r.ReadByte(), Value = r.ReadUInt32() });
                }
                count = r.ReadByte();
                for (int i = 0; i < count; i++)
                {
                    e.Properties2.Add(new TileArtProperty { Id = (TileArtPropertyId)r.ReadByte(), Value = r.ReadUInt32() });
                }

                uint aliases = r.ReadUInt32();
                for (int i = 0; i < aliases; i++)
                {
                    e.StackAliases.Add(new TileArtStackAlias { Amount = r.ReadUInt32(), ItemId = r.ReadUInt32() });
                }

                long appearanceStart = r.BaseStream.Position;
                uint appearances = r.ReadUInt32();
                for (int i = 0; i < appearances; i++)
                {
                    byte subType = r.ReadByte();
                    if (subType == 1)
                    {
                        r.ReadByte();
                        r.ReadUInt32();
                    }
                    else
                    {
                        uint sub = r.ReadUInt32();
                        for (int s = 0; s < sub; s++)
                        {
                            r.ReadUInt32();
                            r.ReadUInt32();
                        }
                    }
                }
                long appearanceEnd = r.BaseStream.Position;
                e.AppearanceData = new byte[appearanceEnd - appearanceStart];
                Buffer.BlockCopy(data, (int)appearanceStart, e.AppearanceData, 0, e.AppearanceData.Length);

                e.HasSitting = r.ReadByte() != 0;
                if (e.HasSitting)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        e.Sitting[i] = r.ReadUInt32();
                    }
                }

                e.RadarR = r.ReadByte();
                e.RadarG = r.ReadByte();
                e.RadarB = r.ReadByte();
                e.RadarA = r.ReadByte();

                int tailStart = (int)r.BaseStream.Position;
                e.Tail = new byte[data.Length - tailStart];
                Buffer.BlockCopy(data, tailStart, e.Tail, 0, e.Tail.Length);
            }

            e.ParseTextures();
            return e;
        }

        private static TileArtImageOffset ReadOffset(BinaryReader r)
        {
            return new TileArtImageOffset
            {
                XStart = r.ReadInt32(),
                YStart = r.ReadInt32(),
                XEnd = r.ReadInt32(),
                YEnd = r.ReadInt32(),
                XOffset = r.ReadInt32(),
                YOffset = r.ReadInt32()
            };
        }

        private static void WriteOffset(BinaryWriter w, TileArtImageOffset o)
        {
            w.Write(o.XStart);
            w.Write(o.YStart);
            w.Write(o.XEnd);
            w.Write(o.YEnd);
            w.Write(o.XOffset);
            w.Write(o.YOffset);
        }

        /// <summary>
        /// Parses the texture blocks of the tail (best effort, the tail itself is never changed by this).
        /// </summary>
        public void ParseTextures()
        {
            Textures.Clear();
            TexturesComplete = false;
            byte[] t = Tail;
            int p = 0;

            try
            {
                for (int slot = 0; slot < 4; slot++)
                {
                    byte has = t[p++];
                    if (has == 0)
                    {
                        continue;
                    }
                    if (has != 1)
                    {
                        return;
                    }

                    var tex = new TileArtTexture { Slot = slot };
                    tex.Unknown1 = t[p++];
                    tex.ShaderIndex = BitConverter.ToUInt32(t, p); p += 4;
                    byte images = t[p++];
                    for (int i = 0; i < images; i++)
                    {
                        var img = new TileArtTextureImage { TailOffset = p };
                        img.NameIndex = BitConverter.ToUInt32(t, p); p += 4;
                        img.Unknown1 = t[p++];
                        img.Unknown2 = BitConverter.ToSingle(t, p); p += 4;
                        img.Unknown3 = BitConverter.ToUInt32(t, p); p += 4;
                        img.Unknown4 = BitConverter.ToUInt32(t, p); p += 4;
                        tex.Images.Add(img);
                    }
                    uint n8 = BitConverter.ToUInt32(t, p); p += 4;
                    for (int i = 0; i < n8; i++)
                    {
                        tex.Unknown8.Add(BitConverter.ToInt32(t, p)); p += 4;
                    }
                    uint n9 = BitConverter.ToUInt32(t, p); p += 4;
                    for (int i = 0; i < n9; i++)
                    {
                        tex.Unknown9.Add(BitConverter.ToSingle(t, p)); p += 4;
                    }
                    Textures.Add(tex);
                }
                TexturesComplete = true;
            }
            catch (ArgumentException)
            {
                // malformed or unknown layout – keep what was parsed
            }
            catch (IndexOutOfRangeException)
            {
            }
        }

        /// <summary>
        /// Changes the string dictionary reference of a texture image (written in place into <see cref="Tail"/>).
        /// </summary>
        public void SetTextureNameIndex(TileArtTextureImage image, uint nameIndex)
        {
            if (image.TailOffset < 0 || image.TailOffset + 4 > Tail.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(image));
            }

            BitConverter.GetBytes(nameIndex).CopyTo(Tail, image.TailOffset);
            image.NameIndex = nameIndex;
        }

        public byte[] Write()
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(Version);
                w.Write(NameIndex);
                w.Write(TileId);
                w.Write(Unknown1);
                w.Write(Unknown2);
                w.Write(Unknown3);
                w.Write(Unknown4);
                w.Write(FixedZero);
                w.Write(OldId);
                w.Write(Unknown5);
                w.Write(Type);
                w.Write(Unknown6);
                w.Write(Unknown7);
                w.Write(Unknown8);
                w.Write(Light1);
                w.Write(Light2);
                w.Write(Unknown9);
                w.Write((ulong)Flags);
                w.Write(Flags2);
                w.Write(Facing);
                WriteOffset(w, EcImageOffset);
                WriteOffset(w, CcImageOffset);

                w.Write((byte)Properties.Count);
                foreach (TileArtProperty p in Properties)
                {
                    w.Write((byte)p.Id);
                    w.Write(p.Value);
                }
                w.Write((byte)Properties2.Count);
                foreach (TileArtProperty p in Properties2)
                {
                    w.Write((byte)p.Id);
                    w.Write(p.Value);
                }

                w.Write((uint)StackAliases.Count);
                foreach (TileArtStackAlias a in StackAliases)
                {
                    w.Write(a.Amount);
                    w.Write(a.ItemId);
                }

                w.Write(AppearanceData);

                w.Write((byte)(HasSitting ? 1 : 0));
                if (HasSitting)
                {
                    foreach (uint s in Sitting)
                    {
                        w.Write(s);
                    }
                }

                w.Write(RadarR);
                w.Write(RadarG);
                w.Write(RadarB);
                w.Write(RadarA);
                w.Write(Tail);
                w.Flush();
                return ms.ToArray();
            }
        }

        public TileArtEntry Clone()
        {
            TileArtEntry copy = Read(Write());
            return copy;
        }

        public uint GetProperty(TileArtPropertyId id, uint defaultValue = 0)
        {
            foreach (TileArtProperty p in Properties)
            {
                if (p.Id == id)
                {
                    return p.Value;
                }
            }
            return defaultValue;
        }
    }
}
