// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Elevation;
using Terrain;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Helper;
using UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Map Maker: blank maps, random generator, templates, height tools, editor and MUL build.
    /// All tabs share the project bar (folder, size, map number, file names).
    /// </summary>
    public partial class MapMaker : Form
    {
        private TerrainPalette _terrains;
        private AltitudePalette _altitudes;
        private List<TerrainChoice> _terrainChoices;
        private readonly List<MapInfo> _mapSizes = new List<MapInfo>();
        private MapMakerConfig _config;
        private GeneratorSettings _settings;
        private BindingList<BiomeRule> _biomes;

        private MapDocument _document;          // current map (full size)
        private MapDocument _previewDocument;   // quick preview of the generator (not saved)
        private bool _documentModified;
        private byte[] _heightBackup;           // undo for the height tab

        private byte[] _sketch;
        private int _sketchWidth;
        private int _sketchHeight;
        private byte[] _heightTemplate;
        private int _heightTemplateWidth;
        private int _heightTemplateHeight;

        private TransitionSet _transitions;
        private CancellationTokenSource _cancel;
        private bool _busy;
        private MapEditorForm _editor;
        private readonly ImageList _templateImages = new ImageList();
        private int _templateLoadVersion;

        // data viewer (old "Data Viewer" tab)
        private int i_Menu;
        private readonly ClsElevationTable i_Altitude = new ClsElevationTable();
        private readonly ClsTerrainTable i_Terrain = new ClsTerrainTable();

        public MapMaker()
        {
            InitializeComponent();
        }

        // ================================================================ startup / shutdown

        private void MapMaker_Load(object sender, EventArgs e)
        {
            _config = MapMakerConfig.Load();
            _settings = _config.Generator ?? GeneratorSettings.CreateDefault();

            try
            {
                _altitudes = AltitudePalette.Load();
                _terrains = TerrainPalette.Load(_altitudes);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not load Terrain.xml / Altitude.xml:\r\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            TerrainIdConverter.Palette = _terrains;
            _terrainChoices = TerrainChoice.FromPalette(_terrains);

            FillTerrainCombo(cbDefaultTerrain, TerrainIds.Grass, false);
            FillTerrainCombo(cbTemplateFill, TerrainIds.DeepWater, false);
            FillTerrainCombo(cbFilterTerrain, -1, true);
            FillTerrainCombo(cbProfileTerrain, TerrainIds.Rock, false);
            cbFilterType.Items.AddRange(new object[] { "(all types)", "Water", "Land", "Mountain", "Other" });
            cbFilterType.SelectedIndex = 0;
            cbScaleMode.Items.AddRange(Enum.GetNames(typeof(TemplateScaleMode)));
            cbScaleMode.SelectedIndex = 0;

            LoadMapSizes();
            ApplyConfig();
            SetupBiomeGrid();
            propertyGenerator.SelectedObject = _settings;
            propertyGenerator.PropertyValueChanged += (s, a) => RefreshBiomeGrid();

            _templateImages.ImageSize = new Size(96, 96);
            _templateImages.ColorDepth = ColorDepth.Depth32Bit;
            listTemplates.LargeImageList = _templateImages;

            rtbHelp.Text = MapMakerHelp.Text;
            SetupToolTips();

            try
            {
                splitMain.SplitterDistance = Math.Max(300, splitMain.Height - 170);
                splitWork.SplitterDistance = Math.Max(500, splitWork.Width - 420);
                splitGenerator.SplitterDistance = Math.Max(300, Math.Min(420, splitGenerator.Width / 2));
                splitTemplates.SplitterDistance = Math.Max(250, splitTemplates.Width / 2);
            }
            catch (Exception)
            {
                // window too small - keep the designer values
            }

            Log(string.Format("Map Maker ready. Data folder: {0}", MapDataPaths.DataRoot));
            Log(string.Format("{0} terrains, altitude {1}..{2}.", _terrains.DefinedIds.Count(), _altitudes.MinAltitude, _altitudes.MaxAltitude));
            RefreshTemplates();
            UpdatePreview();

            // load an existing map of the project automatically
            if (File.Exists(TerrainPath))
            {
                LoadMapFromProject(false);
            }
        }

        private void ApplyConfig()
        {
            string project = _config.ProjectFolder;
            if (string.IsNullOrEmpty(project))
            {
                project = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapProjects", "MyMap");
            }

            txtProject.Text = project;
            txtTerrainFile.Text = string.IsNullOrEmpty(_config.TerrainFile) ? "Terrain.bmp" : _config.TerrainFile;
            txtAltitudeFile.Text = string.IsNullOrEmpty(_config.AltitudeFile) ? "Altitude.bmp" : _config.AltitudeFile;
            txtTemplateFolder.Text = string.IsNullOrEmpty(_config.TemplateFolder) ? MapDataPaths.TemplatesDir : _config.TemplateFolder;
            txtOutputFolder.Text = _config.OutputFolder ?? string.Empty;
            txtImportFolder.Text = _config.ImportFolder ?? string.Empty;
            chkRandomStatics.Checked = _config.RandomStatics;
            chkImportStatics.Checked = _config.ImportStatics;

            int index = Math.Clamp(_config.MapSizeIndex, 0, Math.Max(0, cbMapSize.Items.Count - 1));
            if (cbMapSize.Items.Count > 0)
            {
                cbMapSize.SelectedIndex = index;
            }

            if (SelectedMapSize?.IsCustom == true)
            {
                numWidth.Value = Math.Clamp(_config.CustomWidth, 8, 32760);
                numHeight.Value = Math.Clamp(_config.CustomHeight, 8, 32760);
                numMapNumber.Value = Math.Clamp(_config.MapNumber, 0, 255);
            }
        }

        private void MapMaker_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_busy)
            {
                if (MessageBox.Show(this, "A task is still running. Cancel it and close?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                _cancel?.Cancel();
            }

            if (_editor != null && !_editor.IsDisposed)
            {
                if (_editor.IsModified)
                {
                    _documentModified = true;
                }

                _editor.Close();
            }

            if (_documentModified && _document != null)
            {
                DialogResult result = MessageBox.Show(this, "The current map has unsaved changes. Save it to the project folder?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == DialogResult.Yes && !SaveMapToProject(false))
                {
                    e.Cancel = true;
                    return;
                }
            }

            SaveConfig();
        }

        private void MapMaker_FormClosed(object sender, FormClosedEventArgs e)
        {
            picPreview.Image?.Dispose();
            _templateImages.Dispose();
        }

        private void SaveConfig()
        {
            if (_config == null)
            {
                return;
            }

            _config.ProjectFolder = txtProject.Text;
            _config.TemplateFolder = txtTemplateFolder.Text;
            _config.OutputFolder = txtOutputFolder.Text;
            _config.ImportFolder = txtImportFolder.Text;
            _config.TerrainFile = txtTerrainFile.Text;
            _config.AltitudeFile = txtAltitudeFile.Text;
            _config.MapSizeIndex = cbMapSize.SelectedIndex;
            _config.CustomWidth = (int)numWidth.Value;
            _config.CustomHeight = (int)numHeight.Value;
            _config.MapNumber = (int)numMapNumber.Value;
            _config.RandomStatics = chkRandomStatics.Checked;
            _config.ImportStatics = chkImportStatics.Checked;
            _config.Generator = _settings;
            _config.Save();
        }

        private void SetupToolTips()
        {
            toolTip.SetToolTip(txtProject, "All files of this map are read from / written to this folder.");
            toolTip.SetToolTip(cbMapSize, "Size of new maps. 'Custom size' allows any multiple of 8.");
            toolTip.SetToolTip(numMapNumber, "Number of the mul files: Map#.mul, StaIdx#.mul, Statics#.mul");
            toolTip.SetToolTip(btnPreview, "Fast low resolution preview (not saved).");
            toolTip.SetToolTip(btnGenerate, "Generate the map in full size (Terrain.bmp + Altitude.bmp).");
            toolTip.SetToolTip(chkRepairAfterGenerate, "Removes single tiles / thin strips that have no transition.");
            toolTip.SetToolTip(btnBuildMul, "Build the mul files from the current map.");
            toolTip.SetToolTip(picPreview, "Double click = open in the editor.");
        }

        // ================================================================ helpers

        private string ProjectFolder => txtProject.Text.Trim();

        private string TerrainPath => Path.Combine(ProjectFolder, string.IsNullOrWhiteSpace(txtTerrainFile.Text) ? "Terrain.bmp" : txtTerrainFile.Text.Trim());

        private string AltitudePath => Path.Combine(ProjectFolder, string.IsNullOrWhiteSpace(txtAltitudeFile.Text) ? "Altitude.bmp" : txtAltitudeFile.Text.Trim());

        private MapInfo SelectedMapSize => cbMapSize.SelectedItem as MapInfo;

        private int MapWidth => (int)numWidth.Value;

        private int MapHeight => (int)numHeight.Value;

        private void FillTerrainCombo(ComboBox combo, int selected, bool includeAll)
        {
            combo.Items.Clear();
            combo.Items.AddRange(TerrainChoice.FromPalette(_terrains, includeAll).Cast<object>().ToArray());
            TerrainChoice.Select(combo, selected);
            if (combo.SelectedIndex < 0 && combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        /// <summary>Thread safe log (with time stamp).</summary>
        private void Log(string message)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action<string>(Log), message);
                }
                catch (InvalidOperationException)
                {
                    // form is closing
                }

                return;
            }

            txtLog.AppendText(string.Format("{0:HH:mm:ss}  {1}\r\n", DateTime.Now, message));
        }

        private void SetStatus(string text)
        {
            lblStatus.Text = text;
        }

        private IProgress<int> CreateProgress()
        {
            return new Progress<int>(value => progressBar.Value = Math.Clamp(value, progressBar.Minimum, progressBar.Maximum));
        }

        private void SetBusy(bool busy, string status = null)
        {
            _busy = busy;
            tabControl.Enabled = !busy;
            groupProject.Enabled = !busy;
            menuStrip.Enabled = !busy;
            tablePreviewBottom.Enabled = !busy;
            btnCancel.Enabled = busy;
            UseWaitCursor = busy;
            if (!busy)
            {
                progressBar.Value = 0;
            }

            SetStatus(status ?? (busy ? "Working ..." : "Ready."));
        }

        /// <summary>Runs work in the background with progress, log, cancel and error handling.</summary>
        private async Task<bool> RunAsync(string title, Action<IProgress<int>, CancellationToken> work)
        {
            if (_busy)
            {
                return false;
            }

            _cancel = new CancellationTokenSource();
            CancellationToken token = _cancel.Token;
            IProgress<int> progress = CreateProgress();
            SetBusy(true, title + " ...");
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                await Task.Run(() => work(progress, token), token);
                Log(string.Format("{0}: finished after {1:0.0} s.", title, watch.Elapsed.TotalSeconds));
                return true;
            }
            catch (OperationCanceledException)
            {
                Log(title + ": cancelled.");
                return false;
            }
            catch (Exception ex)
            {
                Log(title + ": ERROR - " + ex.Message);
                MessageBox.Show(this, ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                _cancel.Dispose();
                _cancel = null;
                SetBusy(false);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            _cancel?.Cancel();
            SetStatus("Cancelling ...");
        }

        private void BtnClearLog_Click(object sender, EventArgs e) => txtLog.Clear();

        private void BtnSaveLog_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog { Filter = "Text|*.txt", FileName = "MapMakerLog.txt" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, txtLog.Text);
                }
            }
        }

        private TransitionSet GetTransitions(Action<string> log)
        {
            if (_transitions == null || _transitions.Count == 0)
            {
                _transitions = UoMapCompiler.LoadTransitions(MapDataPaths.TransitionsDir, log);
            }

            return _transitions;
        }

        // ================================================================ project bar

        private void LoadMapSizes()
        {
            _mapSizes.Clear();
            cbMapSize.Items.Clear();
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(MapDataPaths.MapInfoFile);
                foreach (XmlElement element in xml.SelectNodes("//Maps/Map"))
                {
                    MapInfo info = new MapInfo(element);
                    if (!info.IsSeparator)
                    {
                        _mapSizes.Add(info);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("MapInfo.xml could not be read (" + ex.Message + ") - default sizes used.");
            }

            if (_mapSizes.Count == 0)
            {
                _mapSizes.Add(new MapInfo("Felucca (7168x4096)", 0, 7168, 4096, 5120));
                _mapSizes.Add(new MapInfo("Felucca (6144x4096)", 0, 6144, 4096, 5120));
                _mapSizes.Add(new MapInfo("Ilshenar (2304x1600)", 2, 2304, 1600, 0));
                _mapSizes.Add(new MapInfo("Malas (2560x2048)", 3, 2560, 2048, 0));
                _mapSizes.Add(new MapInfo("Tokuno (1448x1448)", 4, 1448, 1448, 0));
                _mapSizes.Add(new MapInfo("Ter Mur (1280x4096)", 5, 1280, 4096, 0));
            }

            if (!_mapSizes.Any(m => m.IsCustom))
            {
                _mapSizes.Add(new MapInfo("Custom size ...", 0, -1, -1, 0));
            }

            cbMapSize.Items.AddRange(_mapSizes.Cast<object>().ToArray());
        }

        private void CbMapSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapInfo info = SelectedMapSize;
            if (info == null)
            {
                return;
            }

            bool custom = info.IsCustom;
            numWidth.Enabled = custom;
            numHeight.Enabled = custom;
            if (!custom)
            {
                numWidth.Value = info.XSize;
                numHeight.Value = info.YSize;
                numMapNumber.Value = info.MapNumber;
                if (_settings != null)
                {
                    _settings.DungeonStartX = info.DungeonX;
                    propertyGenerator.Refresh();
                }
            }
        }

        private void BtnBrowseProject_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Project folder (Terrain.bmp, Altitude.bmp and the mul files are stored here)";
                dialog.UseDescriptionForTitle = true;
                if (Directory.Exists(ProjectFolder))
                {
                    dialog.SelectedPath = ProjectFolder;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtProject.Text = dialog.SelectedPath;
                    if (File.Exists(TerrainPath) && MessageBox.Show(this, "This folder contains a map. Load it?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        LoadMapFromProject(true);
                    }
                }
            }
        }

        private void BtnOpenProjectFolder_Click(object sender, EventArgs e)
        {
            OpenFolder(MapDataPaths.EnsureDirectory(ProjectFolder));
        }

        private void OpenFolder(string folder)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log("Could not open the folder: " + ex.Message);
            }
        }

        // ================================================================ current map

        private void SetDocument(MapDocument doc, bool modified, string source)
        {
            _document = doc;
            _previewDocument = null;
            _documentModified = modified;
            _heightBackup = null;
            btnHeightUndo.Enabled = false;
            UpdatePreview();
            if (source != null)
            {
                Log(source);
            }
        }

        private void MarkDocumentModified()
        {
            _documentModified = true;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            MapDocument doc = _previewDocument ?? _document;
            Image old = picPreview.Image;
            if (doc == null || _terrains == null)
            {
                picPreview.Image = null;
                lblMapInfo.Text = "No map loaded. Create a blank map, generate one or load it from the project.";
            }
            else
            {
                MapDocument.RenderMode mode = rbPrevAltitude.Checked ? MapDocument.RenderMode.Altitude : rbPrevRelief.Checked ? MapDocument.RenderMode.Relief : MapDocument.RenderMode.Terrain;
                int size = Math.Max(256, Math.Max(picPreview.Width, picPreview.Height));
                picPreview.Image = doc.RenderPreview(Math.Min(1400, size * 2), Math.Min(1400, size * 2), mode, _terrains, _altitudes);
                lblMapInfo.Text = _previewDocument != null
                    ? string.Format("PREVIEW {0}x{1} (1 pixel = {2} tiles) - not saved. Click 'Generate full map' to create it.", _previewDocument.Width, _previewDocument.Height, Math.Max(1, MapWidth / Math.Max(1, _previewDocument.Width)))
                    : string.Format("{0} x {1} tiles{2}\r\n{3}", doc.Width, doc.Height, _documentModified ? "  (unsaved changes)" : string.Empty, HeightTools.Describe(doc, _altitudes));
            }

            old?.Dispose();
        }

        private void PreviewMode_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && rb.Checked)
            {
                UpdatePreview();
            }
        }

        private bool EnsureDocument()
        {
            if (_document != null)
            {
                return true;
            }

            if (File.Exists(TerrainPath))
            {
                return LoadMapFromProject(false);
            }

            MessageBox.Show(this, "There is no map yet. Create a blank map, generate one or load it from the project folder.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private bool LoadMapFromProject(bool askDiscard)
        {
            if (askDiscard && _documentModified && MessageBox.Show(this, "Discard the unsaved changes of the current map?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return false;
            }

            if (!File.Exists(TerrainPath))
            {
                MessageBox.Show(this, "Not found: " + TerrainPath, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                MapDocument doc = MapDocument.Load(TerrainPath, AltitudePath, _terrains, _altitudes, Log);
                SetDocument(doc, false, string.Format("Loaded {0} ({1}x{2}).", TerrainPath, doc.Width, doc.Height));
                if (!MapDocument.IsValidUoSize(doc.Width, doc.Height, out string error))
                {
                    Log("Warning: " + error);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log("Loading failed: " + ex.Message);
                MessageBox.Show(this, ex.Message, "Load map", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool SaveMapToProject(bool askOverwrite)
        {
            if (_document == null)
            {
                return false;
            }

            try
            {
                MapDataPaths.EnsureDirectory(ProjectFolder);
                if (askOverwrite && (File.Exists(TerrainPath) || File.Exists(AltitudePath)) &&
                    MessageBox.Show(this, string.Format("Overwrite\r\n{0}\r\n{1} ?", TerrainPath, AltitudePath), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return false;
                }

                _document.Save(TerrainPath, AltitudePath, _terrains, _altitudes);
                _documentModified = false;
                UpdatePreview();
                Log("Saved " + TerrainPath + " + " + Path.GetFileName(AltitudePath));
                return true;
            }
            catch (Exception ex)
            {
                Log("Saving failed: " + ex.Message);
                MessageBox.Show(this, ex.Message, "Save map", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void BtnLoadMap_Click(object sender, EventArgs e) => LoadMapFromProject(true);

        private void BtnSaveMap_Click(object sender, EventArgs e)
        {
            if (_previewDocument != null && _document == null)
            {
                MessageBox.Show(this, "The preview is only a low resolution sketch. Click 'Generate full map' first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (EnsureDocument())
            {
                SaveMapToProject(false);
            }
        }

        private void BtnExportPng_Click(object sender, EventArgs e)
        {
            MapDocument doc = _previewDocument ?? _document;
            if (doc == null)
            {
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog { Filter = "PNG|*.png", FileName = "MapPreview.png" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                MapDocument.RenderMode mode = rbPrevAltitude.Checked ? MapDocument.RenderMode.Altitude : rbPrevRelief.Checked ? MapDocument.RenderMode.Relief : MapDocument.RenderMode.Terrain;
                using (Bitmap bmp = doc.RenderPreview(doc.Width, doc.Height, mode, _terrains, _altitudes))
                {
                    bmp.Save(dialog.FileName, ImageFormat.Png);
                }

                Log("Exported " + dialog.FileName);
            }
        }

        private void MiClose_Click(object sender, EventArgs e) => Close();

        private void MiManual_Click(object sender, EventArgs e) => tabControl.SelectedTab = tabHelp;

        private void MiExportPalettes_Click(object sender, EventArgs e)
        {
            try
            {
                _terrains.Table.SaveACT();
                _terrains.Table.SaveACO();
                _altitudes.Table.SaveACT();
                _altitudes.Table.SaveACO();
                Log("Palettes exported to " + Path.Combine(MapDataPaths.DataRoot, "Photoshop"));
            }
            catch (Exception ex)
            {
                Log("Export failed: " + ex.Message);
            }
        }

        // ================================================================ editor

        private void BtnEditMap_Click(object sender, EventArgs e)
        {
            OpenEditor(null);
        }

        private void OpenEditor(string stampPath)
        {
            if (_editor != null && !_editor.IsDisposed)
            {
                _editor.BringToFront();
                if (stampPath != null)
                {
                    _editor.LoadStamp(stampPath);
                }

                return;
            }

            if (_document == null && File.Exists(TerrainPath))
            {
                LoadMapFromProject(false);
            }

            if (_document == null)
            {
                // start with an empty map of the selected size
                byte fill = (byte)TerrainChoice.SelectedId(cbDefaultTerrain, TerrainIds.Grass);
                SetDocument(MapDocument.CreateBlank(MapWidth, MapHeight, fill, _altitudes.BaseKeyFor(_terrains, fill)), true, "New blank map for the editor.");
            }

            _editor = new MapEditorForm(_document, _terrains, _altitudes, TerrainPath, AltitudePath) { ConfirmOnClose = false };
            _editor.DocumentChanged += Editor_DocumentChanged;
            _editor.FormClosed += (s, a) =>
            {
                if (_editor != null && _editor.Document == _document && _editor.IsModified)
                {
                    _documentModified = true;
                }

                _editor = null;
                UpdatePreview();
            };
            if (stampPath != null)
            {
                _editor.LoadStamp(stampPath);
            }

            _editor.Show(this);
        }

        private void Editor_DocumentChanged(object sender, EventArgs e)
        {
            if (_editor == null)
            {
                return;
            }

            bool sameFiles = string.Equals(_editor.TerrainPath, TerrainPath, StringComparison.OrdinalIgnoreCase);
            if (_editor.Document != _document)
            {
                // the editor created / opened another map: it becomes the current map
                _document = _editor.Document;
                _previewDocument = null;
            }

            // saved into the project files = not modified; new / opened elsewhere = not in the project yet
            _documentModified = !sameFiles || _editor.IsModified;

            UpdatePreview();
        }

        // ================================================================ tab 1: blank map

        private void ChkBlankTerrainAltitude_CheckedChanged(object sender, EventArgs e)
        {
            numBlankAltitude.Enabled = !chkBlankTerrainAltitude.Checked;
        }

        private void BtnCreateBlank_Click(object sender, EventArgs e)
        {
            if (!MapDocument.IsValidUoSize(MapWidth, MapHeight, out string error))
            {
                MessageBox.Show(this, error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_documentModified && MessageBox.Show(this, "Discard the unsaved changes of the current map?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            byte terrain = (byte)TerrainChoice.SelectedId(cbDefaultTerrain, TerrainIds.Grass);
            byte key = chkBlankTerrainAltitude.Checked
                ? _altitudes.BaseKeyFor(_terrains, terrain)
                : _altitudes.Encode(_terrains.IsWater(terrain, _altitudes) ? AltitudeType.Water : AltitudeType.Land, (int)numBlankAltitude.Value);
            MapDocument doc = MapDocument.CreateBlank(MapWidth, MapHeight, terrain, key);

            int dungeonX = SelectedMapSize?.IsCustom == false ? SelectedMapSize.DungeonX : _settings.DungeonStartX;
            if (chkBlankDungeon.Checked && dungeonX > 0 && dungeonX < MapWidth)
            {
                byte blankKey = _altitudes.Encode(AltitudeType.Land, -80);
                for (int y = 0; y < doc.Height; y++)
                {
                    for (int x = dungeonX; x < doc.Width; x++)
                    {
                        int i = doc.Index(x, y);
                        doc.Terrain[i] = TerrainIds.Blank;
                        doc.Altitude[i] = blankKey;
                    }
                }
            }

            SetDocument(doc, true, string.Format("Blank map {0}x{1} created ({2}).", MapWidth, MapHeight, _terrains.DisplayName(terrain)));
            SaveMapToProject(true);
        }

        // ================================================================ tab 2: generator

        private void SetupBiomeGrid()
        {
            gridBiomes.Columns.Clear();
            gridBiomes.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = nameof(BiomeRule.Enabled), HeaderText = "On", FillWeight = 12 });
            gridBiomes.Columns.Add(new DataGridViewComboBoxColumn
            {
                DataPropertyName = nameof(BiomeRule.TerrainId),
                HeaderText = "Terrain",
                DataSource = _terrainChoices,
                ValueMember = nameof(TerrainChoice.Id),
                DisplayMember = nameof(TerrainChoice.Name),
                ValueType = typeof(int),
                FillWeight = 60,
                FlatStyle = FlatStyle.Flat
            });
            gridBiomes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BiomeRule.Coverage), HeaderText = "Land %", FillWeight = 20 });
            gridBiomes.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(BiomeRule.PatchSize), HeaderText = "Patch size", FillWeight = 22, ToolTipText = "Typical size of one area in tiles" });
            gridBiomes.Columns.Add(new DataGridViewComboBoxColumn
            {
                DataPropertyName = nameof(BiomeRule.Placement),
                HeaderText = "Where",
                DataSource = Enum.GetValues(typeof(BiomePlacement)),
                ValueType = typeof(BiomePlacement),
                FillWeight = 34,
                FlatStyle = FlatStyle.Flat
            });
            RefreshBiomeGrid();
        }

        private void RefreshBiomeGrid()
        {
            if (_settings.Biomes == null)
            {
                _settings.Biomes = new List<BiomeRule>();
            }

            _biomes = new BindingList<BiomeRule>(_settings.Biomes);
            gridBiomes.DataSource = _biomes;
        }

        private void GridBiomes_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            SetStatus("Invalid value: " + e.Exception?.Message);
        }

        private void GridBiomes_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // apply check box / combo box changes immediately
            if (gridBiomes.IsCurrentCellDirty && !(gridBiomes.CurrentCell is DataGridViewTextBoxCell))
            {
                gridBiomes.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void BtnAddBiome_Click(object sender, EventArgs e)
        {
            _biomes.Add(new BiomeRule(TerrainIds.Forest, 10, 150, BiomePlacement.Anywhere));
        }

        private void BtnRemoveBiome_Click(object sender, EventArgs e)
        {
            if (gridBiomes.CurrentRow?.DataBoundItem is BiomeRule rule)
            {
                _biomes.Remove(rule);
            }
        }

        private void BtnBiomeUp_Click(object sender, EventArgs e) => MoveBiome(-1);

        private void BtnBiomeDown_Click(object sender, EventArgs e) => MoveBiome(1);

        private void MoveBiome(int direction)
        {
            if (!(gridBiomes.CurrentRow?.DataBoundItem is BiomeRule rule))
            {
                return;
            }

            int index = _biomes.IndexOf(rule);
            int target = index + direction;
            if (target < 0 || target >= _biomes.Count)
            {
                return;
            }

            _biomes.RemoveAt(index);
            _biomes.Insert(target, rule);
            gridBiomes.CurrentCell = gridBiomes.Rows[target].Cells[1];
        }

        private void BtnRandomSeed_Click(object sender, EventArgs e)
        {
            _settings.Seed = new Random().Next(1, int.MaxValue);
            propertyGenerator.Refresh();
            Log("New seed: " + _settings.Seed);
        }

        private void BtnPresetDefaults_Click(object sender, EventArgs e)
        {
            int dungeonX = _settings.DungeonStartX;
            _settings = GeneratorSettings.CreateDefault();
            _settings.DungeonStartX = dungeonX;
            propertyGenerator.SelectedObject = _settings;
            RefreshBiomeGrid();
        }

        private void BtnPresetSave_Click(object sender, EventArgs e)
        {
            gridBiomes.EndEdit();
            using (SaveFileDialog dialog = new SaveFileDialog { Filter = "Generator preset|*.xml", InitialDirectory = MapDataPaths.EnsureDirectory(MapDataPaths.PresetsDir), FileName = "MyWorld.xml" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _settings.Save(dialog.FileName);
                    Log("Preset saved: " + dialog.FileName);
                }
            }
        }

        private void BtnPresetLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Filter = "Generator preset|*.xml", InitialDirectory = MapDataPaths.EnsureDirectory(MapDataPaths.PresetsDir) })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    _settings = GeneratorSettings.LoadFrom(dialog.FileName);
                    propertyGenerator.SelectedObject = _settings;
                    RefreshBiomeGrid();
                    Log("Preset loaded: " + dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Load preset", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLoadSketch_Click(object sender, EventArgs e)
        {
            string file = ChooseImage("Load sketch (terrain colours, [00] No Draw / transparent = generator decides)");
            if (file != null)
            {
                LoadSketch(file);
            }
        }

        private void LoadSketch(string file)
        {
            try
            {
                using (Bitmap bmp = MapDocument.LoadBitmapUnlocked(file))
                {
                    _sketch = TemplateImporter.ImageToTerrain(bmp, _terrains, false);
                    _sketchWidth = bmp.Width;
                    _sketchHeight = bmp.Height;
                }

                lblSketchInfo.Text = string.Format("Sketch: {0} ({1}x{2})", Path.GetFileName(file), _sketchWidth, _sketchHeight);
                Log("Sketch loaded: " + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Sketch", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClearSketch_Click(object sender, EventArgs e)
        {
            _sketch = null;
            lblSketchInfo.Text = "Sketch: none";
        }

        private void BtnLoadHeightTemplate_Click(object sender, EventArgs e)
        {
            string file = ChooseImage("Load height map (grey scale: dark = sea, bright = mountains)");
            if (file != null)
            {
                LoadHeightTemplate(file);
            }
        }

        private void LoadHeightTemplate(string file)
        {
            try
            {
                using (Bitmap bmp = MapDocument.LoadBitmapUnlocked(file))
                {
                    _heightTemplate = TemplateImporter.ImageToGrey(bmp);
                    _heightTemplateWidth = bmp.Width;
                    _heightTemplateHeight = bmp.Height;
                }

                lblHeightTemplateInfo.Text = string.Format("Height map: {0} ({1}x{2})", Path.GetFileName(file), _heightTemplateWidth, _heightTemplateHeight);
                Log("Height map loaded: " + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Height map", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClearHeightTemplate_Click(object sender, EventArgs e)
        {
            _heightTemplate = null;
            lblHeightTemplateInfo.Text = "Height map: none";
        }

        private string ChooseImage(string title)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = "Images|*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|All files|*.*";
                dialog.InitialDirectory = Directory.Exists(txtTemplateFolder.Text) ? txtTemplateFolder.Text : MapDataPaths.EnsureDirectory(MapDataPaths.TemplatesDir);
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
            }
        }

        private GeneratorInput CreateGeneratorInput(int step)
        {
            gridBiomes.EndEdit();
            return new GeneratorInput
            {
                Width = MapWidth,
                Height = MapHeight,
                Settings = _settings,
                Step = step,
                Sketch = _sketch,
                SketchWidth = _sketchWidth,
                SketchHeight = _sketchHeight,
                HeightTemplate = _heightTemplate,
                TemplateWidth = _heightTemplateWidth,
                TemplateHeight = _heightTemplateHeight
            };
        }

        private async void BtnPreview_Click(object sender, EventArgs e)
        {
            if (!MapDocument.IsValidUoSize(MapWidth, MapHeight, out string error))
            {
                MessageBox.Show(this, error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int step = Math.Max(1, (int)Math.Ceiling(Math.Max(MapWidth, MapHeight) / 1024.0));
            GeneratorInput input = CreateGeneratorInput(step);
            MapDocument result = null;
            MapGenerator generator = new MapGenerator(_terrains, _altitudes);
            if (await RunAsync("Preview", (progress, token) => result = generator.Generate(input, progress, null, token)))
            {
                _previewDocument = result;
                UpdatePreview();
            }
        }

        private async void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (!MapDocument.IsValidUoSize(MapWidth, MapHeight, out string error))
            {
                MessageBox.Show(this, error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_documentModified && MessageBox.Show(this, "The current map has unsaved changes. Replace it with a new generated map?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            GeneratorInput input = CreateGeneratorInput(1);
            bool repair = chkRepairAfterGenerate.Checked;
            MapDocument result = null;
            MapGenerator generator = new MapGenerator(_terrains, _altitudes);
            Log(string.Format("Generating {0}x{1}, seed {2} ...", input.Width, input.Height, _settings.Seed));
            bool ok = await RunAsync("Generate map", (progress, token) =>
            {
                result = generator.Generate(input, progress, Log, token);
                if (repair)
                {
                    TransitionSet transitions = GetTransitions(Log);
                    int changed = TransitionRepair.Repair(result, _terrains, _altitudes, transitions, 4, progress, Log, token);
                    Log(string.Format("Transition repair: {0} tiles changed.", changed));
                }
            });

            if (!ok || result == null)
            {
                return;
            }

            SetDocument(result, true, null);
            if (chkSaveAfterGenerate.Checked)
            {
                SaveMapToProject(true);
            }
        }

        // ================================================================ tab 3: templates

        private void BtnTemplateBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Folder with template images / patterns";
                dialog.UseDescriptionForTitle = true;
                if (Directory.Exists(txtTemplateFolder.Text))
                {
                    dialog.SelectedPath = txtTemplateFolder.Text;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtTemplateFolder.Text = dialog.SelectedPath;
                    RefreshTemplates();
                }
            }
        }

        private void BtnTemplateRefresh_Click(object sender, EventArgs e) => RefreshTemplates();

        private void BtnTemplateOpenFolder_Click(object sender, EventArgs e) => OpenFolder(MapDataPaths.EnsureDirectory(txtTemplateFolder.Text));

        private async void RefreshTemplates()
        {
            int version = ++_templateLoadVersion;
            string folder = txtTemplateFolder.Text;
            listTemplates.BeginUpdate();
            listTemplates.Items.Clear();
            listTemplates.EndUpdate();
            foreach (Image image in _templateImages.Images)
            {
                image.Dispose();
            }

            _templateImages.Images.Clear();
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            if (!Directory.Exists(folder) && string.Equals(folder, MapDataPaths.TemplatesDir, StringComparison.OrdinalIgnoreCase))
            {
                MapDataPaths.EnsureDirectory(folder);
            }

            List<string> files = TemplateImporter.ListImages(folder);
            foreach (string file in files)
            {
                ListViewItem item = new ListViewItem(Path.GetFileNameWithoutExtension(file)) { Tag = file, ToolTipText = file };
                listTemplates.Items.Add(item);
            }

            if (files.Count == 0)
            {
                lblTemplateInfo.Text = "No images in this folder. Put PNG / BMP / JPG files into it and click Refresh.";
            }

            // thumbnails in the background
            foreach (ListViewItem item in listTemplates.Items.Cast<ListViewItem>().ToList())
            {
                string file = (string)item.Tag;
                Bitmap thumb = await Task.Run(() =>
                {
                    try
                    {
                        return TemplateImporter.LoadThumbnail(file, 96);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });

                if (version != _templateLoadVersion || IsDisposed)
                {
                    thumb?.Dispose();
                    return;
                }

                if (thumb != null)
                {
                    _templateImages.Images.Add(file, thumb);
                    item.ImageKey = file;
                }
            }
        }

        private string SelectedTemplate => listTemplates.SelectedItems.Count > 0 ? listTemplates.SelectedItems[0].Tag as string : null;

        private void ListTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            string file = SelectedTemplate;
            Image old = picTemplate.Image;
            picTemplate.Image = null;
            old?.Dispose();
            if (file == null)
            {
                return;
            }

            try
            {
                Bitmap bmp = MapDocument.LoadBitmapUnlocked(file);
                lblTemplateInfo.Text = string.Format("{0}\r\n{1} x {2} pixels, {3}", file, bmp.Width, bmp.Height, bmp.PixelFormat);
                picTemplate.Image = bmp;
            }
            catch (Exception ex)
            {
                lblTemplateInfo.Text = "Cannot read the image: " + ex.Message;
            }
        }

        private async void BtnTemplateApply_Click(object sender, EventArgs e)
        {
            string file = SelectedTemplate;
            if (file == null)
            {
                MessageBox.Show(this, "Select an image first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (rbTplSketch.Checked)
            {
                LoadSketch(file);
                tabControl.SelectedTab = tabGenerator;
                return;
            }

            if (rbTplHeight.Checked)
            {
                LoadHeightTemplate(file);
                tabControl.SelectedTab = tabGenerator;
                return;
            }

            if (rbTplStamp.Checked)
            {
                OpenEditor(file);
                return;
            }

            // convert to a map
            if (!MapDocument.IsValidUoSize(MapWidth, MapHeight, out string error))
            {
                MessageBox.Show(this, error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_documentModified && MessageBox.Show(this, "Discard the unsaved changes of the current map?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            TemplateScaleMode mode = (TemplateScaleMode)Enum.Parse(typeof(TemplateScaleMode), (string)cbScaleMode.SelectedItem);
            bool natural = chkNaturalOnly.Checked;
            byte fill = (byte)TerrainChoice.SelectedId(cbTemplateFill, TerrainIds.DeepWater);
            int width = MapWidth;
            int height = MapHeight;
            MapDocument result = null;
            if (await RunAsync("Convert template", (progress, token) =>
            {
                using (Bitmap bmp = MapDocument.LoadBitmapUnlocked(file))
                {
                    progress.Report(20);
                    result = TemplateImporter.ToDocument(bmp, width, height, mode, natural, fill, _terrains, _altitudes);
                    progress.Report(100);
                }
            }))
            {
                SetDocument(result, true, string.Format("Template {0} converted to {1}x{2}.", Path.GetFileName(file), width, height));
                SaveMapToProject(true);
            }
        }

        // ================================================================ tab 4: heights

        private HeightFilter CurrentHeightFilter()
        {
            HeightFilter filter = new HeightFilter { TerrainId = TerrainChoice.SelectedId(cbFilterTerrain, -1) };
            switch (cbFilterType.SelectedIndex)
            {
                case 1:
                    filter.Type = AltitudeType.Water;
                    break;
                case 2:
                    filter.Type = AltitudeType.Land;
                    break;
                case 3:
                    filter.Type = AltitudeType.Mountain;
                    break;
                case 4:
                    filter.Type = AltitudeType.Other;
                    break;
            }

            return filter;
        }

        private async void RunHeightOperation(string title, Func<MapDocument, HeightFilter, int> operation)
        {
            if (!EnsureDocument())
            {
                return;
            }

            MapDocument doc = _document;
            HeightFilter filter = CurrentHeightFilter();
            byte[] backup = (byte[])doc.Altitude.Clone();
            int changed = 0;
            if (await RunAsync(title, (progress, token) => changed = operation(doc, filter)))
            {
                _heightBackup = backup;
                btnHeightUndo.Enabled = true;
                lblHeightStats.Text = string.Format("{0}: {1} tiles changed.  {2}", title, changed, HeightTools.Describe(doc, _altitudes));
                Log(lblHeightStats.Text);
                MarkDocumentModified();
                RefreshEditor();
            }
        }

        private void RefreshEditor()
        {
            if (_editor != null && !_editor.IsDisposed && _editor.Document == _document)
            {
                _editor.Invalidate(true);
                foreach (Control control in _editor.Controls)
                {
                    if (control is MapCanvas canvas)
                    {
                        canvas.RefreshMap();
                    }
                }
            }
        }

        private void BtnOffset_Click(object sender, EventArgs e)
        {
            int offset = (int)numOffset.Value;
            RunHeightOperation("Raise / lower", (doc, filter) => HeightTools.Offset(doc, _altitudes, filter, offset));
        }

        private void BtnScale_Click(object sender, EventArgs e)
        {
            double factor = (double)numScale.Value;
            RunHeightOperation("Multiply height", (doc, filter) => HeightTools.Scale(doc, _altitudes, filter, factor));
        }

        private void BtnSmooth_Click(object sender, EventArgs e)
        {
            int passes = (int)numSmooth.Value;
            RunHeightOperation("Smooth", (doc, filter) => HeightTools.Smooth(doc, _altitudes, filter, passes));
        }

        private void BtnRoughen_Click(object sender, EventArgs e)
        {
            int amplitude = (int)numRoughAmp.Value;
            int size = (int)numRoughSize.Value;
            int seed = new Random().Next();
            RunHeightOperation("Roughen", (doc, filter) => HeightTools.Roughen(doc, _altitudes, filter, amplitude, size, seed));
        }

        private void BtnClamp_Click(object sender, EventArgs e)
        {
            int min = (int)numClampMin.Value;
            int max = (int)numClampMax.Value;
            RunHeightOperation("Limit height", (doc, filter) => HeightTools.Clamp(doc, _altitudes, filter, min, max));
        }

        private void BtnProfile_Click(object sender, EventArgs e)
        {
            int slope = (int)numProfileSlope.Value;
            int max = (int)numProfileMax.Value;
            int terrain = TerrainChoice.SelectedId(cbProfileTerrain, TerrainIds.Rock);
            int seed = new Random().Next();
            RunHeightOperation("Mountain profile", (doc, filter) => HeightTools.MountainProfile(doc, _altitudes, terrain, slope, max, seed));
        }

        private void BtnResetAltitude_Click(object sender, EventArgs e)
        {
            RunHeightOperation("Altitude from terrain", (doc, filter) => HeightTools.ResetFromTerrain(doc, _terrains, _altitudes, filter));
        }

        private void BtnHeightUndo_Click(object sender, EventArgs e)
        {
            if (_heightBackup == null || _document == null || _heightBackup.Length != _document.Altitude.Length)
            {
                return;
            }

            Buffer.BlockCopy(_heightBackup, 0, _document.Altitude, 0, _heightBackup.Length);
            _heightBackup = null;
            btnHeightUndo.Enabled = false;
            lblHeightStats.Text = "Last height change undone.  " + HeightTools.Describe(_document, _altitudes);
            MarkDocumentModified();
            RefreshEditor();
        }

        // ================================================================ tab 5: build mul

        private string OutputFolder => string.IsNullOrWhiteSpace(txtOutputFolder.Text) ? ProjectFolder : txtOutputFolder.Text.Trim();

        private string ImportFolder => string.IsNullOrWhiteSpace(txtImportFolder.Text) ? Path.Combine(ProjectFolder, "Import Files") : txtImportFolder.Text.Trim();

        private void BtnOutputBrowse_Click(object sender, EventArgs e)
        {
            string folder = BrowseFolder("Output folder for the mul files", OutputFolder);
            if (folder != null)
            {
                txtOutputFolder.Text = folder;
            }
        }

        private void BtnImportBrowse_Click(object sender, EventArgs e)
        {
            string folder = BrowseFolder("Folder with static XML files (<Static_Tiles><Tile TileID X Y Z Hue/>)", ImportFolder);
            if (folder != null)
            {
                txtImportFolder.Text = folder;
            }
        }

        private string BrowseFolder(string description, string start)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog { Description = description, UseDescriptionForTitle = true })
            {
                if (Directory.Exists(start))
                {
                    dialog.SelectedPath = start;
                }

                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.SelectedPath : null;
            }
        }

        private async void BtnCheckTransitions_Click(object sender, EventArgs e)
        {
            if (!EnsureDocument())
            {
                return;
            }

            MapDocument doc = _document;
            List<string> result = null;
            if (await RunAsync("Check transitions", (progress, token) => result = UoMapCompiler.CheckTransitions(doc, _terrains, GetTransitions(Log), 30, token)))
            {
                if (result.Count == 0)
                {
                    Log("All terrain borders have transitions.");
                }
                else
                {
                    Log("Terrain combinations without transition:");
                    foreach (string line in result)
                    {
                        Log("  " + line);
                    }
                }
            }
        }

        private async void BtnRepairTransitions_Click(object sender, EventArgs e)
        {
            if (!EnsureDocument())
            {
                return;
            }

            MapDocument doc = _document;
            int changed = 0;
            if (await RunAsync("Repair transitions", (progress, token) => changed = TransitionRepair.Repair(doc, _terrains, _altitudes, GetTransitions(Log), 6, progress, Log, token)))
            {
                Log(string.Format("Transition repair: {0} tiles changed.", changed));
                if (changed > 0)
                {
                    MarkDocumentModified();
                    RefreshEditor();
                }
            }
        }

        private async void BtnBuildMul_Click(object sender, EventArgs e)
        {
            if (!EnsureDocument())
            {
                return;
            }

            MapDocument doc = _document;
            if (!MapDocument.IsValidUoSize(doc.Width, doc.Height, out string error))
            {
                MessageBox.Show(this, error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int mapNumber = (int)numMapNumber.Value;
            string output = OutputFolder;
            string mapFile = Path.Combine(output, string.Format("Map{0}.mul", mapNumber));
            string question = string.Format("Build the mul files for map {0} ({1}x{2}) in\r\n{3} ?", mapNumber, doc.Width, doc.Height, output);
            if (File.Exists(mapFile))
            {
                question += "\r\n\r\nExisting Map/StaIdx/Statics files with this number are overwritten!";
            }

            if (MessageBox.Show(this, question, "Build MUL", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            if (_documentModified && MessageBox.Show(this, "Save the current map (Terrain.bmp / Altitude.bmp) to the project first?", "Build MUL", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SaveMapToProject(false);
            }

            CompileOptions options = new CompileOptions
            {
                OutputFolder = output,
                MapNumber = mapNumber,
                RandomStatics = chkRandomStatics.Checked,
                ImportStatics = chkImportStatics.Checked,
                ImportFolder = ImportFolder,
                Seed = (int)numCompileSeed.Value
            };
            bool repair = chkRepairBeforeBuild.Checked;
            UoMapCompiler compiler = new UoMapCompiler(_terrains, _altitudes);
            bool ok = await RunAsync("Build MUL", (progress, token) =>
            {
                MapDocument source = doc;
                if (repair)
                {
                    source = doc.Clone();
                    TransitionRepair.Repair(source, _terrains, _altitudes, GetTransitions(Log), 4, null, Log, token);
                }

                compiler.Compile(source, options, progress, Log, token);
            });

            if (ok)
            {
                SetStatus("MUL files written to " + output);
            }
        }

        // ================================================================ tools menu (old helper forms)

        private CreateDataIndexForm createDataIndexForm;

        private void createDataIndexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (createDataIndexForm == null || createDataIndexForm.IsDisposed)
            {
                createDataIndexForm = new CreateDataIndexForm();
                createDataIndexForm.Show();
            }
            else
            {
                createDataIndexForm.BringToFront();
            }
        }

        private CreateStaticsForm createStaticsForm;

        private void createStaticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (createStaticsForm == null || createStaticsForm.IsDisposed)
            {
                createStaticsForm = new CreateStaticsForm();
                createStaticsForm.Show();
            }
            else
            {
                createStaticsForm.BringToFront();
            }
        }

        private XMLHEXSearchForm xmlHexSearchForm;

        private void xMLHexSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (xmlHexSearchForm == null || xmlHexSearchForm.IsDisposed)
            {
                xmlHexSearchForm = new XMLHEXSearchForm();
                xmlHexSearchForm.Show();
            }
            else
            {
                xmlHexSearchForm.BringToFront();
            }
        }

        private CreateTransitions createTransitions;

        private void transitionEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (createTransitions == null || createTransitions.IsDisposed)
            {
                createTransitions = new CreateTransitions();
                createTransitions.FormClosed += (s, a) => _transitions = null; // reload changed transitions next time
                createTransitions.Show();
            }
            else
            {
                createTransitions.BringToFront();
            }
        }

        // ================================================================ tab 6: data viewer (old)

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListBox1.SelectedItem == null)
            {
                return;
            }

            if (i_Menu == 0 && ListBox1.SelectedItem is ClsTerrain terrain)
            {
                PropertyGrid1.SelectedObject = terrain;
                try
                {
                    PictureBox1.Image = Art.GetLand(terrain.TileID);
                }
                catch (Exception)
                {
                    PictureBox1.Image = null; // client files not loaded
                }
            }
            else if (i_Menu == 1 && ListBox1.SelectedItem is ClsElevation altitude)
            {
                PropertyGrid1.SelectedObject = altitude;
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            i_Menu = 0;
            lbTerrainList.Text = "Terrain List";
            i_Terrain.Load();
            i_Terrain.Display(ListBox1);
            PictureBox1.Visible = true;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (i_Terrain.TerrainHash.Count == 0)
            {
                MessageBox.Show(this, "Load the terrain list first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            i_Terrain.Save();
            Log("Terrain.xml saved. Restart the Map Maker to use the changes.");
        }

        private void terrainACTToolStripMenuItem1_Click(object sender, EventArgs e) => _terrains.Table.SaveACT();

        private void terrainACOToolStripMenuItem1_Click(object sender, EventArgs e) => _terrains.Table.SaveACO();

        private void AltitudeACTToolStripMenuItem_Click(object sender, EventArgs e) => _altitudes.Table.SaveACT();

        private void AltitudeACOToolStripMenuItem_Click(object sender, EventArgs e) => _altitudes.Table.SaveACO();

        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            i_Menu = 1;
            lbTerrainList.Text = "Altitude List";
            i_Altitude.Load();
            i_Altitude.Display(ListBox1);
            PictureBox1.Visible = false;
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // was: i_Terrain.SaveACT() (wrong table, wrong format)
            if (i_Altitude.AltitudeHash.Count == 0)
            {
                MessageBox.Show(this, "Load the altitude list first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            i_Altitude.Save();
            Log("Altitude.xml saved. Restart the Map Maker to use the changes.");
        }
    }
}
