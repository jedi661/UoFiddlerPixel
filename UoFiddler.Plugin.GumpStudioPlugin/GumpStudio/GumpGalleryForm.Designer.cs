namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    partial class GumpGalleryForm
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
            if (disposing) { _cancel?.Cancel(); picturePreview.Image?.Dispose(); }
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
            listGumps = new System.Windows.Forms.ListView();
            picturePreview = new System.Windows.Forms.PictureBox();
            txtInfo = new System.Windows.Forms.TextBox();
            progressScan = new System.Windows.Forms.ProgressBar();
            flowTop = new System.Windows.Forms.FlowLayoutPanel();
            lblFolder = new System.Windows.Forms.Label();
            txtFolder = new System.Windows.Forms.TextBox();
            btnBrowse = new System.Windows.Forms.Button();
            btnScan = new System.Windows.Forms.Button();
            btnStop = new System.Windows.Forms.Button();
            lblSearch = new System.Windows.Forms.Label();
            txtSearch = new System.Windows.Forms.TextBox();
            chkResolve = new System.Windows.Forms.CheckBox();
            flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnClose = new System.Windows.Forms.Button();
            btnOpen = new System.Windows.Forms.Button();
            btnAddTemplates = new System.Windows.Forms.Button();
            comboCategory = new System.Windows.Forms.ComboBox();
            lblCategory = new System.Windows.Forms.Label();
            btnShowFile = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            imageThumbs = new System.Windows.Forms.ImageList(components);
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            flowTop.SuspendLayout();
            flowButtons.SuspendLayout();
            SuspendLayout();
            //
            // splitMain
            //
            splitMain.Panel1.Controls.Add(listGumps);
            splitMain.Panel2.Controls.Add(picturePreview);
            splitMain.Panel2.Controls.Add(txtInfo);
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.Size = new System.Drawing.Size(1100, 520);
            splitMain.SplitterDistance = 760;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitMain.Name = "splitMain";
            splitMain.TabIndex = 0;
            //
            // listGumps
            //
            listGumps.Dock = System.Windows.Forms.DockStyle.Fill;
            listGumps.HideSelection = false;
            listGumps.LargeImageList = imageThumbs;
            listGumps.MultiSelect = true;
            listGumps.ShowItemToolTips = true;
            listGumps.UseCompatibleStateImageBehavior = false;
            listGumps.Name = "listGumps";
            listGumps.TabIndex = 1;
            listGumps.SelectedIndexChanged += ListGumps_SelectedIndexChanged;
            listGumps.DoubleClick += BtnOpen_Click;
            //
            // picturePreview
            //
            picturePreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picturePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            picturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picturePreview.TabStop = false;
            picturePreview.Name = "picturePreview";
            picturePreview.TabIndex = 2;
            //
            // txtInfo
            //
            txtInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            txtInfo.Multiline = true;
            txtInfo.ReadOnly = true;
            txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtInfo.Size = new System.Drawing.Size(330, 170);
            txtInfo.Name = "txtInfo";
            txtInfo.TabIndex = 3;
            //
            // progressScan
            //
            progressScan.Dock = System.Windows.Forms.DockStyle.Top;
            progressScan.Size = new System.Drawing.Size(1100, 6);
            progressScan.Name = "progressScan";
            progressScan.TabIndex = 4;
            //
            // flowTop
            //
            flowTop.Controls.Add(lblFolder);
            flowTop.Controls.Add(txtFolder);
            flowTop.Controls.Add(btnBrowse);
            flowTop.Controls.Add(btnScan);
            flowTop.Controls.Add(btnStop);
            flowTop.Controls.Add(lblSearch);
            flowTop.Controls.Add(txtSearch);
            flowTop.Controls.Add(chkResolve);
            flowTop.AutoSize = true;
            flowTop.Dock = System.Windows.Forms.DockStyle.Top;
            flowTop.Padding = new System.Windows.Forms.Padding(4);
            flowTop.Name = "flowTop";
            flowTop.TabIndex = 5;
            //
            // lblFolder
            //
            lblFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblFolder.AutoSize = true;
            lblFolder.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblFolder.Text = "Scripts folder:";
            lblFolder.Name = "lblFolder";
            lblFolder.TabIndex = 6;
            //
            // txtFolder
            //
            txtFolder.Size = new System.Drawing.Size(380, 23);
            txtFolder.Name = "txtFolder";
            txtFolder.TabIndex = 7;
            toolTip1.SetToolTip(txtFolder, "e.g. C:\\ServUO\\Scripts – all .cs files below it are searched for gumps\n\nDE: z. B. C:\\ServUO\\Scripts – alle .cs-Dateien darunter werden nach Gumps durchsucht");
            //
            // btnBrowse
            //
            btnBrowse.AutoSize = true;
            btnBrowse.Text = "…";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Name = "btnBrowse";
            btnBrowse.TabIndex = 8;
            btnBrowse.Click += BtnBrowse_Click;
            //
            // btnScan
            //
            btnScan.AutoSize = true;
            btnScan.Text = "Scan";
            btnScan.UseVisualStyleBackColor = true;
            btnScan.Name = "btnScan";
            btnScan.TabIndex = 9;
            toolTip1.SetToolTip(btnScan, "Finds every gump class and builds a picture of it\n\nDE: Findet jede Gump-Klasse und baut ein Bild davon");
            btnScan.Click += BtnScan_Click;
            //
            // btnStop
            //
            btnStop.AutoSize = true;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Enabled = false;
            btnStop.Name = "btnStop";
            btnStop.TabIndex = 10;
            btnStop.Click += BtnStop_Click;
            //
            // lblSearch
            //
            lblSearch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSearch.AutoSize = true;
            lblSearch.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblSearch.Text = "Search:";
            lblSearch.Name = "lblSearch";
            lblSearch.TabIndex = 11;
            //
            // txtSearch
            //
            txtSearch.Size = new System.Drawing.Size(170, 23);
            txtSearch.Name = "txtSearch";
            txtSearch.TabIndex = 12;
            toolTip1.SetToolTip(txtSearch, "Class name, command or file\n\nDE: Klassenname, Befehl oder Datei");
            txtSearch.TextChanged += TxtSearch_TextChanged;
            //
            // chkResolve
            //
            chkResolve.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkResolve.AutoSize = true;
            chkResolve.Text = "Values of other classes";
            chkResolve.Checked = true;
            chkResolve.CheckState = System.Windows.Forms.CheckState.Checked;
            chkResolve.Name = "chkResolve";
            chkResolve.TabIndex = 13;
            toolTip1.SetToolTip(chkResolve, "Reads constants of other classes (e.g. PropsConfig) from the folder – slower, more exact\n\nDE: Liest Konstanten anderer Klassen (z. B. PropsConfig) aus dem Ordner – langsamer, genauer");
            //
            // flowButtons
            //
            flowButtons.Controls.Add(btnClose);
            flowButtons.Controls.Add(btnOpen);
            flowButtons.Controls.Add(btnAddTemplates);
            flowButtons.Controls.Add(comboCategory);
            flowButtons.Controls.Add(lblCategory);
            flowButtons.Controls.Add(btnShowFile);
            flowButtons.Controls.Add(lblStatus);
            flowButtons.AutoSize = true;
            flowButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowButtons.Padding = new System.Windows.Forms.Padding(4);
            flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowButtons.Name = "flowButtons";
            flowButtons.TabIndex = 14;
            //
            // btnClose
            //
            btnClose.AutoSize = true;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnClose.Name = "btnClose";
            btnClose.TabIndex = 15;
            //
            // btnOpen
            //
            btnOpen.AutoSize = true;
            btnOpen.Text = "Open in the studio";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Name = "btnOpen";
            btnOpen.TabIndex = 16;
            toolTip1.SetToolTip(btnOpen, "Imports the gump (double click) – it can be written back into its script later\n\nDE: Liest das Gump ein (Doppelklick) – es kann später in sein Skript zurückgeschrieben werden");
            btnOpen.Click += BtnOpen_Click;
            //
            // btnAddTemplates
            //
            btnAddTemplates.AutoSize = true;
            btnAddTemplates.Text = "Add to templates";
            btnAddTemplates.UseVisualStyleBackColor = true;
            btnAddTemplates.Name = "btnAddTemplates";
            btnAddTemplates.TabIndex = 17;
            toolTip1.SetToolTip(btnAddTemplates, "Saves the selected gumps into the template library\n\nDE: Speichert die gewählten Gumps in die Vorlagen-Bibliothek");
            btnAddTemplates.Click += BtnAddTemplates_Click;
            //
            // comboCategory
            //
            comboCategory.Size = new System.Drawing.Size(160, 23);
            comboCategory.Text = "ServUO";
            comboCategory.Name = "comboCategory";
            comboCategory.TabIndex = 18;
            //
            // lblCategory
            //
            lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCategory.AutoSize = true;
            lblCategory.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblCategory.Text = "Category:";
            lblCategory.Name = "lblCategory";
            lblCategory.TabIndex = 19;
            //
            // btnShowFile
            //
            btnShowFile.AutoSize = true;
            btnShowFile.Text = "Show file";
            btnShowFile.UseVisualStyleBackColor = true;
            btnShowFile.Name = "btnShowFile";
            btnShowFile.TabIndex = 20;
            btnShowFile.Click += BtnShowFile_Click;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Margin = new System.Windows.Forms.Padding(3, 0, 30, 0);
            lblStatus.Text = "";
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 21;
            //
            // imageThumbs
            //
            imageThumbs.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageThumbs.ImageSize = new System.Drawing.Size(128, 96);
            imageThumbs.TransparentColor = System.Drawing.Color.Transparent;
            //
            // toolTip1
            //
            toolTip1.AutoPopDelay = 15000;
            //
            // GumpGalleryForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new System.Drawing.Size(1100, 640);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(800, 450);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Gump gallery – gumps of a scripts folder / Gump-Galerie";
            Controls.Add(splitMain);
            Controls.Add(progressScan);
            Controls.Add(flowTop);
            Controls.Add(flowButtons);
            Name = "GumpGalleryForm";
            flowButtons.ResumeLayout(false);
            flowButtons.PerformLayout();
            flowTop.ResumeLayout(false);
            flowTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.ListView listGumps;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.ProgressBar progressScan;
        private System.Windows.Forms.FlowLayoutPanel flowTop;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.CheckBox chkResolve;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnAddTemplates;
        private System.Windows.Forms.ComboBox comboCategory;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Button btnShowFile;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ImageList imageThumbs;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
