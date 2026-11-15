/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * TileData – list of all items and land tiles that differ from the state of
 * the file (after loading / the last "Save Tiledata"), with revert.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class TileDataChangesForm : Form
    {
        private readonly Action<int, bool> _show;
        private readonly Action<List<int>, List<int>> _reverted;
        private bool _refreshPending;

        public TileDataChangesForm()
            : this(null, null)
        {
        }

        /// <param name="show">shows a tile in the TileData tab (index, land)</param>
        /// <param name="reverted">called after reverting (items, lands)</param>
        public TileDataChangesForm(Action<int, bool> show, Action<List<int>, List<int>> reverted)
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            _show = show;
            _reverted = reverted;
            Icon = Options.GetFiddlerIcon();
            TileDataHistory.Changed += OnHistoryChanged;
            FillList();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            TileDataHistory.Changed -= OnHistoryChanged;
            base.OnFormClosed(e);
        }

        /// <summary>every change (also undo) refreshes the list, once per message loop</summary>
        private void OnHistoryChanged(object sender, EventArgs e)
        {
            if (_refreshPending || IsDisposed)
            {
                return;
            }

            _refreshPending = true;
            BeginInvoke(new Action(() =>
            {
                _refreshPending = false;
                if (!IsDisposed)
                {
                    FillList();
                }
            }));
        }

        public void FillList()
        {
            _list.BeginUpdate();
            _list.Items.Clear();
            if (!TileDataHistory.HasBaseline || TileData.ItemTable == null)
            {
                _lblSummary.Text = "The TileData tab is not loaded yet.";
                _list.EndUpdate();
                return;
            }

            int items = 0, lands = 0;
            foreach (int i in TileDataHistory.ChangedLands())
            {
                TileDataHistory.TryGetOriginalLand(i, out LandData before);
                LandData now = TileData.LandTable[i];
                _list.Items.Add(new ListViewItem(new[] { "Land", $"0x{i:X4} ({i})", now.Name, TileDataBatch.Diff(before, now) }) { Tag = (true, i) });
                lands++;
            }

            foreach (int i in TileDataHistory.ChangedItems())
            {
                TileDataHistory.TryGetOriginalItem(i, out ItemData before);
                ItemData now = TileData.ItemTable[i];
                _list.Items.Add(new ListViewItem(new[] { "Item", $"0x{i:X4} ({i})", now.Name, TileDataBatch.Diff(before, now) }) { Tag = (false, i) });
                items++;
            }

            _list.EndUpdate();
            _lblSummary.Text = items + lands == 0 ? "No changes – everything is like in the file." : $"{items} item(s), {lands} land tile(s) changed";
            OnSelectionChanged(this, EventArgs.Empty);
        }

        private List<(bool Land, int Index)> Selected() =>
            _list.SelectedItems.Cast<ListViewItem>().Select(i => ((bool, int))i.Tag).ToList();

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            bool any = _list.SelectedItems.Count > 0;
            _btnShow.Enabled = _list.SelectedItems.Count == 1;
            _btnRevert.Enabled = any;
            _btnRevertAll.Enabled = _list.Items.Count > 0;
        }

        private void OnClickRefresh(object sender, EventArgs e) => FillList();

        private void OnClickShow(object sender, EventArgs e)
        {
            if (_list.SelectedItems.Count == 1)
            {
                (bool land, int index) = ((bool, int))_list.SelectedItems[0].Tag;
                _show?.Invoke(index, land);
            }
        }

        private void OnClickRevertSelected(object sender, EventArgs e) => Revert(Selected());

        private void OnClickRevertAll(object sender, EventArgs e)
        {
            List<(bool, int)> all = _list.Items.Cast<ListViewItem>().Select(i => ((bool, int))i.Tag).ToList();
            if (all.Count == 0 ||
                MessageBox.Show(this, $"Set all {all.Count} changed tiles back to the state of the file?\n(can be undone with Ctrl+Z)", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Revert(all);
        }

        private void Revert(List<(bool Land, int Index)> tiles)
        {
            List<int> items = tiles.Where(t => !t.Land).Select(t => t.Index).ToList();
            List<int> lands = tiles.Where(t => t.Land).Select(t => t.Index).ToList();
            if (items.Count + lands.Count == 0)
            {
                return;
            }

            TileDataHistory.Revert(items, lands, $"Revert {items.Count + lands.Count} tile(s) to the file");
            Options.ChangedUltimaClass["TileData"] = true;
            _reverted?.Invoke(items, lands);
            FillList();
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            foreach (ListViewItem item in _list.Items)
            {
                sb.AppendLine(string.Join("\t", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => s.Text)));
            }

            if (sb.Length > 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }
    }
}
