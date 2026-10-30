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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Analysis;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Main tab of the Enhanced Client plugin: choose the client folder, then work with the sub editors.
    /// New editors (gumps, animations, maps, UI, ...) are added as further tabs here.
    /// </summary>
    public sealed partial class EnhancedClientControl : UserControl
    {
        private const string FolderSetting = "ClientFolder";

        private const string DictionarySetting = "NameDictionary";

        private EcClient _client;
        private CancellationTokenSource _analyzeCancel;

        public EnhancedClientControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;

            ecTileArtControl.ShowInArtRequested += (source, id) =>
            {
                tabControl.SelectedIndex = 0;
                ecArtControl.ShowImage(source, id);
            };

            string dictionaryPath = EcSettings.Get(DictionarySetting);
            if (!string.IsNullOrEmpty(dictionaryPath) && File.Exists(dictionaryPath) && EcClient.NameDictionary == null)
            {
                try
                {
                    EcClient.NameDictionary = EcNameDictionary.Load(dictionaryPath);
                }
                catch (Exception)
                {
                    EcClient.NameDictionary = null;
                }
            }

            string folder = EcSettings.Get(FolderSetting);
            if (string.IsNullOrEmpty(folder))
            {
                try
                {
                    folder = Files.RootDir;
                }
                catch (Exception)
                {
                    folder = string.Empty;
                }
            }
            toolStripTextBoxFolder.Text = folder ?? string.Empty;
            Disposed += OnControlDisposed;
        }

        private IEcEditor[] Editors => new IEcEditor[]
        {
            ecArtControl, ecTileArtControl, ecGumpsControl, ecHuesControl, ecMobileAnimationControl, ecAnimDefinitionControl, ecMultisControl,
            ecMapControl, ecSoundsControl, ecClilocControl, ecStringsControl, ecFileTreeControl, ecUopBrowserControl
        };

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && _client == null && !string.IsNullOrEmpty(toolStripTextBoxFolder.Text) && File.Exists(Path.Combine(toolStripTextBoxFolder.Text, EcClient.TileArtFileName)))
            {
                // open the remembered folder automatically when it looks like an EC folder
                LoadFolder(toolStripTextBoxFolder.Text);
            }
        }

        private void ActivateCurrent()
        {
            if (tabControl.SelectedTab?.Controls.Count > 0 && tabControl.SelectedTab.Controls[0] is IEcEditor editor)
            {
                editor.OnActivated();
            }
        }

        private void Browse()
        {
            using (var dialog = new FolderBrowserDialog { Description = "Choose the Enhanced Client folder", SelectedPath = toolStripTextBoxFolder.Text })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    toolStripTextBoxFolder.Text = dialog.SelectedPath;
                    LoadFolder(dialog.SelectedPath);
                }
            }
        }

        private void LoadFolder(string folder)
        {
            folder = (folder ?? string.Empty).Trim().Trim('"');
            if (!Directory.Exists(folder))
            {
                EcDialogs.ShowError(this, $"Folder not found:\r\n{folder}");
                return;
            }

            if (_client != null && _client.HasUnsavedChanges && !EcDialogs.Confirm(this, "There are unsaved changes. Load anyway and lose them?"))
            {
                return;
            }

            _client?.Dispose();
            _client = null;

            try
            {
                _client = new EcClient(folder);
                EcSettings.Set(FolderSetting, folder);

                string[] uops = _client.GetUopFiles().Select(Path.GetFileName).ToArray();
                string[] wanted = { EcClient.TileArtFileName, EcClient.StringDictionaryFileName, EcClient.TextureFileName, EcClient.LegacyTextureFileName };
                string[] missing = wanted.Where(w => _client.FindFile(w) == null).ToArray();
                toolStripLabelInfo.Text = $"{uops.Length} UOP files" + (missing.Length > 0 ? $" – missing: {string.Join(", ", missing)}" : string.Empty);
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "The folder can't be opened.", ex);
                return;
            }

            foreach (IEcEditor editor in Editors)
            {
                editor.SetClient(_client);
            }
            ActivateCurrent();
        }

        private async void AnalyzeFolder()
        {
            if (_analyzeCancel != null)
            {
                // second click cancels
                _analyzeCancel.Cancel();
                return;
            }

            string folder = (_client?.Folder ?? toolStripTextBoxFolder.Text ?? string.Empty).Trim().Trim('"');
            if (!Directory.Exists(folder))
            {
                EcDialogs.ShowError(this, "Choose the Enhanced Client folder first.");
                return;
            }

            string zipPath;
            using (var dialog = new SaveFileDialog
            {
                Filter = "Zip (*.zip)|*.zip",
                FileName = $"EC_Analysis_{DateTime.Now:yyyyMMdd_HHmm}.zip",
                Title = "Save analysis report"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                zipPath = dialog.FileName;
            }

            _analyzeCancel = new CancellationTokenSource();
            toolStripButtonAnalyze.Text = "Cancel analysis";
            string oldInfo = toolStripLabelInfo.Text;
            var progress = new Progress<string>(text =>
            {
                int bar = text.IndexOf('|');
                toolStripLabelInfo.Text = bar > 0 ? $"Analyzing {text.Substring(0, bar)}%: {text.Substring(bar + 1)}" : text;
            });

            try
            {
                CancellationToken token = _analyzeCancel.Token;
                await Task.Run(() => new EcFolderAnalyzer(folder).Run(zipPath, progress, token), token);
                toolStripLabelInfo.Text = oldInfo;
                MessageBox.Show(this,
                    $"The analysis was written to:\r\n{zipPath}\r\n\r\nSend this zip file to the developer. It contains a text report and small sample entries of every UOP file (no complete files).",
                    "Analyze folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                toolStripLabelInfo.Text = "Analysis cancelled.";
                TryDelete(zipPath);
            }
            catch (Exception ex)
            {
                toolStripLabelInfo.Text = oldInfo;
                EcDialogs.ShowError(this, "The analysis failed.", ex);
            }
            finally
            {
                _analyzeCancel.Dispose();
                _analyzeCancel = null;
                toolStripButtonAnalyze.Text = "Analyze folder...";
            }
        }

        private void LoadNameDictionary()
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = "Name dictionary (*.dic)|*.dic|All files (*.*)|*.*",
                Title = "Name dictionary, e.g. Dictionary.dic of the Mythic Package Editor"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    EcClient.NameDictionary = EcNameDictionary.Load(dialog.FileName);
                    EcSettings.Set(DictionarySetting, dialog.FileName);
                    toolStripLabelInfo.Text = $"{EcClient.NameDictionary.Count:N0} names loaded.";
                    if (_client != null)
                    {
                        LoadFolder(_client.Folder);
                    }
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "The file can't be read.", ex);
                }
            }
        }

        private void ExportNames()
        {
            if (_client == null)
            {
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = "Name dictionary (*.dic)|*.dic", FileName = "EnhancedNames.dic" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var dictionary = new EcNameDictionary();
                foreach (var package in _client.OpenPackages)
                {
                    foreach (var entry in package.Entries)
                    {
                        dictionary.Add(entry.Name);
                    }
                }
                dictionary.Save(dialog.FileName);
                toolStripLabelInfo.Text = $"{dictionary.Count:N0} names of the open files exported.";
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private void OnControlDisposed(object sender, EventArgs e)
        {
            _analyzeCancel?.Cancel();
            _client?.Dispose();
            _client = null;
        }

        // ==================================================================== event handlers (designer)

        private void OnFolderKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoadFolder(toolStripTextBoxFolder.Text);
            }
        }

        private void OnClickBrowse(object sender, EventArgs e) => Browse();

        private void OnClickLoad(object sender, EventArgs e) => LoadFolder(toolStripTextBoxFolder.Text);

        private void OnClickReload(object sender, EventArgs e) => LoadFolder(_client?.Folder ?? toolStripTextBoxFolder.Text);

        private void OnClickAnalyze(object sender, EventArgs e) => AnalyzeFolder();

        private void OnClickLoadDictionary(object sender, EventArgs e) => LoadNameDictionary();

        private void OnClickExportNames(object sender, EventArgs e) => ExportNames();

        private void OnClickForgetDictionary(object sender, EventArgs e)
        {
            EcClient.NameDictionary = null;
            EcSettings.Set(DictionarySetting, string.Empty);
            toolStripLabelInfo.Text = "Name dictionary removed (reload the folder).";
        }

        private void OnTabChanged(object sender, EventArgs e) => ActivateCurrent();

    }
}
