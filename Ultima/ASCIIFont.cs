using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

// ascii text support written by arul
namespace Ultima
{
    #region class AsciiFont
    public sealed class AsciiFont
    {
        public byte Header { get; }
        public byte[] Unk { get; set; }
        public Bitmap[] Characters { get; set; }
        public int Height { get; set; }

        #region AsciiFont
        public AsciiFont(byte header)
        {
            Header = header;
            Height = 0;
            Unk = new byte[224];
            Characters = new Bitmap[224];
        }
        #endregion

        #region Bitmap GetBitmap
        /// <summary>
        /// Gets Bitmap of given character
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public Bitmap GetBitmap(char character)
        {
            return Characters[((character - 0x20) & 0x7FFFFFFF) % 224];
        }
        #endregion

        #region GetWidth
        public int GetWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int width = 0;

            foreach (var character in text)
            {
                width += GetBitmap(character).Width;
            }

            return width;
        }
        #endregion

        #region ReplaceCharacter
        public void ReplaceCharacter(int character, Bitmap import)
        {
            Characters[character] = import;
            Height = import.Height;
        }
        #endregion

        #region AsciiFont GetFixed
        public static AsciiFont GetFixed(int font, AsciiFont[] fonts)
        {
            if (font is < 0 or > 9)
            {
                font = 3;
            }

            return fonts[font];
        }
        #endregion
    }
    #endregion

    #region class AsciiText
    public static class AsciiText
    {
        public static readonly AsciiFont[] Fonts = new AsciiFont[10]; // Index

        static AsciiText()
        {
            Initialize();
        }

        #region Initialize fonts.mul
        /// <summary>
        /// Reads fonts.mul
        /// </summary>
        public static unsafe void Initialize()
        {
            string path = Files.GetFilePath("fonts.mul");
            if (path == null)
            {
                return;
            }

            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[(int)reader.Length];
                reader.Read(buffer, 0, (int)reader.Length);
                fixed (byte* bin = buffer)
                {
                    byte* read = bin;
                    for (int i = 0; i < 10; ++i) // index
                    {
                        byte header = *read++;
                        Fonts[i] = new AsciiFont(header);

                        for (int k = 0; k < 224; ++k)
                        {
                            byte width = *read++;
                            byte height = *read++;
                            byte unk = *read++; // delimiter?

                            if (width <= 0 || height <= 0)
                            {
                                continue;
                            }

                            if (height > Fonts[i].Height && k < 96)
                            {
                                Fonts[i].Height = height;
                            }

                            var bmp = new Bitmap(width, height);
                            BitmapData bd = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                            var line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;

                            for (int y = 0; y < height; ++y, line += delta)
                            {
                                ushort* cur = line;
                                for (int x = 0; x < width; ++x)
                                {
                                    var pixel = (ushort)(*read++ | (*read++ << 8));
                                    if (pixel == 0)
                                    {
                                        cur[x] = pixel;
                                    }
                                    else
                                    {
                                        cur[x] = (ushort)(pixel ^ 0x8000);
                                    }
                                }
                            }
                            bmp.UnlockBits(bd);

                            Fonts[i].Characters[k] = bmp;
                            Fonts[i].Unk[k] = unk;
                        }
                    }
                }
            }
        }
        #endregion

        #region Save
        public static unsafe void Save(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var bin = new BinaryWriter(fs))
            {
                for (int i = 0; i < 10; ++i)
                {
                    bin.Write(Fonts[i].Header);
                    for (int k = 0; k < 224; ++k)
                    {
                        bin.Write((byte)Fonts[i].Characters[k].Width);
                        bin.Write((byte)Fonts[i].Characters[k].Height);
                        bin.Write(Fonts[i].Unk[k]);
                        Bitmap bmp = Fonts[i].Characters[k];
                        BitmapData bd = bmp.LockBits(
                            new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                            PixelFormat.Format16bppArgb1555);
                        var line = (ushort*)bd.Scan0;
                        int delta = bd.Stride >> 1;
                        for (int y = 0; y < bmp.Height; ++y, line += delta)
                        {
                            ushort* cur = line;
                            for (int x = 0; x < bmp.Width; ++x)
                            {
                                if (cur[x] == 0)
                                {
                                    bin.Write(cur[x]);
                                }
                                else
                                {
                                    bin.Write((ushort)(cur[x] ^ 0x8000));
                                }
                            }
                        }

                        bmp.UnlockBits(bd);
                    }
                }
            }
        }
        #endregion

        #region Bitmap DrawText
        /// <summary>
        /// Draws Text with font in Bitmap and returns
        /// </summary>
        /// <param name="fontId"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Bitmap DrawText(int fontId, string text)
        {
            AsciiFont font = AsciiFont.GetFixed(fontId, Fonts);
            var result = new Bitmap(font.GetWidth(text) + 2, font.Height + 2);

            int dx = 2;
            int dy = font.Height + 2;
            using (Graphics graph = Graphics.FromImage(result))
            {
                foreach (var character in text)
                {
                    Bitmap bmp = font.GetBitmap(character);
                    graph.DrawImage(bmp, dx, dy - bmp.Height);
                    dx += bmp.Width;
                }
            }

            return result;
        }
        #endregion
    }
    #endregion
}
