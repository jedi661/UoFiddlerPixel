namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    partial class GumpWriteBackForm
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
            components = new System.ComponentModel.Container();
            splitMain = new System.Windows.Forms.SplitContainer();
            listChanges = new System.Windows.Forms.ListView();
            listProblems = new System.Windows.Forms.ListBox();
            lblProblems = new System.Windows.Forms.Label();
            lblInfo = new System.Windows.Forms.Label();
            flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnCancel = new System.Windows.Forms.Button();
            btnWrite = new System.Windows.Forms.Button();
            chkBackup = new System.Windows.Forms.CheckBox();
            chkReimport = new System.Windows.Forms.CheckBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            flowButtons.SuspendLayout();
            SuspendLayout();
            //
            // splitMain
            //
            splitMain.Panel1.Controls.Add(listChanges);
            splitMain.Panel2.Controls.Add(listProblems);
            splitMain.Panel2.Controls.Add(lblProblems);
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.Size = new System.Drawing.Size(900, 470);
            splitMain.SplitterDistance = 300;
            splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            splitMain.Name = "splitMain";
            splitMain.TabIndex = 0;
            //
            // listChanges
            //
            listChanges.Dock = System.Windows.Forms.DockStyle.Fill;
            listChanges.FullRowSelect = true;
            listChanges.GridLines = true;
            listChanges.HideSelection = false;
            listChanges.UseCompatibleStateImageBehavior = false;
            listChanges.View = System.Windows.Forms.View.Details;
            listChanges.Name = "listChanges";
            listChanges.TabIndex = 1;
            //
            // listProblems
            //
            listProblems.Dock = System.Windows.Forms.DockStyle.Fill;
            listProblems.HorizontalScrollbar = true;
            listProblems.IntegralHeight = false;
            listProblems.Name = "listProblems";
            listProblems.TabIndex = 2;
            //
            // lblProblems
            //
            lblProblems.Dock = System.Windows.Forms.DockStyle.Top;
            lblProblems.Padding = new System.Windows.Forms.Padding(3, 3, 0, 0);
            lblProblems.Size = new System.Drawing.Size(900, 20);
            lblProblems.Text = "Not written (change these by hand) / Nicht geschrieben (von Hand ändern):";
            lblProblems.Name = "lblProblems";
            lblProblems.TabIndex = 3;
            //
            // lblInfo
            //
            lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
            lblInfo.Padding = new System.Windows.Forms.Padding(6);
            lblInfo.Size = new System.Drawing.Size(900, 44);
            lblInfo.Text = "";
            lblInfo.Name = "lblInfo";
            lblInfo.TabIndex = 4;
            //
            // flowButtons
            //
            flowButtons.Controls.Add(btnCancel);
            flowButtons.Controls.Add(btnWrite);
            flowButtons.Controls.Add(chkBackup);
            flowButtons.Controls.Add(chkReimport);
            flowButtons.AutoSize = true;
            flowButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowButtons.Padding = new System.Windows.Forms.Padding(4);
            flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowButtons.Name = "flowButtons";
            flowButtons.TabIndex = 5;
            //
            // btnCancel
            //
            btnCancel.AutoSize = true;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Name = "btnCancel";
            btnCancel.TabIndex = 6;
            //
            // btnWrite
            //
            btnWrite.AutoSize = true;
            btnWrite.Text = "Write into the script";
            btnWrite.UseVisualStyleBackColor = true;
            btnWrite.Name = "btnWrite";
            btnWrite.TabIndex = 7;
            btnWrite.Click += BtnWrite_Click;
            //
            // chkBackup
            //
            chkBackup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkBackup.AutoSize = true;
            chkBackup.Text = "Backup (.bak)";
            chkBackup.Checked = true;
            chkBackup.CheckState = System.Windows.Forms.CheckState.Checked;
            chkBackup.Name = "chkBackup";
            chkBackup.TabIndex = 8;
            toolTip1.SetToolTip(chkBackup, "Keeps the old file as <name>.cs.bak\n\nDE: Behält die alte Datei als <name>.cs.bak");
            //
            // chkReimport
            //
            chkReimport.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkReimport.AutoSize = true;
            chkReimport.Text = "Import the changed script again";
            chkReimport.Checked = true;
            chkReimport.CheckState = System.Windows.Forms.CheckState.Checked;
            chkReimport.Name = "chkReimport";
            chkReimport.TabIndex = 9;
            toolTip1.SetToolTip(chkReimport, "Afterwards the studio reads the new file, so you can go on changing it\n\nDE: Danach liest das Studio die neue Datei, damit du weiter ändern kannst");
            //
            // toolTip1
            //
            toolTip1.AutoPopDelay = 15000;
            //
            // GumpWriteBackForm
            //
            AcceptButton = btnWrite;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(900, 560);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(600, 400);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Write back into the script / Ins Skript zurückschreiben";
            Controls.Add(splitMain);
            Controls.Add(lblInfo);
            Controls.Add(flowButtons);
            Name = "GumpWriteBackForm";
            flowButtons.ResumeLayout(false);
            flowButtons.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.ListView listChanges;
        private System.Windows.Forms.ListBox listProblems;
        private System.Windows.Forms.Label lblProblems;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnWrite;
        private System.Windows.Forms.CheckBox chkBackup;
        private System.Windows.Forms.CheckBox chkReimport;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
