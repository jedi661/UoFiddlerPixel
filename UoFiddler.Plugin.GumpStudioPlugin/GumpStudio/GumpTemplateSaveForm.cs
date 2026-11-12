using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>Asks for name, category and note of a template (also used to rename / move one).</summary>
    public partial class GumpTemplateSaveForm : Form
    {
        private readonly GumpDesign _whole;
        private readonly GumpDesign _selection;
        private readonly string _originalFile;

        public GumpTemplateSaveForm()
        {
            InitializeComponent();
        }

        /// <param name="whole">The whole gump.</param>
        /// <param name="selection">Only the selected elements (null = nothing selected).</param>
        /// <param name="originalFile">When renaming: the file of the template (it may be overwritten by itself).</param>
        public GumpTemplateSaveForm(GumpDesign whole, GumpDesign selection, string name, string category, string description, string originalFile = null)
            : this()
        {
            _whole = whole;
            _selection = selection;
            _originalFile = originalFile;
            foreach (string c in GumpTemplateLibrary.Categories().DefaultIfEmpty(GumpTemplateLibrary.DefaultCategory))
            {
                comboCategory.Items.Add(c);
            }
            txtName.Text = name ?? string.Empty;
            comboCategory.Text = string.IsNullOrWhiteSpace(category) ? GumpTemplateLibrary.DefaultCategory : category;
            txtDescription.Text = description ?? string.Empty;
            radioSelection.Enabled = selection != null && selection.Elements.Count > 0;
            if (originalFile != null)
            {
                // rename / move: content stays
                Text = "Rename / move template – Vorlage umbenennen / verschieben";
                flowContent.Visible = lblContent.Visible = false;
                btnOk.Text = "OK";
            }
            ShowPreview();
        }

        public string TemplateName => txtName.Text.Trim();

        public string Category => GumpTemplateLibrary.Safe(comboCategory.Text, true);

        public string Description => txtDescription.Text.Trim();

        public bool SelectionOnly => radioSelection.Checked;

        /// <summary>The design to save (whole gump or the building block).</summary>
        public GumpDesign Result => SelectionOnly ? _selection : _whole;

        /// <summary>Starts with "only the selected elements".</summary>
        public void SelectBuildingBlock()
        {
            if (radioSelection.Enabled)
            {
                radioSelection.Checked = true;
            }
        }

        private void ShowPreview()
        {
            picturePreview.Image?.Dispose();
            picturePreview.Image = null;
            GumpDesign d = Result;
            if (d == null)
            {
                return;
            }
            try
            {
                picturePreview.Image = GumpRenderer.RenderImage(d, 1);
            }
            catch (Exception)
            {
                // no client files loaded
            }
        }

        private void RadioContent_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                ShowPreview();
                if (SelectionOnly && txtName.Text == _whole?.ClassName)
                {
                    txtName.Text = "Building block";
                }
            }
        }

        private void TxtName_TextChanged(object sender, EventArgs e)
        {
            bool exists = TemplateName.Length > 0 && GumpTemplateLibrary.Exists(TemplateName, Category)
                          && !string.Equals(GumpTemplateLibrary.PathFor(TemplateName, Category), _originalFile, StringComparison.OrdinalIgnoreCase);
            lblHint.Text = exists
                ? $"\"{TemplateName}\" already exists in {Category} – it will be replaced. / Existiert schon – wird ersetzt."
                : "Saved to GumpTemplates\\<Category>\\<Name>.gump.json next to UoFiddler.";
            lblHint.ForeColor = exists ? Color.DarkOrange : SystemColors.GrayText;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (GumpTemplateLibrary.Safe(TemplateName).Length == 0)
            {
                MessageBox.Show(this, "Please enter a name. / Bitte einen Namen eingeben.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtName.Focus();
                return;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
