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
    public sealed partial class EcAnimDefinitionControl : UserControl, IEcEditor
    {
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
            InitializeComponent();
            Dock = DockStyle.Fill;
            Disposed += OnControlDisposed;
        }

        public void SetClient(EcClient client)
        {
            StopPlay();
            ClearImages();
            _client = client;
            _file = null;
            _tileArt = null;
            _view = new List<EcAnimDefinitionRecord>();
            listViewAnimations.VirtualListSize = 0;
            listViewFrames.Items.Clear();
            previewPanel.Image = null;
            _loaded = false;
            toolStripButtonSave.Enabled = false;
            toolStripStatusLabel.Text = client == null ? "Choose the Enhanced Client folder on top." : "Ready.";
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
                toolStripStatusLabel.Text = "Loading AnimationDefinition.uop and tileart.uop...";
                Application.DoEvents();

                _file = _client.GetAnimDefinitions();
                if (_file == null)
                {
                    toolStripStatusLabel.Text = $"AnimationDefinition.uop was not found in {_client.Folder}.";
                    return;
                }

                _tileArt = _client.FindFile(EcClient.TileArtFileName) != null ? _client.GetTileArt() : null;
                ApplyFilter();
                toolStripStatusLabel.Text = $"AnimationDefinition.uop: {_file.Records.Count} item animations" +
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

            string filter = toolStripTextBoxSearch.Text.Trim();
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

            listViewAnimations.VirtualListSize = _view.Count;
            listViewAnimations.Invalidate();
            Select(jump >= 0 ? jump : (_view.Count > 0 ? 0 : -1));
        }

        private void Select(int index)
        {
            listViewAnimations.SelectedIndices.Clear();
            if (index >= 0 && index < _view.Count)
            {
                listViewAnimations.SelectedIndices.Add(index);
                listViewAnimations.EnsureVisible(index);
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

        private EcAnimDefinitionRecord SelectedRecord => listViewAnimations.SelectedIndices.Count == 1 && listViewAnimations.SelectedIndices[0] < _view.Count ? _view[listViewAnimations.SelectedIndices[0]] : null;

        private void ShowSelected()
        {
            StopPlay();
            EcAnimDefinitionRecord record = SelectedRecord;
            _updating = true;
            listViewFrames.BeginUpdate();
            listViewFrames.Items.Clear();
            if (record != null)
            {
                numericUpDownFrameTime.Value = Math.Max(numericUpDownFrameTime.Minimum, Math.Min(numericUpDownFrameTime.Maximum, (decimal)record.Definition.FrameTime));
                for (int i = 0; i < record.Definition.Frames.Count; i++)
                {
                    uint tile = record.Definition.Frames[i].TileId;
                    var item = new ListViewItem((i + 1).ToString());
                    item.SubItems.Add(tile.ToString());
                    item.SubItems.Add($"0x{tile:X4}");
                    item.SubItems.Add(ItemName(tile));
                    listViewFrames.Items.Add(item);
                }
            }
            listViewFrames.EndUpdate();
            numericUpDownFrameTime.Enabled = record != null;
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

            record.Definition.FrameTime = (float)numericUpDownFrameTime.Value;
            Modified(record);
            if (timerPlay.Enabled)
            {
                timerPlay.Interval = Math.Max(16, (int)(record.Definition.FrameTime * 1000));
            }
        }

        private void Modified(EcAnimDefinitionRecord record)
        {
            _file.MarkModified(record);
            toolStripButtonSave.Enabled = true;
            listViewAnimations.Invalidate();
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
                previewPanel.Image = null;
                labelPreviewInfo.Text = record == null ? string.Empty : "No frames.";
                return;
            }

            index = Math.Max(0, Math.Min(index, record.Definition.Frames.Count - 1));
            uint tile = record.Definition.Frames[index].TileId;
            Bitmap bitmap = GetImage(tile, out string description);
            // the panel disposes its image, so give it a copy of the cached bitmap
            previewPanel.Image = bitmap != null ? new Bitmap(bitmap) : null;
            labelPreviewInfo.Text = $"Frame {index + 1}/{record.Definition.Frames.Count}: item 0x{tile:X4}\r\n" +
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
            timerPlay.Interval = Math.Max(16, (int)(record.Definition.FrameTime * 1000));
            timerPlay.Start();
            buttonPlay.Text = "Stop";
        }

        private void StopPlay()
        {
            timerPlay.Stop();
            buttonPlay.Text = "Play";
        }

        private void TogglePlay()
        {
            if (timerPlay.Enabled)
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
            toolStripButtonSave.Enabled = true;
            toolStripTextBoxSearch.Text = string.Empty;
            ApplyFilter();
            Select(_view.IndexOf(record));
            toolStripStatusLabel.Text = $"Animation for item 0x{id:X4} added. Set the frames, then save. (The item also needs the 'Animation' flag in TileArt.)";
        }

        private void RemoveSelected()
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            if (record == null || !EcDialogs.Confirm(this, $"Remove the animation of item 0x{record.Id:X4}?"))
            {
                return;
            }

            int index = listViewAnimations.SelectedIndices[0];
            _file.Remove(record);
            toolStripButtonSave.Enabled = true;
            ApplyFilter();
            Select(Math.Min(index, _view.Count - 1));
        }

        private int SelectedFrame => listViewFrames.SelectedIndices.Count > 0 ? listViewFrames.SelectedIndices[0] : -1;

        private void RefreshFrames(EcAnimDefinitionRecord record, int select)
        {
            Modified(record);
            ShowSelected();
            if (select >= 0 && select < listViewFrames.Items.Count)
            {
                StopPlay();
                listViewFrames.Items[select].Selected = true;
                listViewFrames.EnsureVisible(select);
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
                toolStripButtonSave.Enabled = false;
                listViewAnimations.Invalidate();
                toolStripStatusLabel.Text = $"Saved {target}.";
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

        private void OnControlDisposed(object sender, EventArgs e)
        {
            timerPlay.Dispose();
            ClearImages();
        }

        // ==================================================================== event handlers (designer)

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyFilter();
            }
        }

        private void OnClickApply(object sender, EventArgs e) => ApplyFilter();

        private void OnClickNew(object sender, EventArgs e) => AddNew(false);

        private void OnClickDuplicate(object sender, EventArgs e) => AddNew(true);

        private void OnClickRemove(object sender, EventArgs e) => RemoveSelected();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnListSelectedIndexChanged(object sender, EventArgs e) => ShowSelected();

        private void OnFrameTimeChanged(object sender, EventArgs e) => FrameTimeChanged();

        private void OnClickPlay(object sender, EventArgs e) => TogglePlay();

        private void OnFrameSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!timerPlay.Enabled)
            {
                ShowFrame(listViewFrames.SelectedIndices.Count > 0 ? listViewFrames.SelectedIndices[0] : 0);
            }
        }

        private void OnClickAddFrame(object sender, EventArgs e) => AddFrame();

        private void OnClickChangeFrame(object sender, EventArgs e) => ChangeFrame();

        private void OnClickRemoveFrame(object sender, EventArgs e) => RemoveFrame();

        private void OnClickFrameUp(object sender, EventArgs e) => MoveFrame(-1);

        private void OnClickFrameDown(object sender, EventArgs e) => MoveFrame(1);

        private void OnTimerTick(object sender, EventArgs e) => NextFrame();

    }
}
