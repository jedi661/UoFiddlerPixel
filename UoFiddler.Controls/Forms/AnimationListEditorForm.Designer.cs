using System;
using System.Xml;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    partial class AnimationListEditorForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationListEditorForm));
            splitMain = new SplitContainer();
            richTextBox = new RichTextBox();
            tblRight = new TableLayoutPanel();
            grpPreview = new GroupBox();
            picPreview = new PictureBox();
            lblPreviewInfo = new Label();
            lblActionLbl = new Label();
            lblActionVal = new Label();
            trackAction = new TrackBar();
            lblFacingLbl = new Label();
            lblFacingVal = new Label();
            trackFacing = new TrackBar();
            grpBrowse = new GroupBox();
            lblBrowseIdLbl = new Label();
            numBrowseId = new NumericUpDown();
            btnBrowsePrev = new Button();
            btnBrowseNext = new Button();
            btnBrowseNextAnim = new Button();
            btnAddToXml = new Button();
            lblBrowseFile = new Label();
            lblBrowseHasAnim = new Label();
            lblBrowseInXml = new Label();
            grpList = new GroupBox();
            listEntries = new ListView();
            lblEntryCount = new Label();
            toolStrip = new ToolStrip();
            btnNew = new ToolStripButton();
            btnOpen = new ToolStripButton();
            btnSave = new ToolStripButton();
            btnSaveAs = new ToolStripButton();
            sep1 = new ToolStripSeparator();
            btnFormat = new ToolStripButton();
            btnHighlight = new ToolStripButton();
            sep2 = new ToolStripSeparator();
            btnCleanup = new ToolStripDropDownButton();
            btnCleanAuto = new ToolStripMenuItem();
            btnCleanStep = new ToolStripMenuItem();
            sep3 = new ToolStripSeparator();
            btnEditEntry = new ToolStripButton();
            btnDeleteEntry = new ToolStripButton();
            sep4 = new ToolStripSeparator();
            btnRefreshList = new ToolStripButton();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            pnlSearch = new Panel();
            lblSearch = new Label();
            txtSearch = new TextBox();
            btnSearchFwd = new Button();
            btnSearchBack = new Button();
            lblReplace = new Label();
            txtReplace = new TextBox();
            btnReplace = new Button();
            pnlPreview = new Panel();
            pnlBrowse = new Panel();
            pnlEdit = new Panel();
            lblEditTitle = new Label();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            tblRight.SuspendLayout();
            grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackAction).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackFacing).BeginInit();
            grpBrowse.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBrowseId).BeginInit();
            grpList.SuspendLayout();
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            pnlSearch.SuspendLayout();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new System.Drawing.Point(0, 63);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(richTextBox);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(tblRight);
            splitMain.Panel2MinSize = 320;
            splitMain.Size = new System.Drawing.Size(1362, 755);
            splitMain.SplitterDistance = 987;
            splitMain.SplitterWidth = 5;
            splitMain.TabIndex = 0;
            // 
            // richTextBox
            // 
            richTextBox.Dock = DockStyle.Fill;
            richTextBox.Location = new System.Drawing.Point(0, 0);
            richTextBox.Name = "richTextBox";
            richTextBox.Size = new System.Drawing.Size(987, 755);
            richTextBox.TabIndex = 0;
            richTextBox.Text = "";
            richTextBox.WordWrap = false;
            richTextBox.TextChanged += richTextBox_TextChanged;
            // 
            // tblRight
            // 
            tblRight.ColumnCount = 1;
            tblRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblRight.Controls.Add(grpPreview, 0, 0);
            tblRight.Controls.Add(grpBrowse, 0, 1);
            tblRight.Controls.Add(grpList, 0, 2);
            tblRight.Dock = DockStyle.Fill;
            tblRight.Location = new System.Drawing.Point(0, 0);
            tblRight.Name = "tblRight";
            tblRight.Padding = new Padding(2);
            tblRight.RowCount = 3;
            tblRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 210F));
            tblRight.RowStyles.Add(new RowStyle(SizeType.Absolute, 210F));
            tblRight.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblRight.Size = new System.Drawing.Size(370, 755);
            tblRight.TabIndex = 0;
            // 
            // grpPreview
            // 
            grpPreview.BackColor = System.Drawing.Color.FromArgb(30, 36, 46);
            grpPreview.Controls.Add(picPreview);
            grpPreview.Controls.Add(lblPreviewInfo);
            grpPreview.Controls.Add(lblActionLbl);
            grpPreview.Controls.Add(lblActionVal);
            grpPreview.Controls.Add(trackAction);
            grpPreview.Controls.Add(lblFacingLbl);
            grpPreview.Controls.Add(lblFacingVal);
            grpPreview.Controls.Add(trackFacing);
            grpPreview.Dock = DockStyle.Fill;
            grpPreview.ForeColor = System.Drawing.Color.FromArgb(64, 156, 255);
            grpPreview.Location = new System.Drawing.Point(2, 2);
            grpPreview.Margin = new Padding(0, 0, 0, 4);
            grpPreview.Name = "grpPreview";
            grpPreview.Size = new System.Drawing.Size(366, 206);
            grpPreview.TabIndex = 0;
            grpPreview.TabStop = false;
            grpPreview.Text = "Preview";
            // 
            // picPreview
            // 
            picPreview.BackColor = System.Drawing.Color.Black;
            picPreview.BorderStyle = BorderStyle.FixedSingle;
            picPreview.Location = new System.Drawing.Point(8, 20);
            picPreview.Name = "picPreview";
            picPreview.Size = new System.Drawing.Size(180, 171);
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.TabIndex = 0;
            picPreview.TabStop = false;
            // 
            // lblPreviewInfo
            // 
            lblPreviewInfo.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            lblPreviewInfo.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            lblPreviewInfo.Location = new System.Drawing.Point(203, 20);
            lblPreviewInfo.Name = "lblPreviewInfo";
            lblPreviewInfo.Size = new System.Drawing.Size(140, 50);
            lblPreviewInfo.TabIndex = 1;
            // 
            // lblActionLbl
            // 
            lblActionLbl.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            lblActionLbl.Location = new System.Drawing.Point(203, 74);
            lblActionLbl.Name = "lblActionLbl";
            lblActionLbl.Size = new System.Drawing.Size(46, 16);
            lblActionLbl.TabIndex = 2;
            lblActionLbl.Text = "Action";
            // 
            // lblActionVal
            // 
            lblActionVal.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            lblActionVal.Location = new System.Drawing.Point(251, 74);
            lblActionVal.Name = "lblActionVal";
            lblActionVal.Size = new System.Drawing.Size(30, 16);
            lblActionVal.TabIndex = 3;
            lblActionVal.Text = "0";
            // 
            // trackAction
            // 
            trackAction.Location = new System.Drawing.Point(203, 92);
            trackAction.Maximum = 33;
            trackAction.Name = "trackAction";
            trackAction.Size = new System.Drawing.Size(150, 45);
            trackAction.TabIndex = 4;
            trackAction.TickStyle = TickStyle.None;
            trackAction.ValueChanged += trackAction_ValueChanged;
            // 
            // lblFacingLbl
            // 
            lblFacingLbl.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            lblFacingLbl.Location = new System.Drawing.Point(203, 135);
            lblFacingLbl.Name = "lblFacingLbl";
            lblFacingLbl.Size = new System.Drawing.Size(56, 16);
            lblFacingLbl.TabIndex = 5;
            lblFacingLbl.Text = "Direction";
            // 
            // lblFacingVal
            // 
            lblFacingVal.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            lblFacingVal.Location = new System.Drawing.Point(261, 135);
            lblFacingVal.Name = "lblFacingVal";
            lblFacingVal.Size = new System.Drawing.Size(30, 16);
            lblFacingVal.TabIndex = 6;
            lblFacingVal.Text = "1";
            // 
            // trackFacing
            // 
            trackFacing.Location = new System.Drawing.Point(203, 153);
            trackFacing.Maximum = 7;
            trackFacing.Name = "trackFacing";
            trackFacing.Size = new System.Drawing.Size(150, 45);
            trackFacing.TabIndex = 7;
            trackFacing.TickStyle = TickStyle.None;
            trackFacing.Value = 1;
            trackFacing.ValueChanged += trackFacing_ValueChanged;
            // 
            // grpBrowse
            // 
            grpBrowse.BackColor = System.Drawing.Color.FromArgb(30, 36, 46);
            grpBrowse.Controls.Add(lblBrowseIdLbl);
            grpBrowse.Controls.Add(numBrowseId);
            grpBrowse.Controls.Add(btnBrowsePrev);
            grpBrowse.Controls.Add(btnBrowseNext);
            grpBrowse.Controls.Add(btnBrowseNextAnim);
            grpBrowse.Controls.Add(btnAddToXml);
            grpBrowse.Controls.Add(lblBrowseFile);
            grpBrowse.Controls.Add(lblBrowseHasAnim);
            grpBrowse.Controls.Add(lblBrowseInXml);
            grpBrowse.Dock = DockStyle.Fill;
            grpBrowse.ForeColor = System.Drawing.Color.FromArgb(64, 156, 255);
            grpBrowse.Location = new System.Drawing.Point(2, 212);
            grpBrowse.Margin = new Padding(0, 0, 0, 4);
            grpBrowse.Name = "grpBrowse";
            grpBrowse.Size = new System.Drawing.Size(366, 206);
            grpBrowse.TabIndex = 1;
            grpBrowse.TabStop = false;
            grpBrowse.Text = "MUL-Browser";
            // 
            // lblBrowseIdLbl
            // 
            lblBrowseIdLbl.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            lblBrowseIdLbl.Location = new System.Drawing.Point(8, 24);
            lblBrowseIdLbl.Name = "lblBrowseIdLbl";
            lblBrowseIdLbl.Size = new System.Drawing.Size(54, 22);
            lblBrowseIdLbl.TabIndex = 0;
            lblBrowseIdLbl.Text = "Body-ID:";
            lblBrowseIdLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numBrowseId
            // 
            numBrowseId.BackColor = System.Drawing.Color.FromArgb(22, 26, 34);
            numBrowseId.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            numBrowseId.Location = new System.Drawing.Point(64, 24);
            numBrowseId.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numBrowseId.Name = "numBrowseId";
            numBrowseId.Size = new System.Drawing.Size(80, 23);
            numBrowseId.TabIndex = 1;
            numBrowseId.ValueChanged += numBrowseId_ValueChanged;
            // 
            // btnBrowsePrev
            // 
            btnBrowsePrev.BackColor = System.Drawing.Color.FromArgb(50, 60, 78);
            btnBrowsePrev.FlatStyle = FlatStyle.Flat;
            btnBrowsePrev.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            btnBrowsePrev.Location = new System.Drawing.Point(150, 23);
            btnBrowsePrev.Name = "btnBrowsePrev";
            btnBrowsePrev.Size = new System.Drawing.Size(76, 24);
            btnBrowsePrev.TabIndex = 2;
            btnBrowsePrev.Text = "< Back";
            btnBrowsePrev.UseVisualStyleBackColor = false;
            btnBrowsePrev.Click += btnBrowsePrev_Click;
            // 
            // btnBrowseNext
            // 
            btnBrowseNext.BackColor = System.Drawing.Color.FromArgb(50, 60, 78);
            btnBrowseNext.FlatStyle = FlatStyle.Flat;
            btnBrowseNext.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            btnBrowseNext.Location = new System.Drawing.Point(230, 23);
            btnBrowseNext.Name = "btnBrowseNext";
            btnBrowseNext.Size = new System.Drawing.Size(76, 24);
            btnBrowseNext.TabIndex = 3;
            btnBrowseNext.Text = "Next >";
            btnBrowseNext.UseVisualStyleBackColor = false;
            btnBrowseNext.Click += btnBrowseNext_Click;
            // 
            // btnBrowseNextAnim
            // 
            btnBrowseNextAnim.BackColor = System.Drawing.Color.FromArgb(64, 156, 255);
            btnBrowseNextAnim.FlatStyle = FlatStyle.Flat;
            btnBrowseNextAnim.ForeColor = System.Drawing.Color.White;
            btnBrowseNextAnim.Location = new System.Drawing.Point(8, 54);
            btnBrowseNextAnim.Name = "btnBrowseNextAnim";
            btnBrowseNextAnim.Size = new System.Drawing.Size(180, 26);
            btnBrowseNextAnim.TabIndex = 4;
            btnBrowseNextAnim.Text = "Next with animation";
            btnBrowseNextAnim.UseVisualStyleBackColor = false;
            btnBrowseNextAnim.Click += btnBrowseNextAnim_Click;
            // 
            // btnAddToXml
            // 
            btnAddToXml.BackColor = System.Drawing.Color.FromArgb(64, 156, 80);
            btnAddToXml.FlatStyle = FlatStyle.Flat;
            btnAddToXml.ForeColor = System.Drawing.Color.White;
            btnAddToXml.Location = new System.Drawing.Point(196, 54);
            btnAddToXml.Name = "btnAddToXml";
            btnAddToXml.Size = new System.Drawing.Size(150, 26);
            btnAddToXml.TabIndex = 5;
            btnAddToXml.Text = "+ Add to XML";
            btnAddToXml.UseVisualStyleBackColor = false;
            btnAddToXml.Click += btnAddToXml_Click;
            // 
            // lblBrowseFile
            // 
            lblBrowseFile.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            lblBrowseFile.Location = new System.Drawing.Point(8, 90);
            lblBrowseFile.Name = "lblBrowseFile";
            lblBrowseFile.Size = new System.Drawing.Size(310, 18);
            lblBrowseFile.TabIndex = 6;
            lblBrowseFile.Text = "Datei: -";
            // 
            // lblBrowseHasAnim
            // 
            lblBrowseHasAnim.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            lblBrowseHasAnim.Location = new System.Drawing.Point(8, 112);
            lblBrowseHasAnim.Name = "lblBrowseHasAnim";
            lblBrowseHasAnim.Size = new System.Drawing.Size(310, 18);
            lblBrowseHasAnim.TabIndex = 7;
            lblBrowseHasAnim.Text = "-";
            // 
            // lblBrowseInXml
            // 
            lblBrowseInXml.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            lblBrowseInXml.Location = new System.Drawing.Point(8, 134);
            lblBrowseInXml.Name = "lblBrowseInXml";
            lblBrowseInXml.Size = new System.Drawing.Size(310, 18);
            lblBrowseInXml.TabIndex = 8;
            lblBrowseInXml.Text = "-";
            // 
            // grpList
            // 
            grpList.BackColor = System.Drawing.Color.FromArgb(30, 36, 46);
            grpList.Controls.Add(listEntries);
            grpList.Controls.Add(lblEntryCount);
            grpList.Dock = DockStyle.Fill;
            grpList.ForeColor = System.Drawing.Color.FromArgb(64, 156, 255);
            grpList.Location = new System.Drawing.Point(2, 422);
            grpList.Margin = new Padding(0);
            grpList.Name = "grpList";
            grpList.Size = new System.Drawing.Size(366, 331);
            grpList.TabIndex = 2;
            grpList.TabStop = false;
            grpList.Text = "XML-Entries";
            // 
            // listEntries
            // 
            listEntries.BackColor = System.Drawing.Color.FromArgb(22, 26, 34);
            listEntries.BorderStyle = BorderStyle.None;
            listEntries.Dock = DockStyle.Fill;
            listEntries.Font = new System.Drawing.Font("Cascadia Code", 8.5F);
            listEntries.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            listEntries.FullRowSelect = true;
            listEntries.Location = new System.Drawing.Point(3, 19);
            listEntries.Name = "listEntries";
            listEntries.Size = new System.Drawing.Size(360, 291);
            listEntries.TabIndex = 0;
            listEntries.UseCompatibleStateImageBehavior = false;
            listEntries.View = View.Details;
            listEntries.SelectedIndexChanged += listEntries_SelectedIndexChanged;
            listEntries.DoubleClick += listEntries_DoubleClick;
            // 
            // lblEntryCount
            // 
            lblEntryCount.Dock = DockStyle.Bottom;
            lblEntryCount.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            lblEntryCount.ForeColor = System.Drawing.Color.FromArgb(100, 120, 140);
            lblEntryCount.Location = new System.Drawing.Point(3, 310);
            lblEntryCount.Name = "lblEntryCount";
            lblEntryCount.Size = new System.Drawing.Size(360, 18);
            lblEntryCount.TabIndex = 1;
            lblEntryCount.Text = "0 Entries";
            lblEntryCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.ImageScalingSize = new System.Drawing.Size(18, 18);
            toolStrip.Items.AddRange(new ToolStripItem[] { btnNew, btnOpen, btnSave, btnSaveAs, sep1, btnFormat, btnHighlight, sep2, btnCleanup, sep3, btnEditEntry, btnDeleteEntry, sep4, btnRefreshList });
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Padding = new Padding(4, 2, 0, 2);
            toolStrip.Size = new System.Drawing.Size(1362, 27);
            toolStrip.TabIndex = 2;
            // 
            // btnNew
            // 
            btnNew.Name = "btnNew";
            btnNew.Size = new System.Drawing.Size(35, 20);
            btnNew.Text = "New";
            btnNew.ToolTipText = "Create new empty XML";
            btnNew.Click += btnNew_Click;
            // 
            // btnOpen
            // 
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new System.Drawing.Size(40, 20);
            btnOpen.Text = "Open";
            btnOpen.ToolTipText = "Open XML file (Ctrl+O)";
            btnOpen.Click += btnOpen_Click;
            // 
            // btnSave
            // 
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(35, 20);
            btnSave.Text = "Save";
            btnSave.ToolTipText = "Save (Ctrl+S)";
            btnSave.Click += btnSave_Click;
            // 
            // btnSaveAs
            // 
            btnSaveAs.Name = "btnSaveAs";
            btnSaveAs.Size = new System.Drawing.Size(49, 20);
            btnSaveAs.Text = "Save as";
            btnSaveAs.ToolTipText = "Save as...";
            btnSaveAs.Click += btnSaveAs_Click;
            // 
            // sep1
            // 
            sep1.Name = "sep1";
            sep1.Size = new System.Drawing.Size(6, 23);
            // 
            // btnFormat
            // 
            btnFormat.Name = "btnFormat";
            btnFormat.Size = new System.Drawing.Size(49, 20);
            btnFormat.Text = "Format";
            btnFormat.ToolTipText = "Indenting and formatting XML";
            btnFormat.Click += btnFormat_Click;
            // 
            // btnHighlight
            // 
            btnHighlight.Name = "btnHighlight";
            btnHighlight.Size = new System.Drawing.Size(61, 20);
            btnHighlight.Text = "Highlight";
            btnHighlight.ToolTipText = "Rebuild syntax highlighting";
            btnHighlight.Click += btnHighlight_Click;
            // 
            // sep2
            // 
            sep2.Name = "sep2";
            sep2.Size = new System.Drawing.Size(6, 23);
            // 
            // btnCleanup
            // 
            btnCleanup.DropDownItems.AddRange(new ToolStripItem[] { btnCleanAuto, btnCleanStep });
            btnCleanup.Name = "btnCleanup";
            btnCleanup.Size = new System.Drawing.Size(64, 20);
            btnCleanup.Text = "Cleanup";
            btnCleanup.ToolTipText = "Remove entries without animation";
            // 
            // btnCleanAuto
            // 
            btnCleanAuto.Name = "btnCleanAuto";
            btnCleanAuto.Size = new System.Drawing.Size(211, 22);
            btnCleanAuto.Text = "Remove all at once";
            btnCleanAuto.Click += btnCleanAuto_Click;
            // 
            // btnCleanStep
            // 
            btnCleanStep.Name = "btnCleanStep";
            btnCleanStep.Size = new System.Drawing.Size(211, 22);
            btnCleanStep.Text = "Go through it step by step";
            btnCleanStep.Click += btnCleanStep_Click;
            // 
            // sep3
            // 
            sep3.Name = "sep3";
            sep3.Size = new System.Drawing.Size(6, 23);
            // 
            // btnEditEntry
            // 
            btnEditEntry.Name = "btnEditEntry";
            btnEditEntry.Size = new System.Drawing.Size(31, 20);
            btnEditEntry.Text = "Edit";
            btnEditEntry.ToolTipText = "Edit selected entry";
            btnEditEntry.Click += btnEditEntry_Click;
            // 
            // btnDeleteEntry
            // 
            btnDeleteEntry.Name = "btnDeleteEntry";
            btnDeleteEntry.Size = new System.Drawing.Size(44, 20);
            btnDeleteEntry.Text = "Delete";
            btnDeleteEntry.ToolTipText = "Delete selected entry";
            btnDeleteEntry.Click += btnDeleteEntry_Click;
            // 
            // sep4
            // 
            sep4.Name = "sep4";
            sep4.Size = new System.Drawing.Size(6, 23);
            // 
            // btnRefreshList
            // 
            btnRefreshList.Name = "btnRefreshList";
            btnRefreshList.Size = new System.Drawing.Size(49, 20);
            btnRefreshList.Text = "Update";
            btnRefreshList.ToolTipText = "Reload entry list";
            btnRefreshList.Click += btnRefreshList_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new System.Drawing.Point(0, 818);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1362, 22);
            statusStrip.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(1347, 17);
            lblStatus.Spring = true;
            lblStatus.Text = "Ready";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlSearch
            // 
            pnlSearch.BackColor = System.Drawing.Color.FromArgb(30, 36, 46);
            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnSearchFwd);
            pnlSearch.Controls.Add(btnSearchBack);
            pnlSearch.Controls.Add(lblReplace);
            pnlSearch.Controls.Add(txtReplace);
            pnlSearch.Controls.Add(btnReplace);
            pnlSearch.Dock = DockStyle.Top;
            pnlSearch.Location = new System.Drawing.Point(0, 27);
            pnlSearch.Name = "pnlSearch";
            pnlSearch.Padding = new Padding(4, 4, 4, 2);
            pnlSearch.Size = new System.Drawing.Size(1362, 36);
            pnlSearch.TabIndex = 1;
            // 
            // lblSearch
            // 
            lblSearch.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            lblSearch.Location = new System.Drawing.Point(4, 8);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new System.Drawing.Size(52, 20);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            txtSearch.BackColor = System.Drawing.Color.FromArgb(22, 26, 34);
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            txtSearch.Location = new System.Drawing.Point(58, 5);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new System.Drawing.Size(180, 23);
            txtSearch.TabIndex = 1;
            txtSearch.KeyDown += txtSearch_KeyDown;
            // 
            // btnSearchFwd
            // 
            btnSearchFwd.BackColor = System.Drawing.Color.FromArgb(50, 60, 78);
            btnSearchFwd.FlatStyle = FlatStyle.Flat;
            btnSearchFwd.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            btnSearchFwd.Location = new System.Drawing.Point(270, 6);
            btnSearchFwd.Name = "btnSearchFwd";
            btnSearchFwd.Size = new System.Drawing.Size(26, 24);
            btnSearchFwd.TabIndex = 2;
            btnSearchFwd.Text = ">";
            btnSearchFwd.UseVisualStyleBackColor = false;
            btnSearchFwd.Click += btnSearchFwd_Click;
            // 
            // btnSearchBack
            // 
            btnSearchBack.BackColor = System.Drawing.Color.FromArgb(50, 60, 78);
            btnSearchBack.FlatStyle = FlatStyle.Flat;
            btnSearchBack.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            btnSearchBack.Location = new System.Drawing.Point(238, 6);
            btnSearchBack.Name = "btnSearchBack";
            btnSearchBack.Size = new System.Drawing.Size(26, 24);
            btnSearchBack.TabIndex = 3;
            btnSearchBack.Text = "<";
            btnSearchBack.UseVisualStyleBackColor = false;
            btnSearchBack.Click += btnSearchBack_Click;
            // 
            // lblReplace
            // 
            lblReplace.ForeColor = System.Drawing.Color.FromArgb(130, 145, 165);
            lblReplace.Location = new System.Drawing.Point(312, 8);
            lblReplace.Name = "lblReplace";
            lblReplace.Size = new System.Drawing.Size(58, 20);
            lblReplace.TabIndex = 4;
            lblReplace.Text = "Replace:";
            // 
            // txtReplace
            // 
            txtReplace.BackColor = System.Drawing.Color.FromArgb(22, 26, 34);
            txtReplace.BorderStyle = BorderStyle.FixedSingle;
            txtReplace.ForeColor = System.Drawing.Color.FromArgb(200, 215, 235);
            txtReplace.Location = new System.Drawing.Point(374, 5);
            txtReplace.Name = "txtReplace";
            txtReplace.Size = new System.Drawing.Size(180, 23);
            txtReplace.TabIndex = 5;
            // 
            // btnReplace
            // 
            btnReplace.BackColor = System.Drawing.Color.FromArgb(64, 100, 180);
            btnReplace.FlatStyle = FlatStyle.Flat;
            btnReplace.ForeColor = System.Drawing.Color.White;
            btnReplace.Location = new System.Drawing.Point(558, 4);
            btnReplace.Name = "btnReplace";
            btnReplace.Size = new System.Drawing.Size(72, 24);
            btnReplace.TabIndex = 6;
            btnReplace.Text = "Replace";
            btnReplace.UseVisualStyleBackColor = false;
            btnReplace.Click += btnReplace_Click;
            // 
            // pnlPreview
            // 
            pnlPreview.Location = new System.Drawing.Point(0, 0);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Size = new System.Drawing.Size(200, 100);
            pnlPreview.TabIndex = 0;
            // 
            // pnlBrowse
            // 
            pnlBrowse.Location = new System.Drawing.Point(0, 0);
            pnlBrowse.Name = "pnlBrowse";
            pnlBrowse.Size = new System.Drawing.Size(200, 100);
            pnlBrowse.TabIndex = 0;
            // 
            // pnlEdit
            // 
            pnlEdit.Location = new System.Drawing.Point(0, 0);
            pnlEdit.Name = "pnlEdit";
            pnlEdit.Size = new System.Drawing.Size(200, 100);
            pnlEdit.TabIndex = 0;
            // 
            // lblEditTitle
            // 
            lblEditTitle.Location = new System.Drawing.Point(0, 0);
            lblEditTitle.Name = "lblEditTitle";
            lblEditTitle.Size = new System.Drawing.Size(100, 23);
            lblEditTitle.TabIndex = 0;
            // 
            // AnimationListEditorForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1362, 840);
            Controls.Add(splitMain);
            Controls.Add(pnlSearch);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(960, 620);
            Name = "AnimationListEditorForm";
            Text = "AnimationList XML Editor";
            KeyDown += Form_KeyDown;
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            tblRight.ResumeLayout(false);
            grpPreview.ResumeLayout(false);
            grpPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackAction).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackFacing).EndInit();
            grpBrowse.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numBrowseId).EndInit();
            grpList.ResumeLayout(false);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            pnlSearch.ResumeLayout(false);
            pnlSearch.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        // ════════════════════════════════════════════════════════════════════
        //  Event-Handler
        // ════════════════════════════════════════════════════════════════════

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
                DoSearch(true);
            if (e.Control && e.KeyCode == Keys.S)
                SaveXml(_fileName);
            if (e.Control && e.KeyCode == Keys.O)
                btnOpen_Click(sender, e);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            _fileName = "";
            richTextBox.Text = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Graphics>\r\n  <!--Entries-->\r\n</Graphics>";
            ApplySyntaxHighlight();
            _modified = false;
            UpdateTitle();
            PopulateEntryList();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Open XML file",
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                InitialDirectory = Options.AppDataPath
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                LoadXml(dlg.FileName);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_fileName))
                btnSaveAs_Click(sender, e);
            else
                SaveXml(_fileName);
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Save XML As",
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                FileName = "Animationlist.xml",
                InitialDirectory = Options.OutputPath ?? Options.AppDataPath
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _fileName = dlg.FileName;
                SaveXml(_fileName);
            }
        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            richTextBox.Text = FormatXml(richTextBox.Text);
            ApplySyntaxHighlight();
            UpdateStatus("XML formatiert.");
        }

        private void btnHighlight_Click(object sender, EventArgs e)
        {
            ApplySyntaxHighlight();
        }

        private void btnCleanAuto_Click(object sender, EventArgs e) => RunCleanup(false);
        private void btnCleanStep_Click(object sender, EventArgs e) => RunCleanup(true);
        private void btnEditEntry_Click(object sender, EventArgs e) => EditSelectedEntry();
        private void btnDeleteEntry_Click(object sender, EventArgs e) => DeleteSelectedEntry();
        private void btnRefreshList_Click(object sender, EventArgs e) => PopulateEntryList();

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) DoSearch(true);
        }

        private void btnSearchFwd_Click(object sender, EventArgs e) => DoSearch(true);
        private void btnSearchBack_Click(object sender, EventArgs e) => DoSearch(false);
        private void btnReplace_Click(object sender, EventArgs e) => DoReplace();

        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            _modified = true;
            UpdateTitle();
        }

        private void trackAction_ValueChanged(object sender, EventArgs e)
        {
            _previewAction = trackAction.Value;
            lblActionVal.Text = trackAction.Value.ToString();
            LoadPreview(_browseId, _previewAction, _previewFacing);
        }

        private void trackFacing_ValueChanged(object sender, EventArgs e)
        {
            _previewFacing = trackFacing.Value;
            lblFacingVal.Text = trackFacing.Value.ToString();
            LoadPreview(_browseId, _previewAction, _previewFacing);
        }

        private void numBrowseId_ValueChanged(object sender, EventArgs e) => BrowseToId((int)numBrowseId.Value);
        private void btnBrowsePrev_Click(object sender, EventArgs e) => BrowseToId(_browseId - 1);
        private void btnBrowseNext_Click(object sender, EventArgs e) => BrowseToId(_browseId + 1);

        private void btnBrowseNextAnim_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
            for (int id = _browseId + 1; id <= 0xFFFF; id++)
            {
                if (Animations.IsActionDefined(id, 0, 0))
                {
                    BrowseToId(id);
                    break;
                }
            }
            System.Windows.Forms.Cursor.Current = Cursors.Default;
        }

        private void btnAddToXml_Click(object sender, EventArgs e) => AddBrowseIdToXml();
        private void listEntries_DoubleClick(object sender, EventArgs e) => EditSelectedEntry();

        private void listEntries_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listEntries.SelectedItems.Count > 0)
            {
                int.TryParse(listEntries.SelectedItems[0].SubItems[0].Text, out int bid);
                BrowseToId(bid);
                JumpToBodyInEditor(bid);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  Hilfsmethoden (Designer-Seite)
        // ════════════════════════════════════════════════════════════════════

        private void DeleteSelectedEntry()
        {
            if (listEntries.SelectedItems.Count == 0) return;
            int bodyId = int.Parse(listEntries.SelectedItems[0].SubItems[0].Text);
            if (MessageBox.Show(
                $"Eintrag Body {bodyId} wirklich entfernen?", "Loeschen",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(richTextBox.Text);
                XmlElement root = doc["Graphics"];
                if (root == null) return;
                foreach (XmlElement el in root.SelectNodes("Mob|Equip"))
                {
                    if (int.TryParse(el.GetAttribute("body"), out int b) && b == bodyId)
                    { root.RemoveChild(el); break; }
                }
                richTextBox.Text = FormatXml(GetXmlString(doc));
                ApplySyntaxHighlight();
                _modified = true;
                PopulateEntryList();
                UpdateStatus($"Body {bodyId} entfernt.");
            }
            catch { }
        }

        private void AddBrowseIdToXml()
        {
            if (IsBodyInXml(_browseId))
            {
                MessageBox.Show($"Body {_browseId} ist bereits in der XML.",
                    "Bereits vorhanden", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var dlg = new EntryEditDialog(_browseId,
                Animations.GetFileName(_browseId) ?? $"anim_{_browseId}", 0, false);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(richTextBox.Text);
                XmlElement root = doc["Graphics"];
                if (root == null) return;
                XmlElement el = doc.CreateElement(dlg.IsEquip ? "Equip" : "Mob");
                el.SetAttribute("name", dlg.EntryName);
                el.SetAttribute("body", _browseId.ToString());
                el.SetAttribute("type", dlg.EntryType.ToString());
                root.AppendChild(el);
                richTextBox.Text = FormatXml(GetXmlString(doc));
                ApplySyntaxHighlight();
                _modified = true;
                PopulateEntryList();
                UpdateStatus($"Body {_browseId} hinzugefuegt.");
            }
            catch { }
        }

        private void JumpToBodyInEditor(int bodyId)
        {
            string search = $"body=\"{bodyId}\"";
            int idx = richTextBox.Text.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                richTextBox.Select(idx, search.Length);
                richTextBox.ScrollToCaret();
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  Designer-Felder
        // ════════════════════════════════════════════════════════════════════

        
        private SplitContainer splitMain;       
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private Panel pnlSearch;
        private Label lblSearch;
        private Label lblReplace;
        private TextBox txtSearch;
        private TextBox txtReplace;
        private Button btnSearchFwd;
        private Button btnSearchBack;
        private Button btnReplace;        
        private RichTextBox richTextBox;        
        private TableLayoutPanel tblRight;        
        private GroupBox grpPreview;
        private PictureBox picPreview;
        private Label lblPreviewInfo;
        private TrackBar trackAction;
        private TrackBar trackFacing;
        private Label lblActionLbl;
        private Label lblFacingLbl;
        private Label lblActionVal;
        private Label lblFacingVal;       
        private GroupBox grpBrowse;
        private Label lblBrowseIdLbl;
        private NumericUpDown numBrowseId;
        private Button btnBrowsePrev;
        private Button btnBrowseNext;
        private Button btnBrowseNextAnim;
        private Label lblBrowseFile;
        private Label lblBrowseHasAnim;
        private Label lblBrowseInXml;
        private Button btnAddToXml;      
        private GroupBox grpList;
        private ListView listEntries;
        private Label lblEntryCount;        
        private ToolStripButton btnNew;
        private ToolStripButton btnOpen;
        private ToolStripButton btnSave;
        private ToolStripButton btnSaveAs;
        private ToolStripSeparator sep1, sep2, sep3, sep4;
        private ToolStripButton btnFormat;
        private ToolStripButton btnHighlight;
        private ToolStripDropDownButton btnCleanup;
        private ToolStripMenuItem btnCleanAuto;
        private ToolStripMenuItem btnCleanStep;
        private ToolStripButton btnEditEntry;
        private ToolStripButton btnDeleteEntry;
        private ToolStripButton btnRefreshList;        
        private Panel pnlPreview;
        private Panel pnlBrowse;
        private Panel pnlEdit;
        private Label lblEditTitle;        
        private SplitContainer splitRight;
    }
}