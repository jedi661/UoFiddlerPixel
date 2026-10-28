/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.MapViewer.Help;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Shows a marked map region like the client (isometric, land textures, statics), checks it for
    /// errors, keeps a protocol and repairs errors (in memory, saved into the output directory).
    /// </summary>
    public partial class MapRegionViewerForm : Form
    {
        private readonly MapDefinition _definition;
        private readonly MapControl _mapControl;
        private readonly ArtPixelCache _art = new ArtPixelCache();
        private readonly IsoRenderSettings _settings = new IsoRenderSettings();
        private readonly RegionData _data;
        private readonly IsoRenderer _renderer;
        private readonly RegionEditSession _session;
        private readonly List<RegionIssue> _issues = new List<RegionIssue>();
        private RegionCell _contextCell;
        private int _contextDrawable = -1;
        private bool _updatingIssues;
        private bool _closeConfirmed;

        /// <summary>false if the session belongs to another window (map editor), which asks for saving</summary>
        private readonly bool _ownsSession = true;

        /// <summary>
        /// Only for the designer
        /// </summary>
        public MapRegionViewerForm()
        {
            InitializeComponent();
        }

        public MapRegionViewerForm(MapDefinition definition, Rectangle area, MapControl mapControl)
            : this(definition, area, mapControl, null)
        {
        }

        /// <summary>
        /// Opens a region with the edit session of another window (shared undo steps, that window asks for saving on close)
        /// </summary>
        public MapRegionViewerForm(MapDefinition definition, Rectangle area, MapControl mapControl, RegionEditSession sharedSession)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _definition = definition;
            _mapControl = mapControl;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _data = new RegionData(definition, area);
                _data.Load(_art);
                _renderer = new IsoRenderer(_data, _art, _settings)
                {
                    MemoryBudget = (long)numericCacheMb.Value * 1024 * 1024
                };
                if (sharedSession != null && sharedSession.Map == _data.Map)
                {
                    _session = sharedSession;
                    _ownsSession = false;
                }
                else
                {
                    _session = new RegionEditSession(_data.Map);
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            Rectangle a = _data.Area;
            Text = $"Region Viewer - {definition.Name}  {a.X},{a.Y} - {a.Right - 1},{a.Bottom - 1}  ({a.Width} x {a.Height})" +
                   (_ownsSession ? string.Empty : "  [Map Editor session]");

            isoView.Renderer = _renderer;
            isoView.Issues = _issues;
            comboBoxIssueFilter.SelectedIndex = 0;
            InitBuild();
            UpdateStatus();
        }

        private void DisposeResources()
        {
            DisposeBuild();
            _renderer?.Dispose();
            pictureBoxPreview?.Image?.Dispose();
        }

        private void OnShown(object sender, EventArgs e)
        {
            if (_data == null)
            {
                return;
            }

            isoView.ZoomToFit();
            RunCheck(false);
            isoView.Focus();
        }

        #region Help

        private string HelpKey()
        {
            TabPage tab = tabControlSide.SelectedTab;
            return tab == tabPageTile ? MapHelpContent.ViewerTile
                : tab == tabPageIssues ? MapHelpContent.ViewerErrors
                : tab == tabPageBuild ? MapHelpContent.ViewerBuild
                : tab == tabPageView ? MapHelpContent.ViewerSettings
                : MapHelpContent.RegionViewer;
        }

        private void OnClickHelp(object sender, EventArgs e) => MapHelpForm.ShowTopic(this, HelpKey());

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            MapHelpForm.ShowTopic(this, HelpKey());
            hevent.Handled = true;
        }

        #endregion

        #region Toolbar: view

        private void OnClickZoomIn(object sender, EventArgs e) => isoView.ZoomIn();

        private void OnClickZoomOut(object sender, EventArgs e) => isoView.ZoomOut();

        private void OnClickFit(object sender, EventArgs e) => isoView.ZoomToFit();

        private void OnZoomChanged(object sender, EventArgs e)
        {
            toolStripLabelZoom.Text = $"{isoView.Zoom * 100:0.#}%";
        }

        private void OnRenderOptionChanged(object sender, EventArgs e)
        {
            if (_renderer == null)
            {
                return;
            }

            _settings.ShowLand = toolStripButtonLand.Checked;
            _settings.ShowStatics = toolStripButtonStatics.Checked;
            _settings.StretchLand = toolStripButtonStretch.Checked;
            _settings.HeightColors = toolStripButtonHeight.Checked;
            _settings.ShowNoDraw = checkBoxNoDraw.Checked;
            _settings.Lighting = checkBoxLighting.Checked;
            _settings.MaxZ = trackBarMaxZ.Value;
            isoView.RefreshRender();
        }

        private void OnOverlayOptionChanged(object sender, EventArgs e)
        {
            isoView.ShowGrid = toolStripButtonGrid.Checked;
            isoView.ShowIssues = toolStripButtonIssueOverlay.Checked;
            isoView.Invalidate();
        }

        private void OnMaxZScroll(object sender, EventArgs e)
        {
            labelMaxZ.Text = trackBarMaxZ.Value >= 127
                ? "Hide statics above z: 127 (all visible)"
                : $"Hide statics above z: {trackBarMaxZ.Value}";
        }

        private void OnMaxZChanged(object sender, EventArgs e)
        {
            if (_settings.MaxZ != trackBarMaxZ.Value)
            {
                OnRenderOptionChanged(sender, e);
            }
        }

        private void OnCacheOptionChanged(object sender, EventArgs e)
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.MemoryBudget = (long)numericCacheMb.Value * 1024 * 1024;
            _renderer.UseDiskCache = checkBoxDiskCache.Checked;
            UpdateStatus();
        }

        private void OnClickClearCache(object sender, EventArgs e)
        {
            isoView.RefreshRender();
            UpdateStatus();
        }

        private void OnRenderProgress(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void OnClickReload(object sender, EventArgs e)
        {
            ReloadRegion();
            RunCheck(false);
        }

        private void OnClickShowOnMap(object sender, EventArgs e)
        {
            if (_mapControl == null || _mapControl.IsDisposed)
            {
                return;
            }

            Point target = isoView.SelectedCell ?? new Point(_data.Area.X + (_data.Area.Width / 2), _data.Area.Y + (_data.Area.Height / 2));
            if (sender == showOnMapToolStripMenuItem && _contextCell != null)
            {
                target = new Point(_contextCell.X, _contextCell.Y);
            }

            if (_mapControl.CurrentDefinition?.Id != _definition.Id)
            {
                _mapControl.SetCurrentMap(_definition, true);
            }

            _mapControl.CenterOn(target.X, target.Y);
        }

        #endregion

        #region Status

        private void UpdateStatus()
        {
            if (_renderer == null)
            {
                return;
            }

            statusLabelRender.Text =
                $"{_data.Area.Width * _data.Area.Height:N0} tiles, {_data.StaticCount:N0} statics | images: {_renderer.CachedChunks} ({_renderer.CachedBytes / (1024 * 1024)} MB)" +
                (_renderer.UseDiskCache ? $", on disk: {_renderer.DiskChunks}" : string.Empty) +
                (isoView.PendingChunks > 0 ? $" | rendering: {isoView.PendingChunks}" : string.Empty);

            labelCacheInfo.Text = $"In memory: {_renderer.CachedChunks} images ({_renderer.CachedBytes / (1024 * 1024)} MB), on disk: {_renderer.DiskChunks}";

            statusLabelChanges.Text = _session.HasUnsavedChanges ? $"{_session.ChangeCount} unsaved change(s)" : string.Empty;
            toolStripButtonSave.Enabled = _session.HasUnsavedChanges;
            toolStripButtonUndo.Enabled = _session.CanUndo;
            toolStripButtonUndoLast.Enabled = _session.CanUndo;
            toolStripButtonUndoLast.ToolTipText = _session.CanUndo
                ? $"Undo: {_session.LastOperationName} (Ctrl+Z)"
                : "Nothing to undo";
            toolStripButtonRedo.Enabled = _session.CanRedo;
            toolStripButtonRedo.ToolTipText = _session.CanRedo ? $"Redo: {_session.NextRedoName} (Ctrl+Y)" : "Nothing to redo";
        }

        private void OnHoverChanged(object sender, EventArgs e)
        {
            OnBuildHoverChanged();
            RegionCell cell = isoView.HoverCell;
            statusLabelCell.Text = cell == null
                ? string.Empty
                : $"X {cell.X}  Y {cell.Y}  Z {cell.LandZ}  land 0x{cell.LandId:X4}  statics {cell.Statics.Count}";
        }

        #endregion

        #region Tile panel

        private void OnCellClicked(object sender, IsoCellEventArgs e)
        {
            if (OnBuildCellClicked())
            {
                return;
            }

            ShowCell(e.Cell, e.Drawable);
        }

        private void ShowCell(RegionCell cell, int drawable)
        {
            listViewTile.BeginUpdate();
            try
            {
                listViewTile.Items.Clear();
                buttonMarkIssue.Enabled = cell != null;

                if (cell == null)
                {
                    labelCellHeader.Text = "Click on a tile in the view";
                    SetPreview(null);
                    return;
                }

                labelCellHeader.Text = $"Position {cell.X}, {cell.Y}   (land z {cell.LandZ}, corners {cell.ZRight}/{cell.ZBottom}/{cell.ZLeft})";

                var landItem = new ListViewItem("Land") { Tag = -1 };
                landItem.SubItems.Add($"0x{cell.LandId:X4}");
                landItem.SubItems.Add(cell.LandName);
                landItem.SubItems.Add(cell.LandZ.ToString());
                landItem.SubItems.Add(string.Empty);
                landItem.SubItems.Add(string.Empty);
                landItem.SubItems.Add(cell.TextureId > 0 ? $"texture 0x{cell.TextureId:X4}" : string.Empty);
                if (!Art.IsValidLand(cell.LandId))
                {
                    landItem.ForeColor = Color.Red;
                }

                listViewTile.Items.Add(landItem);

                int selectStatic = -1;
                if (drawable >= 0 && drawable < _data.Drawables.Count && _data.Drawables[drawable].Kind == DrawableKind.Static)
                {
                    selectStatic = _data.Drawables[drawable].StaticIndex;
                }

                // highest first, like you see them
                foreach (int i in Enumerable.Range(0, cell.Statics.Count).OrderByDescending(i => cell.Statics[i].Z))
                {
                    RegionStatic s = cell.Statics[i];
                    var item = new ListViewItem(s.Pending ? "Static*" : "Static") { Tag = i };
                    item.SubItems.Add($"0x{s.Id:X4}");
                    item.SubItems.Add(s.Name);
                    item.SubItems.Add(s.Z.ToString());
                    item.SubItems.Add(s.Height.ToString());
                    item.SubItems.Add(s.Hue != 0 ? $"0x{s.Hue:X}" : string.Empty);
                    item.SubItems.Add(FlagsText(s.Flags));
                    if (!Art.IsValidStatic(s.Id))
                    {
                        item.ForeColor = Color.Red;
                    }

                    listViewTile.Items.Add(item);
                    if (i == selectStatic)
                    {
                        item.Selected = true;
                    }
                }

                if (listViewTile.SelectedItems.Count == 0)
                {
                    landItem.Selected = true;
                }
            }
            finally
            {
                listViewTile.EndUpdate();
            }

            tabControlSide.SelectedTab = tabPageTile;
        }

        private static string FlagsText(TileFlag flags)
        {
            TileFlag[] shown = { TileFlag.Background, TileFlag.Surface, TileFlag.Wall, TileFlag.Roof, TileFlag.Impassable, TileFlag.Bridge, TileFlag.Foliage, TileFlag.Translucent, TileFlag.Door, TileFlag.Window, TileFlag.LightSource };
            return string.Join(", ", shown.Where(f => (flags & f) != 0));
        }

        private RegionCell SelectedCell => isoView.SelectedCell.HasValue ? _data.GetCell(isoView.SelectedCell.Value.X, isoView.SelectedCell.Value.Y) : null;

        private void OnTileSelectionChanged(object sender, EventArgs e)
        {
            RegionCell cell = SelectedCell;
            if (cell == null || listViewTile.SelectedItems.Count == 0)
            {
                buttonRemoveStatic.Enabled = buttonZUp.Enabled = buttonZDown.Enabled = false;
                return;
            }

            int index = (int)listViewTile.SelectedItems[0].Tag;
            buttonRemoveStatic.Enabled = index >= 0;
            buttonZUp.Enabled = buttonZDown.Enabled = true;

            if (index < 0)
            {
                SetPreview(SafeBitmap(() => Art.GetLand(cell.LandId)));
            }
            else
            {
                RegionStatic s = cell.Statics[index];
                ArtImage image = _art.GetStatic(s.Id, s.Hue);
                SetPreview(image == null ? null : ToBitmap(image));
            }
        }

        private static Bitmap SafeBitmap(Func<Bitmap> loader)
        {
            try
            {
                Bitmap source = loader();
                return source == null ? null : new Bitmap(source);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Bitmap ToBitmap(ArtImage image)
        {
            var bmp = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < image.Height; y++)
            {
                System.Runtime.InteropServices.Marshal.Copy(image.Pixels, y * image.Width, bd.Scan0 + (y * bd.Stride), image.Width);
            }

            bmp.UnlockBits(bd);
            return bmp;
        }

        private void SetPreview(Bitmap bitmap)
        {
            Image old = pictureBoxPreview.Image;
            pictureBoxPreview.Image = bitmap;
            pictureBoxPreview.SizeMode = bitmap != null && (bitmap.Width > pictureBoxPreview.Width || bitmap.Height > pictureBoxPreview.Height)
                ? PictureBoxSizeMode.Zoom
                : PictureBoxSizeMode.CenterImage;
            old?.Dispose();
        }

        private void OnClickRemoveStatic(object sender, EventArgs e)
        {
            RegionCell cell = SelectedCell;
            if (cell == null || listViewTile.SelectedItems.Count == 0 || (int)listViewTile.SelectedItems[0].Tag < 0)
            {
                return;
            }

            RegionStatic s = cell.Statics[(int)listViewTile.SelectedItems[0].Tag];
            if (_session.RemoveStatic(cell.X, cell.Y, s.Id, s.Z, s.Hue))
            {
                AfterEdit($"Removed static 0x{s.Id:X4} at {cell.X},{cell.Y},{s.Z}");
            }
        }

        private void OnClickZUp(object sender, EventArgs e) => ChangeSelectedZ((ModifierKeys & Keys.Shift) != 0 ? 5 : 1);

        private void OnClickZDown(object sender, EventArgs e) => ChangeSelectedZ((ModifierKeys & Keys.Shift) != 0 ? -5 : -1);

        private void ChangeSelectedZ(int delta)
        {
            RegionCell cell = SelectedCell;
            if (cell == null || listViewTile.SelectedItems.Count == 0)
            {
                return;
            }

            int index = (int)listViewTile.SelectedItems[0].Tag;
            if (index < 0)
            {
                int z = Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, cell.LandZ + delta));
                _session.SetLandTile(cell.X, cell.Y, cell.LandId, (sbyte)z);
                AfterEdit($"Land at {cell.X},{cell.Y}: z {cell.LandZ} -> {z}");
            }
            else
            {
                RegionStatic s = cell.Statics[index];
                if (_session.SetStaticZ(cell.X, cell.Y, s.Id, s.Z, s.Hue, s.Z + delta))
                {
                    AfterEdit($"Static 0x{s.Id:X4} at {cell.X},{cell.Y}: z {s.Z} -> {s.Z + delta}");
                }
            }
        }

        private void OnClickMarkIssue(object sender, EventArgs e)
        {
            MarkIssue(SelectedCell);
        }

        private void MarkIssue(RegionCell cell)
        {
            if (cell == null)
            {
                return;
            }

            string note = InputDialog.Show(this, "Mark error", $"Note for position {cell.X}, {cell.Y}:", string.Empty);
            if (note == null)
            {
                return;
            }

            int z = cell.Statics.Count > 0 ? cell.Statics.Max(s => s.Z) : cell.LandZ;
            _issues.Add(new RegionIssue
            {
                Type = RegionIssueType.Manual,
                Severity = RegionIssueSeverity.Warning,
                X = cell.X,
                Y = cell.Y,
                Z = z,
                Description = "Marked by hand",
                Note = note
            });

            FillIssueList();
            isoView.Invalidate();
        }

        #endregion

        #region Context menu

        private void OnContextOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Point canvas = isoView.ScreenToCanvas(isoView.PointToClient(MousePosition));
            _contextDrawable = _renderer.HitTest(canvas);
            _contextCell = _contextDrawable >= 0
                ? _data.GetCell(_data.Drawables[_contextDrawable].X, _data.Drawables[_contextDrawable].Y)
                : _renderer.LandAt(canvas);

            if (_contextCell == null)
            {
                e.Cancel = true;
                return;
            }

            bool isStatic = _contextDrawable >= 0 && _data.Drawables[_contextDrawable].Kind == DrawableKind.Static;
            removeStaticToolStripMenuItem.Enabled = isStatic;
            removeStaticToolStripMenuItem.Text = isStatic
                ? $"Remove static 0x{_data.GetStatic(_data.Drawables[_contextDrawable]).Id:X4}"
                : "Remove this static";
            markErrorToolStripMenuItem.Text = $"Mark error at {_contextCell.X}, {_contextCell.Y}...";
        }

        private void OnClickMarkErrorHere(object sender, EventArgs e)
        {
            MarkIssue(_contextCell);
        }

        private void OnClickRemoveStaticHere(object sender, EventArgs e)
        {
            if (_contextCell == null || _contextDrawable < 0 || _data.Drawables[_contextDrawable].Kind != DrawableKind.Static)
            {
                return;
            }

            RegionStatic s = _data.GetStatic(_data.Drawables[_contextDrawable]);
            if (_session.RemoveStatic(_contextCell.X, _contextCell.Y, s.Id, s.Z, s.Hue))
            {
                AfterEdit($"Removed static 0x{s.Id:X4} at {_contextCell.X},{_contextCell.Y},{s.Z}");
            }
        }

        private void OnClickCopyCoordinates(object sender, EventArgs e)
        {
            if (_contextCell != null)
            {
                Clipboard.SetText($"{_contextCell.X} {_contextCell.Y} {_contextCell.LandZ}");
            }
        }

        #endregion

        #region Edit helpers

        /// <summary>
        /// Reloads the region after a change, updates the map tab and the error list
        /// </summary>
        private void AfterEdit(string logText)
        {
            Point? selected = isoView.SelectedCell;
            ReloadRegion();
            RunCheck(false);

            if (selected.HasValue)
            {
                isoView.SelectedCell = selected;
                ShowCell(_data.GetCell(selected.Value.X, selected.Value.Y), -1);
            }

            statusLabelCell.Text = logText;
            UpdatePlacement();
            UpdateStatus();
        }

        private void ReloadRegion()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _data.Map.ResetCache();
                _data.Load(_art);
                isoView.SelectedDrawable = -1;
                isoView.RefreshRender();

                if (_mapControl != null && !_mapControl.IsDisposed && _mapControl.CurrentMap == _data.Map)
                {
                    _mapControl.RefreshMapAfterEdit();
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        #endregion

        #region Check / fix

        private RegionAnalyzerOptions GetAnalyzerOptions()
        {
            return new RegionAnalyzerOptions
            {
                CheckInvalidIds = checkBoxCheckIds.Checked,
                CheckDuplicates = checkBoxCheckDuplicates.Checked,
                CheckBelowLand = checkBoxCheckBelow.Checked,
                CheckSteepLand = checkBoxCheckSteep.Checked,
                SteepThreshold = (int)numericSteep.Value,
                CheckFloating = checkBoxCheckFloating.Checked,
                FloatingThreshold = (int)numericFloating.Value
            };
        }

        private void OnClickCheck(object sender, EventArgs e)
        {
            RunCheck(true);
        }

        /// <summary>
        /// Runs the analyzer and merges the result into the protocol (notes, ignored entries and
        /// marks by hand are kept, entries that disappeared are set to "Fixed").
        /// </summary>
        private void RunCheck(bool showTab)
        {
            List<RegionIssue> found = RegionAnalyzer.Analyze(_data, GetAnalyzerOptions());
            var merged = new List<RegionIssue>();

            foreach (RegionIssue issue in found)
            {
                RegionIssue old = _issues.FirstOrDefault(i => i.IsSameAs(issue));
                if (old != null)
                {
                    issue.Note = old.Note;
                    issue.Created = old.Created;
                    issue.Status = old.Status == RegionIssueStatus.Ignored ? RegionIssueStatus.Ignored : RegionIssueStatus.Open;
                    _issues.Remove(old);
                }

                merged.Add(issue);
            }

            // everything not found again: manual marks stay as they are, others are solved
            foreach (RegionIssue old in _issues)
            {
                if (old.Type != RegionIssueType.Manual && old.Status == RegionIssueStatus.Open)
                {
                    old.Status = RegionIssueStatus.Fixed;
                }

                merged.Add(old);
            }

            _issues.Clear();
            _issues.AddRange(merged.OrderBy(i => i.Status).ThenByDescending(i => i.Severity).ThenBy(i => i.Y).ThenBy(i => i.X));

            FillIssueList();
            isoView.Invalidate();

            if (showTab)
            {
                tabControlSide.SelectedTab = tabPageIssues;
            }
        }

        private IEnumerable<RegionIssue> FilteredIssues()
        {
            return comboBoxIssueFilter.SelectedIndex switch
            {
                0 => _issues.Where(i => i.Status == RegionIssueStatus.Open),
                2 => _issues.Where(i => i.Severity == RegionIssueSeverity.Error),
                3 => _issues.Where(i => i.Status == RegionIssueStatus.Fixed),
                4 => _issues.Where(i => i.Status == RegionIssueStatus.Ignored),
                5 => _issues.Where(i => i.Type == RegionIssueType.Manual),
                _ => _issues
            };
        }

        private void FillIssueList()
        {
            _updatingIssues = true;
            listViewIssues.BeginUpdate();
            try
            {
                HashSet<RegionIssue> checkedIssues = listViewIssues.CheckedItems.Cast<ListViewItem>().Select(i => (RegionIssue)i.Tag).ToHashSet();
                listViewIssues.Items.Clear();

                foreach (RegionIssue issue in FilteredIssues())
                {
                    var item = new ListViewItem(issue.Status.ToString())
                    {
                        Tag = issue,
                        Checked = checkedIssues.Contains(issue),
                        ToolTipText = issue.CanAutoFix ? "Can be repaired automatically" : "Repair by hand"
                    };
                    item.SubItems.Add(issue.Severity.ToString());
                    item.SubItems.Add(issue.TypeText);
                    item.SubItems.Add(issue.X.ToString());
                    item.SubItems.Add(issue.Y.ToString());
                    item.SubItems.Add(issue.Z.ToString());
                    item.SubItems.Add(issue.TileId >= 0 ? $"0x{issue.TileId:X4}" : string.Empty);
                    item.SubItems.Add(issue.Description);
                    item.SubItems.Add(issue.Note);

                    item.ForeColor = issue.Status != RegionIssueStatus.Open ? SystemColors.GrayText : issue.Severity switch
                    {
                        RegionIssueSeverity.Error => Color.DarkRed,
                        RegionIssueSeverity.Warning => Color.DarkOrange,
                        _ => SystemColors.WindowText
                    };

                    listViewIssues.Items.Add(item);
                }
            }
            finally
            {
                listViewIssues.EndUpdate();
                _updatingIssues = false;
            }

            int open = _issues.Count(i => i.Status == RegionIssueStatus.Open);
            int errors = _issues.Count(i => i.Status == RegionIssueStatus.Open && i.Severity == RegionIssueSeverity.Error);
            int fixedCount = _issues.Count(i => i.Status == RegionIssueStatus.Fixed);
            labelIssueSummary.Text = $"{open} open ({errors} errors), {fixedCount} fixed, {_issues.Count} total";
            tabPageIssues.Text = open > 0 ? $"Errors / Protocol ({open})" : "Errors / Protocol";
        }

        private void OnIssueFilterChanged(object sender, EventArgs e)
        {
            FillIssueList();
        }

        private List<RegionIssue> SelectedIssues()
        {
            return listViewIssues.SelectedItems.Cast<ListViewItem>().Select(i => (RegionIssue)i.Tag).ToList();
        }

        private void OnIssueSelectionChanged(object sender, EventArgs e)
        {
            if (_updatingIssues)
            {
                return;
            }

            isoView.HighlightedIssue = SelectedIssues().FirstOrDefault();
            isoView.Invalidate();
        }

        private void OnIssueDoubleClick(object sender, EventArgs e)
        {
            RegionIssue issue = SelectedIssues().FirstOrDefault();
            if (issue == null)
            {
                return;
            }

            isoView.HighlightedIssue = issue;
            isoView.SelectedCell = new Point(issue.X, issue.Y);
            isoView.SelectedDrawable = -1;
            if (isoView.Zoom < 1)
            {
                isoView.SetZoom(1);
            }

            isoView.CenterOnCell(issue.X, issue.Y);
            ShowCell(_data.GetCell(issue.X, issue.Y), -1);
            tabControlSide.SelectedTab = tabPageIssues;
        }

        private void OnClickCheckFixable(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewIssues.Items)
            {
                var issue = (RegionIssue)item.Tag;
                item.Checked = issue.Status == RegionIssueStatus.Open && issue.CanAutoFix;
            }
        }

        private void OnClickFixChecked(object sender, EventArgs e)
        {
            List<RegionIssue> issues = listViewIssues.CheckedItems.Cast<ListViewItem>()
                .Select(i => (RegionIssue)i.Tag)
                .Where(i => i.Status == RegionIssueStatus.Open && i.CanAutoFix)
                .ToList();

            if (issues.Count == 0)
            {
                MessageBox.Show("Please check the entries to repair (only open entries that can be repaired automatically).",
                    "Region Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FixIssues(issues);
        }

        private void OnClickFixAll(object sender, EventArgs e)
        {
            List<RegionIssue> issues = _issues.Where(i => i.Status == RegionIssueStatus.Open && i.CanAutoFix && i.FixByDefault).ToList();
            if (issues.Count == 0)
            {
                MessageBox.Show("No open errors with a safe automatic repair.", "Region Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string summary = string.Join("\n", issues.GroupBy(i => i.TypeText).Select(g => $"  {g.Key}: {g.Count()}"));
            if (MessageBox.Show($"Repair {issues.Count} error(s)?\n\n{summary}\n\nThe changes are made in memory, use \"Save\" to write the files.",
                    "Region Viewer", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            FixIssues(issues);
        }

        private void FixIssues(List<RegionIssue> issues)
        {
            int done = 0;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // one undo step for all repairs
                using (_session.Batch($"Fix {issues.Count} error(s)"))
                {
                    foreach (RegionIssue issue in issues)
                    {
                        if (_session.Fix(issue, _data))
                        {
                            issue.Note = string.IsNullOrEmpty(issue.Note)
                                ? $"auto fixed {DateTime.Now:yyyy-MM-dd HH:mm}"
                                : $"{issue.Note}; auto fixed {DateTime.Now:yyyy-MM-dd HH:mm}";
                            done++;
                        }
                    }
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            AfterEdit($"{done} of {issues.Count} error(s) repaired");
        }

        private void SetStatus(RegionIssueStatus status)
        {
            foreach (RegionIssue issue in SelectedIssues())
            {
                issue.Status = status;
            }

            FillIssueList();
            isoView.Invalidate();
        }

        private void OnClickIgnore(object sender, EventArgs e) => SetStatus(RegionIssueStatus.Ignored);

        private void OnClickReopen(object sender, EventArgs e) => SetStatus(RegionIssueStatus.Open);

        private void OnClickNote(object sender, EventArgs e)
        {
            List<RegionIssue> selected = SelectedIssues();
            if (selected.Count == 0)
            {
                return;
            }

            string note = InputDialog.Show(this, "Note", $"Note for {selected.Count} entr{(selected.Count == 1 ? "y" : "ies")}:", selected[0].Note);
            if (note == null)
            {
                return;
            }

            foreach (RegionIssue issue in selected)
            {
                issue.Note = note;
            }

            FillIssueList();
        }

        private void OnClickDeleteIssue(object sender, EventArgs e)
        {
            List<RegionIssue> selected = SelectedIssues();
            if (selected.Count == 0)
            {
                return;
            }

            foreach (RegionIssue issue in selected)
            {
                _issues.Remove(issue);
            }

            isoView.HighlightedIssue = null;
            FillIssueList();
            isoView.Invalidate();
        }

        #endregion

        #region Protocol files

        private string DefaultProtocolName(string extension)
        {
            string name = _definition.Name;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return $"RegionProtocol_{name}_{_data.Area.X}_{_data.Area.Y}_{_data.Area.Width}x{_data.Area.Height}.{extension}";
        }

        private void OnClickExportCsv(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = DefaultProtocolName("csv"),
                InitialDirectory = Options.OutputPath
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                RegionIssue.ExportCsv(dialog.FileName, _definition.Name, _issues);
                statusLabelCell.Text = $"Protocol exported to {dialog.FileName}";
            }
        }

        private void OnClickSaveProtocol(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog
            {
                Filter = "Region protocol (*.xml)|*.xml",
                FileName = DefaultProtocolName("xml"),
                InitialDirectory = Options.OutputPath
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                RegionIssue.SaveXml(dialog.FileName, _definition.Id, _definition.Name, _data.Area, _issues);
                statusLabelCell.Text = $"Protocol saved to {dialog.FileName}";
            }
        }

        private void OnClickLoadProtocol(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = "Region protocol (*.xml)|*.xml",
                InitialDirectory = Options.OutputPath
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<RegionIssue> loaded;
                int mapId;
                try
                {
                    loaded = RegionIssue.LoadXml(dialog.FileName, out mapId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The protocol could not be read:\n{ex.Message}", "Region Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (mapId >= 0 && mapId != _definition.Id &&
                    MessageBox.Show($"The protocol belongs to map {MapRegistry.GetName(mapId)}. Load it anyway?", "Region Viewer",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                int added = 0, outside = 0;
                foreach (RegionIssue issue in loaded)
                {
                    if (!_data.Contains(issue.X, issue.Y))
                    {
                        outside++;
                        continue;
                    }

                    RegionIssue existing = _issues.FirstOrDefault(i => i.IsSameAs(issue));
                    if (existing != null)
                    {
                        existing.Note = issue.Note;
                        if (issue.Status == RegionIssueStatus.Ignored)
                        {
                            existing.Status = RegionIssueStatus.Ignored;
                        }

                        continue;
                    }

                    _issues.Add(issue);
                    added++;
                }

                FillIssueList();
                isoView.Invalidate();
                statusLabelCell.Text = $"{added} entries loaded" + (outside > 0 ? $", {outside} outside of this region skipped" : string.Empty);
            }
        }

        #endregion

        #region Save / undo / close

        private void OnClickUndoLast(object sender, EventArgs e)
        {
            string name = _session.UndoLast();
            if (name != null)
            {
                AfterEdit($"Undone: {name}");
            }
        }

        private void OnClickRedo(object sender, EventArgs e)
        {
            string name = _session.Redo();
            if (name != null)
            {
                AfterEdit($"Redone: {name}");
            }
        }

        private void OnClickUndo(object sender, EventArgs e)
        {
            if (!_session.CanUndo)
            {
                return;
            }

            if (MessageBox.Show($"Undo all {_session.OperationCount} change(s) made in this window?", "Region Viewer", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            _session.UndoAll();
            AfterEdit("All changes undone");
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            SaveChanges();
        }

        /// <summary>
        /// Asks for the target directory and the backup, then writes the files. Returns false if canceled or failed.
        /// </summary>
        private bool SaveChanges()
        {
            using (var dialog = new MapSaveDialog(_data.Map, _definition.Name, _session.ChangeCount))
            {
                DialogResult result = dialog.ShowDialog(this);
                if (result == DialogResult.Abort)
                {
                    // a backup was restored in the dialog
                    _session.MarkSaved(false);
                    AfterEdit("Backup restored");
                    return false;
                }

                if (result != DialogResult.OK)
                {
                    return false;
                }

                string directory = dialog.TargetDirectory;
                MapSaveOptions options = dialog.SaveOptions;
                TileMatrix tiles = _data.Map.Tiles;
                bool allWritten = (options.IncludeLand || !tiles.HasModifiedLand) && (options.IncludeStatics || !tiles.HasStaticOverrides);
                bool replacesLoaded = MapSaveService.IsSameDirectory(directory, MapSaveService.GetSourceDirectory(_data.Map)) &&
                                      options.OutputIndex == _data.Map.FileIndex;

                List<string> files;
                MapBackupInfo backup;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    files = MapSaveService.Save(_data.Map, _definition.Name, directory, dialog.CreateBackup, options, out backup);
                    if (dialog.SaveProtocol)
                    {
                        string protocol = Path.Combine(directory, DefaultProtocolName("xml"));
                        RegionIssue.SaveXml(protocol, _definition.Id, _definition.Name, _data.Area, _issues);
                        files.Add(protocol);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Saving failed:\n{ex.Message}\n\nThe changes are still in memory.", "Region Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

                // the original files were replaced and read again: the old undo steps do not fit anymore
                if (allWritten || replacesLoaded)
                {
                    _session.MarkSaved(!replacesLoaded);
                }
                if (replacesLoaded)
                {
                    ReloadRegion();
                    RunCheck(false);
                }

                UpdateStatus();
                MessageBox.Show("Saved:\n" + string.Join("\n", files.Select(Path.GetFileName)) + $"\n\nin {directory}" +
                                (backup != null ? $"\n\nBackup: {backup.Directory}" : string.Empty),
                    "Region Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
        }

        private void OnClickBackups(object sender, EventArgs e)
        {
            using (var form = new MapBackupForm(_definition.Name, new[] { Options.OutputPath, MapSaveService.GetSourceDirectory(_data.Map) }))
            {
                form.ShowDialog(this);
                if (form.Restored)
                {
                    _session.MarkSaved(false);
                    AfterEdit("Backup restored");
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Z) && !(ActiveControl is TextBoxBase))
            {
                OnClickUndoLast(this, EventArgs.Empty);
                return true;
            }

            if ((keyData == (Keys.Control | Keys.Y) || keyData == (Keys.Control | Keys.Shift | Keys.Z)) && !(ActiveControl is TextBoxBase))
            {
                OnClickRedo(this, EventArgs.Empty);
                return true;
            }

            if (HandleBuildKey(keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_session == null || _closeConfirmed || !_session.HasUnsavedChanges || !_ownsSession)
            {
                return;
            }

            DialogResult result = MessageBox.Show(
                $"There are {_session.ChangeCount} unsaved change(s).\n\nYes = save (choose the directory and the backup)\nNo = undo the changes\nCancel = keep the window open",
                "Region Viewer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            switch (result)
            {
                case DialogResult.Yes:
                    if (!SaveChanges())
                    {
                        e.Cancel = true;
                        return;
                    }

                    break;
                case DialogResult.No:
                    _session.UndoAll();
                    _data.Map.ResetCache();
                    if (_mapControl != null && !_mapControl.IsDisposed)
                    {
                        _mapControl.RefreshMapAfterEdit();
                    }

                    break;
                default:
                    e.Cancel = true;
                    return;
            }

            _closeConfirmed = true;
        }

        #endregion
    }
}
