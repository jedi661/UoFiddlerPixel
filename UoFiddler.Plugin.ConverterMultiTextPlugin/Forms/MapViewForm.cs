// =============================================================================
// MapViewForm.cs
// Zweck: Vollständige UO-Kartenansicht mit echten Tiles + Statics-Overlay.
//
// ANSICHTSMODI (Toggle-Button oben):
//   [MAP ONLY] → Nur map*.mul Gelände-Tiles (blockbasiert, 1 Pixel = 1 Block)
//   [MAP+STAT] → Gelände + Statics als farbige Punkte darüber
//
// Steuerung:
//   LMT + Ziehen  → Karte verschieben (Pan)
//   RMT + Ziehen  → Bereich markieren (snappt auf 8er-Block-Grenzen)
//   Mausrad       → Zoom
//   → From Region → X1/Y1/X2/Y2 in Hauptform übertragen
//   → To Region   → ToX/ToY in Hauptform übertragen
// =============================================================================

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public class MapViewForm : Form
    {
        // -----------------------------------------------------------------------
        // Terrain-Farbpalette (TileID → Geländefarbe)
        // -----------------------------------------------------------------------

        private static readonly Color[] TerrainColors = BuildTerrainPalette();

        private static Color[] BuildTerrainPalette()
        {
            var p = new Color[0x4000];
            for (int i = 0; i < p.Length; i++)
                p[i] = Color.FromArgb(60, 55, 50); // Default: dunkler Stein

            p[0x0000] = Color.FromArgb(8, 8, 12);  // Void

            // Wasser
            for (int i = 0x001; i <= 0x006; i++) p[i] = Color.FromArgb(20, 55, 130);
            for (int i = 0x007; i <= 0x01F; i++) p[i] = Color.FromArgb(28, 75, 155);
            for (int i = 0x020; i <= 0x02F; i++) p[i] = Color.FromArgb(38, 95, 175);

            // Sand / Küste
            for (int i = 0x030; i <= 0x04A; i++) p[i] = Color.FromArgb(195, 175, 120);

            // Gras
            for (int i = 0x04B; i <= 0x075; i++) p[i] = Color.FromArgb(52, 112, 42);
            for (int i = 0x076; i <= 0x087; i++) p[i] = Color.FromArgb(42, 95, 32);

            // Erde
            for (int i = 0x088; i <= 0x0A0; i++) p[i] = Color.FromArgb(115, 82, 48);

            // Stein / Fels
            for (int i = 0x0A1; i <= 0x0BF; i++) p[i] = Color.FromArgb(98, 92, 88);

            // Dunkler Pflasterstein
            for (int i = 0x0C0; i <= 0x0E0; i++) p[i] = Color.FromArgb(68, 68, 68);

            // Sumpf
            for (int i = 0x0E1; i <= 0x0FF; i++) p[i] = Color.FromArgb(38, 68, 38);

            // Wiese hell
            for (int i = 0x100; i <= 0x13F; i++) p[i] = Color.FromArgb(72, 135, 58);

            // Schnee / Eis
            for (int i = 0x140; i <= 0x15F; i++) p[i] = Color.FromArgb(208, 222, 238);

            // Lavagestein
            for (int i = 0x160; i <= 0x17F; i++) p[i] = Color.FromArgb(78, 22, 8);

            // Wüste
            for (int i = 0x180; i <= 0x1AF; i++) p[i] = Color.FromArgb(198, 162, 78);

            // Stadtpflaster
            for (int i = 0x1B0; i <= 0x1CF; i++) p[i] = Color.FromArgb(128, 118, 108);

            // Dungeonboden
            for (int i = 0x200; i <= 0x27F; i++) p[i] = Color.FromArgb(42, 38, 35);

            // Holzboden
            for (int i = 0x280; i <= 0x2AF; i++) p[i] = Color.FromArgb(112, 72, 38);

            return p;
        }

        // Statics-Farbe nach Grafik-Kategorie
        private static Color GetStaticColor(ushort graphic)
        {
            if (graphic < 0x0100) return Color.FromArgb(195, 195, 155);
            if (graphic < 0x0400) return Color.FromArgb(130, 175, 130);
            if (graphic < 0x0800) return Color.FromArgb(155, 135, 108);
            if (graphic < 0x1000) return Color.FromArgb(175, 145, 95);
            if (graphic < 0x1800) return Color.FromArgb(148, 95, 75);
            if (graphic < 0x2000) return Color.FromArgb(198, 175, 58);
            if (graphic < 0x3000) return Color.FromArgb(88, 108, 158);
            return Color.FromArgb(175, 95, 95);
        }

        // -----------------------------------------------------------------------
        // Felder
        // -----------------------------------------------------------------------

        private Bitmap _mapBitmap;
        private Bitmap _staticsBitmap;

        private readonly string _mapFilePath;
        private readonly string _staticsFilePath;
        private readonly string _staidxFilePath;
        private readonly int _mapWidth;
        private readonly int _mapHeight;

        private float _zoom = 0.25f;
        private PointF _viewOffset = new PointF(0, 0);

        private Point _panStart;
        private bool _isPanning = false;
        private bool _isSelecting = false;
        private Point _selectionStartTile;
        private Point _selectionEndTile;
        private bool _hasSelection = false;

        /// <summary>false = nur Map, true = Map + Statics</summary>
        private bool _showStatics = false;

        private readonly Action<int, int, int, int> _setFromRegion;
        private readonly Action<int, int> _setToRegion;

        // UI
        private Panel _mapPanel;
        private Label _lblCoords;
        private Label _lblSelection;
        private Button _btnApplyFrom;
        private Button _btnApplyTo;
        private Button _btnToggleView;
        private Label _lblZoom;
        private TrackBar _trackBarZoom;
        private Label _lblLoadStatus;

        // -----------------------------------------------------------------------
        // Konstruktor
        // -----------------------------------------------------------------------

        public MapViewForm(
            string mapFilePath,
            int mapWidth,
            int mapHeight,
            Action<int, int, int, int> setFromRegion,
            Action<int, int> setToRegion)
        {
            _mapFilePath = mapFilePath;
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _setFromRegion = setFromRegion;
            _setToRegion = setToRegion;

            // Statics-Pfade ableiten
            string dir = Path.GetDirectoryName(mapFilePath) ?? "";
            string idStr = Path.GetFileNameWithoutExtension(mapFilePath).Replace("map", "");
            _staticsFilePath = Path.Combine(dir, $"statics{idStr}.mul");
            _staidxFilePath = Path.Combine(dir, $"staidx{idStr}.mul");

            BuildUI();
            this.Load += (s, e) => LoadAllBitmaps();
        }

        // -----------------------------------------------------------------------
        // UI – dunkles, industrielles UO-Karteneditor-Design
        // -----------------------------------------------------------------------

        private void BuildUI()
        {
            this.Text = $"UO Map Viewer  —  {Path.GetFileName(_mapFilePath)}  [{_mapWidth} × {_mapHeight}]";
            this.Size = new Size(1200, 820);
            this.MinimumSize = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(18, 18, 20);
            this.ForeColor = Color.FromArgb(200, 200, 195);
            this.Font = new Font("Consolas", 8.5f);

            // ── Toolbar ─────────────────────────────────────────────────────────
            Panel toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.FromArgb(26, 26, 30),
                Padding = new Padding(6, 8, 6, 0)
            };
            toolbar.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(65, 125, 85), 1);
                e.Graphics.DrawLine(p, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            _lblZoom = new Label
            {
                Text = "ZOOM  6%",
                ForeColor = Color.FromArgb(115, 195, 125),
                Location = new Point(8, 14),
                Size = new Size(80, 18),
                Font = new Font("Consolas", 7.5f, FontStyle.Bold)
            };

            _trackBarZoom = new TrackBar
            {
                Location = new Point(90, 7),
                Width = 165,
                Minimum = 1,
                Maximum = 20,
                Value = 4,
                TickFrequency = 4,
                SmallChange = 1,
                BackColor = Color.FromArgb(26, 26, 30)
            };
            _trackBarZoom.ValueChanged += (s, e) => { UpdateZoom(); _mapPanel?.Invalidate(); };

            _btnToggleView = MakeBtn("◉  MAP ONLY", Color.FromArgb(45, 95, 55), 275, 132);
            _btnToggleView.Click += BtnToggleView_Click;

            _btnApplyFrom = MakeBtn("▶  FROM REGION", Color.FromArgb(28, 65, 125), 422, 148);
            _btnApplyFrom.Enabled = false;
            _btnApplyFrom.Click += BtnApplyFrom_Click;

            _btnApplyTo = MakeBtn("▶  TO REGION", Color.FromArgb(105, 72, 18), 580, 132);
            _btnApplyTo.Enabled = false;
            _btnApplyTo.Click += BtnApplyTo_Click;

            Button btnReload = MakeBtn("⟳  RELOAD", Color.FromArgb(50, 42, 52), 722, 105);
            btnReload.Click += (s, e) => LoadAllBitmaps();

            Label lblHint = new Label
            {
                Text = "LMT=PAN   RMT=SELECT   WHEEL=ZOOM",
                ForeColor = Color.FromArgb(70, 72, 68),
                Location = new Point(840, 15),
                AutoSize = true,
                Font = new Font("Consolas", 7f)
            };

            toolbar.Controls.AddRange(new Control[]
                { _lblZoom, _trackBarZoom, _btnToggleView, _btnApplyFrom, _btnApplyTo, btnReload, lblHint });

            // ── Statusleiste ─────────────────────────────────────────────────────
            Panel statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.FromArgb(20, 20, 23)
            };
            statusBar.Paint += (s, e) =>
            {
                using Pen p = new Pen(Color.FromArgb(65, 125, 85), 1);
                e.Graphics.DrawLine(p, 0, 0, statusBar.Width, 0);
            };

            _lblCoords = new Label
            {
                Text = "CURSOR  X:—  Y:—",
                ForeColor = Color.FromArgb(95, 195, 115),
                Location = new Point(10, 8),
                Size = new Size(400, 18),
                Font = new Font("Consolas", 8.5f, FontStyle.Bold)
            };
            _lblSelection = new Label
            {
                Text = "SELECTION  —",
                ForeColor = Color.FromArgb(215, 185, 75),
                Location = new Point(10, 28),
                Size = new Size(780, 18),
                Font = new Font("Consolas", 8f)
            };
            _lblLoadStatus = new Label
            {
                Text = "READY",
                ForeColor = Color.FromArgb(65, 68, 62),
                Location = new Point(850, 18),
                Size = new Size(340, 18),
                Font = new Font("Consolas", 7.5f),
                TextAlign = ContentAlignment.MiddleRight
            };

            statusBar.Controls.AddRange(new Control[] { _lblCoords, _lblSelection, _lblLoadStatus });

            // ── Map Panel ────────────────────────────────────────────────────────
            _mapPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(10, 10, 12),
                Cursor = Cursors.Cross
            };
            _mapPanel.Paint += MapPanel_Paint;
            _mapPanel.MouseDown += MapPanel_MouseDown;
            _mapPanel.MouseMove += MapPanel_MouseMove;
            _mapPanel.MouseUp += MapPanel_MouseUp;
            _mapPanel.MouseWheel += MapPanel_MouseWheel;

            this.Controls.Add(_mapPanel);
            this.Controls.Add(toolbar);
            this.Controls.Add(statusBar);

            UpdateZoom();
        }

        private Button MakeBtn(string text, Color bg, int x, int w)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 9),
                Size = new Size(w, 28),
                BackColor = bg,
                ForeColor = Color.FromArgb(218, 218, 208),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 7.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(
                Math.Min(255, bg.R + 45),
                Math.Min(255, bg.G + 45),
                Math.Min(255, bg.B + 45));
            return btn;
        }

        // -----------------------------------------------------------------------
        // Toggle Ansicht
        // -----------------------------------------------------------------------

        private void BtnToggleView_Click(object sender, EventArgs e)
        {
            _showStatics = !_showStatics;

            if (_showStatics)
            {
                _btnToggleView.Text = "◉  MAP + STATICS";
                _btnToggleView.BackColor = Color.FromArgb(100, 58, 16);
                _btnToggleView.Size = new Size(148, 28);
                _btnToggleView.FlatAppearance.BorderColor = Color.FromArgb(180, 110, 45);
            }
            else
            {
                _btnToggleView.Text = "◉  MAP ONLY";
                _btnToggleView.BackColor = Color.FromArgb(45, 95, 55);
                _btnToggleView.Size = new Size(132, 28);
                _btnToggleView.FlatAppearance.BorderColor = Color.FromArgb(85, 155, 88);
            }

            _mapPanel.Invalidate();
        }

        // -----------------------------------------------------------------------
        // Karte + Statics laden
        // -----------------------------------------------------------------------

        private void LoadAllBitmaps()
        {
            if (!File.Exists(_mapFilePath))
            {
                SetStatus($"FEHLER: {Path.GetFileName(_mapFilePath)} nicht gefunden");
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            SetStatus("LADE MAP-TILES …");
            Application.DoEvents();

            int blockW = _mapWidth / 8;
            int blockH = _mapHeight / 8;

            // ── Map-Bitmap ───────────────────────────────────────────────────────
            _mapBitmap?.Dispose();
            _mapBitmap = new Bitmap(blockW, blockH);

            try
            {
                using FileStream fs = new FileStream(_mapFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using BinaryReader br = new BinaryReader(fs);

                for (int bx = 0; bx < blockW; bx++)
                {
                    for (int by = 0; by < blockH; by++)
                    {
                        long pos = ((long)bx * blockH + by) * 196L + 4L;
                        if (pos + 2 >= fs.Length) { _mapBitmap.SetPixel(bx, by, Color.Black); continue; }

                        fs.Seek(pos, SeekOrigin.Begin);
                        ushort tileId = br.ReadUInt16();
                        br.ReadByte(); // Z

                        Color c = (tileId < TerrainColors.Length)
                            ? TerrainColors[tileId]
                            : Color.FromArgb(58, 52, 48);
                        _mapBitmap.SetPixel(bx, by, c);
                    }

                    if (bx % 80 == 0)
                    {
                        SetStatus($"MAP  {bx * 100 / blockW}%");
                        Application.DoEvents();
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatus($"MAP-FEHLER: {ex.Message}");
                this.Cursor = Cursors.Default;
                return;
            }

            // ── Statics-Bitmap ───────────────────────────────────────────────────
            _staticsBitmap?.Dispose();
            _staticsBitmap = null;

            bool hasStatics = File.Exists(_staticsFilePath) && File.Exists(_staidxFilePath);

            if (hasStatics)
            {
                SetStatus("LADE STATICS …");
                Application.DoEvents();

                try
                {
                    _staticsBitmap = new Bitmap(blockW, blockH);

                    using FileStream fsIdx = new FileStream(_staidxFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using FileStream fsStat = new FileStream(_staticsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using BinaryReader rIdx = new BinaryReader(fsIdx);
                    using BinaryReader rStat = new BinaryReader(fsStat);

                    for (int bx = 0; bx < blockW; bx++)
                    {
                        for (int by = 0; by < blockH; by++)
                        {
                            long idxPos = ((long)bx * blockH + by) * 12L;
                            if (idxPos + 12 > fsIdx.Length) continue;

                            fsIdx.Seek(idxPos, SeekOrigin.Begin);
                            int lookup = rIdx.ReadInt32();
                            int length = rIdx.ReadInt32();
                            rIdx.ReadInt32(); // extra

                            if (lookup < 0 || length <= 0) continue;

                            int count = length / 7;
                            fsStat.Seek(lookup, SeekOrigin.Begin);

                            int rSum = 0, gSum = 0, bSum = 0, valid = 0;

                            for (int i = 0; i < count; i++)
                            {
                                if (fsStat.Position + 7 > fsStat.Length) break;
                                ushort graphic = rStat.ReadUInt16();
                                rStat.ReadByte();  // x
                                rStat.ReadByte();  // y
                                rStat.ReadSByte(); // z
                                rStat.ReadInt16(); // hue

                                Color sc = GetStaticColor(graphic);
                                rSum += sc.R; gSum += sc.G; bSum += sc.B;
                                valid++;
                            }

                            if (valid > 0)
                            {
                                _staticsBitmap.SetPixel(bx, by, Color.FromArgb(
                                    Math.Min(255, rSum / valid),
                                    Math.Min(255, gSum / valid),
                                    Math.Min(255, bSum / valid)));
                            }
                        }

                        if (bx % 80 == 0)
                        {
                            SetStatus($"STATICS  {bx * 100 / blockW}%");
                            Application.DoEvents();
                        }
                    }
                }
                catch (Exception ex)
                {
                    SetStatus($"STATICS-WARNUNG: {ex.Message}");
                    _staticsBitmap?.Dispose();
                    _staticsBitmap = null;
                    hasStatics = false;
                }
            }

            SetStatus(hasStatics
                ? $"GELADEN  {_mapWidth}×{_mapHeight}  |  {blockW}×{blockH} Blöcke  |  Statics OK"
                : $"GELADEN  {_mapWidth}×{_mapHeight}  |  {blockW}×{blockH} Blöcke  |  Statics N/A");

            // Statics-Button deaktivieren wenn keine Statics-Daten
            if (!hasStatics && _btnToggleView != null)
                _btnToggleView.Enabled = false;

            this.Cursor = Cursors.Default;
            _mapPanel.Invalidate();
        }

        // -----------------------------------------------------------------------
        // Paint
        // -----------------------------------------------------------------------

        private void MapPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            if (_mapBitmap == null)
            {
                g.Clear(Color.FromArgb(10, 10, 12));
                using Font f = new Font("Consolas", 11f);
                g.DrawString("KARTE WIRD GELADEN …", f, new SolidBrush(Color.FromArgb(55, 58, 52)), 22, 22);
                return;
            }

            int panelW = _mapPanel.Width;
            int panelH = _mapPanel.Height;

            // Sichtbarer Bereich im Bitmap (1px = 1 Block = 8 Tiles)
            float srcX = _viewOffset.X / 8f;
            float srcY = _viewOffset.Y / 8f;
            float srcW = Math.Min((panelW / _zoom) / 8f, _mapBitmap.Width - srcX);
            float srcH = Math.Min((panelH / _zoom) / 8f, _mapBitmap.Height - srcY);
            srcX = Math.Max(0, srcX);
            srcY = Math.Max(0, srcY);
            if (srcW <= 0 || srcH <= 0) return;

            RectangleF src = new RectangleF(srcX, srcY, srcW, srcH);
            Rectangle dst = new Rectangle(0, 0, panelW, panelH);

            // 1. Map-Tiles zeichnen
            g.DrawImage(_mapBitmap, dst, src, GraphicsUnit.Pixel);

            // 2. Statics-Overlay (halbtransparent) zeichnen
            if (_showStatics && _staticsBitmap != null)
            {
                using var ia = new System.Drawing.Imaging.ImageAttributes();
                float[][] mat =
                {
                    new float[] {1,0,0,0,0},
                    new float[] {0,1,0,0,0},
                    new float[] {0,0,1,0,0},
                    new float[] {0,0,0,0.75f,0},
                    new float[] {0,0,0,0,1}
                };
                ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(mat));
                g.DrawImage(_staticsBitmap, dst, srcX, srcY, srcW, srcH, GraphicsUnit.Pixel, ia);
            }

            // 3. Gitter bei hohem Zoom
            if (_zoom >= 2f) DrawGrid(g, panelW, panelH);

            // 4. Selektion
            if (_hasSelection || _isSelecting) DrawSelection(g);

            // 5. HUD-Overlays
            DrawHUD(g, panelW, panelH);
        }

        private void DrawGrid(Graphics g, int panelW, int panelH)
        {
            using Pen blockPen = new Pen(Color.FromArgb(28, 95, 255, 95), 0.5f);
            using Pen tilePen = new Pen(Color.FromArgb(12, 175, 175, 175), 0.5f);

            int startX = (int)_viewOffset.X - ((int)_viewOffset.X % 8);
            int startY = (int)_viewOffset.Y - ((int)_viewOffset.Y % 8);
            int endX = startX + (int)(panelW / _zoom) + 16;
            int endY = startY + (int)(panelH / _zoom) + 16;

            for (int tx = startX; tx <= endX; tx++)
            {
                if (_zoom < 4f && tx % 8 != 0) continue;
                Point ps = TileToScreen(tx, startY);
                Point pe = TileToScreen(tx, endY);
                g.DrawLine(tx % 8 == 0 ? blockPen : tilePen, ps, pe);
            }
            for (int ty = startY; ty <= endY; ty++)
            {
                if (_zoom < 4f && ty % 8 != 0) continue;
                Point ps = TileToScreen(startX, ty);
                Point pe = TileToScreen(endX, ty);
                g.DrawLine(ty % 8 == 0 ? blockPen : tilePen, ps, pe);
            }
        }

        private void DrawSelection(Graphics g)
        {
            int x1 = Math.Min(_selectionStartTile.X, _selectionEndTile.X);
            int y1 = Math.Min(_selectionStartTile.Y, _selectionEndTile.Y);
            int x2 = Math.Max(_selectionStartTile.X, _selectionEndTile.X);
            int y2 = Math.Max(_selectionStartTile.Y, _selectionEndTile.Y);

            Point pMin = TileToScreen(x1, y1);
            Point pMax = TileToScreen(x2, y2);

            Rectangle selRect = new Rectangle(
                pMin.X, pMin.Y, pMax.X - pMin.X, pMax.Y - pMin.Y);

            // Füllung
            using SolidBrush fill = new SolidBrush(Color.FromArgb(45, 255, 210, 45));
            g.FillRectangle(fill, selRect);

            // Äußerer Leuchtring
            using Pen glow = new Pen(Color.FromArgb(55, 255, 225, 75), 4f);
            g.DrawRectangle(glow, selRect);

            // Haupt-Rahmen gestrichelt
            using Pen border = new Pen(Color.FromArgb(252, 210, 48), 1.5f);
            border.DashStyle = DashStyle.Dash;
            g.DrawRectangle(border, selRect);

            // Ecken-Marker
            int m = 7;
            using Pen corner = new Pen(Color.FromArgb(255, 240, 100), 2f);
            void DrawCorner(int cx, int cy, int dx, int dy)
            {
                g.DrawLine(corner, cx, cy, cx + dx * m, cy);
                g.DrawLine(corner, cx, cy, cx, cy + dy * m);
            }
            DrawCorner(pMin.X, pMin.Y, 1, 1);
            DrawCorner(pMax.X, pMin.Y, -1, 1);
            DrawCorner(pMin.X, pMax.Y, 1, -1);
            DrawCorner(pMax.X, pMax.Y, -1, -1);

            // Koordinaten-Label
            string lbl = $" [{x1},{y1}] → [{x2},{y2}]  {x2 - x1}×{y2 - y1} ";
            using Font lf = new Font("Consolas", 8f, FontStyle.Bold);
            SizeF ls = g.MeasureString(lbl, lf);
            float lx = pMin.X;
            float ly = pMin.Y - ls.Height - 4;
            if (ly < 2) ly = pMax.Y + 4;

            using SolidBrush lblBg = new SolidBrush(Color.FromArgb(205, 18, 16, 12));
            g.FillRectangle(lblBg, lx - 2, ly - 1, ls.Width + 4, ls.Height + 2);
            g.DrawString(lbl, lf, Brushes.LightYellow, lx, ly);
        }

        private void DrawHUD(Graphics g, int panelW, int panelH)
        {
            // Modus-Badge oben rechts
            string modeText = _showStatics ? "MAP + STATICS" : "MAP ONLY";
            Color modeBg = _showStatics
                ? Color.FromArgb(175, 98, 58, 8)
                : Color.FromArgb(175, 8, 58, 18);
            Color modeFg = _showStatics ? Color.FromArgb(230, 165, 55) : Color.FromArgb(75, 200, 95);
            Color modeBdr = _showStatics ? Color.FromArgb(195, 142, 52) : Color.FromArgb(62, 148, 82);

            using Font mf = new Font("Consolas", 8f, FontStyle.Bold);
            SizeF ms = g.MeasureString(modeText, mf);
            float mx = panelW - ms.Width - 18;
            float my = 8;

            using SolidBrush mbg = new SolidBrush(modeBg);
            g.FillRectangle(mbg, mx - 5, my - 2, ms.Width + 10, ms.Height + 4);
            using Pen mbdr = new Pen(modeBdr, 1f);
            g.DrawRectangle(mbdr, mx - 5, my - 2, ms.Width + 10, ms.Height + 4);
            using SolidBrush mfg = new SolidBrush(modeFg);
            g.DrawString(modeText, mf, mfg, mx, my);

            // Zoom-Anzeige unten rechts
            string zoomText = $"ZOOM {(int)(_zoom * 100)}%";
            using Font zf = new Font("Consolas", 7.5f);
            SizeF zs = g.MeasureString(zoomText, zf);
            g.DrawString(zoomText, zf, new SolidBrush(Color.FromArgb(58, 62, 56)),
                panelW - zs.Width - 8, panelH - zs.Height - 8);
        }

        // -----------------------------------------------------------------------
        // Koordinaten-Konvertierung
        // -----------------------------------------------------------------------

        private Point ScreenToTile(Point screen)
        {
            int tx = Math.Max(0, Math.Min(_mapWidth - 1, (int)(_viewOffset.X + screen.X / _zoom)));
            int ty = Math.Max(0, Math.Min(_mapHeight - 1, (int)(_viewOffset.Y + screen.Y / _zoom)));
            return new Point(tx, ty);
        }

        private Point TileToScreen(int tx, int ty)
            => new Point((int)((tx - _viewOffset.X) * _zoom), (int)((ty - _viewOffset.Y) * _zoom));

        // -----------------------------------------------------------------------
        // Mouse Events
        // -----------------------------------------------------------------------

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPanning = true; _panStart = e.Location;
                _mapPanel.Cursor = Cursors.SizeAll;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _isSelecting = true;
                _hasSelection = false;
                _selectionStartTile = SnapToGrid(ScreenToTile(e.Location));
                _selectionEndTile = _selectionStartTile;
                _mapPanel.Cursor = Cursors.Cross;
            }
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            Point tile = ScreenToTile(e.Location);
            _lblCoords.Text = $"CURSOR  X:{tile.X}  Y:{tile.Y}  |  Block [{tile.X / 8},{tile.Y / 8}]";

            if (_isPanning && e.Button == MouseButtons.Left)
            {
                float dx = (e.X - _panStart.X) / _zoom;
                float dy = (e.Y - _panStart.Y) / _zoom;
                _viewOffset.X = Math.Max(0, Math.Min(_mapWidth - _mapPanel.Width / _zoom, _viewOffset.X - dx));
                _viewOffset.Y = Math.Max(0, Math.Min(_mapHeight - _mapPanel.Height / _zoom, _viewOffset.Y - dy));
                _panStart = e.Location;
                _mapPanel.Invalidate();
            }
            else if (_isSelecting && e.Button == MouseButtons.Right)
            {
                _selectionEndTile = SnapToGrid(ScreenToTile(e.Location));
                UpdateSelLabel();
                _mapPanel.Invalidate();
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPanning = false;
                _mapPanel.Cursor = Cursors.Cross;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _isSelecting = false;
                _selectionEndTile = SnapToGrid(ScreenToTile(e.Location));
                _hasSelection = true;
                _btnApplyFrom.Enabled = true;
                _btnApplyTo.Enabled = true;
                UpdateSelLabel();
                _mapPanel.Invalidate();
            }
        }

        private void MapPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            Point tum = ScreenToTile(e.Location);
            int v = _trackBarZoom.Value + (e.Delta > 0 ? 1 : -1);
            _trackBarZoom.Value = Math.Max(_trackBarZoom.Minimum, Math.Min(_trackBarZoom.Maximum, v));
            _viewOffset.X = Math.Max(0, tum.X - e.X / _zoom);
            _viewOffset.Y = Math.Max(0, tum.Y - e.Y / _zoom);
            _mapPanel.Invalidate();
        }

        // -----------------------------------------------------------------------
        // Zoom
        // -----------------------------------------------------------------------

        private static readonly float[] ZoomLevels =
        {
            0.0625f,0.125f,0.1875f,0.25f,0.375f,0.5f,0.625f,0.75f,
            0.875f,1.0f,1.25f,1.5f,2.0f,2.5f,3.0f,4.0f,5.0f,6.0f,8.0f,10.0f
        };

        private void UpdateZoom()
        {
            int idx = Math.Max(0, Math.Min(ZoomLevels.Length - 1, _trackBarZoom.Value - 1));
            _zoom = ZoomLevels[idx];
            if (_lblZoom != null)
                _lblZoom.Text = $"ZOOM  {(int)(_zoom * 100)}%";
        }

        // -----------------------------------------------------------------------
        // Hilfsmethoden
        // -----------------------------------------------------------------------

        private static Point SnapToGrid(Point tile)
            => new Point((int)Math.Round(tile.X / 8.0) * 8, (int)Math.Round(tile.Y / 8.0) * 8);

        private void UpdateSelLabel()
        {
            int x1 = Math.Min(_selectionStartTile.X, _selectionEndTile.X);
            int y1 = Math.Min(_selectionStartTile.Y, _selectionEndTile.Y);
            int x2 = Math.Max(_selectionStartTile.X, _selectionEndTile.X);
            int y2 = Math.Max(_selectionStartTile.Y, _selectionEndTile.Y);
            _lblSelection.Text =
                $"SELECTION  X1:{x1}  Y1:{y1}  →  X2:{x2}  Y2:{y2}  |  {x2 - x1}×{y2 - y1} Tiles  ({(x2 - x1) / 8}×{(y2 - y1) / 8} Blöcke)";
        }

        private void BtnApplyFrom_Click(object sender, EventArgs e)
        {
            if (!_hasSelection) return;
            int x1 = Math.Min(_selectionStartTile.X, _selectionEndTile.X);
            int y1 = Math.Min(_selectionStartTile.Y, _selectionEndTile.Y);
            int x2 = Math.Max(_selectionStartTile.X, _selectionEndTile.X);
            int y2 = Math.Max(_selectionStartTile.Y, _selectionEndTile.Y);
            _setFromRegion?.Invoke(x1, y1, x2, y2);
            MessageBox.Show($"From Region gesetzt:\nX1={x1}  Y1={y1}\nX2={x2}  Y2={y2}",
                "Koordinaten übertragen", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnApplyTo_Click(object sender, EventArgs e)
        {
            if (!_hasSelection) return;
            int x1 = Math.Min(_selectionStartTile.X, _selectionEndTile.X);
            int y1 = Math.Min(_selectionStartTile.Y, _selectionEndTile.Y);
            _setToRegion?.Invoke(x1, y1);
            MessageBox.Show($"To Region gesetzt:\nToX={x1}  ToY={y1}",
                "Koordinaten übertragen", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SetStatus(string text)
        {
            if (_lblLoadStatus != null) _lblLoadStatus.Text = text;
            // Fortschritts-% auch in Selection-Label anzeigen
            if (_lblSelection != null && (text.Contains("%") || text.StartsWith("LADE")))
                _lblSelection.Text = text;
        }

        // -----------------------------------------------------------------------
        // Dispose
        // -----------------------------------------------------------------------

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mapBitmap?.Dispose();
                _staticsBitmap?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
