// /***************************************************************************
//  *
//  * $Author: Turley
//  * 
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class GumpIDRechner
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GumpIDRechner));

            // -----------------------------------------------------------------------
            // Alle Controls deklarieren
            // Tab 1: Gump ID Rechner
            // Tab 2: Hex/Decimal Converter
            // -----------------------------------------------------------------------
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            tabPage2 = new System.Windows.Forms.TabPage();

            // --- Tab 1 Controls ---
            lbHexAdressInput = new System.Windows.Forms.Label();
            groupBoxResults = new System.Windows.Forms.GroupBox();
            groupBoxGumpID = new System.Windows.Forms.GroupBox();
            groupBoxDecimalInput = new System.Windows.Forms.GroupBox();  // NEU: Decimal-Eingabe Gruppe

            tbAminHex = new System.Windows.Forms.TextBox();
            tbAminID = new System.Windows.Forms.TextBox();
            tbHex = new System.Windows.Forms.TextBox();
            tbDecimal = new System.Windows.Forms.TextBox();
            tbInput = new System.Windows.Forms.TextBox();
            tbDecimalInput = new System.Windows.Forms.TextBox();  // NEU: Decimal-Eingabefeld

            label1 = new System.Windows.Forms.Label();
            lbHex = new System.Windows.Forms.Label();
            lbiD = new System.Windows.Forms.Label();
            lbDecimal = new System.Windows.Forms.Label();
            lbInput = new System.Windows.Forms.Label();
            lbDecimalInputLabel = new System.Windows.Forms.Label();     // NEU: Label fuer Decimal-Eingabe
            lbWomanOffset = new System.Windows.Forms.Label();     // NEU: Hinweis Woman = Men + 10000

            BtWoman = new System.Windows.Forms.Button();
            BtMen = new System.Windows.Forms.Button();
            BtCopyResults = new System.Windows.Forms.Button();    // NEU: Ergebnisse kopieren
            BtDecimalInput = new System.Windows.Forms.Button();    // NEU: Decimal → Hex umrechnen

            // --- Tab 2 Controls ---
            groupBoxConvert = new System.Windows.Forms.GroupBox();
            checkBoxAllConversions = new System.Windows.Forms.CheckBox();  // NEU: Alle Konvertierungen anzeigen
            checkBoxAsciiToText = new System.Windows.Forms.CheckBox();
            checkBoxAsciiCode = new System.Windows.Forms.CheckBox();
            checkBoxCase = new System.Windows.Forms.CheckBox();
            checkBoxAscii = new System.Windows.Forms.CheckBox();
            checkBoxBaseN = new System.Windows.Forms.CheckBox();
            checkBoxOctal = new System.Windows.Forms.CheckBox();
            checkBoxBinary = new System.Windows.Forms.CheckBox();
            checkBoxDecimal = new System.Windows.Forms.CheckBox();
            checkBoxHexAdress = new System.Windows.Forms.CheckBox();

            lbOutput = new System.Windows.Forms.Label();
            lbimput = new System.Windows.Forms.Label();
            lbBaseNLabel = new System.Windows.Forms.Label();     // NEU: Label fuer Base-N Eingabe

            tbInput2 = new System.Windows.Forms.TextBox();
            tbOutput = new System.Windows.Forms.TextBox();
            tbBaseN = new System.Windows.Forms.TextBox();   // NEU: Base-N Eingabefeld

            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            groupBoxResults.SuspendLayout();
            groupBoxGumpID.SuspendLayout();
            groupBoxDecimalInput.SuspendLayout();
            groupBoxConvert.SuspendLayout();
            SuspendLayout();

            // -----------------------------------------------------------------------
            // tabControl1
            // Hauptcontainer mit zwei Tabs
            // -----------------------------------------------------------------------
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new System.Drawing.Point(2, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(530, 400);
            tabControl1.TabIndex = 0;

            // -----------------------------------------------------------------------
            // tabPage1 - "2Gump 2Amin"
            // Berechnung der UO Gump-Adresse aus einer Hex-Eingabe
            // -----------------------------------------------------------------------
            tabPage1.Controls.Add(lbHexAdressInput);
            tabPage1.Controls.Add(groupBoxResults);
            tabPage1.Controls.Add(groupBoxGumpID);
            tabPage1.Controls.Add(groupBoxDecimalInput);
            tabPage1.Controls.Add(lbInput);
            tabPage1.Controls.Add(tbInput);
            tabPage1.Controls.Add(lbWomanOffset);
            tabPage1.Controls.Add(BtCopyResults);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(3);
            tabPage1.Size = new System.Drawing.Size(522, 372);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "2Gump 2Amin";
            tabPage1.UseVisualStyleBackColor = true;

            // -----------------------------------------------------------------------
            // lbHexAdressInput
            // Statuslabel - zeigt ob die berechnete ID gueltig ist oder eine Warnung
            // -----------------------------------------------------------------------
            lbHexAdressInput.AutoSize = true;
            lbHexAdressInput.Location = new System.Drawing.Point(10, 41);
            lbHexAdressInput.Name = "lbHexAdressInput";
            lbHexAdressInput.Size = new System.Drawing.Size(300, 15);
            lbHexAdressInput.TabIndex = 4;
            lbHexAdressInput.Text = "Hex address input..";

            // -----------------------------------------------------------------------
            // lbWomanOffset
            // NEU: Hinweislabel das erklaert warum Woman +10000 ist
            // -----------------------------------------------------------------------
            lbWomanOffset.AutoSize = true;
            lbWomanOffset.Location = new System.Drawing.Point(237, 120);
            lbWomanOffset.Name = "lbWomanOffset";
            lbWomanOffset.Size = new System.Drawing.Size(200, 15);
            lbWomanOffset.TabIndex = 10;
            lbWomanOffset.Text = "Woman = Men + 10000 (UO Gump Offset)";
            lbWomanOffset.ForeColor = System.Drawing.Color.Gray;

            // -----------------------------------------------------------------------
            // groupBoxResults
            // Zeigt die berechneten Ergebnisse: Decimal, Hex, AMin ID, AMin Hex
            // -----------------------------------------------------------------------
            groupBoxResults.Controls.Add(tbAminHex);
            groupBoxResults.Controls.Add(label1);
            groupBoxResults.Controls.Add(tbAminID);
            groupBoxResults.Controls.Add(lbHex);
            groupBoxResults.Controls.Add(lbiD);
            groupBoxResults.Controls.Add(lbDecimal);
            groupBoxResults.Controls.Add(tbHex);
            groupBoxResults.Controls.Add(tbDecimal);
            groupBoxResults.Location = new System.Drawing.Point(18, 140);
            groupBoxResults.Name = "groupBoxResults";
            groupBoxResults.Size = new System.Drawing.Size(218, 180);
            groupBoxResults.TabIndex = 3;
            groupBoxResults.TabStop = false;
            groupBoxResults.Text = "Results";

            // tbAminHex - AMin Hex Ergebnis
            tbAminHex.Location = new System.Drawing.Point(101, 147);
            tbAminHex.Name = "tbAminHex";
            tbAminHex.ReadOnly = true;
            tbAminHex.Size = new System.Drawing.Size(100, 23);
            tbAminHex.TabIndex = 7;

            // label1 - Amin Hex Label
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(25, 150);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(63, 15);
            label1.TabIndex = 6;
            label1.Text = "Amin Hex:";

            // tbAminID - AMin ID Ergebnis (Modulo 1000 der Decimal-ID)
            tbAminID.Location = new System.Drawing.Point(101, 115);
            tbAminID.Name = "tbAminID";
            tbAminID.ReadOnly = true;
            tbAminID.Size = new System.Drawing.Size(100, 23);
            tbAminID.TabIndex = 5;

            // lbHex - Label fuer Hex-Ausgabe
            lbHex.AutoSize = true;
            lbHex.Location = new System.Drawing.Point(57, 56);
            lbHex.Name = "lbHex";
            lbHex.Size = new System.Drawing.Size(31, 15);
            lbHex.TabIndex = 4;
            lbHex.Text = "Hex:";

            // lbiD - Label fuer AMin ID
            lbiD.AutoSize = true;
            lbiD.Location = new System.Drawing.Point(32, 118);
            lbiD.Name = "lbiD";
            lbiD.Size = new System.Drawing.Size(56, 15);
            lbiD.TabIndex = 3;
            lbiD.Text = "Amin ID :";

            // lbDecimal - Label fuer Decimal-Ausgabe
            lbDecimal.AutoSize = true;
            lbDecimal.Location = new System.Drawing.Point(34, 23);
            lbDecimal.Name = "lbDecimal";
            lbDecimal.Size = new System.Drawing.Size(56, 15);
            lbDecimal.TabIndex = 2;
            lbDecimal.Text = "Decimal :";

            // tbHex - Hex Ergebnis (mit 0x-Prefix und 4-stelligem Padding)
            tbHex.Location = new System.Drawing.Point(101, 53);
            tbHex.Name = "tbHex";
            tbHex.ReadOnly = true;
            tbHex.Size = new System.Drawing.Size(100, 23);
            tbHex.TabIndex = 1;

            // tbDecimal - Decimal Ergebnis
            tbDecimal.Location = new System.Drawing.Point(101, 20);
            tbDecimal.Name = "tbDecimal";
            tbDecimal.ReadOnly = true;
            tbDecimal.Size = new System.Drawing.Size(100, 23);
            tbDecimal.TabIndex = 0;

            // -----------------------------------------------------------------------
            // groupBoxGumpID
            // Buttons zum Ausloesen der Berechnung (Men ohne Offset, Woman +10000)
            // -----------------------------------------------------------------------
            groupBoxGumpID.Controls.Add(BtWoman);
            groupBoxGumpID.Controls.Add(BtMen);
            groupBoxGumpID.Location = new System.Drawing.Point(237, 17);
            groupBoxGumpID.Name = "groupBoxGumpID";
            groupBoxGumpID.Size = new System.Drawing.Size(200, 100);
            groupBoxGumpID.TabIndex = 2;
            groupBoxGumpID.TabStop = false;
            groupBoxGumpID.Text = "Gump ID";

            // BtWoman - Berechnet Gump ID fuer Woman (+10000 Offset)
            BtWoman.Location = new System.Drawing.Point(40, 56);
            BtWoman.Name = "BtWoman";
            BtWoman.Size = new System.Drawing.Size(120, 23);
            BtWoman.TabIndex = 1;
            BtWoman.Text = "Woman (+10000)";
            BtWoman.UseVisualStyleBackColor = true;
            BtWoman.Click += BtWoman_Click;

            // BtMen - Berechnet Gump ID fuer Men (kein Offset)
            BtMen.Location = new System.Drawing.Point(40, 27);
            BtMen.Name = "BtMen";
            BtMen.Size = new System.Drawing.Size(120, 23);
            BtMen.TabIndex = 0;
            BtMen.Text = "Men";
            BtMen.UseVisualStyleBackColor = true;
            BtMen.Click += BtMen_Click;

            // -----------------------------------------------------------------------
            // groupBoxDecimalInput
            // NEU: Erlaubt die Eingabe als Dezimalzahl statt als Hex
            // Rechnet automatisch in Hex um und befuellt tbInput
            // -----------------------------------------------------------------------
            groupBoxDecimalInput.Controls.Add(tbDecimalInput);
            groupBoxDecimalInput.Controls.Add(lbDecimalInputLabel);
            groupBoxDecimalInput.Controls.Add(BtDecimalInput);
            groupBoxDecimalInput.Location = new System.Drawing.Point(237, 145);
            groupBoxDecimalInput.Name = "groupBoxDecimalInput";
            groupBoxDecimalInput.Size = new System.Drawing.Size(260, 80);
            groupBoxDecimalInput.TabIndex = 9;
            groupBoxDecimalInput.TabStop = false;
            groupBoxDecimalInput.Text = "Decimal Input (optional)";

            // lbDecimalInputLabel - Label fuer das Decimal-Eingabefeld
            lbDecimalInputLabel.AutoSize = true;
            lbDecimalInputLabel.Location = new System.Drawing.Point(6, 25);
            lbDecimalInputLabel.Name = "lbDecimalInputLabel";
            lbDecimalInputLabel.Size = new System.Drawing.Size(56, 15);
            lbDecimalInputLabel.TabIndex = 0;
            lbDecimalInputLabel.Text = "Decimal:";

            // tbDecimalInput - Eingabefeld fuer Decimal-Werte
            tbDecimalInput.Location = new System.Drawing.Point(65, 22);
            tbDecimalInput.Name = "tbDecimalInput";
            tbDecimalInput.Size = new System.Drawing.Size(90, 23);
            tbDecimalInput.TabIndex = 1;

            // BtDecimalInput - Konvertiert Decimal → Hex und befuellt tbInput
            BtDecimalInput.Location = new System.Drawing.Point(165, 22);
            BtDecimalInput.Name = "BtDecimalInput";
            BtDecimalInput.Size = new System.Drawing.Size(85, 23);
            BtDecimalInput.TabIndex = 2;
            BtDecimalInput.Text = "→ Hex Input";
            BtDecimalInput.UseVisualStyleBackColor = true;
            BtDecimalInput.Click += BtDecimalInput_Click;

            // -----------------------------------------------------------------------
            // lbInput - Label fuer den Hex-Eingabe Textbox
            // -----------------------------------------------------------------------
            lbInput.AutoSize = true;
            lbInput.Location = new System.Drawing.Point(65, 62);
            lbInput.Name = "lbInput";
            lbInput.Size = new System.Drawing.Size(41, 15);
            lbInput.TabIndex = 1;
            lbInput.Text = "Input :";

            // -----------------------------------------------------------------------
            // tbInput - Hex-Eingabefeld (z.B. 0x1234 oder 1234)
            // -----------------------------------------------------------------------
            tbInput.Location = new System.Drawing.Point(119, 59);
            tbInput.Name = "tbInput";
            tbInput.Size = new System.Drawing.Size(100, 23);
            tbInput.TabIndex = 0;

            // -----------------------------------------------------------------------
            // BtCopyResults
            // NEU: Kopiert alle Ergebnisse als formatierten Text in die Zwischenablage
            // -----------------------------------------------------------------------
            BtCopyResults.Location = new System.Drawing.Point(18, 330);
            BtCopyResults.Name = "BtCopyResults";
            BtCopyResults.Size = new System.Drawing.Size(218, 28);
            BtCopyResults.TabIndex = 11;
            BtCopyResults.Text = "📋 Ergebnisse kopieren";
            BtCopyResults.UseVisualStyleBackColor = true;
            BtCopyResults.Click += BtCopyResults_Click;

            // -----------------------------------------------------------------------
            // tabPage2 - "Hex Decimal Calc ..."
            // Allgemeiner Converter fuer verschiedene Zahlensysteme und Textformate
            // -----------------------------------------------------------------------
            tabPage2.Controls.Add(groupBoxConvert);
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(3);
            tabPage2.Size = new System.Drawing.Size(522, 372);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Hex Decimal Calc ...";
            tabPage2.UseVisualStyleBackColor = true;

            // -----------------------------------------------------------------------
            // groupBoxConvert
            // Hauptgruppe fuer alle Konvertierungs-Controls
            // -----------------------------------------------------------------------
            groupBoxConvert.Controls.Add(checkBoxAllConversions);
            groupBoxConvert.Controls.Add(checkBoxAsciiToText);
            groupBoxConvert.Controls.Add(checkBoxAsciiCode);
            groupBoxConvert.Controls.Add(checkBoxCase);
            groupBoxConvert.Controls.Add(checkBoxAscii);
            groupBoxConvert.Controls.Add(checkBoxBaseN);
            groupBoxConvert.Controls.Add(lbBaseNLabel);
            groupBoxConvert.Controls.Add(tbBaseN);
            groupBoxConvert.Controls.Add(checkBoxOctal);
            groupBoxConvert.Controls.Add(checkBoxBinary);
            groupBoxConvert.Controls.Add(checkBoxDecimal);
            groupBoxConvert.Controls.Add(lbOutput);
            groupBoxConvert.Controls.Add(checkBoxHexAdress);
            groupBoxConvert.Controls.Add(lbimput);
            groupBoxConvert.Controls.Add(tbInput2);
            groupBoxConvert.Controls.Add(tbOutput);
            groupBoxConvert.Location = new System.Drawing.Point(6, 6);
            groupBoxConvert.Name = "groupBoxConvert";
            groupBoxConvert.Size = new System.Drawing.Size(510, 360);
            groupBoxConvert.TabIndex = 7;
            groupBoxConvert.TabStop = false;
            groupBoxConvert.Text = "Convert";

            // checkBoxAllConversions - NEU: Zeigt alle Konvertierungen gleichzeitig
            checkBoxAllConversions.AutoSize = true;
            checkBoxAllConversions.Location = new System.Drawing.Point(15, 247);
            checkBoxAllConversions.Name = "checkBoxAllConversions";
            checkBoxAllConversions.Size = new System.Drawing.Size(170, 19);
            checkBoxAllConversions.TabIndex = 14;
            checkBoxAllConversions.Text = "Alle Konvertierungen anzeigen";
            checkBoxAllConversions.UseVisualStyleBackColor = true;
            checkBoxAllConversions.CheckedChanged += CheckBox_CheckedChanged;

            // checkBoxAsciiToText - ASCII Codes → Text
            checkBoxAsciiToText.AutoSize = true;
            checkBoxAsciiToText.Location = new System.Drawing.Point(15, 222);
            checkBoxAsciiToText.Name = "checkBoxAsciiToText";
            checkBoxAsciiToText.Size = new System.Drawing.Size(153, 19);
            checkBoxAsciiToText.TabIndex = 13;
            checkBoxAsciiToText.Text = "Ascii To Text Conversion";
            checkBoxAsciiToText.UseVisualStyleBackColor = true;
            checkBoxAsciiToText.CheckedChanged += CheckBox_CheckedChanged;

            // checkBoxAsciiCode - Text → ASCII Codes
            checkBoxAsciiCode.AutoSize = true;
            checkBoxAsciiCode.Location = new System.Drawing.Point(15, 197);
            checkBoxAsciiCode.Name = "checkBoxAsciiCode";
            checkBoxAsciiCode.Size = new System.Drawing.Size(145, 19);
            checkBoxAsciiCode.TabIndex = 12;
            checkBoxAsciiCode.Text = "Ascii Code Conversion";
            checkBoxAsciiCode.UseVisualStyleBackColor = true;
            checkBoxAsciiCode.CheckedChanged += CheckBox_CheckedChanged;

            // checkBoxCase - Upper/Lower Case Toggle
            checkBoxCase.AutoSize = true;
            checkBoxCase.Location = new System.Drawing.Point(15, 172);
            checkBoxCase.Name = "checkBoxCase";
            checkBoxCase.Size = new System.Drawing.Size(124, 19);
            checkBoxCase.TabIndex = 11;
            checkBoxCase.Text = "Upper / lower case";
            checkBoxCase.UseVisualStyleBackColor = true;
            checkBoxCase.CheckedChanged += CheckBox_CheckedChanged;

            // checkBoxAscii - ASCII Code → Zeichen
            checkBoxAscii.AutoSize = true;
            checkBoxAscii.Location = new System.Drawing.Point(15, 147);
            checkBoxAscii.Name = "checkBoxAscii";
            checkBoxAscii.Size = new System.Drawing.Size(117, 19);
            checkBoxAscii.TabIndex = 10;
            checkBoxAscii.Text = "ASCII Conversion";
            checkBoxAscii.UseVisualStyleBackColor = true;
            checkBoxAscii.CheckedChanged += CheckBox_CheckedChanged;

            // checkBoxBaseN - Base-N Konvertierung mit eigenem Eingabefeld fuer die Basis
            checkBoxBaseN.AutoSize = true;
            checkBoxBaseN.Location = new System.Drawing.Point(15, 122);
            checkBoxBaseN.Name = "checkBoxBaseN";
            checkBoxBaseN.Size = new System.Drawing.Size(127, 19);
            checkBoxBaseN.TabIndex = 9;
            checkBoxBaseN.Text = "Base-N Conversion";
            checkBoxBaseN.UseVisualStyleBackColor = true;
            checkBoxBaseN.CheckedChanged += CheckBox_CheckedChanged;

            // lbBaseNLabel - NEU: Label fuer das Base-N Eingabefeld
            lbBaseNLabel.AutoSize = true;
            lbBaseNLabel.Location = new System.Drawing.Point(145, 123);
            lbBaseNLabel.Name = "lbBaseNLabel";
            lbBaseNLabel.Size = new System.Drawing.Size(38, 15);
            lbBaseNLabel.TabIndex = 15;
            lbBaseNLabel.Text = "Base:";

            // tbBaseN - NEU: Eingabefeld fuer die Zielbasis (2–36)
            tbBaseN.Location = new System.Drawing.Point(188, 120);
            tbBaseN.Name = "tbBaseN";
            tbBaseN.Size = new System.Drawing.Size(40, 23);
            tbBaseN.TabIndex = 16;
            tbBaseN.Text = "16";  // Standard: Hexadezimal

            // checkBoxOctal - Oktal → Decimal
            checkBoxOctal.AutoSize = true;
            checkBoxOctal.Location = new System.Drawing.Point(15, 97);
            checkBoxOctal.Name = "checkBoxOctal";
            checkBoxOctal.Size = new System.Drawing.Size(117, 19);
            checkBoxOctal.TabIndex = 8;
            checkBoxOctal.Text = "Octal Conversion";
            checkBoxOctal.UseVisualStyleBackColor = true;
            checkBoxOctal.CheckedChanged += CheckBox_CheckedChanged;

            // checkBoxBinary - Binaer → Decimal
            checkBoxBinary.AutoSize = true;
            checkBoxBinary.Location = new System.Drawing.Point(15, 72);
            checkBoxBinary.Name = "checkBoxBinary";
            checkBoxBinary.Size = new System.Drawing.Size(122, 19);
            checkBoxBinary.TabIndex = 7;
            checkBoxBinary.Text = "Binary Conversion";
            checkBoxBinary.UseVisualStyleBackColor = true;
            checkBoxBinary.CheckedChanged += CheckBox_CheckedChanged;

            // checkBoxDecimal - Decimal → Hex
            checkBoxDecimal.AutoSize = true;
            checkBoxDecimal.Location = new System.Drawing.Point(15, 22);
            checkBoxDecimal.Name = "checkBoxDecimal";
            checkBoxDecimal.Size = new System.Drawing.Size(132, 19);
            checkBoxDecimal.TabIndex = 5;
            checkBoxDecimal.Text = "Decimal Conversion";
            checkBoxDecimal.UseVisualStyleBackColor = true;
            checkBoxDecimal.CheckedChanged += CheckBox_CheckedChanged;

            // checkBoxHexAdress - Hex → Decimal
            checkBoxHexAdress.AutoSize = true;
            checkBoxHexAdress.Location = new System.Drawing.Point(15, 47);
            checkBoxHexAdress.Name = "checkBoxHexAdress";
            checkBoxHexAdress.Size = new System.Drawing.Size(148, 19);
            checkBoxHexAdress.TabIndex = 6;
            checkBoxHexAdress.Text = "Hex Adress Conversion";
            checkBoxHexAdress.UseVisualStyleBackColor = true;
            checkBoxHexAdress.CheckedChanged += CheckBox_CheckedChanged;

            // lbOutput - Label fuer das Output-Feld
            lbOutput.AutoSize = true;
            lbOutput.Location = new System.Drawing.Point(240, 155);
            lbOutput.Name = "lbOutput";
            lbOutput.Size = new System.Drawing.Size(115, 15);
            lbOutput.TabIndex = 4;
            lbOutput.Text = "Output (Doppelklick=Copy)";

            // lbimput - Label fuer das Input-Feld (Tippfehler aus Original beibehalten)
            lbimput.AutoSize = true;
            lbimput.Location = new System.Drawing.Point(240, 22);
            lbimput.Name = "lbimput";
            lbimput.Size = new System.Drawing.Size(45, 15);
            lbimput.TabIndex = 3;
            lbimput.Text = "Imput :";

            // tbInput2 - Mehrzeiliges Eingabefeld fuer den Converter
            tbInput2.Location = new System.Drawing.Point(240, 40);
            tbInput2.Multiline = true;
            tbInput2.Name = "tbInput2";
            tbInput2.Size = new System.Drawing.Size(260, 110);
            tbInput2.TabIndex = 0;
            tbInput2.TextChanged += tbInput2_TextChanged;

            // tbOutput - Mehrzeiliges Ausgabefeld, Doppelklick kopiert in Zwischenablage
            tbOutput.Location = new System.Drawing.Point(240, 173);
            tbOutput.Multiline = true;
            tbOutput.Name = "tbOutput";
            tbOutput.ReadOnly = true;
            tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tbOutput.Size = new System.Drawing.Size(260, 175);
            tbOutput.TabIndex = 1;
            tbOutput.MouseDoubleClick += tbOutput_MouseDoubleClick;

            // -----------------------------------------------------------------------
            // GumpIDRechner - Hauptform
            // -----------------------------------------------------------------------
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(540, 420);
            Controls.Add(tabControl1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "GumpIDRechner";
            Text = "Gump ID Calculator";

            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            groupBoxResults.ResumeLayout(false);
            groupBoxResults.PerformLayout();
            groupBoxGumpID.ResumeLayout(false);
            groupBoxDecimalInput.ResumeLayout(false);
            groupBoxDecimalInput.PerformLayout();
            groupBoxConvert.ResumeLayout(false);
            groupBoxConvert.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // -----------------------------------------------------------------------
        // Control-Deklarationen fuer den Designer
        // -----------------------------------------------------------------------

        // TabControl
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;

        // Tab 1 - Gump ID Rechner
        private System.Windows.Forms.TextBox tbInput;
        private System.Windows.Forms.TextBox tbDecimalInput;         // NEU
        private System.Windows.Forms.GroupBox groupBoxResults;
        private System.Windows.Forms.GroupBox groupBoxGumpID;
        private System.Windows.Forms.GroupBox groupBoxDecimalInput;  // NEU
        private System.Windows.Forms.Button BtWoman;
        private System.Windows.Forms.Button BtMen;
        private System.Windows.Forms.Button BtCopyResults;           // NEU
        private System.Windows.Forms.Button BtDecimalInput;          // NEU
        private System.Windows.Forms.Label lbInput;
        private System.Windows.Forms.Label lbDecimalInputLabel;      // NEU
        private System.Windows.Forms.Label lbWomanOffset;            // NEU
        private System.Windows.Forms.Label lbiD;
        private System.Windows.Forms.Label lbDecimal;
        private System.Windows.Forms.Label lbHex;
        private System.Windows.Forms.Label lbHexAdressInput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbAminID;
        private System.Windows.Forms.TextBox tbAminHex;
        private System.Windows.Forms.TextBox tbHex;
        private System.Windows.Forms.TextBox tbDecimal;

        // Tab 2 - Converter
        private System.Windows.Forms.GroupBox groupBoxConvert;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.TextBox tbInput2;
        private System.Windows.Forms.TextBox tbBaseN;                // NEU
        private System.Windows.Forms.Label lbOutput;
        private System.Windows.Forms.Label lbimput;
        private System.Windows.Forms.Label lbBaseNLabel;             // NEU
        private System.Windows.Forms.CheckBox checkBoxHexAdress;
        private System.Windows.Forms.CheckBox checkBoxDecimal;
        private System.Windows.Forms.CheckBox checkBoxOctal;
        private System.Windows.Forms.CheckBox checkBoxBinary;
        private System.Windows.Forms.CheckBox checkBoxAscii;
        private System.Windows.Forms.CheckBox checkBoxBaseN;
        private System.Windows.Forms.CheckBox checkBoxCase;
        private System.Windows.Forms.CheckBox checkBoxAsciiCode;
        private System.Windows.Forms.CheckBox checkBoxAsciiToText;
        private System.Windows.Forms.CheckBox checkBoxAllConversions; // NEU
    }
}