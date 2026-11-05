namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    partial class SaveTemplateDialog
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
            lblCategory = new System.Windows.Forms.Label();
            cmbCategory = new System.Windows.Forms.ComboBox();
            lblFormat = new System.Windows.Forms.Label();
            cmbFormat = new System.Windows.Forms.ComboBox();
            chkLand = new System.Windows.Forms.CheckBox();
            chkStatics = new System.Windows.Forms.CheckBox();
            lblInfo = new System.Windows.Forms.Label();
            picPreview = new System.Windows.Forms.PictureBox();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            SuspendLayout();
            //
            // lblName
            //
            lblName.AutoSize = true;
            lblName.Location = new System.Drawing.Point(12, 15);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(42, 15);
            lblName.TabIndex = 0;
            lblName.Text = "Name:";
            //
            // txtName
            //
            txtName.Location = new System.Drawing.Point(90, 12);
            txtName.Name = "txtName";
            txtName.Size = new System.Drawing.Size(300, 23);
            txtName.TabIndex = 1;
            txtName.TextChanged += Content_Changed;
            //
            // lblCategory
            //
            lblCategory.AutoSize = true;
            lblCategory.Location = new System.Drawing.Point(12, 44);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new System.Drawing.Size(60, 15);
            lblCategory.TabIndex = 2;
            lblCategory.Text = "Kategorie:";
            //
            // cmbCategory
            //
            cmbCategory.Location = new System.Drawing.Point(90, 41);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.Size = new System.Drawing.Size(300, 23);
            cmbCategory.TabIndex = 3;
            //
            // lblFormat
            //
            lblFormat.AutoSize = true;
            lblFormat.Location = new System.Drawing.Point(12, 73);
            lblFormat.Name = "lblFormat";
            lblFormat.Size = new System.Drawing.Size(48, 15);
            lblFormat.TabIndex = 4;
            lblFormat.Text = "Format:";
            //
            // cmbFormat
            //
            cmbFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbFormat.Location = new System.Drawing.Point(90, 70);
            cmbFormat.Name = "cmbFormat";
            cmbFormat.Size = new System.Drawing.Size(300, 23);
            cmbFormat.TabIndex = 5;
            cmbFormat.SelectedIndexChanged += Content_Changed;
            //
            // chkLand
            //
            chkLand.AutoSize = true;
            chkLand.Location = new System.Drawing.Point(90, 101);
            chkLand.Name = "chkLand";
            chkLand.Size = new System.Drawing.Size(170, 19);
            chkLand.TabIndex = 6;
            chkLand.Text = "Land-Tiles (Boden) mit Höhen";
            chkLand.UseVisualStyleBackColor = true;
            chkLand.CheckedChanged += Content_Changed;
            //
            // chkStatics
            //
            chkStatics.AutoSize = true;
            chkStatics.Location = new System.Drawing.Point(90, 124);
            chkStatics.Name = "chkStatics";
            chkStatics.Size = new System.Drawing.Size(62, 19);
            chkStatics.TabIndex = 7;
            chkStatics.Text = "Statics";
            chkStatics.UseVisualStyleBackColor = true;
            chkStatics.CheckedChanged += Content_Changed;
            //
            // lblInfo
            //
            lblInfo.Location = new System.Drawing.Point(12, 150);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new System.Drawing.Size(378, 48);
            lblInfo.TabIndex = 8;
            lblInfo.Text = "-";
            //
            // picPreview
            //
            picPreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picPreview.Location = new System.Drawing.Point(400, 12);
            picPreview.Name = "picPreview";
            picPreview.Size = new System.Drawing.Size(220, 186);
            picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picPreview.TabIndex = 9;
            picPreview.TabStop = false;
            //
            // btnOk
            //
            btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOk.Location = new System.Drawing.Point(444, 208);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(85, 27);
            btnOk.TabIndex = 10;
            btnOk.Text = "Speichern";
            btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(535, 208);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(85, 27);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // SaveTemplateDialog
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(632, 245);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblCategory);
            Controls.Add(cmbCategory);
            Controls.Add(lblFormat);
            Controls.Add(cmbFormat);
            Controls.Add(chkLand);
            Controls.Add(chkStatics);
            Controls.Add(lblInfo);
            Controls.Add(picPreview);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SaveTemplateDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Als Vorlage speichern";
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblFormat;
        private System.Windows.Forms.ComboBox cmbFormat;
        private System.Windows.Forms.CheckBox chkLand;
        private System.Windows.Forms.CheckBox chkStatics;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
