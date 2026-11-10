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
    partial class GumpIDRechner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GumpIDRechner));
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            statusMessage = new System.Windows.Forms.ToolStripStatusLabel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabGump = new System.Windows.Forms.TabPage();
            tabWearables = new System.Windows.Forms.TabPage();
            tabBody = new System.Windows.Forms.TabPage();
            tabHue = new System.Windows.Forms.TabPage();
            tabFlags = new System.Windows.Forms.TabPage();
            tabMap = new System.Windows.Forms.TabPage();
            tabLayer = new System.Windows.Forms.TabPage();
            tabConvert = new System.Windows.Forms.TabPage();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabControl1.SuspendLayout();
            SuspendLayout();
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusMessage });
            statusStrip1.Location = new System.Drawing.Point(0, 678);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1100, 22);
            statusStrip1.TabIndex = 1;
            //
            // statusMessage
            //
            statusMessage.Name = "statusMessage";
            statusMessage.Spring = true;
            statusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // splitContainer1
            //
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Panel1.Controls.Add(tabControl1);
            splitContainer1.Panel2MinSize = 220;
            splitContainer1.Size = new System.Drawing.Size(1100, 678);
            splitContainer1.SplitterDistance = 830;
            splitContainer1.TabIndex = 0;
            //
            // tabControl1
            //
            tabControl1.Controls.Add(tabGump);
            tabControl1.Controls.Add(tabWearables);
            tabControl1.Controls.Add(tabBody);
            tabControl1.Controls.Add(tabHue);
            tabControl1.Controls.Add(tabFlags);
            tabControl1.Controls.Add(tabMap);
            tabControl1.Controls.Add(tabLayer);
            tabControl1.Controls.Add(tabConvert);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(830, 678);
            tabControl1.TabIndex = 0;
            //
            // tab pages
            //
            tabGump.Name = "tabGump";
            tabGump.Text = "Gump ID";
            tabGump.UseVisualStyleBackColor = true;
            tabWearables.Name = "tabWearables";
            tabWearables.Text = "Wearables";
            tabWearables.UseVisualStyleBackColor = true;
            tabBody.Name = "tabBody";
            tabBody.Text = "Body / Anim";
            tabBody.UseVisualStyleBackColor = true;
            tabHue.Name = "tabHue";
            tabHue.Text = "Hue / Color";
            tabHue.UseVisualStyleBackColor = true;
            tabFlags.Name = "tabFlags";
            tabFlags.Text = "TileData flags";
            tabFlags.UseVisualStyleBackColor = true;
            tabMap.Name = "tabMap";
            tabMap.Text = "Map";
            tabMap.UseVisualStyleBackColor = true;
            tabLayer.Name = "tabLayer";
            tabLayer.Text = "Layer / Art index";
            tabLayer.UseVisualStyleBackColor = true;
            tabConvert.Name = "tabConvert";
            tabConvert.Text = "Converter";
            tabConvert.UseVisualStyleBackColor = true;
            //
            // GumpIDRechner
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1100, 700);
            Controls.Add(splitContainer1);
            Controls.Add(statusStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(900, 600);
            Name = "GumpIDRechner";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "UO Calculator – Gump ID";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusMessage;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGump;
        private System.Windows.Forms.TabPage tabWearables;
        private System.Windows.Forms.TabPage tabBody;
        private System.Windows.Forms.TabPage tabHue;
        private System.Windows.Forms.TabPage tabFlags;
        private System.Windows.Forms.TabPage tabMap;
        private System.Windows.Forms.TabPage tabLayer;
        private System.Windows.Forms.TabPage tabConvert;
    }
}
