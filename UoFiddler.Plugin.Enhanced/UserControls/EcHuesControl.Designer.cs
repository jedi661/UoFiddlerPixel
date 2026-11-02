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
    partial class EcHuesControl
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
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelClassic = new System.Windows.Forms.ToolStripLabel();
            toolStripButtonOpenClassic = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveClassic = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveClassicAs = new System.Windows.Forms.ToolStripButton();
            tabControl = new System.Windows.Forms.TabControl();
            tabPageEditor = new System.Windows.Forms.TabPage();
            toolStripEditor = new System.Windows.Forms.ToolStrip();
            toolStripLabelSearch = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonApply = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonImportRamp = new System.Windows.Forms.ToolStripButton();
            toolStripButtonExportRamp = new System.Windows.Forms.ToolStripButton();
            toolStripButtonAddCopy = new System.Windows.Forms.ToolStripButton();
            toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonFromClassic = new System.Windows.Forms.ToolStripButton();
            toolStripButtonToClassic = new System.Windows.Forms.ToolStripButton();
            toolStripButtonGradient = new System.Windows.Forms.ToolStripButton();
            splitContainer = new System.Windows.Forms.SplitContainer();
            listViewHues = new System.Windows.Forms.ListView();
            columnHeaderHue = new System.Windows.Forms.ColumnHeader();
            columnHeaderRamp = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderColors = new System.Windows.Forms.ColumnHeader();
            contextMenuStripList = new System.Windows.Forms.ContextMenuStrip(components);
            importRampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportRampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu1 = new System.Windows.Forms.ToolStripSeparator();
            fromClassicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toClassicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            gradientToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            compareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu2 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelDetails = new System.Windows.Forms.Panel();
            flowLayoutPanelName = new System.Windows.Forms.FlowLayoutPanel();
            labelName = new System.Windows.Forms.Label();
            textBoxName = new System.Windows.Forms.TextBox();
            buttonSetName = new System.Windows.Forms.Button();
            pictureBoxRamp = new System.Windows.Forms.PictureBox();
            labelColors = new System.Windows.Forms.Label();
            panelColors = new System.Windows.Forms.Panel();
            labelInfo = new System.Windows.Forms.Label();
            flowLayoutPanelPreview = new System.Windows.Forms.FlowLayoutPanel();
            labelPreview = new System.Windows.Forms.Label();
            comboBoxPreview = new System.Windows.Forms.ComboBox();
            textBoxPreviewId = new System.Windows.Forms.TextBox();
            checkBoxPartial = new System.Windows.Forms.CheckBox();
            buttonPreview = new System.Windows.Forms.Button();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tabPageCompare = new System.Windows.Forms.TabPage();
            toolStripCompare = new System.Windows.Forms.ToolStrip();
            toolStripButtonCompare = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelFilter = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxFilter = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonShowClassic = new System.Windows.Forms.ToolStripButton();
            toolStripButtonShowEc = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonMarkDifferences = new System.Windows.Forms.ToolStripButton();
            toolStripButtonToEc = new System.Windows.Forms.ToolStripButton();
            toolStripButtonToClassicMarked = new System.Windows.Forms.ToolStripButton();
            listViewCompare = new System.Windows.Forms.ListView();
            columnHeaderCompareHue = new System.Windows.Forms.ColumnHeader();
            columnHeaderCompareClassic = new System.Windows.Forms.ColumnHeader();
            columnHeaderCompareEc = new System.Windows.Forms.ColumnHeader();
            columnHeaderCompareStatus = new System.Windows.Forms.ColumnHeader();
            columnHeaderCompareQuality = new System.Windows.Forms.ColumnHeader();
            columnHeaderCompareClassicName = new System.Windows.Forms.ColumnHeader();
            columnHeaderCompareEcName = new System.Windows.Forms.ColumnHeader();
            contextMenuStripCompare = new System.Windows.Forms.ContextMenuStrip(components);
            toEcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toClassicMarkedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu3 = new System.Windows.Forms.ToolStripSeparator();
            showInEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            markAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            labelCompareInfo = new System.Windows.Forms.Label();
            tabPageConvert = new System.Windows.Forms.TabPage();
            flowLayoutPanelConvert = new System.Windows.Forms.FlowLayoutPanel();
            groupBoxToEc = new System.Windows.Forms.GroupBox();
            flowLayoutPanelToEc = new System.Windows.Forms.FlowLayoutPanel();
            radioButtonToEcLoaded = new System.Windows.Forms.RadioButton();
            radioButtonToEcNew = new System.Windows.Forms.RadioButton();
            flowLayoutPanelStyle = new System.Windows.Forms.FlowLayoutPanel();
            labelStyle = new System.Windows.Forms.Label();
            comboBoxStyle = new System.Windows.Forms.ComboBox();
            labelBmpBlur = new System.Windows.Forms.Label();
            numericUpDownBmpBlur = new System.Windows.Forms.NumericUpDown();
            labelDdsBlur = new System.Windows.Forms.Label();
            numericUpDownDdsBlur = new System.Windows.Forms.NumericUpDown();
            labelScale = new System.Windows.Forms.Label();
            comboBoxScale = new System.Windows.Forms.ComboBox();
            flowLayoutPanelToEcRange = new System.Windows.Forms.FlowLayoutPanel();
            checkBoxToEcRange = new System.Windows.Forms.CheckBox();
            numericUpDownToEcFrom = new System.Windows.Forms.NumericUpDown();
            labelToEcTo = new System.Windows.Forms.Label();
            numericUpDownToEcTo = new System.Windows.Forms.NumericUpDown();
            checkBoxToEcNames = new System.Windows.Forms.CheckBox();
            checkBoxToEcOnlyChanged = new System.Windows.Forms.CheckBox();
            checkBoxToEcSkipEmpty = new System.Windows.Forms.CheckBox();
            checkBoxToEcExact = new System.Windows.Forms.CheckBox();
            buttonToEc = new System.Windows.Forms.Button();
            groupBoxToClassic = new System.Windows.Forms.GroupBox();
            flowLayoutPanelToClassic = new System.Windows.Forms.FlowLayoutPanel();
            radioButtonToClassicLoaded = new System.Windows.Forms.RadioButton();
            radioButtonToClassicNew = new System.Windows.Forms.RadioButton();
            flowLayoutPanelToClassicRange = new System.Windows.Forms.FlowLayoutPanel();
            checkBoxToClassicRange = new System.Windows.Forms.CheckBox();
            numericUpDownToClassicFrom = new System.Windows.Forms.NumericUpDown();
            labelToClassicTo = new System.Windows.Forms.Label();
            numericUpDownToClassicTo = new System.Windows.Forms.NumericUpDown();
            checkBoxToClassicNames = new System.Windows.Forms.CheckBox();
            checkBoxToClassicKeepTable = new System.Windows.Forms.CheckBox();
            buttonToClassic = new System.Windows.Forms.Button();
            groupBoxFiles = new System.Windows.Forms.GroupBox();
            tableLayoutPanelFiles = new System.Windows.Forms.TableLayoutPanel();
            labelOutput = new System.Windows.Forms.Label();
            textBoxOutput = new System.Windows.Forms.TextBox();
            buttonOutput = new System.Windows.Forms.Button();
            checkBoxBackup = new System.Windows.Forms.CheckBox();
            textBoxBackup = new System.Windows.Forms.TextBox();
            buttonBackup = new System.Windows.Forms.Button();
            buttonOpenBackup = new System.Windows.Forms.Button();
            groupBoxRepair = new System.Windows.Forms.GroupBox();
            flowLayoutPanelRepair = new System.Windows.Forms.FlowLayoutPanel();
            buttonAnalyze = new System.Windows.Forms.Button();
            buttonRebuildAtlas = new System.Windows.Forms.Button();
            textBoxLog = new System.Windows.Forms.TextBox();
            tabPageHelp = new System.Windows.Forms.TabPage();
            flowLayoutPanelHelp = new System.Windows.Forms.FlowLayoutPanel();
            radioButtonHelpEnglish = new System.Windows.Forms.RadioButton();
            radioButtonHelpGerman = new System.Windows.Forms.RadioButton();
            richTextBoxHelp = new System.Windows.Forms.RichTextBox();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            toolStrip.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageEditor.SuspendLayout();
            toolStripEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            contextMenuStripList.SuspendLayout();
            panelDetails.SuspendLayout();
            flowLayoutPanelName.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRamp).BeginInit();
            panelColors.SuspendLayout();
            flowLayoutPanelPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            tabPageCompare.SuspendLayout();
            toolStripCompare.SuspendLayout();
            contextMenuStripCompare.SuspendLayout();
            tabPageConvert.SuspendLayout();
            flowLayoutPanelConvert.SuspendLayout();
            groupBoxToEc.SuspendLayout();
            flowLayoutPanelToEc.SuspendLayout();
            flowLayoutPanelStyle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownBmpBlur).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownDdsBlur).BeginInit();
            flowLayoutPanelToEcRange.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownToEcFrom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownToEcTo).BeginInit();
            groupBoxToClassic.SuspendLayout();
            flowLayoutPanelToClassic.SuspendLayout();
            flowLayoutPanelToClassicRange.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownToClassicFrom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownToClassicTo).BeginInit();
            groupBoxFiles.SuspendLayout();
            tableLayoutPanelFiles.SuspendLayout();
            groupBoxRepair.SuspendLayout();
            flowLayoutPanelRepair.SuspendLayout();
            tabPageHelp.SuspendLayout();
            flowLayoutPanelHelp.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonSave, toolStripButtonSaveAs, toolStripSeparator1, toolStripLabelClassic, toolStripButtonOpenClassic, toolStripButtonSaveClassic, toolStripButtonSaveClassicAs });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Enabled = false;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Text = "Save Hues.uop";
            toolStripButtonSave.ToolTipText = "Writes the changed Hues.uop (the old file is kept as .bak)";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripButtonSaveAs
            //
            toolStripButtonSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSaveAs.Name = "toolStripButtonSaveAs";
            toolStripButtonSaveAs.Text = "Save Hues.uop as...";
            toolStripButtonSaveAs.Click += OnClickSaveAs;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripLabelClassic
            //
            toolStripLabelClassic.Name = "toolStripLabelClassic";
            toolStripLabelClassic.Text = "Classic: (not loaded)";
            //
            // toolStripButtonOpenClassic
            //
            toolStripButtonOpenClassic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonOpenClassic.Name = "toolStripButtonOpenClassic";
            toolStripButtonOpenClassic.Text = "Open hues.mul...";
            toolStripButtonOpenClassic.ToolTipText = "Classic hues.mul for comparing and converting (default: the one of the classic client loaded in UOFiddler)";
            toolStripButtonOpenClassic.Click += OnClickOpenClassic;
            //
            // toolStripButtonSaveClassic
            //
            toolStripButtonSaveClassic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSaveClassic.Enabled = false;
            toolStripButtonSaveClassic.Name = "toolStripButtonSaveClassic";
            toolStripButtonSaveClassic.Text = "Save hues.mul";
            toolStripButtonSaveClassic.ToolTipText = "Writes the changed classic hues.mul";
            toolStripButtonSaveClassic.Click += OnClickSaveClassic;
            //
            // toolStripButtonSaveClassicAs
            //
            toolStripButtonSaveClassicAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSaveClassicAs.Name = "toolStripButtonSaveClassicAs";
            toolStripButtonSaveClassicAs.Text = "Save hues.mul as...";
            toolStripButtonSaveClassicAs.Click += OnClickSaveClassicAs;
            //
            // tabControl
            //
            tabControl.Controls.Add(tabPageEditor);
            tabControl.Controls.Add(tabPageCompare);
            tabControl.Controls.Add(tabPageConvert);
            tabControl.Controls.Add(tabPageHelp);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.TabIndex = 1;
            tabControl.SelectedIndexChanged += OnTabChanged;
            //
            // tabPageEditor
            //
            tabPageEditor.Controls.Add(splitContainer);
            tabPageEditor.Controls.Add(toolStripEditor);
            tabPageEditor.Location = new System.Drawing.Point(4, 24);
            tabPageEditor.Name = "tabPageEditor";
            tabPageEditor.Padding = new System.Windows.Forms.Padding(3);
            tabPageEditor.Size = new System.Drawing.Size(500, 300);
            tabPageEditor.TabIndex = 0;
            tabPageEditor.Text = "Editor";
            tabPageEditor.UseVisualStyleBackColor = true;
            //
            // toolStripEditor
            //
            toolStripEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelSearch, toolStripTextBoxSearch, toolStripButtonApply, toolStripSeparator2, toolStripButtonImportRamp, toolStripButtonExportRamp, toolStripButtonAddCopy, toolStripButtonDelete, toolStripSeparator3, toolStripButtonFromClassic, toolStripButtonToClassic, toolStripButtonGradient });
            toolStripEditor.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripEditor.Location = new System.Drawing.Point(0, 0);
            toolStripEditor.Name = "toolStripEditor";
            toolStripEditor.TabIndex = 0;
            //
            // toolStripLabelSearch
            //
            toolStripLabelSearch.Name = "toolStripLabelSearch";
            toolStripLabelSearch.Text = "Search:";
            //
            // toolStripTextBoxSearch
            //
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new System.Drawing.Size(150, 25);
            toolStripTextBoxSearch.ToolTipText = "Number or part of the name – Enter";
            toolStripTextBoxSearch.KeyDown += OnSearchKeyDown;
            //
            // toolStripButtonApply
            //
            toolStripButtonApply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonApply.Name = "toolStripButtonApply";
            toolStripButtonApply.Text = "Apply";
            toolStripButtonApply.Click += OnClickApply;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripButtonImportRamp
            //
            toolStripButtonImportRamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonImportRamp.Name = "toolStripButtonImportRamp";
            toolStripButtonImportRamp.Text = "Import ramp...";
            toolStripButtonImportRamp.ToolTipText = "Replace the color ramp with an image (best 256 x 2)";
            toolStripButtonImportRamp.Click += OnClickImportRamp;
            //
            // toolStripButtonExportRamp
            //
            toolStripButtonExportRamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonExportRamp.Name = "toolStripButtonExportRamp";
            toolStripButtonExportRamp.Text = "Export ramp...";
            toolStripButtonExportRamp.Click += OnClickExportRamp;
            //
            // toolStripButtonAddCopy
            //
            toolStripButtonAddCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAddCopy.Name = "toolStripButtonAddCopy";
            toolStripButtonAddCopy.Text = "Add copy...";
            toolStripButtonAddCopy.ToolTipText = "New hue as copy of the selected one";
            toolStripButtonAddCopy.Click += OnClickAddCopy;
            //
            // toolStripButtonDelete
            //
            toolStripButtonDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonDelete.Name = "toolStripButtonDelete";
            toolStripButtonDelete.Text = "Delete";
            toolStripButtonDelete.Click += OnClickDelete;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            //
            // toolStripButtonFromClassic
            //
            toolStripButtonFromClassic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonFromClassic.Name = "toolStripButtonFromClassic";
            toolStripButtonFromClassic.Text = "Colors from classic";
            toolStripButtonFromClassic.ToolTipText = "Draws the selected hues from the classic hues.mul (bitmap, hues.dds, overview)";
            toolStripButtonFromClassic.Click += OnClickFromClassic;
            //
            // toolStripButtonToClassic
            //
            toolStripButtonToClassic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonToClassic.Name = "toolStripButtonToClassic";
            toolStripButtonToClassic.Text = "Colors to classic";
            toolStripButtonToClassic.ToolTipText = "Writes the colors of the selected hues into the loaded classic hues.mul";
            toolStripButtonToClassic.Click += OnClickToClassic;
            //
            // toolStripButtonGradient
            //
            toolStripButtonGradient.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonGradient.Name = "toolStripButtonGradient";
            toolStripButtonGradient.Text = "Gradient...";
            toolStripButtonGradient.ToolTipText = "New colors for the selected hue: a gradient from a dark to a light color";
            toolStripButtonGradient.Click += OnClickGradient;
            //
            // splitContainer
            //
            splitContainer.Panel1.Controls.Add(listViewHues);
            splitContainer.Panel2.Controls.Add(panelDetails);
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Name = "splitContainer";
            splitContainer.Size = new System.Drawing.Size(900, 450);
            splitContainer.SplitterDistance = 560;
            splitContainer.TabIndex = 1;
            //
            // listViewHues
            //
            listViewHues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderHue, columnHeaderRamp, columnHeaderName, columnHeaderColors });
            listViewHues.ContextMenuStrip = contextMenuStripList;
            listViewHues.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewHues.FullRowSelect = true;
            listViewHues.HideSelection = false;
            listViewHues.Name = "listViewHues";
            listViewHues.OwnerDraw = true;
            listViewHues.TabIndex = 0;
            listViewHues.UseCompatibleStateImageBehavior = false;
            listViewHues.View = System.Windows.Forms.View.Details;
            listViewHues.VirtualMode = true;
            listViewHues.RetrieveVirtualItem += RetrieveItem;
            listViewHues.DrawColumnHeader += OnDrawColumnHeader;
            listViewHues.DrawSubItem += DrawSubItem;
            listViewHues.SelectedIndexChanged += OnListSelectedIndexChanged;
            listViewHues.VirtualItemsSelectionRangeChanged += OnListSelectionRangeChanged;
            //
            // columnHeaderHue
            //
            columnHeaderHue.Text = "Hue";
            columnHeaderHue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderHue.Width = 60;
            //
            // columnHeaderRamp
            //
            columnHeaderRamp.Text = "Ramp";
            columnHeaderRamp.Width = 140;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 220;
            //
            // columnHeaderColors
            //
            columnHeaderColors.Text = "Classic colors";
            columnHeaderColors.Width = 110;
            //
            // contextMenuStripList
            //
            contextMenuStripList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { importRampToolStripMenuItem, exportRampToolStripMenuItem, addCopyToolStripMenuItem, deleteToolStripMenuItem, toolStripSeparatorMenu1, fromClassicToolStripMenuItem, toClassicToolStripMenuItem, gradientToolStripMenuItem, compareToolStripMenuItem, toolStripSeparatorMenu2, saveToolStripMenuItem });
            contextMenuStripList.Name = "contextMenuStripList";
            //
            // importRampToolStripMenuItem
            //
            importRampToolStripMenuItem.Name = "importRampToolStripMenuItem";
            importRampToolStripMenuItem.Text = "Import ramp...";
            importRampToolStripMenuItem.Click += OnClickImportRamp;
            //
            // exportRampToolStripMenuItem
            //
            exportRampToolStripMenuItem.Name = "exportRampToolStripMenuItem";
            exportRampToolStripMenuItem.Text = "Export ramp...";
            exportRampToolStripMenuItem.Click += OnClickExportRamp;
            //
            // addCopyToolStripMenuItem
            //
            addCopyToolStripMenuItem.Name = "addCopyToolStripMenuItem";
            addCopyToolStripMenuItem.Text = "Add copy...";
            addCopyToolStripMenuItem.Click += OnClickAddCopy;
            //
            // deleteToolStripMenuItem
            //
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += OnClickDelete;
            //
            // toolStripSeparatorMenu1
            //
            toolStripSeparatorMenu1.Name = "toolStripSeparatorMenu1";
            //
            // fromClassicToolStripMenuItem
            //
            fromClassicToolStripMenuItem.Name = "fromClassicToolStripMenuItem";
            fromClassicToolStripMenuItem.Text = "Colors from classic hues.mul";
            fromClassicToolStripMenuItem.Click += OnClickFromClassic;
            //
            // toClassicToolStripMenuItem
            //
            toClassicToolStripMenuItem.Name = "toClassicToolStripMenuItem";
            toClassicToolStripMenuItem.Text = "Colors to classic hues.mul";
            toClassicToolStripMenuItem.Click += OnClickToClassic;
            //
            // gradientToolStripMenuItem
            //
            gradientToolStripMenuItem.Name = "gradientToolStripMenuItem";
            gradientToolStripMenuItem.Text = "Gradient from two colors...";
            gradientToolStripMenuItem.Click += OnClickGradient;
            //
            // compareToolStripMenuItem
            //
            compareToolStripMenuItem.Name = "compareToolStripMenuItem";
            compareToolStripMenuItem.Text = "Show in Compare";
            compareToolStripMenuItem.Click += OnClickShowInCompare;
            //
            // toolStripSeparatorMenu2
            //
            toolStripSeparatorMenu2.Name = "toolStripSeparatorMenu2";
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save Hues.uop";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // panelDetails
            //
            panelDetails.Controls.Add(labelInfo);
            panelDetails.Controls.Add(pictureBoxPreview);
            panelDetails.Controls.Add(flowLayoutPanelPreview);
            panelDetails.Controls.Add(panelColors);
            panelDetails.Controls.Add(labelColors);
            panelDetails.Controls.Add(pictureBoxRamp);
            panelDetails.Controls.Add(flowLayoutPanelName);
            panelDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            panelDetails.Name = "panelDetails";
            panelDetails.Padding = new System.Windows.Forms.Padding(6);
            panelDetails.TabIndex = 0;
            //
            // flowLayoutPanelName
            //
            flowLayoutPanelName.Controls.Add(labelName);
            flowLayoutPanelName.Controls.Add(textBoxName);
            flowLayoutPanelName.Controls.Add(buttonSetName);
            flowLayoutPanelName.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelName.Name = "flowLayoutPanelName";
            flowLayoutPanelName.Size = new System.Drawing.Size(320, 34);
            flowLayoutPanelName.TabIndex = 0;
            //
            // labelName
            //
            labelName.AutoSize = true;
            labelName.Margin = new System.Windows.Forms.Padding(3, 8, 0, 0);
            labelName.Name = "labelName";
            labelName.Text = "Name:";
            //
            // textBoxName
            //
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new System.Drawing.Size(220, 23);
            textBoxName.TabIndex = 1;
            //
            // buttonSetName
            //
            buttonSetName.AutoSize = true;
            buttonSetName.Name = "buttonSetName";
            buttonSetName.TabIndex = 2;
            buttonSetName.Text = "Set name";
            buttonSetName.UseVisualStyleBackColor = true;
            buttonSetName.Click += OnClickSetName;
            //
            // pictureBoxRamp
            //
            pictureBoxRamp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxRamp.Dock = System.Windows.Forms.DockStyle.Top;
            pictureBoxRamp.Name = "pictureBoxRamp";
            pictureBoxRamp.Size = new System.Drawing.Size(320, 60);
            pictureBoxRamp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBoxRamp.TabStop = false;
            //
            // labelColors
            //
            labelColors.Dock = System.Windows.Forms.DockStyle.Top;
            labelColors.Name = "labelColors";
            labelColors.Padding = new System.Windows.Forms.Padding(0, 6, 0, 2);
            labelColors.Size = new System.Drawing.Size(320, 24);
            labelColors.Text = "The 32 classic colors (click one to change it):";
            //
            // panelColors
            //
            panelColors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelColors.Cursor = System.Windows.Forms.Cursors.Hand;
            panelColors.Dock = System.Windows.Forms.DockStyle.Top;
            panelColors.Name = "panelColors";
            panelColors.Size = new System.Drawing.Size(320, 64);
            panelColors.TabIndex = 3;
            panelColors.Paint += OnPaintColors;
            panelColors.MouseClick += OnClickColors;
            panelColors.MouseMove += OnMoveColors;
            panelColors.Resize += OnResizeColors;
            //
            // labelInfo
            //
            labelInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            labelInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelInfo.Name = "labelInfo";
            labelInfo.Padding = new System.Windows.Forms.Padding(4);
            labelInfo.Size = new System.Drawing.Size(320, 120);
            //
            // flowLayoutPanelPreview
            //
            flowLayoutPanelPreview.Controls.Add(labelPreview);
            flowLayoutPanelPreview.Controls.Add(comboBoxPreview);
            flowLayoutPanelPreview.Controls.Add(textBoxPreviewId);
            flowLayoutPanelPreview.Controls.Add(checkBoxPartial);
            flowLayoutPanelPreview.Controls.Add(buttonPreview);
            flowLayoutPanelPreview.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelPreview.Name = "flowLayoutPanelPreview";
            flowLayoutPanelPreview.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            flowLayoutPanelPreview.Size = new System.Drawing.Size(320, 36);
            flowLayoutPanelPreview.TabIndex = 4;
            //
            // labelPreview
            //
            labelPreview.AutoSize = true;
            labelPreview.Margin = new System.Windows.Forms.Padding(3, 8, 0, 0);
            labelPreview.Name = "labelPreview";
            labelPreview.Text = "Preview on:";
            //
            // comboBoxPreview
            //
            comboBoxPreview.Items.AddRange(new object[] { "Item", "Gump", "Animation (body)" });
            comboBoxPreview.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxPreview.Name = "comboBoxPreview";
            comboBoxPreview.Size = new System.Drawing.Size(120, 23);
            comboBoxPreview.SelectedIndexChanged += OnPreviewChanged;
            //
            // textBoxPreviewId
            //
            textBoxPreviewId.Name = "textBoxPreviewId";
            textBoxPreviewId.Size = new System.Drawing.Size(70, 23);
            textBoxPreviewId.Text = "0x0E75";
            textBoxPreviewId.KeyDown += OnPreviewIdKeyDown;
            //
            // checkBoxPartial
            //
            checkBoxPartial.AutoSize = true;
            checkBoxPartial.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            checkBoxPartial.Name = "checkBoxPartial";
            checkBoxPartial.Text = "Only gray pixels";
            checkBoxPartial.UseVisualStyleBackColor = true;
            checkBoxPartial.CheckedChanged += OnPreviewChanged;
            //
            // buttonPreview
            //
            buttonPreview.AutoSize = true;
            buttonPreview.Name = "buttonPreview";
            buttonPreview.Text = "Show";
            buttonPreview.UseVisualStyleBackColor = true;
            buttonPreview.Click += OnPreviewChanged;
            //
            // pictureBoxPreview
            //
            pictureBoxPreview.BackColor = System.Drawing.Color.White;
            pictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Top;
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new System.Drawing.Size(320, 170);
            pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxPreview.TabStop = false;
            //
            // toolTip
            //
            //
            // tabPageCompare
            //
            tabPageCompare.Controls.Add(listViewCompare);
            tabPageCompare.Controls.Add(labelCompareInfo);
            tabPageCompare.Controls.Add(toolStripCompare);
            tabPageCompare.Location = new System.Drawing.Point(4, 24);
            tabPageCompare.Name = "tabPageCompare";
            tabPageCompare.Padding = new System.Windows.Forms.Padding(3);
            tabPageCompare.Size = new System.Drawing.Size(500, 300);
            tabPageCompare.TabIndex = 1;
            tabPageCompare.Text = "Compare";
            tabPageCompare.UseVisualStyleBackColor = true;
            //
            // toolStripCompare
            //
            toolStripCompare.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonCompare, toolStripSeparator4, toolStripLabelFilter, toolStripComboBoxFilter, toolStripSeparator5, toolStripButtonShowClassic, toolStripButtonShowEc, toolStripSeparator6, toolStripButtonMarkDifferences, toolStripButtonToEc, toolStripButtonToClassicMarked });
            toolStripCompare.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripCompare.Location = new System.Drawing.Point(0, 0);
            toolStripCompare.Name = "toolStripCompare";
            toolStripCompare.TabIndex = 0;
            //
            // toolStripButtonCompare
            //
            toolStripButtonCompare.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonCompare.Name = "toolStripButtonCompare";
            toolStripButtonCompare.Text = "Compare";
            toolStripButtonCompare.ToolTipText = "Compares every hue of the classic hues.mul with Hues.uop";
            toolStripButtonCompare.Click += OnClickCompare;
            //
            // toolStripSeparator4
            //
            toolStripSeparator4.Name = "toolStripSeparator4";
            //
            // toolStripLabelFilter
            //
            toolStripLabelFilter.Name = "toolStripLabelFilter";
            toolStripLabelFilter.Text = "Show:";
            //
            // toolStripComboBoxFilter
            //
            toolStripComboBoxFilter.Items.AddRange(new object[] { "All hues", "Differences", "Colors differ", "Names differ", "Only in classic", "Only in EC", "EC colors not exact" });
            toolStripComboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxFilter.Name = "toolStripComboBoxFilter";
            toolStripComboBoxFilter.Size = new System.Drawing.Size(140, 25);
            toolStripComboBoxFilter.SelectedIndexChanged += OnFilterChanged;
            //
            // toolStripSeparator5
            //
            toolStripSeparator5.Name = "toolStripSeparator5";
            //
            // toolStripButtonShowClassic
            //
            toolStripButtonShowClassic.CheckOnClick = true;
            toolStripButtonShowClassic.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonShowClassic.Checked = true;
            toolStripButtonShowClassic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonShowClassic.Name = "toolStripButtonShowClassic";
            toolStripButtonShowClassic.Text = "Classic";
            toolStripButtonShowClassic.ToolTipText = "Show / hide the classic columns";
            toolStripButtonShowClassic.CheckedChanged += OnShowColumnsChanged;
            //
            // toolStripButtonShowEc
            //
            toolStripButtonShowEc.CheckOnClick = true;
            toolStripButtonShowEc.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonShowEc.Checked = true;
            toolStripButtonShowEc.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonShowEc.Name = "toolStripButtonShowEc";
            toolStripButtonShowEc.Text = "Enhanced";
            toolStripButtonShowEc.ToolTipText = "Show / hide the Enhanced Client columns";
            toolStripButtonShowEc.CheckedChanged += OnShowColumnsChanged;
            //
            // toolStripSeparator6
            //
            toolStripSeparator6.Name = "toolStripSeparator6";
            //
            // toolStripButtonMarkDifferences
            //
            toolStripButtonMarkDifferences.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonMarkDifferences.Name = "toolStripButtonMarkDifferences";
            toolStripButtonMarkDifferences.Text = "Mark all shown";
            toolStripButtonMarkDifferences.ToolTipText = "Marks every row of the list";
            toolStripButtonMarkDifferences.Click += OnClickMarkAll;
            //
            // toolStripButtonToEc
            //
            toolStripButtonToEc.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonToEc.Name = "toolStripButtonToEc";
            toolStripButtonToEc.Text = "Classic → EC";
            toolStripButtonToEc.ToolTipText = "Writes the marked hues from hues.mul into Hues.uop";
            toolStripButtonToEc.Click += OnClickToEcMarked;
            //
            // toolStripButtonToClassicMarked
            //
            toolStripButtonToClassicMarked.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonToClassicMarked.Name = "toolStripButtonToClassicMarked";
            toolStripButtonToClassicMarked.Text = "EC → Classic";
            toolStripButtonToClassicMarked.ToolTipText = "Writes the marked hues from Hues.uop into hues.mul";
            toolStripButtonToClassicMarked.Click += OnClickToClassicMarked;
            //
            // listViewCompare
            //
            listViewCompare.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderCompareHue, columnHeaderCompareClassic, columnHeaderCompareEc, columnHeaderCompareStatus, columnHeaderCompareQuality, columnHeaderCompareClassicName, columnHeaderCompareEcName });
            listViewCompare.ContextMenuStrip = contextMenuStripCompare;
            listViewCompare.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewCompare.FullRowSelect = true;
            listViewCompare.HideSelection = false;
            listViewCompare.Name = "listViewCompare";
            listViewCompare.OwnerDraw = true;
            listViewCompare.TabIndex = 0;
            listViewCompare.UseCompatibleStateImageBehavior = false;
            listViewCompare.View = System.Windows.Forms.View.Details;
            listViewCompare.VirtualMode = true;
            listViewCompare.RetrieveVirtualItem += RetrieveCompareItem;
            listViewCompare.DrawColumnHeader += OnDrawColumnHeader;
            listViewCompare.DrawSubItem += DrawCompareSubItem;
            listViewCompare.SelectedIndexChanged += OnCompareSelectedIndexChanged;
            listViewCompare.DoubleClick += OnClickShowInEditor;
            //
            // columnHeaderCompareHue
            //
            columnHeaderCompareHue.Text = "Hue";
            columnHeaderCompareHue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderCompareHue.Width = 60;
            //
            // columnHeaderCompareClassic
            //
            columnHeaderCompareClassic.Text = "Classic";
            columnHeaderCompareClassic.Width = 140;
            //
            // columnHeaderCompareEc
            //
            columnHeaderCompareEc.Text = "Enhanced";
            columnHeaderCompareEc.Width = 140;
            //
            // columnHeaderCompareStatus
            //
            columnHeaderCompareStatus.Text = "Status";
            columnHeaderCompareStatus.Width = 190;
            //
            // columnHeaderCompareQuality
            //
            columnHeaderCompareQuality.Text = "EC colors";
            columnHeaderCompareQuality.Width = 110;
            //
            // columnHeaderCompareClassicName
            //
            columnHeaderCompareClassicName.Text = "Classic name";
            columnHeaderCompareClassicName.Width = 150;
            //
            // columnHeaderCompareEcName
            //
            columnHeaderCompareEcName.Text = "EC name";
            columnHeaderCompareEcName.Width = 150;
            //
            // contextMenuStripCompare
            //
            contextMenuStripCompare.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toEcToolStripMenuItem, toClassicMarkedToolStripMenuItem, toolStripSeparatorMenu3, showInEditorToolStripMenuItem, markAllToolStripMenuItem });
            contextMenuStripCompare.Name = "contextMenuStripCompare";
            //
            // toEcToolStripMenuItem
            //
            toEcToolStripMenuItem.Name = "toEcToolStripMenuItem";
            toEcToolStripMenuItem.Text = "Classic → EC (marked hues)";
            toEcToolStripMenuItem.Click += OnClickToEcMarked;
            //
            // toClassicMarkedToolStripMenuItem
            //
            toClassicMarkedToolStripMenuItem.Name = "toClassicMarkedToolStripMenuItem";
            toClassicMarkedToolStripMenuItem.Text = "EC → Classic (marked hues)";
            toClassicMarkedToolStripMenuItem.Click += OnClickToClassicMarked;
            //
            // toolStripSeparatorMenu3
            //
            toolStripSeparatorMenu3.Name = "toolStripSeparatorMenu3";
            //
            // showInEditorToolStripMenuItem
            //
            showInEditorToolStripMenuItem.Name = "showInEditorToolStripMenuItem";
            showInEditorToolStripMenuItem.Text = "Show in Editor";
            showInEditorToolStripMenuItem.Click += OnClickShowInEditor;
            //
            // markAllToolStripMenuItem
            //
            markAllToolStripMenuItem.Name = "markAllToolStripMenuItem";
            markAllToolStripMenuItem.Text = "Mark all shown";
            markAllToolStripMenuItem.Click += OnClickMarkAll;
            //
            // labelCompareInfo
            //
            labelCompareInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            labelCompareInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelCompareInfo.Name = "labelCompareInfo";
            labelCompareInfo.Padding = new System.Windows.Forms.Padding(4);
            labelCompareInfo.Size = new System.Drawing.Size(500, 64);
            labelCompareInfo.Text = "Open the classic hues.mul and press Compare.";
            //
            // tabPageConvert
            //
            tabPageConvert.Controls.Add(flowLayoutPanelConvert);
            tabPageConvert.Controls.Add(textBoxLog);
            tabPageConvert.Location = new System.Drawing.Point(4, 24);
            tabPageConvert.Name = "tabPageConvert";
            tabPageConvert.Padding = new System.Windows.Forms.Padding(3);
            tabPageConvert.Size = new System.Drawing.Size(500, 300);
            tabPageConvert.TabIndex = 2;
            tabPageConvert.Text = "Converter";
            tabPageConvert.UseVisualStyleBackColor = true;
            //
            // flowLayoutPanelConvert
            //
            flowLayoutPanelConvert.Controls.Add(groupBoxToEc);
            flowLayoutPanelConvert.Controls.Add(groupBoxToClassic);
            flowLayoutPanelConvert.Controls.Add(groupBoxFiles);
            flowLayoutPanelConvert.Controls.Add(groupBoxRepair);
            flowLayoutPanelConvert.AutoScroll = true;
            flowLayoutPanelConvert.AutoSize = true;
            flowLayoutPanelConvert.AutoSize = false;
            flowLayoutPanelConvert.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanelConvert.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanelConvert.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            flowLayoutPanelConvert.Name = "flowLayoutPanelConvert";
            flowLayoutPanelConvert.Padding = new System.Windows.Forms.Padding(6);
            flowLayoutPanelConvert.WrapContents = false;
            //
            // groupBoxToEc
            //
            groupBoxToEc.Controls.Add(flowLayoutPanelToEc);
            groupBoxToEc.AutoSize = true;
            groupBoxToEc.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupBoxToEc.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            groupBoxToEc.Name = "groupBoxToEc";
            groupBoxToEc.Padding = new System.Windows.Forms.Padding(6, 3, 6, 6);
            groupBoxToEc.Text = "Classic hues.mul  →  Enhanced Hues.uop";
            //
            // flowLayoutPanelToEc
            //
            flowLayoutPanelToEc.Controls.Add(radioButtonToEcLoaded);
            flowLayoutPanelToEc.Controls.Add(radioButtonToEcNew);
            flowLayoutPanelToEc.Controls.Add(flowLayoutPanelStyle);
            flowLayoutPanelToEc.Controls.Add(flowLayoutPanelToEcRange);
            flowLayoutPanelToEc.Controls.Add(checkBoxToEcNames);
            flowLayoutPanelToEc.Controls.Add(checkBoxToEcOnlyChanged);
            flowLayoutPanelToEc.Controls.Add(checkBoxToEcSkipEmpty);
            flowLayoutPanelToEc.Controls.Add(checkBoxToEcExact);
            flowLayoutPanelToEc.Controls.Add(buttonToEc);
            flowLayoutPanelToEc.AutoSize = true;
            flowLayoutPanelToEc.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanelToEc.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanelToEc.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            flowLayoutPanelToEc.Name = "flowLayoutPanelToEc";
            flowLayoutPanelToEc.WrapContents = false;
            //
            // radioButtonToEcLoaded
            //
            radioButtonToEcLoaded.AutoSize = true;
            radioButtonToEcLoaded.Checked = true;
            radioButtonToEcLoaded.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            radioButtonToEcLoaded.Name = "radioButtonToEcLoaded";
            radioButtonToEcLoaded.TabStop = true;
            radioButtonToEcLoaded.Text = "Update the loaded Hues.uop (then \"Save Hues.uop\")";
            radioButtonToEcLoaded.UseVisualStyleBackColor = true;
            radioButtonToEcLoaded.CheckedChanged += OnTargetChanged;
            //
            // radioButtonToEcNew
            //
            radioButtonToEcNew.AutoSize = true;
            radioButtonToEcNew.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            radioButtonToEcNew.Name = "radioButtonToEcNew";
            radioButtonToEcNew.Text = "Create a new Hues.uop in the output folder";
            radioButtonToEcNew.UseVisualStyleBackColor = true;
            radioButtonToEcNew.CheckedChanged += OnTargetChanged;
            //
            // flowLayoutPanelStyle
            //
            flowLayoutPanelStyle.Controls.Add(labelStyle);
            flowLayoutPanelStyle.Controls.Add(comboBoxStyle);
            flowLayoutPanelStyle.Controls.Add(labelBmpBlur);
            flowLayoutPanelStyle.Controls.Add(numericUpDownBmpBlur);
            flowLayoutPanelStyle.Controls.Add(labelDdsBlur);
            flowLayoutPanelStyle.Controls.Add(numericUpDownDdsBlur);
            flowLayoutPanelStyle.Controls.Add(labelScale);
            flowLayoutPanelStyle.Controls.Add(comboBoxScale);
            flowLayoutPanelStyle.AutoSize = true;
            flowLayoutPanelStyle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            flowLayoutPanelStyle.Name = "flowLayoutPanelStyle";
            flowLayoutPanelStyle.WrapContents = false;
            //
            // labelStyle
            //
            labelStyle.AutoSize = true;
            labelStyle.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelStyle.Name = "labelStyle";
            labelStyle.Text = "Ramps:";
            //
            // comboBoxStyle
            //
            comboBoxStyle.Items.AddRange(new object[] { "Original client (linear, colors x 8)", "huesmul2uop (blurred, colors x 255 / 31)", "Custom: linear", "Custom: blurred" });
            comboBoxStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxStyle.Name = "comboBoxStyle";
            comboBoxStyle.Size = new System.Drawing.Size(250, 23);
            comboBoxStyle.SelectedIndexChanged += OnStyleChanged;
            //
            // labelBmpBlur
            //
            labelBmpBlur.AutoSize = true;
            labelBmpBlur.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelBmpBlur.Name = "labelBmpBlur";
            labelBmpBlur.Text = "bitmap blur:";
            //
            // numericUpDownBmpBlur
            //
            numericUpDownBmpBlur.Maximum = new decimal(new int[] { 17, 0, 0, 0 });
            numericUpDownBmpBlur.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numericUpDownBmpBlur.Name = "numericUpDownBmpBlur";
            numericUpDownBmpBlur.Size = new System.Drawing.Size(50, 23);
            numericUpDownBmpBlur.Value = new decimal(new int[] { 4, 0, 0, 0 });
            //
            // labelDdsBlur
            //
            labelDdsBlur.AutoSize = true;
            labelDdsBlur.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelDdsBlur.Name = "labelDdsBlur";
            labelDdsBlur.Text = "hues.dds blur:";
            //
            // numericUpDownDdsBlur
            //
            numericUpDownDdsBlur.Maximum = new decimal(new int[] { 17, 0, 0, 0 });
            numericUpDownDdsBlur.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numericUpDownDdsBlur.Name = "numericUpDownDdsBlur";
            numericUpDownDdsBlur.Size = new System.Drawing.Size(50, 23);
            numericUpDownDdsBlur.Value = new decimal(new int[] { 6, 0, 0, 0 });
            //
            // labelScale
            //
            labelScale.AutoSize = true;
            labelScale.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelScale.Name = "labelScale";
            labelScale.Text = "colors:";
            //
            // comboBoxScale
            //
            comboBoxScale.Items.AddRange(new object[] { "x 255 / 31", "x 8", "bit copy" });
            comboBoxScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxScale.Name = "comboBoxScale";
            comboBoxScale.Size = new System.Drawing.Size(90, 23);
            //
            // flowLayoutPanelToEcRange
            //
            flowLayoutPanelToEcRange.Controls.Add(checkBoxToEcRange);
            flowLayoutPanelToEcRange.Controls.Add(numericUpDownToEcFrom);
            flowLayoutPanelToEcRange.Controls.Add(labelToEcTo);
            flowLayoutPanelToEcRange.Controls.Add(numericUpDownToEcTo);
            flowLayoutPanelToEcRange.AutoSize = true;
            flowLayoutPanelToEcRange.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            flowLayoutPanelToEcRange.Name = "flowLayoutPanelToEcRange";
            flowLayoutPanelToEcRange.WrapContents = false;
            //
            // checkBoxToEcRange
            //
            checkBoxToEcRange.AutoSize = true;
            checkBoxToEcRange.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            checkBoxToEcRange.Margin = new System.Windows.Forms.Padding(3, 6, 3, 2);
            checkBoxToEcRange.Name = "checkBoxToEcRange";
            checkBoxToEcRange.Text = "Only the hues";
            checkBoxToEcRange.UseVisualStyleBackColor = true;
            //
            // numericUpDownToEcFrom
            //
            numericUpDownToEcFrom.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numericUpDownToEcFrom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownToEcFrom.Name = "numericUpDownToEcFrom";
            numericUpDownToEcFrom.Size = new System.Drawing.Size(70, 23);
            numericUpDownToEcFrom.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // labelToEcTo
            //
            labelToEcTo.AutoSize = true;
            labelToEcTo.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelToEcTo.Name = "labelToEcTo";
            labelToEcTo.Text = "to";
            //
            // numericUpDownToEcTo
            //
            numericUpDownToEcTo.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numericUpDownToEcTo.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownToEcTo.Name = "numericUpDownToEcTo";
            numericUpDownToEcTo.Size = new System.Drawing.Size(70, 23);
            numericUpDownToEcTo.Value = new decimal(new int[] { 3000, 0, 0, 0 });
            //
            // checkBoxToEcNames
            //
            checkBoxToEcNames.AutoSize = true;
            checkBoxToEcNames.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxToEcNames.Checked = true;
            checkBoxToEcNames.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            checkBoxToEcNames.Name = "checkBoxToEcNames";
            checkBoxToEcNames.Text = "Names (huenames.csv)";
            checkBoxToEcNames.UseVisualStyleBackColor = true;
            //
            // checkBoxToEcOnlyChanged
            //
            checkBoxToEcOnlyChanged.AutoSize = true;
            checkBoxToEcOnlyChanged.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxToEcOnlyChanged.Checked = true;
            checkBoxToEcOnlyChanged.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            checkBoxToEcOnlyChanged.Name = "checkBoxToEcOnlyChanged";
            checkBoxToEcOnlyChanged.Text = "Only hues that differ (update)";
            checkBoxToEcOnlyChanged.UseVisualStyleBackColor = true;
            //
            // checkBoxToEcSkipEmpty
            //
            checkBoxToEcSkipEmpty.AutoSize = true;
            checkBoxToEcSkipEmpty.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            checkBoxToEcSkipEmpty.Name = "checkBoxToEcSkipEmpty";
            checkBoxToEcSkipEmpty.Text = "Skip unused slots (all colors 0 and no name)";
            checkBoxToEcSkipEmpty.UseVisualStyleBackColor = true;
            //
            // checkBoxToEcExact
            //
            checkBoxToEcExact.AutoSize = true;
            checkBoxToEcExact.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxToEcExact.Checked = true;
            checkBoxToEcExact.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            checkBoxToEcExact.Name = "checkBoxToEcExact";
            checkBoxToEcExact.Text = "Store the exact classic data in Hues.uop (lossless way back, the client ignores it)";
            checkBoxToEcExact.UseVisualStyleBackColor = true;
            //
            // buttonToEc
            //
            buttonToEc.Name = "buttonToEc";
            buttonToEc.Size = new System.Drawing.Size(170, 30);
            buttonToEc.Text = "Convert to Enhanced";
            buttonToEc.UseVisualStyleBackColor = true;
            buttonToEc.Click += OnClickConvertToEc;
            //
            // groupBoxToClassic
            //
            groupBoxToClassic.Controls.Add(flowLayoutPanelToClassic);
            groupBoxToClassic.AutoSize = true;
            groupBoxToClassic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupBoxToClassic.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            groupBoxToClassic.Name = "groupBoxToClassic";
            groupBoxToClassic.Padding = new System.Windows.Forms.Padding(6, 3, 6, 6);
            groupBoxToClassic.Text = "Enhanced Hues.uop  →  Classic hues.mul";
            //
            // flowLayoutPanelToClassic
            //
            flowLayoutPanelToClassic.Controls.Add(radioButtonToClassicLoaded);
            flowLayoutPanelToClassic.Controls.Add(radioButtonToClassicNew);
            flowLayoutPanelToClassic.Controls.Add(flowLayoutPanelToClassicRange);
            flowLayoutPanelToClassic.Controls.Add(checkBoxToClassicNames);
            flowLayoutPanelToClassic.Controls.Add(checkBoxToClassicKeepTable);
            flowLayoutPanelToClassic.Controls.Add(buttonToClassic);
            flowLayoutPanelToClassic.AutoSize = true;
            flowLayoutPanelToClassic.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanelToClassic.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanelToClassic.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            flowLayoutPanelToClassic.Name = "flowLayoutPanelToClassic";
            flowLayoutPanelToClassic.WrapContents = false;
            //
            // radioButtonToClassicLoaded
            //
            radioButtonToClassicLoaded.AutoSize = true;
            radioButtonToClassicLoaded.Checked = true;
            radioButtonToClassicLoaded.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            radioButtonToClassicLoaded.Name = "radioButtonToClassicLoaded";
            radioButtonToClassicLoaded.TabStop = true;
            radioButtonToClassicLoaded.Text = "Update the loaded hues.mul: group headers, table values and the other hues stay (then \"Save hues.mul\")";
            radioButtonToClassicLoaded.UseVisualStyleBackColor = true;
            //
            // radioButtonToClassicNew
            //
            radioButtonToClassicNew.AutoSize = true;
            radioButtonToClassicNew.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            radioButtonToClassicNew.Name = "radioButtonToClassicNew";
            radioButtonToClassicNew.Text = "Create a new hues.mul in the output folder";
            radioButtonToClassicNew.UseVisualStyleBackColor = true;
            //
            // flowLayoutPanelToClassicRange
            //
            flowLayoutPanelToClassicRange.Controls.Add(checkBoxToClassicRange);
            flowLayoutPanelToClassicRange.Controls.Add(numericUpDownToClassicFrom);
            flowLayoutPanelToClassicRange.Controls.Add(labelToClassicTo);
            flowLayoutPanelToClassicRange.Controls.Add(numericUpDownToClassicTo);
            flowLayoutPanelToClassicRange.AutoSize = true;
            flowLayoutPanelToClassicRange.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            flowLayoutPanelToClassicRange.Name = "flowLayoutPanelToClassicRange";
            flowLayoutPanelToClassicRange.WrapContents = false;
            //
            // checkBoxToClassicRange
            //
            checkBoxToClassicRange.AutoSize = true;
            checkBoxToClassicRange.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            checkBoxToClassicRange.Margin = new System.Windows.Forms.Padding(3, 6, 3, 2);
            checkBoxToClassicRange.Name = "checkBoxToClassicRange";
            checkBoxToClassicRange.Text = "Only the hues";
            checkBoxToClassicRange.UseVisualStyleBackColor = true;
            //
            // numericUpDownToClassicFrom
            //
            numericUpDownToClassicFrom.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numericUpDownToClassicFrom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownToClassicFrom.Name = "numericUpDownToClassicFrom";
            numericUpDownToClassicFrom.Size = new System.Drawing.Size(70, 23);
            numericUpDownToClassicFrom.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // labelToClassicTo
            //
            labelToClassicTo.AutoSize = true;
            labelToClassicTo.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelToClassicTo.Name = "labelToClassicTo";
            labelToClassicTo.Text = "to";
            //
            // numericUpDownToClassicTo
            //
            numericUpDownToClassicTo.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numericUpDownToClassicTo.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownToClassicTo.Name = "numericUpDownToClassicTo";
            numericUpDownToClassicTo.Size = new System.Drawing.Size(70, 23);
            numericUpDownToClassicTo.Value = new decimal(new int[] { 3000, 0, 0, 0 });
            //
            // checkBoxToClassicNames
            //
            checkBoxToClassicNames.AutoSize = true;
            checkBoxToClassicNames.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxToClassicNames.Checked = true;
            checkBoxToClassicNames.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            checkBoxToClassicNames.Name = "checkBoxToClassicNames";
            checkBoxToClassicNames.Text = "Names (cut to 20 characters)";
            checkBoxToClassicNames.UseVisualStyleBackColor = true;
            //
            // checkBoxToClassicKeepTable
            //
            checkBoxToClassicKeepTable.AutoSize = true;
            checkBoxToClassicKeepTable.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxToClassicKeepTable.Checked = true;
            checkBoxToClassicKeepTable.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            checkBoxToClassicKeepTable.Name = "checkBoxToClassicKeepTable";
            checkBoxToClassicKeepTable.Text = "Keep table start / end of hues whose colors did not change";
            checkBoxToClassicKeepTable.UseVisualStyleBackColor = true;
            //
            // buttonToClassic
            //
            buttonToClassic.Name = "buttonToClassic";
            buttonToClassic.Size = new System.Drawing.Size(170, 30);
            buttonToClassic.Text = "Convert to Classic";
            buttonToClassic.UseVisualStyleBackColor = true;
            buttonToClassic.Click += OnClickConvertToClassic;
            //
            // groupBoxFiles
            //
            groupBoxFiles.Controls.Add(tableLayoutPanelFiles);
            groupBoxFiles.AutoSize = true;
            groupBoxFiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupBoxFiles.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            groupBoxFiles.Name = "groupBoxFiles";
            groupBoxFiles.Padding = new System.Windows.Forms.Padding(6, 3, 6, 6);
            groupBoxFiles.Text = "Output folder and backup";
            //
            // tableLayoutPanelFiles
            //
            tableLayoutPanelFiles.Controls.Add(labelOutput, 0, 0);
            tableLayoutPanelFiles.Controls.Add(textBoxOutput, 1, 0);
            tableLayoutPanelFiles.Controls.Add(buttonOutput, 2, 0);
            tableLayoutPanelFiles.Controls.Add(checkBoxBackup, 0, 1);
            tableLayoutPanelFiles.Controls.Add(textBoxBackup, 1, 1);
            tableLayoutPanelFiles.Controls.Add(buttonBackup, 2, 1);
            tableLayoutPanelFiles.Controls.Add(buttonOpenBackup, 3, 1);
            tableLayoutPanelFiles.AutoSize = true;
            tableLayoutPanelFiles.ColumnCount = 4;
            tableLayoutPanelFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanelFiles.Name = "tableLayoutPanelFiles";
            tableLayoutPanelFiles.RowCount = 2;
            tableLayoutPanelFiles.Size = new System.Drawing.Size(760, 64);
            tableLayoutPanelFiles.TabIndex = 0;
            tableLayoutPanelFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPanelFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 480F));
            tableLayoutPanelFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPanelFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPanelFiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPanelFiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            //
            // labelOutput
            //
            labelOutput.AutoSize = true;
            labelOutput.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelOutput.Name = "labelOutput";
            labelOutput.Text = "Output folder:";
            //
            // textBoxOutput
            //
            textBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxOutput.Name = "textBoxOutput";
            textBoxOutput.TabIndex = 1;
            textBoxOutput.Leave += OnFolderLeave;
            //
            // buttonOutput
            //
            buttonOutput.Name = "buttonOutput";
            buttonOutput.Size = new System.Drawing.Size(30, 23);
            buttonOutput.Text = "...";
            buttonOutput.UseVisualStyleBackColor = true;
            buttonOutput.Click += OnClickBrowseOutput;
            //
            // checkBoxBackup
            //
            checkBoxBackup.AutoSize = true;
            checkBoxBackup.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxBackup.Checked = true;
            checkBoxBackup.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            checkBoxBackup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 2);
            checkBoxBackup.Name = "checkBoxBackup";
            checkBoxBackup.Text = "Backup (old + new copy):";
            checkBoxBackup.UseVisualStyleBackColor = true;
            checkBoxBackup.CheckedChanged += OnBackupChanged;
            //
            // textBoxBackup
            //
            textBoxBackup.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxBackup.Name = "textBoxBackup";
            textBoxBackup.PlaceholderText = "empty = folder \"UoFiddlerBackups\\\\Hues\" next to the written file";
            textBoxBackup.TabIndex = 4;
            textBoxBackup.Leave += OnFolderLeave;
            //
            // buttonBackup
            //
            buttonBackup.Name = "buttonBackup";
            buttonBackup.Size = new System.Drawing.Size(30, 23);
            buttonBackup.Text = "...";
            buttonBackup.UseVisualStyleBackColor = true;
            buttonBackup.Click += OnClickBrowseBackup;
            //
            // buttonOpenBackup
            //
            buttonOpenBackup.Name = "buttonOpenBackup";
            buttonOpenBackup.Size = new System.Drawing.Size(60, 23);
            buttonOpenBackup.Text = "Open";
            buttonOpenBackup.UseVisualStyleBackColor = true;
            buttonOpenBackup.Click += OnClickOpenBackup;
            //
            // groupBoxRepair
            //
            groupBoxRepair.Controls.Add(flowLayoutPanelRepair);
            groupBoxRepair.AutoSize = true;
            groupBoxRepair.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupBoxRepair.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            groupBoxRepair.Name = "groupBoxRepair";
            groupBoxRepair.Padding = new System.Windows.Forms.Padding(6, 3, 6, 6);
            groupBoxRepair.Text = "Analyze / repair Hues.uop";
            //
            // flowLayoutPanelRepair
            //
            flowLayoutPanelRepair.Controls.Add(buttonAnalyze);
            flowLayoutPanelRepair.Controls.Add(buttonRebuildAtlas);
            flowLayoutPanelRepair.AutoSize = true;
            flowLayoutPanelRepair.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanelRepair.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            flowLayoutPanelRepair.Name = "flowLayoutPanelRepair";
            flowLayoutPanelRepair.WrapContents = false;
            //
            // buttonAnalyze
            //
            buttonAnalyze.Name = "buttonAnalyze";
            buttonAnalyze.Size = new System.Drawing.Size(170, 30);
            buttonAnalyze.Text = "Analyze";
            buttonAnalyze.UseVisualStyleBackColor = true;
            buttonAnalyze.Click += OnClickAnalyze;
            //
            // buttonRebuildAtlas
            //
            buttonRebuildAtlas.Name = "buttonRebuildAtlas";
            buttonRebuildAtlas.Size = new System.Drawing.Size(200, 30);
            buttonRebuildAtlas.Text = "Rebuild hues.dds + overview";
            buttonRebuildAtlas.UseVisualStyleBackColor = true;
            buttonRebuildAtlas.Click += OnClickRebuildAtlas;
            //
            // textBoxLog
            //
            textBoxLog.BackColor = System.Drawing.Color.White;
            textBoxLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            textBoxLog.Font = new System.Drawing.Font("Consolas", 9F);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textBoxLog.Size = new System.Drawing.Size(900, 160);
            textBoxLog.TabIndex = 4;
            textBoxLog.WordWrap = false;
            //
            // tabPageHelp
            //
            tabPageHelp.Controls.Add(richTextBoxHelp);
            tabPageHelp.Controls.Add(flowLayoutPanelHelp);
            tabPageHelp.Location = new System.Drawing.Point(4, 24);
            tabPageHelp.Name = "tabPageHelp";
            tabPageHelp.Padding = new System.Windows.Forms.Padding(3);
            tabPageHelp.Size = new System.Drawing.Size(500, 300);
            tabPageHelp.TabIndex = 3;
            tabPageHelp.Text = "Help";
            tabPageHelp.UseVisualStyleBackColor = true;
            //
            // flowLayoutPanelHelp
            //
            flowLayoutPanelHelp.Controls.Add(radioButtonHelpEnglish);
            flowLayoutPanelHelp.Controls.Add(radioButtonHelpGerman);
            flowLayoutPanelHelp.AutoSize = true;
            flowLayoutPanelHelp.AutoSize = false;
            flowLayoutPanelHelp.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelHelp.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            flowLayoutPanelHelp.Name = "flowLayoutPanelHelp";
            flowLayoutPanelHelp.Size = new System.Drawing.Size(500, 30);
            flowLayoutPanelHelp.WrapContents = false;
            //
            // radioButtonHelpEnglish
            //
            radioButtonHelpEnglish.AutoSize = true;
            radioButtonHelpEnglish.Checked = true;
            radioButtonHelpEnglish.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            radioButtonHelpEnglish.Name = "radioButtonHelpEnglish";
            radioButtonHelpEnglish.TabStop = true;
            radioButtonHelpEnglish.Text = "English";
            radioButtonHelpEnglish.UseVisualStyleBackColor = true;
            radioButtonHelpEnglish.CheckedChanged += OnHelpLanguageChanged;
            //
            // radioButtonHelpGerman
            //
            radioButtonHelpGerman.AutoSize = true;
            radioButtonHelpGerman.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            radioButtonHelpGerman.Name = "radioButtonHelpGerman";
            radioButtonHelpGerman.Text = "Deutsch";
            radioButtonHelpGerman.UseVisualStyleBackColor = true;
            radioButtonHelpGerman.CheckedChanged += OnHelpLanguageChanged;
            //
            // richTextBoxHelp
            //
            richTextBoxHelp.BackColor = System.Drawing.Color.White;
            richTextBoxHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBoxHelp.Font = new System.Drawing.Font("Consolas", 9F);
            richTextBoxHelp.Name = "richTextBoxHelp";
            richTextBoxHelp.ReadOnly = true;
            richTextBoxHelp.WordWrap = false;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel, toolStripProgressBar });
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
            // toolStripProgressBar
            //
            toolStripProgressBar.Name = "toolStripProgressBar";
            toolStripProgressBar.Size = new System.Drawing.Size(160, 16);
            //
            // EcHuesControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tabControl);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcHuesControl";
            Size = new System.Drawing.Size(950, 650);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            tabControl.ResumeLayout(false);
            tabPageEditor.ResumeLayout(false);
            tabPageEditor.PerformLayout();
            toolStripEditor.ResumeLayout(false);
            toolStripEditor.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            contextMenuStripList.ResumeLayout(false);
            panelDetails.ResumeLayout(false);
            panelDetails.PerformLayout();
            flowLayoutPanelName.ResumeLayout(false);
            flowLayoutPanelName.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRamp).EndInit();
            panelColors.ResumeLayout(false);
            panelColors.PerformLayout();
            flowLayoutPanelPreview.ResumeLayout(false);
            flowLayoutPanelPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            tabPageCompare.ResumeLayout(false);
            tabPageCompare.PerformLayout();
            toolStripCompare.ResumeLayout(false);
            toolStripCompare.PerformLayout();
            contextMenuStripCompare.ResumeLayout(false);
            tabPageConvert.ResumeLayout(false);
            tabPageConvert.PerformLayout();
            flowLayoutPanelConvert.ResumeLayout(false);
            flowLayoutPanelConvert.PerformLayout();
            groupBoxToEc.ResumeLayout(false);
            groupBoxToEc.PerformLayout();
            flowLayoutPanelToEc.ResumeLayout(false);
            flowLayoutPanelToEc.PerformLayout();
            flowLayoutPanelStyle.ResumeLayout(false);
            flowLayoutPanelStyle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownBmpBlur).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownDdsBlur).EndInit();
            flowLayoutPanelToEcRange.ResumeLayout(false);
            flowLayoutPanelToEcRange.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownToEcFrom).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownToEcTo).EndInit();
            groupBoxToClassic.ResumeLayout(false);
            groupBoxToClassic.PerformLayout();
            flowLayoutPanelToClassic.ResumeLayout(false);
            flowLayoutPanelToClassic.PerformLayout();
            flowLayoutPanelToClassicRange.ResumeLayout(false);
            flowLayoutPanelToClassicRange.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownToClassicFrom).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownToClassicTo).EndInit();
            groupBoxFiles.ResumeLayout(false);
            groupBoxFiles.PerformLayout();
            tableLayoutPanelFiles.ResumeLayout(false);
            tableLayoutPanelFiles.PerformLayout();
            groupBoxRepair.ResumeLayout(false);
            groupBoxRepair.PerformLayout();
            flowLayoutPanelRepair.ResumeLayout(false);
            flowLayoutPanelRepair.PerformLayout();
            tabPageHelp.ResumeLayout(false);
            tabPageHelp.PerformLayout();
            flowLayoutPanelHelp.ResumeLayout(false);
            flowLayoutPanelHelp.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabelClassic;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpenClassic;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveClassic;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveClassicAs;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageEditor;
        private System.Windows.Forms.ToolStrip toolStripEditor;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSearch;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonApply;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonImportRamp;
        private System.Windows.Forms.ToolStripButton toolStripButtonExportRamp;
        private System.Windows.Forms.ToolStripButton toolStripButtonAddCopy;
        private System.Windows.Forms.ToolStripButton toolStripButtonDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButtonFromClassic;
        private System.Windows.Forms.ToolStripButton toolStripButtonToClassic;
        private System.Windows.Forms.ToolStripButton toolStripButtonGradient;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListView listViewHues;
        private System.Windows.Forms.ColumnHeader columnHeaderHue;
        private System.Windows.Forms.ColumnHeader columnHeaderRamp;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderColors;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripList;
        private System.Windows.Forms.ToolStripMenuItem importRampToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportRampToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu1;
        private System.Windows.Forms.ToolStripMenuItem fromClassicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toClassicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gradientToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu2;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Panel panelDetails;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Button buttonSetName;
        private System.Windows.Forms.PictureBox pictureBoxRamp;
        private System.Windows.Forms.Label labelColors;
        private System.Windows.Forms.Panel panelColors;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPreview;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.ComboBox comboBoxPreview;
        private System.Windows.Forms.TextBox textBoxPreviewId;
        private System.Windows.Forms.CheckBox checkBoxPartial;
        private System.Windows.Forms.Button buttonPreview;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.TabPage tabPageCompare;
        private System.Windows.Forms.ToolStrip toolStripCompare;
        private System.Windows.Forms.ToolStripButton toolStripButtonCompare;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel toolStripLabelFilter;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxFilter;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowClassic;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowEc;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton toolStripButtonMarkDifferences;
        private System.Windows.Forms.ToolStripButton toolStripButtonToEc;
        private System.Windows.Forms.ToolStripButton toolStripButtonToClassicMarked;
        private System.Windows.Forms.ListView listViewCompare;
        private System.Windows.Forms.ColumnHeader columnHeaderCompareHue;
        private System.Windows.Forms.ColumnHeader columnHeaderCompareClassic;
        private System.Windows.Forms.ColumnHeader columnHeaderCompareEc;
        private System.Windows.Forms.ColumnHeader columnHeaderCompareStatus;
        private System.Windows.Forms.ColumnHeader columnHeaderCompareQuality;
        private System.Windows.Forms.ColumnHeader columnHeaderCompareClassicName;
        private System.Windows.Forms.ColumnHeader columnHeaderCompareEcName;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripCompare;
        private System.Windows.Forms.ToolStripMenuItem toEcToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toClassicMarkedToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu3;
        private System.Windows.Forms.ToolStripMenuItem showInEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markAllToolStripMenuItem;
        private System.Windows.Forms.Label labelCompareInfo;
        private System.Windows.Forms.TabPage tabPageConvert;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelConvert;
        private System.Windows.Forms.GroupBox groupBoxToEc;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelToEc;
        private System.Windows.Forms.RadioButton radioButtonToEcLoaded;
        private System.Windows.Forms.RadioButton radioButtonToEcNew;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelStyle;
        private System.Windows.Forms.Label labelStyle;
        private System.Windows.Forms.ComboBox comboBoxStyle;
        private System.Windows.Forms.Label labelBmpBlur;
        private System.Windows.Forms.NumericUpDown numericUpDownBmpBlur;
        private System.Windows.Forms.Label labelDdsBlur;
        private System.Windows.Forms.NumericUpDown numericUpDownDdsBlur;
        private System.Windows.Forms.Label labelScale;
        private System.Windows.Forms.ComboBox comboBoxScale;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelToEcRange;
        private System.Windows.Forms.CheckBox checkBoxToEcRange;
        private System.Windows.Forms.NumericUpDown numericUpDownToEcFrom;
        private System.Windows.Forms.Label labelToEcTo;
        private System.Windows.Forms.NumericUpDown numericUpDownToEcTo;
        private System.Windows.Forms.CheckBox checkBoxToEcNames;
        private System.Windows.Forms.CheckBox checkBoxToEcOnlyChanged;
        private System.Windows.Forms.CheckBox checkBoxToEcSkipEmpty;
        private System.Windows.Forms.CheckBox checkBoxToEcExact;
        private System.Windows.Forms.Button buttonToEc;
        private System.Windows.Forms.GroupBox groupBoxToClassic;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelToClassic;
        private System.Windows.Forms.RadioButton radioButtonToClassicLoaded;
        private System.Windows.Forms.RadioButton radioButtonToClassicNew;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelToClassicRange;
        private System.Windows.Forms.CheckBox checkBoxToClassicRange;
        private System.Windows.Forms.NumericUpDown numericUpDownToClassicFrom;
        private System.Windows.Forms.Label labelToClassicTo;
        private System.Windows.Forms.NumericUpDown numericUpDownToClassicTo;
        private System.Windows.Forms.CheckBox checkBoxToClassicNames;
        private System.Windows.Forms.CheckBox checkBoxToClassicKeepTable;
        private System.Windows.Forms.Button buttonToClassic;
        private System.Windows.Forms.GroupBox groupBoxFiles;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFiles;
        private System.Windows.Forms.Label labelOutput;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Button buttonOutput;
        private System.Windows.Forms.CheckBox checkBoxBackup;
        private System.Windows.Forms.TextBox textBoxBackup;
        private System.Windows.Forms.Button buttonBackup;
        private System.Windows.Forms.Button buttonOpenBackup;
        private System.Windows.Forms.GroupBox groupBoxRepair;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelRepair;
        private System.Windows.Forms.Button buttonAnalyze;
        private System.Windows.Forms.Button buttonRebuildAtlas;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.TabPage tabPageHelp;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelHelp;
        private System.Windows.Forms.RadioButton radioButtonHelpEnglish;
        private System.Windows.Forms.RadioButton radioButtonHelpGerman;
        private System.Windows.Forms.RichTextBox richTextBoxHelp;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
    }
}
