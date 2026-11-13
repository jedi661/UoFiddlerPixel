using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>Shows the planned changes of the script (old / new line) and writes them with a backup.</summary>
    public partial class GumpWriteBackForm : Form
    {
        private readonly ScriptWritePlan _plan;
        private readonly string _file;

        public GumpWriteBackForm()
        {
            InitializeComponent();
        }

        public GumpWriteBackForm(ScriptWritePlan plan, string file)
            : this()
        {
            _plan = plan;
            _file = file;
            listChanges.Columns.Add("Line", 50);
            listChanges.Columns.Add("Change", 200);
            listChanges.Columns.Add("Old", 300);
            listChanges.Columns.Add("New", 300);
            foreach (ScriptChange c in plan.Changes)
            {
                listChanges.Items.Add(new ListViewItem(new[] { c.Line.ToString(), c.What, c.Old, c.New }));
            }
            foreach (string p in plan.Problems)
            {
                listProblems.Items.Add(p);
            }
            if (plan.Problems.Count == 0)
            {
                listProblems.Items.Add("Everything can be written. / Alles kann geschrieben werden.");
            }
            lblInfo.Text = $"{Path.GetFileName(file)}: {plan.Changes.Count} line(s) change, {plan.Problems.Count} thing(s) cannot be written. Only the numbers / texts shown are replaced – logic, comments and formatting stay.\r\n"
                           + $"DE: {plan.Changes.Count} Zeile(n) ändern sich, {plan.Problems.Count} Punkt(e) gehen nicht automatisch. Nur die gezeigten Zahlen / Texte werden ersetzt.";
            btnWrite.Enabled = plan.Changes.Count > 0;
        }

        /// <summary>Read the script again after writing.</summary>
        public bool Reimport => chkReimport.Checked;

        private void BtnWrite_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] old = File.ReadAllBytes(_file);
                bool bom = old.Length >= 3 && old[0] == 0xEF && old[1] == 0xBB && old[2] == 0xBF;
                if (chkBackup.Checked)
                {
                    File.Copy(_file, _file + ".bak", true);
                }
                File.WriteAllText(_file, _plan.NewCode, new UTF8Encoding(bom));
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The script could not be written:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
