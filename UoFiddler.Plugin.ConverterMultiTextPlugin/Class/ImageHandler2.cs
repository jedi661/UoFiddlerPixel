// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    internal class ImageHandler2
    {
        public ImageHandler2(string imagePath = "", int tileWidth = 44, int tileHeight = 44, int offset = 1, string outputDirectory = "out")
        {
            ImagePath = imagePath;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            Offset = offset;
            OutputDirectory = outputDirectory;
        }

        public Color BackgroundColor { get; set; } = Color.Black;
        public string ImagePath { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Offset { get; set; }
        public string OutputDirectory { get; set; }
        public Bitmap OriginalImage { get; private set; }
        public List<Bitmap> Slices { get; private set; } = new List<Bitmap>();
        public string LastErrorMessage { get; private set; } = string.Empty;
        public string FileNameFormat { get; set; } = "{0}";
        public int StartingFileNumber { get; set; } = 0;

        private int xSlices = 0, ySlices = 0;

        public bool Process()
        {
            if (File.Exists(ImagePath))
            {
                OriginalImage = new Bitmap(ImagePath);

                xSlices = (int)Math.Ceiling((double)OriginalImage.Width / TileWidth) + 1;
                ySlices = 2 * (int)Math.Ceiling((double)OriginalImage.Height / TileHeight) + 1;

                SplitImage();
                SaveImages();
                CreateHtmlLayout();
                Console.WriteLine("Sliced successfully.");
                return true;
            }
            else
            {
                LastErrorMessage = "Image path could not be found";
                Console.WriteLine(LastErrorMessage);
            }
            return false;
        }

        private void SplitImage()
        {
            int xOffset = -(TileWidth / 2), yOffset = -(TileHeight / 2);

            for (int row = 0; row < ySlices; row++)
            {
                for (int col = 0; col < xSlices; col++)
                {
                    var start = GetSectionStartPosition(row, col);

                    Bitmap slice = new Bitmap(TileWidth, TileHeight);
                    using (Graphics g = Graphics.FromImage(slice))
                    {
                        g.Clear(BackgroundColor);
                        int offset = Offset;
                        bool reverse = false;

                        for (int yPixel = 0; yPixel < TileHeight; yPixel++)
                        {
                            int grabX = (TileWidth / 2) - offset;
                            int grabQty = offset * 2;

                            for (int i = 0; i < grabQty; i++)
                            {
                                if (grabX + i + start.X < 0 || grabX + i + start.X >= OriginalImage.Width || yPixel + start.Y < 0 || yPixel + start.Y >= OriginalImage.Height)
                                {
                                    continue;
                                }
                                Color pixelColor = OriginalImage.GetPixel(grabX + i + start.X, yPixel + start.Y);
                                slice.SetPixel(grabX + i, yPixel, pixelColor);
                            }

                            if (!reverse)
                            {
                                offset++;
                                if ((TileWidth / 2) - offset < 0)
                                {
                                    offset--;
                                    reverse = true;
                                }
                            }
                            else
                            {
                                offset--;
                            }
                        }
                    }
                    Slices.Add(slice);
                }
            }
        }

        private Point GetSectionStartPosition(int row, int column)
        {
            int x = column * TileWidth;
            int y = (row * (TileHeight / 2)) - (TileHeight / 2);

            if (row % 2 == 0)
            {
                x -= (TileWidth / 2);
            }

            return new Point(x, y);
        }

        private Bitmap GenSampleGrid()
        {
            Bitmap grid = new Bitmap(TileWidth, TileHeight);
            Color gColor = Color.FromArgb(100, 0, 255, 0);

            int x = 0, y = grid.Height / 2;

            while (x < grid.Width && y < grid.Height)
            {
                grid.SetPixel(x, y, gColor);
                x++;
                y++;
            }

            x = grid.Width / 2;
            y = 0;
            while (x < grid.Width && y < grid.Height)
            {
                grid.SetPixel(x, y, gColor);
                x++;
                y++;
            }

            x = grid.Width / 2;
            y = 0;
            while (x > 0 && y < grid.Height)
            {
                grid.SetPixel(x, y, gColor);
                x--;
                y++;
            }

            x = grid.Width - 1;
            y = grid.Height / 2;
            while (x > 0 && y < grid.Height)
            {
                grid.SetPixel(x, y, gColor);
                x--;
                y++;
            }

            return grid;
        }

        public void SaveImages()
        {
            int startingNumber = StartingFileNumber;
            foreach (var image in Slices)
            {
                if (!Directory.Exists(OutputDirectory))
                {
                    Directory.CreateDirectory(OutputDirectory);
                }

                image.Save(Path.Combine(OutputDirectory, string.Format(FileNameFormat + ".bmp", startingNumber)), System.Drawing.Imaging.ImageFormat.Bmp);
                startingNumber++;
            }

            GenSampleGrid().Save(Path.Combine(OutputDirectory, "grid.png"), System.Drawing.Imaging.ImageFormat.Png);
        }

        public void CreateHtmlLayout()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<body bgcolor='{ColorTranslator.ToHtml(BackgroundColor)}' style='padding: 0px; border: 0px; margin: 0px;'>\n");

            int startingNumber = StartingFileNumber;
            string[][] tables = new string[ySlices][];

            for (int i = 0; i < tables.Length; i++)
            {
                tables[i] = new string[xSlices];
            }

            int c = 0, r = 0;
            foreach (var image in Slices)
            {
                string fname = string.Format(FileNameFormat + ".bmp", startingNumber);
                int gx = c;
                int gy = r;

                Point position = GetSectionStartPosition(gy, gx);

                tables[r][c] = $"<img src='{fname}' title='{fname} ({gx}, {gy})' style='" +
                    "position: absolute;" +
                    $"top: {position.Y * 2 + TileHeight};" +
                    $"left: {position.X * 2 + TileWidth}" +
                    "'>";
                c++;

                if (c >= xSlices)
                {
                    c = 0;
                    r++;
                }
                startingNumber++;
            }

            for (int i = 0; i < tables.Length; i++)
            {
                for (int ic = 0; ic < tables[i].Length; ic++)
                {
                    sb.AppendLine(tables[i][ic]);
                }
            }

            sb.AppendLine($"<div style=\"background-image: url('grid.png'); position: absolute; top: 0; left: 0; width: 100%; height: 100%; pointer-events: none;\"></div>");
            sb.AppendLine("</body>");

            try
            {
                File.WriteAllText(Path.Combine(OutputDirectory, "layout.html"), sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

