namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    partial class GumpImportForm
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
            if (disposing)
            {
                picturePreview.Image?.Dispose();
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
            listClasses = new System.Windows.Forms.ListBox();
            lblClasses = new System.Windows.Forms.Label();
            splitPreview = new System.Windows.Forms.SplitContainer();
            picturePreview = new System.Windows.Forms.PictureBox();
            flowPages = new System.Windows.Forms.FlowLayoutPanel();
            lblPage = new System.Windows.Forms.Label();
            numPage = new System.Windows.Forms.NumericUpDown();
            lblInfo = new System.Windows.Forms.Label();
            listHints = new System.Windows.Forms.ListBox();
            lblHints = new System.Windows.Forms.Label();
            flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnCancel = new System.Windows.Forms.Button();
            btnImport = new System.Windows.Forms.Button();
            chkResolve = new System.Windows.Forms.CheckBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitPreview).BeginInit();
            splitPreview.Panel1.SuspendLayout();
            splitPreview.Panel2.SuspendLayout();
            splitPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            flowPages.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPage).BeginInit();
            flowButtons.SuspendLayout();
            SuspendLayout();
            //
            // splitMain
            //
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitMain.Location = new System.Drawing.Point(0, 0);
            splitMain.Name = "splitMain";
            //
            // splitMain.Panel1
            //
            splitMain.Panel1.Controls.Add(listClasses);
            splitMain.Panel1.Controls.Add(lblClasses);
            splitMain.Panel1MinSize = 120;
            //
            // splitMain.Panel2
            //
            splitMain.Panel2.Controls.Add(splitPreview);
            splitMain.Size = new System.Drawing.Size(984, 538);
            splitMain.SplitterDistance = 220;
            splitMain.TabIndex = 0;
            //
            // listClasses
            //
            listClasses.Dock = System.Windows.Forms.DockStyle.Fill;
            listClasses.IntegralHeight = false;
            listClasses.Location = new System.Drawing.Point(0, 20);
            listClasses.Name = "listClasses";
            listClasses.Size = new System.Drawing.Size(220, 518);
            listClasses.TabIndex = 1;
            toolTip1.SetToolTip(listClasses, "Gump classes in the file – pick the one to import\n\nDE: Gump-Klassen in der Datei – die gewünschte auswählen");
            listClasses.SelectedIndexChanged += ListClasses_SelectedIndexChanged;
            listClasses.DoubleClick += BtnImport_Click;
            //
            // lblClasses
            //
            lblClasses.Dock = System.Windows.Forms.DockStyle.Top;
            lblClasses.Location = new System.Drawing.Point(0, 0);
            lblClasses.Name = "lblClasses";
            lblClasses.Padding = new System.Windows.Forms.Padding(3, 3, 0, 0);
            lblClasses.Size = new System.Drawing.Size(220, 20);
            lblClasses.TabIndex = 0;
            lblClasses.Text = "Gump classes:";
            //
            // splitPreview
            //
            splitPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            splitPreview.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitPreview.Location = new System.Drawing.Point(0, 0);
            splitPreview.Name = "splitPreview";
            splitPreview.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitPreview.Panel1
            //
            splitPreview.Panel1.Controls.Add(picturePreview);
            splitPreview.Panel1.Controls.Add(flowPages);
            //
            // splitPreview.Panel2
            //
            splitPreview.Panel2.Controls.Add(listHints);
            splitPreview.Panel2.Controls.Add(lblHints);
            splitPreview.Size = new System.Drawing.Size(760, 538);
            splitPreview.SplitterDistance = 380;
            splitPreview.TabIndex = 0;
            //
            // picturePreview
            //
            picturePreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picturePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            picturePreview.Location = new System.Drawing.Point(0, 30);
            picturePreview.Name = "picturePreview";
            picturePreview.Size = new System.Drawing.Size(760, 350);
            picturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picturePreview.TabIndex = 1;
            picturePreview.TabStop = false;
            //
            // flowPages
            //
            flowPages.Controls.Add(lblPage);
            flowPages.Controls.Add(numPage);
            flowPages.Controls.Add(lblInfo);
            flowPages.Dock = System.Windows.Forms.DockStyle.Top;
            flowPages.Location = new System.Drawing.Point(0, 0);
            flowPages.Name = "flowPages";
            flowPages.Size = new System.Drawing.Size(760, 30);
            flowPages.TabIndex = 0;
            //
            // lblPage
            //
            lblPage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblPage.AutoSize = true;
            lblPage.Name = "lblPage";
            lblPage.TabIndex = 0;
            lblPage.Text = "Page:";
            //
            // numPage
            //
            numPage.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPage.Name = "numPage";
            numPage.Size = new System.Drawing.Size(55, 23);
            numPage.TabIndex = 1;
            numPage.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numPage.ValueChanged += NumPage_ValueChanged;
            //
            // lblInfo
            //
            lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblInfo.AutoSize = true;
            lblInfo.Margin = new System.Windows.Forms.Padding(12, 0, 3, 0);
            lblInfo.Name = "lblInfo";
            lblInfo.TabIndex = 2;
            lblInfo.Text = "";
            //
            // listHints
            //
            listHints.Dock = System.Windows.Forms.DockStyle.Fill;
            listHints.HorizontalScrollbar = true;
            listHints.IntegralHeight = false;
            listHints.Location = new System.Drawing.Point(0, 20);
            listHints.Name = "listHints";
            listHints.Size = new System.Drawing.Size(760, 134);
            listHints.TabIndex = 1;
            //
            // lblHints
            //
            lblHints.Dock = System.Windows.Forms.DockStyle.Top;
            lblHints.Location = new System.Drawing.Point(0, 0);
            lblHints.Name = "lblHints";
            lblHints.Padding = new System.Windows.Forms.Padding(3, 3, 0, 0);
            lblHints.Size = new System.Drawing.Size(760, 20);
            lblHints.TabIndex = 0;
            lblHints.Text = "Hints (values that only exist in the game get placeholders like {from.Name} or a default):";
            //
            // flowButtons
            //
            flowButtons.AutoSize = true;
            flowButtons.Controls.Add(btnCancel);
            flowButtons.Controls.Add(btnImport);
            flowButtons.Controls.Add(chkResolve);
            flowButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowButtons.Location = new System.Drawing.Point(0, 538);
            flowButtons.Name = "flowButtons";
            flowButtons.Padding = new System.Windows.Forms.Padding(6, 4, 6, 6);
            flowButtons.Size = new System.Drawing.Size(984, 43);
            flowButtons.TabIndex = 1;
            //
            // btnCancel
            //
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(90, 27);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // btnImport
            //
            btnImport.Name = "btnImport";
            btnImport.Size = new System.Drawing.Size(110, 27);
            btnImport.TabIndex = 1;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += BtnImport_Click;
            //
            // chkResolve
            //
            chkResolve.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkResolve.AutoSize = true;
            chkResolve.Checked = true;
            chkResolve.CheckState = System.Windows.Forms.CheckState.Checked;
            chkResolve.Margin = new System.Windows.Forms.Padding(3, 3, 30, 3);
            chkResolve.Name = "chkResolve";
            chkResolve.TabIndex = 0;
            chkResolve.Text = "Read values of other classes from the scripts around the file (e.g. PropsConfig)";
            toolTip1.SetToolTip(chkResolve, "Searches the folder of the file up to the Scripts folder for classes the gump uses\n\nDE: Sucht im Ordner der Datei bis hoch zum Scripts-Ordner nach Klassen, die das Gump benutzt");
            chkResolve.CheckedChanged += ChkResolve_CheckedChanged;
            //
            // GumpImportForm
            //
            AcceptButton = btnImport;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(984, 581);
            Controls.Add(splitMain);
            Controls.Add(flowButtons);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(640, 420);
            Name = "GumpImportForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Import ServUO gump / ServUO-Gump einlesen";
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            splitPreview.Panel1.ResumeLayout(false);
            splitPreview.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitPreview).EndInit();
            splitPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            flowPages.ResumeLayout(false);
            flowPages.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPage).EndInit();
            flowButtons.ResumeLayout(false);
            flowButtons.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.ListBox listClasses;
        private System.Windows.Forms.Label lblClasses;
        private System.Windows.Forms.SplitContainer splitPreview;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.FlowLayoutPanel flowPages;
        private System.Windows.Forms.Label lblPage;
        private System.Windows.Forms.NumericUpDown numPage;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ListBox listHints;
        private System.Windows.Forms.Label lblHints;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.CheckBox chkResolve;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
