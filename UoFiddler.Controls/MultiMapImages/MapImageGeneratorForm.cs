/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.MapViewer.Editor;
using UoFiddler.Controls.MapViewer.Help;

namespace UoFiddler.Controls.MultiMapImages
{
    /// <summary>
    /// Creates an image of a map (built-in, from the map list or external map files) or of an image file.
    /// The result can be added to the "Load.." list, saved anywhere, copied or opened in the editor.
    /// </summary>
    public class MapImageGeneratorForm : Form
    {
        private sealed class MapItem
        {
            public MapDefinition Definition;

            public override string ToString()
            {
                return $"{Definition.Name}   ({Definition.Width} x {Definition.Height})" + (Definition.Id < 0 ? "  [external]" : string.Empty);
            }
        }

        private static readonly string[] _styleNames =
        {
            "Ink map (drawn, symbols)", "Colors (radar colors)", "Grayscale", "Black / white (dithered)", "Sketch (outlines)"
        };

        // remembered between openings
        private static readonly MapImageSettings _last = new MapImageSettings();
        private static int _lastSource;
        private static int _lastMapId;
        private static string _lastImageFile = string.Empty;

        private readonly Image _currentImage;
        private readonly string _currentName;

        private readonly RadioButton _rbMap = new RadioButton();
        private readonly ComboBox _comboMaps = new ComboBox();
        private readonly Button _btnExternalMap = new Button();
        private readonly RadioButton _rbImage = new RadioButton();
        private readonly TextBox _txtImage = new TextBox();
        private readonly Button _btnBrowseImage = new Button();
        private readonly RadioButton _rbCurrent = new RadioButton();

        private readonly CheckBox _chkArea = new CheckBox();
        private readonly NumericUpDown _areaX = new NumericUpDown();
        private readonly NumericUpDown _areaY = new NumericUpDown();
        private readonly NumericUpDown _areaW = new NumericUpDown();
        private readonly NumericUpDown _areaH = new NumericUpDown();

        private readonly ComboBox _comboStyle = new ComboBox();
        private readonly NumericUpDown _width = new NumericUpDown();
        private readonly NumericUpDown _height = new NumericUpDown();
        private readonly CheckBox _chkAspect = new CheckBox();
        private readonly Button _btnMultiMapSize = new Button();

        private readonly CheckBox _chkStatics = new CheckBox();
        private readonly CheckBox _chkRipples = new CheckBox();
        private readonly CheckBox _chkParchment = new CheckBox();
        private readonly CheckBox _chkFrame = new CheckBox();
        private readonly NumericUpDown _grid = new NumericUpDown();
        private readonly NumericUpDown _symbolSize = new NumericUpDown();
        private readonly NumericUpDown _density = new NumericUpDown();
        private readonly NumericUpDown _seed = new NumericUpDown();
        private readonly Button _btnRandomSeed = new Button();

        private readonly Button _btnGenerate = new Button();
        private readonly Button _btnCancelGenerate = new Button();
        private readonly ProgressBar _progress = new ProgressBar();
        private readonly Label _status = new Label();

        private readonly TextBox _txtName = new TextBox();
        private readonly Button _btnAddToList = new Button();
        private readonly Button _btnSaveAs = new Button();
        private readonly Button _btnCopy = new Button();
        private readonly Button _btnEdit = new Button();
        private readonly Button _btnClose = new Button();

        private readonly ZoomCanvas _canvas = new ZoomCanvas();
        private readonly Panel _left = new Panel();

        private Bitmap _result;
        private CancellationTokenSource _cts;
        private bool _running;

        public MapImageGeneratorForm(Image currentImage, string currentName)
        {
            _currentImage = currentImage;
            _currentName = currentName;

            Text = "Create map image";
            Icon = Options.GetFiddlerIcon();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(900, 600);
            KeyPreview = true;

            BuildLayout();
            LoadMaps();
            ApplyLast();
            UpdateSourceState();
            UpdateResultButtons();
        }

        /// <summary>Last entry that was added to the list (to show it after closing)</summary>
        public MapImageEntry AddedEntry { get; private set; }

        /// <summary>When set, the caller opens this image in the editor (ownership goes to the caller)</summary>
        public Bitmap ImageToEdit { get; private set; }

        public string ResultName => string.IsNullOrWhiteSpace(_txtName.Text) ? "Map image" : _txtName.Text.Trim();

        #region Layout

        private void BuildLayout()
        {
            _left.Dock = DockStyle.Left;
            _left.Width = 340;
            _left.AutoScroll = true;
            _left.Padding = new Padding(6);

            int y = 8;

            // source
            var groupSource = new GroupBox { Text = "Source", Left = 8, Top = y, Width = 316, Height = 150 };
            _rbMap.Text = "Map:";
            _rbMap.SetBounds(10, 20, 60, 22);
            _comboMaps.SetBounds(72, 20, 232, 23);
            _comboMaps.DropDownStyle = ComboBoxStyle.DropDownList;
            _comboMaps.SelectedIndexChanged += (s, e) => OnMapChanged();
            _btnExternalMap.Text = "External map files...";
            _btnExternalMap.SetBounds(72, 46, 150, 25);
            _btnExternalMap.Click += OnExternalMap;
            _rbImage.Text = "Image file:";
            _rbImage.SetBounds(10, 80, 90, 22);
            _txtImage.SetBounds(26, 102, 220, 23);
            _btnBrowseImage.Text = "...";
            _btnBrowseImage.SetBounds(252, 101, 52, 25);
            _btnBrowseImage.Click += OnBrowseImage;
            _rbCurrent.Text = "Current image of the MultiMap tab";
            _rbCurrent.SetBounds(10, 126, 290, 22);
            _rbCurrent.Enabled = _currentImage != null;
            foreach (RadioButton rb in new[] { _rbMap, _rbImage, _rbCurrent })
            {
                rb.CheckedChanged += (s, e) => UpdateSourceState();
            }

            groupSource.Controls.AddRange(new Control[] { _rbMap, _comboMaps, _btnExternalMap, _rbImage, _txtImage, _btnBrowseImage, _rbCurrent });
            y += groupSource.Height + 6;

            // area
            var groupArea = new GroupBox { Text = "Area (tiles)", Left = 8, Top = y, Width = 316, Height = 100 };
            _chkArea.Text = "Only this area of the map";
            _chkArea.SetBounds(10, 20, 200, 22);
            _chkArea.CheckedChanged += (s, e) => UpdateSourceState();
            var btnWhole = new Button { Text = "Whole map", Left = 212, Top = 18, Width = 92, Height = 25 };
            btnWhole.Click += (s, e) => { _chkArea.Checked = false; OnMapChanged(); };
            AddNumeric(groupArea, "X", _areaX, 10, 48, 0, 65535, 0);
            AddNumeric(groupArea, "Y", _areaY, 160, 48, 0, 65535, 0);
            AddNumeric(groupArea, "W", _areaW, 10, 72, 8, 65536, 1024);
            AddNumeric(groupArea, "H", _areaH, 160, 72, 8, 65536, 1024);
            groupArea.Controls.AddRange(new Control[] { _chkArea, btnWhole });
            y += groupArea.Height + 6;

            // style and size
            var groupStyle = new GroupBox { Text = "Style and size", Left = 8, Top = y, Width = 316, Height = 112 };
            _comboStyle.SetBounds(10, 20, 294, 23);
            _comboStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            _comboStyle.Items.AddRange(_styleNames);
            _comboStyle.SelectedIndexChanged += (s, e) => UpdateSourceState();
            AddNumeric(groupStyle, "Width", _width, 10, 52, 64, 16000, 2560);
            AddNumeric(groupStyle, "Height", _height, 160, 52, 64, 16000, 2048);
            _chkAspect.Text = "Keep aspect ratio";
            _chkAspect.SetBounds(10, 80, 140, 22);
            _chkAspect.CheckedChanged += (s, e) => _height.Enabled = !_chkAspect.Checked;
            _btnMultiMapSize.Text = "2560 x 2048 (MultiMap)";
            _btnMultiMapSize.SetBounds(152, 78, 152, 25);
            _btnMultiMapSize.Click += (s, e) =>
            {
                _width.Value = 2560;
                _height.Value = 2048;
                _chkAspect.Checked = false;
            };
            groupStyle.Controls.AddRange(new Control[] { _comboStyle, _chkAspect, _btnMultiMapSize });
            y += groupStyle.Height + 6;

            // options
            var groupOptions = new GroupBox { Text = "Options", Left = 8, Top = y, Width = 316, Height = 176 };
            _chkStatics.Text = "Statics (trees, houses)";
            _chkStatics.SetBounds(10, 20, 150, 22);
            _chkRipples.Text = "Coast lines / waves";
            _chkRipples.SetBounds(160, 20, 150, 22);
            _chkParchment.Text = "Parchment look";
            _chkParchment.SetBounds(10, 44, 150, 22);
            _chkFrame.Text = "Frame";
            _chkFrame.SetBounds(160, 44, 150, 22);
            AddNumeric(groupOptions, "Grid", _grid, 10, 74, 0, 4096, 0);
            new ToolTip().SetToolTip(_grid, "Grid line every n tiles (map) or pixels (image), 0 = no grid");
            AddDecimal(groupOptions, "Symbols", _symbolSize, 160, 74, 0.3m, 4m, 1m);
            new ToolTip().SetToolTip(_symbolSize, "Size of mountains, trees, houses, coast lines");
            AddDecimal(groupOptions, "Density", _density, 10, 104, 0.2m, 3m, 1m);
            new ToolTip().SetToolTip(_density, "Amount of dots and hatching (ink map) / line sensitivity (sketch)");
            AddNumeric(groupOptions, "Seed", _seed, 160, 104, 0, 999999, 1);
            _btnRandomSeed.Text = "Other random pattern";
            _btnRandomSeed.SetBounds(160, 136, 144, 25);
            _btnRandomSeed.Click += (s, e) => _seed.Value = new Random().Next(1, 999999);
            groupOptions.Controls.AddRange(new Control[] { _chkStatics, _chkRipples, _chkParchment, _chkFrame, _btnRandomSeed });
            y += groupOptions.Height + 6;

            // generate
            _btnGenerate.Text = "Generate";
            _btnGenerate.SetBounds(8, y, 150, 32);
            _btnGenerate.Font = new Font(Font, FontStyle.Bold);
            _btnGenerate.Click += OnGenerate;
            _btnCancelGenerate.Text = "Cancel";
            _btnCancelGenerate.SetBounds(164, y, 80, 32);
            _btnCancelGenerate.Enabled = false;
            _btnCancelGenerate.Click += (s, e) => _cts?.Cancel();
            var btnHelp = new Button { Text = "Help (F1)", Left = 250, Top = y, Width = 74, Height = 32 };
            btnHelp.Click += (s, e) => ShowHelp();
            y += 38;
            _progress.SetBounds(8, y, 316, 16);
            y += 20;
            _status.SetBounds(8, y, 316, 36);
            _status.ForeColor = SystemColors.GrayText;
            y += 40;

            // result
            var groupResult = new GroupBox { Text = "Result", Left = 8, Top = y, Width = 316, Height = 120 };
            var labelName = new Label { Text = "Name:", Left = 10, Top = 23, Width = 50 };
            _txtName.SetBounds(62, 20, 242, 23);
            _btnAddToList.Text = "Add to \"Load..\" list";
            _btnAddToList.SetBounds(10, 50, 145, 28);
            _btnAddToList.Click += OnAddToList;
            _btnSaveAs.Text = "Save as...";
            _btnSaveAs.SetBounds(160, 50, 144, 28);
            _btnSaveAs.Click += OnSaveAs;
            _btnEdit.Text = "Edit (draw, text)...";
            _btnEdit.SetBounds(10, 84, 145, 28);
            _btnEdit.Click += OnEdit;
            _btnCopy.Text = "Copy";
            _btnCopy.SetBounds(160, 84, 144, 28);
            _btnCopy.Click += (s, e) =>
            {
                if (_result != null)
                {
                    Clipboard.SetImage(_result);
                    _status.Text = "Image copied to the clipboard.";
                }
            };
            groupResult.Controls.AddRange(new Control[] { labelName, _txtName, _btnAddToList, _btnSaveAs, _btnEdit, _btnCopy });
            y += groupResult.Height + 8;

            _btnClose.Text = "Close";
            _btnClose.SetBounds(244, y, 80, 28);
            _btnClose.DialogResult = DialogResult.Cancel;
            CancelButton = _btnClose;

            _left.Controls.AddRange(new Control[]
            {
                groupSource, groupArea, groupStyle, groupOptions, _btnGenerate, _btnCancelGenerate, btnHelp, _progress, _status,
                groupResult, _btnClose
            });

            _canvas.Dock = DockStyle.Fill;
            _canvas.PanWithLeftButton = true;
            _canvas.ZoomChanged += (s, e) => UpdateZoomText();

            var zoomBar = new ToolStrip { Dock = DockStyle.Top, GripStyle = ToolStripGripStyle.Hidden };
            zoomBar.Items.Add(new ToolStripButton("Zoom +", null, (s, e) => _canvas.ZoomIn()));
            zoomBar.Items.Add(new ToolStripButton("Zoom -", null, (s, e) => _canvas.ZoomOut()));
            zoomBar.Items.Add(new ToolStripButton("1:1", null, (s, e) => _canvas.ZoomActualSize()));
            zoomBar.Items.Add(new ToolStripButton("Fit", null, (s, e) => _canvas.ZoomToFit()));
            zoomBar.Items.Add(new ToolStripSeparator());
            zoomBar.Items.Add(new ToolStripLabel("Mouse wheel = zoom, drag = move") { ForeColor = SystemColors.GrayText });
            _zoomLabel = new ToolStripLabel("100 %") { Alignment = ToolStripItemAlignment.Right };
            zoomBar.Items.Add(_zoomLabel);

            Controls.Add(_canvas);
            Controls.Add(zoomBar);
            Controls.Add(_left);
        }

        private ToolStripLabel _zoomLabel;

        private void UpdateZoomText()
        {
            _zoomLabel.Text = $"{_canvas.Zoom * 100:0} %";
        }

        private static void AddNumeric(Control parent, string label, NumericUpDown n, int x, int y, int min, int max, int value)
        {
            parent.Controls.Add(new Label { Text = label, Left = x, Top = y + 3, Width = 52 });
            n.SetBounds(x + 54, y, 88, 23);
            n.Minimum = min;
            n.Maximum = max;
            n.Value = Math.Max(min, Math.Min(max, value));
            parent.Controls.Add(n);
        }

        private static void AddDecimal(Control parent, string label, NumericUpDown n, int x, int y, decimal min, decimal max, decimal value)
        {
            parent.Controls.Add(new Label { Text = label, Left = x, Top = y + 3, Width = 52 });
            n.SetBounds(x + 54, y, 88, 23);
            n.DecimalPlaces = 1;
            n.Increment = 0.1m;
            n.Minimum = min;
            n.Maximum = max;
            n.Value = Math.Max(min, Math.Min(max, value));
            parent.Controls.Add(n);
        }

        #endregion

        #region Source

        private void LoadMaps()
        {
            _comboMaps.Items.Clear();
            foreach (MapDefinition def in MapRegistry.Definitions)
            {
                _comboMaps.Items.Add(new MapItem { Definition = def });
            }

            foreach (MapDefinition def in MapRegistry.TemporaryDefinitions)
            {
                _comboMaps.Items.Add(new MapItem { Definition = def });
            }
        }

        private void SelectMap(int id)
        {
            for (int i = 0; i < _comboMaps.Items.Count; i++)
            {
                if (((MapItem)_comboMaps.Items[i]).Definition.Id == id)
                {
                    _comboMaps.SelectedIndex = i;
                    return;
                }
            }

            if (_comboMaps.Items.Count > 0 && _comboMaps.SelectedIndex < 0)
            {
                _comboMaps.SelectedIndex = 0;
            }
        }

        private MapDefinition SelectedMap => (_comboMaps.SelectedItem as MapItem)?.Definition;

        private void OnMapChanged()
        {
            MapDefinition def = SelectedMap;
            if (def == null)
            {
                return;
            }

            if (!_chkArea.Checked)
            {
                _areaX.Value = 0;
                _areaY.Value = 0;
                _areaW.Value = Math.Max(_areaW.Minimum, Math.Min(_areaW.Maximum, def.Width));
                _areaH.Value = Math.Max(_areaH.Minimum, Math.Min(_areaH.Maximum, def.Height));
            }

            if (_rbMap.Checked)
            {
                _txtName.Text = $"{def.Name} - {ShortStyleName()}";
            }
        }

        private string ShortStyleName()
        {
            switch ((MapImageStyle)Math.Max(0, _comboStyle.SelectedIndex))
            {
                case MapImageStyle.Colors: return "Colors";
                case MapImageStyle.Grayscale: return "Grayscale";
                case MapImageStyle.BlackWhite: return "BW";
                case MapImageStyle.Sketch: return "Sketch";
                default: return "Ink";
            }
        }

        private void OnExternalMap(object sender, EventArgs e)
        {
            using (var dialog = new ExternalMapDialog(Options.OutputPath))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Definition == null)
                {
                    return;
                }

                LoadMaps();
                SelectMap(dialog.Definition.Id);
                _rbMap.Checked = true;
            }
        }

        private void OnBrowseImage(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Title = "Image to convert", Filter = MapImageLibrary.OpenFilter })
            {
                if (File.Exists(_txtImage.Text))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(_txtImage.Text);
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                _txtImage.Text = dialog.FileName;
                _rbImage.Checked = true;
                _txtName.Text = $"{Path.GetFileNameWithoutExtension(dialog.FileName)} - {ShortStyleName()}";
            }
        }

        private void UpdateSourceState()
        {
            bool map = _rbMap.Checked;
            _comboMaps.Enabled = _btnExternalMap.Enabled = map && !_running;
            _txtImage.Enabled = _btnBrowseImage.Enabled = _rbImage.Checked && !_running;
            _chkArea.Enabled = map && !_running;
            _areaX.Enabled = _areaY.Enabled = _areaW.Enabled = _areaH.Enabled = map && _chkArea.Checked && !_running;
            _chkStatics.Enabled = map;
            bool ink = _comboStyle.SelectedIndex == (int)MapImageStyle.InkMap;
            _chkRipples.Enabled = _symbolSize.Enabled = ink;
            _seed.Enabled = _btnRandomSeed.Enabled = ink || _chkParchment.Checked;

            if (_rbCurrent.Checked && string.IsNullOrWhiteSpace(_txtName.Text))
            {
                _txtName.Text = $"{_currentName} - {ShortStyleName()}";
            }
        }

        #endregion

        #region Settings

        private void ApplyLast()
        {
            _comboStyle.SelectedIndex = (int)_last.Style;
            _width.Value = Math.Max(_width.Minimum, Math.Min(_width.Maximum, _last.OutputWidth));
            _height.Value = Math.Max(_height.Minimum, Math.Min(_height.Maximum, _last.OutputHeight));
            _chkAspect.Checked = _last.KeepAspect;
            _height.Enabled = !_last.KeepAspect;
            _chkStatics.Checked = _last.Statics;
            _chkRipples.Checked = _last.CoastRipples;
            _chkParchment.Checked = _last.Parchment;
            _chkFrame.Checked = _last.Frame;
            _grid.Value = Math.Max(_grid.Minimum, Math.Min(_grid.Maximum, _last.GridSize));
            _symbolSize.Value = Math.Max(_symbolSize.Minimum, Math.Min(_symbolSize.Maximum, (decimal)_last.SymbolScale));
            _density.Value = Math.Max(_density.Minimum, Math.Min(_density.Maximum, (decimal)_last.Density));
            _seed.Value = Math.Max(_seed.Minimum, Math.Min(_seed.Maximum, _last.Seed));
            _txtImage.Text = _lastImageFile;

            SelectMap(_lastMapId);
            switch (_lastSource)
            {
                case 1:
                    _rbImage.Checked = true;
                    break;
                case 2 when _currentImage != null:
                    _rbCurrent.Checked = true;
                    break;
                default:
                    _rbMap.Checked = true;
                    break;
            }

            OnMapChanged();
            if (_rbCurrent.Checked)
            {
                _txtName.Text = $"{_currentName} - {ShortStyleName()}";
            }
            else if (_rbImage.Checked && !string.IsNullOrEmpty(_txtImage.Text))
            {
                _txtName.Text = $"{Path.GetFileNameWithoutExtension(_txtImage.Text)} - {ShortStyleName()}";
            }
        }

        private MapImageSettings ReadSettings()
        {
            var s = new MapImageSettings
            {
                Style = (MapImageStyle)Math.Max(0, _comboStyle.SelectedIndex),
                OutputWidth = (int)_width.Value,
                OutputHeight = (int)_height.Value,
                KeepAspect = _chkAspect.Checked,
                Statics = _chkStatics.Checked,
                CoastRipples = _chkRipples.Checked,
                Parchment = _chkParchment.Checked,
                Frame = _chkFrame.Checked,
                GridSize = (int)_grid.Value,
                SymbolScale = (float)_symbolSize.Value,
                Density = (float)_density.Value,
                Seed = (int)_seed.Value,
                Area = _rbMap.Checked && _chkArea.Checked
                    ? new Rectangle((int)_areaX.Value, (int)_areaY.Value, (int)_areaW.Value, (int)_areaH.Value)
                    : Rectangle.Empty
            };

            // remember
            _last.Style = s.Style;
            _last.OutputWidth = s.OutputWidth;
            _last.OutputHeight = s.OutputHeight;
            _last.KeepAspect = s.KeepAspect;
            _last.Statics = s.Statics;
            _last.CoastRipples = s.CoastRipples;
            _last.Parchment = s.Parchment;
            _last.Frame = s.Frame;
            _last.GridSize = s.GridSize;
            _last.SymbolScale = s.SymbolScale;
            _last.Density = s.Density;
            _last.Seed = s.Seed;
            _lastSource = _rbImage.Checked ? 1 : _rbCurrent.Checked ? 2 : 0;
            _lastMapId = SelectedMap?.Id ?? 0;
            _lastImageFile = _txtImage.Text;
            return s;
        }

        #endregion

        #region Generate

        private async void OnGenerate(object sender, EventArgs e)
        {
            if (_running)
            {
                return;
            }

            MapImageSettings settings = ReadSettings();
            Bitmap sourceImage = null;
            Map map = null;

            try
            {
                if (_rbMap.Checked)
                {
                    MapDefinition def = SelectedMap;
                    if (def == null)
                    {
                        MessageBox.Show(this, "Please choose a map.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    map = def.GetMap();
                    if (map == null || !map.Tiles.AllFilesExist())
                    {
                        MessageBox.Show(this, $"The files of the map \"{def.Name}\" were not found.", Text, MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (_rbImage.Checked)
                {
                    if (!File.Exists(_txtImage.Text))
                    {
                        MessageBox.Show(this, "Please choose an image file.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    sourceImage = MapImageLibrary.LoadBitmap(_txtImage.Text);
                }
                else
                {
                    if (_currentImage == null)
                    {
                        return;
                    }

                    // own copy: the shown image must not be used by two threads
                    sourceImage = new Bitmap(_currentImage);
                }
            }
            catch (Exception ex)
            {
                sourceImage?.Dispose();
                MessageBox.Show(this, $"The source can not be loaded:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SetRunning(true);
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            var progress = new Progress<int>(p => _progress.Value = Math.Max(0, Math.Min(100, p)));
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _status.Text = "Generating...";

            try
            {
                Bitmap result = await Task.Run(() => map != null
                    ? MapImageRenderer.RenderMap(map, settings, progress, token)
                    : MapImageRenderer.RenderImage(sourceImage, settings, progress, token), token);

                Bitmap old = _result;
                _result = result;
                _canvas.Image = _result;
                old?.Dispose();
                _canvas.ZoomToFit();
                _status.Text = $"{_result.Width} x {_result.Height} pixels, {watch.Elapsed.TotalSeconds:0.0} s";
                if (string.IsNullOrWhiteSpace(_txtName.Text))
                {
                    _txtName.Text = "Map image";
                }
            }
            catch (OperationCanceledException)
            {
                _status.Text = "Cancelled.";
            }
            catch (Exception ex)
            {
                _status.Text = "Error.";
                MessageBox.Show(this, $"Generating failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                sourceImage?.Dispose();
                _cts.Dispose();
                _cts = null;
                _progress.Value = 0;
                SetRunning(false);
            }
        }

        private void SetRunning(bool running)
        {
            _running = running;
            _btnGenerate.Enabled = !running;
            _btnCancelGenerate.Enabled = running;
            _rbMap.Enabled = _rbImage.Enabled = !running;
            _rbCurrent.Enabled = !running && _currentImage != null;
            UseWaitCursor = running;
            _canvas.UseWaitCursor = false;
            UpdateSourceState();
            UpdateResultButtons();
        }

        private void UpdateResultButtons()
        {
            bool has = _result != null && !_running;
            _btnAddToList.Enabled = _btnSaveAs.Enabled = _btnCopy.Enabled = _btnEdit.Enabled = has;
        }

        #endregion

        #region Result

        private void OnAddToList(object sender, EventArgs e)
        {
            if (_result == null)
            {
                return;
            }

            try
            {
                AddedEntry = MapImageLibrary.AddInternal(_result, ResultName);
                _status.Text = $"Added to the list: {AddedEntry.Name}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Saving failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSaveAs(object sender, EventArgs e)
        {
            if (_result == null)
            {
                return;
            }

            MapImageEntry entry = MapImageSaveDialog.SaveWithDialog(this, _result, ResultName, out string file);
            if (entry != null)
            {
                AddedEntry = entry;
            }

            if (file != null)
            {
                _status.Text = $"Saved: {file}";
            }
        }

        private void OnEdit(object sender, EventArgs e)
        {
            if (_result == null)
            {
                return;
            }

            ImageToEdit = _result;
            _result = null;
            _canvas.Image = null;
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        private void ShowHelp()
        {
            MapHelpForm.ShowTopic(this, MapHelpContent.MultiMapImages);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                ShowHelp();
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_running)
            {
                _cts?.Cancel();
                e.Cancel = true;
                return;
            }

            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _canvas.Image = null;
                _result?.Dispose();
                _result = null;
            }

            base.Dispose(disposing);
        }
    }
}
