// /***************************************************************************
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
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            openImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            colorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripTextBoxColors = new System.Windows.Forms.ToolStripTextBox();
            PaintPictureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            spiegelnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toggleGridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            showBorderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            unloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            imageColorClearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            customsRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripComboBox2 = new System.Windows.Forms.ToolStripComboBox();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            btnUp = new System.Windows.Forms.Button();
            btnDown = new System.Windows.Forms.Button();
            btnLeft = new System.Windows.Forms.Button();
            btnRight = new System.Windows.Forms.Button();
            moveTimer = new System.Windows.Forms.Timer(components);
            label1 = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            textBoxWidth = new System.Windows.Forms.TextBox();
            textBoxHeight = new System.Windows.Forms.TextBox();
            textBoxStartX = new System.Windows.Forms.TextBox();
            textBoxStartY = new System.Windows.Forms.TextBox();
            lbWidth = new System.Windows.Forms.Label();
            lbHeight = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            openToolStrip2MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            unloadimageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            gridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripComboBox3 = new System.Windows.Forms.ToolStripComboBox();
            mirror2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItemrotate90degrees = new System.Windows.Forms.ToolStripMenuItem();
            cutImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fillTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            showMaskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            dischargedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            CopyHighlightedAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panel2 = new System.Windows.Forms.Panel();
            button5 = new System.Windows.Forms.Button();
            checkBoxTransparent = new System.Windows.Forms.CheckBox();
            checkBoxCircle = new System.Windows.Forms.CheckBox();
            checkBoxFreehand = new System.Windows.Forms.CheckBox();
            resetButton = new System.Windows.Forms.Button();
            zoomButton = new System.Windows.Forms.Button();
            pictureBox2 = new System.Windows.Forms.PictureBox();
            panel3 = new System.Windows.Forms.Panel();
            checkBoxOrgSize = new System.Windows.Forms.CheckBox();
            panel4 = new System.Windows.Forms.Panel();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel3.SuspendLayout();
            panel4.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripDropDownButton1, toolStripComboBox2 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(879, 25);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openImageToolStripMenuItem, importToolStripMenuItem, toolStripSeparator1, colorToolStripMenuItem, PaintPictureToolStripMenuItem, toolStripSeparator2, spiegelnToolStripMenuItem, toolStripSeparator3, toggleGridToolStripMenuItem, showBorderToolStripMenuItem, unloadToolStripMenuItem, imageColorClearToolStripMenuItem, customsRuleToolStripMenuItem });
            toolStripDropDownButton1.Image = Properties.Resources.Shild;
            toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new System.Drawing.Size(29, 22);
            toolStripDropDownButton1.Text = "toolStripDropDownButton1";
            // 
            // openImageToolStripMenuItem
            // 
            openImageToolStripMenuItem.Image = Properties.Resources.Load;
            openImageToolStripMenuItem.Name = "openImageToolStripMenuItem";
            openImageToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            openImageToolStripMenuItem.Text = "Open Image";
            openImageToolStripMenuItem.Click += openImageToolStripMenuItem_Click;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Image = Properties.Resources.import;
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            importToolStripMenuItem.Text = "Import Clipbord";
            importToolStripMenuItem.ToolTipText = "Import Image from Clipboard";
            importToolStripMenuItem.Click += importToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(173, 6);
            // 
            // colorToolStripMenuItem
            // 
            colorToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripTextBoxColors });
            colorToolStripMenuItem.Image = Properties.Resources.Color;
            colorToolStripMenuItem.Name = "colorToolStripMenuItem";
            colorToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            colorToolStripMenuItem.Text = "BackGrundColor";
            colorToolStripMenuItem.Click += colorToolStripMenuItem_Click;
            // 
            // toolStripTextBoxColors
            // 
            toolStripTextBoxColors.Name = "toolStripTextBoxColors";
            toolStripTextBoxColors.Size = new System.Drawing.Size(100, 23);
            toolStripTextBoxColors.KeyDown += toolStripTextBoxColors_KeyDown;
            // 
            // PaintPictureToolStripMenuItem
            // 
            PaintPictureToolStripMenuItem.Image = Properties.Resources.createDesign;
            PaintPictureToolStripMenuItem.Name = "PaintPictureToolStripMenuItem";
            PaintPictureToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            PaintPictureToolStripMenuItem.Text = "Grafic Paint";
            PaintPictureToolStripMenuItem.Click += PaintGridToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(173, 6);
            // 
            // spiegelnToolStripMenuItem
            // 
            spiegelnToolStripMenuItem.Image = Properties.Resources.Mirror1;
            spiegelnToolStripMenuItem.Name = "spiegelnToolStripMenuItem";
            spiegelnToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            spiegelnToolStripMenuItem.Text = "Mirror";
            spiegelnToolStripMenuItem.Click += spiegelnToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(173, 6);
            // 
            // toggleGridToolStripMenuItem
            // 
            toggleGridToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem1 });
            toggleGridToolStripMenuItem.Image = Properties.Resources.Schablone;
            toggleGridToolStripMenuItem.Name = "toggleGridToolStripMenuItem";
            toggleGridToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            toggleGridToolStripMenuItem.Text = "ToggleGrid";
            toggleGridToolStripMenuItem.Click += toggleGridToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(103, 22);
            toolStripMenuItem1.Text = "Color";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click_1;
            // 
            // showBorderToolStripMenuItem
            // 
            showBorderToolStripMenuItem.Image = Properties.Resources.Rahmen;
            showBorderToolStripMenuItem.Name = "showBorderToolStripMenuItem";
            showBorderToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            showBorderToolStripMenuItem.Text = "Show Border";
            showBorderToolStripMenuItem.Click += showBorderToolStripMenuItem_Click;
            // 
            // unloadToolStripMenuItem
            // 
            unloadToolStripMenuItem.Image = Properties.Resources.Leeren;
            unloadToolStripMenuItem.Name = "unloadToolStripMenuItem";
            unloadToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            unloadToolStripMenuItem.Text = "unloadImage";
            unloadToolStripMenuItem.Click += unloadToolStripMenuItem_Click;
            // 
            // imageColorClearToolStripMenuItem
            // 
            imageColorClearToolStripMenuItem.Image = Properties.Resources.Border;
            imageColorClearToolStripMenuItem.Name = "imageColorClearToolStripMenuItem";
            imageColorClearToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            imageColorClearToolStripMenuItem.Text = "Change the border.";
            imageColorClearToolStripMenuItem.Click += imageColorClearToolStripMenuItem_Click;
            // 
            // customsRuleToolStripMenuItem
            // 
            customsRuleToolStripMenuItem.Image = Properties.Resources.ruler;
            customsRuleToolStripMenuItem.Name = "customsRuleToolStripMenuItem";
            customsRuleToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            customsRuleToolStripMenuItem.Text = "Ruler";
            customsRuleToolStripMenuItem.Click += customsRuleToolStripMenuItem_Click;
            // 
            // toolStripComboBox2
            // 
            toolStripComboBox2.Items.AddRange(new object[] { "green", "water", "clear" });
            toolStripComboBox2.Name = "toolStripComboBox2";
            toolStripComboBox2.Size = new System.Drawing.Size(121, 25);
            toolStripComboBox2.Click += toolStripComboBox2_SelectedIndexChanged;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = System.Drawing.Color.Black;
            pictureBox1.Enabled = false;
            pictureBox1.ErrorImage = null;
            pictureBox1.InitialImage = null;
            pictureBox1.Location = new System.Drawing.Point(36, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(352, 352);
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += pictureBox1_Paint;
            // 
            // btnUp
            // 
            btnUp.BackColor = System.Drawing.SystemColors.ActiveBorder;
            btnUp.Image = Properties.Resources.up;
            btnUp.Location = new System.Drawing.Point(58, 3);
            btnUp.Name = "btnUp";
            btnUp.Size = new System.Drawing.Size(49, 46);
            btnUp.TabIndex = 4;
            btnUp.UseVisualStyleBackColor = false;
            btnUp.Click += btnUp_Click;
            btnUp.MouseDown += btnUp_MouseDown;
            btnUp.MouseUp += btnUp_MouseUp;
            // 
            // btnDown
            // 
            btnDown.BackColor = System.Drawing.SystemColors.ActiveBorder;
            btnDown.BackgroundImage = Properties.Resources.down;
            btnDown.Location = new System.Drawing.Point(58, 64);
            btnDown.Name = "btnDown";
            btnDown.Size = new System.Drawing.Size(49, 43);
            btnDown.TabIndex = 5;
            btnDown.UseVisualStyleBackColor = false;
            btnDown.Click += btnDown_Click;
            btnDown.MouseDown += btnDown_MouseDown;
            btnDown.MouseUp += btnDown_MouseUp;
            // 
            // btnLeft
            // 
            btnLeft.BackColor = System.Drawing.SystemColors.ActiveBorder;
            btnLeft.BackgroundImage = Properties.Resources.left;
            btnLeft.Location = new System.Drawing.Point(1, 30);
            btnLeft.Name = "btnLeft";
            btnLeft.Size = new System.Drawing.Size(50, 49);
            btnLeft.TabIndex = 6;
            btnLeft.UseVisualStyleBackColor = false;
            btnLeft.Click += btnLeft_Click;
            btnLeft.MouseDown += btnLeft_MouseDown;
            btnLeft.MouseUp += btnLeft_MouseUp;
            // 
            // btnRight
            // 
            btnRight.BackColor = System.Drawing.SystemColors.ActiveBorder;
            btnRight.BackgroundImage = Properties.Resources.right;
            btnRight.Location = new System.Drawing.Point(113, 30);
            btnRight.Name = "btnRight";
            btnRight.Size = new System.Drawing.Size(48, 49);
            btnRight.TabIndex = 7;
            btnRight.UseVisualStyleBackColor = false;
            btnRight.Click += btnRight_Click;
            btnRight.MouseDown += btnRight_MouseDown;
            btnRight.MouseUp += btnRight_MouseUp;
            // 
            // moveTimer
            // 
            moveTimer.Interval = 50;
            moveTimer.Tick += moveTimer_Tick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(11, 465);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(69, 15);
            label1.TabIndex = 9;
            label1.Text = "coordinates";
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            panel1.Controls.Add(btnDown);
            panel1.Controls.Add(btnUp);
            panel1.Controls.Add(btnLeft);
            panel1.Controls.Add(btnRight);
            panel1.Location = new System.Drawing.Point(197, 468);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(166, 113);
            panel1.TabIndex = 10;
            // 
            // textBoxWidth
            // 
            textBoxWidth.Location = new System.Drawing.Point(47, 510);
            textBoxWidth.Name = "textBoxWidth";
            textBoxWidth.Size = new System.Drawing.Size(52, 23);
            textBoxWidth.TabIndex = 12;
            textBoxWidth.Text = "44";
            textBoxWidth.TextChanged += textBoxWidth_TextChanged;
            textBoxWidth.KeyPress += textBox_KeyPress;
            // 
            // textBoxHeight
            // 
            textBoxHeight.Location = new System.Drawing.Point(135, 510);
            textBoxHeight.Name = "textBoxHeight";
            textBoxHeight.Size = new System.Drawing.Size(52, 23);
            textBoxHeight.TabIndex = 13;
            textBoxHeight.Text = "105";
            textBoxHeight.TextChanged += textBoxHeight_TextChanged;
            textBoxHeight.KeyPress += textBox_KeyPress;
            // 
            // textBoxStartX
            // 
            textBoxStartX.Location = new System.Drawing.Point(47, 547);
            textBoxStartX.Name = "textBoxStartX";
            textBoxStartX.Size = new System.Drawing.Size(52, 23);
            textBoxStartX.TabIndex = 14;
            textBoxStartX.Text = "174";
            textBoxStartX.TextChanged += textBoxStartX_TextChanged;
            textBoxStartX.KeyPress += textBox_KeyPress;
            // 
            // textBoxStartY
            // 
            textBoxStartY.Location = new System.Drawing.Point(135, 547);
            textBoxStartY.Name = "textBoxStartY";
            textBoxStartY.Size = new System.Drawing.Size(52, 23);
            textBoxStartY.TabIndex = 15;
            textBoxStartY.Text = "247";
            textBoxStartY.TextChanged += textBoxStartY_TextChanged;
            textBoxStartY.KeyPress += textBox_KeyPress;
            // 
            // lbWidth
            // 
            lbWidth.AutoSize = true;
            lbWidth.Location = new System.Drawing.Point(48, 490);
            lbWidth.Name = "lbWidth";
            lbWidth.Size = new System.Drawing.Size(45, 15);
            lbWidth.TabIndex = 16;
            lbWidth.Text = "Width :";
            // 
            // lbHeight
            // 
            lbHeight.AutoSize = true;
            lbHeight.Location = new System.Drawing.Point(135, 490);
            lbHeight.Name = "lbHeight";
            lbHeight.Size = new System.Drawing.Size(49, 15);
            lbHeight.TabIndex = 17;
            lbHeight.Text = "Height :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(21, 550);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(20, 15);
            label2.TabIndex = 18;
            label2.Text = "X :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(112, 550);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(17, 15);
            label3.TabIndex = 19;
            label3.Text = "Y:";
            // 
            // button1
            // 
            button1.BackgroundImage = Properties.Resources.up1;
            button1.Location = new System.Drawing.Point(57, 5);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(49, 46);
            button1.TabIndex = 20;
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.BackgroundImage = Properties.Resources.down;
            button2.Location = new System.Drawing.Point(57, 57);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(49, 46);
            button2.TabIndex = 21;
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.BackgroundImage = Properties.Resources.left;
            button3.Location = new System.Drawing.Point(6, 57);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(49, 46);
            button3.TabIndex = 22;
            button3.UseVisualStyleBackColor = true;
            button3.Click += button4_Click;
            // 
            // button4
            // 
            button4.BackgroundImage = Properties.Resources.right;
            button4.Location = new System.Drawing.Point(107, 57);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(51, 46);
            button4.TabIndex = 23;
            button4.UseVisualStyleBackColor = true;
            button4.Click += button3_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { openToolStrip2MenuItem, saveImageToolStripMenuItem, unloadimageToolStripMenuItem, toolStripSeparator4, gridToolStripMenuItem, mirror2ToolStripMenuItem, toolStripMenuItemrotate90degrees, cutImageToolStripMenuItem, fillTextureToolStripMenuItem, showMaskToolStripMenuItem, toolStripSeparator5, CopyHighlightedAreaToolStripMenuItem, copyToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(196, 258);
            // 
            // openToolStrip2MenuItem
            // 
            openToolStrip2MenuItem.Image = Properties.Resources.Load;
            openToolStrip2MenuItem.Name = "openToolStrip2MenuItem";
            openToolStrip2MenuItem.Size = new System.Drawing.Size(195, 22);
            openToolStrip2MenuItem.Text = "Open";
            openToolStrip2MenuItem.Click += openToolStrip2MenuItem_Click;
            // 
            // saveImageToolStripMenuItem
            // 
            saveImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripComboBox1 });
            saveImageToolStripMenuItem.Image = Properties.Resources.Save;
            saveImageToolStripMenuItem.Name = "saveImageToolStripMenuItem";
            saveImageToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            saveImageToolStripMenuItem.Text = "Save Image";
            saveImageToolStripMenuItem.Click += saveImageToolStripMenuItem_Click;
            // 
            // toolStripComboBox1
            // 
            toolStripComboBox1.AccessibleName = "";
            toolStripComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBox1.Items.AddRange(new object[] { "BMP", "TIFF", "PNG", "JPEG" });
            toolStripComboBox1.Name = "toolStripComboBox1";
            toolStripComboBox1.Size = new System.Drawing.Size(121, 23);
            toolStripComboBox1.Click += saveImageToolStripMenuItem_Click;
            // 
            // unloadimageToolStripMenuItem
            // 
            unloadimageToolStripMenuItem.Image = Properties.Resources.Leeren;
            unloadimageToolStripMenuItem.Name = "unloadimageToolStripMenuItem";
            unloadimageToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            unloadimageToolStripMenuItem.Text = "Unload Image";
            unloadimageToolStripMenuItem.Click += unloadimageToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(192, 6);
            // 
            // gridToolStripMenuItem
            // 
            gridToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripComboBox3 });
            gridToolStripMenuItem.Image = Properties.Resources.Schablone;
            gridToolStripMenuItem.Name = "gridToolStripMenuItem";
            gridToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            gridToolStripMenuItem.Text = "Grid";
            gridToolStripMenuItem.Click += gridToolStripMenuItem_Click;
            gridToolStripMenuItem.Paint += pictureBox3_Paint;
            // 
            // toolStripComboBox3
            // 
            toolStripComboBox3.Name = "toolStripComboBox3";
            toolStripComboBox3.Size = new System.Drawing.Size(121, 23);
            toolStripComboBox3.Text = "Color";
            toolStripComboBox3.Click += toolStripComboBox3_Click;
            // 
            // mirror2ToolStripMenuItem
            // 
            mirror2ToolStripMenuItem.Image = Properties.Resources.Mirror1;
            mirror2ToolStripMenuItem.Name = "mirror2ToolStripMenuItem";
            mirror2ToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            mirror2ToolStripMenuItem.Text = "Mirror";
            mirror2ToolStripMenuItem.Click += mirror2ToolStripMenuItem_Click_1;
            // 
            // toolStripMenuItemrotate90degrees
            // 
            toolStripMenuItemrotate90degrees.Image = Properties.Resources.Rotate;
            toolStripMenuItemrotate90degrees.Name = "toolStripMenuItemrotate90degrees";
            toolStripMenuItemrotate90degrees.Size = new System.Drawing.Size(195, 22);
            toolStripMenuItemrotate90degrees.Text = "Rotate 90 degrees";
            toolStripMenuItemrotate90degrees.Click += toolStripMenuItemrotate90degrees_Click;
            // 
            // cutImageToolStripMenuItem
            // 
            cutImageToolStripMenuItem.Image = Properties.Resources.Cut;
            cutImageToolStripMenuItem.Name = "cutImageToolStripMenuItem";
            cutImageToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            cutImageToolStripMenuItem.Text = "Cut Image";
            cutImageToolStripMenuItem.Click += cutImageToolStripMenuItem_Click;
            // 
            // fillTextureToolStripMenuItem
            // 
            fillTextureToolStripMenuItem.Image = Properties.Resources.fill;
            fillTextureToolStripMenuItem.Name = "fillTextureToolStripMenuItem";
            fillTextureToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            fillTextureToolStripMenuItem.Text = "Fill Texture";
            fillTextureToolStripMenuItem.Click += fillTextureToolStripMenuItem_Click;
            // 
            // showMaskToolStripMenuItem
            // 
            showMaskToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { dischargedToolStripMenuItem });
            showMaskToolStripMenuItem.Image = Properties.Resources.image_eine_maske_laden03;
            showMaskToolStripMenuItem.Name = "showMaskToolStripMenuItem";
            showMaskToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            showMaskToolStripMenuItem.Text = "Show Mask";
            showMaskToolStripMenuItem.ToolTipText = "\"Clipping mask for alignment, mask provided by Otimpyre.";
            showMaskToolStripMenuItem.Click += showMaskToolStripMenuItem_Click;
            // 
            // dischargedToolStripMenuItem
            // 
            dischargedToolStripMenuItem.Image = Properties.Resources.image_eine_maske_laden02;
            dischargedToolStripMenuItem.Name = "dischargedToolStripMenuItem";
            dischargedToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            dischargedToolStripMenuItem.Text = "discharged";
            dischargedToolStripMenuItem.Click += dischargedToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(192, 6);
            // 
            // CopyHighlightedAreaToolStripMenuItem
            // 
            CopyHighlightedAreaToolStripMenuItem.Image = Properties.Resources.Copy;
            CopyHighlightedAreaToolStripMenuItem.Name = "CopyHighlightedAreaToolStripMenuItem";
            CopyHighlightedAreaToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            CopyHighlightedAreaToolStripMenuItem.Text = "Copy Highlighted Area";
            CopyHighlightedAreaToolStripMenuItem.ToolTipText = "Copies the selected area to the clipboard.";
            CopyHighlightedAreaToolStripMenuItem.Click += CopyHighlightedAreaToolStripMenuItem_Click;
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Image = Properties.Resources.import;
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            copyToolStripMenuItem.Text = "Copy Clipbord";
            copyToolStripMenuItem.ToolTipText = "Copy Image to Clipbord";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // panel2
            // 
            panel2.BackColor = System.Drawing.SystemColors.ActiveBorder;
            panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            panel2.Controls.Add(button2);
            panel2.Controls.Add(button5);
            panel2.Controls.Add(button1);
            panel2.Controls.Add(button4);
            panel2.Controls.Add(button3);
            panel2.Location = new System.Drawing.Point(389, 468);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(170, 113);
            panel2.TabIndex = 24;
            // 
            // button5
            // 
            button5.Image = Properties.Resources.Save;
            button5.Location = new System.Drawing.Point(107, 5);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(51, 46);
            button5.TabIndex = 25;
            button5.UseVisualStyleBackColor = true;
            button5.Click += buttonCrop_Click;
            // 
            // checkBoxTransparent
            // 
            checkBoxTransparent.AutoSize = true;
            checkBoxTransparent.Checked = true;
            checkBoxTransparent.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxTransparent.Location = new System.Drawing.Point(579, 473);
            checkBoxTransparent.Name = "checkBoxTransparent";
            checkBoxTransparent.Size = new System.Drawing.Size(87, 19);
            checkBoxTransparent.TabIndex = 25;
            checkBoxTransparent.Text = "Transparent";
            checkBoxTransparent.UseVisualStyleBackColor = true;
            // 
            // checkBoxCircle
            // 
            checkBoxCircle.AutoSize = true;
            checkBoxCircle.Location = new System.Drawing.Point(579, 494);
            checkBoxCircle.Name = "checkBoxCircle";
            checkBoxCircle.Size = new System.Drawing.Size(56, 19);
            checkBoxCircle.TabIndex = 26;
            checkBoxCircle.Text = "Circle";
            checkBoxCircle.UseVisualStyleBackColor = true;
            checkBoxCircle.CheckedChanged += UpdateCheckBoxes;
            // 
            // checkBoxFreehand
            // 
            checkBoxFreehand.AutoSize = true;
            checkBoxFreehand.Location = new System.Drawing.Point(579, 516);
            checkBoxFreehand.Name = "checkBoxFreehand";
            checkBoxFreehand.Size = new System.Drawing.Size(75, 19);
            checkBoxFreehand.TabIndex = 27;
            checkBoxFreehand.Text = "Freehand";
            checkBoxFreehand.UseVisualStyleBackColor = true;
            checkBoxFreehand.CheckedChanged += UpdateCheckBoxes;
            // 
            // resetButton
            // 
            resetButton.Location = new System.Drawing.Point(672, 470);
            resetButton.Name = "resetButton";
            resetButton.Size = new System.Drawing.Size(47, 23);
            resetButton.TabIndex = 28;
            resetButton.Text = "Reset";
            resetButton.UseVisualStyleBackColor = true;
            resetButton.Click += resetButton_Click;
            // 
            // zoomButton
            // 
            zoomButton.Location = new System.Drawing.Point(672, 496);
            zoomButton.Name = "zoomButton";
            zoomButton.Size = new System.Drawing.Size(47, 23);
            zoomButton.TabIndex = 29;
            zoomButton.Text = "Zoom";
            zoomButton.UseVisualStyleBackColor = true;
            zoomButton.Click += zoomButton_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = System.Drawing.SystemColors.ControlDark;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pictureBox2.Location = new System.Drawing.Point(20, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(352, 352);
            pictureBox2.TabIndex = 11;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_MouseClick;
            pictureBox2.Paint += pictureBox2_Paint;
            pictureBox2.MouseDown += pictureBox2_MouseDown;
            pictureBox2.MouseMove += pictureBox2_MouseMove;
            pictureBox2.MouseUp += pictureBox2_MouseUp;
            // 
            // panel3
            // 
            panel3.AutoScroll = true;
            panel3.Controls.Add(pictureBox2);
            panel3.Location = new System.Drawing.Point(468, 28);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(399, 365);
            panel3.TabIndex = 30;
            // 
            // checkBoxOrgSize
            // 
            checkBoxOrgSize.AutoSize = true;
            checkBoxOrgSize.Location = new System.Drawing.Point(47, 576);
            checkBoxOrgSize.Name = "checkBoxOrgSize";
            checkBoxOrgSize.Size = new System.Drawing.Size(68, 19);
            checkBoxOrgSize.TabIndex = 31;
            checkBoxOrgSize.Text = "44 x 123";
            checkBoxOrgSize.UseVisualStyleBackColor = true;
            checkBoxOrgSize.CheckedChanged += checkBoxOrgSize_CheckedChanged;
            // 
            // panel4
            // 
            panel4.Controls.Add(pictureBox1);
            panel4.Location = new System.Drawing.Point(11, 28);
            panel4.Name = "panel4";
            panel4.Size = new System.Drawing.Size(425, 411);
            panel4.TabIndex = 32;
            panel4.Paint += panel4_Paint;
            // 
            // GraphicCutterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(879, 627);
            Controls.Add(panel4);
            Controls.Add(checkBoxOrgSize);
            Controls.Add(panel3);
            Controls.Add(zoomButton);
            Controls.Add(resetButton);
            Controls.Add(checkBoxFreehand);
            Controls.Add(checkBoxCircle);
            Controls.Add(checkBoxTransparent);
            Controls.Add(panel2);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(lbHeight);
            Controls.Add(lbWidth);
            Controls.Add(textBoxStartY);
            Controls.Add(textBoxStartX);
            Controls.Add(textBoxHeight);
            Controls.Add(textBoxWidth);
            Controls.Add(panel1);
            Controls.Add(label1);
            Controls.Add(toolStrip1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GraphicCutterForm";
            Text = "Graphic Cutter";
            TopMost = true;
            KeyDown += Form1_KeyDown;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel1.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel3.ResumeLayout(false);
            panel4.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem PaintPictureToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem openImageToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Timer moveTimer;
        private System.Windows.Forms.ToolStripMenuItem colorToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxColors;
        private System.Windows.Forms.ToolStripMenuItem toggleGridToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBoxWidth;
        private System.Windows.Forms.TextBox textBoxHeight;
        private System.Windows.Forms.TextBox textBoxStartX;
        private System.Windows.Forms.TextBox textBoxStartY;
        private System.Windows.Forms.ToolStripMenuItem showBorderToolStripMenuItem;
        private System.Windows.Forms.Label lbWidth;
        private System.Windows.Forms.Label lbHeight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem spiegelnToolStripMenuItem;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ToolStripMenuItem openToolStrip2MenuItem;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.ToolStripMenuItem gridToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mirror2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemrotate90degrees;
        private System.Windows.Forms.ToolStripMenuItem cutImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fillTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unloadimageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox2;
        private System.Windows.Forms.ToolStripMenuItem imageColorClearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox3;
        private System.Windows.Forms.CheckBox checkBoxTransparent;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxCircle;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.CheckBox checkBoxFreehand;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button zoomButton;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ToolStripMenuItem showMaskToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dischargedToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxOrgSize;
        private System.Windows.Forms.ToolStripMenuItem CopyHighlightedAreaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem customsRuleToolStripMenuItem;
        private System.Windows.Forms.Panel panel4;
    }
}