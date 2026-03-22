using System;
using System.Windows.Forms;
using System.Drawing;

namespace UoFiddler.Controls.Forms
{
    partial class AnimationHexEditorForm
    {
        private System.ComponentModel.IContainer components = null;

        private ToolStrip toolStrip1;
        private ToolStripButton btnGoto;
        private ToolStripButton btnSearch;
        private ToolStripButton btnFindNext;
        private ToolStripButton btnCopyOffset;
        private ToolStripButton btnCopyHex;
        private ToolStripButton btnScreenshot;

        private Panel pnlSearchBar;
        private TextBox txtQuickSearch;
        private Label lblSearchHint;

        private SplitContainer splitMain;

        private HexPanel hexPanel;
        private VScrollBar vScroll;

        private Panel pnlInfo;
        private Label lblPreviewInfo;

        private PictureBox picPreview;
        private Label lblRegionsHeader;
        private ListView listRegions;

        private ColumnHeader colName;
        private ColumnHeader colOffset;
        private ColumnHeader colBytes;
        private ColumnHeader colInfo;

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;

        private ToolTip toolTip1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationHexEditorForm));
            toolStrip1 = new ToolStrip();
            btnGoto = new ToolStripButton();
            btnSearch = new ToolStripButton();
            btnFindNext = new ToolStripButton();
            btnCopyOffset = new ToolStripButton();
            btnCopyHex = new ToolStripButton();
            btnScreenshot = new ToolStripButton();
            pnlSearchBar = new Panel();
            lblSearchHint = new Label();
            txtQuickSearch = new TextBox();
            splitMain = new SplitContainer();
            hexPanel = new HexPanel();
            vScroll = new VScrollBar();
            pnlInfo = new Panel();
            lblPreviewInfo = new Label();
            listRegions = new ListView();
            colName = new ColumnHeader();
            colOffset = new ColumnHeader();
            colBytes = new ColumnHeader();
            colInfo = new ColumnHeader();
            lblRegionsHeader = new Label();
            picPreview = new PictureBox();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            toolTip1 = new ToolTip(components);
            toolStrip1.SuspendLayout();
            pnlSearchBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            pnlInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = Color.FromArgb(48, 48, 48);
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnGoto, btnSearch, btnFindNext, btnCopyOffset, btnCopyHex, btnScreenshot });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1284, 25);
            toolStrip1.TabIndex = 2;
            // 
            // btnGoto
            // 
            btnGoto.ForeColor = Color.White;
            btnGoto.Name = "btnGoto";
            btnGoto.Size = new Size(37, 22);
            btnGoto.Text = "Goto";
            btnGoto.Click += btnGoto_Click;
            // 
            // btnSearch
            // 
            btnSearch.ForeColor = Color.White;
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(46, 22);
            btnSearch.Text = "Search";
            btnSearch.ToolTipText = "Search";
            btnSearch.Click += btnSearch_Click;
            // 
            // btnFindNext
            // 
            btnFindNext.ForeColor = Color.White;
            btnFindNext.Name = "btnFindNext";
            btnFindNext.Size = new Size(36, 22);
            btnFindNext.Text = "Next";
            btnFindNext.ToolTipText = "Next";
            btnFindNext.Click += btnFindNext_Click;
            // 
            // btnCopyOffset
            // 
            btnCopyOffset.ForeColor = Color.White;
            btnCopyOffset.Name = "btnCopyOffset";
            btnCopyOffset.Size = new Size(69, 22);
            btnCopyOffset.Text = "Offset kop.";
            btnCopyOffset.Click += btnCopyOffset_Click;
            // 
            // btnCopyHex
            // 
            btnCopyHex.ForeColor = Color.White;
            btnCopyHex.Name = "btnCopyHex";
            btnCopyHex.Size = new Size(58, 22);
            btnCopyHex.Text = "Hex kop.";
            btnCopyHex.Click += btnCopyHex_Click;
            // 
            // btnScreenshot
            // 
            btnScreenshot.ForeColor = Color.White;
            btnScreenshot.Name = "btnScreenshot";
            btnScreenshot.Size = new Size(69, 22);
            btnScreenshot.Text = "Screenshot";
            btnScreenshot.Click += btnScreenshot_Click;
            // 
            // pnlSearchBar
            // 
            pnlSearchBar.BackColor = Color.FromArgb(42, 42, 42);
            pnlSearchBar.Controls.Add(lblSearchHint);
            pnlSearchBar.Controls.Add(txtQuickSearch);
            pnlSearchBar.Dock = DockStyle.Top;
            pnlSearchBar.Location = new Point(0, 25);
            pnlSearchBar.Name = "pnlSearchBar";
            pnlSearchBar.Size = new Size(1284, 30);
            pnlSearchBar.TabIndex = 1;
            // 
            // lblSearchHint
            // 
            lblSearchHint.ForeColor = Color.FromArgb(160, 160, 160);
            lblSearchHint.Location = new Point(6, 7);
            lblSearchHint.Name = "lblSearchHint";
            lblSearchHint.Size = new Size(100, 23);
            lblSearchHint.TabIndex = 0;
            lblSearchHint.Text = "Schnellsuche (Hex):";
            // 
            // txtQuickSearch
            // 
            txtQuickSearch.BackColor = Color.FromArgb(55, 55, 55);
            txtQuickSearch.BorderStyle = BorderStyle.FixedSingle;
            txtQuickSearch.Font = new Font("Consolas", 9F);
            txtQuickSearch.ForeColor = Color.White;
            txtQuickSearch.Location = new Point(158, 4);
            txtQuickSearch.Name = "txtQuickSearch";
            txtQuickSearch.Size = new Size(280, 22);
            txtQuickSearch.TabIndex = 1;
            txtQuickSearch.KeyDown += txtQuickSearch_KeyDown;
            // 
            // splitMain
            // 
            splitMain.BackColor = Color.FromArgb(60, 60, 60);
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new Point(0, 55);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(hexPanel);
            splitMain.Panel1.Controls.Add(vScroll);
            splitMain.Panel1.Controls.Add(pnlInfo);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(listRegions);
            splitMain.Panel2.Controls.Add(lblRegionsHeader);
            splitMain.Panel2.Controls.Add(picPreview);
            splitMain.Size = new Size(1284, 716);
            splitMain.SplitterDistance = 1035;
            splitMain.TabIndex = 0;
            // 
            // hexPanel
            // 
            hexPanel.BackColor = Color.FromArgb(30, 30, 30);
            hexPanel.Dock = DockStyle.Fill;
            hexPanel.Location = new Point(0, 0);
            hexPanel.Name = "hexPanel";
            hexPanel.Size = new Size(1019, 670);
            hexPanel.TabIndex = 0;
            hexPanel.TabStop = true;
            hexPanel.KeyDown += hexPanel_KeyDown;
            hexPanel.Paint += hexPanel_Paint;
            hexPanel.DoubleClick += hexPanel_DoubleClick;
            hexPanel.MouseClick += hexPanel_MouseClick;
            hexPanel.MouseDown += hexPanel_MouseDown;
            hexPanel.MouseMove += hexPanel_MouseMove;
            hexPanel.MouseUp += hexPanel_MouseUp;
            hexPanel.MouseWheel += hexPanel_MouseWheel;
            hexPanel.Resize += hexPanel_Resize;
            // 
            // vScroll
            // 
            vScroll.Dock = DockStyle.Right;
            vScroll.Location = new Point(1019, 0);
            vScroll.Name = "vScroll";
            vScroll.Size = new Size(16, 670);
            vScroll.TabIndex = 1;
            vScroll.Scroll += vScroll_Scroll;
            // 
            // pnlInfo
            // 
            pnlInfo.BackColor = Color.FromArgb(33, 33, 40);
            pnlInfo.Controls.Add(lblPreviewInfo);
            pnlInfo.Dock = DockStyle.Bottom;
            pnlInfo.Location = new Point(0, 670);
            pnlInfo.Name = "pnlInfo";
            pnlInfo.Padding = new Padding(8, 4, 8, 4);
            pnlInfo.Size = new Size(1035, 46);
            pnlInfo.TabIndex = 2;
            // 
            // lblPreviewInfo
            // 
            lblPreviewInfo.Dock = DockStyle.Fill;
            lblPreviewInfo.Font = new Font("Consolas", 8.5F);
            lblPreviewInfo.ForeColor = Color.FromArgb(160, 210, 255);
            lblPreviewInfo.Location = new Point(8, 4);
            lblPreviewInfo.Name = "lblPreviewInfo";
            lblPreviewInfo.Size = new Size(1019, 38);
            lblPreviewInfo.TabIndex = 0;
            lblPreviewInfo.Text = "No animation loaded.";
            // 
            // listRegions
            // 
            listRegions.BackColor = Color.FromArgb(35, 35, 35);
            listRegions.BorderStyle = BorderStyle.None;
            listRegions.Columns.AddRange(new ColumnHeader[] { colName, colOffset, colBytes, colInfo });
            listRegions.Dock = DockStyle.Fill;
            listRegions.Font = new Font("Consolas", 8.5F);
            listRegions.ForeColor = Color.White;
            listRegions.FullRowSelect = true;
            listRegions.Location = new Point(0, 230);
            listRegions.Name = "listRegions";
            listRegions.Size = new Size(245, 486);
            listRegions.TabIndex = 0;
            listRegions.UseCompatibleStateImageBehavior = false;
            listRegions.View = View.Details;
            listRegions.SelectedIndexChanged += listRegions_SelectedIndexChanged;
            // 
            // colName
            // 
            colName.Text = "Name";
            colName.Width = 130;
            // 
            // colOffset
            // 
            colOffset.Text = "Offset";
            colOffset.Width = 95;
            // 
            // colBytes
            // 
            colBytes.Text = "Bytes";
            colBytes.Width = 58;
            // 
            // colInfo
            // 
            colInfo.Text = "Info";
            colInfo.Width = 160;
            // 
            // lblRegionsHeader
            // 
            lblRegionsHeader.BackColor = Color.FromArgb(42, 42, 42);
            lblRegionsHeader.Dock = DockStyle.Top;
            lblRegionsHeader.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblRegionsHeader.ForeColor = Color.FromArgb(160, 160, 160);
            lblRegionsHeader.Location = new Point(0, 210);
            lblRegionsHeader.Name = "lblRegionsHeader";
            lblRegionsHeader.Size = new Size(245, 20);
            lblRegionsHeader.TabIndex = 1;
            lblRegionsHeader.Text = "Detected regions / sequences:";
            // 
            // picPreview
            // 
            picPreview.BackColor = Color.Black;
            picPreview.BorderStyle = BorderStyle.FixedSingle;
            picPreview.Dock = DockStyle.Top;
            picPreview.Location = new Point(0, 0);
            picPreview.Name = "picPreview";
            picPreview.Size = new Size(245, 210);
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.TabIndex = 2;
            picPreview.TabStop = false;
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.FromArgb(35, 35, 35);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip1.Location = new Point(0, 771);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1284, 22);
            statusStrip1.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.ForeColor = Color.FromArgb(170, 170, 170);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(42, 17);
            lblStatus.Text = "Ready.";
            // 
            // AnimationHexEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(40, 40, 40);
            ClientSize = new Size(1284, 793);
            Controls.Add(splitMain);
            Controls.Add(pnlSearchBar);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            ForeColor = Color.White;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new Size(900, 600);
            Name = "AnimationHexEditorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Animation Hex Editor";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            pnlSearchBar.ResumeLayout(false);
            pnlSearchBar.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            pnlInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
