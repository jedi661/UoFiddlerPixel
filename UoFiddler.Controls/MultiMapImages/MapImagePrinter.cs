/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace UoFiddler.Controls.MultiMapImages
{
    /// <summary>
    /// Prints an image: fitted on one page or as poster on several pages.
    /// </summary>
    public static class MapImagePrinter
    {
        /// <summary>Pages per side for poster printing (1 = fit on one page)</summary>
        public static int PosterPages { get; set; } = 1;

        /// <summary>Print the title above the image</summary>
        public static bool PrintTitle { get; set; } = true;

        /// <summary>Opens the print dialog and prints</summary>
        public static void Print(IWin32Window owner, Image image, string title)
        {
            if (image == null)
            {
                return;
            }

            if (!AskOptions(owner))
            {
                return;
            }

            using (PrintDocument doc = CreateDocument(image, title))
            using (var dialog = new PrintDialog { Document = doc, UseEXDialog = true, AllowSomePages = false })
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    doc.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(owner, $"Printing failed:\n{ex.Message}", "Print", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Shows the print preview (with print button)</summary>
        public static void Preview(IWin32Window owner, Image image, string title)
        {
            if (image == null)
            {
                return;
            }

            if (!AskOptions(owner))
            {
                return;
            }

            using (PrintDocument doc = CreateDocument(image, title))
            using (var dialog = new PrintPreviewDialog { Document = doc, Width = 1000, Height = 760, UseAntiAlias = true })
            {
                dialog.ShowIcon = false;
                dialog.ShowDialog(owner);
            }
        }

        /// <summary>Small dialog: fit / poster, title</summary>
        private static bool AskOptions(IWin32Window owner)
        {
            using (var form = new Form())
            {
                form.Text = "Print options";
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.StartPosition = FormStartPosition.CenterParent;
                form.ClientSize = new Size(300, 150);
                form.ShowInTaskbar = false;

                var label = new Label { Text = "Pages per side (1 = fit on one page,\n2 = poster with 2 x 2 pages, ...):", Left = 12, Top = 12, Width = 280, Height = 34 };
                var pages = new NumericUpDown { Left = 12, Top = 50, Width = 70, Minimum = 1, Maximum = 6, Value = PosterPages };
                var title = new CheckBox { Text = "Print the name above the image", Left = 12, Top = 82, Width = 280, Checked = PrintTitle };
                var ok = new Button { Text = "OK", Left = 128, Top = 114, Width = 75, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 213, Top = 114, Width = 75, DialogResult = DialogResult.Cancel };
                form.Controls.AddRange(new Control[] { label, pages, title, ok, cancel });
                form.AcceptButton = ok;
                form.CancelButton = cancel;
                if (form.ShowDialog(owner) != DialogResult.OK)
                {
                    return false;
                }

                PosterPages = (int)pages.Value;
                PrintTitle = title.Checked;
                return true;
            }
        }

        private static PrintDocument CreateDocument(Image image, string title)
        {
            var doc = new PrintDocument { DocumentName = string.IsNullOrWhiteSpace(title) ? "Map image" : title };
            doc.DefaultPageSettings.Landscape = image.Width > image.Height;
            int n = Math.Max(1, PosterPages);
            int page = 0;

            doc.BeginPrint += (s, e) => page = 0;
            doc.PrintPage += (s, e) =>
            {
                Rectangle bounds = e.MarginBounds;
                Graphics g = e.Graphics;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                if (PrintTitle && !string.IsNullOrWhiteSpace(title))
                {
                    using (var font = new Font("Georgia", 12f, FontStyle.Bold))
                    {
                        string t = n > 1 ? $"{title}   ({page % n + 1}/{page / n + 1})" : title;
                        SizeF ts = g.MeasureString(t, font);
                        g.DrawString(t, font, Brushes.Black, bounds.Left + (bounds.Width - ts.Width) / 2f, bounds.Top);
                        int th = (int)ts.Height + 6;
                        bounds = new Rectangle(bounds.Left, bounds.Top + th, bounds.Width, bounds.Height - th);
                    }
                }

                // part of the image for this page
                int col = page % n, row = page / n;
                float partW = image.Width / (float)n, partH = image.Height / (float)n;
                var src = new RectangleF(col * partW, row * partH, partW, partH);

                float scale = Math.Min(bounds.Width / src.Width, bounds.Height / src.Height);
                float w = src.Width * scale, h = src.Height * scale;
                var dst = new RectangleF(bounds.Left + (bounds.Width - w) / 2f, bounds.Top + (bounds.Height - h) / 2f, w, h);
                g.DrawImage(image, dst, src, GraphicsUnit.Pixel);
                if (n > 1)
                {
                    // thin cut border
                    using (var pen = new Pen(Color.LightGray) { DashStyle = DashStyle.Dash })
                    {
                        g.DrawRectangle(pen, dst.X, dst.Y, dst.Width, dst.Height);
                    }
                }

                page++;
                e.HasMorePages = page < n * n;
            };
            return doc;
        }
    }
}
