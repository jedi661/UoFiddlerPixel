// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class ParticleGrayForm
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
        /// The toolbar and the contents of the tabs are built in ParticleGrayForm.cs (Build...).
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParticleGrayForm));
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            previewTimer = new System.Windows.Forms.Timer(components);
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
            tabMask = new System.Windows.Forms.TabPage();
            tabGray = new System.Windows.Forms.TabPage();
            tabHue = new System.Windows.Forms.TabPage();
            tabArt = new System.Windows.Forms.TabPage();
            tabAnalysis = new System.Windows.Forms.TabPage();
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
            previewTimer.Interval = 150;
            previewTimer.Tick += PreviewTimer_Tick;
            //
            // toolStrip1
            //
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            toolStrip1.Size = new System.Drawing.Size(1280, 27);
            toolStrip1.TabIndex = 1;
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusPos, statusColor, statusInfo, statusMessage });
            statusStrip1.Location = new System.Drawing.Point(0, 778);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1280, 22);
            statusStrip1.TabIndex = 2;
            //
            // statusPos
            //
            statusPos.AutoSize = false;
            statusPos.Name = "statusPos";
            statusPos.Size = new System.Drawing.Size(100, 17);
            statusPos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // statusColor
            //
            statusColor.AutoSize = false;
            statusColor.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            statusColor.Name = "statusColor";
            statusColor.Size = new System.Drawing.Size(250, 17);
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
            statusMessage.Size = new System.Drawing.Size(911, 17);
            statusMessage.Spring = true;
            statusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // splitContainer1
            //
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer1.Location = new System.Drawing.Point(0, 27);
            splitContainer1.Name = "splitContainer1";
            //
            // splitContainer1.Panel1
            //
            splitContainer1.Panel1.Controls.Add(panelCanvas);
            //
            // splitContainer1.Panel2
            //
            splitContainer1.Panel2.Controls.Add(tabControl1);
            splitContainer1.Panel2MinSize = 330;
            splitContainer1.Size = new System.Drawing.Size(1280, 751);
            splitContainer1.SplitterDistance = 900;
            splitContainer1.TabIndex = 0;
            //
            // panelCanvas
            //
            panelCanvas.AutoScroll = true;
            panelCanvas.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            panelCanvas.Controls.Add(canvas);
            panelCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            panelCanvas.Location = new System.Drawing.Point(0, 0);
            panelCanvas.Name = "panelCanvas";
            panelCanvas.Size = new System.Drawing.Size(900, 751);
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
            tabControl1.Controls.Add(tabMask);
            tabControl1.Controls.Add(tabGray);
            tabControl1.Controls.Add(tabHue);
            tabControl1.Controls.Add(tabArt);
            tabControl1.Controls.Add(tabAnalysis);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(376, 751);
            tabControl1.TabIndex = 0;
            //
            // tabMask
            //
            tabMask.Name = "tabMask";
            tabMask.Padding = new System.Windows.Forms.Padding(3);
            tabMask.Text = "Mask";
            tabMask.UseVisualStyleBackColor = true;
            //
            // tabGray
            //
            tabGray.Name = "tabGray";
            tabGray.Padding = new System.Windows.Forms.Padding(3);
            tabGray.Text = "Gray";
            tabGray.UseVisualStyleBackColor = true;
            //
            // tabHue
            //
            tabHue.Name = "tabHue";
            tabHue.Padding = new System.Windows.Forms.Padding(3);
            tabHue.Text = "Hue preview";
            tabHue.UseVisualStyleBackColor = true;
            //
            // tabArt
            //
            tabArt.Name = "tabArt";
            tabArt.Padding = new System.Windows.Forms.Padding(3);
            tabArt.Text = "Art";
            tabArt.UseVisualStyleBackColor = true;
            //
            // tabAnalysis
            //
            tabAnalysis.Name = "tabAnalysis";
            tabAnalysis.Padding = new System.Windows.Forms.Padding(3);
            tabAnalysis.Text = "Analysis";
            tabAnalysis.UseVisualStyleBackColor = true;
            //
            // ParticleGrayForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1280, 800);
            Controls.Add(splitContainer1);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(900, 600);
            Name = "ParticleGrayForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Particle Gray";
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
        private System.Windows.Forms.TabPage tabMask;
        private System.Windows.Forms.TabPage tabGray;
        private System.Windows.Forms.TabPage tabHue;
        private System.Windows.Forms.TabPage tabArt;
        private System.Windows.Forms.TabPage tabAnalysis;
    }
}
