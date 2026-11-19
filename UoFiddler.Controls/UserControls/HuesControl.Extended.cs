/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * HuesControl – extensions:
 *  toolbar:  filter (named, empty, changed, colour family …), list/grid, undo/redo, counter
 *  panel:    info + preview (item / animation / gump), similar hues, colour -> hue
 *  menu:     edit, rename, copy/paste, clear, copy to / swap with, revert, gradient,
 *            from image, ServUO text, range export/import, palette export (PNG/GPL/ACT)
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.MapViewer;

namespace UoFiddler.Controls.UserControls
{
    public partial class HuesControl
    {
        private static readonly string[] _filterNames =
        {
            "All", "Named", "Unnamed", "Empty (black)", "Not empty", "Changed", "Selected",
            "Red", "Orange", "Yellow", "Green", "Cyan", "Blue", "Purple", "Pink", "Gray / white / black",
            "Used in the client files", "Not used (free)"
        };

        private const int FilterUsed = 16;
        private const int FilterUnused = 17;

        // clipboard for copy/paste of hues (inside UoFiddler)
        private static List<HueState> _clipboard = new List<HueState>();


        private HueState[] _baseline = Array.Empty<HueState>();

        // side panel
        private int _stripStep = -1;

        // usage

        #region Toolbar

        /// <summary>The toolbar items are in the designer (also used by the hue pop ups: filter and grid)</summary>
        private void InitToolbarExtensions()
        {
            _filterBox.SelectedIndex = 0;
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (_loaded)
            {
                ApplyFilter(true);
            }
        }

        private void OnGridChanged(object sender, EventArgs e)
        {
            _grid = _gridButton.Checked;
            if (!_loaded)
            {
                return;
            }

            UpdateScrollBar();
            EnsureVisible(_selected);
            pictureBox.Invalidate();
        }

        private void UpdateUndoButtons()
        {
            if (_undoButton == null)
            {
                return;
            }

            _undoButton.Enabled = HueHistory.Main.CanUndo;
            _redoButton.Enabled = HueHistory.Main.CanRedo;
            _undoButton.ToolTipText = HueHistory.Main.CanUndo ? $"Undo: {HueHistory.Main.UndoName} (Ctrl+Z)" : "Undo (Ctrl+Z)";
            _redoButton.ToolTipText = HueHistory.Main.CanRedo ? $"Redo: {HueHistory.Main.RedoName} (Ctrl+Y)" : "Redo (Ctrl+Y)";
        }

        #endregion

        #region Filter / changed

        private void CaptureBaseline()
        {
            _baseline = Hues.List.Select(h => h == null ? null : HueState.Capture(h)).ToArray();
        }

        private bool IsModified(int index)
        {
            return index < _baseline.Length && _baseline[index] != null && !_baseline[index].SameAs(Hues.List[index]);
        }

        /// <summary>true if the filter result can change when hues are edited</summary>
        private bool FilterDependsOnContent => _filterBox != null && _filterBox.SelectedIndex > 0 && _filterBox.SelectedIndex != 6 &&
                                               _filterBox.SelectedIndex != FilterUsed && _filterBox.SelectedIndex != FilterUnused;

        private void ResetFilter()
        {
            if (_filterBox != null && _filterBox.SelectedIndex != 0)
            {
                _filterBox.SelectedIndex = 0; // calls ApplyFilter
            }
        }

        private bool Matches(Hue hue, int filter)
        {
            switch (filter)
            {
                case 1: return !HueAnalysis.IsUnnamed(hue);
                case 2: return HueAnalysis.IsUnnamed(hue);
                case 3: return HueAnalysis.IsEmpty(hue);
                case 4: return !HueAnalysis.IsEmpty(hue);
                case 5: return IsModified(hue.Index);
                case 6: return hue.Index == _selected || _multi.Contains(hue.Index);
                case FilterUsed: return HueUsageIndex.IsUsed(hue.Index);
                case FilterUnused: return HueUsageIndex.IsBuilt && !HueUsageIndex.IsUsed(hue.Index);
                case 0: return true;
                default:
                    var family = (HueFamily)(filter - 6); // 7 = Red … 15 = Gray
                    return HueAnalysis.Family(hue) == family;
            }
        }

        private void ApplyFilter(bool keepScroll)
        {
            int filter = _filterBox?.SelectedIndex ?? 0;
            _view = Hues.List.Where(h => h != null && Matches(h, filter)).Select(h => h.Index).ToList();
            if (!keepScroll)
            {
                vScrollBar.Value = 0;
            }

            UpdateScrollBar();
            if (_countLabel != null)
            {
                _countLabel.Text = filter == 0 ? $"{_view.Count} hues" : $"{_view.Count} of {Hues.List.Length}";
            }

            EnsureVisible(_selected);
            pictureBox.Invalidate();
        }

        #endregion

        #region Edit extensions (only in the hues tab, not in the hue pop ups)

        /// <summary>The controls are in the designer; here the events of the program are connected</summary>
        private void InitEditExtensions()
        {
            // extended hues (hue numbers 3001 .. 0x3FFF, huesext.mul)
            var extItem = new ToolStripMenuItem("Extended hues (3001+)...")
            {
                ToolTipText = "Create / resize / save / load huesext.mul - more hues than the 3000 of hues.mul."
            };
            extItem.Click += (s, e) => ExtendedArtFormatForm.ShowFor(UoExtensionKind.Hues);
            var gotoExt = new ToolStripMenuItem("Go to extended hues (3001)");
            gotoExt.Click += (s, e) =>
            {
                if (!Hues.HasExtension)
                {
                    MessageBox.Show(this, "There are no extended hues yet. Create them with 'Extended hues (3001+)...'.", "Hues",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ResetFilter();
                _selected = Hues.LegacyCount;
                EnsureVisible(_selected);
                pictureBox.Invalidate();
                SelectedChanged?.Invoke(this, EventArgs.Empty);
            };
            contextMenuStrip1.Items.Add(new ToolStripSeparator());
            contextMenuStrip1.Items.Add(extItem);
            contextMenuStrip1.Items.Add(gotoExt);

            HueHistory.Main.Changed += OnHistoryChanged;
            UpdateUndoButtons();
            Disposed += (s, e) => HueHistory.Main.Changed -= OnHistoryChanged;

            SelectedChanged += (s, e) => UpdateSidePanel();

            HueUsageIndex.Built += OnUsageBuilt;
            Disposed += (s, e) => HueUsageIndex.Built -= OnUsageBuilt;
            ControlEvents.FilePathChangeEvent += OnFilesChangedForUsage;
            Disposed += (s, e) => ControlEvents.FilePathChangeEvent -= OnFilesChangedForUsage;
            StartUsageScan();
        }

        /// <summary>Hue pop ups (choosing a hue): no side panel and no editing</summary>
        private void HideEditExtensions()
        {
            sidePanel.Visible = false;
            splitterSide.Visible = false;
            toolStripSeparatorUndo.Visible = false;
            _undoButton.Visible = false;
            _redoButton.Visible = false;
        }

        #region Usage (which hue is used for what in the client files)

        private void StartUsageScan()
        {
            if (HueUsageIndex.IsBuilt || HueUsageIndex.IsBuilding)
            {
                return;
            }

            try
            {
                HueUsageIndex.BuildAsync();
            }
            catch (Exception ex)
            {
                if (_usageTitle != null)
                {
                    _usageTitle.Text = $"Scan failed: {ex.Message}";
                }
            }

            UpdateSidePanel();
        }

        private void OnFilesChangedForUsage()
        {
            HueUsageIndex.Invalidate();
            StartUsageScan();
        }

        private void OnUsageBuilt(object sender, EventArgs e)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            BeginInvoke(new Action(() =>
            {
                if (_filterBox != null && (_filterBox.SelectedIndex == FilterUsed || _filterBox.SelectedIndex == FilterUnused))
                {
                    ApplyFilter(true);
                }

                UpdateSidePanel();
            }));
        }

        private void FillUsage(Hue hue)
        {
            if (_usage == null)
            {
                return;
            }

            _usage.BeginUpdate();
            _usage.Items.Clear();
            if (!HueUsageIndex.IsBuilt)
            {
                _usageTitle.Text = HueUsageIndex.IsBuilding ? "Scanning the maps and .def files …" : HueUsageIndex.Summary;
            }
            else
            {
                IReadOnlyList<HueUsageEntry> list = HueUsageIndex.Get(hue.Index);
                _usageTitle.Text = list.Count == 0
                    ? $"Hue {hue.Index + 1} is not used in the client files."
                    : $"Hue {hue.Index + 1} is used {list.Count}× – click an entry to see it:";
                foreach (HueUsageEntry entry in list)
                {
                    _usage.Items.Add(entry);
                }
            }

            _usage.EndUpdate();
            _toolTip.SetToolTip(_usage, HueUsageIndex.Summary + (HueUsageIndex.LastError != null ? "\n" + HueUsageIndex.LastError : string.Empty));
        }

        private static string UsageLine(Hue hue)
        {
            if (!HueUsageIndex.IsBuilt)
            {
                return HueUsageIndex.IsBuilding ? "scanning …" : "not scanned";
            }

            IReadOnlyList<HueUsageEntry> list = HueUsageIndex.Get(hue.Index);
            if (list.Count == 0)
            {
                return "not used in the client files (free)";
            }

            int statics = list.Where(u => u.Kind == HueUsageKind.Static).Sum(u => u.Count);
            int ids = list.Count(u => u.Kind == HueUsageKind.Static);
            int defs = list.Count(u => u.Kind != HueUsageKind.Static);
            var parts = new List<string>();
            if (statics > 0)
            {
                parts.Add($"{statics}× on the maps ({ids} item id{(ids == 1 ? string.Empty : "s")})");
            }

            if (defs > 0)
            {
                parts.Add($"{defs} .def entr{(defs == 1 ? "y" : "ies")}");
            }

            return string.Join(", ", parts);
        }

        #endregion

        private void OnHistoryChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnHistoryChanged(sender, e)));
                return;
            }

            UpdateUndoButtons();
        }

        // ── menu / button events (the items are in the designer) ──

        private void OnClickEdit(object sender, EventArgs e) => OpenEditor(_selected);

        private void OnClickRename(object sender, EventArgs e) => RenameSelected();

        private void OnClickCopy(object sender, EventArgs e) => CopySelected();

        private void OnClickPaste(object sender, EventArgs e) => PasteToSelected();

        private void OnClickClear(object sender, EventArgs e) => ClearSelected();

        private void OnClickCopyTo(object sender, EventArgs e) => CopyToHue();

        private void OnClickSwap(object sender, EventArgs e) => SwapWithHue();

        private void OnClickRevert(object sender, EventArgs e) => RevertSelected();

        private void OnClickGradient(object sender, EventArgs e) => CreateFromGradient();

        private void OnClickFromImage(object sender, EventArgs e) => CreateFromImage();

        private void OnClickSimilar(object sender, EventArgs e) => FindSimilar();

        private void OnClickServUO(object sender, EventArgs e) => CopyServUO();

        private void OnClickExportRange(object sender, EventArgs e) => ExportRange();

        private void OnClickImportRange(object sender, EventArgs e) => ImportRange();

        private void OnClickPalettePng(object sender, EventArgs e) => ExportPalette("png");

        private void OnClickPaletteGpl(object sender, EventArgs e) => ExportPalette("gpl");

        private void OnClickPaletteAct(object sender, EventArgs e) => ExportPalette("act");

        private void OnClickUndo(object sender, EventArgs e) => DoUndo();

        private void OnClickRedo(object sender, EventArgs e) => DoRedo();

        private bool HandleEditKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.C: CopySelected(); return true;
                case Keys.Control | Keys.V: PasteToSelected(); return true;
                case Keys.Control | Keys.Z: DoUndo(); return true;
                case Keys.Control | Keys.Y:
                case Keys.Control | Keys.Shift | Keys.Z: DoRedo(); return true;
                case Keys.Control | Keys.A:
                    _multi.Clear();
                    foreach (int i in _view)
                    {
                        _multi.Add(i);
                    }

                    pictureBox.Invalidate();
                    UpdateSidePanel();
                    return true;
                case Keys.Delete: ClearSelected(); return true;
                case Keys.F2: RenameSelected(); return true;
                case Keys.Enter: OpenEditor(_selected); return true;
            }

            return false;
        }

        #endregion

        #region Side panel

        private void OnPaintStrip(object sender, PaintEventArgs e)
        {
            Hue hue = SelectedHue;
            HueAnalysis.DrawStrip(e.Graphics, new Rectangle(4, 4, _strip.Width - 8, _strip.Height - 8), hue?.Colors,
                _stripStep >= 0 ? new[] { _stripStep } : null);
        }

        private void OnStripClick(object sender, MouseEventArgs e)
        {
            _stripStep = Math.Max(0, Math.Min(31, (int)((e.X - 4) / ((_strip.Width - 8) / 32f))));
            UpdateSidePanel();
        }

        private void OnUsageSelected(object sender, EventArgs e)
        {
            if (_usage.SelectedItem is HueUsageEntry entry)
            {
                _preview.ShowGraphic(entry.PreviewKind, entry.PreviewId);
            }
        }

        private void OnFollowUsageChanged(object sender, EventArgs e) => UpdateSidePanel();

        private void OnClickScanAgain(object sender, EventArgs e)
        {
            HueUsageIndex.Invalidate();
            StartUsageScan();
        }

        private void OnResultDoubleClick(object sender, EventArgs e)
        {
            if (_results.SelectedItem is ResultItem r)
            {
                ShowHue(r.Index);
            }
        }

        private void OnResultKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && _results.SelectedItem is ResultItem r)
            {
                ShowHue(r.Index);
            }
        }

        private void OnColorTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                FindByColor();
            }
        }

        private void OnClickPickColor(object sender, EventArgs e) => PickColor();

        private void OnClickFindColor(object sender, EventArgs e) => FindByColor();

        private Hue SelectedHue => _selected >= 0 && _selected < Hues.List.Length ? Hues.List[_selected] : null;

        private void UpdateSidePanel()
        {
            if (_info == null)
            {
                return;
            }

            Hue hue = SelectedHue;
            if (hue == null)
            {
                _info.Text = string.Empty;
                return;
            }

            int number = hue.Index + 1;
            int count = SelectedIndices.Count;
            Color main = HueAnalysis.MainColor(hue);
            string step = string.Empty;
            if (_stripStep >= 0)
            {
                Color c = HueAnalysis.ToColor(hue.Colors[_stripStep]);
                step = $"\nStep {_stripStep,2}:   {HueAnalysis.ToHex(c)}  RGB {c.R},{c.G},{c.B}  (0x{hue.Colors[_stripStep]:X4})";
            }

            _info.Text =
                $"Hue (game):  {number}  /  0x{number:X}\n" +
                $"File index:  {hue.Index}\n" +
                $"Name:        {(HueAnalysis.IsUnnamed(hue) ? "(no name)" : hue.Name)}\n" +
                $"Table:       start 0x{hue.TableStart:X4}  end 0x{hue.TableEnd:X4}\n" +
                $"Brightness:  {HueAnalysis.Brightness(hue)} %   main colour {HueAnalysis.ToHex(main)}\n" +
                $"Family:      {HueAnalysis.Family(hue)}" +
                (HueAnalysis.IsEmpty(hue) ? "   – EMPTY (free)" : string.Empty) +
                (IsModified(hue.Index) ? "   – CHANGED" : string.Empty) +
                (count > 1 ? $"\nSelected:    {count} hues" : string.Empty) +
                $"\nUsed:        {UsageLine(hue)}" +
                step;

            _strip.Invalidate();
            _preview.SetHues(hue.Colors, null);
            FillUsage(hue);

            // the preview shows what the hue is used for (or the own test graphic if it is not used)
            if (_chkFollowUsage == null || _chkFollowUsage.Checked)
            {
                HueUsageEntry first = HueUsageIndex.IsBuilt ? HueUsageIndex.Get(hue.Index).FirstOrDefault() : null;
                if (first != null)
                {
                    _preview.ShowGraphic(first.PreviewKind, first.PreviewId);
                }
                else
                {
                    _preview.ShowOwnGraphic();
                }
            }
        }

        #endregion

        #region Find (similar / colour)

        private sealed class ResultItem
        {
            public int Index;
            public string Text;
            public int Step = -1;
        }

        private void ShowResults(string title, IEnumerable<ResultItem> items)
        {
            if (_results == null)
            {
                return;
            }

            _resultsTitle.Text = title;
            _results.BeginUpdate();
            _results.Items.Clear();
            foreach (ResultItem item in items)
            {
                _results.Items.Add(item);
            }

            _results.EndUpdate();
            if (_results.Parent is TabPage page && page.Parent is TabControl tabs)
            {
                tabs.SelectedTab = page;
            }
        }

        private void OnDrawResult(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || !(_results.Items[e.Index] is ResultItem r))
            {
                return;
            }

            Hue hue = Hues.List[r.Index];
            var strip = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, Math.Max(32, e.Bounds.Width - 4), 14);
            HueAnalysis.DrawStrip(e.Graphics, strip, hue.Colors, r.Step >= 0 ? new[] { r.Step } : null);
            TextRenderer.DrawText(e.Graphics, r.Text, _results.Font, new Point(e.Bounds.X + 2, e.Bounds.Y + 17),
                (e.State & DrawItemState.Selected) != 0 ? SystemColors.HighlightText : SystemColors.ControlText);
        }

        private void FindSimilar()
        {
            Hue hue = SelectedHue;
            if (hue == null)
            {
                return;
            }

            List<(int Index, int Score)> similar = HueAnalysis.Similar(Hues.List, hue.Index, 30);
            ShowResults($"Similar to hue {hue.Index + 1}:", similar.Select(s => new ResultItem
            {
                Index = s.Index,
                Text = $"{s.Index + 1} (0x{s.Index + 1:X})  {Hues.List[s.Index].Name}   – difference {s.Score}"
            }));
        }

        private void PickColor()
        {
            HueAnalysis.TryParseColor(_colorText.Text, out Color current);
            using (var dlg = new ColorDialog { FullOpen = true, Color = current.IsEmpty ? Color.Red : current })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _colorText.Text = HueAnalysis.ToHex(dlg.Color);
                    FindByColor();
                }
            }
        }

        private void FindByColor()
        {
            if (!HueAnalysis.TryParseColor(_colorText.Text, out Color color))
            {
                _colorText.ForeColor = Color.Red;
                return;
            }

            _colorText.ForeColor = SystemColors.WindowText;
            List<(int Index, int Step, int Score)> found = HueAnalysis.FindByColor(Hues.List, color, 40);
            ShowResults($"Hues with a colour near {HueAnalysis.ToHex(color)}:", found.Select(f => new ResultItem
            {
                Index = f.Index,
                Step = f.Step,
                Text = $"{f.Index + 1} (0x{f.Index + 1:X})  {Hues.List[f.Index].Name}   – step {f.Step}, difference {f.Score}"
            }));
        }

        #endregion

        #region Edit actions

        private void OpenEditor(int index)
        {
            if (!HuesEditable || index < 0 || index >= Hues.List.Length)
            {
                return;
            }

            new HueEditForm(index).Show();
        }

        private void DoUndo()
        {
            string name = HueHistory.Main.Undo();
            if (name != null)
            {
                _toolTip.Show($"Undone: {name}", pictureBox, 10, 10, 1500);
            }
        }

        private void DoRedo()
        {
            string name = HueHistory.Main.Redo();
            if (name != null)
            {
                _toolTip.Show($"Redone: {name}", pictureBox, 10, 10, 1500);
            }
        }

        private void RenameSelected()
        {
            Hue hue = SelectedHue;
            if (hue == null)
            {
                return;
            }

            string name = InputDialog.Show(this, "Rename hue", $"Name of hue {hue.Index + 1} (max. 20 characters, ASCII):", hue.Name, false);
            if (name == null)
            {
                return;
            }

            name = new string(name.Trim().Where(c => c >= 32 && c < 127).Take(20).ToArray());
            List<int> targets = SelectedIndices;
            HueHistory.Main.Record("Rename", targets, () =>
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    // several hues: "name 1", "name 2", … (cut to 20 characters)
                    string n = targets.Count == 1 ? name : $"{name} {i + 1}";
                    Hues.List[targets[i]].Name = n.Length > 20 ? n.Substring(0, 20) : n;
                }
            });
        }

        private void CopySelected()
        {
            _clipboard = SelectedIndices.Select(i => HueState.Capture(Hues.List[i])).ToList();
            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, SelectedIndices.Select(i => HueFiles.ServUOText(Hues.List[i]))));
            }
            catch (Exception)
            {
                // clipboard busy: the internal copy still works
            }

            _toolTip.Show($"{_clipboard.Count} hue(s) copied", pictureBox, 10, 10, 1200);
        }

        private void PasteToSelected()
        {
            if (_clipboard.Count == 0)
            {
                return;
            }

            List<int> selected = SelectedIndices;
            List<int> targets;
            if (_clipboard.Count == 1)
            {
                targets = selected; // one hue into all selected hues
            }
            else
            {
                int start = selected[0];
                targets = Enumerable.Range(start, _clipboard.Count).Where(i => i < Hues.List.Length).ToList();
            }

            HueHistory.Main.Record($"Paste {_clipboard.Count} hue(s)", targets, () =>
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    _clipboard[_clipboard.Count == 1 ? 0 : i].ApplyTo(Hues.List[targets[i]]);
                }
            });
        }

        private void ClearSelected()
        {
            List<int> targets = SelectedIndices;
            if (MessageBox.Show(this, $"Set {targets.Count} hue(s) to empty (black, no name)?", "Clear",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            var empty = new HueState();
            HueHistory.Main.Record("Clear", targets, () =>
            {
                foreach (int t in targets)
                {
                    empty.ApplyTo(Hues.List[t]);
                }
            });
        }

        private int? AskHueNumber(string title, string prompt)
        {
            string text = InputDialog.Show(this, title, prompt, string.Empty, false);
            if (text == null)
            {
                return null;
            }

            if (!Utils.ConvertStringToInt(text.Trim(), out int number, 1, Hues.List.Length))
            {
                MessageBox.Show(this, $"Please enter a hue number between 1 and {Hues.List.Length}.", title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            return number - 1;
        }

        private void CopyToHue()
        {
            List<int> sources = SelectedIndices;
            int? target = AskHueNumber("Copy to hue", $"Copy {sources.Count} hue(s) to hue number (first target):");
            if (!target.HasValue)
            {
                return;
            }

            List<HueState> states = sources.Select(i => HueState.Capture(Hues.List[i])).ToList();
            List<int> targets = Enumerable.Range(target.Value, states.Count).Where(i => i < Hues.List.Length).ToList();
            HueHistory.Main.Record($"Copy to hue {target.Value + 1}", targets, () =>
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    states[i].ApplyTo(Hues.List[targets[i]]);
                }
            });
            ShowHue(target.Value);
        }

        private void SwapWithHue()
        {
            int source = _selected;
            int? target = AskHueNumber("Swap hues", $"Swap hue {source + 1} with hue number:");
            if (!target.HasValue || target.Value == source)
            {
                return;
            }

            HueState a = HueState.Capture(Hues.List[source]);
            HueState b = HueState.Capture(Hues.List[target.Value]);
            HueHistory.Main.Record($"Swap {source + 1} <-> {target.Value + 1}", new[] { source, target.Value }, () =>
            {
                b.ApplyTo(Hues.List[source]);
                a.ApplyTo(Hues.List[target.Value]);
            });
        }

        private void RevertSelected()
        {
            List<int> targets = SelectedIndices.Where(IsModified).ToList();
            if (targets.Count == 0)
            {
                return;
            }

            HueHistory.Main.Record("Revert", targets, () =>
            {
                foreach (int t in targets)
                {
                    _baseline[t].ApplyTo(Hues.List[t]);
                }
            });
        }

        private void CreateFromGradient()
        {
            Hue hue = SelectedHue;
            if (hue == null)
            {
                return;
            }

            using (var form = new HueGradientForm(hue.Colors, hue.Name))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ApplyColors(form.Result, form.HueName, "Gradient");
                }
            }
        }

        private void CreateFromImage()
        {
            Hue hue = SelectedHue;
            if (hue == null)
            {
                return;
            }

            using (var form = new HueFromImageForm(hue.Name))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ApplyColors(form.Result, form.HueName, "From image");
                }
            }
        }

        /// <summary>New colours (+ name) into all selected hues</summary>
        private void ApplyColors(ushort[] colors, string name, string operation)
        {
            HueState state = HueState.FromColors(colors, name);
            List<int> targets = SelectedIndices;
            HueHistory.Main.Record(operation, targets, () =>
            {
                foreach (int t in targets)
                {
                    state.ApplyTo(Hues.List[t], true, !string.IsNullOrEmpty(name), true);
                }
            });
        }

        private void CopyServUO()
        {
            string text = string.Join(Environment.NewLine, SelectedIndices.Select(i => HueFiles.ServUOText(Hues.List[i])));
            try
            {
                Clipboard.SetText(text);
                _toolTip.Show("Copied: " + text.Split('\n')[0] + (SelectedIndices.Count > 1 ? " …" : string.Empty), pictureBox, 10, 10, 2000);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Copy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ExportRange()
        {
            List<int> selected = SelectedIndices;
            using (var dlg = new SaveFileDialog
            {
                Filter = "Hue range (*.uohues.txt)|*.uohues.txt|Text (*.txt)|*.txt",
                FileName = selected.Count == 1 ? $"hue_{selected[0] + 1}.uohues.txt" : $"hues_{selected[0] + 1}-{selected[selected.Count - 1] + 1}.uohues.txt"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                HueFiles.ExportRange(selected.Select(i => Hues.List[i]), dlg.FileName);
                _toolTip.Show($"{selected.Count} hue(s) exported", pictureBox, 10, 10, 1500);
            }
        }

        private void ImportRange()
        {
            using (var dlg = new OpenFileDialog { Filter = "Hue range / hue export (*.txt)|*.txt|All files|*.*" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<(int Number, HueState State)> entries;
                try
                {
                    entries = HueFiles.ImportRange(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (entries.Count == 0)
                {
                    MessageBox.Show(this, "No hues found in the file.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                bool useNumbers = false;
                if (entries.All(en => en.Number > 0))
                {
                    DialogResult r = MessageBox.Show(this,
                        $"{entries.Count} hue(s) in the file (numbers {entries.Min(en => en.Number)}–{entries.Max(en => en.Number)}).\n\n" +
                        $"Yes = put them at their numbers from the file\nNo = start at the selected hue {_selected + 1}",
                        "Import", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (r == DialogResult.Cancel)
                    {
                        return;
                    }

                    useNumbers = r == DialogResult.Yes;
                }

                var targets = new List<int>();
                for (int i = 0; i < entries.Count; i++)
                {
                    targets.Add(useNumbers ? entries[i].Number - 1 : _selected + i);
                }

                HueHistory.Main.Record($"Import {entries.Count} hue(s)", targets.Where(t => t >= 0 && t < Hues.List.Length), () =>
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        if (targets[i] >= 0 && targets[i] < Hues.List.Length)
                        {
                            entries[i].State.ApplyTo(Hues.List[targets[i]]);
                        }
                    }
                });
            }
        }

        private void ExportPalette(string kind)
        {
            List<int> selected = SelectedIndices;
            string filter = kind switch
            {
                "gpl" => "GIMP palette (*.gpl)|*.gpl",
                "act" => "Adobe colour table (*.act)|*.act",
                _ => "PNG image (*.png)|*.png"
            };

            using (var dlg = new SaveFileDialog { Filter = filter, FileName = $"hues_{selected[0] + 1}.{kind}" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    IEnumerable<Hue> hues = selected.Select(i => Hues.List[i]);
                    switch (kind)
                    {
                        case "gpl":
                            HueFiles.ExportGpl(hues, dlg.FileName, Path.GetFileNameWithoutExtension(dlg.FileName));
                            break;
                        case "act":
                            int written = HueFiles.ExportAct(hues, dlg.FileName);
                            if (written < selected.Count)
                            {
                                MessageBox.Show(this, $"An .act file holds 256 colours: only the first {written} hues were written.", "Export",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            break;
                        default:
                            HueFiles.ExportPng(hues, dlg.FileName);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion
    }
}
