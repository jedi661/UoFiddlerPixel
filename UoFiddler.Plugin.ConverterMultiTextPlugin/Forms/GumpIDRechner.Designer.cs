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
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            lbHexAdressInput = new System.Windows.Forms.Label();
            groupBoxResults = new System.Windows.Forms.GroupBox();
            tbAminHex = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            tbAminID = new System.Windows.Forms.TextBox();
            lbHex = new System.Windows.Forms.Label();
            lbiD = new System.Windows.Forms.Label();
            lbDecimal = new System.Windows.Forms.Label();
            tbHex = new System.Windows.Forms.TextBox();
            tbDecimal = new System.Windows.Forms.TextBox();
            groupBoxGumpID = new System.Windows.Forms.GroupBox();
            btWoman = new System.Windows.Forms.Button();
            BtMen = new System.Windows.Forms.Button();
            lbInput = new System.Windows.Forms.Label();
            tbInput = new System.Windows.Forms.TextBox();
            tabPage2 = new System.Windows.Forms.TabPage();
            groupBoxConvert = new System.Windows.Forms.GroupBox();
            checkBoxAsciiToText = new System.Windows.Forms.CheckBox();
            checkBoxAsciiCode = new System.Windows.Forms.CheckBox();
            checkBoxCase = new System.Windows.Forms.CheckBox();
            checkBoxAscii = new System.Windows.Forms.CheckBox();
            checkBoxBaseN = new System.Windows.Forms.CheckBox();
            checkBoxOctal = new System.Windows.Forms.CheckBox();
            checkBoxBinary = new System.Windows.Forms.CheckBox();
            checkBoxDecimal = new System.Windows.Forms.CheckBox();
            lbOutput = new System.Windows.Forms.Label();
            checkBoxHexAdress = new System.Windows.Forms.CheckBox();
            lbimput = new System.Windows.Forms.Label();
            tbInput2 = new System.Windows.Forms.TextBox();
            tbOutput = new System.Windows.Forms.TextBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBoxResults.SuspendLayout();
            groupBoxGumpID.SuspendLayout();
            tabPage2.SuspendLayout();
            groupBoxConvert.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new System.Drawing.Point(2, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(474, 329);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(lbHexAdressInput);
            tabPage1.Controls.Add(groupBoxResults);
            tabPage1.Controls.Add(groupBoxGumpID);
            tabPage1.Controls.Add(lbInput);
            tabPage1.Controls.Add(tbInput);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(3);
            tabPage1.Size = new System.Drawing.Size(466, 301);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "2Gump 2Amin";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // lbHexAdressInput
            // 
            lbHexAdressInput.AutoSize = true;
            lbHexAdressInput.Location = new System.Drawing.Point(111, 41);
            lbHexAdressInput.Name = "lbHexAdressInput";
            lbHexAdressInput.Size = new System.Drawing.Size(108, 15);
            lbHexAdressInput.TabIndex = 4;
            lbHexAdressInput.Text = "Hex address input..";
            // 
            // groupBoxResults
            // 
            groupBoxResults.Controls.Add(tbAminHex);
            groupBoxResults.Controls.Add(label1);
            groupBoxResults.Controls.Add(tbAminID);
            groupBoxResults.Controls.Add(lbHex);
            groupBoxResults.Controls.Add(lbiD);
            groupBoxResults.Controls.Add(lbDecimal);
            groupBoxResults.Controls.Add(tbHex);
            groupBoxResults.Controls.Add(tbDecimal);
            groupBoxResults.Location = new System.Drawing.Point(18, 118);
            groupBoxResults.Name = "groupBoxResults";
            groupBoxResults.Size = new System.Drawing.Size(218, 168);
            groupBoxResults.TabIndex = 3;
            groupBoxResults.TabStop = false;
            groupBoxResults.Text = "Results";
            // 
            // tbAminHex
            // 
            tbAminHex.Location = new System.Drawing.Point(101, 137);
            tbAminHex.Name = "tbAminHex";
            tbAminHex.Size = new System.Drawing.Size(100, 23);
            tbAminHex.TabIndex = 7;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(25, 140);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(63, 15);
            label1.TabIndex = 6;
            label1.Text = "Amin Hex:";
            // 
            // tbAminID
            // 
            tbAminID.Location = new System.Drawing.Point(101, 105);
            tbAminID.Name = "tbAminID";
            tbAminID.Size = new System.Drawing.Size(100, 23);
            tbAminID.TabIndex = 5;
            // 
            // lbHex
            // 
            lbHex.AutoSize = true;
            lbHex.Location = new System.Drawing.Point(57, 56);
            lbHex.Name = "lbHex";
            lbHex.Size = new System.Drawing.Size(31, 15);
            lbHex.TabIndex = 4;
            lbHex.Text = "Hex:";
            // 
            // lbiD
            // 
            lbiD.AutoSize = true;
            lbiD.Location = new System.Drawing.Point(32, 113);
            lbiD.Name = "lbiD";
            lbiD.Size = new System.Drawing.Size(56, 15);
            lbiD.TabIndex = 3;
            lbiD.Text = "Amin ID :";
            // 
            // lbDecimal
            // 
            lbDecimal.AutoSize = true;
            lbDecimal.Location = new System.Drawing.Point(34, 23);
            lbDecimal.Name = "lbDecimal";
            lbDecimal.Size = new System.Drawing.Size(56, 15);
            lbDecimal.TabIndex = 2;
            lbDecimal.Text = "Decimal :";
            // 
            // tbHex
            // 
            tbHex.Location = new System.Drawing.Point(101, 53);
            tbHex.Name = "tbHex";
            tbHex.Size = new System.Drawing.Size(100, 23);
            tbHex.TabIndex = 1;
            // 
            // tbDecimal
            // 
            tbDecimal.Location = new System.Drawing.Point(101, 20);
            tbDecimal.Name = "tbDecimal";
            tbDecimal.Size = new System.Drawing.Size(100, 23);
            tbDecimal.TabIndex = 0;
            // 
            // groupBoxGumpID
            // 
            groupBoxGumpID.Controls.Add(btWoman);
            groupBoxGumpID.Controls.Add(BtMen);
            groupBoxGumpID.Location = new System.Drawing.Point(237, 17);
            groupBoxGumpID.Name = "groupBoxGumpID";
            groupBoxGumpID.Size = new System.Drawing.Size(200, 100);
            groupBoxGumpID.TabIndex = 2;
            groupBoxGumpID.TabStop = false;
            groupBoxGumpID.Text = "Gump ID";
            // 
            // btWoman
            // 
            btWoman.Location = new System.Drawing.Point(40, 56);
            btWoman.Name = "btWoman";
            btWoman.Size = new System.Drawing.Size(75, 23);
            btWoman.TabIndex = 1;
            btWoman.Text = "Woman";
            btWoman.UseVisualStyleBackColor = true;
            btWoman.Click += btWoman_Click;
            // 
            // BtMen
            // 
            BtMen.Location = new System.Drawing.Point(40, 27);
            BtMen.Name = "BtMen";
            BtMen.Size = new System.Drawing.Size(75, 23);
            BtMen.TabIndex = 0;
            BtMen.Text = "Men";
            BtMen.UseVisualStyleBackColor = true;
            BtMen.Click += BtMen_Click;
            // 
            // lbInput
            // 
            lbInput.AutoSize = true;
            lbInput.Location = new System.Drawing.Point(65, 62);
            lbInput.Name = "lbInput";
            lbInput.Size = new System.Drawing.Size(41, 15);
            lbInput.TabIndex = 1;
            lbInput.Text = "Input :";
            // 
            // tbInput
            // 
            tbInput.Location = new System.Drawing.Point(119, 59);
            tbInput.Name = "tbInput";
            tbInput.Size = new System.Drawing.Size(100, 23);
            tbInput.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(groupBoxConvert);
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(3);
            tabPage2.Size = new System.Drawing.Size(466, 301);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Hex Decimal Calc ...";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBoxConvert
            // 
            groupBoxConvert.Controls.Add(checkBoxAsciiToText);
            groupBoxConvert.Controls.Add(checkBoxAsciiCode);
            groupBoxConvert.Controls.Add(checkBoxCase);
            groupBoxConvert.Controls.Add(checkBoxAscii);
            groupBoxConvert.Controls.Add(checkBoxBaseN);
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
            groupBoxConvert.Size = new System.Drawing.Size(454, 289);
            groupBoxConvert.TabIndex = 7;
            groupBoxConvert.TabStop = false;
            groupBoxConvert.Text = "Convert";
            // 
            // checkBoxAsciiToText
            // 
            checkBoxAsciiToText.AutoSize = true;
            checkBoxAsciiToText.Location = new System.Drawing.Point(15, 197);
            checkBoxAsciiToText.Name = "checkBoxAsciiToText";
            checkBoxAsciiToText.Size = new System.Drawing.Size(153, 19);
            checkBoxAsciiToText.TabIndex = 13;
            checkBoxAsciiToText.Text = "Ascii To Text Conversion";
            checkBoxAsciiToText.UseVisualStyleBackColor = true;
            checkBoxAsciiToText.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // checkBoxAsciiCode
            // 
            checkBoxAsciiCode.AutoSize = true;
            checkBoxAsciiCode.Location = new System.Drawing.Point(15, 172);
            checkBoxAsciiCode.Name = "checkBoxAsciiCode";
            checkBoxAsciiCode.Size = new System.Drawing.Size(145, 19);
            checkBoxAsciiCode.TabIndex = 12;
            checkBoxAsciiCode.Text = "Ascii Code Conversion";
            checkBoxAsciiCode.UseVisualStyleBackColor = true;
            checkBoxAsciiCode.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // checkBoxCase
            // 
            checkBoxCase.AutoSize = true;
            checkBoxCase.Location = new System.Drawing.Point(15, 222);
            checkBoxCase.Name = "checkBoxCase";
            checkBoxCase.Size = new System.Drawing.Size(124, 19);
            checkBoxCase.TabIndex = 11;
            checkBoxCase.Text = "Upper / lower case";
            checkBoxCase.UseVisualStyleBackColor = true;
            checkBoxCase.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // checkBoxAscii
            // 
            checkBoxAscii.AutoSize = true;
            checkBoxAscii.Location = new System.Drawing.Point(15, 147);
            checkBoxAscii.Name = "checkBoxAscii";
            checkBoxAscii.Size = new System.Drawing.Size(117, 19);
            checkBoxAscii.TabIndex = 10;
            checkBoxAscii.Text = "ASCII Conversion";
            checkBoxAscii.UseVisualStyleBackColor = true;
            checkBoxAscii.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // checkBoxBaseN
            // 
            checkBoxBaseN.AutoSize = true;
            checkBoxBaseN.Location = new System.Drawing.Point(15, 122);
            checkBoxBaseN.Name = "checkBoxBaseN";
            checkBoxBaseN.Size = new System.Drawing.Size(127, 19);
            checkBoxBaseN.TabIndex = 9;
            checkBoxBaseN.Text = "Base-N Conversion";
            checkBoxBaseN.UseVisualStyleBackColor = true;
            checkBoxBaseN.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // checkBoxOctal
            // 
            checkBoxOctal.AutoSize = true;
            checkBoxOctal.Location = new System.Drawing.Point(15, 97);
            checkBoxOctal.Name = "checkBoxOctal";
            checkBoxOctal.Size = new System.Drawing.Size(117, 19);
            checkBoxOctal.TabIndex = 8;
            checkBoxOctal.Text = "Octal Conversion";
            checkBoxOctal.UseVisualStyleBackColor = true;
            checkBoxOctal.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // checkBoxBinary
            // 
            checkBoxBinary.AutoSize = true;
            checkBoxBinary.Location = new System.Drawing.Point(15, 72);
            checkBoxBinary.Name = "checkBoxBinary";
            checkBoxBinary.Size = new System.Drawing.Size(122, 19);
            checkBoxBinary.TabIndex = 7;
            checkBoxBinary.Text = "Binary Conversion";
            checkBoxBinary.UseVisualStyleBackColor = true;
            checkBoxBinary.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // checkBoxDecimal
            // 
            checkBoxDecimal.AutoSize = true;
            checkBoxDecimal.Location = new System.Drawing.Point(15, 22);
            checkBoxDecimal.Name = "checkBoxDecimal";
            checkBoxDecimal.Size = new System.Drawing.Size(132, 19);
            checkBoxDecimal.TabIndex = 5;
            checkBoxDecimal.Text = "Decimal Conversion";
            checkBoxDecimal.UseVisualStyleBackColor = true;
            checkBoxDecimal.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // lbOutput
            // 
            lbOutput.AutoSize = true;
            lbOutput.Location = new System.Drawing.Point(204, 129);
            lbOutput.Name = "lbOutput";
            lbOutput.Size = new System.Drawing.Size(45, 15);
            lbOutput.TabIndex = 4;
            lbOutput.Text = "Output";
            // 
            // checkBoxHexAdress
            // 
            checkBoxHexAdress.AutoSize = true;
            checkBoxHexAdress.Location = new System.Drawing.Point(15, 47);
            checkBoxHexAdress.Name = "checkBoxHexAdress";
            checkBoxHexAdress.Size = new System.Drawing.Size(148, 19);
            checkBoxHexAdress.TabIndex = 6;
            checkBoxHexAdress.Text = "Hex Adress Conversion";
            checkBoxHexAdress.UseVisualStyleBackColor = true;
            checkBoxHexAdress.CheckedChanged += CheckBox_CheckedChanged;
            // 
            // lbimput
            // 
            lbimput.AutoSize = true;
            lbimput.Location = new System.Drawing.Point(204, 22);
            lbimput.Name = "lbimput";
            lbimput.Size = new System.Drawing.Size(45, 15);
            lbimput.TabIndex = 3;
            lbimput.Text = "Imput :";
            // 
            // tbInput2
            // 
            tbInput2.Location = new System.Drawing.Point(204, 40);
            tbInput2.Multiline = true;
            tbInput2.Name = "tbInput2";
            tbInput2.Size = new System.Drawing.Size(244, 83);
            tbInput2.TabIndex = 0;
            tbInput2.TextChanged += tbInput2_TextChanged;
            // 
            // tbOutput
            // 
            tbOutput.Location = new System.Drawing.Point(204, 147);
            tbOutput.Multiline = true;
            tbOutput.Name = "tbOutput";
            tbOutput.Size = new System.Drawing.Size(244, 136);
            tbOutput.TabIndex = 1;
            tbOutput.MouseDoubleClick += tbOutput_MouseDoubleClick;
            // 
            // GumpIDRechner
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(483, 347);
            Controls.Add(tabControl1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "GumpIDRechner";
            Text = "Gump ID Calculator";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            groupBoxResults.ResumeLayout(false);
            groupBoxResults.PerformLayout();
            groupBoxGumpID.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            groupBoxConvert.ResumeLayout(false);
            groupBoxConvert.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox tbInput;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBoxResults;
        private System.Windows.Forms.TextBox tbHex;
        private System.Windows.Forms.TextBox tbDecimal;
        private System.Windows.Forms.GroupBox groupBoxGumpID;
        private System.Windows.Forms.Button btWoman;
        private System.Windows.Forms.Button BtMen;
        private System.Windows.Forms.Label lbInput;
        private System.Windows.Forms.Label lbiD;
        private System.Windows.Forms.Label lbDecimal;
        private System.Windows.Forms.Label lbHex;
        private System.Windows.Forms.TextBox tbAminID;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.TextBox tbInput2;
        private System.Windows.Forms.Label lbOutput;
        private System.Windows.Forms.Label lbimput;
        private System.Windows.Forms.CheckBox checkBoxHexAdress;
        private System.Windows.Forms.CheckBox checkBoxDecimal;
        private System.Windows.Forms.GroupBox groupBoxConvert;
        private System.Windows.Forms.Label lbHexAdressInput;
        private System.Windows.Forms.CheckBox checkBoxOctal;
        private System.Windows.Forms.CheckBox checkBoxBinary;
        private System.Windows.Forms.CheckBox checkBoxAscii;
        private System.Windows.Forms.CheckBox checkBoxBaseN;
        private System.Windows.Forms.CheckBox checkBoxCase;
        private System.Windows.Forms.CheckBox checkBoxAsciiCode;
        private System.Windows.Forms.CheckBox checkBoxAsciiToText;
        private System.Windows.Forms.TextBox tbAminHex;
        private System.Windows.Forms.Label label1;
    }
}