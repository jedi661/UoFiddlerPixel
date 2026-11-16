/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Overview and selection of land tiles, items or lights as a picture grid
 * (search by id, hex, range or name; items optionally only light sources).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public enum TilePickerMode
    {
        Land,
        Item,
        Light
    }

    public partial class TilePickerForm : Form
    {
        private readonly TilePickerMode _mode;
        private List<int> _all = new List<int>();

        public int SelectedId { get; private set; } = -1;

        public TilePickerForm()
            : this(TilePickerMode.Land, -1, false)
        {
        }

        public TilePickerForm(TilePickerMode mode, int current, bool onlyLightSources)
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            _mode = mode;
            Icon = Options.GetFiddlerIcon();
            Text = mode == TilePickerMode.Land ? "Choose land tile" : mode == TilePickerMode.Item ? "Choose item" : "All lights";
            _chkOnlyLight.Visible = mode == TilePickerMode.Item;
            _chkOnlyLight.Checked = onlyLightSources && mode == TilePickerMode.Item;
            _grid.BlackBackground = mode == TilePickerMode.Light;
            _grid.ImageProvider = Image;
            _grid.LabelProvider = Label;
            _cmbSize.SelectedIndex = mode == TilePickerMode.Land ? 0 : 1;

            Cursor = Cursors.WaitCursor;
            try
            {
                switch (mode)
                {
                    case TilePickerMode.Land:
                        _all = Enumerable.Range(0, 0x4000).Where(Art.IsValidLand).ToList();
                        break;
                    case TilePickerMode.Item:
                        _all = Enumerable.Range(0, Art.GetMaxItemId() + 1).Where(Art.IsValidStatic).ToList();
                        break;
                    default:
                        _all = Enumerable.Range(0, Ultima.Light.Count).Where(Ultima.Light.TestLight).ToList();
                        break;
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            ApplyFilter();
            if (current >= 0)
            {
                _grid.SelectedId = current;
            }

            OnSelectionChanged(this, EventArgs.Empty);
        }

        private Bitmap Image(int id)
        {
            switch (_mode)
            {
                case TilePickerMode.Land: return Art.GetLand(id);
                case TilePickerMode.Item: return Art.GetStatic(id);
                default: return Ultima.Light.GetLight(id);
            }
        }

        private string NameOf(int id)
        {
            try
            {
                switch (_mode)
                {
                    case TilePickerMode.Land: return TileData.LandTable?[id].Name;
                    case TilePickerMode.Item: return id < (TileData.ItemTable?.Length ?? 0) ? TileData.ItemTable[id].Name : null;
                    default: return LightNames.Get(id);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string Label(int id)
        {
            string name = NameOf(id);
            string hex = _mode == TilePickerMode.Light ? $"{id} (0x{id:X2})" : $"0x{id:X4}";
            return string.IsNullOrWhiteSpace(name) ? hex : $"{hex} {name}";
        }

        private bool IsLightSource(int id) =>
            TileData.ItemTable != null && id < TileData.ItemTable.Length && (TileData.ItemTable[id].Flags & TileFlag.LightSource) != 0;

        private void ApplyFilter()
        {
            IEnumerable<int> list = _all;
            if (_mode == TilePickerMode.Item && _chkOnlyLight.Checked)
            {
                list = list.Where(IsLightSource);
            }

            string text = _txtSearch.Text.Trim();
            if (text.Length > 0)
            {
                if (TileDataBatch.TryParseRanges(text, int.MaxValue, out List<int> ids, out _) && ids.Count > 0 && (text.Any(char.IsDigit)))
                {
                    var set = new HashSet<int>(ids);
                    List<int> byId = list.Where(set.Contains).ToList();
                    list = byId.Count > 0 ? byId : list.Where(i => (NameOf(i) ?? string.Empty).IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else
                {
                    list = list.Where(i => (NameOf(i) ?? string.Empty).IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }

            int keep = _grid.SelectedId;
            _grid.Ids = list.ToList();
            if (keep >= 0)
            {
                _grid.SelectedId = keep;
            }

            _lblCount.Text = $"{_grid.Ids.Count} of {_all.Count}";
        }

        private void OnFilterChanged(object sender, EventArgs e) => ApplyFilter();

        private void OnSizeChanged(object sender, EventArgs e)
        {
            if (int.TryParse(_cmbSize.SelectedItem as string, out int size))
            {
                _grid.CellSize = size;
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            int id = _grid.SelectedId;
            _lblSelected.Text = id < 0 ? "-" : Label(id);
            _btnOk.Enabled = id >= 0;
        }

        private void OnItemActivated(object sender, EventArgs e) => OnClickOk(sender, e);

        private void OnClickOk(object sender, EventArgs e)
        {
            if (_grid.SelectedId < 0)
            {
                return;
            }

            SelectedId = _grid.SelectedId;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
