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
    /// Animations-Studio: baut neue Mobile-Animationen aus Einzelbildern oder Sprite-Sheets.
    /// UO speichert 5 Richtungen (0 = unten, 1 = unten-links, 2 = links, 3 = oben-links, 4 = oben),
    /// die 3 rechten Richtungen erzeugt der Client durch Spiegeln. Die Vorschau zeigt alle 8 Richtungen,
    /// am Fusspunkt ausgerichtet wie im Client, optional mit Zwiebelschicht, Hue und Ausruestungs-Overlay.
    /// </summary>
    public partial class AnimationStudioForm : Form
    {
        private const int DirCount = 5;
        private const int Pad = 8;

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

        /// <summary>One frame: image plus the individual foot point offset.</summary>
        private sealed class AnimFrame
        {
            public AnimFrame(Bitmap image, Point offset)
            {
                Image = image;
                Offset = offset;
            }

            public Bitmap Image { get; set; }

            /// <summary>Foot point offset relative to the default foot point (bottom centre + global X/Y).</summary>
            public Point Offset { get; set; }

            public AnimFrame Clone(bool mirror) =>
                new AnimFrame(mirror ? StudioImaging.FlipHorizontal(Image) : new Bitmap(Image), mirror ? new Point(-Offset.X, Offset.Y) : Offset);
        }

        /// <summary>Frame of the overlay animation (taken from a MUL, with its own centre).</summary>
        private sealed class OverlayFrame
        {
            public Bitmap Image;
            public Point Center;
        }

        private readonly List<AnimFrame>[] _dirs = Enumerable.Range(0, DirCount).Select(_ => new List<AnimFrame>()).ToArray();
        private readonly List<OverlayFrame>[] _overlay = Enumerable.Range(0, DirCount).Select(_ => new List<OverlayFrame>()).ToArray();
        private readonly PixelView[] _compass;
        private int _frameIndex;
        private Point _selectedFoot;          // foot point of the selected view in canvas coordinates
        private Rectangle _selectedFrameRect; // where the current frame was drawn in the selected view
        private bool _updatingOffset;

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
            cbOverlayFile.SelectedIndex = 0;
            cbMobType.SelectedIndex = 3;

            RefreshDirList();
            lbDirections.SelectedIndex = 0;
            timerPlay.Start();
        }

        /// <summary>Pre-selects file type, body and action (used by the equipment generator).</summary>
        public void SetTarget(int fileType, int body, int action, int mobType)
        {
            cbFileType.SelectedIndex = Math.Max(0, Math.Min(4, fileType - 1));
            numBody.Value = Math.Max(numBody.Minimum, Math.Min(numBody.Maximum, body));
            numAction.Value = Math.Max(numAction.Minimum, Math.Min(numAction.Maximum, action));
            cbMobType.SelectedIndex = Math.Max(0, Math.Min(cbMobType.Items.Count - 1, mobType));
        }

        private int CurrentDir => Math.Max(0, lbDirections.SelectedIndex);

        #region Events

        private void BtnAddImages_Click(object sender, EventArgs e) => AddImages();

        private void BtnImportSheet_Click(object sender, EventArgs e) => ImportSheet();

        private void BtnLoadMul_Click(object sender, EventArgs e) => LoadFromMul();

        private void BtnMirror_Click(object sender, EventArgs e) => MirrorDirection();

        private void BtnFillEmpty_Click(object sender, EventArgs e) => FillEmptyDirections();

        private void BtnFrameLeft_Click(object sender, EventArgs e) => MoveFrame(-1);

        private void BtnFrameRight_Click(object sender, EventArgs e) => MoveFrame(1);

        private void BtnRemoveFrames_Click(object sender, EventArgs e) => RemoveFrames();

        private void BtnClearDirection_Click(object sender, EventArgs e) => ClearDirection();

        private void BtnExportMul_Click(object sender, EventArgs e) => ExportToMul(true);

        private void BtnSaveMul_Click(object sender, EventArgs e) => SaveMul();

        private void BtnExportPng_Click(object sender, EventArgs e) => ExportPngFrames();

        private void BtnExportSheet_Click(object sender, EventArgs e) => ExportSheet();

        private void BtnExportVd_Click(object sender, EventArgs e) => ExportVd();

        private void BtnDefEntries_Click(object sender, EventArgs e) => ShowDefEntries();

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

        private void Overlay_Changed(object sender, EventArgs e)
        {
            LoadOverlay();
            RefreshPreview();
        }

        private void ChkSetFoot_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSetFoot.Checked)
            {
                chkPlay.Checked = false;
                viewSelected.Cursor = Cursors.Cross;
                lblStatus.Text = "Frame in der Liste wählen und in der großen Ansicht auf die Füße der Figur klicken.";
            }
            else
            {
                viewSelected.Cursor = Cursors.Default;
            }
        }

        private void CompassView_Click(object sender, EventArgs e)
        {
            int cell = Array.IndexOf(_compass, sender);
            if (cell >= 0 && _cells[cell].Dir >= 0)
            {
                lbDirections.SelectedIndex = _cells[cell].Dir;
            }
        }

        private void LvFrames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvFrames.SelectedIndices.Count != 1)
            {
                return;
            }
            int i = lvFrames.SelectedIndices[0];
            if (!chkPlay.Checked)
            {
                _frameIndex = i;
                RefreshPreview();
            }
            AnimFrame f = _dirs[CurrentDir][i];
            _updatingOffset = true;
            numFrameOffX.Value = Clamp(numFrameOffX, f.Offset.X);
            numFrameOffY.Value = Clamp(numFrameOffY, f.Offset.Y);
            _updatingOffset = false;
        }

        private void FrameOffset_Changed(object sender, EventArgs e)
        {
            if (_updatingOffset || lvFrames.SelectedIndices.Count == 0)
            {
                return;
            }
            foreach (int i in lvFrames.SelectedIndices)
            {
                _dirs[CurrentDir][i].Offset = new Point((int)numFrameOffX.Value, (int)numFrameOffY.Value);
            }
            RefreshPreview();
        }

        private void BtnFootAll_Click(object sender, EventArgs e)
        {
            var off = new Point((int)numFrameOffX.Value, (int)numFrameOffY.Value);
            foreach (AnimFrame f in _dirs[CurrentDir])
            {
                f.Offset = off;
            }
            RefreshPreview();
            lblStatus.Text = $"Frame-Versatz {off.X}/{off.Y} auf alle Frames der Richtung {CurrentDir} übernommen.";
        }

        /// <summary>Click in the big view = new foot point of the current frame.</summary>
        private void ViewSelected_MouseClick(object sender, MouseEventArgs e)
        {
            List<AnimFrame> frames = _dirs[CurrentDir];
            if (!chkSetFoot.Checked || frames.Count == 0 || e.Button != MouseButtons.Left)
            {
                return;
            }
            Rectangle r = viewSelected.ImageRectangle;
            if (r.IsEmpty)
            {
                return;
            }
            int zoom = viewSelected.Zoom;
            int cx = (e.X - r.X) / zoom, cy = (e.Y - r.Y) / zoom;
            int index = _frameIndex % frames.Count;
            AnimFrame f = frames[index];

            // new foot point inside the frame image
            int fx = cx - _selectedFrameRect.X;
            int fy = cy - _selectedFrameRect.Y;
            f.Offset = new Point(fx - DefaultFootX(f.Image), fy - DefaultFootY(f.Image));

            _updatingOffset = true;
            numFrameOffX.Value = Clamp(numFrameOffX, f.Offset.X);
            numFrameOffY.Value = Clamp(numFrameOffY, f.Offset.Y);
            _updatingOffset = false;
            RefreshPreview();
            lblStatus.Text = $"Fußpunkt von Frame {index} (Richtung {CurrentDir}) gesetzt: Versatz {f.Offset.X}/{f.Offset.Y}.";
        }

        private static decimal Clamp(NumericUpDown n, int v) => Math.Max(n.Minimum, Math.Min(n.Maximum, v));

        #endregion

        #region Foot point helpers

        private int DefaultFootX(Bitmap img) => (img.Width / 2) + (int)numCenterX.Value;

        private int DefaultFootY(Bitmap img) => img.Height + (int)numCenterY.Value;

        /// <summary>Foot point of a frame inside its image.</summary>
        private Point Foot(AnimFrame f) => new Point(DefaultFootX(f.Image) + f.Offset.X, DefaultFootY(f.Image) + f.Offset.Y);

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
            imageListThumbs.Images.Clear();

            List<AnimFrame> frames = _dirs[CurrentDir];
            for (int i = 0; i < frames.Count; i++)
            {
                using Bitmap thumb = MakeThumb(frames[i].Image);
                imageListThumbs.Images.Add(thumb);
                lvFrames.Items.Add(new ListViewItem($"{i}  ({frames[i].Image.Width}x{frames[i].Image.Height})", i) { Tag = i });
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
            int w = Math.Max(1, (int)(src.Width * f)), h = Math.Max(1, (int)(src.Height * f));
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
            Hue hue = StudioUi.GetHue((int)numHue.Value);
            for (int i = 0; i < 9; i++)
            {
                int dir = _cells[i].Dir < 0 ? CurrentDir : _cells[i].Dir;
                bool selected = i == 4;
                Bitmap scene = RenderDirection(dir, _cells[i].Mirror, selected && chkOnion.Checked, hue, selected);
                PixelView v = _compass[i];
                Image old = v.Image;
                v.Image = scene;
                old?.Dispose();
                if (selected)
                {
                    int count = _dirs[dir].Count;
                    v.Caption = $"Auswahl: {_dirNames[dir]}  Frame {(count == 0 ? 0 : (_frameIndex % count) + 1)}/{count}";
                }
            }
        }

        /// <summary>
        /// Draws the current frame of a direction on a canvas that is the same for all frames,
        /// aligned at the foot point (like the client). Mirrored directions are flipped as a whole.
        /// </summary>
        private Bitmap RenderDirection(int dir, bool mirror, bool onion, Hue hue, bool remember)
        {
            List<AnimFrame> frames = _dirs[dir];
            List<OverlayFrame> overlay = chkOverlay.Checked ? _overlay[dir] : null;
            if (frames.Count == 0 && (overlay == null || overlay.Count == 0))
            {
                return null;
            }

            // canvas bounds relative to the foot point
            int minX = 0, minY = 0, maxX = 1, maxY = 1;
            foreach (AnimFrame f in frames)
            {
                Point ft = Foot(f);
                minX = Math.Min(minX, -ft.X);
                minY = Math.Min(minY, -ft.Y);
                maxX = Math.Max(maxX, f.Image.Width - ft.X);
                maxY = Math.Max(maxY, f.Image.Height - ft.Y);
            }
            if (overlay != null)
            {
                foreach (OverlayFrame o in overlay)
                {
                    // UO: image top-left = foot - (centerX, centerY + height)
                    minX = Math.Min(minX, -o.Center.X);
                    minY = Math.Min(minY, -(o.Center.Y + o.Image.Height));
                    maxX = Math.Max(maxX, o.Image.Width - o.Center.X);
                    maxY = Math.Max(maxY, -o.Center.Y);
                }
            }

            int w = maxX - minX + (Pad * 2), h = maxY - minY + (Pad * 2);
            var foot = new Point(Pad - minX, Pad - minY);
            var scene = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(scene))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;

                if (frames.Count > 0)
                {
                    int index = _frameIndex % frames.Count;
                    if (onion && frames.Count > 1)
                    {
                        AnimFrame prev = frames[(index + frames.Count - 1) % frames.Count];
                        Point pf = Foot(prev);
                        using var attr = new ImageAttributes();
                        attr.SetColorMatrix(new ColorMatrix { Matrix33 = 0.35f });
                        g.DrawImage(prev.Image, new Rectangle(foot.X - pf.X, foot.Y - pf.Y, prev.Image.Width, prev.Image.Height),
                                    0, 0, prev.Image.Width, prev.Image.Height, GraphicsUnit.Pixel, attr);
                    }

                    AnimFrame cur = frames[index];
                    Point cf = Foot(cur);
                    var rect = new Rectangle(foot.X - cf.X, foot.Y - cf.Y, cur.Image.Width, cur.Image.Height);
                    if (hue != null)
                    {
                        using Bitmap hued = StudioImaging.ApplyHue(cur.Image, hue, chkPartialHue.Checked);
                        g.DrawImageUnscaled(hued, rect.X, rect.Y);
                    }
                    else
                    {
                        g.DrawImageUnscaled(cur.Image, rect.X, rect.Y);
                    }
                    if (remember)
                    {
                        _selectedFrameRect = rect;
                    }
                }

                if (overlay != null && overlay.Count > 0)
                {
                    OverlayFrame o = overlay[_frameIndex % overlay.Count];
                    g.DrawImageUnscaled(o.Image, foot.X - o.Center.X, foot.Y - o.Center.Y - o.Image.Height);
                }
            }

            if (remember)
            {
                _selectedFoot = foot;
            }

            if (mirror)
            {
                scene.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            return scene;
        }

        private void DrawCrosshair(Graphics g, Rectangle r)
        {
            if (!chkCrosshair.Checked || viewSelected.Image == null)
            {
                return;
            }
            int zoom = viewSelected.Zoom;
            int fx = r.X + (_selectedFoot.X * zoom);
            int fy = r.Y + (_selectedFoot.Y * zoom);
            using var pen = new Pen(Color.Red, 1);
            g.DrawLine(pen, fx - 12, fy, fx + 12, fy);
            g.DrawLine(pen, fx, fy - 12, fx, fy + 12);
            using var dia = new Pen(Color.FromArgb(140, Color.Blue));
            int t = 22 * zoom;
            g.DrawPolygon(dia, new[] { new Point(fx, fy - t), new Point(fx + t, fy), new Point(fx, fy + t), new Point(fx - t, fy) });
        }

        /// <summary>Loads the overlay animation (e.g. equipment) for the same action.</summary>
        private void LoadOverlay()
        {
            foreach (List<OverlayFrame> list in _overlay)
            {
                foreach (OverlayFrame o in list)
                {
                    o.Image.Dispose();
                }
                list.Clear();
            }
            if (!chkOverlay.Checked)
            {
                return;
            }
            try
            {
                int fileType = cbOverlayFile.SelectedIndex + 1;
                int body = (int)numOverlayBody.Value, action = (int)numAction.Value;
                for (int d = 0; d < DirCount; d++)
                {
                    AnimIdx idx = AnimationEdit.GetAnimation(fileType, body, action, d);
                    Bitmap[] bits = idx?.GetFrames();
                    if (bits == null || idx.Frames == null)
                    {
                        continue;
                    }
                    for (int i = 0; i < bits.Length && i < idx.Frames.Count; i++)
                    {
                        if (bits[i] == null)
                        {
                            continue;
                        }
                        _overlay[d].Add(new OverlayFrame { Image = StudioImaging.To32(bits[i]), Center = idx.Frames[i].Center });
                        bits[i].Dispose();
                    }
                }
                int total = _overlay.Sum(l => l.Count);
                lblStatus.Text = total == 0
                    ? $"Overlay: Body {body} / Action {action} hat in {cbOverlayFile.Text} keine Frames."
                    : $"Overlay: Body {body} / Action {action} geladen ({total} Frames).";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Overlay konnte nicht geladen werden: " + ex.Message;
            }
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
                foreach (string file in dlg.FileNames.OrderBy(f => f, new NaturalStringComparer()))
                {
                    foreach (Bitmap b in StudioImaging.LoadFrames(file))
                    {
                        _dirs[CurrentDir].Add(new AnimFrame(b, Point.Empty));
                    }
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
                    case 3: // 8 rows clockwise from down
                        if (rows.Count >= 5)
                        {
                            for (int d = 0; d < DirCount; d++)
                            {
                                Replace(d, rows[d], false);
                            }
                        }
                        break;
                    default: // all cells into the current direction
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

                foreach (Bitmap b in rows.SelectMany(r => r))
                {
                    b.Dispose();
                }
            }

            Changed();
            lblStatus.Text = "Sprite-Sheet importiert.";
        }

        /// <summary>Replaces the frames of a direction with copies of the given bitmaps.</summary>
        private void Replace(int dir, List<Bitmap> frames, bool mirror)
        {
            ClearFrames(_dirs[dir]);
            foreach (Bitmap f in frames)
            {
                _dirs[dir].Add(new AnimFrame(mirror ? StudioImaging.FlipHorizontal(f) : new Bitmap(f), Point.Empty));
            }
        }

        private static void ClearFrames(List<AnimFrame> list)
        {
            foreach (AnimFrame f in list)
            {
                f.Image.Dispose();
            }
            list.Clear();
        }

        /// <summary>Crops all frames of one direction to the common opaque bounding box (keeps alignment).</summary>
        private static void CropToUnion(List<AnimFrame> frames)
        {
            if (frames.Count == 0)
            {
                return;
            }
            int w = frames.Max(f => f.Image.Width), h = frames.Max(f => f.Image.Height);
            int minX = w, minY = h, maxX = -1, maxY = -1;
            foreach (AnimFrame af in frames)
            {
                Bitmap f = af.Image;
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
            foreach (AnimFrame af in frames)
            {
                var c = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(c))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImageUnscaled(af.Image, -rect.X, -rect.Y);
                }
                af.Image.Dispose();
                af.Image = c;
            }
        }

        /// <summary>Loads all 5 directions of body/action from the MUL, including the frame centres.</summary>
        private void LoadFromMul()
        {
            try
            {
                int fileType = cbFileType.SelectedIndex + 1;
                int body = (int)numBody.Value, action = (int)numAction.Value;
                int loaded = 0;
                numCenterX.Value = 0;
                numCenterY.Value = 0;
                for (int d = 0; d < DirCount; d++)
                {
                    ClearFrames(_dirs[d]);
                    AnimIdx edit = AnimationEdit.GetAnimation(fileType, body, action, d);
                    Bitmap[] bits = edit?.GetFrames();
                    if (bits == null || edit.Frames == null)
                    {
                        continue;
                    }
                    for (int i = 0; i < bits.Length && i < edit.Frames.Count; i++)
                    {
                        if (bits[i] == null)
                        {
                            continue;
                        }
                        Bitmap img = StudioImaging.To32(bits[i]);
                        bits[i].Dispose();
                        Point c = edit.Frames[i].Center;
                        // UO centre -> offset relative to the default foot point (bottom centre)
                        _dirs[d].Add(new AnimFrame(img, new Point(c.X - (img.Width / 2), c.Y)));
                        loaded++;
                    }
                }
                Changed();
                lblStatus.Text = $"Body {body}, Action {action} aus {cbFileType.Text} geladen ({loaded} Frames, Fußpunkte übernommen).";
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
            List<AnimFrame> frames = _dirs[CurrentDir];
            for (int i = 0; i < frames.Count; i++)
            {
                AnimFrame m = frames[i].Clone(true);
                frames[i].Image.Dispose();
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
                        _dirs[d].AddRange(_dirs[src].Select(f => f.Clone(false)));
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
            List<AnimFrame> frames = _dirs[CurrentDir];
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
            List<AnimFrame> frames = _dirs[CurrentDir];
            foreach (int i in lvFrames.SelectedItems.Cast<ListViewItem>().Select(it => (int)it.Tag).OrderByDescending(i => i))
            {
                frames[i].Image.Dispose();
                frames.RemoveAt(i);
            }
            Changed();
        }

        private void ClearDirection()
        {
            ClearFrames(_dirs[CurrentDir]);
            Changed();
        }

        #endregion

        #region Export

        private bool ExportToMul(bool interactive)
        {
            if (_dirs.All(d => d.Count == 0))
            {
                StudioUi.Info(this, "Keine Frames vorhanden.");
                return false;
            }
            if (interactive && _dirs.Any(d => d.Count == 0) &&
                !StudioUi.Ask(this, "Nicht alle 5 Richtungen haben Frames. Leere Richtungen werden in der MUL geleert.\nTrotzdem fortfahren? (Tipp: \"Leere auffüllen\")"))
            {
                return false;
            }

            int fileType = cbFileType.SelectedIndex + 1;
            int body = (int)numBody.Value, action = (int)numAction.Value;

            if (fileType == 1)
            {
                int maxActions = body < 200 ? 22 : body < 400 ? 13 : 35;
                if (action >= maxActions)
                {
                    StudioUi.Error(this, $"Body {body} hat in anim.mul nur {maxActions} Actions (0–{maxActions - 1}).");
                    return false;
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
                        return false;
                    }

                    List<AnimFrame> frames = _dirs[d];
                    edit.ClearFrames();
                    if (frames.Count == 0)
                    {
                        continue;
                    }

                    // palette with max. 256 colours shared by all frames of this direction
                    List<Bitmap> uoFrames = BuildUoFrames(frames.Select(f => f.Image).ToList(), (int)numColors.Value, out ushort[] palette);
                    edit.ReplacePalette(palette);
                    for (int i = 0; i < uoFrames.Count; i++)
                    {
                        Bitmap f = uoFrames[i];
                        Point foot = Foot(frames[i]);
                        // UO: frame drawn at (footX - centerX, footY - centerY - height)
                        int cx = foot.X;
                        int cy = foot.Y - f.Height;
                        edit.AddFrame(f, cx, cy);
                        f.Dispose();
                    }
                }
                StudioUi.MarkChanged("Animations");
                lblStatus.Text = $"Body {body}, Action {action} in {cbFileType.Text} geschrieben. Mit \"Anim-MUL speichern\" in {StudioUi.OutputPath} sichern.";
                return true;
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
                return false;
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

        /// <summary>Exports the whole body (all actions) as VD file – importable in UO Fiddler and the VD tools.</summary>
        private void ExportVd()
        {
            int fileType = cbFileType.SelectedIndex + 1;
            int body = (int)numBody.Value;
            if (_dirs.Any(d => d.Count > 0) &&
                StudioUi.Ask(this, "Die aktuellen Frames zuerst in die Animation schreiben?\n(Sonst wird nur exportiert, was bereits in der MUL steht.)") &&
                !ExportToMul(false))
            {
                return;
            }
            using var dlg = new SaveFileDialog { Filter = "VD-Datei|*.vd", FileName = $"body_{body}.vd", InitialDirectory = StudioUi.OutputPath };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            try
            {
                Cursor = Cursors.WaitCursor;
                AnimationEdit.ExportToVD(fileType, body, dlg.FileName);
                lblStatus.Text = "VD exportiert: " + dlg.FileName;
                StudioUi.Info(this, $"Body {body} als VD exportiert:\n{dlg.FileName}\n\nDie VD-Datei kann im Animation-Editor von UO Fiddler oder mit \"Animation VD\" / \"VD Editor (UOEC)\" weiterverarbeitet werden.");
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

        /// <summary>Text for mobtypes.txt, bodyconv.def and Body.def.</summary>
        private void ShowDefEntries()
        {
            int fileType = cbFileType.SelectedIndex + 1;
            int body = (int)numBody.Value;
            string type = cbMobType.Text;
            var sb = new StringBuilder();
            sb.AppendLine("# ---- mobtypes.txt -------------------------------------------");
            sb.AppendLine("# BODY   TYPE         FLAGS");
            sb.AppendLine($"{body}\t{type}\t0");
            sb.AppendLine();
            if (fileType > 1)
            {
                sb.AppendLine("# ---- bodyconv.def ---------------------------------------------");
                sb.AppendLine("# Spiel-Body  anim2  anim3  anim4  anim5  (-1 = nicht verwendet)");
                string[] cols = { "-1", "-1", "-1", "-1" };
                cols[fileType - 2] = body.ToString();
                sb.AppendLine($"{body}\t{string.Join("\t", cols)}");
                sb.AppendLine("# Hinweis: Spiel-Body links anpassen, wenn er sich von der Position in der Datei unterscheidet.");
                sb.AppendLine();
            }
            sb.AppendLine("# ---- Body.def (optional) ---------------------------------------");
            sb.AppendLine("# Nur nötig, um einen NEUEN Body auf diese Animation umzuleiten:");
            sb.AppendLine("# NEUER_BODY {BASIS_BODY} HUE");
            sb.AppendLine($"# 2000 {{{body}}} 0");
            sb.AppendLine();
            sb.AppendLine("# ---- ServUO / RunUO ---------------------------------------------");
            sb.AppendLine($"// Body = {body};   // {type}");
            StudioTextForm.ShowText(this, $"Def-Einträge für Body {body}", sb.ToString(), "mobtypes.txt");
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
                    _dirs[d][i].Image.Save(Path.Combine(dlg.SelectedPath, $"anim_dir{d}_frame{i:D2}.png"), ImageFormat.Png);
                    n++;
                }
            }
            lblStatus.Text = $"{n} Frames gespeichert in {dlg.SelectedPath}";
        }

        /// <summary>Sprite sheet (5 rows, UO order), every cell aligned at the foot point.</summary>
        private void ExportSheet()
        {
            int cols = _dirs.Max(d => d.Count);
            if (cols == 0)
            {
                return;
            }
            var all = _dirs.SelectMany(d => d).ToList();
            int left = all.Max(f => Foot(f).X), right = all.Max(f => f.Image.Width - Foot(f).X);
            int top = all.Max(f => Foot(f).Y), bottom = Math.Max(0, all.Max(f => f.Image.Height - Foot(f).Y));
            int cw = left + right, ch = top + bottom;
            using var sheet = new Bitmap(cw * cols, ch * DirCount, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(sheet))
            {
                g.Clear(Color.Transparent);
                for (int d = 0; d < DirCount; d++)
                {
                    for (int i = 0; i < _dirs[d].Count; i++)
                    {
                        AnimFrame f = _dirs[d][i];
                        Point ft = Foot(f);
                        g.DrawImageUnscaled(f.Image, (i * cw) + left - ft.X, (d * ch) + top - ft.Y);
                    }
                }
            }
            using var sfd = new SaveFileDialog { Filter = "PNG|*.png", FileName = "anim_sheet.png", InitialDirectory = StudioUi.OutputPath };
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                sheet.Save(sfd.FileName, ImageFormat.Png);
                lblStatus.Text = $"Sprite-Sheet ({cols} Spalten x 5 Reihen, UO-Reihenfolge, am Fußpunkt ausgerichtet) gespeichert.";
            }
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerPlay.Stop();
            foreach (List<AnimFrame> list in _dirs)
            {
                ClearFrames(list);
            }
            foreach (OverlayFrame o in _overlay.SelectMany(l => l))
            {
                o.Image.Dispose();
            }
            foreach (PixelView v in _compass)
            {
                Image img = v.Image;
                v.Image = null;
                img?.Dispose();
            }
            base.OnFormClosed(e);
        }
    }
}
