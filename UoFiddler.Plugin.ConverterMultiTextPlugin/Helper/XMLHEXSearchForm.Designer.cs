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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Helper
{
    partial class XMLHEXSearchForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XMLHEXSearchForm));
            richTextBox = new System.Windows.Forms.RichTextBox();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            btHexSearch = new System.Windows.Forms.Button();
            btCorrectHex = new System.Windows.Forms.Button();
            btXMLCliboard = new System.Windows.Forms.Button();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripLabelName = new System.Windows.Forms.ToolStripLabel();
            toolStripLabelHexError = new System.Windows.Forms.ToolStripLabel();
            menuStrip1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // richTextBox
            // 
            richTextBox.Location = new System.Drawing.Point(12, 27);
            richTextBox.Name = "richTextBox";
            richTextBox.Size = new System.Drawing.Size(776, 411);
            richTextBox.TabIndex = 0;
            richTextBox.Text = "";
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(800, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadToolStripMenuItem, saveXMLToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            loadToolStripMenuItem.Text = "Load";
            loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
            // 
            // saveXMLToolStripMenuItem
            // 
            saveXMLToolStripMenuItem.Name = "saveXMLToolStripMenuItem";
            saveXMLToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            saveXMLToolStripMenuItem.Text = "Save";
            saveXMLToolStripMenuItem.Click += saveXMLToolStripMenuItem_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { searchToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(110, 26);
            // 
            // searchToolStripMenuItem
            // 
            searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            searchToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            searchToolStripMenuItem.Text = "Search";
            // 
            // btHexSearch
            // 
            btHexSearch.Location = new System.Drawing.Point(12, 444);
            btHexSearch.Name = "btHexSearch";
            btHexSearch.Size = new System.Drawing.Size(75, 23);
            btHexSearch.TabIndex = 3;
            btHexSearch.Text = "Hex Search";
            btHexSearch.UseVisualStyleBackColor = true;
            btHexSearch.Click += btHexSearch_Click;
            // 
            // btCorrectHex
            // 
            btCorrectHex.Location = new System.Drawing.Point(93, 444);
            btCorrectHex.Name = "btCorrectHex";
            btCorrectHex.Size = new System.Drawing.Size(75, 23);
            btCorrectHex.TabIndex = 4;
            btCorrectHex.Text = "correct";
            btCorrectHex.UseVisualStyleBackColor = true;
            btCorrectHex.Click += btCorrectHex_Click;
            // 
            // btXMLCliboard
            // 
            btXMLCliboard.Location = new System.Drawing.Point(174, 444);
            btXMLCliboard.Name = "btXMLCliboard";
            btXMLCliboard.Size = new System.Drawing.Size(75, 23);
            btXMLCliboard.TabIndex = 5;
            btXMLCliboard.Text = "Clipboard";
            btXMLCliboard.UseVisualStyleBackColor = true;
            btXMLCliboard.Click += btXMLCliboard_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelName, toolStripLabelHexError });
            toolStrip1.Location = new System.Drawing.Point(0, 466);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(800, 25);
            toolStrip1.TabIndex = 6;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabelName
            // 
            toolStripLabelName.Name = "toolStripLabelName";
            toolStripLabelName.Size = new System.Drawing.Size(31, 22);
            toolStripLabelName.Text = "XML";
            // 
            // toolStripLabelHexError
            // 
            toolStripLabelHexError.Name = "toolStripLabelHexError";
            toolStripLabelHexError.Size = new System.Drawing.Size(39, 22);
            toolStripLabelHexError.Text = "Name";
            // 
            // XMLHEXSearchForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 491);
            Controls.Add(toolStrip1);
            Controls.Add(btXMLCliboard);
            Controls.Add(btCorrectHex);
            Controls.Add(btHexSearch);
            Controls.Add(richTextBox);
            Controls.Add(menuStrip1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Name = "XMLHEXSearchForm";
            Text = "XMLHEXSearchForm";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveXMLToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.Button btHexSearch;
        private System.Windows.Forms.Button btCorrectHex;
        private System.Windows.Forms.Button btXMLCliboard;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabelName;
        private System.Windows.Forms.ToolStripLabel toolStripLabelHexError;
    }
}