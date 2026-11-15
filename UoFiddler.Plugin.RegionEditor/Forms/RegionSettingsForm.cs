/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Settings of the region editor: server folders (ServUO, Sphere, POL),
 * export folder and options, language, design.
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.RegionEditor.Classes;
using UoFiddler.Plugin.RegionEditor.Formats;

namespace UoFiddler.Plugin.RegionEditor.Forms
{
    public partial class RegionSettingsForm : Form
    {
        private static readonly MergeMode[] _mergeModes = { MergeMode.ReplaceFile, MergeMode.ReplaceFacets, MergeMode.MergeByName };

        private static readonly StartupSource[] _startups =
            { StartupSource.LastProject, StartupSource.ServUo, StartupSource.Sphere, StartupSource.Pol, StartupSource.Nothing };

        private readonly System.Collections.Generic.Dictionary<string, string> _countCache = new System.Collections.Generic.Dictionary<string, string>();

        /// <summary>a "Load" button was clicked: the editor loads the regions of this server after the dialog</summary>
        public ServerKind? LoadRequest { get; private set; }

        public RegionSettingsForm()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            Icon = Options.GetFiddlerIcon();
            ApplyLanguage();
            LoadValues();
            ThemeManager.Apply(this);
        }

        private void ApplyLanguage()
        {
            Text = Loc.T("settings.title");
            _lblHeadServer.Text = Loc.T("settings.server");
            _lblServUo.Text = Loc.T("settings.servuo");
            _lblSphere.Text = Loc.T("settings.sphere");
            _lblPol.Text = Loc.T("settings.pol");
            _lblExport.Text = Loc.T("settings.export");
            _txtServUo.PlaceholderText = Loc.T("settings.servuo.hint");
            _txtSphere.PlaceholderText = Loc.T("settings.sphere.hint");
            _txtPol.PlaceholderText = Loc.T("settings.pol.hint");
            _txtExport.PlaceholderText = Loc.T("settings.export.hint");
            _lblHeadExport.Text = Loc.T("settings.exportoptions");
            _lblMerge.Text = Loc.T("settings.merge");
            _lblRectFormat.Text = Loc.T("settings.rectformat");
            _lblSphereEvents.Text = Loc.T("settings.sphereevents");
            _chkSkipHidden.Text = Loc.T("settings.skiphidden");
            _chkPolTexts.Text = Loc.T("settings.poltexts");
            _lblHeadEditor.Text = Loc.T("settings.editor");
            _lblLanguage.Text = Loc.T("lbl.language");
            _lblTheme.Text = Loc.T("settings.theme");
            _lblPriority.Text = Loc.T("settings.priority");
            _lblAlpha.Text = Loc.T("settings.alpha");
            _btnCancel.Text = Loc.T("btn.cancel");
            _btnOpenExport.Text = Loc.T("menu.export.open");
            _lblStartup.Text = Loc.T("settings.startup");
            _btnLoadServUo.Text = _btnLoadSphere.Text = _btnLoadPol.Text = Loc.T("settings.loadnow");

            _cmbStartup.Items.Clear();
            foreach (StartupSource st in _startups)
            {
                _cmbStartup.Items.Add(Loc.T("startup." + st));
            }

            _cmbMerge.Items.Clear();
            foreach (MergeMode m in _mergeModes)
            {
                _cmbMerge.Items.Add(Loc.T("merge." + m));
            }

            _cmbTheme.Items.Clear();
            _cmbTheme.Items.Add(Loc.T("theme.light"));
            _cmbTheme.Items.Add(Loc.T("theme.dark"));
        }

        private void LoadValues()
        {
            RegionEditorSettings s = RegionEditorSettings.Current;
            _txtServUo.Text = s.ServUoDir;
            _txtSphere.Text = s.SphereFile;
            _txtPol.Text = s.PolDir;
            _txtExport.Text = s.ExportDir;
            _cmbMerge.SelectedIndex = Math.Max(0, Array.IndexOf(_mergeModes, s.Merge));
            _cmbRectFormat.SelectedIndex = s.WidthHeight ? 0 : 1;
            _txtSphereEvents.Text = s.SphereEvents;
            _chkSkipHidden.Checked = s.SkipHidden;
            _chkPolTexts.Checked = s.PolGuardTexts;
            _cmbLanguage.SelectedIndex = Math.Max(0, Array.IndexOf(Loc.Languages, Loc.Language));
            _cmbTheme.SelectedIndex = s.Theme == EditorTheme.Dark ? 1 : 0;
            _numPriority.Value = Math.Max(0, Math.Min(255, s.DefaultPriority));
            _numAlpha.Value = Math.Max(0, Math.Min(255, s.FillAlpha));
            _cmbStartup.SelectedIndex = Math.Max(0, Array.IndexOf(_startups, s.Startup));
            UpdateServerState();
        }

        private void OnPathChanged(object sender, EventArgs e) => UpdateServerState();

        /// <summary>shows which server files were found and how many regions they have</summary>
        private void UpdateServerState()
        {
            string servuo = _txtServUo.Text.Trim();
            string servuoFile = servuo.Length > 0 ? RegionEditorSettings.ServUoRegionsPath(servuo) : string.Empty;
            string sphere = _txtSphere.Text.Trim().Trim('"');
            string pol = _txtPol.Text.Trim().Trim('"');

            _lblServerState.Text =
                $"ServUO: {State(ServerKind.ServUo, servuoFile)}\n" +
                $"Sphere: {State(ServerKind.Sphere, sphere)}\n" +
                $"POL: {State(ServerKind.Pol, pol)}\n" +
                Loc.F("settings.exportto", string.IsNullOrWhiteSpace(_txtExport.Text) ? RegionEditorSettings.Current.EffectiveExportDir : _txtExport.Text.Trim());

            _btnLoadServUo.Enabled = RegionEditorSettings.ServerExists(ServerKind.ServUo, servuoFile);
            _btnLoadSphere.Enabled = RegionEditorSettings.ServerExists(ServerKind.Sphere, sphere);
            _btnLoadPol.Enabled = RegionEditorSettings.ServerExists(ServerKind.Pol, pol);
        }

        /// <summary>"not set" / "not found" / "found – 123 regions"</summary>
        private string State(ServerKind kind, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Loc.T("settings.notset");
            }

            if (!RegionEditorSettings.ServerExists(kind, path))
            {
                return Loc.F("settings.notfound", path);
            }

            string key = kind + "|" + path;
            if (!_countCache.TryGetValue(key, out string text))
            {
                try
                {
                    RegionProject p = RegionEditorSettings.LoadServer(kind, path);
                    text = Loc.F("settings.found", p.AllRegions().Count(), p.Facets.Count);
                }
                catch (Exception ex)
                {
                    text = Loc.F("settings.unreadable", ex.Message);
                }

                _countCache[key] = text;
            }

            return $"✔ {path} – {text}";
        }

        /// <summary>"Load": stores the settings and lets the editor load this server</summary>
        private void OnClickLoadNow(object sender, EventArgs e)
        {
            if (!StoreValues())
            {
                return;
            }

            LoadRequest = sender == _btnLoadSphere ? ServerKind.Sphere : sender == _btnLoadPol ? ServerKind.Pol : ServerKind.ServUo;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            if (sender == _btnBrowseSphere)
            {
                if (File.Exists(_txtSphere.Text))
                {
                    _fileDialog.FileName = _txtSphere.Text;
                }

                if (_fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    _txtSphere.Text = _fileDialog.FileName;
                }

                return;
            }

            TextBox target = sender == _btnBrowseServUo ? _txtServUo : sender == _btnBrowsePol ? _txtPol : _txtExport;
            if (Directory.Exists(target.Text))
            {
                _folderDialog.SelectedPath = target.Text;
            }

            if (_folderDialog.ShowDialog(this) == DialogResult.OK)
            {
                target.Text = _folderDialog.SelectedPath;
            }
        }

        private void OnClickOpenExport(object sender, EventArgs e)
        {
            string dir = string.IsNullOrWhiteSpace(_txtExport.Text) ? RegionEditorSettings.Current.EffectiveExportDir : _txtExport.Text.Trim();
            try
            {
                Directory.CreateDirectory(dir);
                Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnClickOk(object sender, EventArgs e)
        {
            if (!StoreValues())
            {
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>takes the values and writes RegionEditor.json</summary>
        private bool StoreValues()
        {
            RegionEditorSettings s = RegionEditorSettings.Current;
            s.ServUoDir = _txtServUo.Text.Trim().Trim('"');
            s.SphereFile = _txtSphere.Text.Trim().Trim('"');
            s.PolDir = _txtPol.Text.Trim().Trim('"');
            s.ExportDir = _txtExport.Text.Trim().Trim('"');
            s.Merge = _mergeModes[Math.Max(0, _cmbMerge.SelectedIndex)];
            s.WidthHeight = _cmbRectFormat.SelectedIndex != 1;
            s.SphereEvents = _txtSphereEvents.Text.Trim();
            s.SkipHidden = _chkSkipHidden.Checked;
            s.PolGuardTexts = _chkPolTexts.Checked;
            s.Theme = _cmbTheme.SelectedIndex == 1 ? EditorTheme.Dark : EditorTheme.Light;
            s.DefaultPriority = (int)_numPriority.Value;
            s.FillAlpha = (int)_numAlpha.Value;
            s.Startup = _startups[Math.Max(0, _cmbStartup.SelectedIndex)];
            if (_cmbLanguage.SelectedIndex >= 0)
            {
                s.Language = Loc.Languages[_cmbLanguage.SelectedIndex];
            }

            if (!s.Save())
            {
                MessageBox.Show(this, Loc.F("settings.saveerror", RegionEditorSettings.FileName, s.LastError), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (_cmbLanguage.SelectedIndex >= 0)
            {
                Loc.Language = Loc.Languages[_cmbLanguage.SelectedIndex];
            }

            return true;
        }
    }
}
