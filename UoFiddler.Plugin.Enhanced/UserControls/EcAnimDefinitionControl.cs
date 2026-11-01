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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Animations;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Editor for AnimationDefinition.uop: the item animations (fire, water, doors, ...) of the Enhanced Client.
    /// The ids are the item ids, like in the classic animdata.mul; the frames are item ids too, whose images come from
    /// tileart (EC world art, often an area of an image shared by several frames). Many animated items have no EC
    /// image ("blank" or none) – the client shows them in another way; here the legacy art is shown for them.
    /// The classic animation (animdata.mul + art of the loaded classic client) can be shown next to it.
    /// </summary>
    public sealed partial class EcAnimDefinitionControl : UserControl, IEcEditor
    {
        /// <summary>One row of the list: an EC animation and / or the classic animation of the same item id.</summary>
        private sealed class AnimRow
        {
            public uint Id;
            public EcAnimDefinitionRecord Record;
            public Animdata.AnimdataEntry Classic;
        }

        private enum ListFilter
        {
            All = 0,
            CompleteEcImages = 1,
            MissingEcImages = 2,
            DifferentFromClassic = 3,
            NotInClassic = 4,
            OnlyInClassic = 5
        }

        private EcClient _client;
        private EcAnimDefinitionFile _file;
        private TileArtFile _tileArt;
        private EcItemSprites _sprites;
        private List<AnimRow> _view = new List<AnimRow>();
        private bool _loaded;
        private bool _updating;
        private int _playFrame;
        private int _classicFrame;

        public EcAnimDefinitionControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            Disposed += OnControlDisposed;

            // runtime only: defaults of the combo boxes
            _updating = true;
            toolStripComboBoxFilter.SelectedIndex = 0;
            toolStripComboBoxArt.SelectedIndex = 0;
            _updating = false;
        }

        public void SetClient(EcClient client)
        {
            StopPlay();
            _sprites?.Clear();
            _sprites = null;
            _client = client;
            _file = null;
            _tileArt = null;
            _view = new List<AnimRow>();
            listViewAnimations.VirtualListSize = 0;
            listViewFrames.Items.Clear();
            previewPanel.Image = null;
            previewPanelClassic.Image = null;
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
                _sprites = new EcItemSprites(_client);
                ApplyFilter();
                int classic = ClassicTable.Count;
                toolStripStatusLabel.Text = $"AnimationDefinition.uop: {_file.Records.Count} item animations" +
                                            (_file.FailedEntries > 0 ? $", {_file.FailedEntries} could not be read" : string.Empty) +
                                            (classic > 0 ? $"  –  classic animdata.mul: {classic} animations" : "  –  no classic animdata.mul loaded");
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

        private bool LegacyMode => toolStripComboBoxArt.SelectedIndex == 1;

        private string ItemName(uint id)
        {
            TileArtRecord record = _tileArt?.Get(id);
            if (record == null || !_client.HasStringDictionary)
            {
                return string.Empty;
            }
            return _client.StringDictionary.Get(record.Entry.NameIndex) ?? string.Empty;
        }

        // ------------------------------------------------------------------ classic client

        /// <summary>animdata.mul of the classic client loaded in UoFiddler (empty if there is none).</summary>
        private static Dictionary<int, Animdata.AnimdataEntry> ClassicTable
        {
            get
            {
                try
                {
                    return Animdata.AnimData ?? new Dictionary<int, Animdata.AnimdataEntry>();
                }
                catch (Exception)
                {
                    return new Dictionary<int, Animdata.AnimdataEntry>();
                }
            }
        }

        private static Animdata.AnimdataEntry ClassicEntry(uint id) =>
            id <= int.MaxValue && ClassicTable.TryGetValue((int)id, out Animdata.AnimdataEntry entry) ? entry : null;

        /// <summary>Item ids of the classic frames: id + frame offset.</summary>
        private static List<uint> ClassicFrames(uint id, Animdata.AnimdataEntry entry)
        {
            var frames = new List<uint>();
            if (entry == null)
            {
                return frames;
            }
            for (int i = 0; i < entry.FrameCount && i < entry.FrameData.Length; i++)
            {
                frames.Add((uint)Math.Max(0, (int)id + entry.FrameData[i]));
            }
            return frames;
        }

        /// <summary>Classic speed: frame interval × 100 ms (like the AnimData tab of UoFiddler).</summary>
        private static int ClassicIntervalMs(Animdata.AnimdataEntry entry) => Math.Max(50, 100 * (entry?.FrameInterval ?? 1));

        private static Bitmap ClassicImage(uint id)
        {
            try
            {
                return id <= int.MaxValue ? Art.GetStatic((int)id) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string ClassicCompare(AnimRow row)
        {
            if (row.Classic == null)
            {
                return "–";
            }
            if (row.Record == null)
            {
                return "only classic";
            }
            List<uint> classic = ClassicFrames(row.Id, row.Classic);
            return classic.SequenceEqual(row.Record.Definition.Frames.Select(f => f.TileId)) ? "same" : $"different ({classic.Count})";
        }

        private int EcImageCount(EcAnimDefinitionRecord record) =>
            record == null || _sprites == null ? 0 : record.Definition.Frames.Count(f => IsEc(_sprites.PredictKind(f.TileId)));

        private static bool IsEc(EcSpriteKind kind) => kind == EcSpriteKind.WorldArt || kind == EcSpriteKind.WorldArtAtlas;

        // ------------------------------------------------------------------ list

        private void ApplyFilter()
        {
            if (_file == null)
            {
                return;
            }

            var filter = (ListFilter)Math.Max(0, toolStripComboBoxFilter.SelectedIndex);
            IEnumerable<AnimRow> rows;
            if (filter == ListFilter.OnlyInClassic)
            {
                var ec = new HashSet<uint>(_file.Records.Select(r => r.Id));
                rows = ClassicTable.Keys.Where(k => k >= 0 && !ec.Contains((uint)k)).OrderBy(k => k)
                                   .Select(k => new AnimRow { Id = (uint)k, Classic = ClassicTable[k] });
            }
            else
            {
                rows = _file.Records.Select(r => new AnimRow { Id = r.Id, Record = r, Classic = ClassicEntry(r.Id) });
                switch (filter)
                {
                    case ListFilter.CompleteEcImages:
                        rows = rows.Where(r => r.Record.Definition.Frames.Count > 0 && EcImageCount(r.Record) == r.Record.Definition.Frames.Count);
                        break;
                    case ListFilter.MissingEcImages:
                        rows = rows.Where(r => EcImageCount(r.Record) < r.Record.Definition.Frames.Count);
                        break;
                    case ListFilter.DifferentFromClassic:
                        rows = rows.Where(r => r.Classic != null && ClassicCompare(r) != "same");
                        break;
                    case ListFilter.NotInClassic:
                        rows = rows.Where(r => r.Classic == null);
                        break;
                }
            }

            string search = toolStripTextBoxSearch.Text.Trim();
            int jump = -1;
            _view = rows.ToList();
            if (search.Length > 0 && EcDialogs.TryParseNumber(search, out long id))
            {
                jump = _view.FindIndex(r => r.Id >= id);
            }
            else if (search.Length > 0)
            {
                _view = _view.Where(r => ItemName(r.Id).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            listViewAnimations.VirtualListSize = _view.Count;
            listViewAnimations.Invalidate();
            Select(jump >= 0 ? jump : (_view.Count > 0 ? 0 : -1));
            if (filter != ListFilter.All)
            {
                toolStripStatusLabel.Text = $"{_view.Count} animations: {toolStripComboBoxFilter.SelectedItem}";
            }
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

        private void SelectRecord(EcAnimDefinitionRecord record) => Select(_view.FindIndex(r => r.Record == record));

        private void RetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }

            AnimRow row = _view[e.ItemIndex];
            var item = new ListViewItem(row.Id.ToString());
            item.SubItems.Add($"0x{row.Id:X4}");
            item.SubItems.Add(ItemName(row.Id));
            if (row.Record != null)
            {
                int frames = row.Record.Definition.Frames.Count;
                int ec = EcImageCount(row.Record);
                item.SubItems.Add(frames.ToString());
                item.SubItems.Add(row.Record.Definition.FrameTime.ToString("0.###"));
                item.SubItems.Add(ec == frames ? "all" : ec == 0 ? "none" : $"{ec}/{frames}");
                if (ec < frames)
                {
                    item.ForeColor = ec == 0 ? Color.Firebrick : Color.DarkOrange;
                }
            }
            else
            {
                item.SubItems.Add(row.Classic?.FrameCount.ToString() ?? string.Empty);
                item.SubItems.Add(string.Empty);
                item.SubItems.Add("no EC animation");
                item.ForeColor = Color.Gray;
            }
            item.SubItems.Add(ClassicCompare(row));
            if (row.Record?.IsModified == true)
            {
                item.ForeColor = Color.DarkBlue;
            }
            e.Item = item;
        }

        private AnimRow SelectedRow => listViewAnimations.SelectedIndices.Count == 1 && listViewAnimations.SelectedIndices[0] < _view.Count ? _view[listViewAnimations.SelectedIndices[0]] : null;

        private EcAnimDefinitionRecord SelectedRecord => SelectedRow?.Record;

        private void ShowSelected()
        {
            StopPlay();
            AnimRow row = SelectedRow;
            EcAnimDefinitionRecord record = row?.Record;
            List<uint> classic = row != null ? ClassicFrames(row.Id, row.Classic) : new List<uint>();
            _updating = true;
            listViewFrames.BeginUpdate();
            listViewFrames.Items.Clear();
            if (record != null)
            {
                numericUpDownFrameTime.Value = Math.Max(numericUpDownFrameTime.Minimum, Math.Min(numericUpDownFrameTime.Maximum, (decimal)record.Definition.FrameTime));
            }
            int count = Math.Max(record?.Definition.Frames.Count ?? 0, classic.Count);
            for (int i = 0; i < count; i++)
            {
                var item = new ListViewItem((i + 1).ToString());
                if (record != null && i < record.Definition.Frames.Count)
                {
                    uint tile = record.Definition.Frames[i].TileId;
                    EcSpriteKind kind = _sprites.PredictKind(tile);
                    item.SubItems.Add(tile.ToString());
                    item.SubItems.Add($"0x{tile:X4}");
                    item.SubItems.Add(ItemName(tile));
                    item.SubItems.Add(KindText(kind));
                    if (!IsEc(kind))
                    {
                        item.ForeColor = Color.Firebrick;
                    }
                }
                else
                {
                    item.SubItems.Add(string.Empty);
                    item.SubItems.Add(string.Empty);
                    item.SubItems.Add(string.Empty);
                    item.SubItems.Add(string.Empty);
                    item.ForeColor = Color.Gray;
                }
                item.SubItems.Add(i < classic.Count ? $"0x{classic[i]:X4}" : "–");
                listViewFrames.Items.Add(item);
            }
            listViewFrames.EndUpdate();
            numericUpDownFrameTime.Enabled = record != null;
            _updating = false;

            _playFrame = 0;
            _classicFrame = 0;
            ShowFrame(0);
            ShowClassicFrame(0);
            if (count > 1)
            {
                StartPlay();
            }
        }

        private static string KindText(EcSpriteKind kind)
        {
            switch (kind)
            {
                case EcSpriteKind.WorldArt:
                    return "EC image";
                case EcSpriteKind.WorldArtAtlas:
                    return "EC image (shared)";
                case EcSpriteKind.LegacyEcBlank:
                    return "no EC image (blank)";
                default:
                    return "no EC image";
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

        /// <summary>
        /// Draws one frame on a canvas that fits every frame of the animation, so the animation doesn't jump.
        /// Positions are relative to the tile: X from its left corner, Y from its bottom corner.
        /// </summary>
        private Bitmap Compose(IList<(Bitmap Image, int X, int Y)> frames, int index, int tileWidth)
        {
            int half = tileWidth / 2;
            int minX = 0, minY = -tileWidth, maxX = tileWidth, maxY = 0;
            foreach (var (image, x, y) in frames)
            {
                if (image != null)
                {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x + image.Width);
                    maxY = Math.Max(maxY, y + image.Height);
                }
            }
            const int margin = 6;
            int dx = margin - minX, dy = margin - minY;
            var canvas = new Bitmap(maxX - minX + (2 * margin), maxY - minY + (2 * margin), PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(canvas))
            {
                if (checkBoxGround.Checked)
                {
                    var diamond = new[] { new Point(dx + half, dy - tileWidth), new Point(dx + tileWidth, dy - half), new Point(dx + half, dy), new Point(dx, dy - half) };
                    using (var brush = new SolidBrush(Color.FromArgb(40, Color.SteelBlue)))
                    using (var pen = new Pen(Color.FromArgb(110, Color.SteelBlue)))
                    {
                        g.FillPolygon(brush, diamond);
                        g.DrawPolygon(pen, diamond);
                    }
                }
                if (index >= 0 && index < frames.Count && frames[index].Image != null)
                {
                    g.DrawImageUnscaled(frames[index].Image, frames[index].X + dx, frames[index].Y + dy);
                }
            }
            return canvas;
        }

        private void ShowFrame(int index)
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            if (!checkBoxShowEc.Checked)
            {
                return;
            }
            if (record == null || record.Definition.Frames.Count == 0)
            {
                previewPanel.Image = null;
                labelPreviewInfo.Text = SelectedRow == null ? string.Empty : record == null ? "No EC animation for this item." : "No frames.";
                return;
            }

            bool legacy = LegacyMode;
            List<EcItemSprite> sprites = record.Definition.Frames.Select(f => _sprites.Get(f.TileId, legacy)).ToList();
            index = Math.Max(0, Math.Min(index, sprites.Count - 1));
            previewPanel.Image = Compose(sprites.Select(s => (s.Image, s.OffsetX, s.OffsetY)).ToList(), index, EcItemSprites.TileWidth(legacy));
            EcItemSprite sprite = sprites[index];
            int ec = sprites.Count(s => s.IsEnhanced);
            labelPreviewInfo.Text = $"Frame {index + 1}/{sprites.Count}: item 0x{record.Definition.Frames[index].TileId:X4}   {record.Definition.FrameTime:0.###} s\r\n" +
                                    (sprite.Image == null ? "no image found" : sprite.Source) + "\r\n" +
                                    (legacy ? "legacy art" : $"EC images: {ec} of {sprites.Count} frames");
        }

        private void ShowClassicFrame(int index)
        {
            AnimRow row = SelectedRow;
            if (!checkBoxShowClassic.Checked)
            {
                return;
            }
            List<uint> frames = row != null ? ClassicFrames(row.Id, row.Classic) : new List<uint>();
            if (frames.Count == 0)
            {
                previewPanelClassic.Image = null;
                labelClassicInfo.Text = row == null ? string.Empty
                    : ClassicTable.Count == 0 ? "No classic animdata.mul loaded (classic client path in UoFiddler)."
                    : "No classic animation for this item.";
                return;
            }

            var images = frames.Select(f =>
            {
                Bitmap image = ClassicImage(f);
                return (image, image != null ? 22 - (image.Width / 2) : 0, image != null ? -image.Height : 0);
            }).ToList();
            index = Math.Max(0, Math.Min(index, images.Count - 1));
            previewPanelClassic.Image = Compose(images, index, 44);
            labelClassicInfo.Text = $"Frame {index + 1}/{frames.Count}: item 0x{frames[index]:X4}   {ClassicIntervalMs(row.Classic) / 1000.0:0.###} s\r\n" +
                                    $"interval {row.Classic.FrameInterval}, start {row.Classic.FrameStart}\r\n" +
                                    (images[index].image == null ? "no classic art" : "classic art");
        }

        private void UpdateCompareView()
        {
            splitContainerCompare.Panel1Collapsed = !checkBoxShowEc.Checked && checkBoxShowClassic.Checked;
            splitContainerCompare.Panel2Collapsed = !checkBoxShowClassic.Checked;
            if (!checkBoxShowEc.Checked)
            {
                previewPanel.Image = null;
            }
            ShowFrame(_playFrame);
            ShowClassicFrame(_classicFrame);
        }

        private void StartPlay()
        {
            AnimRow row = SelectedRow;
            if (row == null)
            {
                return;
            }

            if (row.Record != null && row.Record.Definition.Frames.Count > 1)
            {
                _playFrame = 0;
                timerPlay.Interval = Math.Max(16, (int)(row.Record.Definition.FrameTime * 1000));
                timerPlay.Start();
            }
            if (row.Classic != null && row.Classic.FrameCount > 1)
            {
                _classicFrame = 0;
                timerClassic.Interval = ClassicIntervalMs(row.Classic);
                timerClassic.Start();
            }
            buttonPlay.Text = timerPlay.Enabled || timerClassic.Enabled ? "Stop" : "Play";
        }

        private void StopPlay()
        {
            timerPlay.Stop();
            timerClassic.Stop();
            buttonPlay.Text = "Play";
        }

        private void TogglePlay()
        {
            if (timerPlay.Enabled || timerClassic.Enabled)
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
                timerPlay.Stop();
                return;
            }

            _playFrame = (_playFrame + 1) % record.Definition.Frames.Count;
            ShowFrame(_playFrame);
        }

        private void NextClassicFrame()
        {
            AnimRow row = SelectedRow;
            if (row?.Classic == null || row.Classic.FrameCount == 0)
            {
                timerClassic.Stop();
                return;
            }

            _classicFrame = (_classicFrame + 1) % row.Classic.FrameCount;
            ShowClassicFrame(_classicFrame);
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

            if (!AskId(this, duplicate ? "Duplicate animation" : "New animation", "Item id of the animation (decimal or 0x hex):", source?.Id ?? SelectedRow?.Id ?? 0, out uint id))
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
            toolStripComboBoxFilter.SelectedIndex = 0;
            ApplyFilter();
            SelectRecord(record);
            toolStripStatusLabel.Text = $"Animation for item 0x{id:X4} added. Set the frames, then save. (The item also needs the 'Animation' flag in TileArt.)";
        }

        /// <summary>
        /// Takes the frames and the speed of the classic animation (animdata.mul); creates the EC animation if it is missing.
        /// </summary>
        private void TakeFromClassic()
        {
            AnimRow row = SelectedRow;
            if (_file == null || row == null)
            {
                return;
            }
            if (row.Classic == null)
            {
                EcDialogs.ShowError(this, $"Item 0x{row.Id:X4} has no classic animation (or no classic animdata.mul is loaded).");
                return;
            }

            List<uint> frames = ClassicFrames(row.Id, row.Classic);
            EcAnimDefinitionRecord record = row.Record;
            if (record != null && !EcDialogs.Confirm(this, $"Replace the {record.Definition.Frames.Count} EC frames of item 0x{row.Id:X4} with the {frames.Count} classic frames?"))
            {
                return;
            }

            record ??= _file.Add(row.Id);
            record.Definition.Frames.Clear();
            record.Definition.Frames.AddRange(frames.Select(f => new EcAnimFrame { TileId = f }));
            record.Definition.FrameTime = ClassicIntervalMs(row.Classic) / 1000f;
            Modified(record);
            if (row.Record == null)
            {
                toolStripComboBoxFilter.SelectedIndex = 0;
                ApplyFilter();
            }
            else
            {
                listViewAnimations.Invalidate();
            }
            SelectRecord(record);
            toolStripStatusLabel.Text = $"Item 0x{row.Id:X4}: {frames.Count} frames taken from classic. Save to keep them.";
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

            int at = SelectedFrame >= 0 ? Math.Min(SelectedFrame + 1, frames.Count) : frames.Count;
            frames.Insert(at, new EcAnimFrame { TileId = id });
            RefreshFrames(record, at);
        }

        private void ChangeFrame()
        {
            EcAnimDefinitionRecord record = SelectedRecord;
            int index = SelectedFrame;
            if (record == null || index < 0 || index >= record.Definition.Frames.Count)
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
            if (record == null || index < 0 || index >= record.Definition.Frames.Count)
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
            if (record == null || index < 0 || index >= record.Definition.Frames.Count || target < 0 || target >= record.Definition.Frames.Count)
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
            timerClassic.Dispose();
            _sprites?.Clear();
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

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                ApplyFilter();
            }
        }

        private void OnArtChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                ShowFrame(_playFrame);
            }
        }

        private void OnClickNew(object sender, EventArgs e) => AddNew(false);

        private void OnClickDuplicate(object sender, EventArgs e) => AddNew(true);

        private void OnClickFromClassic(object sender, EventArgs e) => TakeFromClassic();

        private void OnClickRemove(object sender, EventArgs e) => RemoveSelected();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnClickCopyId(object sender, EventArgs e)
        {
            AnimRow row = SelectedRow;
            if (row != null)
            {
                Clipboard.SetText($"0x{row.Id:X4}");
            }
        }

        private void OnListSelectedIndexChanged(object sender, EventArgs e) => ShowSelected();

        private void OnFrameTimeChanged(object sender, EventArgs e) => FrameTimeChanged();

        private void OnClickPlay(object sender, EventArgs e) => TogglePlay();

        private void OnCompareViewChanged(object sender, EventArgs e) => UpdateCompareView();

        private void OnGroundChanged(object sender, EventArgs e)
        {
            ShowFrame(_playFrame);
            ShowClassicFrame(_classicFrame);
        }

        private void OnFrameSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!timerPlay.Enabled && !timerClassic.Enabled)
            {
                int index = listViewFrames.SelectedIndices.Count > 0 ? listViewFrames.SelectedIndices[0] : 0;
                _playFrame = index;
                _classicFrame = index;
                ShowFrame(index);
                ShowClassicFrame(index);
            }
        }

        private void OnClickAddFrame(object sender, EventArgs e) => AddFrame();

        private void OnClickChangeFrame(object sender, EventArgs e) => ChangeFrame();

        private void OnClickRemoveFrame(object sender, EventArgs e) => RemoveFrame();

        private void OnClickFrameUp(object sender, EventArgs e) => MoveFrame(-1);

        private void OnClickFrameDown(object sender, EventArgs e) => MoveFrame(1);

        private void OnTimerTick(object sender, EventArgs e) => NextFrame();

        private void OnTimerClassicTick(object sender, EventArgs e) => NextClassicFrame();
    }
}
