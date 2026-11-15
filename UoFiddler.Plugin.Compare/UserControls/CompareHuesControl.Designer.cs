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
    partial class CompareHuesControl
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
            if (disposing)
            {
                DisposeExtras();
                components?.Dispose();
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
            this.components = new System.ComponentModel.Container();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.listPanel = new System.Windows.Forms.Panel();
            this._list = new System.Windows.Forms.PictureBox();
            this._scroll = new System.Windows.Forms.VScrollBar();
            this.detailLeft = new System.Windows.Forms.Panel();
            this._detailText = new System.Windows.Forms.Label();
            this._detail = new System.Windows.Forms.PictureBox();
            this._preview = new UoFiddler.Controls.UserControls.HuePreviewPanel();
            this.toolStripApply = new System.Windows.Forms.ToolStrip();
            this.apply21Button = new System.Windows.Forms.ToolStripSplitButton();
            this.apply21ColorsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.apply21NameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.apply21AllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.apply12Button = new System.Windows.Forms.ToolStripSplitButton();
            this.apply12ColorsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.apply12NameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.apply12AllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._btnUndo = new System.Windows.Forms.ToolStripButton();
            this._btnRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveHues1Button = new System.Windows.Forms.ToolStripButton();
            this.saveHues2Button = new System.Windows.Forms.ToolStripButton();
            this.exportDiffButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripFile = new System.Windows.Forms.ToolStrip();
            this.hues2Label = new System.Windows.Forms.ToolStripLabel();
            this._txtPath = new System.Windows.Forms.ToolStripTextBox();
            this.folderButton = new System.Windows.Forms.ToolStripButton();
            this.fileButton = new System.Windows.Forms.ToolStripButton();
            this.loadButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.showLabel = new System.Windows.Forms.ToolStripLabel();
            this._cmbFilter = new System.Windows.Forms.ToolStripComboBox();
            this.prevButton = new System.Windows.Forms.ToolStripButton();
            this.nextButton = new System.Windows.Forms.ToolStripButton();
            this._lblCount = new System.Windows.Forms.ToolStripLabel();
            this.listMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuApply21All = new System.Windows.Forms.ToolStripMenuItem();
            this.menuApply21Colors = new System.Windows.Forms.ToolStripMenuItem();
            this.menuApply21Name = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuApply12All = new System.Windows.Forms.ToolStripMenuItem();
            this.menuApply12Colors = new System.Windows.Forms.ToolStripMenuItem();
            this.menuApply12Name = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuCopyNumber = new System.Windows.Forms.ToolStripMenuItem();
            this.menuCopyServUO = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRedo = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.listPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._list)).BeginInit();
            this.detailLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._detail)).BeginInit();
            this.toolStripApply.SuspendLayout();
            this.toolStripFile.SuspendLayout();
            this.listMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.listPanel);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.detailLeft);
            this.splitContainer.Panel2.Controls.Add(this._preview);
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitContainer.Size = new System.Drawing.Size(900, 500);
            this.splitContainer.SplitterDistance = 300;
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.TabIndex = 0;
            // 
            // listPanel
            // 
            this.listPanel.Controls.Add(this._list);
            this.listPanel.Controls.Add(this._scroll);
            this.listPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listPanel.Name = "listPanel";
            this.listPanel.TabIndex = 1;
            // 
            // _list
            // 
            this._list.BackColor = System.Drawing.Color.White;
            this._list.ContextMenuStrip = this.listMenu;
            this._list.Dock = System.Windows.Forms.DockStyle.Fill;
            this._list.Name = "_list";
            this._list.TabIndex = 2;
            this._list.TabStop = false;
            this._list.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintList);
            this._list.Resize += new System.EventHandler(this.OnListResize);
            this._list.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnListMouseDown);
            // 
            // _scroll
            // 
            this._scroll.Dock = System.Windows.Forms.DockStyle.Right;
            this._scroll.Name = "_scroll";
            this._scroll.TabIndex = 3;
            this._scroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            // 
            // detailLeft
            // 
            this.detailLeft.Controls.Add(this._detailText);
            this.detailLeft.Controls.Add(this._detail);
            this.detailLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailLeft.Name = "detailLeft";
            this.detailLeft.TabIndex = 4;
            // 
            // _detailText
            // 
            this._detailText.Dock = System.Windows.Forms.DockStyle.Fill;
            this._detailText.Font = new System.Drawing.Font("Consolas", 8.5F);
            this._detailText.Name = "_detailText";
            this._detailText.Padding = new System.Windows.Forms.Padding(6, 2, 2, 2);
            // 
            // _detail
            // 
            this._detail.Dock = System.Windows.Forms.DockStyle.Top;
            this._detail.Name = "_detail";
            this._detail.Size = new System.Drawing.Size(480, 74);
            this._detail.TabIndex = 5;
            this._detail.TabStop = false;
            this._detail.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintDetail);
            this._detail.Resize += new System.EventHandler(this.OnDetailResize);
            // 
            // _preview
            // 
            this._preview.Dock = System.Windows.Forms.DockStyle.Right;
            this._preview.Name = "_preview";
            this._preview.Size = new System.Drawing.Size(420, 191);
            this._preview.TabIndex = 6;
            // 
            // toolStripApply
            // 
            this.toolStripApply.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.apply21Button,
            this.apply12Button,
            this._btnUndo,
            this._btnRedo,
            this.toolStripSeparator2,
            this.saveHues1Button,
            this.saveHues2Button,
            this.exportDiffButton});
            this.toolStripApply.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripApply.Name = "toolStripApply";
            this.toolStripApply.Padding = new System.Windows.Forms.Padding(2);
            this.toolStripApply.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripApply.TabIndex = 7;
            // 
            // apply21Button
            // 
            this.apply21Button.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.apply21ColorsMenuItem,
            this.apply21NameMenuItem,
            this.apply21AllMenuItem});
            this.apply21Button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.apply21Button.Name = "apply21Button";
            this.apply21Button.Text = "Apply 2 → 1";
            this.apply21Button.ToolTipText = "Copy the selected hues of Hues 2 into Hues 1 (colours, name and table values)";
            this.apply21Button.ButtonClick += new System.EventHandler(this.OnClickApply21);
            // 
            // apply21ColorsMenuItem
            // 
            this.apply21ColorsMenuItem.Name = "apply21ColorsMenuItem";
            this.apply21ColorsMenuItem.Text = "only colours";
            this.apply21ColorsMenuItem.Click += new System.EventHandler(this.OnClickApply21Colors);
            // 
            // apply21NameMenuItem
            // 
            this.apply21NameMenuItem.Name = "apply21NameMenuItem";
            this.apply21NameMenuItem.Text = "only name";
            this.apply21NameMenuItem.Click += new System.EventHandler(this.OnClickApply21Name);
            // 
            // apply21AllMenuItem
            // 
            this.apply21AllMenuItem.Name = "apply21AllMenuItem";
            this.apply21AllMenuItem.Text = "all differences (shown filter)";
            this.apply21AllMenuItem.Click += new System.EventHandler(this.OnClickApply21All);
            // 
            // apply12Button
            // 
            this.apply12Button.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.apply12ColorsMenuItem,
            this.apply12NameMenuItem,
            this.apply12AllMenuItem});
            this.apply12Button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.apply12Button.Name = "apply12Button";
            this.apply12Button.Text = "Apply 1 → 2";
            this.apply12Button.ToolTipText = "Copy the selected hues of Hues 1 into Hues 2";
            this.apply12Button.ButtonClick += new System.EventHandler(this.OnClickApply12);
            // 
            // apply12ColorsMenuItem
            // 
            this.apply12ColorsMenuItem.Name = "apply12ColorsMenuItem";
            this.apply12ColorsMenuItem.Text = "only colours";
            this.apply12ColorsMenuItem.Click += new System.EventHandler(this.OnClickApply12Colors);
            // 
            // apply12NameMenuItem
            // 
            this.apply12NameMenuItem.Name = "apply12NameMenuItem";
            this.apply12NameMenuItem.Text = "only name";
            this.apply12NameMenuItem.Click += new System.EventHandler(this.OnClickApply12Name);
            // 
            // apply12AllMenuItem
            // 
            this.apply12AllMenuItem.Name = "apply12AllMenuItem";
            this.apply12AllMenuItem.Text = "all differences (shown filter)";
            this.apply12AllMenuItem.Click += new System.EventHandler(this.OnClickApply12All);
            // 
            // _btnUndo
            // 
            this._btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnUndo.Name = "_btnUndo";
            this._btnUndo.Text = "↶ Undo";
            this._btnUndo.ToolTipText = "Undo (Ctrl+Z)";
            this._btnUndo.Click += new System.EventHandler(this.OnClickUndo);
            // 
            // _btnRedo
            // 
            this._btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnRedo.Name = "_btnRedo";
            this._btnRedo.Text = "↷ Redo";
            this._btnRedo.ToolTipText = "Redo (Ctrl+Y)";
            this._btnRedo.Click += new System.EventHandler(this.OnClickRedo);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // saveHues1Button
            // 
            this.saveHues1Button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.saveHues1Button.Name = "saveHues1Button";
            this.saveHues1Button.Text = "Save Hues 1";
            this.saveHues1Button.ToolTipText = "Write the loaded hues (Hues 1) into the output folder";
            this.saveHues1Button.Click += new System.EventHandler(this.OnClickSaveFirst);
            // 
            // saveHues2Button
            // 
            this.saveHues2Button.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.saveHues2Button.Name = "saveHues2Button";
            this.saveHues2Button.Text = "Save Hues 2…";
            this.saveHues2Button.ToolTipText = "Write Hues 2 into a hues.mul (backup .bak)";
            this.saveHues2Button.Click += new System.EventHandler(this.OnClickSaveSecond);
            // 
            // exportDiffButton
            // 
            this.exportDiffButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exportDiffButton.Name = "exportDiffButton";
            this.exportDiffButton.Text = "Export differences…";
            this.exportDiffButton.ToolTipText = "List of all differences as CSV";
            this.exportDiffButton.Click += new System.EventHandler(this.OnClickExportDiff);
            // 
            // toolStripFile
            // 
            this.toolStripFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hues2Label,
            this._txtPath,
            this.folderButton,
            this.fileButton,
            this.loadButton,
            this.toolStripSeparator1,
            this.showLabel,
            this._cmbFilter,
            this.prevButton,
            this.nextButton,
            this._lblCount});
            this.toolStripFile.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripFile.Name = "toolStripFile";
            this.toolStripFile.Padding = new System.Windows.Forms.Padding(2);
            this.toolStripFile.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripFile.TabIndex = 8;
            // 
            // hues2Label
            // 
            this.hues2Label.Name = "hues2Label";
            this.hues2Label.Text = "Hues 2:";
            // 
            // _txtPath
            // 
            this._txtPath.AutoSize = false;
            this._txtPath.Name = "_txtPath";
            this._txtPath.Size = new System.Drawing.Size(260, 25);
            this._txtPath.ToolTipText = "Folder with hues.mul or a hues.mul file (e.g. a backup)";
            this._txtPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnPathKeyDown);
            // 
            // folderButton
            // 
            this.folderButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.folderButton.Name = "folderButton";
            this.folderButton.Text = "Folder…";
            this.folderButton.ToolTipText = "Folder that contains hues.mul";
            this.folderButton.Click += new System.EventHandler(this.OnClickBrowseFolder);
            // 
            // fileButton
            // 
            this.fileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.fileButton.Name = "fileButton";
            this.fileButton.Text = "File…";
            this.fileButton.ToolTipText = "Any hues.mul file";
            this.fileButton.Click += new System.EventHandler(this.OnClickBrowseFile);
            // 
            // loadButton
            // 
            this.loadButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.loadButton.Name = "loadButton";
            this.loadButton.Text = "Load";
            this.loadButton.Click += new System.EventHandler(this.OnClickLoad);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // showLabel
            // 
            this.showLabel.Name = "showLabel";
            this.showLabel.Text = "Show:";
            // 
            // _cmbFilter
            // 
            this._cmbFilter.Items.AddRange(new object[] {
            "All",
            "Differences (any)",
            "Colours differ",
            "Name differs",
            "Table values differ",
            "Only in Hues 1",
            "Only in Hues 2",
            "Selected"});
            this._cmbFilter.AutoSize = false;
            this._cmbFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbFilter.Name = "_cmbFilter";
            this._cmbFilter.Size = new System.Drawing.Size(150, 25);
            this._cmbFilter.SelectedIndexChanged += new System.EventHandler(this.OnFilterChanged);
            // 
            // prevButton
            // 
            this.prevButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.prevButton.Name = "prevButton";
            this.prevButton.Text = "◄ Prev";
            this.prevButton.ToolTipText = "Previous difference (Shift+F3)";
            this.prevButton.Click += new System.EventHandler(this.OnClickPrev);
            // 
            // nextButton
            // 
            this.nextButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.nextButton.Name = "nextButton";
            this.nextButton.Text = "Next ►";
            this.nextButton.ToolTipText = "Next difference (F3)";
            this.nextButton.Click += new System.EventHandler(this.OnClickNext);
            // 
            // _lblCount
            // 
            this._lblCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblCount.ForeColor = System.Drawing.Color.DarkRed;
            this._lblCount.Name = "_lblCount";
            // 
            // listMenu
            // 
            this.listMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuApply21All,
            this.menuApply21Colors,
            this.menuApply21Name,
            this.menuSeparator1,
            this.menuApply12All,
            this.menuApply12Colors,
            this.menuApply12Name,
            this.menuSeparator2,
            this.menuCopyNumber,
            this.menuCopyServUO,
            this.menuSeparator3,
            this.menuUndo,
            this.menuRedo});
            this.listMenu.Name = "listMenu";
            this.listMenu.Opening += new System.ComponentModel.CancelEventHandler(this.OnListMenuOpening);
            // 
            // menuApply21All
            // 
            this.menuApply21All.Name = "menuApply21All";
            this.menuApply21All.Text = "Apply 2 → 1 (everything)";
            this.menuApply21All.Click += new System.EventHandler(this.OnClickApply21);
            // 
            // menuApply21Colors
            // 
            this.menuApply21Colors.Name = "menuApply21Colors";
            this.menuApply21Colors.Text = "Apply 2 → 1 (only colours)";
            this.menuApply21Colors.Click += new System.EventHandler(this.OnClickApply21Colors);
            // 
            // menuApply21Name
            // 
            this.menuApply21Name.Name = "menuApply21Name";
            this.menuApply21Name.Text = "Apply 2 → 1 (only name)";
            this.menuApply21Name.Click += new System.EventHandler(this.OnClickApply21Name);
            // 
            // menuSeparator1
            // 
            this.menuSeparator1.Name = "menuSeparator1";
            // 
            // menuApply12All
            // 
            this.menuApply12All.Name = "menuApply12All";
            this.menuApply12All.Text = "Apply 1 → 2 (everything)";
            this.menuApply12All.Click += new System.EventHandler(this.OnClickApply12);
            // 
            // menuApply12Colors
            // 
            this.menuApply12Colors.Name = "menuApply12Colors";
            this.menuApply12Colors.Text = "Apply 1 → 2 (only colours)";
            this.menuApply12Colors.Click += new System.EventHandler(this.OnClickApply12Colors);
            // 
            // menuApply12Name
            // 
            this.menuApply12Name.Name = "menuApply12Name";
            this.menuApply12Name.Text = "Apply 1 → 2 (only name)";
            this.menuApply12Name.Click += new System.EventHandler(this.OnClickApply12Name);
            // 
            // menuSeparator2
            // 
            this.menuSeparator2.Name = "menuSeparator2";
            // 
            // menuCopyNumber
            // 
            this.menuCopyNumber.Name = "menuCopyNumber";
            this.menuCopyNumber.Text = "Copy hue number";
            this.menuCopyNumber.Click += new System.EventHandler(this.OnClickCopyNumber);
            // 
            // menuCopyServUO
            // 
            this.menuCopyServUO.Name = "menuCopyServUO";
            this.menuCopyServUO.Text = "Copy for ServUO (Hue = 0x…)";
            this.menuCopyServUO.Click += new System.EventHandler(this.OnClickCopyServUO);
            // 
            // menuSeparator3
            // 
            this.menuSeparator3.Name = "menuSeparator3";
            // 
            // menuUndo
            // 
            this.menuUndo.Name = "menuUndo";
            this.menuUndo.Text = "Undo (Ctrl+Z)";
            this.menuUndo.Click += new System.EventHandler(this.OnClickUndo);
            // 
            // menuRedo
            // 
            this.menuRedo.Name = "menuRedo";
            this.menuRedo.Text = "Redo (Ctrl+Y)";
            this.menuRedo.Click += new System.EventHandler(this.OnClickRedo);
            // 
            // CompareHuesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Size = new System.Drawing.Size(900, 560);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.toolStripApply);
            this.Controls.Add(this.toolStripFile);
            this.Name = "CompareHuesControl";
            this.Load += new System.EventHandler(this.OnLoad);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.listPanel.ResumeLayout(false);
            this.listPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._list)).EndInit();
            this.detailLeft.ResumeLayout(false);
            this.detailLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._detail)).EndInit();
            this.toolStripApply.ResumeLayout(false);
            this.toolStripApply.PerformLayout();
            this.toolStripFile.ResumeLayout(false);
            this.toolStripFile.PerformLayout();
            this.listMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel listPanel;
        private System.Windows.Forms.PictureBox _list;
        private System.Windows.Forms.VScrollBar _scroll;
        private System.Windows.Forms.Panel detailLeft;
        private System.Windows.Forms.Label _detailText;
        private System.Windows.Forms.PictureBox _detail;
        private UoFiddler.Controls.UserControls.HuePreviewPanel _preview;
        private System.Windows.Forms.ToolStrip toolStripApply;
        private System.Windows.Forms.ToolStripSplitButton apply21Button;
        private System.Windows.Forms.ToolStripMenuItem apply21ColorsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem apply21NameMenuItem;
        private System.Windows.Forms.ToolStripMenuItem apply21AllMenuItem;
        private System.Windows.Forms.ToolStripSplitButton apply12Button;
        private System.Windows.Forms.ToolStripMenuItem apply12ColorsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem apply12NameMenuItem;
        private System.Windows.Forms.ToolStripMenuItem apply12AllMenuItem;
        private System.Windows.Forms.ToolStripButton _btnUndo;
        private System.Windows.Forms.ToolStripButton _btnRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton saveHues1Button;
        private System.Windows.Forms.ToolStripButton saveHues2Button;
        private System.Windows.Forms.ToolStripButton exportDiffButton;
        private System.Windows.Forms.ToolStrip toolStripFile;
        private System.Windows.Forms.ToolStripLabel hues2Label;
        private System.Windows.Forms.ToolStripTextBox _txtPath;
        private System.Windows.Forms.ToolStripButton folderButton;
        private System.Windows.Forms.ToolStripButton fileButton;
        private System.Windows.Forms.ToolStripButton loadButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel showLabel;
        private System.Windows.Forms.ToolStripComboBox _cmbFilter;
        private System.Windows.Forms.ToolStripButton prevButton;
        private System.Windows.Forms.ToolStripButton nextButton;
        private System.Windows.Forms.ToolStripLabel _lblCount;
        private System.Windows.Forms.ContextMenuStrip listMenu;
        private System.Windows.Forms.ToolStripMenuItem menuApply21All;
        private System.Windows.Forms.ToolStripMenuItem menuApply21Colors;
        private System.Windows.Forms.ToolStripMenuItem menuApply21Name;
        private System.Windows.Forms.ToolStripSeparator menuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuApply12All;
        private System.Windows.Forms.ToolStripMenuItem menuApply12Colors;
        private System.Windows.Forms.ToolStripMenuItem menuApply12Name;
        private System.Windows.Forms.ToolStripSeparator menuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuCopyNumber;
        private System.Windows.Forms.ToolStripMenuItem menuCopyServUO;
        private System.Windows.Forms.ToolStripSeparator menuSeparator3;
        private System.Windows.Forms.ToolStripMenuItem menuUndo;
        private System.Windows.Forms.ToolStripMenuItem menuRedo;
    }
}
