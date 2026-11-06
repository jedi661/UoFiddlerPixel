using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;
using Timer = System.Windows.Forms.Timer;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Animation frame editor: 10 frames with layers (original / paint), shared reference animation,
    /// onion skin, guide lines, drawing tools, undo/redo, playback, and browsers for UO animations,
    /// art and clipboard images.
    /// </summary>
    public partial class AnimationEditFormButton : Form
    {
        #region Fields

        private readonly AnimDocument doc = new AnimDocument();
        private int current;
        private bool updatingUi;

        private Timer playTimer;
        private List<int> playOrder = new List<int>();
        private int playPos = -1;
        private Bitmap previewImage;

        private readonly List<Color> recentColors = new List<Color>();
        private readonly int[] frameZoom = new int[AnimDocument.FrameCount];
        private Image customBackground;
        private string lastProjectFolder;

        /// <summary>
        /// Kept for compatibility: SelectablePictureBox.MirrorAllImages() iterates over this array.
        /// The new editor does not use SelectablePictureBox anymore, so it stays empty.
        /// </summary>
        public static SelectablePictureBox[] boxes = new SelectablePictureBox[AnimDocument.FrameCount];

        #endregion

        #region Constructor / lifetime

        public AnimationEditFormButton()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            KeyPreview = true;

            BuildLayout();
            WireEditor();
            WireOverlays();

            playTimer = new Timer(components);
            playTimer.Tick += PlayTimer_Tick;

            InitAnimBrowser();
            InitArtBrowser();
            InitClipboardTab();

            Load += OnLoad;
            FormClosing += OnFormClosing;
        }

        private async void OnLoad(object sender, EventArgs e)
        {
            split.SplitterDistance = Math.Max(300, split.Width - 330);
            ZoomToFit();
            SetTool(DrawTool.Pencil);
            ApplyColors();
            SelectFrame(0);
            SetStatus("Ready. F1 = help / Hilfe.");

            try
            {
                await LoadBrowserBodiesAsync();
                await LoadArtListAsync();
            }
            catch (Exception ex)
            {
                if (!IsDisposed) SetStatus("Load error: " + ex.Message);
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            playTimer?.Stop();
            CloseBrowsers();
            preview.Image = null;
            previewImage?.Dispose();
            previewImage = null;
            canvas.Reference?.Dispose();
            canvas.Reference = null;
            customBackground?.Dispose();
            doc.Dispose();
        }

        #endregion

        #region Wiring

        private void WireEditor()
        {
            canvas.Document = doc;
            strip.Document = doc;
            strip.ThumbnailRenderer = i => doc.Compose(i);
            canvas.AffectedFrames = Affected;

            canvas.ContentChanged += frames => RefreshFrames(frames);
            canvas.HoverChanged += OnHover;
            canvas.ColorPicked += (c, button) =>
            {
                if (c.A == 0) { SetStatus("Transparent pixel – no color picked."); return; }
                if (button == MouseButtons.Right) canvas.SecondaryColor = c;
                else canvas.PrimaryColor = Color.FromArgb(255, c);
                if (button != MouseButtons.Right) numAlpha.Value = c.A;
                ApplyColors();
            };
            canvas.ZoomRequested += d => SetZoom(canvas.Zoom + d);

            strip.FrameSelected += SelectFrame;
            strip.FrameCheckedChanged += i => { UpdatePlayOrder(); UpdatePreviewStill(); };
            strip.FilesDropped += (i, files) => LoadFilesInto(i, files);
            strip.FrameDoubleClicked += i => { SelectFrame(i); LoadImagesSequential(); };
            strip.ContextMenuStrip = BuildFrameMenu();

            doc.HistoryChanged += (s, e) => UpdateHistoryUi();
            canvas.SelectionChanged += () =>
            {
                Rectangle r = canvas.Selection;
                lbSelection.Text = r.IsEmpty
                    ? "No selection – tool 'Select' (S) or Ctrl + drag."
                    : $"Selection: x {r.X}, y {r.Y}, {r.Width} x {r.Height}";
            };

            // layers
            chkOrigVisible.CheckedChanged += (s, e) => SetLayerProp(f => f.OriginalVisible = chkOrigVisible.Checked);
            chkPaintVisible.CheckedChanged += (s, e) => SetLayerProp(f => f.PaintVisible = chkPaintVisible.Checked);
            trkOrigOpacity.ValueChanged += (s, e) => SetLayerProp(f => f.OriginalOpacity = trkOrigOpacity.Value / 100f);
            trkPaintOpacity.ValueChanged += (s, e) => SetLayerProp(f => f.PaintOpacity = trkPaintOpacity.Value / 100f);

            chkRefVisible.CheckedChanged += (s, e) => { canvas.ReferenceVisible = chkRefVisible.Checked; RefreshView(); };
            trkRefOpacity.ValueChanged += (s, e) => { canvas.ReferenceOpacity = trkRefOpacity.Value / 100f; RefreshView(); };
            chkRefOnTop.CheckedChanged += (s, e) => { canvas.ReferenceOnTop = chkRefOnTop.Checked; RefreshView(); };
            numRefFrame.ValueChanged += (s, e) => { canvas.ReferenceFixedFrame = (int)numRefFrame.Value - 1; RefreshView(); };
            numRefOffX.ValueChanged += (s, e) => { canvas.ReferenceOffset = new Point((int)numRefOffX.Value, (int)numRefOffY.Value); RefreshView(); };
            numRefOffY.ValueChanged += (s, e) => { canvas.ReferenceOffset = new Point((int)numRefOffX.Value, (int)numRefOffY.Value); RefreshView(); };

            // align
            numOffX.ValueChanged += (s, e) => OffsetFromUi();
            numOffY.ValueChanged += (s, e) => OffsetFromUi();
            numAnchorX.ValueChanged += (s, e) => AnchorFromUi();
            numAnchorY.ValueChanged += (s, e) => AnchorFromUi();

            // view
            chkGrid.CheckedChanged += (s, e) => { canvas.ShowGrid = chkGrid.Checked; canvas.Invalidate(); };
            chkAnchor.CheckedChanged += (s, e) => { canvas.ShowAnchor = chkAnchor.Checked; canvas.Invalidate(); };
            chkBounds.CheckedChanged += (s, e) => { canvas.ShowLayerBounds = chkBounds.Checked; canvas.Invalidate(); };
            chkGuides.CheckedChanged += (s, e) => GuidesFromUi();
            numGuideY1.ValueChanged += (s, e) => GuidesFromUi();
            numGuideY2.ValueChanged += (s, e) => GuidesFromUi();
            numGuideX.ValueChanged += (s, e) => GuidesFromUi();
            chkOnion.CheckedChanged += (s, e) => OnionFromUi();
            chkOnionTint.CheckedChanged += (s, e) => OnionFromUi();
            chkOnionWrap.CheckedChanged += (s, e) => OnionFromUi();
            numOnionPrev.ValueChanged += (s, e) => OnionFromUi();
            numOnionNext.ValueChanged += (s, e) => OnionFromUi();
            trkOnionOpacity.ValueChanged += (s, e) => OnionFromUi();
            cbBackground.SelectedIndexChanged += (s, e) => BackgroundFromUi();

            // colors
            tbHex.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                HexToColor();
            };
            tbHex.Leave += (s, e) => HexToColor();
            BuildBasicPalette();

            // playback
            numFps.ValueChanged += (s, e) => { if (playTimer.Enabled) playTimer.Interval = 1000 / (int)numFps.Value; };
            numFrom.ValueChanged += (s, e) => UpdatePlayOrder();
            numTo.ValueChanged += (s, e) => UpdatePlayOrder();
            chkPingPong.CheckedChanged += (s, e) => UpdatePlayOrder();
            chkOnlyChecked.CheckedChanged += (s, e) => UpdatePlayOrder();
            numPreviewZoom.ValueChanged += (s, e) => { preview.Zoom = (int)numPreviewZoom.Value; preview.Invalidate(); };
            chkMirrorPreview.CheckedChanged += (s, e) => { preview.ShowMirrored = chkMirrorPreview.Checked; preview.Invalidate(); };
            chkPreviewRef.CheckedChanged += (s, e) => UpdatePreviewStill();
        }

        private ContextMenuStrip BuildFrameMenu()
        {
            var m = new ContextMenuStrip();
            m.Items.Add("Load images here…", null, (s, e) => LoadImagesSequential());
            m.Items.Add("Paste as original (Ctrl+V)", null, (s, e) => PasteFromClipboard(false));
            m.Items.Add("Copy image (Ctrl+C)", null, (s, e) => CopyFrameToClipboard());
            m.Items.Add(new ToolStripSeparator());
            m.Items.Add("Duplicate to next (Ctrl+D)", null, (s, e) => DuplicateToNext());
            m.Items.Add("Copy to all checked", null, (s, e) => CopyFrameToChecked());
            m.Items.Add("Move left", null, (s, e) => MoveFrame(-1));
            m.Items.Add("Move right", null, (s, e) => MoveFrame(1));
            m.Items.Add(new ToolStripSeparator());
            m.Items.Add("Mirror", null, (s, e) => ApplyToAffected("Mirror", i => doc.MirrorFrame(i)));
            m.Items.Add("Clear frame", null, (s, e) => ClearFrames());
            m.Items.Add(new ToolStripSeparator());
            m.Items.Add("Check only this frame", null, (s, e) =>
            {
                for (int i = 0; i < AnimDocument.FrameCount; i++) doc.Frames[i].Enabled = i == current;
                strip.Invalidate();
                UpdatePlayOrder();
            });
            m.Items.Add("Add to clipboard animation", null, (s, e) => AddCurrentFrameToClipboardList());
            return m;
        }

        #endregion

        #region Frame selection / refresh

        /// <summary>Current frame, plus all checked frames when "All checked" is on.</summary>
        private IEnumerable<int> Affected()
        {
            var list = new List<int> { current };
            if (btAllChecked != null && btAllChecked.Checked)
            {
                for (int i = 0; i < AnimDocument.FrameCount; i++)
                    if (doc.Frames[i].Enabled && i != current) list.Add(i);
            }
            return list;
        }

        private List<int> CheckedFrames() =>
            Enumerable.Range(0, AnimDocument.FrameCount).Where(i => doc.Frames[i].Enabled).ToList();

        private void SelectFrame(int i)
        {
            if (canvas.IsBusy) return;
            current = Math.Max(0, Math.Min(AnimDocument.FrameCount - 1, i));
            canvas.FrameIndex = current;
            if (chkZoomPerFrame.Checked && frameZoom[current] > 0 && frameZoom[current] != canvas.Zoom)
                SetZoom(frameZoom[current]);
            strip.Selected = current;
            LoadFrameUi();
            UpdatePreviewStill();
        }

        /// <summary>Copies the properties of the current frame into the side panel controls.</summary>
        private void LoadFrameUi()
        {
            updatingUi = true;
            try
            {
                AnimFrame f = doc.Frames[current];
                chkOrigVisible.Checked = f.OriginalVisible;
                chkPaintVisible.Checked = f.PaintVisible;
                trkOrigOpacity.Value = (int)Math.Round(f.OriginalOpacity * 100);
                trkPaintOpacity.Value = (int)Math.Round(f.PaintOpacity * 100);
                numOffX.Value = Math.Max(numOffX.Minimum, Math.Min(numOffX.Maximum, f.Offset.X));
                numOffY.Value = Math.Max(numOffY.Minimum, Math.Min(numOffY.Maximum, f.Offset.Y));
                numOffX.Enabled = numOffY.Enabled = f.HasOriginal;
                numCanvasW.Value = doc.CanvasSize.Width;
                numCanvasH.Value = doc.CanvasSize.Height;
                numAnchorX.Maximum = doc.CanvasSize.Width - 1;
                numAnchorY.Maximum = doc.CanvasSize.Height - 1;
                numAnchorX.Value = Math.Min(numAnchorX.Maximum, doc.Anchor.X);
                numAnchorY.Value = Math.Min(numAnchorY.Maximum, doc.Anchor.Y);

                string size = f.HasOriginal ? $"{f.Original.Width}x{f.Original.Height}" : "no original";
                lbFrameInfo.Text = $"Frame {current + 1}: {size}" + (string.IsNullOrEmpty(f.Source) ? "" : "\n" + f.Source);
                stFrame.Text = $"Frame {current + 1}/10 | {size} | offset {f.Offset.X},{f.Offset.Y}" + (string.IsNullOrEmpty(f.Source) ? "" : " | " + f.Source);
            }
            finally { updatingUi = false; }
        }

        private void RefreshFrames(IEnumerable<int> frames)
        {
            foreach (int i in frames.Distinct()) strip.RefreshFrame(i);
            canvas.Invalidate();
            LoadFrameUi();
            UpdatePreviewStill();
        }

        private void RefreshAllFrames()
        {
            canvas.UpdateCanvasSize();
            CenterCanvas();
            strip.RefreshAll();
            canvas.Invalidate();
            LoadFrameUi();
            UpdatePlayOrder();
            UpdatePreviewStill();
            SyncOverlayList();
            LoadOverlayUi();
        }

        /// <summary>For view-only changes (reference, onion, background).</summary>
        private void RefreshView()
        {
            canvas.Invalidate();
            UpdatePreviewStill();
        }

        private void SetStatus(string text) => stMsg.Text = text;

        private void OnHover(Point? p, Color c)
        {
            if (p == null)
            {
                stPos.Text = "x: -  y: -";
                stColor.Text = "";
                stColorSwatch.Image?.Dispose();
                stColorSwatch.Image = null;
                return;
            }
            stPos.Text = $"x: {p.Value.X}  y: {p.Value.Y}";
            stColor.Text = c.A == 0 ? "transparent" : $"{ImageUtil.ToHex(c)}  A{c.A}";
            stColorSwatch.Image?.Dispose();
            stColorSwatch.Image = Swatch(c, 14);
        }

        #endregion

        #region Tools, colors, zoom

        private void SetTool(DrawTool t)
        {
            canvas.Tool = t;
            foreach (var kv in toolButtons) kv.Value.Checked = kv.Key == t;
            canvas.UpdateCursor();
            SetStatus("Tool: " + t);
        }

        private void SetZoom(int z)
        {
            canvas.Zoom = z;
            frameZoom[current] = canvas.Zoom;
            lbZoom.Text = canvas.Zoom + "x";
            stZoom.Text = "Zoom " + canvas.Zoom + "x";
            CenterCanvas();
        }

        private void ZoomToFit()
        {
            int zx = Math.Max(1, (canvasHost.ClientSize.Width - 10) / doc.CanvasSize.Width);
            int zy = Math.Max(1, (canvasHost.ClientSize.Height - 10) / doc.CanvasSize.Height);
            SetZoom(Math.Min(zx, zy));
        }

        private void ApplyColors()
        {
            canvas.PrimaryColor = Color.FromArgb((int)numAlpha.Value, canvas.PrimaryColor);
            btPrimary.Image?.Dispose();
            btPrimary.Image = Swatch(canvas.PrimaryColor);
            btSecondary.Image?.Dispose();
            btSecondary.Image = Swatch(canvas.SecondaryColor);
            pnlPrimary.BackColor = Color.FromArgb(255, canvas.PrimaryColor);
            pnlSecondary.BackColor = Color.FromArgb(255, canvas.SecondaryColor);
            if (!tbHex.Focused) tbHex.Text = ImageUtil.ToHex(canvas.PrimaryColor);
            AddRecentColor(canvas.PrimaryColor);
        }

        private void SwapColors()
        {
            Color p = canvas.PrimaryColor;
            canvas.PrimaryColor = Color.FromArgb((int)numAlpha.Value, canvas.SecondaryColor);
            canvas.SecondaryColor = Color.FromArgb(255, p);
            ApplyColors();
        }

        private void PickColorDialog(bool primary)
        {
            using (var dlg = new ColorDialog { FullOpen = true, Color = primary ? canvas.PrimaryColor : canvas.SecondaryColor })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                if (primary) canvas.PrimaryColor = dlg.Color;
                else canvas.SecondaryColor = dlg.Color;
                ApplyColors();
            }
        }

        private void HexToColor()
        {
            string code = tbHex.Text.Trim();
            if (!code.StartsWith("#")) code = "#" + code;
            if (!Regex.IsMatch(code, "^#[0-9A-Fa-f]{6}$")) return;
            canvas.PrimaryColor = ColorTranslator.FromHtml(code);
            ApplyColors();
        }

        private Button ColorButton(Color c)
        {
            var b = new Button
            {
                Size = new Size(16, 16),
                Margin = new Padding(1),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, c),
                Tag = c
            };
            b.FlatAppearance.BorderColor = Color.Gray;
            b.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right) canvas.SecondaryColor = c;
                else canvas.PrimaryColor = c;
                ApplyColors();
            };
            tips.SetToolTip(b, ImageUtil.ToHex(c));
            return b;
        }

        private void AddRecentColor(Color c)
        {
            Color opaque = Color.FromArgb(255, c);
            int idx = recentColors.FindIndex(x => x.ToArgb() == opaque.ToArgb());
            if (idx == 0) return;
            if (idx > 0) recentColors.RemoveAt(idx);
            recentColors.Insert(0, opaque);
            if (recentColors.Count > 32) recentColors.RemoveAt(recentColors.Count - 1);

            flowRecent.SuspendLayout();
            foreach (Control ctl in flowRecent.Controls.Cast<Control>().ToList()) ctl.Dispose();
            foreach (Color rc in recentColors) flowRecent.Controls.Add(ColorButton(rc));
            flowRecent.ResumeLayout();
        }

        private void BuildBasicPalette()
        {
            Color[] basics =
            {
                Color.Black, Color.White, Color.FromArgb(64, 64, 64), Color.Gray, Color.Silver, Color.FromArgb(211, 211, 211),
                Color.Red, Color.DarkRed, Color.Orange, Color.Gold, Color.Yellow, Color.FromArgb(139, 69, 19),
                Color.Lime, Color.Green, Color.DarkGreen, Color.Cyan, Color.Teal, Color.DodgerBlue,
                Color.Blue, Color.Navy, Color.Purple, Color.Magenta, Color.Pink, Color.FromArgb(255, 220, 177),
                Color.FromArgb(222, 184, 135), Color.FromArgb(160, 120, 90), Color.FromArgb(110, 80, 60), Color.FromArgb(70, 50, 40),
                Color.FromArgb(200, 200, 160), Color.FromArgb(120, 140, 160), Color.FromArgb(80, 100, 60), Color.FromArgb(150, 30, 30)
            };
            foreach (Color c in basics) flowBasic.Controls.Add(ColorButton(c));
        }

        private void FillFramePalette()
        {
            flowFramePalette.SuspendLayout();
            foreach (Control ctl in flowFramePalette.Controls.Cast<Control>().ToList()) ctl.Dispose();
            using (Bitmap b = doc.Compose(current))
            {
                foreach (Color c in ImageUtil.UsedColors(b, 96))
                    flowFramePalette.Controls.Add(ColorButton(c));
            }
            flowFramePalette.ResumeLayout();
            SetStatus($"{flowFramePalette.Controls.Count} colors in frame {current + 1}.");
        }

        #endregion

        #region View settings

        private void GuidesFromUi()
        {
            canvas.ShowGuides = chkGuides.Checked;
            canvas.GuideY1 = (int)numGuideY1.Value;
            canvas.GuideY2 = (int)numGuideY2.Value;
            canvas.GuideX = (int)numGuideX.Value;
            canvas.Invalidate();
        }

        private void GuidesAtAnchor()
        {
            numGuideY1.Value = doc.Anchor.Y;
            numGuideX.Value = doc.Anchor.X;
            if (numGuideY2.Value < 0) numGuideY2.Value = Math.Max(0, doc.Anchor.Y - 60);
            chkGuides.Checked = true;
        }

        private void PickGuideColor()
        {
            using (var dlg = new ColorDialog { Color = canvas.GuideColor })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                canvas.GuideColor = dlg.Color;
                btGuideColor.BackColor = dlg.Color;
                canvas.Invalidate();
            }
        }

        private void OnionFromUi()
        {
            canvas.OnionEnabled = chkOnion.Checked;
            canvas.OnionTint = chkOnionTint.Checked;
            canvas.OnionWrap = chkOnionWrap.Checked;
            canvas.OnionPrev = (int)numOnionPrev.Value;
            canvas.OnionNext = (int)numOnionNext.Value;
            canvas.OnionOpacity = trkOnionOpacity.Value / 100f;
            canvas.Invalidate();
        }

        private void BackgroundFromUi()
        {
            string sel = cbBackground.SelectedItem?.ToString() ?? "Checker";
            Image img = null;
            switch (sel)
            {
                case "Checker": canvas.BackgroundMode = CanvasBackground.Checker; break;
                case "Dark": SetSolid(Color.FromArgb(40, 40, 40)); break;
                case "Black": SetSolid(Color.Black); break;
                case "White": SetSolid(Color.White); break;
                case "Magenta": SetSolid(Color.Magenta); break;
                case "Green": img = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Green2; break;
                case "Water": img = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.water2; break;
                case "Sand": img = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Sand; break;
                case "Street": img = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Street; break;
                case "Forest": img = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Forest; break;
                case "Dirt": img = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Dirt; break;
                case "Dungeon": img = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Dungeon; break;
                case "Cave": img = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.cave; break;
                default:
                    using (var dlg = new OpenFileDialog { Filter = ImageFilter, Title = "Background image" })
                    {
                        if (dlg.ShowDialog(this) == DialogResult.OK)
                        {
                            try
                            {
                                customBackground?.Dispose();
                                customBackground = ImageUtil.LoadCopy(dlg.FileName);
                                img = customBackground;
                            }
                            catch (Exception ex) { ShowError("Image could not be loaded", ex); }
                        }
                    }
                    if (img == null) { cbBackground.SelectedIndex = 0; return; }
                    break;
            }

            if (img != null)
            {
                canvas.BackgroundMode = CanvasBackground.Image;
                canvas.BackgroundPicture = img;
            }
            preview.Checker = canvas.BackgroundMode == CanvasBackground.Checker;
            RefreshView();

            void SetSolid(Color c)
            {
                canvas.BackgroundMode = CanvasBackground.SolidColor;
                canvas.BackgroundSolid = c;
            }
        }

        #endregion

        #region Layer properties / operations

        private void SetLayerProp(Action<AnimFrame> set)
        {
            if (updatingUi) return;
            foreach (int i in Affected()) set(doc.Frames[i]);
            RefreshFrames(Affected());
        }

        /// <summary>Runs an undoable operation on the affected frames (current + checked in multi mode).</summary>
        private void ApplyToAffected(string name, Action<int> op)
        {
            var frames = Affected().ToList();
            doc.BeginChange(name, frames);
            foreach (int i in frames) op(i);
            RefreshFrames(frames);
            SetStatus($"{name}: {frames.Count} frame(s).");
        }

        private EditLayer TargetLayer => cbLayer.SelectedIndex == 1 ? EditLayer.Original : EditLayer.Paint;

        /// <summary>Changes the pixels of the active layer of the affected frames.</summary>
        private void ModifyLayerPixels(string name, Action<PixelBuffer> op)
        {
            var frames = Affected().Where(i => TargetLayer == EditLayer.Paint || doc.Frames[i].HasOriginal).ToList();
            if (frames.Count == 0) { SetStatus("Nothing to change on this layer."); return; }
            doc.BeginChange(name, frames);
            foreach (int i in frames)
            {
                Bitmap target = TargetLayer == EditLayer.Original ? doc.Frames[i].Original : doc.Frames[i].Paint;
                var pb = new PixelBuffer(target);
                op(pb);
                pb.WriteTo(target);
            }
            RefreshFrames(frames);
            SetStatus($"{name}: {frames.Count} frame(s), layer {TargetLayer}.");
        }

        private void MakeKeysTransparent()
        {
            int tol = (int)numTolerance.Value;
            ModifyLayerPixels("Keys → transparent", pb =>
            {
                pb.ReplaceColor(Color.Black.ToArgb(), 0, tol, true);
                pb.ReplaceColor(Color.White.ToArgb(), 0, tol, true);
                pb.ReplaceColor(Color.FromArgb(211, 211, 211).ToArgb(), 0, tol, true);
            });
        }

        private void SecondaryToTransparent()
        {
            int key = Color.FromArgb(255, canvas.SecondaryColor).ToArgb();
            int tol = (int)numTolerance.Value;
            ModifyLayerPixels("Color → transparent", pb => pb.ReplaceColor(key, 0, tol, true));
        }

        private void ReplaceSecondaryWithPrimary()
        {
            int from = Color.FromArgb(255, canvas.SecondaryColor).ToArgb();
            int to = canvas.PrimaryColor.ToArgb();
            int tol = (int)numTolerance.Value;
            ModifyLayerPixels("Replace color", pb => pb.ReplaceColor(from, to, tol, true));
        }

        private void DrawOutline()
        {
            int c = Color.FromArgb(255, canvas.PrimaryColor).ToArgb();
            ModifyLayerPixels("Outline", pb => pb.Outline(c));
        }

        private void ClearTargetLayer()
        {
            if (TargetLayer == EditLayer.Original) ApplyToAffected("Clear original", i => doc.ClearOriginal(i));
            else ApplyToAffected("Clear paint", i => doc.ClearPaint(i));
        }

        private void ClearFrames() => ApplyToAffected("Clear frame", i => doc.ClearFrame(i));

        private void ClearAllFrames()
        {
            if (MessageBox.Show(this, "Clear all 10 frames? (Undo is possible)\nAlle 10 Frames leeren? (Rückgängig möglich)", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            StopPlay();
            doc.BeginChangeAll("Clear all");
            for (int i = 0; i < AnimDocument.FrameCount; i++) doc.ClearFrame(i);
            RefreshAllFrames();
        }

        private void CopyPaintToChecked()
        {
            var targets = CheckedFrames().Where(i => i != current).ToList();
            if (targets.Count == 0) { SetStatus("No other checked frames."); return; }
            doc.BeginChange("Paint → checked", targets);
            foreach (int i in targets)
            {
                using (Graphics g = Graphics.FromImage(doc.Frames[i].Paint))
                    g.DrawImage(doc.Frames[current].Paint, 0, 0, doc.CanvasSize.Width, doc.CanvasSize.Height);
            }
            RefreshFrames(targets);
            SetStatus($"Paint layer of frame {current + 1} drawn onto {targets.Count} frame(s).");
        }

        #endregion

        #region Key transparency / selection

        private void SetKeyTransparent(bool on)
        {
            doc.KeyColorsTransparent = on;
            btKeyTransparent.BackColor = on ? Color.LightGreen : SystemColors.Control;
            strip.RefreshAll();
            RefreshView();
            SetStatus(on ? "Black/white/light gray of the originals shown transparent." : "Black/white shown normally again.");
        }

        /// <summary>
        /// Puts an image into the marked rectangle as a floating overlay (not burned in): B/W stay in the
        /// overlay and are shown transparent, the overlay can still be moved, scaled, tinted and baked later.
        /// </summary>
        private void InsertIntoSelection(Image src, string name)
        {
            Rectangle sel = canvas.Selection;
            if (sel.IsEmpty || src == null)
            {
                SetStatus("Mark an area first: tool 'Select' (S) or Ctrl + drag. / Erst einen Bereich markieren: Werkzeug 'Select' (S) oder Strg + Ziehen.");
                return;
            }
            using (Bitmap b = ImageUtil.To32(src))
                AddOverlay(new List<Bitmap> { ImageUtil.To32(b) }, name, sel);
        }

        private void LoadImageIntoSelection()
        {
            if (canvas.Selection.IsEmpty) { InsertIntoSelection(null, null); return; }
            using (var dlg = new OpenFileDialog { Filter = ImageFilter, Title = "Image for the selection" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    using (Bitmap b = ImageUtil.LoadCopy(dlg.FileName))
                        InsertIntoSelection(b, Path.GetFileName(dlg.FileName));
                }
                catch (Exception ex) { ShowError("Image could not be loaded", ex); }
            }
        }

        private void PasteIntoSelection()
        {
            if (canvas.Selection.IsEmpty) { InsertIntoSelection(null, null); return; }
            Image clip = Clipboard.ContainsImage() ? Clipboard.GetImage() : null;
            if (clip == null) { SetStatus("The clipboard does not contain an image."); return; }
            using (clip) InsertIntoSelection(clip, "Clipboard image");
        }

        private void CopySelection()
        {
            Rectangle sel = canvas.Selection;
            if (sel.IsEmpty) { SetStatus("Nothing marked."); return; }
            using (Bitmap full = doc.Compose(current))
            using (Bitmap part = full.Clone(sel, PixelFormat.Format32bppArgb))
                Clipboard.SetImage(part);
            SetStatus($"Selection {sel.Width}x{sel.Height} copied.");
        }

        private void ClearSelectionArea()
        {
            Rectangle sel = canvas.Selection;
            if (sel.IsEmpty) { SetStatus("Nothing marked."); return; }
            var frames = Affected().Where(i => TargetLayer == EditLayer.Paint || doc.Frames[i].HasOriginal).ToList();
            if (frames.Count == 0) return;
            doc.BeginChange("Clear selection", frames);
            foreach (int i in frames)
            {
                AnimFrame f = doc.Frames[i];
                Bitmap target = TargetLayer == EditLayer.Original ? f.Original : f.Paint;
                Rectangle r = TargetLayer == EditLayer.Original ? new Rectangle(sel.X - f.Offset.X, sel.Y - f.Offset.Y, sel.Width, sel.Height) : sel;
                var pb = new PixelBuffer(target);
                pb.Rect(r, 1, 0, true, true);
                pb.WriteTo(target);
            }
            RefreshFrames(frames);
        }

        #endregion

        #region Alignment / canvas

        private void OffsetFromUi()
        {
            if (updatingUi || !doc.Frames[current].HasOriginal) return;
            doc.BeginChange("Offset", new[] { current }, true);
            doc.Frames[current].Offset = new Point((int)numOffX.Value, (int)numOffY.Value);
            RefreshFrames(new[] { current });
        }

        private void Nudge(int dx, int dy)
        {
            var frames = Affected().ToList();
            doc.BeginChange("Nudge " + TargetLayer, frames, true);
            foreach (int i in frames) doc.Shift(i, TargetLayer, dx, dy);
            RefreshFrames(frames);
        }

        private void CenterOnAnchor(int i)
        {
            AnimFrame f = doc.Frames[i];
            if (f.Original == null) return;
            f.Offset = new Point(doc.Anchor.X - f.Original.Width / 2, doc.Anchor.Y - f.Original.Height / 2);
        }

        private void AlignCheckedBottoms()
        {
            var frames = CheckedFrames().Where(i => doc.Frames[i].HasOriginal).ToList();
            if (frames.Count == 0) return;
            doc.BeginChange("Align bottoms", frames);
            foreach (int i in frames) doc.AlignBottomToAnchor(i);
            RefreshFrames(frames);
            SetStatus($"{frames.Count} frame(s) aligned: bottom center on the anchor.");
        }

        private void CopyOffsetToChecked()
        {
            Point o = doc.Frames[current].Offset;
            var frames = CheckedFrames().Where(i => i != current && doc.Frames[i].HasOriginal).ToList();
            if (frames.Count == 0) return;
            doc.BeginChange("Offset → checked", frames);
            foreach (int i in frames) doc.Frames[i].Offset = o;
            RefreshFrames(frames);
        }

        private void ApplyCanvasSize()
        {
            doc.BeginChangeAll("Canvas size");
            doc.ResizeCanvas((int)numCanvasW.Value, (int)numCanvasH.Value);
            RefreshAllFrames();
            ZoomToFit();
        }

        private void AnchorFromUi()
        {
            if (updatingUi) return;
            doc.SetAnchor(new Point((int)numAnchorX.Value, (int)numAnchorY.Value));
            RefreshView();
        }

        /// <summary>Resizes the canvas so that every frame fits (plus a small margin).</summary>
        private void FitCanvas()
        {
            Rectangle all = Rectangle.Empty;
            for (int i = 0; i < AnimDocument.FrameCount; i++)
            {
                AnimFrame f = doc.Frames[i];
                if (f.Original != null)
                {
                    var r = new Rectangle(f.Offset, f.Original.Size);
                    all = all.IsEmpty ? r : Rectangle.Union(all, r);
                }
                Rectangle p = ImageUtil.ContentBounds(f.Paint);
                if (!p.IsEmpty) all = all.IsEmpty ? p : Rectangle.Union(all, p);
            }
            if (all.IsEmpty) { SetStatus("No content."); return; }

            // keep the anchor horizontally centered: make room on both sides
            int half = Math.Max(doc.Anchor.X - all.Left, all.Right - doc.Anchor.X) + 10;
            int top = doc.Anchor.Y - all.Top + 10;
            int bottom = Math.Max(30, all.Bottom - doc.Anchor.Y + 10);

            doc.BeginChangeAll("Fit canvas");
            doc.SetCanvas(new Size(half * 2, top + bottom), new Point(half, top));
            RefreshAllFrames();
            ZoomToFit();
        }

        #endregion

        #region Frame order / copy

        private void DuplicateToNext()
        {
            if (current >= AnimDocument.FrameCount - 1) { SetStatus("Frame 10 is the last frame."); return; }
            int next = current + 1;
            doc.BeginChange("Duplicate", new[] { next });
            doc.CopyFrame(current, next);
            doc.Frames[next].Enabled = true;
            RefreshFrames(new[] { next });
            SelectFrame(next);
        }

        private void CopyFrameToChecked()
        {
            var targets = CheckedFrames().Where(i => i != current).ToList();
            if (targets.Count == 0) return;
            doc.BeginChange("Copy frame", targets);
            foreach (int i in targets) doc.CopyFrame(current, i);
            RefreshFrames(targets);
        }

        private void MoveFrame(int dir)
        {
            int other = current + dir;
            if (other < 0 || other >= AnimDocument.FrameCount) return;
            doc.BeginChange("Move frame", new[] { current, other });
            doc.SwapFrames(current, other);
            RefreshFrames(new[] { current, other });
            SelectFrame(other);
        }

        private void ReverseChecked()
        {
            var frames = CheckedFrames();
            if (frames.Count < 2) return;
            doc.BeginChange("Reverse", frames);
            for (int a = 0, b = frames.Count - 1; a < b; a++, b--) doc.SwapFrames(frames[a], frames[b]);
            RefreshFrames(frames);
        }

        private void SetAllChecked(bool value)
        {
            foreach (AnimFrame f in doc.Frames) f.Enabled = value;
            strip.Invalidate();
            UpdatePlayOrder();
        }

        private void CheckNonEmpty()
        {
            for (int i = 0; i < AnimDocument.FrameCount; i++) doc.Frames[i].Enabled = !doc.IsEmpty(i);
            strip.Invalidate();
            UpdatePlayOrder();
        }

        #endregion

        #region Undo / redo

        private void DoUndo()
        {
            if (canvas.IsBusy) return;
            int[] frames = doc.Undo();
            if (frames == null) { SetStatus("Nothing to undo."); return; }
            canvas.SelectedOverlay = null;
            RefreshAllFrames();
            SetStatus("Undo (" + frames.Length + " frame(s)).");
        }

        private void DoRedo()
        {
            if (canvas.IsBusy) return;
            int[] frames = doc.Redo();
            if (frames == null) { SetStatus("Nothing to redo."); return; }
            canvas.SelectedOverlay = null;
            RefreshAllFrames();
            SetStatus("Redo (" + frames.Length + " frame(s)).");
        }

        private void UndoTo(int index)
        {
            if (index < 0) return;
            int steps = doc.UndoList.Count - index;
            for (int k = 0; k < steps; k++) doc.Undo();
            RefreshAllFrames();
        }

        private void UpdateHistoryUi()
        {
            btUndo.Enabled = doc.CanUndo;
            btRedo.Enabled = doc.CanRedo;
            btUndo.Text = doc.CanUndo ? "Undo (" + doc.UndoList.Count + ")" : "Undo";
            if (lbHistory == null) return;
            lbHistory.BeginUpdate();
            lbHistory.Items.Clear();
            foreach (HistoryEntry e in doc.UndoList) lbHistory.Items.Add(e.ToString());
            foreach (HistoryEntry e in doc.RedoList.Reverse()) lbHistory.Items.Add("  (redo) " + e.Name);
            lbHistory.EndUpdate();
            if (doc.UndoList.Count > 0) lbHistory.TopIndex = Math.Max(0, doc.UndoList.Count - 10);
        }

        #endregion

        #region Playback

        private void UpdatePlayOrder()
        {
            int from = (int)numFrom.Value - 1, to = (int)numTo.Value - 1;
            if (from > to) (from, to) = (to, from);

            var list = new List<int>();
            for (int i = from; i <= to; i++)
            {
                if (chkOnlyChecked.Checked && !doc.Frames[i].Enabled) continue;
                if (doc.IsEmpty(i)) continue;
                list.Add(i);
            }
            if (chkPingPong.Checked && list.Count > 2)
                list.AddRange(list.Skip(1).Take(list.Count - 2).Reverse().ToList());

            playOrder = list;
            if (playPos >= playOrder.Count) playPos = -1;
        }

        private void TogglePlay()
        {
            if (playTimer.Enabled) { StopPlay(); return; }
            UpdatePlayOrder();
            if (playOrder.Count == 0)
            {
                SetStatus("Nothing to play: check frames with content. / Nichts abzuspielen: Frames mit Inhalt anhaken.");
                return;
            }
            playPos = -1;
            playTimer.Interval = Math.Max(1, 1000 / (int)numFps.Value);
            playTimer.Start();
            btPlay.Text = "■ Stop";
            btPlay.BackColor = Color.OrangeRed;
            btPlay.ForeColor = Color.White;
        }

        private void StopPlay()
        {
            playTimer.Stop();
            btPlay.Text = "▶ Play";
            btPlay.BackColor = SystemColors.Control;
            btPlay.ForeColor = SystemColors.ControlText;
            btPlay.UseVisualStyleBackColor = true;
            strip.Playing = -1;
            UpdatePreviewStill();
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (playOrder.Count == 0) { StopPlay(); return; }
            playPos++;
            if (playPos >= playOrder.Count)
            {
                if (!chkLoop.Checked) { StopPlay(); return; }
                playPos = 0;
            }
            int idx = playOrder[playPos];
            ShowPreview(idx);
            strip.Playing = idx;
            if (chkFollow.Checked && !canvas.IsBusy) SelectFrame(idx);
        }

        private void StepFrame(int dir)
        {
            StopPlay();
            SelectFrame((current + dir + AnimDocument.FrameCount) % AnimDocument.FrameCount);
        }

        private void ShowPreview(int idx)
        {
            Bitmap old = previewImage;
            bool bg = canvas.BackgroundMode != CanvasBackground.Checker;
            bool refVisible = canvas.ReferenceVisible;
            canvas.ReferenceVisible = refVisible && chkPreviewRef.Checked;
            previewImage = canvas.RenderComposite(idx, bg, false);
            canvas.ReferenceVisible = refVisible;
            preview.Image = previewImage;
            preview.Caption = $"Frame {idx + 1}";
            old?.Dispose();
        }

        private void UpdatePreviewStill()
        {
            if (playTimer == null || playTimer.Enabled) return;
            ShowPreview(current);
        }

        #endregion

        #region Import

        private const string ImageFilter = "Image files (*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.tif)|*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|All files (*.*)|*.*";

        /// <summary>
        /// Puts source frames into editor frames. Shared by file import, animation browser, art browser and clipboard.
        /// </summary>
        private void PutFrames(IList<SourceFrame> frames, IList<int> targets, string sourceName)
        {
            int n = Math.Min(frames.Count, targets.Count);
            if (n == 0) { SetStatus("Nothing imported."); return; }
            var used = targets.Take(n).ToList();
            doc.BeginChange("Import", used);
            for (int k = 0; k < n; k++)
            {
                SourceFrame sf = frames[k];
                if (sf?.Bitmap == null) continue;
                doc.SetOriginal(used[k], sf.Bitmap, sf.Center, sourceName + (n > 1 ? $" #{k + 1}" : ""), chkKeepPaint.Checked);
                doc.Frames[used[k]].Enabled = true;
            }
            RefreshFrames(used);
            UpdatePlayOrder();
            SetStatus($"{n} frame(s) imported from {sourceName} into frame(s) {string.Join(",", used.Select(i => i + 1))}.");
        }

        private void LoadFilesInto(int start, string[] files)
        {
            var frames = new List<SourceFrame>();
            foreach (string f in files.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                try { frames.AddRange(ImageUtil.LoadAllFrames(f).Select(b => new SourceFrame { Bitmap = b })); }
                catch (Exception ex) { SetStatus($"Skipped {Path.GetFileName(f)}: {ex.Message}"); }
            }
            if (frames.Count == 0) return;
            var targets = Enumerable.Range(start, AnimDocument.FrameCount - start).ToList();
            string name = files.Length == 1 ? Path.GetFileName(files[0]) : $"{files.Length} files";
            PutFrames(frames, targets, name);
            foreach (SourceFrame f in frames) f.Bitmap?.Dispose();
            SelectFrame(start);
        }

        private void LoadImagesSequential()
        {
            using (var dlg = new OpenFileDialog { Filter = ImageFilter, Multiselect = true, Title = "Load images into frames (starting at the selected frame)" })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK) LoadFilesInto(current, dlg.FileNames);
            }
        }

        private void LoadImageIntoChecked()
        {
            using (var dlg = new OpenFileDialog { Filter = ImageFilter, Title = "Load one image into all checked frames" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    using (Bitmap b = ImageUtil.LoadCopy(dlg.FileName))
                    {
                        var targets = CheckedFrames();
                        var frames = targets.Select(_ => new SourceFrame { Bitmap = b }).ToList();
                        PutFrames(frames, targets, Path.GetFileName(dlg.FileName));
                    }
                }
                catch (Exception ex) { ShowError("Image could not be loaded", ex); }
            }
        }

        private void PasteFromClipboard(bool intoPaint)
        {
            Image clip = Clipboard.ContainsImage() ? Clipboard.GetImage() : null;
            if (clip == null) { SetStatus("The clipboard does not contain an image."); return; }
            using (clip)
            using (Bitmap b = ImageUtil.To32(clip))
            {
                if (!intoPaint)
                {
                    PutFrames(new[] { new SourceFrame { Bitmap = b } }, new[] { current }, "Clipboard");
                    return;
                }
                var frames = Affected().ToList();
                doc.BeginChange("Paste into paint", frames);
                Point p = doc.OffsetFor(b.Size, null);
                foreach (int i in frames)
                    using (Graphics g = Graphics.FromImage(doc.Frames[i].Paint))
                        g.DrawImage(b, p.X, p.Y, b.Width, b.Height);
                RefreshFrames(frames);
            }
        }

        private void CopyFrameToClipboard()
        {
            Bitmap b = doc.ComposeCropped(current, out _);
            if (b == null) { SetStatus("Frame is empty."); return; }
            using (b) Clipboard.SetImage(b);
            SetStatus($"Frame {current + 1} copied to the clipboard.");
        }

        #endregion

        #region Reference

        private void SetReference(ReferenceSet set)
        {
            ReferenceSet old = canvas.Reference;
            canvas.Reference = set;
            old?.Dispose();
            lbRefName.Text = set == null ? "(none)" : $"{set.Name} ({set.Frames.Count} frames)";
            numRefFrame.Maximum = Math.Max(1, set?.Frames.Count ?? 1);
            if (set != null) chkRefVisible.Checked = true;
            RefreshView();
        }

        private void LoadReferenceImages()
        {
            using (var dlg = new OpenFileDialog { Filter = ImageFilter, Multiselect = true, Title = "Reference images / GIF" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var set = new ReferenceSet { Name = dlg.FileNames.Length == 1 ? Path.GetFileName(dlg.FileName) : $"{dlg.FileNames.Length} files" };
                foreach (string f in dlg.FileNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                {
                    try { foreach (Bitmap b in ImageUtil.LoadAllFrames(f)) set.Add(b, null); }
                    catch { }
                }
                SetReference(set);
            }
        }

        /// <summary>Uses a snapshot of the current editor frames as reference (e.g. to redraw over an old version).</summary>
        private void UseFramesAsReference()
        {
            var set = new ReferenceSet { Name = "Editor snapshot" };
            for (int i = 0; i < AnimDocument.FrameCount; i++)
            {
                Bitmap b = doc.Compose(i, true);
                // stored at canvas size with the anchor as "center" so it lines up 1:1
                set.Add(b, new Point(doc.Anchor.X, doc.Anchor.Y - doc.CanvasSize.Height));
            }
            SetReference(set);
        }

        #endregion

        #region Export

        /// <summary>Union of the content of all given frames (keeps the relative alignment when cropping).</summary>
        private Rectangle UnionBounds(IEnumerable<int> frames)
        {
            Rectangle all = Rectangle.Empty;
            foreach (int i in frames)
            {
                using (Bitmap b = doc.Compose(i, true))
                {
                    Rectangle r = ImageUtil.ContentBounds(b);
                    if (r.IsEmpty) continue;
                    all = all.IsEmpty ? r : Rectangle.Union(all, r);
                }
            }
            return all;
        }

        private List<int> ExportFrames()
        {
            var frames = CheckedFrames().Where(i => !doc.IsEmpty(i)).ToList();
            if (frames.Count == 0)
                MessageBox.Show(this, "No checked frames with content.\nKeine angehakten Frames mit Inhalt.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return frames;
        }

        private static ImageFormat FormatFor(string file) =>
            string.Equals(Path.GetExtension(file), ".bmp", StringComparison.OrdinalIgnoreCase) ? ImageFormat.Bmp : ImageFormat.Png;

        private void ExportSpritesheet()
        {
            doc.ForceKeyTransparency = true;
            try
            {
                var frames = ExportFrames();
                if (frames.Count == 0) return;
                Rectangle box = UnionBounds(frames);

                using (var sfd = new SaveFileDialog { Filter = "PNG (*.png)|*.png|BMP (*.bmp)|*.bmp", FileName = "animation_sheet" })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;
                    try
                    {
                        using (var sheet = new Bitmap(box.Width * frames.Count, box.Height, PixelFormat.Format32bppArgb))
                        {
                            using (Graphics g = Graphics.FromImage(sheet))
                            {
                                for (int k = 0; k < frames.Count; k++)
                                    using (Bitmap b = doc.Compose(frames[k], true))
                                        g.DrawImage(b, new Rectangle(k * box.Width, 0, box.Width, box.Height), box, GraphicsUnit.Pixel);
                            }
                            sheet.Save(sfd.FileName, FormatFor(sfd.FileName));
                        }
                        SetStatus($"Spritesheet saved: {frames.Count} frames, {box.Width}x{box.Height} each.");
                    }
                    catch (Exception ex) { ShowError("Export failed", ex); }
                }
            }
            finally { doc.ForceKeyTransparency = false; }
        }

        private void ExportFramesSeparate()
        {
            doc.ForceKeyTransparency = true;
            try
            {
                var frames = ExportFrames();
                if (frames.Count == 0) return;
                Rectangle box = UnionBounds(frames);

                using (var fbd = new FolderBrowserDialog { Description = "Folder for the frame PNGs" })
                {
                    if (fbd.ShowDialog(this) != DialogResult.OK) return;
                    try
                    {
                        foreach (int i in frames)
                        {
                            using (Bitmap b = doc.Compose(i, true))
                            using (Bitmap crop = b.Clone(box, PixelFormat.Format32bppArgb))
                                crop.Save(Path.Combine(fbd.SelectedPath, $"Frame_{i + 1:D2}.png"), ImageFormat.Png);
                        }
                        SetStatus($"{frames.Count} frames saved to {fbd.SelectedPath} ({box.Width}x{box.Height}, anchor at {doc.Anchor.X - box.X},{doc.Anchor.Y - box.Y}).");
                    }
                    catch (Exception ex) { ShowError("Export failed", ex); }
                }
            }
            finally { doc.ForceKeyTransparency = false; }
        }

        private void ExportGif()
        {
            doc.ForceKeyTransparency = true;
            try
            {
                var frames = ExportFrames();
                if (frames.Count == 0) return;
                Rectangle box = UnionBounds(frames);

                using (var sfd = new SaveFileDialog { Filter = "GIF (*.gif)|*.gif", FileName = "animation" })
                {
                    if (sfd.ShowDialog(this) != DialogResult.OK) return;
                    try
                    {
                        int delay = Math.Max(10, 1000 / (int)numFps.Value);
                        using (var gif = AnimatedGif.AnimatedGif.Create(sfd.FileName, delay))
                        {
                            bool bg = canvas.BackgroundMode != CanvasBackground.Checker;
                            foreach (int i in frames)
                            {
                                using (Bitmap comp = canvas.RenderComposite(i, bg, false))
                                using (var crop = new Bitmap(box.Width, box.Height, PixelFormat.Format32bppArgb))
                                {
                                    using (Graphics g = Graphics.FromImage(crop))
                                    {
                                        if (!bg) g.Clear(Color.White);
                                        g.DrawImage(comp, new Rectangle(0, 0, box.Width, box.Height), box, GraphicsUnit.Pixel);
                                    }
                                    gif.AddFrame(crop, delay, AnimatedGif.GifQuality.Bit8);
                                }
                            }
                        }
                        SetStatus($"GIF saved: {frames.Count} frames, {1000 / delay} fps.");
                    }
                    catch (Exception ex) { ShowError("GIF export failed", ex); }
                }
            }
            finally { doc.ForceKeyTransparency = false; }
        }

        #endregion

        #region Project save / load

        private sealed class ProjectFrame
        {
            public bool Enabled { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }
            public string Source { get; set; }
            public bool OriginalVisible { get; set; }
            public bool PaintVisible { get; set; }
            public float OriginalOpacity { get; set; }
            public float PaintOpacity { get; set; }
            public bool HasOriginal { get; set; }
        }

        private sealed class ProjectFile
        {
            public int Version { get; set; } = 1;
            public int CanvasWidth { get; set; }
            public int CanvasHeight { get; set; }
            public int AnchorX { get; set; }
            public int AnchorY { get; set; }
            public int Fps { get; set; }
            public List<ProjectFrame> Frames { get; set; } = new List<ProjectFrame>();
            public List<ProjectOverlay> Overlays { get; set; }
            public bool KeyColorsTransparent { get; set; }
        }

        private void SaveProject()
        {
            using (var fbd = new FolderBrowserDialog { Description = "Project folder (frames are saved as PNG + project.json)" })
            {
                if (lastProjectFolder != null) fbd.SelectedPath = lastProjectFolder;
                if (fbd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var p = new ProjectFile
                    {
                        CanvasWidth = doc.CanvasSize.Width,
                        CanvasHeight = doc.CanvasSize.Height,
                        AnchorX = doc.Anchor.X,
                        AnchorY = doc.Anchor.Y,
                        Fps = (int)numFps.Value
                    };
                    for (int i = 0; i < AnimDocument.FrameCount; i++)
                    {
                        AnimFrame f = doc.Frames[i];
                        p.Frames.Add(new ProjectFrame
                        {
                            Enabled = f.Enabled,
                            OffsetX = f.Offset.X,
                            OffsetY = f.Offset.Y,
                            Source = f.Source,
                            OriginalVisible = f.OriginalVisible,
                            PaintVisible = f.PaintVisible,
                            OriginalOpacity = f.OriginalOpacity,
                            PaintOpacity = f.PaintOpacity,
                            HasOriginal = f.HasOriginal
                        });
                        string orig = Path.Combine(fbd.SelectedPath, $"frame_{i + 1:D2}_original.png");
                        if (f.Original != null) f.Original.Save(orig, ImageFormat.Png);
                        else if (File.Exists(orig)) File.Delete(orig);
                        f.Paint.Save(Path.Combine(fbd.SelectedPath, $"frame_{i + 1:D2}_paint.png"), ImageFormat.Png);
                    }
                    p.Overlays = SaveOverlays(fbd.SelectedPath);
                    p.KeyColorsTransparent = doc.KeyColorsTransparent;
                    File.WriteAllText(Path.Combine(fbd.SelectedPath, "project.json"),
                        JsonSerializer.Serialize(p, new JsonSerializerOptions { WriteIndented = true }));
                    lastProjectFolder = fbd.SelectedPath;
                    SetStatus("Project saved: " + fbd.SelectedPath);
                }
                catch (Exception ex) { ShowError("Project could not be saved", ex); }
            }
        }

        private void OpenProject()
        {
            using (var fbd = new FolderBrowserDialog { Description = "Project folder containing project.json" })
            {
                if (lastProjectFolder != null) fbd.SelectedPath = lastProjectFolder;
                if (fbd.ShowDialog(this) != DialogResult.OK) return;
                string json = Path.Combine(fbd.SelectedPath, "project.json");
                if (!File.Exists(json)) { MessageBox.Show(this, "project.json not found.", "Open project"); return; }
                try
                {
                    ProjectFile p = JsonSerializer.Deserialize<ProjectFile>(File.ReadAllText(json));
                    StopPlay();
                    doc.BeginChangeAll("Open project");
                    doc.ResizeCanvas(p.CanvasWidth, p.CanvasHeight);
                    doc.SetAnchor(new Point(p.AnchorX, p.AnchorY));
                    for (int i = 0; i < AnimDocument.FrameCount && i < p.Frames.Count; i++)
                    {
                        ProjectFrame pf = p.Frames[i];
                        AnimFrame f = doc.Frames[i];
                        doc.ClearFrame(i);
                        string orig = Path.Combine(fbd.SelectedPath, $"frame_{i + 1:D2}_original.png");
                        string paint = Path.Combine(fbd.SelectedPath, $"frame_{i + 1:D2}_paint.png");
                        if (pf.HasOriginal && File.Exists(orig)) f.Original = ImageUtil.LoadCopy(orig);
                        if (File.Exists(paint))
                        {
                            using (Bitmap pb = ImageUtil.LoadCopy(paint))
                            using (Graphics g = Graphics.FromImage(f.Paint))
                                g.DrawImage(pb, 0, 0, pb.Width, pb.Height);
                        }
                        f.Offset = new Point(pf.OffsetX, pf.OffsetY);
                        f.Enabled = pf.Enabled;
                        f.Source = pf.Source ?? "";
                        f.OriginalVisible = pf.OriginalVisible;
                        f.PaintVisible = pf.PaintVisible;
                        f.OriginalOpacity = pf.OriginalOpacity;
                        f.PaintOpacity = pf.PaintOpacity;
                    }
                    if (p.Fps > 0) numFps.Value = Math.Min(numFps.Maximum, p.Fps);
                    LoadOverlays(fbd.SelectedPath, p.Overlays);
                    btKeyTransparent.Checked = p.KeyColorsTransparent;
                    lastProjectFolder = fbd.SelectedPath;
                    RefreshAllFrames();
                    ZoomToFit();
                    SetStatus("Project opened: " + fbd.SelectedPath);
                }
                catch (Exception ex) { ShowError("Project could not be opened", ex); }
            }
        }

        #endregion

        #region Keyboard

        private static Control FocusedControl(Control c)
        {
            while (c is ContainerControl cc && cc.ActiveControl != null) c = cc.ActiveControl;
            return c;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Control focused = FocusedControl(this);
            bool typing = focused is TextBoxBase || focused is NumericUpDown || focused?.Parent is NumericUpDown ||
                          focused is ComboBox || focused is ListBox || focused is TrackBar;
            bool editor = mainTabs.SelectedTab == tabEditor;

            if (keyData == Keys.F1)
            {
                AnimEditHelp.Show(this, System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");
                return true;
            }

            if (!typing && editor && HandleEditorKey(keyData, focused == canvas)) return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool HandleEditorKey(Keys keyData, bool canvasFocused)
        {
            Keys key = keyData & Keys.KeyCode;
            bool ctrl = keyData.HasFlag(Keys.Control);
            bool shift = keyData.HasFlag(Keys.Shift);

            if (ctrl)
            {
                switch (key)
                {
                    case Keys.Z when shift: DoRedo(); return true;
                    case Keys.Z: DoUndo(); return true;
                    case Keys.Y: DoRedo(); return true;
                    case Keys.C: CopyFrameToClipboard(); return true;
                    case Keys.V when !canvas.Selection.IsEmpty: PasteIntoSelection(); return true;
                    case Keys.V: PasteFromClipboard(false); return true;
                    case Keys.Left when shift: MoveFrame(-1); return true;
                    case Keys.Right when shift: MoveFrame(1); return true;
                }
                return false;
            }
            if (keyData.HasFlag(Keys.Alt)) return false;

            int step = shift ? 10 : 1;
            bool overlayMode = canvas.Tool == DrawTool.Overlay && SelOverlay != null;
            switch (key)
            {
                case Keys.Left when overlayMode: NudgeOverlay(-step, 0); return true;
                case Keys.Right when overlayMode: NudgeOverlay(step, 0); return true;
                case Keys.Up when overlayMode: NudgeOverlay(0, -step); return true;
                case Keys.Down when overlayMode: NudgeOverlay(0, step); return true;
                case Keys.Delete when overlayMode: DeleteOverlay(); return true;
                case Keys.Escape when overlayMode: canvas.SelectedOverlay = null; return true;
                case Keys.P: SetTool(DrawTool.Overlay); return true;
                case Keys.Left: Nudge(-step, 0); return true;
                case Keys.Right: Nudge(step, 0); return true;
                case Keys.Up: Nudge(0, -step); return true;
                case Keys.Down: Nudge(0, step); return true;
                case Keys.PageUp: StepFrame(-1); return true;
                case Keys.PageDown: StepFrame(1); return true;
                case Keys.Space: TogglePlay(); return true;
                case Keys.Delete when !canvas.Selection.IsEmpty: ClearSelectionArea(); return true;
                case Keys.Delete: ClearTargetLayer(); return true;
                case Keys.Escape when !canvas.Selection.IsEmpty: canvas.Selection = Rectangle.Empty; return true;
                case Keys.S: SetTool(DrawTool.Select); return true;
                case Keys.T: btKeyTransparent.Checked = !btKeyTransparent.Checked; return true;
                case Keys.Tab when canvasFocused: cbLayer.SelectedIndex = 1 - cbLayer.SelectedIndex; return true;
                case Keys.B: SetTool(DrawTool.Pencil); return true;
                case Keys.E: SetTool(DrawTool.Eraser); return true;
                case Keys.L: SetTool(DrawTool.Line); return true;
                case Keys.R: SetTool(shift ? DrawTool.FilledRectangle : DrawTool.Rectangle); return true;
                case Keys.O: SetTool(shift ? DrawTool.FilledEllipse : DrawTool.Ellipse); return true;
                case Keys.G: SetTool(DrawTool.Fill); return true;
                case Keys.I: SetTool(DrawTool.Pipette); return true;
                case Keys.M: SetTool(DrawTool.Move); return true;
                case Keys.X: SwapColors(); return true;
                case Keys.Oemplus:
                case Keys.Add: SetZoom(canvas.Zoom + 1); return true;
                case Keys.OemMinus:
                case Keys.Subtract: SetZoom(canvas.Zoom - 1); return true;
                case Keys.OemOpenBrackets: numBrush.Value = Math.Max(numBrush.Minimum, numBrush.Value - 1); return true;
                case Keys.OemCloseBrackets: numBrush.Value = Math.Min(numBrush.Maximum, numBrush.Value + 1); return true;
            }

            if (key >= Keys.D0 && key <= Keys.D9)
            {
                int n = key == Keys.D0 ? 9 : key - Keys.D1;
                SelectFrame(n);
                return true;
            }
            return false;
        }

        #endregion

        private void ShowError(string text, Exception ex)
        {
            MessageBox.Show(this, text + ":\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
