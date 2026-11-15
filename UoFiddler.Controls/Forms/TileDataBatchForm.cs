/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * TileData – batch edit: change values and flags of many items or land
 * tiles at once (selection, ID ranges, all tiles of the list, filters).
 * Everything is one undo step.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class TileDataBatchForm : Form
    {
        private readonly bool _land;
        private readonly List<int> _selected = new List<int>();
        private readonly List<int> _visible = new List<int>();
        private readonly Dictionary<TileField, (CheckBox Check, ComboBox Op, TextBox Text)> _rows = new Dictionary<TileField, (CheckBox, ComboBox, TextBox)>();
        private readonly List<TileFlag> _flags = new List<TileFlag>();
        private bool _loading;

        /// <summary>indices that were changed by "Apply"</summary>
        public List<int> ChangedIndices { get; private set; } = new List<int>();

        public bool IsLand => _land;

        public TileDataBatchForm()
            : this(false, null, null)
        {
        }

        public TileDataBatchForm(bool land, IEnumerable<int> selected, IEnumerable<int> visible)
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            _land = land;
            _selected.AddRange(selected ?? Enumerable.Empty<int>());
            _visible.AddRange(visible ?? Enumerable.Empty<int>());
            Icon = Options.GetFiddlerIcon();
            Text = land ? "TileData – batch edit land tiles" : "TileData – batch edit items";

            _loading = true;
            _rows[TileField.Name] = (_chkFName, _cmbOpName, _txtName);
            _rows[TileField.Weight] = (_chkFWeight, _cmbOpWeight, _txtWeight);
            _rows[TileField.Quality] = (_chkFQuality, _cmbOpQuality, _txtQuality);
            _rows[TileField.Animation] = (_chkFAnimation, _cmbOpAnimation, _txtAnimation);
            _rows[TileField.Quantity] = (_chkFQuantity, _cmbOpQuantity, _txtQuantity);
            _rows[TileField.Hue] = (_chkFHue, _cmbOpHue, _txtHue);
            _rows[TileField.StackingOffset] = (_chkFStackingOffset, _cmbOpStackingOffset, _txtStackingOffset);
            _rows[TileField.Value] = (_chkFValue, _cmbOpValue, _txtValue);
            _rows[TileField.Height] = (_chkFHeight, _cmbOpHeight, _txtHeight);
            _rows[TileField.MiscData] = (_chkFMiscData, _cmbOpMiscData, _txtMiscData);
            _rows[TileField.Unk2] = (_chkFUnk2, _cmbOpUnk2, _txtUnk2);
            _rows[TileField.Unk3] = (_chkFUnk3, _cmbOpUnk3, _txtUnk3);
            _rows[TileField.TextureId] = (_chkFTextureId, _cmbOpTextureId, _txtTextureId);

            TileField[] shown = land ? TileDataBatch.LandFields : TileDataBatch.ItemFields;
            foreach (KeyValuePair<TileField, (CheckBox Check, ComboBox Op, TextBox Text)> row in _rows)
            {
                bool show = shown.Contains(row.Key);
                row.Value.Check.Visible = row.Value.Op.Visible = row.Value.Text.Visible = show;
                row.Value.Op.SelectedIndex = 0;
            }

            _txtNameWith.Visible = false;

            // flags (old clients: only the lower 32 flags exist)
            int count = Art.IsUOAHS() ? 64 : 32;
            _cmbOnlyFlag.Items.Clear();
            foreach (TileFlag flag in Enum.GetValues(typeof(TileFlag)).Cast<TileFlag>().Where(f => f != TileFlag.None).Take(count))
            {
                _flags.Add(flag);
                _cmbOnlyFlag.Items.Add(flag.ToString());
                _gridFlags.Rows.Add(flag.ToString(), false, false, string.Empty);
            }

            _cmbOnlyFlag.SelectedIndex = 0;

            _lblSelected.Text = _selected.Count == 0 ? "(nothing selected)" : $"{_selected.Count} tile(s): {Shorten(TileDataBatch.FormatRanges(_selected))}";
            _lblVisible.Text = $"{_visible.Count} tiles in the list (with the filter of the tab)";
            _rbSelected.Enabled = _selected.Count > 0;
            if (_selected.Count > 0)
            {
                _rbSelected.Checked = true;
            }
            else
            {
                _rbRange.Checked = true;
            }

            _txtRange.Text = TileDataBatch.FormatRanges(_selected);
            _loading = false;
            UpdatePreview();
        }

        private static string Shorten(string s) => s.Length > 120 ? s.Substring(0, 117) + "…" : s;

        #region Targets

        private int Max => _land ? TileData.LandTable?.Length ?? 0 : TileData.ItemTable?.Length ?? 0;

        private string NameOf(int i) => _land ? TileData.LandTable[i].Name : TileData.ItemTable[i].Name;

        private TileFlag FlagsOf(int i) => _land ? TileData.LandTable[i].Flags : TileData.ItemTable[i].Flags;

        /// <summary>the tiles the batch is applied to (null = the range can not be read)</summary>
        private List<int> Targets(out string error)
        {
            error = null;
            IEnumerable<int> source;
            if (_rbRange.Checked)
            {
                if (!TileDataBatch.TryParseRanges(_txtRange.Text, Max, out List<int> ids, out error))
                {
                    return null;
                }

                source = ids;
            }
            else
            {
                source = _rbVisible.Checked ? _visible : _selected;
            }

            if (_chkOnlyName.Checked && _txtOnlyName.Text.Length > 0)
            {
                string part = _txtOnlyName.Text;
                source = source.Where(i => (NameOf(i) ?? string.Empty).IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (_chkOnlyFlag.Checked && _cmbOnlyFlag.SelectedIndex >= 0)
            {
                TileFlag flag = _flags[_cmbOnlyFlag.SelectedIndex];
                source = source.Where(i => (FlagsOf(i) & flag) != 0);
            }

            return source.Where(i => i >= 0 && i < Max).Distinct().ToList();
        }

        private void OnTargetChanged(object sender, EventArgs e) => UpdatePreview();

        private void OnRangeTextChanged(object sender, EventArgs e)
        {
            if (!_loading && !_rbRange.Checked && _txtRange.Focused)
            {
                _rbRange.Checked = true;
            }

            UpdatePreview();
        }

        #endregion

        #region Values / flags

        private TileDataBatch BuildBatch()
        {
            var batch = new TileDataBatch();
            foreach (KeyValuePair<TileField, (CheckBox Check, ComboBox Op, TextBox Text)> row in _rows)
            {
                if (!row.Value.Check.Visible || !row.Value.Check.Checked)
                {
                    continue;
                }

                FieldOperation op;
                if (row.Key == TileField.Name)
                {
                    op = row.Value.Op.SelectedIndex == 1 ? FieldOperation.Prefix
                        : row.Value.Op.SelectedIndex == 2 ? FieldOperation.Suffix
                        : row.Value.Op.SelectedIndex == 3 ? FieldOperation.Replace : FieldOperation.Set;
                }
                else
                {
                    op = row.Value.Op.SelectedIndex == 1 ? FieldOperation.Add : row.Value.Op.SelectedIndex == 2 ? FieldOperation.Subtract : FieldOperation.Set;
                }

                batch.Fields.Add(new FieldChange { Field = row.Key, Operation = op, Value = row.Value.Text.Text.Trim(), With = _txtNameWith.Text });
            }

            for (int r = 0; r < _gridFlags.Rows.Count && r < _flags.Count; r++)
            {
                if (_gridFlags.Rows[r].Cells[1].Value is bool set && set)
                {
                    batch.SetFlags |= _flags[r];
                }

                if (_gridFlags.Rows[r].Cells[2].Value is bool clear && clear)
                {
                    batch.ClearFlags |= _flags[r];
                }
            }

            return batch;
        }

        private void OnFieldsChanged(object sender, EventArgs e)
        {
            _txtNameWith.Visible = _cmbOpName.SelectedIndex == 3 && _rows[TileField.Name].Check.Visible;
            UpdatePreview();
        }

        /// <summary>typing a value ticks the field</summary>
        private void OnFieldTextChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            foreach (var (check, _, text) in _rows.Values)
            {
                if ((sender == text || (sender == _txtNameWith && check == _chkFName)) && ((TextBox)sender).Text.Length > 0)
                {
                    check.Checked = true;
                }
            }

            UpdatePreview();
        }

        private void OnGridDirtyStateChanged(object sender, EventArgs e)
        {
            if (_gridFlags.IsCurrentCellDirty)
            {
                _gridFlags.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void OnFlagCellChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_loading || e.RowIndex < 0 || (e.ColumnIndex != 1 && e.ColumnIndex != 2))
            {
                return;
            }

            // "set" and "remove" exclude each other
            DataGridViewRow row = _gridFlags.Rows[e.RowIndex];
            if (row.Cells[e.ColumnIndex].Value is bool v && v)
            {
                _loading = true;
                row.Cells[e.ColumnIndex == 1 ? 2 : 1].Value = false;
                _loading = false;
            }

            UpdatePreview();
        }

        #endregion

        #region Preview / apply

        private void UpdatePreview()
        {
            if (_loading || Max == 0)
            {
                return;
            }

            List<int> targets = Targets(out string error);
            if (targets == null)
            {
                _lblPreview.Text = error;
                _lblPreview.ForeColor = System.Drawing.Color.Firebrick;
                _btnApply.Enabled = false;
                return;
            }

            // how many target tiles have each flag
            var counts = new int[_flags.Count];
            foreach (int i in targets)
            {
                TileFlag f = FlagsOf(i);
                for (int k = 0; k < _flags.Count; k++)
                {
                    if ((f & _flags[k]) != 0)
                    {
                        counts[k]++;
                    }
                }
            }

            _loading = true;
            for (int k = 0; k < _flags.Count && k < _gridFlags.Rows.Count; k++)
            {
                _gridFlags.Rows[k].Cells[3].Value = counts[k] == 0 ? string.Empty : $"{counts[k]} / {targets.Count}";
            }

            _loading = false;

            TileDataBatch batch = BuildBatch();
            string invalid = batch.IsEmpty ? null : batch.Validate();
            int changes = 0;
            if (invalid == null && !batch.IsEmpty)
            {
                changes = _land
                    ? targets.Count(i => !TileDataHistory.SameLand(TileData.LandTable[i], batch.Apply(TileData.LandTable[i])))
                    : targets.Count(i => !TileDataHistory.SameItem(TileData.ItemTable[i], batch.Apply(TileData.ItemTable[i])));
            }

            _lblPreview.ForeColor = invalid != null ? System.Drawing.Color.Firebrick : System.Drawing.SystemColors.ControlText;
            _lblPreview.Text = invalid ?? (batch.IsEmpty
                ? $"{targets.Count} tile(s) – tick a value or a flag"
                : $"{targets.Count} tile(s), {changes} will change");
            _btnApply.Enabled = invalid == null && changes > 0;
        }

        private void OnClickApply(object sender, EventArgs e)
        {
            List<int> targets = Targets(out string error);
            TileDataBatch batch = BuildBatch();
            string invalid = error ?? batch.Validate();
            if (invalid != null)
            {
                MessageBox.Show(this, invalid, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (targets.Count > 500 &&
                MessageBox.Show(this, $"Change {targets.Count} tiles? (can be undone with Ctrl+Z)", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            string description = $"Batch edit of {targets.Count} {(_land ? "land tiles" : "items")}";
            ChangedIndices = _land ? batch.ApplyToLands(targets, description) : batch.ApplyToItems(targets, description);
            if (ChangedIndices.Count > 0)
            {
                Options.ChangedUltimaClass["TileData"] = true;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion
    }
}
