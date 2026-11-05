// /***************************************************************************
//  *
//  * Dragon Map Studio – new transition rule with an example file (12 standard shapes)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    /// <summary>
    /// Transition between two groups: where it applies (map tile or statics), which file holds the patterns.
    /// A new file can be created with the 12 standard shapes as example (tiles are placeholders).
    /// </summary>
    public partial class TransitionRuleDialog : Form
    {
        private readonly DragonRuleSet _rules;
        private bool _fileEdited;

        public TransitionRuleDialog()
            : this(new DragonRuleSet(), 0)
        {
        }

        public TransitionRuleDialog(DragonRuleSet rules, int groupA)
        {
            InitializeComponent();
            UiTranslator.Apply(this);
            _rules = rules;
            cmbLayer.Items.AddRange(new object[] { L.T("Map – Boden-Tile wird ersetzt"), L.T("Static – Statics werden gesetzt") });
            cmbLayer.SelectedIndex = 0;
            cmbGroupB.Items.Add(L.T("* alle anderen Gruppen"));
            foreach (int id in rules.AllGroupIds())
            {
                string text = $"{rules.HexGroup(id)}  {rules.GroupName(id)}";
                cmbGroupA.Items.Add(new Item(id, text));
                cmbGroupB.Items.Add(new Item(id, text));
            }
            Select(cmbGroupA, groupA);
            // neighbour: the first other group (not the same as A)
            int other = rules.AllGroupIds().FirstOrDefault(g => g != groupA, -1);
            if (other >= 0)
            {
                Select(cmbGroupB, other);
            }
            else
            {
                cmbGroupB.SelectedIndex = 0;
            }
            picPatterns.Image = DrawPatterns();
            lblHelp.Text = L.T("So funktioniert ein Übergang: Für jede Zelle der Gruppe A werden die 8 Nachbarn angesehen (NW N NO O SO S SW W). " +
                               "A = Nachbar gehört zur eigenen Gruppe, B = Nachbar ist Gruppe B. Passt ein Muster der Datei, wird das Tile (oder Static) der Zeile gesetzt.") +
                           Environment.NewLine + Environment.NewLine +
                           L.T("Beispiel-Zeile:  BBBAAAAA 0035 0036 000 000  = Norden ist B (Kante oben), Tile 0035 oder 0036, Höhe +0.") + Environment.NewLine +
                           L.T("Die Beispiel-Datei enthält die 12 Standard-Formen (rechts) mit einem Platzhalter-Tile – im Reiter Übergänge unten die Tiles eintragen. " +
                               "Für weiche Übergänge braucht meist auch B → A eine Regel.");
            UpdateFile();
        }

        public TransitionRule Rule { get; private set; }

        /// <summary>New example file (null when an existing file is used).</summary>
        public TransitionFile NewFile { get; private set; }

        private sealed class Item
        {
            public Item(int id, string text)
            {
                Id = id;
                Text = text;
            }

            public int Id { get; }
            public string Text { get; }
            public override string ToString() => Text;
        }

        private static void Select(ComboBox c, int id)
        {
            for (int i = 0; i < c.Items.Count; i++)
            {
                if (c.Items[i] is Item it && it.Id == id)
                {
                    c.SelectedIndex = i;
                    return;
                }
            }
            if (c.Items.Count > 0)
            {
                c.SelectedIndex = 0;
            }
        }

        private int GroupA => (cmbGroupA.SelectedItem as Item)?.Id ?? -1;

        private int GroupB => (cmbGroupB.SelectedItem as Item)?.Id ?? -1;

        private TransitionLayer Layer => cmbLayer.SelectedIndex == 1 ? TransitionLayer.Static : TransitionLayer.Map;

        private void Values_Changed(object sender, EventArgs e)
        {
            if (!_fileEdited)
            {
                UpdateFile();
            }
            UpdateInfo();
        }

        private void File_Changed(object sender, EventArgs e)
        {
            if (txtFile.Focused)
            {
                _fileEdited = true;
            }
            UpdateInfo();
        }

        private static string Short(string name)
        {
            string s = new string((name ?? string.Empty).ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
            return s.Length == 0 ? "gruppe" : s.Length > 12 ? s.Substring(0, 12) : s;
        }

        private void UpdateFile()
        {
            string folder = TransitionRule.GetFolder(Layer, GroupB < 0 ? 2 : 1);
            string b = GroupB < 0 ? "x" : Short(_rules.GroupName(GroupB));
            txtFile.Text = $"{folder}/{Short(_rules.GroupName(GroupA))}2{b}.txt";
            UpdateInfo();
        }

        private void UpdateInfo()
        {
            bool exists = _rules.FindTransitionFile(txtFile.Text) != null;
            chkExample.Enabled = !exists;
            lblFileInfo.Text = exists
                ? L.Pick("Datei ist vorhanden – die Regel benutzt ihre Muster.", "The file exists – the rule uses its patterns.", "Le fichier existe – la règle utilise ses motifs.")
                : L.Pick("Neue Datei – wird beim Speichern / Export im Skript-Ordner angelegt.", "New file – created in the script folder when saving / exporting.", "Nouveau fichier – créé lors de l'enregistrement / export.");
            bool duplicate = _rules.Transitions.Any(t => t.Layer == Layer && t.GroupA == GroupA && t.GroupB == GroupB && string.Equals(t.File, txtFile.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            btnOk.Enabled = GroupA >= 0 && GroupA != GroupB && txtFile.Text.Trim().EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && !duplicate;
        }

        /// <summary>The 12 standard shapes as 3×3 pictures (A = own group light, B = neighbour dark, centre = cell).</summary>
        private static Bitmap DrawPatterns()
        {
            const int cell = 18, pad = 34, cols = 3;
            int w = cols * (3 * cell + pad) + pad, h = 4 * (3 * cell + pad + 16) + pad;
            var bmp = new Bitmap(w, h);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(40, 40, 40));
            using var font = new Font("Segoe UI", 7f);
            // order of the 8 characters: NW N NE E SE S SW W
            (int dx, int dy)[] pos = { (0, 0), (1, 0), (2, 0), (2, 1), (2, 2), (1, 2), (0, 2), (0, 1) };
            for (int i = 0; i < RuleSetTools.StandardPatterns.Length; i++)
            {
                (string pattern, string de, string en) = RuleSetTools.StandardPatterns[i];
                int ox = pad + i % cols * (3 * cell + pad), oy = pad + i / cols * (3 * cell + pad + 16);
                for (int k = 0; k < 8; k++)
                {
                    using var b = new SolidBrush(pattern[k] == 'B' ? Color.FromArgb(70, 110, 190) : Color.FromArgb(110, 170, 80));
                    g.FillRectangle(b, ox + pos[k].dx * cell, oy + pos[k].dy * cell, cell - 1, cell - 1);
                }
                g.FillRectangle(Brushes.Gold, ox + cell, oy + cell, cell - 1, cell - 1);
                TextRenderer.DrawText(g, L.Pick(de, en, en), font, new Rectangle(ox - pad / 2, oy + 3 * cell + 1, 3 * cell + pad, 14), Color.Gainsboro, TextFormatFlags.HorizontalCenter);
            }
            return bmp;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            string file = DragonRuleSet.NormalizePath(txtFile.Text.Trim());
            Rule = new TransitionRule
            {
                Layer = Layer,
                ScriptSet = GroupB < 0 ? 2 : 1,
                GroupA = GroupA,
                GroupB = GroupB,
                File = file,
                Comment = $"{_rules.GroupName(GroupA)} → {(GroupB < 0 ? "*" : _rules.GroupName(GroupB))} (Dragon Map Studio)"
            };
            if (_rules.FindTransitionFile(file) == null && chkExample.Checked)
            {
                NewFile = RuleSetTools.ExampleTransitionFile(_rules, file, GroupA, Layer);
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        public void ApplyTo(DragonRuleSet rules)
        {
            if (Rule == null)
            {
                return;
            }
            if (NewFile != null && rules.FindTransitionFile(NewFile.Path) == null)
            {
                rules.TransitionFiles.Add(NewFile);
            }
            rules.Transitions.Add(Rule);
        }
    }
}
