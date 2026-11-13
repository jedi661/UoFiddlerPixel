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
    /// GIF oder Sprite-Sheet). Die Frames werden als aufeinanderfolgende Art-Items gespeichert,
    /// mit Tiledata (Animation-Flag) und Animdata-Eintrag. Vorschau additiv auf dunklem Grund oder
    /// auf einem Bodentile, mit Hue-Vorschau.
    /// </summary>
    public partial class SpellEffectEditorForm : Form
    {
        private readonly List<Bitmap> _raw = new List<Bitmap>();     // imported frames
        private readonly List<Bitmap> _frames = new List<Bitmap>();  // processed frames (UO colours)
        private int _frameIndex;
        private Bitmap _ground;

        public SpellEffectEditorForm()
        {
            InitializeComponent();
            lvFrames.LargeImageList = imageListThumbs;
            UpdateHueStrip();
            timerPlay.Start();
        }

        #region Events

        private void TimerPlay_Tick(object sender, EventArgs e)
        {
            _frameIndex++;
            ShowFrame();
        }

        private void Process_Changed(object sender, EventArgs e) => Reprocess();

        private void Preview_Changed(object sender, EventArgs e) => ShowFrame();

        private void Ground_Changed(object sender, EventArgs e)
        {
            LoadGround();
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
            if (lvFrames.SelectedIndices.Count == 1 && !chkPlay.Checked)
            {
                _frameIndex = lvFrames.SelectedIndices[0];
                ShowFrame();
            }
        }

        private void BtnAddImages_Click(object sender, EventArgs e) => AddImages();

        private void BtnImportSheet_Click(object sender, EventArgs e) => ImportSheet();

        private void BtnLoadArt_Click(object sender, EventArgs e) => LoadFromArt();

        private void BtnRemove_Click(object sender, EventArgs e) => RemoveSelected();

        private void BtnClear_Click(object sender, EventArgs e) => ClearAll();

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
            if (!chkGround.Checked)
            {
                return;
            }
            try
            {
                int id = (int)numGroundId.Value;
                if (Art.IsValidLand(id))
                {
                    _ground = UltimaImages.CopyLand(id);
                }
            }
            catch
            {
                _ground = null;
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
                    _raw.AddRange(StudioImaging.LoadFrames(f));
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
                _raw.AddRange(row);
            }
            Reprocess();
            lblStatus.Text = $"{_raw.Count} Frames aus Sprite-Sheet.";
        }

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
                var ids = new List<int>();
                if (entry != null && entry.FrameCount > 0)
                {
                    for (int i = 0; i < entry.FrameCount && i < 64; i++)
                    {
                        ids.Add(id + entry.FrameData[i]);
                    }
                    numMs.Value = Math.Max(numMs.Minimum, Math.Min(numMs.Maximum, Math.Max(1, (int)entry.FrameInterval) * 100));
                }
                else
                {
                    ids.Add(id);
                }

                ClearAll();
                foreach (int a in ids)
                {
                    Bitmap b = UltimaImages.CopyStatic(a);
                    if (b != null)
                    {
                        _raw.Add(b);
                    }
                }
                chkBlack.Checked = false;
                Reprocess();
                lblStatus.Text = entry != null ? $"Animierter Effekt 0x{id:X4} mit {ids.Count} Frames geladen." : $"Art 0x{id:X4} geladen (kein Animdata-Eintrag).";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void RemoveSelected()
        {
            // with ping-pong two list entries can point to the same source frame -> distinct
            foreach (int rawIndex in lvFrames.SelectedIndices.Cast<int>().Select(RawIndexOf).Distinct().OrderByDescending(i => i))
            {
                if (rawIndex >= 0 && rawIndex < _raw.Count)
                {
                    _raw[rawIndex].Dispose();
                    _raw.RemoveAt(rawIndex);
                }
            }
            Reprocess();
        }

        private int RawIndexOf(int processedIndex)
        {
            // with ping-pong the second half mirrors the first half
            if (processedIndex < _raw.Count)
            {
                return processedIndex;
            }
            return (2 * _raw.Count) - 2 - processedIndex;
        }

        private void ClearAll()
        {
            foreach (Bitmap b in _raw)
            {
                b.Dispose();
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
            foreach (Bitmap src in _raw)
            {
                Bitmap b = new Bitmap(src);
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
                b = Swap(b, StudioImaging.HardAlpha(b, 100));
                tmp.Add(b);
            }

            if (chkCrop.Checked && tmp.Count > 0)
            {
                CropUnion(tmp);
            }

            foreach (Bitmap b in tmp)
            {
                _frames.Add(StudioImaging.ToUoColors(b));
                b.Dispose();
            }

            if (chkPingPong.Checked && _frames.Count > 2)
            {
                for (int i = _frames.Count - 2; i > 0; i--)
                {
                    _frames.Add(new Bitmap(_frames[i]));
                }
            }

            RefreshThumbs();
            ShowFrame();
            if (_frames.Count > 64)
            {
                lblStatus.Text = $"Achtung: {_frames.Count} Frames – Animdata erlaubt max. 64 Frames.";
            }
            else if (_frames.Count > 0)
            {
                lblStatus.Text = $"{_frames.Count} Frames, {_frames[0].Width}x{_frames[0].Height} Pixel.";
            }
        }

        private static Bitmap Swap(Bitmap old, Bitmap neu)
        {
            if (!ReferenceEquals(old, neu))
            {
                old.Dispose();
            }
            return neu;
        }

        /// <summary>Crops all frames to the union of their content, centred (effects stay in place).</summary>
        private static void CropUnion(List<Bitmap> frames)
        {
            int w = frames.Max(f => f.Width), h = frames.Max(f => f.Height);
            int minX = w, minY = h, maxX = -1, maxY = -1;
            foreach (Bitmap f in frames)
            {
                int ox = (w - f.Width) / 2, oy = (h - f.Height) / 2;
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
            if (maxX < 0)
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
                    int ox = (w - frames[i].Width) / 2, oy = (h - frames[i].Height) / 2;
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
            foreach (Image i in imageListThumbs.Images)
            {
                i.Dispose();
            }
            imageListThumbs.Images.Clear();
            for (int i = 0; i < _frames.Count; i++)
            {
                var t = new Bitmap(48, 48);
                using (Graphics g = Graphics.FromImage(t))
                {
                    g.Clear(Color.FromArgb(24, 24, 28));
                    Bitmap f = _frames[i];
                    float s = Math.Min(1f, Math.Min(46f / f.Width, 46f / f.Height));
                    int w = (int)(f.Width * s), h = (int)(f.Height * s);
                    g.DrawImage(f, (48 - w) / 2, (48 - h) / 2, w, h);
                }
                imageListThumbs.Images.Add(t);
                lvFrames.Items.Add(new ListViewItem(i.ToString(), i));
            }
            lvFrames.EndUpdate();
        }

        private void ShowFrame()
        {
            Image old = viewEffect.Image;
            if (_frames.Count == 0)
            {
                viewEffect.Image = null;
                viewEffect.Caption = "Keine Frames";
                old?.Dispose();
                return;
            }

            Bitmap frame = _frames[_frameIndex % _frames.Count];
            Hue hue = StudioUi.GetHue((int)numHue.Value);
            using Bitmap hued = hue != null ? StudioImaging.ApplyHue(frame, hue, chkPartialHue.Checked) : new Bitmap(frame);

            int w = Math.Max(hued.Width, _ground?.Width ?? 0) + 20;
            int h = Math.Max(hued.Height, _ground?.Height ?? 0) + 20;
            var scene = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(scene))
            {
                g.Clear(chkAdditive.Checked || _ground != null ? Color.FromArgb(20, 20, 24) : Color.Transparent);
                if (_ground != null)
                {
                    // ground tile centred at the bottom like an effect standing on a tile
                    g.DrawImageUnscaled(_ground, (w - _ground.Width) / 2, h - _ground.Height - 4);
                }
            }

            int fx = (w - hued.Width) / 2;
            int fy = _ground != null ? h - hued.Height - 4 - (_ground.Height / 2) + 22 : (h - hued.Height) / 2;
            fy = Math.Max(0, Math.Min(h - hued.Height, fy));
            if (chkAdditive.Checked)
            {
                BlendAdditive(scene, hued, fx, fy);
            }
            else
            {
                using Graphics g = Graphics.FromImage(scene);
                g.DrawImageUnscaled(hued, fx, fy);
            }

            viewEffect.Image = scene;
            viewEffect.Caption = $"Frame {(_frameIndex % _frames.Count) + 1}/{_frames.Count}" + (hue != null ? $"  Hue {(int)numHue.Value} {hue.Name}" : string.Empty);
            old?.Dispose();
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
            if (_frames.Count > 64)
            {
                StudioUi.Error(this, "Animdata erlaubt maximal 64 Frames. Bitte Frames entfernen.");
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

                var frameData = new sbyte[64];
                for (int i = 0; i < _frames.Count; i++)
                {
                    frameData[i] = (sbyte)i;
                }
                byte interval = (byte)Math.Max(1, Math.Min(255, (int)Math.Round((double)numMs.Value / 100.0)));
                Animdata.AnimData ??= new Dictionary<int, Animdata.AnimdataEntry>();
                Animdata.AnimData[start] = new Animdata.AnimdataEntry(frameData, 0, (byte)_frames.Count, interval, 0);

                StudioUi.MarkChanged("Art");
                StudioUi.MarkChanged("TileData");
                StudioUi.MarkChanged("Animdata");
                lblStatus.Text = $"Effekt geschrieben: Items 0x{start:X4} – 0x{start + _frames.Count - 1:X4}, Animdata-Intervall {interval}. Jetzt speichern.";
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
            int duration = Math.Max(1, _frames.Count);
            var sb = new StringBuilder();
            sb.AppendLine($"// Effekt \"{txtName.Text}\" – Basis-ID 0x{id:X4}, {_frames.Count} Frames");
            sb.AppendLine("// ServUO / RunUO:");
            sb.AppendLine($"Effects.SendLocationEffect(target.Location, target.Map, 0x{id:X4}, {duration}, {speed}, {(hue > 0 ? hue - 1 : 0)}, {(chkAdditive.Checked ? 3 : 0)});");
            sb.AppendLine($"Effects.SendTargetEffect(target, 0x{id:X4}, {speed}, {duration});");
            sb.AppendLine($"Effects.SendMovingEffect(from, target, 0x{id:X4}, 7, 0, false, false, {(hue > 0 ? hue - 1 : 0)}, {(chkAdditive.Checked ? 3 : 0)});");
            Clipboard.SetText(sb.ToString());
            lblStatus.Text = "Script-Snippet kopiert.";
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerPlay.Stop();
            foreach (Bitmap b in _raw.Concat(_frames))
            {
                b.Dispose();
            }
            _ground?.Dispose();
            viewEffect.Image?.Dispose();
            picHueStrip.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
