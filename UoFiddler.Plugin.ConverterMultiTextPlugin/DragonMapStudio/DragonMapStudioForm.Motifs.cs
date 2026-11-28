/***************************************************************************
 *
 *  * Dragon Map Studio – motifs ("Muster-Baukasten") in the studio: the tool "Muster" of the CGI view, the designer
 *  *  window, the chooser Kategorie ▸ Name ▸ Richtung and the tool "Muster" of the map editor (statics layer).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private MotifDesignerForm _motifDesigner;
        private List<Motif> _motifs;
        private ToolStripDropDownButton _edMotifChooser;

        private Motif FindMotif(string name)
        {
            _motifs ??= Motif.LoadAll();
            return _motifs.FirstOrDefault(m => m.ToString() == name);
        }

        private Motif CurrentMotif => string.IsNullOrEmpty(BrushCfg.Motif) ? null : FindMotif(BrushCfg.Motif);

        private string MotifLabel()
        {
            Motif m = CurrentMotif;
            return m == null ? L.T("Muster wählen …") : $"{m.Name} ({L.T(Motif.DirName(BrushCfg.MotifDraw.Direction))})";
        }

        /// <summary>Hooks the motif tool into the CGI view and the map editor (once, from BuildToolOptions).</summary>
        private void InitMotifs()
        {
            isoView.FindMotif = FindMotif;
            mapCanvas.MotifRequested += MapCanvas_MotifRequested;
            mapCanvas.MotifPathRequested += MapCanvas_MotifPathRequested;
            mapCanvas.MotifPointMode = () => BrushCfg.MotifDraw.Mode == MotifMode.Points;
            mapCanvas.MotifPathPreview = pts => CurrentMotif?.Path(pts, BrushCfg.MotifDraw, null, out _).Select(p => new Point(p.x, p.y)).Distinct().ToList();
            mapCanvas.MotifPreview = (a, b) =>
            {
                Motif m = CurrentMotif;
                if (m == null)
                {
                    return null;
                }
                return m.Layout(a, b, BrushCfg.MotifDraw, null, out _, out _).Select(p => new Point(p.x, p.y)).Distinct().ToList();
            };

            // map editor bar: chooser, settings, designer, remove the last one
            _edMotifChooser = MotifChooser(() => _edMotifChooser.Text = MotifLabel());
            var settings = new ToolStripButton(L.T("Zeichnen …")) { ToolTipText = L.T("Ziehen an / aus, Höhe, Etagen, Abstand, Ersetzen") };
            settings.Click += (s, e) => ShowSettingsWindow(BrushCfg.MotifDraw, L.T("Muster"));
            var designer = new ToolStripButton(L.T("Muster-Baukasten …"));
            designer.Click += (s, e) => OpenMotifDesigner();
            foreach (ToolStripItem item in new ToolStripItem[] { new ToolStripLabel(L.T("Muster:")), _edMotifChooser, settings, designer })
            {
                toolStripEditor.Items.Add(item);
                _edToolItems.Add((item, t => t == MapTool.Motif));
                item.Visible = mapCanvas.Tool == MapTool.Motif;
            }
        }

        /// <summary>Drop-down Kategorie ▸ Name ▸ Nord / Ost / Süd / West – chooses the motif and the direction of a click.</summary>
        private ToolStripDropDownButton MotifChooser(Action changed)
        {
            var dd = new ToolStripDropDownButton(MotifLabel()) { ToolTipText = L.T("Muster aus dem Muster-Baukasten: Kategorie ▸ Name ▸ Richtung (beim Ziehen nimmt das Studio die Version der Zieh-Richtung)") };
            dd.DropDownOpening += (s, e) =>
            {
                dd.DropDownItems.Clear();
                _motifs = Motif.LoadAll();
                if (_motifs.Count == 0)
                {
                    dd.DropDownItems.Add(L.T("(noch keine Muster – Muster-Baukasten öffnen)"), null, (s2, e2) => OpenMotifDesigner());
                    return;
                }
                foreach (IGrouping<string, Motif> cat in _motifs.GroupBy(m => m.Category).OrderBy(g => g.Key))
                {
                    var cn = new ToolStripMenuItem(cat.Key);
                    foreach (Motif m in cat.OrderBy(m => m.Name))
                    {
                        var mn = new ToolStripMenuItem(m.Name) { Checked = m.ToString() == BrushCfg.Motif };
                        foreach (Compass c in new[] { Compass.South, Compass.West, Compass.North, Compass.East })
                        {
                            Motif mm = m;
                            Compass cc = c;
                            var dn = new ToolStripMenuItem(L.T(Motif.DirName(c)) + (!m.Allowed(c) ? " (" + L.T("nicht erlaubt") + ")" : m.Has(c) ? string.Empty : " (" + L.T("gedreht") + ")"))
                            {
                                Checked = m.ToString() == BrushCfg.Motif && BrushCfg.MotifDraw.Direction == c,
                                Enabled = m.Allowed(c)
                            };
                            dn.Click += (s2, e2) => ChooseMotif(mm, cc, changed);
                            mn.DropDownItems.Add(dn);
                        }
                        Motif m0 = m;
                        mn.Click += (s2, e2) => ChooseMotif(m0, m0.Allowed(BrushCfg.MotifDraw.Direction) ? BrushCfg.MotifDraw.Direction : m0.NextAllowed(BrushCfg.MotifDraw.Direction), changed);
                        cn.DropDownItems.Add(mn);
                    }
                    dd.DropDownItems.Add(cn);
                }
                dd.DropDownItems.Add(new ToolStripSeparator());
                dd.DropDownItems.Add(L.T("Muster-Baukasten …"), null, (s2, e2) => OpenMotifDesigner());
            };
            dd.DropDownItems.Add("…");
            return dd;
        }

        private void ChooseMotif(Motif m, Compass c, Action changed)
        {
            BrushCfg.Motif = m.ToString();
            BrushCfg.MotifDraw.Direction = c;
            changed?.Invoke();
            if (_edMotifChooser != null)
            {
                _edMotifChooser.Text = MotifLabel();
            }
            isoView.Invalidate();
            SetStatus(L.T("Muster") + $": {m} – {L.T(Motif.DirName(c))} · " + L.T("Klick = einmal, ziehen = wiederholt in Zieh-Richtung"));
        }

        /// <summary>Quick options of the CGI tool "Muster".</summary>
        private void MotifQuickItems()
        {
            AddQuick(new ToolStripSeparator());
            ToolStripDropDownButton dd = null;
            dd = MotifChooser(() => dd.Text = MotifLabel());
            AddQuick(dd);
            Quick(BrushCfg.MotifDraw, "Mode", "Drag", "Repeat", "Direction", "Height", "Floors", "FitLast", "Flatten", "ZOffset", "Replace");
            Cmd("Richtungen …", "Richtungen festlegen: je Teil die Grafik für Nord / Ost / Süd / West (Items, Land) – gilt vor der automatischen Erkennung", () => DirectionGroupsForm.ShowShared(this));
            Cmd("Muster-Baukasten …", "Eigenes Fenster: Muster bauen (Zinnen, Zelte, Mauer mit Fenster …), je Richtung Nord / Ost / Süd / West, nach Kategorie speichern, Log + Bild", OpenMotifDesigner);
        }

        private void OpenMotifDesigner()
        {
            if (_motifDesigner != null)
            {
                _motifDesigner.Activate();
                return;
            }
            var host = new MotifDesignerHost
            {
                Map = _cgiMap,
                CurrentMap = () => _cgiMap,
                Brushes = BrushCfg,
                Configure = ConfigureLikeMain,
                CurrentArea = () => isoView.Area,
                Status = SetStatus,
                Help = ShowHelp,
                Use = (m, c) =>
                {
                    _motifs = Motif.LoadAll();
                    BrushCfg.Motif = m.ToString();
                    BrushCfg.MotifDraw.Direction = c;
                    if (_edMotifChooser != null)
                    {
                        _edMotifChooser.Text = MotifLabel();
                    }
                    if (tabMain.SelectedTab == tabEditor)
                    {
                        SelectTool(MapTool.Motif);
                    }
                    else
                    {
                        tabMain.SelectedTab = tabCgi;
                        SelectCgiTool(IsoTool.Motif);
                        UpdateQuickOptions(IsoTool.Motif);
                    }
                }
            };
            _motifDesigner = new MotifDesignerForm(host) { Owner = this };
            _motifDesigner.FormClosed += (s, e) =>
            {
                _motifDesigner = null;
                _motifs = Motif.LoadAll();
            };
            _motifDesigner.Show(this);
        }

        /// <summary>Map editor, tool "Muster": the motif goes into the statics layer (height above the land, one undo step).</summary>
        private void MapCanvas_MotifRequested(Point a, Point b)
        {
            Motif m = CurrentMotif;
            if (_map == null || !MotifReady(m))
            {
                return;
            }
            List<(int x, int y, int z, ushort id, ushort hue)> parts = m.Layout(a, b, BrushCfg.MotifDraw, null, out Compass dir, out int repeats);
            string what = BrushCfg.MotifDraw.Mode == MotifMode.Radius ? L.T("im Radius") + $": {repeats} " + L.T("Stücke") : $"{L.T(Motif.DirName(dir))}: {repeats}×";
            MotifIntoLayer(m, parts, what);
        }

        private void MapCanvas_MotifPathRequested(List<Point> points)
        {
            Motif m = CurrentMotif;
            if (_map == null || !MotifReady(m))
            {
                return;
            }
            List<(int x, int y, int z, ushort id, ushort hue)> parts = m.Path(points, BrushCfg.MotifDraw, null, out int n);
            MotifIntoLayer(m, parts, L.T("Punkt zu Punkt") + $": {n} " + L.T("Stücke"));
        }

        private bool MotifReady(Motif m)
        {
            if (m == null)
            {
                SetStatus(L.T("Erst ein Muster wählen (Leiste oben: Muster ▸ Kategorie ▸ Name ▸ Richtung) – oder im Muster-Baukasten eins bauen."));
                return false;
            }
            return true;
        }

        /// <summary>Map editor: motif pieces into the statics layer (height above the land, one undo step).</summary>
        private void MotifIntoLayer(Motif m, List<(int x, int y, int z, ushort id, ushort hue)> parts, string what)
        {
            MotifDrawSettings o = BrushCfg.MotifDraw;
            var added = parts.Where(p => _map.InBounds(p.x, p.y)).Select(p => new LayerStatic { X = p.x, Y = p.y, Dz = p.z, Id = p.id, Hue = p.hue })
                             .GroupBy(s => (s.X, s.Y, s.Dz, s.Id)).Select(g => g.First()).ToList();
            if (added.Count == 0)
            {
                SetStatus(L.T("Muster: nichts gesetzt (außerhalb der Karte oder Richtung leer)."));
                return;
            }
            var cells = new HashSet<(int, int)>(added.Select(s => (s.X, s.Y)));
            List<LayerStatic> removed = o.Replace ? _staticsLayer.Items.Where(s => cells.Contains((s.X, s.Y))).ToList() : new List<LayerStatic>();
            void Apply(List<LayerStatic> remove, List<LayerStatic> add)
            {
                var set = new HashSet<(int, int, int, ushort, ushort)>(remove.Select(r => (r.X, r.Y, r.Dz, r.Id, r.Hue)));
                _staticsLayer.Items.RemoveAll(st => set.Contains((st.X, st.Y, st.Dz, st.Id, st.Hue)));
                _staticsLayer.Items.AddRange(add);
                _staticsLayer.Modified = true;
                _map.Modified = true;
                mapCanvas.SetStaticMarkers(_staticsLayer.Items);
            }
            var box = Rectangle.FromLTRB(added.Min(s => s.X), added.Min(s => s.Y), added.Max(s => s.X) + 1, added.Max(s => s.Y) + 1);
            mapCanvas.EditRect(box, L.T("Muster") + " " + m.Name, _ => { }, () =>
            {
                Apply(removed, added);
                _history.AddExtra(() => Apply(added, removed), () => Apply(removed, added));
            });
            o.Seed = new Random().Next(1, int.MaxValue);
            mapCanvas.Invalidate();
            SetStatus(L.T("Muster") + $" {m} – {what}, {added.Count} Statics " + L.T("in der Statics-Ebene") +
                      (removed.Count > 0 ? $", {removed.Count} " + L.T("ersetzt") : string.Empty) +
                      (o.Flatten != MotifFlatten.Off ? " · " + L.T("Boden begradigen geht nur in der CGI-Ansicht") : string.Empty) + " · Strg+Z " + L.T("nimmt es zurück") + ".");
        }

        /// <summary>CGI area → motif kit: opens the kit and fetches the statics of the area.</summary>
        private void AreaToMotif()
        {
            if (isoView.Area.IsEmpty)
            {
                SetStatus(L.T("Erst mit 'Bereich' etwas markieren."));
                return;
            }
            OpenMotifDesigner();
            _motifDesigner?.FetchFromMap();
        }

        /// <summary>
        /// "Boden begradigen": the land under the marked area / the selected statics (+ a margin) to one height. With
        /// "Statics mitnehmen" the statics move by the same amount, so a house stands on the new ground (higher / lower by Z).
        /// </summary>
        private void LevelGroundDialog()
        {
            MapData map = _cgiMap;
            if (map == null)
            {
                return;
            }
            List<StaticItem> sel = isoView.Selection.ToList();
            var cells = new List<Point>();
            if (sel.Count > 0)
            {
                cells.AddRange(sel.Select(s => new Point(s.X, s.Y)).Distinct());
            }
            else if (!isoView.Area.IsEmpty)
            {
                Rectangle r = isoView.Area;
                r.Intersect(new Rectangle(0, 0, map.Width, map.Height));
                HashSet<int> mask = isoView.Mask;
                for (int y = r.Top; y < r.Bottom; y++)
                {
                    for (int x = r.Left; x < r.Right; x++)
                    {
                        if (mask == null || mask.Contains(y * map.Width + x))
                        {
                            cells.Add(new Point(x, y));
                        }
                    }
                }
            }
            if (cells.Count == 0)
            {
                SetStatus(L.T("Erst einen Bereich markieren oder Statics auswählen (z. B. ein Haus)."));
                return;
            }
            List<StaticItem> standing = sel.Count > 0 ? sel : cells.SelectMany(c => map.StaticsAt(c.X, c.Y)).ToList();
            int lowStatic = standing.Count > 0 ? standing.Min(s => s.Z) : int.MinValue;
            int landMin = cells.Min(c => map.LandZ(c.X, c.Y)), landMax = cells.Max(c => map.LandZ(c.X, c.Y));
            int landAvg = (int)Math.Round(cells.Average(c => map.LandZ(c.X, c.Y)));

            using var f = new Form
            {
                Text = L.T("Boden begradigen"), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent, ClientSize = new Size(430, 250), Font = new Font("Segoe UI", 9f)
            };
            var info = new Label
            {
                Left = 12, Top = 10, Width = 406, Height = 46,
                Text = $"{cells.Count} " + L.T("Felder") + $", {standing.Count} Statics · " + L.T("Boden") + $" z {landMin}…{landMax} (" + L.T("Mittel") + $" {landAvg})" +
                       (standing.Count > 0 ? " · " + L.T("unterster Static") + $" z {lowStatic}" : string.Empty)
            };
            var zLabel = new Label { Left = 12, Top = 62, Width = 120, Text = L.T("Neue Boden-Höhe (Z):") };
            var z = new NumericUpDown { Left = 140, Top = 60, Width = 70, Minimum = -128, Maximum = 127, Value = standing.Count > 0 ? lowStatic : landAvg };
            var q1 = new Button { Left = 220, Top = 58, Width = 64, Text = L.T("Static"), Enabled = standing.Count > 0 };
            q1.Click += (s, e) => z.Value = Math.Clamp(lowStatic, -128, 127);
            var q2 = new Button { Left = 288, Top = 58, Width = 64, Text = L.T("Mittel") };
            q2.Click += (s, e) => z.Value = landAvg;
            var q3 = new Button { Left = 356, Top = 58, Width = 64, Text = L.T("Höchster") };
            q3.Click += (s, e) => z.Value = landMax;
            var mLabel = new Label { Left = 12, Top = 96, Width = 120, Text = L.T("Rand (Tiles):") };
            var margin = new NumericUpDown { Left = 140, Top = 94, Width = 70, Minimum = 0, Maximum = 8, Value = 1 };
            var move = new CheckBox
            {
                Left = 12, Top = 126, Width = 406, Height = 40, Checked = true,
                Text = L.T("Statics mitnehmen: das Haus steht danach auf der neuen Höhe (so viel höher / tiefer wie der unterste Static)")
            };
            var ok = new Button { Text = L.T("Begradigen"), Left = 236, Top = 206, Width = 90, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = L.T("Abbrechen"), Left = 330, Top = 206, Width = 90, DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[] { info, zLabel, z, q1, q2, q3, mLabel, margin, move, ok, cancel });
            f.AcceptButton = ok;
            f.CancelButton = cancel;
            if (f.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            (List<StaticChange> statics, List<LandChange> land) = Motif.Level(map, cells, (int)z.Value, (int)margin.Value, move.Checked, sel.Count > 0 ? sel : null);
            if (statics.Count == 0 && land.Count == 0)
            {
                SetStatus(L.T("Boden begradigen: nichts zu ändern (schon auf dieser Höhe)."));
                return;
            }
            map.Apply(statics, land);
            map.NameLastStep(L.T("Boden begradigen"));
            isoView.Refresh(true);
            SetStatus(L.T("Boden begradigt") + $": {land.Count} " + L.T("Land-Tiles auf z") + $" {z.Value}" +
                      (statics.Count > 0 ? $", {statics.Count} Statics " + L.T("mitgenommen") : string.Empty) + " · Strg+Z " + L.T("nimmt es zurück") + ".");
        }
    }
}
