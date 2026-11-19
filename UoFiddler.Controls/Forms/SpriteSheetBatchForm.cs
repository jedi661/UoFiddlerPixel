/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Sprite sheets of many items at once: export all items or a range (from id to id) into one or several
    /// sheets (split automatically when a sheet would get too big), import such sheets again (to their original
    /// ids from the .json, or from an address). Planning with warning and time estimate, progress with elapsed
    /// and remaining time, pause / resume and cancel. The work runs in small portions on the UI thread (the art
    /// files are not thread safe), so UOFiddler stays usable.
    /// </summary>
    public sealed class SpriteSheetBatchForm : Form
    {
        private const int SheetPadding = 4;

        private readonly Action<int, Bitmap> _insert;
        private readonly Action _endInsert;

        // export
        private TextBox _fromBox, _toBox, _folderBox, _nameBox;
        private CheckBox _onlyUsed;
        private NumericUpDown _columns, _maxSize, _maxCells;

        // import
        private ListBox _files;
        private ComboBox _placeMode;
        private TextBox _addressBox;

        // progress
        private ProgressBar _progress;
        private Label _progressLabel;
        private Button _pauseButton, _cancelButton, _exportButton, _importButton;
        private TextBox _log;

        // job
        private Timer _timer;
        private Func<bool> _step;     // one unit of work, false = finished
        private Action _finish;
        private readonly Stopwatch _watch = new Stopwatch();
        private int _done, _total;
        private bool _cancel;
        private string _jobName;

        public SpriteSheetBatchForm(Action<int, Bitmap> insert, Action endInsert, int fromId, int toId)
        {
            _insert = insert;
            _endInsert = endInsert;

            Text = "Sprite sheets - export / import a range of items";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(820, 660);
            MinimumSize = new Size(700, 560);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(BuildExportPage(fromId, toId));
            tabs.TabPages.Add(BuildImportPage());
            root.Controls.Add(tabs, 0, 0);

            var prog = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            _progress = new ProgressBar { Width = 360, Height = 22 };
            prog.Controls.Add(_progress);
            _pauseButton = Btn("Pause", (s, e) => TogglePause());
            _cancelButton = Btn("Cancel", (s, e) => CancelJob());
            _pauseButton.Enabled = _cancelButton.Enabled = false;
            prog.Controls.Add(_pauseButton);
            prog.Controls.Add(_cancelButton);
            _progressLabel = new Label { AutoSize = true, Margin = new Padding(6, 6, 3, 3) };
            prog.Controls.Add(_progressLabel);
            root.Controls.Add(prog, 0, 1);

            _log = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font(FontFamily.GenericMonospace, 8.5f) };
            root.Controls.Add(_log, 0, 2);

            FormClosing += (s, e) =>
            {
                if (_timer != null && MessageBox.Show(this, "A job is running. Cancel it and close?", Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                StopTimer();
            };
        }

        #region UI
        private TabPage BuildExportPage(int fromId, int toId)
        {
            var page = new TabPage("Export");
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

            var range = Row();
            range.Controls.Add(Lbl("From id"));
            _fromBox = new TextBox { Width = 90, Text = $"0x{fromId:X4}" };
            range.Controls.Add(_fromBox);
            range.Controls.Add(Lbl("to id"));
            _toBox = new TextBox { Width = 90, Text = $"0x{toId:X4}" };
            range.Controls.Add(_toBox);
            range.Controls.Add(Btn("All", (s, e) =>
            {
                _fromBox.Text = "0x0000";
                _toBox.Text = $"0x{Art.GetMaxExtendedItemId():X4}";
            }));
            _onlyUsed = new CheckBox { Text = "only items with a graphic", AutoSize = true, Checked = true, Margin = new Padding(10, 6, 3, 3) };
            range.Controls.Add(_onlyUsed);
            flow.Controls.Add(range);

            var size = Row();
            size.Controls.Add(Lbl("Columns"));
            _columns = new NumericUpDown { Minimum = 1, Maximum = 256, Value = 16, Width = 60 };
            size.Controls.Add(_columns);
            size.Controls.Add(Lbl("max. sheet size px"));
            _maxSize = new NumericUpDown { Minimum = 256, Maximum = 16384, Value = 4096, Increment = 256, Width = 80 };
            new ToolTip().SetToolTip(_maxSize, "A sheet is never wider or higher than this - more items go into the next sheet (name_002.png ...)");
            size.Controls.Add(_maxSize);
            size.Controls.Add(Lbl("max. items per sheet"));
            _maxCells = new NumericUpDown { Minimum = 1, Maximum = 10000, Value = 256, Width = 70 };
            size.Controls.Add(_maxCells);
            flow.Controls.Add(size);

            var dest = Row();
            dest.Controls.Add(Lbl("Folder"));
            _folderBox = new TextBox { Width = 380, Text = ArtDirectionSettings.Load().ExportFolder ?? Options.OutputPath };
            dest.Controls.Add(_folderBox);
            dest.Controls.Add(Btn("...", (s, e) => ChooseFolder()));
            dest.Controls.Add(Lbl("Name"));
            _nameBox = new TextBox { Width = 120, Text = "items" };
            dest.Controls.Add(_nameBox);
            flow.Controls.Add(dest);

            var go = Row();
            _exportButton = Btn("Plan && export...", (s, e) => StartExport());
            go.Controls.Add(_exportButton);
            flow.Controls.Add(go);

            flow.Controls.Add(Hint("Every sheet gets a .json (cells with item ids) - with it the import puts every graphic back to its id. " +
                                   "Black = transparent, cells separated by magenta lines, every graphic bottom-centred (UO fix point). " +
                                   "A <name>_prompt.txt explains the format for a person or an AI."));
            page.Controls.Add(flow);
            return page;
        }

        private TabPage BuildImportPage()
        {
            var page = new TabPage("Import");
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

            var add = Row();
            add.Controls.Add(Btn("Add sheets...", (s, e) => AddSheets()));
            add.Controls.Add(Btn("Add folder...", (s, e) => AddFolder()));
            add.Controls.Add(Btn("Clear", (s, e) => _files.Items.Clear()));
            flow.Controls.Add(add);

            _files = new ListBox { Width = 760, Height = 120, HorizontalScrollbar = true };
            flow.Controls.Add(_files);

            var place = Row();
            place.Controls.Add(Lbl("Place"));
            _placeMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
            _placeMode.Items.AddRange(new object[]
            {
                "At the original ids (from the .json)",
                "From the address, in a row",
                "From the address, next free slots"
            });
            _placeMode.SelectedIndex = 0;
            place.Controls.Add(_placeMode);
            place.Controls.Add(Lbl("Address"));
            _addressBox = new TextBox { Width = 90, Text = "0x0001" };
            place.Controls.Add(_addressBox);
            _importButton = Btn("Import...", (s, e) => StartImport());
            place.Controls.Add(_importButton);
            flow.Controls.Add(place);

            flow.Controls.Add(Hint("Sheets without .json are cut at the magenta lines; they need 'From the address'. " +
                                   "Before importing you are told how many used items would be replaced. Save the Art afterwards."));
            page.Controls.Add(flow);
            return page;
        }

        private static FlowLayoutPanel Row() => new FlowLayoutPanel { AutoSize = true, WrapContents = true, Margin = new Padding(3, 6, 3, 0) };

        private static Label Hint(string text) => new Label { Text = text, AutoSize = true, MaximumSize = new Size(760, 0), ForeColor = Color.DimGray, Margin = new Padding(6, 10, 3, 3) };

        private static Button Btn(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            return b;
        }

        private static Label Lbl(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 3) };

        private void Log(string text)
        {
            _log.AppendText($"{DateTime.Now:HH:mm:ss}  {text}{Environment.NewLine}");
        }

        private void ChooseFolder()
        {
            using var dlg = new FolderBrowserDialog { SelectedPath = Directory.Exists(_folderBox.Text) ? _folderBox.Text : Options.OutputPath };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _folderBox.Text = dlg.SelectedPath;
                ArtDirectionSettings settings = ArtDirectionSettings.Load();
                settings.ExportFolder = dlg.SelectedPath;
                settings.Save();
            }
        }
        #endregion

        #region Job runner (portions on the UI thread, pause / cancel, time estimate)
        private void StartJob(string name, int total, Func<bool> step, Action finish)
        {
            _jobName = name;
            _total = Math.Max(1, total);
            _done = 0;
            _cancel = false;
            _step = step;
            _finish = finish;
            _watch.Restart();
            _progress.Maximum = _total;
            _progress.Value = 0;
            _pauseButton.Text = "Pause";
            _pauseButton.Enabled = _cancelButton.Enabled = true;
            _exportButton.Enabled = _importButton.Enabled = false;
            _timer = new Timer { Interval = 1 };
            _timer.Tick += (s, e) => Tick();
            _timer.Start();
        }

        private void Tick()
        {
            var budget = Stopwatch.StartNew();
            bool more = true;
            while (more && !_cancel && budget.ElapsedMilliseconds < 60)
            {
                more = _step();
            }

            UpdateProgress();
            if (!more || _cancel)
            {
                StopTimer();
                _watch.Stop();
                _finish?.Invoke();
                Log(_cancel ? $"{_jobName} cancelled after {_done:N0} of {_total:N0} ({Fmt(_watch.Elapsed)})."
                            : $"{_jobName} finished: {_done:N0} in {Fmt(_watch.Elapsed)}.");
                _pauseButton.Enabled = _cancelButton.Enabled = false;
                _exportButton.Enabled = _importButton.Enabled = true;
            }
        }

        private void UpdateProgress()
        {
            _progress.Value = Math.Min(_progress.Maximum, _done);
            double rate = _watch.Elapsed.TotalSeconds > 0 ? _done / _watch.Elapsed.TotalSeconds : 0;
            string remaining = rate > 0 ? Fmt(TimeSpan.FromSeconds((_total - _done) / rate)) : "?";
            _progressLabel.Text = $"{_jobName}: {_done:N0} / {_total:N0}   elapsed {Fmt(_watch.Elapsed)}   remaining ~{remaining}   ({rate:0} per s)";
        }

        private static string Fmt(TimeSpan t) => t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"m\:ss");

        private void TogglePause()
        {
            if (_timer == null)
            {
                return;
            }

            if (_timer.Enabled)
            {
                _timer.Stop();
                _watch.Stop();
                _pauseButton.Text = "Resume";
                _progressLabel.Text += "   - paused";
            }
            else
            {
                _watch.Start();
                _timer.Start();
                _pauseButton.Text = "Pause";
            }
        }

        private void CancelJob()
        {
            if (_timer == null)
            {
                return;
            }

            _cancel = true;
            if (!_timer.Enabled)
            {
                Tick(); // paused: finish now
            }
        }

        private void StopTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }
        #endregion

        #region Export
        private void StartExport()
        {
            int max = Art.GetMaxExtendedItemId();
            if (!Utils.ConvertStringToInt(_fromBox.Text, out int from, 0, max) || !Utils.ConvertStringToInt(_toBox.Text, out int to, 0, max) || to < from)
            {
                Log("Invalid range.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_folderBox.Text))
            {
                ChooseFolder();
                if (string.IsNullOrWhiteSpace(_folderBox.Text))
                {
                    return;
                }
            }

            // phase 1: plan (sizes from the art headers - fast, no decoding)
            var ids = new List<int>();
            var sizes = new List<(int Width, int Height)>();
            int id = from;
            var sample = new Stopwatch();
            int sampled = 0;
            StartJob("Planning", to - from + 1, () =>
            {
                if (id > to)
                {
                    return false;
                }

                if (Art.IsLegalStaticId(id) && (!_onlyUsed.Checked || Art.IsValidStatic(id)))
                {
                    (int w, int h) = ArtSpriteSheet.RawSize(Art.GetRawStatic(id));
                    if (w > 0 && h > 0)
                    {
                        // measure the real export speed on a few items for the time estimate
                        if (sampled < 40)
                        {
                            sample.Start();
                            // never dispose: Art.GetStatic returns the cached bitmap the Items tab shows
                            Bitmap b = Art.GetStatic(id);
                            if (b != null)
                            {
                                ArtDirectionForm.FromBitmap(b, false);
                            }

                            sample.Stop();
                            ++sampled;
                        }

                        ids.Add(id);
                        sizes.Add((w, h));
                    }
                }

                ++id;
                ++_done;
                return true;
            }, () =>
            {
                if (_cancel || ids.Count == 0)
                {
                    if (ids.Count == 0 && !_cancel)
                    {
                        Log("No items with a graphic in this range.");
                    }

                    return;
                }

                List<List<int>> sheets = ArtSpriteSheet.PlanSheets(sizes, (int)_columns.Value, SheetPadding, (int)_maxSize.Value, (int)_maxCells.Value, out List<int> oversize);
                double perItem = sampled > 0 ? sample.Elapsed.TotalSeconds / sampled : 0.002;
                TimeSpan estimate = TimeSpan.FromSeconds((perItem * ids.Count) + (sheets.Count * 0.3));
                (int bw, int bh) = sheets.Select(s =>
                {
                    int mw = s.Max(i => sizes[i].Width), mh = s.Max(i => sizes[i].Height);
                    return ArtSpriteSheet.SheetSize(s.Count, ArtSpriteSheet.EffectiveColumns((int)_columns.Value, SheetPadding, mw, (int)_maxSize.Value), SheetPadding, mw, mh);
                }).OrderByDescending(t => t.Width * (long)t.Height).First();

                string warn = string.Empty;
                if (sheets.Count > 1)
                {
                    warn += $"{Environment.NewLine}The range is too big for one picture: it is split into {sheets.Count} sheets.";
                }

                if (oversize.Count > 0)
                {
                    warn += $"{Environment.NewLine}{oversize.Count} item(s) are bigger than the size limit alone and get a sheet of their own " +
                            $"(e.g. 0x{ids[oversize[0]]:X4} {sizes[oversize[0]].Width}x{sizes[oversize[0]].Height}).";
                }

                if ((long)bw * bh > 64L * 1024 * 1024)
                {
                    warn += $"{Environment.NewLine}WARNING: the biggest sheet has {bw}x{bh} px - many programs can not open such pictures. Lower the size limit.";
                }

                string summary = $"{ids.Count:N0} items  ->  {sheets.Count} sheet(s), biggest {bw} x {bh} px.{warn}" +
                                 $"{Environment.NewLine}{Environment.NewLine}Estimated time: ~{Fmt(estimate)}" +
                                 $"{Environment.NewLine}Folder: {_folderBox.Text}{Environment.NewLine}{Environment.NewLine}Export now?";
                Log(summary.Replace(Environment.NewLine, " "));
                if (MessageBox.Show(this, summary, "Export sprite sheets", MessageBoxButtons.YesNo,
                        string.IsNullOrEmpty(warn) ? MessageBoxIcon.Question : MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    BeginInvoke((Action)(() => RunExport(ids, sheets)));
                }
            });
        }

        private void RunExport(List<int> ids, List<List<int>> sheets)
        {
            string folder = _folderBox.Text;
            Directory.CreateDirectory(folder);
            string baseName = string.IsNullOrWhiteSpace(_nameBox.Text) ? "items" : _nameBox.Text.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                baseName = baseName.Replace(c, '_');
            }

            int sheetIndex = 0, inSheet = 0;
            var sprites = new List<(string Label, int ItemId, ArtPixelImage Image)>();
            SpriteSheetInfo firstInfo = null;

            StartJob("Export", ids.Count, () =>
            {
                if (sheetIndex >= sheets.Count)
                {
                    return false;
                }

                int itemId = ids[sheets[sheetIndex][inSheet]];
                Bitmap b = Art.GetStatic(itemId); // cached bitmap: read only, never dispose
                ArtPixelImage img = b == null ? null : ArtDirectionForm.FromBitmap(b, false);
                string name = TileData.ItemTable != null && itemId < TileData.ItemTable.Length ? (TileData.ItemTable[itemId].Name ?? string.Empty).Trim() : string.Empty;
                sprites.Add(($"0x{itemId:X4} {name}".Trim(), itemId, img));

                ++_done;
                if (++inSheet >= sheets[sheetIndex].Count)
                {
                    WriteSheet(folder, baseName, sheetIndex, sheets.Count, sprites, ref firstInfo);
                    sprites.Clear();
                    inSheet = 0;
                    ++sheetIndex;
                }

                return true;
            }, () =>
            {
                if (_cancel && sprites.Count > 0)
                {
                    // keep what is done: the started sheet is written too
                    WriteSheet(folder, baseName, sheetIndex, sheets.Count, sprites, ref firstInfo);
                }

                if (firstInfo != null)
                {
                    File.WriteAllText(Path.Combine(folder, $"{baseName}_prompt.txt"), ArtSpriteSheet.AiPrompt(firstInfo));
                }
            });
        }

        private void WriteSheet(string folder, string baseName, int index, int count, List<(string Label, int ItemId, ArtPixelImage Image)> sprites, ref SpriteSheetInfo firstInfo)
        {
            string file = count > 1 ? $"{baseName}_{index + 1:D3}" : baseName;
            int mw = sprites.Max(s => s.Image?.Width ?? 1);
            int cols = ArtSpriteSheet.EffectiveColumns((int)_columns.Value, SheetPadding, mw, (int)_maxSize.Value);
            ArtPixelImage sheet = ArtSpriteSheet.Compose(sprites, cols, SheetPadding, file, out SpriteSheetInfo info);
            string png = Path.Combine(folder, file + ".png");
            using (Bitmap bmp = ArtDirectionForm.ToExportBitmap(sheet, Color.Black))
            {
                bmp.Save(png, ImageFormat.Png);
            }

            File.WriteAllText(Path.Combine(folder, file + ".json"), ArtSpriteSheet.ToJson(info));
            firstInfo ??= info;
            Log($"{file}.png  {sheet.Width}x{sheet.Height}  {sprites.Count} items (0x{sprites[0].ItemId:X4} - 0x{sprites[^1].ItemId:X4})");
        }
        #endregion

        #region Import
        private void AddSheets()
        {
            using var dlg = new OpenFileDialog { Filter = "Sprite sheets (*.png)|*.png", Multiselect = true };
            string folder = ArtDirectionSettings.Load().ExportFolder;
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                dlg.InitialDirectory = folder;
            }

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                foreach (string f in dlg.FileNames.OrderBy(f => f))
                {
                    if (!_files.Items.Contains(f))
                    {
                        _files.Items.Add(f);
                    }
                }
            }
        }

        private void AddFolder()
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            foreach (string f in Directory.GetFiles(dlg.SelectedPath, "*.png").OrderBy(f => f))
            {
                if (!_files.Items.Contains(f))
                {
                    _files.Items.Add(f);
                }
            }
        }

        private void StartImport()
        {
            List<string> files = _files.Items.Cast<string>().ToList();
            if (files.Count == 0)
            {
                Log("Add sheets first.");
                return;
            }

            int mode = _placeMode.SelectedIndex;
            int max = Art.GetMaxExtendedItemId();
            int next = 0;
            if (mode > 0 && !Utils.ConvertStringToInt(_addressBox.Text, out next, 0, max))
            {
                Log($"Invalid address '{_addressBox.Text}'.");
                return;
            }

            // count from the .json files: cells and (for original ids) used targets
            var infos = files.Select(f => File.Exists(Path.ChangeExtension(f, ".json")) ? ArtSpriteSheet.FromJson(File.ReadAllText(Path.ChangeExtension(f, ".json"))) : null).ToList();
            int known = infos.Sum(i => i?.Cells.Count(c => c.SpriteWidth > 0) ?? 0);
            int withoutJson = infos.Count(i => i == null);
            if (mode == 0 && withoutJson > 0)
            {
                Log($"{withoutJson} sheet(s) have no .json - choose 'From the address' for them.");
                return;
            }

            int used = mode == 0 ? infos.Sum(i => i.Cells.Count(c => c.SpriteWidth > 0 && c.ItemId >= 0 && Art.IsLegalStaticId(c.ItemId) && Art.IsValidStatic(c.ItemId))) : 0;
            string msg = $"{files.Count} sheet(s), {(known > 0 ? $"{known:N0}" : "?")} graphic(s)" +
                         (withoutJson > 0 ? $" (+ sheets without .json, cut at the magenta lines)" : string.Empty) + "." +
                         (used > 0 ? $"{Environment.NewLine}{used:N0} used item(s) will be replaced!" : string.Empty) +
                         $"{Environment.NewLine}{Environment.NewLine}Import now?";
            if (MessageBox.Show(this, msg, "Import sprite sheets", MessageBoxButtons.YesNo, used > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            var planner = new SlotPlanner(id => !Art.IsValidStatic(id), Art.IsLegalStaticId, max);
            int fileIndex = 0, inserted = 0, skipped = 0;
            var queue = new Queue<(SpriteSheetCell Cell, ArtPixelImage Image)>();

            StartJob("Import", Math.Max(1, known), () =>
            {
                if (queue.Count == 0)
                {
                    if (fileIndex >= files.Count)
                    {
                        return false;
                    }

                    // load the next sheet
                    string f = files[fileIndex];
                    SpriteSheetInfo info = infos[fileIndex];
                    ++fileIndex;
                    try
                    {
                        ArtPixelImage sheet;
                        using (var bmp = new Bitmap(f))
                        {
                            sheet = ArtDirectionForm.FromBitmap(bmp, false);
                        }

                        List<(SpriteSheetCell Cell, ArtPixelImage Image)> cells = ArtSpriteSheet.Slice(sheet, info);
                        foreach ((SpriteSheetCell Cell, ArtPixelImage Image) c in cells.Where(c => c.Image != null))
                        {
                            queue.Enqueue(c);
                        }

                        if (info == null)
                        {
                            _total += cells.Count(c => c.Image != null);
                            _progress.Maximum = _total;
                        }

                        Log($"{Path.GetFileName(f)}: {queue.Count} graphic(s)");
                    }
                    catch (Exception ex)
                    {
                        Log($"{Path.GetFileName(f)}: {ex.Message}");
                    }

                    return true;
                }

                (SpriteSheetCell cell, ArtPixelImage image) = queue.Dequeue();
                int target = -1;
                if (mode == 0)
                {
                    target = cell.ItemId;
                }
                else if (mode == 1)
                {
                    target = next++;
                }
                else
                {
                    List<int> one = planner.Plan(SlotPlacementMode.NextFree, next, 1);
                    if (one != null)
                    {
                        target = one[0];
                        next = target + 1;
                    }
                }

                if (target >= 0 && Art.IsLegalStaticId(target))
                {
                    _insert?.Invoke(target, ArtDirectionForm.ToBitmap(image));
                    ++inserted;
                }
                else
                {
                    ++skipped;
                    if (skipped <= 20)
                    {
                        Log($"skipped: {cell.Label} (no valid target id)");
                    }
                }

                ++_done;
                return true;
            }, () =>
            {
                _endInsert?.Invoke();
                Log($"{inserted:N0} graphic(s) inserted{(skipped > 0 ? $", {skipped} skipped" : string.Empty)}. Save the Art to write the files.");
            });
        }
        #endregion
    }
}
