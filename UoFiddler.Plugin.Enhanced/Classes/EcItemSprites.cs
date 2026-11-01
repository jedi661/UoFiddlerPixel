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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;

namespace UoFiddler.Plugin.Enhanced.Classes
{
    /// <summary>Where the image of an item comes from.</summary>
    public enum EcSpriteKind
    {
        /// <summary>No image found.</summary>
        None,

        /// <summary>EC world art with the item id.</summary>
        WorldArt,

        /// <summary>Area of a world art image shared by several items (atlas, e.g. boat sails, oven frames).</summary>
        WorldArtAtlas,

        /// <summary>The EC image is "blank" (the client draws the item another way, e.g. as effect) – the legacy art is shown.</summary>
        LegacyEcBlank,

        /// <summary>Tileart has no EC image for the item (floors, many animation frames) – the legacy art is shown.</summary>
        LegacyNoEcImage,

        /// <summary>Legacy art because the legacy view was chosen.</summary>
        Legacy
    }

    /// <summary>
    /// Item image cut to its used area, with its position relative to the tile:
    /// X from the left corner of the tile, Y from the bottom corner of the tile (negative = up).
    /// </summary>
    public sealed class EcItemSprite
    {
        public Bitmap Image;
        public int OffsetX;
        public int OffsetY;
        public bool Background;
        public int Height;
        public string Name;
        public string Source;
        public EcSpriteKind Kind;

        /// <summary>True for an image of the Enhanced Client (not the legacy art).</summary>
        public bool IsEnhanced => Kind == EcSpriteKind.WorldArt || Kind == EcSpriteKind.WorldArtAtlas;
    }

    /// <summary>
    /// Finds and caches the images of items the way the client places them (used by the multi and animation views):
    /// <list type="bullet">
    /// <item>EC world art: the image tileart points to (slot 0; can be an atlas shared by several items), cut to the
    /// EC image area and placed with the EC image offset. EC images are 1.5 times the classic size.</item>
    /// <item>Items without an EC image area (floors, fire, bees, ... – "blank" or no slot 0 image): the legacy art
    /// "build/tileartlegacy/&lt;id&gt;.dds", cut to the classic image area, bottom center on the bottom corner of the tile.</item>
    /// </list>
    /// </summary>
    public sealed class EcItemSprites : IDisposable
    {
        /// <summary>EC images are 1.5 times the classic size (tile 66 pixels, 6 pixels per z).</summary>
        public const float EcScale = 1.5f;

        /// <summary>World art id of the empty image some items use in the EC (fire, bees, fountains, ...).</summary>
        public const int BlankWorldArtId = 5703;

        private readonly EcClient _client;
        private readonly Dictionary<(uint Id, bool Legacy), EcItemSprite> _sprites = new Dictionary<(uint, bool), EcItemSprite>();
        private readonly Dictionary<int, Bitmap> _worldArt = new Dictionary<int, Bitmap>();

        public EcItemSprites(EcClient client)
        {
            _client = client;
        }

        /// <summary>Tile width in pixels for the view (classic 44, EC 66).</summary>
        public static int TileWidth(bool legacy) => legacy ? 44 : 66;

        /// <summary>Pixels per z for the view (classic 4, EC 6).</summary>
        public static int ZStep(bool legacy) => legacy ? 4 : 6;

        /// <summary>
        /// Image of an item. <paramref name="legacy"/> = classic size and legacy art; otherwise EC size, EC world art
        /// where tileart has an EC image and the legacy art scaled to EC size for the other items.
        /// </summary>
        public EcItemSprite Get(uint id, bool legacy)
        {
            if (_sprites.TryGetValue((id, legacy), out EcItemSprite sprite))
            {
                return sprite;
            }

            sprite = new EcItemSprite();
            _sprites[(id, legacy)] = sprite;
            if (_client == null)
            {
                return sprite;
            }

            TileArtEntry tile = null;
            try
            {
                tile = (_client.TileArtIfLoaded ?? _client.GetTileArt())?.Get(id)?.Entry;
            }
            catch (Exception)
            {
                // no tileart: legacy art only
            }

            List<int> artIds = tile != null ? WorldArtIds(tile) : new List<int>();
            if (tile != null)
            {
                sprite.Background = ((tile.Flags2 | (ulong)tile.Flags) & 0x1) != 0;
                sprite.Height = (int)(tile.Properties.FirstOrDefault(p => p.Id == TileArtPropertyId.Height)?.Value ?? 0);
                sprite.Name = ItemName(tile);
            }

            try
            {
                if (!legacy && tile != null)
                {
                    TileArtImageOffset o = tile.EcImageOffset;
                    int artId = ChooseWorldArt(artIds, id);
                    if (o.XEnd > o.XStart && o.YEnd > o.YStart && artId >= 0 && artId != BlankWorldArtId)
                    {
                        Bitmap cut = Cut(WorldArt(artId), o);
                        if (cut != null)
                        {
                            sprite.Image = cut;
                            sprite.OffsetX = o.XOffset;
                            sprite.OffsetY = o.YOffset - cut.Height;
                            sprite.Kind = artId == id ? EcSpriteKind.WorldArt : EcSpriteKind.WorldArtAtlas;
                            sprite.Source = artId == id
                                ? $"EC world art {artId:D8}"
                                : $"EC world art {artId:D8} (shared), area {o.XStart},{o.YStart}-{o.XEnd},{o.YEnd}";
                            return sprite;
                        }
                    }
                }

                // legacy (classic) art: the classic image is the area of the classic offset
                Bitmap legacyFull = Decode(_client.GetArtData(EcClient.ArtSources[1], (int)id));
                Bitmap image = tile != null ? Cut(legacyFull, tile.CcImageOffset) : legacyFull;
                if (image != legacyFull)
                {
                    legacyFull?.Dispose();
                }
                if (image == null)
                {
                    return sprite;
                }

                if (!legacy)
                {
                    var scaled = new Bitmap(Math.Max(1, (int)Math.Round(image.Width * EcScale)), Math.Max(1, (int)Math.Round(image.Height * EcScale)), PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(scaled))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                        g.DrawImage(image, 0, 0, scaled.Width, scaled.Height);
                    }
                    image.Dispose();
                    image = scaled;
                }

                // classic placement: bottom center of the image on the bottom corner of the tile
                sprite.Image = image;
                sprite.OffsetX = (TileWidth(legacy) / 2) - (image.Width / 2);
                sprite.OffsetY = -image.Height;
                if (legacy)
                {
                    sprite.Kind = EcSpriteKind.Legacy;
                    sprite.Source = $"Legacy art {id}";
                }
                else if (artIds.Contains(BlankWorldArtId))
                {
                    sprite.Kind = EcSpriteKind.LegacyEcBlank;
                    sprite.Source = "Legacy art – the EC image is \"blank\" (drawn as effect by the client?)";
                }
                else
                {
                    sprite.Kind = EcSpriteKind.LegacyNoEcImage;
                    sprite.Source = "Legacy art – tileart has no EC image for this item";
                }
            }
            catch (Exception)
            {
                // no image
            }
            return sprite;
        }

        private readonly Dictionary<uint, EcSpriteKind> _kinds = new Dictionary<uint, EcSpriteKind>();

        /// <summary>
        /// Where the EC image of an item comes from, from tileart only (fast, no image is loaded; it is not checked
        /// whether the world art file exists).
        /// </summary>
        public EcSpriteKind PredictKind(uint id)
        {
            if (_kinds.TryGetValue(id, out EcSpriteKind kind))
            {
                return kind;
            }
            kind = EcSpriteKind.LegacyNoEcImage;
            try
            {
                TileArtEntry tile = (_client.TileArtIfLoaded ?? _client.GetTileArt())?.Get(id)?.Entry;
                if (tile != null)
                {
                    List<int> artIds = WorldArtIds(tile);
                    TileArtImageOffset o = tile.EcImageOffset;
                    int artId = ChooseWorldArt(artIds, id);
                    if (o.XEnd > o.XStart && o.YEnd > o.YStart && artId >= 0 && artId != BlankWorldArtId)
                    {
                        kind = artId == id ? EcSpriteKind.WorldArt : EcSpriteKind.WorldArtAtlas;
                    }
                    else if (artIds.Contains(BlankWorldArtId))
                    {
                        kind = EcSpriteKind.LegacyEcBlank;
                    }
                }
            }
            catch (Exception)
            {
                // no tileart
            }
            _kinds[id] = kind;
            return kind;
        }

        /// <summary>
        /// Short text for lists: "EC", "EC shared 01121", "legacy (EC blank)", "legacy (no EC image)", "missing".
        /// </summary>
        public static string KindText(EcItemSprite sprite)
        {
            if (sprite == null || sprite.Image == null)
            {
                return "missing";
            }
            switch (sprite.Kind)
            {
                case EcSpriteKind.WorldArt:
                    return "EC";
                case EcSpriteKind.WorldArtAtlas:
                    return "EC (shared image)";
                case EcSpriteKind.LegacyEcBlank:
                    return "legacy (EC blank)";
                case EcSpriteKind.LegacyNoEcImage:
                    return "legacy (no EC image)";
                default:
                    return "legacy";
            }
        }

        private string ItemName(TileArtEntry tile)
        {
            try
            {
                return _client.HasStringDictionary ? _client.StringDictionary.Get(tile.NameIndex) ?? string.Empty : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static Bitmap Decode(byte[] data) => data == null ? null : EcImageHelper.TryDecode(data, out _);

        /// <summary>Decoded world art image (cached, shared by all items that use it).</summary>
        private Bitmap WorldArt(int artId)
        {
            if (!_worldArt.TryGetValue(artId, out Bitmap image))
            {
                image = Decode(_client.GetArtData(EcClient.ArtSources[0], artId));
                _worldArt[artId] = image;
            }
            return image;
        }

        /// <summary>World art ids of the slot 0 images of tileart.</summary>
        private List<int> WorldArtIds(TileArtEntry tile)
        {
            var ids = new List<int>();
            if (!_client.HasStringDictionary)
            {
                return ids;
            }
            foreach (TileArtTexture texture in tile.Textures.Where(t => t.Slot == 0))
            {
                foreach (TileArtTextureImage image in texture.Images)
                {
                    if (EcClient.TryParseTexturePath(_client.StringDictionary.Get(image.NameIndex), out EcArtSource source, out int artId)
                        && source == EcClient.ArtSources[0])
                    {
                        ids.Add(artId);
                    }
                }
            }
            return ids;
        }

        /// <summary>
        /// The image with the item id if tileart lists it, else the first one that is not a helper texture
        /// (ids from 1000000 on: noise, ripple, ground textures, ...); -1 if there is none.
        /// </summary>
        private static int ChooseWorldArt(List<int> ids, uint itemId)
        {
            if (ids.Count == 0 || ids.Contains((int)itemId))
            {
                return (int)itemId;
            }
            List<int> items = ids.Where(i => i < 1000000).ToList();
            return items.Count > 0 ? items[0] : -1;
        }

        /// <summary>Cuts the used area out of a padded image or atlas (the full image is kept; may return it unchanged).</summary>
        private static Bitmap Cut(Bitmap full, TileArtImageOffset o)
        {
            if (full == null)
            {
                return null;
            }
            if (o.XEnd <= o.XStart || o.YEnd <= o.YStart)
            {
                return full;
            }
            Rectangle rect = Rectangle.Intersect(new Rectangle(o.XStart, o.YStart, o.XEnd - o.XStart, o.YEnd - o.YStart), new Rectangle(0, 0, full.Width, full.Height));
            if (rect.Width <= 0 || rect.Height <= 0 || rect.Size == full.Size)
            {
                return full;
            }
            return full.Clone(rect, PixelFormat.Format32bppArgb);
        }

        /// <summary>Releases all images.</summary>
        public void Clear()
        {
            var shared = new HashSet<Bitmap>(_worldArt.Values.Where(b => b != null));
            foreach (EcItemSprite s in _sprites.Values)
            {
                if (s.Image != null && !shared.Contains(s.Image))
                {
                    s.Image.Dispose();
                }
            }
            _sprites.Clear();
            _kinds.Clear();
            foreach (Bitmap b in shared)
            {
                b.Dispose();
            }
            _worldArt.Clear();
        }

        public void Dispose() => Clear();
    }
}
