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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.UoStudio
{
    /// <summary>
    /// Spell- und Effekt-Editor: erstellt Partikel- und Zaubereffekte aus Bildfolgen (Einzelbilder,
    /// GIF oder Sprite-Sheet). Die Frames werden als aufeinanderfolgende Art-Items gespeichert,
    /// mit Tiledata (Animation-Flag) und Animdata-Eintrag. Vorschau additiv auf dunklem Grund oder
    /// auf einem Bodentile, mit Hue-Vorschau.
    /// </summary>
    public class SpellEffectEditorForm : Form
    {
        private readonly List<Bitmap> _raw = new List<Bitmap>();     // imported frames
        private readonly List<Bitmap> _frames = new List<Bitmap>();  // processed frames (UO colours)
        private readonly Timer _timer = new Timer { Interval = 100 };
        private int _frameIndex;
        private Bitmap _ground;

        private readonly PixelView _view = new PixelView { Dock = DockStyle.Fill, Zoom = 2, DarkBackground = true };
        private readonly ListView _lvFrames = new ListView { Dock = DockStyle.Bottom, Height = 110, View = View.LargeIcon, MultiSelect = true, HideSelection = false };
        private readonly ImageList _thumbs = new ImageList { ImageSize = new Size(48, 48), ColorDepth = ColorDepth.Depth32Bit };
        private readonly PictureBox _hueStrip = new PictureBox { Width = 300, Height = 14, BorderStyle = BorderStyle.FixedSingle };
        private readonly ToolStripStatusLabel _status = new ToolStripStatusLabel("Bildfolge (PNG/BMP/GIF) oder Sprite-Sheet importieren.");

        private NumericUpDown _numBlack, _numScale, _numMaxSize, _numMs, _numHue, _numZoom, _numGroundId, _numBrightness;
        private CheckBox _chkBlack, _chkGray, _chkCrop, _chkAdditive, _chkGround, _chkPlay, _chkPartialHue, _chkTranslucent, _chkPingPong;
        private TextBox _txtStartId, _txtName;

        public SpellEffectEditorForm()
        {
            Text = "Spell- & Effekt-Editor (Bildfolge → UO-Effekt)";
            Size = new Size(1180, 780);
            MinimumSize = new Size(950, 620);
            StartPosition = FormStartPosition.CenterScreen;
            BuildUi();
            _timer.Tick += (s, e) =>
            {
                _frameIndex++;
                ShowFrame();
            };
            _timer.Start();
            UpdateHueStrip();
        }

        #region UI

        private void BuildUi()
        {
            var left = new Panel { Dock = DockStyle.Left, Width = 340, AutoScroll = true };
            int y = 6;

            var grpImp = new GroupBox { Text = "Import", Left = 4, Top = y, Width = 316, Height = 84 };
            grpImp.Controls.Add(StudioUi.Button("Bilder / GIF hinzufügen...", 8, 20, 150, (s, e) => AddImages()));
            grpImp.Controls.Add(StudioUi.Button("Sprite-Sheet...", 162, 20, 146, (s, e) => ImportSheet()));
            grpImp.Controls.Add(StudioUi.Button("Aus Art laden", 8, 50, 98, (s, e) => LoadFromArt()));
            grpImp.Controls.Add(StudioUi.Button("Entfernen", 110, 50, 98, (s, e) => RemoveSelected()));
            grpImp.Controls.Add(StudioUi.Button("Alle leeren", 212, 50, 96, (s, e) => ClearAll()));
            left.Controls.Add(grpImp);
            y += 90;

            var grpProc = new GroupBox { Text = "Bearbeitung", Left = 4, Top = y, Width = 316, Height = 170 };
            _chkBlack = StudioUi.Check("Schwarz → transparent, Schwelle:", 8, 22, true);
            grpProc.Controls.Add(_chkBlack);
            _numBlack = StudioUi.Number(240, 20, 60, 0, 254, 16);
            grpProc.Controls.Add(_numBlack);
            _chkGray = StudioUi.Check("Graustufen (voll färbbar per Hue)", 8, 48, false);
            grpProc.Controls.Add(_chkGray);
            grpProc.Controls.Add(StudioUi.Label("Helligkeit:", 8, 74));
            _numBrightness = StudioUi.Number(80, 72, 60, -100, 100, 0);
            grpProc.Controls.Add(_numBrightness);
            grpProc.Controls.Add(StudioUi.Label("Größe %:", 150, 74));
            _numScale = StudioUi.Number(220, 72, 60, 5, 400, 100);
            grpProc.Controls.Add(_numScale);
            grpProc.Controls.Add(StudioUi.Label("Max. Kante px:", 8, 102));
            _numMaxSize = StudioUi.Number(100, 100, 60, 8, 512, 160);
            grpProc.Controls.Add(_numMaxSize);
            _chkCrop = StudioUi.Check("Gemeinsam zuschneiden", 170, 102, true);
            grpProc.Controls.Add(_chkCrop);
            _chkPingPong = StudioUi.Check("Ping-Pong (vor und zurück)", 8, 130, false);
            grpProc.Controls.Add(_chkPingPong);
            left.Controls.Add(grpProc);
            y += 176;

            var grpPrev = new GroupBox { Text = "Vorschau", Left = 4, Top = y, Width = 316, Height = 170 };
            grpPrev.Controls.Add(StudioUi.Label("ms/Frame:", 8, 22));
            _numMs = StudioUi.Number(80, 20, 60, 20, 2000, 100);
            _numMs.ValueChanged += (s, e) => _timer.Interval = (int)_numMs.Value;
            grpPrev.Controls.Add(_numMs);
            _chkPlay = StudioUi.Check("Abspielen", 150, 22, true);
            _chkPlay.CheckedChanged += (s, e) => _timer.Enabled = _chkPlay.Checked;
            grpPrev.Controls.Add(_chkPlay);
            grpPrev.Controls.Add(StudioUi.Label("Zoom:", 236, 22));
            _numZoom = StudioUi.Number(272, 20, 36, 1, 8, 2);
            _numZoom.ValueChanged += (s, e) => _view.Zoom = (int)_numZoom.Value;
            grpPrev.Controls.Add(_numZoom);
            _chkAdditive = StudioUi.Check("Additiv (wie leuchtende Client-Effekte)", 8, 48, true);
            grpPrev.Controls.Add(_chkAdditive);
            _chkGround = StudioUi.Check("Auf Bodentile, Land-ID:", 8, 74, false);
            grpPrev.Controls.Add(_chkGround);
            _numGroundId = StudioUi.Number(170, 72, 70, 0, 0x3FFF, 3);
            _numGroundId.ValueChanged += (s, e) => LoadGround();
            grpPrev.Controls.Add(_numGroundId);
            grpPrev.Controls.Add(StudioUi.Label("Hue:", 8, 102));
            _numHue = StudioUi.Number(50, 100, 70, 0, 3000, 0);
            _numHue.ValueChanged += (s, e) => UpdateHueStrip();
            grpPrev.Controls.Add(_numHue);
            _chkPartialHue = StudioUi.Check("nur Grautöne (PartialHue)", 128, 102, false);
            grpPrev.Controls.Add(_chkPartialHue);
            _hueStrip.Left = 8;
            _hueStrip.Top = 132;
            grpPrev.Controls.Add(_hueStrip);
            left.Controls.Add(grpPrev);
            y += 176;

            var grpExp = new GroupBox { Text = "Export (Art + Tiledata + Animdata)", Left = 4, Top = y, Width = 316, Height = 228 };
            grpExp.Controls.Add(StudioUi.Label("Start-ID:", 8, 22));
            _txtStartId = new TextBox { Left = 80, Top = 20, Width = 80, Text = "0x3E00" };
            grpExp.Controls.Add(_txtStartId);
            grpExp.Controls.Add(StudioUi.Button("Freien Block", 166, 18, 140, (s, e) => FindFreeBlock()));
            grpExp.Controls.Add(StudioUi.Label("Name:", 8, 50));
            _txtName = new TextBox { Left = 80, Top = 48, Width = 226, Text = "magic effect" };
            grpExp.Controls.Add(_txtName);
            _chkTranslucent = StudioUi.Check("Translucent-Flag", 8, 76, false);
            grpExp.Controls.Add(_chkTranslucent);
            grpExp.Controls.Add(StudioUi.Button("In Art / Tiledata / Animdata schreiben", 8, 102, 298, (s, e) => ExportToClient()));
            grpExp.Controls.Add(StudioUi.Button("Art-MUL speichern", 8, 132, 146, (s, e) => Save("Art")));
            grpExp.Controls.Add(StudioUi.Button("tiledata + animdata", 160, 132, 146, (s, e) => Save("TileData")));
            grpExp.Controls.Add(StudioUi.Button("Frames als PNG...", 8, 162, 146, (s, e) => ExportPng()));
            grpExp.Controls.Add(StudioUi.Button("Script-Snippet kopieren", 160, 162, 146, (s, e) => CopyScript()));
            grpExp.Controls.Add(StudioUi.Label("Frame 0 = Basis-ID (wird im Spiel animiert)", 8, 196));
            left.Controls.Add(grpExp);

            foreach (NumericUpDown n in new[] { _numBlack, _numScale, _numMaxSize, _numBrightness })
            {
                n.ValueChanged += (s, e) => Reprocess();
            }
            foreach (CheckBox c in new[] { _chkBlack, _chkGray, _chkCrop, _chkPingPong })
            {
                c.CheckedChanged += (s, e) => Reprocess();
            }
            _chkGround.CheckedChanged += (s, e) =>
            {
                LoadGround();
                ShowFrame();
            };
            _chkAdditive.CheckedChanged += (s, e) => ShowFrame();
            _chkPartialHue.CheckedChanged += (s, e) => ShowFrame();

            _lvFrames.LargeImageList = _thumbs;
            _lvFrames.SelectedIndexChanged += (s, e) =>
            {
                if (_lvFrames.SelectedIndices.Count == 1 && !_chkPlay.Checked)
                {
                    _frameIndex = _lvFrames.SelectedIndices[0];
                    ShowFrame();
                }
            };

            var right = new Panel { Dock = DockStyle.Fill };
            right.Controls.Add(_view);
            right.Controls.Add(_lvFrames);

            var status = new StatusStrip();
            status.Items.Add(_status);

            Controls.Add(right);
            Controls.Add(left);
            Controls.Add(status);
        }

        private void UpdateHueStrip()
        {
            Image old = _hueStrip.Image;
            _hueStrip.Image = StudioUi.HueStrip(StudioUi.GetHue((int)_numHue.Value), _hueStrip.Width, _hueStrip.Height);
            old?.Dispose();
            ShowFrame();
        }

        private void LoadGround()
        {
            _ground?.Dispose();
            _ground = null;
            if (!_chkGround.Checked)
            {
                return;
            }
            try
            {
                int id = (int)_numGroundId.Value;
                if (Art.IsValidLand(id))
                {
                    using Bitmap land = Art.GetLand(id);
                    _ground = StudioImaging.To32(land);
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
                foreach (string f in dlg.FileNames.OrderBy(f => f, new NaturalComparer()))
                {
                    _raw.AddRange(StudioImaging.LoadFrames(f));
                }
                Reprocess();
                _status.Text = $"{_raw.Count} Frames geladen.";
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
            using var ask = new Form
            {
                Text = "Sprite-Sheet aufteilen",
                Size = new Size(320, 170),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false
            };
            int guessCell = Math.Min(sheet.Width, sheet.Height);
            NumericUpDown cols = StudioUi.Number(120, 12, 70, 1, 256, Math.Max(1, sheet.Width / Math.Max(1, guessCell)));
            NumericUpDown rows = StudioUi.Number(120, 42, 70, 1, 256, Math.Max(1, sheet.Height / Math.Max(1, guessCell)));
            ask.Controls.Add(StudioUi.Label("Spalten:", 12, 12));
            ask.Controls.Add(cols);
            ask.Controls.Add(StudioUi.Label("Reihen:", 12, 42));
            ask.Controls.Add(rows);
            ask.Controls.Add(StudioUi.Label($"Sheet: {sheet.Width}x{sheet.Height}", 200, 12));
            var ok = new Button { Text = "OK", Left = 120, Top = 84, Width = 80, DialogResult = DialogResult.OK };
            ask.Controls.Add(ok);
            ask.AcceptButton = ok;
            if (ask.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            foreach (List<Bitmap> row in StudioImaging.SliceSheet(sheet, (int)cols.Value, (int)rows.Value, false))
            {
                _raw.AddRange(row);
            }
            Reprocess();
            _status.Text = $"{_raw.Count} Frames aus Sprite-Sheet.";
        }

        private void LoadFromArt()
        {
            if (!StudioUi.TryParseId(_txtStartId.Text, out int id))
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
                    _numMs.Value = Math.Max(_numMs.Minimum, Math.Min(_numMs.Maximum, Math.Max(1, (int)entry.FrameInterval) * 100));
                }
                else
                {
                    ids.Add(id);
                }

                ClearAll();
                foreach (int a in ids)
                {
                    if (Art.IsValidStatic(a))
                    {
                        using Bitmap b = Art.GetStatic(a);
                        _raw.Add(StudioImaging.To32(b));
                    }
                }
                _chkBlack.Checked = false;
                Reprocess();
                _status.Text = entry != null ? $"Animierter Effekt 0x{id:X4} mit {ids.Count} Frames geladen." : $"Art 0x{id:X4} geladen (kein Animdata-Eintrag).";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void RemoveSelected()
        {
            foreach (int i in _lvFrames.SelectedIndices.Cast<int>().OrderByDescending(i => i))
            {
                int rawIndex = RawIndexOf(i);
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
                if (_chkBlack.Checked)
                {
                    b = Swap(b, StudioImaging.BlackToTransparent(b, (int)_numBlack.Value));
                }
                if (_numBrightness.Value != 0)
                {
                    b = Swap(b, StudioImaging.AdjustColors(b, (int)_numBrightness.Value, 0, 100));
                }
                if (_chkGray.Checked)
                {
                    b = Swap(b, StudioImaging.ToGray(b));
                }
                if (_numScale.Value != 100)
                {
                    float f = (float)_numScale.Value / 100f;
                    b = Swap(b, StudioImaging.Resize(b, Math.Max(1, (int)(b.Width * f)), Math.Max(1, (int)(b.Height * f)), InterpolationMode.HighQualityBicubic));
                }
                int max = (int)_numMaxSize.Value;
                if (b.Width > max || b.Height > max)
                {
                    b = Swap(b, StudioImaging.ResizeFit(b, max, max, InterpolationMode.HighQualityBicubic));
                }
                b = Swap(b, StudioImaging.HardAlpha(b, 100));
                tmp.Add(b);
            }

            if (_chkCrop.Checked && tmp.Count > 0)
            {
                CropUnion(tmp);
            }

            foreach (Bitmap b in tmp)
            {
                _frames.Add(StudioImaging.ToUoColors(b));
                b.Dispose();
            }

            if (_chkPingPong.Checked && _frames.Count > 2)
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
                _status.Text = $"Achtung: {_frames.Count} Frames – Animdata erlaubt max. 64 Frames.";
            }
            else if (_frames.Count > 0)
            {
                _status.Text = $"{_frames.Count} Frames, {_frames[0].Width}x{_frames[0].Height} Pixel.";
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
            _lvFrames.BeginUpdate();
            _lvFrames.Items.Clear();
            foreach (Image i in _thumbs.Images)
            {
                i.Dispose();
            }
            _thumbs.Images.Clear();
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
                _thumbs.Images.Add(t);
                _lvFrames.Items.Add(new ListViewItem(i.ToString(), i));
            }
            _lvFrames.EndUpdate();
        }

        private void ShowFrame()
        {
            Image old = _view.Image;
            if (_frames.Count == 0)
            {
                _view.Image = null;
                _view.Caption = "Keine Frames";
                old?.Dispose();
                return;
            }

            Bitmap frame = _frames[_frameIndex % _frames.Count];
            Hue hue = StudioUi.GetHue((int)_numHue.Value);
            using Bitmap hued = hue != null ? StudioImaging.ApplyHue(frame, hue, _chkPartialHue.Checked) : new Bitmap(frame);

            int w = Math.Max(hued.Width, _ground?.Width ?? 0) + 20;
            int h = Math.Max(hued.Height, _ground?.Height ?? 0) + 20;
            var scene = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(scene))
            {
                g.Clear(_chkAdditive.Checked || _ground != null ? Color.FromArgb(20, 20, 24) : Color.Transparent);
                if (_ground != null)
                {
                    // ground tile centred at the bottom like an effect standing on a tile
                    g.DrawImageUnscaled(_ground, (w - _ground.Width) / 2, h - _ground.Height - 4);
                }
            }

            int fx = (w - hued.Width) / 2;
            int fy = _ground != null ? h - hued.Height - 4 - (_ground.Height / 2) + 22 : (h - hued.Height) / 2;
            fy = Math.Max(0, Math.Min(h - hued.Height, fy));
            if (_chkAdditive.Checked)
            {
                BlendAdditive(scene, hued, fx, fy);
            }
            else
            {
                using Graphics g = Graphics.FromImage(scene);
                g.DrawImageUnscaled(hued, fx, fy);
            }

            _view.Image = scene;
            _view.Caption = $"Frame {(_frameIndex % _frames.Count) + 1}/{_frames.Count}" + (hue != null ? $"  Hue {(int)_numHue.Value} {hue.Name}" : string.Empty);
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
                        _txtStartId.Text = $"0x{start:X4}";
                        _status.Text = $"Freier Block für {need} Frames: 0x{start:X4} – 0x{i:X4}";
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
            if (!StudioUi.TryParseId(_txtStartId.Text, out int start) || start < 0)
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
                        string name = _txtName.Text.Length > 20 ? _txtName.Text.Substring(0, 20) : _txtName.Text;
                        TileData.ItemTable[id].Name = name;
                        TileFlag flags = TileData.ItemTable[id].Flags & ~TileFlag.Animation;
                        if (i == 0)
                        {
                            flags |= TileFlag.Animation;
                        }
                        flags = _chkTranslucent.Checked ? flags | TileFlag.Translucent : flags & ~TileFlag.Translucent;
                        flags = _chkPartialHue.Checked ? flags | TileFlag.PartialHue : flags & ~TileFlag.PartialHue;
                        TileData.ItemTable[id].Flags = flags;
                        TileData.ItemTable[id].Height = 0;
                    }
                }

                var frameData = new sbyte[64];
                for (int i = 0; i < _frames.Count; i++)
                {
                    frameData[i] = (sbyte)i;
                }
                byte interval = (byte)Math.Max(1, Math.Min(255, (int)Math.Round((double)_numMs.Value / 100.0)));
                Animdata.AnimData ??= new Dictionary<int, Animdata.AnimdataEntry>();
                Animdata.AnimData[start] = new Animdata.AnimdataEntry(frameData, 0, (byte)_frames.Count, interval, 0);

                StudioUi.MarkChanged("Art");
                StudioUi.MarkChanged("TileData");
                StudioUi.MarkChanged("Animdata");
                _status.Text = $"Effekt geschrieben: Items 0x{start:X4} – 0x{start + _frames.Count - 1:X4}, Animdata-Intervall {interval}. Jetzt speichern.";
                StudioUi.Info(this, _status.Text + "\n\nBitte \"Art-MUL speichern\" und \"tiledata + animdata\" ausführen.");
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
            _status.Text = $"{_frames.Count} Frames gespeichert in {dlg.SelectedPath}";
        }

        private void CopyScript()
        {
            StudioUi.TryParseId(_txtStartId.Text, out int id);
            int hue = (int)_numHue.Value;
            int speed = Math.Max(1, Math.Min(10, (int)Math.Round(1000.0 / (double)_numMs.Value)));
            int duration = Math.Max(1, _frames.Count);
            var sb = new StringBuilder();
            sb.AppendLine($"// Effekt \"{_txtName.Text}\" – Basis-ID 0x{id:X4}, {_frames.Count} Frames");
            sb.AppendLine("// ServUO / RunUO:");
            sb.AppendLine($"Effects.SendLocationEffect(target.Location, target.Map, 0x{id:X4}, {duration}, {speed}, {(hue > 0 ? hue - 1 : 0)}, {(_chkAdditive.Checked ? 3 : 0)});");
            sb.AppendLine($"Effects.SendTargetEffect(target, 0x{id:X4}, {speed}, {duration});");
            sb.AppendLine($"Effects.SendMovingEffect(from, target, 0x{id:X4}, 7, 0, false, false, {(hue > 0 ? hue - 1 : 0)}, {(_chkAdditive.Checked ? 3 : 0)});");
            Clipboard.SetText(sb.ToString());
            _status.Text = "Script-Snippet kopiert.";
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Dispose();
            foreach (Bitmap b in _raw.Concat(_frames))
            {
                b.Dispose();
            }
            _ground?.Dispose();
            _view.Image?.Dispose();
            _hueStrip.Image?.Dispose();
            _thumbs.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>Sorts "frame2.png" before "frame10.png".</summary>
        private sealed class NaturalComparer : IComparer<string>
        {
            public int Compare(string a, string b)
            {
                a ??= string.Empty;
                b ??= string.Empty;
                int i = 0, j = 0;
                while (i < a.Length && j < b.Length)
                {
                    if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
                    {
                        int si = i, sj = j;
                        while (i < a.Length && char.IsDigit(a[i])) { i++; }
                        while (j < b.Length && char.IsDigit(b[j])) { j++; }
                        string na = a.Substring(si, i - si).TrimStart('0'), nb = b.Substring(sj, j - sj).TrimStart('0');
                        int c = na.Length != nb.Length ? na.Length.CompareTo(nb.Length) : string.CompareOrdinal(na, nb);
                        if (c != 0)
                        {
                            return c;
                        }
                    }
                    else
                    {
                        int c = char.ToLowerInvariant(a[i]).CompareTo(char.ToLowerInvariant(b[j]));
                        if (c != 0)
                        {
                            return c;
                        }
                        i++;
                        j++;
                    }
                }
                return (a.Length - i).CompareTo(b.Length - j);
            }
        }
    }
}
