/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Spell- und Effekt-Editor: erstellt Partikel- und Zaubereffekte aus Bildfolgen (Einzelbilder,
    /// GIF, Sprite-Sheet oder Partikel-Generator). Jeder Frame wird ein Art-Item; die Abspielfolge
    /// (Dauer pro Frame, Ping-Pong) steht im Animdata-Eintrag, so dass keine Art doppelt gespeichert wird.
    /// Vorschau additiv, freistehend, auf Bodentile, auf einer Figur oder als Projektil, mit Hue.
    /// </summary>
    public partial class SpellEffectEditorForm : Form
    {
        private const int MaxSequence = 64;

        /// <summary>Imported frame with its individual settings.</summary>
        private sealed class EffectFrame
        {
            public Bitmap Image;
            public int Dx, Dy;
            public int Repeat = 1;
        }

        private readonly List<EffectFrame> _raw = new List<EffectFrame>();  // imported frames
        private readonly List<Bitmap> _frames = new List<Bitmap>();          // processed frames (UO colours), same order
        private List<int> _sequence = new List<int>();                       // play order (indices into _frames)
        private int _tick;
        private Bitmap _ground;
        private Bitmap _figure;
        private Point _figureCenter;
        private bool _updatingFrame;

        public SpellEffectEditorForm()
        {
            InitializeComponent();
            lvFrames.LargeImageList = imageListThumbs;
            cbPreviewMode.SelectedIndex = 0;
            UpdateHueStrip();
            timerPlay.Start();
        }

        #region Events

        private void TimerPlay_Tick(object sender, EventArgs e)
        {
            _tick++;
            ShowFrame();
        }

        private void Process_Changed(object sender, EventArgs e) => Reprocess();

        private void Preview_Changed(object sender, EventArgs e) => ShowFrame();

        private void PreviewMode_Changed(object sender, EventArgs e)
        {
            LoadGround();
            LoadFigure();
            ShowFrame();
        }

        private void NumMs_ValueChanged(object sender, EventArgs e)
        {
            timerPlay.Interval = (int)numMs.Value;
        }

        private void ChkPlay_CheckedChanged(object sender, EventArgs e)
        {
            timerPlay.Enabled = chkPlay.Checked;
        }

        private void NumZoom_ValueChanged(object sender, EventArgs e)
        {
            viewEffect.Zoom = (int)numZoom.Value;
        }

        private void NumHue_ValueChanged(object sender, EventArgs e) => UpdateHueStrip();

        private void LvFrames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvFrames.SelectedIndices.Count == 0)
            {
                return;
            }
            int i = lvFrames.SelectedIndices[0];
            if (!chkPlay.Checked)
            {
                // show the selected frame
                int pos = _sequence.IndexOf(i);
                _tick = Math.Max(0, pos);
                ShowFrame();
            }
            EffectFrame f = _raw[i];
            _updatingFrame = true;
            numFrameDx.Value = Math.Max(numFrameDx.Minimum, Math.Min(numFrameDx.Maximum, f.Dx));
            numFrameDy.Value = Math.Max(numFrameDy.Minimum, Math.Min(numFrameDy.Maximum, f.Dy));
            numRepeat.Value = Math.Max(numRepeat.Minimum, Math.Min(numRepeat.Maximum, f.Repeat));
            _updatingFrame = false;
        }

        /// <summary>Offset / duration of all selected frames.</summary>
        private void FrameSetting_Changed(object sender, EventArgs e)
        {
            if (_updatingFrame || lvFrames.SelectedIndices.Count == 0)
            {
                return;
            }
            foreach (int i in lvFrames.SelectedIndices)
            {
                _raw[i].Dx = (int)numFrameDx.Value;
                _raw[i].Dy = (int)numFrameDy.Value;
                _raw[i].Repeat = (int)numRepeat.Value;
            }
            int[] selected = lvFrames.SelectedIndices.Cast<int>().ToArray();
            Reprocess();
            foreach (int i in selected)
            {
                if (i < lvFrames.Items.Count)
                {
                    lvFrames.Items[i].Selected = true;
                }
            }
        }

        private void BtnAddImages_Click(object sender, EventArgs e) => AddImages();

        private void BtnImportSheet_Click(object sender, EventArgs e) => ImportSheet();

        private void BtnLoadArt_Click(object sender, EventArgs e) => LoadFromArt();

        private void BtnRemove_Click(object sender, EventArgs e) => RemoveSelected();

        private void BtnClear_Click(object sender, EventArgs e) => ClearAll();

        private void BtnParticles_Click(object sender, EventArgs e) => OpenParticleGenerator();

        private void BtnFreeBlock_Click(object sender, EventArgs e) => FindFreeBlock();

        private void BtnExport_Click(object sender, EventArgs e) => ExportToClient();

        private void BtnSaveArt_Click(object sender, EventArgs e) => Save("Art");

        private void BtnSaveTileData_Click(object sender, EventArgs e) => Save("TileData");

        private void BtnExportPng_Click(object sender, EventArgs e) => ExportPng();

        private void BtnCopyScript_Click(object sender, EventArgs e) => CopyScript();

        private void UpdateHueStrip()
        {
            Image old = picHueStrip.Image;
            picHueStrip.Image = StudioUi.HueStrip(StudioUi.GetHue((int)numHue.Value), picHueStrip.Width, picHueStrip.Height);
            old?.Dispose();
            ShowFrame();
        }

        private void LoadGround()
        {
            _ground?.Dispose();
            _ground = null;
            if (cbPreviewMode.SelectedIndex != 1 && cbPreviewMode.SelectedIndex != 2)
            {
                return;
            }
            try
            {
                _ground = UltimaImages.CopyLand((int)numGroundId.Value);
            }
            catch
            {
                _ground = null;
            }
        }

        /// <summary>Standing human (body 400, action "stand", direction down-left) for the figure preview.</summary>
        private void LoadFigure()
        {
            if (cbPreviewMode.SelectedIndex != 2 || _figure != null)
            {
                return;
            }
            try
            {
                AnimIdx idx = AnimationEdit.GetAnimation(1, 400, 4, 1);
                Bitmap[] bits = idx?.GetFrames();
                if (bits != null && bits.Length > 0 && bits[0] != null && idx.Frames != null && idx.Frames.Count > 0)
                {
                    _figure = StudioImaging.To32(bits[0]);
                    _figureCenter = idx.Frames[0].Center;
                    foreach (Bitmap b in bits)
                    {
                        b?.Dispose();
                    }
                }
            }
            catch
            {
                _figure = null;
            }
        }

        #endregion

        #region Import

        private void AddImages()
        {
            using var dlg = new OpenFileDialog { Filter = StudioUi.ImageFilter, Multiselect = true, Title = "Bildfolge (wird nach Dateinamen sortiert)" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            try
            {
                foreach (string f in dlg.FileNames.OrderBy(f => f, new NaturalStringComparer()))
                {
                    foreach (Bitmap b in StudioImaging.LoadFrames(f))
                    {
                        _raw.Add(new EffectFrame { Image = b });
                    }
                }
                Reprocess();
                lblStatus.Text = $"{_raw.Count} Frames geladen.";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void ImportSheet()
        {
            using var dlg = new OpenFileDialog { Filter = StudioUi.ImageFilter };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            using Bitmap sheet = StudioImaging.LoadImage(dlg.FileName);
            using var ask = new EffectSheetSplitForm(sheet);
            if (ask.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            foreach (List<Bitmap> row in StudioImaging.SliceSheet(sheet, ask.Columns, ask.Rows, false))
            {
                _raw.AddRange(row.Select(b => new EffectFrame { Image = b }));
            }
            Reprocess();
            lblStatus.Text = $"{_raw.Count} Frames aus Sprite-Sheet.";
        }

        private void OpenParticleGenerator()
        {
            using var dlg = new EffectParticleForm();
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            List<Bitmap> frames = dlg.TakeFrames();
            if (frames.Count == 0)
            {
                return;
            }
            if (_raw.Count > 0 && StudioUi.Ask(this, "Vorhandene Frames ersetzen?\n(Nein = anhängen)"))
            {
                ClearAll();
            }
            _raw.AddRange(frames.Select(b => new EffectFrame { Image = b }));
            chkBlack.Checked = true;   // generated on black -> additive
            Reprocess();
            lblStatus.Text = $"{frames.Count} Frames vom Partikel-Generator übernommen.";
        }

        /// <summary>Loads an existing (animated) effect: art frames + animdata sequence.</summary>
        private void LoadFromArt()
        {
            if (!StudioUi.TryParseId(txtStartId.Text, out int id))
            {
                StudioUi.Error(this, "Ungültige Start-ID.");
                return;
            }
            try
            {
                Animdata.AnimdataEntry entry = Animdata.GetAnimData(id);
                var seq = new List<int>();
                if (entry != null && entry.FrameCount > 0)
                {
                    for (int i = 0; i < entry.FrameCount && i < MaxSequence; i++)
                    {
                        seq.Add(id + entry.FrameData[i]);
                    }
                    numMs.Value = Math.Max(numMs.Minimum, Math.Min(numMs.Maximum, Math.Max(1, (int)entry.FrameInterval) * 100));
                }
                else
                {
                    seq.Add(id);
                }

                ClearAll();

                // consecutive equal ids -> one frame with a longer duration
                var runs = new List<(int Id, int Count)>();
                foreach (int a in seq)
                {
                    if (runs.Count > 0 && runs[runs.Count - 1].Id == a)
                    {
                        runs[runs.Count - 1] = (a, runs[runs.Count - 1].Count + 1);
                    }
                    else
                    {
                        runs.Add((a, 1));
                    }
                }

                // ping-pong: second half mirrors the first half
                bool pingPong = false;
                var ids = runs.Select(r => r.Id).ToList();
                int distinct = ids.Distinct().Count();
                if (distinct > 2 && ids.Count == (2 * distinct) - 2 && ids.Skip(distinct).SequenceEqual(ids.Take(distinct - 1).Skip(1).Reverse()))
                {
                    pingPong = true;
                    runs = runs.Take(distinct).ToList();
                }

                foreach (var (artId, count) in runs)
                {
                    Bitmap b = UltimaImages.CopyStatic(artId);
                    if (b != null)
                    {
                        _raw.Add(new EffectFrame { Image = b, Repeat = Math.Min(16, count) });
                    }
                }
                chkBlack.Checked = false;
                chkPingPong.Checked = pingPong;
                Reprocess();
                lblStatus.Text = entry != null
                    ? $"Animierter Effekt 0x{id:X4} geladen: {_raw.Count} Frames, Sequenz {seq.Count} Einträge{(pingPong ? ", Ping-Pong" : string.Empty)}."
                    : $"Art 0x{id:X4} geladen (kein Animdata-Eintrag).";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void RemoveSelected()
        {
            foreach (int i in lvFrames.SelectedIndices.Cast<int>().OrderByDescending(i => i))
            {
                if (i >= 0 && i < _raw.Count)
                {
                    _raw[i].Image.Dispose();
                    _raw.RemoveAt(i);
                }
            }
            Reprocess();
        }

        private void ClearAll()
        {
            foreach (EffectFrame f in _raw)
            {
                f.Image.Dispose();
            }
            _raw.Clear();
            Reprocess();
        }

        #endregion

        #region Processing / preview

        private void Reprocess()
        {
            foreach (Bitmap b in _frames)
            {
                b.Dispose();
            }
            _frames.Clear();

            var tmp = new List<Bitmap>();
            foreach (EffectFrame src in _raw)
            {
                Bitmap b = new Bitmap(src.Image);
                if (chkBlack.Checked)
                {
                    b = Swap(b, StudioImaging.BlackToTransparent(b, (int)numBlack.Value));
                }
                if (numBrightness.Value != 0)
                {
                    b = Swap(b, StudioImaging.AdjustColors(b, (int)numBrightness.Value, 0, 100));
                }
                if (chkGray.Checked)
                {
                    b = Swap(b, StudioImaging.ToGray(b));
                }
                if (numScale.Value != 100)
                {
                    float f = (float)numScale.Value / 100f;
                    b = Swap(b, StudioImaging.Resize(b, Math.Max(1, (int)(b.Width * f)), Math.Max(1, (int)(b.Height * f)), InterpolationMode.HighQualityBicubic));
                }
                int max = (int)numMaxSize.Value;
                if (b.Width > max || b.Height > max)
                {
                    b = Swap(b, StudioImaging.ResizeFit(b, max, max, InterpolationMode.HighQualityBicubic));
                }
                if (numGlow.Value > 0 && numGlowStrength.Value > 0)
                {
                    b = Swap(b, StudioImaging.Glow(b, (int)numGlow.Value, (double)numGlowStrength.Value));
                }
                b = Swap(b, StudioImaging.HardAlpha(b, 100));
                tmp.Add(b);
            }

            if (tmp.Count > 0)
            {
                if (chkCrop.Checked)
                {
                    CropUnion(tmp, _raw);
                }
                else
                {
                    for (int i = 0; i < tmp.Count; i++)
                    {
                        if (_raw[i].Dx != 0 || _raw[i].Dy != 0)
                        {
                            tmp[i] = Swap(tmp[i], StudioImaging.Shift(tmp[i], _raw[i].Dx, _raw[i].Dy));
                        }
                    }
                }
            }

            foreach (Bitmap b in tmp)
            {
                _frames.Add(StudioImaging.ToUoColors(b));
                b.Dispose();
            }

            BuildSequence();
            RefreshThumbs();
            ShowFrame();

            lblSequence.Text = $"Sequenz: {_sequence.Count} / {MaxSequence} Einträge, {_frames.Count} Art-Items";
            lblSequence.ForeColor = _sequence.Count > MaxSequence ? Color.Red : SystemColors.ControlText;
            if (_sequence.Count > MaxSequence)
            {
                lblStatus.Text = $"Achtung: Sequenz hat {_sequence.Count} Einträge – Animdata erlaubt max. {MaxSequence}.";
            }
            else if (_frames.Count > 0)
            {
                lblStatus.Text = $"{_frames.Count} Frames, {_frames[0].Width}x{_frames[0].Height} Pixel, Sequenz {_sequence.Count} Einträge.";
            }
        }

        /// <summary>Play order: every frame Repeat times, optionally back again (ping-pong).</summary>
        private void BuildSequence()
        {
            var seq = new List<int>();
            for (int i = 0; i < _frames.Count; i++)
            {
                for (int r = 0; r < Math.Max(1, _raw[i].Repeat); r++)
                {
                    seq.Add(i);
                }
            }
            if (chkPingPong.Checked && _frames.Count > 2)
            {
                for (int i = _frames.Count - 2; i > 0; i--)
                {
                    for (int r = 0; r < Math.Max(1, _raw[i].Repeat); r++)
                    {
                        seq.Add(i);
                    }
                }
            }
            _sequence = seq;
        }

        private static Bitmap Swap(Bitmap old, Bitmap neu)
        {
            if (!ReferenceEquals(old, neu))
            {
                old.Dispose();
            }
            return neu;
        }

        /// <summary>
        /// Crops all frames to the union of their content (centred, plus the frame offsets) so that
        /// the effect stays in place.
        /// </summary>
        private static void CropUnion(List<Bitmap> frames, List<EffectFrame> settings)
        {
            int w = frames.Max(f => f.Width), h = frames.Max(f => f.Height);
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            for (int n = 0; n < frames.Count; n++)
            {
                Bitmap f = frames[n];
                int ox = ((w - f.Width) / 2) + settings[n].Dx, oy = ((h - f.Height) / 2) + settings[n].Dy;
                int[] p = StudioImaging.GetPixels(f);
                for (int y = 0; y < f.Height; y++)
                {
                    for (int x = 0; x < f.Width; x++)
                    {
                        if (((p[(y * f.Width) + x] >> 24) & 0xFF) > 0)
                        {
                            minX = Math.Min(minX, x + ox);
                            minY = Math.Min(minY, y + oy);
                            maxX = Math.Max(maxX, x + ox);
                            maxY = Math.Max(maxY, y + oy);
                        }
                    }
                }
            }
            if (maxX == int.MinValue)
            {
                return;
            }
            var rect = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            for (int i = 0; i < frames.Count; i++)
            {
                var c = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(c))
                {
                    g.Clear(Color.Transparent);
                    int ox = ((w - frames[i].Width) / 2) + settings[i].Dx, oy = ((h - frames[i].Height) / 2) + settings[i].Dy;
                    g.DrawImageUnscaled(frames[i], ox - rect.X, oy - rect.Y);
                }
                frames[i].Dispose();
                frames[i] = c;
            }
        }

        private void RefreshThumbs()
        {
            lvFrames.BeginUpdate();
            lvFrames.Items.Clear();
            imageListThumbs.Images.Clear();
            for (int i = 0; i < _frames.Count; i++)
            {
                using (var t = new Bitmap(48, 48))
                {
                    using (Graphics g = Graphics.FromImage(t))
                    {
                        g.Clear(Color.FromArgb(24, 24, 28));
                        Bitmap f = _frames[i];
                        float s = Math.Min(1f, Math.Min(46f / f.Width, 46f / f.Height));
                        int w = Math.Max(1, (int)(f.Width * s)), h = Math.Max(1, (int)(f.Height * s));
                        g.DrawImage(f, (48 - w) / 2, (48 - h) / 2, w, h);
                    }
                    imageListThumbs.Images.Add(t);
                }
                string text = _raw[i].Repeat > 1 ? $"{i} (x{_raw[i].Repeat})" : i.ToString();
                lvFrames.Items.Add(new ListViewItem(text, i));
            }
            lvFrames.EndUpdate();
        }

        private void ShowFrame()
        {
            Image old = viewEffect.Image;
            if (_frames.Count == 0 || _sequence.Count == 0)
            {
                viewEffect.Image = null;
                viewEffect.Caption = "Keine Frames";
                old?.Dispose();
                return;
            }

            int seqPos = _tick % _sequence.Count;
            Bitmap frame = _frames[_sequence[seqPos]];
            Hue hue = StudioUi.GetHue((int)numHue.Value);
            using Bitmap hued = hue != null ? StudioImaging.ApplyHue(frame, hue, chkPartialHue.Checked) : new Bitmap(frame);

            Bitmap scene = cbPreviewMode.SelectedIndex switch
            {
                1 => SceneOnTile(hued, false),
                2 => SceneOnTile(hued, true),
                3 => SceneProjectile(hued),
                _ => SceneFree(hued)
            };

            viewEffect.Image = scene;
            viewEffect.Caption = $"Frame {_sequence[seqPos]} (Sequenz {seqPos + 1}/{_sequence.Count})" +
                                 (hue != null ? $"  Hue {(int)numHue.Value} {hue.Name}" : string.Empty);
            old?.Dispose();
        }

        private Bitmap NewScene(int w, int h)
        {
            var scene = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using Graphics g = Graphics.FromImage(scene);
            g.Clear(chkAdditive.Checked || cbPreviewMode.SelectedIndex > 0 ? Color.FromArgb(20, 20, 24) : Color.Transparent);
            return scene;
        }

        private void DrawEffect(Bitmap scene, Bitmap effect, int x, int y)
        {
            if (chkAdditive.Checked)
            {
                BlendAdditive(scene, effect, x, y);
            }
            else
            {
                using Graphics g = Graphics.FromImage(scene);
                g.DrawImageUnscaled(effect, x, y);
            }
        }

        private Bitmap SceneFree(Bitmap effect)
        {
            Bitmap scene = NewScene(effect.Width + 20, effect.Height + 20);
            DrawEffect(scene, effect, 10, 10);
            return scene;
        }

        /// <summary>Effect standing on a land tile (like a static: bottom of the image at the tile bottom), optionally with a figure.</summary>
        private Bitmap SceneOnTile(Bitmap effect, bool withFigure)
        {
            int figH = withFigure && _figure != null ? _figure.Height + _figureCenter.Y + 10 : 0;
            int w = Math.Max(Math.Max(effect.Width, 44), withFigure && _figure != null ? _figure.Width : 0) + 40;
            int h = Math.Max(effect.Height, figH) + 44 + 20;
            Bitmap scene = NewScene(w, h);
            int tileX = (w - 44) / 2, tileY = h - 44 - 6;
            int footX = tileX + 22, footY = tileY + 22;
            using (Graphics g = Graphics.FromImage(scene))
            {
                if (_ground != null)
                {
                    g.DrawImageUnscaled(_ground, tileX, tileY);
                }
                else
                {
                    using var pen = new Pen(Color.FromArgb(90, 90, 110));
                    g.DrawPolygon(pen, new[] { new Point(footX, tileY), new Point(tileX + 44, footY), new Point(footX, tileY + 44), new Point(tileX, footY) });
                }
                if (withFigure && _figure != null)
                {
                    // UO: frame drawn at (foot - centerX, foot - centerY - height)
                    g.DrawImageUnscaled(_figure, footX - _figureCenter.X, footY - _figureCenter.Y - _figure.Height);
                }
            }
            DrawEffect(scene, effect, footX - (effect.Width / 2), tileY + 44 - effect.Height);
            return scene;
        }

        /// <summary>Effect flying from A (bottom left) to B (top right) like a MovingEffect.</summary>
        private Bitmap SceneProjectile(Bitmap effect)
        {
            int w = 340 + effect.Width, h = 240 + effect.Height;
            Bitmap scene = NewScene(w, h);
            var a = new Point(40 + (effect.Width / 2), h - 40 - (effect.Height / 2));
            var b = new Point(w - 40 - (effect.Width / 2), 40 + (effect.Height / 2));
            const int steps = 30;
            double t = (_tick % steps) / (double)(steps - 1);
            int x = (int)(a.X + ((b.X - a.X) * t)), y = (int)(a.Y + ((b.Y - a.Y) * t));
            using (Graphics g = Graphics.FromImage(scene))
            {
                using var pen = new Pen(Color.FromArgb(70, 120, 120, 160)) { DashStyle = DashStyle.Dash };
                g.DrawLine(pen, a, b);
                g.DrawEllipse(pen, a.X - 6, a.Y - 3, 12, 6);
                g.DrawEllipse(pen, b.X - 6, b.Y - 3, 12, 6);
            }
            DrawEffect(scene, effect, x - (effect.Width / 2), y - (effect.Height / 2));
            return scene;
        }

        private static void BlendAdditive(Bitmap dst, Bitmap src, int ox, int oy)
        {
            int[] d = StudioImaging.GetPixels(dst);
            int[] s = StudioImaging.GetPixels(src);
            for (int y = 0; y < src.Height; y++)
            {
                int dy = y + oy;
                if (dy < 0 || dy >= dst.Height)
                {
                    continue;
                }
                for (int x = 0; x < src.Width; x++)
                {
                    int dx = x + ox;
                    if (dx < 0 || dx >= dst.Width)
                    {
                        continue;
                    }
                    int sc = s[(y * src.Width) + x];
                    if (((sc >> 24) & 0xFF) == 0)
                    {
                        continue;
                    }
                    int i = (dy * dst.Width) + dx;
                    int dc = d[i];
                    int r = Math.Min(255, ((dc >> 16) & 0xFF) + ((sc >> 16) & 0xFF));
                    int g = Math.Min(255, ((dc >> 8) & 0xFF) + ((sc >> 8) & 0xFF));
                    int b = Math.Min(255, (dc & 0xFF) + (sc & 0xFF));
                    d[i] = unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
                }
            }
            using Bitmap blended = StudioImaging.FromPixels(d, dst.Width, dst.Height);
            using Graphics gr = Graphics.FromImage(dst);
            gr.CompositingMode = CompositingMode.SourceCopy;
            gr.DrawImageUnscaled(blended, 0, 0);
        }

        #endregion

        #region Export

        private void FindFreeBlock()
        {
            int need = Math.Max(1, _frames.Count);
            try
            {
                int max = Art.GetMaxItemId();
                int run = 0;
                for (int i = 0x100; i <= max; i++)
                {
                    run = Art.IsValidStatic(i) ? 0 : run + 1;
                    if (run == need)
                    {
                        int start = i - need + 1;
                        txtStartId.Text = $"0x{start:X4}";
                        lblStatus.Text = $"Freier Block für {need} Frames: 0x{start:X4} – 0x{i:X4}";
                        return;
                    }
                }
                StudioUi.Info(this, $"Kein freier Block für {need} Frames gefunden.");
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void ExportToClient()
        {
            if (_frames.Count == 0)
            {
                StudioUi.Info(this, "Keine Frames vorhanden.");
                return;
            }
            if (_sequence.Count > MaxSequence)
            {
                StudioUi.Error(this, $"Die Sequenz hat {_sequence.Count} Einträge, Animdata erlaubt maximal {MaxSequence}. Bitte Frames, Dauer oder Ping-Pong reduzieren.");
                return;
            }
            if (!StudioUi.TryParseId(txtStartId.Text, out int start) || start < 0)
            {
                StudioUi.Error(this, "Ungültige Start-ID.");
                return;
            }

            try
            {
                int max = Art.GetMaxItemId();
                if (start + _frames.Count - 1 > max)
                {
                    StudioUi.Error(this, $"Der Block 0x{start:X4} – 0x{start + _frames.Count - 1:X4} überschreitet die maximale Item-ID 0x{max:X4}.");
                    return;
                }
                int used = Enumerable.Range(start, _frames.Count).Count(Art.IsValidStatic);
                if (used > 0 && !StudioUi.Ask(this, $"{used} der {_frames.Count} Item-IDs ab 0x{start:X4} sind belegt. Überschreiben?"))
                {
                    return;
                }

                for (int i = 0; i < _frames.Count; i++)
                {
                    int id = start + i;
                    Art.ReplaceStatic(id, StudioImaging.To1555(_frames[i]));

                    if (TileData.ItemTable != null && id < TileData.ItemTable.Length)
                    {
                        string name = txtName.Text.Length > 20 ? txtName.Text.Substring(0, 20) : txtName.Text;
                        TileData.ItemTable[id].Name = name;
                        TileFlag flags = TileData.ItemTable[id].Flags & ~TileFlag.Animation;
                        if (i == 0)
                        {
                            flags |= TileFlag.Animation;
                        }
                        flags = chkTranslucent.Checked ? flags | TileFlag.Translucent : flags & ~TileFlag.Translucent;
                        flags = chkPartialHue.Checked ? flags | TileFlag.PartialHue : flags & ~TileFlag.PartialHue;
                        TileData.ItemTable[id].Flags = flags;
                        TileData.ItemTable[id].Height = 0;
                    }
                }

                // animdata: play order as offsets from the base id (repeats / ping-pong need no extra art)
                var frameData = new sbyte[64];
                for (int i = 0; i < _sequence.Count; i++)
                {
                    frameData[i] = (sbyte)_sequence[i];
                }
                byte interval = (byte)Math.Max(1, Math.Min(255, (int)Math.Round((double)numMs.Value / 100.0)));
                Animdata.AnimData ??= new Dictionary<int, Animdata.AnimdataEntry>();
                Animdata.AnimData[start] = new Animdata.AnimdataEntry(frameData, 0, (byte)_sequence.Count, interval, 0);

                StudioUi.MarkChanged("Art");
                StudioUi.MarkChanged("TileData");
                StudioUi.MarkChanged("Animdata");
                lblStatus.Text = $"Effekt geschrieben: Items 0x{start:X4} – 0x{start + _frames.Count - 1:X4}, Sequenz {_sequence.Count} Einträge, Intervall {interval}.";
                StudioUi.Info(this, lblStatus.Text + "\n\nBitte \"Art-MUL speichern\" und \"tiledata + animdata\" ausführen.");
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void Save(string what)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                string path = StudioUi.OutputPath;
                if (what == "Art")
                {
                    Art.Save(path);
                    StudioUi.ResetChanged("Art");
                    StudioUi.Info(this, $"art.mul / artidx.mul gespeichert in:\n{path}");
                }
                else
                {
                    TileData.SaveTileData(Path.Combine(path, "tiledata.mul"));
                    if (Animdata.AnimData == null || Files.GetFilePath("animdata.mul") == null)
                    {
                        StudioUi.Error(this, "animdata.mul ist nicht geladen – tiledata.mul wurde gespeichert, animdata.mul nicht.");
                        return;
                    }
                    Animdata.Save(path);
                    StudioUi.ResetChanged("TileData");
                    StudioUi.ResetChanged("Animdata");
                    StudioUi.Info(this, $"tiledata.mul und animdata.mul gespeichert in:\n{path}");
                }
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ExportPng()
        {
            if (_frames.Count == 0)
            {
                return;
            }
            using var dlg = new FolderBrowserDialog { Description = "Zielordner", SelectedPath = StudioUi.OutputPath };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            for (int i = 0; i < _frames.Count; i++)
            {
                _frames[i].Save(Path.Combine(dlg.SelectedPath, $"effect_{i:D2}.png"), ImageFormat.Png);
            }
            lblStatus.Text = $"{_frames.Count} Frames gespeichert in {dlg.SelectedPath}";
        }

        private void CopyScript()
        {
            StudioUi.TryParseId(txtStartId.Text, out int id);
            int hue = (int)numHue.Value;
            int speed = Math.Max(1, Math.Min(10, (int)Math.Round(1000.0 / (double)numMs.Value)));
            int duration = Math.Max(1, _sequence.Count);
            int render = chkAdditive.Checked ? 3 : 0;
            var sb = new StringBuilder();
            sb.AppendLine($"// Effekt \"{txtName.Text}\" – Basis-ID 0x{id:X4}, {_frames.Count} Art-Items, Sequenz {_sequence.Count}");
            sb.AppendLine("// ServUO / RunUO:");
            sb.AppendLine($"Effects.SendLocationEffect(target.Location, target.Map, 0x{id:X4}, {duration}, {speed}, {(hue > 0 ? hue - 1 : 0)}, {render});");
            sb.AppendLine($"Effects.SendTargetEffect(target, 0x{id:X4}, {speed}, {duration});");
            sb.AppendLine($"Effects.SendMovingEffect(from, target, 0x{id:X4}, 7, 0, false, false, {(hue > 0 ? hue - 1 : 0)}, {render});");
            StudioTextForm.ShowText(this, "Effekt-Script", sb.ToString(), "effect.cs");
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerPlay.Stop();
            foreach (EffectFrame f in _raw)
            {
                f.Image.Dispose();
            }
            foreach (Bitmap b in _frames)
            {
                b.Dispose();
            }
            _ground?.Dispose();
            _figure?.Dispose();
            Image img = viewEffect.Image;
            viewEffect.Image = null;
            img?.Dispose();
            picHueStrip.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
