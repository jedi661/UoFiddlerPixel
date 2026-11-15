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
    partial class HuesControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HuesControl));
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ReplaceText = new System.Windows.Forms.ToolStripTextBox();
            exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ExportAllHueNamesListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            vScrollBar = new System.Windows.Forms.VScrollBar();
            pictureBox = new System.Windows.Forms.PictureBox();
            HuesTopMenuToolStrip = new System.Windows.Forms.ToolStrip();
            HueIndexToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            HueIndexToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            HueNameToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            HueNameToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            SearchNameToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorFilter = new System.Windows.Forms.ToolStripSeparator();
            this.filterLabel = new System.Windows.Forms.ToolStripLabel();
            this._filterBox = new System.Windows.Forms.ToolStripComboBox();
            this._gridButton = new System.Windows.Forms.ToolStripButton();
            this._countLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorUndo = new System.Windows.Forms.ToolStripSeparator();
            this._undoButton = new System.Windows.Forms.ToolStripButton();
            this._redoButton = new System.Windows.Forms.ToolStripButton();
            this.editMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.extSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.copyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.swapMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.gradientMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromImageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.similarMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.servUOMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exportRangeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importRangeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paletteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.palettePngMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paletteGplMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paletteActMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.undoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitterSide = new System.Windows.Forms.Splitter();
            this.sidePanel = new System.Windows.Forms.Panel();
            this.sideTabs = new System.Windows.Forms.TabControl();
            this.infoPage = new System.Windows.Forms.TabPage();
            this._preview = new UoFiddler.Controls.UserControls.HuePreviewPanel();
            this.infoButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnServUO = new System.Windows.Forms.Button();
            this.btnRevert = new System.Windows.Forms.Button();
            this.btnGradient = new System.Windows.Forms.Button();
            this.btnFromImage = new System.Windows.Forms.Button();
            this._info = new System.Windows.Forms.Label();
            this._strip = new System.Windows.Forms.PictureBox();
            this.usagePage = new System.Windows.Forms.TabPage();
            this._usage = new System.Windows.Forms.ListBox();
            this._usageTitle = new System.Windows.Forms.Label();
            this.usageTop = new System.Windows.Forms.FlowLayoutPanel();
            this._chkFollowUsage = new System.Windows.Forms.CheckBox();
            this.btnScanAgain = new System.Windows.Forms.Button();
            this.usageHint = new System.Windows.Forms.Label();
            this.findPage = new System.Windows.Forms.TabPage();
            this._results = new System.Windows.Forms.ListBox();
            this._resultsTitle = new System.Windows.Forms.Label();
            this.findTop = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSimilar = new System.Windows.Forms.Button();
            this.colourLabel = new System.Windows.Forms.Label();
            this._colorText = new System.Windows.Forms.TextBox();
            this.btnPickColor = new System.Windows.Forms.Button();
            this.btnFindColor = new System.Windows.Forms.Button();
            this.findHint = new System.Windows.Forms.Label();
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            contextMenuStrip1.SuspendLayout();
            this.sidePanel.SuspendLayout();
            this.sideTabs.SuspendLayout();
            this.infoPage.SuspendLayout();
            this.infoButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._strip)).BeginInit();
            this.usagePage.SuspendLayout();
            this.usageTop.SuspendLayout();
            this.findPage.SuspendLayout();
            this.findTop.SuspendLayout();
            toolStripContainer.ContentPanel.SuspendLayout();
            toolStripContainer.TopToolStripPanel.SuspendLayout();
            toolStripContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            HuesTopMenuToolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.editMenuItem, this.renameMenuItem, this.editSeparator, saveToolStripMenuItem, replaceToolStripMenuItem, exportToolStripMenuItem, importToolStripMenuItem, ExportAllHueNamesListToolStripMenuItem, this.extSeparator1, this.copyMenuItem, this.pasteMenuItem, this.clearMenuItem, this.copyToMenuItem, this.swapMenuItem, this.revertMenuItem, this.extSeparator2, this.gradientMenuItem, this.fromImageMenuItem, this.extSeparator3, this.similarMenuItem, this.servUOMenuItem, this.extSeparator4, this.exportRangeMenuItem, this.importRangeMenuItem, this.paletteMenuItem, this.extSeparator5, this.undoMenuItem, this.redoMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(203, 114);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Image = Properties.Resources.save;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.ToolTipText = "Saving colors.";
            saveToolStripMenuItem.Click += OnClickSave;
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ReplaceText });
            replaceToolStripMenuItem.Image = Properties.Resources.replace;
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            replaceToolStripMenuItem.Text = "Replace with hue…";
            replaceToolStripMenuItem.ToolTipText = "Enter the hue number and press Enter (applies to all selected hues)";
            // 
            // ReplaceText
            // 
            ReplaceText.Name = "ReplaceText";
            ReplaceText.Size = new System.Drawing.Size(100, 23);
            ReplaceText.KeyDown += OnKeyDownReplace;
            ReplaceText.TextChanged += OnTextChangedReplace;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Image = Properties.Resources.Export;
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            exportToolStripMenuItem.Text = "Export..";
            exportToolStripMenuItem.ToolTipText = "Exporting colors.";
            exportToolStripMenuItem.Click += OnExport;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Image = Properties.Resources.import;
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            importToolStripMenuItem.Text = "Import..";
            importToolStripMenuItem.ToolTipText = "Importing colors.";
            importToolStripMenuItem.Click += OnImport;
            // 
            // ExportAllHueNamesListToolStripMenuItem
            // 
            ExportAllHueNamesListToolStripMenuItem.Image = Properties.Resources.Text;
            ExportAllHueNamesListToolStripMenuItem.Name = "ExportAllHueNamesListToolStripMenuItem";
            ExportAllHueNamesListToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            ExportAllHueNamesListToolStripMenuItem.Text = "Export all hue names list";
            ExportAllHueNamesListToolStripMenuItem.ToolTipText = "Exporting the entire list of Hue names into a list.";
            ExportAllHueNamesListToolStripMenuItem.Click += ExportAllHueNamesListToolStripMenuItem_Click;
            // 
            // toolStripContainer
            // 
            toolStripContainer.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer.ContentPanel
            // 
            toolStripContainer.ContentPanel.AutoScroll = true;
            toolStripContainer.ContentPanel.Controls.Add(vScrollBar);
            toolStripContainer.ContentPanel.Controls.Add(pictureBox);
            toolStripContainer.ContentPanel.Controls.Add(this.splitterSide);
            toolStripContainer.ContentPanel.Controls.Add(this.sidePanel);
            toolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            toolStripContainer.ContentPanel.Size = new System.Drawing.Size(740, 359);
            toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            toolStripContainer.LeftToolStripPanelVisible = false;
            toolStripContainer.Location = new System.Drawing.Point(1, 1);
            toolStripContainer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            toolStripContainer.Name = "toolStripContainer";
            toolStripContainer.RightToolStripPanelVisible = false;
            toolStripContainer.Size = new System.Drawing.Size(740, 390);
            toolStripContainer.TabIndex = 9;
            toolStripContainer.Text = "toolStripContainer";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            toolStripContainer.TopToolStripPanel.Controls.Add(HuesTopMenuToolStrip);
            // 
            // vScrollBar
            // 
            vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            vScrollBar.Location = new System.Drawing.Point(723, 0);
            vScrollBar.Name = "vScrollBar";
            vScrollBar.Size = new System.Drawing.Size(17, 359);
            vScrollBar.TabIndex = 4;
            vScrollBar.Scroll += OnScroll;
            // 
            // pictureBox
            // 
            pictureBox.ContextMenuStrip = contextMenuStrip1;
            pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox.Location = new System.Drawing.Point(0, 0);
            pictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(740, 359);
            pictureBox.TabIndex = 3;
            pictureBox.TabStop = false;
            pictureBox.SizeChanged += OnResize;
            pictureBox.Paint += OnPaint;
            pictureBox.MouseClick += OnMouseClick;
            pictureBox.MouseDoubleClick += OnMouseDoubleClick;
            // 
            // HuesTopMenuToolStrip
            // 
            HuesTopMenuToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            HuesTopMenuToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            HuesTopMenuToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { HueIndexToolStripLabel, HueIndexToolStripTextBox, HueNameToolStripLabel, HueNameToolStripTextBox, SearchNameToolStripButton, this.toolStripSeparatorFilter, this.filterLabel, this._filterBox, this._gridButton, this._countLabel, this.toolStripSeparatorUndo, this._undoButton, this._redoButton });
            HuesTopMenuToolStrip.Location = new System.Drawing.Point(0, 0);
            HuesTopMenuToolStrip.Name = "HuesTopMenuToolStrip";
            HuesTopMenuToolStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            HuesTopMenuToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            HuesTopMenuToolStrip.Size = new System.Drawing.Size(740, 31);
            HuesTopMenuToolStrip.Stretch = true;
            HuesTopMenuToolStrip.TabIndex = 0;
            // 
            // HueIndexToolStripLabel
            // 
            HueIndexToolStripLabel.Name = "HueIndexToolStripLabel";
            HueIndexToolStripLabel.Size = new System.Drawing.Size(64, 20);
            HueIndexToolStripLabel.Text = "Hue:";
            // 
            // HueIndexToolStripTextBox
            // 
            HueIndexToolStripTextBox.Name = "HueIndexToolStripTextBox";
            HueIndexToolStripTextBox.Size = new System.Drawing.Size(100, 23);
            HueIndexToolStripTextBox.ToolTipText = "Hue number as used in the game/server (decimal or 0x..)";
            HueIndexToolStripTextBox.KeyUp += HueIndexToolStripTextBox_KeyUp;
            // 
            // HueNameToolStripLabel
            // 
            HueNameToolStripLabel.Name = "HueNameToolStripLabel";
            HueNameToolStripLabel.Size = new System.Drawing.Size(67, 20);
            HueNameToolStripLabel.Text = "Hue Name:";
            // 
            // HueNameToolStripTextBox
            // 
            HueNameToolStripTextBox.Name = "HueNameToolStripTextBox";
            HueNameToolStripTextBox.Size = new System.Drawing.Size(100, 23);
            HueNameToolStripTextBox.KeyUp += HueNameToolStripTextBox_KeyUp;
            // 
            // SearchNameToolStripButton
            // 
            SearchNameToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            SearchNameToolStripButton.Image = (System.Drawing.Image)resources.GetObject("SearchNameToolStripButton.Image");
            SearchNameToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            SearchNameToolStripButton.Name = "SearchNameToolStripButton";
            SearchNameToolStripButton.Size = new System.Drawing.Size(60, 20);
            SearchNameToolStripButton.Text = "Find next";
            SearchNameToolStripButton.Click += SearchNameToolStripButton_Click;
            // 
            // HuesControl
            // 
            // 
            // toolStripSeparatorFilter
            // 
            this.toolStripSeparatorFilter.Name = "toolStripSeparatorFilter";
            // 
            // filterLabel
            // 
            this.filterLabel.Name = "filterLabel";
            this.filterLabel.Text = "Filter:";
            // 
            // _filterBox
            // 
            this._filterBox.Items.AddRange(new object[] {
            "All",
            "Named",
            "Unnamed",
            "Empty (black)",
            "Not empty",
            "Changed",
            "Selected",
            "Red",
            "Orange",
            "Yellow",
            "Green",
            "Cyan",
            "Blue",
            "Purple",
            "Pink",
            "Gray / white / black",
            "Used in the client files",
            "Not used (free)"});
            this._filterBox.AutoSize = false;
            this._filterBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._filterBox.Name = "_filterBox";
            this._filterBox.Size = new System.Drawing.Size(150, 23);
            this._filterBox.ToolTipText = "Show only some hues";
            this._filterBox.SelectedIndexChanged += new System.EventHandler(this.OnFilterChanged);
            // 
            // _gridButton
            // 
            this._gridButton.CheckOnClick = true;
            this._gridButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._gridButton.Name = "_gridButton";
            this._gridButton.Text = "Grid";
            this._gridButton.ToolTipText = "Grid view (all hues as small tiles) / list view";
            this._gridButton.CheckedChanged += new System.EventHandler(this.OnGridChanged);
            // 
            // _countLabel
            // 
            this._countLabel.ForeColor = System.Drawing.Color.DimGray;
            this._countLabel.Name = "_countLabel";
            // 
            // toolStripSeparatorUndo
            // 
            this.toolStripSeparatorUndo.Name = "toolStripSeparatorUndo";
            // 
            // _undoButton
            // 
            this._undoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._undoButton.Enabled = false;
            this._undoButton.Name = "_undoButton";
            this._undoButton.Text = "↶ Undo";
            this._undoButton.Click += new System.EventHandler(this.OnClickUndo);
            // 
            // _redoButton
            // 
            this._redoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._redoButton.Enabled = false;
            this._redoButton.Name = "_redoButton";
            this._redoButton.Text = "↷ Redo";
            this._redoButton.Click += new System.EventHandler(this.OnClickRedo);
            // 
            // editMenuItem
            // 
            this.editMenuItem.Name = "editMenuItem";
            this.editMenuItem.Text = "Edit… (Enter)";
            this.editMenuItem.Click += new System.EventHandler(this.OnClickEdit);
            // 
            // renameMenuItem
            // 
            this.renameMenuItem.Name = "renameMenuItem";
            this.renameMenuItem.Text = "Rename… (F2)";
            this.renameMenuItem.Click += new System.EventHandler(this.OnClickRename);
            // 
            // editSeparator
            // 
            this.editSeparator.Name = "editSeparator";
            // 
            // extSeparator1
            // 
            this.extSeparator1.Name = "extSeparator1";
            // 
            // copyMenuItem
            // 
            this.copyMenuItem.Name = "copyMenuItem";
            this.copyMenuItem.Text = "Copy (Ctrl+C)";
            this.copyMenuItem.Click += new System.EventHandler(this.OnClickCopy);
            // 
            // pasteMenuItem
            // 
            this.pasteMenuItem.Name = "pasteMenuItem";
            this.pasteMenuItem.Text = "Paste (Ctrl+V)";
            this.pasteMenuItem.Click += new System.EventHandler(this.OnClickPaste);
            // 
            // clearMenuItem
            // 
            this.clearMenuItem.Name = "clearMenuItem";
            this.clearMenuItem.Text = "Clear / set empty (Del)";
            this.clearMenuItem.Click += new System.EventHandler(this.OnClickClear);
            // 
            // copyToMenuItem
            // 
            this.copyToMenuItem.Name = "copyToMenuItem";
            this.copyToMenuItem.Text = "Copy to hue…";
            this.copyToMenuItem.Click += new System.EventHandler(this.OnClickCopyTo);
            // 
            // swapMenuItem
            // 
            this.swapMenuItem.Name = "swapMenuItem";
            this.swapMenuItem.Text = "Swap with hue…";
            this.swapMenuItem.Click += new System.EventHandler(this.OnClickSwap);
            // 
            // revertMenuItem
            // 
            this.revertMenuItem.Name = "revertMenuItem";
            this.revertMenuItem.Text = "Revert to loaded state";
            this.revertMenuItem.Click += new System.EventHandler(this.OnClickRevert);
            // 
            // extSeparator2
            // 
            this.extSeparator2.Name = "extSeparator2";
            // 
            // gradientMenuItem
            // 
            this.gradientMenuItem.Name = "gradientMenuItem";
            this.gradientMenuItem.Text = "New from gradient…";
            this.gradientMenuItem.Click += new System.EventHandler(this.OnClickGradient);
            // 
            // fromImageMenuItem
            // 
            this.fromImageMenuItem.Name = "fromImageMenuItem";
            this.fromImageMenuItem.Text = "New from image…";
            this.fromImageMenuItem.Click += new System.EventHandler(this.OnClickFromImage);
            // 
            // extSeparator3
            // 
            this.extSeparator3.Name = "extSeparator3";
            // 
            // similarMenuItem
            // 
            this.similarMenuItem.Name = "similarMenuItem";
            this.similarMenuItem.Text = "Find similar hues";
            this.similarMenuItem.Click += new System.EventHandler(this.OnClickSimilar);
            // 
            // servUOMenuItem
            // 
            this.servUOMenuItem.Name = "servUOMenuItem";
            this.servUOMenuItem.Text = "Copy for ServUO (Hue = 0x…)";
            this.servUOMenuItem.Click += new System.EventHandler(this.OnClickServUO);
            // 
            // extSeparator4
            // 
            this.extSeparator4.Name = "extSeparator4";
            // 
            // exportRangeMenuItem
            // 
            this.exportRangeMenuItem.Name = "exportRangeMenuItem";
            this.exportRangeMenuItem.Text = "Export selection (hue range)…";
            this.exportRangeMenuItem.Click += new System.EventHandler(this.OnClickExportRange);
            // 
            // importRangeMenuItem
            // 
            this.importRangeMenuItem.Name = "importRangeMenuItem";
            this.importRangeMenuItem.Text = "Import hue range…";
            this.importRangeMenuItem.Click += new System.EventHandler(this.OnClickImportRange);
            // 
            // paletteMenuItem
            // 
            this.paletteMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.palettePngMenuItem,
            this.paletteGplMenuItem,
            this.paletteActMenuItem});
            this.paletteMenuItem.Name = "paletteMenuItem";
            this.paletteMenuItem.Text = "Export palette of the selection";
            // 
            // palettePngMenuItem
            // 
            this.palettePngMenuItem.Name = "palettePngMenuItem";
            this.palettePngMenuItem.Text = "PNG image…";
            this.palettePngMenuItem.Click += new System.EventHandler(this.OnClickPalettePng);
            // 
            // paletteGplMenuItem
            // 
            this.paletteGplMenuItem.Name = "paletteGplMenuItem";
            this.paletteGplMenuItem.Text = "GIMP palette (.gpl)…";
            this.paletteGplMenuItem.Click += new System.EventHandler(this.OnClickPaletteGpl);
            // 
            // paletteActMenuItem
            // 
            this.paletteActMenuItem.Name = "paletteActMenuItem";
            this.paletteActMenuItem.Text = "Adobe colour table (.act, max. 8 hues)…";
            this.paletteActMenuItem.Click += new System.EventHandler(this.OnClickPaletteAct);
            // 
            // extSeparator5
            // 
            this.extSeparator5.Name = "extSeparator5";
            // 
            // undoMenuItem
            // 
            this.undoMenuItem.Name = "undoMenuItem";
            this.undoMenuItem.Text = "Undo (Ctrl+Z)";
            this.undoMenuItem.Click += new System.EventHandler(this.OnClickUndo);
            // 
            // redoMenuItem
            // 
            this.redoMenuItem.Name = "redoMenuItem";
            this.redoMenuItem.Text = "Redo (Ctrl+Y)";
            this.redoMenuItem.Click += new System.EventHandler(this.OnClickRedo);
            // 
            // splitterSide
            // 
            this.splitterSide.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitterSide.MinExtra = 250;
            this.splitterSide.MinSize = 220;
            this.splitterSide.Name = "splitterSide";
            this.splitterSide.Size = new System.Drawing.Size(4, 359);
            this.splitterSide.TabIndex = 0;
            // 
            // sidePanel
            // 
            this.sidePanel.Controls.Add(this.sideTabs);
            this.sidePanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.sidePanel.Name = "sidePanel";
            this.sidePanel.Size = new System.Drawing.Size(340, 359);
            this.sidePanel.TabIndex = 1;
            // 
            // sideTabs
            // 
            this.sideTabs.Controls.Add(this.infoPage);
            this.sideTabs.Controls.Add(this.usagePage);
            this.sideTabs.Controls.Add(this.findPage);
            this.sideTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sideTabs.Name = "sideTabs";
            this.sideTabs.SelectedIndex = 0;
            this.sideTabs.TabIndex = 2;
            // 
            // infoPage
            // 
            this.infoPage.Controls.Add(this._preview);
            this.infoPage.Controls.Add(this.infoButtons);
            this.infoPage.Controls.Add(this._info);
            this.infoPage.Controls.Add(this._strip);
            this.infoPage.Name = "infoPage";
            this.infoPage.TabIndex = 3;
            this.infoPage.Text = "Info / preview";
            this.infoPage.UseVisualStyleBackColor = true;
            // 
            // _preview
            // 
            this._preview.Dock = System.Windows.Forms.DockStyle.Fill;
            this._preview.Name = "_preview";
            this._preview.TabIndex = 4;
            // 
            // infoButtons
            // 
            this.infoButtons.Controls.Add(this.btnEdit);
            this.infoButtons.Controls.Add(this.btnServUO);
            this.infoButtons.Controls.Add(this.btnRevert);
            this.infoButtons.Controls.Add(this.btnGradient);
            this.infoButtons.Controls.Add(this.btnFromImage);
            this.infoButtons.AutoSize = true;
            this.infoButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.infoButtons.Name = "infoButtons";
            this.infoButtons.Padding = new System.Windows.Forms.Padding(2);
            this.infoButtons.TabIndex = 5;
            // 
            // btnEdit
            // 
            this.btnEdit.AutoSize = true;
            this.btnEdit.Margin = new System.Windows.Forms.Padding(2);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.TabIndex = 6;
            this.btnEdit.Text = "Edit…";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.OnClickEdit);
            // 
            // btnServUO
            // 
            this.btnServUO.AutoSize = true;
            this.btnServUO.Margin = new System.Windows.Forms.Padding(2);
            this.btnServUO.Name = "btnServUO";
            this.btnServUO.TabIndex = 7;
            this.btnServUO.Text = "Copy for ServUO";
            this.btnServUO.UseVisualStyleBackColor = true;
            this.btnServUO.Click += new System.EventHandler(this.OnClickServUO);
            // 
            // btnRevert
            // 
            this.btnRevert.AutoSize = true;
            this.btnRevert.Margin = new System.Windows.Forms.Padding(2);
            this.btnRevert.Name = "btnRevert";
            this.btnRevert.TabIndex = 8;
            this.btnRevert.Text = "Revert";
            this.btnRevert.UseVisualStyleBackColor = true;
            this.btnRevert.Click += new System.EventHandler(this.OnClickRevert);
            // 
            // btnGradient
            // 
            this.btnGradient.AutoSize = true;
            this.btnGradient.Margin = new System.Windows.Forms.Padding(2);
            this.btnGradient.Name = "btnGradient";
            this.btnGradient.TabIndex = 9;
            this.btnGradient.Text = "Gradient…";
            this.btnGradient.UseVisualStyleBackColor = true;
            this.btnGradient.Click += new System.EventHandler(this.OnClickGradient);
            // 
            // btnFromImage
            // 
            this.btnFromImage.AutoSize = true;
            this.btnFromImage.Margin = new System.Windows.Forms.Padding(2);
            this.btnFromImage.Name = "btnFromImage";
            this.btnFromImage.TabIndex = 10;
            this.btnFromImage.Text = "From image…";
            this.btnFromImage.UseVisualStyleBackColor = true;
            this.btnFromImage.Click += new System.EventHandler(this.OnClickFromImage);
            // 
            // _info
            // 
            this._info.Dock = System.Windows.Forms.DockStyle.Top;
            this._info.Font = new System.Drawing.Font("Consolas", 8.5F);
            this._info.Name = "_info";
            this._info.Padding = new System.Windows.Forms.Padding(4);
            this._info.Size = new System.Drawing.Size(332, 146);
            // 
            // _strip
            // 
            this._toolTip.SetToolTip(this._strip, "Click a colour step to see its value");
            this._strip.Cursor = System.Windows.Forms.Cursors.Hand;
            this._strip.Dock = System.Windows.Forms.DockStyle.Top;
            this._strip.Name = "_strip";
            this._strip.Size = new System.Drawing.Size(332, 38);
            this._strip.TabIndex = 11;
            this._strip.TabStop = false;
            this._strip.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintStrip);
            this._strip.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnStripClick);
            // 
            // usagePage
            // 
            this.usagePage.Controls.Add(this._usage);
            this.usagePage.Controls.Add(this._usageTitle);
            this.usagePage.Controls.Add(this.usageTop);
            this.usagePage.Controls.Add(this.usageHint);
            this.usagePage.Name = "usagePage";
            this.usagePage.TabIndex = 12;
            this.usagePage.Text = "Used by";
            this.usagePage.UseVisualStyleBackColor = true;
            // 
            // _usage
            // 
            this._usage.Dock = System.Windows.Forms.DockStyle.Fill;
            this._usage.FormattingEnabled = true;
            this._usage.HorizontalScrollbar = true;
            this._usage.IntegralHeight = false;
            this._usage.Name = "_usage";
            this._usage.TabIndex = 13;
            this._usage.SelectedIndexChanged += new System.EventHandler(this.OnUsageSelected);
            // 
            // _usageTitle
            // 
            this._usageTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this._usageTitle.Name = "_usageTitle";
            this._usageTitle.Padding = new System.Windows.Forms.Padding(4, 3, 0, 0);
            this._usageTitle.Size = new System.Drawing.Size(332, 34);
            // 
            // usageTop
            // 
            this.usageTop.Controls.Add(this._chkFollowUsage);
            this.usageTop.Controls.Add(this.btnScanAgain);
            this.usageTop.AutoSize = true;
            this.usageTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.usageTop.Name = "usageTop";
            this.usageTop.Padding = new System.Windows.Forms.Padding(2);
            this.usageTop.TabIndex = 14;
            // 
            // _chkFollowUsage
            // 
            this._toolTip.SetToolTip(this._chkFollowUsage, "When a hue is selected, the preview shows the graphic that uses it (the most used one). Without a usage your own test graphic is shown.");
            this._chkFollowUsage.AutoSize = true;
            this._chkFollowUsage.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkFollowUsage.Checked = true;
            this._chkFollowUsage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFollowUsage.Name = "_chkFollowUsage";
            this._chkFollowUsage.TabIndex = 15;
            this._chkFollowUsage.Text = "Preview follows the usage";
            this._chkFollowUsage.UseVisualStyleBackColor = true;
            this._chkFollowUsage.CheckedChanged += new System.EventHandler(this.OnFollowUsageChanged);
            // 
            // btnScanAgain
            // 
            this.btnScanAgain.AutoSize = true;
            this.btnScanAgain.Margin = new System.Windows.Forms.Padding(2);
            this.btnScanAgain.Name = "btnScanAgain";
            this.btnScanAgain.TabIndex = 16;
            this.btnScanAgain.Text = "Scan again";
            this.btnScanAgain.UseVisualStyleBackColor = true;
            this.btnScanAgain.Click += new System.EventHandler(this.OnClickScanAgain);
            // 
            // usageHint
            // 
            this.usageHint.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.usageHint.ForeColor = System.Drawing.Color.DimGray;
            this.usageHint.Name = "usageHint";
            this.usageHint.Padding = new System.Windows.Forms.Padding(4, 3, 0, 0);
            this.usageHint.Size = new System.Drawing.Size(332, 46);
            this.usageHint.Text = "Read from statics*.mul (maps), body.def, corpse.def, gump.def and equipconv.def. Land tiles and textures are never hued by the client.";
            // 
            // findPage
            // 
            this.findPage.Controls.Add(this._results);
            this.findPage.Controls.Add(this._resultsTitle);
            this.findPage.Controls.Add(this.findTop);
            this.findPage.Controls.Add(this.findHint);
            this.findPage.Name = "findPage";
            this.findPage.TabIndex = 17;
            this.findPage.Text = "Find";
            this.findPage.UseVisualStyleBackColor = true;
            // 
            // _results
            // 
            this._results.Dock = System.Windows.Forms.DockStyle.Fill;
            this._results.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this._results.FormattingEnabled = true;
            this._results.IntegralHeight = false;
            this._results.ItemHeight = 34;
            this._results.Name = "_results";
            this._results.TabIndex = 18;
            this._results.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.OnDrawResult);
            this._results.DoubleClick += new System.EventHandler(this.OnResultDoubleClick);
            this._results.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnResultKeyDown);
            // 
            // _resultsTitle
            // 
            this._resultsTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this._resultsTitle.ForeColor = System.Drawing.Color.DimGray;
            this._resultsTitle.Name = "_resultsTitle";
            this._resultsTitle.Padding = new System.Windows.Forms.Padding(4, 3, 0, 0);
            this._resultsTitle.Size = new System.Drawing.Size(332, 20);
            // 
            // findTop
            // 
            this.findTop.Controls.Add(this.btnSimilar);
            this.findTop.Controls.Add(this.colourLabel);
            this.findTop.Controls.Add(this._colorText);
            this.findTop.Controls.Add(this.btnPickColor);
            this.findTop.Controls.Add(this.btnFindColor);
            this.findTop.AutoSize = true;
            this.findTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.findTop.Name = "findTop";
            this.findTop.Padding = new System.Windows.Forms.Padding(2);
            this.findTop.TabIndex = 19;
            // 
            // btnSimilar
            // 
            this.btnSimilar.AutoSize = true;
            this.btnSimilar.Margin = new System.Windows.Forms.Padding(2);
            this.btnSimilar.Name = "btnSimilar";
            this.btnSimilar.TabIndex = 20;
            this.btnSimilar.Text = "Similar to selected";
            this.btnSimilar.UseVisualStyleBackColor = true;
            this.btnSimilar.Click += new System.EventHandler(this.OnClickSimilar);
            // 
            // colourLabel
            // 
            this.colourLabel.AutoSize = true;
            this.colourLabel.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            this.colourLabel.Name = "colourLabel";
            this.colourLabel.Text = "Colour:";
            // 
            // _colorText
            // 
            this._toolTip.SetToolTip(this._colorText, "#RRGGBB or R,G,B");
            this._colorText.Name = "_colorText";
            this._colorText.Size = new System.Drawing.Size(80, 23);
            this._colorText.TabIndex = 21;
            this._colorText.Text = "#A03030";
            this._colorText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnColorTextKeyDown);
            // 
            // btnPickColor
            // 
            this.btnPickColor.AutoSize = true;
            this.btnPickColor.Margin = new System.Windows.Forms.Padding(2);
            this.btnPickColor.Name = "btnPickColor";
            this.btnPickColor.TabIndex = 22;
            this.btnPickColor.Text = "…";
            this.btnPickColor.UseVisualStyleBackColor = true;
            this.btnPickColor.Click += new System.EventHandler(this.OnClickPickColor);
            // 
            // btnFindColor
            // 
            this.btnFindColor.AutoSize = true;
            this.btnFindColor.Margin = new System.Windows.Forms.Padding(2);
            this.btnFindColor.Name = "btnFindColor";
            this.btnFindColor.TabIndex = 23;
            this.btnFindColor.Text = "Find hues with this colour";
            this.btnFindColor.UseVisualStyleBackColor = true;
            this.btnFindColor.Click += new System.EventHandler(this.OnClickFindColor);
            // 
            // findHint
            // 
            this.findHint.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.findHint.ForeColor = System.Drawing.Color.DimGray;
            this.findHint.Name = "findHint";
            this.findHint.Padding = new System.Windows.Forms.Padding(4, 3, 0, 0);
            this.findHint.Size = new System.Drawing.Size(332, 20);
            this.findHint.Text = "Double click a result to select it.";
            // 
            // _toolTip
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(toolStripContainer);
            DoubleBuffered = true;
            ForeColor = System.Drawing.SystemColors.ControlText;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "HuesControl";
            Padding = new System.Windows.Forms.Padding(1);
            Size = new System.Drawing.Size(742, 392);
            Load += OnLoad;
            contextMenuStrip1.ResumeLayout(false);
            this.sidePanel.ResumeLayout(false);
            this.sidePanel.PerformLayout();
            this.sideTabs.ResumeLayout(false);
            this.infoPage.ResumeLayout(false);
            this.infoPage.PerformLayout();
            this.infoButtons.ResumeLayout(false);
            this.infoButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._strip)).EndInit();
            this.usagePage.ResumeLayout(false);
            this.usagePage.PerformLayout();
            this.usageTop.ResumeLayout(false);
            this.usageTop.PerformLayout();
            this.findPage.ResumeLayout(false);
            this.findPage.PerformLayout();
            this.findTop.ResumeLayout(false);
            this.findTop.PerformLayout();
            toolStripContainer.ContentPanel.ResumeLayout(false);
            toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer.TopToolStripPanel.PerformLayout();
            toolStripContainer.ResumeLayout(false);
            toolStripContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            HuesTopMenuToolStrip.ResumeLayout(false);
            HuesTopMenuToolStrip.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox ReplaceText;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripContainer toolStripContainer;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ToolStrip HuesTopMenuToolStrip;
        private System.Windows.Forms.ToolStripLabel HueIndexToolStripLabel;
        private System.Windows.Forms.ToolStripTextBox HueIndexToolStripTextBox;
        private System.Windows.Forms.ToolStripLabel HueNameToolStripLabel;
        private System.Windows.Forms.ToolStripTextBox HueNameToolStripTextBox;
        private System.Windows.Forms.ToolStripButton SearchNameToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem ExportAllHueNamesListToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorFilter;
        private System.Windows.Forms.ToolStripLabel filterLabel;
        private System.Windows.Forms.ToolStripComboBox _filterBox;
        private System.Windows.Forms.ToolStripButton _gridButton;
        private System.Windows.Forms.ToolStripLabel _countLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorUndo;
        private System.Windows.Forms.ToolStripButton _undoButton;
        private System.Windows.Forms.ToolStripButton _redoButton;
        private System.Windows.Forms.ToolStripMenuItem editMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameMenuItem;
        private System.Windows.Forms.ToolStripSeparator editSeparator;
        private System.Windows.Forms.ToolStripSeparator extSeparator1;
        private System.Windows.Forms.ToolStripMenuItem copyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToMenuItem;
        private System.Windows.Forms.ToolStripMenuItem swapMenuItem;
        private System.Windows.Forms.ToolStripMenuItem revertMenuItem;
        private System.Windows.Forms.ToolStripSeparator extSeparator2;
        private System.Windows.Forms.ToolStripMenuItem gradientMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromImageMenuItem;
        private System.Windows.Forms.ToolStripSeparator extSeparator3;
        private System.Windows.Forms.ToolStripMenuItem similarMenuItem;
        private System.Windows.Forms.ToolStripMenuItem servUOMenuItem;
        private System.Windows.Forms.ToolStripSeparator extSeparator4;
        private System.Windows.Forms.ToolStripMenuItem exportRangeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importRangeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem paletteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem palettePngMenuItem;
        private System.Windows.Forms.ToolStripMenuItem paletteGplMenuItem;
        private System.Windows.Forms.ToolStripMenuItem paletteActMenuItem;
        private System.Windows.Forms.ToolStripSeparator extSeparator5;
        private System.Windows.Forms.ToolStripMenuItem undoMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoMenuItem;
        private System.Windows.Forms.Splitter splitterSide;
        private System.Windows.Forms.Panel sidePanel;
        private System.Windows.Forms.TabControl sideTabs;
        private System.Windows.Forms.TabPage infoPage;
        private UoFiddler.Controls.UserControls.HuePreviewPanel _preview;
        private System.Windows.Forms.FlowLayoutPanel infoButtons;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnServUO;
        private System.Windows.Forms.Button btnRevert;
        private System.Windows.Forms.Button btnGradient;
        private System.Windows.Forms.Button btnFromImage;
        private System.Windows.Forms.Label _info;
        private System.Windows.Forms.PictureBox _strip;
        private System.Windows.Forms.TabPage usagePage;
        private System.Windows.Forms.ListBox _usage;
        private System.Windows.Forms.Label _usageTitle;
        private System.Windows.Forms.FlowLayoutPanel usageTop;
        private System.Windows.Forms.CheckBox _chkFollowUsage;
        private System.Windows.Forms.Button btnScanAgain;
        private System.Windows.Forms.Label usageHint;
        private System.Windows.Forms.TabPage findPage;
        private System.Windows.Forms.ListBox _results;
        private System.Windows.Forms.Label _resultsTitle;
        private System.Windows.Forms.FlowLayoutPanel findTop;
        private System.Windows.Forms.Button btnSimilar;
        private System.Windows.Forms.Label colourLabel;
        private System.Windows.Forms.TextBox _colorText;
        private System.Windows.Forms.Button btnPickColor;
        private System.Windows.Forms.Button btnFindColor;
        private System.Windows.Forms.Label findHint;
        private System.Windows.Forms.ToolTip _toolTip;
    }
}
