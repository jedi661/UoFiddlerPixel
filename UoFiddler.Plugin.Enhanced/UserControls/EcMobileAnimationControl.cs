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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Animations;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Viewer / editor for the mobile animations of the Enhanced Client (AnimationFrame1-6.uop, Paperdoll.uop, "AMOU" format).
    /// </summary>
    public sealed partial class EcMobileAnimationControl : UserControl, IEcEditor
    {
        private static readonly string[] _files =
        {
            "AnimationFrame1.uop", "AnimationFrame2.uop", "AnimationFrame3.uop",
            "AnimationFrame4.uop", "AnimationFrame5.uop", "AnimationFrame6.uop", "Paperdoll.uop"
        };

        private static readonly Regex _nameRegex = new Regex(@"^build/(animationframe|paperdoll)/(\d+)/(\d+)\.bin$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private sealed class ActionEntry
        {
            public string File;
            public EcUopPackage Package;
            public EcUopEntry Entry;
            public int Body;
            public int Action;
        }

        private EcClient _client;
        private bool _loaded;
        private List<(int Body, List<ActionEntry> Actions)> _bodies = new List<(int, List<ActionEntry>)>();
        private List<(int Body, List<ActionEntry> Actions)> _bodyView = new List<(int, List<ActionEntry>)>();
        private ActionEntry _current;
        private EcAnimation _animation;
        private List<EcAnimationFrame> _directionFrames = new List<EcAnimationFrame>();
        private int _playIndex;
        private bool _updating;

        // view direction on screen, clockwise from "up": 0 = ↑, 1 = ↗, 2 = →, 3 = ↘, 4 = ↓, 5 = ↙, 6 = ←, 7 = ↖.
        // Like the classic client only 5 directions are stored (0 = ↓, 1 = ↙, 2 = ←, 3 = ↖, 4 = ↑),
        // ↗ → ↘ are the mirrored ↖ ← ↙.
        private static readonly int[] _viewStored = { 4, 3, 2, 1, 0, 1, 2, 3 };
        private static readonly bool[] _viewMirrored = { false, true, true, true, false, false, false, false };
        private static readonly string[] _viewArrows = { "↑", "↗", "→", "↘", "↓", "↙", "←", "↖" };
        private static readonly int[] _storedView = { 4, 5, 6, 7, 0 };
        private int _view = 4;
        private readonly Button[] _viewButtons = new Button[8];

        public EcMobileAnimationControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;

            // runtime only: the compass buttons by view and their tool tips
            Button[] buttons = { buttonViewUp, buttonViewUpRight, buttonViewRight, buttonViewDownRight, buttonViewDown, buttonViewDownLeft, buttonViewLeft, buttonViewUpLeft };
            for (int view = 0; view < buttons.Length; view++)
            {
                _viewButtons[view] = buttons[view];
                toolTip.SetToolTip(buttons[view], $"View {_viewArrows[view]} – stored direction {_viewStored[view]}{(_viewMirrored[view] ? " mirrored" : string.Empty)}");
            }
            Disposed += OnControlDisposed;
        }

        public void SetClient(EcClient client)
        {
            StopPlay();
            _client = client;
            _loaded = false;
            _bodies = new List<(int, List<ActionEntry>)>();
            _bodyView = _bodies;
            listViewBodies.VirtualListSize = 0;
            listViewActions.Items.Clear();
            listViewFrames.Items.Clear();
            previewPanel.Image = null;
            _animation = null;
            _current = null;
            toolStripButtonSave.Enabled = false;
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
                var map = new SortedDictionary<int, List<ActionEntry>>();
                foreach (string file in _files)
                {
                    toolStripStatusLabel.Text = $"Reading {file}...";
                    Application.DoEvents();

                    EcUopPackage package = _client.GetPackage(file);
                    if (package == null)
                    {
                        continue;
                    }
                    package.Changed -= PackageChanged;
                    package.Changed += PackageChanged;

                    foreach (EcUopEntry entry in package.Entries)
                    {
                        Match m = entry.Name == null ? null : _nameRegex.Match(entry.Name);
                        if (m == null || !m.Success)
                        {
                            continue;
                        }
                        int body = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                        // paperdoll ids get an offset so they don't mix with the bodies
                        if (string.Equals(m.Groups[1].Value, "paperdoll", StringComparison.OrdinalIgnoreCase))
                        {
                            body += 1_000_000;
                        }
                        if (!map.TryGetValue(body, out List<ActionEntry> list))
                        {
                            map[body] = list = new List<ActionEntry>();
                        }
                        list.Add(new ActionEntry { File = file, Package = package, Entry = entry, Body = body, Action = int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture) });
                    }
                }

                _bodies = map.Select(kv => (kv.Key, kv.Value.OrderBy(a => a.Action).ToList())).ToList();
                ApplyFilter();
                toolStripStatusLabel.Text = $"{_bodies.Count} bodies with {_bodies.Sum(b => b.Actions.Count)} actions (paperdoll entries are shown as 1000000 + id).";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Loading the animations failed.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void PackageChanged(object sender, EventArgs e)
        {
            toolStripButtonSave.Enabled = _files.Select(f => _client?.IsOpen(f) == true ? _client.GetPackage(f) : null).Any(p => p != null && p.IsDirty);
        }

        private void ApplyFilter()
        {
            string text = toolStripTextBoxSearch.Text.Trim();
            _bodyView = _bodies;
            listViewBodies.VirtualListSize = _bodyView.Count;
            listViewBodies.Invalidate();

            if (text.Length > 0 && EcDialogs.TryParseNumber(text, out long id))
            {
                int index = _bodyView.FindIndex(b => b.Body >= id);
                if (index >= 0)
                {
                    listViewBodies.SelectedIndices.Clear();
                    listViewBodies.SelectedIndices.Add(index);
                    listViewBodies.EnsureVisible(index);
                }
            }
        }

        private void RetrieveBody(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _bodyView.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }
            var (body, actions) = _bodyView[e.ItemIndex];
            var item = new ListViewItem(body.ToString());
            item.SubItems.Add($"0x{body:X}");
            item.SubItems.Add(actions.Count.ToString());
            item.SubItems.Add(string.Join(", ", actions.Select(a => a.File.Replace(".uop", string.Empty)).Distinct()));
            e.Item = item;
        }

        private void ShowBody()
        {
            StopPlay();
            listViewActions.BeginUpdate();
            listViewActions.Items.Clear();
            if (listViewBodies.SelectedIndices.Count == 1 && listViewBodies.SelectedIndices[0] < _bodyView.Count)
            {
                foreach (ActionEntry action in _bodyView[listViewBodies.SelectedIndices[0]].Actions)
                {
                    var item = new ListViewItem(action.Action.ToString()) { Tag = action };
                    item.SubItems.Add(EcImageHelper.FormatSize(action.Entry.DecompressedSize) + (action.Entry.IsModified ? " *" : string.Empty));
                    listViewActions.Items.Add(item);
                }
            }
            listViewActions.EndUpdate();
            if (listViewActions.Items.Count > 0)
            {
                listViewActions.Items[0].Selected = true;
            }
        }

        private void ShowAction()
        {
            StopPlay();
            listViewFrames.Items.Clear();
            previewPanel.Image = null;
            _animation = null;
            _current = listViewActions.SelectedItems.Count == 1 ? listViewActions.SelectedItems[0].Tag as ActionEntry : null;
            if (_current == null)
            {
                return;
            }

            try
            {
                _animation = EcAnimation.Read(_current.Package.ReadData(_current.Entry));
                _updating = true;
                numericUpDownPerDirection.Value = Math.Min(numericUpDownPerDirection.Maximum, _animation.SuggestedFramesPerDirection());
                numericUpDownDirection.Value = _viewStored[_view];
                _updating = false;
                UpdateViewButtons();
                FillDirection();
                toolStripStatusLabel.Text = $"{_current.File}: {_current.Entry.Name}  –  {_animation.Frames.Count} frames, {_animation.Colors.Count} colors, bounds ({_animation.X1},{_animation.Y1})-({_animation.X2},{_animation.Y2})";
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = $"{_current.Entry.DisplayName}: {ex.Message}";
            }
        }

        private int DirectionOf(EcAnimationFrame frame) => Math.Min(4, (frame.Number - 1) / Math.Max(1, (int)numericUpDownPerDirection.Value));

        private void FillDirection()
        {
            StopPlay();
            listViewFrames.BeginUpdate();
            listViewFrames.Items.Clear();
            if (_animation != null)
            {
                int direction = (int)numericUpDownDirection.Value;
                _directionFrames = _animation.Frames.Where(f => DirectionOf(f) == direction).OrderBy(f => f.Number).ToList();
                foreach (EcAnimationFrame frame in _animation.Frames.OrderBy(f => f.Number))
                {
                    var item = new ListViewItem(frame.Number.ToString()) { Tag = frame };
                    item.SubItems.Add(DirectionOf(frame).ToString());
                    item.SubItems.Add(frame.IsEmpty ? "empty" : $"{frame.Width} x {frame.Height}");
                    item.SubItems.Add($"{frame.X1}, {frame.Y1}");
                    if (DirectionOf(frame) != direction)
                    {
                        item.ForeColor = Color.Gray;
                    }
                    listViewFrames.Items.Add(item);
                }
            }
            listViewFrames.EndUpdate();

            if (_directionFrames.Count > 0)
            {
                ShowFrame(_directionFrames[0]);
                if (_directionFrames.Count > 1)
                {
                    StartPlay();
                }
            }
        }

        private EcAnimationFrame CurrentFrame =>
            _directionFrames.Count == 0 ? null : _directionFrames[Math.Min(_playIndex, _directionFrames.Count - 1)];

        private bool Mirrored => _viewMirrored[_view];

        /// <summary>Shows the view direction (0 = ↑ ... 7 = ↖, clockwise).</summary>
        private void SetView(int view)
        {
            _view = ((view % 8) + 8) % 8;
            UpdateViewButtons();
            bool playing = timerPlay.Enabled;
            _updating = true;
            numericUpDownDirection.Value = _viewStored[_view];
            _updating = false;
            int index = _playIndex;
            FillDirection();
            if (!playing && _directionFrames.Count > 0)
            {
                // keep the frame number when turning a stopped animation
                StopPlay();
                _playIndex = Math.Min(index, _directionFrames.Count - 1);
                ShowFrame(_directionFrames[_playIndex]);
            }
        }

        private void UpdateViewButtons()
        {
            for (int i = 0; i < _viewButtons.Length; i++)
            {
                if (_viewButtons[i] != null)
                {
                    _viewButtons[i].BackColor = i == _view ? SystemColors.Highlight : SystemColors.Control;
                    _viewButtons[i].ForeColor = i == _view ? SystemColors.HighlightText : SystemColors.ControlText;
                    _viewButtons[i].UseVisualStyleBackColor = i != _view;
                }
            }
        }

        /// <summary>
        /// Draws the frame on a canvas around the feet of the mobile that is the same for every frame and direction,
        /// so frames don't jump while playing or turning. Mirrored views are drawn flipped.
        /// </summary>
        private void ShowFrame(EcAnimationFrame frame)
        {
            if (_animation == null || frame == null)
            {
                return;
            }

            // symmetric around x = 0 (the feet), so mirrored frames fit too
            int half = Math.Max(22, Math.Max(Math.Abs((int)_animation.X1), Math.Abs((int)_animation.X2))) + 4;
            int top = Math.Min(-4, (int)_animation.Y1) - 4;
            int bottom = Math.Max(16, (int)_animation.Y2) + 4;
            var canvas = new Bitmap(half * 2, bottom - top, PixelFormat.Format32bppArgb);
            bool mirror = Mirrored;
            using (Graphics g = Graphics.FromImage(canvas))
            {
                if (checkBoxGround.Checked)
                {
                    // the tile the mobile stands on (classic size 44 x 44 diamond)
                    var diamond = new[] { new Point(half, -top - 22), new Point(half + 22, -top), new Point(half, -top + 22), new Point(half - 22, -top) };
                    using (var brush = new SolidBrush(Color.FromArgb(40, Color.SteelBlue)))
                    using (var pen = new Pen(Color.FromArgb(110, Color.SteelBlue)))
                    {
                        g.FillPolygon(brush, diamond);
                        g.DrawPolygon(pen, diamond);
                    }
                    // arrow of the view direction
                    double angle = (_view * 45 - 90) * Math.PI / 180.0;
                    var tipPoint = new PointF(half + (float)(Math.Cos(angle) * 20), -top + (float)(Math.Sin(angle) * 10));
                    using (var pen = new Pen(Color.FromArgb(160, Color.OrangeRed), 2f) { EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor })
                    {
                        g.DrawLine(pen, half, -top, tipPoint.X, tipPoint.Y);
                    }
                }

                if (!frame.IsEmpty)
                {
                    using (Bitmap image = EcImageHelper.ToBitmap(_animation.ToBgra(frame), frame.Width, frame.Height))
                    {
                        int x = mirror ? -frame.X2 : frame.X1;
                        if (mirror)
                        {
                            image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                        g.DrawImageUnscaled(image, x + half, frame.Y1 - top);
                    }
                }
            }
            previewPanel.Image = canvas;
            labelInfo.Text = $"View {_viewArrows[_view]}: stored direction {_viewStored[_view]}{(mirror ? " (mirrored, not stored)" : string.Empty)}  –  " +
                         $"frame {frame.Number}  {frame.Width} x {frame.Height}  at {frame.X1}, {frame.Y1}\r\n" +
                         "x/y are relative to the feet of the mobile. Turn with the arrows, ⟳ or the keys Left / Right.";
        }

        private void StartPlay()
        {
            if (_directionFrames.Count == 0)
            {
                return;
            }
            _playIndex = 0;
            timerPlay.Interval = (int)numericUpDownSpeed.Value;
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
            if (_directionFrames.Count == 0)
            {
                StopPlay();
                return;
            }
            _playIndex = (_playIndex + 1) % _directionFrames.Count;
            if (_playIndex == 0 && checkBoxTurn.Checked)
            {
                SetView(_view + 1);
                return;
            }
            ShowFrame(_directionFrames[_playIndex]);
        }

        // ------------------------------------------------------------------ export / import

        private EcAnimationFrame SelectedFrame => listViewFrames.SelectedItems.Count == 1 ? listViewFrames.SelectedItems[0].Tag as EcAnimationFrame : null;

        private string FrameFileName(EcAnimationFrame frame) =>
            $"{_current.Body:D6}_{_current.Action:D2}_{frame.Number:D3}_x{frame.X1}_y{frame.Y1}.png";

        private void ExportPng()
        {
            if (_animation == null)
            {
                return;
            }

            using (var dialog = new FolderBrowserDialog { Description = "Folder for the PNG frames (name: body_action_number_x.._y...png)" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                int count = 0;
                foreach (EcAnimationFrame frame in _animation.Frames.Where(f => !f.IsEmpty))
                {
                    using (Bitmap bitmap = EcImageHelper.ToBitmap(_animation.ToBgra(frame), frame.Width, frame.Height))
                    {
                        bitmap.Save(Path.Combine(dialog.SelectedPath, FrameFileName(frame)), ImageFormat.Png);
                        count++;
                    }
                }
                toolStripStatusLabel.Text = $"{count} frames exported to {dialog.SelectedPath}.";
            }
        }

        private void ExportSelectedFramePng()
        {
            EcAnimationFrame frame = SelectedFrame;
            if (_animation == null || frame == null || frame.IsEmpty)
            {
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = "PNG|*.png", FileName = FrameFileName(frame) })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (Bitmap bitmap = EcImageHelper.ToBitmap(_animation.ToBgra(frame), frame.Width, frame.Height))
                    {
                        bitmap.Save(dialog.FileName, ImageFormat.Png);
                    }
                }
            }
        }

        private void ExportBin()
        {
            if (_current == null)
            {
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = "AMOU animation (*.bin)|*.bin", FileName = $"{_current.Body:D6}_{_current.Action:D2}.bin" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllBytes(dialog.FileName, _current.Package.ReadData(_current.Entry));
                }
            }
        }

        private void StoreAnimation()
        {
            _current.Package.SetData(_current.Entry, _animation.Write());
            toolStripButtonSave.Enabled = true;
        }

        private static readonly Regex _pngRegex = new Regex(@"_(\d{3})_x(-?\d+)_y(-?\d+)\.png$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private void ReplaceFrame(EcAnimationFrame frame, string file)
        {
            using (Bitmap bitmap = EcImageHelper.LoadImageFile(file))
            {
                byte[] bgra = EcImageHelper.ToBgra(bitmap, out int w, out int h);
                Match m = _pngRegex.Match(Path.GetFileName(file));
                short? x = m.Success ? short.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture) : (short?)null;
                short? y = m.Success ? short.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture) : (short?)null;
                _animation.SetFrameImage(frame, bgra, w, h, x, y);
            }
        }

        private void ImportFramePng()
        {
            EcAnimationFrame frame = SelectedFrame;
            if (_animation == null || frame == null)
            {
                return;
            }

            using (var dialog = new OpenFileDialog { Filter = "Images (*.png;*.bmp;*.gif;*.tga;*.dds)|*.png;*.bmp;*.gif;*.tga;*.dds|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    ReplaceFrame(frame, dialog.FileName);
                    StoreAnimation();
                    FillDirection();
                    toolStripStatusLabel.Text = $"Frame {frame.Number} replaced (colors mapped to the palette of the action). Save to keep it.";
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "Import failed.", ex);
                }
            }
        }

        private void ImportPngFolder()
        {
            if (_animation == null)
            {
                return;
            }

            using (var dialog = new FolderBrowserDialog { Description = "Folder with PNG frames named like the export (body_action_number_x.._y...png)" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string prefix = $"{_current.Body:D6}_{_current.Action:D2}_";
                var files = Directory.GetFiles(dialog.SelectedPath, prefix + "*.png");
                if (files.Length == 0)
                {
                    EcDialogs.ShowError(this, $"No files {prefix}*.png found.");
                    return;
                }

                bool newPalette = EcDialogs.Confirm(this, $"{files.Length} frames found.\r\n\r\nBuild a new color palette from the images? " +
                                                          "(Yes: best colors for new artwork, No: keep the current palette)");
                try
                {
                    var images = new List<(EcAnimationFrame Frame, string File)>();
                    foreach (string file in files)
                    {
                        Match m = _pngRegex.Match(Path.GetFileName(file));
                        if (!m.Success)
                        {
                            continue;
                        }
                        ushort number = ushort.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                        EcAnimationFrame frame = _animation.Frames.FirstOrDefault(f => f.Number == number);
                        if (frame == null)
                        {
                            frame = new EcAnimationFrame { ActionId = _animation.ActionId, Number = number };
                            _animation.Frames.Add(frame);
                        }
                        images.Add((frame, file));
                    }

                    if (newPalette)
                    {
                        var pixels = new List<byte[]>();
                        foreach (var (_, file) in images)
                        {
                            using (Bitmap b = EcImageHelper.LoadImageFile(file))
                            {
                                pixels.Add(EcImageHelper.ToBgra(b, out _, out _));
                            }
                        }
                        _animation.BuildPalette(pixels);
                    }

                    foreach (var (frame, file) in images)
                    {
                        ReplaceFrame(frame, file);
                    }

                    // frames are stored in number order
                    List<EcAnimationFrame> ordered = _animation.Frames.OrderBy(f => f.Number).ToList();
                    _animation.Frames.Clear();
                    _animation.Frames.AddRange(ordered);

                    StoreAnimation();
                    FillDirection();
                    toolStripStatusLabel.Text = $"{images.Count} frames imported. Save to keep them.";
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "Import failed.", ex);
                }
            }
        }

        private void ImportBin()
        {
            if (_current == null)
            {
                return;
            }

            using (var dialog = new OpenFileDialog { Filter = "AMOU animation (*.bin)|*.bin|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    byte[] data = File.ReadAllBytes(dialog.FileName);
                    EcAnimation.Read(data); // validates
                    _current.Package.SetData(_current.Entry, data);
                    toolStripButtonSave.Enabled = true;
                    ShowAction();
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "The file is no valid AMOU animation.", ex);
                }
            }
        }

        private void AddBin()
        {
            if (_client == null)
            {
                return;
            }

            using (var dialog = new OpenFileDialog { Filter = "AMOU animation (*.bin)|*.bin|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string[] values = EcDialogs.Prompt(this, "Add animation", new[] { "Body id:", "Action:", "File (AnimationFrame1-6.uop):" },
                    new[] { _current?.Body.ToString() ?? "0", "0", _current?.File ?? "AnimationFrame1.uop" });
                if (values == null || !int.TryParse(values[0], out int body) || !int.TryParse(values[1], out int action))
                {
                    return;
                }

                try
                {
                    byte[] data = File.ReadAllBytes(dialog.FileName);
                    EcAnimation animation = EcAnimation.Read(data);
                    animation.BodyId = (uint)body;
                    foreach (EcAnimationFrame f in animation.Frames)
                    {
                        f.ActionId = (ushort)action;
                    }

                    EcUopPackage package = _client.GetPackage(values[2].Trim());
                    if (package == null)
                    {
                        EcDialogs.ShowError(this, $"{values[2]} was not found.");
                        return;
                    }

                    string name = string.Format(CultureInfo.InvariantCulture, EcClient.AnimationFramePattern, body, action);
                    if (package.Contains(name) && !EcDialogs.Confirm(this, $"{name} already exists in {values[2]}. Replace it?"))
                    {
                        return;
                    }

                    package.AddOrReplace(name, animation.Write(), EcUopCompression.Zlib);
                    toolStripButtonSave.Enabled = true;
                    _loaded = false;
                    OnActivated();
                    toolStripTextBoxSearch.Text = body.ToString();
                    ApplyFilter();
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "Adding the animation failed.", ex);
                }
            }
        }

        private void Save(bool saveAs)
        {
            if (_client == null)
            {
                return;
            }

            List<EcUopPackage> dirty = _files.Where(f => _client.IsOpen(f)).Select(f => _client.GetPackage(f)).Where(p => p != null && p.IsDirty).ToList();
            if (dirty.Count == 0)
            {
                return;
            }

            string folder = null;
            if (saveAs)
            {
                using (var dialog = new FolderBrowserDialog { Description = "Folder for the changed animation files" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    folder = dialog.SelectedPath;
                }
            }
            else if (!EcDialogs.Confirm(this, $"Save {string.Join(", ", dirty.Select(p => Path.GetFileName(p.FilePath)))}?\r\nThe old files are kept as .bak.\r\n\r\nThe Enhanced Client must be closed."))
            {
                return;
            }

            foreach (EcUopPackage package in dirty)
            {
                if (!EcDialogs.ConfirmSignedSave(this, package))
                {
                    return;
                }
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                foreach (EcUopPackage package in dirty)
                {
                    package.Save(folder == null ? package.FilePath : Path.Combine(folder, Path.GetFileName(package.FilePath)));
                }
                toolStripButtonSave.Enabled = false;
                toolStripStatusLabel.Text = "Saved.";
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

        private void OnClickSearch(object sender, EventArgs e) => ApplyFilter();

        private void OnClickExportPng(object sender, EventArgs e) => ExportPng();

        private void OnClickExportFramePng(object sender, EventArgs e) => ExportSelectedFramePng();

        private void OnClickExportBin(object sender, EventArgs e) => ExportBin();

        private void OnClickImportFramePng(object sender, EventArgs e) => ImportFramePng();

        private void OnClickImportPngFolder(object sender, EventArgs e) => ImportPngFolder();

        private void OnClickImportBin(object sender, EventArgs e) => ImportBin();

        private void OnClickAddBin(object sender, EventArgs e) => AddBin();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnBodySelectedIndexChanged(object sender, EventArgs e) => ShowBody();

        private void OnActionSelectedIndexChanged(object sender, EventArgs e) => ShowAction();

        private void OnFrameSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!timerPlay.Enabled && listViewFrames.SelectedItems.Count == 1)
            {
                ShowFrame((EcAnimationFrame)listViewFrames.SelectedItems[0].Tag);
            }
        }

        private void OnDirectionChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                _view = _storedView[(int)numericUpDownDirection.Value];
                UpdateViewButtons();
                FillDirection();
            }
        }

        private void OnPerDirectionChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                FillDirection();
            }
        }

        private void OnSpeedChanged(object sender, EventArgs e) => timerPlay.Interval = (int)numericUpDownSpeed.Value;

        private void OnClickPlay(object sender, EventArgs e) => TogglePlay();

        private void OnClickView(object sender, EventArgs e)
        {
            if (sender is Control c && int.TryParse(c.Tag as string, out int view))
            {
                SetView(view);
            }
        }

        private void OnTurnMouseUp(object sender, MouseEventArgs e) => SetView(_view + (e.Button == MouseButtons.Right ? 7 : 1));

        private void OnGroundChanged(object sender, EventArgs e) => ShowFrame(CurrentFrame);

        private void OnPreviewMouseDown(object sender, MouseEventArgs e) => previewPanel.Focus();

        private void OnPreviewPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                e.IsInputKey = true;
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                SetView(_view + 1);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                SetView(_view + 7);
                e.Handled = true;
            }
        }

        private void OnTimerTick(object sender, EventArgs e) => NextFrame();

    }
}
