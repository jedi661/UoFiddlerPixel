/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Dialog of the Spell- und Effekt-Editor: number of columns and rows of a sprite sheet.
    /// </summary>
    public partial class EffectSheetSplitForm : Form
    {
        /// <summary>Designer constructor.</summary>
        public EffectSheetSplitForm()
        {
            InitializeComponent();
        }

        public EffectSheetSplitForm(Bitmap sheet) : this()
        {
            // guess square cells
            int cell = Math.Max(1, Math.Min(sheet.Width, sheet.Height));
            numColumns.Value = Math.Max(1, Math.Min(numColumns.Maximum, sheet.Width / cell));
            numRows.Value = Math.Max(1, Math.Min(numRows.Maximum, sheet.Height / cell));
            lblSheetSize.Text = $"Sheet: {sheet.Width}x{sheet.Height}";
        }

        public int Columns => (int)numColumns.Value;
        public int Rows => (int)numRows.Value;
    }
}
