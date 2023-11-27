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
    partial class ARTMulIDXCreator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ARTMulIDXCreator));
            textBox1 = new System.Windows.Forms.TextBox();
            btCreateARTIDXMul = new System.Windows.Forms.Button();
            btFileOrder = new System.Windows.Forms.Button();
            textBox2 = new System.Windows.Forms.TextBox();
            lblEntryCount = new System.Windows.Forms.Label();
            btnCountEntries = new System.Windows.Forms.Button();
            btnShowInfo = new System.Windows.Forms.Button();
            textBoxInfo = new System.Windows.Forms.TextBox();
            btnReadArtIdx = new System.Windows.Forms.Button();
            textBoxIndex = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            lbCreatedMul = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            btCreateARTIDXMul_Ulong = new System.Windows.Forms.Button();
            label12 = new System.Windows.Forms.Label();
            btCreateARTIDXMul_Byte = new System.Windows.Forms.Button();
            label11 = new System.Windows.Forms.Label();
            btCreateARTIDXMul_Sbyte = new System.Windows.Forms.Button();
            label10 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            btCreateARTIDXMul_Short = new System.Windows.Forms.Button();
            label8 = new System.Windows.Forms.Label();
            btCreateARTIDXMul_Ushort = new System.Windows.Forms.Button();
            label7 = new System.Windows.Forms.Label();
            btCreateARTIDXMul_Int = new System.Windows.Forms.Button();
            label6 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            btCreateARTIDXMul_uint = new System.Windows.Forms.Button();
            comboBoxMuls = new System.Windows.Forms.ComboBox();
            tabPage2 = new System.Windows.Forms.TabPage();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(6, 34);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(181, 23);
            textBox1.TabIndex = 0;
            // 
            // btCreateARTIDXMul
            // 
            btCreateARTIDXMul.Location = new System.Drawing.Point(112, 93);
            btCreateARTIDXMul.Name = "btCreateARTIDXMul";
            btCreateARTIDXMul.Size = new System.Drawing.Size(97, 23);
            btCreateARTIDXMul.TabIndex = 1;
            btCreateARTIDXMul.Text = "Create \"long\"";
            btCreateARTIDXMul.UseVisualStyleBackColor = true;
            btCreateARTIDXMul.Click += btCreateARTIDXMul_Click;
            // 
            // btFileOrder
            // 
            btFileOrder.Location = new System.Drawing.Point(6, 64);
            btFileOrder.Name = "btFileOrder";
            btFileOrder.Size = new System.Drawing.Size(100, 23);
            btFileOrder.TabIndex = 2;
            btFileOrder.Text = "Select file folder";
            btFileOrder.UseVisualStyleBackColor = true;
            btFileOrder.Click += btFileOrder_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new System.Drawing.Point(193, 34);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(66, 23);
            textBox2.TabIndex = 3;
            textBox2.Text = "81884";
            // 
            // lblEntryCount
            // 
            lblEntryCount.AutoSize = true;
            lblEntryCount.Location = new System.Drawing.Point(8, 6);
            lblEntryCount.Name = "lblEntryCount";
            lblEntryCount.Size = new System.Drawing.Size(72, 15);
            lblEntryCount.TabIndex = 4;
            lblEntryCount.Text = "Index Count";
            // 
            // btnCountEntries
            // 
            btnCountEntries.Location = new System.Drawing.Point(8, 33);
            btnCountEntries.Name = "btnCountEntries";
            btnCountEntries.Size = new System.Drawing.Size(115, 23);
            btnCountEntries.TabIndex = 5;
            btnCountEntries.Text = "Count ArtIDX Read";
            btnCountEntries.UseVisualStyleBackColor = true;
            btnCountEntries.Click += btnCountEntries_Click;
            // 
            // btnShowInfo
            // 
            btnShowInfo.Location = new System.Drawing.Point(8, 362);
            btnShowInfo.Name = "btnShowInfo";
            btnShowInfo.Size = new System.Drawing.Size(75, 23);
            btnShowInfo.TabIndex = 9;
            btnShowInfo.Text = "Index All";
            btnShowInfo.UseVisualStyleBackColor = true;
            btnShowInfo.Click += btnShowInfo_Click;
            // 
            // textBoxInfo
            // 
            textBoxInfo.Location = new System.Drawing.Point(8, 62);
            textBoxInfo.Multiline = true;
            textBoxInfo.Name = "textBoxInfo";
            textBoxInfo.Size = new System.Drawing.Size(253, 273);
            textBoxInfo.TabIndex = 10;
            // 
            // btnReadArtIdx
            // 
            btnReadArtIdx.Location = new System.Drawing.Point(89, 362);
            btnReadArtIdx.Name = "btnReadArtIdx";
            btnReadArtIdx.Size = new System.Drawing.Size(120, 23);
            btnReadArtIdx.TabIndex = 11;
            btnReadArtIdx.Text = "Selected index";
            btnReadArtIdx.UseVisualStyleBackColor = true;
            btnReadArtIdx.Click += btnReadArtIdx_Click;
            // 
            // textBoxIndex
            // 
            textBoxIndex.Location = new System.Drawing.Point(218, 363);
            textBoxIndex.Name = "textBoxIndex";
            textBoxIndex.Size = new System.Drawing.Size(43, 23);
            textBoxIndex.TabIndex = 13;
            textBoxIndex.Text = "1";
            textBoxIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 16);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(25, 15);
            label1.TabIndex = 14;
            label1.Text = "Dir:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(193, 16);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(43, 15);
            label2.TabIndex = 15;
            label2.Text = "Count:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(8, 344);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(108, 15);
            label3.TabIndex = 16;
            label3.Text = "Reading the ArtIdx:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(218, 344);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(39, 15);
            label4.TabIndex = 17;
            label4.Text = "Index:";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new System.Drawing.Point(3, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(547, 426);
            tabControl1.TabIndex = 18;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(lbCreatedMul);
            tabPage1.Controls.Add(label13);
            tabPage1.Controls.Add(btCreateARTIDXMul_Ulong);
            tabPage1.Controls.Add(label12);
            tabPage1.Controls.Add(btCreateARTIDXMul_Byte);
            tabPage1.Controls.Add(label11);
            tabPage1.Controls.Add(btCreateARTIDXMul_Sbyte);
            tabPage1.Controls.Add(label10);
            tabPage1.Controls.Add(label9);
            tabPage1.Controls.Add(btCreateARTIDXMul_Short);
            tabPage1.Controls.Add(label8);
            tabPage1.Controls.Add(btCreateARTIDXMul_Ushort);
            tabPage1.Controls.Add(label7);
            tabPage1.Controls.Add(btCreateARTIDXMul_Int);
            tabPage1.Controls.Add(label6);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(btCreateARTIDXMul_uint);
            tabPage1.Controls.Add(comboBoxMuls);
            tabPage1.Controls.Add(textBox2);
            tabPage1.Controls.Add(textBox1);
            tabPage1.Controls.Add(btCreateARTIDXMul);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(btFileOrder);
            tabPage1.Controls.Add(label1);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new System.Windows.Forms.Padding(3);
            tabPage1.Size = new System.Drawing.Size(539, 398);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Create Muls";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // lbCreatedMul
            // 
            lbCreatedMul.AutoSize = true;
            lbCreatedMul.Location = new System.Drawing.Point(16, 316);
            lbCreatedMul.Name = "lbCreatedMul";
            lbCreatedMul.Size = new System.Drawing.Size(48, 15);
            lbCreatedMul.TabIndex = 33;
            lbCreatedMul.Text = "Output:";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(215, 68);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(175, 15);
            label13.TabIndex = 32;
            label13.Text = " 0 bis 18.446.744.073.709.551.615";
            // 
            // btCreateARTIDXMul_Ulong
            // 
            btCreateARTIDXMul_Ulong.Location = new System.Drawing.Point(112, 64);
            btCreateARTIDXMul_Ulong.Name = "btCreateARTIDXMul_Ulong";
            btCreateARTIDXMul_Ulong.Size = new System.Drawing.Size(97, 23);
            btCreateARTIDXMul_Ulong.TabIndex = 31;
            btCreateARTIDXMul_Ulong.Text = "Create \"ulong\"";
            btCreateARTIDXMul_Ulong.UseVisualStyleBackColor = true;
            btCreateARTIDXMul_Ulong.Click += btCreateARTIDXMul_Ulong_Click;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(215, 284);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(52, 15);
            label12.TabIndex = 30;
            label12.Text = "0 bis 255";
            // 
            // btCreateARTIDXMul_Byte
            // 
            btCreateARTIDXMul_Byte.Location = new System.Drawing.Point(112, 280);
            btCreateARTIDXMul_Byte.Name = "btCreateARTIDXMul_Byte";
            btCreateARTIDXMul_Byte.Size = new System.Drawing.Size(97, 23);
            btCreateARTIDXMul_Byte.TabIndex = 29;
            btCreateARTIDXMul_Byte.Text = "Create \"byte\"";
            btCreateARTIDXMul_Byte.UseVisualStyleBackColor = true;
            btCreateARTIDXMul_Byte.Click += btCreateARTIDXMul_Byte_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(215, 255);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(72, 15);
            label11.TabIndex = 28;
            label11.Text = " -128 bis 127";
            // 
            // btCreateARTIDXMul_Sbyte
            // 
            btCreateARTIDXMul_Sbyte.Location = new System.Drawing.Point(112, 251);
            btCreateARTIDXMul_Sbyte.Name = "btCreateARTIDXMul_Sbyte";
            btCreateARTIDXMul_Sbyte.Size = new System.Drawing.Size(97, 23);
            btCreateARTIDXMul_Sbyte.TabIndex = 27;
            btCreateARTIDXMul_Sbyte.Text = "Create \"sbyte\"";
            btCreateARTIDXMul_Sbyte.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(265, 37);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(52, 15);
            label10.TabIndex = 26;
            label10.Text = "<- Index";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(215, 223);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(122, 15);
            label9.TabIndex = 25;
            label9.Text = "von -32.768 bis 32.767";
            // 
            // btCreateARTIDXMul_Short
            // 
            btCreateARTIDXMul_Short.Location = new System.Drawing.Point(112, 219);
            btCreateARTIDXMul_Short.Name = "btCreateARTIDXMul_Short";
            btCreateARTIDXMul_Short.Size = new System.Drawing.Size(97, 23);
            btCreateARTIDXMul_Short.TabIndex = 24;
            btCreateARTIDXMul_Short.Text = "Create \"short\"";
            btCreateARTIDXMul_Short.UseVisualStyleBackColor = true;
            btCreateARTIDXMul_Short.Click += btCreateARTIDXMul_Short_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(215, 191);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(70, 15);
            label8.TabIndex = 23;
            label8.Text = " 0 bis 65.535";
            // 
            // btCreateARTIDXMul_Ushort
            // 
            btCreateARTIDXMul_Ushort.Location = new System.Drawing.Point(112, 187);
            btCreateARTIDXMul_Ushort.Name = "btCreateARTIDXMul_Ushort";
            btCreateARTIDXMul_Ushort.Size = new System.Drawing.Size(97, 23);
            btCreateARTIDXMul_Ushort.TabIndex = 22;
            btCreateARTIDXMul_Ushort.Text = "Create \"ushort\"";
            btCreateARTIDXMul_Ushort.UseVisualStyleBackColor = true;
            btCreateARTIDXMul_Ushort.Click += btCreateARTIDXMul_Ushort_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(215, 162);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(194, 15);
            label7.TabIndex = 21;
            label7.Text = "von -2.147.483.648 bis 2.147.483.647";
            // 
            // btCreateARTIDXMul_Int
            // 
            btCreateARTIDXMul_Int.Location = new System.Drawing.Point(112, 158);
            btCreateARTIDXMul_Int.Name = "btCreateARTIDXMul_Int";
            btCreateARTIDXMul_Int.Size = new System.Drawing.Size(97, 23);
            btCreateARTIDXMul_Int.TabIndex = 20;
            btCreateARTIDXMul_Int.Text = "Create \"int\"";
            btCreateARTIDXMul_Int.UseVisualStyleBackColor = true;
            btCreateARTIDXMul_Int.Click += btCreateARTIDXMul_Int_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(215, 129);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(103, 15);
            label6.TabIndex = 19;
            label6.Text = "0 bis 4.294.967.295";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(215, 89);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(170, 30);
            label5.TabIndex = 18;
            label5.Text = "von -9.223.372.036.854.775.808 \r\nbis 9.223.372.036.854.775.807 ";
            // 
            // btCreateARTIDXMul_uint
            // 
            btCreateARTIDXMul_uint.Location = new System.Drawing.Point(112, 125);
            btCreateARTIDXMul_uint.Name = "btCreateARTIDXMul_uint";
            btCreateARTIDXMul_uint.Size = new System.Drawing.Size(97, 23);
            btCreateARTIDXMul_uint.TabIndex = 17;
            btCreateARTIDXMul_uint.Text = "Create \"uint\"";
            btCreateARTIDXMul_uint.UseVisualStyleBackColor = true;
            btCreateARTIDXMul_uint.Click += btCreateARTIDXMul_uint_Click;
            // 
            // comboBoxMuls
            // 
            comboBoxMuls.FormattingEnabled = true;
            comboBoxMuls.Location = new System.Drawing.Point(6, 339);
            comboBoxMuls.Name = "comboBoxMuls";
            comboBoxMuls.Size = new System.Drawing.Size(121, 23);
            comboBoxMuls.TabIndex = 16;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(btnCountEntries);
            tabPage2.Controls.Add(label4);
            tabPage2.Controls.Add(lblEntryCount);
            tabPage2.Controls.Add(label3);
            tabPage2.Controls.Add(textBoxInfo);
            tabPage2.Controls.Add(textBoxIndex);
            tabPage2.Controls.Add(btnShowInfo);
            tabPage2.Controls.Add(btnReadArtIdx);
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new System.Windows.Forms.Padding(3);
            tabPage2.Size = new System.Drawing.Size(424, 398);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Read Muls";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // ARTMulIDXCreator
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(555, 440);
            Controls.Add(tabControl1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ARTMulIDXCreator";
            Text = "ARTMulIDXCreator";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btCreateARTIDXMul;
        private System.Windows.Forms.Button btFileOrder;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label lblEntryCount;
        private System.Windows.Forms.Button btnCountEntries;
        private System.Windows.Forms.Button btnShowInfo;
        private System.Windows.Forms.TextBox textBoxInfo;
        private System.Windows.Forms.Button btnReadArtIdx;
        private System.Windows.Forms.TextBox textBoxIndex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ComboBox comboBoxMuls;
        private System.Windows.Forms.Button btCreateARTIDXMul_uint;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btCreateARTIDXMul_Int;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btCreateARTIDXMul_Ushort;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btCreateARTIDXMul_Short;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btCreateARTIDXMul_Sbyte;
        private System.Windows.Forms.Button btCreateARTIDXMul_Byte;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btCreateARTIDXMul_Ulong;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lbCreatedMul;
    }
}