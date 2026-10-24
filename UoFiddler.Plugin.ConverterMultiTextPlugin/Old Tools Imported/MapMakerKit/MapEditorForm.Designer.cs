namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit
{
    partial class MapEditorForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            toolStripMain = new System.Windows.Forms.ToolStrip();
            btnNew = new System.Windows.Forms.ToolStripButton();
            btnOpen = new System.Windows.Forms.ToolStripButton();
            btnSave = new System.Windows.Forms.ToolStripButton();
            btnSaveAs = new System.Windows.Forms.ToolStripButton();
            sep1 = new System.Windows.Forms.ToolStripSeparator();
            btnUndo = new System.Windows.Forms.ToolStripButton();
            btnRedo = new System.Windows.Forms.ToolStripButton();
            sep2 = new System.Windows.Forms.ToolStripSeparator();
            btnZoomIn = new System.Windows.Forms.ToolStripButton();
            btnZoomOut = new System.Windows.Forms.ToolStripButton();
            btnZoomFit = new System.Windows.Forms.ToolStripButton();
            btnZoom1 = new System.Windows.Forms.ToolStripButton();
            sep3 = new System.Windows.Forms.ToolStripSeparator();
            lblView = new System.Windows.Forms.ToolStripLabel();
            cbView = new System.Windows.Forms.ToolStripComboBox();
            btnGrid = new System.Windows.Forms.ToolStripButton();
            sep4 = new System.Windows.Forms.ToolStripSeparator();
            btnCheckTransitions = new System.Windows.Forms.ToolStripButton();
            btnRepairTransitions = new System.Windows.Forms.ToolStripButton();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblPosition = new System.Windows.Forms.ToolStripStatusLabel();
            lblTileInfo = new System.Windows.Forms.ToolStripStatusLabel();
            lblZoom = new System.Windows.Forms.ToolStripStatusLabel();
            lblMessage = new System.Windows.Forms.ToolStripStatusLabel();
            panelLeft = new System.Windows.Forms.Panel();
            groupStamp = new System.Windows.Forms.GroupBox();
            picStamp = new System.Windows.Forms.PictureBox();
            lblStampScale = new System.Windows.Forms.Label();
            numStampScale = new System.Windows.Forms.NumericUpDown();
            lblStampName = new System.Windows.Forms.Label();
            btnChooseStamp = new System.Windows.Forms.Button();
            groupHeight = new System.Windows.Forms.GroupBox();
            numTargetHeight = new System.Windows.Forms.NumericUpDown();
            lblTargetHeight = new System.Windows.Forms.Label();
            numStrength = new System.Windows.Forms.NumericUpDown();
            lblStrength = new System.Windows.Forms.Label();
            groupTerrain = new System.Windows.Forms.GroupBox();
            listTerrain = new System.Windows.Forms.ListBox();
            chkSetAltitude = new System.Windows.Forms.CheckBox();
            txtTerrainFilter = new System.Windows.Forms.TextBox();
            groupBrush = new System.Windows.Forms.GroupBox();
            chkSquare = new System.Windows.Forms.CheckBox();
            lblBrushSize = new System.Windows.Forms.Label();
            trackBrush = new System.Windows.Forms.TrackBar();
            groupTools = new System.Windows.Forms.GroupBox();
            flowTools = new System.Windows.Forms.FlowLayoutPanel();
            rbBrush = new System.Windows.Forms.RadioButton();
            rbFill = new System.Windows.Forms.RadioButton();
            rbRectangle = new System.Windows.Forms.RadioButton();
            rbPicker = new System.Windows.Forms.RadioButton();
            rbRaise = new System.Windows.Forms.RadioButton();
            rbLower = new System.Windows.Forms.RadioButton();
            rbSetHeight = new System.Windows.Forms.RadioButton();
            rbSmooth = new System.Windows.Forms.RadioButton();
            rbStamp = new System.Windows.Forms.RadioButton();
            canvas = new MapCanvas();
            toolTip = new System.Windows.Forms.ToolTip(components);
            toolStripMain.SuspendLayout();
            statusStrip.SuspendLayout();
            panelLeft.SuspendLayout();
            groupStamp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picStamp).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numStampScale).BeginInit();
            groupHeight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTargetHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numStrength).BeginInit();
            groupTerrain.SuspendLayout();
            groupBrush.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBrush).BeginInit();
            groupTools.SuspendLayout();
            flowTools.SuspendLayout();
            SuspendLayout();
            //
            // toolStripMain
            //
            toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnNew, btnOpen, btnSave, btnSaveAs, sep1, btnUndo, btnRedo, sep2, btnZoomIn, btnZoomOut, btnZoomFit, btnZoom1, sep3, lblView, cbView, btnGrid, sep4, btnCheckTransitions, btnRepairTransitions });
            toolStripMain.Location = new System.Drawing.Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            toolStripMain.Size = new System.Drawing.Size(1280, 27);
            toolStripMain.TabIndex = 0;
            //
            // btnNew
            //
            btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnNew.Name = "btnNew";
            btnNew.Text = "New...";
            btnNew.ToolTipText = "New empty map or sketch";
            btnNew.Click += BtnNew_Click;
            //
            // btnOpen
            //
            btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnOpen.Name = "btnOpen";
            btnOpen.Text = "Open...";
            btnOpen.ToolTipText = "Open a terrain image (the altitude image next to it is loaded too)";
            btnOpen.Click += BtnOpen_Click;
            //
            // btnSave
            //
            btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnSave.Name = "btnSave";
            btnSave.Text = "Save";
            btnSave.ToolTipText = "Save terrain + altitude image (Ctrl+S)";
            btnSave.Click += BtnSave_Click;
            //
            // btnSaveAs
            //
            btnSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnSaveAs.Name = "btnSaveAs";
            btnSaveAs.Text = "Save as...";
            btnSaveAs.Click += BtnSaveAs_Click;
            //
            // sep1
            //
            sep1.Name = "sep1";
            //
            // btnUndo
            //
            btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnUndo.Name = "btnUndo";
            btnUndo.Text = "Undo";
            btnUndo.ToolTipText = "Undo (Ctrl+Z)";
            btnUndo.Click += BtnUndo_Click;
            //
            // btnRedo
            //
            btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnRedo.Name = "btnRedo";
            btnRedo.Text = "Redo";
            btnRedo.ToolTipText = "Redo (Ctrl+Y)";
            btnRedo.Click += BtnRedo_Click;
            //
            // sep2
            //
            sep2.Name = "sep2";
            //
            // btnZoomIn
            //
            btnZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Text = "Zoom +";
            btnZoomIn.Click += BtnZoomIn_Click;
            //
            // btnZoomOut
            //
            btnZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Text = "Zoom -";
            btnZoomOut.Click += BtnZoomOut_Click;
            //
            // btnZoomFit
            //
            btnZoomFit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnZoomFit.Name = "btnZoomFit";
            btnZoomFit.Text = "Fit";
            btnZoomFit.ToolTipText = "Show the whole map";
            btnZoomFit.Click += BtnZoomFit_Click;
            //
            // btnZoom1
            //
            btnZoom1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnZoom1.Name = "btnZoom1";
            btnZoom1.Text = "1:8";
            btnZoom1.ToolTipText = "8 pixels per tile";
            btnZoom1.Click += BtnZoom1_Click;
            //
            // sep3
            //
            sep3.Name = "sep3";
            //
            // lblView
            //
            lblView.Name = "lblView";
            lblView.Text = "View:";
            //
            // cbView
            //
            cbView.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbView.Items.AddRange(new object[] { "Terrain", "Altitude", "Relief" });
            cbView.Name = "cbView";
            cbView.Size = new System.Drawing.Size(90, 23);
            cbView.SelectedIndexChanged += CbView_SelectedIndexChanged;
            //
            // btnGrid
            //
            btnGrid.Checked = true;
            btnGrid.CheckOnClick = true;
            btnGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            btnGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnGrid.Name = "btnGrid";
            btnGrid.Text = "8x8 grid";
            btnGrid.ToolTipText = "Show the 8x8 block grid when zoomed in";
            btnGrid.CheckedChanged += BtnGrid_CheckedChanged;
            //
            // sep4
            //
            sep4.Name = "sep4";
            //
            // btnCheckTransitions
            //
            btnCheckTransitions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnCheckTransitions.Name = "btnCheckTransitions";
            btnCheckTransitions.Text = "Check transitions";
            btnCheckTransitions.ToolTipText = "List terrain borders that have no transition (hard edges in the client)";
            btnCheckTransitions.Click += BtnCheckTransitions_Click;
            //
            // btnRepairTransitions
            //
            btnRepairTransitions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnRepairTransitions.Name = "btnRepairTransitions";
            btnRepairTransitions.Text = "Repair transitions";
            btnRepairTransitions.ToolTipText = "Automatically fix single tiles / thin strips without transition";
            btnRepairTransitions.Click += BtnRepairTransitions_Click;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblPosition, lblTileInfo, lblZoom, lblMessage });
            statusStrip.Location = new System.Drawing.Point(0, 828);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1280, 22);
            statusStrip.TabIndex = 3;
            //
            // lblPosition
            //
            lblPosition.AutoSize = false;
            lblPosition.Name = "lblPosition";
            lblPosition.Size = new System.Drawing.Size(120, 17);
            lblPosition.Text = "X: -  Y: -";
            lblPosition.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblTileInfo
            //
            lblTileInfo.AutoSize = false;
            lblTileInfo.Name = "lblTileInfo";
            lblTileInfo.Size = new System.Drawing.Size(380, 17);
            lblTileInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblZoom
            //
            lblZoom.AutoSize = false;
            lblZoom.Name = "lblZoom";
            lblZoom.Size = new System.Drawing.Size(110, 17);
            lblZoom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblMessage
            //
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new System.Drawing.Size(655, 17);
            lblMessage.Spring = true;
            lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // panelLeft
            //
            panelLeft.AutoScroll = true;
            panelLeft.Controls.Add(groupStamp);
            panelLeft.Controls.Add(groupHeight);
            panelLeft.Controls.Add(groupTerrain);
            panelLeft.Controls.Add(groupBrush);
            panelLeft.Controls.Add(groupTools);
            panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            panelLeft.Location = new System.Drawing.Point(0, 27);
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new System.Windows.Forms.Padding(6);
            panelLeft.Size = new System.Drawing.Size(285, 801);
            panelLeft.TabIndex = 1;
            //
            // groupTools
            //
            groupTools.Controls.Add(flowTools);
            groupTools.Dock = System.Windows.Forms.DockStyle.Top;
            groupTools.Location = new System.Drawing.Point(6, 6);
            groupTools.Name = "groupTools";
            groupTools.Size = new System.Drawing.Size(273, 120);
            groupTools.TabIndex = 0;
            groupTools.TabStop = false;
            groupTools.Text = "Tool";
            //
            // flowTools
            //
            flowTools.Controls.Add(rbBrush);
            flowTools.Controls.Add(rbFill);
            flowTools.Controls.Add(rbRectangle);
            flowTools.Controls.Add(rbPicker);
            flowTools.Controls.Add(rbRaise);
            flowTools.Controls.Add(rbLower);
            flowTools.Controls.Add(rbSetHeight);
            flowTools.Controls.Add(rbSmooth);
            flowTools.Controls.Add(rbStamp);
            flowTools.Dock = System.Windows.Forms.DockStyle.Fill;
            flowTools.Location = new System.Drawing.Point(3, 19);
            flowTools.Name = "flowTools";
            flowTools.Size = new System.Drawing.Size(252, 96);
            flowTools.TabIndex = 0;
            //
            // rbBrush
            //
            rbBrush.Appearance = System.Windows.Forms.Appearance.Button;
            rbBrush.Checked = true;
            rbBrush.Name = "rbBrush";
            rbBrush.Size = new System.Drawing.Size(76, 26);
            rbBrush.Margin = new System.Windows.Forms.Padding(2);
            rbBrush.TabIndex = 0;
            rbBrush.TabStop = true;
            rbBrush.Text = "Brush (B)";
            rbBrush.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            rbBrush.CheckedChanged += Tool_CheckedChanged;
            //
            // rbFill
            //
            rbFill.Appearance = System.Windows.Forms.Appearance.Button;
            rbFill.Name = "rbFill";
            rbFill.Size = new System.Drawing.Size(76, 26);
            rbFill.Margin = new System.Windows.Forms.Padding(2);
            rbFill.TabIndex = 1;
            rbFill.Text = "Fill (F)";
            rbFill.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            rbFill.CheckedChanged += Tool_CheckedChanged;
            //
            // rbRectangle
            //
            rbRectangle.Appearance = System.Windows.Forms.Appearance.Button;
            rbRectangle.Name = "rbRectangle";
            rbRectangle.Size = new System.Drawing.Size(76, 26);
            rbRectangle.Margin = new System.Windows.Forms.Padding(2);
            rbRectangle.TabIndex = 2;
            rbRectangle.Text = "Rect (R)";
            rbRectangle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            rbRectangle.CheckedChanged += Tool_CheckedChanged;
            //
            // rbPicker
            //
            rbPicker.Appearance = System.Windows.Forms.Appearance.Button;
            rbPicker.Name = "rbPicker";
            rbPicker.Size = new System.Drawing.Size(76, 26);
            rbPicker.Margin = new System.Windows.Forms.Padding(2);
            rbPicker.TabIndex = 3;
            rbPicker.Text = "Picker (I)";
            rbPicker.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            rbPicker.CheckedChanged += Tool_CheckedChanged;
            //
            // rbRaise
            //
            rbRaise.Appearance = System.Windows.Forms.Appearance.Button;
            rbRaise.Name = "rbRaise";
            rbRaise.Size = new System.Drawing.Size(76, 26);
            rbRaise.Margin = new System.Windows.Forms.Padding(2);
            rbRaise.TabIndex = 4;
            rbRaise.Text = "Raise";
            rbRaise.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            rbRaise.CheckedChanged += Tool_CheckedChanged;
            //
            // rbLower
            //
            rbLower.Appearance = System.Windows.Forms.Appearance.Button;
            rbLower.Name = "rbLower";
            rbLower.Size = new System.Drawing.Size(76, 26);
            rbLower.Margin = new System.Windows.Forms.Padding(2);
            rbLower.TabIndex = 5;
            rbLower.Text = "Lower";
            rbLower.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            rbLower.CheckedChanged += Tool_CheckedChanged;
            //
            // rbSetHeight
            //
            rbSetHeight.Appearance = System.Windows.Forms.Appearance.Button;
            rbSetHeight.Name = "rbSetHeight";
            rbSetHeight.Size = new System.Drawing.Size(76, 26);
            rbSetHeight.Margin = new System.Windows.Forms.Padding(2);
            rbSetHeight.TabIndex = 6;
            rbSetHeight.Text = "Set z";
            rbSetHeight.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            rbSetHeight.CheckedChanged += Tool_CheckedChanged;
            //
            // rbSmooth
            //
            rbSmooth.Appearance = System.Windows.Forms.Appearance.Button;
            rbSmooth.Name = "rbSmooth";
            rbSmooth.Size = new System.Drawing.Size(76, 26);
            rbSmooth.Margin = new System.Windows.Forms.Padding(2);
            rbSmooth.TabIndex = 7;
            rbSmooth.Text = "Smooth";
            rbSmooth.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            rbSmooth.CheckedChanged += Tool_CheckedChanged;
            //
            // rbStamp
            //
            rbStamp.Appearance = System.Windows.Forms.Appearance.Button;
            rbStamp.Name = "rbStamp";
            rbStamp.Size = new System.Drawing.Size(76, 26);
            rbStamp.Margin = new System.Windows.Forms.Padding(2);
            rbStamp.TabIndex = 8;
            rbStamp.Text = "Stamp";
            rbStamp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            rbStamp.CheckedChanged += Tool_CheckedChanged;
            //
            // groupBrush
            //
            groupBrush.Controls.Add(chkSquare);
            groupBrush.Controls.Add(lblBrushSize);
            groupBrush.Controls.Add(trackBrush);
            groupBrush.Dock = System.Windows.Forms.DockStyle.Top;
            groupBrush.Location = new System.Drawing.Point(6, 124);
            groupBrush.Name = "groupBrush";
            groupBrush.Size = new System.Drawing.Size(258, 84);
            groupBrush.TabIndex = 1;
            groupBrush.TabStop = false;
            groupBrush.Text = "Brush";
            //
            // trackBrush
            //
            trackBrush.Location = new System.Drawing.Point(6, 20);
            trackBrush.Maximum = 64;
            trackBrush.Name = "trackBrush";
            trackBrush.Size = new System.Drawing.Size(170, 45);
            trackBrush.TabIndex = 0;
            trackBrush.TickFrequency = 8;
            trackBrush.Value = 3;
            trackBrush.ValueChanged += TrackBrush_ValueChanged;
            //
            // lblBrushSize
            //
            lblBrushSize.Location = new System.Drawing.Point(180, 22);
            lblBrushSize.Name = "lblBrushSize";
            lblBrushSize.Size = new System.Drawing.Size(72, 20);
            lblBrushSize.TabIndex = 1;
            lblBrushSize.Text = "7 tiles";
            //
            // chkSquare
            //
            chkSquare.AutoSize = true;
            chkSquare.Location = new System.Drawing.Point(10, 58);
            chkSquare.Name = "chkSquare";
            chkSquare.Size = new System.Drawing.Size(98, 19);
            chkSquare.TabIndex = 2;
            chkSquare.Text = "Square brush";
            chkSquare.CheckedChanged += ChkSquare_CheckedChanged;
            //
            // groupTerrain
            //
            groupTerrain.Controls.Add(listTerrain);
            groupTerrain.Controls.Add(chkSetAltitude);
            groupTerrain.Controls.Add(txtTerrainFilter);
            groupTerrain.Dock = System.Windows.Forms.DockStyle.Top;
            groupTerrain.Location = new System.Drawing.Point(6, 208);
            groupTerrain.Name = "groupTerrain";
            groupTerrain.Padding = new System.Windows.Forms.Padding(6);
            groupTerrain.Size = new System.Drawing.Size(273, 290);
            groupTerrain.TabIndex = 2;
            groupTerrain.TabStop = false;
            groupTerrain.Text = "Terrain";
            //
            // txtTerrainFilter
            //
            txtTerrainFilter.Dock = System.Windows.Forms.DockStyle.Top;
            txtTerrainFilter.Location = new System.Drawing.Point(6, 22);
            txtTerrainFilter.Name = "txtTerrainFilter";
            txtTerrainFilter.PlaceholderText = "Search terrain ...";
            txtTerrainFilter.Size = new System.Drawing.Size(246, 23);
            txtTerrainFilter.TabIndex = 0;
            txtTerrainFilter.TextChanged += TxtTerrainFilter_TextChanged;
            //
            // chkSetAltitude
            //
            chkSetAltitude.Checked = true;
            chkSetAltitude.CheckState = System.Windows.Forms.CheckState.Checked;
            chkSetAltitude.Dock = System.Windows.Forms.DockStyle.Bottom;
            chkSetAltitude.Location = new System.Drawing.Point(6, 300);
            chkSetAltitude.Name = "chkSetAltitude";
            chkSetAltitude.Size = new System.Drawing.Size(246, 24);
            chkSetAltitude.TabIndex = 2;
            chkSetAltitude.Text = "Also set default altitude";
            //
            // listTerrain
            //
            listTerrain.Dock = System.Windows.Forms.DockStyle.Fill;
            listTerrain.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listTerrain.IntegralHeight = false;
            listTerrain.ItemHeight = 20;
            listTerrain.Location = new System.Drawing.Point(6, 45);
            listTerrain.Name = "listTerrain";
            listTerrain.Size = new System.Drawing.Size(246, 255);
            listTerrain.TabIndex = 1;
            listTerrain.DrawItem += TerrainChoice.DrawItem;
            listTerrain.SelectedIndexChanged += ListTerrain_SelectedIndexChanged;
            //
            // groupHeight
            //
            groupHeight.Controls.Add(numTargetHeight);
            groupHeight.Controls.Add(lblTargetHeight);
            groupHeight.Controls.Add(numStrength);
            groupHeight.Controls.Add(lblStrength);
            groupHeight.Dock = System.Windows.Forms.DockStyle.Top;
            groupHeight.Location = new System.Drawing.Point(6, 538);
            groupHeight.Name = "groupHeight";
            groupHeight.Size = new System.Drawing.Size(258, 82);
            groupHeight.TabIndex = 3;
            groupHeight.TabStop = false;
            groupHeight.Text = "Height tools";
            //
            // lblStrength
            //
            lblStrength.Location = new System.Drawing.Point(8, 24);
            lblStrength.Name = "lblStrength";
            lblStrength.Size = new System.Drawing.Size(150, 20);
            lblStrength.TabIndex = 0;
            lblStrength.Text = "Raise / lower by (z):";
            //
            // numStrength
            //
            numStrength.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numStrength.Location = new System.Drawing.Point(170, 22);
            numStrength.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numStrength.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numStrength.Name = "numStrength";
            numStrength.Size = new System.Drawing.Size(76, 23);
            numStrength.TabIndex = 1;
            numStrength.Value = new decimal(new int[] { 5, 0, 0, 0 });
            //
            // lblTargetHeight
            //
            lblTargetHeight.Location = new System.Drawing.Point(8, 52);
            lblTargetHeight.Name = "lblTargetHeight";
            lblTargetHeight.Size = new System.Drawing.Size(150, 20);
            lblTargetHeight.TabIndex = 2;
            lblTargetHeight.Text = "Set height to (z):";
            //
            // numTargetHeight
            //
            numTargetHeight.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numTargetHeight.Location = new System.Drawing.Point(170, 50);
            numTargetHeight.Maximum = new decimal(new int[] { 115, 0, 0, 0 });
            numTargetHeight.Minimum = new decimal(new int[] { 120, 0, 0, int.MinValue });
            numTargetHeight.Name = "numTargetHeight";
            numTargetHeight.Size = new System.Drawing.Size(76, 23);
            numTargetHeight.TabIndex = 3;
            //
            // groupStamp
            //
            groupStamp.Controls.Add(picStamp);
            groupStamp.Controls.Add(lblStampScale);
            groupStamp.Controls.Add(numStampScale);
            groupStamp.Controls.Add(lblStampName);
            groupStamp.Controls.Add(btnChooseStamp);
            groupStamp.Dock = System.Windows.Forms.DockStyle.Top;
            groupStamp.Location = new System.Drawing.Point(6, 620);
            groupStamp.Name = "groupStamp";
            groupStamp.Size = new System.Drawing.Size(273, 150);
            groupStamp.TabIndex = 4;
            groupStamp.TabStop = false;
            groupStamp.Text = "Stamp (pattern)";
            //
            // btnChooseStamp
            //
            btnChooseStamp.Location = new System.Drawing.Point(8, 22);
            btnChooseStamp.Name = "btnChooseStamp";
            btnChooseStamp.Size = new System.Drawing.Size(120, 26);
            btnChooseStamp.TabIndex = 0;
            btnChooseStamp.Text = "Choose pattern...";
            btnChooseStamp.Click += BtnChooseStamp_Click;
            //
            // lblStampName
            //
            lblStampName.AutoEllipsis = true;
            lblStampName.Location = new System.Drawing.Point(8, 52);
            lblStampName.Name = "lblStampName";
            lblStampName.Size = new System.Drawing.Size(240, 18);
            lblStampName.TabIndex = 1;
            lblStampName.Text = "(no pattern)";
            //
            // lblStampScale
            //
            lblStampScale.Location = new System.Drawing.Point(136, 26);
            lblStampScale.Name = "lblStampScale";
            lblStampScale.Size = new System.Drawing.Size(44, 20);
            lblStampScale.TabIndex = 2;
            lblStampScale.Text = "Scale:";
            //
            // numStampScale
            //
            numStampScale.Location = new System.Drawing.Point(184, 24);
            numStampScale.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            numStampScale.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numStampScale.Name = "numStampScale";
            numStampScale.Size = new System.Drawing.Size(62, 23);
            numStampScale.TabIndex = 3;
            numStampScale.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // picStamp
            //
            picStamp.BackColor = System.Drawing.Color.FromArgb(32, 32, 36);
            picStamp.Location = new System.Drawing.Point(8, 74);
            picStamp.Name = "picStamp";
            picStamp.Size = new System.Drawing.Size(240, 68);
            picStamp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picStamp.TabIndex = 4;
            picStamp.TabStop = false;
            //
            // canvas
            //
            canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            canvas.Location = new System.Drawing.Point(285, 27);
            canvas.Name = "canvas";
            canvas.Size = new System.Drawing.Size(1010, 801);
            canvas.TabIndex = 2;
            canvas.ToolDown += Canvas_ToolDown;
            canvas.ToolMove += Canvas_ToolMove;
            canvas.ToolUp += Canvas_ToolUp;
            canvas.TileHover += Canvas_TileHover;
            canvas.ViewChanged += Canvas_ViewChanged;
            //
            // MapEditorForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1280, 850);
            Controls.Add(canvas);
            Controls.Add(panelLeft);
            Controls.Add(statusStrip);
            Controls.Add(toolStripMain);
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(900, 600);
            Name = "MapEditorForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Map Editor";
            FormClosing += MapEditorForm_FormClosing;
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            panelLeft.ResumeLayout(false);
            groupStamp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picStamp).EndInit();
            ((System.ComponentModel.ISupportInitialize)numStampScale).EndInit();
            groupHeight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numTargetHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numStrength).EndInit();
            groupTerrain.ResumeLayout(false);
            groupTerrain.PerformLayout();
            groupBrush.ResumeLayout(false);
            groupBrush.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBrush).EndInit();
            groupTools.ResumeLayout(false);
            flowTools.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripButton btnNew;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnSaveAs;
        private System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ToolStripButton btnUndo;
        private System.Windows.Forms.ToolStripButton btnRedo;
        private System.Windows.Forms.ToolStripSeparator sep2;
        private System.Windows.Forms.ToolStripButton btnZoomIn;
        private System.Windows.Forms.ToolStripButton btnZoomOut;
        private System.Windows.Forms.ToolStripButton btnZoomFit;
        private System.Windows.Forms.ToolStripButton btnZoom1;
        private System.Windows.Forms.ToolStripSeparator sep3;
        private System.Windows.Forms.ToolStripLabel lblView;
        private System.Windows.Forms.ToolStripComboBox cbView;
        private System.Windows.Forms.ToolStripButton btnGrid;
        private System.Windows.Forms.ToolStripSeparator sep4;
        private System.Windows.Forms.ToolStripButton btnCheckTransitions;
        private System.Windows.Forms.ToolStripButton btnRepairTransitions;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblPosition;
        private System.Windows.Forms.ToolStripStatusLabel lblTileInfo;
        private System.Windows.Forms.ToolStripStatusLabel lblZoom;
        private System.Windows.Forms.ToolStripStatusLabel lblMessage;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.GroupBox groupTools;
        private System.Windows.Forms.FlowLayoutPanel flowTools;
        private System.Windows.Forms.RadioButton rbBrush;
        private System.Windows.Forms.RadioButton rbFill;
        private System.Windows.Forms.RadioButton rbRectangle;
        private System.Windows.Forms.RadioButton rbPicker;
        private System.Windows.Forms.RadioButton rbRaise;
        private System.Windows.Forms.RadioButton rbLower;
        private System.Windows.Forms.RadioButton rbSetHeight;
        private System.Windows.Forms.RadioButton rbSmooth;
        private System.Windows.Forms.RadioButton rbStamp;
        private System.Windows.Forms.GroupBox groupBrush;
        private System.Windows.Forms.TrackBar trackBrush;
        private System.Windows.Forms.Label lblBrushSize;
        private System.Windows.Forms.CheckBox chkSquare;
        private System.Windows.Forms.GroupBox groupTerrain;
        private System.Windows.Forms.TextBox txtTerrainFilter;
        private System.Windows.Forms.ListBox listTerrain;
        private System.Windows.Forms.CheckBox chkSetAltitude;
        private System.Windows.Forms.GroupBox groupHeight;
        private System.Windows.Forms.Label lblStrength;
        private System.Windows.Forms.NumericUpDown numStrength;
        private System.Windows.Forms.Label lblTargetHeight;
        private System.Windows.Forms.NumericUpDown numTargetHeight;
        private System.Windows.Forms.GroupBox groupStamp;
        private System.Windows.Forms.Button btnChooseStamp;
        private System.Windows.Forms.Label lblStampName;
        private System.Windows.Forms.Label lblStampScale;
        private System.Windows.Forms.NumericUpDown numStampScale;
        private System.Windows.Forms.PictureBox picStamp;
        private MapCanvas canvas;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
