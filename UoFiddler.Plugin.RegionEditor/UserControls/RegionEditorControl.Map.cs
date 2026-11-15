/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Region editor – map selection, tools, shapes drawn on the map,
 * view options and the connection to the running client.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.RegionEditor.Classes;

namespace UoFiddler.Plugin.RegionEditor.UserControls
{
    public partial class RegionEditorControl
    {
        private int _clientX;
        private int _clientY;
        private int _clientZ;
        private int _clientMap = -1;

        #region Maps

        private sealed class MapItem
        {
            public MapDefinition Definition { get; set; }
            public int Regions { get; set; }

            public override string ToString() => Regions > 0 ? $"{Definition.Name}  ({Regions})" : Definition.Name;
        }

        private void BuildMapCombo(int selectId)
        {
            _updating = true;
            try
            {
                _cmbMap.Items.Clear();
                foreach (MapDefinition def in MapRegistry.Definitions.Where(d => d.Visible || d.Id == selectId))
                {
                    int count = _project.Facets.FirstOrDefault(f => f.MapId == def.Id)?.AllRegions().Count() ?? 0;
                    _cmbMap.Items.Add(new MapItem { Definition = def, Regions = count });
                }

                int index = _cmbMap.Items.Cast<MapItem>().ToList().FindIndex(m => m.Definition.Id == selectId);
                _cmbMap.SelectedIndex = index >= 0 ? index : _cmbMap.Items.Count > 0 ? 0 : -1;
            }
            finally
            {
                _updating = false;
            }

            if (_cmbMap.SelectedItem is MapItem item && item.Definition.Id != _mapView.MapId)
            {
                ShowMap(item.Definition);
            }
        }

        /// <summary>updates the region counts in the map list</summary>
        private void UpdateMapCounts()
        {
            _updating = true;
            for (int i = 0; i < _cmbMap.Items.Count; i++)
            {
                var item = (MapItem)_cmbMap.Items[i];
                int count = _project.Facets.FirstOrDefault(f => f.MapId == item.Definition.Id)?.AllRegions().Count() ?? 0;
                if (count != item.Regions)
                {
                    item.Regions = count;
                    _cmbMap.Items[i] = item;
                }
            }

            _updating = false;
        }

        private void OnMapSelected(object sender, EventArgs e)
        {
            if (_updating || !(_cmbMap.SelectedItem is MapItem item))
            {
                return;
            }

            ShowMap(item.Definition);
        }

        private void ShowMap(MapDefinition def)
        {
            if (def == null)
            {
                return;
            }

            if (!def.FilesExist())
            {
                SetMessage(Loc.F("msg.mapmissing", def.Name), true);
            }

            _selected = null;
            _selectedRect = -1;
            _mapView.SetMap(def);

            FacetRegions facet = CurrentFacet(false);
            Rectangle all = Rectangle.Empty;
            foreach (RegionNode r in facet?.Regions ?? new List<RegionNode>())
            {
                if (!r.Bounds.IsEmpty)
                {
                    all = all.IsEmpty ? r.Bounds : Rectangle.Union(all, r.Bounds);
                }
            }

            if (!all.IsEmpty)
            {
                _mapView.ZoomTo(all);
            }
            else
            {
                _mapView.CenterOn(def.Width / 2, def.Height / 2);
            }

            UpdateAll();
        }

        /// <summary>switches to a map (when needed) and keeps the selection</summary>
        private void SwitchToMap(int mapId)
        {
            if (_mapView.MapId == mapId)
            {
                return;
            }

            int index = _cmbMap.Items.Cast<MapItem>().ToList().FindIndex(m => m.Definition.Id == mapId);
            if (index < 0)
            {
                MapDefinition def = MapRegistry.GetById(mapId);
                if (def == null)
                {
                    SetMessage(Loc.F("msg.mapunknown", mapId), true);
                    return;
                }

                BuildMapCombo(mapId);
                return;
            }

            _cmbMap.SelectedIndex = index;
        }

        #endregion

        #region Tools

        private ToolStripButton ToolButton(MapTool tool)
        {
            switch (tool)
            {
                case MapTool.Pan: return _btnToolPan;
                case MapTool.Rect: return _btnToolRect;
                case MapTool.Polygon: return _btnToolPolygon;
                case MapTool.Circle: return _btnToolCircle;
                case MapTool.Fill: return _btnToolFill;
                case MapTool.Go: return _btnToolGo;
                case MapTool.Entrance: return _btnToolEntrance;
                default: return _btnToolSelect;
            }
        }

        private void SetTool(MapTool tool)
        {
            _mapView.Tool = tool;
            foreach (MapTool t in Enum.GetValues(typeof(MapTool)))
            {
                ToolButton(t).Checked = t == tool;
            }

            SetMessage(Loc.T("toolhelp." + tool.ToString().ToLowerInvariant()));
        }

        private void OnClickTool(object sender, EventArgs e)
        {
            foreach (MapTool t in Enum.GetValues(typeof(MapTool)))
            {
                if (ToolButton(t) == sender)
                {
                    SetTool(t);
                    _mapView.Focus();
                    return;
                }
            }
        }

        private void OnSnapChanged(object sender, EventArgs e)
        {
            int snap = int.TryParse(_cmbSnap.SelectedItem as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 1;
            _mapView.Snap = snap;
            if (!_updating && _loaded)
            {
                RegionEditorSettings.Current.Snap = snap;
                RegionEditorSettings.Current.Save();
            }
        }

        private void OnClickZoomIn(object sender, EventArgs e) => _mapView.ZoomIn();

        private void OnClickZoomOut(object sender, EventArgs e) => _mapView.ZoomOut();

        private void OnClickZoomFit(object sender, EventArgs e)
        {
            if (_selected != null && !_selected.Bounds.IsEmpty)
            {
                _mapView.ZoomTo(_selected.Bounds);
            }
        }

        private void OnMapViewChanged(object sender, EventArgs e)
        {
            _lblZoom.Text = $"{_mapView.Zoom * 100:0}%";
        }

        #endregion

        #region View options

        private void ApplyViewOptions()
        {
            RegionEditorSettings s = RegionEditorSettings.Current;
            _mapView.ShowStatics = s.ShowStatics;
            _mapView.ShowGrid = s.ShowGrid;
            _mapView.ShowLabels = s.ShowLabels;
            _mapView.ShowMinimap = s.ShowMinimap;
            _mapView.FillAlpha = s.FillAlpha;
            _mapView.Snap = s.Snap;
            _mapView.ClientPosition = s.ShowClient && _clientMap >= 0 ? (_clientX, _clientY, _clientMap) : ((int, int, int)?)null;
            _mapView.Invalidate();
        }

        private void OnClickViewOption(object sender, EventArgs e)
        {
            RegionEditorSettings s = RegionEditorSettings.Current;
            s.ShowStatics = _miViewStatics.Checked;
            s.ShowGrid = _miViewGrid.Checked;
            s.ShowLabels = _miViewLabels.Checked;
            s.ShowMinimap = _miViewMinimap.Checked;
            s.ShowClient = _miViewClient.Checked;
            s.Save();
        }

        private void OnClickViewRefresh(object sender, EventArgs e) => _mapView.RefreshMap();

        #endregion

        #region Shapes from the map

        /// <summary>Shift = add to the selected region, Alt = cut out of it, otherwise a new region</summary>
        private void OnMapShapeCreated(object sender, ShapeEventArgs e)
        {
            List<RegionRect> rects = null;
            RegionPolygon polygon = null;
            switch (e.Tool)
            {
                case MapTool.Rect:
                    if (e.Rect.Width <= 0 || e.Rect.Height <= 0)
                    {
                        return;
                    }

                    rects = new List<RegionRect> { RegionRect.From(e.Rect) };
                    break;
                case MapTool.Polygon:
                    if (Geometry.IsRectangle(e.Polygon, out Rectangle asRect))
                    {
                        rects = new List<RegionRect> { RegionRect.From(asRect) };
                    }
                    else if (e.Subtract)
                    {
                        rects = Geometry.PolygonToRects(e.Polygon);
                    }
                    else
                    {
                        polygon = new RegionPolygon { Points = e.Polygon };
                    }

                    break;
                case MapTool.Circle:
                    if (e.Radius <= 0)
                    {
                        return;
                    }

                    rects = Geometry.CircleToRects(e.Cell, e.Radius);
                    break;
                case MapTool.Fill:
                    rects = FillArea(e.Cell);
                    if (rects == null)
                    {
                        return;
                    }

                    break;
            }

            RegionNode target = _selected;
            bool empty = target != null && target.Rects.Count == 0 && target.Polygons.Count == 0;

            if (e.Subtract && target != null)
            {
                if (target.Locked || rects == null)
                {
                    return;
                }

                Change(Loc.F("hist.cut", target.DisplayName), () =>
                {
                    foreach (RegionRect cut in rects)
                    {
                        target.Rects = Geometry.Subtract(target.Rects, cut.Bounds);
                    }

                    _selectedRect = -1;
                });
                return;
            }

            if (target != null && !target.Locked && (e.AddToSelected || empty))
            {
                Change(Loc.F("hist.addarea", target.DisplayName), () =>
                {
                    if (polygon != null)
                    {
                        target.Polygons.Add(polygon);
                    }
                    else
                    {
                        target.Rects.AddRange(rects);
                        if (rects.Count > 1)
                        {
                            target.Rects = Geometry.Optimize(target.Rects);
                        }

                        _selectedRect = target.Rects.Count - 1;
                    }
                });
                return;
            }

            RegionNode created = AddRegion(null, r =>
            {
                if (polygon != null)
                {
                    r.Polygons.Add(polygon);
                }
                else
                {
                    r.Rects.AddRange(rects.Count > 1 ? Geometry.Optimize(rects) : rects);
                }
            });

            if (created != null)
            {
                _txtName.Focus();
                _txtName.SelectAll();
                SetMessage(Loc.F("msg.created", created.Name));
            }
        }

        /// <summary>
        /// Fill tool: all connected cells of the visible area with the same kind of land
        /// (same land tile name in tiledata, e.g. "grass", "forest", "water").
        /// </summary>
        private List<RegionRect> FillArea(Point start)
        {
            MapDefinition def = _mapView.MapDefinition;
            if (def == null)
            {
                return null;
            }

            Rectangle area = Rectangle.Intersect(_mapView.VisibleCells, new Rectangle(0, 0, def.Width, def.Height));
            Map map = def.GetMap();
            string Kind(int x, int y)
            {
                Tile t = map.Tiles.GetLandTile(x, y);
                if (t.Id >= TileData.LandTable.Length)
                {
                    return string.Empty;
                }

                LandData land = TileData.LandTable[t.Id];
                return (land.Flags & TileFlag.Wet) != 0 ? "#water" : (land.Name ?? string.Empty).Trim().ToLowerInvariant();
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                string kind = Kind(start.X, start.Y);
                CellMask mask = Geometry.FloodFill(start, area, (x, y) => Kind(x, y) == kind, 2_000_000);
                if (mask == null)
                {
                    SetMessage(Loc.T("msg.fillnone"), true);
                    return null;
                }

                List<RegionRect> rects = Geometry.MaskToRects(mask);
                SetMessage(Loc.F("msg.filled", mask.Count, rects.Count, kind.TrimStart('#')));
                return rects;
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, true);
                return null;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnMapPointPicked(object sender, ShapeEventArgs e)
        {
            RegionNode r = _selected;
            if (r == null)
            {
                SetMessage(Loc.T("msg.selectfirst"), true);
                return;
            }

            int z = LandZ(e.Cell.X, e.Cell.Y);
            if (e.Tool == MapTool.Go)
            {
                Change(Loc.F("hist.go", r.DisplayName), () => r.Go = new RegionPoint { X = e.Cell.X, Y = e.Cell.Y, Z = z });
            }
            else
            {
                string map = _mapView.MapId >= 0 && _mapView.MapId < RegionCatalog.Facets.Length && CurrentFacetMapOf(r) != _mapView.MapId
                    ? RegionCatalog.Facets[_mapView.MapId]
                    : null;
                Change(Loc.F("hist.entrance", r.DisplayName), () => r.Entrance = new RegionPoint { X = e.Cell.X, Y = e.Cell.Y, Z = z, Map = map });
            }

            SetTool(MapTool.Select);
        }

        private int CurrentFacetMapOf(RegionNode r)
        {
            _project.FindById(r.Id, out FacetRegions facet, out RegionNode _);
            return facet?.MapId ?? -1;
        }

        private void OnMapRegionPicked(object sender, RegionPickEventArgs e)
        {
            SelectRegion(e.Region, e.RectIndex, false);
        }

        private void OnMapEditStarting(object sender, EventArgs e)
        {
            _history.Record(_project, Loc.F("hist.drag", _selected?.DisplayName));
            _lastRecordKey = null;
        }

        private void OnMapEditFinished(object sender, EventArgs e)
        {
            ShowProperties();
            FillRects();
            UpdateTitle();
        }

        private void OnMapCursorMoved(object sender, RegionPickEventArgs e)
        {
            _lblPos.Text = $"{e.Cell.X}, {e.Cell.Y}";
            (int id, int z) = UltimaMapInfo.Land(_mapView.MapId, e.Cell.X, e.Cell.Y);
            _lblTile.Text = id < 0 ? "-" : $"0x{id:X4}  z {z}";
            List<RegionNode> here = _mapView.RegionAt(e.Cell, true);
            _lblHover.Text = here.Count == 0 ? string.Empty : string.Join(" › ", here.Select(r => r.DisplayName).Reverse());
        }

        #endregion

        #region Map context menu

        private Point _contextCell;

        private void OnMapMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _contextCell = _mapView.CursorCell;
            bool sel = _selected != null;
            _ctxMapSetGo.Enabled = sel;
            _ctxMapSetEntrance.Enabled = sel;
            _ctxMapDeleteRect.Enabled = sel && _selected.Rects.Any(r => r.Contains(_contextCell.X, _contextCell.Y));
            _ctxMapSelect.Enabled = _mapView.RegionAt(_contextCell, false).Count > 0;
            _ctxMapSendChar.Enabled = Client.Running;
            _ctxMapCopyPos.Text = Loc.F("ctx.copypos", _contextCell.X, _contextCell.Y);
        }

        private void OnClickMapSelect(object sender, EventArgs e)
        {
            List<RegionNode> hits = _mapView.RegionAt(_contextCell, false);
            if (hits.Count == 0)
            {
                return;
            }

            // several regions: the next one below the selected one
            int i = _selected == null ? -1 : hits.IndexOf(_selected);
            RegionNode next = hits[(i + 1) % hits.Count];
            SelectRegion(next, next.Rects.FindIndex(r => r.Contains(_contextCell.X, _contextCell.Y)), false);
        }

        private void OnClickMapSetGo(object sender, EventArgs e) => OnMapPointPicked(this, new ShapeEventArgs { Tool = MapTool.Go, Cell = _contextCell });

        private void OnClickMapSetEntrance(object sender, EventArgs e) => OnMapPointPicked(this, new ShapeEventArgs { Tool = MapTool.Entrance, Cell = _contextCell });

        private void OnClickMapDeleteRect(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            RegionRect rect = r?.Rects.FirstOrDefault(x => x.Contains(_contextCell.X, _contextCell.Y));
            if (rect == null || r.Locked)
            {
                return;
            }

            Change(Loc.F("hist.deletearea", r.DisplayName), () =>
            {
                r.Rects.Remove(rect);
                _selectedRect = -1;
            });
        }

        private void OnClickMapSendChar(object sender, EventArgs e) => SendCharTo(_contextCell.X, _contextCell.Y);

        private void OnClickMapCopyPos(object sender, EventArgs e)
        {
            int z = LandZ(_contextCell.X, _contextCell.Y);
            Clipboard.SetText($"{_contextCell.X} {_contextCell.Y} {z}");
            SetMessage(Loc.T("msg.copied"));
        }

        #endregion

        #region Client

        private bool ReadClient()
        {
            if (!Client.Running)
            {
                return false;
            }

            int x = 0, y = 0, z = 0, map = 0;
            Client.Calibrate();
            if (!Client.FindLocation(ref x, ref y, ref z, ref map))
            {
                return false;
            }

            _clientX = x;
            _clientY = y;
            _clientZ = z;
            _clientMap = map;
            _lblClient.Text = $"Client: {_clientX},{_clientY},{_clientZ} {MapRegistry.GetName(map)}";
            return true;
        }

        private void OnClickClientGoto(object sender, EventArgs e)
        {
            if (!ReadClient())
            {
                SetMessage(Loc.T("msg.noclient"), true);
                return;
            }

            SwitchToMap(_clientMap);
            _mapView.CenterOn(_clientX, _clientY);
            ApplyViewOptions();
        }

        private void OnClickSyncClient(object sender, EventArgs e)
        {
            if (_btnSyncClient.Checked)
            {
                _clientTimer.Start();
                OnClientTimer(this, EventArgs.Empty);
            }
            else
            {
                _clientTimer.Stop();
            }
        }

        private void OnClientTimer(object sender, EventArgs e)
        {
            int oldX = _clientX, oldY = _clientY, oldMap = _clientMap;
            if (!ReadClient())
            {
                _lblClient.Text = string.Empty;
                return;
            }

            if (oldX != _clientX || oldY != _clientY || oldMap != _clientMap)
            {
                SwitchToMap(_clientMap);
                _mapView.CenterOn(_clientX, _clientY);
                ApplyViewOptions();
            }
        }

        private void SendCharTo(int x, int y)
        {
            if (!Client.Running)
            {
                SetMessage(Loc.T("msg.noclient"), true);
                return;
            }

            int z = LandZ(x, y);
            string format = "{0} " + Options.MapArgs;
            Client.SendText(string.Format(format, Options.MapCmd, x, y, z, _mapView.MapId, _mapView.MapDefinition?.Name ?? string.Empty));
            SetMessage(Loc.F("msg.sent", x, y, z));
        }

        private void OnClickSendGo(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            if (r == null)
            {
                return;
            }

            if (r.Go != null)
            {
                SendCharTo(r.Go.X, r.Go.Y);
            }
            else if (!r.Bounds.IsEmpty)
            {
                Rectangle b = r.Rects.Count > 0 ? r.Rects[0].Bounds : r.Bounds;
                SendCharTo(b.X + (b.Width / 2), b.Y + (b.Height / 2));
            }
        }

        #endregion
    }
}
