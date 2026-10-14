// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Core;

namespace UoFiddler.Controls.ArtLibrary.Forms
{
    /// <summary>
    /// Dialog "Bibliothek erweitern": asks for the new number of ids.
    /// </summary>
    public partial class ArtLibraryGrowForm : Form
    {
        private int _currentCapacity;

        public ArtLibraryGrowForm()
        {
            InitializeComponent();
        }

        public int CurrentCapacity
        {
            get => _currentCapacity;
            set
            {
                _currentCapacity = value;
                labelCurrent.Text = $"Aktuell: {value:N0} IDs (0 bis 0x{Math.Max(0, value - 1):X})";
                numericUpDownCapacity.Minimum = Math.Min(value + 1, ArtLibraryFile.MaxCapacity);
                numericUpDownCapacity.Maximum = ArtLibraryFile.MaxCapacity;
                numericUpDownCapacity.Value = Math.Min((long)value * 2, ArtLibraryFile.MaxCapacity);
                UpdateInfo();
            }
        }

        public int NewCapacity => (int)numericUpDownCapacity.Value;

        private void NumericUpDownCapacity_ValueChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        private void UpdateInfo()
        {
            long idxBytes = ArtLibraryHeader.Size + ((long)NewCapacity * ArtLibraryEntry.Size);
            labelInfo.Text = $"Neu: 0 bis 0x{NewCapacity - 1:X}  -  Indexdatei ca. {idxBytes / (1024.0 * 1024.0):N1} MB";
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            if (NewCapacity <= _currentCapacity)
            {
                MessageBox.Show(this, "Die neue Anzahl muss größer sein als die aktuelle.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
