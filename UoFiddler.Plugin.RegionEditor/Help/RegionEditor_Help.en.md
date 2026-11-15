# Region editor

The region editor lets you draw regions (towns, dungeons, guard zones, no-housing areas …) directly on the maps of your client and write them for **ServUO/RunUO**, **Sphere** or **POL**. The maps come from the same map list as the map tab (also your own maps from "Manage maps").

## Layout

- **Top:** file menu, undo/redo, map selection, check, settings, help, language, design (light/dark).
- **Second bar:** tools, snap, zoom, view, client.
- **Left:** tree with all regions of the selected map (child regions indented), search (Ctrl+F).
- **Center:** the map with the regions. Bottom right the minimap – a click jumps there.
- **Right:** tabs **Properties**, **Areas**, **Check**, **History**.
- **Bottom:** position, land tile and height under the mouse, regions under the mouse, messages.

## Tools

| Tool | Key | Function |
|---|---|---|
| Select | V | click a region; drag a rectangle = move; handles = resize; Shift = move the whole region; Alt = next region below |
| Pan | H | drag the map (the middle mouse button always works) |
| Rectangle | R | drag an area = new region |
| Polygon | P | click the corners, double click/Enter = finish, Backspace = remove point, Esc = cancel |
| Circle | C | drag from the center |
| Fill | F | connected land of the same kind in the visible area (island, forest, desert, lake …) |
| Go | G | set the go location of the selected region |
| Entrance | E | set the entrance of the selected region (DungeonRegion) |

When drawing:

- **no key** = new region (if the selected region has no area yet, it gets the area)
- **Shift** = add the area to the selected region
- **Alt** = cut the area out of the selected region

**Snap** (1, 2, 4, 8, 16) aligns rectangles and polygon points – 8 matches the map blocks.

Mouse wheel = zoom, arrow keys = move the map, Del = delete the selected area or region.

## Editing regions

- **＋ Region / ＋ Child** (Insert / Shift+Insert). Then draw the area.
- **Rename:** F2 in the tree or the "Name" field.
- **Nesting:** drag a region in the tree onto another one = becomes its child. Shift when dropping = insert before it. Drop on an empty place = top level.
- **Tree context menu:** duplicate, show on map, send the client to the go location, copy as ServUO XML, optimize rectangles, convert polygons, hide, lock.
- **Map context menu:** select region here, set go/entrance here, delete area, send the client here, copy position.
- **Locked** regions can not be moved on the map. **Hidden** regions are not drawn (and optionally not exported).

### Properties

| Field | Meaning |
|---|---|
| Name | name of the region (ServUO: `name`) |
| Type | C# class of the server: TownRegion, DungeonRegion, GuardedRegion, NoHousingRegion, Jail … – own types are allowed |
| Priority | the higher one wins when regions overlap (default 50); child regions inherit it |
| Music | ServUO `MusicName` (POL: midi number, converted) |
| Z range | `<zrange>` of the whole region; per rectangle in the Areas tab |
| Go | target of [go and runes; ⌖ = pick on the map |
| Entrance | `<entrance>` of a DungeonRegion, optionally on another map |
| Guards | `<guards disabled>` and an own guard type |
| Rune | `<rune name>` |
| Options | `smartNoHousing`, `logoutDelay` (grey = not written), hidden, locked |
| Colour | own colour in the editor, otherwise by type |
| Notes | only in the project |
| Server keys | other values for Sphere (`EVENTS`, `FLAGS`, `GROUP`, `TAG.…`) and POL (`POL.EnterText`, `POL.NoCombat` …) |

### Areas

List of all rectangles (1, 2, …) and polygons (P1, P2 …). Enter values below and press **Add** or **Apply**. **Optimize** merges rectangles into as few as possible. Servers only know rectangles – polygons and circles are converted into rectangles automatically on export.

## Undo / history

Every change (also dragging on the map, imports, automatic fixes) can be undone with **Ctrl+Z** and redone with **Ctrl+Y**. The **History** tab lists all steps; "Back to here" jumps back several steps.

## Check

The **Check** tab finds among others: regions without name or area, duplicate names, rectangles outside the map or without size, wrong z values, child regions outside their region, overlaps with the same priority, go location outside the region or in the water, entrance in the water, unknown music, own region types. Problems marked with 🔧 can be fixed automatically. Double click shows the region on the map. Before writing into a server the check runs automatically.

## Files and formats

| Format | Read | Write |
|---|---|---|
| Project `.uoregions` (JSON) | ✔ | ✔ – everything incl. polygons, colours, notes |
| ServUO / RunUO `Data\Regions.xml` | ✔ | ✔ – unknown elements (e.g. `<spawning>`) and comments are kept |
| Sphere `mapN_areas.scp` (`[AREADEF]`, `[ROOMDEF]`) | ✔ | ✔ |
| POL `regions.cfg`, `justice.cfg`, `music.cfg` | ✔ | ✔ |
| CSV | ✔ | ✔ |
| Text, HTML overview, PNG map image | – | ✔ |
| C# classes of own region types (ServUO) | – | ✔ |

## Setting the server folders

In **⚙ Settings**:

- **ServUO / RunUO folder** – the main folder of the server (or `Data`, or the `Regions.xml` itself).
- **Sphere areas file** – e.g. `scripts\maps\map0\map0_areas.scp`. If the file is named `mapN…`, only the regions of that map are written.
- **POL regions folder** – the folder with `regions.cfg` (and `justice.cfg`, `music.cfg` if used).
- **Export folder** – target of "Everything into the export folder". Empty: `<AppData>\RegionExport`.

Then:

- **File → Load from server** reads the regions directly from the configured server.
- **File → Save into server** writes them back. A backup `file.YYYYMMDD-HHMMSS.bak` is always made first. Restart the server afterwards.
- **Existing Regions.xml:** *replace completely*, *replace only the facets of the project* (other facets stay) or *replace/add regions by name* (everything else stays unchanged).
- **File → Export → Everything into the export folder** creates:
  - `servuo\Data\Regions.xml` (+ `servuo\Scripts\Regions\*.cs` for own types)
  - `sphere\mapN\mapN_areas.scp` per map
  - `pol\regions\regions.cfg`, `justice.cfg`, `music.cfg`
  - `regions.csv`, `regions.txt`, `regions.html`, `regions.uoregions`

  Copy the files from there into your ServUO, Sphere or POL server.

## Notes on the formats

- **ServUO:** `<rect>` is written as `x/y/width/height` or `x1/y1/x2/y2` (setting). x2/y2 is the end (not included). Child regions only get their own priority/music when it differs from the parent.
- **Sphere:** `RECT=x1,y1,x2,y2,map` – x2/y2 is written as the end, like ServUO. Child regions are written as own `AREADEF`s with `GROUP=` of the parent. FLAGS are completed from the type (Town/Guarded → `REGION_FLAG_GUARDED`, Dungeon → `REGION_FLAG_UNDERGROUND`, all three → `REGION_FLAG_NOBUILDING`).
- **POL:** `Range x1 y1 x2 y2` with included corners. Regions with several rectangles get several `Range` lines in one block. The realms are britannia, britannia_alt, ilshenar, malas, tokuno, termur.
- Facets that are not in the map list stay in the project and are exported too.

## Client

- **⌖ Client** jumps to the position of the running client, **⟳ Follow** follows it.
- "Send client here" uses the same command as the map tab (options: map command and arguments, e.g. `.goforce`).
