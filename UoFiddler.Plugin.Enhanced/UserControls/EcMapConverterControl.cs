/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Maps;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Map converter (like UOEMC, in both directions): classic map*.mul + statics ↔ Enhanced Client facet*.uop (+ radar
    /// image facet0*.dds), analysis and repair of both formats, new maps. The file names follow the map number.
    /// </summary>
    public sealed partial class EcMapConverterControl : UserControl, IEcEditor
    {
        private const string ClassicSetting = "MapConverterClassicFolder";
        private const string OutputSetting = "MapConverterOutputFolder";

        private EcClient _client;
        private CancellationTokenSource _cancel;
        private bool _updating;

        public EcMapConverterControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;

            // runtime only: defaults
            _updating = true;
            toolStripComboBoxMap.SelectedIndex = 0;
            string classic = EcSettings.Get(ClassicSetting);
            if (string.IsNullOrEmpty(classic))
            {
                try
                {
                    classic = Ultima.Files.RootDir;
                }
                catch (Exception)
                {
                    classic = string.Empty;
                }
            }
            textBoxClassic.Text = classic ?? string.Empty;
            textBoxOutput.Text = EcSettings.Get(OutputSetting) ?? string.Empty;
            SetDefaultSize();
            _updating = false;
            ShowHelp();
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            textBoxEc.Text = client?.Folder ?? string.Empty;
            if (string.IsNullOrWhiteSpace(textBoxOutput.Text) && client != null)
            {
                textBoxOutput.Text = Path.Combine(client.Folder, "MapConverter");
            }
            RefreshFiles();
        }

        public void OnActivated()
        {
            RefreshFiles();
        }

        // ==================================================================== files

        private int MapIndex => Math.Max(0, toolStripComboBoxMap.SelectedIndex);

        private MapFileNames Names => new MapFileNames(MapIndex, toolStripButtonX.Checked);

        private string Classic(string name) => Path.Combine(textBoxClassic.Text.Trim(), name);

        private string Ec(string name) => Path.Combine(textBoxEc.Text.Trim(), name);

        private string Output(string name) => Path.Combine(textBoxOutput.Text.Trim(), name);

        /// <summary>The classic map: map#.mul, else map#LegacyMUL.uop; null if neither exists.</summary>
        private string ClassicMapFile()
        {
            MapFileNames n = Names;
            if (File.Exists(Classic(n.MapMul)))
            {
                return Classic(n.MapMul);
            }
            return File.Exists(Classic(n.MapUop)) ? Classic(n.MapUop) : null;
        }

        private bool ToEc => radioButtonClassicToEc.Checked;

        private void RefreshFiles()
        {
            MapFileNames n = Names;
            listViewFiles.BeginUpdate();
            listViewFiles.Items.Clear();
            void Add(string role, string path, bool output = false, string missing = "missing")
            {
                var item = new ListViewItem(role);
                item.SubItems.Add(path);
                bool exists = File.Exists(path);
                item.SubItems.Add(output ? (exists ? "exists – replaced (.bak)" : "will be created") : (exists ? "found" : missing));
                item.SubItems.Add(exists ? EcImageHelper.FormatSize(new FileInfo(path).Length) : string.Empty);
                if (!exists && !output)
                {
                    item.ForeColor = Color.Firebrick;
                }
                else if (output)
                {
                    item.ForeColor = Color.DarkBlue;
                }
                listViewFiles.Items.Add(item);
            }

            string classicMap = ClassicMapFile();
            Add($"Classic map ({n.Title})", classicMap ?? Classic(n.MapMul), missing: $"missing (also no {n.MapUop})");
            Add("Classic statics index", Classic(n.StaIdx), missing: "missing (no statics)");
            Add("Classic statics", Classic(n.Statics), missing: "missing (no statics)");
            Add("Classic radar colors", Classic(MapFileNames.RadarCol), missing: "missing (no radar image)");
            Add($"EC facet ({n.Title})", Ec(n.Facet));
            Add("EC radar image", Ec(n.RadarDds), missing: "missing (optional)");
            if (ToEc)
            {
                Add("Output: EC facet", Output(n.Facet), true);
                if (checkBoxRadar.Checked)
                {
                    Add("Output: EC radar image", Output(n.RadarDds), true);
                }
            }
            else
            {
                Add("Output: classic map", Output(n.MapMul), true);
                if (checkBoxStatics.Checked)
                {
                    Add("Output: staidx", Output(n.StaIdx), true);
                    Add("Output: statics", Output(n.Statics), true);
                }
            }
            listViewFiles.EndUpdate();
        }

        private void SetDefaultSize()
        {
            (int w, int h) = MapFileNames.DefaultSize(MapIndex);
            if (w > 0)
            {
                toolStripTextBoxWidth.Text = w.ToString(CultureInfo.InvariantCulture);
                toolStripTextBoxHeight.Text = h.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Size from map#.mul (length) or from the facet (data.map); else the known size of the map number.</summary>
        private void DetectSize()
        {
            MapFileNames n = Names;
            string map = Classic(n.MapMul);
            string how = null;
            int w = 0, h = 0;
            if (File.Exists(map))
            {
                (w, h) = MapFileNames.SizeFromMapLength(MapIndex, new FileInfo(map).Length);
                how = $"from the length of {n.MapMul}";
            }
            if (w == 0 && File.Exists(Ec(n.Facet)))
            {
                try
                {
                    using (EcUopPackage package = EcUopPackage.Open(Ec(n.Facet)))
                    {
                        var facet = new EcFacet(package, n.Facet);
                        if (facet.HasSizeEntry)
                        {
                            (w, h) = (facet.Width, facet.Height);
                            how = $"from {n.Facet} (data.map)";
                        }
                    }
                }
                catch (Exception)
                {
                    // no size
                }
            }
            if (w == 0)
            {
                (w, h) = MapFileNames.DefaultSize(MapIndex);
                how = "known size of the map number";
            }
            if (w == 0)
            {
                Log("Size unknown (custom map): enter width and height.");
                return;
            }
            toolStripTextBoxWidth.Text = w.ToString(CultureInfo.InvariantCulture);
            toolStripTextBoxHeight.Text = h.ToString(CultureInfo.InvariantCulture);
            Log($"Size {w} x {h} ({how}).");
        }

        private bool TryGetSize(out int width, out int height)
        {
            bool ok = int.TryParse(toolStripTextBoxWidth.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out width) &
                      int.TryParse(toolStripTextBoxHeight.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out height);
            if (!ok || width <= 0 || height <= 0 || width % 8 != 0 || height % 8 != 0)
            {
                EcDialogs.ShowError(this, "Width and height must be numbers greater than 0 and a multiple of 8.");
                return false;
            }
            return true;
        }

        // ==================================================================== log / busy

        private void Log(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(Log), text);
                return;
            }
            textBoxLog.AppendText(text.Replace("\r\n", "\n").Replace("\n", Environment.NewLine) + Environment.NewLine);
        }

        private void SetBusy(bool busy)
        {
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            foreach (Control c in new Control[] { buttonConvert, buttonAnalyzeClassic, buttonAnalyzeEc, buttonRepairEc, buttonRepairClassic, buttonCreate })
            {
                c.Enabled = !busy;
            }
            buttonCancel.Enabled = busy;
            toolStripComboBoxMap.Enabled = !busy;
            if (!busy)
            {
                toolStripProgressBar.Value = 0;
            }
        }

        private Progress<int> MakeProgress() => new Progress<int>(p => toolStripProgressBar.Value = Math.Max(0, Math.Min(100, p)));

        /// <summary>Runs a job in the background with log, progress, cancel and error message.</summary>
        private async Task Run(string title, Func<IProgress<int>, CancellationToken, MapReport> job)
        {
            _cancel = new CancellationTokenSource();
            CancellationToken token = _cancel.Token;
            IProgress<int> progress = MakeProgress();
            SetBusy(true);
            toolStripStatusLabel.Text = title + "...";
            Log($"=== {title} – {DateTime.Now:HH:mm:ss}");
            try
            {
                MapReport report = await Task.Run(() => job(progress, token));
                Log(report.ToString().TrimEnd());
                toolStripStatusLabel.Text = $"{title}: done" + (report.Problems > 0 ? $", {report.Problems} problems" : string.Empty);
            }
            catch (OperationCanceledException)
            {
                Log("Cancelled.");
                toolStripStatusLabel.Text = $"{title}: cancelled";
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex.Message);
                toolStripStatusLabel.Text = $"{title}: error";
                EcDialogs.ShowError(this, $"{title} failed.", ex);
            }
            finally
            {
                SetBusy(false);
                RefreshFiles();
            }
        }

        private void PrepareOutput()
        {
            Directory.CreateDirectory(textBoxOutput.Text.Trim());
            EcSettings.Set(OutputSetting, textBoxOutput.Text.Trim());
        }

        /// <summary>After a facet in the client folder was written, the open copy of the client is read again.</summary>
        private void ReloadIfClientFile(string path)
        {
            if (_client != null && string.Equals(Path.GetDirectoryName(Path.GetFullPath(path)), Path.GetFullPath(_client.Folder).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase)
                && _client.IsOpen(Path.GetFileName(path)))
            {
                _client.ReloadPackage(Path.GetFileName(path));
                Log($"{Path.GetFileName(path)} was read again in the other tabs.");
            }
        }

        private Func<ushort, int, ushort?> HueColor()
        {
            return (hue, index) =>
            {
                try
                {
                    return (ushort)Ultima.Hues.GetHue(hue).Colors[index];
                }
                catch (Exception)
                {
                    return null;
                }
            };
        }

        private static int MaxItemId()
        {
            try
            {
                return Ultima.TileData.ItemTable?.Length ?? 0x10000;
            }
            catch (Exception)
            {
                return 0x10000;
            }
        }

        // ==================================================================== convert

        private async void Convert()
        {
            if (!TryGetSize(out int width, out int height) || string.IsNullOrWhiteSpace(textBoxOutput.Text))
            {
                if (string.IsNullOrWhiteSpace(textBoxOutput.Text))
                {
                    EcDialogs.ShowError(this, "Choose an output folder.");
                }
                return;
            }
            MapFileNames n = Names;
            var options = new MapConvertOptions { Statics = checkBoxStatics.Checked, Compress = checkBoxCompress.Checked };
            if (!string.IsNullOrWhiteSpace(textBoxTable.Text))
            {
                if (!File.Exists(textBoxTable.Text))
                {
                    EcDialogs.ShowError(this, "The land table file was not found.");
                    return;
                }
                options.LandTable = EcMapConverter.ReadLandTable(textBoxTable.Text);
                Log($"Land table: {options.LandTable.Count} replacements.");
            }
            PrepareOutput();

            if (ToEc)
            {
                string map = ClassicMapFile();
                if (map == null)
                {
                    EcDialogs.ShowError(this, $"{n.MapMul} / {n.MapUop} was not found in the classic folder.");
                    return;
                }
                string staIdx = Classic(n.StaIdx), statics = Classic(n.Statics), radar = Classic(MapFileNames.RadarCol);
                string outFacet = Output(n.Facet), outDds = Output(n.RadarDds);
                bool withRadar = checkBoxRadar.Checked;
                if (withRadar && !File.Exists(radar))
                {
                    Log("radarcol.mul was not found – no radar image.");
                    withRadar = false;
                }
                Func<ushort, int, ushort?> hue = HueColor();
                int index = MapIndex;
                await Run($"Classic → EC ({n.Title})", (progress, cancel) =>
                {
                    ClassicMap classic = ClassicMap.Open(map, options.Statics ? staIdx : null, options.Statics ? statics : null, width, height);
                    MapReport report = EcMapConverter.ClassicToEc(classic, index, outFacet, options, progress, cancel);
                    if (withRadar)
                    {
                        EcMapConverter.WriteRadarImage(classic, radar, outDds, hue, progress, cancel);
                        report.Info($"Saved {outDds}");
                    }
                    return report;
                });
                ReloadIfClientFile(outFacet);
            }
            else
            {
                string facetPath = Ec(n.Facet);
                if (!File.Exists(facetPath))
                {
                    EcDialogs.ShowError(this, $"{n.Facet} was not found in the Enhanced Client folder.");
                    return;
                }
                string outMap = Output(n.MapMul), outIdx = Output(n.StaIdx), outStatics = Output(n.Statics);
                await Run($"EC → Classic ({n.Title})", (progress, cancel) =>
                {
                    using (EcUopPackage package = EcUopPackage.Open(facetPath))
                    {
                        var facet = new EcFacet(package, n.Facet, width, height);
                        return EcMapConverter.EcToClassic(facet, outMap, outIdx, outStatics, options, progress, cancel);
                    }
                });
            }
        }

        // ==================================================================== analyze / repair

        private async void AnalyzeClassic(bool repair)
        {
            if (!TryGetSize(out int width, out int height))
            {
                return;
            }
            MapFileNames n = Names;
            string map = ClassicMapFile();
            if (map == null)
            {
                EcDialogs.ShowError(this, $"{n.MapMul} / {n.MapUop} was not found in the classic folder.");
                return;
            }
            string staIdx = Classic(n.StaIdx), statics = Classic(n.Statics);
            if (repair && (!File.Exists(staIdx) || !File.Exists(statics)))
            {
                EcDialogs.ShowError(this, $"{n.StaIdx} and {n.Statics} are needed for the repair.");
                return;
            }
            if (repair && !EcDialogs.Confirm(this, $"Write {n.StaIdx} and {n.Statics} again? The old files are kept as .bak.\r\n\r\nClose the client and other tools that use them."))
            {
                return;
            }
            bool duplicates = checkBoxClassicDuplicates.Checked;
            int maxId = checkBoxClassicInvalid.Checked ? MaxItemId() : 0;
            int maxCheck = MaxItemId();
            await Run(repair ? $"Repair classic statics ({n.Title})" : $"Analyze classic ({n.Title})", (progress, cancel) =>
            {
                ClassicMap classic = ClassicMap.Open(map, staIdx, statics, width, height);
                return repair
                    ? EcMapConverter.RepairClassicStatics(classic, staIdx, statics, duplicates, maxId, progress)
                    : EcMapConverter.AnalyzeClassic(classic, maxCheck, progress, cancel);
            });
        }

        private async void AnalyzeEc(bool repair)
        {
            if (!TryGetSize(out int width, out int height))
            {
                return;
            }
            MapFileNames n = Names;
            string facetPath = Ec(n.Facet);
            if (!File.Exists(facetPath))
            {
                EcDialogs.ShowError(this, $"{n.Facet} was not found in the Enhanced Client folder.");
                return;
            }
            if (repair && !EcDialogs.Confirm(this, $"Repair {n.Facet}? It is saved in place, the old file is kept as .bak.\r\n\r\nClose the client first."))
            {
                return;
            }
            EcMapConverter.EcRepairOptions options = repair
                ? new EcMapConverter.EcRepairOptions
                {
                    SizeEntry = checkBoxRepairSize.Checked,
                    Headers = checkBoxRepairHeaders.Checked,
                    BorderLayers = checkBoxRepairBorders.Checked,
                    MissingSectors = checkBoxRepairMissing.Checked,
                    Duplicates = checkBoxRepairDuplicates.Checked,
                    ExtraSectors = checkBoxRepairExtra.Checked
                }
                : null;
            await Run(repair ? $"Repair {n.Facet}" : $"Analyze {n.Facet}", (progress, cancel) =>
            {
                using (EcUopPackage package = EcUopPackage.Open(facetPath))
                {
                    var facet = new EcFacet(package, n.Facet, width, height);
                    MapReport report = EcMapConverter.AnalyzeEc(facet, options, progress, cancel);
                    if (repair && report.Fixed > 0)
                    {
                        package.Save(facetPath, true);
                        report.Info($"Saved {facetPath} (old file kept as .bak).");
                    }
                    return report;
                }
            });
            if (repair)
            {
                ReloadIfClientFile(facetPath);
            }
        }

        // ==================================================================== new map

        private async void CreateMap()
        {
            if (!TryGetSize(out int width, out int height) || string.IsNullOrWhiteSpace(textBoxOutput.Text))
            {
                return;
            }
            if (!EcDialogs.TryParseNumber(textBoxNewLand.Text, out long land) || land < 0 || land > ushort.MaxValue)
            {
                EcDialogs.ShowError(this, "Invalid land tile id.");
                return;
            }
            if (!checkBoxNewEc.Checked && !checkBoxNewClassic.Checked)
            {
                return;
            }
            PrepareOutput();
            MapFileNames n = Names;
            int index = MapIndex;
            sbyte z = (sbyte)numericUpDownNewZ.Value;
            bool ec = checkBoxNewEc.Checked, classic = checkBoxNewClassic.Checked;
            string outFacet = Output(n.Facet), outMap = Output(n.MapMul), outIdx = Output(n.StaIdx), outStatics = Output(n.Statics);
            await Run($"New map {index} ({width} x {height})", (progress, cancel) =>
            {
                var report = new MapReport();
                if (ec)
                {
                    report.Info(EcMapConverter.CreateEc(index, width, height, (ushort)land, z, outFacet, progress).ToString().TrimEnd());
                }
                if (classic)
                {
                    report.Info(EcMapConverter.CreateClassic(width, height, (ushort)land, z, outMap, outIdx, outStatics, progress).ToString().TrimEnd());
                }
                return report;
            });
        }

        // ==================================================================== help

        private void ShowHelp()
        {
            richTextBoxHelp.Text = radioButtonHelpGerman.Checked ? MapConverterHelp.German : MapConverterHelp.English;
        }

        // ==================================================================== event handlers (designer)

        private void OnMapChanged(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }
            SetDefaultSize();
            if (File.Exists(Classic(Names.MapMul)))
            {
                DetectSize();
            }
            RefreshFiles();
        }

        private void OnSizeChanged(object sender, EventArgs e) => RefreshFiles();

        private void OnClickDetect(object sender, EventArgs e) => DetectSize();

        private void OnClickRefresh(object sender, EventArgs e) => RefreshFiles();

        private void OnFolderChanged(object sender, EventArgs e)
        {
            EcSettings.Set(ClassicSetting, textBoxClassic.Text.Trim());
            RefreshFiles();
        }

        private string BrowseFolder(string current)
        {
            using (var dialog = new FolderBrowserDialog { SelectedPath = Directory.Exists(current) ? current : string.Empty })
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.SelectedPath : null;
            }
        }

        private void OnClickBrowseClassic(object sender, EventArgs e)
        {
            string folder = BrowseFolder(textBoxClassic.Text);
            if (folder != null)
            {
                textBoxClassic.Text = folder;
                OnFolderChanged(sender, e);
                DetectSize();
            }
        }

        private void OnClickBrowseEc(object sender, EventArgs e)
        {
            string folder = BrowseFolder(textBoxEc.Text);
            if (folder != null)
            {
                textBoxEc.Text = folder;
                RefreshFiles();
            }
        }

        private void OnClickBrowseOutput(object sender, EventArgs e)
        {
            string folder = BrowseFolder(textBoxOutput.Text);
            if (folder != null)
            {
                textBoxOutput.Text = folder;
                EcSettings.Set(OutputSetting, folder);
                RefreshFiles();
            }
        }

        private void OnClickBrowseTable(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "Land table (*.txt)|*.txt|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxTable.Text = dialog.FileName;
                }
            }
        }

        private void OnDirectionChanged(object sender, EventArgs e) => RefreshFiles();

        private void OnClickConvert(object sender, EventArgs e) => Convert();

        private void OnClickCancel(object sender, EventArgs e)
        {
            _cancel?.Cancel();
            Log("Cancel requested...");
        }

        private void OnClickAnalyzeClassic(object sender, EventArgs e) => AnalyzeClassic(false);

        private void OnClickAnalyzeEc(object sender, EventArgs e) => AnalyzeEc(false);

        private void OnClickRepairEc(object sender, EventArgs e) => AnalyzeEc(true);

        private void OnClickRepairClassic(object sender, EventArgs e) => AnalyzeClassic(true);

        private void OnClickCreate(object sender, EventArgs e) => CreateMap();

        private void OnHelpLanguageChanged(object sender, EventArgs e) => ShowHelp();
    }
}
