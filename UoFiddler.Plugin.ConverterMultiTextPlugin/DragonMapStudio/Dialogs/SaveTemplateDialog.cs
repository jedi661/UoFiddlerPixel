// /***************************************************************************
//  *
//  * Dragon Map Studio – save a marked area / selection as template (building, floor pattern)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    /// <summary>Name, category (= sub folder), format and content of a new template.</summary>
    public partial class SaveTemplateDialog : Form
    {
        private readonly bool _landAvailable;

        public SaveTemplateDialog()
            : this(Array.Empty<string>(), true, 0, null)
        {
        }

        /// <param name="categories">Existing categories (sub folders of the template folder).</param>
        /// <param name="landAvailable">An area is marked (land can be saved).</param>
        /// <param name="staticCount">Statics that would be saved.</param>
        /// <param name="preview">Picture of the template.</param>
        public SaveTemplateDialog(IEnumerable<string> categories, bool landAvailable, int staticCount, Image preview)
        {
            InitializeComponent();
            UiTranslator.Apply(this);
            _landAvailable = landAvailable;
            cmbCategory.Items.AddRange(categories.Cast<object>().ToArray());
            cmbCategory.Text = "Eigene";
            cmbFormat.Items.AddRange(new object[] { L.T("Dragon-Vorlage (.dmsv.json) – Land + Statics"), L.T("UoFiddler-Multi (.txt) – nur Statics"), L.T("UO Architect (.uoa) – nur Statics") });
            cmbFormat.SelectedIndex = 0;
            chkLand.Checked = landAvailable;
            chkLand.Enabled = landAvailable;
            chkStatics.Checked = staticCount > 0;
            chkStatics.Text = L.T("Statics") + $" ({staticCount:N0})";
            picPreview.Image = preview;
            txtName.Text = L.T("Neue Vorlage");
            UpdateState();
        }

        public string TemplateName => txtName.Text.Trim();

        public string Category => cmbCategory.Text.Trim().Replace('\\', '/').Trim('/');

        public TemplateFormat Format => cmbFormat.SelectedIndex switch
        {
            1 => TemplateFormat.FiddlerText,
            2 => TemplateFormat.UoArchitect,
            _ => TemplateFormat.Dragon
        };

        public bool IncludeLand => chkLand.Checked && Format == TemplateFormat.Dragon;

        public bool IncludeStatics => chkStatics.Checked;

        private void UpdateState()
        {
            bool dragon = Format == TemplateFormat.Dragon;
            chkLand.Enabled = _landAvailable && dragon;
            lblInfo.Text = dragon
                ? L.T("Eigenes Format: Land-Tiles mit Höhen (relativ zur Mitte) und Statics. Beim Setzen wählbar: mit oder ohne Höhe.")
                : L.T("Nur Statics – lesbar von UoFiddler (Multi-Import .txt) bzw. UO Architect (.uoa).");
            btnOk.Enabled = TemplateName.Length > 0 && (IncludeLand || IncludeStatics);
        }

        private void Content_Changed(object sender, EventArgs e) => UpdateState();
    }
}
