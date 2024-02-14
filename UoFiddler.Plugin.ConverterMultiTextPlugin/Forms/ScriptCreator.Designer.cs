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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptCreator));
            panelBaseScript = new System.Windows.Forms.Panel();
            tabControlItems = new System.Windows.Forms.TabControl();
            tabPageItems = new System.Windows.Forms.TabPage();
            tabPageAnimation = new System.Windows.Forms.TabPage();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            panelBaseScript.SuspendLayout();
            tabControlItems.SuspendLayout();
            SuspendLayout();
            // 
            // panelBaseScript
            // 
            panelBaseScript.Controls.Add(tabControlItems);
            panelBaseScript.Location = new System.Drawing.Point(12, 43);
            panelBaseScript.Name = "panelBaseScript";
            panelBaseScript.Size = new System.Drawing.Size(776, 612);
            panelBaseScript.TabIndex = 0;
            // 
            // tabControlItems
            // 
            tabControlItems.Controls.Add(tabPageItems);
            tabControlItems.Controls.Add(tabPageAnimation);
            tabControlItems.Location = new System.Drawing.Point(3, 3);
            tabControlItems.Name = "tabControlItems";
            tabControlItems.SelectedIndex = 0;
            tabControlItems.Size = new System.Drawing.Size(770, 606);
            tabControlItems.TabIndex = 0;
            // 
            // tabPageItems
            // 
            tabPageItems.Location = new System.Drawing.Point(4, 24);
            tabPageItems.Name = "tabPageItems";
            tabPageItems.Padding = new System.Windows.Forms.Padding(3);
            tabPageItems.Size = new System.Drawing.Size(762, 578);
            tabPageItems.TabIndex = 0;
            tabPageItems.Text = "Items";
            tabPageItems.UseVisualStyleBackColor = true;
            // 
            // tabPageAnimation
            // 
            tabPageAnimation.Location = new System.Drawing.Point(4, 24);
            tabPageAnimation.Name = "tabPageAnimation";
            tabPageAnimation.Padding = new System.Windows.Forms.Padding(3);
            tabPageAnimation.Size = new System.Drawing.Size(762, 578);
            tabPageAnimation.TabIndex = 1;
            tabPageAnimation.Text = "Animation";
            tabPageAnimation.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(800, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // ScriptCreator
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 667);
            Controls.Add(toolStrip1);
            Controls.Add(panelBaseScript);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ScriptCreator";
            Text = "ScriptCreator";
            panelBaseScript.ResumeLayout(false);
            tabControlItems.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panelBaseScript;
        private System.Windows.Forms.TabControl tabControlItems;
        private System.Windows.Forms.TabPage tabPageItems;
        private System.Windows.Forms.TabPage tabPageAnimation;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}