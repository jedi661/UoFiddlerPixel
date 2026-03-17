using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace UoFiddler.Controls.Models.Uop.Imaging
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            Graphics.FromImage(Bitmap).Clear(Color.Transparent);
        }

        public DirectBitmap(Bitmap bmp)
        {
            if (bmp == null) throw new ArgumentNullException(nameof(bmp), "Bitmap cannot be null.");

            Width = bmp.Width;
            Height = bmp.Height;
            Bits = new Int32[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());

            // CORRECTION : Copier les pixels directement depuis le bitmap source
            using (var bmpArgb = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmpArgb))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(bmp, 0, 0, Width, Height);
                }

                // Copier les pixels depuis le bitmap converti
                BitmapData sourceData = bmpArgb.LockBits(
                    new Rectangle(0, 0, Width, Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                try
                {
                    unsafe
                    {
                        int* sourcePtr = (int*)sourceData.Scan0;
                        int sourceStride = sourceData.Stride / 4;

                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                int sourceIndex = y * sourceStride + x;
                                int destIndex = y * Width + x;
                                Bits[destIndex] = sourcePtr[sourceIndex];
                            }
                        }
                    }
                }
                finally
                {
                    bmpArgb.UnlockBits(sourceData);
                }
            }
        }

        public void SetPixel(int x, int y, Color colour)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException($"Pixel coordinates ({x}, {y}) are outside bitmap bounds (Width={Width}, Height={Height})");
            }
            
            int index = x + (y * Width);
            int col = colour.ToArgb();
            Bits[index] = col;
        }

        public Color GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                // Log l'erreur pour diagnostic
                System.Diagnostics.Debug.WriteLine($"ERROR: GetPixel({x}, {y}) out of bounds. Width={Width}, Height={Height}, Bits.Length={Bits.Length}");
                return Color.Transparent; // Retourner transparent au lieu de crasher
            }
            
            int index = x + (y * Width);
            
            // Double vérification de sécurité
            if (index < 0 || index >= Bits.Length)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Calculated index {index} out of bounds. x={x}, y={y}, Width={Width}, Height={Height}, Bits.Length={Bits.Length}");
                return Color.Transparent;
            }
            
            int col = Bits[index];
            return Color.FromArgb(col);
        }
        
        public DirectBitmap ToGifDBmp()
        {
            return new DirectBitmap(ToGif());
        }

        public Bitmap ToGif()
        {
            OctreeQuantizer quantizer = new OctreeQuantizer();
            
            // Vérifier que Width et Height correspondent à la taille de Bits
            int expectedLength = Width * Height;
            if (Bits.Length != expectedLength)
            {
                throw new InvalidOperationException($"DirectBitmap internal state is corrupted. Width={Width}, Height={Height}, expected Bits.Length={expectedLength}, actual={Bits.Length}");
            }
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    quantizer.AddColor(GetPixel(x, y));
                }
            }
            
            List<Color> limitedPalette = quantizer.GetPalette(256);
            
            int stride = (Width + 3) & ~3;
            byte[] b = new byte[Height * stride];
            GCHandle gch = GCHandle.Alloc(b, GCHandleType.Pinned);

            using (Bitmap quantizedBmp = new Bitmap(Width, Height, stride, PixelFormat.Format8bppIndexed, gch.AddrOfPinnedObject()))
            {
                ColorPalette pal = quantizedBmp.Palette;
                for (int i = 0; i < pal.Entries.Length; i++)
                {
                    pal.Entries[i] = Color.Transparent;
                    if (i < limitedPalette.Count)
                        pal.Entries[i] = limitedPalette[i];
                }
                quantizedBmp.Palette = pal;

                BitmapData bmpData = quantizedBmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, quantizedBmp.PixelFormat);
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int paletteIndex = quantizer.GetPaletteIndex(GetPixel(x, y));
                        Marshal.WriteByte(bmpData.Scan0 + (y * bmpData.Stride) + x, (byte)paletteIndex);
                    }
                }
                quantizedBmp.UnlockBits(bmpData);
                gch.Free();
                return (Bitmap)quantizedBmp.Clone();
            }
        }
        
        public static int GetDistance(Color current, Color match)
        {
            int redDifference = current.R - match.R;
            int greenDifference = current.G - match.G;
            int blueDifference = current.B - match.B;
            return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
            GC.SuppressFinalize(this);
        }
    }
}
