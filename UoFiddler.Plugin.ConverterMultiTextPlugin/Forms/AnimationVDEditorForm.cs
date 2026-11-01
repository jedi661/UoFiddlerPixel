// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

// VD editor based on the VD part of the UOEC Super Viewer (btnVDFile_Click / BeginLoadVD
// and its preview): open, view, edit, mirror, swap directions, copy/paste frames, import, export, save.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class AnimationVDEditorForm : Form
    {
        // ════════════════════════════════════════════════════════════════
        //  State
        // ════════════════════════════════════════════════════════════════

        private VdAnimationFile _vd;
        private string _filePath;
        private bool _dirty;

        private int _action = -1;
        private int _frameIndex;

        private float _zoom = 2f;
        private Point _pan = Point.Empty;
        private Color _backColor = Color.FromArgb(60, 60, 60);

        private readonly Timer _playTimer = new Timer();
        private int _delay = 100;

        private bool _updatingUi;

        // mouse handling on the preview
        private Point _mouseStart;
        private bool _draggingFrame;
        private bool _panning;
        private bool _dragUndoPushed;
        private float _dragRestX;
        private float _dragRestY;

        // "Zwischenspeicher" – static, so it stays filled when the form is reopened
        private static List<VdFrame> _frameBuffer = new List<VdFrame>();
        private static VdEntry _directionBuffer;
        private static VdEntry[] _actionBuffer;
        private static string _clipboardToken;
        private const string ClipboardFormat = "UoFiddlerVdFrames";

        // undo
        private sealed class UndoStep
        {
            public string Name;
            public List<(int Action, int Dir, VdEntry Entry)> Entries = new List<(int, int, VdEntry)>();
        }

        private readonly LinkedList<UndoStep> _undo = new LinkedList<UndoStep>();
        private const int MaxUndo = 30;

        private const string OriginFileName = "vd_origin.txt";
        private const string ImageFilter = "Bilder (*.png;*.bmp;*.gif;*.jpg;*.tif)|*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|Alle Dateien (*.*)|*.*";

        public AnimationVDEditorForm()
        {
            InitializeComponent();

            // double buffering for a flicker free preview
            typeof(PictureBox).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(pbPreview, true);

            pbPreview.MouseWheel += PbPreview_MouseWheel;

            _playTimer.Interval = _delay;
            _playTimer.Tick += PlayTimer_Tick;

            cmbImportCenter.SelectedIndex = 0;
            cmbTransparent.SelectedIndex = 0;
            cmbTargetDir.SelectedIndex = 0;
            nudZoom.Value = (decimal)_zoom;
            btnBackColor.BackColor = _backColor;

            UpdateSpeedLabel();
            UpdateUi();
        }

        // ════════════════════════════════════════════════════════════════
        //  Helpers: current selection
        // ════════════════════════════════════════════════════════════════

        /// <summary>View direction 0-7. 5-7 are shown mirrored from 3-1 like the client does.</summary>
        private int ViewDirection => trackDirection.Value;

        private int StoredDirection => ViewDirection <= 4 ? ViewDirection : 8 - ViewDirection;

        private bool IsMirroredView => ViewDirection > 4;

        private VdEntry CurrentEntry => _vd?.GetEntry(_action, StoredDirection);

        private VdFrame CurrentFrame
        {
            get
            {
                VdEntry e = CurrentEntry;
                if (e == null || _frameIndex < 0 || _frameIndex >= e.Frames.Count)
                {
                    return null;
                }
                return e.Frames[_frameIndex];
            }
        }

        private List<int> SelectedFrameIndices()
        {
            var list = lvFrames.SelectedIndices.Cast<int>().OrderBy(i => i).ToList();
            if (list.Count == 0 && CurrentFrame != null)
            {
                list.Add(_frameIndex);
            }
            return list;
        }

        private bool EnsureEntry()
        {
            if (_vd == null)
            {
                SetStatus("Keine VD-Datei geladen. Öffne eine Datei oder erstelle eine neue.");
                return false;
            }
            if (_action < 0)
            {
                SetStatus("Bitte zuerst eine Aktion links auswählen.");
                return false;
            }
            return true;
        }

        private void SetStatus(string text)
        {
            tsslStatus.Text = text;
        }

        // ════════════════════════════════════════════════════════════════
        //  File: new / open / save
        // ════════════════════════════════════════════════════════════════

        private bool ConfirmDiscard()
        {
            if (!_dirty)
            {
                return true;
            }

            DialogResult r = MessageBox.Show(this, "Die aktuelle VD-Datei wurde geändert. Jetzt speichern?", "VD Editor",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
            {
                return false;
            }
            if (r == DialogResult.Yes)
            {
                return SaveFile(false);
            }
            return true;
        }

        private void TsmiNewHigh_Click(object sender, EventArgs e) => NewFile(VdAnimType.High);
        private void TsmiNewLow_Click(object sender, EventArgs e) => NewFile(VdAnimType.Low);
        private void TsmiNewPeople_Click(object sender, EventArgs e) => NewFile(VdAnimType.People);

        private void NewFile(VdAnimType type)
        {
            if (!ConfirmDiscard())
            {
                return;
            }

            SetFile(new VdAnimationFile(type), null);
            SetStatus($"Neue VD-Datei ({type}, {VdAnimationFile.GetActionCount(type)} Aktionen) erstellt.");
        }

        private void TsbOpen_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscard())
            {
                return;
            }

            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "VD-Datei öffnen";
                ofd.Filter = "VD files (*.vd)|*.vd|Alle Dateien (*.*)|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    OpenFile(ofd.FileName);
                }
            }
        }

        private void OpenFile(string path)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                VdAnimationFile vd = VdAnimationFile.Load(path);
                SetFile(vd, path);
                SetStatus($"Geladen: {Path.GetFileName(path)} – {vd.AnimType}, {vd.TotalFrames} Frames.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "VD-Datei konnte nicht geladen werden:\n\n" + ex.Message, "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void SetFile(VdAnimationFile vd, string path)
        {
            StopPlayback();

            _vd?.Dispose();
            _vd = vd;
            _filePath = path;
            _dirty = false;
            _undo.Clear();
            _action = -1;
            _frameIndex = 0;
            _pan = Point.Empty;

            BuildTree();

            // select the first action that has frames (like the viewer does)
            int first = Enumerable.Range(0, _vd.ActionCount).FirstOrDefault(a => !_vd.IsActionEmpty(a));
            SelectAction(first, StoredDirection);
            UpdateUi();
        }

        private void TsbSave_Click(object sender, EventArgs e) => SaveFile(false);
        private void TsbSaveAs_Click(object sender, EventArgs e) => SaveFile(true);

        private bool SaveFile(bool askName)
        {
            if (_vd == null)
            {
                return false;
            }

            string path = _filePath;
            if (askName || string.IsNullOrEmpty(path))
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Title = "VD-Datei speichern";
                    sfd.Filter = "VD file (*.vd)|*.vd";
                    sfd.FileName = string.IsNullOrEmpty(_filePath) ? "animation.vd" : Path.GetFileName(_filePath);
                    if (sfd.ShowDialog(this) != DialogResult.OK)
                    {
                        return false;
                    }
                    path = sfd.FileName;
                }
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                _vd.Save(path);
                _filePath = path;
                _dirty = false;

                string msg = $"Gespeichert: {Path.GetFileName(path)}";
                if (_vd.LastSaveQuantizedEntries > 0)
                {
                    msg += $" – {_vd.LastSaveQuantizedEntries} Richtung(en) hatten mehr als 256 Farben und wurden reduziert.";
                }
                SetStatus(msg);
                UpdateUi();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Speichern fehlgeschlagen:\n\n" + ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void AnimationVDEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ConfirmDiscard())
            {
                e.Cancel = true;
                return;
            }

            StopPlayback();
            _playTimer.Dispose();
            _vd?.Dispose();
            _vd = null;
            ClearImageList();
        }

        // ════════════════════════════════════════════════════════════════
        //  Tree (actions + directions)
        // ════════════════════════════════════════════════════════════════

        private void BuildTree()
        {
            treeActions.BeginUpdate();
            treeActions.Nodes.Clear();
            if (_vd != null)
            {
                for (int a = 0; a < _vd.ActionCount; a++)
                {
                    var node = new TreeNode { Tag = a };
                    for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
                    {
                        node.Nodes.Add(new TreeNode { Tag = new[] { a, d } });
                    }
                    treeActions.Nodes.Add(node);
                }
            }
            treeActions.EndUpdate();
            RefreshTreeTexts();
        }

        private void RefreshTreeTexts()
        {
            if (_vd == null)
            {
                return;
            }

            treeActions.BeginUpdate();
            foreach (TreeNode node in treeActions.Nodes)
            {
                int a = (int)node.Tag;
                int total = 0;
                for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
                {
                    int n = _vd.Entries[a, d].Frames.Count;
                    total += n;
                    TreeNode child = node.Nodes[d];
                    child.Text = $"Richtung {d}  ({n})";
                    child.ForeColor = n == 0 ? Color.Gray : SystemColors.WindowText;
                }
                node.Text = $"{a:00}  {_vd.GetActionName(a)}  [{total}]";
                node.ForeColor = total == 0 ? Color.Gray : SystemColors.WindowText;
            }
            treeActions.EndUpdate();
        }

        private void TreeActions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_updatingUi || e.Node == null)
            {
                return;
            }

            if (e.Node.Tag is int action)
            {
                SelectAction(action, StoredDirection);
            }
            else if (e.Node.Tag is int[] ad)
            {
                SelectAction(ad[0], ad[1]);
            }
        }

        private void SelectAction(int action, int storedDir)
        {
            if (_vd == null)
            {
                return;
            }

            _action = Math.Max(0, Math.Min(action, _vd.ActionCount - 1));
            _frameIndex = 0;

            _updatingUi = true;
            // keep a mirrored view direction if it maps to the same stored direction
            if (StoredDirection != storedDir)
            {
                trackDirection.Value = storedDir;
            }
            // highlight the action in the tree when selected from code (e.g. after loading)
            if (_action < treeActions.Nodes.Count && treeActions.SelectedNode == null)
            {
                treeActions.SelectedNode = treeActions.Nodes[_action];
            }
            _updatingUi = false;

            RebuildFrameList();
            UpdateUi();
        }

        // ════════════════════════════════════════════════════════════════
        //  Frame list
        // ════════════════════════════════════════════════════════════════

        private void ClearImageList()
        {
            lvFrames.Items.Clear();
            foreach (Image img in imageListFrames.Images)
            {
                img.Dispose();
            }
            imageListFrames.Images.Clear();
        }

        private void RebuildFrameList()
        {
            lvFrames.BeginUpdate();
            ClearImageList();

            VdEntry entry = CurrentEntry;
            if (entry != null)
            {
                Size thumb = imageListFrames.ImageSize;
                for (int i = 0; i < entry.Frames.Count; i++)
                {
                    VdFrame f = entry.Frames[i];
                    imageListFrames.Images.Add(CreateThumbnail(f, thumb));
                    lvFrames.Items.Add(new ListViewItem($"{i}  ({f.Width}x{f.Height})", i));
                }
            }

            if (_frameIndex >= lvFrames.Items.Count)
            {
                _frameIndex = Math.Max(0, lvFrames.Items.Count - 1);
            }
            if (_frameIndex < lvFrames.Items.Count)
            {
                _updatingUi = true;
                lvFrames.Items[_frameIndex].Selected = true;
                lvFrames.Items[_frameIndex].Focused = true;
                lvFrames.EnsureVisible(_frameIndex);
                _updatingUi = false;
            }

            lvFrames.EndUpdate();
        }

        private Bitmap CreateThumbnail(VdFrame f, Size size)
        {
            var bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(_backColor);
                if (f.Width > 0 && f.Height > 0)
                {
                    float scale = Math.Min(1f, Math.Min((float)(size.Width - 2) / f.Width, (float)(size.Height - 2) / f.Height));
                    int w = Math.Max(1, (int)(f.Width * scale));
                    int h = Math.Max(1, (int)(f.Height * scale));
                    var r = new Rectangle((size.Width - w) / 2, (size.Height - h) / 2, w, h);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    if (IsMirroredView)
                    {
                        g.DrawImage(f.Image, new[] { new Point(r.Right, r.Top), new Point(r.Left, r.Top), new Point(r.Right, r.Bottom) });
                    }
                    else
                    {
                        g.DrawImage(f.Image, r);
                    }
                }
            }
            return bmp;
        }

        private void LvFrames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingUi || lvFrames.SelectedIndices.Count == 0)
            {
                return;
            }

            _frameIndex = lvFrames.SelectedIndices[0];
            UpdateUi();
        }

        // ════════════════════════════════════════════════════════════════
        //  UI state
        // ════════════════════════════════════════════════════════════════

        private void UpdateUi()
        {
            _updatingUi = true;

            VdEntry entry = CurrentEntry;
            VdFrame frame = CurrentFrame;
            bool hasFile = _vd != null;
            bool hasEntry = entry != null;
            bool hasFrame = frame != null;

            string title = "VD Animation Editor";
            if (hasFile)
            {
                title += " – " + (string.IsNullOrEmpty(_filePath) ? "(neu)" : Path.GetFileName(_filePath)) + (_dirty ? " *" : "");
            }
            Text = title;

            tsbSave.Enabled = hasFile;
            tsbSaveAs.Enabled = hasFile;
            tsddExport.Enabled = hasFile;
            tsddImport.Enabled = hasFile;
            tsbUndo.Enabled = _undo.Count > 0;
            tsbUndo.ToolTipText = _undo.Count > 0 ? "Rückgängig: " + _undo.First.Value.Name + " (Strg+Z)" : "Rückgängig (Strg+Z)";

            groupFrame.Enabled = hasEntry;
            groupDirection.Enabled = hasEntry;
            groupAction.Enabled = hasEntry;
            panelPlayback.Enabled = hasEntry;

            btnDirPaste.Enabled = _directionBuffer != null;
            btnDirPasteMirrored.Enabled = _directionBuffer != null;
            btnActionPaste.Enabled = _actionBuffer != null;

            int count = entry?.Frames.Count ?? 0;
            trackFrame.Maximum = Math.Max(0, count - 1);
            trackFrame.Value = Math.Max(0, Math.Min(_frameIndex, trackFrame.Maximum));
            lblFrame.Text = count == 0 ? "Frame –/0" : $"Frame {_frameIndex + 1}/{count}";

            nudCenterX.Enabled = hasFrame;
            nudCenterY.Enabled = hasFrame;
            if (hasFrame)
            {
                nudCenterX.Value = Clamp(frame.CenterX, nudCenterX);
                nudCenterY.Value = Clamp(frame.CenterY, nudCenterY);
            }

            string dirText = $"Richtung: {ViewDirection}";
            if (IsMirroredView)
            {
                dirText += $" (gespiegelt von {StoredDirection})";
            }
            lblDirection.Text = dirText;

            if (hasFile && _action >= 0)
            {
                tsslInfo.Text = $"{_vd.AnimType} | Aktion {_action:00} {_vd.GetActionName(_action)} | Richtung {StoredDirection} | " +
                                (hasFrame ? $"Frame {frame.Width}x{frame.Height}, Center {frame.CenterX},{frame.CenterY}" : "keine Frames");
            }
            else
            {
                tsslInfo.Text = "";
            }

            _updatingUi = false;
            pbPreview.Invalidate();
        }

        private static decimal Clamp(int v, NumericUpDown nud)
        {
            return Math.Max(nud.Minimum, Math.Min(nud.Maximum, v));
        }

        private void MarkChanged(bool rebuildList = true)
        {
            _dirty = true;
            if (rebuildList)
            {
                RebuildFrameList();
            }
            RefreshTreeTexts();
            UpdateUi();
        }

        // ════════════════════════════════════════════════════════════════
        //  Undo
        // ════════════════════════════════════════════════════════════════

        private void PushUndo(string name, bool wholeAction = false)
        {
            if (_vd == null || _action < 0)
            {
                return;
            }

            var step = new UndoStep { Name = name };
            if (wholeAction)
            {
                for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
                {
                    step.Entries.Add((_action, d, _vd.Entries[_action, d].Clone()));
                }
            }
            else
            {
                step.Entries.Add((_action, StoredDirection, CurrentEntry.Clone()));
            }

            _undo.AddFirst(step);
            while (_undo.Count > MaxUndo)
            {
                foreach (var x in _undo.Last.Value.Entries)
                {
                    x.Entry.Dispose();
                }
                _undo.RemoveLast();
            }
        }

        /// <summary>Snapshot of two directions (for swap / copy to).</summary>
        private void PushUndoDirections(string name, params int[] dirs)
        {
            var step = new UndoStep { Name = name };
            foreach (int d in dirs.Distinct())
            {
                step.Entries.Add((_action, d, _vd.Entries[_action, d].Clone()));
            }
            _undo.AddFirst(step);
        }

        private void TsbUndo_Click(object sender, EventArgs e) => Undo();

        private void Undo()
        {
            if (_undo.Count == 0 || _vd == null)
            {
                return;
            }

            UndoStep step = _undo.First.Value;
            _undo.RemoveFirst();
            foreach (var (a, d, entry) in step.Entries)
            {
                _vd.Entries[a, d].Dispose();
                _vd.Entries[a, d] = entry;
            }

            if (step.Entries.Count > 0)
            {
                _action = step.Entries[0].Action;
            }

            SetStatus("Rückgängig: " + step.Name);
            MarkChanged();
        }

        // ════════════════════════════════════════════════════════════════
        //  Preview
        // ════════════════════════════════════════════════════════════════

        private Point Origin => new Point(pbPreview.Width / 2 + _pan.X, pbPreview.Height * 2 / 3 + _pan.Y);

        private void PbPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(_backColor);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            Point o = Origin;

            if (chkShowCenter.Checked)
            {
                using (var pen = new Pen(Color.FromArgb(120, Color.Red)))
                {
                    g.DrawLine(pen, 0, o.Y, pbPreview.Width, o.Y);
                    g.DrawLine(pen, o.X, 0, o.X, pbPreview.Height);
                }
            }

            VdFrame f = CurrentFrame;
            if (f == null || f.Width == 0)
            {
                string hint = _vd == null
                    ? "VD-Datei öffnen (oder hier hineinziehen)"
                    : "Keine Frames in dieser Richtung – Bilder importieren oder einfügen";
                TextRenderer.DrawText(g, hint, Font, new Point(10, 10), Color.LightGray);
                return;
            }

            // position relative to the origin, scaled around the origin
            Point p = f.GetDrawPosition(Point.Empty, IsMirroredView);
            float x = o.X + p.X * _zoom;
            float y = o.Y + p.Y * _zoom;
            float w = f.Width * _zoom;
            float h = f.Height * _zoom;

            Bitmap img = f.Image;
            Bitmap hued = null;
            int hue = (int)nudHue.Value;
            if (hue > 0)
            {
                hued = CreateHuedImage(img, hue, chkHueGray.Checked);
                if (hued != null)
                {
                    img = hued;
                }
            }

            if (IsMirroredView)
            {
                g.DrawImage(img, new[] { new PointF(x + w, y), new PointF(x, y), new PointF(x + w, y + h) });
            }
            else
            {
                g.DrawImage(img, x, y, w, h);
            }

            hued?.Dispose();

            if (chkShowCenter.Checked)
            {
                using (var pen = new Pen(Color.FromArgb(90, Color.Yellow)) { DashStyle = DashStyle.Dot })
                {
                    g.DrawRectangle(pen, x, y, w, h);
                }
                g.FillEllipse(Brushes.Red, o.X - 3, o.Y - 3, 6, 6);
            }

            string info = $"{_vd.GetActionName(_action)}  Dir {ViewDirection}  Frame {_frameIndex + 1}/{CurrentEntry.Frames.Count}  " +
                          $"{f.Width}x{f.Height}  Center {f.CenterX},{f.CenterY}  Zoom {_zoom:0.##}x";
            TextRenderer.DrawText(g, info, Font, new Point(6, 6), Color.White);
        }

        /// <summary>Applies an Ultima hue (preview only, the frame data is not changed).</summary>
        private static Bitmap CreateHuedImage(Bitmap src, int hueId, bool onlyGray)
        {
            try
            {
                // fully qualified: MapReplaceNewForm.cs has its own Hues class in this namespace
                Ultima.Hue hue = Ultima.Hues.GetHue(hueId - 1);
                if (hue == null)
                {
                    return null;
                }

                using (Bitmap b1555 = To1555(src))
                {
                    hue.ApplyTo(b1555, onlyGray);
                    var result = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(result))
                    {
                        g.DrawImage(b1555, 0, 0, src.Width, src.Height);
                    }
                    return result;
                }
            }
            catch
            {
                // hues.mul not available – just show the normal colors
                return null;
            }
        }

        private static Bitmap To1555(Bitmap src)
        {
            int w = src.Width, h = src.Height;
            var dst = new Bitmap(w, h, PixelFormat.Format16bppArgb1555);
            var row = new int[w];
            var outRow = new short[w];
            BitmapData sd = src.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dd = dst.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            try
            {
                for (int y = 0; y < h; y++)
                {
                    Marshal.Copy(sd.Scan0 + y * sd.Stride, row, 0, w);
                    for (int x = 0; x < w; x++)
                    {
                        int c = row[x];
                        if (((c >> 24) & 0xFF) < 128)
                        {
                            outRow[x] = 0;
                            continue;
                        }
                        int v = 0x8000 | (((c >> 19) & 0x1F) << 10) | (((c >> 11) & 0x1F) << 5) | ((c >> 3) & 0x1F);
                        outRow[x] = unchecked((short)v);
                    }
                    Marshal.Copy(outRow, 0, dd.Scan0 + y * dd.Stride, w);
                }
            }
            finally
            {
                src.UnlockBits(sd);
                dst.UnlockBits(dd);
            }
            return dst;
        }

        private void PbPreview_Resize(object sender, EventArgs e) => pbPreview.Invalidate();

        private void PbPreview_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseStart = e.Location;
            _dragUndoPushed = false;
            _dragRestX = 0;
            _dragRestY = 0;

            if (e.Button == MouseButtons.Left && chkDragCenter.Checked && CurrentFrame != null)
            {
                _draggingFrame = true;
            }
            else if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                _panning = true;
                pbPreview.Cursor = Cursors.SizeAll;
            }
        }

        private void PbPreview_MouseMove(object sender, MouseEventArgs e)
        {
            int dx = e.X - _mouseStart.X;
            int dy = e.Y - _mouseStart.Y;
            if (dx == 0 && dy == 0)
            {
                return;
            }

            if (_panning)
            {
                _pan = new Point(_pan.X + dx, _pan.Y + dy);
                _mouseStart = e.Location;
                pbPreview.Invalidate();
            }
            else if (_draggingFrame)
            {
                // convert screen pixels to frame pixels (keep the rest for smooth zoomed dragging)
                _dragRestX += dx / _zoom;
                _dragRestY += dy / _zoom;
                int mx = (int)_dragRestX;
                int my = (int)_dragRestY;
                if (mx == 0 && my == 0)
                {
                    _mouseStart = e.Location;
                    return;
                }
                _dragRestX -= mx;
                _dragRestY -= my;

                if (!_dragUndoPushed)
                {
                    PushUndo("Center verschieben");
                    _dragUndoPushed = true;
                }

                // moving right means a smaller center X (mirrored view: the other way round)
                int cxDelta = IsMirroredView ? mx : -mx;
                ShiftCenters(cxDelta, -my);
                _mouseStart = e.Location;
            }
        }

        private void PbPreview_MouseUp(object sender, MouseEventArgs e)
        {
            if (_draggingFrame && _dragUndoPushed)
            {
                MarkChanged(false);
            }
            _draggingFrame = false;
            _panning = false;
            pbPreview.Cursor = Cursors.Default;
        }

        private void PbPreview_MouseWheel(object sender, MouseEventArgs e)
        {
            decimal z = nudZoom.Value + (e.Delta > 0 ? 0.5m : -0.5m);
            nudZoom.Value = Math.Max(nudZoom.Minimum, Math.Min(nudZoom.Maximum, z));
        }

        private void NudZoom_ValueChanged(object sender, EventArgs e)
        {
            _zoom = (float)nudZoom.Value;
            pbPreview.Invalidate();
        }

        private void BtnCenterView_Click(object sender, EventArgs e)
        {
            _pan = Point.Empty;
            pbPreview.Invalidate();
        }

        private void BtnBackColor_Click(object sender, EventArgs e)
        {
            using (var cd = new ColorDialog { Color = _backColor, FullOpen = true })
            {
                if (cd.ShowDialog(this) == DialogResult.OK)
                {
                    _backColor = cd.Color;
                    btnBackColor.BackColor = _backColor;
                    RebuildFrameList();
                    pbPreview.Invalidate();
                }
            }
        }

        private void ViewOption_Changed(object sender, EventArgs e) => pbPreview.Invalidate();

        private void TrackDirection_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingUi)
            {
                return;
            }

            int keep = _frameIndex;
            RebuildFrameList();
            _frameIndex = Math.Min(keep, Math.Max(0, (CurrentEntry?.Frames.Count ?? 1) - 1));
            UpdateUi();
        }

        private void TrackFrame_Scroll(object sender, EventArgs e)
        {
            if (_updatingUi)
            {
                return;
            }
            SelectFrame(trackFrame.Value);
        }

        private void SelectFrame(int index)
        {
            VdEntry entry = CurrentEntry;
            if (entry == null || entry.Frames.Count == 0)
            {
                return;
            }

            _frameIndex = ((index % entry.Frames.Count) + entry.Frames.Count) % entry.Frames.Count;

            _updatingUi = true;
            lvFrames.SelectedIndices.Clear();
            if (_frameIndex < lvFrames.Items.Count)
            {
                lvFrames.Items[_frameIndex].Selected = true;
                lvFrames.EnsureVisible(_frameIndex);
            }
            _updatingUi = false;

            UpdateUi();
        }

        // ════════════════════════════════════════════════════════════════
        //  Playback (Play/Pause, Stop, Slower, Faster – like the UOEC viewer)
        // ════════════════════════════════════════════════════════════════

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            if (_playTimer.Enabled)
            {
                _playTimer.Stop();
                btnPlay.Text = "▶ Play";
            }
            else if ((CurrentEntry?.Frames.Count ?? 0) > 1)
            {
                _playTimer.Start();
                btnPlay.Text = "❚❚ Pause";
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            StopPlayback();
            SelectFrame(0);
        }

        private void StopPlayback()
        {
            _playTimer.Stop();
            btnPlay.Text = "▶ Play";
        }

        private void BtnSlower_Click(object sender, EventArgs e)
        {
            _delay = Math.Min(1000, _delay + 20);
            _playTimer.Interval = _delay;
            UpdateSpeedLabel();
        }

        private void BtnFaster_Click(object sender, EventArgs e)
        {
            _delay = Math.Max(20, _delay - 20);
            _playTimer.Interval = _delay;
            UpdateSpeedLabel();
        }

        private void UpdateSpeedLabel()
        {
            lblSpeed.Text = $"{_delay} ms";
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            VdEntry entry = CurrentEntry;
            if (entry == null || entry.Frames.Count < 2)
            {
                StopPlayback();
                return;
            }
            SelectFrame(_frameIndex + 1);
        }

        // ════════════════════════════════════════════════════════════════
        //  Center editing
        // ════════════════════════════════════════════════════════════════

        private void NudCenter_ValueChanged(object sender, EventArgs e)
        {
            VdFrame f = CurrentFrame;
            if (_updatingUi || f == null)
            {
                return;
            }

            int dx = (int)nudCenterX.Value - f.CenterX;
            int dy = (int)nudCenterY.Value - f.CenterY;
            if (dx == 0 && dy == 0)
            {
                return;
            }

            PushUndo("Center ändern");
            ShiftCenters(dx, dy);
            MarkChanged(false);
        }

        /// <summary>Changes the center of the selected frames (or all frames of the direction).</summary>
        private void ShiftCenters(int dx, int dy)
        {
            VdEntry entry = CurrentEntry;
            if (entry == null)
            {
                return;
            }

            IEnumerable<int> targets = chkCenterAll.Checked ? Enumerable.Range(0, entry.Frames.Count) : SelectedFrameIndices();
            foreach (int i in targets)
            {
                entry.Frames[i].CenterX += dx;
                entry.Frames[i].CenterY += dy;
            }

            _dirty = true;
            UpdateUi();
        }

        // ════════════════════════════════════════════════════════════════
        //  Frame operations
        // ════════════════════════════════════════════════════════════════

        private void BtnFrameInsert_Click(object sender, EventArgs e)
        {
            if (!EnsureEntry())
            {
                return;
            }

            using (var ofd = new OpenFileDialog { Filter = ImageFilter, Multiselect = true, Title = "Frames einfügen" })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    InsertFramesFromFiles(ofd.FileNames);
                }
            }
        }

        private void InsertFramesFromFiles(IEnumerable<string> files)
        {
            List<VdFrame> frames = LoadFramesFromFiles(files.OrderBy(NaturalKey).ToList());
            if (frames.Count == 0)
            {
                return;
            }

            PushUndo("Frames einfügen");
            VdEntry entry = CurrentEntry;
            int pos = entry.Frames.Count == 0 ? 0 : Math.Min(entry.Frames.Count, _frameIndex + 1);
            entry.Frames.InsertRange(pos, frames);
            _frameIndex = pos;
            MarkChanged();
            SetStatus($"{frames.Count} Frame(s) eingefügt.");
        }

        private void BtnFrameReplace_Click(object sender, EventArgs e)
        {
            if (!EnsureEntry() || CurrentFrame == null)
            {
                SetStatus("Kein Frame ausgewählt.");
                return;
            }

            using (var ofd = new OpenFileDialog { Filter = ImageFilter, Title = "Frame ersetzen" })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<VdFrame> frames = LoadFramesFromFiles(new List<string> { ofd.FileName }, CurrentFrame);
                if (frames.Count == 0)
                {
                    return;
                }

                PushUndo("Frame ersetzen");
                ReplaceFrame(_frameIndex, frames[0]);
                MarkChanged();
                SetStatus("Frame ersetzt.");
            }
        }

        private void ReplaceFrame(int index, VdFrame frame)
        {
            VdEntry entry = CurrentEntry;
            entry.Frames[index].Dispose();
            entry.Frames[index] = frame;
        }

        private void BtnFrameDelete_Click(object sender, EventArgs e) => DeleteSelectedFrames();

        private void DeleteSelectedFrames()
        {
            VdEntry entry = CurrentEntry;
            List<int> sel = SelectedFrameIndices();
            if (entry == null || sel.Count == 0)
            {
                return;
            }

            PushUndo("Frames löschen");
            foreach (int i in sel.OrderByDescending(i => i))
            {
                entry.Frames[i].Dispose();
                entry.Frames.RemoveAt(i);
            }
            _frameIndex = Math.Max(0, sel[0] - 1);
            MarkChanged();
            SetStatus($"{sel.Count} Frame(s) gelöscht.");
        }

        private void BtnFrameUp_Click(object sender, EventArgs e) => MoveFrame(-1);
        private void BtnFrameDown_Click(object sender, EventArgs e) => MoveFrame(1);

        private void MoveFrame(int delta)
        {
            VdEntry entry = CurrentEntry;
            if (entry == null || CurrentFrame == null)
            {
                return;
            }

            int target = _frameIndex + delta;
            if (target < 0 || target >= entry.Frames.Count)
            {
                return;
            }

            PushUndo("Frame verschieben");
            VdFrame f = entry.Frames[_frameIndex];
            entry.Frames[_frameIndex] = entry.Frames[target];
            entry.Frames[target] = f;
            _frameIndex = target;
            MarkChanged();
        }

        private void BtnFrameReverse_Click(object sender, EventArgs e)
        {
            VdEntry entry = CurrentEntry;
            if (entry == null || entry.Frames.Count < 2)
            {
                return;
            }

            PushUndo("Reihenfolge umkehren");
            entry.Frames.Reverse();
            MarkChanged();
        }

        private void BtnFrameMirrorH_Click(object sender, EventArgs e) => MirrorSelected(true);
        private void BtnFrameMirrorV_Click(object sender, EventArgs e) => MirrorSelected(false);

        private void MirrorSelected(bool horizontal)
        {
            VdEntry entry = CurrentEntry;
            List<int> sel = SelectedFrameIndices();
            if (entry == null || sel.Count == 0)
            {
                return;
            }

            PushUndo(horizontal ? "Frame spiegeln ↔" : "Frame spiegeln ↕");
            foreach (int i in sel)
            {
                if (horizontal)
                {
                    entry.Frames[i].MirrorHorizontal();
                }
                else
                {
                    entry.Frames[i].MirrorVertical();
                }
            }
            MarkChanged();
        }

        // ── Clipboard / Zwischenspeicher ─────────────────────────────────

        private void BtnFrameCopy_Click(object sender, EventArgs e) => CopyFrames();

        private void CopyFrames()
        {
            VdEntry entry = CurrentEntry;
            List<int> sel = SelectedFrameIndices();
            if (entry == null || sel.Count == 0)
            {
                return;
            }

            foreach (VdFrame f in _frameBuffer)
            {
                f.Dispose();
            }
            _frameBuffer = sel.Select(i => entry.Frames[i].Clone()).ToList();

            // also put the first frame on the windows clipboard (PNG keeps transparency)
            try
            {
                _clipboardToken = Guid.NewGuid().ToString();
                var data = new DataObject();
                data.SetData(ClipboardFormat, _clipboardToken);
                data.SetImage(_frameBuffer[0].Image);
                using (var ms = new MemoryStream())
                {
                    _frameBuffer[0].Image.Save(ms, ImageFormat.Png);
                    data.SetData("PNG", false, new MemoryStream(ms.ToArray()));
                }
                Clipboard.SetDataObject(data, true);
            }
            catch (ExternalException)
            {
                // clipboard busy – the internal buffer still works
            }

            SetStatus($"{_frameBuffer.Count} Frame(s) in den Zwischenspeicher kopiert.");
        }

        /// <summary>Frames to paste: our own buffer (keeps centers) or an image from the windows clipboard.</summary>
        private List<VdFrame> GetPasteFrames()
        {
            try
            {
                IDataObject data = Clipboard.GetDataObject();
                bool ours = data != null && data.GetDataPresent(ClipboardFormat) &&
                            Equals(data.GetData(ClipboardFormat) as string, _clipboardToken);

                if (!ours && data != null)
                {
                    Bitmap img = null;
                    if (data.GetDataPresent("PNG") && data.GetData("PNG") is MemoryStream png)
                    {
                        using (var tmp = new Bitmap(png))
                        {
                            img = new Bitmap(tmp);
                        }
                    }
                    else if (Clipboard.ContainsImage())
                    {
                        using (Image tmp = Clipboard.GetImage())
                        {
                            img = new Bitmap(tmp);
                        }
                    }

                    if (img != null)
                    {
                        VdFrame f = CreateFrameFromImage(img, null, null, CurrentFrame);
                        return f == null ? new List<VdFrame>() : new List<VdFrame> { f };
                    }
                }
            }
            catch (ExternalException)
            {
                // clipboard busy – fall back to the internal buffer
            }

            return _frameBuffer.Select(f => f.Clone()).ToList();
        }

        private void BtnFramePaste_Click(object sender, EventArgs e) => PasteFrames(false);
        private void BtnFramePasteReplace_Click(object sender, EventArgs e) => PasteFrames(true);

        private void PasteFrames(bool replace)
        {
            if (!EnsureEntry())
            {
                return;
            }

            List<VdFrame> frames = GetPasteFrames();
            if (frames.Count == 0)
            {
                SetStatus("Zwischenspeicher ist leer.");
                return;
            }

            VdEntry entry = CurrentEntry;
            if (replace && CurrentFrame != null)
            {
                PushUndo("Frame durch Zwischenspeicher ersetzen");
                List<int> sel = SelectedFrameIndices();
                // replace the selected frames one by one with the buffered frames
                for (int i = 0; i < sel.Count && i < frames.Count; i++)
                {
                    ReplaceFrame(sel[i], frames[i]);
                }
                foreach (VdFrame unused in frames.Skip(sel.Count))
                {
                    unused.Dispose();
                }
                SetStatus("Frame(s) ersetzt.");
            }
            else
            {
                PushUndo("Frames einfügen");
                int pos = entry.Frames.Count == 0 ? 0 : Math.Min(entry.Frames.Count, _frameIndex + 1);
                entry.Frames.InsertRange(pos, frames);
                _frameIndex = pos;
                SetStatus($"{frames.Count} Frame(s) eingefügt.");
            }

            MarkChanged();
        }

        // ════════════════════════════════════════════════════════════════
        //  Direction operations
        // ════════════════════════════════════════════════════════════════

        private void BtnDirCopy_Click(object sender, EventArgs e)
        {
            if (CurrentEntry == null)
            {
                return;
            }
            _directionBuffer?.Dispose();
            _directionBuffer = CurrentEntry.Clone();
            SetStatus($"Richtung {StoredDirection} ({_directionBuffer.Frames.Count} Frames) im Zwischenspeicher.");
            UpdateUi();
        }

        private void BtnDirPaste_Click(object sender, EventArgs e) => PasteDirection(false);
        private void BtnDirPasteMirrored_Click(object sender, EventArgs e) => PasteDirection(true);

        private void PasteDirection(bool mirrored)
        {
            if (_directionBuffer == null || CurrentEntry == null)
            {
                return;
            }

            PushUndo(mirrored ? "Richtung gespiegelt einfügen" : "Richtung einfügen");
            VdEntry copy = _directionBuffer.Clone();
            if (mirrored)
            {
                foreach (VdFrame f in copy.Frames)
                {
                    f.MirrorHorizontal();
                }
            }
            SetEntry(_action, StoredDirection, copy);
            _frameIndex = 0;
            MarkChanged();
        }

        private void SetEntry(int action, int dir, VdEntry entry)
        {
            _vd.Entries[action, dir].Dispose();
            _vd.Entries[action, dir] = entry;
        }

        private void BtnDirMirror_Click(object sender, EventArgs e)
        {
            VdEntry entry = CurrentEntry;
            if (entry == null || entry.IsEmpty)
            {
                return;
            }

            PushUndo("Richtung spiegeln");
            foreach (VdFrame f in entry.Frames)
            {
                f.MirrorHorizontal();
            }
            MarkChanged();
        }

        private void BtnDirClear_Click(object sender, EventArgs e)
        {
            VdEntry entry = CurrentEntry;
            if (entry == null || entry.IsEmpty)
            {
                return;
            }

            if (MessageBox.Show(this, $"Alle {entry.Frames.Count} Frames von Richtung {StoredDirection} löschen?", "Richtung leeren",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            PushUndo("Richtung leeren");
            entry.Clear();
            _frameIndex = 0;
            MarkChanged();
        }

        private void BtnDirSwap_Click(object sender, EventArgs e)
        {
            if (CurrentEntry == null)
            {
                return;
            }

            int a = StoredDirection;
            int b = cmbTargetDir.SelectedIndex;
            if (a == b)
            {
                SetStatus("Quell- und Zielrichtung sind gleich.");
                return;
            }

            PushUndoDirections($"Richtung {a} ↔ {b} tauschen", a, b);
            VdEntry tmp = _vd.Entries[_action, a];
            _vd.Entries[_action, a] = _vd.Entries[_action, b];
            _vd.Entries[_action, b] = tmp;
            MarkChanged();
            SetStatus($"Richtung {a} und {b} getauscht.");
        }

        private void BtnDirCopyTo_Click(object sender, EventArgs e)
        {
            if (CurrentEntry == null)
            {
                return;
            }

            int a = StoredDirection;
            int b = cmbTargetDir.SelectedIndex;
            if (a == b)
            {
                SetStatus("Quell- und Zielrichtung sind gleich.");
                return;
            }

            PushUndoDirections($"Richtung {a} → {b} kopieren", a, b);
            VdEntry copy = CurrentEntry.Clone();
            if (chkCopyToMirrored.Checked)
            {
                foreach (VdFrame f in copy.Frames)
                {
                    f.MirrorHorizontal();
                }
            }
            SetEntry(_action, b, copy);
            MarkChanged();
            SetStatus($"Richtung {a} nach {b} kopiert" + (chkCopyToMirrored.Checked ? " (gespiegelt)." : "."));
        }

        // ════════════════════════════════════════════════════════════════
        //  Action operations
        // ════════════════════════════════════════════════════════════════

        private void BtnActionCopy_Click(object sender, EventArgs e)
        {
            if (_vd == null || _action < 0)
            {
                return;
            }

            if (_actionBuffer != null)
            {
                foreach (VdEntry x in _actionBuffer)
                {
                    x.Dispose();
                }
            }

            _actionBuffer = Enumerable.Range(0, VdAnimationFile.DirectionCount).Select(d => _vd.Entries[_action, d].Clone()).ToArray();
            SetStatus($"Aktion {_action:00} {_vd.GetActionName(_action)} im Zwischenspeicher.");
            UpdateUi();
        }

        private void BtnActionPaste_Click(object sender, EventArgs e)
        {
            if (_actionBuffer == null || _vd == null || _action < 0)
            {
                return;
            }

            PushUndo("Aktion einfügen", true);
            for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
            {
                SetEntry(_action, d, _actionBuffer[d].Clone());
            }
            _frameIndex = 0;
            MarkChanged();
        }

        private void BtnActionMirror_Click(object sender, EventArgs e)
        {
            if (_vd == null || _action < 0)
            {
                return;
            }

            PushUndo("Aktion spiegeln", true);
            for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
            {
                foreach (VdFrame f in _vd.Entries[_action, d].Frames)
                {
                    f.MirrorHorizontal();
                }
            }
            MarkChanged();
        }

        private void BtnActionClear_Click(object sender, EventArgs e)
        {
            if (_vd == null || _action < 0 || _vd.IsActionEmpty(_action))
            {
                return;
            }

            if (MessageBox.Show(this, $"Alle Frames der Aktion {_action:00} {_vd.GetActionName(_action)} löschen?", "Aktion leeren",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            PushUndo("Aktion leeren", true);
            for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
            {
                _vd.Entries[_action, d].Clear();
            }
            _frameIndex = 0;
            MarkChanged();
        }

        // ════════════════════════════════════════════════════════════════
        //  Import helpers
        // ════════════════════════════════════════════════════════════════

        private static string NaturalKey(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path) ?? "";
            return Regex.Replace(name, @"\d+", m => m.Value.PadLeft(10, '0'));
        }

        private List<VdFrame> LoadFramesFromFiles(List<string> files, VdFrame reference = null)
        {
            var frames = new List<VdFrame>();
            var errors = new List<string>();

            foreach (string file in files)
            {
                try
                {
                    Bitmap img;
                    using (var tmp = new Bitmap(file))
                    {
                        img = new Bitmap(tmp);
                    }

                    Point? origin = ReadOrigin(Path.GetDirectoryName(file));
                    VdFrame f = CreateFrameFromImage(img, origin, file, reference ?? CurrentFrame);
                    if (f != null)
                    {
                        frames.Add(f);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(Path.GetFileName(file) + ": " + ex.Message);
                }
            }

            if (errors.Count > 0)
            {
                MessageBox.Show(this, "Einige Bilder konnten nicht geladen werden:\n\n" + string.Join("\n", errors.Take(15)),
                    "Import", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return frames;
        }

        /// <summary>
        /// Builds a frame from an image using the selected center mode, transparent color and auto crop.
        /// <paramref name="origin"/> is the canvas origin from an export (vd_origin.txt).
        /// </summary>
        private VdFrame CreateFrameFromImage(Bitmap img, Point? origin, string fileName, VdFrame reference)
        {
            Bitmap bmp = VdAnimationFile.To32Bpp(img, true);
            ApplyTransparentColor(bmp);

            // where is the anchor (origin) in the full image?
            Point anchor;
            switch (cmbImportCenter.SelectedIndex)
            {
                case 1: // like the current frame: keep its center
                    if (reference != null)
                    {
                        anchor = new Point(reference.CenterX, reference.CenterY + bmp.Height);
                        break;
                    }
                    goto default;
                case 2: // canvas origin from the export
                    if (origin.HasValue)
                    {
                        anchor = origin.Value;
                        break;
                    }
                    goto default;
                default: // bottom middle
                    anchor = new Point(bmp.Width / 2, bmp.Height);
                    break;
            }

            if (chkAutoCrop.Checked)
            {
                Rectangle? bounds = VdAnimationFile.GetOpaqueBounds(bmp);
                if (bounds == null)
                {
                    // fully transparent image: keep an empty 1x1 frame
                    bmp.Dispose();
                    return new VdFrame(null, 0, 0);
                }

                Rectangle r = bounds.Value;
                if (r.Size != bmp.Size)
                {
                    Bitmap cropped = bmp.Clone(r, PixelFormat.Format32bppArgb);
                    bmp.Dispose();
                    bmp = cropped;
                    anchor = new Point(anchor.X - r.X, anchor.Y - r.Y);
                }
            }

            // left = origin - cx  →  cx = anchorX ;  top = origin - cy - h  →  cy = anchorY - h
            return new VdFrame(bmp, anchor.X, anchor.Y - bmp.Height);
        }

        private void ApplyTransparentColor(Bitmap bmp)
        {
            int mode = cmbTransparent.SelectedIndex;
            if (mode <= 0)
            {
                return;
            }

            int w = bmp.Width, h = bmp.Height;
            var row = new int[w];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                int key;
                switch (mode)
                {
                    case 1: key = 0x000000; break;
                    case 2: key = 0xFFFFFF; break;
                    case 3: key = 0xFF00FF; break;
                    default:
                        Marshal.Copy(bd.Scan0, row, 0, w);
                        key = row[0] & 0xFFFFFF;
                        break;
                }

                for (int y = 0; y < h; y++)
                {
                    IntPtr p = bd.Scan0 + y * bd.Stride;
                    Marshal.Copy(p, row, 0, w);
                    for (int x = 0; x < w; x++)
                    {
                        if ((row[x] & 0xFFFFFF) == key)
                        {
                            row[x] = 0;
                        }
                    }
                    Marshal.Copy(row, 0, p, w);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
        }

        private static Point? ReadOrigin(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return null;
            }

            string file = Path.Combine(folder, OriginFileName);
            if (!File.Exists(file))
            {
                return null;
            }

            string[] parts = File.ReadAllText(file).Split(new[] { ' ', ';', ',', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 &&
                int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x) &&
                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y))
            {
                return new Point(x, y);
            }
            return null;
        }

        // ════════════════════════════════════════════════════════════════
        //  Import menu
        // ════════════════════════════════════════════════════════════════

        private void TsmiImportFrames_Click(object sender, EventArgs e) => BtnFrameInsert_Click(sender, e);

        private void TsmiImportReplaceDirection_Click(object sender, EventArgs e)
        {
            if (!EnsureEntry())
            {
                return;
            }

            using (var ofd = new OpenFileDialog { Filter = ImageFilter, Multiselect = true, Title = "Richtung durch Bilder ersetzen" })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<VdFrame> frames = LoadFramesFromFiles(ofd.FileNames.OrderBy(NaturalKey).ToList());
                if (frames.Count == 0)
                {
                    return;
                }

                PushUndo("Richtung ersetzen");
                CurrentEntry.Clear();
                CurrentEntry.Frames.AddRange(frames);
                _frameIndex = 0;
                MarkChanged();
                SetStatus($"Richtung {StoredDirection} durch {frames.Count} Bilder ersetzt.");
            }
        }

        private void TsmiImportActionFolder_Click(object sender, EventArgs e)
        {
            if (!EnsureEntry())
            {
                return;
            }

            string folder = PickFolder("Ordner der Aktion wählen (enthält Dir_0 … Dir_4)");
            if (folder == null)
            {
                return;
            }

            PushUndo("Aktion aus Ordner importieren", true);
            int n = ImportActionFolder(_action, folder);
            _frameIndex = 0;
            MarkChanged();
            SetStatus($"{n} Frames in Aktion {_action:00} importiert.");
        }

        private void TsmiImportAllFolder_Click(object sender, EventArgs e)
        {
            if (_vd == null)
            {
                return;
            }

            string folder = PickFolder("Export-Ordner wählen (enthält A00_… Ordner)");
            if (folder == null)
            {
                return;
            }

            if (MessageBox.Show(this, "Alle Aktionen, für die ein Ordner gefunden wird, werden ersetzt.\n" +
                                      "Das kann nicht rückgängig gemacht werden. Fortfahren?", "Alles importieren",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            int total = 0;
            foreach (string dir in Directory.GetDirectories(folder))
            {
                Match m = Regex.Match(Path.GetFileName(dir), @"^A(\d+)", RegexOptions.IgnoreCase);
                if (m.Success && int.TryParse(m.Groups[1].Value, out int a) && a < _vd.ActionCount)
                {
                    total += ImportActionFolder(a, dir);
                }
            }

            _undo.Clear();
            _frameIndex = 0;
            MarkChanged();
            SetStatus($"{total} Frames importiert.");
        }

        /// <summary>Imports Dir_0..Dir_4 sub folders into an action. Folders that are missing stay unchanged.</summary>
        private int ImportActionFolder(int action, string folder)
        {
            int total = 0;
            for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
            {
                string dirFolder = Path.Combine(folder, "Dir_" + d);
                if (!Directory.Exists(dirFolder))
                {
                    continue;
                }

                List<string> files = Directory.GetFiles(dirFolder)
                    .Where(f => Regex.IsMatch(f, @"\.(png|bmp|gif|jpe?g|tiff?)$", RegexOptions.IgnoreCase))
                    .OrderBy(NaturalKey).ToList();

                List<VdFrame> frames = LoadFramesFromFiles(files);
                VdEntry entry = _vd.Entries[action, d];
                entry.Clear();
                entry.Frames.AddRange(frames);
                total += frames.Count;
            }
            return total;
        }

        private void TsmiImportSheetDirection_Click(object sender, EventArgs e)
        {
            if (!EnsureEntry())
            {
                return;
            }

            using (var ofd = new OpenFileDialog { Filter = ImageFilter, Title = "Spritesheet (eine Zeile) importieren" })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Bitmap sheet;
                using (var tmp = new Bitmap(ofd.FileName))
                {
                    sheet = new Bitmap(tmp);
                }

                using (sheet)
                {
                    // companion file from our own export: cellW cellH originX originY count
                    int cellW = sheet.Height, cellH = sheet.Height, count = 0;
                    Point? origin = null;
                    string info = Path.ChangeExtension(ofd.FileName, ".txt");
                    if (File.Exists(info))
                    {
                        int[] v = File.ReadAllText(info).Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : 0).ToArray();
                        if (v.Length >= 5)
                        {
                            cellW = v[0];
                            cellH = v[1];
                            origin = new Point(v[2], v[3]);
                            count = v[4];
                        }
                    }

                    if (cellW <= 0 || cellH <= 0)
                    {
                        return;
                    }
                    if (count <= 0)
                    {
                        count = sheet.Width / cellW;
                    }

                    var frames = new List<VdFrame>();
                    for (int i = 0; i < count && (i + 1) * cellW <= sheet.Width; i++)
                    {
                        Bitmap cell = sheet.Clone(new Rectangle(i * cellW, 0, cellW, Math.Min(cellH, sheet.Height)), PixelFormat.Format32bppArgb);
                        VdFrame f = CreateFrameFromImage(cell, origin, null, CurrentFrame);
                        if (f != null)
                        {
                            frames.Add(f);
                        }
                    }

                    if (frames.Count == 0)
                    {
                        return;
                    }

                    PushUndo("Spritesheet importieren");
                    CurrentEntry.Clear();
                    CurrentEntry.Frames.AddRange(frames);
                    _frameIndex = 0;
                    MarkChanged();
                    SetStatus($"{frames.Count} Frames aus Spritesheet importiert.");
                }
            }
        }

        private string PickFolder(string description)
        {
            using (var fbd = new FolderBrowserDialog { Description = description, UseDescriptionForTitle = true })
            {
                return fbd.ShowDialog(this) == DialogResult.OK ? fbd.SelectedPath : null;
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Export
        // ════════════════════════════════════════════════════════════════

        private string BaseName => string.IsNullOrEmpty(_filePath) ? "animation" : Path.GetFileNameWithoutExtension(_filePath);

        private static string SafeName(string s) => Regex.Replace(s, @"[^\w\-]", "_");

        /// <summary>Common canvas of all frames of an entry: size and origin inside the canvas.</summary>
        private static (Size Size, Point Origin) GetCanvas(VdEntry entry)
        {
            int left = 0, top = 0, right = 1, bottom = 1;
            bool first = true;
            foreach (VdFrame f in entry.Frames)
            {
                Point p = f.GetDrawPosition(Point.Empty, false);
                if (first)
                {
                    left = p.X; top = p.Y; right = p.X + f.Width; bottom = p.Y + f.Height;
                    first = false;
                }
                else
                {
                    left = Math.Min(left, p.X);
                    top = Math.Min(top, p.Y);
                    right = Math.Max(right, p.X + f.Width);
                    bottom = Math.Max(bottom, p.Y + f.Height);
                }
            }
            // origin always inside the canvas
            left = Math.Min(left, 0);
            top = Math.Min(top, 0);
            right = Math.Max(right, 1);
            bottom = Math.Max(bottom, 1);
            return (new Size(right - left, bottom - top), new Point(-left, -top));
        }

        private static Bitmap RenderOnCanvas(VdFrame f, Size size, Point origin, Color? background = null)
        {
            var bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(background ?? Color.Transparent);
                Point p = f.GetDrawPosition(origin, false);
                g.DrawImageUnscaled(f.Image, p);
            }
            return bmp;
        }

        /// <summary>Writes all frames of one entry as canvas PNGs + vd_origin.txt (re-importable with "Canvas-Ursprung").</summary>
        private int ExportEntryToFolder(VdEntry entry, string folder, string prefix)
        {
            if (entry.IsEmpty)
            {
                return 0;
            }

            Directory.CreateDirectory(folder);
            (Size size, Point origin) = GetCanvas(entry);
            for (int i = 0; i < entry.Frames.Count; i++)
            {
                using (Bitmap bmp = RenderOnCanvas(entry.Frames[i], size, origin))
                {
                    bmp.Save(Path.Combine(folder, $"{prefix}_{i:000}.png"), ImageFormat.Png);
                }
            }
            File.WriteAllText(Path.Combine(folder, OriginFileName),
                origin.X.ToString(CultureInfo.InvariantCulture) + " " + origin.Y.ToString(CultureInfo.InvariantCulture));
            return entry.Frames.Count;
        }

        private void TsmiExportFrame_Click(object sender, EventArgs e)
        {
            VdFrame f = CurrentFrame;
            if (f == null)
            {
                SetStatus("Kein Frame ausgewählt.");
                return;
            }

            using (var sfd = new SaveFileDialog { Filter = "PNG (*.png)|*.png|BMP (*.bmp)|*.bmp", Title = "Frame exportieren" })
            {
                sfd.FileName = $"{BaseName}_a{_action:00}_d{StoredDirection}_f{_frameIndex:000}.png";
                if (sfd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                ImageFormat fmt = sfd.FileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ? ImageFormat.Bmp : ImageFormat.Png;
                f.Image.Save(sfd.FileName, fmt);
                SetStatus("Frame exportiert: " + Path.GetFileName(sfd.FileName));
            }
        }

        private void TsmiExportDirection_Click(object sender, EventArgs e)
        {
            if (CurrentEntry == null || CurrentEntry.IsEmpty)
            {
                SetStatus("Diese Richtung enthält keine Frames.");
                return;
            }

            string folder = PickFolder("Zielordner für die Frames der Richtung");
            if (folder == null)
            {
                return;
            }

            int n = ExportEntryToFolder(CurrentEntry, folder, $"{BaseName}_a{_action:00}_d{StoredDirection}");
            SetStatus($"{n} Frames exportiert nach {folder}");
        }

        private void TsmiExportAction_Click(object sender, EventArgs e)
        {
            if (_vd == null || _action < 0)
            {
                return;
            }

            string folder = PickFolder("Zielordner für die Aktion (es werden Dir_0 … Dir_4 angelegt)");
            if (folder == null)
            {
                return;
            }

            int n = 0;
            for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
            {
                n += ExportEntryToFolder(_vd.Entries[_action, d], Path.Combine(folder, "Dir_" + d), $"{BaseName}_a{_action:00}_d{d}");
            }
            SetStatus($"{n} Frames exportiert nach {folder}");
        }

        private void TsmiExportAll_Click(object sender, EventArgs e)
        {
            if (_vd == null)
            {
                return;
            }

            string folder = PickFolder("Zielordner für alle Aktionen");
            if (folder == null)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                int n = 0;
                for (int a = 0; a < _vd.ActionCount; a++)
                {
                    if (_vd.IsActionEmpty(a))
                    {
                        continue;
                    }

                    string actionFolder = Path.Combine(folder, $"A{a:00}_{SafeName(_vd.GetActionName(a))}");
                    for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
                    {
                        n += ExportEntryToFolder(_vd.Entries[a, d], Path.Combine(actionFolder, "Dir_" + d), $"{BaseName}_a{a:00}_d{d}");
                    }
                    Application.DoEvents();
                }
                SetStatus($"{n} Frames exportiert nach {folder}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void TsmiExportSheetDirection_Click(object sender, EventArgs e)
        {
            VdEntry entry = CurrentEntry;
            if (entry == null || entry.IsEmpty)
            {
                SetStatus("Diese Richtung enthält keine Frames.");
                return;
            }

            using (var sfd = new SaveFileDialog { Filter = "PNG (*.png)|*.png", Title = "Spritesheet speichern" })
            {
                sfd.FileName = $"{BaseName}_a{_action:00}_d{StoredDirection}_sheet.png";
                if (sfd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                (Size cell, Point origin) = GetCanvas(entry);
                using (var sheet = new Bitmap(cell.Width * entry.Frames.Count, cell.Height, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(sheet))
                    {
                        g.Clear(Color.Transparent);
                        for (int i = 0; i < entry.Frames.Count; i++)
                        {
                            Point p = entry.Frames[i].GetDrawPosition(new Point(origin.X + i * cell.Width, origin.Y), false);
                            g.DrawImageUnscaled(entry.Frames[i].Image, p);
                        }
                    }
                    sheet.Save(sfd.FileName, ImageFormat.Png);
                }

                File.WriteAllText(Path.ChangeExtension(sfd.FileName, ".txt"),
                    string.Join(" ", cell.Width, cell.Height, origin.X, origin.Y, entry.Frames.Count));
                SetStatus("Spritesheet gespeichert: " + Path.GetFileName(sfd.FileName));
            }
        }

        private void TsmiExportSheetAction_Click(object sender, EventArgs e)
        {
            if (_vd == null || _action < 0 || _vd.IsActionEmpty(_action))
            {
                SetStatus("Diese Aktion enthält keine Frames.");
                return;
            }

            using (var sfd = new SaveFileDialog { Filter = "PNG (*.png)|*.png", Title = "Spritesheet (Aktion) speichern" })
            {
                sfd.FileName = $"{BaseName}_a{_action:00}_sheet.png";
                if (sfd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                // one row per direction, shared cell size
                int cellW = 1, cellH = 1, ox = 0, oy = 0, maxFrames = 1;
                for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
                {
                    VdEntry en = _vd.Entries[_action, d];
                    if (en.IsEmpty)
                    {
                        continue;
                    }
                    (Size s, Point o) = GetCanvas(en);
                    ox = Math.Max(ox, o.X);
                    oy = Math.Max(oy, o.Y);
                    cellW = Math.Max(cellW, s.Width - o.X);
                    cellH = Math.Max(cellH, s.Height - o.Y);
                    maxFrames = Math.Max(maxFrames, en.Frames.Count);
                }
                cellW += ox;
                cellH += oy;

                using (var sheet = new Bitmap(cellW * maxFrames, cellH * VdAnimationFile.DirectionCount, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(sheet))
                    {
                        g.Clear(Color.Transparent);
                        for (int d = 0; d < VdAnimationFile.DirectionCount; d++)
                        {
                            List<VdFrame> frames = _vd.Entries[_action, d].Frames;
                            for (int i = 0; i < frames.Count; i++)
                            {
                                Point p = frames[i].GetDrawPosition(new Point(ox + i * cellW, oy + d * cellH), false);
                                g.DrawImageUnscaled(frames[i].Image, p);
                            }
                        }
                    }
                    sheet.Save(sfd.FileName, ImageFormat.Png);
                }
                SetStatus("Spritesheet gespeichert: " + Path.GetFileName(sfd.FileName));
            }
        }

        private void TsmiExportGif_Click(object sender, EventArgs e)
        {
            VdEntry entry = CurrentEntry;
            if (entry == null || entry.IsEmpty)
            {
                SetStatus("Diese Richtung enthält keine Frames.");
                return;
            }

            using (var sfd = new SaveFileDialog { Filter = "GIF (*.gif)|*.gif", Title = "Animation als GIF speichern" })
            {
                sfd.FileName = $"{BaseName}_a{_action:00}_d{ViewDirection}.gif";
                if (sfd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                (Size size, Point origin) = GetCanvas(entry);
                using (var gif = AnimatedGif.AnimatedGif.Create(sfd.FileName, _delay))
                {
                    foreach (VdFrame f in entry.Frames)
                    {
                        using (Bitmap bmp = RenderOnCanvas(f, size, origin, _backColor))
                        {
                            if (IsMirroredView)
                            {
                                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            }
                            gif.AddFrame(bmp, _delay);
                        }
                    }
                }
                SetStatus("GIF gespeichert: " + Path.GetFileName(sfd.FileName));
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Drag & drop, keyboard
        // ════════════════════════════════════════════════════════════════

        private void AnimationVDEditorForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void AnimationVDEditorForm_DragDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] files) || files.Length == 0)
            {
                return;
            }

            string vd = files.FirstOrDefault(f => f.EndsWith(".vd", StringComparison.OrdinalIgnoreCase));
            if (vd != null)
            {
                if (ConfirmDiscard())
                {
                    OpenFile(vd);
                }
                return;
            }

            if (EnsureEntry())
            {
                InsertFramesFromFiles(files.Where(f => Regex.IsMatch(f, @"\.(png|bmp|gif|jpe?g|tiff?)$", RegexOptions.IgnoreCase)));
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // do not steal keys from number fields / combo boxes
            bool typing = ActiveControl is NumericUpDown || ActiveControl is TextBoxBase || ActiveControl is ComboBox;

            switch (keyData)
            {
                case Keys.Control | Keys.O:
                    TsbOpen_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.S:
                    SaveFile(false);
                    return true;
                case Keys.Control | Keys.Z when !typing:
                    Undo();
                    return true;
                case Keys.Control | Keys.C when !typing:
                    CopyFrames();
                    return true;
                case Keys.Control | Keys.V when !typing:
                    PasteFrames(false);
                    return true;
                case Keys.Delete when !typing:
                    DeleteSelectedFrames();
                    return true;
                case Keys.Space when !typing && !(ActiveControl is ButtonBase):
                    BtnPlay_Click(this, EventArgs.Empty);
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
