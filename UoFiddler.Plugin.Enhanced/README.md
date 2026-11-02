# UoFiddler.Plugin.Enhanced

Plugin for the files of the **Ultima Online Enhanced Client** (EC). It adds the tab **"Enhanced Client"**.
Enable it under *Plugins → Manage Plugins* (`UoFiddler.Plugin.Enhanced.EnhancedPluginBase`).

Choose the EC folder on top of the tab (the folder with `tileart.uop`, `Texture.uop`, ...). The folder is remembered.

## Tabs

The tab "Enhanced Client" holds one sub tab per kind of data – in the same order as the main tabs of UoFiddler.
Every tab can search, show, change, add, delete, export / import and save (with backup, "Save as..." into another folder).

| Tab | Files | What it does |
|---|---|---|
| **Items / Art** | `Texture.uop`, `LegacyTexture.uop`, `EffectTexture.uop`, `TerrainTexture.uop` | Thumbnail grid of the item / effect / terrain images (DDS, TGA), replace / insert / remove (DXT1/3/5, ARGB32, mipmaps), export |
| **TileData** | `tileart.uop`, `string_dictionary.uop` | All entries: name, flags (EC and classic), properties, layer, radar color, image offsets, texture references; duplicate, remove, import/export |
| **Gumps** | `Interface.uop` (`…/textures/gumpart/<id>.tga`), `gumpartLegacyMUL.uop` | Thumbnail grid, replace / insert / remove, export (legacy gumps are encoded in the classic RLE format) |
| **Hues** | `Hues.uop`, `hues.mul` | Sub tabs: **Editor** (ramps, the 32 classic colors, names; hues.dds and the overview are updated with every change), **Compare** (classic ↔ EC, filter, show / hide columns, copy marked hues in both directions), **Converter** (hues.mul → Hues.uop like huesmul2uop but as update, Hues.uop → hues.mul without loss, backup old + new, analyze, rebuild hues.dds), **Help** EN / DE |
| **Animations** | `AnimationFrame1–6.uop`, `Paperdoll.uop` | Bodies and actions (AMOU), playback in all 8 view directions (compass, turn buttons, keys Left / Right, "turn around while playing"; ↗ → ↘ are the mirrored stored directions like in the classic client), ground tile, PNG / `.bin` export and import, new bodies / actions |
| **AnimData** | `AnimationDefinition.uop` | Animated items (frames, time per frame); preview with the item images placed like in the client (EC world art, shared images cut out, legacy art where the EC has no image), list columns "EC images" and "Classic", filters (complete / missing EC images, different from classic, not in classic, only in classic), **classic comparison**: animdata.mul + art of the loaded classic client next to it (show / hide each side), "From classic" takes the classic frames and speed |
| **Multis** | `MultiCollection.uop` | Built like the Multis tab (designer layout): list with the names of `Multilist.xml`, free slots, context menu (new, import, export, remove, reload, save), height slider (max z), min z, floors, art EC or legacy, same draw order as the Multis tab, grid; hover / click highlights parts, component list with show / hide per part, part image; extract image (bmp/tiff/jpg/png), copy, background grass / water / own image / color; export parts (text, UOA, WSC, CSV, UOX3, XML), export all images / parts, import (text, UOA, CSV, WSC), new / copy / remove |
| **Map** | `facet0–5.uop`, `facet0x–5x.uop`, `facet00–05.dds` | Overview from the radar image, sector view (64 × 64 tiles), tile inspector: z, land, blend layers, statics (add / edit / remove with image), copy / paste tiles, sector import / export; **mark area** on the sector or the overview → list of all statics in it (go to, remove), export (text relative, CSV with map coordinates, multi text) and import (own formats or multis: text, UOA, CSV, WSC; add or replace); **View region**: isometric view of the area with land (heights, radar colors / classic land art) and statics in EC images, select / add / edit / remove statics, import / export, save image, save map |
| **Map Converter** | `map#.mul` / `map#LegacyMUL.uop`, `staidx#.mul`, `statics#.mul`, `radarcol.mul` ↔ `facet#.uop`, `facet0#.dds` | Classic → Enhanced and Enhanced → Classic (land, heights, statics, border neighbours, size entry, radar image), optional land table (e.g. seasons), analysis and repair of both formats, new flat maps; file names and size follow the map number ("x files" for map#x / facet#x) |
| **Sounds** | `Audio.uop` | Sounds and music from `audio_sounds.csv` / `audio_music.csv`, play (wav) / open (mp3), export, replace, add, change path, remove |
| **Cliloc** | `LocalizedStrings.uop` | All 8 languages, search, edit, add, delete, text import / export |
| **String Dictionary** | `string_dictionary.uop` | Names, shaders and texture paths tileart points to |
| **Interface / Data** | `Interface.uop`, `GameData.uop`, `MainMisc.uop`, `Shaders.uop`, `SystemTextures.uop`, `LegacyTerrain.uop`, `EffectTexture.uop`, `TerrainDefinition.uop` | Folder tree of the real paths (UI windows, lua, csv, xml, shaders, textures); text files are edited directly, images shown; export (also whole folders), replace, add, remove |
| **UOP Browser** | every `*.uop` | Raw entries of any file with folder filter and image / text / hex preview |

**Analyze folder...** (top of the tab) writes a zip with a text report about every UOP file of the folder (structure,
compression, content types, found file names, data patterns, string_dictionary references), small sample entries and the
table of contents of every file (`toc/*.csv`: hash, name, sizes) – no complete files and no programs.
Send it to the developer to add support for unknown files. A second click cancels.

**Names → Load name dictionary (.dic)** uses a list of known packed names for every file, e.g. the `Dictionary.dic` of the
[Mythic Package Editor](https://github.com/pincoide/Mythic-Package-Editor-EC-) (not included – that project has no license).
**Names → Export known names** writes the names found by the plugin in the same format.

Saving always writes a new file; the old one is kept as `<file>.<date>.bak`. The client must be closed while saving.
"Save as..." writes into another file/folder (recommended for testing).

## File format notes

* UOP (MYP) packages: file names are stored as 64 bit hash (hashlittle2), the table of contents also stores an Adler-32 of the
  12 byte metadata header (`03 00 08 00` + FILETIME). Unchanged entries are copied byte by byte – an unchanged package is saved identical.
* `tileart.uop`: `build/tileart/<id 8 digits>.bin`, zlib compressed. The id of the file name is the one the client uses
  (some original entries store another id inside). Everything up to the radar color is parsed, the rest (texture blocks, ...) is kept
  and only patched in place. Texture references are 1 based indices into `string_dictionary.uop`
  (e.g. `Data\WorldArt\00000002_ankh.tga` → `build/worldart/00000002.dds`).
* Tileart ids are item ids (as in the classic art: 0x0002 = ankh, 0x0E5C = glowing rune).
* `AnimationDefinition.uop`: `build/animationdefinition/<id 8 digits>.bin` – uint id, 12 bytes 0, uint 1, uint 0, int -1,
  float seconds per frame, uint count, count × (uint item id, byte 0).
* Other known names: `build/animationsequence/<id>.bin`, `build/effectdefinitioncollection/<id>.bin`, `build/facetdefinition/<id>.bin`,
  `data/effects/<file name>` in `EffectTexture.uop` (Gamebryo `.nif` models, `.ems` particle scripts as text, `.tga`/`.dds` textures).
* Map sectors store for the tiles on the sector border the land of their neighbours in the next sector ("layers": direction
  0 left, 1 top left, 2 top, 3 right, 4 bottom right, 5 bottom, 6 bottom left, 7 top right, z, land as uint): edge tiles the
  neighbour across the edge, corner tiles also the diagonal one, sorted by direction, neighbours outside the map left out.
  The 8 byte size entry is `build/sectors/facet_0N/data.map` (uint height, uint width) – the hash matches all client facets.
* Maps (`facetN.uop`): `build/sectors/facet_0N/<nr 8 digits>.bin` = 64 × 64 tiles, column by column; sector number =
  x × sectors in y + y (sectors rounded up, Tokuno 1448 → 23). Sector: byte facet, ushort number, per tile: sbyte z,
  ushort land, byte layer count + layers (byte blend, sbyte z, ushort land, ushort 0), byte static count + statics
  (uint item, sbyte z, uint hue). One 8 byte entry holds uint height, uint width. `facetNN.dds` = radar image (1 pixel per tile).
* Cliloc (`data/localizedstrings/<nr>.cliloc`): uint version, ushort, then int id, byte flag, ushort length, UTF-8 text.
* Multis (`build/multicollection/<id 6 digits>.bin`): int id, int count, parts (ushort item, short x, y, z, ushort flags,
  int count + uint clilocs).
* Legacy gumps (`build/gumpartlegacymul/<id>.tga`, no real TGA): uint width, uint height, row offsets (in 4 byte units from the
  table), runs of (ushort RGB555 color, ushort length); color 0 = transparent.
* Hues: `data/definitions/hues/hue<nr 4 digits>.bmp` (256 × 2, 32 bit; line 0 = ramp, line 1 = color 31; the size field of
  the original files is file size / 4), `data/definitions/hues/huenames.csv` ("number,name" or just "number"),
  `build/hues/hues.dds` (1024 × 1024, one 256 pixel row per hue: column n / 1024, row n % 1024; colors the items) and the
  unnamed entry 0xFA5C6A1BC0D8B01B (64 × 64 A8R8G8B8, one pixel = color 31 per hue, column n / 64, row n % 64).
  Original ramps: color i at pixel 8 · i, linear in between, 5 bit value · 8. huesmul2uop: 8 pixels per color, blurred (4 / 6),
  value · 255 / 31. `build/hues/classichues.bin` (written by this plugin, ignored by the client) keeps the exact hues.mul data.
  EC hue n = hues.mul entry n − 1 (groups of 4 byte header + 8 × 88 bytes).
* Sounds: `data/audio/audio_sounds.csv` (id, path), `data/audio/audio_music.csv` (id, path, loop); files are `data/audio/<path>`.
* `string_dictionary.uop`: one entry; `ulong`, `uint count`, `uint`, then `ushort length + text` per string.
* MYP version 5 files (`Texture.uop`, `Interface.uop`, `AnimationFrame*.uop`, `facet*.uop`, `EffectTexture.uop`, ...):
  entries have a 134–137 byte metadata block (`04 00 84/85 00`, `00 14 00`, then about 128 bytes – very likely an RSA signature)
  and a table checksum of unknown kind. Unchanged files are copied 1:1, unchanged entries keep their block. Changed or new entries
  get the plain version 4 header (no signature); saving asks first, because it is not known yet whether the client accepts that.
* More names: `build/terraintexture/<id>.dds`, `build/terraindefinition/<id>.bin`,
  `build/animationframe/<body 6 digits>/<action 2 digits>.bin` (AnimationFrame1–6, "AMOU" frames),
  `build/paperdoll/<id 6 digits>/<nr 2 digits>.bin`.
* Interface.uop, Audio.uop, ... store real paths (`data/interface/default/Source/ContextMenu.lua`, `data/audio/Music/turfin.mp3`).
  `EcContentNameResolver` starts with known names and follows the paths written in the xml/mod/lua/csv/text entries
  (relative to their folder) until no new name is found.
* AMOU animations: header (`AMOU`, version, length, body, bounds x1/y1/x2/y2, color count, color offset, frame count,
  frame table offset), colors (R, G, B, A), frame table (action, number 1..5×frames per direction, bounds, data offset
  relative to the table entry), then one pixel stream per frame: byte < 128 = skip transparent pixels, byte ≥ 128 =
  packet of (byte − 128) opaque pixels with a coverage byte (high nibble: edge pixel before, low nibble: edge pixel after,
  1–15 = 1/16 steps); packets end at the row end and hold at most 127 opaque pixels; a lone edge pixel at the row end is
  written as "after". (Different from the classic "animationlegacyframe" format with RGB555 palettes.)
* Tileart: the second flag field holds the classic tiledata flags (same bit values); property 6 is the equipment layer.
* Tileart EC image offset: the world art DDS is padded to a power of two, the item is the area (x start, y start) – (x end, y end);
  x offset = left edge from the left corner of the tile, y offset = bottom edge from the bottom corner of the tile (negative = up).
  EC images are 1.5 × the classic size (tile 66 pixels, 6 pixels per z). The classic image offset holds the size of the classic image.
* Multi parts: flags 0 = shown, other values = not shown in the game (the UOP packer converts them to classic flags 0).
* Floors and other background tiles have no EC image area in tileart (EC offset 0); their first world art is a tiling ground
  texture (e.g. `02000020_Dirt_A`). The multi view draws them with the legacy art (`build/tileartlegacy/<id>.dds`, area = classic offset), scaled to EC size.
* World art can be an atlas shared by several items: the mast and sails of a boat (0x3EDC, 0x3EDD, 0x3EDE) all point to
  `Data\WorldArt\00016093_mast.tga` and use different areas of it. The image is the slot 0 texture tileart points to
  (the one with the item id if listed, else the first one below 1000000 – higher ids are helper textures like noise or ripple),
  not `worldart/<item id>`.
* AnimationDefinition: the id is the item id (like animdata.mul), the frames are absolute item ids (classic: id + offset).
  Only about a fifth of the frames have an own EC image; many share an image with other frames (e.g. oven 0x462–0x464 all
  use `00001121_Oven.tga`), and most have no EC image at all: slot 0 is `00005703_blank.tga` (fire, bees, fountains – probably
  drawn as effects by the client) or missing. For those the view shows the legacy art.
* More names: `build/sectors/facet_0N/<nr 8 digits>.bin` (facetN.uop), `data/interface/default/textures/gumpart/<id>.tga`,
  `data/definitions/hues/hue<nr 4 digits>.bmp`, `data/localizedstrings/<nr 3 digits>.cliloc`.
* Entries with compression flags other than 0 (none) and 1 (zlib) can't be shown or changed, but are kept when saving.

Why older tools damaged files: the Mythic Package Editor writes the metadata header length as 0 and computes the table
checksum over the data; this plugin keeps the metadata header, the checksum and – for unchanged entries – every byte.

Format knowledge partly from [UOETE](https://github.com/cbnolok/UOETE) (EC tileart editor) and
[UO EC Super Viewer](https://github.com/pincoide/UO-EC-Super-Viewer) (AMOU reading); the AMOU writer was worked out by
re-encoding original frames until they matched byte by byte.

## Structure (like UoFiddler.Controls / Ultima)

* `EcUltima/` – the file formats, no WinForms (like the `Ultima` project):
  `Uop/` (package, hash, names, content name resolver, name dictionary), `Imaging/` (DDS, TGA), `TileArt/`, `Strings/`
  (string dictionary, cliloc), `Animations/` (AMOU, animation definition), `Maps/` (sector, facet), `Multis/`, `Gumps/`,
  `Hues/`, `Sounds/`, `Analysis/`, `EcClient.cs` (client folder, open packages, known names).
* `UserControls/` – one control per tab (`IEcEditor`), `EnhancedClientControl` holds the tabs. Every control has a designer
  layout like the controls of UoFiddler: `<name>.cs` (logic, named event handlers `OnClick...`), `<name>.Designer.cs`
  (layout, edit it in the Visual Studio designer) and `<name>.resx`. Drawing helpers: `EcPreviewPanel`, `EcThumbnailGrid`,
  `EcMapOverviewPanel`, `EcMapSectorPanel`.
* `Classes/` – UI helpers (bitmap conversion, dialogs, settings, `EcPackageSaver`, `EcItemSprites` item images, `EcMapRenderer` isometric map view, `EcStaticsUi`).
* `Forms/` – windows (`EcRegionViewerForm`).
* `Models/` – view models (PropertyGrid view of a tileart entry).

Author: Nikodemus (UoFiddlerPixel). License: Beer-Ware (see the file headers).
