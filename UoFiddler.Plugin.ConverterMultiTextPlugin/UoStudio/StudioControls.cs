/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.UoStudio
{
    /// <summary>
    /// Picture view with checker board background, integer zoom (nearest neighbour, pixel exact)
    /// and optional dark background (for additive effects).
    /// </summary>
    public class PixelView : Panel
    {
        private Image _image;
        private int _zoom = 2;
        private bool _darkBackground;

        public PixelView()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            AutoScroll = true;
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = Color.White;
        }

        /// <summary>The displayed image (not disposed by the view).</summary>
        public Image Image
        {
            get => _image;
            set
            {
                _image = value;
                UpdateScrollSize();
                Invalidate();
            }
        }

        public int Zoom
        {
            get => _zoom;
            set
            {
                _zoom = Math.Max(1, Math.Min(16, value));
                UpdateScrollSize();
                Invalidate();
            }
        }

        public bool DarkBackground
        {
            get => _darkBackground;
            set
            {
                _darkBackground = value;
                Invalidate();
            }
        }

        /// <summary>Optional text drawn into the upper left corner.</summary>
        public string Caption { get; set; }

        /// <summary>Optional extra painting on top of the image (in image coordinates * zoom).</summary>
        public event Action<Graphics, Rectangle> PaintOverlay;

        private void UpdateScrollSize()
        {
            AutoScrollMinSize = _image == null ? Size.Empty : new Size(_image.Width * _zoom, _image.Height * _zoom);
        }

        public Rectangle ImageRectangle
        {
            get
            {
                if (_image == null)
                {
                    return Rectangle.Empty;
                }
                int w = _image.Width * _zoom, h = _image.Height * _zoom;
                int x = Math.Max(0, (ClientSize.Width - w) / 2) + AutoScrollPosition.X;
                int y = Math.Max(0, (ClientSize.Height - h) / 2) + AutoScrollPosition.Y;
                return new Rectangle(x, y, w, h);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            if (_darkBackground)
            {
                g.Clear(Color.FromArgb(20, 20, 24));
            }
            else
            {
                StudioImaging.DrawChecker(g, ClientRectangle, 10);
            }

            if (_image != null)
            {
                Rectangle r = ImageRectangle;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(_image, r);
                PaintOverlay?.Invoke(g, r);
            }

            if (!string.IsNullOrEmpty(Caption))
            {
                using var bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0));
                SizeF s = g.MeasureString(Caption, Font);
                g.FillRectangle(bg, 2, 2, s.Width + 6, s.Height + 2);
                g.DrawString(Caption, Font, Brushes.White, 5, 3);
            }
        }
    }

    /// <summary>Small helpers used by all UO Studio forms.</summary>
    public static class StudioUi
    {
        public const string ImageFilter = "Bilder|*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|Alle Dateien|*.*";

        public static Label Label(string text, int x, int y, int w = 0)
        {
            var l = new Label { Text = text, Left = x, Top = y + 3, AutoSize = w == 0 };
            if (w > 0)
            {
                l.Width = w;
            }
            return l;
        }

        public static NumericUpDown Number(int x, int y, int w, decimal min, decimal max, decimal value, int decimals = 0)
        {
            return new NumericUpDown
            {
                Left = x,
                Top = y,
                Width = w,
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimals,
                Value = Math.Max(min, Math.Min(max, value)),
                Increment = decimals > 0 ? 0.1m : 1
            };
        }

        public static Button Button(string text, int x, int y, int w, EventHandler click)
        {
            var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = 26 };
            b.Click += click;
            return b;
        }

        public static CheckBox Check(string text, int x, int y, bool value)
        {
            return new CheckBox { Text = text, Left = x, Top = y, AutoSize = true, Checked = value };
        }

        public static ComboBox Combo(int x, int y, int w, params object[] items)
        {
            var c = new ComboBox { Left = x, Top = y, Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            c.Items.AddRange(items);
            if (c.Items.Count > 0)
            {
                c.SelectedIndex = 0;
            }
            return c;
        }

        /// <summary>Output directory of UO Fiddler (falls back to the desktop).</summary>
        public static string OutputPath
        {
            get
            {
                string path = Options.OutputPath;
                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                }
                return path;
            }
        }

        /// <summary>Marks a Ultima class as changed so UO Fiddler asks to save on exit.</summary>
        public static void MarkChanged(string ultimaClass)
        {
            if (Options.ChangedUltimaClass.ContainsKey(ultimaClass))
            {
                Options.ChangedUltimaClass[ultimaClass] = true;
            }
        }

        /// <summary>Resets the changed flag after the files were written.</summary>
        public static void ResetChanged(string ultimaClass)
        {
            if (Options.ChangedUltimaClass.ContainsKey(ultimaClass))
            {
                Options.ChangedUltimaClass[ultimaClass] = false;
            }
        }

        public static void Info(IWin32Window owner, string text)
        {
            MessageBox.Show(owner, text, "UO Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Error(IWin32Window owner, string text)
        {
            MessageBox.Show(owner, text, "UO Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Ask(IWin32Window owner, string text)
        {
            return MessageBox.Show(owner, text, "UO Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>Parses decimal or hex ("0x1F") numbers.</summary>
        public static bool TryParseId(string text, out int value)
        {
            text = (text ?? string.Empty).Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out value);
            }
            return int.TryParse(text, out value);
        }

        /// <summary>Returns the hue (0 = no hue, 1.. = hues.mul index + 1) or null.</summary>
        public static Ultima.Hue GetHue(int hueId)
        {
            if (hueId <= 0)
            {
                return null;
            }
            try
            {
                var list = Ultima.Hues.List;
                if (list == null || hueId - 1 >= list.Length)
                {
                    return null;
                }
                return list[hueId - 1];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Renders the 32 colours of a hue as a small strip.</summary>
        public static Bitmap HueStrip(Ultima.Hue hue, int width, int height)
        {
            var bmp = new Bitmap(Math.Max(1, width), Math.Max(1, height));
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(SystemColors.Control);
            if (hue == null)
            {
                g.DrawString("kein Hue", SystemFonts.DefaultFont, Brushes.Gray, 2, 1);
                return bmp;
            }
            float w = width / 32f;
            for (int i = 0; i < 32; i++)
            {
                using var b = new SolidBrush(hue.GetColor(i));
                g.FillRectangle(b, i * w, 0, w + 1, height);
            }
            return bmp;
        }
    }
}
