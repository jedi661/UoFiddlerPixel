/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Region editor – texts in German / English / French and the light / dark design.
 * The designer holds the English texts, they are replaced at runtime.
 *
 ***************************************************************************/

using System;
using System.Windows.Forms;
using UoFiddler.Plugin.RegionEditor.Classes;

namespace UoFiddler.Plugin.RegionEditor.UserControls
{
    public partial class RegionEditorControl
    {
        private void OnLanguageSelected(object sender, EventArgs e)
        {
            if (!_updating && _cmbLang.SelectedIndex >= 0)
            {
                Loc.Language = Loc.Languages[_cmbLang.SelectedIndex];
            }
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            int index = Array.IndexOf(Loc.Languages, Loc.Language);
            if (_cmbLang.SelectedIndex != index)
            {
                _updating = true;
                _cmbLang.SelectedIndex = index;
                _updating = false;
            }

            ApplyLanguage();
            UpdateAll();
            if (_tabs.SelectedTab == _tabCheck)
            {
                RunCheck();
            }
        }

        private void OnClickTheme(object sender, EventArgs e)
        {
            RegionEditorSettings s = RegionEditorSettings.Current;
            s.Theme = s.Theme == EditorTheme.Dark ? EditorTheme.Light : EditorTheme.Dark;
            s.Save(); // raises Changed -> ApplyTheme
        }

        private void ApplyTheme()
        {
            ThemeManager.Apply(this);
            _btnTheme.Text = RegionEditorSettings.Current.Theme == EditorTheme.Dark ? Loc.T("btn.light") : Loc.T("btn.dark");
            _lblRegionInfo.ForeColor = ThemeManager.Palette.SubText;
            _colorImages.Clear();
            _tree.ImageList?.Images.Clear();
            BuildTree();
            if (_selected != null)
            {
                _btnColor.BackColor = RegionMapView.RegionColor(_selected);
            }
        }

        private static void T(ToolStripItem item, string key, string tipKey = null)
        {
            item.Text = Loc.T(key);
            if (tipKey != null)
            {
                item.ToolTipText = Loc.T(tipKey);
            }
        }

        private void ApplyLanguage()
        {
            if (Parent is TabPage page)
            {
                page.Text = Loc.T("tab.title");
            }

            // main row
            T(_ddFile, "menu.file");
            T(_miNew, "menu.new");
            T(_miOpen, "menu.open");
            T(_miRecent, "menu.recent");
            T(_miSave, "menu.save");
            T(_miSaveAs, "menu.saveas");
            T(_miImport, "menu.import");
            T(_miImportServUo, "fmt.servuo");
            T(_miImportSphere, "fmt.sphere");
            T(_miImportPol, "fmt.pol");
            T(_miImportPolFolder, "menu.import.polfolder");
            T(_miImportCsv, "fmt.csv");
            T(_miImportProject, "menu.import.project");
            T(_miExport, "menu.export");
            T(_miExpServUo, "fmt.servuo");
            T(_miExpSphere, "fmt.sphere");
            T(_miExpPolRegions, "fmt.polregions");
            T(_miExpPolJustice, "fmt.poljustice");
            T(_miExpPolMusic, "fmt.polmusic");
            T(_miExpCsv, "fmt.csv");
            T(_miExpTxt, "fmt.txt");
            T(_miExpHtml, "fmt.html");
            T(_miExpPng, "fmt.png");
            T(_miExpCSharp, "menu.export.csharp");
            T(_miExpAll, "menu.export.all");
            T(_miOpenExportDir, "menu.export.open");
            T(_miLoadServer, "menu.loadserver");
            T(_miSaveServer, "menu.saveserver");
            T(_miLoadServUo, "server.servuo");
            T(_miSaveServUo, "server.servuo");
            T(_miLoadSphere, "server.sphere");
            T(_miSaveSphere, "server.sphere");
            T(_miLoadPol, "server.pol");
            T(_miSavePol, "server.pol");
            T(_miSettings, "menu.settings");
            T(_btnUndo, "btn.undo");
            T(_btnRedo, "btn.redo");
            T(_lblMap, "lbl.map");
            T(_btnCheck, "btn.check", "tip.check");
            T(_btnSettings, "btn.settings", "tip.settings");
            T(_btnHelp, "btn.help");
            T(_lblLang, "lbl.language");
            _btnTheme.ToolTipText = Loc.T("tip.theme");
            _btnTheme.Text = RegionEditorSettings.Current.Theme == EditorTheme.Dark ? Loc.T("btn.light") : Loc.T("btn.dark");

            // tool row
            T(_btnToolSelect, "tool.select", "toolhelp.select");
            T(_btnToolPan, "tool.pan", "toolhelp.pan");
            T(_btnToolRect, "tool.rect", "toolhelp.rect");
            T(_btnToolPolygon, "tool.polygon", "toolhelp.polygon");
            T(_btnToolCircle, "tool.circle", "toolhelp.circle");
            T(_btnToolFill, "tool.fill", "toolhelp.fill");
            T(_btnToolGo, "tool.go", "toolhelp.go");
            T(_btnToolEntrance, "tool.entrance", "toolhelp.entrance");
            T(_lblSnap, "lbl.snap");
            _btnZoomIn.ToolTipText = Loc.T("tip.zoomin");
            _btnZoomOut.ToolTipText = Loc.T("tip.zoomout");
            _btnZoomFit.ToolTipText = Loc.T("tip.zoomfit");
            T(_ddView, "menu.view");
            T(_miViewStatics, "view.statics");
            T(_miViewGrid, "view.grid");
            T(_miViewLabels, "view.labels");
            T(_miViewMinimap, "view.minimap");
            T(_miViewClient, "view.client");
            T(_miViewRefresh, "view.refresh");
            T(_btnClientGoto, "btn.client", "tip.client");
            T(_btnSyncClient, "btn.follow", "tip.follow");

            // tree
            T(_btnAddRegion, "btn.addregion", "tip.addregion");
            T(_btnAddChild, "btn.addchild", "tip.addchild");
            _btnDuplicate.ToolTipText = Loc.T("tip.duplicate");
            _btnDeleteRegion.ToolTipText = Loc.T("tip.delete");
            _btnMoveUp.ToolTipText = Loc.T("tip.moveup");
            _btnMoveDown.ToolTipText = Loc.T("tip.movedown");
            _btnExpandAll.ToolTipText = Loc.T("tip.expandall");
            _btnCollapseAll.ToolTipText = Loc.T("tip.collapseall");
            _txtSearch.PlaceholderText = Loc.T("search.placeholder");
            T(_ctxAdd, "ctx.add");
            T(_ctxAddChild, "ctx.addchild");
            T(_ctxDuplicate, "ctx.duplicate");
            T(_ctxRename, "ctx.rename");
            T(_ctxDelete, "ctx.delete");
            T(_ctxZoomTo, "ctx.zoomto");
            T(_ctxSendGo, "ctx.sendgo");
            T(_ctxCopyXml, "ctx.copyxml");
            T(_ctxOptimize, "ctx.optimize");
            T(_ctxPolyToRects, "ctx.polytorects");
            T(_ctxToTop, "ctx.totop");
            T(_ctxHide, "ctx.hide");
            T(_ctxLock, "ctx.lock");

            // map context menu
            T(_ctxMapSelect, "ctx.mapselect");
            T(_ctxMapSetGo, "ctx.mapsetgo");
            T(_ctxMapSetEntrance, "ctx.mapsetentrance");
            T(_ctxMapDeleteRect, "ctx.mapdeleterect");
            T(_ctxMapSendChar, "ctx.mapsendchar");

            // tabs
            _tabProps.Text = Loc.T("tab.props");
            _tabRects.Text = Loc.T("tab.rects");
            _tabCheck.Text = Loc.T("tab.check");
            _tabHistory.Text = Loc.T("tab.history");

            _lblPName.Text = Loc.T("prop.name");
            _lblPType.Text = Loc.T("prop.type");
            _lblPPriority.Text = Loc.T("prop.priority") + ":";
            _lblPMusic.Text = Loc.T("prop.music");
            _lblPZ.Text = Loc.T("prop.z");
            _lblPGo.Text = Loc.T("prop.go");
            _lblPEntrance.Text = Loc.T("prop.entrance");
            _lblPGuards.Text = Loc.T("prop.guards");
            _chkGuardsOff.Text = Loc.T("prop.guardsoff");
            _txtGuardsType.PlaceholderText = Loc.T("prop.guardstype");
            _lblPRune.Text = Loc.T("prop.rune");
            _lblPOptions.Text = Loc.T("prop.options");
            _chkHidden.Text = Loc.T("prop.hidden");
            _chkLocked.Text = Loc.T("prop.locked");
            _lblPColor.Text = Loc.T("prop.color");
            _btnColorReset.Text = Loc.T("prop.colorreset");
            _lblPNotes.Text = Loc.T("prop.notes");
            _lblPExtra.Text = Loc.T("prop.extra");
            _colKey.HeaderText = Loc.T("col.key");
            _colValue.HeaderText = Loc.T("col.value");
            _toolTip.SetToolTip(_btnPickGo, Loc.T("msg.pickgo"));
            _toolTip.SetToolTip(_btnPickEntrance, Loc.T("msg.pickentrance"));
            _toolTip.SetToolTip(_chkZ, Loc.T("tip.zrange"));
            _toolTip.SetToolTip(_chkSmartNoHousing, Loc.T("tip.smartnohousing"));
            _toolTip.SetToolTip(_chkLogoutDelay, Loc.T("tip.logoutdelay"));
            _toolTip.SetToolTip(_gridProps, Loc.T("tip.extra"));
            _toolTip.SetToolTip(_numPriority, Loc.T("tip.priority"));

            _colRNo.Text = "#";
            _colRW.Text = Loc.T("col.width");
            _colRH.Text = Loc.T("col.height");
            _lblRW.Text = Loc.T("col.width") + ":";
            _lblRH.Text = Loc.T("col.height") + ":";
            T(_btnRectAdd, "btn.rectadd", "tip.rectadd");
            T(_btnRectApply, "btn.rectapply", "tip.rectapply");
            _btnRectDelete.ToolTipText = Loc.T("tip.rectdelete");
            T(_btnRectOptimize, "btn.rectoptimize", "tip.rectoptimize");
            _btnPolyToRects.ToolTipText = Loc.T("tip.polytorects");

            T(_btnRunCheck, "btn.runcheck");
            T(_btnFixSelected, "btn.fix", "tip.fix");
            T(_btnFixAll, "btn.fixall", "tip.fixall");
            _colIRegion.Text = Loc.T("col.region");
            _colIMsg.Text = Loc.T("col.problem");

            _btnHistUndo.ToolTipText = Loc.T("btn.undo");
            _btnHistRedo.ToolTipText = Loc.T("btn.redo");
            T(_btnHistJump, "btn.histjump", "tip.histjump");
            T(_btnHistClear, "btn.histclear");

            OnHistoryChanged(this, EventArgs.Empty);
            SetMessage(Loc.T("toolhelp." + _mapView.Tool.ToString().ToLowerInvariant()));
        }
    }
}
