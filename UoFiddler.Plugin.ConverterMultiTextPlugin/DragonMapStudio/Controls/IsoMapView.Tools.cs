/***************************************************************************
 *
 *  * Dragon Map Studio – CGI view: more brushes and edits (Pinsel 21)
 *  *
 *  *  Brushes:  scatter, hue, river, plateau / terraces, noise, tile variants, transitions
 *  *  Clicks:   wall / fence (drag), road along a curve (points), ramp (drag), measure (drag)
 *  *  Select:   lasso (points), magic wand (click) – the mask limits every edit until Esc;
 *  *            drag selected statics with the mouse, rubber band selection on empty ground, Alt+click = pipette
 *  *  Edits:    rotate / mirror area or selection, drop statics to the ground, view filter for kinds of statics
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    public partial class IsoMapView
    {
        // ------------------------------------------------------------------ settings from the form

        /// <summary>Settings of the new brushes (tab "Pinsel").</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CgiBrushSettings BrushSettings { get; set; } = new CgiBrushSettings();

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScatterSet ScatterSet { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WallSet WallSet { get; set; }

        /// <summary>Scatter set by name (for "Weitere Sets mischen").</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string, ScatterSet> FindScatterSet { get; set; }

        /// <summary>The chosen scatter set plus the sets of "Weitere Sets mischen" (one set with all groups).</summary>
        private ScatterSet MixedScatterSet()
        {
            string mix = BrushSettings.Scatter.MixSets;
            if (string.IsNullOrWhiteSpace(mix) || FindScatterSet == null || ScatterSet == null)
            {
                return ScatterSet;
            }
            var all = new ScatterSet { Name = ScatterSet.Name + " +", Groups = ScatterSet.Groups.ToList() };
            foreach (string name in mix.Split(';').Select(n => n.Trim()).Where(n => n.Length > 0))
            {
                ScatterSet other = FindScatterSet(name);
                if (other != null && other != ScatterSet)
                {
                    all.Groups.AddRange(other.Groups);
                }
            }
            return all;
        }

        /// <summary>Tiles of the same family (colour of the colour table) – variants brush and magic wand.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<ushort, ushort[]> LandFamily { get; set; }

        /// <summary>Area + E or menu: the form opens "Suchen &amp; Ersetzen".</summary>
        public event EventHandler ReplaceRequested;

        private static bool IsNewBrush(IsoTool t) => t == IsoTool.Scatter || t == IsoTool.HueBrush || t == IsoTool.River || t == IsoTool.Plateau ||
                                                     t == IsoTool.Noise || t == IsoTool.Variants || t == IsoTool.TransitionBrush || t == IsoTool.Eraser;

        private static bool IsClickTool(IsoTool t) => t == IsoTool.Wall || t == IsoTool.Road || t == IsoTool.Ramp || t == IsoTool.Measure ||
                                                      t == IsoTool.Lasso || t == IsoTool.MagicWand || t == IsoTool.Stairs || t == IsoTool.Motif || IsBuildTool(t);

        /// <summary>Drag tools of the builders: bridge (line), cellar and dungeon (rectangle), course (line).</summary>
        private static bool IsBuildTool(IsoTool t) => t == IsoTool.Bridge || t == IsoTool.Cellar || t == IsoTool.Dungeon || t == IsoTool.Course || t == IsoTool.MountainWall;

        private static bool IsRectTool(IsoTool t) => t == IsoTool.Cellar || t == IsoTool.Dungeon || t == IsoTool.MountainWall;

        /// <summary>Learned dungeon look by name (null = wall set + floor statics).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string, DungeonLook> FindDungeonLook { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BridgeSet BridgeSet { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StairSet StairSet { get; set; }

        /// <summary>Wall set by name (cellar, dungeon).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string, WallSet> FindWallSet { get; set; }

        private bool _underView;

        /// <summary>
        /// "Unter der Karte bauen": statics below the land are drawn on top (visible), a banner shows the building level,
        /// Static setzen / Wand place at ground + <see cref="CgiBrushSettings.UnderOffset"/>.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UnderView
        {
            get => _underView;
            set
            {
                _underView = value;
                Refresh(true);
            }
        }

        private int _previewSeed = Environment.TickCount;

        // ------------------------------------------------------------------ mask (lasso / wand)

        private HashSet<int> _mask;

        /// <summary>Cells of the lasso / magic wand (null = none). While set, every edit stays inside.</summary>
        public HashSet<int> Mask => _mask;

        public void ClearMask()
        {
            _mask = null;
            if (_map != null)
            {
                _map.EditMask = null;
            }
            Invalidate();
        }

        private void SetMask(HashSet<int> cells, Keys mods, string what)
        {
            if ((mods & Keys.Shift) != 0 && _mask != null)
            {
                cells.UnionWith(_mask);
            }
            else if ((mods & Keys.Control) != 0 && _mask != null)
            {
                var rest = new HashSet<int>(_mask);
                rest.ExceptWith(cells);
                cells = rest;
            }
            if (cells.Count == 0)
            {
                ClearMask();
                _area = Rectangle.Empty;
                Message?.Invoke(this, L.T("Auswahl leer."));
                return;
            }
            _mask = cells;
            _map.EditMask = cells;
            _area = CgiBrushes.Bounds(_map, cells);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
            Message?.Invoke(this, what + $": {cells.Count:N0} " + L.T("Felder – alle Werkzeuge und Bereich-Befehle wirken nur noch darin (Umschalt = dazu, Strg = weg, Esc = aufheben)."));
        }

        // ------------------------------------------------------------------ view filter

        private readonly Dictionary<ushort, StaticKind> _kinds = new Dictionary<ushort, StaticKind>();

        /// <summary>Kinds of statics that are not drawn (and cannot be clicked).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StaticKind HiddenKinds { get; set; }

        private bool Hidden(ushort id)
        {
            if (HiddenKinds == StaticKind.None)
            {
                return false;
            }
            if (!_kinds.TryGetValue(id, out StaticKind k))
            {
                _kinds[id] = k = UoArtCache.ItemKind(id);
            }
            return (HiddenKinds & k) != 0;
        }

        // ------------------------------------------------------------------ brush strokes

        private readonly HashSet<int> _visited = new HashSet<int>();
        private readonly Dictionary<int, int> _groundBefore = new Dictionary<int, int>();
        private int _strokeLevel;
        private readonly HashSet<int> _riverCore = new HashSet<int>();
        private Rectangle _strokeBox;

        private int GroundBefore(int x, int y)
        {
            int i = y * _map.Width + x;
            if (!_groundBefore.TryGetValue(i, out int z))
            {
                _groundBefore[i] = z = _map.LandZ(x, y);
            }
            return z;
        }

        /// <summary>Start of a stroke of the land brushes (before the first dab).</summary>
        private void BeginBrushStroke(Point tile)
        {
            _visited.Clear();
            _groundBefore.Clear();
            _strokeBox = Rectangle.Empty;
            _riverCore.Clear();
            int r = Math.Max(0, LandRadius);
            switch (Tool)
            {
                case IsoTool.Plateau:
                    if (BrushSettings.Plateau.UseTargetHeight)
                    {
                        _strokeLevel = TargetHeight;
                    }
                    else
                    {
                        var zs = CgiBrushes.Circle(_map, tile.X, tile.Y, r).Select(c => _map.LandZ(c.x, c.y)).ToList();
                        _strokeLevel = zs.Count == 0 ? _map.LandZ(tile.X, tile.Y) : (int)Math.Round(zs.Average());
                    }
                    break;
                case IsoTool.River:
                    _strokeLevel = _map.LandZ(tile.X, tile.Y) - BrushSettings.River.Depth;
                    break;
            }
        }

        /// <summary>One dab of the new brushes. True = handled.</summary>
        private bool ApplyNewBrush(Point center, int r)
        {
            if (!IsNewBrush(Tool))
            {
                return false;
            }
            bool ctrl = (ModifierKeys & Keys.Control) != 0;
            int reach = Tool == IsoTool.River ? r + Math.Max(0, BrushSettings.River.BankWidth) : r;
            var dab = new Rectangle(center.X - reach, center.Y - reach, reach * 2 + 1, reach * 2 + 1);
            _strokeBox = _strokeBox.IsEmpty ? dab : Rectangle.Union(_strokeBox, dab);
            switch (Tool)
            {
                case IsoTool.Scatter:
                    if (ScatterSet == null)
                    {
                        Message?.Invoke(this, L.T("Streu-Pinsel: erst ein Set wählen (Reiter Pinsel)."));
                        break;
                    }
                    _map.ApplyStatics(CgiBrushes.Scatter(_map, center.X, center.Y, r, MixedScatterSet(), BrushSettings.Scatter, _rnd, ctrl, ctrl ? null : _visited));
                    break;
                case IsoTool.HueBrush:
                    _map.ApplyStatics(CgiBrushes.HueBrush(_map, center.X, center.Y, r, ctrl ? (ushort)0 : PlaceHue, s => !Hidden(s.Id)));
                    break;
                case IsoTool.River:
                    CgiBrushes.River(_map, center.X, center.Y, r + 0.5, _strokeLevel, BrushSettings.River, GroundBefore, _rnd, _riverCore);
                    break;
                case IsoTool.Plateau:
                    CgiBrushes.Plateau(_map, center.X, center.Y, r, _strokeLevel, BrushSettings.Plateau, BrushSettings.Hardness, _rnd);
                    break;
                case IsoTool.Noise:
                    CgiBrushes.NoiseBrush(_map, center.X, center.Y, r, BrushSettings.Noise, BrushSettings.Hardness, _visited, GroundBefore, _rnd);
                    break;
                case IsoTool.Variants:
                    {
                        VariantSet vs = string.IsNullOrEmpty(BrushSettings.VariantSet) ? null : BrushSettings.VariantSets.FirstOrDefault(v => v.Name == BrushSettings.VariantSet);
                        if (vs != null)
                        {
                            CgiBrushes.VariantsFromSet(_map, center.X, center.Y, r, vs, _visited, _rnd);
                            break;
                        }
                    }
                    if (LandFamily == null)
                    {
                        break;
                    }
                    CgiBrushes.Variants(_map, center.X, center.Y, r, LandFamily, _visited, _rnd);
                    break;
                case IsoTool.TransitionBrush:
                    LandTransitions?.Apply(_map, new Rectangle(center.X - r, center.Y - r, r * 2 + 1, r * 2 + 1), _rnd);
                    break;
                case IsoTool.Eraser:
                    {
                        EraserSettings o = BrushSettings.Eraser;
                        ushort[] voids = HexList.Parse(o.VoidTile);
                        var onlyIds = new HashSet<ushort>(HexList.Parse(o.OnlyIds));
                        ushort vt = voids.Length > 0 ? voids[0] : (ushort)0x0002;
                        var del = new List<StaticChange>();
                        foreach ((int x, int y, double _) in CgiBrushes.Circle(_map, center.X, center.Y, r))
                        {
                            if (o.Statics && !ctrl)
                            {
                                del.AddRange(_map.StaticsAt(x, y).Where(s => (!o.OnlyVisible || (!Hidden(s.Id) && s.Z <= MaxZ)) && (onlyIds.Count == 0 || onlyIds.Contains(s.Id))).ToList().Select(MapData.Remove));
                            }
                            if (o.Land || ctrl)
                            {
                                _map.SetLand(x, y, vt, _map.LandZ(x, y));
                            }
                        }
                        foreach (StaticChange c in del)
                        {
                            _selection.Remove(c.Item);
                        }
                        _map.ApplyStatics(del);
                        break;
                    }
            }
            Refresh(true);
            return true;
        }

        /// <summary>End of a stroke (still inside its undo step): transitions around the river.</summary>
        private void EndBrushStroke()
        {
            if (Tool == IsoTool.River && BrushSettings.River.Transitions && LandTransitions != null && !_strokeBox.IsEmpty)
            {
                LandTransitions.Apply(_map, Rectangle.Inflate(_strokeBox, 1, 1), _rnd);
            }
            if (Tool == IsoTool.River)
            {
                _map.ApplyStatics(CgiBrushes.RestoreWater(_map, _riverCore, BrushSettings.River, _rnd));
                _riverCore.Clear();
                Refresh(true);
            }
        }

        /// <summary>Soft edge of the old height tools (Höhe +/−, Glätten).</summary>
        private double HardnessWeight(int x, int y, Point center, int r) =>
            CgiBrushes.Weight(Math.Sqrt((x - center.X) * (x - center.X) + (y - center.Y) * (y - center.Y)), r, BrushSettings.Hardness);

        // ------------------------------------------------------------------ click / drag tools

        private Point? _dragStart;
        private Point _dragEnd;
        private readonly List<Point> _points = new List<Point>();
        private string _measureText;
        private Point? _measureA;
        private Point _measureB;

        // select tool: move by dragging, rubber band
        private bool _moveDrag;
        private Point _moveStart;
        private Point _moveTile;
        private bool _bandDrag;
        private Point _bandStart;
        private Point _bandEnd;

        /// <summary>Points of the road / lasso being placed.</summary>
        public int PendingPoints => _points.Count;

        /// <summary>Mouse down with the left button. True = handled (the old tools are not asked).</summary>
        private bool ToolMouseDown(MouseEventArgs e, Point tile, StaticItem hit)
        {
            // Alt + click: pipette with every tool (item under the mouse, else the land)
            if ((ModifierKeys & Keys.Alt) != 0)
            {
                if (hit != null)
                {
                    ItemPicked?.Invoke(this, new StaticPickEventArgs(hit));
                }
                else if (_map.InBounds(tile.X, tile.Y))
                {
                    LandPicked?.Invoke(this, new MapPointEventArgs(tile.X, tile.Y, _map.LandId(tile.X, tile.Y)));
                }
                return true;
            }
            if (!_map.InBounds(tile.X, tile.Y))
            {
                return IsClickTool(Tool);
            }
            if ((Tool == IsoTool.Wall && BrushSettings.Wall.PointMode) || (Tool == IsoTool.Motif && BrushSettings.MotifDraw.Mode == MotifMode.Points))
            {
                if (e.Clicks >= 2)
                {
                    FinishPoints();
                    return true;
                }
                _points.Add(tile);
                Message?.Invoke(this, $"{_points.Count} " + L.T("Punkte – weiter klicken · Doppelklick, Rechtsklick oder Enter = fertig · Rücktaste = letzten Punkt weg · Esc = abbrechen"));
                Invalidate();
                return true;
            }
            switch (Tool)
            {
                case IsoTool.Wall:
                case IsoTool.Ramp:
                case IsoTool.Measure:
                case IsoTool.Bridge:
                case IsoTool.Cellar:
                case IsoTool.Dungeon:
                case IsoTool.Course:
                case IsoTool.MountainWall:
                    _dragStart = tile;
                    _dragEnd = tile;
                    Invalidate();
                    return true;
                case IsoTool.Road:
                case IsoTool.Lasso:
                    if (e.Clicks >= 2)
                    {
                        FinishPoints();
                        return true;
                    }
                    _points.Add(tile);
                    Message?.Invoke(this, $"{_points.Count} " + L.T("Punkte – weiter klicken · Doppelklick, Rechtsklick oder Enter = fertig · Rücktaste = letzten Punkt weg · Esc = abbrechen"));
                    Invalidate();
                    return true;
                case IsoTool.Motif:
                case IsoTool.Stairs:
                    // click = the model / staircase once, drag = one step per tile
                    _dragStart = tile;
                    _dragEnd = tile;
                    Invalidate();
                    return true;
                case IsoTool.MagicWand:
                    SetMask(CgiBrushes.Wand(_map, tile.X, tile.Y, BrushSettings.Wand, LandFamily), ModifierKeys, L.T("Zauberstab"));
                    return true;
            }
            return false;
        }

        /// <summary>After the select tool chose: start dragging the selection or a rubber band.</summary>
        private void SelectMouseDown(MouseEventArgs e, Point tile, StaticItem hit)
        {
            if (hit != null && _selection.Contains(hit) && (ModifierKeys & (Keys.Control | Keys.Shift)) == 0)
            {
                _moveDrag = true;
                _moveStart = tile;
                _moveTile = tile;
            }
            else if (hit == null && (ModifierKeys & Keys.Shift) == 0)
            {
                _bandDrag = true;
                _bandStart = e.Location;
                _bandEnd = e.Location;
            }
        }

        /// <summary>Right click with a point tool: finish. True = handled.</summary>
        private bool ToolRightClick()
        {
            if ((Tool == IsoTool.Road || Tool == IsoTool.Lasso || Tool == IsoTool.Wall || Tool == IsoTool.Motif) && _points.Count > 0)
            {
                FinishPoints();
                return true;
            }
            return false;
        }

        private void ToolMouseMove(MouseEventArgs e, Point tile)
        {
            if (_dragStart.HasValue && tile != _dragEnd && _map.InBounds(tile.X, tile.Y))
            {
                _dragEnd = tile;
                if (Tool == IsoTool.Measure)
                {
                    UpdateMeasure(_dragStart.Value, tile);
                }
                Invalidate();
            }
            if (_moveDrag && tile != _moveTile && _map.InBounds(tile.X, tile.Y))
            {
                _moveTile = tile;
                Invalidate();
            }
            if (_bandDrag)
            {
                _bandEnd = e.Location;
                Invalidate();
            }
        }

        /// <summary>Mouse up. True = handled.</summary>
        private bool ToolMouseUp(MouseEventArgs e)
        {
            if (_moveDrag)
            {
                _moveDrag = false;
                int dx = _moveTile.X - _moveStart.X, dy = _moveTile.Y - _moveStart.Y;
                if (dx != 0 || dy != 0)
                {
                    MoveSelection(dx, dy, 0, false);
                }
                Invalidate();
                return true;
            }
            if (_bandDrag)
            {
                _bandDrag = false;
                Rectangle band = Rectangle.FromLTRB(Math.Min(_bandStart.X, _bandEnd.X), Math.Min(_bandStart.Y, _bandEnd.Y), Math.Max(_bandStart.X, _bandEnd.X), Math.Max(_bandStart.Y, _bandEnd.Y));
                if (band.Width > 4 || band.Height > 4)
                {
                    Point a = ToScene(band.Location), b = ToScene(new Point(band.Right, band.Bottom));
                    var scene = Rectangle.FromLTRB(a.X, a.Y, b.X, b.Y);
                    int before = _selection.Count;
                    foreach (Placed p in _placed)
                    {
                        if (scene.Contains(new Point(p.Rect.X + p.Rect.Width / 2, p.Rect.Bottom - 2)))
                        {
                            _selection.Add(p.Item);
                        }
                    }
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    Message?.Invoke(this, $"{_selection.Count - before} " + L.T("Static(s) mit dem Rahmen ausgewählt – ziehen verschiebt, Pfeile / Bild↑↓ auch."));
                }
                Invalidate();
                return true;
            }
            if (!_dragStart.HasValue)
            {
                return false;
            }
            Point a0 = _dragStart.Value, b0 = _dragEnd;
            _dragStart = null;
            switch (Tool)
            {
                case IsoTool.Wall:
                    PlaceWall(a0, b0);
                    break;
                case IsoTool.Ramp:
                    PlaceRamp(a0, b0);
                    break;
                case IsoTool.Measure:
                    UpdateMeasure(a0, b0);
                    _measureA = a0;
                    _measureB = b0;
                    Message?.Invoke(this, _measureText);
                    break;
                case IsoTool.Bridge:
                    PlaceBridge(a0, b0);
                    break;
                case IsoTool.Cellar:
                    PlaceCellar(Rectangle.FromLTRB(Math.Min(a0.X, b0.X), Math.Min(a0.Y, b0.Y), Math.Max(a0.X, b0.X) + 1, Math.Max(a0.Y, b0.Y) + 1));
                    break;
                case IsoTool.Dungeon:
                case IsoTool.MountainWall:
                    PlaceDungeon(Rectangle.FromLTRB(Math.Min(a0.X, b0.X), Math.Min(a0.Y, b0.Y), Math.Max(a0.X, b0.X) + 1, Math.Max(a0.Y, b0.Y) + 1), Tool == IsoTool.MountainWall);
                    break;
                case IsoTool.Course:
                    PlaceCourse(a0, b0);
                    break;
                case IsoTool.Motif:
                    PlaceMotif(a0, b0);
                    break;
                case IsoTool.Stairs:
                    if (a0 == b0)
                    {
                        PlaceStairs(a0);
                    }
                    else
                    {
                        PlaceStairRun(a0, b0);
                    }
                    break;
            }
            Invalidate();
            return true;
        }

        /// <summary>Keys of the point tools and the new edits. True = handled.</summary>
        private bool ToolKeyDown(KeyEventArgs e)
        {
            if (Tool == IsoTool.Motif && e.KeyCode == Keys.R && !e.Control)
            {
                Motif rm = string.IsNullOrEmpty(BrushSettings.Motif) ? null : FindMotif?.Invoke(BrushSettings.Motif);
                BrushSettings.MotifDraw.Direction = rm != null ? rm.NextAllowed(BrushSettings.MotifDraw.Direction) : (Compass)(((int)BrushSettings.MotifDraw.Direction + 1) % 4);
                Message?.Invoke(this, L.T("Muster: Richtung") + " " + L.T(Core.Motif.DirName(BrushSettings.MotifDraw.Direction)));
                Invalidate();
                return true;
            }
            if (Tool == IsoTool.Stairs && e.KeyCode == Keys.R && !e.Control)
            {
                BrushSettings.Staircase.Up = (Compass)(((int)BrushSettings.Staircase.Up + 1) % 4);
                Message?.Invoke(this, L.T("Treppe hinauf Richtung") + " " + L.T(BrushSettings.Staircase.Up switch
                {
                    Compass.North => "Nord", Compass.East => "Ost", Compass.South => "Süd", _ => "West"
                }));
                Invalidate();
                return true;
            }
            if ((Tool == IsoTool.Road || Tool == IsoTool.Lasso || Tool == IsoTool.Wall || Tool == IsoTool.Motif) && _points.Count > 0)
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        FinishPoints();
                        return true;
                    case Keys.Back:
                        _points.RemoveAt(_points.Count - 1);
                        Invalidate();
                        return true;
                    case Keys.Escape:
                        _points.Clear();
                        Invalidate();
                        Message?.Invoke(this, L.T("Abgebrochen."));
                        return true;
                }
            }
            if (e.KeyCode == Keys.Escape && _mask != null && _selection.Count == 0 && !_areaPaste && !_pasteGhost)
            {
                ClearMask();
                _area = Rectangle.Empty;
                Message?.Invoke(this, L.T("Lasso / Zauberstab aufgehoben."));
                return true;
            }
            if (e.KeyCode == Keys.Escape && _measureA.HasValue)
            {
                _measureA = null;
                _measureText = null;
                Invalidate();
            }
            bool area = !_area.IsEmpty && _selection.Count == 0 && (Tool == IsoTool.Area || Tool == IsoTool.Lasso || Tool == IsoTool.MagicWand);
            bool sel = _selection.Count > 0;
            if (e.Control || e.Alt)
            {
                return false;
            }
            switch (e.KeyCode)
            {
                case Keys.R when area:
                    TransformArea(1, false, false);
                    return true;
                case Keys.X when area:
                    TransformArea(0, true, false);
                    return true;
                case Keys.Y when area:
                    TransformArea(0, false, true);
                    return true;
                case Keys.E when area:
                    ReplaceRequested?.Invoke(this, EventArgs.Empty);
                    return true;
                case Keys.D when area:
                    DropAreaToGround();
                    return true;
                case Keys.R when sel:
                    TransformSelection(1, false, false);
                    return true;
                case Keys.X when sel:
                    TransformSelection(0, true, false);
                    return true;
                case Keys.D when sel:
                    DropSelectionToGround();
                    return true;
            }
            return false;
        }

        private void FinishPoints()
        {
            if (Tool == IsoTool.Lasso)
            {
                var poly = _points.Select(p => new PointF(p.X + 0.5f, p.Y + 0.5f)).ToList();
                _points.Clear();
                if (poly.Count < 3)
                {
                    Message?.Invoke(this, L.T("Lasso: mindestens 3 Punkte."));
                    Invalidate();
                    return;
                }
                SetMask(CgiBrushes.Polygon(_map, poly), ModifierKeys, L.T("Lasso"));
                return;
            }
            if (Tool == IsoTool.Motif)
            {
                var mp = _points.ToList();
                _points.Clear();
                PlaceMotifPath(mp);
                return;
            }
            if (Tool == IsoTool.Wall)
            {
                var wp = _points.ToList();
                _points.Clear();
                if (wp.Count < 2)
                {
                    Message?.Invoke(this, L.T("Wand: mindestens 2 Punkte."));
                    Invalidate();
                    return;
                }
                PlaceWallPath(wp);
                return;
            }
            if (Tool == IsoTool.Road && !string.IsNullOrEmpty(BrushSettings.RoadStamp))
            {
                var rp = _points.ToList();
                _points.Clear();
                TilePattern pat = FindStampPattern?.Invoke(BrushSettings.RoadStamp);
                if (pat == null || rp.Count < 2)
                {
                    Message?.Invoke(this, L.T("Weg mit Stempel: Stempel nicht gefunden oder weniger als 2 Punkte."));
                    Invalidate();
                    return;
                }
                RoadWithStamp(rp, pat);
                return;
            }
            if (Tool == IsoTool.Road)
            {
                var pts = _points.ToList();
                _points.Clear();
                if (pts.Count < 2)
                {
                    Message?.Invoke(this, L.T("Weg: mindestens 2 Punkte."));
                    Invalidate();
                    return;
                }
                ushort[] tiles = HexList.Parse(BrushSettings.Road.Tiles);
                if (tiles.Length == 0)
                {
                    tiles = PlaceLandIds != null && PlaceLandIds.Length > 0 ? PlaceLandIds : new[] { PlaceLandId };
                }
                _map.BeginLand();
                Rectangle box = CgiBrushes.Road(_map, pts, BrushSettings.Road, tiles, _rnd);
                if (BrushSettings.Road.Transitions && LandTransitions != null && !box.IsEmpty)
                {
                    LandTransitions.Apply(_map, Rectangle.Inflate(box, 1, 1), _rnd);
                }
                int n = _map.EndLand();
                Finished(n, L.T("Weg gebaut") + $" ({pts.Count} " + L.T("Punkte") + ")");
            }
        }

        private void Finished(int changes, string text)
        {
            Refresh(true);
            if (changes > 0)
            {
                _map.NameLastStep(text);
                MapChanged?.Invoke(this, EventArgs.Empty);
                Message?.Invoke(this, text + $": {changes} " + L.T("Änderung(en) – Strg+Z nimmt es zurück."));
            }
            else
            {
                Message?.Invoke(this, text + ": " + L.T("nichts geändert."));
            }
        }

        /// <summary>Stamp of the road (file name) as pattern; set by the form.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string, TilePattern> FindStampPattern { get; set; }

        /// <summary>Road from a stamp: the pattern runs along the smooth curve like the freehand pattern brush (land + statics).</summary>
        private void RoadWithStamp(List<Point> pts, TilePattern pat)
        {
            TilePattern keep = Pattern;
            pat.Freehand = true;
            Pattern = pat;
            _strokeCells.Clear();
            int r = Math.Max(0, (BrushSettings.Road.Width - 1) / 2);
            _map.BeginLand();
            var box = Rectangle.Empty;
            try
            {
                foreach (PointF c in CgiBrushes.Spline(pts))
                {
                    var center = new Point((int)Math.Floor(c.X), (int)Math.Floor(c.Y));
                    ApplyPattern(center, r);
                    var dab = new Rectangle(center.X - r - 1, center.Y - r - 1, r * 2 + 3, r * 2 + 3);
                    box = box.IsEmpty ? dab : Rectangle.Union(box, dab);
                }
                if (pat.Transitions && LandTransitions != null && !box.IsEmpty && pat.Land)
                {
                    LandTransitions.Apply(_map, box, _rnd);
                }
            }
            finally
            {
                Pattern = keep;
                _strokeCells.Clear();
            }
            int n = _map.EndLand();
            Finished(n, L.T("Weg mit Stempel") + $" '{pat.Name}' ({pts.Count} " + L.T("Punkte") + ")");
        }

        private void PlaceWallPath(List<Point> pts)
        {
            if (WallSet == null)
            {
                Message?.Invoke(this, L.T("Wand: erst ein Wand-Set mit IDs anlegen (Reiter Pinsel → Wand)."));
                return;
            }
            WallSettings ws = WallFor();
            List<StaticChange> changes = CgiBrushes.WallPath(_map, pts, WallSet, ws, _rnd);
            int before = _map.UndoCount;
            _map.Apply(changes);
            Finished(_map.UndoCount > before ? changes.Count : 0, L.T("Wand") + $" '{WallSet.Name}' ({pts.Count} " + L.T("Punkte") + $", " + L.T("Dicke") + $" {ws.Thickness})");
        }

        private WallSettings WallFor()
        {
            WallSettings ws = BrushSettings.Wall;
            if (UnderView)
            {
                // building under the map: the wall stands on the building level
                ws = new WallSettings
                {
                    Levels = ws.Levels, LevelHeight = ws.LevelHeight, ZOffset = ws.ZOffset + BrushSettings.UnderOffset,
                    PostsAtEnds = ws.PostsAtEnds, ReplaceExisting = ws.ReplaceExisting, Level = ws.Level, Thickness = ws.Thickness
                };
            }
            return ws;
        }

        private void PlaceWall(Point a, Point b)
        {
            if (WallSet == null || (HexList.Parse(WallSet.AlongX).Length == 0 && HexList.Parse(WallSet.AlongY).Length == 0 && HexList.Parse(WallSet.Post).Length == 0))
            {
                Message?.Invoke(this, L.T("Wand: erst ein Wand-Set mit IDs anlegen (Reiter Pinsel → Wand)."));
                return;
            }
            WallSettings ws = WallFor();
            // drag: straight along X or Y, with the thickness
            Point end = Math.Abs(b.X - a.X) >= Math.Abs(b.Y - a.Y) ? new Point(b.X, a.Y) : new Point(a.X, b.Y);
            List<StaticChange> changes = CgiBrushes.WallPath(_map, new List<Point> { a, end }, WallSet, ws, _rnd);
            int before = _map.UndoCount;
            _map.Apply(changes);
            Finished(_map.UndoCount > before ? changes.Count : 0, L.T("Wand") + $" '{WallSet.Name}'");
        }

        private void PlaceRamp(Point a, Point b)
        {
            if (a == b)
            {
                Message?.Invoke(this, L.T("Rampe: von unten nach oben ziehen (Strg = bis 'Höhe =')."));
                return;
            }
            int width = BrushSettings.Ramp.Width > 0 ? BrushSettings.Ramp.Width : LandRadius * 2 + 1;
            int za = _map.LandZ(a.X, a.Y);
            int zb = (ModifierKeys & Keys.Control) != 0 ? TargetHeight : _map.LandZ(b.X, b.Y);
            ushort[] tiles = PlaceLandIds != null && PlaceLandIds.Length > 0 ? PlaceLandIds : new[] { PlaceLandId };
            _map.BeginLand();
            CgiBrushes.Ramp(_map, a, b, width, za, zb, BrushSettings.Ramp, tiles, _rnd);
            int n = _map.EndLand();
            Finished(n, L.T("Rampe") + $" z {za} → {zb}");
        }

        // ------------------------------------------------------------------ builders

        /// <summary>Bridge plan by "Kategorie / Name" (designer), null = none.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string, BridgeTemplate> FindBridgeTemplate { get; set; }

        /// <summary>The plan the bridge tool builds: the chosen designer plan, else made from the bridge set, else from the tiledata.</summary>
        public BridgeTemplate CurrentBridgePlan(out string source)
        {
            BridgeTemplate t = string.IsNullOrEmpty(BrushSettings.BridgeTemplate) ? null : FindBridgeTemplate?.Invoke(BrushSettings.BridgeTemplate);
            if (t != null)
            {
                source = L.T("Plan") + $" '{t}'";
                return t;
            }
            BridgeSet set = BridgeSet;
            var g = new BridgeGenSettings { Width = BrushSettings.Bridge.Width, PillarEvery = BrushSettings.Bridge.PillarEvery };
            if (set != null && (HexList.Parse(set.DeckX).Length > 0 || HexList.Parse(set.DeckY).Length > 0))
            {
                g.Deck = HexList.Parse(set.DeckX).Length > 0 ? set.DeckX : set.DeckY;
                g.DeckY = HexList.Parse(set.DeckY).Length > 0 ? set.DeckY : string.Empty;
                g.RailX = set.RailX;
                g.RailY = set.RailY;
                g.Post = set.Post;
                g.Pillar = set.Pillar;
                source = L.T("Set") + $" '{set.Name}'";
            }
            else
            {
                BridgeGenSettings auto = BridgeKit.AutoPieces();
                auto.Width = g.Width;
                auto.PillarEvery = g.PillarEvery;
                g = auto;
                source = L.T("Teile aus der TileData") + $": " + L.T("Bretter") + $" {g.Deck}, " + L.T("Geländer") + $" {g.RailX}/{g.RailY}, " + L.T("Pfosten") + $" {g.Post}, " + L.T("Pfeiler") + $" {g.Pillar}";
            }
            t = BridgeKit.Generate(g, set?.Name ?? "Auto");
            StairSet st = StairSet;
            if (st != null)
            {
                t.StairNorth = st.DownNorth;
                t.StairEast = st.DownEast;
                t.StairSouth = st.DownSouth;
                t.StairWest = st.DownWest;
            }
            return t;
        }

        private void PlaceBridge(Point a, Point b)
        {
            if (a == b)
            {
                Message?.Invoke(this, L.T("Brücke: von Ufer zu Ufer ziehen (West-Ost oder Nord-Süd)."));
                return;
            }
            BridgeTemplate plan = CurrentBridgePlan(out string source);
            if (plan.PartsX.Count == 0 && plan.PartsY.Count == 0)
            {
                Message?.Invoke(this, L.T("Brücke: keine Teile gefunden – im Brücken-Designer Teile wählen (Bretter, Geländer) oder die TileData laden."));
                return;
            }
            List<StaticChange> changes = BridgeKit.Build(_map, a, b, plan, BrushSettings.Bridge, _rnd);
            int before = _map.UndoCount;
            _map.Apply(changes);
            Finished(_map.UndoCount > before ? changes.Count : 0, L.T("Brücke") + $" ({source})");
        }

        /// <summary>Motif by "Kategorie / Name" (motif designer).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string, Motif> FindMotif { get; set; }

        private Motif CurrentMotif => string.IsNullOrEmpty(BrushSettings.Motif) ? null : FindMotif?.Invoke(BrushSettings.Motif);

        /// <summary>Pieces of the motif for a click / drag (preview and building).</summary>
        public List<StaticChange> MotifPieces(Point a, Point b, out string info) => MotifPieces(a, b, out info, out _);

        private List<StaticChange> MotifPieces(Point a, Point b, out string info, out List<LandChange> land)
        {
            info = string.Empty;
            land = new List<LandChange>();
            Motif m = CurrentMotif;
            if (m == null)
            {
                return new List<StaticChange>();
            }
            MotifDrawSettings o = BrushSettings.MotifDraw;
            int under = UnderView ? BrushSettings.UnderOffset : 0;
            List<StaticChange> ch = m.Build(_map, a, b, o, ZOffset + under, out Compass dir, out int n, out land);
            info = o.Mode == MotifMode.Radius
                ? $"'{m}' " + L.T("im Radius") + $": {n} " + L.T("Stücke")
                : $"'{m}' {L.T(Core.Motif.DirName(dir))} × {n}" + (o.Floors > 1 ? $", {o.Floors} " + L.T("Etagen") : string.Empty);
            if (land.Count > 0)
            {
                info += $", {land.Count} " + L.T("Land-Tiles begradigt");
            }
            return ch;
        }

        /// <summary>Point to point: the stretches with corners (preview: the points so far + the mouse).</summary>
        private List<StaticChange> MotifPathPieces(List<Point> pts, out string info, out List<LandChange> land)
        {
            info = string.Empty;
            land = new List<LandChange>();
            Motif m = CurrentMotif;
            if (m == null || pts.Count == 0)
            {
                return new List<StaticChange>();
            }
            int under = UnderView ? BrushSettings.UnderOffset : 0;
            List<StaticChange> ch = m.BuildPath(_map, pts, BrushSettings.MotifDraw, ZOffset + under, out int n, out land);
            info = $"'{m}' " + L.T("Punkt zu Punkt") + $": {Core.Motif.SnapPath(pts).Count} " + L.T("Punkte") + $", {n} " + L.T("Stücke") +
                   (land.Count > 0 ? $", {land.Count} " + L.T("Land-Tiles begradigt") : string.Empty);
            return ch;
        }

        private void ApplyMotif(List<StaticChange> ch, List<LandChange> land, string info)
        {
            if (ch.Count == 0)
            {
                Message?.Invoke(this, L.T("Muster: erst ein Muster wählen (Leiste oben / Muster-Baukasten)."));
                return;
            }
            int before = _map.UndoCount;
            _map.Apply(ch, land);
            BrushSettings.MotifDraw.Seed = _rnd.Next(1, int.MaxValue);
            if (land.Count > 0)
            {
                Refresh(true);
            }
            Finished(_map.UndoCount > before ? ch.Count : 0, L.T("Muster") + " " + info);
        }

        private void PlaceMotif(Point a, Point b)
        {
            List<StaticChange> ch = MotifPieces(a, b, out string info, out List<LandChange> land);
            ApplyMotif(ch, land, info);
        }

        private void PlaceMotifPath(List<Point> pts)
        {
            List<StaticChange> ch = MotifPathPieces(pts, out string info, out List<LandChange> land);
            ApplyMotif(ch, land, info);
        }

        /// <summary>Preview pieces of the motif tool (click / drag / the points so far).</summary>
        private List<StaticChange> MotifGhost()
        {
            if (BrushSettings.MotifDraw.Mode == MotifMode.Points)
            {
                var pts = _points.ToList();
                pts.Add(_hover);
                return MotifPathPieces(pts, out _, out _);
            }
            return MotifPieces(_dragStart ?? _hover, _dragStart.HasValue ? _dragEnd : _hover, out _);
        }

        /// <summary>Stair model by "Kategorie / Name" (stair designer).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<string, StairModel> FindStairModel { get; set; }

        /// <summary>The statics the tool "Treppe" would set at a tile: the chosen model, else the staircase of the stair set.</summary>
        public List<StaticChange> StairPieces(Point tile)
        {
            StaircaseSettings o = BrushSettings.Staircase;
            StairModel model = string.IsNullOrEmpty(o.Model) ? null : FindStairModel?.Invoke(o.Model);
            int z = PlaceZ(tile.X, tile.Y);
            if (model != null)
            {
                return model.Build(_map, tile, z, o.Up, o.Repeats, o.Downwards);
            }
            return StairSet == null ? new List<StaticChange>() : Staircase.Build(_map, tile, z, o, StairSet);
        }

        /// <summary>Stairs by dragging: one stair piece per tile with blocks underneath (pieces from the model or the stair set).</summary>
        public List<StaticChange> StairRunPieces(Point a, Point b, out string info)
        {
            StaircaseSettings o = BrushSettings.Staircase;
            bool down = o.Downwards;
            Point from = down ? b : a, to = down ? a : b;
            Compass up = Staircase.DirectionOf(from, to);
            ushort stair, block;
            StairModel model = string.IsNullOrEmpty(o.Model) ? null : FindStairModel?.Invoke(o.Model);
            if (model != null)
            {
                (stair, block) = model.StepPieces(up);
            }
            else
            {
                stair = HexList.Parse(StairSet?.For(Staircase.Opposite(up))).FirstOrDefault();
                block = HexList.Parse(StairSet?.Block).FirstOrDefault();
            }
            int hb = block != 0 ? UoArtCache.StackHeight(block) : 0;
            int hs = UoArtCache.ItemHeight(stair);
            int h = hb >= 1 && hb <= 30 ? hb : hs >= 1 && hs <= 30 ? hs : 5;
            int n = Math.Max(Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
            int z = PlaceZ(a.X, a.Y);
            int baseZ = down ? z - n * h : z;
            info = $"{n + 1} " + L.T("Stufen") + $" " + L.T("hinauf nach") + $" {up}, " + L.T("Stufe") + $" {stair:x4}, Block {block:x4}, " + L.T("je") + $" {h} Z";
            return Staircase.Run(_map, from, to, baseZ, o.Width, stair, block, h);
        }

        private void PlaceStairRun(Point a, Point b)
        {
            List<StaticChange> ch = StairRunPieces(a, b, out string info);
            if (ch.Count == 0)
            {
                Message?.Invoke(this, L.T("Treppe ziehen: keine Stufe für diese Richtung – Modell mit Dreh-Tabelle oder Treppen-Set wählen."));
                return;
            }
            int before = _map.UndoCount;
            _map.Apply(ch);
            Finished(_map.UndoCount > before ? ch.Count : 0, L.T("Treppe gezogen") + ": " + info);
        }

        private void PlaceStairs(Point tile)
        {
            if (!string.IsNullOrEmpty(BrushSettings.Staircase.Model) && FindStairModel?.Invoke(BrushSettings.Staircase.Model) != null)
            {
                List<StaticChange> mc = StairPieces(tile);
                int b0 = _map.UndoCount;
                _map.Apply(mc);
                Finished(_map.UndoCount > b0 ? mc.Count : 0, L.T("Treppen-Modell") + $" '{BrushSettings.Staircase.Model}' × {BrushSettings.Staircase.Repeats}, " + L.T("hinauf") + $" {BrushSettings.Staircase.Up}");
                return;
            }
            StairSet set = StairSet;
            if (set == null || HexList.Parse(set.DownNorth + " " + set.DownEast + " " + set.DownSouth + " " + set.DownWest).Length == 0)
            {
                Message?.Invoke(this, L.T("Treppe: erst ein Treppen-Set wählen – oder eine Treppe von Hand bauen, auswählen und 'Treppe aus Auswahl lernen'."));
                return;
            }
            StaircaseSettings o = BrushSettings.Staircase;
            int z = PlaceZ(tile.X, tile.Y);
            List<StaticChange> changes = Staircase.Build(_map, tile, z, o, set);
            int before = _map.UndoCount;
            _map.Apply(changes);
            int height = o.Floors * o.Steps * o.StepHeight;
            Finished(_map.UndoCount > before ? changes.Count : 0, L.T("Treppe") + $" {o.Floors} × {o.Steps} " + L.T("Stufen") +
                     (o.Downwards ? $", z {z} → {z - height}" : $", z {z} → {z + height}"));
        }

        private void PlaceCellar(Rectangle r)
        {
            if (r.Width < 3 || r.Height < 3)
            {
                Message?.Invoke(this, L.T("Keller: mindestens 3×3 Felder aufziehen (Rand = Wände, innen Boden und Treppe)."));
                return;
            }
            CellarSettings o = BrushSettings.Cellar;
            WallSet walls = string.IsNullOrEmpty(o.WallSet) ? null : FindWallSet?.Invoke(o.WallSet);
            _map.BeginLand();
            _map.ApplyStatics(Builders.BuildCellar(_map, r, o, walls, StairSet, _rnd));
            int n = _map.EndLand();
            Finished(n, L.T("Keller") + $" {r.Width}×{r.Height}, " + L.T("Tiefe") + $" {o.Depth}");
            if (n > 0 && !UnderView)
            {
                Message?.Invoke(this, L.T("Keller gebaut – Ansicht → 'Unter der Karte' zeigt ihn von oben, Statics setzen dann auf Boden") + $" {BrushSettings.UnderOffset}.");
            }
        }

        private void PlaceDungeon(Rectangle r, bool mountainWall)
        {
            if (r.Width < 10 || r.Height < 10)
            {
                Message?.Invoke(this, L.T("Dungeon: mindestens 10×10 Felder aufziehen."));
                return;
            }
            DungeonSettings o = BrushSettings.Dungeon;
            if (mountainWall)
            {
                // Bergwall: the former "cave" dungeon – raised rock walls, rough edges
                o = new DungeonSettings
                {
                    Style = DungeonStyle.Cave, Layout = DungeonLayout.Corridors, Rooms = o.Rooms, RoomMin = o.RoomMin, RoomMax = o.RoomMax, CorridorWidth = o.CorridorWidth, Loops = o.Loops,
                    Rough = Math.Max(30, o.Rough), Seed = o.Seed, FixedFloor = o.FixedFloor, FloorZ = o.FloorZ, WallHeight = o.WallHeight,
                    FloorLand = o.FloorLand, WallLand = o.WallLand, ClearStatics = o.ClearStatics
                };
            }
            else if (o.Style == DungeonStyle.Cave)
            {
                Message?.Invoke(this, L.T("Dungeon: Art 'Bergwall' gehört zum Werkzeug Bergwall – hier wird 'Gebaut' benutzt."));
            }
            DungeonLook look = mountainWall || string.IsNullOrEmpty(o.Look) ? null : FindDungeonLook?.Invoke(o.Look);
            int seed = o.Seed != 0 ? o.Seed : _rnd.Next(1, int.MaxValue);
            DungeonPlan plan = Builders.Plan(r.Width, r.Height, o, new Random(seed));
            if (plan.Rooms.Count == 0)
            {
                Message?.Invoke(this, L.T("Dungeon: kein Raum passt – größer aufziehen oder kleinere Räume einstellen."));
                return;
            }
            WallSet walls = string.IsNullOrEmpty(o.WallSet) ? null : FindWallSet?.Invoke(o.WallSet);
            if (!mountainWall && walls == null && (string.IsNullOrEmpty(o.Look) || FindDungeonLook?.Invoke(o.Look) == null))
            {
                Message?.Invoke(this, L.T("Dungeon: für 'Gebaut' / 'Unter der Karte' ein Wand-Set wählen (Reiter Pinsel → Dungeon), sonst gibt es keine Wände."));
            }
            _map.BeginLand();
            if (look != null)
            {
                int ground = int.MaxValue;
                for (int y = r.Top; y < r.Bottom; y++)
                {
                    for (int x = r.Left; x < r.Right; x++)
                    {
                        ground = Math.Min(ground, _map.LandZ(x, y));
                    }
                }
                bool under = o.Style == DungeonStyle.UnderMap;
                int floorZ = o.FixedFloor ? o.FloorZ : under ? ground - Math.Max(1, o.Depth) : ground;
                _map.ApplyStatics(DungeonStyleTools.Build(_map, r, plan, look, floorZ, under, o.FillVoid, o.ClearStatics, _rnd));
            }
            else
            {
                DungeonSettings use = o;
                if (!mountainWall && o.Style == DungeonStyle.Cave)
                {
                    use = new DungeonSettings { Style = DungeonStyle.Built, FixedFloor = o.FixedFloor, FloorZ = o.FloorZ, Depth = o.Depth, FloorStatics = o.FloorStatics, ClearStatics = o.ClearStatics };
                }
                _map.ApplyStatics(Builders.BuildDungeon(_map, r, plan, use, walls, BrushSettings.Wall, CaveFloorIds, MountainTiles(), _rnd));
            }
            int n = _map.EndLand();
            Finished(n, (mountainWall ? L.T("Bergwall") : L.T("Dungeon")) + $": {plan.Rooms.Count} " + L.T("Räume") + $", " + L.T("Zufalls-Zahl") + $" {seed}" +
                        (look != null ? $", " + L.T("Stil") + $" '{look.Name}'" : string.Empty));
        }

        private void PlaceCourse(Point a, Point b)
        {
            if (a == b)
            {
                return;
            }
            int brushWidth = Math.Max(0, LandRadius) * 2 + 1;
            var core = new HashSet<int>();
            var pts = Builders.Course(new PointF(a.X + 0.5f, a.Y + 0.5f), new PointF(b.X + 0.5f, b.Y + 0.5f), BrushSettings.Course,
                new Random(BrushSettings.Course.Seed != 0 ? BrushSettings.Course.Seed : _previewSeed));
            _previewSeed = Environment.TickCount;
            _groundBefore.Clear();
            int level = _map.LandZ(a.X, a.Y) - BrushSettings.River.Depth;
            _map.BeginLand();
            var box = Rectangle.Empty;
            Point last = new Point(int.MinValue, 0);
            foreach ((float x, float y, float t) in pts)
            {
                int cx = (int)Math.Floor(x), cy = (int)Math.Floor(y);
                if (cx == last.X && cy == last.Y)
                {
                    continue;
                }
                last = new Point(cx, cy);
                double rr = BrushSettings.Course.WidthAt(t, brushWidth) / 2.0;
                CgiBrushes.River(_map, cx, cy, rr, level, BrushSettings.River, GroundBefore, _rnd, core);
                int reach = (int)Math.Ceiling(rr) + BrushSettings.River.BankWidth + 1;
                var dab = new Rectangle(cx - reach, cy - reach, reach * 2 + 1, reach * 2 + 1);
                box = box.IsEmpty ? dab : Rectangle.Union(box, dab);
            }
            if (BrushSettings.River.Transitions && LandTransitions != null && !box.IsEmpty)
            {
                LandTransitions.Apply(_map, box, _rnd);
            }
            _map.ApplyStatics(CgiBrushes.RestoreWater(_map, core, BrushSettings.River, _rnd));
            int n = _map.EndLand();
            Finished(n, L.T("Fluss-Lauf") + $" ({BrushSettings.Course.WidthAt(0, brushWidth):0}–{BrushSettings.Course.WidthAt(1, brushWidth):0} " + L.T("Tiles breit") + ")");
        }

        private void DrawUnderBanner(Graphics g)
        {
            if (!UnderView || _map == null)
            {
                return;
            }
            string text = "⬇ " + L.T("UNTER DER KARTE") + " – " + L.T("Bau-Ebene: Boden") + $" {BrushSettings.UnderOffset:+0;-0;0} Z · " +
                          L.T("Statics unter dem Boden werden darüber gezeigt · Static setzen / Wand bauen auf dieser Ebene");
            using var font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            SizeF sz = g.MeasureString(text, font);
            var r = new RectangleF(8, 8, Math.Min(Width - 16, sz.Width + 16), sz.Height + 8);
            using var back = new SolidBrush(Color.FromArgb(215, 120, 40, 160));
            g.FillRectangle(back, r);
            g.DrawString(text, font, Brushes.White, r.X + 8, r.Y + 4);
        }

        private void UpdateMeasure(Point a, Point b)
        {
            int dx = b.X - a.X, dy = b.Y - a.Y;
            int za = _map.LandZ(a.X, a.Y), zb = _map.LandZ(b.X, b.Y);
            double dist = Math.Sqrt(dx * dx + dy * dy);
            int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
            _measureText = L.T("Messen") + $": ({a.X}, {a.Y}) → ({b.X}, {b.Y}) · {dist:0.0} Tiles ({steps} " + L.T("Schritte") + $") · Δx {dx} Δy {dy} · z {za} → {zb} (Δz {zb - za:+0;-0;0})" +
                           (steps > 0 ? $" · {(zb - za) / (double)steps:+0.0;-0.0;0} z/Tile" : string.Empty);
        }

        // ------------------------------------------------------------------ edits

        /// <summary>Rotates (R) or mirrors (X / Y) the marked area with land and statics.</summary>
        public void TransformArea(int quarterTurns, bool mirrorX, bool mirrorY)
        {
            if (!NeedArea())
            {
                return;
            }
            HashSet<int> mask = _map.EditMask;
            _map.EditMask = null; // the whole rectangle turns
            (List<StaticChange> statics, List<LandChange> land) = CgiBrushes.TransformArea(_map, _area, quarterTurns, mirrorX, mirrorY);
            _map.Apply(statics, land);
            _map.EditMask = mask;
            if (quarterTurns % 2 != 0)
            {
                _area = new Rectangle(_area.X, _area.Y, _area.Height, _area.Width);
                ClearMask();
            }
            string what = quarterTurns != 0 ? L.T("Bereich gedreht (90°)") : mirrorX ? L.T("Bereich gespiegelt (links ↔ rechts)") : L.T("Bereich gespiegelt (oben ↔ unten)");
            Finished(land.Count + statics.Count, what);
        }

        public void TransformSelection(int quarterTurns, bool mirrorX, bool mirrorY)
        {
            if (_map == null || _selection.Count == 0)
            {
                return;
            }
            List<StaticChange> changes = CgiBrushes.TransformStatics(_map, _selection.ToList(), quarterTurns, mirrorX, mirrorY);
            Edit(changes, quarterTurns != 0 ? L.T("Auswahl gedreht (90°) – die Bilder der Statics drehen nicht mit") : L.T("Auswahl gespiegelt"));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void DropSelectionToGround()
        {
            if (_map == null || _selection.Count == 0)
            {
                return;
            }
            Edit(CgiBrushes.DropToGround(_map, _selection), L.T("Auswahl auf den Boden gesetzt"));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void DropAreaToGround()
        {
            if (!NeedArea())
            {
                return;
            }
            var list = new List<StaticItem>();
            for (int y = _area.Top; y < _area.Bottom; y++)
            {
                for (int x = _area.Left; x < _area.Right; x++)
                {
                    if (_mask == null || _mask.Contains(y * _map.Width + x))
                    {
                        list.AddRange(_map.StaticsAt(x, y));
                    }
                }
            }
            Edit(CgiBrushes.DropToGround(_map, list), L.T("Statics im Bereich auf den Boden gesetzt"));
        }

        /// <summary>Search &amp; replace inside the area / mask (or the whole map) as one undo step.</summary>
        public int ReplaceIds(bool wholeMap, ushort[] fromStatic, ushort[] toStatic, int onlyHue, int newHue, ushort[] fromLand, ushort[] toLand)
        {
            if (_map == null)
            {
                return 0;
            }
            Rectangle r = wholeMap || _area.IsEmpty ? new Rectangle(0, 0, _map.Width, _map.Height) : _area;
            HashSet<int> mask = wholeMap ? null : _mask;
            (List<StaticChange> statics, List<LandChange> land) = CgiBrushes.Replace(_map, r, mask == null ? null : (x, y) => mask.Contains(y * _map.Width + x),
                fromStatic, toStatic, onlyHue, newHue, fromLand, toLand, _rnd);
            _map.Apply(statics, land);
            Finished(statics.Count + land.Count, L.T("Ersetzt"));
            return statics.Count + land.Count;
        }

        /// <summary>Marks an area from outside (dungeon workshop).</summary>
        public void SetArea(Rectangle r)
        {
            _area = r;
            Invalidate();
        }

        /// <summary>Jumps the view to a place (bookmarks).</summary>
        public void JumpTo(int x, int y)
        {
            CenterOn(x, y);
            Invalidate();
        }

        // ------------------------------------------------------------------ drawing

        private PointF Middle(int x, int y)
        {
            PointF a = Corner(x, y), c = Corner(x + 1, y + 1);
            return new PointF((a.X + c.X) / 2, (a.Y + c.Y) / 2);
        }

        private void DrawToolOverlay(Graphics g)
        {
            if (_mask != null && _mask.Count > 0)
            {
                using var fill = new SolidBrush(Color.FromArgb(55, 0, 200, 255));
                int n = 0;
                double reach = (Width + Height) / (Half * _zoom) + 4;
                foreach (int i in _mask)
                {
                    int x = i % _map.Width, y = i / _map.Width;
                    if (Math.Abs(x - _camX) + Math.Abs(y - _camY) > reach)
                    {
                        continue;
                    }
                    g.FillPolygon(fill, LandPoly(x, y));
                    if (++n > 30000)
                    {
                        break;
                    }
                }
            }
            if (_points.Count > 0)
            {
                var pts = _points.Select(p => Middle(p.X, p.Y)).ToList();
                if (_map.InBounds(_hover.X, _hover.Y))
                {
                    pts.Add(Middle(_hover.X, _hover.Y));
                }
                using var pen = new Pen(Tool == IsoTool.Lasso ? Color.Cyan : Tool == IsoTool.Wall ? Color.SandyBrown : Color.Orange, 2) { DashStyle = DashStyle.Dash };
                if (Tool == IsoTool.Road && _points.Count >= 1)
                {
                    var all = _points.ToList();
                    if (_map.InBounds(_hover.X, _hover.Y))
                    {
                        all.Add(_hover);
                    }
                    List<PointF> curve = CgiBrushes.Spline(all);
                    if (curve.Count > 1)
                    {
                        PointF[] screen = curve.Select(c => Middle((int)c.X, (int)c.Y)).ToArray();
                        using var road = new Pen(Color.FromArgb(160, 255, 200, 0), Math.Max(2f, BrushSettings.Road.Width * Half * 0.9f)) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
                        g.DrawLines(road, screen);
                    }
                }
                if (pts.Count > 1)
                {
                    g.DrawLines(pen, pts.ToArray());
                    if (Tool == IsoTool.Lasso && pts.Count > 2)
                    {
                        using var close = new Pen(Color.FromArgb(120, Color.Cyan), 1) { DashStyle = DashStyle.Dot };
                        g.DrawLine(close, pts[pts.Count - 1], pts[0]);
                    }
                }
                foreach (PointF p in pts.Take(_points.Count))
                {
                    g.FillEllipse(Brushes.White, p.X - 3, p.Y - 3, 6, 6);
                    g.DrawEllipse(Pens.Black, p.X - 3, p.Y - 3, 6, 6);
                }
            }
            if (_dragStart.HasValue && IsRectTool(Tool))
            {
                Point a = _dragStart.Value, b = _dragEnd;
                var r = Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X) + 1, Math.Max(a.Y, b.Y) + 1);
                using var pen = new Pen(Tool == IsoTool.Cellar ? Color.MediumPurple : Color.OrangeRed, 2) { DashStyle = DashStyle.Dash };
                g.DrawPolygon(pen, AreaPoly(r));
                using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                g.DrawString($"{r.Width}×{r.Height}", font, Brushes.White, Middle(r.X, r.Y));
            }
            else if (_dragStart.HasValue && Tool == IsoTool.Course)
            {
                Point a = _dragStart.Value, b = _dragEnd;
                var pts = Builders.Course(new PointF(a.X + 0.5f, a.Y + 0.5f), new PointF(b.X + 0.5f, b.Y + 0.5f), BrushSettings.Course,
                        new Random(BrushSettings.Course.Seed != 0 ? BrushSettings.Course.Seed : _previewSeed))
                    .Select(p => Middle((int)Math.Floor(p.x), (int)Math.Floor(p.y))).ToArray();
                if (pts.Length > 1)
                {
                    // width grows from the start to the end like the course will
                    int bw = Math.Max(0, LandRadius) * 2 + 1;
                    for (int i = 1; i < pts.Length; i++)
                    {
                        double t = i / (double)(pts.Length - 1);
                        using var pen = new Pen(Color.FromArgb(150, 40, 120, 255), Math.Max(3f, (float)(BrushSettings.Course.WidthAt(t, bw) * Half * 1.4))) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                        g.DrawLine(pen, pts[i - 1], pts[i]);
                    }
                }
            }
            else if (_dragStart.HasValue)
            {
                Point a = _dragStart.Value, b = _dragEnd;
                if (Tool == IsoTool.Wall || Tool == IsoTool.Bridge)
                {
                    b = Math.Abs(b.X - a.X) >= Math.Abs(b.Y - a.Y) ? new Point(b.X, a.Y) : new Point(a.X, b.Y);
                    int n = Math.Max(Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
                    using var wall = new SolidBrush(Tool == IsoTool.Bridge ? Color.FromArgb(110, 160, 110, 60) : Color.FromArgb(110, 255, 120, 0));
                    bool alongX = b.Y == a.Y;
                    int width = Tool == IsoTool.Bridge ? Math.Max(1, BrushSettings.Bridge.Width) : 1;
                    int lo = -(width - 1) / 2, hi = width / 2;
                    for (int k = 0; k <= n; k++)
                    {
                        for (int w = lo; w <= hi; w++)
                        {
                            int x = a.X + Math.Sign(b.X - a.X) * k + (alongX ? 0 : w), y = a.Y + Math.Sign(b.Y - a.Y) * k + (alongX ? w : 0);
                            if (_map.InBounds(x, y))
                            {
                                g.FillPolygon(wall, LandPoly(x, y));
                            }
                        }
                    }
                }
                else
                {
                    using var pen = new Pen(Tool == IsoTool.Ramp ? Color.LimeGreen : Color.White, Tool == IsoTool.Ramp ? Math.Max(2f, (LandRadius * 2 + 1) * Half * 0.8f) : 2f)
                    {
                        StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor
                    };
                    if (Tool == IsoTool.Ramp)
                    {
                        pen.Color = Color.FromArgb(120, Color.LimeGreen);
                    }
                    g.DrawLine(pen, Middle(a.X, a.Y), Middle(b.X, b.Y));
                }
            }
            if (Tool == IsoTool.Measure && (_dragStart.HasValue || _measureA.HasValue) && _measureText != null)
            {
                Point a = _dragStart ?? _measureA.Value, b = _dragStart.HasValue ? _dragEnd : _measureB;
                PointF pa = Middle(a.X, a.Y), pb = Middle(b.X, b.Y);
                using var pen = new Pen(Color.Yellow, 2) { DashStyle = DashStyle.Dash };
                g.DrawLine(pen, pa, pb);
                using var font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                string text = _measureText.Substring(_measureText.IndexOf(':') + 1).Trim();
                SizeF sz = g.MeasureString(text, font);
                var pos = new PointF((pa.X + pb.X) / 2 - sz.Width / 2, (pa.Y + pb.Y) / 2 - sz.Height - 6);
                using var back = new SolidBrush(Color.FromArgb(200, 20, 20, 20));
                g.FillRectangle(back, pos.X - 3, pos.Y - 1, sz.Width + 6, sz.Height + 2);
                g.DrawString(text, font, Brushes.Yellow, pos);
            }
            if (_moveDrag && _moveTile != _moveStart)
            {
                int dx = _moveTile.X - _moveStart.X, dy = _moveTile.Y - _moveStart.Y;
                using var pen = new Pen(Color.Yellow, 1.5f) { DashStyle = DashStyle.Dot };
                foreach (StaticItem s in _selection.Take(600))
                {
                    if (_map.InBounds(s.X + dx, s.Y + dy))
                    {
                        g.DrawPolygon(pen, LandPoly(s.X + dx, s.Y + dy));
                    }
                }
            }
        }

        /// <summary>Rubber band in screen pixels (drawn without the zoom transform).</summary>
        private void DrawBand(Graphics g)
        {
            if (!_bandDrag)
            {
                return;
            }
            Rectangle band = Rectangle.FromLTRB(Math.Min(_bandStart.X, _bandEnd.X), Math.Min(_bandStart.Y, _bandEnd.Y), Math.Max(_bandStart.X, _bandEnd.X), Math.Max(_bandStart.Y, _bandEnd.Y));
            using var fill = new SolidBrush(Color.FromArgb(40, 255, 255, 0));
            using var pen = new Pen(Color.Yellow) { DashStyle = DashStyle.Dash };
            g.FillRectangle(fill, band);
            g.DrawRectangle(pen, band);
        }

        /// <summary>Key and mouse help of the new tools (null = not a new tool).</summary>
        private string NewToolHelp(IsoTool t) => t switch
        {
            IsoTool.Scatter => L.T("Streu-Pinsel: Ziehen verteilt das Set (Reiter Pinsel) – Dichte, Abstand, Boden-Filter · Strg+Ziehen entfernt Statics des Sets · Pinsel oben · ein Zug = ein Rückgängig-Schritt"),
            IsoTool.Eraser => L.T("Radierer: Ziehen löscht alle Statics im Pinsel (Größe oben / [ ]) · Strg = Land zur Leere · Einstellungen im Reiter Pinsel"),
            IsoTool.Motif => BrushSettings.MotifDraw.Mode switch
            {
                MotifMode.Points => L.T("Muster Punkt zu Punkt: klicken, klicken … · Doppelklick / Rechtsklick / Enter = fertig · Rücktaste = letzter Punkt weg · Esc = abbrechen · an Knicken das Eckstück"),
                MotifMode.Radius => L.T("Muster streuen: von der Mitte nach außen ziehen = Radius · Zufalls-Varianten nach Gewicht, Mindest-Abstand, Dichte oben in der Leiste"),
                _ => L.T("Muster: Klick = einmal in der Richtung (R dreht) · Ziehen = wiederholt in Zieh-Richtung mit der Version dieser Richtung · Höhe, Etagen, Abstand oben in der Leiste")
            },
            IsoTool.Stairs => L.T("Treppe: Klick = Modell / Treppe einmal (R dreht) · Ziehen = je Feld eine Stufe mit Blöcken darunter, in Zieh-Richtung hinauf ('Nach unten bauen' = hinunter) · Vorschau am Mauszeiger"),
            IsoTool.HueBrush => L.T("Hue-Pinsel: Ziehen färbt alle Statics im Pinsel mit der Hue (Reiter Items) · Strg = Hue 0 (Original)"),
            IsoTool.River => L.T("Fluss: Ziehen gräbt ein Bett – Wasser-Tiles auf 'Tiefe' unter dem Startpunkt, weiches Ufer, danach Übergänge (Reiter Pinsel → Fluss) · Pinsel oben = Breite"),
            IsoTool.Plateau => L.T("Plateau: Ziehen flacht auf die Höhe unter dem ersten Klick ab (oder 'Höhe ='), Art Terrassen = Stufen · Härte für weiche Ränder (Reiter Pinsel)"),
            IsoTool.Noise => L.T("Unebenheit: Ziehen macht das Gelände leicht wellig (± Stärke, Größe der Wellen) – jedes Feld einmal pro Zug"),
            IsoTool.Variants => L.T("Varianten: Ziehen mischt die Tiles derselben Farbe der Farbtabelle neu – gegen Kachel-Muster"),
            IsoTool.TransitionBrush => L.T("Übergänge: Ziehen berechnet die Dragon-Übergänge (Gras → Sand …) nur unter dem Pinsel neu"),
            IsoTool.Wall => L.T("Wand / Zaun: von einem Ende zum anderen ziehen – West-Ost oder Nord-Süd, Pfosten an den Enden, Stockwerke (Reiter Pinsel → Wand)"),
            IsoTool.Road => L.T("Weg: Punkte klicken, daraus wird eine weiche Kurve mit Breite und geglätteter Höhe · Doppelklick / Rechtsklick / Enter = bauen · Rücktaste = Punkt weg · Esc = abbrechen"),
            IsoTool.Ramp => L.T("Rampe: von unten nach oben ziehen – gleichmäßige Steigung von der Höhe am Start zur Höhe am Ende · Strg = bis 'Höhe =' · Breite = Pinsel oben"),
            IsoTool.Measure => L.T("Messen: ziehen zeigt Abstand, Schritte, Höhenunterschied und Steigung · Esc = weg"),
            IsoTool.Lasso => L.T("Lasso: Punkte um eine Fläche klicken · Doppelklick / Rechtsklick / Enter = schließen · Umschalt = dazu, Strg = weg · danach wirken alle Werkzeuge nur darin, Bereich-Tasten F K J W G R X Y E D · Esc = aufheben"),
            IsoTool.Bridge => L.T("Brücke: von Ufer zu Ufer ziehen (West-Ost oder Nord-Süd) – Bretter, Geländer, Pfosten, Bogen; Höhe = höheres Ende + Z + (Reiter Pinsel → Brücke)"),
            IsoTool.Cellar => L.T("Keller: Rechteck aufziehen – Land wird unsichtbar (NoDraw 0002) auf Boden − Tiefe, Kellerboden + Wände am Rand + Treppe hinunter (Richtung), Boden oben (Reiter Pinsel → Keller) · Ansicht 'Unter der Karte' zum Weiterbauen"),
            IsoTool.Dungeon => L.T("Dungeon: Rechteck aufziehen – Räume + Gänge aus Boden-Textur und Wand-Statics, am besten mit einem gelernten Stil (Bereich einer echten Dungeon-Karte → 'Stil aus Bereich lernen') · gleiche Zufalls-Zahl = gleicher Dungeon"),
            IsoTool.MountainWall => L.T("Bergwall: Rechteck aufziehen – ein Labyrinth aus hohen Fels-Wänden (Land), Räume und Gänge dazwischen"),
            IsoTool.Course => L.T("Fluss-Lauf: von der Quelle zur Mündung ziehen – natürlich geschlängelt, breiter zum Ende, mit den Fluss-Einstellungen (Reiter Pinsel)"),
            IsoTool.MagicWand => L.T("Zauberstab: Klick wählt zusammenhängendes gleiches Land (Tile / Familie / Höhe, Reiter Pinsel) · Umschalt = dazu, Strg = weg · danach wirken alle Werkzeuge nur darin · Esc = aufheben"),
            _ => null
        };
    }
}
