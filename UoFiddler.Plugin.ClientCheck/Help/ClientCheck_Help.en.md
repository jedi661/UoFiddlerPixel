# Client Check – Help

The client check compares **all files of the classic client with each other** and shows what does not fit together. These are errors you would otherwise only notice in the game: invisible items, black spots on the map, missing names, creatures with wrong movements.

The files currently loaded in UoFiddler are checked (client folder from the options, including your changes in memory).

## How to use it

1. Choose what to check under **Checks** (default: everything). The choice is remembered.
2. Click **▶ Start check**. The progress is shown in the status bar. UoFiddler stays usable; **■ Cancel** stops the check.
3. The results appear in the list. A summary at the top shows errors, warnings, infos and the count per area.
4. Click an entry: on the right you see the **description**, **How to fix** and a **preview** (item, land tile, gump or animation).
5. Fix entries that can be fixed with **Fix** (several selected entries work too) or all shown entries at once with **Fix all fixable…**.
6. **Export report** saves the shown entries as an HTML page, a CSV spreadsheet or text.

Switch the language at the top right (Deutsch, English, Français). It applies to all texts at once, including the results and this help.

## Levels

| Level | Meaning |
|---|---|
| **Error** | Something is definitely missing in the game (invisible, broken). Should be fixed. |
| **Warning** | Works, but looks or behaves wrong (e.g. black on the map). |
| **Info** | Incomplete, but usually harmless (e.g. graphic without a name). |

## Filter and handling

- **Show:** filter by level and by area.
- **Search:** searches id and description (e.g. `0x1F03`, `Body 400`, `Felucca`).
- **Hide fixed:** hides entries you already fixed (they are marked with ✓).
- Click a **column header** to sort; click again to reverse.
- **Ctrl+C** or **Copy line** copies the selected entries, **Copy ID** only the id.

## The checks in detail

### Art ↔ tiledata
- **Graphic without a name** (info): the item has a graphic but no name in tiledata. Fix: tab "TileData", set name and flags, save tiledata.mul.
- **tiledata without a graphic** (info): a tiledata entry has a name but no graphic. Fix: add the graphic in the "Items" tab or clear the entry.

### Land tiles
- **Land tile without a name** (info).
- **Missing texture** (warning): tiledata points to a texture that does not exist; sloped ground looks wrong. Fix: add the texture in the "Textures" tab or set TextureId to 0.

### Equipment
All items with the flag **Wearable** are checked.
- **No layer** (warning): `Quality` is 0, the server does not know where the item is worn.
- **Missing animation** (error): the tiledata field `Animation` points to an animation that does not exist – the item is invisible when worn.
- **Missing paperdoll gump** (warning): gump **50000 + animation** (male) is missing and there is no replacement in gump.def – the item is missing on the paperdoll.
- **Missing female gump** (info): gump **60000 + animation** is missing; the client uses the male one (often intended).
- Backpack (layer 21) and mounts (layer 25) have no paperdoll gump of their own and are skipped.

### Bodies / animations
Bodies 1 to 4095 are checked.
- **No entry in mobtypes.txt** (warning): there is an animation, but the client does not know the type (MONSTER, SEA_MONSTER, ANIMAL, HUMAN, EQUIPMENT) and plays wrong actions. **Fixable:** the suggested line is written into a copy of mobtypes.txt in the **output folder** (backup `.bak`). Copy the file into the client afterwards.
- **mobtypes.txt without an animation** (info): a line without a matching animation, usually a harmless leftover.
- **Bodyconv.def points to nothing** (error): a body is redirected to anim2–anim5, but there is no data – the creature is invisible.
- **body.def points to nothing** (warning): a body is redirected to a body without an animation.

### radarcol (map colours)
radarcol.mul holds **one colour** for every land tile and every item for the radar and the map. Without it the item is **black** on the map.
- **No radar colour** (warning): **fixable.** "Fix" sets the average colour of the graphic (in memory). Then click **Save radarcol.mul** – the file is written into the output folder and has to be copied into the client. The button is orange while something is unsaved.
- **radarcol.mul too short** (warning): the file has no room for extended item ids and has to be enlarged.
- Tiles with "NoDraw" in the name are skipped (they are invisible on purpose).

### Multis
- **Component without a graphic** (error): a house or ship contains item ids without a graphic; these parts are missing in the game. Fix: add the graphic or replace the component in the multi editor.

### Statics on maps
All maps are read (staidx/statics). Maps that use the same files (e.g. Felucca and Trammel) are read once.
- **Item does not exist** (error): statics with an item id without a graphic are placed on the map. One entry per id with the count and an example position (`map X,Y,Z`).
- **Invalid hue** (warning): statics with a hue number higher than the number of hues in hues.mul.
- **Read error** (error): staidx and statics do not belong together or are damaged.

### Cliloc (names)
The client shows item names through cliloc: **1020000 + id**, from id 0x4000 **1078872 + id** (as ServUO `Item.LabelNumber`).
- **No cliloc name** (info): there is no entry for the item in Cliloc.enu. Fix: add the entry in the "Cliloc" tab.

## Automatic fixes – what exactly happens

| What | Where it is written |
|---|---|
| Radar colour | in memory, then **Save radarcol.mul** → output folder |
| mobtypes.txt line | copy `mobtypes.txt` in the output folder (copied from the client the first time), backup `.bak` |

Nothing is ever written **directly into the client folder**. Copy the changed files into the client yourself when everything is fine.

## Tips

- After fixing, simply start the check again: fixed problems disappear.
- For a quick check after adding new items, the checks **Art ↔ tiledata**, **Equipment**, **radarcol** and **Cliloc** are enough.
- The map check takes a few seconds per map for big maps.
- The HTML report is good for passing on to the team: it is grouped by area and contains the way to fix every entry.

## Files of this extension

- `ClientCheckPlugin.dll` in the `plugins` folder of UoFiddler
- Help: `plugins\ClientCheckHelp\ClientCheck_Help.de.md`, `…en.md`, `…fr.md`
- Settings (language, chosen checks): `ClientCheck.lang` and `ClientCheck.checks` in the AppData folder of UoFiddler
