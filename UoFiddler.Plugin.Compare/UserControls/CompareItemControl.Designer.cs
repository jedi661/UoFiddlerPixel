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
    partial class CompareItemControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompareItemControl));
            listBoxOrg = new System.Windows.Forms.ListBox();
            contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(components);
            austausschenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            okayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            listBoxSec = new System.Windows.Forms.ListBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            extractAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportImageVonBisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyItem2To1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pictureBoxOrg = new System.Windows.Forms.PictureBox();
            pictureBoxSec = new System.Windows.Forms.PictureBox();
            textBoxSecondDir = new System.Windows.Forms.TextBox();
            button1 = new System.Windows.Forms.Button();
            checkBox1 = new System.Windows.Forms.CheckBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            BtDataDirectoryisSettings = new System.Windows.Forms.Button();
            btremoveitemfromindex = new System.Windows.Forms.Button();
            btLeftMoveItemMore = new System.Windows.Forms.Button();
            btLeftMoveItem = new System.Windows.Forms.Button();
            comboBoxSaveDir = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            OnClickSearch = new System.Windows.Forms.Button();
            searchTextBox = new System.Windows.Forms.TextBox();
            button2 = new System.Windows.Forms.Button();
            contextMenuStrip2.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrg).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSec).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // listBoxOrg
            // 
            listBoxOrg.ContextMenuStrip = contextMenuStrip2;
            listBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxOrg.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listBoxOrg.FormattingEnabled = true;
            listBoxOrg.IntegralHeight = false;
            listBoxOrg.Location = new System.Drawing.Point(4, 3);
            listBoxOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBoxOrg.Name = "listBoxOrg";
            listBoxOrg.Size = new System.Drawing.Size(220, 255);
            listBoxOrg.TabIndex = 0;
            listBoxOrg.DrawItem += DrawItemOrg;
            listBoxOrg.MeasureItem += MeasureOrg;
            listBoxOrg.SelectedIndexChanged += OnIndexChangedOrg;
            // 
            // contextMenuStrip2
            // 
            contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { austausschenToolStripMenuItem });
            contextMenuStrip2.Name = "contextMenuStrip2";
            contextMenuStrip2.Size = new System.Drawing.Size(159, 26);
            // 
            // austausschenToolStripMenuItem
            // 
            austausschenToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { okayToolStripMenuItem });
            austausschenToolStripMenuItem.Name = "austausschenToolStripMenuItem";
            austausschenToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            austausschenToolStripMenuItem.Text = "Replace graphic";
            austausschenToolStripMenuItem.Click += OnClickShowSelection;
            // 
            // okayToolStripMenuItem
            // 
            okayToolStripMenuItem.Name = "okayToolStripMenuItem";
            okayToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            okayToolStripMenuItem.Text = "Okay";
            okayToolStripMenuItem.Click += OnClickOkButton;
            // 
            // listBoxSec
            // 
            listBoxSec.ContextMenuStrip = contextMenuStrip1;
            listBoxSec.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxSec.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listBoxSec.FormattingEnabled = true;
            listBoxSec.IntegralHeight = false;
            listBoxSec.Location = new System.Drawing.Point(612, 3);
            listBoxSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBoxSec.Name = "listBoxSec";
            listBoxSec.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            listBoxSec.Size = new System.Drawing.Size(221, 255);
            listBoxSec.TabIndex = 1;
            listBoxSec.DrawItem += DrawItemSec;
            listBoxSec.MeasureItem += MeasureSec;
            listBoxSec.SelectedIndexChanged += OnIndexChangedSec;
            listBoxSec.KeyDown += ListBoxSec_KeyDown;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { extractAsToolStripMenuItem, exportImageVonBisToolStripMenuItem, copyItem2To1ToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(188, 70);
            // 
            // extractAsToolStripMenuItem
            // 
            extractAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tiffToolStripMenuItem, bmpToolStripMenuItem });
            extractAsToolStripMenuItem.Name = "extractAsToolStripMenuItem";
            extractAsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            extractAsToolStripMenuItem.Text = "Export Image..";
            // 
            // tiffToolStripMenuItem
            // 
            tiffToolStripMenuItem.Name = "tiffToolStripMenuItem";
            tiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            tiffToolStripMenuItem.Text = "As Bmp";
            tiffToolStripMenuItem.Click += ExportAsBmp;
            // 
            // bmpToolStripMenuItem
            // 
            bmpToolStripMenuItem.Name = "bmpToolStripMenuItem";
            bmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            bmpToolStripMenuItem.Text = "As Tiff";
            bmpToolStripMenuItem.Click += ExportAsTiff;
            // 
            // exportImageVonBisToolStripMenuItem
            // 
            exportImageVonBisToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asToolStripMenuItem, asTiffToolStripMenuItem });
            exportImageVonBisToolStripMenuItem.Name = "exportImageVonBisToolStripMenuItem";
            exportImageVonBisToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            exportImageVonBisToolStripMenuItem.Text = "Export image from to";
            exportImageVonBisToolStripMenuItem.ToolTipText = "Press the Ctrl or Shift key to select what should be saved from start to end.";
            // 
            // asToolStripMenuItem
            // 
            asToolStripMenuItem.Name = "asToolStripMenuItem";
            asToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asToolStripMenuItem.Text = "As Bmp";
            asToolStripMenuItem.Click += ExportAsBmp2;
            // 
            // asTiffToolStripMenuItem
            // 
            asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asTiffToolStripMenuItem.Text = "As Tiff";
            asTiffToolStripMenuItem.Click += ExportAsTiff2;
            // 
            // copyItem2To1ToolStripMenuItem
            // 
            copyItem2To1ToolStripMenuItem.Name = "copyItem2To1ToolStripMenuItem";
            copyItem2To1ToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
            copyItem2To1ToolStripMenuItem.Text = "Copy Item 2 to 1";
            copyItem2To1ToolStripMenuItem.Click += OnClickCopy;
            // 
            // pictureBoxOrg
            // 
            pictureBoxOrg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxOrg.Location = new System.Drawing.Point(5, 4);
            pictureBoxOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxOrg.Name = "pictureBoxOrg";
            pictureBoxOrg.Size = new System.Drawing.Size(362, 120);
            pictureBoxOrg.TabIndex = 2;
            pictureBoxOrg.TabStop = false;
            // 
            // pictureBoxSec
            // 
            pictureBoxSec.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxSec.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxSec.Location = new System.Drawing.Point(5, 131);
            pictureBoxSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxSec.Name = "pictureBoxSec";
            pictureBoxSec.Size = new System.Drawing.Size(362, 120);
            pictureBoxSec.TabIndex = 3;
            pictureBoxSec.TabStop = false;
            // 
            // textBoxSecondDir
            // 
            textBoxSecondDir.Location = new System.Drawing.Point(14, 9);
            textBoxSecondDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxSecondDir.Name = "textBoxSecondDir";
            textBoxSecondDir.Size = new System.Drawing.Size(293, 23);
            textBoxSecondDir.TabIndex = 4;
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.Location = new System.Drawing.Point(349, 6);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(99, 29);
            button1.TabIndex = 5;
            button1.Text = "Load Second";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnClickLoadSecond;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(456, 11);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(143, 19);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "Show only Differences";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.Click += OnChangeShowDiff;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pictureBoxSec, 0, 1);
            tableLayoutPanel1.Controls.Add(pictureBoxOrg, 0, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(232, 3);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new System.Drawing.Size(372, 255);
            tableLayoutPanel1.TabIndex = 7;
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
            tableLayoutPanel2.Size = new System.Drawing.Size(837, 261);
            tableLayoutPanel2.TabIndex = 8;
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
            splitContainer1.Panel2.Controls.Add(BtDataDirectoryisSettings);
            splitContainer1.Panel2.Controls.Add(btremoveitemfromindex);
            splitContainer1.Panel2.Controls.Add(btLeftMoveItemMore);
            splitContainer1.Panel2.Controls.Add(btLeftMoveItem);
            splitContainer1.Panel2.Controls.Add(comboBoxSaveDir);
            splitContainer1.Panel2.Controls.Add(label2);
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Panel2.Controls.Add(OnClickSearch);
            splitContainer1.Panel2.Controls.Add(searchTextBox);
            splitContainer1.Panel2.Controls.Add(button2);
            splitContainer1.Panel2.Controls.Add(textBoxSecondDir);
            splitContainer1.Panel2.Controls.Add(checkBox1);
            splitContainer1.Panel2.Controls.Add(button1);
            splitContainer1.Size = new System.Drawing.Size(837, 432);
            splitContainer1.SplitterDistance = 261;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 9;
            // 
            // BtDataDirectoryisSettings
            // 
            BtDataDirectoryisSettings.Location = new System.Drawing.Point(573, 42);
            BtDataDirectoryisSettings.Name = "BtDataDirectoryisSettings";
            BtDataDirectoryisSettings.Size = new System.Drawing.Size(36, 23);
            BtDataDirectoryisSettings.TabIndex = 16;
            BtDataDirectoryisSettings.Text = "Dir";
            BtDataDirectoryisSettings.UseVisualStyleBackColor = true;
            BtDataDirectoryisSettings.Click += BtDataDirectoryisSettings_Click;
            // 
            // btremoveitemfromindex
            // 
            btremoveitemfromindex.Image = Properties.Resources.right;
            btremoveitemfromindex.Location = new System.Drawing.Point(727, 3);
            btremoveitemfromindex.Name = "btremoveitemfromindex";
            btremoveitemfromindex.Size = new System.Drawing.Size(52, 53);
            btremoveitemfromindex.TabIndex = 15;
            btremoveitemfromindex.UseVisualStyleBackColor = true;
            btremoveitemfromindex.Click += btremoveitemfromindex_Click;
            // 
            // btLeftMoveItemMore
            // 
            btLeftMoveItemMore.Image = Properties.Resources.left2;
            btLeftMoveItemMore.Location = new System.Drawing.Point(671, 3);
            btLeftMoveItemMore.Name = "btLeftMoveItemMore";
            btLeftMoveItemMore.Size = new System.Drawing.Size(52, 53);
            btLeftMoveItemMore.TabIndex = 14;
            btLeftMoveItemMore.UseVisualStyleBackColor = true;
            btLeftMoveItemMore.Click += btLeftMoveItemMore_Click;
            // 
            // btLeftMoveItem
            // 
            btLeftMoveItem.Image = (System.Drawing.Image)resources.GetObject("btLeftMoveItem.Image");
            btLeftMoveItem.Location = new System.Drawing.Point(612, 3);
            btLeftMoveItem.Name = "btLeftMoveItem";
            btLeftMoveItem.Size = new System.Drawing.Size(53, 53);
            btLeftMoveItem.TabIndex = 13;
            btLeftMoveItem.UseVisualStyleBackColor = true;
            btLeftMoveItem.Click += btLeftMoveItem_Click;
            // 
            // comboBoxSaveDir
            // 
            comboBoxSaveDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxSaveDir.FormattingEnabled = true;
            comboBoxSaveDir.Location = new System.Drawing.Point(76, 45);
            comboBoxSaveDir.Name = "comboBoxSaveDir";
            comboBoxSaveDir.Size = new System.Drawing.Size(278, 23);
            comboBoxSaveDir.TabIndex = 12;
            comboBoxSaveDir.SelectedIndexChanged += comboBoxSaveDir_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(360, 50);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(88, 15);
            label2.TabIndex = 11;
            label2.Text = "Hex addresses :";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(11, 50);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(63, 15);
            label1.TabIndex = 10;
            label1.Text = "Load files :";
            // 
            // OnClickSearch
            // 
            OnClickSearch.Location = new System.Drawing.Point(515, 42);
            OnClickSearch.Name = "OnClickSearch";
            OnClickSearch.Size = new System.Drawing.Size(57, 23);
            OnClickSearch.TabIndex = 9;
            OnClickSearch.Text = "Search";
            OnClickSearch.UseVisualStyleBackColor = true;
            OnClickSearch.Click += OnClickSearch_Click;
            // 
            // searchTextBox
            // 
            searchTextBox.Location = new System.Drawing.Point(456, 42);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.Size = new System.Drawing.Size(57, 23);
            searchTextBox.TabIndex = 8;
            // 
            // button2
            // 
            button2.AutoSize = true;
            button2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            button2.Location = new System.Drawing.Point(315, 8);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(26, 25);
            button2.TabIndex = 7;
            button2.Text = "...";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnClickBrowse;
            // 
            // CompareItemControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CompareItemControl";
            Size = new System.Drawing.Size(837, 432);
            Load += OnLoad;
            contextMenuStrip2.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrg).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSec).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem bmpToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyItem2To1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAsToolStripMenuItem;
        private System.Windows.Forms.ListBox listBoxOrg;
        private System.Windows.Forms.ListBox listBoxSec;
        private System.Windows.Forms.PictureBox pictureBoxOrg;
        private System.Windows.Forms.PictureBox pictureBoxSec;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox textBoxSecondDir;
        private System.Windows.Forms.ToolStripMenuItem tiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportImageVonBisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem austausschenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem okayToolStripMenuItem;
        private System.Windows.Forms.Button OnClickSearch;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxSaveDir;
        private System.Windows.Forms.Button btLeftMoveItem;
        private System.Windows.Forms.Button btLeftMoveItemMore;
        private System.Windows.Forms.Button btremoveitemfromindex;
        private System.Windows.Forms.Button BtDataDirectoryisSettings;
    }
}
