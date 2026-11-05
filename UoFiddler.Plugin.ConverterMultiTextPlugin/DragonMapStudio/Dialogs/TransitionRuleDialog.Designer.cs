namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    partial class TransitionRuleDialog
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
            lblLayer = new System.Windows.Forms.Label();
            cmbLayer = new System.Windows.Forms.ComboBox();
            lblGroupA = new System.Windows.Forms.Label();
            cmbGroupA = new System.Windows.Forms.ComboBox();
            lblGroupB = new System.Windows.Forms.Label();
            cmbGroupB = new System.Windows.Forms.ComboBox();
            lblFile = new System.Windows.Forms.Label();
            txtFile = new System.Windows.Forms.TextBox();
            chkExample = new System.Windows.Forms.CheckBox();
            lblFileInfo = new System.Windows.Forms.Label();
            picPatterns = new System.Windows.Forms.PictureBox();
            lblHelp = new System.Windows.Forms.Label();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)picPatterns).BeginInit();
            SuspendLayout();
            //
            // lblLayer
            //
            lblLayer.AutoSize = true;
            lblLayer.Location = new System.Drawing.Point(12, 15);
            lblLayer.Text = "Ebene:";
            lblLayer.Name = "lblLayer";
            lblLayer.TabIndex = 0;
            //
            // cmbLayer
            //
            cmbLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbLayer.Location = new System.Drawing.Point(130, 12);
            cmbLayer.Size = new System.Drawing.Size(300, 23);
            cmbLayer.Name = "cmbLayer";
            cmbLayer.TabIndex = 1;
            cmbLayer.SelectedIndexChanged += Values_Changed;
            //
            // lblGroupA
            //
            lblGroupA.AutoSize = true;
            lblGroupA.Location = new System.Drawing.Point(12, 44);
            lblGroupA.Text = "Gruppe A (Zelle):";
            lblGroupA.Name = "lblGroupA";
            lblGroupA.TabIndex = 2;
            //
            // cmbGroupA
            //
            cmbGroupA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbGroupA.Location = new System.Drawing.Point(130, 41);
            cmbGroupA.Size = new System.Drawing.Size(300, 23);
            cmbGroupA.Name = "cmbGroupA";
            cmbGroupA.TabIndex = 3;
            cmbGroupA.SelectedIndexChanged += Values_Changed;
            //
            // lblGroupB
            //
            lblGroupB.AutoSize = true;
            lblGroupB.Location = new System.Drawing.Point(12, 73);
            lblGroupB.Text = "Gruppe B (Nachbar):";
            lblGroupB.Name = "lblGroupB";
            lblGroupB.TabIndex = 4;
            //
            // cmbGroupB
            //
            cmbGroupB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbGroupB.Location = new System.Drawing.Point(130, 70);
            cmbGroupB.Size = new System.Drawing.Size(300, 23);
            cmbGroupB.Name = "cmbGroupB";
            cmbGroupB.TabIndex = 5;
            cmbGroupB.SelectedIndexChanged += Values_Changed;
            //
            // lblFile
            //
            lblFile.AutoSize = true;
            lblFile.Location = new System.Drawing.Point(12, 102);
            lblFile.Text = "Datei:";
            lblFile.Name = "lblFile";
            lblFile.TabIndex = 6;
            //
            // txtFile
            //
            txtFile.Location = new System.Drawing.Point(130, 99);
            txtFile.Size = new System.Drawing.Size(300, 23);
            txtFile.Name = "txtFile";
            txtFile.TabIndex = 7;
            txtFile.TextChanged += File_Changed;
            //
            // chkExample
            //
            chkExample.AutoSize = true;
            chkExample.Checked = true;
            chkExample.CheckState = System.Windows.Forms.CheckState.Checked;
            chkExample.Location = new System.Drawing.Point(130, 128);
            chkExample.Text = "Neue Datei mit den 12 Standard-Mustern anlegen (Beispiel)";
            chkExample.Name = "chkExample";
            chkExample.TabIndex = 8;
            //
            // lblFileInfo
            //
            lblFileInfo.Location = new System.Drawing.Point(130, 152);
            lblFileInfo.Size = new System.Drawing.Size(300, 34);
            lblFileInfo.Text = "-";
            lblFileInfo.Name = "lblFileInfo";
            lblFileInfo.TabIndex = 9;
            //
            // picPatterns
            //
            picPatterns.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picPatterns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picPatterns.Location = new System.Drawing.Point(440, 12);
            picPatterns.Size = new System.Drawing.Size(268, 300);
            picPatterns.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picPatterns.Name = "picPatterns";
            picPatterns.TabIndex = 10;
            //
            // lblHelp
            //
            lblHelp.Location = new System.Drawing.Point(12, 320);
            lblHelp.Size = new System.Drawing.Size(696, 150);
            lblHelp.Text = "-";
            lblHelp.Name = "lblHelp";
            lblHelp.TabIndex = 11;
            //
            // btnOk
            //
            btnOk.Location = new System.Drawing.Point(532, 481);
            btnOk.Size = new System.Drawing.Size(85, 27);
            btnOk.Text = "Anlegen";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Name = "btnOk";
            btnOk.TabIndex = 12;
            btnOk.Click += BtnOk_Click;
            //
            // btnCancel
            //
            btnCancel.Location = new System.Drawing.Point(623, 481);
            btnCancel.Size = new System.Drawing.Size(85, 27);
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Name = "btnCancel";
            btnCancel.TabIndex = 13;
            //
            // TransitionRuleDialog
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(720, 520);
            Controls.Add(lblLayer);
            Controls.Add(cmbLayer);
            Controls.Add(lblGroupA);
            Controls.Add(cmbGroupA);
            Controls.Add(lblGroupB);
            Controls.Add(cmbGroupB);
            Controls.Add(lblFile);
            Controls.Add(txtFile);
            Controls.Add(chkExample);
            Controls.Add(lblFileInfo);
            Controls.Add(picPatterns);
            Controls.Add(lblHelp);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TransitionRuleDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Neue Übergangsregel";
            ((System.ComponentModel.ISupportInitialize)picPatterns).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblLayer;
        private System.Windows.Forms.ComboBox cmbLayer;
        private System.Windows.Forms.Label lblGroupA;
        private System.Windows.Forms.ComboBox cmbGroupA;
        private System.Windows.Forms.Label lblGroupB;
        private System.Windows.Forms.ComboBox cmbGroupB;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.CheckBox chkExample;
        private System.Windows.Forms.Label lblFileInfo;
        private System.Windows.Forms.PictureBox picPatterns;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
