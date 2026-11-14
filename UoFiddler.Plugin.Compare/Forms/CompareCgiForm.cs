/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * CGI-Vergleich: zeigt einen Bereich von Map1 (Original) und Map2 (Custom)
 * nebeneinander in der Client-Ansicht (isometrisch, mit Statics) und kopiert
 * Land und/oder Statics von einer Karte in die andere – an dieselbe oder eine
 * andere Position. Änderungen bleiben im Speicher (Undo/Redo) bis "Speichern".
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.MapViewer;
using UoFiddler.Controls.MapViewer.Build;
using UoFiddler.Controls.MapViewer.Editor;

namespace UoFiddler.Plugin.Compare.Forms
{
    /// <summary>
    /// Two isometric views (Map1 left, Map2 right) of the same map area with difference markers,
    /// synchronized scrolling/zoom and copying of land / statics between the maps.
    /// </summary>
    public sealed class CompareCgiForm : Form
    {
        /// <summary>largest area (cells per side) that is opened</summary>
        public const int MaxAreaSize = 512;

        private static readonly Color BarBack = Color.FromArgb(45, 45, 48);
        private static readonly Color InputBack = Color.FromArgb(62, 62, 66);

        private sealed class Side
        {
            public Map Map;
            public string Name;
            public RegionEditSession Session;
            public RegionData Data;
            public IsoRenderer Renderer;
            public IsoViewControl View;
            public Label Header;
            public Label Info;
        }

        private readonly Side _left;   // Map1 (original)
        private readonly Side _right;  // Map2 (custom)
        private readonly ArtPixelCache _art = new ArtPixelCache();
        private readonly IsoRenderSettings _settings = new IsoRenderSettings();
        private readonly Action<Rectangle?> _mapsChanged;

        // sides of the operations for undo / redo (the sessions hold the actual steps)
        private readonly List<Side> _undoOrder = new List<Side>();
        private readonly List<Side> _redoOrder = new List<Side>();

        private Rectangle _area;
        private Rectangle? _subArea;
        private HashSet<Point> _diffCells = new HashSet<Point>();
        private List<Point> _diffList = new List<Point>();
        private int _diffIndex = -1;
        private bool _syncing;
        private bool _settingTarget;
        private bool _closeSilently;

        private readonly ToolTip _tip = new ToolTip();

        // view bar
        private CheckBox _chkStatics;
        private CheckBox _chkDiff;
        private CheckBox _chkGrid;
        private CheckBox _chkSync;
        private NumericUpDown _numMaxZ;
        private Button _btnNextDiff;

        // copy bar
        private ComboBox _cmbDirection;
        private CheckBox _chkLand;
        private CheckBox _chkCopyStatics;
        private ComboBox _cmbExisting;
        private CheckBox _chkSelect;

        // target bar
        private RadioButton _rbSame;
        private RadioButton _rbOther;
        private NumericUpDown _numX;
        private NumericUpDown _numY;
        private CheckBox _chkPick;
        private NumericUpDown _numZ;
        private CheckBox _chkAdjust;
        private CheckBox _chkConfirm;
        private Button _btnCopy;
        private Button _btnUndo;
        private Button _btnRedo;
        private Button _btnSave1;
        private Button _btnSave2;

        private ToolStripStatusLabel _lblStatus;
        private ToolStripStatusLabel _lblChanges;

        /// <param name="map1">Map1 (original)</param>
        /// <param name="session1">edit session of Map1 (kept by the caller, so changes survive closing the window)</param>
        /// <param name="map2">Map2 (custom)</param>
        /// <param name="session2">edit session of Map2</param>
        /// <param name="area">map area that is shown</param>
        /// <param name="mapsChanged">called after a map was changed in memory (changed area or null = everything)</param>
        public CompareCgiForm(Map map1, string name1, RegionEditSession session1,
                              Map map2, string name2, RegionEditSession session2,
                              Rectangle area, Action<Rectangle?> mapsChanged)
        {
            _left = new Side { Map = map1, Name = name1, Session = session1 ?? new RegionEditSession(map1) };
            _right = new Side { Map = map2, Name = name2, Session = session2 ?? new RegionEditSession(map2) };
            _mapsChanged = mapsChanged;

            BuildUi();
            LoadArea(area);
        }

        #region UI

        private void BuildUi()
        {
            Text = "CGI-Vergleich";
            Icon = Options.GetFiddlerIcon();
            Size = new Size(1400, 860);
            MinimumSize = new Size(900, 520);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            KeyPreview = true;

            // ── view bar ──────────────────────────────────────────────
            FlowLayoutPanel barView = Bar();
            _chkStatics = Check("Statics", true, "Statics in beiden Ansichten ein-/ausblenden");
            _chkStatics.CheckedChanged += (s, e) => { _settings.ShowStatics = _chkStatics.Checked; RefreshViews(); };
            _chkDiff = Check("Unterschiede markieren", true, "Zellen, die sich zwischen Map1 und Map2 unterscheiden, rot markieren");
            _chkDiff.CheckedChanged += (s, e) => ApplyOverlays();
            _chkGrid = Check("Raster", false, "Raster über den Zellen anzeigen");
            _chkGrid.CheckedChanged += (s, e) =>
            {
                _left.View.ShowGrid = _right.View.ShowGrid = _chkGrid.Checked;
                _left.View.Invalidate();
                _right.View.Invalidate();
            };
            _chkSync = Check("Ansichten synchron", true, "Verschieben und Zoomen in einer Ansicht bewegt die andere mit");
            _chkSync.CheckedChanged += (s, e) => { if (_chkSync.Checked) { SyncFrom(_left); } };
            _numMaxZ = Num(-128, 127, 127, 55);
            _tip.SetToolTip(_numMaxZ, "Statics über dieser Höhe ausblenden (z. B. um unter Dächer zu schauen)");
            _numMaxZ.ValueChanged += (s, e) => { _settings.MaxZ = (int)_numMaxZ.Value; RefreshViews(); };
            _btnNextDiff = Btn("Nächster Unterschied ►", (s, e) => JumpToNextDiff(), "Zur nächsten unterschiedlichen Zelle springen");

            barView.Controls.AddRange(new Control[]
            {
                _chkStatics, _chkDiff, _chkGrid, _chkSync, Lbl("Max. Z:"), _numMaxZ, Gap(),
                Btn("Zoom −", (s, e) => ZoomBoth(false), null),
                Btn("Zoom +", (s, e) => ZoomBoth(true), null),
                Btn("Einpassen", (s, e) => FitBoth(), "Ganzen Bereich anzeigen"),
                Gap(), _btnNextDiff
            });

            // ── copy bar ──────────────────────────────────────────────
            FlowLayoutPanel barCopy = Bar();
            _cmbDirection = Combo(220);
            _cmbDirection.Items.AddRange(new object[] { $"{_left.Name}  →  {_right.Name}", $"{_right.Name}  →  {_left.Name}" });
            _cmbDirection.SelectedIndex = 0;
            _cmbDirection.SelectedIndexChanged += (s, e) => { UpdateDirectionNames(); ApplyOverlays(); };
            _chkLand = Check("Land (Boden)", true, "Land-Tiles (ID und Höhe) kopieren");
            _chkCopyStatics = Check("Statics mitkopieren", true, "Statics des Bereichs mitkopieren");
            _chkCopyStatics.CheckedChanged += (s, e) => _cmbExisting.Enabled = _chkCopyStatics.Checked;
            _cmbExisting = Combo(230);
            _cmbExisting.Items.AddRange(new object[]
            {
                "Ziel-Statics im Bereich ersetzen",
                "nur Pflanzen im Ziel entfernen",
                "Ziel-Statics behalten (hinzufügen)"
            });
            _cmbExisting.SelectedIndex = 0;
            _tip.SetToolTip(_cmbExisting, "Was mit den vorhandenen Statics im Zielbereich passiert, wenn Statics mitkopiert werden");
            _chkSelect = Check("Teilbereich wählen", false, "Mit der linken Maustaste in einer Ansicht einen Teilbereich aufziehen (verschieben dann mit der mittleren Maustaste)");
            _chkSelect.Appearance = Appearance.Button;
            _chkSelect.FlatStyle = FlatStyle.Flat;
            _chkSelect.BackColor = InputBack;
            _chkSelect.CheckedChanged += (s, e) => UpdateMouseModes();

            barCopy.Controls.AddRange(new Control[]
            {
                Lbl("Kopieren:"), _cmbDirection, _chkLand, _chkCopyStatics, _cmbExisting, Gap(),
                _chkSelect,
                Btn("Ganzer Bereich", (s, e) => { _subArea = null; ApplyOverlays(); }, "Teilauswahl aufheben – der ganze geöffnete Bereich wird kopiert")
            });

            // ── target bar ────────────────────────────────────────────
            FlowLayoutPanel barTarget = Bar();
            _rbSame = new RadioButton { Text = "gleiche Position", Checked = true, AutoSize = true, Margin = new Padding(3, 6, 3, 3) };
            _rbOther = new RadioButton { Text = "andere Position:", AutoSize = true, Margin = new Padding(3, 6, 3, 3) };
            _rbSame.CheckedChanged += (s, e) => ApplyOverlays();
            int maxW = Math.Max(_left.Map.Width, _right.Map.Width);
            int maxH = Math.Max(_left.Map.Height, _right.Map.Height);
            _numX = Num(0, maxW - 1, 0, 65);
            _numY = Num(0, maxH - 1, 0, 65);
            _numX.ValueChanged += (s, e) => OnTargetValueChanged();
            _numY.ValueChanged += (s, e) => OnTargetValueChanged();
            _chkPick = Check("Ziel anklicken", false, "Danach in der Ziel-Ansicht auf die Zelle klicken, an der die linke obere Ecke landen soll");
            _chkPick.Appearance = Appearance.Button;
            _chkPick.FlatStyle = FlatStyle.Flat;
            _chkPick.BackColor = InputBack;
            _chkPick.CheckedChanged += (s, e) => UpdateMouseModes();
            _numZ = Num(-127, 127, 0, 50);
            _tip.SetToolTip(_numZ, "Zusätzlicher Höhenversatz für Land und Statics");
            _chkAdjust = Check("Höhe an Ziel anpassen", false, "Höhe an den Rand des Zielbereichs anpassen (mit weichem Übergang). Statics folgen dem Boden.");
            _chkConfirm = Check("Nachfragen", true, "Vor dem Kopieren eine Zusammenfassung anzeigen");
            _btnCopy = Btn("Kopieren ▶", (s, e) => DoCopy(), "Ausgewählten Bereich in die Zielkarte kopieren (nur im Speicher, Strg+Z = rückgängig)");
            _btnCopy.BackColor = Color.FromArgb(0, 122, 204);
            _btnCopy.Font = new Font(Font, FontStyle.Bold);
            _btnUndo = Btn("↶ Rückgängig", (s, e) => DoUndo(), "Letzten Kopiervorgang rückgängig machen (Strg+Z)");
            _btnRedo = Btn("↷ Wiederholen", (s, e) => DoRedo(), "Wiederholen (Strg+Y)");
            _btnSave1 = Btn("Map1 speichern…", (s, e) => SaveSide(_left), "Änderungen an Map1 in Dateien schreiben");
            _btnSave2 = Btn("Map2 speichern…", (s, e) => SaveSide(_right), "Änderungen an Map2 in Dateien schreiben");

            barTarget.Controls.AddRange(new Control[]
            {
                Lbl("Ziel:"), _rbSame, _rbOther, Lbl("X"), _numX, Lbl("Y"), _numY, _chkPick,
                Btn("Ansicht zum Ziel", (s, e) => GoToTarget(), "Den Zielbereich in beiden Ansichten öffnen"),
                Gap(), Lbl("Z ±"), _numZ, _chkAdjust, Gap(), _chkConfirm, _btnCopy, _btnUndo, _btnRedo, Gap(), _btnSave1, _btnSave2
            });

            // ── views ─────────────────────────────────────────────────
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = Color.FromArgb(60, 60, 64),
                SplitterWidth = 5
            };
            BuildSide(_left, split.Panel1);
            BuildSide(_right, split.Panel2);
            split.SplitterMoved += (s, e) => SyncFrom(_left);
            Shown += (s, e) =>
            {
                split.SplitterDistance = split.Width / 2;
                FitBoth();
            };

            // ── status ────────────────────────────────────────────────
            var status = new StatusStrip { BackColor = BarBack, ForeColor = Color.White, SizingGrip = true };
            _lblStatus = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.LightGray };
            _lblChanges = new ToolStripStatusLabel { ForeColor = Color.FromArgb(255, 180, 60) };
            status.Items.Add(_lblStatus);
            status.Items.Add(_lblChanges);

            // dock order: the control added last is docked first (top-most)
            Controls.Add(split);
            Controls.Add(status);
            Controls.Add(barTarget);
            Controls.Add(barCopy);
            Controls.Add(barView);

            UpdateDirectionNames();
        }

        private void BuildSide(Side side, Control parent)
        {
            side.View = new IsoViewControl { Dock = DockStyle.Fill };
            side.Header = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0),
                BackColor = side == _left ? Color.FromArgb(40, 70, 110) : Color.FromArgb(110, 70, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                AutoEllipsis = true
            };
            side.Info = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(6, 2, 2, 2),
                BackColor = Color.FromArgb(36, 36, 40),
                ForeColor = Color.Gainsboro,
                Font = new Font("Consolas", 8.5f),
                AutoEllipsis = true
            };

            side.View.ViewChanged += (s, e) => SyncFrom(side);
            side.View.HoverChanged += (s, e) => OnHover(side);
            side.View.AreaSelected += (s, e) => OnAreaSelected(side);
            side.View.CellClicked += (s, e) => OnCellClicked(side, e.Cell);

            parent.Controls.Add(side.View);
            parent.Controls.Add(side.Info);
            parent.Controls.Add(side.Header);
        }

        private static FlowLayoutPanel Bar() => new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            BackColor = BarBack,
            ForeColor = Color.White,
            Padding = new Padding(3, 2, 3, 2)
        };

        private CheckBox Check(string text, bool on, string tip)
        {
            var c = new CheckBox { Text = text, Checked = on, AutoSize = true, Margin = new Padding(4, 6, 4, 3), ForeColor = Color.White };
            if (tip != null)
            {
                _tip.SetToolTip(c, tip);
            }

            return c;
        }

        private Button Btn(string text, EventHandler click, string tip)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 26,
                BackColor = InputBack,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(3, 2, 3, 2),
                Padding = new Padding(4, 0, 4, 0)
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 96);
            b.Click += click;
            if (tip != null)
            {
                _tip.SetToolTip(b, tip);
            }

            return b;
        }

        private static Label Lbl(string text) => new Label
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(6, 7, 2, 3),
            ForeColor = Color.LightGray
        };

        private static Control Gap() => new Label { Width = 1, Height = 22, BackColor = Color.FromArgb(80, 80, 86), Margin = new Padding(6, 3, 6, 3) };

        private static ComboBox Combo(int width) => new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = width,
            FlatStyle = FlatStyle.Flat,
            BackColor = InputBack,
            ForeColor = Color.White,
            Margin = new Padding(3, 3, 3, 3)
        };

        private static NumericUpDown Num(int min, int max, int value, int width) => new NumericUpDown
        {
            Minimum = min,
            Maximum = Math.Max(min, max),
            Value = Math.Max(min, Math.Min(Math.Max(min, max), value)),
            Width = width,
            BackColor = InputBack,
            ForeColor = Color.White,
            Margin = new Padding(3, 3, 3, 3)
        };

        #endregion

        #region Area / loading

        /// <summary>Opens another map area in both views (unsaved changes stay in memory)</summary>
        public void LoadArea(Rectangle area)
        {
            int w = Math.Min(_left.Map.Width, _right.Map.Width);
            int h = Math.Min(_left.Map.Height, _right.Map.Height);
            area.Intersect(new Rectangle(0, 0, w, h));
            if (area.Width <= 0 || area.Height <= 0)
            {
                return;
            }

            area.Width = Math.Min(area.Width, MaxAreaSize);
            area.Height = Math.Min(area.Height, MaxAreaSize);

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _area = area;
                _subArea = null;
                _diffIndex = -1;
                LoadSide(_left);
                LoadSide(_right);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if (_rbSame.Checked)
            {
                SetTargetFields(area.Location);
            }

            Text = $"CGI-Vergleich – {area.X},{area.Y} bis {area.Right - 1},{area.Bottom - 1}  ({area.Width} × {area.Height})";
            UpdateDirectionNames();
            ComputeDiffs();
            ApplyOverlays();
            UpdateChangeInfo();

            if (Visible)
            {
                FitBoth();
            }
        }

        private void LoadSide(Side side)
        {
            side.Map.ResetCache();
            var data = new RegionData(side.Map, _area);
            data.Load(_art);
            var renderer = new IsoRenderer(data, _art, _settings);

            IsoRenderer old = side.Renderer;
            side.Data = data;
            side.Renderer = renderer;
            side.View.Renderer = renderer;
            side.View.SelectedCell = null;
            side.View.SelectedDrawable = -1;
            old?.Dispose();
        }

        /// <summary>Reads the cells of one side again (after a change in memory)</summary>
        private void ReloadSide(Side side)
        {
            side.Map.ResetCache();
            side.Data.Load(_art);
            side.View.SelectedDrawable = -1;
            side.View.RefreshRender();
        }

        private void RefreshViews()
        {
            _left.View.RefreshRender();
            _right.View.RefreshRender();
        }

        #endregion

        #region Differences

        private void ComputeDiffs()
        {
            var set = new HashSet<Point>();
            if (_left.Data != null && _right.Data != null)
            {
                foreach (RegionCell a in _left.Data.Cells)
                {
                    RegionCell b = _right.Data.GetCell(a.X, a.Y);
                    if (b == null || CellsDiffer(a, b))
                    {
                        set.Add(new Point(a.X, a.Y));
                    }
                }
            }

            _diffCells = set;
            _diffList = set.OrderBy(p => p.Y).ThenBy(p => p.X).ToList();
            _diffIndex = Math.Min(_diffIndex, _diffList.Count - 1);
            _btnNextDiff.Enabled = _diffList.Count > 0;
        }

        private static bool CellsDiffer(RegionCell a, RegionCell b)
        {
            if (a.LandId != b.LandId || a.LandZ != b.LandZ || a.Statics.Count != b.Statics.Count)
            {
                return true;
            }

            for (int i = 0; i < a.Statics.Count; i++)
            {
                RegionStatic sa = a.Statics[i], sb = b.Statics[i];
                if (sa.Id != sb.Id || sa.Z != sb.Z || sa.Hue != sb.Hue)
                {
                    return true;
                }
            }

            return false;
        }

        private void JumpToNextDiff()
        {
            if (_diffList.Count == 0)
            {
                return;
            }

            _diffIndex = (_diffIndex + 1) % _diffList.Count;
            Point p = _diffList[_diffIndex];
            _left.View.CenterOnCell(p.X, p.Y);
            if (!_chkSync.Checked)
            {
                _right.View.CenterOnCell(p.X, p.Y);
            }

            _left.View.SelectedCell = p;
            _right.View.SelectedCell = p;
            _left.View.Invalidate();
            _right.View.Invalidate();
            _left.Info.Text = Describe(_left, p);
            _right.Info.Text = Describe(_right, p);
            SetStatus($"Unterschied {_diffIndex + 1} / {_diffList.Count} bei {p.X},{p.Y}");
        }

        #endregion

        #region Overlays / view sync / mouse

        private Side Source => _cmbDirection.SelectedIndex == 1 ? _right : _left;

        private Side Target => _cmbDirection.SelectedIndex == 1 ? _left : _right;

        private static Side Other(Side side, Side a, Side b) => side == a ? b : a;

        private Rectangle SourceArea => _subArea ?? _area;

        private Point TargetPosition(Rectangle source)
        {
            return _rbSame.Checked ? source.Location : new Point((int)_numX.Value, (int)_numY.Value);
        }

        private void UpdateDirectionNames()
        {
            if (_cmbDirection == null || _left.Header == null)
            {
                return;
            }

            _left.Header.Text = $"  {_left.Name}" + (Source == _left ? "   [Quelle]" : "   [Ziel]");
            _right.Header.Text = $"  {_right.Name}" + (Source == _right ? "   [Quelle]" : "   [Ziel]");
        }

        /// <summary>Difference markers, source area and target area in both views</summary>
        private void ApplyOverlays()
        {
            if (_left.View == null || _left.Data == null)
            {
                return;
            }

            ICollection<Point> diff = _chkDiff.Checked && _diffCells.Count > 0 ? _diffCells : null;
            _left.View.ObstacleCells = diff;
            _right.View.ObstacleCells = diff;

            Rectangle source = SourceArea;
            Point target = TargetPosition(source);
            Source.View.SelectedArea = source;
            Target.View.SelectedArea = new Rectangle(target, source.Size);
            _left.View.Invalidate();
            _right.View.Invalidate();

            UpdateDirectionNames();
            SetStatus($"Quelle: {Source.Name} {source.X},{source.Y} ({source.Width} × {source.Height})  →  " +
                      $"Ziel: {Target.Name} {target.X},{target.Y}" +
                      (_area.Contains(new Rectangle(target, source.Size)) ? string.Empty : "  (außerhalb der Ansicht)") +
                      $"   |   Unterschiede im Bereich: {_diffCells.Count} Zellen");
        }

        /// <summary>Fills the X/Y fields without switching to "other position"</summary>
        private void SetTargetFields(Point p)
        {
            _settingTarget = true;
            try
            {
                _numX.Value = Math.Max(_numX.Minimum, Math.Min(_numX.Maximum, p.X));
                _numY.Value = Math.Max(_numY.Minimum, Math.Min(_numY.Maximum, p.Y));
            }
            finally
            {
                _settingTarget = false;
            }

            ApplyOverlays();
        }

        private void OnTargetValueChanged()
        {
            if (_settingTarget)
            {
                return;
            }

            if (!_rbOther.Checked)
            {
                _rbOther.Checked = true; // typing a coordinate means "other position"
            }

            ApplyOverlays();
        }

        private void UpdateMouseModes()
        {
            bool select = _chkSelect.Checked && !_chkPick.Checked;
            _left.View.AreaSelectMode = select;
            _right.View.AreaSelectMode = select;
            if (_chkPick.Checked)
            {
                SetStatus($"Ziel wählen: in der Ansicht \"{Target.Name}\" auf die Zelle klicken, an der die linke obere Ecke landen soll (Esc = abbrechen)");
            }
            else
            {
                ApplyOverlays();
            }
        }

        private void SyncFrom(Side side)
        {
            if (_syncing || _chkSync == null || !_chkSync.Checked || side.Data == null)
            {
                return;
            }

            Side other = Other(side, _left, _right);
            if (other.Data == null)
            {
                return;
            }

            _syncing = true;
            try
            {
                // the same map cell has the same canvas position up to the different origins of both layouts
                PointF offset = side.View.ViewOffset;
                other.View.SetView(side.View.Zoom, new PointF(
                    offset.X - side.Data.OriginX + other.Data.OriginX,
                    offset.Y - side.Data.OriginY + other.Data.OriginY));
            }
            finally
            {
                _syncing = false;
            }
        }

        private void ZoomBoth(bool zoomIn)
        {
            foreach (Side side in _chkSync.Checked ? new[] { _left } : new[] { _left, _right })
            {
                if (zoomIn)
                {
                    side.View.ZoomIn();
                }
                else
                {
                    side.View.ZoomOut();
                }
            }
        }

        private void FitBoth()
        {
            _left.View.ZoomToFit();
            if (_chkSync.Checked)
            {
                SyncFrom(_left);
            }
            else
            {
                _right.View.ZoomToFit();
            }
        }

        private void OnHover(Side side)
        {
            RegionCell cell = side.View.HoverCell;
            Point? p = cell == null ? (Point?)null : new Point(cell.X, cell.Y);
            Side other = Other(side, _left, _right);

            // mirror the cell under the mouse into the other view
            side.View.SelectedCell = null;
            other.View.SelectedCell = p;
            other.View.Invalidate();

            if (p.HasValue)
            {
                _left.Info.Text = Describe(_left, p.Value);
                _right.Info.Text = Describe(_right, p.Value);
            }

            if (_chkPick.Checked && side == Target && p.HasValue)
            {
                Target.View.SelectedArea = new Rectangle(p.Value, SourceArea.Size);
                Target.View.Invalidate();
            }
        }

        private string Describe(Side side, Point p)
        {
            RegionCell c = side.Data?.GetCell(p.X, p.Y);
            if (c == null)
            {
                return $"{p.X},{p.Y}: außerhalb";
            }

            var sb = new StringBuilder();
            sb.Append(_diffCells.Contains(p) ? "≠ " : "  ");
            sb.Append($"{p.X},{p.Y}  Land 0x{c.LandId:X4} z{c.LandZ}  {c.LandName}");
            sb.AppendLine();
            sb.Append($"  Statics ({c.Statics.Count})");
            if (c.Statics.Count > 0)
            {
                sb.Append(": ");
                sb.Append(string.Join(", ", c.Statics.Take(8).Select(s =>
                    $"0x{s.Id:X4} z{s.Z}" + (s.Hue != 0 ? $" h{s.Hue}" : string.Empty))));
                if (c.Statics.Count > 8)
                {
                    sb.Append(", …");
                }
            }

            return sb.ToString();
        }

        private void OnAreaSelected(Side side)
        {
            Rectangle? selected = side.View.SelectedArea;
            if (!selected.HasValue)
            {
                return;
            }

            Rectangle a = Rectangle.Intersect(selected.Value, _area);
            _subArea = a.Width > 0 && a.Height > 0 ? a : (Rectangle?)null;
            if (_rbSame.Checked && _subArea.HasValue)
            {
                SetTargetFields(_subArea.Value.Location);
            }

            ApplyOverlays();
        }

        private void OnCellClicked(Side side, RegionCell cell)
        {
            if (!_chkPick.Checked || cell == null)
            {
                return;
            }

            if (side != Target)
            {
                SetStatus($"Bitte in der Ziel-Ansicht \"{Target.Name}\" klicken.");
                return;
            }

            SetTargetFields(new Point(cell.X, cell.Y));
            _rbOther.Checked = true;
            _chkPick.Checked = false;
            ApplyOverlays();
        }

        private void GoToTarget()
        {
            Rectangle source = SourceArea;
            Point t = TargetPosition(source);
            int x = t.X - ((_area.Width - source.Width) / 2);
            int y = t.Y - ((_area.Height - source.Height) / 2);
            Rectangle? keep = _subArea;
            LoadArea(new Rectangle(Math.Max(0, x), Math.Max(0, y), _area.Width, _area.Height));

            // keep the copied part (in map coordinates) if it is still visible
            if (keep.HasValue && _area.Contains(keep.Value))
            {
                _subArea = keep;
                ApplyOverlays();
            }
        }

        #endregion

        #region Copy / undo

        private void DoCopy()
        {
            if (!_chkLand.Checked && !_chkCopyStatics.Checked)
            {
                MessageBox.Show(this, "Bitte \"Land\" und/oder \"Statics mitkopieren\" auswählen.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Side src = Source, dst = Target;
            Rectangle area = SourceArea;
            Point t = TargetPosition(area);

            MapClip clip;
            try
            {
                clip = MapClip.FromMap(src.Map, area, src.Session, src.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Der Bereich konnte nicht gelesen werden:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool adjust = _chkAdjust.Checked;
            var options = new ClipPasteOptions
            {
                IncludeLand = _chkLand.Checked,
                IncludeStatics = _chkCopyStatics.Checked,
                HeightMode = adjust ? ClipHeightMode.MatchEdges : ClipHeightMode.Keep,
                ZOffset = (int)_numZ.Value,
                BlendWidth = adjust ? 2 : 0,
                StaticsFollowGround = adjust,
                RemoveExisting = !_chkCopyStatics.Checked
                    ? ObstacleFilter.None
                    : _cmbExisting.SelectedIndex switch
                    {
                        0 => ObstacleFilter.All,
                        1 => ObstacleFilter.Plants,
                        _ => ObstacleFilter.None
                    }
            };

            ClipPasteResult result;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                result = ClipPaster.Compute(clip, dst.Map, t.X, t.Y, options, dst.Session);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if (result.Land.Count == 0 && result.Statics.Count == 0 && result.RemovedStatics.Count == 0)
            {
                MessageBox.Show(this, "Es gibt nichts zu kopieren (Bereich leer oder außerhalb der Zielkarte).", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_chkConfirm.Checked)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{src.Name}  {area.X},{area.Y} ({area.Width} × {area.Height})");
                sb.AppendLine($"→ {dst.Name}  {t.X},{t.Y}");
                sb.AppendLine();
                if (options.IncludeLand)
                {
                    sb.AppendLine($"• {result.Land.Count} Land-Tiles setzen");
                }

                if (options.IncludeStatics)
                {
                    sb.AppendLine($"• {result.Statics.Count} Statics hinzufügen");
                    sb.AppendLine($"• {result.RemovedStatics.Count} vorhandene Statics im Ziel entfernen");
                }

                if (result.AppliedOffset != 0)
                {
                    sb.AppendLine($"• Höhenversatz: {result.AppliedOffset:+#;-#;0}");
                }

                if (result.Outside > 0)
                {
                    sb.AppendLine($"• {result.Outside} Zellen liegen außerhalb der Zielkarte (übersprungen)");
                }

                if (result.Clamped > 0)
                {
                    sb.AppendLine($"• {result.Clamped} Höhen auf -128..127 begrenzt");
                }

                if (result.EdgeProblems.Count > 0)
                {
                    sb.AppendLine($"• Achtung: {result.EdgeProblems.Count} Randzellen mit Höhensprung (max. {result.MaxEdgeJump})");
                }

                sb.AppendLine();
                sb.Append("Die Änderung bleibt im Speicher, bis sie gespeichert wird (Strg+Z = rückgängig). Fortfahren?");
                if (MessageBox.Show(this, sb.ToString(), "Kopieren", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                {
                    return;
                }
            }

            string name = $"Kopie {src.Name} {area.X},{area.Y} → {t.X},{t.Y}";
            ClipPaster.Apply(result, dst.Session, name);
            _undoOrder.Add(dst);
            _redoOrder.Clear();

            AfterEdit(dst, $"Kopiert: {result.Land.Count} Land, +{result.Statics.Count} / −{result.RemovedStatics.Count} Statics in {dst.Name} bei {t.X},{t.Y}");

            if (!_area.IntersectsWith(result.TargetArea))
            {
                if (MessageBox.Show(this, "Der Zielbereich liegt außerhalb der aktuellen Ansicht.\nAnsicht zum Ziel verschieben?", Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    GoToTarget();
                }
            }
        }

        private void DoUndo()
        {
            if (_undoOrder.Count == 0)
            {
                SetStatus("Nichts rückgängig zu machen.");
                return;
            }

            Side side = _undoOrder[_undoOrder.Count - 1];
            _undoOrder.RemoveAt(_undoOrder.Count - 1);
            string name = side.Session.UndoLast();
            if (name == null)
            {
                return;
            }

            _redoOrder.Add(side);
            AfterEdit(side, $"Rückgängig: {name}");
        }

        private void DoRedo()
        {
            if (_redoOrder.Count == 0)
            {
                SetStatus("Nichts zu wiederholen.");
                return;
            }

            Side side = _redoOrder[_redoOrder.Count - 1];
            _redoOrder.RemoveAt(_redoOrder.Count - 1);
            string name = side.Session.Redo();
            if (name == null)
            {
                return;
            }

            _undoOrder.Add(side);
            AfterEdit(side, $"Wiederholt: {name}");
        }

        private void AfterEdit(Side side, string message)
        {
            Rectangle? changed = side.Session.TakeChangedArea();
            ReloadSide(side);
            ComputeDiffs();
            ApplyOverlays();
            UpdateChangeInfo();
            SetStatus(message);
            _mapsChanged?.Invoke(changed);
        }

        private void UpdateChangeInfo()
        {
            int c1 = _left.Session.ChangeCount, c2 = _right.Session.ChangeCount;
            _lblChanges.Text = c1 + c2 == 0 ? "Keine ungespeicherten Änderungen" : $"Ungespeichert – Map1: {c1}   Map2: {c2}";
            _btnUndo.Enabled = _undoOrder.Count > 0;
            _btnRedo.Enabled = _redoOrder.Count > 0;
            _btnSave1.Enabled = HasChanges(_left);
            _btnSave2.Enabled = HasChanges(_right);
        }

        private static bool HasChanges(Side side)
        {
            TileMatrix tiles = side.Map.Tiles;
            return side.Session.ChangeCount > 0 || tiles.HasModifiedLand || tiles.HasStaticOverrides;
        }

        private void SetStatus(string text)
        {
            if (_lblStatus != null)
            {
                _lblStatus.Text = text;
            }
        }

        #endregion

        #region Save

        private bool SaveSide(Side side)
        {
            using (var dialog = new MapSaveDialog(side.Map, side.Name, side.Session.ChangeCount))
            {
                DialogResult result = dialog.ShowDialog(this);
                if (result == DialogResult.Abort)
                {
                    // a backup was restored in the dialog: the files of this map changed
                    ForgetEdits(side);
                    if (!IsRegisteredMap(side.Map))
                    {
                        side.Map.ResetAll();
                    }

                    AfterSave(side, "Backup wiederhergestellt");
                    return false;
                }

                if (result != DialogResult.OK)
                {
                    return false;
                }

                string directory = dialog.TargetDirectory;
                if (string.IsNullOrWhiteSpace(directory))
                {
                    MessageBox.Show(this, "Kein Zielordner gewählt.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                MapSaveOptions options = dialog.SaveOptions;
                TileMatrix tiles = side.Map.Tiles;
                bool allWritten = (options.IncludeLand || !tiles.HasModifiedLand) && (options.IncludeStatics || !tiles.HasStaticOverrides);
                bool replacesLoaded = MapSaveService.IsSameDirectory(directory, MapSaveService.GetSourceDirectory(side.Map)) &&
                                      options.OutputIndex == side.Map.FileIndex;

                List<string> files;
                MapBackupInfo backup;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    // Map2 of the compare plugin is not in the map list: close its files ourselves
                    _left.Map.Tiles.CloseStreams();
                    _right.Map.Tiles.CloseStreams();
                    files = MapSaveService.Save(side.Map, side.Name, directory, dialog.CreateBackup, options, out backup);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Speichern fehlgeschlagen:\n{ex.Message}\n\nDie Änderungen sind weiter im Speicher.", Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

                // maps outside of the map list that read one of the written files have to read them again
                foreach (Side s in new[] { _left, _right })
                {
                    if (!IsRegisteredMap(s.Map) && files.Any(s.Map.UsesFile))
                    {
                        s.Map.ResetAll();
                        if (s != side)
                        {
                            ForgetEdits(s);
                        }
                    }
                }

                if (allWritten || replacesLoaded)
                {
                    side.Session.MarkSaved(!replacesLoaded);
                }

                if (replacesLoaded)
                {
                    ForgetEdits(side);
                }

                AfterSave(side, $"{side.Name} gespeichert");
                MessageBox.Show(this, "Gespeichert:\n" + string.Join("\n", files.Select(Path.GetFileName)) + $"\n\nin {directory}" +
                                      (backup != null ? $"\n\nBackup: {backup.Directory}" : string.Empty),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
        }

        /// <summary>The undo steps of a side do not fit anymore (files were replaced)</summary>
        private void ForgetEdits(Side side)
        {
            side.Session.MarkSaved(false);
            _undoOrder.RemoveAll(s => s == side);
            _redoOrder.RemoveAll(s => s == side);
        }

        private void AfterSave(Side side, string message)
        {
            side.Session.TakeChangedArea();
            ReloadSide(_left);
            ReloadSide(_right);
            ComputeDiffs();
            ApplyOverlays();
            UpdateChangeInfo();
            SetStatus(message);
            _mapsChanged?.Invoke(null);
        }

        private static bool IsRegisteredMap(Map map) => Map.AllMaps().Contains(map);

        #endregion

        #region Keys / closing

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!(ActiveControl is TextBoxBase) && !(ActiveControl is NumericUpDown))
            {
                if (keyData == (Keys.Control | Keys.Z))
                {
                    DoUndo();
                    return true;
                }

                if (keyData == (Keys.Control | Keys.Y) || keyData == (Keys.Control | Keys.Shift | Keys.Z))
                {
                    DoRedo();
                    return true;
                }
            }

            if (keyData == Keys.Escape && _chkPick.Checked)
            {
                _chkPick.Checked = false;
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Closes without asking (the caller discards or keeps the changes itself)</summary>
        public void CloseSilently()
        {
            _closeSilently = true;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel || _closeSilently)
            {
                return;
            }

            int c1 = _left.Session.ChangeCount, c2 = _right.Session.ChangeCount;
            if (c1 + c2 == 0)
            {
                return;
            }

            DialogResult answer = MessageBox.Show(this,
                $"Ungespeicherte Änderungen:\n  {_left.Name}: {c1}\n  {_right.Name}: {c2}\n\n" +
                "Jetzt speichern?\n\nJa = speichern\nNein = im Speicher behalten (später im CGI-Vergleich speichern)\nAbbrechen = Fenster offen lassen",
                Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (answer == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (answer == DialogResult.Yes)
            {
                foreach (Side side in new[] { _left, _right })
                {
                    if (side.Session.ChangeCount > 0 && !SaveSide(side))
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _left.View.Renderer = null;
            _right.View.Renderer = null;
            _left.Renderer?.Dispose();
            _right.Renderer?.Dispose();
            _left.Renderer = _right.Renderer = null;
            _art.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tip.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
