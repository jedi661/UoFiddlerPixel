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

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Helpers;

namespace UoFiddler.Controls.ArtLibrary.Forms
{
    /// <summary>
    /// Dialog for every import/export/copy/move: optional source ids, target id (decimal or hex),
    /// "keep ids" vs. "one after another", overwrite and transparency options.
    /// Fields that a caller does not need are hidden with the Show* properties.
    /// </summary>
    public partial class ArtLibraryTargetIdForm : Form
    {
        private int _maxTargetId = int.MaxValue;
        private int _maxSourceId = int.MaxValue;

        // Control.Visible is false while the dialog is not shown, so the options are kept in fields
        private bool _showSource = true;
        private bool _showModeOptions = true;
        private bool _showOverwrite = true;
        private bool _showTransparency = true;

        public ArtLibraryTargetIdForm()
        {
            InitializeComponent();
        }

        #region [ Configuration ]

        public string Title
        {
            get => Text;
            set => Text = value;
        }

        public string InfoText
        {
            get => labelInfo.Text;
            set => labelInfo.Text = value;
        }

        public string SourceLabel
        {
            get => labelSource.Text;
            set => labelSource.Text = value;
        }

        public string TargetLabel
        {
            get => labelTarget.Text;
            set => labelTarget.Text = value;
        }

        public string KeepIdsText
        {
            get => radioButtonKeepIds.Text;
            set => radioButtonKeepIds.Text = value;
        }

        public bool ShowSource
        {
            get => _showSource;
            set => labelSource.Visible = textBoxSource.Visible = _showSource = value;
        }

        public bool ShowModeOptions
        {
            get => _showModeOptions;
            set => radioButtonKeepIds.Visible = radioButtonSequential.Visible = _showModeOptions = value;
        }

        public bool ShowOverwrite
        {
            get => _showOverwrite;
            set => checkBoxOverwrite.Visible = _showOverwrite = value;
        }

        public bool ShowTransparencyOptions
        {
            get => _showTransparency;
            set => groupBoxTransparency.Visible = _showTransparency = value;
        }

        /// <summary>Highest allowed target id (validation).</summary>
        public int MaxTargetId
        {
            get => _maxTargetId;
            set => _maxTargetId = value;
        }

        /// <summary>Highest allowed source id (validation).</summary>
        public int MaxSourceId
        {
            get => _maxSourceId;
            set => _maxSourceId = value;
        }

        #endregion

        #region [ Values ]

        public string SourceText
        {
            get => textBoxSource.Text;
            set => textBoxSource.Text = value ?? string.Empty;
        }

        /// <summary>Parsed source ids (valid after OK).</summary>
        public List<int> SourceIds { get; private set; } = new List<int>();

        public int TargetId
        {
            get => ArtIdParser.TryParse(textBoxTarget.Text, out int id) ? id : -1;
            set => textBoxTarget.Text = value < 0 ? string.Empty : ArtIdParser.ToHex(value);
        }

        /// <summary>True = target id = source id; false = one after another from <see cref="TargetId"/>.</summary>
        public bool KeepIds
        {
            get => _showModeOptions && radioButtonKeepIds.Checked;
            set
            {
                radioButtonKeepIds.Checked = value;
                radioButtonSequential.Checked = !value;
            }
        }

        public bool Overwrite
        {
            get => _showOverwrite && checkBoxOverwrite.Checked;
            set => checkBoxOverwrite.Checked = value;
        }

        public ArtImportOptions ImportOptions
        {
            get => new ArtImportOptions
            {
                BlackTransparent = checkBoxBlack.Checked,
                WhiteTransparent = checkBoxWhite.Checked,
                LightGrayTransparent = checkBoxWhite.Checked
            };
            set
            {
                value ??= ArtImportOptions.Default;
                checkBoxBlack.Checked = value.BlackTransparent;
                checkBoxWhite.Checked = value.WhiteTransparent;
            }
        }

        #endregion

        private void RadioButtonKeepIds_CheckedChanged(object sender, EventArgs e)
        {
            textBoxTarget.Enabled = !_showModeOptions || !radioButtonKeepIds.Checked;
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            if (ShowSource)
            {
                if (!ArtIdParser.TryParseList(textBoxSource.Text, out List<int> ids))
                {
                    MessageBox.Show(this, "Bitte gültige Quell-IDs eingeben, z. B. 0x1000-0x10FF, 1234, 5000-5010.",
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxSource.Focus();
                    return;
                }

                if (ids.Exists(id => id > _maxSourceId))
                {
                    MessageBox.Show(this, $"Quell-IDs dürfen höchstens {ArtIdParser.ToDisplay(_maxSourceId)} sein.",
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxSource.Focus();
                    return;
                }

                SourceIds = ids;
            }

            if (textBoxTarget.Enabled)
            {
                int target = TargetId;
                if (target < 0 || target > _maxTargetId)
                {
                    MessageBox.Show(this, $"Bitte eine gültige Ziel-ID eingeben (0 bis {ArtIdParser.ToDisplay(_maxTargetId)}),\n" +
                                          "dezimal (1234) oder hexadezimal (0x4D2, 4D2h).",
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBoxTarget.Focus();
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ArtLibraryTargetIdForm_Shown(object sender, EventArgs e)
        {
            RadioButtonKeepIds_CheckedChanged(sender, e);
            if (_showSource)
            {
                textBoxSource.Focus();
                textBoxSource.SelectAll();
            }
            else
            {
                textBoxTarget.Focus();
                textBoxTarget.SelectAll();
            }
        }
    }
}
