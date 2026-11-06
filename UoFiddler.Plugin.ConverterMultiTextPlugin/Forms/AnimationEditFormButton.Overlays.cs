using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Overlays: floating images above the frames (e.g. put a cage around a bird on every frame at once).
    /// They stay movable / scalable / tintable until they are baked into the paint layer.
    /// </summary>
    public partial class AnimationEditFormButton
    {
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
    }
}
