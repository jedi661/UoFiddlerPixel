// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Forms
{
    partial class LineConverterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LineConverterForm));
            TextBoxInputOutput = new System.Windows.Forms.TextBox();
            ContextMenuStripCovert = new System.Windows.Forms.ContextMenuStrip(components);
            searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ToolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            BtnConvert = new System.Windows.Forms.Button();
            BtnConvert2 = new System.Windows.Forms.Button();
            lbCounter = new System.Windows.Forms.Label();
            BtnClear = new System.Windows.Forms.Button();
            BtnCopy = new System.Windows.Forms.Button();
            BtnRestore = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            chkAddSpaces = new System.Windows.Forms.CheckBox();
            BtnConvertParagraphsToLines2WithoutComments = new System.Windows.Forms.Button();
            BtnConvertWithBlocks = new System.Windows.Forms.Button();
            chkBlockSize4000 = new System.Windows.Forms.CheckBox();
            lblBlockCount = new System.Windows.Forms.Label();
            LbInfo = new System.Windows.Forms.Label();
            TBBlockCount = new System.Windows.Forms.TextBox();
            lbBlockSize = new System.Windows.Forms.Label();
            BtnBullBlockSize = new System.Windows.Forms.Button();
            LbCharactercounter = new System.Windows.Forms.Label();
            LbInfo2 = new System.Windows.Forms.Label();
            ContextMenuStripCovert.SuspendLayout();
            SuspendLayout();
            // 
            // TextBoxInputOutput
            // 
            TextBoxInputOutput.ContextMenuStrip = ContextMenuStripCovert;
            TextBoxInputOutput.Location = new System.Drawing.Point(12, 89);
            TextBoxInputOutput.MaxLength = 2000000;
            TextBoxInputOutput.Multiline = true;
            TextBoxInputOutput.Name = "TextBoxInputOutput";
            TextBoxInputOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            TextBoxInputOutput.Size = new System.Drawing.Size(993, 318);
            TextBoxInputOutput.TabIndex = 0;
            TextBoxInputOutput.TextChanged += TextBoxInputOutput_TextChanged;
            // 
            // ContextMenuStripCovert
            // 
            ContextMenuStripCovert.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { searchToolStripMenuItem, loadToolStripMenuItem, saveToolStripMenuItem });
            ContextMenuStripCovert.Name = "ContextMenuStripCovert";
            ContextMenuStripCovert.Size = new System.Drawing.Size(110, 70);
            // 
            // searchToolStripMenuItem
            // 
            searchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolStripTextBoxSearch });
            searchToolStripMenuItem.Image = Properties.Resources.Mirror;
            searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            searchToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            searchToolStripMenuItem.Text = "Search";
            // 
            // ToolStripTextBoxSearch
            // 
            ToolStripTextBoxSearch.Name = "ToolStripTextBoxSearch";
            ToolStripTextBoxSearch.Size = new System.Drawing.Size(100, 23);
            ToolStripTextBoxSearch.TextChanged += ToolStripTextBoxSearch_TextChanged;
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Image = Properties.Resources.Directory;
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            loadToolStripMenuItem.Text = "Load";
            loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Image = Properties.Resources.notepad;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // BtnConvert
            // 
            BtnConvert.Location = new System.Drawing.Point(12, 422);
            BtnConvert.Name = "BtnConvert";
            BtnConvert.Size = new System.Drawing.Size(75, 23);
            BtnConvert.TabIndex = 1;
            BtnConvert.Text = "Convert";
            BtnConvert.UseVisualStyleBackColor = true;
            BtnConvert.Click += BtnConvert_Click;
            // 
            // BtnConvert2
            // 
            BtnConvert2.Location = new System.Drawing.Point(12, 451);
            BtnConvert2.Name = "BtnConvert2";
            BtnConvert2.Size = new System.Drawing.Size(75, 23);
            BtnConvert2.TabIndex = 2;
            BtnConvert2.Text = "Convert";
            BtnConvert2.UseVisualStyleBackColor = true;
            BtnConvert2.Click += BtnConvert2_Click;
            // 
            // lbCounter
            // 
            lbCounter.AutoSize = true;
            lbCounter.Location = new System.Drawing.Point(425, 426);
            lbCounter.Name = "lbCounter";
            lbCounter.Size = new System.Drawing.Size(40, 15);
            lbCounter.TabIndex = 3;
            lbCounter.Text = "Count";
            // 
            // BtnClear
            // 
            BtnClear.Location = new System.Drawing.Point(182, 422);
            BtnClear.Name = "BtnClear";
            BtnClear.Size = new System.Drawing.Size(75, 23);
            BtnClear.TabIndex = 4;
            BtnClear.Text = "Clear";
            BtnClear.UseVisualStyleBackColor = true;
            BtnClear.Click += BtnClear_Click;
            // 
            // BtnCopy
            // 
            BtnCopy.Location = new System.Drawing.Point(263, 422);
            BtnCopy.Name = "BtnCopy";
            BtnCopy.Size = new System.Drawing.Size(75, 23);
            BtnCopy.TabIndex = 5;
            BtnCopy.Text = "Copy";
            BtnCopy.UseVisualStyleBackColor = true;
            BtnCopy.Click += BtnCopy_Click;
            // 
            // BtnRestore
            // 
            BtnRestore.Location = new System.Drawing.Point(344, 422);
            BtnRestore.Name = "BtnRestore";
            BtnRestore.Size = new System.Drawing.Size(75, 23);
            BtnRestore.TabIndex = 6;
            BtnRestore.Text = "Restore";
            BtnRestore.UseVisualStyleBackColor = true;
            BtnRestore.Click += BtnRestore_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(994, 75);
            label1.TabIndex = 7;
            label1.Text = resources.GetString("label1.Text");
            // 
            // chkAddSpaces
            // 
            chkAddSpaces.AutoSize = true;
            chkAddSpaces.Location = new System.Drawing.Point(93, 425);
            chkAddSpaces.Name = "chkAddSpaces";
            chkAddSpaces.Size = new System.Drawing.Size(87, 19);
            chkAddSpaces.TabIndex = 8;
            chkAddSpaces.Text = "Add Spaces";
            chkAddSpaces.UseVisualStyleBackColor = true;
            // 
            // BtnConvertParagraphsToLines2WithoutComments
            // 
            BtnConvertParagraphsToLines2WithoutComments.Location = new System.Drawing.Point(12, 497);
            BtnConvertParagraphsToLines2WithoutComments.Name = "BtnConvertParagraphsToLines2WithoutComments";
            BtnConvertParagraphsToLines2WithoutComments.Size = new System.Drawing.Size(75, 23);
            BtnConvertParagraphsToLines2WithoutComments.TabIndex = 9;
            BtnConvertParagraphsToLines2WithoutComments.Text = "Convert";
            BtnConvertParagraphsToLines2WithoutComments.UseVisualStyleBackColor = true;
            BtnConvertParagraphsToLines2WithoutComments.Click += BtnConvertParagraphsToLines2WithoutComments_Click;
            // 
            // BtnConvertWithBlocks
            // 
            BtnConvertWithBlocks.Location = new System.Drawing.Point(89, 497);
            BtnConvertWithBlocks.Name = "BtnConvertWithBlocks";
            BtnConvertWithBlocks.Size = new System.Drawing.Size(150, 23);
            BtnConvertWithBlocks.TabIndex = 10;
            BtnConvertWithBlocks.Text = "Convert Block Size 8000";
            BtnConvertWithBlocks.UseVisualStyleBackColor = true;
            BtnConvertWithBlocks.Click += BtnConvertWithBlocks_Click;
            // 
            // chkBlockSize4000
            // 
            chkBlockSize4000.AutoSize = true;
            chkBlockSize4000.Location = new System.Drawing.Point(242, 500);
            chkBlockSize4000.Name = "chkBlockSize4000";
            chkBlockSize4000.Size = new System.Drawing.Size(105, 19);
            chkBlockSize4000.TabIndex = 11;
            chkBlockSize4000.Text = "Block Size 4000";
            chkBlockSize4000.UseVisualStyleBackColor = true;
            // 
            // lblBlockCount
            // 
            lblBlockCount.AutoSize = true;
            lblBlockCount.Location = new System.Drawing.Point(425, 501);
            lblBlockCount.Name = "lblBlockCount";
            lblBlockCount.Size = new System.Drawing.Size(72, 15);
            lblBlockCount.TabIndex = 12;
            lblBlockCount.Text = "Block Count";
            // 
            // LbInfo
            // 
            LbInfo.AutoSize = true;
            LbInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            LbInfo.Location = new System.Drawing.Point(12, 479);
            LbInfo.Name = "LbInfo";
            LbInfo.Size = new System.Drawing.Size(155, 15);
            LbInfo.TabIndex = 13;
            LbInfo.Text = "Without a comment block:";
            // 
            // TBBlockCount
            // 
            TBBlockCount.Location = new System.Drawing.Point(353, 496);
            TBBlockCount.Name = "TBBlockCount";
            TBBlockCount.Size = new System.Drawing.Size(66, 23);
            TBBlockCount.TabIndex = 14;
            // 
            // lbBlockSize
            // 
            lbBlockSize.AutoSize = true;
            lbBlockSize.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lbBlockSize.Location = new System.Drawing.Point(350, 478);
            lbBlockSize.Name = "lbBlockSize";
            lbBlockSize.Size = new System.Drawing.Size(107, 15);
            lbBlockSize.TabIndex = 15;
            lbBlockSize.Text = "Block Size manual";
            // 
            // BtnBullBlockSize
            // 
            BtnBullBlockSize.Location = new System.Drawing.Point(182, 471);
            BtnBullBlockSize.Name = "BtnBullBlockSize";
            BtnBullBlockSize.Size = new System.Drawing.Size(156, 23);
            BtnBullBlockSize.TabIndex = 16;
            BtnBullBlockSize.Text = "Copy Clipboard Delete";
            BtnBullBlockSize.UseVisualStyleBackColor = true;
            BtnBullBlockSize.Click += BtnBullBlockSize_Click;
            // 
            // LbCharactercounter
            // 
            LbCharactercounter.AutoSize = true;
            LbCharactercounter.Location = new System.Drawing.Point(936, 426);
            LbCharactercounter.Name = "LbCharactercounter";
            LbCharactercounter.Size = new System.Drawing.Size(43, 15);
            LbCharactercounter.TabIndex = 17;
            LbCharactercounter.Text = "Count:";
            // 
            // LbInfo2
            // 
            LbInfo2.AutoSize = true;
            LbInfo2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            LbInfo2.Location = new System.Drawing.Point(820, 426);
            LbInfo2.Name = "LbInfo2";
            LbInfo2.Size = new System.Drawing.Size(115, 15);
            LbInfo2.TabIndex = 18;
            LbInfo2.Text = "Character Counter :";
            // 
            // LineConverterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1017, 528);
            Controls.Add(LbInfo2);
            Controls.Add(LbCharactercounter);
            Controls.Add(BtnBullBlockSize);
            Controls.Add(lbBlockSize);
            Controls.Add(TBBlockCount);
            Controls.Add(LbInfo);
            Controls.Add(lblBlockCount);
            Controls.Add(chkBlockSize4000);
            Controls.Add(BtnConvertWithBlocks);
            Controls.Add(BtnConvertParagraphsToLines2WithoutComments);
            Controls.Add(chkAddSpaces);
            Controls.Add(label1);
            Controls.Add(BtnRestore);
            Controls.Add(BtnCopy);
            Controls.Add(BtnClear);
            Controls.Add(lbCounter);
            Controls.Add(BtnConvert2);
            Controls.Add(BtnConvert);
            Controls.Add(TextBoxInputOutput);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "LineConverterForm";
            Text = "Paragraph-to-Line Converter - AI shrink characters";
            ContextMenuStripCovert.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox TextBoxInputOutput;
        private System.Windows.Forms.Button BtnConvert;
        private System.Windows.Forms.Button BtnConvert2;
        private System.Windows.Forms.Label lbCounter;
        private System.Windows.Forms.Button BtnClear;
        private System.Windows.Forms.Button BtnCopy;
        private System.Windows.Forms.Button BtnRestore;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkAddSpaces;
        private System.Windows.Forms.Button BtnConvertParagraphsToLines2WithoutComments;
        private System.Windows.Forms.Button BtnConvertWithBlocks;
        private System.Windows.Forms.CheckBox chkBlockSize4000;
        private System.Windows.Forms.Label lblBlockCount;
        private System.Windows.Forms.Label LbInfo;
        private System.Windows.Forms.TextBox TBBlockCount;
        private System.Windows.Forms.Label lbBlockSize;
        private System.Windows.Forms.ContextMenuStrip ContextMenuStripCovert;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox ToolStripTextBoxSearch;
        private System.Windows.Forms.Button BtnBullBlockSize;
        private System.Windows.Forms.Label LbCharactercounter;
        private System.Windows.Forms.Label LbInfo2;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
    }
}