/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Improved by: Claude (Anthropic)
 * Changes: Modern dark UI design, progress bar, diff statistics label,
 *          "Next/Prev Diff" navigation buttons, export button, zoom text input,
 *          screenshot button added to toolbar.
 *
 ***************************************************************************/

using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    partial class CompareMapControl
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
                // [8] Only unregister if we actually registered (i.e. _loaded is true)
                if (_loaded)
                {
                    ControlEvents.MapDiffChangeEvent -= OnMapDiffChangeEvent;
                    ControlEvents.MapNameChangeEvent -= OnMapNameChangeEvent;
                    ControlEvents.MapSizeChangeEvent -= OnMapSizeChangeEvent;
                    ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
                }

                _map?.Dispose();
                _map = null;

                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            this.hScrollBar = new System.Windows.Forms.HScrollBar();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorLoad = new System.Windows.Forms.ToolStripSeparator();
            this.CoordsLabel = new System.Windows.Forms.ToolStripLabel();
            this.ZoomLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripZoomTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparatorZoom = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.showDifferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.showMap1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showMap2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.feluccaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trammelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ilshenarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.malasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tokunoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.terMurToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.exportDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.screenshotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            // Navigation strip (top)
            this.toolStripNav = new System.Windows.Forms.ToolStrip();
            this.toolStripLabelDiffStats = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorNav = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonPrevDiff = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonNextDiff = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDiffList = new System.Windows.Forms.ToolStripButton();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripLabelProgress = new System.Windows.Forms.ToolStripLabel();

            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.toolStripNav.SuspendLayout();
            this.SuspendLayout();

            // ──────────────────────────────────────────────
            // vScrollBar
            // ──────────────────────────────────────────────
            this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar.Location = new System.Drawing.Point(703, 25);
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Size = new System.Drawing.Size(17, 323);
            this.vScrollBar.TabIndex = 1;
            this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.HandleScroll);

            // ──────────────────────────────────────────────
            // hScrollBar
            // ──────────────────────────────────────────────
            this.hScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar.Location = new System.Drawing.Point(0, 348);
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.Size = new System.Drawing.Size(720, 17);
            this.hScrollBar.TabIndex = 2;
            this.hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.HandleScroll);

            // ──────────────────────────────────────────────
            // pictureBox
            // ──────────────────────────────────────────────
            this.pictureBox.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 25);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(703, 323);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.pictureBox.SizeChanged += new System.EventHandler(this.OnResize);
            this.pictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);

            // ──────────────────────────────────────────────
            // contextMenuStrip1
            // ──────────────────────────────────────────────
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.zoomToolStripMenuItem,
                this.zoomToolStripMenuItem1 });
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(115, 48);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.OnOpeningContext);

            this.zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
            this.zoomToolStripMenuItem.Text = "Zoom +";
            this.zoomToolStripMenuItem.Click += new System.EventHandler(this.OnZoomPlus);

            this.zoomToolStripMenuItem1.Name = "zoomToolStripMenuItem1";
            this.zoomToolStripMenuItem1.Text = "Zoom -";
            this.zoomToolStripMenuItem1.Click += new System.EventHandler(this.OnZoomMinus);

            // ──────────────────────────────────────────────
            // toolTip1
            // ──────────────────────────────────────────────
            this.toolTip1.UseAnimation = false;
            this.toolTip1.UseFading = false;

            // ──────────────────────────────────────────────
            // toolStripNav  (top bar: diff stats + navigation)
            // ──────────────────────────────────────────────
            this.toolStripNav.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolStripNav.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripNav.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.toolStripNav.ForeColor = System.Drawing.Color.White;
            this.toolStripNav.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripNav.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripLabelDiffStats,
                this.toolStripSeparatorNav,
                this.toolStripButtonPrevDiff,
                this.toolStripButtonNextDiff,
                this.toolStripButtonDiffList,
                this.toolStripProgressBar,
                this.toolStripLabelProgress });
            this.toolStripNav.Location = new System.Drawing.Point(0, 0);
            this.toolStripNav.Name = "toolStripNav";
            this.toolStripNav.Size = new System.Drawing.Size(720, 25);
            this.toolStripNav.TabIndex = 6;

            this.toolStripLabelDiffStats.Name = "toolStripLabelDiffStats";
            this.toolStripLabelDiffStats.Text = "Diffs: –";
            this.toolStripLabelDiffStats.ForeColor = System.Drawing.Color.FromArgb(255, 180, 60);
            this.toolStripLabelDiffStats.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);

            this.toolStripSeparatorNav.Name = "toolStripSeparatorNav";

            this.toolStripButtonPrevDiff.Name = "toolStripButtonPrevDiff";
            this.toolStripButtonPrevDiff.Text = "◄ Prev Diff";
            this.toolStripButtonPrevDiff.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonPrevDiff.Enabled = false;
            this.toolStripButtonPrevDiff.Click += new System.EventHandler(this.OnClickPrevDiff);

            this.toolStripButtonNextDiff.Name = "toolStripButtonNextDiff";
            this.toolStripButtonNextDiff.Text = "Next Diff ►";
            this.toolStripButtonNextDiff.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonNextDiff.Enabled = false;
            this.toolStripButtonNextDiff.Click += new System.EventHandler(this.OnClickNextDiff);

            this.toolStripButtonDiffList.Name = "toolStripButtonDiffList";
            this.toolStripButtonDiffList.Text = "Diff List…";
            this.toolStripButtonDiffList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonDiffList.ToolTipText = "Show all diff coordinates – click to jump, copy to clipboard";
            this.toolStripButtonDiffList.Click += new System.EventHandler(this.OnClickDiffList);

            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(120, 16);
            this.toolStripProgressBar.Visible = false;
            this.toolStripProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;

            this.toolStripLabelProgress.Name = "toolStripLabelProgress";
            this.toolStripLabelProgress.Text = "Calculating...";
            this.toolStripLabelProgress.Visible = false;
            this.toolStripLabelProgress.ForeColor = System.Drawing.Color.LightGray;

            // ──────────────────────────────────────────────
            // toolStrip1  (bottom bar)
            // ──────────────────────────────────────────────
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            this.toolStrip1.ForeColor = System.Drawing.Color.White;
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripTextBox1,
                this.toolStripButton1,
                this.toolStripButton2,
                this.toolStripSeparatorLoad,
                this.CoordsLabel,
                this.ZoomLabel,
                this.toolStripZoomTextBox,
                this.toolStripSeparatorZoom,
                this.toolStripDropDownButton1 });
            this.toolStrip1.Location = new System.Drawing.Point(0, 365);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(720, 25);
            this.toolStrip1.TabIndex = 5;

            // Path textbox
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(174, 25);
            this.toolStripTextBox1.ToolTipText = "Path to map directory (Map2)";

            // Browse button
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Text = "...";
            this.toolStripButton1.ToolTipText = "Browse for map directory";
            this.toolStripButton1.Click += new System.EventHandler(this.OnClickBrowseLoc);

            // Load button
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Text = "Load";
            this.toolStripButton2.ToolTipText = "Load map and calculate differences";
            this.toolStripButton2.Click += new System.EventHandler(this.OnClickLoad);

            this.toolStripSeparatorLoad.Name = "toolStripSeparatorLoad";

            // Coords label
            this.CoordsLabel.AutoSize = false;
            this.CoordsLabel.Name = "CoordsLabel";
            this.CoordsLabel.Size = new System.Drawing.Size(120, 17);
            this.CoordsLabel.Text = "Coords: 0, 0";
            this.CoordsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CoordsLabel.ForeColor = System.Drawing.Color.LightGray;

            // Zoom label
            this.ZoomLabel.Name = "ZoomLabel";
            this.ZoomLabel.Text = "Zoom:";
            this.ZoomLabel.ForeColor = System.Drawing.Color.LightGray;

            // Zoom text input
            this.toolStripZoomTextBox.Name = "toolStripZoomTextBox";
            this.toolStripZoomTextBox.Size = new System.Drawing.Size(45, 25);
            this.toolStripZoomTextBox.Text = "1";
            this.toolStripZoomTextBox.ToolTipText = "Enter zoom value (0.25 – 18) and press Enter";
            this.toolStripZoomTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnZoomTextBoxKeyPress);

            this.toolStripSeparatorZoom.Name = "toolStripSeparatorZoom";

            // ── Map dropdown ──
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.showDifferencesToolStripMenuItem,
                this.toolStripSeparator2,
                this.showMap1ToolStripMenuItem,
                this.showMap2ToolStripMenuItem,
                this.toolStripSeparator1,
                this.feluccaToolStripMenuItem,
                this.trammelToolStripMenuItem,
                this.ilshenarToolStripMenuItem,
                this.malasToolStripMenuItem,
                this.tokunoToolStripMenuItem,
                this.terMurToolStripMenuItem,
                this.toolStripSeparator3,
                this.markDiffToolStripMenuItem,
                this.exportDiffToolStripMenuItem,
                this.screenshotToolStripMenuItem });
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Text = "Map ▾";

            this.showDifferencesToolStripMenuItem.CheckOnClick = true;
            this.showDifferencesToolStripMenuItem.Name = "showDifferencesToolStripMenuItem";
            this.showDifferencesToolStripMenuItem.Text = "Show Differences";
            this.showDifferencesToolStripMenuItem.Click += new System.EventHandler(this.OnClickShowDiff);

            this.toolStripSeparator2.Name = "toolStripSeparator2";

            this.showMap1ToolStripMenuItem.Name = "showMap1ToolStripMenuItem";
            this.showMap1ToolStripMenuItem.Text = "Show Map1 (Original)";
            this.showMap1ToolStripMenuItem.Click += new System.EventHandler(this.OnClickShowMap1);

            this.showMap2ToolStripMenuItem.Name = "showMap2ToolStripMenuItem";
            this.showMap2ToolStripMenuItem.Text = "Show Map2 (Custom)";
            this.showMap2ToolStripMenuItem.Click += new System.EventHandler(this.OnClickShowMap2);

            this.toolStripSeparator1.Name = "toolStripSeparator1";

            this.feluccaToolStripMenuItem.Name = "feluccaToolStripMenuItem";
            this.feluccaToolStripMenuItem.Text = "Felucca";
            this.feluccaToolStripMenuItem.Click += new System.EventHandler(this.OnClickChangeFelucca);

            this.trammelToolStripMenuItem.Name = "trammelToolStripMenuItem";
            this.trammelToolStripMenuItem.Text = "Trammel";
            this.trammelToolStripMenuItem.Click += new System.EventHandler(this.OnClickChangeTrammel);

            this.ilshenarToolStripMenuItem.Name = "ilshenarToolStripMenuItem";
            this.ilshenarToolStripMenuItem.Text = "Ilshenar";
            this.ilshenarToolStripMenuItem.Click += new System.EventHandler(this.OnClickChangeIlshenar);

            this.malasToolStripMenuItem.Name = "malasToolStripMenuItem";
            this.malasToolStripMenuItem.Text = "Malas";
            this.malasToolStripMenuItem.Click += new System.EventHandler(this.OnClickChangeMalas);

            this.tokunoToolStripMenuItem.Name = "tokunoToolStripMenuItem";
            this.tokunoToolStripMenuItem.Text = "Tokuno";
            this.tokunoToolStripMenuItem.Click += new System.EventHandler(this.OnClickChangeTokuno);

            this.terMurToolStripMenuItem.Name = "terMurToolStripMenuItem";
            this.terMurToolStripMenuItem.Text = "TerMur";
            this.terMurToolStripMenuItem.Click += new System.EventHandler(this.OnClickChangeTerMur);

            this.toolStripSeparator3.Name = "toolStripSeparator3";

            this.markDiffToolStripMenuItem.CheckOnClick = true;
            this.markDiffToolStripMenuItem.Name = "markDiffToolStripMenuItem";
            this.markDiffToolStripMenuItem.Text = "Mark Diff (Patches)";
            this.markDiffToolStripMenuItem.Click += new System.EventHandler(this.OnClickMarkDiff);

            this.exportDiffToolStripMenuItem.Name = "exportDiffToolStripMenuItem";
            this.exportDiffToolStripMenuItem.Text = "Export Diff List…";
            this.exportDiffToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportDiff);

            this.screenshotToolStripMenuItem.Name = "screenshotToolStripMenuItem";
            this.screenshotToolStripMenuItem.Text = "Save Screenshot…";
            this.screenshotToolStripMenuItem.Click += new System.EventHandler(this.OnClickScreenshot);

            // ──────────────────────────────────────────────
            // CompareMapControl (root)
            // ──────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.vScrollBar);
            this.Controls.Add(this.hScrollBar);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.toolStripNav);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CompareMapControl";
            this.Size = new System.Drawing.Size(720, 390);
            this.Load += new System.EventHandler(this.OnLoad);

            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStripNav.ResumeLayout(false);
            this.toolStripNav.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // ── existing controls ──
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripLabel CoordsLabel;
        private System.Windows.Forms.ToolStripMenuItem feluccaToolStripMenuItem;
        private System.Windows.Forms.HScrollBar hScrollBar;
        private System.Windows.Forms.ToolStripMenuItem ilshenarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem malasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markDiffToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ToolStripMenuItem showDifferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showMap1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showMap2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem terMurToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tokunoToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem trammelToolStripMenuItem;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.ToolStripLabel ZoomLabel;
        private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem1;
        // ── new controls ──
        private System.Windows.Forms.ToolStrip toolStripNav;
        private System.Windows.Forms.ToolStripLabel toolStripLabelDiffStats;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorNav;
        private System.Windows.Forms.ToolStripButton toolStripButtonPrevDiff;
        private System.Windows.Forms.ToolStripButton toolStripButtonNextDiff;
        private System.Windows.Forms.ToolStripButton toolStripButtonDiffList;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        private System.Windows.Forms.ToolStripLabel toolStripLabelProgress;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorLoad;
        private System.Windows.Forms.ToolStripTextBox toolStripZoomTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorZoom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem exportDiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem screenshotToolStripMenuItem;
    }
}