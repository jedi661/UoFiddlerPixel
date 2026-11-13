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
    /// Dialog of the Animations-Studio: splits a sprite sheet into columns (frames) and rows (directions)
    /// and chooses the row order.
    /// </summary>
    public partial class AnimationSheetImportForm : Form
    {
        /// <summary>Designer constructor.</summary>
        public AnimationSheetImportForm()
        {
            InitializeComponent();
            viewSheet.PaintOverlay += ViewSheet_PaintOverlay;
            cbMapping.SelectedIndex = 0;
        }

        public AnimationSheetImportForm(Bitmap sheet) : this()
        {
            // guess rows / columns: square cells
            int guessRows = sheet.Height >= sheet.Width / 2 ? 5 : 1;
            numRows.Value = guessRows;
            int cell = Math.Max(1, sheet.Height / guessRows);
            numColumns.Value = Math.Max(1, Math.Min(200, sheet.Width / cell));

            viewSheet.Zoom = Math.Max(1, 700 / Math.Max(1, sheet.Width));
            viewSheet.Image = sheet;
        }

        public int Columns => (int)numColumns.Value;
        public int Rows => (int)numRows.Value;
        public int Mapping => cbMapping.SelectedIndex;
        public bool CropUnion => chkCropUnion.Checked;
        public bool RemoveBackground => chkRemoveBackground.Checked;

        private void Grid_Changed(object sender, EventArgs e)
        {
            viewSheet.Invalidate();
        }

        private void CbMapping_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMapping.SelectedIndex is 2 or 3)
            {
                numRows.Value = 8;
            }
            else if (cbMapping.SelectedIndex is 0 or 1)
            {
                numRows.Value = 5;
            }
        }

        private void ViewSheet_PaintOverlay(Graphics g, Rectangle r)
        {
            using var pen = new Pen(Color.FromArgb(180, Color.Red));
            float cw = r.Width / (float)numColumns.Value, ch = r.Height / (float)numRows.Value;
            for (int i = 1; i < numColumns.Value; i++)
            {
                g.DrawLine(pen, r.X + (i * cw), r.Y, r.X + (i * cw), r.Bottom);
            }
            for (int i = 1; i < numRows.Value; i++)
            {
                g.DrawLine(pen, r.X, r.Y + (i * ch), r.Right, r.Y + (i * ch));
            }
        }
    }
}
