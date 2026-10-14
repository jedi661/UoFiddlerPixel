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
using System.IO;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Core;

namespace UoFiddler.Controls.ArtLibrary.Forms
{
    /// <summary>
    /// Dialog "Neue Bibliothek": asks for name, folder and number of ids.
    /// The library itself is created by the caller (ArtLibraryForm) with <see cref="ArtLibraryFile.Create"/>.
    /// </summary>
    public partial class ArtLibraryNewForm : Form
    {
        public ArtLibraryNewForm()
        {
            InitializeComponent();

            numericUpDownCapacity.Minimum = 1;
            numericUpDownCapacity.Maximum = ArtLibraryFile.MaxCapacity;
            numericUpDownCapacity.Value = ArtLibraryFile.DefaultCapacity;

            UpdateSizeInfo();
        }

        /// <summary>File name of the new library (without extension).</summary>
        public string LibraryName
        {
            get => textBoxName.Text.Trim();
            set => textBoxName.Text = value ?? string.Empty;
        }

        /// <summary>Target folder.</summary>
        public string LibraryDirectory
        {
            get => textBoxDirectory.Text.Trim();
            set => textBoxDirectory.Text = value ?? string.Empty;
        }

        /// <summary>Number of ids.</summary>
        public int Capacity
        {
            get => (int)numericUpDownCapacity.Value;
            set => numericUpDownCapacity.Value = Math.Max(numericUpDownCapacity.Minimum, Math.Min(numericUpDownCapacity.Maximum, value));
        }

        /// <summary>Full base path of the library that will be created.</summary>
        public string BasePath => Path.Combine(LibraryDirectory, LibraryName);

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Ordner für die neue Bibliothek wählen";
                dialog.UseDescriptionForTitle = true;
                if (Directory.Exists(LibraryDirectory))
                {
                    dialog.SelectedPath = LibraryDirectory;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    LibraryDirectory = dialog.SelectedPath;
                }
            }
        }

        private void NumericUpDownCapacity_ValueChanged(object sender, EventArgs e)
        {
            UpdateSizeInfo();
        }

        private void UpdateSizeInfo()
        {
            long idxBytes = ArtLibraryHeader.Size + ((long)Capacity * ArtLibraryEntry.Size);
            labelSizeInfo.Text = $"IDs 0 bis {Capacity - 1:N0} (0x{Capacity - 1:X})  -  Indexdatei ca. {idxBytes / (1024.0 * 1024.0):N1} MB";
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            if (!ArtLibraryFile.IsValidFileName(LibraryName))
            {
                MessageBox.Show(this, "Bitte einen gültigen Namen eingeben (keine Zeichen wie \\ / : * ? \" < > |).",
                    "Neue Bibliothek", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(LibraryDirectory))
            {
                MessageBox.Show(this, "Bitte einen Ordner wählen.", "Neue Bibliothek", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ArtLibraryFile.Exists(LibraryDirectory, LibraryName))
            {
                MessageBox.Show(this, $"Im Ordner gibt es bereits eine Bibliothek \"{LibraryName}\".",
                    "Neue Bibliothek", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxName.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
