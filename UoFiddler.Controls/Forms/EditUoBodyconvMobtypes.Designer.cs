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

namespace UoFiddler.Controls.Forms
{
    partial class EditUoBodyconvMobtypes
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditUoBodyconvMobtypes));
            btLoadBodyconv = new System.Windows.Forms.Button();
            btLoadPfad = new System.Windows.Forms.Button();
            textBoxPfad = new System.Windows.Forms.TextBox();
            btmobtypes = new System.Windows.Forms.Button();
            richTextBoxEdit = new System.Windows.Forms.RichTextBox();
            contextMenuStripRichTextBoxEdit = new System.Windows.Forms.ContextMenuStrip(components);
            searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            lbSearchCount = new System.Windows.Forms.Label();
            btSaveFile = new System.Windows.Forms.Button();
            lbFileName = new System.Windows.Forms.Label();
            textBoxID = new System.Windows.Forms.TextBox();
            btBackwardText = new System.Windows.Forms.Button();
            lbID = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            textBoxBody = new System.Windows.Forms.TextBox();
            lbBody = new System.Windows.Forms.Label();
            contextMenuStripRichTextBoxEdit.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // btLoadBodyconv
            // 
            btLoadBodyconv.Location = new System.Drawing.Point(8, 57);
            btLoadBodyconv.Name = "btLoadBodyconv";
            btLoadBodyconv.Size = new System.Drawing.Size(71, 23);
            btLoadBodyconv.TabIndex = 0;
            btLoadBodyconv.Text = "Bodyconv";
            btLoadBodyconv.UseVisualStyleBackColor = true;
            btLoadBodyconv.Click += btLoadBodyconv_Click;
            // 
            // btLoadPfad
            // 
            btLoadPfad.Location = new System.Drawing.Point(8, 14);
            btLoadPfad.Name = "btLoadPfad";
            btLoadPfad.Size = new System.Drawing.Size(71, 23);
            btLoadPfad.TabIndex = 2;
            btLoadPfad.Text = "Pfad";
            btLoadPfad.UseVisualStyleBackColor = true;
            btLoadPfad.Click += btLoadPfad_Click;
            // 
            // textBoxPfad
            // 
            textBoxPfad.Location = new System.Drawing.Point(85, 15);
            textBoxPfad.Name = "textBoxPfad";
            textBoxPfad.Size = new System.Drawing.Size(173, 23);
            textBoxPfad.TabIndex = 3;
            // 
            // btmobtypes
            // 
            btmobtypes.Location = new System.Drawing.Point(8, 86);
            btmobtypes.Name = "btmobtypes";
            btmobtypes.Size = new System.Drawing.Size(71, 23);
            btmobtypes.TabIndex = 4;
            btmobtypes.Text = "mobtypes";
            btmobtypes.UseVisualStyleBackColor = true;
            btmobtypes.Click += btmobtypes_Click;
            // 
            // richTextBoxEdit
            // 
            richTextBoxEdit.ContextMenuStrip = contextMenuStripRichTextBoxEdit;
            richTextBoxEdit.Location = new System.Drawing.Point(297, 29);
            richTextBoxEdit.Name = "richTextBoxEdit";
            richTextBoxEdit.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            richTextBoxEdit.Size = new System.Drawing.Size(694, 529);
            richTextBoxEdit.TabIndex = 5;
            richTextBoxEdit.Text = "";
            // 
            // contextMenuStripRichTextBoxEdit
            // 
            contextMenuStripRichTextBoxEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { searchToolStripMenuItem });
            contextMenuStripRichTextBoxEdit.Name = "contextMenuStripRichTextBoxEdit";
            contextMenuStripRichTextBoxEdit.Size = new System.Drawing.Size(110, 26);
            // 
            // searchToolStripMenuItem
            // 
            searchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripTextBoxSearch });
            searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            searchToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            searchToolStripMenuItem.Text = "Search";
            searchToolStripMenuItem.Click += searchToolStripMenuItem_Click;
            // 
            // toolStripTextBoxSearch
            // 
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new System.Drawing.Size(100, 23);
            // 
            // lbSearchCount
            // 
            lbSearchCount.AutoSize = true;
            lbSearchCount.Location = new System.Drawing.Point(297, 11);
            lbSearchCount.Name = "lbSearchCount";
            lbSearchCount.Size = new System.Drawing.Size(46, 15);
            lbSearchCount.TabIndex = 6;
            lbSearchCount.Text = "Count :";
            // 
            // btSaveFile
            // 
            btSaveFile.Location = new System.Drawing.Point(85, 69);
            btSaveFile.Name = "btSaveFile";
            btSaveFile.Size = new System.Drawing.Size(75, 23);
            btSaveFile.TabIndex = 7;
            btSaveFile.Text = "Save";
            btSaveFile.UseVisualStyleBackColor = true;
            btSaveFile.Click += btSaveFile_Click;
            // 
            // lbFileName
            // 
            lbFileName.AutoSize = true;
            lbFileName.Location = new System.Drawing.Point(604, 11);
            lbFileName.Name = "lbFileName";
            lbFileName.Size = new System.Drawing.Size(45, 15);
            lbFileName.TabIndex = 8;
            lbFileName.Text = "Name :";
            // 
            // textBoxID
            // 
            textBoxID.Location = new System.Drawing.Point(183, 69);
            textBoxID.Name = "textBoxID";
            textBoxID.Size = new System.Drawing.Size(75, 23);
            textBoxID.TabIndex = 9;
            // 
            // btBackwardText
            // 
            btBackwardText.Location = new System.Drawing.Point(183, 146);
            btBackwardText.Name = "btBackwardText";
            btBackwardText.Size = new System.Drawing.Size(75, 23);
            btBackwardText.TabIndex = 10;
            btBackwardText.Text = "Backward";
            btBackwardText.UseVisualStyleBackColor = true;
            btBackwardText.Click += btBackwardText_Click;
            // 
            // lbID
            // 
            lbID.AutoSize = true;
            lbID.Location = new System.Drawing.Point(183, 51);
            lbID.Name = "lbID";
            lbID.Size = new System.Drawing.Size(24, 15);
            lbID.TabIndex = 11;
            lbID.Text = "ID :";
            // 
            // panel1
            // 
            panel1.Controls.Add(lbBody);
            panel1.Controls.Add(textBoxBody);
            panel1.Controls.Add(btSaveFile);
            panel1.Controls.Add(lbID);
            panel1.Controls.Add(btLoadBodyconv);
            panel1.Controls.Add(btLoadPfad);
            panel1.Controls.Add(btBackwardText);
            panel1.Controls.Add(textBoxPfad);
            panel1.Controls.Add(textBoxID);
            panel1.Controls.Add(btmobtypes);
            panel1.Location = new System.Drawing.Point(12, 29);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(279, 236);
            panel1.TabIndex = 12;
            // 
            // textBoxBody
            // 
            textBoxBody.Location = new System.Drawing.Point(183, 117);
            textBoxBody.Name = "textBoxBody";
            textBoxBody.Size = new System.Drawing.Size(75, 23);
            textBoxBody.TabIndex = 12;
            // 
            // lbBody
            // 
            lbBody.AutoSize = true;
            lbBody.Location = new System.Drawing.Point(183, 99);
            lbBody.Name = "lbBody";
            lbBody.Size = new System.Drawing.Size(50, 15);
            lbBody.TabIndex = 13;
            lbBody.Text = "Body-ID";
            // 
            // EditUoBodyconvMobtypes
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1003, 570);
            Controls.Add(panel1);
            Controls.Add(lbFileName);
            Controls.Add(lbSearchCount);
            Controls.Add(richTextBoxEdit);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "EditUoBodyconvMobtypes";
            Text = "Edit UoBodyconv or Mobtypes";
            contextMenuStripRichTextBoxEdit.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btLoadBodyconv;
        private System.Windows.Forms.Button btLoadPfad;
        private System.Windows.Forms.TextBox textBoxPfad;
        private System.Windows.Forms.Button btmobtypes;
        private System.Windows.Forms.RichTextBox richTextBoxEdit;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRichTextBoxEdit;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.Label lbSearchCount;
        private System.Windows.Forms.Button btSaveFile;
        private System.Windows.Forms.Label lbFileName;
        private System.Windows.Forms.TextBox textBoxID;
        private System.Windows.Forms.Button btBackwardText;
        private System.Windows.Forms.Label lbID;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBoxBody;
        private System.Windows.Forms.Label lbBody;
    }
}