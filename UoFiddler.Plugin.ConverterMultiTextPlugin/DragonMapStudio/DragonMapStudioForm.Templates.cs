// /***************************************************************************
//  *
//  * Dragon Map Studio – CGI view: templates (buildings, floor patterns), cave and road brushes
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private TemplateLibrary _templates;
        private bool _templatesLoading;
        private List<MapTemplate> _shownTemplates = new List<MapTemplate>();
        private List<(string name, ushort[] ids)> _landPresets = new List<(string, ushort[])>();

        private void InitTemplatesAndCaves()
        {
            _cgiTools[tsbCgiCave] = IsoTool.Cave;
            _cgiTools[tsbCgiTemplate] = IsoTool.Template;
            FillLandPresets();
            CgiCaveSettings_Changed(this, EventArgs.Empty);
            CgiTemplateSettings_Changed(this, EventArgs.Empty);
            lblCgiTplInfo.Text = L.T("Ordner:") + " " + _settings.TemplateFolder;
        }

        // ================================================================== land presets (colour table groups)

        /// <summary>Every group of the colour table (grass, cobblestones, cave floor, marble …) with its land tiles.</summary>
        private void FillLandPresets()
        {
            _landPresets = new List<(string, ushort[])>();
            foreach (DragonGroup g in _rules.Groups.OrderBy(g => g.Id))
            {
                ushort[] ids = _rules.EntriesOfGroup(g.Id).SelectMany(e => e.GetTileIds()).Distinct().OrderBy(i => i).ToArray();
                if (ids.Length > 0)
                {
                    _landPresets.Add(($"{g.HexId} {g.Name}  ({ids.Length} Tiles)", ids));
                }
            }
            cmbCgiLandPreset.Items.Clear();
            cmbCgiLandPreset.Items.Add(L.T("Aus der Farbtabelle: Boden, Weg, Höhle …"));
            cmbCgiLandPreset.Items.AddRange(_landPresets.Select(p => (object)p.name).ToArray());
            cmbCgiLandPreset.SelectedIndex = 0;
        }

        private void CmbCgiLandPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = cmbCgiLandPreset.SelectedIndex - 1;
            if (i < 0 || i >= _landPresets.Count)
            {
                return;
            }
            txtCgiLandId.Text = string.Join(" ", _landPresets[i].ids.Select(id => id.ToString("x4")));
            if (isoView.Tool != IsoTool.LandSet && isoView.Tool != IsoTool.Area)
            {
                SelectCgiTool(IsoTool.LandSet);
            }
            SetStatus($"Land-Pinsel: {_landPresets[i].name} – die Tiles werden zufällig gemischt. Für Wege 'Höhe vom ersten Klick übernehmen' einschalten.");
        }

        // ================================================================== cave

        private void CgiCaveSettings_Changed(object sender, EventArgs e)
        {
            ushort[] floor = HexList.Parse(txtCgiCaveFloor.Text);
            if (floor.Length > 0)
            {
                isoView.CaveFloorIds = floor;
            }
            isoView.CaveWallHeight = (int)numCgiCaveWall.Value;
            isoView.CaveRaiseWalls = chkCgiCaveWalls.Checked;
            isoView.CaveUseTargetHeight = chkCgiCaveTarget.Checked;
            lblCgiCaveInfo.Text =
                L.T("Höhle: Werkzeug 'Höhle' (Leiste Land), am Fuß des Berges anfangen und hineinziehen. Der Boden bekommt die Höhe des ersten Klicks " +
                "(oder 'Höhe =' aus dem Reiter Land), der Rand wird bis zur Wand-Höhe angehoben – der Client zeichnet dort die Fels-Textur als Wand. " +
                "Pinsel-Größe oben = Breite des Ganges. Ein Zug = ein Rückgängig-Schritt.") + Environment.NewLine + Environment.NewLine +
                L.T("Wege und Gebäude-Böden: Reiter Land → 'Aus der Farbtabelle' (z. B. cobblestones, marble floor, Wood Floorboards) und " +
                "'Höhe vom ersten Klick übernehmen'. Ganze Flächen: Werkzeug 'Bereich', dann F.") + Environment.NewLine + Environment.NewLine +
                L.T("Fertige Böden / Höhlen-Stücke lassen sich mit 'Bereich → Vorlage…' (Reiter Vorlagen) speichern und wieder setzen.");
        }

        private void BtnCgiCaveTool_Click(object sender, EventArgs e)
        {
            SelectCgiTool(IsoTool.Cave);
            isoView.Focus();
        }

        private void BtnCgiCaveBlack_Click(object sender, EventArgs e)
        {
            isoView.FillArea(new ushort[] { 0x0244 }, "Schwarz 0244");
            isoView.Focus();
        }

        // ================================================================== template library

        private void TabCgiSide_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabCgiSide.SelectedTab == tabCgiTemplates)
            {
                EnsureTemplateLibrary();
            }
        }

        private void EnsureTemplateLibrary()
        {
            if (_templates == null && !_templatesLoading)
            {
                LoadTemplateLibrary(null);
            }
        }

        private void LoadTemplateLibrary(string select)
        {
            string root = _settings.TemplateFolder;
            _templatesLoading = true;
            RunBusy("Vorlagen laden", (progress, token) =>
            {
                var lib = new TemplateLibrary();
                lib.Load(root, progress);
                return lib;
            }, lib =>
            {
                _templatesLoading = false;
                _templates = lib;
                string current = cmbCgiTplCategory.SelectedIndex > 0 ? cmbCgiTplCategory.Text : null;
                cmbCgiTplCategory.Items.Clear();
                cmbCgiTplCategory.Items.Add(L.T("Alle Kategorien"));
                cmbCgiTplCategory.Items.AddRange(lib.Categories.Cast<object>().ToArray());
                if (current != null && cmbCgiTplCategory.Items.Contains(current))
                {
                    cmbCgiTplCategory.SelectedItem = current;
                }
                else
                {
                    cmbCgiTplCategory.SelectedIndex = 0;
                }
                ApplyTemplateFilter(select);
                lib.Errors.ForEach(err => Log("Vorlage nicht lesbar: " + err));
                SetStatus($"{lib.Items.Count:N0} Vorlagen in {lib.Categories.Count()} Kategorien geladen ({root})" + (lib.Errors.Count > 0 ? $", {lib.Errors.Count} nicht lesbar (siehe Log)" : string.Empty));
            });
        }

        private void CgiTemplateFilter_Changed(object sender, EventArgs e) => ApplyTemplateFilter(null);

        private void ApplyTemplateFilter(string selectFile)
        {
            if (_templates == null)
            {
                return;
            }
            string category = cmbCgiTplCategory.SelectedIndex > 0 ? cmbCgiTplCategory.Text : null;
            _shownTemplates = _templates.Find(category, txtCgiTplFilter.Text).OrderBy(t => t.Category, StringComparer.OrdinalIgnoreCase).ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
            lstCgiTemplates.BeginUpdate();
            lstCgiTemplates.Items.Clear();
            foreach (MapTemplate t in _shownTemplates)
            {
                string kind = t.HasLand ? (t.Statics.Count > 0 ? "Boden+Statics" : "Boden") : $"{t.Statics.Count:N0} St.";
                lstCgiTemplates.Items.Add($"{t.Name}   ({kind})" + (category == null ? $"   – {t.Category}" : string.Empty));
            }
            lstCgiTemplates.EndUpdate();
            int sel = selectFile == null ? -1 : _shownTemplates.FindIndex(t => string.Equals(t.File, selectFile, StringComparison.OrdinalIgnoreCase));
            if (sel >= 0)
            {
                lstCgiTemplates.SelectedIndex = sel;
            }
        }

        private MapTemplate SelectedTemplate =>
            lstCgiTemplates.SelectedIndex >= 0 && lstCgiTemplates.SelectedIndex < _shownTemplates.Count ? _shownTemplates[lstCgiTemplates.SelectedIndex] : null;

        private void LstCgiTemplates_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapTemplate t = SelectedTemplate;
            Image old = picCgiTpl.Image;
            picCgiTpl.Image = null;
            old?.Dispose();
            if (t == null)
            {
                return;
            }
            try
            {
                picCgiTpl.Image = TemplatePreview.Render(t, 560, 260);
            }
            catch (Exception)
            {
                picCgiTpl.Image = null;
            }
            string desc = string.IsNullOrWhiteSpace(t.Description) ? string.Empty : $" · {t.Description}";
            string author = string.IsNullOrWhiteSpace(t.Author) ? string.Empty : $" · von {t.Author}";
            lblCgiTplInfo.Text = $"{t.Name}: {t.Summary}{Environment.NewLine}{t.Format}, {t.Category}{desc}{author}";
            isoView.Template = t;
            if (isoView.Tool == IsoTool.Template)
            {
                isoView.Invalidate();
            }
        }

        private void CgiTemplateSettings_Changed(object sender, EventArgs e)
        {
            isoView.TemplateStatics = chkCgiTplStatics.Checked;
            isoView.TemplateLand = chkCgiTplLand.Checked;
            isoView.TemplateLandZ = chkCgiTplLandZ.Checked;
            isoView.TemplateClearStatics = chkCgiTplClear.Checked;
            if (isoView.Tool == IsoTool.Template)
            {
                isoView.ZOffset = (int)numCgiTplZ.Value;
            }
            isoView.Invalidate();
        }

        private void BtnCgiTplPlace_Click(object sender, EventArgs e)
        {
            if (SelectedTemplate == null)
            {
                SetStatus("Erst eine Vorlage in der Liste wählen.");
                return;
            }
            isoView.Template = SelectedTemplate;
            SelectCgiTool(IsoTool.Template);
            isoView.Focus();
        }

        private void BtnCgiTplReload_Click(object sender, EventArgs e) => LoadTemplateLibrary(SelectedTemplate?.File);

        private void BtnCgiTplFolder_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(_settings.TemplateFolder);
            OpenFolder(_settings.TemplateFolder);
        }

        private void BtnCgiTplAreaStatics_Click(object sender, EventArgs e)
        {
            isoView.SelectAreaStatics();
            isoView.Focus();
        }

        private void BtnCgiTplSaveArea_Click(object sender, EventArgs e)
        {
            if (_cgiMap == null || isoView.Area.IsEmpty)
            {
                SetStatus("Erst mit dem Werkzeug 'Bereich' (Leiste Land) einen Bereich aufziehen.");
                return;
            }
            MapTemplate withStatics = isoView.CaptureArea(true, true);
            SaveTemplate(withStatics, true);
        }

        private void BtnCgiTplSaveSel_Click(object sender, EventArgs e)
        {
            MapTemplate t = isoView.CaptureSelection();
            if (t == null)
            {
                SetStatus("Nichts ausgewählt – Statics mit 'Auswahl' (Strg+Klick) oder im Bereich mit S auswählen.");
                return;
            }
            SaveTemplate(t, false);
        }

        /// <summary>Asks name / category / format and saves the template into the template folder.</summary>
        private void SaveTemplate(MapTemplate t, bool landAvailable)
        {
            Bitmap preview = null;
            try
            {
                preview = TemplatePreview.Render(t, 440, 372);
            }
            catch (Exception)
            {
                preview = null;
            }
            IEnumerable<string> categories = _templates?.Categories ?? Enumerable.Empty<string>();
            using var dlg = new SaveTemplateDialog(categories, landAvailable, t.Statics.Count, preview);
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                preview?.Dispose();
                return;
            }
            preview?.Dispose();
            if (!dlg.IncludeLand)
            {
                t.LandTiles = null;
                t.LandZ = null;
                t.LandWidth = t.LandHeight = 0;
            }
            if (!dlg.IncludeStatics)
            {
                t.Statics.Clear();
            }
            t.Name = dlg.TemplateName;
            t.Category = dlg.Category;
            t.Author = Environment.UserName;
            try
            {
                string folder = string.IsNullOrEmpty(dlg.Category) ? _settings.TemplateFolder : Path.Combine(_settings.TemplateFolder, dlg.Category.Replace('/', Path.DirectorySeparatorChar));
                string file = TemplateIO.Save(t, folder, dlg.Format);
                Log("Vorlage gespeichert: " + file);
                SetStatus($"Vorlage gespeichert: {file}");
                if (_templates != null)
                {
                    _templates.Items.RemoveAll(x => string.Equals(x.File, file, StringComparison.OrdinalIgnoreCase));
                    foreach (MapTemplate loaded in TemplateIO.Load(file))
                    {
                        loaded.Category = dlg.Category;
                        _templates.Items.Add(loaded);
                    }
                    if (!cmbCgiTplCategory.Items.Contains(dlg.Category) && dlg.Category.Length > 0)
                    {
                        cmbCgiTplCategory.Items.Add(dlg.Category);
                    }
                    txtCgiTplFilter.Text = string.Empty;
                    cmbCgiTplCategory.SelectedIndex = 0;
                    ApplyTemplateFilter(file);
                }
                else
                {
                    LoadTemplateLibrary(file);
                }
                tabCgiSide.SelectedTab = tabCgiTemplates;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Speichern fehlgeschlagen: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
