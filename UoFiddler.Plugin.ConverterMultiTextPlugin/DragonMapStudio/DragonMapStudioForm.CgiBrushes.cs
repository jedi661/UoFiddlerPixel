/***************************************************************************
 *
 *  * Dragon Map Studio – CGI view: tab "Pinsel" and everything around the new brushes (Pinsel 21)
 *  *
 *  *  - scatter sets (the Dragon item files + own sets from the selection), wall sets (IDs from the catalogue)
 *  *  - settings of river, road, ramp, plateau, noise, magic wand, brush hardness
 *  *  - view filter (trees, walls, roofs, water, floors, other), bookmarks, history window
 *  *  - options bar items, context menu entries, search & replace
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private TabPage _tabCgiBrushes;
        private ComboBox _cmbScatterSet;
        private ComboBox _cmbWallSet;
        private PropertyGrid _propWallSet;
        private ListBox _lstBookmarks;
        private ToolStripComboBox _tscScatter;
        private ToolStripComboBox _tscWall;
        private readonly List<(ToolStripItem item, Func<IsoTool, bool> show)> _cgiToolItems = new List<(ToolStripItem, Func<IsoTool, bool>)>();
        private Form _historyWindow;
        private ListBox _historyList;
        private int _namedSteps;
        private MapData _namedMap;

        private CgiBrushSettings BrushCfg => _settings.CgiBrushes ??= new CgiBrushSettings();

        /// <summary>Tab page of the new brushes (created before the category buttons are built).</summary>
        private TabPage EnsureBrushTab()
        {
            if (_tabCgiBrushes == null)
            {
                _tabCgiBrushes = new TabPage(L.T("Pinsel")) { Name = "tabCgiBrushes", UseVisualStyleBackColor = true };
                tabCgiSide.TabPages.Add(_tabCgiBrushes);
            }
            return _tabCgiBrushes;
        }

        private void InitCgiBrushes()
        {
            BrushCfg.Normalize();
            isoView.BrushSettings = BrushCfg;
            isoView.HiddenKinds = BrushCfg.HiddenKinds;
            isoView.LandFamily = LandFamilyOf;
            isoView.ReplaceRequested += (s, e) => ShowReplaceDialog();
            isoView.Message += (s, text) =>
            {
                // the newest undo step gets the text of its message (history list)
                if (_cgiMap != null)
                {
                    if (!ReferenceEquals(_namedMap, _cgiMap))
                    {
                        _namedMap = _cgiMap;
                        _namedSteps = 0;
                    }
                    if (_cgiMap.UndoCount > _namedSteps)
                    {
                        _cgiMap.NameLastStep(text);
                    }
                    _namedSteps = _cgiMap.UndoCount;
                }
                RefreshHistory();
            };
            isoView.MapChanged += (s, e) => RefreshHistory();
            BuildBrushTab();
            ApplyScatterSet();
            ApplyWallSet();
        }

        // ================================================================== land families (colour table)

        private Dictionary<ushort, ushort[]> _families;
        private DragonRuleSet _familiesOf;

        /// <summary>Tiles of the colour (palette entry) a land tile belongs to.</summary>
        private ushort[] LandFamilyOf(ushort id)
        {
            if (_families == null || !ReferenceEquals(_familiesOf, _rules))
            {
                _familiesOf = _rules;
                _families = new Dictionary<ushort, ushort[]>();
                foreach (DragonPaletteEntry e in _rules.Palette.Where(p => p.Defined))
                {
                    ushort[] tiles = e.GetTileIds();
                    if (tiles.Length < 2)
                    {
                        continue;
                    }
                    foreach (ushort t in tiles)
                    {
                        if (!_families.TryGetValue(t, out ushort[] have) || have.Length < tiles.Length)
                        {
                            _families[t] = tiles.Distinct().ToArray();
                        }
                    }
                }
            }
            return _families.TryGetValue(id, out ushort[] f) ? f : null;
        }

        // ================================================================== scatter / wall sets

        private List<ScatterSet> AllScatterSets()
        {
            var list = _rules.ItemFiles.Where(f => f.Groups.Any(g => g.Parts.Count > 0)).Select(ScatterSet.FromItemFile).ToList();
            list.AddRange(BrushCfg.OwnSets);
            return list;
        }

        private void FillScatterCombos()
        {
            List<ScatterSet> sets = AllScatterSets();
            foreach (object combo in new object[] { _cmbScatterSet, _tscScatter })
            {
                ComboBox.ObjectCollection items = combo is ToolStripComboBox ts ? ts.Items : (combo as ComboBox)?.Items;
                if (items == null)
                {
                    continue;
                }
                items.Clear();
                foreach (ScatterSet s in sets)
                {
                    items.Add(s);
                }
            }
            SelectScatterInCombos();
        }

        private bool _syncing;

        private void SelectScatterInCombos()
        {
            _syncing = true;
            try
            {
                foreach (object combo in new object[] { _cmbScatterSet, _tscScatter })
                {
                    ComboBox.ObjectCollection items = combo is ToolStripComboBox ts ? ts.Items : (combo as ComboBox)?.Items;
                    if (items == null)
                    {
                        continue;
                    }
                    int index = -1;
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] is ScatterSet s && s.Name == BrushCfg.ScatterSet)
                        {
                            index = i;
                        }
                    }
                    if (combo is ToolStripComboBox t2)
                    {
                        t2.SelectedIndex = index;
                    }
                    else
                    {
                        ((ComboBox)combo).SelectedIndex = index;
                    }
                }
            }
            finally
            {
                _syncing = false;
            }
        }

        private void ApplyScatterSet()
        {
            isoView.ScatterSet = AllScatterSets().FirstOrDefault(s => s.Name == BrushCfg.ScatterSet);
        }

        private void ScatterChosen(object item)
        {
            if (_syncing || item is not ScatterSet s)
            {
                return;
            }
            BrushCfg.ScatterSet = s.Name;
            ApplyScatterSet();
            SelectScatterInCombos();
            SetStatus(L.T("Streu-Set") + $": {s.Name} – {s.Groups.Count} " + L.T("Objekte"));
        }

        /// <summary>Own scatter set from the selected statics: as one object, or every static on its own.</summary>
        private void ScatterSetFromSelection(bool asOneObject)
        {
            List<StaticItem> sel = isoView.Selection.ToList();
            if (sel.Count == 0 || _cgiMap == null)
            {
                SetStatus(L.T("Erst Statics auswählen (Werkzeug Auswahl, Rahmen ziehen)."));
                return;
            }
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name des Streu-Sets:"), Text, asOneObject ? L.T("Mein Objekt") : L.T("Meine Pflanzen"));
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            var set = new ScatterSet { Name = name.Trim() };
            if (asOneObject)
            {
                int minX = sel.Min(s => s.X), minY = sel.Min(s => s.Y);
                StaticItem anchor = sel.OrderBy(s => s.X + s.Y).First();
                int baseZ = _cgiMap.LandZ(anchor.X, anchor.Y);
                set.Groups.Add(new ItemGroupDef
                {
                    Frequency = 1,
                    Comment = name,
                    Parts = sel.Select(s => new ItemPart { Id = s.Id, Hue = s.Hue, Z = s.Z - baseZ, Dx = s.X - anchor.X, Dy = s.Y - anchor.Y }).ToList()
                });
            }
            else
            {
                foreach (IGrouping<(ushort, ushort), StaticItem> g in sel.GroupBy(s => (s.Id, s.Hue)))
                {
                    StaticItem s = g.First();
                    set.Groups.Add(new ItemGroupDef
                    {
                        Frequency = g.Count(),
                        Comment = UoArtCache.ItemName(s.Id),
                        Parts = { new ItemPart { Id = s.Id, Hue = s.Hue, Z = s.Z - _cgiMap.LandZ(s.X, s.Y) } }
                    });
                }
            }
            BrushCfg.OwnSets.RemoveAll(o => o.Name == set.Name);
            BrushCfg.OwnSets.Add(set);
            BrushCfg.ScatterSet = set.Name;
            FillScatterCombos();
            ApplyScatterSet();
            SetStatus(L.T("Streu-Set gespeichert") + $": {set.Name} ({set.Groups.Count} " + L.T("Objekte") + ")");
        }

        private void FillWallCombos()
        {
            foreach (object combo in new object[] { _cmbWallSet, _tscWall })
            {
                ComboBox.ObjectCollection items = combo is ToolStripComboBox ts ? ts.Items : (combo as ComboBox)?.Items;
                if (items == null)
                {
                    continue;
                }
                items.Clear();
                foreach (WallSet w in BrushCfg.WallSets)
                {
                    items.Add(w);
                }
            }
            _syncing = true;
            try
            {
                int index = BrushCfg.WallSets.FindIndex(w => w.Name == BrushCfg.WallSet);
                if (_cmbWallSet != null)
                {
                    _cmbWallSet.SelectedIndex = index;
                }
                if (_tscWall != null)
                {
                    _tscWall.SelectedIndex = index;
                }
            }
            finally
            {
                _syncing = false;
            }
        }

        private void ApplyWallSet()
        {
            isoView.WallSet = BrushCfg.WallSets.FirstOrDefault(w => w.Name == BrushCfg.WallSet);
            if (_propWallSet != null)
            {
                _propWallSet.SelectedObject = isoView.WallSet;
            }
        }

        private void WallChosen(object item)
        {
            if (_syncing || item is not WallSet w)
            {
                return;
            }
            BrushCfg.WallSet = w.Name;
            ApplyWallSet();
            FillWallCombos();
        }

        private void NewWallSet()
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name des Wand-Sets (z. B. Steinmauer, Holzzaun):"), Text, L.T("Mauer"));
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            var w = new WallSet { Name = name.Trim() };
            BrushCfg.WallSets.RemoveAll(x => x.Name == w.Name);
            BrushCfg.WallSets.Add(w);
            BrushCfg.WallSet = w.Name;
            ApplyWallSet();
            FillWallCombos();
            SetStatus(L.T("Wand-Set angelegt – jetzt im Item-Katalog das Wandstück wählen und '← West-Ost', '← Nord-Süd', '← Pfosten' klicken."));
        }

        /// <summary>Puts the item chosen in the catalogue into a slot of the wall set (slot 0 = west – east, 1 = north – south, 2 = post).</summary>
        private void WallSlotFromItem(int slot)
        {
            WallSet w = isoView.WallSet;
            if (w == null)
            {
                NewWallSet();
                w = isoView.WallSet;
                if (w == null)
                {
                    return;
                }
            }
            string id = isoView.PlaceId.ToString("x4");
            string Add(string list) => string.IsNullOrWhiteSpace(list) ? id : list.Contains(id) ? list : list + " " + id;
            switch (slot)
            {
                case 0:
                    w.AlongX = Add(w.AlongX);
                    break;
                case 1:
                    w.AlongY = Add(w.AlongY);
                    break;
                default:
                    w.Post = id;
                    break;
            }
            _propWallSet?.Refresh();
            SetStatus($"{w.Name}: {id} {UoArtCache.ItemName(isoView.PlaceId)} → " + L.T(slot == 0 ? "West-Ost" : slot == 1 ? "Nord-Süd" : "Pfosten"));
        }

        // ================================================================== tab "Pinsel"

        private void BuildBrushTab()
        {
            FlowLayoutPanel col = SectionColumn(EnsureBrushTab());
            PropertyGrid Grid(object o, int height) => new PropertyGrid
            {
                SelectedObject = o, ToolbarVisible = false, HelpVisible = true, Height = height, PropertySort = PropertySort.Categorized
            };
            Button Btn(string text, Color c, Action click, bool primary = false)
            {
                var b = new Button { Text = L.T(text) };
                StyleButton(b, c, primary);
                b.Click += (s, e) => click();
                return b;
            }
            Button ToolBtn(string text, IsoTool tool, Color c) => Btn(text, c, () =>
            {
                SelectCgiTool(tool);
                isoView.Focus();
            }, true);

            // ---- scatter
            _cmbScatterSet = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbScatterSet.SelectedIndexChanged += (s, e) => ScatterChosen(_cmbScatterSet.SelectedItem);
            AddSection(col, "brush.scatter", "Streu-Pinsel (Bäume, Büsche, Steine …)", ColTerrain, () => ToolIcons.Get("scatter"),
                Row(ToolBtn("Streu-Pinsel wählen", IsoTool.Scatter, ColTerrain)),
                Row(_cmbScatterSet),
                Row(Btn("Set aus Auswahl: ein Objekt", ColBuild, () => ScatterSetFromSelection(true)), Btn("… jedes Static einzeln", ColBuild, () => ScatterSetFromSelection(false))),
                Row(Btn("Eigenes Set löschen", ColCheck, () =>
                {
                    if (BrushCfg.OwnSets.RemoveAll(o => o.Name == BrushCfg.ScatterSet) > 0)
                    {
                        FillScatterCombos();
                        ApplyScatterSet();
                    }
                    else
                    {
                        SetStatus(L.T("Die Sets aus den Dragon-Skripten lassen sich hier nicht löschen (Reiter Regeln → Items)."));
                    }
                }), Btn("Boden unter der Maus übernehmen", ColView, () =>
                {
                    Point h = isoView.HoverTile;
                    if (_cgiMap != null && _cgiMap.InBounds(h.X, h.Y))
                    {
                        string id = _cgiMap.LandId(h.X, h.Y).ToString("x4");
                        if (!BrushCfg.Scatter.OnlyOnTiles.Contains(id))
                        {
                            BrushCfg.Scatter.OnlyOnTiles = (BrushCfg.Scatter.OnlyOnTiles + " " + id).Trim();
                        }
                        col.Refresh();
                        SetStatus(L.T("Nur auf Land-Tiles") + ": " + BrushCfg.Scatter.OnlyOnTiles);
                    }
                })),
                Row(Grid(BrushCfg.Scatter, 230)),
                Row(Hint(L.T("Ziehen = streuen · Strg+Ziehen = Statics des Sets entfernen · Sets: alle Item-Dateien der Dragon-Skripte (forest, jungle, swamp …) und eigene aus der Auswahl."))));
            FillScatterCombos();

            // ---- wall
            _cmbWallSet = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbWallSet.SelectedIndexChanged += (s, e) => WallChosen(_cmbWallSet.SelectedItem);
            _propWallSet = Grid(null, 130);
            AddSection(col, "brush.wall", "Wand / Zaun", ColCave, () => ToolIcons.Get("wall"), true,
                Row(ToolBtn("Wand-Werkzeug wählen", IsoTool.Wall, ColCave)),
                Row(_cmbWallSet),
                Row(Btn("Neues Set …", ColBuild, NewWallSet), Btn("Set löschen", ColCheck, () =>
                {
                    BrushCfg.WallSets.RemoveAll(w => w.Name == BrushCfg.WallSet);
                    BrushCfg.WallSet = BrushCfg.WallSets.FirstOrDefault()?.Name ?? string.Empty;
                    ApplyWallSet();
                    FillWallCombos();
                })),
                Row(Btn("← West-Ost", ColView, () => WallSlotFromItem(0)), Btn("← Nord-Süd", ColView, () => WallSlotFromItem(1)), Btn("← Pfosten", ColView, () => WallSlotFromItem(2))),
                Row(_propWallSet),
                Row(Grid(BrushCfg.Wall, 170)),
                Row(Hint(L.T("Ein Set = die Wandstücke für beide Richtungen und ein Pfosten: im Reiter Items das Stück wählen und '← …' klicken (mehrere IDs = zufällig gemischt)."))));
            FillWallCombos();

            // ---- land brushes
            AddSection(col, "brush.river", "Fluss", ColBuild, () => ToolIcons.Get("river"), true,
                Row(ToolBtn("Fluss-Pinsel wählen", IsoTool.River, ColBuild)), Row(Grid(BrushCfg.River, 190)));
            AddSection(col, "brush.road", "Weg / Straße (Kurve)", ColCave, () => ToolIcons.Get("road"), true,
                Row(ToolBtn("Weg-Werkzeug wählen", IsoTool.Road, ColCave)), Row(Grid(BrushCfg.Road, 190)),
                Row(Hint(L.T("Punkte klicken · Doppelklick / Rechtsklick / Enter = bauen · Rücktaste = Punkt weg · Esc = abbrechen. Leere Weg-Tiles = Land-Tiles aus dem Reiter Land."))));
            AddSection(col, "brush.ramp", "Rampe", ColTerrain, () => ToolIcons.Get("ramp"), true,
                Row(ToolBtn("Rampe wählen", IsoTool.Ramp, ColTerrain)), Row(Grid(BrushCfg.Ramp, 120)));
            AddSection(col, "brush.plateau", "Plateau / Terrassen", ColTerrain, () => ToolIcons.Get("plateau"), true,
                Row(ToolBtn("Plateau-Pinsel wählen", IsoTool.Plateau, ColTerrain)), Row(Grid(BrushCfg.Plateau, 130)));
            AddSection(col, "brush.noise", "Unebenheit", ColTerrain, () => ToolIcons.Get("noise"), true,
                Row(ToolBtn("Unebenheit-Pinsel wählen", IsoTool.Noise, ColTerrain)), Row(Grid(BrushCfg.Noise, 120)));
            var hardness = new NumericUpDown { Minimum = 1, Maximum = 100, Value = Math.Clamp(BrushCfg.Hardness, 1, 100), Width = 70 };
            hardness.ValueChanged += (s, e) => BrushCfg.Hardness = (int)hardness.Value;
            AddSection(col, "brush.more", "Varianten, Übergänge, Härte", ColView, () => ToolIcons.Get("variants"), true,
                Row(ToolBtn("Varianten-Pinsel", IsoTool.Variants, ColView), ToolBtn("Übergänge-Pinsel", IsoTool.TransitionBrush, ColView)),
                Row(new Label { Text = L.T("Härte (%) für Höhe +/−, Glätten, Plateau, Unebenheit:"), AutoSize = true }, hardness),
                Row(Hint(L.T("Härte 100 = harter Rand wie bisher, kleiner = zur Mitte stärker, am Rand weich."))));

            // ---- selection
            AddSection(col, "brush.select", "Lasso, Zauberstab, Messen", ColCheck, () => ToolIcons.Get("magicwand"), true,
                Row(ToolBtn("Lasso", IsoTool.Lasso, ColCheck), ToolBtn("Zauberstab", IsoTool.MagicWand, ColCheck), ToolBtn("Messen", IsoTool.Measure, ColView)),
                Row(Grid(BrushCfg.Wand, 120)),
                Row(Btn("Lasso / Zauberstab aufheben (Esc)", ColView, () =>
                {
                    isoView.ClearArea();
                    SetStatus(L.T("Lasso / Zauberstab aufgehoben."));
                })),
                Row(Hint(L.T("Solange Lasso oder Zauberstab aktiv sind, wirken ALLE Werkzeuge nur darin. Bereich-Tasten: F K J W G M T · R drehen · X / Y spiegeln · E ersetzen · D auf den Boden."))));

            // ---- view filter
            var checks = new List<Control[]>();
            foreach ((StaticKind kind, string text) in new[]
                     {
                         (StaticKind.Plant, "Bäume / Pflanzen"), (StaticKind.Wall, "Wände / Türen / Fenster"), (StaticKind.Roof, "Dächer"),
                         (StaticKind.Water, "Wasser"), (StaticKind.Floor, "Böden"), (StaticKind.Other, "Sonstige")
                     })
            {
                StaticKind k = kind;
                var c = new CheckBox { Text = L.T(text), AutoSize = true, Checked = (BrushCfg.HiddenKinds & k) == 0 };
                c.CheckedChanged += (s, e) => SetKindVisible(k, c.Checked);
                checks.Add(Row(c));
            }
            checks.Add(Row(Hint(L.T("Ausgeblendete Statics werden nicht gezeichnet und lassen sich nicht anklicken – gut zum Arbeiten unter Bäumen oder in Häusern. 'Max Z' oben blendet Höhen aus."))));
            AddSection(col, "brush.filter", "Ansicht-Filter (Statics)", ColView, null, true, checks.ToArray());

            // ---- bookmarks + history
            _lstBookmarks = new ListBox { Height = 110, IntegralHeight = false };
            _lstBookmarks.DoubleClick += (s, e) => JumpToBookmark(_lstBookmarks.SelectedIndex, false);
            AddSection(col, "brush.bookmarks", "Lesezeichen", ColBuild, null, true,
                Row(_lstBookmarks),
                Row(Btn("★ Hier merken", ColBuild, () => AddBookmark(isoView.HoverTile.X >= 0 ? isoView.HoverTile : CgiViewCenter())), Btn("Hinspringen", ColView, () => JumpToBookmark(_lstBookmarks.SelectedIndex, false))),
                Row(Btn("Im Karten-Editor zeigen", ColView, () => JumpToBookmark(_lstBookmarks.SelectedIndex, true)), Btn("Löschen", ColCheck, () => DeleteBookmark(_lstBookmarks.SelectedIndex))));
            FillBookmarks();
            AddSection(col, "brush.history", "Verlauf", ColView, null, true,
                Row(Btn("Verlauf-Fenster öffnen", ColView, ShowHistory)),
                Row(Hint(L.T("Alle Schritte der CGI-Karte; ein Klick auf einen Schritt geht dorthin zurück (oder wieder vor)."))));
        }

        private Point CgiViewCenter()
        {
            RectangleF v = isoView.ViewRect;
            return new Point((int)(v.X + v.Width / 2), (int)(v.Y + v.Height / 2));
        }

        private void SetKindVisible(StaticKind k, bool visible)
        {
            BrushCfg.HiddenKinds = visible ? BrushCfg.HiddenKinds & ~k : BrushCfg.HiddenKinds | k;
            isoView.HiddenKinds = BrushCfg.HiddenKinds;
            isoView.Refresh(true);
        }

        // ================================================================== bookmarks

        private static (string name, int x, int y)? ParseBookmark(string s)
        {
            string[] t = (s ?? string.Empty).Split('|');
            if (t.Length == 3 && int.TryParse(t[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x) &&
                int.TryParse(t[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y))
            {
                return (t[0], x, y);
            }
            return null;
        }

        private void FillBookmarks()
        {
            if (_lstBookmarks == null)
            {
                return;
            }
            _lstBookmarks.Items.Clear();
            foreach (string b in BrushCfg.Bookmarks)
            {
                (string name, int x, int y)? p = ParseBookmark(b);
                if (p.HasValue)
                {
                    _lstBookmarks.Items.Add($"{p.Value.name}  ({p.Value.x}, {p.Value.y})");
                }
            }
        }

        private void AddBookmark(Point at)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name des Lesezeichens:"), Text, $"{at.X}, {at.Y}");
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            BrushCfg.Bookmarks.Add($"{name.Replace("|", "/").Trim()}|{at.X}|{at.Y}");
            FillBookmarks();
            SetStatus(L.T("Lesezeichen gesetzt") + $": {name} ({at.X}, {at.Y})");
        }

        private void DeleteBookmark(int index)
        {
            if (index >= 0 && index < BrushCfg.Bookmarks.Count)
            {
                BrushCfg.Bookmarks.RemoveAt(index);
                FillBookmarks();
            }
        }

        private void JumpToBookmark(int index, bool editor)
        {
            if (index < 0 || index >= BrushCfg.Bookmarks.Count)
            {
                return;
            }
            (string name, int x, int y)? p = ParseBookmark(BrushCfg.Bookmarks[index]);
            if (!p.HasValue)
            {
                return;
            }
            if (editor)
            {
                if (_map != null)
                {
                    tabMain.SelectedTab = tabEditor;
                    mapCanvas.CenterOn(p.Value.x + 0.5, p.Value.y + 0.5);
                }
                return;
            }
            if (_cgiMap != null)
            {
                tabMain.SelectedTab = tabCgi;
                isoView.JumpTo(p.Value.x, p.Value.y);
            }
            SetStatus(L.T("Lesezeichen") + $": {p.Value.name}");
        }

        /// <summary>Bookmark menu (CGI and map editor context menus).</summary>
        private ToolStripMenuItem BookmarkMenu(Point here, bool editor)
        {
            var m = new ToolStripMenuItem(L.T("Lesezeichen"));
            m.DropDownItems.Add(L.T("★ Hier merken …"), null, (s, e) => AddBookmark(here));
            if (BrushCfg.Bookmarks.Count > 0)
            {
                m.DropDownItems.Add(new ToolStripSeparator());
            }
            for (int i = 0; i < BrushCfg.Bookmarks.Count; i++)
            {
                int index = i;
                (string name, int x, int y)? p = ParseBookmark(BrushCfg.Bookmarks[i]);
                if (p.HasValue)
                {
                    m.DropDownItems.Add($"{p.Value.name}  ({p.Value.x}, {p.Value.y})", null, (s, e) => JumpToBookmark(index, editor));
                }
            }
            return m;
        }

        // ================================================================== history

        private void ShowHistory()
        {
            if (_historyWindow != null && !_historyWindow.IsDisposed)
            {
                _historyWindow.Activate();
                return;
            }
            _historyWindow = new Form
            {
                Text = L.T("Verlauf – CGI-Ansicht"),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(380, 480),
                Owner = this
            };
            Point p = isoView.PointToScreen(new Point(Math.Max(0, isoView.Width - 400), 20));
            _historyWindow.Location = p;
            _historyList = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false, Font = new Font("Segoe UI", 9f) };
            _historyList.Click += (s, e) => HistoryJump(_historyList.SelectedIndex);
            _historyWindow.Controls.Add(_historyList);
            _historyWindow.Controls.Add(new Label
            {
                Dock = DockStyle.Top, Height = 34, Padding = new Padding(4),
                Text = L.T("Klick auf einen Schritt = bis dorthin zurück (grau = wiederholen). Oben der neueste."),
                ForeColor = Color.FromArgb(71, 85, 105)
            });
            _historyWindow.FormClosed += (s, e) => _historyList = null;
            _historyWindow.Show(this);
            RefreshHistory();
        }

        private int _historyRedo;

        private void RefreshHistory()
        {
            if (_historyList == null || _historyList.IsDisposed)
            {
                return;
            }
            _historyList.BeginUpdate();
            _historyList.Items.Clear();
            if (_cgiMap != null)
            {
                List<string> redo = _cgiMap.RedoNames.ToList();
                _historyRedo = redo.Count;
                for (int i = redo.Count - 1; i >= 0; i--)
                {
                    _historyList.Items.Add("↷ " + redo[i]);
                }
                _historyList.Items.Add("● " + L.T("jetzt"));
                foreach (string u in _cgiMap.UndoNames)
                {
                    _historyList.Items.Add("   " + u);
                }
            }
            _historyList.EndUpdate();
        }

        private void HistoryJump(int index)
        {
            if (_cgiMap == null || index < 0)
            {
                return;
            }
            int now = _historyRedo; // index of "jetzt"
            if (index < now)
            {
                for (int k = 0; k < now - index; k++)
                {
                    isoView.Redo();
                }
            }
            else if (index > now)
            {
                for (int k = 0; k < index - now; k++)
                {
                    isoView.Undo();
                }
            }
            RefreshHistory();
            UpdateCgiState();
        }

        // ================================================================== replace

        private void ShowReplaceDialog()
        {
            if (_cgiMap == null)
            {
                SetStatus(L.T("Keine CGI-Karte geladen."));
                return;
            }
            Point h = isoView.HoverTile;
            ushort land = _cgiMap.InBounds(h.X, h.Y) ? _cgiMap.LandId(h.X, h.Y) : (ushort)0;
            ushort item = isoView.Selection.FirstOrDefault()?.Id ?? 0;
            using var dlg = new ReplaceDialog(!isoView.Area.IsEmpty, item, land);
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            if (dlg.FromStatic.Length == 0 && (dlg.FromLand.Length == 0 || dlg.ToLand.Length == 0))
            {
                SetStatus(L.T("Nichts zu suchen angegeben."));
                return;
            }
            isoView.ReplaceIds(dlg.WholeMap, dlg.FromStatic, dlg.ToStatic, dlg.OnlyHue, dlg.NewHue, dlg.FromLand, dlg.ToLand);
            UpdateCgiState();
        }

        // ================================================================== options bar + context menu

        /// <summary>Items of the options bar that only show with some tools.</summary>
        private void BuildBrushOptionItems()
        {
            ToolStripButton Cmd(string text, string tip, Action click, Func<IsoTool, bool> show)
            {
                var b = new ToolStripButton(L.T(text)) { ToolTipText = L.T(tip) };
                b.Click += (s, e) =>
                {
                    click();
                    isoView.Focus();
                };
                toolStripCgiLand.Items.Add(b);
                _cgiToolItems.Add((b, show));
                return b;
            }
            _tscScatter = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190, ToolTipText = L.T("Streu-Set") };
            _tscScatter.SelectedIndexChanged += (s, e) => ScatterChosen(_tscScatter.SelectedItem);
            toolStripCgiLand.Items.Add(_tscScatter);
            _cgiToolItems.Add((_tscScatter, t => t == IsoTool.Scatter));
            FillScatterCombos();
            Cmd("Set aus Auswahl", "Ausgewählte Statics als eigenes Streu-Set (ein Objekt)", () => ScatterSetFromSelection(true), t => t == IsoTool.Scatter);

            _tscWall = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150, ToolTipText = L.T("Wand-Set") };
            _tscWall.SelectedIndexChanged += (s, e) => WallChosen(_tscWall.SelectedItem);
            toolStripCgiLand.Items.Add(_tscWall);
            _cgiToolItems.Add((_tscWall, t => t == IsoTool.Wall));
            FillWallCombos();
            Cmd("Neues Wand-Set", "Wand-Set anlegen, dann Stücke aus dem Item-Katalog zuordnen", NewWallSet, t => t == IsoTool.Wall);
            Cmd("← West-Ost", "Gewähltes Item (Reiter Items) als Wandstück West-Ost", () => WallSlotFromItem(0), t => t == IsoTool.Wall);
            Cmd("← Nord-Süd", "Gewähltes Item als Wandstück Nord-Süd", () => WallSlotFromItem(1), t => t == IsoTool.Wall);
            Cmd("← Pfosten", "Gewähltes Item als Pfosten / Ecke", () => WallSlotFromItem(2), t => t == IsoTool.Wall);

            bool AreaTool(IsoTool t) => t == IsoTool.Area || t == IsoTool.Lasso || t == IsoTool.MagicWand;
            Cmd("Drehen (R)", "Bereich 90° im Uhrzeigersinn drehen (Land + Statics)", () => isoView.TransformArea(1, false, false), AreaTool);
            Cmd("↔ (X)", "Bereich links ↔ rechts spiegeln", () => isoView.TransformArea(0, true, false), AreaTool);
            Cmd("↕ (Y)", "Bereich oben ↕ unten spiegeln", () => isoView.TransformArea(0, false, true), AreaTool);
            Cmd("Ersetzen … (E)", "Statics / Land im Bereich suchen und ersetzen", ShowReplaceDialog, AreaTool);
            Cmd("Auf Boden (D)", "Statics im Bereich auf die Bodenhöhe setzen", () => isoView.DropAreaToGround(), AreaTool);
            Cmd("Aufheben (Esc)", "Lasso / Zauberstab aufheben", () => isoView.ClearArea(), t => t == IsoTool.Lasso || t == IsoTool.MagicWand);
            Cmd("Drehen (R)", "Ausgewählte Statics um ihre Mitte drehen (Positionen)", () => isoView.TransformSelection(1, false, false), t => t == IsoTool.Select);
            Cmd("Spiegeln (X)", "Ausgewählte Statics spiegeln", () => isoView.TransformSelection(0, true, false), t => t == IsoTool.Select);
            Cmd("Auf Boden (D)", "Ausgewählte Statics auf die Bodenhöhe setzen", () => isoView.DropSelectionToGround(), t => t == IsoTool.Select);
            Cmd("Verlauf", "Verlauf-Fenster: alle Schritte, Klick geht dorthin zurück", ShowHistory, t => true);
        }

        private void UpdateBrushOptionItems(IsoTool t)
        {
            foreach ((ToolStripItem item, Func<IsoTool, bool> show) in _cgiToolItems)
            {
                item.Visible = show(t);
            }
        }

        /// <summary>Entries of the CGI context menu for the new tools and edits.</summary>
        private void AddBrushMenu(ToolStripItemCollection items, MapPointEventArgs e)
        {
            bool has = _cgiMap != null;
            if (isoView.Selection.Count > 0)
            {
                var sel = new ToolStripMenuItem(L.T("Auswahl") + $" ({isoView.Selection.Count})");
                sel.DropDownItems.Add(L.T("Drehen 90° (R)"), null, (s, a) => isoView.TransformSelection(1, false, false));
                sel.DropDownItems.Add(L.T("Spiegeln links ↔ rechts (X)"), null, (s, a) => isoView.TransformSelection(0, true, false));
                sel.DropDownItems.Add(L.T("Spiegeln oben ↕ unten"), null, (s, a) => isoView.TransformSelection(0, false, true));
                sel.DropDownItems.Add(L.T("Auf den Boden setzen (D)"), null, (s, a) => isoView.DropSelectionToGround());
                sel.DropDownItems.Add(L.T("Färben (H)"), null, (s, a) => isoView.HueSelection(isoView.PlaceHue));
                sel.DropDownItems.Add(L.T("Als Streu-Set speichern (ein Objekt) …"), null, (s, a) => ScatterSetFromSelection(true));
                sel.DropDownItems.Add(L.T("Als Streu-Set speichern (einzeln) …"), null, (s, a) => ScatterSetFromSelection(false));
                sel.DropDownItems.Add(L.T("Löschen (Entf)"), null, (s, a) => isoView.DeleteSelection());
                items.Add(sel);
            }
            var edit = new ToolStripMenuItem(L.T("Bearbeiten")) { Enabled = has };
            bool area = has && !isoView.Area.IsEmpty;
            edit.DropDownItems.Add(L.T("Bereich drehen 90° (R)"), null, (s, a) => isoView.TransformArea(1, false, false)).Enabled = area;
            edit.DropDownItems.Add(L.T("Bereich spiegeln links ↔ rechts (X)"), null, (s, a) => isoView.TransformArea(0, true, false)).Enabled = area;
            edit.DropDownItems.Add(L.T("Bereich spiegeln oben ↕ unten (Y)"), null, (s, a) => isoView.TransformArea(0, false, true)).Enabled = area;
            edit.DropDownItems.Add(L.T("Statics im Bereich auf den Boden (D)"), null, (s, a) => isoView.DropAreaToGround()).Enabled = area;
            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(L.T("Suchen & Ersetzen … (E)"), null, (s, a) => ShowReplaceDialog());
            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(L.T("Lasso"), ToolIcons.Get("lasso"), (s, a) => SelectCgiTool(IsoTool.Lasso));
            edit.DropDownItems.Add(L.T("Zauberstab (hier klicken)"), ToolIcons.Get("magicwand"), (s, a) => SelectCgiTool(IsoTool.MagicWand));
            edit.DropDownItems.Add(L.T("Lasso / Zauberstab aufheben"), null, (s, a) => isoView.ClearArea()).Enabled = isoView.Mask != null;
            edit.DropDownItems.Add(L.T("Messen"), ToolIcons.Get("measure"), (s, a) => SelectCgiTool(IsoTool.Measure));
            items.Add(edit);

            var filter = new ToolStripMenuItem(L.T("Ansicht-Filter"));
            foreach ((StaticKind kind, string text) in new[]
                     {
                         (StaticKind.Plant, "Bäume / Pflanzen"), (StaticKind.Wall, "Wände / Türen / Fenster"), (StaticKind.Roof, "Dächer"),
                         (StaticKind.Water, "Wasser"), (StaticKind.Floor, "Böden"), (StaticKind.Other, "Sonstige")
                     })
            {
                StaticKind k = kind;
                filter.DropDownItems.Add(new ToolStripMenuItem(L.T(text), null, (s, a) =>
                {
                    SetKindVisible(k, (BrushCfg.HiddenKinds & k) != 0);
                    RebuildFilterChecks();
                }) { Checked = (BrushCfg.HiddenKinds & k) == 0 });
            }
            filter.DropDownItems.Add(new ToolStripSeparator());
            filter.DropDownItems.Add(L.T("Alle zeigen"), null, (s, a) =>
            {
                BrushCfg.HiddenKinds = StaticKind.None;
                isoView.HiddenKinds = StaticKind.None;
                isoView.Refresh(true);
                RebuildFilterChecks();
            });
            items.Add(filter);
            items.Add(BookmarkMenu(new Point(e.X, e.Y), false));
            items.Add(L.T("Verlauf …"), null, (s, a) => ShowHistory());
        }

        /// <summary>The check boxes of the tab follow a change from the menu.</summary>
        private void RebuildFilterChecks()
        {
            if (_tabCgiBrushes == null)
            {
                return;
            }
            foreach (CheckBox c in AllControls(_tabCgiBrushes).OfType<CheckBox>())
            {
                StaticKind k = c.Text == L.T("Bäume / Pflanzen") ? StaticKind.Plant : c.Text == L.T("Wände / Türen / Fenster") ? StaticKind.Wall :
                    c.Text == L.T("Dächer") ? StaticKind.Roof : c.Text == L.T("Wasser") ? StaticKind.Water : c.Text == L.T("Böden") ? StaticKind.Floor :
                    c.Text == L.T("Sonstige") ? StaticKind.Other : StaticKind.None;
                if (k != StaticKind.None)
                {
                    c.Checked = (BrushCfg.HiddenKinds & k) == 0;
                }
            }
        }

        private static IEnumerable<Control> AllControls(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (Control d in AllControls(c))
                {
                    yield return d;
                }
            }
        }
    }
}
