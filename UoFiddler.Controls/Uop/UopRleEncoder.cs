using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace UoFiddler.Controls.Uop
{
    public static class UopRleEncoder
    {
        public static byte[] Encode(Bitmap bitmap, List<Color> palette, short centerX, short centerY)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // 1. Write Palette (512 bytes)
                // UOP Palettes are 256 entries * 2 bytes (Little Endian 555 or 565? Usually 555 in UO)
                // Based on UopAnimationDataManager.ReadPalette, it reads UInt16.
                
                // We need to map current palette colors back to 555 ushorts.
                // Note: The palette provided here should match the one used to index the bitmap.
                foreach (var color in palette)
                {
                    // Convert ARGB to 15-bit 555 (A RRRR GGGG BBBB)
                    // Or usually UO is XRRRRGGGGGBBBB (1555)
                    ushort val = (ushort)(((color.R >> 3) << 10) | ((color.G >> 3) << 5) | (color.B >> 3));
                    // Check if alpha bit is needed? Usually 0 is transparent in RLE, but palette colors are solid.
                    // The reader ignores the top bit usually, or it's opacity.
                    // UopAnimationDataManager: 
                    // byte r = (byte)(((color555 >> 10) & 0x1F) << 3);
                    
                    writer.Write(val);
                }
                
                // Fill remaining palette if less than 256 (should not happen if list is full)
                for (int i = palette.Count; i < 256; i++)
                {
                    writer.Write((ushort)0);
                }

                // 2. Write Frame Header
                // short CenterX, short CenterY, ushort Width, ushort Height
                writer.Write(centerX);
                writer.Write(centerY);
                writer.Write((ushort)bitmap.Width);
                writer.Write((ushort)bitmap.Height);

                // 3. Encode RLE Data
                EncodeBitmapRle(writer, bitmap, palette, centerX, centerY);

                return ms.ToArray();
            }
        }

        private static void EncodeBitmapRle(BinaryWriter writer, Bitmap bmp, List<Color> palette, short centerX, short centerY)
        {
            // Lock bits for speed
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            
            try
            {
                unsafe
                {
                    byte* ptr = (byte*)bd.Scan0;
                    int stride = bd.Stride;
                    int width = bmp.Width;
                    int height = bmp.Height;

                    // Formulas derived from decoding:
                    // FinalY = Height - 1 - (-y - CenterY)
                    // => FinalY = Height - 1 + y + CenterY
                    // => y = FinalY - Height + 1 - CenterY
                    
                    // FinalX = CenterX + x + i
                    // => x = FinalX - CenterX (for the start of the run)

                    for (int py = 0; py < height; py++)
                    {
                        int row = py; // This is FinalY
                        
                        // Calculate raw Y component for RLE
                        int realY = row - height + 1 - centerY;

                        // Check 10-bit range for Y (-512 to 511)
                        if (realY < -512 || realY > 511) continue; // Cannot encode this line

                        // Encode signed 10-bit Y
                        // If negative: 0x400 - abs(y) ? No. 
                        // Decoding: if (raw & 0x200) y = -(0x400 - raw) => y = raw - 0x400
                        // So if y < 0: raw = 0x400 + y. 
                        // Example: y = -5. raw = 1024 - 5 = 1019 (0x3FB). Correct.
                        uint rawY = (uint)(realY < 0 ? 0x400 + realY : realY);
                        rawY &= 0x3FF; // Ensure 10 bits

                        int px = 0;
                        while (px < width)
                        {
                            // Find start of opaque pixels
                            while (px < width && IsTransparent(ptr, stride, px, py))
                            {
                                px++;
                            }

                            if (px >= width) break; // End of line

                            int runStart = px;

                            // Find end of run
                            while (px < width && !IsTransparent(ptr, stride, px, py))
                            {
                                px++;
                                // Max run length is 12 bits (4095), practically limited by width
                            }

                            int runLength = px - runStart;

                            // Calculate raw X component for RLE start
                            int realX = runStart - centerX;
                            
                            // Check 10-bit range for X
                             if (realX < -512 || realX > 511)
                             {
                                 // Skip this run or clipping occurred
                                 continue;
                             }

                             // Encode signed 10-bit X
                             uint rawX = (uint)(realX < 0 ? 0x400 + realX : realX);
                             rawX &= 0x3FF;

                             // Construct Header
                             // Header = (rawX << 22) | (rawY << 12) | runLength
                             uint rleHeader = (rawX << 22) | (rawY << 12) | (uint)runLength;
                             writer.Write(rleHeader);

                             // Write Indices
                             for (int k = 0; k < runLength; k++)
                             {
                                 int currentX = runStart + k;
                                 Color c = GetPixel(ptr, stride, currentX, py);
                                 byte index = FindPaletteIndex(c, palette);
                                 writer.Write(index);
                             }
                        }
                    }
                }
                
                // Write End Marker
                writer.Write((uint)0x7FFF7FFF);
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
        }

        private static unsafe bool IsTransparent(byte* ptr, int stride, int x, int y)
        {
            // BGRA
            int offset = y * stride + x * 4;
            // Assuming 0 alpha is transparent. UO usually treats black (0,0,0) as transparent in some contexts, 
            // but for 32bpp bitmap from decoder, we rely on Alpha.
            return ptr[offset + 3] == 0; 
        }

        private static unsafe Color GetPixel(byte* ptr, int stride, int x, int y)
        {
             int offset = y * stride + x * 4;
             byte b = ptr[offset];
             byte g = ptr[offset + 1];
             byte r = ptr[offset + 2];
             byte a = ptr[offset + 3];
             return Color.FromArgb(a, r, g, b);
        }

        private static byte FindPaletteIndex(Color c, List<Color> palette)
        {
            // Exact match optimization
            for (int i = 0; i < palette.Count; i++)
            {
                if (palette[i].R == c.R && palette[i].G == c.G && palette[i].B == c.B)
                    return (byte)i;
            }
            
            // Nearest match (Euclidean distance)
            int bestIndex = 0;
            int minDist = int.MaxValue;
            
            for (int i = 0; i < palette.Count; i++)
            {
                int dr = c.R - palette[i].R;
                int dg = c.G - palette[i].G;
                int db = c.B - palette[i].B;
                int dist = dr*dr + dg*dg + db*db;
                
                if (dist < minDist)
                {
                    minDist = dist;
                    bestIndex = i;
                    if (minDist == 0) break;
                }
            }
            return (byte)bestIndex;
        }
    }
}
