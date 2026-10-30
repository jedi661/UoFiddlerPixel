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
    public sealed class EcMobileAnimationControl : UserControl, IEcEditor
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

        private readonly ToolStripTextBox _searchBox;
        private readonly ToolStripButton _saveButton;
        private readonly ListView _bodyList;
        private readonly ListView _actionList;
        private readonly ListView _frameList;
        private readonly NumericUpDown _directionBox;
        private readonly NumericUpDown _perDirectionBox;
        private readonly NumericUpDown _speedBox;
        private readonly Button _playButton;
        private readonly EcPreviewPanel _preview;
        private readonly Label _info;
        private readonly Timer _timer;
        private readonly ToolStripStatusLabel _statusLabel;

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
        private readonly CheckBox _turnBox;
        private readonly CheckBox _groundBox;

        public EcMobileAnimationControl()
        {
            Dock = DockStyle.Fill;

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _searchBox = new ToolStripTextBox { Width = 90, ToolTipText = "Body id – Enter" };
            _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplyFilter(); } };
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false };

            toolStrip.Items.Add(new ToolStripLabel("Body:"));
            toolStrip.Items.Add(_searchBox);
            toolStrip.Items.Add(new ToolStripButton("Search", null, (s, e) => ApplyFilter()));
            toolStrip.Items.Add(new ToolStripSeparator());

            var exportButton = new ToolStripDropDownButton("Export");
            exportButton.DropDownItems.Add("Frames of this action as PNG...", null, (s, e) => ExportPng());
            exportButton.DropDownItems.Add("Selected frame as PNG...", null, (s, e) => ExportSelectedFramePng());
            exportButton.DropDownItems.Add("Action as .bin (AMOU)...", null, (s, e) => ExportBin());
            toolStrip.Items.Add(exportButton);

            var importButton = new ToolStripDropDownButton("Import");
            importButton.DropDownItems.Add("Replace selected frame with PNG...", null, (s, e) => ImportFramePng());
            importButton.DropDownItems.Add("Replace frames from PNG folder (exported names)...", null, (s, e) => ImportPngFolder());
            importButton.DropDownItems.Add(new ToolStripSeparator());
            importButton.DropDownItems.Add("Replace action with .bin (AMOU)...", null, (s, e) => ImportBin());
            importButton.DropDownItems.Add("Add .bin as new body / action...", null, (s, e) => AddBin());
            toolStrip.Items.Add(importButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            _bodyList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false, VirtualMode = true };
            _bodyList.Columns.Add("Body", 60, HorizontalAlignment.Right);
            _bodyList.Columns.Add("Hex", 55);
            _bodyList.Columns.Add("Actions", 55, HorizontalAlignment.Right);
            _bodyList.Columns.Add("File", 120);
            _bodyList.RetrieveVirtualItem += RetrieveBody;
            _bodyList.SelectedIndexChanged += (s, e) => ShowBody();

            _actionList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false };
            _actionList.Columns.Add("Action", 55, HorizontalAlignment.Right);
            _actionList.Columns.Add("Size", 70, HorizontalAlignment.Right);
            _actionList.SelectedIndexChanged += (s, e) => ShowAction();

            _frameList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false };
            _frameList.Columns.Add("#", 40, HorizontalAlignment.Right);
            _frameList.Columns.Add("Dir", 35, HorizontalAlignment.Right);
            _frameList.Columns.Add("Size", 70);
            _frameList.Columns.Add("Position", 90);
            _frameList.SelectedIndexChanged += (s, e) => { if (!_timer.Enabled && _frameList.SelectedItems.Count == 1) { ShowFrame((EcAnimationFrame)_frameList.SelectedItems[0].Tag); } };

            _directionBox = new NumericUpDown { Minimum = 0, Maximum = 4, Width = 45 };
            _directionBox.ValueChanged += (s, e) =>
            {
                if (!_updating)
                {
                    _view = _storedView[(int)_directionBox.Value];
                    UpdateViewButtons();
                    FillDirection();
                }
            };
            _perDirectionBox = new NumericUpDown { Minimum = 1, Maximum = 200, Width = 50 };
            _perDirectionBox.ValueChanged += (s, e) => { if (!_updating) { FillDirection(); } };
            _speedBox = new NumericUpDown { Minimum = 20, Maximum = 2000, Value = 120, Increment = 10, Width = 60 };
            _speedBox.ValueChanged += (s, e) => _timer.Interval = (int)_speedBox.Value;
            _playButton = new Button { Text = "Play", Width = 60 };
            _playButton.Click += (s, e) => TogglePlay();

            // compass: 3 x 3 buttons around the turn buttons
            var compass = new TableLayoutPanel { ColumnCount = 3, RowCount = 3, Width = 96, Height = 96, Margin = new Padding(3, 0, 6, 0) };
            for (int i = 0; i < 3; i++)
            {
                compass.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
                compass.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            }
            int[,] cells = { { 7, 0, 1 }, { 6, -1, 2 }, { 5, 4, 3 } };
            var tip = new ToolTip();
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int view = cells[row, col];
                    if (view < 0)
                    {
                        var turn = new Button { Text = "⟳", Dock = DockStyle.Fill, Margin = new Padding(1), Font = new Font(Font.FontFamily, 11f) };
                        turn.MouseUp += (s, e) => SetView(_view + (e.Button == MouseButtons.Right ? 7 : 1));
                        tip.SetToolTip(turn, "Turn right (right click: turn left). Keys: Left / Right in the preview");
                        compass.Controls.Add(turn, col, row);
                        continue;
                    }
                    var button = new Button { Text = _viewArrows[view], Dock = DockStyle.Fill, Margin = new Padding(1), Font = new Font(Font.FontFamily, 11f) };
                    button.Click += (s, e) => SetView(view);
                    tip.SetToolTip(button, $"View {_viewArrows[view]} – stored direction {_viewStored[view]}{(_viewMirrored[view] ? " mirrored" : string.Empty)}");
                    _viewButtons[view] = button;
                    compass.Controls.Add(button, col, row);
                }
            }

            _turnBox = new CheckBox { Text = "Turn around while playing", AutoSize = true, Margin = new Padding(3, 4, 0, 0) };
            _groundBox = new CheckBox { Text = "Ground tile", AutoSize = true, Checked = true, Margin = new Padding(3, 4, 0, 0) };
            _groundBox.CheckedChanged += (s, e) => ShowFrame(CurrentFrame);

            var controlsPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 104, Padding = new Padding(2), FlowDirection = FlowDirection.LeftToRight };
            var options = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            var line = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            options.Controls.Add(line);
            options.Controls.Add(_turnBox);
            options.Controls.Add(_groundBox);
            controlsPanel.Controls.Add(compass);
            controlsPanel.Controls.Add(options);
            FlowLayoutPanel controls = line;
            controls.Controls.Add(new Label { Text = "Stored dir:", AutoSize = true, Margin = new Padding(3, 8, 0, 0) });
            controls.Controls.Add(_directionBox);
            controls.Controls.Add(new Label { Text = "Frames/dir:", AutoSize = true, Margin = new Padding(6, 8, 0, 0) });
            controls.Controls.Add(_perDirectionBox);
            controls.Controls.Add(new Label { Text = "ms:", AutoSize = true, Margin = new Padding(6, 8, 0, 0) });
            controls.Controls.Add(_speedBox);
            controls.Controls.Add(_playButton);

            _preview = new EcPreviewPanel { Dock = DockStyle.Fill, Zoom = 2f, TabStop = true };
            _preview.MouseDown += (s, e) => _preview.Focus();
            _preview.PreviewKeyDown += (s, e) => { if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right) { e.IsInputKey = true; } };
            _preview.KeyDown += (s, e) =>
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
            };
            _info = new Label { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4), Font = EcDialogs.MonoFont(8.5f) };

            var previewPanel = new Panel { Dock = DockStyle.Fill };
            previewPanel.Controls.Add(_preview);
            previewPanel.Controls.Add(_info);
            previewPanel.Controls.Add(controlsPanel);

            var frameSplit = new SplitContainer { Dock = DockStyle.Fill };
            frameSplit.Panel1.Controls.Add(_frameList);
            frameSplit.Panel2.Controls.Add(previewPanel);
            frameSplit.HandleCreated += (s, e) => { try { frameSplit.SplitterDistance = 250; } catch (InvalidOperationException) { } };

            var listSplit = new SplitContainer { Dock = DockStyle.Fill };
            listSplit.Panel1.Controls.Add(_bodyList);
            listSplit.Panel2.Controls.Add(_actionList);
            listSplit.HandleCreated += (s, e) => { try { listSplit.SplitterDistance = 300; } catch (InvalidOperationException) { } };

            var mainSplit = new SplitContainer { Dock = DockStyle.Fill };
            mainSplit.Panel1.Controls.Add(listSplit);
            mainSplit.Panel2.Controls.Add(frameSplit);
            mainSplit.HandleCreated += (s, e) => { try { mainSplit.SplitterDistance = 440; } catch (InvalidOperationException) { } };

            var status = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Choose the Enhanced Client folder on top.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            status.Items.Add(_statusLabel);

            _timer = new Timer { Interval = 120 };
            _timer.Tick += (s, e) => NextFrame();

            Controls.Add(mainSplit);
            Controls.Add(toolStrip);
            Controls.Add(status);
        }

        public void SetClient(EcClient client)
        {
            StopPlay();
            _client = client;
            _loaded = false;
            _bodies = new List<(int, List<ActionEntry>)>();
            _bodyView = _bodies;
            _bodyList.VirtualListSize = 0;
            _actionList.Items.Clear();
            _frameList.Items.Clear();
            _preview.Image = null;
            _animation = null;
            _current = null;
            _saveButton.Enabled = false;
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
                    _statusLabel.Text = $"Reading {file}...";
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
                _statusLabel.Text = $"{_bodies.Count} bodies with {_bodies.Sum(b => b.Actions.Count)} actions (paperdoll entries are shown as 1000000 + id).";
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
            _saveButton.Enabled = _files.Select(f => _client?.IsOpen(f) == true ? _client.GetPackage(f) : null).Any(p => p != null && p.IsDirty);
        }

        private void ApplyFilter()
        {
            string text = _searchBox.Text.Trim();
            _bodyView = _bodies;
            _bodyList.VirtualListSize = _bodyView.Count;
            _bodyList.Invalidate();

            if (text.Length > 0 && EcDialogs.TryParseNumber(text, out long id))
            {
                int index = _bodyView.FindIndex(b => b.Body >= id);
                if (index >= 0)
                {
                    _bodyList.SelectedIndices.Clear();
                    _bodyList.SelectedIndices.Add(index);
                    _bodyList.EnsureVisible(index);
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
            _actionList.BeginUpdate();
            _actionList.Items.Clear();
            if (_bodyList.SelectedIndices.Count == 1 && _bodyList.SelectedIndices[0] < _bodyView.Count)
            {
                foreach (ActionEntry action in _bodyView[_bodyList.SelectedIndices[0]].Actions)
                {
                    var item = new ListViewItem(action.Action.ToString()) { Tag = action };
                    item.SubItems.Add(EcImageHelper.FormatSize(action.Entry.DecompressedSize) + (action.Entry.IsModified ? " *" : string.Empty));
                    _actionList.Items.Add(item);
                }
            }
            _actionList.EndUpdate();
            if (_actionList.Items.Count > 0)
            {
                _actionList.Items[0].Selected = true;
            }
        }

        private void ShowAction()
        {
            StopPlay();
            _frameList.Items.Clear();
            _preview.Image = null;
            _animation = null;
            _current = _actionList.SelectedItems.Count == 1 ? _actionList.SelectedItems[0].Tag as ActionEntry : null;
            if (_current == null)
            {
                return;
            }

            try
            {
                _animation = EcAnimation.Read(_current.Package.ReadData(_current.Entry));
                _updating = true;
                _perDirectionBox.Value = Math.Min(_perDirectionBox.Maximum, _animation.SuggestedFramesPerDirection());
                _directionBox.Value = _viewStored[_view];
                _updating = false;
                UpdateViewButtons();
                FillDirection();
                _statusLabel.Text = $"{_current.File}: {_current.Entry.Name}  –  {_animation.Frames.Count} frames, {_animation.Colors.Count} colors, bounds ({_animation.X1},{_animation.Y1})-({_animation.X2},{_animation.Y2})";
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"{_current.Entry.DisplayName}: {ex.Message}";
            }
        }

        private int DirectionOf(EcAnimationFrame frame) => Math.Min(4, (frame.Number - 1) / Math.Max(1, (int)_perDirectionBox.Value));

        private void FillDirection()
        {
            StopPlay();
            _frameList.BeginUpdate();
            _frameList.Items.Clear();
            if (_animation != null)
            {
                int direction = (int)_directionBox.Value;
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
                    _frameList.Items.Add(item);
                }
            }
            _frameList.EndUpdate();

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
            bool playing = _timer.Enabled;
            _updating = true;
            _directionBox.Value = _viewStored[_view];
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
                if (_groundBox.Checked)
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
            _preview.Image = canvas;
            _info.Text = $"View {_viewArrows[_view]}: stored direction {_viewStored[_view]}{(mirror ? " (mirrored, not stored)" : string.Empty)}  –  " +
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
            _timer.Interval = (int)_speedBox.Value;
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
            if (_directionFrames.Count == 0)
            {
                StopPlay();
                return;
            }
            _playIndex = (_playIndex + 1) % _directionFrames.Count;
            if (_playIndex == 0 && _turnBox.Checked)
            {
                SetView(_view + 1);
                return;
            }
            ShowFrame(_directionFrames[_playIndex]);
        }

        // ------------------------------------------------------------------ export / import

        private EcAnimationFrame SelectedFrame => _frameList.SelectedItems.Count == 1 ? _frameList.SelectedItems[0].Tag as EcAnimationFrame : null;

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
                _statusLabel.Text = $"{count} frames exported to {dialog.SelectedPath}.";
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
            _saveButton.Enabled = true;
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
                    _statusLabel.Text = $"Frame {frame.Number} replaced (colors mapped to the palette of the action). Save to keep it.";
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
                    _statusLabel.Text = $"{images.Count} frames imported. Save to keep them.";
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
                    _saveButton.Enabled = true;
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
                    _saveButton.Enabled = true;
                    _loaded = false;
                    OnActivated();
                    _searchBox.Text = body.ToString();
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
                _saveButton.Enabled = false;
                _statusLabel.Text = "Saved.";
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
            }
            base.Dispose(disposing);
        }
    }
}
