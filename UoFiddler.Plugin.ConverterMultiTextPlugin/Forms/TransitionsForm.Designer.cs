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
    partial class TransitionsForm
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
        /// The contents of the tabs and views are built in TransitionsForm.cs (Build...).
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransitionsForm));
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            updateTimer = new System.Windows.Forms.Timer(components);
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            statusMessage = new System.Windows.Forms.ToolStripStatusLabel();
            splitMain = new System.Windows.Forms.SplitContainer();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabTextures = new System.Windows.Forms.TabPage();
            tabMasks = new System.Windows.Forms.TabPage();
            tabLearn = new System.Windows.Forms.TabPage();
            tabLibrary = new System.Windows.Forms.TabPage();
            tabOutput = new System.Windows.Forms.TabPage();
            splitRight = new System.Windows.Forms.SplitContainer();
            splitTop = new System.Windows.Forms.SplitContainer();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitRight).BeginInit();
            splitRight.Panel1.SuspendLayout();
            splitRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitTop).BeginInit();
            splitTop.SuspendLayout();
            SuspendLayout();
            //
            // toolTip1
            //
            toolTip1.AutoPopDelay = 15000;
            toolTip1.InitialDelay = 600;
            toolTip1.ReshowDelay = 200;
            //
            // updateTimer
            //
            updateTimer.Interval = 120;
            updateTimer.Tick += UpdateTimer_Tick;
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusMessage });
            statusStrip1.Location = new System.Drawing.Point(0, 818);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1360, 22);
            statusStrip1.TabIndex = 1;
            //
            // statusMessage
            //
            statusMessage.Name = "statusMessage";
            statusMessage.Size = new System.Drawing.Size(1345, 17);
            statusMessage.Spring = true;
            statusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // splitMain
            //
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitMain.Location = new System.Drawing.Point(0, 0);
            splitMain.Name = "splitMain";
            //
            // splitMain.Panel1
            //
            splitMain.Panel1.Controls.Add(tabControl1);
            splitMain.Panel1MinSize = 360;
            //
            // splitMain.Panel2
            //
            splitMain.Panel2.Controls.Add(splitRight);
            splitMain.Size = new System.Drawing.Size(1360, 818);
            splitMain.SplitterDistance = 390;
            splitMain.TabIndex = 0;
            //
            // tabControl1
            //
            tabControl1.Controls.Add(tabTextures);
            tabControl1.Controls.Add(tabMasks);
            tabControl1.Controls.Add(tabLearn);
            tabControl1.Controls.Add(tabLibrary);
            tabControl1.Controls.Add(tabOutput);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(390, 818);
            tabControl1.TabIndex = 0;
            //
            // tabTextures
            //
            tabTextures.Name = "tabTextures";
            tabTextures.Padding = new System.Windows.Forms.Padding(3);
            tabTextures.Text = "Textures";
            tabTextures.UseVisualStyleBackColor = true;
            //
            // tabMasks
            //
            tabMasks.Name = "tabMasks";
            tabMasks.Padding = new System.Windows.Forms.Padding(3);
            tabMasks.Text = "Masks";
            tabMasks.UseVisualStyleBackColor = true;
            //
            // tabLearn
            //
            tabLearn.Name = "tabLearn";
            tabLearn.Padding = new System.Windows.Forms.Padding(3);
            tabLearn.Text = "Learn (UO)";
            tabLearn.UseVisualStyleBackColor = true;
            //
            // tabLibrary
            //
            tabLibrary.Name = "tabLibrary";
            tabLibrary.Padding = new System.Windows.Forms.Padding(3);
            tabLibrary.Text = "Library";
            tabLibrary.UseVisualStyleBackColor = true;
            //
            // tabOutput
            //
            tabOutput.Name = "tabOutput";
            tabOutput.Padding = new System.Windows.Forms.Padding(3);
            tabOutput.Text = "Output";
            tabOutput.UseVisualStyleBackColor = true;
            //
            // splitRight
            //
            splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            splitRight.Location = new System.Drawing.Point(0, 0);
            splitRight.Name = "splitRight";
            splitRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitRight.Panel1
            //
            splitRight.Panel1.Controls.Add(splitTop);
            splitRight.Size = new System.Drawing.Size(966, 818);
            splitRight.SplitterDistance = 380;
            splitRight.TabIndex = 0;
            //
            // splitTop
            //
            splitTop.Dock = System.Windows.Forms.DockStyle.Fill;
            splitTop.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitTop.Location = new System.Drawing.Point(0, 0);
            splitTop.Name = "splitTop";
            splitTop.Size = new System.Drawing.Size(966, 380);
            splitTop.SplitterDistance = 640;
            splitTop.TabIndex = 0;
            //
            // TransitionsForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1360, 840);
            Controls.Add(splitMain);
            Controls.Add(statusStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(1000, 680);
            Name = "TransitionsForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "UO Texture Transitions";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            splitRight.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitRight).EndInit();
            splitRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitTop).EndInit();
            splitTop.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusMessage;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabTextures;
        private System.Windows.Forms.TabPage tabMasks;
        private System.Windows.Forms.TabPage tabLearn;
        private System.Windows.Forms.TabPage tabLibrary;
        private System.Windows.Forms.TabPage tabOutput;
        private System.Windows.Forms.SplitContainer splitRight;
        private System.Windows.Forms.SplitContainer splitTop;
    }
}
