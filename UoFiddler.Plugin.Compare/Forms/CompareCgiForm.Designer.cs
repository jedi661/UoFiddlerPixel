namespace UoFiddler.Plugin.Compare.Forms
{
    partial class CompareCgiForm
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
            this.components = new System.ComponentModel.Container();
            this.splitViews = new System.Windows.Forms.SplitContainer();
            this.viewLeft = new UoFiddler.Controls.MapViewer.IsoViewControl();
            this.infoLeft = new System.Windows.Forms.Label();
            this.headerLeft = new System.Windows.Forms.Label();
            this.viewRight = new UoFiddler.Controls.MapViewer.IsoViewControl();
            this.infoRight = new System.Windows.Forms.Label();
            this.headerRight = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this._lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblChanges = new System.Windows.Forms.ToolStripStatusLabel();
            this.barTarget = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTarget = new System.Windows.Forms.Label();
            this._rbSame = new System.Windows.Forms.RadioButton();
            this._rbOther = new System.Windows.Forms.RadioButton();
            this.lblX = new System.Windows.Forms.Label();
            this._numX = new System.Windows.Forms.NumericUpDown();
            this.lblY = new System.Windows.Forms.Label();
            this._numY = new System.Windows.Forms.NumericUpDown();
            this._chkPick = new System.Windows.Forms.CheckBox();
            this.btnGoTarget = new System.Windows.Forms.Button();
            this.gap3 = new System.Windows.Forms.Label();
            this.lblZ = new System.Windows.Forms.Label();
            this._numZ = new System.Windows.Forms.NumericUpDown();
            this._chkAdjust = new System.Windows.Forms.CheckBox();
            this.gap4 = new System.Windows.Forms.Label();
            this._chkConfirm = new System.Windows.Forms.CheckBox();
            this._btnCopy = new System.Windows.Forms.Button();
            this._btnUndo = new System.Windows.Forms.Button();
            this._btnRedo = new System.Windows.Forms.Button();
            this.gap5 = new System.Windows.Forms.Label();
            this._btnSave1 = new System.Windows.Forms.Button();
            this._btnSave2 = new System.Windows.Forms.Button();
            this.barCopy = new System.Windows.Forms.FlowLayoutPanel();
            this.lblCopy = new System.Windows.Forms.Label();
            this._cmbDirection = new System.Windows.Forms.ComboBox();
            this._chkLand = new System.Windows.Forms.CheckBox();
            this._chkCopyStatics = new System.Windows.Forms.CheckBox();
            this._cmbExisting = new System.Windows.Forms.ComboBox();
            this.gap6 = new System.Windows.Forms.Label();
            this._chkSelect = new System.Windows.Forms.CheckBox();
            this.btnWholeArea = new System.Windows.Forms.Button();
            this.barView = new System.Windows.Forms.FlowLayoutPanel();
            this._chkStatics = new System.Windows.Forms.CheckBox();
            this._chkDiff = new System.Windows.Forms.CheckBox();
            this._chkGrid = new System.Windows.Forms.CheckBox();
            this._chkSync = new System.Windows.Forms.CheckBox();
            this.lblMaxZ = new System.Windows.Forms.Label();
            this._numMaxZ = new System.Windows.Forms.NumericUpDown();
            this.gap1 = new System.Windows.Forms.Label();
            this.btnZoomOut = new System.Windows.Forms.Button();
            this.btnZoomIn = new System.Windows.Forms.Button();
            this.btnFit = new System.Windows.Forms.Button();
            this.gap2 = new System.Windows.Forms.Label();
            this._btnNextDiff = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitViews)).BeginInit();
            this.splitViews.Panel1.SuspendLayout();
            this.splitViews.Panel2.SuspendLayout();
            this.splitViews.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.barTarget.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numZ)).BeginInit();
            this.barCopy.SuspendLayout();
            this.barView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numMaxZ)).BeginInit();
            this.SuspendLayout();
            // 
            // splitViews
            // 
            // 
            // splitViews.Panel1
            // 
            this.splitViews.Panel1.Controls.Add(this.viewLeft);
            this.splitViews.Panel1.Controls.Add(this.infoLeft);
            this.splitViews.Panel1.Controls.Add(this.headerLeft);
            // 
            // splitViews.Panel2
            // 
            this.splitViews.Panel2.Controls.Add(this.viewRight);
            this.splitViews.Panel2.Controls.Add(this.infoRight);
            this.splitViews.Panel2.Controls.Add(this.headerRight);
            this.splitViews.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(64)))));
            this.splitViews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitViews.Location = new System.Drawing.Point(0, 99);
            this.splitViews.Name = "splitViews";
            this.splitViews.Size = new System.Drawing.Size(1384, 700);
            this.splitViews.SplitterDistance = 690;
            this.splitViews.SplitterWidth = 5;
            this.splitViews.TabIndex = 0;
            this.splitViews.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.OnSplitterMoved);
            // 
            // viewLeft
            // 
            this.viewLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(22)))));
            this.viewLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewLeft.Name = "viewLeft";
            this.viewLeft.TabIndex = 1;
            this.viewLeft.ViewChanged += new System.EventHandler(this.OnViewChangedLeft);
            this.viewLeft.HoverChanged += new System.EventHandler(this.OnHoverLeft);
            this.viewLeft.AreaSelected += new System.EventHandler(this.OnAreaSelectedLeft);
            this.viewLeft.CellClicked += new System.EventHandler<UoFiddler.Controls.MapViewer.IsoCellEventArgs>(this.OnCellClickedLeft);
            // 
            // infoLeft
            // 
            this.infoLeft.AutoEllipsis = true;
            this.infoLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(40)))));
            this.infoLeft.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.infoLeft.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.infoLeft.ForeColor = System.Drawing.Color.Gainsboro;
            this.infoLeft.Name = "infoLeft";
            this.infoLeft.Padding = new System.Windows.Forms.Padding(6, 2, 2, 2);
            this.infoLeft.Size = new System.Drawing.Size(690, 40);
            // 
            // headerLeft
            // 
            this.headerLeft.AutoEllipsis = true;
            this.headerLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(70)))), ((int)(((byte)(110)))));
            this.headerLeft.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerLeft.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.headerLeft.ForeColor = System.Drawing.Color.White;
            this.headerLeft.Name = "headerLeft";
            this.headerLeft.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.headerLeft.Size = new System.Drawing.Size(690, 24);
            this.headerLeft.Text = "  Map1";
            this.headerLeft.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // viewRight
            // 
            this.viewRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(22)))));
            this.viewRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewRight.Name = "viewRight";
            this.viewRight.TabIndex = 2;
            this.viewRight.ViewChanged += new System.EventHandler(this.OnViewChangedRight);
            this.viewRight.HoverChanged += new System.EventHandler(this.OnHoverRight);
            this.viewRight.AreaSelected += new System.EventHandler(this.OnAreaSelectedRight);
            this.viewRight.CellClicked += new System.EventHandler<UoFiddler.Controls.MapViewer.IsoCellEventArgs>(this.OnCellClickedRight);
            // 
            // infoRight
            // 
            this.infoRight.AutoEllipsis = true;
            this.infoRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(40)))));
            this.infoRight.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.infoRight.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.infoRight.ForeColor = System.Drawing.Color.Gainsboro;
            this.infoRight.Name = "infoRight";
            this.infoRight.Padding = new System.Windows.Forms.Padding(6, 2, 2, 2);
            this.infoRight.Size = new System.Drawing.Size(690, 40);
            // 
            // headerRight
            // 
            this.headerRight.AutoEllipsis = true;
            this.headerRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(70)))), ((int)(((byte)(40)))));
            this.headerRight.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerRight.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.headerRight.ForeColor = System.Drawing.Color.White;
            this.headerRight.Name = "headerRight";
            this.headerRight.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.headerRight.Size = new System.Drawing.Size(690, 24);
            this.headerRight.Text = "  Map2";
            this.headerRight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._lblStatus,
            this._lblChanges});
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.statusStrip.ForeColor = System.Drawing.Color.White;
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.TabIndex = 3;
            // 
            // _lblStatus
            // 
            this._lblStatus.ForeColor = System.Drawing.Color.LightGray;
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Spring = true;
            this._lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblChanges
            // 
            this._lblChanges.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(180)))), ((int)(((byte)(60)))));
            this._lblChanges.Name = "_lblChanges";
            // 
            // barTarget
            // 
            this.barTarget.Controls.Add(this.lblTarget);
            this.barTarget.Controls.Add(this._rbSame);
            this.barTarget.Controls.Add(this._rbOther);
            this.barTarget.Controls.Add(this.lblX);
            this.barTarget.Controls.Add(this._numX);
            this.barTarget.Controls.Add(this.lblY);
            this.barTarget.Controls.Add(this._numY);
            this.barTarget.Controls.Add(this._chkPick);
            this.barTarget.Controls.Add(this.btnGoTarget);
            this.barTarget.Controls.Add(this.gap3);
            this.barTarget.Controls.Add(this.lblZ);
            this.barTarget.Controls.Add(this._numZ);
            this.barTarget.Controls.Add(this._chkAdjust);
            this.barTarget.Controls.Add(this.gap4);
            this.barTarget.Controls.Add(this._chkConfirm);
            this.barTarget.Controls.Add(this._btnCopy);
            this.barTarget.Controls.Add(this._btnUndo);
            this.barTarget.Controls.Add(this._btnRedo);
            this.barTarget.Controls.Add(this.gap5);
            this.barTarget.Controls.Add(this._btnSave1);
            this.barTarget.Controls.Add(this._btnSave2);
            this.barTarget.AutoSize = true;
            this.barTarget.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.barTarget.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.barTarget.Dock = System.Windows.Forms.DockStyle.Top;
            this.barTarget.ForeColor = System.Drawing.Color.White;
            this.barTarget.Name = "barTarget";
            this.barTarget.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.barTarget.Size = new System.Drawing.Size(1384, 31);
            this.barTarget.TabIndex = 4;
            // 
            // lblTarget
            // 
            this.lblTarget.AutoSize = true;
            this.lblTarget.ForeColor = System.Drawing.Color.LightGray;
            this.lblTarget.Margin = new System.Windows.Forms.Padding(6, 7, 2, 3);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Text = "Ziel:";
            // 
            // _rbSame
            // 
            this._rbSame.AutoSize = true;
            this._rbSame.Checked = true;
            this._rbSame.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._rbSame.Name = "_rbSame";
            this._rbSame.TabIndex = 5;
            this._rbSame.TabStop = true;
            this._rbSame.Text = "gleiche Position";
            this._rbSame.UseVisualStyleBackColor = true;
            this._rbSame.CheckedChanged += new System.EventHandler(this.OnTargetModeChanged);
            // 
            // _rbOther
            // 
            this._rbOther.AutoSize = true;
            this._rbOther.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._rbOther.Name = "_rbOther";
            this._rbOther.TabIndex = 6;
            this._rbOther.Text = "andere Position:";
            this._rbOther.UseVisualStyleBackColor = true;
            // 
            // lblX
            // 
            this.lblX.AutoSize = true;
            this.lblX.ForeColor = System.Drawing.Color.LightGray;
            this.lblX.Margin = new System.Windows.Forms.Padding(6, 7, 2, 3);
            this.lblX.Name = "lblX";
            this.lblX.Text = "X";
            // 
            // _numX
            // 
            this._numX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._numX.ForeColor = System.Drawing.Color.White;
            this._numX.Margin = new System.Windows.Forms.Padding(3);
            this._numX.Maximum = new decimal(new int[] {65535, 0, 0, 0});
            this._numX.Minimum = new decimal(new int[] {0, 0, 0, 0});
            this._numX.Name = "_numX";
            this._numX.Size = new System.Drawing.Size(65, 23);
            this._numX.TabIndex = 7;
            this._numX.Value = new decimal(new int[] {0, 0, 0, 0});
            this._numX.ValueChanged += new System.EventHandler(this.OnTargetNumberChanged);
            // 
            // lblY
            // 
            this.lblY.AutoSize = true;
            this.lblY.ForeColor = System.Drawing.Color.LightGray;
            this.lblY.Margin = new System.Windows.Forms.Padding(6, 7, 2, 3);
            this.lblY.Name = "lblY";
            this.lblY.Text = "Y";
            // 
            // _numY
            // 
            this._numY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._numY.ForeColor = System.Drawing.Color.White;
            this._numY.Margin = new System.Windows.Forms.Padding(3);
            this._numY.Maximum = new decimal(new int[] {65535, 0, 0, 0});
            this._numY.Minimum = new decimal(new int[] {0, 0, 0, 0});
            this._numY.Name = "_numY";
            this._numY.Size = new System.Drawing.Size(65, 23);
            this._numY.TabIndex = 8;
            this._numY.Value = new decimal(new int[] {0, 0, 0, 0});
            this._numY.ValueChanged += new System.EventHandler(this.OnTargetNumberChanged);
            // 
            // _chkPick
            // 
            this._chkPick.Appearance = System.Windows.Forms.Appearance.Button;
            this._chkPick.AutoSize = true;
            this._chkPick.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._chkPick.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._chkPick.ForeColor = System.Drawing.Color.White;
            this._chkPick.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._chkPick.Name = "_chkPick";
            this._chkPick.TabIndex = 9;
            this._chkPick.Text = "Ziel anklicken";
            this._chkPick.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this._chkPick, "Danach in der Ziel-Ansicht auf die Zelle klicken, an der die linke obere Ecke landen soll");
            this._chkPick.CheckedChanged += new System.EventHandler(this.OnMouseModeChanged);
            // 
            // btnGoTarget
            // 
            this.btnGoTarget.AutoSize = true;
            this.btnGoTarget.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnGoTarget.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.btnGoTarget.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this.btnGoTarget.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGoTarget.ForeColor = System.Drawing.Color.White;
            this.btnGoTarget.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGoTarget.Name = "btnGoTarget";
            this.btnGoTarget.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnGoTarget.TabIndex = 10;
            this.btnGoTarget.Text = "Ansicht zum Ziel";
            this.btnGoTarget.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this.btnGoTarget, "Den Zielbereich in beiden Ansichten öffnen");
            this.btnGoTarget.Click += new System.EventHandler(this.OnClickGoTarget);
            // 
            // gap3
            // 
            this.gap3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(86)))));
            this.gap3.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.gap3.Name = "gap3";
            this.gap3.Size = new System.Drawing.Size(1, 22);
            // 
            // lblZ
            // 
            this.lblZ.AutoSize = true;
            this.lblZ.ForeColor = System.Drawing.Color.LightGray;
            this.lblZ.Margin = new System.Windows.Forms.Padding(6, 7, 2, 3);
            this.lblZ.Name = "lblZ";
            this.lblZ.Text = "Z ±";
            // 
            // _numZ
            // 
            this._numZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._numZ.ForeColor = System.Drawing.Color.White;
            this._numZ.Margin = new System.Windows.Forms.Padding(3);
            this._numZ.Maximum = new decimal(new int[] {127, 0, 0, 0});
            this._numZ.Minimum = new decimal(new int[] {127, 0, 0, -2147483648});
            this._numZ.Name = "_numZ";
            this._numZ.Size = new System.Drawing.Size(50, 23);
            this._numZ.TabIndex = 11;
            this._numZ.Value = new decimal(new int[] {0, 0, 0, 0});
            this.toolTip.SetToolTip(this._numZ, "Zusätzlicher Höhenversatz für Land und Statics");
            // 
            // _chkAdjust
            // 
            this._chkAdjust.AutoSize = true;
            this._chkAdjust.ForeColor = System.Drawing.Color.White;
            this._chkAdjust.Margin = new System.Windows.Forms.Padding(4, 6, 4, 3);
            this._chkAdjust.Name = "_chkAdjust";
            this._chkAdjust.TabIndex = 12;
            this._chkAdjust.Text = "Höhe an Ziel anpassen";
            this._chkAdjust.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this._chkAdjust, "Höhe an den Rand des Zielbereichs anpassen (mit weichem Übergang). Statics folgen dem Boden.");
            // 
            // gap4
            // 
            this.gap4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(86)))));
            this.gap4.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.gap4.Name = "gap4";
            this.gap4.Size = new System.Drawing.Size(1, 22);
            // 
            // _chkConfirm
            // 
            this._chkConfirm.AutoSize = true;
            this._chkConfirm.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkConfirm.Checked = true;
            this._chkConfirm.ForeColor = System.Drawing.Color.White;
            this._chkConfirm.Margin = new System.Windows.Forms.Padding(4, 6, 4, 3);
            this._chkConfirm.Name = "_chkConfirm";
            this._chkConfirm.TabIndex = 13;
            this._chkConfirm.Text = "Nachfragen";
            this._chkConfirm.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this._chkConfirm, "Vor dem Kopieren eine Zusammenfassung anzeigen");
            // 
            // _btnCopy
            // 
            this._btnCopy.AutoSize = true;
            this._btnCopy.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._btnCopy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this._btnCopy.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this._btnCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnCopy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnCopy.ForeColor = System.Drawing.Color.White;
            this._btnCopy.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._btnCopy.Name = "_btnCopy";
            this._btnCopy.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnCopy.TabIndex = 14;
            this._btnCopy.Text = "Kopieren ▶";
            this._btnCopy.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this._btnCopy, "Ausgewählten Bereich in die Zielkarte kopieren (nur im Speicher, Strg+Z = rückgängig)");
            this._btnCopy.Click += new System.EventHandler(this.OnClickCopy);
            // 
            // _btnUndo
            // 
            this._btnUndo.AutoSize = true;
            this._btnUndo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._btnUndo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._btnUndo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this._btnUndo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnUndo.ForeColor = System.Drawing.Color.White;
            this._btnUndo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._btnUndo.Name = "_btnUndo";
            this._btnUndo.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnUndo.TabIndex = 15;
            this._btnUndo.Text = "↶ Rückgängig";
            this._btnUndo.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this._btnUndo, "Letzten Kopiervorgang rückgängig machen (Strg+Z)");
            this._btnUndo.Click += new System.EventHandler(this.OnClickUndo);
            // 
            // _btnRedo
            // 
            this._btnRedo.AutoSize = true;
            this._btnRedo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._btnRedo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._btnRedo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this._btnRedo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnRedo.ForeColor = System.Drawing.Color.White;
            this._btnRedo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._btnRedo.Name = "_btnRedo";
            this._btnRedo.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnRedo.TabIndex = 16;
            this._btnRedo.Text = "↷ Wiederholen";
            this._btnRedo.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this._btnRedo, "Wiederholen (Strg+Y)");
            this._btnRedo.Click += new System.EventHandler(this.OnClickRedo);
            // 
            // gap5
            // 
            this.gap5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(86)))));
            this.gap5.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.gap5.Name = "gap5";
            this.gap5.Size = new System.Drawing.Size(1, 22);
            // 
            // _btnSave1
            // 
            this._btnSave1.AutoSize = true;
            this._btnSave1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._btnSave1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._btnSave1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this._btnSave1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSave1.ForeColor = System.Drawing.Color.White;
            this._btnSave1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._btnSave1.Name = "_btnSave1";
            this._btnSave1.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnSave1.TabIndex = 17;
            this._btnSave1.Text = "Map1 speichern…";
            this._btnSave1.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this._btnSave1, "Änderungen an Map1 in Dateien schreiben");
            this._btnSave1.Click += new System.EventHandler(this.OnClickSave1);
            // 
            // _btnSave2
            // 
            this._btnSave2.AutoSize = true;
            this._btnSave2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._btnSave2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._btnSave2.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this._btnSave2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnSave2.ForeColor = System.Drawing.Color.White;
            this._btnSave2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._btnSave2.Name = "_btnSave2";
            this._btnSave2.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnSave2.TabIndex = 18;
            this._btnSave2.Text = "Map2 speichern…";
            this._btnSave2.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this._btnSave2, "Änderungen an Map2 in Dateien schreiben");
            this._btnSave2.Click += new System.EventHandler(this.OnClickSave2);
            // 
            // barCopy
            // 
            this.barCopy.Controls.Add(this.lblCopy);
            this.barCopy.Controls.Add(this._cmbDirection);
            this.barCopy.Controls.Add(this._chkLand);
            this.barCopy.Controls.Add(this._chkCopyStatics);
            this.barCopy.Controls.Add(this._cmbExisting);
            this.barCopy.Controls.Add(this.gap6);
            this.barCopy.Controls.Add(this._chkSelect);
            this.barCopy.Controls.Add(this.btnWholeArea);
            this.barCopy.AutoSize = true;
            this.barCopy.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.barCopy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.barCopy.Dock = System.Windows.Forms.DockStyle.Top;
            this.barCopy.ForeColor = System.Drawing.Color.White;
            this.barCopy.Name = "barCopy";
            this.barCopy.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.barCopy.Size = new System.Drawing.Size(1384, 31);
            this.barCopy.TabIndex = 19;
            // 
            // lblCopy
            // 
            this.lblCopy.AutoSize = true;
            this.lblCopy.ForeColor = System.Drawing.Color.LightGray;
            this.lblCopy.Margin = new System.Windows.Forms.Padding(6, 7, 2, 3);
            this.lblCopy.Name = "lblCopy";
            this.lblCopy.Text = "Kopieren:";
            // 
            // _cmbDirection
            // 
            this._cmbDirection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._cmbDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbDirection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._cmbDirection.ForeColor = System.Drawing.Color.White;
            this._cmbDirection.FormattingEnabled = true;
            this._cmbDirection.Margin = new System.Windows.Forms.Padding(3);
            this._cmbDirection.Name = "_cmbDirection";
            this._cmbDirection.Size = new System.Drawing.Size(220, 23);
            this._cmbDirection.TabIndex = 20;
            this._cmbDirection.SelectedIndexChanged += new System.EventHandler(this.OnDirectionChanged);
            // 
            // _chkLand
            // 
            this._chkLand.AutoSize = true;
            this._chkLand.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkLand.Checked = true;
            this._chkLand.ForeColor = System.Drawing.Color.White;
            this._chkLand.Margin = new System.Windows.Forms.Padding(4, 6, 4, 3);
            this._chkLand.Name = "_chkLand";
            this._chkLand.TabIndex = 21;
            this._chkLand.Text = "Land (Boden)";
            this._chkLand.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this._chkLand, "Land-Tiles (ID und Höhe) kopieren");
            // 
            // _chkCopyStatics
            // 
            this._chkCopyStatics.AutoSize = true;
            this._chkCopyStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkCopyStatics.Checked = true;
            this._chkCopyStatics.ForeColor = System.Drawing.Color.White;
            this._chkCopyStatics.Margin = new System.Windows.Forms.Padding(4, 6, 4, 3);
            this._chkCopyStatics.Name = "_chkCopyStatics";
            this._chkCopyStatics.TabIndex = 22;
            this._chkCopyStatics.Text = "Statics mitkopieren";
            this._chkCopyStatics.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this._chkCopyStatics, "Statics des Bereichs mitkopieren");
            this._chkCopyStatics.CheckedChanged += new System.EventHandler(this.OnCopyStaticsChanged);
            // 
            // _cmbExisting
            // 
            this._cmbExisting.Items.AddRange(new object[] {
            "Ziel-Statics im Bereich ersetzen",
            "nur Pflanzen im Ziel entfernen",
            "Ziel-Statics behalten (hinzufügen)"});
            this._cmbExisting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._cmbExisting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbExisting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._cmbExisting.ForeColor = System.Drawing.Color.White;
            this._cmbExisting.FormattingEnabled = true;
            this._cmbExisting.Margin = new System.Windows.Forms.Padding(3);
            this._cmbExisting.Name = "_cmbExisting";
            this._cmbExisting.Size = new System.Drawing.Size(230, 23);
            this._cmbExisting.TabIndex = 23;
            this.toolTip.SetToolTip(this._cmbExisting, "Was mit den vorhandenen Statics im Zielbereich passiert, wenn Statics mitkopiert werden");
            // 
            // gap6
            // 
            this.gap6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(86)))));
            this.gap6.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.gap6.Name = "gap6";
            this.gap6.Size = new System.Drawing.Size(1, 22);
            // 
            // _chkSelect
            // 
            this._chkSelect.Appearance = System.Windows.Forms.Appearance.Button;
            this._chkSelect.AutoSize = true;
            this._chkSelect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._chkSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._chkSelect.ForeColor = System.Drawing.Color.White;
            this._chkSelect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._chkSelect.Name = "_chkSelect";
            this._chkSelect.TabIndex = 24;
            this._chkSelect.Text = "Teilbereich wählen";
            this._chkSelect.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this._chkSelect, "Mit der linken Maustaste in einer Ansicht einen Teilbereich aufziehen (verschieben dann mit der mittleren Maustaste)");
            this._chkSelect.CheckedChanged += new System.EventHandler(this.OnMouseModeChanged);
            // 
            // btnWholeArea
            // 
            this.btnWholeArea.AutoSize = true;
            this.btnWholeArea.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnWholeArea.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.btnWholeArea.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this.btnWholeArea.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWholeArea.ForeColor = System.Drawing.Color.White;
            this.btnWholeArea.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnWholeArea.Name = "btnWholeArea";
            this.btnWholeArea.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnWholeArea.TabIndex = 25;
            this.btnWholeArea.Text = "Ganzer Bereich";
            this.btnWholeArea.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this.btnWholeArea, "Teilauswahl aufheben – der ganze geöffnete Bereich wird kopiert");
            this.btnWholeArea.Click += new System.EventHandler(this.OnClickWholeArea);
            // 
            // barView
            // 
            this.barView.Controls.Add(this._chkStatics);
            this.barView.Controls.Add(this._chkDiff);
            this.barView.Controls.Add(this._chkGrid);
            this.barView.Controls.Add(this._chkSync);
            this.barView.Controls.Add(this.lblMaxZ);
            this.barView.Controls.Add(this._numMaxZ);
            this.barView.Controls.Add(this.gap1);
            this.barView.Controls.Add(this.btnZoomOut);
            this.barView.Controls.Add(this.btnZoomIn);
            this.barView.Controls.Add(this.btnFit);
            this.barView.Controls.Add(this.gap2);
            this.barView.Controls.Add(this._btnNextDiff);
            this.barView.AutoSize = true;
            this.barView.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.barView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.barView.Dock = System.Windows.Forms.DockStyle.Top;
            this.barView.ForeColor = System.Drawing.Color.White;
            this.barView.Name = "barView";
            this.barView.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.barView.Size = new System.Drawing.Size(1384, 31);
            this.barView.TabIndex = 26;
            // 
            // _chkStatics
            // 
            this._chkStatics.AutoSize = true;
            this._chkStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkStatics.Checked = true;
            this._chkStatics.ForeColor = System.Drawing.Color.White;
            this._chkStatics.Margin = new System.Windows.Forms.Padding(4, 6, 4, 3);
            this._chkStatics.Name = "_chkStatics";
            this._chkStatics.TabIndex = 27;
            this._chkStatics.Text = "Statics";
            this._chkStatics.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this._chkStatics, "Statics in beiden Ansichten ein-/ausblenden");
            this._chkStatics.CheckedChanged += new System.EventHandler(this.OnStaticsChanged);
            // 
            // _chkDiff
            // 
            this._chkDiff.AutoSize = true;
            this._chkDiff.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkDiff.Checked = true;
            this._chkDiff.ForeColor = System.Drawing.Color.White;
            this._chkDiff.Margin = new System.Windows.Forms.Padding(4, 6, 4, 3);
            this._chkDiff.Name = "_chkDiff";
            this._chkDiff.TabIndex = 28;
            this._chkDiff.Text = "Unterschiede markieren";
            this._chkDiff.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this._chkDiff, "Zellen, die sich zwischen Map1 und Map2 unterscheiden, rot markieren");
            this._chkDiff.CheckedChanged += new System.EventHandler(this.OnDiffChanged);
            // 
            // _chkGrid
            // 
            this._chkGrid.AutoSize = true;
            this._chkGrid.ForeColor = System.Drawing.Color.White;
            this._chkGrid.Margin = new System.Windows.Forms.Padding(4, 6, 4, 3);
            this._chkGrid.Name = "_chkGrid";
            this._chkGrid.TabIndex = 29;
            this._chkGrid.Text = "Raster";
            this._chkGrid.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this._chkGrid, "Raster über den Zellen anzeigen");
            this._chkGrid.CheckedChanged += new System.EventHandler(this.OnGridChanged);
            // 
            // _chkSync
            // 
            this._chkSync.AutoSize = true;
            this._chkSync.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkSync.Checked = true;
            this._chkSync.ForeColor = System.Drawing.Color.White;
            this._chkSync.Margin = new System.Windows.Forms.Padding(4, 6, 4, 3);
            this._chkSync.Name = "_chkSync";
            this._chkSync.TabIndex = 30;
            this._chkSync.Text = "Ansichten synchron";
            this._chkSync.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this._chkSync, "Verschieben und Zoomen in einer Ansicht bewegt die andere mit");
            this._chkSync.CheckedChanged += new System.EventHandler(this.OnSyncChanged);
            // 
            // lblMaxZ
            // 
            this.lblMaxZ.AutoSize = true;
            this.lblMaxZ.ForeColor = System.Drawing.Color.LightGray;
            this.lblMaxZ.Margin = new System.Windows.Forms.Padding(6, 7, 2, 3);
            this.lblMaxZ.Name = "lblMaxZ";
            this.lblMaxZ.Text = "Max. Z:";
            // 
            // _numMaxZ
            // 
            this._numMaxZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._numMaxZ.ForeColor = System.Drawing.Color.White;
            this._numMaxZ.Margin = new System.Windows.Forms.Padding(3);
            this._numMaxZ.Maximum = new decimal(new int[] {127, 0, 0, 0});
            this._numMaxZ.Minimum = new decimal(new int[] {128, 0, 0, -2147483648});
            this._numMaxZ.Name = "_numMaxZ";
            this._numMaxZ.Size = new System.Drawing.Size(55, 23);
            this._numMaxZ.TabIndex = 31;
            this._numMaxZ.Value = new decimal(new int[] {127, 0, 0, 0});
            this.toolTip.SetToolTip(this._numMaxZ, "Statics über dieser Höhe ausblenden (z. B. um unter Dächer zu schauen)");
            this._numMaxZ.ValueChanged += new System.EventHandler(this.OnMaxZChanged);
            // 
            // gap1
            // 
            this.gap1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(86)))));
            this.gap1.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.gap1.Name = "gap1";
            this.gap1.Size = new System.Drawing.Size(1, 22);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.AutoSize = true;
            this.btnZoomOut.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnZoomOut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.btnZoomOut.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this.btnZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomOut.ForeColor = System.Drawing.Color.White;
            this.btnZoomOut.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnZoomOut.TabIndex = 32;
            this.btnZoomOut.Text = "Zoom −";
            this.btnZoomOut.UseVisualStyleBackColor = false;
            this.btnZoomOut.Click += new System.EventHandler(this.OnClickZoomOut);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.AutoSize = true;
            this.btnZoomIn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnZoomIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.btnZoomIn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this.btnZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomIn.ForeColor = System.Drawing.Color.White;
            this.btnZoomIn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnZoomIn.TabIndex = 33;
            this.btnZoomIn.Text = "Zoom +";
            this.btnZoomIn.UseVisualStyleBackColor = false;
            this.btnZoomIn.Click += new System.EventHandler(this.OnClickZoomIn);
            // 
            // btnFit
            // 
            this.btnFit.AutoSize = true;
            this.btnFit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnFit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this.btnFit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this.btnFit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFit.ForeColor = System.Drawing.Color.White;
            this.btnFit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnFit.Name = "btnFit";
            this.btnFit.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnFit.TabIndex = 34;
            this.btnFit.Text = "Einpassen";
            this.btnFit.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this.btnFit, "Ganzen Bereich anzeigen");
            this.btnFit.Click += new System.EventHandler(this.OnClickFit);
            // 
            // gap2
            // 
            this.gap2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(86)))));
            this.gap2.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.gap2.Name = "gap2";
            this.gap2.Size = new System.Drawing.Size(1, 22);
            // 
            // _btnNextDiff
            // 
            this._btnNextDiff.AutoSize = true;
            this._btnNextDiff.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._btnNextDiff.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(66)))));
            this._btnNextDiff.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(96)))));
            this._btnNextDiff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnNextDiff.ForeColor = System.Drawing.Color.White;
            this._btnNextDiff.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this._btnNextDiff.Name = "_btnNextDiff";
            this._btnNextDiff.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._btnNextDiff.TabIndex = 35;
            this._btnNextDiff.Text = "Nächster Unterschied ►";
            this._btnNextDiff.UseVisualStyleBackColor = false;
            this.toolTip.SetToolTip(this._btnNextDiff, "Zur nächsten unterschiedlichen Zelle springen");
            this._btnNextDiff.Click += new System.EventHandler(this.OnClickNextDiff);
            // 
            // CompareCgiForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(1384, 821);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(900, 520);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CGI-Vergleich";
            this.Controls.Add(this.splitViews);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.barTarget);
            this.Controls.Add(this.barCopy);
            this.Controls.Add(this.barView);
            this.Name = "CompareCgiForm";
            this.Shown += new System.EventHandler(this.OnFormShown);
            this.splitViews.Panel1.ResumeLayout(false);
            this.splitViews.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitViews)).EndInit();
            this.splitViews.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.barTarget.ResumeLayout(false);
            this.barTarget.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numZ)).EndInit();
            this.barCopy.ResumeLayout(false);
            this.barCopy.PerformLayout();
            this.barView.ResumeLayout(false);
            this.barView.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numMaxZ)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitViews;
        private UoFiddler.Controls.MapViewer.IsoViewControl viewLeft;
        private System.Windows.Forms.Label infoLeft;
        private System.Windows.Forms.Label headerLeft;
        private UoFiddler.Controls.MapViewer.IsoViewControl viewRight;
        private System.Windows.Forms.Label infoRight;
        private System.Windows.Forms.Label headerRight;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel _lblChanges;
        private System.Windows.Forms.FlowLayoutPanel barTarget;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.RadioButton _rbSame;
        private System.Windows.Forms.RadioButton _rbOther;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.NumericUpDown _numX;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.NumericUpDown _numY;
        private System.Windows.Forms.CheckBox _chkPick;
        private System.Windows.Forms.Button btnGoTarget;
        private System.Windows.Forms.Label gap3;
        private System.Windows.Forms.Label lblZ;
        private System.Windows.Forms.NumericUpDown _numZ;
        private System.Windows.Forms.CheckBox _chkAdjust;
        private System.Windows.Forms.Label gap4;
        private System.Windows.Forms.CheckBox _chkConfirm;
        private System.Windows.Forms.Button _btnCopy;
        private System.Windows.Forms.Button _btnUndo;
        private System.Windows.Forms.Button _btnRedo;
        private System.Windows.Forms.Label gap5;
        private System.Windows.Forms.Button _btnSave1;
        private System.Windows.Forms.Button _btnSave2;
        private System.Windows.Forms.FlowLayoutPanel barCopy;
        private System.Windows.Forms.Label lblCopy;
        private System.Windows.Forms.ComboBox _cmbDirection;
        private System.Windows.Forms.CheckBox _chkLand;
        private System.Windows.Forms.CheckBox _chkCopyStatics;
        private System.Windows.Forms.ComboBox _cmbExisting;
        private System.Windows.Forms.Label gap6;
        private System.Windows.Forms.CheckBox _chkSelect;
        private System.Windows.Forms.Button btnWholeArea;
        private System.Windows.Forms.FlowLayoutPanel barView;
        private System.Windows.Forms.CheckBox _chkStatics;
        private System.Windows.Forms.CheckBox _chkDiff;
        private System.Windows.Forms.CheckBox _chkGrid;
        private System.Windows.Forms.CheckBox _chkSync;
        private System.Windows.Forms.Label lblMaxZ;
        private System.Windows.Forms.NumericUpDown _numMaxZ;
        private System.Windows.Forms.Label gap1;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Button btnFit;
        private System.Windows.Forms.Label gap2;
        private System.Windows.Forms.Button _btnNextDiff;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
