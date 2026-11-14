/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * CompareMapControl – Bereich markieren und im CGI-Vergleich öffnen
 * (Map1 und Map2 nebeneinander in der Client-Ansicht, kopieren zwischen den Karten).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.MapViewer;
using UoFiddler.Plugin.Compare.Forms;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareMapControl
    {
        private const int QuickAreaSize = 64;

        private bool _selecting;
        private Point _selectionStart;
        private Rectangle? _selection;
        private Point _contextWorld;

        // edit sessions of both maps: owned here so changes survive closing the CGI window
        private RegionEditSession _editSession1;
        private RegionEditSession _editSession2;
        private CompareCgiForm _cgiForm;

        private ToolStripSeparator _toolStripSeparatorCgi;
        private ToolStripButton _toolStripButtonSelectArea;
        private ToolStripButton _toolStripButtonCgi;
        private ToolStripSeparator _contextSeparatorCgi;
        private ToolStripMenuItem _cgiSelectionMenuItem;
        private ToolStripMenuItem _cgiHereMenuItem;
        private ToolStripMenuItem _clearSelectionMenuItem;

        // ─────────────────────────────────────────────────────────────────
        // UI
        // ─────────────────────────────────────────────────────────────────
        private void InitCgiCompare()
        {
            _toolStripSeparatorCgi = new ToolStripSeparator();

            _toolStripButtonSelectArea = new ToolStripButton
            {
                Text = "▭ Bereich markieren",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                CheckOnClick = true,
                ForeColor = Color.White,
                ToolTipText = "Linke Maustaste zieht einen Bereich auf (auch ohne diesen Schalter: Shift + Ziehen)"
            };
            _toolStripButtonSelectArea.CheckedChanged += (s, e) =>
                pictureBox.Cursor = _toolStripButtonSelectArea.Checked ? Cursors.Cross : Cursors.Default;

            _toolStripButtonCgi = new ToolStripButton
            {
                Text = "CGI-Vergleich…",
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Enabled = false,
                ForeColor = Color.White,
                ToolTipText = "Markierten Bereich von Map1 und Map2 nebeneinander in der Client-Ansicht öffnen (Statics, kopieren)"
            };
            _toolStripButtonCgi.Click += (s, e) => OpenCgiCompareForSelection();

            int index = toolStripNav.Items.IndexOf(toolStripProgressBar);
            toolStripNav.Items.Insert(index, _toolStripButtonCgi);
            toolStripNav.Items.Insert(index, _toolStripButtonSelectArea);
            toolStripNav.Items.Insert(index, _toolStripSeparatorCgi);

            _contextSeparatorCgi = new ToolStripSeparator();
            _cgiSelectionMenuItem = new ToolStripMenuItem("CGI-Vergleich der Markierung…", null, (s, e) => OpenCgiCompareForSelection());
            _cgiHereMenuItem = new ToolStripMenuItem($"CGI-Vergleich hier ({QuickAreaSize} × {QuickAreaSize})…", null, (s, e) => OpenCgiCompareHere());
            _clearSelectionMenuItem = new ToolStripMenuItem("Markierung aufheben", null, (s, e) => ClearSelection());
            contextMenuStrip1.Items.AddRange(new ToolStripItem[]
            {
                _contextSeparatorCgi, _cgiSelectionMenuItem, _cgiHereMenuItem, _clearSelectionMenuItem
            });
            contextMenuStrip1.Opening += (s, e) =>
            {
                _contextWorld = ScreenToWorld(pictureBox.PointToClient(MousePosition));
                _cgiSelectionMenuItem.Enabled = _selection.HasValue;
                _clearSelectionMenuItem.Enabled = _selection.HasValue;
            };
        }

        // ─────────────────────────────────────────────────────────────────
        // Coordinates
        // ─────────────────────────────────────────────────────────────────
        private Map RenderMap => (!showMap1ToolStripMenuItem.Checked && _currentMap != null) ? _currentMap : _originalMap;

        /// <summary>World position of the upper left corner of the picture (same as in OnPaint)</summary>
        private Point ViewOrigin()
        {
            Map map = RenderMap;
            if (map == null)
            {
                return Point.Empty;
            }

            int bx = Math.Max(0, Math.Min(hScrollBar.Value >> 3, (map.Width >> 3) - 1));
            int by = Math.Max(0, Math.Min(vScrollBar.Value >> 3, (map.Height >> 3) - 1));
            return new Point(bx << 3, by << 3);
        }

        private Point ScreenToWorld(Point screen)
        {
            Map map = RenderMap;
            if (map == null)
            {
                return Point.Empty;
            }

            Point origin = ViewOrigin();
            int x = origin.X + (int)Math.Floor(screen.X / _zoom);
            int y = origin.Y + (int)Math.Floor(screen.Y / _zoom);
            return new Point(Math.Max(0, Math.Min(map.Width - 1, x)), Math.Max(0, Math.Min(map.Height - 1, y)));
        }

        // ─────────────────────────────────────────────────────────────────
        // Selection (called from the mouse handlers)
        // ─────────────────────────────────────────────────────────────────
        private bool WantsSelection(MouseEventArgs e)
        {
            return e.Button == MouseButtons.Left && _originalMap != null &&
                   (_toolStripButtonSelectArea.Checked || (ModifierKeys & Keys.Shift) == Keys.Shift);
        }

        private void BeginSelection(Point screen)
        {
            _selecting = true;
            _selectionStart = ScreenToWorld(screen);
            _selection = new Rectangle(_selectionStart, new Size(1, 1));
            toolTip1.RemoveAll();
            pictureBox.Invalidate();
        }

        private void UpdateSelection(Point screen)
        {
            Point p = ScreenToWorld(screen);
            int x1 = Math.Min(_selectionStart.X, p.X), y1 = Math.Min(_selectionStart.Y, p.Y);
            int x2 = Math.Max(_selectionStart.X, p.X), y2 = Math.Max(_selectionStart.Y, p.Y);
            _selection = Rectangle.FromLTRB(x1, y1, x2 + 1, y2 + 1);
            CoordsLabel.Text = $"Markierung: {_selection.Value.Width} × {_selection.Value.Height}";
            pictureBox.Invalidate();
        }

        private void EndSelection()
        {
            _selecting = false;
            UpdateSelectionState();
            pictureBox.Invalidate();
        }

        private void ClearSelection()
        {
            _selection = null;
            UpdateSelectionState();
            pictureBox.Invalidate();
        }

        private void UpdateSelectionState()
        {
            _toolStripButtonCgi.Enabled = _selection.HasValue;
            if (_selection.HasValue)
            {
                Rectangle s = _selection.Value;
                _toolStripButtonCgi.ToolTipText =
                    $"Markierung {s.X},{s.Y} – {s.Right - 1},{s.Bottom - 1} ({s.Width} × {s.Height}) im CGI-Vergleich öffnen";
            }
        }

        /// <summary>Draws the marked area on top of the map picture</summary>
        private void DrawSelection(Graphics g)
        {
            if (!_selection.HasValue)
            {
                return;
            }

            Point origin = ViewOrigin();
            Rectangle s = _selection.Value;
            var rect = new RectangleF(
                (float)((s.X - origin.X) * _zoom),
                (float)((s.Y - origin.Y) * _zoom),
                (float)Math.Max(1, s.Width * _zoom),
                (float)Math.Max(1, s.Height * _zoom));

            using (var fill = new SolidBrush(Color.FromArgb(40, Color.DeepSkyBlue)))
            using (var pen = new Pen(Color.DeepSkyBlue, 2f) { DashStyle = _selecting ? DashStyle.Dash : DashStyle.Solid })
            {
                g.FillRectangle(fill, rect);
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }

            string text = $"{s.Width} × {s.Height}" + (s.Width > CompareCgiForm.MaxAreaSize || s.Height > CompareCgiForm.MaxAreaSize
                ? $"  (max. {CompareCgiForm.MaxAreaSize})"
                : string.Empty);
            using (var font = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var back = new SolidBrush(Color.FromArgb(170, 0, 0, 0)))
            {
                SizeF size = g.MeasureString(text, font);
                float tx = rect.X + 2, ty = Math.Max(0, rect.Y - size.Height - 2);
                g.FillRectangle(back, tx, ty, size.Width, size.Height);
                g.DrawString(text, font, Brushes.White, tx, ty);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Open the CGI compare window
        // ─────────────────────────────────────────────────────────────────
        private void OpenCgiCompareForSelection()
        {
            if (!_selection.HasValue)
            {
                MessageBox.Show("Bitte zuerst einen Bereich markieren:\n\"▭ Bereich markieren\" einschalten oder Shift + linke Maustaste ziehen.",
                    "CGI-Vergleich", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            OpenCgiCompare(_selection.Value);
        }

        private void OpenCgiCompareHere()
        {
            var area = new Rectangle(_contextWorld.X - (QuickAreaSize / 2), _contextWorld.Y - (QuickAreaSize / 2), QuickAreaSize, QuickAreaSize);
            area.X = Math.Max(0, area.X);
            area.Y = Math.Max(0, area.Y);
            _selection = area;
            UpdateSelectionState();
            pictureBox.Invalidate();
            OpenCgiCompare(area);
        }

        private void OpenCgiCompare(Rectangle area)
        {
            if (_currentMap == null || _originalMap == null)
            {
                MessageBox.Show("Bitte zuerst Map2 laden (Ordner wählen und \"Load\").", "CGI-Vergleich",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            area.Intersect(new Rectangle(0, 0, Math.Min(_originalMap.Width, _currentMap.Width), Math.Min(_originalMap.Height, _currentMap.Height)));
            if (area.Width <= 0 || area.Height <= 0)
            {
                return;
            }

            if (area.Width > CompareCgiForm.MaxAreaSize || area.Height > CompareCgiForm.MaxAreaSize)
            {
                area.Width = Math.Min(area.Width, CompareCgiForm.MaxAreaSize);
                area.Height = Math.Min(area.Height, CompareCgiForm.MaxAreaSize);
                MessageBox.Show($"Der Bereich ist größer als {CompareCgiForm.MaxAreaSize} × {CompareCgiForm.MaxAreaSize} und wird auf " +
                                $"{area.Width} × {area.Height} ab {area.X},{area.Y} begrenzt.", "CGI-Vergleich",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            EnsureEditSessions();

            if (_cgiForm != null && !_cgiForm.IsDisposed)
            {
                _cgiForm.LoadArea(area);
                if (_cgiForm.WindowState == FormWindowState.Minimized)
                {
                    _cgiForm.WindowState = FormWindowState.Normal;
                }

                _cgiForm.Activate();
                return;
            }

            _cgiForm = new CompareCgiForm(_originalMap, Map1DisplayName(), _editSession1,
                                          _currentMap, Map2DisplayName(), _editSession2,
                                          area, OnCgiMapsChanged);
            _cgiForm.FormClosed += (s, e) => _cgiForm = null;
            _cgiForm.Show(FindForm());
        }

        private string Map1DisplayName()
        {
            string name = _currentMapId >= 0 && _currentMapId < Options.MapNames.Length ? Options.MapNames[_currentMapId] : $"Map {_currentMapId}";
            return $"Map1 ({name})";
        }

        private string Map2DisplayName()
        {
            string path = toolStripTextBox1.Text?.TrimEnd('\\', '/') ?? string.Empty;
            string folder = string.IsNullOrEmpty(path) ? "Custom" : Path.GetFileName(path);
            return $"Map2 ({folder})";
        }

        private void EnsureEditSessions()
        {
            if (_editSession1 == null || _editSession1.Map != _originalMap)
            {
                _editSession1 = new RegionEditSession(_originalMap);
            }

            if (_editSession2 == null || _editSession2.Map != _currentMap)
            {
                _editSession2 = new RegionEditSession(_currentMap);
            }
        }

        /// <summary>
        /// Asks before unsaved changes of the CGI compare are dropped (map change, reload).
        /// Returns false if the user wants to keep them.
        /// </summary>
        private bool ConfirmDiscardEdits()
        {
            int count = (_editSession1?.ChangeCount ?? 0) + (_editSession2?.ChangeCount ?? 0);
            if (count == 0)
            {
                return true;
            }

            if (MessageBox.Show($"Es gibt {count} ungespeicherte Änderung(en) aus dem CGI-Vergleich.\n\nVerwerfen und fortfahren?",
                    "CGI-Vergleich", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return false;
            }

            DiscardEditSessions();
            return true;
        }

        /// <summary>Closes the CGI window and drops all changes in memory</summary>
        private void DiscardEditSessions()
        {
            if (_cgiForm != null && !_cgiForm.IsDisposed)
            {
                _cgiForm.CloseSilently();
            }

            _cgiForm = null;

            if (_editSession1 != null && _editSession1.ChangeCount > 0)
            {
                _editSession1.UndoAll();
            }

            if (_editSession2 != null && _editSession2.ChangeCount > 0)
            {
                _editSession2.UndoAll();
            }

            _editSession1 = null;
            _editSession2 = null;
        }

        /// <summary>A map was changed in the CGI window: redraw and update the differences</summary>
        private void OnCgiMapsChanged(Rectangle? area)
        {
            _originalMap?.ResetCache();
            _currentMap?.ResetCache();

            if (!area.HasValue || _calculatingDiffs)
            {
                _ = CalculateDiffsAsync();
            }
            else
            {
                UpdateDiffsInArea(area.Value);
            }

            pictureBox.Invalidate();
        }

        /// <summary>Recalculates the differences of the blocks of an area only (after a copy)</summary>
        private void UpdateDiffsInArea(Rectangle area)
        {
            if (_currentMap == null || _originalMap == null)
            {
                return;
            }

            int blocksW = Math.Min(_currentMap.Width, _originalMap.Width) >> 3;
            int blocksH = Math.Min(_currentMap.Height, _originalMap.Height) >> 3;
            int bx1 = Math.Max(0, area.Left >> 3), by1 = Math.Max(0, area.Top >> 3);
            int bx2 = Math.Min(blocksW - 1, (area.Right - 1) >> 3), by2 = Math.Min(blocksH - 1, (area.Bottom - 1) >> 3);

            for (int x = bx1; x <= bx2; x++)
            {
                for (int y = by1; y <= by2; y++)
                {
                    for (int xb = 0; xb < 8; xb++)
                    {
                        for (int yb = 0; yb < 8; yb++)
                        {
                            _diffTileSet.Remove(new Point((x * 8) + xb, (y * 8) + yb));
                        }
                    }

                    var block = new Point(x, y);
                    if (CompareBlock(_currentMap, _originalMap, x, y, _diffTileSet))
                    {
                        _diffBlockSet.Add(block);
                    }
                    else
                    {
                        _diffBlockSet.Remove(block);
                    }
                }
            }

            _diffBlocks = _diffBlockSet.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            _currentDiffIndex = Math.Min(_currentDiffIndex, _diffBlocks.Count - 1);
            UpdateDiffStats(_diffBlocks.Count);
            NotifyDiffListDialog();
        }

        /// <summary>
        /// Compares one 8x8 block of both maps. Adds the changed tiles to <paramref name="diffTiles"/>.
        /// Returns true if the block differs.
        /// </summary>
        private static bool CompareBlock(Map curMap, Map origMap, int x, int y, ICollection<Point> diffTiles)
        {
            Tile[] customTiles = curMap.Tiles.GetLandBlock(x, y);
            Tile[] origTiles = origMap.Tiles.GetLandBlock(x, y);
            HuedTile[][][] customStatics = curMap.Tiles.GetStaticBlock(x, y);
            HuedTile[][][] origStatics = origMap.Tiles.GetStaticBlock(x, y);

            bool blockDiffers = false;

            for (int xb = 0; xb < 8; xb++)
            {
                for (int yb = 0; yb < 8; yb++)
                {
                    int idx = ((yb & 7) << 3) + (xb & 7);
                    bool tileDiffers = false;

                    if (customTiles[idx].Id != origTiles[idx].Id
                     || customTiles[idx].Z != origTiles[idx].Z)
                    {
                        tileDiffers = true;
                    }
                    else if (customStatics[xb][yb].Length != origStatics[xb][yb].Length)
                    {
                        tileDiffers = true;
                    }
                    else
                    {
                        for (int i = 0; i < customStatics[xb][yb].Length; i++)
                        {
                            if (customStatics[xb][yb][i].Id != origStatics[xb][yb][i].Id
                             || customStatics[xb][yb][i].Z != origStatics[xb][yb][i].Z
                             || customStatics[xb][yb][i].Hue != origStatics[xb][yb][i].Hue)
                            {
                                tileDiffers = true;
                                break;
                            }
                        }
                    }

                    if (tileDiffers)
                    {
                        diffTiles.Add(new Point((x * 8) + xb, (y * 8) + yb));
                        blockDiffers = true;
                    }
                }
            }

            return blockDiffers;
        }
    }
}
