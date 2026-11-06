using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
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

            InitDesignerState();
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
    

        // =====================================================================
        // === BROWSERS
        // =====================================================================

        #region Helpers

        /// <summary>Runs work on a thread with a big stack (the Ultima SDK has deep call chains).</summary>
        private static Task<T> RunBigStack<T>(Func<T> work)
        {
            var tcs = new TaskCompletionSource<T>();
            var thread = new Thread(() =>
            {
                try { tcs.SetResult(work()); }
                catch (Exception ex) { tcs.SetException(ex); }
            }, 16 * 1024 * 1024) { IsBackground = true };
            thread.Start();
            return tcs.Task;
        }




        #endregion

        // =====================================================================
        // === ANIMATION BROWSER ===============================================
        // =====================================================================

        #region Anim browser - fields and layout

        private const int SourceAuto = 0, SourceUop = 6, SourceVd = 7, SourceFolder = 8, SourceImages = 9;

        private Timer browserTimer;

        private AnimSource source;
        private int sourceIndex = SourceAuto;
        private int bodiesVersion;
        private List<SourceFrame> browserFrames = new List<SourceFrame>();
        private int browserPos;
        private bool browserBusy;


        #endregion

        #region Anim browser - logic

        private enum BrowserTarget { Checked, FromCurrent, Current }

        private void InitAnimBrowser()
        {
            browserTimer = new Timer(components) { Interval = 150 };
            browserTimer.Tick += (s, e) =>
            {
                if (browserFrames.Count == 0) return;
                browserPos = (browserPos + 1) % browserFrames.Count;
                ShowBrowserFrame(browserPos);
            };

            cbSource.SelectedIndex = 0;
            source = new AutoSource();
            cbSource.SelectedIndexChanged += async (s, e) => await OnSourceChanged();
            lbBodies.SelectedIndexChanged += (s, e) => OnBodySelected();
            lbActions.SelectedIndexChanged += (s, e) => LoadBrowserFrames();
            lbDirections.SelectedIndexChanged += (s, e) => LoadBrowserFrames();
            numHue.ValueChanged += (s, e) => LoadBrowserFrames();
            lbFrames.SelectedIndexChanged += (s, e) => { if (!browserTimer.Enabled) ShowBrowserFrame(lbFrames.SelectedIndex); };
            numBrowserZoom.ValueChanged += (s, e) => { browserPreview.Zoom = (int)numBrowserZoom.Value; browserPreview.Invalidate(); };
            numBrowserSpeed.ValueChanged += (s, e) => browserTimer.Interval = (int)numBrowserSpeed.Value;
            tbBodySearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                SearchBody();
            };
        }

        /// <summary>Programmatic source selection (e.g. from the File menu).</summary>
        private void SelectSourceIndex(int index)
        {
            if (cbSource.SelectedIndex == index) _ = OnSourceChanged();
            else cbSource.SelectedIndex = index;
        }

        private AnimSource CreateSource(int index)
        {
            switch (index)
            {
                case SourceAuto: return new AutoSource();
                case 1: case 2: case 3: case 4: case 5: return new MulSource(index);
                case SourceUop: return new UopSource();
                case SourceVd:
                    using (var dlg = new OpenFileDialog { Filter = "VD animation (*.vd)|*.vd|All files (*.*)|*.*", Title = "Open VD file" })
                        return dlg.ShowDialog(this) == DialogResult.OK ? new VdSource(dlg.FileName) : null;
                case SourceFolder:
                    using (var fbd = new FolderBrowserDialog { Description = "Folder with anim.idx/anim.mul (anim2…anim5 too)" })
                        return fbd.ShowDialog(this) == DialogResult.OK ? new ExternalMulSource(fbd.SelectedPath) : null;
                case SourceImages:
                    using (var dlg = new OpenFileDialog { Filter = ImageFilter, Multiselect = true, Title = "Images or animated GIF" })
                        return dlg.ShowDialog(this) == DialogResult.OK ? new ImageFilesSource(dlg.FileNames) : null;
            }
            return null;
        }

        private async Task OnSourceChanged()
        {
            if (browserBusy) return;
            AnimSource next = CreateSource(cbSource.SelectedIndex);
            if (next == null)
            {
                // dialog cancelled -> back to the previous source
                browserBusy = true;
                cbSource.SelectedIndex = sourceIndex;
                browserBusy = false;
                return;
            }

            AnimSource old = source;
            source = next;
            sourceIndex = cbSource.SelectedIndex;
            if (old != null) lock (old.SyncRoot) old.Dispose();

            numHue.Enabled = source.SupportsHue;
            lbDirections.Enabled = source.SupportsDirections;
            await LoadBrowserBodiesAsync();
        }

        private async Task LoadBrowserBodiesAsync()
        {
            int version = ++bodiesVersion;
            AnimSource src = source;
            StopBrowserPlay();
            lbBodies.Items.Clear();
            lbActions.Items.Clear();
            ClearBrowserFrames();
            lbBrowserInfo.Text = $"Loading {src.Name} …";

            List<ListEntry> bodies;
            try
            {
                bodies = await RunBigStack(() => { lock (src.SyncRoot) return src.LoadBodies(); });
            }
            catch (Exception ex)
            {
                if (!IsDisposed && version == bodiesVersion) lbBrowserInfo.Text = "Load error: " + ex.Message;
                return;
            }
            if (IsDisposed || version != bodiesVersion) return;

            lbBodies.BeginUpdate();
            lbBodies.Items.AddRange(bodies.Cast<object>().ToArray());
            lbBodies.EndUpdate();

            string err = src is UopSource u && u.LoadError != null ? " – " + u.LoadError : "";
            lbBrowserInfo.Text = $"{src.Name}: {bodies.Count} entries{err}. Select Body → Action → Direction.";
            if (lbBodies.Items.Count == 1) lbBodies.SelectedIndex = 0;
        }

        private void OnBodySelected()
        {
            if (!(lbBodies.SelectedItem is ListEntry body)) return;
            List<ListEntry> actions;
            try { lock (source.SyncRoot) actions = source.GetActions(body.Id); }
            catch (Exception ex) { lbBrowserInfo.Text = "Error: " + ex.Message; return; }

            lbActions.BeginUpdate();
            lbActions.Items.Clear();
            lbActions.Items.AddRange(actions.Cast<object>().ToArray());
            lbActions.EndUpdate();
            if (lbActions.Items.Count > 0) lbActions.SelectedIndex = 0;
            else { ClearBrowserFrames(); lbBrowserInfo.Text = "No actions found for this body."; }
        }

        private void ClearBrowserFrames()
        {
            browserPreview.Image = null;
            foreach (SourceFrame f in browserFrames) f.Bitmap?.Dispose();
            browserFrames = new List<SourceFrame>();
            lbFrames.Items.Clear();
        }

        private void LoadBrowserFrames()
        {
            if (!(lbBodies.SelectedItem is ListEntry body) || !(lbActions.SelectedItem is ListEntry action)) return;
            bool wasPlaying = browserTimer.Enabled;
            StopBrowserPlay();
            ClearBrowserFrames();

            int dir = Math.Max(0, lbDirections.SelectedIndex);
            try
            {
                lock (source.SyncRoot) browserFrames = source.GetFrames(body.Id, action.Id, dir, (int)numHue.Value);
            }
            catch (Exception ex)
            {
                lbBrowserInfo.Text = "Error: " + ex.Message;
                return;
            }

            for (int i = 0; i < browserFrames.Count; i++)
            {
                Bitmap b = browserFrames[i].Bitmap;
                lbFrames.Items.Add(b == null ? $"Frame {i + 1} (empty)" : $"Frame {i + 1}  ({b.Width}x{b.Height})");
            }
            numBrowserStart.Maximum = Math.Max(1, browserFrames.Count);

            if (browserFrames.Count == 0)
            {
                lbBrowserInfo.Text = "No frames found.";
                return;
            }
            lbFrames.SelectedIndex = 0;
            if (wasPlaying) ToggleBrowserPlay();
        }

        private string BrowserSourceName()
        {
            string body = (lbBodies.SelectedItem as ListEntry)?.Label ?? "?";
            int action = (lbActions.SelectedItem as ListEntry)?.Id ?? 0;
            return $"{source.Name} | {body.Split(new[] { "  " }, StringSplitOptions.None)[0]} A{action} D{Math.Max(0, lbDirections.SelectedIndex)}";
        }

        private void ShowBrowserFrame(int i)
        {
            if (i < 0 || i >= browserFrames.Count) return;
            SourceFrame f = browserFrames[i];
            browserPreview.Image = f.Bitmap;
            browserPreview.Caption = $"{i + 1}/{browserFrames.Count}";
            string center = f.Center.HasValue ? $" | center {f.Center.Value.X},{f.Center.Value.Y}" : "";
            string size = f.Bitmap != null ? $"{f.Bitmap.Width}x{f.Bitmap.Height}" : "empty";
            lbBrowserInfo.Text = $"{BrowserSourceName()} | frame {i + 1}/{browserFrames.Count} | {size}{center}";
        }

        private void ToggleBrowserPlay()
        {
            if (browserTimer.Enabled) { StopBrowserPlay(); return; }
            if (browserFrames.Count == 0) return;
            browserPos = Math.Max(0, lbFrames.SelectedIndex);
            browserTimer.Interval = (int)numBrowserSpeed.Value;
            browserTimer.Start();
            btBrowserPlay.BackColor = Color.LightGreen;
        }

        private void StopBrowserPlay()
        {
            browserTimer?.Stop();
            if (btBrowserPlay != null)
            {
                btBrowserPlay.BackColor = SystemColors.Control;
                btBrowserPlay.UseVisualStyleBackColor = true;
            }
        }

        /// <summary>Source frames picked by Start and Step.</summary>
        private List<SourceFrame> PickBrowserFrames()
        {
            var list = new List<SourceFrame>();
            int step = (int)numBrowserStep.Value;
            for (int i = (int)numBrowserStart.Value - 1; i < browserFrames.Count; i += step)
                list.Add(browserFrames[i]);
            return list;
        }

        private void BrowserToEditor(BrowserTarget target)
        {
            if (browserFrames.Count == 0) { lbBrowserInfo.Text = "No frames loaded."; return; }
            List<int> targets;
            List<SourceFrame> frames;
            switch (target)
            {
                case BrowserTarget.Current:
                    int sel = Math.Max(0, lbFrames.SelectedIndex);
                    frames = new List<SourceFrame> { browserFrames[sel] };
                    targets = new List<int> { current };
                    break;
                case BrowserTarget.FromCurrent:
                    frames = PickBrowserFrames();
                    targets = Enumerable.Range(current, AnimDocument.FrameCount - current).ToList();
                    break;
                default:
                    frames = PickBrowserFrames();
                    targets = CheckedFrames();
                    if (targets.Count == 0) { lbBrowserInfo.Text = "Check at least one frame in the frame strip."; return; }
                    break;
            }
            PutFrames(frames, targets, BrowserSourceName());
            if (frames.Count > targets.Count)
                SetStatus($"Only {targets.Count} of {frames.Count} frames fit (10 frame limit / checked frames). Use Start/Step to choose.");
            mainTabs.SelectedTab = tabEditor;
        }

        private void BrowserAsReference()
        {
            if (browserFrames.Count == 0) return;
            var set = new ReferenceSet { Name = BrowserSourceName() };
            foreach (SourceFrame f in PickBrowserFrames())
                set.Add(f.Bitmap != null ? new Bitmap(f.Bitmap) : null, f.Center);
            SetReference(set);
            SetStatus("Reference set: " + set.Name);
            mainTabs.SelectedTab = tabEditor;
        }

        private void BrowserCopyFrame()
        {
            int i = lbFrames.SelectedIndex;
            if (i < 0 || browserFrames[i].Bitmap == null) return;
            Clipboard.SetImage(browserFrames[i].Bitmap);
            lbBrowserInfo.Text = "Frame copied to the clipboard.";
        }

        private void BrowserExport()
        {
            if (browserFrames.Count == 0) return;
            using (var fbd = new FolderBrowserDialog { Description = "Export folder for the animation frames" })
            {
                if (fbd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    string prefix = string.Concat(BrowserSourceName().Split(Path.GetInvalidFileNameChars())).Replace(" ", "").Replace("|", "_");
                    int n = 0;
                    for (int i = 0; i < browserFrames.Count; i++)
                    {
                        if (browserFrames[i].Bitmap == null) continue;
                        browserFrames[i].Bitmap.Save(Path.Combine(fbd.SelectedPath, $"{prefix}_F{i:D3}.png"), ImageFormat.Png);
                        n++;
                    }
                    lbBrowserInfo.Text = $"{n} frames exported to {fbd.SelectedPath}";
                }
                catch (Exception ex) { ShowError("Export failed", ex); }
            }
        }

        private void SearchBody()
        {
            string q = tbBodySearch.Text.Trim();
            if (q.Length == 0 || lbBodies.Items.Count == 0) return;
            bool isNum = int.TryParse(q, out int id);
            int start = lbBodies.SelectedIndex + 1;
            for (int k = 0; k < lbBodies.Items.Count; k++)
            {
                int i = (start + k) % lbBodies.Items.Count;
                var e = (ListEntry)lbBodies.Items[i];
                bool hit = isNum ? e.Id == id || e.Id % 100000 == id : e.Label.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!hit) continue;
                lbBodies.SelectedIndex = i;
                lbBodies.TopIndex = Math.Max(0, i - 3);
                return;
            }
            lbBrowserInfo.Text = $"'{q}' not found.";
        }

        private void CloseBrowsers()
        {
            bodiesVersion++;
            artLoadVersion++;
            browserTimer?.Stop();
            clipTimer?.Stop();
            ClearBrowserFrames();
            if (source != null) lock (source.SyncRoot) source.Dispose();
            artPreview.Image = null;
            artPreviewImage?.Dispose();
            foreach (ClipItem c in clipItems) c.Dispose();
            clipItems.Clear();
        }

        #endregion

        // =====================================================================
        // === ART BROWSER =====================================================
        // =====================================================================

        #region Art browser

        private const int ArtIdLimit = 0x14000;

        private readonly List<int> artIds = new List<int>();
        private int artLoadVersion;
        private Bitmap artPreviewImage;


        private void InitArtBrowser()
        {
            lbArt.SelectedIndexChanged += (s, e) => ShowArt();
            numArtZoom.ValueChanged += (s, e) => { artPreview.Zoom = (int)numArtZoom.Value; artPreview.Invalidate(); };
            rbStatic.CheckedChanged += async (s, e) => { if (rbStatic.Checked) await LoadArtListAsync(); };
            rbLand.CheckedChanged += async (s, e) => { if (rbLand.Checked) await LoadArtListAsync(); };
            tbArtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                ArtSearch();
            };
        }

        private async Task LoadArtListAsync()
        {
            int version = ++artLoadVersion;
            bool isStatic = rbStatic.Checked;
            lbArtInfo.Text = "Loading…";
            lbArt.Items.Clear();
            artIds.Clear();

            List<int> ids;
            try
            {
                ids = await RunBigStack(() =>
                {
                    lock (SdkSync.Lock)
                    {
                        var list = new List<int>();
                        int limit = isStatic ? Math.Min(ArtIdLimit, Art.GetMaxItemId() + 1) : 0x4000;
                        for (int i = 0; i < limit; i++)
                        {
                            try { if (isStatic ? Art.IsValidStatic(i) : Art.IsValidLand(i)) list.Add(i); }
                            catch { }
                        }
                        return list;
                    }
                });
            }
            catch (Exception ex)
            {
                if (!IsDisposed && version == artLoadVersion) lbArtInfo.Text = "Art load error: " + ex.Message;
                return;
            }
            if (IsDisposed || version != artLoadVersion) return;

            artIds.AddRange(ids);
            lbArt.BeginUpdate();
            lbArt.Items.AddRange(ids.Select(id => (object)$"0x{id:X4} ({id})").ToArray());
            lbArt.EndUpdate();
            lbArtInfo.Text = $"{ids.Count} {(isStatic ? "statics" : "land tiles")}";
        }

        /// <summary>The original (not zoomed) art bitmap. It comes from the SDK cache: never dispose it.</summary>
        private Bitmap SelectedArt(out int id)
        {
            id = -1;
            int sel = lbArt.SelectedIndex;
            if (sel < 0 || sel >= artIds.Count) return null;
            id = artIds[sel];
            lock (SdkSync.Lock) return rbStatic.Checked ? Art.GetStatic(id) : Art.GetLand(id);
        }

        private void ShowArt()
        {
            try
            {
                Bitmap b = SelectedArt(out int id);
                Bitmap old = artPreviewImage;
                artPreviewImage = b != null ? ImageUtil.To32(b) : null;
                artPreview.Image = artPreviewImage;
                old?.Dispose();
                lbArtInfo.Text = b != null
                    ? $"ID 0x{id:X4} ({id}) | {b.Width}x{b.Height} | {(rbStatic.Checked ? "Static" : "Land")}"
                    : $"ID 0x{id:X4} – no image";
            }
            catch (Exception ex) { lbArtInfo.Text = "Error: " + ex.Message; }
        }

        private void ArtSearch()
        {
            string q = tbArtSearch.Text.Trim();
            if (q.Length == 0) return;
            bool ok = q.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? int.TryParse(q.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int id)
                : int.TryParse(q, out id);
            if (!ok) { lbArtInfo.Text = "Enter a decimal ID or hex (0x…)."; return; }
            int idx = artIds.IndexOf(id);
            if (idx < 0) { lbArtInfo.Text = $"Art {q} not found."; return; }
            lbArt.SelectedIndex = idx;
            lbArt.TopIndex = Math.Max(0, idx - 3);
        }

        private void ArtToEditor(bool allChecked)
        {
            Bitmap b = SelectedArt(out int id);
            if (b == null) return;
            using (Bitmap copy = ImageUtil.To32(b))
            {
                var targets = allChecked ? CheckedFrames() : new List<int> { current };
                if (targets.Count == 0) { lbArtInfo.Text = "Check at least one frame."; return; }
                PutFrames(targets.Select(_ => new SourceFrame { Bitmap = copy }).ToList(), targets,
                    $"{(rbStatic.Checked ? "Static" : "Land")} 0x{id:X4}");
            }
            mainTabs.SelectedTab = tabEditor;
        }

        private void ArtAsReference()
        {
            Bitmap b = SelectedArt(out int id);
            if (b == null) return;
            var set = new ReferenceSet { Name = $"Art 0x{id:X4}" };
            set.Add(ImageUtil.To32(b), null);
            SetReference(set);
            mainTabs.SelectedTab = tabEditor;
        }

        private void ArtExport()
        {
            Bitmap b = SelectedArt(out int id);
            if (b == null) return;
            using (var sfd = new SaveFileDialog { Filter = "PNG (*.png)|*.png|BMP (*.bmp)|*.bmp", FileName = $"Art_0x{id:X4}" })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    using (Bitmap copy = ImageUtil.To32(b)) copy.Save(sfd.FileName, FormatFor(sfd.FileName));
                    lbArtInfo.Text = "Saved: " + sfd.FileName;
                }
                catch (Exception ex) { ShowError("Export failed", ex); }
            }
        }

        #endregion

        // =====================================================================
        // === CLIPBOARD ANIMATION =============================================
        // =====================================================================

        #region Clipboard animation

        private sealed class ClipItem : IDisposable
        {
            public string Name;
            public Bitmap Original;
            public Bitmap Shown;

            public void Dispose()
            {
                if (!ReferenceEquals(Shown, Original)) Shown?.Dispose();
                Original?.Dispose();
            }

            public override string ToString() => Name;
        }

        private const int ClipLimit = 60;
        private readonly List<ClipItem> clipItems = new List<ClipItem>();
        private Timer clipTimer;
        private int clipPos;
        private int clipCounter = 1;


        private void InitClipboardTab()
        {
            clipTimer = new Timer(components);
            clipTimer.Tick += (s, e) =>
            {
                if (clipItems.Count == 0) { clipTimer.Stop(); return; }
                clipPos = (clipPos + 1) % clipItems.Count;
                clipPreview.Image = clipItems[clipPos].Shown;
                clipPreview.Caption = clipItems[clipPos].Name;
            };
            lbClip.SelectedIndexChanged += (s, e) =>
            {
                if (lbClip.SelectedItem is ClipItem c) { clipPreview.Image = c.Shown; clipPreview.Caption = c.Name; }
            };
            chkClipTransparent.CheckedChanged += (s, e) =>
            {
                clipPreview.Image = null;
                foreach (ClipItem c in clipItems) UpdateClipShown(c);
                if (lbClip.SelectedItem is ClipItem sel) clipPreview.Image = sel.Shown;
            };
            numClipFps.ValueChanged += (s, e) => clipTimer.Interval = 1000 / (int)numClipFps.Value;
        }

        private void UpdateClipShown(ClipItem c)
        {
            if (!ReferenceEquals(c.Shown, c.Original)) c.Shown?.Dispose();
            c.Shown = chkClipTransparent.Checked
                ? ImageUtil.MakeTransparent(c.Original, new[] { Color.Black, Color.White })
                : c.Original;
        }

        private void AddClipItem(Bitmap bmp, string name)
        {
            if (clipItems.Count >= ClipLimit) { MessageBox.Show(this, $"Maximum of {ClipLimit} images reached."); bmp.Dispose(); return; }
            var item = new ClipItem { Name = name, Original = bmp };
            UpdateClipShown(item);
            clipItems.Add(item);
            lbClip.Items.Add(item);
            lbClip.SelectedItem = item;
        }

        private void AddClipboardImage()
        {
            Image img = Clipboard.ContainsImage() ? Clipboard.GetImage() : null;
            if (img == null) { MessageBox.Show(this, "The clipboard does not contain an image."); return; }
            using (img) AddClipItem(ImageUtil.To32(img), $"Item{clipCounter++:00}");
        }

        private void AddCurrentFrameToClipboardList()
        {
            Bitmap b = doc.ComposeCropped(current, out _);
            if (b == null) { SetStatus("Frame is empty."); return; }
            AddClipItem(b, $"Frame{current + 1}_{clipCounter++:00}");
            SetStatus("Frame added to the clipboard animation list.");
        }

        private void ReplaceClipItem()
        {
            if (!(lbClip.SelectedItem is ClipItem c)) return;
            Image img = Clipboard.ContainsImage() ? Clipboard.GetImage() : null;
            if (img == null) { MessageBox.Show(this, "The clipboard does not contain an image."); return; }
            clipPreview.Image = null;
            if (!ReferenceEquals(c.Shown, c.Original)) c.Shown?.Dispose();
            c.Original.Dispose();
            using (img) c.Original = ImageUtil.To32(img);
            c.Shown = null;
            UpdateClipShown(c);
            clipPreview.Image = c.Shown;
        }

        private void DeleteClipItem()
        {
            if (!(lbClip.SelectedItem is ClipItem c)) return;
            clipPreview.Image = null;
            clipItems.Remove(c);
            lbClip.Items.Remove(c);
            c.Dispose();
        }

        private void DeleteAllClipItems()
        {
            clipTimer.Stop();
            clipPreview.Image = null;
            foreach (ClipItem c in clipItems) c.Dispose();
            clipItems.Clear();
            lbClip.Items.Clear();
        }

        private void PlayClip()
        {
            if (clipItems.Count == 0) return;
            clipPos = -1;
            clipTimer.Interval = 1000 / (int)numClipFps.Value;
            clipTimer.Start();
        }

        private void ClipToEditor()
        {
            if (clipItems.Count == 0) return;
            int start = lbClip.SelectedIndex >= 0 ? lbClip.SelectedIndex : 0;
            var frames = clipItems.Skip(start).Select(c => new SourceFrame { Bitmap = c.Shown }).ToList();
            PutFrames(frames, Enumerable.Range(current, AnimDocument.FrameCount - current).ToList(), "Clipboard list");
            mainTabs.SelectedTab = tabEditor;
        }

        #endregion
    

        // =====================================================================
        // === OVERLAYS
        // =====================================================================

        private bool updatingOverlayUi;

        private Overlay SelOverlay => canvas.SelectedOverlay;

        private void WireOverlays()
        {
            canvas.OverlaySelected += o => { SyncOverlayList(); LoadOverlayUi(); };
            canvas.OverlayMoved += o => LoadOverlayUi();

            lbOverlays.SelectedIndexChanged += (s, e) =>
            {
                if (updatingOverlayUi) return;
                int i = lbOverlays.SelectedIndex;
                canvas.SelectedOverlay = i >= 0 && i < doc.Overlays.Count ? doc.Overlays[i] : null;
            };

            numOvX.ValueChanged += (s, e) => ChangeOverlay("Overlay position", o => o.Position = new Point((int)numOvX.Value, o.Position.Y), false);
            numOvY.ValueChanged += (s, e) => ChangeOverlay("Overlay position", o => o.Position = new Point(o.Position.X, (int)numOvY.Value), false);
            numOvScale.ValueChanged += (s, e) => ChangeOverlay("Overlay scale", o => o.ScalePercent = (int)numOvScale.Value, true);
            cbOvTint.SelectedIndexChanged += (s, e) => ChangeOverlay("Overlay tint", o => o.Tint = (OverlayTint)cbOvTint.SelectedIndex, true);
            chkOvVisible.CheckedChanged += (s, e) => ChangeOverlay("Overlay visible", o => o.Visible = chkOvVisible.Checked, false);
            trkOvOpacity.ValueChanged += (s, e) => ChangeOverlay("Overlay opacity", o => o.Opacity = trkOvOpacity.Value / 100f, false);
            cbOvScope.SelectedIndexChanged += (s, e) => ChangeOverlay("Overlay frames", ApplyScope, false);
            chkOvFrames.CheckedChanged += (s, e) => { canvas.ShowOverlayFrames = chkOvFrames.Checked; canvas.Invalidate(); };

            LoadOverlayUi();
        }

        #region UI sync

        private void SyncOverlayList()
        {
            updatingOverlayUi = true;
            try
            {
                lbOverlays.BeginUpdate();
                lbOverlays.Items.Clear();
                foreach (Overlay o in doc.Overlays) lbOverlays.Items.Add(o.Describe(current));
                lbOverlays.SelectedIndex = SelOverlay != null ? doc.Overlays.IndexOf(SelOverlay) : -1;
                lbOverlays.EndUpdate();
            }
            finally { updatingOverlayUi = false; }
        }

        private void LoadOverlayUi()
        {
            updatingOverlayUi = true;
            try
            {
                Overlay o = SelOverlay;
                bool has = o != null;
                foreach (Control c in new Control[] { numOvX, numOvY, numOvScale, cbOvTint, btOvColor, cbOvScope, chkOvVisible, trkOvOpacity })
                    c.Enabled = has;
                if (!has) return;

                numOvX.Value = Math.Max(numOvX.Minimum, Math.Min(numOvX.Maximum, o.Position.X));
                numOvY.Value = Math.Max(numOvY.Minimum, Math.Min(numOvY.Maximum, o.Position.Y));
                numOvScale.Value = Math.Max(numOvScale.Minimum, Math.Min(numOvScale.Maximum, o.ScalePercent));
                cbOvTint.SelectedIndex = (int)o.Tint;
                btOvColor.BackColor = o.TintColor;
                chkOvVisible.Checked = o.Visible;
                trkOvOpacity.Value = (int)Math.Round(o.Opacity * 100);
                cbOvScope.SelectedIndex = o.Frames == null ? 0 : o.Frames.Count == 1 && o.Frames.Contains(current) ? 2 : 1;

                int idx = doc.Overlays.IndexOf(o);
                if (idx >= 0 && idx < lbOverlays.Items.Count) lbOverlays.Items[idx] = o.Describe(current);
            }
            finally { updatingOverlayUi = false; }
        }

        /// <summary>Undoable change of the selected overlay. invalidate = scale/tint changed (rebuild image).</summary>
        private void ChangeOverlay(string name, Action<Overlay> change, bool invalidate)
        {
            if (updatingOverlayUi || SelOverlay == null) return;
            doc.BeginChange(name, Array.Empty<int>(), true);
            change(SelOverlay);
            if (invalidate) SelOverlay.Invalidate();
            OverlaysChanged();
        }

        private void ApplyScope(Overlay o)
        {
            switch (cbOvScope.SelectedIndex)
            {
                case 0: o.Frames = null; break;
                case 1: o.Frames = new HashSet<int>(CheckedFrames()); break;
                default: o.Frames = new HashSet<int> { current }; break;
            }
        }

        /// <summary>Overlays affect every frame -> refresh thumbnails, canvas, preview and the list.</summary>
        private void OverlaysChanged()
        {
            strip.RefreshAll();
            canvas.Invalidate();
            UpdatePlayOrder();
            UpdatePreviewStill();
            SyncOverlayList();
            LoadOverlayUi();
        }

        #endregion

        #region Create

        /// <summary>
        /// Adds a new overlay. fitInto = scale (keeping the aspect ratio) and center into this rectangle,
        /// otherwise it is placed with its bottom center on the anchor.
        /// </summary>
        private void AddOverlay(List<Bitmap> images, string name, Rectangle? fitInto)
        {
            if (images == null || images.Count == 0) return;
            if (chkAutoCrop.Checked)
            {
                for (int k = 0; k < images.Count; k++)
                {
                    Bitmap cropped = OverlayUtil.AutoCrop(images[k]);
                    images[k].Dispose();
                    images[k] = cropped;
                }
            }

            var o = new Overlay { Name = string.IsNullOrEmpty(name) ? "Overlay " + (doc.Overlays.Count + 1) : name, Sources = images };
            ApplyScope(o);
            Size size = images[0].Size;

            if (fitInto.HasValue && !fitInto.Value.IsEmpty)
            {
                Rectangle r = fitInto.Value;
                double s = Math.Min(r.Width / (double)size.Width, r.Height / (double)size.Height);
                if (!chkSelKeepAspect.Checked) s = Math.Max(r.Width / (double)size.Width, r.Height / (double)size.Height);
                o.ScalePercent = Math.Max(10, (int)Math.Round(s * 100));
                Size sz = o.SizeFor(0);
                o.Position = new Point(r.X + (r.Width - sz.Width) / 2, r.Y + (r.Height - sz.Height) / 2);
            }
            else
            {
                o.Position = doc.OffsetFor(size, null);
            }

            doc.BeginChange("Add overlay", Array.Empty<int>());
            doc.Overlays.Add(o);
            SetTool(DrawTool.Overlay);
            canvas.SelectedOverlay = o;
            sideTabs.SelectedIndex = 0;
            OverlaysChanged();
            SetStatus($"Overlay '{o.Name}' added ({o.SizeFor(0).Width}x{o.SizeFor(0).Height}). Drag it with the Overlay tool, arrow keys = fine position. / Mit dem Overlay-Werkzeug ziehen, Pfeiltasten = fein.");
        }

        private void LoadOverlayFromFile()
        {
            using (var dlg = new OpenFileDialog { Filter = ImageFilter, Title = "Overlay image or animated GIF" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try { AddOverlay(ImageUtil.LoadAllFrames(dlg.FileName), Path.GetFileNameWithoutExtension(dlg.FileName), null); }
                catch (Exception ex) { ShowError("Image could not be loaded", ex); }
            }
        }

        private void OverlayFromClipboard()
        {
            Image clip = Clipboard.ContainsImage() ? Clipboard.GetImage() : null;
            if (clip == null) { SetStatus("The clipboard does not contain an image."); return; }
            using (clip) AddOverlay(new List<Bitmap> { ImageUtil.To32(clip) }, "Clipboard", canvas.Selection.IsEmpty ? (Rectangle?)null : canvas.Selection);
        }

        /// <summary>Copies the visible content of the marked area as an overlay at the same position (to copy it elsewhere).</summary>
        private void OverlayFromSelection()
        {
            Rectangle sel = canvas.Selection;
            if (sel.IsEmpty) { SetStatus("Mark an area first (Select tool S or Ctrl + drag)."); return; }
            Bitmap part;
            using (Bitmap full = doc.Compose(current))
                part = full.Clone(sel, PixelFormat.Format32bppArgb);
            bool crop = chkAutoCrop.Checked;
            chkAutoCrop.Checked = false; // keep the exact position
            try { AddOverlay(new List<Bitmap> { part }, $"Part F{current + 1}", null); }
            finally { chkAutoCrop.Checked = crop; }
            SelOverlay.Position = sel.Location;
            OverlaysChanged();
        }

        /// <summary>Used by the art and animation browsers ("→ Overlay").</summary>
        private void AddOverlayFromBitmaps(IEnumerable<Bitmap> bitmaps, string name)
        {
            var list = bitmaps.Where(b => b != null).Select(b => ImageUtil.To32(b)).ToList();
            if (list.Count == 0) return;
            AddOverlay(list, name, canvas.Selection.IsEmpty ? (Rectangle?)null : canvas.Selection);
            mainTabs.SelectedTab = tabEditor;
        }

        #endregion

        #region Edit

        private void PickOverlayColor()
        {
            if (SelOverlay == null) return;
            using (var dlg = new ColorDialog { Color = SelOverlay.TintColor, FullOpen = true })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK) SetOverlayTintColor(dlg.Color);
            }
        }

        private void SetOverlayTintColor(Color c)
        {
            if (SelOverlay == null) return;
            doc.BeginChange("Overlay tint", Array.Empty<int>());
            SelOverlay.TintColor = Color.FromArgb(255, c);
            SelOverlay.Tint = OverlayTint.Color;
            SelOverlay.Invalidate();
            OverlaysChanged();
        }

        private void ScaleOverlay(double factor)
        {
            if (SelOverlay == null) return;
            Overlay o = SelOverlay;
            Size before = o.SizeFor(current);
            doc.BeginChange("Overlay scale", Array.Empty<int>());
            o.ScalePercent = Math.Max(10, Math.Min(1600, (int)Math.Round(o.ScalePercent * factor)));
            o.Invalidate();
            Size after = o.SizeFor(current);
            // keep the center in place
            o.Position = new Point(o.Position.X + (before.Width - after.Width) / 2, o.Position.Y + (before.Height - after.Height) / 2);
            OverlaysChanged();
        }

        private void FitOverlayToSelection()
        {
            if (SelOverlay == null) return;
            Rectangle r = canvas.Selection;
            if (r.IsEmpty) { SetStatus("Mark an area first (Select tool S or Ctrl + drag)."); return; }
            Overlay o = SelOverlay;
            Bitmap src = o.Sources[0];
            doc.BeginChange("Overlay fit", Array.Empty<int>());
            double s = Math.Min(r.Width / (double)src.Width, r.Height / (double)src.Height);
            o.ScalePercent = Math.Max(10, (int)Math.Round(s * 100));
            o.Invalidate();
            Size sz = o.SizeFor(0);
            o.Position = new Point(r.X + (r.Width - sz.Width) / 2, r.Y + (r.Height - sz.Height) / 2);
            OverlaysChanged();
        }

        private void CenterOverlay()
        {
            if (SelOverlay == null) return;
            doc.BeginChange("Overlay center", Array.Empty<int>());
            Size sz = SelOverlay.SizeFor(current);
            SelOverlay.Position = doc.OffsetFor(sz, null);
            OverlaysChanged();
        }

        private void NudgeOverlay(int dx, int dy)
        {
            if (SelOverlay == null) return;
            doc.BeginChange("Move overlay", Array.Empty<int>(), true);
            SelOverlay.Position = new Point(SelOverlay.Position.X + dx, SelOverlay.Position.Y + dy);
            OverlaysChanged();
        }

        private void DuplicateOverlay()
        {
            if (SelOverlay == null) return;
            doc.BeginChange("Duplicate overlay", Array.Empty<int>());
            Overlay copy = SelOverlay.Clone();
            copy.Name = SelOverlay.Name + " copy";
            copy.Position = new Point(copy.Position.X + 10, copy.Position.Y + 10);
            doc.Overlays.Add(copy);
            canvas.SelectedOverlay = copy;
            OverlaysChanged();
            SetStatus("Copy created – drag it to the new place. / Kopie erstellt – an die neue Stelle ziehen.");
        }

        private void MoveOverlayOrder(int dir)
        {
            if (SelOverlay == null) return;
            int i = doc.Overlays.IndexOf(SelOverlay), j = i + dir;
            if (j < 0 || j >= doc.Overlays.Count) return;
            doc.BeginChange("Overlay order", Array.Empty<int>());
            (doc.Overlays[i], doc.Overlays[j]) = (doc.Overlays[j], doc.Overlays[i]);
            OverlaysChanged();
        }

        private void DeleteOverlay()
        {
            if (SelOverlay == null) return;
            doc.BeginChange("Delete overlay", Array.Empty<int>());
            Overlay o = SelOverlay;
            doc.Overlays.Remove(o);
            o.Invalidate();
            canvas.SelectedOverlay = null;
            OverlaysChanged();
        }

        private void BakeOverlay(bool remove)
        {
            Overlay o = SelOverlay;
            if (o == null) { SetStatus("Select an overlay first."); return; }
            var frames = Enumerable.Range(0, AnimDocument.FrameCount).Where(o.AppliesTo).ToList();
            doc.BeginChange(remove ? "Bake + remove overlay" : "Bake overlay", frames);
            foreach (int i in frames) doc.BakeOverlay(o, i);
            if (remove)
            {
                doc.Overlays.Remove(o);
                o.Invalidate();
                canvas.SelectedOverlay = null;
            }
            RefreshFrames(frames);
            OverlaysChanged();
            SetStatus($"Overlay baked into the paint layer of {frames.Count} frame(s).");
        }

        private void BakeAllOverlays()
        {
            if (doc.Overlays.Count == 0) return;
            doc.BeginChangeAll("Bake all overlays");
            foreach (Overlay o in doc.Overlays)
                for (int i = 0; i < AnimDocument.FrameCount; i++) doc.BakeOverlay(o, i);
            foreach (Overlay o in doc.Overlays) o.Invalidate();
            doc.Overlays.Clear();
            canvas.SelectedOverlay = null;
            RefreshAllFrames();
            OverlaysChanged();
        }

        #endregion

        #region Project save / load

        private sealed class ProjectOverlay
        {
            public string Name { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Scale { get; set; }
            public int Tint { get; set; }
            public int TintColor { get; set; }
            public bool Visible { get; set; }
            public float Opacity { get; set; }
            public List<int> Frames { get; set; }
            public int ImageCount { get; set; }
        }

        private List<ProjectOverlay> SaveOverlays(string folder)
        {
            var list = new List<ProjectOverlay>();
            for (int k = 0; k < doc.Overlays.Count; k++)
            {
                Overlay o = doc.Overlays[k];
                list.Add(new ProjectOverlay
                {
                    Name = o.Name, X = o.Position.X, Y = o.Position.Y, Scale = o.ScalePercent, Tint = (int)o.Tint,
                    TintColor = o.TintColor.ToArgb(), Visible = o.Visible, Opacity = o.Opacity,
                    Frames = o.Frames?.OrderBy(x => x).ToList(), ImageCount = o.Sources.Count
                });
                for (int n = 0; n < o.Sources.Count; n++)
                    o.Sources[n].Save(Path.Combine(folder, $"overlay_{k + 1:D2}_{n + 1:D2}.png"), ImageFormat.Png);
            }
            return list;
        }

        private void LoadOverlays(string folder, List<ProjectOverlay> list)
        {
            foreach (Overlay o in doc.Overlays) o.Invalidate();
            doc.Overlays.Clear();
            canvas.SelectedOverlay = null;
            if (list == null) return;
            for (int k = 0; k < list.Count; k++)
            {
                ProjectOverlay p = list[k];
                var o = new Overlay
                {
                    Name = p.Name ?? "Overlay", Position = new Point(p.X, p.Y), ScalePercent = Math.Max(10, p.Scale),
                    Tint = (OverlayTint)p.Tint, TintColor = Color.FromArgb(p.TintColor), Visible = p.Visible,
                    Opacity = p.Opacity <= 0 ? 1f : p.Opacity, Frames = p.Frames != null ? new HashSet<int>(p.Frames) : null
                };
                for (int n = 0; n < p.ImageCount; n++)
                {
                    string file = Path.Combine(folder, $"overlay_{k + 1:D2}_{n + 1:D2}.png");
                    if (File.Exists(file)) o.Sources.Add(ImageUtil.LoadCopy(file));
                }
                if (o.Sources.Count > 0) doc.Overlays.Add(o);
            }
        }

        #endregion
    

        // =====================================================================
        // === HELPERS
        // =====================================================================

        /// <summary>Tool button per drawing tool (filled in WireDesignerEvents).</summary>
        private readonly Dictionary<DrawTool, ToolStripButton> toolButtons = new Dictionary<DrawTool, ToolStripButton>();

        /// <summary>Small color square with checkerboard (for colors with alpha).</summary>
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

        /// <summary>Keeps the canvas centered in its scroll panel when it is smaller than the panel.</summary>
        private void CenterCanvas()
        {
            if (canvas == null || canvasHost == null) return;
            Point scroll = canvasHost.AutoScrollPosition;
            int x = Math.Max(0, (canvasHost.ClientSize.Width - canvas.Width) / 2);
            int y = Math.Max(0, (canvasHost.ClientSize.Height - canvas.Height) / 2);
            canvas.Location = new Point(x + scroll.X, y + scroll.Y);
        }
    

        #region Designer event handlers

        /// <summary>Start values the designer does not store (tool map, selected list entries).</summary>
        private void InitDesignerState()
        {
            toolButtons[DrawTool.Pencil] = this.btToolPen;
            toolButtons[DrawTool.Eraser] = this.btToolEraser;
            toolButtons[DrawTool.Line] = this.btToolLine;
            toolButtons[DrawTool.Rectangle] = this.btToolRect;
            toolButtons[DrawTool.FilledRectangle] = this.btToolRectFilled;
            toolButtons[DrawTool.Ellipse] = this.btToolEllipse;
            toolButtons[DrawTool.FilledEllipse] = this.btToolEllipseFilled;
            toolButtons[DrawTool.Fill] = this.btToolFill;
            toolButtons[DrawTool.Pipette] = this.btToolPick;
            toolButtons[DrawTool.Move] = this.btToolMove;
            toolButtons[DrawTool.Select] = this.btToolSelect;
            toolButtons[DrawTool.Overlay] = this.btToolOverlay;

            cbLayer.SelectedIndex = 0;
            cbSource.SelectedIndex = 0;
            lbDirections.SelectedIndex = 0;
            cbOvTint.SelectedIndex = 0;
            cbOvScope.SelectedIndex = 0;
            cbBackground.SelectedIndex = 0;
        }

        private void miLoadImages_Click(object sender, EventArgs e)
        {
            LoadImagesSequential();
        }

        private void miLoadIntoChecked_Click(object sender, EventArgs e)
        {
            LoadImageIntoChecked();
        }

        private void miLoadVd_Click(object sender, EventArgs e)
        {
            mainTabs.SelectedTab = tabAnim; SelectSourceIndex(SourceVd);
        }

        private void miOpenProject_Click(object sender, EventArgs e)
        {
            OpenProject();
        }

        private void miSaveProject_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private void miExportSheet_Click(object sender, EventArgs e)
        {
            ExportSpritesheet();
        }

        private void miExportPngs_Click(object sender, EventArgs e)
        {
            ExportFramesSeparate();
        }

        private void miExportGif_Click(object sender, EventArgs e)
        {
            ExportGif();
        }

        private void miClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void miUndo_Click(object sender, EventArgs e)
        {
            DoUndo();
        }

        private void miRedo_Click(object sender, EventArgs e)
        {
            DoRedo();
        }

        private void miCopyFrame_Click(object sender, EventArgs e)
        {
            CopyFrameToClipboard();
        }

        private void miPasteOriginal_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(false);
        }

        private void miPastePaint_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(true);
        }

        private void miLoadIntoSelection_Click(object sender, EventArgs e)
        {
            LoadImageIntoSelection();
        }

        private void miPasteIntoSelection_Click(object sender, EventArgs e)
        {
            PasteIntoSelection();
        }

        private void miCopySelection_Click(object sender, EventArgs e)
        {
            CopySelection();
        }

        private void miClearSelection_Click(object sender, EventArgs e)
        {
            ClearSelectionArea();
        }

        private void miDeselect_Click(object sender, EventArgs e)
        {
            canvas.Selection = Rectangle.Empty;
        }

        private void miClearLayer_Click(object sender, EventArgs e)
        {
            ClearTargetLayer();
        }

        private void miClearFrame_Click(object sender, EventArgs e)
        {
            ClearFrames();
        }

        private void miClearAll_Click(object sender, EventArgs e)
        {
            ClearAllFrames();
        }

        private void miMirror_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Mirror", i => doc.MirrorFrame(i));
        }

        private void miRotate_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Rotate", i => doc.RotateOriginal90(i));
        }

        private void miMerge_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Merge", i => doc.MergePaintDown(i));
        }

        private void miKeysTransparent_Click(object sender, EventArgs e)
        {
            MakeKeysTransparent();
        }

        private void miSecondaryTransparent_Click(object sender, EventArgs e)
        {
            SecondaryToTransparent();
        }

        private void miReplaceColor_Click(object sender, EventArgs e)
        {
            ReplaceSecondaryWithPrimary();
        }

        private void miOutline_Click(object sender, EventArgs e)
        {
            DrawOutline();
        }

        private void miCheckAll_Click(object sender, EventArgs e)
        {
            SetAllChecked(true);
        }

        private void miUncheckAll_Click(object sender, EventArgs e)
        {
            SetAllChecked(false);
        }

        private void miCheckNonEmpty_Click(object sender, EventArgs e)
        {
            CheckNonEmpty();
        }

        private void miDuplicate_Click(object sender, EventArgs e)
        {
            DuplicateToNext();
        }

        private void miCopyToChecked_Click(object sender, EventArgs e)
        {
            CopyFrameToChecked();
        }

        private void miMoveLeft_Click(object sender, EventArgs e)
        {
            MoveFrame(-1);
        }

        private void miMoveRight_Click(object sender, EventArgs e)
        {
            MoveFrame(1);
        }

        private void miReverse_Click(object sender, EventArgs e)
        {
            ReverseChecked();
        }

        private void miAlignBottoms_Click(object sender, EventArgs e)
        {
            AlignCheckedBottoms();
        }

        private void miFitCanvas_Click(object sender, EventArgs e)
        {
            FitCanvas();
        }

        private void miZoomIn_Click(object sender, EventArgs e)
        {
            SetZoom(canvas.Zoom + 1);
        }

        private void miZoomOut_Click(object sender, EventArgs e)
        {
            SetZoom(canvas.Zoom - 1);
        }

        private void miZoom1_Click(object sender, EventArgs e)
        {
            SetZoom(1);
        }

        private void miZoomFit_Click(object sender, EventArgs e)
        {
            ZoomToFit();
        }

        private void miPlay_Click(object sender, EventArgs e)
        {
            TogglePlay();
        }

        private void miHelpEn_Click(object sender, EventArgs e)
        {
            AnimEditHelp.Show(this, false);
        }

        private void miHelpDe_Click(object sender, EventArgs e)
        {
            AnimEditHelp.Show(this, true);
        }

        private void btToolPen_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.Pencil);
        }

        private void btToolEraser_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.Eraser);
        }

        private void btToolLine_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.Line);
        }

        private void btToolRect_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.Rectangle);
        }

        private void btToolRectFilled_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.FilledRectangle);
        }

        private void btToolEllipse_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.Ellipse);
        }

        private void btToolEllipseFilled_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.FilledEllipse);
        }

        private void btToolFill_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.Fill);
        }

        private void btToolPick_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.Pipette);
        }

        private void btToolMove_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.Move);
        }

        private void btToolSelect_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.Select);
        }

        private void btToolOverlay_Click(object sender, EventArgs e)
        {
            SetTool(DrawTool.Overlay);
        }

        private void btKeyTransparent_CheckedChanged(object sender, EventArgs e)
        {
            SetKeyTransparent(btKeyTransparent.Checked);
        }

        private void btPrimary_Click(object sender, EventArgs e)
        {
            PickColorDialog(true);
        }

        private void btSecondary_Click(object sender, EventArgs e)
        {
            PickColorDialog(false);
        }

        private void btSwapColors_Click(object sender, EventArgs e)
        {
            SwapColors();
        }

        private void numAlpha_ValueChanged(object sender, EventArgs e)
        {
            ApplyColors();
        }

        private void numBrush_ValueChanged(object sender, EventArgs e)
        {
            canvas.BrushSize = (int)numBrush.Value;
        }

        private void btRound_CheckedChanged(object sender, EventArgs e)
        {
            canvas.RoundBrush = btRound.Checked;
        }

        private void numTolerance_ValueChanged(object sender, EventArgs e)
        {
            canvas.FillTolerance = (int)numTolerance.Value;
        }

        private void cbLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            canvas.Layer = cbLayer.SelectedIndex == 1 ? EditLayer.Original : EditLayer.Paint; canvas.Invalidate();
        }

        private void btAllChecked_CheckedChanged(object sender, EventArgs e)
        {
            btAllChecked.BackColor = btAllChecked.Checked ? Color.LightGreen : SystemColors.Control;
            SetStatus(btAllChecked.Checked ? "Edits now apply to the current AND all checked frames." : "Edits apply to the current frame only.");
        }

        private void btUndo_Click(object sender, EventArgs e)
        {
            DoUndo();
        }

        private void btRedo_Click(object sender, EventArgs e)
        {
            DoRedo();
        }

        private void btZoomOut_Click(object sender, EventArgs e)
        {
            SetZoom(canvas.Zoom - 1);
        }

        private void btZoomIn_Click(object sender, EventArgs e)
        {
            SetZoom(canvas.Zoom + 1);
        }

        private void btHelp_Click(object sender, EventArgs e)
        {
            AnimEditHelp.Show(this, System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");
        }

        private void fmLoad_Click(object sender, EventArgs e)
        {
            LoadImagesSequential();
        }

        private void fmPaste_Click(object sender, EventArgs e)
        {
            PasteFromClipboard(false);
        }

        private void fmCopy_Click(object sender, EventArgs e)
        {
            CopyFrameToClipboard();
        }

        private void fmDuplicate_Click(object sender, EventArgs e)
        {
            DuplicateToNext();
        }

        private void fmCopyToChecked_Click(object sender, EventArgs e)
        {
            CopyFrameToChecked();
        }

        private void fmMoveLeft_Click(object sender, EventArgs e)
        {
            MoveFrame(-1);
        }

        private void fmMoveRight_Click(object sender, EventArgs e)
        {
            MoveFrame(1);
        }

        private void fmMirror_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Mirror", i => doc.MirrorFrame(i));
        }

        private void fmClear_Click(object sender, EventArgs e)
        {
            ClearFrames();
        }

        private void fmCheckOnly_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < AnimDocument.FrameCount; i++) doc.Frames[i].Enabled = i == current;
            strip.Invalidate();
            UpdatePlayOrder();
        }

        private void fmToClipList_Click(object sender, EventArgs e)
        {
            AddCurrentFrameToClipboardList();
        }

        private void canvasHost_Resize(object sender, EventArgs e)
        {
            CenterCanvas();
        }

        private void btBodySearch_Click(object sender, EventArgs e)
        {
            SearchBody();
        }

        private void btBrowserPlay_Click(object sender, EventArgs e)
        {
            ToggleBrowserPlay();
        }

        private void btBrowserStop_Click(object sender, EventArgs e)
        {
            StopBrowserPlay();
        }

        private void btBrowserToChecked_Click(object sender, EventArgs e)
        {
            BrowserToEditor(BrowserTarget.Checked);
        }

        private void btBrowserFromCurrent_Click(object sender, EventArgs e)
        {
            BrowserToEditor(BrowserTarget.FromCurrent);
        }

        private void btBrowserToCurrent_Click(object sender, EventArgs e)
        {
            BrowserToEditor(BrowserTarget.Current);
        }

        private void btBrowserAsRef_Click(object sender, EventArgs e)
        {
            BrowserAsReference();
        }

        private void btBrowserToOverlay_Click(object sender, EventArgs e)
        {
            AddOverlayFromBitmaps(PickBrowserFrames().Select(f => f.Bitmap), "Anim " + BrowserSourceName());
        }

        private void btBrowserCopy_Click(object sender, EventArgs e)
        {
            BrowserCopyFrame();
        }

        private void btBrowserExport_Click(object sender, EventArgs e)
        {
            BrowserExport();
        }

        private void btArtSearch_Click(object sender, EventArgs e)
        {
            ArtSearch();
        }

        private void btArtPrev_Click(object sender, EventArgs e)
        {
            if (lbArt.SelectedIndex > 0) lbArt.SelectedIndex--;
        }

        private void btArtNext_Click(object sender, EventArgs e)
        {
            if (lbArt.SelectedIndex < lbArt.Items.Count - 1) lbArt.SelectedIndex++;
        }

        private void btArtToCurrent_Click(object sender, EventArgs e)
        {
            ArtToEditor(false);
        }

        private void btArtToChecked_Click(object sender, EventArgs e)
        {
            ArtToEditor(true);
        }

        private void btArtAsRef_Click(object sender, EventArgs e)
        {
            ArtAsReference();
        }

        private void btArtToOverlay_Click(object sender, EventArgs e)
        {
            Bitmap b = SelectedArt(out int aid); if (b != null) AddOverlayFromBitmaps(new[] { b }, $"Art 0x{aid:X4}");
        }

        private void btArtCopy_Click(object sender, EventArgs e)
        {
            Bitmap b = SelectedArt(out _); if (b != null) Clipboard.SetImage(b);
        }

        private void btArtExport_Click(object sender, EventArgs e)
        {
            ArtExport();
        }

        private void btClipAdd_Click(object sender, EventArgs e)
        {
            AddClipboardImage();
        }

        private void btClipAddFrame_Click(object sender, EventArgs e)
        {
            AddCurrentFrameToClipboardList();
        }

        private void btClipReplace_Click(object sender, EventArgs e)
        {
            ReplaceClipItem();
        }

        private void btClipDelete_Click(object sender, EventArgs e)
        {
            DeleteClipItem();
        }

        private void btClipDeleteAll_Click(object sender, EventArgs e)
        {
            DeleteAllClipItems();
        }

        private void btClipPlay_Click(object sender, EventArgs e)
        {
            PlayClip();
        }

        private void btClipStop_Click(object sender, EventArgs e)
        {
            clipTimer.Stop();
        }

        private void btClipToEditor_Click(object sender, EventArgs e)
        {
            ClipToEditor();
        }

        private void btPrevFrame_Click(object sender, EventArgs e)
        {
            StepFrame(-1);
        }

        private void btPlay_Click(object sender, EventArgs e)
        {
            TogglePlay();
        }

        private void btNextFrame_Click(object sender, EventArgs e)
        {
            StepFrame(1);
        }

        private void btOvLoad_Click(object sender, EventArgs e)
        {
            LoadOverlayFromFile();
        }

        private void btOvPaste_Click(object sender, EventArgs e)
        {
            OverlayFromClipboard();
        }

        private void btOvFromSelection_Click(object sender, EventArgs e)
        {
            OverlayFromSelection();
        }

        private void btOvIntoSelection_Click(object sender, EventArgs e)
        {
            LoadImageIntoSelection();
        }

        private void btOvX2_Click(object sender, EventArgs e)
        {
            ScaleOverlay(2.0);
        }

        private void btOvHalf_Click(object sender, EventArgs e)
        {
            ScaleOverlay(0.5);
        }

        private void btOvFit_Click(object sender, EventArgs e)
        {
            FitOverlayToSelection();
        }

        private void btOvColor_Click(object sender, EventArgs e)
        {
            PickOverlayColor();
        }

        private void btOvColor_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) SetOverlayTintColor(canvas.PrimaryColor);
        }

        private void btOvDuplicate_Click(object sender, EventArgs e)
        {
            DuplicateOverlay();
        }

        private void btOvCenter_Click(object sender, EventArgs e)
        {
            CenterOverlay();
        }

        private void btOvUp_Click(object sender, EventArgs e)
        {
            MoveOverlayOrder(1);
        }

        private void btOvDown_Click(object sender, EventArgs e)
        {
            MoveOverlayOrder(-1);
        }

        private void btOvDelete_Click(object sender, EventArgs e)
        {
            DeleteOverlay();
        }

        private void btOvBake_Click(object sender, EventArgs e)
        {
            BakeOverlay(false);
        }

        private void btOvBakeRemove_Click(object sender, EventArgs e)
        {
            BakeOverlay(true);
        }

        private void btOvBakeAll_Click(object sender, EventArgs e)
        {
            BakeAllOverlays();
        }

        private void btSelLoad_Click(object sender, EventArgs e)
        {
            LoadImageIntoSelection();
        }

        private void btSelPaste_Click(object sender, EventArgs e)
        {
            PasteIntoSelection();
        }

        private void btSelCopy_Click(object sender, EventArgs e)
        {
            CopySelection();
        }

        private void btSelClear_Click(object sender, EventArgs e)
        {
            ClearSelectionArea();
        }

        private void btSelDeselect_Click(object sender, EventArgs e)
        {
            canvas.Selection = Rectangle.Empty;
        }

        private void btMergePaint_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Merge", i => doc.MergePaintDown(i));
        }

        private void btClearLayer_Click(object sender, EventArgs e)
        {
            ClearTargetLayer();
        }

        private void btPaintToChecked_Click(object sender, EventArgs e)
        {
            CopyPaintToChecked();
        }

        private void btRefLoad_Click(object sender, EventArgs e)
        {
            LoadReferenceImages();
        }

        private void btRefFromFrames_Click(object sender, EventArgs e)
        {
            UseFramesAsReference();
        }

        private void btRefClear_Click(object sender, EventArgs e)
        {
            SetReference(null);
        }

        private void btKeysTransparent_Click(object sender, EventArgs e)
        {
            MakeKeysTransparent();
        }

        private void btSecondaryTransparent_Click(object sender, EventArgs e)
        {
            SecondaryToTransparent();
        }

        private void btReplaceColor_Click(object sender, EventArgs e)
        {
            ReplaceSecondaryWithPrimary();
        }

        private void btOutline_Click(object sender, EventArgs e)
        {
            DrawOutline();
        }

        private void btCenterAnchor_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Center", CenterOnAnchor);
        }

        private void btBottomAnchor_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Align bottom", i => doc.AlignBottomToAnchor(i));
        }

        private void btAlignChecked_Click(object sender, EventArgs e)
        {
            AlignCheckedBottoms();
        }

        private void btOffsetToChecked_Click(object sender, EventArgs e)
        {
            CopyOffsetToChecked();
        }

        private void btScale_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Scale", i => doc.ScaleOriginal(i, (double)numScale.Value / 100.0));
        }

        private void btMirror_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Mirror", i => doc.MirrorFrame(i));
        }

        private void btRotate_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Rotate", i => doc.RotateOriginal90(i));
        }

        private void btScaleX2_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Scale", i => doc.ScaleOriginal(i, 2));
        }

        private void btScaleHalf_Click(object sender, EventArgs e)
        {
            ApplyToAffected("Scale", i => doc.ScaleOriginal(i, 0.5));
        }

        private void btCanvasApply_Click(object sender, EventArgs e)
        {
            ApplyCanvasSize();
        }

        private void btFitCanvas_Click(object sender, EventArgs e)
        {
            FitCanvas();
        }

        private void btGuideColor_Click(object sender, EventArgs e)
        {
            PickGuideColor();
        }

        private void btGuidesAtAnchor_Click(object sender, EventArgs e)
        {
            GuidesAtAnchor();
        }

        private void btGuidesOff_Click(object sender, EventArgs e)
        {
            numGuideY1.Value = -1; numGuideY2.Value = -1; numGuideX.Value = -1;
        }

        private void pnlPrimary_Click(object sender, EventArgs e)
        {
            PickColorDialog(true);
        }

        private void pnlSecondary_Click(object sender, EventArgs e)
        {
            PickColorDialog(false);
        }

        private void btSwapColors2_Click(object sender, EventArgs e)
        {
            SwapColors();
        }

        private void btReadPalette_Click(object sender, EventArgs e)
        {
            FillFramePalette();
        }

        private void btHistUndo_Click(object sender, EventArgs e)
        {
            DoUndo();
        }

        private void btHistRedo_Click(object sender, EventArgs e)
        {
            DoRedo();
        }

        private void btHistClear_Click(object sender, EventArgs e)
        {
            doc.ClearHistory();
        }

        private void lbHistory_DoubleClick(object sender, EventArgs e)
        {
            UndoTo(lbHistory.SelectedIndex);
        }

        #endregion
    }
}
