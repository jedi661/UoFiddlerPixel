/***************************************************************************
 *
 * MultiEditor – layers, height ruler, structure tool, building parts,
 * map static selection (copy / move / delete), view rotation, decoration.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Forms;

namespace UoFiddler.Plugin.MultiEditor.UserControls
{
    public partial class MultiEditorControl
    {
        private readonly LayerManager _layers = new LayerManager();
        private readonly MapStaticsEditor _mapEditor = new MapStaticsEditor();
        private readonly List<(MapStatic s, Point cell)> _mapSelection = new List<(MapStatic, Point)>();

        private TabPage _layersTab;
        private TabPage _prefabTab;
        private ListView _layerList;
        private ListView _prefabList;
        private ImageList _prefabImages;
        private bool _layerListUpdating;

        private ToolStrip _toolBar2;
        private ToolStripButton _tbStruct;
        private ToolStripButton _tbMapSel;
        private ToolStripButton _tbDeco;
        private ToolStripComboBox _patternCombo;
        private ToolStripComboBox _styleCombo;

        private bool _drawDeco;
        private bool _showDecoTint = true;
        private bool _stackStructures = true;
        private bool _structWindows;
        private bool _structDoor = true;
        private bool _showRuler = true;
        private IStructurePattern _structurePattern;
        private bool _fillingCombos;
        private HouseStyle _structureStyle;

        public bool ShowDecoMarked => _showDecoTint;

        public bool IsTileHidden(MultiTile tile) => _layers.IsHidden(tile);

        public bool IsTileGhost(MultiTile tile) => _layers.IsGhost(tile);

        public bool IsTileLocked(MultiTile tile) => _layers.IsLocked(tile);

        #region Init / UI

        private void InitializeStructures()
        {
            _mapEditor.Changed += () => _background.ContentVersion++;
            _background.CellMapper = (x, y) => _compList?.ViewRotation > 0 ? _compList.ViewToCanonical(x, y) : new Point(x, y);
            StructureLibrary.Instance.SeedFromStyles(StyleKnowledge.Instance);
        }

        private void BuildStructureToolBar()
        {
            if (_toolBar2 != null)
            {
                splitContainer1.Panel2.Controls.Remove(_toolBar2);
                _toolBar2.Dispose();
            }

            _toolBar2 = new ToolStrip { Dock = DockStyle.Top, GripStyle = ToolStripGripStyle.Hidden };

            _tbStruct = new ToolStripButton(L.T("Struktur", "Structure"))
            {
                ToolTipText = L.T("Struktur-Werkzeug (B): Linie ziehen = Mauer, Viereck ziehen = Raum. Umschalt = Dach in andere Richtung / ohne Endpfosten",
                    "Structure tool (B): drag a line = wall, drag a rectangle = room. Shift = roof in the other direction / no end post")
            };
            _tbStruct.Click += (_, _) => SetExtTool(ExtTool.Structure);

            _patternCombo = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190, ToolTipText = L.T("Was gebaut wird", "What is built") };
            _patternCombo.DropDown += (_, _) => FillPatternCombo();
            _patternCombo.SelectedIndexChanged += (_, _) =>
            {
                if (_fillingCombos)
                {
                    return;
                }

                _structurePattern = _patternCombo.SelectedItem as PatternItem is { } p ? p.Pattern : null;
                SetExtTool(ExtTool.Structure);
            };
            _styleCombo = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150, ToolTipText = L.T("Baustil (aus dem Generator / gelernt)", "Building style (generator / learned)") };
            _styleCombo.DropDown += (_, _) => FillStyleCombo();
            _styleCombo.SelectedIndexChanged += (_, _) => _structureStyle = _styleCombo.SelectedItem as HouseStyle;

            var options = new ToolStripDropDownButton(L.T("Optionen", "Options"));
            var stack = new ToolStripMenuItem(L.T("Aufstocken (auf vorhandene Mauern bauen)", "Stack (build on top of existing walls)")) { CheckOnClick = true, Checked = _stackStructures };
            stack.CheckedChanged += (_, _) => _stackStructures = stack.Checked;
            var windows = new ToolStripMenuItem(L.T("Fenster einbauen", "Add windows")) { CheckOnClick = true, Checked = _structWindows };
            windows.CheckedChanged += (_, _) => _structWindows = windows.Checked;
            var door = new ToolStripMenuItem(L.T("Türöffnung (Süd)", "Door gap (south)")) { CheckOnClick = true, Checked = _structDoor };
            door.CheckedChanged += (_, _) => _structDoor = door.Checked;
            options.DropDownItems.AddRange(new ToolStripItem[]
            {
                stack, windows, door, new ToolStripSeparator(),
                Item(L.T("Linien-Muster aus Auswahl lernen…", "Learn line pattern from selection…"), (_, _) => LearnLinePattern()),
                Item(L.T("Linien-Muster löschen…", "Delete line pattern…"), (_, _) => DeleteLinePattern()),
                Item(L.T("Hausbau-Generator (Stile)…", "House generator (styles)…"), (_, _) => OpenGenerator())
            });

            _tbMapSel = new ToolStripButton(L.T("Map-Auswahl", "Map select"))
            {
                ToolTipText = L.T("Statics der Map auswählen (M): klicken = oberstes Static, ziehen = Bereich, Strg = hinzufügen. Rechtsklick = kopieren / übernehmen / verschieben / löschen",
                    "Select map statics (M): click = top static, drag = area, Ctrl = add. Right click = copy / take / move / delete")
            };
            _tbMapSel.Click += (_, _) => SetExtTool(ExtTool.MapSelect);

            _tbDeco = new ToolStripButton(L.T("Einrichtung", "Furniture"))
            {
                CheckOnClick = true,
                Checked = _drawDeco,
                ToolTipText = L.T("Neue Kacheln als Einrichtung zeichnen (E) – wird nicht in Multi.mul gespeichert, nur als Addon",
                    "Draw new tiles as furniture (E) – not stored in multi.mul, only as addon")
            };
            _tbDeco.CheckedChanged += (_, _) => _drawDeco = _tbDeco.Checked;

            var rotate = new ToolStripButton(L.T("↻ Ansicht", "↻ View")) { ToolTipText = L.T("Ansicht um 90° drehen (Export bleibt in Original-Ausrichtung)", "Rotate view by 90° (exports keep the original orientation)") };
            rotate.Click += (_, _) => RotateView();
            var ruler = new ToolStripButton(L.T("Lineal", "Ruler")) { CheckOnClick = true, Checked = _showRuler, ToolTipText = L.T("Höhenlineal am Rand (L)", "Height ruler at the border (L)") };
            ruler.CheckedChanged += (_, _) =>
            {
                _showRuler = ruler.Checked;
                pictureBoxMulti.Invalidate();
            };

            _toolBar2.Items.AddRange(new ToolStripItem[]
            {
                _tbStruct, _patternCombo, _styleCombo, options, new ToolStripSeparator(),
                _tbMapSel, new ToolStripSeparator(), _tbDeco, new ToolStripSeparator(), rotate, ruler
            });

            FillPatternCombo();
            FillStyleCombo();
            splitContainer1.Panel2.Controls.Add(_toolBar2);
            UpdateStructureButtons();
        }

        private sealed class PatternItem
        {
            public IStructurePattern Pattern { get; set; }

            public override string ToString() => Pattern.Name;
        }

        private void FillPatternCombo()
        {
            if (_patternCombo == null)
            {
                return;
            }

            string selected = _structurePattern?.Name;
            _fillingCombos = true;
            _patternCombo.Items.Clear();
            foreach (IStructurePattern p in StructureBuilder.GetPatterns())
            {
                _patternCombo.Items.Add(new PatternItem { Pattern = p });
            }

            int idx = 0;
            for (int i = 0; i < _patternCombo.Items.Count; ++i)
            {
                if (((PatternItem)_patternCombo.Items[i]).Pattern.Name == selected)
                {
                    idx = i;
                }
            }

            if (_patternCombo.Items.Count > 0)
            {
                _patternCombo.ComboBox.SelectedIndex = idx;
                _structurePattern = ((PatternItem)_patternCombo.Items[idx]).Pattern;
            }

            _fillingCombos = false;
        }

        private void FillStyleCombo()
        {
            if (_styleCombo == null)
            {
                return;
            }

            HouseStyle selected = _structureStyle;
            _styleCombo.Items.Clear();
            foreach (HouseStyle s in StyleKnowledge.Instance.Styles)
            {
                _styleCombo.Items.Add(s);
            }

            if (_styleCombo.Items.Count == 0)
            {
                return;
            }

            int idx = selected != null ? _styleCombo.Items.IndexOf(selected) : -1;
            _styleCombo.ComboBox.SelectedIndex = Math.Max(0, idx);
            _structureStyle = (HouseStyle)_styleCombo.SelectedItem;
        }

        private void SetExtTool(ExtTool tool)
        {
            _pasteMode = false;
            _dragging = false;
            _extTool = tool;
            BTN_Select.Checked = false;
            BTN_Draw.Checked = false;
            BTN_Remove.Checked = false;
            BTN_Z.Checked = false;
            BTN_Pipette.Checked = false;
            BTN_Trans.Checked = false;
            UpdateToolButtons();
            pictureBoxMulti.Invalidate();
        }

        private void UpdateStructureButtons()
        {
            if (_tbStruct == null)
            {
                return;
            }

            _tbStruct.Checked = _extTool == ExtTool.Structure;
            _tbMapSel.Checked = _extTool == ExtTool.MapSelect;
            _tbDeco.Checked = _drawDeco;
        }

        #endregion

        #region Layers tab

        private void BuildLayersTab()
        {
            if (_layersTab == null)
            {
                _layersTab = new TabPage { UseVisualStyleBackColor = true };
                TC_MultiEditorToolbox.TabPages.Add(_layersTab);
            }

            _layersTab.Text = L.T("Ebenen", "Layers");
            _layersTab.Controls.Clear();

            _layerList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                CheckBoxes = true,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false
            };
            _layerList.Columns.Add(L.T("Ebene", "Layer"), 90);
            _layerList.Columns.Add("Z", 70);
            _layerList.Columns.Add(L.T("Status", "State"), 60);
            _layerList.ItemChecked += LayerList_ItemChecked;
            _layerList.DoubleClick += (_, _) => UseLayerAsFloor();

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };

            void Btn(string text, string tip, Action a)
            {
                var b = new Button { Text = text, AutoSize = true, Margin = new Padding(2) };
                b.Click += (_, _) => a();
                toolTip1.SetToolTip(b, tip);
                buttons.Controls.Add(b);
            }

            var height = new NumericUpDown { Minimum = 1, Maximum = 60, Value = _layers.FloorHeight, Width = 45, Margin = new Padding(2, 5, 2, 2) };
            height.ValueChanged += (_, _) => _layers.FloorHeight = (int)height.Value;
            toolTip1.SetToolTip(height, L.T("Etagenhöhe für „Auto“", "Floor height for \"Auto\""));
            buttons.Controls.Add(height);
            Btn("Auto", L.T("Ebenen pro Etage neu anlegen", "Create one layer per floor"), () => AutoLayers(true));
            Btn(L.T("Neu", "New"), L.T("Eigene Ebene (Z-Bereich)", "Custom layer (Z range)"), NewLayer);
            Btn(L.T("Bearb.", "Edit"), L.T("Name / Z-Bereich ändern", "Change name / Z range"), EditLayer);
            Btn("X", L.T("Ebene löschen", "Delete layer"), DeleteLayer);
            Btn(L.T("Nur diese", "Only this"), L.T("Nur diese Ebene zeigen, darunter durchscheinend", "Show only this layer, layers below ghosted"), () => ShowOnlyLayer(true));
            Btn(L.T("Alle", "All"), L.T("Alle Ebenen zeigen", "Show all layers"), ShowAllLayers);
            Btn(L.T("Durchsch.", "Ghost"), L.T("Durchscheinend an/aus", "Ghost on/off"), () => ToggleLayer(l => l.Ghost = !l.Ghost));
            Btn(L.T("Sperren", "Lock"), L.T("Sperren an/aus (nicht wählbar)", "Lock on/off (not pickable)"), () => ToggleLayer(l => l.Locked = !l.Locked));
            Btn(L.T("Bau-Ebene", "Build level"), L.T("Virtuellen Boden auf diese Ebene setzen (Doppelklick)", "Set the virtual floor to this layer (double click)"), UseLayerAsFloor);

            _layersTab.Controls.Add(_layerList);
            _layersTab.Controls.Add(buttons);
            RefreshLayerList();
        }

        private void AutoLayers(bool force)
        {
            if (_compList == null)
            {
                return;
            }

            if (!force && _layers.Layers.Count > 0)
            {
                return;
            }

            _layers.CreateFloors(_compList.ZMin, _compList.ZMax, i => i == 0 ? L.T("Erdgeschoss", "Ground floor") : L.T($"{i}. Etage", $"Floor {i}"));
            RefreshLayerList();
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Called whenever a new multi was loaded / created (from UndoList_Clear)
        /// </summary>
        private void ResetForNewMulti()
        {
            _mapSelection.Clear();
            _houseMarkers.Clear();
            _walkIssues.Clear();
            _layers.Layers.Clear();
            _layers.DecoVisible = true;
            _layers.DecoGhost = false;
            _layers.DecoLocked = false;
            if (_compList != null)
            {
                AutoLayers(true);
            }
        }

        private EditorLayer SelectedLayer => _layerList?.SelectedItems.Count > 0 ? _layerList.SelectedItems[0].Tag as EditorLayer : null;

        private void RefreshLayerList()
        {
            if (_layerList == null)
            {
                return;
            }

            _layerListUpdating = true;
            object selected = SelectedLayer;
            _layerList.BeginUpdate();
            _layerList.Items.Clear();

            var deco = new ListViewItem(L.T("Einrichtung", "Furniture")) { Tag = "deco", Checked = _layers.DecoVisible, ForeColor = Color.SaddleBrown };
            deco.SubItems.Add(_compList != null ? $"{_compList.DecoCount}" : "0");
            deco.SubItems.Add(LayerState(_layers.DecoGhost, _layers.DecoLocked));
            _layerList.Items.Add(deco);

            foreach (EditorLayer l in _layers.Layers.OrderByDescending(l => l.ZMin))
            {
                int count = _compList?.RealTiles.Count(t => !t.IsDeco && _layers.GetLayer(t.Z) == l) ?? 0;
                var item = new ListViewItem(l.Name) { Tag = l, Checked = l.Visible };
                item.SubItems.Add($"{Math.Max(-128, l.ZMin)}…{Math.Min(127, l.ZMax)}");
                item.SubItems.Add(LayerState(l.Ghost, l.Locked) + (count > 0 ? $" ({count})" : string.Empty));
                _layerList.Items.Add(item);
                if (l == selected)
                {
                    item.Selected = true;
                }
            }

            _layerList.EndUpdate();
            _layerListUpdating = false;
        }

        private static string LayerState(bool ghost, bool locked)
        {
            string s = string.Empty;
            if (ghost)
            {
                s += L.T("D", "G");
            }

            if (locked)
            {
                s += L.T("S", "L");
            }

            return s;
        }

        private void LayerList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_layerListUpdating)
            {
                return;
            }

            if (e.Item.Tag is EditorLayer l)
            {
                l.Visible = e.Item.Checked;
            }
            else if ((e.Item.Tag as string) == "deco")
            {
                _layers.DecoVisible = e.Item.Checked;
            }

            pictureBoxMulti.Invalidate();
        }

        private void ToggleLayer(Action<EditorLayer> action)
        {
            if (_layerList?.SelectedItems.Count > 0 && (_layerList.SelectedItems[0].Tag as string) == "deco")
            {
                var tmp = new EditorLayer { Ghost = _layers.DecoGhost, Locked = _layers.DecoLocked };
                action(tmp);
                _layers.DecoGhost = tmp.Ghost;
                _layers.DecoLocked = tmp.Locked;
            }
            else if (SelectedLayer != null)
            {
                action(SelectedLayer);
            }

            RefreshLayerList();
            pictureBoxMulti.Invalidate();
        }

        private void NewLayer()
        {
            var d = new SimpleDialog(L.T("Neue Ebene", "New layer"))
                .AddText("name", "Name:", L.T("Ebene", "Layer") + " " + (_layers.Layers.Count + 1))
                .AddNumber("min", "Z min:", DrawFloorZ, -128, 127)
                .AddNumber("max", "Z max:", DrawFloorZ + 19, -128, 127);
            if (!d.ShowOk(this))
            {
                return;
            }

            _layers.Layers.Add(new EditorLayer { Name = d.GetText("name"), ZMin = Math.Min(d.GetInt("min"), d.GetInt("max")), ZMax = Math.Max(d.GetInt("min"), d.GetInt("max")) });
            RefreshLayerList();
            pictureBoxMulti.Invalidate();
        }

        private void EditLayer()
        {
            EditorLayer l = SelectedLayer;
            if (l == null)
            {
                return;
            }

            var d = new SimpleDialog(L.T("Ebene bearbeiten", "Edit layer"))
                .AddText("name", "Name:", l.Name)
                .AddNumber("min", "Z min:", l.ZMin, -128, 127)
                .AddNumber("max", "Z max:", l.ZMax, -128, 127);
            if (!d.ShowOk(this))
            {
                return;
            }

            l.Name = d.GetText("name");
            l.ZMin = Math.Min(d.GetInt("min"), d.GetInt("max"));
            l.ZMax = Math.Max(d.GetInt("min"), d.GetInt("max"));
            RefreshLayerList();
            pictureBoxMulti.Invalidate();
        }

        private void DeleteLayer()
        {
            EditorLayer l = SelectedLayer;
            if (l == null)
            {
                return;
            }

            _layers.Layers.Remove(l);
            RefreshLayerList();
            pictureBoxMulti.Invalidate();
        }

        private void ShowOnlyLayer(bool ghostBelow)
        {
            EditorLayer l = SelectedLayer;
            if (l == null)
            {
                return;
            }

            _layers.ShowOnly(l, ghostBelow);
            RefreshLayerList();
            pictureBoxMulti.Invalidate();
        }

        private void ShowAllLayers()
        {
            _layers.ShowAll();
            _layers.DecoVisible = true;
            _layers.DecoGhost = false;
            RefreshLayerList();
            pictureBoxMulti.Invalidate();
        }

        private void UseLayerAsFloor()
        {
            EditorLayer l = SelectedLayer;
            if (l == null)
            {
                return;
            }

            int z = Math.Max(-128, Math.Min(127, l.ZMin < -100 ? 0 : l.ZMin));
            numericUpDown_Floor.Value = z;
            BTN_Floor.Checked = true;
            BTN_Floor_Clicked(this, EventArgs.Empty);
            if (_compList != null)
            {
                _compList.SetFloorZ(z);
            }

            ScrollbarsSetValue();
            pictureBoxMulti.Invalidate();
        }

        #endregion

        #region Height ruler

        /// <summary>
        /// Vertical Z scale at the left border, measured at the cell below the mouse (or the origin)
        /// </summary>
        private void PaintRuler(Graphics g)
        {
            if (!_showRuler || _compList == null)
            {
                return;
            }

            GraphicsState state = g.Save();
            g.ResetTransform();
            g.SmoothingMode = SmoothingMode.None;

            int cx = _compList.OriginX;
            int cy = _compList.OriginY;
            if (_compList != null)
            {
                CellAt(_mouseLoc, BTN_Floor.Checked ? DrawFloorZ : 0, out int mx, out int my);
                if (InMulti(mx, my))
                {
                    cx = mx;
                    cy = my;
                }
            }

            int top = int.MinValue;
            int bottom = int.MaxValue;
            foreach (MultiTile t in _compList.RealTiles)
            {
                if (t.Id < TileData.ItemTable.Length)
                {
                    top = Math.Max(top, t.Z + TileData.ItemTable[t.Id].Height);
                }

                bottom = Math.Min(bottom, t.Z);
            }

            if (top == int.MinValue)
            {
                top = 20;
                bottom = 0;
            }

            int zFrom = Math.Min(bottom, 0) - 5;
            int zTo = Math.Max(top, 20) + 10;

            float ScreenY(int z)
            {
                Point p = BackgroundLayer.CellToScreen(_compList, cx, cy, z, hScrollBar.Value, vScrollBar.Value);
                return (p.Y + 22) * _zoom;
            }

            const int width = 48;
            using var back = new SolidBrush(Color.FromArgb(200, 30, 30, 36));
            g.FillRectangle(back, 0, 0, width, pictureBoxMulti.Height);

            // layer bands
            Color[] bands = { Color.FromArgb(70, 80, 160, 255), Color.FromArgb(70, 120, 220, 120) };
            int bi = 0;
            using var font = new Font("Arial", 7f);
            foreach (EditorLayer l in _layers.Layers.OrderBy(l => l.ZMin))
            {
                float y1 = ScreenY(Math.Max(zFrom, l.ZMin));
                float y2 = ScreenY(Math.Min(zTo, l.ZMax + 1));
                using var b = new SolidBrush(l.Visible ? bands[bi++ % 2] : Color.FromArgb(60, 120, 120, 120));
                g.FillRectangle(b, 0, Math.Min(y1, y2), 8, Math.Abs(y1 - y2));
            }

            using var pen = new Pen(Color.FromArgb(180, 220, 220, 220));
            int step = _zoom < 0.5f ? 10 : 5;
            int labelStep = _zoom < 0.5f ? 40 : _zoom < 1f ? 20 : 10;
            for (int z = (int)Math.Floor(zFrom / (double)step) * step; z <= zTo; z += step)
            {
                float y = ScreenY(z);
                if (y < -10 || y > pictureBoxMulti.Height + 10)
                {
                    continue;
                }

                bool label = z % labelStep == 0;
                g.DrawLine(pen, label ? 26 : 34, y, width, y);
                if (label)
                {
                    g.DrawString(z.ToString(), font, Brushes.White, 9, y - 6);
                }
            }

            // virtual floor, building top, hovered tile
            void Marker(int z, Color c, string text)
            {
                float y = ScreenY(z);
                using var p = new Pen(c, 2);
                g.DrawLine(p, 8, y, width, y);
                using var dash = new Pen(Color.FromArgb(90, c)) { DashStyle = DashStyle.Dash };
                g.DrawLine(dash, width, y, pictureBoxMulti.Width, y);
                using var b = new SolidBrush(c);
                g.DrawString(text, font, b, width + 2, y - 11);
            }

            if (BTN_Floor.Checked)
            {
                Marker(DrawFloorZ, Color.LimeGreen, L.T($"Bau-Ebene {DrawFloorZ}", $"Build level {DrawFloorZ}"));
            }

            Marker(top, Color.Orange, L.T($"Oberkante Z {top}  (Höhe {top - bottom})", $"Top Z {top}  (height {top - bottom})"));
            if (_hoverTile != null)
            {
                Marker(_hoverTile.Z, Color.Yellow, $"Z {_hoverTile.Z}");
            }

            g.Restore(state);
        }

        #endregion

        #region Structure tool

        private StructureContext CreateStructureContext()
        {
            return new StructureContext
            {
                Start = _dragStartCell,
                End = _dragEndCell,
                Z = _dragZ,
                Style = _structureStyle ?? StyleKnowledge.Instance.Styles.FirstOrDefault(s => s.IsUsable),
                Alternate = (ModifierKeys & Keys.Shift) != 0,
                DoorGap = _structDoor,
                Windows = _structWindows,
                DrawTile = _drawTile.Id == 0xFFFF ? -1 : _drawTile.Id,
                Hue = _drawHue,
                SwapMap = StyleKnowledge.Instance.GetRotationSwapMap()
            };
        }

        /// <summary>
        /// Tiles the structure tool would build now (stacking applied)
        /// </summary>
        private List<ImportTile> BuildStructure(out string name)
        {
            name = string.Empty;
            if (_compList == null)
            {
                return new List<ImportTile>();
            }

            if (_structurePattern == null)
            {
                FillPatternCombo();
            }

            IStructurePattern pattern = _structurePattern;
            if (pattern == null)
            {
                return new List<ImportTile>();
            }

            name = pattern.Name;
            StructureContext c = CreateStructureContext();

            if (_stackStructures)
            {
                IEnumerable<MultiTile> area = pattern.IsLine
                    ? (c.AlongX
                        ? _compList.GetTilesInRect(c.MinX, c.Start.Y, c.MaxX, c.Start.Y)
                        : _compList.GetTilesInRect(c.Start.X, c.MinY, c.Start.X, c.MaxY))
                    : _compList.GetTilesInRect(c.MinX, c.MinY, c.MaxX, c.MaxY);
                int? top = StructureBuilder.DetectTopZ(area.Where(t => !_layers.IsHidden(t)));
                if (top != null && top.Value > c.Z)
                {
                    c.Z = top.Value;
                }
            }

            List<ImportTile> tiles = pattern.Build(c);
            foreach (ImportTile t in tiles)
            {
                t.Deco = _drawDeco;
            }

            return StructureBuilder.RemoveExisting(tiles, _compList.Tiles);
        }

        private void FinishStructure()
        {
            List<ImportTile> tiles = WithSymmetry(BuildStructure(out string name));
            if (tiles.Count == 0)
            {
                ShowStatus(L.T("Nichts gebaut – Stil braucht Wand X/Y und Boden (Generator) bzw. Raum min. 2x2.", "Nothing built – style needs wall X/Y and floor (generator), room at least 2x2."));
                return;
            }

            // grow the multi if the structure reaches over the border (end posts, roof overhang)
            int needW = tiles.Max(t => t.X) + 1;
            int needH = tiles.Max(t => t.Y) + 1;
            if (needW > _compList.Width || needH > _compList.Height)
            {
                _compList.Resize(Math.Max(needW, _compList.Width), Math.Max(needH, _compList.Height));
            }

            int n = _compList.TilesAdd(tiles, name);
            AfterModelChanged(true);
            AutoLayersIfNeeded();
            ShowStatus($"{name}: {n} " + L.T("Kacheln", "tiles"));
        }

        private void AutoLayersIfNeeded()
        {
            if (_layers.NeedsMoreFloors(_compList.ZMax))
            {
                AutoLayers(true);
            }
            else
            {
                RefreshLayerList();
            }
        }

        private void PaintStructurePreview(Graphics g)
        {
            List<ImportTile> tiles = WithSymmetry(BuildStructure(out string name));
            foreach (ImportTile t in tiles.OrderBy(t => t.X + t.Y).ThenBy(t => t.Z).Take(1500))
            {
                DrawGhost(g, t.Id, t.Hue, t.X, t.Y, t.Z);
            }

            using var pen = new Pen(Color.Yellow, 1);
            DrawCellOutline(g, pen, _dragStartCell.X, _dragStartCell.Y, _dragZ);
            DrawCellOutline(g, pen, _dragEndCell.X, _dragEndCell.Y, _dragZ);
            int w = Math.Abs(_dragEndCell.X - _dragStartCell.X) + 1;
            int h = Math.Abs(_dragEndCell.Y - _dragStartCell.Y) + 1;
            int z = tiles.Count > 0 ? tiles.Min(t => t.Z) : _dragZ;
            SetToolStripText($"{name}: {w} x {h}  Z {z}  ({tiles.Count})");
        }

        private void LearnLinePattern()
        {
            List<MultiTile> work = GetWorkSet();
            if (work.Count == 0)
            {
                MessageBox.Show(this, L.T("Zuerst ein Stück Mauer auswählen (Rahmen-Auswahl, Taste 9), z.B. 3 Felder mit Fenster oder Zinnen.",
                    "Select a piece of wall first (box select, key 9), e.g. 3 cells with a window or battlements."), "Pattern");
                return;
            }

            var tiles = work.Select(t => new ImportTile { Id = t.Id, X = t.X, Y = t.Y, Z = t.Z, Hue = t.Hue }).ToList();
            int length = Math.Max(tiles.Max(t => t.X) - tiles.Min(t => t.X), tiles.Max(t => t.Y) - tiles.Min(t => t.Y)) + 1;
            var d = new SimpleDialog(L.T("Linien-Muster lernen", "Learn line pattern"))
                .AddText("name", "Name:", L.T("Mauer mit Fenster", "Wall with window"))
                .AddNumber("period", L.T("Wiederholung alle (Felder):", "Repeat every (cells):"), length, 1, length)
                .AddInfo(L.T("Beim Ziehen wird das Muster wiederholt. In die andere Richtung gezogen werden bekannte Wandteile (X ↔ Y) getauscht.",
                    "When dragging, the pattern repeats. Dragged in the other direction, known wall parts (X ↔ Y) are swapped."));
            if (!d.ShowOk(this))
            {
                return;
            }

            LinePatternData p = LearnedLinePattern.Learn(tiles, d.GetText("name"), d.GetInt("period"));
            if (p == null)
            {
                return;
            }

            StructureLibrary.Instance.LinePatterns.Add(p);
            StructureLibrary.Instance.Save();
            FillPatternCombo();
            ShowStatus(L.T($"Muster „{p.Name}“ gelernt ({p.Tiles.Count} Kacheln, Periode {p.Period})", $"Pattern \"{p.Name}\" learned ({p.Tiles.Count} tiles, period {p.Period})"));
        }

        private void DeleteLinePattern()
        {
            List<LinePatternData> list = StructureLibrary.Instance.LinePatterns;
            if (list.Count == 0)
            {
                return;
            }

            var d = new SimpleDialog(L.T("Linien-Muster löschen", "Delete line pattern"))
                .AddCombo("p", L.T("Muster:", "Pattern:"), list.Select(p => (object)p.Name), 0);
            if (!d.ShowOk(this) || d.GetIndex("p") < 0)
            {
                return;
            }

            list.RemoveAt(d.GetIndex("p"));
            StructureLibrary.Instance.Save();
            _structurePattern = null;
            FillPatternCombo();
        }

        #endregion

        #region Building parts (prefabs)

        private void BuildPrefabTab()
        {
            if (_prefabTab == null)
            {
                _prefabTab = new TabPage { UseVisualStyleBackColor = true };
                TC_MultiEditorToolbox.TabPages.Add(_prefabTab);
            }

            _prefabTab.Text = L.T("Bauteile", "Parts");
            _prefabTab.Controls.Clear();

            _prefabImages ??= new ImageList { ImageSize = new Size(64, 64), ColorDepth = ColorDepth.Depth32Bit };
            _prefabList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.LargeIcon,
                LargeImageList = _prefabImages,
                MultiSelect = false,
                HideSelection = false
            };
            _prefabList.ItemActivate += (_, _) => UseSelectedPrefab();
            _prefabList.Click += (_, _) => UseSelectedPrefab();
            toolTip1.SetToolTip(_prefabList, L.T("Klick = als Stempel setzen (R = drehen, Esc = Ende)", "Click = place as stamp (R = rotate, Esc = stop)"));

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };

            void Btn(string text, Action a)
            {
                var b = new Button { Text = text, AutoSize = true, Margin = new Padding(2) };
                b.Click += (_, _) => a();
                buttons.Controls.Add(b);
            }

            Btn(L.T("+ Auswahl", "+ Selection"), () => SavePrefab(GetWorkSet().Select(t => new ImportTile { Id = t.Id, X = t.X, Y = t.Y, Z = t.Z, Flag = t.Flag, Hue = t.Hue, Deco = t.IsDeco }).ToList()));
            Btn(L.T("+ Map", "+ Map"), () => SavePrefab(MapSelectionAsTiles()));
            Btn(L.T("+ Datei", "+ File"), SavePrefabFromFile);
            Btn(L.T("Umbenennen", "Rename"), RenamePrefab);
            Btn(L.T("Löschen", "Delete"), DeletePrefab);

            _prefabTab.Controls.Add(_prefabList);
            _prefabTab.Controls.Add(buttons);
            RefreshPrefabList();
        }

        private void RefreshPrefabList()
        {
            if (_prefabList == null)
            {
                return;
            }

            _prefabList.BeginUpdate();
            _prefabList.Items.Clear();
            _prefabList.Groups.Clear();
            foreach (Image img in _prefabImages.Images)
            {
                img.Dispose();
            }

            _prefabImages.Images.Clear();

            var groups = new Dictionary<string, ListViewGroup>();
            foreach (Prefab p in StructureLibrary.Instance.Prefabs)
            {
                string cat = string.IsNullOrEmpty(p.Category) ? L.T("Allgemein", "General") : p.Category;
                if (!groups.TryGetValue(cat, out ListViewGroup g))
                {
                    g = new ListViewGroup(cat);
                    groups[cat] = g;
                    _prefabList.Groups.Add(g);
                }

                var thumb = new Bitmap(64, 64);
                using (Bitmap preview = MultiPreview.Render(p.ToTiles(), 64, 64))
                using (Graphics gr = Graphics.FromImage(thumb))
                {
                    gr.Clear(Color.Transparent);
                    if (preview != null)
                    {
                        gr.DrawImage(preview, (64 - preview.Width) / 2, (64 - preview.Height) / 2, preview.Width, preview.Height);
                    }
                }

                _prefabImages.Images.Add(thumb);
                _prefabList.Items.Add(new ListViewItem(p.Name, _prefabImages.Images.Count - 1, g) { Tag = p, ToolTipText = $"{p.Tiles.Count} " + L.T("Kacheln", "tiles") });
            }

            _prefabList.EndUpdate();
        }

        private Prefab SelectedPrefab => _prefabList?.SelectedItems.Count > 0 ? _prefabList.SelectedItems[0].Tag as Prefab : null;

        private void UseSelectedPrefab()
        {
            Prefab p = SelectedPrefab;
            if (p == null || _compList == null)
            {
                return;
            }

            _clipboard = p.ToTiles();
            if (_clipboard.Count == 0)
            {
                return;
            }

            // stamps are placed relative to the build level / ground
            int minZ = _clipboard.Min(t => t.Z);
            int baseZ = BTN_Floor.Checked ? DrawFloorZ : 0;
            foreach (ImportTile t in _clipboard)
            {
                t.Z = t.Z - minZ + baseZ;
            }

            _pasteMode = true;
            UpdateInfo();
            ShowStatus(L.T($"Bauteil „{p.Name}“: klicken zum Setzen, R = drehen, Esc = Ende", $"Part \"{p.Name}\": click to place, R = rotate, Esc = stop"));
            pictureBoxMulti.Invalidate();
        }

        private void SavePrefab(List<ImportTile> tiles)
        {
            if (tiles == null || tiles.Count == 0)
            {
                MessageBox.Show(this, L.T("Nichts ausgewählt.", "Nothing selected."), L.T("Bauteil", "Part"));
                return;
            }

            var cats = StructureLibrary.Instance.Prefabs.Select(p => p.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            var d = new SimpleDialog(L.T("Als Bauteil speichern", "Save as building part"))
                .AddText("name", "Name:", L.T("Bauteil", "Part") + " " + (StructureLibrary.Instance.Prefabs.Count + 1))
                .AddText("cat", L.T("Kategorie:", "Category:"), cats.FirstOrDefault() ?? L.T("Treppen", "Stairs"))
                .AddInfo(L.T("Beispiele: Treppen, Türrahmen, Zinnen, Möbel, Deko. Vorhandene: ", "Examples: stairs, door frames, battlements, furniture, deco. Existing: ") + string.Join(", ", cats));
            if (!d.ShowOk(this))
            {
                return;
            }

            StructureLibrary.Instance.Prefabs.Add(Prefab.FromTiles(d.GetText("name"), d.GetText("cat").Trim(), tiles));
            StructureLibrary.Instance.Save();
            RefreshPrefabList();
            TC_MultiEditorToolbox.SelectedTab = _prefabTab;
        }

        private void SavePrefabFromFile()
        {
            using var dlg = new OpenFileDialog { Filter = MultiFormats.ImportFilter, Multiselect = true };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            foreach (string f in dlg.FileNames)
            {
                try
                {
                    ImportResult r = MultiFormats.Load(f);
                    if (r.Tiles.Count > 0)
                    {
                        StructureLibrary.Instance.Prefabs.Add(Prefab.FromTiles(r.Name, L.T("Importiert", "Imported"), r.Tiles));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, f + Environment.NewLine + ex.Message);
                }
            }

            StructureLibrary.Instance.Save();
            RefreshPrefabList();
        }

        private void RenamePrefab()
        {
            Prefab p = SelectedPrefab;
            if (p == null)
            {
                return;
            }

            var d = new SimpleDialog(L.T("Bauteil", "Part"))
                .AddText("name", "Name:", p.Name)
                .AddText("cat", L.T("Kategorie:", "Category:"), p.Category);
            if (!d.ShowOk(this))
            {
                return;
            }

            p.Name = d.GetText("name");
            p.Category = d.GetText("cat").Trim();
            StructureLibrary.Instance.Save();
            RefreshPrefabList();
        }

        private void DeletePrefab()
        {
            Prefab p = SelectedPrefab;
            if (p == null || MessageBox.Show(this, L.T($"„{p.Name}“ löschen?", $"Delete \"{p.Name}\"?"), L.T("Bauteil", "Part"), MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            StructureLibrary.Instance.Prefabs.Remove(p);
            StructureLibrary.Instance.Save();
            RefreshPrefabList();
        }

        private void RotateClipboard()
        {
            if (_clipboard == null || _clipboard.Count == 0)
            {
                return;
            }

            _clipboard = StructureBuilder.RotateCw(_clipboard, StyleKnowledge.Instance.GetRotationSwapMap());
            pictureBoxMulti.Invalidate();
        }

        #endregion

        #region Map static selection

        private void FinishMapSelect()
        {
            if (!_background.HasMap)
            {
                ShowStatus(L.T("Zuerst einen Map-Bereich laden (Menü Map).", "Load a map area first (menu Map)."));
                return;
            }

            bool add = (ModifierKeys & Keys.Control) != 0;
            if (!add)
            {
                _mapSelection.Clear();
            }

            if (_dragStartCell == _dragEndCell)
            {
                // click: top static of the cell (toggle with Ctrl)
                HuedTile? top = _background.GetTopStatic(_dragStartCell.X, _dragStartCell.Y);
                if (top != null)
                {
                    Point w = _background.CellToWorld(_dragStartCell.X, _dragStartCell.Y);
                    var ms = new MapStatic(w.X, w.Y, top.Value);
                    int idx = _mapSelection.FindIndex(m => m.s.Equals(ms));
                    if (idx >= 0)
                    {
                        _mapSelection.RemoveAt(idx);
                    }
                    else
                    {
                        _mapSelection.Add((ms, _dragStartCell));
                    }
                }
            }
            else
            {
                int maxZ = MaxHeightTrackBar.Value;
                for (int x = Math.Min(_dragStartCell.X, _dragEndCell.X); x <= Math.Max(_dragStartCell.X, _dragEndCell.X); ++x)
                {
                    for (int y = Math.Min(_dragStartCell.Y, _dragEndCell.Y); y <= Math.Max(_dragStartCell.Y, _dragEndCell.Y); ++y)
                    {
                        Point w = _background.CellToWorld(x, y);
                        foreach (HuedTile st in _background.GetStatics(x, y))
                        {
                            int rz = st.Z - _background.BaseZ;
                            if (rz < _minVisibleZ || rz > Math.Max(maxZ, rz))
                            {
                                continue;
                            }

                            var ms = new MapStatic(w.X, w.Y, st);
                            if (!_mapSelection.Any(m => m.s.Equals(ms)))
                            {
                                _mapSelection.Add((ms, new Point(x, y)));
                            }
                        }
                    }
                }
            }

            ShowStatus(L.T($"{_mapSelection.Count} Map-Statics ausgewählt – Rechtsklick für Aktionen", $"{_mapSelection.Count} map statics selected – right click for actions"));
        }

        private void PaintMapSelection(Graphics g)
        {
            if (_mapSelection.Count == 0 || _compList == null)
            {
                return;
            }

            using var pen = new Pen(Color.Cyan, 2) { DashStyle = DashStyle.Dash };
            foreach ((MapStatic s, Point cell) in _mapSelection)
            {
                Bitmap bmp = Art.GetStatic(s.Id);
                if (bmp == null)
                {
                    continue;
                }

                int px = ((cell.X - cell.Y) * 22) - (bmp.Width / 2) + MultiEditorComponentList.GapXMod - _compList.XMin - hScrollBar.Value;
                int py = ((cell.X + cell.Y) * 22) - ((s.Z - _background.BaseZ) * 4) - bmp.Height + MultiEditorComponentList.GapYMod - _compList.YMin - vScrollBar.Value;
                g.DrawRectangle(pen, px, py, bmp.Width, bmp.Height);
            }
        }

        /// <summary>
        /// Map selection as tiles in view cells (Z relative to the base Z)
        /// </summary>
        private List<ImportTile> MapSelectionAsTiles()
        {
            return _mapSelection.Select(m => new ImportTile
            {
                Id = m.s.Id,
                X = m.cell.X,
                Y = m.cell.Y,
                Z = m.s.Z - _background.BaseZ,
                Hue = m.s.Hue
            }).ToList();
        }

        private void CopyMapSelection()
        {
            List<ImportTile> tiles = MapSelectionAsTiles();
            if (tiles.Count == 0)
            {
                return;
            }

            int ax = tiles.Min(t => t.X);
            int ay = tiles.Min(t => t.Y);
            foreach (ImportTile t in tiles)
            {
                t.X -= ax;
                t.Y -= ay;
            }

            _clipboard = tiles;
            _pasteMode = true;
            UpdateInfo();
            ShowStatus(L.T("Map-Statics kopiert – klicken zum Einfügen ins Multi, R = drehen, Esc = Ende", "Map statics copied – click to paste into the multi, R = rotate, Esc = stop"));
            pictureBoxMulti.Invalidate();
        }

        private void TakeMapSelection(bool move)
        {
            if (_compList == null || _mapSelection.Count == 0)
            {
                return;
            }

            var inside = _mapSelection.Where(m => InMulti(m.cell.X, m.cell.Y)).ToList();
            List<ImportTile> tiles = inside.Select(m => new ImportTile
            {
                Id = m.s.Id,
                X = m.cell.X,
                Y = m.cell.Y,
                Z = m.s.Z - _background.BaseZ,
                Hue = m.s.Hue,
                Deco = _drawDeco
            }).ToList();

            int n = _compList.TilesAdd(tiles, move ? "Move map statics" : "Take map statics");
            if (move && n > 0)
            {
                _mapEditor.Remove(_background.Map, inside.Select(m => m.s).ToList(), "Move into multi");
            }

            _mapSelection.Clear();
            AfterModelChanged(true);
            ShowStatus(move
                ? L.T($"{n} Statics ins Multi verschoben (Map noch nicht gespeichert)", $"{n} statics moved into the multi (map not saved yet)")
                : L.T($"{n} Statics als Kacheln übernommen", $"{n} statics taken as tiles"));
        }

        private void DeleteMapSelection()
        {
            if (_mapSelection.Count == 0 || !_background.HasMap)
            {
                return;
            }

            if (MessageBox.Show(this, L.T($"{_mapSelection.Count} Statics von der Map entfernen? (Rückgängig: Menü Map)", $"Remove {_mapSelection.Count} statics from the map? (undo: menu Map)"),
                    "Map", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            int n = _mapEditor.Remove(_background.Map, _mapSelection.Select(m => m.s).ToList(), "Delete map statics");
            _mapSelection.Clear();
            pictureBoxMulti.Invalidate();
            ShowStatus(L.T($"{n} Statics entfernt (noch nicht gespeichert)", $"{n} statics removed (not saved yet)"));
        }

        private void UndoMapChange()
        {
            if (_mapEditor.Undo())
            {
                _mapSelection.Clear();
                pictureBoxMulti.Invalidate();
                ShowStatus(L.T("Map-Änderung rückgängig gemacht", "Map change undone"));
            }
        }

        private void SaveMapStatics()
        {
            if (_mapEditor.ChangedMaps.Count == 0)
            {
                MessageBox.Show(this, L.T("Keine geänderten Map-Statics.", "No changed map statics."), "Map");
                return;
            }

            if (MessageBox.Show(this, L.T($"statics/staidx von {_mapEditor.ChangedMaps.Count} Map(s) in\n{Options.OutputPath}\nspeichern?", $"Save statics/staidx of {_mapEditor.ChangedMaps.Count} map(s) to\n{Options.OutputPath}?"),
                    "Map", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                int n = _mapEditor.SaveAll(Options.OutputPath);
                ShowStatus(L.T($"{n} Map(s) gespeichert", $"{n} map(s) saved"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Map", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        #region View rotation / decoration

        private void RotateView()
        {
            if (_compList == null)
            {
                return;
            }

            RotateMarkersWithView(_compList.Height);
            _compList.Rotate90(StyleKnowledge.Instance.GetRotationSwapMap(), true);
            _mapSelection.Clear();
            AfterModelChanged();
            ShowStatus(L.T($"Ansicht gedreht ({_compList.ViewRotation * 90}°) – Speichern/Export erfolgt in Original-Ausrichtung",
                $"View rotated ({_compList.ViewRotation * 90}°) – save/export uses the original orientation"));
        }

        private void SetDecoOfWorkSet(bool deco)
        {
            List<MultiTile> work = GetWorkSet();
            if (_compList == null || work.Count == 0)
            {
                return;
            }

            _compList.TilesSetDeco(work, deco);
            AfterModelChanged(true);
            RefreshLayerList();
        }

        /// <summary>
        /// Extra entries for the multi context menu
        /// </summary>
        private void AddStructureContextItems(MultiTile t)
        {
            List<MultiTile> work = GetWorkSet();
            if (t != null && work.Count == 0)
            {
                work.Add(t);
            }

            if (work.Count > 0)
            {
                bool deco = work.All(w => w.IsDeco);
                var decoItem = Item(L.T("Einrichtung (nur Addon, nicht Multi.mul)", "Furniture (addon only, not multi.mul)"), (_, _) =>
                {
                    _compList.TilesSetDeco(work, !deco);
                    AfterModelChanged(true);
                    RefreshLayerList();
                });
                decoItem.Checked = deco;
                _multiContext.Items.Add(decoItem);
                _multiContext.Items.Add(Item(L.T("Als Bauteil speichern…", "Save as building part…"), (_, _) =>
                    SavePrefab(work.Select(w => new ImportTile { Id = w.Id, X = w.X, Y = w.Y, Z = w.Z, Flag = w.Flag, Hue = w.Hue, Deco = w.IsDeco }).ToList())));
                if (work.Count > 1)
                {
                    _multiContext.Items.Add(Item(L.T("Als Linien-Muster lernen…", "Learn as line pattern…"), (_, _) => LearnLinePattern()));
                }
            }

            if (t != null)
            {
                EditorLayer layer = _layers.GetLayer(t.Z);
                if (layer != null)
                {
                    var lm = new ToolStripMenuItem(L.T($"Ebene „{layer.Name}“", $"Layer \"{layer.Name}\""));
                    lm.DropDownItems.Add(Item(L.T("Ausblenden", "Hide"), (_, _) =>
                    {
                        layer.Visible = false;
                        RefreshLayerList();
                        pictureBoxMulti.Invalidate();
                    }));
                    lm.DropDownItems.Add(Item(L.T("Nur diese zeigen", "Show only this"), (_, _) =>
                    {
                        _layers.ShowOnly(layer, true);
                        RefreshLayerList();
                        pictureBoxMulti.Invalidate();
                    }));
                    lm.DropDownItems.Add(Item(L.T("Alle zeigen", "Show all"), (_, _) => ShowAllLayers()));
                    _multiContext.Items.Add(lm);
                }
            }

            if (_background.HasMap)
            {
                var map = new ToolStripMenuItem(L.T($"Map-Statics ({_mapSelection.Count} ausgewählt)", $"Map statics ({_mapSelection.Count} selected)"));
                map.DropDownItems.Add(Item(L.T("Oberstes Static hier auswählen", "Select top static here"), (_, _) =>
                {
                    _dragStartCell = _contextCell;
                    _dragEndCell = _contextCell;
                    FinishMapSelect();
                    pictureBoxMulti.Invalidate();
                }));
                bool any = _mapSelection.Count > 0;
                AddEnabledTo(map, Item(L.T("Kopieren (zum Einfügen)", "Copy (to paste)"), (_, _) => CopyMapSelection()), any);
                AddEnabledTo(map, Item(L.T("Als Kacheln übernehmen", "Take as tiles"), (_, _) => TakeMapSelection(false)), any);
                AddEnabledTo(map, Item(L.T("Ins Multi verschieben (von Map entfernen)", "Move into multi (remove from map)"), (_, _) => TakeMapSelection(true)), any);
                AddEnabledTo(map, Item(L.T("Von Map löschen", "Delete from map"), (_, _) => DeleteMapSelection()), any);
                AddEnabledTo(map, Item(L.T("Als Bauteil speichern…", "Save as building part…"), (_, _) => SavePrefab(MapSelectionAsTiles())), any);
                AddEnabledTo(map, Item(L.T("Auswahl aufheben", "Clear selection"), (_, _) =>
                {
                    _mapSelection.Clear();
                    pictureBoxMulti.Invalidate();
                }), any);
                map.DropDownItems.Add(new ToolStripSeparator());
                AddEnabledTo(map, Item(L.T("Map-Änderung rückgängig", "Undo map change") + (_mapEditor.CanUndo ? $" ({_mapEditor.UndoAction})" : string.Empty), (_, _) => UndoMapChange()), _mapEditor.CanUndo);
                AddEnabledTo(map, Item(L.T("Map-Statics speichern…", "Save map statics…"), (_, _) => SaveMapStatics()), _mapEditor.ChangedMaps.Count > 0);
                _multiContext.Items.Add(map);
            }

            if (_pasteMode)
            {
                _multiContext.Items.Add(Item(L.T("Stempel drehen (R)", "Rotate stamp (R)"), (_, _) => RotateClipboard()));
            }
        }

        private static void AddEnabledTo(ToolStripMenuItem parent, ToolStripItem item, bool enabled)
        {
            item.Enabled = enabled;
            parent.DropDownItems.Add(item);
        }

        #endregion
    }
}
