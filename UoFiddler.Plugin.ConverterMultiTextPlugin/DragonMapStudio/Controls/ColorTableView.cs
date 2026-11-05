// /***************************************************************************
//  *
//  * Dragon Map Studio – clickable colour table image (IMod13_Colortable.bmp)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    /// <summary>
    /// Shows the colour table picture. Every pixel knows its palette index (read directly from the 8-bit BMP,
    /// or matched by colour for other images), so a click returns the "colour address".
    /// Put it into a panel with AutoScroll – the control sizes itself to image × zoom.
    /// </summary>
    [ToolboxItem(true)]
    public class ColorTableView : Control
    {
        private Bitmap _image;
        private Bitmap _highlight;
        private int[] _indices;
        private int _imgW;
        private int _imgH;
        private int _zoom = 2;
        private int _selected = -1;
        private int _hoverIndex = -1;
        private readonly Timer _blink = new Timer { Interval = 450 };
        private bool _blinkOn = true;

        public ColorTableView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            BackColor = Color.FromArgb(40, 40, 40);
            Cursor = Cursors.Cross;
            _blink.Tick += (s, e) =>
            {
                _blinkOn = !_blinkOn;
                Invalidate();
            };
        }

        public event EventHandler<PaletteIndexEventArgs> IndexClicked;
        public event EventHandler<PaletteIndexEventArgs> IndexHovered;

        [Category("Dragon"), DefaultValue(2)]
        public int Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Clamp(value, 1, 16);
                UpdateSize();
                Invalidate();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selected;
            set
            {
                _selected = value;
                BuildHighlight();
                Invalidate();
            }
        }

        [Browsable(false)]
        public bool HasImage => _image != null;

        /// <summary>Loads the colour table picture. For non 8-bit pictures the colours are matched to the rule set palette.</summary>
        public void LoadImage(string path, DragonRuleSet rules)
        {
            byte[] data = System.IO.File.ReadAllBytes(path);
            Bitmap shown;
            byte[] indices;
            int[] cells;
            int w, h;
            if (IndexedBitmapIO.TryReadBmp8(data, out w, out h, out indices, out Color[] pal))
            {
                cells = Array.ConvertAll(indices, b => (int)b);
                var map = new IndexedMap(w, h, Array.ConvertAll(indices, b => (ushort)b));
                shown = IndexedBitmapIO.ToBitmap(map, pal);
            }
            else
            {
                using var src = new Bitmap(new System.IO.MemoryStream(data));
                w = src.Width;
                h = src.Height;
                IndexedMap map = IndexedBitmapIO.FromBitmap(src, rules, null);
                cells = Array.ConvertAll(map.Pixels, b => (int)b);
                shown = new Bitmap(src);
            }
            SetImage(shown, cells, w, h);
        }

        /// <summary>Uses an image that was generated in memory (palette index per pixel, -1 = no colour).</summary>
        public void SetImage(Bitmap image, int[] indices, int width, int height)
        {
            _image?.Dispose();
            _image = image;
            _indices = indices;
            _imgW = width;
            _imgH = height;
            BuildHighlight();
            UpdateSize();
            Invalidate();
        }

        private void UpdateSize()
        {
            if (_image != null)
            {
                Size = new Size(_imgW * _zoom, _imgH * _zoom);
            }
        }

        /// <summary>Overlay that marks every pixel of the selected index.</summary>
        private void BuildHighlight()
        {
            _highlight?.Dispose();
            _highlight = null;
            if (_indices == null || _selected < 0)
            {
                _blink.Stop();
                return;
            }
            var bmp = new Bitmap(_imgW, _imgH, PixelFormat.Format32bppArgb);
            var row = new int[_imgW];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, _imgW, _imgH), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < _imgH; y++)
                {
                    for (int x = 0; x < _imgW; x++)
                    {
                        bool sel = _indices[y * _imgW + x] == _selected;
                        // border pixels of the selected area get a strong colour, the inside a light veil
                        bool border = sel && (x == 0 || y == 0 || x == _imgW - 1 || y == _imgH - 1 ||
                                              _indices[y * _imgW + x - 1] != _selected || _indices[y * _imgW + x + 1] != _selected ||
                                              _indices[(y - 1) * _imgW + x] != _selected || _indices[(y + 1) * _imgW + x] != _selected);
                        row[x] = border ? unchecked((int)0xFFFF00FF) : sel ? 0x50FFFFFF : 0;
                    }
                    Marshal.Copy(row, 0, bd.Scan0 + y * bd.Stride, _imgW);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            _highlight = bmp;
            _blink.Start();
        }

        public int IndexAt(Point p)
        {
            if (_indices == null)
            {
                return -1;
            }
            int x = p.X / _zoom, y = p.Y / _zoom;
            return x < 0 || y < 0 || x >= _imgW || y >= _imgH ? -1 : _indices[y * _imgW + x];
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_image == null)
            {
                TextRenderer.DrawText(g, "Keine Farbtabelle geladen.", Font, new Point(10, 10), Color.White);
                return;
            }
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            var dest = new Rectangle(0, 0, _imgW * _zoom, _imgH * _zoom);
            g.DrawImage(_image, dest);
            if (_highlight != null && _blinkOn)
            {
                g.DrawImage(_highlight, dest);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = IndexAt(e.Location);
            if (idx != _hoverIndex)
            {
                _hoverIndex = idx;
                IndexHovered?.Invoke(this, new PaletteIndexEventArgs(idx, MouseButtons.None));
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int idx = IndexAt(e.Location);
            if (idx >= 0)
            {
                IndexClicked?.Invoke(this, new PaletteIndexEventArgs(idx, e.Button));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _blink.Dispose();
                _image?.Dispose();
                _highlight?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
