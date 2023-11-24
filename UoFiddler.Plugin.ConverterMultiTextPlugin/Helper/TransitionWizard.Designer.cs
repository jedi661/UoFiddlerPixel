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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Helper
{
    partial class TransitionWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransitionWizard));
            MainMenu1 = new System.Windows.Forms.MenuStrip();
            MenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            MenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            MenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            MenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            MenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            MenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            MenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            MenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            ImageList1 = new System.Windows.Forms.ImageList(components);
            Label1 = new System.Windows.Forms.Label();
            Label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            Select_Group_A = new System.Windows.Forms.ComboBox();
            Select_Group_B = new System.Windows.Forms.ComboBox();
            TextBox1 = new System.Windows.Forms.TextBox();
            Panel2 = new System.Windows.Forms.Panel();
            TreeView1 = new System.Windows.Forms.TreeView();
            Panel1 = new System.Windows.Forms.Panel();
            TileID = new System.Windows.Forms.VScrollBar();
            ToolBar1 = new System.Windows.Forms.ToolStrip();
            ToolAdd = new System.Windows.Forms.ToolStripButton();
            ToolDelete = new System.Windows.Forms.ToolStripButton();
            MapZoom = new System.Windows.Forms.ToolStripButton();
            StaticZoom = new System.Windows.Forms.ToolStripButton();
            MainMenu1.SuspendLayout();
            Panel1.SuspendLayout();
            ToolBar1.SuspendLayout();
            SuspendLayout();
            // 
            // MainMenu1
            // 
            MainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { MenuItem1, MenuItem2, MenuItem5 });
            MainMenu1.Location = new System.Drawing.Point(0, 0);
            MainMenu1.Name = "MainMenu1";
            MainMenu1.Size = new System.Drawing.Size(656, 24);
            MainMenu1.TabIndex = 0;
            MainMenu1.Text = "menuStrip1";
            // 
            // MenuItem1
            // 
            MenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { MenuItem6, MenuItem7, MenuItem8 });
            MenuItem1.Name = "MenuItem1";
            MenuItem1.Size = new System.Drawing.Size(37, 20);
            MenuItem1.Text = "File";
            // 
            // MenuItem6
            // 
            MenuItem6.Name = "MenuItem6";
            MenuItem6.Size = new System.Drawing.Size(100, 22);
            MenuItem6.Text = "New";
            MenuItem6.Click += MenuItem6_Click;
            // 
            // MenuItem7
            // 
            MenuItem7.Name = "MenuItem7";
            MenuItem7.Size = new System.Drawing.Size(100, 22);
            MenuItem7.Text = "Save";
            MenuItem7.Click += MenuItem7_Click;
            // 
            // MenuItem8
            // 
            MenuItem8.Name = "MenuItem8";
            MenuItem8.Size = new System.Drawing.Size(100, 22);
            MenuItem8.Text = "Load";
            MenuItem8.Click += MenuItem8_Click;
            // 
            // MenuItem2
            // 
            MenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { MenuItem3, MenuItem4 });
            MenuItem2.Name = "MenuItem2";
            MenuItem2.Size = new System.Drawing.Size(44, 20);
            MenuItem2.Text = "View";
            // 
            // MenuItem3
            // 
            MenuItem3.Name = "MenuItem3";
            MenuItem3.Size = new System.Drawing.Size(157, 22);
            MenuItem3.Text = "View Map Tiles";
            MenuItem3.Click += MenuItem3_Click;
            // 
            // MenuItem4
            // 
            MenuItem4.Name = "MenuItem4";
            MenuItem4.Size = new System.Drawing.Size(157, 22);
            MenuItem4.Text = "View Static Tiles";
            MenuItem4.Click += MenuItem4_Click;
            // 
            // MenuItem5
            // 
            MenuItem5.Name = "MenuItem5";
            MenuItem5.Size = new System.Drawing.Size(48, 20);
            MenuItem5.Text = "Make";
            MenuItem5.Click += MenuItem5_Click;
            // 
            // ImageList1
            // 
            ImageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            ImageList1.ImageSize = new System.Drawing.Size(16, 16);
            ImageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Location = new System.Drawing.Point(5, 30);
            Label1.Name = "Label1";
            Label1.Size = new System.Drawing.Size(46, 13);
            Label1.TabIndex = 1;
            Label1.Text = "Group A";
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Location = new System.Drawing.Point(132, 30);
            Label2.Name = "Label2";
            Label2.Size = new System.Drawing.Size(46, 13);
            Label2.TabIndex = 2;
            Label2.Text = "Group B";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(272, 30);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(60, 13);
            label3.TabIndex = 3;
            label3.Text = "Description";
            // 
            // Select_Group_A
            // 
            Select_Group_A.FormattingEnabled = true;
            Select_Group_A.Location = new System.Drawing.Point(5, 47);
            Select_Group_A.Name = "Select_Group_A";
            Select_Group_A.Size = new System.Drawing.Size(121, 21);
            Select_Group_A.Sorted = true;
            Select_Group_A.TabIndex = 4;
            Select_Group_A.SelectedIndexChanged += Select_Group_A_SelectedIndexChanged;
            // 
            // Select_Group_B
            // 
            Select_Group_B.FormattingEnabled = true;
            Select_Group_B.Location = new System.Drawing.Point(132, 47);
            Select_Group_B.Name = "Select_Group_B";
            Select_Group_B.Size = new System.Drawing.Size(121, 21);
            Select_Group_B.Sorted = true;
            Select_Group_B.TabIndex = 5;
            Select_Group_B.SelectedIndexChanged += Select_Group_B_SelectedIndexChanged;
            // 
            // TextBox1
            // 
            TextBox1.Location = new System.Drawing.Point(272, 47);
            TextBox1.Name = "TextBox1";
            TextBox1.Size = new System.Drawing.Size(377, 20);
            TextBox1.TabIndex = 69;
            // 
            // Panel2
            // 
            Panel2.Location = new System.Drawing.Point(5, 74);
            Panel2.Name = "Panel2";
            Panel2.Size = new System.Drawing.Size(256, 296);
            Panel2.TabIndex = 65;
            Panel2.Paint += Panel2_Paint;
            // 
            // TreeView1
            // 
            TreeView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            TreeView1.Location = new System.Drawing.Point(272, 73);
            TreeView1.Name = "TreeView1";
            TreeView1.Size = new System.Drawing.Size(200, 296);
            TreeView1.TabIndex = 1;
            TreeView1.AfterSelect += TreeView1_AfterSelec;
            // 
            // Panel1
            // 
            Panel1.Controls.Add(TileID);
            Panel1.Location = new System.Drawing.Point(478, 74);
            Panel1.Name = "Panel1";
            Panel1.Size = new System.Drawing.Size(169, 240);
            Panel1.TabIndex = 70;
            Panel1.Paint += Panel1_Paint;
            // 
            // TileID
            // 
            TileID.Dock = System.Windows.Forms.DockStyle.Right;
            TileID.LargeChange = 16;
            TileID.Location = new System.Drawing.Point(152, 0);
            TileID.Maximum = 65500;
            TileID.Name = "TileID";
            TileID.Size = new System.Drawing.Size(17, 240);
            TileID.TabIndex = 0;
            TileID.Scroll += LandTileID_Scroll;
            // 
            // ToolBar1
            // 
            ToolBar1.Dock = System.Windows.Forms.DockStyle.None;
            ToolBar1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            ToolBar1.ImageScalingSize = new System.Drawing.Size(30, 30);
            ToolBar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolAdd, ToolDelete, MapZoom, StaticZoom });
            ToolBar1.Location = new System.Drawing.Point(481, 330);
            ToolBar1.Name = "ToolBar1";
            ToolBar1.Size = new System.Drawing.Size(155, 39);
            ToolBar1.TabIndex = 71;
            ToolBar1.Tag = "75";
            ToolBar1.Text = "toolStrip1";
            // 
            // ToolAdd
            // 
            ToolAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolAdd.Image = Properties.Resources.uomc01;
            ToolAdd.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            ToolAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolAdd.Name = "ToolAdd";
            ToolAdd.Size = new System.Drawing.Size(40, 36);
            ToolAdd.Tag = "Add";
            ToolAdd.Text = "Add";
            ToolAdd.Click += ToolAdd_Click;
            // 
            // ToolDelete
            // 
            ToolDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            ToolDelete.Image = Properties.Resources.uomc02;
            ToolDelete.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            ToolDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            ToolDelete.Name = "ToolDelete";
            ToolDelete.Size = new System.Drawing.Size(40, 36);
            ToolDelete.Tag = "Delete";
            ToolDelete.Text = "Delete";
            ToolDelete.Click += ToolDelete_Click;
            // 
            // MapZoom
            // 
            MapZoom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            MapZoom.Image = Properties.Resources.uomc18;
            MapZoom.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            MapZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            MapZoom.Name = "MapZoom";
            MapZoom.Size = new System.Drawing.Size(36, 36);
            MapZoom.Tag = "MapZoom";
            MapZoom.Text = "Map Zoom";
            MapZoom.Click += MapZoom_Click;
            // 
            // StaticZoom
            // 
            StaticZoom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            StaticZoom.Image = Properties.Resources.uomc17;
            StaticZoom.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            StaticZoom.ImageTransparentColor = System.Drawing.Color.Magenta;
            StaticZoom.Name = "StaticZoom";
            StaticZoom.Size = new System.Drawing.Size(36, 36);
            StaticZoom.Tag = "StaticZoom";
            StaticZoom.Text = "Static Zoom";
            StaticZoom.Click += StaticZoom_Click;
            // 
            // TransitionWizard
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(656, 380);
            Controls.Add(ToolBar1);
            Controls.Add(Panel1);
            Controls.Add(TreeView1);
            Controls.Add(Panel2);
            Controls.Add(TextBox1);
            Controls.Add(Select_Group_B);
            Controls.Add(Select_Group_A);
            Controls.Add(label3);
            Controls.Add(Label2);
            Controls.Add(Label1);
            Controls.Add(MainMenu1);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = MainMenu1;
            Name = "TransitionWizard";
            Text = "UO Landscaper Transition Wizard";
            MainMenu1.ResumeLayout(false);
            MainMenu1.PerformLayout();
            Panel1.ResumeLayout(false);
            ToolBar1.ResumeLayout(false);
            ToolBar1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip MainMenu1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem6;
        private System.Windows.Forms.ToolStripMenuItem MenuItem7;
        private System.Windows.Forms.ToolStripMenuItem MenuItem8;
        private System.Windows.Forms.ToolStripMenuItem MenuItem2;
        private System.Windows.Forms.ToolStripMenuItem MenuItem3;
        private System.Windows.Forms.ToolStripMenuItem MenuItem4;
        private System.Windows.Forms.ToolStripMenuItem MenuItem5;
        private System.Windows.Forms.ImageList ImageList1;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox Select_Group_A;
        private System.Windows.Forms.ComboBox Select_Group_B;
        private System.Windows.Forms.TextBox TextBox1;
        private System.Windows.Forms.Panel Panel2;
        private System.Windows.Forms.TreeView TreeView1;
        private System.Windows.Forms.Panel Panel1;
        private System.Windows.Forms.VScrollBar TileID;
        private System.Windows.Forms.ToolStrip ToolBar1;
        private System.Windows.Forms.ToolStripButton ToolAdd;
        private System.Windows.Forms.ToolStripButton ToolDelete;
        private System.Windows.Forms.ToolStripButton MapZoom;
        private System.Windows.Forms.ToolStripButton StaticZoom;
    }
}