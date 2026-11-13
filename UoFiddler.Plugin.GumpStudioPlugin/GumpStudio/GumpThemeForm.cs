using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>Themes: choose a look, see your gump with it, apply it; make themes from a gump and save them.</summary>
    public partial class GumpThemeForm : Form
    {
        private readonly GumpDesign _design;

        public GumpThemeForm()
        {
            InitializeComponent();
        }

        public GumpThemeForm(GumpDesign design)
            : this()
        {
            _design = design;
            Reload(null);
        }

        public GumpTheme Theme => listThemes.SelectedItem as GumpTheme;

        public bool Frames => chkFrames.Checked;

        public bool Buttons => chkButtons.Checked;

        public bool Switches => chkSwitches.Checked;

        public bool Texts => chkTexts.Checked;

        private void Reload(string select)
        {
            var errors = new List<string>();
            listThemes.BeginUpdate();
            listThemes.Items.Clear();
            foreach (GumpTheme t in GumpThemes.BuiltIn().Concat(GumpThemes.LoadUser(errors)))
            {
                listThemes.Items.Add(t);
            }
            listThemes.EndUpdate();
            int index = select == null ? 0 : listThemes.Items.Cast<GumpTheme>().ToList().FindIndex(t => !t.BuiltIn && t.Name == select);
            listThemes.SelectedIndex = Math.Max(0, Math.Min(listThemes.Items.Count - 1, index));
            if (errors.Count > 0)
            {
                lblStatus.Text = string.Join("; ", errors);
            }
        }

        private void ListThemes_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyTheme.SelectedObject = Theme;
            btnDeleteTheme.Enabled = Theme != null && !Theme.BuiltIn;
            UpdatePreview();
        }

        private void PropertyTheme_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            listThemes.Items[listThemes.SelectedIndex] = Theme;
            UpdatePreview();
        }

        private void Option_Changed(object sender, EventArgs e) => UpdatePreview();

        private void UpdatePreview()
        {
            picturePreview.Image?.Dispose();
            picturePreview.Image = null;
            if (Theme == null || _design == null)
            {
                return;
            }
            GumpDesign d = _design.Clone();
            int n = GumpThemes.Apply(d, Theme, Frames, Buttons, Switches, Texts);
            try
            {
                picturePreview.Image = GumpRenderer.RenderImage(d, 1);
            }
            catch (Exception)
            {
                // no client files
            }
            lblStatus.Text = $"{n} element(s) change.";
        }

        private void BtnFromGump_Click(object sender, EventArgs e)
        {
            GumpTheme t = GumpThemes.FromDesign(_design, _design.ClassName + " theme");
            GumpThemes.Save(t);
            Reload(t.Name);
            lblStatus.Text = $"Theme \"{t.Name}\" made from your gump and saved – rename it on the right (Name) and press Save.";
        }

        private void BtnSaveTheme_Click(object sender, EventArgs e)
        {
            GumpTheme t = Theme;
            if (t == null)
            {
                return;
            }
            try
            {
                GumpTheme copy = t.Clone();
                if (copy.BuiltIn)
                {
                    copy.Name += " (copy)";
                }
                GumpThemes.Save(copy);
                Reload(copy.Name);
                lblStatus.Text = $"Saved to {GumpThemes.FileOf(copy)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteTheme_Click(object sender, EventArgs e)
        {
            GumpTheme t = Theme;
            if (t == null || t.BuiltIn || MessageBox.Show(this, $"Delete the theme \"{t.Name}\"?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            GumpThemes.Delete(t);
            Reload(null);
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            if (Theme != null)
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
