/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
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
    public sealed class EnhancedClientControl : UserControl
    {
        private const string FolderSetting = "ClientFolder";

        private readonly ToolStripTextBox _folderBox;
        private readonly ToolStripLabel _infoLabel;
        private readonly TabControl _tabs;
        private readonly EcArtControl _artControl;
        private readonly EcTileArtControl _tileArtControl;
        private readonly EcStringsControl _stringsControl;
        private readonly EcAnimDefinitionControl _animControl;
        private readonly EcMobileAnimationControl _mobileControl;
        private const string DictionarySetting = "NameDictionary";
        private readonly EcUopBrowserControl _browserControl;
        private readonly EcGumpsControl _gumpsControl;
        private readonly EcHuesControl _huesControl;
        private readonly EcMultisControl _multisControl;
        private readonly EcMapControl _mapControl;
        private readonly EcSoundsControl _soundsControl;
        private readonly EcClilocControl _clilocControl;
        private readonly EcFileTreeControl _fileTreeControl;

        private readonly ToolStripButton _analyzeButton;
        private EcClient _client;
        private CancellationTokenSource _analyzeCancel;

        public EnhancedClientControl()
        {
            Dock = DockStyle.Fill;

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _folderBox = new ToolStripTextBox { Width = 420, ToolTipText = "Folder of the Enhanced Client (where tileart.uop, Texture.uop, ... are)" };
            _folderBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadFolder(_folderBox.Text); } };
            _infoLabel = new ToolStripLabel(string.Empty) { ForeColor = Color.DimGray };

            toolStrip.Items.Add(new ToolStripLabel("Enhanced Client folder:"));
            toolStrip.Items.Add(_folderBox);
            toolStrip.Items.Add(new ToolStripButton("Browse...", null, (s, e) => Browse()));
            toolStrip.Items.Add(new ToolStripButton("Load", null, (s, e) => LoadFolder(_folderBox.Text)));
            toolStrip.Items.Add(new ToolStripButton("Reload", null, (s, e) => LoadFolder(_client?.Folder ?? _folderBox.Text)) { ToolTipText = "Close all files and read them again (unsaved changes are lost)" });
            toolStrip.Items.Add(new ToolStripSeparator());
            _analyzeButton = new ToolStripButton("Analyze folder...", null, (s, e) => AnalyzeFolder())
            {
                ToolTipText = "Write a report (zip) about every UOP file of the folder – send it to the developer to add support for unknown files"
            };
            toolStrip.Items.Add(_analyzeButton);

            var namesButton = new ToolStripDropDownButton("Names") { ToolTipText = "Lists of known packed file names" };
            namesButton.DropDownItems.Add("Load name dictionary (.dic)...", null, (s, e) => LoadNameDictionary());
            namesButton.DropDownItems.Add("Export known names of the open files (.dic)...", null, (s, e) => ExportNames());
            namesButton.DropDownItems.Add("Forget the name dictionary", null, (s, e) => { EcClient.NameDictionary = null; EcSettings.Set(DictionarySetting, string.Empty); _infoLabel.Text = "Name dictionary removed (reload the folder)."; });
            toolStrip.Items.Add(namesButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_infoLabel);

            _artControl = new EcArtControl();
            _tileArtControl = new EcTileArtControl();
            _stringsControl = new EcStringsControl();
            _animControl = new EcAnimDefinitionControl();
            _mobileControl = new EcMobileAnimationControl();
            _browserControl = new EcUopBrowserControl();
            _gumpsControl = new EcGumpsControl();
            _huesControl = new EcHuesControl();
            _multisControl = new EcMultisControl();
            _mapControl = new EcMapControl();
            _soundsControl = new EcSoundsControl();
            _clilocControl = new EcClilocControl();
            _fileTreeControl = new EcFileTreeControl();

            _tileArtControl.ShowInArtRequested += (source, id) =>
            {
                _tabs.SelectedIndex = 0;
                _artControl.ShowImage(source, id);
            };

            _tabs = new TabControl { Dock = DockStyle.Fill };
            // same order as the main tabs of UoFiddler
            AddTab("Items / Art", _artControl);
            AddTab("TileData", _tileArtControl);
            AddTab("Gumps", _gumpsControl);
            AddTab("Hues", _huesControl);
            AddTab("Animations", _mobileControl);
            AddTab("AnimData", _animControl);
            AddTab("Multis", _multisControl);
            AddTab("Map", _mapControl);
            AddTab("Sounds", _soundsControl);
            AddTab("Cliloc", _clilocControl);
            AddTab("String Dictionary", _stringsControl);
            AddTab("Interface / Data", _fileTreeControl);
            AddTab("UOP Browser", _browserControl);
            _tabs.SelectedIndexChanged += (s, e) => ActivateCurrent();

            Controls.Add(_tabs);
            Controls.Add(toolStrip);

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
            _folderBox.Text = folder ?? string.Empty;
        }

        private void AddTab(string title, Control control)
        {
            var page = new TabPage(title);
            page.Controls.Add(control);
            _tabs.TabPages.Add(page);
        }

        private IEcEditor[] Editors => new IEcEditor[]
        {
            _artControl, _tileArtControl, _gumpsControl, _huesControl, _mobileControl, _animControl, _multisControl,
            _mapControl, _soundsControl, _clilocControl, _stringsControl, _fileTreeControl, _browserControl
        };

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && _client == null && !string.IsNullOrEmpty(_folderBox.Text) && File.Exists(Path.Combine(_folderBox.Text, EcClient.TileArtFileName)))
            {
                // open the remembered folder automatically when it looks like an EC folder
                LoadFolder(_folderBox.Text);
            }
        }

        private void ActivateCurrent()
        {
            if (_tabs.SelectedTab?.Controls.Count > 0 && _tabs.SelectedTab.Controls[0] is IEcEditor editor)
            {
                editor.OnActivated();
            }
        }

        private void Browse()
        {
            using (var dialog = new FolderBrowserDialog { Description = "Choose the Enhanced Client folder", SelectedPath = _folderBox.Text })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _folderBox.Text = dialog.SelectedPath;
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
                _infoLabel.Text = $"{uops.Length} UOP files" + (missing.Length > 0 ? $" – missing: {string.Join(", ", missing)}" : string.Empty);
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

            string folder = (_client?.Folder ?? _folderBox.Text ?? string.Empty).Trim().Trim('"');
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
            _analyzeButton.Text = "Cancel analysis";
            string oldInfo = _infoLabel.Text;
            var progress = new Progress<string>(text =>
            {
                int bar = text.IndexOf('|');
                _infoLabel.Text = bar > 0 ? $"Analyzing {text.Substring(0, bar)}%: {text.Substring(bar + 1)}" : text;
            });

            try
            {
                CancellationToken token = _analyzeCancel.Token;
                await Task.Run(() => new EcFolderAnalyzer(folder).Run(zipPath, progress, token), token);
                _infoLabel.Text = oldInfo;
                MessageBox.Show(this,
                    $"The analysis was written to:\r\n{zipPath}\r\n\r\nSend this zip file to the developer. It contains a text report and small sample entries of every UOP file (no complete files).",
                    "Analyze folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                _infoLabel.Text = "Analysis cancelled.";
                TryDelete(zipPath);
            }
            catch (Exception ex)
            {
                _infoLabel.Text = oldInfo;
                EcDialogs.ShowError(this, "The analysis failed.", ex);
            }
            finally
            {
                _analyzeCancel.Dispose();
                _analyzeCancel = null;
                _analyzeButton.Text = "Analyze folder...";
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
                    _infoLabel.Text = $"{EcClient.NameDictionary.Count:N0} names loaded.";
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
                _infoLabel.Text = $"{dictionary.Count:N0} names of the open files exported.";
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _analyzeCancel?.Cancel();
                _client?.Dispose();
                _client = null;
            }
            base.Dispose(disposing);
        }
    }
}
