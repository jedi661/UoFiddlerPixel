namespace UoFiddler.Controls.MapViewer.Build
{
    partial class AreaExportForm
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
                pictureBoxPreview?.Image?.Dispose();
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
            splitContainer = new System.Windows.Forms.SplitContainer();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            labelSummary = new System.Windows.Forms.Label();
            tableLayoutRight = new System.Windows.Forms.TableLayoutPanel();
            groupBoxFilter = new System.Windows.Forms.GroupBox();
            tableLayoutFilter = new System.Windows.Forms.TableLayoutPanel();
            labelArea = new System.Windows.Forms.Label();
            labelZ = new System.Windows.Forms.Label();
            flowPanelZ = new System.Windows.Forms.FlowLayoutPanel();
            numericMinZ = new System.Windows.Forms.NumericUpDown();
            labelTo = new System.Windows.Forms.Label();
            numericMaxZ = new System.Windows.Forms.NumericUpDown();
            labelOrigin = new System.Windows.Forms.Label();
            comboBoxOrigin = new System.Windows.Forms.ComboBox();
            labelBaseZ = new System.Windows.Forms.Label();
            flowPanelBaseZ = new System.Windows.Forms.FlowLayoutPanel();
            comboBoxBaseZ = new System.Windows.Forms.ComboBox();
            numericBaseZ = new System.Windows.Forms.NumericUpDown();
            checkBoxDuplicates = new System.Windows.Forms.CheckBox();
            checkBoxNoDraw = new System.Windows.Forms.CheckBox();
            groupBoxItems = new System.Windows.Forms.GroupBox();
            tableLayoutItems = new System.Windows.Forms.TableLayoutPanel();
            listViewItems = new System.Windows.Forms.ListView();
            columnId = new System.Windows.Forms.ColumnHeader();
            columnName = new System.Windows.Forms.ColumnHeader();
            columnCount = new System.Windows.Forms.ColumnHeader();
            columnZ = new System.Windows.Forms.ColumnHeader();
            columnFlags = new System.Windows.Forms.ColumnHeader();
            flowPanelItems = new System.Windows.Forms.FlowLayoutPanel();
            buttonAll = new System.Windows.Forms.Button();
            buttonNone = new System.Windows.Forms.Button();
            buttonNoPlants = new System.Windows.Forms.Button();
            buttonOnlyPlants = new System.Windows.Forms.Button();
            groupBoxBuilding = new System.Windows.Forms.GroupBox();
            tableLayoutBuilding = new System.Windows.Forms.TableLayoutPanel();
            labelName = new System.Windows.Forms.Label();
            textBoxName = new System.Windows.Forms.TextBox();
            labelCategory = new System.Windows.Forms.Label();
            textBoxCategory = new System.Windows.Forms.TextBox();
            labelAuthor = new System.Windows.Forms.Label();
            textBoxAuthor = new System.Windows.Forms.TextBox();
            groupBoxTargets = new System.Windows.Forms.GroupBox();
            flowPanelTargets = new System.Windows.Forms.FlowLayoutPanel();
            comboBoxFormat = new System.Windows.Forms.ComboBox();
            buttonSaveFile = new System.Windows.Forms.Button();
            buttonLibrary = new System.Windows.Forms.Button();
            buttonMulti = new System.Windows.Forms.Button();
            buttonAddon = new System.Windows.Forms.Button();
            buttonDecoration = new System.Windows.Forms.Button();
            buttonMultiScript = new System.Windows.Forms.Button();
            panelBottom = new System.Windows.Forms.FlowLayoutPanel();
            buttonClose = new System.Windows.Forms.Button();
            previewTimer = new System.Windows.Forms.Timer(components);
            toolTip = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)numericMinZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericMaxZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericBaseZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            tableLayoutRight.SuspendLayout();
            groupBoxFilter.SuspendLayout();
            tableLayoutFilter.SuspendLayout();
            flowPanelZ.SuspendLayout();
            flowPanelBaseZ.SuspendLayout();
            groupBoxItems.SuspendLayout();
            tableLayoutItems.SuspendLayout();
            flowPanelItems.SuspendLayout();
            groupBoxBuilding.SuspendLayout();
            tableLayoutBuilding.SuspendLayout();
            groupBoxTargets.SuspendLayout();
            flowPanelTargets.SuspendLayout();
            panelBottom.SuspendLayout();
            SuspendLayout();
            //
            // splitContainer
            //
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Name = "splitContainer";
            splitContainer.Panel1.Controls.Add(pictureBoxPreview);
            splitContainer.Panel1.Controls.Add(labelSummary);
            splitContainer.Panel2.Controls.Add(tableLayoutRight);
            splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(4);
            splitContainer.Size = new System.Drawing.Size(1100, 640);
            splitContainer.SplitterDistance = 560;
            //
            // pictureBoxPreview
            //
            pictureBoxPreview.BackColor = System.Drawing.Color.FromArgb(24, 24, 28);
            pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxPreview.TabStop = false;
            pictureBoxPreview.Resize += OnPreviewResize;
            //
            // labelSummary
            //
            labelSummary.Dock = System.Windows.Forms.DockStyle.Bottom;
            labelSummary.Name = "labelSummary";
            labelSummary.Padding = new System.Windows.Forms.Padding(4);
            labelSummary.Size = new System.Drawing.Size(560, 44);
            labelSummary.Text = "-";
            //
            // tableLayoutRight
            //
            tableLayoutRight.ColumnCount = 1;
            tableLayoutRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutRight.Controls.Add(groupBoxFilter, 0, 0);
            tableLayoutRight.Controls.Add(groupBoxItems, 0, 1);
            tableLayoutRight.Controls.Add(groupBoxBuilding, 0, 2);
            tableLayoutRight.Controls.Add(groupBoxTargets, 0, 3);
            tableLayoutRight.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutRight.Name = "tableLayoutRight";
            tableLayoutRight.RowCount = 4;
            tableLayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            //
            // groupBoxFilter
            //
            groupBoxFilter.AutoSize = true;
            groupBoxFilter.Controls.Add(tableLayoutFilter);
            groupBoxFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxFilter.Name = "groupBoxFilter";
            groupBoxFilter.TabStop = false;
            groupBoxFilter.Text = "Area";
            //
            // tableLayoutFilter
            //
            tableLayoutFilter.AutoSize = true;
            tableLayoutFilter.ColumnCount = 2;
            tableLayoutFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutFilter.Controls.Add(labelArea, 0, 0);
            tableLayoutFilter.SetColumnSpan(labelArea, 2);
            tableLayoutFilter.Controls.Add(labelZ, 0, 1);
            tableLayoutFilter.Controls.Add(flowPanelZ, 1, 1);
            tableLayoutFilter.Controls.Add(labelOrigin, 0, 2);
            tableLayoutFilter.Controls.Add(comboBoxOrigin, 1, 2);
            tableLayoutFilter.Controls.Add(labelBaseZ, 0, 3);
            tableLayoutFilter.Controls.Add(flowPanelBaseZ, 1, 3);
            tableLayoutFilter.Controls.Add(checkBoxDuplicates, 0, 4);
            tableLayoutFilter.SetColumnSpan(checkBoxDuplicates, 2);
            tableLayoutFilter.Controls.Add(checkBoxNoDraw, 0, 5);
            tableLayoutFilter.SetColumnSpan(checkBoxNoDraw, 2);
            tableLayoutFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutFilter.Name = "tableLayoutFilter";
            tableLayoutFilter.RowCount = 6;
            tableLayoutFilter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFilter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFilter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFilter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFilter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFilter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            //
            // labelArea
            //
            labelArea.AutoSize = true;
            labelArea.Name = "labelArea";
            labelArea.Text = "-";
            //
            // labelZ
            //
            labelZ.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelZ.AutoSize = true;
            labelZ.Name = "labelZ";
            labelZ.Text = "Statics z:";
            //
            // flowPanelZ
            //
            flowPanelZ.AutoSize = true;
            flowPanelZ.Controls.Add(numericMinZ);
            flowPanelZ.Controls.Add(labelTo);
            flowPanelZ.Controls.Add(numericMaxZ);
            flowPanelZ.Dock = System.Windows.Forms.DockStyle.Fill;
            flowPanelZ.Name = "flowPanelZ";
            //
            // numericMinZ
            //
            numericMinZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numericMinZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numericMinZ.Name = "numericMinZ";
            numericMinZ.Size = new System.Drawing.Size(60, 23);
            numericMinZ.Value = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numericMinZ.ValueChanged += OnFilterChanged;
            //
            // labelTo
            //
            labelTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelTo.AutoSize = true;
            labelTo.Name = "labelTo";
            labelTo.Text = "to";
            //
            // numericMaxZ
            //
            numericMaxZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numericMaxZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numericMaxZ.Name = "numericMaxZ";
            numericMaxZ.Size = new System.Drawing.Size(60, 23);
            numericMaxZ.Value = new decimal(new int[] { 127, 0, 0, 0 });
            numericMaxZ.ValueChanged += OnFilterChanged;
            //
            // labelOrigin
            //
            labelOrigin.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelOrigin.AutoSize = true;
            labelOrigin.Name = "labelOrigin";
            labelOrigin.Text = "Origin 0,0:";
            //
            // comboBoxOrigin
            //
            comboBoxOrigin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxOrigin.Items.AddRange(new object[] { "Upper left corner", "Center (like house multis)" });
            comboBoxOrigin.Name = "comboBoxOrigin";
            comboBoxOrigin.Size = new System.Drawing.Size(200, 23);
            comboBoxOrigin.SelectedIndexChanged += OnFilterChanged;
            //
            // labelBaseZ
            //
            labelBaseZ.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelBaseZ.AutoSize = true;
            labelBaseZ.Name = "labelBaseZ";
            labelBaseZ.Text = "Z 0 is:";
            //
            // flowPanelBaseZ
            //
            flowPanelBaseZ.AutoSize = true;
            flowPanelBaseZ.Controls.Add(comboBoxBaseZ);
            flowPanelBaseZ.Controls.Add(numericBaseZ);
            flowPanelBaseZ.Dock = System.Windows.Forms.DockStyle.Fill;
            flowPanelBaseZ.Name = "flowPanelBaseZ";
            //
            // comboBoxBaseZ
            //
            comboBoxBaseZ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxBaseZ.Items.AddRange(new object[] { "Lowest ground of the area", "Lowest static", "Map z (absolute)", "Fixed z" });
            comboBoxBaseZ.Name = "comboBoxBaseZ";
            comboBoxBaseZ.Size = new System.Drawing.Size(180, 23);
            comboBoxBaseZ.SelectedIndexChanged += OnFilterChanged;
            //
            // numericBaseZ
            //
            numericBaseZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numericBaseZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numericBaseZ.Name = "numericBaseZ";
            numericBaseZ.Size = new System.Drawing.Size(60, 23);
            numericBaseZ.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numericBaseZ.ValueChanged += OnFilterChanged;
            //
            // checkBoxDuplicates
            //
            checkBoxDuplicates.AutoSize = true;
            checkBoxDuplicates.Name = "checkBoxDuplicates";
            checkBoxDuplicates.Text = "Remove exact duplicates";
            checkBoxDuplicates.UseVisualStyleBackColor = true;
            checkBoxDuplicates.CheckedChanged += OnFilterChanged;
            checkBoxDuplicates.Checked = true;
            checkBoxDuplicates.CheckState = System.Windows.Forms.CheckState.Checked;
            //
            // checkBoxNoDraw
            //
            checkBoxNoDraw.AutoSize = true;
            checkBoxNoDraw.Name = "checkBoxNoDraw";
            checkBoxNoDraw.Text = "Skip nodraw / invisible tiles";
            checkBoxNoDraw.UseVisualStyleBackColor = true;
            checkBoxNoDraw.CheckedChanged += OnFilterChanged;
            checkBoxNoDraw.Checked = true;
            checkBoxNoDraw.CheckState = System.Windows.Forms.CheckState.Checked;
            //
            // groupBoxItems
            //
            groupBoxItems.Controls.Add(tableLayoutItems);
            groupBoxItems.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxItems.Name = "groupBoxItems";
            groupBoxItems.TabStop = false;
            groupBoxItems.Text = "Items (uncheck what should not be taken)";
            //
            // tableLayoutItems
            //
            tableLayoutItems.ColumnCount = 1;
            tableLayoutItems.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutItems.Controls.Add(listViewItems, 0, 0);
            tableLayoutItems.Controls.Add(flowPanelItems, 0, 1);
            tableLayoutItems.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutItems.Name = "tableLayoutItems";
            tableLayoutItems.RowCount = 2;
            tableLayoutItems.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutItems.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            //
            // listViewItems
            //
            listViewItems.CheckBoxes = true;
            listViewItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnId, columnName, columnCount, columnZ, columnFlags });
            listViewItems.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewItems.FullRowSelect = true;
            listViewItems.GridLines = true;
            listViewItems.MinimumSize = new System.Drawing.Size(0, 160);
            listViewItems.Name = "listViewItems";
            listViewItems.UseCompatibleStateImageBehavior = false;
            listViewItems.View = System.Windows.Forms.View.Details;
            listViewItems.ItemChecked += OnItemChecked;
            //
            // columnId
            //
            columnId.Text = "ID";
            columnId.Width = 60;
            //
            // columnName
            //
            columnName.Text = "Name";
            columnName.Width = 170;
            //
            // columnCount
            //
            columnCount.Text = "Count";
            columnCount.Width = 50;
            //
            // columnZ
            //
            columnZ.Text = "Z";
            columnZ.Width = 70;
            //
            // columnFlags
            //
            columnFlags.Text = "Flags";
            columnFlags.Width = 120;
            //
            // flowPanelItems
            //
            flowPanelItems.AutoSize = true;
            flowPanelItems.Controls.Add(buttonAll);
            flowPanelItems.Controls.Add(buttonNone);
            flowPanelItems.Controls.Add(buttonNoPlants);
            flowPanelItems.Controls.Add(buttonOnlyPlants);
            flowPanelItems.Dock = System.Windows.Forms.DockStyle.Fill;
            flowPanelItems.Name = "flowPanelItems";
            //
            // buttonAll
            //
            buttonAll.AutoSize = true;
            buttonAll.Name = "buttonAll";
            buttonAll.Text = "All";
            buttonAll.UseVisualStyleBackColor = true;
            buttonAll.Click += OnClickAll;
            //
            // buttonNone
            //
            buttonNone.AutoSize = true;
            buttonNone.Name = "buttonNone";
            buttonNone.Text = "None";
            buttonNone.UseVisualStyleBackColor = true;
            buttonNone.Click += OnClickNone;
            //
            // buttonNoPlants
            //
            buttonNoPlants.AutoSize = true;
            buttonNoPlants.Name = "buttonNoPlants";
            buttonNoPlants.Text = "Without trees / plants";
            buttonNoPlants.UseVisualStyleBackColor = true;
            buttonNoPlants.Click += OnClickNoPlants;
            //
            // buttonOnlyPlants
            //
            buttonOnlyPlants.AutoSize = true;
            buttonOnlyPlants.Name = "buttonOnlyPlants";
            buttonOnlyPlants.Text = "Only trees / plants";
            buttonOnlyPlants.UseVisualStyleBackColor = true;
            buttonOnlyPlants.Click += OnClickOnlyPlants;
            //
            // groupBoxBuilding
            //
            groupBoxBuilding.AutoSize = true;
            groupBoxBuilding.Controls.Add(tableLayoutBuilding);
            groupBoxBuilding.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxBuilding.Name = "groupBoxBuilding";
            groupBoxBuilding.TabStop = false;
            groupBoxBuilding.Text = "Building";
            //
            // tableLayoutBuilding
            //
            tableLayoutBuilding.AutoSize = true;
            tableLayoutBuilding.ColumnCount = 2;
            tableLayoutBuilding.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutBuilding.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutBuilding.Controls.Add(labelName, 0, 0);
            tableLayoutBuilding.Controls.Add(textBoxName, 1, 0);
            tableLayoutBuilding.Controls.Add(labelCategory, 0, 1);
            tableLayoutBuilding.Controls.Add(textBoxCategory, 1, 1);
            tableLayoutBuilding.Controls.Add(labelAuthor, 0, 2);
            tableLayoutBuilding.Controls.Add(textBoxAuthor, 1, 2);
            tableLayoutBuilding.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutBuilding.Name = "tableLayoutBuilding";
            tableLayoutBuilding.RowCount = 3;
            tableLayoutBuilding.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutBuilding.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutBuilding.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
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
            //
            // labelCategory
            //
            labelCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelCategory.AutoSize = true;
            labelCategory.Name = "labelCategory";
            labelCategory.Text = "Category:";
            //
            // textBoxCategory
            //
            textBoxCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxCategory.Name = "textBoxCategory";
            //
            // labelAuthor
            //
            labelAuthor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelAuthor.AutoSize = true;
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Text = "Author:";
            //
            // textBoxAuthor
            //
            textBoxAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxAuthor.Name = "textBoxAuthor";
            //
            // groupBoxTargets
            //
            groupBoxTargets.AutoSize = true;
            groupBoxTargets.Controls.Add(flowPanelTargets);
            groupBoxTargets.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxTargets.Name = "groupBoxTargets";
            groupBoxTargets.TabStop = false;
            groupBoxTargets.Text = "Take over";
            //
            // flowPanelTargets
            //
            flowPanelTargets.AutoSize = true;
            flowPanelTargets.Controls.Add(comboBoxFormat);
            flowPanelTargets.Controls.Add(buttonSaveFile);
            flowPanelTargets.Controls.Add(buttonLibrary);
            flowPanelTargets.Controls.Add(buttonMulti);
            flowPanelTargets.Controls.Add(buttonAddon);
            flowPanelTargets.Controls.Add(buttonDecoration);
            flowPanelTargets.Controls.Add(buttonMultiScript);
            flowPanelTargets.Dock = System.Windows.Forms.DockStyle.Fill;
            flowPanelTargets.Name = "flowPanelTargets";
            //
            // comboBoxFormat
            //
            comboBoxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxFormat.Name = "comboBoxFormat";
            comboBoxFormat.Size = new System.Drawing.Size(220, 23);
            comboBoxFormat.SelectedIndexChanged += OnFormatChanged;
            //
            // buttonSaveFile
            //
            buttonSaveFile.AutoSize = true;
            buttonSaveFile.Name = "buttonSaveFile";
            buttonSaveFile.Text = "Save as file...";
            buttonSaveFile.UseVisualStyleBackColor = true;
            buttonSaveFile.Click += OnClickSaveFile;
            toolTip.SetToolTip(buttonSaveFile, "Saves the building in the selected format");
            //
            // buttonLibrary
            //
            buttonLibrary.AutoSize = true;
            buttonLibrary.Name = "buttonLibrary";
            buttonLibrary.Text = "To library";
            buttonLibrary.UseVisualStyleBackColor = true;
            buttonLibrary.Click += OnClickLibrary;
            toolTip.SetToolTip(buttonLibrary, "Stores the building in the own building library");
            //
            // buttonMulti
            //
            buttonMulti.AutoSize = true;
            buttonMulti.Name = "buttonMulti";
            buttonMulti.Text = "Into multi.mul...";
            buttonMulti.UseVisualStyleBackColor = true;
            buttonMulti.Click += OnClickMulti;
            toolTip.SetToolTip(buttonMulti, "Stores the building as entry of multi.mul / multi.idx (output directory)");
            //
            // buttonAddon
            //
            buttonAddon.AutoSize = true;
            buttonAddon.Name = "buttonAddon";
            buttonAddon.Text = "ServUO addon script...";
            buttonAddon.UseVisualStyleBackColor = true;
            buttonAddon.Click += OnClickAddon;
            toolTip.SetToolTip(buttonAddon, "C# script: addon + deed (with hues)");
            //
            // buttonDecoration
            //
            buttonDecoration.AutoSize = true;
            buttonDecoration.Name = "buttonDecoration";
            buttonDecoration.Text = "ServUO decoration (.cfg)...";
            buttonDecoration.UseVisualStyleBackColor = true;
            buttonDecoration.Click += OnClickDecoration;
            toolTip.SetToolTip(buttonDecoration, "Decoration file with absolute map positions ([Decorate)");
            //
            // buttonMultiScript
            //
            buttonMultiScript.AutoSize = true;
            buttonMultiScript.Name = "buttonMultiScript";
            buttonMultiScript.Text = "Open in Multi Script";
            buttonMultiScript.UseVisualStyleBackColor = true;
            buttonMultiScript.Click += OnClickMultiScript;
            toolTip.SetToolTip(buttonMultiScript, "Opens the existing Multi Script window with these parts");
            //
            // panelBottom
            //
            panelBottom.AutoSize = true;
            panelBottom.Controls.Add(buttonClose);
            panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            panelBottom.Name = "panelBottom";
            //
            // buttonClose
            //
            buttonClose.AutoSize = true;
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonClose.Name = "buttonClose";
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // previewTimer
            //
            previewTimer.Interval = 250;
            previewTimer.Tick += OnPreviewTimer;
            //
            // AreaExportForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(1100, 680);
            Controls.Add(splitContainer);
            Controls.Add(panelBottom);
            MinimumSize = new System.Drawing.Size(900, 560);
            Name = "AreaExportForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Export area";
            ((System.ComponentModel.ISupportInitialize)numericMinZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericMaxZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericBaseZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            splitContainer.ResumeLayout(false);
            tableLayoutRight.ResumeLayout(false);
            tableLayoutRight.PerformLayout();
            groupBoxFilter.ResumeLayout(false);
            groupBoxFilter.PerformLayout();
            tableLayoutFilter.ResumeLayout(false);
            tableLayoutFilter.PerformLayout();
            flowPanelZ.ResumeLayout(false);
            flowPanelZ.PerformLayout();
            flowPanelBaseZ.ResumeLayout(false);
            flowPanelBaseZ.PerformLayout();
            groupBoxItems.ResumeLayout(false);
            groupBoxItems.PerformLayout();
            tableLayoutItems.ResumeLayout(false);
            tableLayoutItems.PerformLayout();
            flowPanelItems.ResumeLayout(false);
            flowPanelItems.PerformLayout();
            groupBoxBuilding.ResumeLayout(false);
            groupBoxBuilding.PerformLayout();
            tableLayoutBuilding.ResumeLayout(false);
            tableLayoutBuilding.PerformLayout();
            groupBoxTargets.ResumeLayout(false);
            groupBoxTargets.PerformLayout();
            flowPanelTargets.ResumeLayout(false);
            flowPanelTargets.PerformLayout();
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Label labelSummary;
        private System.Windows.Forms.TableLayoutPanel tableLayoutRight;
        private System.Windows.Forms.GroupBox groupBoxFilter;
        private System.Windows.Forms.TableLayoutPanel tableLayoutFilter;
        private System.Windows.Forms.Label labelArea;
        private System.Windows.Forms.Label labelZ;
        private System.Windows.Forms.FlowLayoutPanel flowPanelZ;
        private System.Windows.Forms.NumericUpDown numericMinZ;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.NumericUpDown numericMaxZ;
        private System.Windows.Forms.Label labelOrigin;
        private System.Windows.Forms.ComboBox comboBoxOrigin;
        private System.Windows.Forms.Label labelBaseZ;
        private System.Windows.Forms.FlowLayoutPanel flowPanelBaseZ;
        private System.Windows.Forms.ComboBox comboBoxBaseZ;
        private System.Windows.Forms.NumericUpDown numericBaseZ;
        private System.Windows.Forms.CheckBox checkBoxDuplicates;
        private System.Windows.Forms.CheckBox checkBoxNoDraw;
        private System.Windows.Forms.GroupBox groupBoxItems;
        private System.Windows.Forms.TableLayoutPanel tableLayoutItems;
        private System.Windows.Forms.ListView listViewItems;
        private System.Windows.Forms.ColumnHeader columnId;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnCount;
        private System.Windows.Forms.ColumnHeader columnZ;
        private System.Windows.Forms.ColumnHeader columnFlags;
        private System.Windows.Forms.FlowLayoutPanel flowPanelItems;
        private System.Windows.Forms.Button buttonAll;
        private System.Windows.Forms.Button buttonNone;
        private System.Windows.Forms.Button buttonNoPlants;
        private System.Windows.Forms.Button buttonOnlyPlants;
        private System.Windows.Forms.GroupBox groupBoxBuilding;
        private System.Windows.Forms.TableLayoutPanel tableLayoutBuilding;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelCategory;
        private System.Windows.Forms.TextBox textBoxCategory;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.TextBox textBoxAuthor;
        private System.Windows.Forms.GroupBox groupBoxTargets;
        private System.Windows.Forms.FlowLayoutPanel flowPanelTargets;
        private System.Windows.Forms.ComboBox comboBoxFormat;
        private System.Windows.Forms.Button buttonSaveFile;
        private System.Windows.Forms.Button buttonLibrary;
        private System.Windows.Forms.Button buttonMulti;
        private System.Windows.Forms.Button buttonAddon;
        private System.Windows.Forms.Button buttonDecoration;
        private System.Windows.Forms.Button buttonMultiScript;
        private System.Windows.Forms.FlowLayoutPanel panelBottom;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Timer previewTimer;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
