namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class DecriptClientForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.btnSelectClient = new System.Windows.Forms.Button();
            this.btnDecrypt = new System.Windows.Forms.Button();
            this.txtClientPath = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.groupPatches = new System.Windows.Forms.GroupBox();
            this.lblLogin = new System.Windows.Forms.Label();
            this.lblBlowfish = new System.Windows.Forms.Label();
            this.lblTwofish = new System.Windows.Forms.Label();
            this.lblMD5 = new System.Windows.Forms.Label();
            this.lblMultiClient = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblLog = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.chkAutoOpenFolder = new System.Windows.Forms.CheckBox();
            this.groupPatches.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSelectClient
            // 
            this.btnSelectClient.Location = new System.Drawing.Point(12, 12);
            this.btnSelectClient.Name = "btnSelectClient";
            this.btnSelectClient.Size = new System.Drawing.Size(110, 28);
            this.btnSelectClient.TabIndex = 0;
            this.btnSelectClient.Text = "Client wählen...";
            this.btnSelectClient.UseVisualStyleBackColor = true;
            this.btnSelectClient.Click += new System.EventHandler(this.btnSelectClient_Click);
            // 
            // btnDecrypt
            // 
            this.btnDecrypt.Enabled = false;
            this.btnDecrypt.Location = new System.Drawing.Point(128, 12);
            this.btnDecrypt.Name = "btnDecrypt";
            this.btnDecrypt.Size = new System.Drawing.Size(130, 28);
            this.btnDecrypt.TabIndex = 1;
            this.btnDecrypt.Text = "Decrypt & Patch";
            this.btnDecrypt.UseVisualStyleBackColor = true;
            this.btnDecrypt.Click += new System.EventHandler(this.btnDecrypt_Click);
            // 
            // txtClientPath
            // 
            this.txtClientPath.AllowDrop = true;
            this.txtClientPath.Location = new System.Drawing.Point(12, 48);
            this.txtClientPath.Name = "txtClientPath";
            this.txtClientPath.ReadOnly = true;
            this.txtClientPath.Size = new System.Drawing.Size(560, 23);
            this.txtClientPath.TabIndex = 2;
            this.txtClientPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtClientPath_DragDrop);
            this.txtClientPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtClientPath_DragEnter);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatus.Location = new System.Drawing.Point(12, 80);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(48, 15);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Status:";
            // 
            // groupPatches
            // 
            this.groupPatches.Controls.Add(this.lblLogin);
            this.groupPatches.Controls.Add(this.lblBlowfish);
            this.groupPatches.Controls.Add(this.lblTwofish);
            this.groupPatches.Controls.Add(this.lblMD5);
            this.groupPatches.Controls.Add(this.lblMultiClient);
            this.groupPatches.Location = new System.Drawing.Point(12, 110);
            this.groupPatches.Name = "groupPatches";
            this.groupPatches.Size = new System.Drawing.Size(280, 160);
            this.groupPatches.TabIndex = 4;
            this.groupPatches.TabStop = false;
            this.groupPatches.Text = "Patch Status";
            // 
            // lblLogin
            // 
            this.lblLogin.AutoSize = true;
            this.lblLogin.Location = new System.Drawing.Point(15, 28);
            this.lblLogin.Name = "lblLogin";
            this.lblLogin.Size = new System.Drawing.Size(90, 15);
            this.lblLogin.TabIndex = 0;
            this.lblLogin.Text = "Login Encryption: -";
            // 
            // lblBlowfish
            // 
            this.lblBlowfish.AutoSize = true;
            this.lblBlowfish.Location = new System.Drawing.Point(15, 52);
            this.lblBlowfish.Name = "lblBlowfish";
            this.lblBlowfish.Size = new System.Drawing.Size(95, 15);
            this.lblBlowfish.TabIndex = 1;
            this.lblBlowfish.Text = "Blowfish: -";
            // 
            // lblTwofish
            // 
            this.lblTwofish.AutoSize = true;
            this.lblTwofish.Location = new System.Drawing.Point(15, 76);
            this.lblTwofish.Name = "lblTwofish";
            this.lblTwofish.Size = new System.Drawing.Size(90, 15);
            this.lblTwofish.TabIndex = 2;
            this.lblTwofish.Text = "Twofish: -";
            // 
            // lblMD5
            // 
            this.lblMD5.AutoSize = true;
            this.lblMD5.Location = new System.Drawing.Point(15, 100);
            this.lblMD5.Name = "lblMD5";
            this.lblMD5.Size = new System.Drawing.Size(95, 15);
            this.lblMD5.TabIndex = 3;
            this.lblMD5.Text = "MD5 Decrypt: -";
            // 
            // lblMultiClient
            // 
            this.lblMultiClient.AutoSize = true;
            this.lblMultiClient.Location = new System.Drawing.Point(15, 124);
            this.lblMultiClient.Name = "lblMultiClient";
            this.lblMultiClient.Size = new System.Drawing.Size(105, 15);
            this.lblMultiClient.TabIndex = 4;
            this.lblMultiClient.Text = "Multi-Client: -";
            // 
            // txtLog
            // 
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.Location = new System.Drawing.Point(310, 130);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(262, 250);
            this.txtLog.TabIndex = 5;
            // 
            // lblLog
            // 
            this.lblLog.AutoSize = true;
            this.lblLog.Location = new System.Drawing.Point(310, 110);
            this.lblLog.Name = "lblLog";
            this.lblLog.Size = new System.Drawing.Size(28, 15);
            this.lblLog.TabIndex = 6;
            this.lblLog.Text = "Log";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 390);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(560, 18);
            this.progressBar.TabIndex = 7;
            // 
            // chkAutoOpenFolder
            // 
            this.chkAutoOpenFolder.AutoSize = true;
            this.chkAutoOpenFolder.Checked = true;
            this.chkAutoOpenFolder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoOpenFolder.Location = new System.Drawing.Point(12, 280);
            this.chkAutoOpenFolder.Name = "chkAutoOpenFolder";
            this.chkAutoOpenFolder.Size = new System.Drawing.Size(220, 19);
            this.chkAutoOpenFolder.TabIndex = 8;
            this.chkAutoOpenFolder.Text = "Ordner nach Fertigstellung öffnen";
            this.chkAutoOpenFolder.UseVisualStyleBackColor = true;
            // 
            // DecriptClientForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 421);
            this.Controls.Add(this.chkAutoOpenFolder);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblLog);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.groupPatches);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtClientPath);
            this.Controls.Add(this.btnDecrypt);
            this.Controls.Add(this.btnSelectClient);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DecriptClientForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "UO Client Decryptor & Multi-Client Patcher";
            this.TopMost = true;
            this.groupPatches.ResumeLayout(false);
            this.groupPatches.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnSelectClient;
        private System.Windows.Forms.Button btnDecrypt;
        private System.Windows.Forms.TextBox txtClientPath;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox groupPatches;
        private System.Windows.Forms.Label lblLogin;
        private System.Windows.Forms.Label lblBlowfish;
        private System.Windows.Forms.Label lblTwofish;
        private System.Windows.Forms.Label lblMD5;
        private System.Windows.Forms.Label lblMultiClient;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblLog;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.CheckBox chkAutoOpenFolder;
        private System.Windows.Forms.CheckBox chkLogin;
        private System.Windows.Forms.CheckBox chkBlowfish;
        private System.Windows.Forms.CheckBox chkTwofish;
        private System.Windows.Forms.CheckBox chkMD5;
        private System.Windows.Forms.CheckBox chkMultiClient;
    }
}