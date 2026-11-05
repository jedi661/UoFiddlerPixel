// /***************************************************************************
//  *
//  * Dragon Map Studio – new group (groups.txt)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    /// <summary>Number and name of a new group; the next free number is suggested.</summary>
    public partial class GroupDialog : Form
    {
        private readonly DragonRuleSet _rules;

        public GroupDialog()
            : this(new DragonRuleSet())
        {
        }

        public GroupDialog(DragonRuleSet rules)
        {
            InitializeComponent();
            UiTranslator.Apply(this);
            _rules = rules;
            foreach (int id in rules.AllGroupIds())
            {
                lstGroups.Items.Add($"{rules.HexGroup(id)}  {rules.GroupName(id)}");
            }
            int free = RuleSetTools.FreeGroupId(rules);
            txtId.Text = free < 0 ? string.Empty : rules.HexGroup(free);
            txtName.Text = L.T("Neue Gruppe");
            lblHelp.Text = L.T("Eine Gruppe fasst Farben zusammen (z. B. alle Höhen von Gras). Übergänge und Items gelten für Gruppen. " +
                               "Eine neue Gruppe braucht Übergangsregeln zu ihren Nachbarn (Knopf 'Neue Übergangsregel').");
            UpdateState();
        }

        public int GroupId { get; private set; } = -1;

        private bool TryId(out int id)
        {
            return int.TryParse(txtId.Text.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id) && id >= 0 && id <= _rules.MaxGroup;
        }

        private void UpdateState()
        {
            bool ok = TryId(out int id);
            bool used = ok && _rules.AllGroupIds().Contains(id);
            lblIdInfo.Text = !ok ? L.Pick($"ungültig (0 … {_rules.MaxGroup:x})", $"invalid (0 … {_rules.MaxGroup:x})", $"invalide (0 … {_rules.MaxGroup:x})")
                : used ? L.Pick("schon vorhanden", "already exists", "existe déjà") : L.Pick("frei", "free", "libre");
            lblIdInfo.ForeColor = ok && !used ? Color.DarkGreen : Color.Firebrick;
            btnOk.Enabled = ok && !used && txtName.Text.Trim().Length > 0;
        }

        private void Values_Changed(object sender, EventArgs e) => UpdateState();

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (!TryId(out int id))
            {
                return;
            }
            GroupId = id;
            DialogResult = DialogResult.OK;
            Close();
        }

        public void ApplyTo(DragonRuleSet rules)
        {
            if (GroupId >= 0 && rules.FindGroup(GroupId) == null)
            {
                rules.Groups.Add(new DragonGroup { Id = GroupId, Name = txtName.Text.Trim(), Comment = txtComment.Text.Trim() });
                rules.Groups.Sort((a, b) => a.Id.CompareTo(b.Id));
            }
        }
    }
}
