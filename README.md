# UOPixelFiddler

## About

UOFiddler based on Ultima SDK it's a tool to view and alter almost every UO 2D client file. Source code is released under the Beerware and Wineware license.
This is an extended version of UoFiddler that has more maps and more tools, as well as Map Edit, and will be expanded over time to support the Ultima standard 
version more than the original version. Also, some source codes from older and New projects that are no longer supported have been integrated.

Version 4.12.24

**CompareItemControl: Rework**

*Bug fixes*
- Replace and remove now hit the correct item. With a filter active, the wrong items were overwritten before.
- Arrow keys act only once. Delete removes items and asks for confirmation first.
- IDs outside the second file are no longer compared against item 0. The last item is no longer missing.
- Fixed a memory leak in the context menu.
- The "Compare Land" tab no longer overwrites the second file of the item tab.
- The TileData window no longer crashes when it opens.

*Loading*
- Supports `artLegacyMUL.uop`. MUL is preferred when present.
- `tiledata.mul` is loaded automatically. Old and new formats are detected.
- Load via Enter, drag & drop or "Recent". Missing files are reported.
- Fast pixel comparison with a progress bar and cancel, without freezing the window.

*New features*
- Filters: All, Differences, Changed, Only Original, Only Second, Free.
- Multi-selection in both lists.
- Copy dialog: same position, from a start position, or search for free slots, with a preview.
- Copy TileData along, and review or edit it in its own window.
- Save Art and TileData.
- Export as Bmp, Tiff or Png, search in hex or decimal, "Next free slot".
- The preview fits large items into the view and shows TileData info.


Version 4.12.23

# MultiEditor 2.3 – UO Fiddler Plugin

An extended multi editor for Ultima Online. You can build houses, castles, bridges and decorations, learn from example buildings, plan on real map ground, and save the result as **multi.mul**, **map statics** or a **ServUO script**.
The UI and help are available in **English and German** (switchable, help with F1).

---

## Import

| Format | Notes |
|---|---|
| Text lists `ID X Y Z [flag] [hue]` | Hex (`0x..`) **and** decimal, detected automatically or forced |
| UO Architect `.uoa` | Binary version 1 **and 2** (hues are kept), UOA text |
| WSC / freeze, UOX3, CSV, XML | WSC/XML with hues |
| **ServUO / RunUO addon scripts (`.cs`)** | Arrays, `AddComponent(...)`, variable style, `AddComplexComponent(...)` |
| MultiEditor project `.mep` | Stores everything: size, origin, flags, hues, furniture, house markers |
| multi.mul | Tree list or the **multi browser** with thumbnails |

The editor also keeps a list of recent files and can import a tile list from the clipboard (Ctrl+V).

## Save & export

- **Directly into multi.mul / multi.idx**, either into the output folder or into the client folder (with a `*.bak_date` backup). You can also find the next free id or delete an id.
- **ServUO addon** (`Addon` + `AddonDeed`) in array or line style. Hued tiles are written via `AddComplexComponent`.
- **ServUO house script** (`BaseHouse` + `HouseDeed`) with:
  - south/east doors (wood or metal)
  - house sign and ban location
  - area, price, lockdowns and secures

  It works with ServUO and RunUO.
- Text (hex/decimal), WSC/XML with hues, PNG preview and project `.mep`.
- **Furniture** (decoration) is not written into multi.mul. You can export it as a separate addon instead.

## Building

- **Undo / redo** with 200 steps (Ctrl+Z / Ctrl+Y, also as buttons).
- Tools: select, draw, remove, altitude, pipette, transparent, **rectangle (filled or border only)**, line, box select, erase area.
- A rectangle border drawn with wall tiles of a learned style automatically gets the **correct corners**.
- Multi selection, copy / cut / paste as a stamp (R = rotate). Arrow keys and Page Up/Down move the selection.
- Zoom (Ctrl+mouse wheel), rotate, mirror, crop, set origin, replace ids.
- **Rotate the view by 90°**. Exports always keep the original orientation.
- **Symmetry mode**: X, Y or 4-way.
- **Hue picker** with color gradient and tile preview.
- Large **context menu** (right click) with all important actions.
- Shortcuts: `1-9, 0` tools · `B` structure · `M` map select · `E` furniture · `G` grid · `H` heights · `L` ruler · `F1` help

### Structure tool
- Drag a line to build a **wall**. Drag a rectangle to build a **room** with a floor and a **flat roof** or **gable roof (shingles)**.
- Drag over an existing building again to **add a floor on top** (can be turned off).
- **Learn line patterns** (e.g. wall – wall – window with battlements). The pattern repeats while dragging.
- Extensible through the `IStructurePattern` interface.

### Building parts library
- Save stairs, door frames, battlements, furniture and other parts from a selection, from map statics or from files.
- Each part has a thumbnail and a category. Place it as a stamp and rotate it.

### Layers & height ruler
- One layer per floor, or your own Z ranges. Each layer can be visible, ghosted or locked.
- "Show only this" and "set as build level" per layer.
- A height ruler at the border shows the floors, the build level, the top edge and the total height.

## House generator (learns from examples)

- It learns a **template** from good, finished houses (current multi, a multi.mul id range, or files). The template contains:
  - floor tiles and floor height
  - where the wall lines lie relative to the floor edge
  - the four corners and the four sides with their window pattern, for each floor
  - the roof: type, overhang, step and rows
- A house of the same size is **rebuilt exactly**; bigger houses follow the same rules.
- The example house is shown next to the result, **colored by what was learned** (corners, sides, roof).
- Tab **"Learned"**: view and correct every value.
- Tab **"Parts"**: place ready parts with position, floor and rotation.
- Tab **"Roles"**: correct wrongly detected tiles.
- Random houses, and restyling an existing multi into another style.

## Map as building ground

- Choose a map area on any map, with zoom, static search and automatic base Z.
- **Land is drawn like in the client**: each cell is stretched between its four corner heights (texture + lighting). The grid follows the terrain, and the height of every cell can be shown.
- Map statics appear in the background (optionally semi-transparent). "Snap to ground" places drawn tiles on the land height.
- **Select map statics** (single or area). You can copy them, take them as tiles, move them into the multi, delete them from the map or save them as a building part.
- **Write the multi into map statics**. All map changes can be undone until `statics*.mul` / `staidx*.mul` are saved.
- Other backgrounds: grass, water, sand, dirt, snow, forest, your own land ids or your own image (e.g. a floor plan).

## Checks & lists

- **Walkability check**: finds steps that are too high, missing floor, ceilings that are too low, and areas that can't be reached from a door or the ban location. Results are listed and marked in the view.
- **Material and tile list** with counts, roles, prices per piece and a total sum. Export as CSV, JSON or a C# table for crafting or cost scripts.
- **Multi browser**: all multis of multi.mul as thumbnails, with a filter. Double click opens a multi in the editor.

## Data files (in `plugins/`)

| File | Content |
|---|---|
| `multieditor.xml` | Tile groups (editable in the editor, a backup is made when saving) |
| `MultiEditorSettings.json` | Language, recent files, recently used tiles, options |
| `MultiEditorStyles.json` | Learned styles, templates and tile roles |
| `MultiEditorStructures.json` | Building parts and learned line patterns |

## Notes

- multi.mul stores no hues. Hues are only used for the preview, ServUO scripts and map statics.
- If the client uses `MultiCollection.uop`, pack it afterwards with the UOP packer.
- The walkability check approximates UO movement (person height 16, step 2, stairs up to 10). It is not an exact server simulation.
- Save your map files before writing statics.

Version 4.12.22

- # Animation Frame Editor

Rebuilt editor in `UoFiddler.Plugin.ConverterMultiTextPlugin` for creating, painting and checking animations from up to 10 frames.
*Neu aufgebauter Editor zum Erstellen, Bemalen und Prüfen von Animationen aus bis zu 10 Frames.*

## Features

- **Layers per frame / Ebenen pro Frame:** Original, Paint, shared Reference animation and Background, each with visibility and opacity.
- **Undo / Redo:** up to 60 steps, including multi-frame actions, with a history list.
- **Drawing tools / Zeichenwerkzeuge:** pencil, eraser, line, rectangle, ellipse (outline/filled), flood fill with tolerance, pipette, move. Primary/secondary color, alpha, recent colors, frame palette, color → transparent, replace color, outline.
- **B/W transparent (switchable):** black, white and light gray of the original images shown as transparent – on/off at any time, images stay unchanged.
- **Selection / Auswahl:** mark a rectangle (Select tool or Ctrl + drag) and load or paste an image into it, copy or clear the area.
- **Overlays:** floating images above the frames (e.g. a cage around a bird) – move, undo, copy, place on all / checked / one frame, pixel-exact scale, tint (original, gray, color), auto-crop of black/white borders, animated overlays from GIF or animations, bake into the frames when done. Black/white stay transparent like in UO.
- **All checked mode:** every edit applies to the current frame and all checked frames at once.
- **Alignment / Ausrichtung:** anchor (feet position, UO centers like the client), arrow-key nudge, offsets, align bottoms, pixel-exact scale, rotate, mirror, canvas size.
- **Comparison / Vergleich:** guide lines, pixel grid, onion skin, transparent reference animation, zoom (global or per frame).
- **Playback / Abspielen:** FPS, from/to range, loop, ping-pong, mirrored preview, ground backgrounds.
- **Sources / Quellen:** Auto (body.def/bodyconv), anim.mul–anim5.mul, UOP, `.vd` files, another client folder, images / animated GIF. Art browser and clipboard list are linked to the editor.
- **Files / Dateien:** save/open project (PNG per layer + `project.json`), export spritesheet, single PNGs or animated GIF.
- **UI:** toolbar, frame strip with all 10 frames (drag & drop), large canvas, side panel, status bar, bilingual tooltips (EN/DE) and help page (F1).

## Shortcuts / Tastenkürzel

| Key | Action |
|---|---|
| `1`…`0` | select frame 1–10 |
| `PageUp` / `PageDown` | previous / next frame |
| `Space` | play / stop |
| `B E L R O G I M` | pencil, eraser, line, rect, ellipse, fill, pipette, move |
| `S` / `Ctrl`+drag | select rectangle |
| `T` | B/W transparent on/off |
| `Esc` | deselect |
| `P` | overlay tool (drag, arrows, Del) |
| `X` | swap colors |
| `[` / `]` | brush size |
| `+` / `-`, `Ctrl`+wheel | zoom |
| Arrow keys (`Shift` = 10 px) | move active layer |
| `Tab` | switch layer Paint / Original |
| `Ctrl+Z` / `Ctrl+Y` | undo / redo |
| `Ctrl+C` / `Ctrl+V` | copy / paste frame image (with selection: paste into it) |
| `Ctrl+D` | duplicate frame to next |
| `Del` | clear active layer (with selection: clear the area) |
| `F1` | help |

Version 4.12.21

## Dragon Map Studio (new) – UoFiddler.Plugin.ConverterMultiTextPlugin

A complete rebuild of **Dragon 1.03.62 (IMod13)** inside UoFiddler: paint a map as an image, build `map#.mul` / `staidx#.mul` / `statics#.mul` (and `map#LegacyMUL.uop`), look at it like the client and edit it there.
Open it in the **ConverterMultiText** plugin → tab **Map** → button **Dragon Map Studio** (below "Uo Map"). All forms can be opened in the Visual Studio designer.

### Tabs
| Tab | What you can do |
|---|---|
| **Colour table** | IMod13 colour table: left click copies the colour address (format selectable), right/middle click = brush colour 1/2. Generate a table from the rules, define new colours. Mouse over a colour shows its land tiles / textures, items and transition statics. |
| **Map editor** | Paint the 8-bit (or 16-bit) map: brush, eraser, spray, line, rectangle, ellipse, fill, picker, height + / −, select / copy / paste, undo / redo, overview, heights shown from zoom 12, map list of the project folder. |
| **CGI view** | Client view of the built map (land art, textures on slopes, statics with hue) and a **2D view** from above. Edit statics, land, coast, water, caves, templates (see below). |
| **Generator & outline** | Random continents (seed, land %, mountains, snow, climate, beach, cave entrances) or land from an outline image (PNG with transparency, silhouette, mask). |
| **Rules & scripts** | The whole Dragon rule set in tables: groups, colours (maptrans), transitions with 3×3 pattern preview, items, unevenly, fix table, colour aliases, hints – plus the rules workshop (see below). |
| **Create MUL / UOP** | Build with all Dragon steps (colours → tiles, unevenly, map and static transitions, items, fix table), remove duplicate statics, repair before building, coast like the original map, `dragon_log.txt` with missing transitions and coordinates. MUL / UOP → BMP import (like Dragon_m2b). |
| **Settings / Help** | All settings in one place, help in **English, German and French**. |

### Highlights
- **Uses all Dragon scripts and addons** (IMod13 shipped in `DragonData`), reads and writes the original script folder format, plus an own JSON rule set with all information. Palettes: ACT, PAL (JASC/RIFF), ACO, GPL, BMP.
- **8 bit or 16 bit:** classic Dragon (256 colours, 8-bit BMP, 2-digit addresses) or extended (up to **65 536 colours**, groups up to 7fff, 16-bit greyscale PNG, 4-digit addresses). One switch in the rules tab, convert maps without loss, palettes with pages of 256.
- **Rules workshop:** coloured tool bar with the 8/16-bit switch, **+ New colour** (address check, RGB duplicate check, group, height, tiles from a picture catalogue), **+ New group**, **+ New transition rule** with an example file of the 12 standard patterns (edges, outer and inner corners), **Check**, **Fix hints**, save the rule set under a name (loaded at the next start), help with examples.
- **Script editor:** edit every Dragon script as text; for the line under the cursor it shows what it addresses (colour, group, tiles / statics as pictures, transition pattern as 3×3 picture) and a syntax help with an example per file type.
- **Map check and repair** following the Dragon tutorial: single tiles and 1-tile strips, swamp surrounded by grass, bridge groups between groups without transition, mountains rising in rings from the ground, height jumps, GrassBump lower than grass. Bilingual log files (`map_check_log.txt`) to send on.
- **Coast like the original map0** (values read with the built-in **map analysis**): sea floor `0064` at −15 with water statics `1797`–`179c` at −5, beach −3 / −1 / 0, shore statics at water level.
- **Map analysis:** reads heights of any map (e.g. the original `map0.mul`) – land z by distance to water, sea floor, shore tiles, coast statics, height steps → `map_analysis_log.txt`.

### CGI view (client view) – editing
- **Statics:** select, place (picture catalogue of all items, hue, z offset, stack), place multis, delete, colour; arrows move, Ctrl+arrow copies next to it, PgUp/PgDn z; copy / paste; selected statics are marked in the item catalogue (key **I** takes them over).
- **Land:** paint land tiles / textures (picture catalogue, big catalogue window for land, textures and items), height + / − / =, smooth, brush size 1–33. Floor and road presets from the colour table groups (cobblestones, marble, wood floor, cave floor …), mixed at random, optionally at one height.
- **Area tool:** fill, smooth, raise / lower, adjust coast, water, delete or select statics, copy / paste land + statics.
- **Coast brush** (shape a coast like the original), **water brush** (water statics; items with the tiledata flag *Translucent* are drawn see-through), **cave brush** (dig a tunnel into a mountain – floor at one height, the edge becomes the wall with the rock texture behind).
- **Templates (buildings / floors):** library with categories, search and preview. Reads **UoFiddler multi text** (hex and decimal), **UO Architect `.uoa`** (v1/v2), **world saves `.wsc`** and **Sphere `.scp`**; 586 buildings are included in `DragonData/Multis`. Place with a see-through preview on the ground height, then move with the arrow keys. Save a marked area or a selection as a new template (land with heights and/or statics) as `.dmsv.json`, UoFiddler `.txt` or `.uoa`.
- **Problems scan** (missing art, cliffs, statics below ground, duplicates), overview map with the editor selection, key help always shown below the view.
- Saving writes statics and (when changed) the map, UOP as option; old files are kept as `.bak`.

### Languages
The interface switches between **German, English and French** (menu *Sprache / Language / Langue*). Translations are plain text tables in `DragonData/Lang` (`en.txt`, `fr.txt`) and can be extended.


Version 4.12.20

### Tabs
| Tab | What you can do |
|---|---|
| **Items / Art** | EC world art, legacy art, effect and terrain textures |
| **TileData** | tileart.uop entries: names, flags, properties, textures |
| **Gumps** | Interface gumps and legacy gumps |
| **Art Converter** | Classic items and gumps → EC, own images on any id, report of what is missing in the EC |
| **Hues** | Editor (32 classic colors, preview on item / gump / animation, gradient), compare hues.mul ↔ Hues.uop, converter in both directions |
| **Animations** | Mobile animations; import from the classic client, sprite sheets |
| **AnimData** | Item animations, compared with the classic animdata |
| **Multis** | Multi editor with EC graphics |
| **Map** | Facets, statics, region viewer with editing tools |
| **Map Converter** | Classic maps ↔ EC facets, analysis, repair, new maps |
| **Sounds, Cliloc, String Dictionary, Interface / Data, UOP Browser** | the other EC files |
| **Search** | Find an item, gump, hue, multi or name everywhere; double click opens it |
| **Check** | One check of the whole client with a report |
| **Patches** | Create, save and apply patch packages (.ecpatch) |

### Highlights of 0.17.0
- **Search** across the classic client and the whole EC (optionally all maps).
- **Map editor tools:** move statics (drag, arrow keys, PgUp/PgDn for z, Del), place items with an item search window, paint land (border layers rebuilt automatically), place multis.
- **Hues:** preview a hue on an item, gump or animation; gradient from two colors.
- **Check:** tileart, missing texture files, multis and item animations with unknown items, hues, maps.
- **Animations:** convert classic animations to EC (optionally 1.5×), export/import sprite sheets.
- **Patches:** collect changes, save as one file, apply to any client folder with backup.

### Notes
- Texture.uop and LegacyTexture.uop are signed (version 5); changed entries are written without signature.
- Every write keeps a backup (`.bak` or `UoFiddlerBackups` with old + new copy).

Version 4.12.19

## What's new in UoFiddlerPixel

### UoFiddler.Plugin.Enhanced (Enhanced Client editor) – version 0.15.0
- **Multis:** preview with the correct EC graphics (houses, boats, sails), heights, highlight single parts,
  free slots, context menu (import, export, remove, reload, save), background image.
- **AnimDefinition:** side-by-side comparison with the classic Animdata, each side can be shown or hidden.
- **Map:** mark an area, list all statics in it, region view with Enhanced graphics,
  import, export, insert and save statics.
- **Map Converter (new tab):** classic maps (map#.mul, statics) ↔ EC facet#.uop in both directions,
  radar image, automatic file choice per map, analysis and repair, create new maps.
- **Hues (rebuilt, huesmul2uop built in and improved):**
  - Editor with the 32 classic colors you can click and change; hues.dds and the overview are always
    written too, so items and animations now show the same color.
  - Compare: hues.mul ↔ Hues.uop side by side, filters, copy marked hues in either direction.
  - Converter: hues.mul → Hues.uop as an update or a new file, Hues.uop → hues.mul byte for byte,
    ramps drawn like the original client or like huesmul2uop, backup (old + new), analysis and repair.
- **Help** in English and German.

### UoFiddler.Plugin.UopPacker – version 2.0.0
- Restructured, the old tab style kept, bugs fixed.
- New tabs: Multis (MultiCollection.uop + housing.bin), UOP Info, Backup, Help (EN/DE).
- Automatic backup (old and new file in one backup folder), with restore.
- Supports art, gumps, sounds, maps (including larger custom maps) and multis.

Version 4.12.17
- **MultiMap images** (MultiMap tab → "Create image..."): create an image of any map – built-in, from the map list or external map files (also only an area) – or of any image file. Styles: **ink map** (drawn like a treasure map: coast lines with waves, mountains, trees, houses, dotted roads, sand/swamp/hills), radar colors, grayscale, black/white (dithered like MultiMap.rle) and sketch; options for parchment look, frame, coordinate grid, symbol size, density and random pattern.
- **Image list**: generated, saved or added images appear under "Load.." (stored inside the app or linked from any folder); open image files, paste from the clipboard, manage (rename/remove/open folder).
- **Editor** ("Edit..."): pen, eraser, line, arrow, rectangle, ellipse, text labels (font, color, light border, rotation), map symbols (mountain, tree, house, ankh, skull, compass, ...), fill, color picker, select/crop/cut, rotate/flip/resize, grayscale/black-white/invert/sepia, undo/redo.
- **Mouse wheel zoom** at the mouse position in the MultiMap tab, the generator and the editor; drag to move.
- **Print** (fit to one page or as poster over several pages, print preview), **copy** to the clipboard, **save** inside the app or in any folder as PNG, JPEG, BMP, GIF, TIFF or ICO; export now also as GIF.
- "Generate.." → MultiMap / Facet **from the current image** (automatically converted to 2560 x 2048 black/white).

Version 4.12.16
- **Map Editor** (map tab → "Map Editor..."): copy areas (land/statics) of any map – also external ones – edit them (rotate, mirror, filter statics) and paste them on any map with height adaption at the border, live preview and saving to your own folder/file number.
- **Tools**: brushes 1–64 (square/round), paint texture, replace, raise/lower, flatten, smooth, erase statics, eyedropper – every stroke is one step with **undo/redo** (Ctrl+Z / Ctrl+Y).
- **Brush editor**: define patterns as a grid (which tile goes where, variants, heights, statics), show numbers, move cells, categories and search, copy from the map or import from a picture.
- **Transitions** define edges, corners and inner corners between terrain groups and set them automatically.
- **Lines, curves and shapes** (rectangle, circle, triangle), filled or as outline, drawn with any tool.
- **Generator**: settlement with plots, floors, road and paths, lake with shore, rough ground, trees and plants.
- Fast map overview, help with F1 in every window.

Version 4.12.15
- Region-Viewer: View the selected area in the client view (iso, textures, hills, statics with hue), zoom, show heights/grid, look inside buildings.
- Error Check & Log: Invalid IDs, duplicate/hidden statics, steep land; automatically repair, notes, export as CSV/XML.
- Build:** Place multis from multi.mul, custom library or files (TXT dec./hex, UOA, WSC, CSV), remove obstacles, level the ground, Undo (Ctrl+Z).
- Export Area: Check the selected map area and take it over as a file, into the library, into multi.mul or as a ServUO-Addon/Decoration.
- Building Library: For a very large number of entries, standard in the output folder (UoFiddler_BuildingLibrary), movable.
- Save with Backup: Target selectable (Output, Original or other folder), automatic backups and restore; Multi-Script now sets the Hue.

Version 4.12.14
- New map management system with support for an unlimited number of maps, automatic detection, and flexible configuration.
- New client-style Region Viewer (GDI+) for analyzing, validating, documenting, and fixing map and static errors.
- Selected areas can be directly captured, viewed, and used for map replacements; coordinates are automatically copied to the clipboard.
- Numerous bug fixes, along with significantly faster, more stable, and more reliable map loading and management.

Version: 4.12.13
- Map maker revised

Version: 4.12.12
- Added Bibliothek Arts
- Added Bibliothek Landtiles and Textures
- Added Bibliothek Gumps

Version: 4.12.11 
– Compare Maps revised
- Made `CalculateDiffs()` asynchronous (`Task.Run`) to prevent UI freezing  
- Fixed null‑check bug in `CalculateDiffs`  
- `OnMouseMove` now triggers `Invalidate` only when tooltip content actually changes  
- Corrected bitmap disposal throughout the code  
- `ZoomMap` now disposes the old bitmap after replacement  
- Added CSV export for diff list  
- Added screenshot feature to save current viewport as PNG  
- Added Next‑Diff / Previous‑Diff navigation  
- Added diff statistics to the top toolbar  
- Added manual zoom via text input  
- Introduced modern dark UI  
- `OnPaint` now clamps block coordinates to map bounds
- List components that can be shown, hidden, or toggled open.

- `CalculateDiffs` now uses `Math.Min` for map dimensions to avoid out‑of‑range errors  
- Replaced `bool[][][][]` with `HashSet<Point>` for block‑ and tile‑level caching → ~90% less RAM, O(1) tile lookup  
- Added versioned cancellation token (`_diffVersion`) to discard stale async results  
- Replaced tooltip string concatenation with `StringBuilder`  
- Replaced thousands of `DrawRectangle` calls with a single `GraphicsPath` batch draw  
- Screenshot now uses `InvokePaint` instead of `DrawToBitmap`  
- `ScrollBar.Maximum` is now clamped directly in `ChangeScrollBar`  
- `Dispose` override now unregisters all event handlers to prevent memory leaks  

- Added “Diff List…” dialog  
  - Displays all diff block coordinates (BlockX/Y, WorldX/Y)  
  - Clicking a row jumps directly to that location  
  - Includes “Jump” button for selected entry  
  - “Copy All” copies the full list to clipboard  
  - Auto‑refreshes when new diff calculation completes

Version : 4.12.10
- ParticleGray Tool.
- Added a search function in Amindata based on hex addresses and names.
- Added a Artwork Gallery Tool in Artworks Items.

Version : 4.12.9
- Fixed importing animation frames from gif files.
- Reading of compressed Cliloc files is possible but saving will still result in old cliloc format.
- For Cliloc comparison tab you have now additional check boxes to decide if you want to read clilocs in new format.
- In order to read new Clilocs you need to enable option in Settings -> Options (it's disabled by default).
- Added PNG format when inserting or replacing graphics.
- Animation Export Funktion Bnp tiff and jpg and png.
- Export all Null function.
- Gumpart and cliloc write encryption support.
- Added ProgressBardialog2 add Cancel Button and funktion (items).
- Added ProgressBardialog2 add Cancel Button and funktion (LandTiles).
- Added ProgressBardialog2 add Cancel Button and funktion (Textures).
- Added ProgressBardialog2 add Cancel Button and funktion (Gumps).
- Added ProgressBardialog add Cancel Button and funktion (Items).

Version : 4.12.8
- Bugs fixed and revised.
- Nuget Update System Drawing Common 6.0.
- Gumps: Added listing methodform.
- Add Animation XML Editor.
- Changelog revised search revised.

Version : 4.12.7
- Added methods to load, stop, and play random MP3 files from a directory, 
  including logic to prevent song replay when manually stopped and enabling randomized song selection upon song end.
- Added Com : medien player lib.
- Update Nuget Paket.
- RoundButton class added for toolbox.
- Items: Added Mass Removal and Select with Ctrl.

Version : 4.12.6
- Added backup directory copy feature (Path Settings). 
- Added BtnLogDir Open Log Dir (Options).
- Added function reads the colors of an image, displays them in a RichTextBox, and allows visualization in a PictureBox through highlighting. (Items).
- Added map cordinate to clipboard (map)
- Added Image animation management with support for image import from clipboard and load and customizable animation speed.
- Added Tiledata Item and Land Copy and Insert all Settings, into contex menu. And multiple import.
- Added an export function in Gump for all addresses to a text file in decimal or hexadecimal format. 
  Also added an import function to import from a text file and directory with decimal or hexadecimal image names to the specified address.
- Added Gump Mirror function.
- Added hotkey (Ctrl + Alt + P) to bring the Profile window to the front and play a sound.
- Added UOP Packer autofill feature that automatically populates textboxes based on file selection and file type when the checkbox is enabled.
- Added AnimationEdit Lists all IDs in a text file.
- Added a new label to the Items toolstrip to display decimals, making it more user-friendly than before.
- Added Items - toggle for rhombus function via button.
- Added Items - an additional PictureBox with dimensions 1000x1000 to display larger images for testing and zoom function.

Version : 4.12.5
- AminEdit added new features, Body Human display will be expanded, and Mounted.
- Serilog package update.
- IronSoftware.System.Drawing package update.
- Octokit package update.
- Serlog.Sinks.File package update.
- Microsoft.Web.WebVie2 package update.
- Added package AnimatedGif.
- Added support for hued AnimData, export AnimData to Gif.
- Added ability to export design as XML in multi editor plugin.
- Various improvements to RadarCol control.
- - save radarcol on treeview selection change.
  - enable revert, revert all for radarcol changes.
  - add "range items to individual average".
  - add "range items to range average".
  - add filter and checkbox selections.
- Added Copy hex address to clipboard.
- Added Background water and grass added to the picture box in multis.
- External Map Replace expanded in expansion.- 
- Added gif export to Dress and Animation tabs  (author KevinEady)
- Added UOPacker plugin can now unpack multicollection.uop (author Stefano Merotta)
- Added tooltips to tiledata tab (author Dragon Slayer)
- Added edit function in Tiledata tab to edit Cou text files.
- Added occupied item counter Items tab.
- Added Frame Counter.

Version : 4.12.4
- Add About revised, removed more Richtextbox and added a matrix animation with the names of the developers, new links, and new information.
- Add Sound Loop.
- Added functionality to update the count of occupied and free sound IDs in real-time when sounds are added, removed, or the view is reloaded.
- Removed Escape key function, changed F2 key function to Ctrl+E, added Space key function to play/stop sound.
- Update Nuget Paket.
- Implemented functionality in the ServerStartBox for selecting, saving, and loading paths for the ServUO directory and Visual Studio, 
  as well as for starting the ServUO server or another server directly through the .exe.
- Add DesignStudio
- Fixed: When importing an image from the clipboard, the selected graphic ID was not retained in the LandTilesTileView.
- Added functionality to toggle LandTilesTileView background color between white and black upon clicking background.
- Add a ColorBox that enables interactive analysis of color information in an image. 
  By selecting pixels, the associated color values are determined and displayed in a RichTextBox.
- Add a ColorBox Texture.
- Add TextureColorForm allows for interactive color adjustments of textures.
- Fixed importing unicode font characters
- Fixed Amindata Start Error/stop error
- Added export to XML (centred+ format) in Multis tab (author Prapilk)
- Add support for import/export AnimData by @KevinEady
- Add TIFUnicode from (author Prapilk)
- Add Implemented color selection and saving for form and controls.
- Add image converter: black and white, different colors, and functions.
- Add Paragraph-to-Line Converter.
- Add XmlException if XML is missing Load Animationlist.
- Add Animationlist Editor.
- Add Main button added for Ultima Online directory.
- Add Admin a HttpClient CheckInternetConnectionAsync.
- Add "ItemSearchForm" expanded and restructured.
- Add Items a Color to Grayscale.
- Add Multi - Visual highlighting of index components in MultiComponentBox.
- Add Item “Store Image Name and Hexadecimal Value in Temporary Storage".

Version : 4.12.3
- Add Items Image Colors and Update.
- Add Script Creator - Book.
- Add Help - Tutorial Animationen Install German and Englisch.
- Add Animation - Edit UoBodyconv and Mobtypes Form.
- Add Animation - Script Script Creator.
- Add Animation - Script Script for RunUo and Sphere.
- Add Animation - Script Edit Animationlist.
- Add Creater Amin.idx and Amin.mul Empty.
- Add ScriptCreator Buttons Edit and Scripts
- Add Items Mark.
- Add Items Goto Mark.
- Add LandTiles Mark.
- Add LandTiles Go To.
- Add LandtilesView into TileArtForm.
- Add Editor for Tilesbrush.xml (CentrEd)
- Add Transitions - Graphic (From Prapilk).
- Add TileViewContextMenuStrip_Closing To focus on the index.
- Add Ciloc a Copy Clipboard Only
- Add Ciloc Advanced search function that highlights matches and navigates through results.
- Add AddHtmlLocalized Form created, fills text fields, generates and copies code for HTML localization
- Add Aboutbox Link and Repository Info

Version : 4.12.2
- BildFusion Add Sharp , Clipboard, Texture Cut.
- Make Texture, White is transparent, and black represents the transition, Texture into Texture, Colors.
- Add triangle for Tiles TextureWindowForm.
- Path Settings, Reload all Files and New, Load Single Mul file, Delete Line
- Add New Map Forell into SDK Map class
- Add Muls into SDK File class
- Add Map Forell into Map Controll and make nee class.
- Add Options %AppData% open dir
- Add Options New Map Forell
- Add Insert Maker Delete function
- Add Singe UOP Extract and Single Art Extract
- Add Pass coordinates
- Created Uo Art Merge class, which is a multifunctional tool for moving Art Mul images, 
  including shifting, saving, same height, showing only differences, free ID choice, mirroring, removing, copying to clipboard, 
  importing from clipboard, saving XML directories, ID deletion, and hex search.
- Add Label Infos
- Add Image Details
- Add MapReplaceNewForm
- Add Map7 Dragon
- Add IsoTileSlicer
- Add Octokit 9.1.2
- Add UoMap
- Add Map8 (Intermediate world)
- Add MultiScript
- Add Land Tile Art

Version : 4.12.1
- Implemented copying multiple graphics to the clipboard with Ctrl+X.
- Add Strg+F3 Search.
- Add ParticelGreyHueShadow and ParticelGreyHue CheckBox into TextureCutter.
- Add Items ParticelGrayHueShadow.
- Add Items ParticelGrayHueShadow Colord Dialog.
- Add Items Checkbox ParticelGray.
- Add Items Rhombus.
- Add Items Grid Image.
- Add Items Grid Color.
- Add Items Copy Clipboard - DetailPictureBox.
- Light class completely revised.
- Added size to light to scale the size of the light image.
- Add Landtiles Zoom Form.
- Add Texture editor for tiles incorporated into the textures.
- Add Picture Preview.
- Add Added color change so you can change the pixel colors.
- Add Mirror, scale textures to 64x64 and 128x128, rotate right left and create textures.
- Add Backgrund Button.
- Add Texture creator.

Version : 4.12.0
- Update to net8.
- Add Custom Sound Gumps Button
- Add Alarmclock Snooze Timer 5 min
- Gump - Instant search and focus with ‘Enter
- Gump Pagedown and PageUp
- Add Animation Html
- Add Icons ToolStripComboBox1 Stairs
- Gump ID Calculator and Buttons
- Add Animation Checkbox Free IDs Color Blue

## Changelog
Version : 4.11.26
- Add Map Tools
- Added XML debugging hexcode
- Add Create Art and Artidx Mul tool Texture Tiledata
- Load Menu : Saves the last selected profile, reselecting it upon startup
- Search Animation

Version : 4.11.25
- New feature added: Names outsourced to Gumps and stored in an XML file, with custom formatting and editing based on ID.
- Search function adjusted to include names as well.
- Added sound button and disabled messages
- Gumps Added a tagging function for IDs in the Gumps.
- Add a Scréenshoot Button Main Contextmenu.
- Octokit 9.0.0
- Serilog 3.1.1
- System Resources Extension 8.00
- Microsoft Web WebView2 1.0.2151
- Button Binary Code
- Button Morse Code
- Calendar
- Added zoom function in the tildedata -> Items

Version : 4.11.24
- Added ItemControl a Built-in image swap function
- Added a notepad that saves information into an XML file, serving as a reminder that can be edited later.
- Added a Notes form in the main menu to display messages.
- Added a Animtionen Edit
- Octokit 8.1.1
- Discord Link fixed
- Added Main Images 5 New
- Incorporated mirror function for the context, allowing for individual image loading and bulk image loading.
- Added a Gump Edit new System.

Version : 4.11.23
- Fixed an issue in Texturecutter regarding the input of hex addresses so that it works with and without the "#" symbol.
- Added a checkbox that, when active, moves the input from one textbox to another.
- Octokit Update 8.0.1
- Added an alarm clock feature where you can set a time for the alarm to go off.
- The Alarmclock now saves its last positioned location.
- Settings are now saved, including the size of the MainForm and whether it's maximized, in the personalized settings. Positioning, however, is still not exact.
- The AlarmClock now has the feature to load an alternative wave file as an alarm sound.
- The Dec Hex Bin Converter now also saves the position where it was placed.
- TextureCutter now saves its position as well.
- Added a mask in TextureCutter for alignment during cutting and unloading the mask.
- Added a checkbox for the ideal height and width of image measurements.
- Added functionality to the Graphic Cutter to mark areas on images and copy the marked area of the graphic to the clipboard.
- CompareItems - Fixed the System.NullReferenceException error.
- Added a ruler for pictureBox1 in GraphicCutter.
- Added CheckBoxLines to fill textures within lines.
- Fix NullReferenceException CompareMapControl load Button
- Added a function to MapControl that allows drawing on the map, and the coordinates are displayed in a MessageBox for the respective area.
- Added a label to Animation that displays information about the .vd file, import details, and the last ID slot address.
- Added a button and a form to Convert MultiTextControl.
- The method OnClickSelectTiledata in the ItemsControl class now attempts to update a PictureBox in the TileDataControl class when a certain ID is selected.
- Added Texturecutter PaintBox

Version : 4.11.22
- Added a ContextMenuStrip button for the hex address to export the selected items to the clipboard. In the AminData function, implemented logic to pass the hex address to the TextBox in the ContextMenuStrip when there is a hex address in the clipboard.
- Added images for the Gumps and tooltips.
- In GraphicCutter, added a checkbox and a transparency function, as well as an import function via the clipboard.
- Assigned the arrow keys for moving in the GraphicCutter. Added the ability to copy the image from PictureBox2 to the clipboard, and a sound plays when copying with Ctrl+X.
- Added Mapmaker form and a button.
- Added a "Circle" checkbox and a "Freehand" checkbox to draw shapes in the graphic frame. The default selection is a rectangle.
- Added a function to fill the marked area with a texture or graphic.
- Added a zoom button to allow zooming in on the image.
- Added a reset button that clears the selections and resets the zoom level.
- Added a button to open the tempGrafic in the main directory.
- Expanded and redesigned functions in TextureCutter, added optimization, brightness, gamma, and contrast adjustment with increments of 0.01. Also added an RGB checkbox that, when active, adjusts the values by 0.01 when moved up or down.
- Added a Trackbar to control the entire process.
- "TextureCutter" Drawing function now directly calculates on the cursor with zoom functionality.
- "TextureCutter" Added a fill function so that you can include rectangle, circle, or freehand textures here as well.
- Panels now display the color that will be applied to the image when using the pipette, which takes the color from the TextBox.
- Revised the zoom function for Textcutter and added a label to display the zoom factor.
- In Tiledata, added a button that clears all values, with a sound when activated.
- In Tiledata, added a new listing for "tree" and a "clear" function.
- Revised the "Sharp" button, added a Trackbar to control the sharpness level.
- Improved the White Balance function, refined the values, and added a Trackbar to set values from 1 to 10%.
- The mode now has a label, and when applied, the image is sent to the PictureBox. Brightness has been refined to be more precise.
- Added a second textbox for color values, along with a second ColorDialog to select values for the textbox.
- The drawing function now allows you to draw with a second color when it is set in the textbox and activated with the checkbox.
- Added a "Draken" checkbox to reverse the White Balance and make it darker.
- The design has been updated.
- New icons have been added.
- TextureCutter Two buttons for coloring have been added.
- For the Gumps and Landtiles, the image is adjusted in the clipboard with black, and the color d3d3d3 is converted.

Version : 4.11.21
- Added graphics for "Compare Items."
- Added "Compare Items." two buttons for individual and bulk imports.
- Expanded the left textbox with a preview feature so that when you click on the item, it gets imported to the selected position and removed from the list of choices.
- Expanded the left key binding so that when a key is pressed, it triggers the btLeftMoveItem method in listBoxSec, moving the selected item.
- The last selected ID address is added to the search textbox.
- Added a "Remove" button to clear the ID slot, and this method is also triggered with the right arrow key.
- Added two buttons, "Insert Items" and "Remove Items," to the "Compare Land" feature. The arrow keys are also assigned for this purpose.
- Added "Compare Land" a hex address search feature.
- Added "Compare Land" a decimal listing for the list boxes.
- Added "Compare Land" a "Move Multiple" button to move multiple selected graphics.
- "Compare Land" You can select multiple items in listBoxSec by holding the Ctrl key and either clicking with the mouse or using the arrow keys to move up and down.
- Added two buttons, "Insert" and "Remove," to the "Compare Texture" feature, and assigned them to the left and right arrow keys.
- Expanded listBoxOrg and listBoxSec to display decimal values as well.
- The listBoxOrg and listBoxSec in Compare Items were also expanded to display decimal values.
- Added a ColorWithTolerance function to replace color values within a specified tolerance range "Texturecutter".
- Added a Textcutter button in a form that lists all colors, including a label, a button to set the hex value into the textbox, and a refresh function to reread.
- Added a button in Textcutter that reads the most common color values from the image, listing the top 20.
- In Texturecutter, a form was added with a mode that includes the following functions: grayscale conversion of the image, changing brightness, adjusting contrast, gamma correction, modifying saturation, and altering color.
- Added a checkbox to prevent colors from being affected (i.e., #FFFFFF and #000000), as well as a reset button that sets the TrackBars and labels to 0.

Version : 4.11.20
- Added textboxes to TextureCutter to clear color values from an image and added a context menu to copy the image to the clipboard.
- Added an import function to Texturecutter to insert the image into the PictureBox.
- Added a refresh function to Texturecutter to update the image when color is replaced; this can be repeated as needed.
- Added a dropper tool to Texturecutter to retrieve the color code from the image in the PictureBox.
- Added a mirror function to Texturecutter that horizontally mirrors the image in the PictureBox.
- Added Ctrl+V and Ctrl+X functionality to Texturecutter to copy and import the image from the PictureBox.
- Added zoom in and zoom out buttons to Texturecutter, as well as zoom functionality using the mouse wheel.
- Added a reset button to Texturecutter to restore the original size.
- Added a ColorDialog to Texturecutter that places the color values into the textbox.
- Added coordinates that are displayed on a label.
- Expanded the ToUpdate function to work with the zoom function and refresh properly.
- Added indexed colors.
- Added Label Colorcode
- Added a "Draw" button to allow drawing on the image with the respective color.
- Added an "Erase" button to allow using the mouse to erase a specific area.

Version 4.11.19
- Add Decripter Client (ConverterMultiPlugin)
- Add Zoom function Mouseweel Plugin Compare Map
- Add Radarcolor Label Items and Land Name.
- Add Radarcolor Updatebutton TreeView
- Change Design Radarcolor ColorTreeView = DrawAll
- I added the function to AminData that transmits the current hexadecimal address to the textbox, the Add button is modified to include the function that sends the next hexadecimal address to the textbox with "Add," so that you only need to press "Add."
- Update octokit package.
- Add Links (Servuo, Discord)
- Added two context menu functions to the texture control: "Copy Hex Address" and "Copy Decimal Address" to clipboard.
- Add Icons
- F1-F12 keys reserved for tabs
- Page up page down also changes the tabs
- Textures: Added a sound function that allows only the sound to be heard while deactivating the MessageBox when the sound function is active.
- TileData LandTile: Sound enabled during caching.
- Added a label that continuously displays the decimal ID of the position. Clicking on the label will insert the ID into the textbox.
- Font: Added functions for importing and exporting to the clipboard via the context menu or Ctrl+V and Ctrl+X, along with an optional sound function that can be activated.
- Font: The function replaces #d3d3d3 with #ffffff when copying to the clipboard.

Version 4.11.18
- Added a Performzoomstep to the map.
- Map - Getmapinfo: Added Getmapinfo view for textures and static items. Map details now also work with the texture.
- Zoom factor increased to 16 on the map
- Added Map Replace Tiles PictureBoxes to display the tiles and static elements. An update function has been implemented to refresh the RichTextBox for displaying XML data. This also enables the XML to be saved to an external directory. An 'Open Directory' button has been included to facilitate directory access. Graphics have been incorporated, along with buttons to navigate and view between the statics and tiles.
- UOP Packer CheckBox added, which allows the file to be overwritten if active.

Version 4.11.17
- Animation: Made frames visible again.
- Frames highlighting added.
- Export to clipboard added for graphics.
- Import from clipboard added for graphics, save function is still missing.
- Rewrite the Animation.xml function again to save it in an external directory.
- Added a label that shows the source of the animations.
- AnimationEdit has received an import function to import graphics from the clipboard.
- AnimationEdit has received a function to copy graphics to the clipboard.
- AnimationEdit has received a Mirror function to flip the graphics horizontally.
- AnimationEdit KeyDown functions have been added to enable importing with Ctrl+V and copying with Ctrl+X.
- AnimationEdit has been given a function to highlight the selected frames.
- AnimationEdit has received a 90-degree rotation of the graphic to the left.
- Added calculator for Binary, Decimal, and Hexadecimal.
- Added a Delete button to the LoadProfile that deletes the selected entries.
- Added "Find Empty IDs" functionality to AnimationEdit.
- AnimationEdit added a feature to display only animations with specific IDs in the context menu.
- The middle mouse button has been assigned the function of copying the settings in the tiledata. This function is only active when a setting has been selected, and the Ctrl+Y also enables this function now.

Version : 4.11.16
- Design and graphics adjusted for the methods.
- The Multi has been equipped with an index search feature.
- Downgarde net7, texturecutter image reloaded

Version : 4.11.15 
- TextureCutter: I have added a texture cutter that trims textures to the respective size of an image, such as 33x33, 44x44, 64x64, 128x128, and 256x256, all in a sequence of multiple images.
- TextureCutter: Added that it is allowed to crop only smaller than 5x5 by enabling the checkbox, but it is at your own risk as it can easily create over 1000+ images from a single image.
- TextureCutter: has added a new feature to create tiles for LandTiles from textures. It also includes a resolution function to enhance graphics, a sharp function, color enhancement, white balance, and a save button.
- Tiledata: The idea Taken over with the search directly from AsYlum, The design here was set to graphics. (The old search forms have been kept and not removed.)
- Soundmultab Great job on incorporating the Sound tab from version 4.10.8 
- Removed color definition from the "draw default" for background tabs.
- Adjusted the default name of the Arts.mul file in the UOPPackerControl to include it in the "convert to mul" process and in the creation of the "artLegacyMUL.uop" file.

Version : 4.11.14 
-  LandTiles has received another function, which allows importing using the keyboard shortcut Ctrl+V and to copy using Ctrl+X.
- The items have gained the function to import using the keyboard shortcut Ctrl+V and to copy using Ctrl+X.
- The textures have also gained the function to import graphics using the keyboard shortcut Ctrl+V and to copy graphics using Ctrl+X.
- Gumps has also received the keyboard shortcut Ctrl+V for importing and Ctrl+X for copying.
- TileData has received a button that saves the settings on the selected ID repeatedly with each click. This function is only active when there are settings loaded from memory.
- MainForm has received a new function that opens the selected directory, specified in the options path, in Explorer.
- Added tooltips Tab Hues.mul
- Added Icons and to Contextmenu
- Updated to net 8 release.

Version : 4.11.13 
- Added Multis contextMenuStrip1 Copy Image to clipboard.
- Admin tool form set up to be opened only once.
- ChangeLog extended to display colors when I define them with  abbreviations in the  colors. Here you now set the colors with the text.
- Graphics added and replaced, function added for selection
- Gumps: I have added a button "Show all Free Slots" that displays all empty IDs in the Gump.mul. I have also modified the method "PopulateListBox" accordingly.
- ToolTip Text added

Version : 4.11.12 
- Added a mirror function to items.
- Added tooltips to items.
- GraficCutter: toolStrip ComboBox Added green, water clear.
- GraficCutter: Added the "changeBorder" function that makes the graphics transparent.
- GraficCutter: Added graphics.
- GraficCutter: Added Color selection added for grid
- MultiControl:  tabPage6 add contextMenuStrip3 and Copy Text

Version : 4.11.11 
- A new link has been added to uodev.uo-freeshards.de.
- A new tool has been added: a graphic cutter for arts.

Version : 4.11.10 
- Documentation now available.
- Added logo selection in the main menu under the context menu, with multiple graphics to choose from.
- The logo selection is now being saved, and it will be displayed again upon restarting with the selected logo.
- Two links to the German community focusing on development, graphics, and design have been added.

Version : 4.11.9 
- Changelog added.
- Changelog.txt added.
- Changelog Search feature added.
- System.NullReferenceException: Sound Button Add\Replace is fixed (If no file is selected).
- AdminTool Added a ping and tracert button.
- Help link removed, a documentation form created with Help Doku so that the application description is local, the whole thing is made possible with WebView, the cache is stored in %LOCALAPPDATA%, and when UOFiddler is closed, the UoFiddler.exe.WebView2 folder is deleted.

Version : 4.11.8 
- LandTile added: a 90-degree rotation to the left.

Version : 4.11.7 
- For the MainForm, a TabPanel_DrawItem event was added. The tabs are being reloaded and sorted differently. Additionally, the tabs have been assigned their own colors for identification purposes.
- Texture - Another function added: a 90-degree rotation to the left. It works with all sizes, such as 64x64, 128x128, or larger.

Version : 4.11.6 
- The Light has received another extension. It now includes two forms, each containing LandTiles and StaticItems, allowing for browsing and previewing them in advance. The forms can be positioned flexibly and function across tabs, enabling scrolling up and down.

Version : 4.11.5 
- Enable multiple selection for saving in Items and LandTiles by pressing the Ctrl key, then selecting the desired items, and finally clicking on the Save button with the respective format.
- Light has received the functions of copying and importing from the clipboard, as well as copying from the clipboard to the ID slot.

Version : 4.11.4 
- Multiple functions have been added to Radarcolor, expanding the graphical representation. Loading of data now utilizes ColorTreeView for graphical display. When saving the Color Palette, it is also displayed with colors. 
By activating the checkbox, the Color Palette's hex code is copied to the clipboard. Another textbox lists the hex codes, and there is a textbox for the 15-bit color depth per channel. In this case, the color code represents the intensities of the red, 
green, and blue color channels, each with 5 bits, making it compatible with Photoshop's color code. Additionally, the Photoshop textbox has a clipboard function that stores the color code when clicked.

Version : 4.11.3 
- A copy function has been added to the Controls, Items, LandTiles, Textures, and Gumps, allowing the graphics to be copied to the clipboard for direct use with copy and paste.
- Import function revised again, direct import through buffer added, existing SelectImageFormatForm class retained for outsourcing to Temp to store the graphics there and addressed this with new importByTempToolStripMenuItem.
- Import clipboard - Import graphics from clipboard (Items, LandTile, Gumps, Texture)

Version : 4.11.2 
- Added a new ConverterMultiText to convert Decimal to Hex. This allows for converting old Sphere Multitexts, enabling their import into the Multi.mul file in UoFiddler. Additionally, for the sake of preserving the old scripts, they are automatically placed in a folder named "Oldscript".
- The Tiledata has received a copy button that copies the selected settings from the selected location. You can now place them at any location by pressing again, save and you’re done.
- An XML editor has been added to the multis that edits, saves and recreates the Multilist.xml. A reload button has been added that reads in the Multilist.xml again without having to restart the program. A save button that copies the Multilist.xml and stores it in the OLDScript directory.

Version : 4.11.1 
- CompareItems expanded.
- Multi-selection added for saving images (Bmp, Tiff).
- Graphics can now be placed at any hexadecimal address.
- Display of the placed graphic.
- Hexadecimal address search added.
- The selected directory is now exported to its own directory called 'DirectoryisSettings' and saved as 'CompareiItemsDirectoryisSettings.txt'. It will be loaded on the next startup until a new directory is set."
- Added combo box for multiple selection of directories to quickly switch between them.
- Tiledata now has a ToolStripComboBox that sets predefined patterns for selecting settings.
- Tiledata now has a sound button that disables and enables the message box and plays a sound located in the main directory. This feature works for both items and land tiles, so no more messages and the Windows sound.

Version : 4.10.8 
- Tiledata: Tiledata: Search has been added to the control screen.
- Removed obsolete setting to collapse right panel in sounds tab.

Version : 4.10.7 
- Fixed search ranges. Updated land tiles control layout. #XG1 (The old search forms have been kept and not removed.)

Version : 4.10.6 
- The search functionality has been moved to the Item Tabs control screen. #XG1 (The old search forms have been kept and not removed.)

Version : 4.10.5 
- Added option to export hue names and ids to file - in case you need all hues list for some reason.
- Updated nuget packages to latest.

Version : 4.10.4 
- Updated map file unpacking process in UOP Packer plugin. When unpacking maps plugin will remove extra bytes from end of the mul files as some of the UOP maps contain additional 196 bytes. This extra bytes break some of the tooling
- Extracting maps no longer requires setting index file path.

Version : 4.10.3 
- Fixed art.mul/idx unpacking in UOP Packer plugin.


Version : 4.10.2 
- Fixed preview coloring in hue editor.

Version : 4.10.1 
- Fixed invalid value for tiledata flag `Unused8`. As tiledata csv export is positional this fix may affect files exported before this version. You'll have to compare files on your own. To fix older files just reorder columns later in file so this part `artused,noshadow,pixelbleed,playanimonce,unused8` should be `artused,unused8,noshadow,pixelbleed,playanimonce`.

Version : 4.10.0 
- Migrate to .net 7.

Version : 4.9.28 
- Fixed applying hues to items and animations.
- Updated map replace tiles form to better indicate progress and possible issues with XML file.

Version : 4.9.27 
- Added "Remove tile border" option that makes tile borders invisible (tabs: items, land tiles, textures, fonts).
- Added "Set background color same as tile background". When you select custom tile background color it will be applied to whole control too when this option is checked (currently limited to Items tab only).

Version : 4.9.26 
- Added "Export all sounds" option to Sounds tab.

Version : 4.9.25 
- Fixed multi editor plugin import range (maximum multi id was lower than in multi tab). 
- Minor cleanup.

Version : 4.9.24 
- Added UOX3 multi export. It's very basic Version : so expect some features to be missing.

Version : 4.9.23 
- Import multi dialog will now run as modal.
- Added import/export for multi as csv. To be used with punt's multi utilities.
- Added import multi to "Mass import" plug-in
- Fixed hue format saving to match original format better.

Version : 4.9.22 
- Fixed focusing of an item after search on Items, Land tiles and Textures tabs.

Version : 4.9.21 
- Added export to JPG and PNG on item detail form.
- Added export to JPG and PNG on animation edit form.

Version : 4.9.20 
- Removed source file locking when importing or replacing assets.

Version : 4.9.19 
- Fixed update checker exception handling.

Version : 4.9.18 
- Fixed crash when exporting animation frames where action has missing frame.

Version : 4.9.17 
- Updated client window enumeration logic to look for alternative clients first to fix Client.SendText() on OrionUO client.

Version : 4.9.16 
- Gump list will now select last inserted index when using "Insert at..." and "Insert starting from..." while "Show free slots" is enabled.

Version : 4.9.15 
- Fixed problems with "Replace starting from.." in Items, Land tiles and Textures tabs. It now replaces tiles as it should. Stops when index reaches max index value.
- Fixed "Show free slots" menu item not clearing its state after reloading files.

Version : 4.9.14 
- Fixed crash when browsing Textures with "show free slots" enabled.
- Fixed crash when browsing SkillGrp that uses nonexistent skill id.

Version : 4.9.13 
- Fix for multi (multi tab) region export/import. MultiRegion should now have same coordinates after import.
- Added "Replace starting from.." to Items tab. Allows replacing multiple items starting from selected index.
- Renamed "Insert Starting From.." to "Replace starting from.." as it reflects actual function better.
- Extended Ultima.Client with method to enumerate multiple ultima windows. Author: NaphalAXT. Source: https://github.com/NaphalAXT/ultimasVos/commit/5f2847739e8c690f8caa5e41c26075f64b930c0b
- Minor code fixes and cleanups.

Version : 4.9.12 
- Added search items on Dress tab.

Version : 4.9.11 
- Added "Show Free Slots" option to Land Tiles tab.
- Added "Show Free Slots" option to Textures tab.

Version : 4.9.10 
- Send item plug-in should now register itself only once and not twice.
- Send item to client plug-in should work with ClassicUO (thanks to Xuri)

Version : 4.9.9 
- Fixed Items tab - some larger statics items were not visible by default.
- Updated Sounds tab to use 0-based indexes by default. There is new option "Offset Sound Id by 1 (POL emulator)" for POL users. POL starts sound ids from 1.
- Added search by text to Hues control.

Version : 4.9.8 
- Updated animationlist.xml (you need to copy updated file from UOFiddler directory to your profile folder and override existing animationlist.xml)
- Fixed speech.mul Id value reading - no longer limited to 256.

Version : 4.9.7 
"Select in Gump" tab option added to context menus in Items and Tiledata tabs.
Minor updates in projects to fix building using `dotnet build` command.
Fixed wrong map size pre-selection when using .uop format.
"Insert starting from" option added to Gumps, Land Tiles and Textures tabs - allows to insert range of items in sequence (author Maybacco). 
Added "Set textures" option for Tiledata tab - sets texture id for all land tiles without it but having corresponding texture. Assumption is that land tile index value is equal to texture index. Option will only update tiles where TexID is 0. You can also enable option to set individual landtile TexID to land tile index via double click in TexID text box (default is off). (author Maybacco).
Default controls for items, land tiles and textures were removed. Alternative controls were renamed and are now main and only controls available.
Removed obsolete hash file usage.
"Replace" button on animation edit form has been hidden until its implementation is finished.
Fixed inability to map animation body id values over 1696. It works now up to body id 2048 which is UO client limit. #58 
New button "Average All" in RadarColor tab which checks all items and land tiles and sets all black non empty ones to average color. It's batch update command (author DiPaolaMarco).
Fixed invalid marking of AnimData as modified when user only browse contents of the tab.
Fixed 'Show Free Slots' on Items tab when using UOP file format.
Fixed 'Show Free Slots' on Gumps tab.
New options. You can now select selection color and focus color for tile view controls (items, land tiles, textures and fonts).
Fixed crash when providing invalid regular expression pattern string in items, land tiles, tiledata and cliloc tabs. 
Cliloc tab. "Goto text" uses case insensitive string comparison by default. You can switch to regular expressions if needed.
Renamed Hue tab to Hues. Hues control - added quick Search option. You can now jump to hue by it's index. Displayed hue index values are now 0-based same as stored in hues.mul.
Updated animation editor layout.
Added action names to animation list in animation editor.
Updated Serilog.Sinks.File package to latest versi

Version : 4.8 

    AsYlum added multiple selection in items tab using <ctrl> left-click used to with the test plug-in for exporting multiple items to an itemdesc.cfg file.
    AsYlum added graphic size & offset information to Item details box.
    AsYlum added export all item's graphic offsets to offset.cfg. This is useful for properly positioning art tiles displayed in a gump using the TilePic gump command.




## Requirements

Starting from version 4.10.0:

- Requires .NET Desktop Runtime 7.0.x (or SDK) installed to run the application.
- You can download .NET 8.0 at: <https://dotnet.microsoft.com/en-us/download/dotnet/8.0>
- You can download .NET 7.0 at: <https://dotnet.microsoft.com/en-us/download/dotnet/7.0>
- Minimum supported Windows version is Windows 10.

Older versions, 4.9.28 and lower use .Net Framework 4.8

## Building

You'll need Visual Studio 2022 v17.4, .NET 8.0 and .NET desktop development workload.

## Reporting bugs and issues

Please report any bugs or issues here: [issues](https://github.com/polserver/UOFiddler/issues).
Or here :[issues](https://github.com/jedi661/UoFiddler).
