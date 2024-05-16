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
    partial class FontsControl
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
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            writeTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            setOffsetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            extractCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            importClipbordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyClipbordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            treeView = new System.Windows.Forms.TreeView();
            LoadUnicodeFontsCheckBox = new System.Windows.Forms.CheckBox();
            FontsTileView = new TileView.TileViewControl();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            tTFunicodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSplitSound = new System.Windows.Forms.ToolStripSplitButton();
            toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { writeTextToolStripMenuItem, toolStripSeparator1, setOffsetsToolStripMenuItem, extractCharacterToolStripMenuItem, importCharacterToolStripMenuItem, toolStripSeparator2, importClipbordToolStripMenuItem, copyClipbordToolStripMenuItem, toolStripSeparator3, saveToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(165, 176);
            // 
            // writeTextToolStripMenuItem
            // 
            writeTextToolStripMenuItem.Image = Properties.Resources.Edit;
            writeTextToolStripMenuItem.Name = "writeTextToolStripMenuItem";
            writeTextToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            writeTextToolStripMenuItem.Text = "Write Text";
            writeTextToolStripMenuItem.ToolTipText = "Wrote the text so that it's visible.";
            writeTextToolStripMenuItem.Click += OnClickWriteText;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(161, 6);
            // 
            // setOffsetsToolStripMenuItem
            // 
            setOffsetsToolStripMenuItem.Name = "setOffsetsToolStripMenuItem";
            setOffsetsToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            setOffsetsToolStripMenuItem.Text = "Set Offsets";
            setOffsetsToolStripMenuItem.Click += OnClickSetOffsets;
            // 
            // extractCharacterToolStripMenuItem
            // 
            extractCharacterToolStripMenuItem.Image = Properties.Resources.Copy;
            extractCharacterToolStripMenuItem.Name = "extractCharacterToolStripMenuItem";
            extractCharacterToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            extractCharacterToolStripMenuItem.Text = "Extract Character";
            extractCharacterToolStripMenuItem.ToolTipText = "Exported graphics to a file.";
            extractCharacterToolStripMenuItem.Click += OnClickExport;
            // 
            // importCharacterToolStripMenuItem
            // 
            importCharacterToolStripMenuItem.Image = Properties.Resources.import;
            importCharacterToolStripMenuItem.Name = "importCharacterToolStripMenuItem";
            importCharacterToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            importCharacterToolStripMenuItem.Text = "Import Character";
            importCharacterToolStripMenuItem.ToolTipText = "Imported graphics from a file.";
            importCharacterToolStripMenuItem.Click += OnClickImport;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(161, 6);
            // 
            // importClipbordToolStripMenuItem
            // 
            importClipbordToolStripMenuItem.Image = Properties.Resources.import;
            importClipbordToolStripMenuItem.Name = "importClipbordToolStripMenuItem";
            importClipbordToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            importClipbordToolStripMenuItem.Text = "Import Clipbord";
            importClipbordToolStripMenuItem.ToolTipText = "Imported from clipboard.";
            importClipbordToolStripMenuItem.Click += importClipbordToolStripMenuItem_Click;
            // 
            // copyClipbordToolStripMenuItem
            // 
            copyClipbordToolStripMenuItem.Image = Properties.Resources.Clipbord;
            copyClipbordToolStripMenuItem.Name = "copyClipbordToolStripMenuItem";
            copyClipbordToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            copyClipbordToolStripMenuItem.Text = "Copy Clipbord";
            copyClipbordToolStripMenuItem.ToolTipText = "Copied to the clipboard.";
            copyClipbordToolStripMenuItem.Click += copyClipbordToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(161, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Image = Properties.Resources.Save2;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.ToolTipText = "Saved to the .mul file.";
            saveToolStripMenuItem.Click += OnClickSave;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(FontsTileView);
            splitContainer1.Panel2.Controls.Add(statusStrip1);
            splitContainer1.Size = new System.Drawing.Size(732, 382);
            splitContainer1.SplitterDistance = 150;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(treeView);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(LoadUnicodeFontsCheckBox);
            splitContainer2.Size = new System.Drawing.Size(150, 382);
            splitContainer2.SplitterDistance = 325;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            // 
            // treeView
            // 
            treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            treeView.HideSelection = false;
            treeView.Location = new System.Drawing.Point(0, 0);
            treeView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            treeView.Name = "treeView";
            treeView.Size = new System.Drawing.Size(150, 325);
            treeView.TabIndex = 2;
            treeView.AfterSelect += OnSelect;
            // 
            // LoadUnicodeFontsCheckBox
            // 
            LoadUnicodeFontsCheckBox.AutoSize = true;
            LoadUnicodeFontsCheckBox.Location = new System.Drawing.Point(18, 16);
            LoadUnicodeFontsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LoadUnicodeFontsCheckBox.Name = "LoadUnicodeFontsCheckBox";
            LoadUnicodeFontsCheckBox.Size = new System.Drawing.Size(131, 19);
            LoadUnicodeFontsCheckBox.TabIndex = 0;
            LoadUnicodeFontsCheckBox.Text = "Load Unicode Fonts";
            LoadUnicodeFontsCheckBox.UseVisualStyleBackColor = true;
            LoadUnicodeFontsCheckBox.CheckedChanged += LoadUnicodeFontsCheckBox_CheckedChanged;
            // 
            // FontsTileView
            // 
            FontsTileView.AutoScroll = true;
            FontsTileView.AutoScrollMinSize = new System.Drawing.Size(0, 34);
            FontsTileView.BackColor = System.Drawing.Color.White;
            FontsTileView.ContextMenuStrip = contextMenuStrip1;
            FontsTileView.Dock = System.Windows.Forms.DockStyle.Fill;
            FontsTileView.FocusIndex = -1;
            FontsTileView.Location = new System.Drawing.Point(0, 0);
            FontsTileView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            FontsTileView.MultiSelect = false;
            FontsTileView.Name = "FontsTileView";
            FontsTileView.Size = new System.Drawing.Size(577, 360);
            FontsTileView.TabIndex = 8;
            FontsTileView.TileBackgroundColor = System.Drawing.SystemColors.Window;
            FontsTileView.TileBorderColor = System.Drawing.Color.Gray;
            FontsTileView.TileBorderWidth = 1F;
            FontsTileView.TileFocusColor = System.Drawing.Color.DarkRed;
            FontsTileView.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            FontsTileView.TileMargin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            FontsTileView.TilePadding = new System.Windows.Forms.Padding(0);
            FontsTileView.TileSize = new System.Drawing.Size(30, 30);
            FontsTileView.VirtualListSize = 1;
            FontsTileView.ItemSelectionChanged += FontsTileView_ItemSelectionChanged;
            FontsTileView.DrawItem += FontsTileView_DrawItem;
            FontsTileView.KeyDown += Form_KeyDown;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripDropDownButton1, toolStripSplitSound, toolStripStatusLabel1 });
            statusStrip1.Location = new System.Drawing.Point(0, 360);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            statusStrip1.Size = new System.Drawing.Size(577, 22);
            statusStrip1.TabIndex = 7;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tTFunicodeToolStripMenuItem });
            toolStripDropDownButton1.Image = Properties.Resources.Edit;
            toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new System.Drawing.Size(29, 20);
            toolStripDropDownButton1.Text = "toolStripDropDownButton1";
            // 
            // tTFunicodeToolStripMenuItem
            // 
            tTFunicodeToolStripMenuItem.Image = Properties.Resources.preview;
            tTFunicodeToolStripMenuItem.Name = "tTFunicodeToolStripMenuItem";
            tTFunicodeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            tTFunicodeToolStripMenuItem.Text = "TTFunicode";
            tTFunicodeToolStripMenuItem.Click += tTFunicodeToolStripMenuItem_Click;
            // 
            // toolStripSplitSound
            // 
            toolStripSplitSound.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripSplitSound.Image = Properties.Resources.volume;
            toolStripSplitSound.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripSplitSound.Name = "toolStripSplitSound";
            toolStripSplitSound.Size = new System.Drawing.Size(32, 20);
            toolStripSplitSound.Text = "toolStripSplitButton1";
            toolStripSplitSound.ToolTipText = "Activated the sound.";
            toolStripSplitSound.ButtonClick += toolStripSplitSound_ButtonClick;
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new System.Drawing.Size(87, 17);
            toolStripStatusLabel1.Text = "<no selection>";
            // 
            // FontsControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "FontsControl";
            Size = new System.Drawing.Size(732, 382);
            Load += OnLoad;
            Resize += FontsControl_Resize;
            contextMenuStrip1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem extractCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setOffsetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem writeTextToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private TileView.TileViewControl FontsTileView;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.CheckBox LoadUnicodeFontsCheckBox;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripMenuItem importClipbordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyClipbordToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitSound;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem tTFunicodeToolStripMenuItem;
    }
}
