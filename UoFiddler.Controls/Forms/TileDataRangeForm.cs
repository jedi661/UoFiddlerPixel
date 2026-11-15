/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * TileData – select tiles by ID ranges ("0x0B2C-0x0B40, 0x0C00").
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class TileDataRangeForm : Form
    {
        private readonly int _max;

        public List<int> Ids { get; private set; } = new List<int>();

        public bool AddToSelection => _chkAdd.Checked;

        public TileDataRangeForm()
            : this(0x10000, null)
        {
        }

        public TileDataRangeForm(int max, string preset)
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            _max = max;
            Icon = Options.GetFiddlerIcon();
            _txtRange.Text = preset ?? string.Empty;
            OnRangeChanged(this, EventArgs.Empty);
        }

        private void OnRangeChanged(object sender, EventArgs e)
        {
            if (TileDataBatch.TryParseRanges(_txtRange.Text, _max, out List<int> ids, out string error))
            {
                Ids = ids;
                _lblResult.Text = ids.Count == 0 ? "no id" : $"{ids.Count} tile(s)";
                _btnApply.Enabled = ids.Count > 0;
            }
            else
            {
                _lblResult.Text = error;
                _btnApply.Enabled = false;
            }
        }

        private void OnClickOk(object sender, EventArgs e)
        {
            if (Ids.Count > 0)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
