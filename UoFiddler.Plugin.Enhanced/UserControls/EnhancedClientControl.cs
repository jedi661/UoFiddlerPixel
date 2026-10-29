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
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.Core;

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
        private readonly EcUopBrowserControl _browserControl;

        private EcClient _client;

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
            toolStrip.Items.Add(_infoLabel);

            _artControl = new EcArtControl();
            _tileArtControl = new EcTileArtControl();
            _stringsControl = new EcStringsControl();
            _browserControl = new EcUopBrowserControl();

            _tileArtControl.ShowInArtRequested += (source, id) =>
            {
                _tabs.SelectedIndex = 0;
                _artControl.ShowImage(source, id);
            };

            _tabs = new TabControl { Dock = DockStyle.Fill };
            AddTab("Art / Textures", _artControl);
            AddTab("TileArt", _tileArtControl);
            AddTab("String Dictionary", _stringsControl);
            AddTab("UOP Browser", _browserControl);
            _tabs.SelectedIndexChanged += (s, e) => ActivateCurrent();

            Controls.Add(_tabs);
            Controls.Add(toolStrip);

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

        private IEcEditor[] Editors => new IEcEditor[] { _artControl, _tileArtControl, _stringsControl, _browserControl };

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client?.Dispose();
                _client = null;
            }
            base.Dispose(disposing);
        }
    }
}
