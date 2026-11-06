using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit
{
    /// <summary>Bilingual (English / German) tooltips and the help window of the animation editor.</summary>
    internal static class AnimEditHelp
    {
        /// <summary>Tooltip text with both languages: English first, German below.</summary>
        public static string Tip(string en, string de) => en + "\n\nDE: " + de;

        public const string English =
@"ANIMATION FRAME EDITOR – HELP

PURPOSE
Build and check animations from up to 10 frames. You can take frames from UO animations
(anim.mul … anim5.mul, UOP, VD files, another client folder), from art, from the clipboard
or from your own images, paint on them, align them and play them in a loop.

LAYOUT
• Toolbar (top): drawing tools, colors, brush size, layer, ""All checked"" mode, undo/redo, zoom.
• Frame strip: the 10 frames. Click = edit this frame. The small box = frame is ""checked""
  (used for playback, export and batch actions). Drop image files onto a frame to load them.
  Double click = load an image into the frame. Right click = frame menu.
• Editor tab: the big drawing area of the selected frame.
• Right panel: live preview / playback and the tabs Layers, Align, View, Colors, History.
• Status bar: pixel position, color under the mouse, frame info.

LAYERS (per frame)
• Original = the imported image (UO frame, art, file). It keeps its own size and position.
• Paint = your drawing layer above the original. It never destroys the original.
• Reference = another animation shown transparent behind or above all frames (shared).
• Background = checkerboard, a color or a ground tile (grass, sand, …).
Choose with ""Draw on"" whether tools change the Paint layer (default) or the Original.
Visibility and opacity can be set for every layer. ""Merge paint"" burns the paint into the original.

TOOLS (shortcut)
Pencil (B) · Eraser (E) · Line (L) · Rectangle (R) · Filled rectangle · Ellipse (O)
Filled ellipse · Fill (G) · Pipette (I) · Move (M) · Select (S) · Overlay (P)
• Left mouse = primary color, right mouse = secondary color, X = swap colors.
• Shift while drawing a line/shape = straight 45° line / square / circle.
• Alt + click = pipette with any tool. Middle mouse = pan the view.
• Ctrl + mouse wheel = zoom, wheel = scroll, Shift + wheel = scroll sideways.
• Alpha < 255 paints semi transparent. Fill tolerance makes the fill accept similar colors.

BLACK / WHITE TRANSPARENT (T)
The toolbar switch ""B/W transparent"" shows pure black, white and light gray (#D3D3D3) of the
original images as transparent. The images are NOT changed – switch it off and the colors are back.
It is also used for thumbnails, preview, export and for images loaded into a selection.

SELECTION – LOAD AN IMAGE INTO A MARKED AREA
Tool Select (S) or Ctrl + drag with any tool: mark a rectangle (Shift = square). Then in the Layers tab
or the Edit menu: ""Load image…"" or ""Paste"" (Ctrl+V) fits an image into the area as a movable overlay (keep aspect ratio
optional), ""Copy"" copies the area, ""Clear area"" (Delete) erases it, Esc removes the selection.
The image goes onto the active layer (""Draw on""); with ""All checked"" into all checked frames.

OVERLAYS – PUT NEW GRAPHICS ON EXISTING FRAMES (e.g. a cage around a bird)
An overlay is a floating image above the frames. It is NOT burned in, so you can move it, undo,
move again, copy it and place the copy elsewhere. Tab ""Overlay"" on the right:
• New: ""Load image / GIF…"", ""Paste"", ""Selection → overlay"" (copy a part of the frame), ""Into selection…"".
  Art browser and Anim browser have ""→ Overlay"" too (an animation becomes an animated overlay).
• Auto-crop: removes the transparent / black / white / light gray border on import, only the object stays.
• Tool Overlay (P): click = select, drag = move, arrow keys = 1 px (Shift 10), Delete = remove, Esc = deselect.
• Show on: all frames (placed once for the whole animation), checked frames or the current frame only.
• Scale % (pixel exact, no blur, no new colors), tint Original / Gray / Color, opacity, visible.
• Black and white stay inside the overlay and are shown transparent like in UO (switch ""B/W transparent"").
  Switch it off and ""Show frames + size labels"" on to see the full image size.
• ""Bake"" burns the overlay into the paint layer (black/white left out). Undo works for everything.
• Exports always treat black/white as transparent.

EDIT ALL FRAMES AT ONCE
Switch on ""All checked"" in the toolbar: every stroke, fill, move, nudge, mirror, clear …
is applied to the selected frame AND all checked frames – one undo step for all.

ALIGNMENT AND COMPARISON
• Arrow keys move the current layer by 1 px (Shift = 10 px). In ""Original"" mode they move the image,
  in ""Paint"" mode they shift your drawing.
• Anchor (green cross) = feet position. UO frames are placed with their center on it, just like the client.
• Guide lines: two height lines (e.g. ground and head) and one vertical line.
• Onion skin: shows previous (red) and next (blue) frames transparently.
• Reference: in the Anim/Art browser press ""As reference"" – that animation is shown transparent
  while you draw. It follows the frame number or stays on a fixed frame. Offset and opacity adjustable.
• Align tab: offset X/Y, center to anchor, align bottoms, scale, rotate, mirror, canvas size.

PLAYBACK
Play/Stop (Space), FPS, loop range From/To, ping-pong, only checked frames, mirrored preview,
preview zoom, ""Follow in editor"" (the editor jumps along with the playback).

SOURCES (Anim browser tab)
• Auto: like the client (body.def / bodyconv.def).
• anim.mul, anim2.mul … anim5.mul: one specific file of the current client.
• UOP: AnimationFrame*.uop of the current client.
• VD file: a .vd export (UoFiddler format, incl. UOP creature VD).
• Other client folder: anim*.idx/mul from any folder (second client, backup, test shard).
• Images / GIF: own PNG/BMP files or an animated GIF as a frame sequence.
Then: Body → Action → Direction (0-7, 5-7 are mirrored) → Hue → frames.
Buttons: to checked frames, from the current frame onward, into the current frame, as reference, export.
""Start"" and ""Step"" pick which source frames are used (e.g. every 2nd frame).

UNDO / REDO
Ctrl+Z / Ctrl+Y (or Ctrl+Shift+Z). Up to 60 steps. The History tab lists them.

FILES
• File > Save project / Open project: all frames with layers as PNG + project.json in a folder.
• Export: spritesheet, single PNG frames (cropped or canvas size), animated GIF, copy frame.

SHORTCUTS
1 … 0 select frame 1 … 10 · PageUp/PageDown previous/next frame · Space play/stop
Ctrl+C / Ctrl+V copy / paste frame image · Delete clear layer · +/- zoom · F1 help";

        public const string German =
@"ANIMATIONS-FRAME-EDITOR – HILFE

ZWECK
Animationen aus bis zu 10 Frames bauen und prüfen. Frames kommen aus UO-Animationen
(anim.mul … anim5.mul, UOP, VD-Dateien, anderer Client-Ordner), aus Art, aus der Zwischenablage
oder aus eigenen Bildern. Darauf zeichnen, ausrichten und in Schleife abspielen.

AUFBAU
• Werkzeugleiste (oben): Zeichenwerkzeuge, Farben, Pinselgröße, Ebene, Modus „All checked“, Undo/Redo, Zoom.
• Frame-Leiste: die 10 Frames. Klick = Frame bearbeiten. Kleines Kästchen = Frame ist „angehakt“
  (für Abspielen, Export und Mehrfach-Aktionen). Bilddateien auf einen Frame ziehen = laden.
  Doppelklick = Bild in den Frame laden. Rechtsklick = Frame-Menü.
• Reiter Editor: große Zeichenfläche des gewählten Frames.
• Rechte Seite: Live-Vorschau / Abspielen und die Reiter Layers, Align, View, Colors, History.
• Statusleiste: Pixelposition, Farbe unter der Maus, Frame-Infos.

EBENEN (pro Frame)
• Original = importiertes Bild (UO-Frame, Art, Datei). Behält eigene Größe und Position.
• Paint = deine Zeichenebene über dem Original. Das Original bleibt unverändert.
• Reference = eine andere Animation, durchscheinend hinter oder über allen Frames (gemeinsam).
• Hintergrund = Schachbrett, Farbe oder Boden-Kachel (Gras, Sand, …).
Mit „Draw on“ wählst du, ob die Werkzeuge die Paint-Ebene (Standard) oder das Original ändern.
Sichtbarkeit und Deckkraft gibt es für jede Ebene. „Merge paint“ brennt die Zeichnung ins Original.

WERKZEUGE (Taste)
Stift (B) · Radierer (E) · Linie (L) · Rechteck (R) · Gefülltes Rechteck · Ellipse (O)
Gefüllte Ellipse · Füllen (G) · Pipette (I) · Verschieben (M) · Auswahl (S) · Overlay (P)
• Linke Maustaste = Hauptfarbe, rechte = Zweitfarbe, X = Farben tauschen.
• Shift beim Zeichnen von Linie/Form = gerade 45°-Linie / Quadrat / Kreis.
• Alt + Klick = Pipette mit jedem Werkzeug. Mittlere Maustaste = Ansicht verschieben.
• Strg + Mausrad = Zoom, Mausrad = scrollen, Shift + Mausrad = seitlich scrollen.
• Alpha < 255 malt halbtransparent. Toleranz beim Füllen nimmt ähnliche Farben mit.

SCHWARZ / WEISS TRANSPARENT (T)
Der Schalter „B/W transparent“ in der Werkzeugleiste zeigt reines Schwarz, Weiß und Hellgrau (#D3D3D3)
der Original-Bilder transparent. Die Bilder werden NICHT verändert – ausschalten und die Farben sind wieder da.
Gilt auch für Vorschaubilder, Vorschau, Export und für Bilder, die in eine Auswahl geladen werden.

AUSWAHL – BILD IN EINEN MARKIERTEN BEREICH LADEN
Werkzeug Select (S) oder Strg + Ziehen mit jedem Werkzeug: Rechteck markieren (Shift = Quadrat). Dann im
Reiter Layers oder Menü Edit: „Load image…“ oder „Paste“ (Strg+V) setzt ein Bild als verschiebbares Overlay passend hinein
(Seitenverhältnis optional), „Copy“ kopiert den Bereich, „Clear area“ (Entf) löscht ihn, Esc hebt die Auswahl auf.
Das Bild kommt auf die aktive Ebene („Draw on“); mit „All checked“ in alle angehakten Frames.

OVERLAYS – NEUE GRAFIKEN AUF BESTEHENDE FRAMES SETZEN (z. B. Käfig um einen Vogel)
Ein Overlay ist ein schwebendes Bild über den Frames. Es wird NICHT eingebrannt: verschieben, rückgängig,
wieder verschieben, kopieren und woanders platzieren geht jederzeit. Reiter „Overlay“ rechts:
• Neu: „Load image / GIF…“, „Paste“, „Selection → overlay“ (Teil des Frames kopieren), „Into selection…“.
  Art- und Anim-Browser haben ebenfalls „→ Overlay“ (eine Animation wird zum animierten Overlay).
• Auto-crop: schneidet beim Laden den transparenten / schwarzen / weißen / hellgrauen Rand ab, nur das Objekt bleibt.
• Werkzeug Overlay (P): Klick = auswählen, ziehen = verschieben, Pfeiltasten = 1 px (Shift 10), Entf = löschen, Esc = abwählen.
• Show on: alle Frames (einmal für die ganze Animation gesetzt), angehakte Frames oder nur der aktuelle Frame.
• Scale % (pixelgenau, kein Verschwimmen, keine neuen Farben), Einfärben Original / Grau / Farbe, Deckkraft, sichtbar.
• Schwarz und Weiß bleiben im Overlay und werden wie in UO transparent gezeigt (Schalter „B/W transparent“).
  Schalter aus und „Show frames + size labels“ an zeigt die volle Bildgröße.
• „Bake“ brennt das Overlay in die Paint-Ebene ein (Schwarz/Weiß bleiben frei). Undo geht für alles.
• Beim Export sind Schwarz/Weiß immer transparent.

ALLE FRAMES GLEICHZEITIG
„All checked“ in der Werkzeugleiste einschalten: jeder Strich, Füllen, Verschieben, Pfeiltasten,
Spiegeln, Leeren … wirkt auf den gewählten Frame UND alle angehakten Frames – ein Undo-Schritt für alle.

AUSRICHTEN UND VERGLEICHEN
• Pfeiltasten verschieben die aktuelle Ebene um 1 px (Shift = 10 px). Im Modus „Original“ das Bild,
  im Modus „Paint“ die Zeichnung.
• Anker (grünes Kreuz) = Fußpunkt. UO-Frames werden wie im Client mit ihrem Center darauf gesetzt.
• Hilfslinien: zwei Höhenlinien (z. B. Boden und Kopf) und eine senkrechte Linie.
• Onion-Skin: zeigt vorherige (rot) und nächste (blau) Frames durchscheinend.
• Referenz: im Anim-/Art-Browser „As reference“ drücken – diese Animation scheint beim Zeichnen durch.
  Sie folgt der Frame-Nummer oder bleibt auf einem festen Frame. Versatz und Deckkraft einstellbar.
• Reiter Align: Versatz X/Y, auf Anker zentrieren, Unterkanten ausrichten, skalieren, drehen,
  spiegeln, Leinwandgröße.

ABSPIELEN
Play/Stop (Leertaste), FPS, Schleifenbereich Von/Bis, Ping-Pong, nur angehakte Frames,
gespiegelte Vorschau, Vorschau-Zoom, „Follow in editor“ (Editor springt beim Abspielen mit).

QUELLEN (Reiter Anim browser)
• Auto: wie der Client (body.def / bodyconv.def).
• anim.mul, anim2.mul … anim5.mul: genau eine Datei des aktuellen Clients.
• UOP: AnimationFrame*.uop des aktuellen Clients.
• VD file: eine .vd-Exportdatei (UoFiddler-Format, auch UOP-Creature-VD).
• Other client folder: anim*.idx/mul aus einem beliebigen Ordner (zweiter Client, Backup, Testshard).
• Images / GIF: eigene PNG/BMP-Dateien oder ein animiertes GIF als Frame-Folge.
Danach: Body → Action → Richtung (0-7, 5-7 gespiegelt) → Hue → Frames.
Buttons: in angehakte Frames, ab aktuellem Frame, in aktuellen Frame, als Referenz, Export.
„Start“ und „Step“ bestimmen, welche Quell-Frames genommen werden (z. B. jeder 2.).

RÜCKGÄNGIG / WIEDERHOLEN
Strg+Z / Strg+Y (oder Strg+Shift+Z). Bis zu 60 Schritte. Der Reiter History listet sie auf.

DATEIEN
• File > Save project / Open project: alle Frames mit Ebenen als PNG + project.json in einem Ordner.
• Export: Spritesheet, einzelne PNG-Frames (zugeschnitten oder Leinwandgröße), animiertes GIF, Frame kopieren.

TASTENKÜRZEL
1 … 0 Frame 1 … 10 wählen · Bild auf/ab vorheriger/nächster Frame · Leertaste Abspielen/Stop
Strg+C / Strg+V Frame-Bild kopieren / einfügen · Entf Ebene leeren · +/- Zoom · F1 Hilfe";

        public static void Show(IWin32Window owner, bool german)
        {
            var form = new Form
            {
                Text = "Help / Hilfe – Animation Frame Editor",
                Size = new Size(820, 720),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                ShowIcon = false
            };
            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(MakePage("English", English));
            tabs.TabPages.Add(MakePage("Deutsch", German));
            tabs.SelectedIndex = german ? 1 : 0;
            form.Controls.Add(tabs);
            form.Show(owner);
        }

        private static TabPage MakePage(string title, string text)
        {
            var page = new TabPage(title);
            page.Controls.Add(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.75f),
                BackColor = SystemColors.Window,
                Text = text.Replace("\r\n", "\n").Replace("\n", "\r\n")
            });
            return page;
        }
    }
}
