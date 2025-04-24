using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Helpers
{
    public static class GrayPixelAnalyzer
    {
        public static void AnalyzeAndSave(Bitmap image, string filePath, string headerText = "")
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            string content = AnalyzeToText(image, headerText);
            File.WriteAllText(filePath, content);
        }

        public static string AnalyzeToText(Bitmap image, string headerText = "")
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(headerText))
                sb.AppendLine(headerText);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);

                    if (pixel.R == pixel.G && pixel.G == pixel.B && pixel.R > 0 && pixel.R < 255)
                    {
                        string hex = $"#{pixel.R:X2}{pixel.G:X2}{pixel.B:X2}";
                        sb.AppendLine($"({x},{y}) - {hex}");
                    }
                }
            }

            return sb.ToString();
        }

        public static void ShowInForm(Bitmap image, string headerText = "")
        {
            string result = AnalyzeToText(image, headerText);

            Form form = new Form
            {
                Text = "Grayscale analysis",
                Width = 500,
                Height = 600
            };

            RichTextBox richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                WordWrap = false,
                Text = result
            };

            form.Controls.Add(richTextBox);
            form.Show();
        }
    }
}
