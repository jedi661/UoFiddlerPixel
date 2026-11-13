using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>Shows what a ServUO script gives before it is imported: gump classes, picture of the gump and the hints.</summary>
    public partial class GumpImportForm : Form
    {
        private readonly string _file;
        private readonly string _code;
        private readonly Dictionary<string, (GumpDesign Design, List<string> Hints)> _results = new Dictionary<string, (GumpDesign, List<string>)>();

        public GumpImportForm()
        {
            InitializeComponent();
        }

        public GumpImportForm(string file, string code)
            : this()
        {
            _file = file;
            _code = code ?? string.Empty;
            Text = "Import – " + System.IO.Path.GetFileName(file);
            List<string> classes = GumpImporter.FindGumpClasses(_code);
            if (classes.Count == 0)
            {
                classes.Add(string.Empty);   // loose Add… lines
            }
            foreach (string c in classes)
            {
                listClasses.Items.Add(c.Length == 0 ? "(Add… lines)" : c);
            }
            splitMain.Panel1Collapsed = classes.Count < 2;
        }

        /// <summary>The imported gump of the chosen class.</summary>
        public GumpDesign Result { get; private set; }

        public List<string> Hints { get; private set; } = new List<string>();

        /// <summary>Values of other classes were read from the scripts around the file.</summary>
        public bool Resolved => chkResolve.Checked;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (listClasses.Items.Count > 0)
            {
                listClasses.SelectedIndex = 0;
            }
        }

        private string ClassName => listClasses.SelectedItem is string s && !s.StartsWith("(", StringComparison.Ordinal) ? s : null;

        private (GumpDesign Design, List<string> Hints) Current()
        {
            string key = (ClassName ?? string.Empty) + "|" + chkResolve.Checked;
            if (!_results.TryGetValue(key, out (GumpDesign Design, List<string> Hints) r))
            {
                var hints = new List<string>();
                Cursor = Cursors.WaitCursor;
                try
                {
                    GumpDesign d = GumpImporter.Import(_code, hints, ClassName, chkResolve.Checked && _file != null ? GumpImporter.FolderResolver(_file) : null);
                    r = (d, hints);
                }
                catch (Exception ex)
                {
                    hints.Add("Import failed: " + ex.Message);
                    r = (new GumpDesign(), hints);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
                _results[key] = r;
            }
            return r;
        }

        private void ShowCurrent()
        {
            (GumpDesign d, List<string> hints) = Current();
            numPage.Maximum = Math.Max(1, d.Pages);
            listHints.BeginUpdate();
            listHints.Items.Clear();
            foreach (string h in hints)
            {
                listHints.Items.Add(h);
            }
            if (hints.Count == 0)
            {
                listHints.Items.Add("Everything could be read. / Alles konnte gelesen werden.");
            }
            listHints.EndUpdate();
            lblInfo.Text = $"{d.ClassName} · {d.Elements.Count} elements · {d.Pages} page(s) · {hints.Count} hint(s)" + (string.IsNullOrEmpty(d.Command) ? string.Empty : $" · command [{d.Command}");
            btnImport.Enabled = d.Elements.Count > 0;
            ShowPage();
        }

        private void ShowPage()
        {
            picturePreview.Image?.Dispose();
            picturePreview.Image = null;
            try
            {
                picturePreview.Image = GumpRenderer.RenderImage(Current().Design, (int)numPage.Value);
            }
            catch (Exception)
            {
                // no client files
            }
        }

        private void ListClasses_SelectedIndexChanged(object sender, EventArgs e) => ShowCurrent();

        private void ChkResolve_CheckedChanged(object sender, EventArgs e) => ShowCurrent();

        private void NumPage_ValueChanged(object sender, EventArgs e) => ShowPage();

        private void BtnImport_Click(object sender, EventArgs e)
        {
            (GumpDesign d, List<string> hints) = Current();
            if (d.Elements.Count == 0)
            {
                return;
            }
            Result = d;
            Hints = hints;
            DialogResult = DialogResult.OK;
        }
    }
}
