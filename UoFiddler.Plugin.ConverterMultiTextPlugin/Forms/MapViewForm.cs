// =============================================================================
// MapViewForm.cs
// Zweck: Eigenständiges Fenster zur Anzeige einer UO-Karte in voller Größe.
//        Der Benutzer kann per Rechtsklick + Ziehen einen Bereich markieren,
//        dessen Koordinaten dann zurück in die MapReplaceNewForm übertragen werden.
//
// Verwendung:
//   - Wird über den Button "Open Map View" in MapReplaceNewForm geöffnet.
//   - Die aktive Map-Checkbox bestimmt welche Karte geladen wird.
//   - Nach dem Markieren und Bestätigen werden X1/Y1/X2/Y2 übertragen.
//
// Steuerung:
//   - Linke Maustaste gedrückt halten + ziehen  → Karte verschieben (Pan)
//   - Rechte Maustaste gedrückt halten + ziehen → Bereich markieren (Selection)
//   - Mausrad                                   → Zoom In/Out
//   - Button "Übernehmen"                       → Koordinaten in Hauptform eintragen
//   - Button "Als From Region"                  → in X1/Y1/X2/Y2 eintragen
//   - Button "Als To Region"                    → in ToX/ToY eintragen
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
        // Felder
        // -----------------------------------------------------------------------

        /// <summary>Die geladene Karte als Bitmap (gesamte Karte oder gecachter Ausschnitt).</summary>
        private Bitmap _mapBitmap;

        /// <summary>Pfad zur map*.mul-Datei die angezeigt werden soll.</summary>
        private readonly string _mapFilePath;

        /// <summary>Breite der Karte in Tiles.</summary>
        private readonly int _mapWidth;

        /// <summary>Höhe der Karte in Tiles.</summary>
        private readonly int _mapHeight;

        /// <summary>Aktueller Zoom-Faktor (1.0 = 1 Tile = 1 Pixel).</summary>
        private float _zoom = 1.0f;

        /// <summary>Aktueller Viewport-Offset in Tile-Koordinaten (obere linke Ecke).</summary>
        private PointF _viewOffset = new PointF(0, 0);

        /// <summary>Startpunkt beim Linksklick-Pan (in Screen-Koordinaten).</summary>
        private Point _panStart;

        /// <summary>Ob gerade gepannt wird.</summary>
        private bool _isPanning = false;

        /// <summary>Ob gerade eine Selektion gezogen wird (Rechtsklick).</summary>
        private bool _isSelecting = false;

        /// <summary>Startpunkt der Selektion in Tile-Koordinaten.</summary>
        private Point _selectionStartTile;

        /// <summary>Endpunkt der Selektion in Tile-Koordinaten.</summary>
        private Point _selectionEndTile;

        /// <summary>Ob eine Selektion existiert die übernommen werden kann.</summary>
        private bool _hasSelection = false;

        /// <summary>Referenz auf die Hauptform zum Zurückschreiben der Koordinaten.</summary>
        private readonly MapReplaceNewForm _mainForm;

        /// <summary>Callback-Actions zum Schreiben der Koordinaten in die Hauptform.</summary>
        private readonly Action<int, int, int, int> _setFromRegion;
        private readonly Action<int, int> _setToRegion;

        // UI-Controls
        private Panel _mapPanel;
        private Label _lblCoords;
        private Label _lblSelection;
        private Button _btnApplyFrom;
        private Button _btnApplyTo;
        private Button _btnReload;
        private Label _lblZoom;
        private TrackBar _trackBarZoom;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;

        // -----------------------------------------------------------------------
        // Konstruktor
        // -----------------------------------------------------------------------

        /// <summary>
        /// Erstellt die MapViewForm.
        /// </summary>
        /// <param name="mapFilePath">Vollständiger Pfad zur map*.mul-Datei.</param>
        /// <param name="mapWidth">Kartenbreite in Tiles.</param>
        /// <param name="mapHeight">Kartenhöhe in Tiles.</param>
        /// <param name="setFromRegion">Callback: (x1, y1, x2, y2) → trägt in From Region ein.</param>
        /// <param name="setToRegion">Callback: (tox, toy) → trägt in To Region ein.</param>
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

            InitializeFormControls();
            this.Load += MapViewForm_Load;
        }

        // -----------------------------------------------------------------------
        // InitializeFormControls
        // -----------------------------------------------------------------------

        /// <summary>Erstellt alle UI-Controls programmatisch.</summary>
        private void InitializeFormControls()
        {
            this.Text = $"Map Viewer - {Path.GetFileName(_mapFilePath)} ({_mapWidth}x{_mapHeight})";
            this.Size = new Size(1000, 750);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // --- Toolbar oben ---
            Panel toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(45, 45, 45)
            };

            _lblZoom = new Label
            {
                Text = "Zoom: 100%",
                ForeColor = Color.White,
                Location = new Point(8, 11),
                AutoSize = true
            };

            _trackBarZoom = new TrackBar
            {
                Location = new Point(80, 8),
                Width = 150,
                Minimum = 1,
                Maximum = 20,
                Value = 4,          // Startzoom: 25% (1/4 Tile pro Pixel)
                TickFrequency = 2,
                SmallChange = 1
            };
            _trackBarZoom.ValueChanged += TrackBarZoom_ValueChanged;

            _btnApplyFrom = new Button
            {
                Text = "→ From Region (X1/Y1-X2/Y2)",
                Location = new Point(250, 8),
                Size = new Size(180, 24),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _btnApplyFrom.Click += BtnApplyFrom_Click;

            _btnApplyTo = new Button
            {
                Text = "→ To Region (ToX/ToY)",
                Location = new Point(440, 8),
                Size = new Size(155, 24),
                BackColor = Color.DarkGoldenrod,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _btnApplyTo.Click += BtnApplyTo_Click;

            _btnReload = new Button
            {
                Text = "⟳ Neu laden",
                Location = new Point(610, 8),
                Size = new Size(90, 24),
                BackColor = Color.DimGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnReload.Click += (s, e) => LoadMapBitmap();

            Label lblHint = new Label
            {
                Text = "LMT=Verschieben  |  RMT=Bereich markieren  |  Rad=Zoom",
                ForeColor = Color.LightGray,
                Location = new Point(715, 11),
                AutoSize = true
            };

            toolbar.Controls.AddRange(new Control[] {
                _lblZoom, _trackBarZoom, _btnApplyFrom, _btnApplyTo, _btnReload, lblHint
            });

            // --- Info-Panel unten ---
            Panel infoPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            _lblCoords = new Label
            {
                Text = "Mausposition: -",
                ForeColor = Color.LightGreen,
                Location = new Point(8, 8),
                Size = new Size(300, 18),
                Font = new Font("Consolas", 9f)
            };

            _lblSelection = new Label
            {
                Text = "Selektion: keine",
                ForeColor = Color.Yellow,
                Location = new Point(8, 26),
                Size = new Size(700, 18),
                Font = new Font("Consolas", 9f)
            };

            infoPanel.Controls.Add(_lblCoords);
            infoPanel.Controls.Add(_lblSelection);

            // --- Map Panel (Zeichenfläche) ---
            _mapPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Cursor = Cursors.Cross
            };
            _mapPanel.Paint += MapPanel_Paint;
            _mapPanel.MouseDown += MapPanel_MouseDown;
            _mapPanel.MouseMove += MapPanel_MouseMove;
            _mapPanel.MouseUp += MapPanel_MouseUp;
            _mapPanel.MouseWheel += MapPanel_MouseWheel;

            // Controls hinzufügen (Reihenfolge wichtig für Dock)
            this.Controls.Add(_mapPanel);
            this.Controls.Add(toolbar);
            this.Controls.Add(infoPanel);

            // Zoom initial setzen
            UpdateZoomFromTrackbar();
        }

        // -----------------------------------------------------------------------
        // MapViewForm_Load
        // -----------------------------------------------------------------------

        private void MapViewForm_Load(object sender, EventArgs e)
        {
            LoadMapBitmap();
        }

        // -----------------------------------------------------------------------
        // LoadMapBitmap
        // -----------------------------------------------------------------------

        /// <summary>
        /// Lädt die map*.mul und rendert sie als Bitmap.
        /// Für große Karten wird auf Blockebene gerendert (1 Block = 1 Pixel bei niedrigem Zoom).
        /// Bei höherem Zoom wird tile-genau gerendert.
        /// 
        /// Strategie: Immer vollständige Karte als kleine Übersicht rendern
        /// (1 Pixel = 1 Block = 8 Tiles), damit die gesamte Karte in den Speicher passt.
        /// Beim Zoomen wird der sichtbare Ausschnitt tile-genau nachgerendert.
        /// </summary>
        private void LoadMapBitmap()
        {
            if (!File.Exists(_mapFilePath))
            {
                MessageBox.Show($"Kartendatei nicht gefunden:\n{_mapFilePath}", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Cursor auf Warten setzen
            this.Cursor = Cursors.WaitCursor;
            _statusLabel_Set("Karte wird geladen...");

            try
            {
                // Übersichtsbild: 1 Pixel pro Block (8×8 Tiles)
                int blockWidth = _mapWidth / 8;
                int blockHeight = _mapHeight / 8;

                _mapBitmap?.Dispose();
                _mapBitmap = new Bitmap(blockWidth, blockHeight);

                using FileStream fs = new FileStream(_mapFilePath,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                using BinaryReader reader = new BinaryReader(fs);

                // Jeden Block lesen und als einzelnen Pixel darstellen
                // Wir nehmen den ersten Tile des Blocks als repräsentative Farbe
                for (int bx = 0; bx < blockWidth; bx++)
                {
                    for (int by = 0; by < blockHeight; by++)
                    {
                        // Block-Position: Header (4 Bytes) + Tile-Daten
                        // Erster Tile im Block = Position nach Header
                        long blockPos = ((long)bx * blockHeight + by) * 196;
                        long tilePos = blockPos + 4; // Header überspringen

                        if (tilePos + 3 > fs.Length)
                        {
                            _mapBitmap.SetPixel(bx, by, Color.Black);
                            continue;
                        }

                        fs.Seek(tilePos, SeekOrigin.Begin);
                        short tileId = reader.ReadInt16();
                        // Z nicht benötigt für Farbdarstellung auf Block-Ebene
                        reader.ReadByte();

                        // Einfache Farbe basierend auf TileID
                        Color c = GetSimpleTileColor(tileId);
                        _mapBitmap.SetPixel(bx, by, c);
                    }

                    // Fortschritt alle 100 Blöcke anzeigen
                    if (bx % 100 == 0)
                    {
                        _statusLabel_Set($"Lade... {bx * 100 / blockWidth}%");
                        Application.DoEvents();
                    }
                }

                _statusLabel_Set($"Karte geladen: {_mapWidth}×{_mapHeight} Tiles ({blockWidth}×{blockHeight} Blöcke)");
                _mapPanel.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Karte:\n{ex.Message}", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        // -----------------------------------------------------------------------
        // GetSimpleTileColor
        // -----------------------------------------------------------------------

        /// <summary>
        /// Gibt eine einfache Farbe für einen TileID-Wert zurück.
        /// Basiert auf bekannten UO-Tile-Ranges für schnelles Rendering ohne hues.mul.
        /// </summary>
        private Color GetSimpleTileColor(short tileId)
        {
            if (tileId < 0) return Color.DarkBlue;      // Wasser/Void

            // Tile-ID Ranges (ungefähre UO-Standard-Ranges)
            if (tileId == 0x0002 || tileId == 0x0001) return Color.DarkBlue;    // Tiefwasser
            if (tileId >= 0x0002 && tileId <= 0x0030) return Color.Blue;         // Wasser
            if (tileId >= 0x0031 && tileId <= 0x004A) return Color.SandyBrown;   // Sand/Strand
            if (tileId >= 0x004B && tileId <= 0x0080) return Color.ForestGreen;  // Gras
            if (tileId >= 0x0081 && tileId <= 0x00A0) return Color.SaddleBrown;  // Schmutz/Erde
            if (tileId >= 0x00A1 && tileId <= 0x00C0) return Color.DarkGray;     // Stein/Fels
            if (tileId >= 0x00C1 && tileId <= 0x00E0) return Color.Gray;         // Pflaster
            if (tileId >= 0x00E1 && tileId <= 0x0100) return Color.DarkGreen;    // Sumpf/Moor
            if (tileId >= 0x0101 && tileId <= 0x0140) return Color.LightGreen;   // Wiese
            if (tileId >= 0x0141 && tileId <= 0x0160) return Color.White;        // Schnee/Eis
            if (tileId >= 0x0161 && tileId <= 0x0180) return Color.DimGray;      // Lavagestein

            // Fallback: Farbe aus TileID hashen für Vielfalt
            // Cast auf uint nötig: tileId * 2654435761u ergibt long → explizit auf int casten
            int hash = (int)(((uint)tileId * 2654435761u) & 0xFFFFFFu);
            return Color.FromArgb(
                40 + (hash & 0xFF) % 150,
                40 + ((hash >> 8) & 0xFF) % 150,
                40 + ((hash >> 16) & 0xFF) % 150);
        }

        // -----------------------------------------------------------------------
        // Koordinaten-Konvertierung
        // -----------------------------------------------------------------------

        /// <summary>
        /// Wandelt eine Screen-Position (Pixel im mapPanel) in Tile-Koordinaten um.
        /// Berücksichtigt Zoom und ViewOffset.
        /// Das Bitmap repräsentiert 1 Pixel = 1 Block (8 Tiles).
        /// </summary>
        private Point ScreenToTile(Point screenPos)
        {
            // 1 Pixel im Bitmap = 8 Tiles
            // Zoom streckt das Bitmap
            float pixelsPerBlock = _zoom * 8f; // wie viele Screen-Pixel = 1 Block
            // Nein: _zoom = Tiles pro Pixel, daher:
            // Bei zoom=1: 1 Screen-Pixel = 1 Tile
            // Bitmap-Pixel = Block = 8 Tiles

            // Screen → Tile:
            // tileX = viewOffset.X + screenPos.X / zoom
            int tileX = (int)(_viewOffset.X + screenPos.X / _zoom);
            int tileY = (int)(_viewOffset.Y + screenPos.Y / _zoom);

            tileX = Math.Max(0, Math.Min(_mapWidth - 1, tileX));
            tileY = Math.Max(0, Math.Min(_mapHeight - 1, tileY));

            return new Point(tileX, tileY);
        }

        /// <summary>
        /// Wandelt Tile-Koordinaten in Screen-Position um.
        /// </summary>
        private Point TileToScreen(int tileX, int tileY)
        {
            int sx = (int)((tileX - _viewOffset.X) * _zoom);
            int sy = (int)((tileY - _viewOffset.Y) * _zoom);
            return new Point(sx, sy);
        }

        // -----------------------------------------------------------------------
        // MapPanel_Paint
        // -----------------------------------------------------------------------

        private void MapPanel_Paint(object sender, PaintEventArgs e)
        {
            if (_mapBitmap == null)
            {
                e.Graphics.Clear(Color.Black);
                e.Graphics.DrawString("Karte wird geladen...", Font, Brushes.White, 10, 10);
                return;
            }

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            // Das Bitmap hat 1 Pixel = 1 Block = 8 Tiles
            // Wir skalieren das Bitmap so, dass _zoom Tiles pro Screen-Pixel gilt:
            // 1 Bitmap-Pixel = 8 Tiles
            // Bei zoom=1: 1 Screen-Pixel = 1 Tile → 1 Bitmap-Pixel = 8 Screen-Pixel
            // displayScale = zoom * 8

            float displayScale = _zoom * 8f;

            // Quelle: der sichtbare Bereich im Bitmap
            // Ziel: das gesamte Panel
            int panelW = _mapPanel.Width;
            int panelH = _mapPanel.Height;

            // Sichtbarer Tile-Bereich
            float visibleTilesX = panelW / _zoom;
            float visibleTilesY = panelH / _zoom;

            // Entsprechende Bitmap-Koordinaten (1 Block = 8 Tiles)
            float srcX = _viewOffset.X / 8f;
            float srcY = _viewOffset.Y / 8f;
            float srcW = visibleTilesX / 8f;
            float srcH = visibleTilesY / 8f;

            // Clamp auf Bitmap-Grenzen
            srcX = Math.Max(0, srcX);
            srcY = Math.Max(0, srcY);
            srcW = Math.Min(srcW, _mapBitmap.Width - srcX);
            srcH = Math.Min(srcH, _mapBitmap.Height - srcY);

            if (srcW <= 0 || srcH <= 0) return;

            RectangleF srcRect = new RectangleF(srcX, srcY, srcW, srcH);
            Rectangle dstRect = new Rectangle(0, 0, panelW, panelH);

            e.Graphics.DrawImage(_mapBitmap, dstRect, srcRect, GraphicsUnit.Pixel);

            // --- Selektion zeichnen ---
            if (_hasSelection || _isSelecting)
            {
                Point tileMin = new Point(
                    Math.Min(_selectionStartTile.X, _selectionEndTile.X),
                    Math.Min(_selectionStartTile.Y, _selectionEndTile.Y));
                Point tileMax = new Point(
                    Math.Max(_selectionStartTile.X, _selectionEndTile.X),
                    Math.Max(_selectionStartTile.Y, _selectionEndTile.Y));

                Point screenMin = TileToScreen(tileMin.X, tileMin.Y);
                Point screenMax = TileToScreen(tileMax.X, tileMax.Y);

                Rectangle selRect = new Rectangle(
                    screenMin.X, screenMin.Y,
                    screenMax.X - screenMin.X,
                    screenMax.Y - screenMin.Y);

                // Halbtransparente Füllung
                using SolidBrush fillBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 0));
                e.Graphics.FillRectangle(fillBrush, selRect);

                // Rahmen
                using Pen borderPen = new Pen(Color.Yellow, 2f);
                borderPen.DashStyle = DashStyle.Dash;
                e.Graphics.DrawRectangle(borderPen, selRect);

                // Koordinaten-Label an der Selektion
                string coordText = $"[{tileMin.X},{tileMin.Y}] - [{tileMax.X},{tileMax.Y}]";
                SizeF textSize = e.Graphics.MeasureString(coordText, Font);
                float textX = screenMin.X;
                float textY = screenMin.Y - textSize.Height - 2;
                if (textY < 0) textY = screenMax.Y + 2;

                e.Graphics.FillRectangle(Brushes.Black,
                    textX - 1, textY - 1, textSize.Width + 2, textSize.Height + 2);
                e.Graphics.DrawString(coordText, Font, Brushes.Yellow, textX, textY);
            }

            // --- Gitterlinien bei hohem Zoom ---
            if (_zoom >= 4f)
            {
                DrawGrid(e.Graphics);
            }
        }

        /// <summary>Zeichnet ein Tile-Gitter wenn der Zoom hoch genug ist.</summary>
        private void DrawGrid(Graphics g)
        {
            using Pen gridPen = new Pen(Color.FromArgb(30, 255, 255, 255), 0.5f);

            int startTileX = (int)_viewOffset.X;
            int startTileY = (int)_viewOffset.Y;
            int endTileX = startTileX + (int)(_mapPanel.Width / _zoom) + 1;
            int endTileY = startTileY + (int)(_mapPanel.Height / _zoom) + 1;

            // Vertikale Linien
            for (int tx = startTileX; tx <= endTileX; tx += 8)
            {
                Point s = TileToScreen(tx, startTileY);
                Point e = TileToScreen(tx, endTileY);
                g.DrawLine(gridPen, s, e);
            }

            // Horizontale Linien
            for (int ty = startTileY; ty <= endTileY; ty += 8)
            {
                Point s = TileToScreen(startTileX, ty);
                Point e = TileToScreen(endTileX, ty);
                g.DrawLine(gridPen, s, e);
            }
        }

        // -----------------------------------------------------------------------
        // Mouse Events
        // -----------------------------------------------------------------------

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Pan starten
                _isPanning = true;
                _panStart = e.Location;
                _mapPanel.Cursor = Cursors.SizeAll;
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Selektion starten
                _isSelecting = true;
                _hasSelection = false;
                _selectionStartTile = ScreenToTile(e.Location);
                _selectionEndTile = _selectionStartTile;
                _mapPanel.Cursor = Cursors.Cross;
            }
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            // Tile-Koordinaten unter Maus berechnen und anzeigen
            Point tile = ScreenToTile(e.Location);
            _lblCoords.Text = $"Mausposition: X={tile.X}, Y={tile.Y}  |  Tile-ID an Position: (Block {tile.X / 8},{tile.Y / 8})";

            if (_isPanning && e.Button == MouseButtons.Left)
            {
                // Pan: ViewOffset anpassen
                float dx = (e.X - _panStart.X) / _zoom;
                float dy = (e.Y - _panStart.Y) / _zoom;

                _viewOffset.X -= dx;
                _viewOffset.Y -= dy;

                // Grenzen einhalten
                _viewOffset.X = Math.Max(0, Math.Min(_mapWidth - _mapPanel.Width / _zoom, _viewOffset.X));
                _viewOffset.Y = Math.Max(0, Math.Min(_mapHeight - _mapPanel.Height / _zoom, _viewOffset.Y));

                _panStart = e.Location;
                _mapPanel.Invalidate();
            }
            else if (_isSelecting && e.Button == MouseButtons.Right)
            {
                // Selektion aktualisieren
                _selectionEndTile = ScreenToTile(e.Location);

                // Selektion auf 8er-Grid snappen
                _selectionEndTile = SnapToGrid(_selectionEndTile);

                UpdateSelectionLabel();
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
                _selectionEndTile = ScreenToTile(e.Location);
                _selectionEndTile = SnapToGrid(_selectionEndTile);
                _hasSelection = true;

                // Buttons aktivieren
                _btnApplyFrom.Enabled = true;
                _btnApplyTo.Enabled = true;

                UpdateSelectionLabel();
                _mapPanel.Invalidate();
            }
        }

        private void MapPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            // Zoom um die Mausposition herum
            Point tileUnderMouse = ScreenToTile(e.Location);

            if (e.Delta > 0)
                _trackBarZoom.Value = Math.Min(_trackBarZoom.Maximum, _trackBarZoom.Value + 1);
            else
                _trackBarZoom.Value = Math.Max(_trackBarZoom.Minimum, _trackBarZoom.Value - 1);

            // ViewOffset so anpassen, dass der Tile unter der Maus an der gleichen
            // Screen-Position bleibt
            _viewOffset.X = tileUnderMouse.X - e.X / _zoom;
            _viewOffset.Y = tileUnderMouse.Y - e.Y / _zoom;
            _viewOffset.X = Math.Max(0, _viewOffset.X);
            _viewOffset.Y = Math.Max(0, _viewOffset.Y);

            _mapPanel.Invalidate();
        }

        // -----------------------------------------------------------------------
        // Zoom
        // -----------------------------------------------------------------------

        private void TrackBarZoom_ValueChanged(object sender, EventArgs e)
        {
            UpdateZoomFromTrackbar();
            _mapPanel.Invalidate();
        }

        /// <summary>
        /// Wandelt den TrackBar-Wert in einen Zoom-Faktor um.
        /// Werte: 1=0.125x, 4=0.5x, 8=1.0x (1 Pixel = 1 Tile), 16=2.0x, 20=4.0x
        /// </summary>
        private void UpdateZoomFromTrackbar()
        {
            // Zoom-Kurve: 0.0625 bis 4.0
            float[] zoomLevels = {
                0.0625f, 0.125f, 0.1875f, 0.25f, 0.375f, 0.5f, 0.625f, 0.75f,
                0.875f, 1.0f, 1.25f, 1.5f, 2.0f, 2.5f, 3.0f, 4.0f, 5.0f, 6.0f, 8.0f, 10.0f
            };

            int idx = Math.Max(0, Math.Min(zoomLevels.Length - 1, _trackBarZoom.Value - 1));
            _zoom = zoomLevels[idx];

            int percent = (int)(_zoom * 100);
            _lblZoom.Text = $"Zoom: {percent}%";
        }

        // -----------------------------------------------------------------------
        // Snapping
        // -----------------------------------------------------------------------

        /// <summary>
        /// Snappt eine Tile-Koordinate auf das nächste 8er-Raster (Block-Grenze).
        /// Das erleichtert die Auswahl ganzer Blöcke.
        /// </summary>
        private Point SnapToGrid(Point tile)
        {
            // Auf nächste 8er-Grenze runden
            return new Point(
                (int)Math.Round(tile.X / 8.0) * 8,
                (int)Math.Round(tile.Y / 8.0) * 8);
        }

        // -----------------------------------------------------------------------
        // Selektion übernehmen
        // -----------------------------------------------------------------------

        private void BtnApplyFrom_Click(object sender, EventArgs e)
        {
            if (!_hasSelection) return;

            int x1 = Math.Min(_selectionStartTile.X, _selectionEndTile.X);
            int y1 = Math.Min(_selectionStartTile.Y, _selectionEndTile.Y);
            int x2 = Math.Max(_selectionStartTile.X, _selectionEndTile.X);
            int y2 = Math.Max(_selectionStartTile.Y, _selectionEndTile.Y);

            _setFromRegion?.Invoke(x1, y1, x2, y2);

            MessageBox.Show(
                $"From Region übertragen:\nX1={x1}, Y1={y1}\nX2={x2}, Y2={y2}",
                "Koordinaten übertragen",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnApplyTo_Click(object sender, EventArgs e)
        {
            if (!_hasSelection) return;

            int x1 = Math.Min(_selectionStartTile.X, _selectionEndTile.X);
            int y1 = Math.Min(_selectionStartTile.Y, _selectionEndTile.Y);

            _setToRegion?.Invoke(x1, y1);

            MessageBox.Show(
                $"To Region übertragen:\nToX={x1}, ToY={y1}",
                "Koordinaten übertragen",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // -----------------------------------------------------------------------
        // Hilfsmethoden
        // -----------------------------------------------------------------------

        private void UpdateSelectionLabel()
        {
            int x1 = Math.Min(_selectionStartTile.X, _selectionEndTile.X);
            int y1 = Math.Min(_selectionStartTile.Y, _selectionEndTile.Y);
            int x2 = Math.Max(_selectionStartTile.X, _selectionEndTile.X);
            int y2 = Math.Max(_selectionStartTile.Y, _selectionEndTile.Y);
            int w = x2 - x1;
            int h = y2 - y1;

            _lblSelection.Text =
                $"Selektion: X1={x1}, Y1={y1} → X2={x2}, Y2={y2}  |  Größe: {w}×{h} Tiles ({w / 8}×{h / 8} Blöcke)";
        }

        private void _statusLabel_Set(string text)
        {
            // StatusStrip ist optional - wir nutzen das Selection-Label
            if (_lblSelection != null)
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
            }
            base.Dispose(disposing);
        }
    }
}
