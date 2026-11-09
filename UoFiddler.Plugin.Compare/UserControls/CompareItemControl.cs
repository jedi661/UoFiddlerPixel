/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.Compare.Classes;
using UoFiddler.Plugin.Compare.Forms;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareItemControl : UserControl
    {
        private enum FilterMode
        {
            All = 0,
            AllDifferences = 1,
            Changed = 2,
            OnlyOriginal = 3,
            OnlySecond = 4,
            FreeInOriginal = 5
        }

        // File names are kept from the old version, so existing settings are still found.
        private const string _settingsFileName = "CompareiItemsDirectoryisSettings.txt";
        private const string _lastDirectoriesFileName = "CompareiItemsLastDirectories.txt";
        private const int _maxRecentDirectories = 10;
        private const int _maxReplaceMenuEntries = 30;

        private readonly string _settingsDirectory =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DirectoryisSettings");

        private SecondArtFile _second;
        private SecondTileData _secondTile;

        /// <summary>Compare result per item id (index = item id).</summary>
        private ItemDiffState[] _state = Array.Empty<ItemDiffState>();

        /// <summary>Ids shown in both lists (sorted ascending, both lists always show the same ids).</summary>
        private int[] _visibleIds = Array.Empty<int>();

        private CancellationTokenSource _scanCts;
        private Task _scanTask = Task.CompletedTask;
        private bool _scanComplete;
        private int _filterVersion;
        private bool _syncingSelection;
        private bool _changingFilter;
        private bool _loaded;
        private bool _eventsRegistered;
        private readonly List<Image> _menuThumbnails = new List<Image>();

        #region [ CompareItemControl ]
        public CompareItemControl()
        {
            InitializeComponent();

            _changingFilter = true;
            comboBoxFilter.SelectedIndex = 0;
            comboBoxSearchMode.SelectedIndex = 0;
            _changingFilter = false;

            UpdateButtons();
        }
        #endregion

        #region [ Load / CleanUp ]
        private void OnLoad(object sender, EventArgs e)
        {
            if (DesignMode || _loaded)
            {
                return;
            }

            _loaded = true;

            EnsureSettingsDirectoryExists();
            string settingsFile = Path.Combine(_settingsDirectory, _settingsFileName);
            if (File.Exists(settingsFile))
            {
                textBoxSecondDir.Text = File.ReadAllText(settingsFile).Trim();
            }
            LoadComboBoxSaveDir();

            ResetState();
            RebuildLists();

            if (!_eventsRegistered)
            {
                ControlEvents.ItemChangeEvent += OnItemChangeEvent;
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                _eventsRegistered = true;
            }
        }

        /// <summary>
        /// Called from Dispose (see designer file).
        /// </summary>
        private void CleanUp()
        {
            CancelScan();

            if (_eventsRegistered)
            {
                ControlEvents.ItemChangeEvent -= OnItemChangeEvent;
                ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
                _eventsRegistered = false;
            }

            ClearReplaceMenu();
            _second?.Dispose();
            _second = null;
        }

        private void OnItemChangeEvent(object sender, int id)
        {
            if (!_loaded || ReferenceEquals(sender, this))
            {
                return;
            }

            // an item was changed in another tab -> compare it again
            RefreshState(id);
            listBoxOrg.Invalidate();
            listBoxSec.Invalidate();
            UpdatePreviews();
        }

        private void OnFilePathChangeEvent()
        {
            if (!_loaded)
            {
                return;
            }

            // the original client files were reloaded
            ResetState();
            RebuildLists();
            if (_second != null)
            {
                _ = StartScanAsync();
            }
        }
        #endregion

        #region [ Ids / State ]
        private int OrgMaxId => Art.GetMaxItemId();
        private int SecMaxId => _second?.GetMaxItemId() ?? -1;
        private int MaxId => Math.Max(OrgMaxId, SecMaxId);

        private bool IsOrgValid(int id) => id >= 0 && id <= OrgMaxId && Art.IsValidStatic(id);

        private Bitmap GetOrg(int id) => IsOrgValid(id) ? Art.GetStatic(id) : null;

        private Bitmap GetSec(int id) => _second != null && _second.IsValidStatic(id) ? _second.GetStatic(id) : null;

        private void ResetState()
        {
            _state = new ItemDiffState[MaxId + 1];
            _scanComplete = false;
        }

        private ItemDiffState GetState(int id)
        {
            if (_second == null || id < 0 || id >= _state.Length)
            {
                return ItemDiffState.Unknown;
            }

            if (_state[id] == ItemDiffState.Unknown)
            {
                _state[id] = ComputeState(id);
            }

            return _state[id];
        }

        private void RefreshState(int id)
        {
            if (id >= 0 && id < _state.Length)
            {
                _state[id] = ItemDiffState.Unknown;
                if (_second != null)
                {
                    _state[id] = ComputeState(id);
                }
            }
        }

        private ItemDiffState ComputeState(int id)
        {
            Bitmap org = GetOrg(id);
            Bitmap sec = GetSec(id);

            if (org == null && sec == null)
            {
                return ItemDiffState.BothEmpty;
            }

            if (org == null)
            {
                return ItemDiffState.OnlySecond;
            }

            if (sec == null)
            {
                return ItemDiffState.OnlyOriginal;
            }

            bool equal = ItemCompareHelper.BitmapsEqual(org, sec);

            // Without the data cache every GetStatic call of the second file returns a new bitmap -> free it now.
            if (!Files.CacheData)
            {
                sec.Dispose();
            }

            return equal ? ItemDiffState.Same : ItemDiffState.Different;
        }

        private bool IsDifference(ItemDiffState state)
        {
            return state == ItemDiffState.Different || state == ItemDiffState.OnlyOriginal || state == ItemDiffState.OnlySecond;
        }
        #endregion

        #region [ Scan ]
        private void CancelScan()
        {
            _scanCts?.Cancel();
        }

        /// <summary>
        /// Compares all items. Runs on the UI thread in small chunks (the Ultima classes are not thread safe),
        /// so the window stays responsive.
        /// </summary>
        private Task StartScanAsync()
        {
            CancelScan();

            CancellationTokenSource cts = new CancellationTokenSource();
            _scanCts = cts;
            _scanComplete = false;
            _scanTask = ScanAsync(cts);
            return _scanTask;
        }

        private async Task ScanAsync(CancellationTokenSource cts)
        {
            CancellationToken token = cts.Token;
            ItemDiffState[] state = _state;

            progressBarScan.Maximum = Math.Max(1, state.Length);
            progressBarScan.Value = 0;
            progressBarScan.Visible = true;
            buttonCancelScan.Visible = true;
            labelStatus.Text = "Comparing...";

            bool completed = true;
            Stopwatch watch = Stopwatch.StartNew();
            for (int id = 0; id < state.Length; ++id)
            {
                if (token.IsCancellationRequested || IsDisposed || !ReferenceEquals(state, _state))
                {
                    completed = false;
                    break;
                }

                if (state[id] == ItemDiffState.Unknown)
                {
                    state[id] = ComputeState(id);
                }

                if (watch.ElapsedMilliseconds > 40)
                {
                    progressBarScan.Value = id;
                    labelStatus.Text = $"Comparing... {id * 100 / state.Length}%";
                    await Task.Delay(1);
                    watch.Restart();
                }
            }

            if (ReferenceEquals(_scanCts, cts))
            {
                _scanCts = null;
                _scanComplete = completed && ReferenceEquals(state, _state);

                if (!IsDisposed)
                {
                    progressBarScan.Visible = false;
                    buttonCancelScan.Visible = false;
                }
            }
            cts.Dispose();

            if (!IsDisposed)
            {
                UpdateStatus();
                listBoxOrg.Invalidate();
                listBoxSec.Invalidate();
            }
        }

        private void ButtonCancelScan_Click(object sender, EventArgs e)
        {
            CancelScan();
        }
        #endregion

        #region [ Lists / Filter ]
        private FilterMode CurrentFilter => comboBoxFilter.SelectedIndex < 0 ? FilterMode.All : (FilterMode)comboBoxFilter.SelectedIndex;

        private static bool FilterNeedsCompare(FilterMode mode) => mode != FilterMode.All && mode != FilterMode.FreeInOriginal;

        private bool MatchesFilter(int id, FilterMode mode)
        {
            switch (mode)
            {
                case FilterMode.AllDifferences:
                    return IsDifference(GetState(id));
                case FilterMode.Changed:
                    return GetState(id) == ItemDiffState.Different;
                case FilterMode.OnlyOriginal:
                    return GetState(id) == ItemDiffState.OnlyOriginal;
                case FilterMode.OnlySecond:
                    return GetState(id) == ItemDiffState.OnlySecond;
                case FilterMode.FreeInOriginal:
                    return id <= OrgMaxId && !Art.IsValidStatic(id);
                default:
                    return true;
            }
        }

        private void SetFilterSilently(FilterMode mode)
        {
            _changingFilter = true;
            comboBoxFilter.SelectedIndex = (int)mode;
            _changingFilter = false;
        }

        private async void OnFilterChanged(object sender, EventArgs e)
        {
            if (_changingFilter || !_loaded)
            {
                return;
            }

            await ApplyFilterAsync();
        }

        private async Task ApplyFilterAsync()
        {
            int version = ++_filterVersion;
            FilterMode mode = CurrentFilter;

            if (FilterNeedsCompare(mode))
            {
                if (_second == null)
                {
                    MessageBox.Show("The second item file is not loaded.", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SetFilterSilently(FilterMode.All);
                    return;
                }

                if (!_scanComplete)
                {
                    if (_scanCts == null)
                    {
                        _ = StartScanAsync();
                    }

                    await _scanTask;

                    if (version != _filterVersion)
                    {
                        return; // the filter was changed again in the meantime
                    }

                    if (!_scanComplete)
                    {
                        // comparison was cancelled
                        SetFilterSilently(FilterMode.All);
                        RebuildLists();
                        return;
                    }
                }
            }

            RebuildLists();
        }

        /// <summary>
        /// Fills both lists with the ids of the current filter and keeps the focused item if possible.
        /// </summary>
        private void RebuildLists()
        {
            int focusId = GetFocusedId(listBoxSec);
            if (focusId < 0)
            {
                focusId = GetFocusedId(listBoxOrg);
            }

            FilterMode mode = CurrentFilter;
            int max = _second == null ? OrgMaxId : MaxId;
            List<int> ids = new List<int>(max + 1);
            for (int id = 0; id <= max; ++id)
            {
                if (MatchesFilter(id, mode))
                {
                    ids.Add(id);
                }
            }

            _visibleIds = ids.ToArray();
            object[] boxed = _visibleIds.Cast<object>().ToArray();

            _syncingSelection = true;
            try
            {
                listBoxOrg.BeginUpdate();
                listBoxOrg.Items.Clear();
                listBoxOrg.Items.AddRange(boxed);
                listBoxOrg.EndUpdate();

                listBoxSec.BeginUpdate();
                listBoxSec.Items.Clear();
                if (_second != null)
                {
                    listBoxSec.Items.AddRange(boxed);
                }
                listBoxSec.EndUpdate();
            }
            finally
            {
                _syncingSelection = false;
            }

            if (_visibleIds.Length > 0)
            {
                int pos = focusId >= 0 ? Array.BinarySearch(_visibleIds, focusId) : 0;
                if (pos < 0)
                {
                    pos = Math.Min(~pos, _visibleIds.Length - 1);
                }
                SelectId(_visibleIds[pos]);
            }
            else
            {
                UpdatePreviews();
            }

            UpdateStatus();
            UpdateButtons();
        }

        private int IndexOfId(int id)
        {
            int pos = Array.BinarySearch(_visibleIds, id);
            return pos >= 0 ? pos : -1;
        }

        /// <summary>
        /// Selects exactly this id in both lists and scrolls to it.
        /// </summary>
        private void SelectId(int id)
        {
            int pos = IndexOfId(id);
            if (pos < 0)
            {
                return;
            }

            _syncingSelection = true;
            try
            {
                SelectSingle(listBoxOrg, pos);
                if (listBoxSec.Items.Count > pos)
                {
                    SelectSingle(listBoxSec, pos);
                }
            }
            finally
            {
                _syncingSelection = false;
            }

            UpdatePreviews();
        }

        private static void SelectSingle(ListBox listBox, int pos)
        {
            if (pos < 0 || pos >= listBox.Items.Count)
            {
                return;
            }

            listBox.BeginUpdate();
            listBox.ClearSelected();
            listBox.SetSelected(pos, true);
            listBox.EndUpdate();
            EnsureVisible(listBox, pos);
        }

        private static void EnsureVisible(ListBox listBox, int pos)
        {
            int visible = Math.Max(1, listBox.ClientSize.Height / Math.Max(1, listBox.ItemHeight));
            if (pos < listBox.TopIndex || pos >= listBox.TopIndex + visible)
            {
                listBox.TopIndex = Math.Max(0, pos - (visible / 2));
            }
        }

        /// <summary>
        /// Id of the item that has the focus (the one last clicked), -1 when nothing is selected.
        /// </summary>
        private static int GetFocusedId(ListBox listBox)
        {
            int index = ListBoxHelper.GetFocusedSelectedIndex(listBox);
            return index >= 0 ? (int)listBox.Items[index] : -1;
        }

        private static List<int> GetSelectedIds(ListBox listBox)
        {
            List<int> ids = new List<int>(listBox.SelectedIndices.Count);
            foreach (int index in listBox.SelectedIndices)
            {
                ids.Add((int)listBox.Items[index]);
            }
            ids.Sort();
            return ids;
        }
        #endregion

        #region [ Selection ]
        private void OnIndexChangedOrg(object sender, EventArgs e)
        {
            if (_syncingSelection)
            {
                return;
            }

            int id = GetFocusedId(listBoxOrg);
            if (id >= 0 && listBoxOrg.SelectedIndices.Count == 1 && listBoxSec.Items.Count > 0)
            {
                // single selection -> show the same id in the second list
                int pos = IndexOfId(id);
                _syncingSelection = true;
                try
                {
                    SelectSingle(listBoxSec, pos);
                }
                finally
                {
                    _syncingSelection = false;
                }
            }

            if (id >= 0)
            {
                searchTextBox.Text = FormatId(id);
            }

            UpdatePreviews();
            UpdateButtons();
        }

        private void OnIndexChangedSec(object sender, EventArgs e)
        {
            if (_syncingSelection)
            {
                return;
            }

            int id = GetFocusedId(listBoxSec);
            if (id >= 0 && listBoxSec.SelectedIndices.Count == 1)
            {
                int pos = IndexOfId(id);
                _syncingSelection = true;
                try
                {
                    SelectSingle(listBoxOrg, pos);
                }
                finally
                {
                    _syncingSelection = false;
                }
            }

            UpdatePreviews();
            UpdateButtons();
        }

        private string FormatId(int id)
        {
            return comboBoxSearchMode.SelectedIndex == 1 ? id.ToString() : $"0x{id:X}";
        }

        private void UpdateButtons()
        {
            bool secLoaded = _second != null;
            bool secSelected = secLoaded && listBoxSec.SelectedIndices.Count > 0;
            bool orgSelected = listBoxOrg.SelectedIndices.Count > 0;
            bool tileDataLoaded = TileData.ItemTable != null;

            btLeftMoveItem.Enabled = secSelected;
            btLeftMoveItemMore.Enabled = secSelected;
            buttonCopyTo.Enabled = secSelected;
            buttonCopyAllDiff.Enabled = secLoaded;
            btremoveitemfromindex.Enabled = orgSelected;
            buttonEditTileData.Enabled = orgSelected && tileDataLoaded;
            buttonSaveTileData.Enabled = tileDataLoaded;
            checkBoxCopyTileData.Enabled = secLoaded && _secondTile != null && tileDataLoaded;
            if (!checkBoxCopyTileData.Enabled)
            {
                checkBoxCopyTileData.Checked = false;
            }
        }
        #endregion

        #region [ Drawing ]
        private void DrawItemOrg(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= listBoxOrg.Items.Count)
            {
                return;
            }

            int id = (int)listBoxOrg.Items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) != 0;

            Color color = Color.Gray;
            if (!IsOrgValid(id))
            {
                color = Color.Red;
            }
            else if (_second != null)
            {
                ItemDiffState state = GetState(id);
                if (state == ItemDiffState.Different)
                {
                    color = Color.Blue;
                }
                else if (state == ItemDiffState.OnlyOriginal)
                {
                    color = Color.DarkOrange;
                }
            }

            string name = TileData.ItemTable != null && id < TileData.ItemTable.Length ? TileData.ItemTable[id].Name : null;
            DrawListEntry(e, id, name, color, selected);
        }

        private void DrawItemSec(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= listBoxSec.Items.Count || _second == null)
            {
                return;
            }

            int id = (int)listBoxSec.Items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) != 0;

            Color color = Color.Gray;
            if (!_second.IsValidStatic(id))
            {
                color = Color.Red;
            }
            else
            {
                ItemDiffState state = GetState(id);
                if (state == ItemDiffState.Different)
                {
                    color = Color.Blue;
                }
                else if (state == ItemDiffState.OnlySecond)
                {
                    color = Color.Green;
                }
            }

            string name = _secondTile != null && _secondTile.HasItem(id) ? _secondTile.ItemTable[id].Name : null;
            DrawListEntry(e, id, name, color, selected);
        }

        private static void DrawListEntry(DrawItemEventArgs e, int id, string name, Color color, bool selected)
        {
            using (Brush back = new SolidBrush(selected ? Color.LightSteelBlue : e.BackColor))
            {
                e.Graphics.FillRectangle(back, e.Bounds);
            }

            string text = string.IsNullOrEmpty(name) ? $"0x{id:X4} ({id})" : $"0x{id:X4} ({id}) {name}";
            Rectangle bounds = new Rectangle(e.Bounds.X + 3, e.Bounds.Y, e.Bounds.Width - 3, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, text, e.Font, bounds, color,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            if ((e.State & DrawItemState.Focus) != 0)
            {
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds);
            }
        }
        #endregion

        #region [ Preview ]
        private void UpdatePreviews()
        {
            int orgId = GetFocusedId(listBoxOrg);
            int secId = GetFocusedId(listBoxSec);

            if (orgId >= 0)
            {
                Bitmap bmp = GetOrg(orgId);
                SetPreview(pictureBoxOrg, bmp);
                string size = bmp != null ? $" {bmp.Width}x{bmp.Height}" : " (empty)";
                string tile = TileData.ItemTable != null && orgId < TileData.ItemTable.Length
                    ? "  | " + ItemCompareHelper.Describe(TileData.ItemTable[orgId])
                    : string.Empty;
                labelOrgInfo.Text = $"Original 0x{orgId:X4} ({orgId}){size}{tile}";
            }
            else
            {
                SetPreview(pictureBoxOrg, null);
                labelOrgInfo.Text = "Original";
            }

            if (secId >= 0 && _second != null)
            {
                Bitmap bmp = GetSec(secId);
                SetPreview(pictureBoxSec, bmp);
                string size = bmp != null ? $" {bmp.Width}x{bmp.Height}" : " (empty)";
                string state;
                switch (GetState(secId))
                {
                    case ItemDiffState.Same: state = " [same]"; break;
                    case ItemDiffState.Different: state = " [changed]"; break;
                    case ItemDiffState.OnlySecond: state = " [new]"; break;
                    case ItemDiffState.OnlyOriginal: state = " [missing]"; break;
                    default: state = string.Empty; break;
                }
                string tile = _secondTile != null && _secondTile.HasItem(secId)
                    ? "  | " + ItemCompareHelper.Describe(_secondTile.ItemTable[secId])
                    : string.Empty;
                labelSecInfo.Text = $"Second 0x{secId:X4} ({secId}){size}{state}{tile}";
            }
            else
            {
                SetPreview(pictureBoxSec, null);
                labelSecInfo.Text = _second == null ? "Second (not loaded)" : "Second";
            }

            toolTip1.SetToolTip(labelOrgInfo, labelOrgInfo.Text);
            toolTip1.SetToolTip(labelSecInfo, labelSecInfo.Text);
        }

        /// <summary>
        /// Shows an image centered, big images are zoomed to fit.
        /// </summary>
        private static void SetPreview(PictureBox pictureBox, Image image)
        {
            pictureBox.BackgroundImage = image;
            UpdatePreviewLayout(pictureBox);
        }

        private static void UpdatePreviewLayout(PictureBox pictureBox)
        {
            Image image = pictureBox.BackgroundImage;
            pictureBox.BackgroundImageLayout =
                image != null && (image.Width > pictureBox.ClientSize.Width || image.Height > pictureBox.ClientSize.Height)
                    ? ImageLayout.Zoom
                    : ImageLayout.Center;
        }

        private void OnPreviewSizeChanged(object sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox)
            {
                UpdatePreviewLayout(pictureBox);
            }
        }

        private void UpdateStatus()
        {
            if (_second == null)
            {
                labelStatus.Text = $"Second file not loaded | showing {_visibleIds.Length}";
                return;
            }

            string tile = _secondTile != null ? $"TileData: {_secondTile.FormatDescription}" : "no tiledata.mul";
            string text = $"{_second.SourceDescription}, max 0x{SecMaxId:X} | {tile}";

            if (_scanComplete)
            {
                int changed = 0, onlySec = 0, onlyOrg = 0;
                foreach (ItemDiffState s in _state)
                {
                    switch (s)
                    {
                        case ItemDiffState.Different: changed++; break;
                        case ItemDiffState.OnlySecond: onlySec++; break;
                        case ItemDiffState.OnlyOriginal: onlyOrg++; break;
                    }
                }
                text += $" | changed {changed}, new {onlySec}, only original {onlyOrg}";
            }
            else if (_scanCts == null)
            {
                text += " | comparison cancelled";
            }

            labelStatus.Text = $"{text} | showing {_visibleIds.Length}";
            toolTip1.SetToolTip(labelStatus, $"{_second.Directory}\n{labelStatus.Text}\n\nGray = same, Blue = changed, Green = new in second, Orange = only in original, Red = empty");
        }
        #endregion

        #region [ Load second ]
        private async void OnClickLoadSecond(object sender, EventArgs e)
        {
            await LoadSecondAsync(textBoxSecondDir.Text);
        }

        private async Task LoadSecondAsync(string directory)
        {
            directory = directory?.Trim().Trim('"');
            if (string.IsNullOrEmpty(directory))
            {
                MessageBox.Show("Please select the directory of the second client first.", "Load Second",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SecondArtFile file;
            SecondTileData tile = null;
            string tileError = null;

            Cursor = Cursors.WaitCursor;
            try
            {
                file = SecondArtFile.Load(directory);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "Load Second", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                tile = SecondTileData.Load(directory, file.GetMaxItemId() >= 0xFFDC);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                tileError = ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            CancelScan();
            SetPreview(pictureBoxSec, null);

            _second?.Dispose();
            _second = file;
            _secondTile = tile;

            textBoxSecondDir.Text = directory;
            SaveLastDirectory(directory);
            LoadComboBoxSaveDir();

            if (tile != null && TileData.ItemTable != null)
            {
                checkBoxCopyTileData.Checked = true;
            }

            ResetState();

            if (FilterNeedsCompare(CurrentFilter))
            {
                // the old filter needs the new comparison -> start it and filter afterwards
                RebuildListsUnfilteredWhileScanning();
                await ApplyFilterAsync();
            }
            else
            {
                RebuildLists();
                _ = StartScanAsync();
            }

            if (tileError != null)
            {
                MessageBox.Show($"tiledata.mul of the second client could not be read:\n{tileError}", "Load Second",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RebuildListsUnfilteredWhileScanning()
        {
            FilterMode mode = CurrentFilter;
            SetFilterSilently(FilterMode.All);
            RebuildLists();
            SetFilterSilently(mode);
        }

        private void TextBoxSecondDir_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OnClickLoadSecond(sender, e);
            }
        }

        private void TextBoxSecondDir_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = GetDroppedDirectory(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private async void TextBoxSecondDir_DragDrop(object sender, DragEventArgs e)
        {
            string directory = GetDroppedDirectory(e);
            if (directory != null)
            {
                textBoxSecondDir.Text = directory;
                await LoadSecondAsync(directory);
            }
        }

        private static string GetDroppedDirectory(DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is not string[] paths || paths.Length == 0)
            {
                return null;
            }

            string path = paths[0];
            if (Directory.Exists(path))
            {
                return path;
            }

            return File.Exists(path) ? Path.GetDirectoryName(path) : null;
        }
        #endregion

        #region [ Settings / recent directories ]
        private void EnsureSettingsDirectoryExists()
        {
            try
            {
                Directory.CreateDirectory(_settingsDirectory);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // settings are optional
            }
        }

        private string RecentFilePath => Path.Combine(_settingsDirectory, _lastDirectoriesFileName);

        private List<string> ReadRecentDirectories()
        {
            try
            {
                if (File.Exists(RecentFilePath))
                {
                    return File.ReadAllLines(RecentFilePath)
                        .Select(l => l.Trim())
                        .Where(l => l.Length > 0)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // ignore
            }
            return new List<string>();
        }

        private void WriteRecentDirectories(List<string> directories)
        {
            try
            {
                EnsureSettingsDirectoryExists();
                File.WriteAllLines(RecentFilePath, directories.Take(_maxRecentDirectories));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // ignore
            }
        }

        private void SaveLastDirectory(string directory)
        {
            List<string> list = ReadRecentDirectories();
            list.RemoveAll(d => string.Equals(d.TrimEnd('\\', '/'), directory.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase));
            list.Insert(0, directory);
            WriteRecentDirectories(list);

            try
            {
                EnsureSettingsDirectoryExists();
                File.WriteAllText(Path.Combine(_settingsDirectory, _settingsFileName), directory);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // ignore
            }
        }

        private void LoadComboBoxSaveDir()
        {
            List<string> list = ReadRecentDirectories();
            List<string> existing = list.Where(Directory.Exists).ToList();
            if (existing.Count != list.Count)
            {
                WriteRecentDirectories(existing); // remove directories that do not exist any more
            }

            comboBoxSaveDir.BeginUpdate();
            comboBoxSaveDir.Items.Clear();
            comboBoxSaveDir.Items.AddRange(existing.Cast<object>().ToArray());
            comboBoxSaveDir.EndUpdate();
        }

        private async void ComboBoxSaveDir_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxSaveDir.SelectedItem is string directory)
            {
                textBoxSecondDir.Text = directory;
                await LoadSecondAsync(directory);
            }
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the art files";
                dialog.ShowNewFolderButton = false;
                if (Directory.Exists(textBoxSecondDir.Text.Trim()))
                {
                    dialog.SelectedPath = textBoxSecondDir.Text.Trim();
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxSecondDir.Text = dialog.SelectedPath;
                    OnClickLoadSecond(sender, e);
                }
            }
        }

        private void BtDataDirectoryisSettings_Click(object sender, EventArgs e)
        {
            EnsureSettingsDirectoryExists();
            if (Directory.Exists(_settingsDirectory))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{_settingsDirectory}\"") { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show($"The directory does not exist:\n{_settingsDirectory}");
            }
        }
        #endregion

        #region [ Search ]
        private void OnClickSearch_Click(object sender, EventArgs e)
        {
            if (!ItemCompareHelper.TryParseId(searchTextBox.Text, comboBoxSearchMode.SelectedIndex != 1, out int id))
            {
                MessageBox.Show(comboBoxSearchMode.SelectedIndex == 1
                        ? "Invalid id. Please enter a decimal number (or a hex number with 0x)."
                        : "Invalid address. Please enter a valid hex address.",
                    "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            GoToId(id);
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OnClickSearch_Click(sender, e);
            }
        }

        /// <summary>
        /// Selects the id. When the current filter hides it, the user can switch to "All".
        /// </summary>
        private void GoToId(int id)
        {
            int max = _second == null ? OrgMaxId : MaxId;
            if (id > max)
            {
                MessageBox.Show($"Id 0x{id:X} is outside of the item range (max 0x{max:X}).", "Search",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (IndexOfId(id) < 0)
            {
                if (MessageBox.Show($"Id 0x{id:X} is hidden by the current filter.\nShow all items?", "Search",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                SetFilterSilently(FilterMode.All);
                RebuildLists();
            }

            SelectId(id);
            listBoxOrg.Focus();
        }

        private void OnClickNextFreeSlot(object sender, EventArgs e)
        {
            int start = Math.Max(0, GetFocusedId(listBoxOrg) + 1);
            int free = CopyItemsForm.FindFreeSlot(start, new HashSet<int>(), false);
            if (free < 0)
            {
                MessageBox.Show("No free slot found after the selected item.", "Free slot",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            GoToId(free);
        }
        #endregion

        #region [ Keys ]
        private void ListBoxSec_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left && !e.Control && !e.Shift)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                BtLeftMoveItemMore_Click(sender, e);
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SelectAll(listBoxSec);
            }
        }

        private void ListBoxOrg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                Btremoveitemfromindex_Click(sender, e);
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                SelectAll(listBoxOrg);
            }
        }

        private void SelectAll(ListBox listBox)
        {
            _syncingSelection = true;
            try
            {
                listBox.BeginUpdate();
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    listBox.SetSelected(i, true);
                }
                listBox.EndUpdate();
            }
            finally
            {
                _syncingSelection = false;
            }
            UpdateButtons();
        }
        #endregion

        #region [ Copy ]
        /// <summary>
        /// Copies the art (and optionally the tiledata) of the given mappings into the original.
        /// </summary>
        private int ApplyCopy(IEnumerable<CopyMapping> mappings)
        {
            int copied = 0;
            bool tileDataChanged = false;
            int orgMax = OrgMaxId;

            Cursor = Cursors.WaitCursor;
            try
            {
                foreach (CopyMapping m in mappings)
                {
                    Bitmap source = GetSec(m.SourceId);
                    if (source == null || m.TargetId < 0 || m.TargetId > orgMax)
                    {
                        continue;
                    }

                    Art.ReplaceStatic(m.TargetId, ItemCompareHelper.CloneArt(source));
                    ControlEvents.FireItemChangeEvent(this, m.TargetId);

                    if (m.TileData.HasValue && TileData.ItemTable != null && m.TargetId < TileData.ItemTable.Length)
                    {
                        TileData.ItemTable[m.TargetId] = m.TileData.Value;
                        ControlEvents.FireTileDataChangeEvent(this, m.TargetId + 0x4000);
                        tileDataChanged = true;
                    }

                    RefreshState(m.TargetId);
                    copied++;
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (copied > 0)
            {
                Options.ChangedUltimaClass["Art"] = true;
            }

            if (tileDataChanged)
            {
                Options.ChangedUltimaClass["TileData"] = true;
            }

            listBoxOrg.Invalidate();
            listBoxSec.Invalidate();
            UpdatePreviews();
            UpdateStatus();
            return copied;
        }

        /// <summary>
        /// Builds "same position" mappings; tiledata is taken from the second client when the checkbox is set.
        /// </summary>
        private List<CopyMapping> BuildSamePositionMappings(IEnumerable<int> sourceIds)
        {
            bool copyTile = checkBoxCopyTileData.Checked && _secondTile != null && TileData.ItemTable != null;
            List<CopyMapping> list = new List<CopyMapping>();
            foreach (int id in sourceIds)
            {
                if (GetSec(id) == null || id > OrgMaxId)
                {
                    continue;
                }

                list.Add(new CopyMapping
                {
                    SourceId = id,
                    TargetId = id,
                    IsValid = true,
                    Overwrites = IsOrgValid(id),
                    TileData = copyTile && _secondTile.HasItem(id) && id < TileData.ItemTable.Length
                        ? _secondTile.ItemTable[id]
                        : (ItemData?)null
                });
            }
            return list;
        }

        /// <summary>
        /// Copies the focused second item to the same position.
        /// </summary>
        private void BtLeftMoveItem_Click(object sender, EventArgs e)
        {
            int id = GetFocusedId(listBoxSec);
            if (id < 0 || _second == null)
            {
                return;
            }

            if (GetSec(id) == null)
            {
                return;
            }

            if (id > OrgMaxId)
            {
                MessageBox.Show($"0x{id:X} is outside of the range of the original file (max 0x{OrgMaxId:X}).\nUse \"Copy selected to...\" to choose another position.",
                    "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ApplyCopy(BuildSamePositionMappings(new[] { id }));
        }

        /// <summary>
        /// Copies all selected second items to the same positions.
        /// </summary>
        private void BtLeftMoveItemMore_Click(object sender, EventArgs e)
        {
            if (_second == null)
            {
                return;
            }

            List<int> ids = GetSelectedIds(listBoxSec);
            if (ids.Count == 0)
            {
                return;
            }

            if (ids.Count == 1)
            {
                BtLeftMoveItem_Click(sender, e);
                return;
            }

            List<CopyMapping> mappings = BuildSamePositionMappings(ids);
            if (mappings.Count == 0)
            {
                MessageBox.Show("None of the selected items can be copied (empty or out of range).", "Copy",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int overwrite = mappings.Count(m => m.Overwrites && GetState(m.SourceId) != ItemDiffState.Same);
            string text = $"Copy {mappings.Count} item(s) to the same position in the original?";
            if (overwrite > 0)
            {
                text += $"\n\n{overwrite} existing item(s) will be overwritten.";
            }
            if (mappings.Any(m => m.TileData.HasValue))
            {
                text += "\nTileData is copied too.";
            }

            if (MessageBox.Show(text, "Copy", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ApplyCopy(mappings);
            }
        }

        private void OnClickCopyTo(object sender, EventArgs e)
        {
            int start = GetFocusedId(listBoxOrg);
            ShowCopyDialog(GetSelectedIds(listBoxSec), CopyTargetMode.StartPosition, start);
        }

        private void OnClickCopyToFree(object sender, EventArgs e)
        {
            ShowCopyDialog(GetSelectedIds(listBoxSec), CopyTargetMode.FreeSlots, 0);
        }

        private async void OnClickCopyAllDifferences(object sender, EventArgs e)
        {
            if (_second == null)
            {
                return;
            }

            if (!_scanComplete)
            {
                if (_scanCts == null)
                {
                    _ = StartScanAsync();
                }

                await _scanTask;
                if (!_scanComplete)
                {
                    return;
                }
            }

            List<int> ids = new List<int>();
            for (int id = 0; id < _state.Length; ++id)
            {
                ItemDiffState state = _state[id];
                if (state == ItemDiffState.Different || state == ItemDiffState.OnlySecond)
                {
                    ids.Add(id);
                }
            }

            if (ids.Count == 0)
            {
                MessageBox.Show("There are no new or changed items in the second file.", "Copy",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ShowCopyDialog(ids, CopyTargetMode.SamePosition, 0);
        }

        private void ShowCopyDialog(List<int> sourceIds, CopyTargetMode mode, int startId)
        {
            if (_second == null || sourceIds.Count == 0)
            {
                return;
            }

            using (CopyItemsForm form = new CopyItemsForm(sourceIds, _second, _secondTile, mode, Math.Max(0, startId),
                       checkBoxCopyTileData.Checked))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                int copied = ApplyCopy(form.Result);
                int first = form.Result.Count > 0 ? form.Result[0].TargetId : -1;
                if (copied > 0 && first >= 0 && IndexOfId(first) >= 0)
                {
                    SelectId(first);
                }
            }
        }
        #endregion

        #region [ Replace graphic (context menu of the original list) ]
        private void ContextMenuStripOrg_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int target = GetFocusedId(listBoxOrg);
            bool hasOrgSelection = listBoxOrg.SelectedIndices.Count > 0;

            ClearReplaceMenu();

            List<int> secIds = _second != null ? GetSelectedIds(listBoxSec) : new List<int>();
            foreach (int id in secIds.Take(_maxReplaceMenuEntries))
            {
                Bitmap image = GetSec(id);
                if (image == null)
                {
                    continue;
                }

                Image thumbnail = CreateThumbnail(image, 48);
                _menuThumbnails.Add(thumbnail);

                ToolStripMenuItem item = new ToolStripMenuItem($"0x{id:X4} -> 0x{target:X4}", thumbnail)
                {
                    ImageScaling = ToolStripItemImageScaling.None,
                    Tag = id
                };
                item.Click += OnClickReplaceWith;
                replaceGraphicToolStripMenuItem.DropDownItems.Add(item);
            }

            replaceGraphicToolStripMenuItem.Enabled = target >= 0 && replaceGraphicToolStripMenuItem.DropDownItems.Count > 0;
            replaceGraphicToolStripMenuItem.Text = replaceGraphicToolStripMenuItem.Enabled
                ? "Replace graphic with"
                : "Replace graphic with (select items in the second list)";
            editTileDataOrgToolStripMenuItem.Enabled = hasOrgSelection && TileData.ItemTable != null;
            exportOrgToolStripMenuItem.Enabled = hasOrgSelection;
            removeOrgToolStripMenuItem.Enabled = hasOrgSelection;
        }

        private void ClearReplaceMenu()
        {
            foreach (ToolStripItem item in replaceGraphicToolStripMenuItem.DropDownItems)
            {
                item.Click -= OnClickReplaceWith;
            }
            replaceGraphicToolStripMenuItem.DropDownItems.Clear();

            foreach (Image image in _menuThumbnails)
            {
                image.Dispose();
            }
            _menuThumbnails.Clear();
        }

        private static Image CreateThumbnail(Image image, int maxSize)
        {
            float scale = Math.Min(1f, Math.Min((float)maxSize / image.Width, (float)maxSize / image.Height));
            int width = Math.Max(1, (int)(image.Width * scale));
            int height = Math.Max(1, (int)(image.Height * scale));
            Bitmap thumbnail = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(thumbnail))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return thumbnail;
        }

        private void OnClickReplaceWith(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem item || item.Tag is not int sourceId)
            {
                return;
            }

            int target = GetFocusedId(listBoxOrg);
            if (target < 0 || target > OrgMaxId)
            {
                return;
            }

            bool copyTile = checkBoxCopyTileData.Checked && _secondTile != null && _secondTile.HasItem(sourceId)
                && TileData.ItemTable != null && target < TileData.ItemTable.Length;

            ApplyCopy(new[]
            {
                new CopyMapping
                {
                    SourceId = sourceId,
                    TargetId = target,
                    IsValid = true,
                    Overwrites = IsOrgValid(target),
                    TileData = copyTile ? _secondTile.ItemTable[sourceId] : (ItemData?)null
                }
            });
        }
        #endregion

        #region [ Remove ]
        private void Btremoveitemfromindex_Click(object sender, EventArgs e)
        {
            List<int> ids = GetSelectedIds(listBoxOrg).Where(IsOrgValid).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            string what = ids.Count == 1 ? $"item 0x{ids[0]:X}" : $"{ids.Count} items";
            if (MessageBox.Show($"Remove {what} from the original art file?", "Remove",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            foreach (int id in ids)
            {
                Art.RemoveStatic(id);
                ControlEvents.FireItemChangeEvent(this, id);
                RefreshState(id);
            }

            Options.ChangedUltimaClass["Art"] = true;
            listBoxOrg.Invalidate();
            listBoxSec.Invalidate();
            UpdatePreviews();
            UpdateStatus();
        }
        #endregion

        #region [ TileData ]
        private void OnClickEditTileData(object sender, EventArgs e)
        {
            if (TileData.ItemTable == null)
            {
                MessageBox.Show("The tiledata.mul of the original is not loaded.", "TileData",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<int> ids = GetSelectedIds(listBoxOrg).Where(id => id < TileData.ItemTable.Length).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            if (ids.Count > 2000)
            {
                MessageBox.Show("Please select at most 2000 items.", "TileData", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<TileDataEditEntry> entries = ids.Select(id =>
            {
                bool hasSecond = _secondTile != null && _secondTile.HasItem(id);
                return new TileDataEditEntry
                {
                    SourceId = hasSecond ? id : -1,
                    TargetId = id,
                    HasOriginal = true,
                    Original = TileData.ItemTable[id],
                    HasSecond = hasSecond,
                    Second = hasSecond ? _secondTile.ItemTable[id] : default,
                    Edited = TileData.ItemTable[id],
                    Image = GetOrg(id) ?? GetSec(id)
                };
            }).ToList();

            using (TileDataEditForm form = new TileDataEditForm(entries, "Edit TileData of the original", Art.IsUOAHS()))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
            }

            int changed = 0;
            foreach (TileDataEditEntry entry in entries)
            {
                if (ItemCompareHelper.ItemDataEquals(entry.Original, entry.Edited))
                {
                    continue;
                }

                TileData.ItemTable[entry.TargetId] = entry.Edited;
                ControlEvents.FireTileDataChangeEvent(this, entry.TargetId + 0x4000);
                changed++;
            }

            if (changed > 0)
            {
                Options.ChangedUltimaClass["TileData"] = true;
                listBoxOrg.Invalidate();
                UpdatePreviews();
            }
        }

        private void OnClickSaveTileData(object sender, EventArgs e)
        {
            if (TileData.ItemTable == null)
            {
                return;
            }

            string fileName = Path.Combine(Options.OutputPath, "tiledata.mul");
            if (MessageBox.Show($"Save TileData to\n{fileName}?", "Save TileData",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                Directory.CreateDirectory(Options.OutputPath);
                TileData.SaveTileData(fileName);
                Options.ChangedUltimaClass["TileData"] = false;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show($"TileData could not be saved:\n{ex.Message}", "Save TileData", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            MessageBox.Show($"TileData saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnClickSaveArt(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Save art.mul and artidx.mul to\n{Options.OutputPath}?\n\nThis can take a while.", "Save Art",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                Directory.CreateDirectory(Options.OutputPath);
                Art.Save(Options.OutputPath);
                Options.ChangedUltimaClass["Art"] = false;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show($"Art could not be saved:\n{ex.Message}", "Save Art", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            MessageBox.Show($"Art saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region [ Export ]
        private void OnClickExportSec(object sender, EventArgs e)
        {
            if (_second == null)
            {
                return;
            }

            ExportImages(GetSelectedIds(listBoxSec), GetSec, "Item(Sec)", (sender as ToolStripItem)?.Tag as string);
        }

        private void OnClickExportOrg(object sender, EventArgs e)
        {
            ExportImages(GetSelectedIds(listBoxOrg), GetOrg, "Item", (sender as ToolStripItem)?.Tag as string);
        }

        /// <summary>
        /// Exports every item from the lowest to the highest selected id of the second list.
        /// </summary>
        private void OnClickExportRange(object sender, EventArgs e)
        {
            if (_second == null)
            {
                return;
            }

            List<int> ids = GetSelectedIds(listBoxSec);
            if (ids.Count < 2)
            {
                MessageBox.Show("Please select at least 2 items (Ctrl or Shift + click).", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            IEnumerable<int> range = Enumerable.Range(ids[0], ids[ids.Count - 1] - ids[0] + 1);
            ExportImages(range, GetSec, "Item(Sec)", (sender as ToolStripItem)?.Tag as string);
        }

        private void ExportImages(IEnumerable<int> ids, Func<int, Bitmap> getImage, string prefix, string format)
        {
            format = string.IsNullOrEmpty(format) ? "bmp" : format;
            string path = Options.OutputPath;
            int count = 0;

            try
            {
                Cursor = Cursors.WaitCursor;
                Directory.CreateDirectory(path);

                foreach (int id in ids)
                {
                    Bitmap image = getImage(id);
                    if (image == null)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(path, $"{prefix} 0x{id:X}.{format}");
                    switch (format)
                    {
                        case "png":
                            using (Bitmap copy = new Bitmap(image))
                            {
                                copy.Save(fileName, ImageFormat.Png);
                            }
                            break;
                        case "tiff":
                            image.Save(fileName, ImageFormat.Tiff);
                            break;
                        default:
                            image.Save(fileName, ImageFormat.Bmp);
                            break;
                    }
                    count++;
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Runtime.InteropServices.ExternalException)
            {
                MessageBox.Show($"Export failed:\n{ex.Message}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            MessageBox.Show(count == 0 ? "Nothing to export (the selected items are empty)." : $"{count} image(s) saved to {path}",
                "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion
    }
}
