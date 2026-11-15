/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Region editor: regions of ServUO/RunUO, Sphere and POL on the maps of the
 * MapRegistry. The layout is in RegionEditorControl.Designer.cs (Visual
 * Studio designer), the code is split into:
 *   RegionEditorControl.cs            project, changes, undo/redo, keys
 *   RegionEditorControl.Tree.cs       region tree, search, drag & drop
 *   RegionEditorControl.Properties.cs properties and areas tab
 *   RegionEditorControl.Map.cs        map, tools, client
 *   RegionEditorControl.Files.cs      open/save, import/export, server folders
 *   RegionEditorControl.Check.cs      check and history tab
 *   RegionEditorControl.Language.cs   texts and design
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.MapViewer;
using UoFiddler.Plugin.RegionEditor.Classes;

namespace UoFiddler.Plugin.RegionEditor.UserControls
{
    public partial class RegionEditorControl : UserControl
    {
        private RegionProject _project = new RegionProject();
        private readonly History _history = new History();
        private readonly UltimaMapInfo _mapInfo = new UltimaMapInfo();

        /// <summary>project file (.uoregions), null = not saved yet</summary>
        private string _projectFile;

        private RegionNode _selected;
        private int _selectedRect = -1;
        private bool _updating;
        private bool _loaded;

        // several small changes of the same value are one undo step
        private string _lastRecordKey;
        private DateTime _lastRecordTime;

        public RegionEditorControl()
        {
            InitializeComponent();
        }

        private static bool IsDesigner(Control c) => LicenseManager.UsageMode == LicenseUsageMode.Designtime || FormsDesignerHelper.IsInDesignMode() || c.Site?.DesignMode == true;

        #region Load / unload

        private void OnControlLoad(object sender, EventArgs e)
        {
            if (_loaded || IsDesigner(this))
            {
                return;
            }

            _loaded = true;
            MapRegistryStore.EnsureLoaded();
            RegionEditorSettings settings = RegionEditorSettings.Current;

            _updating = true;
            _cmbType.Items.AddRange(RegionCatalog.Types.Cast<object>().ToArray());
            _cmbMusic.Items.Add(string.Empty);
            _cmbMusic.Items.AddRange(RegionCatalog.Music.Cast<object>().ToArray());
            _cmbEnMap.Items.Add(string.Empty);
            _cmbEnMap.Items.AddRange(RegionCatalog.Facets.Cast<object>().ToArray());
            _cmbSnap.SelectedItem = settings.Snap.ToString();
            if (_cmbSnap.SelectedIndex < 0)
            {
                _cmbSnap.SelectedIndex = 0;
            }

            _cmbLang.SelectedIndex = Math.Max(0, Array.IndexOf(Loc.Languages, Loc.Language));
            _miViewStatics.Checked = settings.ShowStatics;
            _miViewGrid.Checked = settings.ShowGrid;
            _miViewLabels.Checked = settings.ShowLabels;
            _miViewMinimap.Checked = settings.ShowMinimap;
            _miViewClient.Checked = settings.ShowClient;
            _updating = false;

            ApplyViewOptions();
            SetTool(MapTool.Select);
            BuildMapCombo(0);

            Loc.LanguageChanged += OnLanguageChanged;
            RegionEditorSettings.Changed += OnSettingsChanged;
            _history.Changed += OnHistoryChanged;
            MapRegistry.Changed += OnMapRegistryChanged;
            ControlEvents.FilePathChangeEvent += OnFilePathChanged;

            ApplyLanguage();
            ApplyTheme();
            BuildRecentMenu();
            UpdateAll();

            // the last project is opened again
            if (!string.IsNullOrEmpty(settings.LastProject) && System.IO.File.Exists(settings.LastProject))
            {
                OpenFile(settings.LastProject, false, true);
            }
        }

        private void UnhookEvents()
        {
            Loc.LanguageChanged -= OnLanguageChanged;
            RegionEditorSettings.Changed -= OnSettingsChanged;
            _history.Changed -= OnHistoryChanged;
            MapRegistry.Changed -= OnMapRegistryChanged;
            ControlEvents.FilePathChangeEvent -= OnFilePathChanged;
            _clientTimer?.Stop();
        }

        private void OnMapRegistryChanged(object sender, EventArgs e)
        {
            if (!IsDisposed)
            {
                BuildMapCombo(_mapView.MapId);
            }
        }

        private void OnFilePathChanged()
        {
            if (!IsDisposed)
            {
                BuildMapCombo(_mapView.MapId);
                _mapView.RefreshMap();
            }
        }

        private void OnSettingsChanged(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            ApplyViewOptions();
            ApplyTheme();
        }

        #endregion

        #region Changes and undo / redo

        /// <summary>stores the state for undo and runs the change</summary>
        private void Change(string description, Action action, string coalesceKey = null)
        {
            bool coalesce = coalesceKey != null && coalesceKey == _lastRecordKey && (DateTime.Now - _lastRecordTime).TotalSeconds < 2 && _history.CanUndo;
            if (!coalesce)
            {
                _history.Record(_project, description);
            }

            _lastRecordKey = coalesceKey;
            _lastRecordTime = DateTime.Now;
            action();
            UpdateAll();
        }

        private void OnClickUndo(object sender, EventArgs e) => Undo();

        private void OnClickRedo(object sender, EventArgs e) => Redo();

        private void Undo()
        {
            if (!_history.CanUndo)
            {
                return;
            }

            string text = _history.UndoText;
            ReplaceProject(_history.Undo(_project));
            SetMessage(Loc.F("msg.undone", text));
        }

        private void Redo()
        {
            if (!_history.CanRedo)
            {
                return;
            }

            string text = _history.RedoText;
            ReplaceProject(_history.Redo(_project));
            SetMessage(Loc.F("msg.redone", text));
        }

        /// <summary>the project object was exchanged (undo, open): keep the selection by id</summary>
        private void ReplaceProject(RegionProject project)
        {
            string selectedId = _selected?.Id;
            _project = project ?? new RegionProject();
            _lastRecordKey = null;
            _selected = selectedId == null ? null : _project.FindById(selectedId, out FacetRegions _, out RegionNode _);
            if (_selected == null)
            {
                _selectedRect = -1;
            }

            UpdateAll();
        }

        private void OnHistoryChanged(object sender, EventArgs e)
        {
            _btnUndo.Enabled = _btnHistUndo.Enabled = _history.CanUndo;
            _btnRedo.Enabled = _btnHistRedo.Enabled = _history.CanRedo;
            _btnUndo.ToolTipText = _history.CanUndo ? Loc.F("tip.undo", _history.UndoText) : Loc.T("btn.undo");
            _btnRedo.ToolTipText = _history.CanRedo ? Loc.F("tip.redo", _history.RedoText) : Loc.T("btn.redo");
            FillHistory();
            UpdateTitle();
        }

        /// <summary>refreshes everything that shows the project</summary>
        private void UpdateAll()
        {
            if (!_loaded)
            {
                return;
            }

            if (_selected != null && _project.FindById(_selected.Id, out FacetRegions facet, out RegionNode _) != null && facet.MapId != _mapView.MapId)
            {
                // the selected region is on another map
                _selected = null;
                _selectedRect = -1;
            }

            _mapView.Facet = CurrentFacet(false);
            _mapView.SelectedRegion = _selected;
            _mapView.SelectedRectIndex = _selectedRect;
            BuildTree();
            ShowProperties();
            FillRects();
            OnHistoryChanged(this, EventArgs.Empty);
            UpdateCount();
            _mapView.Invalidate();
        }

        private void UpdateTitle()
        {
            string name = _projectFile != null ? System.IO.Path.GetFileName(_projectFile)
                : _project.SourceFile != null ? System.IO.Path.GetFileName(_project.SourceFile) : Loc.T("project.unnamed");
            string dirty = _history.HasChanges(_project) ? " *" : string.Empty;
            if (Parent is TabPage page)
            {
                page.Text = $"{Loc.T("tab.title")}{dirty}";
            }

            _lblCount.ToolTipText = name + dirty;
        }

        private void UpdateCount()
        {
            FacetRegions facet = CurrentFacet(false);
            _lblCount.Text = Loc.F("status.count", facet?.AllRegions().Count() ?? 0, _project.AllRegions().Count());
            UpdateMapCounts();
            UpdateTitle();
        }

        private void SetMessage(string text, bool error = false)
        {
            _lblMessage.ForeColor = error ? ThemeManager.Palette.Error : ThemeManager.Palette.Text;
            _lblMessage.Text = text;
        }

        #endregion

        #region Keys

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.Z:
                    if (!(ActiveControlDeep() is TextBoxBase))
                    {
                        Undo();
                        return true;
                    }

                    break;
                case Keys.Control | Keys.Y:
                case Keys.Control | Keys.Shift | Keys.Z:
                    if (!(ActiveControlDeep() is TextBoxBase))
                    {
                        Redo();
                        return true;
                    }

                    break;
                case Keys.Control | Keys.F:
                    _txtSearch.Focus();
                    _txtSearch.SelectAll();
                    return true;
                case Keys.F2:
                    if (_tree.SelectedNode != null)
                    {
                        _tree.SelectedNode.BeginEdit();
                        return true;
                    }

                    break;
            }

            // tool keys only on the map
            if (_mapView.Focused)
            {
                switch (keyData)
                {
                    case Keys.V:
                        SetTool(MapTool.Select);
                        return true;
                    case Keys.H:
                        SetTool(MapTool.Pan);
                        return true;
                    case Keys.R:
                        SetTool(MapTool.Rect);
                        return true;
                    case Keys.P:
                        SetTool(MapTool.Polygon);
                        return true;
                    case Keys.C:
                        SetTool(MapTool.Circle);
                        return true;
                    case Keys.F:
                        SetTool(MapTool.Fill);
                        return true;
                    case Keys.G:
                        SetTool(MapTool.Go);
                        return true;
                    case Keys.E:
                        SetTool(MapTool.Entrance);
                        return true;
                    case Keys.Delete:
                        DeleteSelectedArea();
                        return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private Control ActiveControlDeep()
        {
            Control c = ActiveControl;
            while (c is ContainerControl cc && cc.ActiveControl != null)
            {
                c = cc.ActiveControl;
            }

            return c;
        }

        #endregion
    }
}
