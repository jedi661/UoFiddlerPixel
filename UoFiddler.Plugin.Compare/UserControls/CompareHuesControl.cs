﻿/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Compare hues: the loaded hues.mul (Hues 1) with a second hues.mul (Hues 2).
 *  - kind of difference (colours / name / table values / only in one file), counter, filter
 *  - previous / next difference, multi selection
 *  - detail: both colour strips with the different steps, preview of both hues
 *  - apply 2 -> 1 and 1 -> 2 (colours / name / everything), also for all differences, undo/redo
 *  - save Hues 1 (output folder) and Hues 2 (any file, backup), export the differences (CSV)
 *
 * The numbers are hue numbers of the game (file index + 1), as in the hues tab.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareHuesControl : UserControl
    {
        [Flags]
        private enum DiffKind
        {
            Same = 0,
            Colors = 1,
            Name = 2,
            Table = 4,
            OnlyIn1 = 8,
            OnlyIn2 = 16
        }

        [Flags]
        private enum Parts
        {
            Colors = 1,
            Name = 2,
            Table = 4,
            All = Colors | Name | Table
        }

        private static readonly string[] _filterNames =
        {
            "All", "Differences (any)", "Colours differ", "Name differs", "Table values differ", "Only in Hues 1", "Only in Hues 2", "Selected"
        };

        private const int _itemHeight = 20;
        private const int _numberWidth = 92;
        private const int _nameWidth = 150;

        private bool _loaded;
        private bool _hue2Loaded;
        private string _secondFile;
        private int _count1;
        private int _count2;
        private DiffKind[] _diff = Array.Empty<DiffKind>();
        private List<int> _view = new List<int>();
        private int _selected;
        private readonly HashSet<int> _multi = new HashSet<int>();
        private bool _secondChanged;

        private readonly HueHistory _history2 = new HueHistory(() => SecondHue.List);
        private readonly List<HueHistory> _undoOrder = new List<HueHistory>();
        private readonly List<HueHistory> _redoOrder = new List<HueHistory>();

        // controls
        private ToolStripTextBox _txtPath;
        private ToolStripComboBox _cmbFilter;
        private ToolStripLabel _lblCount;
        private ToolStripButton _btnUndo;
        private ToolStripButton _btnRedo;
        private PictureBox _list;
        private VScrollBar _scroll;
        private PictureBox _detail;
        private Label _detailText;
        private HuePreviewPanel _preview;
        private readonly ToolTip _tip = new ToolTip();

        public CompareHuesControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            BuildUi();
        }

        #region UI

        private void BuildUi()
        {
            // ── file / navigation ─────────────────────────────────────
            var bar1 = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, RenderMode = ToolStripRenderMode.System, Padding = new Padding(2) };
            _txtPath = new ToolStripTextBox { AutoSize = false, Width = 260, ToolTipText = "Folder with hues.mul or a hues.mul file (e.g. a backup)" };
            _txtPath.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    LoadSecond();
                }
            };
            _cmbFilter = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 150 };
            _cmbFilter.Items.AddRange(_filterNames.Cast<object>().ToArray());
            _cmbFilter.SelectedIndex = 0;
            _cmbFilter.SelectedIndexChanged += (s, e) => ApplyFilter();
            _lblCount = new ToolStripLabel { ForeColor = Color.DarkRed, Font = new Font(Font, FontStyle.Bold) };

            bar1.Items.Add(new ToolStripLabel("Hues 2:"));
            bar1.Items.Add(_txtPath);
            bar1.Items.Add(Button("Folder…", (s, e) => BrowseFolder(), "Folder that contains hues.mul"));
            bar1.Items.Add(Button("File…", (s, e) => BrowseFile(), "Any hues.mul file"));
            bar1.Items.Add(Button("Load", (s, e) => LoadSecond(), null));
            bar1.Items.Add(new ToolStripSeparator());
            bar1.Items.Add(new ToolStripLabel("Show:"));
            bar1.Items.Add(_cmbFilter);
            bar1.Items.Add(Button("◄ Prev", (s, e) => JumpDiff(-1), "Previous difference (Shift+F3)"));
            bar1.Items.Add(Button("Next ►", (s, e) => JumpDiff(1), "Next difference (F3)"));
            bar1.Items.Add(_lblCount);

            // ── apply / save ──────────────────────────────────────────
            var bar2 = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, RenderMode = ToolStripRenderMode.System, Padding = new Padding(2) };
            var apply21 = new ToolStripSplitButton("Apply 2 → 1") { ToolTipText = "Copy the selected hues of Hues 2 into Hues 1 (colours, name and table values)" };
            apply21.ButtonClick += (s, e) => Apply(true, Parts.All, SelectedIndices);
            apply21.DropDownItems.Add("only colours", null, (s, e) => Apply(true, Parts.Colors | Parts.Table, SelectedIndices));
            apply21.DropDownItems.Add("only name", null, (s, e) => Apply(true, Parts.Name, SelectedIndices));
            apply21.DropDownItems.Add("all differences (shown filter)", null, (s, e) => Apply(true, Parts.All, DiffIndicesInView()));

            var apply12 = new ToolStripSplitButton("Apply 1 → 2") { ToolTipText = "Copy the selected hues of Hues 1 into Hues 2" };
            apply12.ButtonClick += (s, e) => Apply(false, Parts.All, SelectedIndices);
            apply12.DropDownItems.Add("only colours", null, (s, e) => Apply(false, Parts.Colors | Parts.Table, SelectedIndices));
            apply12.DropDownItems.Add("only name", null, (s, e) => Apply(false, Parts.Name, SelectedIndices));
            apply12.DropDownItems.Add("all differences (shown filter)", null, (s, e) => Apply(false, Parts.All, DiffIndicesInView()));

            _btnUndo = Button("↶ Undo", (s, e) => DoUndo(), "Undo (Ctrl+Z)");
            _btnRedo = Button("↷ Redo", (s, e) => DoRedo(), "Redo (Ctrl+Y)");

            bar2.Items.Add(apply21);
            bar2.Items.Add(apply12);
            bar2.Items.Add(_btnUndo);
            bar2.Items.Add(_btnRedo);
            bar2.Items.Add(new ToolStripSeparator());
            bar2.Items.Add(Button("Save Hues 1", (s, e) => SaveFirst(), "Write the loaded hues (Hues 1) into the output folder"));
            bar2.Items.Add(Button("Save Hues 2…", (s, e) => SaveSecond(), "Write Hues 2 into a hues.mul (backup .bak)"));
            bar2.Items.Add(Button("Export differences…", (s, e) => ExportDiff(), "List of all differences as CSV"));

            // ── list ──────────────────────────────────────────────────
            _list = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.White };
            _list.Paint += OnPaintList;
            _list.Resize += (s, e) => { UpdateScroll(); _list.Invalidate(); };
            _list.MouseDown += OnListMouseDown;
            _list.MouseWheel += (s, e) => ScrollTo(_scroll.Value + (e.Delta < 0 ? 3 : -3));
            _scroll = new VScrollBar { Dock = DockStyle.Right };
            _scroll.Scroll += (s, e) => _list.Invalidate();
            _list.ContextMenuStrip = BuildContextMenu();

            var listPanel = new Panel { Dock = DockStyle.Fill };
            listPanel.Controls.Add(_list);
            listPanel.Controls.Add(_scroll);

            // ── detail ────────────────────────────────────────────────
            _detail = new PictureBox { Dock = DockStyle.Top, Height = 74 };
            _detail.Paint += OnPaintDetail;
            _detail.Resize += (s, e) => _detail.Invalidate();
            _detailText = new Label { Dock = DockStyle.Fill, Font = new Font("Consolas", 8.5f), Padding = new Padding(6, 2, 2, 2) };
            var detailLeft = new Panel { Dock = DockStyle.Fill };
            detailLeft.Controls.Add(_detailText);
            detailLeft.Controls.Add(_detail);

            _preview = new HuePreviewPanel { Dock = DockStyle.Right, Width = 420 };

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterWidth = 5 };
            split.Panel1.Controls.Add(listPanel);
            split.Panel2.Controls.Add(detailLeft);
            split.Panel2.Controls.Add(_preview);
            HandleCreated += (s, e) =>
            {
                try
                {
                    split.SplitterDistance = Math.Max(150, Height - 300);
                }
                catch (InvalidOperationException)
                {
                    // ignored (control too small)
                }
            };

            Controls.Add(split);
            Controls.Add(bar2);
            Controls.Add(bar1);
            UpdateUndoButtons();
        }

        private ToolStripButton Button(string text, EventHandler click, string tip)
        {
            var b = new ToolStripButton(text) { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = tip };
            b.Click += click;
            return b;
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Apply 2 → 1 (everything)", null, (s, e) => Apply(true, Parts.All, SelectedIndices));
            menu.Items.Add("Apply 2 → 1 (only colours)", null, (s, e) => Apply(true, Parts.Colors | Parts.Table, SelectedIndices));
            menu.Items.Add("Apply 2 → 1 (only name)", null, (s, e) => Apply(true, Parts.Name, SelectedIndices));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Apply 1 → 2 (everything)", null, (s, e) => Apply(false, Parts.All, SelectedIndices));
            menu.Items.Add("Apply 1 → 2 (only colours)", null, (s, e) => Apply(false, Parts.Colors | Parts.Table, SelectedIndices));
            menu.Items.Add("Apply 1 → 2 (only name)", null, (s, e) => Apply(false, Parts.Name, SelectedIndices));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Copy hue number", null, (s, e) => CopyText($"{_selected + 1} (0x{_selected + 1:X})"));
            menu.Items.Add("Copy for ServUO (Hue = 0x…)", null, (s, e) => CopyText(HueFiles.ServUOText(Hues.List[_selected])));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Undo (Ctrl+Z)", null, (s, e) => DoUndo());
            menu.Items.Add("Redo (Ctrl+Y)", null, (s, e) => DoRedo());
            menu.Opening += (s, e) =>
            {
                bool ok = _hue2Loaded;
                for (int i = 0; i < 7; i++)
                {
                    menu.Items[i].Enabled = ok;
                }
            };
            return menu;
        }

        #endregion

        #region Load / compare

        private void OnLoad(object sender, EventArgs e)
        {
            if (_loaded)
            {
                return;
            }

            Options.LoadedUltimaClass["Hues"] = true;
            _count1 = HueFiles.CountInFile(Files.GetFilePath("hues.mul"));
            if (_count1 == 0)
            {
                _count1 = Hues.List.Length;
            }

            ControlEvents.HueChangeEvent += OnHueChangeEvent;
            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            HueHistory.Main.Changed += OnHistoryChanged;
            HueUsageIndex.Built += OnUsageBuilt;
            _loaded = true;
            Recompute();
            if (!HueUsageIndex.IsBuilt && !HueUsageIndex.IsBuilding)
            {
                HueUsageIndex.BuildAsync();
            }
        }

        private void DisposeExtras()
        {
            if (_loaded)
            {
                ControlEvents.HueChangeEvent -= OnHueChangeEvent;
                ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
                HueHistory.Main.Changed -= OnHistoryChanged;
                HueUsageIndex.Built -= OnUsageBuilt;
            }

            _tip.Dispose();
        }

        private void OnHueChangeEvent()
        {
            if (IsDisposed)
            {
                return;
            }

            Recompute();
        }

        private void OnFilePathChangeEvent()
        {
            _count1 = HueFiles.CountInFile(Files.GetFilePath("hues.mul"));
            Recompute();
        }

        private void OnHistoryChanged(object sender, EventArgs e) => UpdateUndoButtons();

        private void OnUsageBuilt(object sender, EventArgs e)
        {
            if (!IsDisposed && IsHandleCreated)
            {
                BeginInvoke(new Action(UpdateDetail));
            }
        }

        private void BrowseFolder()
        {
            using (var dialog = new FolderBrowserDialog { Description = "Select directory containing the hue file", ShowNewFolderButton = false })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _txtPath.Text = dialog.SelectedPath;
                    LoadSecond();
                }
            }
        }

        private void BrowseFile()
        {
            using (var dialog = new OpenFileDialog { Filter = "hues.mul|hues*.mul;*.mul|All files|*.*", Title = "Second hues.mul" })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _txtPath.Text = dialog.FileName;
                    LoadSecond();
                }
            }
        }

        private void LoadSecond()
        {
            string path = _txtPath.Text?.Trim().Trim('"') ?? string.Empty;
            string file = Directory.Exists(path) ? Path.Combine(path, "hues.mul") : path;
            if (!File.Exists(file))
            {
                MessageBox.Show($"hues.mul not found:\n{file}", "Compare hues", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_secondChanged && MessageBox.Show("Hues 2 has unsaved changes. Load another file anyway?", "Compare hues",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SecondHue.Initialize(file);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loading failed:\n{ex.Message}", "Compare hues", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _secondFile = file;
            _count2 = HueFiles.CountInFile(file);
            _hue2Loaded = true;
            _secondChanged = false;

            // the undo steps of the old file do not fit anymore (before: the old compare results were kept, too)
            _history2.Clear();
            _undoOrder.RemoveAll(h => h == _history2);
            _redoOrder.RemoveAll(h => h == _history2);
            _multi.Clear();
            _selected = 0;
            _scroll.Value = 0;
            Recompute();
            UpdateUndoButtons();
        }

        private DiffKind Compare(int index)
        {
            if (!_hue2Loaded)
            {
                return DiffKind.Same;
            }

            Hue a = Hues.List[index];
            Hue b = index < SecondHue.List.Length ? SecondHue.List[index] : null;
            bool in1 = index < _count1, in2 = index < _count2 && b != null;

            if (in1 && !in2)
            {
                return HueAnalysis.IsEmpty(a) ? DiffKind.Same : DiffKind.OnlyIn1;
            }

            if (!in1 && in2)
            {
                return HueAnalysis.IsEmpty(b) ? DiffKind.Same : DiffKind.OnlyIn2;
            }

            if (a == null || b == null)
            {
                return DiffKind.Same;
            }

            DiffKind kind = DiffKind.Same;
            for (int i = 0; i < 32; i++)
            {
                if (a.Colors[i] != b.Colors[i])
                {
                    kind |= DiffKind.Colors;
                    break;
                }
            }

            if ((a.Name ?? string.Empty).Trim() != (b.Name ?? string.Empty).Trim())
            {
                kind |= DiffKind.Name;
            }

            if (a.TableStart != b.TableStart || a.TableEnd != b.TableEnd)
            {
                kind |= DiffKind.Table;
            }

            return kind;
        }

        private void Recompute()
        {
            if (!_loaded || Hues.List == null)
            {
                return;
            }

            _diff = new DiffKind[Hues.List.Length];
            for (int i = 0; i < _diff.Length; i++)
            {
                _diff[i] = Compare(i);
            }

            ApplyFilter();
        }

        private bool Matches(int index, int filter)
        {
            DiffKind d = _diff[index];
            return filter switch
            {
                1 => d != DiffKind.Same,
                2 => (d & DiffKind.Colors) != 0,
                3 => (d & DiffKind.Name) != 0,
                4 => (d & DiffKind.Table) != 0,
                5 => (d & DiffKind.OnlyIn1) != 0,
                6 => (d & DiffKind.OnlyIn2) != 0,
                7 => index == _selected || _multi.Contains(index),
                _ => true
            };
        }

        private void ApplyFilter()
        {
            int filter = _cmbFilter.SelectedIndex;
            _view = Enumerable.Range(0, _diff.Length).Where(i => Matches(i, filter)).ToList();
            UpdateScroll();

            int total = _diff.Count(d => d != DiffKind.Same);
            if (!_hue2Loaded)
            {
                _lblCount.Text = "load a second hues.mul";
                _lblCount.ForeColor = Color.DimGray;
            }
            else
            {
                _lblCount.Text = total == 0
                    ? "no differences"
                    : $"{total} of {_diff.Length} different  (colours {_diff.Count(d => (d & DiffKind.Colors) != 0)}, " +
                      $"names {_diff.Count(d => (d & DiffKind.Name) != 0)}, only in 1/2 {_diff.Count(d => (d & (DiffKind.OnlyIn1 | DiffKind.OnlyIn2)) != 0)})" +
                      (filter > 0 ? $"  – shown {_view.Count}" : string.Empty);
                _lblCount.ForeColor = total == 0 ? Color.DarkGreen : Color.DarkRed;
            }

            _list.Invalidate();
            UpdateDetail();
        }

        #endregion

        #region List

        private int VisibleRows => Math.Max(1, (_list.Height - _itemHeight) / _itemHeight);

        private void UpdateScroll()
        {
            _scroll.Minimum = 0;
            _scroll.Maximum = Math.Max(0, _view.Count - 1);
            _scroll.LargeChange = VisibleRows;
            _scroll.SmallChange = 1;
            int max = Math.Max(0, _view.Count - VisibleRows);
            if (_scroll.Value > max)
            {
                _scroll.Value = max;
            }
        }

        private void ScrollTo(int value)
        {
            int max = Math.Max(0, _view.Count - VisibleRows);
            _scroll.Value = Math.Max(0, Math.Min(max, value));
            _list.Invalidate();
        }

        private void EnsureVisible(int index)
        {
            int pos = _view.IndexOf(index);
            if (pos < 0)
            {
                return;
            }

            if (pos < _scroll.Value)
            {
                ScrollTo(pos);
            }
            else if (pos >= _scroll.Value + VisibleRows)
            {
                ScrollTo(pos - VisibleRows + 1);
            }
        }

        private int IndexAt(int y)
        {
            if (y < _itemHeight)
            {
                return -1; // header
            }

            int pos = _scroll.Value + ((y - _itemHeight) / _itemHeight);
            return pos >= 0 && pos < _view.Count ? _view[pos] : -1;
        }

        private static Color KindColor(DiffKind d)
        {
            if ((d & (DiffKind.OnlyIn1 | DiffKind.OnlyIn2)) != 0)
            {
                return Color.FromArgb(205, 205, 205);
            }

            if ((d & DiffKind.Colors) != 0)
            {
                return Color.FromArgb(255, 150, 150);
            }

            if ((d & DiffKind.Name) != 0)
            {
                return Color.FromArgb(255, 205, 130);
            }

            if ((d & DiffKind.Table) != 0)
            {
                return Color.FromArgb(215, 190, 255);
            }

            return Color.White;
        }

        private void OnPaintList(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            int width = _list.Width;
            int strip = Math.Max(64, (width - _numberWidth - (2 * _nameWidth) - 8) / 2);
            int x1 = _numberWidth + _nameWidth;
            int x2 = x1 + strip + 8;
            int xName2 = x2 + strip;

            // header
            g.FillRectangle(SystemBrushes.Control, 0, 0, width, _itemHeight);
            g.DrawString("Hue", Font, Brushes.Black, 4, 3);
            g.DrawString("Hues 1 (loaded)", Font, Brushes.Black, _numberWidth + 2, 3);
            g.DrawString(_hue2Loaded ? $"Hues 2 ({Path.GetFileName(Path.GetDirectoryName(_secondFile) ?? string.Empty)}/{Path.GetFileName(_secondFile)})" : "Hues 2 (not loaded)",
                Font, Brushes.Black, x2, 3);

            for (int row = 0; row <= VisibleRows; row++)
            {
                int pos = _scroll.Value + row;
                if (pos >= _view.Count)
                {
                    break;
                }

                int index = _view[pos];
                int y = (row + 1) * _itemHeight;
                Hue a = Hues.List[index];
                Hue b = _hue2Loaded && index < SecondHue.List.Length ? SecondHue.List[index] : null;
                DiffKind d = index < _diff.Length ? _diff[index] : DiffKind.Same;

                Color back = index == _selected ? SystemColors.Highlight : _multi.Contains(index) ? Color.LightSteelBlue : KindColor(d);
                Color fore = index == _selected ? SystemColors.HighlightText : Color.Black;
                using (var brush = new SolidBrush(back))
                {
                    g.FillRectangle(brush, 0, y, _numberWidth + _nameWidth, _itemHeight);
                    g.FillRectangle(brush, xName2, y, width - xName2, _itemHeight);
                }

                using (var brush = new SolidBrush(fore))
                {
                    int number = index + 1;
                    g.DrawString($"{number} (0x{number:X})", Font, brush, 3, y + 3);
                    g.DrawString(Trim(a?.Name), Font, brush, new RectangleF(_numberWidth, y + 3, _nameWidth - 2, _itemHeight - 3));
                    if (b != null)
                    {
                        g.DrawString(Trim(b.Name), Font, brush, new RectangleF(xName2 + 2, y + 3, width - xName2 - 2, _itemHeight - 3));
                    }
                }

                HueAnalysis.DrawStrip(g, new Rectangle(x1, y + 1, strip, _itemHeight - 2), a?.Colors);
                if (b != null)
                {
                    HueAnalysis.DrawStrip(g, new Rectangle(x2, y + 1, strip, _itemHeight - 2), b.Colors);
                }

                if (d != DiffKind.Same)
                {
                    using (var pen = new Pen(Color.Red, 2))
                    {
                        g.DrawLine(pen, x1 + strip + 4, y + 2, x1 + strip + 4, y + _itemHeight - 2);
                    }
                }
            }
        }

        private static string Trim(string name) => string.IsNullOrEmpty(name) ? string.Empty : name.Trim();

        private void OnListMouseDown(object sender, MouseEventArgs e)
        {
            int index = IndexAt(e.Y); // before: the position of the whole control was used (wrong row)
            _scroll.Focus();
            if (index < 0)
            {
                return;
            }

            bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;

            if (e.Button == MouseButtons.Right)
            {
                if (index != _selected && !_multi.Contains(index))
                {
                    _multi.Clear();
                    SelectHue(index);
                }

                return;
            }

            if (shift)
            {
                int a = _view.IndexOf(_selected), b = _view.IndexOf(index);
                if (!ctrl)
                {
                    _multi.Clear();
                }

                if (a >= 0 && b >= 0)
                {
                    for (int p = Math.Min(a, b); p <= Math.Max(a, b); p++)
                    {
                        _multi.Add(_view[p]);
                    }
                }
            }
            else if (ctrl)
            {
                if (_multi.Count == 0)
                {
                    _multi.Add(_selected);
                }

                if (!_multi.Remove(index))
                {
                    _multi.Add(index);
                }
            }
            else
            {
                _multi.Clear();
            }

            SelectHue(index);
        }

        private void SelectHue(int index)
        {
            _selected = index;
            EnsureVisible(index);
            _list.Invalidate();
            UpdateDetail();
        }

        private List<int> SelectedIndices
        {
            get
            {
                var set = new HashSet<int>(_multi) { _selected };
                return set.Where(i => i >= 0 && i < Hues.List.Length).OrderBy(i => i).ToList();
            }
        }

        private List<int> DiffIndicesInView() => _view.Where(i => _diff[i] != DiffKind.Same).ToList();

        private void JumpDiff(int direction)
        {
            var diffs = Enumerable.Range(0, _diff.Length).Where(i => _diff[i] != DiffKind.Same && _view.Contains(i)).ToList();
            if (diffs.Count == 0)
            {
                return;
            }

            int next = direction > 0
                ? diffs.FirstOrDefault(i => i > _selected, diffs[0])
                : diffs.LastOrDefault(i => i < _selected, diffs[diffs.Count - 1]);
            _multi.Clear();
            SelectHue(next);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ActiveControl is ToolStrip strip && strip.Items.OfType<ToolStripControlHost>().Any(h => h.Focused))
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            int pos = _view.IndexOf(_selected);
            switch (keyData)
            {
                case Keys.Up when pos > 0:
                    _multi.Clear();
                    SelectHue(_view[pos - 1]);
                    return true;
                case Keys.Down when pos >= 0 && pos < _view.Count - 1:
                    _multi.Clear();
                    SelectHue(_view[pos + 1]);
                    return true;
                case Keys.F3:
                    JumpDiff(1);
                    return true;
                case Keys.Shift | Keys.F3:
                    JumpDiff(-1);
                    return true;
                case Keys.Control | Keys.Z:
                    DoUndo();
                    return true;
                case Keys.Control | Keys.Y:
                    DoRedo();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Detail

        private void UpdateDetail()
        {
            if (_selected < 0 || _selected >= Hues.List.Length)
            {
                return;
            }

            Hue a = Hues.List[_selected];
            Hue b = _hue2Loaded && _selected < SecondHue.List.Length ? SecondHue.List[_selected] : null;
            int number = _selected + 1;

            var sb = new StringBuilder();
            sb.AppendLine($"Hue {number} (0x{number:X})   {DescribeKind(_diff.Length > _selected ? _diff[_selected] : DiffKind.Same)}");
            sb.AppendLine($"Name 1: {Trim(a.Name),-22} Table 0x{a.TableStart:X4}-0x{a.TableEnd:X4}");
            if (b != null)
            {
                sb.AppendLine($"Name 2: {Trim(b.Name),-22} Table 0x{b.TableStart:X4}-0x{b.TableEnd:X4}");
                int changed = 0;
                for (int i = 0; i < 32; i++)
                {
                    if (a.Colors[i] == b.Colors[i])
                    {
                        continue;
                    }

                    changed++;
                    if (changed <= 12)
                    {
                        sb.AppendLine($"  step {i,2}: {HueAnalysis.ToHex(HueAnalysis.ToColor(a.Colors[i]))} -> {HueAnalysis.ToHex(HueAnalysis.ToColor(b.Colors[i]))}");
                    }
                }

                if (changed > 12)
                {
                    sb.AppendLine($"  … {changed - 12} more steps");
                }

                if (changed > 0)
                {
                    sb.Insert(0, $"{changed} of 32 colour steps differ\n");
                }
            }

            if (SelectedIndices.Count > 1)
            {
                sb.AppendLine($"({SelectedIndices.Count} hues selected)");
            }

            // what the hue is used for in the loaded client files (statics of the maps, body/corpse/gump/equipconv.def)
            HueUsageEntry first = null;
            if (HueUsageIndex.IsBuilt)
            {
                IReadOnlyList<HueUsageEntry> usage = HueUsageIndex.Get(_selected);
                first = usage.FirstOrDefault();
                sb.AppendLine(usage.Count == 0 ? "Used: not in the client files" : $"Used ({usage.Count}): {usage[0].Text}" + (usage.Count > 1 ? " …" : string.Empty));
            }
            else
            {
                sb.AppendLine(HueUsageIndex.IsBuilding ? "Used: scanning …" : string.Empty);
            }

            _detailText.Text = sb.ToString();
            _detail.Invalidate();
            _preview.SetHues(a.Colors, b?.Colors, $"Hues 1 – {number}", b == null ? null : $"Hues 2 – {number}");
            if (first != null)
            {
                _preview.ShowGraphic(first.PreviewKind, first.PreviewId);
            }
            else
            {
                _preview.ShowOwnGraphic();
            }
        }

        private static string DescribeKind(DiffKind d)
        {
            if (d == DiffKind.Same)
            {
                return "equal";
            }

            var parts = new List<string>();
            if ((d & DiffKind.Colors) != 0) { parts.Add("colours differ"); }
            if ((d & DiffKind.Name) != 0) { parts.Add("name differs"); }
            if ((d & DiffKind.Table) != 0) { parts.Add("table values differ"); }
            if ((d & DiffKind.OnlyIn1) != 0) { parts.Add("only in Hues 1"); }
            if ((d & DiffKind.OnlyIn2) != 0) { parts.Add("only in Hues 2"); }
            return string.Join(", ", parts);
        }

        private void OnPaintDetail(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(SystemColors.Control);
            if (_selected < 0 || _selected >= Hues.List.Length)
            {
                return;
            }

            Hue a = Hues.List[_selected];
            Hue b = _hue2Loaded && _selected < SecondHue.List.Length ? SecondHue.List[_selected] : null;
            var marked = new List<int>();
            if (b != null)
            {
                for (int i = 0; i < 32; i++)
                {
                    if (a.Colors[i] != b.Colors[i])
                    {
                        marked.Add(i);
                    }
                }
            }

            int w = _detail.Width - 70;
            g.DrawString("Hues 1", Font, Brushes.Black, 4, 10);
            HueAnalysis.DrawStrip(g, new Rectangle(62, 4, w, 30), a.Colors, marked);
            g.DrawString("Hues 2", Font, Brushes.Black, 4, 46);
            if (b != null)
            {
                HueAnalysis.DrawStrip(g, new Rectangle(62, 40, w, 30), b.Colors, marked);
            }
        }

        #endregion

        #region Apply / undo / save / export

        /// <param name="secondToFirst">true = Hues 2 -> Hues 1, false = Hues 1 -> Hues 2</param>
        private void Apply(bool secondToFirst, Parts parts, List<int> indices)
        {
            if (!_hue2Loaded || indices.Count == 0)
            {
                return;
            }

            if (indices.Count > 1 && MessageBox.Show($"Apply {indices.Count} hue(s) {(secondToFirst ? "2 → 1" : "1 → 2")}?", "Compare hues",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Hue[] from = secondToFirst ? SecondHue.List : Hues.List;
            Hue[] to = secondToFirst ? Hues.List : SecondHue.List;
            HueHistory history = secondToFirst ? HueHistory.Main : _history2;
            List<int> valid = indices.Where(i => i < from.Length && i < to.Length && from[i] != null && to[i] != null).ToList();

            history.Record($"Apply {(secondToFirst ? "2 → 1" : "1 → 2")} ({valid.Count})", valid, () =>
            {
                foreach (int i in valid)
                {
                    // before: TableStart/TableEnd were recalculated instead of copied
                    HueState.Capture(from[i]).ApplyTo(to[i], (parts & Parts.Colors) != 0, (parts & Parts.Name) != 0, (parts & Parts.Table) != 0);
                }
            });

            _undoOrder.Add(history);
            _redoOrder.Clear();
            if (!secondToFirst)
            {
                _secondChanged = true;
            }

            Recompute(); // the main list fires HueChangeEvent itself, the second list not
            UpdateUndoButtons();
        }

        private void DoUndo()
        {
            if (_undoOrder.Count == 0)
            {
                return;
            }

            HueHistory h = _undoOrder[_undoOrder.Count - 1];
            _undoOrder.RemoveAt(_undoOrder.Count - 1);
            if (h.Undo() != null)
            {
                _redoOrder.Add(h);
                if (h == _history2)
                {
                    _secondChanged = true;
                }
            }

            Recompute();
            UpdateUndoButtons();
        }

        private void DoRedo()
        {
            if (_redoOrder.Count == 0)
            {
                return;
            }

            HueHistory h = _redoOrder[_redoOrder.Count - 1];
            _redoOrder.RemoveAt(_redoOrder.Count - 1);
            if (h.Redo() != null)
            {
                _undoOrder.Add(h);
                if (h == _history2)
                {
                    _secondChanged = true;
                }
            }

            Recompute();
            UpdateUndoButtons();
        }

        private void UpdateUndoButtons()
        {
            if (_btnUndo == null)
            {
                return;
            }

            _btnUndo.Enabled = _undoOrder.Count > 0;
            _btnRedo.Enabled = _redoOrder.Count > 0;
        }

        private void SaveFirst()
        {
            string path = Options.OutputPath;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("The output folder is not set or does not exist (Options).", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Hues.Save(path);
            Options.ChangedUltimaClass["Hues"] = false;
            MessageBox.Show($"Hues 1 saved to {Path.Combine(path, "hues.mul")}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveSecond()
        {
            if (!_hue2Loaded)
            {
                return;
            }

            using (var dlg = new SaveFileDialog
            {
                Filter = "hues.mul|*.mul",
                FileName = "hues.mul",
                InitialDirectory = Path.GetDirectoryName(_secondFile)
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    HueFiles.SaveMul(SecondHue.List, dlg.FileName);
                    _secondChanged = false;
                    MessageBox.Show($"Hues 2 saved to {dlg.FileName}" + (File.Exists(dlg.FileName + ".bak") ? $"\nBackup: {dlg.FileName}.bak" : string.Empty),
                        "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Saving failed:\n{ex.Message}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportDiff()
        {
            if (!_hue2Loaded)
            {
                return;
            }

            using (var dlg = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv|Text (*.txt)|*.txt", FileName = "hue_differences.csv" })
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine("Hue;Hex;Difference;Name1;Name2;ChangedSteps;Table1;Table2");
                for (int i = 0; i < _diff.Length; i++)
                {
                    if (_diff[i] == DiffKind.Same)
                    {
                        continue;
                    }

                    Hue a = Hues.List[i];
                    Hue b = SecondHue.List[i];
                    int steps = Enumerable.Range(0, 32).Count(s => a.Colors[s] != b.Colors[s]);
                    sb.AppendLine($"{i + 1};0x{i + 1:X};{DescribeKind(_diff[i])};{Csv(a.Name)};{Csv(b.Name)};{steps};" +
                                  $"0x{a.TableStart:X4}-0x{a.TableEnd:X4};0x{b.TableStart:X4}-0x{b.TableEnd:X4}");
                }

                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Saved: {dlg.FileName}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string Csv(string text) => (text ?? string.Empty).Trim().Replace(";", ",");

        private void CopyText(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion
    }
}
