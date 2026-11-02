/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Maps;
using UoFiddler.Plugin.Enhanced.EcUltima.Multis;

namespace UoFiddler.Plugin.Enhanced.Forms
{
    /// <summary>
    /// Region viewer of the Enhanced Client map (like "View Region" of the Map tab): isometric view of a marked area
    /// with land and statics in EC images, select / add / edit / remove statics, import and export the statics of the area.
    /// Changes go into the facet in memory; "Save map" writes the facet file.
    /// </summary>
    public sealed partial class EcRegionViewerForm : Form
    {
        /// <summary>Biggest area side in tiles (EC images are big: 96 tiles ≈ 6300 pixels).</summary>
        public const int MaxAreaEc = 96;
        public const int MaxAreaLegacy = 160;

        private readonly EcClient _client;
        private readonly EcFacet _facet;
        private readonly byte[] _radar;
        private readonly Action _changed;
        private readonly Action _save;
        private readonly EcItemSprites _sprites;
        private Rectangle _area;
        private EcMapRenderResult _render;
        private Point _tile = new Point(-1, -1);
        private EcPlacedStatic _selected;
        private bool _updating;

        /// <summary>Designer only.</summary>
        public EcRegionViewerForm()
        {
            InitializeComponent();
        }

        public EcRegionViewerForm(EcClient client, EcFacet facet, Rectangle area, byte[] radarDds, Action changed, Action save)
        {
            InitializeComponent();
            _client = client;
            _facet = facet;
            _radar = radarDds;
            _changed = changed;
            _save = save;
            _sprites = new EcItemSprites(client);
            _area = facet.Clip(area);

            _updating = true;
            toolStripComboBoxArt.SelectedIndex = 0;
            toolStripTextBoxMaxZ.Text = "127";
            toolStripComboBoxTool.SelectedIndex = 0;
            toolStripComboBoxBrush.SelectedIndex = 0;
            _updating = false;
            previewPanel.Overlay = DrawOverlay;
            Text = $"Region Viewer (Enhanced Client) – {facet.FileName}  {_area.X},{_area.Y} – {_area.Right - 1},{_area.Bottom - 1}  ({_area.Width} x {_area.Height})";
            Disposed += (s, e) => _sprites.Clear();
            Shown += (s, e) => Redraw();
        }

        private bool Legacy => toolStripComboBoxArt.SelectedIndex == 1;

        // ------------------------------------------------------------------ drawing

        private void Redraw()
        {
            if (_facet == null)
            {
                return;
            }

            int max = Legacy ? MaxAreaLegacy : MaxAreaEc;
            Rectangle area = _area;
            if (area.Width > max || area.Height > max)
            {
                area = new Rectangle(area.X, area.Y, Math.Min(area.Width, max), Math.Min(area.Height, max));
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                toolStripStatusLabel.Text = "Drawing...";
                Application.DoEvents();
                int.TryParse(toolStripTextBoxMaxZ.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int maxZ);
                var options = new EcMapRenderOptions
                {
                    Legacy = Legacy,
                    ShowLand = toolStripButtonLand.Checked,
                    LandArt = toolStripButtonLandArt.Checked,
                    ShowStatics = toolStripButtonStatics.Checked,
                    Grid = toolStripButtonGrid.Checked,
                    MaxZ = toolStripTextBoxMaxZ.Text.Trim().Length == 0 ? 127 : maxZ
                };
                _render = EcMapRenderer.Render(_facet, area, _radar, _sprites, options);
                previewPanel.Image = _render.Image;

                // keep the selection after a redraw
                if (_selected != null)
                {
                    _selected = _render.Statics.FirstOrDefault(p => ReferenceEquals(p.Static, _selected.Static));
                }
                int ec = _render.Statics.Count(p => p.Sprite.IsEnhanced);
                toolStripStatusLabel.Text = $"{area.Width} x {area.Height} tiles, {_render.Statics.Count} statics" +
                                            (Legacy ? string.Empty : $" ({ec} with EC image, {_render.Statics.Count - ec} legacy)") +
                                            (area != _area ? $"  –  only {area.Width} x {area.Height} of {_area.Width} x {_area.Height} tiles are shown" : string.Empty);
                ShowTile();
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void DrawOverlay(Graphics g, Rectangle area, float zoom)
        {
            if (_render == null)
            {
                return;
            }
            var state = g.Save();
            g.TranslateTransform(area.X, area.Y);
            g.ScaleTransform(zoom, zoom);

            if (_tile.X >= 0 && _render.Area.Contains(_tile))
            {
                int half = _render.TileHalf;
                int x = _tile.X - _render.Area.X, y = _tile.Y - _render.Area.Y;
                int z = _facet.GetTile(_tile.X, _tile.Y)?.Z ?? 0;
                int cx = ((x - y) * half) + _render.Origin.X;
                int top = ((x + y) * half) + _render.Origin.Y - (z * _render.ZStep);
                var diamond = new[] { new Point(cx, top), new Point(cx + half, top + half), new Point(cx, top + (2 * half)), new Point(cx - half, top + half) };
                using (var brush = new SolidBrush(Color.FromArgb(60, Color.Gold)))
                using (var pen = new Pen(Color.Gold, 1.5f / zoom))
                {
                    g.FillPolygon(brush, diamond);
                    g.DrawPolygon(pen, diamond);
                }
            }
            if (_selected != null)
            {
                using (var pen = new Pen(Color.OrangeRed, 2f / zoom))
                {
                    g.DrawRectangle(pen, _selected.Rect);
                }
            }
            g.Restore(state);
        }

        // ------------------------------------------------------------------ selection

        private void ShowTile()
        {
            _updating = true;
            listViewTileStatics.BeginUpdate();
            listViewTileStatics.Items.Clear();
            EcMapTile tile = _tile.X >= 0 ? _facet.GetTile(_tile.X, _tile.Y) : null;
            if (tile == null)
            {
                labelTileInfo.Text = "Click on the map to select a tile or a static.";
            }
            else
            {
                labelTileInfo.Text = $"Tile {_tile.X}, {_tile.Y}\r\nland 0x{tile.LandId:X4}  z {tile.Z}\r\n{tile.Statics.Count} statics";
                foreach (EcStatic st in tile.Statics)
                {
                    EcItemSprite sprite = _sprites.Get(st.ItemId, Legacy);
                    var item = new ListViewItem($"0x{st.ItemId:X4}") { Tag = st };
                    item.SubItems.Add(sprite.Name ?? string.Empty);
                    item.SubItems.Add(st.Z.ToString(CultureInfo.InvariantCulture));
                    item.SubItems.Add(st.Hue.ToString(CultureInfo.InvariantCulture));
                    if (!sprite.IsEnhanced && !Legacy)
                    {
                        item.ForeColor = Color.Firebrick;
                    }
                    listViewTileStatics.Items.Add(item);
                    if (_selected != null && ReferenceEquals(_selected.Static, st))
                    {
                        item.Selected = true;
                    }
                }
            }
            listViewTileStatics.EndUpdate();
            _updating = false;
            ShowStaticImage();
            previewPanel.Invalidate();
        }

        private EcStatic SelectedStatic =>
            listViewTileStatics.SelectedItems.Count == 1 ? listViewTileStatics.SelectedItems[0].Tag as EcStatic : _selected?.Static;

        private void ShowStaticImage()
        {
            EcStatic st = SelectedStatic;
            EcItemSprite sprite = st != null ? _sprites.Get(st.ItemId, Legacy) : null;
            pictureBoxStatic.Image = sprite?.Image;
            if (sprite != null)
            {
                toolStripStatusLabel.Text = $"0x{st.ItemId:X4} {sprite.Name}  z {st.Z}  hue {st.Hue}  –  {sprite.Source ?? "no image"}";
            }
        }

        // ------------------------------------------------------------------ editing

        private bool AskStatic(string title, EcStatic st)
        {
            string[] values = EcDialogs.Prompt(this, title, new[] { "Item id:", "z:", "Hue:" },
                new[] { $"0x{st.ItemId:X4}", st.Z.ToString(CultureInfo.InvariantCulture), st.Hue.ToString(CultureInfo.InvariantCulture) });
            if (values == null || !EcDialogs.TryParseNumber(values[0], out long id) || !EcDialogs.TryParseNumber(values[1], out long z) || !EcDialogs.TryParseNumber(values[2], out long hue))
            {
                return false;
            }
            st.ItemId = (uint)id;
            st.Z = (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, z));
            st.Hue = (uint)hue;
            return true;
        }

        private void Changed()
        {
            _changed?.Invoke();
            Redraw();
        }

        private void AddStatic()
        {
            EcMapTile tile = _tile.X >= 0 ? _facet.GetTile(_tile.X, _tile.Y) : null;
            if (tile == null)
            {
                EcDialogs.ShowError(this, "Click on a tile first.");
                return;
            }
            var st = new EcStatic { Z = tile.Z, ItemId = _selected?.Static.ItemId ?? 0 };
            if (AskStatic($"Add static at {_tile.X}, {_tile.Y}", st) && _facet.AddStatic(_tile.X, _tile.Y, st))
            {
                _selected = new EcPlacedStatic { X = _tile.X, Y = _tile.Y, Static = st };
                Changed();
            }
        }

        private void EditStatic()
        {
            EcStatic st = SelectedStatic;
            if (st == null)
            {
                return;
            }
            if (AskStatic("Edit static", st))
            {
                _facet.MarkModified(_tile.X, _tile.Y);
                Changed();
            }
        }

        private void RemoveStatic()
        {
            EcStatic st = SelectedStatic;
            if (st == null)
            {
                return;
            }
            if (_facet.RemoveStatic(_tile.X, _tile.Y, st))
            {
                _selected = null;
                Changed();
            }
        }

        // ------------------------------------------------------------------ tools (place, paint, move)

        private enum Tool
        {
            Select,
            PlaceItem,
            PaintLand,
            PlaceMulti
        }

        private Tool CurrentTool => (Tool)Math.Max(0, toolStripComboBoxTool.SelectedIndex);

        private EcPlacedStatic _drag;
        private Point _dragTile;
        private bool _painting;
        private Rectangle _painted;

        private static bool TryNumber(string text, out long value) => EcDialogs.TryParseNumber(text.Trim(), out value);

        /// <summary>z from the tool bar, or the z of the land when the field is empty.</summary>
        private sbyte ToolZ(EcMapTile tile)
        {
            return TryNumber(toolStripTextBoxZ.Text, out long z) ? (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, z)) : tile.Z;
        }

        private void PlaceItem(Point tilePos)
        {
            EcMapTile tile = _facet.GetTile(tilePos.X, tilePos.Y);
            if (tile == null)
            {
                return;
            }
            if (!TryNumber(toolStripTextBoxItem.Text, out long id) || id < 0)
            {
                PickItem();
                return;
            }
            var st = new EcStatic { ItemId = (uint)id, Z = ToolZ(tile) };
            if (_facet.AddStatic(tilePos.X, tilePos.Y, st))
            {
                _tile = tilePos;
                _selected = new EcPlacedStatic { X = tilePos.X, Y = tilePos.Y, Static = st };
                Changed();
                toolStripStatusLabel.Text = $"0x{id:X4} placed at {tilePos.X}, {tilePos.Y}, {st.Z}. Click again to place more, right click / \"Select\" to stop.";
            }
        }

        private void PlaceMulti(Point tilePos)
        {
            EcMapTile tile = _facet.GetTile(tilePos.X, tilePos.Y);
            if (tile == null)
            {
                return;
            }
            if (!TryNumber(toolStripTextBoxMulti.Text, out long id) || id < 0)
            {
                EcDialogs.ShowError(this, "Enter the multi id in the tool bar (field \"Multi\").");
                return;
            }
            byte[] data = _client.GetPackage("MultiCollection.uop")?.ReadData(string.Format(CultureInfo.InvariantCulture, EcMulti.NamePattern, id));
            if (data == null)
            {
                EcDialogs.ShowError(this, $"Multi {id} is not in MultiCollection.uop.");
                return;
            }
            EcMulti multi = EcMulti.Read(data);
            sbyte baseZ = ToolZ(tile);
            int placed = 0, outside = 0;
            foreach (EcMultiPart part in multi.Parts.Where(p => p.Flags == 0))
            {
                var st = new EcStatic { ItemId = part.ItemId, Z = (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, baseZ + part.Z)) };
                if (_facet.AddStatic(tilePos.X + part.X, tilePos.Y + part.Y, st))
                {
                    placed++;
                }
                else
                {
                    outside++;
                }
            }
            var b = multi.Bounds();
            _area = _facet.Clip(Rectangle.Union(_area, new Rectangle(tilePos.X + b.MinX, tilePos.Y + b.MinY, b.MaxX - b.MinX + 1, b.MaxY - b.MinY + 1)));
            Changed();
            toolStripStatusLabel.Text = $"Multi {id}: {placed} parts placed at {tilePos.X}, {tilePos.Y}, {baseZ}" + (outside > 0 ? $", {outside} outside the map" : string.Empty) + ".";
        }

        private void PaintAt(Point tilePos)
        {
            if (!TryNumber(toolStripTextBoxLand.Text, out long land) || land < 0 || land > ushort.MaxValue)
            {
                _painting = false;
                EcDialogs.ShowError(this, "Enter the land tile id in the tool bar (field \"Land\").");
                return;
            }
            sbyte? z = TryNumber(toolStripTextBoxZ.Text, out long zv) ? (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, zv)) : (sbyte?)null;
            int size = new[] { 1, 3, 5, 9 }[Math.Max(0, toolStripComboBoxBrush.SelectedIndex)];
            var brush = new Rectangle(tilePos.X - (size / 2), tilePos.Y - (size / 2), size, size);
            for (int x = brush.Left; x < brush.Right; x++)
            {
                for (int y = brush.Top; y < brush.Bottom; y++)
                {
                    _facet.SetLand(x, y, (ushort)land, z);
                }
            }
            _painted = _painted.IsEmpty ? brush : Rectangle.Union(_painted, brush);
            toolStripStatusLabel.Text = $"Painting land 0x{land:X4} ... ({_painted.Width} x {_painted.Height} tiles)";
        }

        private void EndPaint()
        {
            if (!_painting)
            {
                return;
            }
            _painting = false;
            if (!_painted.IsEmpty)
            {
                int borders = _facet.RefreshBorders(_painted);
                Rectangle area = _painted;
                _painted = Rectangle.Empty;
                Changed();
                toolStripStatusLabel.Text = $"Land painted in {area.Width} x {area.Height} tiles ({borders} border tiles updated for the terrain blending).";
            }
        }

        private void MoveSelected(int dx, int dy, int dz)
        {
            EcStatic st = SelectedStatic;
            if (st == null || _tile.X < 0)
            {
                return;
            }
            if (dz != 0)
            {
                st.Z = (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, st.Z + dz));
                _facet.MarkModified(_tile.X, _tile.Y);
            }
            if (dx != 0 || dy != 0)
            {
                var target = new Point(_tile.X + dx, _tile.Y + dy);
                if (!_facet.MoveStatic(_tile.X, _tile.Y, st, target.X, target.Y))
                {
                    return;
                }
                _tile = target;
            }
            _selected = new EcPlacedStatic { X = _tile.X, Y = _tile.Y, Static = st };
            Changed();
            toolStripStatusLabel.Text = $"0x{st.ItemId:X4} at {_tile.X}, {_tile.Y}, {st.Z}";
        }

        private void PickItem()
        {
            TryNumber(toolStripTextBoxItem.Text, out long current);
            using (var picker = new EcItemPickerForm(_client, _sprites, (uint)Math.Max(0, current)))
            {
                if (picker.ShowDialog(this) == DialogResult.OK)
                {
                    toolStripTextBoxItem.Text = $"0x{picker.SelectedId:X4}";
                    toolStripComboBoxTool.SelectedIndex = (int)Tool.PlaceItem;
                    toolStripStatusLabel.Text = $"Click on the map to place 0x{picker.SelectedId:X4}.";
                }
            }
        }

        // ------------------------------------------------------------------ import / export

        private void ExportStatics(EcStaticsFormat format)
        {
            string message = EcStaticsUi.Export(this, _facet, _area, format);
            if (message != null)
            {
                toolStripStatusLabel.Text = message;
            }
        }

        private void ImportStatics()
        {
            string message = EcStaticsUi.Import(this, _facet, _area, _tile, out Rectangle target);
            if (message == null)
            {
                return;
            }
            // show the imported area too
            _area = _facet.Clip(Rectangle.Union(_area, target));
            Changed();
            toolStripStatusLabel.Text = message;
        }

        private void ClearStatics()
        {
            if (!EcDialogs.Confirm(this, $"Remove all statics of the area {_area.X},{_area.Y} – {_area.Right - 1},{_area.Bottom - 1}?"))
            {
                return;
            }
            int removed = _facet.ClearStatics(_area);
            _selected = null;
            Changed();
            toolStripStatusLabel.Text = $"{removed} statics removed.";
        }

        private void SaveImage()
        {
            if (_render?.Image == null)
            {
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = "PNG|*.png|BMP|*.bmp|JPEG|*.jpg", FileName = $"facet{_facet.FacetId}_{_area.X}_{_area.Y}.png" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string ext = Path.GetExtension(dialog.FileName).ToLowerInvariant();
                    _render.Image.Save(dialog.FileName, ext == ".bmp" ? ImageFormat.Bmp : ext == ".jpg" ? ImageFormat.Jpeg : ImageFormat.Png);
                }
            }
        }

        // ==================================================================== event handlers (designer)

        private void OnViewOptionChanged(object sender, EventArgs e)
        {
            if (!_updating && _facet != null)
            {
                Redraw();
            }
        }

        private void OnMaxZKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                Redraw();
            }
        }

        private void OnClickRefresh(object sender, EventArgs e) => Redraw();

        private void OnClickImport(object sender, EventArgs e) => ImportStatics();

        private void OnClickExport(object sender, EventArgs e)
        {
            if (Enum.TryParse((sender as ToolStripItem)?.Tag as string, out EcStaticsFormat format))
            {
                ExportStatics(format);
            }
        }

        private void OnClickClearStatics(object sender, EventArgs e) => ClearStatics();

        private void OnClickSaveImage(object sender, EventArgs e) => SaveImage();

        private void OnClickSave(object sender, EventArgs e) => _save?.Invoke();

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point? p = previewPanel.ClientToImage(e.Location);
            if (p == null || _render == null)
            {
                return;
            }
            if (_painting && e.Button == MouseButtons.Left)
            {
                PaintAt(EcMapRenderer.TileAt(_render, p.Value, _facet));
                return;
            }
            EcPlacedStatic hit = EcMapRenderer.HitTest(_render, p.Value);
            Point tile = hit != null ? new Point(hit.X, hit.Y) : EcMapRenderer.TileAt(_render, p.Value, _facet);
            toolStripStatusLabel.Text = hit != null
                ? $"{hit.X}, {hit.Y}: 0x{hit.Static.ItemId:X4} {hit.Sprite.Name}  z {hit.Static.Z}  hue {hit.Static.Hue}  ({EcItemSprites.KindText(hit.Sprite)})"
                : $"{tile.X}, {tile.Y}";
        }

        private void OnPreviewMouseClick(object sender, MouseEventArgs e)
        {
            Point? p = previewPanel.ClientToImage(e.Location);
            if (p == null || _render == null)
            {
                return;
            }
            if (e.Button == MouseButtons.Right && CurrentTool != Tool.Select)
            {
                toolStripComboBoxTool.SelectedIndex = (int)Tool.Select;
                return;
            }
            if (e.Button == MouseButtons.Left && CurrentTool == Tool.PlaceItem)
            {
                PlaceItem(EcMapRenderer.TileAt(_render, p.Value, _facet));
                return;
            }
            if (e.Button == MouseButtons.Left && CurrentTool == Tool.PlaceMulti)
            {
                PlaceMulti(EcMapRenderer.TileAt(_render, p.Value, _facet));
                return;
            }
            if (CurrentTool == Tool.PaintLand)
            {
                return;
            }
            EcPlacedStatic hit = EcMapRenderer.HitTest(_render, p.Value);
            _selected = hit;
            _tile = hit != null ? new Point(hit.X, hit.Y) : EcMapRenderer.TileAt(_render, p.Value, _facet);
            ShowTile();
        }

        private void OnPreviewMouseDown(object sender, MouseEventArgs e)
        {
            previewPanel.Focus();
            Point? p = previewPanel.ClientToImage(e.Location);
            if (p == null || _render == null || e.Button != MouseButtons.Left)
            {
                return;
            }
            if (CurrentTool == Tool.PaintLand)
            {
                _painting = true;
                _painted = Rectangle.Empty;
                PaintAt(EcMapRenderer.TileAt(_render, p.Value, _facet));
                return;
            }
            if (CurrentTool == Tool.Select)
            {
                EcPlacedStatic hit = EcMapRenderer.HitTest(_render, p.Value);
                _drag = hit != null && _selected != null && ReferenceEquals(hit.Static, _selected.Static) ? hit : null;
                _dragTile = hit != null ? new Point(hit.X, hit.Y) : Point.Empty;
            }
        }

        private void OnPreviewMouseUp(object sender, MouseEventArgs e)
        {
            if (_painting)
            {
                EndPaint();
                return;
            }
            Point? p = previewPanel.ClientToImage(e.Location);
            EcPlacedStatic drag = _drag;
            _drag = null;
            if (drag == null || p == null || _render == null)
            {
                return;
            }
            Point target = EcMapRenderer.TileAt(_render, p.Value, _facet);
            if (target != _dragTile && target.X >= 0 && _facet.MoveStatic(_dragTile.X, _dragTile.Y, drag.Static, target.X, target.Y))
            {
                _tile = target;
                _selected = new EcPlacedStatic { X = target.X, Y = target.Y, Static = drag.Static };
                Changed();
                toolStripStatusLabel.Text = $"0x{drag.Static.ItemId:X4} moved to {target.X}, {target.Y}.";
            }
        }

        /// <summary>Keys for the selected static (arrow keys never reach KeyDown, so they are handled here).</summary>
        private bool HandleStaticKey(Keys keyData)
        {
            if (toolStrip.ContainsFocus || toolStripTools.ContainsFocus || listViewTileStatics.Focused || SelectedStatic == null)
            {
                return false;
            }
            Keys key = keyData & Keys.KeyCode;
            int step = (keyData & Keys.Shift) != 0 ? 5 : 1;
            switch (key)
            {
                case Keys.Delete:
                    RemoveStatic();
                    return true;
                case Keys.Left:
                    MoveSelected(-1, 0, 0);
                    return true;
                case Keys.Right:
                    MoveSelected(1, 0, 0);
                    return true;
                case Keys.Up:
                    MoveSelected(0, -1, 0);
                    return true;
                case Keys.Down:
                    MoveSelected(0, 1, 0);
                    return true;
                case Keys.PageUp:
                    MoveSelected(0, 0, step);
                    return true;
                case Keys.PageDown:
                    MoveSelected(0, 0, -step);
                    return true;
                default:
                    return false;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return HandleStaticKey(keyData) || base.ProcessCmdKey(ref msg, keyData);
        }

        private void OnFormKeyDown(object sender, KeyEventArgs e)
        {
            // most keys are handled in ProcessCmdKey; this catches the rest (e.g. when the preview has the focus)
            if (!e.Handled && HandleStaticKey(e.KeyData))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void OnToolChanged(object sender, EventArgs e)
        {
            _painting = false;
            _drag = null;
            previewPanel.Cursor = CurrentTool == Tool.Select ? Cursors.Default : Cursors.Cross;
            toolStripLabelToolHint.Text = CurrentTool switch
            {
                Tool.PlaceItem => "Click = place the item (z field or land z), right click = stop",
                Tool.PaintLand => "Hold the mouse button and paint; z field empty = keep the z",
                Tool.PlaceMulti => "Click = place the multi (its center on the tile), right click = stop",
                _ => "Drag = move, arrows = move, PgUp / PgDn = z (Shift: 5), Del = remove"
            };
        }

        private void OnClickPickItem(object sender, EventArgs e) => PickItem();

        private void OnTileStaticSelected(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }
            EcStatic st = listViewTileStatics.SelectedItems.Count == 1 ? listViewTileStatics.SelectedItems[0].Tag as EcStatic : null;
            _selected = st != null ? _render?.Statics.FirstOrDefault(s => ReferenceEquals(s.Static, st)) : null;
            ShowStaticImage();
            previewPanel.Invalidate();
        }

        private void OnClickAddStatic(object sender, EventArgs e) => AddStatic();

        private void OnClickEditStatic(object sender, EventArgs e) => EditStatic();

        private void OnClickRemoveStatic(object sender, EventArgs e) => RemoveStatic();

        private void OnClickCopyId(object sender, EventArgs e)
        {
            EcStatic st = SelectedStatic;
            if (st != null)
            {
                Clipboard.SetText($"0x{st.ItemId:X4}");
            }
        }

        private void OnClickCopyPosition(object sender, EventArgs e)
        {
            if (_tile.X >= 0)
            {
                Clipboard.SetText($"{_tile.X},{_tile.Y},{SelectedStatic?.Z ?? _facet.GetTile(_tile.X, _tile.Y)?.Z ?? 0}");
            }
        }
    }
}
