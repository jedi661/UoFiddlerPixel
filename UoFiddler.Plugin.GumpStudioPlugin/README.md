# Gump Studio (UoFiddler.Plugin.DesignStudio)

Visual designer for **ServUO / RunUO / ModernUO gumps** inside UoFiddlerPixel.
You see the gump the way the client draws it, with the graphics of your client files. The finished C# script is written below it while you work.

*Deutsch: siehe unten.*

## Features

**Designer**
- The work area uses real client graphics: gumps, items, hues and the unicode font.
- The script is written live below the work area:
  - a complete ServUO class (constants, command, `OnResponse`)
  - only the `Add…` lines
  - a Sphere `[DIALOG]`
- All elements are supported: background, image, tiled image, alpha region, label, HTML / cliloc, button, item button, check, radio, text entry, item and sprite.
- There are also ready parts such as OK / Cancel, page buttons, a title bar and a list row.
- Editing tools:
  - drag & drop from the gump / item browser
  - snap grid, resize handles and editing text in place
  - align / distribute, pages, undo / redo, copy / paste (also as ServUO lines)
- Test mode (F5): buttons, pages, check boxes, radios and text entries work like in the game. The status line shows what `OnResponse` would receive.
- Problem list: duplicate IDs, page buttons to empty pages, missing graphics, umlauts in labels and more.

**Script gallery and write back**
- **Script gallery**: scan a ServUO / RunUO / ModernUO `Scripts` folder. Every gump class gets a thumbnail, and you can search by class, command or file. Open a gump or add it to the template library.
- **Write back**: change an imported gump, then write the changes into the original script. Only the changed numbers and texts of the `Add…` calls are replaced, so logic, comments and formatting stay.
  - Values that come from code are adjusted, e.g. `x + 20` becomes `x + 25`.
  - A loop that you move as a whole changes its numbers in one place.
  - Deleted elements are commented out, new elements are inserted as new lines.
  - You see an old / new preview of every changed line first, and a `.bak` backup is made.
- **Compare**: show the current state, the imported original or a picture (e.g. a screenshot) half transparent over the work area.

**More design tools**
- **List / table generator**: set columns and rows (paste from Excel), rows per page and a button per row. Row 1 is the template and all other rows follow it. The script gets a `for` loop.
- **Themes**: change frames, button set, check boxes and text colors of the whole gump at once. Six built-in themes are included; your own are saved as JSON.
- **Own graphics**: put a PNG into the gump file, or cut a picture into the 9 parts of an `AddBackground` frame.
- **Smart guides** while dragging, with distances to the neighbours. **Groups** (Ctrl+G).
- **Button actions**: close, reopen, open another gump, teleport, give an item, run a command, send a message or custom code. The `OnResponse` code is written for you.
- **Second language**: give a text a `TextDe` version. The script picks it with `from.Language == "DEU"`.
- **Test log** (optionally written into the script) and **client limit checks**: text that is cut off, entries without a length limit, gumps over 800 × 600, newer-client features.
- **ModernUO output**: `DynamicGump` with `BuildLayout(ref DynamicGumpBuilder builder)`. ModernUO scripts can also be imported.

**Template library** (`Templates`, Ctrl+T)
- Save the whole gump as a template, or only the selected elements as a reusable **building block**.
- Categories are folders. A `/` in the name makes a sub category, e.g. `Shops/Vendors`.
- Templates are saved as `GumpTemplates/<Category>/<Name>.gump.json` next to UoFiddler.exe.
- The library shows thumbnails and a large preview with notes. It also offers search, rename / move (also by dragging onto a category), copy, delete and export.
- Add files from disk or by dragging them from Explorer: `.gump.json` projects or ServUO `.cs` scripts.
- **New from template**, or **Insert into current gump**. Inserting moves button, switch and entry IDs behind the ones already used.
- Your own templates also appear in the `New ▾` menu, grouped by category.

**Script import** (`Import ServUO…`)
- A preview window shows the gump of each class in the file (page by page) and the hints, before you take it over.
- The constructor is actually run by a small interpreter. It understands:
  - variables, `x++` / `y += 25` and arrays
  - `for` / `foreach` / `while` loops, `if` / `else` and `switch`
  - helper methods with return values (`Color()`, `Center()` …)
  - `String.Format` and `$"…"` strings
  - constants, enums and chained constructors (`: this(…)`)
- Constants of other classes (e.g. `PropsConfig`) are read from the scripts around the imported file, up to the `Scripts` folder.
- Values that only exist in the game turn into placeholders:
  - Texts show the expression, e.g. `{from.Name}`.
  - Numbers become `0`.
  - Lists of the game get 3 sample rows.
  - Width / height parameters become 400 × 300.
- Tested with stock ServUO gumps: AdminGump, WhoGump, GoGump, PropsGump, ClientGump, WarningGump, AddGump, ResurrectGump and more.

## Usage

1. Open UoFiddler and load your client files.
2. Open the *Design Studio* tab or the menu entry **Gump Studio**.
3. Start a gump in one of three ways: pick a template from `New ▾`, open a `.gump.json` project, or import an existing `.cs` script.
4. Design the gump, press F5 to test it, then copy the script or save it as `.cs`, e.g. into `ServUO/Scripts/Custom`.

Press F1 in the studio for the full help (English / German).

## Files

| File | Purpose |
|---|---|
| `GumpStudio/GumpModel.cs` | Data model (`GumpDesign`, `GumpElement`), JSON |
| `GumpStudio/GumpRenderer.cs` | Draws gumps like the client |
| `GumpStudio/GumpCanvas.cs` | Work area and test mode |
| `GumpStudio/GumpBrowser.cs` | Gump / item browser, ID and hue pickers |
| `GumpStudio/GumpCode.cs` | Script generator (ServUO, Sphere), validation |
| `GumpStudio/GumpImporter.cs` | Script import (interpreter) |
| `GumpStudio/GumpTemplates.cs` | Built-in templates |
| `GumpStudio/GumpTemplateLibrary.cs` | Own templates and categories |
| `GumpStudio/GumpScriptWriter.cs` | Write back into the original script |
| `GumpStudio/GumpLists.cs` | Lists / tables (template row → rows, for loop) |
| `GumpStudio/GumpThemes.cs` | Themes |
| `GumpStudio/GumpGalleryForm.cs`, `GumpWriteBackForm.cs`, `GumpListForm.cs`, `GumpThemeForm.cs`, `GumpArtForm.cs` | Dialogs (designer based) |
| `GumpStudio/GumpStudioControl.cs` | Main control |
| `GumpStudio/GumpImportForm.cs`, `GumpTemplateForm.cs`, `GumpTemplateSaveForm.cs` | Dialogs (designer based) |

---

## Deutsch (kurz)

Mit dem **Gump Studio** entwirfst du ServUO-Gumps mit den echten Grafiken deines Clients. Das fertige Skript entsteht dabei live unter der Arbeitsfläche.

- **Vorlagen:** Unter `Templates` (Strg+T) speicherst du ganze Gumps oder markierte Bausteine in Kategorien. Die Dateien liegen in `GumpTemplates/<Kategorie>/<Name>.gump.json`. Aus einer Vorlage startest du ein neues Gump oder fügst sie ins offene Gump ein.
- **Import:** Vorhandene ServUO-Skripte werden eingelesen und als Gump angezeigt, mit Vorschau vor dem Übernehmen. Variablen, Schleifen und Hilfsmethoden werden ausgewertet. Spielwerte erscheinen als Platzhalter wie `{from.Name}`.
- **Testen:** Mit F5 funktioniert das Gump wie im Spiel.
- **Neu:**
  - Die Skript-Galerie zeigt alle Gumps eines Scripts-Ordners als Bilder.
  - „Write back“ schreibt deine Änderungen ins Original-Skript zurück, nur die geänderten Zahlen und Texte.
  - Weitere Werkzeuge: Listen- und Tabellen-Generator, Themen, eigene Grafiken und Rahmen, Hilfslinien, Gruppen.
  - Für Buttons gibt es Aktionen, Texte können eine zweite Sprache bekommen (DEU).
  - ModernUO wird beim Schreiben und Einlesen unterstützt.
- **Hilfe:** F1 öffnet die vollständige Hilfe auf Deutsch und Englisch.
