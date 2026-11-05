// /***************************************************************************
//  *
//  * Dragon Map Studio – generator, outline, rule tables, build, settings
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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private float[] _mask;
        private int _maskW;
        private int _maskH;
        private TransitionFile _shownTransitionFile;
        private ItemFile _shownItemFile;

        // ================================================================== generator

        private (int w, int h) GeneratorWorldSize()
        {
            GeneratorSettings g = _settings.Generator;
            if (g.UseCurrentMapSize && _map != null)
            {
                return (_map.Width, _map.Height);
            }
            return ((Math.Max(8, g.Width) + 7) / 8 * 8, (Math.Max(8, g.Height) + 7) / 8 * 8);
        }

        /// <summary>The outline mask blurred for the generator (or null).</summary>
        private float[] PrepareMask(out int mw, out int mh)
        {
            mw = _maskW;
            mh = _maskH;
            if (_mask == null || !_settings.Generator.UseOutlineMask)
            {
                return null;
            }
            float[] copy = (float[])_mask.Clone();
            OutlineImporter.BoxBlur(copy, mw, mh, Math.Max(0, _settings.Generator.MaskBlur));
            return copy;
        }

        private void BtnGenPreview_Click(object sender, EventArgs e) => GeneratePreview();

        /// <summary>Cave entrances need the rule 1d -> mountain (map/cave-enti2mountain.txt); activate it once.</summary>
        private void EnsureCaveRule()
        {
            GeneratorSettings g = _settings.Generator;
            if (g.CaveEntrances > 0 && _rules.EnsureCaveRule(g.CaveGroup, g.MountainGroup))
            {
                MarkRulesDirty();
                BindRuleGrids();
                Log($"Regel aktiviert: {g.CaveGroup:x2} → {g.MountainGroup:x2} map/cave-enti2mountain.txt (Höhleneingänge). Mit 'JSON speichern' oder 'Als Dragon-Skripte exportieren' dauerhaft machen.");
            }
        }

        private void GeneratePreview()
        {
            EnsureCaveRule();
            DragonRuleSet rules = _rules;
            GeneratorSettings gs = _settings.Generator.Clone();
            if (gs.RandomSeed)
            {
                _settings.Generator.Seed = gs.Seed = Environment.TickCount & 0x7FFFFFFF;
                propGenerator.Refresh();
            }
            (int ww, int wh) = GeneratorWorldSize();
            double k = Math.Min(1.0, 900.0 / Math.Max(ww, wh));
            int pw = Math.Max(8, (int)(ww * k)), ph = Math.Max(8, (int)(wh * k));
            float[] mask = PrepareMask(out int mw, out int mh);
            Color[] colors = rules.GetColors();
            RunBusy("Vorschau", (progress, token) =>
            {
                IndexedMap preview = new MapGenerator(rules, gs).Generate(ww, wh, pw, ph, mask, mw, mh, progress, token);
                return IndexedBitmapIO.ToBitmap(preview, colors);
            }, bmp =>
            {
                picGenPreview.Image?.Dispose();
                picGenPreview.Image = bmp;
                lblGenInfo.Text = $"Vorschau {pw}×{ph} für {ww}×{wh}, Seed {gs.Seed}" + (mask != null ? ", mit Umriss-Maske" : string.Empty);
            });
        }

        private void BtnGenNewSeed_Click(object sender, EventArgs e)
        {
            _settings.Generator.Seed = new Random().Next(1, int.MaxValue);
            propGenerator.Refresh();
            GeneratePreview();
        }

        private void BtnGenToEditor_Click(object sender, EventArgs e)
        {
            EnsureCaveRule();
            bool intoCurrent = _settings.Generator.UseCurrentMapSize && _map != null;
            if (!intoCurrent && !ConfirmDiscardMap())
            {
                return;
            }
            DragonRuleSet rules = _rules;
            GeneratorSettings gs = _settings.Generator.Clone();
            (int ww, int wh) = GeneratorWorldSize();
            float[] mask = PrepareMask(out int mw, out int mh);
            RunBusy("Karte erzeugen", (progress, token) => new MapGenerator(rules, gs).Generate(ww, wh, ww, wh, mask, mw, mh, progress, token), generated =>
            {
                if (intoCurrent && _map != null && _map.Width == generated.Width && _map.Height == generated.Height)
                {
                    _history.Begin("Generator");
                    _history.TouchAll();
                    Buffer.BlockCopy(generated.Pixels, 0, _map.Pixels, 0, generated.Pixels.Length);
                    _history.End();
                    AfterExternalEdit();
                }
                else
                {
                    generated.FilePath = Path.Combine(_settings.ProjectFolder, $"Generiert_{gs.Seed}.bmp");
                    generated.Modified = true;
                    SetMap(generated);
                }
                tabMain.SelectedTab = tabEditor;
                Log($"Generator: {ww}×{wh}, Seed {gs.Seed}");
            });
        }

        private static JsonSerializerOptions PresetJson => new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };

        private static string PresetFolder => Path.Combine(StudioSettings.SettingsFolder, "Presets");

        private void BtnGenSavePreset_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(PresetFolder);
            using var dlg = new SaveFileDialog { Filter = "Generator-Preset|*.json", InitialDirectory = PresetFolder, FileName = "MeinPreset.json" };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(dlg.FileName, JsonSerializer.Serialize(_settings.Generator, PresetJson));
                SetStatus("Preset gespeichert: " + dlg.FileName);
            }
        }

        private void BtnGenLoadPreset_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(PresetFolder);
            using var dlg = new OpenFileDialog { Filter = "Generator-Preset|*.json", InitialDirectory = PresetFolder };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            try
            {
                GeneratorSettings g = JsonSerializer.Deserialize<GeneratorSettings>(File.ReadAllText(dlg.FileName), PresetJson);
                if (g != null)
                {
                    g.Biomes ??= GeneratorSettings.CreateDefaultBiomes();
                    _settings.Generator = g;
                    propGenerator.SelectedObject = g;
                    BindBiomes();
                    GeneratePreview();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Preset nicht lesbar: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BindBiomes()
        {
            gridBiomes.DataSource = new BindingList<GeneratorBiome>(_settings.Generator.Biomes);
        }

        private void BtnBiomesDefault_Click(object sender, EventArgs e)
        {
            _settings.Generator.Biomes = GeneratorSettings.CreateDefaultBiomes();
            BindBiomes();
        }

        // ================================================================== outline

        private Bitmap LoadOutlineImage()
        {
            string path = _settings.Outline.ImagePath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                BtnOutlineLoad_Click(this, EventArgs.Empty);
                path = _settings.Outline.ImagePath;
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    return null;
                }
            }
            using var fs = File.OpenRead(path);
            using Image img = Image.FromStream(fs);
            return new Bitmap(img);
        }

        private void BtnOutlineLoad_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "Bilder|*.png;*.bmp;*.jpg;*.jpeg;*.gif;*.tif;*.tiff|Alle Dateien|*.*", Title = "Umriss-Bild laden" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _settings.Outline.ImagePath = dlg.FileName;
            propOutline.Refresh();
            BtnOutlinePreview_Click(sender, e);
        }

        private (int w, int h) OutlineTargetSize()
        {
            if (_map != null)
            {
                return (_map.Width, _map.Height);
            }
            return GeneratorWorldSize();
        }

        private void BtnOutlinePreview_Click(object sender, EventArgs e)
        {
            Bitmap img = LoadOutlineImage();
            if (img == null)
            {
                return;
            }
            (int tw, int th) = OutlineTargetSize();
            double k = Math.Min(1.0, 900.0 / Math.Max(tw, th));
            int pw = Math.Max(8, (int)(tw * k)), ph = Math.Max(8, (int)(th * k));
            OutlineSettings s = _settings.Outline.Clone();
            s.Smooth = Math.Max(0, (int)Math.Round(s.Smooth * k));
            s.OutlineThickness = Math.Max(1, (int)Math.Round(s.OutlineThickness * k));
            RunBusy("Umriss-Vorschau", (progress, token) =>
            {
                using (img)
                {
                    float[] mask = OutlineImporter.CreateMask(img, pw, ph, s);
                    return MaskPreview(mask, pw, ph, s.OutlineThickness);
                }
            }, bmp =>
            {
                picGenPreview.Image?.Dispose();
                picGenPreview.Image = bmp;
                lblGenInfo.Text = $"Umriss-Vorschau für {tw}×{th} (grün = Land, blau = Wasser, gelb = Küste).";
            });
        }

        private static Bitmap MaskPreview(float[] mask, int w, int h, int thickness)
        {
            bool[] edge = OutlineImporter.EdgeOf(mask, w, h, Math.Max(1, thickness));
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppRgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                var row = new int[w];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        row[x] = edge[i] ? unchecked((int)0xFFFFD800) : mask[i] >= 0.5f ? unchecked((int)0xFF3E8E41) : unchecked((int)0xFF1C3D7A);
                    }
                    Marshal.Copy(row, 0, bd.Scan0 + y * bd.Stride, w);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return bmp;
        }

        private void BtnOutlineApply_Click(object sender, EventArgs e)
        {
            if (_map == null)
            {
                MnuNewMap_Click(sender, e);
                if (_map == null)
                {
                    return;
                }
            }
            Bitmap img = LoadOutlineImage();
            if (img == null)
            {
                return;
            }
            IndexedMap map = _map;
            OutlineSettings s = _settings.Outline.Clone();
            RunBusy("Umriss berechnen", (progress, token) =>
            {
                using (img)
                {
                    return OutlineImporter.CreateMask(img, map.Width, map.Height, s);
                }
            }, mask =>
            {
                OutlineImporter.Apply(map, mask, s, _history);
                _mask = mask;
                _maskW = map.Width;
                _maskH = map.Height;
                AfterExternalEdit();
                tabMain.SelectedTab = tabEditor;
                SetStatus("Umriss in die Karte gezeichnet (Rückgängig mit Strg+Z). Die Maske ist auch für den Generator gesetzt.");
            });
        }

        private void BtnOutlineAsMask_Click(object sender, EventArgs e)
        {
            Bitmap img = LoadOutlineImage();
            if (img == null)
            {
                return;
            }
            (int tw, int th) = OutlineTargetSize();
            OutlineSettings s = _settings.Outline.Clone();
            RunBusy("Maske berechnen", (progress, token) =>
            {
                using (img)
                {
                    return OutlineImporter.CreateMask(img, tw, th, s);
                }
            }, mask =>
            {
                _mask = mask;
                _maskW = tw;
                _maskH = th;
                _settings.Generator.UseOutlineMask = true;
                propGenerator.Refresh();
                SetStatus($"Umriss-Maske {tw}×{th} gesetzt – der Generator erzeugt jetzt Land nur im Umriss.");
                GeneratePreviewLater();
            });
        }

        /// <summary>Runs the preview as soon as the current background task is finished.</summary>
        private void GeneratePreviewLater()
        {
            if (_busy)
            {
                _afterBusy = GeneratePreview;
            }
            else
            {
                GeneratePreview();
            }
        }

        private void BtnMaskFromMap_Click(object sender, EventArgs e)
        {
            if (_map == null)
            {
                return;
            }
            _mask = OutlineImporter.MaskFromMap(_map, _rules, _settings.Generator.WaterGroup);
            _maskW = _map.Width;
            _maskH = _map.Height;
            _settings.Generator.UseOutlineMask = true;
            propGenerator.Refresh();
            SetStatus("Maske aus der aktuellen Karte gesetzt (alles außer Wasser = Land).");
            GeneratePreview();
        }

        private void BtnMaskClear_Click(object sender, EventArgs e)
        {
            _mask = null;
            SetStatus("Umriss-Maske gelöscht.");
        }

        // ================================================================== rule tables

        private void BindRuleGrids()
        {
            BindGrid(gridGroups, new BindingList<DragonGroup>(_rules.Groups));
            BindGrid(gridPaletteEntries, new BindingList<DragonPaletteEntry>(_rules.Palette));
            BindGrid(gridTransitions, new BindingList<TransitionRule>(_rules.Transitions));
            BindGrid(gridItemRules, new BindingList<ItemRule>(_rules.ItemRules));
            BindGrid(gridUneven, new BindingList<UnevenRule>(_rules.UnevenRules));
            BindGrid(gridFixes, new BindingList<FixEntry>(_rules.Fixes));
            BindGrid(gridAliases, new BindingList<ColorAlias>(_rules.ColorAliases));
            BindBiomes();
            ShowTransitionFile(null);
            ShowItemFile(null);
            EnsureSwatchColumn(gridPaletteEntries);
            EnsurePreviewColumn(gridPatterns);

            if (!_gridEventsHooked)
            {
                _gridEventsHooked = true;
                gridTransitions.CurrentCellChanged += (s, e) => ShowTransitionFile(gridTransitions.CurrentRow?.DataBoundItem as TransitionRule);
                gridItemRules.CurrentCellChanged += (s, e) => ShowItemFile(gridItemRules.CurrentRow?.DataBoundItem as ItemRule);
                gridPatterns.CellPainting += GridPatterns_CellPainting;
                gridPaletteEntries.CellPainting += GridPalette_CellPainting;
                foreach (DataGridView g in new[] { gridGroups, gridPaletteEntries, gridTransitions, gridItemRules, gridUneven, gridFixes, gridAliases, gridPatterns, gridItemGroups })
                {
                    g.CellFormatting += RuleGrid_CellFormatting;
                    g.CellParsing += RuleGrid_CellParsing;
                    g.CellValueChanged += RuleGrid_CellValueChanged;
                    g.UserDeletedRow += (s, e) => MarkRulesDirty();
                    g.UserAddedRow += (s, e) => MarkRulesDirty();
                }
            }
        }

        private bool _gridEventsHooked;

        /// <summary>Binds a list; enum properties get a combo box column.</summary>
        private static void BindGrid(DataGridView grid, object source)
        {
            grid.AutoGenerateColumns = true;
            grid.DataSource = source;
            foreach (DataGridViewColumn col in grid.Columns.Cast<DataGridViewColumn>().ToList())
            {
                Type t = col.ValueType;
                if (t != null && t.IsEnum && col is not DataGridViewComboBoxColumn)
                {
                    var combo = new DataGridViewComboBoxColumn
                    {
                        DataPropertyName = col.DataPropertyName,
                        HeaderText = col.HeaderText,
                        Name = col.Name,
                        DataSource = Enum.GetValues(t),
                        ValueType = t,
                        FlatStyle = FlatStyle.Flat
                    };
                    int index = col.Index;
                    grid.Columns.Remove(col);
                    grid.Columns.Insert(index, combo);
                }
            }
        }

        private void RuleGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            MarkRulesDirty();
            if (sender == gridPaletteEntries || sender == gridGroups)
            {
                RefreshPaletteViews();
                _rules.BuildHeightLookups(out byte[] raise, out byte[] lower);
                mapCanvas.SetHeightLookups(raise, lower);
            }
            if (sender == gridTransitions)
            {
                ShowTransitionFile(gridTransitions.CurrentRow?.DataBoundItem as TransitionRule);
            }
            if (sender == gridItemRules)
            {
                ShowItemFile(gridItemRules.CurrentRow?.DataBoundItem as ItemRule);
            }
        }

        /// <summary>Group numbers, palette indices and tile ids are shown in hex like in the Dragon scripts.</summary>
        private static int HexDigits(DataGridView grid, int column)
        {
            if (column < 0)
            {
                return 0;
            }
            switch (grid.Columns[column].DataPropertyName)
            {
                case nameof(TransitionRule.GroupA):
                case nameof(TransitionRule.GroupB):
                case nameof(ItemRule.Group):
                case nameof(DragonGroup.Id):
                    return 2;
                case nameof(DragonPaletteEntry.Index):
                    return grid.DataSource is BindingList<GeneratorBiome> ? 0 : 2;
                case nameof(FixEntry.TileId):
                    return 4;
                default:
                    return 0;
            }
        }

        private void RuleGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            int digits = HexDigits(grid, e.ColumnIndex);
            if (digits == 0 || e.Value == null)
            {
                return;
            }
            long v = Convert.ToInt64(e.Value);
            e.Value = v < 0 ? "*" : v.ToString(digits == 4 ? "x4" : "x2");
            e.FormattingApplied = true;
        }

        private void RuleGrid_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            var grid = (DataGridView)sender;
            int digits = HexDigits(grid, e.ColumnIndex);
            if (digits == 0 || e.Value is not string text)
            {
                return;
            }
            text = text.Trim();
            int v = text == "*" || text == "-1" ? -1 : HexList.ParseHexInt(text);
            e.Value = Convert.ChangeType(v, e.DesiredType);
            e.ParsingApplied = true;
        }

        private void Grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            SetStatus("Ungültiger Wert: " + e.Exception?.Message);
        }

        private void ShowTransitionFile(TransitionRule rule)
        {
            TransitionFile file = null;
            if (rule != null && !string.IsNullOrWhiteSpace(rule.File))
            {
                rule.File = DragonRuleSet.NormalizePath(rule.File);
                file = _rules.FindTransitionFile(rule.File);
                if (file == null)
                {
                    // new rule typed by the user: create the (empty) file so patterns can be added
                    file = new TransitionFile { Path = rule.File };
                    _rules.TransitionFiles.Add(file);
                }
            }
            if (file == _shownTransitionFile && gridPatterns.DataSource != null)
            {
                return;
            }
            _shownTransitionFile = file;
            gridPatterns.DataSource = file == null ? null : new BindingList<TransitionPattern>(file.Patterns);
            EnsurePreviewColumn(gridPatterns);
            lblPatternFile.Text = file == null
                ? "Muster der gewählten Übergangsdatei"
                : $"{file.Path} – {file.Patterns.Count} Muster (Reihenfolge NW N NO O SO S SW W; A = eigene Gruppe, B = Nachbar). Neue Zeile unten = neues Muster.";
        }

        private void ShowItemFile(ItemRule rule)
        {
            ItemFile file = null;
            if (rule != null && !string.IsNullOrWhiteSpace(rule.File))
            {
                rule.File = DragonRuleSet.NormalizePath(rule.File);
                if (!rule.File.Contains('/'))
                {
                    rule.File = "items/" + rule.File;
                }
                file = _rules.FindItemFile(rule.File);
                if (file == null)
                {
                    file = new ItemFile { Path = rule.File };
                    _rules.ItemFiles.Add(file);
                }
            }
            if (file == _shownItemFile && gridItemGroups.DataSource != null)
            {
                return;
            }
            _shownItemFile = file;
            gridItemGroups.DataSource = file == null ? null : new BindingList<ItemGroupDef>(file.Groups);
            lblItemFile.Text = file == null
                ? "Item-Gruppen der gewählten Datei"
                : $"{file.Path} – {file.Groups.Count} Objekte. Statics: id(hex) z hue(hex) dx dy; mehrere mit ';' trennen.";
        }

        private static void EnsurePreviewColumn(DataGridView grid)
        {
            if (!grid.Columns.Contains("colPreview"))
            {
                grid.Columns.Insert(0, new DataGridViewTextBoxColumn { Name = "colPreview", HeaderText = "3×3", ReadOnly = true, Width = 40, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
            }
            grid.RowTemplate.Height = 36;
        }

        private static void EnsureSwatchColumn(DataGridView grid)
        {
            if (!grid.Columns.Contains("colSwatch"))
            {
                grid.Columns.Insert(0, new DataGridViewTextBoxColumn { Name = "colSwatch", HeaderText = "Farbe", ReadOnly = true, Width = 44, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
            }
        }

        private void GridPalette_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || gridPaletteEntries.Columns[e.ColumnIndex].Name != "colSwatch")
            {
                return;
            }
            e.PaintBackground(e.CellBounds, true);
            if (gridPaletteEntries.Rows[e.RowIndex].DataBoundItem is DragonPaletteEntry p)
            {
                Rectangle r = Rectangle.Inflate(e.CellBounds, -4, -3);
                using var b = new SolidBrush(p.Color);
                e.Graphics.FillRectangle(b, r);
                e.Graphics.DrawRectangle(Pens.Black, r);
            }
            e.Handled = true;
        }

        /// <summary>Draws the 3×3 neighbour pattern: centre = the cell, A = own group (green), B = neighbour (orange).</summary>
        private void GridPatterns_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || gridPatterns.Columns[e.ColumnIndex].Name != "colPreview")
            {
                return;
            }
            e.PaintBackground(e.CellBounds, true);
            if (gridPatterns.Rows[e.RowIndex].DataBoundItem is TransitionPattern p)
            {
                int size = Math.Min(e.CellBounds.Width, e.CellBounds.Height) - 6;
                int cell = Math.Max(3, size / 3);
                int x0 = e.CellBounds.X + (e.CellBounds.Width - cell * 3) / 2, y0 = e.CellBounds.Y + (e.CellBounds.Height - cell * 3) / 2;
                // pattern index -> grid position (NW N NE E SE S SW W)
                int[] gx = { 0, 1, 2, 2, 2, 1, 0, 0 };
                int[] gy = { 0, 0, 0, 1, 2, 2, 2, 1 };
                string pat = p.Pattern;
                using var a = new SolidBrush(Color.FromArgb(90, 170, 90));
                using var b = new SolidBrush(Color.FromArgb(230, 140, 40));
                using var c = new SolidBrush(Color.FromArgb(40, 110, 40));
                for (int k = 0; k < 8; k++)
                {
                    e.Graphics.FillRectangle(pat[k] == 'B' ? b : a, x0 + gx[k] * cell, y0 + gy[k] * cell, cell, cell);
                }
                e.Graphics.FillRectangle(c, x0 + cell, y0 + cell, cell, cell);
                e.Graphics.DrawRectangle(Pens.Black, x0, y0, cell * 3, cell * 3);
            }
            e.Handled = true;
        }

        private void BtnRulesReload_Click(object sender, EventArgs e)
        {
            if (_rulesDirty && MessageBox.Show(this, "Ungespeicherte Regeländerungen verwerfen?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            LoadRules();
            LoadColorTable();
        }

        private void MnuLoadScripts_Click(object sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog { Description = "Dragon Skript-Ordner (mit groups.txt und maptrans.txt)", SelectedPath = _settings.ScriptFolder };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _settings.ScriptFolder = dlg.SelectedPath;
            _settings.RuleSetFile = string.Empty;
            propSettings.Refresh();
            LoadRules();
            LoadColorTable();
        }

        private void MnuLoadJson_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "Dragon Map Studio Regelsatz|*.json", InitialDirectory = StudioSettings.SettingsFolder };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _settings.RuleSetFile = dlg.FileName;
            propSettings.Refresh();
            LoadRules();
            LoadColorTable();
        }

        private void MnuSaveJson_Click(object sender, EventArgs e) => SaveRulesJson(null);

        /// <summary>Saves the rule set as JSON (asks for the file when <paramref name="path"/> is null).</summary>
        private bool SaveRulesJson(string path)
        {
            if (path == null)
            {
                Directory.CreateDirectory(StudioSettings.SettingsFolder);
                using var dlg = new SaveFileDialog
                {
                    Filter = "Dragon Map Studio Regelsatz|*.json",
                    InitialDirectory = string.IsNullOrWhiteSpace(_settings.RuleSetFile) ? StudioSettings.SettingsFolder : Path.GetDirectoryName(_settings.RuleSetFile),
                    FileName = string.IsNullOrWhiteSpace(_settings.RuleSetFile) ? "rules.json" : Path.GetFileName(_settings.RuleSetFile)
                };
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }
                path = dlg.FileName;
            }
            try
            {
                _rules.SaveJson(path);
                _settings.RuleSetFile = path;
                propSettings.Refresh();
                _rulesDirty = false;
                UpdateTitle();
                SetStatus("Regelsatz gespeichert: " + path);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Speichern fehlgeschlagen: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void MnuExportScripts_Click(object sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog { Description = "Ziel-Ordner für die Dragon-Skripte (am besten ein neuer, leerer Ordner)" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            try
            {
                DragonScriptIO.SaveFolder(_rules, dlg.SelectedPath);
                PaletteIO.Save(Path.Combine(dlg.SelectedPath, "palette.act"), _rules.GetColors());
                SetStatus("Dragon-Skripte exportiert: " + dlg.SelectedPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Export fehlgeschlagen: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MnuImportPalette_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = PaletteIO.FileFilter, Title = "Palette importieren (nur die RGB-Werte werden übernommen)" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            try
            {
                Color[] colors = PaletteIO.Load(dlg.FileName);
                for (int i = 0; i < 256; i++)
                {
                    _rules.Palette[i].SetColor(colors[i]);
                }
                MarkRulesDirty();
                ApplyRules();
                SetStatus("Palette übernommen: " + dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Palette nicht lesbar: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MnuExportPalette_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog { Filter = PaletteIO.SaveFilter, FileName = "dragon-palette.act" };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                PaletteIO.Save(dlg.FileName, _rules.GetColors(), _rules.Palette.Select(p => p.Name).ToList());
                SetStatus("Palette exportiert: " + dlg.FileName);
            }
        }

        private void MnuCreateColorTable_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog { Filter = "PNG|*.png|BMP|*.bmp", FileName = "Farbtabelle.png" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            using Bitmap bmp = ColorTableRenderer.Render(_rules, out _);
            bmp.Save(dlg.FileName, dlg.FileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ? ImageFormat.Bmp : ImageFormat.Png);
            RenderColorTableFromRules();
            SetStatus("Farbtabelle gespeichert: " + dlg.FileName);
        }

        private void MnuValidate_Click(object sender, EventArgs e)
        {
            List<string> problems = _rules.Validate();
            txtRuleWarnings.Text = (problems.Count == 0 ? "Keine Probleme gefunden." : string.Join(Environment.NewLine, problems)) +
                                   (_rules.Warnings.Count > 0 ? Environment.NewLine + Environment.NewLine + "Beim Laden:" + Environment.NewLine + string.Join(Environment.NewLine, _rules.Warnings) : string.Empty);
            tabMain.SelectedTab = tabRules;
            tabRulesInner.SelectedTab = tabRuleWarnings;
            SetStatus($"Prüfung: {problems.Count} Hinweise.");
        }

        // ================================================================== build

        private BuildOptions CurrentBuildOptions()
        {
            BuildOptions o = _settings.Build.Clone();
            if (string.IsNullOrWhiteSpace(o.OutputFolder))
            {
                o.OutputFolder = _settings.OutputFolder;
            }
            return o;
        }

        private void BtnBuildCurrent_Click(object sender, EventArgs e)
        {
            if (_map == null)
            {
                MessageBox.Show(this, "Keine Karte geöffnet.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            StartBuild(_map.Clone(), _map.FilePath ?? "aktuelle Karte");
        }

        private void BtnBuildFromFile_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = IndexedBitmapIO.OpenFilter, InitialDirectory = _settings.ProjectFolder, Title = "Dragon-BMP wählen" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            try
            {
                var warnings = new List<string>();
                IndexedMap map = IndexedBitmapIO.Load(dlg.FileName, _rules, _settings.RemapForeignPalettes ? ColorMatchMode.NearestColor : ColorMatchMode.KeepIndices, warnings);
                warnings.ForEach(Log);
                StartBuild(map, dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartBuild(IndexedMap map, string sourceName)
        {
            BuildOptions o = CurrentBuildOptions();
            DragonRuleSet rules = _rules;
            tabMain.SelectedTab = tabBuild;
            Log($"=== Build: {sourceName} → {o.OutputFolder} (Karte {o.MapNumber}) ===");
            RunBusy("MUL erzeugen", (progress, token) => new DragonCompiler(rules, o, Log).Build(map, progress, token), result =>
            {
                foreach (string w in result.Warnings)
                {
                    Log("Hinweis: " + w);
                }
                foreach (string f in result.Files)
                {
                    Log("Datei: " + f);
                }
                Log($"Fertig in {result.Duration.TotalSeconds:0.0} s.");
            });
        }

        private void BtnOpenOutput_Click(object sender, EventArgs e) => OpenFolder(CurrentBuildOptions().OutputFolder);

        private void BtnM2BBrowse_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "Karten|map*.mul;map*LegacyMUL.uop|Alle Dateien|*.*", Title = "map#.mul oder map#LegacyMUL.uop wählen" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            txtM2BFile.Text = dlg.FileName;
            numM2BMapNo.Value = Math.Clamp(MulMapIO.GuessMapNumber(dlg.FileName), 0, 9);
            MapSizePreset guess = MulMapIO.GuessSize(dlg.FileName);
            if (guess != null)
            {
                cmbM2BSize.SelectedItem = guess;
            }
        }

        private void CmbM2BSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbM2BSize.SelectedItem is MapSizePreset p)
            {
                numM2BWidth.Value = p.Width;
                numM2BHeight.Value = p.Height;
            }
        }

        private void BtnM2BConvert_Click(object sender, EventArgs e)
        {
            string file = txtM2BFile.Text;
            if (!File.Exists(file))
            {
                BtnM2BBrowse_Click(sender, e);
                file = txtM2BFile.Text;
                if (!File.Exists(file))
                {
                    return;
                }
            }
            if (!ConfirmDiscardMap())
            {
                return;
            }
            int w = (int)numM2BWidth.Value / 8 * 8, h = (int)numM2BHeight.Value / 8 * 8, num = (int)numM2BMapNo.Value;
            DragonRuleSet rules = _rules;
            int unknownIdx = rules.NearestIndex(0x04, 0);
            byte unknown = (byte)(unknownIdx < 0 ? 0 : unknownIdx);
            Log($"=== Einlesen: {file} ({w}×{h}) ===");
            RunBusy("Karte einlesen", (progress, token) =>
            {
                MulMapIO.ReadMap(file, w, h, out ushort[] tiles, out sbyte[] z, num);
                IndexedMap map = MapToBitmapConverter.Convert(rules, tiles, z, w, h, unknown, out long unknownCount, progress, token);
                return (map, unknownCount);
            }, r =>
            {
                r.map.FilePath = Path.Combine(_settings.ProjectFolder, Path.GetFileNameWithoutExtension(file) + "_import.bmp");
                r.map.Modified = true;
                SetMap(r.map);
                tabMain.SelectedTab = tabEditor;
                Log($"Eingelesen: {w}×{h}, {r.unknownCount:N0} Tiles ohne passende Farbe (als {unknown:x2} gesetzt).");
            });
        }

        // ================================================================== settings

        private void BtnSettingsSave_Click(object sender, EventArgs e)
        {
            _settings.Save();
            mapCanvas.GridFromZoom = _settings.GridFromZoom;
            _history.MemoryLimit = Math.Max(64, _settings.UndoMemoryMb) * 1024L * 1024;
            cmbCopyFormat.SelectedIndex = (int)_settings.CopyFormat;
            RefreshLibrary();
            SetStatus("Einstellungen gespeichert: " + StudioSettings.SettingsFile);
        }

        private void BtnSettingsApply_Click(object sender, EventArgs e)
        {
            BtnSettingsSave_Click(sender, e);
            LoadRules();
            LoadColorTable();
        }

        private void BtnOpenProjectFolder_Click(object sender, EventArgs e) => OpenFolder(_settings.ProjectFolder);

        private void BtnOpenScriptFolder_Click(object sender, EventArgs e) => OpenFolder(_settings.ScriptFolder);
    }
}
