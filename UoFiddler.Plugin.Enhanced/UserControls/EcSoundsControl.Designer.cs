/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    partial class EcSoundsControl
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
            toolStrip = new System.Windows.Forms.ToolStrip();
            toolStripComboBoxKind = new System.Windows.Forms.ToolStripComboBox();
            toolStripLabelSearch = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonApply = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonPlay = new System.Windows.Forms.ToolStripButton();
            toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonExport = new System.Windows.Forms.ToolStripButton();
            toolStripButtonReplace = new System.Windows.Forms.ToolStripButton();
            toolStripButtonAdd = new System.Windows.Forms.ToolStripButton();
            toolStripButtonChangePath = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRemove = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            listViewSounds = new System.Windows.Forms.ListView();
            columnHeaderId = new System.Windows.Forms.ColumnHeader();
            columnHeaderFile = new System.Windows.Forms.ColumnHeader();
            columnHeaderSize = new System.Windows.Forms.ColumnHeader();
            columnHeaderMore = new System.Windows.Forms.ColumnHeader();
            contextMenuStripList = new System.Windows.Forms.ContextMenuStrip(components);
            playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu1 = new System.Windows.Forms.ToolStripSeparator();
            exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            changePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu2 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            contextMenuStripList.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripComboBoxKind, toolStripLabelSearch, toolStripTextBoxSearch, toolStripButtonApply, toolStripSeparator1, toolStripButtonPlay, toolStripButtonStop, toolStripSeparator2, toolStripButtonExport, toolStripButtonReplace, toolStripButtonAdd, toolStripButtonChangePath, toolStripButtonRemove, toolStripSeparator3, toolStripButtonSave, toolStripButtonSaveAs });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripComboBoxKind
            //
            toolStripComboBoxKind.Items.AddRange(new object[] { "Sounds", "Music", "Files in no list" });
            toolStripComboBoxKind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxKind.Name = "toolStripComboBoxKind";
            toolStripComboBoxKind.Size = new System.Drawing.Size(140, 25);
            toolStripComboBoxKind.SelectedIndexChanged += OnKindChanged;
            //
            // toolStripLabelSearch
            //
            toolStripLabelSearch.Name = "toolStripLabelSearch";
            toolStripLabelSearch.Text = "Search:";
            //
            // toolStripTextBoxSearch
            //
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new System.Drawing.Size(160, 25);
            toolStripTextBoxSearch.ToolTipText = "Id or part of the path – Enter";
            toolStripTextBoxSearch.KeyDown += OnSearchKeyDown;
            //
            // toolStripButtonApply
            //
            toolStripButtonApply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonApply.Name = "toolStripButtonApply";
            toolStripButtonApply.Text = "Apply";
            toolStripButtonApply.Click += OnClickApply;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripButtonPlay
            //
            toolStripButtonPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonPlay.Name = "toolStripButtonPlay";
            toolStripButtonPlay.Text = "Play";
            toolStripButtonPlay.ToolTipText = "wav: here, mp3/ogg: with the Windows player";
            toolStripButtonPlay.Click += OnClickPlay;
            //
            // toolStripButtonStop
            //
            toolStripButtonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonStop.Name = "toolStripButtonStop";
            toolStripButtonStop.Text = "Stop";
            toolStripButtonStop.Click += OnClickStop;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripButtonExport
            //
            toolStripButtonExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonExport.Name = "toolStripButtonExport";
            toolStripButtonExport.Text = "Export...";
            toolStripButtonExport.Click += OnClickExport;
            //
            // toolStripButtonReplace
            //
            toolStripButtonReplace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonReplace.Name = "toolStripButtonReplace";
            toolStripButtonReplace.Text = "Replace...";
            toolStripButtonReplace.Click += OnClickReplace;
            //
            // toolStripButtonAdd
            //
            toolStripButtonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAdd.Name = "toolStripButtonAdd";
            toolStripButtonAdd.Text = "Add...";
            toolStripButtonAdd.Click += OnClickAdd;
            //
            // toolStripButtonChangePath
            //
            toolStripButtonChangePath.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonChangePath.Name = "toolStripButtonChangePath";
            toolStripButtonChangePath.Text = "Change path...";
            toolStripButtonChangePath.Click += OnClickChangePath;
            //
            // toolStripButtonRemove
            //
            toolStripButtonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRemove.Name = "toolStripButtonRemove";
            toolStripButtonRemove.Text = "Remove";
            toolStripButtonRemove.Click += OnClickRemove;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Enabled = false;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Text = "Save";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripButtonSaveAs
            //
            toolStripButtonSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSaveAs.Name = "toolStripButtonSaveAs";
            toolStripButtonSaveAs.Text = "Save as...";
            toolStripButtonSaveAs.Click += OnClickSaveAs;
            //
            // listViewSounds
            //
            listViewSounds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderId, columnHeaderFile, columnHeaderSize, columnHeaderMore });
            listViewSounds.ContextMenuStrip = contextMenuStripList;
            listViewSounds.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewSounds.FullRowSelect = true;
            listViewSounds.HideSelection = false;
            listViewSounds.MultiSelect = false;
            listViewSounds.Name = "listViewSounds";
            listViewSounds.TabIndex = 1;
            listViewSounds.UseCompatibleStateImageBehavior = false;
            listViewSounds.View = System.Windows.Forms.View.Details;
            listViewSounds.VirtualMode = true;
            listViewSounds.RetrieveVirtualItem += RetrieveItem;
            listViewSounds.DoubleClick += OnClickPlay;
            //
            // columnHeaderId
            //
            columnHeaderId.Text = "Id";
            columnHeaderId.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderId.Width = 70;
            //
            // columnHeaderFile
            //
            columnHeaderFile.Text = "File";
            columnHeaderFile.Width = 420;
            //
            // columnHeaderSize
            //
            columnHeaderSize.Text = "Size";
            columnHeaderSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderSize.Width = 80;
            //
            // columnHeaderMore
            //
            columnHeaderMore.Text = "More";
            columnHeaderMore.Width = 80;
            //
            // contextMenuStripList
            //
            contextMenuStripList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { playToolStripMenuItem, stopToolStripMenuItem, toolStripSeparatorMenu1, exportToolStripMenuItem, replaceToolStripMenuItem, addToolStripMenuItem, changePathToolStripMenuItem, removeToolStripMenuItem, toolStripSeparatorMenu2, saveToolStripMenuItem });
            contextMenuStripList.Name = "contextMenuStripList";
            //
            // playToolStripMenuItem
            //
            playToolStripMenuItem.Name = "playToolStripMenuItem";
            playToolStripMenuItem.Text = "Play";
            playToolStripMenuItem.Click += OnClickPlay;
            //
            // stopToolStripMenuItem
            //
            stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            stopToolStripMenuItem.Text = "Stop";
            stopToolStripMenuItem.Click += OnClickStop;
            //
            // toolStripSeparatorMenu1
            //
            toolStripSeparatorMenu1.Name = "toolStripSeparatorMenu1";
            //
            // exportToolStripMenuItem
            //
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Text = "Export...";
            exportToolStripMenuItem.Click += OnClickExport;
            //
            // replaceToolStripMenuItem
            //
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Text = "Replace...";
            replaceToolStripMenuItem.Click += OnClickReplace;
            //
            // addToolStripMenuItem
            //
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Text = "Add...";
            addToolStripMenuItem.Click += OnClickAdd;
            //
            // changePathToolStripMenuItem
            //
            changePathToolStripMenuItem.Name = "changePathToolStripMenuItem";
            changePathToolStripMenuItem.Text = "Change path...";
            changePathToolStripMenuItem.Click += OnClickChangePath;
            //
            // removeToolStripMenuItem
            //
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemove;
            //
            // toolStripSeparatorMenu2
            //
            toolStripSeparatorMenu2.Name = "toolStripSeparatorMenu2";
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 2;
            //
            // toolStripStatusLabel
            //
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Spring = true;
            toolStripStatusLabel.Text = "Choose the Enhanced Client folder on top.";
            toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // EcSoundsControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(listViewSounds);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcSoundsControl";
            Size = new System.Drawing.Size(900, 500);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            contextMenuStripList.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxKind;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSearch;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonApply;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonPlay;
        private System.Windows.Forms.ToolStripButton toolStripButtonStop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonExport;
        private System.Windows.Forms.ToolStripButton toolStripButtonReplace;
        private System.Windows.Forms.ToolStripButton toolStripButtonAdd;
        private System.Windows.Forms.ToolStripButton toolStripButtonChangePath;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.ListView listViewSounds;
        private System.Windows.Forms.ColumnHeader columnHeaderId;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderMore;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripList;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu1;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu2;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}
