﻿/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Controls.UserControls
{
    partial class AnimDataControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            treeView1 = new System.Windows.Forms.TreeView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            AddTextBox = new System.Windows.Forms.ToolStripTextBox();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            groupBox1 = new System.Windows.Forms.GroupBox();
            button1 = new System.Windows.Forms.Button();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            btnOnClickImport = new System.Windows.Forms.Button();
            btnOnClickExport = new System.Windows.Forms.Button();
            button6 = new System.Windows.Forms.Button();
            groupBox4 = new System.Windows.Forms.GroupBox();
            button5 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            checkBoxRelative = new System.Windows.Forms.CheckBox();
            button3 = new System.Windows.Forms.Button();
            textBoxAddFrame = new System.Windows.Forms.TextBox();
            treeViewFrames = new System.Windows.Forms.TreeView();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label2 = new System.Windows.Forms.Label();
            numericUpDownFrameDelay = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            numericUpDownStartDelay = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFrameDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownStartDelay).BeginInit();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(treeView1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new System.Drawing.Size(857, 587);
            splitContainer1.SplitterDistance = 246;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // treeView1
            // 
            treeView1.ContextMenuStrip = contextMenuStrip1;
            treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            treeView1.HideSelection = false;
            treeView1.Location = new System.Drawing.Point(0, 0);
            treeView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            treeView1.Name = "treeView1";
            treeView1.Size = new System.Drawing.Size(246, 587);
            treeView1.TabIndex = 0;
            treeView1.AfterSelect += AfterNodeSelect;
            treeView1.NodeMouseClick += OnClickNode;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { addToolStripMenuItem, removeToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(118, 48);
            contextMenuStrip1.Opening += ContextMenuStrip1_Opening;
            // 
            // addToolStripMenuItem
            // 
            addToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { AddTextBox });
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            addToolStripMenuItem.Text = "Add";
            // 
            // AddTextBox
            // 
            AddTextBox.Name = "AddTextBox";
            AddTextBox.Size = new System.Drawing.Size(100, 23);
            AddTextBox.KeyDown += OnKeyDownAdd;
            AddTextBox.TextChanged += OnTextChangeAdd;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemoveAnim;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(groupBox1);
            splitContainer2.Panel1MinSize = 100;
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(groupBox3);
            splitContainer2.Panel2.Controls.Add(groupBox4);
            splitContainer2.Panel2.Controls.Add(groupBox2);
            splitContainer2.Size = new System.Drawing.Size(606, 587);
            splitContainer2.SplitterDistance = 334;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 6;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(pictureBox1);
            groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox1.Location = new System.Drawing.Point(0, 0);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(334, 587);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Preview";
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            button1.Location = new System.Drawing.Point(69, 554);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(88, 27);
            button1.TabIndex = 1;
            button1.Text = "Start/Stop";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnClickStartStop;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox1.Location = new System.Drawing.Point(4, 19);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(326, 565);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnOnClickImport);
            groupBox3.Controls.Add(btnOnClickExport);
            groupBox3.Controls.Add(button6);
            groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            groupBox3.Location = new System.Drawing.Point(0, 518);
            groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Size = new System.Drawing.Size(267, 61);
            groupBox3.TabIndex = 6;
            groupBox3.TabStop = false;
            // 
            // btnOnClickImport
            // 
            btnOnClickImport.Location = new System.Drawing.Point(79, 22);
            btnOnClickImport.Name = "btnOnClickImport";
            btnOnClickImport.Size = new System.Drawing.Size(61, 27);
            btnOnClickImport.TabIndex = 8;
            btnOnClickImport.Text = "Import...";
            btnOnClickImport.UseVisualStyleBackColor = true;
            btnOnClickImport.Click += btnOnClickImport_Click;
            // 
            // btnOnClickExport
            // 
            btnOnClickExport.Location = new System.Drawing.Point(14, 22);
            btnOnClickExport.Name = "btnOnClickExport";
            btnOnClickExport.Size = new System.Drawing.Size(59, 27);
            btnOnClickExport.TabIndex = 7;
            btnOnClickExport.Text = "Export...";
            btnOnClickExport.UseVisualStyleBackColor = true;
            btnOnClickExport.Click += btnOnClickExport_Click;
            // 
            // button6
            // 
            button6.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button6.Location = new System.Drawing.Point(150, 22);
            button6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button6.Name = "button6";
            button6.Size = new System.Drawing.Size(49, 27);
            button6.TabIndex = 6;
            button6.Text = "Save";
            button6.UseVisualStyleBackColor = true;
            button6.Click += OnClickSave;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(button5);
            groupBox4.Controls.Add(button2);
            groupBox4.Controls.Add(button4);
            groupBox4.Controls.Add(checkBoxRelative);
            groupBox4.Controls.Add(button3);
            groupBox4.Controls.Add(textBoxAddFrame);
            groupBox4.Controls.Add(treeViewFrames);
            groupBox4.Dock = System.Windows.Forms.DockStyle.Top;
            groupBox4.Location = new System.Drawing.Point(0, 95);
            groupBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox4.Size = new System.Drawing.Size(267, 423);
            groupBox4.TabIndex = 4;
            groupBox4.TabStop = false;
            groupBox4.Text = "Frames";
            // 
            // button5
            // 
            button5.Location = new System.Drawing.Point(108, 385);
            button5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(88, 27);
            button5.TabIndex = 4;
            button5.Text = "Remove";
            button5.UseVisualStyleBackColor = true;
            button5.Click += OnClickRemove;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(14, 385);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(88, 27);
            button2.TabIndex = 2;
            button2.Text = "Add";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnClickAdd;
            // 
            // button4
            // 
            button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            button4.Location = new System.Drawing.Point(227, 178);
            button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(26, 25);
            button4.TabIndex = 3;
            button4.Text = "▼";
            button4.UseVisualStyleBackColor = true;
            button4.Click += OnClickFrameDown;
            // 
            // checkBoxRelative
            // 
            checkBoxRelative.AutoSize = true;
            checkBoxRelative.Location = new System.Drawing.Point(108, 358);
            checkBoxRelative.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxRelative.Name = "checkBoxRelative";
            checkBoxRelative.Size = new System.Drawing.Size(67, 19);
            checkBoxRelative.TabIndex = 1;
            checkBoxRelative.Text = "Relative";
            checkBoxRelative.UseVisualStyleBackColor = true;
            checkBoxRelative.CheckedChanged += OnCheckChange;
            // 
            // button3
            // 
            button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            button3.Location = new System.Drawing.Point(227, 147);
            button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(26, 25);
            button3.TabIndex = 2;
            button3.Text = "▲";
            button3.UseVisualStyleBackColor = true;
            button3.Click += OnClickFrameUp;
            // 
            // textBoxAddFrame
            // 
            textBoxAddFrame.Location = new System.Drawing.Point(14, 355);
            textBoxAddFrame.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxAddFrame.Name = "textBoxAddFrame";
            textBoxAddFrame.Size = new System.Drawing.Size(83, 23);
            textBoxAddFrame.TabIndex = 0;
            textBoxAddFrame.TextChanged += OnTextChanged;
            // 
            // treeViewFrames
            // 
            treeViewFrames.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            treeViewFrames.HideSelection = false;
            treeViewFrames.Location = new System.Drawing.Point(14, 22);
            treeViewFrames.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            treeViewFrames.Name = "treeViewFrames";
            treeViewFrames.Size = new System.Drawing.Size(211, 326);
            treeViewFrames.TabIndex = 1;
            treeViewFrames.AfterSelect += AfterSelectTreeViewFrames;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(numericUpDownFrameDelay);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(numericUpDownStartDelay);
            groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            groupBox2.Location = new System.Drawing.Point(0, 0);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(267, 95);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Data";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(10, 59);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(72, 15);
            label2.TabIndex = 3;
            label2.Text = "Frame Delay";
            // 
            // numericUpDownFrameDelay
            // 
            numericUpDownFrameDelay.Location = new System.Drawing.Point(103, 57);
            numericUpDownFrameDelay.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDownFrameDelay.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDownFrameDelay.Name = "numericUpDownFrameDelay";
            numericUpDownFrameDelay.Size = new System.Drawing.Size(74, 23);
            numericUpDownFrameDelay.TabIndex = 2;
            numericUpDownFrameDelay.ValueChanged += OnValueChangedFrameDelay;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(10, 29);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(63, 15);
            label1.TabIndex = 1;
            label1.Text = "Start Delay";
            // 
            // numericUpDownStartDelay
            // 
            numericUpDownStartDelay.Location = new System.Drawing.Point(103, 27);
            numericUpDownStartDelay.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDownStartDelay.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDownStartDelay.Name = "numericUpDownStartDelay";
            numericUpDownStartDelay.Size = new System.Drawing.Size(74, 23);
            numericUpDownStartDelay.TabIndex = 0;
            numericUpDownStartDelay.ValueChanged += OnValueChangedStartDelay;
            // 
            // AnimDataControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AnimDataControl";
            Size = new System.Drawing.Size(857, 587);
            Load += OnLoad;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFrameDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownStartDelay).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolStripTextBox AddTextBox;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownFrameDelay;
        private System.Windows.Forms.NumericUpDown numericUpDownStartDelay;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TreeView treeViewFrames;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBoxRelative;
        private System.Windows.Forms.TextBox textBoxAddFrame;
        private System.Windows.Forms.Button btnOnClickExport;
        private System.Windows.Forms.Button btnOnClickImport;
    }
}
