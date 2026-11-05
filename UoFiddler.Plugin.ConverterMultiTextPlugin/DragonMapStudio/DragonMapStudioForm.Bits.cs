// /***************************************************************************
//  *
//  * Dragon Map Studio – 8 bit (classic Dragon, 256 colours) or 16 bit (up to 65 536 colours)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private bool _syncingPage;

        /// <summary>File type of a map: 8 bit = Dragon BMP, 16 bit = PNG.</summary>
        private static string MapExtension(IndexedMap map) => map != null && map.Bits == 16 ? ".png" : ".bmp";

        /// <summary>
        /// A colour above ff can only be painted into a 16-bit map: asks to convert the map (nothing is lost).
        /// </summary>
        private bool EnsureMapCanUse(int index)
        {
            if (_map == null || index <= _map.MaxIndex)
            {
                return true;
            }
            DialogResult r = MessageBox.Show(this, L.Pick(
                    $"Farbe {_rules.HexIndex(index)} liegt über ff – die Karte ist 8 Bit (klassisches Dragon).\n\nKarte in 16 Bit umwandeln? Sie wird dann als PNG gespeichert.",
                    $"Colour {_rules.HexIndex(index)} is above ff – the map is 8 bit (classic Dragon).\n\nConvert the map to 16 bit? It is then saved as PNG.",
                    $"La couleur {_rules.HexIndex(index)} est au-delà de ff – la carte est en 8 bits (Dragon classique).\n\nConvertir la carte en 16 bits ? Elle sera enregistrée en PNG."),
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes)
            {
                return false;
            }
            ConvertMapTo16();
            return true;
        }

        private void ConvertMapTo16()
        {
            if (_map == null || _map.Bits == 16)
            {
                return;
            }
            _map.Bits = 16;
            _map.Modified = true;
            if (!string.IsNullOrEmpty(_map.FilePath) && _map.FilePath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
            {
                _map.FilePath = System.IO.Path.ChangeExtension(_map.FilePath, ".png");
            }
            if (_rules.ColorBits == 8)
            {
                ConvertRulesTo16();
            }
            UpdateTitle();
            UpdatePaletteBitsUi();
            SetStatus(L.Pick("Karte ist jetzt 16 Bit – Speichern schreibt ein 16-Bit-PNG (das 8-Bit-BMP bleibt unverändert).",
                "The map is now 16 bit – saving writes a 16-bit PNG (the 8-bit BMP stays unchanged).",
                "La carte est maintenant en 16 bits – l'enregistrement écrit un PNG 16 bits (le BMP 8 bits reste inchangé)."));
        }

        private void ConvertRulesTo16()
        {
            if (_rules.ColorBits == 16)
            {
                return;
            }
            _rules.ConvertTo16Bit();
            _rules.EnsurePaletteSize(512);
            MarkRulesDirty();
            ApplyRules();
            Log(L.Pick("Regelsatz auf 16 Bit umgestellt (4-stellige Adressen, bis 65 536 Farben).",
                "Rule set switched to 16 bit (4-digit addresses, up to 65 536 colours).",
                "Jeu de règles passé en 16 bits (adresses à 4 chiffres, jusqu'à 65 536 couleurs)."));
        }

        private void MnuConvertMap16_Click(object sender, EventArgs e)
        {
            if (_map == null)
            {
                SetStatus(L.Pick("Keine Karte geöffnet.", "No map open.", "Aucune carte ouverte."));
                return;
            }
            ConvertMapTo16();
        }

        private void MnuRules16_Click(object sender, EventArgs e)
        {
            if (_rules.ColorBits == 16)
            {
                _rules.EnsurePaletteSize(_rules.Palette.Count + 256);
                ApplyRules();
                MarkRulesDirty();
                SetStatus(L.Pick($"Regelsatz ist 16 Bit – eine weitere Seite angelegt ({_rules.Palette.Count} Farben).",
                    $"Rule set is 16 bit – one more page added ({_rules.Palette.Count} colours).",
                    $"Le jeu de règles est en 16 bits – une page de plus ({_rules.Palette.Count} couleurs)."));
                return;
            }
            DialogResult r = MessageBox.Show(this, L.Pick(
                    "Regelsatz auf 16 Bit umstellen?\n\n• bis 65 536 Farben (4-stellige Adressen in maptrans / groups)\n• Gruppen bis 7fff\n" +
                    "• 8-Bit-Karten (BMP) funktionieren weiter\n• Der Export als Dragon-Skripte schreibt 4-stellige Adressen – das alte Dragon kann sie nicht lesen.",
                    "Switch the rule set to 16 bit?\n\n• up to 65 536 colours (4-digit addresses in maptrans / groups)\n• groups up to 7fff\n" +
                    "• 8-bit maps (BMP) keep working\n• Exported Dragon scripts get 4-digit addresses – the old Dragon cannot read them.",
                    "Passer le jeu de règles en 16 bits ?\n\n• jusqu'à 65 536 couleurs (adresses à 4 chiffres dans maptrans / groups)\n• groupes jusqu'à 7fff\n" +
                    "• les cartes 8 bits (BMP) fonctionnent toujours\n• Les scripts Dragon exportés ont des adresses à 4 chiffres – l'ancien Dragon ne peut pas les lire."),
                Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                ConvertRulesTo16();
                SetStatus(L.Pick("Regelsatz ist jetzt 16 Bit. Neue Farben: 'Neue Farbe definieren' (Reiter Farbtabelle), weitere Seiten über das Menü.",
                    "The rule set is now 16 bit. New colours: 'Define new colour' (tab Colour table), more pages via the menu.",
                    "Le jeu de règles est maintenant en 16 bits. Nouvelles couleurs : 'Définir une nouvelle couleur', plus de pages via le menu."));
            }
        }

        /// <summary>Page controls and info after the palette size changed.</summary>
        private void UpdatePaletteBitsUi()
        {
            int pages = Math.Max(1, (_rules.Palette.Count + 255) / 256);
            _syncingPage = true;
            foreach (NumericUpDown n in new[] { numEditorPage, numColorPage })
            {
                n.Maximum = pages - 1;
                n.Enabled = pages > 1;
            }
            numEditorPage.Value = Math.Min(editorPalette.Page, pages - 1);
            numColorPage.Value = Math.Min(paletteGrid.Page, pages - 1);
            _syncingPage = false;
            int first = editorPalette.Page * 256;
            lblEditorPageInfo.Text = _rules.ColorBits == 16
                ? $"16 Bit · {_rules.HexIndex(first)}–{_rules.HexIndex(Math.Min(first + 255, _rules.Palette.Count - 1))}"
                : L.Pick("8 Bit · 256 Farben", "8 bit · 256 colours", "8 bits · 256 couleurs");
        }

        private void NumPalettePage_ValueChanged(object sender, EventArgs e)
        {
            if (_syncingPage)
            {
                return;
            }
            if (sender == numEditorPage)
            {
                editorPalette.Page = (int)numEditorPage.Value;
            }
            else
            {
                paletteGrid.Page = (int)numColorPage.Value;
            }
            UpdatePaletteBitsUi();
        }

        private void PaletteGrid_PageChanged(object sender, EventArgs e) => UpdatePaletteBitsUi();

        /// <summary>Shows the page of a colour in both palette views.</summary>
        private void ShowPaletteIndex(int index)
        {
            int page = index / 256;
            if (paletteGrid.Page != page)
            {
                paletteGrid.Page = page;
            }
            if (editorPalette.Page != page)
            {
                editorPalette.Page = page;
            }
        }
    }
}
