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
| **Hues** | `Hues.uop` | Color ramps (256 × 2 BMP per hue) and names; import / export ramp, add copy, delete |
| **Animations** | `AnimationFrame1–6.uop`, `Paperdoll.uop` | Bodies and actions (AMOU), playback per direction, PNG / `.bin` export and import, new bodies / actions |
| **AnimData** | `AnimationDefinition.uop` | Animated items (frames, time per frame) with preview |
| **Multis** | `MultiCollection.uop` | Parts table (editable), isometric preview with the EC item images, new / copy / delete, text import / export |
| **Map** | `facet0–5.uop`, `facet0x–5x.uop`, `facet00–05.dds` | Overview from the radar image, sector view (64 × 64 tiles), tile inspector: z, land, blend layers, statics (add / edit / remove with image), copy / paste tiles, sector import / export |
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
* Maps (`facetN.uop`): `build/sectors/facet_0N/<nr 8 digits>.bin` = 64 × 64 tiles, column by column; sector number =
  x × sectors in y + y (sectors rounded up, Tokuno 1448 → 23). Sector: byte facet, ushort number, per tile: sbyte z,
  ushort land, byte layer count + layers (byte blend, sbyte z, ushort land, ushort 0), byte static count + statics
  (uint item, sbyte z, uint hue). One 8 byte entry holds uint height, uint width. `facetNN.dds` = radar image (1 pixel per tile).
* Cliloc (`data/localizedstrings/<nr>.cliloc`): uint version, ushort, then int id, byte flag, ushort length, UTF-8 text.
* Multis (`build/multicollection/<id 6 digits>.bin`): int id, int count, parts (ushort item, short x, y, z, ushort flags,
  int count + uint clilocs).
* Legacy gumps (`build/gumpartlegacymul/<id>.tga`, no real TGA): uint width, uint height, row offsets (in 4 byte units from the
  table), runs of (ushort RGB555 color, ushort length); color 0 = transparent.
* Hues: `data/definitions/hues/hue<nr 4 digits>.bmp` (256 × 2, 32 bit; the original header is kept when a ramp is replaced),
  names in a text entry ("number,name").
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
* `UserControls/` – one control per tab (`IEcEditor`), `EnhancedClientControl` holds the tabs.
* `Classes/` – UI helpers (bitmap conversion, dialogs, settings, `EcPackageSaver`).
* `Models/` – view models (PropertyGrid view of a tileart entry).
