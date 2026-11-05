// /***************************************************************************
//  *
//  * Dragon Map Studio – rules workshop: 8/16 bit switch, new colour / group / transition, hints, script editor, help
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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private ColorPreviewPopup _colorPopup;
        private bool _updatingRulesToolbar;

        // ================================================================== hover preview of a colour

        private void ShowColorPreview(int index, int rowHeight)
        {
            if (_rules == null || index < 0 || index >= _rules.Palette.Count)
            {
                HideColorPreview();
                return;
            }
            if (_colorPopup == null || _colorPopup.IsDisposed)
            {
                _colorPopup = new ColorPreviewPopup();
                FormClosed += (s, e) => _colorPopup?.Dispose();
                // hide when the studio is minimised (not on Deactivate: showing the preview itself would hide it again)
                Resize += (s, e) =>
                {
                    if (WindowState == FormWindowState.Minimized)
                    {
                        HideColorPreview();
                    }
                };
                colorTableView.MouseLeave += (s, e) => HideColorPreview();
            }
            _colorPopup.ShowFor(_rules, index, Control.MousePosition, rowHeight);
        }

        private void HideColorPreview() => _colorPopup?.HidePreview();

        private void Palette_IndexHovered(object sender, PaletteIndexEventArgs e)
        {
            if (e.Index < 0)
            {
                HideColorPreview();
                return;
            }
            ShowColorPreview(e.Index, (sender as PaletteGridView)?.CellHeight ?? 20);
        }

        /// <summary>Rules grids: a colour row (or a group row = its first colour) shows the preview below the row.</summary>
        private void GridRules_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (sender is not DataGridView grid || e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count || grid.IsCurrentCellInEditMode)
            {
                HideColorPreview();
                return;
            }
            object item = grid.Rows[e.RowIndex].DataBoundItem;
            int index = item switch
            {
                DragonPaletteEntry p => p.Index,
                DragonGroup g => _rules.EntriesOfGroup(g.Id).Select(x => x.Index).DefaultIfEmpty(-1).First(),
                _ => -1
            };
            if (index < 0)
            {
                HideColorPreview();
                return;
            }
            ShowColorPreview(index, grid.Rows[e.RowIndex].Height);
        }

        private void Palette_IndexDoubleClicked(object sender, PaletteIndexEventArgs e) => EditColor(e.Index);

        private void GridPaletteEntries_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && gridPaletteEntries.Rows[e.RowIndex].DataBoundItem is DragonPaletteEntry p)
            {
                EditColor(p.Index);
            }
        }

        // ================================================================== colour / group / transition

        /// <summary>Opens the colour dialog: -1 = new colour (based on the selected one), otherwise edit.</summary>
        private void EditColor(int index)
        {
            HideColorPreview();
            if (index < 0 && _rules.FirstFreeIndex() < 0)
            {
                if (_rules.ColorBits == 8)
                {
                    BtnNewColor_Click(this, EventArgs.Empty); // asks for 16 bit
                    return;
                }
                MessageBox.Show(this, L.Pick("Alle Farben sind belegt.", "All colours are used.", "Toutes les couleurs sont utilisées."), Text);
                return;
            }
            DragonPaletteEntry template = _selectedColor >= 0 && _selectedColor < _rules.Palette.Count ? _rules.Palette[_selectedColor] : null;
            EnsureLandList();
            using var dlg = new ColorDefinitionDialog(_rules, index, template, _allLand);
            DialogResult r = dlg.ShowDialog(this);
            if (r != DialogResult.OK && !dlg.GroupsChanged)
            {
                return;
            }
            if (r == DialogResult.OK)
            {
                dlg.ApplyTo(_rules);
            }
            MarkRulesDirty();
            ApplyRules();
            if (r == DialogResult.OK)
            {
                SelectColor(dlg.Index);
                SetStatus(L.Pick($"Farbe {_rules.HexIndex(dlg.Index)} gespeichert – im Editor und in der Farbtabelle verfügbar. 'Speichern' im Reiter Regeln sichert den Regelsatz.",
                    $"Colour {_rules.HexIndex(dlg.Index)} saved – available in the editor and the colour table. 'Save' in the Rules tab stores the rule set.",
                    $"Couleur {_rules.HexIndex(dlg.Index)} enregistrée – disponible dans l'éditeur. 'Enregistrer' dans l'onglet Règles sauvegarde le jeu de règles."));
            }
        }

        private void TsbRulesNewColor_Click(object sender, EventArgs e) => EditColor(-1);

        private void TsbRulesNewGroup_Click(object sender, EventArgs e)
        {
            using var dlg = new GroupDialog(_rules);
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            dlg.ApplyTo(_rules);
            MarkRulesDirty();
            ApplyRules();
            tabMain.SelectedTab = tabRules;
            tabRulesInner.SelectedTab = tabRuleGroups;
            SetStatus(L.Pick($"Gruppe {_rules.HexGroup(dlg.GroupId)} angelegt. Jetzt Farben zuordnen ('+ Neue Farbe') und Übergänge anlegen ('+ Neue Übergangsregel').",
                $"Group {_rules.HexGroup(dlg.GroupId)} created. Now assign colours ('+ New colour') and add transitions ('+ New transition rule').",
                $"Groupe {_rules.HexGroup(dlg.GroupId)} créé. Attribuez des couleurs et créez des transitions."));
        }

        private void TsbRulesNewTransition_Click(object sender, EventArgs e)
        {
            int groupA = _selectedColor >= 0 && _selectedColor < _rules.Palette.Count ? Math.Max(0, _rules.Palette[_selectedColor].Group) : 0;
            using var dlg = new TransitionRuleDialog(_rules, groupA);
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            dlg.ApplyTo(_rules);
            MarkRulesDirty();
            ApplyRules();
            tabMain.SelectedTab = tabRules;
            tabRulesInner.SelectedTab = tabRuleTransitions;
            int row = _rules.Transitions.IndexOf(dlg.Rule);
            if (row >= 0 && row < gridTransitions.Rows.Count)
            {
                gridTransitions.CurrentCell = gridTransitions.Rows[row].Cells[0];
                gridTransitions.FirstDisplayedScrollingRowIndex = row;
            }
            SetStatus(dlg.NewFile != null
                ? L.Pick($"Regel angelegt, Beispiel-Datei {dlg.NewFile.Path} mit 12 Mustern – unten die Tiles eintragen.", $"Rule created, example file {dlg.NewFile.Path} with 12 patterns – enter the tiles below.", $"Règle créée, fichier exemple {dlg.NewFile.Path} avec 12 motifs.")
                : L.Pick("Regel angelegt.", "Rule created.", "Règle créée."));
        }

        private void TsbRulesFixHints_Click(object sender, EventArgs e)
        {
            List<string> done = RuleSetTools.FixHints(_rules);
            List<string> left = _rules.Validate();
            txtRuleWarnings.Text = (done.Count == 0 ? L.Pick("Nichts zu beheben.", "Nothing to fix.", "Rien à corriger.") : L.Pick("Behoben:", "Fixed:", "Corrigé :") + Environment.NewLine + string.Join(Environment.NewLine, done)) +
                                   Environment.NewLine + Environment.NewLine +
                                   (left.Count == 0 ? L.Pick("Prüfung: keine Probleme.", "Check: no problems.", "Vérification : aucun problème.") : L.Pick("Noch offen:", "Still open:", "Reste :") + Environment.NewLine + string.Join(Environment.NewLine, left));
            if (done.Count > 0)
            {
                MarkRulesDirty();
                ApplyRules();
            }
            tabMain.SelectedTab = tabRules;
            tabRulesInner.SelectedTab = tabRuleWarnings;
            SetStatus(L.Pick($"{done.Count} Hinweise behoben, {left.Count} offen – 'Speichern' sichert den Regelsatz.", $"{done.Count} hints fixed, {left.Count} open – 'Save' stores the rule set.", $"{done.Count} corrigés, {left.Count} restants."));
        }

        private void TsbScriptEditor_Click(object sender, EventArgs e)
        {
            HideColorPreview();
            using var dlg = new ScriptEditorForm(_settings.ScriptFolder, _rules);
            dlg.ShowDialog(this);
            if (!dlg.Saved)
            {
                return;
            }
            bool fromJson = !string.IsNullOrWhiteSpace(_settings.RuleSetFile) && File.Exists(_settings.RuleSetFile);
            string question = fromJson
                ? L.Pick("Skripte gespeichert. Der Regelsatz wird zurzeit aus einer JSON-Datei geladen.\nAuf die Skripte umstellen und neu einlesen?",
                    "Scripts saved. The rule set is currently loaded from a JSON file.\nSwitch to the scripts and read them again?",
                    "Scripts enregistrés. Le jeu de règles est chargé depuis un fichier JSON.\nPasser aux scripts et les relire ?")
                : L.Pick("Skripte gespeichert. Regeln jetzt neu einlesen?", "Scripts saved. Read the rules again now?", "Scripts enregistrés. Relire les règles maintenant ?");
            if (MessageBox.Show(this, question, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (fromJson)
                {
                    _settings.RuleSetFile = string.Empty;
                    propSettings.Refresh();
                }
                LoadRules();
                LoadColorTable();
            }
        }

        // ================================================================== 8 / 16 bit, name, save

        private void TsbRulesBits_Click(object sender, EventArgs e)
        {
            if (sender == tsbRules16)
            {
                // 8 -> 16 asks first; when already 16 bit one more page of 256 colours is added
                MnuRules16_Click(sender, e);
            }
            else if (_rules.ColorBits == 16)
            {
                int highest = _rules.Palette.Where(p => p.Defined).Select(p => p.Index).DefaultIfEmpty(0).Max();
                int highestGroup = _rules.AllGroupIds().DefaultIfEmpty(0).Max();
                if (highest > 0xFF || highestGroup > 0xFE)
                {
                    MessageBox.Show(this, L.Pick($"Zurück auf 8 Bit geht nicht: Farben bis {_rules.HexIndex(highest)} bzw. Gruppen bis {highestGroup:x} sind benutzt (8 Bit: höchstens ff / fe).",
                            $"Back to 8 bit is not possible: colours up to {_rules.HexIndex(highest)} or groups up to {highestGroup:x} are used (8 bit: at most ff / fe).",
                            $"Retour en 8 bits impossible : couleurs jusqu'à {_rules.HexIndex(highest)} ou groupes jusqu'à {highestGroup:x} utilisés."),
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _rules.ColorBits = 8;
                    _rules.Normalize();
                    MarkRulesDirty();
                    ApplyRules();
                    SetStatus(L.Pick("Regelsatz ist wieder 8 Bit (Dragon-kompatibel).", "The rule set is 8 bit again (Dragon compatible).", "Le jeu de règles est de nouveau en 8 bits."));
                }
            }
            UpdateRulesToolbar();
        }

        /// <summary>Highlights the active format and shows the name of the rule set.</summary>
        private void UpdateRulesToolbar()
        {
            if (_rules == null)
            {
                return;
            }
            _updatingRulesToolbar = true;
            bool is16 = _rules.ColorBits == 16;
            tsbRules8.Checked = !is16;
            tsbRules16.Checked = is16;
            tsbRules8.BackColor = is16 ? SystemColors.Control : Color.FromArgb(120, 180, 240);
            tsbRules16.BackColor = is16 ? Color.FromArgb(255, 160, 60) : SystemColors.Control;
            tsbRules8.Font = new Font(toolStripRules.Font, is16 ? FontStyle.Regular : FontStyle.Bold);
            tsbRules16.Font = new Font(toolStripRules.Font, is16 ? FontStyle.Bold : FontStyle.Regular);
            tsbRules16.Text = is16 ? L.T("16 Bit – Seite dazu (+256)") : L.T("16 Bit (bis 65 536 Farben)");
            if (tstRulesName.Text != _rules.Name)
            {
                tstRulesName.Text = _rules.Name;
            }
            _updatingRulesToolbar = false;
            UpdateRulesHelp();
        }

        private void TstRulesName_TextChanged(object sender, EventArgs e)
        {
            if (_updatingRulesToolbar || _rules == null || tstRulesName.Text == _rules.Name)
            {
                return;
            }
            _rules.Name = tstRulesName.Text;
            MarkRulesDirty();
        }

        /// <summary>Saves the rule set as JSON (file = name) and makes it the one that is loaded at start.</summary>
        private void TsbRulesSave_Click(object sender, EventArgs e)
        {
            string path = _settings.RuleSetFile;
            if (string.IsNullOrWhiteSpace(path))
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DragonMapStudio", "Regeln");
                Directory.CreateDirectory(folder);
                path = Path.Combine(folder, MakeSafeFileName(string.IsNullOrWhiteSpace(_rules.Name) ? "Regelsatz" : _rules.Name.Trim()) + ".json");
            }
            if (SaveRulesJson(path))
            {
                SetStatus(L.Pick($"Regelsatz gespeichert: {path} – wird beim nächsten Start geladen.", $"Rule set saved: {path} – loaded at the next start.", $"Jeu de règles enregistré : {path} – chargé au prochain démarrage."));
            }
        }

        // ================================================================== help

        private void TabRulesInner_SelectedIndexChanged(object sender, EventArgs e) => UpdateRulesHelp();

        private void UpdateRulesHelp()
        {
            string text;
            TabPage t = tabRulesInner.SelectedTab;
            if (t == tabRuleGroups)
            {
                text = L.Pick(
                    "GRUPPEN\nEine Gruppe fasst Farben zusammen, z. B. alle Höhen von Gras. Übergänge (Kanten) und Items gelten pro Gruppe.\n\nNeu: '+ Neue Gruppe…' (nächste freie Nummer). Danach Farben zuordnen und Übergänge anlegen.\n\nBeispiel (groups.txt):\n  // 1f = Mein Pflaster\n  c0 1f",
                    "GROUPS\nA group collects colours, e.g. all heights of grass. Transitions (edges) and items apply per group.\n\nNew: '+ New group…' (next free number). Then assign colours and add transitions.\n\nExample (groups.txt):\n  // 1f = My paving\n  c0 1f",
                    "GROUPES\nUn groupe réunit des couleurs, p. ex. toutes les hauteurs d'herbe. Transitions et objets s'appliquent par groupe.\n\nNouveau : '+ Nouveau groupe…'.");
            }
            else if (t == tabRulePalette)
            {
                text = L.Pick(
                    "FARBEN (maptrans)\nJede Farbe = eine Adresse im Kartenbild. Beim Bauen wird daraus ein Land-Tile (zufällig aus der Liste, an Hängen dessen Textur) auf der Höhe Z.\n\n'+ Neue Farbe…' oder Doppelklick auf eine Zeile öffnet den Farb-Dialog: Adresse, Name, Kategorie, RGB (wird auf Doppel geprüft), Gruppe, Höhe, Tiles mit Katalog.\n\nMaus über eine Farbe (Farbtabelle, Paletten): Vorschau der Tiles, Items und Statics.\n\nBeispiel (maptrans.txt):\n  c0 000 03e9 03ea 03eb 03ec\n  = Pflaster auf Höhe 0\n\n8 Bit: 256 Farben. Mehr: oben '16 Bit'.",
                    "COLOURS (maptrans)\nEvery colour = an address in the map image. Building turns it into a land tile (random from the list, on slopes its texture) at height Z.\n\n'+ New colour…' or a double click on a row opens the colour dialog: address, name, category, RGB (checked for duplicates), group, height, tiles with catalogue.\n\nMouse over a colour (colour table, palettes): preview of tiles, items and statics.\n\nExample (maptrans.txt):\n  c0 000 03e9 03ea 03eb 03ec\n  = paving at height 0\n\n8 bit: 256 colours. More: '16 bit' at the top.",
                    "COULEURS (maptrans)\nChaque couleur = une adresse dans l'image. La construction en fait une tuile (au hasard dans la liste) à la hauteur Z.\n\n'+ Nouvelle couleur…' ou double-clic : adresse, nom, catégorie, RVB (contrôle des doublons), groupe, hauteur, tuiles.\n\nExemple :  c0 000 03e9 03ea 03eb 03ec");
            }
            else if (t == tabRuleTransitions)
            {
                text = L.Pick(
                    "ÜBERGÄNGE\nZwischen zwei Gruppen ersetzt eine Übergangsdatei die Rand-Tiles (Map) oder setzt Statics (Static).\nMuster: 8 Zeichen für die Nachbarn NW N NO O SO S SW W – A = eigene Gruppe, B = Nachbargruppe.\n\n'+ Neue Übergangsregel…' legt die Regel an und auf Wunsch eine Beispiel-Datei mit den 12 Standard-Formen (Kanten, Außen- und Innenecken). Unten die Tiles eintragen.\n\nBeispiel:\n  betweentrans.txt:  00 1f grass2pflaster.txt\n  map/grass2pflaster.txt:\n  BBBAAAAA 0035 0036 000 000\n\nTipp: Übergänge brauchen mindestens 2 Tiles nebeneinander.",
                    "TRANSITIONS\nBetween two groups a transition file replaces the edge tiles (Map) or places statics (Static).\nPattern: 8 characters for the neighbours NW N NE E SE S SW W – A = own group, B = neighbour group.\n\n'+ New transition rule…' creates the rule and, if wanted, an example file with the 12 standard shapes (edges, outer and inner corners). Enter the tiles below.\n\nExample:\n  betweentrans.txt:  00 1f grass2paving.txt\n  map/grass2paving.txt:\n  BBBAAAAA 0035 0036 000 000\n\nTip: transitions need at least 2 tiles side by side.",
                    "TRANSITIONS\nEntre deux groupes, un fichier de transition remplace les tuiles du bord (Map) ou pose des statiques (Static).\nMotif : 8 caractères NO N NE E SE S SO O – A = propre groupe, B = voisin.\n\n'+ Nouvelle règle de transition…' crée la règle et un fichier exemple avec 12 formes.");
            }
            else if (t == tabRuleItems)
            {
                text = L.Pick(
                    "ITEMS\nPro Gruppe zufällige Statics (Bäume, Steine, Blumen …). Chance in Promille, Item-Datei mit Häufigkeit und Teilen (id z hue dx dy).\n\nBeispiel (items.txt):\n  1f 0020 0000 0000 0000 0000 pflaster.txt\nitems/pflaster.txt:\n  10 0b8f 0 0 0 0",
                    "ITEMS\nRandom statics per group (trees, stones, flowers …). Chance in per mille, item file with frequency and parts (id z hue dx dy).\n\nExample (items.txt):\n  1f 0020 0000 0000 0000 0000 paving.txt\nitems/paving.txt:\n  10 0b8f 0 0 0 0",
                    "OBJETS\nStatiques aléatoires par groupe. Chance en pour mille.");
            }
            else if (t == tabRuleWarnings)
            {
                text = L.Pick("HINWEISE\n'Prüfen' sucht doppelte RGB-Farben, Farben ohne Gruppe, fehlende Dateien.\n'Hinweise beheben' ordnet Farben ohne Gruppe zu (gleiche Tiles) und leitet Regeln mit fehlender Datei auf eine ähnlich benannte Datei um oder schaltet sie aus.\nDanach 'Speichern'.",
                    "HINTS\n'Check' looks for duplicate RGB colours, colours without group, missing files.\n'Fix hints' assigns colours without group (same tiles) and points rules with a missing file to a similarly named file or switches them off.\nThen 'Save'.",
                    "REMARQUES\n'Vérifier' cherche les couleurs RVB en double, les couleurs sans groupe, les fichiers manquants.\n'Corriger' attribue les groupes et redirige ou désactive les règles.");
            }
            else
            {
                text = L.Pick("Werkzeugleiste oben: Format 8 / 16 Bit, neue Farbe / Gruppe / Übergangsregel, Prüfen, Hinweise beheben, Skript-Editor, Speichern unter einem Namen (wird beim nächsten Start geladen).",
                    "Tool bar at the top: format 8 / 16 bit, new colour / group / transition rule, check, fix hints, script editor, save under a name (loaded at the next start).",
                    "Barre d'outils : format 8 / 16 bits, nouvelle couleur / groupe / règle, vérifier, corriger, éditeur de scripts, enregistrer sous un nom.");
            }
            text += Environment.NewLine + Environment.NewLine + (_rules.ColorBits == 16
                ? L.Pick("Format: 16 Bit – Adressen 0000–ffff, Gruppen bis 7fff, Karten als 16-Bit-PNG.", "Format: 16 bit – addresses 0000–ffff, groups up to 7fff, maps as 16-bit PNG.", "Format : 16 bits – adresses 0000–ffff.")
                : L.Pick("Format: 8 Bit (klassisches Dragon) – 256 Farben. Mehr Farben: oben '16 Bit'.", "Format: 8 bit (classic Dragon) – 256 colours. More colours: '16 bit' at the top.", "Format : 8 bits – 256 couleurs. Plus : '16 bits' en haut."));
            txtRulesHelp.Text = text.Replace("\n", Environment.NewLine);
        }
    }
}
