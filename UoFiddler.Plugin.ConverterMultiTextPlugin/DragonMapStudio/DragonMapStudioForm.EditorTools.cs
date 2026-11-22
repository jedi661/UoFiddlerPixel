/***************************************************************************
 *
 *  * Dragon Map Studio – map editor: brush catalogue, colour info, colour stamps,
 *  * CGI preview of a marked area, context menu
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        // ------------------------------------------------ info
        private Panel _infoSwatch;
        private Label _infoText;
        private FlowLayoutPanel _infoTiles;
        private int _infoIndex = -2;

        // ------------------------------------------------ brush catalogue
        private ComboBox _catCategory;
        private TextBox _catSearch;
        private ListBox _catList;

        private sealed class CatalogItem
        {
            public string Header;
            public Color HeaderColor;
            public DragonPaletteEntry Entry;
        }

        // ------------------------------------------------ stamps
        private ListBox _stampList;
        private CheckBox _stampAdapt;
        private List<ColorStamp> _stamps = new List<ColorStamp>();

        // ------------------------------------------------ CGI preview window
        private Form _previewWindow;
        private IsoMapView _previewView;
        private ContextMenuStrip _editorMenu;

        /// <summary>Folder of the colour stamps: setting, otherwise "Farbstempel" in the project folder.</summary>
        private string ColorStampFolder
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_settings.ColorStampFolder))
                {
                    return _settings.ColorStampFolder;
                }
                string project = string.IsNullOrWhiteSpace(_settings.ProjectFolder) ? StudioSettings.SettingsFolder : _settings.ProjectFolder;
                return Path.Combine(project, "Farbstempel");
            }
        }

        /// <summary>Old place (until Stempel 10): "Farbstempel" inside the template folder – the template list read the stamps as templates.</summary>
        private string OldColorStampFolder => Path.Combine(string.IsNullOrWhiteSpace(_settings.TemplateFolder) ? StudioSettings.SettingsFolder : _settings.TemplateFolder, "Farbstempel");

        /// <summary>Moves stamps of the old place into the stamp folder (once).</summary>
        private void MigrateColorStamps()
        {
            try
            {
                string old = OldColorStampFolder, now = ColorStampFolder;
                if (!Directory.Exists(old) || string.Equals(Path.GetFullPath(old).TrimEnd('\\', '/'), Path.GetFullPath(now).TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                string[] files = Directory.GetFiles(old, "*" + ColorStampTools.Extension);
                if (files.Length == 0)
                {
                    return;
                }
                Directory.CreateDirectory(now);
                int moved = 0;
                foreach (string f in files)
                {
                    string target = Path.Combine(now, Path.GetFileName(f));
                    if (!File.Exists(target))
                    {
                        File.Move(f, target);
                        moved++;
                    }
                }
                if (Directory.GetFileSystemEntries(old).Length == 0)
                {
                    Directory.Delete(old);
                }
                Log($"{moved} Farb-Stempel aus {old} nach {now} verschoben (im Vorlagen-Ordner wurden sie als Vorlagen mitgelesen).");
                _templates = null;
            }
            catch (Exception ex)
            {
                Log("Farb-Stempel verschieben: " + ex.Message);
            }
        }

        /// <summary>Chooses another stamp folder (e.g. inside the project) and offers to move the stamps there.</summary>
        private void ChooseColorStampFolder()
        {
            using var dlg = new FolderBrowserDialog { Description = L.T("Ordner für die Farb-Stempel (z. B. im Projekt-Ordner)"), SelectedPath = ColorStampFolder, ShowNewFolderButton = true };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            string old = ColorStampFolder;
            string[] files = Directory.Exists(old) ? Directory.GetFiles(old, "*" + ColorStampTools.Extension) : Array.Empty<string>();
            _settings.ColorStampFolder = dlg.SelectedPath;
            _settings.Save();
            if (files.Length > 0 && !string.Equals(Path.GetFullPath(old), Path.GetFullPath(dlg.SelectedPath), StringComparison.OrdinalIgnoreCase))
            {
                DialogResult r = MessageBox.Show(this, $"{files.Length} Stempel aus\n{old}\nin den neuen Ordner\n{dlg.SelectedPath}\n\nJa = verschieben, Nein = kopieren, Abbrechen = dort lassen", Text,
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r != DialogResult.Cancel)
                {
                    Directory.CreateDirectory(dlg.SelectedPath);
                    foreach (string f in files)
                    {
                        string target = Path.Combine(dlg.SelectedPath, Path.GetFileName(f));
                        if (File.Exists(target))
                        {
                            continue;
                        }
                        if (r == DialogResult.Yes)
                        {
                            File.Move(f, target);
                        }
                        else
                        {
                            File.Copy(f, target);
                        }
                    }
                }
            }
            LoadColorStamps();
            SetStatus(L.T("Farbstempel-Ordner: ") + ColorStampFolder);
        }

        /// <summary>Adds stamp files from anywhere (other project, download) into the stamp folder.</summary>
        private void ImportColorStamps()
        {
            using var dlg = new OpenFileDialog { Filter = "Farb-Stempel|*" + ColorStampTools.Extension, Multiselect = true, Title = L.T("Farb-Stempel hinzufügen") };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            Directory.CreateDirectory(ColorStampFolder);
            int n = 0;
            string last = null;
            foreach (string f in dlg.FileNames)
            {
                string target = Path.Combine(ColorStampFolder, Path.GetFileName(f));
                if (string.Equals(Path.GetFullPath(f), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                File.Copy(f, target, true);
                last = target;
                n++;
            }
            LoadColorStamps(last);
            SetStatus($"{n} Stempel in {ColorStampFolder} übernommen.");
        }

        /// <summary>Sections of the map editor side panel (called by BuildEditorLayout).</summary>
        private (CollapsibleSection info, CollapsibleSection catalog, CollapsibleSection stamps) BuildEditorToolSections()
        {
            // ---------------- info: colour under the mouse / picked
            CollapsibleSection info = NewSection("editor.info", "Farbe unter der Maus / Pipette", ColView, 104);
            _infoSwatch = new Panel { Location = new Point(6, 6), Size = new Size(40, 40), BorderStyle = BorderStyle.FixedSingle };
            _infoText = new Label { Location = new Point(52, 4), AutoSize = false, Size = new Size(240, 52), Font = new Font("Segoe UI", 8f), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            _infoTiles = new FlowLayoutPanel { Location = new Point(6, 58), Size = new Size(290, 40), WrapContents = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            info.Body.Controls.Add(_infoSwatch);
            info.Body.Controls.Add(_infoText);
            info.Body.Controls.Add(_infoTiles);
            info.Body.Resize += (s, e) =>
            {
                _infoText.Width = Math.Max(80, info.Body.ClientSize.Width - 58);
                _infoTiles.Width = Math.Max(80, info.Body.ClientSize.Width - 12);
            };

            // ---------------- brush catalogue
            CollapsibleSection catalog = NewSection("editor.catalog", "Pinsel-Katalog (Farben nach Art)", ColTerrain, 270);
            _catCategory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Left, Width = 130 };
            _catCategory.Items.AddRange(ColorStampTools.Categories.Select(c => (object)L.T(c)).ToArray());
            _catCategory.SelectedIndex = 0;
            _catSearch = new TextBox { Dock = DockStyle.Fill, PlaceholderText = L.T("Suchen: Name, Gruppe, Hex …") };
            var catTop = new Panel { Dock = DockStyle.Top, Height = 26, Padding = new Padding(0, 0, 0, 3) };
            catTop.Controls.Add(_catSearch);
            catTop.Controls.Add(_catCategory);
            _catList = new ListBox
            {
                Dock = DockStyle.Fill,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 22,
                IntegralHeight = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            _catList.DrawItem += CatList_DrawItem;
            _catList.MouseDown += CatList_MouseDown;
            _catList.DoubleClick += (s, e) => SelectTool(MapTool.Brush);
            var catTip = new ToolTip();
            int lastTip = -1;
            _catList.MouseMove += (s, e) =>
            {
                int i = _catList.IndexFromPoint(e.Location);
                if (i == lastTip)
                {
                    return;
                }
                lastTip = i;
                catTip.SetToolTip(_catList, i >= 0 && _catList.Items[i] is CatalogItem ci && ci.Entry != null ? EntryDescription(ci.Entry) : string.Empty);
            };
            var catHint = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 18,
                Text = L.T("Links = Farbe 1 · Rechts = Farbe 2 · Doppelklick = Pinsel"),
                ForeColor = Color.FromArgb(71, 85, 105),
                Font = new Font("Segoe UI", 7.5f)
            };
            catalog.Body.Controls.Add(_catList);
            catalog.Body.Controls.Add(catHint);
            catalog.Body.Controls.Add(catTop);
            _catCategory.SelectedIndexChanged += (s, e) => FillBrushCatalog();
            _catSearch.TextChanged += (s, e) => FillBrushCatalog();

            // ---------------- stamps & preview
            CollapsibleSection stamps = NewSection("editor.stamps", "Bereich: CGI-Vorschau & Farb-Stempel", ColCave, 250, true);
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true };
            Button B(string text, Color c, EventHandler click, bool primary = false)
            {
                var b = new Button { Text = L.T(text), AutoSize = true, Margin = new Padding(2) };
                StyleButton(b, c, primary);
                b.AutoSize = true;
                b.Click += click;
                return b;
            }
            top.Controls.Add(B("Bereich markieren", ColView, (s, e) => SelectTool(MapTool.Select)));
            top.Controls.Add(B("Auswahl → CGI-Vorschau (V)", ColCave, (s, e) => ShowSelectionInCgi(), true));
            top.Controls.Add(B("Auswahl → Stempel speichern", ColBuild, (s, e) => SaveSelectionAsStamp()));
            _stampAdapt = new CheckBox { Text = L.T("Höhen an den Boden anpassen"), Checked = true, AutoSize = true, Margin = new Padding(4, 6, 2, 2) };
            top.Controls.Add(_stampAdapt);
            _stampRepeat = new CheckBox { Text = L.T("Mehrfach setzen"), Checked = false, AutoSize = true, Margin = new Padding(4, 6, 2, 2) };
            top.Controls.Add(_stampRepeat);
            _stampList = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
            _stampList.DoubleClick += (s, e) => PlaceSelectedStamp();
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            bottom.Controls.Add(B("Stempel setzen", ColBuild, (s, e) => PlaceSelectedStamp(), true));
            bottom.Controls.Add(B("Abbrechen", ColView, (s, e) => CancelStamp()));
            bottom.Controls.Add(B("Löschen", ColCheck, (s, e) => DeleteSelectedStamp()));
            bottom.Controls.Add(B("Ordner", ColView, (s, e) =>
            {
                Directory.CreateDirectory(ColorStampFolder);
                OpenFolder(ColorStampFolder);
            }));
            stamps.Body.Controls.Add(_stampList);
            stamps.Body.Controls.Add(bottom);
            stamps.Body.Controls.Add(top);

            // ---------------- map events
            mapCanvas.ContextRequested += MapCanvas_ContextRequested;
            mapCanvas.CursorMoved += (s, e) => ShowEditorInfo(e.Index, false);
            mapCanvas.ColorPicked += (s, e) =>
            {
                ShowEditorInfo(e.Index, true);
                SelectInCatalog(e.Index);
            };
            FillBrushCatalog();
            LoadColorStamps();
            return (info, catalog, stamps);
        }

        // ================================================================== info

        private string EntryDescription(DragonPaletteEntry p)
        {
            string group = p.Group >= 0 ? $"{_rules.HexGroup(p.Group)} {_rules.GroupName(p.Group)}" : L.T("keine Gruppe");
            ushort[] tiles = p.GetTileIds();
            string t = tiles.Length == 0 ? "-" : string.Join(" ", tiles.Take(8).Select(x => x.ToString("x4"))) + (tiles.Length > 8 ? " …" : string.Empty);
            return $"{p.HexIndex}  {p.Name}\r\n{L.T("Gruppe")}: {group} · Z {p.Altitude}\r\nTiles: {t}" +
                   (p.Defined ? string.Empty : "\r\n" + L.T("nicht definiert – wird nicht gebaut"));
        }

        /// <summary>Colour under the mouse (or picked): swatch, name, group, height, tiles with their land art.</summary>
        private void ShowEditorInfo(int index, bool picked)
        {
            if (_infoText == null || index == _infoIndex && !picked)
            {
                return;
            }
            _infoIndex = index;
            if (index < 0 || index >= _rules.Palette.Count)
            {
                _infoSwatch.BackColor = SystemColors.Control;
                _infoText.Text = L.T("Maus über die Karte bewegen oder mit der Pipette (I) eine Farbe aufnehmen.");
                _infoTiles.Controls.Clear();
                return;
            }
            DragonPaletteEntry p = _rules.Palette[index];
            _infoSwatch.BackColor = p.Color;
            _infoText.Text = (picked ? "✔ " : string.Empty) + EntryDescription(p).Replace("\r\nTiles:", "\r\nTiles:");
            _infoTiles.SuspendLayout();
            _infoTiles.Controls.Clear();
            foreach (ushort id in p.GetTileIds().Distinct().Take(7))
            {
                Image art = null;
                try
                {
                    art = UoArtCache.Land(id);
                }
                catch (Exception)
                {
                    art = null;
                }
                var pb = new PictureBox
                {
                    Size = new Size(34, 34),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = art,
                    BackColor = art == null ? UoArtCache.LandColor(id) : Color.FromArgb(24, 24, 27),
                    Margin = new Padding(1)
                };
                new ToolTip().SetToolTip(pb, $"{id:x4} {UoArtCache.LandName(id)}");
                _infoTiles.Controls.Add(pb);
            }
            _infoTiles.ResumeLayout();
        }

        // ================================================================== brush catalogue

        private void FillBrushCatalog()
        {
            if (_catList == null || _rules == null)
            {
                return;
            }
            string cat = _catCategory.SelectedIndex <= 0 ? null : ColorStampTools.Categories[_catCategory.SelectedIndex];
            string f = _catSearch.Text.Trim();
            var items = new List<CatalogItem>();
            foreach (IGrouping<int, DragonPaletteEntry> g in _rules.Palette.Where(p => p.Defined).GroupBy(p => p.Group).OrderBy(g => g.Key))
            {
                string gname = g.Key >= 0 ? _rules.GroupName(g.Key) : L.T("ohne Gruppe");
                var entries = g.Where(e => cat == null || ColorStampTools.CategoryOf(e, gname) == cat)
                               .Where(e => f.Length == 0 || e.Name.Contains(f, StringComparison.OrdinalIgnoreCase) || gname.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                                           e.HexIndex.Contains(f, StringComparison.OrdinalIgnoreCase))
                               .OrderBy(e => e.Altitude).ToList();
                if (entries.Count == 0)
                {
                    continue;
                }
                string category = ColorStampTools.CategoryOf(entries[0], gname);
                items.Add(new CatalogItem { Header = $"{(g.Key >= 0 ? _rules.HexGroup(g.Key) : "--")}  {gname}  ·  {L.T(category)}", HeaderColor = CategoryColor(category) });
                items.AddRange(entries.Select(e => new CatalogItem { Entry = e }));
            }
            _catList.BeginUpdate();
            _catList.Items.Clear();
            _catList.Items.AddRange(items.Cast<object>().ToArray());
            _catList.EndUpdate();
        }

        private static Color CategoryColor(string category) => category switch
        {
            "Gras & Wald" => Color.FromArgb(22, 128, 61),
            "Wasser & Küste" => Color.FromArgb(14, 116, 144),
            "Berg & Fels" => Color.FromArgb(87, 83, 78),
            "Wege & Straßen" => Color.FromArgb(146, 64, 14),
            "Hausböden" => Color.FromArgb(124, 58, 237),
            "Höhle & Dungeon" => Color.FromArgb(180, 83, 9),
            _ => Color.FromArgb(71, 85, 105)
        };

        private void CatList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || !(_catList.Items[e.Index] is CatalogItem item))
            {
                return;
            }
            Graphics g = e.Graphics;
            Rectangle r = e.Bounds;
            using var font = new Font("Segoe UI", 8f, item.Entry == null ? FontStyle.Bold : FontStyle.Regular);
            if (item.Entry == null)
            {
                using var back = new SolidBrush(item.HeaderColor);
                g.FillRectangle(back, r);
                TextRenderer.DrawText(g, item.Header, font, new Rectangle(r.X + 4, r.Y, r.Width - 6, r.Height), Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                return;
            }
            DragonPaletteEntry p = item.Entry;
            bool primary = mapCanvas.PrimaryIndex == p.Index, secondary = mapCanvas.SecondaryIndex == p.Index;
            Color bg = (e.State & DrawItemState.Selected) != 0 ? Color.FromArgb(219, 234, 254) : Color.White;
            using (var back = new SolidBrush(bg))
            {
                g.FillRectangle(back, r);
            }
            using (var sw = new SolidBrush(p.Color))
            {
                g.FillRectangle(sw, r.X + 14, r.Y + 3, 26, r.Height - 6);
            }
            g.DrawRectangle(Pens.Gray, r.X + 14, r.Y + 3, 26, r.Height - 6);
            string mark = primary ? "1" : secondary ? "2" : string.Empty;
            if (mark.Length > 0)
            {
                TextRenderer.DrawText(g, mark, font, new Rectangle(r.X + 14, r.Y, 26, r.Height), p.Color.GetBrightness() > 0.55f ? Color.Black : Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            TextRenderer.DrawText(g, $"{p.HexIndex}  {p.Name}", font, new Rectangle(r.X + 46, r.Y, r.Width - 100, r.Height), Color.FromArgb(30, 41, 59),
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, $"Z {p.Altitude}", font, new Rectangle(r.Right - 52, r.Y, 48, r.Height), Color.FromArgb(100, 116, 139),
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void CatList_MouseDown(object sender, MouseEventArgs e)
        {
            int i = _catList.IndexFromPoint(e.Location);
            if (i < 0 || !(_catList.Items[i] is CatalogItem item) || item.Entry == null)
            {
                return;
            }
            _catList.SelectedIndex = i;
            SetBrushColor(item.Entry.Index, e.Button != MouseButtons.Right);
            SelectColor(item.Entry.Index);
            ShowEditorInfo(item.Entry.Index, true);
            if (mapCanvas.Tool == MapTool.Picker || mapCanvas.Tool == MapTool.Select || mapCanvas.Tool == MapTool.Pan)
            {
                SelectTool(MapTool.Brush);
            }
            _catList.Invalidate();
        }

        /// <summary>Picked colour: show it in the catalogue (category "Alle" if it is filtered out).</summary>
        private void SelectInCatalog(int index)
        {
            if (_catList == null)
            {
                return;
            }
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < _catList.Items.Count; i++)
                {
                    if (_catList.Items[i] is CatalogItem ci && ci.Entry != null && ci.Entry.Index == index)
                    {
                        _catList.SelectedIndex = i;
                        _catList.TopIndex = Math.Max(0, i - 3);
                        _catList.Invalidate();
                        return;
                    }
                }
                if (pass == 0)
                {
                    _catSearch.Text = string.Empty;
                    _catCategory.SelectedIndex = 0;
                }
            }
        }

        // ================================================================== colour stamps

        private void LoadColorStamps(string select = null)
        {
            if (_stampList == null)
            {
                return;
            }
            if (!_stampsMigrated)
            {
                _stampsMigrated = true;
                MigrateColorStamps();
            }
            var errors = new List<string>();
            _stamps = ColorStampTools.LoadAll(ColorStampFolder, errors);
            errors.ForEach(err => Log("Farbstempel nicht lesbar: " + err));
            _stampList.BeginUpdate();
            _stampList.Items.Clear();
            _stampList.Items.AddRange(_stamps.Cast<object>().ToArray());
            _stampList.EndUpdate();
            int sel = select == null ? -1 : _stamps.FindIndex(s => string.Equals(s.File, select, StringComparison.OrdinalIgnoreCase));
            if (sel >= 0)
            {
                _stampList.SelectedIndex = sel;
            }
            if (_stampBrowser != null)
            {
                _stampBrowser.Folder = ColorStampFolder;
                _stampBrowser.SetStamps(_stamps);
            }
        }

        private void SaveSelectionAsStamp()
        {
            IndexedMap cut = mapCanvas.CopySelection();
            if (cut == null)
            {
                SetStatus(L.T("Erst mit 'Auswahl' (S) einen Bereich markieren – z. B. einen Hausboden, eine Kreuzung oder einen Höhleneingang."));
                return;
            }
            IEnumerable<string> cats = _stamps.Select(s => s.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Concat(ColorStampTools.Categories.Skip(1));
            using var dlg = new Dialogs.CgiStampDialog("Auswahl → Farb-Stempel",
                L.T("Die Auswahl wird zusätzlich im Speicher gebaut: der Stempel bekommt die Land-Tiles mit Höhen (damit ist er auch ein Pinsel in der CGI-Ansicht und man kann Regeln daraus lernen). " +
                    "Statics: die der Statics-Ebene in der Auswahl."), cats, true, $"Stempel {cut.Width}x{cut.Height}");
            if (dlg.ShowDialog(this) != DialogResult.OK || dlg.StampName.Length == 0)
            {
                return;
            }
            ColorStamp stamp = ColorStampTools.FromMap(cut, _rules, _settings.Generator.WaterGroup, dlg.StampName, dlg.Category);
            stamp.DrawMode = dlg.DrawMode;
            stamp.Layers = dlg.Statics == StaticsMode.LandOnly ? StampLayers.LandOnly : StampLayers.Both;
            Rectangle sel = mapCanvas.Selection;
            // build the selection (+ 8 tiles border) to know the real land tiles and heights
            int x0 = Math.Max(0, (sel.Left - 8) / 8 * 8), y0 = Math.Max(0, (sel.Top - 8) / 8 * 8);
            int x1 = Math.Min(_map.Width, (sel.Right + 8 + 7) / 8 * 8), y1 = Math.Min(_map.Height, (sel.Bottom + 8 + 7) / 8 * 8);
            var r8 = Rectangle.FromLTRB(x0, y0, x1, y1);
            IndexedMap crop = _map.Crop(r8);
            BuildOptions o = CurrentBuildOptions();
            o.ExtraStatics = _staticsLayer.Crop(r8);
            o.ExtraHeights = _staticsLayer.CropHeights(r8);
            o.ExtraStaticsComplete = _staticsLayer.Complete;
            DragonRuleSet rules = _rules;
            List<LayerStatic> layer = _staticsLayer.Items.Where(st => sel.Contains(st.X, st.Y)).ToList();
            RunBusy("Stempel bauen", (progress, token) => new DragonCompiler(rules, o, s => { }).BuildInMemory(crop, progress, token), built =>
            {
                var inner = new Rectangle(sel.X - r8.X, sel.Y - r8.Y, sel.Width, sel.Height);
                (stamp.LandTiles, stamp.LandZ) = CgiToColor.Cut(built, inner);
                if (dlg.Statics != StaticsMode.LandOnly)
                {
                    stamp.Statics = layer.Select(st => new LayerStatic
                    {
                        X = st.X - sel.X,
                        Y = st.Y - sel.Y,
                        Id = st.Id,
                        Hue = st.Hue,
                        Dz = st.Absolute ? st.Dz - built.LandZ(st.X - r8.X, st.Y - r8.Y) : st.Dz
                    }).ToList();
                }
                string file = ColorStampTools.Save(stamp, ColorStampFolder);
                LoadColorStamps(file);
                SetStatus($"Stempel gespeichert: {file} (Boden-Höhe {stamp.BaseAltitude}, {stamp.Statics.Count} Statics, mit Land-Tiles – auch als CGI-Pinsel nutzbar).");
            });
        }

        private void PlaceSelectedStamp()
        {
            if (_stampList?.SelectedItem is not ColorStamp s)
            {
                SetStatus(L.T("Erst einen Stempel in der Liste wählen."));
                return;
            }
            UseStamp(s, _stampAdapt.Checked, _stampRepeat?.Checked ?? false, false, false);
        }

        /// <summary>Stamp / paste leaves the mouse.</summary>
        private void CancelStamp()
        {
            if (mapCanvas.PasteActive)
            {
                mapCanvas.EndPaste();
                SetStatus(L.T("Stempeln beendet."));
            }
            _placingStamp = null;
        }

        /// <summary>The stamp hangs at the mouse; every click writes it (heights adapted to the ground there), statics into the layer.</summary>
        private void PlaceStamp(ColorStamp s, bool adapt, bool repeat)
        {
            if (_map == null)
            {
                SetStatus(L.T("Im Karten-Editor ist keine Karte geöffnet."));
                return;
            }
            tabMain.SelectedTab = tabEditor;
            var raw = new IndexedMap(s.Width, s.Height, (ushort[])s.Pixels.Clone(), _map?.Bits ?? 8);
            mapCanvas.BeginPaste(raw);
            _placingStamp = s;
            mapCanvas.PasteName = "Stempel " + s.Name;
            mapCanvas.PasteRepeat = repeat;
            mapCanvas.PasteTransform = (buffer, p) =>
            {
                if (!adapt || _map == null)
                {
                    return buffer;
                }
                // every pixel: the colour of its group at the wanted height (heights the colours do not have -> height layer)
                var outMap = new IndexedMap(s.Width, s.Height, 0, _map.Bits);
                for (int y = 0; y < s.Height; y++)
                {
                    for (int x = 0; x < s.Width; x++)
                    {
                        int i = y * s.Width + x;
                        int mx = p.X + x, my = p.Y + y;
                        ushort current = _map.InBounds(mx, my) ? _map[mx, my] : (ushort)0;
                        int colour = StampPixelForHeight(s, i, current, out int want);
                        outMap.Pixels[i] = (ushort)colour;
                        if (s.Pixels[i] != ColorStampTools.Transparent && _map.InBounds(mx, my))
                        {
                            NoteHeight(mx, my, colour, want);
                        }
                    }
                }
                return outMap;
            };
            mapCanvas.Focus();
            SetStatus($"Stempel '{s.Name}': Klick setzt ihn {(repeat ? "(mehrmals – rechte Taste / Esc / Abbrechen = fertig)" : "(einmal)")}{(adapt ? ", Höhen passen sich dem Boden an" : string.Empty)}. Danach im Karten-Editor 'CGI aktualisieren'.");
        }

        private void DeleteSelectedStamp()
        {
            if (_stampList?.SelectedItem is ColorStamp s)
            {
                DeleteStamp(s);
            }
        }

        private void DeleteStamp(ColorStamp s)
        {
            if (MessageBox.Show(this, $"Stempel '{s.Name}' löschen?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                File.Delete(s.File);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            LoadColorStamps();
        }

        // ================================================================== stamp menu (window on another monitor or panel on the right)

        private StampBrowser _stampBrowser;
        private Form _stampWindow;
        private Panel _stampDock;

        private void EnsureStampBrowser()
        {
            if (_stampBrowser != null)
            {
                return;
            }
            _stampBrowser = new StampBrowser { Dock = DockStyle.Fill, Thumbnail = StampThumbnail };
            _stampBrowser.PlaceRequested += (s, st) => UseStamp(st, _stampBrowser.AdaptHeights, _stampBrowser.PlaceRepeated, _stampBrowser.StepUndo, _stampBrowser.LivePreview);
            _stampBrowser.StampChanged += (s, st) =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(st.File))
                    {
                        System.IO.File.WriteAllText(st.File, System.Text.Json.JsonSerializer.Serialize(st, new System.Text.Json.JsonSerializerOptions { IncludeFields = true }));
                    }
                }
                catch (Exception ex)
                {
                    Log("Stempel speichern: " + ex.Message);
                }
            };
            _stampBrowser.CancelRequested += (s, e) => CancelStamp();
            _stampBrowser.SetHeightOptions(_settings.PatternHeight, _settings.PatternHeightValue, _settings.PatternHeightOffset, _settings.PatternAutoFix);
            _stampBrowser.HeightOptionsChanged += (s, e) =>
            {
                _settings.PatternHeight = _stampBrowser.HeightMode;
                _settings.PatternHeightValue = _stampBrowser.HeightValue;
                _settings.PatternHeightOffset = _stampBrowser.HeightOffset;
                _settings.PatternAutoFix = _stampBrowser.AutoFix;
            };
            _stampBrowser.LearnRequested += (s, st) => LearnRulesFromStamp(st);
            _stampBrowser.ChooseFolderRequested += (s, e) => ChooseColorStampFolder();
            _stampBrowser.ImportRequested += (s, e) => ImportColorStamps();
            _stampBrowser.DeleteRequested += (s, st) => DeleteStamp(st);
            _stampBrowser.FolderRequested += (s, e) =>
            {
                Directory.CreateDirectory(ColorStampFolder);
                OpenFolder(ColorStampFolder);
            };
            _stampBrowser.ReloadRequested += (s, e) => LoadColorStamps();
            _stampBrowser.DockToggleRequested += (s, e) =>
            {
                if (_stampWindow != null)
                {
                    _stampWindow.Close();
                }
                else
                {
                    ShowStampWindow();
                }
            };
            _stampBrowser.CloseRequested += (s, e) =>
            {
                if (_stampWindow != null)
                {
                    _stampWindow.Close();
                }
                if (_stampDock != null)
                {
                    _stampDock.Visible = false;
                }
            };
            _stampDock = new Panel { Dock = DockStyle.Right, Width = 270, Visible = false };
            splitEditor.Panel2.Controls.Add(_stampDock);
            _stampBrowser.Folder = ColorStampFolder;
            _stampBrowser.SetStamps(_stamps);
        }

        /// <summary>Button "Stempel-Menü": with several monitors its own window, otherwise the panel right of the map.</summary>
        private void ToggleStampMenu()
        {
            EnsureStampBrowser();
            if (_stampWindow != null)
            {
                _stampWindow.Activate();
                return;
            }
            if (_stampDock.Visible)
            {
                _stampDock.Visible = false;
                return;
            }
            if (Screen.AllScreens.Length > 1)
            {
                ShowStampWindow();
            }
            else
            {
                DockStampBrowser();
            }
        }

        private void DockStampBrowser()
        {
            _stampDock.Controls.Add(_stampBrowser);
            _stampBrowser.Undocked = false;
            _stampDock.Visible = true;
        }

        private void ShowStampWindow()
        {
            _stampDock.Visible = false;
            var f = new Form
            {
                Text = L.T("Stempel – Dragon Map Studio"),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(320, 720),
                Owner = this
            };
            // on a second monitor if there is one, otherwise next to the main window
            Screen other = Screen.AllScreens.FirstOrDefault(sc => !sc.Bounds.Contains(Bounds.Location)) ?? Screen.FromControl(this);
            f.Location = other.Bounds.Contains(Bounds.Location)
                ? new Point(Math.Max(other.WorkingArea.Left, Right - f.Width - 20), Top + 80)
                : new Point(other.WorkingArea.Left + 40, other.WorkingArea.Top + 40);
            f.Controls.Add(_stampBrowser);
            _stampBrowser.Undocked = true;
            f.FormClosed += (s, e) =>
            {
                _stampWindow = null;
                // closing the window = the menu comes back as panel on the right
                if (!IsDisposed)
                {
                    DockStampBrowser();
                }
            };
            _stampWindow = f;
            f.Show(this);
        }

        /// <summary>72×72 picture of a stamp: its colours, statics as orange dots.</summary>
        private Image StampThumbnail(ColorStamp s)
        {
            Color[] colors = _rules.GetColors();
            var bmp = new Bitmap(72, 72);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(24, 24, 27));
            float k = Math.Min(70f / s.Width, 70f / s.Height);
            float ox = (72 - s.Width * k) / 2, oy = (72 - s.Height * k) / 2;
            var brushes = new Dictionary<ushort, SolidBrush>();
            try
            {
                for (int y = 0; y < s.Height; y++)
                {
                    for (int x = 0; x < s.Width; x++)
                    {
                        ushort c = s.Pixels[y * s.Width + x];
                        if (c == ColorStampTools.Transparent)
                        {
                            continue;
                        }
                        if (!brushes.TryGetValue(c, out SolidBrush b))
                        {
                            brushes[c] = b = new SolidBrush(c < colors.Length ? colors[c] : Color.Magenta);
                        }
                        g.FillRectangle(b, ox + x * k, oy + y * k, Math.Max(1, k), Math.Max(1, k));
                    }
                }
            }
            finally
            {
                foreach (SolidBrush b in brushes.Values)
                {
                    b.Dispose();
                }
            }
            if (s.Statics != null)
            {
                using var dot = new SolidBrush(Color.FromArgb(230, 255, 140, 0));
                foreach (LayerStatic st in s.Statics.Take(4000))
                {
                    g.FillRectangle(dot, ox + st.X * k, oy + st.Y * k, Math.Max(1, k * 0.6f), Math.Max(1, k * 0.6f));
                }
            }
            return bmp;
        }

        // ================================================================== statics layer

        private bool _stampsMigrated;
        private CheckBox _stampRepeat;
        private StaticsLayer _staticsLayer = new StaticsLayer();
        private ColorStamp _placingStamp;
        private bool _pasteHooked;

        private void LoadStaticsLayer()
        {
            try
            {
                _staticsLayer = StaticsLayer.Load(StaticsLayer.PathFor(_map?.FilePath));
            }
            catch (Exception ex)
            {
                _staticsLayer = new StaticsLayer();
                Log("Statics-Ebene nicht lesbar: " + ex.Message);
            }
            mapCanvas.SetStaticMarkers(_staticsLayer.Items);
            _placingStamp = null;
            if (!_pasteHooked)
            {
                _pasteHooked = true;
                mapCanvas.PasteApplied += MapCanvas_PasteApplied;
            }
            if (_staticsLayer.Items.Count > 0)
            {
                Log($"Statics-Ebene: {_staticsLayer.Items.Count:N0} Statics ({StaticsLayer.PathFor(_map?.FilePath)}).");
            }
        }

        /// <summary>A colour stamp with statics was written: its statics go into the layer (same undo step), the old ones there are replaced.</summary>
        private void MapCanvas_PasteApplied(object sender, MapPointEventArgs e)
        {
            ColorStamp s = _placingStamp;
            if (s == null || !mapCanvas.PasteActive || s.Statics == null || s.Statics.Count == 0 || _map == null)
            {
                return;
            }
            var area = new Rectangle(e.X, e.Y, s.Width, s.Height);
            List<LayerStatic> removed = _staticsLayer.Items.Where(st => area.Contains(st.X, st.Y)).ToList();
            List<LayerStatic> added = s.Statics.Select(st => new LayerStatic { X = st.X + e.X, Y = st.Y + e.Y, Dz = st.Dz, Id = st.Id, Hue = st.Hue })
                                              .Where(st => _map.InBounds(st.X, st.Y)).ToList();
            void Apply(List<LayerStatic> remove, List<LayerStatic> add)
            {
                var set = new HashSet<(int, int, int, ushort, ushort)>(remove.Select(r => (r.X, r.Y, r.Dz, r.Id, r.Hue)));
                _staticsLayer.Items.RemoveAll(st => set.Contains((st.X, st.Y, st.Dz, st.Id, st.Hue)));
                _staticsLayer.Items.AddRange(add);
                _staticsLayer.Modified = true;
                _map.Modified = true;
                mapCanvas.SetStaticMarkers(_staticsLayer.Items);
            }
            Apply(removed, added);
            _history.AddExtra(() => Apply(added, removed), () => Apply(removed, added));
        }

        // ================================================================== CGI preview of a marked area

        /// <summary>
        /// Builds only the marked area (or the visible part, at most 512×512) in memory and shows it in a CGI window –
        /// the map itself is not changed. 8 tiles border so the transitions at the edge are right.
        /// </summary>
        private void ShowSelectionInCgi()
        {
            if (_map == null)
            {
                return;
            }
            Rectangle area = mapCanvas.Selection;
            if (area.IsEmpty)
            {
                RectangleF v = mapCanvas.ViewRect;
                var c = new Point((int)(v.X + v.Width / 2), (int)(v.Y + v.Height / 2));
                area = new Rectangle(c.X - 64, c.Y - 64, 128, 128);
            }
            if (area.Width > 512 || area.Height > 512)
            {
                area = new Rectangle(area.X + Math.Max(0, area.Width - 512) / 2, area.Y + Math.Max(0, area.Height - 512) / 2, Math.Min(512, area.Width), Math.Min(512, area.Height));
            }
            int x0 = Math.Max(0, (area.Left - 8) / 8 * 8), y0 = Math.Max(0, (area.Top - 8) / 8 * 8);
            int x1 = Math.Min(_map.Width, (area.Right + 8 + 7) / 8 * 8), y1 = Math.Min(_map.Height, (area.Bottom + 8 + 7) / 8 * 8);
            var r = Rectangle.FromLTRB(x0, y0, x1, y1);
            IndexedMap crop = _map.Crop(r);
            BuildOptions o = CurrentBuildOptions();
            o.ExtraStatics = _staticsLayer.Crop(r);
            o.ExtraHeights = _staticsLayer.CropHeights(r);
            o.ExtraStaticsComplete = _staticsLayer.Complete;
            DragonRuleSet rules = _rules;
            EnsureCaveRule(crop);
            Rectangle inner = new Rectangle(area.X - r.X, area.Y - r.Y, area.Width, area.Height);
            RunBusy("CGI-Vorschau", (progress, token) => new DragonCompiler(rules, o, s => { }).BuildInMemory(crop, progress, token), data =>
            {
                ShowPreviewWindow(data, r, inner);
            });
        }

        private void ShowPreviewWindow(MapData data, Rectangle source, Rectangle inner, bool activate = true)
        {
            if (_previewWindow == null || _previewWindow.IsDisposed)
            {
                _previewWindow = new Form
                {
                    FormBorderStyle = FormBorderStyle.SizableToolWindow,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.CenterParent,
                    Size = new Size(980, 760),
                    Owner = this
                };
                _previewView = new IsoMapView { Dock = DockStyle.Fill, ShowGrid = true, ShowHeights = false };
                _previewWindow.FormClosed += (s, e) => LiveWindowClosed();
                var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
                var grid = new ToolStripButton(L.T("Raster")) { CheckOnClick = true, Checked = true };
                grid.Click += (s, e) => { _previewView.ShowGrid = grid.Checked; _previewView.Refresh(true); };
                var labels = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
                labels.Items.AddRange(new object[] { L.T("keine Infos"), L.T("Höhe"), L.T("Land-ID"), L.T("ID + Höhe"), L.T("ID + Höhe + Textur") });
                labels.SelectedIndex = 0;
                labels.SelectedIndexChanged += (s, e) =>
                {
                    _previewView.ShowHeights = labels.SelectedIndex > 0;
                    _previewView.LabelMode = (IsoLabel)Math.Max(0, labels.SelectedIndex - 1);
                    _previewView.Refresh(true);
                };
                var stat = new ToolStripButton(L.T("Statics")) { CheckOnClick = true, Checked = true };
                stat.Click += (s, e) => { _previewView.ShowStatics = stat.Checked; _previewView.Refresh(true); };
                var statIds = new ToolStripButton(L.T("Static-IDs")) { CheckOnClick = true };
                statIds.ToolTipText = L.T("ID und Höhe jedes Statics einblenden");
                statIds.Click += (s, e) => { _previewView.ShowStaticIds = statIds.Checked; _previewView.Refresh(true); };
                var zin = new ToolStripButton("Zoom +");
                zin.Click += (s, e) => _previewView.ZoomStep(1);
                var zout = new ToolStripButton("Zoom −");
                zout.Click += (s, e) => _previewView.ZoomStep(-1);
                var again = new ToolStripButton(L.T("Neu bauen (Auswahl)"));
                again.Click += (s, e) => ShowSelectionInCgi();
                bar.Items.AddRange(new ToolStripItem[] { again, new ToolStripSeparator(), grid, labels, stat, statIds, new ToolStripSeparator(), zin, zout });
                _previewWindow.Controls.Add(_previewView);
                _previewWindow.Controls.Add(bar);
                _previewWindow.Show(this);
                if (!activate)
                {
                    // live preview while drawing: the map editor keeps the mouse / keyboard
                    Activate();
                    mapCanvas.Focus();
                }
            }
            _previewView.SetMap(data);
            _previewView.CenterOn(inner.X + inner.Width / 2.0, inner.Y + inner.Height / 2.0);
            _previewWindow.Text = $"{L.T("CGI-Vorschau")} – x {source.X + inner.X}…{source.X + inner.Right - 1}, y {source.Y + inner.Y}…{source.Y + inner.Bottom - 1} " +
                                  L.T("(nur dieser Bereich gebaut, die Karte bleibt unverändert)");
            if (activate)
            {
                _previewWindow.Activate();
                _previewView.Focus();
            }
            else
            {
                _previewWindow.Text = L.T("Live-CGI beim Zeichnen") + $" – x {source.X + inner.X}, y {source.Y + inner.Y}";
            }
        }

        // ================================================================== context menu

        private void MapCanvas_ContextRequested(object sender, MapClickEventArgs e)
        {
            _editorMenu?.Dispose();
            _editorMenu = new ContextMenuStrip();
            int index = _map != null && _map.InBounds(e.X, e.Y) ? _map[e.X, e.Y] : -1;
            if (index >= 0 && index < _rules.Palette.Count)
            {
                DragonPaletteEntry p = _rules.Palette[index];
                var head = new ToolStripMenuItem($"{p.HexIndex}  {p.Name}  (Z {p.Altitude})") { Enabled = false };
                using (var bmp = new Bitmap(16, 16))
                {
                    using Graphics g = Graphics.FromImage(bmp);
                    g.Clear(p.Color);
                    head.Image = (Image)bmp.Clone();
                }
                _editorMenu.Items.Add(head);
                _editorMenu.Items.Add(L.T("Farbe hier als Farbe 1"), null, (s, a) => { SetBrushColor(index, true); SelectColor(index); SelectInCatalog(index); ShowEditorInfo(index, true); });
                _editorMenu.Items.Add(L.T("Farbe hier als Farbe 2"), null, (s, a) => { SetBrushColor(index, false); SelectColor(index); });
                _editorMenu.Items.Add(L.T("In der Farbtabelle zeigen"), null, (s, a) => { SelectColor(index); tabMain.SelectedTab = tabColors; });
                _editorMenu.Items.Add(new ToolStripSeparator());
            }
            _editorMenu.Items.Add(mnuUndo.Text.Replace("&", string.Empty), null, (s, a) => MnuUndo_Click(s, a)).Enabled = _history.CanUndo;
            _editorMenu.Items.Add(mnuRedo.Text.Replace("&", string.Empty), null, (s, a) => MnuRedo_Click(s, a)).Enabled = _history.CanRedo;
            _editorMenu.Items.Add(L.T("Ganzen Strich rückgängig (Strg+Umschalt+Z)"), null, (s, a) => UndoWholeStroke()).Enabled = _history.CanUndo && _history.UndoGroup > 0;
            _editorMenu.Items.Add(new ToolStripSeparator());
            bool sel = !mapCanvas.Selection.IsEmpty;
            _editorMenu.Items.Add(L.T("Auswahl → CGI-Vorschau (V)"), null, (s, a) => ShowSelectionInCgi());
            _editorMenu.Items.Add(L.T("Auswahl → Farb-Stempel speichern"), null, (s, a) => SaveSelectionAsStamp()).Enabled = sel;
            _editorMenu.Items.Add(L.T("Kopieren (Strg+C)"), null, (s, a) => MnuCopy_Click(s, a)).Enabled = sel;
            _editorMenu.Items.Add(L.T("Ausschneiden (Strg+X)"), null, (s, a) => MnuCut_Click(s, a)).Enabled = sel;
            _editorMenu.Items.Add(L.T("Einfügen (Strg+V)"), null, (s, a) => MnuPaste_Click(s, a));
            _editorMenu.Items.Add(L.T("Auswahl mit Farbe 1 füllen (Strg+F)"), null, (s, a) => MnuFillSelection_Click(s, a)).Enabled = sel;
            _editorMenu.Items.Add(new ToolStripSeparator());
            var heights = new ToolStripMenuItem(L.T("Höhen-Ebene"));
            heights.DropDownItems.Add(new ToolStripMenuItem(L.T("Höhe behalten (Pinsel, Linie, Formen, Füllen)"), null, (s, a) => ToggleKeepHeight(!_settings.BrushKeepHeight)) { Checked = _settings.BrushKeepHeight });
            heights.DropDownItems.Add(new ToolStripMenuItem(L.T("Höhen-Ebene zeigen"), null, (s, a) => ToggleHeightLayerView(!_settings.ShowHeightLayer)) { Checked = _settings.ShowHeightLayer });
            heights.DropDownItems.Add(new ToolStripSeparator());
            heights.DropDownItems.Add(L.T("Eintrag hier löschen"), null, (s, a) => ClearHeightLayer(new Rectangle(e.X, e.Y, 1, 1)))
                   .Enabled = _staticsLayer.Heights.ContainsKey((e.X, e.Y));
            heights.DropDownItems.Add(L.T("In der Auswahl löschen"), null, (s, a) => ClearHeightLayer(mapCanvas.Selection)).Enabled = sel;
            heights.DropDownItems.Add(L.T("Ganze Karte löschen …"), null, (s, a) =>
            {
                if (_map != null && MessageBox.Show(this, L.T("Alle Einträge der Höhen-Ebene löschen? (Strg+Z geht)"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ClearHeightLayer(new Rectangle(0, 0, _map.Width, _map.Height));
                }
            }).Enabled = _staticsLayer.Heights.Count > 0;
            _editorMenu.Items.Add(heights);
            _editorMenu.Items.Add(BookmarkMenu(new Point(e.X, e.Y), true));
            var tools = new ToolStripMenuItem(L.T("Werkzeug"));
            foreach ((string text, MapTool tool) in new[]
                     {
                         ("Pinsel (B)", MapTool.Brush), ("Pipette (I)", MapTool.Picker), ("Füllen (F)", MapTool.Fill), ("Auswahl (S)", MapTool.Select),
                         ("Höhe +", MapTool.Raise), ("Höhe −", MapTool.Lower), ("Höhleneingang", MapTool.CaveStamp), ("Hand (H)", MapTool.Pan),
                         ("Halbkreis", MapTool.HalfCircle), ("Dreieck", MapTool.Triangle), ("Neigungs-Linie", MapTool.SlopeLine),
                         ("Natürlicher Lauf", MapTool.Course), ("Dungeon", MapTool.Dungeon)
                     })
            {
                tools.DropDownItems.Add(L.T(text), null, (s, a) => SelectTool(tool)).Enabled = mapCanvas.Tool != tool;
            }
            _editorMenu.Items.Add(tools);
            _editorMenu.Items.Add(L.T("CGI-Ansicht aktualisieren (sichtbarer Teil / Auswahl)"), null, (s, a) => RefreshCgiFromEditor());
            _editorMenu.Items.Add(L.T("Hier in der CGI-Ansicht öffnen (ganze Karte neu bauen)"), null, (s, a) => BuildCgiFromEditor(e.X, e.Y));
            if (mapCanvas.PasteActive)
            {
                _editorMenu.Items.Insert(0, new ToolStripMenuItem(L.T("Stempeln / Einfügen beenden"), null, (s, a) => CancelStamp()));
            }
            _editorMenu.Items.Add(new ToolStripSeparator());
            _editorMenu.Items.Add(L.T("Hilfe (F1)"), null, (s, a) => ShowHelp("editor"));
            _editorMenu.Show(mapCanvas, mapCanvas.PointToClient(Control.MousePosition));
        }
    }
}
