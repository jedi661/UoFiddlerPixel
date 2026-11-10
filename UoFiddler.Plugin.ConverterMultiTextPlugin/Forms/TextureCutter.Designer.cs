// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class TextureCutter
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
        /// The menus, the toolbar and the contents of the tabs are built in TextureCutter.cs (BuildMenus / BuildTabs).
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureCutter));
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            previewTimer = new System.Windows.Forms.Timer(components);
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            statusPos = new System.Windows.Forms.ToolStripStatusLabel();
            statusColor = new System.Windows.Forms.ToolStripStatusLabel();
            statusInfo = new System.Windows.Forms.ToolStripStatusLabel();
            statusMessage = new System.Windows.Forms.ToolStripStatusLabel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            panelCanvas = new System.Windows.Forms.Panel();
            canvas = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter.TextureCanvas();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabTiles = new System.Windows.Forms.TabPage();
            tabColors = new System.Windows.Forms.TabPage();
            tabReplace = new System.Windows.Forms.TabPage();
            tabOverlay = new System.Windows.Forms.TabPage();
            tabTexmaps = new System.Windows.Forms.TabPage();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panelCanvas.SuspendLayout();
            tabControl1.SuspendLayout();
            SuspendLayout();
            //
            // toolTip1
            //
            toolTip1.AutoPopDelay = 15000;
            toolTip1.InitialDelay = 600;
            toolTip1.ReshowDelay = 200;
            //
            // previewTimer
            //
            previewTimer.Interval = 60;
            previewTimer.Tick += PreviewTimer_Tick;
            //
            // menuStrip1
            //
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(1280, 24);
            menuStrip1.TabIndex = 0;
            //
            // toolStrip1
            //
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Location = new System.Drawing.Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            toolStrip1.Size = new System.Drawing.Size(1280, 27);
            toolStrip1.TabIndex = 1;
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusPos, statusColor, statusInfo, statusMessage });
            statusStrip1.Location = new System.Drawing.Point(0, 798);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1280, 22);
            statusStrip1.TabIndex = 3;
            //
            // statusPos
            //
            statusPos.AutoSize = false;
            statusPos.Name = "statusPos";
            statusPos.Size = new System.Drawing.Size(110, 17);
            statusPos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // statusColor
            //
            statusColor.AutoSize = false;
            statusColor.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            statusColor.Name = "statusColor";
            statusColor.Size = new System.Drawing.Size(150, 17);
            statusColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // statusInfo
            //
            statusInfo.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            statusInfo.Name = "statusInfo";
            statusInfo.Size = new System.Drawing.Size(4, 17);
            //
            // statusMessage
            //
            statusMessage.Name = "statusMessage";
            statusMessage.Size = new System.Drawing.Size(1001, 17);
            statusMessage.Spring = true;
            statusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // splitContainer1
            //
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer1.Location = new System.Drawing.Point(0, 51);
            splitContainer1.Name = "splitContainer1";
            //
            // splitContainer1.Panel1
            //
            splitContainer1.Panel1.Controls.Add(panelCanvas);
            //
            // splitContainer1.Panel2
            //
            splitContainer1.Panel2.Controls.Add(tabControl1);
            splitContainer1.Panel2MinSize = 300;
            splitContainer1.Size = new System.Drawing.Size(1280, 747);
            splitContainer1.SplitterDistance = 900;
            splitContainer1.TabIndex = 2;
            //
            // panelCanvas
            //
            panelCanvas.AutoScroll = true;
            panelCanvas.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            panelCanvas.Controls.Add(canvas);
            panelCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            panelCanvas.Location = new System.Drawing.Point(0, 0);
            panelCanvas.Name = "panelCanvas";
            panelCanvas.Size = new System.Drawing.Size(900, 747);
            panelCanvas.TabIndex = 0;
            panelCanvas.Paint += PanelCanvas_Paint;
            panelCanvas.Resize += PanelCanvas_Resize;
            //
            // canvas
            //
            canvas.Location = new System.Drawing.Point(0, 0);
            canvas.Name = "canvas";
            canvas.Size = new System.Drawing.Size(1, 1);
            canvas.TabIndex = 0;
            //
            // tabControl1
            //
            tabControl1.Controls.Add(tabTiles);
            tabControl1.Controls.Add(tabColors);
            tabControl1.Controls.Add(tabReplace);
            tabControl1.Controls.Add(tabOverlay);
            tabControl1.Controls.Add(tabTexmaps);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(376, 747);
            tabControl1.TabIndex = 0;
            //
            // tabTiles
            //
            tabTiles.Name = "tabTiles";
            tabTiles.Padding = new System.Windows.Forms.Padding(3);
            tabTiles.Text = "Tiles";
            tabTiles.UseVisualStyleBackColor = true;
            //
            // tabColors
            //
            tabColors.Name = "tabColors";
            tabColors.Padding = new System.Windows.Forms.Padding(3);
            tabColors.Text = "Colors";
            tabColors.UseVisualStyleBackColor = true;
            //
            // tabReplace
            //
            tabReplace.Name = "tabReplace";
            tabReplace.Padding = new System.Windows.Forms.Padding(3);
            tabReplace.Text = "Replace";
            tabReplace.UseVisualStyleBackColor = true;
            //
            // tabOverlay
            //
            tabOverlay.Name = "tabOverlay";
            tabOverlay.Padding = new System.Windows.Forms.Padding(3);
            tabOverlay.Text = "Overlay";
            tabOverlay.UseVisualStyleBackColor = true;
            //
            // tabTexmaps
            //
            tabTexmaps.Name = "tabTexmaps";
            tabTexmaps.Padding = new System.Windows.Forms.Padding(3);
            tabTexmaps.Text = "texmaps";
            tabTexmaps.UseVisualStyleBackColor = true;
            //
            // TextureCutter
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1280, 820);
            Controls.Add(splitContainer1);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            MinimumSize = new System.Drawing.Size(900, 640);
            Name = "TextureCutter";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Texture Cutter";
            TopMost = true;
            FormClosed += TextureCutter_FormClosed;
            Load += TextureCutterForm_Load;
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panelCanvas.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer previewTimer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusPos;
        private System.Windows.Forms.ToolStripStatusLabel statusColor;
        private System.Windows.Forms.ToolStripStatusLabel statusInfo;
        private System.Windows.Forms.ToolStripStatusLabel statusMessage;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelCanvas;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter.TextureCanvas canvas;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabTiles;
        private System.Windows.Forms.TabPage tabColors;
        private System.Windows.Forms.TabPage tabReplace;
        private System.Windows.Forms.TabPage tabOverlay;
        private System.Windows.Forms.TabPage tabTexmaps;
    }
}
