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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Analysis;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Searches an item, gump, hue or multi id (or a name) through the classic client and the whole Enhanced Client;
    /// a double click shows the result in its tab.
    /// </summary>
    public sealed partial class EcSearchControl : UserControl, IEcEditor
    {
        private EcClient _client;
        private EcItemSprites _sprites;
        private CancellationTokenSource _cancel;
        private List<EcSearchHit> _hits = new List<EcSearchHit>();

        /// <summary>Raised when a result should be shown in its tab.</summary>
        public event Action<EcNavigation> NavigateRequested;

        public EcSearchControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            toolStripComboBoxKind.SelectedIndex = 0;
            Disposed += (s, e) =>
            {
                pictureBoxPreview.Image = null;
                _sprites?.Dispose();
            };
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            pictureBoxPreview.Image = null; // the image belongs to the sprite cache that is released now
            _sprites?.Dispose();
            _sprites = client == null ? null : new EcItemSprites(client);
            listViewResults.Items.Clear();
            _hits.Clear();
        }

        public void OnActivated()
        {
            toolStripTextBoxSearch.Focus();
        }

        /// <summary>Starts a search from outside (e.g. "search this item" of another tab).</summary>
        public void SearchFor(EcSearchKind kind, int id)
        {
            toolStripComboBoxKind.SelectedIndex = (int)kind;
            toolStripTextBoxSearch.Text = $"0x{id:X4}";
            Search();
        }

        // ==================================================================== classic lookup

        private static EcClassicLookup ClassicLookup() => new EcClassicLookup
        {
            ItemInfo = id =>
            {
                try
                {
                    Ultima.ItemData[] table = Ultima.TileData.ItemTable;
                    bool art = Ultima.Art.IsValidStatic(id);
                    if (table == null || id >= table.Length)
                    {
                        return art ? "art only (no tiledata)" : null;
                    }
                    Ultima.ItemData d = table[id];
                    if (!art && string.IsNullOrEmpty(d.Name))
                    {
                        return null;
                    }
                    return $"\"{d.Name}\"  flags {d.Flags}  weight {d.Weight}  height {d.Height}" + (art ? string.Empty : "  (no art)");
                }
                catch (Exception)
                {
                    return null;
                }
            },
            GumpInfo = id =>
            {
                try
                {
                    return Ultima.Gumps.IsValidIndex(id) ? "present" : null;
                }
                catch (Exception)
                {
                    return null;
                }
            },
            HueInfo = id =>
            {
                try
                {
                    return id >= 1 && id <= Ultima.Hues.List.Length ? $"\"{Ultima.Hues.List[id - 1].Name}\"" : null;
                }
                catch (Exception)
                {
                    return null;
                }
            },
            ItemsByName = text =>
            {
                var list = new List<(int, string)>();
                try
                {
                    Ultima.ItemData[] table = Ultima.TileData.ItemTable;
                    for (int i = 0; table != null && i < table.Length; i++)
                    {
                        if (table[i].Name != null && table[i].Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            list.Add((i, table[i].Name));
                        }
                    }
                }
                catch (Exception)
                {
                    // no classic client
                }
                return list;
            }
        };

        // ==================================================================== search

        private async void Search()
        {
            if (_client == null)
            {
                EcDialogs.ShowError(this, "Choose the Enhanced Client folder on top first.");
                return;
            }
            var kind = (EcSearchKind)Math.Max(0, toolStripComboBoxKind.SelectedIndex);
            string text = toolStripTextBoxSearch.Text;
            bool maps = toolStripButtonMaps.Checked;
            _cancel = new CancellationTokenSource();
            CancellationToken token = _cancel.Token;
            var progress = new Progress<int>(p => toolStripProgressBar.Value = Math.Max(0, Math.Min(100, p)));
            SetBusy(true);
            toolStripStatusLabel.Text = "Searching...";
            try
            {
                var search = new EcSearch(_client, ClassicLookup());
                _hits = await Task.Run(() => search.Run(kind, text, maps, progress, token));
                ShowHits();
                toolStripStatusLabel.Text = $"{_hits.Count} results.";
            }
            catch (OperationCanceledException)
            {
                toolStripStatusLabel.Text = "Cancelled.";
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = "Error: " + ex.Message;
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ShowHits()
        {
            listViewResults.BeginUpdate();
            listViewResults.Items.Clear();
            foreach (EcSearchHit hit in _hits)
            {
                var item = new ListViewItem(hit.Area) { Tag = hit };
                item.SubItems.Add(hit.Where);
                item.SubItems.Add(hit.Detail);
                if (hit.Detail != null && (hit.Detail.Contains("MISSING") || hit.Detail.StartsWith("no ", StringComparison.Ordinal) || hit.Detail.StartsWith("not ", StringComparison.Ordinal)))
                {
                    item.ForeColor = Color.Firebrick;
                }
                else if (hit.Go != null)
                {
                    item.ForeColor = Color.DarkBlue;
                }
                listViewResults.Items.Add(item);
            }
            listViewResults.EndUpdate();
        }

        private EcSearchHit SelectedHit => listViewResults.SelectedItems.Count == 1 ? listViewResults.SelectedItems[0].Tag as EcSearchHit : null;

        private void ShowPreview()
        {
            EcSearchHit hit = SelectedHit;
            pictureBoxPreview.Image = null;
            if (hit == null)
            {
                return;
            }
            labelPreview.Text = $"{hit.Area}: {hit.Where}\r\n{hit.Detail}" + (hit.Go != null ? "\r\nDouble click: show in its tab" : string.Empty);
            if (hit.Go != null && _sprites != null && (hit.Go.Target == EcTarget.Item || hit.Go.Target == EcTarget.TileArt || hit.Go.Target == EcTarget.AnimDefinition))
            {
                pictureBoxPreview.Image = _sprites.Get((uint)hit.Go.Id, false).Image;
            }
        }

        private void GoTo()
        {
            EcSearchHit hit = SelectedHit;
            if (hit?.Go != null)
            {
                NavigateRequested?.Invoke(hit.Go);
            }
        }

        private void SetBusy(bool busy)
        {
            toolStripButtonSearch.Enabled = !busy;
            toolStripButtonCancel.Enabled = busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            if (!busy)
            {
                toolStripProgressBar.Value = 0;
            }
        }

        private void Export()
        {
            if (_hits.Count == 0)
            {
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = "Text|*.txt|CSV|*.csv", FileName = "search.txt" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                bool csv = dialog.FilterIndex == 2;
                var sb = new StringBuilder();
                foreach (EcSearchHit h in _hits)
                {
                    sb.AppendLine(csv ? $"\"{h.Area}\";\"{h.Where}\";\"{h.Detail?.Replace("\"", "'")}\"" : $"{h.Area,-16} {h.Where,-32} {h.Detail}");
                }
                File.WriteAllText(dialog.FileName, sb.ToString());
            }
        }

        // ==================================================================== event handlers (designer)

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Search();
            }
        }

        private void OnClickSearch(object sender, EventArgs e) => Search();

        private void OnClickCancel(object sender, EventArgs e) => _cancel?.Cancel();

        private void OnClickGoTo(object sender, EventArgs e) => GoTo();

        private void OnResultSelected(object sender, EventArgs e) => ShowPreview();

        private void OnClickCopy(object sender, EventArgs e)
        {
            EcSearchHit hit = SelectedHit;
            if (hit != null)
            {
                Clipboard.SetText($"{hit.Area}\t{hit.Where}\t{hit.Detail}");
            }
        }

        private void OnClickExport(object sender, EventArgs e) => Export();
    }
}
