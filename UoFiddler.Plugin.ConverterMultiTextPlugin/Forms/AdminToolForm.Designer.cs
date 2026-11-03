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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class AdminToolForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdminToolForm));
            panelHeader = new System.Windows.Forms.Panel();
            labelTitle = new System.Windows.Forms.Label();
            labelSubtitle = new System.Windows.Forms.Label();
            LabelInternetStatus = new System.Windows.Forms.Label();
            toolStripMain = new System.Windows.Forms.ToolStrip();
            toolLabelTarget = new System.Windows.Forms.ToolStripLabel();
            comboTarget = new System.Windows.Forms.ToolStripComboBox();
            toolSepMain1 = new System.Windows.Forms.ToolStripSeparator();
            BtnPing = new System.Windows.Forms.ToolStripButton();
            BtnPingContinuous = new System.Windows.Forms.ToolStripButton();
            BtnTracert = new System.Windows.Forms.ToolStripButton();
            BtnDns = new System.Windows.Forms.ToolStripButton();
            BtnPortCheck = new System.Windows.Forms.ToolStripButton();
            BtnPortScan = new System.Windows.Forms.ToolStripButton();
            BtnUoStatus = new System.Windows.Forms.ToolStripButton();
            toolSepMain2 = new System.Windows.Forms.ToolStripSeparator();
            BtnLanScan = new System.Windows.Forms.ToolStripButton();
            toolSepMain3 = new System.Windows.Forms.ToolStripSeparator();
            BtnStop = new System.Windows.Forms.ToolStripButton();
            toolStripOptions = new System.Windows.Forms.ToolStrip();
            toolLabelPort = new System.Windows.Forms.ToolStripLabel();
            txtPort = new System.Windows.Forms.ToolStripTextBox();
            toolLabelTimeout = new System.Windows.Forms.ToolStripLabel();
            txtTimeout = new System.Windows.Forms.ToolStripTextBox();
            toolLabelCount = new System.Windows.Forms.ToolStripLabel();
            txtCount = new System.Windows.Forms.ToolStripTextBox();
            toolLabelPortList = new System.Windows.Forms.ToolStripLabel();
            txtPortList = new System.Windows.Forms.ToolStripTextBox();
            toolLabelFamily = new System.Windows.Forms.ToolStripLabel();
            comboFamily = new System.Windows.Forms.ToolStripComboBox();
            toolSepOpt1 = new System.Windows.Forms.ToolStripSeparator();
            BtnTopMost = new System.Windows.Forms.ToolStripButton();
            BtnCheckOnOpen = new System.Windows.Forms.ToolStripButton();
            statusStrip = new System.Windows.Forms.StatusStrip();
            statusPublicIp = new System.Windows.Forms.ToolStripStatusLabel();
            statusState = new System.Windows.Forms.ToolStripStatusLabel();
            statusProgress = new System.Windows.Forms.ToolStripProgressBar();
            statusCount = new System.Windows.Forms.ToolStripStatusLabel();
            splitMain = new System.Windows.Forms.SplitContainer();
            tableBook = new System.Windows.Forms.TableLayoutPanel();
            labelBook = new System.Windows.Forms.Label();
            tableSearch = new System.Windows.Forms.TableLayoutPanel();
            textBoxSearch = new System.Windows.Forms.TextBox();
            comboGroup = new System.Windows.Forms.ComboBox();
            listServers = new System.Windows.Forms.ListView();
            colName = new System.Windows.Forms.ColumnHeader();
            colHost = new System.Windows.Forms.ColumnHeader();
            colIPv4 = new System.Windows.Forms.ColumnHeader();
            colIPv6 = new System.Windows.Forms.ColumnHeader();
            colPort = new System.Windows.Forms.ColumnHeader();
            colGroup = new System.Windows.Forms.ColumnHeader();
            colStatus = new System.Windows.Forms.ColumnHeader();
            colLatency = new System.Windows.Forms.ColumnHeader();
            colLastCheck = new System.Windows.Forms.ColumnHeader();
            flowBookButtons = new System.Windows.Forms.FlowLayoutPanel();
            BtnAdd = new System.Windows.Forms.Button();
            BtnAddFromTarget = new System.Windows.Forms.Button();
            BtnEdit = new System.Windows.Forms.Button();
            BtnDelete = new System.Windows.Forms.Button();
            BtnCheckAll = new System.Windows.Forms.Button();
            BtnImport = new System.Windows.Forms.Button();
            BtnExport = new System.Windows.Forms.Button();
            tabMain = new System.Windows.Forms.TabControl();
            tabOutput = new System.Windows.Forms.TabPage();
            richLog = new System.Windows.Forms.RichTextBox();
            toolStripLog = new System.Windows.Forms.ToolStrip();
            BtnClearLog = new System.Windows.Forms.ToolStripButton();
            BtnCopyLog = new System.Windows.Forms.ToolStripButton();
            BtnSaveLog = new System.Windows.Forms.ToolStripButton();
            toolSepLog1 = new System.Windows.Forms.ToolStripSeparator();
            toolLabelLast = new System.Windows.Forms.ToolStripLabel();
            labelIP = new System.Windows.Forms.ToolStripLabel();
            BtnCopyIP = new System.Windows.Forms.ToolStripButton();
            tabStats = new System.Windows.Forms.TabPage();
            latencyGraph = new LatencyGraph();
            tableStats = new System.Windows.Forms.TableLayoutPanel();
            lblStatSent = new System.Windows.Forms.Label();
            lblStatReceived = new System.Windows.Forms.Label();
            lblStatLoss = new System.Windows.Forms.Label();
            lblStatMin = new System.Windows.Forms.Label();
            lblStatAvg = new System.Windows.Forms.Label();
            lblStatMax = new System.Windows.Forms.Label();
            lblStatJitter = new System.Windows.Forms.Label();
            tabResults = new System.Windows.Forms.TabPage();
            listResults = new System.Windows.Forms.ListView();
            colResAddress = new System.Windows.Forms.ColumnHeader();
            colResHost = new System.Windows.Forms.ColumnHeader();
            colResPing = new System.Windows.Forms.ColumnHeader();
            colResPort = new System.Windows.Forms.ColumnHeader();
            colResInfo = new System.Windows.Forms.ColumnHeader();
            toolStripResults = new System.Windows.Forms.ToolStrip();
            BtnResultsSave = new System.Windows.Forms.ToolStripButton();
            BtnResultsUse = new System.Windows.Forms.ToolStripButton();
            BtnResultsClear = new System.Windows.Forms.ToolStripButton();
            tabNetwork = new System.Windows.Forms.TabPage();
            richNetwork = new System.Windows.Forms.RichTextBox();
            toolStripNetwork = new System.Windows.Forms.ToolStrip();
            BtnNetworkRefresh = new System.Windows.Forms.ToolStripButton();
            BtnCopyPublicIp = new System.Windows.Forms.ToolStripButton();
            contextServers = new System.Windows.Forms.ContextMenuStrip(components);
            menuUseTarget = new System.Windows.Forms.ToolStripMenuItem();
            menuPing = new System.Windows.Forms.ToolStripMenuItem();
            menuCheck = new System.Windows.Forms.ToolStripMenuItem();
            menuPortCheck = new System.Windows.Forms.ToolStripMenuItem();
            menuTracert = new System.Windows.Forms.ToolStripMenuItem();
            menuUoStatus = new System.Windows.Forms.ToolStripMenuItem();
            menuSep1 = new System.Windows.Forms.ToolStripSeparator();
            menuCopyHost = new System.Windows.Forms.ToolStripMenuItem();
            menuCopyIPv4 = new System.Windows.Forms.ToolStripMenuItem();
            menuCopyIPv6 = new System.Windows.Forms.ToolStripMenuItem();
            menuCopyAll = new System.Windows.Forms.ToolStripMenuItem();
            menuSep2 = new System.Windows.Forms.ToolStripSeparator();
            menuFavorite = new System.Windows.Forms.ToolStripMenuItem();
            menuEdit = new System.Windows.Forms.ToolStripMenuItem();
            menuDelete = new System.Windows.Forms.ToolStripMenuItem();
            timerSearch = new System.Windows.Forms.Timer(components);
            toolTip = new System.Windows.Forms.ToolTip(components);
            panelHeader.SuspendLayout();
            toolStripMain.SuspendLayout();
            toolStripOptions.SuspendLayout();
            statusStrip.SuspendLayout();
            toolStripLog.SuspendLayout();
            toolStripResults.SuspendLayout();
            toolStripNetwork.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            tableBook.SuspendLayout();
            tableSearch.SuspendLayout();
            flowBookButtons.SuspendLayout();
            tabMain.SuspendLayout();
            tabOutput.SuspendLayout();
            tabStats.SuspendLayout();
            tableStats.SuspendLayout();
            tabResults.SuspendLayout();
            tabNetwork.SuspendLayout();
            contextServers.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = System.Drawing.Color.FromArgb(32, 36, 44);
            panelHeader.Controls.Add(LabelInternetStatus);
            panelHeader.Controls.Add(labelSubtitle);
            panelHeader.Controls.Add(labelTitle);
            panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            panelHeader.Location = new System.Drawing.Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new System.Drawing.Size(1180, 56);
            panelHeader.TabIndex = 0;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            labelTitle.ForeColor = System.Drawing.Color.White;
            labelTitle.Location = new System.Drawing.Point(12, 5);
            labelTitle.Name = "labelTitle";
            labelTitle.Text = "Server Admin Tool";
            // 
            // labelSubtitle
            // 
            labelSubtitle.AutoSize = true;
            labelSubtitle.ForeColor = System.Drawing.Color.FromArgb(160, 170, 185);
            labelSubtitle.Location = new System.Drawing.Point(15, 33);
            labelSubtitle.Name = "labelSubtitle";
            labelSubtitle.Text = "Ping  ·  Traceroute  ·  DNS  ·  Port check / scan  ·  Shard status  ·  LAN discovery  ·  Address book (IPv4 / IPv6)";
            // 
            // LabelInternetStatus
            // 
            LabelInternetStatus.Dock = System.Windows.Forms.DockStyle.Right;
            LabelInternetStatus.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            LabelInternetStatus.ForeColor = System.Drawing.Color.Silver;
            LabelInternetStatus.Location = new System.Drawing.Point(880, 0);
            LabelInternetStatus.Name = "LabelInternetStatus";
            LabelInternetStatus.Padding = new System.Windows.Forms.Padding(0, 0, 14, 0);
            LabelInternetStatus.Size = new System.Drawing.Size(300, 56);
            LabelInternetStatus.Text = "● Internet: checking…";
            LabelInternetStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStripMain
            // 
            toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolLabelTarget, comboTarget, toolSepMain1, BtnPing, BtnPingContinuous, BtnTracert, BtnDns, BtnPortCheck, BtnPortScan, BtnUoStatus, toolSepMain2, BtnLanScan, toolSepMain3, BtnStop });
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            toolStripMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStripMain.Location = new System.Drawing.Point(0, 56);
            toolStripMain.Size = new System.Drawing.Size(1180, 30);
            toolStripMain.TabIndex = 1;
            toolStripMain.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            // 
            // toolLabelTarget
            // 
            toolLabelTarget.Name = "toolLabelTarget";
            toolLabelTarget.Text = "Target:";
            // 
            // comboTarget
            // 
            comboTarget.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            comboTarget.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            comboTarget.FlatStyle = System.Windows.Forms.FlatStyle.System;
            comboTarget.Name = "comboTarget";
            comboTarget.Size = new System.Drawing.Size(300, 26);
            comboTarget.ToolTipText = "Host name, IPv4 or IPv6 address. Also accepts host:port, [IPv6]:port or a URL. Enter = Ping";
            comboTarget.KeyDown += ComboTarget_KeyDown;
            // 
            // toolSepMain1
            // 
            toolSepMain1.Name = "toolSepMain1";
            // 
            // toolSepMain2
            // 
            toolSepMain2.Name = "toolSepMain2";
            // 
            // toolSepMain3
            // 
            toolSepMain3.Name = "toolSepMain3";
            // 
            // toolSepOpt1
            // 
            toolSepOpt1.Name = "toolSepOpt1";
            // 
            // toolSepLog1
            // 
            toolSepLog1.Name = "toolSepLog1";
            // 
            // BtnPing
            // 
            BtnPing.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnPing.Name = "BtnPing";
            BtnPing.Text = "Ping";
            BtnPing.ToolTipText = "Ping the target (count from the options bar)";
            BtnPing.Click += BtnPing_Click;
            // 
            // BtnPingContinuous
            // 
            BtnPingContinuous.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnPingContinuous.Name = "BtnPingContinuous";
            BtnPingContinuous.Text = "Ping ∞";
            BtnPingContinuous.ToolTipText = "Ping continuously until Stop";
            BtnPingContinuous.Click += BtnPingContinuous_Click;
            // 
            // BtnTracert
            // 
            BtnTracert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnTracert.Name = "BtnTracert";
            BtnTracert.Text = "Traceroute";
            BtnTracert.ToolTipText = "Trace the route to the target";
            BtnTracert.Click += BtnTracert_Click;
            // 
            // BtnDns
            // 
            BtnDns.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnDns.Name = "BtnDns";
            BtnDns.Text = "DNS lookup";
            BtnDns.ToolTipText = "Resolve all IPv4 / IPv6 addresses, or reverse lookup for an IP";
            BtnDns.Click += BtnDns_Click;
            // 
            // BtnPortCheck
            // 
            BtnPortCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnPortCheck.Name = "BtnPortCheck";
            BtnPortCheck.Text = "Port check";
            BtnPortCheck.ToolTipText = "Test if the TCP port is reachable (game server login port)";
            BtnPortCheck.Click += BtnPortCheck_Click;
            // 
            // BtnPortScan
            // 
            BtnPortScan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnPortScan.Name = "BtnPortScan";
            BtnPortScan.Text = "Port scan";
            BtnPortScan.ToolTipText = "Scan the port list from the options bar";
            BtnPortScan.Click += BtnPortScan_Click;
            // 
            // BtnUoStatus
            // 
            BtnUoStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnUoStatus.Name = "BtnUoStatus";
            BtnUoStatus.Text = "Shard status";
            BtnUoStatus.ToolTipText = "Experimental: ask a RunUO / ServUO shard for its status (clients, items, uptime)";
            BtnUoStatus.Click += BtnUoStatus_Click;
            // 
            // BtnLanScan
            // 
            BtnLanScan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnLanScan.Name = "BtnLanScan";
            BtnLanScan.Text = "Find servers (LAN)";
            BtnLanScan.ToolTipText = "Search the local network for hosts and open game server ports";
            BtnLanScan.Click += BtnLanScan_Click;
            // 
            // BtnStop
            // 
            BtnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnStop.Name = "BtnStop";
            BtnStop.Text = "■ Stop";
            BtnStop.ToolTipText = "Cancel the running operation (Esc)";
            BtnStop.Enabled = false;
            BtnStop.ForeColor = System.Drawing.Color.Firebrick;
            BtnStop.Click += BtnStop_Click;
            // 
            // toolStripOptions
            // 
            toolStripOptions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolLabelPort, txtPort, toolLabelTimeout, txtTimeout, toolLabelCount, txtCount, toolLabelPortList, txtPortList, toolLabelFamily, comboFamily, toolSepOpt1, BtnTopMost, BtnCheckOnOpen });
            toolStripOptions.Name = "toolStripOptions";
            toolStripOptions.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            toolStripOptions.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStripOptions.Location = new System.Drawing.Point(0, 86);
            toolStripOptions.Size = new System.Drawing.Size(1180, 27);
            toolStripOptions.TabIndex = 2;
            // 
            // toolLabelPort
            // 
            toolLabelPort.Name = "toolLabelPort";
            toolLabelPort.Text = "Port:";
            // 
            // toolLabelTimeout
            // 
            toolLabelTimeout.Name = "toolLabelTimeout";
            toolLabelTimeout.Text = "Timeout (ms):";
            // 
            // toolLabelCount
            // 
            toolLabelCount.Name = "toolLabelCount";
            toolLabelCount.Text = "Count:";
            // 
            // toolLabelPortList
            // 
            toolLabelPortList.Name = "toolLabelPortList";
            toolLabelPortList.Text = "Scan ports:";
            // 
            // toolLabelFamily
            // 
            toolLabelFamily.Name = "toolLabelFamily";
            toolLabelFamily.Text = "IP:";
            // 
            // toolLabelLast
            // 
            toolLabelLast.Name = "toolLabelLast";
            toolLabelLast.Text = "Last address:";
            // 
            // txtPort
            // 
            txtPort.Name = "txtPort";
            txtPort.Size = new System.Drawing.Size(55, 23);
            txtPort.ToolTipText = "Default TCP port (UO servers: 2593)";
            // 
            // txtTimeout
            // 
            txtTimeout.Name = "txtTimeout";
            txtTimeout.Size = new System.Drawing.Size(55, 23);
            txtTimeout.ToolTipText = "Timeout per request in milliseconds";
            // 
            // txtCount
            // 
            txtCount.Name = "txtCount";
            txtCount.Size = new System.Drawing.Size(40, 23);
            txtCount.ToolTipText = "Number of pings (0 = endless)";
            // 
            // txtPortList
            // 
            txtPortList.Name = "txtPortList";
            txtPortList.Size = new System.Drawing.Size(170, 23);
            txtPortList.ToolTipText = "Ports for the port scan, e.g. 2593-2600,80,443";
            // 
            // comboFamily
            // 
            comboFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboFamily.Items.AddRange(new object[] { "Auto", "IPv4", "IPv6" });
            comboFamily.Name = "comboFamily";
            comboFamily.Size = new System.Drawing.Size(70, 23);
            comboFamily.ToolTipText = "Which IP version to use when a host name has both";
            // 
            // BtnTopMost
            // 
            BtnTopMost.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnTopMost.Name = "BtnTopMost";
            BtnTopMost.Text = "Always on top";
            BtnTopMost.CheckOnClick = true;
            BtnTopMost.Click += BtnTopMost_Click;
            // 
            // BtnCheckOnOpen
            // 
            BtnCheckOnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnCheckOnOpen.Name = "BtnCheckOnOpen";
            BtnCheckOnOpen.Text = "Check all on open";
            BtnCheckOnOpen.ToolTipText = "Check every saved server when the tool opens";
            BtnCheckOnOpen.CheckOnClick = true;
            BtnCheckOnOpen.Click += BtnCheckOnOpen_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusPublicIp, statusState, statusProgress, statusCount });
            statusStrip.Location = new System.Drawing.Point(0, 738);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1180, 24);
            statusStrip.TabIndex = 4;
            // 
            // statusPublicIp
            // 
            statusPublicIp.Name = "statusPublicIp";
            statusPublicIp.Text = "Public IP: …";
            statusPublicIp.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            // 
            // statusState
            // 
            statusState.Name = "statusState";
            statusState.Spring = true;
            statusState.Text = "Ready";
            statusState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusProgress
            // 
            statusProgress.Name = "statusProgress";
            statusProgress.Size = new System.Drawing.Size(180, 18);
            statusProgress.Visible = false;
            // 
            // statusCount
            // 
            statusCount.Name = "statusCount";
            statusCount.Text = "0 servers";
            // 
            // splitMain
            // 
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitMain.Location = new System.Drawing.Point(0, 113);
            splitMain.Name = "splitMain";
            splitMain.Panel1.Controls.Add(tableBook);
            splitMain.Panel1MinSize = 320;
            splitMain.Panel2.Controls.Add(tabMain);
            splitMain.Panel2MinSize = 320;
            splitMain.Size = new System.Drawing.Size(1180, 625);
            splitMain.SplitterDistance = 500;
            splitMain.SplitterWidth = 6;
            splitMain.TabIndex = 3;
            // 
            // tableBook
            // 
            tableBook.ColumnCount = 1;
            tableBook.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableBook.Controls.Add(labelBook, 0, 0);
            tableBook.Controls.Add(tableSearch, 0, 1);
            tableBook.Controls.Add(listServers, 0, 2);
            tableBook.Controls.Add(flowBookButtons, 0, 3);
            tableBook.Dock = System.Windows.Forms.DockStyle.Fill;
            tableBook.Name = "tableBook";
            tableBook.Padding = new System.Windows.Forms.Padding(8, 6, 2, 6);
            tableBook.RowCount = 4;
            tableBook.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableBook.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableBook.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableBook.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableBook.Size = new System.Drawing.Size(500, 625);
            tableBook.TabIndex = 0;
            // 
            // labelBook
            // 
            labelBook.AutoSize = true;
            labelBook.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            labelBook.Margin = new System.Windows.Forms.Padding(0, 2, 0, 6);
            labelBook.Name = "labelBook";
            labelBook.Text = "Saved servers";
            // 
            // tableSearch
            // 
            tableSearch.AutoSize = true;
            tableSearch.ColumnCount = 2;
            tableSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65F));
            tableSearch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            tableSearch.Controls.Add(textBoxSearch, 0, 0);
            tableSearch.Controls.Add(comboGroup, 1, 0);
            tableSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            tableSearch.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            tableSearch.Name = "tableSearch";
            tableSearch.RowCount = 1;
            tableSearch.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableSearch.TabIndex = 1;
            // 
            // textBoxSearch
            // 
            textBoxSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxSearch.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.PlaceholderText = "Search name, host, IP, port, group, notes…  (Ctrl+F)";
            textBoxSearch.TabIndex = 0;
            textBoxSearch.TextChanged += TextBoxSearch_TextChanged;
            textBoxSearch.KeyDown += TextBoxSearch_KeyDown;
            // 
            // comboGroup
            // 
            comboGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            comboGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboGroup.Margin = new System.Windows.Forms.Padding(0);
            comboGroup.Name = "comboGroup";
            comboGroup.TabIndex = 1;
            comboGroup.SelectedIndexChanged += ComboGroup_SelectedIndexChanged;
            // 
            // listServers
            // 
            listServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colName, colHost, colIPv4, colIPv6, colPort, colGroup, colStatus, colLatency, colLastCheck });
            listServers.ContextMenuStrip = contextServers;
            listServers.Dock = System.Windows.Forms.DockStyle.Fill;
            listServers.FullRowSelect = true;
            listServers.GridLines = true;
            listServers.HideSelection = false;
            listServers.Margin = new System.Windows.Forms.Padding(0);
            listServers.Name = "listServers";
            listServers.ShowItemToolTips = true;
            listServers.TabIndex = 2;
            listServers.UseCompatibleStateImageBehavior = false;
            listServers.View = System.Windows.Forms.View.Details;
            listServers.ColumnClick += ListServers_ColumnClick;
            listServers.DoubleClick += ListServers_DoubleClick;
            listServers.KeyDown += ListServers_KeyDown;
            listServers.SelectedIndexChanged += ListServers_SelectedIndexChanged;
            // 
            // colName
            // 
            colName.Text = "Name";
            colName.Width = 130;
            // 
            // colHost
            // 
            colHost.Text = "Host";
            colHost.Width = 130;
            // 
            // colIPv4
            // 
            colIPv4.Text = "IPv4";
            colIPv4.Width = 105;
            // 
            // colIPv6
            // 
            colIPv6.Text = "IPv6";
            colIPv6.Width = 130;
            // 
            // colPort
            // 
            colPort.Text = "Port";
            colPort.Width = 50;
            // 
            // colGroup
            // 
            colGroup.Text = "Group";
            colGroup.Width = 80;
            // 
            // colStatus
            // 
            colStatus.Text = "Status";
            colStatus.Width = 90;
            // 
            // colLatency
            // 
            colLatency.Text = "Ping";
            colLatency.Width = 55;
            // 
            // colLastCheck
            // 
            colLastCheck.Text = "Last check";
            colLastCheck.Width = 110;
            // 
            // flowBookButtons
            // 
            flowBookButtons.AutoSize = true;
            flowBookButtons.Controls.Add(BtnAdd);
            flowBookButtons.Controls.Add(BtnAddFromTarget);
            flowBookButtons.Controls.Add(BtnEdit);
            flowBookButtons.Controls.Add(BtnDelete);
            flowBookButtons.Controls.Add(BtnCheckAll);
            flowBookButtons.Controls.Add(BtnImport);
            flowBookButtons.Controls.Add(BtnExport);
            flowBookButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            flowBookButtons.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            flowBookButtons.Name = "flowBookButtons";
            flowBookButtons.TabIndex = 3;
            // 
            // BtnAdd
            // 
            BtnAdd.AutoSize = true;
            BtnAdd.Margin = new System.Windows.Forms.Padding(0, 0, 4, 4);
            BtnAdd.Name = "BtnAdd";
            BtnAdd.TabIndex = 0;
            BtnAdd.Text = "Add…";
            toolTip.SetToolTip(BtnAdd, "Add a new server");
            BtnAdd.UseVisualStyleBackColor = true;
            BtnAdd.Click += BtnAdd_Click;
            // 
            // BtnAddFromTarget
            // 
            BtnAddFromTarget.AutoSize = true;
            BtnAddFromTarget.Margin = new System.Windows.Forms.Padding(0, 0, 4, 4);
            BtnAddFromTarget.Name = "BtnAddFromTarget";
            BtnAddFromTarget.TabIndex = 1;
            BtnAddFromTarget.Text = "Save target";
            toolTip.SetToolTip(BtnAddFromTarget, "Save the current target to the address book (resolves IPv4/IPv6)");
            BtnAddFromTarget.UseVisualStyleBackColor = true;
            BtnAddFromTarget.Click += BtnAddFromTarget_Click;
            // 
            // BtnEdit
            // 
            BtnEdit.AutoSize = true;
            BtnEdit.Margin = new System.Windows.Forms.Padding(0, 0, 4, 4);
            BtnEdit.Name = "BtnEdit";
            BtnEdit.TabIndex = 2;
            BtnEdit.Text = "Edit…";
            BtnEdit.UseVisualStyleBackColor = true;
            BtnEdit.Click += BtnEdit_Click;
            // 
            // BtnDelete
            // 
            BtnDelete.AutoSize = true;
            BtnDelete.Margin = new System.Windows.Forms.Padding(0, 0, 4, 4);
            BtnDelete.Name = "BtnDelete";
            BtnDelete.TabIndex = 3;
            BtnDelete.Text = "Delete";
            BtnDelete.UseVisualStyleBackColor = true;
            BtnDelete.Click += BtnDelete_Click;
            // 
            // BtnCheckAll
            // 
            BtnCheckAll.AutoSize = true;
            BtnCheckAll.Margin = new System.Windows.Forms.Padding(0, 0, 4, 4);
            BtnCheckAll.Name = "BtnCheckAll";
            BtnCheckAll.TabIndex = 4;
            BtnCheckAll.Text = "Check all";
            toolTip.SetToolTip(BtnCheckAll, "Ping + port check every visible server in parallel");
            BtnCheckAll.UseVisualStyleBackColor = true;
            BtnCheckAll.Click += BtnCheckAll_Click;
            // 
            // BtnImport
            // 
            BtnImport.AutoSize = true;
            BtnImport.Margin = new System.Windows.Forms.Padding(0, 0, 4, 4);
            BtnImport.Name = "BtnImport";
            BtnImport.TabIndex = 5;
            BtnImport.Text = "Import…";
            toolTip.SetToolTip(BtnImport, "Import servers from JSON or CSV");
            BtnImport.UseVisualStyleBackColor = true;
            BtnImport.Click += BtnImport_Click;
            // 
            // BtnExport
            // 
            BtnExport.AutoSize = true;
            BtnExport.Margin = new System.Windows.Forms.Padding(0, 0, 4, 4);
            BtnExport.Name = "BtnExport";
            BtnExport.TabIndex = 6;
            BtnExport.Text = "Export…";
            toolTip.SetToolTip(BtnExport, "Export servers to JSON or CSV");
            BtnExport.UseVisualStyleBackColor = true;
            BtnExport.Click += BtnExport_Click;
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabOutput);
            tabMain.Controls.Add(tabStats);
            tabMain.Controls.Add(tabResults);
            tabMain.Controls.Add(tabNetwork);
            tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.TabIndex = 0;
            // 
            // tabOutput
            // 
            tabOutput.Controls.Add(richLog);
            tabOutput.Controls.Add(toolStripLog);
            tabOutput.Name = "tabOutput";
            tabOutput.Padding = new System.Windows.Forms.Padding(0);
            tabOutput.Text = "Output";
            tabOutput.UseVisualStyleBackColor = true;
            // 
            // richLog
            // 
            richLog.BackColor = System.Drawing.Color.FromArgb(24, 26, 31);
            richLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richLog.DetectUrls = false;
            richLog.Dock = System.Windows.Forms.DockStyle.Fill;
            richLog.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            richLog.ForeColor = System.Drawing.Color.FromArgb(220, 223, 228);
            richLog.Name = "richLog";
            richLog.ReadOnly = true;
            richLog.Text = "";
            richLog.WordWrap = false;
            // 
            // toolStripLog
            // 
            toolStripLog.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripLog.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { BtnClearLog, BtnCopyLog, BtnSaveLog, toolSepLog1, toolLabelLast, labelIP, BtnCopyIP });
            toolStripLog.Name = "toolStripLog";
            toolStripLog.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            toolStripLog.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // BtnClearLog
            // 
            BtnClearLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnClearLog.Name = "BtnClearLog";
            BtnClearLog.Text = "Clear";
            BtnClearLog.Click += BtnClearLog_Click;
            // 
            // BtnCopyLog
            // 
            BtnCopyLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnCopyLog.Name = "BtnCopyLog";
            BtnCopyLog.Text = "Copy";
            BtnCopyLog.Click += BtnCopyLog_Click;
            // 
            // BtnSaveLog
            // 
            BtnSaveLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnSaveLog.Name = "BtnSaveLog";
            BtnSaveLog.Text = "Save…";
            BtnSaveLog.ToolTipText = "Save the output to a text file";
            BtnSaveLog.Click += BtnSaveLog_Click;
            // 
            // labelIP
            // 
            labelIP.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            labelIP.Name = "labelIP";
            labelIP.Text = "-";
            // 
            // BtnCopyIP
            // 
            BtnCopyIP.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnCopyIP.Name = "BtnCopyIP";
            BtnCopyIP.Text = "Copy address";
            BtnCopyIP.ToolTipText = "Copy the last resolved address to the clipboard";
            BtnCopyIP.Click += BtnCopyIP_Click;
            // 
            // tabStats
            // 
            tabStats.Controls.Add(latencyGraph);
            tabStats.Controls.Add(tableStats);
            tabStats.Name = "tabStats";
            tabStats.Padding = new System.Windows.Forms.Padding(0);
            tabStats.Text = "Statistics";
            tabStats.UseVisualStyleBackColor = true;
            // 
            // tableStats
            // 
            tableStats.BackColor = System.Drawing.Color.FromArgb(32, 36, 44);
            tableStats.ColumnCount = 7;
            tableStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.29F));
            tableStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.29F));
            tableStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.29F));
            tableStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.29F));
            tableStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.29F));
            tableStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.29F));
            tableStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.29F));
            tableStats.Controls.Add(lblStatSent, 0, 0);
            tableStats.Controls.Add(lblStatReceived, 1, 0);
            tableStats.Controls.Add(lblStatLoss, 2, 0);
            tableStats.Controls.Add(lblStatMin, 3, 0);
            tableStats.Controls.Add(lblStatAvg, 4, 0);
            tableStats.Controls.Add(lblStatMax, 5, 0);
            tableStats.Controls.Add(lblStatJitter, 6, 0);
            tableStats.Dock = System.Windows.Forms.DockStyle.Top;
            tableStats.Name = "tableStats";
            tableStats.RowCount = 1;
            tableStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableStats.Size = new System.Drawing.Size(660, 64);
            // 
            // lblStatSent
            // 
            lblStatSent.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatSent.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStatSent.ForeColor = System.Drawing.Color.White;
            lblStatSent.Name = "lblStatSent";
            lblStatSent.Text = "-";
            lblStatSent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatReceived
            // 
            lblStatReceived.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatReceived.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStatReceived.ForeColor = System.Drawing.Color.White;
            lblStatReceived.Name = "lblStatReceived";
            lblStatReceived.Text = "-";
            lblStatReceived.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatLoss
            // 
            lblStatLoss.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatLoss.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStatLoss.ForeColor = System.Drawing.Color.White;
            lblStatLoss.Name = "lblStatLoss";
            lblStatLoss.Text = "-";
            lblStatLoss.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatMin
            // 
            lblStatMin.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatMin.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStatMin.ForeColor = System.Drawing.Color.White;
            lblStatMin.Name = "lblStatMin";
            lblStatMin.Text = "-";
            lblStatMin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatAvg
            // 
            lblStatAvg.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatAvg.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStatAvg.ForeColor = System.Drawing.Color.White;
            lblStatAvg.Name = "lblStatAvg";
            lblStatAvg.Text = "-";
            lblStatAvg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatMax
            // 
            lblStatMax.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatMax.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStatMax.ForeColor = System.Drawing.Color.White;
            lblStatMax.Name = "lblStatMax";
            lblStatMax.Text = "-";
            lblStatMax.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStatJitter
            // 
            lblStatJitter.Dock = System.Windows.Forms.DockStyle.Fill;
            lblStatJitter.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblStatJitter.ForeColor = System.Drawing.Color.White;
            lblStatJitter.Name = "lblStatJitter";
            lblStatJitter.Text = "-";
            lblStatJitter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // latencyGraph
            // 
            latencyGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            latencyGraph.Name = "latencyGraph";
            // 
            // tabResults
            // 
            tabResults.Controls.Add(listResults);
            tabResults.Controls.Add(toolStripResults);
            tabResults.Name = "tabResults";
            tabResults.Padding = new System.Windows.Forms.Padding(0);
            tabResults.Text = "Scan results";
            tabResults.UseVisualStyleBackColor = true;
            // 
            // listResults
            // 
            listResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colResAddress, colResHost, colResPing, colResPort, colResInfo });
            listResults.Dock = System.Windows.Forms.DockStyle.Fill;
            listResults.FullRowSelect = true;
            listResults.GridLines = true;
            listResults.HideSelection = false;
            listResults.Name = "listResults";
            listResults.UseCompatibleStateImageBehavior = false;
            listResults.View = System.Windows.Forms.View.Details;
            listResults.DoubleClick += BtnResultsUse_Click;
            // 
            // colResAddress
            // 
            colResAddress.Text = "Address";
            colResAddress.Width = 150;
            // 
            // colResHost
            // 
            colResHost.Text = "Host name";
            colResHost.Width = 180;
            // 
            // colResPing
            // 
            colResPing.Text = "Ping";
            colResPing.Width = 60;
            // 
            // colResPort
            // 
            colResPort.Text = "Port";
            colResPort.Width = 110;
            // 
            // colResInfo
            // 
            colResInfo.Text = "Info";
            colResInfo.Width = 260;
            // 
            // toolStripResults
            // 
            toolStripResults.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripResults.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { BtnResultsSave, BtnResultsUse, BtnResultsClear });
            toolStripResults.Name = "toolStripResults";
            toolStripResults.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            toolStripResults.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // BtnResultsSave
            // 
            BtnResultsSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnResultsSave.Name = "BtnResultsSave";
            BtnResultsSave.Text = "Save selected to address book";
            BtnResultsSave.Click += BtnResultsSave_Click;
            // 
            // BtnResultsUse
            // 
            BtnResultsUse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnResultsUse.Name = "BtnResultsUse";
            BtnResultsUse.Text = "Use as target";
            BtnResultsUse.Click += BtnResultsUse_Click;
            // 
            // BtnResultsClear
            // 
            BtnResultsClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnResultsClear.Name = "BtnResultsClear";
            BtnResultsClear.Text = "Clear";
            BtnResultsClear.Click += BtnResultsClear_Click;
            // 
            // tabNetwork
            // 
            tabNetwork.Controls.Add(richNetwork);
            tabNetwork.Controls.Add(toolStripNetwork);
            tabNetwork.Name = "tabNetwork";
            tabNetwork.Padding = new System.Windows.Forms.Padding(0);
            tabNetwork.Text = "My network";
            tabNetwork.UseVisualStyleBackColor = true;
            // 
            // richNetwork
            // 
            richNetwork.BackColor = System.Drawing.Color.FromArgb(24, 26, 31);
            richNetwork.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richNetwork.DetectUrls = false;
            richNetwork.Dock = System.Windows.Forms.DockStyle.Fill;
            richNetwork.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            richNetwork.ForeColor = System.Drawing.Color.FromArgb(220, 223, 228);
            richNetwork.Name = "richNetwork";
            richNetwork.ReadOnly = true;
            richNetwork.Text = "";
            richNetwork.WordWrap = false;
            // 
            // toolStripNetwork
            // 
            toolStripNetwork.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripNetwork.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { BtnNetworkRefresh, BtnCopyPublicIp });
            toolStripNetwork.Name = "toolStripNetwork";
            toolStripNetwork.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            toolStripNetwork.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // BtnNetworkRefresh
            // 
            BtnNetworkRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnNetworkRefresh.Name = "BtnNetworkRefresh";
            BtnNetworkRefresh.Text = "Refresh";
            BtnNetworkRefresh.ToolTipText = "Reload adapters, local and public IPv4 / IPv6 addresses";
            BtnNetworkRefresh.Click += BtnNetworkRefresh_Click;
            // 
            // BtnCopyPublicIp
            // 
            BtnCopyPublicIp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            BtnCopyPublicIp.Name = "BtnCopyPublicIp";
            BtnCopyPublicIp.Text = "Copy public IP";
            BtnCopyPublicIp.Click += BtnCopyPublicIp_Click;
            // 
            // contextServers
            // 
            contextServers.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuUseTarget, menuPing, menuCheck, menuPortCheck, menuTracert, menuUoStatus, menuSep1, menuCopyHost, menuCopyIPv4, menuCopyIPv6, menuCopyAll, menuSep2, menuFavorite, menuEdit, menuDelete });
            contextServers.Name = "contextServers";
            contextServers.Opening += ContextServers_Opening;
            // 
            // menuUseTarget
            // 
            menuUseTarget.Name = "menuUseTarget";
            menuUseTarget.Text = "Use as target";
            menuUseTarget.Click += MenuUseTarget_Click;
            // 
            // menuPing
            // 
            menuPing.Name = "menuPing";
            menuPing.Text = "Ping";
            menuPing.Click += MenuPing_Click;
            // 
            // menuCheck
            // 
            menuCheck.Name = "menuCheck";
            menuCheck.Text = "Check status";
            menuCheck.Click += MenuCheck_Click;
            // 
            // menuPortCheck
            // 
            menuPortCheck.Name = "menuPortCheck";
            menuPortCheck.Text = "Port check";
            menuPortCheck.Click += MenuPortCheck_Click;
            // 
            // menuTracert
            // 
            menuTracert.Name = "menuTracert";
            menuTracert.Text = "Traceroute";
            menuTracert.Click += MenuTracert_Click;
            // 
            // menuUoStatus
            // 
            menuUoStatus.Name = "menuUoStatus";
            menuUoStatus.Text = "Shard status";
            menuUoStatus.Click += MenuUoStatus_Click;
            // 
            // menuSep1
            // 
            menuSep1.Name = "menuSep1";
            // 
            // menuCopyHost
            // 
            menuCopyHost.Name = "menuCopyHost";
            menuCopyHost.Text = "Copy host";
            menuCopyHost.Click += MenuCopyHost_Click;
            // 
            // menuCopyIPv4
            // 
            menuCopyIPv4.Name = "menuCopyIPv4";
            menuCopyIPv4.Text = "Copy IPv4";
            menuCopyIPv4.Click += MenuCopyIPv4_Click;
            // 
            // menuCopyIPv6
            // 
            menuCopyIPv6.Name = "menuCopyIPv6";
            menuCopyIPv6.Text = "Copy IPv6";
            menuCopyIPv6.Click += MenuCopyIPv6_Click;
            // 
            // menuCopyAll
            // 
            menuCopyAll.Name = "menuCopyAll";
            menuCopyAll.Text = "Copy as text";
            menuCopyAll.Click += MenuCopyAll_Click;
            // 
            // menuSep2
            // 
            menuSep2.Name = "menuSep2";
            // 
            // menuFavorite
            // 
            menuFavorite.Name = "menuFavorite";
            menuFavorite.Text = "Toggle favorite";
            menuFavorite.Click += MenuFavorite_Click;
            // 
            // menuEdit
            // 
            menuEdit.Name = "menuEdit";
            menuEdit.Text = "Edit…";
            menuEdit.Click += MenuEdit_Click;
            // 
            // menuDelete
            // 
            menuDelete.Name = "menuDelete";
            menuDelete.Text = "Delete";
            menuDelete.Click += MenuDelete_Click;
            menuUseTarget.Font = new System.Drawing.Font(contextServers.Font, System.Drawing.FontStyle.Bold);
            menuDelete.ShortcutKeyDisplayString = "Del";
            menuEdit.ShortcutKeyDisplayString = "F2";
            // 
            // timerSearch
            // 
            timerSearch.Interval = 250;
            timerSearch.Tick += TimerSearch_Tick;
            // 
            // AdminToolForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1180, 762);
            Controls.Add(splitMain);
            Controls.Add(toolStripOptions);
            Controls.Add(toolStripMain);
            Controls.Add(panelHeader);
            Controls.Add(statusStrip);
            DoubleBuffered = true;
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(900, 560);
            Name = "AdminToolForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "AdminTool - Server Admin";
            contextServers.ResumeLayout(false);
            tabNetwork.ResumeLayout(false);
            tabResults.ResumeLayout(false);
            tableStats.ResumeLayout(false);
            tabStats.ResumeLayout(false);
            tabOutput.ResumeLayout(false);
            tabMain.ResumeLayout(false);
            flowBookButtons.ResumeLayout(false);
            tableSearch.ResumeLayout(false);
            tableBook.ResumeLayout(false);
            tabNetwork.PerformLayout();
            tabResults.PerformLayout();
            tabOutput.PerformLayout();
            flowBookButtons.PerformLayout();
            tableSearch.PerformLayout();
            tableBook.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            toolStripNetwork.ResumeLayout(false);
            toolStripNetwork.PerformLayout();
            toolStripResults.ResumeLayout(false);
            toolStripResults.PerformLayout();
            toolStripLog.ResumeLayout(false);
            toolStripLog.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            toolStripOptions.ResumeLayout(false);
            toolStripOptions.PerformLayout();
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelSubtitle;
        private System.Windows.Forms.Label LabelInternetStatus;
        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripLabel toolLabelTarget;
        private System.Windows.Forms.ToolStripComboBox comboTarget;
        private System.Windows.Forms.ToolStripSeparator toolSepMain1;
        private System.Windows.Forms.ToolStripButton BtnPing;
        private System.Windows.Forms.ToolStripButton BtnPingContinuous;
        private System.Windows.Forms.ToolStripButton BtnTracert;
        private System.Windows.Forms.ToolStripButton BtnDns;
        private System.Windows.Forms.ToolStripButton BtnPortCheck;
        private System.Windows.Forms.ToolStripButton BtnPortScan;
        private System.Windows.Forms.ToolStripButton BtnUoStatus;
        private System.Windows.Forms.ToolStripSeparator toolSepMain2;
        private System.Windows.Forms.ToolStripButton BtnLanScan;
        private System.Windows.Forms.ToolStripSeparator toolSepMain3;
        private System.Windows.Forms.ToolStripButton BtnStop;
        private System.Windows.Forms.ToolStrip toolStripOptions;
        private System.Windows.Forms.ToolStripLabel toolLabelPort;
        private System.Windows.Forms.ToolStripTextBox txtPort;
        private System.Windows.Forms.ToolStripLabel toolLabelTimeout;
        private System.Windows.Forms.ToolStripTextBox txtTimeout;
        private System.Windows.Forms.ToolStripLabel toolLabelCount;
        private System.Windows.Forms.ToolStripTextBox txtCount;
        private System.Windows.Forms.ToolStripLabel toolLabelPortList;
        private System.Windows.Forms.ToolStripTextBox txtPortList;
        private System.Windows.Forms.ToolStripLabel toolLabelFamily;
        private System.Windows.Forms.ToolStripComboBox comboFamily;
        private System.Windows.Forms.ToolStripSeparator toolSepOpt1;
        private System.Windows.Forms.ToolStripButton BtnTopMost;
        private System.Windows.Forms.ToolStripButton BtnCheckOnOpen;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusPublicIp;
        private System.Windows.Forms.ToolStripStatusLabel statusState;
        private System.Windows.Forms.ToolStripProgressBar statusProgress;
        private System.Windows.Forms.ToolStripStatusLabel statusCount;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TableLayoutPanel tableBook;
        private System.Windows.Forms.Label labelBook;
        private System.Windows.Forms.TableLayoutPanel tableSearch;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.ComboBox comboGroup;
        private System.Windows.Forms.ListView listServers;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colHost;
        private System.Windows.Forms.ColumnHeader colIPv4;
        private System.Windows.Forms.ColumnHeader colIPv6;
        private System.Windows.Forms.ColumnHeader colPort;
        private System.Windows.Forms.ColumnHeader colGroup;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colLatency;
        private System.Windows.Forms.ColumnHeader colLastCheck;
        private System.Windows.Forms.FlowLayoutPanel flowBookButtons;
        private System.Windows.Forms.Button BtnAdd;
        private System.Windows.Forms.Button BtnAddFromTarget;
        private System.Windows.Forms.Button BtnEdit;
        private System.Windows.Forms.Button BtnDelete;
        private System.Windows.Forms.Button BtnCheckAll;
        private System.Windows.Forms.Button BtnImport;
        private System.Windows.Forms.Button BtnExport;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabOutput;
        private System.Windows.Forms.RichTextBox richLog;
        private System.Windows.Forms.ToolStrip toolStripLog;
        private System.Windows.Forms.ToolStripButton BtnClearLog;
        private System.Windows.Forms.ToolStripButton BtnCopyLog;
        private System.Windows.Forms.ToolStripButton BtnSaveLog;
        private System.Windows.Forms.ToolStripSeparator toolSepLog1;
        private System.Windows.Forms.ToolStripLabel toolLabelLast;
        private System.Windows.Forms.ToolStripLabel labelIP;
        private System.Windows.Forms.ToolStripButton BtnCopyIP;
        private System.Windows.Forms.TabPage tabStats;
        private LatencyGraph latencyGraph;
        private System.Windows.Forms.TableLayoutPanel tableStats;
        private System.Windows.Forms.Label lblStatSent;
        private System.Windows.Forms.Label lblStatReceived;
        private System.Windows.Forms.Label lblStatLoss;
        private System.Windows.Forms.Label lblStatMin;
        private System.Windows.Forms.Label lblStatAvg;
        private System.Windows.Forms.Label lblStatMax;
        private System.Windows.Forms.Label lblStatJitter;
        private System.Windows.Forms.TabPage tabResults;
        private System.Windows.Forms.ListView listResults;
        private System.Windows.Forms.ColumnHeader colResAddress;
        private System.Windows.Forms.ColumnHeader colResHost;
        private System.Windows.Forms.ColumnHeader colResPing;
        private System.Windows.Forms.ColumnHeader colResPort;
        private System.Windows.Forms.ColumnHeader colResInfo;
        private System.Windows.Forms.ToolStrip toolStripResults;
        private System.Windows.Forms.ToolStripButton BtnResultsSave;
        private System.Windows.Forms.ToolStripButton BtnResultsUse;
        private System.Windows.Forms.ToolStripButton BtnResultsClear;
        private System.Windows.Forms.TabPage tabNetwork;
        private System.Windows.Forms.RichTextBox richNetwork;
        private System.Windows.Forms.ToolStrip toolStripNetwork;
        private System.Windows.Forms.ToolStripButton BtnNetworkRefresh;
        private System.Windows.Forms.ToolStripButton BtnCopyPublicIp;
        private System.Windows.Forms.ContextMenuStrip contextServers;
        private System.Windows.Forms.ToolStripMenuItem menuUseTarget;
        private System.Windows.Forms.ToolStripMenuItem menuPing;
        private System.Windows.Forms.ToolStripMenuItem menuCheck;
        private System.Windows.Forms.ToolStripMenuItem menuPortCheck;
        private System.Windows.Forms.ToolStripMenuItem menuTracert;
        private System.Windows.Forms.ToolStripMenuItem menuUoStatus;
        private System.Windows.Forms.ToolStripSeparator menuSep1;
        private System.Windows.Forms.ToolStripMenuItem menuCopyHost;
        private System.Windows.Forms.ToolStripMenuItem menuCopyIPv4;
        private System.Windows.Forms.ToolStripMenuItem menuCopyIPv6;
        private System.Windows.Forms.ToolStripMenuItem menuCopyAll;
        private System.Windows.Forms.ToolStripSeparator menuSep2;
        private System.Windows.Forms.ToolStripMenuItem menuFavorite;
        private System.Windows.Forms.ToolStripMenuItem menuEdit;
        private System.Windows.Forms.ToolStripMenuItem menuDelete;
        private System.Windows.Forms.Timer timerSearch;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
