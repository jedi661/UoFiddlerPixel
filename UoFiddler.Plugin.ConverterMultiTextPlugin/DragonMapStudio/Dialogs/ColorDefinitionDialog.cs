// /***************************************************************************
//  *
//  * Dragon Map Studio – define / edit a colour (address, name, category, RGB, group, height, tiles)
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
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    /// <summary>
    /// One colour of the rule set: what is painted with it (land tiles = textures), its group (transitions, items),
    /// its height and its RGB value. Checks that address and RGB value are not used twice.
    /// </summary>
    public partial class ColorDefinitionDialog : Form
    {
        private readonly DragonRuleSet _rules;
        private readonly int _editIndex;
        private readonly List<int> _landIds;
        private Color _color = Color.Magenta;

        public ColorDefinitionDialog()
            : this(new DragonRuleSet(), -1, null, null)
        {
        }

        /// <param name="index">Colour to edit, -1 = new colour (next free address).</param>
        /// <param name="template">Values for a new colour (e.g. the selected colour).</param>
        public ColorDefinitionDialog(DragonRuleSet rules, int index, DragonPaletteEntry template, List<int> landIds)
        {
            InitializeComponent();
            UiTranslator.Apply(this);
            _rules = rules;
            _editIndex = index;
            _landIds = landIds;
            FormClosed += (s, e) => _tip.Dispose();
            Text = index >= 0 ? L.T("Farbe bearbeiten") : L.T("Neue Farbe definieren");
            cmbCategory.Items.AddRange(rules.Palette.Select(p => p.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c).Cast<object>().ToArray());
            FillGroups();
            DragonPaletteEntry source = index >= 0 ? rules.Palette[index] : template;
            int address = index >= 0 ? index : Math.Max(0, rules.FirstFreeIndex());
            txtAddress.Text = rules.HexIndex(address);
            txtName.Text = index >= 0 ? source.Name : L.T("Neue Farbe");
            cmbCategory.Text = source?.Category is { Length: > 0 } c ? c : "Eigene";
            numAltitude.Value = Math.Clamp(source?.Altitude ?? 0, -128, 127);
            txtTiles.Text = source?.Tiles ?? "0003";
            txtComment.Text = source?.Comment ?? string.Empty;
            SelectGroup(source?.Group ?? 0);
            _color = index >= 0 ? source.Color : RuleSetTools.UniqueColor(rules, source?.Color ?? Color.Magenta, -1);
            lblHelp.Text = L.T("Eine Farbe ist eine Adresse im Kartenbild. Beim Bauen wird daraus: eines der Land-Tiles (zufällig, an Hängen deren Textur) auf der Höhe Z, " +
                               "die Übergänge und Items ihrer Gruppe. Jede Farbe braucht einen eigenen RGB-Wert, sonst kann das Bild sie nicht unterscheiden.");
            UpdateState();
        }

        /// <summary>Chosen address.</summary>
        public int Index { get; private set; } = -1;

        /// <summary>A group was added in the dialog.</summary>
        public bool GroupsChanged { get; private set; }

        private void FillGroups()
        {
            int keep = cmbGroup.SelectedItem is GroupItem gi ? gi.Id : 0;
            cmbGroup.Items.Clear();
            foreach (int id in _rules.AllGroupIds())
            {
                cmbGroup.Items.Add(new GroupItem(id, $"{_rules.HexGroup(id)}  {_rules.GroupName(id)}"));
            }
            SelectGroup(keep);
        }

        private void SelectGroup(int id)
        {
            for (int i = 0; i < cmbGroup.Items.Count; i++)
            {
                if (((GroupItem)cmbGroup.Items[i]).Id == id)
                {
                    cmbGroup.SelectedIndex = i;
                    return;
                }
            }
            if (cmbGroup.Items.Count > 0 && cmbGroup.SelectedIndex < 0)
            {
                cmbGroup.SelectedIndex = 0;
            }
        }

        private sealed class GroupItem
        {
            public GroupItem(int id, string text)
            {
                Id = id;
                Text = text;
            }

            public int Id { get; }
            public string Text { get; }
            public override string ToString() => Text;
        }

        private bool TryAddress(out int address)
        {
            string t = txtAddress.Text.Trim();
            if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                t = t.Substring(2);
            }
            return int.TryParse(t, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address) && address >= 0 && address < _rules.MaxColors;
        }

        private void UpdateState()
        {
            panelColor.BackColor = _color;
            bool okAddress = TryAddress(out int address);
            string addressInfo;
            if (!okAddress)
            {
                addressInfo = L.Pick($"ungültig (0 … {_rules.MaxColors - 1:x})", $"invalid (0 … {_rules.MaxColors - 1:x})", $"invalide (0 … {_rules.MaxColors - 1:x})");
            }
            else if (address != _editIndex && address < _rules.Palette.Count && _rules.Palette[address].Defined)
            {
                addressInfo = L.Pick($"belegt: {_rules.Palette[address].Name}", $"in use: {_rules.Palette[address].Name}", $"occupée : {_rules.Palette[address].Name}");
                okAddress = false;
            }
            else
            {
                addressInfo = L.Pick("frei", "free", "libre");
            }
            lblAddressInfo.Text = addressInfo;
            lblAddressInfo.ForeColor = okAddress ? Color.DarkGreen : Color.Firebrick;

            List<DragonPaletteEntry> same = RuleSetTools.SameColor(_rules, (_color.R << 16) | (_color.G << 8) | _color.B, okAddress ? address : _editIndex);
            lblColorInfo.Text = same.Count == 0
                ? L.Pick("RGB-Wert ist eindeutig.", "RGB value is unique.", "Valeur RVB unique.")
                : L.Pick($"Gleiche Farbe wie {string.Join(", ", same.Take(3).Select(s => _rules.HexIndex(s.Index) + " " + s.Name))} – 'Eindeutig machen'.",
                    $"Same colour as {string.Join(", ", same.Take(3).Select(s => _rules.HexIndex(s.Index) + " " + s.Name))} – 'Make unique'.",
                    $"Même couleur que {string.Join(", ", same.Take(3).Select(s => _rules.HexIndex(s.Index) + " " + s.Name))} – 'Rendre unique'.");
            lblColorInfo.ForeColor = same.Count == 0 ? Color.DarkGreen : Color.Firebrick;

            ushort[] tiles = HexList.Parse(txtTiles.Text);
            flowTiles.SuspendLayout();
            foreach (Control c in flowTiles.Controls.Cast<Control>().ToList())
            {
                c.Dispose();
            }
            foreach (ushort id in tiles.Take(24))
            {
                var pic = new PictureBox { Size = new Size(44, 44), SizeMode = PictureBoxSizeMode.Zoom, Image = UoArtCache.Land(id) ?? (Image)UoArtCache.Texture(id), Margin = new Padding(2) };
                ToolTipFor(pic, $"{id:x4} {UoArtCache.LandName(id)}");
                flowTiles.Controls.Add(pic);
            }
            flowTiles.ResumeLayout();
            picPreview.Image = tiles.Length > 0 ? UoArtCache.Land(tiles[0]) ?? (Image)UoArtCache.Texture(tiles[0]) : null;
            btnOk.Enabled = okAddress && txtName.Text.Trim().Length > 0 && tiles.Length > 0 && cmbGroup.SelectedItem != null;
        }

        private readonly ToolTip _tip = new ToolTip();

        private void ToolTipFor(Control c, string text) => _tip.SetToolTip(c, text);

        private void Values_Changed(object sender, EventArgs e) => UpdateState();

        private void BtnColor_Click(object sender, EventArgs e)
        {
            using var dlg = new ColorDialog { FullOpen = true, Color = _color };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _color = dlg.Color;
                UpdateState();
            }
        }

        private void BtnUnique_Click(object sender, EventArgs e)
        {
            TryAddress(out int address);
            _color = RuleSetTools.UniqueColor(_rules, _color, address);
            UpdateState();
        }

        private void BtnNewGroup_Click(object sender, EventArgs e)
        {
            using var dlg = new GroupDialog(_rules);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                dlg.ApplyTo(_rules);
                GroupsChanged = true;
                FillGroups();
                SelectGroup(dlg.GroupId);
                UpdateState();
            }
        }

        private void BtnCatalog_Click(object sender, EventArgs e)
        {
            List<int> land = _landIds ?? Enumerable.Range(0, UoArtCache.MaxLandId + 1).ToList();
            ushort[] current = HexList.Parse(txtTiles.Text);
            using var dlg = new TilePickerForm(land, new List<int>(), TileCatalogMode.Land, current.Length > 0 ? current[0] : -1);
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedId >= 0)
            {
                // the chosen tile is added to the list (several tiles = mixed at random)
                string id = dlg.SelectedId.ToString("x4");
                txtTiles.Text = current.Length == 0 ? id : txtTiles.Text.Trim() + " " + id;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (!TryAddress(out int address))
            {
                return;
            }
            if (RuleSetTools.SameColor(_rules, (_color.R << 16) | (_color.G << 8) | _color.B, address).Count > 0)
            {
                DialogResult r = MessageBox.Show(this, L.Pick("Diese RGB-Farbe benutzt schon eine andere Adresse. Automatisch eindeutig machen?",
                        "Another address already uses this RGB colour. Make it unique automatically?",
                        "Une autre adresse utilise déjà cette couleur RVB. La rendre unique automatiquement ?"),
                    Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel)
                {
                    return;
                }
                if (r == DialogResult.Yes)
                {
                    _color = RuleSetTools.UniqueColor(_rules, _color, address);
                }
            }
            Index = address;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>Writes the colour into the rule set (16 bit: the palette grows when needed).</summary>
        public void ApplyTo(DragonRuleSet rules)
        {
            if (Index < 0)
            {
                return;
            }
            if (Index >= rules.Palette.Count)
            {
                rules.EnsurePaletteSize(Index + 1);
            }
            if (_editIndex >= 0 && _editIndex != Index)
            {
                // moved to another address: the old one becomes free
                DragonPaletteEntry old = rules.Palette[_editIndex];
                old.Defined = false;
                old.Group = -1;
                old.Name = "frei";
            }
            DragonPaletteEntry e = rules.Palette[Index];
            e.Defined = true;
            e.SetColor(_color);
            e.Name = txtName.Text.Trim();
            e.Category = cmbCategory.Text.Trim();
            e.Group = ((GroupItem)cmbGroup.SelectedItem).Id;
            e.Altitude = (int)numAltitude.Value;
            e.Tiles = string.Join(" ", HexList.Parse(txtTiles.Text).Select(t => t.ToString("x4")));
            e.Comment = txtComment.Text.Trim();
        }
    }
}
