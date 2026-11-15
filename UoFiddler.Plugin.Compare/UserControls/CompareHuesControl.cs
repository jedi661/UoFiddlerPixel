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

        public CompareHuesControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _cmbFilter.SelectedIndex = 0;
            _list.MouseWheel += OnListMouseWheel;
            HandleCreated += OnHandleCreatedSetSplitter;
            UpdateUndoButtons();
        }

        #region Designer events

        private void OnHandleCreatedSetSplitter(object sender, EventArgs e)
        {
            try
            {
                splitContainer.SplitterDistance = Math.Max(150, Height - 300);
            }
            catch (InvalidOperationException)
            {
                // ignored (control too small)
            }
        }

        private void OnPathKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoadSecond();
            }
        }

        private void OnClickBrowseFolder(object sender, EventArgs e) => BrowseFolder();

        private void OnClickBrowseFile(object sender, EventArgs e) => BrowseFile();

        private void OnClickLoad(object sender, EventArgs e) => LoadSecond();

        private void OnFilterChanged(object sender, EventArgs e) => ApplyFilter();

        private void OnClickPrev(object sender, EventArgs e) => JumpDiff(-1);

        private void OnClickNext(object sender, EventArgs e) => JumpDiff(1);

        private void OnClickApply21(object sender, EventArgs e) => Apply(true, Parts.All, SelectedIndices);

        private void OnClickApply21Colors(object sender, EventArgs e) => Apply(true, Parts.Colors | Parts.Table, SelectedIndices);

        private void OnClickApply21Name(object sender, EventArgs e) => Apply(true, Parts.Name, SelectedIndices);

        private void OnClickApply21All(object sender, EventArgs e) => Apply(true, Parts.All, DiffIndicesInView());

        private void OnClickApply12(object sender, EventArgs e) => Apply(false, Parts.All, SelectedIndices);

        private void OnClickApply12Colors(object sender, EventArgs e) => Apply(false, Parts.Colors | Parts.Table, SelectedIndices);

        private void OnClickApply12Name(object sender, EventArgs e) => Apply(false, Parts.Name, SelectedIndices);

        private void OnClickApply12All(object sender, EventArgs e) => Apply(false, Parts.All, DiffIndicesInView());

        private void OnClickUndo(object sender, EventArgs e) => DoUndo();

        private void OnClickRedo(object sender, EventArgs e) => DoRedo();

        private void OnClickSaveFirst(object sender, EventArgs e) => SaveFirst();

        private void OnClickSaveSecond(object sender, EventArgs e) => SaveSecond();

        private void OnClickExportDiff(object sender, EventArgs e) => ExportDiff();

        private void OnClickCopyNumber(object sender, EventArgs e) => CopyText($"{_selected + 1} (0x{_selected + 1:X})");

        private void OnClickCopyServUO(object sender, EventArgs e) => CopyText(HueFiles.ServUOText(Hues.List[_selected]));

        private void OnListMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (ToolStripItem item in new ToolStripItem[] { menuApply21All, menuApply21Colors, menuApply21Name, menuApply12All, menuApply12Colors, menuApply12Name })
            {
                item.Enabled = _hue2Loaded;
            }
        }

        private void OnListResize(object sender, EventArgs e)
        {
            UpdateScroll();
            _list.Invalidate();
        }

        private void OnListMouseWheel(object sender, MouseEventArgs e) => ScrollTo(_scroll.Value + (e.Delta < 0 ? 3 : -3));

        private void OnScroll(object sender, ScrollEventArgs e) => _list.Invalidate();

        private void OnDetailResize(object sender, EventArgs e) => _detail.Invalidate();

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
