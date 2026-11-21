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
            _cgiTools[tsbCgiCaveEntrance] = IsoTool.CaveEntrance;
            _cgiTools[tsbCgiMountain] = IsoTool.Mountain;
            _cgiTools[tsbCgiStamp] = IsoTool.CaveStamp;
            _settings.CaveStamp ??= new CaveStampSettings();
            isoView.CaveStamp = _settings.CaveStamp;
            propCgiStamp.SelectedObject = _settings.CaveStamp;
            tscCgiLabels.SelectedIndex = 0;
            lblCgiStampInfo.Text = L.T("Höhlen-Stempel: auf den Boden direkt vor einer Bergwand klicken, die nach Süden oder Osten zeigt. " +
                "Boden und Berg werden am Tile erkannt (Gras darf höher liegen): die Höhe vorn wird der Höhlenboden, " +
                "die vordere Reihe wird unsichtbar (0002), dahinter der Bogen 01D3–01DA bis zur Mund-Höhe, links und rechts der Rahmen, davor ein Weg. " +
                "Ziehen statt klicken: von einer Seite des Mundes zur anderen = Breite, das Ende oben an der Bergwand = Höhe. " +
                "Auswerten: einen Eingang (z. B. aus der Original-Karte) mit 'Bereich' markieren und den Knopf drücken – das Log zeigt ID, Höhe, Textur, Farbe und Muster jedes Tiles. " +
                "Der Client zeichnet Land nur am Hang mit Textur, flach mit dem Land-Bild – die Ansicht macht es jetzt genauso.");
            _settings.Mountain ??= new MountainSettings();
            isoView.Mountain = _settings.Mountain;
            propCgiMountain.SelectedObject = _settings.Mountain;
            lblCgiMountainInfo.Text = L.T("Berg: Werkzeug 'Berg' (Leiste Land) – am Fuß klicken und ziehen, der Berg steigt ab der Bodenhöhe (oder 'Fuß-Z') " +
                "mit 'Steilheit' Z pro Tile bis 'Gipfel-Z'. Mehrmals ziehen baut einen Grat. Ganzer Berg: mit 'Bereich' markieren, dann M oder der Knopf oben. " +
                "Danach mit 'Eingang' / 'Höhle' einen Höhleneingang anlegen. Strg+Z nimmt alles zurück.");
            gridCgiCaveTiles.ImageOf = CaveTilePicture;
            gridCgiCaveTiles.NameOf = CaveTileName;
            txtCgiCaveOwn.Text = _settings.CaveTileIds ?? string.Empty;
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
            isoView.CaveEntranceMouthHeight = (int)numCgiCaveMouth.Value;
            isoView.CaveEntranceFrontNoDraw = chkCgiCaveFront.Checked;
            ApplyCaveEntranceRules();
            lblCgiCaveInfo.Text =
                L.T("Höhleneingang: Werkzeug 'Eingang' – am Fuß des Berges (Höhe des Bodens davor) klicken und 2–3 Tiles in den Hang ziehen. " +
                "Der Mund wird 01DB (schwarz) bzw. 0002 (unsichtbar), der Rand zum Berg bekommt die Eingangs-Tiles 01D3–01DA wie Dragon " +
                "(map/cave-enti2mountain.txt). Dahinter mit 'Höhle' weitergraben oder auf dem Server einen Teleporter in den Dungeon setzen.") + Environment.NewLine + Environment.NewLine +
                L.T("Höhle: Werkzeug 'Höhle' (Leiste Land), am Fuß des Berges anfangen und hineinziehen. Der Boden bekommt die Höhe des ersten Klicks " +
                "(oder 'Höhe =' aus dem Reiter Land), der Rand wird bis zur Wand-Höhe angehoben – der Client zeichnet dort die Fels-Textur als Wand. " +
                "Pinsel-Größe oben = Breite des Ganges. Ein Zug = ein Rückgängig-Schritt.") + Environment.NewLine + Environment.NewLine +
                L.T("Wege und Gebäude-Böden: Reiter Land → 'Aus der Farbtabelle' (z. B. cobblestones, marble floor, Wood Floorboards) und " +
                "'Höhe vom ersten Klick übernehmen'. Ganze Flächen: Werkzeug 'Bereich', dann F.") + Environment.NewLine + Environment.NewLine +
                L.T("Fertige Böden / Höhlen-Stücke lassen sich mit 'Bereich → Vorlage…' (Reiter Vorlagen) speichern und wieder setzen.");
        }

        /// <summary>Entrance brush: mouth tile, the patterns of map/cave-enti2mountain.txt and the mountain tiles of the colour table.</summary>
        private void ApplyCaveEntranceRules()
        {
            isoView.CaveEntranceFillId = chkCgiCaveInvisible.Checked ? (ushort)0x0002 : (ushort)0x01DB;
            if (_rules == null)
            {
                return;
            }
            TransitionFile file = _rules.FindTransitionFile("map/cave-enti2mountain.txt") ?? _rules.FindTransitionFile("map/cave-ent2mountain.txt");
            var patterns = new Dictionary<byte, TransitionPattern>();
            if (file != null)
            {
                foreach (TransitionPattern p in file.Patterns)
                {
                    patterns.TryAdd(p.Mask, p);
                }
            }
            isoView.CaveEntrancePatterns = patterns;
            int mountain = _settings?.Generator?.MountainGroup ?? 0x06;
            isoView.CaveEntranceMountainIds = new HashSet<ushort>(_rules.EntriesOfGroup(mountain).SelectMany(e => e.GetTileIds()));
            isoView.CaveIsMountain = MountainClassifier(isoView.CaveEntranceMountainIds);
            if (isoView.CaveEntranceMountainIds.Count > 0)
            {
                isoView.MountainFallbackTiles = isoView.CaveEntranceMountainIds.OrderBy(i => i).ToArray();
            }
        }

        private void BtnCgiCaveEntrance_Click(object sender, EventArgs e)
        {
            ApplyCaveEntranceRules();
            SelectCgiTool(IsoTool.CaveEntrance);
            isoView.Focus();
        }

        // ================================================================== cave tiles (gallery) and entrance patterns

        private const string CaveEntranceCategory = "Höhleneingang";

        /// <summary>Tiles a cave is built from in the original: entrance 01D3–01DB, invisible 0002, black 0244, cave floor 0245–0249.</summary>
        private static readonly int[] KnownCaveTiles =
        {
            0x0002, 0x01D3, 0x01D4, 0x01D5, 0x01D6, 0x01D7, 0x01D8, 0x01D9, 0x01DA, 0x01DB, 0x01DD, 0x01DE,
            0x0244, 0x0245, 0x0246, 0x0247, 0x0248, 0x0249
        };

        private List<int> _caveTiles;

        /// <summary>Gallery: known cave tiles + every land tile whose tiledata name contains "cave" / "höhle" + own ids.</summary>
        private void EnsureCaveTiles(bool force = false)
        {
            if (_caveTiles != null && !force)
            {
                return;
            }
            var ids = new List<int>();
            void Add(int id)
            {
                if (id >= 0 && id <= UoArtCache.MaxLandId && !ids.Contains(id))
                {
                    ids.Add(id);
                }
            }
            foreach (ushort id in HexList.Parse(txtCgiCaveOwn.Text))
            {
                Add(id);
            }
            foreach (int id in KnownCaveTiles)
            {
                Add(id);
            }
            bool art = UoArtCache.ArtAvailableCached;
            for (int id = 0; id <= UoArtCache.MaxLandId; id++)
            {
                string name = UoArtCache.LandName(id);
                if ((name.Contains("cave", StringComparison.OrdinalIgnoreCase) || name.Contains("höhle", StringComparison.OrdinalIgnoreCase)) &&
                    (!art || UoArtCache.IsValidLand(id)))
                {
                    Add(id);
                }
            }
            _caveTiles = ids;
            gridCgiCaveTiles.Ids = ids;
            lblCgiCaveTiles.Text = L.T("Höhlen-Tiles") + $" ({ids.Count}): " + L.T("TileData 'cave' + eigene · Doppelklick = Land-Pinsel");
        }

        private Image CaveTilePicture(int id)
        {
            return chkCgiCaveTextures.Checked ? UoArtCache.Texture(id) ?? (Image)UoArtCache.Land(id) : UoArtCache.Land(id);
        }

        private static string CaveTileName(int id)
        {
            string role = id switch
            {
                0x0002 => "unsichtbarer Eingang",
                0x01DB => "Eingang schwarz (Mund)",
                0x01DA => "Eingang, Berg im Westen",
                0x01D6 => "Eingang, Berg im Osten",
                0x01D8 => "Eingang, Berg im Süden",
                0x01D4 => "Eingang, Berg im Norden",
                0x01D3 or 0x01D5 or 0x01D7 or 0x01D9 => "Eingang, Ecke",
                0x01DD or 0x01DE => "Höhlenboden → Erdweg",
                0x0244 => "Schwarz (Dungeon-Füllung)",
                >= 0x0245 and <= 0x0249 => "Höhlenboden",
                _ => string.Empty
            };
            string name = UoArtCache.LandName(id);
            string tex = UoArtCache.Texture(id) != null ? "  (mit Textur)" : "  (keine Textur)";
            return (role.Length > 0 ? role + " – " : string.Empty) + name + tex;
        }

        private void ChkCgiCaveTextures_CheckedChanged(object sender, EventArgs e) => gridCgiCaveTiles.Invalidate();

        private void TxtCgiCaveOwn_Leave(object sender, EventArgs e)
        {
            string text = string.Join(" ", HexList.Parse(txtCgiCaveOwn.Text).Select(id => id.ToString("x4")));
            txtCgiCaveOwn.Text = text;
            if (_settings.CaveTileIds != text)
            {
                _settings.CaveTileIds = text;
                _settings.Save();
            }
            EnsureCaveTiles(true);
        }

        private void BtnCgiCaveAddOwn_Click(object sender, EventArgs e)
        {
            int id = gridCgiCaveTiles.SelectedId >= 0 ? gridCgiCaveTiles.SelectedId : isoView.PlaceLandId;
            List<ushort> own = HexList.Parse(txtCgiCaveOwn.Text).ToList();
            if (!own.Contains((ushort)id))
            {
                own.Add((ushort)id);
            }
            txtCgiCaveOwn.Text = string.Join(" ", own.Select(i => i.ToString("x4")));
            TxtCgiCaveOwn_Leave(sender, e);
            SetStatus($"{id:x4} als eigenes Höhlen-Tile gespeichert (Einstellungen).");
        }

        private void GridCgiCaveTiles_ItemActivated(object sender, EventArgs e)
        {
            int id = gridCgiCaveTiles.SelectedId;
            if (id < 0)
            {
                return;
            }
            txtCgiLandId.Text = id.ToString("x4");
            SelectCgiTool(IsoTool.LandSet);
            isoView.Focus();
        }

        private void BtnCgiCaveAsFloor_Click(object sender, EventArgs e)
        {
            int id = gridCgiCaveTiles.SelectedId;
            if (id < 0)
            {
                SetStatus("Erst ein Tile in der Galerie wählen.");
                return;
            }
            List<ushort> floor = HexList.Parse(txtCgiCaveFloor.Text).ToList();
            if (!floor.Contains((ushort)id))
            {
                floor.Add((ushort)id);
            }
            txtCgiCaveFloor.Text = string.Join(" ", floor.Select(i => i.ToString("x4")));
        }

        private void BtnCgiCaveAsMouth_Click(object sender, EventArgs e)
        {
            int id = gridCgiCaveTiles.SelectedId;
            if (id < 0)
            {
                SetStatus("Erst ein Tile in der Galerie wählen.");
                return;
            }
            chkCgiCaveInvisible.Checked = false;
            isoView.CaveEntranceFillId = (ushort)id;
            SetStatus($"Eingang-Pinsel: Mund = {id:x4} (bis zur nächsten Änderung im Reiter).");
            SelectCgiTool(IsoTool.CaveEntrance);
            isoView.Focus();
        }

        private List<MapTemplate> _cavePatterns = new List<MapTemplate>();

        /// <summary>Templates of the category "Höhleneingang" (sub folder of the template folder).</summary>
        private void FillCavePatterns(string selectFile)
        {
            if (_templates == null)
            {
                return;
            }
            _cavePatterns = _templates.Find(CaveEntranceCategory, string.Empty).OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
            lstCgiCavePatterns.BeginUpdate();
            lstCgiCavePatterns.Items.Clear();
            foreach (MapTemplate t in _cavePatterns)
            {
                lstCgiCavePatterns.Items.Add($"{t.Name}   ({t.LandWidth}×{t.LandHeight}" + (t.Statics.Count > 0 ? $", {t.Statics.Count} St." : string.Empty) + ")");
            }
            lstCgiCavePatterns.EndUpdate();
            int sel = selectFile == null ? -1 : _cavePatterns.FindIndex(t => string.Equals(t.File, selectFile, StringComparison.OrdinalIgnoreCase));
            if (sel >= 0)
            {
                lstCgiCavePatterns.SelectedIndex = sel;
            }
        }

        private void BtnCgiCaveSavePattern_Click(object sender, EventArgs e)
        {
            if (_cgiMap == null || isoView.Area.IsEmpty)
            {
                SetStatus("Erst mit dem Werkzeug 'Bereich' einen fertigen Eingang (z. B. aus der Original-Karte) mit etwas Boden davor markieren.");
                return;
            }
            EnsureTemplateLibrary();
            MapTemplate t = isoView.CaptureArea(true, true);
            SaveTemplate(t, true, "Eingang", CaveEntranceCategory);
        }

        private void BtnCgiCavePlacePattern_Click(object sender, EventArgs e)
        {
            int i = lstCgiCavePatterns.SelectedIndex;
            if (i < 0 || i >= _cavePatterns.Count)
            {
                SetStatus("Erst ein Eingangs-Muster wählen (oder mit 'Bereich → Muster speichern' eins anlegen).");
                return;
            }
            // land with its heights, relative to the ground where you click
            chkCgiTplLand.Checked = true;
            chkCgiTplLandZ.Checked = true;
            isoView.Template = _cavePatterns[i];
            SelectCgiTool(IsoTool.Template);
            tabCgiSide.SelectedTab = tabCgiCave;
            isoView.Focus();
            SetStatus($"Muster '{_cavePatterns[i].Name}': Klick auf den Boden vor dem Berg setzt es (Land + Höhen relativ zum Klick).");
        }

        // ================================================================== mountain

        private HashSet<ushort> _mountainByName;

        /// <summary>
        /// Mountain / rock tiles: the mountain group of the colour table, the mountain brush tiles and every land tile whose
        /// tiledata name contains "mountain" or "rock". Everything else (grass, dirt, sand …) is ground, whatever its height.
        /// </summary>
        private Func<ushort, bool> MountainClassifier(HashSet<ushort> groupIds)
        {
            if (_mountainByName == null || _mountainByName.Count == 0)
            {
                _mountainByName = new HashSet<ushort>();
                for (int id = 3; id <= UoArtCache.MaxLandId; id++)
                {
                    string name = UoArtCache.LandName(id);
                    if (name.Contains("mountain", StringComparison.OrdinalIgnoreCase) || name.Contains("rock", StringComparison.OrdinalIgnoreCase))
                    {
                        _mountainByName.Add((ushort)id);
                    }
                }
            }
            var all = new HashSet<ushort>(_mountainByName);
            all.UnionWith(groupIds ?? new HashSet<ushort>());
            all.UnionWith(HexList.Parse(_settings?.Mountain?.Tiles ?? string.Empty));
            all.RemoveWhere(id => CaveEntranceBuilder.IsEntranceTile(id, 0x0002));
            return all.Contains;
        }

        private void PropCgiStamp_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            isoView.CaveStamp = _settings.CaveStamp;
        }

        private void BtnCgiStampTool_Click(object sender, EventArgs e)
        {
            ApplyCaveEntranceRules();
            SelectCgiTool(IsoTool.CaveStamp);
            isoView.Focus();
        }

        private void TscCgiLabels_SelectedIndexChanged(object sender, EventArgs e)
        {
            isoView.LabelMode = (IsoLabel)Math.Max(0, tscCgiLabels.SelectedIndex);
            if (!tsbCgiHeights.Checked && tscCgiLabels.Focused)
            {
                tsbCgiHeights.Checked = true;
                isoView.ShowHeights = true;
            }
            isoView.Refresh(true);
        }

        private void BtnCgiAreaLog_Click(object sender, EventArgs e)
        {
            if (_cgiMap == null || isoView.Area.IsEmpty)
            {
                SetStatus("Erst mit dem Werkzeug 'Bereich' (Leiste Land) z. B. einen Höhleneingang samt Rand markieren.");
                return;
            }
            ApplyCaveEntranceRules();
            string text = isoView.AnalyzeArea();
            if (text == null)
            {
                return;
            }
            Rectangle a = isoView.Area;
            string path = WriteLog($"cgi_bereich_{a.X}_{a.Y}_{a.Width}x{a.Height}.txt", text);
            Log("Bereichs-Auswertung: " + path);
            SetStatus("Auswertung gespeichert: " + path + " – die Datei kannst du mir schicken.");
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch (Exception)
            {
                // no editor associated – the path is in the status line
            }
        }

        private void CgiViewTextures_Changed(object sender, EventArgs e)
        {
            isoView.AlwaysTextures = chkCgiAlwaysTextures.Checked;
            isoView.ShowNoDraw = chkCgiShowNoDraw.Checked;
            isoView.Refresh(true);
        }

        private void PropCgiMountain_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            isoView.Mountain = _settings.Mountain;
            isoView.Invalidate();
        }

        private void BtnCgiMountainTool_Click(object sender, EventArgs e)
        {
            SelectCgiTool(IsoTool.Mountain);
            isoView.Focus();
        }

        private void BtnCgiMountainArea_Click(object sender, EventArgs e)
        {
            if (isoView.Area.IsEmpty)
            {
                SetStatus("Erst mit dem Werkzeug 'Bereich' (Leiste Land) die Fläche des Berges aufziehen.");
                return;
            }
            isoView.MountainArea();
            isoView.Focus();
        }

        private void BtnCgiMountainSeed_Click(object sender, EventArgs e)
        {
            _settings.Mountain.Seed = new Random().Next(1, 1000000);
            propCgiMountain.Refresh();
            SetStatus($"Zufalls-Zahl {_settings.Mountain.Seed} – Strg+Z und noch einmal M zeigt den anderen Berg.");
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
            else if (tabCgiSide.SelectedTab == tabCgiCave)
            {
                EnsureCaveTiles();
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
                FillCavePatterns(select);
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
        private void SaveTemplate(MapTemplate t, bool landAvailable, string presetName = null, string presetCategory = null)
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
            dlg.Preset(presetName, presetCategory);
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
                if (presetCategory == CaveEntranceCategory)
                {
                    FillCavePatterns(file);
                }
                else
                {
                    tabCgiSide.SelectedTab = tabCgiTemplates;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Speichern fehlgeschlagen: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
