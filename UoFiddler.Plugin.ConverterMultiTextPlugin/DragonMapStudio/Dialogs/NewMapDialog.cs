// /***************************************************************************
//  *
//  * Dragon Map Studio – "new map" dialog
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public partial class NewMapDialog : Form
    {
        private readonly DragonRuleSet _rules;

        public NewMapDialog() : this(null, 6144, 4096, 0x20)
        {
        }

        public NewMapDialog(DragonRuleSet rules, int width, int height, int fillIndex)
        {
            InitializeComponent();
            UiTranslator.Apply(this);
            _rules = rules;
            cmbPreset.Items.Add(L.T("Eigene Größe"));
            foreach (MapSizePreset p in MapSizePreset.All)
            {
                cmbPreset.Items.Add(p);
            }
            cmbPreset.SelectedIndex = 0;
            numWidth.Value = Math.Clamp(width, 8, 16384);
            numHeight.Value = Math.Clamp(height, 8, 16384);
            chkBits16.Checked = rules != null && rules.ColorBits == 16;
            numFill.Maximum = chkBits16.Checked ? Math.Max(255, (rules?.Palette.Count ?? 256) - 1) : 255;
            numFill.Value = Math.Clamp(fillIndex, 0, (int)numFill.Maximum);
            UpdateFillPreview();
        }

        public string MapName => string.IsNullOrWhiteSpace(txtName.Text) ? "NeueKarte" : txtName.Text.Trim();

        public int MapWidth => ((int)numWidth.Value + 7) / 8 * 8;

        public int MapHeight => ((int)numHeight.Value + 7) / 8 * 8;

        public ushort FillIndex => (ushort)numFill.Value;

        /// <summary>8 = classic Dragon (BMP, 256 colours), 16 = extended (PNG, up to 65 536 colours).</summary>
        public int Bits => chkBits16.Checked ? 16 : 8;

        private void ChkBits16_CheckedChanged(object sender, EventArgs e)
        {
            numFill.Maximum = chkBits16.Checked ? Math.Max(255, (_rules?.Palette.Count ?? 256) - 1) : 255;
            UpdateFillPreview();
        }

        private void CmbPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPreset.SelectedItem is MapSizePreset p)
            {
                numWidth.Value = p.Width;
                numHeight.Value = p.Height;
            }
        }

        private void NumFill_ValueChanged(object sender, EventArgs e) => UpdateFillPreview();

        private void UpdateFillPreview()
        {
            if (_rules == null)
            {
                return;
            }
            DragonPaletteEntry entry = _rules.Palette[Math.Min((int)numFill.Value, _rules.Palette.Count - 1)];
            panelFillSwatch.BackColor = entry.Color;
            lblFillName.Text = entry.Name;
        }
    }
}
