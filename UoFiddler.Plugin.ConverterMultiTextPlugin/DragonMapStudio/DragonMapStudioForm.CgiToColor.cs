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
            bool into = false;
            if (_map != null && _map.Width == _cgiMap.Width && _map.Height == _cgiMap.Height)
            {
                DialogResult r = MessageBox.Show(this,
                    L.T("Die geöffnete Farbkarte hat dieselbe Größe.\n\nJa = die CGI-Karte in die geöffnete Farbkarte schreiben (Rückgängig mit Strg+Z möglich, Statics-Ebene wird ersetzt)\n" +
                        "Nein = als neue Karte öffnen (die geöffnete bleibt in der Datei, ungespeicherte Änderungen werden abgefragt)\nAbbrechen = nichts tun"),
                    Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel)
                {
                    return;
                }
                into = r == DialogResult.Yes;
            }
            if (!into && !ConfirmDiscardMap())
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
                if (into && _map != null && _map.Width == data.Width && _map.Height == data.Height)
                {
                    WriteConversionIntoEditor(conv, data, dlg.Statics, true);
                    string rep = CgiToColor.Report(conv, rules, data.Source) + Environment.NewLine + string.Join(Environment.NewLine, log);
                    string p = WriteLog("cgi_karte_zu_farben.txt", rep);
                    tabMain.SelectedTab = tabEditor;
                    SetStatus($"CGI-Karte in die geöffnete Farbkarte geschrieben (Strg+Z nimmt es zurück). Bericht: {p}");
                    return;
                }
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

        // ================================================================== CGI area -> the open colour map (same place)

        /// <summary>
        /// Writes the colours of a conversion into the open colour map at the same place (one undo step) and replaces the
        /// statics layer there (absolute heights = exactly like in the CGI view).
        /// </summary>
        private void WriteConversionIntoEditor(CgiConversion conv, MapData data, StaticsMode statics, bool wholeMap)
        {
            Rectangle a = conv.Area;
            List<LayerStatic> before = _staticsLayer.Items.Where(st => a.Contains(st.X, st.Y)).ToList();
            List<LayerStatic> after = statics == StaticsMode.Layer
                ? conv.Statics.Select(st => new LayerStatic
                {
                    X = st.X + a.X, Y = st.Y + a.Y, Id = st.Id, Hue = st.Hue, Absolute = true, Dz = data.LandZ(st.X + a.X, st.Y + a.Y) + st.Dz
                }).ToList()
                : new List<LayerStatic>();
            bool completeBefore = _staticsLayer.Complete;
            void Swap(List<LayerStatic> remove, List<LayerStatic> add, bool complete)
            {
                var set = new HashSet<(int, int, int, ushort, ushort)>(remove.Select(r => (r.X, r.Y, r.Dz, r.Id, r.Hue)));
                _staticsLayer.Items.RemoveAll(st => a.Contains(st.X, st.Y) && set.Contains((st.X, st.Y, st.Dz, st.Id, st.Hue)));
                _staticsLayer.Items.AddRange(add);
                _staticsLayer.Complete = complete;
                _staticsLayer.Modified = true;
                mapCanvas.SetStaticMarkers(_staticsLayer.Items);
            }
            bool completeAfter = wholeMap && statics == StaticsMode.Layer ? true : completeBefore;
            mapCanvas.EditRect(a, wholeMap ? "CGI-Karte → Farben" : "CGI-Bereich → Farben", m =>
            {
                for (int y = 0; y < a.Height; y++)
                {
                    Array.Copy(conv.Colours, y * a.Width, m.Pixels, (a.Y + y) * m.Width + a.X, a.Width);
                }
                Swap(before, after, completeAfter);
            }, () => _history.AddExtra(() => Swap(after, before, completeBefore), () => Swap(before, after, completeAfter)));
            _thumbDirty = true;
        }

        /// <summary>CGI view: the marked area into the open colour map at the same place (map editor entries elsewhere stay).</summary>
        private void CgiAreaToEditorMap()
        {
            if (_cgiMap == null || isoView.Area.IsEmpty)
            {
                SetStatus(L.T("Erst mit dem Werkzeug 'Bereich' (Leiste Land) ein Stück markieren."));
                return;
            }
            if (_map == null || _map.Width != _cgiMap.Width || _map.Height != _cgiMap.Height)
            {
                MessageBox.Show(this, L.T("Im Karten-Editor muss eine Farbkarte in derselben Größe wie die CGI-Karte geöffnet sein."), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Rectangle area = isoView.Area;
            LandTransitionEngine engine = TransitionEngine();
            CgiConversion conv = CgiToColor.Analyze(_cgiMap, area, _rules, engine, true, CompilerTileGroups());
            using var dlg = new CgiStampDialog("CGI-Bereich → Farbkarte (gleiche Stelle)", string.Join(Environment.NewLine, conv.Log.Skip(1)), Array.Empty<string>(), false, string.Empty);
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            var log = new List<string>();
            if (!ResolveMissingColours(conv, dlg.Colors, log))
            {
                return;
            }
            CgiToColor.Finish(conv, _cgiMap, _rules, engine);
            WriteConversionIntoEditor(conv, _cgiMap, dlg.Statics == StaticsMode.ItemRule ? StaticsMode.LandOnly : dlg.Statics, false);
            if (dlg.Statics == StaticsMode.ItemRule)
            {
                SetStatus(L.T("Item-Regel gibt es nur für Stempel – die Statics wurden nicht übernommen."));
            }
            mapCanvas.CenterOn(area.X + area.Width / 2.0, area.Y + area.Height / 2.0);
            SetStatus($"CGI-Bereich {area.Width}×{area.Height} in die Farbkarte geschrieben (x {area.X}, y {area.Y}) – Strg+Z im Karten-Editor nimmt es zurück.");
        }

        // ================================================================== map editor -> refresh the CGI view

        /// <summary>
        /// Map editor: builds the visible part (at most 1024×1024, + 8 tiles border) again and puts it into the CGI map – the
        /// rest of the CGI map stays. Without a CGI map of this size the whole map is built.
        /// </summary>
        private void RefreshCgiFromEditor()
        {
            if (_map == null)
            {
                return;
            }
            RectangleF v = mapCanvas.ViewRect;
            double cx = v.X + v.Width / 2, cy = v.Y + v.Height / 2;
            if (_cgiMap == null || _cgiMap.Width != _map.Width || _cgiMap.Height != _map.Height)
            {
                BuildCgiFromEditor(cx, cy);
                return;
            }
            Rectangle area = Rectangle.Round(v);
            if (!mapCanvas.Selection.IsEmpty)
            {
                area = mapCanvas.Selection;
            }
            if (area.Width > 1024 || area.Height > 1024)
            {
                area = new Rectangle((int)cx - 512, (int)cy - 512, 1024, 1024);
            }
            area.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
            if (!_cgiFromEditor || _cgiUserEdits)
            {
                DialogResult r = MessageBox.Show(this,
                    L.T("Die CGI-Karte hat eigene Änderungen (gesetzte Statics, Land) oder stammt nicht aus dieser Farbkarte.\n" +
                        "Im aktualisierten Bereich werden sie durch die Farbkarte ersetzt.\n\n" +
                        "Ja = vorher den CGI-Bereich in die Farbkarte übernehmen (dann geht nichts verloren)\nNein = trotzdem aktualisieren\nAbbrechen = nichts tun"),
                    Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (r == DialogResult.Cancel)
                {
                    return;
                }
                if (r == DialogResult.Yes)
                {
                    LandTransitionEngine eng = TransitionEngine();
                    CgiConversion keep = CgiToColor.Analyze(_cgiMap, area, _rules, eng, true, CompilerTileGroups());
                    var log = new List<string>();
                    if (!ResolveMissingColours(keep, MissingColorMode.Suggest, log))
                    {
                        return;
                    }
                    CgiToColor.Finish(keep, _cgiMap, _rules, eng);
                    WriteConversionIntoEditor(keep, _cgiMap, StaticsMode.Layer, false);
                }
            }
            int x0 = Math.Max(0, (area.Left - 8) / 8 * 8), y0 = Math.Max(0, (area.Top - 8) / 8 * 8);
            int x1 = Math.Min(_map.Width, (area.Right + 8 + 7) / 8 * 8), y1 = Math.Min(_map.Height, (area.Bottom + 8 + 7) / 8 * 8);
            var r8 = Rectangle.FromLTRB(x0, y0, x1, y1);
            IndexedMap crop = _map.Crop(r8);
            BuildOptions o = CurrentBuildOptions();
            o.ExtraStatics = _staticsLayer.Crop(r8);
            o.ExtraStaticsComplete = _staticsLayer.Complete;
            DragonRuleSet rules = _rules;
            EnsureCaveRule(crop);
            MapData target = _cgiMap;
            RunBusy("CGI aktualisieren", (progress, token) => new DragonCompiler(rules, o, s => { }).BuildInMemory(crop, progress, token), built =>
            {
                if (_cgiMap != target)
                {
                    return;
                }
                // only the inner area (the border of 8 tiles was only there for the transitions)
                var land = new List<LandChange>();
                var statics = new List<StaticChange>();
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    for (int x = area.Left; x < area.Right; x++)
                    {
                        int i = y * target.Width + x;
                        ushort id = built.LandId(x - r8.X, y - r8.Y);
                        sbyte z = (sbyte)built.LandZ(x - r8.X, y - r8.Y);
                        if (target.Tiles[i] != id || target.Z[i] != z)
                        {
                            land.Add(new LandChange { Index = i, OldId = target.Tiles[i], OldZ = target.Z[i], NewId = id, NewZ = z });
                        }
                        statics.AddRange(target.StaticsAt(x, y).ToList().Select(MapData.Remove));
                        foreach (StaticItem s in built.StaticsAt(x - r8.X, y - r8.Y))
                        {
                            statics.Add(MapData.Add(new StaticItem { Id = s.Id, Hue = s.Hue, X = x, Y = y, Z = s.Z }));
                        }
                    }
                }
                target.Apply(statics, land);
                _cgiFromEditor = true;
                _cgiUserEdits = false;
                isoView.Refresh(true);
                UpdateCgiThumbnail();
                tabMain.SelectedTab = tabCgi;
                isoView.CenterOn(cx, cy);
                isoView.Focus();
                SetStatus($"CGI-Ansicht aktualisiert: Bereich {area.Width}×{area.Height} ab ({area.X}, {area.Y}) neu gebaut ({land.Count:N0} Land-Tiles geändert).");
            });
        }
    }
}
