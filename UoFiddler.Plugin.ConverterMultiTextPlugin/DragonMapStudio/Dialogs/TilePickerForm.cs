// /***************************************************************************
//  *
//  * Dragon Map Studio – big picture catalogue: land tiles, textures, items
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public enum TileCatalogMode
    {
        Land,
        Textures,
        Items
    }

    /// <summary>Resizable catalogue with big pictures; double click or OK takes the tile.</summary>
    public partial class TilePickerForm : Form
    {
        private readonly List<int> _land;
        private readonly List<int> _items;
        private List<int> _withTexture;

        public TilePickerForm()
            : this(new List<int>(), new List<int>(), TileCatalogMode.Land, -1)
        {
        }

        public TilePickerForm(List<int> land, List<int> items, TileCatalogMode mode, int selected)
        {
            InitializeComponent();
            _land = land ?? new List<int>();
            _items = items ?? new List<int>();
            cmbMode.Items.AddRange(new object[] { "Land-Tiles", "Texturen (Land mit Textur)", "Items / Statics" });
            cmbSize.Items.AddRange(new object[] { "klein", "mittel", "groß", "sehr groß" });
            cmbSize.SelectedIndex = 2;
            grid.NameOf = NameOf;
            grid.ImageOf = ImageOf;
            cmbMode.SelectedIndex = (int)mode;
            grid.SelectedId = selected;
            UpdateInfo();
        }

        public TileCatalogMode Mode => (TileCatalogMode)Math.Max(0, cmbMode.SelectedIndex);

        public int SelectedId => grid.SelectedId;

        private Image ImageOf(int id)
        {
            switch (Mode)
            {
                case TileCatalogMode.Items:
                    return UoArtCache.Static(id, 0);
                case TileCatalogMode.Textures:
                    return UoArtCache.Texture(id);
                default:
                    return UoArtCache.Land(id);
            }
        }

        private string NameOf(int id) => Mode == TileCatalogMode.Items ? UoArtCache.ItemName(id) : UoArtCache.LandName(id);

        private void ApplyFilter()
        {
            List<int> source;
            switch (Mode)
            {
                case TileCatalogMode.Items:
                    source = _items;
                    break;
                case TileCatalogMode.Textures:
                    // only land tiles that have a texture (searched once)
                    source = _withTexture ??= _land.Where(id => UoArtCache.Texture(id) != null).ToList();
                    break;
                default:
                    source = _land;
                    break;
            }
            string f = txtFilter.Text.Trim();
            string hex = f.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? f.Substring(2) : f;
            bool isHex = HexList.TryParseHex(hex, out _);
            grid.Ids = f.Length == 0
                ? source
                : source.Where(id => (isHex && id.ToString("x4").Contains(hex, StringComparison.OrdinalIgnoreCase)) ||
                                     NameOf(id).Contains(f, StringComparison.OrdinalIgnoreCase)).ToList();
            lblCount.Text = $"{grid.Ids.Count:N0} Einträge";
        }

        private void UpdateInfo()
        {
            int id = grid.SelectedId;
            if (id < 0)
            {
                lblInfo.Text = "Bild anklicken = auswählen, Doppelklick oder OK = übernehmen.";
                picPreview.Image = null;
                return;
            }
            picPreview.Image = ImageOf(id);
            lblInfo.Text = Mode == TileCatalogMode.Items
                ? $"{id:x4}  {UoArtCache.ItemName(id)}{Environment.NewLine}Höhe {UoArtCache.ItemHeight(id)}  ·  TileData: {UoArtCache.ItemFlagsText(id)}"
                : $"{id:x4}  {UoArtCache.LandName(id)}{Environment.NewLine}{(UoArtCache.Texture(id) != null ? "mit Textur (für Hänge)" : "ohne Textur")}" +
                  (UoArtCache.LandIsWet(id) ? "  ·  nass" : string.Empty);
        }

        private void CmbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                ApplyFilter();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            UpdateInfo();
        }

        private void CmbSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            int[] sizes = { 48, 64, 96, 140 };
            grid.CellSize = sizes[Math.Max(0, cmbSize.SelectedIndex)];
        }

        private void TxtFilter_TextChanged(object sender, EventArgs e) => ApplyFilter();

        private void Grid_SelectedIdChanged(object sender, EventArgs e) => UpdateInfo();

        private void Grid_ItemActivated(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
