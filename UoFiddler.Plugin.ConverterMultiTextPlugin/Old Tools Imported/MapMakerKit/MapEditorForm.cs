// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    /// <summary>
    /// Paint editor for Terrain.bmp / Altitude.bmp: brush, fill, rectangle, picker, height tools,
    /// stamps (patterns from the template folder), undo/redo and transition check / repair.
    /// </summary>
    internal partial class MapEditorForm : Form
    {
        private enum Tool
        {
            Brush,
            Fill,
            Rectangle,
            Picker,
            Raise,
            Lower,
            SetHeight,
            Smooth,
            Stamp
        }

        private readonly TerrainPalette _terrains;
        private readonly AltitudePalette _altitudes;
        private readonly List<TerrainChoice> _allTerrains;
        private MapDocument _doc;
        private UndoBuffer _undo;
        private Tool _tool = Tool.Brush;
        private int _selectedTerrain = TerrainIds.Grass;
        private bool _modified;
        private Point _rectStart;
        private bool[] _strokeMask;
        private readonly List<int> _strokeTouched = new List<int>();
        private byte[] _stamp;
        private int _stampWidth;
        private int _stampHeight;
        private Point _lastStampAt = new Point(int.MinValue, int.MinValue);
        private static TransitionSet _transitions;

        /// <summary>Path of the terrain image (null = not saved yet).</summary>
        public string TerrainPath { get; private set; }

        /// <summary>Path of the altitude image.</summary>
        public string AltitudePath { get; private set; }

        public MapDocument Document => _doc;

        /// <summary>True when the map has changes that are not saved yet.</summary>
        public bool IsModified => _modified;

        /// <summary>
        /// False when the map is shared with the Map Maker: the editor then does not ask on close,
        /// the Map Maker keeps the (unsaved) changes.
        /// </summary>
        public bool ConfirmOnClose { get; set; } = true;

        /// <summary>Raised after the map was saved or replaced (the Map Maker refreshes its preview).</summary>
        public event EventHandler DocumentChanged;

        public MapEditorForm(MapDocument doc, TerrainPalette terrains, AltitudePalette altitudes, string terrainPath, string altitudePath)
        {
            InitializeComponent();
            _terrains = terrains;
            _altitudes = altitudes;
            _allTerrains = TerrainChoice.FromPalette(terrains);
            canvas.TerrainPalette = terrains;
            canvas.AltitudePalette = altitudes;
            cbView.SelectedIndex = 0;
            FillTerrainList(string.Empty);
            SetDocument(doc ?? MapDocument.CreateBlank(1024, 1024, TerrainIds.Grass, altitudes.DefaultLandKey), terrainPath, altitudePath);
            TrackBrush_ValueChanged(this, EventArgs.Empty);
            UpdateUndoButtons();
        }

        private void SetDocument(MapDocument doc, string terrainPath, string altitudePath)
        {
            _doc = doc;
            _undo = new UndoBuffer(doc);
            _strokeMask = null;
            TerrainPath = terrainPath;
            AltitudePath = altitudePath;
            canvas.Document = doc;
            _modified = false;
            UpdateTitle();
            UpdateUndoButtons();
        }

        private void UpdateTitle()
        {
            string name = string.IsNullOrEmpty(TerrainPath) ? "(not saved)" : TerrainPath;
            Text = string.Format("Map Editor - {0}x{1} - {2}{3}", _doc.Width, _doc.Height, name, _modified ? " *" : string.Empty);
        }

        private void MarkModified()
        {
            if (!_modified)
            {
                _modified = true;
                UpdateTitle();
            }

            UpdateUndoButtons();
        }

        private void UpdateUndoButtons()
        {
            btnUndo.Enabled = _undo != null && _undo.CanUndo;
            btnRedo.Enabled = _undo != null && _undo.CanRedo;
        }

        // ================================================================ terrain list

        private void FillTerrainList(string filter)
        {
            listTerrain.BeginUpdate();
            listTerrain.Items.Clear();
            foreach (TerrainChoice choice in _allTerrains)
            {
                if (string.IsNullOrEmpty(filter) || choice.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    listTerrain.Items.Add(choice);
                }
            }

            listTerrain.EndUpdate();
            SelectTerrainInList(_selectedTerrain);
        }

        private void SelectTerrainInList(int id)
        {
            for (int i = 0; i < listTerrain.Items.Count; i++)
            {
                if (((TerrainChoice)listTerrain.Items[i]).Id == id)
                {
                    listTerrain.SelectedIndex = i;
                    return;
                }
            }
        }

        private void TxtTerrainFilter_TextChanged(object sender, EventArgs e) => FillTerrainList(txtTerrainFilter.Text.Trim());

        private void ListTerrain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listTerrain.SelectedItem is TerrainChoice choice)
            {
                _selectedTerrain = choice.Id;
            }
        }

        // ================================================================ tool options

        private void Tool_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is RadioButton rb) || !rb.Checked)
            {
                return;
            }

            _tool = rb == rbFill ? Tool.Fill
                : rb == rbRectangle ? Tool.Rectangle
                : rb == rbPicker ? Tool.Picker
                : rb == rbRaise ? Tool.Raise
                : rb == rbLower ? Tool.Lower
                : rb == rbSetHeight ? Tool.SetHeight
                : rb == rbSmooth ? Tool.Smooth
                : rb == rbStamp ? Tool.Stamp
                : Tool.Brush;

            bool brushTool = _tool == Tool.Brush || _tool == Tool.Raise || _tool == Tool.Lower || _tool == Tool.SetHeight || _tool == Tool.Smooth;
            canvas.BrushRadius = brushTool ? trackBrush.Value : 0;
            canvas.Invalidate();
            if (_tool == Tool.Stamp && _stamp == null)
            {
                BtnChooseStamp_Click(this, EventArgs.Empty);
            }
        }

        private void TrackBrush_ValueChanged(object sender, EventArgs e)
        {
            lblBrushSize.Text = string.Format("{0} tiles", (trackBrush.Value * 2) + 1);
            if (_tool != Tool.Fill && _tool != Tool.Rectangle && _tool != Tool.Picker && _tool != Tool.Stamp)
            {
                canvas.BrushRadius = trackBrush.Value;
            }

            canvas.Invalidate();
        }

        private void ChkSquare_CheckedChanged(object sender, EventArgs e)
        {
            canvas.SquareBrush = chkSquare.Checked;
            canvas.Invalidate();
        }

        private void CbView_SelectedIndexChanged(object sender, EventArgs e)
        {
            canvas.Mode = cbView.SelectedIndex == 1 ? MapDocument.RenderMode.Altitude
                : cbView.SelectedIndex == 2 ? MapDocument.RenderMode.Relief
                : MapDocument.RenderMode.Terrain;
            canvas.RefreshMap();
        }

        private void BtnGrid_CheckedChanged(object sender, EventArgs e)
        {
            canvas.ShowGrid = btnGrid.Checked;
            canvas.Invalidate();
        }

        private void BtnZoomIn_Click(object sender, EventArgs e) => canvas.SetZoom(canvas.Zoom * 1.5);

        private void BtnZoomOut_Click(object sender, EventArgs e) => canvas.SetZoom(canvas.Zoom / 1.5);

        private void BtnZoomFit_Click(object sender, EventArgs e) => canvas.ZoomToFit();

        private void BtnZoom1_Click(object sender, EventArgs e)
        {
            Point center = canvas.MouseTile.X >= 0 ? canvas.MouseTile : new Point(_doc.Width / 2, _doc.Height / 2);
            canvas.SetZoom(8);
            canvas.CenterOn(center.X, center.Y);
        }

        private void Canvas_ViewChanged(object sender, EventArgs e)
        {
            lblZoom.Text = canvas.Zoom >= 1 ? string.Format("Zoom {0:0.#}x", canvas.Zoom) : string.Format("Zoom 1:{0:0.#}", 1 / canvas.Zoom);
        }

        private void Canvas_TileHover(object sender, TileMouseEventArgs e)
        {
            if (!_doc.Contains(e.X, e.Y))
            {
                lblPosition.Text = "X: -  Y: -";
                lblTileInfo.Text = string.Empty;
                return;
            }

            int i = _doc.Index(e.X, e.Y);
            byte t = _doc.Terrain[i];
            byte a = _doc.Altitude[i];
            lblPosition.Text = string.Format("X: {0}  Y: {1}", e.X, e.Y);
            lblTileInfo.Text = string.Format("{0}   |   z {1} ({2})", _terrains.DisplayName(t), _altitudes.Altitude[a], _altitudes.TypeOf(a));
        }

        // ================================================================ painting

        private void Canvas_ToolDown(object sender, TileMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            switch (_tool)
            {
                case Tool.Picker:
                    Pick(e.X, e.Y);
                    return;
                case Tool.Fill:
                    BeginStroke();
                    FloodFill(e.X, e.Y);
                    EndStroke();
                    return;
                case Tool.Rectangle:
                    _rectStart = new Point(e.X, e.Y);
                    canvas.Selection = NormalizedRect(_rectStart, _rectStart);
                    canvas.Invalidate();
                    return;
                case Tool.Stamp:
                    BeginStroke();
                    Stamp(e.X, e.Y);
                    return;
                default:
                    BeginStroke();
                    ApplyBrush(e.X, e.Y);
                    return;
            }
        }

        private void Canvas_ToolMove(object sender, TileMouseEventArgs e)
        {
            switch (_tool)
            {
                case Tool.Rectangle:
                    canvas.Selection = NormalizedRect(_rectStart, new Point(e.X, e.Y));
                    canvas.Invalidate();
                    return;
                case Tool.Picker:
                case Tool.Fill:
                    return;
                case Tool.Stamp:
                    // drag = repeat the stamp when the cursor moved at least one stamp width
                    if (_stamp != null && (Math.Abs(e.X - _lastStampAt.X) >= _stampWidth * (int)numStampScale.Value || Math.Abs(e.Y - _lastStampAt.Y) >= _stampHeight * (int)numStampScale.Value))
                    {
                        Stamp(e.X, e.Y);
                    }

                    return;
                default:
                    ApplyBrush(e.X, e.Y);
                    return;
            }
        }

        private void Canvas_ToolUp(object sender, TileMouseEventArgs e)
        {
            if (_tool == Tool.Rectangle && !canvas.Selection.IsEmpty)
            {
                Rectangle r = Rectangle.Intersect(canvas.Selection, new Rectangle(0, 0, _doc.Width, _doc.Height));
                BeginStroke();
                for (int y = r.Top; y < r.Bottom; y++)
                {
                    for (int x = r.Left; x < r.Right; x++)
                    {
                        PaintTile(x, y);
                    }
                }

                canvas.Selection = Rectangle.Empty;
            }

            EndStroke();
        }

        private static Rectangle NormalizedRect(Point a, Point b)
        {
            int x = Math.Min(a.X, b.X);
            int y = Math.Min(a.Y, b.Y);
            return new Rectangle(x, y, Math.Abs(a.X - b.X) + 1, Math.Abs(a.Y - b.Y) + 1);
        }

        private bool _strokeOpen;

        private void BeginStroke()
        {
            _undo.Begin();
            _strokeOpen = true;
            if (_strokeMask == null || _strokeMask.Length != _doc.Terrain.Length)
            {
                _strokeMask = new bool[_doc.Terrain.Length];
            }
        }

        private void EndStroke()
        {
            if (!_strokeOpen)
            {
                return;
            }

            _strokeOpen = false;
            foreach (int i in _strokeTouched)
            {
                _strokeMask[i] = false;
            }

            _strokeTouched.Clear();
            _undo.End();
            canvas.RefreshMap();
            MarkModified();
        }

        private void ApplyBrush(int cx, int cy)
        {
            if (!_strokeOpen)
            {
                return;
            }

            int r = trackBrush.Value;
            bool square = chkSquare.Checked;
            int strength = (int)numStrength.Value;
            int target = (int)numTargetHeight.Value;
            List<(int Index, byte Key)> smoothResult = _tool == Tool.Smooth ? new List<(int, byte)>() : null;

            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (!square && (dx * dx) + (dy * dy) > (r * r) + r)
                    {
                        continue;
                    }

                    int x = cx + dx;
                    int y = cy + dy;
                    if (!_doc.Contains(x, y))
                    {
                        continue;
                    }

                    int i = _doc.Index(x, y);
                    switch (_tool)
                    {
                        case Tool.Brush:
                            PaintTile(x, y);
                            break;

                        case Tool.Raise:
                        case Tool.Lower:
                        case Tool.SetHeight:
                            if (_strokeMask[i])
                            {
                                break; // every tile only once per stroke
                            }

                            MarkStroke(i);
                            _undo.Touch(x, y);
                            int current = _altitudes.Altitude[_doc.Altitude[i]];
                            int value = _tool == Tool.Raise ? current + strength : _tool == Tool.Lower ? current - strength : target;
                            _doc.Altitude[i] = _altitudes.WithAltitude(_doc.Altitude[i], value);
                            break;

                        case Tool.Smooth:
                            smoothResult.Add((i, SmoothedKey(x, y)));
                            break;
                    }
                }
            }

            if (smoothResult != null)
            {
                foreach ((int index, byte key) in smoothResult)
                {
                    _undo.Touch(index % _doc.Width, index / _doc.Width);
                    _doc.Altitude[index] = key;
                }
            }

            canvas.RefreshMap();
        }

        private void MarkStroke(int i)
        {
            _strokeMask[i] = true;
            _strokeTouched.Add(i);
        }

        private byte SmoothedKey(int x, int y)
        {
            int sum = 0;
            int count = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (_doc.Contains(x + dx, y + dy))
                    {
                        sum += _altitudes.Altitude[_doc.Altitude[_doc.Index(x + dx, y + dy)]];
                        count++;
                    }
                }
            }

            int i = _doc.Index(x, y);
            return _altitudes.WithAltitude(_doc.Altitude[i], (int)Math.Round(sum / (double)count));
        }

        private void PaintTile(int x, int y)
        {
            PaintTile(x, y, (byte)_selectedTerrain);
        }

        private void PaintTile(int x, int y, byte terrain)
        {
            int i = _doc.Index(x, y);
            bool setAltitude = chkSetAltitude.Checked;
            byte key = setAltitude ? _altitudes.BaseKeyFor(_terrains, terrain) : _doc.Altitude[i];
            if (_doc.Terrain[i] == terrain && _doc.Altitude[i] == key)
            {
                return;
            }

            _undo.Touch(x, y);
            _doc.Terrain[i] = terrain;
            _doc.Altitude[i] = key;
        }

        private void FloodFill(int x, int y)
        {
            if (!_doc.Contains(x, y))
            {
                return;
            }

            byte from = _doc.Terrain[_doc.Index(x, y)];
            byte to = (byte)_selectedTerrain;
            if (from == to)
            {
                return;
            }

            Stack<int> stack = new Stack<int>();
            stack.Push(_doc.Index(x, y));
            int w = _doc.Width;
            int filled = 0;
            while (stack.Count > 0)
            {
                int i = stack.Pop();
                if (_doc.Terrain[i] != from)
                {
                    continue;
                }

                int px = i % w;
                int py = i / w;
                PaintTile(px, py, to);
                filled++;
                if (px > 0)
                {
                    stack.Push(i - 1);
                }

                if (px < w - 1)
                {
                    stack.Push(i + 1);
                }

                if (py > 0)
                {
                    stack.Push(i - w);
                }

                if (py < _doc.Height - 1)
                {
                    stack.Push(i + w);
                }
            }

            lblMessage.Text = string.Format("{0} tiles filled.", filled);
        }

        private void Pick(int x, int y)
        {
            if (!_doc.Contains(x, y))
            {
                return;
            }

            int i = _doc.Index(x, y);
            _selectedTerrain = _doc.Terrain[i];
            if (!string.IsNullOrEmpty(txtTerrainFilter.Text))
            {
                txtTerrainFilter.Text = string.Empty;
            }

            SelectTerrainInList(_selectedTerrain);
            numTargetHeight.Value = Math.Clamp(_altitudes.Altitude[_doc.Altitude[i]], (int)numTargetHeight.Minimum, (int)numTargetHeight.Maximum);
            lblMessage.Text = string.Format("Picked {0}, z {1}", _terrains.DisplayName(_selectedTerrain), _altitudes.Altitude[_doc.Altitude[i]]);
        }

        // ================================================================ stamp

        private void BtnChooseStamp_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Choose a pattern (terrain colours, transparent / black 'No Draw' = not painted)";
                dialog.Filter = "Images|*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|All files|*.*";
                string dir = MapDataPaths.EnsureDirectory(MapDataPaths.TemplatesDir);
                dialog.InitialDirectory = dir;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                LoadStamp(dialog.FileName);
            }
        }

        public void LoadStamp(string path)
        {
            try
            {
                using (Bitmap bmp = MapDocument.LoadBitmapUnlocked(path))
                {
                    _stamp = TemplateImporter.ImageToTerrain(bmp, _terrains, false);
                    _stampWidth = bmp.Width;
                    _stampHeight = bmp.Height;
                }

                picStamp.Image?.Dispose();
                MapDocument preview = new MapDocument(_stampWidth, _stampHeight);
                Buffer.BlockCopy(_stamp, 0, preview.Terrain, 0, _stamp.Length);
                picStamp.Image = preview.RenderPreview(240, 68, MapDocument.RenderMode.Terrain, _terrains, _altitudes);
                lblStampName.Text = string.Format("{0} ({1}x{2})", Path.GetFileName(path), _stampWidth, _stampHeight);
                rbStamp.Checked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not load the pattern: " + ex.Message, "Stamp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Stamp(int cx, int cy)
        {
            if (_stamp == null || !_strokeOpen)
            {
                return;
            }

            int scale = (int)numStampScale.Value;
            int w = _stampWidth * scale;
            int h = _stampHeight * scale;
            int x0 = cx - (w / 2);
            int y0 = cy - (h / 2);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    byte t = _stamp[((y / scale) * _stampWidth) + (x / scale)];
                    if (t == TerrainIds.NoDraw || !_doc.Contains(x0 + x, y0 + y))
                    {
                        continue;
                    }

                    PaintTile(x0 + x, y0 + y, t);
                }
            }

            _lastStampAt = new Point(cx, cy);
            canvas.RefreshMap();
        }

        // ================================================================ undo

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            _undo.Undo();
            canvas.RefreshMap();
            MarkModified();
        }

        private void BtnRedo_Click(object sender, EventArgs e)
        {
            _undo.Redo();
            canvas.RefreshMap();
            MarkModified();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.Z:
                    BtnUndo_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.Y:
                    BtnRedo_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.S:
                    BtnSave_Click(this, EventArgs.Empty);
                    return true;
            }

            if (!txtTerrainFilter.Focused && (keyData & Keys.Modifiers) == Keys.None)
            {
                switch (keyData)
                {
                    case Keys.B:
                        rbBrush.Checked = true;
                        return true;
                    case Keys.F:
                        rbFill.Checked = true;
                        return true;
                    case Keys.R:
                        rbRectangle.Checked = true;
                        return true;
                    case Keys.I:
                        rbPicker.Checked = true;
                        return true;
                    case Keys.OemOpenBrackets:
                        trackBrush.Value = Math.Max(trackBrush.Minimum, trackBrush.Value - 1);
                        return true;
                    case Keys.OemCloseBrackets:
                        trackBrush.Value = Math.Min(trackBrush.Maximum, trackBrush.Value + 1);
                        return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ================================================================ files

        private void BtnNew_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscard())
            {
                return;
            }

            using (NewMapDialog dialog = new NewMapDialog(_allTerrains, _doc.Width, _doc.Height))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                byte terrain = (byte)dialog.TerrainId;
                SetDocument(MapDocument.CreateBlank(dialog.MapWidth, dialog.MapHeight, terrain, _altitudes.BaseKeyFor(_terrains, terrain)), null, null);
                DocumentChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscard())
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Open terrain image";
                dialog.Filter = "Images|*.bmp;*.png;*.gif;*.tif;*.tiff|All files|*.*";
                if (!string.IsNullOrEmpty(TerrainPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(TerrainPath);
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string altitudePath = GuessAltitudePath(dialog.FileName);
                try
                {
                    MapDocument doc = MapDocument.Load(dialog.FileName, altitudePath, _terrains, _altitudes, message => lblMessage.Text = message);
                    SetDocument(doc, dialog.FileName, altitudePath);
                    DocumentChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Terrain.bmp -> Altitude.bmp, MyMap.bmp -> MyMap_Altitude.bmp</summary>
        public static string GuessAltitudePath(string terrainPath)
        {
            string dir = Path.GetDirectoryName(terrainPath) ?? string.Empty;
            string name = Path.GetFileNameWithoutExtension(terrainPath);
            if (name.Equals("Terrain", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(dir, "Altitude.bmp");
            }

            if (name.EndsWith("Terrain", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(dir, name.Substring(0, name.Length - "Terrain".Length) + "Altitude.bmp");
            }

            return Path.Combine(dir, name + "_Altitude.bmp");
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TerrainPath))
            {
                BtnSaveAs_Click(sender, e);
                return;
            }

            Save(TerrainPath, AltitudePath ?? GuessAltitudePath(TerrainPath));
        }

        private void BtnSaveAs_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Save terrain image (the altitude image is saved next to it)";
                dialog.Filter = "8 bit bitmap|*.bmp";
                dialog.FileName = string.IsNullOrEmpty(TerrainPath) ? "Terrain.bmp" : Path.GetFileName(TerrainPath);
                if (!string.IsNullOrEmpty(TerrainPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(TerrainPath);
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    Save(dialog.FileName, GuessAltitudePath(dialog.FileName));
                }
            }
        }

        private void Save(string terrainPath, string altitudePath)
        {
            try
            {
                _doc.Save(terrainPath, altitudePath, _terrains, _altitudes);
                TerrainPath = terrainPath;
                AltitudePath = altitudePath;
                _modified = false;
                UpdateTitle();
                lblMessage.Text = "Saved: " + terrainPath + "  +  " + Path.GetFileName(altitudePath);
                DocumentChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ConfirmDiscard()
        {
            if (!_modified)
            {
                return true;
            }

            DialogResult result = MessageBox.Show(this, "The map has unsaved changes. Save them first?", "Map Editor", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                return false;
            }

            if (result == DialogResult.Yes)
            {
                BtnSave_Click(this, EventArgs.Empty);
                return !_modified;
            }

            return true;
        }

        private void MapEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ConfirmOnClose && e.CloseReason == CloseReason.UserClosing && !ConfirmDiscard())
            {
                e.Cancel = true;
            }
        }

        // ================================================================ transitions

        private async Task<TransitionSet> GetTransitionsAsync()
        {
            if (_transitions == null)
            {
                lblMessage.Text = "Loading transitions ...";
                _transitions = await Task.Run(() => UoMapCompiler.LoadTransitions(MapDataPaths.TransitionsDir, null));
            }

            return _transitions;
        }

        private async void BtnCheckTransitions_Click(object sender, EventArgs e)
        {
            SetBusy(true);
            try
            {
                TransitionSet transitions = await GetTransitionsAsync();
                lblMessage.Text = "Checking transitions ...";
                List<string> result = await Task.Run(() => UoMapCompiler.CheckTransitions(_doc, _terrains, transitions, 25, CancellationToken.None));
                lblMessage.Text = result.Count == 0 ? "All terrain borders have transitions." : string.Format("{0} terrain combinations without transition.", result.Count);
                MessageBox.Show(this, result.Count == 0 ? "No problems found." : "Terrain combinations without transition (tiles):\r\n\r\n" + string.Join("\r\n", result) + "\r\n\r\nUse 'Repair transitions' or paint a strip of a terrain in between.", "Check transitions", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Check transitions", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void BtnRepairTransitions_Click(object sender, EventArgs e)
        {
            SetBusy(true);
            try
            {
                TransitionSet transitions = await GetTransitionsAsync();
                lblMessage.Text = "Repairing transitions ...";
                _undo.Begin();
                _undo.TouchAll();
                List<string> log = new List<string>();
                int changed = await Task.Run(() => TransitionRepair.Repair(_doc, _terrains, _altitudes, transitions, 6, null, log.Add, CancellationToken.None));
                _undo.End();
                canvas.RefreshMap();
                MarkModified();
                lblMessage.Text = string.Format("Transition repair: {0} tiles changed. {1}", changed, log.LastOrDefault());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Repair transitions", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy)
        {
            UseWaitCursor = busy;
            toolStripMain.Enabled = !busy;
            canvas.Enabled = !busy;
        }
    }

    /// <summary>Small dialog: size + fill terrain for a new map / sketch.</summary>
    internal sealed class NewMapDialog : Form
    {
        private readonly NumericUpDown _width = new NumericUpDown();
        private readonly NumericUpDown _height = new NumericUpDown();
        private readonly ComboBox _terrain = new ComboBox();

        public int MapWidth => (int)_width.Value;
        public int MapHeight => (int)_height.Value;
        public int TerrainId => TerrainChoice.SelectedId(_terrain, TerrainIds.Grass);

        public NewMapDialog(IEnumerable<TerrainChoice> terrains, int width, int height)
        {
            Text = "New map / sketch";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(380, 170);

            TableLayoutPanel table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, Padding = new Padding(10) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            foreach (NumericUpDown n in new[] { _width, _height })
            {
                n.Minimum = 8;
                n.Maximum = 32760;
                n.Increment = 8;
                n.Width = 100;
            }

            _width.Value = Math.Clamp(width, 8, 32760);
            _height.Value = Math.Clamp(height, 8, 32760);
            _terrain.DropDownStyle = ComboBoxStyle.DropDownList;
            _terrain.DrawMode = DrawMode.OwnerDrawFixed;
            _terrain.DrawItem += TerrainChoice.DrawItem;
            _terrain.Width = 230;
            _terrain.Items.AddRange(terrains.Cast<object>().ToArray());
            TerrainChoice.Select(_terrain, TerrainIds.Grass);

            table.Controls.Add(new Label { Text = "Width (tiles):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            table.Controls.Add(_width, 1, 0);
            table.Controls.Add(new Label { Text = "Height (tiles):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            table.Controls.Add(_height, 1, 1);
            table.Controls.Add(new Label { Text = "Fill with:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            table.Controls.Add(_terrain, 1, 2);
            table.Controls.Add(new Label { Text = "Tip: for a generator sketch fill with [00] No Draw (= let the generator decide).", AutoSize = true, MaximumSize = new Size(340, 0) }, 0, 3);
            table.SetColumnSpan(table.GetControlFromPosition(0, 3), 2);

            FlowLayoutPanel buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = true };
            Button ok = new Button { Text = "OK", DialogResult = DialogResult.OK };
            Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            table.Controls.Add(buttons, 0, 4);
            table.SetColumnSpan(buttons, 2);
            Controls.Add(table);
            AcceptButton = ok;
            CancelButton = cancel;
            ok.Click += (s, e) =>
            {
                if (!MapDocument.IsValidUoSize(MapWidth, MapHeight, out string error))
                {
                    MessageBox.Show(this, error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
            };
        }
    }
}
