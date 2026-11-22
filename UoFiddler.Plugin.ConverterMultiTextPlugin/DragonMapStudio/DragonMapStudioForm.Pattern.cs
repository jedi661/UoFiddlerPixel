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
        private bool _liveAlways;
        private bool _liveFollow;
        private Point _liveCenter = new Point(-10000, -10000);
        private ToolStripButton _liveButton;
        private ToolStripButton _liveFollowButton;
        private bool _liveHooked;
        private Timer _liveTimer;
        private bool _liveEnabled;
        private bool _liveDirty;
        private bool _liveBusy;

        /// <summary>Stamp menu "Setzen": once (paste, layers respected) or as freehand pattern brush.</summary>
        private void UseStamp(ColorStamp s, bool adapt, bool repeat, bool stepUndo, bool live)
        {
            _liveEnabled = live;
            _groupEntries.Clear();
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
            // live while drawing (not only after letting go)
            mapCanvas.PatternCellsPainted += (o, cells) =>
            {
                _strokePainted.AddRange(cells);
                MarkLiveDirty();
            };
            mapCanvas.StrokeFinishing += (o, e) => FixStrokeAltitudes();
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
            if (!_patternAdapt)
            {
                return c;
            }
            return StampPixelForHeight(s, i, current);
        }

        private readonly Dictionary<int, List<DragonPaletteEntry>> _groupEntries = new Dictionary<int, List<DragonPaletteEntry>>();

        /// <summary>
        /// Stamp pixel i on a cell whose colour is <paramref name="current"/>: the colour of its group at the wanted height –
        /// Original = height there + height in the stamp, Terrain = height there, Fixed = the fixed height; + offset.
        /// </summary>
        private int StampPixelForHeight(ColorStamp s, int i, ushort current)
        {
            ushort c = s.Pixels[i];
            if (c == ColorStampTools.Transparent)
            {
                return current;
            }
            int g = i < s.Groups.Length ? s.Groups[i] : -1;
            if (g < 0)
            {
                return c;
            }
            int here = current < _rules.Palette.Count ? _rules.Palette[current].Altitude : s.BaseAltitude;
            int want = _settings.PatternHeight switch
            {
                PatternHeightMode.Original => here + (i < s.RelativeAltitudes.Length ? s.RelativeAltitudes[i] : 0),
                PatternHeightMode.Fixed => _settings.PatternHeightValue,
                _ => here
            } + _settings.PatternHeightOffset;
            if (!_groupEntries.TryGetValue(g, out List<DragonPaletteEntry> entries))
            {
                _groupEntries[g] = entries = _rules.EntriesOfGroup(g).Where(e => e.Defined).ToList();
            }
            return entries.Count == 0 ? c : entries.OrderBy(e => Math.Abs(e.Altitude - want)).ThenBy(e => e.Index == c ? 0 : 1).First().Index;
        }

        private readonly List<Point> _strokePainted = new List<Point>();

        /// <summary>
        /// Automatic repair after a pattern stroke (still in its undo step): painted cells and their border whose altitude lies more
        /// than 10 away from the average of the 8 neighbours get the colour of their group closest to that average.
        /// </summary>
        private void FixStrokeAltitudes()
        {
            if (!_settings.PatternAutoFix || _strokePainted.Count == 0 || _map == null)
            {
                _strokePainted.Clear();
                return;
            }
            int minX = _strokePainted.Min(p => p.X) - 1, maxX = _strokePainted.Max(p => p.X) + 1;
            int minY = _strokePainted.Min(p => p.Y) - 1, maxY = _strokePainted.Max(p => p.Y) + 1;
            var box = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            box.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
            _history.Touch(box);
            var before = (ushort[])_map.Pixels.Clone();
            int Alt(int x, int y) => before[y * _map.Width + x] < _rules.Palette.Count ? _rules.Palette[before[y * _map.Width + x]].Altitude : 0;
            int n = 0;
            for (int y = box.Top; y < box.Bottom; y++)
            {
                for (int x = box.Left; x < box.Right; x++)
                {
                    int sum = 0, cnt = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if ((dx != 0 || dy != 0) && _map.InBounds(x + dx, y + dy))
                            {
                                sum += Alt(x + dx, y + dy);
                                cnt++;
                            }
                        }
                    }
                    int avg = cnt == 0 ? Alt(x, y) : (int)Math.Round(sum / (double)cnt);
                    ushort cur = before[y * _map.Width + x];
                    if (cur >= _rules.Palette.Count || Math.Abs(_rules.Palette[cur].Altitude - avg) <= 10)
                    {
                        continue;
                    }
                    int g = _rules.Palette[cur].Group;
                    if (!_groupEntries.TryGetValue(g, out List<DragonPaletteEntry> entries))
                    {
                        _groupEntries[g] = entries = _rules.EntriesOfGroup(g).Where(e => e.Defined).ToList();
                    }
                    if (entries.Count == 0)
                    {
                        continue;
                    }
                    _map.Pixels[y * _map.Width + x] = (ushort)entries.OrderBy(e => Math.Abs(e.Altitude - avg)).First().Index;
                    n++;
                }
            }
            _strokePainted.Clear();
            if (n > 0)
            {
                mapCanvas.InvalidateMap();
                SetStatus($"Nachbesserung: {n} Höhen-Spitzen am Strich angeglichen.");
            }
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

        /// <summary>
        /// Map editor tool bar: "Live-CGI" for every tool (brush, fill, line, heights …) and "folgt Maus": the small CGI window
        /// shows the surroundings of the mouse built – while drawing and, with "folgt Maus", also when only moving over the map.
        /// </summary>
        private void AddLiveButtons()
        {
            _liveButton = new ToolStripButton(L.T("◉ Live-CGI"))
            {
                CheckOnClick = true,
                ToolTipText = L.T("Beim Zeichnen mit jedem Werkzeug zeigt ein kleines CGI-Fenster die Umgebung der Maus gebaut"),
                BackColor = Color.FromArgb(180, 83, 9),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            _liveFollowButton = new ToolStripButton(L.T("folgt Maus"))
            {
                CheckOnClick = true,
                ToolTipText = L.T("Live-CGI auch beim bloßen Bewegen der Maus über die Karte aktualisieren (neu gebaut, sobald die Maus ein Stück weiter ist)")
            };
            _liveButton.CheckedChanged += (s, e) =>
            {
                _liveAlways = _liveButton.Checked;
                _liveFollowButton.Enabled = _liveAlways;
                HookLiveEvents();
                if (_liveAlways)
                {
                    _liveCenter = new Point(-10000, -10000);
                    MarkLiveDirty();
                }
            };
            _liveFollowButton.CheckedChanged += (s, e) => _liveFollow = _liveFollowButton.Checked;
            _liveFollowButton.Enabled = false;
            toolStripEditor.Items.Add(_liveButton);
            toolStripEditor.Items.Add(_liveFollowButton);
        }

        private void HookLiveEvents()
        {
            if (_liveHooked)
            {
                return;
            }
            _liveHooked = true;
            mapCanvas.MapEdited += (o, e) => MarkLiveDirty();
            mapCanvas.Painting += (o, e) => MarkLiveDirty();
            mapCanvas.CursorMoved += (o, e) =>
            {
                if (_liveAlways && _liveFollow && (Math.Abs(e.X - _liveCenter.X) > 16 || Math.Abs(e.Y - _liveCenter.Y) > 16))
                {
                    MarkLiveDirty();
                }
            };
        }

        /// <summary>The live window was closed by the user: switch live off.</summary>
        private void LiveWindowClosed()
        {
            if (_liveButton != null && _liveButton.Checked)
            {
                _liveButton.Checked = false;
            }
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
            bool stampLive = _liveEnabled && (mapCanvas.Tool == MapTool.PatternBrush || mapCanvas.PasteActive);
            if (_map == null || !(stampLive || _liveAlways))
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
            int size = mapCanvas.Tool == MapTool.PatternBrush || mapCanvas.PasteActive
                ? Math.Clamp(Math.Max(_patternStamp?.Width ?? 32, _patternStamp?.Height ?? 32) * 2, 48, 96)
                : Math.Clamp(Math.Max(mapCanvas.BrushSize * 3, 64), 64, 128);
            _liveCenter = c;
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
