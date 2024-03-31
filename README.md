# UOPixelFiddler

## About

UOFiddler based on Ultima SDK it's a tool to view and alter almost every UO 2D client file. Source code is released under the Beerware and Wineware license.
This is an extended version of UoFiddler that has more maps and more tools, as well as Map Edit, and will be expanded over time to support the Ultima standard 
version more than the original version. Also, some source codes from older and New projects that are no longer supported have been integrated.

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
