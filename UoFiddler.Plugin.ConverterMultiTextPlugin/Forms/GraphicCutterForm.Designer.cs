﻿// /***************************************************************************
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
    partial class GraphicCutterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GraphicCutterForm));
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            moveTimer = new System.Windows.Forms.Timer(components);
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            openImageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importClipboardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openIntoResultMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileSep1 = new System.Windows.Forms.ToolStripSeparator();
            saveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openOutputFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileSep2 = new System.Windows.Forms.ToolStripSeparator();
            unloadStageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            unloadResultMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileSep3 = new System.Windows.Forms.ToolStripSeparator();
            closeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            imageMenu = new System.Windows.Forms.ToolStripMenuItem();
            mirrorStageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            transparentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            autoTrimMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bakeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            imageSep1 = new System.Windows.Forms.ToolStripSeparator();
            centerImageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewMenu = new System.Windows.Forms.ToolStripMenuItem();
            gridMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            gridColorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            gridResultMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            borderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cutFrameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            rulerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewSep1 = new System.Windows.Forms.ToolStripSeparator();
            backgroundMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bgBlackMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bgWhiteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bgColorMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bgHexTextBox = new System.Windows.Forms.ToolStripTextBox();
            bgSep = new System.Windows.Forms.ToolStripSeparator();
            bgImageNoneMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bgImageGreenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bgImageWaterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewSep2 = new System.Windows.Forms.ToolStripSeparator();
            topMostMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            resultMenu = new System.Windows.Forms.ToolStripMenuItem();
            mirrorResultMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            rotateResultMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            resultSep1 = new System.Windows.Forms.ToolStripSeparator();
            copyResultMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copySelectionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fillTextureMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cutStripsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            resultSep2 = new System.Windows.Forms.ToolStripSeparator();
            showMaskMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            clearSelectionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            resultSep3 = new System.Windows.Forms.ToolStripSeparator();
            soundMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsOpen = new System.Windows.Forms.ToolStripButton();
            tsImport = new System.Windows.Forms.ToolStripButton();
            tsSave = new System.Windows.Forms.ToolStripButton();
            tsFormat = new System.Windows.Forms.ToolStripComboBox();
            tsSep1 = new System.Windows.Forms.ToolStripSeparator();
            tsCopy = new System.Windows.Forms.ToolStripButton();
            tsMirror = new System.Windows.Forms.ToolStripButton();
            tsRotate = new System.Windows.Forms.ToolStripButton();
            tsCutStrips = new System.Windows.Forms.ToolStripButton();
            tsFill = new System.Windows.Forms.ToolStripButton();
            tsSep2 = new System.Windows.Forms.ToolStripSeparator();
            tsGrid = new System.Windows.Forms.ToolStripButton();
            tsBorder = new System.Windows.Forms.ToolStripButton();
            tsRuler = new System.Windows.Forms.ToolStripButton();
            tsMask = new System.Windows.Forms.ToolStripButton();
            tsSep3 = new System.Windows.Forms.ToolStripSeparator();
            tsBgLabel = new System.Windows.Forms.ToolStripLabel();
            tsBackground = new System.Windows.Forms.ToolStripComboBox();
            tsBackColor = new System.Windows.Forms.ToolStripButton();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            statusPos = new System.Windows.Forms.ToolStripStatusLabel();
            statusInfo = new System.Windows.Forms.ToolStripStatusLabel();
            statusMessage = new System.Windows.Forms.ToolStripStatusLabel();
            mainLayout = new System.Windows.Forms.TableLayoutPanel();
            groupStage = new System.Windows.Forms.GroupBox();
            panelStage = new System.Windows.Forms.Panel();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            groupResult = new System.Windows.Forms.GroupBox();
            panelResult = new System.Windows.Forms.Panel();
            pictureBox2 = new System.Windows.Forms.PictureBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            ctxOpen = new System.Windows.Forms.ToolStripMenuItem();
            ctxSave = new System.Windows.Forms.ToolStripMenuItem();
            ctxSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            ctxUnload = new System.Windows.Forms.ToolStripMenuItem();
            ctxSep1 = new System.Windows.Forms.ToolStripSeparator();
            ctxGrid = new System.Windows.Forms.ToolStripMenuItem();
            ctxMirror = new System.Windows.Forms.ToolStripMenuItem();
            ctxRotate = new System.Windows.Forms.ToolStripMenuItem();
            ctxCutStrips = new System.Windows.Forms.ToolStripMenuItem();
            ctxFill = new System.Windows.Forms.ToolStripMenuItem();
            ctxMask = new System.Windows.Forms.ToolStripMenuItem();
            ctxSep2 = new System.Windows.Forms.ToolStripSeparator();
            ctxCopySelection = new System.Windows.Forms.ToolStripMenuItem();
            ctxCopyResult = new System.Windows.Forms.ToolStripMenuItem();
            flowControls = new System.Windows.Forms.FlowLayoutPanel();
            groupMoveImage = new System.Windows.Forms.GroupBox();
            btnUp = new System.Windows.Forms.Button();
            btnLeft = new System.Windows.Forms.Button();
            btnRight = new System.Windows.Forms.Button();
            btnDown = new System.Windows.Forms.Button();
            groupCut = new System.Windows.Forms.GroupBox();
            labelWidth = new System.Windows.Forms.Label();
            numericWidth = new System.Windows.Forms.NumericUpDown();
            labelHeight = new System.Windows.Forms.Label();
            numericHeight = new System.Windows.Forms.NumericUpDown();
            labelX = new System.Windows.Forms.Label();
            numericX = new System.Windows.Forms.NumericUpDown();
            labelY = new System.Windows.Forms.Label();
            numericY = new System.Windows.Forms.NumericUpDown();
            labelPreset = new System.Windows.Forms.Label();
            comboPreset = new System.Windows.Forms.ComboBox();
            checkBoxFollow = new System.Windows.Forms.CheckBox();
            groupMoveCut = new System.Windows.Forms.GroupBox();
            btnCutUp = new System.Windows.Forms.Button();
            btnCutLeft = new System.Windows.Forms.Button();
            btnCutRight = new System.Windows.Forms.Button();
            btnCutDown = new System.Windows.Forms.Button();
            buttonQuickSave = new System.Windows.Forms.Button();
            groupOptions = new System.Windows.Forms.GroupBox();
            checkBoxTransparent = new System.Windows.Forms.CheckBox();
            labelTolerance = new System.Windows.Forms.Label();
            numericTolerance = new System.Windows.Forms.NumericUpDown();
            labelSelection = new System.Windows.Forms.Label();
            radioRectangle = new System.Windows.Forms.RadioButton();
            radioCircle = new System.Windows.Forms.RadioButton();
            radioFreehand = new System.Windows.Forms.RadioButton();
            labelZoomTitle = new System.Windows.Forms.Label();
            zoomOutButton = new System.Windows.Forms.Button();
            labelZoom = new System.Windows.Forms.Label();
            zoomButton = new System.Windows.Forms.Button();
            resetButton = new System.Windows.Forms.Button();
            checkBoxBake = new System.Windows.Forms.CheckBox();
            saveArtMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editMenu = new System.Windows.Forms.ToolStripMenuItem();
            undoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            redoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pipetteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            clearPickedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            snapMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            sidePanelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bgLandMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addCollectionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            helpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpEnglishMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpGermanMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ctxAddCollection = new System.Windows.Forms.ToolStripMenuItem();
            editSep1 = new System.Windows.Forms.ToolStripSeparator();
            tsSepU = new System.Windows.Forms.ToolStripSeparator();
            tsSep4 = new System.Windows.Forms.ToolStripSeparator();
            tsUndo = new System.Windows.Forms.ToolStripButton();
            tsRedo = new System.Windows.Forms.ToolStripButton();
            tsAddCollection = new System.Windows.Forms.ToolStripButton();
            tsPipette = new System.Windows.Forms.ToolStripButton();
            tsPanel = new System.Windows.Forms.ToolStripButton();
            tsHelp = new System.Windows.Forms.ToolStripButton();
            tsFillLabel = new System.Windows.Forms.ToolStripLabel();
            tsFillMode = new System.Windows.Forms.ToolStripComboBox();
            checkBoxSnap = new System.Windows.Forms.CheckBox();
            labelPicked = new System.Windows.Forms.Label();
            buttonClearPicked = new System.Windows.Forms.Button();
            sidePanel = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter.CutterSidePanel();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            mainLayout.SuspendLayout();
            groupStage.SuspendLayout();
            panelStage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupResult.SuspendLayout();
            panelResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            contextMenuStrip1.SuspendLayout();
            flowControls.SuspendLayout();
            groupMoveImage.SuspendLayout();
            groupCut.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericY).BeginInit();
            groupMoveCut.SuspendLayout();
            groupOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericTolerance).BeginInit();
            SuspendLayout();
            //
            // toolTip1
            //
            toolTip1.AutoPopDelay = 15000;
            toolTip1.InitialDelay = 600;
            toolTip1.ReshowDelay = 200;
            //
            // moveTimer
            //
            moveTimer.Interval = 50;
            moveTimer.Tick += MoveTimer_Tick;
            //
            // menuStrip1
            //
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileMenu, editMenu, imageMenu, viewMenu, resultMenu, helpMenu });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(1360, 24);
            menuStrip1.TabIndex = 0;
            //
            // fileMenu
            //
            fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openImageMenuItem, importClipboardMenuItem, openIntoResultMenuItem, fileSep1, saveMenuItem, saveAsMenuItem, openOutputFolderMenuItem, saveArtMenuItem, fileSep2, unloadStageMenuItem, unloadResultMenuItem, fileSep3, closeMenuItem });
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new System.Drawing.Size(37, 20);
            fileMenu.Text = "&File";
            //
            // openImageMenuItem
            //
            openImageMenuItem.Image = Properties.Resources.Load;
            openImageMenuItem.Name = "openImageMenuItem";
            openImageMenuItem.ShortcutKeyDisplayString = "Ctrl+O";
            openImageMenuItem.Size = new System.Drawing.Size(250, 22);
            openImageMenuItem.Text = "Open image...";
            openImageMenuItem.Click += OpenImage_Click;
            //
            // importClipboardMenuItem
            //
            importClipboardMenuItem.Image = Properties.Resources.import;
            importClipboardMenuItem.Name = "importClipboardMenuItem";
            importClipboardMenuItem.ShortcutKeyDisplayString = "Ctrl+V";
            importClipboardMenuItem.Size = new System.Drawing.Size(250, 22);
            importClipboardMenuItem.Text = "Paste image from clipboard";
            importClipboardMenuItem.Click += ImportClipboard_Click;
            //
            // openIntoResultMenuItem
            //
            openIntoResultMenuItem.Name = "openIntoResultMenuItem";
            openIntoResultMenuItem.Size = new System.Drawing.Size(250, 22);
            openIntoResultMenuItem.Text = "Open image into result...";
            openIntoResultMenuItem.Click += OpenIntoResult_Click;
            //
            // fileSep1
            //
            fileSep1.Name = "fileSep1";
            fileSep1.Size = new System.Drawing.Size(247, 6);
            //
            // saveMenuItem
            //
            saveMenuItem.Image = Properties.Resources.Save;
            saveMenuItem.Name = "saveMenuItem";
            saveMenuItem.ShortcutKeyDisplayString = "Ctrl+S";
            saveMenuItem.Size = new System.Drawing.Size(250, 22);
            saveMenuItem.Text = "Save result (tempGrafic)";
            saveMenuItem.Click += Save_Click;
            //
            // saveAsMenuItem
            //
            saveAsMenuItem.Name = "saveAsMenuItem";
            saveAsMenuItem.ShortcutKeyDisplayString = "Ctrl+Shift+S";
            saveAsMenuItem.Size = new System.Drawing.Size(250, 22);
            saveAsMenuItem.Text = "Save result as...";
            saveAsMenuItem.Click += SaveAs_Click;
            //
            // openOutputFolderMenuItem
            //
            openOutputFolderMenuItem.Name = "openOutputFolderMenuItem";
            openOutputFolderMenuItem.Size = new System.Drawing.Size(250, 22);
            openOutputFolderMenuItem.Text = "Open tempGrafic folder";
            openOutputFolderMenuItem.Click += OpenOutputFolder_Click;
            //
            // fileSep2
            //
            fileSep2.Name = "fileSep2";
            fileSep2.Size = new System.Drawing.Size(247, 6);
            //
            // unloadStageMenuItem
            //
            unloadStageMenuItem.Image = Properties.Resources.Leeren;
            unloadStageMenuItem.Name = "unloadStageMenuItem";
            unloadStageMenuItem.Size = new System.Drawing.Size(250, 22);
            unloadStageMenuItem.Text = "Unload stage image";
            unloadStageMenuItem.Click += UnloadStage_Click;
            //
            // unloadResultMenuItem
            //
            unloadResultMenuItem.Name = "unloadResultMenuItem";
            unloadResultMenuItem.Size = new System.Drawing.Size(250, 22);
            unloadResultMenuItem.Text = "Unload result";
            unloadResultMenuItem.Click += UnloadResult_Click;
            //
            // fileSep3
            //
            fileSep3.Name = "fileSep3";
            fileSep3.Size = new System.Drawing.Size(247, 6);
            //
            // closeMenuItem
            //
            closeMenuItem.Name = "closeMenuItem";
            closeMenuItem.Size = new System.Drawing.Size(250, 22);
            closeMenuItem.Text = "Close";
            closeMenuItem.Click += Close_Click;
            //
            // imageMenu
            //
            imageMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mirrorStageMenuItem, transparentMenuItem, autoTrimMenuItem, bakeMenuItem, imageSep1, centerImageMenuItem });
            imageMenu.Name = "imageMenu";
            imageMenu.Size = new System.Drawing.Size(52, 20);
            imageMenu.Text = "&Image";
            //
            // mirrorStageMenuItem
            //
            mirrorStageMenuItem.Image = Properties.Resources.Mirror1;
            mirrorStageMenuItem.Name = "mirrorStageMenuItem";
            mirrorStageMenuItem.Size = new System.Drawing.Size(280, 22);
            mirrorStageMenuItem.Text = "Mirror image";
            mirrorStageMenuItem.Click += MirrorStage_Click;
            //
            // transparentMenuItem
            //
            transparentMenuItem.Name = "transparentMenuItem";
            transparentMenuItem.Size = new System.Drawing.Size(280, 22);
            transparentMenuItem.Text = "Black / white transparent";
            transparentMenuItem.Click += Transparent_Click;
            //
            // autoTrimMenuItem
            //
            autoTrimMenuItem.Image = Properties.Resources.Border;
            autoTrimMenuItem.Name = "autoTrimMenuItem";
            autoTrimMenuItem.Size = new System.Drawing.Size(280, 22);
            autoTrimMenuItem.Text = "Remove transparent border (auto trim)";
            autoTrimMenuItem.Click += AutoTrim_Click;
            //
            // bakeMenuItem
            //
            bakeMenuItem.Image = Properties.Resources.createDesign;
            bakeMenuItem.Name = "bakeMenuItem";
            bakeMenuItem.Size = new System.Drawing.Size(280, 22);
            bakeMenuItem.Text = "Cut with grid and background";
            bakeMenuItem.Click += Bake_Click;
            //
            // imageSep1
            //
            imageSep1.Name = "imageSep1";
            imageSep1.Size = new System.Drawing.Size(277, 6);
            //
            // centerImageMenuItem
            //
            centerImageMenuItem.Name = "centerImageMenuItem";
            centerImageMenuItem.Size = new System.Drawing.Size(280, 22);
            centerImageMenuItem.Text = "Reset position (bottom center)";
            centerImageMenuItem.Click += CenterImage_Click;
            //
            // viewMenu
            //
            viewMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { gridMenuItem, gridColorMenuItem, gridResultMenuItem, borderMenuItem, cutFrameMenuItem, snapMenuItem, rulerMenuItem, sidePanelMenuItem, viewSep1, backgroundMenuItem, viewSep2, topMostMenuItem });
            viewMenu.Name = "viewMenu";
            viewMenu.Size = new System.Drawing.Size(44, 20);
            viewMenu.Text = "&View";
            //
            // gridMenuItem
            //
            gridMenuItem.Image = Properties.Resources.Schablone;
            gridMenuItem.Name = "gridMenuItem";
            gridMenuItem.ShortcutKeyDisplayString = "Ctrl+G";
            gridMenuItem.Size = new System.Drawing.Size(230, 22);
            gridMenuItem.Text = "Grid on stage";
            gridMenuItem.Click += Grid_Click;
            //
            // gridColorMenuItem
            //
            gridColorMenuItem.Image = Properties.Resources.Color;
            gridColorMenuItem.Name = "gridColorMenuItem";
            gridColorMenuItem.Size = new System.Drawing.Size(230, 22);
            gridColorMenuItem.Text = "Grid color...";
            gridColorMenuItem.Click += GridColor_Click;
            //
            // gridResultMenuItem
            //
            gridResultMenuItem.Name = "gridResultMenuItem";
            gridResultMenuItem.Size = new System.Drawing.Size(230, 22);
            gridResultMenuItem.Text = "Grid on result";
            gridResultMenuItem.Click += GridResult_Click;
            //
            // borderMenuItem
            //
            borderMenuItem.Image = Properties.Resources.Rahmen;
            borderMenuItem.Name = "borderMenuItem";
            borderMenuItem.Size = new System.Drawing.Size(230, 22);
            borderMenuItem.Text = "Image border";
            borderMenuItem.Click += Border_Click;
            //
            // cutFrameMenuItem
            //
            cutFrameMenuItem.Name = "cutFrameMenuItem";
            cutFrameMenuItem.Size = new System.Drawing.Size(230, 22);
            cutFrameMenuItem.Text = "Cut area frame";
            cutFrameMenuItem.Click += CutFrame_Click;
            //
            // rulerMenuItem
            //
            rulerMenuItem.Image = Properties.Resources.ruler;
            rulerMenuItem.Name = "rulerMenuItem";
            rulerMenuItem.Size = new System.Drawing.Size(230, 22);
            rulerMenuItem.Text = "Ruler";
            rulerMenuItem.Click += Ruler_Click;
            //
            // viewSep1
            //
            viewSep1.Name = "viewSep1";
            viewSep1.Size = new System.Drawing.Size(227, 6);
            //
            // backgroundMenuItem
            //
            backgroundMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { bgBlackMenuItem, bgWhiteMenuItem, bgColorMenuItem, bgHexTextBox, bgSep, bgImageNoneMenuItem, bgImageGreenMenuItem, bgImageWaterMenuItem, bgLandMenuItem });
            backgroundMenuItem.Image = Properties.Resources.Color;
            backgroundMenuItem.Name = "backgroundMenuItem";
            backgroundMenuItem.Size = new System.Drawing.Size(230, 22);
            backgroundMenuItem.Text = "Stage background";
            //
            // bgBlackMenuItem
            //
            bgBlackMenuItem.Name = "bgBlackMenuItem";
            bgBlackMenuItem.Size = new System.Drawing.Size(200, 22);
            bgBlackMenuItem.Text = "Black";
            bgBlackMenuItem.Click += BgBlack_Click;
            //
            // bgWhiteMenuItem
            //
            bgWhiteMenuItem.Name = "bgWhiteMenuItem";
            bgWhiteMenuItem.Size = new System.Drawing.Size(200, 22);
            bgWhiteMenuItem.Text = "White";
            bgWhiteMenuItem.Click += BgWhite_Click;
            //
            // bgColorMenuItem
            //
            bgColorMenuItem.Name = "bgColorMenuItem";
            bgColorMenuItem.Size = new System.Drawing.Size(200, 22);
            bgColorMenuItem.Text = "Choose color...";
            bgColorMenuItem.Click += BgColor_Click;
            //
            // bgHexTextBox
            //
            bgHexTextBox.MaxLength = 7;
            bgHexTextBox.Name = "bgHexTextBox";
            bgHexTextBox.Size = new System.Drawing.Size(100, 23);
            bgHexTextBox.KeyDown += BgHex_KeyDown;
            //
            // bgSep
            //
            bgSep.Name = "bgSep";
            bgSep.Size = new System.Drawing.Size(197, 6);
            //
            // bgImageNoneMenuItem
            //
            bgImageNoneMenuItem.Name = "bgImageNoneMenuItem";
            bgImageNoneMenuItem.Size = new System.Drawing.Size(200, 22);
            bgImageNoneMenuItem.Text = "No background image";
            bgImageNoneMenuItem.Click += BgImageNone_Click;
            //
            // bgImageGreenMenuItem
            //
            bgImageGreenMenuItem.Name = "bgImageGreenMenuItem";
            bgImageGreenMenuItem.Size = new System.Drawing.Size(200, 22);
            bgImageGreenMenuItem.Text = "Grass";
            bgImageGreenMenuItem.Click += BgImageGreen_Click;
            //
            // bgImageWaterMenuItem
            //
            bgImageWaterMenuItem.Name = "bgImageWaterMenuItem";
            bgImageWaterMenuItem.Size = new System.Drawing.Size(200, 22);
            bgImageWaterMenuItem.Text = "Water";
            bgImageWaterMenuItem.Click += BgImageWater_Click;
            //
            // viewSep2
            //
            viewSep2.Name = "viewSep2";
            viewSep2.Size = new System.Drawing.Size(227, 6);
            //
            // topMostMenuItem
            //
            topMostMenuItem.Name = "topMostMenuItem";
            topMostMenuItem.Size = new System.Drawing.Size(230, 22);
            topMostMenuItem.Text = "Always on top";
            topMostMenuItem.Click += TopMost_Click;
            //
            // resultMenu
            //
            resultMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mirrorResultMenuItem, rotateResultMenuItem, resultSep1, copyResultMenuItem, copySelectionMenuItem, addCollectionMenuItem, fillTextureMenuItem, cutStripsMenuItem, resultSep2, showMaskMenuItem, clearSelectionMenuItem, resultSep3, soundMenuItem });
            resultMenu.Name = "resultMenu";
            resultMenu.Size = new System.Drawing.Size(51, 20);
            resultMenu.Text = "&Result";
            //
            // mirrorResultMenuItem
            //
            mirrorResultMenuItem.Image = Properties.Resources.Mirror1;
            mirrorResultMenuItem.Name = "mirrorResultMenuItem";
            mirrorResultMenuItem.Size = new System.Drawing.Size(270, 22);
            mirrorResultMenuItem.Text = "Mirror";
            mirrorResultMenuItem.Click += MirrorResult_Click;
            //
            // rotateResultMenuItem
            //
            rotateResultMenuItem.Image = Properties.Resources.Rotate;
            rotateResultMenuItem.Name = "rotateResultMenuItem";
            rotateResultMenuItem.Size = new System.Drawing.Size(270, 22);
            rotateResultMenuItem.Text = "Rotate 90° left";
            rotateResultMenuItem.Click += RotateResult_Click;
            //
            // resultSep1
            //
            resultSep1.Name = "resultSep1";
            resultSep1.Size = new System.Drawing.Size(267, 6);
            //
            // copyResultMenuItem
            //
            copyResultMenuItem.Image = Properties.Resources.Copy;
            copyResultMenuItem.Name = "copyResultMenuItem";
            copyResultMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
            copyResultMenuItem.Size = new System.Drawing.Size(270, 22);
            copyResultMenuItem.Text = "Copy result";
            copyResultMenuItem.Click += CopyResult_Click;
            //
            // copySelectionMenuItem
            //
            copySelectionMenuItem.Name = "copySelectionMenuItem";
            copySelectionMenuItem.ShortcutKeyDisplayString = "Ctrl+Shift+C";
            copySelectionMenuItem.Size = new System.Drawing.Size(270, 22);
            copySelectionMenuItem.Text = "Copy selection";
            copySelectionMenuItem.Click += CopySelection_Click;
            //
            // fillTextureMenuItem
            //
            fillTextureMenuItem.Image = Properties.Resources.fill;
            fillTextureMenuItem.Name = "fillTextureMenuItem";
            fillTextureMenuItem.Size = new System.Drawing.Size(270, 22);
            fillTextureMenuItem.Text = "Fill selection with texture...";
            fillTextureMenuItem.Click += FillTexture_Click;
            //
            // cutStripsMenuItem
            //
            cutStripsMenuItem.Image = Properties.Resources.Cut;
            cutStripsMenuItem.Name = "cutStripsMenuItem";
            cutStripsMenuItem.Size = new System.Drawing.Size(270, 22);
            cutStripsMenuItem.Text = "Split into art pieces...";
            cutStripsMenuItem.Click += CutStrips_Click;
            //
            // resultSep2
            //
            resultSep2.Name = "resultSep2";
            resultSep2.Size = new System.Drawing.Size(267, 6);
            //
            // showMaskMenuItem
            //
            showMaskMenuItem.Image = Properties.Resources.image_eine_maske_laden03;
            showMaskMenuItem.Name = "showMaskMenuItem";
            showMaskMenuItem.Size = new System.Drawing.Size(270, 22);
            showMaskMenuItem.Text = "Show alignment mask (44 × 123)";
            showMaskMenuItem.Click += ShowMask_Click;
            //
            // clearSelectionMenuItem
            //
            clearSelectionMenuItem.Name = "clearSelectionMenuItem";
            clearSelectionMenuItem.ShortcutKeyDisplayString = "Esc";
            clearSelectionMenuItem.Size = new System.Drawing.Size(270, 22);
            clearSelectionMenuItem.Text = "Remove selection";
            clearSelectionMenuItem.Click += ClearSelection_Click;
            //
            // resultSep3
            //
            resultSep3.Name = "resultSep3";
            resultSep3.Size = new System.Drawing.Size(267, 6);
            //
            // soundMenuItem
            //
            soundMenuItem.Name = "soundMenuItem";
            soundMenuItem.Size = new System.Drawing.Size(270, 22);
            soundMenuItem.Text = "Play sound when copying";
            soundMenuItem.Click += Sound_Click;
            //
            // toolStrip1
            //
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsOpen, tsImport, tsSave, tsFormat, tsFillLabel, tsFillMode, tsSep1, tsUndo, tsRedo, tsSepU, tsCopy, tsAddCollection, tsMirror, tsRotate, tsCutStrips, tsFill, tsPipette, tsSep2, tsGrid, tsBorder, tsRuler, tsMask, tsSep3, tsBgLabel, tsBackground, tsBackColor, tsSep4, tsPanel, tsHelp });
            toolStrip1.Location = new System.Drawing.Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            toolStrip1.Size = new System.Drawing.Size(1360, 31);
            toolStrip1.TabIndex = 1;
            //
            // tsOpen
            //
            tsOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsOpen.Image = Properties.Resources.Load;
            tsOpen.Name = "tsOpen";
            tsOpen.Size = new System.Drawing.Size(24, 24);
            tsOpen.Text = "Open image";
            tsOpen.Click += OpenImage_Click;
            //
            // tsImport
            //
            tsImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsImport.Image = Properties.Resources.import;
            tsImport.Name = "tsImport";
            tsImport.Size = new System.Drawing.Size(24, 24);
            tsImport.Text = "Paste image";
            tsImport.Click += ImportClipboard_Click;
            //
            // tsSave
            //
            tsSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsSave.Image = Properties.Resources.Save;
            tsSave.Name = "tsSave";
            tsSave.Size = new System.Drawing.Size(24, 24);
            tsSave.Text = "Save result";
            tsSave.Click += Save_Click;
            //
            // tsFormat
            //
            tsFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tsFormat.Items.AddRange(new object[] { "BMP", "TIFF", "PNG", "JPEG" });
            tsFormat.Name = "tsFormat";
            tsFormat.Size = new System.Drawing.Size(70, 27);
            //
            // tsSep1
            //
            tsSep1.Name = "tsSep1";
            tsSep1.Size = new System.Drawing.Size(6, 27);
            //
            // tsCopy
            //
            tsCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsCopy.Image = Properties.Resources.Copy;
            tsCopy.Name = "tsCopy";
            tsCopy.Size = new System.Drawing.Size(24, 24);
            tsCopy.Text = "Copy result";
            tsCopy.Click += CopyResult_Click;
            //
            // tsMirror
            //
            tsMirror.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsMirror.Image = Properties.Resources.Mirror1;
            tsMirror.Name = "tsMirror";
            tsMirror.Size = new System.Drawing.Size(24, 24);
            tsMirror.Text = "Mirror result";
            tsMirror.Click += MirrorResult_Click;
            //
            // tsRotate
            //
            tsRotate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsRotate.Image = Properties.Resources.Rotate;
            tsRotate.Name = "tsRotate";
            tsRotate.Size = new System.Drawing.Size(24, 24);
            tsRotate.Text = "Rotate result";
            tsRotate.Click += RotateResult_Click;
            //
            // tsCutStrips
            //
            tsCutStrips.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsCutStrips.Image = Properties.Resources.Cut;
            tsCutStrips.Name = "tsCutStrips";
            tsCutStrips.Size = new System.Drawing.Size(24, 24);
            tsCutStrips.Text = "Cut into strips";
            tsCutStrips.Click += CutStrips_Click;
            //
            // tsFill
            //
            tsFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsFill.Image = Properties.Resources.fill;
            tsFill.Name = "tsFill";
            tsFill.Size = new System.Drawing.Size(24, 24);
            tsFill.Text = "Fill texture";
            tsFill.Click += FillTexture_Click;
            //
            // tsSep2
            //
            tsSep2.Name = "tsSep2";
            tsSep2.Size = new System.Drawing.Size(6, 27);
            //
            // tsGrid
            //
            tsGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsGrid.Image = Properties.Resources.Schablone;
            tsGrid.Name = "tsGrid";
            tsGrid.Size = new System.Drawing.Size(24, 24);
            tsGrid.Text = "Grid";
            tsGrid.Click += Grid_Click;
            //
            // tsBorder
            //
            tsBorder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBorder.Image = Properties.Resources.Rahmen;
            tsBorder.Name = "tsBorder";
            tsBorder.Size = new System.Drawing.Size(24, 24);
            tsBorder.Text = "Image border";
            tsBorder.Click += Border_Click;
            //
            // tsRuler
            //
            tsRuler.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsRuler.Image = Properties.Resources.ruler;
            tsRuler.Name = "tsRuler";
            tsRuler.Size = new System.Drawing.Size(24, 24);
            tsRuler.Text = "Ruler";
            tsRuler.Click += Ruler_Click;
            //
            // tsMask
            //
            tsMask.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsMask.Image = Properties.Resources.image_eine_maske_laden03;
            tsMask.Name = "tsMask";
            tsMask.Size = new System.Drawing.Size(24, 24);
            tsMask.Text = "Alignment mask";
            tsMask.Click += ShowMask_Click;
            //
            // tsSep3
            //
            tsSep3.Name = "tsSep3";
            tsSep3.Size = new System.Drawing.Size(6, 27);
            //
            // tsBgLabel
            //
            tsBgLabel.Name = "tsBgLabel";
            tsBgLabel.Size = new System.Drawing.Size(74, 24);
            tsBgLabel.Text = "Background:";
            //
            // tsBackground
            //
            tsBackground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tsBackground.Items.AddRange(new object[] { "none", "grass", "water", "land tile" });
            tsBackground.Name = "tsBackground";
            tsBackground.Size = new System.Drawing.Size(80, 27);
            tsBackground.SelectedIndexChanged += BackgroundCombo_SelectedIndexChanged;
            //
            // tsBackColor
            //
            tsBackColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBackColor.Image = Properties.Resources.Color;
            tsBackColor.Name = "tsBackColor";
            tsBackColor.Size = new System.Drawing.Size(24, 24);
            tsBackColor.Text = "Black / white background";
            tsBackColor.Click += BackColorToggle_Click;
            //
            // saveArtMenuItem
            //
            saveArtMenuItem.Name = "saveArtMenuItem";
            saveArtMenuItem.Size = new System.Drawing.Size(250, 22);
            saveArtMenuItem.Text = "Save art.mul (output folder)";
            saveArtMenuItem.Click += SaveArt_Click;
            //
            // editMenu
            //
            editMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { undoMenuItem, redoMenuItem, editSep1, pipetteMenuItem, clearPickedMenuItem });
            editMenu.Name = "editMenu";
            editMenu.Size = new System.Drawing.Size(39, 20);
            editMenu.Text = "&Edit";
            //
            // undoMenuItem
            //
            undoMenuItem.ShortcutKeyDisplayString = "Ctrl+Z";
            undoMenuItem.Name = "undoMenuItem";
            undoMenuItem.Size = new System.Drawing.Size(250, 22);
            undoMenuItem.Text = "Undo";
            undoMenuItem.Click += Undo_Click;
            //
            // redoMenuItem
            //
            redoMenuItem.ShortcutKeyDisplayString = "Ctrl+Y";
            redoMenuItem.Name = "redoMenuItem";
            redoMenuItem.Size = new System.Drawing.Size(250, 22);
            redoMenuItem.Text = "Redo";
            redoMenuItem.Click += Redo_Click;
            //
            // editSep1
            //
            editSep1.Name = "editSep1";
            editSep1.Size = new System.Drawing.Size(247, 6);
            //
            // pipetteMenuItem
            //
            pipetteMenuItem.Name = "pipetteMenuItem";
            pipetteMenuItem.Size = new System.Drawing.Size(250, 22);
            pipetteMenuItem.Text = "Pipette (pick a transparent color)";
            pipetteMenuItem.Click += Pipette_Click;
            //
            // clearPickedMenuItem
            //
            clearPickedMenuItem.Name = "clearPickedMenuItem";
            clearPickedMenuItem.Size = new System.Drawing.Size(250, 22);
            clearPickedMenuItem.Text = "Clear picked colors";
            clearPickedMenuItem.Click += ClearPicked_Click;
            //
            // snapMenuItem
            //
            snapMenuItem.Name = "snapMenuItem";
            snapMenuItem.Size = new System.Drawing.Size(250, 22);
            snapMenuItem.Text = "Snap cut area to grid";
            snapMenuItem.Click += SnapMenu_Click;
            //
            // sidePanelMenuItem
            //
            sidePanelMenuItem.ShortcutKeyDisplayString = "Ctrl+B";
            sidePanelMenuItem.Name = "sidePanelMenuItem";
            sidePanelMenuItem.Size = new System.Drawing.Size(250, 22);
            sidePanelMenuItem.Text = "Art / collection panel";
            sidePanelMenuItem.Click += SidePanel_Click;
            //
            // bgLandMenuItem
            //
            bgLandMenuItem.Name = "bgLandMenuItem";
            bgLandMenuItem.Size = new System.Drawing.Size(250, 22);
            bgLandMenuItem.Text = "Land tile (art.mul)";
            bgLandMenuItem.Click += BgLand_Click;
            //
            // addCollectionMenuItem
            //
            addCollectionMenuItem.ShortcutKeyDisplayString = "Ctrl+Enter";
            addCollectionMenuItem.Name = "addCollectionMenuItem";
            addCollectionMenuItem.Size = new System.Drawing.Size(250, 22);
            addCollectionMenuItem.Text = "Add to collection";
            addCollectionMenuItem.Click += AddToCollection_Click;
            //
            // ctxAddCollection
            //
            ctxAddCollection.ShortcutKeyDisplayString = "Ctrl+Enter";
            ctxAddCollection.Name = "ctxAddCollection";
            ctxAddCollection.Size = new System.Drawing.Size(250, 22);
            ctxAddCollection.Text = "Add to collection";
            ctxAddCollection.Click += AddToCollection_Click;
            //
            // helpMenu
            //
            helpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { helpMenuItem, helpEnglishMenuItem, helpGermanMenuItem });
            helpMenu.Name = "helpMenu";
            helpMenu.Size = new System.Drawing.Size(44, 20);
            helpMenu.Text = "&Help";
            //
            // helpMenuItem
            //
            helpMenuItem.ShortcutKeyDisplayString = "F1";
            helpMenuItem.Name = "helpMenuItem";
            helpMenuItem.Size = new System.Drawing.Size(250, 22);
            helpMenuItem.Text = "Help / Hilfe";
            helpMenuItem.Click += Help_Click;
            //
            // helpEnglishMenuItem
            //
            helpEnglishMenuItem.Name = "helpEnglishMenuItem";
            helpEnglishMenuItem.Size = new System.Drawing.Size(250, 22);
            helpEnglishMenuItem.Text = "Help (English)";
            helpEnglishMenuItem.Click += HelpEnglish_Click;
            //
            // helpGermanMenuItem
            //
            helpGermanMenuItem.Name = "helpGermanMenuItem";
            helpGermanMenuItem.Size = new System.Drawing.Size(250, 22);
            helpGermanMenuItem.Text = "Hilfe (Deutsch)";
            helpGermanMenuItem.Click += HelpGerman_Click;
            //
            // tsFillLabel
            //
            tsFillLabel.Name = "tsFillLabel";
            tsFillLabel.Text = "Transparent:";
            //
            // tsFillMode
            //
            tsFillMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            tsFillMode.Items.AddRange(new object[] { "keep", "black", "white" });
            tsFillMode.Name = "tsFillMode";
            tsFillMode.Size = new System.Drawing.Size(64, 27);
            //
            // tsUndo
            //
            tsUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsUndo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            tsUndo.Name = "tsUndo";
            tsUndo.Text = "↶";
            tsUndo.Click += Undo_Click;
            //
            // tsRedo
            //
            tsRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsRedo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            tsRedo.Name = "tsRedo";
            tsRedo.Text = "↷";
            tsRedo.Click += Redo_Click;
            //
            // tsAddCollection
            //
            tsAddCollection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsAddCollection.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            tsAddCollection.Name = "tsAddCollection";
            tsAddCollection.Text = "+";
            tsAddCollection.Click += AddToCollection_Click;
            //
            // tsPipette
            //
            tsPipette.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsPipette.Name = "tsPipette";
            tsPipette.Text = "Pipette";
            tsPipette.Click += Pipette_Click;
            //
            // tsPanel
            //
            tsPanel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsPanel.Name = "tsPanel";
            tsPanel.Text = "art.mul";
            tsPanel.Click += SidePanel_Click;
            //
            // tsHelp
            //
            tsHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsHelp.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            tsHelp.Name = "tsHelp";
            tsHelp.Text = "?";
            tsHelp.Click += Help_Click;
            //
            // tsSepU
            //
            tsSepU.Name = "tsSepU";
            tsSepU.Size = new System.Drawing.Size(6, 27);
            //
            // tsSep4
            //
            tsSep4.Name = "tsSep4";
            tsSep4.Size = new System.Drawing.Size(6, 27);
            //
            // statusStrip1
            //
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusPos, statusInfo, statusMessage });
            statusStrip1.Location = new System.Drawing.Point(0, 818);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1360, 22);
            statusStrip1.TabIndex = 3;
            //
            // statusPos
            //
            statusPos.AutoSize = false;
            statusPos.Name = "statusPos";
            statusPos.Size = new System.Drawing.Size(130, 17);
            statusPos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // statusInfo
            //
            statusInfo.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            statusInfo.Name = "statusInfo";
            statusInfo.Size = new System.Drawing.Size(4, 17);
            //
            // statusMessage
            //
            statusMessage.Name = "statusMessage";
            statusMessage.Size = new System.Drawing.Size(855, 17);
            statusMessage.Spring = true;
            statusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // mainLayout
            //
            mainLayout.ColumnCount = 3;
            mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 440F));
            mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 320F));
            mainLayout.Controls.Add(groupStage, 0, 0);
            mainLayout.Controls.Add(groupResult, 1, 0);
            mainLayout.Controls.Add(flowControls, 0, 1);
            mainLayout.Controls.Add(sidePanel, 2, 0);
            mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            mainLayout.Location = new System.Drawing.Point(0, 55);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new System.Windows.Forms.Padding(4);
            mainLayout.RowCount = 2;
            mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            mainLayout.Size = new System.Drawing.Size(1360, 763);
            mainLayout.TabIndex = 2;
            mainLayout.SetColumnSpan(flowControls, 2);
            mainLayout.SetRowSpan(sidePanel, 2);
            //
            // sidePanel
            //
            sidePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            sidePanel.Name = "sidePanel";
            sidePanel.TabIndex = 3;
            //
            // groupStage
            //
            groupStage.Controls.Add(panelStage);
            groupStage.Dock = System.Windows.Forms.DockStyle.Fill;
            groupStage.Location = new System.Drawing.Point(7, 7);
            groupStage.Name = "groupStage";
            groupStage.Size = new System.Drawing.Size(434, 494);
            groupStage.TabIndex = 0;
            groupStage.TabStop = false;
            groupStage.Text = "Stage – position the image";
            //
            // panelStage
            //
            panelStage.AutoScroll = true;
            panelStage.AutoScrollMinSize = new System.Drawing.Size(410, 410);
            panelStage.Controls.Add(pictureBox1);
            panelStage.Dock = System.Windows.Forms.DockStyle.Fill;
            panelStage.Location = new System.Drawing.Point(3, 19);
            panelStage.Name = "panelStage";
            panelStage.Size = new System.Drawing.Size(428, 472);
            panelStage.TabIndex = 0;
            panelStage.Paint += PanelStage_Paint;
            //
            // pictureBox1
            //
            pictureBox1.BackColor = System.Drawing.Color.Black;
            pictureBox1.Location = new System.Drawing.Point(44, 8);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(352, 352);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += PictureBox1_Paint;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseLeave += PictureBox1_MouseLeave;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            //
            // groupResult
            //
            groupResult.Controls.Add(panelResult);
            groupResult.Dock = System.Windows.Forms.DockStyle.Fill;
            groupResult.Location = new System.Drawing.Point(447, 7);
            groupResult.Name = "groupResult";
            groupResult.Size = new System.Drawing.Size(550, 494);
            groupResult.TabIndex = 1;
            groupResult.TabStop = false;
            groupResult.Text = "Result – cut out area";
            //
            // panelResult
            //
            panelResult.AutoScroll = true;
            panelResult.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            panelResult.Controls.Add(pictureBox2);
            panelResult.Dock = System.Windows.Forms.DockStyle.Fill;
            panelResult.Location = new System.Drawing.Point(3, 19);
            panelResult.Name = "panelResult";
            panelResult.Size = new System.Drawing.Size(544, 472);
            panelResult.TabIndex = 0;
            panelResult.Paint += PanelResult_Paint;
            panelResult.Resize += PanelResult_Resize;
            //
            // pictureBox2
            //
            pictureBox2.ContextMenuStrip = contextMenuStrip1;
            pictureBox2.Cursor = System.Windows.Forms.Cursors.Cross;
            pictureBox2.Location = new System.Drawing.Point(0, 0);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(1, 1);
            pictureBox2.TabIndex = 0;
            pictureBox2.TabStop = false;
            pictureBox2.Paint += PictureBox2_Paint;
            pictureBox2.MouseDown += PictureBox2_MouseDown;
            pictureBox2.MouseMove += PictureBox2_MouseMove;
            pictureBox2.MouseUp += PictureBox2_MouseUp;
            //
            // contextMenuStrip1
            //
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { ctxOpen, ctxSave, ctxSaveAs, ctxUnload, ctxSep1, ctxGrid, ctxMirror, ctxRotate, ctxCutStrips, ctxFill, ctxMask, ctxSep2, ctxCopySelection, ctxCopyResult, ctxAddCollection });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(250, 280);
            //
            // ctxOpen
            //
            ctxOpen.Image = Properties.Resources.Load;
            ctxOpen.Name = "ctxOpen";
            ctxOpen.Size = new System.Drawing.Size(249, 22);
            ctxOpen.Text = "Open image into result...";
            ctxOpen.Click += OpenIntoResult_Click;
            //
            // ctxSave
            //
            ctxSave.Image = Properties.Resources.Save;
            ctxSave.Name = "ctxSave";
            ctxSave.ShortcutKeyDisplayString = "Ctrl+S";
            ctxSave.Size = new System.Drawing.Size(249, 22);
            ctxSave.Text = "Save (tempGrafic)";
            ctxSave.Click += Save_Click;
            //
            // ctxSaveAs
            //
            ctxSaveAs.Name = "ctxSaveAs";
            ctxSaveAs.Size = new System.Drawing.Size(249, 22);
            ctxSaveAs.Text = "Save as...";
            ctxSaveAs.Click += SaveAs_Click;
            //
            // ctxUnload
            //
            ctxUnload.Image = Properties.Resources.Leeren;
            ctxUnload.Name = "ctxUnload";
            ctxUnload.Size = new System.Drawing.Size(249, 22);
            ctxUnload.Text = "Unload result";
            ctxUnload.Click += UnloadResult_Click;
            //
            // ctxSep1
            //
            ctxSep1.Name = "ctxSep1";
            ctxSep1.Size = new System.Drawing.Size(246, 6);
            //
            // ctxGrid
            //
            ctxGrid.Image = Properties.Resources.Schablone;
            ctxGrid.Name = "ctxGrid";
            ctxGrid.Size = new System.Drawing.Size(249, 22);
            ctxGrid.Text = "Grid on result";
            ctxGrid.Click += GridResult_Click;
            //
            // ctxMirror
            //
            ctxMirror.Image = Properties.Resources.Mirror1;
            ctxMirror.Name = "ctxMirror";
            ctxMirror.Size = new System.Drawing.Size(249, 22);
            ctxMirror.Text = "Mirror";
            ctxMirror.Click += MirrorResult_Click;
            //
            // ctxRotate
            //
            ctxRotate.Image = Properties.Resources.Rotate;
            ctxRotate.Name = "ctxRotate";
            ctxRotate.Size = new System.Drawing.Size(249, 22);
            ctxRotate.Text = "Rotate 90° left";
            ctxRotate.Click += RotateResult_Click;
            //
            // ctxCutStrips
            //
            ctxCutStrips.Image = Properties.Resources.Cut;
            ctxCutStrips.Name = "ctxCutStrips";
            ctxCutStrips.Size = new System.Drawing.Size(249, 22);
            ctxCutStrips.Text = "Split into art pieces...";
            ctxCutStrips.Click += CutStrips_Click;
            //
            // ctxFill
            //
            ctxFill.Image = Properties.Resources.fill;
            ctxFill.Name = "ctxFill";
            ctxFill.Size = new System.Drawing.Size(249, 22);
            ctxFill.Text = "Fill selection with texture...";
            ctxFill.Click += FillTexture_Click;
            //
            // ctxMask
            //
            ctxMask.Image = Properties.Resources.image_eine_maske_laden03;
            ctxMask.Name = "ctxMask";
            ctxMask.Size = new System.Drawing.Size(249, 22);
            ctxMask.Text = "Show alignment mask";
            ctxMask.Click += ShowMask_Click;
            //
            // ctxSep2
            //
            ctxSep2.Name = "ctxSep2";
            ctxSep2.Size = new System.Drawing.Size(246, 6);
            //
            // ctxCopySelection
            //
            ctxCopySelection.Image = Properties.Resources.Copy;
            ctxCopySelection.Name = "ctxCopySelection";
            ctxCopySelection.ShortcutKeyDisplayString = "Ctrl+Shift+C";
            ctxCopySelection.Size = new System.Drawing.Size(249, 22);
            ctxCopySelection.Text = "Copy selection";
            ctxCopySelection.Click += CopySelection_Click;
            //
            // ctxCopyResult
            //
            ctxCopyResult.Image = Properties.Resources.import;
            ctxCopyResult.Name = "ctxCopyResult";
            ctxCopyResult.ShortcutKeyDisplayString = "Ctrl+C";
            ctxCopyResult.Size = new System.Drawing.Size(249, 22);
            ctxCopyResult.Text = "Copy result";
            ctxCopyResult.Click += CopyResult_Click;
            //
            // flowControls
            //
            flowControls.AutoSize = true;
            flowControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowControls.Controls.Add(groupMoveImage);
            flowControls.Controls.Add(groupCut);
            flowControls.Controls.Add(groupMoveCut);
            flowControls.Controls.Add(groupOptions);
            flowControls.Dock = System.Windows.Forms.DockStyle.Fill;
            flowControls.Location = new System.Drawing.Point(7, 507);
            flowControls.Name = "flowControls";
            flowControls.Size = new System.Drawing.Size(1026, 192);
            flowControls.TabIndex = 2;
            //
            // groupMoveImage
            //
            groupMoveImage.Controls.Add(btnUp);
            groupMoveImage.Controls.Add(btnLeft);
            groupMoveImage.Controls.Add(btnRight);
            groupMoveImage.Controls.Add(btnDown);
            groupMoveImage.Location = new System.Drawing.Point(3, 3);
            groupMoveImage.Name = "groupMoveImage";
            groupMoveImage.Size = new System.Drawing.Size(164, 182);
            groupMoveImage.TabIndex = 0;
            groupMoveImage.TabStop = false;
            groupMoveImage.Text = "Move image";
            //
            // btnUp
            //
            btnUp.BackgroundImage = Properties.Resources.up;
            btnUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            btnUp.Location = new System.Drawing.Point(61, 20);
            btnUp.Name = "btnUp";
            btnUp.Size = new System.Drawing.Size(42, 42);
            btnUp.TabIndex = 0;
            btnUp.UseVisualStyleBackColor = true;
            //
            // btnLeft
            //
            btnLeft.BackgroundImage = Properties.Resources.left;
            btnLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            btnLeft.Location = new System.Drawing.Point(15, 64);
            btnLeft.Name = "btnLeft";
            btnLeft.Size = new System.Drawing.Size(42, 42);
            btnLeft.TabIndex = 1;
            btnLeft.UseVisualStyleBackColor = true;
            //
            // btnRight
            //
            btnRight.BackgroundImage = Properties.Resources.right;
            btnRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            btnRight.Location = new System.Drawing.Point(107, 64);
            btnRight.Name = "btnRight";
            btnRight.Size = new System.Drawing.Size(42, 42);
            btnRight.TabIndex = 2;
            btnRight.UseVisualStyleBackColor = true;
            //
            // btnDown
            //
            btnDown.BackgroundImage = Properties.Resources.down;
            btnDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            btnDown.Location = new System.Drawing.Point(61, 108);
            btnDown.Name = "btnDown";
            btnDown.Size = new System.Drawing.Size(42, 42);
            btnDown.TabIndex = 3;
            btnDown.UseVisualStyleBackColor = true;
            //
            // groupCut
            //
            groupCut.Controls.Add(labelWidth);
            groupCut.Controls.Add(numericWidth);
            groupCut.Controls.Add(labelHeight);
            groupCut.Controls.Add(numericHeight);
            groupCut.Controls.Add(labelX);
            groupCut.Controls.Add(numericX);
            groupCut.Controls.Add(labelY);
            groupCut.Controls.Add(numericY);
            groupCut.Controls.Add(labelPreset);
            groupCut.Controls.Add(comboPreset);
            groupCut.Controls.Add(checkBoxFollow);
            groupCut.Controls.Add(checkBoxSnap);
            groupCut.Location = new System.Drawing.Point(173, 3);
            groupCut.Name = "groupCut";
            groupCut.Size = new System.Drawing.Size(250, 182);
            groupCut.TabIndex = 1;
            groupCut.TabStop = false;
            groupCut.Text = "Cut area";
            //
            // labelWidth
            //
            labelWidth.AutoSize = true;
            labelWidth.Location = new System.Drawing.Point(10, 26);
            labelWidth.Name = "labelWidth";
            labelWidth.Size = new System.Drawing.Size(42, 15);
            labelWidth.TabIndex = 0;
            labelWidth.Text = "Width:";
            //
            // numericWidth
            //
            numericWidth.Location = new System.Drawing.Point(60, 22);
            numericWidth.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            numericWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericWidth.Name = "numericWidth";
            numericWidth.Size = new System.Drawing.Size(60, 23);
            numericWidth.TabIndex = 1;
            numericWidth.Value = new decimal(new int[] { 44, 0, 0, 0 });
            numericWidth.ValueChanged += CutField_ValueChanged;
            //
            // labelHeight
            //
            labelHeight.AutoSize = true;
            labelHeight.Location = new System.Drawing.Point(128, 26);
            labelHeight.Name = "labelHeight";
            labelHeight.Size = new System.Drawing.Size(46, 15);
            labelHeight.TabIndex = 2;
            labelHeight.Text = "Height:";
            //
            // numericHeight
            //
            numericHeight.Location = new System.Drawing.Point(180, 22);
            numericHeight.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            numericHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericHeight.Name = "numericHeight";
            numericHeight.Size = new System.Drawing.Size(60, 23);
            numericHeight.TabIndex = 3;
            numericHeight.Value = new decimal(new int[] { 105, 0, 0, 0 });
            numericHeight.ValueChanged += CutField_ValueChanged;
            //
            // labelX
            //
            labelX.AutoSize = true;
            labelX.Location = new System.Drawing.Point(10, 58);
            labelX.Name = "labelX";
            labelX.Size = new System.Drawing.Size(17, 15);
            labelX.TabIndex = 4;
            labelX.Text = "X:";
            //
            // numericX
            //
            numericX.Location = new System.Drawing.Point(60, 54);
            numericX.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            numericX.Minimum = new decimal(new int[] { 4096, 0, 0, int.MinValue });
            numericX.Name = "numericX";
            numericX.Size = new System.Drawing.Size(60, 23);
            numericX.TabIndex = 5;
            numericX.Value = new decimal(new int[] { 174, 0, 0, 0 });
            numericX.ValueChanged += CutField_ValueChanged;
            //
            // labelY
            //
            labelY.AutoSize = true;
            labelY.Location = new System.Drawing.Point(128, 58);
            labelY.Name = "labelY";
            labelY.Size = new System.Drawing.Size(17, 15);
            labelY.TabIndex = 6;
            labelY.Text = "Y:";
            //
            // numericY
            //
            numericY.Location = new System.Drawing.Point(180, 54);
            numericY.Maximum = new decimal(new int[] { 4096, 0, 0, 0 });
            numericY.Minimum = new decimal(new int[] { 4096, 0, 0, int.MinValue });
            numericY.Name = "numericY";
            numericY.Size = new System.Drawing.Size(60, 23);
            numericY.TabIndex = 7;
            numericY.Value = new decimal(new int[] { 247, 0, 0, 0 });
            numericY.ValueChanged += CutField_ValueChanged;
            //
            // labelPreset
            //
            labelPreset.AutoSize = true;
            labelPreset.Location = new System.Drawing.Point(10, 90);
            labelPreset.Name = "labelPreset";
            labelPreset.Size = new System.Drawing.Size(42, 15);
            labelPreset.TabIndex = 8;
            labelPreset.Text = "Preset:";
            //
            // comboPreset
            //
            comboPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboPreset.Items.AddRange(new object[] { "(choose size)", "Image size", "44 × 44 (tile)", "44 × 105", "44 × 123 (mask)" });
            comboPreset.Location = new System.Drawing.Point(60, 86);
            comboPreset.Name = "comboPreset";
            comboPreset.Size = new System.Drawing.Size(180, 23);
            comboPreset.TabIndex = 9;
            comboPreset.SelectedIndexChanged += Preset_SelectedIndexChanged;
            //
            // checkBoxFollow
            //
            checkBoxFollow.AutoSize = true;
            checkBoxFollow.Checked = true;
            checkBoxFollow.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxFollow.Location = new System.Drawing.Point(12, 120);
            checkBoxFollow.Name = "checkBoxFollow";
            checkBoxFollow.Size = new System.Drawing.Size(181, 19);
            checkBoxFollow.TabIndex = 10;
            checkBoxFollow.Text = "Cut area follows the image";
            checkBoxFollow.UseVisualStyleBackColor = true;
            //
            // checkBoxSnap
            //
            checkBoxSnap.AutoSize = true;
            checkBoxSnap.Location = new System.Drawing.Point(12, 146);
            checkBoxSnap.Name = "checkBoxSnap";
            checkBoxSnap.Size = new System.Drawing.Size(150, 19);
            checkBoxSnap.TabIndex = 11;
            checkBoxSnap.Text = "Snap to grid";
            checkBoxSnap.UseVisualStyleBackColor = true;
            checkBoxSnap.CheckedChanged += Snap_CheckedChanged;
            //
            // groupMoveCut
            //
            groupMoveCut.Controls.Add(btnCutUp);
            groupMoveCut.Controls.Add(btnCutLeft);
            groupMoveCut.Controls.Add(btnCutRight);
            groupMoveCut.Controls.Add(btnCutDown);
            groupMoveCut.Controls.Add(buttonQuickSave);
            groupMoveCut.Location = new System.Drawing.Point(429, 3);
            groupMoveCut.Name = "groupMoveCut";
            groupMoveCut.Size = new System.Drawing.Size(164, 182);
            groupMoveCut.TabIndex = 2;
            groupMoveCut.TabStop = false;
            groupMoveCut.Text = "Move cut area";
            //
            // btnCutUp
            //
            btnCutUp.BackgroundImage = Properties.Resources.up;
            btnCutUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            btnCutUp.Location = new System.Drawing.Point(61, 20);
            btnCutUp.Name = "btnCutUp";
            btnCutUp.Size = new System.Drawing.Size(42, 42);
            btnCutUp.TabIndex = 0;
            btnCutUp.UseVisualStyleBackColor = true;
            //
            // btnCutLeft
            //
            btnCutLeft.BackgroundImage = Properties.Resources.left;
            btnCutLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            btnCutLeft.Location = new System.Drawing.Point(15, 64);
            btnCutLeft.Name = "btnCutLeft";
            btnCutLeft.Size = new System.Drawing.Size(42, 42);
            btnCutLeft.TabIndex = 1;
            btnCutLeft.UseVisualStyleBackColor = true;
            //
            // btnCutRight
            //
            btnCutRight.BackgroundImage = Properties.Resources.right;
            btnCutRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            btnCutRight.Location = new System.Drawing.Point(107, 64);
            btnCutRight.Name = "btnCutRight";
            btnCutRight.Size = new System.Drawing.Size(42, 42);
            btnCutRight.TabIndex = 2;
            btnCutRight.UseVisualStyleBackColor = true;
            //
            // btnCutDown
            //
            btnCutDown.BackgroundImage = Properties.Resources.down;
            btnCutDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            btnCutDown.Location = new System.Drawing.Point(61, 108);
            btnCutDown.Name = "btnCutDown";
            btnCutDown.Size = new System.Drawing.Size(42, 42);
            btnCutDown.TabIndex = 3;
            btnCutDown.UseVisualStyleBackColor = true;
            //
            // buttonQuickSave
            //
            buttonQuickSave.BackgroundImage = Properties.Resources.Save;
            buttonQuickSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            buttonQuickSave.Location = new System.Drawing.Point(115, 20);
            buttonQuickSave.Name = "buttonQuickSave";
            buttonQuickSave.Size = new System.Drawing.Size(34, 34);
            buttonQuickSave.TabIndex = 4;
            buttonQuickSave.UseVisualStyleBackColor = true;
            buttonQuickSave.Click += Save_Click;
            //
            // groupOptions
            //
            groupOptions.Controls.Add(checkBoxTransparent);
            groupOptions.Controls.Add(labelTolerance);
            groupOptions.Controls.Add(numericTolerance);
            groupOptions.Controls.Add(labelSelection);
            groupOptions.Controls.Add(radioRectangle);
            groupOptions.Controls.Add(radioCircle);
            groupOptions.Controls.Add(radioFreehand);
            groupOptions.Controls.Add(labelZoomTitle);
            groupOptions.Controls.Add(zoomOutButton);
            groupOptions.Controls.Add(labelZoom);
            groupOptions.Controls.Add(zoomButton);
            groupOptions.Controls.Add(resetButton);
            groupOptions.Controls.Add(checkBoxBake);
            groupOptions.Controls.Add(labelPicked);
            groupOptions.Controls.Add(buttonClearPicked);
            groupOptions.Location = new System.Drawing.Point(599, 3);
            groupOptions.Name = "groupOptions";
            groupOptions.Size = new System.Drawing.Size(300, 182);
            groupOptions.TabIndex = 3;
            groupOptions.TabStop = false;
            groupOptions.Text = "Options";
            //
            // checkBoxTransparent
            //
            checkBoxTransparent.AutoSize = true;
            checkBoxTransparent.Checked = true;
            checkBoxTransparent.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxTransparent.Location = new System.Drawing.Point(12, 24);
            checkBoxTransparent.Name = "checkBoxTransparent";
            checkBoxTransparent.Size = new System.Drawing.Size(158, 19);
            checkBoxTransparent.TabIndex = 0;
            checkBoxTransparent.Text = "Black / white transparent";
            checkBoxTransparent.UseVisualStyleBackColor = true;
            checkBoxTransparent.CheckedChanged += TransparentCheckBox_CheckedChanged;
            //
            // labelTolerance
            //
            labelTolerance.AutoSize = true;
            labelTolerance.Location = new System.Drawing.Point(184, 25);
            labelTolerance.Name = "labelTolerance";
            labelTolerance.Size = new System.Drawing.Size(31, 15);
            labelTolerance.TabIndex = 1;
            labelTolerance.Text = "Tol.:";
            //
            // numericTolerance
            //
            numericTolerance.Location = new System.Drawing.Point(226, 22);
            numericTolerance.Maximum = new decimal(new int[] { 128, 0, 0, 0 });
            numericTolerance.Name = "numericTolerance";
            numericTolerance.Size = new System.Drawing.Size(60, 23);
            numericTolerance.TabIndex = 2;
            numericTolerance.ValueChanged += Tolerance_ValueChanged;
            //
            // labelSelection
            //
            labelSelection.AutoSize = true;
            labelSelection.Location = new System.Drawing.Point(10, 56);
            labelSelection.Name = "labelSelection";
            labelSelection.Size = new System.Drawing.Size(58, 15);
            labelSelection.TabIndex = 3;
            labelSelection.Text = "Selection:";
            //
            // radioRectangle
            //
            radioRectangle.AutoSize = true;
            radioRectangle.Checked = true;
            radioRectangle.Location = new System.Drawing.Point(76, 54);
            radioRectangle.Name = "radioRectangle";
            radioRectangle.Size = new System.Drawing.Size(77, 19);
            radioRectangle.TabIndex = 4;
            radioRectangle.TabStop = true;
            radioRectangle.Text = "Rectangle";
            radioRectangle.UseVisualStyleBackColor = true;
            radioRectangle.CheckedChanged += SelectionMode_CheckedChanged;
            //
            // radioCircle
            //
            radioCircle.AutoSize = true;
            radioCircle.Location = new System.Drawing.Point(158, 54);
            radioCircle.Name = "radioCircle";
            radioCircle.Size = new System.Drawing.Size(55, 19);
            radioCircle.TabIndex = 5;
            radioCircle.Text = "Circle";
            radioCircle.UseVisualStyleBackColor = true;
            radioCircle.CheckedChanged += SelectionMode_CheckedChanged;
            //
            // radioFreehand
            //
            radioFreehand.AutoSize = true;
            radioFreehand.Location = new System.Drawing.Point(218, 54);
            radioFreehand.Name = "radioFreehand";
            radioFreehand.Size = new System.Drawing.Size(74, 19);
            radioFreehand.TabIndex = 6;
            radioFreehand.Text = "Freehand";
            radioFreehand.UseVisualStyleBackColor = true;
            radioFreehand.CheckedChanged += SelectionMode_CheckedChanged;
            //
            // labelZoomTitle
            //
            labelZoomTitle.AutoSize = true;
            labelZoomTitle.Location = new System.Drawing.Point(10, 91);
            labelZoomTitle.Name = "labelZoomTitle";
            labelZoomTitle.Size = new System.Drawing.Size(77, 15);
            labelZoomTitle.TabIndex = 7;
            labelZoomTitle.Text = "Result zoom:";
            //
            // zoomOutButton
            //
            zoomOutButton.Location = new System.Drawing.Point(96, 86);
            zoomOutButton.Name = "zoomOutButton";
            zoomOutButton.Size = new System.Drawing.Size(30, 26);
            zoomOutButton.TabIndex = 8;
            zoomOutButton.Text = "–";
            zoomOutButton.UseVisualStyleBackColor = true;
            zoomOutButton.Click += ZoomOut_Click;
            //
            // labelZoom
            //
            labelZoom.Location = new System.Drawing.Point(128, 91);
            labelZoom.Name = "labelZoom";
            labelZoom.Size = new System.Drawing.Size(34, 15);
            labelZoom.TabIndex = 9;
            labelZoom.Text = "1x";
            labelZoom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // zoomButton
            //
            zoomButton.Location = new System.Drawing.Point(164, 86);
            zoomButton.Name = "zoomButton";
            zoomButton.Size = new System.Drawing.Size(30, 26);
            zoomButton.TabIndex = 10;
            zoomButton.Text = "+";
            zoomButton.UseVisualStyleBackColor = true;
            zoomButton.Click += ZoomIn_Click;
            //
            // resetButton
            //
            resetButton.Location = new System.Drawing.Point(206, 86);
            resetButton.Name = "resetButton";
            resetButton.Size = new System.Drawing.Size(80, 26);
            resetButton.TabIndex = 11;
            resetButton.Text = "Reset";
            resetButton.UseVisualStyleBackColor = true;
            resetButton.Click += Reset_Click;
            //
            // checkBoxBake
            //
            checkBoxBake.AutoSize = true;
            checkBoxBake.Location = new System.Drawing.Point(12, 120);
            checkBoxBake.Name = "checkBoxBake";
            checkBoxBake.Size = new System.Drawing.Size(196, 19);
            checkBoxBake.TabIndex = 12;
            checkBoxBake.Text = "Cut with grid and background";
            checkBoxBake.UseVisualStyleBackColor = true;
            checkBoxBake.CheckedChanged += Bake_CheckedChanged;
            //
            // labelPicked
            //
            labelPicked.Location = new System.Drawing.Point(10, 150);
            labelPicked.Name = "labelPicked";
            labelPicked.Size = new System.Drawing.Size(210, 18);
            labelPicked.TabIndex = 13;
            labelPicked.Text = "Picked colors: none";
            labelPicked.Paint += LabelPicked_Paint;
            //
            // buttonClearPicked
            //
            buttonClearPicked.Location = new System.Drawing.Point(226, 145);
            buttonClearPicked.Name = "buttonClearPicked";
            buttonClearPicked.Size = new System.Drawing.Size(60, 26);
            buttonClearPicked.TabIndex = 14;
            buttonClearPicked.Text = "Clear";
            buttonClearPicked.UseVisualStyleBackColor = true;
            buttonClearPicked.Click += ClearPicked_Click;
            //
            // GraphicCutterForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1360, 840);
            Controls.Add(mainLayout);
            Controls.Add(statusStrip1);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MinimumSize = new System.Drawing.Size(1000, 760);
            Name = "GraphicCutterForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Graphic Cutter";
            TopMost = true;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            groupStage.ResumeLayout(false);
            panelStage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            groupResult.ResumeLayout(false);
            panelResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            flowControls.ResumeLayout(false);
            groupMoveImage.ResumeLayout(false);
            groupCut.ResumeLayout(false);
            groupCut.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericY).EndInit();
            groupMoveCut.ResumeLayout(false);
            groupOptions.ResumeLayout(false);
            groupOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericTolerance).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer moveTimer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem openImageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importClipboardMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openIntoResultMenuItem;
        private System.Windows.Forms.ToolStripSeparator fileSep1;
        private System.Windows.Forms.ToolStripMenuItem saveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openOutputFolderMenuItem;
        private System.Windows.Forms.ToolStripSeparator fileSep2;
        private System.Windows.Forms.ToolStripMenuItem unloadStageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unloadResultMenuItem;
        private System.Windows.Forms.ToolStripSeparator fileSep3;
        private System.Windows.Forms.ToolStripMenuItem closeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageMenu;
        private System.Windows.Forms.ToolStripMenuItem mirrorStageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem transparentMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoTrimMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bakeMenuItem;
        private System.Windows.Forms.ToolStripSeparator imageSep1;
        private System.Windows.Forms.ToolStripMenuItem centerImageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMenu;
        private System.Windows.Forms.ToolStripMenuItem gridMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gridColorMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gridResultMenuItem;
        private System.Windows.Forms.ToolStripMenuItem borderMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutFrameMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rulerMenuItem;
        private System.Windows.Forms.ToolStripSeparator viewSep1;
        private System.Windows.Forms.ToolStripMenuItem backgroundMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bgBlackMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bgWhiteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bgColorMenuItem;
        private System.Windows.Forms.ToolStripTextBox bgHexTextBox;
        private System.Windows.Forms.ToolStripSeparator bgSep;
        private System.Windows.Forms.ToolStripMenuItem bgImageNoneMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bgImageGreenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bgImageWaterMenuItem;
        private System.Windows.Forms.ToolStripSeparator viewSep2;
        private System.Windows.Forms.ToolStripMenuItem topMostMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resultMenu;
        private System.Windows.Forms.ToolStripMenuItem mirrorResultMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateResultMenuItem;
        private System.Windows.Forms.ToolStripSeparator resultSep1;
        private System.Windows.Forms.ToolStripMenuItem copyResultMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copySelectionMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fillTextureMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutStripsMenuItem;
        private System.Windows.Forms.ToolStripSeparator resultSep2;
        private System.Windows.Forms.ToolStripMenuItem showMaskMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearSelectionMenuItem;
        private System.Windows.Forms.ToolStripSeparator resultSep3;
        private System.Windows.Forms.ToolStripMenuItem soundMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsOpen;
        private System.Windows.Forms.ToolStripButton tsImport;
        private System.Windows.Forms.ToolStripButton tsSave;
        private System.Windows.Forms.ToolStripComboBox tsFormat;
        private System.Windows.Forms.ToolStripSeparator tsSep1;
        private System.Windows.Forms.ToolStripButton tsCopy;
        private System.Windows.Forms.ToolStripButton tsMirror;
        private System.Windows.Forms.ToolStripButton tsRotate;
        private System.Windows.Forms.ToolStripButton tsCutStrips;
        private System.Windows.Forms.ToolStripButton tsFill;
        private System.Windows.Forms.ToolStripSeparator tsSep2;
        private System.Windows.Forms.ToolStripButton tsGrid;
        private System.Windows.Forms.ToolStripButton tsBorder;
        private System.Windows.Forms.ToolStripButton tsRuler;
        private System.Windows.Forms.ToolStripButton tsMask;
        private System.Windows.Forms.ToolStripSeparator tsSep3;
        private System.Windows.Forms.ToolStripLabel tsBgLabel;
        private System.Windows.Forms.ToolStripComboBox tsBackground;
        private System.Windows.Forms.ToolStripButton tsBackColor;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusPos;
        private System.Windows.Forms.ToolStripStatusLabel statusInfo;
        private System.Windows.Forms.ToolStripStatusLabel statusMessage;
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.GroupBox groupStage;
        private System.Windows.Forms.Panel panelStage;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupResult;
        private System.Windows.Forms.Panel panelResult;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ctxOpen;
        private System.Windows.Forms.ToolStripMenuItem ctxSave;
        private System.Windows.Forms.ToolStripMenuItem ctxSaveAs;
        private System.Windows.Forms.ToolStripMenuItem ctxUnload;
        private System.Windows.Forms.ToolStripSeparator ctxSep1;
        private System.Windows.Forms.ToolStripMenuItem ctxGrid;
        private System.Windows.Forms.ToolStripMenuItem ctxMirror;
        private System.Windows.Forms.ToolStripMenuItem ctxRotate;
        private System.Windows.Forms.ToolStripMenuItem ctxCutStrips;
        private System.Windows.Forms.ToolStripMenuItem ctxFill;
        private System.Windows.Forms.ToolStripMenuItem ctxMask;
        private System.Windows.Forms.ToolStripSeparator ctxSep2;
        private System.Windows.Forms.ToolStripMenuItem ctxCopySelection;
        private System.Windows.Forms.ToolStripMenuItem ctxCopyResult;
        private System.Windows.Forms.FlowLayoutPanel flowControls;
        private System.Windows.Forms.GroupBox groupMoveImage;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.GroupBox groupCut;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.NumericUpDown numericWidth;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.NumericUpDown numericHeight;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.NumericUpDown numericX;
        private System.Windows.Forms.Label labelY;
        private System.Windows.Forms.NumericUpDown numericY;
        private System.Windows.Forms.Label labelPreset;
        private System.Windows.Forms.ComboBox comboPreset;
        private System.Windows.Forms.CheckBox checkBoxFollow;
        private System.Windows.Forms.GroupBox groupMoveCut;
        private System.Windows.Forms.Button btnCutUp;
        private System.Windows.Forms.Button btnCutLeft;
        private System.Windows.Forms.Button btnCutRight;
        private System.Windows.Forms.Button btnCutDown;
        private System.Windows.Forms.Button buttonQuickSave;
        private System.Windows.Forms.GroupBox groupOptions;
        private System.Windows.Forms.CheckBox checkBoxTransparent;
        private System.Windows.Forms.Label labelTolerance;
        private System.Windows.Forms.NumericUpDown numericTolerance;
        private System.Windows.Forms.Label labelSelection;
        private System.Windows.Forms.RadioButton radioRectangle;
        private System.Windows.Forms.RadioButton radioCircle;
        private System.Windows.Forms.RadioButton radioFreehand;
        private System.Windows.Forms.Label labelZoomTitle;
        private System.Windows.Forms.Button zoomOutButton;
        private System.Windows.Forms.Label labelZoom;
        private System.Windows.Forms.Button zoomButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.CheckBox checkBoxBake;
        private System.Windows.Forms.ToolStripMenuItem saveArtMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editMenu;
        private System.Windows.Forms.ToolStripMenuItem undoMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pipetteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearPickedMenuItem;
        private System.Windows.Forms.ToolStripMenuItem snapMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sidePanelMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bgLandMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCollectionMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpMenu;
        private System.Windows.Forms.ToolStripMenuItem helpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpEnglishMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpGermanMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ctxAddCollection;
        private System.Windows.Forms.ToolStripSeparator editSep1;
        private System.Windows.Forms.ToolStripSeparator tsSepU;
        private System.Windows.Forms.ToolStripSeparator tsSep4;
        private System.Windows.Forms.ToolStripButton tsUndo;
        private System.Windows.Forms.ToolStripButton tsRedo;
        private System.Windows.Forms.ToolStripButton tsAddCollection;
        private System.Windows.Forms.ToolStripButton tsPipette;
        private System.Windows.Forms.ToolStripButton tsPanel;
        private System.Windows.Forms.ToolStripButton tsHelp;
        private System.Windows.Forms.ToolStripLabel tsFillLabel;
        private System.Windows.Forms.ToolStripComboBox tsFillMode;
        private System.Windows.Forms.CheckBox checkBoxSnap;
        private System.Windows.Forms.Label labelPicked;
        private System.Windows.Forms.Button buttonClearPicked;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter.CutterSidePanel sidePanel;
    }
}
