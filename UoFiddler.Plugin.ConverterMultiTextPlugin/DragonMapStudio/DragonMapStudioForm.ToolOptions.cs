/***************************************************************************
 *
 *  * Dragon Map Studio – tool options (Werkstatt 27)
 *  *
 *  *  - CGI side panel (items, multis, land …): hide / show, own window (with undo / redo), tool palette hide / show
 *  *  - every CGI tool shows its most important values (height, depth, width …) directly in the options bar, ⚙ = all
 *  *  - hue: colour strip, name, picker with all hues, remembered favourites (Färben, Hue-Pinsel)
 *  *  - eraser: remember the ids of the selected statics; multi: picture right away, catalog, clear selection
 *  *  - roads: saved roads (settings + stamp); variants: own tile sets; wall: thickness, point to point
 *  *  - map editor: kinds of the natural course (river, mountain ridge, road, own colours, stamp)
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private readonly List<ToolStripItem> _quickItems = new List<ToolStripItem>();
        private Action _quickChanged;
        private Form _sideWindow;
        private bool _toolOptionsBuilt;
        private ToolStripComboBox _edCourseKind;
        private ArtCatalog _multiCatalog;

        private void BuildToolOptions()
        {
            if (_toolOptionsBuilt)
            {
                return;
            }
            _toolOptionsBuilt = true;

            // ---- layout switches (top bar of the CGI view, right side)
            ToolStripButton Switch(string text, string tip, Action click)
            {
                var b = new ToolStripButton(L.T(text)) { ToolTipText = L.T(tip), Alignment = ToolStripItemAlignment.Right };
                b.Click += (s, e) => click();
                toolStripCgi.Items.Add(b);
                return b;
            }
            Switch("⧉ Seite lösen", "Die Seitenleiste (Items, Multis, Land, Pinsel …) in ein eigenes Fenster – frei verschieben, mit Rückgängig / Wiederholen. Schließen = wieder andocken.", UndockSidePanel);
            Switch("◨ Seite", "Seitenleiste rechts ein- / ausblenden (F8)", ToggleSidePanel);
            Switch("◧ Werkzeuge", "Werkzeug-Palette links ein- / ausblenden (F7)", ToggleCgiPalette);
            var edPal = new ToolStripButton(L.T("◧ Werkzeuge")) { ToolTipText = L.T("Werkzeug-Palette links ein- / ausblenden (F7)"), Alignment = ToolStripItemAlignment.Right };
            edPal.Click += (s, e) => ToggleEditorPalette();
            toolStripEditor.Items.Add(edPal);

            // ---- the stamp of a road
            isoView.FindStampPattern = file =>
            {
                ColorStamp st = _stamps.FirstOrDefault(s => s.File == file);
                if (st == null)
                {
                    return null;
                }
                TilePattern p = TilePattern.FromStamp(st);
                p.Freehand = true;
                p.Land = p.HasLand;
                p.StaticsOn = true;
                p.Transitions = true;
                p.HeightMode = _settings.PatternHeight;
                p.HeightValue = _settings.PatternHeightValue;
                p.HeightOffset = _settings.PatternHeightOffset;
                return p;
            };

            // ---- multi tab: picture right away, catalog, clear
            numCgiMultiId.KeyUp += (s, e) => _ = numCgiMultiId.Value; // commits typed text → ValueChanged
            var clear = new Button { Text = L.T("✖ Auswahl löschen"), Location = new Point(190, 3), Size = new Size(100, 25), FlatStyle = FlatStyle.System };
            clear.Click += (s, e) => ClearMultiChoice();
            panelCgiMultiTop.Controls.Add(clear);
            tabCgiMulti.Controls.Remove(picCgiMulti);
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 180 };
            split.Panel1.Controls.Add(picCgiMulti);
            _multiCatalog = new ArtCatalog();
            _multiCatalog.Picked += (s, e) =>
            {
                if (e.Kind == ArtKind.Multi)
                {
                    numCgiMultiId.Value = Math.Clamp(e.Id, 0, (int)numCgiMultiId.Maximum);
                    NumCgiMultiId_ValueChanged(numCgiMultiId, EventArgs.Empty);
                    SelectCgiTool(IsoTool.Multi);
                }
            };
            split.Panel2.Controls.Add(_multiCatalog);
            tabCgiMulti.Controls.Add(split);
            split.BringToFront();
            tabCgiMulti.Enter += (s, e) =>
            {
                if (_multiCatalog.Kind != ArtKind.Multi)
                {
                    _multiCatalog.Kind = ArtKind.Multi;
                }
                NumCgiMultiId_ValueChanged(numCgiMultiId, EventArgs.Empty);
            };

            // ---- map editor: kind of the natural course
            BuildCourseKinds();
        }

        // ================================================================== side panel, palettes

        private void ToggleSidePanel()
        {
            if (_sideWindow != null)
            {
                _sideWindow.Visible = !_sideWindow.Visible;
                return;
            }
            splitCgi.Panel2Collapsed = !splitCgi.Panel2Collapsed;
        }

        private void ToggleCgiPalette()
        {
            if (_cgiPalette != null)
            {
                _cgiPalette.Visible = !_cgiPalette.Visible;
            }
        }

        private void ToggleEditorPalette()
        {
            if (_editorPalette != null)
            {
                _editorPalette.Visible = !_editorPalette.Visible;
            }
        }

        /// <summary>The whole side panel of the CGI view in its own window; closing it docks it back.</summary>
        private void UndockSidePanel()
        {
            if (_sideWindow != null)
            {
                _sideWindow.Close();
                return;
            }
            Control host = splitCgi.Panel2;
            List<Control> controls = host.Controls.Cast<Control>().ToList();
            Point screen = host.PointToScreen(Point.Empty);
            var f = new Form
            {
                Text = L.T("Seitenleiste") + " – Dragon Map Studio",
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Owner = this,
                Bounds = new Rectangle(Math.Max(0, screen.X - 40), Math.Max(0, screen.Y), Math.Max(340, host.Width + 16), Math.Max(400, host.Height + 60))
            };
            var panel = new Panel { Dock = DockStyle.Fill };
            var strip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            var undo = new ToolStripButton("↶ " + L.T("Rückgängig")) { ToolTipText = "Strg+Z" };
            undo.Click += (s, e) => isoView.Undo();
            var redo = new ToolStripButton("↷ " + L.T("Wiederholen")) { ToolTipText = "Strg+Y" };
            redo.Click += (s, e) => isoView.Redo();
            var dock = new ToolStripButton("⇥ " + L.T("Wieder andocken")) { Alignment = ToolStripItemAlignment.Right };
            dock.Click += (s, e) => f.Close();
            strip.Items.AddRange(new ToolStripItem[] { undo, redo, dock });
            foreach (Control c in controls)
            {
                host.Controls.Remove(c);
                panel.Controls.Add(c);
            }
            f.Controls.Add(panel);
            f.Controls.Add(strip);
            f.KeyPreview = true;
            f.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.Z)
                {
                    isoView.Undo();
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.Y)
                {
                    isoView.Redo();
                    e.Handled = true;
                }
            };
            f.FormClosed += (s, e) =>
            {
                foreach (Control c in controls)
                {
                    panel.Controls.Remove(c);
                    host.Controls.Add(c);
                }
                splitCgi.Panel2Collapsed = false;
                _sideWindow = null;
            };
            splitCgi.Panel2Collapsed = true;
            _sideWindow = f;
            f.Show(this);
        }

        // ================================================================== quick options of the CGI tools

        /// <summary>The most important values of the tool right in the options bar (plus ⚙ for all settings).</summary>
        private void UpdateQuickOptions(IsoTool t)
        {
            if (!_toolOptionsBuilt)
            {
                return;
            }
            toolStripCgiLand.SuspendLayout();
            foreach (ToolStripItem i in _quickItems)
            {
                toolStripCgiLand.Items.Remove(i);
                i.Dispose();
            }
            _quickItems.Clear();
            _quickChanged = null;
            CgiBrushSettings b = BrushCfg;
            var all = new List<(object o, string title)>();
            switch (t)
            {
                case IsoTool.Coast:
                    Quick(isoView.Coast, "SurfaceZ", "FloorZ", "WaterWidth", "MaxLandZ", "SetFloor");
                    all.Add((isoView.Coast, L.T("Küste")));
                    break;
                case IsoTool.CoastSmooth:
                    Quick(isoView.Coast, "SurfaceZ", "FloorZ", "WaterWidth");
                    Quick(isoView.CoastSmooth, "DropWidth", "BlurPasses", "MoveStatics");
                    all.Add((isoView.CoastSmooth, L.T("Küste glätten")));
                    all.Add((isoView.Coast, L.T("Küste")));
                    break;
                case IsoTool.River:
                    Quick(b.River, "Depth", "BankWidth", "BankStep", "BedDepth", "WaterStatics", "FollowTerrain");
                    all.Add((b.River, L.T("Fluss")));
                    break;
                case IsoTool.Course:
                    Quick(b.Course, "Width", "EndWidth", "Meander");
                    Quick(b.River, "Depth", "BankWidth", "BedDepth", "WaterStatics");
                    all.Add((b.Course, L.T("Fluss-Lauf")));
                    all.Add((b.River, L.T("Fluss")));
                    break;
                case IsoTool.Road:
                    RoadPresetItems();
                    Quick(b.Road, "Width", "Shoulder", "Smoothing", "FollowTerrain");
                    all.Add((b.Road, L.T("Weg / Straße")));
                    break;
                case IsoTool.Ramp:
                    Quick(b.Ramp, "Width", "Shoulder");
                    all.Add((b.Ramp, L.T("Rampe")));
                    break;
                case IsoTool.Plateau:
                    Quick(b.Plateau, "Mode", "TerraceStep", "UseTargetHeight");
                    all.Add((b.Plateau, L.T("Plateau")));
                    break;
                case IsoTool.Noise:
                    Quick(b.Noise, "Pattern", "Amplitude", "Scale", "Detail", "Angle");
                    {
                        // small picture of the pattern (light = higher)
                        var preview = new ToolStripLabel { AutoSize = false, Width = 52, Height = 26, ImageScaling = ToolStripItemImageScaling.None, ToolTipText = L.T("Vorschau des Musters (hell = höher, dunkel = tiefer)") };
                        void Draw()
                        {
                            var bmp = new Bitmap(48, 24);
                            for (int y = 0; y < 24; y++)
                            {
                                for (int x = 0; x < 48; x++)
                                {
                                    double v = CgiBrushes.NoiseValue(x, y, b.Noise);
                                    if (b.Noise.Pattern == NoisePattern.Steps)
                                    {
                                        int st = Math.Max(1, b.Noise.StepHeight);
                                        v = Math.Round(v * b.Noise.Amplitude / st) * st / Math.Max(1.0, b.Noise.Amplitude);
                                    }
                                    int g = Math.Clamp((int)(128 + v * 110), 0, 255);
                                    bmp.SetPixel(x, y, Color.FromArgb(g, (int)(g * 0.85), (int)(g * 0.6)));
                                }
                            }
                            preview.Image?.Dispose();
                            preview.Image = bmp;
                        }
                        Draw();
                        _quickChanged = Draw;
                        AddQuick(preview);
                    }
                    all.Add((b.Noise, L.T("Unebenheit")));
                    break;
                case IsoTool.Variants:
                    VariantItems();
                    break;
                case IsoTool.Wall:
                    WallItems();
                    Quick(b.Wall, "Thickness", "Levels", "PointMode", "Level", "ZOffset");
                    all.Add((b.Wall, L.T("Wand")));
                    break;
                case IsoTool.Scatter:
                    Quick(b.Scatter, "Density", "MinDistance", "ZOffset");
                    all.Add((b.Scatter, L.T("Streuen")));
                    break;
                case IsoTool.Hue:
                case IsoTool.HueBrush:
                    HueItems();
                    break;
                case IsoTool.Eraser:
                    Quick(b.Eraser, "Statics", "Land", "OnlyVisible", "OnlyIds");
                    Cmd("Auswahl merken", "Die IDs der gewählten Statics merken: der Radierer löscht dann nur diese", () =>
                    {
                        if (isoView.Selection.Count == 0)
                        {
                            SetStatus(L.T("Erst mit 'Auswahl' Statics wählen (Strg = mehrere), dann 'Auswahl merken'."));
                            return;
                        }
                        b.Eraser.OnlyIds = string.Join(" ", isoView.Selection.Select(s => s.Id).Distinct().Select(i => i.ToString("x4")));
                        UpdateQuickOptions(isoView.Tool);
                        SetStatus(L.T("Radierer löscht nur noch") + ": " + b.Eraser.OnlyIds);
                    });
                    Cmd("alle", "Wieder alle Statics löschen", () =>
                    {
                        b.Eraser.OnlyIds = string.Empty;
                        UpdateQuickOptions(isoView.Tool);
                    });
                    all.Add((b.Eraser, L.T("Radierer")));
                    break;
                case IsoTool.Delete:
                    Cmd("→ Radierer (mit Pinselgröße)", "Löschen mit Pinsel: alle Statics unter dem Pinsel, auf Wunsch nur bestimmte IDs", () => SelectCgiTool(IsoTool.Eraser));
                    break;
                case IsoTool.Multi:
                    Cmd("✖ Multi-Auswahl löschen", "Keine Multi mehr am Mauszeiger", ClearMultiChoice);
                    break;
                case IsoTool.Mountain:
                    Quick(isoView.Mountain, "PeakZ", "Steepness", "HillSize", "Roughness");
                    all.Add((isoView.Mountain, L.T("Berg")));
                    break;
                case IsoTool.Bridge:
                    Quick(b.Bridge, "Width", "Arch", "ZOffset", "Stairs");
                    all.Add((b.Bridge, L.T("Brücke")));
                    break;
                case IsoTool.Stairs:
                    {
                        var st = new StairToolOptions(b);
                        Quick(st, "Direction", "Steps", "Width");
                        break;
                    }
            }
            if (all.Count > 0)
            {
                var gear = new ToolStripDropDownButton("⚙") { ToolTipText = L.T("Alle Einstellungen dieses Werkzeugs") };
                foreach ((object o, string title) in all)
                {
                    object captured = o;
                    gear.DropDownItems.Add(title + " …", null, (s, e) => ShowSettingsWindow(captured, title));
                }
                AddQuick(gear);
            }
            toolStripCgiLand.ResumeLayout();
        }

        private void AddQuick(ToolStripItem item)
        {
            _quickItems.Add(item);
            toolStripCgiLand.Items.Add(item);
        }

        private void Cmd(string text, string tip, Action click)
        {
            var b = new ToolStripButton(L.T(text)) { ToolTipText = L.T(tip) };
            b.Click += (s, e) =>
            {
                click();
                isoView.Focus();
            };
            AddQuick(b);
        }

        /// <summary>Fields of a settings object as small inputs (numbers, switches, choices, text).</summary>
        private void Quick(object o, params string[] props)
        {
            if (o == null)
            {
                return;
            }
            AddQuick(new ToolStripSeparator());
            foreach (string name in props)
            {
                PropertyInfo p = o.GetType().GetProperty(name);
                if (p == null || !p.CanWrite)
                {
                    continue;
                }
                string label = L.T(p.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? name);
                string tip = L.T(p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty);
                Type type = p.PropertyType;
                if (type == typeof(bool))
                {
                    var c = new ToolStripButton(label) { CheckOnClick = true, Checked = (bool)p.GetValue(o), ToolTipText = tip };
                    c.CheckedChanged += (s, e) =>
                    {
                        p.SetValue(o, c.Checked);
                        _quickChanged?.Invoke();
                    };
                    AddQuick(c);
                    continue;
                }
                AddQuick(new ToolStripLabel(label + ":") { ToolTipText = tip });
                if (type == typeof(int) || type == typeof(double))
                {
                    var n = new NumericUpDown
                    {
                        Minimum = -128, Maximum = 999, Width = type == typeof(double) ? 58 : 52, DecimalPlaces = type == typeof(double) ? 1 : 0,
                        Increment = type == typeof(double) ? 0.5m : 1
                    };
                    n.Value = Math.Clamp(Convert.ToDecimal(p.GetValue(o)), n.Minimum, n.Maximum);
                    n.ValueChanged += (s, e) =>
                    {
                        p.SetValue(o, type == typeof(int) ? (object)(int)n.Value : (double)n.Value);
                        _quickChanged?.Invoke();
                    };
                    AddQuick(new ToolStripControlHost(n) { ToolTipText = tip });
                }
                else if (type.IsEnum)
                {
                    var cb = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, ToolTipText = tip, AutoSize = false, Width = 150 };
                    Array values = Enum.GetValues(type);
                    foreach (object v in values)
                    {
                        string text = type.GetField(v.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? v.ToString();
                        cb.Items.Add(L.T(text));
                    }
                    cb.SelectedIndex = Array.IndexOf(values, p.GetValue(o));
                    cb.SelectedIndexChanged += (s, e) =>
                    {
                        p.SetValue(o, values.GetValue(Math.Max(0, cb.SelectedIndex)));
                        _quickChanged?.Invoke();
                    };
                    AddQuick(cb);
                }
                else if (type == typeof(string))
                {
                    var tb = new ToolStripTextBox { Text = (string)p.GetValue(o) ?? string.Empty, ToolTipText = tip, AutoSize = false, Width = 110 };
                    tb.TextChanged += (s, e) => p.SetValue(o, tb.Text);
                    AddQuick(tb);
                }
            }
        }

        // ================================================================== hue

        private void HueItems()
        {
            AddQuick(new ToolStripSeparator());
            ushort hue = isoView.PlaceHue;
            Color c = UoArtCache.HueColor(hue);
            var swatch = new ToolStripLabel("      ") { AutoSize = false, Width = 40, BackColor = c == Color.Transparent ? SystemColors.Control : c, ToolTipText = L.T("Farbe der Hue") };
            AddQuick(new ToolStripLabel("Hue:"));
            var n = new NumericUpDown { Minimum = 0, Maximum = numCgiHue.Maximum, Width = 60, Value = Math.Clamp(hue, 0, (int)numCgiHue.Maximum) };
            n.ValueChanged += (s, e) => SetHue((ushort)n.Value, false);
            AddQuick(new ToolStripControlHost(n));
            AddQuick(swatch);
            AddQuick(new ToolStripLabel(hue == 0 ? L.T("Original") : UoArtCache.HueName(hue)) { ForeColor = Color.FromArgb(71, 85, 105) });
            Cmd("🎨 Wählen …", "Alle Hues mit Farbverlauf, Suche nach Nummer oder Name, Favoriten", () =>
            {
                using var dlg = new HuePickerForm(isoView.PlaceHue, BrushCfg.FavoriteHues);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    SetHue(dlg.Hue, true);
                }
            });
            bool fav = BrushCfg.FavoriteHues.Contains(hue);
            Cmd(fav ? "★ vergessen" : "☆ merken", "Diese Hue als Favorit merken", () =>
            {
                if (!BrushCfg.FavoriteHues.Remove(isoView.PlaceHue) && isoView.PlaceHue != 0)
                {
                    BrushCfg.FavoriteHues.Add(isoView.PlaceHue);
                }
                UpdateQuickOptions(isoView.Tool);
            });
            var favs = new ToolStripDropDownButton("★ " + L.T("Favoriten")) { ToolTipText = L.T("Gemerkte Hues") };
            if (BrushCfg.FavoriteHues.Count == 0)
            {
                favs.DropDownItems.Add(new ToolStripMenuItem(L.T("Noch keine – eine Hue wählen und '☆ merken'")) { Enabled = false });
            }
            foreach (ushort h in BrushCfg.FavoriteHues)
            {
                ushort captured = h;
                var bmp = new Bitmap(16, 16);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using var br = new SolidBrush(UoArtCache.HueColor(h));
                    g.FillRectangle(br, 0, 0, 16, 16);
                    g.DrawRectangle(Pens.Black, 0, 0, 15, 15);
                }
                favs.DropDownItems.Add($"{h}  {UoArtCache.HueName(h)}", bmp, (s, e) => SetHue(captured, true));
            }
            AddQuick(favs);
        }

        private void SetHue(ushort hue, bool rebuild)
        {
            numCgiHue.Value = Math.Clamp(hue, 0, (int)numCgiHue.Maximum);
            isoView.PlaceHue = hue;
            if (rebuild)
            {
                UpdateQuickOptions(isoView.Tool);
            }
            else
            {
                // only the swatch and the name
                foreach (ToolStripItem i in _quickItems)
                {
                    if (i is ToolStripLabel l && l.Text.Trim().Length == 0 && l.Width == 40)
                    {
                        Color c = UoArtCache.HueColor(hue);
                        l.BackColor = c == Color.Transparent ? SystemColors.Control : c;
                    }
                }
            }
            SetStatus("Hue " + hue + ": " + (hue == 0 ? L.T("Original") : UoArtCache.HueName(hue)));
        }

        // ================================================================== multi

        private void ClearMultiChoice()
        {
            isoView.MultiId = -1;
            picCgiMulti.Image = null;
            lblCgiMultiInfo.Text = L.T("Keine Multi gewählt – im Katalog unten eine wählen oder die ID eingeben.");
            if (isoView.Tool == IsoTool.Multi)
            {
                SelectCgiTool(IsoTool.Select);
            }
            isoView.Invalidate();
            SetStatus(L.T("Multi-Auswahl gelöscht."));
        }

        // ================================================================== road presets

        private void RoadPresetItems()
        {
            CgiBrushSettings b = BrushCfg;
            AddQuick(new ToolStripSeparator());
            var presets = new ToolStripDropDownButton(L.T("Weg-Vorlage") + (string.IsNullOrEmpty(b.RoadStamp) ? string.Empty : " ✎")) { ToolTipText = L.T("Gespeicherte Wege: Einstellungen + Stempel") };
            foreach (RoadPreset p in b.RoadPresets)
            {
                RoadPreset captured = p;
                presets.DropDownItems.Add(p.ToString(), null, (s, e) =>
                {
                    b.Road = Clone(captured.Settings);
                    b.RoadStamp = captured.Stamp ?? string.Empty;
                    isoView.BrushSettings = b;
                    UpdateQuickOptions(isoView.Tool);
                    SetStatus(L.T("Weg-Vorlage") + $" '{captured.Name}' – " + L.T("Punkte klicken, Doppelklick = bauen"));
                });
            }
            if (b.RoadPresets.Count > 0)
            {
                presets.DropDownItems.Add(new ToolStripSeparator());
            }
            presets.DropDownItems.Add(L.T("Jetzige Einstellungen als Vorlage speichern …"), null, (s, e) =>
            {
                string n = AskName("Name des Wegs:", "Pflasterweg");
                if (n == null)
                {
                    return;
                }
                b.RoadPresets.RemoveAll(r => r.Name == n);
                b.RoadPresets.Add(new RoadPreset { Name = n, Settings = Clone(b.Road), Stamp = b.RoadStamp });
                UpdateQuickOptions(isoView.Tool);
            });
            presets.DropDownItems.Add(L.T("Vorlagen bearbeiten …"), null, (s, e) =>
            {
                using var ed = new NamedListEditor(L.T("Weg-Vorlagen"), b.RoadPresets, () => new RoadPreset { Name = L.T("Neuer Weg") },
                    o => new RoadPreset { Name = ((RoadPreset)o).Name + " 2", Settings = Clone(((RoadPreset)o).Settings), Stamp = ((RoadPreset)o).Stamp },
                    L.T("Ein Weg = die Weg-Einstellungen (Breite, Tiles, Höhe) und auf Wunsch ein Stempel, der entlang der Kurve gemalt wird."));
                ed.ShowDialog(this);
                UpdateQuickOptions(isoView.Tool);
            });
            AddQuick(presets);
            // stamp along the road: tiles, a stamp (learned from the map) …
            var stamp = new ToolStripDropDownButton(string.IsNullOrEmpty(b.RoadStamp) ? L.T("aus Tiles") : "✎ " + System.IO.Path.GetFileNameWithoutExtension(b.RoadStamp).Replace(ColorStampTools.Extension, string.Empty))
            {
                ToolTipText = L.T("Womit der Weg gebaut wird: Weg-Tiles oder ein Stempel (Land + Statics, z. B. aus der Karte gelernt)")
            };
            stamp.DropDownItems.Add(L.T("Weg-Tiles (Einstellung 'Weg-Tiles' / Reiter Land)"), null, (s, e) =>
            {
                b.RoadStamp = string.Empty;
                UpdateQuickOptions(isoView.Tool);
            });
            foreach (IGrouping<string, ColorStamp> cat in _stamps.Where(s => s.CanLearn || (s.Statics?.Count ?? 0) > 0)
                         .GroupBy(s => string.IsNullOrWhiteSpace(s.Category) ? L.T("ohne Kategorie") : s.Category).OrderBy(g => g.Key))
            {
                var sub = new ToolStripMenuItem(cat.Key);
                foreach (ColorStamp st in cat.OrderBy(s => s.Name))
                {
                    ColorStamp captured = st;
                    sub.DropDownItems.Add(st.Name, null, (s, e) =>
                    {
                        b.RoadStamp = captured.File;
                        UpdateQuickOptions(isoView.Tool);
                        SetStatus(L.T("Weg mit Stempel") + $" '{captured.Name}' – " + L.T("Breite = Weg-Breite"));
                    });
                }
                stamp.DropDownItems.Add(sub);
            }
            stamp.DropDownItems.Add(new ToolStripSeparator());
            stamp.DropDownItems.Add(L.T("Neuen Stempel aus der Karte: Bereich markieren → '→ Stempel (P)'"), null, (s, e) => SelectCgiTool(IsoTool.Area));
            AddQuick(stamp);
        }

        private static T Clone<T>(T o) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(o));

        // ================================================================== variants

        private void VariantItems()
        {
            CgiBrushSettings b = BrushCfg;
            AddQuick(new ToolStripSeparator());
            AddQuick(new ToolStripLabel(L.T("Muster:")));
            var cb = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 170 };
            cb.Items.Add(L.T("(Familie der Farbtabelle)"));
            foreach (VariantSet v in b.VariantSets)
            {
                cb.Items.Add(v);
            }
            int i = b.VariantSets.FindIndex(v => v.Name == b.VariantSet);
            cb.SelectedIndex = i + 1;
            cb.SelectedIndexChanged += (s, e) => b.VariantSet = cb.SelectedItem is VariantSet v ? v.Name : string.Empty;
            AddQuick(cb);
            Cmd("Neu aus Bereich", "Die Land-Tiles des markierten Bereichs (so oft wie sie vorkommen) als neues Muster", () =>
            {
                Rectangle r = isoView.Area;
                if (r.IsEmpty || _cgiMap == null)
                {
                    SetStatus(L.T("Erst mit 'Bereich' ein Stück Boden markieren, das als Muster dienen soll."));
                    return;
                }
                string n = AskName("Name des Musters:", "Gras gemischt");
                if (n == null)
                {
                    return;
                }
                var ids = new List<ushort>();
                for (int y = r.Top; y < r.Bottom; y++)
                {
                    for (int x = r.Left; x < r.Right; x++)
                    {
                        ids.Add(_cgiMap.LandId(x, y));
                    }
                }
                // frequency kept, but at most 64 entries
                var groups = ids.GroupBy(v => v).OrderByDescending(g => g.Count()).ToList();
                int total = ids.Count;
                var list = new List<ushort>();
                foreach (IGrouping<ushort, ushort> g in groups)
                {
                    int times = Math.Max(1, (int)Math.Round(g.Count() * 40.0 / total));
                    list.AddRange(Enumerable.Repeat(g.Key, times));
                }
                b.VariantSets.RemoveAll(v => v.Name == n);
                b.VariantSets.Add(new VariantSet { Name = n, Tiles = string.Join(" ", list.Take(64).Select(v => v.ToString("x4"))) });
                b.VariantSet = n;
                UpdateQuickOptions(isoView.Tool);
                SetStatus(L.T("Muster") + $" '{n}': {groups.Count} " + L.T("verschiedene Tiles") + " – " + L.T("mit dem Varianten-Pinsel malen"));
            });
            Cmd("Muster bearbeiten …", "Eigene Muster: Tiles, nur auf bestimmten Tiles, Chance", () =>
            {
                using var ed = new NamedListEditor(L.T("Varianten-Muster"), b.VariantSets, () => new VariantSet { Name = L.T("Neues Muster"), Tiles = isoView.PlaceLandId.ToString("x4") },
                    o => new VariantSet { Name = ((VariantSet)o).Name + " 2", Tiles = ((VariantSet)o).Tiles, OnlyOn = ((VariantSet)o).OnlyOn, Chance = ((VariantSet)o).Chance },
                    L.T("Ein Muster = Land-Tiles, die der Pinsel zufällig setzt (ein Tile mehrfach = öfter). 'Nur auf' begrenzt es auf bestimmte Tiles, Chance < 100 = einzelne Tupfer."));
                ed.AddAction("+ Land-Tile aus Reiter Land", o => ((VariantSet)o).Tiles = (((VariantSet)o).Tiles + " " + isoView.PlaceLandId.ToString("x4")).Trim());
                ed.ShowDialog(this);
                UpdateQuickOptions(isoView.Tool);
            });
        }

        // ================================================================== wall

        private void WallItems()
        {
            CgiBrushSettings b = BrushCfg;
            AddQuick(new ToolStripSeparator());
            AddQuick(new ToolStripLabel(L.T("Set:")));
            var cb = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 140 };
            foreach (WallSet w in b.WallSets)
            {
                cb.Items.Add(w);
            }
            cb.SelectedIndex = b.WallSets.FindIndex(w => w.Name == b.WallSet);
            cb.SelectedIndexChanged += (s, e) =>
            {
                if (cb.SelectedItem is WallSet w)
                {
                    b.WallSet = w.Name;
                    ApplyWallSet();
                    FillWallCombos();
                }
            };
            AddQuick(cb);
            Cmd("Sets bearbeiten …", "Wand-Sets: Teile West-Ost, Nord-Süd, Pfosten (hex) – neu, kopieren, löschen", () =>
            {
                using var ed = new NamedListEditor(L.T("Wand-Sets"), b.WallSets, () => new WallSet { Name = L.T("Neue Wand") },
                    o => new WallSet { Name = ((WallSet)o).Name + " 2", AlongX = ((WallSet)o).AlongX, AlongY = ((WallSet)o).AlongY, Post = ((WallSet)o).Post },
                    L.T("Im Reiter Items ein Stück wählen und hier '+ …' klicken. Mehrere IDs = zufällig gemischt."));
                ed.AddAction("+ Item → West-Ost", o => ((WallSet)o).AlongX = (((WallSet)o).AlongX + " " + isoView.PlaceId.ToString("x4")).Trim());
                ed.AddAction("+ Item → Nord-Süd", o => ((WallSet)o).AlongY = (((WallSet)o).AlongY + " " + isoView.PlaceId.ToString("x4")).Trim());
                ed.AddAction("Item → Pfosten", o => ((WallSet)o).Post = isoView.PlaceId.ToString("x4"));
                ed.AddAction("Aus Auswahl (X / Y / Pfosten raten)", o =>
                {
                    var w = (WallSet)o;
                    foreach (ushort id in isoView.Selection.Select(s => s.Id).Distinct())
                    {
                        int dir = BridgeKit.Orientation(id);
                        if (dir > 0)
                        {
                            w.AlongX = (w.AlongX + " " + id.ToString("x4")).Trim();
                        }
                        else if (dir < 0)
                        {
                            w.AlongY = (w.AlongY + " " + id.ToString("x4")).Trim();
                        }
                        else
                        {
                            w.Post = id.ToString("x4");
                        }
                    }
                });
                ed.Select(o => ((WallSet)o).Name == b.WallSet);
                ed.ShowDialog(this);
                ApplyWallSet();
                FillWallCombos();
                UpdateQuickOptions(isoView.Tool);
            });
        }

        // ================================================================== map editor: kind of the natural course

        private void BuildCourseKinds()
        {
            _edCourseKind = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 130, ToolTipText = L.T("Was der Lauf malt: Fluss, Berg-Kamm, Weg, eigene Farben oder Stempel") };
            _edCourseKind.SelectedIndexChanged += (s, e) =>
            {
                if (_edCourseKind.SelectedItem is CoursePreset p)
                {
                    BrushCfg.CoursePreset = p.Name;
                    ApplyCourseKind(p);
                }
            };
            var edit = new ToolStripButton(L.T("Lauf-Arten …")) { ToolTipText = L.T("Lauf-Arten anlegen und ändern: Farben (Kern, Rand, Außen), Breiten, Schlängeln") };
            edit.Click += (s, e) => EditCourseKinds();
            var label = new ToolStripLabel(L.T("Art:"));
            int at = toolStripEditor.Items.IndexOf(_edCourseStamp);
            if (at < 0)
            {
                at = toolStripEditor.Items.Count;
            }
            foreach (ToolStripItem item in new ToolStripItem[] { label, _edCourseKind, edit })
            {
                toolStripEditor.Items.Insert(at++, item);
                _edToolItems.Add((item, t => t == MapTool.Course));
            }
            FillCourseKinds();
        }

        private void FillCourseKinds()
        {
            _edCourseKind.Items.Clear();
            foreach (CoursePreset p in BrushCfg.CoursePresets)
            {
                _edCourseKind.Items.Add(p);
            }
            int i = BrushCfg.CoursePresets.FindIndex(p => p.Name == BrushCfg.CoursePreset);
            if (_edCourseKind.Items.Count > 0)
            {
                _edCourseKind.SelectedIndex = Math.Max(0, i);
            }
        }

        /// <summary>Colours of the kind: found by name in the palette when they are not set yet.</summary>
        private void ResolveCourseColours(CoursePreset p)
        {
            if (p.Core >= 0 || _rules?.Palette == null || p.Kind == CourseKind.Own || p.Kind == CourseKind.Stamp)
            {
                return;
            }
            List<DragonPaletteEntry> defined = _rules.Palette.Where(e => e.Defined).ToList();
            DragonPaletteEntry Find(string[] words, bool high)
            {
                var hits = defined.Where(e => words.Any(w => (e.Name ?? string.Empty).ToLowerInvariant().Contains(w))).ToList();
                if (hits.Count == 0)
                {
                    return null;
                }
                return high ? hits.OrderByDescending(e => e.Altitude).First() : hits.OrderBy(e => e.Altitude).First();
            }
            switch (p.Kind)
            {
                case CourseKind.River:
                    {
                        DragonPaletteEntry w = Find(new[] { "water", "wasser", "river", "fluss" }, true);
                        DragonPaletteEntry sand = Find(new[] { "sand", "beach", "strand", "shore" }, false);
                        if (w != null)
                        {
                            p.Core = w.Index;
                        }
                        if (sand != null && p.Edge < 0)
                        {
                            p.Edge = sand.Index;
                        }
                        break;
                    }
                case CourseKind.Mountain:
                    {
                        DragonPaletteEntry top = Find(new[] { "mountain", "berg", "rock", "fels" }, true);
                        DragonPaletteEntry low = Find(new[] { "mountain", "berg", "rock", "fels" }, false);
                        if (top != null)
                        {
                            p.Core = top.Index;
                        }
                        if (low != null && low != top && p.Edge < 0)
                        {
                            p.Edge = low.Index;
                        }
                        break;
                    }
                case CourseKind.Road:
                    {
                        DragonPaletteEntry r = Find(new[] { "road", "weg", "cobble", "pflaster", "dirt", "erde", "path" }, false);
                        if (r != null)
                        {
                            p.Core = r.Index;
                        }
                        break;
                    }
            }
        }

        private void ApplyCourseKind(CoursePreset p)
        {
            ResolveCourseColours(p);
            CourseSettings c = p.ToCourse(BrushCfg.Course.Seed);
            BrushCfg.Course.Width = c.Width;
            BrushCfg.Course.EndWidth = c.EndWidth;
            BrushCfg.Course.Widen = false;
            BrushCfg.Course.Meander = c.Meander;
            BrushCfg.Course.Bends = c.Bends;
            mapCanvas.Course = BrushCfg.Course;
            bool stamp = p.Kind == CourseKind.Stamp;
            mapCanvas.CourseWithPattern = stamp;
            if (_edCourseStamp != null)
            {
                _edCourseStamp.Checked = stamp;
            }
            mapCanvas.CourseColours = stamp ? ((int, int, int, int, int)?)null : (p.Core, p.Edge, p.EdgeWidth, p.Edge2, p.Edge2Width);
            string Col(int i) => i < 0 ? L.T("Farbe 1") : i < _rules.Palette.Count ? $"{i} {_rules.Palette[i].Name}" : i.ToString();
            SetStatus(L.T("Natürlicher Lauf") + $" '{p.Name}': " + (stamp ? L.T("mit dem Muster-Stempel") : L.T("Kern") + " " + Col(p.Core) +
                      (p.Edge >= 0 ? ", " + L.T("Rand") + " " + Col(p.Edge) + $" ({p.EdgeWidth})" : string.Empty) +
                      (p.Edge2 >= 0 ? ", " + L.T("Außen") + " " + Col(p.Edge2) + $" ({p.Edge2Width})" : string.Empty)) +
                      $" · {p.Width}–{(p.EndWidth > 0 ? p.EndWidth : p.Width)} " + L.T("Tiles breit"));
        }

        private void EditCourseKinds()
        {
            List<CoursePreset> list = BrushCfg.CoursePresets;
            using var ed = new NamedListEditor(L.T("Lauf-Arten (natürlicher Lauf)"), list, () => new CoursePreset { Name = L.T("Neue Art"), Kind = CourseKind.Own },
                o => Clone((CoursePreset)o),
                L.T("Farben sind Nummern der Farbtabelle: links die Farbe wählen (Farbe 1) und hier 'Farbe 1 → …' klicken. Kern = Mitte, Rand = beidseitig daneben, Außen = noch weiter außen."));
            ed.AddAction("Farbe 1 → Kern", o => ((CoursePreset)o).Core = mapCanvas.PrimaryIndex);
            ed.AddAction("Farbe 1 → Rand", o => ((CoursePreset)o).Edge = mapCanvas.PrimaryIndex);
            ed.AddAction("Farbe 1 → Außen", o => ((CoursePreset)o).Edge2 = mapCanvas.PrimaryIndex);
            ed.AddAction("Rand / Außen weg", o =>
            {
                ((CoursePreset)o).Edge = -1;
                ((CoursePreset)o).Edge2 = -1;
            });
            ed.AddAction("Farben neu suchen", o =>
            {
                ((CoursePreset)o).Core = -1;
                ((CoursePreset)o).Edge = -1;
                ResolveCourseColours((CoursePreset)o);
            });
            ed.Select(o => ((CoursePreset)o).Name == BrushCfg.CoursePreset);
            ed.ShowDialog(this);
            if (ed.Selected is CoursePreset sel)
            {
                BrushCfg.CoursePreset = sel.Name;
            }
            FillCourseKinds();
        }
    }
}
