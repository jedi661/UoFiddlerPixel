# UoFiddler.Plugin.Enhanced

Plugin for the files of the **Ultima Online Enhanced Client** (EC). It adds the tab **"Enhanced Client"**.
Enable it under *Plugins → Manage Plugins* (`UoFiddler.Plugin.Enhanced.EnhancedPluginBase`).

Choose the EC folder on top of the tab (the folder with `tileart.uop`, `Texture.uop`, ...). The folder is remembered.

## Editors

| Tab | Files | What it does |
|---|---|---|
| **Art / Textures** | `Texture.uop`, `LegacyTexture.uop` | Thumbnail grid of the DDS images (`build/worldart/<id>.dds`, `build/tileartlegacy/<id>.dds`, `build/tileartenhanced/<id>.dds`), preview with zoom, replace / insert / remove images (PNG, BMP, GIF, JPG, TIF, DDS → DXT1/DXT3/DXT5/ARGB32, mipmaps), export PNG/BMP/DDS, export all |
| **TileArt** | `tileart.uop`, `string_dictionary.uop` | All entries with name, flags, properties (weight, height, ...), radar color, image offsets, stack aliases, texture references with preview; duplicate entries to new ids, remove, import/export single entries, CSV list |
| **String Dictionary** | `string_dictionary.uop` | Search, add and change the texts tileart points to (names, shaders, texture paths) |
| **UOP Browser** | every `*.uop` | Entries of any UOP file with image / hex preview, resolve names with a pattern (`build/xxx/{0:D8}.dds`), export, replace, add, remove |

Saving always writes a new file; the old one is kept as `<file>.<date>.bak`. The client must be closed while saving.
"Save as..." writes into another file/folder (recommended for testing).

## File format notes

* UOP (MYP) packages: file names are stored as 64 bit hash (hashlittle2), the table of contents also stores an Adler-32 of the
  12 byte metadata header (`03 00 08 00` + FILETIME). Unchanged entries are copied byte by byte – an unchanged package is saved identical.
* `tileart.uop`: `build/tileart/<id 8 digits>.bin`, zlib compressed. The id of the file name is the one the client uses
  (some original entries store another id inside). Everything up to the radar color is parsed, the rest (texture blocks, ...) is kept
  and only patched in place. Texture references are 1 based indices into `string_dictionary.uop`
  (e.g. `Data\WorldArt\00000002_ankh.tga` → `build/worldart/00000002.dds`).
* `string_dictionary.uop`: one entry; `ulong`, `uint count`, `uint`, then `ushort length + text` per string.
* Entries with compression flags other than 0 (none) and 1 (zlib) can't be shown or changed, but are kept when saving.

Format knowledge partly from [UOETE](https://github.com/cbnolok/UOETE) (EC tileart editor).

## Structure

* `Core/` – file formats without WinForms (`EcUopPackage`, `DdsImage`, `TileArtFile`, `EcStringDictionary`, `EcClient`)
* `Classes/` – helpers for the UI (bitmap conversion, dialogs, settings, PropertyGrid view)
* `UserControls/` – the tab (`EnhancedClientControl`) and one control per editor (`IEcEditor`)

New editors (gumps, animations, maps, UI) are added as further `IEcEditor` tabs in `EnhancedClientControl`.
