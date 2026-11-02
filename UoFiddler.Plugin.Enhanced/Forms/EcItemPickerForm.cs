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
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;

namespace UoFiddler.Plugin.Enhanced.Forms
{
    /// <summary>Finds an item (id or name) with its EC image – used to place items on the map.</summary>
    public sealed partial class EcItemPickerForm : Form
    {
        private readonly EcClient _client;
        private readonly EcItemSprites _sprites;
        private readonly List<(uint Id, string Name)> _all = new List<(uint, string)>();
        private List<(uint Id, string Name)> _view = new List<(uint, string)>();

        /// <summary>The chosen item id (valid after OK).</summary>
        public uint SelectedId { get; private set; }

        /// <summary>Designer only.</summary>
        public EcItemPickerForm()
        {
            InitializeComponent();
        }

        public EcItemPickerForm(EcClient client, EcItemSprites sprites, uint startId)
        {
            InitializeComponent();
            _client = client;
            _sprites = sprites;
            TileArtFile tileArt = client.GetTileArt();
            if (tileArt != null)
            {
                foreach (TileArtRecord r in tileArt.Records.Where(r => r.Id < 0x40000))
                {
                    string name = client.HasStringDictionary ? client.StringDictionary.Get(r.Entry.NameIndex) : null;
                    _all.Add((r.Id, name ?? string.Empty));
                }
            }
            toolStripTextBoxSearch.Text = startId > 0 ? $"0x{startId:X4}" : string.Empty;
            Shown += (s, e) =>
            {
                ApplySearch();
                toolStripTextBoxSearch.Focus();
            };
        }

        private void ApplySearch()
        {
            string text = toolStripTextBoxSearch.Text.Trim();
            bool onlyEc = toolStripButtonOnlyEc.Checked;
            IEnumerable<(uint Id, string Name)> rows = _all;
            if (onlyEc)
            {
                rows = rows.Where(r => _sprites.PredictKind(r.Id) is EcSpriteKind.WorldArt or EcSpriteKind.WorldArtAtlas);
            }
            int jump = -1;
            if (text.Length > 0 && EcDialogs.TryParseNumber(text, out long id))
            {
                _view = rows.ToList();
                jump = _view.FindIndex(r => r.Id >= id);
            }
            else
            {
                _view = text.Length == 0 ? rows.ToList() : rows.Where(r => r.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            listViewItems.VirtualListSize = _view.Count;
            listViewItems.Invalidate();
            if (jump >= 0)
            {
                listViewItems.SelectedIndices.Clear();
                listViewItems.SelectedIndices.Add(jump);
                listViewItems.EnsureVisible(jump);
            }
        }

        private void ShowSelected()
        {
            if (listViewItems.SelectedIndices.Count != 1 || listViewItems.SelectedIndices[0] >= _view.Count)
            {
                return;
            }
            (uint id, string name) = _view[listViewItems.SelectedIndices[0]];
            EcItemSprite sprite = _sprites.Get(id, false);
            pictureBoxPreview.Image = sprite.Image;
            labelInfo.Text = $"0x{id:X4} ({id}) {name}\r\n{EcItemSprites.KindText(sprite)}  height {sprite.Height}";
            SelectedId = id;
        }

        // ==================================================================== event handlers (designer)

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplySearch();
            }
        }

        private void OnClickSearch(object sender, EventArgs e) => ApplySearch();

        private void RetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }
            (uint id, string name) = _view[e.ItemIndex];
            var item = new ListViewItem(id.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add($"0x{id:X4}");
            item.SubItems.Add(name);
            e.Item = item;
        }

        private void OnSelectionChanged(object sender, EventArgs e) => ShowSelected();

        private void OnClickOk(object sender, EventArgs e)
        {
            if (listViewItems.SelectedIndices.Count == 1)
            {
                ShowSelected();
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
