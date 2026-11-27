/***************************************************************************
 *
 *  * Dragon Map Studio – whole map areas as stamps: a town of an old map (land tiles, heights, statics) becomes a
 *  *  template for the CGI view (tool "Vorlage") or a colour stamp for the map editor. The area is marked in the
 *  *  window "Fremde Karte" (or in the CGI view) – by dragging, Umschalt = extend, Strg + Klick = two corners with
 *  *  scrolling in between, or by coordinates.
 *
 ***************************************************************************/

using System;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private ForeignMapForm _foreignStamps;

        /// <summary>Tool items for templates (place on the original position) – once, from BuildToolOptions.</summary>
        private void InitAreaStamps()
        {
            var origin = new ToolStripButton(L.T("An Original-Position setzen"))
            {
                ToolTipText = L.T("Die Vorlage genau dort setzen, wo sie auf der Quell-Karte lag (gleiche X / Y) – Höhe wie unter der Maus: Boden dort + Z")
            };
            origin.Click += (s, e) => isoView.PlaceTemplateAtOrigin(false);
            var originZ = new ToolStripButton(L.T("… mit Original-Höhe"))
            {
                ToolTipText = L.T("Wie 'An Original-Position', dazu mit den Original-Höhen der Quell-Karte (1:1-Kopie einer Stadt in eine gleich große Karte)")
            };
            originZ.Click += (s, e) => isoView.PlaceTemplateAtOrigin(true);
            foreach (ToolStripItem item in new ToolStripItem[] { origin, originZ })
            {
                item.Visible = false;
                toolStripCgiLand.Items.Add(item);
                _cgiToolItems.Add((item, t => t == IsoTool.Template));
            }
        }

        /// <summary>CGI: mark an area by numbers.</summary>
        private void AskCgiArea()
        {
            if (_cgiMap == null)
            {
                SetStatus(L.T("Erst eine Karte in der CGI-Ansicht laden oder bauen."));
                return;
            }
            SelectCgiTool(IsoTool.Area);
            isoView.AskArea(this);
        }

        /// <summary>Opens the window "Fremde Karte" as the source of area stamps (old map → template / colour stamp).</summary>
        private void OpenForeignStamps()
        {
            if (_foreignStamps != null && !_foreignStamps.IsDisposed)
            {
                _foreignStamps.Activate();
                return;
            }
            _foreignStamps = new ForeignMapForm(ConfigureLikeMain, false) { Owner = this };
            _foreignStamps.TemplateTaken += (s, t) =>
            {
                SaveTemplate(t, true, t.Name, L.T("Städte"));
                SetStatus(L.T("Vorlage gespeichert – CGI-Ansicht: Werkzeug 'Vorlage' wählen und auf die Karte klicken, oder 'An Original-Position setzen'."));
            };
            _foreignStamps.ColorStampRequested += (map, area) =>
            {
                AreaToColorStamp(map, area);
                _foreignStamps?.Activate();
            };
            _foreignStamps.FormClosed += (s, e) => _foreignStamps = null;
            _foreignStamps.Show(this);
        }
    }
}
