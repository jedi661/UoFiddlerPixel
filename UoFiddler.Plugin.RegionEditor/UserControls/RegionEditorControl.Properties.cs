/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Region editor – tab "Properties" and tab "Areas" (rectangles / polygons).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.RegionEditor.Classes;

namespace UoFiddler.Plugin.RegionEditor.UserControls
{
    public partial class RegionEditorControl
    {
        #region Properties tab

        private void ShowProperties()
        {
            RegionNode r = _selected;
            _updating = true;
            try
            {
                _tblProps.Enabled = r != null;
                _txtName.Text = r?.Name ?? string.Empty;
                _cmbType.Text = r?.Type ?? string.Empty;
                _numPriority.Value = Clamp(_numPriority, r?.Priority ?? 50);
                _cmbMusic.Text = r?.Music ?? string.Empty;

                _chkZ.Checked = r?.ZMin != null || r?.ZMax != null;
                _numZMin.Value = Clamp(_numZMin, r?.ZMin ?? -128);
                _numZMax.Value = Clamp(_numZMax, r?.ZMax ?? 127);

                _chkGo.Checked = r?.Go != null;
                _numGoX.Value = Clamp(_numGoX, r?.Go?.X ?? 0);
                _numGoY.Value = Clamp(_numGoY, r?.Go?.Y ?? 0);
                _numGoZ.Value = Clamp(_numGoZ, r?.Go?.Z ?? 0);

                _chkEntrance.Checked = r?.Entrance != null;
                _numEnX.Value = Clamp(_numEnX, r?.Entrance?.X ?? 0);
                _numEnY.Value = Clamp(_numEnY, r?.Entrance?.Y ?? 0);
                _numEnZ.Value = Clamp(_numEnZ, r?.Entrance?.Z ?? 0);
                _cmbEnMap.Text = r?.Entrance?.Map ?? string.Empty;

                _chkGuardsOff.Checked = r?.GuardsDisabled ?? false;
                _txtGuardsType.Text = r?.GuardsType ?? string.Empty;
                _txtRune.Text = r?.RuneName ?? string.Empty;
                _chkSmartNoHousing.CheckState = Tri(r?.SmartNoHousing);
                _chkLogoutDelay.CheckState = Tri(r?.LogoutDelayActive);
                _chkHidden.Checked = r?.Hidden ?? false;
                _chkLocked.Checked = r?.Locked ?? false;
                _txtNotes.Text = r?.Notes ?? string.Empty;

                Color color = r == null ? SystemColors.Control : RegionMapView.RegionColor(r);
                _btnColor.BackColor = color;
                _btnColorReset.Enabled = r?.Color != null;

                _gridProps.Rows.Clear();
                if (r != null)
                {
                    foreach (KeyValuePair<string, string> p in r.Properties)
                    {
                        _gridProps.Rows.Add(p.Key, p.Value);
                    }
                }

                UpdateEnabledState();
                UpdateRegionInfo();
            }
            finally
            {
                _updating = false;
            }
        }

        private void UpdateRegionInfo()
        {
            RegionNode r = _selected;
            if (r == null)
            {
                _lblRegionInfo.Text = Loc.T("info.noselection");
                return;
            }

            Rectangle b = r.Bounds;
            _lblRegionInfo.Text = Loc.F("info.area", r.Rects.Count + r.Polygons.Count, r.Area) +
                                  (b.IsEmpty ? string.Empty : $"\n{b.X},{b.Y} – {b.Right},{b.Bottom}") +
                                  (r.Children.Count > 0 ? "\n" + Loc.F("info.children", r.Children.Count) : string.Empty);
        }

        private void UpdateEnabledState()
        {
            _numZMin.Enabled = _numZMax.Enabled = _chkZ.Checked;
            _numGoX.Enabled = _numGoY.Enabled = _numGoZ.Enabled = _chkGo.Checked;
            _numEnX.Enabled = _numEnY.Enabled = _numEnZ.Enabled = _cmbEnMap.Enabled = _chkEntrance.Checked;
        }

        private static decimal Clamp(NumericUpDown n, int value) => Math.Max(n.Minimum, Math.Min(n.Maximum, value));

        private static CheckState Tri(bool? value) => value == null ? CheckState.Indeterminate : value.Value ? CheckState.Checked : CheckState.Unchecked;

        private static bool? Tri(CheckState state) => state == CheckState.Indeterminate ? (bool?)null : state == CheckState.Checked;

        private static string NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private void OnPropertyTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (sender == _txtName)
                {
                    OnNameValidated(sender, e);
                }
                else
                {
                    OnPropertyValueChanged(sender, e);
                }
            }
        }

        private void OnNameValidated(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            string name = _txtName.Text.Trim();
            if (_updating || r == null || name == r.Name)
            {
                return;
            }

            Change(Loc.F("hist.rename", r.DisplayName, name), () => r.Name = name);
        }

        private void OnTypeChanged(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            string type = _cmbType.Text.Trim();
            if (_updating || r == null || type == (r.Type ?? string.Empty))
            {
                return;
            }

            Change(Loc.F("hist.props", r.DisplayName), () => r.Type = type);
        }

        private void OnMusicChanged(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            string music = NullIfEmpty(_cmbMusic.Text);
            if (_updating || r == null || music == r.Music)
            {
                return;
            }

            Change(Loc.F("hist.props", r.DisplayName), () => r.Music = music);
        }

        /// <summary>all other fields of the properties tab</summary>
        private void OnPropertyValueChanged(object sender, EventArgs e)
        {
            UpdateEnabledState();
            RegionNode r = _selected;
            if (_updating || r == null)
            {
                return;
            }

            // nothing changed (e.g. Validated without edit) -> no undo step
            if (!PropertiesDiffer(r))
            {
                return;
            }

            Change(Loc.F("hist.props", r.DisplayName), () => ReadProperties(r), "props:" + r.Id + ":" + ((Control)sender).Name);
        }

        private bool PropertiesDiffer(RegionNode r)
        {
            RegionNode probe = History.Deserialize(History.Serialize(new RegionProject { Facets = { new FacetRegions { Regions = { r } } } })).Facets[0].Regions[0];
            ReadProperties(probe);
            string a = History.Serialize(new RegionProject { Facets = { new FacetRegions { Regions = { probe } } } });
            string b = History.Serialize(new RegionProject { Facets = { new FacetRegions { Regions = { r } } } });
            return a != b;
        }

        private void ReadProperties(RegionNode r)
        {
            r.Priority = (int)_numPriority.Value;
            if (_chkZ.Checked)
            {
                r.ZMin = (int)_numZMin.Value;
                r.ZMax = (int)_numZMax.Value;
            }
            else
            {
                r.ZMin = r.ZMax = null;
            }

            if (_chkGo.Checked)
            {
                if (r.Go == null)
                {
                    // first time: center of the region and the height of the land there
                    Rectangle b = r.Rects.Count > 0 ? r.Rects[0].Bounds : r.Bounds;
                    int x = b.IsEmpty ? (int)_numGoX.Value : b.X + (b.Width / 2), y = b.IsEmpty ? (int)_numGoY.Value : b.Y + (b.Height / 2);
                    r.Go = new RegionPoint { X = x, Y = y, Z = LandZ(x, y) };
                    _updating = true;
                    _numGoX.Value = Clamp(_numGoX, x);
                    _numGoY.Value = Clamp(_numGoY, y);
                    _numGoZ.Value = Clamp(_numGoZ, r.Go.Z ?? 0);
                    _updating = false;
                }
                else
                {
                    r.Go.X = (int)_numGoX.Value;
                    r.Go.Y = (int)_numGoY.Value;
                    r.Go.Z = (int)_numGoZ.Value;
                }
            }
            else
            {
                r.Go = null;
            }

            if (_chkEntrance.Checked)
            {
                r.Entrance = r.Entrance ?? new RegionPoint();
                r.Entrance.X = (int)_numEnX.Value;
                r.Entrance.Y = (int)_numEnY.Value;
                r.Entrance.Z = (int)_numEnZ.Value;
                r.Entrance.Map = NullIfEmpty(_cmbEnMap.Text);
            }
            else
            {
                r.Entrance = null;
            }

            r.GuardsDisabled = _chkGuardsOff.Checked;
            r.GuardsType = NullIfEmpty(_txtGuardsType.Text);
            r.RuneName = NullIfEmpty(_txtRune.Text);
            r.SmartNoHousing = Tri(_chkSmartNoHousing.CheckState);
            r.LogoutDelayActive = Tri(_chkLogoutDelay.CheckState);
            r.Hidden = _chkHidden.Checked;
            r.Locked = _chkLocked.Checked;
            r.Notes = NullIfEmpty(_txtNotes.Text);
        }

        private int LandZ(int x, int y)
        {
            try
            {
                return UltimaMapInfo.Land(_mapView.MapId, x, y).Z;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private void OnClickPickGo(object sender, EventArgs e)
        {
            SetTool(MapTool.Go);
            SetMessage(Loc.T("msg.pickgo"));
        }

        private void OnClickPickEntrance(object sender, EventArgs e)
        {
            SetTool(MapTool.Entrance);
            SetMessage(Loc.T("msg.pickentrance"));
        }

        private void OnClickColor(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            if (r == null)
            {
                return;
            }

            _colorDialog.Color = RegionMapView.RegionColor(r);
            if (_colorDialog.ShowDialog(this) == DialogResult.OK)
            {
                int argb = _colorDialog.Color.ToArgb();
                Change(Loc.F("hist.props", r.DisplayName), () => r.Color = argb);
            }
        }

        private void OnClickColorReset(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            if (r?.Color != null)
            {
                Change(Loc.F("hist.props", r.DisplayName), () => r.Color = null);
            }
        }

        private void OnGridPropsEdited(object sender, DataGridViewCellEventArgs e) => ReadGridProps();

        private void OnGridPropsRowDeleted(object sender, DataGridViewRowEventArgs e) => ReadGridProps();

        private void ReadGridProps()
        {
            RegionNode r = _selected;
            if (_updating || r == null)
            {
                return;
            }

            var values = new Dictionary<string, string>();
            foreach (DataGridViewRow row in _gridProps.Rows)
            {
                string key = (row.Cells[0].Value as string)?.Trim();
                if (!row.IsNewRow && !string.IsNullOrEmpty(key))
                {
                    values[key] = (row.Cells[1].Value as string) ?? string.Empty;
                }
            }

            if (values.Count == r.Properties.Count && values.All(v => r.Properties.TryGetValue(v.Key, out string old) && old == v.Value))
            {
                return;
            }

            BeginInvoke(new Action(() => Change(Loc.F("hist.props", r.DisplayName), () => r.Properties = values)));
        }

        #endregion

        #region Areas tab

        private void FillRects()
        {
            RegionNode r = _selected;
            _updating = true;
            _listRects.BeginUpdate();
            try
            {
                _listRects.Items.Clear();
                if (r != null)
                {
                    for (int i = 0; i < r.Rects.Count; i++)
                    {
                        RegionRect rect = r.Rects[i];
                        _listRects.Items.Add(new ListViewItem(new[]
                        {
                            (i + 1).ToString(CultureInfo.InvariantCulture), rect.X.ToString(CultureInfo.InvariantCulture), rect.Y.ToString(CultureInfo.InvariantCulture),
                            rect.Width.ToString(CultureInfo.InvariantCulture), rect.Height.ToString(CultureInfo.InvariantCulture),
                            rect.ZMin.HasValue || rect.ZMax.HasValue ? $"{rect.ZMin?.ToString() ?? "…"}..{rect.ZMax?.ToString() ?? "…"}" : string.Empty
                        }) { Tag = rect, Selected = i == _selectedRect });
                    }

                    for (int i = 0; i < r.Polygons.Count; i++)
                    {
                        Rectangle b = r.Polygons[i].Bounds;
                        _listRects.Items.Add(new ListViewItem(new[]
                        {
                            "P" + (i + 1).ToString(CultureInfo.InvariantCulture), b.X.ToString(CultureInfo.InvariantCulture), b.Y.ToString(CultureInfo.InvariantCulture),
                            b.Width.ToString(CultureInfo.InvariantCulture), b.Height.ToString(CultureInfo.InvariantCulture),
                            Loc.F("rect.polygon", r.Polygons[i].Points.Count)
                        }) { Tag = r.Polygons[i], ForeColor = ThemeManager.Palette.SubText });
                    }
                }

                bool sel = r != null;
                _btnRectAdd.Enabled = sel;
                _btnRectApply.Enabled = sel && _selectedRect >= 0;
                _btnRectDelete.Enabled = sel;
                _btnRectOptimize.Enabled = sel && r.Rects.Count > 1;
                _btnPolyToRects.Enabled = sel && r.Polygons.Count > 0;

                if (sel && _selectedRect >= 0 && _selectedRect < r.Rects.Count)
                {
                    RegionRect rect = r.Rects[_selectedRect];
                    _numRX.Value = Clamp(_numRX, rect.X);
                    _numRY.Value = Clamp(_numRY, rect.Y);
                    _numRW.Value = Clamp(_numRW, rect.Width);
                    _numRH.Value = Clamp(_numRH, rect.Height);
                    _chkRZ.Checked = rect.ZMin.HasValue || rect.ZMax.HasValue;
                    _numRZ1.Value = Clamp(_numRZ1, rect.ZMin ?? -128);
                    _numRZ2.Value = Clamp(_numRZ2, rect.ZMax ?? 127);
                }
            }
            finally
            {
                _listRects.EndUpdate();
                _updating = false;
            }
        }

        private void OnRectSelected(object sender, EventArgs e)
        {
            if (_updating || _selected == null)
            {
                return;
            }

            ListViewItem item = _listRects.SelectedItems.Count > 0 ? _listRects.SelectedItems[0] : null;
            _selectedRect = item?.Tag is RegionRect rect ? _selected.Rects.IndexOf(rect) : -1;
            _mapView.SelectedRectIndex = _selectedRect;
            _btnRectApply.Enabled = _selectedRect >= 0;
            if (_selectedRect >= 0)
            {
                _updating = true;
                RegionRect r = _selected.Rects[_selectedRect];
                _numRX.Value = Clamp(_numRX, r.X);
                _numRY.Value = Clamp(_numRY, r.Y);
                _numRW.Value = Clamp(_numRW, r.Width);
                _numRH.Value = Clamp(_numRH, r.Height);
                _chkRZ.Checked = r.ZMin.HasValue || r.ZMax.HasValue;
                _numRZ1.Value = Clamp(_numRZ1, r.ZMin ?? -128);
                _numRZ2.Value = Clamp(_numRZ2, r.ZMax ?? 127);
                _updating = false;
            }

            _mapView.Invalidate();
        }

        private void OnRectDoubleClick(object sender, EventArgs e)
        {
            if (_listRects.SelectedItems.Count == 0)
            {
                return;
            }

            object tag = _listRects.SelectedItems[0].Tag;
            Rectangle b = tag is RegionRect r ? r.Bounds : tag is RegionPolygon p ? p.Bounds : Rectangle.Empty;
            if (!b.IsEmpty)
            {
                _mapView.ZoomTo(b);
            }
        }

        private RegionRect RectFromEditor() => new RegionRect
        {
            X = (int)_numRX.Value,
            Y = (int)_numRY.Value,
            Width = (int)_numRW.Value,
            Height = (int)_numRH.Value,
            ZMin = _chkRZ.Checked ? (int)_numRZ1.Value : (int?)null,
            ZMax = _chkRZ.Checked ? (int)_numRZ2.Value : (int?)null
        };

        private void OnClickRectAdd(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            if (r == null)
            {
                return;
            }

            RegionRect rect = RectFromEditor();
            Change(Loc.F("hist.addarea", r.DisplayName), () =>
            {
                r.Rects.Add(rect);
                _selectedRect = r.Rects.Count - 1;
            });
        }

        private void OnClickRectApply(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            if (r == null || _selectedRect < 0 || _selectedRect >= r.Rects.Count)
            {
                return;
            }

            RegionRect values = RectFromEditor();
            RegionRect target = r.Rects[_selectedRect];
            Change(Loc.F("hist.editarea", r.DisplayName), () =>
            {
                target.X = values.X;
                target.Y = values.Y;
                target.Width = values.Width;
                target.Height = values.Height;
                target.ZMin = values.ZMin;
                target.ZMax = values.ZMax;
            });
        }

        private void OnClickRectDelete(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            if (r == null || _listRects.SelectedItems.Count == 0)
            {
                return;
            }

            object tag = _listRects.SelectedItems[0].Tag;
            Change(Loc.F("hist.deletearea", r.DisplayName), () =>
            {
                if (tag is RegionRect rect)
                {
                    r.Rects.Remove(rect);
                }
                else if (tag is RegionPolygon poly)
                {
                    r.Polygons.Remove(poly);
                }

                _selectedRect = -1;
            });
        }

        /// <summary>Delete key on the map: the selected rectangle, otherwise the region</summary>
        private void DeleteSelectedArea()
        {
            RegionNode r = _selected;
            if (r == null || r.Locked)
            {
                return;
            }

            if (_selectedRect >= 0 && _selectedRect < r.Rects.Count && r.Rects.Count + r.Polygons.Count > 1)
            {
                RegionRect rect = r.Rects[_selectedRect];
                Change(Loc.F("hist.deletearea", r.DisplayName), () =>
                {
                    r.Rects.Remove(rect);
                    _selectedRect = -1;
                });
                return;
            }

            DeleteRegion(r);
        }

        private void OnClickRectOptimize(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            if (r == null || r.Rects.Count < 2)
            {
                return;
            }

            List<RegionRect> optimized = Geometry.Optimize(r.Rects);
            if (optimized.Count >= r.Rects.Count)
            {
                SetMessage(Loc.T("msg.nooptimize"));
                return;
            }

            int before = r.Rects.Count;
            Change(Loc.F("hist.optimize", r.DisplayName), () =>
            {
                r.Rects = optimized;
                _selectedRect = -1;
            });
            SetMessage(Loc.F("msg.optimized", before, optimized.Count));
        }

        private void OnClickPolyToRects(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            if (r == null || r.Polygons.Count == 0)
            {
                return;
            }

            Change(Loc.F("hist.polytorects", r.DisplayName), () =>
            {
                foreach (RegionPolygon p in r.Polygons)
                {
                    foreach (RegionRect rect in Geometry.PolygonToRects(p.Points))
                    {
                        rect.ZMin = p.ZMin;
                        rect.ZMax = p.ZMax;
                        r.Rects.Add(rect);
                    }
                }

                r.Polygons.Clear();
                r.Rects = Geometry.Optimize(r.Rects);
            });
        }

        #endregion
    }
}
