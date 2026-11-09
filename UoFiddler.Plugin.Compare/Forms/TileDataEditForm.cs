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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.Forms
{
    /// <summary>
    /// One tiledata entry that is edited in <see cref="TileDataEditForm"/>.
    /// </summary>
    internal sealed class TileDataEditEntry
    {
        /// <summary>Id in the second file, -1 when the entry has no source.</summary>
        public int SourceId { get; set; } = -1;

        /// <summary>Id in the original file the tiledata is written to.</summary>
        public int TargetId { get; set; }

        public bool HasOriginal { get; set; }
        public ItemData Original { get; set; }

        public bool HasSecond { get; set; }
        public ItemData Second { get; set; }

        /// <summary>The values that will be written.</summary>
        public ItemData Edited { get; set; }

        public Bitmap Image { get; set; }

        public override string ToString()
        {
            string name = string.IsNullOrEmpty(Edited.Name) ? "(no name)" : Edited.Name;
            return SourceId >= 0 && SourceId != TargetId
                ? $"0x{SourceId:X4} -> 0x{TargetId:X4}  {name}"
                : $"0x{TargetId:X4}  {name}";
        }
    }

    /// <summary>
    /// Window to look at and adjust tiledata entries (name, values and flags) before they are written.
    /// </summary>
    internal sealed class TileDataEditForm : Form
    {
        private readonly List<TileDataEditEntry> _entries;
        private readonly List<TileFlag> _flagValues;
        private bool _loading;

        private readonly ListBox _list = new ListBox();
        private readonly PictureBox _picture = new PictureBox();
        private readonly Label _labelOriginal = new Label();
        private readonly Label _labelSecond = new Label();
        private readonly TextBox _textName = new TextBox();
        private readonly Dictionary<string, NumericUpDown> _numbers = new Dictionary<string, NumericUpDown>();
        private readonly CheckedListBox _flags = new CheckedListBox();
        private readonly Button _buttonUseSecond = new Button();
        private readonly Button _buttonUseOriginal = new Button();
        private readonly Button _buttonCopyToSelected = new Button();

        /// <param name="entries">Entries to edit. The <see cref="TileDataEditEntry.Edited"/> values are changed in place.</param>
        /// <param name="title">Window title.</param>
        /// <param name="newFormat">True when the target tiledata has 64 bit flags (High Seas).</param>
        public TileDataEditForm(List<TileDataEditEntry> entries, string title, bool newFormat)
        {
            _entries = entries;
            _flagValues = Enum.GetValues(typeof(TileFlag)).Cast<TileFlag>()
                .Where(f => f != TileFlag.None && (newFormat || (ulong)f <= 0x80000000UL))
                .ToList();

            BuildUi(title);

            _list.Items.AddRange(_entries.Cast<object>().ToArray());
            if (_list.Items.Count > 0)
            {
                _list.SelectedIndex = 0;
            }
        }

        private void BuildUi(string title)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(900, 640);
            MinimumSize = new Size(760, 520);
            ShowInTaskbar = false;
            MinimizeBox = false;

            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                FixedPanel = FixedPanel.Panel1,
                SplitterDistance = 260
            };

            _list.Dock = DockStyle.Fill;
            _list.IntegralHeight = false;
            _list.SelectionMode = SelectionMode.MultiExtended;
            _list.DrawMode = DrawMode.OwnerDrawFixed;
            _list.DrawItem += OnDrawListItem;
            _list.SelectedIndexChanged += (s, e) => ShowEntry(CurrentEntry);
            split.Panel1.Controls.Add(_list);

            TableLayoutPanel right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(4)
            };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _picture.Dock = DockStyle.Fill;
            _picture.BorderStyle = BorderStyle.FixedSingle;
            _picture.SizeMode = PictureBoxSizeMode.Zoom;
            right.Controls.Add(_picture, 0, 0);
            right.SetColumnSpan(_picture, 2);

            _labelOriginal.AutoSize = true;
            _labelOriginal.MaximumSize = new Size(600, 0);
            _labelOriginal.ForeColor = Color.DarkSlateGray;
            right.Controls.Add(_labelOriginal, 0, 1);
            right.SetColumnSpan(_labelOriginal, 2);

            _labelSecond.AutoSize = true;
            _labelSecond.MaximumSize = new Size(600, 0);
            _labelSecond.ForeColor = Color.DarkBlue;
            right.Controls.Add(_labelSecond, 0, 2);
            right.SetColumnSpan(_labelSecond, 2);

            // value fields
            TableLayoutPanel fields = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoScroll = true
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _textName.MaxLength = 20;
            _textName.Dock = DockStyle.Fill;
            _textName.TextChanged += (s, e) => OnValueChanged();
            AddField(fields, "Name", _textName);

            AddNumber(fields, "Weight", 0, 255);
            AddNumber(fields, "Height", 0, 255);
            AddNumber(fields, "Quality", 0, 255);
            AddNumber(fields, "Quantity", 0, 255);
            AddNumber(fields, "Value", 0, 255);
            AddNumber(fields, "Hue", 0, 255);
            AddNumber(fields, "StackingOffset", 0, 255);
            AddNumber(fields, "Animation", short.MinValue, short.MaxValue);
            AddNumber(fields, "MiscData", short.MinValue, short.MaxValue);
            AddNumber(fields, "Unk2", 0, 255);
            AddNumber(fields, "Unk3", 0, 255);
            right.Controls.Add(fields, 0, 3);

            _flags.Dock = DockStyle.Fill;
            _flags.CheckOnClick = true;
            _flags.IntegralHeight = false;
            foreach (TileFlag f in _flagValues)
            {
                _flags.Items.Add(f.ToString());
            }
            _flags.ItemCheck += OnFlagItemCheck;
            right.Controls.Add(_flags, 1, 3);

            FlowLayoutPanel entryButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            _buttonUseSecond.Text = "Use Second";
            _buttonUseSecond.AutoSize = true;
            _buttonUseSecond.Click += (s, e) => ResetSelected(useSecond: true);
            _buttonUseOriginal.Text = "Use Original";
            _buttonUseOriginal.AutoSize = true;
            _buttonUseOriginal.Click += (s, e) => ResetSelected(useSecond: false);
            _buttonCopyToSelected.Text = "Copy values to selected (keep names)";
            _buttonCopyToSelected.AutoSize = true;
            _buttonCopyToSelected.Click += OnCopyToSelected;
            entryButtons.Controls.AddRange(new Control[] { _buttonUseSecond, _buttonUseOriginal, _buttonCopyToSelected });
            right.Controls.Add(entryButtons, 0, 4);
            right.SetColumnSpan(entryButtons, 2);

            split.Panel2.Controls.Add(right);

            FlowLayoutPanel bottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Padding = new Padding(4)
            };
            Button ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
            Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            bottom.Controls.Add(cancel);
            bottom.Controls.Add(ok);
            AcceptButton = ok;
            CancelButton = cancel;

            Controls.Add(split);
            Controls.Add(bottom);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                // commits a value that is still being typed into a NumericUpDown
                ValidateChildren();
                OnValueChanged();
            }
            base.OnFormClosing(e);
        }

        private static void AddField(TableLayoutPanel panel, string caption, Control control)
        {
            int row = panel.RowCount++;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 3, 3) }, 0, row);
            panel.Controls.Add(control, 1, row);
        }

        private void AddNumber(TableLayoutPanel panel, string name, int min, int max)
        {
            NumericUpDown num = new NumericUpDown { Minimum = min, Maximum = max, Width = 90 };
            num.ValueChanged += (s, e) => OnValueChanged();
            _numbers[name] = num;
            AddField(panel, name, num);
        }

        private TileDataEditEntry CurrentEntry
        {
            get
            {
                if (_list.SelectedIndices.Count == 0)
                {
                    return null;
                }

                // the focused (last clicked) entry is shown in the editor
                int index = ListBoxHelper.GetFocusedSelectedIndex(_list);
                return index >= 0 ? (TileDataEditEntry)_list.Items[index] : null;
            }
        }

        private void ShowEntry(TileDataEditEntry entry)
        {
            _loading = true;
            try
            {
                bool enabled = entry != null;
                _textName.Enabled = enabled;
                _flags.Enabled = enabled;
                foreach (NumericUpDown n in _numbers.Values)
                {
                    n.Enabled = enabled;
                }

                if (entry == null)
                {
                    _picture.Image = null;
                    _labelOriginal.Text = string.Empty;
                    _labelSecond.Text = string.Empty;
                    return;
                }

                _picture.Image = entry.Image;
                _labelOriginal.Text = entry.HasOriginal
                    ? $"Original 0x{entry.TargetId:X4}: {ItemCompareHelper.Describe(entry.Original)}"
                    : $"Original 0x{entry.TargetId:X4}: (no tiledata entry)";
                _labelSecond.Text = entry.HasSecond
                    ? $"Second 0x{entry.SourceId:X4}: {ItemCompareHelper.Describe(entry.Second)}"
                    : "Second: (no tiledata entry)";
                _buttonUseSecond.Enabled = entry.HasSecond;
                _buttonUseOriginal.Enabled = entry.HasOriginal;

                ItemData d = entry.Edited;
                _textName.Text = d.Name ?? string.Empty;
                SetNumber("Weight", d.Weight);
                SetNumber("Height", d.Height);
                SetNumber("Quality", d.Quality);
                SetNumber("Quantity", d.Quantity);
                SetNumber("Value", d.Value);
                SetNumber("Hue", d.Hue);
                SetNumber("StackingOffset", d.StackingOffset);
                SetNumber("Animation", d.Animation);
                SetNumber("MiscData", d.MiscData);
                SetNumber("Unk2", d.Unk2);
                SetNumber("Unk3", d.Unk3);

                for (int i = 0; i < _flagValues.Count; ++i)
                {
                    _flags.SetItemChecked(i, (d.Flags & _flagValues[i]) != 0);
                }
            }
            finally
            {
                _loading = false;
            }
        }

        private void SetNumber(string name, decimal value)
        {
            NumericUpDown n = _numbers[name];
            n.Value = Math.Max(n.Minimum, Math.Min(n.Maximum, value));
        }

        private byte GetByte(string name) => (byte)_numbers[name].Value;
        private short GetShort(string name) => (short)_numbers[name].Value;

        private void OnFlagItemCheck(object sender, ItemCheckEventArgs e)
        {
            // ItemCheck fires before the check state changes, so the new value is passed on directly.
            // No BeginInvoke: the form may not have a window handle yet (e.g. while it is filled).
            if (_loading)
            {
                return;
            }

            OnValueChanged(e.Index, e.NewValue == CheckState.Checked);
        }

        private void OnValueChanged()
        {
            OnValueChanged(-1, false);
        }

        /// <param name="changedFlagIndex">Index of a flag whose new state is <paramref name="changedFlagChecked"/>, -1 for none.</param>
        private void OnValueChanged(int changedFlagIndex, bool changedFlagChecked)
        {
            if (_loading)
            {
                return;
            }

            TileDataEditEntry entry = CurrentEntry;
            if (entry == null)
            {
                return;
            }

            ItemData d = entry.Edited;
            d.Name = _textName.Text;
            d.Weight = GetByte("Weight");
            d.Height = GetByte("Height");
            d.Quality = GetByte("Quality");
            d.Quantity = GetByte("Quantity");
            d.Value = GetByte("Value");
            d.Hue = GetByte("Hue");
            d.StackingOffset = GetByte("StackingOffset");
            d.Animation = GetShort("Animation");
            d.MiscData = GetShort("MiscData");
            d.Unk2 = GetByte("Unk2");
            d.Unk3 = GetByte("Unk3");

            // keep flags that are not shown (e.g. 64 bit flags on an old format target)
            TileFlag flags = d.Flags;
            for (int i = 0; i < _flagValues.Count; ++i)
            {
                bool isChecked = i == changedFlagIndex ? changedFlagChecked : _flags.GetItemChecked(i);
                if (isChecked)
                {
                    flags |= _flagValues[i];
                }
                else
                {
                    flags &= ~_flagValues[i];
                }
            }
            d.Flags = flags;

            entry.Edited = d;
            RefreshListText();
        }

        private void RefreshListText()
        {
            _list.Invalidate();
        }

        private void OnDrawListItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0 && e.Index < _list.Items.Count)
            {
                TileDataEditEntry entry = (TileDataEditEntry)_list.Items[e.Index];
                bool changed = entry.HasOriginal && !ItemCompareHelper.ItemDataEquals(entry.Original, entry.Edited);
                Color color = (e.State & DrawItemState.Selected) != 0
                    ? SystemColors.HighlightText
                    : changed ? Color.DarkBlue : SystemColors.ControlText;
                TextRenderer.DrawText(e.Graphics, entry.ToString(), e.Font, e.Bounds, color,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
            e.DrawFocusRectangle();
        }

        private void ResetSelected(bool useSecond)
        {
            foreach (TileDataEditEntry entry in _list.SelectedItems.Cast<TileDataEditEntry>().ToList())
            {
                if (useSecond && entry.HasSecond)
                {
                    entry.Edited = entry.Second;
                }
                else if (!useSecond && entry.HasOriginal)
                {
                    entry.Edited = entry.Original;
                }
            }
            RefreshAll();
        }

        private void OnCopyToSelected(object sender, EventArgs e)
        {
            TileDataEditEntry current = CurrentEntry;
            if (current == null)
            {
                return;
            }

            foreach (TileDataEditEntry entry in _list.SelectedItems.Cast<TileDataEditEntry>().ToList())
            {
                if (ReferenceEquals(entry, current))
                {
                    continue;
                }

                ItemData d = current.Edited;
                d.Name = entry.Edited.Name;
                entry.Edited = d;
            }
            RefreshAll();
        }

        private void RefreshAll()
        {
            _list.Invalidate();
            ShowEntry(CurrentEntry);
        }
    }
}
