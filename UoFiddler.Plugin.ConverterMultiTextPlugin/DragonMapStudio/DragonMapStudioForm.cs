// /***************************************************************************
//  *
//  * Dragon Map Studio – main window (colour table, map editor, files)
//  * Generator, rules and build are in DragonMapStudioForm.Tools.cs.
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    /// <summary>
    /// Dragon Map Studio: colour table, map editor (draw / load / undo / redo), generator, outline import,
    /// Dragon rules (all scripts editable) and MUL / UOP build. Opened from the "Map" tab of the plugin.
    /// </summary>
    public partial class DragonMapStudioForm : Form
    {
        private StudioSettings _settings;
        private DragonRuleSet _rules = new DragonRuleSet();
        private bool _rulesDirty;
        private IndexedMap _map;
        private readonly MapHistory _history = new MapHistory();
        private IndexedMap _clipboard;
        private int _selectedColor = -1;
        private bool _thumbDirty;
        private CancellationTokenSource _cts;
        private bool _busy;
        private Action _afterBusy;
        private readonly Dictionary<ToolStripButton, MapTool> _toolButtons = new Dictionary<ToolStripButton, MapTool>();

        public DragonMapStudioForm()
        {
            InitializeComponent();

            _toolButtons[tsbBrush] = MapTool.Brush;
            _toolButtons[tsbEraser] = MapTool.Eraser;
            _toolButtons[tsbSpray] = MapTool.Spray;
            _toolButtons[tsbLine] = MapTool.Line;
            _toolButtons[tsbRect] = MapTool.Rectangle;
            _toolButtons[tsbRectFill] = MapTool.FilledRectangle;
            _toolButtons[tsbEllipse] = MapTool.Ellipse;
            _toolButtons[tsbEllipseFill] = MapTool.FilledEllipse;
            _toolButtons[tsbFill] = MapTool.Fill;
            _toolButtons[tsbPicker] = MapTool.Picker;
            _toolButtons[tsbRaise] = MapTool.Raise;
            _toolButtons[tsbLower] = MapTool.Lower;
            _toolButtons[tsbSelect] = MapTool.Select;
            _toolButtons[tsbPan] = MapTool.Pan;

            _history.Changed += (s, e) => UpdateUndoState();
        }

        // ================================================================== start / end

        private void DragonMapStudioForm_Load(object sender, EventArgs e)
        {
            _settings = StudioSettings.Load();
            LocalizedTypeProvider.RegisterAll();
            _history.MemoryLimit = Math.Max(64, _settings.UndoMemoryMb) * 1024L * 1024;

            cmbCopyFormat.Items.AddRange(new object[]
            {
                "Index hex (0a)", "Index hex (0x0A)", "Index dezimal (10)", "HTML (#RRGGBB)", "RGB (r,g,b)", "maptrans-Zeile", "Alles"
            });
            cmbCopyFormat.SelectedIndex = (int)_settings.CopyFormat;
            numColorTableZoom.Value = Math.Clamp(_settings.ColorTableZoom, 1, 12);

            foreach (int size in new[] { 1, 2, 3, 4, 5, 8, 12, 16, 24, 32, 48, 64, 96, 128, 192, 256 })
            {
                tscBrushSize.Items.Add(size.ToString(CultureInfo.InvariantCulture));
            }
            tscBrushSize.Text = _settings.BrushSize.ToString(CultureInfo.InvariantCulture);
            tsbGrid.Checked = true;
            mapCanvas.GridFromZoom = _settings.GridFromZoom;
            SelectTool(MapTool.Brush);

            cmbM2BSize.Items.Add(L.T("Eigene Größe"));
            foreach (MapSizePreset p in MapSizePreset.All)
            {
                cmbM2BSize.Items.Add(p);
            }
            cmbM2BSize.SelectedIndex = 1;

            InitHelp();
            toolTip.SetToolTip(colorTableView, "Linksklick = Adresse kopieren, Rechtsklick = Farbe 1, Mitte = Farbe 2");
            toolTip.SetToolTip(paletteGrid, "Linksklick = auswählen, Rechtsklick = Farbe 2 im Editor");

            propSettings.SelectedObject = _settings;
            propBuild.SelectedObject = _settings.Build;
            propGenerator.SelectedObject = _settings.Generator;
            propOutline.SelectedObject = _settings.Outline;

            LoadRules();
            LoadColorTable();
            RefreshLibrary();
            UpdateRecentMenu();
            InitCgi();
            ApplyLanguage(_settings.Language, false);

            if (!string.IsNullOrWhiteSpace(_settings.LastMap) && File.Exists(_settings.LastMap))
            {
                OpenMapFile(_settings.LastMap);
            }
            else
            {
                UpdateTitle();
            }
            timerThumbnail.Start();
            UpdateUndoState();
        }

        private void DragonMapStudioForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_busy)
            {
                if (MessageBox.Show(this, "Eine Aufgabe läuft noch. Abbrechen und schließen?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                _cts?.Cancel();
            }
            if (!ConfirmDiscardMap() || !ConfirmDiscardCgi())
            {
                e.Cancel = true;
                return;
            }
            if (_rulesDirty)
            {
                DialogResult r = MessageBox.Show(this, "Die Regeln wurden geändert. Als JSON-Regelsatz speichern?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (r == DialogResult.Yes && !SaveRulesJson(null))
                {
                    e.Cancel = true;
                    return;
                }
            }
            timerThumbnail.Stop();
            _settings.PrimaryIndex = mapCanvas.PrimaryIndex;
            _settings.SecondaryIndex = mapCanvas.SecondaryIndex;
            _settings.BrushSize = mapCanvas.BrushSize;
            _settings.Save();
        }

        private void MnuClose_Click(object sender, EventArgs e) => Close();

        private void UpdateTitle()
        {
            string name = _map == null ? "keine Karte" : (_map.FilePath == null ? "Unbenannt" : Path.GetFileName(_map.FilePath));
            string size = _map == null ? string.Empty : $" ({_map.Width}×{_map.Height}, {_map.Bits} Bit)";
            string mod = _map != null && _map.Modified ? " *" : string.Empty;
            Text = $"Dragon Map Studio – {name}{size}{mod} – Regeln: {_rules.Name} ({_rules.ColorBits} Bit){(_rulesDirty ? " *" : string.Empty)}";
        }

        private void SetStatus(string text) => lblStatus.Text = text;

        private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabMain.SelectedTab == tabEditor)
            {
                mapCanvas.Focus();
            }
            else if (tabMain.SelectedTab == tabCgi)
            {
                EnsureItemList();
                UpdateCgiMark();
                isoView.Focus();
            }
        }

        // ================================================================== rules

        private void LoadRules()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_settings.RuleSetFile) && File.Exists(_settings.RuleSetFile))
                {
                    _rules = DragonRuleSet.LoadJson(_settings.RuleSetFile);
                }
                else
                {
                    string palette = !string.IsNullOrWhiteSpace(_settings.PaletteFile) && File.Exists(_settings.PaletteFile)
                        ? _settings.PaletteFile
                        : _settings.ColorTableImage;
                    _rules = DragonScriptIO.LoadFolder(_settings.ScriptFolder, palette, _settings.MapTransFile);
                }
            }
            catch (Exception ex)
            {
                _rules = new DragonRuleSet();
                _rules.Warnings.Add("Regeln konnten nicht geladen werden: " + ex.Message);
                MessageBox.Show(this, "Regeln konnten nicht geladen werden:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            _rulesDirty = false;
            ApplyRules();
            SetStatus($"Regeln geladen: {_rules.Palette.Count(p => p.Defined)} Farben, {_rules.Groups.Count} Gruppen, {_rules.Transitions.Count} Übergänge, {_rules.ItemRules.Count} Item-Regeln" +
                      (_rules.Warnings.Count > 0 ? $", {_rules.Warnings.Count} Hinweise" : string.Empty));
        }

        /// <summary>Pushes the rule set into all views.</summary>
        private void ApplyRules()
        {
            RefreshPaletteViews();
            _rules.BuildHeightLookups(out ushort[] raise, out ushort[] lower);
            mapCanvas.SetHeightLookups(raise, lower);
            mapCanvas.SetAltitudes(_rules.Palette.Select(p => p.Altitude).ToArray());
            BindRuleGrids();
            if (_cgiTools.Count > 0)
            {
                UpdateRuleLandColors();
                FillLandPresets();
            }
            lblRuleSetInfo.Text = $"{_rules.Name}  –  {_rules.Palette.Count(p => p.Defined)} Farben · {_rules.Groups.Count} Gruppen · {_rules.Transitions.Count} Übergänge · {_rules.TransitionFiles.Sum(f => f.Patterns.Count)} Muster · {_rules.ItemFiles.Sum(f => f.Groups.Count)} Item-Gruppen";
            txtRuleWarnings.Text = _rules.Warnings.Count == 0 ? "Keine Hinweise." : string.Join(Environment.NewLine, _rules.Warnings);
            UpdateTitle();
        }

        /// <summary>Colours and names changed: palette grids, canvas, colour info.</summary>
        private void RefreshPaletteViews()
        {
            Color[] colors = _rules.GetColors();
            bool[] defined = _rules.Palette.Select(p => p.Defined).ToArray();
            string[] names = _rules.Palette.Select(p => p.Name).ToArray();
            paletteGrid.SetPalette(colors, defined, names);
            editorPalette.SetPalette(colors, defined, names);
            mapCanvas.SetPalette(colors);
            UpdateBrushColorPanels();
            UpdatePaletteBitsUi();
            if (_selectedColor >= 0)
            {
                ShowColorInfo(_selectedColor);
            }
            _thumbDirty = true;
        }

        private void MarkRulesDirty()
        {
            _rulesDirty = true;
            UpdateTitle();
        }

        // ================================================================== colour table

        private void LoadColorTable()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_settings.ColorTableImage) && File.Exists(_settings.ColorTableImage))
                {
                    colorTableView.LoadImage(_settings.ColorTableImage, _rules);
                    return;
                }
            }
            catch (Exception ex)
            {
                SetStatus("Farbtabelle nicht lesbar: " + ex.Message);
            }
            RenderColorTableFromRules();
        }

        private void RenderColorTableFromRules()
        {
            Bitmap bmp = ColorTableRenderer.Render(_rules, out int[] indices);
            colorTableView.SetImage(bmp, indices, bmp.Width, bmp.Height);
        }

        private void NumColorTableZoom_ValueChanged(object sender, EventArgs e)
        {
            colorTableView.Zoom = (int)numColorTableZoom.Value;
            if (_settings != null)
            {
                _settings.ColorTableZoom = (int)numColorTableZoom.Value;
            }
        }

        private void CmbCopyFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_settings != null && cmbCopyFormat.SelectedIndex >= 0)
            {
                _settings.CopyFormat = (CopyFormat)cmbCopyFormat.SelectedIndex;
                propSettings.Refresh();
            }
        }

        private void BtnLoadColorTable_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "Bilder|*.bmp;*.png;*.gif|Alle Dateien|*.*", Title = "Farbtabelle laden" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _settings.ColorTableImage = dlg.FileName;
            propSettings.Refresh();
            LoadColorTable();
        }

        private void BtnRenderColorTable_Click(object sender, EventArgs e)
        {
            RenderColorTableFromRules();
            SetStatus("Farbtabelle aus den aktuellen Regeln erzeugt (enthält auch eigene Farben).");
        }

        private void ColorTableView_IndexClicked(object sender, PaletteIndexEventArgs e)
        {
            SelectColor(e.Index);
            switch (e.Button)
            {
                case MouseButtons.Left:
                    CopyColorAddress(e.Index);
                    break;
                case MouseButtons.Right:
                    SetBrushColor(e.Index, true);
                    SetStatus($"Farbe 1 = {_rules.HexIndex(e.Index)} {_rules.Palette[e.Index].Name}");
                    break;
                case MouseButtons.Middle:
                    SetBrushColor(e.Index, false);
                    SetStatus($"Farbe 2 = {_rules.HexIndex(e.Index)} {_rules.Palette[e.Index].Name}");
                    break;
            }
        }

        private void ColorTableView_IndexHovered(object sender, PaletteIndexEventArgs e)
        {
            if (e.Index < 0)
            {
                lblPosition.Text = "-";
                return;
            }
            DragonPaletteEntry p = _rules.Palette[e.Index];
            lblPosition.Text = $"{p.HexIndex} · {p.HtmlColor} · {p.Name}";
        }

        private void PaletteGrid_IndexClicked(object sender, PaletteIndexEventArgs e)
        {
            SelectColor(e.Index);
            if (e.Button == MouseButtons.Right)
            {
                SetBrushColor(e.Index, false);
            }
        }

        private void SelectColor(int index)
        {
            if (index < 0 || index >= _rules.Palette.Count)
            {
                return;
            }
            _selectedColor = index;
            ShowPaletteIndex(index);
            colorTableView.SelectedIndex = index;
            paletteGrid.PrimaryIndex = index;
            propColorEntry.SelectedObject = _rules.Palette[index];
            ShowColorInfo(index);
        }

        private void ShowColorInfo(int index)
        {
            DragonPaletteEntry p = _rules.Palette[index];
            panelColorSwatch.BackColor = p.Color;
            lblColorAddress.Text = $"{_rules.HexIndex(index)}  ({index})";
            string group = p.Group >= 0 ? $"{_rules.HexGroup(p.Group)} {_rules.GroupName(p.Group)}" : "keine";
            string tiles = p.GetTileIds().Length == 0 ? "-" : string.Join(" ", p.GetTileIds().Take(12).Select(t => t.ToString("x4"))) + (p.GetTileIds().Length > 12 ? " …" : string.Empty);
            lblColorDetails.Text =
                $"Name: {p.Name}{Environment.NewLine}" +
                $"Farbe: {p.HtmlColor}  RGB {p.R},{p.G},{p.B}{Environment.NewLine}" +
                $"Gruppe: {group}{Environment.NewLine}" +
                $"Definiert: {(p.Defined ? "ja" : "nein (nicht in maptrans)")}{Environment.NewLine}" +
                $"Höhe (Z): {p.Altitude}{Environment.NewLine}" +
                $"Tiles: {tiles}" +
                (string.IsNullOrEmpty(p.Comment) ? string.Empty : $"{Environment.NewLine}Kommentar: {p.Comment}");
            picTilePreview.Image = LoadTilePreview(p);
        }

        /// <summary>Land tile picture from the client (if UoFiddler has the files loaded) or the own texture.</summary>
        private static Image LoadTilePreview(DragonPaletteEntry p)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(p.TexturePath) && File.Exists(p.TexturePath))
                {
                    using var fs = File.OpenRead(p.TexturePath);
                    return new Bitmap(Image.FromStream(fs));
                }
                ushort[] ids = p.GetTileIds();
                if (ids.Length > 0)
                {
                    return Ultima.Art.GetLand(ids[0]);
                }
            }
            catch (Exception)
            {
                // client files not loaded – no preview
            }
            return null;
        }

        private string FormatColor(int index, CopyFormat format)
        {
            DragonPaletteEntry p = _rules.Palette[index];
            switch (format)
            {
                case CopyFormat.HexIndex0x:
                    return $"0x{index:X2}";
                case CopyFormat.DecimalIndex:
                    return index.ToString(CultureInfo.InvariantCulture);
                case CopyFormat.HtmlColor:
                    return p.HtmlColor;
                case CopyFormat.Rgb:
                    return $"{p.R},{p.G},{p.B}";
                case CopyFormat.MapTransLine:
                    return $"{p.HexIndex} {(p.Altitude >= 0 ? p.Altitude.ToString("000", CultureInfo.InvariantCulture) : p.Altitude.ToString(CultureInfo.InvariantCulture).PadLeft(3))} {string.Join(" ", p.GetTileIds().Select(t => t.ToString("x4")))}";
                case CopyFormat.Full:
                    return $"Index {_rules.HexIndex(index)} ({index}) | {p.HtmlColor} | RGB {p.R},{p.G},{p.B} | Gruppe {(p.Group >= 0 ? _rules.HexGroup(p.Group) : "-")} {_rules.GroupName(p.Group)} | Z {p.Altitude} | Tiles {p.Tiles}";
                default:
                    return p.HexIndex;
            }
        }

        private void CopyColorAddress(int index)
        {
            string text = FormatColor(index, _settings.CopyFormat);
            try
            {
                Clipboard.SetText(text);
                SetStatus($"Kopiert: {text}");
            }
            catch (Exception ex)
            {
                SetStatus("Zwischenablage nicht verfügbar: " + ex.Message);
            }
        }

        private void BtnCopyColor_Click(object sender, EventArgs e)
        {
            if (_selectedColor >= 0)
            {
                CopyColorAddress(_selectedColor);
            }
        }

        private void BtnUseAsPrimary_Click(object sender, EventArgs e)
        {
            if (_selectedColor >= 0)
            {
                SetBrushColor(_selectedColor, true);
            }
        }

        private void BtnUseAsSecondary_Click(object sender, EventArgs e)
        {
            if (_selectedColor >= 0)
            {
                SetBrushColor(_selectedColor, false);
            }
        }

        private void BtnUseAsLand_Click(object sender, EventArgs e)
        {
            if (_selectedColor >= 0)
            {
                _settings.Outline.LandIndex = _selectedColor;
                propOutline.Refresh();
                SetStatus($"Umriss-Land = {_rules.HexIndex(_selectedColor)}");
            }
        }

        private void BtnUseAsWater_Click(object sender, EventArgs e)
        {
            if (_selectedColor >= 0)
            {
                _settings.Outline.WaterIndex = _selectedColor;
                propOutline.Refresh();
                SetStatus($"Umriss-Wasser = {_rules.HexIndex(_selectedColor)}");
            }
        }

        /// <summary>Defines a new colour on the first free palette index (own terrains / textures).</summary>
        private void BtnNewColor_Click(object sender, EventArgs e)
        {
            int free = _rules.FirstFreeIndex();
            if (free < 0 && _rules.ColorBits == 8)
            {
                DialogResult r = MessageBox.Show(this, L.Pick(
                        "Alle 256 Farben sind belegt (8 Bit, klassisches Dragon).\n\nRegelsatz auf 16 Bit umstellen? Dann gibt es bis zu 65 536 Farben.\n" +
                        "Karten mit Farben über ff werden als 16-Bit-PNG gespeichert; das alte Dragon kann diese Skripte nicht lesen.",
                        "All 256 colours are used (8 bit, classic Dragon).\n\nSwitch the rule set to 16 bit? Then up to 65 536 colours are possible.\n" +
                        "Maps with colours above ff are saved as 16-bit PNG; the old Dragon cannot read these scripts.",
                        "Les 256 couleurs sont utilisées (8 bits, Dragon classique).\n\nPasser le jeu de règles en 16 bits ? Jusqu'à 65 536 couleurs seront possibles.\n" +
                        "Les cartes avec des couleurs au-delà de ff sont enregistrées en PNG 16 bits ; l'ancien Dragon ne lit pas ces scripts."),
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes)
                {
                    return;
                }
                ConvertRulesTo16();
                free = _rules.FirstFreeIndex();
            }
            if (free < 0)
            {
                MessageBox.Show(this, L.Pick("Alle Farben sind belegt.", "All colours are used.", "Toutes les couleurs sont utilisées."), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var dlg = new ColorDialog { FullOpen = true, Color = Color.Magenta };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            DragonPaletteEntry template = _selectedColor >= 0 ? _rules.Palette[_selectedColor] : null;
            DragonPaletteEntry entry = _rules.Palette[free];
            entry.SetColor(dlg.Color);
            entry.Defined = true;
            entry.Name = "Neue Farbe";
            entry.Category = "Eigene";
            entry.Group = template?.Group ?? 0;
            entry.Altitude = template?.Altitude ?? 0;
            entry.Tiles = template?.Tiles ?? "0003";
            entry.Comment = "Eigene Farbe";
            MarkRulesDirty();
            ApplyRules();
            SelectColor(free);
            SetStatus($"Neue Farbe auf Index {_rules.HexIndex(free)} angelegt – rechts Name, Gruppe, Höhe, Tiles und Textur einstellen.");
        }

        private void BtnEditRgb_Click(object sender, EventArgs e)
        {
            if (_selectedColor < 0)
            {
                return;
            }
            DragonPaletteEntry entry = _rules.Palette[_selectedColor];
            using var dlg = new ColorDialog { FullOpen = true, Color = entry.Color };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            entry.SetColor(dlg.Color);
            MarkRulesDirty();
            RefreshPaletteViews();
            propColorEntry.Refresh();
        }

        private void PropColorEntry_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            MarkRulesDirty();
            RefreshPaletteViews();
            if (e.ChangedItem?.PropertyDescriptor?.Name == nameof(DragonPaletteEntry.Group) ||
                e.ChangedItem?.PropertyDescriptor?.Name == nameof(DragonPaletteEntry.Altitude) ||
                e.ChangedItem?.PropertyDescriptor?.Name == nameof(DragonPaletteEntry.Defined))
            {
                _rules.BuildHeightLookups(out ushort[] raise, out ushort[] lower);
                mapCanvas.SetHeightLookups(raise, lower);
            }
            gridPaletteEntries.Refresh();
        }

        // ================================================================== brush colours

        private void SetBrushColor(int index, bool primary)
        {
            if (!EnsureMapCanUse(index))
            {
                return;
            }
            if (primary)
            {
                mapCanvas.PrimaryIndex = index;
            }
            else
            {
                mapCanvas.SecondaryIndex = index;
            }
            UpdateBrushColorPanels();
        }

        private void UpdateBrushColorPanels()
        {
            if (_rules == null)
            {
                return;
            }
            int last = _rules.Palette.Count - 1;
            DragonPaletteEntry p1 = _rules.Palette[Math.Clamp(mapCanvas.PrimaryIndex, 0, last)];
            DragonPaletteEntry p2 = _rules.Palette[Math.Clamp(mapCanvas.SecondaryIndex, 0, last)];
            panelPrimarySwatch.BackColor = p1.Color;
            panelSecondarySwatch.BackColor = p2.Color;
            lblPrimary.Text = $"1: {p1.HexIndex}{Environment.NewLine}{p1.Name}";
            lblSecondary.Text = $"2: {p2.HexIndex}{Environment.NewLine}{p2.Name}";
            editorPalette.PrimaryIndex = mapCanvas.PrimaryIndex;
            editorPalette.SecondaryIndex = mapCanvas.SecondaryIndex;
        }

        private void EditorPalette_IndexClicked(object sender, PaletteIndexEventArgs e)
        {
            SetBrushColor(e.Index, e.Button != MouseButtons.Right);
        }

        private void MapCanvas_ColorPicked(object sender, PaletteIndexEventArgs e)
        {
            SetBrushColor(e.Index, e.Button != MouseButtons.Right);
            SelectColor(e.Index);
        }

        // ================================================================== editor tools

        private void SelectTool(MapTool tool)
        {
            mapCanvas.Tool = tool;
            foreach (KeyValuePair<ToolStripButton, MapTool> kv in _toolButtons)
            {
                kv.Key.Checked = kv.Value == tool;
            }
        }

        private void ToolButton_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripButton b && _toolButtons.TryGetValue(b, out MapTool tool))
            {
                SelectTool(tool);
                mapCanvas.Focus();
            }
        }

        private void TscBrushSize_Changed(object sender, EventArgs e)
        {
            if (int.TryParse(tscBrushSize.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int size) && size > 0)
            {
                mapCanvas.BrushSize = Math.Min(size, 1024);
                mapCanvas.Invalidate();
            }
        }

        private void ChangeBrushSize(int direction)
        {
            int[] steps = { 1, 2, 3, 4, 5, 8, 12, 16, 24, 32, 48, 64, 96, 128, 192, 256 };
            int current = mapCanvas.BrushSize;
            int next = direction > 0 ? steps.FirstOrDefault(s => s > current) : steps.LastOrDefault(s => s < current);
            if (next > 0)
            {
                tscBrushSize.Text = next.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void TsbSquare_Click(object sender, EventArgs e)
        {
            mapCanvas.SquareBrush = tsbSquare.Checked;
            mapCanvas.Invalidate();
        }

        private void TsbZoomIn_Click(object sender, EventArgs e) => mapCanvas.ZoomStep(1);

        private void TsbZoomOut_Click(object sender, EventArgs e) => mapCanvas.ZoomStep(-1);

        private void TsbZoomFit_Click(object sender, EventArgs e) => mapCanvas.ZoomToFit();

        private void TsbZoom100_Click(object sender, EventArgs e) => mapCanvas.SetZoom(1);

        private void TsbHeights_Click(object sender, EventArgs e)
        {
            mapCanvas.ShowHeights = tsbHeights.Checked;
            if (tsbHeights.Checked && mapCanvas.Zoom < 12)
            {
                SetStatus("Höhen werden ab Zoom 12 angezeigt (Mausrad).");
            }
            mapCanvas.Invalidate();
        }

        private void TsbGrid_Click(object sender, EventArgs e)
        {
            mapCanvas.ShowGrid = tsbGrid.Checked;
            mapCanvas.Invalidate();
        }

        private void MapCanvas_CursorMoved(object sender, MapPointEventArgs e)
        {
            if (e.Index < 0)
            {
                lblPosition.Text = $"X {e.X}  Y {e.Y}";
                return;
            }
            DragonPaletteEntry p = _rules.Palette[e.Index];
            lblPosition.Text = $"X {e.X}  Y {e.Y}  Block {e.X / 8},{e.Y / 8}  |  {p.HexIndex} {p.Name}";
        }

        private void MapCanvas_ViewChanged(object sender, EventArgs e)
        {
            double z = mapCanvas.Zoom;
            lblZoom.Text = z >= 1 ? $"Zoom {z:0.#}×" : $"Zoom 1:{1 / z:0.#}";
            mapNavigator.SetView(mapCanvas.ViewRect);
        }

        private void MapCanvas_MapEdited(object sender, EventArgs e)
        {
            _thumbDirty = true;
            UpdateTitle();
        }

        private void MapNavigator_NavigateTo(object sender, MapPointEventArgs e) => mapCanvas.CenterOn(e.X, e.Y);

        private void TimerThumbnail_Tick(object sender, EventArgs e)
        {
            if (!_thumbDirty || _map == null || _busy)
            {
                return;
            }
            _thumbDirty = false;
            mapNavigator.SetThumbnail(IndexedBitmapIO.ToBitmap(_map, _rules.GetColors(), 320), _map.Width, _map.Height);
            mapNavigator.SetView(mapCanvas.ViewRect);
        }

        /// <summary>Editor shortcuts (only when the map has the focus, so text fields keep Ctrl+C / Ctrl+Z).</summary>
        private void DragonMapStudioForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (tabMain.SelectedTab != tabEditor || !mapCanvas.Focused)
            {
                return;
            }
            bool handled = true;
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Z: MnuUndo_Click(sender, e); break;
                    case Keys.Y: MnuRedo_Click(sender, e); break;
                    case Keys.C: MnuCopy_Click(sender, e); break;
                    case Keys.X: MnuCut_Click(sender, e); break;
                    case Keys.V: MnuPaste_Click(sender, e); break;
                    case Keys.A: MnuSelectAll_Click(sender, e); break;
                    case Keys.F: MnuFillSelection_Click(sender, e); break;
                    default: handled = false; break;
                }
            }
            else if (!e.Alt)
            {
                switch (e.KeyCode)
                {
                    case Keys.B: SelectTool(MapTool.Brush); break;
                    case Keys.E: SelectTool(MapTool.Eraser); break;
                    case Keys.L: SelectTool(MapTool.Line); break;
                    case Keys.R: SelectTool(MapTool.FilledRectangle); break;
                    case Keys.F: SelectTool(MapTool.Fill); break;
                    case Keys.I: SelectTool(MapTool.Picker); break;
                    case Keys.S: SelectTool(MapTool.Select); break;
                    case Keys.H: SelectTool(MapTool.Pan); break;
                    case Keys.OemOpenBrackets: ChangeBrushSize(-1); break;
                    case Keys.OemCloseBrackets: ChangeBrushSize(1); break;
                    case Keys.Add:
                    case Keys.Oemplus: mapCanvas.ZoomStep(1); break;
                    case Keys.Subtract:
                    case Keys.OemMinus: mapCanvas.ZoomStep(-1); break;
                    case Keys.Delete: MnuDeleteSelection_Click(sender, e); break;
                    default: handled = false; break;
                }
            }
            else
            {
                handled = false;
            }
            if (handled)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        // ================================================================== undo / clipboard

        private void UpdateUndoState()
        {
            mnuUndo.Enabled = tsbUndo.Enabled = _history.CanUndo;
            mnuRedo.Enabled = tsbRedo.Enabled = _history.CanRedo;
            mnuUndo.Text = _history.CanUndo ? $"&Rückgängig: {_history.UndoName}" : "&Rückgängig";
            mnuRedo.Text = _history.CanRedo ? $"&Wiederholen: {_history.RedoName}" : "&Wiederholen";
            tsbUndo.ToolTipText = mnuUndo.Text.Replace("&", string.Empty);
            tsbRedo.ToolTipText = mnuRedo.Text.Replace("&", string.Empty);
        }

        private void MnuUndo_Click(object sender, EventArgs e)
        {
            if (_busy || !_history.CanUndo)
            {
                return;
            }
            _history.Undo();
            AfterExternalEdit();
        }

        private void MnuRedo_Click(object sender, EventArgs e)
        {
            if (_busy || !_history.CanRedo)
            {
                return;
            }
            _history.Redo();
            AfterExternalEdit();
        }

        private void AfterExternalEdit()
        {
            mapCanvas.InvalidateMap();
            _thumbDirty = true;
            UpdateTitle();
            UpdateUndoState();
        }

        private void MnuCopy_Click(object sender, EventArgs e)
        {
            IndexedMap copy = mapCanvas.CopySelection();
            if (copy != null)
            {
                _clipboard = copy;
                SetStatus($"Auswahl kopiert ({copy.Width}×{copy.Height}). Strg+V = als Stempel einfügen.");
            }
        }

        private void MnuCut_Click(object sender, EventArgs e)
        {
            IndexedMap copy = mapCanvas.CopySelection();
            if (copy != null)
            {
                _clipboard = copy;
                mapCanvas.FillSelection((ushort)mapCanvas.SecondaryIndex, "Ausschneiden");
            }
        }

        private void MnuPaste_Click(object sender, EventArgs e)
        {
            if (_clipboard == null)
            {
                SetStatus("Nichts zum Einfügen – erst eine Auswahl kopieren.");
                return;
            }
            tabMain.SelectedTab = tabEditor;
            mapCanvas.BeginPaste(_clipboard);
            mapCanvas.Focus();
            SetStatus("Einfügen: Linksklick setzt (mehrfach möglich), Esc oder Rechtsklick beendet.");
        }

        private void MnuDeleteSelection_Click(object sender, EventArgs e) => mapCanvas.FillSelection((ushort)mapCanvas.SecondaryIndex, "Auswahl löschen");

        private void MnuFillSelection_Click(object sender, EventArgs e) => mapCanvas.FillSelection((ushort)mapCanvas.PrimaryIndex, "Auswahl füllen");

        private void MnuSelectAll_Click(object sender, EventArgs e) => mapCanvas.SelectAll();

        // ================================================================== map files

        private void SetMap(IndexedMap map)
        {
            _map = map;
            _history.Attach(map);
            mapCanvas.SetMap(map, _history);
            mapCanvas.PrimaryIndex = Math.Clamp(_settings.PrimaryIndex, 0, Math.Min(map.MaxIndex, _rules.Palette.Count - 1));
            mapCanvas.SecondaryIndex = Math.Clamp(_settings.SecondaryIndex, 0, Math.Min(map.MaxIndex, _rules.Palette.Count - 1));
            UpdateBrushColorPanels();
            _thumbDirty = true;
            TimerThumbnail_Tick(this, EventArgs.Empty);
            UpdateTitle();
            UpdateUndoState();
        }

        /// <summary>Asks to save a modified map. False = the user cancelled.</summary>
        private bool ConfirmDiscardMap()
        {
            if (_map == null || !_map.Modified)
            {
                return true;
            }
            DialogResult r = MessageBox.Show(this, "Die Karte wurde geändert. Speichern?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
            {
                return false;
            }
            return r == DialogResult.No || SaveMap(false);
        }

        private void OpenMapFile(string path)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var warnings = new List<string>();
                IndexedMap map = IndexedBitmapIO.Load(path, _rules, _settings.RemapForeignPalettes ? ColorMatchMode.NearestColor : ColorMatchMode.KeepIndices, warnings);
                SetMap(map);
                _settings.AddRecent(path);
                UpdateRecentMenu();
                tabMain.SelectedTab = tabEditor;
                SetStatus($"Geöffnet: {path} ({map.Width}×{map.Height})" + (warnings.Count > 0 ? " – " + string.Join(" ", warnings) : string.Empty));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Karte konnte nicht geöffnet werden:\n{path}\n\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private bool SaveMap(bool saveAs)
        {
            if (_map == null)
            {
                return false;
            }
            string path = _map.FilePath;
            string ext = MapExtension(_map);
            if (saveAs || string.IsNullOrEmpty(path) || !path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                Directory.CreateDirectory(_settings.ProjectFolder);
                using var dlg = new SaveFileDialog
                {
                    Filter = IndexedBitmapIO.SaveFilter,
                    InitialDirectory = _settings.ProjectFolder,
                    FileName = (string.IsNullOrEmpty(path) ? "NeueKarte" : Path.GetFileNameWithoutExtension(path)) + ext,
                    FilterIndex = _map.Bits == 16 ? 2 : 1,
                    Title = "Karte speichern"
                };
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }
                path = dlg.FileName;
                // the chosen file type decides the format: .png = 16 bit, .bmp = 8 bit (only when all colours fit)
                if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    _map.Bits = 16;
                }
                else if (_map.Bits == 16)
                {
                    if (_map.HighestUsedIndex() > 0xFF)
                    {
                        MessageBox.Show(this, L.Pick("Die Karte benutzt Farben über ff und kann nur als 16-Bit-PNG gespeichert werden.",
                                "The map uses colours above ff and can only be saved as 16-bit PNG.",
                                "La carte utilise des couleurs au-delà de ff et ne peut être enregistrée qu'en PNG 16 bits."),
                            Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                    _map.Bits = 8;
                }
            }
            try
            {
                IndexedBitmapIO.Save(path, _map, _rules.GetColors());
                _map.FilePath = path;
                _map.Modified = false;
                _settings.AddRecent(path);
                UpdateRecentMenu();
                RefreshLibrary();
                UpdateTitle();
                SetStatus($"Gespeichert: {path}");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Speichern fehlgeschlagen:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void MnuNewMap_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardMap())
            {
                return;
            }
            int fill = _rules.NearestIndex(_settings.Generator.WaterGroup, _settings.Generator.WaterAltitude);
            using var dlg = new NewMapDialog(_rules, _settings.NewMapWidth, _settings.NewMapHeight, fill < 0 ? 0 : fill);
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _settings.NewMapWidth = dlg.MapWidth;
            _settings.NewMapHeight = dlg.MapHeight;
            if (dlg.Bits == 16 && _rules.ColorBits == 8)
            {
                ConvertRulesTo16();
            }
            var map = new IndexedMap(dlg.MapWidth, dlg.MapHeight, dlg.FillIndex, dlg.Bits)
            {
                FilePath = Path.Combine(_settings.ProjectFolder, MakeSafeFileName(dlg.MapName) + (dlg.Bits == 16 ? ".png" : ".bmp")),
                Modified = true
            };
            SetMap(map);
            tabMain.SelectedTab = tabEditor;
            SetStatus($"Neue Karte {map.Width}×{map.Height} – Speichern legt sie im Projekt-Ordner an.");
        }

        private static string MakeSafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        private void MnuOpenMap_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardMap())
            {
                return;
            }
            using var dlg = new OpenFileDialog { Filter = IndexedBitmapIO.OpenFilter, InitialDirectory = _settings.ProjectFolder, Title = "Karte öffnen" };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                OpenMapFile(dlg.FileName);
            }
        }

        private void MnuSaveMap_Click(object sender, EventArgs e) => SaveMap(false);

        private void MnuSaveMapAs_Click(object sender, EventArgs e) => SaveMap(true);

        private void MnuExportPng_Click(object sender, EventArgs e)
        {
            if (_map == null)
            {
                return;
            }
            using var dlg = new SaveFileDialog { Filter = "PNG|*.png", InitialDirectory = _settings.ProjectFolder, FileName = Path.GetFileNameWithoutExtension(_map.FilePath ?? "Karte") + ".png" };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                IndexedBitmapIO.SavePng(dlg.FileName, _map, _rules.GetColors());
                SetStatus("PNG exportiert: " + dlg.FileName);
            }
        }

        private void MnuImportMul_Click(object sender, EventArgs e)
        {
            tabMain.SelectedTab = tabBuild;
            BtnM2BBrowse_Click(sender, e);
        }

        private void UpdateRecentMenu()
        {
            mnuRecent.DropDownItems.Clear();
            foreach (string path in _settings.RecentMaps.Where(File.Exists))
            {
                var item = new ToolStripMenuItem(path);
                item.Click += (s, e) =>
                {
                    if (ConfirmDiscardMap())
                    {
                        OpenMapFile(path);
                    }
                };
                mnuRecent.DropDownItems.Add(item);
            }
            mnuRecent.Enabled = mnuRecent.DropDownItems.Count > 0;
        }

        // ================================================================== map library (project folder)

        private void RefreshLibrary()
        {
            lstMaps.BeginUpdate();
            try
            {
                lstMaps.Items.Clear();
                if (string.IsNullOrWhiteSpace(_settings.ProjectFolder))
                {
                    return;
                }
                Directory.CreateDirectory(_settings.ProjectFolder);
                // 8-bit BMPs and 16-bit PNG maps (other PNGs are previews and are not listed)
                IEnumerable<string> files = Directory.GetFiles(_settings.ProjectFolder, "*.bmp")
                    .Concat(Directory.GetFiles(_settings.ProjectFolder, "*.png").Where(IndexedBitmapIO.IsPng16));
                foreach (string file in files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    var info = new FileInfo(file);
                    string size = ReadBmpSize(file);
                    var item = new ListViewItem(Path.GetFileNameWithoutExtension(file)) { Tag = file };
                    item.SubItems.Add(size);
                    item.SubItems.Add(info.LastWriteTime.ToString("dd.MM.yy HH:mm", CultureInfo.CurrentCulture));
                    lstMaps.Items.Add(item);
                }
                toolTip.SetToolTip(lstMaps, _settings.ProjectFolder);
            }
            catch (Exception ex)
            {
                SetStatus("Projekt-Ordner nicht lesbar: " + ex.Message);
            }
            finally
            {
                lstMaps.EndUpdate();
            }
        }

        private static string ReadBmpSize(string file)
        {
            try
            {
                using var fs = File.OpenRead(file);
                var head = new byte[26];
                if (fs.Read(head, 0, 26) == 26 && head[0] == 'B' && head[1] == 'M')
                {
                    return $"{BitConverter.ToInt32(head, 18)}×{Math.Abs(BitConverter.ToInt32(head, 22))}";
                }
                if (head[1] == 'P' && head[2] == 'N' && head[3] == 'G')
                {
                    int w = (head[16] << 24) | (head[17] << 16) | (head[18] << 8) | head[19];
                    int h = (head[20] << 24) | (head[21] << 16) | (head[22] << 8) | head[23];
                    return $"{w}×{h} 16 Bit";
                }
            }
            catch (Exception)
            {
                // ignore
            }
            return "?";
        }

        private string SelectedLibraryFile => lstMaps.SelectedItems.Count > 0 ? lstMaps.SelectedItems[0].Tag as string : null;

        private void BtnLibOpen_Click(object sender, EventArgs e)
        {
            string file = SelectedLibraryFile;
            if (file != null && ConfirmDiscardMap())
            {
                OpenMapFile(file);
            }
        }

        private void BtnLibDuplicate_Click(object sender, EventArgs e)
        {
            string file = SelectedLibraryFile;
            if (file == null)
            {
                return;
            }
            string dir = Path.GetDirectoryName(file);
            string name = Path.GetFileNameWithoutExtension(file);
            string fileExt = Path.GetExtension(file);
            string target = Path.Combine(dir, name + "_Kopie" + fileExt);
            for (int i = 2; File.Exists(target); i++)
            {
                target = Path.Combine(dir, $"{name}_Kopie{i}{fileExt}");
            }
            File.Copy(file, target);
            RefreshLibrary();
        }

        private void BtnLibRename_Click(object sender, EventArgs e)
        {
            string file = SelectedLibraryFile;
            if (file == null)
            {
                return;
            }
            string name = Microsoft.VisualBasic.Interaction.InputBox("Neuer Name:", "Karte umbenennen", Path.GetFileNameWithoutExtension(file));
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            string target = Path.Combine(Path.GetDirectoryName(file), MakeSafeFileName(name.Trim()) + Path.GetExtension(file));
            if (File.Exists(target))
            {
                MessageBox.Show(this, "Eine Karte mit diesem Namen gibt es schon.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            File.Move(file, target);
            if (_map != null && string.Equals(_map.FilePath, file, StringComparison.OrdinalIgnoreCase))
            {
                _map.FilePath = target;
                UpdateTitle();
            }
            RefreshLibrary();
        }

        private void BtnLibDelete_Click(object sender, EventArgs e)
        {
            string file = SelectedLibraryFile;
            if (file == null)
            {
                return;
            }
            if (MessageBox.Show(this, $"Karte '{Path.GetFileName(file)}' in den Papierkorb verschieben?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Löschen fehlgeschlagen: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (_map != null && string.Equals(_map.FilePath, file, StringComparison.OrdinalIgnoreCase))
            {
                _map.FilePath = null;
                _map.Modified = true;
                UpdateTitle();
            }
            RefreshLibrary();
        }

        private void BtnLibRefresh_Click(object sender, EventArgs e) => RefreshLibrary();

        // ================================================================== background work

        /// <summary>
        /// Runs work in the background with progress bar and cancel link; <paramref name="done"/> runs on the UI thread.
        /// Results and progress are always marshalled with BeginInvoke (independent of the synchronisation context).
        /// </summary>
        private void RunBusy<T>(string text, Func<IProgress<int>, CancellationToken, T> work, Action<T> done)
        {
            if (_busy)
            {
                SetStatus("Bitte warten – es läuft schon eine Aufgabe.");
                return;
            }
            _busy = true;
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            SetStatus(text);
            progressBar.Value = 0;
            progressBar.Visible = true;
            lblCancel.Visible = true;
            mapCanvas.Enabled = false;
            var progress = new UiProgress(this, p => progressBar.Value = Math.Clamp(p, 0, 100));
            var sw = Stopwatch.StartNew();

            Task.Run(() => work(progress, token), token).ContinueWith(task =>
            {
                if (IsDisposed || Disposing)
                {
                    return;
                }
                BeginInvoke(new Action(() => FinishBusy(text, task, done, sw)));
            }, TaskScheduler.Default);
        }

        private void FinishBusy<T>(string text, Task<T> task, Action<T> done, Stopwatch sw)
        {
            try
            {
                if (task.IsCanceled || task.Exception?.GetBaseException() is OperationCanceledException)
                {
                    SetStatus($"{text} abgebrochen.");
                }
                else if (task.IsFaulted)
                {
                    Exception ex = task.Exception.GetBaseException();
                    SetStatus($"{text} fehlgeschlagen: {ex.Message}");
                    Log("FEHLER: " + ex);
                    MessageBox.Show(this, ex.Message, text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    done?.Invoke(task.Result);
                    SetStatus($"{text} fertig ({sw.Elapsed.TotalSeconds:0.0} s)");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"{text} fehlgeschlagen: {ex.Message}");
                Log("FEHLER: " + ex);
            }
            finally
            {
                _busy = false;
                progressBar.Visible = false;
                lblCancel.Visible = false;
                mapCanvas.Enabled = true;
                _cts?.Dispose();
                _cts = null;
                Action next = _afterBusy;
                _afterBusy = null;
                if (next != null)
                {
                    BeginInvoke(next);
                }
            }
        }

        /// <summary>Progress that always reports on the UI thread of the form.</summary>
        private sealed class UiProgress : IProgress<int>
        {
            private readonly Control _owner;
            private readonly Action<int> _report;
            private int _last = -1;

            public UiProgress(Control owner, Action<int> report)
            {
                _owner = owner;
                _report = report;
            }

            public void Report(int value)
            {
                if (value == Interlocked.Exchange(ref _last, value) || _owner.IsDisposed)
                {
                    return;
                }
                try
                {
                    _owner.BeginInvoke(_report, value);
                }
                catch (InvalidOperationException)
                {
                    // window closed
                }
            }
        }

        private void LblCancel_Click(object sender, EventArgs e) => _cts?.Cancel();

        /// <summary>Thread safe log (build tab).</summary>
        private void Log(string line)
        {
            if (IsDisposed)
            {
                return;
            }
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(Log), line);
                return;
            }
            txtBuildLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {line}{Environment.NewLine}");
        }

        private static void OpenFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return;
            }
            Directory.CreateDirectory(folder);
            Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
        }

        private void MnuHelpTopics_Click(object sender, EventArgs e) => tabMain.SelectedTab = tabHelp;

        private void MnuOpenSettingsFolder_Click(object sender, EventArgs e) => OpenFolder(StudioSettings.SettingsFolder);
    }
}
