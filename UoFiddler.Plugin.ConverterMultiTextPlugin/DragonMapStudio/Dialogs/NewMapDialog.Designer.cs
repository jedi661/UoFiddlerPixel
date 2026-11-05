namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    partial class NewMapDialog
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
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            lblPreset = new System.Windows.Forms.Label();
            cmbPreset = new System.Windows.Forms.ComboBox();
            lblWidth = new System.Windows.Forms.Label();
            numWidth = new System.Windows.Forms.NumericUpDown();
            lblHeight = new System.Windows.Forms.Label();
            numHeight = new System.Windows.Forms.NumericUpDown();
            lblFill = new System.Windows.Forms.Label();
            numFill = new System.Windows.Forms.NumericUpDown();
            chkBits16 = new System.Windows.Forms.CheckBox();
            panelFillSwatch = new System.Windows.Forms.Panel();
            lblFillName = new System.Windows.Forms.Label();
            lblHint = new System.Windows.Forms.Label();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numFill).BeginInit();
            SuspendLayout();
            //
            // lblName
            //
            lblName.AutoSize = true;
            lblName.Location = new System.Drawing.Point(14, 17);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(42, 15);
            lblName.TabIndex = 0;
            lblName.Text = "Name:";
            //
            // txtName
            //
            txtName.Location = new System.Drawing.Point(120, 14);
            txtName.Name = "txtName";
            txtName.Size = new System.Drawing.Size(250, 23);
            txtName.TabIndex = 1;
            txtName.Text = "NeueKarte";
            //
            // lblPreset
            //
            lblPreset.AutoSize = true;
            lblPreset.Location = new System.Drawing.Point(14, 49);
            lblPreset.Name = "lblPreset";
            lblPreset.Size = new System.Drawing.Size(64, 15);
            lblPreset.TabIndex = 2;
            lblPreset.Text = "Vorgabe:";
            //
            // cmbPreset
            //
            cmbPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbPreset.FormattingEnabled = true;
            cmbPreset.Location = new System.Drawing.Point(120, 46);
            cmbPreset.Name = "cmbPreset";
            cmbPreset.Size = new System.Drawing.Size(250, 23);
            cmbPreset.TabIndex = 3;
            cmbPreset.SelectedIndexChanged += CmbPreset_SelectedIndexChanged;
            //
            // lblWidth
            //
            lblWidth.AutoSize = true;
            lblWidth.Location = new System.Drawing.Point(14, 81);
            lblWidth.Name = "lblWidth";
            lblWidth.Size = new System.Drawing.Size(45, 15);
            lblWidth.TabIndex = 4;
            lblWidth.Text = "Breite:";
            //
            // numWidth
            //
            numWidth.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numWidth.Location = new System.Drawing.Point(120, 79);
            numWidth.Maximum = new decimal(new int[] { 16384, 0, 0, 0 });
            numWidth.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numWidth.Name = "numWidth";
            numWidth.Size = new System.Drawing.Size(100, 23);
            numWidth.TabIndex = 5;
            numWidth.Value = new decimal(new int[] { 6144, 0, 0, 0 });
            //
            // lblHeight
            //
            lblHeight.AutoSize = true;
            lblHeight.Location = new System.Drawing.Point(14, 112);
            lblHeight.Name = "lblHeight";
            lblHeight.Size = new System.Drawing.Size(40, 15);
            lblHeight.TabIndex = 6;
            lblHeight.Text = "Höhe:";
            //
            // numHeight
            //
            numHeight.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numHeight.Location = new System.Drawing.Point(120, 110);
            numHeight.Maximum = new decimal(new int[] { 16384, 0, 0, 0 });
            numHeight.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numHeight.Name = "numHeight";
            numHeight.Size = new System.Drawing.Size(100, 23);
            numHeight.TabIndex = 7;
            numHeight.Value = new decimal(new int[] { 4096, 0, 0, 0 });
            //
            // lblFill
            //
            lblFill.AutoSize = true;
            lblFill.Location = new System.Drawing.Point(14, 143);
            lblFill.Name = "lblFill";
            lblFill.Size = new System.Drawing.Size(97, 15);
            lblFill.TabIndex = 8;
            lblFill.Text = "Füllfarbe (Index):";
            //
            // numFill
            //
            numFill.Hexadecimal = true;
            numFill.Location = new System.Drawing.Point(120, 141);
            numFill.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numFill.Name = "numFill";
            numFill.Size = new System.Drawing.Size(60, 23);
            numFill.TabIndex = 9;
            numFill.Value = new decimal(new int[] { 32, 0, 0, 0 });
            numFill.ValueChanged += NumFill_ValueChanged;
            //
            // chkBits16
            //
            chkBits16.AutoSize = true;
            chkBits16.Location = new System.Drawing.Point(120, 172);
            chkBits16.Name = "chkBits16";
            chkBits16.Size = new System.Drawing.Size(230, 19);
            chkBits16.TabIndex = 12;
            chkBits16.Text = "16 Bit (bis 65 536 Farben, PNG)";
            chkBits16.UseVisualStyleBackColor = true;
            chkBits16.CheckedChanged += ChkBits16_CheckedChanged;
            //
            // panelFillSwatch
            //
            panelFillSwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelFillSwatch.Location = new System.Drawing.Point(188, 141);
            panelFillSwatch.Name = "panelFillSwatch";
            panelFillSwatch.Size = new System.Drawing.Size(32, 23);
            panelFillSwatch.TabIndex = 10;
            //
            // lblFillName
            //
            lblFillName.AutoSize = true;
            lblFillName.Location = new System.Drawing.Point(228, 145);
            lblFillName.Name = "lblFillName";
            lblFillName.Size = new System.Drawing.Size(12, 15);
            lblFillName.TabIndex = 11;
            lblFillName.Text = "-";
            //
            // lblHint
            //
            lblHint.ForeColor = System.Drawing.SystemColors.GrayText;
            lblHint.Location = new System.Drawing.Point(14, 199);
            lblHint.Name = "lblHint";
            lblHint.Size = new System.Drawing.Size(356, 34);
            lblHint.TabIndex = 12;
            lblHint.Text = "Breite und Höhe werden auf Vielfache von 8 gerundet (UO speichert 8×8-Blöcke). 1 Pixel = 1 Tile.";
            //
            // btnOk
            //
            btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOk.Location = new System.Drawing.Point(214, 242);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(75, 27);
            btnOk.TabIndex = 13;
            btnOk.Text = "Anlegen";
            btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(295, 242);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 27);
            btnCancel.TabIndex = 14;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // NewMapDialog
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(384, 281);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblPreset);
            Controls.Add(cmbPreset);
            Controls.Add(lblWidth);
            Controls.Add(numWidth);
            Controls.Add(lblHeight);
            Controls.Add(numHeight);
            Controls.Add(lblFill);
            Controls.Add(numFill);
            Controls.Add(chkBits16);
            Controls.Add(panelFillSwatch);
            Controls.Add(lblFillName);
            Controls.Add(lblHint);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "NewMapDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Neue Karte";
            ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFill).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblPreset;
        private System.Windows.Forms.ComboBox cmbPreset;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.Label lblFill;
        private System.Windows.Forms.NumericUpDown numFill;
        private System.Windows.Forms.CheckBox chkBits16;
        private System.Windows.Forms.Panel panelFillSwatch;
        private System.Windows.Forms.Label lblFillName;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
