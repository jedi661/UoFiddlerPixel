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
using System.Globalization;
using System.Linq;
using UoFiddler.Plugin.Enhanced.EcUltima.Gumps;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.Strings;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Art
{
    /// <summary>Which images an item gets in the Enhanced Client.</summary>
    public enum ArtTarget
    {
        /// <summary>Legacy texture only (classic size); the client enlarges it itself in the enhanced view.</summary>
        LegacyOnly = 0,

        /// <summary>Legacy texture plus an EC image (world art, 1.5 x) – sharper, chosen enlarging.</summary>
        LegacyAndEc = 1
    }

    /// <summary>Classic tiledata of an item (filled by the UI from the loaded classic client).</summary>
    public sealed class ClassicItemInfo
    {
        public string Name { get; set; }

        /// <summary>Classic tile flags (64 bit, positions as in tiledata.mul).</summary>
        public ulong Flags { get; set; }

        public byte Weight { get; set; }
        public byte Quality { get; set; }
        public byte Quantity { get; set; }
        public byte Value { get; set; }
        public byte Height { get; set; }
        public short Animation { get; set; }

        /// <summary>Radar color (RGB555) from radarcol.mul.</summary>
        public ushort RadarColor { get; set; }
    }

    public sealed class ArtConvertOptions
    {
        public ArtTarget Target { get; set; } = ArtTarget.LegacyAndEc;
        public ArtScaleMode Scale { get; set; } = ArtScaleMode.Smooth;

        /// <summary>Write name, flags, weight, height ... from the classic tiledata (always done for new entries).</summary>
        public bool ItemData { get; set; } = true;

        /// <summary>Uncompressed DDS (lossless, 4 times larger) instead of DXT5 like the client files.</summary>
        public bool Uncompressed { get; set; }
    }

    /// <summary>State of an item in the Enhanced Client.</summary>
    public enum EcItemState
    {
        /// <summary>No tileart entry – the EC doesn't know the item.</summary>
        Missing,

        /// <summary>Tileart entry but no texture (the item is invisible).</summary>
        NoImage,

        /// <summary>Only the legacy texture (the client enlarges it).</summary>
        LegacyOnly,

        /// <summary>Own EC image (world art).</summary>
        EcImage
    }

    /// <summary>
    /// Classic art / gumps → Enhanced Client, the way the client files are built:
    /// <list type="bullet">
    /// <item>LegacyTexture.uop "build/tileartlegacy/{id:D8}.dds": the classic image, at least 44 pixels wide
    /// (narrower images centered), power of two texture, DXT5.</item>
    /// <item>Texture.uop "build/worldart/{id:D8}.dds": the image enlarged 1.5 times (optional).</item>
    /// <item>tileart.uop: slot 0 "Data\WorldArt\{id:D8}.tga" (only with EC image), slot 1 "Data\TileArtLegacy\{id}.tga",
    /// slot 2 "Data\TileArtEnhanced\{id}.tga", each with UOSpriteShader and the noise texture; CC area = legacy image,
    /// EC area = world art image placed where the classic client draws the item (bottom center on the tile).</item>
    /// <item>Flags2 = classic flags, Flags = the same without the bits the client keeps in Flags2 only
    /// (transparent, translucent, damaging, articles, 0x80000, 0x4000000) – as in the original tileart.</item>
    /// <item>gumpartLegacyMUL.uop "build/gumpartlegacymul/{id:D8}.tga": classic gump (RLE).</item>
    /// </list>
    /// </summary>
    public sealed class EcArtConverter
    {
        public const string LegacyFile = EcClient.LegacyTextureFileName;
        public const string WorldArtFile = EcClient.TextureFileName;
        public const string GumpFile = "gumpartLegacyMUL.uop";
        public const string LegacyPattern = "build/tileartlegacy/{0:D8}.dds";
        public const string WorldArtPattern = "build/worldart/{0:D8}.dds";
        public const string GumpPattern = "build/gumpartlegacymul/{0:D8}.tga";
        public const string ShaderName = "UOSpriteShader";
        public const string NoiseTexture = "Data\\WorldArt\\01000045_noise.tga";

        /// <summary>Width of the legacy texture at least (one tile).</summary>
        public const int LegacyMinWidth = 44;

        /// <summary>Flag bits the original tileart keeps in Flags2 only.</summary>
        public const ulong Flags2Only = 0x4UL | 0x8UL | 0x20UL | 0x4000UL | 0x8000UL | 0x80000UL | 0x4000000UL;

        /// <summary>Classic flag bits that are written (the 32 classic bits and NoShadow, PixelBleed, MultiMovable).</summary>
        public const ulong ClassicFlagMask = 0xFFFFFFFFUL | 0x1000000000UL | 0x2000000000UL | 0x10000000000UL;

        private readonly EcClient _client;

        public EcArtConverter(EcClient client)
        {
            _client = client;
        }

        public static string WorldArtPath(uint id) => string.Format(CultureInfo.InvariantCulture, "Data\\WorldArt\\{0:D8}.tga", id);
        public static string LegacyPath(uint id) => string.Format(CultureInfo.InvariantCulture, "Data\\TileArtLegacy\\{0}.tga", id);
        public static string EnhancedPath(uint id) => string.Format(CultureInfo.InvariantCulture, "Data\\TileArtEnhanced\\{0}.tga", id);

        // ==================================================================== images

        /// <summary>The legacy texture image: at least 44 wide, narrower images centered like the classic client draws them.</summary>
        public static byte[] LegacyCanvas(byte[] bgra, int w, int h, out int canvasWidth)
        {
            canvasWidth = Math.Max(LegacyMinWidth, w);
            if (canvasWidth == w)
            {
                return bgra;
            }
            int left = (LegacyMinWidth / 2) - (w / 2);
            var canvas = new byte[canvasWidth * h * 4];
            for (int y = 0; y < h; y++)
            {
                Buffer.BlockCopy(bgra, y * w * 4, canvas, ((y * canvasWidth) + left) * 4, w * 4);
            }
            return canvas;
        }

        /// <summary>Where the EC image goes: classic left edge (22 - w / 2) times 1.5, bottom on the bottom corner of the tile.</summary>
        public static TileArtImageOffset EcOffset(int classicWidth, int ecWidth, int ecHeight)
        {
            int classicLeft = (LegacyMinWidth / 2) - (classicWidth / 2);
            return new TileArtImageOffset
            {
                XStart = 0,
                YStart = 0,
                XEnd = ecWidth,
                YEnd = ecHeight,
                XOffset = (int)Math.Round(classicLeft * EcArtScaler.EcScale, MidpointRounding.AwayFromZero),
                YOffset = 0
            };
        }

        private static byte[] ToDds(byte[] bgra, int w, int h, bool uncompressed)
        {
            byte[] padded = EcArtScaler.PadPowerOfTwo(bgra, w, h, out int pw, out int ph);
            return DdsImage.Encode(padded, pw, ph, uncompressed ? DdsSaveFormat.A8R8G8B8 : DdsSaveFormat.Dxt5, 1);
        }

        // ==================================================================== flags / data

        public static ulong EcFlags2(ulong classic) => classic & ClassicFlagMask;

        public static ulong EcFlags(ulong classic) => classic & ClassicFlagMask & ~Flags2Only;

        /// <summary>Tileart properties from the classic data (ascending id, only values above 0, like the client files).</summary>
        public static List<TileArtProperty> Properties(ClassicItemInfo info)
        {
            var list = new List<TileArtProperty>();
            void Add(TileArtPropertyId id, uint value)
            {
                if (value > 0)
                {
                    list.Add(new TileArtProperty { Id = id, Value = value });
                }
            }
            bool wearable = (info.Flags & 0x400000UL) != 0;
            Add(TileArtPropertyId.Weight, info.Weight);
            if (!wearable)
            {
                Add(TileArtPropertyId.Quality, info.Quality);
            }
            Add(TileArtPropertyId.Quantity, info.Quantity);
            Add(TileArtPropertyId.Height, info.Height);
            Add(TileArtPropertyId.Value, info.Value);
            if (wearable)
            {
                Add(TileArtPropertyId.Layer, info.Quality);
            }
            if (wearable || (info.Flags & 0x1000000UL) != 0)
            {
                Add(TileArtPropertyId.Animation, (uint)Math.Max((short)0, info.Animation));
            }
            return list;
        }

        // ==================================================================== state

        /// <summary>How the item exists in the Enhanced Client (tileart, legacy texture, EC image).</summary>
        public EcItemState GetState(uint id)
        {
            TileArtRecord record = _client.GetTileArt()?.Get(id);
            if (record == null)
            {
                return EcItemState.Missing;
            }
            TileArtEntry e = record.Entry;
            bool ec = e.Textures.Any(t => t.Slot == 0) && e.EcImageOffset.XEnd > e.EcImageOffset.XStart;
            if (ec)
            {
                return EcItemState.EcImage;
            }
            EcUopPackage legacy = _client.GetPackage(LegacyFile);
            bool hasLegacy = legacy?.GetEntry(string.Format(CultureInfo.InvariantCulture, LegacyPattern, id)) != null;
            return hasLegacy ? EcItemState.LegacyOnly : EcItemState.NoImage;
        }

        public static string StateText(EcItemState state) => state switch
        {
            EcItemState.Missing => "missing in EC",
            EcItemState.NoImage => "no image",
            EcItemState.LegacyOnly => "legacy only",
            _ => "EC image"
        };

        // ==================================================================== items

        /// <summary>
        /// Writes an item into the Enhanced Client: legacy texture, (EC image), tileart entry and texture names.
        /// The packages are changed in memory; save them afterwards (tileart and string dictionary: call Apply first).
        /// </summary>
        /// <param name="ecBgra">Optional: a ready EC image (already 1.5 x) instead of enlarging <paramref name="bgra"/>.</param>
        public string ConvertItem(uint id, byte[] bgra, int w, int h, ClassicItemInfo info, ArtConvertOptions options, byte[] ecBgra = null, int ecW = 0, int ecH = 0)
        {
            if (w <= 0 || h <= 0)
            {
                throw new ArgumentException($"Item {id}: the image is empty.");
            }
            EcUopPackage legacyPackage = _client.GetPackage(LegacyFile) ?? throw new InvalidOperationException(LegacyFile + " was not found.");
            TileArtFile tileArt = _client.GetTileArt() ?? throw new InvalidOperationException(EcClient.TileArtFileName + " was not found.");
            EcStringDictionary dict = _client.StringDictionary ?? throw new InvalidOperationException(EcClient.StringDictionaryFileName + " was not found.");
            bool withEc = options.Target == ArtTarget.LegacyAndEc;
            EcUopPackage worldPackage = withEc ? _client.GetPackage(WorldArtFile) ?? throw new InvalidOperationException(WorldArtFile + " was not found.") : null;

            // 1. legacy texture
            byte[] canvas = LegacyCanvas(bgra, w, h, out int canvasWidth);
            legacyPackage.AddOrReplace(string.Format(CultureInfo.InvariantCulture, LegacyPattern, id), ToDds(canvas, canvasWidth, h, options.Uncompressed), EcUopCompression.Zlib);

            // 2. EC image
            TileArtImageOffset ecOffset = default;
            if (withEc)
            {
                int sw = ecW, sh = ecH;
                byte[] scaled = ecBgra ?? EcArtScaler.Scale(bgra, w, h, options.Scale, out sw, out sh);
                worldPackage.AddOrReplace(string.Format(CultureInfo.InvariantCulture, WorldArtPattern, id), ToDds(scaled, sw, sh, options.Uncompressed), EcUopCompression.Zlib);
                ecOffset = EcOffset(w, sw, sh);
            }

            // 3. tileart
            TileArtRecord record = tileArt.Get(id);
            bool created = record == null;
            TileArtEntry entry = created ? NewEntry(id) : record.Entry;
            if (!entry.TexturesComplete)
            {
                throw new InvalidOperationException($"Item {id}: the texture blocks of the tileart entry can't be read – not changed.");
            }

            uint shader = dict.GetOrAdd(ShaderName);
            uint noise = dict.GetOrAdd(NoiseTexture);
            var textures = entry.Textures.Where(t => t.Slot == 3).Select(t => TileArtEntry.CloneTexture(t, t.Slot)).ToList();
            SetSlot(textures, 0, withEc ? dict.GetOrAdd(WorldArtPath(id)) : 0, shader, noise, withEc);
            SetSlot(textures, 1, dict.GetOrAdd(LegacyPath(id)), shader, noise, true);
            SetSlot(textures, 2, dict.GetOrAdd(EnhancedPath(id)), shader, noise, true);
            entry.SetTextures(textures);

            entry.CcImageOffset = new TileArtImageOffset { XStart = 0, YStart = 0, XEnd = canvasWidth, YEnd = h };
            entry.EcImageOffset = withEc ? ecOffset : default;
            entry.Unknown1 = (byte)(withEc ? 1 : 0);

            if (info != null && (created || options.ItemData))
            {
                if (!string.IsNullOrWhiteSpace(info.Name))
                {
                    entry.NameIndex = dict.GetOrAdd(info.Name);
                }
                entry.Flags2 = EcFlags2(info.Flags);
                entry.Flags = (TileArtFlags)EcFlags(info.Flags);
                List<TileArtProperty> props = Properties(info);
                entry.Properties.Clear();
                entry.Properties.AddRange(props);
                entry.Properties2.Clear();
                entry.Properties2.AddRange(props.Select(p => new TileArtProperty { Id = p.Id, Value = p.Value }));
                entry.RadarR = (byte)(((info.RadarColor >> 10) & 0x1F) * 8);
                entry.RadarG = (byte)(((info.RadarColor >> 5) & 0x1F) * 8);
                entry.RadarB = (byte)((info.RadarColor & 0x1F) * 8);
                entry.RadarA = 255;
            }

            if (created)
            {
                tileArt.AddEntry(entry, id);
            }
            else
            {
                tileArt.MarkModified(record);
            }
            return $"Item {id} (0x{id:X4}): " + (created ? "new tileart entry, " : "tileart updated, ") +
                   $"legacy {canvasWidth} x {h}" + (withEc ? $", EC image {ecOffset.XEnd} x {ecOffset.YEnd}" + (ecBgra == null ? $" ({options.Scale})" : " (own image)") : ", no EC image");
        }

        /// <summary>
        /// Sets the texture slot to the standard block (image + noise, UOSpriteShader) or removes it. Extra images of an old
        /// block (masks, grunge) belong to the old picture and are dropped.
        /// </summary>
        private static void SetSlot(List<TileArtTexture> textures, int slot, uint path, uint shader, uint noise, bool present)
        {
            textures.RemoveAll(t => t.Slot == slot);
            if (!present)
            {
                return;
            }
            var t = new TileArtTexture { Slot = slot, Unknown1 = 0, ShaderIndex = shader };
            t.Images.Add(new TileArtTextureImage { NameIndex = path, Unknown1 = 0, Unknown2 = 1f, Unknown3 = 0, Unknown4 = 0, TailOffset = -1 });
            t.Images.Add(new TileArtTextureImage { NameIndex = noise, Unknown1 = 0, Unknown2 = 16f, Unknown3 = 1, Unknown4 = 1, TailOffset = -1 });
            textures.Add(t);
        }

        /// <summary>An empty tileart entry with the values nearly all client entries have.</summary>
        public static TileArtEntry NewEntry(uint id)
        {
            var e = new TileArtEntry
            {
                Version = 4,
                TileId = id,
                Unknown3 = 1f,
                Unknown4 = 1.5f,
                OldId = 0xFFFFFF,
                Type = 400,
                Unknown7 = 1065353216,
                Facing = 1,
                RadarA = 255,
                AppearanceData = new byte[4],
                Tail = new byte[] { 0, 0, 0, 0, 0 }
            };
            e.ParseTextures();
            return e;
        }

        /// <summary>An image that is already EC size reduced to the classic size (for the legacy texture).</summary>
        public static byte[] ToClassicSize(byte[] ecBgra, int ecW, int ecH, out int w, out int h)
        {
            w = Math.Max(1, (int)Math.Round(ecW / EcArtScaler.EcScale, MidpointRounding.AwayFromZero));
            h = Math.Max(1, (int)Math.Round(ecH / EcArtScaler.EcScale, MidpointRounding.AwayFromZero));
            return EcArtScaler.Downsample(ecBgra, ecW, ecH, w, h);
        }

        // ==================================================================== gumps

        public GumpState GetGumpState(int id)
        {
            EcUopPackage package = _client.GetPackage(GumpFile);
            EcUopEntry entry = package?.GetEntry(string.Format(CultureInfo.InvariantCulture, GumpPattern, id));
            if (entry == null)
            {
                return GumpState.Missing;
            }
            return entry.DecompressedSize <= 8 ? GumpState.Empty : GumpState.Present;
        }

        /// <summary>Writes a classic gump (BGRA) into gumpartLegacyMUL.uop.</summary>
        public string ConvertGump(int id, byte[] bgra, int w, int h)
        {
            EcUopPackage package = _client.GetPackage(GumpFile) ?? throw new InvalidOperationException(GumpFile + " was not found.");
            package.AddOrReplace(string.Format(CultureInfo.InvariantCulture, GumpPattern, id), EcLegacyGump.Encode(bgra, w, h), package.PreferredCompression);
            return $"Gump {id} (0x{id:X4}): {w} x {h}";
        }
    }

    public enum GumpState
    {
        Missing,
        Empty,
        Present
    }
}
