/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Region editor – project files, import, export and the server folders.
 *
 *  - "Load from server" / "Save into server" use the folders of the settings
 *    (ServUO: <folder>\Data\Regions.xml, Sphere: areas file, POL: regions folder).
 *    Before a server file is written a backup "<file>.<date>.bak" is made.
 *  - "Everything into the export folder" writes all formats into the export
 *    folder, from there the files can be copied to any server.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.RegionEditor.Classes;
using UoFiddler.Plugin.RegionEditor.Forms;
using UoFiddler.Plugin.RegionEditor.Formats;

namespace UoFiddler.Plugin.RegionEditor.UserControls
{
    public partial class RegionEditorControl
    {
        private const string ProjectFilter = "Region project (*.uoregions)|*.uoregions";

        private static string AllFilter =>
            $"{Loc.T("filter.all")}|*.uoregions;*.xml;*.scp;*.cfg;*.csv|{ProjectFilter}|ServUO / RunUO (*.xml)|*.xml|Sphere (*.scp)|*.scp|POL (*.cfg)|*.cfg|CSV (*.csv)|*.csv";

        #region Project

        /// <returns>false = the user cancelled</returns>
        private bool AskSaveChanges()
        {
            if (!_history.HasChanges(_project) || _project.AllRegions().Count() == 0)
            {
                return true;
            }

            DialogResult result = MessageBox.Show(this, Loc.T("ask.savechanges"), Loc.T("tab.title"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                return false;
            }

            return result == DialogResult.No || SaveProject(false);
        }

        private void OnClickNew(object sender, EventArgs e)
        {
            if (!AskSaveChanges())
            {
                return;
            }

            _projectFile = null;
            _history.Clear();
            ReplaceProject(new RegionProject());
            _history.MarkSaved(_project);
            BuildMapCombo(_mapView.MapId);
            SetMessage(Loc.T("msg.newproject"));
        }

        private void OnClickOpen(object sender, EventArgs e)
        {
            if (!AskSaveChanges())
            {
                return;
            }

            using (var dialog = new OpenFileDialog { Filter = AllFilter, Title = Loc.T("menu.open") })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    OpenFile(dialog.FileName, false, false);
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e) => SaveProject(false);

        private void OnClickSaveAs(object sender, EventArgs e) => SaveProject(true);

        private bool SaveProject(bool askName)
        {
            string file = _projectFile;
            if (askName || file == null)
            {
                using (var dialog = new SaveFileDialog { Filter = ProjectFilter, Title = Loc.T("menu.saveas"), FileName = DefaultName(ProjectFormat.Extension) })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return false;
                    }

                    file = dialog.FileName;
                }
            }

            try
            {
                ProjectFormat.Save(_project, file);
                _projectFile = file;
                _history.MarkSaved(_project);
                RememberFile(file, true);
                SetMessage(Loc.F("msg.saved", file));
                return true;
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return false;
            }
        }

        private string DefaultName(string extension)
        {
            string name = _projectFile != null ? Path.GetFileNameWithoutExtension(_projectFile)
                : _project.SourceFile != null ? Path.GetFileNameWithoutExtension(_project.SourceFile) : "Regions";
            return name + extension;
        }

        /// <summary>opens any supported file; append = add to the current project</summary>
        private void OpenFile(string file, bool append, bool silent)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                RegionProject loaded = LoadAny(file);
                if (append)
                {
                    Change(Loc.F("hist.import", Path.GetFileName(file)), () => MergeProject(loaded));
                }
                else
                {
                    bool isProject = file.EndsWith(ProjectFormat.Extension, StringComparison.OrdinalIgnoreCase);
                    _projectFile = isProject ? file : null;
                    _history.Clear();
                    ReplaceProject(loaded);
                    _history.MarkSaved(_project);
                    if (isProject)
                    {
                        RememberFile(file, true);
                    }
                    else
                    {
                        RememberFile(file, false);
                    }
                }

                // show the first map that has regions
                FacetRegions first = _project.Facets.FirstOrDefault(f => f.MapId == _mapView.MapId && f.Regions.Count > 0) ??
                                     _project.Facets.FirstOrDefault(f => f.Regions.Count > 0);
                BuildMapCombo(first?.MapId ?? _mapView.MapId);
                if (first != null && first.MapId == _mapView.MapId)
                {
                    ShowMap(_mapView.MapDefinition);
                }

                SetMessage(Loc.F("msg.loaded", Path.GetFileName(file), loaded.AllRegions().Count(), loaded.Facets.Count));
                ReportUnknownFacets(loaded);
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    ShowError(ex);
                }
                else
                {
                    SetMessage(ex.Message, true);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ReportUnknownFacets(RegionProject project)
        {
            List<FacetRegions> unknown = project.Facets.Where(f => MapRegistry.GetById(f.MapId) == null).ToList();
            if (unknown.Count > 0)
            {
                MessageBox.Show(this, Loc.F("msg.unknownfacets", string.Join(", ", unknown.Select(f => $"{f.Name} ({f.MapId})"))), Loc.T("tab.title"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static RegionProject LoadAny(string file)
        {
            if (Directory.Exists(file))
            {
                return PolFormat.LoadFolder(file);
            }

            string ext = Path.GetExtension(file).ToLowerInvariant();
            switch (ext)
            {
                case ".uoregions":
                case ".json":
                    return ProjectFormat.Load(file);
                case ".scp":
                    return SphereFormat.Load(file);
                case ".cfg":
                    return PolFormat.Load(file);
                case ".csv":
                    {
                        RegionProject p = CsvFormat.Parse(File.ReadAllText(file));
                        p.SourceFile = file;
                        p.SourceFormat = "csv";
                        return p;
                    }

                default:
                    return ServUoFormat.Load(file);
            }
        }

        /// <summary>adds the regions of another project (facets with the same map are joined)</summary>
        private void MergeProject(RegionProject other)
        {
            foreach (FacetRegions facet in other.Facets)
            {
                FacetRegions target = _project.GetFacet(facet.MapId, facet.Name, true);
                target.Regions.AddRange(facet.Regions);
            }

            // ids must stay unique (the same file can be imported twice)
            var ids = new HashSet<string>();
            foreach (RegionNode r in _project.AllRegions())
            {
                if (!ids.Add(r.Id))
                {
                    r.Id = Guid.NewGuid().ToString("N");
                    ids.Add(r.Id);
                }
            }
        }

        private void RememberFile(string file, bool project)
        {
            RegionEditorSettings s = RegionEditorSettings.Current;
            s.AddRecent(file);
            if (project)
            {
                s.LastProject = file;
            }

            s.Save();
            BuildRecentMenu();
        }

        private void BuildRecentMenu()
        {
            foreach (ToolStripItem item in _miRecent.DropDownItems.Cast<ToolStripItem>().ToList())
            {
                _miRecent.DropDownItems.Remove(item);
                item.Dispose();
            }

            foreach (string file in RegionEditorSettings.Current.RecentFiles)
            {
                var item = new ToolStripMenuItem(file) { Tag = file };
                item.Click += OnClickRecent;
                _miRecent.DropDownItems.Add(item);
            }

            _miRecent.Enabled = _miRecent.DropDownItems.Count > 0;
        }

        private void OnClickRecent(object sender, EventArgs e)
        {
            if (sender is ToolStripItem item && item.Tag is string file && AskSaveChanges())
            {
                OpenFile(file, false, false);
            }
        }

        private void ShowError(Exception ex)
        {
            SetMessage(ex.Message, true);
            MessageBox.Show(this, ex.Message, Loc.T("tab.title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region Import

        private void OnClickImport(object sender, EventArgs e)
        {
            string file;
            if (sender == _miImportPolFolder)
            {
                using (var dialog = new FolderBrowserDialog { Description = Loc.T("menu.import.polfolder") })
                {
                    if (!string.IsNullOrEmpty(RegionEditorSettings.Current.PolDir))
                    {
                        dialog.SelectedPath = RegionEditorSettings.Current.PolDir;
                    }

                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    file = dialog.SelectedPath;
                }
            }
            else
            {
                string filter = sender == _miImportServUo ? "ServUO / RunUO (*.xml)|*.xml"
                    : sender == _miImportSphere ? "Sphere (*.scp)|*.scp"
                    : sender == _miImportPol ? "POL (*.cfg)|*.cfg"
                    : sender == _miImportCsv ? "CSV (*.csv)|*.csv" : ProjectFilter;
                using (var dialog = new OpenFileDialog { Filter = filter + "|" + Loc.T("filter.any") + "|*.*", Title = ((ToolStripItem)sender).Text })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    file = dialog.FileName;
                }
            }

            bool append = sender == _miImportProject;
            if (!append && _project.AllRegions().Any())
            {
                DialogResult answer = MessageBox.Show(this, Loc.T("ask.importappend"), Loc.T("tab.title"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (answer == DialogResult.Cancel)
                {
                    return;
                }

                append = answer == DialogResult.Yes;
                if (!append && !AskSaveChanges())
                {
                    return;
                }
            }

            OpenFile(file, append, false);
        }

        #endregion

        #region Export

        private ServUoFormat.Options ServUoOptions(MergeMode merge) => new ServUoFormat.Options
        {
            WidthHeight = RegionEditorSettings.Current.WidthHeight,
            SkipHidden = RegionEditorSettings.Current.SkipHidden,
            Merge = merge
        };

        private SphereFormat.Options SphereOptions(int mapId) => new SphereFormat.Options
        {
            MapId = mapId,
            SkipHidden = RegionEditorSettings.Current.SkipHidden,
            DefaultEvents = RegionEditorSettings.Current.SphereEvents
        };

        private PolFormat.Options PolOptions(PolFile file) => new PolFormat.Options
        {
            File = file,
            SkipHidden = RegionEditorSettings.Current.SkipHidden,
            GuardTexts = RegionEditorSettings.Current.PolGuardTexts
        };

        private string AskExportFile(string fileName, string filter)
        {
            string dir = RegionEditorSettings.Current.EffectiveExportDir;
            Directory.CreateDirectory(dir);
            using (var dialog = new SaveFileDialog { Filter = filter, FileName = fileName, InitialDirectory = dir, Title = Loc.T("menu.export") })
            {
                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
            }
        }

        private bool CheckBeforeExport()
        {
            List<RegionIssue> errors = Validator.Check(_project, _mapInfo).Where(i => i.Severity == IssueSeverity.Error).ToList();
            if (errors.Count == 0)
            {
                return true;
            }

            DialogResult answer = MessageBox.Show(this, Loc.F("ask.exporterrors", errors.Count), Loc.T("tab.title"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes)
            {
                ShowIssues();
                return false;
            }

            return true;
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            if (sender != _miExpPng && sender != _miExpHtml && !CheckBeforeExport())
            {
                return;
            }

            try
            {
                string file;
                if (sender == _miExpServUo)
                {
                    file = AskExportFile("Regions.xml", "ServUO / RunUO (*.xml)|*.xml");
                    if (file != null)
                    {
                        string backup = ServUoFormat.Save(_project, file, ServUoOptions(File.Exists(file) ? RegionEditorSettings.Current.Merge : MergeMode.ReplaceFile));
                        Done(file, backup);
                    }
                }
                else if (sender == _miExpSphere)
                {
                    file = AskExportFile($"map{Math.Max(0, _mapView.MapId)}_areas.scp", "Sphere (*.scp)|*.scp");
                    if (file != null)
                    {
                        int map = SphereFormat.MapFromFileName(file);
                        bool onlyMap = Path.GetFileName(file).StartsWith("map", StringComparison.OrdinalIgnoreCase) && _project.Facets.Any(f => f.MapId == map);
                        Done(file, SphereFormat.Save(_project, file, SphereOptions(onlyMap ? map : -1)));
                    }
                }
                else if (sender == _miExpPolRegions || sender == _miExpPolJustice || sender == _miExpPolMusic)
                {
                    PolFile kind = sender == _miExpPolJustice ? PolFile.Justice : sender == _miExpPolMusic ? PolFile.Music : PolFile.Regions;
                    file = AskExportFile(PolFormat.FileName(kind), "POL (*.cfg)|*.cfg");
                    if (file != null)
                    {
                        Done(file, PolFormat.Save(_project, file, PolOptions(kind)));
                    }
                }
                else if (sender == _miExpCsv)
                {
                    file = AskExportFile(DefaultName(".csv"), "CSV (*.csv)|*.csv");
                    if (file != null)
                    {
                        FileHelper.WriteAllText(file, CsvFormat.ToText(_project));
                        Done(file, null);
                    }
                }
                else if (sender == _miExpTxt)
                {
                    file = AskExportFile(DefaultName(".txt"), "Text (*.txt)|*.txt");
                    if (file != null)
                    {
                        FileHelper.WriteAllText(file, TextFormat.ToText(_project));
                        Done(file, null);
                    }
                }
                else if (sender == _miExpHtml)
                {
                    file = AskExportFile(DefaultName(".html"), "HTML (*.html)|*.html");
                    if (file != null)
                    {
                        bool images = MessageBox.Show(this, Loc.T("ask.htmlimages"), Loc.T("tab.title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
                        FileHelper.WriteAllText(file, BuildHtml(images));
                        Done(file, null);
                        OpenWithShell(file);
                    }
                }
                else if (sender == _miExpPng)
                {
                    ExportPng();
                }
                else if (sender == _miExpCSharp)
                {
                    ExportCSharp(null);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void Done(string file, string backup)
        {
            SetMessage(backup == null ? Loc.F("msg.written", file) : Loc.F("msg.writtenbackup", file, Path.GetFileName(backup)));
        }

        private string BuildHtml(bool images)
        {
            Dictionary<int, byte[]> pngs = null;
            if (images)
            {
                Cursor = Cursors.WaitCursor;
                pngs = new Dictionary<int, byte[]>();
                foreach (FacetRegions facet in _project.Facets.Where(f => f.Regions.Count > 0))
                {
                    MapDefinition def = MapRegistry.GetById(facet.MapId);
                    if (def == null || !def.FilesExist())
                    {
                        continue;
                    }

                    using (Bitmap bmp = MapImageExport.Render(def, facet, 0.25, RegionEditorSettings.Current.ShowStatics, true, RegionEditorSettings.Current.FillAlpha))
                    {
                        pngs[facet.MapId] = MapImageExport.ToPng(bmp);
                    }
                }
            }

            return HtmlFormat.ToHtml(_project, Loc.T("html.title"), pngs, id => _mapInfo.GetSize(id));
        }

        private void ExportPng()
        {
            MapDefinition def = _mapView.MapDefinition;
            if (def == null)
            {
                return;
            }

            string file = AskExportFile($"{def.Name}_regions.png", "PNG (*.png)|*.png");
            if (file == null)
            {
                return;
            }

            // full size up to 4096 pixels, larger maps are scaled down
            double scale = Math.Min(1, 4096.0 / Math.Max(def.Width, def.Height));
            Cursor = Cursors.WaitCursor;
            using (Bitmap bmp = MapImageExport.Render(def, CurrentFacet(false), scale, _mapView.ShowStatics, _mapView.ShowLabels, _mapView.FillAlpha))
            {
                bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            }

            Done(file, null);
        }

        /// <param name="folder">null = ask</param>
        private List<string> ExportCSharp(string folder)
        {
            List<string> types = CSharpFormat.CustomTypes(_project);
            if (types.Count == 0)
            {
                if (folder == null)
                {
                    MessageBox.Show(this, Loc.T("msg.nocustomtypes"), Loc.T("tab.title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return new List<string>();
            }

            if (folder == null)
            {
                using (var dialog = new FolderBrowserDialog { Description = Loc.T("menu.export.csharp"), SelectedPath = RegionEditorSettings.Current.EffectiveExportDir })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return new List<string>();
                    }

                    folder = dialog.SelectedPath;
                }
            }

            List<string> files = CSharpFormat.WriteClasses(_project, folder);
            SetMessage(Loc.F("msg.csharp", files.Count, folder));
            return files;
        }

        /// <summary>all formats into the export folder (servuo\Data\Regions.xml, sphere\mapN\mapN_areas.scp, pol\*.cfg …)</summary>
        private void OnClickExportAll(object sender, EventArgs e)
        {
            if (!CheckBeforeExport())
            {
                return;
            }

            string dir = RegionEditorSettings.Current.EffectiveExportDir;
            try
            {
                Cursor = Cursors.WaitCursor;
                Directory.CreateDirectory(dir);
                var written = new List<string>();

                string servuo = Path.Combine(dir, "servuo", "Data", "Regions.xml");
                ServUoFormat.Save(_project, servuo, ServUoOptions(MergeMode.ReplaceFile));
                written.Add(servuo);

                foreach (FacetRegions facet in _project.Facets.Where(f => f.Regions.Count > 0))
                {
                    string sphere = Path.Combine(dir, "sphere", $"map{facet.MapId}", $"map{facet.MapId}_areas.scp");
                    SphereFormat.Save(_project, sphere, SphereOptions(facet.MapId));
                    written.Add(sphere);
                }

                foreach (PolFile kind in new[] { PolFile.Regions, PolFile.Justice, PolFile.Music })
                {
                    string pol = Path.Combine(dir, "pol", "regions", PolFormat.FileName(kind));
                    PolFormat.Save(_project, pol, PolOptions(kind));
                    written.Add(pol);
                }

                string csv = Path.Combine(dir, "regions.csv");
                FileHelper.WriteAllText(csv, CsvFormat.ToText(_project));
                written.Add(csv);
                string txt = Path.Combine(dir, "regions.txt");
                FileHelper.WriteAllText(txt, TextFormat.ToText(_project));
                written.Add(txt);
                string html = Path.Combine(dir, "regions.html");
                FileHelper.WriteAllText(html, BuildHtml(false));
                written.Add(html);
                string project = Path.Combine(dir, "regions" + ProjectFormat.Extension);
                ProjectFormat.Save(_project, project);
                written.Add(project);
                written.AddRange(ExportCSharp(Path.Combine(dir, "servuo", "Scripts", "Regions")));

                // old backups of the export folder are not needed
                foreach (string bak in Directory.GetFiles(dir, "*.bak", SearchOption.AllDirectories))
                {
                    File.Delete(bak);
                }

                SetMessage(Loc.F("msg.exportall", written.Count, dir));
                if (MessageBox.Show(this, Loc.F("ask.openexport", written.Count, dir), Loc.T("tab.title"), MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    OpenWithShell(dir);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void OnClickOpenExportDir(object sender, EventArgs e)
        {
            string dir = RegionEditorSettings.Current.EffectiveExportDir;
            Directory.CreateDirectory(dir);
            OpenWithShell(dir);
        }

        private static void OpenWithShell(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region Server folders

        /// <summary>path of the server file, null (and settings are offered) when not configured</summary>
        private string ServerPath(object sender, bool forWrite)
        {
            RegionEditorSettings s = RegionEditorSettings.Current;
            string path = sender == _miLoadServUo || sender == _miSaveServUo ? s.ServUoRegionsFile
                : sender == _miLoadSphere || sender == _miSaveSphere ? NullIfEmpty(s.SphereFile)
                : NullIfEmpty(s.PolDir);

            bool exists = path != null && (File.Exists(path) || Directory.Exists(path));
            if (path == null || (!forWrite && !exists))
            {
                if (MessageBox.Show(this, path == null ? Loc.T("ask.noserver") : Loc.F("ask.servermissing", path), Loc.T("tab.title"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    ShowSettings();
                }

                return null;
            }

            return path;
        }

        private void OnClickLoadServer(object sender, EventArgs e)
        {
            string path = ServerPath(sender, false);
            if (path == null || !AskSaveChanges())
            {
                return;
            }

            OpenFile(path, false, false);
        }

        private void OnClickSaveServer(object sender, EventArgs e)
        {
            string path = ServerPath(sender, true);
            if (path == null || !CheckBeforeExport())
            {
                return;
            }

            try
            {
                string backup;
                if (sender == _miSaveServUo)
                {
                    MergeMode merge = RegionEditorSettings.Current.Merge;
                    if (MessageBox.Show(this, Loc.F("ask.saveservuo", path, Loc.T("merge." + merge)), Loc.T("tab.title"),
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                    {
                        return;
                    }

                    backup = ServUoFormat.Save(_project, path, ServUoOptions(merge));
                }
                else if (sender == _miSaveSphere)
                {
                    int map = SphereFormat.MapFromFileName(path);
                    bool onlyMap = Path.GetFileName(path).StartsWith("map", StringComparison.OrdinalIgnoreCase);
                    if (MessageBox.Show(this, Loc.F("ask.saveserver", path), Loc.T("tab.title"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                    {
                        return;
                    }

                    backup = SphereFormat.Save(_project, path, SphereOptions(onlyMap ? map : -1));
                }
                else
                {
                    // regions.cfg always, justice.cfg / music.cfg when they are used by the server
                    if (MessageBox.Show(this, Loc.F("ask.saveserver", path), Loc.T("tab.title"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                    {
                        return;
                    }

                    backup = PolFormat.Save(_project, Path.Combine(path, "regions.cfg"), PolOptions(PolFile.Regions));
                    foreach (PolFile kind in new[] { PolFile.Justice, PolFile.Music })
                    {
                        string file = Path.Combine(path, PolFormat.FileName(kind));
                        if (File.Exists(file))
                        {
                            PolFormat.Save(_project, file, PolOptions(kind));
                        }
                    }
                }

                Done(path, backup);
                MessageBox.Show(this, Loc.T("msg.restart"), Loc.T("tab.title"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void OnClickSettings(object sender, EventArgs e) => ShowSettings();

        private void ShowSettings()
        {
            using (var form = new RegionSettingsForm())
            {
                form.ShowDialog(this);
            }
        }

        private void OnClickHelp(object sender, EventArgs e) => RegionHelpForm.ShowHelp(FindForm());

        #endregion
    }
}
