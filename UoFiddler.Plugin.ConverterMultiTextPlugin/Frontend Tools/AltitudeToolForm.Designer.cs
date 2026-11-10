namespace UoFiddler.Plugin.ConverterMultiTextPlugin
{
    partial class AltitudeToolForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AltitudeToolForm));
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            statusPos = new System.Windows.Forms.ToolStripStatusLabel();
            statusInfo = new System.Windows.Forms.ToolStripStatusLabel();
            statusProgress = new System.Windows.Forms.ToolStripProgressBar();
            statusCancel = new System.Windows.Forms.ToolStripDropDownButton();
            statusMessage = new System.Windows.Forms.ToolStripStatusLabel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabSource = new System.Windows.Forms.TabPage();
            tabExport = new System.Windows.Forms.TabPage();
            tabImport = new System.Windows.Forms.TabPage();
            tabEdit = new System.Windows.Forms.TabPage();
            tabInfo = new System.Windows.Forms.TabPage();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabControl1.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip1
            //
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1280, 25);
            toolStrip1.TabIndex = 0;
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusPos, statusInfo, statusProgress, statusCancel, statusMessage });
            statusStrip1.Location = new System.Drawing.Point(0, 778);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1280, 22);
            statusStrip1.TabIndex = 2;
            //
            // statusPos
            //
            statusPos.AutoSize = false;
            statusPos.Name = "statusPos";
            statusPos.Size = new System.Drawing.Size(190, 17);
            statusPos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // statusInfo
            //
            statusInfo.Name = "statusInfo";
            statusInfo.Size = new System.Drawing.Size(0, 17);
            //
            // statusProgress
            //
            statusProgress.Name = "statusProgress";
            statusProgress.Size = new System.Drawing.Size(150, 16);
            statusProgress.Visible = false;
            //
            // statusCancel
            //
            statusCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            statusCancel.Name = "statusCancel";
            statusCancel.ShowDropDownArrow = false;
            statusCancel.Text = "Cancel";
            statusCancel.Visible = false;
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
            splitContainer1.Location = new System.Drawing.Point(0, 25);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Panel2.Controls.Add(tabControl1);
            splitContainer1.Panel2MinSize = 340;
            splitContainer1.Size = new System.Drawing.Size(1280, 753);
            splitContainer1.SplitterDistance = 890;
            splitContainer1.TabIndex = 1;
            //
            // tabControl1
            //
            tabControl1.Controls.Add(tabSource);
            tabControl1.Controls.Add(tabExport);
            tabControl1.Controls.Add(tabImport);
            tabControl1.Controls.Add(tabEdit);
            tabControl1.Controls.Add(tabInfo);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(386, 753);
            tabControl1.TabIndex = 0;
            //
            // tab pages
            //
            tabSource.Name = "tabSource";
            tabSource.Text = "Map";
            tabSource.UseVisualStyleBackColor = true;
            tabExport.Name = "tabExport";
            tabExport.Text = "Extract";
            tabExport.UseVisualStyleBackColor = true;
            tabImport.Name = "tabImport";
            tabImport.Text = "Apply";
            tabImport.UseVisualStyleBackColor = true;
            tabEdit.Name = "tabEdit";
            tabEdit.Text = "Modify";
            tabEdit.UseVisualStyleBackColor = true;
            tabInfo.Name = "tabInfo";
            tabInfo.Text = "Info";
            tabInfo.UseVisualStyleBackColor = true;
            //
            // AltitudeToolForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1280, 800);
            Controls.Add(splitContainer1);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(900, 600);
            Name = "AltitudeToolForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Altitude Tool – map heights ↔ image";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusPos;
        private System.Windows.Forms.ToolStripStatusLabel statusInfo;
        private System.Windows.Forms.ToolStripProgressBar statusProgress;
        private System.Windows.Forms.ToolStripDropDownButton statusCancel;
        private System.Windows.Forms.ToolStripStatusLabel statusMessage;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabSource;
        private System.Windows.Forms.TabPage tabExport;
        private System.Windows.Forms.TabPage tabImport;
        private System.Windows.Forms.TabPage tabEdit;
        private System.Windows.Forms.TabPage tabInfo;
    }
}
