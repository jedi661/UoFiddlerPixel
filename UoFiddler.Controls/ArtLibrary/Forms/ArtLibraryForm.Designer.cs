// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Controls.ArtLibrary.Forms
{
    partial class ArtLibraryForm
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
            toolStripMain = new System.Windows.Forms.ToolStrip();
            toolStripLabelLibrary = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxLibrary = new System.Windows.Forms.ToolStripComboBox();
            toolStripButtonNew = new System.Windows.Forms.ToolStripButton();
            toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            toolStripButtonReload = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelId = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxId = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonGoTo = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonOnlyOccupied = new System.Windows.Forms.ToolStripButton();
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            LibraryTileView = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            contextMenuStripTileView = new System.Windows.Forms.ContextMenuStrip(components);
            findNextFreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            copyIdHexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyIdDecToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            backgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitContainerDetail = new System.Windows.Forms.SplitContainer();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            richTextBoxDetail = new System.Windows.Forms.RichTextBox();
            statusStripMain = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabelLibrary = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabelCount = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabelSelected = new System.Windows.Forms.ToolStripStatusLabel();
            colorDialog = new System.Windows.Forms.ColorDialog();
            toolStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            contextMenuStripTileView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerDetail).BeginInit();
            splitContainerDetail.Panel1.SuspendLayout();
            splitContainerDetail.Panel2.SuspendLayout();
            splitContainerDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            statusStripMain.SuspendLayout();
            SuspendLayout();
            //
            // toolStripMain
            //
            toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelLibrary, toolStripComboBoxLibrary, toolStripButtonNew, toolStripButtonOpen, toolStripButtonReload, toolStripSeparator1, toolStripLabelId, toolStripTextBoxId, toolStripButtonGoTo, toolStripSeparator2, toolStripButtonOnlyOccupied });
            toolStripMain.Location = new System.Drawing.Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStripMain.Size = new System.Drawing.Size(1184, 28);
            toolStripMain.TabIndex = 0;
            //
            // toolStripLabelLibrary
            //
            toolStripLabelLibrary.Name = "toolStripLabelLibrary";
            toolStripLabelLibrary.Size = new System.Drawing.Size(64, 25);
            toolStripLabelLibrary.Text = "Bibliothek:";
            //
            // toolStripComboBoxLibrary
            //
            toolStripComboBoxLibrary.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxLibrary.DropDownWidth = 450;
            toolStripComboBoxLibrary.Name = "toolStripComboBoxLibrary";
            toolStripComboBoxLibrary.Size = new System.Drawing.Size(260, 28);
            toolStripComboBoxLibrary.ToolTipText = "Zuletzt geöffnete Bibliotheken";
            toolStripComboBoxLibrary.SelectedIndexChanged += ToolStripComboBoxLibrary_SelectedIndexChanged;
            //
            // toolStripButtonNew
            //
            toolStripButtonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonNew.Image = Properties.Resources.Add;
            toolStripButtonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonNew.Name = "toolStripButtonNew";
            toolStripButtonNew.Size = new System.Drawing.Size(23, 25);
            toolStripButtonNew.Text = "Neue Bibliothek";
            toolStripButtonNew.Click += ToolStripButtonNew_Click;
            //
            // toolStripButtonOpen
            //
            toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonOpen.Image = Properties.Resources.disk_load_image;
            toolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonOpen.Name = "toolStripButtonOpen";
            toolStripButtonOpen.Size = new System.Drawing.Size(23, 25);
            toolStripButtonOpen.Text = "Bibliothek öffnen";
            toolStripButtonOpen.Click += ToolStripButtonOpen_Click;
            //
            // toolStripButtonReload
            //
            toolStripButtonReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonReload.Image = Properties.Resources.reload;
            toolStripButtonReload.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonReload.Name = "toolStripButtonReload";
            toolStripButtonReload.Size = new System.Drawing.Size(23, 25);
            toolStripButtonReload.Text = "Neu laden";
            toolStripButtonReload.Click += ToolStripButtonReload_Click;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            //
            // toolStripLabelId
            //
            toolStripLabelId.Name = "toolStripLabelId";
            toolStripLabelId.Size = new System.Drawing.Size(21, 25);
            toolStripLabelId.Text = "ID:";
            //
            // toolStripTextBoxId
            //
            toolStripTextBoxId.Name = "toolStripTextBoxId";
            toolStripTextBoxId.Size = new System.Drawing.Size(100, 28);
            toolStripTextBoxId.ToolTipText = "Dezimal (1234) oder Hex (0x4D2, 4D2h) - Enter springt zur ID";
            toolStripTextBoxId.KeyDown += ToolStripTextBoxId_KeyDown;
            //
            // toolStripButtonGoTo
            //
            toolStripButtonGoTo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonGoTo.Image = Properties.Resources.jump;
            toolStripButtonGoTo.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonGoTo.Name = "toolStripButtonGoTo";
            toolStripButtonGoTo.Size = new System.Drawing.Size(23, 25);
            toolStripButtonGoTo.Text = "Gehe zu ID";
            toolStripButtonGoTo.Click += ToolStripButtonGoTo_Click;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            //
            // toolStripButtonOnlyOccupied
            //
            toolStripButtonOnlyOccupied.Checked = true;
            toolStripButtonOnlyOccupied.CheckOnClick = true;
            toolStripButtonOnlyOccupied.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonOnlyOccupied.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonOnlyOccupied.Name = "toolStripButtonOnlyOccupied";
            toolStripButtonOnlyOccupied.Size = new System.Drawing.Size(73, 25);
            toolStripButtonOnlyOccupied.Text = "Nur belegte";
            toolStripButtonOnlyOccupied.ToolTipText = "An: nur belegte IDs zeigen  -  Aus: alle IDs von 0 bis Ende zeigen";
            toolStripButtonOnlyOccupied.Click += ToolStripButtonOnlyOccupied_Click;
            //
            // splitContainerMain
            //
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.Location = new System.Drawing.Point(0, 28);
            splitContainerMain.Name = "splitContainerMain";
            //
            // splitContainerMain.Panel1
            //
            splitContainerMain.Panel1.Controls.Add(LibraryTileView);
            //
            // splitContainerMain.Panel2
            //
            splitContainerMain.Panel2.Controls.Add(splitContainerDetail);
            splitContainerMain.Size = new System.Drawing.Size(1184, 611);
            splitContainerMain.SplitterDistance = 850;
            splitContainerMain.SplitterWidth = 5;
            splitContainerMain.TabIndex = 1;
            //
            // LibraryTileView
            //
            LibraryTileView.AutoScroll = true;
            LibraryTileView.BackColor = System.Drawing.Color.White;
            LibraryTileView.ContextMenuStrip = contextMenuStripTileView;
            LibraryTileView.Dock = System.Windows.Forms.DockStyle.Fill;
            LibraryTileView.FocusIndex = -1;
            LibraryTileView.Location = new System.Drawing.Point(0, 0);
            LibraryTileView.MultiSelect = true;
            LibraryTileView.Name = "LibraryTileView";
            LibraryTileView.Size = new System.Drawing.Size(850, 611);
            LibraryTileView.TabIndex = 0;
            LibraryTileView.TileBackgroundColor = System.Drawing.SystemColors.Window;
            LibraryTileView.TileBorderColor = System.Drawing.Color.Gray;
            LibraryTileView.TileBorderWidth = 1F;
            LibraryTileView.TileFocusColor = System.Drawing.Color.DarkRed;
            LibraryTileView.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            LibraryTileView.TileMargin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            LibraryTileView.TilePadding = new System.Windows.Forms.Padding(1);
            LibraryTileView.TileSize = new System.Drawing.Size(48, 48);
            LibraryTileView.VirtualListSize = 0;
            LibraryTileView.ItemSelectionChanged += LibraryTileView_ItemSelectionChanged;
            LibraryTileView.FocusSelectionChanged += LibraryTileView_FocusSelectionChanged;
            LibraryTileView.DrawItem += LibraryTileView_DrawItem;
            LibraryTileView.KeyDown += LibraryTileView_KeyDown;
            //
            // contextMenuStripTileView
            //
            contextMenuStripTileView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { findNextFreeToolStripMenuItem, toolStripSeparator3, copyIdHexToolStripMenuItem, copyIdDecToolStripMenuItem, toolStripSeparator4, removeToolStripMenuItem, toolStripSeparator5, backgroundColorToolStripMenuItem });
            contextMenuStripTileView.Name = "contextMenuStripTileView";
            contextMenuStripTileView.Size = new System.Drawing.Size(230, 132);
            contextMenuStripTileView.Opening += ContextMenuStripTileView_Opening;
            //
            // findNextFreeToolStripMenuItem
            //
            findNextFreeToolStripMenuItem.Name = "findNextFreeToolStripMenuItem";
            findNextFreeToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            findNextFreeToolStripMenuItem.Text = "Nächste freie ID";
            findNextFreeToolStripMenuItem.Click += FindNextFreeToolStripMenuItem_Click;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(226, 6);
            //
            // copyIdHexToolStripMenuItem
            //
            copyIdHexToolStripMenuItem.Image = Properties.Resources.hexdecimal_adresse_to_clipbord;
            copyIdHexToolStripMenuItem.Name = "copyIdHexToolStripMenuItem";
            copyIdHexToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            copyIdHexToolStripMenuItem.Text = "ID kopieren (Hex)";
            copyIdHexToolStripMenuItem.Click += CopyIdHexToolStripMenuItem_Click;
            //
            // copyIdDecToolStripMenuItem
            //
            copyIdDecToolStripMenuItem.Name = "copyIdDecToolStripMenuItem";
            copyIdDecToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            copyIdDecToolStripMenuItem.Text = "ID kopieren (Dezimal)";
            copyIdDecToolStripMenuItem.Click += CopyIdDecToolStripMenuItem_Click;
            //
            // toolStripSeparator4
            //
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(226, 6);
            //
            // removeToolStripMenuItem
            //
            removeToolStripMenuItem.Image = Properties.Resources.Remove;
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.ShortcutKeyDisplayString = "Entf";
            removeToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            removeToolStripMenuItem.Text = "Entfernen";
            removeToolStripMenuItem.Click += RemoveToolStripMenuItem_Click;
            //
            // toolStripSeparator5
            //
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(226, 6);
            //
            // backgroundColorToolStripMenuItem
            //
            backgroundColorToolStripMenuItem.Image = Properties.Resources.colordialog_background;
            backgroundColorToolStripMenuItem.Name = "backgroundColorToolStripMenuItem";
            backgroundColorToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            backgroundColorToolStripMenuItem.Text = "Hintergrundfarbe ändern";
            backgroundColorToolStripMenuItem.Click += BackgroundColorToolStripMenuItem_Click;
            //
            // splitContainerDetail
            //
            splitContainerDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerDetail.Location = new System.Drawing.Point(0, 0);
            splitContainerDetail.Name = "splitContainerDetail";
            splitContainerDetail.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitContainerDetail.Panel1
            //
            splitContainerDetail.Panel1.Controls.Add(pictureBoxPreview);
            //
            // splitContainerDetail.Panel2
            //
            splitContainerDetail.Panel2.Controls.Add(richTextBoxDetail);
            splitContainerDetail.Size = new System.Drawing.Size(329, 611);
            splitContainerDetail.SplitterDistance = 380;
            splitContainerDetail.SplitterWidth = 5;
            splitContainerDetail.TabIndex = 0;
            //
            // pictureBoxPreview
            //
            pictureBoxPreview.BackColor = System.Drawing.Color.White;
            pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxPreview.Location = new System.Drawing.Point(0, 0);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new System.Drawing.Size(329, 380);
            pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxPreview.TabIndex = 0;
            pictureBoxPreview.TabStop = false;
            //
            // richTextBoxDetail
            //
            richTextBoxDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBoxDetail.Font = new System.Drawing.Font("Consolas", 9F);
            richTextBoxDetail.Location = new System.Drawing.Point(0, 0);
            richTextBoxDetail.Name = "richTextBoxDetail";
            richTextBoxDetail.ReadOnly = true;
            richTextBoxDetail.Size = new System.Drawing.Size(329, 226);
            richTextBoxDetail.TabIndex = 0;
            richTextBoxDetail.Text = "";
            //
            // statusStripMain
            //
            statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabelLibrary, toolStripStatusLabelCount, toolStripStatusLabelSelected });
            statusStripMain.Location = new System.Drawing.Point(0, 639);
            statusStripMain.Name = "statusStripMain";
            statusStripMain.Size = new System.Drawing.Size(1184, 22);
            statusStripMain.TabIndex = 2;
            //
            // toolStripStatusLabelLibrary
            //
            toolStripStatusLabelLibrary.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            toolStripStatusLabelLibrary.Name = "toolStripStatusLabelLibrary";
            toolStripStatusLabelLibrary.Size = new System.Drawing.Size(150, 17);
            toolStripStatusLabelLibrary.Text = "Keine Bibliothek geöffnet";
            //
            // toolStripStatusLabelCount
            //
            toolStripStatusLabelCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            toolStripStatusLabelCount.Name = "toolStripStatusLabelCount";
            toolStripStatusLabelCount.Size = new System.Drawing.Size(4, 17);
            //
            // toolStripStatusLabelSelected
            //
            toolStripStatusLabelSelected.Name = "toolStripStatusLabelSelected";
            toolStripStatusLabelSelected.Size = new System.Drawing.Size(0, 17);
            //
            // ArtLibraryForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1184, 661);
            Controls.Add(splitContainerMain);
            Controls.Add(statusStripMain);
            Controls.Add(toolStripMain);
            MinimumSize = new System.Drawing.Size(640, 400);
            Name = "ArtLibraryForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Art-Bibliothek";
            FormClosed += ArtLibraryForm_FormClosed;
            Shown += ArtLibraryForm_Shown;
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            contextMenuStripTileView.ResumeLayout(false);
            splitContainerDetail.Panel1.ResumeLayout(false);
            splitContainerDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerDetail).EndInit();
            splitContainerDetail.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            statusStripMain.ResumeLayout(false);
            statusStripMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripLabel toolStripLabelLibrary;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxLibrary;
        private System.Windows.Forms.ToolStripButton toolStripButtonNew;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripButton toolStripButtonReload;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabelId;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxId;
        private System.Windows.Forms.ToolStripButton toolStripButtonGoTo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonOnlyOccupied;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl LibraryTileView;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTileView;
        private System.Windows.Forms.ToolStripMenuItem findNextFreeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem copyIdHexToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyIdDecToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem backgroundColorToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerDetail;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.RichTextBox richTextBoxDetail;
        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLibrary;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCount;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSelected;
        private System.Windows.Forms.ColorDialog colorDialog;
    }
}
