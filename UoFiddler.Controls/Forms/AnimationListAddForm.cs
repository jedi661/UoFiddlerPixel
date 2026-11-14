/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Adds an animation (body id) with name and type to Animationlist.xml.
 * Can show a list of animations that were edited in the Animation editor
 * and are not in the list yet.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>An animation edited in the Animation editor</summary>
    public sealed class AnimationCandidate
    {
        /// <summary>real body id, -1 = unknown (anim2..anim5 index without Bodyconv.def entry)</summary>
        public int Body { get; set; } = -1;

        /// <summary>1..5 = anim.mul..anim5.mul, 6 = AnimationFrame*.uop</summary>
        public int FileType { get; set; }

        /// <summary>index inside the file</summary>
        public int Index { get; set; }

        public bool Done { get; set; }

        public string FileLabel => FileType == 6 ? "uop" : FileType == 1 ? "anim.mul" : $"anim{FileType}.mul";

        public override string ToString()
        {
            string body = Body >= 0 ? $"Body {Body}" : "Body ?";
            return $"{(Done ? "✓ " : string.Empty)}{body}  –  {FileLabel} #{Index}";
        }
    }

    public sealed class AnimationListAddForm : Form
    {
        private readonly string[][] _actionNames;
        private readonly List<AnimationCandidate> _candidates;

        private ListBox _listCandidates;
        private NumericUpDown _numBody;
        private Label _lblInfo;
        private TextBox _txtName;
        private ComboBox _cmbType;
        private ComboBox _cmbAction;
        private TrackBar _trackDirection;
        private PictureBox _preview;
        private Label _lblStatus;
        private readonly Timer _timer = new Timer { Interval = 150 };

        private AnimationFrame[] _frames;
        private int _frameIndex;
        private bool _filling;
        private AnimationCandidate _currentCandidate;

        /// <param name="actionNames">action names per type (AnimationListControl.GetActionNames)</param>
        /// <param name="candidates">edited animations to work through (or null)</param>
        /// <param name="entry">entry to show/edit (or null)</param>
        public AnimationListAddForm(string[][] actionNames, IEnumerable<AnimationCandidate> candidates, AnimationListEntry entry)
        {
            _actionNames = actionNames;
            _candidates = candidates?.ToList() ?? new List<AnimationCandidate>();

            BuildUi();

            _timer.Tick += (s, e) => NextFrame();

            if (_candidates.Count > 0)
            {
                _listCandidates.SelectedIndex = 0;
            }
            else if (entry != null)
            {
                ShowEntry(entry);
            }
            else
            {
                OnBodyChanged();
            }
        }

        #region UI

        private void BuildUi()
        {
            Text = "Add animation to the list";
            Icon = Options.GetFiddlerIcon();
            Size = new Size(_candidates.Count > 0 ? 820 : 560, 560);
            MinimumSize = new Size(520, 480);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                Padding = new Padding(8)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _numBody = new NumericUpDown { Minimum = 0, Maximum = 65535, Width = 90 };
            _numBody.ValueChanged += (s, e) => { if (!_filling) { OnBodyChanged(); } };
            var btnFree = new Button { Text = "Free id", AutoSize = true };
            btnFree.Click += (s, e) => _numBody.Value = FindFreeBody((int)_numBody.Value);
            new ToolTip().SetToolTip(btnFree, "Next body id that is not used by a client animation, Bodyconv.def or the list");
            AddRow(grid, "Body id:", _numBody, btnFree);

            _lblInfo = new Label { AutoSize = true, MaximumSize = new Size(480, 0), ForeColor = Color.DimGray };
            grid.Controls.Add(_lblInfo, 1, grid.RowCount);
            grid.SetColumnSpan(_lblInfo, 2);
            grid.RowCount++;

            _txtName = new TextBox { Dock = DockStyle.Fill };
            var btnSuggest = new Button { Text = "Suggest", AutoSize = true };
            btnSuggest.Click += (s, e) => Suggest();
            AddRow(grid, "Name:", _txtName, btnSuggest);

            _cmbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
            _cmbType.Items.AddRange(new object[] { "Monster", "Sea", "Animal", "Human", "Equipment (paperdoll)" });
            _cmbType.SelectedIndex = 0;
            _cmbType.SelectedIndexChanged += (s, e) => { if (!_filling) { FillActions(); } };
            AddRow(grid, "Type:", _cmbType, null);

            _cmbAction = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            _cmbAction.SelectedIndexChanged += (s, e) => LoadPreview();
            _trackDirection = new TrackBar { Minimum = 0, Maximum = 7, Value = 1, Width = 140, TickStyle = TickStyle.None, AutoSize = false, Height = 26 };
            _trackDirection.ValueChanged += (s, e) => LoadPreview();
            var actionPanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
            actionPanel.Controls.Add(_cmbAction);
            actionPanel.Controls.Add(new Label { Text = "Direction:", AutoSize = true, Margin = new Padding(8, 6, 0, 0) });
            actionPanel.Controls.Add(_trackDirection);
            AddRow(grid, "Preview:", actionPanel, null);

            _preview = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 44), BorderStyle = BorderStyle.FixedSingle };
            _preview.Paint += OnPaintPreview;

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
            var btnClose = new Button { Text = "Close", AutoSize = true };
            btnClose.Click += (s, e) => Close();
            var btnServUo = new Button { Text = "ServUO script && setup…", AutoSize = true };
            btnServUo.Click += (s, e) => OpenServUo();
            var btnAdd = new Button { Text = "Add to list", AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
            btnAdd.Click += (s, e) => AddToList();
            buttons.Controls.Add(btnClose);
            buttons.Controls.Add(btnServUo);
            buttons.Controls.Add(btnAdd);
            AcceptButton = btnAdd;

            _lblStatus = new Label { Dock = DockStyle.Bottom, Height = 22, Padding = new Padding(8, 3, 0, 0), ForeColor = Color.DarkGreen };

            var right = new Panel { Dock = DockStyle.Fill };
            var previewHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 8, 4) };
            previewHost.Controls.Add(_preview);
            right.Controls.Add(previewHost);
            right.Controls.Add(grid);

            if (_candidates.Count > 0)
            {
                var left = new Panel { Dock = DockStyle.Left, Width = 260, Padding = new Padding(8, 8, 0, 8) };
                _listCandidates = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false, Font = new Font("Consolas", 9f) };
                _listCandidates.Items.AddRange(_candidates.Cast<object>().ToArray());
                _listCandidates.SelectedIndexChanged += (s, e) => ShowCandidate(_listCandidates.SelectedItem as AnimationCandidate);
                left.Controls.Add(_listCandidates);
                left.Controls.Add(new Label
                {
                    Text = "Edited animations that are not in the list:",
                    Dock = DockStyle.Top,
                    Height = 34
                });
                Controls.Add(right);
                Controls.Add(left);
            }
            else
            {
                Controls.Add(right);
            }

            Controls.Add(_lblStatus);
            Controls.Add(buttons);
        }

        private static void AddRow(TableLayoutPanel grid, string label, Control control, Control extra)
        {
            int row = grid.RowCount;
            grid.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, 0, row);
            grid.Controls.Add(control, 1, row);
            if (extra != null)
            {
                grid.Controls.Add(extra, 2, row);
            }

            grid.RowCount++;
        }

        #endregion

        #region Show entry / candidate

        /// <summary>Shows an existing entry (rename / change type)</summary>
        public void ShowEntry(AnimationListEntry entry)
        {
            _currentCandidate = null;
            _filling = true;
            try
            {
                _numBody.Value = Math.Max(0, Math.Min(65535, entry.Body));
                _txtName.Text = entry.Name;
                _cmbType.SelectedIndex = entry.IsEquipment ? 4 : AnimationListStore.NormalizeType(entry.Type);
            }
            finally
            {
                _filling = false;
            }

            UpdateInfo();
            FillActions();
        }

        private void ShowCandidate(AnimationCandidate candidate)
        {
            if (candidate == null)
            {
                return;
            }

            _currentCandidate = candidate;
            _filling = true;
            try
            {
                _numBody.Value = candidate.Body >= 0 ? candidate.Body : FindFreeBody(1000);
            }
            finally
            {
                _filling = false;
            }

            OnBodyChanged();
        }

        private void OnBodyChanged()
        {
            int body = (int)_numBody.Value;
            AnimationListEntry existing = AnimationListStore.Find(body);
            _filling = true;
            try
            {
                if (existing != null)
                {
                    _txtName.Text = existing.Name;
                    _cmbType.SelectedIndex = existing.IsEquipment ? 4 : existing.Type;
                }
                else
                {
                    Suggest();
                }
            }
            finally
            {
                _filling = false;
            }

            UpdateInfo();
            FillActions();
        }

        /// <summary>Name and type suggested from the animation files</summary>
        private void Suggest()
        {
            int body = (int)_numBody.Value;
            _txtName.Text = AnimationListStore.SuggestName(body);

            if (AnimationListStore.IsInClientFiles(body))
            {
                bool wasFilling = _filling;
                _filling = true;
                _cmbType.SelectedIndex = AnimationListStore.SuggestType(body, _actionNames);
                _filling = wasFilling;
            }

            if (!_filling)
            {
                FillActions();
            }
        }

        private void UpdateInfo()
        {
            int body = (int)_numBody.Value;
            var parts = new List<string>();
            AnimationListEntry existing = AnimationListStore.Find(body);
            parts.Add(existing != null ? $"In the list as \"{existing.Name}\" ({(existing.IsEquipment ? "Equipment" : "Mob")})." : "Not in the list yet.");

            if (AnimationListStore.IsInClientFiles(body))
            {
                string file = string.Empty;
                try
                {
                    file = Animations.GetFileName(body);
                }
                catch (Exception)
                {
                    // ignored
                }

                parts.Add($"Found in the client files ({file}).");
            }
            else
            {
                parts.Add("Not found in the loaded client files – the preview stays empty until the saved animation files are in the client folder.");
            }

            if (_currentCandidate != null && _currentCandidate.Body < 0)
            {
                parts.Add($"{_currentCandidate.FileLabel} index {_currentCandidate.Index} has no body id in Bodyconv.def: choose the body id the server should use " +
                          "(the ServUO dialog creates the Bodyconv.def line).");
            }

            _lblInfo.Text = string.Join(" ", parts);
        }

        /// <summary>Next id from start that no client animation, Bodyconv.def entry or list entry uses</summary>
        private static int FindFreeBody(int start)
        {
            HashSet<int> inList = new HashSet<int>(AnimationListStore.Load().Select(e => e.Body));
            for (int body = Math.Max(1, start); body < 65535; body++)
            {
                if (inList.Contains(body) || BodyConverter.Contains(body) || AnimationListStore.IsInClientFiles(body))
                {
                    continue;
                }

                return body;
            }

            return start;
        }

        #endregion

        #region Preview

        private void FillActions()
        {
            int type = Math.Min(3, Math.Max(0, _cmbType.SelectedIndex));
            int body = (int)_numBody.Value;
            int previous = _cmbAction.SelectedItem is ActionItem a ? a.Action : 0;

            _cmbAction.BeginUpdate();
            _cmbAction.Items.Clear();
            for (int i = 0; i < _actionNames[type].Length; i++)
            {
                bool defined = false;
                try
                {
                    defined = Animations.IsActionDefined(body, i, 0);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (defined)
                {
                    _cmbAction.Items.Add(new ActionItem(i, _actionNames[type][i]));
                }
            }

            _cmbAction.EndUpdate();

            if (_cmbAction.Items.Count == 0)
            {
                _frames = null;
                _timer.Stop();
                _preview.Invalidate();
                return;
            }

            ActionItem keep = _cmbAction.Items.Cast<ActionItem>().FirstOrDefault(i => i.Action == previous);
            _cmbAction.SelectedItem = keep ?? _cmbAction.Items[0];
        }

        private void LoadPreview()
        {
            _timer.Stop();
            _frames = null;
            _frameIndex = 0;

            if (_cmbAction.SelectedItem is ActionItem action)
            {
                int hue = 0;
                int dir = (_trackDirection.Value - 3) & 7; // same mapping as the AnimationList tab
                try
                {
                    _frames = Animations.GetAnimation((int)_numBody.Value, action.Action, dir, ref hue, false, false);
                }
                catch (Exception)
                {
                    _frames = null;
                }
            }

            if (_frames != null && _frames.Length > 1)
            {
                _timer.Start();
            }

            _preview.Invalidate();
        }

        private void NextFrame()
        {
            if (_frames == null || _frames.Length == 0)
            {
                _timer.Stop();
                return;
            }

            _frameIndex = (_frameIndex + 1) % _frames.Length;
            _preview.Invalidate();
        }

        private void OnPaintPreview(object sender, PaintEventArgs e)
        {
            if (_frames == null || _frames.Length == 0)
            {
                TextRenderer.DrawText(e.Graphics, "no preview", Font, _preview.ClientRectangle, Color.Gray,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            AnimationFrame frame = _frames[Math.Min(_frameIndex, _frames.Length - 1)];
            if (frame?.Bitmap == null)
            {
                return;
            }

            int cx = _preview.ClientSize.Width / 2;
            int cy = (_preview.ClientSize.Height * 3) / 4;
            e.Graphics.DrawImage(frame.Bitmap, cx - frame.Center.X, cy - frame.Center.Y - frame.Bitmap.Height);
        }

        private sealed class ActionItem
        {
            public ActionItem(int action, string name)
            {
                Action = action;
                Name = name;
            }

            public int Action { get; }
            public string Name { get; }

            public override string ToString() => $"{Action} {Name}";
        }

        #endregion

        #region Actions

        private AnimationListEntry CurrentEntry() => new AnimationListEntry
        {
            Body = (int)_numBody.Value,
            Name = AnimationListStore.CleanName(_txtName.Text),
            Type = Math.Min(3, _cmbType.SelectedIndex),
            IsEquipment = _cmbType.SelectedIndex == 4
        };

        private void AddToList()
        {
            AnimationListEntry entry = CurrentEntry();
            if (entry.Name.Length == 0)
            {
                MessageBox.Show(this, "Please enter a name.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            AnimationListEntry existing = AnimationListStore.Find(entry.Body);
            if (existing != null && existing.IsEquipment != entry.IsEquipment &&
                MessageBox.Show(this, $"Body {entry.Body} is already in the list as {(existing.IsEquipment ? "Equipment" : "Mob")} \"{existing.Name}\".\n" +
                                      "Add it to the other tab as well?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                AnimationListStore.AddOrUpdate(entry);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Animationlist.xml could not be written:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _lblStatus.Text = $"✓ \"{entry.Name}\" ({entry.Body}) saved in {AnimationListStore.FilePath}";
            UpdateInfo();

            if (_currentCandidate != null)
            {
                _currentCandidate.Done = true;
                _currentCandidate.Body = entry.Body;
                int index = _listCandidates.SelectedIndex;
                _listCandidates.Items[index] = _currentCandidate; // refresh text
                int next = _candidates.FindIndex(c => !c.Done);
                if (next >= 0)
                {
                    _listCandidates.SelectedIndex = next;
                }
            }
        }

        private void OpenServUo()
        {
            AnimationListEntry entry = CurrentEntry();
            AnimationCandidate source = _currentCandidate == null ? null : new AnimationCandidate
            {
                Body = entry.Body,
                FileType = _currentCandidate.FileType,
                Index = _currentCandidate.Index
            };

            var form = new ServUOSetupForm(entry.Body, entry.IsEquipment ? 4 : entry.Type, entry.Name, source);
            form.Show(this);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            base.OnFormClosed(e);
        }

        #endregion
    }
}
