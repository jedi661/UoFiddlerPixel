/***************************************************************************
 *
 *  * Dragon Map Studio – CGI view -> colours: marked area as colour stamp,
 *  * whole map as colour map (+ statics layer) for the map editor
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        /// <summary>Tiles the compiler makes itself (sea floor of the coast profile) belong to the water group.</summary>
        private Dictionary<ushort, int> CompilerTileGroups()
        {
            var d = new Dictionary<ushort, int>();
            foreach (ushort t in HexList.Parse(CoastSettings.From(CurrentBuildOptions()).FloorTiles))
            {
                d[t] = _settings.Generator.WaterGroup;
            }
            return d;
        }

        private LandTransitionEngine TransitionEngine()
        {
            try
            {
                return new LandTransitionEngine(_rules);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Missing colours: list to confirm / create all / none. False = cancelled.</summary>
        private bool ResolveMissingColours(CgiConversion conv, MissingColorMode mode, List<string> log)
        {
            if (mode == MissingColorMode.ExistingOnly || conv.Suggestions.Count == 0)
            {
                conv.Suggestions.ForEach(s => s.Accept = false);
                return true;
            }
            if (mode == MissingColorMode.Suggest)
            {
                using var dlg = new ColorSuggestionDialog(conv.Suggestions, _rules);
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }
                dlg.ApplyTo();
            }
            List<string> created = CgiToColor.CreateColours(_rules, conv.Suggestions);
            log.AddRange(created);
            if (conv.Suggestions.Any(s => s.CreatedIndex >= 0))
            {
                created.ForEach(Log);
                Log("Neue Farben: mit 'JSON speichern' oder 'Als Dragon-Skripte exportieren' dauerhaft machen.");
                MarkRulesDirty();
                ApplyRules();
            }
            return true;
        }

        /// <summary>CGI view: the marked area (land, heights, statics) as colour stamp for the map editor.</summary>
        private void CgiAreaToColorStamp()
        {
            if (_cgiMap == null || isoView.Area.IsEmpty)
            {
                SetStatus(L.T("Erst mit dem Werkzeug 'Bereich' (Leiste Land) ein Stück der Karte markieren – z. B. ein Haus mit Boden, eine Kreuzung, einen Eingang."));
                return;
            }
            Rectangle area = isoView.Area;
            LandTransitionEngine engine = TransitionEngine();
            CgiConversion conv = CgiToColor.Analyze(_cgiMap, area, _rules, engine, true, CompilerTileGroups());
            string summary = string.Join(Environment.NewLine, conv.Log.Skip(1));
            IEnumerable<string> cats = _stamps.Select(s => s.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Concat(ColorStampTools.Categories.Skip(1));
            using var dlg = new CgiStampDialog("Bereich → Farb-Stempel", summary, cats, true, $"Stempel {area.X}_{area.Y}");
            if (dlg.ShowDialog(this) != DialogResult.OK || dlg.StampName.Length == 0)
            {
                return;
            }
            var log = new List<string>();
            if (!ResolveMissingColours(conv, dlg.Colors, log))
            {
                return;
            }
            CgiToColor.Finish(conv, _cgiMap, _rules, engine);
            ColorStamp stamp = CgiToColor.ToStamp(conv, _rules, _settings.Generator.WaterGroup, dlg.StampName, dlg.Category, dlg.Statics, log);
            if (dlg.Statics == StaticsMode.ItemRule && conv.Statics.Count > 0)
            {
                MarkRulesDirty();
                ApplyRules();
            }
            string file = ColorStampTools.Save(stamp, ColorStampFolder);
            LoadColorStamps(file);
            string report = CgiToColor.Report(conv, _rules, _cgiMap.Source) + Environment.NewLine + string.Join(Environment.NewLine, log);
            string path = WriteLog($"cgi_stempel_{MakeSafeFileName(dlg.StampName)}.txt", report);
            Log($"Farb-Stempel '{dlg.StampName}' gespeichert: {file} ({stamp.Width}×{stamp.Height}, {stamp.Statics.Count} Statics) – Bericht: {path}");
            SetStatus($"Farb-Stempel '{dlg.StampName}' gespeichert – im Karten-Editor unter Stempel. Bericht: {path}");
        }

        /// <summary>CGI view: the whole map as colour map + statics layer in the map editor (rebuild / find errors).</summary>
        private void CgiMapToColorMap()
        {
            if (_cgiMap == null)
            {
                SetStatus(L.T("Erst eine Karte in der CGI-Ansicht laden oder bauen."));
                return;
            }
            if (!ConfirmDiscardMap())
            {
                return;
            }
            MapData data = _cgiMap;
            DragonRuleSet rules = _rules;
            LandTransitionEngine engine = TransitionEngine();
            Dictionary<ushort, int> extra = CompilerTileGroups();
            RunBusy("Karte analysieren", (progress, token) => CgiToColor.Analyze(data, new Rectangle(0, 0, data.Width, data.Height), rules, engine, true, extra), conv =>
            {
                string summary = string.Join(Environment.NewLine, conv.Log.Skip(1));
                using var dlg = new CgiStampDialog("Ganze Karte → Farbkarte", summary, Array.Empty<string>(), false, string.Empty);
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                var log = new List<string>();
                if (!ResolveMissingColours(conv, dlg.Colors, log))
                {
                    return;
                }
                CgiToColor.Finish(conv, data, rules, engine);
                int bits = rules.ColorBits;
                var map = new IndexedMap(data.Width, data.Height, conv.Colours, bits)
                {
                    FilePath = Path.Combine(_settings.ProjectFolder, "Aus_CGI_" + MakeSafeFileName(Path.GetFileNameWithoutExtension(data.Source ?? "Karte")) + (bits == 16 ? ".png" : ".bmp")),
                    Modified = true
                };
                SetMap(map);
                if (dlg.Statics == StaticsMode.LandOnly || dlg.Statics == StaticsMode.ItemRule)
                {
                    if (dlg.Statics == StaticsMode.ItemRule)
                    {
                        log.Add("Item-Regel passt nicht für eine ganze Karte – die Statics wurden weggelassen (für Teile: 'Bereich → Farb-Stempel').");
                    }
                }
                else
                {
                    // whole map: absolute heights, so every static stands exactly where it stood
                    _staticsLayer.Items.AddRange(conv.Statics.Select(st => new LayerStatic
                    {
                        X = st.X, Y = st.Y, Id = st.Id, Hue = st.Hue, Absolute = true, Dz = data.LandZ(st.X, st.Y) + st.Dz
                    }));
                    _staticsLayer.Complete = true;
                    _staticsLayer.Modified = true;
                    log.Add("Statics-Ebene ist vollständig: beim Bauen setzt Dragon keine eigenen Statics dazu (keine doppelten).");
                    mapCanvas.SetStaticMarkers(_staticsLayer.Items);
                }
                string report = CgiToColor.Report(conv, rules, data.Source) + Environment.NewLine + string.Join(Environment.NewLine, log);
                string path = WriteLog("cgi_karte_zu_farben.txt", report);
                tabMain.SelectedTab = tabEditor;
                Log($"Farbkarte aus der CGI-Karte: {map.Width}×{map.Height}, {_staticsLayer.Items.Count:N0} Statics in der Statics-Ebene – Bericht: {path}");
                SetStatus($"Farbkarte erzeugt (noch nicht gespeichert: {map.FilePath}). Bericht: {path}");
            });
        }
    }
}
