/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    partial class EcAnimDefinitionControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            toolStrip = new System.Windows.Forms.ToolStrip();
            toolStripLabelSearch = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonApply = new System.Windows.Forms.ToolStripButton();
            toolStripComboBoxFilter = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelArt = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxArt = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonNew = new System.Windows.Forms.ToolStripButton();
            toolStripButtonDuplicate = new System.Windows.Forms.ToolStripButton();
            toolStripButtonFromClassic = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRemove = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            listViewAnimations = new System.Windows.Forms.ListView();
            columnHeaderId = new System.Windows.Forms.ColumnHeader();
            columnHeaderHex = new System.Windows.Forms.ColumnHeader();
            columnHeaderItemName = new System.Windows.Forms.ColumnHeader();
            columnHeaderFrames = new System.Windows.Forms.ColumnHeader();
            columnHeaderTime = new System.Windows.Forms.ColumnHeader();
            columnHeaderEcImages = new System.Windows.Forms.ColumnHeader();
            columnHeaderClassic = new System.Windows.Forms.ColumnHeader();
            contextMenuStripList = new System.Windows.Forms.ContextMenuStrip(components);
            newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            duplicateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fromClassicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu1 = new System.Windows.Forms.ToolStripSeparator();
            copyIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu2 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitContainerDetail = new System.Windows.Forms.SplitContainer();
            panelFrames = new System.Windows.Forms.Panel();
            listViewFrames = new System.Windows.Forms.ListView();
            columnHeaderFrameNumber = new System.Windows.Forms.ColumnHeader();
            columnHeaderFrameItem = new System.Windows.Forms.ColumnHeader();
            columnHeaderFrameHex = new System.Windows.Forms.ColumnHeader();
            columnHeaderFrameName = new System.Windows.Forms.ColumnHeader();
            columnHeaderFrameImage = new System.Windows.Forms.ColumnHeader();
            columnHeaderFrameClassic = new System.Windows.Forms.ColumnHeader();
            contextMenuStripFrames = new System.Windows.Forms.ContextMenuStrip(components);
            addFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            changeFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorFrames = new System.Windows.Forms.ToolStripSeparator();
            moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripFrames = new System.Windows.Forms.ToolStrip();
            toolStripButtonAddFrame = new System.Windows.Forms.ToolStripButton();
            toolStripButtonChangeFrame = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRemoveFrame = new System.Windows.Forms.ToolStripButton();
            toolStripButtonFrameUp = new System.Windows.Forms.ToolStripButton();
            toolStripButtonFrameDown = new System.Windows.Forms.ToolStripButton();
            flowLayoutPanelTime = new System.Windows.Forms.FlowLayoutPanel();
            labelTime = new System.Windows.Forms.Label();
            numericUpDownFrameTime = new System.Windows.Forms.NumericUpDown();
            buttonPlay = new System.Windows.Forms.Button();
            checkBoxShowEc = new System.Windows.Forms.CheckBox();
            checkBoxShowClassic = new System.Windows.Forms.CheckBox();
            checkBoxGround = new System.Windows.Forms.CheckBox();
            panelPreview = new System.Windows.Forms.Panel();
            splitContainerCompare = new System.Windows.Forms.SplitContainer();
            groupBoxEc = new System.Windows.Forms.GroupBox();
            previewPanel = new EcPreviewPanel();
            labelPreviewInfo = new System.Windows.Forms.Label();
            groupBoxClassic = new System.Windows.Forms.GroupBox();
            previewPanelClassic = new EcPreviewPanel();
            labelClassicInfo = new System.Windows.Forms.Label();
            timerPlay = new System.Windows.Forms.Timer(components);
            timerClassic = new System.Windows.Forms.Timer(components);
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            contextMenuStripList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerDetail).BeginInit();
            splitContainerDetail.Panel1.SuspendLayout();
            splitContainerDetail.Panel2.SuspendLayout();
            splitContainerDetail.SuspendLayout();
            panelFrames.SuspendLayout();
            contextMenuStripFrames.SuspendLayout();
            toolStripFrames.SuspendLayout();
            flowLayoutPanelTime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFrameTime).BeginInit();
            panelPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerCompare).BeginInit();
            splitContainerCompare.Panel1.SuspendLayout();
            splitContainerCompare.Panel2.SuspendLayout();
            splitContainerCompare.SuspendLayout();
            groupBoxEc.SuspendLayout();
            groupBoxClassic.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelSearch, toolStripTextBoxSearch, toolStripButtonApply, toolStripComboBoxFilter, toolStripSeparator1, toolStripLabelArt, toolStripComboBoxArt, toolStripSeparator2, toolStripButtonNew, toolStripButtonDuplicate, toolStripButtonFromClassic, toolStripButtonRemove, toolStripSeparator3, toolStripButtonSave, toolStripButtonSaveAs });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelSearch
            //
            toolStripLabelSearch.Name = "toolStripLabelSearch";
            toolStripLabelSearch.Text = "Search:";
            //
            // toolStripTextBoxSearch
            //
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new System.Drawing.Size(140, 25);
            toolStripTextBoxSearch.ToolTipText = "Id (decimal / 0x hex) or part of the item name – Enter";
            toolStripTextBoxSearch.KeyDown += OnSearchKeyDown;
            //
            // toolStripButtonApply
            //
            toolStripButtonApply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonApply.Name = "toolStripButtonApply";
            toolStripButtonApply.Text = "Apply";
            toolStripButtonApply.Click += OnClickApply;
            //
            // toolStripComboBoxFilter
            //
            toolStripComboBoxFilter.Items.AddRange(new object[] { "All EC animations", "All frames with EC images", "Frames without EC images", "Different from classic", "Not in classic", "Only in classic (missing in EC)" });
            toolStripComboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxFilter.Name = "toolStripComboBoxFilter";
            toolStripComboBoxFilter.Size = new System.Drawing.Size(210, 25);
            toolStripComboBoxFilter.ToolTipText = "Which animations are listed";
            toolStripComboBoxFilter.SelectedIndexChanged += OnFilterChanged;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripLabelArt
            //
            toolStripLabelArt.Name = "toolStripLabelArt";
            toolStripLabelArt.Text = "Art:";
            //
            // toolStripComboBoxArt
            //
            toolStripComboBoxArt.Items.AddRange(new object[] { "Enhanced (EC art)", "Legacy (classic art)" });
            toolStripComboBoxArt.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxArt.Name = "toolStripComboBoxArt";
            toolStripComboBoxArt.Size = new System.Drawing.Size(140, 25);
            toolStripComboBoxArt.ToolTipText = "Images of the EC preview: EC world art (legacy art where the EC has none) or the legacy art";
            toolStripComboBoxArt.SelectedIndexChanged += OnArtChanged;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripButtonNew
            //
            toolStripButtonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonNew.Name = "toolStripButtonNew";
            toolStripButtonNew.Text = "New...";
            toolStripButtonNew.ToolTipText = "New animation for an item id";
            toolStripButtonNew.Click += OnClickNew;
            //
            // toolStripButtonDuplicate
            //
            toolStripButtonDuplicate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonDuplicate.Name = "toolStripButtonDuplicate";
            toolStripButtonDuplicate.Text = "Duplicate...";
            toolStripButtonDuplicate.ToolTipText = "Copy the selected animation to another item id";
            toolStripButtonDuplicate.Click += OnClickDuplicate;
            //
            // toolStripButtonFromClassic
            //
            toolStripButtonFromClassic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonFromClassic.Name = "toolStripButtonFromClassic";
            toolStripButtonFromClassic.Text = "From classic";
            toolStripButtonFromClassic.ToolTipText = "Take the frames and the speed of the classic animdata.mul (creates the EC animation if it is missing)";
            toolStripButtonFromClassic.Click += OnClickFromClassic;
            //
            // toolStripButtonRemove
            //
            toolStripButtonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRemove.Name = "toolStripButtonRemove";
            toolStripButtonRemove.Text = "Remove";
            toolStripButtonRemove.Click += OnClickRemove;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Enabled = false;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Text = "Save";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripButtonSaveAs
            //
            toolStripButtonSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSaveAs.Name = "toolStripButtonSaveAs";
            toolStripButtonSaveAs.Text = "Save as...";
            toolStripButtonSaveAs.Click += OnClickSaveAs;
            //
            // splitContainerMain
            //
            splitContainerMain.Panel1.Controls.Add(listViewAnimations);
            splitContainerMain.Panel2.Controls.Add(splitContainerDetail);
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Size = new System.Drawing.Size(1100, 550);
            splitContainerMain.SplitterDistance = 470;
            splitContainerMain.TabIndex = 1;
            //
            // listViewAnimations
            //
            listViewAnimations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderId, columnHeaderHex, columnHeaderItemName, columnHeaderFrames, columnHeaderTime, columnHeaderEcImages, columnHeaderClassic });
            listViewAnimations.ContextMenuStrip = contextMenuStripList;
            listViewAnimations.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewAnimations.FullRowSelect = true;
            listViewAnimations.HideSelection = false;
            listViewAnimations.MultiSelect = false;
            listViewAnimations.Name = "listViewAnimations";
            listViewAnimations.TabIndex = 0;
            listViewAnimations.UseCompatibleStateImageBehavior = false;
            listViewAnimations.View = System.Windows.Forms.View.Details;
            listViewAnimations.VirtualMode = true;
            listViewAnimations.RetrieveVirtualItem += RetrieveItem;
            listViewAnimations.SelectedIndexChanged += OnListSelectedIndexChanged;
            //
            // columnHeaderId
            //
            columnHeaderId.Text = "Id";
            columnHeaderId.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderId.Width = 60;
            //
            // columnHeaderHex
            //
            columnHeaderHex.Text = "Hex";
            columnHeaderHex.Width = 60;
            //
            // columnHeaderItemName
            //
            columnHeaderItemName.Text = "Item name";
            columnHeaderItemName.Width = 130;
            //
            // columnHeaderFrames
            //
            columnHeaderFrames.Text = "Frames";
            columnHeaderFrames.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderFrames.Width = 50;
            //
            // columnHeaderTime
            //
            columnHeaderTime.Text = "Time";
            columnHeaderTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderTime.Width = 45;
            //
            // columnHeaderEcImages
            //
            columnHeaderEcImages.Text = "EC images";
            columnHeaderEcImages.Width = 65;
            //
            // columnHeaderClassic
            //
            columnHeaderClassic.Text = "Classic";
            columnHeaderClassic.Width = 80;
            //
            // contextMenuStripList
            //
            contextMenuStripList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { newToolStripMenuItem, duplicateToolStripMenuItem, fromClassicToolStripMenuItem, removeToolStripMenuItem, toolStripSeparatorMenu1, copyIdToolStripMenuItem, toolStripSeparatorMenu2, saveToolStripMenuItem });
            contextMenuStripList.Name = "contextMenuStripList";
            //
            // newToolStripMenuItem
            //
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.Text = "New...";
            newToolStripMenuItem.Click += OnClickNew;
            //
            // duplicateToolStripMenuItem
            //
            duplicateToolStripMenuItem.Name = "duplicateToolStripMenuItem";
            duplicateToolStripMenuItem.Text = "Duplicate...";
            duplicateToolStripMenuItem.Click += OnClickDuplicate;
            //
            // fromClassicToolStripMenuItem
            //
            fromClassicToolStripMenuItem.Name = "fromClassicToolStripMenuItem";
            fromClassicToolStripMenuItem.Text = "Take frames from classic";
            fromClassicToolStripMenuItem.Click += OnClickFromClassic;
            //
            // removeToolStripMenuItem
            //
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemove;
            //
            // toolStripSeparatorMenu1
            //
            toolStripSeparatorMenu1.Name = "toolStripSeparatorMenu1";
            //
            // copyIdToolStripMenuItem
            //
            copyIdToolStripMenuItem.Name = "copyIdToolStripMenuItem";
            copyIdToolStripMenuItem.Text = "Copy id (hex)";
            copyIdToolStripMenuItem.Click += OnClickCopyId;
            //
            // toolStripSeparatorMenu2
            //
            toolStripSeparatorMenu2.Name = "toolStripSeparatorMenu2";
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // splitContainerDetail
            //
            splitContainerDetail.Panel1.Controls.Add(panelFrames);
            splitContainerDetail.Panel2.Controls.Add(panelPreview);
            splitContainerDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerDetail.Name = "splitContainerDetail";
            splitContainerDetail.Orientation = System.Windows.Forms.Orientation.Horizontal;
            splitContainerDetail.Size = new System.Drawing.Size(626, 550);
            splitContainerDetail.SplitterDistance = 210;
            splitContainerDetail.TabIndex = 1;
            //
            // panelFrames
            //
            panelFrames.Controls.Add(listViewFrames);
            panelFrames.Controls.Add(toolStripFrames);
            panelFrames.Controls.Add(flowLayoutPanelTime);
            panelFrames.Dock = System.Windows.Forms.DockStyle.Fill;
            panelFrames.Name = "panelFrames";
            panelFrames.TabIndex = 0;
            //
            // listViewFrames
            //
            listViewFrames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderFrameNumber, columnHeaderFrameItem, columnHeaderFrameHex, columnHeaderFrameName, columnHeaderFrameImage, columnHeaderFrameClassic });
            listViewFrames.ContextMenuStrip = contextMenuStripFrames;
            listViewFrames.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewFrames.FullRowSelect = true;
            listViewFrames.HideSelection = false;
            listViewFrames.MultiSelect = false;
            listViewFrames.Name = "listViewFrames";
            listViewFrames.TabIndex = 2;
            listViewFrames.UseCompatibleStateImageBehavior = false;
            listViewFrames.View = System.Windows.Forms.View.Details;
            listViewFrames.SelectedIndexChanged += OnFrameSelectedIndexChanged;
            listViewFrames.DoubleClick += OnClickChangeFrame;
            //
            // columnHeaderFrameNumber
            //
            columnHeaderFrameNumber.Text = "#";
            columnHeaderFrameNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderFrameNumber.Width = 35;
            //
            // columnHeaderFrameItem
            //
            columnHeaderFrameItem.Text = "Item id";
            columnHeaderFrameItem.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderFrameItem.Width = 60;
            //
            // columnHeaderFrameHex
            //
            columnHeaderFrameHex.Text = "Hex";
            columnHeaderFrameHex.Width = 60;
            //
            // columnHeaderFrameName
            //
            columnHeaderFrameName.Text = "Name";
            columnHeaderFrameName.Width = 130;
            //
            // columnHeaderFrameImage
            //
            columnHeaderFrameImage.Text = "Image";
            columnHeaderFrameImage.Width = 150;
            //
            // columnHeaderFrameClassic
            //
            columnHeaderFrameClassic.Text = "Classic";
            columnHeaderFrameClassic.Width = 70;
            //
            // contextMenuStripFrames
            //
            contextMenuStripFrames.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { addFrameToolStripMenuItem, changeFrameToolStripMenuItem, removeFrameToolStripMenuItem, toolStripSeparatorFrames, moveUpToolStripMenuItem, moveDownToolStripMenuItem });
            contextMenuStripFrames.Name = "contextMenuStripFrames";
            //
            // addFrameToolStripMenuItem
            //
            addFrameToolStripMenuItem.Name = "addFrameToolStripMenuItem";
            addFrameToolStripMenuItem.Text = "Add frame...";
            addFrameToolStripMenuItem.Click += OnClickAddFrame;
            //
            // changeFrameToolStripMenuItem
            //
            changeFrameToolStripMenuItem.Name = "changeFrameToolStripMenuItem";
            changeFrameToolStripMenuItem.Text = "Change...";
            changeFrameToolStripMenuItem.Click += OnClickChangeFrame;
            //
            // removeFrameToolStripMenuItem
            //
            removeFrameToolStripMenuItem.Name = "removeFrameToolStripMenuItem";
            removeFrameToolStripMenuItem.Text = "Remove";
            removeFrameToolStripMenuItem.Click += OnClickRemoveFrame;
            //
            // toolStripSeparatorFrames
            //
            toolStripSeparatorFrames.Name = "toolStripSeparatorFrames";
            //
            // moveUpToolStripMenuItem
            //
            moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            moveUpToolStripMenuItem.Text = "Up";
            moveUpToolStripMenuItem.Click += OnClickFrameUp;
            //
            // moveDownToolStripMenuItem
            //
            moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            moveDownToolStripMenuItem.Text = "Down";
            moveDownToolStripMenuItem.Click += OnClickFrameDown;
            //
            // toolStripFrames
            //
            toolStripFrames.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonAddFrame, toolStripButtonChangeFrame, toolStripButtonRemoveFrame, toolStripButtonFrameUp, toolStripButtonFrameDown });
            toolStripFrames.Dock = System.Windows.Forms.DockStyle.Top;
            toolStripFrames.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripFrames.Name = "toolStripFrames";
            toolStripFrames.TabIndex = 1;
            //
            // toolStripButtonAddFrame
            //
            toolStripButtonAddFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAddFrame.Name = "toolStripButtonAddFrame";
            toolStripButtonAddFrame.Text = "Add frame...";
            toolStripButtonAddFrame.Click += OnClickAddFrame;
            //
            // toolStripButtonChangeFrame
            //
            toolStripButtonChangeFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonChangeFrame.Name = "toolStripButtonChangeFrame";
            toolStripButtonChangeFrame.Text = "Change...";
            toolStripButtonChangeFrame.Click += OnClickChangeFrame;
            //
            // toolStripButtonRemoveFrame
            //
            toolStripButtonRemoveFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRemoveFrame.Name = "toolStripButtonRemoveFrame";
            toolStripButtonRemoveFrame.Text = "Remove";
            toolStripButtonRemoveFrame.Click += OnClickRemoveFrame;
            //
            // toolStripButtonFrameUp
            //
            toolStripButtonFrameUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonFrameUp.Name = "toolStripButtonFrameUp";
            toolStripButtonFrameUp.Text = "Up";
            toolStripButtonFrameUp.Click += OnClickFrameUp;
            //
            // toolStripButtonFrameDown
            //
            toolStripButtonFrameDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonFrameDown.Name = "toolStripButtonFrameDown";
            toolStripButtonFrameDown.Text = "Down";
            toolStripButtonFrameDown.Click += OnClickFrameDown;
            //
            // flowLayoutPanelTime
            //
            flowLayoutPanelTime.Controls.Add(labelTime);
            flowLayoutPanelTime.Controls.Add(numericUpDownFrameTime);
            flowLayoutPanelTime.Controls.Add(buttonPlay);
            flowLayoutPanelTime.Controls.Add(checkBoxShowEc);
            flowLayoutPanelTime.Controls.Add(checkBoxShowClassic);
            flowLayoutPanelTime.Controls.Add(checkBoxGround);
            flowLayoutPanelTime.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelTime.Name = "flowLayoutPanelTime";
            flowLayoutPanelTime.Padding = new System.Windows.Forms.Padding(4);
            flowLayoutPanelTime.Size = new System.Drawing.Size(626, 32);
            flowLayoutPanelTime.TabIndex = 0;
            flowLayoutPanelTime.WrapContents = false;
            //
            // labelTime
            //
            labelTime.AutoSize = true;
            labelTime.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            labelTime.Name = "labelTime";
            labelTime.Text = "Time per frame (s):";
            //
            // numericUpDownFrameTime
            //
            numericUpDownFrameTime.DecimalPlaces = 3;
            numericUpDownFrameTime.Increment = new decimal(new int[] { 25, 0, 0, 196608 });
            numericUpDownFrameTime.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            numericUpDownFrameTime.Minimum = new decimal(new int[] { 1, 0, 0, 196608 });
            numericUpDownFrameTime.Name = "numericUpDownFrameTime";
            numericUpDownFrameTime.Size = new System.Drawing.Size(70, 23);
            numericUpDownFrameTime.TabIndex = 1;
            numericUpDownFrameTime.Value = new decimal(new int[] { 1, 0, 0, 196608 });
            numericUpDownFrameTime.ValueChanged += OnFrameTimeChanged;
            //
            // buttonPlay
            //
            buttonPlay.Name = "buttonPlay";
            buttonPlay.Size = new System.Drawing.Size(60, 23);
            buttonPlay.TabIndex = 2;
            buttonPlay.Text = "Play";
            buttonPlay.UseVisualStyleBackColor = true;
            buttonPlay.Click += OnClickPlay;
            //
            // checkBoxShowEc
            //
            checkBoxShowEc.AutoSize = true;
            checkBoxShowEc.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxShowEc.Checked = true;
            checkBoxShowEc.Margin = new System.Windows.Forms.Padding(12, 6, 0, 0);
            checkBoxShowEc.Name = "checkBoxShowEc";
            checkBoxShowEc.TabIndex = 3;
            checkBoxShowEc.Text = "EC";
            checkBoxShowEc.UseVisualStyleBackColor = true;
            checkBoxShowEc.CheckedChanged += OnCompareViewChanged;
            //
            // checkBoxShowClassic
            //
            checkBoxShowClassic.AutoSize = true;
            checkBoxShowClassic.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxShowClassic.Checked = true;
            checkBoxShowClassic.Margin = new System.Windows.Forms.Padding(6, 6, 0, 0);
            checkBoxShowClassic.Name = "checkBoxShowClassic";
            checkBoxShowClassic.TabIndex = 4;
            checkBoxShowClassic.Text = "Classic";
            checkBoxShowClassic.UseVisualStyleBackColor = true;
            checkBoxShowClassic.CheckedChanged += OnCompareViewChanged;
            //
            // checkBoxGround
            //
            checkBoxGround.AutoSize = true;
            checkBoxGround.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxGround.Checked = true;
            checkBoxGround.Margin = new System.Windows.Forms.Padding(6, 6, 0, 0);
            checkBoxGround.Name = "checkBoxGround";
            checkBoxGround.TabIndex = 5;
            checkBoxGround.Text = "Tile";
            checkBoxGround.UseVisualStyleBackColor = true;
            checkBoxGround.CheckedChanged += OnGroundChanged;
            //
            // panelPreview
            //
            panelPreview.Controls.Add(splitContainerCompare);
            panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            panelPreview.Name = "panelPreview";
            panelPreview.TabIndex = 0;
            //
            // splitContainerCompare
            //
            splitContainerCompare.Panel1.Controls.Add(groupBoxEc);
            splitContainerCompare.Panel2.Controls.Add(groupBoxClassic);
            splitContainerCompare.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerCompare.Name = "splitContainerCompare";
            splitContainerCompare.Size = new System.Drawing.Size(626, 330);
            splitContainerCompare.SplitterDistance = 310;
            splitContainerCompare.TabIndex = 1;
            //
            // groupBoxEc
            //
            groupBoxEc.Controls.Add(previewPanel);
            groupBoxEc.Controls.Add(labelPreviewInfo);
            groupBoxEc.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxEc.Name = "groupBoxEc";
            groupBoxEc.TabIndex = 0;
            groupBoxEc.TabStop = false;
            groupBoxEc.Text = "Enhanced Client (AnimationDefinition.uop)";
            //
            // previewPanel
            //
            previewPanel.AutoScroll = true;
            previewPanel.BackColor = System.Drawing.Color.White;
            previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            previewPanel.Name = "previewPanel";
            previewPanel.TabIndex = 1;
            //
            // labelPreviewInfo
            //
            labelPreviewInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelPreviewInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelPreviewInfo.Name = "labelPreviewInfo";
            labelPreviewInfo.Padding = new System.Windows.Forms.Padding(4);
            labelPreviewInfo.Size = new System.Drawing.Size(300, 48);
            //
            // groupBoxClassic
            //
            groupBoxClassic.Controls.Add(previewPanelClassic);
            groupBoxClassic.Controls.Add(labelClassicInfo);
            groupBoxClassic.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxClassic.Name = "groupBoxClassic";
            groupBoxClassic.TabIndex = 0;
            groupBoxClassic.TabStop = false;
            groupBoxClassic.Text = "Classic client (animdata.mul + art)";
            //
            // previewPanelClassic
            //
            previewPanelClassic.AutoScroll = true;
            previewPanelClassic.BackColor = System.Drawing.Color.White;
            previewPanelClassic.Dock = System.Windows.Forms.DockStyle.Fill;
            previewPanelClassic.Name = "previewPanelClassic";
            previewPanelClassic.TabIndex = 1;
            previewPanelClassic.Zoom = 1.5F;
            //
            // labelClassicInfo
            //
            labelClassicInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelClassicInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelClassicInfo.Name = "labelClassicInfo";
            labelClassicInfo.Padding = new System.Windows.Forms.Padding(4);
            labelClassicInfo.Size = new System.Drawing.Size(300, 48);
            //
            // timerPlay
            //
            timerPlay.Tick += OnTimerTick;
            //
            // timerClassic
            //
            timerClassic.Tick += OnTimerClassicTick;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 2;
            //
            // toolStripStatusLabel
            //
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Spring = true;
            toolStripStatusLabel.Text = "Choose the Enhanced Client folder on top.";
            toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // EcAnimDefinitionControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainerMain);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcAnimDefinitionControl";
            Size = new System.Drawing.Size(1100, 600);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            contextMenuStripList.ResumeLayout(false);
            splitContainerDetail.Panel1.ResumeLayout(false);
            splitContainerDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerDetail).EndInit();
            splitContainerDetail.ResumeLayout(false);
            panelFrames.ResumeLayout(false);
            panelFrames.PerformLayout();
            contextMenuStripFrames.ResumeLayout(false);
            toolStripFrames.ResumeLayout(false);
            toolStripFrames.PerformLayout();
            flowLayoutPanelTime.ResumeLayout(false);
            flowLayoutPanelTime.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFrameTime).EndInit();
            panelPreview.ResumeLayout(false);
            panelPreview.PerformLayout();
            splitContainerCompare.Panel1.ResumeLayout(false);
            splitContainerCompare.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerCompare).EndInit();
            splitContainerCompare.ResumeLayout(false);
            groupBoxEc.ResumeLayout(false);
            groupBoxEc.PerformLayout();
            groupBoxClassic.ResumeLayout(false);
            groupBoxClassic.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSearch;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonApply;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxFilter;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabelArt;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxArt;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonNew;
        private System.Windows.Forms.ToolStripButton toolStripButtonDuplicate;
        private System.Windows.Forms.ToolStripButton toolStripButtonFromClassic;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.ListView listViewAnimations;
        private System.Windows.Forms.ColumnHeader columnHeaderId;
        private System.Windows.Forms.ColumnHeader columnHeaderHex;
        private System.Windows.Forms.ColumnHeader columnHeaderItemName;
        private System.Windows.Forms.ColumnHeader columnHeaderFrames;
        private System.Windows.Forms.ColumnHeader columnHeaderTime;
        private System.Windows.Forms.ColumnHeader columnHeaderEcImages;
        private System.Windows.Forms.ColumnHeader columnHeaderClassic;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripList;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplicateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromClassicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu1;
        private System.Windows.Forms.ToolStripMenuItem copyIdToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu2;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerDetail;
        private System.Windows.Forms.Panel panelFrames;
        private System.Windows.Forms.ListView listViewFrames;
        private System.Windows.Forms.ColumnHeader columnHeaderFrameNumber;
        private System.Windows.Forms.ColumnHeader columnHeaderFrameItem;
        private System.Windows.Forms.ColumnHeader columnHeaderFrameHex;
        private System.Windows.Forms.ColumnHeader columnHeaderFrameName;
        private System.Windows.Forms.ColumnHeader columnHeaderFrameImage;
        private System.Windows.Forms.ColumnHeader columnHeaderFrameClassic;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFrames;
        private System.Windows.Forms.ToolStripMenuItem addFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFrames;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStripFrames;
        private System.Windows.Forms.ToolStripButton toolStripButtonAddFrame;
        private System.Windows.Forms.ToolStripButton toolStripButtonChangeFrame;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemoveFrame;
        private System.Windows.Forms.ToolStripButton toolStripButtonFrameUp;
        private System.Windows.Forms.ToolStripButton toolStripButtonFrameDown;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelTime;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.NumericUpDown numericUpDownFrameTime;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.CheckBox checkBoxShowEc;
        private System.Windows.Forms.CheckBox checkBoxShowClassic;
        private System.Windows.Forms.CheckBox checkBoxGround;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.SplitContainer splitContainerCompare;
        private System.Windows.Forms.GroupBox groupBoxEc;
        private EcPreviewPanel previewPanel;
        private System.Windows.Forms.Label labelPreviewInfo;
        private System.Windows.Forms.GroupBox groupBoxClassic;
        private EcPreviewPanel previewPanelClassic;
        private System.Windows.Forms.Label labelClassicInfo;
        private System.Windows.Forms.Timer timerPlay;
        private System.Windows.Forms.Timer timerClassic;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}
