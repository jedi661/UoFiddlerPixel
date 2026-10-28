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
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.MapViewer.Editor.Tools;
using UoFiddler.Controls.MapViewer.Help;

namespace UoFiddler.Controls.MapViewer.Editor
{
    /// <summary>
    /// Manages and edits brushes: list with categories and search, new/duplicate/rename/delete,
    /// pattern grid (which tile at which position, variants, heights), picture import.
    /// </summary>
    public class BrushEditorForm : Form
    {
        private readonly BrushLibrary _library;
        private readonly ArtPixelCache _art;
        private readonly LandPaletteControl _palette;
        private readonly TreeView _tree = new TreeView();
        private readonly TextBox _search = new TextBox();
        private readonly TextBox _textName = new TextBox();
        private readonly ComboBox _comboCategory = new ComboBox();
        private readonly RadioButton _radioTextures = new RadioButton();
        private readonly RadioButton _radioPattern = new RadioButton();
        private readonly NumericUpDown _numericW = new NumericUpDown();
        private readonly NumericUpDown _numericH = new NumericUpDown();
        private readonly CheckBox _checkPaintZ = new CheckBox();
        private readonly NumericUpDown _numericCellZ = new NumericUpDown();
        private readonly CheckBox _checkLand = new CheckBox();
        private readonly CheckBox _checkStatics = new CheckBox();
        private readonly CheckBox _checkReplace = new CheckBox();
        private readonly ComboBox _comboHeight = new ComboBox();
        private readonly ComboBox _comboAnchor = new ComboBox();
        private readonly Label _labelInfo = new Label();
        private readonly BrushGridControl _grid = new BrushGridControl();
        private readonly PictureBox _iso = new PictureBox();
        private readonly FlowLayoutPanel _textureStrip = new FlowLayoutPanel();
        private readonly Panel _patternTools = new Panel();
        private readonly Panel _texturesTools = new Panel();
        private readonly NumericUpDown _numericSize = new NumericUpDown();
        private readonly ComboBox _comboShape = new ComboBox();
        private readonly CheckBox _checkNumbers = new CheckBox();
        private readonly CheckBox _checkIds = new CheckBox();
        private readonly CheckBox _checkMove = new CheckBox();
        private BrushPreset _brush;
        private BrushPreset _source;
        private bool _modified;
        private bool _filling;

        public BrushEditorForm(BrushLibrary library, ArtPixelCache art, BrushPreset current)
        {
            _library = library;
            _art = art;
            Text = "Brushes";
            Icon = Options.GetFiddlerIcon();
            ClientSize = new Size(1280, 780);
            MinimumSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterParent;
            KeyPreview = true;

            _palette = new LandPaletteControl(art) { Dock = DockStyle.Right, Width = 340 };

            Controls.Add(BuildCenter());
            Controls.Add(new Splitter { Dock = DockStyle.Right, Width = 4 });
            Controls.Add(_palette);
            Controls.Add(new Splitter { Dock = DockStyle.Left, Width = 4 });
            Controls.Add(BuildList());
            Controls.Add(BuildBottom());

            HelpRequested += (s, e) =>
            {
                MapHelpForm.ShowTopic(this, MapHelpContent.BrushEditor);
                e.Handled = true;
            };

            _palette.SelectionChanged += OnPaletteSelectionChanged;
            FormClosing += OnClosing;
            Shown += (s, e) =>
            {
                _palette.Reload();
                FillTree(null);
                SetBrush(current?.Clone() ?? new BrushPreset { Mode = BrushMode.Pattern, Pattern = new BrushPattern(4, 4) }, null);
            };
        }

        /// <summary>The brush to use in the map editor (after "Use brush")</summary>
        public BrushPreset Result { get; private set; }

        #region Layout

        private Control BuildList()
        {
            var panel = new Panel { Dock = DockStyle.Left, Width = 240, Padding = new Padding(4) };
            _search.Dock = DockStyle.Top;
            _search.PlaceholderText = "Search brushes (name / category)";
            _search.TextChanged += (s, e) => FillTree(_source);
            _tree.Dock = DockStyle.Fill;
            _tree.HideSelection = false;
            _tree.AfterSelect += OnTreeSelect;

            var buttons = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 96, ColumnCount = 2, RowCount = 3 };
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            Button Add(string text, EventHandler click, string tip)
            {
                var b = new Button { Text = text, Dock = DockStyle.Fill };
                b.Click += click;
                new ToolTip().SetToolTip(b, tip);
                buttons.Controls.Add(b);
                return b;
            }

            Add("New", (s, e) => NewBrush(), "New empty brush");
            Add("Duplicate", (s, e) => Duplicate(), "Copy of the brush (then change and save it)");
            Add("Rename...", (s, e) => Rename(), "Change the name of the selected brush");
            Add("Category...", (s, e) => ChangeCategory(), "Move the brush into another category");
            Add("Delete", (s, e) => DeleteBrush(), "Delete the selected brush from the library");
            Add("Save", (s, e) => SaveBrush(), "Saves the brush (same name and category = replace)");

            panel.Controls.Add(_tree);
            panel.Controls.Add(_search);
            panel.Controls.Add(buttons);
            return panel;
        }

        private Control BuildCenter()
        {
            var center = new Panel { Dock = DockStyle.Fill };

            var top = new Panel { Dock = DockStyle.Top, Height = 150, Padding = new Padding(6) };
            var labelName = new Label { Text = "Name:", Left = 6, Top = 9, AutoSize = true };
            _textName.SetBounds(70, 6, 220, 23);
            _textName.TextChanged += (s, e) => Changed();
            var labelCat = new Label { Text = "Category:", Left = 300, Top = 9, AutoSize = true };
            _comboCategory.SetBounds(370, 6, 180, 23);
            _comboCategory.TextChanged += (s, e) => Changed();

            var labelSize = new Label { Text = "Brush size:", Left = 6, Top = 39, AutoSize = true };
            _numericSize.SetBounds(80, 36, 56, 23);
            _numericSize.Minimum = 1;
            _numericSize.Maximum = 64;
            _numericSize.ValueChanged += OnBrushSizeChanged;
            _comboShape.DropDownStyle = ComboBoxStyle.DropDownList;
            _comboShape.Items.AddRange(new object[] { "Square", "Round" });
            _comboShape.SetBounds(142, 36, 80, 23);
            _comboShape.SelectedIndexChanged += OnBrushSizeChanged;

            _radioTextures.Text = "Textures (random)";
            _radioTextures.SetBounds(240, 37, 130, 22);
            _radioPattern.Text = "Pattern (tile per position)";
            _radioPattern.SetBounds(374, 37, 190, 22);
            _radioTextures.CheckedChanged += OnModeChanged;

            _checkLand.Text = "Land";
            _checkLand.SetBounds(6, 66, 60, 22);
            _checkStatics.Text = "Statics";
            _checkStatics.SetBounds(70, 66, 70, 22);
            _checkReplace.Text = "Replace statics";
            _checkReplace.SetBounds(144, 66, 120, 22);
            foreach (CheckBox c in new[] { _checkLand, _checkStatics, _checkReplace })
            {
                c.CheckedChanged += (s, e) => Changed();
            }

            _comboHeight.DropDownStyle = ComboBoxStyle.DropDownList;
            _comboHeight.Items.AddRange(new object[] { "Height: keep the ground", "Height: pattern z (absolute)", "Height: pattern z, moved to the ground" });
            _comboHeight.SetBounds(270, 66, 240, 23);
            _comboHeight.SelectedIndexChanged += (s, e) => Changed();
            _comboAnchor.DropDownStyle = ComboBoxStyle.DropDownList;
            _comboAnchor.Items.AddRange(new object[] { "Starts where the stroke starts (stamp)", "Repeats on the map grid (seamless)" });
            _comboAnchor.SetBounds(516, 66, 240, 23);
            _comboAnchor.SelectedIndexChanged += (s, e) => Changed();

            _labelInfo.SetBounds(6, 94, 760, 52);
            _labelInfo.ForeColor = SystemColors.GrayText;

            top.Controls.AddRange(new Control[] { labelName, _textName, labelCat, _comboCategory, labelSize, _numericSize, _comboShape, _radioTextures, _radioPattern, _checkLand, _checkStatics, _checkReplace, _comboHeight, _comboAnchor, _labelInfo });

            // pattern tools
            _patternTools.Dock = DockStyle.Top;
            _patternTools.Height = 66;
            var labelPattern = new Label { Text = "Pattern:", Left = 6, Top = 8, AutoSize = true };
            _numericW.SetBounds(60, 5, 48, 23);
            _numericH.SetBounds(112, 5, 48, 23);
            foreach (NumericUpDown n in new[] { _numericW, _numericH })
            {
                n.Minimum = 1;
                n.Maximum = 64;
            }

            var buttonResize = new Button { Text = "Resize", Left = 164, Top = 4, Width = 58, Height = 25 };
            buttonResize.Click += (s, e) => EditPattern(p => p.ResizeCentered((int)_numericW.Value, (int)_numericH.Value));
            var buttonLeft = new Button { Text = "◄", Left = 228, Top = 4, Width = 28, Height = 25 };
            buttonLeft.Click += (s, e) => EditPattern(p => p.Shift(-1, 0));
            var buttonRight = new Button { Text = "►", Left = 258, Top = 4, Width = 28, Height = 25 };
            buttonRight.Click += (s, e) => EditPattern(p => p.Shift(1, 0));
            var buttonUp = new Button { Text = "▲", Left = 288, Top = 4, Width = 28, Height = 25 };
            buttonUp.Click += (s, e) => EditPattern(p => p.Shift(0, -1));
            var buttonDown = new Button { Text = "▼", Left = 318, Top = 4, Width = 28, Height = 25 };
            buttonDown.Click += (s, e) => EditPattern(p => p.Shift(0, 1));
            var buttonRotate = new Button { Text = "Rotate", Left = 352, Top = 4, Width = 56, Height = 25 };
            buttonRotate.Click += (s, e) => EditPattern(p => p.Rotate90());
            var buttonMx = new Button { Text = "Mirror X", Left = 410, Top = 4, Width = 62, Height = 25 };
            buttonMx.Click += (s, e) => EditPattern(p => p.MirrorX());
            var buttonMy = new Button { Text = "Mirror Y", Left = 474, Top = 4, Width = 62, Height = 25 };
            buttonMy.Click += (s, e) => EditPattern(p => p.MirrorY());
            var buttonFill = new Button { Text = "Fill all", Left = 540, Top = 4, Width = 56, Height = 25 };
            buttonFill.Click += (s, e) => FillCells(false);
            var buttonFillEmpty = new Button { Text = "Fill empty", Left = 598, Top = 4, Width = 70, Height = 25 };
            buttonFillEmpty.Click += (s, e) => FillCells(true);
            var buttonClear = new Button { Text = "Clear", Left = 670, Top = 4, Width = 50, Height = 25 };
            buttonClear.Click += (s, e) => EditPattern(p => new BrushPattern(p.Width, p.Height));
            var buttonClearStatics = new Button { Text = "No statics", Left = 722, Top = 4, Width = 72, Height = 25 };
            buttonClearStatics.Click += (s, e) => EditPattern(p =>
            {
                BrushPattern c = p.Clone();
                c.Statics.Clear();
                return c;
            });
            var buttonImport = new Button { Text = "Import picture...", Left = 796, Top = 4, Width = 112, Height = 25 };
            buttonImport.Click += OnClickImportPicture;

            _checkMove.Text = "Move (drag a cell onto another)";
            _checkMove.SetBounds(6, 36, 210, 22);
            _checkMove.CheckedChanged += (s, e) => _grid.MoveMode = _checkMove.Checked;
            _checkPaintZ.Text = "Click sets z:";
            _checkPaintZ.SetBounds(220, 36, 96, 22);
            _numericCellZ.SetBounds(318, 35, 52, 23);
            _numericCellZ.Minimum = -128;
            _numericCellZ.Maximum = 127;
            _patternTools.Controls.AddRange(new Control[] { labelPattern, _numericW, _numericH, buttonResize, buttonLeft, buttonRight, buttonUp, buttonDown, buttonRotate, buttonMx, buttonMy,
                buttonFill, buttonFillEmpty, buttonClear, buttonClearStatics, buttonImport, _checkMove, _checkPaintZ, _numericCellZ });

            // textures tools
            _texturesTools.Dock = DockStyle.Top;
            _texturesTools.Height = 34;
            var buttonFixRandom = new Button { Text = "Fix positions (as shown)", Left = 6, Top = 4, Width = 170, Height = 25 };
            buttonFixRandom.Click += (s, e) => FixPositions(true);
            var buttonFixOrder = new Button { Text = "Place in order", Left = 180, Top = 4, Width = 110, Height = 25 };
            buttonFixOrder.Click += (s, e) => FixPositions(false);
            var hintTextures = new Label { Left = 296, Top = 8, AutoSize = true, ForeColor = SystemColors.GrayText, Text = "turns the brush into a pattern: then every texture can be moved to its place" };
            _texturesTools.Controls.AddRange(new Control[] { buttonFixRandom, buttonFixOrder, hintTextures });

            // view options
            var viewBar = new Panel { Dock = DockStyle.Top, Height = 28 };
            _checkNumbers.Text = "Numbers";
            _checkNumbers.Checked = true;
            _checkNumbers.SetBounds(6, 4, 80, 22);
            _checkIds.Text = "Tile ids";
            _checkIds.SetBounds(90, 4, 80, 22);
            _checkNumbers.CheckedChanged += (s, e) => UpdateGrid();
            _checkIds.CheckedChanged += (s, e) => UpdateGrid();
            var hint = new Label
            {
                Left = 176,
                Top = 7,
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Text = "Left = palette tile(s), right = empty, Shift+drag = move, Ctrl+click = take into the palette. Blue = cell under the mouse."
            };
            viewBar.Controls.AddRange(new Control[] { _checkNumbers, _checkIds, hint });

            _textureStrip.Dock = DockStyle.Top;
            _textureStrip.Height = 72;
            _textureStrip.AutoScroll = true;
            _textureStrip.BackColor = Color.FromArgb(24, 24, 28);

            // grid (top view) left, picture like on the map right
            var views = new SplitContainer { Dock = DockStyle.Fill };
            bool splitSet = false;
            views.SizeChanged += (s, e) =>
            {
                // only possible when the control has its size (before: exception)
                if (!splitSet && views.Width > 300)
                {
                    splitSet = true;
                    views.SplitterDistance = views.Width / 2;
                }
            };
            _grid.Dock = DockStyle.Fill;
            _grid.Selection = () => _palette.SelectedIds;
            _grid.Image = id => _palette.GetImage(id);
            _grid.PatternChanged += OnGridChanged;
            _grid.CellPicked += (s, e) =>
            {
                if (e.Tiles.Count > 0)
                {
                    _palette.Select(e.Tiles);
                }
            };
            _grid.MouseDown += (s, e) =>
            {
                // "Click sets z" changes the height of the clicked cell instead of the tile
                if (_checkPaintZ.Checked && _brush?.Pattern != null && e.Button == MouseButtons.Left)
                {
                    _grid.ReadOnly = true;
                    SetCellZ(e.Location);
                }
            };
            _grid.MouseUp += (s, e) => _grid.ReadOnly = false;
            _iso.Dock = DockStyle.Fill;
            _iso.BackColor = Color.FromArgb(18, 18, 22);
            _iso.SizeMode = PictureBoxSizeMode.Normal;
            _iso.Resize += (s, e) => UpdateIso();
            views.Panel1.Controls.Add(_grid);
            views.Panel2.Controls.Add(_iso);
            views.Panel2.Controls.Add(new Label { Dock = DockStyle.Top, Height = 18, Text = "Like on the map (numbers = cells of the grid):", ForeColor = SystemColors.GrayText });
            views.Panel1.Controls.Add(new Label { Dock = DockStyle.Top, Height = 18, Text = "Brush from above (x right, y down):", ForeColor = SystemColors.GrayText });

            center.Controls.Add(views);
            center.Controls.Add(viewBar);
            center.Controls.Add(_textureStrip);
            center.Controls.Add(_texturesTools);
            center.Controls.Add(_patternTools);
            center.Controls.Add(top);
            return center;
        }

        private Control BuildBottom()
        {
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 38, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
            var buttonClose = new Button { Text = "Close", Width = 100 };
            buttonClose.Click += (s, e) => Close();
            var buttonUse = new Button { Text = "Use brush", Width = 120 };
            buttonUse.Click += (s, e) =>
            {
                ApplyFields();
                Result = _brush.Clone();
                if (_modified && MessageBox.Show("Save the brush in the library too?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SaveBrush();
                }

                _modified = false;
                DialogResult = DialogResult.OK;
                Close();
            };
            var buttonHelp = new Button { Text = "Help (F1)", Width = 90 };
            buttonHelp.Click += (s, e) => MapHelpForm.ShowTopic(this, MapHelpContent.BrushEditor);
            bottom.Controls.Add(buttonClose);
            bottom.Controls.Add(buttonUse);
            bottom.Controls.Add(buttonHelp);
            AcceptButton = buttonUse;
            return bottom;
        }

        #endregion

        #region Brush list

        private void FillTree(BrushPreset select)
        {
            string filter = _search.Text.Trim();
            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            TreeNode selected = null;
            foreach (IGrouping<string, BrushPreset> group in _library.Brushes
                         .Where(b => filter.Length == 0 || b.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                         .GroupBy(b => string.IsNullOrEmpty(b.Category) ? "(no category)" : b.Category)
                         .OrderBy(g => g.Key))
            {
                var node = new TreeNode($"{group.Key} ({group.Count()})");
                foreach (BrushPreset b in group.OrderBy(b => b.Name))
                {
                    var child = new TreeNode(b.Name) { Tag = b, ToolTipText = b.Describe() };
                    node.Nodes.Add(child);
                    if (b == select)
                    {
                        selected = child;
                    }
                }

                _tree.Nodes.Add(node);
            }

            _tree.ExpandAll();
            _tree.EndUpdate();
            _tree.ShowNodeToolTips = true;
            if (selected != null)
            {
                _filling = true;
                _tree.SelectedNode = selected;
                _filling = false;
            }

            _comboCategory.Items.Clear();
            _comboCategory.Items.AddRange(_library.Brushes.Select(b => b.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct().OrderBy(c => c).Cast<object>().ToArray());
        }

        private void OnTreeSelect(object sender, TreeViewEventArgs e)
        {
            if (_filling || !(e.Node?.Tag is BrushPreset b) || b == _source)
            {
                return;
            }

            if (!AskSave())
            {
                _filling = true;
                _tree.SelectedNode = FindNode(_source);
                _filling = false;
                return;
            }

            SetBrush(b.Clone(), b);
        }

        private TreeNode FindNode(BrushPreset b)
        {
            return _tree.Nodes.Cast<TreeNode>().SelectMany(n => n.Nodes.Cast<TreeNode>()).FirstOrDefault(n => n.Tag == b);
        }

        /// <summary>false = keep the current brush (cancel)</summary>
        private bool AskSave()
        {
            if (!_modified)
            {
                return true;
            }

            DialogResult r = MessageBox.Show($"\"{_brush.Name}\" was changed. Save it?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
            {
                return false;
            }

            if (r == DialogResult.Yes)
            {
                SaveBrush();
            }

            _modified = false;
            return true;
        }

        private void NewBrush()
        {
            if (!AskSave())
            {
                return;
            }

            SetBrush(new BrushPreset { Name = "New brush", Category = _comboCategory.Text, Mode = BrushMode.Pattern, Pattern = new BrushPattern(4, 4) }, null);
            _modified = true;
            _textName.Focus();
            _textName.SelectAll();
        }

        private void Duplicate()
        {
            ApplyFields();
            BrushPreset copy = _brush.Clone();
            copy.Name += " (copy)";
            SetBrush(copy, null);
            _modified = true;
        }

        private void Rename()
        {
            string name = InputDialog.Show(this, "Rename brush", "Name:", _brush.Name, false);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            _textName.Text = name.Trim();
            if (_source != null)
            {
                _source.Name = name.Trim();
                SaveLibrary();
                FillTree(_source);
            }
        }

        private void ChangeCategory()
        {
            string cat = InputDialog.Show(this, "Category", "Category (empty = none), e.g. Floors, Coast, Roads:", _brush.Category, false);
            if (cat == null)
            {
                return;
            }

            _comboCategory.Text = cat.Trim();
            if (_source != null)
            {
                _source.Category = cat.Trim();
                SaveLibrary();
                FillTree(_source);
            }
        }

        private void DeleteBrush()
        {
            if (_source == null || MessageBox.Show($"Delete \"{_source.DisplayName}\" from the library?", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            _library.Brushes.Remove(_source);
            _source = null;
            _modified = true;
            SaveLibrary();
            FillTree(null);
        }

        private void SaveBrush()
        {
            ApplyFields();
            if (string.IsNullOrWhiteSpace(_brush.Name))
            {
                _brush.Name = "Brush";
            }

            BrushPreset saved = _brush.Clone();
            int index = _source != null ? _library.Brushes.IndexOf(_source) : -1;
            if (index < 0)
            {
                index = _library.Brushes.FindIndex(b => string.Equals(b.Name, saved.Name, StringComparison.OrdinalIgnoreCase) &&
                                                        string.Equals(b.Category ?? string.Empty, saved.Category ?? string.Empty, StringComparison.OrdinalIgnoreCase));
            }

            if (index >= 0)
            {
                _library.Brushes[index] = saved;
            }
            else
            {
                _library.Brushes.Add(saved);
            }

            _source = saved;
            if (SaveLibrary())
            {
                _modified = false;
            }

            FillTree(saved);
        }

        private bool SaveLibrary()
        {
            try
            {
                _library.Save();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Saving failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK && !AskSave())
            {
                e.Cancel = true;
            }
        }

        #endregion

        #region Edit

        private void SetBrush(BrushPreset brush, BrushPreset source)
        {
            _brush = brush;
            _source = source;
            _filling = true;
            _textName.Text = brush.Name;
            _comboCategory.Text = brush.Category ?? string.Empty;
            _radioTextures.Checked = brush.Mode == BrushMode.Textures;
            _radioPattern.Checked = brush.Mode == BrushMode.Pattern;
            _checkLand.Checked = brush.PaintLand;
            _checkStatics.Checked = brush.PaintStatics;
            _checkReplace.Checked = brush.ReplaceStatics;
            _comboHeight.SelectedIndex = (int)brush.Height;
            _comboAnchor.SelectedIndex = (int)brush.Anchor;
            _numericSize.Value = Math.Max(_numericSize.Minimum, Math.Min(_numericSize.Maximum, brush.Size));
            _comboShape.SelectedIndex = (int)brush.Shape;
            if (brush.Mode == BrushMode.Pattern && brush.Pattern == null)
            {
                brush.Pattern = new BrushPattern(brush.Size, brush.Size);
            }

            _numericW.Value = brush.Pattern?.Width ?? brush.Size;
            _numericH.Value = brush.Pattern?.Height ?? brush.Size;
            _filling = false;
            _modified = false;
            UpdateView();
        }

        private void ApplyFields()
        {
            _brush.Name = _textName.Text.Trim();
            _brush.Category = _comboCategory.Text.Trim();
            _brush.Mode = _radioPattern.Checked ? BrushMode.Pattern : BrushMode.Textures;
            _brush.PaintLand = _checkLand.Checked;
            _brush.PaintStatics = _checkStatics.Checked;
            _brush.ReplaceStatics = _checkReplace.Checked;
            _brush.Height = (PatternHeight)Math.Max(0, _comboHeight.SelectedIndex);
            _brush.Anchor = (PatternAnchor)Math.Max(0, _comboAnchor.SelectedIndex);
            _brush.Size = (int)_numericSize.Value;
            _brush.Shape = (BrushShape)Math.Max(0, _comboShape.SelectedIndex);
        }

        private void Changed()
        {
            if (!_filling)
            {
                _modified = true;
            }
        }

        private void OnModeChanged(object sender, EventArgs e)
        {
            if (_filling)
            {
                return;
            }

            _brush.Mode = _radioPattern.Checked ? BrushMode.Pattern : BrushMode.Textures;
            if (_brush.Mode == BrushMode.Pattern && _brush.Pattern == null)
            {
                // start with the textures at the positions the random brush would use
                _brush.Pattern = BrushPattern.FromTextures(_brush.Tiles, (int)_numericSize.Value, (BrushShape)_comboShape.SelectedIndex, true);
            }

            _modified = true;
            UpdateView();
        }

        /// <summary>Brush size or shape: a pattern of the old brush size gets the new size (content stays in the middle)</summary>
        private void OnBrushSizeChanged(object sender, EventArgs e)
        {
            if (_filling || _brush == null)
            {
                return;
            }

            int size = (int)_numericSize.Value;
            if (_brush.Mode == BrushMode.Pattern && _brush.Pattern != null && _brush.Pattern.Width == _brush.Pattern.Height && _brush.Pattern.Width != size &&
                _brush.Pattern.Width == _brush.Size)
            {
                _brush.Pattern = _brush.Pattern.ResizeCentered(size, size);
                _filling = true;
                _numericW.Value = size;
                _numericH.Value = size;
                _filling = false;
            }

            _brush.Size = size;
            _brush.Shape = (BrushShape)Math.Max(0, _comboShape.SelectedIndex);
            _brush.Touch();
            _modified = true;
            UpdateView();
        }

        private void FixPositions(bool asShown)
        {
            if (_brush.Tiles.Count == 0)
            {
                MessageBox.Show("Select textures in the palette first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _brush.Pattern = BrushPattern.FromTextures(_brush.Tiles, (int)_numericSize.Value, (BrushShape)_comboShape.SelectedIndex, asShown);
            _brush.Mode = BrushMode.Pattern;
            _filling = true;
            _radioPattern.Checked = true;
            _numericW.Value = _brush.Pattern.Width;
            _numericH.Value = _brush.Pattern.Height;
            _filling = false;
            _modified = true;
            UpdateView();
        }

        private void UpdateView()
        {
            bool pattern = _brush.Mode == BrushMode.Pattern;
            _patternTools.Visible = pattern;
            _texturesTools.Visible = !pattern;
            _textureStrip.Visible = !pattern;
            _labelInfo.Text = _brush.Describe() + (pattern
                ? "\nEvery cell = one position of the brush. Dark = empty (the ground stays), hatched = outside of a round brush, x2 = variants, z = height, dot = statics."
                : "\nRandom brush: the textures (strip) are spread by position. The grid shows an example. \"Fix positions\" makes a pattern you can arrange.");

            if (!pattern)
            {
                _textureStrip.SuspendLayout();
                foreach (PictureBox c in _textureStrip.Controls.OfType<PictureBox>().ToList())
                {
                    c.Image?.Dispose();
                    c.Dispose();
                }

                _textureStrip.Controls.Clear();
                foreach (ushort id in _brush.Tiles.Distinct())
                {
                    ushort tileId = id;
                    var box = new PictureBox
                    {
                        Width = 56,
                        Height = 56,
                        Margin = new Padding(3),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Image = _palette.GetImage(id) is Bitmap b ? new Bitmap(b) : null,
                        BackColor = Color.DimGray,
                        Cursor = Cursors.Hand
                    };
                    box.Click += (s, e) =>
                    {
                        _brush.Tiles.RemoveAll(t => t == tileId);
                        _brush.Touch();
                        _modified = true;
                        UpdateView();
                    };
                    _textureStrip.Controls.Add(box);
                }

                _textureStrip.ResumeLayout();
            }

            UpdateGrid();
        }

        private void UpdateGrid()
        {
            bool pattern = _brush.Mode == BrushMode.Pattern;
            _grid.Pattern = pattern ? _brush.Pattern : null;
            _grid.RandomTiles = pattern ? null : _brush.Tiles;
            _grid.BrushSize = (int)_numericSize.Value;
            _grid.Shape = (BrushShape)Math.Max(0, _comboShape.SelectedIndex);
            _grid.ShowNumbers = _checkNumbers.Checked;
            _grid.ShowIds = _checkIds.Checked;
            _grid.Invalidate();
            UpdateIso();
        }

        private void UpdateIso()
        {
            if (_brush == null || _iso.ClientSize.Width < 10 || _iso.ClientSize.Height < 10)
            {
                return;
            }

            int size = (int)_numericSize.Value;
            var shape = (BrushShape)Math.Max(0, _comboShape.SelectedIndex);
            BrushPattern p = _brush.Mode == BrushMode.Pattern ? _brush.Pattern : BrushPattern.FromTextures(_brush.Tiles, size, shape, true);
            HashSet<Point> mask = p != null && p.Width == size && p.Height == size
                ? new HashSet<Point>(MapBrush.Cells((size - 1) / 2, (size - 1) / 2, size, shape, size, size))
                : null;
            Image old = _iso.Image;
            _iso.Image = BrushGridControl.RenderIso(p, _art, _iso.ClientSize, _iso.BackColor, _checkNumbers.Checked, mask);
            old?.Dispose();
        }

        private void OnGridChanged(object sender, EventArgs e)
        {
            _brush.Touch();
            _modified = true;
            _labelInfo.Text = _brush.Describe() + "\n" + _labelInfo.Text.Split('\n').Skip(1).FirstOrDefault();
            UpdateIso();
        }

        private void SetCellZ(Point location)
        {
            BrushPattern p = _brush.Pattern;
            // same layout as the grid control: find the cell by asking for the clicked position
            int cols = p.Width, rows = p.Height;
            int cs = Math.Max(8, Math.Min(_grid.MaxCellSize, Math.Min((_grid.ClientSize.Width - 8) / cols, (_grid.ClientSize.Height - 8) / rows)));
            int ox = Math.Max(4, (_grid.ClientSize.Width - (cs * cols)) / 2), oy = Math.Max(4, (_grid.ClientSize.Height - (cs * rows)) / 2);
            int x = (location.X - ox) / cs, y = (location.Y - oy) / cs;
            if (location.X < ox || location.Y < oy || x >= cols || y >= rows)
            {
                return;
            }

            p.SetZ(x, y, (sbyte)_numericCellZ.Value);
            _grid.Invalidate();
            OnGridChanged(this, EventArgs.Empty);
        }

        private void OnPaletteSelectionChanged(object sender, EventArgs e)
        {
            if (_brush == null || _brush.Mode != BrushMode.Textures)
            {
                return;
            }

            _brush.Tiles.Clear();
            _brush.Tiles.AddRange(_palette.SelectedIds);
            _brush.Touch();
            _modified = true;
            UpdateView();
        }

        private void EditPattern(Func<BrushPattern, BrushPattern> edit)
        {
            if (_brush.Pattern == null)
            {
                return;
            }

            _brush.Pattern = edit(_brush.Pattern);
            _filling = true;
            _numericW.Value = _brush.Pattern.Width;
            _numericH.Value = _brush.Pattern.Height;
            if (_brush.Pattern.Width == _brush.Pattern.Height)
            {
                _numericSize.Value = Math.Max(_numericSize.Minimum, Math.Min(_numericSize.Maximum, _brush.Pattern.Width));
                _brush.Size = (int)_numericSize.Value;
            }

            _filling = false;
            _modified = true;
            UpdateView();
        }

        private void FillCells(bool onlyEmpty)
        {
            if (_brush.Pattern == null || _palette.SelectedIds.Count == 0)
            {
                MessageBox.Show("Select one or more tiles in the palette first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            BrushPattern p = _brush.Pattern;
            for (int y = 0; y < p.Height; y++)
            {
                for (int x = 0; x < p.Width; x++)
                {
                    if (!onlyEmpty || p.IsEmpty(x, y))
                    {
                        p.SetIds(x, y, _palette.SelectedIds);
                    }
                }
            }

            _brush.Touch();
            _modified = true;
            UpdateView();
        }

        private void OnClickImportPicture(object sender, EventArgs e)
        {
            using (var dialog = new ImagePatternDialog(_art))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Result != null)
                {
                    _brush.Mode = BrushMode.Pattern;
                    _filling = true;
                    _radioPattern.Checked = true;
                    _filling = false;
                    EditPattern(_ => dialog.Result);
                }
            }
        }

        #endregion
    }
}
