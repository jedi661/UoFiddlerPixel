namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    partial class GroupDialog
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
            lblId = new System.Windows.Forms.Label();
            txtId = new System.Windows.Forms.TextBox();
            lblIdInfo = new System.Windows.Forms.Label();
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            lblComment = new System.Windows.Forms.Label();
            txtComment = new System.Windows.Forms.TextBox();
            lblExisting = new System.Windows.Forms.Label();
            lstGroups = new System.Windows.Forms.ListBox();
            lblHelp = new System.Windows.Forms.Label();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // lblId
            //
            lblId.AutoSize = true;
            lblId.Location = new System.Drawing.Point(12, 15);
            lblId.Text = "Nummer (hex):";
            lblId.Name = "lblId";
            lblId.TabIndex = 0;
            //
            // txtId
            //
            txtId.Location = new System.Drawing.Point(120, 12);
            txtId.Size = new System.Drawing.Size(80, 23);
            txtId.Name = "txtId";
            txtId.TabIndex = 1;
            txtId.TextChanged += Values_Changed;
            //
            // lblIdInfo
            //
            lblIdInfo.AutoSize = true;
            lblIdInfo.Location = new System.Drawing.Point(208, 15);
            lblIdInfo.Text = "-";
            lblIdInfo.Name = "lblIdInfo";
            lblIdInfo.TabIndex = 2;
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
            txtName.Location = new System.Drawing.Point(120, 41);
            txtName.Size = new System.Drawing.Size(330, 23);
            txtName.Name = "txtName";
            txtName.TabIndex = 4;
            txtName.TextChanged += Values_Changed;
            //
            // lblComment
            //
            lblComment.AutoSize = true;
            lblComment.Location = new System.Drawing.Point(12, 73);
            lblComment.Text = "Kommentar:";
            lblComment.Name = "lblComment";
            lblComment.TabIndex = 5;
            //
            // txtComment
            //
            txtComment.Location = new System.Drawing.Point(120, 70);
            txtComment.Size = new System.Drawing.Size(330, 23);
            txtComment.Name = "txtComment";
            txtComment.TabIndex = 6;
            //
            // lblExisting
            //
            lblExisting.AutoSize = true;
            lblExisting.Location = new System.Drawing.Point(12, 102);
            lblExisting.Text = "Vorhandene Gruppen:";
            lblExisting.Name = "lblExisting";
            lblExisting.TabIndex = 7;
            //
            // lstGroups
            //
            lstGroups.IntegralHeight = false;
            lstGroups.Location = new System.Drawing.Point(12, 120);
            lstGroups.Size = new System.Drawing.Size(438, 110);
            lstGroups.Name = "lstGroups";
            lstGroups.TabIndex = 8;
            //
            // lblHelp
            //
            lblHelp.Location = new System.Drawing.Point(12, 236);
            lblHelp.Size = new System.Drawing.Size(438, 52);
            lblHelp.Text = "-";
            lblHelp.Name = "lblHelp";
            lblHelp.TabIndex = 9;
            //
            // btnOk
            //
            btnOk.Location = new System.Drawing.Point(274, 293);
            btnOk.Size = new System.Drawing.Size(85, 27);
            btnOk.Text = "Anlegen";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Name = "btnOk";
            btnOk.TabIndex = 10;
            btnOk.Click += BtnOk_Click;
            //
            // btnCancel
            //
            btnCancel.Location = new System.Drawing.Point(365, 293);
            btnCancel.Size = new System.Drawing.Size(85, 27);
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Name = "btnCancel";
            btnCancel.TabIndex = 11;
            //
            // GroupDialog
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(470, 330);
            Controls.Add(lblId);
            Controls.Add(txtId);
            Controls.Add(lblIdInfo);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblComment);
            Controls.Add(txtComment);
            Controls.Add(lblExisting);
            Controls.Add(lstGroups);
            Controls.Add(lblHelp);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GroupDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Neue Gruppe";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.Label lblIdInfo;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Label lblExisting;
        private System.Windows.Forms.ListBox lstGroups;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}
