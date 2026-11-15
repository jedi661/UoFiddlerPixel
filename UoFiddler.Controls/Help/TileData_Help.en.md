# TileData – help

The TileData tab edits the properties of the **items** (statics) and **land tiles** of `tiledata.mul`, and the **text files of ClassicUO**.

All changes are made **in memory** first. They are written to the file with **Save Tiledata** (`tiledata.mul` in the output folder).

## Layout

| Tab | Content |
|---|---|
| Items | list of all items, image, values (name, weight, height …) and flags |
| Land Tiles | list of all land tiles, image, name, texture id and flags |
| ClassicUO | editor for `chair.txt`, `cave.txt`, `tree.txt` … of ClassicUO |

Red entries in the lists differ from the state of the file. After **Save Tiledata**, undo or revert they turn black again.

## Multi selection

In both lists (items and land):

- **Click** – select one tile
- **Ctrl + click** – add the tile to the selection or remove it
- **Shift + click** – range from the last clicked tile to this one (inside one group)
- **Right click → Select ID range…** – select by ids, e.g. `0x0B2C-0x0B40, 0x0C00, 3000..3010`
- **Right click → Clear selection** – only the shown tile stays selected

Selected tiles have a light blue background. The fields always show the values of the last clicked tile.

### The same change on several tiles

1. Select the tiles (Ctrl/Shift).
2. In the fields, change **only what should apply to all of them**, e.g. Height = 5 and the tick of *Surface*.
3. Click **Save changes**.

**With one tile**, all fields are written.

**With several tiles**, only the fields and flags you changed since clicking are written. All other values of the tiles (name, animation …) stay as they are. This also works with presets, *Insert settings* and the clear button: they fill the fields, and **Save changes** applies what was changed to all selected tiles.

With **Save directly on changes** (menu), every change is applied to **all selected** tiles at once.

## Batch edit

For bigger jobs, **Batch edit…** (toolbar or right click) opens its own window.

**Which tiles:**
- the selected tiles
- an ID range (`0x0B2C-0x0B40, 3000..3010`)
- all tiles of the list (with the filter of the tab)

You can also limit it to "only names containing …" and "only with flag …".

**Values:** only ticked fields are changed. Typing a value ticks the field.
- Numbers: *set to*, *add* or *subtract*. The value stays in its valid range (0–255 or ±32767).
- Name: *set to*, *prefix*, *suffix* or *replace* (max. 20 characters).

**Flags:** *set* adds the flag, *remove* removes it. The column *have it* shows how many of the tiles have the flag already.

At the bottom you see how many tiles really change. **Apply** does everything as **one** undo step.

## Undo / redo

- **↶ Undo** / **Ctrl+Z** and **↷ Redo** / **Ctrl+Y** (or Ctrl+Shift+Z)
- Undo works for everything: fields, flags, save changes, batch edit, CSV import, presets, insert settings, set textures, revert.
- Several key presses in the same field shortly after each other are one step.
- After reloading the files the history starts again.
- Ctrl+Y in the item list stays "Push marked" while the marker button is active.

## Changes and revert

**Changes…** lists all items and land tiles that differ from the state of the file, with the exact changes (e.g. `Weight 1 → 255; +Wall -Surface`).

- **Show** (or double click) jumps to the tile.
- **Revert selected** / **Revert all** sets tiles back to the file. This can be undone too.
- **Copy list** copies the list as text.

**Right click → Revert to file state** reverts the selected tiles directly.

## ClassicUO files

ClassicUO reads extra information from `<ClassicUO>\Data\Client\*.txt`.

Setup: choose the ClassicUO folder with **Load**. The `Data` or `Data\Client` folder works as well, the right sub folder is found. Then choose the file in the list.

| File | Content | Format |
|---|---|---|
| chair.txt | seats | `GRAPHIC,D1,D2,D3,D4,OFFSET_Y,MIRROR_OFFSET_Y` (direction 0/2/4/6, -1 = not possible) |
| cave.txt | cave tiles | one graphic id per line (decimal) |
| vegetation.txt | grass, bushes … (can be hidden) | one graphic id per line (decimal) |
| tree.txt | trees → stumps | `GRAPHIC=FLAG` (0/1) |
| containers.txt | container gumps | `GUMP OPEN_SOUND CLOSE_SOUND X Y W H [ICON] [MIN_X] [MIN_Y]` |
| seasons.txt | seasons | `SEASON,static/landtile,ORIGINAL,REPLACEMENT` (commas only, no spaces) |
| lights.txt | light of items | `ITEM_ID SHADER` or `ITEM_ID H<hue>` |
| lightshaders.txt | light colours | `ID RGB R_CURVE [G_CURVE B_CURVE]` (curves Standard, A–E) |

Comment lines start with `#` or `;` (seasons.txt: `#` or `//`). Ids are decimal; only seasons.txt and lights.txt also accept `0x…`.

**Editor functions:**
- The description of the chosen file is shown on the right.
- Below the buttons you see what the line under the cursor refers to, e.g. `item 0x0B2C "chair"`.
- **Check** checks the file: wrong format, graphics without art, duplicate entries. Lines with problems are marked red.
- **Add selected tiles** adds the tiles selected in the items tab as new lines, each with a comment line with the name. Tiles already in the file are skipped. Afterwards set the directions for chairs, the replacement for seasons, the shader for lights.
- **Right click in the item list → Add to ClassicUO file** does the same from the list (chair, cave, vegetation, tree, seasons, lights). In the land list: **Add to seasons.txt**.
- **Save Txt** saves. Before the first save of a file, `<file>.txt.bak` is made. You are asked when the format has errors.
- **Reload** loads the file again, **Open folder** opens the folder.
- Switching to another file with unsaved changes asks first.

ClassicUO reads the files when it starts. Restart the client after saving.

## More functions

- **Presets** (drop-down in the toolbar): typical values for wall, door, stairs, chair … Land presets ("LandTile …") only change the land flags.
- **Copy / Insert settings** (right click): copy the values of one tile to another one. With several selected tiles click **Save changes** afterwards.
- **Marker / push marked**: remember values and transfer them to other items with the middle mouse button or Ctrl+Y.
- **Import / export CSV**: all items or land tiles as a table. The import can be undone.
- **Set filter**: filter the lists by values and flags.
- **Set textures**: set the texture ids of the land tiles automatically.
