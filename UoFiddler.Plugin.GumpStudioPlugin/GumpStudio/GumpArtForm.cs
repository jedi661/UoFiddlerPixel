using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>
    /// Own gump graphics: a picture becomes a gump (AddImage), or a picture of a box is cut into the 9 parts of an
    /// AddBackground frame. Both are written into the gump file in memory (save it in UoFiddler's Gumps tab).
    /// </summary>
    public partial class GumpArtForm : Form
    {
        private Bitmap _picture;
        private Bitmap _frame;

        public GumpArtForm()
        {
            InitializeComponent();
            numPictureId.Value = FindFree(0xF000, 1);
            numFrameId.Value = FindFree(0xF100, 9);
        }

        /// <summary>The first gump ID written.</summary>
        public int InsertedId { get; private set; } = -1;

        /// <summary>True = 9 part frame (use AddBackground), false = one picture (AddImage).</summary>
        public bool InsertedFrame { get; private set; }

        private static int Count => Math.Max(0, Gumps.GetCount());

        private static bool Free(int id) => id >= 0 && id < Count && !Gumps.IsValidIndex(id);

        /// <summary>First ID from start on with count free IDs in a row (0 = none found).</summary>
        private static int FindFree(int start, int count)
        {
            int max = Count;
            if (max == 0)
            {
                return 0;
            }
            for (int pass = 0; pass < 2; pass++)
            {
                for (int id = pass == 0 ? Math.Min(start, max - 1) : 1; id + count <= max; id++)
                {
                    if (Enumerable.Range(id, count).All(Free))
                    {
                        return id;
                    }
                }
            }
            return 0;
        }

        private static Bitmap LoadPicture(IWin32Window owner)
        {
            using (var d = new OpenFileDialog { Filter = "Pictures (*.png;*.bmp;*.gif;*.jpg;*.tif)|*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|All files|*.*" })
            {
                if (d.ShowDialog(owner) != DialogResult.OK)
                {
                    return null;
                }
                try
                {
                    using (var loaded = new Bitmap(d.FileName))
                    {
                        var bmp = new Bitmap(loaded.Width, loaded.Height, PixelFormat.Format32bppArgb);
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.DrawImage(loaded, 0, 0, loaded.Width, loaded.Height);
                        }
                        return bmp;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(owner, "The picture could not be read:\n" + ex.Message, "Gump Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }

        /// <summary>Copy for the gump file: pure black becomes transparent (color 0 is see-through in UO).</summary>
        private static Bitmap Prepare(Bitmap source, Rectangle part, bool blackTransparent)
        {
            var bmp = new Bitmap(Math.Max(1, part.Width), Math.Max(1, part.Height), PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(source, new Rectangle(0, 0, part.Width, part.Height), part, GraphicsUnit.Pixel);
            }
            if (blackTransparent)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        if (c.A > 0 && c.R < 8 && c.G < 8 && c.B < 8)
                        {
                            bmp.SetPixel(x, y, Color.Transparent);
                        }
                    }
                }
            }
            return bmp;
        }

        // ------------------------------------------------------------------ frame (9 parts)

        /// <summary>The 9 parts in AddBackground order: top-left, top, top-right, left, center, right, bottom-left, bottom, bottom-right.</summary>
        private Rectangle[] Slices()
        {
            if (_frame == null)
            {
                return Array.Empty<Rectangle>();
            }
            int w = _frame.Width, h = _frame.Height;
            int l = Math.Min((int)numLeft.Value, w - 2), t = Math.Min((int)numTop.Value, h - 2);
            int r = Math.Min((int)numRight.Value, w - l - 1), b = Math.Min((int)numBottom.Value, h - t - 1);
            int cw = Math.Max(1, w - l - r), ch = Math.Max(1, h - t - b);
            return new[]
            {
                new Rectangle(0, 0, l, t), new Rectangle(l, 0, cw, t), new Rectangle(w - r, 0, r, t),
                new Rectangle(0, t, l, ch), new Rectangle(l, t, cw, ch), new Rectangle(w - r, t, r, ch),
                new Rectangle(0, h - b, l, b), new Rectangle(l, h - b, cw, b), new Rectangle(w - r, h - b, r, b)
            };
        }

        /// <summary>Draws the frame like the client draws AddBackground: corners, tiled edges, tiled center.</summary>
        private Bitmap FramePreview(int width, int height)
        {
            Rectangle[] s = Slices();
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            if (s.Length != 9)
            {
                return bmp;
            }
            Bitmap[] parts = s.Select(r => Prepare(_frame, r, chkFrameBlack.Checked)).ToArray();
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(60, 60, 60));
                int l = parts[0].Width, t = parts[0].Height, r = parts[2].Width, b = parts[6].Height;
                void Tile(Bitmap p, Rectangle area)
                {
                    if (area.Width <= 0 || area.Height <= 0)
                    {
                        return;
                    }
                    using (var brush = new TextureBrush(p, WrapMode.Tile))
                    {
                        brush.TranslateTransform(area.X, area.Y);
                        g.FillRectangle(brush, area);
                    }
                }
                Tile(parts[4], new Rectangle(l, t, width - l - r, height - t - b));
                Tile(parts[1], new Rectangle(l, 0, width - l - r, t));
                Tile(parts[7], new Rectangle(l, height - b, width - l - r, b));
                Tile(parts[3], new Rectangle(0, t, l, height - t - b));
                Tile(parts[5], new Rectangle(width - r, t, r, height - t - b));
                g.DrawImageUnscaled(parts[0], 0, 0);
                g.DrawImageUnscaled(parts[2], width - r, 0);
                g.DrawImageUnscaled(parts[6], 0, height - b);
                g.DrawImageUnscaled(parts[8], width - r, height - b);
            }
            foreach (Bitmap p in parts)
            {
                p.Dispose();
            }
            return bmp;
        }

        private void PictureSource_Paint(object sender, PaintEventArgs e)
        {
            if (_frame == null)
            {
                return;
            }
            // picture box zoom: where the picture is drawn
            float scale = Math.Min(pictureSource.ClientSize.Width / (float)_frame.Width, pictureSource.ClientSize.Height / (float)_frame.Height);
            float ox = (pictureSource.ClientSize.Width - _frame.Width * scale) / 2, oy = (pictureSource.ClientSize.Height - _frame.Height * scale) / 2;
            using (var pen = new Pen(Color.Magenta) { DashStyle = DashStyle.Dash })
            {
                float l = ox + (float)numLeft.Value * scale, r = ox + (_frame.Width - (float)numRight.Value) * scale;
                float t = oy + (float)numTop.Value * scale, b = oy + (_frame.Height - (float)numBottom.Value) * scale;
                e.Graphics.DrawLine(pen, l, oy, l, oy + _frame.Height * scale);
                e.Graphics.DrawLine(pen, r, oy, r, oy + _frame.Height * scale);
                e.Graphics.DrawLine(pen, ox, t, ox + _frame.Width * scale, t);
                e.Graphics.DrawLine(pen, ox, b, ox + _frame.Width * scale, b);
            }
        }

        // ------------------------------------------------------------------ events

        private void Option_Changed(object sender, EventArgs e) => UpdatePreview();

        private void UpdatePreview()
        {
            if (tabMain.SelectedTab == tabPicture)
            {
                picturePicture.Image?.Dispose();
                picturePicture.Image = _picture == null ? null : Prepare(_picture, new Rectangle(Point.Empty, _picture.Size), chkBlackTransparent.Checked);
                int id = (int)numPictureId.Value;
                lblStatus.Text = _picture == null ? "Load a picture. / Bild laden."
                    : $"{_picture.Width} × {_picture.Height} → gump 0x{id:X} ({id})" + (id >= Count ? " – outside the gump file!" : Free(id) ? " – free" : " – used (will be replaced)");
            }
            else
            {
                pictureSource.Invalidate();
                pictureFramePreview.Image?.Dispose();
                pictureFramePreview.Image = _frame == null ? null : FramePreview((int)numPreviewWidth.Value, (int)numPreviewHeight.Value);
                int id = (int)numFrameId.Value;
                int used = Enumerable.Range(id, 9).Count(i => !Free(i));
                lblStatus.Text = _frame == null ? "Load a picture of a box / window. / Bild eines Kastens laden."
                    : $"9 parts → gumps 0x{id:X} … 0x{id + 8:X}" + (id + 8 >= Count ? " – outside the gump file!" : used == 0 ? " – all free" : $" – {used} used (will be replaced)");
            }
        }

        private void BtnLoadPicture_Click(object sender, EventArgs e)
        {
            Bitmap b = LoadPicture(this);
            if (b != null)
            {
                _picture?.Dispose();
                _picture = b;
                UpdatePreview();
            }
        }

        private void BtnLoadFrame_Click(object sender, EventArgs e)
        {
            Bitmap b = LoadPicture(this);
            if (b == null)
            {
                return;
            }
            _frame?.Dispose();
            _frame = b;
            pictureSource.Image = _frame;
            int m = Math.Max(4, Math.Min(_frame.Width, _frame.Height) / 4);
            numLeft.Value = numTop.Value = numRight.Value = numBottom.Value = Math.Min(numLeft.Maximum, m);
            UpdatePreview();
        }

        private void BtnFreePicture_Click(object sender, EventArgs e)
        {
            numPictureId.Value = FindFree((int)numPictureId.Value + 1, 1);
        }

        private void BtnFreeFrame_Click(object sender, EventArgs e)
        {
            numFrameId.Value = FindFree((int)numFrameId.Value + 1, 9);
        }

        private void BtnInsert_Click(object sender, EventArgs e)
        {
            bool frame = tabMain.SelectedTab == tabFrame;
            int id = (int)(frame ? numFrameId.Value : numPictureId.Value);
            int count = frame ? 9 : 1;
            if (frame ? _frame == null : _picture == null)
            {
                return;
            }
            if (id + count > Count)
            {
                MessageBox.Show(this, "These IDs are outside the gump file. / Diese IDs liegen außerhalb der Gump-Datei.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int used = Enumerable.Range(id, count).Count(i => !Free(i));
            if (used > 0 && MessageBox.Show(this, $"{used} of these gump IDs are used and will be replaced. Continue?\n\nDE: {used} dieser IDs sind belegt und werden ersetzt. Weiter?",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                Rectangle[] parts = frame ? Slices() : new[] { new Rectangle(Point.Empty, _picture.Size) };
                for (int i = 0; i < count; i++)
                {
                    Bitmap bmp = Prepare(frame ? _frame : _picture, parts[i], frame ? chkFrameBlack.Checked : chkBlackTransparent.Checked);
                    Gumps.ReplaceGump(id + i, bmp);
                    ControlEvents.FireGumpChangeEvent(this, id + i);
                }
                Options.ChangedUltimaClass["Gumps"] = true;
                GumpRenderer.ClearCache();
                InsertedId = id;
                InsertedFrame = frame;
                MessageBox.Show(this, $"Written to gump 0x{id:X}" + (frame ? $" … 0x{id + 8:X}" : string.Empty) + ".\nSave the gump file in UoFiddler (Gumps tab → Save) to keep it.\n\n"
                                      + "DE: In den Speicher geschrieben. Zum Behalten in UoFiddler die Gump-Datei speichern (Reiter Gumps → Speichern).",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Writing failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
