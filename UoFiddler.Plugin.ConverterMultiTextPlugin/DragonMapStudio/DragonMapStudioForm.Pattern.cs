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
        private Point? _livePin;
        private ToolStripDropDownButton _liveJumpButton;

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
                ApplyPendingHeights();
                MarkLiveDirty();
            };
            mapCanvas.PasteApplied += (o, e) => ApplyPendingHeights();
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
            int colour = StampPixelForHeight(s, i, current, out int want);
            NoteHeight(x, y, colour, want);
            return colour;
        }

        /// <summary>Heights the colours cannot hold, collected while painting; written into the height layer in the same undo step.</summary>
        private readonly Dictionary<(int x, int y), (sbyte z, ushort colour)?> _pendingHeights = new Dictionary<(int, int), (sbyte, ushort)?>();

        /// <summary>
        /// The colour got the closest height of its group – if that is not the wanted height (e.g. wood floor only Z 0, ground Z 5),
        /// the wanted height goes into the height layer; otherwise an old entry of the cell is removed.
        /// </summary>
        private void NoteHeight(int x, int y, int colour, int want)
        {
            if (colour < 0 || colour >= _rules.Palette.Count)
            {
                return;
            }
            int have = _rules.Palette[colour].Altitude;
            _pendingHeights[(x, y)] = Math.Abs(have - want) > 1 ? ((sbyte)Math.Clamp(want, -128, 127), (ushort)colour) : null;
        }

        /// <summary>Writes the collected heights into the height layer (undo / redo with the pixels).</summary>
        private void ApplyPendingHeights()
        {
            if (_pendingHeights.Count == 0)
            {
                return;
            }
            var before = new Dictionary<(int, int), (sbyte, ushort)?>();
            var after = new Dictionary<(int, int), (sbyte, ushort)?>(_pendingHeights);
            foreach ((int, int) k in _pendingHeights.Keys)
            {
                before[k] = _staticsLayer.Heights.TryGetValue(k, out (sbyte z, ushort colour) v) ? v : null;
            }
            _pendingHeights.Clear();
            void Set(Dictionary<(int, int), (sbyte, ushort)?> values)
            {
                foreach (KeyValuePair<(int, int), (sbyte, ushort)?> kv in values)
                {
                    if (kv.Value.HasValue)
                    {
                        _staticsLayer.Heights[kv.Key] = kv.Value.Value;
                    }
                    else
                    {
                        _staticsLayer.Heights.Remove(kv.Key);
                    }
                }
                _staticsLayer.Modified = true;
                if (_map != null)
                {
                    _map.Modified = true;
                }
            }
            Set(after);
            _history.AddExtra(() => Set(before), () => Set(after));
        }

        private readonly Dictionary<int, List<DragonPaletteEntry>> _groupEntries = new Dictionary<int, List<DragonPaletteEntry>>();

        /// <summary>
        /// Stamp pixel i on a cell whose colour is <paramref name="current"/>: the colour of its group at the wanted height –
        /// Original = height there + height in the stamp, Terrain = height there, Fixed = the fixed height; + offset.
        /// </summary>
        private int StampPixelForHeight(ColorStamp s, int i, ushort current) => StampPixelForHeight(s, i, current, out _);

        private int StampPixelForHeight(ColorStamp s, int i, ushort current, out int want)
        {
            ushort c = s.Pixels[i];
            want = c < _rules.Palette.Count ? _rules.Palette[c].Altitude : 0;
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
            want = _settings.PatternHeight switch
            {
                PatternHeightMode.Original => here + (i < s.RelativeAltitudes.Length ? s.RelativeAltitudes[i] : 0),
                PatternHeightMode.Fixed => _settings.PatternHeightValue,
                _ => here
            } + _settings.PatternHeightOffset;
            if (!_groupEntries.TryGetValue(g, out List<DragonPaletteEntry> entries))
            {
                _groupEntries[g] = entries = _rules.EntriesOfGroup(g).Where(e => e.Defined).ToList();
            }
            int target = want;
            return entries.Count == 0 ? c : entries.OrderBy(e => Math.Abs(e.Altitude - target)).ThenBy(e => e.Index == c ? 0 : 1).First().Index;
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
            int Alt(int x, int y)
            {
                ushort c = before[y * _map.Width + x];
                // the height layer counts: a floor of Z 0 painted on Z 5 stands at Z 5
                if (_staticsLayer.Heights.TryGetValue((x, y), out (sbyte z, ushort colour) h) && h.colour == c)
                {
                    return h.z;
                }
                return c < _rules.Palette.Count ? _rules.Palette[c].Altitude : 0;
            }
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
            _liveFollowButton.CheckedChanged += (s, e) =>
            {
                _liveFollow = _liveFollowButton.Checked;
                if (_liveFollow)
                {
                    _livePin = null;
                }
            };
            _liveFollowButton.Enabled = false;
            toolStripEditor.Items.Add(_liveButton);
            toolStripEditor.Items.Add(_liveFollowButton);
            AddLiveJumpButton();
        }

        // ------------------------------------------------------------------ jump the live CGI with a free mouse button

        /// <summary>Mouse buttons the map editor does not use: side buttons, middle button only with Strg / Alt (alone it pans).</summary>
        private static bool JumpButtonFree(MouseButtons b, Keys mods) =>
            b == MouseButtons.XButton1 || b == MouseButtons.XButton2 ||
            (b == MouseButtons.Middle && (mods == Keys.Control || mods == Keys.Alt));

        private static string JumpButtonName(MouseButtons b, Keys mods)
        {
            string n = b switch
            {
                MouseButtons.XButton1 => L.T("Maustaste 4 (Seite hinten)"),
                MouseButtons.XButton2 => L.T("Maustaste 5 (Seite vorne)"),
                MouseButtons.Middle => L.T("Mittlere Maustaste"),
                MouseButtons.Left => L.T("Linke Maustaste"),
                MouseButtons.Right => L.T("Rechte Maustaste"),
                _ => b.ToString()
            };
            if ((mods & Keys.Control) != 0)
            {
                n = L.T("Strg") + " + " + n;
            }
            if ((mods & Keys.Alt) != 0)
            {
                n = "Alt + " + n;
            }
            if ((mods & Keys.Shift) != 0)
            {
                n = L.T("Umschalt") + " + " + n;
            }
            return n;
        }

        private void AddLiveJumpButton()
        {
            _liveJumpButton = new ToolStripDropDownButton(L.T("⌖ Sprung"))
            {
                ToolTipText = L.T("Mit einer freien Maustaste auf eine Stelle der Karte klicken: das Live-CGI springt dorthin und baut die Stelle")
            };
            var assign = new ToolStripMenuItem(L.T("Maustaste belegen …"));
            assign.Click += LiveJumpAssign_Click;
            var clear = new ToolStripMenuItem(L.T("Belegung löschen"));
            clear.Click += LiveJumpClear_Click;
            _liveJumpButton.DropDownItems.Add(assign);
            _liveJumpButton.DropDownItems.Add(clear);
            toolStripEditor.Items.Add(_liveJumpButton);

            if (Enum.TryParse(_settings.LiveJumpButton ?? string.Empty, out MouseButtons b) && JumpButtonFree(b, (Keys)_settings.LiveJumpModifiers))
            {
                mapCanvas.JumpButton = b;
                mapCanvas.JumpModifiers = (Keys)_settings.LiveJumpModifiers;
            }
            mapCanvas.ButtonCaptured += MapCanvas_JumpButtonCaptured;
            mapCanvas.JumpClicked += MapCanvas_JumpClicked;
            UpdateLiveJumpText();
        }

        private void UpdateLiveJumpText()
        {
            bool on = mapCanvas.JumpButton != MouseButtons.None;
            _liveJumpButton.Text = on ? L.T("⌖ Sprung") + ": " + JumpButtonName(mapCanvas.JumpButton, mapCanvas.JumpModifiers) : L.T("⌖ Sprung (keine Taste)");
        }

        private void LiveJumpAssign_Click(object sender, EventArgs e)
        {
            mapCanvas.CaptureNextButton = true;
            SetStatus(L.T("Sprungtaste: jetzt mit der gewünschten Maustaste in die Karte klicken (frei: Maustaste 4 / 5, Strg oder Alt + Mitteltaste)."));
        }

        private void LiveJumpClear_Click(object sender, EventArgs e)
        {
            mapCanvas.JumpButton = MouseButtons.None;
            mapCanvas.JumpModifiers = Keys.None;
            _settings.LiveJumpButton = string.Empty;
            _settings.LiveJumpModifiers = 0;
            _settings.Save();
            UpdateLiveJumpText();
            SetStatus(L.T("Sprungtaste gelöscht."));
        }

        private void MapCanvas_JumpButtonCaptured(object sender, MapClickEventArgs e)
        {
            if (!JumpButtonFree(e.Button, e.Modifiers))
            {
                MessageBox.Show(this,
                    JumpButtonName(e.Button, e.Modifiers) + L.T(" ist schon belegt (Zeichnen, Kontextmenü oder Verschieben).") + "\n\n" +
                    L.T("Frei sind: Maustaste 4 und 5 (Seitentasten), Strg + Mitteltaste, Alt + Mitteltaste."),
                    L.T("Sprungtaste"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetStatus(L.T("Sprungtaste nicht geändert."));
                return;
            }
            mapCanvas.JumpButton = e.Button;
            mapCanvas.JumpModifiers = e.Modifiers;
            _settings.LiveJumpButton = e.Button.ToString();
            _settings.LiveJumpModifiers = (int)e.Modifiers;
            _settings.Save();
            UpdateLiveJumpText();
            SetStatus(L.T("Sprungtaste") + ": " + JumpButtonName(e.Button, e.Modifiers));
        }

        /// <summary>Jump: the live CGI shows the clicked cell (turned on if needed) and stays there until you draw again.</summary>
        private void MapCanvas_JumpClicked(object sender, MapClickEventArgs e)
        {
            if (_liveFollowButton.Checked)
            {
                _liveFollowButton.Checked = false; // otherwise the next mouse move would take it away again
            }
            _livePin = new Point(e.X, e.Y);
            if (!_liveButton.Checked)
            {
                _liveButton.Checked = true; // marks dirty itself
            }
            _liveAlways = true;
            MarkLiveDirty();
            SetStatus(L.T("Live-CGI springt zu") + $" {e.X}, {e.Y}");
        }

        private void HookLiveEvents()
        {
            if (_liveHooked)
            {
                return;
            }
            _liveHooked = true;
            mapCanvas.MapEdited += (o, e) => MarkLiveDirty();
            mapCanvas.Painting += (o, e) =>
            {
                _livePin = null; // drawing again: the live CGI follows the pen
                MarkLiveDirty();
            };
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
            Point c = _livePin ?? mapCanvas.CursorTile;
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
            o.ExtraHeights = _staticsLayer.CropHeights(r);
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
