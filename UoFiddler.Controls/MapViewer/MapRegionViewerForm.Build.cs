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
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.MapViewer.Build;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// "Build" tab: place buildings (multis, library, files), remove statics in the way, flatten the ground,
    /// clear/flatten/export areas of the map.
    /// </summary>
    public partial class MapRegionViewerForm
    {
        private MultiTemplate _template;
        private MultiImage _ghost;
        private bool _ghostWithInvisible;
        private BuildPlacement _placement;
        private bool _placing;
        private bool _positionLocked;
        private List<MapStatic> _obstacles = new List<MapStatic>();
        private MultiBrowserForm _browser;
        private Rectangle? _area;
        private bool _buildInitialized;

        private void InitBuild()
        {
            comboBoxAnchor.SelectedIndex = 0;
            comboBoxHeightMode.SelectedIndex = 0;
            comboBoxObstacles.SelectedIndex = 0;
            comboBoxAreaFilter.SelectedIndex = 0;
            isoView.AreaSelected += OnAreaSelected;
            _buildInitialized = true;
            UpdateBuildButtons();
        }

        private void DisposeBuild()
        {
            _ghost?.Dispose();
            _ghost = null;
            if (_browser != null && !_browser.IsDisposed)
            {
                _browser.Close();
            }
        }

        #region Template

        private void OnClickOpenBrowser(object sender, EventArgs e)
        {
            MultiBrowserForm browser = GetBrowser();
            browser.Show();
            browser.Activate();
        }

        private MultiBrowserForm GetBrowser()
        {
            if (_browser == null || _browser.IsDisposed)
            {
                _browser = new MultiBrowserForm(SetTemplateFromBrowser) { Owner = this };
            }

            return _browser;
        }

        private void SetTemplateFromBrowser(MultiTemplate template)
        {
            SetTemplate(template);
            Activate();
        }

        private void OnClickImportTemplate(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = MultiFormats.ImportFilter, Title = "Import building" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    SetTemplate(MultiFormats.Load(dialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"The file could not be read:\n{ex.Message}", "Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnClickNormalizeZ(object sender, EventArgs e)
        {
            if (_template == null)
            {
                return;
            }

            MultiTemplate normalized = _template.NormalizeZ();
            normalized.Source = _template.Source;
            SetTemplate(normalized, _placing);
        }

        private void OnClickClearTemplate(object sender, EventArgs e)
        {
            StopPlacing();
            _template = null;
            _ghost?.Dispose();
            _ghost = null;
            UpdateTemplateLabel();
            UpdateBuildButtons();
        }

        /// <summary>Uses a building for placing and starts the placing mode</summary>
        public void SetTemplate(MultiTemplate template, bool startPlacing = true)
        {
            _template = template;
            _ghost?.Dispose();
            _ghost = null;
            UpdateTemplateLabel();
            tabControlSide.SelectedTab = tabPageBuild;

            if (template != null && template.Components.Count > 0 && startPlacing)
            {
                StartPlacing();
            }

            UpdateBuildButtons();
        }

        private void UpdateTemplateLabel()
        {
            MultiTemplate t = _template;
            labelTemplate.Text = t == null
                ? "No building selected. Open the multi window or import a file."
                : $"{t.Name}  ({t.Source})\n{t.Components.Count} parts, {t.Width} x {t.Height}, z {t.MinZ}..{t.MaxZ}" +
                  (t.HasInvisibleParts ? $", {t.Components.Count(c => !c.IsVisible)} invisible" : string.Empty) +
                  (t.MaxZ - t.MinZ > 255 ? "\nThe building is higher than z 255 and can not be placed completely!" : string.Empty);
        }

        private MultiImage GetGhost()
        {
            if (_template == null)
            {
                return null;
            }

            if (_ghost == null || _ghostWithInvisible != checkBoxInvisible.Checked)
            {
                _ghost?.Dispose();
                _ghostWithInvisible = checkBoxInvisible.Checked;
                _ghost = MultiPreview.Render(_template, _art, _ghostWithInvisible);
            }

            return _ghost;
        }

        #endregion

        #region Placing

        private void OnClickPlaceMode(object sender, EventArgs e)
        {
            if (_placing)
            {
                StopPlacing();
            }
            else
            {
                StartPlacing();
            }
        }

        private void StartPlacing()
        {
            if (_template == null)
            {
                return;
            }

            _placing = true;
            _positionLocked = false;
            _placement = new BuildPlacement(_template, _data.Map);

            RegionCell hover = isoView.HoverCell;
            _placement.X = hover?.X ?? _data.Area.X + (_data.Area.Width / 2);
            _placement.Y = hover?.Y ?? _data.Area.Y + (_data.Area.Height / 2);

            isoView.PlacementMode = true;
            UpdatePlacement();
            isoView.Focus();
        }

        private void StopPlacing()
        {
            _placing = false;
            _positionLocked = false;
            _placement = null;
            _obstacles.Clear();
            isoView.PlacementMode = false;
            isoView.GhostImage = null;
            isoView.FootprintCells = null;
            isoView.ObstacleCells = null;
            listViewObstacles.Items.Clear();
            listBoxChecks.Items.Clear();
            labelPlacement.Text = "-";
            isoView.Invalidate();
            UpdateBuildButtons();
        }

        private void OnClickCancelPlace(object sender, EventArgs e)
        {
            StopPlacing();
        }

        private void OnPlacementOptionChanged(object sender, EventArgs e)
        {
            if (!_buildInitialized)
            {
                return;
            }

            labelZValue.Text = comboBoxHeightMode.SelectedIndex == 4 ? "Fixed z:" : "Z offset:";
            UpdatePlacement();
        }

        private void OnObstacleFilterChanged(object sender, EventArgs e)
        {
            FillObstacleList();
        }

        /// <summary>
        /// Recalculates the placement after a change of position or options and updates the view
        /// </summary>
        private void UpdatePlacement()
        {
            if (!_placing || _placement == null || !_buildInitialized)
            {
                return;
            }

            _placement.Anchor = comboBoxAnchor.SelectedIndex switch
            {
                1 => PlacementAnchor.UpperLeft,
                2 => PlacementAnchor.Origin,
                _ => PlacementAnchor.Center
            };
            _placement.HeightMode = comboBoxHeightMode.SelectedIndex switch
            {
                1 => PlacementHeight.GroundLowest,
                2 => PlacementHeight.GroundHighest,
                3 => PlacementHeight.GroundAverage,
                4 => PlacementHeight.Fixed,
                _ => PlacementHeight.GroundAtAnchor
            };
            _placement.ZValue = (int)numericZValue.Value;
            _placement.IncludeInvisible = checkBoxInvisible.Checked;
            _placement.Update(_data.Area);

            _obstacles = _placement.FindObstacles(_session);

            // ghost: template cell 0,0 at z 0 is drawn at the placed position of that cell
            MultiImage ghost = GetGhost();
            int originX = _placement.X + _placement.OffsetX, originY = _placement.Y + _placement.OffsetY;
            isoView.GhostImage = ghost?.Bitmap;
            if (ghost != null)
            {
                isoView.GhostLocation = new PointF(
                    _data.CanvasX(originX, originY) - ghost.OriginX,
                    _data.CanvasY(originX, originY, _placement.BaseZ) - ghost.OriginY);
            }

            isoView.FootprintCells = _placement.Footprint;
            isoView.FootprintInvalid = !_placement.IsValid;
            isoView.ObstacleCells = new HashSet<Point>(_obstacles.Select(o => new Point(o.X, o.Y)));
            isoView.Invalidate();

            labelPlacement.Text = $"{(_positionLocked ? "Fixed at" : "Following the mouse:")} {_placement.X}, {_placement.Y}   base z {_placement.BaseZ}   " +
                                  $"(ground {_placement.GroundMin}..{_placement.GroundMax})";

            listBoxChecks.BeginUpdate();
            listBoxChecks.Items.Clear();
            foreach (string error in _placement.Errors)
            {
                listBoxChecks.Items.Add("Error: " + error);
            }

            foreach (string warning in _placement.Warnings)
            {
                listBoxChecks.Items.Add("Warning: " + warning);
            }

            listBoxChecks.Items.Add(_obstacles.Count == 0
                ? "No statics in the way."
                : $"{_obstacles.Count} statics in the way ({_obstacles.Count(o => BuildPlacement.IsPlant(o.Id))} trees/plants/rocks).");
            listBoxChecks.Items.Add($"{_placement.Statics.Count} parts will be placed.");
            listBoxChecks.EndUpdate();

            // the list is only filled when the position is fixed (fast mouse movement)
            if (_positionLocked)
            {
                FillObstacleList();
            }
            else if (listViewObstacles.Items.Count > 0)
            {
                listViewObstacles.Items.Clear();
            }

            UpdateBuildButtons();
        }

        private ObstacleFilter CurrentObstacleFilter => comboBoxObstacles.SelectedIndex switch
        {
            1 => ObstacleFilter.Plants,
            2 => ObstacleFilter.None,
            _ => ObstacleFilter.All
        };

        private void FillObstacleList()
        {
            if (!_placing)
            {
                return;
            }

            ObstacleFilter filter = CurrentObstacleFilter;
            listViewObstacles.BeginUpdate();
            try
            {
                listViewObstacles.Items.Clear();
                foreach (MapStatic s in _obstacles.Take(5000))
                {
                    var item = new ListViewItem(s.X.ToString()) { Tag = s, Checked = BuildPlacement.MatchesFilter(s, filter) };
                    item.SubItems.Add(s.Y.ToString());
                    item.SubItems.Add(s.Z.ToString());
                    item.SubItems.Add($"0x{s.Id:X4}");
                    item.SubItems.Add(s.Id < TileData.ItemTable.Length ? TileData.ItemTable[s.Id].Name : string.Empty);
                    listViewObstacles.Items.Add(item);
                }
            }
            finally
            {
                listViewObstacles.EndUpdate();
            }
        }

        /// <summary>Statics that are removed when placing (checked list entries or the filter)</summary>
        private List<MapStatic> ObstaclesToRemove()
        {
            if (listViewObstacles.Items.Count > 0 && listViewObstacles.Items.Count == Math.Min(5000, _obstacles.Count))
            {
                var removed = listViewObstacles.CheckedItems.Cast<ListViewItem>().Select(i => (MapStatic)i.Tag).ToList();
                if (_obstacles.Count > 5000)
                {
                    removed.AddRange(_obstacles.Skip(5000).Where(o => BuildPlacement.MatchesFilter(o, CurrentObstacleFilter)));
                }

                return removed;
            }

            ObstacleFilter filter = CurrentObstacleFilter;
            return _obstacles.Where(o => BuildPlacement.MatchesFilter(o, filter)).ToList();
        }

        private void OnBuildHoverChanged()
        {
            if (!_placing || _positionLocked || _placement == null)
            {
                return;
            }

            RegionCell cell = isoView.CellAt(isoView.PointToClient(MousePosition));
            if (cell == null || (cell.X == _placement.X && cell.Y == _placement.Y))
            {
                return;
            }

            _placement.X = cell.X;
            _placement.Y = cell.Y;
            UpdatePlacement();
        }

        /// <summary>Click in the view: fixes / releases the position while placing</summary>
        private bool OnBuildCellClicked()
        {
            if (isoView.AreaSelectMode)
            {
                return true;
            }

            if (!_placing || _placement == null)
            {
                return false;
            }

            if (!_positionLocked)
            {
                RegionCell cell = isoView.CellAt(isoView.PointToClient(MousePosition));
                if (cell != null)
                {
                    _placement.X = cell.X;
                    _placement.Y = cell.Y;
                }
            }

            _positionLocked = !_positionLocked;
            isoView.SelectedCell = null;
            UpdatePlacement();
            return true;
        }

        private bool HandleBuildKey(Keys keyData)
        {
            if (!_placing || _placement == null)
            {
                return false;
            }

            if (ActiveControl is TextBoxBase || ActiveControl is NumericUpDown || ActiveControl is ComboBox || ActiveControl is ListView || ActiveControl is ListBox)
            {
                return false;
            }

            Keys key = keyData & Keys.KeyCode;
            int step = (keyData & Keys.Shift) != 0 ? 5 : 1;

            switch (key)
            {
                case Keys.Left:
                    MoveBuilding(-step, 0);
                    return true;
                case Keys.Right:
                    MoveBuilding(step, 0);
                    return true;
                case Keys.Up:
                    MoveBuilding(0, -step);
                    return true;
                case Keys.Down:
                    MoveBuilding(0, step);
                    return true;
                case Keys.PageUp:
                    numericZValue.Value = Math.Min(numericZValue.Maximum, numericZValue.Value + step);
                    return true;
                case Keys.PageDown:
                    numericZValue.Value = Math.Max(numericZValue.Minimum, numericZValue.Value - step);
                    return true;
                case Keys.Enter:
                    PlaceBuilding();
                    return true;
                case Keys.Escape:
                    StopPlacing();
                    return true;
            }

            return false;
        }

        private void MoveBuilding(int dx, int dy)
        {
            _positionLocked = true;
            _placement.X = Math.Max(0, Math.Min(_data.Map.Width - 1, _placement.X + dx));
            _placement.Y = Math.Max(0, Math.Min(_data.Map.Height - 1, _placement.Y + dy));
            UpdatePlacement();
        }

        private void OnClickPlaceNow(object sender, EventArgs e)
        {
            PlaceBuilding();
        }

        private void PlaceBuilding()
        {
            if (!_placing || _placement == null)
            {
                return;
            }

            UpdatePlacement();
            if (!_placement.IsValid)
            {
                MessageBox.Show("The building can not be placed here:\n\n" + string.Join("\n", _placement.Errors), "Place building",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<MapStatic> remove = ObstaclesToRemove();
            int added;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                added = _placement.Apply(_session, remove, checkBoxFlatten.Checked);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            string name = _template.Name;
            int x = _placement.X, y = _placement.Y, z = _placement.BaseZ;
            StopPlacing();
            AfterEdit($"Placed {name} at {x},{y},{z}: {added} statics, {remove.Count} removed" + (checkBoxFlatten.Checked ? ", ground flattened" : string.Empty));
        }

        #endregion

        #region Area

        private void OnClickSelectArea(object sender, EventArgs e)
        {
            if (_placing)
            {
                StopPlacing();
            }

            isoView.AreaSelectMode = !isoView.AreaSelectMode;
            buttonSelectArea.Text = isoView.AreaSelectMode ? "Drag in the view..." : "Select area";
            isoView.Cursor = isoView.AreaSelectMode ? Cursors.Cross : Cursors.Default;
            isoView.Focus();
        }

        private void OnAreaSelected(object sender, EventArgs e)
        {
            isoView.AreaSelectMode = false;
            isoView.Cursor = Cursors.Default;
            buttonSelectArea.Text = "Select area";
            _area = isoView.SelectedArea;

            if (_area.HasValue)
            {
                // land height in the area as suggestion for flattening
                Rectangle a = _area.Value;
                var heights = new List<int>();
                for (int x = a.Left; x < a.Right; x++)
                {
                    for (int y = a.Top; y < a.Bottom; y++)
                    {
                        heights.Add(_data.Map.Tiles.GetLandTile(x, y).Z);
                    }
                }

                numericFlattenZ.Value = heights.Count == 0 ? 0 : (int)Math.Round(heights.Average());
            }

            UpdateAreaLabel();
            UpdateBuildButtons();
        }

        private void OnAreaFilterChanged(object sender, EventArgs e)
        {
            UpdateAreaLabel();
        }

        private List<MapStatic> AreaStatics()
        {
            if (!_area.HasValue)
            {
                return new List<MapStatic>();
            }

            Rectangle a = _area.Value;
            int minZ = (int)numericAreaMinZ.Value, maxZ = (int)numericAreaMaxZ.Value;
            bool plantsOnly = comboBoxAreaFilter.SelectedIndex == 1;
            return _session.GetStaticsInArea(a.Left, a.Top, a.Right - 1, a.Bottom - 1)
                .Where(s => s.Z >= minZ && s.Z <= maxZ && (!plantsOnly || BuildPlacement.IsPlant(s.Id)))
                .ToList();
        }

        private void UpdateAreaLabel()
        {
            if (!_area.HasValue)
            {
                labelArea.Text = "no area selected";
                return;
            }

            Rectangle a = _area.Value;
            labelArea.Text = $"{a.Left},{a.Top} - {a.Right - 1},{a.Bottom - 1}  ({a.Width} x {a.Height}), {AreaStatics().Count} statics";
        }

        private void OnClickAreaRemove(object sender, EventArgs e)
        {
            List<MapStatic> statics = AreaStatics();
            if (statics.Count == 0)
            {
                MessageBox.Show("No statics in the area (check the z range and the filter).", "Area", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Remove {statics.Count} statics from the area?\n(in memory, \"Undo\" is possible)", "Area",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            int removed = _session.RemoveStatics(statics, "Clear area");
            AfterEdit($"{removed} statics removed from the area");
            UpdateAreaLabel();
        }

        private void OnClickAreaFlatten(object sender, EventArgs e)
        {
            if (!_area.HasValue)
            {
                return;
            }

            Rectangle a = _area.Value;
            int z = (int)numericFlattenZ.Value;
            if (MessageBox.Show($"Set the land of {a.Width} x {a.Height} tiles to z {z}?\nStatics are not moved.", "Area",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            var cells = new List<(int, int)>();
            for (int x = a.Left; x < a.Right; x++)
            {
                for (int y = a.Top; y < a.Bottom; y++)
                {
                    cells.Add((x, y));
                }
            }

            int changed = _session.FlattenLand(cells, z);
            AfterEdit($"{changed} land tiles set to z {z}");
        }

        /// <summary>Statics of the area as building, z relative to the lowest land of the area</summary>
        private MultiTemplate AreaTemplate()
        {
            if (!_area.HasValue)
            {
                return null;
            }

            Rectangle a = _area.Value;
            int ground = int.MaxValue;
            for (int x = a.Left; x < a.Right; x++)
            {
                for (int y = a.Top; y < a.Bottom; y++)
                {
                    ground = Math.Min(ground, _data.Map.Tiles.GetLandTile(x, y).Z);
                }
            }

            string name = InputDialog.Show(this, "Building from the map", "Name:", $"{_definition.Name} {a.Left},{a.Top}", false);
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            MultiTemplate t = BuildPlacement.FromArea(_session, a, (int)numericAreaMinZ.Value, (int)numericAreaMaxZ.Value,
                ground == int.MaxValue ? 0 : ground, name.Trim());
            if (comboBoxAreaFilter.SelectedIndex == 1)
            {
                t.Components.RemoveAll(c => !BuildPlacement.IsPlant(c.ItemId));
            }

            t.Author = Environment.UserName;
            if (t.Components.Count == 0)
            {
                MessageBox.Show("No statics in the area (check the z range and the filter).", "Area", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            return t;
        }

        private void OnClickAreaExport(object sender, EventArgs e)
        {
            if (!_area.HasValue)
            {
                return;
            }

            // same dialog as "Export Area" in the map tab (includes the changes made in this window)
            var form = new AreaExportForm(_definition, _area.Value, _session);
            form.Show(this);
        }

        private void OnClickAreaLibrary(object sender, EventArgs e)
        {
            MultiTemplate t = AreaTemplate();
            if (t == null)
            {
                return;
            }

            string category = InputDialog.Show(this, "Building library", "Category:", "From map", false);
            if (category == null)
            {
                return;
            }

            t.Category = category.Trim();
            MultiBrowserForm browser = GetBrowser();
            int id = browser.AddToLibrary(t);
            browser.Show();
            browser.ShowLibrary();
            statusLabelCell.Text = id >= 0 ? $"\"{t.Name}\" stored in the library as #{id} ({t.Components.Count} parts)" : "The library could not be opened.";
        }

        private void OnClickAreaMulti(object sender, EventArgs e)
        {
            MultiTemplate t = AreaTemplate();
            if (t != null)
            {
                MultiBrowserForm.SaveToMultiMul(this, t);
            }
        }

        #endregion

        private void UpdateBuildButtons()
        {
            if (!_buildInitialized)
            {
                return;
            }

            bool hasTemplate = _template != null && _template.Components.Count > 0;
            buttonNormalizeZ.Enabled = hasTemplate;
            buttonClearTemplate.Enabled = _template != null;
            buttonPlaceMode.Enabled = hasTemplate;
            buttonPlaceMode.Text = _placing ? "Stop placing" : "Start placing";
            buttonPlaceNow.Enabled = _placing && _placement != null && _placement.IsValid;
            buttonCancelPlace.Enabled = _placing;

            bool hasArea = _area.HasValue;
            buttonAreaRemove.Enabled = hasArea;
            buttonAreaFlatten.Enabled = hasArea;
            buttonAreaExport.Enabled = hasArea;
            buttonAreaLibrary.Enabled = hasArea;
            buttonAreaMulti.Enabled = hasArea;
        }
    }
}
