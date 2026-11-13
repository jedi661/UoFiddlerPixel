using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>
    /// List / table generator: columns, rows (pasted from Excel or typed), rows per page, button per row.
    /// Makes a template row – all other rows and the page buttons follow it, the script gets a for loop.
    /// </summary>
    public partial class GumpListForm : Form
    {
        private readonly GumpDesign _design;
        private readonly GumpListSpec _edit;
        private bool _loading;

        public GumpListForm()
        {
            InitializeComponent();
        }

        /// <param name="design">The gump (for the preview).</param>
        /// <param name="edit">A list to change, or null for a new one.</param>
        /// <param name="page">Page for a new list.</param>
        public GumpListForm(GumpDesign design, GumpListSpec edit, int page)
            : this()
        {
            _design = design;
            _edit = edit;
            _loading = true;
            if (edit != null)
            {
                Text = "Change list / Liste ändern – " + edit.Name;
                btnOk.Text = "Apply";
                txtName.Text = edit.Name;
                numPerPage.Value = Math.Min(numPerPage.Maximum, edit.RowsPerPage);
                numRowHeight.Value = Math.Max(numRowHeight.Minimum, Math.Min(numRowHeight.Maximum, edit.RowHeight));
                numX.Value = Clamp(numX, edit.X);
                numY.Value = Clamp(numY, edit.Y);
                numFirstPage.Value = Clamp(numFirstPage, edit.FirstPage);
                chkSelectButton.Checked = edit.SelectButton;
                chkSelectButton.Enabled = false;
                numButtonBase.Value = Clamp(numButtonBase, edit.ButtonBase);
                numNavY.Value = Clamp(numNavY, edit.NavY);
                numNavBack.Value = Clamp(numNavBack, edit.NavBackX);
                numNavNext.Value = Clamp(numNavNext, edit.NavNextX);
                txtColumns.Text = string.Join("\r\n", edit.Columns.Select(c => $"{c.Header}|{c.Width}|{c.Hue}"));
                var rows = new StringBuilder();
                for (int r = 0; r < edit.Rows; r++)
                {
                    rows.Append(string.Join(";", edit.Columns.Select((c, i) => edit.Text(i, r)))).Append("\r\n");
                }
                txtRows.Text = rows.ToString();
            }
            else
            {
                int bg = design.Elements.Where(e => e.Type == ElementType.Background).Select(e => e.Y + e.Height).DefaultIfEmpty(330).Max();
                numFirstPage.Value = Math.Max(1, page);
                numNavY.Value = Clamp(numNavY, bg - 35);
                int buttons = design.Elements.Where(e => e.Type == ElementType.Button).Select(e => e.ButtonId).DefaultIfEmpty(0).Max();
                numButtonBase.Value = Clamp(numButtonBase, Math.Max(100, (buttons / 100 + 1) * 100));
                txtColumns.Text = "Name|140|1152\r\nLevel|60|1152";
                BtnSample_Click(this, EventArgs.Empty);
            }
            _loading = false;
            UpdatePreview();
        }

        private static decimal Clamp(NumericUpDown n, int v) => Math.Max(n.Minimum, Math.Min(n.Maximum, v));

        /// <summary>The list as chosen (new) or the changed list.</summary>
        public GumpListSpec Result { get; private set; }

        /// <summary>Makes the list spec from the fields.</summary>
        private GumpListSpec BuildSpec()
        {
            var spec = new GumpListSpec
            {
                Name = string.IsNullOrWhiteSpace(txtName.Text) ? "List" : txtName.Text.Trim(),
                RowsPerPage = (int)numPerPage.Value,
                RowHeight = (int)numRowHeight.Value,
                X = (int)numX.Value,
                Y = (int)numY.Value,
                FirstPage = (int)numFirstPage.Value,
                SelectButton = chkSelectButton.Checked,
                ButtonBase = (int)numButtonBase.Value,
                SwitchBase = (int)numButtonBase.Value,
                EntryBase = (int)numButtonBase.Value,
                NavY = (int)numNavY.Value,
                NavBackX = (int)numNavBack.Value,
                NavNextX = (int)numNavNext.Value
            };
            if (_edit != null)
            {
                spec.Id = _edit.Id;
            }
            foreach (string line in txtColumns.Lines.Where(l => l.Trim().Length > 0))
            {
                string[] p = line.Split('|');
                spec.Columns.Add(new GumpListColumn
                {
                    Header = p[0].Trim(),
                    Width = p.Length > 1 && int.TryParse(p[1].Trim(), out int w) ? Math.Max(10, w) : 120,
                    Hue = p.Length > 2 && int.TryParse(p[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int h) ? h : 1152
                });
            }
            List<string[]> rows = txtRows.Lines.Where(l => l.Trim().Length > 0)
                .Select(l => l.Split(l.Contains("\t") ? '\t' : ';').Select(x => x.Trim()).ToArray()).ToList();
            int columns = Math.Max(spec.Columns.Count, rows.Select(r => r.Length).DefaultIfEmpty(1).Max());
            while (spec.Columns.Count < columns)
            {
                spec.Columns.Add(new GumpListColumn { Header = string.Empty, Width = 100 });
            }
            for (int c = 0; c < spec.Columns.Count; c++)
            {
                spec.Columns[c].Texts = rows.Select(r => c < r.Length ? r[c] : string.Empty).ToList();
            }
            spec.Rows = Math.Max(1, rows.Count);
            if (!spec.Paged && spec.FirstPage > 0 && _design.Pages <= 1 && !_design.Elements.Any(e => e.Page > 0))
            {
                // one page without paging: put it on page 0 when the gump has no pages
                spec.FirstPage = 0;
            }
            if (spec.Paged && spec.FirstPage < 1)
            {
                spec.FirstPage = 1;
            }
            return spec;
        }

        private void Option_Changed(object sender, EventArgs e)
        {
            if (!_loading)
            {
                previewTimer.Stop();
                previewTimer.Start();
            }
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            previewTimer.Stop();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (_design == null)
            {
                return;
            }
            GumpListSpec spec = BuildSpec();
            GumpDesign d = _design.Clone();
            if (_edit != null)
            {
                GumpListSpec old = d.Lists.FirstOrDefault(l => l.Id == _edit.Id);
                if (old != null)
                {
                    GumpLists.Remove(d, old);
                }
            }
            GumpLists.Create(d, spec);
            picturePreview.Image?.Dispose();
            picturePreview.Image = null;
            try
            {
                picturePreview.Image = GumpRenderer.RenderImage(d, Math.Max(1, spec.FirstPage));
            }
            catch (Exception)
            {
                // no client files
            }
            lblHint.Text = $"{spec.Rows} rows · {spec.Columns.Count} columns · {spec.PageCount} page(s)" + (spec.SelectButton ? $" · button IDs {spec.ButtonBase} … {spec.ButtonBase + spec.Rows - 1}" : string.Empty);
        }

        private void BtnSample_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            string[] names = { "Britain", "Trinsic", "Minoc", "Yew", "Moonglow", "Vesper", "Skara Brae", "Jhelom", "Magincia", "Ocllo", "Cove", "Buccaneer's Den" };
            for (int i = 0; i < names.Length; i++)
            {
                sb.Append(names[i]).Append(';').Append((i + 1) * 5).Append("\r\n");
            }
            txtRows.Text = sb.ToString();
        }

        private void BtnPaste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                txtRows.Text = Clipboard.GetText();
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Result = BuildSpec();
            if (Result.Columns.Count == 0)
            {
                MessageBox.Show(this, "The list needs at least one column. / Die Liste braucht mindestens eine Spalte.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
