using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;
using Timer = System.Windows.Forms.Timer;

namespace UoFiddler.Controls.Forms
{
    /// <summary>Animation browser, art browser and clipboard animation tabs.</summary>
    public partial class AnimationEditFormButton
    {
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

        private static ListBox MakeList(int height = 0)
        {
            var lb = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
            if (height > 0) { lb.Dock = DockStyle.Top; lb.Height = height; }
            return lb;
        }

        private static Panel Column(int width, params Control[] topToBottom)
        {
            var p = new Panel { Dock = width > 0 ? DockStyle.Left : DockStyle.Fill, Width = width, Padding = new Padding(3) };
            // Dock order: last added = first docked, so add in reverse
            for (int i = topToBottom.Length - 1; i >= 0; i--) p.Controls.Add(topToBottom[i]);
            return p;
        }

        private static Label Caption(string text) => new Label { Text = text, Dock = DockStyle.Top, Height = 18, TextAlign = ContentAlignment.BottomLeft };

        #endregion

        // =====================================================================
        // === ANIMATION BROWSER ===============================================
        // =====================================================================

        #region Anim browser - fields and layout

        private const int SourceAuto = 0, SourceUop = 6, SourceVd = 7, SourceFolder = 8, SourceImages = 9;

        private ComboBox cbSource;
        private TextBox tbBodySearch;
        private ListBox lbBodies, lbActions, lbDirections, lbFrames;
        private NumericUpDown numHue, numBrowserZoom, numBrowserSpeed, numBrowserStart, numBrowserStep;
        private PreviewBox browserPreview;
        private Label lbBrowserInfo;
        private Button btBrowserPlay;
        private Timer browserTimer;

        private AnimSource source;
        private int sourceIndex = SourceAuto;
        private int bodiesVersion;
        private List<SourceFrame> browserFrames = new List<SourceFrame>();
        private int browserPos;
        private bool browserBusy;

        private void BuildAnimBrowserTab()
        {
            cbSource = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
            cbSource.Items.AddRange(new object[]
            {
                "Auto (body.def / bodyconv.def)", "anim.mul", "anim2.mul", "anim3.mul", "anim4.mul", "anim5.mul",
                "AnimationFrame*.uop (UOP)", "VD file (.vd)…", "Other client folder (anim*.mul)…", "Images / GIF…"
            });
            Tip(cbSource, "Where the animation comes from: Auto (like the client), one anim file, UOP, a .vd file, another client folder or your own images/GIF.",
                "Woher die Animation kommt: Auto (wie der Client), eine bestimmte anim-Datei, UOP, eine .vd-Datei, ein anderer Client-Ordner oder eigene Bilder/GIF.");

            tbBodySearch = new TextBox { Dock = DockStyle.Top };
            Tip(tbBodySearch, "Search a body: number (e.g. 400) or text from the list. Enter = find next.", "Body suchen: Nummer (z. B. 400) oder Text aus der Liste. Enter = nächster Treffer.");

            lbBodies = MakeList();
            Tip(lbBodies, "Bodies of the selected source.", "Bodies der gewählten Quelle.");

            lbActions = MakeList();
            Tip(lbActions, "Actions (walk, run, attack …) of the selected body.", "Aktionen (Laufen, Rennen, Angriff …) des gewählten Bodys.");
            lbDirections = MakeList(140);
            string[] dirNames = { "Dir 0", "Dir 1", "Dir 2", "Dir 3", "Dir 4", "Dir 5 (mirror of 3)", "Dir 6 (mirror of 2)", "Dir 7 (mirror of 1)" };
            lbDirections.Items.AddRange(dirNames);
            lbDirections.SelectedIndex = 0;
            Tip(lbDirections, "Direction. 0-4 are stored in the files, 5-7 are mirrored copies (like in the client).", "Richtung. 0-4 sind in den Dateien gespeichert, 5-7 sind gespiegelt (wie im Client).");
            numHue = Num(0, 3000, 0, 70);
            numHue.Dock = DockStyle.Top;
            Tip(numHue, "Hue number (0 = none). Only for MUL / VD sources.", "Hue-Nummer (0 = keine). Nur für MUL- / VD-Quellen.");

            lbFrames = MakeList();
            lbFrames.Dock = DockStyle.Left;
            lbFrames.Width = 150;
            Tip(lbFrames, "Frames of the animation. Select to view.", "Frames der Animation. Auswählen zum Ansehen.");
            browserPreview = new PreviewBox { Dock = DockStyle.Fill, Zoom = 2 };

            numBrowserZoom = Num(1, 8, 2, 40);
            Tip(numBrowserZoom, "Preview zoom.", "Vorschau-Zoom.");
            numBrowserSpeed = Num(30, 2000, 150, 56);
            Tip(numBrowserSpeed, "Preview speed in ms per frame.", "Vorschau-Geschwindigkeit in ms pro Frame.");
            numBrowserStart = Num(1, 999, 1, 50);
            Tip(numBrowserStart, "First source frame that is copied to the editor.", "Erster Quell-Frame, der in den Editor kommt.");
            numBrowserStep = Num(1, 10, 1, 40);
            Tip(numBrowserStep, "Use every n-th source frame (2 = every second frame – useful for long animations).", "Jeden n-ten Quell-Frame nehmen (2 = jeden zweiten – praktisch bei langen Animationen).");

            btBrowserPlay = Btn("▶ Play", (s, e) => ToggleBrowserPlay(), 70);
            Tip(btBrowserPlay, "Play / stop the source animation.", "Quell-Animation abspielen / stoppen.");

            Button toChecked = Btn("→ Checked frames", (s, e) => BrowserToEditor(BrowserTarget.Checked));
            Tip(toChecked, "Copy the frames (from Start, every Step) into the checked editor frames.", "Frames (ab Start, jeder Step) in die angehakten Editor-Frames kopieren.");
            Button toFromCurrent = Btn("→ From current frame on", (s, e) => BrowserToEditor(BrowserTarget.FromCurrent));
            Tip(toFromCurrent, "Copy the frames into the editor starting at the selected editor frame.", "Frames ab dem gewählten Editor-Frame einfügen.");
            Button toCurrent = Btn("→ Current frame", (s, e) => BrowserToEditor(BrowserTarget.Current));
            Tip(toCurrent, "Copy only the selected source frame into the current editor frame.", "Nur den gewählten Quell-Frame in den aktuellen Editor-Frame kopieren.");
            Button asRef = Btn("As reference", (s, e) => BrowserAsReference());
            Tip(asRef, "Show this animation transparent behind your frames (Layers tab: opacity, offset, fixed frame).", "Diese Animation durchscheinend hinter deinen Frames zeigen (Reiter Layers: Deckkraft, Versatz, fester Frame).");
            Button toOverlay = Btn("→ Overlay", (s, e) => AddOverlayFromBitmaps(PickBrowserFrames().Select(f => f.Bitmap), "Anim " + BrowserSourceName()));
            Tip(toOverlay, "Use the frames (Start/Step) as an animated overlay above your frames: editor frame 1 gets source frame 1 and so on.",
                "Frames (Start/Step) als animiertes Overlay über deinen Frames nutzen: Editor-Frame 1 bekommt Quell-Frame 1 usw.");
            Button copy = Btn("Copy frame", (s, e) => BrowserCopyFrame());
            Tip(copy, "Copy the selected source frame to the clipboard.", "Gewählten Quell-Frame in die Zwischenablage kopieren.");
            Button export = Btn("Export PNGs…", (s, e) => BrowserExport());
            Tip(export, "Save all frames of this animation as PNG files.", "Alle Frames dieser Animation als PNG speichern.");

            lbBrowserInfo = new Label { Dock = DockStyle.Bottom, Height = 34, TextAlign = ContentAlignment.MiddleLeft };

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, WrapContents = true };
            buttons.Controls.AddRange(new Control[]
            {
                btBrowserPlay, Btn("■", (s, e) => StopBrowserPlay(), 30), L("ms"), numBrowserSpeed, L("Zoom"), numBrowserZoom,
                L("Start"), numBrowserStart, L("Step"), numBrowserStep,
                toChecked, toFromCurrent, toCurrent, asRef, toOverlay, copy, export
            });

            var framesArea = new Panel { Dock = DockStyle.Fill };
            framesArea.Controls.Add(browserPreview);
            framesArea.Controls.Add(lbFrames);

            var col3 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(3) };
            col3.Controls.Add(framesArea);
            col3.Controls.Add(Caption("Frames"));
            col3.Controls.Add(buttons);
            col3.Controls.Add(lbBrowserInfo);

            var col2 = Column(210, Caption("Action"), lbActions);
            var col2Bottom = new Panel { Dock = DockStyle.Bottom, Height = 220 };
            lbDirections.Dock = DockStyle.Fill;
            // docking: the control added last ends up on top
            col2Bottom.Controls.Add(lbDirections);
            col2Bottom.Controls.Add(Caption("Direction"));
            col2Bottom.Controls.Add(numHue);
            col2Bottom.Controls.Add(Caption("Hue"));
            col2.Controls.Add(col2Bottom);

            var searchRow = new Panel { Dock = DockStyle.Top, Height = 24 };
            var btSearch = new Button { Text = "Find", Dock = DockStyle.Right, Width = 50 };
            btSearch.Click += (s, e) => SearchBody();
            searchRow.Controls.Add(tbBodySearch);
            searchRow.Controls.Add(btSearch);
            tbBodySearch.Dock = DockStyle.Fill;

            var col1 = Column(250, Caption("Source / Quelle"), cbSource, Caption("Body"), searchRow, lbBodies);

            tabAnim.Controls.Add(col3);
            tabAnim.Controls.Add(col2);
            tabAnim.Controls.Add(col1);
        }

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

        private RadioButton rbStatic, rbLand;
        private TextBox tbArtSearch;
        private ListBox lbArt;
        private PreviewBox artPreview;
        private NumericUpDown numArtZoom;
        private Label lbArtInfo;
        private readonly List<int> artIds = new List<int>();
        private int artLoadVersion;
        private Bitmap artPreviewImage;

        private void BuildArtBrowserTab()
        {
            rbStatic = new RadioButton { Text = "Static (items)", Checked = true, AutoSize = true };
            rbLand = new RadioButton { Text = "Land (tiles)", AutoSize = true };
            Tip(rbStatic, "Item / static art.", "Item- / Static-Art.");
            Tip(rbLand, "Land tiles.", "Boden-Kacheln.");
            tbArtSearch = new TextBox { Dock = DockStyle.Fill };
            Tip(tbArtSearch, "ID decimal or hex (0x…). Enter = search.", "ID dezimal oder hex (0x…). Enter = suchen.");
            lbArt = MakeList();
            artPreview = new PreviewBox { Dock = DockStyle.Fill, Zoom = 2 };
            numArtZoom = Num(1, 8, 2, 40);
            lbArtInfo = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft };

            Button toCurrent = Btn("→ Current frame", (s, e) => ArtToEditor(false));
            Tip(toCurrent, "Put the art into the current editor frame (bottom center on the anchor).", "Art in den aktuellen Editor-Frame setzen (unten mittig am Anker).");
            Button toChecked = Btn("→ All checked frames", (s, e) => ArtToEditor(true));
            Tip(toChecked, "Put the art into every checked frame – e.g. a weapon or item to animate.", "Art in jeden angehakten Frame setzen – z. B. Waffe oder Item zum Animieren.");
            Button asRef = Btn("As reference", (s, e) => ArtAsReference());
            Tip(asRef, "Show the art transparent behind the frames (compare size and position).", "Art durchscheinend hinter den Frames zeigen (Größe und Position vergleichen).");
            Button toOverlay = Btn("→ Overlay", (s, e) => { Bitmap b = SelectedArt(out int aid); if (b != null) AddOverlayFromBitmaps(new[] { b }, $"Art 0x{aid:X4}"); });
            Tip(toOverlay, "Place the art as a movable overlay on the frames (e.g. a cage around a creature). Black/white of the art become transparent.",
                "Art als verschiebbares Overlay auf die Frames setzen (z. B. Käfig um eine Kreatur). Schwarz/Weiß der Art werden transparent.");
            Button copy = Btn("Copy", (s, e) => { Bitmap b = SelectedArt(out _); if (b != null) Clipboard.SetImage(b); });
            Tip(copy, "Copy the original art (not zoomed) to the clipboard.", "Original-Art (ungezoomt) in die Zwischenablage kopieren.");
            Button export = Btn("Export…", (s, e) => ArtExport());
            Button prev = Btn("◀", (s, e) => { if (lbArt.SelectedIndex > 0) lbArt.SelectedIndex--; }, 30);
            Button next = Btn("▶", (s, e) => { if (lbArt.SelectedIndex < lbArt.Items.Count - 1) lbArt.SelectedIndex++; }, 30);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
            buttons.Controls.AddRange(new Control[] { prev, next, L("Zoom"), numArtZoom, toCurrent, toChecked, asRef, toOverlay, copy, export });

            var searchRow = new Panel { Dock = DockStyle.Top, Height = 24 };
            var btSearch = new Button { Text = "Find", Dock = DockStyle.Right, Width = 50 };
            btSearch.Click += (s, e) => ArtSearch();
            searchRow.Controls.Add(tbArtSearch);
            searchRow.Controls.Add(btSearch);

            var radios = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
            radios.Controls.AddRange(new Control[] { rbStatic, rbLand });

            var left = Column(230, radios, searchRow, lbArt);
            var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(3) };
            right.Controls.Add(artPreview);
            right.Controls.Add(buttons);
            right.Controls.Add(lbArtInfo);

            tabArt.Controls.Add(right);
            tabArt.Controls.Add(left);
        }

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
        private ListBox lbClip;
        private PreviewBox clipPreview;
        private CheckBox chkClipTransparent;
        private NumericUpDown numClipFps;
        private Timer clipTimer;
        private int clipPos;
        private int clipCounter = 1;

        private void BuildClipboardTab()
        {
            lbClip = MakeList();
            Tip(lbClip, "Images collected from the clipboard (or from the editor). Play them as a quick animation.", "Gesammelte Bilder aus der Zwischenablage (oder dem Editor). Als schnelle Animation abspielen.");
            clipPreview = new PreviewBox { Dock = DockStyle.Fill, Zoom = 2 };
            chkClipTransparent = Chk("Black/White transparent");
            Tip(chkClipTransparent, "Show pure black and white as transparent.", "Reines Schwarz und Weiß transparent anzeigen.");
            numClipFps = Num(1, 60, 8, 48);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
            buttons.Controls.AddRange(new Control[]
            {
                Btn("Add from clipboard", (s, e) => AddClipboardImage()),
                Btn("Add current frame", (s, e) => AddCurrentFrameToClipboardList()),
                Btn("Replace with clipboard", (s, e) => ReplaceClipItem()),
                Btn("Delete", (s, e) => DeleteClipItem()),
                Btn("Delete all", (s, e) => DeleteAllClipItems()),
                Btn("▶ Play", (s, e) => PlayClip()),
                Btn("■ Stop", (s, e) => clipTimer.Stop()),
                L("FPS"), numClipFps, chkClipTransparent,
                Btn("→ Editor (from current frame)", (s, e) => ClipToEditor())
            });

            var left = Column(200, Caption("Images"), lbClip);
            var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(3) };
            right.Controls.Add(clipPreview);
            right.Controls.Add(buttons);

            tabClip.Controls.Add(right);
            tabClip.Controls.Add(left);
        }

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
    }
}
