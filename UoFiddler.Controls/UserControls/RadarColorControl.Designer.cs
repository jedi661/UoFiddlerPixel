/***************************************************************************
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
    partial class RadarColorControl
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
            treeViewItem = new ColorTreeView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            selectInItemsTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            selectInTiledataTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            treeViewLand = new System.Windows.Forms.TreeView();
            contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(components);
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            pictureBoxArt = new System.Windows.Forms.PictureBox();
            pictureBoxColor = new System.Windows.Forms.PictureBox();
            splitContainer5 = new System.Windows.Forms.SplitContainer();
            splitContainer6 = new System.Windows.Forms.SplitContainer();
            tabControl2 = new System.Windows.Forms.TabControl();
            tabPage3 = new System.Windows.Forms.TabPage();
            tabPage4 = new System.Windows.Forms.TabPage();
            btupdateTreeView = new System.Windows.Forms.Button();
            label6 = new System.Windows.Forms.Label();
            LabelTildataNameItemsLand = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            textBoxPhotoshopCode = new System.Windows.Forms.TextBox();
            textBoxHexCode = new System.Windows.Forms.TextBox();
            checkBoxHexCode = new System.Windows.Forms.CheckBox();
            LabelColorCode = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            progressBar2 = new System.Windows.Forms.ProgressBar();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            button6 = new System.Windows.Forms.Button();
            button5 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            numericUpDownShortCol = new System.Windows.Forms.NumericUpDown();
            textBoxMeanFrom = new System.Windows.Forms.TextBox();
            textBoxMeanTo = new System.Windows.Forms.TextBox();
            button3 = new System.Windows.Forms.Button();
            numericUpDownB = new System.Windows.Forms.NumericUpDown();
            numericUpDownG = new System.Windows.Forms.NumericUpDown();
            numericUpDownR = new System.Windows.Forms.NumericUpDown();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            buttonMean = new System.Windows.Forms.Button();
            contextMenuStrip1.SuspendLayout();
            contextMenuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxArt).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxColor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer5).BeginInit();
            splitContainer5.Panel1.SuspendLayout();
            splitContainer5.Panel2.SuspendLayout();
            splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer6).BeginInit();
            splitContainer6.Panel1.SuspendLayout();
            splitContainer6.Panel2.SuspendLayout();
            splitContainer6.SuspendLayout();
            tabControl2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownShortCol).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownG).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownR).BeginInit();
            SuspendLayout();
            // 
            // treeViewItem
            // 
            treeViewItem.ContextMenuStrip = contextMenuStrip1;
            treeViewItem.Dock = System.Windows.Forms.DockStyle.Fill;
            treeViewItem.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
            treeViewItem.HideSelection = false;
            treeViewItem.Location = new System.Drawing.Point(4, 4);
            treeViewItem.Margin = new System.Windows.Forms.Padding(4);
            treeViewItem.Name = "treeViewItem";
            treeViewItem.Size = new System.Drawing.Size(228, 193);
            treeViewItem.TabIndex = 0;
            treeViewItem.AfterSelect += AfterSelectTreeViewItem;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { selectInItemsTabToolStripMenuItem, selectInTiledataTabToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(183, 48);
            // 
            // selectInItemsTabToolStripMenuItem
            // 
            selectInItemsTabToolStripMenuItem.Name = "selectInItemsTabToolStripMenuItem";
            selectInItemsTabToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            selectInItemsTabToolStripMenuItem.Text = "Select in Items tab";
            selectInItemsTabToolStripMenuItem.Click += OnClickSelectItemsTab;
            // 
            // selectInTiledataTabToolStripMenuItem
            // 
            selectInTiledataTabToolStripMenuItem.Name = "selectInTiledataTabToolStripMenuItem";
            selectInTiledataTabToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            selectInTiledataTabToolStripMenuItem.Text = "Select in Tiledata tab";
            selectInTiledataTabToolStripMenuItem.Click += OnClickSelectItemTiledataTab;
            // 
            // treeViewLand
            // 
            treeViewLand.ContextMenuStrip = contextMenuStrip2;
            treeViewLand.Dock = System.Windows.Forms.DockStyle.Fill;
            treeViewLand.HideSelection = false;
            treeViewLand.Location = new System.Drawing.Point(4, 4);
            treeViewLand.Margin = new System.Windows.Forms.Padding(4);
            treeViewLand.Name = "treeViewLand";
            treeViewLand.Size = new System.Drawing.Size(228, 193);
            treeViewLand.TabIndex = 0;
            treeViewLand.AfterSelect += AfterSelectTreeViewLand;
            // 
            // contextMenuStrip2
            // 
            contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2 });
            contextMenuStrip2.Name = "contextMenuStrip1";
            contextMenuStrip2.Size = new System.Drawing.Size(189, 48);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(188, 22);
            toolStripMenuItem1.Text = "Select in Landtiles tab";
            toolStripMenuItem1.Click += OnClickSelectLandTilesTab;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(188, 22);
            toolStripMenuItem2.Text = "Select in Tiledata tab";
            toolStripMenuItem2.Click += OnClickSelectLandTiledataTab;
            // 
            // pictureBoxArt
            // 
            pictureBoxArt.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxArt.Location = new System.Drawing.Point(0, 0);
            pictureBoxArt.Margin = new System.Windows.Forms.Padding(4);
            pictureBoxArt.Name = "pictureBoxArt";
            pictureBoxArt.Size = new System.Drawing.Size(244, 154);
            pictureBoxArt.TabIndex = 0;
            pictureBoxArt.TabStop = false;
            // 
            // pictureBoxColor
            // 
            pictureBoxColor.Location = new System.Drawing.Point(4, 4);
            pictureBoxColor.Margin = new System.Windows.Forms.Padding(4);
            pictureBoxColor.Name = "pictureBoxColor";
            pictureBoxColor.Size = new System.Drawing.Size(161, 116);
            pictureBoxColor.TabIndex = 0;
            pictureBoxColor.TabStop = false;
            // 
            // splitContainer5
            // 
            splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer5.Location = new System.Drawing.Point(0, 0);
            splitContainer5.Margin = new System.Windows.Forms.Padding(4);
            splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            splitContainer5.Panel1.Controls.Add(splitContainer6);
            // 
            // splitContainer5.Panel2
            // 
            splitContainer5.Panel2.Controls.Add(btupdateTreeView);
            splitContainer5.Panel2.Controls.Add(label6);
            splitContainer5.Panel2.Controls.Add(LabelTildataNameItemsLand);
            splitContainer5.Panel2.Controls.Add(label5);
            splitContainer5.Panel2.Controls.Add(label4);
            splitContainer5.Panel2.Controls.Add(label3);
            splitContainer5.Panel2.Controls.Add(textBoxPhotoshopCode);
            splitContainer5.Panel2.Controls.Add(textBoxHexCode);
            splitContainer5.Panel2.Controls.Add(checkBoxHexCode);
            splitContainer5.Panel2.Controls.Add(LabelColorCode);
            splitContainer5.Panel2.Controls.Add(label2);
            splitContainer5.Panel2.Controls.Add(label1);
            splitContainer5.Panel2.Controls.Add(progressBar2);
            splitContainer5.Panel2.Controls.Add(progressBar1);
            splitContainer5.Panel2.Controls.Add(button6);
            splitContainer5.Panel2.Controls.Add(button5);
            splitContainer5.Panel2.Controls.Add(button4);
            splitContainer5.Panel2.Controls.Add(numericUpDownShortCol);
            splitContainer5.Panel2.Controls.Add(textBoxMeanFrom);
            splitContainer5.Panel2.Controls.Add(textBoxMeanTo);
            splitContainer5.Panel2.Controls.Add(button3);
            splitContainer5.Panel2.Controls.Add(numericUpDownB);
            splitContainer5.Panel2.Controls.Add(numericUpDownG);
            splitContainer5.Panel2.Controls.Add(numericUpDownR);
            splitContainer5.Panel2.Controls.Add(button2);
            splitContainer5.Panel2.Controls.Add(button1);
            splitContainer5.Panel2.Controls.Add(buttonMean);
            splitContainer5.Panel2.Controls.Add(pictureBoxColor);
            splitContainer5.Size = new System.Drawing.Size(744, 388);
            splitContainer5.SplitterDistance = 244;
            splitContainer5.TabIndex = 1;
            // 
            // splitContainer6
            // 
            splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer6.Location = new System.Drawing.Point(0, 0);
            splitContainer6.Margin = new System.Windows.Forms.Padding(4);
            splitContainer6.Name = "splitContainer6";
            splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            splitContainer6.Panel1.Controls.Add(tabControl2);
            // 
            // splitContainer6.Panel2
            // 
            splitContainer6.Panel2.Controls.Add(pictureBoxArt);
            splitContainer6.Size = new System.Drawing.Size(244, 388);
            splitContainer6.SplitterDistance = 229;
            splitContainer6.SplitterWidth = 5;
            splitContainer6.TabIndex = 0;
            // 
            // tabControl2
            // 
            tabControl2.Controls.Add(tabPage3);
            tabControl2.Controls.Add(tabPage4);
            tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl2.Location = new System.Drawing.Point(0, 0);
            tabControl2.Margin = new System.Windows.Forms.Padding(4);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new System.Drawing.Size(244, 229);
            tabControl2.TabIndex = 0;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(treeViewItem);
            tabPage3.Location = new System.Drawing.Point(4, 24);
            tabPage3.Margin = new System.Windows.Forms.Padding(4);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new System.Windows.Forms.Padding(4);
            tabPage3.Size = new System.Drawing.Size(236, 201);
            tabPage3.TabIndex = 0;
            tabPage3.Text = "Items";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(treeViewLand);
            tabPage4.Location = new System.Drawing.Point(4, 24);
            tabPage4.Margin = new System.Windows.Forms.Padding(4);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new System.Windows.Forms.Padding(4);
            tabPage4.Size = new System.Drawing.Size(236, 201);
            tabPage4.TabIndex = 1;
            tabPage4.Text = "Land Tiles";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // btupdateTreeView
            // 
            btupdateTreeView.Location = new System.Drawing.Point(97, 128);
            btupdateTreeView.Name = "btupdateTreeView";
            btupdateTreeView.Size = new System.Drawing.Size(108, 27);
            btupdateTreeView.TabIndex = 31;
            btupdateTreeView.Text = "Update TreeView";
            btupdateTreeView.UseVisualStyleBackColor = true;
            btupdateTreeView.Click += btupdateTreeView_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(4, 270);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(79, 15);
            label6.TabIndex = 30;
            label6.Text = "Items / Land :";
            // 
            // LabelTildataNameItemsLand
            // 
            LabelTildataNameItemsLand.AutoSize = true;
            LabelTildataNameItemsLand.Location = new System.Drawing.Point(89, 270);
            LabelTildataNameItemsLand.Name = "LabelTildataNameItemsLand";
            LabelTildataNameItemsLand.Size = new System.Drawing.Size(39, 15);
            LabelTildataNameItemsLand.TabIndex = 29;
            LabelTildataNameItemsLand.Text = "Name";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(204, 7);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(81, 15);
            label5.TabIndex = 28;
            label5.Text = "Color Palette :";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(360, 7);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(71, 15);
            label4.TabIndex = 27;
            label4.Text = "Photoshop :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(291, 7);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(65, 15);
            label3.TabIndex = 26;
            label3.Text = "Hex Code :";
            // 
            // textBoxPhotoshopCode
            // 
            textBoxPhotoshopCode.Location = new System.Drawing.Point(364, 25);
            textBoxPhotoshopCode.Name = "textBoxPhotoshopCode";
            textBoxPhotoshopCode.ReadOnly = true;
            textBoxPhotoshopCode.Size = new System.Drawing.Size(62, 23);
            textBoxPhotoshopCode.TabIndex = 25;
            textBoxPhotoshopCode.Click += textBoxPhotoshopCode_Click;
            // 
            // textBoxHexCode
            // 
            textBoxHexCode.Location = new System.Drawing.Point(295, 25);
            textBoxHexCode.Name = "textBoxHexCode";
            textBoxHexCode.Size = new System.Drawing.Size(61, 23);
            textBoxHexCode.TabIndex = 24;
            textBoxHexCode.TextChanged += textBoxHexCode_TextChanged;
            textBoxHexCode.KeyPress += textBoxHexCode_KeyPress;
            // 
            // checkBoxHexCode
            // 
            checkBoxHexCode.AutoSize = true;
            checkBoxHexCode.Location = new System.Drawing.Point(201, 85);
            checkBoxHexCode.Name = "checkBoxHexCode";
            checkBoxHexCode.Size = new System.Drawing.Size(180, 19);
            checkBoxHexCode.TabIndex = 23;
            checkBoxHexCode.Text = "Copy color code to clipboard";
            checkBoxHexCode.UseVisualStyleBackColor = true;
            // 
            // LabelColorCode
            // 
            LabelColorCode.AutoSize = true;
            LabelColorCode.Location = new System.Drawing.Point(4, 174);
            LabelColorCode.Name = "LabelColorCode";
            LabelColorCode.Size = new System.Drawing.Size(92, 15);
            LabelColorCode.TabIndex = 22;
            LabelColorCode.Text = "LabelColorCode";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(226, 297);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(62, 15);
            label2.TabIndex = 21;
            label2.Text = "Land Tiles:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(255, 270);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(39, 15);
            label1.TabIndex = 20;
            label1.Text = "Items:";
            // 
            // progressBar2
            // 
            progressBar2.Location = new System.Drawing.Point(309, 292);
            progressBar2.Name = "progressBar2";
            progressBar2.Size = new System.Drawing.Size(117, 22);
            progressBar2.TabIndex = 19;
            // 
            // progressBar1
            // 
            progressBar1.Location = new System.Drawing.Point(309, 264);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(117, 22);
            progressBar1.TabIndex = 18;
            // 
            // button6
            // 
            button6.AutoSize = true;
            button6.Location = new System.Drawing.Point(220, 227);
            button6.Margin = new System.Windows.Forms.Padding(4);
            button6.Name = "button6";
            button6.Size = new System.Drawing.Size(206, 31);
            button6.TabIndex = 17;
            button6.Text = "Average All (Items and Land Tiles)";
            button6.UseVisualStyleBackColor = true;
            button6.Click += OnClickMeanColorAll;
            // 
            // button5
            // 
            button5.Location = new System.Drawing.Point(97, 232);
            button5.Margin = new System.Windows.Forms.Padding(4);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(88, 26);
            button5.TabIndex = 16;
            button5.Text = "Import..";
            button5.UseVisualStyleBackColor = true;
            button5.Click += OnClickImport;
            // 
            // button4
            // 
            button4.Location = new System.Drawing.Point(3, 232);
            button4.Margin = new System.Windows.Forms.Padding(4);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(88, 26);
            button4.TabIndex = 15;
            button4.Text = "Export..";
            button4.UseVisualStyleBackColor = true;
            button4.Click += OnClickExport;
            // 
            // numericUpDownShortCol
            // 
            numericUpDownShortCol.Location = new System.Drawing.Point(201, 25);
            numericUpDownShortCol.Margin = new System.Windows.Forms.Padding(4);
            numericUpDownShortCol.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            numericUpDownShortCol.Name = "numericUpDownShortCol";
            numericUpDownShortCol.Size = new System.Drawing.Size(87, 23);
            numericUpDownShortCol.TabIndex = 14;
            numericUpDownShortCol.ValueChanged += OnNumericShortColChanged;
            // 
            // textBoxMeanFrom
            // 
            textBoxMeanFrom.Location = new System.Drawing.Point(220, 131);
            textBoxMeanFrom.Margin = new System.Windows.Forms.Padding(4);
            textBoxMeanFrom.Name = "textBoxMeanFrom";
            textBoxMeanFrom.Size = new System.Drawing.Size(60, 23);
            textBoxMeanFrom.TabIndex = 13;
            // 
            // textBoxMeanTo
            // 
            textBoxMeanTo.Location = new System.Drawing.Point(298, 131);
            textBoxMeanTo.Margin = new System.Windows.Forms.Padding(4);
            textBoxMeanTo.Name = "textBoxMeanTo";
            textBoxMeanTo.Size = new System.Drawing.Size(60, 23);
            textBoxMeanTo.TabIndex = 12;
            // 
            // button3
            // 
            button3.AutoSize = true;
            button3.Location = new System.Drawing.Point(220, 158);
            button3.Margin = new System.Windows.Forms.Padding(4);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(183, 31);
            button3.TabIndex = 11;
            button3.Text = "Average Color from-to";
            button3.UseVisualStyleBackColor = true;
            button3.Click += OnClickMeanColorFromTo;
            // 
            // numericUpDownB
            // 
            numericUpDownB.Location = new System.Drawing.Point(325, 55);
            numericUpDownB.Margin = new System.Windows.Forms.Padding(4);
            numericUpDownB.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDownB.Name = "numericUpDownB";
            numericUpDownB.Size = new System.Drawing.Size(55, 23);
            numericUpDownB.TabIndex = 9;
            numericUpDownB.ValueChanged += OnChangeB;
            // 
            // numericUpDownG
            // 
            numericUpDownG.Location = new System.Drawing.Point(264, 55);
            numericUpDownG.Margin = new System.Windows.Forms.Padding(4);
            numericUpDownG.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDownG.Name = "numericUpDownG";
            numericUpDownG.Size = new System.Drawing.Size(55, 23);
            numericUpDownG.TabIndex = 8;
            numericUpDownG.ValueChanged += OnChangeG;
            // 
            // numericUpDownR
            // 
            numericUpDownR.Location = new System.Drawing.Point(201, 55);
            numericUpDownR.Margin = new System.Windows.Forms.Padding(4);
            numericUpDownR.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDownR.Name = "numericUpDownR";
            numericUpDownR.Size = new System.Drawing.Size(55, 23);
            numericUpDownR.TabIndex = 7;
            numericUpDownR.ValueChanged += OnChangeR;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(97, 197);
            button2.Margin = new System.Windows.Forms.Padding(4);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(88, 26);
            button2.TabIndex = 5;
            button2.Text = "Save File";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnClickSaveFile;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(3, 197);
            button1.Margin = new System.Windows.Forms.Padding(4);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(88, 26);
            button1.TabIndex = 4;
            button1.Text = "Save Color";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnClickSaveColor;
            // 
            // buttonMean
            // 
            buttonMean.Location = new System.Drawing.Point(3, 129);
            buttonMean.Margin = new System.Windows.Forms.Padding(4);
            buttonMean.Name = "buttonMean";
            buttonMean.Size = new System.Drawing.Size(88, 26);
            buttonMean.TabIndex = 1;
            buttonMean.Text = "Average Color";
            buttonMean.UseVisualStyleBackColor = true;
            buttonMean.Click += OnClickMeanColor;
            // 
            // RadarColorControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer5);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "RadarColorControl";
            Size = new System.Drawing.Size(744, 388);
            Load += OnLoad;
            contextMenuStrip1.ResumeLayout(false);
            contextMenuStrip2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxArt).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxColor).EndInit();
            splitContainer5.Panel1.ResumeLayout(false);
            splitContainer5.Panel2.ResumeLayout(false);
            splitContainer5.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer5).EndInit();
            splitContainer5.ResumeLayout(false);
            splitContainer6.Panel1.ResumeLayout(false);
            splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer6).EndInit();
            splitContainer6.ResumeLayout(false);
            tabControl2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numericUpDownShortCol).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownB).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownG).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownR).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button buttonMean;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.NumericUpDown numericUpDownB;
        private System.Windows.Forms.NumericUpDown numericUpDownG;
        private System.Windows.Forms.NumericUpDown numericUpDownR;
        private System.Windows.Forms.NumericUpDown numericUpDownShortCol;
        private System.Windows.Forms.PictureBox pictureBoxArt;
        private System.Windows.Forms.PictureBox pictureBoxColor;
        private System.Windows.Forms.ToolStripMenuItem selectInItemsTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInTiledataTabToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox textBoxMeanFrom;
        private System.Windows.Forms.TextBox textBoxMeanTo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.TreeView treeViewLand;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label LabelColorCode;
        private ColorTreeView treeViewItem;
        private System.Windows.Forms.CheckBox checkBoxHexCode;
        private System.Windows.Forms.TextBox textBoxHexCode;
        private System.Windows.Forms.TextBox textBoxPhotoshopCode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label LabelTildataNameItemsLand;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btupdateTreeView;
    }
}
