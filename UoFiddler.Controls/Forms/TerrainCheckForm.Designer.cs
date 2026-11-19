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
    partial class TerrainCheckForm
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
            this._scanLand = new System.Windows.Forms.CheckBox();
            this._scanTextures = new System.Windows.Forms.CheckBox();
            this._findLabel = new System.Windows.Forms.Label();
            this._kindEmpty = new System.Windows.Forms.CheckBox();
            this._kindSingle = new System.Windows.Forms.CheckBox();
            this._kindFew = new System.Windows.Forms.CheckBox();
            this._scanButton = new System.Windows.Forms.Button();
            this._cancelEmptyButton = new System.Windows.Forms.Button();
            this._emptyStatus = new System.Windows.Forms.Label();
            this._emptyTop = new System.Windows.Forms.FlowLayoutPanel();
            this._emptyList = new System.Windows.Forms.ListView();
            this._ecType = new System.Windows.Forms.ColumnHeader();
            this._ecId = new System.Windows.Forms.ColumnHeader();
            this._ecKind = new System.Windows.Forms.ColumnHeader();
            this._ecColours = new System.Windows.Forms.ColumnHeader();
            this._ecLink = new System.Windows.Forms.ColumnHeader();
            this._checkAllButton = new System.Windows.Forms.Button();
            this._checkNoneButton = new System.Windows.Forms.Button();
            this._deleteButton = new System.Windows.Forms.Button();
            this._clearLinksCheck = new System.Windows.Forms.CheckBox();
            this._emptyShowButton = new System.Windows.Forms.Button();
            this._emptyHint = new System.Windows.Forms.Label();
            this._emptyBottom = new System.Windows.Forms.FlowLayoutPanel();
            this._emptyLayout = new System.Windows.Forms.TableLayoutPanel();
            this._linkScanButton = new System.Windows.Forms.Button();
            this._cancelLinkButton = new System.Windows.Forms.Button();
            this._thresholdLabel = new System.Windows.Forms.Label();
            this._threshold = new System.Windows.Forms.NumericUpDown();
            this._onlyProblemsCheck = new System.Windows.Forms.CheckBox();
            this._linkStatus = new System.Windows.Forms.Label();
            this._linkTop = new System.Windows.Forms.FlowLayoutPanel();
            this._linkList = new System.Windows.Forms.ListView();
            this._lcLand = new System.Windows.Forms.ColumnHeader();
            this._lcName = new System.Windows.Forms.ColumnHeader();
            this._lcTex = new System.Windows.Forms.ColumnHeader();
            this._lcSize = new System.Windows.Forms.ColumnHeader();
            this._lcMatch = new System.Windows.Forms.ColumnHeader();
            this._lcBest = new System.Windows.Forms.ColumnHeader();
            this._lcProblem = new System.Windows.Forms.ColumnHeader();
            this._pvLandCaption = new System.Windows.Forms.Label();
            this._pvLand = new System.Windows.Forms.PictureBox();
            this._pvFromTexCaption = new System.Windows.Forms.Label();
            this._pvFromTex = new System.Windows.Forms.PictureBox();
            this._pvTexCaption = new System.Windows.Forms.Label();
            this._pvTex = new System.Windows.Forms.PictureBox();
            this._previewLayout = new System.Windows.Forms.TableLayoutPanel();
            this._linkMiddle = new System.Windows.Forms.TableLayoutPanel();
            this._setTexIdButton = new System.Windows.Forms.Button();
            this._fixTurnButton = new System.Windows.Forms.Button();
            this._openStudioButton = new System.Windows.Forms.Button();
            this._linkShowButton = new System.Windows.Forms.Button();
            this._linkShowTileDataButton = new System.Windows.Forms.Button();
            this._linkHint = new System.Windows.Forms.Label();
            this._linkBottom = new System.Windows.Forms.FlowLayoutPanel();
            this._linkLayout = new System.Windows.Forms.TableLayoutPanel();
            this._tabs = new System.Windows.Forms.TabControl();
            this._emptyPage = new System.Windows.Forms.TabPage();
            this._linkPage = new System.Windows.Forms.TabPage();
            this._thumbs = new System.Windows.Forms.ImageList(this.components);
            this._jobTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this._threshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._pvLand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._pvFromTex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._pvTex)).BeginInit();
            this._emptyTop.SuspendLayout();
            this._emptyBottom.SuspendLayout();
            this._emptyLayout.SuspendLayout();
            this._linkTop.SuspendLayout();
            this._previewLayout.SuspendLayout();
            this._linkMiddle.SuspendLayout();
            this._linkBottom.SuspendLayout();
            this._linkLayout.SuspendLayout();
            this._tabs.SuspendLayout();
            this._emptyPage.SuspendLayout();
            this._linkPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _scanLand
            // 
            this._scanLand.AutoSize = true;
            this._scanLand.Name = "_scanLand";
            this._scanLand.Text = "Land tiles";
            this._scanLand.Checked = true;
            this._scanLand.CheckState = System.Windows.Forms.CheckState.Checked;
            this._scanLand.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._scanLand.UseVisualStyleBackColor = true;
            // 
            // _scanTextures
            // 
            this._scanTextures.AutoSize = true;
            this._scanTextures.Name = "_scanTextures";
            this._scanTextures.Text = "Textures";
            this._scanTextures.Checked = true;
            this._scanTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this._scanTextures.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._scanTextures.UseVisualStyleBackColor = true;
            // 
            // _findLabel
            // 
            this._findLabel.AutoSize = true;
            this._findLabel.Name = "_findLabel";
            this._findLabel.Text = "Find:";
            this._findLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _kindEmpty
            // 
            this._kindEmpty.AutoSize = true;
            this._kindEmpty.Name = "_kindEmpty";
            this._kindEmpty.Text = "Empty (black / transparent)";
            this._kindEmpty.Checked = true;
            this._kindEmpty.CheckState = System.Windows.Forms.CheckState.Checked;
            this._kindEmpty.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._kindEmpty.UseVisualStyleBackColor = true;
            // 
            // _kindSingle
            // 
            this._kindSingle.AutoSize = true;
            this._kindSingle.Name = "_kindSingle";
            this._kindSingle.Text = "One colour (placeholder)";
            this._kindSingle.Checked = true;
            this._kindSingle.CheckState = System.Windows.Forms.CheckState.Checked;
            this._kindSingle.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._kindSingle.UseVisualStyleBackColor = true;
            // 
            // _kindFew
            // 
            this._kindFew.AutoSize = true;
            this._kindFew.Name = "_kindFew";
            this._kindFew.Text = "2 - 4 colours";
            this._kindFew.Checked = false;
            this._kindFew.CheckState = System.Windows.Forms.CheckState.Unchecked;
            this._kindFew.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._kindFew.UseVisualStyleBackColor = true;
            // 
            // _scanButton
            // 
            this._scanButton.AutoSize = true;
            this._scanButton.Name = "_scanButton";
            this._scanButton.Text = "Search";
            this._scanButton.UseVisualStyleBackColor = true;
            this._scanButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._scanButton.Click += new System.EventHandler(this.OnScanEmpty);
            // 
            // _cancelEmptyButton
            // 
            this._cancelEmptyButton.AutoSize = true;
            this._cancelEmptyButton.Name = "_cancelEmptyButton";
            this._cancelEmptyButton.Text = "Cancel";
            this._cancelEmptyButton.UseVisualStyleBackColor = true;
            this._cancelEmptyButton.Enabled = false;
            this._cancelEmptyButton.Click += new System.EventHandler(this.OnCancel);
            // 
            // _emptyStatus
            // 
            this._emptyStatus.AutoSize = true;
            this._emptyStatus.Name = "_emptyStatus";
            this._emptyStatus.Text = "";
            this._emptyStatus.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _emptyTop
            // 
            this._emptyTop.Name = "_emptyTop";
            this._emptyTop.AutoSize = true;
            this._emptyTop.Dock = System.Windows.Forms.DockStyle.Top;
            this._emptyTop.WrapContents = true;
            this._emptyTop.Controls.Add(this._scanLand);
            this._emptyTop.Controls.Add(this._scanTextures);
            this._emptyTop.Controls.Add(this._findLabel);
            this._emptyTop.Controls.Add(this._kindEmpty);
            this._emptyTop.Controls.Add(this._kindSingle);
            this._emptyTop.Controls.Add(this._kindFew);
            this._emptyTop.Controls.Add(this._scanButton);
            this._emptyTop.Controls.Add(this._cancelEmptyButton);
            this._emptyTop.Controls.Add(this._emptyStatus);
            // 
            // _ecType
            // 
            this._ecType.Text = "Type";
            this._ecType.Width = 90;
            // 
            // _ecId
            // 
            this._ecId.Text = "Id";
            this._ecId.Width = 90;
            // 
            // _ecKind
            // 
            this._ecKind.Text = "Found";
            this._ecKind.Width = 190;
            // 
            // _ecColours
            // 
            this._ecColours.Text = "Colours";
            this._ecColours.Width = 70;
            // 
            // _ecLink
            // 
            this._ecLink.Text = "Link / used by";
            this._ecLink.Width = 420;
            // 
            // _emptyList
            // 
            this._emptyList.Name = "_emptyList";
            this._emptyList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._emptyList.View = System.Windows.Forms.View.Details;
            this._emptyList.FullRowSelect = true;
            this._emptyList.HideSelection = false;
            this._emptyList.MultiSelect = true;
            this._emptyList.CheckBoxes = true;
            this._emptyList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this._ecType, this._ecId, this._ecKind, this._ecColours, this._ecLink });
            this._emptyList.DoubleClick += new System.EventHandler(this.OnEmptyShow);
            // 
            // _checkAllButton
            // 
            this._checkAllButton.AutoSize = true;
            this._checkAllButton.Name = "_checkAllButton";
            this._checkAllButton.Text = "Check all";
            this._checkAllButton.UseVisualStyleBackColor = true;
            this._checkAllButton.Click += new System.EventHandler(this.OnCheckAll);
            // 
            // _checkNoneButton
            // 
            this._checkNoneButton.AutoSize = true;
            this._checkNoneButton.Name = "_checkNoneButton";
            this._checkNoneButton.Text = "Check none";
            this._checkNoneButton.UseVisualStyleBackColor = true;
            this._checkNoneButton.Click += new System.EventHandler(this.OnCheckNone);
            // 
            // _deleteButton
            // 
            this._deleteButton.AutoSize = true;
            this._deleteButton.Name = "_deleteButton";
            this._deleteButton.Text = "Delete checked";
            this._deleteButton.UseVisualStyleBackColor = true;
            this._deleteButton.ForeColor = System.Drawing.Color.DarkRed;
            this._deleteButton.Click += new System.EventHandler(this.OnDeleteChecked);
            // 
            // _clearLinksCheck
            // 
            this._clearLinksCheck.AutoSize = true;
            this._clearLinksCheck.Name = "_clearLinksCheck";
            this._clearLinksCheck.Text = "Also set the texture id of land tiles that point to a deleted texture to 0";
            this._clearLinksCheck.Checked = true;
            this._clearLinksCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this._clearLinksCheck.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._clearLinksCheck.UseVisualStyleBackColor = true;
            // 
            // _emptyShowButton
            // 
            this._emptyShowButton.AutoSize = true;
            this._emptyShowButton.Name = "_emptyShowButton";
            this._emptyShowButton.Text = "Show in tab";
            this._emptyShowButton.UseVisualStyleBackColor = true;
            this._emptyShowButton.Click += new System.EventHandler(this.OnEmptyShow);
            // 
            // _emptyHint
            // 
            this._emptyHint.AutoSize = true;
            this._emptyHint.Name = "_emptyHint";
            this._emptyHint.Text = "Deleting only changes the loaded data - save the LandTiles / Textures / TileData tab afterwards.";
            this._emptyHint.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._emptyHint.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _emptyBottom
            // 
            this._emptyBottom.Name = "_emptyBottom";
            this._emptyBottom.AutoSize = true;
            this._emptyBottom.Dock = System.Windows.Forms.DockStyle.Top;
            this._emptyBottom.WrapContents = true;
            this._emptyBottom.Controls.Add(this._checkAllButton);
            this._emptyBottom.Controls.Add(this._checkNoneButton);
            this._emptyBottom.Controls.Add(this._deleteButton);
            this._emptyBottom.Controls.Add(this._clearLinksCheck);
            this._emptyBottom.Controls.Add(this._emptyShowButton);
            this._emptyBottom.Controls.Add(this._emptyHint);
            // 
            // _emptyLayout
            // 
            this._emptyLayout.Name = "_emptyLayout";
            this._emptyLayout.ColumnCount = 1;
            this._emptyLayout.RowCount = 3;
            this._emptyLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._emptyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._emptyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._emptyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._emptyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._emptyLayout.Controls.Add(this._emptyTop, 0, 0);
            this._emptyLayout.Controls.Add(this._emptyList, 0, 1);
            this._emptyLayout.Controls.Add(this._emptyBottom, 0, 2);
            // 
            // _linkScanButton
            // 
            this._linkScanButton.AutoSize = true;
            this._linkScanButton.Name = "_linkScanButton";
            this._linkScanButton.Text = "Check all land tiles";
            this._linkScanButton.UseVisualStyleBackColor = true;
            this._linkScanButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._linkScanButton.Click += new System.EventHandler(this.OnScanLinks);
            // 
            // _cancelLinkButton
            // 
            this._cancelLinkButton.AutoSize = true;
            this._cancelLinkButton.Name = "_cancelLinkButton";
            this._cancelLinkButton.Text = "Cancel";
            this._cancelLinkButton.UseVisualStyleBackColor = true;
            this._cancelLinkButton.Enabled = false;
            this._cancelLinkButton.Click += new System.EventHandler(this.OnCancel);
            // 
            // _thresholdLabel
            // 
            this._thresholdLabel.AutoSize = true;
            this._thresholdLabel.Name = "_thresholdLabel";
            this._thresholdLabel.Text = "Report when the texture fits less than (%):";
            this._thresholdLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _threshold
            // 
            this._threshold.Name = "_threshold";
            this._threshold.Width = 60;
            this._threshold.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this._threshold.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this._threshold.Value = new decimal(new int[] { 75, 0, 0, 0 });
            // 
            // _onlyProblemsCheck
            // 
            this._onlyProblemsCheck.AutoSize = true;
            this._onlyProblemsCheck.Name = "_onlyProblemsCheck";
            this._onlyProblemsCheck.Text = "Only problems";
            this._onlyProblemsCheck.Checked = true;
            this._onlyProblemsCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this._onlyProblemsCheck.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._onlyProblemsCheck.UseVisualStyleBackColor = true;
            // 
            // _linkStatus
            // 
            this._linkStatus.AutoSize = true;
            this._linkStatus.Name = "_linkStatus";
            this._linkStatus.Text = "";
            this._linkStatus.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _linkTop
            // 
            this._linkTop.Name = "_linkTop";
            this._linkTop.AutoSize = true;
            this._linkTop.Dock = System.Windows.Forms.DockStyle.Top;
            this._linkTop.WrapContents = true;
            this._linkTop.Controls.Add(this._linkScanButton);
            this._linkTop.Controls.Add(this._cancelLinkButton);
            this._linkTop.Controls.Add(this._thresholdLabel);
            this._linkTop.Controls.Add(this._threshold);
            this._linkTop.Controls.Add(this._onlyProblemsCheck);
            this._linkTop.Controls.Add(this._linkStatus);
            // 
            // _lcLand
            // 
            this._lcLand.Text = "Land tile";
            this._lcLand.Width = 85;
            // 
            // _lcName
            // 
            this._lcName.Text = "Name";
            this._lcName.Width = 130;
            // 
            // _lcTex
            // 
            this._lcTex.Text = "Texture id";
            this._lcTex.Width = 85;
            // 
            // _lcSize
            // 
            this._lcSize.Text = "Texture";
            this._lcSize.Width = 70;
            // 
            // _lcMatch
            // 
            this._lcMatch.Text = "Fits";
            this._lcMatch.Width = 60;
            // 
            // _lcBest
            // 
            this._lcBest.Text = "Best turn";
            this._lcBest.Width = 150;
            // 
            // _lcProblem
            // 
            this._lcProblem.Text = "Problem";
            this._lcProblem.Width = 420;
            // 
            // _linkList
            // 
            this._linkList.Name = "_linkList";
            this._linkList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._linkList.View = System.Windows.Forms.View.Details;
            this._linkList.FullRowSelect = true;
            this._linkList.HideSelection = false;
            this._linkList.MultiSelect = true;
            this._linkList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this._lcLand, this._lcName, this._lcTex, this._lcSize, this._lcMatch, this._lcBest, this._lcProblem });
            this._linkList.DoubleClick += new System.EventHandler(this.OnOpenStudio);
            this._linkList.SelectedIndexChanged += new System.EventHandler(this.OnLinkSelected);
            // 
            // _pvLandCaption
            // 
            this._pvLandCaption.AutoSize = true;
            this._pvLandCaption.Name = "_pvLandCaption";
            this._pvLandCaption.Text = "Land tile (art)";
            this._pvLandCaption.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _pvLand
            // 
            this._pvLand.Name = "_pvLand";
            this._pvLand.Size = new System.Drawing.Size(184, 184);
            this._pvLand.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pvLand.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this._pvLand.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._pvLand.TabStop = false;
            // 
            // _pvFromTexCaption
            // 
            this._pvFromTexCaption.AutoSize = true;
            this._pvFromTexCaption.Name = "_pvFromTexCaption";
            this._pvFromTexCaption.Text = "Land tile made from the texture";
            this._pvFromTexCaption.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _pvFromTex
            // 
            this._pvFromTex.Name = "_pvFromTex";
            this._pvFromTex.Size = new System.Drawing.Size(184, 184);
            this._pvFromTex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pvFromTex.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this._pvFromTex.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._pvFromTex.TabStop = false;
            // 
            // _pvTexCaption
            // 
            this._pvTexCaption.AutoSize = true;
            this._pvTexCaption.Name = "_pvTexCaption";
            this._pvTexCaption.Text = "Texture";
            this._pvTexCaption.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _pvTex
            // 
            this._pvTex.Name = "_pvTex";
            this._pvTex.Size = new System.Drawing.Size(260, 260);
            this._pvTex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pvTex.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this._pvTex.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._pvTex.TabStop = false;
            // 
            // _previewLayout
            // 
            this._previewLayout.Name = "_previewLayout";
            this._previewLayout.ColumnCount = 1;
            this._previewLayout.RowCount = 6;
            this._previewLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._previewLayout.AutoSize = true;
            this._previewLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._previewLayout.Controls.Add(this._pvLandCaption, 0, 0);
            this._previewLayout.Controls.Add(this._pvLand, 0, 1);
            this._previewLayout.Controls.Add(this._pvFromTexCaption, 0, 2);
            this._previewLayout.Controls.Add(this._pvFromTex, 0, 3);
            this._previewLayout.Controls.Add(this._pvTexCaption, 0, 4);
            this._previewLayout.Controls.Add(this._pvTex, 0, 5);
            // 
            // _linkMiddle
            // 
            this._linkMiddle.Name = "_linkMiddle";
            this._linkMiddle.ColumnCount = 2;
            this._linkMiddle.RowCount = 1;
            this._linkMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this._linkMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._linkMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._linkMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._linkMiddle.Controls.Add(this._linkList, 0, 0);
            this._linkMiddle.Controls.Add(this._previewLayout, 1, 0);
            // 
            // _setTexIdButton
            // 
            this._setTexIdButton.AutoSize = true;
            this._setTexIdButton.Name = "_setTexIdButton";
            this._setTexIdButton.Text = "Set texture id = land id";
            this._setTexIdButton.UseVisualStyleBackColor = true;
            this._setTexIdButton.Click += new System.EventHandler(this.OnSetTexIdToLand);
            // 
            // _fixTurnButton
            // 
            this._fixTurnButton.AutoSize = true;
            this._fixTurnButton.Name = "_fixTurnButton";
            this._fixTurnButton.Text = "Turn the texture to fit";
            this._fixTurnButton.UseVisualStyleBackColor = true;
            this._fixTurnButton.Click += new System.EventHandler(this.OnFixTurn);
            // 
            // _openStudioButton
            // 
            this._openStudioButton.AutoSize = true;
            this._openStudioButton.Name = "_openStudioButton";
            this._openStudioButton.Text = "Open in the studio";
            this._openStudioButton.UseVisualStyleBackColor = true;
            this._openStudioButton.Click += new System.EventHandler(this.OnOpenStudio);
            // 
            // _linkShowButton
            // 
            this._linkShowButton.AutoSize = true;
            this._linkShowButton.Name = "_linkShowButton";
            this._linkShowButton.Text = "Show in LandTiles";
            this._linkShowButton.UseVisualStyleBackColor = true;
            this._linkShowButton.Click += new System.EventHandler(this.OnLinkShowLand);
            // 
            // _linkShowTileDataButton
            // 
            this._linkShowTileDataButton.AutoSize = true;
            this._linkShowTileDataButton.Name = "_linkShowTileDataButton";
            this._linkShowTileDataButton.Text = "Show in TileData";
            this._linkShowTileDataButton.UseVisualStyleBackColor = true;
            this._linkShowTileDataButton.Click += new System.EventHandler(this.OnLinkShowTileData);
            // 
            // _linkHint
            // 
            this._linkHint.AutoSize = true;
            this._linkHint.Name = "_linkHint";
            this._linkHint.Text = "Fits = how much the land tile made from the texture looks like the land tile art (client orientation). Changes are only in memory - save afterwards.";
            this._linkHint.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._linkHint.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _linkBottom
            // 
            this._linkBottom.Name = "_linkBottom";
            this._linkBottom.AutoSize = true;
            this._linkBottom.Dock = System.Windows.Forms.DockStyle.Top;
            this._linkBottom.WrapContents = true;
            this._linkBottom.Controls.Add(this._setTexIdButton);
            this._linkBottom.Controls.Add(this._fixTurnButton);
            this._linkBottom.Controls.Add(this._openStudioButton);
            this._linkBottom.Controls.Add(this._linkShowButton);
            this._linkBottom.Controls.Add(this._linkShowTileDataButton);
            this._linkBottom.Controls.Add(this._linkHint);
            // 
            // _linkLayout
            // 
            this._linkLayout.Name = "_linkLayout";
            this._linkLayout.ColumnCount = 1;
            this._linkLayout.RowCount = 3;
            this._linkLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._linkLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._linkLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._linkLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._linkLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._linkLayout.Controls.Add(this._linkTop, 0, 0);
            this._linkLayout.Controls.Add(this._linkMiddle, 0, 1);
            this._linkLayout.Controls.Add(this._linkBottom, 0, 2);
            // 
            // _emptyPage
            // 
            this._emptyPage.Name = "_emptyPage";
            this._emptyPage.Text = "Empty / placeholder images";
            this._emptyPage.UseVisualStyleBackColor = true;
            this._emptyPage.Controls.Add(this._emptyLayout);
            // 
            // _linkPage
            // 
            this._linkPage.Name = "_linkPage";
            this._linkPage.Text = "Land tile <-> texture";
            this._linkPage.UseVisualStyleBackColor = true;
            this._linkPage.Controls.Add(this._linkLayout);
            // 
            // _tabs
            // 
            this._tabs.Name = "_tabs";
            this._tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabs.Controls.Add(this._emptyPage);
            this._tabs.Controls.Add(this._linkPage);
            // 
            // _thumbs
            // 
            this._thumbs.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this._thumbs.ImageSize = new System.Drawing.Size(32, 32);
            this._emptyList.SmallImageList = this._thumbs;
            // 
            // _jobTimer
            // 
            this._jobTimer.Interval = 15;
            this._jobTimer.Tick += new System.EventHandler(this.OnJobTick);
            // 
            // TerrainCheckForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 720);
            this.Controls.Add(this._tabs);
            this.Name = "TerrainCheckForm";
            this.Text = "Land tiles / textures: check";
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ShowIcon = false;
            ((System.ComponentModel.ISupportInitialize)(this._threshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._pvLand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._pvFromTex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._pvTex)).EndInit();
            this._linkPage.ResumeLayout(false);
            this._linkPage.PerformLayout();
            this._emptyPage.ResumeLayout(false);
            this._emptyPage.PerformLayout();
            this._tabs.ResumeLayout(false);
            this._tabs.PerformLayout();
            this._linkLayout.ResumeLayout(false);
            this._linkLayout.PerformLayout();
            this._linkBottom.ResumeLayout(false);
            this._linkBottom.PerformLayout();
            this._linkMiddle.ResumeLayout(false);
            this._linkMiddle.PerformLayout();
            this._previewLayout.ResumeLayout(false);
            this._previewLayout.PerformLayout();
            this._linkTop.ResumeLayout(false);
            this._linkTop.PerformLayout();
            this._emptyLayout.ResumeLayout(false);
            this._emptyLayout.PerformLayout();
            this._emptyBottom.ResumeLayout(false);
            this._emptyBottom.PerformLayout();
            this._emptyTop.ResumeLayout(false);
            this._emptyTop.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.CheckBox _scanLand;
        private System.Windows.Forms.CheckBox _scanTextures;
        private System.Windows.Forms.Label _findLabel;
        private System.Windows.Forms.CheckBox _kindEmpty;
        private System.Windows.Forms.CheckBox _kindSingle;
        private System.Windows.Forms.CheckBox _kindFew;
        private System.Windows.Forms.Button _scanButton;
        private System.Windows.Forms.Button _cancelEmptyButton;
        private System.Windows.Forms.Label _emptyStatus;
        private System.Windows.Forms.FlowLayoutPanel _emptyTop;
        private System.Windows.Forms.ListView _emptyList;
        private System.Windows.Forms.ColumnHeader _ecType;
        private System.Windows.Forms.ColumnHeader _ecId;
        private System.Windows.Forms.ColumnHeader _ecKind;
        private System.Windows.Forms.ColumnHeader _ecColours;
        private System.Windows.Forms.ColumnHeader _ecLink;
        private System.Windows.Forms.Button _checkAllButton;
        private System.Windows.Forms.Button _checkNoneButton;
        private System.Windows.Forms.Button _deleteButton;
        private System.Windows.Forms.CheckBox _clearLinksCheck;
        private System.Windows.Forms.Button _emptyShowButton;
        private System.Windows.Forms.Label _emptyHint;
        private System.Windows.Forms.FlowLayoutPanel _emptyBottom;
        private System.Windows.Forms.TableLayoutPanel _emptyLayout;
        private System.Windows.Forms.Button _linkScanButton;
        private System.Windows.Forms.Button _cancelLinkButton;
        private System.Windows.Forms.Label _thresholdLabel;
        private System.Windows.Forms.NumericUpDown _threshold;
        private System.Windows.Forms.CheckBox _onlyProblemsCheck;
        private System.Windows.Forms.Label _linkStatus;
        private System.Windows.Forms.FlowLayoutPanel _linkTop;
        private System.Windows.Forms.ListView _linkList;
        private System.Windows.Forms.ColumnHeader _lcLand;
        private System.Windows.Forms.ColumnHeader _lcName;
        private System.Windows.Forms.ColumnHeader _lcTex;
        private System.Windows.Forms.ColumnHeader _lcSize;
        private System.Windows.Forms.ColumnHeader _lcMatch;
        private System.Windows.Forms.ColumnHeader _lcBest;
        private System.Windows.Forms.ColumnHeader _lcProblem;
        private System.Windows.Forms.Label _pvLandCaption;
        private System.Windows.Forms.PictureBox _pvLand;
        private System.Windows.Forms.Label _pvFromTexCaption;
        private System.Windows.Forms.PictureBox _pvFromTex;
        private System.Windows.Forms.Label _pvTexCaption;
        private System.Windows.Forms.PictureBox _pvTex;
        private System.Windows.Forms.TableLayoutPanel _previewLayout;
        private System.Windows.Forms.TableLayoutPanel _linkMiddle;
        private System.Windows.Forms.Button _setTexIdButton;
        private System.Windows.Forms.Button _fixTurnButton;
        private System.Windows.Forms.Button _openStudioButton;
        private System.Windows.Forms.Button _linkShowButton;
        private System.Windows.Forms.Button _linkShowTileDataButton;
        private System.Windows.Forms.Label _linkHint;
        private System.Windows.Forms.FlowLayoutPanel _linkBottom;
        private System.Windows.Forms.TableLayoutPanel _linkLayout;
        private System.Windows.Forms.TabControl _tabs;
        private System.Windows.Forms.TabPage _emptyPage;
        private System.Windows.Forms.TabPage _linkPage;
        private System.Windows.Forms.ImageList _thumbs;
        private System.Windows.Forms.Timer _jobTimer;
    }
}
