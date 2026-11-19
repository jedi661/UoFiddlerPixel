/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Controls.Forms
{
    partial class LandTextureStudioForm
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
                components?.Dispose();
                DisposeImages();
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
            this.components = new System.ComponentModel.Container();
            this._loadFileButton = new System.Windows.Forms.Button();
            this._pasteButton = new System.Windows.Forms.Button();
            this._fromLandLabel = new System.Windows.Forms.Label();
            this._fromLandBox = new System.Windows.Forms.TextBox();
            this._fromLandButton = new System.Windows.Forms.Button();
            this._fromTexLabel = new System.Windows.Forms.Label();
            this._fromTexBox = new System.Windows.Forms.TextBox();
            this._fromTexButton = new System.Windows.Forms.Button();
            this._modeLabel = new System.Windows.Forms.Label();
            this._modeBox = new System.Windows.Forms.ComboBox();
            this._fitLabel = new System.Windows.Forms.Label();
            this._fitBox = new System.Windows.Forms.ComboBox();
            this._sourceBox = new System.Windows.Forms.PictureBox();
            this._sourceTop = new System.Windows.Forms.FlowLayoutPanel();
            this._sourceMode = new System.Windows.Forms.FlowLayoutPanel();
            this._sourceInfo = new System.Windows.Forms.Label();
            this._sourceLayout = new System.Windows.Forms.TableLayoutPanel();
            this._sourceGroup = new System.Windows.Forms.GroupBox();
            this._rotLeftButton = new System.Windows.Forms.Button();
            this._rotRightButton = new System.Windows.Forms.Button();
            this._mirrorHButton = new System.Windows.Forms.Button();
            this._mirrorVButton = new System.Windows.Forms.Button();
            this._resetOrientButton = new System.Windows.Forms.Button();
            this._alignLabel = new System.Windows.Forms.Label();
            this._alignBox = new System.Windows.Forms.TextBox();
            this._alignButton = new System.Windows.Forms.Button();
            this._sizeLabel = new System.Windows.Forms.Label();
            this._sizeBox = new System.Windows.Forms.ComboBox();
            this._orientFlow = new System.Windows.Forms.FlowLayoutPanel();
            this._texCaption = new System.Windows.Forms.Label();
            this._textureBox = new System.Windows.Forms.PictureBox();
            this._landCaption = new System.Windows.Forms.Label();
            this._landBox = new System.Windows.Forms.PictureBox();
            this._mapCaption = new System.Windows.Forms.Label();
            this._mapBox = new System.Windows.Forms.PictureBox();
            this._matchLabel = new System.Windows.Forms.Label();
            this._resultGrid = new System.Windows.Forms.TableLayoutPanel();
            this._resultLayout = new System.Windows.Forms.TableLayoutPanel();
            this._resultGroup = new System.Windows.Forms.GroupBox();
            this._landIdLabel = new System.Windows.Forms.Label();
            this._landIdBox = new System.Windows.Forms.TextBox();
            this._landFreeButton = new System.Windows.Forms.Button();
            this._landFreeExtButton = new System.Windows.Forms.Button();
            this._landIdInfo = new System.Windows.Forms.Label();
            this._texIdLabel = new System.Windows.Forms.Label();
            this._texIdBox = new System.Windows.Forms.TextBox();
            this._texFreeButton = new System.Windows.Forms.Button();
            this._sameIdCheck = new System.Windows.Forms.CheckBox();
            this._texIdInfo = new System.Windows.Forms.Label();
            this._nameLabel = new System.Windows.Forms.Label();
            this._nameBox = new System.Windows.Forms.TextBox();
            this._flagsLabel = new System.Windows.Forms.Label();
            this._flagsFromBox = new System.Windows.Forms.TextBox();
            this._writeLandCheck = new System.Windows.Forms.CheckBox();
            this._writeTexCheck = new System.Windows.Forms.CheckBox();
            this._radarCheck = new System.Windows.Forms.CheckBox();
            this._patchButton = new System.Windows.Forms.Button();
            this._saveButton = new System.Windows.Forms.Button();
            this._showLandButton = new System.Windows.Forms.Button();
            this._showTexButton = new System.Windows.Forms.Button();
            this._showTileDataButton = new System.Windows.Forms.Button();
            this._linkLabel = new System.Windows.Forms.Label();
            this._statusLabel = new System.Windows.Forms.Label();
            this._targetFlow = new System.Windows.Forms.FlowLayoutPanel();
            this._targetGroup = new System.Windows.Forms.GroupBox();
            this._rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this._tips = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this._sourceBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._textureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._landBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._mapBox)).BeginInit();
            this._sourceTop.SuspendLayout();
            this._sourceMode.SuspendLayout();
            this._sourceLayout.SuspendLayout();
            this._sourceGroup.SuspendLayout();
            this._orientFlow.SuspendLayout();
            this._resultGrid.SuspendLayout();
            this._resultLayout.SuspendLayout();
            this._resultGroup.SuspendLayout();
            this._targetFlow.SuspendLayout();
            this._targetGroup.SuspendLayout();
            this._rootLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // _loadFileButton
            // 
            this._loadFileButton.AutoSize = true;
            this._loadFileButton.Name = "_loadFileButton";
            this._loadFileButton.Text = "Load image...";
            this._loadFileButton.UseVisualStyleBackColor = true;
            this._loadFileButton.Click += new System.EventHandler(this.OnLoadFile);
            // 
            // _pasteButton
            // 
            this._pasteButton.AutoSize = true;
            this._pasteButton.Name = "_pasteButton";
            this._pasteButton.Text = "Paste (clipboard)";
            this._pasteButton.UseVisualStyleBackColor = true;
            this._pasteButton.Click += new System.EventHandler(this.OnPaste);
            // 
            // _fromLandLabel
            // 
            this._fromLandLabel.AutoSize = true;
            this._fromLandLabel.Name = "_fromLandLabel";
            this._fromLandLabel.Text = "From land tile:";
            this._fromLandLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _fromLandBox
            // 
            this._fromLandBox.Name = "_fromLandBox";
            this._fromLandBox.Width = 70;
            this._fromLandBox.Text = "0x3";
            // 
            // _fromLandButton
            // 
            this._fromLandButton.AutoSize = true;
            this._fromLandButton.Name = "_fromLandButton";
            this._fromLandButton.Text = "Load";
            this._fromLandButton.UseVisualStyleBackColor = true;
            this._fromLandButton.Click += new System.EventHandler(this.OnLoadFromLand);
            // 
            // _fromTexLabel
            // 
            this._fromTexLabel.AutoSize = true;
            this._fromTexLabel.Name = "_fromTexLabel";
            this._fromTexLabel.Text = "From texture:";
            this._fromTexLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _fromTexBox
            // 
            this._fromTexBox.Name = "_fromTexBox";
            this._fromTexBox.Width = 70;
            this._fromTexBox.Text = "0x3";
            // 
            // _fromTexButton
            // 
            this._fromTexButton.AutoSize = true;
            this._fromTexButton.Name = "_fromTexButton";
            this._fromTexButton.Text = "Load";
            this._fromTexButton.UseVisualStyleBackColor = true;
            this._fromTexButton.Click += new System.EventHandler(this.OnLoadFromTexture);
            // 
            // _modeLabel
            // 
            this._modeLabel.AutoSize = true;
            this._modeLabel.Name = "_modeLabel";
            this._modeLabel.Text = "The image is:";
            this._modeLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _modeBox
            // 
            this._modeBox.Name = "_modeBox";
            this._modeBox.Width = 300;
            this._modeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._modeBox.Items.AddRange(new object[] { "a texture (square, seen from above)", "a land tile (44x44 diamond)" });
            this._modeBox.SelectedIndexChanged += new System.EventHandler(this.OnSettingsChanged);
            // 
            // _fitLabel
            // 
            this._fitLabel.AutoSize = true;
            this._fitLabel.Name = "_fitLabel";
            this._fitLabel.Text = "Fit:";
            this._fitLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _fitBox
            // 
            this._fitBox.Name = "_fitBox";
            this._fitBox.Width = 200;
            this._fitBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._fitBox.Items.AddRange(new object[] { "crop the middle square", "stretch the whole image" });
            this._fitBox.SelectedIndexChanged += new System.EventHandler(this.OnSettingsChanged);
            // 
            // _sourceBox
            // 
            this._sourceBox.Name = "_sourceBox";
            this._sourceBox.Size = new System.Drawing.Size(260, 260);
            this._sourceBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._sourceBox.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this._sourceBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._sourceBox.TabStop = false;
            // 
            // _sourceTop
            // 
            this._sourceTop.Name = "_sourceTop";
            this._sourceTop.AutoSize = true;
            this._sourceTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sourceTop.WrapContents = true;
            this._sourceTop.Controls.Add(this._loadFileButton);
            this._sourceTop.Controls.Add(this._pasteButton);
            this._sourceTop.Controls.Add(this._fromLandLabel);
            this._sourceTop.Controls.Add(this._fromLandBox);
            this._sourceTop.Controls.Add(this._fromLandButton);
            this._sourceTop.Controls.Add(this._fromTexLabel);
            this._sourceTop.Controls.Add(this._fromTexBox);
            this._sourceTop.Controls.Add(this._fromTexButton);
            this._sourceTop.SetFlowBreak(this._pasteButton, true);
            this._sourceTop.SetFlowBreak(this._fromLandButton, true);
            // 
            // _sourceMode
            // 
            this._sourceMode.Name = "_sourceMode";
            this._sourceMode.AutoSize = true;
            this._sourceMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sourceMode.WrapContents = true;
            this._sourceMode.Controls.Add(this._modeLabel);
            this._sourceMode.Controls.Add(this._modeBox);
            this._sourceMode.Controls.Add(this._fitLabel);
            this._sourceMode.Controls.Add(this._fitBox);
            this._sourceMode.SetFlowBreak(this._modeBox, true);
            // 
            // _sourceInfo
            // 
            this._sourceInfo.AutoSize = true;
            this._sourceInfo.Name = "_sourceInfo";
            this._sourceInfo.Text = "no image loaded";
            this._sourceInfo.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._sourceInfo.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _sourceLayout
            // 
            this._sourceLayout.Name = "_sourceLayout";
            this._sourceLayout.ColumnCount = 1;
            this._sourceLayout.RowCount = 4;
            this._sourceLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sourceLayout.AutoSize = true;
            this._sourceLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sourceLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sourceLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sourceLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sourceLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sourceLayout.Controls.Add(this._sourceTop, 0, 0);
            this._sourceLayout.Controls.Add(this._sourceMode, 0, 1);
            this._sourceLayout.Controls.Add(this._sourceBox, 0, 2);
            this._sourceLayout.Controls.Add(this._sourceInfo, 0, 3);
            // 
            // _sourceGroup
            // 
            this._sourceGroup.Name = "_sourceGroup";
            this._sourceGroup.Text = "1. Source";
            this._sourceGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sourceGroup.AutoSize = true;
            this._sourceGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._sourceGroup.Controls.Add(this._sourceLayout);
            // 
            // _rotLeftButton
            // 
            this._rotLeftButton.AutoSize = true;
            this._rotLeftButton.Name = "_rotLeftButton";
            this._rotLeftButton.Text = "Turn left";
            this._rotLeftButton.UseVisualStyleBackColor = true;
            this._rotLeftButton.Click += new System.EventHandler(this.OnRotateLeft);
            // 
            // _rotRightButton
            // 
            this._rotRightButton.AutoSize = true;
            this._rotRightButton.Name = "_rotRightButton";
            this._rotRightButton.Text = "Turn right";
            this._rotRightButton.UseVisualStyleBackColor = true;
            this._rotRightButton.Click += new System.EventHandler(this.OnRotateRight);
            // 
            // _mirrorHButton
            // 
            this._mirrorHButton.AutoSize = true;
            this._mirrorHButton.Name = "_mirrorHButton";
            this._mirrorHButton.Text = "Mirror <->";
            this._mirrorHButton.UseVisualStyleBackColor = true;
            this._mirrorHButton.Click += new System.EventHandler(this.OnMirrorH);
            // 
            // _mirrorVButton
            // 
            this._mirrorVButton.AutoSize = true;
            this._mirrorVButton.Name = "_mirrorVButton";
            this._mirrorVButton.Text = "Mirror up/down";
            this._mirrorVButton.UseVisualStyleBackColor = true;
            this._mirrorVButton.Click += new System.EventHandler(this.OnMirrorV);
            // 
            // _resetOrientButton
            // 
            this._resetOrientButton.AutoSize = true;
            this._resetOrientButton.Name = "_resetOrientButton";
            this._resetOrientButton.Text = "Reset";
            this._resetOrientButton.UseVisualStyleBackColor = true;
            this._resetOrientButton.Click += new System.EventHandler(this.OnResetOrientation);
            // 
            // _alignLabel
            // 
            this._alignLabel.AutoSize = true;
            this._alignLabel.Name = "_alignLabel";
            this._alignLabel.Text = "Align to land tile:";
            this._alignLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _alignBox
            // 
            this._alignBox.Name = "_alignBox";
            this._alignBox.Width = 70;
            this._alignBox.Text = "";
            // 
            // _alignButton
            // 
            this._alignButton.AutoSize = true;
            this._alignButton.Name = "_alignButton";
            this._alignButton.Text = "Auto align";
            this._alignButton.UseVisualStyleBackColor = true;
            this._alignButton.Click += new System.EventHandler(this.OnAutoAlign);
            // 
            // _sizeLabel
            // 
            this._sizeLabel.AutoSize = true;
            this._sizeLabel.Name = "_sizeLabel";
            this._sizeLabel.Text = "Texture size:";
            this._sizeLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _sizeBox
            // 
            this._sizeBox.Name = "_sizeBox";
            this._sizeBox.Width = 90;
            this._sizeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._sizeBox.Items.AddRange(new object[] { "64x64", "128x128" });
            this._sizeBox.SelectedIndexChanged += new System.EventHandler(this.OnSettingsChanged);
            // 
            // _orientFlow
            // 
            this._orientFlow.Name = "_orientFlow";
            this._orientFlow.AutoSize = true;
            this._orientFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._orientFlow.WrapContents = true;
            this._orientFlow.Controls.Add(this._rotLeftButton);
            this._orientFlow.Controls.Add(this._rotRightButton);
            this._orientFlow.Controls.Add(this._mirrorHButton);
            this._orientFlow.Controls.Add(this._mirrorVButton);
            this._orientFlow.Controls.Add(this._resetOrientButton);
            this._orientFlow.Controls.Add(this._alignLabel);
            this._orientFlow.Controls.Add(this._alignBox);
            this._orientFlow.Controls.Add(this._alignButton);
            this._orientFlow.Controls.Add(this._sizeLabel);
            this._orientFlow.Controls.Add(this._sizeBox);
            this._orientFlow.SetFlowBreak(this._resetOrientButton, true);
            this._orientFlow.SetFlowBreak(this._alignButton, true);
            // 
            // _texCaption
            // 
            this._texCaption.AutoSize = true;
            this._texCaption.Name = "_texCaption";
            this._texCaption.Text = "Texture (texmaps)";
            this._texCaption.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._texCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            // 
            // _textureBox
            // 
            this._textureBox.Name = "_textureBox";
            this._textureBox.Size = new System.Drawing.Size(260, 260);
            this._textureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._textureBox.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this._textureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._textureBox.TabStop = false;
            // 
            // _landCaption
            // 
            this._landCaption.AutoSize = true;
            this._landCaption.Name = "_landCaption";
            this._landCaption.Text = "Land tile (art, 44x44) x4";
            this._landCaption.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._landCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            // 
            // _landBox
            // 
            this._landBox.Name = "_landBox";
            this._landBox.Size = new System.Drawing.Size(184, 184);
            this._landBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._landBox.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this._landBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._landBox.TabStop = false;
            // 
            // _mapCaption
            // 
            this._mapCaption.AutoSize = true;
            this._mapCaption.Name = "_mapCaption";
            this._mapCaption.Text = "On the map (3 x 3 tiles) - is it seamless?";
            this._mapCaption.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._mapCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            // 
            // _mapBox
            // 
            this._mapBox.Name = "_mapBox";
            this._mapBox.Size = new System.Drawing.Size(270, 270);
            this._mapBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._mapBox.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this._mapBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._mapBox.TabStop = false;
            // 
            // _matchLabel
            // 
            this._matchLabel.AutoSize = true;
            this._matchLabel.Name = "_matchLabel";
            this._matchLabel.Text = "";
            this._matchLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._matchLabel.ForeColor = System.Drawing.Color.DimGray;
            this._matchLabel.MaximumSize = new System.Drawing.Size(560, 0);
            // 
            // _resultGrid
            // 
            this._resultGrid.Name = "_resultGrid";
            this._resultGrid.ColumnCount = 2;
            this._resultGrid.RowCount = 4;
            this._resultGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._resultGrid.AutoSize = true;
            this._resultGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._resultGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._resultGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._resultGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._resultGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._resultGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._resultGrid.Controls.Add(this._texCaption, 0, 0);
            this._resultGrid.Controls.Add(this._landCaption, 1, 0);
            this._resultGrid.Controls.Add(this._textureBox, 0, 1);
            this._resultGrid.Controls.Add(this._landBox, 1, 1);
            this._resultGrid.Controls.Add(this._mapCaption, 0, 2);
            this._resultGrid.Controls.Add(this._mapBox, 0, 3);
            this._resultGrid.SetColumnSpan(this._mapCaption, 2);
            this._resultGrid.SetColumnSpan(this._mapBox, 2);
            // 
            // _resultLayout
            // 
            this._resultLayout.Name = "_resultLayout";
            this._resultLayout.ColumnCount = 1;
            this._resultLayout.RowCount = 3;
            this._resultLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._resultLayout.AutoSize = true;
            this._resultLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._resultLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._resultLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._resultLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._resultLayout.Controls.Add(this._orientFlow, 0, 0);
            this._resultLayout.Controls.Add(this._resultGrid, 0, 1);
            this._resultLayout.Controls.Add(this._matchLabel, 0, 2);
            // 
            // _resultGroup
            // 
            this._resultGroup.Name = "_resultGroup";
            this._resultGroup.Text = "2. Orientation and result";
            this._resultGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this._resultGroup.AutoSize = true;
            this._resultGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._resultGroup.Controls.Add(this._resultLayout);
            // 
            // _landIdLabel
            // 
            this._landIdLabel.AutoSize = true;
            this._landIdLabel.Name = "_landIdLabel";
            this._landIdLabel.Text = "Land tile id:";
            this._landIdLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _landIdBox
            // 
            this._landIdBox.Name = "_landIdBox";
            this._landIdBox.Width = 90;
            this._landIdBox.Text = "";
            this._landIdBox.TextChanged += new System.EventHandler(this.OnTargetChanged);
            // 
            // _landFreeButton
            // 
            this._landFreeButton.AutoSize = true;
            this._landFreeButton.Name = "_landFreeButton";
            this._landFreeButton.Text = "Next free";
            this._landFreeButton.UseVisualStyleBackColor = true;
            this._landFreeButton.Click += new System.EventHandler(this.OnLandNextFree);
            // 
            // _landFreeExtButton
            // 
            this._landFreeExtButton.AutoSize = true;
            this._landFreeExtButton.Name = "_landFreeExtButton";
            this._landFreeExtButton.Text = "Next free (0x4000+)";
            this._landFreeExtButton.UseVisualStyleBackColor = true;
            this._landFreeExtButton.Click += new System.EventHandler(this.OnLandNextFreeExtended);
            // 
            // _landIdInfo
            // 
            this._landIdInfo.AutoSize = true;
            this._landIdInfo.Name = "_landIdInfo";
            this._landIdInfo.Text = "";
            this._landIdInfo.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._landIdInfo.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _texIdLabel
            // 
            this._texIdLabel.AutoSize = true;
            this._texIdLabel.Name = "_texIdLabel";
            this._texIdLabel.Text = "Texture id:";
            this._texIdLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _texIdBox
            // 
            this._texIdBox.Name = "_texIdBox";
            this._texIdBox.Width = 90;
            this._texIdBox.Text = "";
            this._texIdBox.TextChanged += new System.EventHandler(this.OnTargetChanged);
            // 
            // _texFreeButton
            // 
            this._texFreeButton.AutoSize = true;
            this._texFreeButton.Name = "_texFreeButton";
            this._texFreeButton.Text = "Next free";
            this._texFreeButton.UseVisualStyleBackColor = true;
            this._texFreeButton.Click += new System.EventHandler(this.OnTexNextFree);
            // 
            // _sameIdCheck
            // 
            this._sameIdCheck.AutoSize = true;
            this._sameIdCheck.Name = "_sameIdCheck";
            this._sameIdCheck.Text = "Texture id = land tile id";
            this._sameIdCheck.Checked = true;
            this._sameIdCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this._sameIdCheck.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._sameIdCheck.UseVisualStyleBackColor = true;
            this._sameIdCheck.CheckedChanged += new System.EventHandler(this.OnSameIdChanged);
            // 
            // _texIdInfo
            // 
            this._texIdInfo.AutoSize = true;
            this._texIdInfo.Name = "_texIdInfo";
            this._texIdInfo.Text = "";
            this._texIdInfo.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._texIdInfo.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _nameLabel
            // 
            this._nameLabel.AutoSize = true;
            this._nameLabel.Name = "_nameLabel";
            this._nameLabel.Text = "TileData name:";
            this._nameLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _nameBox
            // 
            this._nameBox.Name = "_nameBox";
            this._nameBox.Width = 160;
            this._nameBox.Text = "";
            this._nameBox.MaxLength = 20;
            // 
            // _flagsLabel
            // 
            this._flagsLabel.AutoSize = true;
            this._flagsLabel.Name = "_flagsLabel";
            this._flagsLabel.Text = "Copy flags from land id:";
            this._flagsLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _flagsFromBox
            // 
            this._flagsFromBox.Name = "_flagsFromBox";
            this._flagsFromBox.Width = 70;
            this._flagsFromBox.Text = "";
            // 
            // _writeLandCheck
            // 
            this._writeLandCheck.AutoSize = true;
            this._writeLandCheck.Name = "_writeLandCheck";
            this._writeLandCheck.Text = "Write the land tile";
            this._writeLandCheck.Checked = true;
            this._writeLandCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this._writeLandCheck.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._writeLandCheck.UseVisualStyleBackColor = true;
            // 
            // _writeTexCheck
            // 
            this._writeTexCheck.AutoSize = true;
            this._writeTexCheck.Name = "_writeTexCheck";
            this._writeTexCheck.Text = "Write the texture";
            this._writeTexCheck.Checked = true;
            this._writeTexCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this._writeTexCheck.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._writeTexCheck.UseVisualStyleBackColor = true;
            // 
            // _radarCheck
            // 
            this._radarCheck.AutoSize = true;
            this._radarCheck.Name = "_radarCheck";
            this._radarCheck.Text = "Radar colour from the average colour";
            this._radarCheck.Checked = true;
            this._radarCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this._radarCheck.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._radarCheck.UseVisualStyleBackColor = true;
            // 
            // _patchButton
            // 
            this._patchButton.AutoSize = true;
            this._patchButton.Name = "_patchButton";
            this._patchButton.Text = "Patch (land tile + texture + tiledata link)";
            this._patchButton.UseVisualStyleBackColor = true;
            this._patchButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._patchButton.BackColor = System.Drawing.Color.FromArgb(220, 240, 220);
            this._patchButton.Click += new System.EventHandler(this.OnPatch);
            // 
            // _saveButton
            // 
            this._saveButton.AutoSize = true;
            this._saveButton.Name = "_saveButton";
            this._saveButton.Text = "Save files...";
            this._saveButton.UseVisualStyleBackColor = true;
            this._saveButton.Click += new System.EventHandler(this.OnSaveFiles);
            // 
            // _showLandButton
            // 
            this._showLandButton.AutoSize = true;
            this._showLandButton.Name = "_showLandButton";
            this._showLandButton.Text = "Show in LandTiles";
            this._showLandButton.UseVisualStyleBackColor = true;
            this._showLandButton.Click += new System.EventHandler(this.OnShowLand);
            // 
            // _showTexButton
            // 
            this._showTexButton.AutoSize = true;
            this._showTexButton.Name = "_showTexButton";
            this._showTexButton.Text = "Show in Textures";
            this._showTexButton.UseVisualStyleBackColor = true;
            this._showTexButton.Click += new System.EventHandler(this.OnShowTexture);
            // 
            // _showTileDataButton
            // 
            this._showTileDataButton.AutoSize = true;
            this._showTileDataButton.Name = "_showTileDataButton";
            this._showTileDataButton.Text = "Show in TileData";
            this._showTileDataButton.UseVisualStyleBackColor = true;
            this._showTileDataButton.Click += new System.EventHandler(this.OnShowTileData);
            // 
            // _linkLabel
            // 
            this._linkLabel.AutoSize = true;
            this._linkLabel.Name = "_linkLabel";
            this._linkLabel.Text = "";
            this._linkLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._linkLabel.MaximumSize = new System.Drawing.Size(1100, 0);
            // 
            // _statusLabel
            // 
            this._statusLabel.AutoSize = true;
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Text = "";
            this._statusLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._statusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._statusLabel.MaximumSize = new System.Drawing.Size(1100, 0);
            // 
            // _targetFlow
            // 
            this._targetFlow.Name = "_targetFlow";
            this._targetFlow.AutoSize = true;
            this._targetFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._targetFlow.WrapContents = true;
            this._targetFlow.Controls.Add(this._landIdLabel);
            this._targetFlow.Controls.Add(this._landIdBox);
            this._targetFlow.Controls.Add(this._landFreeButton);
            this._targetFlow.Controls.Add(this._landFreeExtButton);
            this._targetFlow.Controls.Add(this._landIdInfo);
            this._targetFlow.Controls.Add(this._texIdLabel);
            this._targetFlow.Controls.Add(this._texIdBox);
            this._targetFlow.Controls.Add(this._texFreeButton);
            this._targetFlow.Controls.Add(this._sameIdCheck);
            this._targetFlow.Controls.Add(this._texIdInfo);
            this._targetFlow.Controls.Add(this._nameLabel);
            this._targetFlow.Controls.Add(this._nameBox);
            this._targetFlow.Controls.Add(this._flagsLabel);
            this._targetFlow.Controls.Add(this._flagsFromBox);
            this._targetFlow.Controls.Add(this._writeLandCheck);
            this._targetFlow.Controls.Add(this._writeTexCheck);
            this._targetFlow.Controls.Add(this._radarCheck);
            this._targetFlow.Controls.Add(this._patchButton);
            this._targetFlow.Controls.Add(this._saveButton);
            this._targetFlow.Controls.Add(this._showLandButton);
            this._targetFlow.Controls.Add(this._showTexButton);
            this._targetFlow.Controls.Add(this._showTileDataButton);
            this._targetFlow.Controls.Add(this._linkLabel);
            this._targetFlow.Controls.Add(this._statusLabel);
            this._targetFlow.SetFlowBreak(this._landIdInfo, true);
            this._targetFlow.SetFlowBreak(this._texIdInfo, true);
            this._targetFlow.SetFlowBreak(this._radarCheck, true);
            this._targetFlow.SetFlowBreak(this._showTileDataButton, true);
            this._targetFlow.SetFlowBreak(this._linkLabel, true);
            // 
            // _targetGroup
            // 
            this._targetGroup.Name = "_targetGroup";
            this._targetGroup.Text = "3. Target slots and TileData link";
            this._targetGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this._targetGroup.AutoSize = true;
            this._targetGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._targetGroup.Controls.Add(this._targetFlow);
            // 
            // _rootLayout
            // 
            this._rootLayout.Name = "_rootLayout";
            this._rootLayout.ColumnCount = 2;
            this._rootLayout.RowCount = 2;
            this._rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._sourceGroup, 0, 0);
            this._rootLayout.Controls.Add(this._resultGroup, 1, 0);
            this._rootLayout.Controls.Add(this._targetGroup, 0, 1);
            this._rootLayout.SetColumnSpan(this._targetGroup, 2);
            this._rootLayout.Padding = new System.Windows.Forms.Padding(6);
            // 
            // LandTextureStudioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1180, 840);
            this.Controls.Add(this._rootLayout);
            this.Name = "LandTextureStudioForm";
            this.Text = "Land tile / texture studio";
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ShowIcon = false;
            this.AutoScroll = true;
            ((System.ComponentModel.ISupportInitialize)(this._sourceBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._textureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._landBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._mapBox)).EndInit();
            this._rootLayout.ResumeLayout(false);
            this._rootLayout.PerformLayout();
            this._targetGroup.ResumeLayout(false);
            this._targetGroup.PerformLayout();
            this._targetFlow.ResumeLayout(false);
            this._targetFlow.PerformLayout();
            this._resultGroup.ResumeLayout(false);
            this._resultGroup.PerformLayout();
            this._resultLayout.ResumeLayout(false);
            this._resultLayout.PerformLayout();
            this._resultGrid.ResumeLayout(false);
            this._resultGrid.PerformLayout();
            this._orientFlow.ResumeLayout(false);
            this._orientFlow.PerformLayout();
            this._sourceGroup.ResumeLayout(false);
            this._sourceGroup.PerformLayout();
            this._sourceLayout.ResumeLayout(false);
            this._sourceLayout.PerformLayout();
            this._sourceMode.ResumeLayout(false);
            this._sourceMode.PerformLayout();
            this._sourceTop.ResumeLayout(false);
            this._sourceTop.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button _loadFileButton;
        private System.Windows.Forms.Button _pasteButton;
        private System.Windows.Forms.Label _fromLandLabel;
        private System.Windows.Forms.TextBox _fromLandBox;
        private System.Windows.Forms.Button _fromLandButton;
        private System.Windows.Forms.Label _fromTexLabel;
        private System.Windows.Forms.TextBox _fromTexBox;
        private System.Windows.Forms.Button _fromTexButton;
        private System.Windows.Forms.Label _modeLabel;
        private System.Windows.Forms.ComboBox _modeBox;
        private System.Windows.Forms.Label _fitLabel;
        private System.Windows.Forms.ComboBox _fitBox;
        private System.Windows.Forms.PictureBox _sourceBox;
        private System.Windows.Forms.FlowLayoutPanel _sourceTop;
        private System.Windows.Forms.FlowLayoutPanel _sourceMode;
        private System.Windows.Forms.Label _sourceInfo;
        private System.Windows.Forms.TableLayoutPanel _sourceLayout;
        private System.Windows.Forms.GroupBox _sourceGroup;
        private System.Windows.Forms.Button _rotLeftButton;
        private System.Windows.Forms.Button _rotRightButton;
        private System.Windows.Forms.Button _mirrorHButton;
        private System.Windows.Forms.Button _mirrorVButton;
        private System.Windows.Forms.Button _resetOrientButton;
        private System.Windows.Forms.Label _alignLabel;
        private System.Windows.Forms.TextBox _alignBox;
        private System.Windows.Forms.Button _alignButton;
        private System.Windows.Forms.Label _sizeLabel;
        private System.Windows.Forms.ComboBox _sizeBox;
        private System.Windows.Forms.FlowLayoutPanel _orientFlow;
        private System.Windows.Forms.Label _texCaption;
        private System.Windows.Forms.PictureBox _textureBox;
        private System.Windows.Forms.Label _landCaption;
        private System.Windows.Forms.PictureBox _landBox;
        private System.Windows.Forms.Label _mapCaption;
        private System.Windows.Forms.PictureBox _mapBox;
        private System.Windows.Forms.Label _matchLabel;
        private System.Windows.Forms.TableLayoutPanel _resultGrid;
        private System.Windows.Forms.TableLayoutPanel _resultLayout;
        private System.Windows.Forms.GroupBox _resultGroup;
        private System.Windows.Forms.Label _landIdLabel;
        private System.Windows.Forms.TextBox _landIdBox;
        private System.Windows.Forms.Button _landFreeButton;
        private System.Windows.Forms.Button _landFreeExtButton;
        private System.Windows.Forms.Label _landIdInfo;
        private System.Windows.Forms.Label _texIdLabel;
        private System.Windows.Forms.TextBox _texIdBox;
        private System.Windows.Forms.Button _texFreeButton;
        private System.Windows.Forms.CheckBox _sameIdCheck;
        private System.Windows.Forms.Label _texIdInfo;
        private System.Windows.Forms.Label _nameLabel;
        private System.Windows.Forms.TextBox _nameBox;
        private System.Windows.Forms.Label _flagsLabel;
        private System.Windows.Forms.TextBox _flagsFromBox;
        private System.Windows.Forms.CheckBox _writeLandCheck;
        private System.Windows.Forms.CheckBox _writeTexCheck;
        private System.Windows.Forms.CheckBox _radarCheck;
        private System.Windows.Forms.Button _patchButton;
        private System.Windows.Forms.Button _saveButton;
        private System.Windows.Forms.Button _showLandButton;
        private System.Windows.Forms.Button _showTexButton;
        private System.Windows.Forms.Button _showTileDataButton;
        private System.Windows.Forms.Label _linkLabel;
        private System.Windows.Forms.Label _statusLabel;
        private System.Windows.Forms.FlowLayoutPanel _targetFlow;
        private System.Windows.Forms.GroupBox _targetGroup;
        private System.Windows.Forms.TableLayoutPanel _rootLayout;
        private System.Windows.Forms.ToolTip _tips;
    }
}
