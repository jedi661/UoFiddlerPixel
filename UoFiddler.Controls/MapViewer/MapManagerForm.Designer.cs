namespace UoFiddler.Controls.MapViewer
{
    partial class MapManagerForm
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
            splitContainer = new System.Windows.Forms.SplitContainer();
            listViewMaps = new System.Windows.Forms.ListView();
            columnVisible = new System.Windows.Forms.ColumnHeader();
            columnId = new System.Windows.Forms.ColumnHeader();
            columnName = new System.Windows.Forms.ColumnHeader();
            columnFile = new System.Windows.Forms.ColumnHeader();
            columnSize = new System.Windows.Forms.ColumnHeader();
            columnSource = new System.Windows.Forms.ColumnHeader();
            columnStatus = new System.Windows.Forms.ColumnHeader();
            labelHint = new System.Windows.Forms.Label();
            tabControl = new System.Windows.Forms.TabControl();
            tabPageDetails = new System.Windows.Forms.TabPage();
            tableLayoutDetails = new System.Windows.Forms.TableLayoutPanel();
            labelId = new System.Windows.Forms.Label();
            numericId = new System.Windows.Forms.NumericUpDown();
            labelName = new System.Windows.Forms.Label();
            textBoxName = new System.Windows.Forms.TextBox();
            labelFileIndex = new System.Windows.Forms.Label();
            numericFileIndex = new System.Windows.Forms.NumericUpDown();
            labelWidth = new System.Windows.Forms.Label();
            numericWidth = new System.Windows.Forms.NumericUpDown();
            labelHeight = new System.Windows.Forms.Label();
            numericHeight = new System.Windows.Forms.NumericUpDown();
            labelSource = new System.Windows.Forms.Label();
            panelSource = new System.Windows.Forms.FlowLayoutPanel();
            radioClient = new System.Windows.Forms.RadioButton();
            radioCustom = new System.Windows.Forms.RadioButton();
            labelPath = new System.Windows.Forms.Label();
            panelPath = new System.Windows.Forms.TableLayoutPanel();
            textBoxPath = new System.Windows.Forms.TextBox();
            buttonBrowse = new System.Windows.Forms.Button();
            checkBoxVisible = new System.Windows.Forms.CheckBox();
            buttonGuessSize = new System.Windows.Forms.Button();
            labelInfo = new System.Windows.Forms.Label();
            panelDetailButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonApply = new System.Windows.Forms.Button();
            buttonNew = new System.Windows.Forms.Button();
            buttonRemove = new System.Windows.Forms.Button();
            tabPageDetect = new System.Windows.Forms.TabPage();
            listViewDetected = new System.Windows.Forms.ListView();
            columnDetectedFile = new System.Windows.Forms.ColumnHeader();
            columnDetectedIndex = new System.Windows.Forms.ColumnHeader();
            columnDetectedSize = new System.Windows.Forms.ColumnHeader();
            columnDetectedStatics = new System.Windows.Forms.ColumnHeader();
            columnDetectedKnown = new System.Windows.Forms.ColumnHeader();
            panelDetectTop = new System.Windows.Forms.TableLayoutPanel();
            textBoxDetectPath = new System.Windows.Forms.TextBox();
            buttonDetectBrowse = new System.Windows.Forms.Button();
            buttonDetectClient = new System.Windows.Forms.Button();
            panelDetectButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonAddDetected = new System.Windows.Forms.Button();
            labelDetectHint = new System.Windows.Forms.Label();
            panelBottom = new System.Windows.Forms.FlowLayoutPanel();
            buttonClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageDetails.SuspendLayout();
            tableLayoutDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericId).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericFileIndex).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericHeight).BeginInit();
            panelSource.SuspendLayout();
            panelPath.SuspendLayout();
            panelDetailButtons.SuspendLayout();
            tabPageDetect.SuspendLayout();
            panelDetectTop.SuspendLayout();
            panelDetectButtons.SuspendLayout();
            panelBottom.SuspendLayout();
            SuspendLayout();
            //
            // splitContainer
            //
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Location = new System.Drawing.Point(0, 0);
            splitContainer.Name = "splitContainer";
            //
            // splitContainer.Panel1
            //
            splitContainer.Panel1.Controls.Add(listViewMaps);
            splitContainer.Panel1.Controls.Add(labelHint);
            splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(6, 6, 0, 6);
            //
            // splitContainer.Panel2
            //
            splitContainer.Panel2.Controls.Add(tabControl);
            splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(0, 6, 6, 6);
            splitContainer.Size = new System.Drawing.Size(944, 471);
            splitContainer.SplitterDistance = 520;
            splitContainer.TabIndex = 0;
            //
            // listViewMaps
            //
            listViewMaps.CheckBoxes = true;
            listViewMaps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnVisible, columnId, columnName, columnFile, columnSize, columnSource, columnStatus });
            listViewMaps.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewMaps.FullRowSelect = true;
            listViewMaps.GridLines = true;
            listViewMaps.HideSelection = false;
            listViewMaps.Location = new System.Drawing.Point(6, 6);
            listViewMaps.MultiSelect = false;
            listViewMaps.Name = "listViewMaps";
            listViewMaps.Size = new System.Drawing.Size(514, 429);
            listViewMaps.TabIndex = 0;
            listViewMaps.UseCompatibleStateImageBehavior = false;
            listViewMaps.View = System.Windows.Forms.View.Details;
            listViewMaps.ItemChecked += OnMapItemChecked;
            listViewMaps.SelectedIndexChanged += OnMapSelectionChanged;
            //
            // columnVisible
            //
            columnVisible.Text = "Show";
            columnVisible.Width = 48;
            //
            // columnId
            //
            columnId.Text = "ID";
            columnId.Width = 40;
            //
            // columnName
            //
            columnName.Text = "Name";
            columnName.Width = 130;
            //
            // columnFile
            //
            columnFile.Text = "File";
            columnFile.Width = 60;
            //
            // columnSize
            //
            columnSize.Text = "Size";
            columnSize.Width = 80;
            //
            // columnSource
            //
            columnSource.Text = "Source";
            columnSource.Width = 90;
            //
            // columnStatus
            //
            columnStatus.Text = "Status";
            columnStatus.Width = 60;
            //
            // labelHint
            //
            labelHint.Dock = System.Windows.Forms.DockStyle.Bottom;
            labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelHint.Location = new System.Drawing.Point(6, 435);
            labelHint.Name = "labelHint";
            labelHint.Size = new System.Drawing.Size(514, 30);
            labelHint.TabIndex = 1;
            labelHint.Text = "Checked maps are shown in the map list. Built-in maps can only be renamed or hidden.";
            labelHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // tabControl
            //
            tabControl.Controls.Add(tabPageDetails);
            tabControl.Controls.Add(tabPageDetect);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Location = new System.Drawing.Point(0, 6);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(414, 459);
            tabControl.TabIndex = 0;
            //
            // tabPageDetails
            //
            tabPageDetails.Controls.Add(tableLayoutDetails);
            tabPageDetails.Location = new System.Drawing.Point(4, 24);
            tabPageDetails.Name = "tabPageDetails";
            tabPageDetails.Padding = new System.Windows.Forms.Padding(8);
            tabPageDetails.Size = new System.Drawing.Size(406, 431);
            tabPageDetails.TabIndex = 0;
            tabPageDetails.Text = "Map";
            tabPageDetails.UseVisualStyleBackColor = true;
            //
            // tableLayoutDetails
            //
            tableLayoutDetails.ColumnCount = 2;
            tableLayoutDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            tableLayoutDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutDetails.Controls.Add(labelId, 0, 0);
            tableLayoutDetails.Controls.Add(numericId, 1, 0);
            tableLayoutDetails.Controls.Add(labelName, 0, 1);
            tableLayoutDetails.Controls.Add(textBoxName, 1, 1);
            tableLayoutDetails.Controls.Add(labelFileIndex, 0, 2);
            tableLayoutDetails.Controls.Add(numericFileIndex, 1, 2);
            tableLayoutDetails.Controls.Add(labelWidth, 0, 3);
            tableLayoutDetails.Controls.Add(numericWidth, 1, 3);
            tableLayoutDetails.Controls.Add(labelHeight, 0, 4);
            tableLayoutDetails.Controls.Add(numericHeight, 1, 4);
            tableLayoutDetails.Controls.Add(buttonGuessSize, 1, 5);
            tableLayoutDetails.Controls.Add(labelSource, 0, 6);
            tableLayoutDetails.Controls.Add(panelSource, 1, 6);
            tableLayoutDetails.Controls.Add(labelPath, 0, 7);
            tableLayoutDetails.Controls.Add(panelPath, 1, 7);
            tableLayoutDetails.Controls.Add(checkBoxVisible, 1, 8);
            tableLayoutDetails.Controls.Add(labelInfo, 0, 9);
            tableLayoutDetails.Controls.Add(panelDetailButtons, 0, 10);
            tableLayoutDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutDetails.Location = new System.Drawing.Point(8, 8);
            tableLayoutDetails.Name = "tableLayoutDetails";
            tableLayoutDetails.RowCount = 11;
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDetails.Size = new System.Drawing.Size(390, 415);
            tableLayoutDetails.TabIndex = 0;
            //
            // labelId
            //
            labelId.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelId.AutoSize = true;
            labelId.Name = "labelId";
            labelId.Text = "Map ID:";
            //
            // numericId
            //
            numericId.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericId.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericId.Name = "numericId";
            numericId.Size = new System.Drawing.Size(80, 23);
            numericId.TabIndex = 1;
            //
            // labelName
            //
            labelName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelName.AutoSize = true;
            labelName.Name = "labelName";
            labelName.Text = "Name:";
            //
            // textBoxName
            //
            textBoxName.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxName.Name = "textBoxName";
            textBoxName.TabIndex = 2;
            //
            // labelFileIndex
            //
            labelFileIndex.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelFileIndex.AutoSize = true;
            labelFileIndex.Name = "labelFileIndex";
            labelFileIndex.Text = "File index:";
            //
            // numericFileIndex
            //
            numericFileIndex.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericFileIndex.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericFileIndex.Name = "numericFileIndex";
            numericFileIndex.Size = new System.Drawing.Size(80, 23);
            numericFileIndex.TabIndex = 3;
            //
            // labelWidth
            //
            labelWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelWidth.AutoSize = true;
            labelWidth.Name = "labelWidth";
            labelWidth.Text = "Width:";
            //
            // numericWidth
            //
            numericWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericWidth.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numericWidth.Maximum = new decimal(new int[] { 32768, 0, 0, 0 });
            numericWidth.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numericWidth.Name = "numericWidth";
            numericWidth.Size = new System.Drawing.Size(80, 23);
            numericWidth.TabIndex = 4;
            numericWidth.Value = new decimal(new int[] { 6144, 0, 0, 0 });
            //
            // labelHeight
            //
            labelHeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelHeight.AutoSize = true;
            labelHeight.Name = "labelHeight";
            labelHeight.Text = "Height:";
            //
            // numericHeight
            //
            numericHeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numericHeight.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numericHeight.Maximum = new decimal(new int[] { 32768, 0, 0, 0 });
            numericHeight.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numericHeight.Name = "numericHeight";
            numericHeight.Size = new System.Drawing.Size(80, 23);
            numericHeight.TabIndex = 5;
            numericHeight.Value = new decimal(new int[] { 4096, 0, 0, 0 });
            //
            // buttonGuessSize
            //
            buttonGuessSize.AutoSize = true;
            buttonGuessSize.Name = "buttonGuessSize";
            buttonGuessSize.TabIndex = 6;
            buttonGuessSize.Text = "Size from file";
            buttonGuessSize.UseVisualStyleBackColor = true;
            buttonGuessSize.Click += OnClickGuessSize;
            //
            // labelSource
            //
            labelSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelSource.AutoSize = true;
            labelSource.Name = "labelSource";
            labelSource.Text = "Source:";
            //
            // panelSource
            //
            panelSource.AutoSize = true;
            panelSource.Controls.Add(radioClient);
            panelSource.Controls.Add(radioCustom);
            panelSource.Name = "panelSource";
            panelSource.TabIndex = 7;
            //
            // radioClient
            //
            radioClient.AutoSize = true;
            radioClient.Checked = true;
            radioClient.Name = "radioClient";
            radioClient.TabStop = true;
            radioClient.Text = "Client directory";
            radioClient.UseVisualStyleBackColor = true;
            radioClient.CheckedChanged += OnSourceChanged;
            //
            // radioCustom
            //
            radioCustom.AutoSize = true;
            radioCustom.Name = "radioCustom";
            radioCustom.Text = "Other directory";
            radioCustom.UseVisualStyleBackColor = true;
            radioCustom.CheckedChanged += OnSourceChanged;
            //
            // labelPath
            //
            labelPath.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelPath.AutoSize = true;
            labelPath.Name = "labelPath";
            labelPath.Text = "Directory:";
            //
            // panelPath
            //
            panelPath.ColumnCount = 2;
            panelPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            panelPath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelPath.Controls.Add(textBoxPath, 0, 0);
            panelPath.Controls.Add(buttonBrowse, 1, 0);
            panelPath.Dock = System.Windows.Forms.DockStyle.Fill;
            panelPath.Name = "panelPath";
            panelPath.RowCount = 1;
            panelPath.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            panelPath.Size = new System.Drawing.Size(294, 32);
            panelPath.TabIndex = 8;
            //
            // textBoxPath
            //
            textBoxPath.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxPath.Enabled = false;
            textBoxPath.Name = "textBoxPath";
            textBoxPath.TabIndex = 0;
            //
            // buttonBrowse
            //
            buttonBrowse.AutoSize = true;
            buttonBrowse.Enabled = false;
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.TabIndex = 1;
            buttonBrowse.Text = "...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += OnClickBrowse;
            //
            // checkBoxVisible
            //
            checkBoxVisible.AutoSize = true;
            checkBoxVisible.Checked = true;
            checkBoxVisible.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxVisible.Name = "checkBoxVisible";
            checkBoxVisible.TabIndex = 9;
            checkBoxVisible.Text = "Show in map list";
            checkBoxVisible.UseVisualStyleBackColor = true;
            //
            // labelInfo
            //
            tableLayoutDetails.SetColumnSpan(labelInfo, 2);
            labelInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            labelInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            labelInfo.Name = "labelInfo";
            labelInfo.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            labelInfo.TabIndex = 10;
            //
            // panelDetailButtons
            //
            panelDetailButtons.AutoSize = true;
            tableLayoutDetails.SetColumnSpan(panelDetailButtons, 2);
            panelDetailButtons.Controls.Add(buttonApply);
            panelDetailButtons.Controls.Add(buttonNew);
            panelDetailButtons.Controls.Add(buttonRemove);
            panelDetailButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            panelDetailButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            panelDetailButtons.Name = "panelDetailButtons";
            panelDetailButtons.TabIndex = 11;
            //
            // buttonApply
            //
            buttonApply.AutoSize = true;
            buttonApply.Name = "buttonApply";
            buttonApply.TabIndex = 0;
            buttonApply.Text = "Apply";
            buttonApply.UseVisualStyleBackColor = true;
            buttonApply.Click += OnClickApply;
            //
            // buttonNew
            //
            buttonNew.AutoSize = true;
            buttonNew.Name = "buttonNew";
            buttonNew.TabIndex = 1;
            buttonNew.Text = "New map";
            buttonNew.UseVisualStyleBackColor = true;
            buttonNew.Click += OnClickNew;
            //
            // buttonRemove
            //
            buttonRemove.AutoSize = true;
            buttonRemove.Name = "buttonRemove";
            buttonRemove.TabIndex = 2;
            buttonRemove.Text = "Remove";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += OnClickRemove;
            //
            // tabPageDetect
            //
            tabPageDetect.Controls.Add(listViewDetected);
            tabPageDetect.Controls.Add(labelDetectHint);
            tabPageDetect.Controls.Add(panelDetectTop);
            tabPageDetect.Controls.Add(panelDetectButtons);
            tabPageDetect.Location = new System.Drawing.Point(4, 24);
            tabPageDetect.Name = "tabPageDetect";
            tabPageDetect.Padding = new System.Windows.Forms.Padding(8);
            tabPageDetect.Size = new System.Drawing.Size(406, 431);
            tabPageDetect.TabIndex = 1;
            tabPageDetect.Text = "Search Directory";
            tabPageDetect.UseVisualStyleBackColor = true;
            //
            // panelDetectTop
            //
            panelDetectTop.AutoSize = true;
            panelDetectTop.ColumnCount = 3;
            panelDetectTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            panelDetectTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelDetectTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelDetectTop.Controls.Add(textBoxDetectPath, 0, 0);
            panelDetectTop.Controls.Add(buttonDetectBrowse, 1, 0);
            panelDetectTop.Controls.Add(buttonDetectClient, 2, 0);
            panelDetectTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelDetectTop.Name = "panelDetectTop";
            panelDetectTop.RowCount = 1;
            panelDetectTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            panelDetectTop.TabIndex = 0;
            //
            // textBoxDetectPath
            //
            textBoxDetectPath.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxDetectPath.Name = "textBoxDetectPath";
            textBoxDetectPath.ReadOnly = true;
            textBoxDetectPath.TabIndex = 0;
            //
            // buttonDetectBrowse
            //
            buttonDetectBrowse.AutoSize = true;
            buttonDetectBrowse.Name = "buttonDetectBrowse";
            buttonDetectBrowse.TabIndex = 1;
            buttonDetectBrowse.Text = "Directory...";
            buttonDetectBrowse.UseVisualStyleBackColor = true;
            buttonDetectBrowse.Click += OnClickDetectBrowse;
            //
            // buttonDetectClient
            //
            buttonDetectClient.AutoSize = true;
            buttonDetectClient.Name = "buttonDetectClient";
            buttonDetectClient.TabIndex = 2;
            buttonDetectClient.Text = "Client";
            buttonDetectClient.UseVisualStyleBackColor = true;
            buttonDetectClient.Click += OnClickDetectClient;
            //
            // labelDetectHint
            //
            labelDetectHint.Dock = System.Windows.Forms.DockStyle.Top;
            labelDetectHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelDetectHint.Name = "labelDetectHint";
            labelDetectHint.Size = new System.Drawing.Size(390, 34);
            labelDetectHint.Text = "Finds mapX.mul / mapXLegacyMUL.uop. The size is calculated from the file size (please check it).";
            //
            // listViewDetected
            //
            listViewDetected.CheckBoxes = true;
            listViewDetected.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnDetectedFile, columnDetectedIndex, columnDetectedSize, columnDetectedStatics, columnDetectedKnown });
            listViewDetected.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewDetected.FullRowSelect = true;
            listViewDetected.GridLines = true;
            listViewDetected.HideSelection = false;
            listViewDetected.Name = "listViewDetected";
            listViewDetected.TabIndex = 1;
            listViewDetected.UseCompatibleStateImageBehavior = false;
            listViewDetected.View = System.Windows.Forms.View.Details;
            //
            // columnDetectedFile
            //
            columnDetectedFile.Text = "File";
            columnDetectedFile.Width = 150;
            //
            // columnDetectedIndex
            //
            columnDetectedIndex.Text = "Index";
            columnDetectedIndex.Width = 45;
            //
            // columnDetectedSize
            //
            columnDetectedSize.Text = "Size";
            columnDetectedSize.Width = 80;
            //
            // columnDetectedStatics
            //
            columnDetectedStatics.Text = "Statics";
            columnDetectedStatics.Width = 50;
            //
            // columnDetectedKnown
            //
            columnDetectedKnown.Text = "In list";
            columnDetectedKnown.Width = 50;
            //
            // panelDetectButtons
            //
            panelDetectButtons.AutoSize = true;
            panelDetectButtons.Controls.Add(buttonAddDetected);
            panelDetectButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelDetectButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            panelDetectButtons.Name = "panelDetectButtons";
            panelDetectButtons.TabIndex = 2;
            //
            // buttonAddDetected
            //
            buttonAddDetected.AutoSize = true;
            buttonAddDetected.Name = "buttonAddDetected";
            buttonAddDetected.TabIndex = 0;
            buttonAddDetected.Text = "Add checked maps";
            buttonAddDetected.UseVisualStyleBackColor = true;
            buttonAddDetected.Click += OnClickAddDetected;
            //
            // panelBottom
            //
            panelBottom.AutoSize = true;
            panelBottom.Controls.Add(buttonClose);
            panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            panelBottom.Name = "panelBottom";
            panelBottom.Padding = new System.Windows.Forms.Padding(6);
            panelBottom.TabIndex = 1;
            //
            // buttonClose
            //
            buttonClose.AutoSize = true;
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonClose.Name = "buttonClose";
            buttonClose.TabIndex = 0;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += OnClickClose;
            //
            // MapManagerForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(944, 512);
            Controls.Add(splitContainer);
            Controls.Add(panelBottom);
            MinimumSize = new System.Drawing.Size(760, 420);
            Name = "MapManagerForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Manage Maps";
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabPageDetails.ResumeLayout(false);
            tableLayoutDetails.ResumeLayout(false);
            tableLayoutDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericId).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericFileIndex).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericHeight).EndInit();
            panelSource.ResumeLayout(false);
            panelSource.PerformLayout();
            panelPath.ResumeLayout(false);
            panelPath.PerformLayout();
            panelDetailButtons.ResumeLayout(false);
            panelDetailButtons.PerformLayout();
            tabPageDetect.ResumeLayout(false);
            tabPageDetect.PerformLayout();
            panelDetectTop.ResumeLayout(false);
            panelDetectTop.PerformLayout();
            panelDetectButtons.ResumeLayout(false);
            panelDetectButtons.PerformLayout();
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListView listViewMaps;
        private System.Windows.Forms.ColumnHeader columnVisible;
        private System.Windows.Forms.ColumnHeader columnId;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnFile;
        private System.Windows.Forms.ColumnHeader columnSize;
        private System.Windows.Forms.ColumnHeader columnSource;
        private System.Windows.Forms.ColumnHeader columnStatus;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageDetails;
        private System.Windows.Forms.TableLayoutPanel tableLayoutDetails;
        private System.Windows.Forms.Label labelId;
        private System.Windows.Forms.NumericUpDown numericId;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelFileIndex;
        private System.Windows.Forms.NumericUpDown numericFileIndex;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.NumericUpDown numericWidth;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.NumericUpDown numericHeight;
        private System.Windows.Forms.Label labelSource;
        private System.Windows.Forms.FlowLayoutPanel panelSource;
        private System.Windows.Forms.RadioButton radioClient;
        private System.Windows.Forms.RadioButton radioCustom;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.TableLayoutPanel panelPath;
        private System.Windows.Forms.TextBox textBoxPath;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.CheckBox checkBoxVisible;
        private System.Windows.Forms.Button buttonGuessSize;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.FlowLayoutPanel panelDetailButtons;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.TabPage tabPageDetect;
        private System.Windows.Forms.ListView listViewDetected;
        private System.Windows.Forms.ColumnHeader columnDetectedFile;
        private System.Windows.Forms.ColumnHeader columnDetectedIndex;
        private System.Windows.Forms.ColumnHeader columnDetectedSize;
        private System.Windows.Forms.ColumnHeader columnDetectedStatics;
        private System.Windows.Forms.ColumnHeader columnDetectedKnown;
        private System.Windows.Forms.TableLayoutPanel panelDetectTop;
        private System.Windows.Forms.TextBox textBoxDetectPath;
        private System.Windows.Forms.Button buttonDetectBrowse;
        private System.Windows.Forms.Button buttonDetectClient;
        private System.Windows.Forms.FlowLayoutPanel panelDetectButtons;
        private System.Windows.Forms.Button buttonAddDetected;
        private System.Windows.Forms.Label labelDetectHint;
        private System.Windows.Forms.FlowLayoutPanel panelBottom;
        private System.Windows.Forms.Button buttonClose;
    }
}
