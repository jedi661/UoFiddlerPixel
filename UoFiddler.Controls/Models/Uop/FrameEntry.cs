using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using UoFiddler.Controls.Models.Uop.Imaging;

namespace UoFiddler.Controls.Models.Uop
{
    public class FrameEntry : IDisposable
    {
        private ushort m_ID;
        private ushort m_Frame;
        private short m_CenterX;
        private short m_CenterY;
        private short m_InitCoordsX;
        private short m_InitCoordsY;
        private short m_EndCoordsX;
        private short m_EndCoordsY;
        private uint m_DataOffset;
        private int m_width;
        private int m_height;
        private DirectBitmap m_image;
        private bool m_originalVD = false;
        private string[,] m_VDImageData;
        private List<ColourEntry> m_VDFrameColors = new List<ColourEntry>();
        private const int _doubleXor = (0x200 << 22) | (0x200 << 12);

        public bool disposed = false;
        
        public short CenterX => m_CenterX;
        public short CenterY => m_CenterY;
        public short InitCoordsX => m_InitCoordsX;
        public short InitCoordsY => m_InitCoordsY;
        public int Width => m_width;
        public int Height => m_height;
        public DirectBitmap Image { get => m_image; set => m_image = value; }
        public List<ColourEntry> VDFrameColors => m_VDFrameColors;

        public FrameEntry(UopFrameExportData frameData)
        {
            m_ID = frameData.ID;
            m_Frame = frameData.Frame;
            m_InitCoordsX = frameData.InitCoordsX;
            m_InitCoordsY = frameData.InitCoordsY;
            m_EndCoordsX = frameData.EndCoordsX;
            m_EndCoordsY = frameData.EndCoordsY;
            m_DataOffset = frameData.DataOffset;
            m_width = frameData.Width;
            m_height = frameData.Height;
            m_CenterX = frameData.CenterX;
            m_CenterY = frameData.CenterY;
            m_image = frameData.Image; // DirectBitmap
            
            m_VDFrameColors = new List<ColourEntry>();
            if (frameData.Palette != null && frameData.Palette.Count > 0)
            {
                m_VDFrameColors = frameData.Palette;
            }
            
            m_VDImageData = new string[m_width, m_height];
            if (m_VDFrameColors.Count > 0)
            {
                for (int y = 0; y < m_height; y++)
                {
                    for (int x = 0; x < m_width; x++)
                    {
                        Color pixelColor = m_image.GetPixel(x, y);
                        if (pixelColor.A > 0)
                        {
                            int paletteIndex = GetPaletteIndex(pixelColor, m_VDFrameColors);
                            m_VDImageData[x, y] = paletteIndex.ToString();
                        }
                    }
                }
            }
        }

        public void Resize(double scale)
        {
            if (scale == 1.0) return;

            int newWidth = (int)Math.Max(1, m_width * scale);
            int newHeight = (int)Math.Max(1, m_height * scale);

            // Resize Image
            DirectBitmap resizedImage = new DirectBitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resizedImage.Bitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(m_image.Bitmap, 0, 0, newWidth, newHeight);
            }

            // Replace old image
            if (m_image != null) m_image.Dispose();
            m_image = resizedImage;

            // Update Dimensions and Centers
            m_width = newWidth;
            m_height = newHeight;
            m_CenterX = (short)(m_CenterX * scale);
            m_CenterY = (short)(m_CenterY * scale);

            // Re-generate VDImageData
            m_VDImageData = new string[m_width, m_height];
            if (m_VDFrameColors.Count > 0)
            {
                for (int y = 0; y < m_height; y++)
                {
                    for (int x = 0; x < m_width; x++)
                    {
                        Color pixelColor = m_image.GetPixel(x, y);
                        if (pixelColor.A > 0)
                        {
                            int paletteIndex = GetPaletteIndex(pixelColor, m_VDFrameColors);
                            m_VDImageData[x, y] = paletteIndex.ToString();
                        }
                    }
                }
            }
        }

        public void ExportVDImageData(BinaryWriter writer, DirectBitmap frameImage, short centerX, short centerY, int topX, int topY, List<ColourEntry> colors)
        {
            writer.Write(centerX);
            writer.Write(centerY);
            
            // CORRECTION : Utiliser les dimensions r�elles de frameImage
            int actualWidth = m_originalVD ? m_width : frameImage.Width;
            int actualHeight = m_originalVD ? m_height : frameImage.Height;
            
            writer.Write((ushort)actualWidth);
            writer.Write((ushort)actualHeight);

            // CORRECTION : Boucler sur les dimensions r�elles
            for (int y = 0; y < actualHeight; y++)
            {
                int i = 0;
                int x = 0;
                while (i < actualWidth)
                {
                    // CORRECTION : V�rifier les limites de m_VDImageData
                    for (i = x; i < actualWidth && i < m_width; i++)
                    {
                        if (m_VDImageData[i, y] != null)
                        {
                            break;
                        }
                    }

                    if (i >= actualWidth || i >= m_width) continue;

                    int j;
                    for (j = i + 1; j < actualWidth && j < m_width; j++)
                    {
                        if (m_VDImageData[j, y] == null)
                        {
                            break;
                        }
                    }

                    int length = j - i;
                    int xOffset = ((j - length - centerX) + (m_originalVD ? 0 : topX)) + 0x200;
                    int yOffset = ((y - centerY - actualHeight) + (m_originalVD ? 0 : topY)) + 0x200;

                    byte[] data = new byte[length];
                    for (int r = 0; r < length; r++)
                    {
                        if (m_originalVD)
                        {
                            string stringColor = m_VDImageData[r + i, y];
                            data[r] = (byte)int.Parse(stringColor);
                        }
                        else
                        {
                            // CORRECTION : V�rifier les limites avant d'acc�der au pixel
                            int pixelX = r + i + topX;
                            int pixelY = y + topY;
                            
                            if (pixelX >= 0 && pixelX < frameImage.Width && pixelY >= 0 && pixelY < frameImage.Height)
                            {
                                data[r] = (byte)GetPaletteIndex(frameImage.GetPixel(pixelX, pixelY), colors);
                            }
                            else
                            {
                                data[r] = 0; // Couleur transparente/noire
                            }
                        }
                    }
                    
                    int header = length | (yOffset << 12) | (xOffset << 22);
                    header ^= _doubleXor;
                    writer.Write(header);

                    foreach (byte b in data)
                    {
                        writer.Write(b);
                    }
                    
                    x = j + 1;
                    i = x;
                }
            }
            writer.Write(0x7FFF7FFF);
        }

        private int GetPaletteIndex(Color col, List<ColourEntry> colors)
        {
            List<ColourEntry> cols = colors ?? m_VDFrameColors;
            int found = cols.FindIndex(c => c.ColorRGB555 == ColourEntry.ARGBtoRGB555(col.R, col.G, col.B, col.A));
            if (found != -1) return found;
            return FindNearestColor(cols, col);
        }

        private int FindNearestColor(List<ColourEntry> map, Color current)
        {
            int colorDiffs = map.Select(n => DirectBitmap.GetDistance(Color.FromArgb(n.Alpha, n.R, n.G, n.B), current)).Min();
            return map.FindIndex(n => DirectBitmap.GetDistance(Color.FromArgb(n.Alpha, n.R, n.G, n.B), current) == colorDiffs);
        }
        
        public void Dispose()
        {
            if (disposed) return;
            if (m_image != null && !m_image.Disposed)
                m_image.Dispose();
            m_image = null;
            disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
