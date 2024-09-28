/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 * 
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Windows.Forms;
using UoFiddler.Plugin.UopPacker.Classes;

namespace UoFiddler.Plugin.UopPacker.UserControls
{
    #region [ class UopPackerControl ]
    public partial class UopPackerControl : UserControl
    {
        private readonly LegacyMulFileConverter _conv;
        private int _total;
        private int _success;

        #region [ UopPackerControl ]
        private UopPackerControl()
        {
            InitializeComponent();

            _conv = new LegacyMulFileConverter();

            multype.DataSource = uoptype.DataSource = Enum.GetValues(typeof(FileType));
            mulMapIndex.ReadOnly = uopMapIndex.ReadOnly = true;

            Dock = DockStyle.Fill;

            checkBoxOverwriteSaveUop.Checked = Properties.Settings.Default.OverwriteSaveUop;

        }
        #endregion

        #region UopPackerControl
        public UopPackerControl(string version) : this()
        {
            VersionLabel.Text = version;
        }
        #endregion

        #region InputMulSelect
        private void InputMulSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 1;
            FileDialog.FileName = "art.mul";

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                inmul.Text = FileDialog.FileName;

                if (CheckBoxAutoFill.Checked) // Autofill
                {
                    AutoFillTextBoxes();
                }
            }
        }
        #endregion

        #region InputIdxSelect
        private void InputIdxSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 3;
            FileDialog.FileName = "artidx.mul";
            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                inidx.Text = FileDialog.FileName;
            }
        }
        #endregion

        #region OutputUopSelect
        private void OutputUopSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 2;
            FileDialog.FileName = "artLegacyMUL.uop";

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                outuop.Text = FileDialog.FileName;
            }
        }
        #endregion

        #region ToUop
        private void ToUop(object sender, EventArgs e)
        {
            var selectedFileType = multype?.SelectedValue?.ToString() ?? string.Empty;
            if (!Enum.TryParse(selectedFileType, out FileType fileType))
            {
                MessageBox.Show("You must specify input type");
                return;
            }

            if (inmul.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input mul");
                return;
            }

            if (inidx.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input idx");
                return;
            }

            if (outuop.Text.Length == 0)
            {
                MessageBox.Show("You must specify the output uop");
                return;
            }

            if (!File.Exists(inmul.Text))
            {
                MessageBox.Show("The input mul does not exists");
                return;
            }

            if (!File.Exists(inidx.Text))
            {
                MessageBox.Show("The input idx does not exists");
                return;
            }

            /*if (File.Exists(outuop.Text))
            {
                MessageBox.Show("Output file already exists");
                return;
            }*/

            if (File.Exists(outuop.Text) && !checkBoxOverwriteSaveUop.Checked)
            {
                MessageBox.Show("Output file already exists");
                return;
            }


            try
            {
                multouop.Text = "Converting...";
                multouop.Enabled = false;

                LegacyMulFileConverter.ToUop(inmul.Text, inidx.Text, outuop.Text, fileType, (int)mulMapIndex.Value);
            }
            catch
            {
                MessageBox.Show("An error occurred");
            }
            finally
            {
                multouop.Text = "Convert";
                multouop.Enabled = true;
            }
        }
        #endregion

        #region OutMulSelect
        private void OutMulSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 1;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                outmul.Text = FileDialog.FileName;
            }
        }
        #endregion

        #region  OutputIdxSelect
        private void OutputIdxSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 3;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                outidx.Text = FileDialog.FileName;
            }
        }
        #endregion

        #region InputUopSelect
        private void InputUopSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 2;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                inuop.Text = FileDialog.FileName;
            }
        }
        #endregion

        #region ToMul
        private void ToMul(object sender, EventArgs e)
        {
            var selectedFileType = uoptype?.SelectedValue?.ToString() ?? string.Empty;
            if (!Enum.TryParse(selectedFileType, out FileType fileType))
            {
                MessageBox.Show("You must specify input type");
                return;
            }

            if (outmul.Text.Length == 0)
            {
                MessageBox.Show("You must specify the output mul");
                return;
            }

            if (outidx.Text.Length == 0 && fileType != FileType.MapLegacyMul)
            {
                MessageBox.Show("You must specify the output idx");
                return;
            }

            if (inuop.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input uop");
                return;
            }

            if (!File.Exists(inuop.Text))
            {
                MessageBox.Show("The input file does not exists");
                return;
            }

            if (File.Exists(outmul.Text))
            {
                MessageBox.Show("Output mul file already exists");
                return;
            }

            if (File.Exists(outidx.Text) && fileType != FileType.MapLegacyMul)
            {
                MessageBox.Show("Output index file already exists");
                return;
            }

            try
            {
                uoptomul.Text = "Converting...";
                uoptomul.Enabled = false;

                _conv.FromUop(inuop.Text, outmul.Text, outidx.Text, fileType, (int)uopMapIndex.Value);
            }
            catch
            {
                MessageBox.Show("An error occurred");
            }
            finally
            {
                uoptomul.Text = "Convert";
                uoptomul.Enabled = true;
            }
        }
        #endregion

        #region SelectFolder
        private void SelectFolder_Click(object sender, EventArgs e)
        {
            if (FolderDialog.ShowDialog() == DialogResult.OK)
            {
                inputfolder.Text = FolderDialog.SelectedPath;
            }
        }
        #endregion

        #region Extract
        private void Extract(string inFile, string outFile, string outIdx, FileType type, int typeIndex)
        {
            try
            {
                statustext.Text = inFile;
                Refresh();
                inFile = FixPath(inFile);

                if (!File.Exists(inFile))
                {
                    return;
                }

                outFile = FixPath(outFile);

                if (File.Exists(outFile))
                {
                    return;
                }

                outIdx = FixPath(outIdx);
                ++_total;

                _conv.FromUop(inFile, outFile, outIdx, type, typeIndex);

                ++_success;
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occurred while performing the action.\r\n{e.Message}");
            }
        }
        #endregion

        #region Pack
        private void Pack(string inFile, string inIdx, string outFile, FileType type, int typeIndex)
        {
            try
            {
                statustext.Text = inFile;
                Refresh();
                inFile = FixPath(inFile);

                if (!File.Exists(inFile))
                {
                    return;
                }

                outFile = FixPath(outFile);

                if (File.Exists(outFile))
                {
                    return;
                }

                inIdx = FixPath(inIdx);
                ++_total;

                LegacyMulFileConverter.ToUop(inFile, inIdx, outFile, type, typeIndex);

                ++_success;
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occurred while performing the action.\r\n{e.Message}");
            }
        }
        #endregion

        #region FixPath
        private string FixPath(string file)
        {
            return (file == null) ? null : Path.Combine(inputfolder.Text, file);
        }
        #endregion

        #region StartFolderButtonClick
        private void StartFolderButtonClick(object sender, EventArgs e)
        {
            if (inputfolder.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input folder");
                return;
            }

            if (extract.Checked)
            {
                _success = _total = 0;

                Extract("artLegacyMUL.uop", "art.mul", "artidx.mul", FileType.ArtLegacyMul, 0);
                Extract("gumpartLegacyMUL.uop", "gumpart.mul", "gumpidx.mul", FileType.GumpartLegacyMul, 0);
                Extract("soundLegacyMUL.uop", "sound.mul", "soundidx.mul", FileType.SoundLegacyMul, 0);

                for (int i = 0; i <= 5; ++i)
                {
                    string map = $"map{i}";

                    Extract(map + "LegacyMUL.uop", map + ".mul", null, FileType.MapLegacyMul, i);
                    Extract(map + "xLegacyMUL.uop", map + "x.mul", null, FileType.MapLegacyMul, i);
                }

                statustext.Text = $"Done ({_success}/{_total} files extracted)";
            }
            else if (pack.Checked)
            {
                _success = _total = 0;

                Pack("art.mul", "artidx.mul", "artLegacyMUL.uop", FileType.ArtLegacyMul, 0);
                Pack("gumpart.mul", "gumpidx.mul", "gumpartLegacyMUL.uop", FileType.GumpartLegacyMul, 0);
                Pack("sound.mul", "soundidx.mul", "soundLegacyMUL.uop", FileType.SoundLegacyMul, 0);

                for (int i = 0; i <= 5; ++i)
                {
                    string map = $"map{i}";

                    Pack(map + ".mul", null, map + "LegacyMUL.uop", FileType.MapLegacyMul, i);
                    Pack(map + "x.mul", null, map + "xLegacyMUL.uop", FileType.MapLegacyMul, i);
                }

                statustext.Text = $"Done ({_success}/{_total} files packed)";
            }
            else
            {
                MessageBox.Show("You must select an option");
            }
        }
        #endregion

        #region SingleFileUopArtExtractButton
        private void SingleFileUopArtExtractButtonClick(object sender, EventArgs e)
        {
            if (inputfolder.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input folder");
                return;
            }

            if (extract.Checked)
            {
                _success = _total = 0;
                ExtractSingleArtUopToMul();
                statustext.Text = $"Done ({_success}/{_total} files extracted)";
            }
            else
            {
                MessageBox.Show("You must select an option");
            }
        }

        private void ExtractSingleArtUopToMul()
        {
            string inFile = "artLegacyMUL.uop";
            string outFile = "art.mul";
            string outIdx = "artidx.mul";
            FileType type = FileType.ArtLegacyMul;
            int typeIndex = 0;

            try
            {
                statustext.Text = inFile;
                Refresh();
                inFile = FixPath(inFile);
                if (!File.Exists(inFile))
                {
                    return;
                }
                outFile = FixPath(outFile);
                if (File.Exists(outFile))
                {
                    return;
                }
                outIdx = FixPath(outIdx);
                ++_total;
                _conv.FromUop(inFile, outFile, outIdx, type, typeIndex);
                ++_success;
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occurred while performing the action.\r\n{e.Message}");
            }
        }
        #endregion

        #region SingleFileExtractButton
        private void SingleFileExtractButtonClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "UOP files (*.uop)|*.uop";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string inFile = openFileDialog.FileName;
                ExtractSingleUopToMul(inFile);
            }
        } 
        private void ExtractSingleUopToMul(string inFile)
        {
            string outFile = Path.ChangeExtension(inFile, ".mul");
            string outIdx = Path.ChangeExtension(inFile, ".idx");

            FileType type = FileType.ArtLegacyMul; // Default type
            int typeIndex = 0; // Default typeIndex

            // Determine the file type and type index based on the input file name
            if (inFile.Contains("artLegacyMUL.uop"))
            {
                type = FileType.ArtLegacyMul;
                typeIndex = 0;
            }
            else if (inFile.Contains("gumpartLegacyMUL.uop"))
            {
                type = FileType.GumpartLegacyMul;
                typeIndex = 0;
            }
            else if (inFile.Contains("soundLegacyMUL.uop"))
            {
                type = FileType.SoundLegacyMul;
                typeIndex = 0;
            }
            else if (inFile.Contains("map") && inFile.Contains("LegacyMUL.uop"))
            {
                type = FileType.MapLegacyMul;
                for (int i = 0; i <= 6; i++)
                {
                    if (inFile.Contains($"map{i}LegacyMUL.uop"))
                    {
                        typeIndex = i;
                        break;
                    }
                }
            }
            else if (inFile.Contains("LegacyMULOld.uop"))
            {
                // Set the correct FileType and typeIndex here
                type = FileType.MapLegacyMul; // Example: Change this as needed
                typeIndex = 0; // Example: Change this as needed
            }
            else
            {
                MessageBox.Show("Unknown file type");
                return;
            }

            try
            {
                statustext.Text = inFile;
                Refresh();
                if (!File.Exists(inFile))
                {
                    MessageBox.Show("The input file does not exist");
                    return;
                }
                if (File.Exists(outFile))
                {
                    MessageBox.Show("Output file already exists");
                    return;
                }
                ++_total;
                _conv.FromUop(inFile, outFile, outIdx, type, typeIndex);
                ++_success;
                statustext.Text = $"Done ({_success}/{_total} files extracted)";
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occurred.\r\n{e.Message}");
            }
        }
        #endregion

        #region [ CheckBoxAutoFill_CheckedChanged ]
        private void CheckBoxAutoFill_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxAutoFill.Checked)
            {
                AutoFillTextBoxes();
            }
        }
        #endregion

        #region [ AutoFillTextBoxes ]
        private void AutoFillTextBoxes()
        {
            if (string.IsNullOrEmpty(inmul.Text))
            {
                return;
            }

            string directory = Path.GetDirectoryName(inmul.Text);
            string selectedFileType = multype?.SelectedValue?.ToString() ?? string.Empty;

            if (Enum.TryParse(selectedFileType, out FileType fileType))
            {
                switch (fileType)
                {
                    case FileType.ArtLegacyMul:
                        inidx.Text = Path.Combine(directory, "artidx.mul");
                        outuop.Text = Path.Combine(directory, "artLegacyMUL.uop");
                        break;
                    case FileType.GumpartLegacyMul:
                        inidx.Text = Path.Combine(directory, "gumpidx.mul");
                        outuop.Text = Path.Combine(directory, "gumpartLegacyMUL.uop");
                        break;
                    case FileType.SoundLegacyMul:
                        inidx.Text = Path.Combine(directory, "soundidx.mul");
                        outuop.Text = Path.Combine(directory, "soundLegacyMUL.uop");
                        break;
                    case FileType.MapLegacyMul:
                        inidx.Text = null; // Map files don't have an idx file
                        outuop.Text = Path.Combine(directory, "mapLegacyMUL.uop");
                        break;
                    default:
                        MessageBox.Show("Unknown file type");
                        break;
                }
            }
            else
            {
                MessageBox.Show("Invalid file type selected");
            }
        }
        #endregion
    }
    #endregion
}
