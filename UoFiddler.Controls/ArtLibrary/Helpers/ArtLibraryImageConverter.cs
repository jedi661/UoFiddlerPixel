// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using UoFiddler.Controls.ArtLibrary.Core;

namespace UoFiddler.Controls.ArtLibrary.Helpers
{
    /// <summary>
    /// Which colors become transparent when an external image is imported.
    /// Same idea as UoFiddler's import (Utils.ConvertBmp): pure black and pure white are background.
    /// </summary>
    public sealed class ArtImportOptions
    {
        /// <summary>Pure black (0,0,0) becomes transparent.</summary>
        public bool BlackTransparent { get; set; } = true;

        /// <summary>Pure white (255,255,255 / 254,254,254) becomes transparent.</summary>
        public bool WhiteTransparent { get; set; } = true;

        /// <summary>Light gray #D3D3D3 (UoFiddler clipboard background) becomes transparent.</summary>
        public bool LightGrayTransparent { get; set; } = true;

        public static ArtImportOptions Default => new ArtImportOptions();
    }

    /// <summary>
    /// Converts external images (files, clipboard) into ARGB1555 bitmaps ready for art.mul / the library.
    /// </summary>
    public static class ArtLibraryImageConverter
    {
        private const ushort Black1555 = 0x8000;
        private const ushort White1555 = 0xFFFF;

        // #D3D3D3 -> 211 >> 3 = 26 per channel
        private const ushort LightGray1555 = 0x8000 | (26 << 10) | (26 << 5) | 26;

        public static readonly string[] SupportedExtensions = { ".bmp", ".png", ".tif", ".tiff", ".gif", ".jpg", ".jpeg" };

        public static bool IsSupportedFile(string path)
        {
            string extension = Path.GetExtension(path ?? string.Empty).ToLowerInvariant();
            return Array.IndexOf(SupportedExtensions, extension) >= 0;
        }

        /// <summary>
        /// Loads an image file without keeping the file locked.
        /// </summary>
        public static Bitmap LoadFile(string path)
        {
            using (var stream = new MemoryStream(File.ReadAllBytes(path)))
            using (var image = Image.FromStream(stream))
            {
                return new Bitmap(image);
            }
        }

        /// <summary>
        /// Returns a new ARGB1555 bitmap with the background colors of <paramref name="options"/> made transparent.
        /// </summary>
        public static Bitmap PrepareForArt(Image source, ArtImportOptions options = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            options ??= ArtImportOptions.Default;

            using (var bitmap = new Bitmap(source))
            {
                ushort[] pixels = ArtLibraryCodec.ToPixels(bitmap);
                for (int i = 0; i < pixels.Length; ++i)
                {
                    ushort pixel = pixels[i];
                    if ((options.BlackTransparent && pixel == Black1555)
                        || (options.WhiteTransparent && pixel == White1555)
                        || (options.LightGrayTransparent && pixel == LightGray1555))
                    {
                        pixels[i] = 0;
                    }
                }

                return ArtLibraryCodec.ToBitmap(pixels, bitmap.Width, bitmap.Height);
            }
        }

        /// <summary>
        /// Loads a file and prepares it for the art format.
        /// </summary>
        public static Bitmap LoadFileForArt(string path, ArtImportOptions options = null)
        {
            using (Bitmap loaded = LoadFile(path))
            {
                return PrepareForArt(loaded, options);
            }
        }

        /// <summary>
        /// Copy of an art bitmap for export: transparent pixels become <paramref name="background"/>
        /// (black by default, so the image can be imported again without loss).
        /// </summary>
        public static Bitmap ToExportBitmap(Bitmap art, Color? background = null)
        {
            var result = new Bitmap(art.Width, art.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(background ?? Color.Black);
                g.DrawImageUnscaled(art, 0, 0);
            }

            return result;
        }

        public static ImageFormat GetImageFormat(string extension)
        {
            switch ((extension ?? string.Empty).TrimStart('.').ToLowerInvariant())
            {
                case "png":
                    return ImageFormat.Png;
                case "tif":
                case "tiff":
                    return ImageFormat.Tiff;
                case "jpg":
                case "jpeg":
                    return ImageFormat.Jpeg;
                default:
                    return ImageFormat.Bmp;
            }
        }
    }
}
