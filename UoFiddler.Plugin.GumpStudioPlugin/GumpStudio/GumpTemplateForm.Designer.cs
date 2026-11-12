namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    partial class GumpTemplateForm
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
            toolStripLibrary = new System.Windows.Forms.ToolStrip();
            tsNewCategory = new System.Windows.Forms.ToolStripButton();
            tsDeleteCategory = new System.Windows.Forms.ToolStripButton();
            tsSep1 = new System.Windows.Forms.ToolStripSeparator();
            tsRename = new System.Windows.Forms.ToolStripButton();
            tsDuplicate = new System.Windows.Forms.ToolStripButton();
            tsDelete = new System.Windows.Forms.ToolStripButton();
            tsSep2 = new System.Windows.Forms.ToolStripSeparator();
            tsImportFiles = new System.Windows.Forms.ToolStripButton();
            tsExport = new System.Windows.Forms.ToolStripButton();
            tsOpenFolder = new System.Windows.Forms.ToolStripButton();
            tsRefresh = new System.Windows.Forms.ToolStripButton();
            tsSep3 = new System.Windows.Forms.ToolStripSeparator();
            tsSearchLabel = new System.Windows.Forms.ToolStripLabel();
            tsSearch = new System.Windows.Forms.ToolStripTextBox();
            tsThumbSize = new System.Windows.Forms.ToolStripComboBox();
            splitMain = new System.Windows.Forms.SplitContainer();
            treeCategories = new System.Windows.Forms.TreeView();
            splitRight = new System.Windows.Forms.SplitContainer();
            listTemplates = new System.Windows.Forms.ListView();
            contextList = new System.Windows.Forms.ContextMenuStrip(components);
            miOpen = new System.Windows.Forms.ToolStripMenuItem();
            miInsert = new System.Windows.Forms.ToolStripMenuItem();
            miSep1 = new System.Windows.Forms.ToolStripSeparator();
            miRename = new System.Windows.Forms.ToolStripMenuItem();
            miDuplicate = new System.Windows.Forms.ToolStripMenuItem();
            miDelete = new System.Windows.Forms.ToolStripMenuItem();
            miSep2 = new System.Windows.Forms.ToolStripSeparator();
            miShowFile = new System.Windows.Forms.ToolStripMenuItem();
            imageThumbs = new System.Windows.Forms.ImageList(components);
            tableDetails = new System.Windows.Forms.TableLayoutPanel();
            picturePreview = new System.Windows.Forms.PictureBox();
            lblTitle = new System.Windows.Forms.Label();
            lblInfo = new System.Windows.Forms.Label();
            txtDescription = new System.Windows.Forms.TextBox();
            btnSaveDescription = new System.Windows.Forms.Button();
            flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnClose = new System.Windows.Forms.Button();
            btnInsert = new System.Windows.Forms.Button();
            btnOpen = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            toolStripLibrary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitRight).BeginInit();
            splitRight.Panel1.SuspendLayout();
            splitRight.Panel2.SuspendLayout();
            splitRight.SuspendLayout();
            contextList.SuspendLayout();
            tableDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            flowButtons.SuspendLayout();
            SuspendLayout();
            //
            // toolStripLibrary
            //
            toolStripLibrary.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripLibrary.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsNewCategory, tsDeleteCategory, tsSep1, tsRename, tsDuplicate, tsDelete, tsSep2, tsImportFiles, tsExport, tsOpenFolder, tsRefresh, tsSep3, tsSearchLabel, tsSearch, tsThumbSize });
            toolStripLibrary.Location = new System.Drawing.Point(0, 0);
            toolStripLibrary.Name = "toolStripLibrary";
            toolStripLibrary.Size = new System.Drawing.Size(1084, 25);
            toolStripLibrary.TabIndex = 0;
            //
            // tsNewCategory
            //
            tsNewCategory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsNewCategory.Name = "tsNewCategory";
            tsNewCategory.Text = "+ Category";
            tsNewCategory.ToolTipText = "New category (\"/\" makes a sub category)\n\nDE: Neue Kategorie (\"/\" macht eine Unterkategorie)";
            tsNewCategory.Click += TsNewCategory_Click;
            //
            // tsDeleteCategory
            //
            tsDeleteCategory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsDeleteCategory.Name = "tsDeleteCategory";
            tsDeleteCategory.Text = "– Category";
            tsDeleteCategory.ToolTipText = "Delete the selected category (only when it is empty)\n\nDE: Gewählte Kategorie löschen (nur wenn sie leer ist)";
            tsDeleteCategory.Click += TsDeleteCategory_Click;
            //
            // tsSep1
            //
            tsSep1.Name = "tsSep1";
            //
            // tsRename
            //
            tsRename.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsRename.Name = "tsRename";
            tsRename.Text = "Rename / move…";
            tsRename.ToolTipText = "Rename the template or move it to another category (F2)\n\nDE: Vorlage umbenennen oder in eine andere Kategorie verschieben (F2)";
            tsRename.Click += TsRename_Click;
            //
            // tsDuplicate
            //
            tsDuplicate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsDuplicate.Name = "tsDuplicate";
            tsDuplicate.Text = "Copy";
            tsDuplicate.ToolTipText = "Copy the template (also built-in ones, to change them)\n\nDE: Vorlage kopieren (auch mitgelieferte, um sie zu ändern)";
            tsDuplicate.Click += TsDuplicate_Click;
            //
            // tsDelete
            //
            tsDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsDelete.Name = "tsDelete";
            tsDelete.Text = "Delete";
            tsDelete.ToolTipText = "Delete the template (Del)\n\nDE: Vorlage löschen (Entf)";
            tsDelete.Click += TsDelete_Click;
            //
            // tsSep2
            //
            tsSep2.Name = "tsSep2";
            //
            // tsImportFiles
            //
            tsImportFiles.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsImportFiles.Name = "tsImportFiles";
            tsImportFiles.Text = "Add files…";
            tsImportFiles.ToolTipText = "Add Gump Studio projects (*.gump.json) or ServUO gump scripts (*.cs) to the selected category – also by dragging files onto the list\n\nDE: Gump-Studio-Projekte (*.gump.json) oder ServUO-Gump-Skripte (*.cs) zur gewählten Kategorie hinzufügen – auch per Ziehen auf die Liste";
            tsImportFiles.Click += TsImportFiles_Click;
            //
            // tsExport
            //
            tsExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsExport.Name = "tsExport";
            tsExport.Text = "Export…";
            tsExport.ToolTipText = "Save the template as project file somewhere else (e.g. to give it to someone)\n\nDE: Vorlage als Projektdatei woanders speichern (z. B. zum Weitergeben)";
            tsExport.Click += TsExport_Click;
            //
            // tsOpenFolder
            //
            tsOpenFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsOpenFolder.Name = "tsOpenFolder";
            tsOpenFolder.Text = "Folder";
            tsOpenFolder.ToolTipText = "Open the template folder in Explorer\n\nDE: Vorlagen-Ordner im Explorer öffnen";
            tsOpenFolder.Click += TsOpenFolder_Click;
            //
            // tsRefresh
            //
            tsRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsRefresh.Name = "tsRefresh";
            tsRefresh.Text = "Refresh";
            tsRefresh.ToolTipText = "Read the folder again (F5)\n\nDE: Ordner neu einlesen (F5)";
            tsRefresh.Click += TsRefresh_Click;
            //
            // tsSep3
            //
            tsSep3.Name = "tsSep3";
            //
            // tsSearchLabel
            //
            tsSearchLabel.Name = "tsSearchLabel";
            tsSearchLabel.Text = "Search:";
            //
            // tsSearch
            //
            tsSearch.Name = "tsSearch";
            tsSearch.Size = new System.Drawing.Size(180, 25);
            tsSearch.ToolTipText = "Search in names and notes of all categories\n\nDE: In Namen und Notizen aller Kategorien suchen";
            tsSearch.TextChanged += TsSearch_TextChanged;
            //
            // tsThumbSize
            //
            tsThumbSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tsThumbSize.Items.AddRange(new object[] { "Small", "Medium", "Large" });
            tsThumbSize.Name = "tsThumbSize";
            tsThumbSize.Size = new System.Drawing.Size(80, 25);
            tsThumbSize.ToolTipText = "Size of the pictures\n\nDE: Größe der Bilder";
            tsThumbSize.SelectedIndexChanged += TsThumbSize_SelectedIndexChanged;
            //
            // splitMain
            //
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitMain.Location = new System.Drawing.Point(0, 25);
            splitMain.Name = "splitMain";
            //
            // splitMain.Panel1
            //
            splitMain.Panel1.Controls.Add(treeCategories);
            splitMain.Panel1MinSize = 120;
            //
            // splitMain.Panel2
            //
            splitMain.Panel2.Controls.Add(splitRight);
            splitMain.Size = new System.Drawing.Size(1084, 553);
            splitMain.SplitterDistance = 210;
            splitMain.TabIndex = 1;
            //
            // treeCategories
            //
            treeCategories.AllowDrop = true;
            treeCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            treeCategories.HideSelection = false;
            treeCategories.LabelEdit = true;
            treeCategories.Location = new System.Drawing.Point(0, 0);
            treeCategories.Name = "treeCategories";
            treeCategories.Size = new System.Drawing.Size(210, 553);
            treeCategories.TabIndex = 0;
            toolTip1.SetToolTip(treeCategories, "Categories – drag a template onto a category to move it; click a selected category again (or F2) to rename it\n\nDE: Kategorien – Vorlage auf eine Kategorie ziehen, um sie zu verschieben; gewählte Kategorie nochmals anklicken (oder F2) zum Umbenennen");
            treeCategories.AfterSelect += TreeCategories_AfterSelect;
            treeCategories.BeforeLabelEdit += TreeCategories_BeforeLabelEdit;
            treeCategories.AfterLabelEdit += TreeCategories_AfterLabelEdit;
            treeCategories.DragDrop += TreeCategories_DragDrop;
            treeCategories.DragOver += TreeCategories_DragOver;
            //
            // splitRight
            //
            splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            splitRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitRight.Location = new System.Drawing.Point(0, 0);
            splitRight.Name = "splitRight";
            //
            // splitRight.Panel1
            //
            splitRight.Panel1.Controls.Add(listTemplates);
            //
            // splitRight.Panel2
            //
            splitRight.Panel2.Controls.Add(tableDetails);
            splitRight.Panel2MinSize = 200;
            splitRight.Size = new System.Drawing.Size(870, 553);
            splitRight.SplitterDistance = 540;
            splitRight.TabIndex = 0;
            //
            // listTemplates
            //
            listTemplates.AllowDrop = true;
            listTemplates.ContextMenuStrip = contextList;
            listTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            listTemplates.HideSelection = false;
            listTemplates.LargeImageList = imageThumbs;
            listTemplates.Location = new System.Drawing.Point(0, 0);
            listTemplates.MultiSelect = false;
            listTemplates.Name = "listTemplates";
            listTemplates.Size = new System.Drawing.Size(540, 553);
            listTemplates.TabIndex = 0;
            listTemplates.UseCompatibleStateImageBehavior = false;
            listTemplates.ItemDrag += ListTemplates_ItemDrag;
            listTemplates.SelectedIndexChanged += ListTemplates_SelectedIndexChanged;
            listTemplates.DragDrop += ListTemplates_DragDrop;
            listTemplates.DragEnter += ListTemplates_DragEnter;
            listTemplates.DoubleClick += ListTemplates_DoubleClick;
            listTemplates.KeyDown += ListTemplates_KeyDown;
            //
            // contextList
            //
            contextList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { miOpen, miInsert, miSep1, miRename, miDuplicate, miDelete, miSep2, miShowFile });
            contextList.Name = "contextList";
            contextList.Size = new System.Drawing.Size(260, 148);
            contextList.Opening += ContextList_Opening;
            //
            // miOpen
            //
            miOpen.Name = "miOpen";
            miOpen.Text = "New gump from this template";
            miOpen.Click += BtnOpen_Click;
            //
            // miInsert
            //
            miInsert.Name = "miInsert";
            miInsert.Text = "Insert into the current gump";
            miInsert.Click += BtnInsert_Click;
            //
            // miSep1
            //
            miSep1.Name = "miSep1";
            //
            // miRename
            //
            miRename.Name = "miRename";
            miRename.Text = "Rename / move…";
            miRename.Click += TsRename_Click;
            //
            // miDuplicate
            //
            miDuplicate.Name = "miDuplicate";
            miDuplicate.Text = "Copy";
            miDuplicate.Click += TsDuplicate_Click;
            //
            // miDelete
            //
            miDelete.Name = "miDelete";
            miDelete.Text = "Delete";
            miDelete.Click += TsDelete_Click;
            //
            // miSep2
            //
            miSep2.Name = "miSep2";
            //
            // miShowFile
            //
            miShowFile.Name = "miShowFile";
            miShowFile.Text = "Show file in Explorer";
            miShowFile.Click += MiShowFile_Click;
            //
            // imageThumbs
            //
            imageThumbs.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageThumbs.ImageSize = new System.Drawing.Size(128, 96);
            imageThumbs.TransparentColor = System.Drawing.Color.Transparent;
            //
            // tableDetails
            //
            tableDetails.ColumnCount = 1;
            tableDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableDetails.Controls.Add(picturePreview, 0, 0);
            tableDetails.Controls.Add(lblTitle, 0, 1);
            tableDetails.Controls.Add(lblInfo, 0, 2);
            tableDetails.Controls.Add(txtDescription, 0, 3);
            tableDetails.Controls.Add(btnSaveDescription, 0, 4);
            tableDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            tableDetails.Location = new System.Drawing.Point(0, 0);
            tableDetails.Name = "tableDetails";
            tableDetails.RowCount = 5;
            tableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            tableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableDetails.Size = new System.Drawing.Size(326, 553);
            tableDetails.TabIndex = 0;
            //
            // picturePreview
            //
            picturePreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picturePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picturePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            picturePreview.Name = "picturePreview";
            picturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picturePreview.TabIndex = 0;
            picturePreview.TabStop = false;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 1;
            lblTitle.Text = "–";
            //
            // lblInfo
            //
            lblInfo.AutoSize = true;
            lblInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            lblInfo.Name = "lblInfo";
            lblInfo.TabIndex = 2;
            lblInfo.Text = "";
            //
            // txtDescription
            //
            txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtDescription.TabIndex = 3;
            toolTip1.SetToolTip(txtDescription, "Note of the template\n\nDE: Notiz zur Vorlage");
            txtDescription.TextChanged += TxtDescription_TextChanged;
            //
            // btnSaveDescription
            //
            btnSaveDescription.AutoSize = true;
            btnSaveDescription.Enabled = false;
            btnSaveDescription.Name = "btnSaveDescription";
            btnSaveDescription.TabIndex = 4;
            btnSaveDescription.Text = "Save note";
            btnSaveDescription.UseVisualStyleBackColor = true;
            btnSaveDescription.Click += BtnSaveDescription_Click;
            //
            // flowButtons
            //
            flowButtons.AutoSize = true;
            flowButtons.Controls.Add(btnClose);
            flowButtons.Controls.Add(btnInsert);
            flowButtons.Controls.Add(btnOpen);
            flowButtons.Controls.Add(lblStatus);
            flowButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowButtons.Location = new System.Drawing.Point(0, 578);
            flowButtons.Name = "flowButtons";
            flowButtons.Padding = new System.Windows.Forms.Padding(6, 4, 6, 6);
            flowButtons.Size = new System.Drawing.Size(1084, 43);
            flowButtons.TabIndex = 2;
            //
            // btnClose
            //
            btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(90, 27);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            //
            // btnInsert
            //
            btnInsert.AutoSize = true;
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new System.Drawing.Size(180, 27);
            btnInsert.TabIndex = 1;
            btnInsert.Text = "Insert into current gump";
            toolTip1.SetToolTip(btnInsert, "Adds the elements to the gump you are editing (IDs are moved behind the used ones)\n\nDE: Fügt die Elemente in das Gump ein, das du gerade bearbeitest (IDs werden hinter die benutzten gesetzt)");
            btnInsert.UseVisualStyleBackColor = true;
            btnInsert.Click += BtnInsert_Click;
            //
            // btnOpen
            //
            btnOpen.AutoSize = true;
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new System.Drawing.Size(160, 27);
            btnOpen.TabIndex = 0;
            btnOpen.Text = "New gump from it";
            toolTip1.SetToolTip(btnOpen, "Starts a new gump from the template (double click)\n\nDE: Beginnt ein neues Gump aus der Vorlage (Doppelklick)");
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += BtnOpen_Click;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Margin = new System.Windows.Forms.Padding(3, 0, 20, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 3;
            lblStatus.Text = "";
            //
            // GumpTemplateForm
            //
            AcceptButton = btnOpen;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new System.Drawing.Size(1084, 621);
            Controls.Add(splitMain);
            Controls.Add(flowButtons);
            Controls.Add(toolStripLibrary);
            KeyPreview = true;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(760, 450);
            Name = "GumpTemplateForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Gump templates / Gump-Vorlagen";
            KeyDown += GumpTemplateForm_KeyDown;
            toolStripLibrary.ResumeLayout(false);
            toolStripLibrary.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            splitRight.Panel1.ResumeLayout(false);
            splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitRight).EndInit();
            splitRight.ResumeLayout(false);
            contextList.ResumeLayout(false);
            tableDetails.ResumeLayout(false);
            tableDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            flowButtons.ResumeLayout(false);
            flowButtons.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripLibrary;
        private System.Windows.Forms.ToolStripButton tsNewCategory;
        private System.Windows.Forms.ToolStripButton tsDeleteCategory;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton tsRename;
        private System.Windows.Forms.ToolStripButton tsDuplicate;
        private System.Windows.Forms.ToolStripButton tsDelete;
        private System.Windows.Forms.ToolStripSeparator tsSep2;
        private System.Windows.Forms.ToolStripButton tsImportFiles;
        private System.Windows.Forms.ToolStripButton tsExport;
        private System.Windows.Forms.ToolStripButton tsOpenFolder;
        private System.Windows.Forms.ToolStripButton tsRefresh;
        private System.Windows.Forms.ToolStripSeparator tsSep3;
        private System.Windows.Forms.ToolStripLabel tsSearchLabel;
        private System.Windows.Forms.ToolStripTextBox tsSearch;
        private System.Windows.Forms.ToolStripComboBox tsThumbSize;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TreeView treeCategories;
        private System.Windows.Forms.SplitContainer splitRight;
        private System.Windows.Forms.ListView listTemplates;
        private System.Windows.Forms.ContextMenuStrip contextList;
        private System.Windows.Forms.ToolStripMenuItem miOpen;
        private System.Windows.Forms.ToolStripMenuItem miInsert;
        private System.Windows.Forms.ToolStripSeparator miSep1;
        private System.Windows.Forms.ToolStripMenuItem miRename;
        private System.Windows.Forms.ToolStripMenuItem miDuplicate;
        private System.Windows.Forms.ToolStripMenuItem miDelete;
        private System.Windows.Forms.ToolStripSeparator miSep2;
        private System.Windows.Forms.ToolStripMenuItem miShowFile;
        private System.Windows.Forms.ImageList imageThumbs;
        private System.Windows.Forms.TableLayoutPanel tableDetails;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button btnSaveDescription;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
