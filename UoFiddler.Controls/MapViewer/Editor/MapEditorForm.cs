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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.MapViewer.Help;
using UoFiddler.Controls.MapViewer.Build;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Map editor: copies an area (land and/or statics) of a map or an external map and pastes it on any map
    /// at a position chosen in the iso view. Height adaption at the border, live preview, undo, save with
    /// own directory / file number.
    /// </summary>
    public partial class MapEditorForm : Form
    {
        private const string PreviewName = "Paste preview";
        private const string PasteName = "Paste clip";

        private readonly MapControl _mapControl;
        private readonly ArtPixelCache _art = new ArtPixelCache();
        private readonly IsoRenderSettings _settings = new IsoRenderSettings();
        private readonly Dictionary<Map, RegionEditSession> _sessions = new Dictionary<Map, RegionEditSession>();
        private readonly HashSet<ushort> _excludedIds = new HashSet<ushort>();

        private MapDefinition _sourceDef;
        private MapDefinition _targetDef;
        private Rectangle _initialArea;

        private MapClip _clip;
        private MultiImage _ghost;
        private bool _ghostDirty = true;

        private RegionData _data;
        private IsoRenderer _renderer;
        private int _knownOperations = -1;

        private bool _pasting;
        private bool _locked;
        private int _pasteX;
        private int _pasteY;
        private ClipPasteResult _result;
        private bool _previewApplied;
        private int _previewOperations;

        private bool _syncing;
        private bool _fillingIds;
        private bool _filling;

        /// <summary>Only for the designer</summary>
        public MapEditorForm()
        {
            InitializeComponent();
        }

        /// <param name="mapControl">map tab (for its mark and to refresh it), can be null</param>
        /// <param name="source">source map (null = current map of the map tab)</param>
        /// <param name="area">area to copy (empty = none)</param>
        public MapEditorForm(MapControl mapControl, MapDefinition source, Rectangle area)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _mapControl = mapControl;
            _sourceDef = source ?? mapControl?.CurrentDefinition ?? MapRegistry.Definitions.FirstOrDefault();
            _targetDef = _sourceDef;
            _initialArea = area;

            comboBoxHeightMode.SelectedIndex = 1;
            comboBoxRemove.SelectedIndex = 0;
            _settings.ShowStatics = true;

            FillMapCombos();
            MapRegistry.Changed += OnRegistryChanged;
            InitTools();
        }

        private void DisposeResources()
        {
            MapRegistry.Changed -= OnRegistryChanged;
            DisposeTools();
            previewTimer.Stop();
            _ghost?.Dispose();
            _ghost = null;
            _renderer?.Dispose();
            _renderer = null;
            pictureBoxClip.Image?.Dispose();
        }

        private void OnShown(object sender, EventArgs e)
        {
            if (!_initialArea.IsEmpty)
            {
                SetSourceArea(_initialArea);
                mapOverviewSource.ShowArea(_initialArea);
                CopyClip();
                LoadWorkingArea(_initialArea.X + (_initialArea.Width / 2), _initialArea.Y + (_initialArea.Height / 2));
            }
            else
            {
                SetSourceArea(new Rectangle(0, 0, 32, 32));
            }

            UpdateState();
        }

        #region Maps

        private sealed class MapItem
        {
            public MapItem(MapDefinition def) => Definition = def;

            public MapDefinition Definition { get; }

            public override string ToString()
            {
                return MapRegistry.IsTemporary(Definition)
                    ? $"{Definition.Name}  (external)"
                    : $"{Definition.Name}  (map{Definition.FileIndex})";
            }
        }

        private void FillMapCombos()
        {
            _filling = true;
            try
            {
                var items = MapRegistry.Definitions.Where(d => d.Visible || d == _sourceDef || d == _targetDef)
                    .Concat(MapRegistry.TemporaryDefinitions)
                    .Distinct()
                    .Select(d => new MapItem(d))
                    .ToArray();

                foreach ((ComboBox combo, MapDefinition selected) in new[] { (comboBoxSourceMap, _sourceDef), (comboBoxTargetMap, _targetDef) })
                {
                    combo.BeginUpdate();
                    combo.Items.Clear();
                    combo.Items.AddRange(items.Cast<object>().ToArray());
                    combo.SelectedIndex = Array.FindIndex(items, i => i.Definition == selected);
                    combo.EndUpdate();
                }
            }
            finally
            {
                _filling = false;
            }

            ShowSourceMap();
            ShowTargetMap();
        }

        private void OnRegistryChanged(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnRegistryChanged(sender, e)));
                return;
            }

            FillMapCombos();
        }

        private static Map TryGetMap(MapDefinition def, out string error)
        {
            error = null;
            if (def == null)
            {
                error = "No map selected.";
                return null;
            }

            try
            {
                Map map = def.GetMap();
                if (map?.Tiles.MapPath == null)
                {
                    error = $"The files of \"{def.Name}\" (map{def.FileIndex}) were not found.";
                    return null;
                }

                return map;
            }
            catch (Exception ex)
            {
                error = $"\"{def.Name}\" can not be opened: {ex.Message}";
                return null;
            }
        }

        private RegionEditSession GetSession(Map map)
        {
            if (!_sessions.TryGetValue(map, out RegionEditSession session))
            {
                session = new RegionEditSession(map);
                _sessions[map] = session;
            }

            return session;
        }

        private RegionEditSession TargetSession => _data == null ? null : GetSession(_data.Map);

        private void ShowSourceMap()
        {
            Map map = TryGetMap(_sourceDef, out string error);
            if (mapOverviewSource.Map == map && map != null)
            {
                return;
            }

            mapOverviewSource.Map = map;
            if (map == null && error != null && _sourceDef != null)
            {
                statusLabelInfo.Text = error;
            }

            UpdateSourceLimits();
            OnSourceNumericChanged(this, EventArgs.Empty);
        }

        private void ShowTargetMap()
        {
            Map map = TryGetMap(_targetDef, out string error);
            if (mapOverviewTarget.Map == map && map != null)
            {
                return;
            }

            mapOverviewTarget.Map = map;
            if (map == null && error != null && _targetDef != null)
            {
                statusLabelInfo.Text = error;
            }

            if (_data != null)
            {
                mapOverviewTarget.Highlight = _data.Area;
            }
        }

        private void OnSourceMapChanged(object sender, EventArgs e)
        {
            if (_filling || !(comboBoxSourceMap.SelectedItem is MapItem item) || item.Definition == _sourceDef)
            {
                return;
            }

            _sourceDef = item.Definition;
            ShowSourceMap();
        }

        private void OnTargetMapChanged(object sender, EventArgs e)
        {
            if (_filling || !(comboBoxTargetMap.SelectedItem is MapItem item) || item.Definition == _targetDef)
            {
                return;
            }

            StopPasting();
            _generator?.Close();
            _targetDef = item.Definition;
            UnloadWorkingArea();
            ShowTargetMap();
            Map map = mapOverviewTarget.Map;
            if (map != null)
            {
                LoadWorkingArea(map.Width / 2, map.Height / 2);
                mapOverviewTarget.CenterOn(map.Width / 2, map.Height / 2);
            }

            UpdateState();
        }

        private void OnClickSourceExternal(object sender, EventArgs e)
        {
            MapDefinition def = OpenExternal();
            if (def != null)
            {
                _sourceDef = def;
                FillMapCombos();
            }
        }

        private void OnClickTargetExternal(object sender, EventArgs e)
        {
            MapDefinition def = OpenExternal();
            if (def != null)
            {
                StopPasting();
                _generator?.Close();
                UnloadWorkingArea();
                _targetDef = def;
                FillMapCombos();
                Map map = mapOverviewTarget.Map;
                if (map != null)
                {
                    LoadWorkingArea(map.Width / 2, map.Height / 2);
                }

                UpdateState();
            }
        }

        private MapDefinition OpenExternal()
        {
            string start = MapRegistry.TemporaryDefinitions.LastOrDefault()?.CustomPath ?? Options.OutputPath;
            using (var dialog = new ExternalMapDialog(start))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return null;
                }

                Map map = TryGetMap(dialog.Definition, out string error);
                if (map == null)
                {
                    MessageBox.Show(error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                return dialog.Definition;
            }
        }

        private void OnOverviewStaticsChanged(object sender, EventArgs e)
        {
            mapOverviewSource.ShowStatics = checkBoxSourceStatics.Checked;
            mapOverviewTarget.ShowStatics = checkBoxSourceStatics.Checked;
            mapOverviewSource.Invalidate();
            mapOverviewTarget.Invalidate();
        }

        private void OnOverviewHover(object sender, MapPositionEventArgs e)
        {
            statusLabelCell.Text = $"Map X {e.X}  Y {e.Y}";
        }

        #endregion

        #region Source / copy

        private void UpdateSourceLimits()
        {
            Map map = mapOverviewSource.Map;
            if (map == null)
            {
                return;
            }

            _syncing = true;
            try
            {
                numericSourceX.Maximum = map.Width - 1;
                numericSourceY.Maximum = map.Height - 1;
            }
            finally
            {
                _syncing = false;
            }
        }

        private void SetSourceArea(Rectangle area)
        {
            _syncing = true;
            try
            {
                numericSourceX.Value = Clamp(area.X, numericSourceX);
                numericSourceY.Value = Clamp(area.Y, numericSourceY);
                numericSourceW.Value = Clamp(area.Width, numericSourceW);
                numericSourceH.Value = Clamp(area.Height, numericSourceH);
            }
            finally
            {
                _syncing = false;
            }

            OnSourceNumericChanged(this, EventArgs.Empty);
        }

        private static decimal Clamp(int value, NumericUpDown n)
        {
            return Math.Max(n.Minimum, Math.Min(n.Maximum, value));
        }

        private Rectangle SourceArea => new Rectangle((int)numericSourceX.Value, (int)numericSourceY.Value, (int)numericSourceW.Value, (int)numericSourceH.Value);

        private void OnSourceNumericChanged(object sender, EventArgs e)
        {
            if (_syncing)
            {
                return;
            }

            mapOverviewSource.Selection = SourceArea;
            mapOverviewSource.Invalidate();
        }

        private void OnSourceSelectionChanged(object sender, EventArgs e)
        {
            if (mapOverviewSource.Selection.HasValue)
            {
                Rectangle r = mapOverviewSource.Selection.Value;
                _syncing = true;
                try
                {
                    numericSourceX.Value = Clamp(r.X, numericSourceX);
                    numericSourceY.Value = Clamp(r.Y, numericSourceY);
                    numericSourceW.Value = Clamp(r.Width, numericSourceW);
                    numericSourceH.Value = Clamp(r.Height, numericSourceH);
                }
                finally
                {
                    _syncing = false;
                }

                statusLabelInfo.Text = $"Marked: {r.X},{r.Y} - {r.Right - 1},{r.Bottom - 1}  ({r.Width} x {r.Height})" +
                                       (r.Width > numericSourceW.Maximum || r.Height > numericSourceH.Maximum ? $"  (max. {numericSourceW.Maximum} x {numericSourceH.Maximum})" : string.Empty);
            }
        }

        private void OnClickUseMapMark(object sender, EventArgs e)
        {
            if (_mapControl == null || _mapControl.IsDisposed || _mapControl.MarkedRegion.IsEmpty)
            {
                MessageBox.Show("There is no marked area in the map tab.\nUse \"Mark\" there or mark the area here on the map.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MapDefinition def = _mapControl.CurrentDefinition;
            if (def != null && def != _sourceDef)
            {
                _sourceDef = def;
                FillMapCombos();
            }

            Rectangle area = _mapControl.MarkedRegion;
            SetSourceArea(area);
            mapOverviewSource.ShowArea(area);
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            CopyClip();
            if (_clip != null)
            {
                tabControlSide.SelectedTab = tabPageClip;
            }
        }

        private void CopyClip()
        {
            Map map = TryGetMap(_sourceDef, out string error);
            if (map == null)
            {
                MessageBox.Show(error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!checkBoxCopyLand.Checked && !checkBoxCopyStatics.Checked)
            {
                MessageBox.Show("Check land and/or statics.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            StopPasting();
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _sessions.TryGetValue(map, out RegionEditSession session);
                MapClip clip = MapClip.FromMap(map, SourceArea, session, _sourceDef.Name);
                if (!checkBoxCopyStatics.Checked)
                {
                    clip.Statics.Clear();
                }

                ClipHasLand = checkBoxCopyLand.Checked;
                checkBoxPasteLand.Checked = ClipHasLand;
                checkBoxPasteStatics.Checked = clip.Statics.Count > 0;
                _excludedIds.Clear();
                SetClip(clip);
                statusLabelInfo.Text = $"Copied: {clip}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The area can not be copied:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>false if only the statics were copied (the land of the clip is not used)</summary>
        private bool ClipHasLand { get; set; } = true;

        #endregion

        #region Clip

        private void SetClip(MapClip clip)
        {
            _clip = clip;
            _ghostDirty = true;
            FillClipIds();
            UpdateClipInfo();
            UpdateGhostThumbnail();
            UpdatePaste();
            UpdateState();
        }

        private void UpdateClipInfo()
        {
            if (_clip == null)
            {
                labelClipInfo.Text = "No clip. Mark an area in \"1. Source\" and copy it.";
                return;
            }

            Rectangle a = _clip.SourceArea;
            labelClipInfo.Text = $"From \"{_clip.SourceName}\" {a.X},{a.Y}  -  {_clip.Width} x {_clip.Height} tiles\n" +
                                 (ClipHasLand ? $"land z {_clip.MinLandZ}..{_clip.MaxLandZ}, " : "no land, ") +
                                 $"{_clip.Statics.Count} statics ({_clip.Statics.Count(s => IsIncluded(s))} used)";
        }

        private bool IsIncluded(ClipStatic s)
        {
            return !_excludedIds.Contains(s.Id) && s.Z >= numericMinZ.Value && s.Z <= numericMaxZ.Value;
        }

        private void FillClipIds()
        {
            _fillingIds = true;
            listViewClipIds.BeginUpdate();
            try
            {
                listViewClipIds.Items.Clear();
                if (_clip == null)
                {
                    return;
                }

                foreach (IGrouping<ushort, ClipStatic> group in _clip.Statics.GroupBy(s => s.Id).OrderByDescending(g => g.Count()).Take(5000))
                {
                    ushort id = group.Key;
                    var item = new ListViewItem($"0x{id:X4}") { Tag = id, Checked = !_excludedIds.Contains(id) };
                    item.SubItems.Add(id < TileData.ItemTable.Length ? TileData.ItemTable[id].Name : string.Empty);
                    item.SubItems.Add(group.Count().ToString(CultureInfo.InvariantCulture));
                    listViewClipIds.Items.Add(item);
                }
            }
            finally
            {
                listViewClipIds.EndUpdate();
                _fillingIds = false;
            }
        }

        private void OnClipIdChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_fillingIds || !(e.Item.Tag is ushort id))
            {
                return;
            }

            if (e.Item.Checked)
            {
                _excludedIds.Remove(id);
            }
            else
            {
                _excludedIds.Add(id);
            }

            ClipFilterChanged();
        }

        private void OnClipFilterChanged(object sender, EventArgs e)
        {
            ClipFilterChanged();
        }

        private void ClipFilterChanged()
        {
            _ghostDirty = true;
            UpdateClipInfo();
            previewTimer.Stop();
            previewTimer.Start();
        }

        private void SetAllIds(bool check)
        {
            _fillingIds = true;
            listViewClipIds.BeginUpdate();
            try
            {
                foreach (ListViewItem item in listViewClipIds.Items)
                {
                    item.Checked = check;
                }
            }
            finally
            {
                listViewClipIds.EndUpdate();
                _fillingIds = false;
            }

            _excludedIds.Clear();
            if (!check && _clip != null)
            {
                _excludedIds.UnionWith(_clip.Statics.Select(s => s.Id));
            }

            ClipFilterChanged();
        }

        private void OnClickCheckAll(object sender, EventArgs e) => SetAllIds(true);

        private void OnClickCheckNone(object sender, EventArgs e) => SetAllIds(false);

        private void OnClickRemoveUnchecked(object sender, EventArgs e)
        {
            if (_clip == null)
            {
                return;
            }

            int before = _clip.Statics.Count;
            _clip.Statics.RemoveAll(s => !IsIncluded(s));
            _excludedIds.Clear();
            _syncing = true;
            numericMinZ.Value = numericMinZ.Minimum;
            numericMaxZ.Value = numericMaxZ.Maximum;
            _syncing = false;
            statusLabelInfo.Text = $"{before - _clip.Statics.Count} statics deleted from the clip.";
            SetClip(_clip);
        }

        private void OnClickAddStatic(object sender, EventArgs e)
        {
            if (_clip == null)
            {
                return;
            }

            string text = InputDialog.Show(this, "Add static to the clip",
                $"ID X Y Z [Hue]   (X 0..{_clip.Width - 1}, Y 0..{_clip.Height - 1} inside the clip, ID decimal or 0x hex, empty Z = ground)",
                "0x0001 0 0", false);
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            string[] parts = text.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3 || !TryParseNumber(parts[0], out int id) || !TryParseNumber(parts[1], out int x) || !TryParseNumber(parts[2], out int y))
            {
                MessageBox.Show("Please enter at least: ID X Y", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (x < 0 || y < 0 || x >= _clip.Width || y >= _clip.Height || id < 0 || id > ushort.MaxValue)
            {
                MessageBox.Show("The position is outside of the clip or the ID is invalid.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int z = _clip.GetLandZ(x, y);
            int hue = 0;
            if (parts.Length > 3 && !TryParseNumber(parts[3], out z))
            {
                return;
            }

            if (parts.Length > 4 && !TryParseNumber(parts[4], out hue))
            {
                return;
            }

            _clip.Statics.Add(new ClipStatic(x, y, (ushort)id, (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, z)), (short)hue));
            checkBoxPasteStatics.Checked = true;
            SetClip(_clip);
        }

        private static bool TryParseNumber(string text, out int value)
        {
            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private void OnClickExportClip(object sender, EventArgs e)
        {
            if (_clip == null || _clip.Statics.Count == 0)
            {
                return;
            }

            var template = new MultiTemplate { Name = $"Clip {_clip.SourceName} {_clip.SourceArea.X},{_clip.SourceArea.Y}", Source = "Map editor" };
            foreach (ClipStatic s in _clip.Statics.Where(IsIncluded))
            {
                template.Components.Add(new MultiComponent(s.Id, s.X, s.Y, s.Z, (ushort)s.Hue));
            }

            MultiExportFormat[] formats = (MultiExportFormat[])Enum.GetValues(typeof(MultiExportFormat));
            using (var dialog = new SaveFileDialog
            {
                Filter = string.Join("|", formats.Select(MultiFormats.ExportFilter)),
                FileName = "clip",
                InitialDirectory = Options.OutputPath
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    MultiFormats.Export(template, dialog.FileName, formats[Math.Max(0, dialog.FilterIndex - 1)]);
                    statusLabelInfo.Text = $"Exported: {dialog.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnClickRotate(object sender, EventArgs e) => TransformClip(c => c.Rotate90());

        private void OnClickMirrorX(object sender, EventArgs e) => TransformClip(c => c.MirrorX());

        private void OnClickMirrorY(object sender, EventArgs e) => TransformClip(c => c.MirrorY());

        private void TransformClip(Func<MapClip, MapClip> transform)
        {
            if (_clip == null)
            {
                return;
            }

            RemovePreview(false);
            SetClip(transform(_clip));
            statusLabelInfo.Text = "Positions turned/mirrored. Directional items (walls, doors, stairs) keep their graphic.";
        }

        private void OnClickClearClip(object sender, EventArgs e)
        {
            StopPasting();
            _excludedIds.Clear();
            SetClip(null);
        }

        private MultiImage GetGhost()
        {
            if (_ghostDirty)
            {
                _ghost?.Dispose();
                _ghost = ClipPreview.Render(_clip, _art, CurrentOptions());
                _ghostDirty = false;
            }

            return _ghost;
        }

        private void UpdateGhostThumbnail()
        {
            Image old = pictureBoxClip.Image;
            pictureBoxClip.Image = _clip == null
                ? null
                : ClipPreview.Thumbnail(GetGhost(), pictureBoxClip.ClientSize, pictureBoxClip.BackColor,
                    _clip.Statics.Count == 0 && !ClipHasLand ? "empty" : "too large for a preview (the outline is shown)");
            old?.Dispose();
        }

        #endregion

        #region Target working area

        private void OnTargetPositionClicked(object sender, MapPositionEventArgs e)
        {
            StopPasting();
            LoadWorkingArea(e.X, e.Y);
            UpdateState();
        }

        private void UnloadWorkingArea()
        {
            // points, areas and the generator belong to the old working area / map
            _linePoints.Clear();
            _generatorArea = null;
            _shapeStart = null;
            isoView.SelectedArea = null;
            isoView.ObstacleCells = null;
            isoView.Renderer = null;
            _renderer?.Dispose();
            _renderer = null;
            _data = null;
            mapOverviewTarget.Highlight = null;
            mapOverviewTarget.Invalidate();
        }

        private void LoadWorkingArea(int centerX, int centerY)
        {
            Map map = TryGetMap(_targetDef, out string error);
            if (map == null)
            {
                statusLabelInfo.Text = error;
                return;
            }

            int size = (int)numericWorkSize.Value;
            if (_clip != null)
            {
                size = Math.Max(size, Math.Min(600, Math.Max(_clip.Width, _clip.Height) + 32));
            }

            var area = new Rectangle(centerX - (size / 2), centerY - (size / 2), size, size);
            area.X = Math.Max(0, Math.Min(map.Width - Math.Min(size, map.Width), area.X));
            area.Y = Math.Max(0, Math.Min(map.Height - Math.Min(size, map.Height), area.Y));

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                RemovePreview(false);
                UnloadWorkingArea();
                _data = new RegionData(_targetDef, area);
                _data.Load(_art);
                _renderer = new IsoRenderer(_data, _art, _settings);
                isoView.Renderer = _renderer;
                isoView.ZoomToFit();
                _knownOperations = TargetSession.OperationCount;
                mapOverviewTarget.Highlight = _data.Area;
                mapOverviewTarget.Invalidate();
                Text = $"Map Editor - target: {_targetDef.Name}  {_data.Area.X},{_data.Area.Y} ({_data.Area.Width} x {_data.Area.Height})";
            }
            catch (Exception ex)
            {
                UnloadWorkingArea();
                statusLabelInfo.Text = $"The area can not be loaded: {ex.Message}";
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>Reads the working area again after a change</summary>
        private void ReloadView(bool wholeMapChanged = false)
        {
            if (_data == null)
            {
                return;
            }

            _data.Map.ResetCache();
            _data.Load(_art);
            isoView.SelectedDrawable = -1;
            isoView.RefreshRender();
            _knownOperations = TargetSession.OperationCount;

            // only the working area can be changed by the editor (backup/reload: everything)
            Rectangle? changed = TargetSession.TakeChangedArea();
            if (wholeMapChanged)
            {
                changed = null;
            }
            else if (!changed.HasValue)
            {
                changed = Rectangle.Empty;
            }
            if (changed != Rectangle.Empty)
            {
                mapOverviewTarget.RefreshMap(changed);
                if (mapOverviewSource.Map == _data.Map)
                {
                    mapOverviewSource.RefreshMap(changed);
                }
            }

            if (_mapControl != null && !_mapControl.IsDisposed && _mapControl.CurrentMap == _data.Map)
            {
                _mapControl.RefreshMapAfterEdit();
            }

            UpdatePasteOverlay();
        }

        private void OnActivated(object sender, EventArgs e)
        {
            // changes made in a Region Viewer with the same session
            if (_data != null && TargetSession.OperationCount != _knownOperations)
            {
                ReloadView();
                UpdateState();
            }
        }

        #endregion

        #region Help

        private string HelpKey()
        {
            TabPage tab = tabControlSide.SelectedTab;
            return tab == tabPageSource ? MapHelpContent.EditorSource
                : tab == tabPageClip ? MapHelpContent.EditorClip
                : tab == tabPageTarget ? MapHelpContent.EditorTarget
                : tab == _tabTools ? MapHelpContent.EditorTools
                : MapHelpContent.Editor;
        }

        private void OnClickHelp(object sender, EventArgs e) => MapHelpForm.ShowTopic(this, HelpKey());

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            MapHelpForm.ShowTopic(this, HelpKey());
            hevent.Handled = true;
        }

        #endregion

        #region View

        private void OnClickZoomIn(object sender, EventArgs e) => isoView.ZoomIn();

        private void OnClickZoomOut(object sender, EventArgs e) => isoView.ZoomOut();

        private void OnClickFit(object sender, EventArgs e) => isoView.ZoomToFit();

        private void OnZoomChanged(object sender, EventArgs e)
        {
            toolStripLabelZoom.Text = $"{isoView.Zoom * 100:0.#}%";
        }

        private void OnViewOptionChanged(object sender, EventArgs e)
        {
            isoView.ShowGrid = toolStripButtonGrid.Checked;
            if (_settings.ShowStatics != toolStripButtonStatics.Checked)
            {
                _settings.ShowStatics = toolStripButtonStatics.Checked;
                isoView.RefreshRender();
            }

            isoView.Invalidate();
        }

        private void OnRenderProgress(object sender, EventArgs e)
        {
        }

        private void OnHoverChanged(object sender, EventArgs e)
        {
            RegionCell cell = isoView.HoverCell;
            statusLabelCell.Text = cell == null ? string.Empty : $"X {cell.X}  Y {cell.Y}  Z {cell.LandZ}  land 0x{cell.LandId:X4}  statics {cell.Statics.Count}";
            UpdateBrushOutline();

            if (!_pasting || _locked || _clip == null || cell == null)
            {
                return;
            }

            int x = cell.X - (_clip.Width / 2), y = cell.Y - (_clip.Height / 2);
            if (x != _pasteX || y != _pasteY)
            {
                _pasteX = x;
                _pasteY = y;
                PositionChanged();
            }
        }

        private void OnCellClicked(object sender, IsoCellEventArgs e)
        {
            if (!_pasting || _clip == null)
            {
                return;
            }

            if (!_locked)
            {
                RegionCell cell = isoView.CellAt(isoView.PointToClient(MousePosition));
                if (cell != null)
                {
                    _pasteX = cell.X - (_clip.Width / 2);
                    _pasteY = cell.Y - (_clip.Height / 2);
                }
            }

            _locked = !_locked;
            isoView.SelectedCell = null;
            PositionChanged();
        }

        #endregion

        #region Paste

        private ClipPasteOptions CurrentOptions()
        {
            return new ClipPasteOptions
            {
                IncludeLand = checkBoxPasteLand.Checked && ClipHasLand,
                IncludeStatics = checkBoxPasteStatics.Checked,
                HeightMode = comboBoxHeightMode.SelectedIndex == 0 ? ClipHeightMode.Keep : ClipHeightMode.MatchEdges,
                ZOffset = (int)numericZOffset.Value,
                BlendWidth = (int)numericBlend.Value,
                StaticsFollowGround = checkBoxFollowGround.Checked,
                RemoveExisting = comboBoxRemove.SelectedIndex switch
                {
                    1 => ObstacleFilter.Plants,
                    2 => ObstacleFilter.None,
                    _ => ObstacleFilter.All
                },
                ExcludedIds = new HashSet<ushort>(_excludedIds),
                MinZ = (int)numericMinZ.Value,
                MaxZ = (int)numericMaxZ.Value
            };
        }

        private void OnPasteOptionChanged(object sender, EventArgs e)
        {
            if (sender == checkBoxPasteLand || sender == checkBoxPasteStatics)
            {
                _ghostDirty = true;
                UpdateGhostThumbnail();
            }

            if (_pasting)
            {
                PositionChanged();
            }
        }

        private void OnClickPasteMode(object sender, EventArgs e)
        {
            if (_pasting)
            {
                StopPasting();
            }
            else
            {
                StartPasting();
            }
        }

        private void StartPasting()
        {
            if (_clip == null)
            {
                MessageBox.Show("Copy an area first (tab \"1. Source\").", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_data == null)
            {
                Map map = TryGetMap(_targetDef, out string error);
                if (map == null)
                {
                    MessageBox.Show(error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                LoadWorkingArea(map.Width / 2, map.Height / 2);
                if (_data == null)
                {
                    return;
                }
            }

            if (ToolActive)
            {
                _checkToolActive.Checked = false;
            }

            _pasting = true;
            _locked = false;
            RegionCell hover = isoView.HoverCell;
            int cx = hover?.X ?? _data.Area.X + (_data.Area.Width / 2);
            int cy = hover?.Y ?? _data.Area.Y + (_data.Area.Height / 2);
            _pasteX = cx - (_clip.Width / 2);
            _pasteY = cy - (_clip.Height / 2);
            isoView.PlacementMode = true;
            tabControlSide.SelectedTab = tabPageTarget;
            PositionChanged();
            isoView.Focus();
        }

        private void StopPasting()
        {
            previewTimer.Stop();
            bool hadPreview = _previewApplied;
            RemovePreview(false);
            _pasting = false;
            _locked = false;
            _result = null;
            isoView.PlacementMode = false;
            isoView.GhostImage = null;
            isoView.FootprintCells = null;
            isoView.ObstacleCells = null;
            labelPasteResult.Text = "-";
            if (hadPreview)
            {
                ReloadView();
            }

            isoView.Invalidate();
            UpdateState();
        }

        private void OnClickPasteCancel(object sender, EventArgs e) => StopPasting();

        private void OnClickPasteNow(object sender, EventArgs e) => Paste();

        /// <summary>Position or options changed: remove an applied preview, update ghost and start the timer</summary>
        private void PositionChanged()
        {
            if (!_pasting || _clip == null || _data == null)
            {
                return;
            }

            bool hadPreview = _previewApplied;
            RemovePreview(false);
            _result = null;
            if (hadPreview)
            {
                ReloadView();
            }

            UpdatePasteOverlay();
            previewTimer.Stop();
            previewTimer.Start();
            UpdateState();
        }

        private void UpdatePaste()
        {
            if (_pasting)
            {
                if (_clip == null)
                {
                    StopPasting();
                    return;
                }

                PositionChanged();
            }
        }

        /// <summary>Ghost image, outline and border problems in the view</summary>
        private void UpdatePasteOverlay()
        {
            if (!_pasting || _clip == null || _data == null)
            {
                return;
            }

            ClipPasteOptions options = CurrentOptions();
            int offset = _result?.AppliedOffset ?? ClipPaster.EstimateOffset(_clip, _data.Map, _pasteX, _pasteY, options);

            MultiImage ghost = _previewApplied ? null : GetGhost();
            isoView.GhostImage = ghost?.Bitmap;
            if (ghost != null)
            {
                isoView.GhostLocation = new PointF(
                    _data.CanvasX(_pasteX, _pasteY) - ghost.OriginX,
                    _data.CanvasY(_pasteX, _pasteY, offset) - ghost.OriginY);
            }

            // outline of the clip
            var outline = new HashSet<Point>();
            int w = _clip.Width, h = _clip.Height;
            for (int x = 0; x < w; x++)
            {
                outline.Add(new Point(_pasteX + x, _pasteY));
                outline.Add(new Point(_pasteX + x, _pasteY + h - 1));
            }

            for (int y = 0; y < h; y++)
            {
                outline.Add(new Point(_pasteX, _pasteY + y));
                outline.Add(new Point(_pasteX + w - 1, _pasteY + y));
            }

            isoView.FootprintCells = outline;
            isoView.FootprintInvalid = _result != null && (_result.Outside > 0 || _result.Clamped > 0);
            isoView.ObstacleCells = _result?.EdgeProblems;
            isoView.Invalidate();

            string state = _locked ? "Fixed at" : "Following the mouse:";
            string text = $"{state} {_pasteX},{_pasteY} - {_pasteX + w - 1},{_pasteY + h - 1}   z offset {offset:+0;-0;0}";
            if (_result != null)
            {
                text += $"\n{_result.Land.Count:N0} land tiles, {_result.Statics.Count:N0} statics, {_result.RemovedStatics.Count:N0} existing statics removed" +
                        $"\nHighest step at the border: {_result.MaxEdgeJump}" +
                        (_result.EdgeProblems.Count > 0 ? $" ({_result.EdgeProblems.Count} cells > {new ClipPasteOptions().EdgeThreshold}, red; increase \"Edge blend\")" : " (ok)");
                if (_result.Outside > 0)
                {
                    text += $"\n{_result.Outside:N0} cells are outside of the map and are skipped.";
                }

                if (_result.Clamped > 0)
                {
                    text += $"\n{_result.Clamped:N0} heights were limited to -128..127.";
                }
            }
            else
            {
                text += "\n(calculating...)";
            }

            if (_previewApplied)
            {
                text += "\nPreview shown in the view. Enter = paste, click = move again.";
            }
            else if (!_locked)
            {
                text += "\nClick = fix position, arrows = move, Page up/down = z, Enter = paste, Esc = cancel";
            }

            labelPasteResult.Text = text;
        }

        private void OnPreviewTimer(object sender, EventArgs e)
        {
            previewTimer.Stop();

            if (_ghostDirty && _clip != null)
            {
                UpdateGhostThumbnail();
            }

            if (!_pasting || _clip == null || _data == null)
            {
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                _result = ClipPaster.Compute(_clip, _data.Map, _pasteX, _pasteY, CurrentOptions(), TargetSession);

                if (_locked && checkBoxLivePreview.Checked && !_previewApplied)
                {
                    ClipPaster.Apply(_result, TargetSession, PreviewName);
                    _previewApplied = true;
                    _previewOperations = TargetSession.OperationCount;
                    ReloadView();
                }
                else
                {
                    UpdatePasteOverlay();
                }
            }
            catch (Exception ex)
            {
                labelPasteResult.Text = $"Error: {ex.Message}";
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            UpdateState();
        }

        /// <summary>Undoes the shown preview (if it is still the last step)</summary>
        private void RemovePreview(bool reload)
        {
            if (!_previewApplied)
            {
                return;
            }

            _previewApplied = false;
            RegionEditSession session = TargetSession;
            if (session != null && session.OperationCount == _previewOperations && session.LastOperationName == PreviewName)
            {
                session.UndoLast(false);
            }

            if (reload)
            {
                ReloadView();
            }
        }

        private void Paste()
        {
            if (!_pasting || _clip == null || _data == null)
            {
                return;
            }

            previewTimer.Stop();
            RemovePreview(false);
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                RegionEditSession session = TargetSession;
                ClipPasteResult result = ClipPaster.Compute(_clip, _data.Map, _pasteX, _pasteY, CurrentOptions(), session);
                ClipPaster.Apply(result, session, PasteName);
                _result = null;
                _pasting = false;
                _locked = false;
                isoView.PlacementMode = false;
                isoView.GhostImage = null;
                isoView.FootprintCells = null;
                isoView.ObstacleCells = result.EdgeProblems.Count > 0 ? result.EdgeProblems : null;
                ReloadView();
                labelPasteResult.Text = $"Pasted at {_pasteX},{_pasteY}: {result.Land.Count:N0} land tiles, {result.Statics.Count:N0} statics, " +
                                        $"{result.RemovedStatics.Count:N0} removed. Highest step at the border: {result.MaxEdgeJump}.\n" +
                                        "Ctrl+Z = undo. \"Save...\" writes the map files.";
                statusLabelInfo.Text = "Clip pasted (not saved yet).";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Pasting failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            UpdateState();
        }

        private void MoveClip(int dx, int dy)
        {
            _locked = true;
            _pasteX += dx;
            _pasteY += dy;
            PositionChanged();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool textFocus = ActiveControl is TextBoxBase || ActiveControl is NumericUpDown || ActiveControl is ComboBox ||
                             ActiveControl is ListView || ActiveControl is ListBox;

            if (keyData == (Keys.Control | Keys.Z) && !(ActiveControl is TextBoxBase))
            {
                if (_strokeBatch == null)
                {
                    OnClickUndoLast(this, EventArgs.Empty);
                }

                return true;
            }

            if ((keyData == (Keys.Control | Keys.Y) || keyData == (Keys.Control | Keys.Shift | Keys.Z)) && !(ActiveControl is TextBoxBase))
            {
                OnClickRedo(this, EventArgs.Empty);
                return true;
            }

            if (!textFocus && keyData == Keys.T && _tabTools != null)
            {
                _checkToolActive.Checked = !_checkToolActive.Checked;
                return true;
            }

            if (!textFocus && HandleShapeKey(keyData))
            {
                return true;
            }

            if (!textFocus && HandleToolKey(keyData))
            {
                return true;
            }

            if (_pasting && !textFocus)
            {
                Keys key = keyData & Keys.KeyCode;
                int step = (keyData & Keys.Shift) != 0 ? 5 : 1;
                switch (key)
                {
                    case Keys.Left:
                        MoveClip(-step, 0);
                        return true;
                    case Keys.Right:
                        MoveClip(step, 0);
                        return true;
                    case Keys.Up:
                        MoveClip(0, -step);
                        return true;
                    case Keys.Down:
                        MoveClip(0, step);
                        return true;
                    case Keys.PageUp:
                        numericZOffset.Value = Math.Min(numericZOffset.Maximum, numericZOffset.Value + step);
                        return true;
                    case Keys.PageDown:
                        numericZOffset.Value = Math.Max(numericZOffset.Minimum, numericZOffset.Value - step);
                        return true;
                    case Keys.Enter:
                        Paste();
                        return true;
                    case Keys.Escape:
                        StopPasting();
                        return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Undo / save / close

        private void UpdateState()
        {
            RegionEditSession session = TargetSession;
            bool changes = session?.HasUnsavedChanges == true;
            toolStripButtonSave.Enabled = changes && !_previewApplied;
            toolStripButtonUndoLast.Enabled = session?.CanUndo == true;
            toolStripButtonUndoAll.Enabled = session?.CanUndo == true;
            toolStripButtonUndoLast.ToolTipText = session?.CanUndo == true ? $"Undo: {session.LastOperationName} (Ctrl+Z)" : "Nothing to undo";
            toolStripButtonRedo.Enabled = session?.CanRedo == true;
            toolStripButtonRedo.ToolTipText = session?.CanRedo == true ? $"Redo: {session.NextRedoName} (Ctrl+Y)" : "Nothing to redo";
            toolStripButtonOpenViewer.Enabled = _data != null;

            int otherMaps = _sessions.Values.Count(s => s != session && s.HasUnsavedChanges);
            statusLabelChanges.Text = (changes ? $"{session.ChangeCount} unsaved change(s)" : string.Empty) +
                                      (otherMaps > 0 ? $"  (+ {otherMaps} other map(s))" : string.Empty);

            buttonPasteMode.Text = _pasting ? "Stop positioning" : "Position clip";
            buttonPasteMode.Enabled = _clip != null;
            buttonPasteNow.Enabled = _pasting;
            buttonPasteCancel.Enabled = _pasting;
            bool hasClip = _clip != null;
            buttonRotate.Enabled = buttonMirrorX.Enabled = buttonMirrorY.Enabled = buttonClearClip.Enabled = hasClip;
            buttonAddStatic.Enabled = buttonRemoveUnchecked.Enabled = buttonCheckAll.Enabled = buttonCheckNone.Enabled = hasClip;
            buttonClipToLibrary.Enabled = hasClip && _clip.Statics.Count > 0;
        }

        private void OnClickUndoLast(object sender, EventArgs e)
        {
            RegionEditSession session = TargetSession;
            if (session == null)
            {
                return;
            }

            if (_pasting)
            {
                StopPasting();
            }

            string name = session.UndoLast();
            if (name != null)
            {
                isoView.ObstacleCells = null;
                ReloadView();
                statusLabelInfo.Text = $"Undone: {name}";
            }

            UpdateState();
        }

        private void OnClickRedo(object sender, EventArgs e)
        {
            RegionEditSession session = TargetSession;
            if (session == null || _strokeBatch != null)
            {
                return;
            }

            if (_pasting)
            {
                StopPasting();
            }

            string name = session.Redo();
            if (name != null)
            {
                isoView.ObstacleCells = null;
                ReloadView();
                statusLabelInfo.Text = $"Redone: {name}";
            }

            UpdateState();
        }

        private void OnClickUndoAll(object sender, EventArgs e)
        {
            RegionEditSession session = TargetSession;
            if (session == null || !session.CanUndo)
            {
                return;
            }

            StopPasting();
            if (MessageBox.Show($"Undo all {session.OperationCount} change(s) of \"{_targetDef.Name}\"?", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            session.UndoAll();
            isoView.ObstacleCells = null;
            ReloadView();
            UpdateState();
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            if (_data == null)
            {
                return;
            }

            StopPasting();
            SaveChanges(_targetDef, _data.Map);
        }

        /// <summary>Asks for directory, file number, land/statics and backup and writes the map. False if canceled or failed.</summary>
        private bool SaveChanges(MapDefinition def, Map map)
        {
            RegionEditSession session = GetSession(map);
            using (var dialog = new MapSaveDialog(map, def.Name, session.ChangeCount))
            {
                DialogResult result = dialog.ShowDialog(this);
                if (result == DialogResult.Abort)
                {
                    session.MarkSaved(false);
                    ReloadView(true);
                    UpdateState();
                    return false;
                }

                if (result != DialogResult.OK)
                {
                    return false;
                }

                string directory = dialog.TargetDirectory;
                MapSaveOptions options = dialog.SaveOptions;
                TileMatrix tiles = map.Tiles;
                bool allWritten = (options.IncludeLand || !tiles.HasModifiedLand) && (options.IncludeStatics || !tiles.HasStaticOverrides);
                bool replacesLoaded = MapSaveService.IsSameDirectory(directory, MapSaveService.GetSourceDirectory(map)) && options.OutputIndex == map.FileIndex;

                List<string> files;
                MapBackupInfo backup;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    files = MapSaveService.Save(map, def.Name, directory, dialog.CreateBackup, options, out backup);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Saving failed:\n{ex.Message}\n\nThe changes are still in memory.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

                if (allWritten || replacesLoaded)
                {
                    session.MarkSaved(!replacesLoaded);
                }

                if (_data != null && _data.Map == map)
                {
                    ReloadView(replacesLoaded);
                }

                UpdateState();
                MessageBox.Show("Saved:\n" + string.Join("\n", files.Select(Path.GetFileName)) + $"\n\nin {directory}" +
                                (backup != null ? $"\n\nBackup: {backup.Directory}" : string.Empty),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
        }

        private void OnClickBackups(object sender, EventArgs e)
        {
            MapDefinition def = _targetDef;
            Map map = _data?.Map ?? TryGetMap(def, out _);
            if (def == null || map == null)
            {
                return;
            }

            StopPasting();
            using (var form = new MapBackupForm(def.Name, new[] { Options.OutputPath, MapSaveService.GetSourceDirectory(map) }))
            {
                form.ShowDialog(this);
                if (form.Restored)
                {
                    GetSession(map).MarkSaved(false);
                    ReloadView(true);
                    UpdateState();
                }
            }
        }

        private void OnClickOpenViewer(object sender, EventArgs e)
        {
            if (_data == null)
            {
                return;
            }

            StopPasting();
            var viewer = new MapRegionViewerForm(_targetDef, _data.Area, _mapControl, TargetSession) { Owner = this };
            viewer.Show();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            StopPasting();

            foreach (KeyValuePair<Map, RegionEditSession> pair in _sessions.ToList())
            {
                RegionEditSession session = pair.Value;
                if (!session.HasUnsavedChanges)
                {
                    continue;
                }

                MapDefinition def = FindDefinition(pair.Key);
                string name = def?.Name ?? $"map{pair.Key.FileIndex}";
                DialogResult result = MessageBox.Show(
                    $"\"{name}\" has {session.ChangeCount} unsaved change(s).\n\nYes = save (choose the directory, file number and backup)\nNo = undo the changes\nCancel = keep the window open",
                    Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                switch (result)
                {
                    case DialogResult.Yes:
                        if (def == null || !SaveChanges(def, pair.Key))
                        {
                            e.Cancel = true;
                            return;
                        }

                        if (session.HasUnsavedChanges)
                        {
                            // only a part was written (e.g. without statics): the rest is undone
                            session.UndoAll();
                        }

                        break;
                    case DialogResult.No:
                        session.UndoAll();
                        pair.Key.ResetCache();
                        break;
                    default:
                        e.Cancel = true;
                        return;
                }
            }

            foreach (Form owned in OwnedForms)
            {
                owned.Close();
            }

            if (_mapControl != null && !_mapControl.IsDisposed)
            {
                _mapControl.RefreshMapAfterEdit();
            }
        }

        private MapDefinition FindDefinition(Map map)
        {
            if (_targetDef != null && _data?.Map == map)
            {
                return _targetDef;
            }

            return MapRegistry.Definitions.Concat(MapRegistry.TemporaryDefinitions).FirstOrDefault(d =>
            {
                try
                {
                    return d.GetMap() == map;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        #endregion
    }
}
