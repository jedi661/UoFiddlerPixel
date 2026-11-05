// /***************************************************************************
//  *
//  * Dragon Map Studio – tab "CGI-Ansicht": client view of the built map, static / multi editing
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private MapData _cgiMap;
        private bool _cgiFromEditor;
        private readonly Dictionary<ToolStripButton, IsoTool> _cgiTools = new Dictionary<ToolStripButton, IsoTool>();
        private List<int> _allItems;
        private List<int> _shownItems = new List<int>();
        private List<MapProblem> _problems = new List<MapProblem>();

        /// <summary>Editable view of the selected statics (property grid on the right).</summary>
        private sealed class CgiSelectionInfo
        {
            [DisplayName("Item-ID (hex)"), Description("Ändert die ID aller ausgewählten Statics.")]
            public string ItemId { get; set; }

            [DisplayName("Hue"), Description("Farbe (0 = original). Gilt für alle ausgewählten Statics.")]
            public int Hue { get; set; }

            [DisplayName("Z"), Description("Höhe. Bei mehreren: alle bekommen diese Höhe.")]
            public int Z { get; set; }

            [DisplayName("X"), Description("Nur bei einem Static änderbar.")]
            public int X { get; set; }

            [DisplayName("Y"), Description("Nur bei einem Static änderbar.")]
            public int Y { get; set; }
        }

        /// <summary>Land tile -> Dragon colour (fallback colours of the CGI view without client files).</summary>
        private void UpdateRuleLandColors()
        {
            var colors = new Dictionary<int, Color>();
            foreach (DragonPaletteEntry e in _rules.Palette.Where(p => p.Defined).OrderBy(p => p.Altitude))
            {
                foreach (ushort id in e.GetTileIds())
                {
                    colors.TryAdd(id, e.Color);
                }
            }
            foreach (TransitionRule r in _rules.Transitions.Where(t => t.Layer == TransitionLayer.Map && t.GroupA >= 0))
            {
                DragonPaletteEntry entry = _rules.EntriesOfGroup(r.GroupA).FirstOrDefault();
                TransitionFile f = _rules.FindTransitionFile(r.File);
                if (entry == null || f == null)
                {
                    continue;
                }
                foreach (TransitionPattern p in f.Patterns)
                {
                    foreach (ushort id in p.GetTileIds())
                    {
                        colors.TryAdd(id, entry.Color);
                    }
                }
            }
            UoArtCache.RuleLandColors = colors;
        }

        private void InitCgi()
        {
            UpdateRuleLandColors();
            _cgiTools[tsbCgiSelect] = IsoTool.Select;
            _cgiTools[tsbCgiPlace] = IsoTool.Place;
            _cgiTools[tsbCgiMulti] = IsoTool.Multi;
            _cgiTools[tsbCgiDelete] = IsoTool.Delete;
            _cgiTools[tsbCgiHue] = IsoTool.Hue;
            _cgiTools[tsbCgiLandSet] = IsoTool.LandSet;
            _cgiTools[tsbCgiRaise] = IsoTool.Raise;
            _cgiTools[tsbCgiLower] = IsoTool.Lower;
            _cgiTools[tsbCgiSetHeight] = IsoTool.SetHeight;
            _cgiTools[tsbCgiSmooth] = IsoTool.Smooth;
            _cgiTools[tsbCgiArea] = IsoTool.Area;
            isoView.LandPicked += IsoView_LandPicked;
            SelectCgiTool(IsoTool.Select);
            foreach (string z in new[] { "127", "100", "80", "60", "40", "30", "20", "10", "0" })
            {
                tscCgiMaxZ.Items.Add(z);
            }
            tscCgiMaxZ.Text = "127";
            tsbCgiStatics.Checked = true;
            tsbCgiProblems.Checked = true;
            toolTip.SetToolTip(isoView,
                "Mausrad = Zoom, mittlere Maus / Leertaste = verschieben, Rechtsklick = Item aufnehmen\n" +
                "Pfeiltasten = Auswahl verschieben (ohne Auswahl: Ansicht), Strg+Pfeil = Kopie daneben einfügen\n" +
                "Bild↑/Bild↓ = Z ±1 (Umschalt ±5), Entf = löschen, H = färben, Strg+C / Strg+V, Strg+Z / Strg+Y");
            CgiPlaceSettings_Changed(this, EventArgs.Empty);
            UpdateCgiState();
        }

        private void SelectCgiTool(IsoTool tool)
        {
            isoView.Tool = tool;
            foreach (KeyValuePair<ToolStripButton, IsoTool> kv in _cgiTools)
            {
                kv.Key.Checked = kv.Value == tool;
            }
            if (tool == IsoTool.Multi)
            {
                tabCgiSide.SelectedTab = tabCgiMulti;
            }
            else if (tool == IsoTool.Place || tool == IsoTool.Hue)
            {
                tabCgiSide.SelectedTab = tabCgiItems;
            }
            else if (tool == IsoTool.LandSet || tool == IsoTool.Raise || tool == IsoTool.Lower || tool == IsoTool.SetHeight || tool == IsoTool.Smooth)
            {
                tabCgiSide.SelectedTab = tabCgiLand;
                EnsureLandList();
            }
            isoView.Invalidate();
        }

        private void CgiTool_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripButton b && _cgiTools.TryGetValue(b, out IsoTool tool))
            {
                SelectCgiTool(tool);
                isoView.Focus();
            }
        }

        private void UpdateCgiState()
        {
            bool has = _cgiMap != null;
            tsbCgiSave.Enabled = tsbCgiSaveAs.Enabled = has;
            tsbCgiUndo.Enabled = has && _cgiMap.CanUndo;
            tsbCgiRedo.Enabled = has && _cgiMap.CanRedo;
            btnCgiScan.Enabled = has;
            tabCgi.Text = has && (_cgiMap.StaticsModified || _cgiMap.LandModified) ? "CGI-Ansicht *" : "CGI-Ansicht";
        }

        private void SetCgiMap(MapData data, double? centerX = null, double? centerY = null)
        {
            _cgiMap = data;
            _problems.Clear();
            lstCgiProblems.DataSource = null;
            isoView.SetProblems(null);
            isoView.SetMap(data);
            numCgiX.Maximum = Math.Max(0, data.Width - 1);
            numCgiY.Maximum = Math.Max(0, data.Height - 1);
            if (centerX.HasValue && centerY.HasValue)
            {
                isoView.CenterOn(centerX.Value, centerY.Value);
            }
            UpdateCgiThumbnail();
            UpdateCgiState();
            tabMain.SelectedTab = tabCgi;
            isoView.Focus();
        }

        /// <summary>Overview: the editor colours when the map was built from the editor, otherwise radar colours.</summary>
        private void UpdateCgiThumbnail()
        {
            if (_cgiMap == null)
            {
                return;
            }
            Bitmap thumb;
            if (_cgiFromEditor && _map != null && _map.Width == _cgiMap.Width && _map.Height == _cgiMap.Height)
            {
                thumb = IndexedBitmapIO.ToBitmap(_map, _rules.GetColors(), 320);
            }
            else
            {
                int step = Math.Max(1, (Math.Max(_cgiMap.Width, _cgiMap.Height) + 319) / 320);
                int w = Math.Max(1, _cgiMap.Width / step), h = Math.Max(1, _cgiMap.Height / step);
                thumb = new Bitmap(w, h);
                var colors = new Dictionary<ushort, Color>();
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        ushort id = _cgiMap.LandId(x * step, y * step);
                        if (!colors.TryGetValue(id, out Color c))
                        {
                            colors[id] = c = UoArtCache.LandColor(id);
                        }
                        thumb.SetPixel(x, y, c);
                    }
                }
            }
            cgiNavigator.SetThumbnail(thumb, _cgiMap.Width, _cgiMap.Height);
            cgiNavigator.SetView(isoView.ViewRect);
            UpdateCgiMark();
        }

        private void UpdateCgiMark()
        {
            Rectangle sel = mapCanvas.Selection;
            cgiNavigator.SetMark(sel.IsEmpty ? RectangleF.Empty : new RectangleF(sel.X, sel.Y, sel.Width, sel.Height));
        }

        private bool ConfirmDiscardCgi()
        {
            if (_cgiMap == null || !_cgiMap.StaticsModified)
            {
                return true;
            }
            DialogResult r = MessageBox.Show(this, "Die Statics in der CGI-Ansicht wurden geändert. Speichern?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
            {
                return false;
            }
            return r == DialogResult.No || SaveCgi(null);
        }

        // ================================================================== build / load / save

        private void TsbCgiBuild_Click(object sender, EventArgs e) => BuildCgiFromEditor(null, null);

        private void BuildCgiFromEditor(double? cx, double? cy)
        {
            if (_map == null)
            {
                MessageBox.Show(this, "Im Karten-Editor ist keine Karte geöffnet.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ConfirmDiscardCgi())
            {
                return;
            }
            BuildOptions o = CurrentBuildOptions();
            DragonRuleSet rules = _rules;
            IndexedMap copy = _map.Clone();
            RunBusy("CGI-Karte bauen", (progress, token) => new DragonCompiler(rules, o, Log).BuildInMemory(copy, progress, token), data =>
            {
                _cgiFromEditor = true;
                SetCgiMap(data, cx ?? data.Width / 2.0, cy ?? data.Height / 2.0);
                SetStatus($"CGI-Karte gebaut: {data.Width}×{data.Height}, {data.StaticCount:N0} Statics. 'Speichern' schreibt nach {o.OutputFolder}.");
                if (!UoArtCache.ArtAvailable)
                {
                    SetStatus("Hinweis: Keine Client-Grafiken geladen – UoFiddler-Pfad zum Client prüfen. Es werden Radarfarben gezeigt.");
                }
            });
        }

        private void TsbCgiLoad_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardCgi())
            {
                return;
            }
            using var dlg = new OpenFileDialog { Filter = "Karten|map*.mul;map*LegacyMUL.uop|Alle Dateien|*.*", Title = "Karte für die CGI-Ansicht laden", InitialDirectory = CurrentBuildOptions().OutputFolder };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            string file = dlg.FileName;
            MapSizePreset size = MulMapIO.GuessSize(file);
            int w = size?.Width ?? (int)numM2BWidth.Value / 8 * 8;
            int h = size?.Height ?? (int)numM2BHeight.Value / 8 * 8;
            if (size == null && file.EndsWith(".uop", StringComparison.OrdinalIgnoreCase) == false)
            {
                SetStatus($"Größe nicht erkannt – benutze {w}×{h} (Reiter MUL/UOP → BMP).");
            }
            int num = MulMapIO.GuessMapNumber(file);
            RunBusy("Karte laden", (progress, token) =>
            {
                MapData data = MapData.LoadFiles(file, w, h, num, out string info);
                return (data, info);
            }, r =>
            {
                _cgiFromEditor = false;
                SetCgiMap(r.data);
                SetStatus($"Geladen: {file} ({w}×{h}), {r.info}");
            });
        }

        private void TsbCgiSave_Click(object sender, EventArgs e) => SaveCgi(null);

        private void TsbCgiSaveAs_Click(object sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog { Description = "Ordner für map / staidx / statics", SelectedPath = _cgiMap?.Folder ?? CurrentBuildOptions().OutputFolder };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                SaveCgi(dlg.SelectedPath);
            }
        }

        /// <summary>
        /// Saves the statics. A map that was built in memory is saved completely (map + statics, UOP when set in the build options).
        /// </summary>
        private bool SaveCgi(string folder)
        {
            if (_cgiMap == null)
            {
                return false;
            }
            BuildOptions o = CurrentBuildOptions();
            bool builtInMemory = _cgiMap.Folder == null;
            folder ??= _cgiMap.Folder ?? o.OutputFolder;
            bool writeMap = builtInMemory || _cgiMap.LandModified || !File.Exists(Path.Combine(folder, $"map{_cgiMap.MapNumber}.mul"));
            try
            {
                if (builtInMemory)
                {
                    _cgiMap.MapNumber = o.MapNumber;
                }
                List<string> files = _cgiMap.Save(folder, writeMap, o.WriteUop);
                files.ForEach(f => Log("CGI gespeichert: " + f));
                SetStatus($"Gespeichert in {folder}: {string.Join(", ", files.Select(Path.GetFileName))}");
                UpdateCgiState();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Speichern fehlgeschlagen: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ================================================================== editor link / navigation

        private void TsbCgiHere_Click(object sender, EventArgs e)
        {
            RectangleF v = mapCanvas.ViewRect;
            double cx = v.X + v.Width / 2, cy = v.Y + v.Height / 2;
            if (_cgiMap == null || _map == null || _cgiMap.Width != _map.Width || _cgiMap.Height != _map.Height || (_cgiFromEditor && _map.Modified && !_cgiMap.StaticsModified))
            {
                BuildCgiFromEditor(cx, cy);
                return;
            }
            tabMain.SelectedTab = tabCgi;
            isoView.CenterOn(cx, cy);
            isoView.Focus();
        }

        private void BtnCgiGoto_Click(object sender, EventArgs e)
        {
            isoView.CenterOn((double)numCgiX.Value, (double)numCgiY.Value);
            isoView.Focus();
        }

        private void BtnCgiToSelection_Click(object sender, EventArgs e)
        {
            Rectangle sel = mapCanvas.Selection;
            if (sel.IsEmpty)
            {
                SetStatus("Im Karten-Editor ist nichts markiert (Werkzeug 'Auswahl').");
                return;
            }
            isoView.CenterOn(sel.X + sel.Width / 2.0, sel.Y + sel.Height / 2.0);
            UpdateCgiMark();
            isoView.Focus();
        }

        private void CgiNavigator_NavigateTo(object sender, MapPointEventArgs e)
        {
            isoView.CenterOn(e.X, e.Y);
        }

        private void IsoView_ViewChanged(object sender, EventArgs e)
        {
            cgiNavigator.SetView(isoView.ViewRect);
            Point c = isoView.CameraTile;
            lblZoom.Text = $"Zoom {isoView.Zoom:0.##}×";
            if (!numCgiX.Focused && !numCgiY.Focused)
            {
                numCgiX.Value = Math.Clamp(c.X, 0, (int)numCgiX.Maximum);
                numCgiY.Value = Math.Clamp(c.Y, 0, (int)numCgiY.Maximum);
            }
        }

        private void IsoView_HoverChanged(object sender, MapPointEventArgs e)
        {
            if (_cgiMap == null || !_cgiMap.InBounds(e.X, e.Y))
            {
                lblPosition.Text = "-";
                return;
            }
            ushort land = _cgiMap.LandId(e.X, e.Y);
            List<StaticItem> statics = _cgiMap.StaticsAt(e.X, e.Y).ToList();
            string st = statics.Count == 0 ? string.Empty : "  |  " + string.Join(", ", statics.Take(4).Select(s => $"{s.Id:x4}@{s.Z}")) + (statics.Count > 4 ? " …" : string.Empty);
            lblPosition.Text = $"X {e.X}  Y {e.Y}  Land {land:x4} {UoArtCache.LandName(land)} z {_cgiMap.LandZ(e.X, e.Y)}{st}";
        }

        private void IsoView_Message(object sender, string e) => SetStatus(e);

        private void IsoView_MapChanged(object sender, EventArgs e) => UpdateCgiState();

        private void TsbCgiUndo_Click(object sender, EventArgs e) => isoView.Undo();

        private void TsbCgiRedo_Click(object sender, EventArgs e) => isoView.Redo();

        private void TsbCgiZoomIn_Click(object sender, EventArgs e) => isoView.ZoomStep(1);

        private void TsbCgiZoomOut_Click(object sender, EventArgs e) => isoView.ZoomStep(-1);

        private void TsbCgiView_Click(object sender, EventArgs e)
        {
            isoView.ShowGrid = tsbCgiGrid.Checked;
            isoView.ShowStatics = tsbCgiStatics.Checked;
            isoView.ShowProblems = tsbCgiProblems.Checked;
            isoView.ShowHeights = tsbCgiHeights.Checked;
            isoView.Refresh(true);
        }

        private void TscCgiMaxZ_Changed(object sender, EventArgs e)
        {
            if (int.TryParse(tscCgiMaxZ.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int z))
            {
                isoView.MaxZ = Math.Clamp(z, -128, 127);
                isoView.Refresh(true);
            }
        }

        // ================================================================== land / textures

        private List<int> _allLand;
        private List<int> _shownLand = new List<int>();

        private void EnsureLandList()
        {
            if (_allLand != null)
            {
                return;
            }
            bool art = UoArtCache.ArtAvailableCached;
            _allLand = new List<int>();
            for (int id = 0; id <= UoArtCache.MaxLandId; id++)
            {
                if (!art || UoArtCache.IsValidLand(id))
                {
                    _allLand.Add(id);
                }
            }
            ApplyLandFilter();
            CgiLandSettings_Changed(this, EventArgs.Empty);
        }

        private void ApplyLandFilter()
        {
            string f = txtCgiLandFilter.Text.Trim();
            string hexText = f.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? f.Substring(2) : f;
            bool isHex = HexList.TryParseHex(hexText, out _);
            _shownLand = string.IsNullOrEmpty(f)
                ? _allLand ?? new List<int>()
                : (_allLand ?? new List<int>()).Where(id => (isHex && id.ToString("x4").Contains(hexText, StringComparison.OrdinalIgnoreCase)) ||
                                                            UoArtCache.LandName(id).Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();
            lstCgiLand.VirtualListSize = _shownLand.Count;
            lstCgiLand.Invalidate();
        }

        private void TxtCgiLandFilter_TextChanged(object sender, EventArgs e) => ApplyLandFilter();

        private void LstCgiLand_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int id = e.ItemIndex < _shownLand.Count ? _shownLand[e.ItemIndex] : 0;
            e.Item = new ListViewItem(id.ToString("x4"));
            e.Item.SubItems.Add(UoArtCache.LandName(id));
        }

        private void LstCgiLand_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstCgiLand.SelectedIndices.Count > 0 && lstCgiLand.SelectedIndices[0] < _shownLand.Count)
            {
                txtCgiLandId.Text = _shownLand[lstCgiLand.SelectedIndices[0]].ToString("x4");
                if (isoView.Tool != IsoTool.LandSet)
                {
                    SelectCgiTool(IsoTool.LandSet);
                }
            }
        }

        private void CgiLandSettings_Changed(object sender, EventArgs e)
        {
            if (HexList.TryParseHex(txtCgiLandId.Text, out ushort id))
            {
                isoView.PlaceLandId = (ushort)(id & 0x3FFF);
            }
            isoView.LandRadius = (int)numCgiRadius.Value;
            isoView.HeightStep = (int)numCgiStep.Value;
            isoView.TargetHeight = (int)numCgiTarget.Value;
            picCgiLandArt.Image = UoArtCache.Land(isoView.PlaceLandId);
            picCgiLandTex.Image = UoArtCache.Texture(isoView.PlaceLandId);
            string wet = UoArtCache.LandIsWetFlat(isoView.PlaceLandId) ? " · nass, ohne Textur (wird flach gezeichnet)" : string.Empty;
            lblCgiLandInfo.Text = $"{isoView.PlaceLandId:x4} {UoArtCache.LandName(isoView.PlaceLandId)}{wet}{Environment.NewLine}" +
                                  "Links: Land-Grafik, rechts: Textur für Hänge. Ziehen mit gedrückter Maus malt, ein Zug = ein Rückgängig-Schritt. Rechtsklick nimmt Land-ID und Höhe auf.";
            isoView.Invalidate();
        }

        private void IsoView_LandPicked(object sender, MapPointEventArgs e)
        {
            txtCgiLandId.Text = e.Index.ToString("x4");
            numCgiTarget.Value = Math.Clamp(_cgiMap.LandZ(e.X, e.Y), -128, 127);
            SetStatus($"Land aufgenommen: {e.Index:x4} {UoArtCache.LandName(e.Index)}, Höhe {_cgiMap.LandZ(e.X, e.Y)}");
        }

        // ================================================================== items / hue / multis

        private void EnsureItemList()
        {
            if (_allItems != null)
            {
                return;
            }
            _allItems = new List<int>();
            bool art = UoArtCache.ArtAvailableCached;
            int max = art ? Math.Min(UoArtCache.MaxItemId, 0x1FFFF) : 0x3FFF;
            for (int id = 0; id <= max; id++)
            {
                if (!art || UoArtCache.IsValidStatic(id))
                {
                    _allItems.Add(id);
                }
            }
            ApplyItemFilter();
        }

        private void ApplyItemFilter()
        {
            string f = txtCgiFilter.Text.Trim();
            if (string.IsNullOrEmpty(f))
            {
                _shownItems = _allItems ?? new List<int>();
            }
            else
            {
                string hexText = f.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? f.Substring(2) : f;
                bool isHex = HexList.TryParseHex(hexText, out _);
                _shownItems = (_allItems ?? new List<int>()).Where(id =>
                    (isHex && id.ToString("x4").Contains(hexText, StringComparison.OrdinalIgnoreCase)) ||
                    UoArtCache.ItemName(id).Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            lstCgiItems.VirtualListSize = _shownItems.Count;
            lstCgiItems.Invalidate();
        }

        private void TxtCgiFilter_TextChanged(object sender, EventArgs e) => ApplyItemFilter();

        private void LstCgiItems_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int id = e.ItemIndex < _shownItems.Count ? _shownItems[e.ItemIndex] : 0;
            e.Item = new ListViewItem(id.ToString("x4"));
            e.Item.SubItems.Add(UoArtCache.ItemName(id));
        }

        private void LstCgiItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstCgiItems.SelectedIndices.Count == 0)
            {
                return;
            }
            int idx = lstCgiItems.SelectedIndices[0];
            if (idx < _shownItems.Count)
            {
                txtCgiItemId.Text = _shownItems[idx].ToString("x4");
                if (isoView.Tool != IsoTool.Place && isoView.Tool != IsoTool.Hue)
                {
                    SelectCgiTool(IsoTool.Place);
                }
            }
        }

        private void CgiPlaceSettings_Changed(object sender, EventArgs e)
        {
            if (HexList.TryParseHex(txtCgiItemId.Text, out ushort id))
            {
                isoView.PlaceId = id;
            }
            isoView.PlaceHue = (ushort)numCgiHue.Value;
            isoView.ZOffset = (int)numCgiZOffset.Value;
            isoView.StackOnStatics = chkCgiStack.Checked;
            isoView.MultiInvisibleParts = chkCgiMultiInvisible.Checked;
            Color hueColor = UoArtCache.HueColor(isoView.PlaceHue);
            panelCgiHueSwatch.BackColor = hueColor == Color.Transparent ? SystemColors.Control : hueColor;
            picCgiItem.Image = UoArtCache.Static(isoView.PlaceId, isoView.PlaceHue);
            lblCgiItemInfo.Text = $"{isoView.PlaceId:x4} {UoArtCache.ItemName(isoView.PlaceId)}  ·  Höhe {UoArtCache.ItemHeight(isoView.PlaceId)}{Environment.NewLine}Hue {isoView.PlaceHue}: {UoArtCache.HueName(isoView.PlaceHue)}";
            isoView.Invalidate();
        }

        private void NumCgiMultiId_ValueChanged(object sender, EventArgs e)
        {
            isoView.MultiId = (int)numCgiMultiId.Value;
            picCgiMulti.Image?.Dispose();
            picCgiMulti.Image = UoArtCache.MultiImage(isoView.MultiId);
            lblCgiMultiInfo.Text = isoView.MultiPartCount > 0
                ? $"Multi {isoView.MultiId:x4}: {isoView.MultiPartCount} Teile. Werkzeug 'Multi setzen' und in die Karte klicken."
                : $"Multi {isoView.MultiId:x4}: nicht vorhanden (oder Client-Dateien nicht geladen).";
            if (isoView.Tool != IsoTool.Multi && isoView.MultiPartCount > 0 && tabCgiSide.SelectedTab == tabCgiMulti)
            {
                SelectCgiTool(IsoTool.Multi);
            }
        }

        /// <summary>Right click on a static: take over id and hue.</summary>
        private void IsoView_ItemPicked(object sender, StaticPickEventArgs e)
        {
            txtCgiItemId.Text = e.Item.Id.ToString("x4");
            numCgiHue.Value = Math.Clamp(e.Item.Hue, 0, (int)numCgiHue.Maximum);
            SetStatus($"Aufgenommen: {e.Item.Id:x4} {UoArtCache.ItemName(e.Item.Id)}, Hue {e.Item.Hue}");
        }

        // ================================================================== selection panel

        private void IsoView_SelectionChanged(object sender, EventArgs e)
        {
            IReadOnlyCollection<StaticItem> sel = isoView.Selection;
            if (sel.Count == 0)
            {
                lblCgiSelInfo.Text = "Nichts ausgewählt." + Environment.NewLine +
                                     "Klick = auswählen, Strg+Klick = dazu, Umschalt+Klick = alles auf dem Tile.";
                propCgiSelection.SelectedObject = null;
                UpdateCgiState();
                return;
            }
            StaticItem f = sel.First();
            lblCgiSelInfo.Text = sel.Count == 1
                ? $"{f.Id:x4} {UoArtCache.ItemName(f.Id)}{Environment.NewLine}({f.X}, {f.Y}, {f.Z})  Hue {f.Hue}{Environment.NewLine}" +
                  "Pfeile = verschieben, Strg+Pfeil = Kopie daneben, Bild↑/↓ = Z, Entf = löschen"
                : $"{sel.Count} Statics ausgewählt.{Environment.NewLine}Pfeile = verschieben, Strg+Pfeil = Kopie daneben, Bild↑/↓ = Z, Entf = löschen";
            propCgiSelection.SelectedObject = new CgiSelectionInfo { ItemId = f.Id.ToString("x4"), Hue = f.Hue, Z = f.Z, X = f.X, Y = f.Y };
            UpdateCgiState();
        }

        private void PropCgiSelection_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (propCgiSelection.SelectedObject is not CgiSelectionInfo info)
            {
                return;
            }
            switch (e.ChangedItem?.PropertyDescriptor?.Name)
            {
                case nameof(CgiSelectionInfo.ItemId):
                    if (HexList.TryParseHex(info.ItemId, out ushort id))
                    {
                        isoView.ModifySelection(t => t.Id = id, $"ID → {id:x4}");
                    }
                    break;
                case nameof(CgiSelectionInfo.Hue):
                    isoView.ModifySelection(t => t.Hue = (ushort)Math.Clamp(info.Hue, 0, 0xFFFF), $"Hue → {info.Hue}");
                    break;
                case nameof(CgiSelectionInfo.Z):
                    isoView.ModifySelection(t => t.Z = Math.Clamp(info.Z, -128, 127), $"Z → {info.Z}");
                    break;
                case nameof(CgiSelectionInfo.X):
                case nameof(CgiSelectionInfo.Y):
                    if (isoView.Selection.Count == 1 && _cgiMap.InBounds(info.X, info.Y))
                    {
                        isoView.ModifySelection(t =>
                        {
                            t.X = info.X;
                            t.Y = info.Y;
                        }, $"Position → ({info.X}, {info.Y})");
                    }
                    else
                    {
                        IsoView_SelectionChanged(this, EventArgs.Empty);
                    }
                    break;
            }
            isoView.Focus();
        }

        // ================================================================== problems

        private void BtnCgiScan_Click(object sender, EventArgs e)
        {
            if (_cgiMap == null)
            {
                return;
            }
            MapData data = _cgiMap;
            var o = new ProblemScanOptions { CliffHeight = (int)numCgiCliff.Value };
            RunBusy("Karte prüfen", (progress, token) => MapProblemScanner.Scan(data, o, progress, token), list =>
            {
                _problems = list;
                _fillingProblems = true;
                lstCgiProblems.DataSource = null;
                lstCgiProblems.DataSource = list;
                lstCgiProblems.ClearSelected();
                _fillingProblems = false;
                isoView.SetProblems(list);
                tabCgiSide.SelectedTab = tabCgiProblems;
                string summary = string.Join(", ", list.GroupBy(p => p.Kind).Select(g => $"{KindName(g.Key)}: {g.Count()}"));
                SetStatus(list.Count == 0 ? "Keine Probleme gefunden." : $"{list.Count} Probleme{(list.Count >= o.MaxResults ? " (Liste gekürzt)" : string.Empty)} – {summary}");
            });
        }

        private static string KindName(MapProblemKind k)
        {
            switch (k)
            {
                case MapProblemKind.MissingLandArt: return "Land ohne Grafik";
                case MapProblemKind.MissingStaticArt: return "Static ohne Grafik";
                case MapProblemKind.Cliff: return "Steilkanten";
                case MapProblemKind.StaticBelowGround: return "im Boden";
                default: return "doppelt";
            }
        }

        private bool _fillingProblems;

        private void LstCgiProblems_DoubleClick(object sender, EventArgs e)
        {
            if (!_fillingProblems && lstCgiProblems.SelectedItem is MapProblem p)
            {
                isoView.CenterOn(p.X, p.Y);
            }
        }
    }
}
