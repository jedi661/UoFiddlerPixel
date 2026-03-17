using System.Drawing;

namespace UoFiddler.Controls.Models.Uop
{
    public class ColourEntry
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte Alpha { get; set; }
        public Color Pixel { get; set; }
        public ushort ColorRGB555 { get; set; }

        public ColourEntry(byte R, byte G, byte B, byte Alpha)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.Alpha = Alpha;
            this.Pixel = Color.FromArgb(Alpha, R, G, B);
            this.ColorRGB555 = ARGBtoRGB555(this.Pixel);
        }

        public ColourEntry(Color col)
        {
            this.R = col.R;
            this.G = col.G;
            this.B = col.B;
            this.Alpha = col.A;
            this.Pixel = col;
            this.ColorRGB555 = ARGBtoRGB555(this.Pixel);
        }

        public ColourEntry(ushort colorRGB555)
        {
            this.ColorRGB555 = colorRGB555;
            this.Alpha = 255; // Assume opaque for 555
            this.R = (byte)(((colorRGB555 >> 10) & 0x1F) * 255 / 31);
            this.G = (byte)(((colorRGB555 >> 5) & 0x1F) * 255 / 31);
            this.B = (byte)((colorRGB555 & 0x1F) * 255 / 31);
            this.Pixel = Color.FromArgb(this.Alpha, this.R, this.G, this.B);
        }

        public static ushort ARGBtoRGB555(Color color)
        {
            return (ushort)((((color.R & 0xF8) << 7) | ((color.G & 0xF8) << 2) | (color.B >> 3)) | (color.A != 0 ? 0x8000 : 0));
        }

        public static ushort ARGBtoRGB555(byte r, byte g, byte b, byte a)
        {
            return ARGBtoRGB555(Color.FromArgb(a, r, g, b));
        }
    }
}
