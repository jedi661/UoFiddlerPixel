namespace UoFiddler.Forms
{
    partial class LineConverterForm
    {
        // -----------------------------------------------------------------------
        // Designer bookkeeping
        // -----------------------------------------------------------------------

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        // -----------------------------------------------------------------------
        // STATIC UI RESOURCES (Designer‑safe)
        // -----------------------------------------------------------------------

        private System.Drawing.Color ClrAccent = System.Drawing.Color.FromArgb(0, 102, 180);
        private System.Drawing.Color ClrHeader = System.Drawing.Color.FromArgb(245, 248, 252);
        private System.Drawing.Color ClrPanelBg = System.Drawing.Color.FromArgb(250, 251, 253);
        private System.Drawing.Color ClrSep = System.Drawing.Color.FromArgb(210, 218, 228);

        private System.Drawing.Font FontUi = new System.Drawing.Font("Segoe UI", 9f);
        private System.Drawing.Font FontBold = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
        private System.Drawing.Font FontTitle = new System.Drawing.Font("Segoe UI Semibold", 14f);
        private System.Drawing.Font FontSub = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Italic);

        // -----------------------------------------------------------------------
        // InitializeComponent (Designer‑compatible)
        // -----------------------------------------------------------------------

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LineConverterForm));
            PnlHeader = new System.Windows.Forms.Panel();
            LblTitle = new System.Windows.Forms.Label();
            LblSubtitle = new System.Windows.Forms.Label();
            CmbAiModel = new System.Windows.Forms.ComboBox();
            BtnRemoveDuplicateLines = new System.Windows.Forms.Button();
            BtnRemoveEmptyLines = new System.Windows.Forms.Button();
            PnlToolbar = new System.Windows.Forms.Panel();
            BtnDarkMode = new System.Windows.Forms.Button();
            BtnRedo = new System.Windows.Forms.Button();
            BtnUndo = new System.Windows.Forms.Button();
            BtnAiExplain = new System.Windows.Forms.Button();
            BtnAiSafeMode = new System.Windows.Forms.Button();
            LblConvertGroup = new System.Windows.Forms.Label();
            BtnConvert = new System.Windows.Forms.Button();
            BtnConvert2 = new System.Windows.Forms.Button();
            BtnConvertSkipComments = new System.Windows.Forms.Button();
            BtnNormalise = new System.Windows.Forms.Button();
            ChkAddSpaces = new System.Windows.Forms.CheckBox();
            BtnCopyAll = new System.Windows.Forms.Button();
            BtnRestore = new System.Windows.Forms.Button();
            BtnClear = new System.Windows.Forms.Button();
            TextBoxInputOutput = new System.Windows.Forms.TextBox();
            ContextMenuText = new System.Windows.Forms.ContextMenuStrip(components);
            SearchMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ToolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            ReplaceMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            TsSep1 = new System.Windows.Forms.ToolStripSeparator();
            LoadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            SaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            PnlChunks = new System.Windows.Forms.Panel();
            LblChunkTitle = new System.Windows.Forms.Label();
            LblBlockSizeLabel = new System.Windows.Forms.Label();
            NumBlockSize = new System.Windows.Forms.NumericUpDown();
            BtnBuildChunks = new System.Windows.Forms.Button();
            SepChunk1 = new System.Windows.Forms.Label();
            LblTotalChunks = new System.Windows.Forms.Label();
            LblChunksLeft = new System.Windows.Forms.Label();
            LblCharsCopied = new System.Windows.Forms.Label();
            LblCharsRemaining = new System.Windows.Forms.Label();
            SepChunk2 = new System.Windows.Forms.Label();
            BtnCopyNextChunk = new System.Windows.Forms.Button();
            BtnResetChunks = new System.Windows.Forms.Button();
            LblChunkStatus = new System.Windows.Forms.Label();
            StatusStrip1 = new System.Windows.Forms.StatusStrip();
            LblCharCount = new System.Windows.Forms.ToolStripStatusLabel();
            LblStatusSep = new System.Windows.Forms.ToolStripStatusLabel();
            LblStatusHint = new System.Windows.Forms.ToolStripStatusLabel();
            PnlHeader.SuspendLayout();
            PnlToolbar.SuspendLayout();
            ContextMenuText.SuspendLayout();
            PnlChunks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NumBlockSize).BeginInit();
            StatusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // PnlHeader
            // 
            PnlHeader.Controls.Add(LblTitle);
            PnlHeader.Controls.Add(LblSubtitle);
            PnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            PnlHeader.Location = new System.Drawing.Point(0, 75);
            PnlHeader.Name = "PnlHeader";
            PnlHeader.Size = new System.Drawing.Size(1113, 75);
            PnlHeader.TabIndex = 1;
            // 
            // LblTitle
            // 
            LblTitle.AutoSize = true;
            LblTitle.Location = new System.Drawing.Point(14, 10);
            LblTitle.Name = "LblTitle";
            LblTitle.Size = new System.Drawing.Size(285, 15);
            LblTitle.TabIndex = 0;
            LblTitle.Text = "Paragraph-to-Line Converter  ·  AI Character Shrinker";
            // 
            // LblSubtitle
            // 
            LblSubtitle.AutoSize = true;
            LblSubtitle.ForeColor = System.Drawing.Color.FromArgb(90, 100, 115);
            LblSubtitle.Location = new System.Drawing.Point(16, 33);
            LblSubtitle.Name = "LblSubtitle";
            LblSubtitle.Size = new System.Drawing.Size(665, 30);
            LblSubtitle.TabIndex = 1;
            LblSubtitle.Text = resources.GetString("LblSubtitle.Text");
            // 
            // CmbAiModel
            // 
            CmbAiModel.FormattingEnabled = true;
            CmbAiModel.Items.AddRange(new object[] { "ChatGPT", "Claude", "Gemini", "DeepSeek" });
            CmbAiModel.Location = new System.Drawing.Point(168, 10);
            CmbAiModel.Name = "CmbAiModel";
            CmbAiModel.Size = new System.Drawing.Size(82, 23);
            CmbAiModel.TabIndex = 2;
            CmbAiModel.SelectedIndexChanged += CmbAiModel_SelectedIndexChanged;
            // 
            // BtnRemoveDuplicateLines
            // 
            BtnRemoveDuplicateLines.Location = new System.Drawing.Point(867, 44);
            BtnRemoveDuplicateLines.Name = "BtnRemoveDuplicateLines";
            BtnRemoveDuplicateLines.Size = new System.Drawing.Size(143, 25);
            BtnRemoveDuplicateLines.TabIndex = 8;
            BtnRemoveDuplicateLines.Text = "Remove Duplicate Lines";
            BtnRemoveDuplicateLines.Click += BtnRemoveDuplicateLines_Click;
            // 
            // BtnRemoveEmptyLines
            // 
            BtnRemoveEmptyLines.Location = new System.Drawing.Point(723, 44);
            BtnRemoveEmptyLines.Name = "BtnRemoveEmptyLines";
            BtnRemoveEmptyLines.Size = new System.Drawing.Size(143, 25);
            BtnRemoveEmptyLines.TabIndex = 7;
            BtnRemoveEmptyLines.Text = "Remove Empty Lines";
            BtnRemoveEmptyLines.Click += BtnRemoveEmptyLines_Click;
            // 
            // PnlToolbar
            // 
            PnlToolbar.BackColor = System.Drawing.Color.FromArgb(235, 240, 248);
            PnlToolbar.Controls.Add(BtnDarkMode);
            PnlToolbar.Controls.Add(BtnRedo);
            PnlToolbar.Controls.Add(BtnUndo);
            PnlToolbar.Controls.Add(BtnAiExplain);
            PnlToolbar.Controls.Add(BtnAiSafeMode);
            PnlToolbar.Controls.Add(BtnRemoveDuplicateLines);
            PnlToolbar.Controls.Add(LblConvertGroup);
            PnlToolbar.Controls.Add(BtnConvert);
            PnlToolbar.Controls.Add(BtnRemoveEmptyLines);
            PnlToolbar.Controls.Add(BtnConvert2);
            PnlToolbar.Controls.Add(BtnConvertSkipComments);
            PnlToolbar.Controls.Add(BtnNormalise);
            PnlToolbar.Controls.Add(ChkAddSpaces);
            PnlToolbar.Controls.Add(BtnCopyAll);
            PnlToolbar.Controls.Add(BtnRestore);
            PnlToolbar.Controls.Add(BtnClear);
            PnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            PnlToolbar.Location = new System.Drawing.Point(0, 0);
            PnlToolbar.Name = "PnlToolbar";
            PnlToolbar.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            PnlToolbar.Size = new System.Drawing.Size(1113, 75);
            PnlToolbar.TabIndex = 2;
            // 
            // BtnDarkMode
            // 
            BtnDarkMode.Location = new System.Drawing.Point(14, 43);
            BtnDarkMode.Name = "BtnDarkMode";
            BtnDarkMode.Size = new System.Drawing.Size(94, 26);
            BtnDarkMode.TabIndex = 13;
            BtnDarkMode.Text = "Dark Mode";
            BtnDarkMode.Click += BtnDarkMode_Click;
            // 
            // BtnRedo
            // 
            BtnRedo.Location = new System.Drawing.Point(1060, 44);
            BtnRedo.Name = "BtnRedo";
            BtnRedo.Size = new System.Drawing.Size(49, 25);
            BtnRedo.TabIndex = 12;
            BtnRedo.Text = "Redo";
            // 
            // BtnUndo
            // 
            BtnUndo.Location = new System.Drawing.Point(1010, 44);
            BtnUndo.Name = "BtnUndo";
            BtnUndo.Size = new System.Drawing.Size(49, 25);
            BtnUndo.TabIndex = 11;
            BtnUndo.Text = "Undo";
            // 
            // BtnAiExplain
            // 
            BtnAiExplain.Location = new System.Drawing.Point(628, 44);
            BtnAiExplain.Name = "BtnAiExplain";
            BtnAiExplain.Size = new System.Drawing.Size(94, 26);
            BtnAiExplain.TabIndex = 10;
            BtnAiExplain.Text = "AI‑Explain";
            BtnAiExplain.Click += BtnAiExplain_Click;
            // 
            // BtnAiSafeMode
            // 
            BtnAiSafeMode.Location = new System.Drawing.Point(534, 44);
            BtnAiSafeMode.Name = "BtnAiSafeMode";
            BtnAiSafeMode.Size = new System.Drawing.Size(94, 26);
            BtnAiSafeMode.TabIndex = 9;
            BtnAiSafeMode.Text = "AI‑Safe Mode";
            BtnAiSafeMode.Click += BtnAiSafeMode_Click;
            // 
            // LblConvertGroup
            // 
            LblConvertGroup.AutoSize = true;
            LblConvertGroup.ForeColor = System.Drawing.Color.FromArgb(60, 70, 85);
            LblConvertGroup.Location = new System.Drawing.Point(10, 17);
            LblConvertGroup.Name = "LblConvertGroup";
            LblConvertGroup.Size = new System.Drawing.Size(104, 15);
            LblConvertGroup.TabIndex = 0;
            LblConvertGroup.Text = "Conversion mode:";
            // 
            // BtnConvert
            // 
            BtnConvert.Location = new System.Drawing.Point(120, 12);
            BtnConvert.Name = "BtnConvert";
            BtnConvert.Size = new System.Drawing.Size(130, 26);
            BtnConvert.TabIndex = 1;
            BtnConvert.Text = "Mode A";
            BtnConvert.Click += BtnConvert_Click;
            // 
            // BtnConvert2
            // 
            BtnConvert2.Location = new System.Drawing.Point(258, 12);
            BtnConvert2.Name = "BtnConvert2";
            BtnConvert2.Size = new System.Drawing.Size(130, 26);
            BtnConvert2.TabIndex = 2;
            BtnConvert2.Text = "Mode B";
            BtnConvert2.Click += BtnConvert2_Click;
            // 
            // BtnConvertSkipComments
            // 
            BtnConvertSkipComments.Location = new System.Drawing.Point(396, 12);
            BtnConvertSkipComments.Name = "BtnConvertSkipComments";
            BtnConvertSkipComments.Size = new System.Drawing.Size(130, 26);
            BtnConvertSkipComments.TabIndex = 3;
            BtnConvertSkipComments.Text = "Mode C";
            BtnConvertSkipComments.Click += BtnConvertSkipComments_Click;
            // 
            // BtnNormalise
            // 
            BtnNormalise.Location = new System.Drawing.Point(534, 12);
            BtnNormalise.Name = "BtnNormalise";
            BtnNormalise.Size = new System.Drawing.Size(130, 26);
            BtnNormalise.TabIndex = 4;
            BtnNormalise.Text = "Mode D";
            BtnNormalise.Click += BtnNormalise_Click;
            // 
            // ChkAddSpaces
            // 
            ChkAddSpaces.AutoSize = true;
            ChkAddSpaces.Checked = true;
            ChkAddSpaces.CheckState = System.Windows.Forms.CheckState.Checked;
            ChkAddSpaces.Location = new System.Drawing.Point(672, 15);
            ChkAddSpaces.Name = "ChkAddSpaces";
            ChkAddSpaces.Size = new System.Drawing.Size(139, 19);
            ChkAddSpaces.TabIndex = 5;
            ChkAddSpaces.Text = "Add spaces (Mode A)";
            // 
            // BtnCopyAll
            // 
            BtnCopyAll.Location = new System.Drawing.Point(827, 12);
            BtnCopyAll.Name = "BtnCopyAll";
            BtnCopyAll.Size = new System.Drawing.Size(92, 26);
            BtnCopyAll.TabIndex = 6;
            BtnCopyAll.Text = "Copy All";
            BtnCopyAll.Click += BtnCopyAll_Click;
            // 
            // BtnRestore
            // 
            BtnRestore.Location = new System.Drawing.Point(925, 12);
            BtnRestore.Name = "BtnRestore";
            BtnRestore.Size = new System.Drawing.Size(92, 26);
            BtnRestore.TabIndex = 7;
            BtnRestore.Text = "↩ Restore";
            BtnRestore.Click += BtnRestore_Click;
            // 
            // BtnClear
            // 
            BtnClear.Location = new System.Drawing.Point(1017, 12);
            BtnClear.Name = "BtnClear";
            BtnClear.Size = new System.Drawing.Size(92, 26);
            BtnClear.TabIndex = 8;
            BtnClear.Text = "Clear";
            BtnClear.Click += BtnClear_Click;
            // 
            // TextBoxInputOutput
            // 
            TextBoxInputOutput.BackColor = System.Drawing.Color.FromArgb(252, 253, 255);
            TextBoxInputOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            TextBoxInputOutput.ContextMenuStrip = ContextMenuText;
            TextBoxInputOutput.Font = new System.Drawing.Font("Consolas", 9.5F);
            TextBoxInputOutput.Location = new System.Drawing.Point(8, 152);
            TextBoxInputOutput.MaxLength = 2000000;
            TextBoxInputOutput.Multiline = true;
            TextBoxInputOutput.Name = "TextBoxInputOutput";
            TextBoxInputOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            TextBoxInputOutput.Size = new System.Drawing.Size(830, 455);
            TextBoxInputOutput.TabIndex = 3;
            TextBoxInputOutput.WordWrap = false;
            TextBoxInputOutput.TextChanged += TextBoxInputOutput_TextChanged;
            // 
            // ContextMenuText
            // 
            ContextMenuText.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { SearchMenuItem, ReplaceMenuItem, TsSep1, LoadMenuItem, SaveMenuItem });
            ContextMenuText.Name = "ContextMenuText";
            ContextMenuText.Size = new System.Drawing.Size(158, 98);
            // 
            // SearchMenuItem
            // 
            SearchMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolStripTextBoxSearch });
            SearchMenuItem.Name = "SearchMenuItem";
            SearchMenuItem.Size = new System.Drawing.Size(157, 22);
            SearchMenuItem.Text = "Search…";
            // 
            // ToolStripTextBoxSearch
            // 
            ToolStripTextBoxSearch.Name = "ToolStripTextBoxSearch";
            ToolStripTextBoxSearch.Size = new System.Drawing.Size(100, 23);
            ToolStripTextBoxSearch.KeyDown += ToolStripTextBoxSearch_KeyDown;
            ToolStripTextBoxSearch.TextChanged += ToolStripTextBoxSearch_TextChanged;
            // 
            // ReplaceMenuItem
            // 
            ReplaceMenuItem.Name = "ReplaceMenuItem";
            ReplaceMenuItem.Size = new System.Drawing.Size(157, 22);
            ReplaceMenuItem.Text = "Find & Replace…";
            ReplaceMenuItem.Click += TextReplacementToolStripMenuItem_Click;
            // 
            // TsSep1
            // 
            TsSep1.Name = "TsSep1";
            TsSep1.Size = new System.Drawing.Size(154, 6);
            // 
            // LoadMenuItem
            // 
            LoadMenuItem.Name = "LoadMenuItem";
            LoadMenuItem.Size = new System.Drawing.Size(157, 22);
            LoadMenuItem.Text = "Load from file…";
            LoadMenuItem.Click += LoadToolStripMenuItem_Click;
            // 
            // SaveMenuItem
            // 
            SaveMenuItem.Name = "SaveMenuItem";
            SaveMenuItem.Size = new System.Drawing.Size(157, 22);
            SaveMenuItem.Text = "Save to file…";
            SaveMenuItem.Click += SaveToolStripMenuItem_Click;
            // 
            // PnlChunks
            // 
            PnlChunks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            PnlChunks.Controls.Add(CmbAiModel);
            PnlChunks.Controls.Add(LblChunkTitle);
            PnlChunks.Controls.Add(LblBlockSizeLabel);
            PnlChunks.Controls.Add(NumBlockSize);
            PnlChunks.Controls.Add(BtnBuildChunks);
            PnlChunks.Controls.Add(SepChunk1);
            PnlChunks.Controls.Add(LblTotalChunks);
            PnlChunks.Controls.Add(LblChunksLeft);
            PnlChunks.Controls.Add(LblCharsCopied);
            PnlChunks.Controls.Add(LblCharsRemaining);
            PnlChunks.Controls.Add(SepChunk2);
            PnlChunks.Controls.Add(BtnCopyNextChunk);
            PnlChunks.Controls.Add(BtnResetChunks);
            PnlChunks.Controls.Add(LblChunkStatus);
            PnlChunks.Location = new System.Drawing.Point(848, 154);
            PnlChunks.Name = "PnlChunks";
            PnlChunks.Size = new System.Drawing.Size(262, 455);
            PnlChunks.TabIndex = 4;
            // 
            // LblChunkTitle
            // 
            LblChunkTitle.AutoSize = true;
            LblChunkTitle.Location = new System.Drawing.Point(10, 10);
            LblChunkTitle.Name = "LblChunkTitle";
            LblChunkTitle.Size = new System.Drawing.Size(111, 15);
            LblChunkTitle.TabIndex = 0;
            LblChunkTitle.Text = "Build & Copy Chunks";
            // 
            // LblBlockSizeLabel
            // 
            LblBlockSizeLabel.AutoSize = true;
            LblBlockSizeLabel.Location = new System.Drawing.Point(10, 38);
            LblBlockSizeLabel.Name = "LblBlockSizeLabel";
            LblBlockSizeLabel.Size = new System.Drawing.Size(118, 15);
            LblBlockSizeLabel.TabIndex = 1;
            LblBlockSizeLabel.Text = "Characters per block:";
            // 
            // NumBlockSize
            // 
            NumBlockSize.Increment = new decimal(new int[] { 500, 0, 0, 0 });
            NumBlockSize.Location = new System.Drawing.Point(10, 56);
            NumBlockSize.Name = "NumBlockSize";
            NumBlockSize.Size = new System.Drawing.Size(100, 23);
            NumBlockSize.TabIndex = 2;
            NumBlockSize.ThousandsSeparator = true;
            NumBlockSize.ValueChanged += NumBlockSize_ValueChanged;
            // 
            // BtnBuildChunks
            // 
            BtnBuildChunks.Location = new System.Drawing.Point(118, 54);
            BtnBuildChunks.Name = "BtnBuildChunks";
            BtnBuildChunks.Size = new System.Drawing.Size(132, 26);
            BtnBuildChunks.TabIndex = 3;
            BtnBuildChunks.Text = "Build chunks";
            BtnBuildChunks.Click += BtnBuildChunks_Click;
            // 
            // SepChunk1
            // 
            SepChunk1.AutoSize = true;
            SepChunk1.Location = new System.Drawing.Point(10, 90);
            SepChunk1.Name = "SepChunk1";
            SepChunk1.Size = new System.Drawing.Size(0, 15);
            SepChunk1.TabIndex = 4;
            // 
            // LblTotalChunks
            // 
            LblTotalChunks.Location = new System.Drawing.Point(10, 106);
            LblTotalChunks.Name = "LblTotalChunks";
            LblTotalChunks.Size = new System.Drawing.Size(100, 23);
            LblTotalChunks.TabIndex = 5;
            // 
            // LblChunksLeft
            // 
            LblChunksLeft.Location = new System.Drawing.Point(10, 126);
            LblChunksLeft.Name = "LblChunksLeft";
            LblChunksLeft.Size = new System.Drawing.Size(100, 23);
            LblChunksLeft.TabIndex = 6;
            // 
            // LblCharsCopied
            // 
            LblCharsCopied.Location = new System.Drawing.Point(10, 146);
            LblCharsCopied.Name = "LblCharsCopied";
            LblCharsCopied.Size = new System.Drawing.Size(100, 23);
            LblCharsCopied.TabIndex = 7;
            // 
            // LblCharsRemaining
            // 
            LblCharsRemaining.Location = new System.Drawing.Point(10, 166);
            LblCharsRemaining.Name = "LblCharsRemaining";
            LblCharsRemaining.Size = new System.Drawing.Size(100, 23);
            LblCharsRemaining.TabIndex = 8;
            // 
            // SepChunk2
            // 
            SepChunk2.AutoSize = true;
            SepChunk2.Location = new System.Drawing.Point(10, 190);
            SepChunk2.Name = "SepChunk2";
            SepChunk2.Size = new System.Drawing.Size(0, 15);
            SepChunk2.TabIndex = 9;
            // 
            // BtnCopyNextChunk
            // 
            BtnCopyNextChunk.Enabled = false;
            BtnCopyNextChunk.Location = new System.Drawing.Point(10, 208);
            BtnCopyNextChunk.Name = "BtnCopyNextChunk";
            BtnCopyNextChunk.Size = new System.Drawing.Size(240, 34);
            BtnCopyNextChunk.TabIndex = 10;
            BtnCopyNextChunk.Text = "Copy the next chunk to clipboard";
            BtnCopyNextChunk.Click += BtnCopyNextChunk_Click;
            // 
            // BtnResetChunks
            // 
            BtnResetChunks.Location = new System.Drawing.Point(10, 250);
            BtnResetChunks.Name = "BtnResetChunks";
            BtnResetChunks.Size = new System.Drawing.Size(120, 26);
            BtnResetChunks.TabIndex = 11;
            BtnResetChunks.Text = "Reset the chunk";
            BtnResetChunks.Click += BtnResetChunks_Click;
            // 
            // LblChunkStatus
            // 
            LblChunkStatus.Location = new System.Drawing.Point(10, 286);
            LblChunkStatus.Name = "LblChunkStatus";
            LblChunkStatus.Size = new System.Drawing.Size(240, 40);
            LblChunkStatus.TabIndex = 12;
            // 
            // StatusStrip1
            // 
            StatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { LblCharCount, LblStatusSep, LblStatusHint });
            StatusStrip1.Location = new System.Drawing.Point(0, 615);
            StatusStrip1.Name = "StatusStrip1";
            StatusStrip1.Size = new System.Drawing.Size(1113, 22);
            StatusStrip1.TabIndex = 5;
            // 
            // LblCharCount
            // 
            LblCharCount.Name = "LblCharCount";
            LblCharCount.Size = new System.Drawing.Size(70, 17);
            LblCharCount.Text = "0 characters";
            // 
            // LblStatusSep
            // 
            LblStatusSep.Name = "LblStatusSep";
            LblStatusSep.Size = new System.Drawing.Size(22, 17);
            LblStatusSep.Text = "  |  ";
            // 
            // LblStatusHint
            // 
            LblStatusHint.Name = "LblStatusHint";
            LblStatusHint.Size = new System.Drawing.Size(363, 17);
            LblStatusHint.Text = "Right-click the text area for Search, Replace, Load and Save options.";
            // 
            // LineConverterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(243, 246, 250);
            ClientSize = new System.Drawing.Size(1113, 637);
            Controls.Add(PnlHeader);
            Controls.Add(PnlToolbar);
            Controls.Add(TextBoxInputOutput);
            Controls.Add(PnlChunks);
            Controls.Add(StatusStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(900, 540);
            Name = "LineConverterForm";
            Text = "Paragraph-to-Line Converter  –  AI Character Shrinker";
            PnlHeader.ResumeLayout(false);
            PnlHeader.PerformLayout();
            PnlToolbar.ResumeLayout(false);
            PnlToolbar.PerformLayout();
            ContextMenuText.ResumeLayout(false);
            PnlChunks.ResumeLayout(false);
            PnlChunks.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NumBlockSize).EndInit();
            StatusStrip1.ResumeLayout(false);
            StatusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        // -----------------------------------------------------------------------
        // Post‑Init Styling
        // -----------------------------------------------------------------------

        private void ApplyStyles()
        {
            // Toolbar buttons
            StyleToolbarButton(BtnConvert, "Mode A – Join Lines");
            StyleToolbarButton(BtnConvert2, "Mode B – Max Compress");
            StyleToolbarButton(BtnConvertSkipComments, "Mode C – Skip Comments");
            StyleToolbarButton(BtnNormalise, "Mode D – Normalise");

            // Utility buttons
            StyleUtilityButton(BtnCopyAll, "Copy All", System.Drawing.Color.FromArgb(235, 245, 235));
            StyleUtilityButton(BtnRestore, "↩ Restore", System.Drawing.Color.FromArgb(255, 248, 230));
            StyleUtilityButton(BtnClear, "Clear", System.Drawing.Color.FromArgb(255, 238, 238));
            BtnClear.ForeColor = System.Drawing.Color.FromArgb(180, 30, 30);

            // Chunk panel buttons
            BtnBuildChunks.Text = "Build & Split Text";
            BtnBuildChunks.BackColor = ClrAccent;
            BtnBuildChunks.ForeColor = System.Drawing.Color.White;
            BtnBuildChunks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnBuildChunks.FlatAppearance.BorderColor = ClrAccent;
            BtnBuildChunks.Font = FontBold;

            BtnCopyNextChunk.Text = "Copy Next Chunk  →";
            BtnCopyNextChunk.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            BtnCopyNextChunk.ForeColor = System.Drawing.Color.White;
            BtnCopyNextChunk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnCopyNextChunk.FlatAppearance.BorderColor = ClrAccent;
            BtnCopyNextChunk.Font = new System.Drawing.Font("Segoe UI Semibold", 10f);

            BtnResetChunks.Text = "Reset Session";
            BtnResetChunks.BackColor = System.Drawing.Color.WhiteSmoke;
            BtnResetChunks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            BtnResetChunks.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 190, 205);
            BtnResetChunks.Font = FontUi;

            // Chunk status
            LblChunkStatus.Text = "No chunks built yet.";
            LblChunkStatus.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Italic);
            LblChunkStatus.ForeColor = System.Drawing.Color.Gray;

            // Stats labels
            StyleStatLabel(LblTotalChunks);
            StyleStatLabel(LblChunksLeft);
            StyleStatLabel(LblCharsCopied);
            StyleStatLabel(LblCharsRemaining);
        }

        private void StyleToolbarButton(System.Windows.Forms.Button b, string text)
        {
            b.Text = text;
            b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            b.FlatAppearance.BorderColor = ClrAccent;
            b.BackColor = System.Drawing.Color.White;
            b.ForeColor = ClrAccent;
            b.Font = FontUi;
            b.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        private void StyleUtilityButton(System.Windows.Forms.Button b, string text, System.Drawing.Color bg)
        {
            b.Text = text;
            b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            b.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 190, 205);
            b.BackColor = bg;
            b.Font = FontUi;
            b.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        private void StyleStatLabel(System.Windows.Forms.Label lbl)
        {
            lbl.Font = FontUi;
            lbl.ForeColor = System.Drawing.Color.FromArgb(55, 65, 80);
            lbl.AutoSize = true;
        }

        // -----------------------------------------------------------------------
        // Separator lines (must NOT be created in InitializeComponent)
        // -----------------------------------------------------------------------

        private void CreateSeparators()
        {
            SepChunk1.Text = new string('─', 36);
            SepChunk1.Font = new System.Drawing.Font("Consolas", 8f);
            SepChunk1.ForeColor = ClrSep;

            SepChunk2.Text = new string('─', 36);
            SepChunk2.Font = new System.Drawing.Font("Consolas", 8f);
            SepChunk2.ForeColor = ClrSep;
        }

        // -----------------------------------------------------------------------
        // Chunk Help Label (dynamic control)
        // -----------------------------------------------------------------------

        private void CreateChunkHelpLabel()
        {
            System.Windows.Forms.Label help = new System.Windows.Forms.Label();
            help.Text =
                "HOW TO USE:\r\n" +
                "1. Paste or load your text.\r\n" +
                "2. Set characters per block.\r\n" +
                "3. Click 'Build & Split Text'.\r\n" +
                "4. Click 'Copy Next Chunk →'\r\n" +
                "   and paste into your AI chat.\r\n" +
                "5. Repeat until all chunks\r\n" +
                "   are copied.";
            help.Font = new System.Drawing.Font("Segoe UI", 8f);
            help.ForeColor = System.Drawing.Color.FromArgb(110, 120, 135);
            help.Location = new System.Drawing.Point(10, 340);
            help.Size = new System.Drawing.Size(240, 105);

            PnlChunks.Controls.Add(help);
        }

        // -----------------------------------------------------------------------
        // Event Wiring (Designer‑safe)
        // -----------------------------------------------------------------------

        private void WireEvents()
        {
            BtnConvert.Click += BtnConvert_Click;
            BtnConvert2.Click += BtnConvert2_Click;
            BtnConvertSkipComments.Click += BtnConvertSkipComments_Click;
            BtnNormalise.Click += BtnNormalise_Click;

            BtnCopyAll.Click += BtnCopyAll_Click;
            BtnRestore.Click += BtnRestore_Click;
            BtnClear.Click += BtnClear_Click;

            BtnBuildChunks.Click += BtnBuildChunks_Click;
            BtnCopyNextChunk.Click += BtnCopyNextChunk_Click;
            BtnResetChunks.Click += BtnResetChunks_Click;

            ReplaceMenuItem.Click += TextReplacementToolStripMenuItem_Click;
            LoadMenuItem.Click += LoadToolStripMenuItem_Click;
            SaveMenuItem.Click += SaveToolStripMenuItem_Click;
            ToolStripTextBoxSearch.TextChanged += ToolStripTextBoxSearch_TextChanged;
        }

        // -----------------------------------------------------------------------
        // Control Declarations (Designer‑required)
        // -----------------------------------------------------------------------

        private System.Windows.Forms.Panel PnlHeader;
        private System.Windows.Forms.Label LblTitle;
        private System.Windows.Forms.Label LblSubtitle;

        private System.Windows.Forms.Panel PnlToolbar;
        private System.Windows.Forms.Label LblConvertGroup;
        private System.Windows.Forms.Button BtnConvert;
        private System.Windows.Forms.Button BtnConvert2;
        private System.Windows.Forms.Button BtnConvertSkipComments;
        private System.Windows.Forms.Button BtnNormalise;
        private System.Windows.Forms.CheckBox ChkAddSpaces;
        private System.Windows.Forms.Button BtnCopyAll;
        private System.Windows.Forms.Button BtnRestore;
        private System.Windows.Forms.Button BtnClear;

        private System.Windows.Forms.TextBox TextBoxInputOutput;

        private System.Windows.Forms.Panel PnlChunks;
        private System.Windows.Forms.Label LblChunkTitle;
        private System.Windows.Forms.Label LblBlockSizeLabel;
        private System.Windows.Forms.NumericUpDown NumBlockSize;
        private System.Windows.Forms.Button BtnBuildChunks;
        private System.Windows.Forms.Label SepChunk1;
        private System.Windows.Forms.Label LblTotalChunks;
        private System.Windows.Forms.Label LblChunksLeft;
        private System.Windows.Forms.Label LblCharsCopied;
        private System.Windows.Forms.Label LblCharsRemaining;
        private System.Windows.Forms.Label SepChunk2;
        private System.Windows.Forms.Button BtnCopyNextChunk;
        private System.Windows.Forms.Button BtnResetChunks;
        private System.Windows.Forms.Label LblChunkStatus;

        private System.Windows.Forms.StatusStrip StatusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel LblCharCount;
        private System.Windows.Forms.ToolStripStatusLabel LblStatusSep;
        private System.Windows.Forms.ToolStripStatusLabel LblStatusHint;

        private System.Windows.Forms.ContextMenuStrip ContextMenuText;
        private System.Windows.Forms.ToolStripMenuItem SearchMenuItem;
        private System.Windows.Forms.ToolStripTextBox ToolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripMenuItem ReplaceMenuItem;
        private System.Windows.Forms.ToolStripSeparator TsSep1;
        private System.Windows.Forms.ToolStripMenuItem LoadMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveMenuItem;
        private System.Windows.Forms.ComboBox CmbAiModel;
        private System.Windows.Forms.Button BtnRemoveDuplicateLines;
        private System.Windows.Forms.Button BtnRemoveEmptyLines;
        private System.Windows.Forms.Button BtnAiSafeMode;
        private System.Windows.Forms.Button BtnAiExplain;
        private System.Windows.Forms.Button BtnRedo;
        private System.Windows.Forms.Button BtnUndo;
        private System.Windows.Forms.Button BtnDarkMode;
    }
}
