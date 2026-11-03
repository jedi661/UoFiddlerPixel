/***************************************************************************
 *
 * $Author: Nikodemus
 * Coder: Nikodemus
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UoFiddler.Forms.Notepad
{
    /// <summary>
    /// Prints a note with all formatting and images (RichEdit EM_FORMATRANGE), with header and page numbers.
    /// Also exports PDF: through "Microsoft Print to PDF" (selectable text) or a built-in image PDF.
    /// </summary>
    internal sealed class NotePrinter : IDisposable
    {
        public const string PdfPrinterName = "Microsoft Print to PDF";

        private readonly RichTextBox _box = new();
        private readonly string _title;
        private readonly bool _header;
        private int _from;
        private int _to;
        private int _next;
        private int _page;

        public PrintDocument Document { get; } = new();

        public NotePrinter(Note note, string rtf, bool header)
        {
            _title = note.Title;
            _header = header;
            _box.CreateControl();
            _ = _box.Handle;
            try
            {
                _box.Rtf = rtf;
            }
            catch (ArgumentException)
            {
                _box.Text = note.PlainText ?? "";
            }

            _from = 0;
            _to = _box.TextLength;
            Document.DocumentName = string.IsNullOrWhiteSpace(note.Title) ? "Note" : note.Title;
            Document.BeginPrint += (_, _) =>
            {
                _next = _from;
                _page = 0;
            };
            Document.PrintPage += PrintPage;
            Document.EndPrint += (_, _) => RichEditNative.FormatRangeDone(_box);
        }

        public void SetRange(int from, int length)
        {
            _from = Math.Clamp(from, 0, _box.TextLength);
            _to = Math.Clamp(from + length, _from, _box.TextLength);
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            Rectangle content = e.MarginBounds;
            _page++;
            DrawHeaderFooter(e.Graphics, e.MarginBounds, ref content, _page);

            int before = _next;
            _next = RichEditNative.FormatRange(_box, e.Graphics, content, e.PageBounds, _next, _to, false);
            e.HasMorePages = _next < _to && _next > before;
        }

        /// <summary>
        /// Header with title and date, footer with page number. Units: 1/100 inch.
        /// </summary>
        private void DrawHeaderFooter(Graphics g, Rectangle margin, ref Rectangle content, int page)
        {
            if (!_header)
            {
                return;
            }

            using var titleFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var smallFont = new Font("Segoe UI", 8f);
            using var pen = new Pen(Color.Silver, 1f);
            var right = new StringFormat { Alignment = StringAlignment.Far };
            var center = new StringFormat { Alignment = StringAlignment.Center };

            g.DrawString(_title, titleFont, Brushes.Black, margin.Left, margin.Top - 30);
            g.DrawString(DateTime.Now.ToString("g", CultureInfo.CurrentCulture), smallFont, Brushes.Gray,
                new RectangleF(margin.Left, margin.Top - 28, margin.Width, 20), right);
            g.DrawLine(pen, margin.Left, margin.Top - 10, margin.Right, margin.Top - 10);

            g.DrawLine(pen, margin.Left, margin.Bottom + 8, margin.Right, margin.Bottom + 8);
            g.DrawString($"Page {page}", smallFont, Brushes.Gray, new RectangleF(margin.Left, margin.Bottom + 12, margin.Width, 20), center);
        }

        #region PDF

        public static bool HasPdfPrinter()
        {
            try
            {
                return PrinterSettings.InstalledPrinters.Cast<string>().Any(p => string.Equals(p, PdfPrinterName, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// PDF with real (selectable) text through the Windows PDF printer.
        /// </summary>
        public void ExportPdfWithPrinter(string path, PageSettings pageSettings)
        {
            Document.PrinterSettings = new PrinterSettings
            {
                PrinterName = PdfPrinterName,
                PrintToFile = true,
                PrintFileName = path
            };

            if (pageSettings != null)
            {
                Document.DefaultPageSettings.Margins = pageSettings.Margins;
                Document.DefaultPageSettings.Landscape = pageSettings.Landscape;
                PaperSize paper = Document.PrinterSettings.PaperSizes.Cast<PaperSize>()
                    .FirstOrDefault(p => p.Kind == pageSettings.PaperSize.Kind);
                if (paper != null)
                {
                    Document.DefaultPageSettings.PaperSize = paper;
                }
            }

            Document.PrintController = new StandardPrintController();
            Document.Print();
        }

        /// <summary>
        /// Built-in PDF (pages as images), works without any printer. A4, margins in 1/100 inch.
        /// </summary>
        public void ExportPdfAsImages(string path, bool landscape, Margins margins)
        {
            // A4 in 1/100 inch
            int pageW = landscape ? 1169 : 827, pageH = landscape ? 827 : 1169;
            margins ??= new Margins(80, 80, 90, 90);
            var page = new Rectangle(0, 0, pageW, pageH);
            var marginRect = new Rectangle(margins.Left, margins.Top, pageW - margins.Left - margins.Right, pageH - margins.Top - margins.Bottom);
            const int dpi = 150;

            var pdf = new SimplePdfWriter();
            using Graphics refGraphics = Graphics.FromHwnd(IntPtr.Zero);
            int next = _from, pageNo = 0;

            do
            {
                pageNo++;
                int before = next;
                IntPtr refHdc = refGraphics.GetHdc();
                Metafile metafile;
                try
                {
                    metafile = new Metafile(refHdc, new RectangleF(0, 0, pageW / 100f, pageH / 100f), MetafileFrameUnit.Inch, EmfType.EmfPlusDual);
                }
                finally
                {
                    refGraphics.ReleaseHdc(refHdc);
                }

                using (metafile)
                {
                    using (Graphics g = Graphics.FromImage(metafile))
                    {
                        // work in 1/100 inch like a printer
                        g.PageUnit = GraphicsUnit.Inch;
                        g.PageScale = 0.01f;
                        Rectangle content = marginRect;
                        DrawHeaderFooter(g, marginRect, ref content, pageNo);
                        g.ResetTransform();
                        g.PageUnit = GraphicsUnit.Pixel;
                        g.PageScale = 1f;
                        next = RichEditNative.FormatRange(_box, g, content, page, next, _to, false);
                    }

                    int w = pageW * dpi / 100, h = pageH * dpi / 100;
                    using var bmp = new Bitmap(w, h);
                    bmp.SetResolution(dpi, dpi);
                    using (Graphics bg = Graphics.FromImage(bmp))
                    {
                        bg.Clear(Color.White);
                        bg.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        bg.SmoothingMode = SmoothingMode.HighQuality;
                        bg.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        bg.DrawImage(metafile, new Rectangle(0, 0, w, h));
                    }

                    pdf.AddJpegPage(bmp, pageW * 0.72f, pageH * 0.72f);
                }

                if (next <= before)
                {
                    break;
                }
            }
            while (next < _to && pageNo < 2000);

            RichEditNative.FormatRangeDone(_box);
            pdf.Save(path, _title);
        }

        #endregion

        public void Dispose()
        {
            Document.Dispose();
            _box.Dispose();
        }
    }

    /// <summary>
    /// Minimal PDF writer: one JPEG image per page.
    /// </summary>
    internal sealed class SimplePdfWriter
    {
        private readonly List<(byte[] Jpeg, int W, int H, float PtW, float PtH)> _pages = new();

        public void AddJpegPage(Bitmap bmp, float widthPt, float heightPt)
        {
            ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            using var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
            using var ms = new MemoryStream();
            bmp.Save(ms, codec, parameters);
            AddJpegPage(ms.ToArray(), bmp.Width, bmp.Height, widthPt, heightPt);
        }

        public void AddJpegPage(byte[] jpeg, int width, int height, float widthPt, float heightPt) =>
            _pages.Add((jpeg, width, height, widthPt, heightPt));

        public void Save(string path, string title)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            var offsets = new List<long>();
            Encoding ascii = Encoding.ASCII;

            void Write(string s)
            {
                byte[] b = ascii.GetBytes(s);
                fs.Write(b, 0, b.Length);
            }

            void BeginObject(int number)
            {
                while (offsets.Count < number)
                {
                    offsets.Add(0);
                }

                offsets[number - 1] = fs.Position;
                Write($"{number} 0 obj\n");
            }

            static string F(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);

            Write("%PDF-1.4\n%");
            fs.Write(new byte[] { 0xE2, 0xE3, 0xCF, 0xD3, (byte)'\n' }, 0, 5);

            int pageCount = _pages.Count;
            // object numbers: 1 catalog, 2 pages, 3 info, then per page: page, content, image
            int PageObj(int i) => 4 + i * 3;

            BeginObject(1);
            Write("<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

            BeginObject(2);
            Write("<< /Type /Pages /Kids [" + string.Join(" ", Enumerable.Range(0, pageCount).Select(i => $"{PageObj(i)} 0 R")) +
                  $"] /Count {pageCount} >>\nendobj\n");

            BeginObject(3);
            string safeTitle = new string((title ?? "Note").Where(c => c >= 32 && c < 127 && c != '(' && c != ')' && c != '\\').ToArray());
            Write($"<< /Title ({safeTitle}) /Producer (UoFiddler Notepad) >>\nendobj\n");

            for (int i = 0; i < pageCount; i++)
            {
                var p = _pages[i];
                int pageObj = PageObj(i), contentObj = pageObj + 1, imageObj = pageObj + 2;

                BeginObject(pageObj);
                Write($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {F(p.PtW)} {F(p.PtH)}] " +
                      $"/Resources << /XObject << /Im{i} {imageObj} 0 R >> >> /Contents {contentObj} 0 R >>\nendobj\n");

                string content = $"q {F(p.PtW)} 0 0 {F(p.PtH)} 0 0 cm /Im{i} Do Q\n";
                BeginObject(contentObj);
                Write($"<< /Length {content.Length} >>\nstream\n{content}endstream\nendobj\n");

                BeginObject(imageObj);
                Write($"<< /Type /XObject /Subtype /Image /Width {p.W} /Height {p.H} /ColorSpace /DeviceRGB " +
                      $"/BitsPerComponent 8 /Filter /DCTDecode /Length {p.Jpeg.Length} >>\nstream\n");
                fs.Write(p.Jpeg, 0, p.Jpeg.Length);
                Write("\nendstream\nendobj\n");
            }

            long xref = fs.Position;
            Write($"xref\n0 {offsets.Count + 1}\n0000000000 65535 f \n");
            foreach (long o in offsets)
            {
                Write($"{o:D10} 00000 n \n");
            }

            Write($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R /Info 3 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        }
    }
}
