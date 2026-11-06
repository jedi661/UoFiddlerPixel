using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// UI construction of the animation editor. The layout is built in code (not in the designer),
    /// because most parts are generated (10 frames, tool buttons, color swatches).
    /// </summary>
    public partial class AnimationEditFormButton
    {
        #region Controls

        private ToolTip tips;
        private MenuStrip menu;
        private ToolStrip tools;
        private StatusStrip status;
        private FrameStrip strip;
        private SplitContainer split;
        private TabControl mainTabs;
        private TabPage tabEditor, tabAnim, tabArt, tabClip;
        private Panel canvasHost;
        private FrameCanvas canvas;

        // toolbar
        private readonly Dictionary<DrawTool, ToolStripButton> toolButtons = new Dictionary<DrawTool, ToolStripButton>();
        private ToolStripButton btKeyTransparent;
        private ToolStripButton btPrimary, btSecondary, btSwapColors, btRound, btAllChecked, btUndo, btRedo, btZoomIn, btZoomOut, btHelp;
        private ToolStripComboBox cbLayer;
        private ToolStripLabel lbZoom;
        private NumericUpDown numBrush, numAlpha, numTolerance;

        // preview / playback
        private PreviewBox preview;
        private Button btPlay, btPrevFrame, btNextFrame;
        private NumericUpDown numFps, numFrom, numTo, numPreviewZoom;
        private CheckBox chkPingPong, chkOnlyChecked, chkMirrorPreview, chkFollow, chkLoop, chkPreviewRef;

        // side tabs
        private TabControl sideTabs;

        // layers
        private CheckBox chkOrigVisible, chkPaintVisible, chkRefVisible, chkRefOnTop, chkKeepPaint;
        private TrackBar trkOrigOpacity, trkPaintOpacity, trkRefOpacity;
        private NumericUpDown numRefFrame, numRefOffX, numRefOffY;
        private Label lbRefName, lbSelection;

        // overlays
        private ListBox lbOverlays;
        private NumericUpDown numOvX, numOvY, numOvScale;
        private ComboBox cbOvTint, cbOvScope;
        private Button btOvColor;
        private CheckBox chkOvVisible, chkOvFrames, chkAutoCrop;
        private TrackBar trkOvOpacity;
        private CheckBox chkSelKeepAspect;

        // align
        private NumericUpDown numOffX, numOffY, numScale, numCanvasW, numCanvasH, numAnchorX, numAnchorY;
        private Label lbFrameInfo;

        // view
        private CheckBox chkGrid, chkAnchor, chkGuides, chkOnion, chkOnionTint, chkOnionWrap, chkBounds, chkZoomPerFrame;
        private NumericUpDown numGuideY1, numGuideY2, numGuideX, numOnionPrev, numOnionNext;
        private TrackBar trkOnionOpacity;
        private Button btGuideColor;
        private ComboBox cbBackground;

        // colors
        private TextBox tbHex;
        private Panel pnlPrimary, pnlSecondary;
        private FlowLayoutPanel flowRecent, flowFramePalette, flowBasic;

        // history
        private ListBox lbHistory;

        // status
        private ToolStripStatusLabel stPos, stColorSwatch, stColor, stFrame, stZoom, stMsg;

        #endregion

        #region Small factory helpers

        private void Tip(Control c, string en, string de) => tips.SetToolTip(c, AnimEditHelp.Tip(en, de));

        private static void Tip(ToolStripItem item, string en, string de) => item.ToolTipText = AnimEditHelp.Tip(en, de);

        private static Label L(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 0) };

        private static NumericUpDown Num(int min, int max, int value, int width = 56)
        {
            return new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, value)), Width = width };
        }

        private static CheckBox Chk(string text, bool value = false)
        {
            return new CheckBox { Text = text, Checked = value, AutoSize = true, Margin = new Padding(3, 5, 3, 0) };
        }

        private static Button Btn(string text, EventHandler click, int width = 0)
        {
            var b = new Button { Text = text, AutoSize = width == 0, UseVisualStyleBackColor = true };
            if (width > 0) b.Width = width;
            b.Click += click;
            return b;
        }

        private static TrackBar Trk(int value)
        {
            return new TrackBar { Minimum = 0, Maximum = 100, Value = value, TickFrequency = 10, Width = 150, Height = 30, AutoSize = false };
        }

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var f = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0), FlowDirection = FlowDirection.LeftToRight };
            f.Controls.AddRange(controls);
            return f;
        }

        private static FlowLayoutPanel Wrap(int width, params Control[] controls)
        {
            var f = new FlowLayoutPanel { AutoSize = true, WrapContents = true, Width = width, MaximumSize = new Size(width, 0), Margin = new Padding(0) };
            f.Controls.AddRange(controls);
            return f;
        }

        private static GroupBox Group(string title, params Control[] rows)
        {
            var inner = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            inner.Controls.AddRange(rows);
            var g = new GroupBox { Text = title, AutoSize = true, Padding = new Padding(4), Margin = new Padding(3, 3, 3, 6) };
            g.Controls.Add(inner);
            return g;
        }

        private static TabPage SidePage(string title, params Control[] groups)
        {
            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            flow.Controls.AddRange(groups);
            var page = new TabPage(title);
            page.Controls.Add(flow);
            return page;
        }

        private static ToolStripControlHost Host(Control c) => new ToolStripControlHost(c) { Margin = new Padding(1, 1, 3, 1) };

        private static Bitmap Swatch(Color c, int size = 16)
        {
            var b = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(b))
            {
                ImageUtil.DrawChecker(g, new Rectangle(0, 0, size, size), 4);
                using (var br = new SolidBrush(c)) g.FillRectangle(br, 0, 0, size, size);
                g.DrawRectangle(Pens.Black, 0, 0, size - 1, size - 1);
            }
            return b;
        }

        #endregion

        private void BuildLayout()
        {
            SuspendLayout();
            tips = new ToolTip(components) { AutoPopDelay = 30000, InitialDelay = 400, ReshowDelay = 100 };

            BuildStatus();
            BuildMenu();
            BuildToolbar();

            strip = new FrameStrip { Dock = DockStyle.Top };
            Tip(strip, "The 10 frames. Click = edit frame, small box = frame is checked (playback, export, 'All checked' mode). Drop image files here. Double click = load image. Right click = frame menu.",
                "Die 10 Frames. Klick = Frame bearbeiten, kleines Kästchen = Frame angehakt (Abspielen, Export, Modus 'All checked'). Bilddateien hier ablegen. Doppelklick = Bild laden. Rechtsklick = Frame-Menü.");

            split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2, SplitterWidth = 5 };

            mainTabs = new TabControl { Dock = DockStyle.Fill };
            tabEditor = new TabPage("Editor");
            tabAnim = new TabPage("Anim browser");
            tabArt = new TabPage("Art browser");
            tabClip = new TabPage("Clipboard animation");
            mainTabs.TabPages.AddRange(new[] { tabEditor, tabAnim, tabArt, tabClip });

            canvasHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(60, 60, 60) };
            canvas = new FrameCanvas();
            canvasHost.Controls.Add(canvas);
            canvasHost.Resize += (s, e) => CenterCanvas();
            tabEditor.Controls.Add(canvasHost);
            Tip(canvas, "Drawing area of the selected frame. Left = primary color, right = secondary color, Alt+click = pipette, middle mouse = pan, Ctrl+wheel = zoom, arrow keys = move layer.",
                "Zeichenfläche des gewählten Frames. Links = Hauptfarbe, rechts = Zweitfarbe, Alt+Klick = Pipette, mittlere Maustaste = verschieben, Strg+Mausrad = Zoom, Pfeiltasten = Ebene verschieben.");

            split.Panel1.Controls.Add(mainTabs);
            split.Panel2.Controls.Add(BuildSidePanel());

            BuildAnimBrowserTab();
            BuildArtBrowserTab();
            BuildClipboardTab();

            // order matters for docking: fill first, then the top/bottom bars
            Controls.Add(split);
            Controls.Add(strip);
            Controls.Add(tools);
            Controls.Add(menu);
            Controls.Add(status);
            MainMenuStrip = menu;

            ResumeLayout(true);
        }

        private void CenterCanvas()
        {
            if (canvas == null || canvasHost == null) return;
            Point scroll = canvasHost.AutoScrollPosition;
            int x = Math.Max(0, (canvasHost.ClientSize.Width - canvas.Width) / 2);
            int y = Math.Max(0, (canvasHost.ClientSize.Height - canvas.Height) / 2);
            canvas.Location = new Point(x + scroll.X, y + scroll.Y);
        }

        #region Menu

        /// <summary>
        /// Menu item. Shortcuts that would collide with text boxes (Ctrl+Z, Ctrl+Shift+Arrow …) are only
        /// displayed here and handled by HandleEditorKey, so typing in a text field keeps working.
        /// </summary>
        private ToolStripMenuItem MI(string text, EventHandler click, Keys keys = Keys.None, string en = null, string de = null, string display = null)
        {
            var item = new ToolStripMenuItem(text, null, click);
            if (keys != Keys.None) item.ShortcutKeys = keys;
            if (display != null) item.ShortcutKeyDisplayString = display;
            if (en != null) Tip(item, en, de);
            return item;
        }

        private void BuildMenu()
        {
            menu = new MenuStrip { ShowItemToolTips = true };

            var file = new ToolStripMenuItem("&File");
            file.DropDownItems.AddRange(new ToolStripItem[]
            {
                MI("Load images (from current frame on)…", (s, e) => LoadImagesSequential(), Keys.Control | Keys.O,
                    "Load one or more images (or an animated GIF). They fill the frames starting at the selected frame.",
                    "Ein oder mehrere Bilder (oder ein animiertes GIF) laden. Sie füllen die Frames ab dem gewählten Frame."),
                MI("Load one image into all checked frames…", (s, e) => LoadImageIntoChecked(),
                    keys: Keys.None, en: "Load a single image into every checked frame (good as a base to paint variations).",
                    de: "Ein einzelnes Bild in jeden angehakten Frame laden (gut als Basis für Varianten)."),
                MI("Load VD file into browser…", (s, e) => { mainTabs.SelectedTab = tabAnim; SelectSourceIndex(SourceVd); },
                    en: "Opens a .vd animation file in the Anim browser.", de: "Öffnet eine .vd-Animationsdatei im Anim-Browser."),
                new ToolStripSeparator(),
                MI("Open project…", (s, e) => OpenProject(), Keys.Control | Keys.Shift | Keys.O,
                    "Opens a project folder saved with 'Save project' (all layers, offsets, settings).",
                    "Öffnet einen mit 'Save project' gespeicherten Projektordner (alle Ebenen, Versätze, Einstellungen)."),
                MI("Save project…", (s, e) => SaveProject(), Keys.Control | Keys.S,
                    "Saves all frames with their layers as PNG + project.json into a folder.",
                    "Speichert alle Frames mit Ebenen als PNG + project.json in einen Ordner."),
                new ToolStripSeparator(),
                MI("Export spritesheet (checked)…", (s, e) => ExportSpritesheet(),
                    en: "All checked frames side by side in one PNG/BMP, cropped to a common box so alignment is kept.",
                    de: "Alle angehakten Frames nebeneinander in einer PNG/BMP, auf gemeinsamen Rahmen zugeschnitten (Ausrichtung bleibt)."),
                MI("Export frames as PNG (checked)…", (s, e) => ExportFramesSeparate(),
                    en: "One PNG per checked frame, cropped to a common box.",
                    de: "Eine PNG pro angehaktem Frame, auf gemeinsamen Rahmen zugeschnitten."),
                MI("Export animated GIF (checked)…", (s, e) => ExportGif(),
                    en: "Animated GIF of the checked frames with the current FPS and background.",
                    de: "Animiertes GIF der angehakten Frames mit aktueller FPS und Hintergrund."),
                new ToolStripSeparator(),
                MI("Close", (s, e) => Close())
            });

            var edit = new ToolStripMenuItem("&Edit");
            edit.DropDownItems.AddRange(new ToolStripItem[]
            {
                MI("Undo", (s, e) => DoUndo(), display: "Ctrl+Z"),
                MI("Redo", (s, e) => DoRedo(), display: "Ctrl+Y"),
                new ToolStripSeparator(),
                MI("Copy frame image", (s, e) => CopyFrameToClipboard(), Keys.None,
                    "Copies the visible frame (cropped) to the clipboard. Shortcut Ctrl+C in the editor.",
                    "Kopiert den sichtbaren Frame (zugeschnitten) in die Zwischenablage. Kürzel Strg+C im Editor.", "Ctrl+C"),
                MI("Paste as original", (s, e) => PasteFromClipboard(false), Keys.None,
                    "Clipboard image becomes the original layer of the current frame (Ctrl+V).",
                    "Bild aus der Zwischenablage wird Original-Ebene des aktuellen Frames (Strg+V).", "Ctrl+V"),
                MI("Paste into paint layer", (s, e) => PasteFromClipboard(true), Keys.None,
                    "Draws the clipboard image onto the paint layer (bottom center on the anchor).",
                    "Zeichnet das Bild aus der Zwischenablage auf die Paint-Ebene (unten mittig am Anker)."),
                new ToolStripSeparator(),
                MI("Load image into selection…", (s, e) => LoadImageIntoSelection(), Keys.None,
                    "Loads an image file and fits it into the marked rectangle (active layer, 'All checked' works too).",
                    "Lädt eine Bilddatei und setzt sie passend in das markierte Rechteck (aktive Ebene, 'All checked' geht auch)."),
                MI("Paste into selection", (s, e) => PasteIntoSelection(), Keys.None,
                    "Fits the clipboard image into the marked rectangle (Ctrl+V while something is marked).",
                    "Setzt das Bild aus der Zwischenablage passend in das markierte Rechteck (Strg+V, wenn etwas markiert ist)."),
                MI("Copy selection", (s, e) => CopySelection(), Keys.None,
                    "Copies the visible content of the marked rectangle to the clipboard.",
                    "Kopiert den sichtbaren Inhalt des markierten Rechtecks in die Zwischenablage."),
                MI("Clear selection area", (s, e) => ClearSelectionArea(), Keys.None,
                    "Makes the marked area of the active layer transparent (Delete while something is marked).",
                    "Macht den markierten Bereich der aktiven Ebene transparent (Entf, wenn etwas markiert ist)."),
                MI("Deselect", (s, e) => canvas.Selection = Rectangle.Empty, Keys.None, null, null, "Esc"),
                new ToolStripSeparator(),
                MI("Clear layer", (s, e) => ClearTargetLayer(), Keys.None,
                    "Clears the active layer ('Draw on') of the affected frames (Delete key).",
                    "Leert die aktive Ebene ('Draw on') der betroffenen Frames (Entf-Taste).", "Del"),
                MI("Clear frame (all layers)", (s, e) => ClearFrames()),
                MI("Clear ALL frames", (s, e) => ClearAllFrames()),
                new ToolStripSeparator(),
                MI("Mirror", (s, e) => ApplyToAffected("Mirror", i => doc.MirrorFrame(i)), Keys.Control | Keys.M,
                    "Mirrors the frame horizontally around the anchor.", "Spiegelt den Frame waagerecht um den Anker."),
                MI("Rotate 90° (original)", (s, e) => ApplyToAffected("Rotate", i => doc.RotateOriginal90(i))),
                MI("Merge paint into original", (s, e) => ApplyToAffected("Merge", i => doc.MergePaintDown(i))),
                new ToolStripSeparator(),
                MI("Black/White/Gray → transparent", (s, e) => MakeKeysTransparent(), Keys.None,
                    "Makes pure black, white and light gray (#D3D3D3) transparent on the active layer.",
                    "Macht reines Schwarz, Weiß und Hellgrau (#D3D3D3) auf der aktiven Ebene transparent."),
                MI("Secondary color → transparent", (s, e) => SecondaryToTransparent(), Keys.None,
                    "Makes the secondary color transparent (uses the fill tolerance).",
                    "Macht die Zweitfarbe transparent (nutzt die Füll-Toleranz)."),
                MI("Replace secondary with primary color", (s, e) => ReplaceSecondaryWithPrimary(), Keys.None,
                    "Replaces every pixel with the secondary color by the primary color (uses the fill tolerance).",
                    "Ersetzt alle Pixel in Zweitfarbe durch die Hauptfarbe (nutzt die Füll-Toleranz)."),
                MI("Outline (primary color)", (s, e) => DrawOutline(), Keys.None,
                    "Draws a 1 pixel outline in the primary color around everything on the active layer.",
                    "Zeichnet eine 1-Pixel-Kontur in Hauptfarbe um alles auf der aktiven Ebene.")
            });

            var frames = new ToolStripMenuItem("F&rames");
            frames.DropDownItems.AddRange(new ToolStripItem[]
            {
                MI("Check all", (s, e) => SetAllChecked(true)),
                MI("Uncheck all", (s, e) => SetAllChecked(false)),
                MI("Check only frames with content", (s, e) => CheckNonEmpty()),
                new ToolStripSeparator(),
                MI("Duplicate frame to next", (s, e) => DuplicateToNext(), Keys.Control | Keys.D,
                    "Copies the current frame (all layers) to the next frame and selects it – quick way to draw the next step.",
                    "Kopiert den aktuellen Frame (alle Ebenen) in den nächsten und wählt ihn – schnell den nächsten Schritt zeichnen."),
                MI("Copy frame to all checked", (s, e) => CopyFrameToChecked()),
                MI("Move frame left", (s, e) => MoveFrame(-1), display: "Ctrl+Shift+Left"),
                MI("Move frame right", (s, e) => MoveFrame(1), display: "Ctrl+Shift+Right"),
                MI("Reverse order of checked frames", (s, e) => ReverseChecked()),
                new ToolStripSeparator(),
                MI("Align bottoms of checked frames to anchor", (s, e) => AlignCheckedBottoms()),
                MI("Fit canvas to frames", (s, e) => FitCanvas())
            });

            var view = new ToolStripMenuItem("&View");
            view.DropDownItems.AddRange(new ToolStripItem[]
            {
                MI("Zoom in", (s, e) => SetZoom(canvas.Zoom + 1), display: "+"),
                MI("Zoom out", (s, e) => SetZoom(canvas.Zoom - 1), display: "-"),
                MI("Zoom 1:1", (s, e) => SetZoom(1)),
                MI("Zoom to fit", (s, e) => ZoomToFit()),
                new ToolStripSeparator(),
                MI("Play / Stop", (s, e) => TogglePlay(), display: "Space")
            });

            var help = new ToolStripMenuItem("&Help");
            help.DropDownItems.AddRange(new ToolStripItem[]
            {
                MI("Help (English)", (s, e) => AnimEditHelp.Show(this, false), Keys.F1),
                MI("Hilfe (Deutsch)", (s, e) => AnimEditHelp.Show(this, true))
            });

            menu.Items.AddRange(new ToolStripItem[] { file, edit, frames, view, help });
        }

        #endregion

        #region Toolbar

        private void BuildToolbar()
        {
            tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, ImageScalingSize = new Size(16, 16), ShowItemToolTips = true };

            void AddTool(DrawTool t, string text, string en, string de)
            {
                var b = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text, Tag = t };
                b.Click += (s, e) => SetTool(t);
                Tip(b, en, de);
                toolButtons[t] = b;
                tools.Items.Add(b);
            }

            AddTool(DrawTool.Pencil, "Pen", "Pencil (B): pixel exact drawing.", "Stift (B): pixelgenau zeichnen.");
            AddTool(DrawTool.Eraser, "Eraser", "Eraser (E): makes pixels transparent.", "Radierer (E): macht Pixel transparent.");
            AddTool(DrawTool.Line, "Line", "Line (L). Shift = 0/45/90°.", "Linie (L). Shift = 0/45/90°.");
            AddTool(DrawTool.Rectangle, "Rect", "Rectangle (R). Shift = square.", "Rechteck (R). Shift = Quadrat.");
            AddTool(DrawTool.FilledRectangle, "Rect■", "Filled rectangle.", "Gefülltes Rechteck.");
            AddTool(DrawTool.Ellipse, "Ellipse", "Ellipse (O). Shift = circle.", "Ellipse (O). Shift = Kreis.");
            AddTool(DrawTool.FilledEllipse, "Ellipse●", "Filled ellipse.", "Gefüllte Ellipse.");
            AddTool(DrawTool.Fill, "Fill", "Flood fill (G). Uses the tolerance.", "Füllen (G). Nutzt die Toleranz.");
            AddTool(DrawTool.Pipette, "Pick", "Pipette (I): left = primary, right = secondary color. Alt+click works with every tool.",
                "Pipette (I): links = Hauptfarbe, rechts = Zweitfarbe. Alt+Klick geht mit jedem Werkzeug.");
            AddTool(DrawTool.Move, "Move", "Move (M): drag the active layer. Arrow keys move by 1 px (Shift = 10).",
                "Verschieben (M): aktive Ebene ziehen. Pfeiltasten verschieben um 1 px (Shift = 10).");

            AddTool(DrawTool.Select, "Select", "Select (S): drag a rectangle (Ctrl + drag works with every tool). Then load an image into it, paste, copy or clear it (Layers tab / Edit menu). Esc = deselect.",
                "Auswahl (S): Rechteck aufziehen (Strg + Ziehen geht mit jedem Werkzeug). Dann Bild hineinladen, einfügen, kopieren oder leeren (Reiter Layers / Menü Edit). Esc = Auswahl aufheben.");

            AddTool(DrawTool.Overlay, "Overlay", "Overlay (P): click an overlay to select it, drag to move it. Arrow keys move it by 1 px (Shift = 10), Delete removes it. Overlays are floating images above the frames (Overlay tab).",
                "Overlay (P): Overlay anklicken zum Auswählen, ziehen zum Verschieben. Pfeiltasten verschieben um 1 px (Shift = 10), Entf löscht es. Overlays sind schwebende Bilder über den Frames (Reiter Overlay).");

            tools.Items.Add(new ToolStripSeparator());

            btKeyTransparent = new ToolStripButton("B/W transparent") { CheckOnClick = true, DisplayStyle = ToolStripItemDisplayStyle.Text };
            btKeyTransparent.CheckedChanged += (s, e) => SetKeyTransparent(btKeyTransparent.Checked);
            Tip(btKeyTransparent, "On/off (T): shows black, white and light gray of the original images as transparent. The images are not changed – switch it off and the colors are back. Also used for export and for images loaded into a selection.",
                "Ein/aus (T): zeigt Schwarz, Weiß und Hellgrau der Original-Bilder transparent. Die Bilder werden nicht verändert – ausschalten und die Farben sind wieder da. Gilt auch für Export und für Bilder, die in eine Auswahl geladen werden.");
            tools.Items.Add(btKeyTransparent);

            tools.Items.Add(new ToolStripSeparator());

            btPrimary = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image };
            btPrimary.Click += (s, e) => PickColorDialog(true);
            Tip(btPrimary, "Primary color (left mouse). Click to choose.", "Hauptfarbe (linke Maustaste). Klicken zum Wählen.");
            btSecondary = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image };
            btSecondary.Click += (s, e) => PickColorDialog(false);
            Tip(btSecondary, "Secondary color (right mouse). Click to choose.", "Zweitfarbe (rechte Maustaste). Klicken zum Wählen.");
            btSwapColors = new ToolStripButton("⇄") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            btSwapColors.Click += (s, e) => SwapColors();
            Tip(btSwapColors, "Swap primary and secondary color (X).", "Haupt- und Zweitfarbe tauschen (X).");
            tools.Items.AddRange(new ToolStripItem[] { btPrimary, btSecondary, btSwapColors });

            numAlpha = Num(0, 255, 255, 48);
            numAlpha.ValueChanged += (s, e) => ApplyColors();
            tips.SetToolTip(numAlpha, AnimEditHelp.Tip("Alpha of the paint color (255 = opaque, lower = semi transparent).", "Alpha der Malfarbe (255 = deckend, kleiner = halbtransparent)."));
            tools.Items.Add(new ToolStripLabel("A"));
            tools.Items.Add(Host(numAlpha));

            tools.Items.Add(new ToolStripSeparator());
            numBrush = Num(1, 64, 1, 44);
            numBrush.ValueChanged += (s, e) => canvas.BrushSize = (int)numBrush.Value;
            tips.SetToolTip(numBrush, AnimEditHelp.Tip("Brush size in pixels ([ and ] change it).", "Pinselgröße in Pixeln ([ und ] ändern sie)."));
            tools.Items.Add(new ToolStripLabel("Size"));
            tools.Items.Add(Host(numBrush));

            btRound = new ToolStripButton("Round") { CheckOnClick = true, Checked = true, DisplayStyle = ToolStripItemDisplayStyle.Text };
            btRound.CheckedChanged += (s, e) => canvas.RoundBrush = btRound.Checked;
            Tip(btRound, "Round brush (off = square brush).", "Runder Pinsel (aus = eckiger Pinsel).");
            tools.Items.Add(btRound);

            numTolerance = Num(0, 255, 0, 44);
            numTolerance.ValueChanged += (s, e) => canvas.FillTolerance = (int)numTolerance.Value;
            tips.SetToolTip(numTolerance, AnimEditHelp.Tip("Color tolerance for fill, replace and 'color → transparent' (0 = exact).", "Farbtoleranz für Füllen, Ersetzen und 'Farbe → transparent' (0 = exakt)."));
            tools.Items.Add(new ToolStripLabel("Tol"));
            tools.Items.Add(Host(numTolerance));

            tools.Items.Add(new ToolStripSeparator());
            tools.Items.Add(new ToolStripLabel("Draw on"));
            cbLayer = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 80 };
            cbLayer.Items.AddRange(new object[] { "Paint", "Original" });
            cbLayer.SelectedIndex = 0;
            cbLayer.SelectedIndexChanged += (s, e) => { canvas.Layer = cbLayer.SelectedIndex == 1 ? EditLayer.Original : EditLayer.Paint; canvas.Invalidate(); };
            Tip(cbLayer, "Layer the tools work on: Paint (own drawing layer, original stays untouched) or Original (the imported image itself). Tab switches.",
                "Ebene für die Werkzeuge: Paint (eigene Zeichenebene, Original bleibt unberührt) oder Original (das importierte Bild selbst). Tab wechselt.");
            tools.Items.Add(cbLayer);

            btAllChecked = new ToolStripButton("All checked") { CheckOnClick = true, DisplayStyle = ToolStripItemDisplayStyle.Text };
            btAllChecked.CheckedChanged += (s, e) =>
            {
                btAllChecked.BackColor = btAllChecked.Checked ? Color.LightGreen : SystemColors.Control;
                SetStatus(btAllChecked.Checked ? "Edits now apply to the current AND all checked frames." : "Edits apply to the current frame only.");
            };
            Tip(btAllChecked, "Multi frame mode: drawing, moving, nudging, mirroring, clearing … is applied to the current frame AND all checked frames (one undo step).",
                "Mehrfach-Modus: Zeichnen, Verschieben, Pfeiltasten, Spiegeln, Leeren … wirkt auf den aktuellen UND alle angehakten Frames (ein Undo-Schritt).");
            tools.Items.Add(btAllChecked);

            tools.Items.Add(new ToolStripSeparator());
            btUndo = new ToolStripButton("Undo") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            btUndo.Click += (s, e) => DoUndo();
            Tip(btUndo, "Undo (Ctrl+Z).", "Rückgängig (Strg+Z).");
            btRedo = new ToolStripButton("Redo") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            btRedo.Click += (s, e) => DoRedo();
            Tip(btRedo, "Redo (Ctrl+Y / Ctrl+Shift+Z).", "Wiederholen (Strg+Y / Strg+Shift+Z).");
            tools.Items.AddRange(new ToolStripItem[] { btUndo, btRedo });

            tools.Items.Add(new ToolStripSeparator());
            btZoomOut = new ToolStripButton("−") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            btZoomOut.Click += (s, e) => SetZoom(canvas.Zoom - 1);
            Tip(btZoomOut, "Zoom out (-, Ctrl+wheel).", "Verkleinern (-, Strg+Mausrad).");
            lbZoom = new ToolStripLabel("3x");
            btZoomIn = new ToolStripButton("+") { DisplayStyle = ToolStripItemDisplayStyle.Text };
            btZoomIn.Click += (s, e) => SetZoom(canvas.Zoom + 1);
            Tip(btZoomIn, "Zoom in (+, Ctrl+wheel).", "Vergrößern (+, Strg+Mausrad).");
            tools.Items.AddRange(new ToolStripItem[] { btZoomOut, lbZoom, btZoomIn });

            btHelp = new ToolStripButton("? Help") { DisplayStyle = ToolStripItemDisplayStyle.Text, Alignment = ToolStripItemAlignment.Right };
            btHelp.Click += (s, e) => AnimEditHelp.Show(this, System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");
            Tip(btHelp, "Help page with explanations (F1).", "Hilfe-Seite mit Erklärungen (F1).");
            tools.Items.Add(btHelp);
        }

        #endregion

        #region Status bar

        private void BuildStatus()
        {
            status = new StatusStrip();
            stPos = new ToolStripStatusLabel("x: -  y: -") { AutoSize = false, Width = 110, TextAlign = ContentAlignment.MiddleLeft };
            stColorSwatch = new ToolStripStatusLabel { DisplayStyle = ToolStripItemDisplayStyle.Image };
            stColor = new ToolStripStatusLabel("") { AutoSize = false, Width = 150, TextAlign = ContentAlignment.MiddleLeft };
            stFrame = new ToolStripStatusLabel("") { AutoSize = false, Width = 360, TextAlign = ContentAlignment.MiddleLeft };
            stZoom = new ToolStripStatusLabel("") { AutoSize = false, Width = 60 };
            stMsg = new ToolStripStatusLabel("") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            status.Items.AddRange(new ToolStripItem[] { stPos, stColorSwatch, stColor, stFrame, stZoom, stMsg });
        }

        #endregion

        #region Side panel

        private Control BuildSidePanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            // --- playback (always visible) ---
            preview = new PreviewBox { Dock = DockStyle.Top, Height = 250, Zoom = 2 };
            Tip(preview, "Live preview / playback of the animation (with background and reference).", "Live-Vorschau / Wiedergabe der Animation (mit Hintergrund und Referenz).");

            btPlay = Btn("▶ Play", (s, e) => TogglePlay(), 70);
            Tip(btPlay, "Play / stop the animation (Space).", "Animation abspielen / stoppen (Leertaste).");
            btPrevFrame = Btn("◀", (s, e) => StepFrame(-1), 30);
            Tip(btPrevFrame, "Previous frame (Page Up).", "Vorheriger Frame (Bild auf).");
            btNextFrame = Btn("▶", (s, e) => StepFrame(1), 30);
            Tip(btNextFrame, "Next frame (Page Down).", "Nächster Frame (Bild ab).");
            numFps = Num(1, 60, 8, 48);
            Tip(numFps, "Speed in frames per second.", "Geschwindigkeit in Frames pro Sekunde.");
            numFrom = Num(1, 10, 1, 42);
            Tip(numFrom, "Loop range: first frame.", "Schleifenbereich: erster Frame.");
            numTo = Num(1, 10, 10, 42);
            Tip(numTo, "Loop range: last frame.", "Schleifenbereich: letzter Frame.");
            numPreviewZoom = Num(1, 8, 2, 40);
            Tip(numPreviewZoom, "Zoom of the preview.", "Zoom der Vorschau.");

            chkLoop = Chk("Loop", true);
            Tip(chkLoop, "Repeat endlessly (off = stop after the last frame).", "Endlos wiederholen (aus = nach letztem Frame stoppen).");
            chkPingPong = Chk("Ping-pong");
            Tip(chkPingPong, "Play forward and back (1-2-3-2-1 …).", "Vor und zurück abspielen (1-2-3-2-1 …).");
            chkOnlyChecked = Chk("Only checked", true);
            Tip(chkOnlyChecked, "Play only checked frames.", "Nur angehakte Frames abspielen.");
            chkMirrorPreview = Chk("Mirrored too");
            Tip(chkMirrorPreview, "Shows a mirrored copy next to it (other walking direction).", "Zeigt daneben eine gespiegelte Kopie (andere Laufrichtung).");
            chkFollow = Chk("Follow in editor");
            Tip(chkFollow, "The editor and frame strip jump along with the playback.", "Editor und Frame-Leiste springen beim Abspielen mit.");
            chkPreviewRef = Chk("Show reference", true);
            Tip(chkPreviewRef, "Include the reference animation in the preview.", "Referenz-Animation in der Vorschau zeigen.");

            var playPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            playPanel.Controls.Add(Row(btPrevFrame, btPlay, btNextFrame, L("FPS"), numFps, L("Zoom"), numPreviewZoom));
            playPanel.Controls.Add(Row(L("From"), numFrom, L("To"), numTo, chkLoop, chkPingPong));
            playPanel.Controls.Add(Row(chkOnlyChecked, chkMirrorPreview, chkPreviewRef));
            playPanel.Controls.Add(Row(chkFollow));

            sideTabs = new TabControl { Dock = DockStyle.Fill };
            sideTabs.TabPages.Add(BuildOverlayPage());
            sideTabs.TabPages.Add(BuildLayersPage());
            sideTabs.TabPages.Add(BuildAlignPage());
            sideTabs.TabPages.Add(BuildViewPage());
            sideTabs.TabPages.Add(BuildColorsPage());
            sideTabs.TabPages.Add(BuildHistoryPage());

            panel.Controls.Add(sideTabs);
            panel.Controls.Add(playPanel);
            panel.Controls.Add(preview);
            return panel;
        }

        private TabPage BuildLayersPage()
        {
            chkOrigVisible = Chk("Original", true);
            Tip(chkOrigVisible, "Show the original layer (imported image).", "Original-Ebene anzeigen (importiertes Bild).");
            trkOrigOpacity = Trk(100);
            Tip(trkOrigOpacity, "Opacity of the original layer – lower it to trace over it.", "Deckkraft der Original-Ebene – verringern, um darüber nachzuzeichnen.");
            chkPaintVisible = Chk("Paint", true);
            Tip(chkPaintVisible, "Show your drawing layer.", "Deine Zeichenebene anzeigen.");
            trkPaintOpacity = Trk(100);
            Tip(trkPaintOpacity, "Opacity of the paint layer.", "Deckkraft der Paint-Ebene.");

            chkKeepPaint = Chk("Keep paint layer when importing", true);
            Tip(chkKeepPaint, "On: loading a new original keeps your drawing. Off: the paint layer is cleared.", "An: beim Laden eines neuen Originals bleibt die Zeichnung erhalten. Aus: Paint-Ebene wird geleert.");

            var frameLayers = Group("Layers of the frame / Ebenen",
                Row(chkOrigVisible, trkOrigOpacity),
                Row(chkPaintVisible, trkPaintOpacity),
                chkKeepPaint,
                Wrap(300,
                    Btn("Merge paint ↓", (s, e) => ApplyToAffected("Merge", i => doc.MergePaintDown(i))),
                    Btn("Clear layer", (s, e) => ClearTargetLayer()),
                    Btn("Paint → checked", (s, e) => CopyPaintToChecked())));

            chkRefVisible = Chk("Reference", true);
            Tip(chkRefVisible, "Show the reference animation (set with 'As reference' in the browsers).", "Referenz-Animation anzeigen (mit 'As reference' in den Browsern gesetzt).");
            trkRefOpacity = Trk(40);
            Tip(trkRefOpacity, "Opacity of the reference.", "Deckkraft der Referenz.");
            chkRefOnTop = Chk("On top");
            Tip(chkRefOnTop, "Draw the reference above the frame (otherwise behind it).", "Referenz über dem Frame zeichnen (sonst dahinter).");
            numRefFrame = Num(0, 999, 0, 52);
            Tip(numRefFrame, "0 = reference follows the frame number. 1..n = always show this reference frame.", "0 = Referenz folgt der Frame-Nummer. 1..n = immer diesen Referenz-Frame zeigen.");
            numRefOffX = Num(-1000, 1000, 0, 54);
            Tip(numRefOffX, "Horizontal offset of the reference.", "Waagerechter Versatz der Referenz.");
            numRefOffY = Num(-1000, 1000, 0, 54);
            Tip(numRefOffY, "Vertical offset of the reference.", "Senkrechter Versatz der Referenz.");
            lbRefName = new Label { AutoSize = true, Text = "(none)", ForeColor = Color.DimGray, Margin = new Padding(3, 4, 3, 0) };

            var reference = Group("Reference animation / Referenz",
                Row(chkRefVisible, trkRefOpacity),
                Row(chkRefOnTop, L("Frame"), numRefFrame),
                Row(L("Offset X"), numRefOffX, L("Y"), numRefOffY),
                lbRefName,
                Row(Btn("Load images…", (s, e) => LoadReferenceImages()),
                    Btn("Frames → ref", (s, e) => UseFramesAsReference()),
                    Btn("Clear", (s, e) => SetReference(null))));

            var ops = Group("Layer tools / Ebenen-Werkzeuge",
                Wrap(300,
                    Btn("B/W/Gray → transparent", (s, e) => MakeKeysTransparent()),
                    Btn("2nd color → transparent", (s, e) => SecondaryToTransparent()),
                    Btn("Replace 2nd → 1st", (s, e) => ReplaceSecondaryWithPrimary()),
                    Btn("Outline", (s, e) => DrawOutline())));

            lbSelection = new Label { AutoSize = true, Text = "No selection – tool 'Select' (S) or Ctrl + drag.", ForeColor = Color.DimGray, MaximumSize = new Size(290, 0), Margin = new Padding(3, 4, 3, 2) };
            chkSelKeepAspect = Chk("Keep aspect ratio", true);
            Tip(chkSelKeepAspect, "On: the image keeps its proportions and is centered in the selection. Off: stretched to fill it.", "An: Bild behält seine Proportionen und wird mittig in die Auswahl gesetzt. Aus: auf die Auswahl gestreckt.");
            var selection = Group("Selection / Auswahl",
                lbSelection,
                chkSelKeepAspect,
                Wrap(300,
                    Btn("Load image…", (s, e) => LoadImageIntoSelection()),
                    Btn("Paste", (s, e) => PasteIntoSelection()),
                    Btn("Copy", (s, e) => CopySelection()),
                    Btn("Clear area", (s, e) => ClearSelectionArea()),
                    Btn("Deselect", (s, e) => canvas.Selection = Rectangle.Empty)));

            return SidePage("Layers", selection, frameLayers, reference, ops);
        }

        private TabPage BuildOverlayPage()
        {
            lbOverlays = new ListBox { Width = 290, Height = 110, IntegralHeight = false };
            Tip(lbOverlays, "All overlays (floating images above the frames). Select one to edit it. They are not burned in until you press 'Bake'.",
                "Alle Overlays (schwebende Bilder über den Frames). Eins auswählen zum Bearbeiten. Sie werden erst mit 'Bake' fest eingebrannt.");

            chkAutoCrop = Chk("Auto-crop black/white border on import", true);
            Tip(chkAutoCrop, "Cuts away the transparent / black / white / light gray border of a loaded image, so only the object (e.g. the cage) is left.",
                "Schneidet den transparenten / schwarzen / weißen / hellgrauen Rand eines geladenen Bildes ab, sodass nur das Objekt (z. B. der Käfig) übrig bleibt.");

            var add = Group("New overlay / Neues Overlay",
                chkAutoCrop,
                Wrap(300,
                    Btn("Load image / GIF…", (s, e) => LoadOverlayFromFile()),
                    Btn("Paste", (s, e) => OverlayFromClipboard()),
                    Btn("Selection → overlay", (s, e) => OverlayFromSelection()),
                    Btn("Into selection…", (s, e) => LoadImageIntoSelection())));
            Tip(add, "Load an image or animated GIF as overlay, paste one, cut a marked area of the frame as overlay, or fit an image into the marked area.",
                "Bild oder animiertes GIF als Overlay laden, einfügen, markierten Bereich des Frames als Overlay ausschneiden oder ein Bild in den markierten Bereich einpassen.");

            numOvX = Num(-2048, 2048, 0, 58);
            Tip(numOvX, "X position of the overlay.", "X-Position des Overlays.");
            numOvY = Num(-2048, 2048, 0, 58);
            Tip(numOvY, "Y position of the overlay.", "Y-Position des Overlays.");
            numOvScale = Num(10, 1600, 100, 58);
            Tip(numOvScale, "Size in percent. Pixel exact (nearest neighbour): no blur, no new colors.", "Größe in Prozent. Pixelgenau (Nearest Neighbour): kein Verschwimmen, keine neuen Farben.");
            cbOvTint = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
            cbOvTint.Items.AddRange(new object[] { "Original", "Gray", "Color" });
            cbOvTint.SelectedIndex = 0;
            Tip(cbOvTint, "Tint: Original colors, Gray (like particle gray) or a color of your choice. Black/white/light gray are never tinted, so they stay transparent.",
                "Einfärben: Originalfarben, Grau (wie Partikel-Grau) oder eine Wunschfarbe. Schwarz/Weiß/Hellgrau werden nie eingefärbt und bleiben transparent.");
            btOvColor = Btn("Color", (s, e) => PickOverlayColor(), 50);
            btOvColor.BackColor = Color.Gray;
            Tip(btOvColor, "Tint color (for 'Color'). Right click = take the primary color.", "Farbe zum Einfärben (für 'Color'). Rechtsklick = Hauptfarbe übernehmen.");
            btOvColor.MouseUp += (s, e) => { if (e.Button == MouseButtons.Right) SetOverlayTintColor(canvas.PrimaryColor); };
            cbOvScope = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            cbOvScope.Items.AddRange(new object[] { "All frames", "Checked frames", "Current frame only" });
            cbOvScope.SelectedIndex = 0;
            Tip(cbOvScope, "On which frames the overlay is shown – 'All frames' places it once for the whole animation, even if the frames differ.",
                "Auf welchen Frames das Overlay erscheint – 'All frames' setzt es einmal für die ganze Animation, auch wenn die Frames verschieden sind.");
            chkOvVisible = Chk("Visible", true);
            trkOvOpacity = Trk(100);
            Tip(trkOvOpacity, "Opacity of the overlay.", "Deckkraft des Overlays.");
            chkOvFrames = Chk("Show frames + size labels", true);
            Tip(chkOvFrames, "Shows a frame with name, size and position around every overlay. Together with 'B/W transparent' off you see the full image size.",
                "Zeigt um jedes Overlay einen Rahmen mit Name, Größe und Position. Zusammen mit 'B/W transparent' aus siehst du die volle Bildgröße.");

            var edit = Group("Selected overlay / Gewähltes Overlay",
                lbOverlays,
                Row(L("X"), numOvX, L("Y"), numOvY, chkOvVisible),
                Row(L("Scale %"), numOvScale, Btn("×2", (s, e) => ScaleOverlay(2.0), 34), Btn("×½", (s, e) => ScaleOverlay(0.5), 34), Btn("Fit sel.", (s, e) => FitOverlayToSelection())),
                Row(L("Tint"), cbOvTint, btOvColor),
                Row(L("Show on"), cbOvScope),
                Row(L("Opacity"), trkOvOpacity),
                chkOvFrames,
                Wrap(300,
                    Btn("Duplicate", (s, e) => DuplicateOverlay()),
                    Btn("Center on anchor", (s, e) => CenterOverlay()),
                    Btn("Up", (s, e) => MoveOverlayOrder(1), 40),
                    Btn("Down", (s, e) => MoveOverlayOrder(-1), 50),
                    Btn("Delete", (s, e) => DeleteOverlay())));

            var bake = Group("Bake / Einbrennen",
                Wrap(300,
                    Btn("Bake into frames", (s, e) => BakeOverlay(false)),
                    Btn("Bake + remove", (s, e) => BakeOverlay(true)),
                    Btn("Bake all + remove", (s, e) => BakeAllOverlays())));
            Tip(bake, "Burns the overlay into the paint layer of its frames (black/white left out). Until then it stays movable. Undo works.",
                "Brennt das Overlay in die Paint-Ebene seiner Frames ein (Schwarz/Weiß bleiben frei). Bis dahin bleibt es verschiebbar. Undo geht.");

            return SidePage("Overlay", add, edit, bake);
        }

        private TabPage BuildAlignPage()
        {
            numOffX = Num(-2048, 2048, 0, 60);
            Tip(numOffX, "X position of the original layer on the canvas.", "X-Position der Original-Ebene auf der Leinwand.");
            numOffY = Num(-2048, 2048, 0, 60);
            Tip(numOffY, "Y position of the original layer on the canvas.", "Y-Position der Original-Ebene auf der Leinwand.");
            lbFrameInfo = new Label { AutoSize = true, MaximumSize = new Size(290, 0), Margin = new Padding(3, 4, 3, 4) };

            var pos = Group("Position of the original / Position",
                Row(L("X"), numOffX, L("Y"), numOffY),
                lbFrameInfo,
                Wrap(300,
                    Btn("Center on anchor", (s, e) => ApplyToAffected("Center", CenterOnAnchor)),
                    Btn("Bottom on anchor", (s, e) => ApplyToAffected("Align bottom", i => doc.AlignBottomToAnchor(i))),
                    Btn("Align checked", (s, e) => AlignCheckedBottoms()),
                    Btn("Offset → checked", (s, e) => CopyOffsetToChecked())));

            numScale = Num(10, 800, 200, 56);
            Tip(numScale, "Scale factor in percent (nearest neighbour, pixel exact).", "Skalierung in Prozent (Nearest Neighbour, pixelgenau).");
            var transform = Group("Transform / Umformen",
                Row(L("Scale %"), numScale, Btn("Scale", (s, e) => ApplyToAffected("Scale", i => doc.ScaleOriginal(i, (double)numScale.Value / 100.0)))),
                Wrap(300,
                    Btn("Mirror", (s, e) => ApplyToAffected("Mirror", i => doc.MirrorFrame(i))),
                    Btn("Rotate 90°", (s, e) => ApplyToAffected("Rotate", i => doc.RotateOriginal90(i))),
                    Btn("×2", (s, e) => ApplyToAffected("Scale", i => doc.ScaleOriginal(i, 2))),
                    Btn("×½", (s, e) => ApplyToAffected("Scale", i => doc.ScaleOriginal(i, 0.5)))));

            numCanvasW = Num(16, 2048, doc.CanvasSize.Width, 60);
            numCanvasH = Num(16, 2048, doc.CanvasSize.Height, 60);
            Tip(numCanvasW, "Canvas width (all frames).", "Leinwandbreite (alle Frames).");
            Tip(numCanvasH, "Canvas height (all frames).", "Leinwandhöhe (alle Frames).");
            numAnchorX = Num(0, 2048, doc.Anchor.X, 60);
            numAnchorY = Num(0, 2048, doc.Anchor.Y, 60);
            Tip(numAnchorX, "Anchor X: where UO frames place their center (green cross).", "Anker X: hier setzen UO-Frames ihr Center (grünes Kreuz).");
            Tip(numAnchorY, "Anchor Y: the feet / ground line of the animation.", "Anker Y: Fuß- / Bodenlinie der Animation.");

            var canvasGroup = Group("Canvas / Leinwand",
                Row(L("W"), numCanvasW, L("H"), numCanvasH, Btn("Apply", (s, e) => ApplyCanvasSize())),
                Row(L("Anchor X"), numAnchorX, L("Y"), numAnchorY),
                Row(Btn("Fit to frames", (s, e) => FitCanvas())));

            return SidePage("Align", pos, transform, canvasGroup);
        }

        private TabPage BuildViewPage()
        {
            chkGrid = Chk("Pixel grid (zoom ≥ 6)", true);
            Tip(chkGrid, "Shows a pixel grid at high zoom.", "Zeigt bei hohem Zoom ein Pixelraster.");
            chkAnchor = Chk("Anchor", true);
            Tip(chkAnchor, "Shows the anchor (green cross = feet position).", "Zeigt den Anker (grünes Kreuz = Fußpunkt).");
            chkBounds = Chk("Original bounds", true);
            Tip(chkBounds, "Dashed box around the original layer while 'Draw on: Original'.", "Gestrichelter Rahmen um die Original-Ebene bei 'Draw on: Original'.");
            chkGuides = Chk("Guide lines", true);
            Tip(chkGuides, "Show the guide lines below (helpful to keep height and position equal in all frames).", "Hilfslinien anzeigen (hilft, Höhe und Position in allen Frames gleich zu halten).");
            numGuideY1 = Num(-1, 2048, -1, 58);
            Tip(numGuideY1, "Height line 1 (y in pixels, -1 = off). E.g. ground.", "Höhenlinie 1 (y in Pixeln, -1 = aus). Z. B. Boden.");
            numGuideY2 = Num(-1, 2048, -1, 58);
            Tip(numGuideY2, "Height line 2, dashed (y, -1 = off). E.g. head height.", "Höhenlinie 2, gestrichelt (y, -1 = aus). Z. B. Kopfhöhe.");
            numGuideX = Num(-1, 2048, -1, 58);
            Tip(numGuideX, "Vertical line (x, -1 = off).", "Senkrechte Linie (x, -1 = aus).");
            btGuideColor = Btn("Color", (s, e) => PickGuideColor(), 50);
            btGuideColor.BackColor = Color.Red;
            Tip(btGuideColor, "Color of the guide lines.", "Farbe der Hilfslinien.");

            chkZoomPerFrame = Chk("Zoom per frame");
            Tip(chkZoomPerFrame, "Every frame remembers its own editor zoom (otherwise one global zoom).", "Jeder Frame merkt sich seinen eigenen Editor-Zoom (sonst ein globaler Zoom).");

            var guides = Group("Guides / Hilfslinien",
                Row(chkGrid, chkZoomPerFrame), Row(chkAnchor, chkBounds), Row(chkGuides, btGuideColor),
                Row(L("Y1"), numGuideY1, L("Y2"), numGuideY2, L("X"), numGuideX),
                Row(Btn("Lines at anchor", (s, e) => GuidesAtAnchor()), Btn("Off", (s, e) => { numGuideY1.Value = -1; numGuideY2.Value = -1; numGuideX.Value = -1; })));

            chkOnion = Chk("Onion skin");
            Tip(chkOnion, "Shows neighbour frames transparently (previous red, next blue) – essential for smooth motion.", "Zeigt Nachbar-Frames durchscheinend (vorherige rot, nächste blau) – wichtig für flüssige Bewegung.");
            numOnionPrev = Num(0, 4, 1, 40);
            Tip(numOnionPrev, "How many previous frames.", "Wie viele vorherige Frames.");
            numOnionNext = Num(0, 4, 1, 40);
            Tip(numOnionNext, "How many next frames.", "Wie viele nächste Frames.");
            trkOnionOpacity = Trk(35);
            Tip(trkOnionOpacity, "Opacity of the onion skin.", "Deckkraft des Onion-Skins.");
            chkOnionTint = Chk("Tint", true);
            Tip(chkOnionTint, "Color the neighbour frames red/blue.", "Nachbar-Frames rot/blau einfärben.");
            chkOnionWrap = Chk("Wrap", true);
            Tip(chkOnionWrap, "Frame 1 also shows frame 10 (loop).", "Frame 1 zeigt auch Frame 10 (Schleife).");

            var onion = Group("Onion skin",
                Row(chkOnion, chkOnionTint, chkOnionWrap),
                Row(L("Prev"), numOnionPrev, L("Next"), numOnionNext),
                Row(L("Opacity"), trkOnionOpacity));

            cbBackground = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
            cbBackground.Items.AddRange(new object[] { "Checker", "Dark", "Black", "White", "Magenta", "Green", "Water", "Sand", "Street", "Forest", "Dirt", "Dungeon", "Cave", "Custom image…" });
            cbBackground.SelectedIndex = 0;
            Tip(cbBackground, "Background for editor and preview (ground tiles show how it looks in game).", "Hintergrund für Editor und Vorschau (Boden-Kacheln zeigen, wie es im Spiel wirkt).");

            var bg = Group("Background / Hintergrund", cbBackground);

            return SidePage("View", guides, onion, bg);
        }

        private TabPage BuildColorsPage()
        {
            pnlPrimary = new Panel { Size = new Size(36, 36), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            pnlPrimary.Click += (s, e) => PickColorDialog(true);
            Tip(pnlPrimary, "Primary color – click to choose.", "Hauptfarbe – klicken zum Wählen.");
            pnlSecondary = new Panel { Size = new Size(28, 28), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand, Margin = new Padding(3, 10, 3, 3) };
            pnlSecondary.Click += (s, e) => PickColorDialog(false);
            Tip(pnlSecondary, "Secondary color – click to choose.", "Zweitfarbe – klicken zum Wählen.");
            tbHex = new TextBox { Width = 80, Margin = new Padding(3, 10, 3, 3) };
            Tip(tbHex, "Primary color as hex code (#RRGGBB). Press Enter to apply.", "Hauptfarbe als Hex-Code (#RRGGBB). Enter übernimmt.");

            flowRecent = new FlowLayoutPanel { Width = 290, Height = 48, Margin = new Padding(0) };
            Tip(flowRecent, "Recently used colors. Left click = primary, right click = secondary.", "Zuletzt benutzte Farben. Linksklick = Haupt-, Rechtsklick = Zweitfarbe.");
            flowFramePalette = new FlowLayoutPanel { Width = 290, Height = 110, AutoScroll = true, Margin = new Padding(0) };
            Tip(flowFramePalette, "Colors used in the current frame (most frequent first). Left = primary, right = secondary.", "Im aktuellen Frame benutzte Farben (häufigste zuerst). Links = Haupt-, rechts = Zweitfarbe.");
            flowBasic = new FlowLayoutPanel { Width = 290, Height = 48, Margin = new Padding(0) };

            var current = Group("Colors / Farben",
                Row(pnlPrimary, pnlSecondary, tbHex, Btn("⇄", (s, e) => SwapColors(), 30)),
                L("Recent / Zuletzt"), flowRecent,
                L("Basic / Grundfarben"), flowBasic);

            var framePal = Group("Frame palette / Frame-Palette",
                Btn("Read colors of current frame", (s, e) => FillFramePalette()),
                flowFramePalette);

            return SidePage("Colors", current, framePal);
        }

        private TabPage BuildHistoryPage()
        {
            lbHistory = new ListBox { Width = 290, Height = 300, IntegralHeight = false };
            Tip(lbHistory, "All undo steps (newest at the bottom). Double click = undo back to this step.", "Alle Undo-Schritte (neueste unten). Doppelklick = bis zu diesem Schritt rückgängig.");
            lbHistory.DoubleClick += (s, e) => UndoTo(lbHistory.SelectedIndex);

            var g = Group("History / Verlauf",
                Row(Btn("Undo", (s, e) => DoUndo()), Btn("Redo", (s, e) => DoRedo()), Btn("Clear history", (s, e) => { doc.ClearHistory(); })),
                lbHistory);
            return SidePage("History", g);
        }

        #endregion
    }
}
