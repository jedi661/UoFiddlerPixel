/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.SendItem.Forms
{
    partial class SendItemOptionsForm
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
            toolStripTop = new System.Windows.Forms.ToolStrip();
            lblItemsCmd = new System.Windows.Forms.ToolStripLabel();
            cmdtext = new System.Windows.Forms.ToolStripTextBox();
            lblArgs = new System.Windows.Forms.ToolStripLabel();
            argstext = new System.Windows.Forms.ToolStripTextBox();
            SendOnClick = new System.Windows.Forms.ToolStripButton();
            sepTop1 = new System.Windows.Forms.ToolStripSeparator();
            lblDelay = new System.Windows.Forms.ToolStripLabel();
            txtDelay = new System.Windows.Forms.ToolStripTextBox();
            BtnFocusClient = new System.Windows.Forms.ToolStripButton();
            sepTop2 = new System.Windows.Forms.ToolStripSeparator();
            BtnImport = new System.Windows.Forms.ToolStripButton();
            BtnExport = new System.Windows.Forms.ToolStripButton();
            BtnHelp = new System.Windows.Forms.ToolStripButton();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblClient = new System.Windows.Forms.ToolStripStatusLabel();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            lblCount = new System.Windows.Forms.ToolStripStatusLabel();
            splitMain = new System.Windows.Forms.SplitContainer();
            tableLeft = new System.Windows.Forms.TableLayoutPanel();
            tableSearch = new System.Windows.Forms.TableLayoutPanel();
            textSearch = new System.Windows.Forms.TextBox();
            comboFlag = new System.Windows.Forms.ComboBox();
            comboSize = new System.Windows.Forms.ComboBox();
            panelPreview = new System.Windows.Forms.Panel();
            pictureStatic = new System.Windows.Forms.PictureBox();
            lblStaticInfo = new System.Windows.Forms.Label();
            lblHue = new System.Windows.Forms.Label();
            numHue = new System.Windows.Forms.NumericUpDown();
            lblTemplate = new System.Windows.Forms.Label();
            comboTemplate = new System.Windows.Forms.ComboBox();
            txtCommandPreview = new System.Windows.Forms.TextBox();
            BtnPlace = new System.Windows.Forms.Button();
            BtnAddToTab = new System.Windows.Forms.Button();
            BtnFromItems = new System.Windows.Forms.Button();
            BtnCopyId = new System.Windows.Forms.Button();
            toolStripTabs = new System.Windows.Forms.ToolStrip();
            BtnNewTab = new System.Windows.Forms.ToolStripButton();
            BtnRenameTab = new System.Windows.Forms.ToolStripButton();
            BtnDeleteTab = new System.Windows.Forms.ToolStripButton();
            BtnTabLeft = new System.Windows.Forms.ToolStripButton();
            BtnTabRight = new System.Windows.Forms.ToolStripButton();
            sepTabs1 = new System.Windows.Forms.ToolStripSeparator();
            BtnNewCommand = new System.Windows.Forms.ToolStripButton();
            BtnAddStatics = new System.Windows.Forms.ToolStripButton();
            sepTabs2 = new System.Windows.Forms.ToolStripSeparator();
            lblTabHint = new System.Windows.Forms.ToolStripLabel();
            tabButtons = new System.Windows.Forms.TabControl();
            groupHistory = new System.Windows.Forms.GroupBox();
            listHistory = new System.Windows.Forms.ListBox();
            staticGrid = new StaticGrid();
            contextButton = new System.Windows.Forms.ContextMenuStrip(components);
            menuBtnSend = new System.Windows.Forms.ToolStripMenuItem();
            menuBtnEdit = new System.Windows.Forms.ToolStripMenuItem();
            menuBtnDuplicate = new System.Windows.Forms.ToolStripMenuItem();
            menuBtnMoveTo = new System.Windows.Forms.ToolStripMenuItem();
            menuBtnLeft = new System.Windows.Forms.ToolStripMenuItem();
            menuBtnRight = new System.Windows.Forms.ToolStripMenuItem();
            menuBtnSep = new System.Windows.Forms.ToolStripSeparator();
            menuBtnDelete = new System.Windows.Forms.ToolStripMenuItem();
            contextGrid = new System.Windows.Forms.ContextMenuStrip(components);
            menuGridPlace = new System.Windows.Forms.ToolStripMenuItem();
            menuGridAddToTab = new System.Windows.Forms.ToolStripMenuItem();
            menuGridCopyId = new System.Windows.Forms.ToolStripMenuItem();
            contextHistory = new System.Windows.Forms.ContextMenuStrip(components);
            menuHistorySend = new System.Windows.Forms.ToolStripMenuItem();
            menuHistorySave = new System.Windows.Forms.ToolStripMenuItem();
            menuHistoryClear = new System.Windows.Forms.ToolStripMenuItem();
            timerSearch = new System.Windows.Forms.Timer(components);
            toolTip = new System.Windows.Forms.ToolTip(components);
            toolStripTop.SuspendLayout();
            statusStrip.SuspendLayout();
            toolStripTabs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            tableLeft.SuspendLayout();
            tableSearch.SuspendLayout();
            panelPreview.SuspendLayout();
            groupHistory.SuspendLayout();
            contextButton.SuspendLayout();
            contextGrid.SuspendLayout();
            contextHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureStatic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHue).BeginInit();
            SuspendLayout();
            // 
            // toolStripTop
            // 
            toolStripTop.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblItemsCmd, cmdtext, lblArgs, argstext, SendOnClick, sepTop1, lblDelay, txtDelay, BtnFocusClient, sepTop2, BtnImport, BtnExport, BtnHelp });
            toolStripTop.Name = "toolStripTop";
            toolStripTop.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            toolStripTop.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStripTop.Location = new System.Drawing.Point(0, 0);
            toolStripTop.Size = new System.Drawing.Size(1200, 27);
            toolStripTop.TabIndex = 0;
            // 
            // lblItemsCmd
            // 
            lblItemsCmd.Name = "lblItemsCmd";
            lblItemsCmd.Text = "Items tab cmd:";
            // 
            // cmdtext
            // 
            cmdtext.Name = "cmdtext";
            cmdtext.Size = new System.Drawing.Size(90, 23);
            cmdtext.ToolTipText = "Command sent from the Items tab context menu / double click";
            // 
            // lblArgs
            // 
            lblArgs.Name = "lblArgs";
            lblArgs.Text = "Args:";
            // 
            // argstext
            // 
            argstext.Name = "argstext";
            argstext.Size = new System.Drawing.Size(90, 23);
            argstext.ToolTipText = "{1} = selected item id, e.g. 0x{1:X4}";
            // 
            // SendOnClick
            // 
            SendOnClick.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            SendOnClick.Name = "SendOnClick";
            SendOnClick.Text = "Send on double click";
            SendOnClick.ToolTipText = "Double click in the Items tab sends the item";
            SendOnClick.CheckOnClick = true;
            // 
            // sepTop1
            // 
            sepTop1.Name = "sepTop1";
            // 
            // lblDelay
            // 
            lblDelay.Name = "lblDelay";
            lblDelay.Text = "Delay (ms):";
            // 
            // txtDelay
            // 
            txtDelay.Name = "txtDelay";
            txtDelay.Size = new System.Drawing.Size(45, 23);
            txtDelay.ToolTipText = "Pause between several command lines";
            // 
            // BtnFocusClient
            // 
            BtnFocusClient.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnFocusClient.Name = "BtnFocusClient";
            BtnFocusClient.Text = "Focus client after send";
            BtnFocusClient.ToolTipText = "Bring the game client to the front after sending, so you can target right away";
            BtnFocusClient.CheckOnClick = true;
            // 
            // sepTop2
            // 
            sepTop2.Name = "sepTop2";
            // 
            // BtnImport
            // 
            BtnImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnImport.Name = "BtnImport";
            BtnImport.Text = "Import…";
            BtnImport.ToolTipText = "Import tabs and buttons from a JSON file";
            BtnImport.Click += BtnImport_Click;
            // 
            // BtnExport
            // 
            BtnExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnExport.Name = "BtnExport";
            BtnExport.Text = "Export…";
            BtnExport.ToolTipText = "Export all tabs and buttons to a JSON file";
            BtnExport.Click += BtnExport_Click;
            // 
            // BtnHelp
            // 
            BtnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnHelp.Name = "BtnHelp";
            BtnHelp.Text = "?";
            BtnHelp.ToolTipText = "Help: placeholders and shortcuts";
            BtnHelp.Click += BtnHelp_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblClient, lblStatus, lblCount });
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1200, 24);
            statusStrip.TabIndex = 2;
            // 
            // lblClient
            // 
            lblClient.Name = "lblClient";
            lblClient.Text = "● Client: ?";
            lblClient.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            lblClient.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Spring = true;
            lblStatus.Text = "Ready";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCount
            // 
            lblCount.Name = "lblCount";
            lblCount.Text = "";
            // 
            // splitMain
            // 
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitMain.Location = new System.Drawing.Point(0, 27);
            splitMain.Name = "splitMain";
            splitMain.Panel1.Controls.Add(tableLeft);
            splitMain.Panel1MinSize = 380;
            splitMain.Panel2.Controls.Add(tabButtons);
            splitMain.Panel2.Controls.Add(groupHistory);
            splitMain.Panel2.Controls.Add(toolStripTabs);
            splitMain.Panel2MinSize = 300;
            splitMain.Size = new System.Drawing.Size(1200, 729);
            splitMain.SplitterDistance = 560;
            splitMain.SplitterWidth = 6;
            splitMain.TabIndex = 1;
            // 
            // tableLeft
            // 
            tableLeft.ColumnCount = 1;
            tableLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLeft.Controls.Add(tableSearch, 0, 0);
            tableLeft.Controls.Add(staticGrid, 0, 1);
            tableLeft.Controls.Add(panelPreview, 0, 2);
            tableLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLeft.Name = "tableLeft";
            tableLeft.Padding = new System.Windows.Forms.Padding(6, 6, 2, 6);
            tableLeft.RowCount = 3;
            tableLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 214F));
            tableLeft.Size = new System.Drawing.Size(560, 729);
            tableLeft.TabIndex = 0;
            // 
            // tableSearch
            // 
            tableSearch.AutoSize = true;
            tableSearch.ColumnCount = 3;
            tableSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            tableSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            tableSearch.Controls.Add(textSearch, 0, 0);
            tableSearch.Controls.Add(comboFlag, 1, 0);
            tableSearch.Controls.Add(comboSize, 2, 0);
            tableSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            tableSearch.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            tableSearch.Name = "tableSearch";
            tableSearch.RowCount = 1;
            tableSearch.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableSearch.TabIndex = 0;
            // 
            // textSearch
            // 
            textSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            textSearch.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            textSearch.Name = "textSearch";
            textSearch.PlaceholderText = "Search: name, 0x1234, 4660 or range 0x1000-0x10FF  (Ctrl+F)";
            textSearch.TabIndex = 0;
            textSearch.TextChanged += TextSearch_TextChanged;
            textSearch.KeyDown += TextSearch_KeyDown;
            // 
            // comboFlag
            // 
            comboFlag.Dock = System.Windows.Forms.DockStyle.Fill;
            comboFlag.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboFlag.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            comboFlag.Name = "comboFlag";
            toolTip.SetToolTip(comboFlag, "Filter by tiledata flag");
            comboFlag.SelectedIndexChanged += ComboFlag_SelectedIndexChanged;
            // 
            // comboSize
            // 
            comboSize.Dock = System.Windows.Forms.DockStyle.Fill;
            comboSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboSize.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            comboSize.Name = "comboSize";
            toolTip.SetToolTip(comboSize, "Thumbnail size");
            comboSize.SelectedIndexChanged += ComboSize_SelectedIndexChanged;
            comboSize.Items.AddRange(new object[] { "48", "64", "80", "96", "128" });
            // 
            // staticGrid
            // 
            staticGrid.ContextMenuStrip = contextGrid;
            staticGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            staticGrid.Margin = new System.Windows.Forms.Padding(0);
            staticGrid.Name = "staticGrid";
            staticGrid.TabIndex = 1;
            staticGrid.SelectionChanged += StaticGrid_SelectionChanged;
            staticGrid.ItemActivated += StaticGrid_ItemActivated;
            // 
            // panelPreview
            // 
            panelPreview.Controls.Add(pictureStatic);
            panelPreview.Controls.Add(lblStaticInfo);
            panelPreview.Controls.Add(lblHue);
            panelPreview.Controls.Add(numHue);
            panelPreview.Controls.Add(lblTemplate);
            panelPreview.Controls.Add(comboTemplate);
            panelPreview.Controls.Add(txtCommandPreview);
            panelPreview.Controls.Add(BtnPlace);
            panelPreview.Controls.Add(BtnAddToTab);
            panelPreview.Controls.Add(BtnFromItems);
            panelPreview.Controls.Add(BtnCopyId);
            panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            panelPreview.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            panelPreview.Name = "panelPreview";
            panelPreview.TabIndex = 2;
            // 
            // pictureStatic
            // 
            pictureStatic.BackColor = System.Drawing.Color.FromArgb(40, 42, 48);
            pictureStatic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureStatic.Location = new System.Drawing.Point(0, 4);
            pictureStatic.Name = "pictureStatic";
            pictureStatic.Size = new System.Drawing.Size(160, 200);
            pictureStatic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureStatic.TabStop = false;
            // 
            // lblStaticInfo
            // 
            lblStaticInfo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStaticInfo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStaticInfo.Location = new System.Drawing.Point(170, 4);
            lblStaticInfo.Name = "lblStaticInfo";
            lblStaticInfo.Size = new System.Drawing.Size(380, 58);
            lblStaticInfo.Text = "Select a static";
            // 
            // lblHue
            // 
            lblHue.AutoSize = true;
            lblHue.Location = new System.Drawing.Point(170, 70);
            lblHue.Name = "lblHue";
            lblHue.Text = "Hue:";
            // 
            // numHue
            // 
            numHue.Location = new System.Drawing.Point(232, 67);
            numHue.Maximum = new decimal(new int[] { 3000, 0, 0, 0 });
            numHue.Name = "numHue";
            numHue.Size = new System.Drawing.Size(80, 23);
            numHue.TabIndex = 3;
            toolTip.SetToolTip(numHue, "Hue for the placed static (0 = original colors)");
            numHue.ValueChanged += NumHue_ValueChanged;
            // 
            // lblTemplate
            // 
            lblTemplate.AutoSize = true;
            lblTemplate.Location = new System.Drawing.Point(170, 101);
            lblTemplate.Name = "lblTemplate";
            lblTemplate.Text = "Template:";
            // 
            // comboTemplate
            // 
            comboTemplate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboTemplate.Location = new System.Drawing.Point(232, 98);
            comboTemplate.Name = "comboTemplate";
            comboTemplate.Size = new System.Drawing.Size(318, 23);
            comboTemplate.TabIndex = 4;
            toolTip.SetToolTip(comboTemplate, "Command used to place a static. {hex} {id} {name} {hue} {sethue}");
            comboTemplate.TextChanged += ComboTemplate_TextChanged;
            // 
            // txtCommandPreview
            // 
            txtCommandPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtCommandPreview.BackColor = System.Drawing.Color.FromArgb(24, 26, 31);
            txtCommandPreview.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtCommandPreview.ForeColor = System.Drawing.Color.FromArgb(98, 210, 130);
            txtCommandPreview.Location = new System.Drawing.Point(170, 130);
            txtCommandPreview.Name = "txtCommandPreview";
            txtCommandPreview.ReadOnly = true;
            txtCommandPreview.Size = new System.Drawing.Size(380, 23);
            txtCommandPreview.TabStop = false;
            toolTip.SetToolTip(txtCommandPreview, "Exactly this text is sent to the client");
            // 
            // BtnPlace
            // 
            BtnPlace.Location = new System.Drawing.Point(170, 164);
            BtnPlace.Name = "BtnPlace";
            BtnPlace.Size = new System.Drawing.Size(120, 30);
            BtnPlace.Text = "▶ Place in game";
            BtnPlace.UseVisualStyleBackColor = true;
            BtnPlace.Click += BtnPlace_Click;
            toolTip.SetToolTip(BtnPlace, "Send the place command to the client (Enter / double click)");
            // 
            // BtnAddToTab
            // 
            BtnAddToTab.Location = new System.Drawing.Point(294, 164);
            BtnAddToTab.Name = "BtnAddToTab";
            BtnAddToTab.Size = new System.Drawing.Size(95, 30);
            BtnAddToTab.Text = "+ Add to tab";
            BtnAddToTab.UseVisualStyleBackColor = true;
            BtnAddToTab.Click += BtnAddToTab_Click;
            toolTip.SetToolTip(BtnAddToTab, "Add the selected statics as buttons to the current tab");
            // 
            // BtnFromItems
            // 
            BtnFromItems.Location = new System.Drawing.Point(393, 164);
            BtnFromItems.Name = "BtnFromItems";
            BtnFromItems.Size = new System.Drawing.Size(100, 30);
            BtnFromItems.Text = "From Items tab";
            BtnFromItems.UseVisualStyleBackColor = true;
            BtnFromItems.Click += BtnFromItems_Click;
            toolTip.SetToolTip(BtnFromItems, "Take the item selected in UoFiddler's Items tab");
            // 
            // BtnCopyId
            // 
            BtnCopyId.Location = new System.Drawing.Point(497, 164);
            BtnCopyId.Name = "BtnCopyId";
            BtnCopyId.Size = new System.Drawing.Size(65, 30);
            BtnCopyId.Text = "Copy ID";
            BtnCopyId.UseVisualStyleBackColor = true;
            BtnCopyId.Click += BtnCopyId_Click;
            BtnPlace.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            // 
            // toolStripTabs
            // 
            toolStripTabs.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripTabs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { BtnNewTab, BtnRenameTab, BtnDeleteTab, BtnTabLeft, BtnTabRight, sepTabs1, BtnNewCommand, BtnAddStatics, sepTabs2, lblTabHint });
            toolStripTabs.Name = "toolStripTabs";
            toolStripTabs.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            toolStripTabs.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // BtnNewTab
            // 
            BtnNewTab.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnNewTab.Name = "BtnNewTab";
            BtnNewTab.Text = "New tab";
            BtnNewTab.ToolTipText = "Add a category tab";
            BtnNewTab.Click += BtnNewTab_Click;
            // 
            // BtnRenameTab
            // 
            BtnRenameTab.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnRenameTab.Name = "BtnRenameTab";
            BtnRenameTab.Text = "Rename";
            BtnRenameTab.Click += BtnRenameTab_Click;
            // 
            // BtnDeleteTab
            // 
            BtnDeleteTab.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnDeleteTab.Name = "BtnDeleteTab";
            BtnDeleteTab.Text = "Delete tab";
            BtnDeleteTab.Click += BtnDeleteTab_Click;
            // 
            // BtnTabLeft
            // 
            BtnTabLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnTabLeft.Name = "BtnTabLeft";
            BtnTabLeft.Text = "◀";
            BtnTabLeft.ToolTipText = "Move tab left";
            BtnTabLeft.Click += BtnTabLeft_Click;
            // 
            // BtnTabRight
            // 
            BtnTabRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnTabRight.Name = "BtnTabRight";
            BtnTabRight.Text = "▶";
            BtnTabRight.ToolTipText = "Move tab right";
            BtnTabRight.Click += BtnTabRight_Click;
            // 
            // sepTabs1
            // 
            sepTabs1.Name = "sepTabs1";
            // 
            // BtnNewCommand
            // 
            BtnNewCommand.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnNewCommand.Name = "BtnNewCommand";
            BtnNewCommand.Text = "+ Command button";
            BtnNewCommand.ToolTipText = "Save one or more commands as a button";
            BtnNewCommand.Click += BtnNewCommand_Click;
            // 
            // BtnAddStatics
            // 
            BtnAddStatics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnAddStatics.Name = "BtnAddStatics";
            BtnAddStatics.Text = "+ Selected statics";
            BtnAddStatics.ToolTipText = "Add the statics selected on the left as buttons";
            BtnAddStatics.Click += BtnAddStatics_Click;
            // 
            // sepTabs2
            // 
            sepTabs2.Name = "sepTabs2";
            // 
            // lblTabHint
            // 
            lblTabHint.Name = "lblTabHint";
            lblTabHint.ForeColor = System.Drawing.SystemColors.GrayText;
            lblTabHint.Text = "Tip: drag statics from the left onto a tab";
            // 
            // tabButtons
            // 
            tabButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            tabButtons.Name = "tabButtons";
            tabButtons.SelectedIndex = 0;
            tabButtons.TabIndex = 1;
            tabButtons.SelectedIndexChanged += TabButtons_SelectedIndexChanged;
            // 
            // groupHistory
            // 
            groupHistory.Controls.Add(listHistory);
            groupHistory.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupHistory.Name = "groupHistory";
            groupHistory.Padding = new System.Windows.Forms.Padding(6, 3, 6, 6);
            groupHistory.Size = new System.Drawing.Size(630, 150);
            groupHistory.TabIndex = 2;
            groupHistory.TabStop = false;
            groupHistory.Text = "Sent commands (double click = send again)";
            // 
            // listHistory
            // 
            listHistory.ContextMenuStrip = contextHistory;
            listHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            listHistory.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            listHistory.IntegralHeight = false;
            listHistory.Name = "listHistory";
            listHistory.TabIndex = 0;
            listHistory.DoubleClick += MenuHistorySend_Click;
            // 
            // contextButton
            // 
            contextButton.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuBtnSend, menuBtnEdit, menuBtnDuplicate, menuBtnMoveTo, menuBtnLeft, menuBtnRight, menuBtnSep, menuBtnDelete });
            contextButton.Name = "contextButton";
            contextButton.Opening += ContextButton_Opening;
            // 
            // menuBtnSend
            // 
            menuBtnSend.Name = "menuBtnSend";
            menuBtnSend.Text = "Send";
            menuBtnSend.Click += MenuBtnSend_Click;
            // 
            // menuBtnEdit
            // 
            menuBtnEdit.Name = "menuBtnEdit";
            menuBtnEdit.Text = "Edit…";
            menuBtnEdit.Click += MenuBtnEdit_Click;
            // 
            // menuBtnDuplicate
            // 
            menuBtnDuplicate.Name = "menuBtnDuplicate";
            menuBtnDuplicate.Text = "Duplicate";
            menuBtnDuplicate.Click += MenuBtnDuplicate_Click;
            // 
            // menuBtnMoveTo
            // 
            menuBtnMoveTo.Name = "menuBtnMoveTo";
            menuBtnMoveTo.Text = "Move to tab";
            // 
            // menuBtnLeft
            // 
            menuBtnLeft.Name = "menuBtnLeft";
            menuBtnLeft.Text = "Move left";
            menuBtnLeft.Click += MenuBtnLeft_Click;
            // 
            // menuBtnRight
            // 
            menuBtnRight.Name = "menuBtnRight";
            menuBtnRight.Text = "Move right";
            menuBtnRight.Click += MenuBtnRight_Click;
            // 
            // menuBtnSep
            // 
            menuBtnSep.Name = "menuBtnSep";
            // 
            // menuBtnDelete
            // 
            menuBtnDelete.Name = "menuBtnDelete";
            menuBtnDelete.Text = "Delete";
            menuBtnDelete.Click += MenuBtnDelete_Click;
            menuBtnSend.Font = new System.Drawing.Font(contextButton.Font, System.Drawing.FontStyle.Bold);
            // 
            // contextGrid
            // 
            contextGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuGridPlace, menuGridAddToTab, menuGridCopyId });
            contextGrid.Name = "contextGrid";
            contextGrid.Opening += ContextGrid_Opening;
            // 
            // menuGridPlace
            // 
            menuGridPlace.Name = "menuGridPlace";
            menuGridPlace.Text = "Place in game";
            menuGridPlace.Click += MenuGridPlace_Click;
            // 
            // menuGridAddToTab
            // 
            menuGridAddToTab.Name = "menuGridAddToTab";
            menuGridAddToTab.Text = "Add to current tab";
            menuGridAddToTab.Click += MenuGridAddToTab_Click;
            // 
            // menuGridCopyId
            // 
            menuGridCopyId.Name = "menuGridCopyId";
            menuGridCopyId.Text = "Copy ID";
            menuGridCopyId.Click += MenuGridCopyId_Click;
            menuGridPlace.Font = new System.Drawing.Font(contextGrid.Font, System.Drawing.FontStyle.Bold);
            // 
            // contextHistory
            // 
            contextHistory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuHistorySend, menuHistorySave, menuHistoryClear });
            contextHistory.Name = "contextHistory";
            // 
            // menuHistorySend
            // 
            menuHistorySend.Name = "menuHistorySend";
            menuHistorySend.Text = "Send again";
            menuHistorySend.Click += MenuHistorySend_Click;
            // 
            // menuHistorySave
            // 
            menuHistorySave.Name = "menuHistorySave";
            menuHistorySave.Text = "Save as button…";
            menuHistorySave.Click += MenuHistorySave_Click;
            // 
            // menuHistoryClear
            // 
            menuHistoryClear.Name = "menuHistoryClear";
            menuHistoryClear.Text = "Clear history";
            menuHistoryClear.Click += MenuHistoryClear_Click;
            // 
            // timerSearch
            // 
            timerSearch.Interval = 200;
            timerSearch.Tick += TimerSearch_Tick;
            // 
            // SendItemOptionsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1200, 780);
            Controls.Add(splitMain);
            Controls.Add(toolStripTop);
            Controls.Add(statusStrip);
            DoubleBuffered = true;
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(900, 560);
            Name = "SendItemOptionsForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "SendItem – Statics & Commands";
            ((System.ComponentModel.ISupportInitialize)numHue).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureStatic).EndInit();
            contextHistory.ResumeLayout(false);
            contextGrid.ResumeLayout(false);
            contextButton.ResumeLayout(false);
            groupHistory.ResumeLayout(false);
            panelPreview.ResumeLayout(false);
            tableSearch.ResumeLayout(false);
            tableLeft.ResumeLayout(false);
            panelPreview.PerformLayout();
            tableSearch.PerformLayout();
            tableLeft.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            splitMain.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            toolStripTabs.ResumeLayout(false);
            toolStripTabs.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            toolStripTop.ResumeLayout(false);
            toolStripTop.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripTop;
        private System.Windows.Forms.ToolStripLabel lblItemsCmd;
        private System.Windows.Forms.ToolStripTextBox cmdtext;
        private System.Windows.Forms.ToolStripLabel lblArgs;
        private System.Windows.Forms.ToolStripTextBox argstext;
        private System.Windows.Forms.ToolStripButton SendOnClick;
        private System.Windows.Forms.ToolStripSeparator sepTop1;
        private System.Windows.Forms.ToolStripLabel lblDelay;
        private System.Windows.Forms.ToolStripTextBox txtDelay;
        private System.Windows.Forms.ToolStripButton BtnFocusClient;
        private System.Windows.Forms.ToolStripSeparator sepTop2;
        private System.Windows.Forms.ToolStripButton BtnImport;
        private System.Windows.Forms.ToolStripButton BtnExport;
        private System.Windows.Forms.ToolStripButton BtnHelp;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblClient;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblCount;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TableLayoutPanel tableLeft;
        private System.Windows.Forms.TableLayoutPanel tableSearch;
        private System.Windows.Forms.TextBox textSearch;
        private System.Windows.Forms.ComboBox comboFlag;
        private System.Windows.Forms.ComboBox comboSize;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.PictureBox pictureStatic;
        private System.Windows.Forms.Label lblStaticInfo;
        private System.Windows.Forms.Label lblHue;
        private System.Windows.Forms.NumericUpDown numHue;
        private System.Windows.Forms.Label lblTemplate;
        private System.Windows.Forms.ComboBox comboTemplate;
        private System.Windows.Forms.TextBox txtCommandPreview;
        private System.Windows.Forms.Button BtnPlace;
        private System.Windows.Forms.Button BtnAddToTab;
        private System.Windows.Forms.Button BtnFromItems;
        private System.Windows.Forms.Button BtnCopyId;
        private System.Windows.Forms.ToolStrip toolStripTabs;
        private System.Windows.Forms.ToolStripButton BtnNewTab;
        private System.Windows.Forms.ToolStripButton BtnRenameTab;
        private System.Windows.Forms.ToolStripButton BtnDeleteTab;
        private System.Windows.Forms.ToolStripButton BtnTabLeft;
        private System.Windows.Forms.ToolStripButton BtnTabRight;
        private System.Windows.Forms.ToolStripSeparator sepTabs1;
        private System.Windows.Forms.ToolStripButton BtnNewCommand;
        private System.Windows.Forms.ToolStripButton BtnAddStatics;
        private System.Windows.Forms.ToolStripSeparator sepTabs2;
        private System.Windows.Forms.ToolStripLabel lblTabHint;
        private System.Windows.Forms.TabControl tabButtons;
        private System.Windows.Forms.GroupBox groupHistory;
        private System.Windows.Forms.ListBox listHistory;
        private StaticGrid staticGrid;
        private System.Windows.Forms.ContextMenuStrip contextButton;
        private System.Windows.Forms.ToolStripMenuItem menuBtnSend;
        private System.Windows.Forms.ToolStripMenuItem menuBtnEdit;
        private System.Windows.Forms.ToolStripMenuItem menuBtnDuplicate;
        private System.Windows.Forms.ToolStripMenuItem menuBtnMoveTo;
        private System.Windows.Forms.ToolStripMenuItem menuBtnLeft;
        private System.Windows.Forms.ToolStripMenuItem menuBtnRight;
        private System.Windows.Forms.ToolStripSeparator menuBtnSep;
        private System.Windows.Forms.ToolStripMenuItem menuBtnDelete;
        private System.Windows.Forms.ContextMenuStrip contextGrid;
        private System.Windows.Forms.ToolStripMenuItem menuGridPlace;
        private System.Windows.Forms.ToolStripMenuItem menuGridAddToTab;
        private System.Windows.Forms.ToolStripMenuItem menuGridCopyId;
        private System.Windows.Forms.ContextMenuStrip contextHistory;
        private System.Windows.Forms.ToolStripMenuItem menuHistorySend;
        private System.Windows.Forms.ToolStripMenuItem menuHistorySave;
        private System.Windows.Forms.ToolStripMenuItem menuHistoryClear;
        private System.Windows.Forms.Timer timerSearch;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
