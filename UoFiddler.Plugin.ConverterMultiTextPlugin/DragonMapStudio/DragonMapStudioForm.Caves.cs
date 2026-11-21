/***************************************************************************
 *
 *  * Dragon Map Studio – cave entrances in the 2D map editor (colours)
 *
 ***************************************************************************/

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private CaveColorOptions CaveColorOptions()
        {
            _settings.CaveColor ??= new CaveColorOptions();
            CaveColorOptions o = _settings.CaveColor;
            o.MountainGroup = _settings.Generator.MountainGroup;
            o.WaterGroup = _settings.Generator.WaterGroup;
            return o;
        }

        /// <summary>
        /// Groups / rules the colour stamp needs: black entrance group (created when missing), rule entrance -> mountain,
        /// rule cave floor -> dirt. Returns the entrance group (or 1d for the invisible variant).
        /// </summary>
        private int EnsureCaveColourRules(bool invisible)
        {
            CaveColorOptions o = CaveColorOptions();
            var log = new List<string>();
            int group;
            if (invisible)
            {
                group = _settings.Generator.CaveGroup;
                if (_rules.EnsureCaveRule(group, o.MountainGroup))
                {
                    log.Add($"Regel {_rules.HexGroup(group)} → {_rules.HexGroup(o.MountainGroup)} (unsichtbarer Eingang) aktiviert.");
                }
            }
            else
            {
                group = CaveColorStamp.EnsureVisibleGroup(_rules, o.MountainGroup, log);
            }
            CaveColorStamp.EnsureCaveDirtRule(_rules, o, log);
            if (log.Count > 0)
            {
                log.ForEach(Log);
                Log("Mit 'JSON speichern' oder 'Als Dragon-Skripte exportieren' dauerhaft machen.");
                MarkRulesDirty();
                ApplyRules();
            }
            return group;
        }

        private void MapCanvas_ToolClicked(object sender, MapClickEventArgs e)
        {
            if (mapCanvas.Tool != MapTool.CaveStamp || _map == null)
            {
                return;
            }
            bool invisible = (e.Modifiers & Keys.Shift) != 0 || CaveColorOptions().Invisible;
            int group = EnsureCaveColourRules(invisible);
            if (group < 0)
            {
                SetStatus("Höhleneingang: keine Eingangs-Gruppe verfügbar (siehe Log).");
                return;
            }
            CaveColorOptions o = CaveColorOptions();
            string message = string.Empty;
            int radius = o.Width + o.PathLength + 8;
            if (e.Button == MouseButtons.Right)
            {
                mapCanvas.EditAround(e.X, e.Y, radius, "Höhleneingang entfernen", m => CaveColorStamp.Remove(m, _rules, e.X, e.Y, o, group, out message));
            }
            else
            {
                mapCanvas.EditAround(e.X, e.Y, radius, "Höhleneingang", m => CaveColorStamp.Stamp(m, _rules, e.X, e.Y, o, group, out message));
            }
            SetStatus(message);
        }
    }
}
