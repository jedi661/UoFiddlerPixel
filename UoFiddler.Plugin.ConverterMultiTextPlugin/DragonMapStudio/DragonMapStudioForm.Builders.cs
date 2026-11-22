/***************************************************************************
 *
 *  * Dragon Map Studio – shapes and builders (Bauen 22)
 *  *
 *  *  Map editor: half circle, triangle, slope line (height from A to B), natural course (brush or pattern stamp),
 *  *              dungeon generator in colours (colour 1 = floor, colour 2 = walls)
 *  *  CGI view:   bridge, cellar under the map, dungeon generator (cave / built / under the map), river course,
 *  *              "Unter der Karte" view with building level
 *  *  Sets:       bridge sets and stair sets (IDs from the item catalogue), wall sets from "Pinsel"
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private ComboBox _cmbBridgeSet;
        private ComboBox _cmbDungeonLook;
        private PropertyGrid _propDungeonLook;
        private PropertyGrid _propBridgeSet;
        private ComboBox _cmbStairSet;
        private PropertyGrid _propStairSet;
        private CheckBox _chkUnderView;
        private ToolStripButton _tsbUnderView;
        private ToolStripButton _edShapeFilled;
        private ToolStripButton _edCourseStamp;
        private readonly List<(ToolStripItem item, Func<MapTool, bool> show)> _edToolItems = new List<(ToolStripItem, Func<MapTool, bool>)>();
        private bool _buildersHooked;

        private void InitBuilders()
        {
            if (_buildersHooked)
            {
                return;
            }
            _buildersHooked = true;
            isoView.FindWallSet = name => BrushCfg.WallSets.FirstOrDefault(w => w.Name == name);
            isoView.FindDungeonLook = name => BrushCfg.DungeonLooks.FirstOrDefault(l => l.Name == name);
            isoView.BridgeSet = BrushCfg.BridgeSets.FirstOrDefault(b => b.Name == BrushCfg.BridgeSet);
            isoView.StairSet = BrushCfg.StairSets.FirstOrDefault(b => b.Name == BrushCfg.StairSet);
            isoView.FindBridgeTemplate = FindBridgePlan;
            mapCanvas.Course = BrushCfg.Course;
            mapCanvas.SlopeColour = SlopeColourFor;
            mapCanvas.StepClosing += (s, e) => _slope = null;
            mapCanvas.DungeonRequested += MapCanvas_DungeonRequested;
            BuildBuilderSections();
        }

        // ================================================================== map editor: slope line, dungeon

        private (int za, int zb)? _slope;

        /// <summary>Slope line: the colour of the group of the chosen colour at the height between A and B (height layer if it is missing).</summary>
        private ushort SlopeColourFor(int x, int y, ushort current, ushort chosen, double t, Point a, Point b)
        {
            _slope ??= (mapCanvas.AltitudeAt(a.X, a.Y), mapCanvas.AltitudeAt(b.X, b.Y));
            (int za, int zb) = _slope.Value;
            int want = (int)Math.Round(za + (zb - za) * t, MidpointRounding.AwayFromZero);
            if (chosen >= _rules.Palette.Count)
            {
                return chosen;
            }
            int result = chosen;
            int g = _rules.Palette[chosen].Group;
            if (g >= 0)
            {
                if (!_groupEntries.TryGetValue(g, out List<DragonPaletteEntry> entries))
                {
                    _groupEntries[g] = entries = _rules.EntriesOfGroup(g).Where(e => e.Defined).ToList();
                }
                if (entries.Count > 0)
                {
                    result = entries.OrderBy(e => Math.Abs(e.Altitude - want)).ThenBy(e => e.Index == chosen ? 0 : 1).First().Index;
                }
            }
            if (Math.Abs(_rules.Palette[result].Altitude - want) > 1)
            {
                _pendingHeights[(x, y)] = ((sbyte)Math.Clamp(want, -128, 127), (ushort)result);
            }
            else if (_pendingHeights.ContainsKey((x, y)) || _staticsLayer.Heights.ContainsKey((x, y)))
            {
                _pendingHeights[(x, y)] = null;
            }
            return (ushort)result;
        }

        private void MapCanvas_DungeonRequested(object sender, Rectangle r)
        {
            if (_map == null)
            {
                return;
            }
            DungeonSettings o = BrushCfg.Dungeon;
            int seed = o.Seed != 0 ? o.Seed : new Random().Next(1, int.MaxValue);
            DungeonPlan plan = Builders.Plan(r.Width, r.Height, o, new Random(seed));
            if (plan.Rooms.Count == 0)
            {
                SetStatus(L.T("Dungeon: kein Raum passt – größer aufziehen oder kleinere Räume einstellen."));
                return;
            }
            var floor = (ushort)mapCanvas.PrimaryIndex;
            var wall = (ushort)mapCanvas.SecondaryIndex;
            mapCanvas.EditRect(r, L.T("Dungeon"), m => Builders.PaintDungeon(m, r, plan, floor, wall, o.FillRock2D));
            mapCanvas.InvalidateMap();
            SetStatus(L.T("Dungeon") + $": {plan.Rooms.Count} " + L.T("Räume") + $", {plan.FloorCount} " + L.T("Felder Boden (Farbe 1), Wände Farbe 2") + " – " +
                      L.T("Zufalls-Zahl") + $" {seed} · Strg+Z " + L.T("nimmt es zurück") + ".");
        }

        // ================================================================== sets

        private void ApplyBridgeSet()
        {
            isoView.BridgeSet = BrushCfg.BridgeSets.FirstOrDefault(b => b.Name == BrushCfg.BridgeSet);
            if (_propBridgeSet != null)
            {
                _propBridgeSet.SelectedObject = isoView.BridgeSet;
            }
        }

        private void ApplyStairSet()
        {
            isoView.StairSet = BrushCfg.StairSets.FirstOrDefault(b => b.Name == BrushCfg.StairSet);
            if (_propStairSet != null)
            {
                _propStairSet.SelectedObject = isoView.StairSet;
            }
        }

        private static void FillNamed<T>(ComboBox box, List<T> list, string chosen, Func<T, string> name)
        {
            if (box == null)
            {
                return;
            }
            box.Items.Clear();
            foreach (T t in list)
            {
                box.Items.Add(t);
            }
            box.SelectedIndex = list.FindIndex(t => name(t) == chosen);
        }

        private string AskName(string prompt, string start)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(L.T(prompt), Text, L.T(start));
            return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        }

        /// <summary>The item chosen in the catalogue goes into a slot of the current bridge / stair set.</summary>
        private void SlotFromItem(object set, Action<object, string> put, PropertyGrid grid, string slotName)
        {
            if (set == null)
            {
                SetStatus(L.T("Erst ein Set anlegen ('Neu …')."));
                return;
            }
            string id = isoView.PlaceId.ToString("x4");
            put(set, id);
            grid?.Refresh();
            SetStatus($"{id} {UoArtCache.ItemName(isoView.PlaceId)} → {L.T(slotName)}");
        }

        private static string AddId(string list, string id) => string.IsNullOrWhiteSpace(list) ? id : list.Contains(id) ? list : list + " " + id;

        // ================================================================== tab "Pinsel": sections

        private void BuildBuilderSections()
        {
            if (_tabCgiBrushes?.Controls.Count == 0 || !(_tabCgiBrushes.Controls[0] is FlowLayoutPanel col))
            {
                return;
            }
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
            var sections = new List<Control>();
            int before = col.Controls.Count;

            // ---- bridge
            _cmbBridgeSet = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbBridgeSet.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbBridgeSet.SelectedItem is BridgeSet b)
                {
                    BrushCfg.BridgeSet = b.Name;
                    ApplyBridgeSet();
                }
            };
            _propBridgeSet = Grid(null, 150);
            _cmbBridgePlan = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbBridgePlan.SelectedIndexChanged += (s, e) =>
            {
                BrushCfg.BridgeTemplate = _cmbBridgePlan.SelectedItem is BridgeTemplate t ? t.ToString() : string.Empty;
                isoView.Invalidate();
            };
            AddSection(col, "build.bridge", "Brücke (Statics über Wasser)", ColBuild, () => ToolIcons.Get("bridge"), true,
                Row(ToolBtn("Brücken-Werkzeug wählen", IsoTool.Bridge, ColBuild)),
                Row(Btn("Brücken-Designer öffnen …", ColBuild, OpenBridgeDesigner, true)),
                Row(new Label { Text = L.T("Plan:"), AutoSize = true }),
                Row(_cmbBridgePlan),
                Row(new Label { Text = L.T("Set (ohne Plan):"), AutoSize = true }),
                Row(_cmbBridgeSet),
                Row(Btn("Neu …", ColBuild, () =>
                {
                    string n = AskName("Name des Brücken-Sets:", "Holzbrücke");
                    if (n == null)
                    {
                        return;
                    }
                    BrushCfg.BridgeSets.RemoveAll(b => b.Name == n);
                    BrushCfg.BridgeSets.Add(new BridgeSet { Name = n });
                    BrushCfg.BridgeSet = n;
                    FillNamed(_cmbBridgeSet, BrushCfg.BridgeSets, n, b => b.Name);
                    ApplyBridgeSet();
                }), Btn("Löschen", ColCheck, () =>
                {
                    BrushCfg.BridgeSets.RemoveAll(b => b.Name == BrushCfg.BridgeSet);
                    BrushCfg.BridgeSet = BrushCfg.BridgeSets.FirstOrDefault()?.Name ?? string.Empty;
                    FillNamed(_cmbBridgeSet, BrushCfg.BridgeSets, BrushCfg.BridgeSet, b => b.Name);
                    ApplyBridgeSet();
                })),
                Row(Btn("← Brett W-O", ColView, () => SlotFromItem(isoView.BridgeSet, (o, id) => ((BridgeSet)o).DeckX = AddId(((BridgeSet)o).DeckX, id), _propBridgeSet, "Bretter West-Ost")),
                    Btn("← Brett N-S", ColView, () => SlotFromItem(isoView.BridgeSet, (o, id) => ((BridgeSet)o).DeckY = AddId(((BridgeSet)o).DeckY, id), _propBridgeSet, "Bretter Nord-Süd"))),
                Row(Btn("← Geländer W-O", ColView, () => SlotFromItem(isoView.BridgeSet, (o, id) => ((BridgeSet)o).RailX = AddId(((BridgeSet)o).RailX, id), _propBridgeSet, "Geländer West-Ost")),
                    Btn("← Geländer N-S", ColView, () => SlotFromItem(isoView.BridgeSet, (o, id) => ((BridgeSet)o).RailY = AddId(((BridgeSet)o).RailY, id), _propBridgeSet, "Geländer Nord-Süd")),
                    Btn("← Pfosten", ColView, () => SlotFromItem(isoView.BridgeSet, (o, id) => ((BridgeSet)o).Post = id, _propBridgeSet, "Pfosten"))),
                Row(_propBridgeSet),
                Row(Grid(BrushCfg.Bridge, 140)),
                Row(Hint(L.T("Von Ufer zu Ufer ziehen – beim Ziehen sieht man die echten Teile. Ohne Plan und ohne Set sucht das Studio Bretter, Geländer, Pfosten und Pfeiler selbst in der TileData. Eigene Brücken baut man im Brücken-Designer. Höhe = höheres Ende + Z +, die Mitte um 'Bogen' höher."))));
            FillNamed(_cmbBridgeSet, BrushCfg.BridgeSets, BrushCfg.BridgeSet, b => b.Name);
            ApplyBridgeSet();
            FillBridgePlans();

            // ---- cellar + under the map
            _chkUnderView = new CheckBox { Text = L.T("Unter der Karte bauen (Ansicht + Bau-Ebene)"), AutoSize = true, Checked = isoView.UnderView };
            _chkUnderView.CheckedChanged += (s, e) => SetUnderView(_chkUnderView.Checked);
            var offset = new NumericUpDown { Minimum = -127, Maximum = 0, Value = Math.Clamp(BrushCfg.UnderOffset, -127, 0), Width = 70 };
            offset.ValueChanged += (s, e) =>
            {
                BrushCfg.UnderOffset = (int)offset.Value;
                isoView.Invalidate();
            };
            _cmbStairSet = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbStairSet.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbStairSet.SelectedItem is StairSet b)
                {
                    BrushCfg.StairSet = b.Name;
                    ApplyStairSet();
                }
            };
            _propStairSet = Grid(null, 130);
            AddSection(col, "build.cellar", "Keller / unter der Karte bauen", Color.FromArgb(126, 34, 206), () => ToolIcons.Get("cellar"), true,
                Row(ToolBtn("Keller-Werkzeug wählen", IsoTool.Cellar, Color.FromArgb(126, 34, 206))),
                Row(_chkUnderView),
                Row(new Label { Text = L.T("Bau-Ebene (Z unter dem Boden):"), AutoSize = true }, offset),
                Row(Grid(BrushCfg.Cellar, 200)),
                Row(_cmbStairSet),
                Row(ToolBtn("Treppe setzen", IsoTool.Stairs, ColBuild), Btn("Treppe aus TileData", ColView, AutoStairSet)),
                Row(Btn("Treppe: Richtung / Stufen …", ColView, () => ShowSettingsWindow(new StairToolOptions(BrushCfg), L.T("Treppe")))),
                Row(Btn("Neues Treppen-Set …", ColBuild, () =>
                {
                    string n = AskName("Name des Treppen-Sets:", "Steintreppe");
                    if (n == null)
                    {
                        return;
                    }
                    BrushCfg.StairSets.RemoveAll(b => b.Name == n);
                    BrushCfg.StairSets.Add(new StairSet { Name = n });
                    BrushCfg.StairSet = n;
                    FillNamed(_cmbStairSet, BrushCfg.StairSets, n, b => b.Name);
                    ApplyStairSet();
                }), Btn("Löschen", ColCheck, () =>
                {
                    BrushCfg.StairSets.RemoveAll(b => b.Name == BrushCfg.StairSet);
                    BrushCfg.StairSet = BrushCfg.StairSets.FirstOrDefault()?.Name ?? string.Empty;
                    FillNamed(_cmbStairSet, BrushCfg.StairSets, BrushCfg.StairSet, b => b.Name);
                    ApplyStairSet();
                })),
                Row(Btn("← ab Nord", ColView, () => SlotFromItem(isoView.StairSet, (o, id) => ((StairSet)o).DownNorth = AddId(((StairSet)o).DownNorth, id), _propStairSet, "hinunter nach Norden")),
                    Btn("← ab Ost", ColView, () => SlotFromItem(isoView.StairSet, (o, id) => ((StairSet)o).DownEast = AddId(((StairSet)o).DownEast, id), _propStairSet, "hinunter nach Osten")),
                    Btn("← ab Süd", ColView, () => SlotFromItem(isoView.StairSet, (o, id) => ((StairSet)o).DownSouth = AddId(((StairSet)o).DownSouth, id), _propStairSet, "hinunter nach Süden")),
                    Btn("← ab West", ColView, () => SlotFromItem(isoView.StairSet, (o, id) => ((StairSet)o).DownWest = AddId(((StairSet)o).DownWest, id), _propStairSet, "hinunter nach Westen"))),
                Row(_propStairSet),
                Row(Hint(L.T("Keller: Rechteck aufziehen. Das Land wird unsichtbar (NoDraw 0002) auf Boden − Tiefe – so kann der Client darunter zeichnen und man darunter laufen. " +
                             "Rand = Wände (Wand-Set, Name in 'Wände'), innen Kellerboden, Treppe von der Bodenhöhe in 'Stufen-Höhe'-Schritten hinunter, darüber optional der Hausboden. " +
                             "'Unter der Karte bauen' zeigt Statics unter dem Boden über dem Land und setzt Statics / Wände auf die Bau-Ebene."))));
            FillNamed(_cmbStairSet, BrushCfg.StairSets, BrushCfg.StairSet, b => b.Name);
            ApplyStairSet();

            // ---- dungeon
            _cmbDungeonLook = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbDungeonLook.SelectedIndexChanged += (s, e) =>
            {
                BrushCfg.Dungeon.Look = _cmbDungeonLook.SelectedItem is DungeonLook l ? l.Name : string.Empty;
                if (_propDungeonLook != null)
                {
                    _propDungeonLook.SelectedObject = _cmbDungeonLook.SelectedItem as DungeonLook;
                }
            };
            _propDungeonLook = Grid(null, 170);
            AddSection(col, "build.dungeon", "Dungeon-Generator", Color.FromArgb(68, 64, 60), () => ToolIcons.Get("dungeon"), true,
                Row(ToolBtn("Dungeon-Werkzeug (CGI) wählen", IsoTool.Dungeon, Color.FromArgb(68, 64, 60))),
                Row(new Label { Text = L.T("Gelernter Stil:"), AutoSize = true }, _cmbDungeonLook),
                Row(Btn("Stil aus markiertem Bereich lernen …", ColBuild, () => AnalyzeDungeonArea(true), true)),
                Row(Btn("Nur analysieren (Log)", ColView, () => AnalyzeDungeonArea(false)), Btn("Stil löschen", ColCheck, () =>
                {
                    BrushCfg.DungeonLooks.RemoveAll(l => l.Name == BrushCfg.Dungeon.Look);
                    BrushCfg.Dungeon.Look = string.Empty;
                    FillDungeonLooks();
                })),
                Row(_propDungeonLook),
                Row(Hint(L.T("So entsteht ein echter UO-Dungeon: auf einer Karte mit Dungeons (z. B. map0 bei x > 5119: Deceit, Doom …) einen Dungeon mit 'Bereich' markieren und 'Stil lernen'. " +
                             "Das Studio merkt sich Boden-Textur, Void-Land, welche Wandstücke in welcher Lage stehen (Nachbarn N O S W, Ecken) und die Deko – und schreibt alles als Log. " +
                             "Der Generator baut damit neue Räume und Gänge im selben Stil."))),
                Row(Btn("Dungeon im Karten-Editor (Farben)", ColView, () =>
                {
                    tabMain.SelectedTab = tabEditor;
                    SelectTool(MapTool.Dungeon);
                    mapCanvas.Focus();
                })),
                Row(Grid(BrushCfg.Dungeon, 330)),
                Row(Hint(L.T("Räume werden zufällig verteilt (ohne Überschneidung), über einen minimalen Spannbaum verbunden, dazu einige Schleifen; Gänge in L-Form. " +
                             "Gebaut = Boden-Textur + Wand-Statics (gelernter Stil oder Wand-Set). Unter der Karte = wie ein Keller unter dem Boden. " +
                             "Im Karten-Editor: Farbe 1 = Boden, Farbe 2 = Wand / Void."))));
            FillDungeonLooks();
            AddSection(col, "build.mountainwall", "Bergwall (Fels-Labyrinth)", ColTerrain, () => ToolIcons.Get("mountainwall"), true,
                Row(ToolBtn("Bergwall-Werkzeug wählen", IsoTool.MountainWall, ColTerrain)),
                Row(Hint(L.T("Rechteck aufziehen: Räume und Gänge wie beim Dungeon, aber die Wände sind hohes Fels-Land (Berg-Tiles) – ein Labyrinth aus Bergwänden. " +
                             "Räume, Gänge, Zufalls-Zahl, Boden- / Wand-Land und Wand-Höhe aus den Dungeon-Einstellungen."))));

            // ---- course
            AddSection(col, "build.course", "Natürlicher Lauf (Fluss / Weg)", ColBuild, () => ToolIcons.Get("course"), true,
                Row(ToolBtn("Fluss-Lauf (CGI) wählen", IsoTool.Course, ColBuild)),
                Row(Btn("Lauf im Karten-Editor (Farbe / Stempel)", ColView, () =>
                {
                    tabMain.SelectedTab = tabEditor;
                    SelectTool(MapTool.Course);
                    mapCanvas.Focus();
                })),
                Row(Grid(BrushCfg.Course, 140)),
                Row(Hint(L.T("Von der Quelle zur Mündung ziehen. CGI: Wasser mit den Fluss-Einstellungen. Karten-Editor: mit Farbe 1 und Pinselgröße – oder mit dem Muster-Stempel (Statics), Schalter oben."))));

            // the new sections directly below the brushes (before the view filter / bookmarks)
            int at = Math.Min(before, col.Controls.Count);
            var added = new List<Control>();
            for (int i = before; i < col.Controls.Count; i++)
            {
                added.Add(col.Controls[i]);
            }
            int insertAt = 0;
            for (int i = 0; i < before; i++)
            {
                if (col.Controls[i] is CollapsibleSection cs && cs.Title == L.T("Ansicht-Filter (Statics)"))
                {
                    insertAt = i;
                    break;
                }
                insertAt = i + 1;
            }
            foreach (Control c in added)
            {
                col.Controls.SetChildIndex(c, insertAt++);
            }
        }

        private void FillDungeonLooks()
        {
            if (_cmbDungeonLook == null)
            {
                return;
            }
            _cmbDungeonLook.Items.Clear();
            _cmbDungeonLook.Items.Add(L.T("(kein – Wand-Set benutzen)"));
            foreach (DungeonLook l in BrushCfg.DungeonLooks)
            {
                _cmbDungeonLook.Items.Add(l);
            }
            int i = BrushCfg.DungeonLooks.FindIndex(l => l.Name == BrushCfg.Dungeon.Look);
            _cmbDungeonLook.SelectedIndex = i + 1;
        }

        /// <summary>
        /// Dungeon analysis of the marked area: log (file + clipboard) with land, heights, statics and how the walls stand;
        /// with <paramref name="learn"/> the look is stored as a style for the generator.
        /// </summary>
        private void AnalyzeDungeonArea(bool learn)
        {
            if (_cgiMap == null || isoView.Area.IsEmpty)
            {
                SetStatus(L.T("Erst in der CGI-Ansicht einen Dungeon mit 'Bereich' markieren (mit etwas schwarzem Rand drumherum)."));
                return;
            }
            string name = "Dungeon";
            if (learn)
            {
                name = AskName("Name des Dungeon-Stils (z. B. Deceit, Doom, Schlamm-Höhle):", "Mein Dungeon");
                if (name == null)
                {
                    return;
                }
            }
            Rectangle area = isoView.Area;
            Cursor = Cursors.WaitCursor;
            DungeonLook look;
            string report;
            try
            {
                (look, report) = DungeonStyleTools.Analyze(_cgiMap, area, null, name, $"{_cgiMap.Source} ({area.X}, {area.Y}, {area.Width}×{area.Height})");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            string file = "dungeon_analyse_" + string.Concat(name.Select(c => char.IsLetterOrDigit(c) ? c : '_')) + ".txt";
            string path = WriteLog(file, report);
            try
            {
                Clipboard.SetText(report);
            }
            catch (Exception)
            {
                // the file is written, the clipboard is only a convenience
            }
            if (learn)
            {
                BrushCfg.DungeonLooks.RemoveAll(l => l.Name == look.Name);
                BrushCfg.DungeonLooks.Add(look);
                BrushCfg.Dungeon.Look = look.Name;
                FillDungeonLooks();
                OpenBrushSection("build.dungeon");
            }
            SetStatus((learn ? L.T("Dungeon-Stil gelernt") + $" '{look.Name}': {look.Walls.Count(w => w.Filled > 0)} " + L.T("Wand-Lagen") + $", {look.DecorPercent} % " + L.T("Deko") + " · " : string.Empty) +
                      L.T("Log") + $": {path} (" + L.T("auch in der Zwischenablage") + ")");
        }

        private DungeonWorkshopForm _workshop;
        private BridgeDesignerForm _bridgeDesigner;
        private List<BridgeTemplate> _bridgePlans;
        private ComboBox _cmbBridgePlan;

        private BridgeTemplate FindBridgePlan(string name)
        {
            _bridgePlans ??= BridgeKit.LoadAll();
            return _bridgePlans.FirstOrDefault(t => t.ToString() == name);
        }

        private void FillBridgePlans()
        {
            _bridgePlans = BridgeKit.LoadAll();
            if (_cmbBridgePlan == null)
            {
                return;
            }
            _cmbBridgePlan.Items.Clear();
            _cmbBridgePlan.Items.Add(L.T("(kein Plan – Brücken-Set / Teile aus der TileData)"));
            foreach (BridgeTemplate t in _bridgePlans)
            {
                _cmbBridgePlan.Items.Add(t);
            }
            int i = _bridgePlans.FindIndex(t => t.ToString() == BrushCfg.BridgeTemplate);
            _cmbBridgePlan.SelectedIndex = i + 1;
        }

        /// <summary>Own window: design bridges in a test field (pieces from the tiledata with pictures), save by category.</summary>
        private void OpenBridgeDesigner()
        {
            if (_bridgeDesigner != null)
            {
                _bridgeDesigner.Activate();
                return;
            }
            var host = new BridgeDesignerHost
            {
                Map = _cgiMap,
                Brushes = BrushCfg,
                Configure = ConfigureLikeMain,
                CurrentArea = () => isoView.Area,
                Status = SetStatus,
                Help = ShowHelp,
                Use = t =>
                {
                    FillBridgePlans();
                    SelectCgiTool(IsoTool.Bridge);
                }
            };
            _bridgeDesigner = new BridgeDesignerForm(host) { Owner = this };
            _bridgeDesigner.FormClosed += (s, e) =>
            {
                _bridgeDesigner = null;
                FillBridgePlans();
            };
            _bridgeDesigner.Show(this);
        }

        /// <summary>Fills the stair set with the stairs found in the tiledata (as a new set "Treppe (TileData)").</summary>
        private void AutoStairSet()
        {
            StairSet st = BridgeKit.AutoStairs();
            if (string.IsNullOrEmpty(st.DownNorth))
            {
                SetStatus(L.T("Keine Treppen-Gruppe in der TileData gefunden – Stufen im Katalog suchen (Suche: stair) und mit '← ab …' zuordnen."));
                return;
            }
            BrushCfg.StairSets.RemoveAll(b => b.Name == st.Name);
            BrushCfg.StairSets.Add(st);
            BrushCfg.StairSet = st.Name;
            FillNamed(_cmbStairSet, BrushCfg.StairSets, st.Name, b => b.Name);
            ApplyStairSet();
            SetStatus(L.T("Treppen-Set aus der TileData") + $": {st.Name} (N {st.DownNorth}, O {st.DownEast}, S {st.DownSouth}, W {st.DownWest}) – " +
                      L.T("Richtung prüfen: Werkzeug Treppe zeigt die Stufen als Vorschau, R dreht."));
        }

        /// <summary>Opens the dungeon workshop for the area marked in the CGI view.</summary>
        private void OpenDungeonWorkshop(WorkshopKind kind)
        {
            tabMain.SelectedTab = tabCgi;
            if (_cgiMap == null)
            {
                SetStatus(L.T("Keine CGI-Karte geladen."));
                return;
            }
            if (isoView.Area.IsEmpty || isoView.Area.Width < 6 || isoView.Area.Height < 6)
            {
                SelectCgiTool(IsoTool.Area);
                SetStatus(L.T("Dungeon-Werkstatt: erst mit 'Bereich' den Platz markieren (zum Markieren großer Plätze weit herauszoomen), dann noch einmal klicken."));
                return;
            }
            _workshop?.Close();
            if (_workshop != null && !_workshop.IsDisposed)
            {
                return; // the open workshop wanted to keep its changes
            }
            var host = new WorkshopHost
            {
                Map = _cgiMap,
                Area = isoView.Area,
                Brushes = BrushCfg,
                Configure = ConfigureLikeMain,
                CurrentArea = () => isoView.Area,
                LandList = () =>
                {
                    EnsureLandList();
                    return _allLand;
                },
                RockTiles = () =>
                {
                    ushort[] t = HexList.Parse(_settings.Mountain?.Tiles ?? string.Empty);
                    return t.Length > 0 ? t : isoView.MountainFallbackTiles;
                },
                CaveFloor = () => isoView.CaveFloorIds,
                WriteLog = (file, text) =>
                {
                    string path = WriteLog(file, text);
                    try
                    {
                        Clipboard.SetText(text);
                    }
                    catch (Exception)
                    {
                        // only a convenience
                    }
                    return path;
                },
                Status = SetStatus,
                Help = ShowHelp,
                Applied = () =>
                {
                    isoView.Refresh(true);
                    IsoView_MapChanged(isoView, EventArgs.Empty);
                    UpdateCgiThumbnail();
                }
            };
            _workshop = new DungeonWorkshopForm(host, kind) { Owner = this };
            _workshop.FormClosed += (s, e) => _workshop = null;
            _workshop.Show(this);
        }

        /// <summary>A CGI view of the workshop gets the settings of the main view (art, tiles, brushes, item, hue …).</summary>
        private void ConfigureLikeMain(IsoMapView v)
        {
            v.LandTransitions = isoView.LandTransitions;
            v.CaveFloorIds = isoView.CaveFloorIds;
            v.MountainFallbackTiles = isoView.MountainFallbackTiles;
            v.Mountain = isoView.Mountain;
            v.CaveIsMountain = isoView.CaveIsMountain;
            v.Coast = isoView.Coast;
            v.CoastSmooth = isoView.CoastSmooth;
            v.BrushSettings = BrushCfg;
            v.WallSet = isoView.WallSet;
            v.ScatterSet = isoView.ScatterSet;
            v.LandFamily = isoView.LandFamily;
            v.FindWallSet = isoView.FindWallSet;
            v.FindDungeonLook = isoView.FindDungeonLook;
            v.BridgeSet = isoView.BridgeSet;
            v.StairSet = isoView.StairSet;
            v.PlaceId = isoView.PlaceId;
            v.PlaceHue = isoView.PlaceHue;
            v.PlaceLandId = isoView.PlaceLandId;
            v.PlaceLandIds = isoView.PlaceLandIds;
            v.LandFlatten = isoView.LandFlatten;
            v.LandRadius = isoView.LandRadius;
            v.HeightStep = isoView.HeightStep;
            v.TargetHeight = isoView.TargetHeight;
            v.ZOffset = isoView.ZOffset;
            v.ShowStatics = isoView.ShowStatics;
            v.TranslucentWater = isoView.TranslucentWater;
            v.HiddenKinds = isoView.HiddenKinds;
            v.LabelMode = isoView.LabelMode;
            v.ShowStaticIds = isoView.ShowStaticIds;
        }

        private void SetUnderView(bool on)
        {
            isoView.UnderView = on;
            if (_chkUnderView != null && _chkUnderView.Checked != on)
            {
                _chkUnderView.Checked = on;
            }
            if (_tsbUnderView != null)
            {
                _tsbUnderView.Checked = on;
            }
            _cgiPalette?.RefreshState();
            SetStatus(on ? L.T("Unter der Karte bauen: Statics unter dem Boden werden gezeigt, Static setzen und Wände auf Boden") + $" {BrushCfg.UnderOffset}."
                : L.T("Wieder über der Karte."));
        }

        // ================================================================== palettes, options bar

        /// <summary>Map editor palette: the new shapes (after the old shapes).</summary>
        private void AddEditorShapeEntries(ToolPalette p)
        {
            p.Add(EdTool("halfcircle", "Halbkreis", string.Empty, "Von der Mitte der geraden Seite zur Rundung ziehen · gefüllt / Rand oben", MapTool.HalfCircle, AccShape));
            p.Add(EdTool("triangle", "Dreieck", string.Empty, "Von der Mitte der Grundseite zur Spitze ziehen · gefüllt / Rand oben", MapTool.Triangle, AccShape));
            p.Add(EdTool("slopeline", "Neigungs-Linie", string.Empty, "Linie, deren Höhe von der Höhe am Start zur Höhe am Ende läuft (Farbe 1 in der passenden Höhe, sonst Höhen-Ebene) – Rampen, Straßen am Hang", MapTool.SlopeLine, AccHeight));
            p.Add(EdTool("course", "Natürlicher Lauf", string.Empty, "Von A nach B ziehen: geschlängelter Fluss / Weg mit Pinselgröße – oder mit dem Muster-Stempel (Statics)", MapTool.Course, AccShape));
            p.Add(EdTool("dungeon", "Dungeon", string.Empty, "Rechteck aufziehen: Räume + Gänge, Farbe 1 = Boden, Farbe 2 = Wand", MapTool.Dungeon, AccCave));
        }

        /// <summary>CGI palette: the builders.</summary>
        private void AddCgiBuilderEntries(ToolPalette p, Action<ToolPaletteEntry, string> add)
        {
            p.AddSeparator();
            add(CgiTool("bridge", "Brücke", string.Empty, "Von Ufer zu Ufer ziehen: Bretter, Geländer, Pfosten, Bogen (Brücken-Set im Reiter Pinsel)", IsoTool.Bridge, ColBuild), "build");
            add(new ToolPaletteEntry
            {
                Key = "dungeon", Name = "Dungeon-Werkstatt", Accent = Color.FromArgb(68, 64, 60),
                Help = "Erst mit 'Bereich' einen Platz markieren, dann klicken: eigenes Fenster – klassischer Dungeon, Keller, Berg-Dungeon, Bergwall; Vorschau oben / unten, 2D-Plan, Skript, Rückgängig, Verschieben, Höhe, Löschen",
                Click = () => OpenDungeonWorkshop(WorkshopKind.Classic)
            }, "build");
            add(new ToolPaletteEntry
            {
                Key = "cellar", Name = "Keller (Werkstatt)", Accent = Color.FromArgb(126, 34, 206),
                Help = "Erst mit 'Bereich' einen Platz markieren, dann klicken: die Werkstatt im Keller-Modus (Statics unter der Karte, Loch auf Kartenhöhe)",
                Click = () => OpenDungeonWorkshop(WorkshopKind.Cellar)
            }, "build");
            add(CgiTool("course", "Fluss-Lauf", string.Empty, "Von der Quelle zur Mündung ziehen: natürlich geschlängelter Fluss mit Wasser, Breite am Anfang / Ende einstellbar", IsoTool.Course, ColBuild), "build");
            add(new ToolPaletteEntry
            {
                Key = "bridgedesigner", Name = "Brücken-Designer", Accent = ColBuild,
                Help = "Eigenes Fenster: Brücken aus Statics entwerfen (Teile aus der TileData mit Bild, Testfeld, Muster von Hand, aus Karte / Multi), nach Kategorie speichern",
                Click = OpenBridgeDesigner
            }, "build");
            add(CgiTool("stairs", "Treppe", string.Empty, "Klick: Stufen nach unten in die Richtung (R dreht), bis zum Boden gefüllt (Treppen-Set)", IsoTool.Stairs, ColBuild), "build");

            add(new ToolPaletteEntry
            {
                Key = "undermap",
                Name = "Unter der Karte bauen (an / aus)",
                Help = "Kein Werkzeug, ein Schalter: Statics unter dem Boden werden gezeigt, Static setzen / Wände auf der Bau-Ebene (Boden − 20)",
                Accent = Color.FromArgb(126, 34, 206),
                IsChecked = () => isoView.UnderView,
                Click = () => SetUnderView(!isoView.UnderView)
            }, "build");
        }

        private void BuildBuilderOptionItems()
        {
            // map editor
            _edShapeFilled = new ToolStripButton(L.T("Gefüllt")) { CheckOnClick = true, Checked = mapCanvas.ShapeFilled, ToolTipText = L.T("Halbkreis / Dreieck gefüllt – aus = nur der Rand (Dicke = Pinselgröße)") };
            _edShapeFilled.CheckedChanged += (s, e) => mapCanvas.ShapeFilled = _edShapeFilled.Checked;
            _edCourseStamp = new ToolStripButton(L.T("mit Muster-Stempel (Statics)")) { CheckOnClick = true, ToolTipText = L.T("An = der Lauf wird mit dem gewählten Muster-Stempel gemalt (Land + Statics), aus = mit Farbe 1 und Pinselgröße") };
            _edCourseStamp.CheckedChanged += (s, e) =>
            {
                mapCanvas.CourseWithPattern = _edCourseStamp.Checked;
                if (_edCourseStamp.Checked && mapCanvas.PatternColour == null)
                {
                    SetStatus(L.T("Erst im Stempel-Menü einen Stempel als Muster-Pinsel wählen (Freihand), dann wieder 'Natürlicher Lauf'."));
                }
            };
            var courseSettings = new ToolStripButton(L.T("Schlängeln …")) { ToolTipText = L.T("Einstellungen des Laufs (Schlängeln, Bögen, Breite wächst)") };
            courseSettings.Click += (s, e) => ShowSettingsWindow(BrushCfg.Course, L.T("Natürlicher Lauf"));
            var dungeonSettings = new ToolStripButton(L.T("Dungeon-Einstellungen …"));
            dungeonSettings.Click += (s, e) => ShowSettingsWindow(BrushCfg.Dungeon, L.T("Dungeon-Generator"));
            int at = toolStripEditor.Items.IndexOf(_edKeepHeight) + 1;
            foreach ((ToolStripItem item, Func<MapTool, bool> show) in new (ToolStripItem, Func<MapTool, bool>)[]
                     {
                         (_edShapeFilled, t => t == MapTool.HalfCircle || t == MapTool.Triangle),
                         (_edCourseStamp, t => t == MapTool.Course),
                         (courseSettings, t => t == MapTool.Course),
                         (dungeonSettings, t => t == MapTool.Dungeon)
                     })
            {
                toolStripEditor.Items.Insert(at++, item);
                _edToolItems.Add((item, show));
            }

            // CGI view
            var bridgeSets = new ToolStripButton(L.T("Brücken-Einstellungen …"));
            bridgeSets.Click += (s, e) => OpenBrushSection("build.bridge");
            var cellar = new ToolStripButton(L.T("Keller-Einstellungen …"));
            cellar.Click += (s, e) => OpenBrushSection("build.cellar");
            var dungeon = new ToolStripButton(L.T("Dungeon-Einstellungen …"));
            dungeon.Click += (s, e) => ShowSettingsWindow(BrushCfg.Dungeon, L.T("Dungeon-Generator"));
            var course = new ToolStripButton(L.T("Fluss: Breite / Schlängeln …"));
            course.Click += (s, e) => ShowSettingsWindow(BrushCfg.Course, L.T("Natürlicher Lauf"));
            var river = new ToolStripButton(L.T("Wasser …"));
            river.Click += (s, e) => ShowSettingsWindow(BrushCfg.River, L.T("Fluss"));
            var designer = new ToolStripButton(L.T("Brücken-Designer …"));
            designer.Click += (s, e) => OpenBridgeDesigner();
            var stairs = new ToolStripButton(L.T("Richtung / Stufen …"));
            stairs.Click += (s, e) => ShowSettingsWindow(new StairToolOptions(BrushCfg), L.T("Treppe"));
            var eraser = new ToolStripButton(L.T("Radierer …"));
            eraser.Click += (s, e) => ShowSettingsWindow(BrushCfg.Eraser, L.T("Radierer"));
            _tsbUnderView = new ToolStripButton(L.T("⬇ Unter der Karte")) { CheckOnClick = true, Checked = isoView.UnderView, ToolTipText = L.T("Unter der Karte bauen: Statics unter dem Boden zeigen, Bau-Ebene Boden − 20") };
            _tsbUnderView.Click += (s, e) => SetUnderView(_tsbUnderView.Checked);
            foreach ((ToolStripItem item, Func<IsoTool, bool> show) in new (ToolStripItem, Func<IsoTool, bool>)[]
                     {
                         (bridgeSets, t => t == IsoTool.Bridge),
                         (cellar, t => t == IsoTool.Cellar),
                         (dungeon, t => t == IsoTool.Dungeon),
                         (course, t => t == IsoTool.Course),
                         (river, t => t == IsoTool.Course || t == IsoTool.River),
                         (designer, t => t == IsoTool.Bridge),
                         (stairs, t => t == IsoTool.Stairs),
                         (eraser, t => t == IsoTool.Eraser),
                         (_tsbUnderView, t => true)
                     })
            {
                toolStripCgiLand.Items.Add(item);
                _cgiToolItems.Add((item, show));
            }
        }

        private void UpdateBuilderEditorItems(MapTool t)
        {
            foreach ((ToolStripItem item, Func<MapTool, bool> show) in _edToolItems)
            {
                item.Visible = show(t);
            }
        }

        /// <summary>Opens the tab "Pinsel" with this section unfolded.</summary>
        private void OpenBrushSection(string key)
        {
            tabCgiSide.SelectedTab = _tabCgiBrushes;
            UpdateCgiNav();
            if (_tabCgiBrushes?.Controls.Count > 0 && _tabCgiBrushes.Controls[0] is FlowLayoutPanel col)
            {
                foreach (CollapsibleSection s in col.Controls.OfType<CollapsibleSection>())
                {
                    if (_sectionKeys.TryGetValue(s, out string k) && k == key)
                    {
                        s.Collapsed = false;
                        col.ScrollControlIntoView(s);
                    }
                }
            }
        }

        private Form _settingsWindow;

        /// <summary>Small window with the settings of a tool (stays open next to the map).</summary>
        private void ShowSettingsWindow(object settings, string title)
        {
            _settingsWindow?.Close();
            var f = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(360, 460),
                Owner = this,
                Location = new Point(Math.Max(0, Cursor.Position.X - 380), Math.Max(0, Cursor.Position.Y))
            };
            f.Controls.Add(new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = settings, ToolbarVisible = false, HelpVisible = true, PropertySort = PropertySort.Categorized });
            f.FormClosed += (s, e) => _settingsWindow = null;
            _settingsWindow = f;
            f.Show(this);
        }
    }
}
