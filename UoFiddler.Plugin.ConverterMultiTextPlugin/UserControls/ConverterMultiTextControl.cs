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
        Timer timer = new Timer();

        private string originalFileName;

        //One Form
        private bool isFormOpen = false;

        public ConverterMultiTextControl()
        {
            InitializeComponent();

            label1.Text = "";
            label2.Text = "";

            timer.Interval = 1000;

            // Add the event handler
            timer.Tick += new EventHandler(timer_Tick);

            // Start the timer
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            // Update the text of the ToolStripStatusLabel with the current time
            toolStripStatusLabelTime.Text = DateTime.Now.ToString("HH:mm:ss") + " Uhr";
        }

        private void BtnMultiOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Save the path and filename of the selected file.
                string filePath = openFileDialog.FileName;
                originalFileName = Path.GetFileNameWithoutExtension(filePath);

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

        private void btnSpeichernTxt_Click(object sender, EventArgs e)
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
            string fileName = originalFileName + ".txt";
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

        private void btnUmwandeln_Click(object sender, EventArgs e)
        {
            string inputText = textBox1.Text.Trim(); // Retrieve the text from TextBox1 and remove leading/trailing whitespace.
            if (string.IsNullOrEmpty(inputText)) // Check if textBox1 is empty.
            {
                // Handle the case where textBox1 is empty.
                // For example, display an error message to the user.
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

        private void btnCopyTBox2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
        }

        private void buttonGraficCutterForm_Click(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.buttonGraficCutterForm, "Graphic Cutter");

            GraphicCutterForm form = new GraphicCutterForm();
            form.FormClosed += GraphicCutterForm_FormClosed; // Subscribe to the FormClosed event.
            form.Show();
            isFormOpen = true;

            buttonGraficCutterForm.Enabled = false; // Disable the button.
        }
        private void GraphicCutterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            isFormOpen = false;
            buttonGraficCutterForm.Enabled = true; // Enable the button again.
        }

        private void TextureCutter_Click(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            // Create a new instance of ToolTip.
            ToolTip toolTip1 = new ToolTip();

            // Set the delay properties.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            // Force the text to appear right-aligned.
            toolTip1.ShowAlways = true;
            // Set the text that will be displayed when the mouse pointer is over the button.
            toolTip1.SetToolTip(this.TextureCutter, "Texture Cutter");

            TextureCutter form = new TextureCutter();
            form.FormClosed += TextureCutter_FormClosed;
            form.Show();
            isFormOpen = true;

            TextureCutter.Enabled = false;
        }

        private void TextureCutter_FormClosed(object sender, FormClosedEventArgs e)
        {
            isFormOpen = false;
            TextureCutter.Enabled = true; // Enable the button again.
        }

        private void btDecriptClient_Click(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btDecriptClient, "Decript");

            DecriptClientForm form = new DecriptClientForm();
            form.FormClosed += DecriptClientForm_FormClosed;
            form.Show();
            isFormOpen = true;

            form.Enabled = true;

            //tabControl1.SelectedTab = tabPageClient;  //Example of how I can switch to tab from main
        }

        private void DecriptClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            isFormOpen = false;
            btDecriptClient.Enabled = true; // Enable the button again.
        }

        private void btMapMaker_Click(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btMapMaker, "Map Maker");

            MapMaker form = new MapMaker();
            form.FormClosed += MapMaker_FormClosed;
            form.Show();
            isFormOpen = true;

            btMapMaker.Enabled = false;
        }
        private void MapMaker_FormClosed(object sender, FormClosedEventArgs e)
        {
            isFormOpen = false;
            btMapMaker.Enabled = true; // Enable the button again.
        }

        private void btAnimationVDForm_Click(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btAnimationVDForm, "VD Animation");

            AnimationVDForm form = new AnimationVDForm();
            form.FormClosed += AnimationVDForm_FormClosed;
            form.Show();
            isFormOpen = true;

            btAnimationVDForm.Enabled = false;
        }

        private void AnimationVDForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isFormOpen)
            {
                isFormOpen = false;
                btAnimationVDForm.Enabled = true; // Enable the button again.
            }
        }

        private void btAnimationEditFormButton_Click(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                return; // Exit the method if the form is already open.
            }


            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btAnimationEditFormButton, "Animation Edit");

            AnimationEditFormButton form = new AnimationEditFormButton();
            form.FormClosed += AnimationEditFormButton_FormClosed;
            form.Show();
            isFormOpen = true;

            btAnimationEditFormButton.Enabled = false;
        }

        private void AnimationEditFormButton_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isFormOpen)
            {
                isFormOpen = false;
                btAnimationEditFormButton.Enabled = true; // Enable the button again.
            }
        }
        private void btGumpsEdit_Click(object sender, EventArgs e)
        {
            if (isFormOpen) // Wenn das Formular bereits geöffnet ist, beende die Methode.
            {
                return;
            }


            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btGumpsEdit, "Gump Edit");

            GumpsEdit form = new GumpsEdit();
            form.FormClosed += GumpsEditClosed;
            form.Show();
            isFormOpen = true;

            btGumpsEdit.Enabled = false;
        }

        private void GumpsEditClosed(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                isFormOpen = false;
                btGumpsEdit.Enabled = true;
            }
        }

        #region AltitudeButton
        private void btAltitudeTool_Click(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                return;
            }


            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btAltitudeTool, "Altitude Tool Frontend");

            AltitudeToolForm form = new AltitudeToolForm();
            form.FormClosed += AltitudeTool_FormClosed;
            form.Show();
            isFormOpen = true;

            btAltitudeTool.Enabled = false;
        }
        private void AltitudeTool_FormClosed(object sender, FormClosedEventArgs e)
        {
            isFormOpen = false;
            btAltitudeTool.Enabled = true;
        }
        #endregion

        #region Button binary code
        private void btBinaryCode_Click(object sender, EventArgs e)
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
                MessageBox.Show("Fehler beim Umwandeln der Zeichenkette in eine Zahl: " + ex.Message);
            }
        }

        #endregion

        #region MorseCode
        private void btMorseCode_Click(object sender, EventArgs e)
        {
            Dictionary<char, string> morseCodeDictionary = new Dictionary<char, string>()
            {
                {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."}, {'F', "..-."},
                {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"}, {'K', "-.-"}, {'L', ".-.."},
                {'M', "--"}, {'N', "-."}, {'O', "---"}, {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."},
                {'S', "..."}, {'T', "-"}, {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"},
                {'Y', "-.--"}, {'Z', "--.."}, {'0', "-----"}, {'1', ".----"}, {'2', "..---"},
                {'3', "...--"}, {'4', "....-"}, {'5', "....."}, {'6', "-...."}, {'7', "--..."},
                {'8', "---.."}, {'9', "----."}, {' ', "/"}, {'Ä', ".-.-"}, {'Ö', "---."}, {'Ü', "..--"}
            };

            if (checkBoxASCII.Checked)
            {
                string morseCode = textBox2.Text;
                string text = "";

                foreach (var word in morseCode.Split(new string[] { " / " }, StringSplitOptions.None))
                {
                    foreach (var letter in word.Split(' '))
                    {
                        text += morseCodeDictionary.FirstOrDefault(x => x.Value == letter).Key;
                    }
                    text += " ";
                }

                textBox1.Text = text.Trim();
            }
            else
            {
                string text = textBox1.Text.ToUpper();
                string morseCode = String.Join(" ", text.Select(c => morseCodeDictionary.ContainsKey(c) ? morseCodeDictionary[c] : ""));
                textBox2.Text = morseCode;
            }
        }
        #endregion

        #region Clear
        private void btclear_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
        }
        #endregion

        #region Clipboard Text
        private void importClipboardToolStripMenuItem_Click(object sender, EventArgs e)
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

        #region Button Copy Replace Map

        private void btMapReplace_Click(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                return; // Verlassen Sie die Methode, wenn das Formular bereits geöffnet ist.
            }

            ToolTip toolTip1 = new ToolTip();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btMapReplace, "Copy Replace Map");

            // Das Formular, das geöffnet werden soll, heißt 'MapReplaceNewForm'.
            MapReplaceNewForm form = new MapReplaceNewForm();
            form.FormClosed += btMapReplace_FormClosed;
            form.Show();
            isFormOpen = true;

            // Deaktivieren Sie die Schaltfläche 'btMapReplace'.
            this.btMapReplace.Enabled = false;
        }

        private void btMapReplace_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isFormOpen)
            {
                isFormOpen = false;
                // Aktivieren Sie die Schaltfläche 'btMapReplace' erneut.
                this.btMapReplace.Enabled = true;
            }
        }
        #endregion

        #region Art Mul Creator

        private void btArtMul_Click(object sender, EventArgs e)
        {
            if (isFormOpen) // If the form is already open, exit the method.
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btArtMul, "ART Mul IDX Creator");

            var artMulIdxCreatorForm = new ARTMulIDXCreator();
            artMulIdxCreatorForm.FormClosed += ArtMulIdxCreatorFormClosed;
            artMulIdxCreatorForm.Show();
            isFormOpen = true;

            btArtMul.Enabled = false;
        }

        private void ArtMulIdxCreatorFormClosed(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                isFormOpen = false;
                btArtMul.Enabled = true;
            }
        }
        #endregion

        #region ScriptCreator
        private void btScriptCreator_Click(object sender, EventArgs e)
        {
            if (isFormOpen) // If the form is already open, exit the method.
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
            isFormOpen = true;

            btScriptCreator.Enabled = false;
        }

        private void ScriptCreatorFormClosed(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                isFormOpen = false;
                btScriptCreator.Enabled = true;
            }
        }
        #endregion

        #region UOArtMerge
        private void btUOArtMerge_Click(object sender, EventArgs e)
        {
            if (isFormOpen) // If the form is already open, exit the method.
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btUOArtMerge, "UO Art Merge");

            var uoArtMergeForm = new UOArtMergeForm();
            uoArtMergeForm.FormClosed += UOArtMergeFormClosed;
            uoArtMergeForm.Show();
            isFormOpen = true;

            btUOArtMerge.Enabled = false;
        }

        private void UOArtMergeFormClosed(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                isFormOpen = false;
                btUOArtMerge.Enabled = true;
            }
        }
        #endregion

        #region Gump ID Rechner
        private void btGumpIDRechner_Click(object sender, EventArgs e)
        {
            if (isFormOpen) // If the form is already open, exit the method.
            {
                return;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btGumpIDRechner, "Gump ID Rechner");

            var gumpIDRechnerForm = new GumpIDRechner();
            gumpIDRechnerForm.FormClosed += GumpIDRechnerFormClosed;
            gumpIDRechnerForm.Show();
            isFormOpen = true;

            btGumpIDRechner.Enabled = false;
        }

        private void GumpIDRechnerFormClosed(object sender, EventArgs e)
        {
            if (isFormOpen)
            {
                isFormOpen = false;
                btGumpIDRechner.Enabled = true;
            }
        }
        #endregion
    }
}
