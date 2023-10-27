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

namespace UoFiddler.Plugin.Compare.UserControls
{
    partial class CompareLandControl
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
            listBoxOrg = new System.Windows.Forms.ListBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            pictureBoxSec = new System.Windows.Forms.PictureBox();
            pictureBoxOrg = new System.Windows.Forms.PictureBox();
            textBoxSecondDir = new System.Windows.Forms.TextBox();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            listBoxSec = new System.Windows.Forms.ListBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyLandTile2To1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkBox1 = new System.Windows.Forms.CheckBox();
            button1 = new System.Windows.Forms.Button();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            btmultipleImageID = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            tbSearchHex = new System.Windows.Forms.TextBox();
            btRemoveImageId = new System.Windows.Forms.Button();
            btmoveItemtoId = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSec).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrg).BeginInit();
            tableLayoutPanel2.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // listBoxOrg
            // 
            listBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxOrg.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listBoxOrg.FormattingEnabled = true;
            listBoxOrg.IntegralHeight = false;
            listBoxOrg.Location = new System.Drawing.Point(4, 3);
            listBoxOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBoxOrg.Name = "listBoxOrg";
            listBoxOrg.Size = new System.Drawing.Size(211, 363);
            listBoxOrg.TabIndex = 0;
            listBoxOrg.DrawItem += DrawitemOrg;
            listBoxOrg.MeasureItem += MeasureOrg;
            listBoxOrg.SelectedIndexChanged += OnIndexChangedOrg;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pictureBoxSec, 0, 1);
            tableLayoutPanel1.Controls.Add(pictureBoxOrg, 0, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(223, 3);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new System.Drawing.Size(357, 363);
            tableLayoutPanel1.TabIndex = 7;
            // 
            // pictureBoxSec
            // 
            pictureBoxSec.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxSec.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxSec.Location = new System.Drawing.Point(5, 185);
            pictureBoxSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxSec.Name = "pictureBoxSec";
            pictureBoxSec.Size = new System.Drawing.Size(347, 174);
            pictureBoxSec.TabIndex = 3;
            pictureBoxSec.TabStop = false;
            // 
            // pictureBoxOrg
            // 
            pictureBoxOrg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxOrg.Location = new System.Drawing.Point(5, 4);
            pictureBoxOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxOrg.Name = "pictureBoxOrg";
            pictureBoxOrg.Size = new System.Drawing.Size(347, 174);
            pictureBoxOrg.TabIndex = 2;
            pictureBoxOrg.TabStop = false;
            // 
            // textBoxSecondDir
            // 
            textBoxSecondDir.Location = new System.Drawing.Point(9, 16);
            textBoxSecondDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxSecondDir.Name = "textBoxSecondDir";
            textBoxSecondDir.Size = new System.Drawing.Size(168, 23);
            textBoxSecondDir.TabIndex = 4;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27273F));
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.45454F));
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27273F));
            tableLayoutPanel2.Controls.Add(listBoxOrg, 0, 0);
            tableLayoutPanel2.Controls.Add(listBoxSec, 2, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel1, 1, 0);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new System.Drawing.Size(804, 369);
            tableLayoutPanel2.TabIndex = 8;
            // 
            // listBoxSec
            // 
            listBoxSec.ContextMenuStrip = contextMenuStrip1;
            listBoxSec.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxSec.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listBoxSec.FormattingEnabled = true;
            listBoxSec.IntegralHeight = false;
            listBoxSec.Location = new System.Drawing.Point(588, 3);
            listBoxSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBoxSec.Name = "listBoxSec";
            listBoxSec.Size = new System.Drawing.Size(212, 363);
            listBoxSec.TabIndex = 1;
            listBoxSec.DrawItem += DrawItemSec;
            listBoxSec.MeasureItem += MeasureSec;
            listBoxSec.SelectedIndexChanged += OnIndexChangedSec;
            listBoxSec.KeyDown += ListBoxSec_KeyDown;
            listBoxSec.MouseDown += listBoxSec_MouseDown;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { exportImageToolStripMenuItem, copyLandTile2To1ToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(182, 48);
            // 
            // exportImageToolStripMenuItem
            // 
            exportImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asBmpToolStripMenuItem, asTiffToolStripMenuItem });
            exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            exportImageToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            exportImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asBmpToolStripMenuItem.Text = "As Bmp";
            asBmpToolStripMenuItem.Click += ExportAsBmp;
            // 
            // asTiffToolStripMenuItem
            // 
            asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asTiffToolStripMenuItem.Text = "As Tiff";
            asTiffToolStripMenuItem.Click += ExportAsTiff;
            // 
            // copyLandTile2To1ToolStripMenuItem
            // 
            copyLandTile2To1ToolStripMenuItem.Name = "copyLandTile2To1ToolStripMenuItem";
            copyLandTile2To1ToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            copyLandTile2To1ToolStripMenuItem.Text = "Copy LandTile 2 to 1";
            copyLandTile2To1ToolStripMenuItem.Click += OnClickCopy;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(326, 18);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(143, 19);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "Show only Differences";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.Click += OnChangeShowDiff;
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.Location = new System.Drawing.Point(219, 14);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(99, 29);
            button1.TabIndex = 5;
            button1.Text = "Load Second";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnClickLoadSecond;
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(btmultipleImageID);
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Panel2.Controls.Add(tbSearchHex);
            splitContainer1.Panel2.Controls.Add(btRemoveImageId);
            splitContainer1.Panel2.Controls.Add(btmoveItemtoId);
            splitContainer1.Panel2.Controls.Add(button2);
            splitContainer1.Panel2.Controls.Add(textBoxSecondDir);
            splitContainer1.Panel2.Controls.Add(checkBox1);
            splitContainer1.Panel2.Controls.Add(button1);
            splitContainer1.Size = new System.Drawing.Size(804, 443);
            splitContainer1.SplitterDistance = 369;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 10;
            // 
            // btmultipleImageID
            // 
            btmultipleImageID.Image = Properties.Resources.left2;
            btmultipleImageID.Location = new System.Drawing.Point(527, 4);
            btmultipleImageID.Name = "btmultipleImageID";
            btmultipleImageID.Size = new System.Drawing.Size(53, 50);
            btmultipleImageID.TabIndex = 12;
            btmultipleImageID.UseVisualStyleBackColor = true;
            btmultipleImageID.Click += btmultipleImageID_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(639, 10);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(69, 15);
            label1.TabIndex = 11;
            label1.Text = "Hex Adress:";
            // 
            // tbSearchHex
            // 
            tbSearchHex.Location = new System.Drawing.Point(639, 28);
            tbSearchHex.Name = "tbSearchHex";
            tbSearchHex.Size = new System.Drawing.Size(100, 23);
            tbSearchHex.TabIndex = 10;
            tbSearchHex.TextChanged += tbSearchHex_TextChanged;
            // 
            // btRemoveImageId
            // 
            btRemoveImageId.Image = Properties.Resources.right;
            btRemoveImageId.Location = new System.Drawing.Point(578, 4);
            btRemoveImageId.Name = "btRemoveImageId";
            btRemoveImageId.Size = new System.Drawing.Size(55, 50);
            btRemoveImageId.TabIndex = 9;
            btRemoveImageId.UseVisualStyleBackColor = true;
            btRemoveImageId.Click += btRemoveImageId_Click;
            // 
            // btmoveItemtoId
            // 
            btmoveItemtoId.Image = Properties.Resources.left;
            btmoveItemtoId.Location = new System.Drawing.Point(476, 4);
            btmoveItemtoId.Name = "btmoveItemtoId";
            btmoveItemtoId.Size = new System.Drawing.Size(54, 50);
            btmoveItemtoId.TabIndex = 8;
            btmoveItemtoId.UseVisualStyleBackColor = true;
            btmoveItemtoId.Click += btmoveItemtoId_Click;
            // 
            // button2
            // 
            button2.AutoSize = true;
            button2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            button2.Location = new System.Drawing.Point(185, 16);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(26, 25);
            button2.TabIndex = 7;
            button2.Text = "...";
            button2.UseVisualStyleBackColor = true;
            button2.Click += BrowseOnClick;
            // 
            // CompareLandControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CompareLandControl";
            Size = new System.Drawing.Size(804, 443);
            Load += OnLoad;
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxSec).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrg).EndInit();
            tableLayoutPanel2.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyLandTile2To1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ListBox listBoxOrg;
        private System.Windows.Forms.ListBox listBoxSec;
        private System.Windows.Forms.PictureBox pictureBoxOrg;
        private System.Windows.Forms.PictureBox pictureBoxSec;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox textBoxSecondDir;
        private System.Windows.Forms.Button btmoveItemtoId;
        private System.Windows.Forms.Button btRemoveImageId;
        private System.Windows.Forms.TextBox tbSearchHex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btmultipleImageID;
    }
}
