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
    partial class MultiMapControl
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
                OnDisposeImages();
                if (components != null)
                {
                    components.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultiMapControl));
            this.pictureBox = new UoFiddler.Controls.MultiMapImages.ZoomCanvas();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorContext = new System.Windows.Forms.ToolStripSeparator();
            this.fitContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actualSizeContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.multiMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet00ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet01ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet02ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet03ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet04ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet05ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorLibraryStart = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparatorLibraryEnd = new System.Windows.Forms.ToolStripSeparator();
            this.openImageFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addImageFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.multiMapFromImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facetFromImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorGenerate = new System.Windows.Forms.ToolStripSeparator();
            this.multiMapFromCurrentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facetFromCurrentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createImageToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton3 = new System.Windows.Forms.ToolStripDropDownButton();
            this.asBmpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asGifToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorExport = new System.Windows.Forms.ToolStripSeparator();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.copyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.printToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomInToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.zoomOutToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.actualSizeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.fitToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.zoomToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.nameStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.positionStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.sizeStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextMenuStrip.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // pictureBox
            //
            this.pictureBox.AutoScroll = true;
            this.pictureBox.BackColor = System.Drawing.Color.White;
            this.pictureBox.ContextMenuStrip = this.contextMenuStrip;
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 25);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.PanWithLeftButton = true;
            this.pictureBox.Size = new System.Drawing.Size(760, 347);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.ZoomChanged += new System.EventHandler(this.OnZoomChanged);
            this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            //
            // contextMenuStrip
            //
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyContextMenuItem,
            this.editContextMenuItem,
            this.saveAsContextMenuItem,
            this.printContextMenuItem,
            this.toolStripSeparatorContext,
            this.fitContextMenuItem,
            this.actualSizeContextMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(181, 142);
            //
            // copyContextMenuItem
            //
            this.copyContextMenuItem.Name = "copyContextMenuItem";
            this.copyContextMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyContextMenuItem.Text = "Copy";
            this.copyContextMenuItem.Click += new System.EventHandler(this.OnClickCopy);
            //
            // editContextMenuItem
            //
            this.editContextMenuItem.Name = "editContextMenuItem";
            this.editContextMenuItem.Size = new System.Drawing.Size(180, 22);
            this.editContextMenuItem.Text = "Edit...";
            this.editContextMenuItem.Click += new System.EventHandler(this.OnClickEdit);
            //
            // saveAsContextMenuItem
            //
            this.saveAsContextMenuItem.Name = "saveAsContextMenuItem";
            this.saveAsContextMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveAsContextMenuItem.Text = "Save as...";
            this.saveAsContextMenuItem.Click += new System.EventHandler(this.OnClickSaveAs);
            //
            // printContextMenuItem
            //
            this.printContextMenuItem.Name = "printContextMenuItem";
            this.printContextMenuItem.Size = new System.Drawing.Size(180, 22);
            this.printContextMenuItem.Text = "Print...";
            this.printContextMenuItem.Click += new System.EventHandler(this.OnClickPrint);
            //
            // toolStripSeparatorContext
            //
            this.toolStripSeparatorContext.Name = "toolStripSeparatorContext";
            this.toolStripSeparatorContext.Size = new System.Drawing.Size(177, 6);
            //
            // fitContextMenuItem
            //
            this.fitContextMenuItem.Name = "fitContextMenuItem";
            this.fitContextMenuItem.Size = new System.Drawing.Size(180, 22);
            this.fitContextMenuItem.Text = "Fit to window";
            this.fitContextMenuItem.Click += new System.EventHandler(this.OnClickFit);
            //
            // actualSizeContextMenuItem
            //
            this.actualSizeContextMenuItem.Name = "actualSizeContextMenuItem";
            this.actualSizeContextMenuItem.Size = new System.Drawing.Size(180, 22);
            this.actualSizeContextMenuItem.Text = "Actual size (100 %)";
            this.actualSizeContextMenuItem.Click += new System.EventHandler(this.OnClickActualSize);
            //
            // toolStrip1
            //
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripDropDownButton2,
            this.createImageToolStripButton,
            this.toolStripDropDownButton3,
            this.editToolStripButton,
            this.copyToolStripButton,
            this.printToolStripDropDownButton,
            this.zoomToolStripLabel,
            this.fitToolStripButton,
            this.actualSizeToolStripButton,
            this.zoomOutToolStripButton,
            this.zoomInToolStripButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(760, 25);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            //
            // toolStripDropDownButton1
            //
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.multiMapToolStripMenuItem,
            this.facet00ToolStripMenuItem,
            this.facet01ToolStripMenuItem,
            this.facet02ToolStripMenuItem,
            this.facet03ToolStripMenuItem,
            this.facet04ToolStripMenuItem,
            this.facet05ToolStripMenuItem,
            this.toolStripSeparatorLibraryStart,
            this.toolStripSeparatorLibraryEnd,
            this.openImageFileToolStripMenuItem,
            this.addImageFileToolStripMenuItem,
            this.fromClipboardToolStripMenuItem,
            this.manageListToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(52, 22);
            this.toolStripDropDownButton1.Text = "Load..";
            //
            // multiMapToolStripMenuItem
            //
            this.multiMapToolStripMenuItem.Name = "multiMapToolStripMenuItem";
            this.multiMapToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.multiMapToolStripMenuItem.Text = "MultiMap";
            this.multiMapToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            //
            // facet00ToolStripMenuItem
            //
            this.facet00ToolStripMenuItem.Name = "facet00ToolStripMenuItem";
            this.facet00ToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.facet00ToolStripMenuItem.Text = "Facet00";
            this.facet00ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            //
            // facet01ToolStripMenuItem
            //
            this.facet01ToolStripMenuItem.Name = "facet01ToolStripMenuItem";
            this.facet01ToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.facet01ToolStripMenuItem.Text = "Facet01";
            this.facet01ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            //
            // facet02ToolStripMenuItem
            //
            this.facet02ToolStripMenuItem.Name = "facet02ToolStripMenuItem";
            this.facet02ToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.facet02ToolStripMenuItem.Text = "Facet02";
            this.facet02ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            //
            // facet03ToolStripMenuItem
            //
            this.facet03ToolStripMenuItem.Name = "facet03ToolStripMenuItem";
            this.facet03ToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.facet03ToolStripMenuItem.Text = "Facet03";
            this.facet03ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            //
            // facet04ToolStripMenuItem
            //
            this.facet04ToolStripMenuItem.Name = "facet04ToolStripMenuItem";
            this.facet04ToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.facet04ToolStripMenuItem.Text = "Facet04";
            this.facet04ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            //
            // facet05ToolStripMenuItem
            //
            this.facet05ToolStripMenuItem.Name = "facet05ToolStripMenuItem";
            this.facet05ToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.facet05ToolStripMenuItem.Text = "Facet05";
            this.facet05ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            //
            // toolStripSeparatorLibraryStart
            //
            this.toolStripSeparatorLibraryStart.Name = "toolStripSeparatorLibraryStart";
            this.toolStripSeparatorLibraryStart.Size = new System.Drawing.Size(227, 6);
            //
            // toolStripSeparatorLibraryEnd
            //
            this.toolStripSeparatorLibraryEnd.Name = "toolStripSeparatorLibraryEnd";
            this.toolStripSeparatorLibraryEnd.Size = new System.Drawing.Size(227, 6);
            //
            // openImageFileToolStripMenuItem
            //
            this.openImageFileToolStripMenuItem.Name = "openImageFileToolStripMenuItem";
            this.openImageFileToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.openImageFileToolStripMenuItem.Text = "Open image file...";
            this.openImageFileToolStripMenuItem.Click += new System.EventHandler(this.OnClickOpenImageFile);
            //
            // addImageFileToolStripMenuItem
            //
            this.addImageFileToolStripMenuItem.Name = "addImageFileToolStripMenuItem";
            this.addImageFileToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.addImageFileToolStripMenuItem.Text = "Add image file to list...";
            this.addImageFileToolStripMenuItem.Click += new System.EventHandler(this.OnClickAddImageFile);
            //
            // fromClipboardToolStripMenuItem
            //
            this.fromClipboardToolStripMenuItem.Name = "fromClipboardToolStripMenuItem";
            this.fromClipboardToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.fromClipboardToolStripMenuItem.Text = "From clipboard";
            this.fromClipboardToolStripMenuItem.Click += new System.EventHandler(this.OnClickFromClipboard);
            //
            // manageListToolStripMenuItem
            //
            this.manageListToolStripMenuItem.Name = "manageListToolStripMenuItem";
            this.manageListToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.manageListToolStripMenuItem.Text = "Manage list...";
            this.manageListToolStripMenuItem.Click += new System.EventHandler(this.OnClickManageList);
            //
            // toolStripDropDownButton2
            //
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.multiMapFromImageToolStripMenuItem,
            this.facetFromImageToolStripMenuItem,
            this.toolStripSeparatorGenerate,
            this.multiMapFromCurrentToolStripMenuItem,
            this.facetFromCurrentToolStripMenuItem});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(73, 22);
            this.toolStripDropDownButton2.Text = "Generate..";
            //
            // multiMapFromImageToolStripMenuItem
            //
            this.multiMapFromImageToolStripMenuItem.Name = "multiMapFromImageToolStripMenuItem";
            this.multiMapFromImageToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.multiMapFromImageToolStripMenuItem.Text = "MultiMap from Image";
            this.multiMapFromImageToolStripMenuItem.Click += new System.EventHandler(this.OnClickGenerateRLE);
            //
            // facetFromImageToolStripMenuItem
            //
            this.facetFromImageToolStripMenuItem.Name = "facetFromImageToolStripMenuItem";
            this.facetFromImageToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.facetFromImageToolStripMenuItem.Text = "Facet from Image";
            this.facetFromImageToolStripMenuItem.Click += new System.EventHandler(this.OnClickGenerateFacetFromImage);
            //
            // toolStripSeparatorGenerate
            //
            this.toolStripSeparatorGenerate.Name = "toolStripSeparatorGenerate";
            this.toolStripSeparatorGenerate.Size = new System.Drawing.Size(247, 6);
            //
            // multiMapFromCurrentToolStripMenuItem
            //
            this.multiMapFromCurrentToolStripMenuItem.Name = "multiMapFromCurrentToolStripMenuItem";
            this.multiMapFromCurrentToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.multiMapFromCurrentToolStripMenuItem.Text = "MultiMap from current image";
            this.multiMapFromCurrentToolStripMenuItem.Click += new System.EventHandler(this.OnClickMultiMapFromCurrent);
            //
            // facetFromCurrentToolStripMenuItem
            //
            this.facetFromCurrentToolStripMenuItem.Name = "facetFromCurrentToolStripMenuItem";
            this.facetFromCurrentToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.facetFromCurrentToolStripMenuItem.Text = "Facet from current image";
            this.facetFromCurrentToolStripMenuItem.Click += new System.EventHandler(this.OnClickFacetFromCurrent);
            //
            // createImageToolStripButton
            //
            this.createImageToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.createImageToolStripButton.Name = "createImageToolStripButton";
            this.createImageToolStripButton.Size = new System.Drawing.Size(85, 22);
            this.createImageToolStripButton.Text = "Create image...";
            this.createImageToolStripButton.ToolTipText = "Create an image of a map (built-in, map list, external map files) or of an image file";
            this.createImageToolStripButton.Click += new System.EventHandler(this.OnClickCreateImage);
            //
            // toolStripDropDownButton3
            //
            this.toolStripDropDownButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem1,
            this.asTiffToolStripMenuItem1,
            this.asPngToolStripMenuItem,
            this.asPngToolStripMenuItem1,
            this.asGifToolStripMenuItem,
            this.toolStripSeparatorExport,
            this.saveAsToolStripMenuItem,
            this.saveToListToolStripMenuItem});
            this.toolStripDropDownButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton3.Image")));
            this.toolStripDropDownButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton3.Name = "toolStripDropDownButton3";
            this.toolStripDropDownButton3.Size = new System.Drawing.Size(60, 22);
            this.toolStripDropDownButton3.Text = "Export..";
            //
            // asBmpToolStripMenuItem1
            //
            this.asBmpToolStripMenuItem1.Name = "asBmpToolStripMenuItem1";
            this.asBmpToolStripMenuItem1.Size = new System.Drawing.Size(230, 22);
            this.asBmpToolStripMenuItem1.Text = "As Bmp..";
            this.asBmpToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExportBmp);
            //
            // asTiffToolStripMenuItem1
            //
            this.asTiffToolStripMenuItem1.Name = "asTiffToolStripMenuItem1";
            this.asTiffToolStripMenuItem1.Size = new System.Drawing.Size(230, 22);
            this.asTiffToolStripMenuItem1.Text = "As Tiff..";
            this.asTiffToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExportTiff);
            //
            // asPngToolStripMenuItem
            //
            this.asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            this.asPngToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.asPngToolStripMenuItem.Text = "As Jpg..";
            this.asPngToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportJpg);
            //
            // asPngToolStripMenuItem1
            //
            this.asPngToolStripMenuItem1.Name = "asPngToolStripMenuItem1";
            this.asPngToolStripMenuItem1.Size = new System.Drawing.Size(230, 22);
            this.asPngToolStripMenuItem1.Text = "As Png..";
            this.asPngToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExportPng);
            //
            // asGifToolStripMenuItem
            //
            this.asGifToolStripMenuItem.Name = "asGifToolStripMenuItem";
            this.asGifToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.asGifToolStripMenuItem.Text = "As Gif..";
            this.asGifToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportGif);
            //
            // toolStripSeparatorExport
            //
            this.toolStripSeparatorExport.Name = "toolStripSeparatorExport";
            this.toolStripSeparatorExport.Size = new System.Drawing.Size(227, 6);
            //
            // saveAsToolStripMenuItem
            //
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.saveAsToolStripMenuItem.Text = "Save as... (folder / format)";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.OnClickSaveAs);
            //
            // saveToListToolStripMenuItem
            //
            this.saveToListToolStripMenuItem.Name = "saveToListToolStripMenuItem";
            this.saveToListToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.saveToListToolStripMenuItem.Text = "Add to \"Load..\" list";
            this.saveToListToolStripMenuItem.Click += new System.EventHandler(this.OnClickSaveToList);
            //
            // editToolStripButton
            //
            this.editToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.editToolStripButton.Name = "editToolStripButton";
            this.editToolStripButton.Size = new System.Drawing.Size(39, 22);
            this.editToolStripButton.Text = "Edit...";
            this.editToolStripButton.ToolTipText = "Draw and write on the image";
            this.editToolStripButton.Click += new System.EventHandler(this.OnClickEdit);
            //
            // copyToolStripButton
            //
            this.copyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.copyToolStripButton.Name = "copyToolStripButton";
            this.copyToolStripButton.Size = new System.Drawing.Size(39, 22);
            this.copyToolStripButton.Text = "Copy";
            this.copyToolStripButton.ToolTipText = "Copy the image to the clipboard";
            this.copyToolStripButton.Click += new System.EventHandler(this.OnClickCopy);
            //
            // printToolStripDropDownButton
            //
            this.printToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.printToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.printToolStripMenuItem,
            this.printPreviewToolStripMenuItem});
            this.printToolStripDropDownButton.Name = "printToolStripDropDownButton";
            this.printToolStripDropDownButton.Size = new System.Drawing.Size(46, 22);
            this.printToolStripDropDownButton.Text = "Print..";
            //
            // printToolStripMenuItem
            //
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.printToolStripMenuItem.Text = "Print...";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.OnClickPrint);
            //
            // printPreviewToolStripMenuItem
            //
            this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            this.printPreviewToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.printPreviewToolStripMenuItem.Text = "Print preview...";
            this.printPreviewToolStripMenuItem.Click += new System.EventHandler(this.OnClickPrintPreview);
            //
            // zoomToolStripLabel
            //
            this.zoomToolStripLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.zoomToolStripLabel.Name = "zoomToolStripLabel";
            this.zoomToolStripLabel.Size = new System.Drawing.Size(41, 22);
            this.zoomToolStripLabel.Text = "100 %";
            //
            // fitToolStripButton
            //
            this.fitToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.fitToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.fitToolStripButton.Name = "fitToolStripButton";
            this.fitToolStripButton.Size = new System.Drawing.Size(24, 22);
            this.fitToolStripButton.Text = "Fit";
            this.fitToolStripButton.ToolTipText = "Fit to window";
            this.fitToolStripButton.Click += new System.EventHandler(this.OnClickFit);
            //
            // actualSizeToolStripButton
            //
            this.actualSizeToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.actualSizeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.actualSizeToolStripButton.Name = "actualSizeToolStripButton";
            this.actualSizeToolStripButton.Size = new System.Drawing.Size(27, 22);
            this.actualSizeToolStripButton.Text = "1:1";
            this.actualSizeToolStripButton.ToolTipText = "Actual size (100 %)";
            this.actualSizeToolStripButton.Click += new System.EventHandler(this.OnClickActualSize);
            //
            // zoomOutToolStripButton
            //
            this.zoomOutToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.zoomOutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.zoomOutToolStripButton.Name = "zoomOutToolStripButton";
            this.zoomOutToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.zoomOutToolStripButton.Text = "-";
            this.zoomOutToolStripButton.ToolTipText = "Zoom out (mouse wheel)";
            this.zoomOutToolStripButton.Click += new System.EventHandler(this.OnClickZoomOut);
            //
            // zoomInToolStripButton
            //
            this.zoomInToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.zoomInToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.zoomInToolStripButton.Name = "zoomInToolStripButton";
            this.zoomInToolStripButton.Size = new System.Drawing.Size(23, 22);
            this.zoomInToolStripButton.Text = "+";
            this.zoomInToolStripButton.ToolTipText = "Zoom in (mouse wheel)";
            this.zoomInToolStripButton.Click += new System.EventHandler(this.OnClickZoomIn);
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nameStatusLabel,
            this.positionStatusLabel,
            this.sizeStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 372);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(760, 22);
            this.statusStrip.TabIndex = 6;
            //
            // nameStatusLabel
            //
            this.nameStatusLabel.Name = "nameStatusLabel";
            this.nameStatusLabel.Size = new System.Drawing.Size(545, 17);
            this.nameStatusLabel.Spring = true;
            this.nameStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // positionStatusLabel
            //
            this.positionStatusLabel.AutoSize = false;
            this.positionStatusLabel.Name = "positionStatusLabel";
            this.positionStatusLabel.Size = new System.Drawing.Size(90, 17);
            //
            // sizeStatusLabel
            //
            this.sizeStatusLabel.AutoSize = false;
            this.sizeStatusLabel.Name = "sizeStatusLabel";
            this.sizeStatusLabel.Size = new System.Drawing.Size(110, 17);
            //
            // MultiMapControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStrip1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MultiMapControl";
            this.Size = new System.Drawing.Size(760, 394);
            this.Load += new System.EventHandler(this.OnLoad);
            this.contextMenuStrip.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asGifToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorExport;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet00ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet01ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet02ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet03ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet04ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet05ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facetFromImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem multiMapFromImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorGenerate;
        private System.Windows.Forms.ToolStripMenuItem multiMapFromCurrentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facetFromCurrentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem multiMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorLibraryStart;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorLibraryEnd;
        private System.Windows.Forms.ToolStripMenuItem openImageFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addImageFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageListToolStripMenuItem;
        private UoFiddler.Controls.MultiMapImages.ZoomCanvas pictureBox;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton3;
        private System.Windows.Forms.ToolStripButton createImageToolStripButton;
        private System.Windows.Forms.ToolStripButton editToolStripButton;
        private System.Windows.Forms.ToolStripButton copyToolStripButton;
        private System.Windows.Forms.ToolStripDropDownButton printToolStripDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton zoomInToolStripButton;
        private System.Windows.Forms.ToolStripButton zoomOutToolStripButton;
        private System.Windows.Forms.ToolStripButton actualSizeToolStripButton;
        private System.Windows.Forms.ToolStripButton fitToolStripButton;
        private System.Windows.Forms.ToolStripLabel zoomToolStripLabel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel nameStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel positionStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel sizeStatusLabel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem copyContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printContextMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorContext;
        private System.Windows.Forms.ToolStripMenuItem fitContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actualSizeContextMenuItem;
    }
}
