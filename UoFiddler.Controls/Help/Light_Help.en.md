# Light – help

The light tab shows and edits the lights of `light.mul` / `lightidx.mul`. A light is a grey scale image: every pixel has a value from 0 (no light) to 31 (full brightness). The client puts it over the world when an item with the flag *LightSource* shines.

All changes are made in memory first. They are written with **Save light.mul** (output folder).

## List and search

- The list shows `id (hex) – name`.
- **Names**: F2 or right click → *Name…*. The names are stored in `%AppData%\UoFiddler\LightNames.json`, not in the mul.
- **Search** at the top: id (`12`), hex (`0x0C`), range (`10-20`) or part of the name.
- **▦ Overview** shows all lights as pictures. A double click selects one.
- **Del** removes the light, **F2** renames it.

## Preview

- Normally you see the light alone (black = no light, white = full brightness), with zoom.
- **In-game preview** shows the light over ground tiles with an item in the middle:
  - **Ground**: land tile of the ground. **◀ ▶** browse, **▦ Overview…** chooses from the overview.
  - **Light source**: the item in the middle. ◀ ▶ browse, optionally only through items with the flag *LightSource*.
  - **Darkness %**: how dark the scene is without light.
  - **Light offset X/Y**: where the light is relative to the item.
  - **Light colour…**: test coloured light. ClassicUO can colour lights with a hue in `lights.txt`.
  - **Flicker**: the brightness changes like a flame.
- Right click on the preview: copy the preview or save it as PNG.

## More than 100 lights

The original files have 100 entries. The number is read from `lightidx.mul`.

- **Entries: [ ] Set** extends the files, e.g. to 255, or shortens them.
- Inserting a light behind the end extends the files automatically.
- ClassicUO reads all entries of `lightidx.mul`. Please test yourself whether other clients use more than 100.
- Which light an item uses is set in the tiledata.

## Right click in the list

| Entry | Function |
|---|---|
| Export image | as PNG, BMP, TIFF, JPG; or all lights as PNG into a folder |
| Name… / Copy ID | set the name, copy the id |
| Remove | remove the light |
| Replace from file… | load an image (brightness of the image = light value, black = no light) |
| Insert at ID | type the id, Enter, choose an image |
| Copy to ID / Move to ID | copy or move the light (with its name) to another id |
| Copy image / Import from clipboard | through the clipboard |
| Edit in light designer… | opens the light designer with this light |

## More buttons

- **✔ Check…** finds duplicate lights (same image), empty lights, very dark ones (brightest value below 4), very big ones (over 512 pixels) and lights with "negative" values above 31. It also shows how many entries are free.
- **Import folder…** reads all images of a folder. The id is part of the file name: `Light 12 Candle.png` → light 12 named "Candle". `12.png` and `0x0C.bmp` work too. This is the counterpart of "Export → all lights".

## Light designer

Open it with **✚ New light…** (new light, first free id) or **✎ Edit…** (selected light).

### Designing a new light

- **Templates** (double click): candle, lamp, big light, torch, window (light falls left/right), spot, beam, ring.
- **Shape**: round, oval, cone (spot), window (gets wider), beam (same width), ring.
- **Image size** and **position of the light source** in the image.
- **Radius**, **direction °** (0 = right, 90 = down, 180 = left, 270 = up), **opening angle**, **length**, **opening width**.
- **Brightness** (1–31), **falloff** (100 % = even, more = darker faster), **soft edge**, **grain** (like the originals) and **grain pattern**.
- **Mirror ↔ ↕** and **rotate** 0/90/180/270°.

### With the mouse

- **Drag with the left button** in the preview to move the light source (with an edited image: the whole image).
- **Drag with the right button** to move the guide image.

### Guide image

An item (e.g. a window), a land tile or an image file is shown over the light, so you can fit the light to it. Position, size and opacity can be set. The guide image is **not** saved.

### Testing

- View **Light only** or **In game** (ground, item, darkness, light offset)
- **Colour…** for coloured light, **Flicker** for flickering
- Hold **◑ Before** to see the state when the designer was opened

### Editing the finished image

Rotate freely (°; 90° steps rotate without loss), size %, brightness %, move X/Y. Afterwards the shape fields are switched off. **Back to the design** switches back.

### Undo

**↶ / ↷** or Ctrl+Z / Ctrl+Y.

### Saving

- **Into light.mul**: at the entered id. Next to it you see whether the id is free or will be replaced. Afterwards click **Save light.mul** in the light tab.
- **Export PNG…** saves the image.
- **Save as .uolight…** saves design and image in one file. Load it again with **Import…** to change it later.
- **Import…** loads `.uolight`, PNG, BMP or TIFF.
- **Clipboard list**:
  - **Keep** puts the current light there. The list stays while UoFiddler runs.
  - **Load** or a double click loads it again.
  - **All into folder…** saves all of them as `.uolight`.
  - **Load folder…** loads a folder (default `%AppData%\UoFiddler\LightLibrary`).
