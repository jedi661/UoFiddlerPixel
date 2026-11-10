// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ParticleGray;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Helpers;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Particle gray (partial hue): turns colored images – or only marked parts of them – into the exact gray
    /// that Ultima Online recolors with a hue. With mask tools (brush, magic wand, stamps, selections), zoom,
    /// hue preview, art.mul items / land tiles and analysis.
    /// </summary>
    public partial class ParticleGrayForm : Form
    {
        private const int MaxUndo = 30;

        private Bitmap _image;     // edited image (owned)
        private Bitmap _original;  // as loaded – for "restore colors"
        private bool[] _mask;
        private Bitmap _huePreview;
        private bool _hueOnCanvas;
        private readonly List<(Bitmap Image, bool[] Mask)> _undo = new List<(Bitmap, bool[])>();
        private readonly List<(Bitmap Image, bool[] Mask)> _redo = new List<(Bitmap, bool[])>();
        private readonly List<(string Name, bool[,] Shape, Bitmap Preview, bool BuiltIn)> _stamps = new List<(string, bool[,], Bitmap, bool)>();
        private ushort[] _hueColors;
        private string _hueName;
        private Color _dyeColor = Color.FromArgb(0xFF, 0x99, 0x33);
        private bool[] _usedArt;
        private bool _updating;

        public ParticleGrayForm()
        {
            InitializeComponent();
            BuildToolbar();
            BuildMaskTab();
            BuildGrayTab();
            BuildHueTab();
            BuildArtTab();
            BuildAnalysisTab();

            canvas.BeginEdit += (s, e) => { ShowImageOnCanvas(); PushUndo(); };
            canvas.ImageEdited += (s, e) => ImageChanged();
            canvas.BeginMaskEdit += (s, e) => { ShowImageOnCanvas(); PushUndo(); };
            canvas.MaskChanged += (s, e) => UpdateStatus();
            canvas.HoverPixel += Canvas_HoverPixel;
            canvas.ColorPicked += (s, p) =>
            {
                if (p.Secondary)
                {
                    SetDyeColor(Color.FromArgb(255, p.Color));
                }
                else
                {
                    canvas.PrimaryColor = Color.FromArgb(255, p.Color);
                    UpdateColorButton();
                }
                SetMessage($"Picked #{p.Color.R:X2}{p.Color.G:X2}{p.Color.B:X2}" + (p.Secondary ? " as dye color." : " as paint color."));
            };
            canvas.SelectionChanged += (s, e) => UpdateStatus();
            canvas.MaskColor = Color.FromArgb(120, 255, 0, 0);
            canvas.FillTolerance = 20;
            panelCanvas.MouseWheel += Canvas_MouseWheel;
            canvas.MouseWheel += Canvas_MouseWheel;
            tabControl1.SelectedIndexChanged += (s, e) =>
            {
                if (tabControl1.SelectedTab == tabArt && _usedArt == null)
                {
                    ReloadArt();
                }
            };

            LoadHue(1);
            SetTool(CanvasTool.MaskPaint);
            SyncUi();
            UpdateStatus();
            SetMessage("Load an image, an art item or a land tile. Mark areas (brush, wand, stamps), then \"Convert to particle gray\". F1 = help.");
        }

        #region Helpers

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private void SetMessage(string text) => statusMessage.Text = text;

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 1, 0, 1) };
            row.Controls.AddRange(controls);
            return row;
        }

        private static Label Lbl(string text, int width = 0) => width > 0
            ? new Label { Text = text, Width = width, Margin = new Padding(3, 7, 3, 0) }
            : new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 0) };

        private Button Btn(string text, EventHandler click, string tip = null, int width = 0)
        {
            var b = new Button { Text = text, AutoSize = width == 0, Height = 26, UseVisualStyleBackColor = true, Margin = new Padding(2) };
            if (width > 0)
            {
                b.Width = width;
            }
            b.Click += click;
            if (tip != null)
            {
                toolTip1.SetToolTip(b, tip);
            }
            return b;
        }

        private static NumericUpDown Num(int min, int max, int value, int width = 55) =>
            new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, value)), Width = width, Margin = new Padding(3, 3, 3, 0) };

        private static CheckBox Chk(string text, bool value = false) => new CheckBox { Text = text, AutoSize = true, Checked = value, Margin = new Padding(3, 5, 3, 0) };

        private static GroupBox Group(string title, params Control[] rows)
        {
            var box = new GroupBox { Text = title, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(6, 4, 6, 6), Margin = new Padding(3, 3, 3, 6) };
            var flow = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Dock = DockStyle.Fill };
            flow.Controls.AddRange(rows);
            box.Controls.Add(flow);
            return box;
        }

        private static void FillTab(TabPage page, params Control[] groups)
        {
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            flow.Controls.AddRange(groups);
            page.Controls.Add(flow);
        }

        private bool CheckImage()
        {
            if (_image != null)
            {
                return true;
            }
            MessageBox.Show(this, "No image is loaded.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private bool HasMask => _mask != null && _mask.Any(m => m);

        private static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private bool IsTextInputFocused()
        {
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if (item is ToolStripComboBox cb && cb.Focused)
                {
                    return true;
                }
            }
            Control c = ActiveControl;
            while (c is ContainerControl cc && cc.ActiveControl != null)
            {
                c = cc.ActiveControl;
            }
            return c is TextBoxBase || c is UpDownBase || c?.Parent is UpDownBase || c is ComboBox || c is ArtTileGrid || c is ListBox;
        }

        #endregion

        #region Toolbar

        private readonly Dictionary<CanvasTool, ToolStripButton> _toolButtons = new Dictionary<CanvasTool, ToolStripButton>();
        private ToolStripButton _tsUndo, _tsRedo, _tsShowMask, _tsRound, _tsMirror, _tsHueView, _tsColor;
        private ToolStripComboBox _tsShape, _tsBrush;
        private ToolStripLabel _tsZoom;

        private ToolStripButton TextButton(string text, string tip, EventHandler click, bool bold = false)
        {
            var b = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = tip };
            if (bold)
            {
                b.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }
            b.Click += click;
            return b;
        }

        private ToolStripButton ImageButton(Image image, string tip, EventHandler click)
        {
            var b = new ToolStripButton(image) { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = tip };
            b.Click += click;
            return b;
        }

        private void BuildToolbar()
        {
            _tsUndo = TextButton("↶", Tip("Undo (Ctrl+Z) – image and mask", "Rückgängig (Strg+Z) – Bild und Maske"), (s, e) => Undo(), true);
            _tsRedo = TextButton("↷", Tip("Redo (Ctrl+Y)", "Wiederholen (Strg+Y)"), (s, e) => Redo(), true);
            void Tool(CanvasTool t, string text, string en, string de) => _toolButtons[t] = TextButton(text, Tip(en, de), (s, e) => SetTool(t));
            Tool(CanvasTool.MaskPaint, "Mask brush", "Mark pixels (B). Left = mark, right = unmark. Brush size and round / square in the toolbar.",
                "Pixel markieren (B). Links = markieren, rechts = entfernen. Pinselgröße und rund / eckig in der Leiste.");
            Tool(CanvasTool.Wand, "Wand", "Magic wand (W): marks connected pixels of similar color. Shift = all similar pixels in the image. Right = unmark. Tolerance: Mask tab.",
                "Zauberstab (W): markiert zusammenhängende Pixel ähnlicher Farbe. Umschalt = alle ähnlichen im Bild. Rechts = entfernen. Toleranz: Reiter Mask.");
            Tool(CanvasTool.Stamp, "Stamp", "Stamp (T): marks the chosen shape (Mask tab). Drag = stamp repeatedly, right = unmark.",
                "Stempel (T): markiert die gewählte Form (Reiter Mask). Ziehen = mehrfach stempeln, rechts = entfernen.");
            Tool(CanvasTool.Select, "Select", "Selection (M): rectangle, ellipse, freehand, polygon – then \"Selection → mask\".",
                "Auswahl (M): Rechteck, Ellipse, Freihand, Polygon – dann \"Selection → mask\".");
            Tool(CanvasTool.Pencil, "Pencil", "Paint pixels with the paint color (P).", "Pixel mit der Malfarbe malen (P).");
            Tool(CanvasTool.Eraser, "Eraser", "Erase to transparent (E).", "Zu transparent radieren (E).");
            Tool(CanvasTool.Pipette, "Pipette", "Left = paint color, right = dye color (I).", "Links = Malfarbe, rechts = Färbefarbe (I).");

            _tsShape = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(85, 23), ToolTipText = Tip("Shape of the selection", "Form der Auswahl") };
            _tsShape.Items.AddRange(new object[] { "Rectangle", "Ellipse", "Freehand", "Polygon" });
            _tsShape.SelectedIndex = 2;
            canvas.SelectionShape = CanvasSelection.Freehand;
            _tsShape.SelectedIndexChanged += (s, e) => { canvas.SelectionShape = (CanvasSelection)_tsShape.SelectedIndex; canvas.ClearSelection(); SetTool(CanvasTool.Select); };

            _tsBrush = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(45, 23), ToolTipText = Tip("Brush size (mask brush, pencil, eraser)", "Pinselgröße (Maskenpinsel, Stift, Radierer)") };
            foreach (int i in new[] { 1, 2, 3, 4, 5, 6, 8, 10, 12, 16, 24, 32 })
            {
                _tsBrush.Items.Add(i);
            }
            _tsBrush.SelectedIndex = 2;
            canvas.BrushSize = 3;
            _tsBrush.SelectedIndexChanged += (s, e) => canvas.BrushSize = (int)_tsBrush.SelectedItem;
            _tsRound = TextButton("Round", Tip("Round or square mask brush", "Runder oder eckiger Maskenpinsel"), (s, e) => { canvas.RoundBrush = !canvas.RoundBrush; SyncUi(); });
            _tsMirror = TextButton("Mirror", Tip("Mirror every stroke left / right", "Jeden Strich links / rechts spiegeln"), (s, e) => { canvas.MirrorDraw = !canvas.MirrorDraw; SyncUi(); });
            _tsShowMask = TextButton("Show mask", Tip("Show the red mask", "Rote Maske anzeigen"), (s, e) => { canvas.ShowMask = !canvas.ShowMask; canvas.Invalidate(); SyncUi(); });
            _tsHueView = TextButton("Hue view", Tip("Shows the image with the hue of the Hue tab, like in UO (H). Editing switches back.",
                "Zeigt das Bild mit dem Hue aus dem Reiter Hue, wie in UO (H). Bearbeiten schaltet zurück."), (s, e) => ToggleHueView());
            _tsColor = new ToolStripButton { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = Tip("Paint color – click to choose", "Malfarbe – Klick zum Wählen") };
            _tsColor.Click += (s, e) =>
            {
                using (var d = new ColorDialog { Color = canvas.PrimaryColor, FullOpen = true })
                {
                    if (d.ShowDialog(this) == DialogResult.OK)
                    {
                        canvas.PrimaryColor = d.Color;
                        UpdateColorButton();
                    }
                }
            };
            UpdateColorButton();
            _tsZoom = new ToolStripLabel("1x") { AutoSize = false, Width = 36 };

            toolStrip1.Items.AddRange(new ToolStripItem[]
            {
                ImageButton(Properties.Resources.Load, Tip("Open image (Ctrl+O)", "Bild öffnen (Strg+O)"), (s, e) => OpenImage()),
                ImageButton(Properties.Resources.import, Tip("Paste image (Ctrl+V)", "Bild einfügen (Strg+V)"), (s, e) => PasteImage()),
                ImageButton(Properties.Resources.Save, Tip("Save image (Ctrl+S)", "Bild speichern (Strg+S)"), (s, e) => SaveImage()),
                ImageButton(Properties.Resources.Copy, Tip("Copy image (Ctrl+C)", "Bild kopieren (Strg+C)"), (s, e) => CopyImage()),
                new ToolStripSeparator(), _tsUndo, _tsRedo, new ToolStripSeparator(),
                _toolButtons[CanvasTool.MaskPaint], _toolButtons[CanvasTool.Wand], _toolButtons[CanvasTool.Stamp], _toolButtons[CanvasTool.Select], _tsShape,
                _toolButtons[CanvasTool.Pencil], _toolButtons[CanvasTool.Eraser], _toolButtons[CanvasTool.Pipette], _tsColor,
                new ToolStripSeparator(), _tsBrush, _tsRound, _tsMirror, _tsShowMask, new ToolStripSeparator(),
                TextButton("Convert", Tip("Convert the mask (or the whole image) to particle gray (G)", "Maske (oder ganzes Bild) in Particle Gray umwandeln (G)"), (s, e) => ConvertGray(), true),
                _tsHueView, new ToolStripSeparator(),
                TextButton("–", Tip("Zoom out (Ctrl+-)", "Verkleinern (Strg+-)"), (s, e) => SetZoom(canvas.Zoom - 1), true), _tsZoom,
                TextButton("+", Tip("Zoom in (Ctrl++ or Ctrl+wheel)", "Vergrößern (Strg++ oder Strg+Mausrad)"), (s, e) => SetZoom(canvas.Zoom + 1), true),
                TextButton("Fit", Tip("Fit into the window (Ctrl+F)", "In das Fenster einpassen (Strg+F)"), (s, e) => FitZoom()),
                TextButton("?", Tip("Help (F1)", "Hilfe (F1)"), (s, e) => ShowHelp(), true)
            });
        }

        private void UpdateColorButton()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            using (var b = new SolidBrush(canvas.PrimaryColor))
            {
                g.FillRectangle(b, 0, 0, 16, 16);
                g.DrawRectangle(Pens.Gray, 0, 0, 15, 15);
            }
            _tsColor.Image?.Dispose();
            _tsColor.Image = bmp;
        }

        private void SetTool(CanvasTool tool)
        {
            canvas.Tool = tool;
            if (tool == CanvasTool.Stamp && canvas.StampShape == null)
            {
                UseSelectedStamp();
            }
            SyncUi();
        }

        private void SyncUi()
        {
            _tsUndo.Enabled = _undo.Count > 0;
            _tsRedo.Enabled = _redo.Count > 0;
            _tsRound.Text = canvas.RoundBrush ? "Round" : "Square";
            _tsMirror.Checked = canvas.MirrorDraw;
            _tsShowMask.Checked = canvas.ShowMask;
            _tsHueView.Checked = _hueOnCanvas;
            _tsZoom.Text = canvas.Zoom + "x";
            foreach (KeyValuePair<CanvasTool, ToolStripButton> kv in _toolButtons)
            {
                kv.Value.Checked = canvas.Tool == kv.Key;
            }
        }

        private void UpdateStatus()
        {
            if (_image == null)
            {
                statusInfo.Text = "no image";
                return;
            }
            int marked = _mask?.Count(m => m) ?? 0;
            ParticleGrayCore.LevelHistogram(_image, out int hued, out int total);
            statusInfo.Text = $"{_image.Width}×{_image.Height}   |   marked {marked} px   |   hued by UO {hued} of {total} px   |   zoom {canvas.Zoom}x";
        }

        #endregion

        #region Image, mask, undo

        private void PushUndo()
        {
            if (_image == null)
            {
                return;
            }
            _undo.Add((CutterImaging.To32Bpp(_image), (bool[])_mask?.Clone()));
            if (_undo.Count > MaxUndo)
            {
                _undo[0].Image.Dispose();
                _undo.RemoveAt(0);
            }
            foreach ((Bitmap img, bool[] _) in _redo)
            {
                img.Dispose();
            }
            _redo.Clear();
            SyncUi();
        }

        private void Undo()
        {
            if (_undo.Count == 0)
            {
                return;
            }
            _redo.Add((_image, _mask));
            (Bitmap img, bool[] mask) = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            SetState(img, mask);
            SetMessage($"Undo ({_undo.Count} more).");
        }

        private void Redo()
        {
            if (_redo.Count == 0)
            {
                return;
            }
            _undo.Add((_image, _mask));
            (Bitmap img, bool[] mask) = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            SetState(img, mask);
            SetMessage($"Redo ({_redo.Count} more).");
        }

        private void SetState(Bitmap img, bool[] mask)
        {
            _image = img;
            _mask = mask ?? new bool[img.Width * img.Height];
            canvas.Mask = _mask;
            ShowImageOnCanvas();
            canvas.InvalidateMask();
            ImageChanged();
        }

        /// <summary>Replaces the image with undo (the old one stays in the undo list).</summary>
        private void ApplyResult(Bitmap result, string message)
        {
            PushUndo();
            Bitmap old = _image;
            _image = result;
            ShowImageOnCanvas();
            if (old != null && !_undo.Any(u => ReferenceEquals(u.Image, old)) && !_redo.Any(u => ReferenceEquals(u.Image, old)))
            {
                old.Dispose();
            }
            ImageChanged();
            SetMessage(message);
        }

        private void LoadImage(Bitmap bmp, string name)
        {
            if (bmp == null)
            {
                return;
            }
            if (_image != null)
            {
                PushUndo();
            }
            _original?.Dispose();
            _original = CutterImaging.To32Bpp(bmp);
            Bitmap old = _image;
            _image = bmp;
            _mask = new bool[bmp.Width * bmp.Height];
            canvas.Mask = _mask;
            canvas.ClearSelection();
            ShowImageOnCanvas();
            old?.Dispose();
            canvas.InvalidateMask();
            FitZoom();
            ImageChanged();
            SetMessage($"Loaded {name} ({bmp.Width}×{bmp.Height}).");
        }

        private void ShowImageOnCanvas()
        {
            if (_hueOnCanvas)
            {
                _hueOnCanvas = false;
                SyncUi();
            }
            if (!ReferenceEquals(canvas.Image, _image))
            {
                canvas.Image = _image;
            }
            CenterCanvas();
        }

        private void ImageChanged()
        {
            UpdateStatus();
            SchedulePreview();
            _panelHistogram?.Invalidate();
            panelCanvas.Invalidate();
        }

        private void SetMask(bool[] mask, string message)
        {
            if (_image == null)
            {
                return;
            }
            PushUndo();
            _mask = mask;
            canvas.Mask = _mask;
            canvas.InvalidateMask();
            UpdateStatus();
            SetMessage(message);
        }

        #endregion

        #region Canvas, zoom, view

        private void Canvas_HoverPixel(object sender, Point p)
        {
            if (p.X < 0 || _image == null || p.X >= _image.Width || p.Y >= _image.Height)
            {
                statusPos.Text = statusColor.Text = string.Empty;
                statusColor.BackColor = SystemColors.Control;
                return;
            }
            int px = _image.GetPixel(p.X, p.Y).ToArgb();
            Color c = Color.FromArgb(px);
            statusPos.Text = $"X {p.X}  Y {p.Y}";
            string hued = ParticleGrayCore.IsHued(px) ? $"  gray level {ParticleGrayCore.GrayLevel(px)} → hued" : (c.A == 0 ? string.Empty : "  not hued");
            statusColor.Text = c.A == 0 ? "transparent" : $"{Hex(c)}{hued}";
            statusColor.BackColor = c.A == 0 ? SystemColors.Control : c;
            statusColor.ForeColor = c.A == 0 || c.R * 0.299 + c.G * 0.587 + c.B * 0.114 > 140 ? Color.Black : Color.White;
        }

        private void Canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {
                return;
            }
            if (e is HandledMouseEventArgs h)
            {
                h.Handled = true;
            }
            SetZoom(canvas.Zoom + (e.Delta > 0 ? 1 : -1));
        }

        private void SetZoom(int zoom)
        {
            canvas.Zoom = zoom;
            CenterCanvas();
            SyncUi();
            UpdateStatus();
        }

        private void FitZoom()
        {
            if (_image == null)
            {
                return;
            }
            Size c = panelCanvas.ClientSize;
            SetZoom(Math.Max(1, Math.Min(16, Math.Min((c.Width - 10) / _image.Width, (c.Height - 10) / _image.Height))));
        }

        private void CenterCanvas()
        {
            Size c = panelCanvas.ClientSize;
            Point s = panelCanvas.AutoScrollPosition;
            canvas.Location = new Point(Math.Max(0, (c.Width - canvas.Width) / 2) + s.X, Math.Max(0, (c.Height - canvas.Height) / 2) + s.Y);
        }

        private void PanelCanvas_Resize(object sender, EventArgs e) => CenterCanvas();

        private void PanelCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (_image != null)
            {
                return;
            }
            const string hint = "No image.\nOpen one (Ctrl+O), paste one (Ctrl+V) or load an item / land tile in the Art tab.\n\n"
                                + "Kein Bild.\nÖffnen (Strg+O), einfügen (Strg+V) oder ein Item / eine Landkachel im Reiter Art laden.";
            TextRenderer.DrawText(e.Graphics, hint, Font, panelCanvas.ClientRectangle, Color.Gainsboro,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private void ToggleHueView()
        {
            if (_image == null)
            {
                return;
            }
            if (_hueOnCanvas)
            {
                ShowImageOnCanvas();
                return;
            }
            UpdatePreviewNow();
            if (_huePreview != null)
            {
                _hueOnCanvas = true;
                canvas.Image = _huePreview;
                SyncUi();
                SetMessage($"Hue view: hue {(int)_numHue.Value} \"{_hueName}\". Any edit switches back to the image.");
            }
        }

        #endregion

        #region File, clipboard

        private void OpenImage()
        {
            using (var d = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Title = "Open image" })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    LoadImage(CutterImaging.LoadBitmap(d.FileName), Path.GetFileName(d.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The image could not be loaded:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PasteImage()
        {
            try
            {
                Bitmap bmp = CutterImaging.GetClipboardImage();
                if (bmp == null)
                {
                    SetMessage("The clipboard contains no image.");
                    return;
                }
                LoadImage(bmp, "clipboard image");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The clipboard could not be read:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveImage()
        {
            if (!CheckImage())
            {
                return;
            }
            using (var d = new SaveFileDialog { Filter = "PNG|*.png|BMP|*.bmp|TIFF|*.tiff", FileName = "ParticleGray_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    CutterImaging.SaveImage(_image, d.FileName, TransparentFill.Keep);
                    SetMessage("Saved: " + d.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CopyImage()
        {
            if (!CheckImage())
            {
                return;
            }
            try
            {
                CutterImaging.CopyToClipboard(_hueOnCanvas && _huePreview != null ? _huePreview : _image);
                SetMessage(_hueOnCanvas ? "Hue view copied to the clipboard." : "Image copied to the clipboard.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Copy failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Mask tab

        private NumericUpDown _numTolerance, _numStampSize;
        private ArtTileGrid _gridStamps;
        private TextBox _txtStampName;

        private void BuildMaskTab()
        {
            _numTolerance = Num(0, 255, 20);
            _numTolerance.ValueChanged += (s, e) => canvas.FillTolerance = (int)_numTolerance.Value;
            toolTip1.SetToolTip(_numTolerance, Tip("How different a color may be for the magic wand (0 = exact).", "Wie stark eine Farbe für den Zauberstab abweichen darf (0 = genau)."));
            var maskColor = Btn("Mask color...", (s, e) =>
            {
                using (var d = new ColorDialog { Color = Color.FromArgb(255, canvas.MaskColor), FullOpen = true })
                {
                    if (d.ShowDialog(this) == DialogResult.OK)
                    {
                        canvas.MaskColor = Color.FromArgb(120, d.Color);
                        canvas.InvalidateMask();
                    }
                }
            });
            GroupBox mask = Group("Mask (red = will be changed)",
                Row(Btn("Selection → mask +", (s, e) => SelectionToMask(true), Tip("Adds the selection to the mask.", "Fügt die Auswahl zur Maske hinzu.")),
                    Btn("Selection → mask −", (s, e) => SelectionToMask(false), Tip("Removes the selection from the mask (old \"Correction mask\").", "Entfernt die Auswahl aus der Maske (früher \"Correction mask\")."))),
                Row(Btn("All", (s, e) => MaskOp(m => m.Select(_ => true).ToArray(), "Everything marked.")),
                    Btn("Clear", (s, e) => MaskOp(m => new bool[m.Length], "Mask cleared.")),
                    Btn("Invert", (s, e) => MaskOp(m => m.Select(v => !v).ToArray(), "Mask inverted.")),
                    Btn("Grow", (s, e) => MaskOp(m => ParticleGrayCore.Grow(m, _image.Width, _image.Height, true), "Mask grown by 1 px.")),
                    Btn("Shrink", (s, e) => MaskOp(m => ParticleGrayCore.Grow(m, _image.Width, _image.Height, false), "Mask shrunk by 1 px."))),
                Row(Btn("Mark colored pixels", (s, e) => MaskWhere(ParticleGrayCore.IsColored, "Colored pixels marked.")),
                    Btn("Mark hued (gray) pixels", (s, e) => MaskWhere(ParticleGrayCore.IsHued, "Pixels that UO hues are marked."))),
                Row(Btn("Mark paint color", (s, e) => MarkColor(canvas.PrimaryColor), Tip("All pixels similar to the paint color (tolerance below). Pick it with the pipette.",
                    "Alle Pixel ähnlich der Malfarbe (Toleranz unten). Mit der Pipette aufnehmen.")), Lbl("Tolerance:"), _numTolerance),
                Row(maskColor));

            _gridStamps = new ArtTileGrid { Width = 330, Height = 170, CellSize = new Size(64, 74), Text = "No stamps." };
            _gridStamps.ImageProvider = i => i >= 0 && i < _stamps.Count ? _stamps[i].Preview : null;
            _gridStamps.LabelProvider = i => i >= 0 && i < _stamps.Count ? _stamps[i].Name : string.Empty;
            _gridStamps.SelectionChanged += (s, e) => UseSelectedStamp();
            _gridStamps.ItemActivated += (s, e) => SetTool(CanvasTool.Stamp);
            _numStampSize = Num(2, 128, 12);
            _numStampSize.ValueChanged += (s, e) => { ReloadStamps(); UseSelectedStamp(); };
            _txtStampName = new TextBox { Width = 110, Text = "My stamp", Margin = new Padding(3, 4, 3, 0) };
            GroupBox stamps = Group("Stamps (shapes for the Stamp tool)",
                _gridStamps,
                Row(Lbl("Size (built-in):"), _numStampSize, Btn("Use", (s, e) => SetTool(CanvasTool.Stamp))),
                Row(Btn("Mask → new stamp", (s, e) => MaskToStamp(), Tip("Saves the marked area as own stamp shape.", "Speichert den markierten Bereich als eigene Stempelform.")), _txtStampName),
                Row(Btn("Delete stamp", (s, e) => DeleteStamp()), Btn("Open folder", (s, e) => OpenFolder(ParticleGrayCore.StampFolder))));

            FillTab(tabMask, mask, stamps);
            ReloadStamps();
        }

        private void SelectionToMask(bool add)
        {
            if (!CheckImage())
            {
                return;
            }
            using (GraphicsPath path = canvas.GetSelectionPath())
            {
                if (path == null)
                {
                    SetMessage("Make a selection first (tool Select).");
                    return;
                }
                int w = _image.Width, h = _image.Height;
                var m = (bool[])_mask.Clone();
                using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Transparent);
                        g.FillPath(Brushes.White, path);
                    }
                    int[] px = CutterImaging.GetPixels(bmp);
                    for (int i = 0; i < px.Length; i++)
                    {
                        if ((px[i] >> 24) != 0)
                        {
                            m[i] = add;
                        }
                    }
                }
                SetMask(m, add ? "Selection added to the mask." : "Selection removed from the mask.");
                canvas.ClearSelection();
            }
        }

        private void MaskOp(Func<bool[], bool[]> op, string message)
        {
            if (CheckImage())
            {
                SetMask(op(_mask), message);
            }
        }

        private void MaskWhere(Func<int, bool> test, string message)
        {
            if (CheckImage())
            {
                SetMask(ParticleGrayCore.MaskWhere(_image, test), message);
            }
        }

        private void MarkColor(Color c)
        {
            int tol = (int)_numTolerance.Value;
            MaskWhere(p => ((p >> 24) & 0xFF) >= 128 && Math.Abs(((p >> 16) & 0xFF) - c.R) <= tol && Math.Abs(((p >> 8) & 0xFF) - c.G) <= tol && Math.Abs((p & 0xFF) - c.B) <= tol,
                $"Pixels like {Hex(c)} marked.");
        }

        private void ReloadStamps()
        {
            int keep = _gridStamps.SelectedItem;
            foreach ((string _, bool[,] _, Bitmap preview, bool _) in _stamps)
            {
                preview?.Dispose();
            }
            _stamps.Clear();
            int size = (int)_numStampSize.Value;
            for (int i = 0; i < ParticleGrayCore.ShapeNames.Length; i++)
            {
                bool[,] shape = ParticleGrayCore.MakeShape(i, size);
                _stamps.Add((ParticleGrayCore.ShapeNames[i], shape, ParticleGrayCore.ShapeToBitmap(ParticleGrayCore.MakeShape(i, 32)), true));
            }
            foreach ((string name, bool[,] shape, Bitmap preview) in ParticleGrayCore.LoadStamps())
            {
                _stamps.Add((name, shape, preview, false));
            }
            _gridStamps.Items = Enumerable.Range(0, _stamps.Count).ToList();
            _gridStamps.SelectedIndex = Math.Max(0, Math.Min(_stamps.Count - 1, keep));
        }

        private void UseSelectedStamp()
        {
            int i = _gridStamps?.SelectedItem ?? -1;
            if (i >= 0 && i < _stamps.Count)
            {
                canvas.StampShape = _stamps[i].Shape;
            }
        }

        private void MaskToStamp()
        {
            if (!CheckImage())
            {
                return;
            }
            bool[,] shape = ParticleGrayCore.ShapeFromMask(_mask, _image.Width, _image.Height);
            if (shape == null)
            {
                SetMessage("Mark an area first – it becomes the stamp.");
                return;
            }
            try
            {
                string file = ParticleGrayCore.SaveStamp(shape, _txtStampName.Text);
                ReloadStamps();
                _gridStamps.SelectedIndex = _stamps.Count - 1;
                SetMessage($"Stamp saved ({shape.GetLength(0)}×{shape.GetLength(1)}): {file}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The stamp could not be saved:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteStamp()
        {
            int i = _gridStamps.SelectedItem;
            if (i < 0 || _stamps[i].BuiltIn)
            {
                SetMessage("Built-in stamps cannot be deleted.");
                return;
            }
            string file = Path.Combine(ParticleGrayCore.StampFolder, _stamps[i].Name + ".png");
            if (MessageBox.Show(this, $"Delete the stamp \"{_stamps[i].Name}\"?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                File.Delete(file);
                ReloadStamps();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void OpenFolder(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
            }
            catch (Exception)
            {
                // explorer missing
            }
        }

        #endregion

        #region Gray tab

        private ComboBox _comboMode;
        private CheckBox _chkStretch, _chkAvoidBlack;
        private NumericUpDown _numBright, _numContrast;
        private Button _btnDye;
        private TextBox _txtDye;

        private void BuildGrayTab()
        {
            _comboMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            _comboMode.Items.AddRange(new object[] { "Luminance (natural)", "Average", "Lightest channel", "Darkest channel" });
            _comboMode.SelectedIndex = 0;
            _chkStretch = Chk("Use all 32 gray levels (stretch)", true);
            _chkAvoidBlack = Chk("Avoid black (black is transparent in UO)", true);
            _numBright = Num(-50, 50, 0);
            _numContrast = Num(-50, 50, 0);
            toolTip1.SetToolTip(_comboMode, Tip("How a color becomes a gray level. Luminance looks most natural.", "Wie aus einer Farbe eine Graustufe wird. Luminance wirkt am natürlichsten."));
            toolTip1.SetToolTip(_chkStretch, Tip("Spreads the marked area over all 32 levels – the hue shows all its colors.", "Verteilt den markierten Bereich auf alle 32 Stufen – der Hue zeigt alle seine Farben."));

            GroupBox convert = Group("Convert to particle gray (exact 15 bit gray like UO)",
                Row(Lbl("Mode:", 75), _comboMode),
                _chkStretch, _chkAvoidBlack,
                Row(Lbl("Brightness:", 75), _numBright, Lbl("Contrast:"), _numContrast),
                Row(Btn("Convert mask → gray", (s, e) => ConvertGray(), Tip("Only the marked pixels; without mask the whole image.", "Nur die markierten Pixel; ohne Maske das ganze Bild."))),
                Row(Btn("Restore original colors in mask", (s, e) => RestoreColors(), Tip("Puts the colors of the loaded image back (old \"Restore Color\").",
                    "Setzt die Farben des geladenen Bildes zurück (früher \"Restore Color\")."))),
                Row(Btn("Protect colors outside the mask", (s, e) => FixAccidental(), Tip(
                    "Colored areas can contain pixels that are exactly gray by chance – UO would hue them too. They get a tiny blue shift (invisible) so only the mask is hued.",
                    "Farbige Bereiche können zufällig exakt graue Pixel enthalten – UO würde sie mit einfärben. Sie bekommen eine winzige Blauverschiebung (unsichtbar), damit nur die Maske eingefärbt wird."))));

            _btnDye = new Button { Width = 90, Height = 26, FlatStyle = FlatStyle.Flat, Margin = new Padding(2) };
            _btnDye.Click += (s, e) =>
            {
                using (var d = new ColorDialog { Color = _dyeColor, FullOpen = true })
                {
                    if (d.ShowDialog(this) == DialogResult.OK)
                    {
                        SetDyeColor(d.Color);
                    }
                }
            };
            _txtDye = new TextBox { Width = 70, Margin = new Padding(3, 4, 3, 0) };
            _txtDye.TextChanged += (s, e) =>
            {
                if (_updating)
                {
                    return;
                }
                string t = _txtDye.Text.Trim().TrimStart('#');
                if (t.Length == 6 && int.TryParse(t, NumberStyles.HexNumber, null, out int rgb))
                {
                    _txtDye.BackColor = SystemColors.Window;
                    SetDyeColor(Color.FromArgb(255, Color.FromArgb(rgb)));
                }
                else
                {
                    _txtDye.BackColor = Color.MistyRose;
                }
            };
            SetDyeColor(_dyeColor);
            GroupBox dye = Group("Dyeing (old \"Dyeing in\")",
                Row(Lbl("Color:"), _btnDye, _txtDye, Btn("RGB mixer", (s, e) => OpenMixer())),
                Row(Btn("Dye gray pixels (mask or all)", (s, e) => Dye(), Tip("Gray pixels get the color, their brightness stays. With mask only inside it.",
                    "Graue Pixel bekommen die Farbe, ihre Helligkeit bleibt. Mit Maske nur darin."))));

            FillTab(tabGray, convert, dye);
        }

        private GraySettings ReadGraySettings() => new GraySettings
        {
            Mode = (GrayMode)_comboMode.SelectedIndex,
            Stretch = _chkStretch.Checked,
            Brightness = (int)_numBright.Value,
            Contrast = (int)_numContrast.Value,
            AvoidBlack = _chkAvoidBlack.Checked
        };

        private bool[] MaskOrAll(string action)
        {
            if (HasMask)
            {
                return _mask;
            }
            return MessageBox.Show(this, $"Nothing is marked. {action} the whole image?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes
                ? null
                : Array.Empty<bool>();
        }

        private void ConvertGray()
        {
            if (!CheckImage())
            {
                return;
            }
            bool[] mask = MaskOrAll("Convert");
            if (mask != null && mask.Length == 0)
            {
                return;
            }
            Bitmap result = ParticleGrayCore.Convert(_image, mask, ReadGraySettings(), out int count);
            ApplyResult(result, $"{count} pixels converted to particle gray – UO hues them now.");
        }

        private void RestoreColors()
        {
            if (!CheckImage() || _original == null)
            {
                return;
            }
            bool[] mask = MaskOrAll("Restore");
            if (mask != null && mask.Length == 0)
            {
                return;
            }
            ApplyResult(ParticleGrayCore.Restore(_image, _original, mask), "Original colors restored.");
        }

        private void FixAccidental()
        {
            if (!CheckImage())
            {
                return;
            }
            Bitmap result = ParticleGrayCore.FixAccidentalGrays(_image, HasMask ? _mask : null, out int count);
            if (count == 0)
            {
                result.Dispose();
                SetMessage("No accidental gray pixels found outside the mask.");
                return;
            }
            ApplyResult(result, $"{count} gray pixels outside the mask protected – UO will not hue them.");
        }

        private void SetDyeColor(Color c)
        {
            _dyeColor = c;
            _btnDye.BackColor = c;
            _btnDye.ForeColor = c.R * 0.299 + c.G * 0.587 + c.B * 0.114 > 140 ? Color.Black : Color.White;
            _btnDye.Text = Hex(c);
            if (!string.Equals(_txtDye.Text.TrimStart('#'), Hex(c).TrimStart('#'), StringComparison.OrdinalIgnoreCase))
            {
                _updating = true;
                _txtDye.Text = Hex(c);
                _updating = false;
            }
        }

        private void Dye()
        {
            if (!CheckImage())
            {
                return;
            }
            Bitmap result = ParticleGrayCore.Colorize(_image, HasMask ? _mask : null, _dyeColor, out int count);
            ApplyResult(result, $"{count} gray pixels dyed with {Hex(_dyeColor)}.");
        }

        private void OpenMixer()
        {
            using (var mixer = new FormRgbMixer(_dyeColor) { TopMost = TopMost })
            {
                if (mixer.ShowDialog(this) == DialogResult.OK)
                {
                    SetDyeColor(mixer.SelectedColor);
                }
            }
        }

        /// <summary>RGB color mixer (kept from the old version, now with a start color).</summary>
        public class FormRgbMixer : Form
        {
            private readonly TrackBar _r, _g, _b;
            private readonly Label _lr, _lg, _lb;
            private readonly Panel _preview;
            private readonly TextBox _hex;

            public FormRgbMixer() : this(Color.Black)
            {
            }

            public FormRgbMixer(Color start)
            {
                Text = "RGB color mixer";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = MinimizeBox = false;
                ShowIcon = false;
                ClientSize = new Size(300, 290);
                StartPosition = FormStartPosition.CenterParent;
                TrackBar Bar(int top, int value) => new TrackBar { Minimum = 0, Maximum = 255, TickFrequency = 16, Left = 20, Top = top, Width = 200, Value = value };
                _r = Bar(20, start.R);
                _g = Bar(70, start.G);
                _b = Bar(120, start.B);
                _lr = new Label { Left = 230, Top = 25, Width = 60 };
                _lg = new Label { Left = 230, Top = 75, Width = 60 };
                _lb = new Label { Left = 230, Top = 125, Width = 60 };
                _preview = new Panel { Left = 20, Top = 175, Width = 100, Height = 50, BorderStyle = BorderStyle.FixedSingle };
                _hex = new TextBox { Left = 130, Top = 190, Width = 130, ReadOnly = true };
                var ok = new Button { Text = "OK", Left = 60, Top = 245, Width = 80, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 150, Top = 245, Width = 80, DialogResult = DialogResult.Cancel };
                AcceptButton = ok;
                CancelButton = cancel;
                foreach (TrackBar t in new[] { _r, _g, _b })
                {
                    t.ValueChanged += (s, e) => UpdateColor();
                }
                Controls.AddRange(new Control[] { _r, _g, _b, _lr, _lg, _lb, _preview, _hex, ok, cancel });
                UpdateColor();
            }

            public Color SelectedColor => Color.FromArgb(_r.Value, _g.Value, _b.Value);

            public string SelectedHexColor => _hex.Text;

            private void UpdateColor()
            {
                _lr.Text = "R " + _r.Value;
                _lg.Text = "G " + _g.Value;
                _lb.Text = "B " + _b.Value;
                _preview.BackColor = SelectedColor;
                _hex.Text = $"#{_r.Value:X2}{_g.Value:X2}{_b.Value:X2}";
            }
        }

        #endregion

        #region Hue tab

        private NumericUpDown _numHue;
        private Label _lblHue;
        private Panel _panelHueStrip, _panelHuePreview;
        private ComboBox _comboPreview;
        private CheckBox _chkHueAll;

        private void BuildHueTab()
        {
            _numHue = Num(1, 3000, 1, 70);
            _numHue.ValueChanged += (s, e) => LoadHue((int)_numHue.Value);
            _lblHue = new Label { AutoSize = true, Margin = new Padding(3, 7, 3, 0) };
            _panelHueStrip = new Panel { Width = 320, Height = 24, Margin = new Padding(3) };
            _panelHueStrip.Paint += (s, e) =>
            {
                if (_hueColors == null)
                {
                    return;
                }
                float w = _panelHueStrip.Width / 32f;
                for (int i = 0; i < 32; i++)
                {
                    int c = _hueColors[i];
                    int r = (c >> 10) & 0x1F, g = (c >> 5) & 0x1F, b = c & 0x1F;
                    using (var brush = new SolidBrush(Color.FromArgb(r * 255 / 31, g * 255 / 31, b * 255 / 31)))
                    {
                        e.Graphics.FillRectangle(brush, i * w, 0, w + 1, _panelHueStrip.Height);
                    }
                }
            };
            toolTip1.SetToolTip(_panelHueStrip, Tip("The 32 colors of the hue: gray level 0 (left) … 31 (right).", "Die 32 Farben des Hues: Graustufe 0 (links) … 31 (rechts)."));
            _chkHueAll = Chk("Hue all pixels (not only gray – like items without \"partial hue\")");
            _chkHueAll.CheckedChanged += (s, e) => SchedulePreview();
            _comboPreview = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
            _comboPreview.Items.AddRange(new object[] { "With hue", "Hued pixels (magenta)", "Image" });
            _comboPreview.SelectedIndex = 0;
            _comboPreview.SelectedIndexChanged += (s, e) => SchedulePreview();
            _panelHuePreview = new Panel { Width = 330, Height = 330, BackColor = Color.FromArgb(45, 45, 48), Margin = new Padding(3) };
            _panelHuePreview.Paint += (s, e) =>
            {
                Bitmap shown = _comboPreview.SelectedIndex == 2 ? _image : _huePreview;
                if (shown == null)
                {
                    return;
                }
                Rectangle box = Rectangle.Inflate(_panelHuePreview.ClientRectangle, -6, -6);
                double scale = Math.Min(8, Math.Min(box.Width / (double)shown.Width, box.Height / (double)shown.Height));
                var dest = new Rectangle(box.X + (box.Width - (int)(shown.Width * scale)) / 2, box.Y + (box.Height - (int)(shown.Height * scale)) / 2, (int)(shown.Width * scale), (int)(shown.Height * scale));
                CutterImaging.DrawChecker(e.Graphics, dest);
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.DrawImage(shown, dest);
            };
            GroupBox hue = Group("UO hue (hues.mul)",
                Row(Lbl("Hue:"), _numHue, Btn("◀", (s, e) => StepHue(-1)), Btn("▶", (s, e) => StepHue(1)), Btn("Random", (s, e) => StepHue(0))),
                _lblHue, _panelHueStrip, _chkHueAll);
            GroupBox preview = Group("Preview",
                Row(_comboPreview, Btn("On canvas (H)", (s, e) => ToggleHueView())),
                _panelHuePreview,
                Row(Btn("Copy preview", (s, e) => { if (_huePreview != null) CutterImaging.CopyToClipboard(_huePreview); }),
                    Btn("Apply hue to image", (s, e) => ApplyHueToImage(), Tip("Burns the hue colors into the image (e.g. to save a colored version).",
                        "Brennt die Hue-Farben ins Bild ein (z. B. um eine gefärbte Version zu speichern)."))));
            FillTab(tabHue, hue, preview);
        }

        private void LoadHue(int number)
        {
            int count = ParticleGrayCore.HueCount;
            if (count > 0 && _numHue.Maximum != count)
            {
                _numHue.Maximum = count;
            }
            _hueColors = ParticleGrayCore.HueColors(number, out _hueName);
            _lblHue.Text = _hueColors == null ? "hues.mul is not loaded – no hue preview." : $"Hue {number} (0x{number:X4}): {_hueName}";
            _panelHueStrip.Invalidate();
            SchedulePreview();
        }

        private void StepHue(int delta)
        {
            int max = (int)_numHue.Maximum;
            int v = delta == 0 ? new Random().Next(1, max + 1) : (int)_numHue.Value + delta;
            _numHue.Value = Math.Max(1, Math.Min(max, v < 1 ? max : v > max ? 1 : v));
        }

        private void SchedulePreview()
        {
            previewTimer.Stop();
            previewTimer.Start();
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            previewTimer.Stop();
            UpdatePreviewNow();
        }

        private void UpdatePreviewNow()
        {
            Bitmap old = _huePreview;
            _huePreview = null;
            if (_image != null)
            {
                _huePreview = _comboPreview.SelectedIndex == 1
                    ? ParticleGrayCore.HighlightHued(_image)
                    : ParticleGrayCore.ApplyHue(_image, _hueColors, !_chkHueAll.Checked);
            }
            if (_hueOnCanvas)
            {
                if (_huePreview != null)
                {
                    canvas.Image = _huePreview;
                }
                else
                {
                    ShowImageOnCanvas();
                }
            }
            old?.Dispose();
            _panelHuePreview.Invalidate();
        }

        private void ApplyHueToImage()
        {
            if (!CheckImage() || _hueColors == null)
            {
                return;
            }
            ApplyResult(ParticleGrayCore.ApplyHue(_image, _hueColors, !_chkHueAll.Checked), $"Hue {(int)_numHue.Value} applied to the image.");
        }

        #endregion

        #region Art tab

        private RadioButton _radioLand;
        private ComboBox _comboArtFilter;
        private TextBox _txtArtId;
        private ArtTileGrid _gridArt;
        private Label _lblArt;
        private CheckBox _chkPartialHue;

        private ArtKind Kind => _radioLand.Checked ? ArtKind.Land : ArtKind.Item;

        private void BuildArtTab()
        {
            var items = new RadioButton { Text = "Items", AutoSize = true, Checked = true, Margin = new Padding(3, 6, 3, 0) };
            _radioLand = new RadioButton { Text = "Land tiles", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
            items.CheckedChanged += (s, e) => { if (items.Checked) ReloadArt(); };
            _radioLand.CheckedChanged += (s, e) => { if (_radioLand.Checked) ReloadArt(); };
            _comboArtFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
            _comboArtFilter.Items.AddRange(new object[] { "All", "Used", "Free" });
            _comboArtFilter.SelectedIndex = 1;
            _comboArtFilter.SelectedIndexChanged += (s, e) => RebuildArtList();
            _txtArtId = new TextBox { Width = 70, Margin = new Padding(3, 4, 3, 0) };
            _txtArtId.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; GoToArt(); } };
            _gridArt = new ArtTileGrid { Width = 330, Height = 320, Text = "art.mul is not loaded." };
            _gridArt.ImageProvider = id => CutterArt.GetShared(Kind, id);
            _gridArt.LabelProvider = id => $"0x{id:X4}";
            _gridArt.IsMarked = id => _usedArt != null && id < _usedArt.Length && !_usedArt[id];
            _gridArt.SelectionChanged += (s, e) => UpdateArtInfo();
            _gridArt.ItemActivated += (s, e) => ArtToEditor();
            _lblArt = new Label { Width = 330, Height = 34 };
            _chkPartialHue = Chk("Set \"Partial Hue\" flag in tiledata (items)", true);
            toolTip1.SetToolTip(_chkPartialHue, Tip("UO only hues the gray pixels of items with this flag. Save tiledata.mul in UoFiddler afterwards.",
                "UO färbt nur bei Items mit diesem Flag ausschließlich die grauen Pixel. Danach tiledata.mul in UoFiddler speichern."));
            GroupBox art = Group("art.mul",
                Row(items, _radioLand, _comboArtFilter),
                Row(Lbl("ID:"), _txtArtId, Btn("Go", (s, e) => GoToArt()), Btn("Next free", (s, e) => NextFreeArt())),
                _gridArt, _lblArt,
                Row(Btn("→ Editor", (s, e) => ArtToEditor(), Tip("Load the selected art (double click).", "Gewählte Grafik laden (Doppelklick).")),
                    Btn("Image → ID", (s, e) => ImageToArt(), Tip("Write the edited image into the selected ID.", "Bearbeitetes Bild in die gewählte ID schreiben."))),
                _chkPartialHue,
                Row(Btn("Save art.mul", (s, e) => SaveArt())));
            FillTab(tabArt, art);
        }

        private void ReloadArt()
        {
            if (!CutterArt.IsAvailable)
            {
                _usedArt = null;
                _gridArt.Items = Array.Empty<int>();
                _lblArt.Text = "art.mul is not loaded.";
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            _usedArt = CutterArt.ScanUsed(Kind);
            Cursor.Current = Cursors.Default;
            RebuildArtList();
        }

        private void RebuildArtList()
        {
            if (_usedArt == null)
            {
                return;
            }
            int keep = _gridArt.SelectedItem;
            var list = new List<int>();
            for (int i = 0; i < _usedArt.Length; i++)
            {
                if (_comboArtFilter.SelectedIndex == 0 || (_comboArtFilter.SelectedIndex == 1 && _usedArt[i]) || (_comboArtFilter.SelectedIndex == 2 && !_usedArt[i]))
                {
                    list.Add(i);
                }
            }
            _gridArt.Items = list;
            if (keep >= 0)
            {
                _gridArt.SelectItem(keep);
            }
            UpdateArtInfo();
        }

        private void UpdateArtInfo()
        {
            int id = _gridArt.SelectedItem;
            if (id < 0)
            {
                _lblArt.Text = $"{_gridArt.Items.Count} entries";
                return;
            }
            Bitmap bmp = CutterArt.GetShared(Kind, id);
            string name = string.Empty, flag = string.Empty;
            try
            {
                lock (SdkSync.Lock)
                {
                    if (Kind == ArtKind.Item && id < TileData.ItemTable.Length)
                    {
                        name = TileData.ItemTable[id].Name?.Trim();
                        flag = (TileData.ItemTable[id].Flags & TileFlag.PartialHue) != 0 ? ", partial hue" : ", no partial hue";
                    }
                    else if (Kind == ArtKind.Land)
                    {
                        name = TileData.LandTable[id].Name?.Trim();
                    }
                }
            }
            catch (Exception)
            {
                // tiledata missing
            }
            _lblArt.Text = $"{Kind} 0x{id:X4} \"{name}\"" + (bmp == null ? " – free" : $" – {bmp.Width}×{bmp.Height}{flag}");
            _txtArtId.Text = $"0x{id:X4}";
        }

        private void GoToArt()
        {
            if (!CutterImaging.TryParseId(_txtArtId.Text, out int id))
            {
                SetMessage("Invalid ID.");
                return;
            }
            if (!_gridArt.Items.Contains(id))
            {
                _comboArtFilter.SelectedIndex = 0;
            }
            _gridArt.SelectItem(id);
        }

        private void NextFreeArt()
        {
            int id = CutterArt.FindFree(Kind, Math.Max(0, _gridArt.SelectedItem + 1));
            if (id < 0)
            {
                SetMessage("No free ID found.");
                return;
            }
            if (!_gridArt.Items.Contains(id))
            {
                _comboArtFilter.SelectedIndex = 0;
            }
            _gridArt.SelectItem(id);
        }

        private void ArtToEditor()
        {
            int id = _gridArt.SelectedItem;
            Bitmap bmp = id >= 0 ? CutterArt.GetCopy(Kind, id) : null;
            if (bmp == null)
            {
                SetMessage("Select a used ID first.");
                return;
            }
            LoadImage(bmp, $"{Kind.ToString().ToLowerInvariant()} 0x{id:X4}");
        }

        private void ImageToArt()
        {
            if (!CheckImage() || !CutterArt.IsAvailable)
            {
                return;
            }
            int id = _gridArt.SelectedItem;
            if (id < 0)
            {
                SetMessage("Select an ID first.");
                return;
            }
            if (Kind == ArtKind.Land && (_image.Width != CutterArt.LandSize || _image.Height != CutterArt.LandSize))
            {
                MessageBox.Show(this, "Land tiles must be 44 × 44.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (CutterArt.IsValid(Kind, id) &&
                MessageBox.Show(this, $"{Kind} 0x{id:X4} already contains a graphic. Replace it?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                CutterArt.Write(Kind, id, _image, this);
                if (_usedArt != null && id < _usedArt.Length)
                {
                    _usedArt[id] = true;
                }
                string extra = string.Empty;
                if (Kind == ArtKind.Item && _chkPartialHue.Checked)
                {
                    lock (SdkSync.Lock)
                    {
                        if (id < TileData.ItemTable.Length)
                        {
                            TileData.ItemTable[id].Flags |= TileFlag.PartialHue;
                        }
                    }
                    UoFiddler.Controls.Classes.Options.ChangedUltimaClass["TileData"] = true;
                    extra = " Partial hue flag set (save tiledata.mul in UoFiddler).";
                }
                _gridArt.Invalidate();
                UpdateArtInfo();
                SetMessage($"Written to {Kind} 0x{id:X4}. Save art.mul to keep it.{extra}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Writing failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveArt()
        {
            if (MessageBox.Show(this, $"Save artidx.mul and art.mul into:\n{UoFiddler.Controls.Classes.Options.OutputPath}", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }
            try
            {
                SetMessage("art.mul saved in " + CutterArt.Save());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Analysis tab

        private Panel _panelHistogram;

        private void BuildAnalysisTab()
        {
            _panelHistogram = new Panel { Width = 330, Height = 160, BackColor = Color.FromArgb(45, 45, 48), Margin = new Padding(3) };
            _panelHistogram.Paint += PanelHistogram_Paint;
            toolTip1.SetToolTip(_panelHistogram, Tip("How many pixels use each of the 32 gray levels. Gaps = hue colors that never appear.",
                "Wie viele Pixel jede der 32 Graustufen benutzen. Lücken = Hue-Farben, die nie vorkommen."));
            GroupBox hist = Group("Gray levels used by hued pixels (0 … 31)", _panelHistogram);
            GroupBox tools = Group("Analysis",
                Row(Btn("Pixel analysis (zoomed list)", (s, e) => PixelAnalysis(), Tip("Every pixel with color and hex code; click a pixel to find it and copy its code.",
                    "Jeder Pixel mit Farbe und Hex-Code; Pixel anklicken = finden und Code kopieren."))),
                Row(Btn("Gray pixel list", (s, e) => { if (CheckImage()) GrayPixelAnalyzer.ShowInForm(CutterImaging.To32Bpp(_image), $"Analysis from {DateTime.Now:G}"); }),
                    Btn("Save gray pixel list...", (s, e) => SaveGrayList())));
            FillTab(tabAnalysis, hist, tools);
        }

        private void PanelHistogram_Paint(object sender, PaintEventArgs e)
        {
            if (_image == null)
            {
                return;
            }
            int[] hist = ParticleGrayCore.LevelHistogram(_image, out int hued, out int total);
            int max = Math.Max(1, hist.Max());
            Rectangle r = Rectangle.Inflate(_panelHistogram.ClientRectangle, -6, -6);
            r.Height -= 16;
            float w = r.Width / 32f;
            for (int i = 0; i < 32; i++)
            {
                int h = (int)(r.Height * hist[i] / (double)max);
                Color c = _hueColors != null
                    ? Color.FromArgb(((_hueColors[i] >> 10) & 0x1F) * 255 / 31, ((_hueColors[i] >> 5) & 0x1F) * 255 / 31, (_hueColors[i] & 0x1F) * 255 / 31)
                    : Color.FromArgb(i * 8, i * 8, i * 8);
                using (var b = new SolidBrush(c))
                {
                    e.Graphics.FillRectangle(b, r.X + i * w, r.Bottom - h, Math.Max(1, w - 1), h);
                }
            }
            TextRenderer.DrawText(e.Graphics, $"{hued} of {total} pixels are hued by UO", Font, new Rectangle(r.X, r.Bottom + 2, r.Width, 16), Color.Gainsboro);
        }

        private void SaveGrayList()
        {
            if (!CheckImage())
            {
                return;
            }
            using (var d = new SaveFileDialog { Filter = "Text files (*.txt)|*.txt", FileName = "Graypixel_analysis.txt" })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    using (Bitmap copy = CutterImaging.To32Bpp(_image))
                    {
                        GrayPixelAnalyzer.AnalyzeAndSave(copy, d.FileName, $"Analysis from {DateTime.Now:G}");
                    }
                    SetMessage("Analysis saved: " + d.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>The old pixel analysis, made safe: works on a copy, no background thread on the shown bitmap.</summary>
        private void PixelAnalysis()
        {
            if (!CheckImage())
            {
                return;
            }
            if (_image.Width * _image.Height > 40000 &&
                MessageBox.Show(this, "The image is large – the list can take a while. Continue?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            Bitmap image = CutterImaging.To32Bpp(_image);
            int[] px = CutterImaging.GetPixels(image);
            int w = image.Width, h = image.Height;
            const int zoom = 10;
            var form = new Form { Text = "Pixel color analysis", Size = new Size(1000, 800), ShowIcon = false, StartPosition = FormStartPosition.CenterParent, TopMost = TopMost };
            var split = new SplitContainer { Dock = DockStyle.Fill };
            var pic = new PictureBox { SizeMode = PictureBoxSizeMode.AutoSize };
            var picPanel = new Panel { AutoScroll = true, Dock = DockStyle.Fill };
            picPanel.Controls.Add(pic);
            split.Panel1.Controls.Add(picPanel);
            var mode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200, Dock = DockStyle.Top };
            mode.Items.AddRange(new object[] { "Color + gray", "Only colors", "Only gray (hued by UO)" });
            mode.SelectedIndex = 0;
            var list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false };
            list.Columns.Add("Pixel", 90);
            list.Columns.Add("R, G, B", 110);
            list.Columns.Add("Hex", 80);
            list.Columns.Add("UO", 120);
            split.Panel2.Controls.Add(list);
            split.Panel2.Controls.Add(mode);
            form.Controls.Add(split);

            var zoomed = new Bitmap(w * zoom, h * zoom);
            using (var g = Graphics.FromImage(zoomed))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(image, new Rectangle(0, 0, w * zoom, h * zoom));
            }
            pic.Image = zoomed;
            var index = new Dictionary<int, ListViewItem>();

            void Fill()
            {
                list.BeginUpdate();
                list.Items.Clear();
                index.Clear();
                for (int i = 0; i < px.Length; i++)
                {
                    int p = px[i];
                    if (((p >> 24) & 0xFF) == 0)
                    {
                        continue;
                    }
                    bool hued = ParticleGrayCore.IsHued(p);
                    if ((mode.SelectedIndex == 1 && hued) || (mode.SelectedIndex == 2 && !hued))
                    {
                        continue;
                    }
                    Color c = Color.FromArgb(p);
                    var item = new ListViewItem(new[] { $"{i % w}, {i / w}", $"{c.R}, {c.G}, {c.B}", Hex(c), hued ? $"gray level {ParticleGrayCore.GrayLevel(p)}" : "not hued" })
                    {
                        UseItemStyleForSubItems = false
                    };
                    item.SubItems[2].BackColor = c;
                    item.SubItems[2].ForeColor = c.R * 0.299 + c.G * 0.587 + c.B * 0.114 > 140 ? Color.Black : Color.White;
                    list.Items.Add(item);
                    index[i] = item;
                }
                list.EndUpdate();
            }
            Fill();
            mode.SelectedIndexChanged += (s, e) => Fill();
            pic.MouseClick += (s, e) =>
            {
                int x = e.X / zoom, y = e.Y / zoom;
                if (x < 0 || y < 0 || x >= w || y >= h)
                {
                    return;
                }
                Color c = Color.FromArgb(px[y * w + x]);
                Clipboard.SetText(Hex(c));
                if (index.TryGetValue(y * w + x, out ListViewItem item))
                {
                    list.SelectedItems.Clear();
                    item.Selected = true;
                    item.EnsureVisible();
                }
                form.Text = $"Pixel color analysis – {x}, {y} {Hex(c)} copied";
            };
            form.FormClosed += (s, e) => { zoomed.Dispose(); image.Dispose(); };
            form.Show(this);
        }

        #endregion

        #region Keyboard, help, closing

        private void ShowHelp() => ParticleGrayHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            bool ctrl = (keyData & Keys.Control) == Keys.Control;
            bool shift = (keyData & Keys.Shift) == Keys.Shift;
            bool alt = (keyData & Keys.Alt) == Keys.Alt;
            bool text = IsTextInputFocused();
            if (alt)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            if (key == Keys.F1)
            {
                ShowHelp();
                return true;
            }
            if (ctrl)
            {
                switch (key)
                {
                    case Keys.O: OpenImage(); return true;
                    case Keys.S: SaveImage(); return true;
                    case Keys.F: FitZoom(); return true;
                    case Keys.Oemplus: case Keys.Add: SetZoom(canvas.Zoom + 1); return true;
                    case Keys.OemMinus: case Keys.Subtract: SetZoom(canvas.Zoom - 1); return true;
                }
                if (!text)
                {
                    switch (key)
                    {
                        case Keys.V: PasteImage(); return true;
                        case Keys.C: CopyImage(); return true;
                        case Keys.Z: if (shift) Redo(); else Undo(); return true;
                        case Keys.Y: Redo(); return true;
                        case Keys.A: MaskOp(m => m.Select(_ => true).ToArray(), "Everything marked."); return true;
                        case Keys.D: MaskOp(m => new bool[m.Length], "Mask cleared."); return true;
                        case Keys.I: MaskOp(m => m.Select(v => !v).ToArray(), "Mask inverted."); return true;
                    }
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }
            if (text)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            switch (key)
            {
                case Keys.B: SetTool(CanvasTool.MaskPaint); return true;
                case Keys.W: SetTool(CanvasTool.Wand); return true;
                case Keys.T: SetTool(CanvasTool.Stamp); return true;
                case Keys.M: SetTool(CanvasTool.Select); return true;
                case Keys.P: SetTool(CanvasTool.Pencil); return true;
                case Keys.E: SetTool(CanvasTool.Eraser); return true;
                case Keys.I: SetTool(CanvasTool.Pipette); return true;
                case Keys.G: ConvertGray(); return true;
                case Keys.H: ToggleHueView(); return true;
                case Keys.Escape: canvas.ClearSelection(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            previewTimer.Stop();
            _image?.Dispose();
            _original?.Dispose();
            _huePreview?.Dispose();
            foreach ((Bitmap img, bool[] _) in _undo.Concat(_redo))
            {
                img.Dispose();
            }
            foreach ((string _, bool[,] _, Bitmap preview, bool _) in _stamps)
            {
                preview?.Dispose();
            }
            base.OnFormClosed(e);
        }

        #endregion
    }
}
