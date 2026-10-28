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

namespace UoFiddler.Controls.MapViewer.Editor.Transitions
{
    /// <summary>
    /// Creates/edits transitions (like "Neue Transition" in UO-Caeri) and terrain groups.
    /// Upper grid: edges (row 1) and corners (row 2) around terrain A, lower grid: inner corners (row 3) around terrain B.
    /// </summary>
    public class TransitionEditorForm : Form
    {
        private readonly TransitionLibrary _library;
        private readonly LandPaletteControl _palette;
        private readonly SlotGrid _grid;
        private readonly ComboBox _comboSets = new ComboBox();
        private readonly ComboBox _comboA = new ComboBox();
        private readonly ComboBox _comboB = new ComboBox();
        private readonly CheckBox _checkRing2 = new CheckBox();
        private readonly Label _labelTitle = new Label();
        private readonly Label _labelMissing = new Label();
        private readonly Label _labelSlot = new Label();
        private readonly ListBox _listGroups = new ListBox();
        private readonly TextBox _textGroupName = new TextBox();
        private readonly TextBox _textGroupIds = new TextBox();
        private bool _modified;
        private bool _filling;

        public TransitionEditorForm(TransitionLibrary library, ArtPixelCache art)
        {
            _library = library;
            Text = "Transitions and terrain groups";
            Icon = Options.GetFiddlerIcon();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(900, 600);
            KeyPreview = true;

            _palette = new LandPaletteControl(art) { Dock = DockStyle.Left, Width = 360 };
            _grid = new SlotGrid(this) { Dock = DockStyle.Fill };

            var tabs = new TabControl { Dock = DockStyle.Fill };
            var tabTransition = new TabPage("Transition");
            var tabGroups = new TabPage("Terrain groups");
            tabs.TabPages.Add(tabTransition);
            tabs.TabPages.Add(tabGroups);

            BuildTransitionTab(tabTransition);
            BuildGroupsTab(tabGroups);

            var splitter = new Splitter { Dock = DockStyle.Left, Width = 4 };
            Controls.Add(tabs);
            Controls.Add(splitter);
            Controls.Add(_palette);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
            var buttonClose = new Button { Text = "Close", Width = 90 };
            buttonClose.Click += (s, e) => Close();
            var buttonHelp = new Button { Text = "Help (F1)", Width = 90 };
            buttonHelp.Click += (s, e) => Help.MapHelpForm.ShowTopic(this, Help.MapHelpContent.Transitions);
            HelpRequested += (s, e) =>
            {
                Help.MapHelpForm.ShowTopic(this, tabs.SelectedIndex == 1 ? Help.MapHelpContent.TerrainGroups : Help.MapHelpContent.Transitions);
                e.Handled = true;
            };
            var buttonSave = new Button { Text = "Save", Width = 90 };
            buttonSave.Click += (s, e) => SaveLibrary();
            var labelFile = new Label { AutoSize = true, Padding = new Padding(0, 7, 12, 0), ForeColor = SystemColors.GrayText, Text = library.FileName ?? string.Empty };
            bottom.Controls.Add(buttonClose);
            bottom.Controls.Add(buttonSave);
            bottom.Controls.Add(buttonHelp);
            bottom.Controls.Add(labelFile);
            Controls.Add(bottom);

            FormClosing += OnClosing;
            Shown += (s, e) =>
            {
                _palette.Reload();
                FillGroups();
                FillSets(_library.Sets.FirstOrDefault());
            };
        }

        /// <summary>the set shown at the end (for the editor)</summary>
        public TransitionSet CurrentSet => _comboSets.SelectedItem as TransitionSet;

        private TransitionSet Set => CurrentSet;

        #region Layout

        private void BuildTransitionTab(TabPage page)
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 64 };
            var labelSet = new Label { Text = "Transition:", Left = 6, Top = 9, AutoSize = true };
            _comboSets.SetBounds(80, 5, 230, 23);
            _comboSets.DropDownStyle = ComboBoxStyle.DropDownList;
            _comboSets.SelectedIndexChanged += (s, e) => ShowSet();
            var buttonNew = new Button { Text = "New", Left = 316, Top = 4, Width = 60 };
            buttonNew.Click += OnClickNewSet;
            var buttonCopy = new Button { Text = "Copy", Left = 380, Top = 4, Width = 60 };
            buttonCopy.Click += OnClickCopySet;
            var buttonRename = new Button { Text = "Rename", Left = 444, Top = 4, Width = 70 };
            buttonRename.Click += OnClickRenameSet;
            var buttonDelete = new Button { Text = "Delete", Left = 518, Top = 4, Width = 64 };
            buttonDelete.Click += OnClickDeleteSet;

            var labelA = new Label { Text = "A (inside):", Left = 6, Top = 39, AutoSize = true };
            _comboA.SetBounds(80, 35, 150, 23);
            _comboA.DropDownStyle = ComboBoxStyle.DropDownList;
            _comboA.SelectedIndexChanged += OnGroupComboChanged;
            var labelB = new Label { Text = "->  B (outside):", Left = 238, Top = 39, AutoSize = true };
            _comboB.SetBounds(336, 35, 150, 23);
            _comboB.DropDownStyle = ComboBoxStyle.DropDownList;
            _comboB.SelectedIndexChanged += OnGroupComboChanged;

            top.Controls.AddRange(new Control[] { labelSet, _comboSets, buttonNew, buttonCopy, buttonRename, buttonDelete, labelA, _comboA, labelB, _comboB });

            var right = new Panel { Dock = DockStyle.Right, Width = 300, Padding = new Padding(8) };
            _labelTitle.SetBounds(8, 8, 284, 24);
            _labelTitle.Font = new Font(Font.FontFamily, 11f, FontStyle.Bold);
            var help = new Label
            {
                Left = 8,
                Top = 36,
                Width = 284,
                Height = 210,
                Text = "Top: row 1 (edges) and row 2 (corners) in the ring around terrain A.\n" +
                       "Below: row 3 (inner corners) around terrain B.\n\n" +
                       "Drag a tile from the palette onto a field, or click a field to take the selected palette tile.\n" +
                       "Right click or drag a field out of the window = delete.\n\n" +
                       "The graphics are placed on the B cells next to A. Several graphics per position (2nd row) are used as random variants."
            };
            _checkRing2.Text = "2nd row (more variants per side and corner)";
            _checkRing2.SetBounds(8, 252, 284, 22);
            _checkRing2.CheckedChanged += (s, e) => _grid.Ring2 = _checkRing2.Checked;
            _labelSlot.SetBounds(8, 282, 284, 60);
            _labelSlot.ForeColor = Color.DarkSlateBlue;
            _labelMissing.SetBounds(8, 346, 284, 120);
            _labelMissing.ForeColor = Color.DarkRed;
            var notesLabel = new Label { Text = "Notes:", Left = 8, Top = 470, AutoSize = true };
            var notes = new TextBox { Left = 8, Top = 488, Width = 284, Height = 120, Multiline = true, ScrollBars = ScrollBars.Vertical, Name = "notes" };
            notes.TextChanged += (s, e) =>
            {
                if (!_filling && Set != null)
                {
                    Set.Notes = notes.Text;
                    _modified = true;
                }
            };
            right.Controls.AddRange(new Control[] { _labelTitle, help, _checkRing2, _labelSlot, _labelMissing, notesLabel, notes });

            page.Controls.Add(_grid);
            page.Controls.Add(right);
            page.Controls.Add(top);
        }

        private void BuildGroupsTab(TabPage page)
        {
            _listGroups.SetBounds(8, 8, 260, 500);
            _listGroups.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            _listGroups.SelectedIndexChanged += (s, e) => ShowGroup();

            var labelName = new Label { Text = "Name:", Left = 280, Top = 10, AutoSize = true };
            _textGroupName.SetBounds(330, 7, 260, 23);
            var labelIds = new Label { Text = "Land tiles (hex with 0x, ranges 0x16-0x19):", Left = 280, Top = 40, AutoSize = true };
            _textGroupIds.SetBounds(280, 60, 420, 120);
            _textGroupIds.Multiline = true;
            _textGroupIds.ScrollBars = ScrollBars.Vertical;

            var buttonApply = new Button { Text = "Apply", Left = 280, Top = 188, Width = 100 };
            buttonApply.Click += OnClickApplyGroup;
            var buttonNew = new Button { Text = "New group", Left = 386, Top = 188, Width = 100 };
            buttonNew.Click += (s, e) =>
            {
                _listGroups.ClearSelected();
                _textGroupName.Text = "New group";
                _textGroupIds.Text = TransitionSet.FormatIds(_palette.SelectedIds);
                _textGroupName.Focus();
                _textGroupName.SelectAll();
            };
            var buttonFromPalette = new Button { Text = "Palette selection -> ids", Left = 492, Top = 188, Width = 170 };
            buttonFromPalette.Click += (s, e) => _textGroupIds.Text = TransitionSet.FormatIds(_palette.SelectedIds);
            var buttonShow = new Button { Text = "Show in palette", Left = 280, Top = 220, Width = 140 };
            buttonShow.Click += (s, e) =>
            {
                try
                {
                    _palette.Select(TransitionSet.ParseIds(_textGroupIds.Text));
                }
                catch (FormatException ex)
                {
                    MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            var buttonDelete = new Button { Text = "Delete group", Left = 426, Top = 220, Width = 110 };
            buttonDelete.Click += OnClickDeleteGroup;
            var buttonGuess = new Button { Text = "Create groups from tile names (tiledata)", Left = 280, Top = 268, Width = 300 };
            buttonGuess.Click += (s, e) =>
            {
                int n = _library.AddGroupsFromTileNames();
                _modified |= n > 0;
                FillGroups();
                MessageBox.Show($"{n} group(s) added. Check them: transition tiles often have the same name as the terrain.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            var hint = new Label
            {
                Left = 280,
                Top = 304,
                Width = 420,
                Height = 80,
                ForeColor = SystemColors.GrayText,
                Text = "A terrain group is the set of plain tiles of one terrain (e.g. Sand 0x16-0x19).\n" +
                       "The terrain tool paints these tiles randomly; for the transition automatic they count as A or B."
            };

            page.Controls.AddRange(new Control[] { _listGroups, labelName, _textGroupName, labelIds, _textGroupIds, buttonApply, buttonNew, buttonFromPalette, buttonShow, buttonDelete, buttonGuess, hint });
        }

        #endregion

        #region Sets

        private void FillSets(TransitionSet select)
        {
            _filling = true;
            _comboSets.BeginUpdate();
            _comboSets.Items.Clear();
            _comboSets.Items.AddRange(_library.Sets.Cast<object>().ToArray());
            _comboSets.EndUpdate();
            _filling = false;
            _comboSets.SelectedItem = select;
            if (_comboSets.SelectedIndex < 0 && _comboSets.Items.Count > 0)
            {
                _comboSets.SelectedIndex = 0;
            }

            ShowSet();
        }

        private void FillGroups()
        {
            _filling = true;
            object a = _comboA.SelectedItem, b = _comboB.SelectedItem, g = _listGroups.SelectedItem;
            string[] names = _library.Groups.Select(x => x.Name).ToArray();
            foreach (ComboBox combo in new[] { _comboA, _comboB })
            {
                combo.BeginUpdate();
                combo.Items.Clear();
                combo.Items.AddRange(names.Cast<object>().ToArray());
                combo.EndUpdate();
            }

            _listGroups.BeginUpdate();
            _listGroups.Items.Clear();
            _listGroups.Items.AddRange(_library.Groups.Cast<object>().ToArray());
            _listGroups.EndUpdate();
            _comboA.SelectedItem = a;
            _comboB.SelectedItem = b;
            _listGroups.SelectedItem = g;
            _filling = false;
            ShowSet();
        }

        private void ShowSet()
        {
            if (_filling)
            {
                return;
            }

            TransitionSet set = Set;
            _filling = true;
            _comboA.SelectedItem = set?.GroupA;
            _comboB.SelectedItem = set?.GroupB;
            _comboA.Enabled = _comboB.Enabled = set != null;
            Control notes = Controls.Find("notes", true).FirstOrDefault();
            if (notes != null)
            {
                notes.Text = set?.Notes ?? string.Empty;
                notes.Enabled = set != null;
            }

            _filling = false;
            _checkRing2.Checked = set != null && TransitionSet.AllSlots.Any(s => set.Slots[s].Count > 1);
            _labelTitle.Text = set == null ? "No transition - press \"New\"" : $"{set.GroupA}  ->  {set.GroupB}";
            UpdateMissing();
            _grid.Invalidate();
        }

        private void UpdateMissing()
        {
            TransitionSet set = Set;
            if (set == null)
            {
                _labelMissing.Text = string.Empty;
                return;
            }

            List<TransitionSlot> missing = set.MissingSlots();
            var text = new List<string>();
            if (_library.GroupIds(set.GroupA).Count == 0)
            {
                text.Add($"Group A \"{set.GroupA}\" has no tiles.");
            }

            if (_library.GroupIds(set.GroupB).Count == 0)
            {
                text.Add($"Group B \"{set.GroupB}\" has no tiles.");
            }

            text.Add(missing.Count == 0
                ? "All positions are filled."
                : "Empty: " + string.Join(", ", missing.Select(TransitionSet.Label)) + "\n(these cells stay unchanged)");
            _labelMissing.Text = string.Join("\n", text);
        }

        private void OnGroupComboChanged(object sender, EventArgs e)
        {
            if (_filling || Set == null)
            {
                return;
            }

            Set.GroupA = _comboA.SelectedItem as string ?? Set.GroupA;
            Set.GroupB = _comboB.SelectedItem as string ?? Set.GroupB;
            if (string.IsNullOrEmpty(Set.Name) || Set.Name.Contains("->"))
            {
                Set.Name = $"{Set.GroupA} -> {Set.GroupB}";
            }

            _modified = true;
            int index = _comboSets.SelectedIndex;
            _filling = true;
            _comboSets.Items[index] = Set;
            _filling = false;
            ShowSet();
        }

        private void OnClickNewSet(object sender, EventArgs e)
        {
            if (_library.Groups.Count < 2)
            {
                MessageBox.Show("Create at least two terrain groups first (tab \"Terrain groups\").", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var set = new TransitionSet { GroupA = _library.Groups[0].Name, GroupB = _library.Groups[1].Name };
            set.Name = $"{set.GroupA} -> {set.GroupB}";
            _library.Sets.Add(set);
            _modified = true;
            FillSets(set);
        }

        private void OnClickCopySet(object sender, EventArgs e)
        {
            if (Set == null)
            {
                return;
            }

            TransitionSet copy = Set.Clone();
            copy.Name += " (copy)";
            _library.Sets.Add(copy);
            _modified = true;
            FillSets(copy);
        }

        private void OnClickRenameSet(object sender, EventArgs e)
        {
            if (Set == null)
            {
                return;
            }

            string name = InputDialog.Show(this, "Rename", "Name of the transition:", Set.Name, false);
            if (!string.IsNullOrWhiteSpace(name))
            {
                Set.Name = name.Trim();
                _modified = true;
                FillSets(Set);
            }
        }

        private void OnClickDeleteSet(object sender, EventArgs e)
        {
            if (Set == null || MessageBox.Show($"Delete \"{Set}\"?", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            _library.Sets.Remove(Set);
            _modified = true;
            FillSets(_library.Sets.FirstOrDefault());
        }

        #endregion

        #region Groups

        private void ShowGroup()
        {
            if (_listGroups.SelectedItem is TerrainGroup g)
            {
                _textGroupName.Text = g.Name;
                _textGroupIds.Text = TransitionSet.FormatIds(g.Ids);
            }
        }

        private void OnClickApplyGroup(object sender, EventArgs e)
        {
            string name = _textGroupName.Text.Trim();
            if (name.Length == 0)
            {
                return;
            }

            List<ushort> ids;
            try
            {
                ids = TransitionSet.ParseIds(_textGroupIds.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TerrainGroup group = _listGroups.SelectedItem as TerrainGroup;
            TerrainGroup sameName = _library.GetGroup(name);
            if (group == null)
            {
                group = sameName ?? new TerrainGroup();
                if (sameName == null)
                {
                    _library.Groups.Add(group);
                }
            }
            else if (sameName != null && sameName != group)
            {
                MessageBox.Show($"A group \"{name}\" exists already.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // keep the references of the transitions
            if (!string.IsNullOrEmpty(group.Name) && group.Name != name)
            {
                foreach (TransitionSet s in _library.Sets)
                {
                    if (s.GroupA == group.Name)
                    {
                        s.GroupA = name;
                    }

                    if (s.GroupB == group.Name)
                    {
                        s.GroupB = name;
                    }
                }
            }

            group.Name = name;
            group.Ids.Clear();
            group.Ids.AddRange(ids.Distinct());
            _modified = true;
            FillGroups();
            _listGroups.SelectedItem = group;
            FillSets(Set);
        }

        private void OnClickDeleteGroup(object sender, EventArgs e)
        {
            if (!(_listGroups.SelectedItem is TerrainGroup g))
            {
                return;
            }

            int used = _library.Sets.Count(s => s.GroupA == g.Name || s.GroupB == g.Name);
            if (MessageBox.Show($"Delete the group \"{g.Name}\"?" + (used > 0 ? $"\nIt is used by {used} transition(s)." : string.Empty), Text,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            _library.Groups.Remove(g);
            _modified = true;
            FillGroups();
        }

        #endregion

        #region Save / close

        private bool SaveLibrary()
        {
            try
            {
                _library.Save();
                _modified = false;
                _library.OnChanged();
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
            if (!_modified)
            {
                return;
            }

            DialogResult r = MessageBox.Show("Save the transitions and groups?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel || (r == DialogResult.Yes && !SaveLibrary()))
            {
                e.Cancel = true;
                return;
            }

            if (r == DialogResult.No)
            {
                // read the file again, the changes are dropped
                try
                {
                    if (System.IO.File.Exists(_library.FileName))
                    {
                        _library.Load(_library.FileName);
                    }
                    else
                    {
                        _library.Groups.Clear();
                        _library.Sets.Clear();
                    }
                }
                catch (Exception)
                {
                    // keep what is in memory
                }

                _library.OnChanged();
            }
        }

        #endregion

        #region Slot grid

        /// <summary>The two grids with the drop fields</summary>
        private sealed class SlotGrid : Control
        {
            private const int Cell = 72;
            private const int Gap = 6;
            private readonly TransitionEditorForm _owner;
            private bool _ring2;
            private SlotField _down;
            private SlotField _hover;

            public SlotGrid(TransitionEditorForm owner)
            {
                _owner = owner;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
                BackColor = Color.FromArgb(16, 16, 20);
                AllowDrop = true;
            }

            public bool Ring2
            {
                get => _ring2;
                set
                {
                    _ring2 = value;
                    Invalidate();
                }
            }

            private sealed class SlotField
            {
                public Rectangle Bounds;
                public TransitionSlot? Slot;
                public int Variant;

                /// <summary>0 = slot, 1 = center A, 2 = center B</summary>
                public int Center;
            }

            private List<SlotField> Layout()
            {
                var fields = new List<SlotField>();
                int n = _ring2 ? 5 : 3, h = n / 2;
                int x0 = 12, y0 = 12;
                int y1 = y0 + (n * (Cell + Gap)) + 28;
                for (int j = 0; j < n; j++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        int dx = i - h, dy = j - h;
                        var rect = new Rectangle(x0 + (i * (Cell + Gap)), y0 + (j * (Cell + Gap)), Cell, Cell);
                        if (dx == 0 && dy == 0)
                        {
                            fields.Add(new SlotField { Bounds = rect, Center = 1 });
                        }
                        else if (UpperSlot(dx, dy, out TransitionSlot s, out int v))
                        {
                            fields.Add(new SlotField { Bounds = rect, Slot = s, Variant = v });
                        }

                        var rectB = new Rectangle(rect.X, y1 + (j * (Cell + Gap)), Cell, Cell);
                        if (dx == 0 && dy == 0)
                        {
                            fields.Add(new SlotField { Bounds = rectB, Center = 2 });
                        }
                        else if (LowerSlot(dx, dy, out TransitionSlot s2, out int v2))
                        {
                            fields.Add(new SlotField { Bounds = rectB, Slot = s2, Variant = v2 });
                        }
                    }
                }

                return fields;
            }

            private static bool UpperSlot(int dx, int dy, out TransitionSlot slot, out int variant)
            {
                variant = 0;
                slot = TransitionSlot.EdgeN;
                int ring = Math.Max(Math.Abs(dx), Math.Abs(dy));
                if (ring == 1)
                {
                    slot = (dx, dy) switch
                    {
                        (-1, -1) => TransitionSlot.CornerNW,
                        (0, -1) => TransitionSlot.EdgeN,
                        (1, -1) => TransitionSlot.CornerNE,
                        (1, 0) => TransitionSlot.EdgeE,
                        (1, 1) => TransitionSlot.CornerSE,
                        (0, 1) => TransitionSlot.EdgeS,
                        (-1, 1) => TransitionSlot.CornerSW,
                        _ => TransitionSlot.EdgeW
                    };
                    return true;
                }

                if (Math.Abs(dx) == 2 && Math.Abs(dy) == 2)
                {
                    slot = dx < 0 ? (dy < 0 ? TransitionSlot.CornerNW : TransitionSlot.CornerSW) : (dy < 0 ? TransitionSlot.CornerNE : TransitionSlot.CornerSE);
                    variant = 1;
                    return true;
                }

                if (dy == -2 || dy == 2)
                {
                    slot = dy < 0 ? TransitionSlot.EdgeN : TransitionSlot.EdgeS;
                    variant = dx + 2;
                    return true;
                }

                slot = dx < 0 ? TransitionSlot.EdgeW : TransitionSlot.EdgeE;
                variant = dy + 2;
                return true;
            }

            private static bool LowerSlot(int dx, int dy, out TransitionSlot slot, out int variant)
            {
                variant = 0;
                slot = TransitionSlot.InnerNW;
                int ring = Math.Max(Math.Abs(dx), Math.Abs(dy));
                if (ring == 1)
                {
                    slot = (dx, dy) switch
                    {
                        (-1, -1) => TransitionSlot.InnerNW,
                        (0, -1) => TransitionSlot.BEdgeN,
                        (1, -1) => TransitionSlot.InnerNE,
                        (1, 0) => TransitionSlot.BEdgeE,
                        (1, 1) => TransitionSlot.InnerSE,
                        (0, 1) => TransitionSlot.BEdgeS,
                        (-1, 1) => TransitionSlot.InnerSW,
                        _ => TransitionSlot.BEdgeW
                    };
                    return true;
                }

                if (Math.Abs(dx) == 2 && Math.Abs(dy) == 2)
                {
                    slot = dx < 0 ? (dy < 0 ? TransitionSlot.InnerNW : TransitionSlot.InnerSW) : (dy < 0 ? TransitionSlot.InnerNE : TransitionSlot.InnerSE);
                    variant = 1;
                    return true;
                }

                return false;
            }

            private SlotField FieldAt(Point p) => Layout().FirstOrDefault(f => f.Bounds.Contains(p));

            private ushort? IdOf(SlotField f)
            {
                TransitionSet set = _owner.Set;
                if (set == null)
                {
                    return null;
                }

                if (f.Center != 0)
                {
                    IReadOnlyList<ushort> ids = _owner._library.GroupIds(f.Center == 1 ? set.GroupA : set.GroupB);
                    return ids.Count > 0 ? ids[0] : (ushort?)null;
                }

                return set.Get(f.Slot.Value, f.Variant);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(BackColor);
                TransitionSet set = _owner.Set;
                List<SlotField> fields = Layout();
                int n = _ring2 ? 5 : 3;
                using (var font = new Font("Consolas", 9f))
                using (var border = new Pen(Color.Goldenrod, 1))
                using (var centerPen = new Pen(Color.RoyalBlue, 3))
                using (var hoverPen = new Pen(Color.White, 2))
                using (var empty = new SolidBrush(Color.FromArgb(26, 30, 52)))
                using (var outer = new SolidBrush(Color.FromArgb(70, 120, 90)))
                {
                    g.DrawString(set == null ? string.Empty : $"Around A ({set.GroupA})", font, Brushes.Gainsboro, 12, 0);
                    g.DrawString(set == null ? string.Empty : $"Around B ({set.GroupB})", font, Brushes.Gainsboro, 12, 12 + (n * (Cell + Gap)) + 12);

                    foreach (SlotField f in fields)
                    {
                        ushort? id = IdOf(f);
                        Bitmap bmp = id.HasValue ? _owner._palette.GetImage(id.Value) : null;
                        bool isRing2 = f.Slot.HasValue && f.Variant > 0;
                        g.FillRectangle(bmp == null && isRing2 ? outer : empty, f.Bounds);
                        if (bmp != null)
                        {
                            g.DrawImage(bmp, f.Bounds);
                        }

                        g.DrawRectangle(f.Center != 0 ? centerPen : border, f.Bounds);
                        if (f == _hover || (_hover != null && f.Slot == _hover.Slot && f.Variant == _hover.Variant && f.Center == _hover.Center))
                        {
                            g.DrawRectangle(hoverPen, Rectangle.Inflate(f.Bounds, -2, -2));
                        }

                        string label = f.Center == 1 ? "A" : f.Center == 2 ? "B" : TransitionSet.Label(f.Slot.Value);
                        if (bmp == null || f.Center != 0)
                        {
                            g.DrawString(label, font, Brushes.Khaki, f.Bounds.X + 3, f.Bounds.Y + 2);
                        }

                        if (id.HasValue && f.Center == 0)
                        {
                            g.DrawString($"0x{id.Value:X}", font, Brushes.White, f.Bounds.X + 3, f.Bounds.Bottom - 15);
                        }
                    }
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                SlotField f = FieldAt(e.Location);
                if (f?.Bounds != _hover?.Bounds)
                {
                    _hover = f;
                    Invalidate();
                    _owner._labelSlot.Text = f == null ? string.Empty
                        : f.Center == 1 ? "A: plain tiles of the inside terrain (click = take the palette selection as group A tiles)"
                        : f.Center == 2 ? "B: plain tiles of the outside terrain (click = take the palette selection as group B tiles)"
                        : $"{TransitionSet.Label(f.Slot.Value)}{(f.Variant > 0 ? $" variant {f.Variant + 1}" : string.Empty)}: {TransitionSet.Describe(f.Slot.Value)}";
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                _down = FieldAt(e.Location);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                SlotField down = _down;
                _down = null;
                TransitionSet set = _owner.Set;
                if (down == null || set == null)
                {
                    return;
                }

                // dragged out of the control = delete
                if (!ClientRectangle.Contains(e.Location) && down.Center == 0)
                {
                    SetSlot(down, null);
                    return;
                }

                SlotField up = FieldAt(e.Location);
                if (up == null || up.Bounds != down.Bounds)
                {
                    return;
                }

                if (e.Button == MouseButtons.Right)
                {
                    if (down.Center == 0)
                    {
                        SetSlot(down, null);
                    }

                    return;
                }

                IReadOnlyList<ushort> sel = _owner._palette.SelectedIds;
                if (down.Center != 0)
                {
                    if (sel.Count == 0)
                    {
                        return;
                    }

                    string groupName = down.Center == 1 ? set.GroupA : set.GroupB;
                    TerrainGroup group = _owner._library.GetGroup(groupName);
                    if (group != null && MessageBox.Show($"Use the {sel.Count} selected palette tile(s) as tiles of the group \"{groupName}\"?", _owner.Text,
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        group.Ids.Clear();
                        group.Ids.AddRange(sel);
                        _owner._modified = true;
                        _owner.FillGroups();
                    }

                    return;
                }

                if (sel.Count > 0)
                {
                    SetSlot(down, sel[0]);
                }
                else if (IdOf(down).HasValue)
                {
                    _owner._palette.Select(new[] { IdOf(down).Value });
                }
            }

            private void SetSlot(SlotField f, ushort? id)
            {
                TransitionSet set = _owner.Set;
                List<ushort> list = set.Slots[f.Slot.Value];
                if (id.HasValue && f.Variant > list.Count)
                {
                    // variant field further right than the existing ones: append
                    list.Add(id.Value);
                }
                else
                {
                    set.Set(f.Slot.Value, f.Variant, id);
                }

                _owner._modified = true;
                _owner.UpdateMissing();
                Invalidate();
            }

            protected override void OnDragOver(DragEventArgs e)
            {
                base.OnDragOver(e);
                SlotField f = FieldAt(PointToClient(new Point(e.X, e.Y)));
                e.Effect = f != null && f.Center == 0 && _owner.Set != null && LandPaletteControl.TryGetDragId(e.Data, out _) ? DragDropEffects.Copy : DragDropEffects.None;
                if (f?.Bounds != _hover?.Bounds)
                {
                    _hover = f;
                    Invalidate();
                }
            }

            protected override void OnDragDrop(DragEventArgs e)
            {
                base.OnDragDrop(e);
                SlotField f = FieldAt(PointToClient(new Point(e.X, e.Y)));
                if (f != null && f.Center == 0 && _owner.Set != null && LandPaletteControl.TryGetDragId(e.Data, out ushort id))
                {
                    SetSlot(f, id);
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                _hover = null;
                Invalidate();
            }
        }

        #endregion
    }
}
