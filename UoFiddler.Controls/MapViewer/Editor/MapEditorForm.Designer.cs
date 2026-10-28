namespace UoFiddler.Controls.MapViewer.Editor
{
    partial class MapEditorForm
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
            if (disposing)
            {
                components?.Dispose();
                DisposeResources();
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
            toolStripMain = new System.Windows.Forms.ToolStrip();
            toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
            toolStripButtonUndoLast = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRedo = new System.Windows.Forms.ToolStripButton();
            toolStripButtonUndoAll = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonBackups = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonZoomIn = new System.Windows.Forms.ToolStripButton();
            toolStripButtonZoomOut = new System.Windows.Forms.ToolStripButton();
            toolStripButtonFit = new System.Windows.Forms.ToolStripButton();
            toolStripLabelZoom = new System.Windows.Forms.ToolStripLabel();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonGrid = new System.Windows.Forms.ToolStripButton();
            toolStripButtonStatics = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonOpenViewer = new System.Windows.Forms.ToolStripButton();
            statusStrip = new System.Windows.Forms.StatusStrip();
            statusLabelInfo = new System.Windows.Forms.ToolStripStatusLabel();
            statusLabelCell = new System.Windows.Forms.ToolStripStatusLabel();
            statusLabelChanges = new System.Windows.Forms.ToolStripStatusLabel();
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            tabControlSide = new System.Windows.Forms.TabControl();
            tabPageSource = new System.Windows.Forms.TabPage();
            mapOverviewSource = new MapOverviewControl();
            panelSourceBottom = new System.Windows.Forms.Panel();
            labelArea = new System.Windows.Forms.Label();
            labelSourceX = new System.Windows.Forms.Label();
            numericSourceX = new System.Windows.Forms.NumericUpDown();
            labelSourceY = new System.Windows.Forms.Label();
            numericSourceY = new System.Windows.Forms.NumericUpDown();
            labelSourceW = new System.Windows.Forms.Label();
            numericSourceW = new System.Windows.Forms.NumericUpDown();
            labelSourceH = new System.Windows.Forms.Label();
            numericSourceH = new System.Windows.Forms.NumericUpDown();
            checkBoxCopyLand = new System.Windows.Forms.CheckBox();
            checkBoxCopyStatics = new System.Windows.Forms.CheckBox();
            buttonCopy = new System.Windows.Forms.Button();
            buttonUseMapMark = new System.Windows.Forms.Button();
            labelSourceHint = new System.Windows.Forms.Label();
            panelSourceTop = new System.Windows.Forms.Panel();
            labelSourceMap = new System.Windows.Forms.Label();
            comboBoxSourceMap = new System.Windows.Forms.ComboBox();
            buttonSourceExternal = new System.Windows.Forms.Button();
            checkBoxSourceStatics = new System.Windows.Forms.CheckBox();
            tabPageClip = new System.Windows.Forms.TabPage();
            listViewClipIds = new System.Windows.Forms.ListView();
            columnHeaderId = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderCount = new System.Windows.Forms.ColumnHeader();
            panelClipBottom = new System.Windows.Forms.Panel();
            labelZRange = new System.Windows.Forms.Label();
            numericMinZ = new System.Windows.Forms.NumericUpDown();
            labelZTo = new System.Windows.Forms.Label();
            numericMaxZ = new System.Windows.Forms.NumericUpDown();
            buttonCheckAll = new System.Windows.Forms.Button();
            buttonCheckNone = new System.Windows.Forms.Button();
            buttonRemoveUnchecked = new System.Windows.Forms.Button();
            buttonAddStatic = new System.Windows.Forms.Button();
            buttonClipToLibrary = new System.Windows.Forms.Button();
            panelClipButtons = new System.Windows.Forms.Panel();
            buttonRotate = new System.Windows.Forms.Button();
            buttonMirrorX = new System.Windows.Forms.Button();
            buttonMirrorY = new System.Windows.Forms.Button();
            buttonClearClip = new System.Windows.Forms.Button();
            pictureBoxClip = new System.Windows.Forms.PictureBox();
            labelClipInfo = new System.Windows.Forms.Label();
            tabPageTarget = new System.Windows.Forms.TabPage();
            mapOverviewTarget = new MapOverviewControl();
            panelTargetBottom = new System.Windows.Forms.Panel();
            checkBoxPasteLand = new System.Windows.Forms.CheckBox();
            checkBoxPasteStatics = new System.Windows.Forms.CheckBox();
            checkBoxFollowGround = new System.Windows.Forms.CheckBox();
            labelHeightMode = new System.Windows.Forms.Label();
            comboBoxHeightMode = new System.Windows.Forms.ComboBox();
            labelZOffset = new System.Windows.Forms.Label();
            numericZOffset = new System.Windows.Forms.NumericUpDown();
            labelBlend = new System.Windows.Forms.Label();
            numericBlend = new System.Windows.Forms.NumericUpDown();
            labelRemove = new System.Windows.Forms.Label();
            comboBoxRemove = new System.Windows.Forms.ComboBox();
            checkBoxLivePreview = new System.Windows.Forms.CheckBox();
            buttonPasteMode = new System.Windows.Forms.Button();
            buttonPasteNow = new System.Windows.Forms.Button();
            buttonPasteCancel = new System.Windows.Forms.Button();
            labelPasteResult = new System.Windows.Forms.Label();
            panelTargetTop = new System.Windows.Forms.Panel();
            labelTargetMap = new System.Windows.Forms.Label();
            comboBoxTargetMap = new System.Windows.Forms.ComboBox();
            buttonTargetExternal = new System.Windows.Forms.Button();
            labelWorkSize = new System.Windows.Forms.Label();
            numericWorkSize = new System.Windows.Forms.NumericUpDown();
            labelWorkHint = new System.Windows.Forms.Label();
            isoView = new IsoViewControl();
            toolTip = new System.Windows.Forms.ToolTip(components);
            previewTimer = new System.Windows.Forms.Timer(components);
            toolStripMain.SuspendLayout();
            statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            tabControlSide.SuspendLayout();
            tabPageSource.SuspendLayout();
            panelSourceBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericSourceX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericSourceY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericSourceW).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericSourceH).BeginInit();
            panelSourceTop.SuspendLayout();
            tabPageClip.SuspendLayout();
            panelClipBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericMinZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericMaxZ).BeginInit();
            panelClipButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxClip).BeginInit();
            tabPageTarget.SuspendLayout();
            panelTargetBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericZOffset).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericBlend).BeginInit();
            panelTargetTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericWorkSize).BeginInit();
            SuspendLayout();
            //
            // toolStripMain
            //
            toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonUndoLast, toolStripButtonRedo, toolStripButtonUndoAll, toolStripSeparator1, toolStripButtonSave, toolStripButtonBackups, toolStripSeparator2, toolStripButtonZoomIn, toolStripButtonZoomOut, toolStripButtonFit, toolStripLabelZoom, toolStripSeparator3, toolStripButtonGrid, toolStripButtonStatics, toolStripSeparator4, toolStripButtonOpenViewer, toolStripButtonHelp });
            toolStripMain.Location = new System.Drawing.Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Size = new System.Drawing.Size(1280, 25);
            toolStripMain.TabIndex = 0;
            //
            // toolStripButtonRedo
            //
            toolStripButtonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRedo.Enabled = false;
            toolStripButtonRedo.Name = "toolStripButtonRedo";
            toolStripButtonRedo.Size = new System.Drawing.Size(38, 22);
            toolStripButtonRedo.Text = "Redo";
            toolStripButtonRedo.ToolTipText = "Redo (Ctrl+Y)";
            toolStripButtonRedo.Click += OnClickRedo;
            //
            // toolStripButtonUndoLast
            //
            toolStripButtonUndoLast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonUndoLast.Enabled = false;
            toolStripButtonUndoLast.Name = "toolStripButtonUndoLast";
            toolStripButtonUndoLast.Size = new System.Drawing.Size(40, 22);
            toolStripButtonUndoLast.Text = "Undo";
            toolStripButtonUndoLast.Click += OnClickUndoLast;
            //
            // toolStripButtonUndoAll
            //
            toolStripButtonUndoAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonUndoAll.Enabled = false;
            toolStripButtonUndoAll.Name = "toolStripButtonUndoAll";
            toolStripButtonUndoAll.Size = new System.Drawing.Size(56, 22);
            toolStripButtonUndoAll.Text = "Undo all";
            toolStripButtonUndoAll.ToolTipText = "Undo all changes of the target map made in this window";
            toolStripButtonUndoAll.Click += OnClickUndoAll;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Enabled = false;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Size = new System.Drawing.Size(44, 22);
            toolStripButtonSave.Text = "Save...";
            toolStripButtonSave.ToolTipText = "Save the target map (choose directory, file number, land/statics, backup)";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripButtonBackups
            //
            toolStripButtonBackups.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonBackups.Name = "toolStripButtonBackups";
            toolStripButtonBackups.Size = new System.Drawing.Size(62, 22);
            toolStripButtonBackups.Text = "Backups...";
            toolStripButtonBackups.Click += OnClickBackups;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripButtonHelp
            //
            toolStripButtonHelp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripButtonHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonHelp.Name = "toolStripButtonHelp";
            toolStripButtonHelp.Size = new System.Drawing.Size(60, 22);
            toolStripButtonHelp.Text = "Help (F1)";
            toolStripButtonHelp.ToolTipText = "Explanation of all functions";
            toolStripButtonHelp.Click += OnClickHelp;
            //
            // toolStripButtonZoomIn
            //
            toolStripButtonZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonZoomIn.Name = "toolStripButtonZoomIn";
            toolStripButtonZoomIn.Size = new System.Drawing.Size(23, 22);
            toolStripButtonZoomIn.Text = "+";
            toolStripButtonZoomIn.ToolTipText = "Zoom in";
            toolStripButtonZoomIn.Click += OnClickZoomIn;
            //
            // toolStripButtonZoomOut
            //
            toolStripButtonZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonZoomOut.Name = "toolStripButtonZoomOut";
            toolStripButtonZoomOut.Size = new System.Drawing.Size(23, 22);
            toolStripButtonZoomOut.Text = "-";
            toolStripButtonZoomOut.ToolTipText = "Zoom out";
            toolStripButtonZoomOut.Click += OnClickZoomOut;
            //
            // toolStripButtonFit
            //
            toolStripButtonFit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonFit.Name = "toolStripButtonFit";
            toolStripButtonFit.Size = new System.Drawing.Size(26, 22);
            toolStripButtonFit.Text = "Fit";
            toolStripButtonFit.Click += OnClickFit;
            //
            // toolStripLabelZoom
            //
            toolStripLabelZoom.Name = "toolStripLabelZoom";
            toolStripLabelZoom.Size = new System.Drawing.Size(35, 22);
            toolStripLabelZoom.Text = "100%";
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripButtonGrid
            //
            toolStripButtonGrid.CheckOnClick = true;
            toolStripButtonGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonGrid.Name = "toolStripButtonGrid";
            toolStripButtonGrid.Size = new System.Drawing.Size(33, 22);
            toolStripButtonGrid.Text = "Grid";
            toolStripButtonGrid.Click += OnViewOptionChanged;
            //
            // toolStripButtonStatics
            //
            toolStripButtonStatics.Checked = true;
            toolStripButtonStatics.CheckOnClick = true;
            toolStripButtonStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonStatics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonStatics.Name = "toolStripButtonStatics";
            toolStripButtonStatics.Size = new System.Drawing.Size(46, 22);
            toolStripButtonStatics.Text = "Statics";
            toolStripButtonStatics.Click += OnViewOptionChanged;
            //
            // toolStripSeparator4
            //
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripButtonOpenViewer
            //
            toolStripButtonOpenViewer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonOpenViewer.Name = "toolStripButtonOpenViewer";
            toolStripButtonOpenViewer.Size = new System.Drawing.Size(136, 22);
            toolStripButtonOpenViewer.Text = "Edit in Region Viewer...";
            toolStripButtonOpenViewer.ToolTipText = "Opens the working area in the Region Viewer (same undo steps): check errors, edit statics, build";
            toolStripButtonOpenViewer.Click += OnClickOpenViewer;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusLabelInfo, statusLabelCell, statusLabelChanges });
            statusStrip.Location = new System.Drawing.Point(0, 778);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1280, 22);
            statusStrip.TabIndex = 2;
            //
            // statusLabelInfo
            //
            statusLabelInfo.Name = "statusLabelInfo";
            statusLabelInfo.Size = new System.Drawing.Size(900, 17);
            statusLabelInfo.Spring = true;
            statusLabelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // statusLabelCell
            //
            statusLabelCell.Name = "statusLabelCell";
            statusLabelCell.Size = new System.Drawing.Size(200, 17);
            //
            // statusLabelChanges
            //
            statusLabelChanges.ForeColor = System.Drawing.Color.DarkRed;
            statusLabelChanges.Name = "statusLabelChanges";
            statusLabelChanges.Size = new System.Drawing.Size(150, 17);
            //
            // splitContainerMain
            //
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainerMain.Location = new System.Drawing.Point(0, 25);
            splitContainerMain.Name = "splitContainerMain";
            //
            // splitContainerMain.Panel1
            //
            splitContainerMain.Panel1.Controls.Add(tabControlSide);
            splitContainerMain.Panel1MinSize = 300;
            //
            // splitContainerMain.Panel2
            //
            splitContainerMain.Panel2.Controls.Add(isoView);
            splitContainerMain.Size = new System.Drawing.Size(1280, 753);
            splitContainerMain.SplitterDistance = 390;
            splitContainerMain.TabIndex = 1;
            //
            // tabControlSide
            //
            tabControlSide.Controls.Add(tabPageSource);
            tabControlSide.Controls.Add(tabPageClip);
            tabControlSide.Controls.Add(tabPageTarget);
            tabControlSide.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControlSide.Location = new System.Drawing.Point(0, 0);
            tabControlSide.Name = "tabControlSide";
            tabControlSide.SelectedIndex = 0;
            tabControlSide.Size = new System.Drawing.Size(390, 753);
            tabControlSide.TabIndex = 0;
            //
            // tabPageSource
            //
            tabPageSource.Controls.Add(mapOverviewSource);
            tabPageSource.Controls.Add(panelSourceBottom);
            tabPageSource.Controls.Add(panelSourceTop);
            tabPageSource.Location = new System.Drawing.Point(4, 24);
            tabPageSource.Name = "tabPageSource";
            tabPageSource.Size = new System.Drawing.Size(382, 725);
            tabPageSource.TabIndex = 0;
            tabPageSource.Text = "1. Source";
            tabPageSource.UseVisualStyleBackColor = true;
            //
            // mapOverviewSource
            //
            mapOverviewSource.Dock = System.Windows.Forms.DockStyle.Fill;
            mapOverviewSource.Location = new System.Drawing.Point(0, 62);
            mapOverviewSource.Name = "mapOverviewSource";
            mapOverviewSource.Size = new System.Drawing.Size(382, 503);
            mapOverviewSource.TabIndex = 1;
            mapOverviewSource.SelectionChanged += OnSourceSelectionChanged;
            mapOverviewSource.PositionHover += OnOverviewHover;
            //
            // panelSourceBottom
            //
            panelSourceBottom.Controls.Add(labelArea);
            panelSourceBottom.Controls.Add(labelSourceX);
            panelSourceBottom.Controls.Add(numericSourceX);
            panelSourceBottom.Controls.Add(labelSourceY);
            panelSourceBottom.Controls.Add(numericSourceY);
            panelSourceBottom.Controls.Add(labelSourceW);
            panelSourceBottom.Controls.Add(numericSourceW);
            panelSourceBottom.Controls.Add(labelSourceH);
            panelSourceBottom.Controls.Add(numericSourceH);
            panelSourceBottom.Controls.Add(checkBoxCopyLand);
            panelSourceBottom.Controls.Add(checkBoxCopyStatics);
            panelSourceBottom.Controls.Add(buttonCopy);
            panelSourceBottom.Controls.Add(buttonUseMapMark);
            panelSourceBottom.Controls.Add(labelSourceHint);
            panelSourceBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelSourceBottom.Location = new System.Drawing.Point(0, 565);
            panelSourceBottom.Name = "panelSourceBottom";
            panelSourceBottom.Size = new System.Drawing.Size(382, 160);
            panelSourceBottom.TabIndex = 2;
            //
            // labelArea
            //
            labelArea.AutoSize = true;
            labelArea.Location = new System.Drawing.Point(6, 6);
            labelArea.Name = "labelArea";
            labelArea.Size = new System.Drawing.Size(150, 15);
            labelArea.TabIndex = 0;
            labelArea.Text = "Area to copy (map tiles):";
            //
            // labelSourceX
            //
            labelSourceX.AutoSize = true;
            labelSourceX.Location = new System.Drawing.Point(6, 32);
            labelSourceX.Name = "labelSourceX";
            labelSourceX.Size = new System.Drawing.Size(14, 15);
            labelSourceX.TabIndex = 1;
            labelSourceX.Text = "X";
            //
            // numericSourceX
            //
            numericSourceX.Location = new System.Drawing.Point(22, 30);
            numericSourceX.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericSourceX.Name = "numericSourceX";
            numericSourceX.Size = new System.Drawing.Size(66, 23);
            numericSourceX.TabIndex = 2;
            numericSourceX.ValueChanged += OnSourceNumericChanged;
            //
            // labelSourceY
            //
            labelSourceY.AutoSize = true;
            labelSourceY.Location = new System.Drawing.Point(94, 32);
            labelSourceY.Name = "labelSourceY";
            labelSourceY.Size = new System.Drawing.Size(14, 15);
            labelSourceY.TabIndex = 3;
            labelSourceY.Text = "Y";
            //
            // numericSourceY
            //
            numericSourceY.Location = new System.Drawing.Point(110, 30);
            numericSourceY.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericSourceY.Name = "numericSourceY";
            numericSourceY.Size = new System.Drawing.Size(66, 23);
            numericSourceY.TabIndex = 4;
            numericSourceY.ValueChanged += OnSourceNumericChanged;
            //
            // labelSourceW
            //
            labelSourceW.AutoSize = true;
            labelSourceW.Location = new System.Drawing.Point(182, 32);
            labelSourceW.Name = "labelSourceW";
            labelSourceW.Size = new System.Drawing.Size(18, 15);
            labelSourceW.TabIndex = 5;
            labelSourceW.Text = "W";
            //
            // numericSourceW
            //
            numericSourceW.Location = new System.Drawing.Point(202, 30);
            numericSourceW.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
            numericSourceW.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericSourceW.Name = "numericSourceW";
            numericSourceW.Size = new System.Drawing.Size(60, 23);
            numericSourceW.TabIndex = 6;
            numericSourceW.Value = new decimal(new int[] { 32, 0, 0, 0 });
            numericSourceW.ValueChanged += OnSourceNumericChanged;
            //
            // labelSourceH
            //
            labelSourceH.AutoSize = true;
            labelSourceH.Location = new System.Drawing.Point(268, 32);
            labelSourceH.Name = "labelSourceH";
            labelSourceH.Size = new System.Drawing.Size(16, 15);
            labelSourceH.TabIndex = 7;
            labelSourceH.Text = "H";
            //
            // numericSourceH
            //
            numericSourceH.Location = new System.Drawing.Point(286, 30);
            numericSourceH.Maximum = new decimal(new int[] { 1024, 0, 0, 0 });
            numericSourceH.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericSourceH.Name = "numericSourceH";
            numericSourceH.Size = new System.Drawing.Size(60, 23);
            numericSourceH.TabIndex = 8;
            numericSourceH.Value = new decimal(new int[] { 32, 0, 0, 0 });
            numericSourceH.ValueChanged += OnSourceNumericChanged;
            //
            // checkBoxCopyLand
            //
            checkBoxCopyLand.AutoSize = true;
            checkBoxCopyLand.Checked = true;
            checkBoxCopyLand.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCopyLand.Location = new System.Drawing.Point(8, 62);
            checkBoxCopyLand.Name = "checkBoxCopyLand";
            checkBoxCopyLand.Size = new System.Drawing.Size(52, 19);
            checkBoxCopyLand.TabIndex = 9;
            checkBoxCopyLand.Text = "Land";
            checkBoxCopyLand.UseVisualStyleBackColor = true;
            //
            // checkBoxCopyStatics
            //
            checkBoxCopyStatics.AutoSize = true;
            checkBoxCopyStatics.Checked = true;
            checkBoxCopyStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCopyStatics.Location = new System.Drawing.Point(80, 62);
            checkBoxCopyStatics.Name = "checkBoxCopyStatics";
            checkBoxCopyStatics.Size = new System.Drawing.Size(62, 19);
            checkBoxCopyStatics.TabIndex = 10;
            checkBoxCopyStatics.Text = "Statics";
            checkBoxCopyStatics.UseVisualStyleBackColor = true;
            //
            // buttonCopy
            //
            buttonCopy.Location = new System.Drawing.Point(6, 88);
            buttonCopy.Name = "buttonCopy";
            buttonCopy.Size = new System.Drawing.Size(170, 28);
            buttonCopy.TabIndex = 11;
            buttonCopy.Text = "Copy area to the clip";
            buttonCopy.UseVisualStyleBackColor = true;
            buttonCopy.Click += OnClickCopy;
            //
            // buttonUseMapMark
            //
            buttonUseMapMark.Location = new System.Drawing.Point(182, 88);
            buttonUseMapMark.Name = "buttonUseMapMark";
            buttonUseMapMark.Size = new System.Drawing.Size(180, 28);
            buttonUseMapMark.TabIndex = 12;
            buttonUseMapMark.Text = "Use mark of the map tab";
            buttonUseMapMark.UseVisualStyleBackColor = true;
            buttonUseMapMark.Click += OnClickUseMapMark;
            //
            // labelSourceHint
            //
            labelSourceHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelSourceHint.Location = new System.Drawing.Point(6, 120);
            labelSourceHint.Name = "labelSourceHint";
            labelSourceHint.Size = new System.Drawing.Size(370, 36);
            labelSourceHint.TabIndex = 13;
            labelSourceHint.Text = "Left mouse: mark an area. Right mouse: move. Wheel: zoom.";
            //
            // panelSourceTop
            //
            panelSourceTop.Controls.Add(labelSourceMap);
            panelSourceTop.Controls.Add(comboBoxSourceMap);
            panelSourceTop.Controls.Add(buttonSourceExternal);
            panelSourceTop.Controls.Add(checkBoxSourceStatics);
            panelSourceTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelSourceTop.Location = new System.Drawing.Point(0, 0);
            panelSourceTop.Name = "panelSourceTop";
            panelSourceTop.Size = new System.Drawing.Size(382, 62);
            panelSourceTop.TabIndex = 0;
            //
            // labelSourceMap
            //
            labelSourceMap.AutoSize = true;
            labelSourceMap.Location = new System.Drawing.Point(6, 9);
            labelSourceMap.Name = "labelSourceMap";
            labelSourceMap.Size = new System.Drawing.Size(34, 15);
            labelSourceMap.TabIndex = 0;
            labelSourceMap.Text = "Map:";
            //
            // comboBoxSourceMap
            //
            comboBoxSourceMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxSourceMap.Location = new System.Drawing.Point(46, 6);
            comboBoxSourceMap.Name = "comboBoxSourceMap";
            comboBoxSourceMap.Size = new System.Drawing.Size(230, 23);
            comboBoxSourceMap.TabIndex = 1;
            comboBoxSourceMap.SelectedIndexChanged += OnSourceMapChanged;
            //
            // buttonSourceExternal
            //
            buttonSourceExternal.Location = new System.Drawing.Point(282, 5);
            buttonSourceExternal.Name = "buttonSourceExternal";
            buttonSourceExternal.Size = new System.Drawing.Size(94, 25);
            buttonSourceExternal.TabIndex = 2;
            buttonSourceExternal.Text = "External...";
            buttonSourceExternal.UseVisualStyleBackColor = true;
            buttonSourceExternal.Click += OnClickSourceExternal;
            //
            // checkBoxSourceStatics
            //
            checkBoxSourceStatics.AutoSize = true;
            checkBoxSourceStatics.Checked = true;
            checkBoxSourceStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxSourceStatics.Location = new System.Drawing.Point(8, 37);
            checkBoxSourceStatics.Name = "checkBoxSourceStatics";
            checkBoxSourceStatics.Size = new System.Drawing.Size(130, 19);
            checkBoxSourceStatics.TabIndex = 3;
            checkBoxSourceStatics.Text = "Show statics";
            checkBoxSourceStatics.UseVisualStyleBackColor = true;
            checkBoxSourceStatics.CheckedChanged += OnOverviewStaticsChanged;
            //
            // tabPageClip
            //
            tabPageClip.Controls.Add(listViewClipIds);
            tabPageClip.Controls.Add(panelClipBottom);
            tabPageClip.Controls.Add(panelClipButtons);
            tabPageClip.Controls.Add(pictureBoxClip);
            tabPageClip.Controls.Add(labelClipInfo);
            tabPageClip.Location = new System.Drawing.Point(4, 24);
            tabPageClip.Name = "tabPageClip";
            tabPageClip.Size = new System.Drawing.Size(382, 725);
            tabPageClip.TabIndex = 1;
            tabPageClip.Text = "2. Clip";
            tabPageClip.UseVisualStyleBackColor = true;
            //
            // listViewClipIds
            //
            listViewClipIds.CheckBoxes = true;
            listViewClipIds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderId, columnHeaderName, columnHeaderCount });
            listViewClipIds.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewClipIds.FullRowSelect = true;
            listViewClipIds.HideSelection = false;
            listViewClipIds.Location = new System.Drawing.Point(0, 236);
            listViewClipIds.Name = "listViewClipIds";
            listViewClipIds.Size = new System.Drawing.Size(382, 389);
            listViewClipIds.TabIndex = 3;
            listViewClipIds.UseCompatibleStateImageBehavior = false;
            listViewClipIds.View = System.Windows.Forms.View.Details;
            listViewClipIds.ItemChecked += OnClipIdChecked;
            //
            // columnHeaderId
            //
            columnHeaderId.Text = "ID";
            columnHeaderId.Width = 70;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 190;
            //
            // columnHeaderCount
            //
            columnHeaderCount.Text = "Count";
            columnHeaderCount.Width = 70;
            //
            // panelClipBottom
            //
            panelClipBottom.Controls.Add(labelZRange);
            panelClipBottom.Controls.Add(numericMinZ);
            panelClipBottom.Controls.Add(labelZTo);
            panelClipBottom.Controls.Add(numericMaxZ);
            panelClipBottom.Controls.Add(buttonCheckAll);
            panelClipBottom.Controls.Add(buttonCheckNone);
            panelClipBottom.Controls.Add(buttonRemoveUnchecked);
            panelClipBottom.Controls.Add(buttonAddStatic);
            panelClipBottom.Controls.Add(buttonClipToLibrary);
            panelClipBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelClipBottom.Location = new System.Drawing.Point(0, 625);
            panelClipBottom.Name = "panelClipBottom";
            panelClipBottom.Size = new System.Drawing.Size(382, 100);
            panelClipBottom.TabIndex = 4;
            //
            // labelZRange
            //
            labelZRange.AutoSize = true;
            labelZRange.Location = new System.Drawing.Point(6, 9);
            labelZRange.Name = "labelZRange";
            labelZRange.Size = new System.Drawing.Size(90, 15);
            labelZRange.TabIndex = 0;
            labelZRange.Text = "Statics z from:";
            //
            // numericMinZ
            //
            numericMinZ.Location = new System.Drawing.Point(100, 6);
            numericMinZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numericMinZ.Minimum = new decimal(new int[] { 128, 0, 0, int.MinValue });
            numericMinZ.Name = "numericMinZ";
            numericMinZ.Size = new System.Drawing.Size(60, 23);
            numericMinZ.TabIndex = 1;
            numericMinZ.Value = new decimal(new int[] { 128, 0, 0, int.MinValue });
            numericMinZ.ValueChanged += OnClipFilterChanged;
            //
            // labelZTo
            //
            labelZTo.AutoSize = true;
            labelZTo.Location = new System.Drawing.Point(166, 9);
            labelZTo.Name = "labelZTo";
            labelZTo.Size = new System.Drawing.Size(21, 15);
            labelZTo.TabIndex = 2;
            labelZTo.Text = "to:";
            //
            // numericMaxZ
            //
            numericMaxZ.Location = new System.Drawing.Point(192, 6);
            numericMaxZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numericMaxZ.Minimum = new decimal(new int[] { 128, 0, 0, int.MinValue });
            numericMaxZ.Name = "numericMaxZ";
            numericMaxZ.Size = new System.Drawing.Size(60, 23);
            numericMaxZ.TabIndex = 3;
            numericMaxZ.Value = new decimal(new int[] { 127, 0, 0, 0 });
            numericMaxZ.ValueChanged += OnClipFilterChanged;
            //
            // buttonCheckAll
            //
            buttonCheckAll.Location = new System.Drawing.Point(6, 36);
            buttonCheckAll.Name = "buttonCheckAll";
            buttonCheckAll.Size = new System.Drawing.Size(70, 26);
            buttonCheckAll.TabIndex = 4;
            buttonCheckAll.Text = "All";
            buttonCheckAll.UseVisualStyleBackColor = true;
            buttonCheckAll.Click += OnClickCheckAll;
            //
            // buttonCheckNone
            //
            buttonCheckNone.Location = new System.Drawing.Point(80, 36);
            buttonCheckNone.Name = "buttonCheckNone";
            buttonCheckNone.Size = new System.Drawing.Size(70, 26);
            buttonCheckNone.TabIndex = 5;
            buttonCheckNone.Text = "None";
            buttonCheckNone.UseVisualStyleBackColor = true;
            buttonCheckNone.Click += OnClickCheckNone;
            //
            // buttonRemoveUnchecked
            //
            buttonRemoveUnchecked.Location = new System.Drawing.Point(154, 36);
            buttonRemoveUnchecked.Name = "buttonRemoveUnchecked";
            buttonRemoveUnchecked.Size = new System.Drawing.Size(220, 26);
            buttonRemoveUnchecked.TabIndex = 6;
            buttonRemoveUnchecked.Text = "Delete unchecked from the clip";
            buttonRemoveUnchecked.UseVisualStyleBackColor = true;
            buttonRemoveUnchecked.Click += OnClickRemoveUnchecked;
            //
            // buttonAddStatic
            //
            buttonAddStatic.Location = new System.Drawing.Point(6, 68);
            buttonAddStatic.Name = "buttonAddStatic";
            buttonAddStatic.Size = new System.Drawing.Size(180, 26);
            buttonAddStatic.TabIndex = 7;
            buttonAddStatic.Text = "Add static to the clip...";
            buttonAddStatic.UseVisualStyleBackColor = true;
            buttonAddStatic.Click += OnClickAddStatic;
            //
            // buttonClipToLibrary
            //
            buttonClipToLibrary.Location = new System.Drawing.Point(194, 68);
            buttonClipToLibrary.Name = "buttonClipToLibrary";
            buttonClipToLibrary.Size = new System.Drawing.Size(180, 26);
            buttonClipToLibrary.TabIndex = 8;
            buttonClipToLibrary.Text = "Export statics...";
            buttonClipToLibrary.UseVisualStyleBackColor = true;
            buttonClipToLibrary.Click += OnClickExportClip;
            //
            // panelClipButtons
            //
            panelClipButtons.Controls.Add(buttonRotate);
            panelClipButtons.Controls.Add(buttonMirrorX);
            panelClipButtons.Controls.Add(buttonMirrorY);
            panelClipButtons.Controls.Add(buttonClearClip);
            panelClipButtons.Dock = System.Windows.Forms.DockStyle.Top;
            panelClipButtons.Location = new System.Drawing.Point(0, 202);
            panelClipButtons.Name = "panelClipButtons";
            panelClipButtons.Size = new System.Drawing.Size(382, 34);
            panelClipButtons.TabIndex = 2;
            //
            // buttonRotate
            //
            buttonRotate.Location = new System.Drawing.Point(6, 4);
            buttonRotate.Name = "buttonRotate";
            buttonRotate.Size = new System.Drawing.Size(84, 26);
            buttonRotate.TabIndex = 0;
            buttonRotate.Text = "Rotate 90°";
            buttonRotate.UseVisualStyleBackColor = true;
            buttonRotate.Click += OnClickRotate;
            //
            // buttonMirrorX
            //
            buttonMirrorX.Location = new System.Drawing.Point(94, 4);
            buttonMirrorX.Name = "buttonMirrorX";
            buttonMirrorX.Size = new System.Drawing.Size(84, 26);
            buttonMirrorX.TabIndex = 1;
            buttonMirrorX.Text = "Mirror X";
            buttonMirrorX.UseVisualStyleBackColor = true;
            buttonMirrorX.Click += OnClickMirrorX;
            //
            // buttonMirrorY
            //
            buttonMirrorY.Location = new System.Drawing.Point(182, 4);
            buttonMirrorY.Name = "buttonMirrorY";
            buttonMirrorY.Size = new System.Drawing.Size(84, 26);
            buttonMirrorY.TabIndex = 2;
            buttonMirrorY.Text = "Mirror Y";
            buttonMirrorY.UseVisualStyleBackColor = true;
            buttonMirrorY.Click += OnClickMirrorY;
            //
            // buttonClearClip
            //
            buttonClearClip.Location = new System.Drawing.Point(290, 4);
            buttonClearClip.Name = "buttonClearClip";
            buttonClearClip.Size = new System.Drawing.Size(84, 26);
            buttonClearClip.TabIndex = 3;
            buttonClearClip.Text = "Clear";
            buttonClearClip.UseVisualStyleBackColor = true;
            buttonClearClip.Click += OnClickClearClip;
            //
            // pictureBoxClip
            //
            pictureBoxClip.BackColor = System.Drawing.Color.FromArgb(18, 18, 22);
            pictureBoxClip.Dock = System.Windows.Forms.DockStyle.Top;
            pictureBoxClip.Location = new System.Drawing.Point(0, 52);
            pictureBoxClip.Name = "pictureBoxClip";
            pictureBoxClip.Size = new System.Drawing.Size(382, 150);
            pictureBoxClip.TabIndex = 1;
            pictureBoxClip.TabStop = false;
            //
            // labelClipInfo
            //
            labelClipInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelClipInfo.Location = new System.Drawing.Point(0, 0);
            labelClipInfo.Name = "labelClipInfo";
            labelClipInfo.Padding = new System.Windows.Forms.Padding(4);
            labelClipInfo.Size = new System.Drawing.Size(382, 52);
            labelClipInfo.TabIndex = 0;
            labelClipInfo.Text = "No clip. Mark an area in \"1. Source\" and copy it.";
            //
            // tabPageTarget
            //
            tabPageTarget.Controls.Add(mapOverviewTarget);
            tabPageTarget.Controls.Add(panelTargetBottom);
            tabPageTarget.Controls.Add(panelTargetTop);
            tabPageTarget.Location = new System.Drawing.Point(4, 24);
            tabPageTarget.Name = "tabPageTarget";
            tabPageTarget.Size = new System.Drawing.Size(382, 725);
            tabPageTarget.TabIndex = 2;
            tabPageTarget.Text = "3. Target";
            tabPageTarget.UseVisualStyleBackColor = true;
            //
            // mapOverviewTarget
            //
            mapOverviewTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            mapOverviewTarget.Location = new System.Drawing.Point(0, 62);
            mapOverviewTarget.Name = "mapOverviewTarget";
            mapOverviewTarget.SelectMode = false;
            mapOverviewTarget.Size = new System.Drawing.Size(382, 403);
            mapOverviewTarget.TabIndex = 1;
            mapOverviewTarget.PositionClicked += OnTargetPositionClicked;
            mapOverviewTarget.PositionHover += OnOverviewHover;
            //
            // panelTargetBottom
            //
            panelTargetBottom.Controls.Add(checkBoxPasteLand);
            panelTargetBottom.Controls.Add(checkBoxPasteStatics);
            panelTargetBottom.Controls.Add(checkBoxFollowGround);
            panelTargetBottom.Controls.Add(labelHeightMode);
            panelTargetBottom.Controls.Add(comboBoxHeightMode);
            panelTargetBottom.Controls.Add(labelZOffset);
            panelTargetBottom.Controls.Add(numericZOffset);
            panelTargetBottom.Controls.Add(labelBlend);
            panelTargetBottom.Controls.Add(numericBlend);
            panelTargetBottom.Controls.Add(labelRemove);
            panelTargetBottom.Controls.Add(comboBoxRemove);
            panelTargetBottom.Controls.Add(checkBoxLivePreview);
            panelTargetBottom.Controls.Add(buttonPasteMode);
            panelTargetBottom.Controls.Add(buttonPasteNow);
            panelTargetBottom.Controls.Add(buttonPasteCancel);
            panelTargetBottom.Controls.Add(labelPasteResult);
            panelTargetBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelTargetBottom.Location = new System.Drawing.Point(0, 465);
            panelTargetBottom.Name = "panelTargetBottom";
            panelTargetBottom.Size = new System.Drawing.Size(382, 260);
            panelTargetBottom.TabIndex = 2;
            //
            // checkBoxPasteLand
            //
            checkBoxPasteLand.AutoSize = true;
            checkBoxPasteLand.Checked = true;
            checkBoxPasteLand.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxPasteLand.Location = new System.Drawing.Point(8, 6);
            checkBoxPasteLand.Name = "checkBoxPasteLand";
            checkBoxPasteLand.Size = new System.Drawing.Size(52, 19);
            checkBoxPasteLand.TabIndex = 0;
            checkBoxPasteLand.Text = "Land";
            checkBoxPasteLand.UseVisualStyleBackColor = true;
            checkBoxPasteLand.CheckedChanged += OnPasteOptionChanged;
            //
            // checkBoxPasteStatics
            //
            checkBoxPasteStatics.AutoSize = true;
            checkBoxPasteStatics.Checked = true;
            checkBoxPasteStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxPasteStatics.Location = new System.Drawing.Point(70, 6);
            checkBoxPasteStatics.Name = "checkBoxPasteStatics";
            checkBoxPasteStatics.Size = new System.Drawing.Size(62, 19);
            checkBoxPasteStatics.TabIndex = 1;
            checkBoxPasteStatics.Text = "Statics";
            checkBoxPasteStatics.UseVisualStyleBackColor = true;
            checkBoxPasteStatics.CheckedChanged += OnPasteOptionChanged;
            //
            // checkBoxFollowGround
            //
            checkBoxFollowGround.AutoSize = true;
            checkBoxFollowGround.Checked = true;
            checkBoxFollowGround.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxFollowGround.Location = new System.Drawing.Point(150, 6);
            checkBoxFollowGround.Name = "checkBoxFollowGround";
            checkBoxFollowGround.Size = new System.Drawing.Size(170, 19);
            checkBoxFollowGround.TabIndex = 2;
            checkBoxFollowGround.Text = "Statics follow the ground";
            checkBoxFollowGround.UseVisualStyleBackColor = true;
            checkBoxFollowGround.CheckedChanged += OnPasteOptionChanged;
            //
            // labelHeightMode
            //
            labelHeightMode.AutoSize = true;
            labelHeightMode.Location = new System.Drawing.Point(6, 36);
            labelHeightMode.Name = "labelHeightMode";
            labelHeightMode.Size = new System.Drawing.Size(46, 15);
            labelHeightMode.TabIndex = 3;
            labelHeightMode.Text = "Height:";
            //
            // comboBoxHeightMode
            //
            comboBoxHeightMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxHeightMode.Items.AddRange(new object[] { "Keep z of the source", "Match the border (auto)" });
            comboBoxHeightMode.Location = new System.Drawing.Point(110, 33);
            comboBoxHeightMode.Name = "comboBoxHeightMode";
            comboBoxHeightMode.Size = new System.Drawing.Size(190, 23);
            comboBoxHeightMode.TabIndex = 4;
            comboBoxHeightMode.SelectedIndexChanged += OnPasteOptionChanged;
            //
            // labelZOffset
            //
            labelZOffset.AutoSize = true;
            labelZOffset.Location = new System.Drawing.Point(6, 65);
            labelZOffset.Name = "labelZOffset";
            labelZOffset.Size = new System.Drawing.Size(53, 15);
            labelZOffset.TabIndex = 5;
            labelZOffset.Text = "Z offset:";
            //
            // numericZOffset
            //
            numericZOffset.Location = new System.Drawing.Point(110, 62);
            numericZOffset.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericZOffset.Minimum = new decimal(new int[] { 255, 0, 0, int.MinValue });
            numericZOffset.Name = "numericZOffset";
            numericZOffset.Size = new System.Drawing.Size(60, 23);
            numericZOffset.TabIndex = 6;
            numericZOffset.ValueChanged += OnPasteOptionChanged;
            //
            // labelBlend
            //
            labelBlend.AutoSize = true;
            labelBlend.Location = new System.Drawing.Point(180, 65);
            labelBlend.Name = "labelBlend";
            labelBlend.Size = new System.Drawing.Size(70, 15);
            labelBlend.TabIndex = 7;
            labelBlend.Text = "Edge blend:";
            //
            // numericBlend
            //
            numericBlend.Location = new System.Drawing.Point(256, 62);
            numericBlend.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
            numericBlend.Name = "numericBlend";
            numericBlend.Size = new System.Drawing.Size(50, 23);
            numericBlend.TabIndex = 8;
            numericBlend.Value = new decimal(new int[] { 3, 0, 0, 0 });
            numericBlend.ValueChanged += OnPasteOptionChanged;
            //
            // labelRemove
            //
            labelRemove.AutoSize = true;
            labelRemove.Location = new System.Drawing.Point(6, 94);
            labelRemove.Name = "labelRemove";
            labelRemove.Size = new System.Drawing.Size(100, 15);
            labelRemove.TabIndex = 9;
            labelRemove.Text = "Remove existing:";
            //
            // comboBoxRemove
            //
            comboBoxRemove.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxRemove.Items.AddRange(new object[] { "All statics in the area", "Only trees/plants/rocks", "Nothing (keep)" });
            comboBoxRemove.Location = new System.Drawing.Point(110, 91);
            comboBoxRemove.Name = "comboBoxRemove";
            comboBoxRemove.Size = new System.Drawing.Size(190, 23);
            comboBoxRemove.TabIndex = 10;
            comboBoxRemove.SelectedIndexChanged += OnPasteOptionChanged;
            //
            // checkBoxLivePreview
            //
            checkBoxLivePreview.AutoSize = true;
            checkBoxLivePreview.Checked = true;
            checkBoxLivePreview.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxLivePreview.Location = new System.Drawing.Point(8, 122);
            checkBoxLivePreview.Name = "checkBoxLivePreview";
            checkBoxLivePreview.Size = new System.Drawing.Size(300, 19);
            checkBoxLivePreview.TabIndex = 11;
            checkBoxLivePreview.Text = "Show the real result when the position is fixed";
            checkBoxLivePreview.UseVisualStyleBackColor = true;
            checkBoxLivePreview.CheckedChanged += OnPasteOptionChanged;
            //
            // buttonPasteMode
            //
            buttonPasteMode.Location = new System.Drawing.Point(6, 148);
            buttonPasteMode.Name = "buttonPasteMode";
            buttonPasteMode.Size = new System.Drawing.Size(120, 30);
            buttonPasteMode.TabIndex = 12;
            buttonPasteMode.Text = "Position clip";
            buttonPasteMode.UseVisualStyleBackColor = true;
            buttonPasteMode.Click += OnClickPasteMode;
            //
            // buttonPasteNow
            //
            buttonPasteNow.Enabled = false;
            buttonPasteNow.Location = new System.Drawing.Point(132, 148);
            buttonPasteNow.Name = "buttonPasteNow";
            buttonPasteNow.Size = new System.Drawing.Size(118, 30);
            buttonPasteNow.TabIndex = 13;
            buttonPasteNow.Text = "Paste (Enter)";
            buttonPasteNow.UseVisualStyleBackColor = true;
            buttonPasteNow.Click += OnClickPasteNow;
            //
            // buttonPasteCancel
            //
            buttonPasteCancel.Enabled = false;
            buttonPasteCancel.Location = new System.Drawing.Point(256, 148);
            buttonPasteCancel.Name = "buttonPasteCancel";
            buttonPasteCancel.Size = new System.Drawing.Size(118, 30);
            buttonPasteCancel.TabIndex = 14;
            buttonPasteCancel.Text = "Cancel (Esc)";
            buttonPasteCancel.UseVisualStyleBackColor = true;
            buttonPasteCancel.Click += OnClickPasteCancel;
            //
            // labelPasteResult
            //
            labelPasteResult.Location = new System.Drawing.Point(6, 184);
            labelPasteResult.Name = "labelPasteResult";
            labelPasteResult.Size = new System.Drawing.Size(370, 72);
            labelPasteResult.TabIndex = 15;
            labelPasteResult.Text = "-";
            //
            // panelTargetTop
            //
            panelTargetTop.Controls.Add(labelTargetMap);
            panelTargetTop.Controls.Add(comboBoxTargetMap);
            panelTargetTop.Controls.Add(buttonTargetExternal);
            panelTargetTop.Controls.Add(labelWorkSize);
            panelTargetTop.Controls.Add(numericWorkSize);
            panelTargetTop.Controls.Add(labelWorkHint);
            panelTargetTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelTargetTop.Location = new System.Drawing.Point(0, 0);
            panelTargetTop.Name = "panelTargetTop";
            panelTargetTop.Size = new System.Drawing.Size(382, 62);
            panelTargetTop.TabIndex = 0;
            //
            // labelTargetMap
            //
            labelTargetMap.AutoSize = true;
            labelTargetMap.Location = new System.Drawing.Point(6, 9);
            labelTargetMap.Name = "labelTargetMap";
            labelTargetMap.Size = new System.Drawing.Size(34, 15);
            labelTargetMap.TabIndex = 0;
            labelTargetMap.Text = "Map:";
            //
            // comboBoxTargetMap
            //
            comboBoxTargetMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxTargetMap.Location = new System.Drawing.Point(46, 6);
            comboBoxTargetMap.Name = "comboBoxTargetMap";
            comboBoxTargetMap.Size = new System.Drawing.Size(230, 23);
            comboBoxTargetMap.TabIndex = 1;
            comboBoxTargetMap.SelectedIndexChanged += OnTargetMapChanged;
            //
            // buttonTargetExternal
            //
            buttonTargetExternal.Location = new System.Drawing.Point(282, 5);
            buttonTargetExternal.Name = "buttonTargetExternal";
            buttonTargetExternal.Size = new System.Drawing.Size(94, 25);
            buttonTargetExternal.TabIndex = 2;
            buttonTargetExternal.Text = "External...";
            buttonTargetExternal.UseVisualStyleBackColor = true;
            buttonTargetExternal.Click += OnClickTargetExternal;
            //
            // labelWorkSize
            //
            labelWorkSize.AutoSize = true;
            labelWorkSize.Location = new System.Drawing.Point(6, 38);
            labelWorkSize.Name = "labelWorkSize";
            labelWorkSize.Size = new System.Drawing.Size(84, 15);
            labelWorkSize.TabIndex = 3;
            labelWorkSize.Text = "Working area:";
            //
            // numericWorkSize
            //
            numericWorkSize.Increment = new decimal(new int[] { 32, 0, 0, 0 });
            numericWorkSize.Location = new System.Drawing.Point(96, 35);
            numericWorkSize.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            numericWorkSize.Minimum = new decimal(new int[] { 32, 0, 0, 0 });
            numericWorkSize.Name = "numericWorkSize";
            numericWorkSize.Size = new System.Drawing.Size(60, 23);
            numericWorkSize.TabIndex = 4;
            numericWorkSize.Value = new decimal(new int[] { 160, 0, 0, 0 });
            //
            // labelWorkHint
            //
            labelWorkHint.AutoSize = true;
            labelWorkHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelWorkHint.Location = new System.Drawing.Point(162, 38);
            labelWorkHint.Name = "labelWorkHint";
            labelWorkHint.Size = new System.Drawing.Size(200, 15);
            labelWorkHint.TabIndex = 5;
            labelWorkHint.Text = "click on the map below to load it";
            //
            // isoView
            //
            isoView.BackColor = System.Drawing.Color.FromArgb(18, 18, 22);
            isoView.Dock = System.Windows.Forms.DockStyle.Fill;
            isoView.Location = new System.Drawing.Point(0, 0);
            isoView.Name = "isoView";
            isoView.Size = new System.Drawing.Size(886, 753);
            isoView.TabIndex = 0;
            isoView.CellClicked += OnCellClicked;
            isoView.HoverChanged += OnHoverChanged;
            isoView.ZoomChanged += OnZoomChanged;
            isoView.RenderProgress += OnRenderProgress;
            //
            // previewTimer
            //
            previewTimer.Interval = 350;
            previewTimer.Tick += OnPreviewTimer;
            //
            // MapEditorForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1280, 800);
            Controls.Add(splitContainerMain);
            Controls.Add(statusStrip);
            Controls.Add(toolStripMain);
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(900, 600);
            Name = "MapEditorForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Map Editor - copy areas";
            Activated += OnActivated;
            FormClosing += OnFormClosing;
            Shown += OnShown;
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            tabControlSide.ResumeLayout(false);
            tabPageSource.ResumeLayout(false);
            panelSourceBottom.ResumeLayout(false);
            panelSourceBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericSourceX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericSourceY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericSourceW).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericSourceH).EndInit();
            panelSourceTop.ResumeLayout(false);
            panelSourceTop.PerformLayout();
            tabPageClip.ResumeLayout(false);
            panelClipBottom.ResumeLayout(false);
            panelClipBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericMinZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericMaxZ).EndInit();
            panelClipButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxClip).EndInit();
            tabPageTarget.ResumeLayout(false);
            panelTargetBottom.ResumeLayout(false);
            panelTargetBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericZOffset).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericBlend).EndInit();
            panelTargetTop.ResumeLayout(false);
            panelTargetTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericWorkSize).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripButton toolStripButtonHelp;
        private System.Windows.Forms.ToolStripButton toolStripButtonUndoLast;
        private System.Windows.Forms.ToolStripButton toolStripButtonRedo;
        private System.Windows.Forms.ToolStripButton toolStripButtonUndoAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonBackups;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomIn;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomOut;
        private System.Windows.Forms.ToolStripButton toolStripButtonFit;
        private System.Windows.Forms.ToolStripLabel toolStripLabelZoom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButtonGrid;
        private System.Windows.Forms.ToolStripButton toolStripButtonStatics;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpenViewer;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelInfo;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelCell;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelChanges;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.TabControl tabControlSide;
        private System.Windows.Forms.TabPage tabPageSource;
        private MapOverviewControl mapOverviewSource;
        private System.Windows.Forms.Panel panelSourceBottom;
        private System.Windows.Forms.Label labelArea;
        private System.Windows.Forms.Label labelSourceX;
        private System.Windows.Forms.NumericUpDown numericSourceX;
        private System.Windows.Forms.Label labelSourceY;
        private System.Windows.Forms.NumericUpDown numericSourceY;
        private System.Windows.Forms.Label labelSourceW;
        private System.Windows.Forms.NumericUpDown numericSourceW;
        private System.Windows.Forms.Label labelSourceH;
        private System.Windows.Forms.NumericUpDown numericSourceH;
        private System.Windows.Forms.CheckBox checkBoxCopyLand;
        private System.Windows.Forms.CheckBox checkBoxCopyStatics;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.Button buttonUseMapMark;
        private System.Windows.Forms.Label labelSourceHint;
        private System.Windows.Forms.Panel panelSourceTop;
        private System.Windows.Forms.Label labelSourceMap;
        private System.Windows.Forms.ComboBox comboBoxSourceMap;
        private System.Windows.Forms.Button buttonSourceExternal;
        private System.Windows.Forms.CheckBox checkBoxSourceStatics;
        private System.Windows.Forms.TabPage tabPageClip;
        private System.Windows.Forms.ListView listViewClipIds;
        private System.Windows.Forms.ColumnHeader columnHeaderId;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderCount;
        private System.Windows.Forms.Panel panelClipBottom;
        private System.Windows.Forms.Label labelZRange;
        private System.Windows.Forms.NumericUpDown numericMinZ;
        private System.Windows.Forms.Label labelZTo;
        private System.Windows.Forms.NumericUpDown numericMaxZ;
        private System.Windows.Forms.Button buttonCheckAll;
        private System.Windows.Forms.Button buttonCheckNone;
        private System.Windows.Forms.Button buttonRemoveUnchecked;
        private System.Windows.Forms.Button buttonAddStatic;
        private System.Windows.Forms.Button buttonClipToLibrary;
        private System.Windows.Forms.Panel panelClipButtons;
        private System.Windows.Forms.Button buttonRotate;
        private System.Windows.Forms.Button buttonMirrorX;
        private System.Windows.Forms.Button buttonMirrorY;
        private System.Windows.Forms.Button buttonClearClip;
        private System.Windows.Forms.PictureBox pictureBoxClip;
        private System.Windows.Forms.Label labelClipInfo;
        private System.Windows.Forms.TabPage tabPageTarget;
        private MapOverviewControl mapOverviewTarget;
        private System.Windows.Forms.Panel panelTargetBottom;
        private System.Windows.Forms.CheckBox checkBoxPasteLand;
        private System.Windows.Forms.CheckBox checkBoxPasteStatics;
        private System.Windows.Forms.CheckBox checkBoxFollowGround;
        private System.Windows.Forms.Label labelHeightMode;
        private System.Windows.Forms.ComboBox comboBoxHeightMode;
        private System.Windows.Forms.Label labelZOffset;
        private System.Windows.Forms.NumericUpDown numericZOffset;
        private System.Windows.Forms.Label labelBlend;
        private System.Windows.Forms.NumericUpDown numericBlend;
        private System.Windows.Forms.Label labelRemove;
        private System.Windows.Forms.ComboBox comboBoxRemove;
        private System.Windows.Forms.CheckBox checkBoxLivePreview;
        private System.Windows.Forms.Button buttonPasteMode;
        private System.Windows.Forms.Button buttonPasteNow;
        private System.Windows.Forms.Button buttonPasteCancel;
        private System.Windows.Forms.Label labelPasteResult;
        private System.Windows.Forms.Panel panelTargetTop;
        private System.Windows.Forms.Label labelTargetMap;
        private System.Windows.Forms.ComboBox comboBoxTargetMap;
        private System.Windows.Forms.Button buttonTargetExternal;
        private System.Windows.Forms.Label labelWorkSize;
        private System.Windows.Forms.NumericUpDown numericWorkSize;
        private System.Windows.Forms.Label labelWorkHint;
        private IsoViewControl isoView;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Timer previewTimer;
    }
}
