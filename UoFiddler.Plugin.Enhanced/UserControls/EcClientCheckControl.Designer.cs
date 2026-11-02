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
    partial class EcClientCheckControl
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
            toolStripButtonTileArt = new System.Windows.Forms.ToolStripButton();
            toolStripButtonTextures = new System.Windows.Forms.ToolStripButton();
            toolStripButtonMultis = new System.Windows.Forms.ToolStripButton();
            toolStripButtonAnimations = new System.Windows.Forms.ToolStripButton();
            toolStripButtonHues = new System.Windows.Forms.ToolStripButton();
            toolStripButtonMaps = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonCheck = new System.Windows.Forms.ToolStripButton();
            toolStripButtonCancel = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            tabControl = new System.Windows.Forms.TabControl();
            tabPageReport = new System.Windows.Forms.TabPage();
            textBoxReport = new System.Windows.Forms.TextBox();
            tabPageHelp = new System.Windows.Forms.TabPage();
            flowLayoutPanelHelp = new System.Windows.Forms.FlowLayoutPanel();
            radioButtonHelpEnglish = new System.Windows.Forms.RadioButton();
            radioButtonHelpGerman = new System.Windows.Forms.RadioButton();
            richTextBoxHelp = new System.Windows.Forms.RichTextBox();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            toolStrip.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageReport.SuspendLayout();
            tabPageHelp.SuspendLayout();
            flowLayoutPanelHelp.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonTileArt, toolStripButtonTextures, toolStripButtonMultis, toolStripButtonAnimations, toolStripButtonHues, toolStripButtonMaps, toolStripSeparator1, toolStripButtonCheck, toolStripButtonCancel, toolStripSeparator2, toolStripButtonSave });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripButtonTileArt
            //
            toolStripButtonTileArt.CheckOnClick = true;
            toolStripButtonTileArt.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonTileArt.Checked = true;
            toolStripButtonTileArt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonTileArt.Name = "toolStripButtonTileArt";
            toolStripButtonTileArt.Text = "TileArt";
            toolStripButtonTileArt.ToolTipText = "Entries without image, broken name references, wrong ids";
            //
            // toolStripButtonTextures
            //
            toolStripButtonTextures.CheckOnClick = true;
            toolStripButtonTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonTextures.Checked = true;
            toolStripButtonTextures.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonTextures.Name = "toolStripButtonTextures";
            toolStripButtonTextures.Text = "Textures";
            toolStripButtonTextures.ToolTipText = "Texture files tileart points to (Texture.uop / LegacyTexture.uop)";
            //
            // toolStripButtonMultis
            //
            toolStripButtonMultis.CheckOnClick = true;
            toolStripButtonMultis.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonMultis.Checked = true;
            toolStripButtonMultis.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonMultis.Name = "toolStripButtonMultis";
            toolStripButtonMultis.Text = "Multis";
            toolStripButtonMultis.ToolTipText = "Multi parts with items the EC doesn't know";
            //
            // toolStripButtonAnimations
            //
            toolStripButtonAnimations.CheckOnClick = true;
            toolStripButtonAnimations.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonAnimations.Checked = true;
            toolStripButtonAnimations.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAnimations.Name = "toolStripButtonAnimations";
            toolStripButtonAnimations.Text = "Item animations";
            toolStripButtonAnimations.ToolTipText = "AnimationDefinition frames with unknown items";
            //
            // toolStripButtonHues
            //
            toolStripButtonHues.CheckOnClick = true;
            toolStripButtonHues.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonHues.Checked = true;
            toolStripButtonHues.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonHues.Name = "toolStripButtonHues";
            toolStripButtonHues.Text = "Hues";
            toolStripButtonHues.ToolTipText = "Hues.uop: bitmaps, hues.dds, overview, names";
            //
            // toolStripButtonMaps
            //
            toolStripButtonMaps.CheckOnClick = true;
            toolStripButtonMaps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonMaps.Name = "toolStripButtonMaps";
            toolStripButtonMaps.Text = "Maps (slow)";
            toolStripButtonMaps.ToolTipText = "Every facet: size, sectors, border layers, statics";
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripButtonCheck
            //
            toolStripButtonCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonCheck.Name = "toolStripButtonCheck";
            toolStripButtonCheck.Text = "Check client";
            toolStripButtonCheck.Click += OnClickCheck;
            //
            // toolStripButtonCancel
            //
            toolStripButtonCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonCancel.Enabled = false;
            toolStripButtonCancel.Name = "toolStripButtonCancel";
            toolStripButtonCancel.Text = "Cancel";
            toolStripButtonCancel.Click += OnClickCancel;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Text = "Save report...";
            toolStripButtonSave.Click += OnClickSave;
            //
            // tabControl
            //
            tabControl.Controls.Add(tabPageReport);
            tabControl.Controls.Add(tabPageHelp);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.TabIndex = 1;
            //
            // tabPageReport
            //
            tabPageReport.Controls.Add(textBoxReport);
            tabPageReport.Location = new System.Drawing.Point(4, 24);
            tabPageReport.Name = "tabPageReport";
            tabPageReport.Padding = new System.Windows.Forms.Padding(3);
            tabPageReport.Size = new System.Drawing.Size(500, 300);
            tabPageReport.TabIndex = 0;
            tabPageReport.Text = "Report";
            tabPageReport.UseVisualStyleBackColor = true;
            //
            // textBoxReport
            //
            textBoxReport.BackColor = System.Drawing.Color.White;
            textBoxReport.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxReport.Font = new System.Drawing.Font("Consolas", 9F);
            textBoxReport.Multiline = true;
            textBoxReport.Name = "textBoxReport";
            textBoxReport.ReadOnly = true;
            textBoxReport.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textBoxReport.Text = "Choose the areas on top and press \"Check client\".";
            textBoxReport.WordWrap = false;
            //
            // tabPageHelp
            //
            tabPageHelp.Controls.Add(richTextBoxHelp);
            tabPageHelp.Controls.Add(flowLayoutPanelHelp);
            tabPageHelp.Location = new System.Drawing.Point(4, 24);
            tabPageHelp.Name = "tabPageHelp";
            tabPageHelp.Padding = new System.Windows.Forms.Padding(3);
            tabPageHelp.Size = new System.Drawing.Size(500, 300);
            tabPageHelp.TabIndex = 1;
            tabPageHelp.Text = "Help";
            tabPageHelp.UseVisualStyleBackColor = true;
            //
            // flowLayoutPanelHelp
            //
            flowLayoutPanelHelp.Controls.Add(radioButtonHelpEnglish);
            flowLayoutPanelHelp.Controls.Add(radioButtonHelpGerman);
            flowLayoutPanelHelp.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelHelp.Name = "flowLayoutPanelHelp";
            flowLayoutPanelHelp.Size = new System.Drawing.Size(500, 30);
            //
            // radioButtonHelpEnglish
            //
            radioButtonHelpEnglish.AutoSize = true;
            radioButtonHelpEnglish.Checked = true;
            radioButtonHelpEnglish.Name = "radioButtonHelpEnglish";
            radioButtonHelpEnglish.TabStop = true;
            radioButtonHelpEnglish.Text = "English";
            radioButtonHelpEnglish.UseVisualStyleBackColor = true;
            radioButtonHelpEnglish.CheckedChanged += OnHelpLanguageChanged;
            //
            // radioButtonHelpGerman
            //
            radioButtonHelpGerman.AutoSize = true;
            radioButtonHelpGerman.Name = "radioButtonHelpGerman";
            radioButtonHelpGerman.Text = "Deutsch";
            radioButtonHelpGerman.UseVisualStyleBackColor = true;
            radioButtonHelpGerman.CheckedChanged += OnHelpLanguageChanged;
            //
            // richTextBoxHelp
            //
            richTextBoxHelp.BackColor = System.Drawing.Color.White;
            richTextBoxHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBoxHelp.Font = new System.Drawing.Font("Consolas", 9F);
            richTextBoxHelp.Name = "richTextBoxHelp";
            richTextBoxHelp.ReadOnly = true;
            richTextBoxHelp.WordWrap = false;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel, toolStripProgressBar });
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
            // toolStripProgressBar
            //
            toolStripProgressBar.Name = "toolStripProgressBar";
            toolStripProgressBar.Size = new System.Drawing.Size(160, 16);
            //
            // EcClientCheckControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tabControl);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcClientCheckControl";
            Size = new System.Drawing.Size(950, 600);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            tabControl.ResumeLayout(false);
            tabPageReport.ResumeLayout(false);
            tabPageReport.PerformLayout();
            tabPageHelp.ResumeLayout(false);
            tabPageHelp.PerformLayout();
            flowLayoutPanelHelp.ResumeLayout(false);
            flowLayoutPanelHelp.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonTileArt;
        private System.Windows.Forms.ToolStripButton toolStripButtonTextures;
        private System.Windows.Forms.ToolStripButton toolStripButtonMultis;
        private System.Windows.Forms.ToolStripButton toolStripButtonAnimations;
        private System.Windows.Forms.ToolStripButton toolStripButtonHues;
        private System.Windows.Forms.ToolStripButton toolStripButtonMaps;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonCheck;
        private System.Windows.Forms.ToolStripButton toolStripButtonCancel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageReport;
        private System.Windows.Forms.TextBox textBoxReport;
        private System.Windows.Forms.TabPage tabPageHelp;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelHelp;
        private System.Windows.Forms.RadioButton radioButtonHelpEnglish;
        private System.Windows.Forms.RadioButton radioButtonHelpGerman;
        private System.Windows.Forms.RichTextBox richTextBoxHelp;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
    }
}
