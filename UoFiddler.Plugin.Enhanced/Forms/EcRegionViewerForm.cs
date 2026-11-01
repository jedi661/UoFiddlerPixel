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
            EcPlacedStatic hit = EcMapRenderer.HitTest(_render, p.Value);
            _selected = hit;
            _tile = hit != null ? new Point(hit.X, hit.Y) : EcMapRenderer.TileAt(_render, p.Value, _facet);
            ShowTile();
        }

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
