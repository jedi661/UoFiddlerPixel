/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Animations;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Editor for AnimationDefinition.uop: the item animations (fire, water, doors, ...) of the Enhanced Client.
    /// </summary>
    public sealed class EcAnimDefinitionControl : UserControl, IEcEditor
    {
        private readonly ToolStripTextBox _searchBox;
        private readonly ToolStripButton _saveButton;
        private readonly ListView _list;
        private readonly ListView _frameList;
        private readonly NumericUpDown _frameTimeBox;
        private readonly EcPreviewPanel _preview;
        private readonly Label _previewInfo;
        private readonly Button _playButton;
        private readonly Timer _timer;
        private readonly ToolStripStatusLabel _statusLabel;
        private readonly Dictionary<uint, Bitmap> _imageCache = new Dictionary<uint, Bitmap>();

        private EcClient _client;
        private EcAnimDefinitionFile _file;
        private TileArtFile _tileArt;
        private List<EcAnimDefinitionRecord> _view = new List<EcAnimDefinitionRecord>();
        private bool _loaded;
        private bool _updating;
        private int _playFrame;

        public EcAnimDefinitionControl()
        {
            Dock = DockStyle.Fill;

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _searchBox = new ToolStripTextBox { Width = 140, ToolTipText = "Id (decimal / 0x hex) or part of the item name – Enter" };
            _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplyFilter(); } };
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false };

            toolStrip.Items.Add(new ToolStripLabel("Search:"));
            toolStrip.Items.Add(_searchBox);
            toolStrip.Items.Add(new ToolStripButton("Apply", null, (s, e) => ApplyFilter()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("New...", null, (s, e) => AddNew(false)) { ToolTipText = "New animation for an item id" });
            toolStrip.Items.Add(new ToolStripButton("Duplicate...", null, (s, e) => AddNew(true)) { ToolTipText = "Copy the selected animation to another item id" });
            toolStrip.Items.Add(new ToolStripButton("Remove", null, (s, e) => RemoveSelected()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, VirtualMode = true, MultiSelect = false };
            _list.Columns.Add("Id", 70, HorizontalAlignment.Right);
            _list.Columns.Add("Hex", 70);
            _list.Columns.Add("Item name", 200);
            _list.Columns.Add("Frames", 55, HorizontalAlignment.Right);
            _list.Columns.Add("Time", 55, HorizontalAlignment.Right);
            _list.RetrieveVirtualItem += RetrieveItem;
            _list.SelectedIndexChanged += (s, e) => ShowSelected();

            // ---------------- frame editor
            _frameTimeBox = new NumericUpDown { DecimalPlaces = 3, Increment = 0.025m, Minimum = 0.001m, Maximum = 60m, Width = 80 };
            _frameTimeBox.ValueChanged += (s, e) => FrameTimeChanged();

            var timePanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 32, Padding = new Padding(4) };
            timePanel.Controls.Add(new Label { Text = "Time per frame (s):", AutoSize = true, Margin = new Padding(3, 7, 3, 3) });
            timePanel.Controls.Add(_frameTimeBox);
            _playButton = new Button { Text = "Play", Width = 70 };
            _playButton.Click += (s, e) => TogglePlay();
            timePanel.Controls.Add(_playButton);

            _frameList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false };
            _frameList.Columns.Add("#", 35, HorizontalAlignment.Right);
            _frameList.Columns.Add("Item id", 70, HorizontalAlignment.Right);
            _frameList.Columns.Add("Hex", 65);
            _frameList.Columns.Add("Name", 200);
            _frameList.SelectedIndexChanged += (s, e) => { if (!_timer.Enabled) { ShowFrame(_frameList.SelectedIndices.Count > 0 ? _frameList.SelectedIndices[0] : 0); } };
            _frameList.DoubleClick += (s, e) => ChangeFrame();

            var frameStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
            frameStrip.Items.Add(new ToolStripButton("Add frame...", null, (s, e) => AddFrame()));
            frameStrip.Items.Add(new ToolStripButton("Change...", null, (s, e) => ChangeFrame()));
            frameStrip.Items.Add(new ToolStripButton("Remove", null, (s, e) => RemoveFrame()));
            frameStrip.Items.Add(new ToolStripButton("Up", null, (s, e) => MoveFrame(-1)));
            frameStrip.Items.Add(new ToolStripButton("Down", null, (s, e) => MoveFrame(1)));

            var framePanel = new Panel { Dock = DockStyle.Fill };
            framePanel.Controls.Add(_frameList);
            framePanel.Controls.Add(frameStrip);
            framePanel.Controls.Add(timePanel);

            _preview = new EcPreviewPanel { Dock = DockStyle.Fill };
            _previewInfo = new Label { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4), Font = EcDialogs.MonoFont(8.5f) };
            var previewPanel = new Panel { Dock = DockStyle.Fill };
            previewPanel.Controls.Add(_preview);
            previewPanel.Controls.Add(_previewInfo);

            var detailSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            detailSplit.Panel1.Controls.Add(framePanel);
            detailSplit.Panel2.Controls.Add(previewPanel);
            detailSplit.HandleCreated += (s, e) => { try { detailSplit.SplitterDistance = Math.Max(150, detailSplit.Height / 2); } catch (InvalidOperationException) { } };

            var split = new SplitContainer { Dock = DockStyle.Fill };
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(detailSplit);
            split.HandleCreated += (s, e) => { try { split.SplitterDistance = Math.Max(250, split.Width / 2 - 100); } catch (InvalidOperationException) { } };

            var status = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Choose the Enhanced Client folder on top.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            status.Items.Add(_statusLabel);

            _timer = new Timer();
            _timer.Tick += (s, e) => NextFrame();

            Controls.Add(split);
            Controls.Add(toolStrip);
            Controls.Add(status);
        }

        public void SetClient(EcClient client)
        {
            StopPlay();
            ClearImages();
            _client = client;
            _file = null;
            _tileArt = null;
            _view = new List<EcAnimDefinitionRecord>();
            _list.VirtualListSize = 0;
            _frameList.Items.Clear();
            _preview.Image = null;
            _loaded = false;
            _saveButton.Enabled = false;
            _statusLabel.Text = client == null ? "Choose the Enhanced Client folder on top." : "Ready.";
            if (Visible && client != null)
            {
                OnActivated();
            }
        }

        public void OnActivated()
        {
            if (_loaded || _client == null)
            {
                return;
            }
            _loaded = true;

            Cursor = Cursors.WaitCursor;
            try
            {
                _statusLabel.Text = "Loading AnimationDefinition.uop and tileart.uop...";
                Application.DoEvents();

                _file = _client.GetAnimDefinitions();
                if (_file == null)
                {
                    _statusLabel.Text = $"AnimationDefinition.uop was not found in {_client.Folder}.";
                    return;
                }

                _tileArt = _client.FindFile(EcClient.TileArtFileName) != null ? _client.GetTileArt() : null;
                ApplyFilter();
                _statusLabel.Text = $"AnimationDefinition.uop: {_file.Records.Count} item animations" +
                                    (_file.FailedEntries > 0 ? $", {_file.FailedEntries} could not be read" : string.Empty);
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Loading AnimationDefinition.uop failed.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private string ItemName(uint id)
        {
            TileArtRecord record = _tileArt?.Get(id);
            if (record == null || !_client.HasStringDictionary)
            {
                return string.Empty;
            }
            return _client.StringDictionary.Get(record.Entry.NameIndex) ?? string.Empty;
        }

        private void ApplyFilter()
        {
            if (_file == null)
            {
                return;
            }

            string filter = _searchBox.Text.Trim();
            int jump = -1;
            if (filter.Length > 0 && EcDialogs.TryParseNumber(filter, out long id))
            {
                _view = _file.Records.ToList();
                jump = _view.FindIndex(r => r.Id >= id);
            }
            else if (filter.Length > 0)
            {
                _view = _file.Records.Where(r => ItemName(r.Id).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            else
            {
                _view = _file.Records.ToList();
            }

            _list.VirtualListSize = _view.Count;
            _list.Invalidate();
            Select(jump >= 0 ? jump : (_view.Count > 0 ? 0 : -1));
        }

        private void Select(int index)
        {
            _list.SelectedIndices.Clear();
            if (index >= 0 && index < _view.Count)
            {
                _list.SelectedIndices.Add(index);
                _list.EnsureVisible(index);
            }
            else
            {
                ShowSelected();
            }
        }

        private void RetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }

            EcAnimDefinitionRecord r = _view[e.ItemIndex];
            var item = new ListViewItem(r.Id.ToString());
            item.SubItems.Add($"0x{r.Id:X4}");
            item.SubItems.Add(ItemName(r.Id));
            item.SubItems.Add(r.Definition.Frames.Count.ToString());
            item.SubItems.Add(r.Definition.FrameTime.ToString("0.###"));
            if (r.IsModified)
            {
                item.ForeColor = Color.DarkBlue;
            }
            e.Item = item;
        }

        private EcAnimDefinitionRecord SelectedRecord => _list.SelectedIndices.Count == 1 && _list.SelectedIndices[0] < _view.Count ? _view[_list.SelectedIndices[0]] : null;

        private void ShowSelected()
        {
            StopPlay();
            EcAnimDefinitionRecord record = SelectedRecord;
            _updating = true;
            _frameList.BeginUpdate();
            _frameList.Items.Clear();
            if (record != null)
            {
                _frameTimeBox.Value = Math.Max(_frameTimeBox.Minimum, Math.Min(_frameTimeBox.Maximum, (decimal)record.Definition.FrameTime));
                for (int i = 0; i < record.Definition.Frames.Count; i++)
                {
                    uint tile = record.Definition.Frames[i].TileId;
                    var item = new ListViewItem((i + 1).ToString());
                    item.SubItems.Add(tile.ToString());
                    item.SubItems.Add($"0x{tile:X4}");
                    item.SubItems.Add(ItemName(tile));
                    _frameList.Items.Add(item);
                }
            }
            _frameList.EndUpdate();
            _frameTimeBox.Enabled = record != null;
            _updating = false;

            ShowFrame(0);
            if (record != null && record.Definition.Frames.Count > 1)
            {
                StartPlay();
            }
        }

        private void FrameTimeChanged()
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            if (_updating || record == null)
            {
                return;
            }

            record.Definition.FrameTime = (float)_frameTimeBox.Value;
            Modified(record);
            if (_timer.Enabled)
            {
                _timer.Interval = Math.Max(16, (int)(record.Definition.FrameTime * 1000));
            }
        }

        private void Modified(EcAnimDefinitionRecord record)
        {
            _file.MarkModified(record);
            _saveButton.Enabled = true;
            _list.Invalidate();
        }

        // ------------------------------------------------------------------ preview

        private Bitmap GetImage(uint tileId, out string description)
        {
            description = null;
            if (_imageCache.TryGetValue(tileId, out Bitmap cached))
            {
                return cached;
            }

            byte[] data = _client.GetItemImage(tileId, out description);
            Bitmap bitmap = data != null ? EcImageHelper.TryDecode(data, out _) : null;
            _imageCache[tileId] = bitmap;
            return bitmap;
        }

        private void ShowFrame(int index)
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            if (record == null || record.Definition.Frames.Count == 0)
            {
                _preview.Image = null;
                _previewInfo.Text = record == null ? string.Empty : "No frames.";
                return;
            }

            index = Math.Max(0, Math.Min(index, record.Definition.Frames.Count - 1));
            uint tile = record.Definition.Frames[index].TileId;
            Bitmap bitmap = GetImage(tile, out string description);
            // the panel disposes its image, so give it a copy of the cached bitmap
            _preview.Image = bitmap != null ? new Bitmap(bitmap) : null;
            _previewInfo.Text = $"Frame {index + 1}/{record.Definition.Frames.Count}: item 0x{tile:X4}\r\n" +
                                (bitmap == null ? "no image found" : description ?? string.Empty);
        }

        private void StartPlay()
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            if (record == null || record.Definition.Frames.Count == 0)
            {
                return;
            }

            _playFrame = 0;
            _timer.Interval = Math.Max(16, (int)(record.Definition.FrameTime * 1000));
            _timer.Start();
            _playButton.Text = "Stop";
        }

        private void StopPlay()
        {
            _timer.Stop();
            _playButton.Text = "Play";
        }

        private void TogglePlay()
        {
            if (_timer.Enabled)
            {
                StopPlay();
            }
            else
            {
                StartPlay();
            }
        }

        private void NextFrame()
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            if (record == null || record.Definition.Frames.Count == 0)
            {
                StopPlay();
                return;
            }

            _playFrame = (_playFrame + 1) % record.Definition.Frames.Count;
            ShowFrame(_playFrame);
        }

        private void ClearImages()
        {
            foreach (Bitmap bitmap in _imageCache.Values)
            {
                bitmap?.Dispose();
            }
            _imageCache.Clear();
        }

        // ------------------------------------------------------------------ editing

        private static bool AskId(IWin32Window owner, string title, string label, uint suggestion, out uint id)
        {
            id = 0;
            string text = EcDialogs.Prompt(owner, title, label, $"0x{suggestion:X4}");
            if (text == null || !EcDialogs.TryParseNumber(text, out long value) || value < 0 || value > uint.MaxValue)
            {
                return false;
            }
            id = (uint)value;
            return true;
        }

        private void AddNew(bool duplicate)
        {
            if (_file == null)
            {
                return;
            }

            EcAnimDefinitionRecord source = duplicate ? SelectedRecord : null;
            if (duplicate && source == null)
            {
                return;
            }

            if (!AskId(this, duplicate ? "Duplicate animation" : "New animation", "Item id of the animation (decimal or 0x hex):", source?.Id ?? 0, out uint id))
            {
                return;
            }

            if (_file.Get(id) != null)
            {
                EcDialogs.ShowError(this, $"Item 0x{id:X4} already has an animation.");
                return;
            }

            EcAnimDefinitionRecord record = _file.Add(id, source?.Definition);
            _saveButton.Enabled = true;
            _searchBox.Text = string.Empty;
            ApplyFilter();
            Select(_view.IndexOf(record));
            _statusLabel.Text = $"Animation for item 0x{id:X4} added. Set the frames, then save. (The item also needs the 'Animation' flag in TileArt.)";
        }

        private void RemoveSelected()
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            if (record == null || !EcDialogs.Confirm(this, $"Remove the animation of item 0x{record.Id:X4}?"))
            {
                return;
            }

            int index = _list.SelectedIndices[0];
            _file.Remove(record);
            _saveButton.Enabled = true;
            ApplyFilter();
            Select(Math.Min(index, _view.Count - 1));
        }

        private int SelectedFrame => _frameList.SelectedIndices.Count > 0 ? _frameList.SelectedIndices[0] : -1;

        private void RefreshFrames(EcAnimDefinitionRecord record, int select)
        {
            Modified(record);
            ShowSelected();
            if (select >= 0 && select < _frameList.Items.Count)
            {
                StopPlay();
                _frameList.Items[select].Selected = true;
                _frameList.EnsureVisible(select);
            }
        }

        private void AddFrame()
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            if (record == null)
            {
                return;
            }

            List<EcAnimFrame> frames = record.Definition.Frames;
            uint suggestion = frames.Count > 0 ? frames[frames.Count - 1].TileId + 1 : record.Id;
            if (!AskId(this, "Add frame", "Item id shown in the new frame:", suggestion, out uint id))
            {
                return;
            }

            int at = SelectedFrame >= 0 ? SelectedFrame + 1 : frames.Count;
            frames.Insert(at, new EcAnimFrame { TileId = id });
            RefreshFrames(record, at);
        }

        private void ChangeFrame()
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            int index = SelectedFrame;
            if (record == null || index < 0)
            {
                return;
            }

            if (!AskId(this, "Change frame", "Item id shown in this frame:", record.Definition.Frames[index].TileId, out uint id))
            {
                return;
            }

            record.Definition.Frames[index].TileId = id;
            RefreshFrames(record, index);
        }

        private void RemoveFrame()
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            int index = SelectedFrame;
            if (record == null || index < 0)
            {
                return;
            }

            record.Definition.Frames.RemoveAt(index);
            RefreshFrames(record, Math.Min(index, record.Definition.Frames.Count - 1));
        }

        private void MoveFrame(int direction)
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            int index = SelectedFrame;
            int target = index + direction;
            if (record == null || index < 0 || target < 0 || target >= record.Definition.Frames.Count)
            {
                return;
            }

            List<EcAnimFrame> frames = record.Definition.Frames;
            (frames[index], frames[target]) = (frames[target], frames[index]);
            RefreshFrames(record, target);
        }

        private void Save(bool saveAs)
        {
            if (_file == null)
            {
                return;
            }

            string target = _file.Package.FilePath;
            if (saveAs)
            {
                using (var dialog = new SaveFileDialog { Filter = "UOP files (*.uop)|*.uop", FileName = Path.GetFileName(target) })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    target = dialog.FileName;
                }
            }
            else if (!EcDialogs.Confirm(this, "Save AnimationDefinition.uop?\r\nThe old file is kept as .bak next to it.\r\n\r\nThe Enhanced Client must be closed."))
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                _file.Apply();
                _file.Package.Save(target);
                _saveButton.Enabled = false;
                _list.Invalidate();
                _statusLabel.Text = $"Saved {target}.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Saving failed (is the file opened by the client?).", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                ClearImages();
            }
            base.Dispose(disposing);
        }
    }
}
