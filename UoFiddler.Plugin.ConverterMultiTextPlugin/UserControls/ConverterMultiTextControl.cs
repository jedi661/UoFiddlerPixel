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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.UserControls
{
    public partial class ConverterMultiTextControl : UserControl
    {
        readonly Timer _timer = new Timer();

        private string _originalFileName;

        //One Form
        private bool _isFormOpen = false;

        public ConverterMultiTextControl()
        {
            InitializeComponent();

            label1.Text = "";
            label2.Text = "";

            _timer.Interval = 1000;

            // Add the event handler
            _timer.Tick += new EventHandler(Timer_Tick);

            // Start the timer
            _timer.Start();
        }

        #region [ Timer_Tick ]
        void Timer_Tick(object sender, EventArgs e)
        {
            // Update the text of the ToolStripStatusLabel with the current time
            toolStripStatusLabelTime.Text = DateTime.Now.ToString("HH:mm:ss") + " Uhr";
        }
        #endregion

        #region [ BtnMultiOpen_Click ]
        private void BtnMultiOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Save the path and filename of the selected file.
                string filePath = openFileDialog.FileName;
                _originalFileName = Path.GetFileNameWithoutExtension(filePath);

                // Read the text file and write it into a TextBox.
                string fileContent = File.ReadAllText(filePath);
                string[] lines = fileContent.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                StringBuilder sb = new StringBuilder();
                bool startAppending = false;
                foreach (string line in lines)
                {
                    if (line.Contains("num components"))
                    {
                        startAppending = true;
                        continue;
                    }

                    if (startAppending)
                    {
                        sb.AppendLine(line);
                    }
                }

                textBox1.Clear();
                textBox2.Clear();
                if (sb.Length > 0)
                {
                    textBox1.Text = sb.ToString();
                }
                else
                {
                    textBox1.Text = fileContent;
                }

                label1.Text = "The text has been inserted.";
            }
        }
        #endregion

        #region [ BtnSpeichernTxt_Click ]
        private void BtnSpeichernTxt_Click(object sender, EventArgs e)
        {
            // Check if TextBox2 has any content.
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                label2.Text = "There is no text that can be saved.";
                return;
            }

            // Verzeichnis erstellen
            string directoryPath = Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), "OLDScript");
            Directory.CreateDirectory(directoryPath);

            // Take the filename from the original file name.
            string fileName = _originalFileName + ".txt";
            string filePath = Path.Combine(directoryPath, fileName);

            // Write the contents of TextBox1 to a text file.
            File.WriteAllText(filePath, textBox1.Text);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt";
            saveFileDialog.InitialDirectory = Path.Combine(Path.GetDirectoryName(openFileDialog.FileName), "Export");

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Write the contents of TextBox2 to a text file.
                File.WriteAllText(saveFileDialog.FileName, textBox2.Text);

                label2.Text = "The text was successfully exported to the file.";
            }
        }
        #endregion

        #region [ BtnUmwandeln_Click ]
        private void BtnUmwandeln_Click(object sender, EventArgs e)
        {
            string inputText = textBox1.Text.Trim(); // Retrieve the text from TextBox1 and remove leading/trailing whitespace.
            if (string.IsNullOrEmpty(inputText)) // Check if textBox1 is empty.
            {
                // Handle the case where textBox1 is empty.                
                MessageBox.Show("Please enter text into the field.");
                return; // Exit the method.
            }

            string[] inputLines = inputText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None); // Split the text into a string array.

            StringBuilder outputText = new StringBuilder(); // Create a StringBuilder object to concatenate the converted lines.

            foreach (string inputLine in inputLines)
            {
                string[] inputValues = inputLine.Split(' '); // Split the values in each line by whitespace.
                int sum = 0; // Initialize the sum of the first few decimal numbers in the line.

                for (int i = 0; i < inputValues.Length; i++)
                {
                    if (i == 0) // Select and convert the first decimal number in the line.
                    {
                        sum = Convert.ToInt32(inputValues[i]);
                        outputText.Append("0x" + sum.ToString("X") + " "); // Add the first converted hexadecimal number to the StringBuilder.
                    }
                    else // Simply add the remaining values to a StringBuilder.
                    {
                        outputText.Append(inputValues[i] + " ");
                    }
                }
                outputText.Append(Environment.NewLine); // Add a line break at the end of each line.
            }

            textBox2.Text = outputText.ToString(); // Set the result in TextBox2
        }
        #endregion

        #region [ BtnCopyTBox2_Click ]
        private void BtnCopyTBox2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
        }
        #endregion

        #region [ ButtonGraficCutterForm_Click ]
        private void ButtonGraficCutterForm_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.ButtonGraficCutterForm, "Graphic Cutter");

            GraphicCutterForm form = new GraphicCutterForm();
            form.FormClosed += GraphicCutterForm_FormClosed;
            form.Show();
            _isFormOpen = true;

            ButtonGraficCutterForm.Enabled = false;
        }
        private void GraphicCutterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isFormOpen = false;
            ButtonGraficCutterForm.Enabled = true;
        }
        #endregion

        #region [ TextureCutter_Click ]
        private void TextureCutter_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.TextureCutter, "Texture Cutter");

            TextureCutter form = new TextureCutter();
            form.FormClosed += TextureCutter_FormClosed;
            form.Show();
            _isFormOpen = true;

            TextureCutter.Enabled = false;
        }

        private void TextureCutter_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isFormOpen = false;
            TextureCutter.Enabled = true;
        }
        #endregion

        #region [ BtDecriptClient ]
        private void BtDecriptClient_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtDecriptClient, "Decript");

            DecriptClientForm form = new DecriptClientForm();
            form.FormClosed += DecriptClientForm_FormClosed;
            form.Show();
            _isFormOpen = true;

            form.Enabled = true;
        }

        private void DecriptClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isFormOpen = false;
            BtDecriptClient.Enabled = true;
        }
        #endregion

        #region [ BtMapMaker_Click ]
        private void BtMapMaker_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtMapMaker, "Map Maker");

            MapMaker form = new MapMaker();
            form.FormClosed += MapMaker_FormClosed;
            form.Show();
            _isFormOpen = true;

            BtMapMaker.Enabled = false;
        }
        private void MapMaker_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isFormOpen = false;
            BtMapMaker.Enabled = true;
        }
        #endregion

        #region [ BtAnimationVDForm ]
        private void BtAnimationVDForm_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtAnimationVDForm, "VD Animation");

            AnimationVDForm form = new AnimationVDForm();
            form.FormClosed += AnimationVDForm_FormClosed;
            form.Show();
            _isFormOpen = true;

            BtAnimationVDForm.Enabled = false;
        }
        private void AnimationVDForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtAnimationVDForm.Enabled = true; // Enable the button again.
            }
        }
        #endregion

        #region [ BtAnimationEditFormButton_Click ]
        private void BtAnimationEditFormButton_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtAnimationEditFormButton, "Animation Edit");

            AnimationEditFormButton form = new AnimationEditFormButton();
            form.FormClosed += AnimationEditFormButton_FormClosed;
            form.Show();
            _isFormOpen = true;

            BtAnimationEditFormButton.Enabled = false;
        }
        private void AnimationEditFormButton_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtAnimationEditFormButton.Enabled = true; // Enable the button again.
            }
        }
        #endregion

        #region [ BtGumpsEdit ]
        private void BtGumpsEdit_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }


            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtGumpsEdit, "Gump Edit");

            GumpsEdit form = new GumpsEdit();
            form.FormClosed += GumpsEditClosed;
            form.Show();
            _isFormOpen = true;

            BtGumpsEdit.Enabled = false;
        }
        #endregion

        #region [ GumpsEditClosed ]
        private void GumpsEditClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtGumpsEdit.Enabled = true;
            }
        }
        #endregion

        #region [ AltitudeButton ]
        private void BtAltitudeTool_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtAltitudeTool, "Altitude Tool Frontend");

            AltitudeToolForm form = new AltitudeToolForm();
            form.FormClosed += AltitudeTool_FormClosed;
            form.Show();
            _isFormOpen = true;

            BtAltitudeTool.Enabled = false;
        }
        private void AltitudeTool_FormClosed(object sender, FormClosedEventArgs e)
        {
            _isFormOpen = false;
            BtAltitudeTool.Enabled = true;
        }
        #endregion

        #region [ Button binary code ]
        private void BtBinaryCode_Click(object sender, EventArgs e)
        {
            try
            {
                if (checkBoxASCII.Checked)
                {
                    string binary = textBox2.Text;
                    binary = binary.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                    List<Byte> byteList = new List<Byte>();

                    foreach (var bin in binary.Split(' '))
                    {
                        byteList.Add(Convert.ToByte(bin, 2));
                    }

                    textBox1.Text = Encoding.UTF8.GetString(byteList.ToArray());
                }
                else
                {
                    string text = textBox1.Text;
                    byte[] textBytes = Encoding.UTF8.GetBytes(text);
                    string binary = String.Join(" ", textBytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
                    textBox2.Text = binary;
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Error converting string to number: " + ex.Message);
            }
        }

        #endregion

        #region [ MorseCode ]
        private static readonly Dictionary<char, string> _morseCodeDictionary = new Dictionary<char, string>()
        {
            {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."}, {'F', "..-."},
            {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"}, {'K', "-.-"}, {'L', ".-.."},
            {'M', "--"}, {'N', "-."}, {'O', "---"}, {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."},
            {'S', "..."}, {'T', "-"}, {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"},
            {'Y', "-.--"}, {'Z', "--.."}, {'0', "-----"}, {'1', ".----"}, {'2', "..---"},
            {'3', "...--"}, {'4', "....-"}, {'5', "....."}, {'6', "-...."}, {'7', "--..."},
            {'8', "---.."}, {'9', "----."}, {' ', "/"}, {'Ä', ".-.-"}, {'Ö', "---."}, {'Ü', "..--"}
        };

        private void BtMorseCode_Click(object sender, EventArgs e)
        {
            if (checkBoxASCII.Checked)
            {
                string morseCode = textBox2.Text;
                string text = "";

                foreach (var word in morseCode.Split([" / "], StringSplitOptions.None))
                {
                    foreach (var letter in word.Split(' '))
                    {
                        text += _morseCodeDictionary.FirstOrDefault(x => x.Value == letter).Key;
                    }
                    text += " ";
                }

                textBox1.Text = text.Trim();
            }
            else
            {
                string text = textBox1.Text.ToUpper();
                string morseCode = String.Join(" ", text.Select(c => _morseCodeDictionary.TryGetValue(c, out string value) ? value : ""));
                textBox2.Text = morseCode;
            }
        }

        #endregion

        #region [ Clear ]
        private void Btclear_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
        }
        #endregion

        #region [ Clipboard Text ]
        private void ImportClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                textBox1.Text = Clipboard.GetText();
            }
            else
            {
                MessageBox.Show("The clipboard contains no text.");
            }
        }
        #endregion

        #region [ Copy Replace Map ]

        private void BtMapReplace_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtMapReplace, "Copy Replace Map");
            MapReplaceNewForm form = new MapReplaceNewForm();
            form.FormClosed += BtMapReplace_FormClosed;
            form.Show();
            _isFormOpen = true;
            this.BtMapReplace.Enabled = false;
        }

        private void BtMapReplace_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                this.BtMapReplace.Enabled = true;
            }
        }
        #endregion

        #region [ Art Mul Creator ]

        private void BtArtMul_Click(object sender, EventArgs e)
        {
            if (_isFormOpen) // If the form is already open, exit the method.
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtArtMul, "ART Mul IDX Creator");

            var artMulIdxCreatorForm = new ARTMulIDXCreator();
            artMulIdxCreatorForm.FormClosed += ArtMulIdxCreatorFormClosed;
            artMulIdxCreatorForm.Show();
            _isFormOpen = true;

            BtArtMul.Enabled = false;
        }

        private void ArtMulIdxCreatorFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtArtMul.Enabled = true;
            }
        }
        #endregion

        #region [ ScriptCreator ]
        private void BtScriptCreator_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btScriptCreator, "Script Creator");

            var scriptCreatorForm = new ScriptCreator();
            scriptCreatorForm.FormClosed += ScriptCreatorFormClosed;
            scriptCreatorForm.Show();
            _isFormOpen = true;

            btScriptCreator.Enabled = false;
        }

        private void ScriptCreatorFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                btScriptCreator.Enabled = true;
            }
        }
        #endregion

        #region [ UOArtMerge ]
        private void BtUOArtMerge_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtUOArtMerge, "UO Art Merge");

            var uoArtMergeForm = new UOArtMergeForm();
            uoArtMergeForm.FormClosed += UOArtMergeFormClosed;
            uoArtMergeForm.Show();
            _isFormOpen = true;

            BtUOArtMerge.Enabled = false;
        }

        private void UOArtMergeFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtUOArtMerge.Enabled = true;
            }
        }
        #endregion

        #region [ Gump ID Rechner ]
        private void BtGumpIDRechner_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtGumpIDRechner, "Gump ID Rechner");

            var gumpIDRechnerForm = new GumpIDRechner();
            gumpIDRechnerForm.FormClosed += GumpIDRechnerFormClosed;
            gumpIDRechnerForm.Show();
            _isFormOpen = true;

            BtGumpIDRechner.Enabled = false;
        }

        private void GumpIDRechnerFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtGumpIDRechner.Enabled = true;
            }
        }
        #endregion

        #region [ IsoTiloSlicer ]
        private void BtIsoTiloSlicer_Click(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtIsoTiloSlicer, "Iso Tilo Slicer");

            var isoTiloSlicerForm = new IsoTiloSlicer();
            isoTiloSlicerForm.FormClosed += IsoTiloSlicerFormClosed;
            isoTiloSlicerForm.Show();
            _isFormOpen = true;

            BtIsoTiloSlicer.Enabled = false;
        }

        private void IsoTiloSlicerFormClosed(object sender, EventArgs e)
        {
            if (_isFormOpen)
            {
                _isFormOpen = false;
                BtIsoTiloSlicer.Enabled = true;
            }
        }

        #endregion

        #region [ UOMap ]
        private bool _isUOMapFormOpen = false;

        private void UOMap_Click(object sender, EventArgs e)
        {
            if (_isUOMapFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.UOMap, "UOMap");

            var uomapForm = new UOMap();
            uomapForm.FormClosed += UOMap_FormClosed;
            uomapForm.Show();
            _isUOMapFormOpen = true;

            UOMap.Enabled = false;
        }

        private void UOMap_FormClosed(object sender, EventArgs e)
        {
            if (_isUOMapFormOpen)
            {
                _isUOMapFormOpen = false;
                UOMap.Enabled = true;
            }
        }
        #endregion

        #region [ BtTileArtForm ]
        private bool _isTileArtFormOpen = false;
        private void BtTileArtForm_Click(object sender, EventArgs e)
        {
            if (_isTileArtFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtTileArtForm, "TileArt");

            var tileArtForm = new TileArtForm();
            tileArtForm.FormClosed += TileArtForm_FormClosed;
            tileArtForm.Show();
            _isTileArtFormOpen = true;

            BtTileArtForm.Enabled = false;
        }

        private void TileArtForm_FormClosed(object sender, EventArgs e)
        {
            if (_isTileArtFormOpen)
            {
                _isTileArtFormOpen = false;
                BtTileArtForm.Enabled = true;
            }
        }
        #endregion

        #region [ Transitions ]
        private bool _isTransitionsFormOpen = false;

        private void BtTransitions_Click(object sender, EventArgs e)
        {
            if (_isTransitionsFormOpen) // If the form is already open, exit the method.
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtTransitions, "TransitionsForm");

            var transitionsForm = new TransitionsForm();
            transitionsForm.FormClosed += TransitionsForm_FormClosed;
            transitionsForm.Show();
            _isTransitionsFormOpen = true;

            BtTransitions.Enabled = false;
        }

        private void TransitionsForm_FormClosed(object sender, EventArgs e)
        {
            if (_isTransitionsFormOpen)
            {
                _isTransitionsFormOpen = false;
                BtTransitions.Enabled = true;
            }
        }

        #endregion

        #region [ MultiTileEntry ]
        public struct MultiTileEntry
        {
            public ushort m_ItemID;
            public short m_OffsetX, m_OffsetY, m_OffsetZ;
            public int m_Flags;
            public int m_Unk1;
        }
        #endregion

        #region [ BtTest_Click }
        private void BtTest_Click(object sender, EventArgs e)
        {
            // Get input from textBox1
            string input = textBox1.Text;

            // split input lines
            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // List for saving the converted entries
            List<MultiTileEntry> entries = new List<MultiTileEntry>();

            // Process each line
            foreach (string line in lines)
            {
                // Divide the line into parts
                string[] parts = line.Split(',');

                // Create new entry and assign values
                MultiTileEntry entry = new MultiTileEntry();
                entry.m_ItemID = Convert.ToUInt16(parts[2].Split(':')[1].Trim()); // ID
                entry.m_OffsetX = (short)(short.Parse(parts[0].Split(':')[1].Trim()) - 1405); // X
                entry.m_OffsetY = (short)(short.Parse(parts[1].Split(':')[1].Trim()) - 1709); // Y
                entry.m_OffsetZ = short.Parse(parts[3].Split(':')[1].Trim()); // Z
                entry.m_Flags = 0;
                entry.m_Unk1 = 0;

                // Add entry to list
                entries.Add(entry);
            }

            // Show converted entries in textBox2
            textBox2.Text = string.Join("\r\n", entries.Select(x => $"0x{x.m_ItemID.ToString("X4")} {x.m_OffsetX} {x.m_OffsetY} {x.m_OffsetZ} {x.m_Flags} {x.m_Unk1}"));
        }
        #endregion

        #region [ BtConverter ]
        private bool _isConverterFormOpen = false;

        private void BtConverter_Click(object sender, EventArgs e)
        {
            if (_isConverterFormOpen)
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.BtConverter, "Converter");

            var converterForm = new UoFiddler.Plugin.ConverterMultiTextPlugin.Forms.ConverterForm();
            converterForm.FormClosed += ConverterForm_FormClosed;
            converterForm.Show();
            _isConverterFormOpen = true;

            BtConverter.Enabled = false;
        }

        private void ConverterForm_FormClosed(object sender, EventArgs e)
        {
            if (_isConverterFormOpen)
            {
                _isConverterFormOpen = false;
                BtConverter.Enabled = true;
            }
        }
        #endregion        
    }
}
