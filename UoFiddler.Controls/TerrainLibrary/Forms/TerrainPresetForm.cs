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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Helpers;
using UoFiddler.Controls.ArtLibrary.Metadata;
using UoFiddler.Controls.TerrainLibrary.Helpers;
using UoFiddler.Controls.TerrainLibrary.Metadata;

namespace UoFiddler.Controls.TerrainLibrary.Forms
{
    /// <summary>
    /// Dialog "Vorlagen verwalten": own templates (tiledata name, flags, texture id, radar color, category,
    /// description) can be created, changed and deleted. Built-in templates are shown read-only and can be
    /// saved as an own copy.
    /// </summary>
    public partial class TerrainPresetForm : Form
    {
        private readonly TerrainMetadata _metadata;
        private readonly int _slotId;
        private ushort? _color;
        private bool _filling;

        /// <summary>Designer only.</summary>
        public TerrainPresetForm() : this(TerrainMetadata.Load(string.Empty), -1, null)
        {
        }

        /// <param name="metadata">Metadata of the open library.</param>
        /// <param name="slotId">Selected slot (for "Aus Slot übernehmen"), -1 = none.</param>
        /// <param name="newNameFromSlot">When set, the fields start with the slot's settings and this name.</param>
        public TerrainPresetForm(TerrainMetadata metadata, int slotId, string newNameFromSlot)
        {
            InitializeComponent();
            _metadata = metadata;
            _slotId = slotId;

            TerrainFlags.Fill(checkedListBoxFlags);
            comboBoxCategory.Items.Add(new CategoryItem(null, "(nicht ändern)"));
            foreach (ArtLibraryCategory category in _metadata.Catalog.GetCategoriesSorted())
            {
                comboBoxCategory.Items.Add(new CategoryItem(category.Id, _metadata.Catalog.GetCategoryPath(category.Id)));
            }

            comboBoxCategory.SelectedIndex = 0;
            buttonFromSlot.Enabled = slotId >= 0;
            FillList(null);

            if (newNameFromSlot != null)
            {
                ClearFields();
                FillFromSlot();
                textBoxName.Text = newNameFromSlot;
            }
        }

        public bool Changed { get; private set; }

        private sealed class CategoryItem
        {
            public CategoryItem(int? id, string text)
            {
                Id = id;
                Text = text;
            }

            public int? Id { get; }
            public string Text { get; }

            public override string ToString() => Text;
        }

        private void FillList(string select)
        {
            listBoxPresets.BeginUpdate();
            listBoxPresets.Items.Clear();
            foreach (TerrainPreset preset in _metadata.GetAllPresets(true))
            {
                listBoxPresets.Items.Add(preset);
                if (select != null && !preset.BuiltIn && string.Equals(preset.Name, select, StringComparison.CurrentCultureIgnoreCase))
                {
                    listBoxPresets.SelectedItem = preset;
                }
            }

            listBoxPresets.EndUpdate();
        }

        private void ClearFields()
        {
            _filling = true;
            textBoxName.Text = textBoxTileName.Text = textBoxTextureId.Text = textBoxDescription.Text = string.Empty;
            TerrainFlags.Set(checkedListBoxFlags, 0);
            comboBoxCategory.SelectedIndex = 0;
            SetColor(null);
            _filling = false;
        }

        private void ShowPreset(TerrainPreset preset)
        {
            _filling = true;
            textBoxName.Text = preset.Name;
            textBoxTileName.Text = preset.TileName ?? string.Empty;
            textBoxTextureId.Text = preset.TextureId == null ? string.Empty : $"0x{preset.TextureId.Value:X4}";
            TerrainFlags.Set(checkedListBoxFlags, preset.TileFlags ?? 0);
            textBoxDescription.Text = preset.Description ?? string.Empty;
            SetColor(preset.RadarColor);
            comboBoxCategory.SelectedIndex = 0;
            for (int i = 0; i < comboBoxCategory.Items.Count; ++i)
            {
                if (((CategoryItem)comboBoxCategory.Items[i]).Id == preset.CategoryId)
                {
                    comboBoxCategory.SelectedIndex = i;
                }
            }

            buttonDelete.Enabled = !preset.BuiltIn;
            labelBuiltIn.Visible = preset.BuiltIn;
            _filling = false;
        }

        private void SetColor(ushort? color)
        {
            _color = color;
            panelColor.BackColor = color == null ? SystemColors.Control : TerrainColor.ToColor(color.Value);
            textBoxColor.Text = color == null ? string.Empty : TerrainColor.ToHex(color.Value);
        }

        private void FillFromSlot()
        {
            TerrainSlotData slot = _metadata.GetSlot(_slotId);
            if (slot == null)
            {
                return;
            }

            _filling = true;
            textBoxTileName.Text = slot.TileName ?? string.Empty;
            textBoxTextureId.Text = slot.TextureId == null ? string.Empty : $"0x{slot.TextureId.Value:X4}";
            TerrainFlags.Set(checkedListBoxFlags, slot.TileFlags ?? 0);
            SetColor(slot.RadarColor);
            textBoxDescription.Text = _metadata.Catalog.Get(_slotId)?.Description ?? string.Empty;
            _filling = false;
        }

        private void ListBoxPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxPresets.SelectedItem is TerrainPreset preset)
            {
                ShowPreset(preset);
            }
        }

        private void ButtonNew_Click(object sender, EventArgs e)
        {
            listBoxPresets.ClearSelected();
            ClearFields();
            buttonDelete.Enabled = false;
            labelBuiltIn.Visible = false;
            textBoxName.Focus();
        }

        private void ButtonFromSlot_Click(object sender, EventArgs e)
        {
            FillFromSlot();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            string name = textBoxName.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show(this, "Bitte einen Namen für die Vorlage eingeben.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxName.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(textBoxColor.Text) && TerrainColor.TryParse(textBoxColor.Text, out ushort typed))
            {
                _color = typed;
            }
            else if (string.IsNullOrWhiteSpace(textBoxColor.Text))
            {
                _color = null;
            }

            TerrainPreset existing = _metadata.Presets.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (existing != null && !(listBoxPresets.SelectedItem is TerrainPreset selected && ReferenceEquals(selected, existing))
                && MessageBox.Show(this, $"Es gibt bereits eine eigene Vorlage \"{name}\". Ersetzen?", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            ulong flags = TerrainFlags.Get(checkedListBoxFlags);
            var preset = new TerrainPreset
            {
                Name = name,
                TileName = string.IsNullOrWhiteSpace(textBoxTileName.Text) ? null : textBoxTileName.Text.Trim(),
                TileFlags = flags,
                TextureId = ArtIdParser.TryParse(textBoxTextureId.Text, out int texId) && texId <= ushort.MaxValue ? (ushort)texId : (ushort?)null,
                RadarColor = _color,
                CategoryId = (comboBoxCategory.SelectedItem as CategoryItem)?.Id,
                Description = string.IsNullOrWhiteSpace(textBoxDescription.Text) ? null : textBoxDescription.Text.Trim()
            };

            _metadata.Presets.RemoveAll(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase));
            _metadata.Presets.Add(preset);
            _metadata.MarkDirty();
            Changed = true;
            FillList(name);
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (!(listBoxPresets.SelectedItem is TerrainPreset preset) || preset.BuiltIn)
            {
                return;
            }

            if (MessageBox.Show(this, $"Vorlage \"{preset.Name}\" löschen? (Slots, auf die sie angewendet wurde, behalten ihre Werte.)",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _metadata.Presets.Remove(preset);
            _metadata.MarkDirty();
            Changed = true;
            ClearFields();
            FillList(null);
        }

        private void ButtonPickColor_Click(object sender, EventArgs e)
        {
            colorDialog.Color = _color == null ? Color.Gray : TerrainColor.ToColor(_color.Value);
            if (colorDialog.ShowDialog(this) == DialogResult.OK)
            {
                SetColor(TerrainColor.FromColor(colorDialog.Color));
            }
        }

        private void TextBoxColor_Leave(object sender, EventArgs e)
        {
            if (_filling)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxColor.Text))
            {
                SetColor(null);
            }
            else if (TerrainColor.TryParse(textBoxColor.Text, out ushort value))
            {
                SetColor(value);
            }
        }
    }
}
