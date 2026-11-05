namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    partial class ColorDefinitionDialog
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
            lblAddress = new System.Windows.Forms.Label();
            txtAddress = new System.Windows.Forms.TextBox();
            lblAddressInfo = new System.Windows.Forms.Label();
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            lblCategory = new System.Windows.Forms.Label();
            cmbCategory = new System.Windows.Forms.ComboBox();
            lblColor = new System.Windows.Forms.Label();
            panelColor = new System.Windows.Forms.Panel();
            btnColor = new System.Windows.Forms.Button();
            btnUnique = new System.Windows.Forms.Button();
            lblColorInfo = new System.Windows.Forms.Label();
            lblGroup = new System.Windows.Forms.Label();
            cmbGroup = new System.Windows.Forms.ComboBox();
            btnNewGroup = new System.Windows.Forms.Button();
            lblAltitude = new System.Windows.Forms.Label();
            numAltitude = new System.Windows.Forms.NumericUpDown();
            lblTiles = new System.Windows.Forms.Label();
            txtTiles = new System.Windows.Forms.TextBox();
            btnCatalog = new System.Windows.Forms.Button();
            flowTiles = new System.Windows.Forms.FlowLayoutPanel();
            lblComment = new System.Windows.Forms.Label();
            txtComment = new System.Windows.Forms.TextBox();
            lblHelp = new System.Windows.Forms.Label();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            picPreview = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)numAltitude).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            panelColor.SuspendLayout();
            flowTiles.SuspendLayout();
            SuspendLayout();
            //
            // lblAddress
            //
            lblAddress.AutoSize = true;
            lblAddress.Location = new System.Drawing.Point(12, 15);
            lblAddress.Text = "Adresse (hex):";
            lblAddress.Name = "lblAddress";
            lblAddress.TabIndex = 0;
            //
            // txtAddress
            //
            txtAddress.Location = new System.Drawing.Point(130, 12);
            txtAddress.Size = new System.Drawing.Size(80, 23);
            txtAddress.Name = "txtAddress";
            txtAddress.TabIndex = 1;
            txtAddress.TextChanged += Values_Changed;
            //
            // lblAddressInfo
            //
            lblAddressInfo.AutoSize = true;
            lblAddressInfo.Location = new System.Drawing.Point(218, 15);
            lblAddressInfo.Text = "-";
            lblAddressInfo.Name = "lblAddressInfo";
            lblAddressInfo.TabIndex = 2;
            //
            // lblName
            //
            lblName.AutoSize = true;
            lblName.Location = new System.Drawing.Point(12, 44);
            lblName.Text = "Name:";
            lblName.Name = "lblName";
            lblName.TabIndex = 3;
            //
            // txtName
            //
            txtName.Location = new System.Drawing.Point(130, 41);
            txtName.Size = new System.Drawing.Size(250, 23);
            txtName.Name = "txtName";
            txtName.TabIndex = 4;
            txtName.TextChanged += Values_Changed;
            //
            // lblCategory
            //
            lblCategory.AutoSize = true;
            lblCategory.Location = new System.Drawing.Point(12, 73);
            lblCategory.Text = "Kategorie:";
            lblCategory.Name = "lblCategory";
            lblCategory.TabIndex = 5;
            //
            // cmbCategory
            //
            cmbCategory.Location = new System.Drawing.Point(130, 70);
            cmbCategory.Size = new System.Drawing.Size(250, 23);
            cmbCategory.Name = "cmbCategory";
            cmbCategory.TabIndex = 6;
            //
            // lblColor
            //
            lblColor.AutoSize = true;
            lblColor.Location = new System.Drawing.Point(12, 104);
            lblColor.Text = "Farbe (RGB):";
            lblColor.Name = "lblColor";
            lblColor.TabIndex = 7;
            //
            // panelColor
            //
            panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelColor.Cursor = System.Windows.Forms.Cursors.Hand;
            panelColor.Location = new System.Drawing.Point(130, 99);
            panelColor.Size = new System.Drawing.Size(44, 27);
            panelColor.Name = "panelColor";
            panelColor.TabIndex = 8;
            panelColor.Click += BtnColor_Click;
            //
            // btnColor
            //
            btnColor.Location = new System.Drawing.Point(180, 99);
            btnColor.Size = new System.Drawing.Size(80, 27);
            btnColor.Text = "Wählen…";
            btnColor.UseVisualStyleBackColor = true;
            btnColor.Name = "btnColor";
            btnColor.TabIndex = 9;
            btnColor.Click += BtnColor_Click;
            //
            // btnUnique
            //
            btnUnique.Location = new System.Drawing.Point(266, 99);
            btnUnique.Size = new System.Drawing.Size(114, 27);
            btnUnique.Text = "Eindeutig machen";
            btnUnique.UseVisualStyleBackColor = true;
            btnUnique.Name = "btnUnique";
            btnUnique.TabIndex = 10;
            btnUnique.Click += BtnUnique_Click;
            //
            // lblColorInfo
            //
            lblColorInfo.Location = new System.Drawing.Point(130, 130);
            lblColorInfo.Size = new System.Drawing.Size(250, 34);
            lblColorInfo.Text = "-";
            lblColorInfo.Name = "lblColorInfo";
            lblColorInfo.TabIndex = 11;
            //
            // lblGroup
            //
            lblGroup.AutoSize = true;
            lblGroup.Location = new System.Drawing.Point(12, 170);
            lblGroup.Text = "Gruppe:";
            lblGroup.Name = "lblGroup";
            lblGroup.TabIndex = 12;
            //
            // cmbGroup
            //
            cmbGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbGroup.Location = new System.Drawing.Point(130, 167);
            cmbGroup.Size = new System.Drawing.Size(250, 23);
            cmbGroup.Name = "cmbGroup";
            cmbGroup.TabIndex = 13;
            cmbGroup.SelectedIndexChanged += Values_Changed;
            //
            // btnNewGroup
            //
            btnNewGroup.Location = new System.Drawing.Point(386, 165);
            btnNewGroup.Size = new System.Drawing.Size(110, 27);
            btnNewGroup.Text = "Neue Gruppe…";
            btnNewGroup.UseVisualStyleBackColor = true;
            btnNewGroup.Name = "btnNewGroup";
            btnNewGroup.TabIndex = 14;
            btnNewGroup.Click += BtnNewGroup_Click;
            //
            // lblAltitude
            //
            lblAltitude.AutoSize = true;
            lblAltitude.Location = new System.Drawing.Point(12, 201);
            lblAltitude.Text = "Höhe (Z):";
            lblAltitude.Name = "lblAltitude";
            lblAltitude.TabIndex = 15;
            //
            // numAltitude
            //
            numAltitude.Location = new System.Drawing.Point(130, 198);
            numAltitude.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numAltitude.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numAltitude.Size = new System.Drawing.Size(70, 23);
            numAltitude.Name = "numAltitude";
            numAltitude.TabIndex = 16;
            //
            // lblTiles
            //
            lblTiles.AutoSize = true;
            lblTiles.Location = new System.Drawing.Point(12, 232);
            lblTiles.Text = "Land-Tiles (hex):";
            lblTiles.Name = "lblTiles";
            lblTiles.TabIndex = 17;
            //
            // txtTiles
            //
            txtTiles.Location = new System.Drawing.Point(130, 229);
            txtTiles.Size = new System.Drawing.Size(366, 23);
            txtTiles.Name = "txtTiles";
            txtTiles.TabIndex = 18;
            txtTiles.TextChanged += Values_Changed;
            //
            // btnCatalog
            //
            btnCatalog.Location = new System.Drawing.Point(502, 227);
            btnCatalog.Size = new System.Drawing.Size(120, 27);
            btnCatalog.Text = "Katalog…";
            btnCatalog.UseVisualStyleBackColor = true;
            btnCatalog.Name = "btnCatalog";
            btnCatalog.TabIndex = 19;
            btnCatalog.Click += BtnCatalog_Click;
            //
            // flowTiles
            //
            flowTiles.AutoScroll = true;
            flowTiles.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            flowTiles.Location = new System.Drawing.Point(130, 258);
            flowTiles.Size = new System.Drawing.Size(492, 62);
            flowTiles.Name = "flowTiles";
            flowTiles.TabIndex = 20;
            //
            // lblComment
            //
            lblComment.AutoSize = true;
            lblComment.Location = new System.Drawing.Point(12, 331);
            lblComment.Text = "Kommentar:";
            lblComment.Name = "lblComment";
            lblComment.TabIndex = 21;
            //
            // txtComment
            //
            txtComment.Location = new System.Drawing.Point(130, 328);
            txtComment.Size = new System.Drawing.Size(492, 23);
            txtComment.Name = "txtComment";
            txtComment.TabIndex = 22;
            //
            // lblHelp
            //
            lblHelp.Location = new System.Drawing.Point(12, 360);
            lblHelp.Size = new System.Drawing.Size(610, 64);
            lblHelp.Text = "-";
            lblHelp.Name = "lblHelp";
            lblHelp.TabIndex = 23;
            //
            // btnOk
            //
            btnOk.Location = new System.Drawing.Point(446, 432);
            btnOk.Size = new System.Drawing.Size(85, 27);
            btnOk.Text = "Übernehmen";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Name = "btnOk";
            btnOk.TabIndex = 24;
            btnOk.Click += BtnOk_Click;
            //
            // btnCancel
            //
            btnCancel.Location = new System.Drawing.Point(537, 432);
            btnCancel.Size = new System.Drawing.Size(85, 27);
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Name = "btnCancel";
            btnCancel.TabIndex = 25;
            //
            // picPreview
            //
            picPreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picPreview.Location = new System.Drawing.Point(502, 12);
            picPreview.Size = new System.Drawing.Size(120, 120);
            picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picPreview.Name = "picPreview";
            picPreview.TabIndex = 26;
            //
            // ColorDefinitionDialog
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(640, 470);
            Controls.Add(lblAddress);
            Controls.Add(txtAddress);
            Controls.Add(lblAddressInfo);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblCategory);
            Controls.Add(cmbCategory);
            Controls.Add(lblColor);
            Controls.Add(panelColor);
            Controls.Add(btnColor);
            Controls.Add(btnUnique);
            Controls.Add(lblColorInfo);
            Controls.Add(lblGroup);
            Controls.Add(cmbGroup);
            Controls.Add(btnNewGroup);
            Controls.Add(lblAltitude);
            Controls.Add(numAltitude);
            Controls.Add(lblTiles);
            Controls.Add(txtTiles);
            Controls.Add(btnCatalog);
            Controls.Add(flowTiles);
            Controls.Add(lblComment);
            Controls.Add(txtComment);
            Controls.Add(lblHelp);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            Controls.Add(picPreview);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ColorDefinitionDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Farbe definieren";
            ((System.ComponentModel.ISupportInitialize)numAltitude).EndInit();
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            flowTiles.ResumeLayout(false);
            flowTiles.PerformLayout();
            panelColor.ResumeLayout(false);
            panelColor.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Label lblAddressInfo;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Button btnColor;
        private System.Windows.Forms.Button btnUnique;
        private System.Windows.Forms.Label lblColorInfo;
        private System.Windows.Forms.Label lblGroup;
        private System.Windows.Forms.ComboBox cmbGroup;
        private System.Windows.Forms.Button btnNewGroup;
        private System.Windows.Forms.Label lblAltitude;
        private System.Windows.Forms.NumericUpDown numAltitude;
        private System.Windows.Forms.Label lblTiles;
        private System.Windows.Forms.TextBox txtTiles;
        private System.Windows.Forms.Button btnCatalog;
        private System.Windows.Forms.FlowLayoutPanel flowTiles;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox picPreview;
    }
}
