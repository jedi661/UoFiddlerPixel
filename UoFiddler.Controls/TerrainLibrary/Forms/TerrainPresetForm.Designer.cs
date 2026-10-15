// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Controls.TerrainLibrary.Forms
{
    partial class TerrainPresetForm
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
            listBoxPresets = new System.Windows.Forms.ListBox();
            groupBoxList = new System.Windows.Forms.GroupBox();
            labelName = new System.Windows.Forms.Label();
            labelTileName = new System.Windows.Forms.Label();
            labelTextureId = new System.Windows.Forms.Label();
            labelFlags = new System.Windows.Forms.Label();
            labelColor = new System.Windows.Forms.Label();
            labelCategory = new System.Windows.Forms.Label();
            labelDescription = new System.Windows.Forms.Label();
            textBoxName = new System.Windows.Forms.TextBox();
            textBoxTileName = new System.Windows.Forms.TextBox();
            textBoxTextureId = new System.Windows.Forms.TextBox();
            checkedListBoxFlags = new System.Windows.Forms.CheckedListBox();
            panelColor = new System.Windows.Forms.Panel();
            textBoxColor = new System.Windows.Forms.TextBox();
            buttonPickColor = new System.Windows.Forms.Button();
            panelColorRow = new System.Windows.Forms.Panel();
            comboBoxCategory = new System.Windows.Forms.ComboBox();
            textBoxDescription = new System.Windows.Forms.TextBox();
            labelBuiltIn = new System.Windows.Forms.Label();
            tableLayoutPanelFields = new System.Windows.Forms.TableLayoutPanel();
            groupBoxFields = new System.Windows.Forms.GroupBox();
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            buttonNew = new System.Windows.Forms.Button();
            buttonFromSlot = new System.Windows.Forms.Button();
            buttonSave = new System.Windows.Forms.Button();
            buttonDelete = new System.Windows.Forms.Button();
            buttonClose = new System.Windows.Forms.Button();
            flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
            colorDialog = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            groupBoxList.SuspendLayout();
            panelColorRow.SuspendLayout();
            tableLayoutPanelFields.SuspendLayout();
            groupBoxFields.SuspendLayout();
            flowLayoutPanelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // listBoxPresets
            // 
            listBoxPresets.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxPresets.IntegralHeight = false;
            listBoxPresets.ItemHeight = 15;
            listBoxPresets.Name = "listBoxPresets";
            listBoxPresets.SelectedIndexChanged += ListBoxPresets_SelectedIndexChanged;
            // 
            // groupBoxList
            // 
            groupBoxList.Controls.Add(listBoxPresets);
            groupBoxList.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxList.Name = "groupBoxList";
            groupBoxList.Text = "Vorlagen";
            // 
            // labelName
            // 
            labelName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelName.AutoSize = true;
            labelName.Name = "labelName";
            labelName.Text = "Name:";
            // 
            // labelTileName
            // 
            labelTileName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelTileName.AutoSize = true;
            labelTileName.Name = "labelTileName";
            labelTileName.Text = "tiledata-Name:";
            // 
            // labelTextureId
            // 
            labelTextureId.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelTextureId.AutoSize = true;
            labelTextureId.Name = "labelTextureId";
            labelTextureId.Text = "Textur-ID:";
            // 
            // labelFlags
            // 
            labelFlags.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelFlags.AutoSize = true;
            labelFlags.Name = "labelFlags";
            labelFlags.Text = "Flags:";
            // 
            // labelColor
            // 
            labelColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelColor.AutoSize = true;
            labelColor.Name = "labelColor";
            labelColor.Text = "RadarColor:";
            // 
            // labelCategory
            // 
            labelCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelCategory.AutoSize = true;
            labelCategory.Name = "labelCategory";
            labelCategory.Text = "Kategorie:";
            // 
            // labelDescription
            // 
            labelDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            labelDescription.AutoSize = true;
            labelDescription.Name = "labelDescription";
            labelDescription.Text = "Beschreibung:";
            // 
            // textBoxName
            // 
            textBoxName.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxName.Name = "textBoxName";
            // 
            // textBoxTileName
            // 
            textBoxTileName.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxTileName.MaxLength = 20;
            textBoxTileName.Name = "textBoxTileName";
            // 
            // textBoxTextureId
            // 
            textBoxTextureId.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxTextureId.Name = "textBoxTextureId";
            textBoxTextureId.PlaceholderText = "leer = Textur-ID des Slots behalten";
            // 
            // checkedListBoxFlags
            // 
            checkedListBoxFlags.CheckOnClick = true;
            checkedListBoxFlags.ColumnWidth = 120;
            checkedListBoxFlags.Dock = System.Windows.Forms.DockStyle.Fill;
            checkedListBoxFlags.IntegralHeight = false;
            checkedListBoxFlags.MultiColumn = true;
            checkedListBoxFlags.Name = "checkedListBoxFlags";
            // 
            // panelColor
            // 
            panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelColor.Location = new System.Drawing.Point(0, 2);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(24, 22);
            // 
            // textBoxColor
            // 
            textBoxColor.Location = new System.Drawing.Point(30, 2);
            textBoxColor.Name = "textBoxColor";
            textBoxColor.PlaceholderText = "0x1234 / #RRGGBB";
            textBoxColor.Size = new System.Drawing.Size(130, 23);
            textBoxColor.Leave += TextBoxColor_Leave;
            // 
            // buttonPickColor
            // 
            buttonPickColor.Location = new System.Drawing.Point(166, 1);
            buttonPickColor.Name = "buttonPickColor";
            buttonPickColor.Size = new System.Drawing.Size(80, 25);
            buttonPickColor.Text = "Wählen...";
            buttonPickColor.UseVisualStyleBackColor = true;
            buttonPickColor.Click += ButtonPickColor_Click;
            // 
            // panelColorRow
            // 
            panelColorRow.Controls.Add(buttonPickColor);
            panelColorRow.Controls.Add(textBoxColor);
            panelColorRow.Controls.Add(panelColor);
            panelColorRow.Dock = System.Windows.Forms.DockStyle.Fill;
            panelColorRow.Name = "panelColorRow";
            // 
            // comboBoxCategory
            // 
            comboBoxCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxCategory.Name = "comboBoxCategory";
            // 
            // textBoxDescription
            // 
            textBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxDescription.Multiline = true;
            textBoxDescription.Name = "textBoxDescription";
            // 
            // labelBuiltIn
            // 
            labelBuiltIn.Dock = System.Windows.Forms.DockStyle.Fill;
            labelBuiltIn.ForeColor = System.Drawing.Color.DarkBlue;
            labelBuiltIn.Name = "labelBuiltIn";
            labelBuiltIn.Text = "Standardvorlage: Änderungen werden mit \"Speichern\" als eigene Vorlage abgelegt.";
            labelBuiltIn.Visible = false;
            // 
            // tableLayoutPanelFields
            // 
            tableLayoutPanelFields.ColumnCount = 2;
            tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tableLayoutPanelFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanelFields.Controls.Add(labelName, 0, 0);
            tableLayoutPanelFields.Controls.Add(textBoxName, 1, 0);
            tableLayoutPanelFields.Controls.Add(labelTileName, 0, 1);
            tableLayoutPanelFields.Controls.Add(textBoxTileName, 1, 1);
            tableLayoutPanelFields.Controls.Add(labelTextureId, 0, 2);
            tableLayoutPanelFields.Controls.Add(textBoxTextureId, 1, 2);
            tableLayoutPanelFields.Controls.Add(labelFlags, 0, 3);
            tableLayoutPanelFields.Controls.Add(checkedListBoxFlags, 1, 3);
            tableLayoutPanelFields.Controls.Add(labelColor, 0, 4);
            tableLayoutPanelFields.Controls.Add(panelColorRow, 1, 4);
            tableLayoutPanelFields.Controls.Add(labelCategory, 0, 5);
            tableLayoutPanelFields.Controls.Add(comboBoxCategory, 1, 5);
            tableLayoutPanelFields.Controls.Add(labelDescription, 0, 6);
            tableLayoutPanelFields.Controls.Add(textBoxDescription, 1, 6);
            tableLayoutPanelFields.Controls.Add(labelBuiltIn, 0, 7);
            tableLayoutPanelFields.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanelFields.Name = "tableLayoutPanelFields";
            tableLayoutPanelFields.RowCount = 8;
            tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            tableLayoutPanelFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            tableLayoutPanelFields.SetColumnSpan(labelBuiltIn, 2);
            // 
            // groupBoxFields
            // 
            groupBoxFields.Controls.Add(tableLayoutPanelFields);
            groupBoxFields.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxFields.Name = "groupBoxFields";
            groupBoxFields.Text = "Einstellungen der Vorlage";
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Panel1.Controls.Add(groupBoxList);
            splitContainerMain.Panel2.Controls.Add(groupBoxFields);
            splitContainerMain.Size = new System.Drawing.Size(784, 470);
            splitContainerMain.SplitterDistance = 230;
            // 
            // buttonNew
            // 
            buttonNew.Name = "buttonNew";
            buttonNew.Size = new System.Drawing.Size(70, 27);
            buttonNew.Text = "Neu";
            buttonNew.UseVisualStyleBackColor = true;
            buttonNew.Click += ButtonNew_Click;
            // 
            // buttonFromSlot
            // 
            buttonFromSlot.Name = "buttonFromSlot";
            buttonFromSlot.Size = new System.Drawing.Size(140, 27);
            buttonFromSlot.Text = "Aus Slot übernehmen";
            buttonFromSlot.UseVisualStyleBackColor = true;
            buttonFromSlot.Click += ButtonFromSlot_Click;
            // 
            // buttonSave
            // 
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new System.Drawing.Size(90, 27);
            buttonSave.Text = "Speichern";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += ButtonSave_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new System.Drawing.Size(80, 27);
            buttonDelete.Text = "Löschen";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += ButtonDelete_Click;
            // 
            // buttonClose
            // 
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(90, 27);
            buttonClose.Text = "Schließen";
            buttonClose.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanelButtons
            // 
            flowLayoutPanelButtons.Controls.Add(buttonNew);
            flowLayoutPanelButtons.Controls.Add(buttonFromSlot);
            flowLayoutPanelButtons.Controls.Add(buttonSave);
            flowLayoutPanelButtons.Controls.Add(buttonDelete);
            flowLayoutPanelButtons.Controls.Add(buttonClose);
            flowLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
            flowLayoutPanelButtons.Padding = new System.Windows.Forms.Padding(6);
            flowLayoutPanelButtons.Size = new System.Drawing.Size(784, 42);
            // 
            // TerrainPresetForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(784, 512);
            Controls.Add(splitContainerMain);
            Controls.Add(flowLayoutPanelButtons);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(640, 420);
            Name = "TerrainPresetForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Vorlagen verwalten (Einstellungen für Slots)";
            flowLayoutPanelButtons.ResumeLayout(false);
            flowLayoutPanelButtons.PerformLayout();
            groupBoxFields.ResumeLayout(false);
            groupBoxFields.PerformLayout();
            tableLayoutPanelFields.ResumeLayout(false);
            tableLayoutPanelFields.PerformLayout();
            panelColorRow.ResumeLayout(false);
            panelColorRow.PerformLayout();
            groupBoxList.ResumeLayout(false);
            groupBoxList.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox listBoxPresets;
        private System.Windows.Forms.GroupBox groupBoxList;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelTileName;
        private System.Windows.Forms.Label labelTextureId;
        private System.Windows.Forms.Label labelFlags;
        private System.Windows.Forms.Label labelColor;
        private System.Windows.Forms.Label labelCategory;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.TextBox textBoxTileName;
        private System.Windows.Forms.TextBox textBoxTextureId;
        private System.Windows.Forms.CheckedListBox checkedListBoxFlags;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.TextBox textBoxColor;
        private System.Windows.Forms.Button buttonPickColor;
        private System.Windows.Forms.Panel panelColorRow;
        private System.Windows.Forms.ComboBox comboBoxCategory;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label labelBuiltIn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFields;
        private System.Windows.Forms.GroupBox groupBoxFields;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Button buttonFromSlot;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
        private System.Windows.Forms.ColorDialog colorDialog;
    }
}
