// /***************************************************************************
//  *
//  * Dragon Map Studio – switch the user interface language (Deutsch / English / Français)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        /// <summary>German texts of combo box items that are translated (the lists are filled in German).</summary>
        private readonly Dictionary<object, List<object>> _comboOriginals = new Dictionary<object, List<object>>();

        private void MnuLanguage_Click(object sender, EventArgs e)
        {
            UiLanguage lang = sender == mnuLangEn ? UiLanguage.English : sender == mnuLangFr ? UiLanguage.Francais : UiLanguage.Deutsch;
            ApplyLanguage(lang, true);
        }

        /// <summary>Switches all texts of the studio: menus, tool bars, tabs, labels, tool tips, settings, help, key help.</summary>
        private void ApplyLanguage(UiLanguage lang, bool save)
        {
            L.Load(lang);
            UiTranslator.Apply(this, toolTip);
            foreach (object combo in new object[] { cmbCopyFormat, cmbM2BSize, cmbCgiItemSize, cmbCgiLandSize, tscCgiBrush })
            {
                TranslateComboItems(combo);
            }
            // lists filled from data: only the first entry is a text
            if (cmbCgiLandPreset.Items.Count > 0)
            {
                int sel = cmbCgiLandPreset.SelectedIndex;
                cmbCgiLandPreset.Items[0] = L.T("Aus der Farbtabelle: Boden, Weg, Höhle …");
                cmbCgiLandPreset.SelectedIndex = sel;
            }
            if (cmbCgiTplCategory.Items.Count > 0)
            {
                int sel = cmbCgiTplCategory.SelectedIndex;
                cmbCgiTplCategory.Items[0] = L.T("Alle Kategorien");
                cmbCgiTplCategory.SelectedIndex = sel;
            }
            mnuLangDe.Checked = lang == UiLanguage.Deutsch;
            mnuLangEn.Checked = lang == UiLanguage.English;
            mnuLangFr.Checked = lang == UiLanguage.Francais;
            cmbHelpLanguage.SelectedIndex = (int)lang;

            // property grids and rule grids read the (translated) display names again
            foreach (PropertyGrid grid in new[] { propSettings, propBuild, propGenerator, propOutline })
            {
                object o = grid.SelectedObject;
                grid.SelectedObject = null;
                grid.SelectedObject = o;
            }
            BindRuleGrids();

            // texts that are made in code
            UpdateCgiState();
            UpdateCgiKeys();
            CgiLandSettings_Changed(this, EventArgs.Empty);
            CgiCoastSettings_Changed(this, EventArgs.Empty);
            CgiCaveSettings_Changed(this, EventArgs.Empty);
            IsoView_SelectionChanged(this, EventArgs.Empty);
            lblCgiTplInfo.Text = L.T("Ordner:") + " " + _settings.TemplateFolder;

            if (save && _settings.Language != lang)
            {
                _settings.Language = lang;
                try
                {
                    _settings.Save();
                }
                catch (Exception)
                {
                    // not important
                }
            }
            SetStatus(L.Pick("Sprache: Deutsch", "Language: English", "Langue : français"));
        }

        /// <summary>Translates the string items of a combo box; the selection stays.</summary>
        private void TranslateComboItems(object combo)
        {
            ComboBox.ObjectCollection items = combo is ToolStripComboBox ts ? ts.Items : (combo as ComboBox)?.Items;
            if (items == null)
            {
                return;
            }
            if (!_comboOriginals.TryGetValue(combo, out List<object> originals) || originals.Count != items.Count)
            {
                originals = new List<object>();
                foreach (object o in items)
                {
                    originals.Add(o);
                }
                _comboOriginals[combo] = originals;
            }
            int sel = combo is ToolStripComboBox t2 ? t2.SelectedIndex : ((ComboBox)combo).SelectedIndex;
            for (int i = 0; i < items.Count; i++)
            {
                if (originals[i] is string de)
                {
                    items[i] = L.T(de);
                }
            }
            if (combo is ToolStripComboBox t3)
            {
                t3.SelectedIndex = sel;
            }
            else
            {
                ((ComboBox)combo).SelectedIndex = sel;
            }
        }
    }
}
