namespace UoFiddler.Controls.Forms
{
    partial class TileDataBatchForm
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
            this._split = new System.Windows.Forms.SplitContainer();
            this._grpFields = new System.Windows.Forms.GroupBox();
            this._tblFields = new System.Windows.Forms.TableLayoutPanel();
            this._chkFName = new System.Windows.Forms.CheckBox();
            this._cmbOpName = new System.Windows.Forms.ComboBox();
            this._txtName = new System.Windows.Forms.TextBox();
            this._txtNameWith = new System.Windows.Forms.TextBox();
            this._chkFWeight = new System.Windows.Forms.CheckBox();
            this._cmbOpWeight = new System.Windows.Forms.ComboBox();
            this._txtWeight = new System.Windows.Forms.TextBox();
            this._chkFQuality = new System.Windows.Forms.CheckBox();
            this._cmbOpQuality = new System.Windows.Forms.ComboBox();
            this._txtQuality = new System.Windows.Forms.TextBox();
            this._chkFAnimation = new System.Windows.Forms.CheckBox();
            this._cmbOpAnimation = new System.Windows.Forms.ComboBox();
            this._txtAnimation = new System.Windows.Forms.TextBox();
            this._chkFQuantity = new System.Windows.Forms.CheckBox();
            this._cmbOpQuantity = new System.Windows.Forms.ComboBox();
            this._txtQuantity = new System.Windows.Forms.TextBox();
            this._chkFHue = new System.Windows.Forms.CheckBox();
            this._cmbOpHue = new System.Windows.Forms.ComboBox();
            this._txtHue = new System.Windows.Forms.TextBox();
            this._chkFStackingOffset = new System.Windows.Forms.CheckBox();
            this._cmbOpStackingOffset = new System.Windows.Forms.ComboBox();
            this._txtStackingOffset = new System.Windows.Forms.TextBox();
            this._chkFValue = new System.Windows.Forms.CheckBox();
            this._cmbOpValue = new System.Windows.Forms.ComboBox();
            this._txtValue = new System.Windows.Forms.TextBox();
            this._chkFHeight = new System.Windows.Forms.CheckBox();
            this._cmbOpHeight = new System.Windows.Forms.ComboBox();
            this._txtHeight = new System.Windows.Forms.TextBox();
            this._chkFMiscData = new System.Windows.Forms.CheckBox();
            this._cmbOpMiscData = new System.Windows.Forms.ComboBox();
            this._txtMiscData = new System.Windows.Forms.TextBox();
            this._chkFUnk2 = new System.Windows.Forms.CheckBox();
            this._cmbOpUnk2 = new System.Windows.Forms.ComboBox();
            this._txtUnk2 = new System.Windows.Forms.TextBox();
            this._chkFUnk3 = new System.Windows.Forms.CheckBox();
            this._cmbOpUnk3 = new System.Windows.Forms.ComboBox();
            this._txtUnk3 = new System.Windows.Forms.TextBox();
            this._chkFTextureId = new System.Windows.Forms.CheckBox();
            this._cmbOpTextureId = new System.Windows.Forms.ComboBox();
            this._txtTextureId = new System.Windows.Forms.TextBox();
            this._lblFieldsHint = new System.Windows.Forms.Label();
            this._grpFlags = new System.Windows.Forms.GroupBox();
            this._gridFlags = new System.Windows.Forms.DataGridView();
            this._colFlag = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._colSet = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._colClear = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this._colHave = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._grpTarget = new System.Windows.Forms.GroupBox();
            this._tblTarget = new System.Windows.Forms.TableLayoutPanel();
            this._rbSelected = new System.Windows.Forms.RadioButton();
            this._lblSelected = new System.Windows.Forms.Label();
            this._rbRange = new System.Windows.Forms.RadioButton();
            this._txtRange = new System.Windows.Forms.TextBox();
            this._rbVisible = new System.Windows.Forms.RadioButton();
            this._lblVisible = new System.Windows.Forms.Label();
            this._chkOnlyName = new System.Windows.Forms.CheckBox();
            this._txtOnlyName = new System.Windows.Forms.TextBox();
            this._chkOnlyFlag = new System.Windows.Forms.CheckBox();
            this._cmbOnlyFlag = new System.Windows.Forms.ComboBox();
            this._bottom = new System.Windows.Forms.FlowLayoutPanel();
            this._btnClose = new System.Windows.Forms.Button();
            this._btnApply = new System.Windows.Forms.Button();
            this._lblPreview = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._split)).BeginInit();
            this._split.Panel1.SuspendLayout();
            this._split.Panel2.SuspendLayout();
            this._split.SuspendLayout();
            this._grpFields.SuspendLayout();
            this._tblFields.SuspendLayout();
            this._grpFlags.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridFlags)).BeginInit();
            this._grpTarget.SuspendLayout();
            this._tblTarget.SuspendLayout();
            this._bottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // _split
            // 
            // 
            // _split.Panel1
            // 
            this._split.Panel1.Controls.Add(this._grpFields);
            // 
            // _split.Panel2
            // 
            this._split.Panel2.Controls.Add(this._grpFlags);
            this._split.Dock = System.Windows.Forms.DockStyle.Fill;
            this._split.Name = "_split";
            this._split.Size = new System.Drawing.Size(884, 410);
            this._split.SplitterDistance = 470;
            this._split.TabIndex = 0;
            // 
            // _grpFields
            // 
            this._grpFields.Controls.Add(this._tblFields);
            this._grpFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grpFields.Name = "_grpFields";
            this._grpFields.Padding = new System.Windows.Forms.Padding(6);
            this._grpFields.TabIndex = 1;
            this._grpFields.Text = "Values (only ticked fields are changed)";
            // 
            // _tblFields
            // 
            this._tblFields.Controls.Add(this._chkFName, 0, 0);
            this._tblFields.Controls.Add(this._cmbOpName, 1, 0);
            this._tblFields.Controls.Add(this._txtName, 2, 0);
            this._tblFields.Controls.Add(this._txtNameWith, 3, 0);
            this._tblFields.Controls.Add(this._chkFWeight, 0, 1);
            this._tblFields.Controls.Add(this._cmbOpWeight, 1, 1);
            this._tblFields.Controls.Add(this._txtWeight, 2, 1);
            this._tblFields.Controls.Add(this._chkFQuality, 0, 2);
            this._tblFields.Controls.Add(this._cmbOpQuality, 1, 2);
            this._tblFields.Controls.Add(this._txtQuality, 2, 2);
            this._tblFields.Controls.Add(this._chkFAnimation, 0, 3);
            this._tblFields.Controls.Add(this._cmbOpAnimation, 1, 3);
            this._tblFields.Controls.Add(this._txtAnimation, 2, 3);
            this._tblFields.Controls.Add(this._chkFQuantity, 0, 4);
            this._tblFields.Controls.Add(this._cmbOpQuantity, 1, 4);
            this._tblFields.Controls.Add(this._txtQuantity, 2, 4);
            this._tblFields.Controls.Add(this._chkFHue, 0, 5);
            this._tblFields.Controls.Add(this._cmbOpHue, 1, 5);
            this._tblFields.Controls.Add(this._txtHue, 2, 5);
            this._tblFields.Controls.Add(this._chkFStackingOffset, 0, 6);
            this._tblFields.Controls.Add(this._cmbOpStackingOffset, 1, 6);
            this._tblFields.Controls.Add(this._txtStackingOffset, 2, 6);
            this._tblFields.Controls.Add(this._chkFValue, 0, 7);
            this._tblFields.Controls.Add(this._cmbOpValue, 1, 7);
            this._tblFields.Controls.Add(this._txtValue, 2, 7);
            this._tblFields.Controls.Add(this._chkFHeight, 0, 8);
            this._tblFields.Controls.Add(this._cmbOpHeight, 1, 8);
            this._tblFields.Controls.Add(this._txtHeight, 2, 8);
            this._tblFields.Controls.Add(this._chkFMiscData, 0, 9);
            this._tblFields.Controls.Add(this._cmbOpMiscData, 1, 9);
            this._tblFields.Controls.Add(this._txtMiscData, 2, 9);
            this._tblFields.Controls.Add(this._chkFUnk2, 0, 10);
            this._tblFields.Controls.Add(this._cmbOpUnk2, 1, 10);
            this._tblFields.Controls.Add(this._txtUnk2, 2, 10);
            this._tblFields.Controls.Add(this._chkFUnk3, 0, 11);
            this._tblFields.Controls.Add(this._cmbOpUnk3, 1, 11);
            this._tblFields.Controls.Add(this._txtUnk3, 2, 11);
            this._tblFields.Controls.Add(this._chkFTextureId, 0, 12);
            this._tblFields.Controls.Add(this._cmbOpTextureId, 1, 12);
            this._tblFields.Controls.Add(this._txtTextureId, 2, 12);
            this._tblFields.Controls.Add(this._lblFieldsHint, 0, 13);
            this._tblFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._tblFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._tblFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._tblFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblFields.AutoScroll = true;
            this._tblFields.ColumnCount = 4;
            this._tblFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tblFields.Name = "_tblFields";
            this._tblFields.RowCount = 14;
            this._tblFields.TabIndex = 2;
            // 
            // _chkFName
            // 
            this._chkFName.AutoSize = true;
            this._chkFName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFName.Name = "_chkFName";
            this._chkFName.TabIndex = 3;
            this._chkFName.Text = "Name";
            this._chkFName.UseVisualStyleBackColor = true;
            this._chkFName.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpName
            // 
            this._cmbOpName.Items.AddRange(new object[] {
            "set to",
            "prefix",
            "suffix",
            "replace"});
            this._cmbOpName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpName.Name = "_cmbOpName";
            this._cmbOpName.Size = new System.Drawing.Size(90, 23);
            this._cmbOpName.TabIndex = 4;
            this._cmbOpName.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtName
            // 
            this._txtName.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtName.Name = "_txtName";
            this._txtName.TabIndex = 5;
            this._txtName.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _txtNameWith
            // 
            this._txtNameWith.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtNameWith.Name = "_txtNameWith";
            this._txtNameWith.PlaceholderText = "replace with";
            this._txtNameWith.TabIndex = 6;
            this._txtNameWith.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFWeight
            // 
            this._chkFWeight.AutoSize = true;
            this._chkFWeight.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFWeight.Name = "_chkFWeight";
            this._chkFWeight.TabIndex = 7;
            this._chkFWeight.Text = "Weight";
            this._chkFWeight.UseVisualStyleBackColor = true;
            this._chkFWeight.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpWeight
            // 
            this._cmbOpWeight.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpWeight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpWeight.Name = "_cmbOpWeight";
            this._cmbOpWeight.Size = new System.Drawing.Size(90, 23);
            this._cmbOpWeight.TabIndex = 8;
            this._cmbOpWeight.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtWeight
            // 
            this._txtWeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtWeight.Name = "_txtWeight";
            this._txtWeight.TabIndex = 9;
            this._txtWeight.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFQuality
            // 
            this._chkFQuality.AutoSize = true;
            this._chkFQuality.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFQuality.Name = "_chkFQuality";
            this._chkFQuality.TabIndex = 10;
            this._chkFQuality.Text = "Quality / Layer";
            this._chkFQuality.UseVisualStyleBackColor = true;
            this._chkFQuality.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpQuality
            // 
            this._cmbOpQuality.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpQuality.Name = "_cmbOpQuality";
            this._cmbOpQuality.Size = new System.Drawing.Size(90, 23);
            this._cmbOpQuality.TabIndex = 11;
            this._cmbOpQuality.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtQuality
            // 
            this._txtQuality.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtQuality.Name = "_txtQuality";
            this._txtQuality.TabIndex = 12;
            this._txtQuality.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFAnimation
            // 
            this._chkFAnimation.AutoSize = true;
            this._chkFAnimation.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFAnimation.Name = "_chkFAnimation";
            this._chkFAnimation.TabIndex = 13;
            this._chkFAnimation.Text = "Animation / Gump";
            this._chkFAnimation.UseVisualStyleBackColor = true;
            this._chkFAnimation.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpAnimation
            // 
            this._cmbOpAnimation.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpAnimation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpAnimation.Name = "_cmbOpAnimation";
            this._cmbOpAnimation.Size = new System.Drawing.Size(90, 23);
            this._cmbOpAnimation.TabIndex = 14;
            this._cmbOpAnimation.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtAnimation
            // 
            this._txtAnimation.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtAnimation.Name = "_txtAnimation";
            this._txtAnimation.TabIndex = 15;
            this._txtAnimation.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFQuantity
            // 
            this._chkFQuantity.AutoSize = true;
            this._chkFQuantity.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFQuantity.Name = "_chkFQuantity";
            this._chkFQuantity.TabIndex = 16;
            this._chkFQuantity.Text = "Quantity";
            this._chkFQuantity.UseVisualStyleBackColor = true;
            this._chkFQuantity.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpQuantity
            // 
            this._cmbOpQuantity.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpQuantity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpQuantity.Name = "_cmbOpQuantity";
            this._cmbOpQuantity.Size = new System.Drawing.Size(90, 23);
            this._cmbOpQuantity.TabIndex = 17;
            this._cmbOpQuantity.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtQuantity
            // 
            this._txtQuantity.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtQuantity.Name = "_txtQuantity";
            this._txtQuantity.TabIndex = 18;
            this._txtQuantity.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFHue
            // 
            this._chkFHue.AutoSize = true;
            this._chkFHue.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFHue.Name = "_chkFHue";
            this._chkFHue.TabIndex = 19;
            this._chkFHue.Text = "Hue";
            this._chkFHue.UseVisualStyleBackColor = true;
            this._chkFHue.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpHue
            // 
            this._cmbOpHue.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpHue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpHue.Name = "_cmbOpHue";
            this._cmbOpHue.Size = new System.Drawing.Size(90, 23);
            this._cmbOpHue.TabIndex = 20;
            this._cmbOpHue.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtHue
            // 
            this._txtHue.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtHue.Name = "_txtHue";
            this._txtHue.TabIndex = 21;
            this._txtHue.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFStackingOffset
            // 
            this._chkFStackingOffset.AutoSize = true;
            this._chkFStackingOffset.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFStackingOffset.Name = "_chkFStackingOffset";
            this._chkFStackingOffset.TabIndex = 22;
            this._chkFStackingOffset.Text = "Stacking offset";
            this._chkFStackingOffset.UseVisualStyleBackColor = true;
            this._chkFStackingOffset.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpStackingOffset
            // 
            this._cmbOpStackingOffset.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpStackingOffset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpStackingOffset.Name = "_cmbOpStackingOffset";
            this._cmbOpStackingOffset.Size = new System.Drawing.Size(90, 23);
            this._cmbOpStackingOffset.TabIndex = 23;
            this._cmbOpStackingOffset.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtStackingOffset
            // 
            this._txtStackingOffset.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtStackingOffset.Name = "_txtStackingOffset";
            this._txtStackingOffset.TabIndex = 24;
            this._txtStackingOffset.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFValue
            // 
            this._chkFValue.AutoSize = true;
            this._chkFValue.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFValue.Name = "_chkFValue";
            this._chkFValue.TabIndex = 25;
            this._chkFValue.Text = "Value";
            this._chkFValue.UseVisualStyleBackColor = true;
            this._chkFValue.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpValue
            // 
            this._cmbOpValue.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpValue.Name = "_cmbOpValue";
            this._cmbOpValue.Size = new System.Drawing.Size(90, 23);
            this._cmbOpValue.TabIndex = 26;
            this._cmbOpValue.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtValue
            // 
            this._txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtValue.Name = "_txtValue";
            this._txtValue.TabIndex = 27;
            this._txtValue.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFHeight
            // 
            this._chkFHeight.AutoSize = true;
            this._chkFHeight.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFHeight.Name = "_chkFHeight";
            this._chkFHeight.TabIndex = 28;
            this._chkFHeight.Text = "Height";
            this._chkFHeight.UseVisualStyleBackColor = true;
            this._chkFHeight.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpHeight
            // 
            this._cmbOpHeight.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpHeight.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpHeight.Name = "_cmbOpHeight";
            this._cmbOpHeight.Size = new System.Drawing.Size(90, 23);
            this._cmbOpHeight.TabIndex = 29;
            this._cmbOpHeight.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtHeight
            // 
            this._txtHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtHeight.Name = "_txtHeight";
            this._txtHeight.TabIndex = 30;
            this._txtHeight.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFMiscData
            // 
            this._chkFMiscData.AutoSize = true;
            this._chkFMiscData.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFMiscData.Name = "_chkFMiscData";
            this._chkFMiscData.TabIndex = 31;
            this._chkFMiscData.Text = "Misc data";
            this._chkFMiscData.UseVisualStyleBackColor = true;
            this._chkFMiscData.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpMiscData
            // 
            this._cmbOpMiscData.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpMiscData.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpMiscData.Name = "_cmbOpMiscData";
            this._cmbOpMiscData.Size = new System.Drawing.Size(90, 23);
            this._cmbOpMiscData.TabIndex = 32;
            this._cmbOpMiscData.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtMiscData
            // 
            this._txtMiscData.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtMiscData.Name = "_txtMiscData";
            this._txtMiscData.TabIndex = 33;
            this._txtMiscData.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFUnk2
            // 
            this._chkFUnk2.AutoSize = true;
            this._chkFUnk2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFUnk2.Name = "_chkFUnk2";
            this._chkFUnk2.TabIndex = 34;
            this._chkFUnk2.Text = "Unknown 2";
            this._chkFUnk2.UseVisualStyleBackColor = true;
            this._chkFUnk2.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpUnk2
            // 
            this._cmbOpUnk2.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpUnk2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpUnk2.Name = "_cmbOpUnk2";
            this._cmbOpUnk2.Size = new System.Drawing.Size(90, 23);
            this._cmbOpUnk2.TabIndex = 35;
            this._cmbOpUnk2.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtUnk2
            // 
            this._txtUnk2.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtUnk2.Name = "_txtUnk2";
            this._txtUnk2.TabIndex = 36;
            this._txtUnk2.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFUnk3
            // 
            this._chkFUnk3.AutoSize = true;
            this._chkFUnk3.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFUnk3.Name = "_chkFUnk3";
            this._chkFUnk3.TabIndex = 37;
            this._chkFUnk3.Text = "Unknown 3";
            this._chkFUnk3.UseVisualStyleBackColor = true;
            this._chkFUnk3.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpUnk3
            // 
            this._cmbOpUnk3.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpUnk3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpUnk3.Name = "_cmbOpUnk3";
            this._cmbOpUnk3.Size = new System.Drawing.Size(90, 23);
            this._cmbOpUnk3.TabIndex = 38;
            this._cmbOpUnk3.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtUnk3
            // 
            this._txtUnk3.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtUnk3.Name = "_txtUnk3";
            this._txtUnk3.TabIndex = 39;
            this._txtUnk3.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _chkFTextureId
            // 
            this._chkFTextureId.AutoSize = true;
            this._chkFTextureId.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._chkFTextureId.Name = "_chkFTextureId";
            this._chkFTextureId.TabIndex = 40;
            this._chkFTextureId.Text = "Texture ID";
            this._chkFTextureId.UseVisualStyleBackColor = true;
            this._chkFTextureId.CheckedChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _cmbOpTextureId
            // 
            this._cmbOpTextureId.Items.AddRange(new object[] {
            "set to",
            "add",
            "subtract"});
            this._cmbOpTextureId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOpTextureId.Name = "_cmbOpTextureId";
            this._cmbOpTextureId.Size = new System.Drawing.Size(90, 23);
            this._cmbOpTextureId.TabIndex = 41;
            this._cmbOpTextureId.SelectedIndexChanged += new System.EventHandler(this.OnFieldsChanged);
            // 
            // _txtTextureId
            // 
            this._txtTextureId.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtTextureId.Name = "_txtTextureId";
            this._txtTextureId.TabIndex = 42;
            this._txtTextureId.TextChanged += new System.EventHandler(this.OnFieldTextChanged);
            // 
            // _lblFieldsHint
            // 
            this._tblFields.SetColumnSpan(this._lblFieldsHint, 4);
            this._lblFieldsHint.AutoSize = true;
            this._lblFieldsHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this._lblFieldsHint.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this._lblFieldsHint.Name = "_lblFieldsHint";
            this._lblFieldsHint.Text = "Numbers decimal or 0x…; \"add\"/\"subtract\" keep the value inside its range.";
            // 
            // _grpFlags
            // 
            this._grpFlags.Controls.Add(this._gridFlags);
            this._grpFlags.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grpFlags.Name = "_grpFlags";
            this._grpFlags.Padding = new System.Windows.Forms.Padding(6);
            this._grpFlags.TabIndex = 43;
            this._grpFlags.Text = "Flags";
            // 
            // _gridFlags
            // 
            this._gridFlags.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colFlag,
            this._colSet,
            this._colClear,
            this._colHave});
            this._gridFlags.AllowUserToAddRows = false;
            this._gridFlags.AllowUserToDeleteRows = false;
            this._gridFlags.AllowUserToResizeRows = false;
            this._gridFlags.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._gridFlags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._gridFlags.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gridFlags.Name = "_gridFlags";
            this._gridFlags.RowHeadersVisible = false;
            this._gridFlags.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this._gridFlags.TabIndex = 44;
            this._gridFlags.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnFlagCellChanged);
            this._gridFlags.CurrentCellDirtyStateChanged += new System.EventHandler(this.OnGridDirtyStateChanged);
            // 
            // _colFlag
            // 
            this._colFlag.FillWeight = 50F;
            this._colFlag.HeaderText = "Flag";
            this._colFlag.Name = "_colFlag";
            this._colFlag.ReadOnly = true;
            // 
            // _colSet
            // 
            this._colSet.FillWeight = 18F;
            this._colSet.HeaderText = "set";
            this._colSet.Name = "_colSet";
            // 
            // _colClear
            // 
            this._colClear.FillWeight = 18F;
            this._colClear.HeaderText = "remove";
            this._colClear.Name = "_colClear";
            // 
            // _colHave
            // 
            this._colHave.FillWeight = 24F;
            this._colHave.HeaderText = "have it";
            this._colHave.Name = "_colHave";
            this._colHave.ReadOnly = true;
            // 
            // _grpTarget
            // 
            this._grpTarget.Controls.Add(this._tblTarget);
            this._grpTarget.Dock = System.Windows.Forms.DockStyle.Top;
            this._grpTarget.Name = "_grpTarget";
            this._grpTarget.Padding = new System.Windows.Forms.Padding(6);
            this._grpTarget.Size = new System.Drawing.Size(884, 150);
            this._grpTarget.TabIndex = 45;
            this._grpTarget.Text = "Which tiles";
            // 
            // _tblTarget
            // 
            this._tblTarget.Controls.Add(this._rbSelected, 0, 0);
            this._tblTarget.Controls.Add(this._lblSelected, 1, 0);
            this._tblTarget.Controls.Add(this._rbRange, 0, 1);
            this._tblTarget.Controls.Add(this._txtRange, 1, 1);
            this._tblTarget.Controls.Add(this._rbVisible, 0, 2);
            this._tblTarget.Controls.Add(this._lblVisible, 1, 2);
            this._tblTarget.Controls.Add(this._chkOnlyName, 0, 3);
            this._tblTarget.Controls.Add(this._txtOnlyName, 1, 3);
            this._tblTarget.Controls.Add(this._chkOnlyFlag, 2, 3);
            this._tblTarget.Controls.Add(this._cmbOnlyFlag, 3, 3);
            this._tblTarget.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._tblTarget.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._tblTarget.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._tblTarget.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._tblTarget.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblTarget.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblTarget.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblTarget.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblTarget.ColumnCount = 4;
            this._tblTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tblTarget.Name = "_tblTarget";
            this._tblTarget.RowCount = 4;
            this._tblTarget.TabIndex = 46;
            // 
            // _rbSelected
            // 
            this._rbSelected.AutoSize = true;
            this._rbSelected.Checked = true;
            this._rbSelected.Name = "_rbSelected";
            this._rbSelected.TabIndex = 47;
            this._rbSelected.Text = "Selected tiles";
            this._rbSelected.UseVisualStyleBackColor = true;
            this._rbSelected.CheckedChanged += new System.EventHandler(this.OnTargetChanged);
            // 
            // _lblSelected
            // 
            this._tblTarget.SetColumnSpan(this._lblSelected, 3);
            this._lblSelected.AutoEllipsis = true;
            this._lblSelected.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblSelected.Name = "_lblSelected";
            this._lblSelected.Text = "-";
            this._lblSelected.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _rbRange
            // 
            this._rbRange.AutoSize = true;
            this._rbRange.Name = "_rbRange";
            this._rbRange.TabIndex = 48;
            this._rbRange.Text = "ID range:";
            this._rbRange.UseVisualStyleBackColor = true;
            this._rbRange.CheckedChanged += new System.EventHandler(this.OnTargetChanged);
            // 
            // _txtRange
            // 
            this._tblTarget.SetColumnSpan(this._txtRange, 3);
            this._txtRange.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtRange.Name = "_txtRange";
            this._txtRange.PlaceholderText = "e.g. 0x0B2C-0x0B40, 0x0C00, 3000..3010";
            this._txtRange.TabIndex = 49;
            this._txtRange.TextChanged += new System.EventHandler(this.OnRangeTextChanged);
            // 
            // _rbVisible
            // 
            this._rbVisible.AutoSize = true;
            this._rbVisible.Name = "_rbVisible";
            this._rbVisible.TabIndex = 50;
            this._rbVisible.Text = "All tiles of the list";
            this._rbVisible.UseVisualStyleBackColor = true;
            this._rbVisible.CheckedChanged += new System.EventHandler(this.OnTargetChanged);
            // 
            // _lblVisible
            // 
            this._tblTarget.SetColumnSpan(this._lblVisible, 3);
            this._lblVisible.AutoEllipsis = true;
            this._lblVisible.Dock = System.Windows.Forms.DockStyle.Fill;
            this._lblVisible.ForeColor = System.Drawing.SystemColors.GrayText;
            this._lblVisible.Name = "_lblVisible";
            this._lblVisible.Text = "(with the filter of the tab)";
            this._lblVisible.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _chkOnlyName
            // 
            this._chkOnlyName.AutoSize = true;
            this._chkOnlyName.Name = "_chkOnlyName";
            this._chkOnlyName.TabIndex = 51;
            this._chkOnlyName.Text = "only names containing:";
            this._chkOnlyName.UseVisualStyleBackColor = true;
            this._chkOnlyName.CheckedChanged += new System.EventHandler(this.OnTargetChanged);
            // 
            // _txtOnlyName
            // 
            this._txtOnlyName.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtOnlyName.Name = "_txtOnlyName";
            this._txtOnlyName.TabIndex = 52;
            this._txtOnlyName.TextChanged += new System.EventHandler(this.OnTargetChanged);
            // 
            // _chkOnlyFlag
            // 
            this._chkOnlyFlag.AutoSize = true;
            this._chkOnlyFlag.Name = "_chkOnlyFlag";
            this._chkOnlyFlag.TabIndex = 53;
            this._chkOnlyFlag.Text = "only with flag:";
            this._chkOnlyFlag.UseVisualStyleBackColor = true;
            this._chkOnlyFlag.CheckedChanged += new System.EventHandler(this.OnTargetChanged);
            // 
            // _cmbOnlyFlag
            // 
            this._cmbOnlyFlag.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cmbOnlyFlag.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbOnlyFlag.Name = "_cmbOnlyFlag";
            this._cmbOnlyFlag.TabIndex = 54;
            this._cmbOnlyFlag.SelectedIndexChanged += new System.EventHandler(this.OnTargetChanged);
            // 
            // _bottom
            // 
            this._bottom.Controls.Add(this._btnClose);
            this._bottom.Controls.Add(this._btnApply);
            this._bottom.Controls.Add(this._lblPreview);
            this._bottom.AutoSize = true;
            this._bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._bottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this._bottom.Name = "_bottom";
            this._bottom.Padding = new System.Windows.Forms.Padding(6);
            this._bottom.TabIndex = 55;
            // 
            // _btnClose
            // 
            this._btnClose.AutoSize = true;
            this._btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(90, 27);
            this._btnClose.TabIndex = 56;
            this._btnClose.Text = "Close";
            this._btnClose.UseVisualStyleBackColor = true;
            // 
            // _btnApply
            // 
            this._btnApply.AutoSize = true;
            this._btnApply.Name = "_btnApply";
            this._btnApply.Size = new System.Drawing.Size(90, 27);
            this._btnApply.TabIndex = 57;
            this._btnApply.Text = "Apply";
            this._btnApply.UseVisualStyleBackColor = true;
            this._btnApply.Click += new System.EventHandler(this.OnClickApply);
            // 
            // _lblPreview
            // 
            this._lblPreview.AutoSize = true;
            this._lblPreview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblPreview.Margin = new System.Windows.Forms.Padding(3, 8, 20, 3);
            this._lblPreview.Name = "_lblPreview";
            this._lblPreview.Text = "-";
            // 
            // TileDataBatchForm
            // 
            this.AcceptButton = this._btnApply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnClose;
            this.ClientSize = new System.Drawing.Size(884, 620);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(760, 520);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TileData – batch edit";
            this.Controls.Add(this._split);
            this.Controls.Add(this._grpTarget);
            this.Controls.Add(this._bottom);
            this.Name = "TileDataBatchForm";
            this._split.Panel1.ResumeLayout(false);
            this._split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._split)).EndInit();
            this._split.ResumeLayout(false);
            this._grpFields.ResumeLayout(false);
            this._grpFields.PerformLayout();
            this._tblFields.ResumeLayout(false);
            this._tblFields.PerformLayout();
            this._grpFlags.ResumeLayout(false);
            this._grpFlags.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridFlags)).EndInit();
            this._grpTarget.ResumeLayout(false);
            this._grpTarget.PerformLayout();
            this._tblTarget.ResumeLayout(false);
            this._tblTarget.PerformLayout();
            this._bottom.ResumeLayout(false);
            this._bottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer _split;
        private System.Windows.Forms.GroupBox _grpFields;
        private System.Windows.Forms.TableLayoutPanel _tblFields;
        private System.Windows.Forms.CheckBox _chkFName;
        private System.Windows.Forms.ComboBox _cmbOpName;
        private System.Windows.Forms.TextBox _txtName;
        private System.Windows.Forms.TextBox _txtNameWith;
        private System.Windows.Forms.CheckBox _chkFWeight;
        private System.Windows.Forms.ComboBox _cmbOpWeight;
        private System.Windows.Forms.TextBox _txtWeight;
        private System.Windows.Forms.CheckBox _chkFQuality;
        private System.Windows.Forms.ComboBox _cmbOpQuality;
        private System.Windows.Forms.TextBox _txtQuality;
        private System.Windows.Forms.CheckBox _chkFAnimation;
        private System.Windows.Forms.ComboBox _cmbOpAnimation;
        private System.Windows.Forms.TextBox _txtAnimation;
        private System.Windows.Forms.CheckBox _chkFQuantity;
        private System.Windows.Forms.ComboBox _cmbOpQuantity;
        private System.Windows.Forms.TextBox _txtQuantity;
        private System.Windows.Forms.CheckBox _chkFHue;
        private System.Windows.Forms.ComboBox _cmbOpHue;
        private System.Windows.Forms.TextBox _txtHue;
        private System.Windows.Forms.CheckBox _chkFStackingOffset;
        private System.Windows.Forms.ComboBox _cmbOpStackingOffset;
        private System.Windows.Forms.TextBox _txtStackingOffset;
        private System.Windows.Forms.CheckBox _chkFValue;
        private System.Windows.Forms.ComboBox _cmbOpValue;
        private System.Windows.Forms.TextBox _txtValue;
        private System.Windows.Forms.CheckBox _chkFHeight;
        private System.Windows.Forms.ComboBox _cmbOpHeight;
        private System.Windows.Forms.TextBox _txtHeight;
        private System.Windows.Forms.CheckBox _chkFMiscData;
        private System.Windows.Forms.ComboBox _cmbOpMiscData;
        private System.Windows.Forms.TextBox _txtMiscData;
        private System.Windows.Forms.CheckBox _chkFUnk2;
        private System.Windows.Forms.ComboBox _cmbOpUnk2;
        private System.Windows.Forms.TextBox _txtUnk2;
        private System.Windows.Forms.CheckBox _chkFUnk3;
        private System.Windows.Forms.ComboBox _cmbOpUnk3;
        private System.Windows.Forms.TextBox _txtUnk3;
        private System.Windows.Forms.CheckBox _chkFTextureId;
        private System.Windows.Forms.ComboBox _cmbOpTextureId;
        private System.Windows.Forms.TextBox _txtTextureId;
        private System.Windows.Forms.Label _lblFieldsHint;
        private System.Windows.Forms.GroupBox _grpFlags;
        private System.Windows.Forms.DataGridView _gridFlags;
        private System.Windows.Forms.DataGridViewTextBoxColumn _colFlag;
        private System.Windows.Forms.DataGridViewCheckBoxColumn _colSet;
        private System.Windows.Forms.DataGridViewCheckBoxColumn _colClear;
        private System.Windows.Forms.DataGridViewTextBoxColumn _colHave;
        private System.Windows.Forms.GroupBox _grpTarget;
        private System.Windows.Forms.TableLayoutPanel _tblTarget;
        private System.Windows.Forms.RadioButton _rbSelected;
        private System.Windows.Forms.Label _lblSelected;
        private System.Windows.Forms.RadioButton _rbRange;
        private System.Windows.Forms.TextBox _txtRange;
        private System.Windows.Forms.RadioButton _rbVisible;
        private System.Windows.Forms.Label _lblVisible;
        private System.Windows.Forms.CheckBox _chkOnlyName;
        private System.Windows.Forms.TextBox _txtOnlyName;
        private System.Windows.Forms.CheckBox _chkOnlyFlag;
        private System.Windows.Forms.ComboBox _cmbOnlyFlag;
        private System.Windows.Forms.FlowLayoutPanel _bottom;
        private System.Windows.Forms.Button _btnClose;
        private System.Windows.Forms.Button _btnApply;
        private System.Windows.Forms.Label _lblPreview;
    }
}
