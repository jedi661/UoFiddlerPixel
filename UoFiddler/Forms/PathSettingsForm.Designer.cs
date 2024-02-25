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

namespace UoFiddler.Forms
{
    partial class PathSettingsForm
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
            pgPaths = new System.Windows.Forms.PropertyGrid();
            contextMenuPath = new System.Windows.Forms.ContextMenuStrip(components);
            newDirAndMulToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadSingleMulFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            DeleteLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tsPathSettingsMenu = new System.Windows.Forms.ToolStrip();
            tsBtnReloadPaths = new System.Windows.Forms.ToolStripButton();
            tsBtnSetPathManual = new System.Windows.Forms.ToolStripButton();
            tsTbRootPath = new System.Windows.Forms.ToolStripTextBox();
            contextMenuPath.SuspendLayout();
            tsPathSettingsMenu.SuspendLayout();
            SuspendLayout();
            // 
            // pgPaths
            // 
            pgPaths.ContextMenuStrip = contextMenuPath;
            pgPaths.Dock = System.Windows.Forms.DockStyle.Fill;
            pgPaths.HelpVisible = false;
            pgPaths.Location = new System.Drawing.Point(0, 25);
            pgPaths.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pgPaths.Name = "pgPaths";
            pgPaths.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            pgPaths.Size = new System.Drawing.Size(744, 396);
            pgPaths.TabIndex = 0;
            pgPaths.ToolbarVisible = false;
            // 
            // contextMenuPath
            // 
            contextMenuPath.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { newDirAndMulToolStripMenuItem, loadSingleMulFileToolStripMenuItem, DeleteLineToolStripMenuItem });
            contextMenuPath.Name = "contextMenuStrip1";
            contextMenuPath.Size = new System.Drawing.Size(205, 70);
            // 
            // newDirAndMulToolStripMenuItem
            // 
            newDirAndMulToolStripMenuItem.Image = Properties.Resources.reload_files;
            newDirAndMulToolStripMenuItem.Name = "newDirAndMulToolStripMenuItem";
            newDirAndMulToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            newDirAndMulToolStripMenuItem.Text = "Reload  all Files and New";
            newDirAndMulToolStripMenuItem.ToolTipText = "Reload  and Load all .Mul files from Dir";
            newDirAndMulToolStripMenuItem.Click += newDirAndMulToolStripMenuItem_Click;
            // 
            // loadSingleMulFileToolStripMenuItem
            // 
            loadSingleMulFileToolStripMenuItem.Image = Properties.Resources.Directory;
            loadSingleMulFileToolStripMenuItem.Name = "loadSingleMulFileToolStripMenuItem";
            loadSingleMulFileToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            loadSingleMulFileToolStripMenuItem.Text = "Load Single Mul File";
            loadSingleMulFileToolStripMenuItem.ToolTipText = "Load Singe File from Dir";
            loadSingleMulFileToolStripMenuItem.Click += loadSingleMulFileToolStripMenuItem_Click;
            // 
            // DeleteLineToolStripMenuItem
            // 
            DeleteLineToolStripMenuItem.Image = Properties.Resources.indentLeft;
            DeleteLineToolStripMenuItem.Name = "DeleteLineToolStripMenuItem";
            DeleteLineToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            DeleteLineToolStripMenuItem.Text = "Delete Line";
            DeleteLineToolStripMenuItem.ToolTipText = "Remove Line";
            DeleteLineToolStripMenuItem.Click += DeleteLineToolStripMenuItem_Click;
            // 
            // tsPathSettingsMenu
            // 
            tsPathSettingsMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            tsPathSettingsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsBtnReloadPaths, tsBtnSetPathManual, tsTbRootPath });
            tsPathSettingsMenu.Location = new System.Drawing.Point(0, 0);
            tsPathSettingsMenu.Name = "tsPathSettingsMenu";
            tsPathSettingsMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            tsPathSettingsMenu.Size = new System.Drawing.Size(744, 25);
            tsPathSettingsMenu.TabIndex = 1;
            tsPathSettingsMenu.Text = "toolStrip1";
            // 
            // tsBtnReloadPaths
            // 
            tsBtnReloadPaths.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsBtnReloadPaths.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnReloadPaths.Name = "tsBtnReloadPaths";
            tsBtnReloadPaths.Size = new System.Drawing.Size(79, 22);
            tsBtnReloadPaths.Text = "Reload paths";
            tsBtnReloadPaths.Click += ReloadPath;
            // 
            // tsBtnSetPathManual
            // 
            tsBtnSetPathManual.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsBtnSetPathManual.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnSetPathManual.Name = "tsBtnSetPathManual";
            tsBtnSetPathManual.Size = new System.Drawing.Size(97, 22);
            tsBtnSetPathManual.Text = "Set path manual";
            tsBtnSetPathManual.Click += OnClickManual;
            // 
            // tsTbRootPath
            // 
            tsTbRootPath.Name = "tsTbRootPath";
            tsTbRootPath.Size = new System.Drawing.Size(408, 25);
            tsTbRootPath.KeyDown += OnKeyDownDir;
            // 
            // PathSettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(744, 421);
            Controls.Add(pgPaths);
            Controls.Add(tsPathSettingsMenu);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximumSize = new System.Drawing.Size(791, 917);
            MinimumSize = new System.Drawing.Size(744, 340);
            Name = "PathSettingsForm";
            Text = "Path Settings";
            contextMenuPath.ResumeLayout(false);
            tsPathSettingsMenu.ResumeLayout(false);
            tsPathSettingsMenu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PropertyGrid pgPaths;
        private System.Windows.Forms.ToolStripButton tsBtnReloadPaths;
        private System.Windows.Forms.ToolStripButton tsBtnSetPathManual;
        private System.Windows.Forms.ToolStrip tsPathSettingsMenu;
        private System.Windows.Forms.ToolStripTextBox tsTbRootPath;
        private System.Windows.Forms.ContextMenuStrip contextMenuPath;
        private System.Windows.Forms.ToolStripMenuItem newDirAndMulToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadSingleMulFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DeleteLineToolStripMenuItem;
    }
}