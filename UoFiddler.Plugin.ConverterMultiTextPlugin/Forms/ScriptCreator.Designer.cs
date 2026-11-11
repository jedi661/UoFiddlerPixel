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
    partial class ScriptCreator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptCreator));
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            previewTimer = new System.Windows.Forms.Timer(components);
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            statusMessage = new System.Windows.Forms.ToolStripStatusLabel();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabGenerator = new System.Windows.Forms.TabPage();
            tabTemplates = new System.Windows.Forms.TabPage();
            tabClientFiles = new System.Windows.Forms.TabPage();
            statusStrip1.SuspendLayout();
            tabControl1.SuspendLayout();
            SuspendLayout();
            //
            // previewTimer
            //
            previewTimer.Interval = 250;
            previewTimer.Tick += PreviewTimer_Tick;
            //
            // toolStrip1
            //
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1300, 25);
            toolStrip1.TabIndex = 0;
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusMessage });
            statusStrip1.Location = new System.Drawing.Point(0, 798);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1300, 22);
            statusStrip1.TabIndex = 2;
            //
            // statusMessage
            //
            statusMessage.Name = "statusMessage";
            statusMessage.Spring = true;
            statusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // tabControl1
            //
            tabControl1.Controls.Add(tabGenerator);
            tabControl1.Controls.Add(tabTemplates);
            tabControl1.Controls.Add(tabClientFiles);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 25);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(1300, 773);
            tabControl1.TabIndex = 1;
            //
            // tab pages
            //
            tabGenerator.Name = "tabGenerator";
            tabGenerator.Text = "Generator";
            tabGenerator.UseVisualStyleBackColor = true;
            tabTemplates.Name = "tabTemplates";
            tabTemplates.Text = "Templates";
            tabTemplates.UseVisualStyleBackColor = true;
            tabClientFiles.Name = "tabClientFiles";
            tabClientFiles.Text = "Client text files";
            tabClientFiles.UseVisualStyleBackColor = true;
            //
            // ScriptCreator
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1300, 820);
            Controls.Add(tabControl1);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(1000, 600);
            Name = "ScriptCreator";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Script Creator";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tabControl1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer previewTimer;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusMessage;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGenerator;
        private System.Windows.Forms.TabPage tabTemplates;
        private System.Windows.Forms.TabPage tabClientFiles;
    }
}
