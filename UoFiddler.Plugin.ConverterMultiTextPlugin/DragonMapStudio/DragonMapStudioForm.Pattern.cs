/***************************************************************************
 *
 *  * Dragon Map Studio – map editor: stamps drawn once or as freehand pattern brush
 *  * (roads, rivers …), land / statics / both, live CGI preview around the mouse,
 *  * undo per part of a stroke or the whole stroke
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        /// <summary>Cells per undo step when drawing stepwise.</summary>
        private const int StepCells = 32;

        private ColorStamp _patternStamp;
        private bool _patternAdapt = true;
        private Dictionary<int, List<LayerStatic>> _patternStatics;
        private bool _patternHooked;

        // live CGI preview
        private Timer _liveTimer;
        private bool _liveEnabled;
        private bool _liveDirty;
        private bool _liveBusy;

        /// <summary>Stamp menu "Setzen": once (paste, layers respected) or as freehand pattern brush.</summary>
        private void UseStamp(ColorStamp s, bool adapt, bool repeat, bool stepUndo, bool live)
        {
            _liveEnabled = live;
            HookPatternEvents();
            mapCanvas.StrokeStepCells = stepUndo ? StepCells : 0;
            if (s.DrawMode == StampDrawMode.Freehand)
            {
                StartPatternBrush(s, adapt);
                return;
            }
            if (s.Layers == StampLayers.Both)
            {
                PlaceStamp(s, adapt, repeat);
                return;
            }
            // land only: without statics; statics only: all pixels transparent
            var copy = new ColorStamp
            {
                Name = s.Name,
                Category = s.Category,
                Width = s.Width,
                Height = s.Height,
                Pixels = s.Layers == StampLayers.StaticsOnly ? Enumerable.Repeat(ColorStampTools.Transparent, s.Pixels.Length).ToArray() : s.Pixels,
                Groups = s.Groups,
                RelativeAltitudes = s.RelativeAltitudes,
                BaseAltitude = s.BaseAltitude,
                Statics = s.Layers == StampLayers.LandOnly ? new List<LayerStatic>() : s.Statics
            };
            PlaceStamp(copy, adapt, repeat);
        }

        /// <summary>Freehand: every cell under the brush gets the stamp pixel of its position (the stamp repeats like a texture).</summary>
        private void StartPatternBrush(ColorStamp s, bool adapt)
        {
            if (_map == null)
            {
                SetStatus(L.T("Im Karten-Editor ist keine Karte geöffnet."));
                return;
            }
            CancelStamp();
            _patternStamp = s;
            _patternAdapt = adapt;
            _patternStatics = new Dictionary<int, List<LayerStatic>>();
            foreach (LayerStatic st in s.Statics ?? new List<LayerStatic>())
            {
                int k = st.Y * s.Width + st.X;
                if (!_patternStatics.TryGetValue(k, out List<LayerStatic> l))
                {
                    _patternStatics[k] = l = new List<LayerStatic>();
                }
                l.Add(st);
            }
            mapCanvas.PatternColour = PatternColour;
            tabMain.SelectedTab = tabEditor;
            SelectTool(MapTool.PatternBrush);
            mapCanvas.Focus();
            SetStatus($"Muster-Pinsel '{s.Name}': ziehen = malen (Pinselgröße in der Leiste), {LayersText(s.Layers)}" +
                      (mapCanvas.StrokeStepCells > 0 ? $", Rückgängig je {StepCells} Felder (Strg+Umschalt+Z = ganzer Strich)" : ", ein Strich = ein Rückgängig-Schritt") + ".");
            if (_liveEnabled)
            {
                MarkLiveDirty();
            }
        }

        private void HookPatternEvents()
        {
            if (_patternHooked)
            {
                return;
            }
            _patternHooked = true;
            mapCanvas.PatternCellsPainted += MapCanvas_PatternCellsPainted;
            mapCanvas.StrokeEnded += (o, e) => MarkLiveDirty();
            mapCanvas.MapEdited += (o, e) => MarkLiveDirty();
        }

        private static string LayersText(StampLayers l) => l switch
        {
            StampLayers.LandOnly => L.T("nur Land"),
            StampLayers.StaticsOnly => L.T("nur Statics"),
            _ => L.T("Land + Statics")
        };

        private static int Mod(int a, int m) => ((a % m) + m) % m;

        /// <summary>Colour of the pattern at (x, y): stamp pixel of the position, height adapted to the colour that lies there.</summary>
        private int PatternColour(int x, int y, ushort current)
        {
            ColorStamp s = _patternStamp;
            if (s == null || s.Layers == StampLayers.StaticsOnly)
            {
                return -1;
            }
            int i = Mod(y, s.Height) * s.Width + Mod(x, s.Width);
            ushort c = s.Pixels[i];
            if (c == ColorStampTools.Transparent)
            {
                return -1;
            }
            int g = i < s.Groups.Length ? s.Groups[i] : -1;
            if (!_patternAdapt || g < 0 || current >= _rules.Palette.Count)
            {
                return c;
            }
            int want = _rules.Palette[current].Altitude + (i < s.RelativeAltitudes.Length ? s.RelativeAltitudes[i] : 0);
            List<DragonPaletteEntry> entries = _rules.EntriesOfGroup(g).Where(e => e.Defined).ToList();
            return entries.Count == 0 ? c : entries.OrderBy(e => Math.Abs(e.Altitude - want)).ThenBy(e => e.Index == c ? 0 : 1).First().Index;
        }

        /// <summary>Statics of the pattern for the newly painted cells – in the statics layer, in the same undo step.</summary>
        private void MapCanvas_PatternCellsPainted(object sender, List<Point> cells)
        {
            ColorStamp s = _patternStamp;
            if (s == null || s.Layers == StampLayers.LandOnly || _patternStatics == null)
            {
                return;
            }
            var set = new HashSet<Point>(cells);
            List<LayerStatic> removed = _staticsLayer.Items.Where(st => set.Contains(new Point(st.X, st.Y))).ToList();
            var added = new List<LayerStatic>();
            foreach (Point p in cells)
            {
                if (_patternStatics.TryGetValue(Mod(p.Y, s.Height) * s.Width + Mod(p.X, s.Width), out List<LayerStatic> l))
                {
                    added.AddRange(l.Select(st => new LayerStatic { X = p.X, Y = p.Y, Dz = st.Dz, Id = st.Id, Hue = st.Hue }));
                }
            }
            if (removed.Count == 0 && added.Count == 0)
            {
                return;
            }
            void Swap(List<LayerStatic> remove, List<LayerStatic> add)
            {
                var r = new HashSet<(int, int, int, ushort, ushort)>(remove.Select(x => (x.X, x.Y, x.Dz, x.Id, x.Hue)));
                _staticsLayer.Items.RemoveAll(st => r.Contains((st.X, st.Y, st.Dz, st.Id, st.Hue)));
                _staticsLayer.Items.AddRange(add);
                _staticsLayer.Modified = true;
                _map.Modified = true;
                mapCanvas.SetStaticMarkers(_staticsLayer.Items);
            }
            Swap(removed, added);
            _history.AddExtra(() => Swap(added, removed), () => Swap(removed, added));
        }

        /// <summary>Strg+Umschalt+Z: all parts of the last stroke.</summary>
        private void UndoWholeStroke()
        {
            if (_busy || !_history.CanUndo)
            {
                return;
            }
            Rectangle r = _history.UndoWholeGroup();
            mapCanvas.InvalidateMap();
            _thumbDirty = true;
            UpdateTitle();
            SetStatus(L.T("Ganzer Strich rückgängig."));
            MarkLiveDirty();
        }

        // ================================================================== live CGI preview while drawing

        private void MarkLiveDirty()
        {
            if (!_liveEnabled || _map == null || mapCanvas.Tool != MapTool.PatternBrush && !mapCanvas.PasteActive)
            {
                return;
            }
            _liveDirty = true;
            if (_liveTimer == null)
            {
                _liveTimer = new Timer { Interval = 350 };
                _liveTimer.Tick += (s, e) => LiveTick();
            }
            _liveTimer.Start();
        }

        /// <summary>Builds the surroundings of the mouse (a bit larger than the stamp, at most 96×96) in the background and shows it.</summary>
        private void LiveTick()
        {
            if (!_liveDirty || _liveBusy || _map == null)
            {
                return;
            }
            _liveDirty = false;
            _liveTimer.Stop();
            Point c = mapCanvas.CursorTile;
            if (!_map.InBounds(c.X, c.Y))
            {
                RectangleF v = mapCanvas.ViewRect;
                c = new Point((int)(v.X + v.Width / 2), (int)(v.Y + v.Height / 2));
            }
            int size = Math.Clamp(Math.Max(_patternStamp?.Width ?? 32, _patternStamp?.Height ?? 32) * 2, 48, 96);
            var area = new Rectangle(c.X - size / 2, c.Y - size / 2, size, size);
            area.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
            int x0 = Math.Max(0, (area.Left - 8) / 8 * 8), y0 = Math.Max(0, (area.Top - 8) / 8 * 8);
            int x1 = Math.Min(_map.Width, (area.Right + 8 + 7) / 8 * 8), y1 = Math.Min(_map.Height, (area.Bottom + 8 + 7) / 8 * 8);
            var r = Rectangle.FromLTRB(x0, y0, x1, y1);
            IndexedMap crop = _map.Crop(r);
            BuildOptions o = CurrentBuildOptions();
            o.ExtraStatics = _staticsLayer.Crop(r);
            o.ExtraStaticsComplete = _staticsLayer.Complete;
            DragonRuleSet rules = _rules;
            var inner = new Rectangle(area.X - r.X, area.Y - r.Y, area.Width, area.Height);
            _liveBusy = true;
            Task.Run(() =>
            {
                try
                {
                    return new DragonCompiler(rules, o, s => { }).BuildInMemory(crop, null, System.Threading.CancellationToken.None);
                }
                catch (Exception)
                {
                    return null;
                }
            }).ContinueWith(t =>
            {
                if (IsDisposed)
                {
                    return;
                }
                BeginInvoke(new Action(() =>
                {
                    _liveBusy = false;
                    if (t.Result != null)
                    {
                        ShowPreviewWindow(t.Result, r, inner, false);
                    }
                    if (_liveDirty)
                    {
                        _liveTimer.Start();
                    }
                }));
            });
        }
    }
}
