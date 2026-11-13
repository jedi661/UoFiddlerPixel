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
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.UoStudio
{
    /// <summary>
    /// Animations-Studio: baut neue Mobile-Animationen aus Einzelbildern oder Sprite-Sheets.
    /// UO speichert 5 Richtungen (0 = unten, 1 = unten-links, 2 = links, 3 = oben-links, 4 = oben),
    /// die 3 rechten Richtungen erzeugt der Client durch Spiegeln. Die Vorschau zeigt alle 8 Richtungen.
    /// </summary>
    public class AnimationStudioForm : Form
    {
        private const int DirCount = 5;

        private static readonly string[] _dirNames =
        {
            "0  ↓  unten (Süd-Ost)",
            "1  ↙  unten-links (Süd)",
            "2  ←  links (Süd-West)",
            "3  ↖  oben-links (West)",
            "4  ↑  oben (Nord-West)"
        };

        private readonly List<Bitmap>[] _dirs = Enumerable.Range(0, DirCount).Select(_ => new List<Bitmap>()).ToArray();
        private readonly Timer _timer = new Timer { Interval = 120 };
        private int _frameIndex;

        private ListBox _lbDirs;
        private ListView _lvFrames;
        private readonly ImageList _thumbs = new ImageList { ImageSize = new Size(48, 48), ColorDepth = ColorDepth.Depth32Bit };
        private NumericUpDown _numDelay, _numZoom, _numCenterX, _numCenterY;
        private CheckBox _chkPlay, _chkCrosshair;
        private ComboBox _cbFileType;
        private NumericUpDown _numBody, _numAction, _numColors;
        private readonly PixelView[] _compass = new PixelView[9];
        private readonly ToolStripStatusLabel _status = new ToolStripStatusLabel("Bilder oder ein Sprite-Sheet importieren.");

        // compass cell -> (anim direction, mirrored); index 4 = big preview of selected direction
        private static readonly (int Dir, bool Mirror, string Name)[] _cells =
        {
            (3, false, "↖ 3"), (4, false, "↑ 4"), (3, true, "↗ 3 gespiegelt"),
            (2, false, "← 2"), (-1, false, "Auswahl"), (2, true, "→ 2 gespiegelt"),
            (1, false, "↙ 1"), (0, false, "↓ 0"), (1, true, "↘ 1 gespiegelt")
        };

        public AnimationStudioForm()
        {
            Text = "Animations-Studio (Mobile-Animationen, 5 Richtungen + Spiegeln)";
            Size = new Size(1240, 800);
            MinimumSize = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;

            BuildUi();
            _timer.Tick += (s, e) =>
            {
                _frameIndex++;
                RefreshPreview();
            };
            _timer.Start();
            _lbDirs.SelectedIndex = 0;
        }

        private int CurrentDir => Math.Max(0, _lbDirs.SelectedIndex);

        #region UI

        private void BuildUi()
        {
            var left = new Panel { Dock = DockStyle.Left, Width = 380, AutoScroll = true };
            int y = 6;

            left.Controls.Add(StudioUi.Label("Richtungen (so speichert UO sie):", 6, y));
            y += 22;
            _lbDirs = new ListBox { Left = 6, Top = y, Width = 360, Height = 84, IntegralHeight = false };
            _lbDirs.SelectedIndexChanged += (s, e) => RefreshFrameList();
            left.Controls.Add(_lbDirs);
            y += 90;

            left.Controls.Add(StudioUi.Button("Bilder hinzufügen...", 6, y, 176, (s, e) => AddImages()));
            left.Controls.Add(StudioUi.Button("Sprite-Sheet importieren...", 188, y, 178, (s, e) => ImportSheet()));
            y += 30;
            left.Controls.Add(StudioUi.Button("Aus MUL laden", 6, y, 116, (s, e) => LoadFromMul()));
            left.Controls.Add(StudioUi.Button("Richtung spiegeln", 126, y, 118, (s, e) => MirrorDirection()));
            left.Controls.Add(StudioUi.Button("Leere auffüllen", 248, y, 118, (s, e) => FillEmptyDirections()));
            y += 30;
            left.Controls.Add(StudioUi.Button("◀ Frame", 6, y, 70, (s, e) => MoveFrame(-1)));
            left.Controls.Add(StudioUi.Button("Frame ▶", 80, y, 70, (s, e) => MoveFrame(1)));
            left.Controls.Add(StudioUi.Button("Entfernen", 154, y, 80, (s, e) => RemoveFrames()));
            left.Controls.Add(StudioUi.Button("Richtung leeren", 238, y, 128, (s, e) => ClearDirection()));
            y += 32;

            _lvFrames = new ListView
            {
                Left = 6,
                Top = y,
                Width = 360,
                Height = 150,
                LargeImageList = _thumbs,
                View = View.LargeIcon,
                MultiSelect = true,
                HideSelection = false
            };
            left.Controls.Add(_lvFrames);
            y += 156;

            var grpPlay = new GroupBox { Text = "Vorschau", Left = 4, Top = y, Width = 362, Height = 76 };
            grpPlay.Controls.Add(StudioUi.Label("ms/Frame:", 8, 20));
            _numDelay = StudioUi.Number(76, 20, 60, 20, 2000, 120);
            _numDelay.ValueChanged += (s, e) => _timer.Interval = (int)_numDelay.Value;
            grpPlay.Controls.Add(_numDelay);
            grpPlay.Controls.Add(StudioUi.Label("Zoom:", 144, 20));
            _numZoom = StudioUi.Number(186, 20, 50, 1, 8, 1);
            _numZoom.ValueChanged += (s, e) => ApplyZoom();
            grpPlay.Controls.Add(_numZoom);
            _chkPlay = StudioUi.Check("Abspielen", 248, 22, true);
            _chkPlay.CheckedChanged += (s, e) => _timer.Enabled = _chkPlay.Checked;
            grpPlay.Controls.Add(_chkPlay);
            _chkCrosshair = StudioUi.Check("Fußpunkt anzeigen", 8, 48, true);
            _chkCrosshair.CheckedChanged += (s, e) => RefreshPreview();
            grpPlay.Controls.Add(_chkCrosshair);
            left.Controls.Add(grpPlay);
            y += 82;

            var grpCenter = new GroupBox { Text = "Fußpunkt (Center) relativ zu unten-Mitte", Left = 4, Top = y, Width = 362, Height = 52 };
            grpCenter.Controls.Add(StudioUi.Label("X:", 8, 20));
            _numCenterX = StudioUi.Number(28, 20, 60, -200, 200, 0);
            _numCenterX.ValueChanged += (s, e) => RefreshPreview();
            grpCenter.Controls.Add(_numCenterX);
            grpCenter.Controls.Add(StudioUi.Label("Y:", 100, 20));
            _numCenterY = StudioUi.Number(120, 20, 60, -200, 200, 0);
            _numCenterY.ValueChanged += (s, e) => RefreshPreview();
            grpCenter.Controls.Add(_numCenterY);
            grpCenter.Controls.Add(StudioUi.Label("(Y positiv = höher)", 190, 20));
            left.Controls.Add(grpCenter);
            y += 58;

            var grpExport = new GroupBox { Text = "Export in Anim-MUL", Left = 4, Top = y, Width = 362, Height = 170 };
            grpExport.Controls.Add(StudioUi.Label("Datei:", 8, 22));
            _cbFileType = StudioUi.Combo(60, 22, 110, "anim.mul", "anim2.mul", "anim3.mul", "anim4.mul", "anim5.mul");
            grpExport.Controls.Add(_cbFileType);
            grpExport.Controls.Add(StudioUi.Label("Farben:", 180, 22));
            _numColors = StudioUi.Number(240, 22, 60, 8, 256, 256);
            grpExport.Controls.Add(_numColors);
            grpExport.Controls.Add(StudioUi.Label("Body:", 8, 52));
            _numBody = StudioUi.Number(60, 52, 70, 0, 5000, 400);
            grpExport.Controls.Add(_numBody);
            grpExport.Controls.Add(StudioUi.Label("Action:", 140, 52));
            _numAction = StudioUi.Number(190, 52, 50, 0, 80, 0);
            grpExport.Controls.Add(_numAction);
            grpExport.Controls.Add(StudioUi.Label("0 Walk, 1 Run/Stand, 2 Idle…", 244, 52));
            grpExport.Controls.Add(StudioUi.Button("In Animation schreiben", 8, 82, 170, (s, e) => ExportToMul()));
            grpExport.Controls.Add(StudioUi.Button("Anim-MUL speichern", 184, 82, 170, (s, e) => SaveMul()));
            grpExport.Controls.Add(StudioUi.Button("Frames als PNG...", 8, 112, 170, (s, e) => ExportPngFrames()));
            grpExport.Controls.Add(StudioUi.Button("Sprite-Sheet als PNG...", 184, 112, 170, (s, e) => ExportSheet()));
            grpExport.Controls.Add(StudioUi.Label("anim.mul: Body <200 Monster (22 Actions), <400 Tiere (13), ab 400 Menschen (35)", 8, 142));
            left.Controls.Add(grpExport);

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3 };
            for (int i = 0; i < 3; i++)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
            }
            for (int i = 0; i < 9; i++)
            {
                var v = new PixelView { Dock = DockStyle.Fill, Caption = _cells[i].Name, Zoom = 1, Margin = new Padding(2) };
                int cell = i;
                if (i == 4)
                {
                    v.PaintOverlay += DrawCrosshair;
                    v.BorderStyle = BorderStyle.Fixed3D;
                }
                else
                {
                    v.Cursor = Cursors.Hand;
                    v.Click += (s, e) =>
                    {
                        if (_cells[cell].Dir >= 0)
                        {
                            _lbDirs.SelectedIndex = _cells[cell].Dir;
                        }
                    };
                }
                _compass[i] = v;
                grid.Controls.Add(v, i % 3, i / 3);
            }

            var status = new StatusStrip();
            status.Items.Add(_status);

            Controls.Add(grid);
            Controls.Add(left);
            Controls.Add(status);

            RefreshDirList();
        }

        private void ApplyZoom()
        {
            foreach (PixelView v in _compass)
            {
                v.Zoom = (int)_numZoom.Value;
            }
            _compass[4].Zoom = (int)_numZoom.Value * 2;
        }

        #endregion

        #region Lists

        private void RefreshDirList()
        {
            int sel = _lbDirs.SelectedIndex;
            _lbDirs.BeginUpdate();
            _lbDirs.Items.Clear();
            for (int i = 0; i < DirCount; i++)
            {
                _lbDirs.Items.Add($"{_dirNames[i]}   [{_dirs[i].Count} Frames]");
            }
            _lbDirs.EndUpdate();
            _lbDirs.SelectedIndex = sel < 0 ? 0 : sel;
        }

        private void RefreshFrameList()
        {
            _lvFrames.BeginUpdate();
            _lvFrames.Items.Clear();
            foreach (Image img in _thumbs.Images)
            {
                img.Dispose();
            }
            _thumbs.Images.Clear();

            List<Bitmap> frames = _dirs[CurrentDir];
            for (int i = 0; i < frames.Count; i++)
            {
                _thumbs.Images.Add(MakeThumb(frames[i]));
                _lvFrames.Items.Add(new ListViewItem($"{i}  ({frames[i].Width}x{frames[i].Height})", i) { Tag = i });
            }
            _lvFrames.EndUpdate();
            RefreshPreview();
        }

        private static Bitmap MakeThumb(Bitmap src)
        {
            var bmp = new Bitmap(48, 48);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(230, 230, 230));
            float f = Math.Min(1f, Math.Min(46f / src.Width, 46f / src.Height));
            int w = (int)(src.Width * f), h = (int)(src.Height * f);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(src, (48 - w) / 2, (48 - h) / 2, w, h);
            return bmp;
        }

        private void Changed()
        {
            int sel = _lbDirs.SelectedIndex;
            RefreshDirList();
            if (sel == _lbDirs.SelectedIndex)
            {
                RefreshFrameList();
            }
        }

        #endregion

        #region Preview

        private void RefreshPreview()
        {
            for (int i = 0; i < 9; i++)
            {
                int dir = _cells[i].Dir < 0 ? CurrentDir : _cells[i].Dir;
                List<Bitmap> frames = _dirs[dir];
                PixelView v = _compass[i];
                Image old = v.Image;
                if (frames.Count == 0)
                {
                    v.Image = null;
                }
                else
                {
                    Bitmap f = frames[_frameIndex % frames.Count];
                    v.Image = _cells[i].Mirror ? StudioImaging.FlipHorizontal(f) : new Bitmap(f);
                }
                old?.Dispose();
                if (i == 4)
                {
                    v.Caption = $"Auswahl: {_dirNames[dir]}  Frame {(frames.Count == 0 ? 0 : (_frameIndex % frames.Count) + 1)}/{frames.Count}";
                }
            }
        }

        private void DrawCrosshair(Graphics g, Rectangle r)
        {
            if (!_chkCrosshair.Checked || _compass[4].Image == null)
            {
                return;
            }
            int zoom = _compass[4].Zoom;
            int w = _compass[4].Image.Width;
            int h = _compass[4].Image.Height;
            // UO: frame is drawn at (x - centerX, y - centerY - height) -> foot point
            int fx = r.X + (((w / 2) + (int)_numCenterX.Value) * zoom);
            int fy = r.Y + ((h + (int)_numCenterY.Value) * zoom);
            using var pen = new Pen(Color.Red, 1);
            g.DrawLine(pen, fx - 12, fy, fx + 12, fy);
            g.DrawLine(pen, fx, fy - 12, fx, fy + 12);
            using var dia = new Pen(Color.FromArgb(140, Color.Blue));
            int t = 22 * zoom;
            g.DrawPolygon(dia, new[] { new Point(fx, fy - t), new Point(fx + t, fy), new Point(fx, fy + t), new Point(fx - t, fy) });
        }

        #endregion

        #region Import

        private void AddImages()
        {
            using var dlg = new OpenFileDialog { Filter = StudioUi.ImageFilter, Multiselect = true, Title = "Einzelbilder (oder GIF) für die gewählte Richtung" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            try
            {
                foreach (string file in dlg.FileNames.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    _dirs[CurrentDir].AddRange(StudioImaging.LoadFrames(file));
                }
                Changed();
                _status.Text = $"{dlg.FileNames.Length} Datei(en) zu Richtung {CurrentDir} hinzugefügt.";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void ImportSheet()
        {
            using var ofd = new OpenFileDialog { Filter = StudioUi.ImageFilter, Title = "Sprite-Sheet laden (Spalten = Frames, Reihen = Richtungen)" };
            if (ofd.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Bitmap sheet;
            try
            {
                sheet = StudioImaging.LoadImage(ofd.FileName);
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
                return;
            }

            using (sheet)
            using (var dlg = new SheetImportDialog(sheet))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<List<Bitmap>> rows = StudioImaging.SliceSheet(sheet, dlg.Columns, dlg.Rows, false);
                if (dlg.RemoveBackground)
                {
                    Color key = sheet.GetPixel(0, 0);
                    foreach (List<Bitmap> row in rows)
                    {
                        for (int i = 0; i < row.Count; i++)
                        {
                            Bitmap nb = StudioImaging.RemoveBackground(row[i], key, 12);
                            row[i].Dispose();
                            row[i] = nb;
                        }
                    }
                }

                // rows -> anim directions
                switch (dlg.Mapping)
                {
                    case 0: // 5 rows UO order
                        for (int d = 0; d < DirCount && d < rows.Count; d++)
                        {
                            Replace(d, rows[d], false);
                        }
                        break;
                    case 1: // 5 rows right side: up, up-right, right, down-right, down
                        if (rows.Count >= 5)
                        {
                            Replace(4, rows[0], false);
                            Replace(3, rows[1], true);
                            Replace(2, rows[2], true);
                            Replace(1, rows[3], true);
                            Replace(0, rows[4], false);
                        }
                        break;
                    case 2: // 8 rows game direction order N, NE, E, SE, S, SW, W, NW
                        if (rows.Count >= 8)
                        {
                            Replace(0, rows[3], false);
                            Replace(1, rows[4], false);
                            Replace(2, rows[5], false);
                            Replace(3, rows[6], false);
                            Replace(4, rows[7], false);
                        }
                        break;
                    case 3: // 8 rows clockwise from down: S(↓), SW(↙), W(←), NW(↖), N(↑), NE, E, SE
                        if (rows.Count >= 5)
                        {
                            for (int d = 0; d < DirCount; d++)
                            {
                                Replace(d, rows[d], false);
                            }
                        }
                        break;
                    default: // all rows into the current direction
                        Replace(CurrentDir, rows.SelectMany(r => r).ToList(), false);
                        break;
                }

                if (dlg.CropUnion)
                {
                    for (int d = 0; d < DirCount; d++)
                    {
                        CropToUnion(_dirs[d]);
                    }
                }

                // dispose unused rows
                foreach (Bitmap b in rows.SelectMany(r => r))
                {
                    if (!_dirs.Any(list => list.Contains(b)))
                    {
                        b.Dispose();
                    }
                }
            }

            Changed();
            _status.Text = "Sprite-Sheet importiert.";
        }

        private void Replace(int dir, List<Bitmap> frames, bool mirror)
        {
            foreach (Bitmap b in _dirs[dir])
            {
                b.Dispose();
            }
            _dirs[dir].Clear();
            foreach (Bitmap f in frames)
            {
                _dirs[dir].Add(mirror ? StudioImaging.FlipHorizontal(f) : new Bitmap(f));
            }
        }

        /// <summary>Crops all frames of one direction to the common opaque bounding box (keeps alignment).</summary>
        private static void CropToUnion(List<Bitmap> frames)
        {
            if (frames.Count == 0)
            {
                return;
            }
            int w = frames.Max(f => f.Width), h = frames.Max(f => f.Height);
            int minX = w, minY = h, maxX = -1, maxY = -1;
            foreach (Bitmap f in frames)
            {
                int[] p = StudioImaging.GetPixels(f);
                for (int y = 0; y < f.Height; y++)
                {
                    for (int x = 0; x < f.Width; x++)
                    {
                        if (((p[(y * f.Width) + x] >> 24) & 0xFF) > 0)
                        {
                            minX = Math.Min(minX, x);
                            minY = Math.Min(minY, y);
                            maxX = Math.Max(maxX, x);
                            maxY = Math.Max(maxY, y);
                        }
                    }
                }
            }
            if (maxX < 0)
            {
                return;
            }
            // keep horizontal symmetry around the original centre so the foot point stays in the middle
            int cx = w / 2;
            int half = Math.Max(cx - minX, maxX - cx) + 1;
            var rect = new Rectangle(cx - half, minY, half * 2, maxY - minY + 1);
            for (int i = 0; i < frames.Count; i++)
            {
                using Bitmap padded = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(padded))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImageUnscaled(frames[i], 0, 0);
                }
                Bitmap c = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(c))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(padded, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
                }
                frames[i].Dispose();
                frames[i] = c;
            }
        }

        private void LoadFromMul()
        {
            try
            {
                int fileType = _cbFileType.SelectedIndex + 1;
                int body = (int)_numBody.Value, action = (int)_numAction.Value;
                int loaded = 0;
                for (int d = 0; d < DirCount; d++)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(fileType, body, action, d);
                    Bitmap[] frames = edit?.GetFrames();
                    var list = new List<Bitmap>();
                    if (frames != null)
                    {
                        foreach (Bitmap f in frames)
                        {
                            if (f != null)
                            {
                                list.Add(StudioImaging.To32(f));
                            }
                        }
                    }
                    Replace(d, list, false);
                    foreach (Bitmap b in list)
                    {
                        b.Dispose();
                    }
                    loaded += _dirs[d].Count;
                }
                Changed();
                _status.Text = $"Body {body}, Action {action} aus {_cbFileType.Text} geladen ({loaded} Frames).";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, "Animation konnte nicht geladen werden:\n" + ex.Message);
            }
        }

        #endregion

        #region Frame editing

        private void MirrorDirection()
        {
            List<Bitmap> frames = _dirs[CurrentDir];
            for (int i = 0; i < frames.Count; i++)
            {
                Bitmap m = StudioImaging.FlipHorizontal(frames[i]);
                frames[i].Dispose();
                frames[i] = m;
            }
            Changed();
        }

        /// <summary>Empty directions are filled from the nearest filled direction (copy).</summary>
        private void FillEmptyDirections()
        {
            int filled = 0;
            for (int d = 0; d < DirCount; d++)
            {
                if (_dirs[d].Count > 0)
                {
                    continue;
                }
                for (int dist = 1; dist < DirCount; dist++)
                {
                    int src = d - dist >= 0 && _dirs[d - dist].Count > 0 ? d - dist :
                              d + dist < DirCount && _dirs[d + dist].Count > 0 ? d + dist : -1;
                    if (src >= 0)
                    {
                        Replace(d, _dirs[src], false);
                        filled++;
                        break;
                    }
                }
            }
            Changed();
            _status.Text = filled == 0 ? "Keine leeren Richtungen." : $"{filled} leere Richtung(en) aufgefüllt (Kopie der Nachbarrichtung).";
        }

        private void MoveFrame(int delta)
        {
            if (_lvFrames.SelectedItems.Count != 1)
            {
                return;
            }
            List<Bitmap> frames = _dirs[CurrentDir];
            int i = (int)_lvFrames.SelectedItems[0].Tag;
            int j = i + delta;
            if (j < 0 || j >= frames.Count)
            {
                return;
            }
            (frames[i], frames[j]) = (frames[j], frames[i]);
            RefreshFrameList();
            _lvFrames.Items[j].Selected = true;
        }

        private void RemoveFrames()
        {
            List<Bitmap> frames = _dirs[CurrentDir];
            foreach (int i in _lvFrames.SelectedItems.Cast<ListViewItem>().Select(it => (int)it.Tag).OrderByDescending(i => i))
            {
                frames[i].Dispose();
                frames.RemoveAt(i);
            }
            Changed();
        }

        private void ClearDirection()
        {
            Replace(CurrentDir, new List<Bitmap>(), false);
            Changed();
        }

        #endregion

        #region Export

        private void ExportToMul()
        {
            if (_dirs.All(d => d.Count == 0))
            {
                StudioUi.Info(this, "Keine Frames vorhanden.");
                return;
            }
            if (_dirs.Any(d => d.Count == 0) &&
                !StudioUi.Ask(this, "Nicht alle 5 Richtungen haben Frames. Leere Richtungen werden in der MUL geleert.\nTrotzdem fortfahren? (Tipp: \"Leere auffüllen\")"))
            {
                return;
            }

            int fileType = _cbFileType.SelectedIndex + 1;
            int body = (int)_numBody.Value, action = (int)_numAction.Value;

            if (fileType == 1)
            {
                int maxActions = body < 200 ? 22 : body < 400 ? 13 : 35;
                if (action >= maxActions)
                {
                    StudioUi.Error(this, $"Body {body} hat in anim.mul nur {maxActions} Actions (0–{maxActions - 1}).");
                    return;
                }
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                for (int d = 0; d < DirCount; d++)
                {
                    AnimIdx edit = AnimationEdit.GetAnimation(fileType, body, action, d);
                    if (edit == null)
                    {
                        StudioUi.Error(this, $"Body {body} / Action {action} / Richtung {d} existiert in {_cbFileType.Text} nicht (Index außerhalb der Datei).");
                        return;
                    }

                    List<Bitmap> frames = _dirs[d];
                    edit.ClearFrames();
                    if (frames.Count == 0)
                    {
                        continue;
                    }

                    // palette with max. 256 colours shared by all frames of this direction
                    List<Bitmap> uoFrames = BuildUoFrames(frames, (int)_numColors.Value, out ushort[] palette);
                    edit.ReplacePalette(palette);
                    foreach (Bitmap f in uoFrames)
                    {
                        int cx = (f.Width / 2) + (int)_numCenterX.Value;
                        int cy = (int)_numCenterY.Value;
                        edit.AddFrame(f, cx, cy);
                        f.Dispose();
                    }
                }
                StudioUi.MarkChanged("Animations");
                _status.Text = $"Body {body}, Action {action} in {_cbFileType.Text} geschrieben. Mit \"Anim-MUL speichern\" in {StudioUi.OutputPath} sichern.";
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

        /// <summary>Reduces all frames to one palette (max colours) and converts them to ARGB1555.</summary>
        internal static List<Bitmap> BuildUoFrames(List<Bitmap> frames, int maxColors, out ushort[] palette)
        {
            int colors = Math.Min(256, maxColors);
            while (true)
            {
                List<Color> pal = StudioImaging.BuildPalette(frames, colors);
                var result = new List<Bitmap>();
                var set = new HashSet<ushort>();
                foreach (Bitmap f in frames)
                {
                    using Bitmap hard = StudioImaging.HardAlpha(f, 128);
                    using Bitmap q = StudioImaging.ApplyPalette(hard, pal, false);
                    Bitmap b = StudioImaging.To1555(q);
                    CollectColors(b, set);
                    result.Add(b);
                }

                if (set.Count <= 256 || colors <= 8)
                {
                    palette = new ushort[256];
                    int i = 0;
                    foreach (ushort c in set.Take(256))
                    {
                        palette[i++] = c;
                    }
                    return result;
                }

                foreach (Bitmap b in result)
                {
                    b.Dispose();
                }
                colors -= 8;
            }
        }

        private static unsafe void CollectColors(Bitmap b, HashSet<ushort> set)
        {
            BitmapData bd = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            for (int y = 0; y < b.Height; y++)
            {
                ushort* line = (ushort*)(bd.Scan0 + (y * bd.Stride));
                for (int x = 0; x < b.Width; x++)
                {
                    if (line[x] != 0)
                    {
                        set.Add(line[x]);
                    }
                }
            }
            b.UnlockBits(bd);
        }

        private void SaveMul()
        {
            int fileType = _cbFileType.SelectedIndex + 1;
            try
            {
                Cursor = Cursors.WaitCursor;
                AnimationEdit.Save(fileType, StudioUi.OutputPath);
                StudioUi.ResetChanged("Animations");
                StudioUi.Info(this, $"{_cbFileType.Text} gespeichert in:\n{StudioUi.OutputPath}");
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

        private void ExportPngFrames()
        {
            using var dlg = new FolderBrowserDialog { Description = "Zielordner für die Frames", SelectedPath = StudioUi.OutputPath };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            int n = 0;
            for (int d = 0; d < DirCount; d++)
            {
                for (int i = 0; i < _dirs[d].Count; i++)
                {
                    _dirs[d][i].Save(Path.Combine(dlg.SelectedPath, $"anim_dir{d}_frame{i:D2}.png"), ImageFormat.Png);
                    n++;
                }
            }
            _status.Text = $"{n} Frames gespeichert in {dlg.SelectedPath}";
        }

        private void ExportSheet()
        {
            int cols = _dirs.Max(d => d.Count);
            if (cols == 0)
            {
                return;
            }
            int cw = _dirs.SelectMany(d => d).Max(b => b.Width);
            int ch = _dirs.SelectMany(d => d).Max(b => b.Height);
            using var sheet = new Bitmap(cw * cols, ch * DirCount, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(sheet))
            {
                g.Clear(Color.Transparent);
                for (int d = 0; d < DirCount; d++)
                {
                    for (int i = 0; i < _dirs[d].Count; i++)
                    {
                        Bitmap f = _dirs[d][i];
                        // bottom centre alignment like in game
                        g.DrawImageUnscaled(f, (i * cw) + ((cw - f.Width) / 2), (d * ch) + (ch - f.Height));
                    }
                }
            }
            using var sfd = new SaveFileDialog { Filter = "PNG|*.png", FileName = "anim_sheet.png", InitialDirectory = StudioUi.OutputPath };
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                sheet.Save(sfd.FileName, ImageFormat.Png);
                _status.Text = $"Sprite-Sheet ({cols} Spalten x 5 Reihen, UO-Reihenfolge) gespeichert.";
            }
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Dispose();
            foreach (Bitmap b in _dirs.SelectMany(d => d))
            {
                b.Dispose();
            }
            foreach (PixelView v in _compass)
            {
                v.Image?.Dispose();
            }
            _thumbs.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>Dialog for slicing a sprite sheet (columns / rows / row order).</summary>
        private sealed class SheetImportDialog : Form
        {
            private readonly NumericUpDown _cols, _rows;
            private readonly ComboBox _mapping;
            private readonly CheckBox _crop, _removeBg;
            private readonly PixelView _view;

            public SheetImportDialog(Bitmap sheet)
            {
                Text = "Sprite-Sheet aufteilen";
                Size = new Size(820, 600);
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;

                var top = new Panel { Dock = DockStyle.Top, Height = 92 };
                top.Controls.Add(StudioUi.Label("Spalten (Frames):", 8, 8));
                _cols = StudioUi.Number(120, 8, 60, 1, 200, 8);
                top.Controls.Add(_cols);
                top.Controls.Add(StudioUi.Label("Reihen:", 190, 8));
                _rows = StudioUi.Number(240, 8, 60, 1, 64, 5);
                top.Controls.Add(_rows);
                top.Controls.Add(StudioUi.Label("Reihenfolge:", 8, 38));
                _mapping = StudioUi.Combo(120, 38, 460,
                    "5 Reihen in UO-Reihenfolge (↓, ↙, ←, ↖, ↑)",
                    "5 Reihen rechte Seite (↑, ↗, →, ↘, ↓) – wird automatisch gespiegelt",
                    "8 Reihen Spielrichtung (↗ N, → NO, ↘ O, ↓ SO, ↙ S, ← SW, ↖ W, ↑ NW)",
                    "8 Reihen ab unten im Uhrzeigersinn (↓, ↙, ←, ↖, ↑, ↗, →, ↘)",
                    "Alle Zellen → aktuell gewählte Richtung");
                top.Controls.Add(_mapping);
                _crop = StudioUi.Check("Gemeinsam zuschneiden", 320, 10, true);
                top.Controls.Add(_crop);
                _removeBg = StudioUi.Check("Hintergrund (Farbe oben links) entfernen", 8, 66, false);
                top.Controls.Add(_removeBg);
                var ok = new Button { Text = "Importieren", Left = 600, Top = 36, Width = 100, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Abbrechen", Left = 706, Top = 36, Width = 90, DialogResult = DialogResult.Cancel };
                top.Controls.Add(ok);
                top.Controls.Add(cancel);
                AcceptButton = ok;
                CancelButton = cancel;

                // guess rows/cols: square cells
                int guessRows = sheet.Height >= sheet.Width / 2 ? 5 : 1;
                _rows.Value = guessRows;
                int cell = Math.Max(1, sheet.Height / guessRows);
                _cols.Value = Math.Max(1, Math.Min(200, sheet.Width / cell));

                _view = new PixelView { Dock = DockStyle.Fill, Image = sheet, Zoom = Math.Max(1, 700 / Math.Max(1, sheet.Width)) };
                _view.PaintOverlay += (g, r) =>
                {
                    using var pen = new Pen(Color.FromArgb(180, Color.Red));
                    float cw = r.Width / (float)_cols.Value, ch = r.Height / (float)_rows.Value;
                    for (int i = 1; i < _cols.Value; i++)
                    {
                        g.DrawLine(pen, r.X + (i * cw), r.Y, r.X + (i * cw), r.Bottom);
                    }
                    for (int i = 1; i < _rows.Value; i++)
                    {
                        g.DrawLine(pen, r.X, r.Y + (i * ch), r.Right, r.Y + (i * ch));
                    }
                };
                _cols.ValueChanged += (s, e) => _view.Invalidate();
                _rows.ValueChanged += (s, e) => _view.Invalidate();
                _mapping.SelectedIndexChanged += (s, e) =>
                {
                    if (_mapping.SelectedIndex is 2 or 3)
                    {
                        _rows.Value = 8;
                    }
                    else if (_mapping.SelectedIndex is 0 or 1)
                    {
                        _rows.Value = 5;
                    }
                };

                Controls.Add(_view);
                Controls.Add(top);
            }

            public int Columns => (int)_cols.Value;
            public int Rows => (int)_rows.Value;
            public int Mapping => _mapping.SelectedIndex;
            public bool CropUnion => _crop.Checked;
            public bool RemoveBackground => _removeBg.Checked;
        }
    }
}
