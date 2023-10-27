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

namespace UoFiddler.Forms
{
    partial class Bin_Dec_Hex_ConverterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Bin_Dec_Hex_ConverterForm));
            textBoxBinär = new System.Windows.Forms.TextBox();
            textBoxDecimal = new System.Windows.Forms.TextBox();
            textBoxHexdezimal = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            bt_0Binär = new System.Windows.Forms.Button();
            bt_1Binär = new System.Windows.Forms.Button();
            bt_Decimal01 = new System.Windows.Forms.Button();
            bt_Decimal02 = new System.Windows.Forms.Button();
            bt_Decimal03 = new System.Windows.Forms.Button();
            bt_Decimal04 = new System.Windows.Forms.Button();
            bt_Decimal05 = new System.Windows.Forms.Button();
            bt_Decimal06 = new System.Windows.Forms.Button();
            bt_Decimal07 = new System.Windows.Forms.Button();
            bt_Decimal08 = new System.Windows.Forms.Button();
            bt_Decimal09 = new System.Windows.Forms.Button();
            bt_Decimal00 = new System.Windows.Forms.Button();
            bt_Hex00 = new System.Windows.Forms.Button();
            bt_Hex09 = new System.Windows.Forms.Button();
            bt_Hex08 = new System.Windows.Forms.Button();
            bt_Hex07 = new System.Windows.Forms.Button();
            bt_Hex06 = new System.Windows.Forms.Button();
            bt_Hex05 = new System.Windows.Forms.Button();
            bt_Hex04 = new System.Windows.Forms.Button();
            bt_Hex03 = new System.Windows.Forms.Button();
            bt_Hex02 = new System.Windows.Forms.Button();
            bt_Hex01 = new System.Windows.Forms.Button();
            bt_Hex0A = new System.Windows.Forms.Button();
            bt_Hex0B = new System.Windows.Forms.Button();
            bt_Hex0C = new System.Windows.Forms.Button();
            bt_Hex0D = new System.Windows.Forms.Button();
            bt_Hex0E = new System.Windows.Forms.Button();
            bt_Hex0F = new System.Windows.Forms.Button();
            Bt_Delete_fields = new System.Windows.Forms.Button();
            tb_listing = new System.Windows.Forms.TextBox();
            bt_Listing_Clear = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // textBoxBinär
            // 
            textBoxBinär.Location = new System.Drawing.Point(42, 63);
            textBoxBinär.Name = "textBoxBinär";
            textBoxBinär.Size = new System.Drawing.Size(285, 23);
            textBoxBinär.TabIndex = 0;
            textBoxBinär.TextChanged += textBoxBinär_TextChanged;
            textBoxBinär.KeyPress += textBoxBinär_KeyPress;
            // 
            // textBoxDecimal
            // 
            textBoxDecimal.Location = new System.Drawing.Point(42, 166);
            textBoxDecimal.Name = "textBoxDecimal";
            textBoxDecimal.Size = new System.Drawing.Size(286, 23);
            textBoxDecimal.TabIndex = 1;
            textBoxDecimal.TextChanged += textBoxDecimal_TextChanged;
            textBoxDecimal.KeyPress += textBoxDecimal_KeyPress;
            // 
            // textBoxHexdezimal
            // 
            textBoxHexdezimal.Location = new System.Drawing.Point(42, 283);
            textBoxHexdezimal.Name = "textBoxHexdezimal";
            textBoxHexdezimal.Size = new System.Drawing.Size(286, 23);
            textBoxHexdezimal.TabIndex = 2;
            textBoxHexdezimal.TextChanged += textBoxHexdezimal_TextChanged;
            textBoxHexdezimal.KeyPress += textBoxHexdezimal_KeyPress;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(42, 36);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(43, 15);
            label1.TabIndex = 3;
            label1.Text = "Binary:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(42, 139);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(53, 15);
            label2.TabIndex = 4;
            label2.Text = "Decimal:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(41, 256);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(79, 15);
            label3.TabIndex = 5;
            label3.Text = "Hexadecimal:";
            // 
            // bt_0Binär
            // 
            bt_0Binär.Location = new System.Drawing.Point(42, 92);
            bt_0Binär.Name = "bt_0Binär";
            bt_0Binär.Size = new System.Drawing.Size(32, 23);
            bt_0Binär.TabIndex = 6;
            bt_0Binär.Text = "0";
            bt_0Binär.UseVisualStyleBackColor = true;
            bt_0Binär.Click += bt_0Binär_Click;
            // 
            // bt_1Binär
            // 
            bt_1Binär.Location = new System.Drawing.Point(79, 92);
            bt_1Binär.Name = "bt_1Binär";
            bt_1Binär.Size = new System.Drawing.Size(32, 23);
            bt_1Binär.TabIndex = 7;
            bt_1Binär.Text = "1";
            bt_1Binär.UseVisualStyleBackColor = true;
            bt_1Binär.Click += bt_1Binär_Click;
            // 
            // bt_Decimal01
            // 
            bt_Decimal01.Location = new System.Drawing.Point(42, 205);
            bt_Decimal01.Name = "bt_Decimal01";
            bt_Decimal01.Size = new System.Drawing.Size(32, 23);
            bt_Decimal01.TabIndex = 8;
            bt_Decimal01.Text = "1";
            bt_Decimal01.UseVisualStyleBackColor = true;
            bt_Decimal01.Click += bt_Decimal01_Click;
            // 
            // bt_Decimal02
            // 
            bt_Decimal02.Location = new System.Drawing.Point(79, 205);
            bt_Decimal02.Name = "bt_Decimal02";
            bt_Decimal02.Size = new System.Drawing.Size(32, 23);
            bt_Decimal02.TabIndex = 9;
            bt_Decimal02.Text = "2";
            bt_Decimal02.UseVisualStyleBackColor = true;
            bt_Decimal02.Click += bt_Decimal02_Click;
            // 
            // bt_Decimal03
            // 
            bt_Decimal03.Location = new System.Drawing.Point(117, 205);
            bt_Decimal03.Name = "bt_Decimal03";
            bt_Decimal03.Size = new System.Drawing.Size(32, 23);
            bt_Decimal03.TabIndex = 10;
            bt_Decimal03.Text = "3";
            bt_Decimal03.UseVisualStyleBackColor = true;
            bt_Decimal03.Click += bt_Decimal03_Click;
            // 
            // bt_Decimal04
            // 
            bt_Decimal04.Location = new System.Drawing.Point(155, 205);
            bt_Decimal04.Name = "bt_Decimal04";
            bt_Decimal04.Size = new System.Drawing.Size(32, 23);
            bt_Decimal04.TabIndex = 11;
            bt_Decimal04.Text = "4";
            bt_Decimal04.UseVisualStyleBackColor = true;
            bt_Decimal04.Click += bt_Decimal04_Click;
            // 
            // bt_Decimal05
            // 
            bt_Decimal05.Location = new System.Drawing.Point(193, 205);
            bt_Decimal05.Name = "bt_Decimal05";
            bt_Decimal05.Size = new System.Drawing.Size(32, 23);
            bt_Decimal05.TabIndex = 12;
            bt_Decimal05.Text = "5";
            bt_Decimal05.UseVisualStyleBackColor = true;
            bt_Decimal05.Click += bt_Decimal05_Click;
            // 
            // bt_Decimal06
            // 
            bt_Decimal06.Location = new System.Drawing.Point(231, 205);
            bt_Decimal06.Name = "bt_Decimal06";
            bt_Decimal06.Size = new System.Drawing.Size(32, 23);
            bt_Decimal06.TabIndex = 13;
            bt_Decimal06.Text = "6";
            bt_Decimal06.UseVisualStyleBackColor = true;
            bt_Decimal06.Click += bt_Decimal06_Click;
            // 
            // bt_Decimal07
            // 
            bt_Decimal07.Location = new System.Drawing.Point(269, 205);
            bt_Decimal07.Name = "bt_Decimal07";
            bt_Decimal07.Size = new System.Drawing.Size(32, 23);
            bt_Decimal07.TabIndex = 14;
            bt_Decimal07.Text = "7";
            bt_Decimal07.UseVisualStyleBackColor = true;
            bt_Decimal07.Click += bt_Decimal07_Click;
            // 
            // bt_Decimal08
            // 
            bt_Decimal08.Location = new System.Drawing.Point(307, 205);
            bt_Decimal08.Name = "bt_Decimal08";
            bt_Decimal08.Size = new System.Drawing.Size(32, 23);
            bt_Decimal08.TabIndex = 15;
            bt_Decimal08.Text = "8";
            bt_Decimal08.UseVisualStyleBackColor = true;
            bt_Decimal08.Click += bt_Decimal08_Click;
            // 
            // bt_Decimal09
            // 
            bt_Decimal09.Location = new System.Drawing.Point(345, 205);
            bt_Decimal09.Name = "bt_Decimal09";
            bt_Decimal09.Size = new System.Drawing.Size(32, 23);
            bt_Decimal09.TabIndex = 16;
            bt_Decimal09.Text = "9";
            bt_Decimal09.UseVisualStyleBackColor = true;
            bt_Decimal09.Click += bt_Decimal09_Click;
            // 
            // bt_Decimal00
            // 
            bt_Decimal00.Location = new System.Drawing.Point(383, 205);
            bt_Decimal00.Name = "bt_Decimal00";
            bt_Decimal00.Size = new System.Drawing.Size(32, 23);
            bt_Decimal00.TabIndex = 17;
            bt_Decimal00.Text = "0";
            bt_Decimal00.UseVisualStyleBackColor = true;
            bt_Decimal00.Click += bt_Decimal00_Click;
            // 
            // bt_Hex00
            // 
            bt_Hex00.Location = new System.Drawing.Point(386, 321);
            bt_Hex00.Name = "bt_Hex00";
            bt_Hex00.Size = new System.Drawing.Size(32, 23);
            bt_Hex00.TabIndex = 27;
            bt_Hex00.Text = "0";
            bt_Hex00.UseVisualStyleBackColor = true;
            bt_Hex00.Click += bt_Hex00_Click;
            // 
            // bt_Hex09
            // 
            bt_Hex09.Location = new System.Drawing.Point(348, 321);
            bt_Hex09.Name = "bt_Hex09";
            bt_Hex09.Size = new System.Drawing.Size(32, 23);
            bt_Hex09.TabIndex = 26;
            bt_Hex09.Text = "9";
            bt_Hex09.UseVisualStyleBackColor = true;
            bt_Hex09.Click += bt_Hex09_Click;
            // 
            // bt_Hex08
            // 
            bt_Hex08.Location = new System.Drawing.Point(310, 321);
            bt_Hex08.Name = "bt_Hex08";
            bt_Hex08.Size = new System.Drawing.Size(32, 23);
            bt_Hex08.TabIndex = 25;
            bt_Hex08.Text = "8";
            bt_Hex08.UseVisualStyleBackColor = true;
            bt_Hex08.Click += bt_Hex08_Click;
            // 
            // bt_Hex07
            // 
            bt_Hex07.Location = new System.Drawing.Point(272, 321);
            bt_Hex07.Name = "bt_Hex07";
            bt_Hex07.Size = new System.Drawing.Size(32, 23);
            bt_Hex07.TabIndex = 24;
            bt_Hex07.Text = "7";
            bt_Hex07.UseVisualStyleBackColor = true;
            bt_Hex07.Click += bt_Hex07_Click;
            // 
            // bt_Hex06
            // 
            bt_Hex06.Location = new System.Drawing.Point(234, 321);
            bt_Hex06.Name = "bt_Hex06";
            bt_Hex06.Size = new System.Drawing.Size(32, 23);
            bt_Hex06.TabIndex = 23;
            bt_Hex06.Text = "6";
            bt_Hex06.UseVisualStyleBackColor = true;
            bt_Hex06.Click += bt_Hex06_Click;
            // 
            // bt_Hex05
            // 
            bt_Hex05.Location = new System.Drawing.Point(196, 321);
            bt_Hex05.Name = "bt_Hex05";
            bt_Hex05.Size = new System.Drawing.Size(32, 23);
            bt_Hex05.TabIndex = 22;
            bt_Hex05.Text = "5";
            bt_Hex05.UseVisualStyleBackColor = true;
            bt_Hex05.Click += bt_Hex05_Click;
            // 
            // bt_Hex04
            // 
            bt_Hex04.Location = new System.Drawing.Point(158, 321);
            bt_Hex04.Name = "bt_Hex04";
            bt_Hex04.Size = new System.Drawing.Size(32, 23);
            bt_Hex04.TabIndex = 21;
            bt_Hex04.Text = "4";
            bt_Hex04.UseVisualStyleBackColor = true;
            bt_Hex04.Click += bt_Hex04_Click;
            // 
            // bt_Hex03
            // 
            bt_Hex03.Location = new System.Drawing.Point(120, 321);
            bt_Hex03.Name = "bt_Hex03";
            bt_Hex03.Size = new System.Drawing.Size(32, 23);
            bt_Hex03.TabIndex = 20;
            bt_Hex03.Text = "3";
            bt_Hex03.UseVisualStyleBackColor = true;
            bt_Hex03.Click += bt_Hex03_Click;
            // 
            // bt_Hex02
            // 
            bt_Hex02.Location = new System.Drawing.Point(82, 321);
            bt_Hex02.Name = "bt_Hex02";
            bt_Hex02.Size = new System.Drawing.Size(32, 23);
            bt_Hex02.TabIndex = 19;
            bt_Hex02.Text = "2";
            bt_Hex02.UseVisualStyleBackColor = true;
            bt_Hex02.Click += bt_Hex02_Click;
            // 
            // bt_Hex01
            // 
            bt_Hex01.Location = new System.Drawing.Point(42, 321);
            bt_Hex01.Name = "bt_Hex01";
            bt_Hex01.Size = new System.Drawing.Size(32, 23);
            bt_Hex01.TabIndex = 18;
            bt_Hex01.Text = "1";
            bt_Hex01.UseVisualStyleBackColor = true;
            bt_Hex01.Click += bt_Hex01_Click;
            // 
            // bt_Hex0A
            // 
            bt_Hex0A.Location = new System.Drawing.Point(424, 321);
            bt_Hex0A.Name = "bt_Hex0A";
            bt_Hex0A.Size = new System.Drawing.Size(32, 23);
            bt_Hex0A.TabIndex = 28;
            bt_Hex0A.Text = "A";
            bt_Hex0A.UseVisualStyleBackColor = true;
            bt_Hex0A.Click += bt_Hex0A_Click;
            // 
            // bt_Hex0B
            // 
            bt_Hex0B.Location = new System.Drawing.Point(462, 321);
            bt_Hex0B.Name = "bt_Hex0B";
            bt_Hex0B.Size = new System.Drawing.Size(32, 23);
            bt_Hex0B.TabIndex = 29;
            bt_Hex0B.Text = "B";
            bt_Hex0B.UseVisualStyleBackColor = true;
            bt_Hex0B.Click += bt_Hex0B_Click;
            // 
            // bt_Hex0C
            // 
            bt_Hex0C.Location = new System.Drawing.Point(500, 321);
            bt_Hex0C.Name = "bt_Hex0C";
            bt_Hex0C.Size = new System.Drawing.Size(32, 23);
            bt_Hex0C.TabIndex = 30;
            bt_Hex0C.Text = "C";
            bt_Hex0C.UseVisualStyleBackColor = true;
            bt_Hex0C.Click += bt_Hex0C_Click;
            // 
            // bt_Hex0D
            // 
            bt_Hex0D.Location = new System.Drawing.Point(538, 321);
            bt_Hex0D.Name = "bt_Hex0D";
            bt_Hex0D.Size = new System.Drawing.Size(32, 23);
            bt_Hex0D.TabIndex = 31;
            bt_Hex0D.Text = "D";
            bt_Hex0D.UseVisualStyleBackColor = true;
            bt_Hex0D.Click += bt_Hex0D_Click;
            // 
            // bt_Hex0E
            // 
            bt_Hex0E.Location = new System.Drawing.Point(576, 321);
            bt_Hex0E.Name = "bt_Hex0E";
            bt_Hex0E.Size = new System.Drawing.Size(32, 23);
            bt_Hex0E.TabIndex = 32;
            bt_Hex0E.Text = "E";
            bt_Hex0E.UseVisualStyleBackColor = true;
            bt_Hex0E.Click += bt_Hex0E_Click;
            // 
            // bt_Hex0F
            // 
            bt_Hex0F.Location = new System.Drawing.Point(614, 321);
            bt_Hex0F.Name = "bt_Hex0F";
            bt_Hex0F.Size = new System.Drawing.Size(32, 23);
            bt_Hex0F.TabIndex = 33;
            bt_Hex0F.Text = "F";
            bt_Hex0F.UseVisualStyleBackColor = true;
            bt_Hex0F.Click += bt_Hex0F_Click;
            // 
            // Bt_Delete_fields
            // 
            Bt_Delete_fields.Location = new System.Drawing.Point(42, 362);
            Bt_Delete_fields.Name = "Bt_Delete_fields";
            Bt_Delete_fields.Size = new System.Drawing.Size(113, 23);
            Bt_Delete_fields.TabIndex = 34;
            Bt_Delete_fields.Text = "Delete fields";
            Bt_Delete_fields.UseVisualStyleBackColor = true;
            Bt_Delete_fields.Click += Bt_Delete_fields_Click;
            // 
            // tb_listing
            // 
            tb_listing.Location = new System.Drawing.Point(432, 63);
            tb_listing.Multiline = true;
            tb_listing.Name = "tb_listing";
            tb_listing.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tb_listing.Size = new System.Drawing.Size(214, 126);
            tb_listing.TabIndex = 35;
            // 
            // bt_Listing_Clear
            // 
            bt_Listing_Clear.Location = new System.Drawing.Point(571, 205);
            bt_Listing_Clear.Name = "bt_Listing_Clear";
            bt_Listing_Clear.Size = new System.Drawing.Size(75, 23);
            bt_Listing_Clear.TabIndex = 37;
            bt_Listing_Clear.Text = "Clear";
            bt_Listing_Clear.UseVisualStyleBackColor = true;
            bt_Listing_Clear.Click += bt_Listing_Clear_Click;
            // 
            // Bin_Dec_Hex_ConverterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(673, 411);
            Controls.Add(bt_Listing_Clear);
            Controls.Add(tb_listing);
            Controls.Add(Bt_Delete_fields);
            Controls.Add(bt_Hex0F);
            Controls.Add(bt_Hex0E);
            Controls.Add(bt_Hex0D);
            Controls.Add(bt_Hex0C);
            Controls.Add(bt_Hex0B);
            Controls.Add(bt_Hex0A);
            Controls.Add(bt_Hex00);
            Controls.Add(bt_Hex09);
            Controls.Add(bt_Hex08);
            Controls.Add(bt_Hex07);
            Controls.Add(bt_Hex06);
            Controls.Add(bt_Hex05);
            Controls.Add(bt_Hex04);
            Controls.Add(bt_Hex03);
            Controls.Add(bt_Hex02);
            Controls.Add(bt_Hex01);
            Controls.Add(bt_Decimal00);
            Controls.Add(bt_Decimal09);
            Controls.Add(bt_Decimal08);
            Controls.Add(bt_Decimal07);
            Controls.Add(bt_Decimal06);
            Controls.Add(bt_Decimal05);
            Controls.Add(bt_Decimal04);
            Controls.Add(bt_Decimal03);
            Controls.Add(bt_Decimal02);
            Controls.Add(bt_Decimal01);
            Controls.Add(bt_1Binär);
            Controls.Add(bt_0Binär);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBoxHexdezimal);
            Controls.Add(textBoxDecimal);
            Controls.Add(textBoxBinär);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "Bin_Dec_Hex_ConverterForm";
            Text = "Binary Decimal Hexadecimal Converter";
            FormClosed += Bin_Dec_Hex_ConverterForm_FormClosed;
            Load += Bin_Dec_Hex_ConverterForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBoxBinär;
        private System.Windows.Forms.TextBox textBoxDecimal;
        private System.Windows.Forms.TextBox textBoxHexdezimal;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button bt_0Binär;
        private System.Windows.Forms.Button bt_1Binär;
        private System.Windows.Forms.Button bt_Decimal01;
        private System.Windows.Forms.Button bt_Decimal02;
        private System.Windows.Forms.Button bt_Decimal03;
        private System.Windows.Forms.Button bt_Decimal04;
        private System.Windows.Forms.Button bt_Decimal05;
        private System.Windows.Forms.Button bt_Decimal06;
        private System.Windows.Forms.Button bt_Decimal07;
        private System.Windows.Forms.Button bt_Decimal08;
        private System.Windows.Forms.Button bt_Decimal09;
        private System.Windows.Forms.Button bt_Decimal00;
        private System.Windows.Forms.Button bt_Hex00;
        private System.Windows.Forms.Button bt_Hex09;
        private System.Windows.Forms.Button bt_Hex08;
        private System.Windows.Forms.Button bt_Hex07;
        private System.Windows.Forms.Button bt_Hex06;
        private System.Windows.Forms.Button bt_Hex05;
        private System.Windows.Forms.Button bt_Hex04;
        private System.Windows.Forms.Button bt_Hex03;
        private System.Windows.Forms.Button bt_Hex02;
        private System.Windows.Forms.Button bt_Hex01;
        private System.Windows.Forms.Button bt_Hex0A;
        private System.Windows.Forms.Button bt_Hex0B;
        private System.Windows.Forms.Button bt_Hex0C;
        private System.Windows.Forms.Button bt_Hex0D;
        private System.Windows.Forms.Button bt_Hex0E;
        private System.Windows.Forms.Button bt_Hex0F;
        private System.Windows.Forms.Button Bt_Delete_fields;
        private System.Windows.Forms.TextBox tb_listing;
        private System.Windows.Forms.Button bt_Listing_Clear;
    }
}