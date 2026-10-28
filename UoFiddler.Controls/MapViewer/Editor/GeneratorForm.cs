/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.MapViewer.Editor.Tools;
using UoFiddler.Controls.MapViewer.Editor.Transitions;
using UoFiddler.Controls.MapViewer.Help;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Generator: plots with floors, road with paths, lake, rough ground, trees and plants in an area of the working area.
    /// "Generate" again replaces the last result (new seed = new variant).
    /// </summary>
    public class GeneratorForm : Form
    {
        private readonly Map _map;
        private readonly RegionEditSession _session;
        private readonly TransitionLibrary _transitions;
        private readonly Func<BrushPreset> _currentBrush;
        private readonly Func<IReadOnlyList<ushort>> _paletteSelection;
        private readonly Func<IEnumerable<ushort>> _clipStatics;
        private Rectangle _area;
        private int _lastOperations = -1;

        private readonly CheckBox _houses = new CheckBox { Text = "Plots (house floors)", Checked = true, AutoSize = true };
        private readonly NumericUpDown _count = Num(1, 200, 6);
        private readonly NumericUpDown _plotW = Num(2, 64, 7);
        private readonly NumericUpDown _plotH = Num(2, 64, 7);
        private readonly NumericUpDown _spacing = Num(0, 64, 3);
        private readonly NumericUpDown _roadDistance = Num(0, 32, 2);
        private readonly ComboBox _floor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly TextBox _floorTiles = new TextBox { Width = 200, PlaceholderText = "tiles, e.g. 0x4E5-0x4E8" };
        private readonly CheckBox _flatten = new CheckBox { Text = "Flatten plots", Checked = true, AutoSize = true };
        private readonly CheckBox _clearStatics = new CheckBox { Text = "Remove statics on plots and road", Checked = true, AutoSize = true };

        private readonly CheckBox _road = new CheckBox { Text = "Road through the area", Checked = true, AutoSize = true };
        private readonly ComboBox _roadDirection = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        private readonly NumericUpDown _roadWidth = Num(1, 16, 3);
        private readonly ComboBox _roadBrush = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
        private readonly TextBox _roadTiles = new TextBox { Width = 200, Text = "0x0071-0x0074", PlaceholderText = "tiles" };
        private readonly CheckBox _paths = new CheckBox { Text = "Paths from the plots to the road", Checked = true, AutoSize = true };

        private readonly CheckBox _rough = new CheckBox { Text = "Rough ground (random heights)", Checked = true, AutoSize = true };
        private readonly NumericUpDown _roughAmount = Num(1, 10, 1);
        private readonly NumericUpDown _roughPercent = Num(1, 100, 30);
        private readonly CheckBox _roughLower = new CheckBox { Text = "also lower", AutoSize = true };
        private readonly CheckBox _roughRoad = new CheckBox { Text = "on the road", Checked = true, AutoSize = true };
        private readonly CheckBox _roughGround = new CheckBox { Text = "on free ground", Checked = true, AutoSize = true };

        private readonly CheckBox _lake = new CheckBox { Text = "Lake", AutoSize = true };
        private readonly NumericUpDown _lakeX = Num(0, 65535, 0);
        private readonly NumericUpDown _lakeY = Num(0, 65535, 0);
        private readonly NumericUpDown _lakeRx = Num(1, 200, 8);
        private readonly NumericUpDown _lakeRy = Num(1, 200, 6);
        private readonly NumericUpDown _lakeIrregular = Num(0, 100, 35);
        private readonly TextBox _waterTiles = new TextBox { Width = 200, Text = "0x00A8-0x00AB" };
        private readonly NumericUpDown _waterZ = Num(-128, 127, -5);
        private readonly NumericUpDown _shore = Num(0, 10, 2);
        private readonly ComboBox _shoreTransition = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };

        private readonly CheckBox _vegetation = new CheckBox { Text = "Trees and plants", Checked = true, AutoSize = true };
        private readonly TextBox _treeIds = new TextBox { Width = 420 };
        private readonly NumericUpDown _treeDensity = Num(0, 1000, 25);
        private readonly NumericUpDown _treeDistance = Num(0, 10, 2);
        private readonly TextBox _plantIds = new TextBox { Width = 420, PlaceholderText = "static ids of bushes, grass, flowers, rocks ..." };
        private readonly NumericUpDown _plantDensity = Num(0, 1000, 40);
        private readonly TextBox _vegGround = new TextBox { Width = 300, PlaceholderText = "empty = everywhere except water, road, plots" };
        private readonly CheckBox _onlyEmpty = new CheckBox { Text = "only on cells without statics", Checked = true, AutoSize = true };

        private readonly NumericUpDown _seed = Num(1, int.MaxValue, 1);
        private readonly CheckBox _replace = new CheckBox { Text = "Replace the last result", Checked = true, AutoSize = true };
        private readonly Label _labelArea = new Label { AutoSize = true };
        private readonly Label _labelResult = new Label { Dock = DockStyle.Fill, ForeColor = Color.DarkGreen };

        public GeneratorForm(Map map, RegionEditSession session, Rectangle area, TransitionLibrary transitions, BrushLibrary brushes,
            Func<BrushPreset> currentBrush, Func<IReadOnlyList<ushort>> paletteSelection, Func<IEnumerable<ushort>> clipStatics)
        {
            _map = map;
            _session = session;
            _transitions = transitions;
            _currentBrush = currentBrush;
            _paletteSelection = paletteSelection;
            _clipStatics = clipStatics;

            Text = "Generator - settlement, road, lake, vegetation";
            Icon = Options.GetFiddlerIcon();
            ClientSize = new Size(660, 560);
            MinimumSize = new Size(620, 500);
            StartPosition = FormStartPosition.CenterParent;

            _roadDirection.Items.AddRange(new object[] { "west - east", "north - south" });
            _roadDirection.SelectedIndex = 0;
            foreach (ComboBox combo in new[] { _floor, _roadBrush })
            {
                combo.Items.Add("(current brush)");
                combo.Items.Add("(tiles in the field)");
                combo.Items.AddRange(brushes.Brushes.Cast<object>().ToArray());
            }

            _floor.Items.Insert(0, "(no floor, only flatten)");
            _floor.SelectedIndex = 1;
            _roadBrush.SelectedIndex = 1;
            _shoreTransition.Items.Add("(none)");
            _shoreTransition.Items.AddRange(transitions.Sets.Cast<object>().ToArray());
            _shoreTransition.SelectedIndex = 0;
            _treeIds.Text = TransitionSet.FormatIds(new GeneratorOptions().TreeIds);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(Page("Settlement",
                Row(_houses),
                Row(L("Count:"), _count, L("Plot size:"), _plotW, L("x"), _plotH),
                Row(L("Space between plots:"), _spacing, L("Distance to the road:"), _roadDistance),
                Row(L("Floor:"), _floor),
                Row(L("Tiles:"), _floorTiles, Btn("< palette", (s, e) => _floorTiles.Text = TransitionSet.FormatIds(_paletteSelection()))),
                Row(_flatten, _clearStatics),
                Row(new Label { Height = 8 }),
                Row(_road, L("Direction:"), _roadDirection, L("Width:"), _roadWidth),
                Row(L("Road:"), _roadBrush),
                Row(L("Tiles:"), _roadTiles, Btn("< palette", (s, e) => _roadTiles.Text = TransitionSet.FormatIds(_paletteSelection()))),
                Row(_paths),
                Hint("Plots are placed on both sides of the road. A pattern brush as floor starts at the corner of each plot (floors inside of houses).")));
            tabs.TabPages.Add(Page("Ground",
                Row(_rough),
                Row(L("Height change up to:"), _roughAmount, L("Chance per cell %:"), _roughPercent),
                Row(_roughLower, _roughRoad, _roughGround),
                Hint("Lifts single cells randomly so that roads and grass do not look flat. Statics move with the ground.")));
            tabs.TabPages.Add(Page("Lake",
                Row(_lake),
                Row(L("Center X:"), _lakeX, L("Y:"), _lakeY, Btn("= middle of the area", (s, e) => CenterLake())),
                Row(L("Radius X:"), _lakeRx, L("Y:"), _lakeRy, L("Irregular %:"), _lakeIrregular),
                Row(L("Water tiles:"), _waterTiles, L("Water z:"), _waterZ),
                Row(L("Shore slope (cells):"), _shore),
                Row(L("Shore transition:"), _shoreTransition),
                Hint("The shore transition must have the water as terrain A (inside) and the ground as terrain B.")));
            tabs.TabPages.Add(Page("Vegetation",
                Row(_vegetation),
                Row(L("Trees:"), _treeIds),
                Row(L("Trees per 1000 cells:"), _treeDensity, L("Min. distance:"), _treeDistance),
                Row(L("Plants:"), _plantIds),
                Row(L("Plants per 1000 cells:"), _plantDensity, Btn("Statics of the clip", (s, e) => TakeClipStatics())),
                Row(L("Only on land:"), _vegGround, Btn("< palette", (s, e) => _vegGround.Text = TransitionSet.FormatIds(_paletteSelection()))),
                Row(_onlyEmpty),
                Hint("\"Statics of the clip\": copy a forest (tab 1) and use its static ids as plants.")));

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 104, Padding = new Padding(6) };
            var row1 = Row(_labelArea);
            var row2 = Row(L("Seed:"), _seed, Btn("New seed", (s, e) => _seed.Value = new Random().Next(1, int.MaxValue)), _replace);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 34, FlowDirection = FlowDirection.RightToLeft };
            var close = new Button { Text = "Close", Width = 90 };
            close.Click += (s, e) => Close();
            var generate = new Button { Text = "Generate", Width = 120 };
            generate.Click += OnClickGenerate;
            var help = new Button { Text = "Help (F1)", Width = 90 };
            help.Click += (s, e) => MapHelpForm.ShowTopic(this, MapHelpContent.Generator);
            buttons.Controls.AddRange(new Control[] { close, generate, help });
            row1.Dock = DockStyle.Top;
            row2.Dock = DockStyle.Top;
            bottom.Controls.Add(_labelResult);
            bottom.Controls.Add(buttons);
            bottom.Controls.Add(row2);
            bottom.Controls.Add(row1);
            AcceptButton = generate;

            Controls.Add(tabs);
            Controls.Add(bottom);

            HelpRequested += (s, e) =>
            {
                MapHelpForm.ShowTopic(this, MapHelpContent.Generator);
                e.Handled = true;
            };

            SetArea(area);
            CenterLake();
        }

        /// <summary>called after every generation (the editor reloads its view)</summary>
        public event EventHandler Generated;

        public void SetArea(Rectangle area)
        {
            _area = area;
            _labelArea.Text = $"Area: {area.X},{area.Y} - {area.Right - 1},{area.Bottom - 1} ({area.Width} x {area.Height}).  " +
                              "Tool \"Select area\" in the map editor chooses another area.";
        }

        #region Layout helpers

        private static NumericUpDown Num(int min, int max, int value) => new NumericUpDown { Minimum = min, Maximum = max, Value = value, Width = 64 };

        private static Label L(string text) => new Label { Text = text, AutoSize = true, Padding = new Padding(0, 6, 0, 0) };

        private static Button Btn(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true, Height = 25 };
            b.Click += click;
            return b;
        }

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 2, 0, 2) };
            row.Controls.AddRange(controls);
            return row;
        }

        private static Label Hint(string text) => new Label { Text = text, AutoSize = false, Width = 600, Height = 40, ForeColor = SystemColors.GrayText };

        private static TabPage Page(string title, params Control[] rows)
        {
            var page = new TabPage(title) { UseVisualStyleBackColor = true };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(6) };
            flow.Controls.AddRange(rows);
            page.Controls.Add(flow);
            return page;
        }

        #endregion

        private void CenterLake()
        {
            _lakeX.Value = Math.Max(_lakeX.Minimum, Math.Min(_lakeX.Maximum, _area.X + (_area.Width / 2)));
            _lakeY.Value = Math.Max(_lakeY.Minimum, Math.Min(_lakeY.Maximum, _area.Y + (_area.Height / 2)));
        }

        private void TakeClipStatics()
        {
            List<ushort> ids = _clipStatics()?.Distinct().ToList() ?? new List<ushort>();
            if (ids.Count == 0)
            {
                MessageBox.Show("The clip has no statics. Copy an area with statics in tab \"1. Source\".", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _plantIds.Text = TransitionSet.FormatIds(ids);
        }

        private static List<ushort> Ids(TextBox box)
        {
            try
            {
                return TransitionSet.ParseIds(box.Text);
            }
            catch (FormatException ex)
            {
                throw new FormatException($"{ex.Message} ({box.PlaceholderText})");
            }
        }

        private BrushPreset BrushFrom(ComboBox combo, TextBox tiles, int offset)
        {
            int index = combo.SelectedIndex - offset;
            if (index < 0)
            {
                return null;
            }

            if (index == 0)
            {
                return _currentBrush()?.Clone();
            }

            if (index == 1)
            {
                List<ushort> ids = Ids(tiles);
                if (ids.Count == 0)
                {
                    return null;
                }

                var b = new BrushPreset { Mode = BrushMode.Textures };
                b.Tiles.AddRange(ids);
                return b;
            }

            return (combo.SelectedItem as BrushPreset)?.Clone();
        }

        private GeneratorOptions ReadOptions()
        {
            return new GeneratorOptions
            {
                Area = _area,
                Seed = (int)_seed.Value,
                Houses = _houses.Checked,
                HouseCount = (int)_count.Value,
                PlotWidth = (int)_plotW.Value,
                PlotHeight = (int)_plotH.Value,
                Spacing = (int)_spacing.Value,
                RoadDistance = (int)_roadDistance.Value,
                FloorBrush = BrushFrom(_floor, _floorTiles, 1),
                FlattenPlots = _flatten.Checked,
                ClearStatics = _clearStatics.Checked,
                Road = _road.Checked,
                RoadHorizontal = _roadDirection.SelectedIndex == 0,
                RoadWidth = (int)_roadWidth.Value,
                RoadBrush = BrushFrom(_roadBrush, _roadTiles, 0),
                PathsToHouses = _paths.Checked,
                Roughness = _rough.Checked,
                RoughAmount = (int)_roughAmount.Value,
                RoughPercent = (int)_roughPercent.Value,
                RoughAlsoLower = _roughLower.Checked,
                RoughRoad = _roughRoad.Checked,
                RoughGround = _roughGround.Checked,
                Lake = _lake.Checked,
                LakeCenter = new Point((int)_lakeX.Value, (int)_lakeY.Value),
                LakeRadiusX = (int)_lakeRx.Value,
                LakeRadiusY = (int)_lakeRy.Value,
                LakeIrregular = (int)_lakeIrregular.Value,
                WaterTiles = Ids(_waterTiles),
                WaterZ = (int)_waterZ.Value,
                ShoreWidth = (int)_shore.Value,
                ShoreTransition = _shoreTransition.SelectedItem as TransitionSet,
                Vegetation = _vegetation.Checked,
                TreeIds = Ids(_treeIds),
                TreeDensity = (int)_treeDensity.Value,
                TreeMinDistance = (int)_treeDistance.Value,
                PlantIds = Ids(_plantIds),
                PlantDensity = (int)_plantDensity.Value,
                VegetationGround = new HashSet<ushort>(Ids(_vegGround)),
                VegetationOnlyEmpty = _onlyEmpty.Checked
            };
        }

        private void OnClickGenerate(object sender, EventArgs e)
        {
            GeneratorOptions options;
            try
            {
                options = ReadOptions();
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // the last result is undone first (only if it is still the last step)
            if (_replace.Checked && _lastOperations == _session.OperationCount && _session.LastOperationName == MapGenerator.StepName)
            {
                _session.UndoLast(false);
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                GeneratorResult result = MapGenerator.Run(_map, _session, options, _transitions);
                _lastOperations = _session.OperationCount;
                _labelResult.Text = result + "\nCtrl+Z in the map editor = undo. Generate again = new result (other seed for another variant).";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The generator failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            Generated?.Invoke(this, EventArgs.Empty);
        }
    }
}
