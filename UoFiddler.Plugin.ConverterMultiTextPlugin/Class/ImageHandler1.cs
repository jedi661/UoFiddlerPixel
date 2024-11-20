using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    internal class ImageHandler1
    {
        public ImageHandler1(string imagePath = "", int tileWidth = 44, int tileHeight = 44, int offset = 1, string outputDirectory = "out")
        {
            ImagePath = imagePath;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            Offset = offset;
            OutputDirectory = outputDirectory;
        }

        public string ImagePath { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Offset { get; set; }
        public string OutputDirectory { get; set; }
        public string FileNameFormat { get; set; } = "{0}";
        public int StartingFileNumber { get; set; } = 0;
        public string LastErrorMessage { get; private set; } = string.Empty;

        private Bitmap OriginalImage;
        private List<Bitmap> Slices = new List<Bitmap>();

        public bool Process()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ImagePath) || !File.Exists(ImagePath))
                {
                    LastErrorMessage = "Image file not found.";
                    return false;
                }

                OriginalImage = new Bitmap(ImagePath);

                int xSlices = (int)Math.Ceiling((double)OriginalImage.Width / TileWidth);
                int ySlices = (int)Math.Ceiling((double)OriginalImage.Height / TileHeight);

                SplitImage(xSlices, ySlices);
                SaveImages();
                CreateHtmlLayout(xSlices, ySlices);

                return true;
            }
            catch (Exception ex)
            {
                LastErrorMessage = $"An error occurred during processing: {ex.Message}";
                return false;
            }
        }

        private void SplitImage(int xSlices, int ySlices)
        {
            for (int row = 0; row < ySlices; row++)
            {
                for (int col = 0; col < xSlices; col++)
                {
                    int startX = col * TileWidth;
                    int startY = row * TileHeight;

                    Rectangle cropRect = new Rectangle(startX, startY, TileWidth, TileHeight);
                    Bitmap slice = new Bitmap(TileWidth, TileHeight);

                    using (Graphics g = Graphics.FromImage(slice))
                    {
                        g.DrawImage(OriginalImage, new Rectangle(0, 0, TileWidth, TileHeight), cropRect, GraphicsUnit.Pixel);
                    }

                    Slices.Add(slice);
                }
            }
        }

        private void SaveImages()
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            int fileNumber = StartingFileNumber;

            foreach (Bitmap slice in Slices)
            {
                string filePath = Path.Combine(OutputDirectory, string.Format(FileNameFormat + ".bmp", fileNumber));
                slice.Save(filePath, ImageFormat.Bmp);
                fileNumber++;
            }
        }

        private void CreateHtmlLayout(int xSlices, int ySlices)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html><head><style>body { margin: 0; padding: 0; }</style></head><body>");

            int fileNumber = StartingFileNumber;

            for (int row = 0; row < ySlices; row++)
            {
                for (int col = 0; col < xSlices; col++)
                {
                    string imageName = string.Format(FileNameFormat + ".bmp", fileNumber);
                    htmlBuilder.AppendLine($"<img src='{imageName}' style='position:absolute; top:{row * TileHeight}px; left:{col * TileWidth}px;'>");
                    fileNumber++;
                }
            }

            htmlBuilder.AppendLine("</body></html>");

            string htmlFilePath = Path.Combine(OutputDirectory, "layout.html");
            File.WriteAllText(htmlFilePath, htmlBuilder.ToString());
        }
    }
}
