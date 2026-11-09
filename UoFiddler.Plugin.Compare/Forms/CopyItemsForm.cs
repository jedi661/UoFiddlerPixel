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
    internal enum CopyTargetMode
    {
        SamePosition,
        StartPosition,
        FreeSlots
    }

    /// <summary>
    /// One planned copy operation: item of the second file -> id in the original file.
    /// </summary>
    internal sealed class CopyMapping
    {
        public int SourceId { get; set; }
        public int TargetId { get; set; } = -1;
        public bool IsValid { get; set; }
        public bool Overwrites { get; set; }
        public string Status { get; set; }

        /// <summary>TileData that will be written to the target, null = tiledata is not touched.</summary>
        public ItemData? TileData { get; set; }
    }

    /// <summary>
    /// Dialog to copy several items of the second file into the original file.
    /// </summary>
    internal sealed class CopyItemsForm : Form
    {
        private readonly List<int> _sources;
        private readonly SecondArtFile _second;
        private readonly SecondTileData _secondTile;
        private readonly Dictionary<int, ItemData> _editedTileData = new Dictionary<int, ItemData>();
        private List<CopyMapping> _mappings = new List<CopyMapping>();
        private bool _initializing = true;

        private readonly RadioButton _rbSame = new RadioButton();
        private readonly RadioButton _rbStart = new RadioButton();
        private readonly RadioButton _rbFree = new RadioButton();
        private readonly TextBox _textStart = new TextBox();
        private readonly CheckBox _chkFreeTileData = new CheckBox();
        private readonly CheckBox _chkTileData = new CheckBox();
        private readonly Button _buttonEditTileData = new Button();
        private readonly ListView _listView = new ListView();
        private readonly Label _labelSummary = new Label();
        private readonly Button _buttonOk = new Button();

        public CopyItemsForm(IEnumerable<int> sourceIds, SecondArtFile second, SecondTileData secondTile,
            CopyTargetMode mode, int startId, bool copyTileData)
        {
            _sources = sourceIds.Distinct().OrderBy(i => i).ToList();
            _second = second;
            _secondTile = secondTile;

            BuildUi();

            _textStart.Text = $"0x{Math.Max(0, startId):X}";
            _chkTileData.Checked = copyTileData && TileData.ItemTable != null;
            _chkTileData.Enabled = TileData.ItemTable != null;
            if (TileData.ItemTable == null)
            {
                _chkTileData.Text = "Copy TileData (original tiledata.mul not loaded)";
            }
            else if (_secondTile == null)
            {
                _chkTileData.Text = "Write TileData (no tiledata.mul in second directory - values have to be edited)";
            }

            switch (mode)
            {
                case CopyTargetMode.StartPosition:
                    _rbStart.Checked = true;
                    break;
                case CopyTargetMode.FreeSlots:
                    _rbFree.Checked = true;
                    break;
                default:
                    _rbSame.Checked = true;
                    break;
            }

            _initializing = false;
            UpdateMappings();
        }

        /// <summary>The planned and valid copy operations (filled when the dialog is closed with OK).</summary>
        public List<CopyMapping> Result { get; private set; } = new List<CopyMapping>();

        public CopyTargetMode Mode => _rbStart.Checked ? CopyTargetMode.StartPosition
            : _rbFree.Checked ? CopyTargetMode.FreeSlots
            : CopyTargetMode.SamePosition;

        public bool CopyTileData => _chkTileData.Checked;

        private void BuildUi()
        {
            Text = $"Copy {_sources.Count} item(s) from second to original";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(760, 560);
            MinimumSize = new Size(620, 420);
            ShowInTaskbar = false;
            MinimizeBox = false;

            TableLayoutPanel main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(6)
            };
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // target
            GroupBox groupTarget = new GroupBox { Text = "Target position", Dock = DockStyle.Fill, AutoSize = true };
            FlowLayoutPanel targetFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };

            _rbSame.Text = "Same position (ID in second = ID in original)";
            _rbSame.AutoSize = true;
            _rbStart.Text = "Consecutive from start position (occupied slots are overwritten)";
            _rbStart.AutoSize = true;
            _rbFree.Text = "Search free (empty) slots from start position";
            _rbFree.AutoSize = true;

            FlowLayoutPanel startFlow = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            startFlow.Controls.Add(new Label { Text = "Start position (hex, or decimal with #):", AutoSize = true, Margin = new Padding(20, 7, 3, 3) });
            _textStart.Width = 90;
            Button buttonFirstFree = new Button { Text = "First free slot", AutoSize = true };
            buttonFirstFree.Click += (s, e) =>
            {
                int free = FindFreeSlot(0, new HashSet<int>(), _chkFreeTileData.Checked);
                if (free >= 0)
                {
                    _textStart.Text = $"0x{free:X}";
                }
            };
            startFlow.Controls.Add(_textStart);
            startFlow.Controls.Add(buttonFirstFree);

            _chkFreeTileData.Text = "A free slot must also have an empty TileData entry";
            _chkFreeTileData.AutoSize = true;
            _chkFreeTileData.Checked = true;
            _chkFreeTileData.Margin = new Padding(20, 3, 3, 3);

            targetFlow.Controls.AddRange(new Control[] { _rbSame, _rbStart, _rbFree, startFlow, _chkFreeTileData });
            groupTarget.Controls.Add(targetFlow);
            main.Controls.Add(groupTarget, 0, 0);

            // tiledata
            FlowLayoutPanel tileFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
            _chkTileData.Text = "Copy TileData (name, flags, height, ...) too";
            _chkTileData.AutoSize = true;
            _chkTileData.Margin = new Padding(3, 7, 3, 3);
            _buttonEditTileData.Text = "Edit TileData...";
            _buttonEditTileData.AutoSize = true;
            _buttonEditTileData.Click += OnEditTileData;
            tileFlow.Controls.Add(_chkTileData);
            tileFlow.Controls.Add(_buttonEditTileData);
            main.Controls.Add(tileFlow, 0, 1);

            // preview
            _listView.Dock = DockStyle.Fill;
            _listView.View = View.Details;
            _listView.FullRowSelect = true;
            _listView.GridLines = true;
            _listView.Columns.Add("Second", 90);
            _listView.Columns.Add("-> Original", 90);
            _listView.Columns.Add("Status", 220);
            _listView.Columns.Add("TileData", 280);
            main.Controls.Add(_listView, 0, 2);

            _labelSummary.AutoSize = true;
            _labelSummary.Margin = new Padding(3, 6, 3, 3);
            main.Controls.Add(_labelSummary, 0, 3);

            FlowLayoutPanel bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
            _buttonOk.Text = "Copy";
            _buttonOk.AutoSize = true;
            _buttonOk.Click += OnOk;
            Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            bottom.Controls.Add(cancel);
            bottom.Controls.Add(_buttonOk);
            main.Controls.Add(bottom, 0, 4);
            CancelButton = cancel;

            Controls.Add(main);

            _rbSame.CheckedChanged += (s, e) => UpdateMappings();
            _rbStart.CheckedChanged += (s, e) => UpdateMappings();
            _rbFree.CheckedChanged += (s, e) => UpdateMappings();
            _textStart.TextChanged += (s, e) => UpdateMappings();
            _chkFreeTileData.CheckedChanged += (s, e) => UpdateMappings();
            _chkTileData.CheckedChanged += (s, e) => UpdateMappings();
        }

        private static bool TryParseStart(string text, out int id)
        {
            text = text?.Trim() ?? string.Empty;
            if (text.StartsWith("#"))
            {
                return ItemCompareHelper.TryParseId(text.Substring(1), false, out id);
            }
            return ItemCompareHelper.TryParseId(text, true, out id);
        }

        /// <summary>
        /// A slot is free when the original has no art there (and optionally no tiledata).
        /// </summary>
        internal static bool IsFreeSlot(int id, bool requireEmptyTileData)
        {
            if (Art.IsValidStatic(id))
            {
                return false;
            }

            if (requireEmptyTileData && TileData.ItemTable != null && id < TileData.ItemTable.Length)
            {
                return ItemCompareHelper.IsEmpty(TileData.ItemTable[id]);
            }

            return true;
        }

        /// <summary>
        /// Returns the first free slot &gt;= start that is not in <paramref name="used"/>, or -1.
        /// </summary>
        internal static int FindFreeSlot(int start, HashSet<int> used, bool requireEmptyTileData)
        {
            int max = Art.GetMaxItemId();
            for (int id = Math.Max(0, start); id <= max; ++id)
            {
                if (!used.Contains(id) && IsFreeSlot(id, requireEmptyTileData))
                {
                    return id;
                }
            }
            return -1;
        }

        private void UpdateMappings()
        {
            if (_initializing)
            {
                return;
            }

            int max = Art.GetMaxItemId();
            bool startOk = TryParseStart(_textStart.Text, out int start);
            _textStart.BackColor = startOk || _rbSame.Checked ? SystemColors.Window : Color.MistyRose;
            _textStart.Enabled = !_rbSame.Checked;
            _chkFreeTileData.Enabled = _rbFree.Checked;

            List<CopyMapping> list = new List<CopyMapping>();
            HashSet<int> used = new HashSet<int>();
            int next = start;

            foreach (int src in _sources)
            {
                CopyMapping m = new CopyMapping { SourceId = src };
                list.Add(m);

                if (_second.GetStatic(src) == null)
                {
                    m.Status = "No image in second file - skipped";
                    continue;
                }

                int target;
                if (_rbSame.Checked)
                {
                    target = src;
                }
                else if (!startOk)
                {
                    m.Status = "Invalid start position";
                    continue;
                }
                else if (_rbStart.Checked)
                {
                    target = next++;
                }
                else
                {
                    target = FindFreeSlot(next, used, _chkFreeTileData.Checked);
                    if (target < 0)
                    {
                        m.Status = "No free slot left";
                        continue;
                    }
                    next = target + 1;
                }

                if (target > max)
                {
                    m.Status = $"Out of range (max 0x{max:X})";
                    continue;
                }

                if (!used.Add(target))
                {
                    m.Status = "Target used twice - skipped";
                    continue;
                }

                m.TargetId = target;
                m.IsValid = true;
                m.Overwrites = Art.IsValidStatic(target);
                m.Status = m.Overwrites ? "Overwrites existing item" : "Free slot";

                if (_chkTileData.Checked && TileData.ItemTable != null && target < TileData.ItemTable.Length)
                {
                    m.TileData = GetTileDataFor(src, target);
                }
            }

            _mappings = list;
            FillListView();
        }

        private ItemData GetTileDataFor(int src, int target)
        {
            if (_editedTileData.TryGetValue(src, out ItemData edited))
            {
                return edited;
            }

            if (_secondTile != null && _secondTile.HasItem(src))
            {
                return _secondTile.ItemTable[src];
            }

            return TileData.ItemTable[target];
        }

        private void FillListView()
        {
            _listView.BeginUpdate();
            _listView.Items.Clear();
            foreach (CopyMapping m in _mappings)
            {
                ListViewItem item = new ListViewItem($"0x{m.SourceId:X4}");
                item.SubItems.Add(m.IsValid ? $"0x{m.TargetId:X4}" : "-");
                item.SubItems.Add(m.Status);
                item.SubItems.Add(m.TileData.HasValue ? ItemCompareHelper.Describe(m.TileData.Value) : (m.IsValid ? "(unchanged)" : string.Empty));
                if (!m.IsValid)
                {
                    item.ForeColor = Color.Gray;
                }
                else if (m.Overwrites)
                {
                    item.ForeColor = Color.DarkRed;
                }
                _listView.Items.Add(item);
            }
            _listView.EndUpdate();

            int valid = _mappings.Count(m => m.IsValid);
            int overwrite = _mappings.Count(m => m.IsValid && m.Overwrites);
            int skipped = _mappings.Count - valid;
            _labelSummary.Text = $"{valid} item(s) will be copied, {overwrite} of them overwrite existing items, {skipped} skipped.";
            _buttonOk.Enabled = valid > 0;
            _buttonEditTileData.Enabled = valid > 0 && TileData.ItemTable != null;
        }

        private void OnEditTileData(object sender, EventArgs e)
        {
            List<TileDataEditEntry> entries = new List<TileDataEditEntry>();
            foreach (CopyMapping m in _mappings.Where(m => m.IsValid && m.TargetId < TileData.ItemTable.Length))
            {
                bool hasSecond = _secondTile != null && _secondTile.HasItem(m.SourceId);
                entries.Add(new TileDataEditEntry
                {
                    SourceId = m.SourceId,
                    TargetId = m.TargetId,
                    HasOriginal = true,
                    Original = TileData.ItemTable[m.TargetId],
                    HasSecond = hasSecond,
                    Second = hasSecond ? _secondTile.ItemTable[m.SourceId] : default,
                    Edited = GetTileDataFor(m.SourceId, m.TargetId),
                    Image = _second.GetStatic(m.SourceId)
                });
            }

            using (TileDataEditForm form = new TileDataEditForm(entries, "TileData for the copied items", Art.IsUOAHS()))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
            }

            foreach (TileDataEditEntry entry in entries)
            {
                _editedTileData[entry.SourceId] = entry.Edited;
            }

            _chkTileData.Checked = true; // editing only makes sense when the values are written
            UpdateMappings();
        }

        private void OnOk(object sender, EventArgs e)
        {
            List<CopyMapping> valid = _mappings.Where(m => m.IsValid).ToList();
            int overwrite = valid.Count(m => m.Overwrites);
            if (overwrite > 0)
            {
                DialogResult answer = MessageBox.Show(this,
                    $"{overwrite} existing item(s) in the original file will be overwritten.\n\nContinue?",
                    "Overwrite items", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes)
                {
                    return;
                }
            }

            Result = valid;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
