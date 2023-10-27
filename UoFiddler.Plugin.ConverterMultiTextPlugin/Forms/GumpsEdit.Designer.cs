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
    partial class GumpsEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GumpsEdit));
            pictureBox = new System.Windows.Forms.PictureBox();
            contextMenuStripPicturebBox = new System.Windows.Forms.ContextMenuStrip(components);
            contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            showFreeIdsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            findNextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            addIDNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            listBox = new System.Windows.Forms.ListBox();
            IDLabelinortotal = new System.Windows.Forms.Label();
            LabelFreeIDs = new System.Windows.Forms.Label();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            IDLabel = new System.Windows.Forms.ToolStripLabel();
            SizeLabel = new System.Windows.Forms.ToolStripButton();
            panel1 = new System.Windows.Forms.Panel();
            topMenuToolStrip = new System.Windows.Forms.ToolStrip();
            IndexToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            searchByIdToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            contextMenuStrip.SuspendLayout();
            toolStrip1.SuspendLayout();
            panel1.SuspendLayout();
            topMenuToolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox
            // 
            pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox.ContextMenuStrip = contextMenuStripPicturebBox;
            pictureBox.Location = new System.Drawing.Point(15, 10);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(642, 484);
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            // 
            // contextMenuStripPicturebBox
            // 
            contextMenuStripPicturebBox.Name = "contextMenuStripPicturebBox";
            contextMenuStripPicturebBox.Size = new System.Drawing.Size(61, 4);
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyToolStripMenuItem, importToolStripMenuItem, toolStripSeparator3, removeToolStripMenuItem, replaceToolStripMenuItem, toolStripSeparator1, showFreeIdsToolStripMenuItem, findNextFreeSlotToolStripMenuItem, toolStripSeparator2, addIDNamesToolStripMenuItem, toolStripSeparator4, saveToolStripMenuItem });
            contextMenuStrip.Name = "contextMenuStrip1";
            contextMenuStrip.Size = new System.Drawing.Size(174, 204);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Image = Properties.Resources.Copy;
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Image = Properties.Resources.import;
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            importToolStripMenuItem.Text = "Import";
            importToolStripMenuItem.Click += importToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(170, 6);
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Image = Properties.Resources.Rotate;
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemove;
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.Image = Properties.Resources.reload;
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            replaceToolStripMenuItem.Text = "Replace";
            replaceToolStripMenuItem.Click += OnClickReplace;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(170, 6);
            // 
            // showFreeIdsToolStripMenuItem
            // 
            showFreeIdsToolStripMenuItem.Image = Properties.Resources.Mirror;
            showFreeIdsToolStripMenuItem.Name = "showFreeIdsToolStripMenuItem";
            showFreeIdsToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            showFreeIdsToolStripMenuItem.Text = "Show Free Ids";
            showFreeIdsToolStripMenuItem.Click += showFreeIdsToolStripMenuItem_Click;
            // 
            // findNextFreeSlotToolStripMenuItem
            // 
            findNextFreeSlotToolStripMenuItem.Image = Properties.Resources.Mirror1;
            findNextFreeSlotToolStripMenuItem.Name = "findNextFreeSlotToolStripMenuItem";
            findNextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            findNextFreeSlotToolStripMenuItem.Text = "Find Next Free Slot";
            findNextFreeSlotToolStripMenuItem.Click += OnClickFindFree;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(170, 6);
            // 
            // addIDNamesToolStripMenuItem
            // 
            addIDNamesToolStripMenuItem.Image = Properties.Resources.image_eine_maske_laden01;
            addIDNamesToolStripMenuItem.Name = "addIDNamesToolStripMenuItem";
            addIDNamesToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            addIDNamesToolStripMenuItem.Text = "Add ID Names";
            addIDNamesToolStripMenuItem.Click += addIDNamesToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(170, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Image = Properties.Resources.Save2;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // listBox
            // 
            listBox.ContextMenuStrip = contextMenuStrip;
            listBox.FormattingEnabled = true;
            listBox.ItemHeight = 15;
            listBox.Location = new System.Drawing.Point(12, 37);
            listBox.Name = "listBox";
            listBox.Size = new System.Drawing.Size(305, 484);
            listBox.TabIndex = 3;
            listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            listBox.KeyDown += GumpControl_KeyDown;
            // 
            // IDLabelinortotal
            // 
            IDLabelinortotal.AutoSize = true;
            IDLabelinortotal.Location = new System.Drawing.Point(12, 525);
            IDLabelinortotal.Name = "IDLabelinortotal";
            IDLabelinortotal.Size = new System.Drawing.Size(38, 15);
            IDLabelinortotal.TabIndex = 4;
            IDLabelinortotal.Text = "Total :";
            // 
            // LabelFreeIDs
            // 
            LabelFreeIDs.AutoSize = true;
            LabelFreeIDs.Location = new System.Drawing.Point(205, 525);
            LabelFreeIDs.Name = "LabelFreeIDs";
            LabelFreeIDs.Size = new System.Drawing.Size(29, 15);
            LabelFreeIDs.TabIndex = 5;
            LabelFreeIDs.Text = "Free";
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { IDLabel, SizeLabel });
            toolStrip1.Location = new System.Drawing.Point(0, 562);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1089, 25);
            toolStrip1.TabIndex = 6;
            toolStrip1.Text = "toolStrip1";
            // 
            // IDLabel
            // 
            IDLabel.Name = "IDLabel";
            IDLabel.Size = new System.Drawing.Size(18, 22);
            IDLabel.Text = "ID";
            // 
            // SizeLabel
            // 
            SizeLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            SizeLabel.Image = (System.Drawing.Image)resources.GetObject("SizeLabel.Image");
            SizeLabel.ImageTransparentColor = System.Drawing.Color.Magenta;
            SizeLabel.Name = "SizeLabel";
            SizeLabel.Size = new System.Drawing.Size(31, 22);
            SizeLabel.Text = "Size";
            // 
            // panel1
            // 
            panel1.Controls.Add(pictureBox);
            panel1.Location = new System.Drawing.Point(334, 27);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(683, 513);
            panel1.TabIndex = 7;
            // 
            // topMenuToolStrip
            // 
            topMenuToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { IndexToolStripLabel, searchByIdToolStripTextBox, toolStripButtonSave });
            topMenuToolStrip.Location = new System.Drawing.Point(0, 0);
            topMenuToolStrip.Name = "topMenuToolStrip";
            topMenuToolStrip.Size = new System.Drawing.Size(1089, 25);
            topMenuToolStrip.TabIndex = 8;
            topMenuToolStrip.Text = "toolStrip2";
            // 
            // IndexToolStripLabel
            // 
            IndexToolStripLabel.Name = "IndexToolStripLabel";
            IndexToolStripLabel.Size = new System.Drawing.Size(39, 22);
            IndexToolStripLabel.Text = "Index:";
            // 
            // searchByIdToolStripTextBox
            // 
            searchByIdToolStripTextBox.Name = "searchByIdToolStripTextBox";
            searchByIdToolStripTextBox.Size = new System.Drawing.Size(100, 25);
            searchByIdToolStripTextBox.TextChanged += SearchByIdToolStripTextBox_TextChanged;
            // 
            // toolStripButtonSave
            // 
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonSave.Image = Properties.Resources.Save2;
            toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Size = new System.Drawing.Size(23, 22);
            toolStripButtonSave.Text = "toolStripButton1";
            toolStripButtonSave.Click += OnClickSave;
            // 
            // GumpsEdit
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1089, 587);
            Controls.Add(topMenuToolStrip);
            Controls.Add(panel1);
            Controls.Add(toolStrip1);
            Controls.Add(LabelFreeIDs);
            Controls.Add(IDLabelinortotal);
            Controls.Add(listBox);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "GumpsEdit";
            Text = "Gumps Edit";
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            contextMenuStrip.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            topMenuToolStrip.ResumeLayout(false);
            topMenuToolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.Label IDLabelinortotal;
        private System.Windows.Forms.Label LabelFreeIDs;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel IDLabel;
        private System.Windows.Forms.ToolStripButton SizeLabel;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStrip topMenuToolStrip;
        private System.Windows.Forms.ToolStripLabel IndexToolStripLabel;
        private System.Windows.Forms.ToolStripTextBox searchByIdToolStripTextBox;
        private System.Windows.Forms.ToolStripMenuItem showFreeIdsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addIDNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextFreeSlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPicturebBox;
    }
}