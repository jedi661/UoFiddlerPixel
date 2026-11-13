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
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Animations-Studio: baut neue Mobile-Animationen aus Einzelbildern oder Sprite-Sheets.
    /// UO speichert 5 Richtungen (0 = unten, 1 = unten-links, 2 = links, 3 = oben-links, 4 = oben),
    /// die 3 rechten Richtungen erzeugt der Client durch Spiegeln. Die Vorschau zeigt alle 8 Richtungen.
    /// </summary>
    public partial class AnimationStudioForm : Form
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

        // compass cell -> (anim direction, mirrored); index 4 = big preview of the selected direction
        private static readonly (int Dir, bool Mirror)[] _cells =
        {
            (3, false), (4, false), (3, true),
            (2, false), (-1, false), (2, true),
            (1, false), (0, false), (1, true)
        };

        private readonly List<Bitmap>[] _dirs = Enumerable.Range(0, DirCount).Select(_ => new List<Bitmap>()).ToArray();
        private readonly PixelView[] _compass;
        private int _frameIndex;

        public AnimationStudioForm()
        {
            InitializeComponent();

            _compass = new[]
            {
                viewUpLeft, viewUp, viewUpRight,
                viewLeft, viewSelected, viewRight,
                viewDownLeft, viewDown, viewDownRight
            };
            viewSelected.PaintOverlay += DrawCrosshair;
            lvFrames.LargeImageList = imageListThumbs;
            cbFileType.SelectedIndex = 0;

            RefreshDirList();
            lbDirections.SelectedIndex = 0;
            timerPlay.Start();
        }

        private int CurrentDir => Math.Max(0, lbDirections.SelectedIndex);

        #region Button events

        private void BtnAddImages_Click(object sender, EventArgs e) => AddImages();

        private void BtnImportSheet_Click(object sender, EventArgs e) => ImportSheet();

        private void BtnLoadMul_Click(object sender, EventArgs e) => LoadFromMul();

        private void BtnMirror_Click(object sender, EventArgs e) => MirrorDirection();

        private void BtnFillEmpty_Click(object sender, EventArgs e) => FillEmptyDirections();

        private void BtnFrameLeft_Click(object sender, EventArgs e) => MoveFrame(-1);

        private void BtnFrameRight_Click(object sender, EventArgs e) => MoveFrame(1);

        private void BtnRemoveFrames_Click(object sender, EventArgs e) => RemoveFrames();

        private void BtnClearDirection_Click(object sender, EventArgs e) => ClearDirection();

        private void BtnExportMul_Click(object sender, EventArgs e) => ExportToMul();

        private void BtnSaveMul_Click(object sender, EventArgs e) => SaveMul();

        private void BtnExportPng_Click(object sender, EventArgs e) => ExportPngFrames();

        private void BtnExportSheet_Click(object sender, EventArgs e) => ExportSheet();

        private void LbDirections_SelectedIndexChanged(object sender, EventArgs e) => RefreshFrameList();

        private void TimerPlay_Tick(object sender, EventArgs e)
        {
            _frameIndex++;
            RefreshPreview();
        }

        private void NumDelay_ValueChanged(object sender, EventArgs e)
        {
            timerPlay.Interval = (int)numDelay.Value;
        }

        private void NumZoom_ValueChanged(object sender, EventArgs e)
        {
            foreach (PixelView v in _compass)
            {
                v.Zoom = (int)numZoom.Value;
            }
            viewSelected.Zoom = (int)numZoom.Value * 2;
        }

        private void ChkPlay_CheckedChanged(object sender, EventArgs e)
        {
            timerPlay.Enabled = chkPlay.Checked;
        }

        private void PreviewSetting_Changed(object sender, EventArgs e)
        {
            RefreshPreview();
        }

        private void CompassView_Click(object sender, EventArgs e)
        {
            int cell = Array.IndexOf(_compass, sender);
            if (cell >= 0 && _cells[cell].Dir >= 0)
            {
                lbDirections.SelectedIndex = _cells[cell].Dir;
            }
        }

        #endregion

        #region Lists

        private void RefreshDirList()
        {
            int sel = lbDirections.SelectedIndex;
            lbDirections.BeginUpdate();
            lbDirections.Items.Clear();
            for (int i = 0; i < DirCount; i++)
            {
                lbDirections.Items.Add($"{_dirNames[i]}   [{_dirs[i].Count} Frames]");
            }
            lbDirections.EndUpdate();
            lbDirections.SelectedIndex = sel < 0 ? 0 : sel;
        }

        private void RefreshFrameList()
        {
            lvFrames.BeginUpdate();
            lvFrames.Items.Clear();
            foreach (Image img in imageListThumbs.Images)
            {
                img.Dispose();
            }
            imageListThumbs.Images.Clear();

            List<Bitmap> frames = _dirs[CurrentDir];
            for (int i = 0; i < frames.Count; i++)
            {
                imageListThumbs.Images.Add(MakeThumb(frames[i]));
                lvFrames.Items.Add(new ListViewItem($"{i}  ({frames[i].Width}x{frames[i].Height})", i) { Tag = i });
            }
            lvFrames.EndUpdate();
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
            int sel = lbDirections.SelectedIndex;
            RefreshDirList();
            if (sel == lbDirections.SelectedIndex)
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
            if (!chkCrosshair.Checked || viewSelected.Image == null)
            {
                return;
            }
            int zoom = viewSelected.Zoom;
            int w = viewSelected.Image.Width;
            int h = viewSelected.Image.Height;
            // UO: frame is drawn at (x - centerX, y - centerY - height) -> foot point
            int fx = r.X + (((w / 2) + (int)numCenterX.Value) * zoom);
            int fy = r.Y + ((h + (int)numCenterY.Value) * zoom);
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
                lblStatus.Text = $"{dlg.FileNames.Length} Datei(en) zu Richtung {CurrentDir} hinzugefügt.";
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
            using (var dlg = new AnimationSheetImportForm(sheet))
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
            lblStatus.Text = "Sprite-Sheet importiert.";
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
                int fileType = cbFileType.SelectedIndex + 1;
                int body = (int)numBody.Value, action = (int)numAction.Value;
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
                lblStatus.Text = $"Body {body}, Action {action} aus {cbFileType.Text} geladen ({loaded} Frames).";
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
            lblStatus.Text = filled == 0 ? "Keine leeren Richtungen." : $"{filled} leere Richtung(en) aufgefüllt (Kopie der Nachbarrichtung).";
        }

        private void MoveFrame(int delta)
        {
            if (lvFrames.SelectedItems.Count != 1)
            {
                return;
            }
            List<Bitmap> frames = _dirs[CurrentDir];
            int i = (int)lvFrames.SelectedItems[0].Tag;
            int j = i + delta;
            if (j < 0 || j >= frames.Count)
            {
                return;
            }
            (frames[i], frames[j]) = (frames[j], frames[i]);
            RefreshFrameList();
            lvFrames.Items[j].Selected = true;
        }

        private void RemoveFrames()
        {
            List<Bitmap> frames = _dirs[CurrentDir];
            foreach (int i in lvFrames.SelectedItems.Cast<ListViewItem>().Select(it => (int)it.Tag).OrderByDescending(i => i))
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

            int fileType = cbFileType.SelectedIndex + 1;
            int body = (int)numBody.Value, action = (int)numAction.Value;

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
                        StudioUi.Error(this, $"Body {body} / Action {action} / Richtung {d} existiert in {cbFileType.Text} nicht (Index außerhalb der Datei).");
                        return;
                    }

                    List<Bitmap> frames = _dirs[d];
                    edit.ClearFrames();
                    if (frames.Count == 0)
                    {
                        continue;
                    }

                    // palette with max. 256 colours shared by all frames of this direction
                    List<Bitmap> uoFrames = BuildUoFrames(frames, (int)numColors.Value, out ushort[] palette);
                    edit.ReplacePalette(palette);
                    foreach (Bitmap f in uoFrames)
                    {
                        int cx = (f.Width / 2) + (int)numCenterX.Value;
                        int cy = (int)numCenterY.Value;
                        edit.AddFrame(f, cx, cy);
                        f.Dispose();
                    }
                }
                StudioUi.MarkChanged("Animations");
                lblStatus.Text = $"Body {body}, Action {action} in {cbFileType.Text} geschrieben. Mit \"Anim-MUL speichern\" in {StudioUi.OutputPath} sichern.";
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
            int fileType = cbFileType.SelectedIndex + 1;
            try
            {
                Cursor = Cursors.WaitCursor;
                AnimationEdit.Save(fileType, StudioUi.OutputPath);
                StudioUi.ResetChanged("Animations");
                StudioUi.Info(this, $"{cbFileType.Text} gespeichert in:\n{StudioUi.OutputPath}");
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
            lblStatus.Text = $"{n} Frames gespeichert in {dlg.SelectedPath}";
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
                lblStatus.Text = $"Sprite-Sheet ({cols} Spalten x 5 Reihen, UO-Reihenfolge) gespeichert.";
            }
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerPlay.Stop();
            foreach (Bitmap b in _dirs.SelectMany(d => d))
            {
                b.Dispose();
            }
            foreach (PixelView v in _compass)
            {
                v.Image?.Dispose();
            }
            base.OnFormClosed(e);
        }
    }
}
