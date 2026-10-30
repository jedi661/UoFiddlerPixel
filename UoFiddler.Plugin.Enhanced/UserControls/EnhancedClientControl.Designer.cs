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
    partial class EnhancedClientControl
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
            toolStripLabelFolder = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxFolder = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonBrowse = new System.Windows.Forms.ToolStripButton();
            toolStripButtonLoad = new System.Windows.Forms.ToolStripButton();
            toolStripButtonReload = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonAnalyze = new System.Windows.Forms.ToolStripButton();
            toolStripDropDownNames = new System.Windows.Forms.ToolStripDropDownButton();
            loadDictionaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportNamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            forgetDictionaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelInfo = new System.Windows.Forms.ToolStripLabel();
            tabControl = new System.Windows.Forms.TabControl();
            tabPageArt = new System.Windows.Forms.TabPage();
            ecArtControl = new EcArtControl();
            tabPageTileData = new System.Windows.Forms.TabPage();
            ecTileArtControl = new EcTileArtControl();
            tabPageGumps = new System.Windows.Forms.TabPage();
            ecGumpsControl = new EcGumpsControl();
            tabPageHues = new System.Windows.Forms.TabPage();
            ecHuesControl = new EcHuesControl();
            tabPageAnimations = new System.Windows.Forms.TabPage();
            ecMobileAnimationControl = new EcMobileAnimationControl();
            tabPageAnimData = new System.Windows.Forms.TabPage();
            ecAnimDefinitionControl = new EcAnimDefinitionControl();
            tabPageMultis = new System.Windows.Forms.TabPage();
            ecMultisControl = new EcMultisControl();
            tabPageMap = new System.Windows.Forms.TabPage();
            ecMapControl = new EcMapControl();
            tabPageSounds = new System.Windows.Forms.TabPage();
            ecSoundsControl = new EcSoundsControl();
            tabPageCliloc = new System.Windows.Forms.TabPage();
            ecClilocControl = new EcClilocControl();
            tabPageStrings = new System.Windows.Forms.TabPage();
            ecStringsControl = new EcStringsControl();
            tabPageInterface = new System.Windows.Forms.TabPage();
            ecFileTreeControl = new EcFileTreeControl();
            tabPageBrowser = new System.Windows.Forms.TabPage();
            ecUopBrowserControl = new EcUopBrowserControl();
            toolStrip.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageArt.SuspendLayout();
            tabPageTileData.SuspendLayout();
            tabPageGumps.SuspendLayout();
            tabPageHues.SuspendLayout();
            tabPageAnimations.SuspendLayout();
            tabPageAnimData.SuspendLayout();
            tabPageMultis.SuspendLayout();
            tabPageMap.SuspendLayout();
            tabPageSounds.SuspendLayout();
            tabPageCliloc.SuspendLayout();
            tabPageStrings.SuspendLayout();
            tabPageInterface.SuspendLayout();
            tabPageBrowser.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelFolder, toolStripTextBoxFolder, toolStripButtonBrowse, toolStripButtonLoad, toolStripButtonReload, toolStripSeparator1, toolStripButtonAnalyze, toolStripDropDownNames, toolStripSeparator2, toolStripLabelInfo });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelFolder
            //
            toolStripLabelFolder.Name = "toolStripLabelFolder";
            toolStripLabelFolder.Text = "Enhanced Client folder:";
            //
            // toolStripTextBoxFolder
            //
            toolStripTextBoxFolder.Name = "toolStripTextBoxFolder";
            toolStripTextBoxFolder.Size = new System.Drawing.Size(420, 25);
            toolStripTextBoxFolder.ToolTipText = "Folder of the Enhanced Client (where tileart.uop, Texture.uop, ... are)";
            toolStripTextBoxFolder.KeyDown += OnFolderKeyDown;
            //
            // toolStripButtonBrowse
            //
            toolStripButtonBrowse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonBrowse.Name = "toolStripButtonBrowse";
            toolStripButtonBrowse.Text = "Browse...";
            toolStripButtonBrowse.Click += OnClickBrowse;
            //
            // toolStripButtonLoad
            //
            toolStripButtonLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonLoad.Name = "toolStripButtonLoad";
            toolStripButtonLoad.Text = "Load";
            toolStripButtonLoad.Click += OnClickLoad;
            //
            // toolStripButtonReload
            //
            toolStripButtonReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonReload.Name = "toolStripButtonReload";
            toolStripButtonReload.Text = "Reload";
            toolStripButtonReload.ToolTipText = "Close all files and read them again (unsaved changes are lost)";
            toolStripButtonReload.Click += OnClickReload;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripButtonAnalyze
            //
            toolStripButtonAnalyze.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAnalyze.Name = "toolStripButtonAnalyze";
            toolStripButtonAnalyze.Text = "Analyze folder...";
            toolStripButtonAnalyze.ToolTipText = "Write a report (zip) about every UOP file of the folder – send it to the developer to add support for unknown files";
            toolStripButtonAnalyze.Click += OnClickAnalyze;
            //
            // toolStripDropDownNames
            //
            toolStripDropDownNames.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadDictionaryToolStripMenuItem, exportNamesToolStripMenuItem, forgetDictionaryToolStripMenuItem });
            toolStripDropDownNames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownNames.Name = "toolStripDropDownNames";
            toolStripDropDownNames.Text = "Names";
            toolStripDropDownNames.ToolTipText = "Lists of known packed file names";
            //
            // loadDictionaryToolStripMenuItem
            //
            loadDictionaryToolStripMenuItem.Name = "loadDictionaryToolStripMenuItem";
            loadDictionaryToolStripMenuItem.Text = "Load name dictionary (.dic)...";
            loadDictionaryToolStripMenuItem.Click += OnClickLoadDictionary;
            //
            // exportNamesToolStripMenuItem
            //
            exportNamesToolStripMenuItem.Name = "exportNamesToolStripMenuItem";
            exportNamesToolStripMenuItem.Text = "Export known names of the open files (.dic)...";
            exportNamesToolStripMenuItem.Click += OnClickExportNames;
            //
            // forgetDictionaryToolStripMenuItem
            //
            forgetDictionaryToolStripMenuItem.Name = "forgetDictionaryToolStripMenuItem";
            forgetDictionaryToolStripMenuItem.Text = "Forget the name dictionary";
            forgetDictionaryToolStripMenuItem.Click += OnClickForgetDictionary;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripLabelInfo
            //
            toolStripLabelInfo.ForeColor = System.Drawing.Color.DimGray;
            toolStripLabelInfo.Name = "toolStripLabelInfo";
            //
            // tabControl
            //
            tabControl.Controls.Add(tabPageArt);
            tabControl.Controls.Add(tabPageTileData);
            tabControl.Controls.Add(tabPageGumps);
            tabControl.Controls.Add(tabPageHues);
            tabControl.Controls.Add(tabPageAnimations);
            tabControl.Controls.Add(tabPageAnimData);
            tabControl.Controls.Add(tabPageMultis);
            tabControl.Controls.Add(tabPageMap);
            tabControl.Controls.Add(tabPageSounds);
            tabControl.Controls.Add(tabPageCliloc);
            tabControl.Controls.Add(tabPageStrings);
            tabControl.Controls.Add(tabPageInterface);
            tabControl.Controls.Add(tabPageBrowser);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.TabIndex = 1;
            tabControl.SelectedIndexChanged += OnTabChanged;
            //
            // tabPageArt
            //
            tabPageArt.Controls.Add(ecArtControl);
            tabPageArt.Location = new System.Drawing.Point(4, 24);
            tabPageArt.Name = "tabPageArt";
            tabPageArt.Padding = new System.Windows.Forms.Padding(3);
            tabPageArt.Size = new System.Drawing.Size(500, 300);
            tabPageArt.TabIndex = 0;
            tabPageArt.Text = "Items / Art";
            tabPageArt.UseVisualStyleBackColor = true;
            //
            // ecArtControl
            //
            ecArtControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecArtControl.Name = "ecArtControl";
            ecArtControl.TabIndex = 0;
            //
            // tabPageTileData
            //
            tabPageTileData.Controls.Add(ecTileArtControl);
            tabPageTileData.Location = new System.Drawing.Point(4, 24);
            tabPageTileData.Name = "tabPageTileData";
            tabPageTileData.Padding = new System.Windows.Forms.Padding(3);
            tabPageTileData.Size = new System.Drawing.Size(500, 300);
            tabPageTileData.TabIndex = 1;
            tabPageTileData.Text = "TileData";
            tabPageTileData.UseVisualStyleBackColor = true;
            //
            // ecTileArtControl
            //
            ecTileArtControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecTileArtControl.Name = "ecTileArtControl";
            ecTileArtControl.TabIndex = 0;
            //
            // tabPageGumps
            //
            tabPageGumps.Controls.Add(ecGumpsControl);
            tabPageGumps.Location = new System.Drawing.Point(4, 24);
            tabPageGumps.Name = "tabPageGumps";
            tabPageGumps.Padding = new System.Windows.Forms.Padding(3);
            tabPageGumps.Size = new System.Drawing.Size(500, 300);
            tabPageGumps.TabIndex = 2;
            tabPageGumps.Text = "Gumps";
            tabPageGumps.UseVisualStyleBackColor = true;
            //
            // ecGumpsControl
            //
            ecGumpsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecGumpsControl.Name = "ecGumpsControl";
            ecGumpsControl.TabIndex = 0;
            //
            // tabPageHues
            //
            tabPageHues.Controls.Add(ecHuesControl);
            tabPageHues.Location = new System.Drawing.Point(4, 24);
            tabPageHues.Name = "tabPageHues";
            tabPageHues.Padding = new System.Windows.Forms.Padding(3);
            tabPageHues.Size = new System.Drawing.Size(500, 300);
            tabPageHues.TabIndex = 3;
            tabPageHues.Text = "Hues";
            tabPageHues.UseVisualStyleBackColor = true;
            //
            // ecHuesControl
            //
            ecHuesControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecHuesControl.Name = "ecHuesControl";
            ecHuesControl.TabIndex = 0;
            //
            // tabPageAnimations
            //
            tabPageAnimations.Controls.Add(ecMobileAnimationControl);
            tabPageAnimations.Location = new System.Drawing.Point(4, 24);
            tabPageAnimations.Name = "tabPageAnimations";
            tabPageAnimations.Padding = new System.Windows.Forms.Padding(3);
            tabPageAnimations.Size = new System.Drawing.Size(500, 300);
            tabPageAnimations.TabIndex = 4;
            tabPageAnimations.Text = "Animations";
            tabPageAnimations.UseVisualStyleBackColor = true;
            //
            // ecMobileAnimationControl
            //
            ecMobileAnimationControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecMobileAnimationControl.Name = "ecMobileAnimationControl";
            ecMobileAnimationControl.TabIndex = 0;
            //
            // tabPageAnimData
            //
            tabPageAnimData.Controls.Add(ecAnimDefinitionControl);
            tabPageAnimData.Location = new System.Drawing.Point(4, 24);
            tabPageAnimData.Name = "tabPageAnimData";
            tabPageAnimData.Padding = new System.Windows.Forms.Padding(3);
            tabPageAnimData.Size = new System.Drawing.Size(500, 300);
            tabPageAnimData.TabIndex = 5;
            tabPageAnimData.Text = "AnimData";
            tabPageAnimData.UseVisualStyleBackColor = true;
            //
            // ecAnimDefinitionControl
            //
            ecAnimDefinitionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecAnimDefinitionControl.Name = "ecAnimDefinitionControl";
            ecAnimDefinitionControl.TabIndex = 0;
            //
            // tabPageMultis
            //
            tabPageMultis.Controls.Add(ecMultisControl);
            tabPageMultis.Location = new System.Drawing.Point(4, 24);
            tabPageMultis.Name = "tabPageMultis";
            tabPageMultis.Padding = new System.Windows.Forms.Padding(3);
            tabPageMultis.Size = new System.Drawing.Size(500, 300);
            tabPageMultis.TabIndex = 6;
            tabPageMultis.Text = "Multis";
            tabPageMultis.UseVisualStyleBackColor = true;
            //
            // ecMultisControl
            //
            ecMultisControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecMultisControl.Name = "ecMultisControl";
            ecMultisControl.TabIndex = 0;
            //
            // tabPageMap
            //
            tabPageMap.Controls.Add(ecMapControl);
            tabPageMap.Location = new System.Drawing.Point(4, 24);
            tabPageMap.Name = "tabPageMap";
            tabPageMap.Padding = new System.Windows.Forms.Padding(3);
            tabPageMap.Size = new System.Drawing.Size(500, 300);
            tabPageMap.TabIndex = 7;
            tabPageMap.Text = "Map";
            tabPageMap.UseVisualStyleBackColor = true;
            //
            // ecMapControl
            //
            ecMapControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecMapControl.Name = "ecMapControl";
            ecMapControl.TabIndex = 0;
            //
            // tabPageSounds
            //
            tabPageSounds.Controls.Add(ecSoundsControl);
            tabPageSounds.Location = new System.Drawing.Point(4, 24);
            tabPageSounds.Name = "tabPageSounds";
            tabPageSounds.Padding = new System.Windows.Forms.Padding(3);
            tabPageSounds.Size = new System.Drawing.Size(500, 300);
            tabPageSounds.TabIndex = 8;
            tabPageSounds.Text = "Sounds";
            tabPageSounds.UseVisualStyleBackColor = true;
            //
            // ecSoundsControl
            //
            ecSoundsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecSoundsControl.Name = "ecSoundsControl";
            ecSoundsControl.TabIndex = 0;
            //
            // tabPageCliloc
            //
            tabPageCliloc.Controls.Add(ecClilocControl);
            tabPageCliloc.Location = new System.Drawing.Point(4, 24);
            tabPageCliloc.Name = "tabPageCliloc";
            tabPageCliloc.Padding = new System.Windows.Forms.Padding(3);
            tabPageCliloc.Size = new System.Drawing.Size(500, 300);
            tabPageCliloc.TabIndex = 9;
            tabPageCliloc.Text = "Cliloc";
            tabPageCliloc.UseVisualStyleBackColor = true;
            //
            // ecClilocControl
            //
            ecClilocControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecClilocControl.Name = "ecClilocControl";
            ecClilocControl.TabIndex = 0;
            //
            // tabPageStrings
            //
            tabPageStrings.Controls.Add(ecStringsControl);
            tabPageStrings.Location = new System.Drawing.Point(4, 24);
            tabPageStrings.Name = "tabPageStrings";
            tabPageStrings.Padding = new System.Windows.Forms.Padding(3);
            tabPageStrings.Size = new System.Drawing.Size(500, 300);
            tabPageStrings.TabIndex = 10;
            tabPageStrings.Text = "String Dictionary";
            tabPageStrings.UseVisualStyleBackColor = true;
            //
            // ecStringsControl
            //
            ecStringsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecStringsControl.Name = "ecStringsControl";
            ecStringsControl.TabIndex = 0;
            //
            // tabPageInterface
            //
            tabPageInterface.Controls.Add(ecFileTreeControl);
            tabPageInterface.Location = new System.Drawing.Point(4, 24);
            tabPageInterface.Name = "tabPageInterface";
            tabPageInterface.Padding = new System.Windows.Forms.Padding(3);
            tabPageInterface.Size = new System.Drawing.Size(500, 300);
            tabPageInterface.TabIndex = 11;
            tabPageInterface.Text = "Interface / Data";
            tabPageInterface.UseVisualStyleBackColor = true;
            //
            // ecFileTreeControl
            //
            ecFileTreeControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecFileTreeControl.Name = "ecFileTreeControl";
            ecFileTreeControl.TabIndex = 0;
            //
            // tabPageBrowser
            //
            tabPageBrowser.Controls.Add(ecUopBrowserControl);
            tabPageBrowser.Location = new System.Drawing.Point(4, 24);
            tabPageBrowser.Name = "tabPageBrowser";
            tabPageBrowser.Padding = new System.Windows.Forms.Padding(3);
            tabPageBrowser.Size = new System.Drawing.Size(500, 300);
            tabPageBrowser.TabIndex = 12;
            tabPageBrowser.Text = "UOP Browser";
            tabPageBrowser.UseVisualStyleBackColor = true;
            //
            // ecUopBrowserControl
            //
            ecUopBrowserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            ecUopBrowserControl.Name = "ecUopBrowserControl";
            ecUopBrowserControl.TabIndex = 0;
            //
            // EnhancedClientControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tabControl);
            Controls.Add(toolStrip);
            Name = "EnhancedClientControl";
            Size = new System.Drawing.Size(1100, 650);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            tabControl.ResumeLayout(false);
            tabPageArt.ResumeLayout(false);
            tabPageArt.PerformLayout();
            tabPageTileData.ResumeLayout(false);
            tabPageTileData.PerformLayout();
            tabPageGumps.ResumeLayout(false);
            tabPageGumps.PerformLayout();
            tabPageHues.ResumeLayout(false);
            tabPageHues.PerformLayout();
            tabPageAnimations.ResumeLayout(false);
            tabPageAnimations.PerformLayout();
            tabPageAnimData.ResumeLayout(false);
            tabPageAnimData.PerformLayout();
            tabPageMultis.ResumeLayout(false);
            tabPageMultis.PerformLayout();
            tabPageMap.ResumeLayout(false);
            tabPageMap.PerformLayout();
            tabPageSounds.ResumeLayout(false);
            tabPageSounds.PerformLayout();
            tabPageCliloc.ResumeLayout(false);
            tabPageCliloc.PerformLayout();
            tabPageStrings.ResumeLayout(false);
            tabPageStrings.PerformLayout();
            tabPageInterface.ResumeLayout(false);
            tabPageInterface.PerformLayout();
            tabPageBrowser.ResumeLayout(false);
            tabPageBrowser.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelFolder;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFolder;
        private System.Windows.Forms.ToolStripButton toolStripButtonBrowse;
        private System.Windows.Forms.ToolStripButton toolStripButtonLoad;
        private System.Windows.Forms.ToolStripButton toolStripButtonReload;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonAnalyze;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownNames;
        private System.Windows.Forms.ToolStripMenuItem loadDictionaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportNamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem forgetDictionaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabelInfo;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageArt;
        private EcArtControl ecArtControl;
        private System.Windows.Forms.TabPage tabPageTileData;
        private EcTileArtControl ecTileArtControl;
        private System.Windows.Forms.TabPage tabPageGumps;
        private EcGumpsControl ecGumpsControl;
        private System.Windows.Forms.TabPage tabPageHues;
        private EcHuesControl ecHuesControl;
        private System.Windows.Forms.TabPage tabPageAnimations;
        private EcMobileAnimationControl ecMobileAnimationControl;
        private System.Windows.Forms.TabPage tabPageAnimData;
        private EcAnimDefinitionControl ecAnimDefinitionControl;
        private System.Windows.Forms.TabPage tabPageMultis;
        private EcMultisControl ecMultisControl;
        private System.Windows.Forms.TabPage tabPageMap;
        private EcMapControl ecMapControl;
        private System.Windows.Forms.TabPage tabPageSounds;
        private EcSoundsControl ecSoundsControl;
        private System.Windows.Forms.TabPage tabPageCliloc;
        private EcClilocControl ecClilocControl;
        private System.Windows.Forms.TabPage tabPageStrings;
        private EcStringsControl ecStringsControl;
        private System.Windows.Forms.TabPage tabPageInterface;
        private EcFileTreeControl ecFileTreeControl;
        private System.Windows.Forms.TabPage tabPageBrowser;
        private EcUopBrowserControl ecUopBrowserControl;
    }
}
