// /***************************************************************************
//  *
//  * Dragon Map Studio – palette files (ACT, PAL, ACO, GPL, BMP)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>
    /// Reads and writes the palette formats of the Dragon "Addon" folder:
    /// Photoshop .act (colour table) and .aco (swatches), JASC / RIFF .pal, GIMP .gpl and the palette of an 8-bit .bmp.
    /// </summary>
    public static class PaletteIO
    {
        public const string FileFilter =
            "Alle Paletten|*.bmp;*.act;*.pal;*.aco;*.gpl|8-Bit BMP|*.bmp|Photoshop Farbtabelle|*.act|Paletten (JASC/RIFF)|*.pal|Photoshop Farbfelder|*.aco|GIMP Palette|*.gpl";

        public const string SaveFilter =
            "Photoshop Farbtabelle (*.act)|*.act|JASC Palette (*.pal)|*.pal|Photoshop Farbfelder (*.aco)|*.aco|GIMP Palette (*.gpl)|*.gpl";

        /// <summary>Loads 256 colours (missing entries are black).</summary>
        public static Color[] Load(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            byte[] data = File.ReadAllBytes(path);
            Color[] colors;
            switch (ext)
            {
                case ".bmp":
                    colors = ReadBmpPalette(data);
                    break;
                case ".act":
                    colors = ReadAct(data);
                    break;
                case ".aco":
                    colors = ReadAco(data);
                    break;
                case ".gpl":
                    colors = ReadGpl(File.ReadAllLines(path));
                    break;
                case ".pal":
                    colors = data.Length >= 4 && data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F'
                        ? ReadRiffPal(data)
                        : ReadJascPal(File.ReadAllLines(path));
                    break;
                default:
                    throw new NotSupportedException($"Unbekanntes Paletten-Format: {ext}");
            }

            var result = new Color[256];
            for (int i = 0; i < 256; i++)
            {
                result[i] = i < colors.Length ? colors[i] : Color.Black;
            }
            return result;
        }

        public static void Save(string path, Color[] colors, IList<string> names = null)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            switch (ext)
            {
                case ".act":
                    File.WriteAllBytes(path, WriteAct(colors));
                    break;
                case ".aco":
                    File.WriteAllBytes(path, WriteAco(colors, names));
                    break;
                case ".gpl":
                    File.WriteAllText(path, WriteGpl(colors, names));
                    break;
                default:
                    File.WriteAllText(path, WriteJascPal(colors));
                    break;
            }
        }

        // ------------------------------------------------------------------ BMP

        public static Color[] ReadBmpPalette(byte[] data)
        {
            if (data.Length < 54 || data[0] != 'B' || data[1] != 'M')
            {
                throw new InvalidDataException("Keine BMP-Datei.");
            }
            int headerSize = BitConverter.ToInt32(data, 14);
            int bpp = BitConverter.ToUInt16(data, 28);
            if (bpp > 8)
            {
                throw new InvalidDataException($"Das BMP hat {bpp} Bit – eine Farbtabelle braucht ein 8-Bit-BMP mit Palette.");
            }
            int used = headerSize >= 40 ? BitConverter.ToInt32(data, 46) : 0;
            int count = used > 0 ? used : 1 << bpp;
            int offset = 14 + headerSize;
            var colors = new Color[256];
            for (int i = 0; i < 256; i++)
            {
                int p = offset + i * 4;
                colors[i] = i < count && p + 2 < data.Length ? Color.FromArgb(data[p + 2], data[p + 1], data[p]) : Color.Black;
            }
            return colors;
        }

        // ------------------------------------------------------------------ ACT

        public static Color[] ReadAct(byte[] data)
        {
            int count = 256;
            if (data.Length >= 772)
            {
                int c = (data[768] << 8) | data[769];
                if (c > 0 && c <= 256)
                {
                    count = c;
                }
            }
            var colors = new Color[256];
            for (int i = 0; i < 256; i++)
            {
                colors[i] = i < count && i * 3 + 2 < data.Length ? Color.FromArgb(data[i * 3], data[i * 3 + 1], data[i * 3 + 2]) : Color.Black;
            }
            return colors;
        }

        public static byte[] WriteAct(Color[] colors)
        {
            var data = new byte[768];
            for (int i = 0; i < 256 && i < colors.Length; i++)
            {
                data[i * 3] = colors[i].R;
                data[i * 3 + 1] = colors[i].G;
                data[i * 3 + 2] = colors[i].B;
            }
            return data;
        }

        // ------------------------------------------------------------------ PAL

        public static Color[] ReadJascPal(string[] lines)
        {
            if (lines.Length < 3 || !lines[0].Trim().Equals("JASC-PAL", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Keine JASC-Palette.");
            }
            var list = new List<Color>();
            for (int i = 3; i < lines.Length && list.Count < 256; i++)
            {
                string[] t = lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (t.Length >= 3)
                {
                    list.Add(Color.FromArgb(Clamp(t[0]), Clamp(t[1]), Clamp(t[2])));
                }
            }
            return list.ToArray();
        }

        public static string WriteJascPal(Color[] colors)
        {
            var sb = new StringBuilder();
            sb.Append("JASC-PAL\r\n0100\r\n256\r\n");
            for (int i = 0; i < 256; i++)
            {
                Color c = i < colors.Length ? colors[i] : Color.Black;
                sb.Append(CultureInfo.InvariantCulture, $"{c.R} {c.G} {c.B}\r\n");
            }
            return sb.ToString();
        }

        public static Color[] ReadRiffPal(byte[] data)
        {
            // RIFF....PAL data<size><version 2><count 2><entries 4 bytes r g b flags>
            for (int p = 12; p + 8 <= data.Length;)
            {
                string id = Encoding.ASCII.GetString(data, p, 4);
                int size = BitConverter.ToInt32(data, p + 4);
                if (id == "data")
                {
                    int count = BitConverter.ToUInt16(data, p + 10);
                    var colors = new Color[Math.Min(256, count)];
                    for (int i = 0; i < colors.Length; i++)
                    {
                        int o = p + 12 + i * 4;
                        colors[i] = Color.FromArgb(data[o], data[o + 1], data[o + 2]);
                    }
                    return colors;
                }
                p += 8 + size + (size & 1);
            }
            throw new InvalidDataException("RIFF-Palette ohne data-Block.");
        }

        // ------------------------------------------------------------------ ACO

        public static Color[] ReadAco(byte[] data)
        {
            int count = (data[2] << 8) | data[3];
            var list = new List<Color>();
            for (int i = 0; i < count && list.Count < 256; i++)
            {
                int o = 4 + i * 10;
                if (o + 10 > data.Length)
                {
                    break;
                }
                int space = (data[o] << 8) | data[o + 1];
                int w = (data[o + 2] << 8) | data[o + 3];
                int x = (data[o + 4] << 8) | data[o + 5];
                int y = (data[o + 6] << 8) | data[o + 7];
                list.Add(space == 0 ? Color.FromArgb(w >> 8, x >> 8, y >> 8) : Color.Black);
            }
            return list.ToArray();
        }

        /// <summary>Writes ACO version 1 + version 2 (with names).</summary>
        public static byte[] WriteAco(Color[] colors, IList<string> names)
        {
            using var ms = new MemoryStream();
            void U16(int v)
            {
                ms.WriteByte((byte)(v >> 8));
                ms.WriteByte((byte)v);
            }
            void Color16(Color c)
            {
                U16(0);
                U16(c.R * 257);
                U16(c.G * 257);
                U16(c.B * 257);
                U16(0);
            }

            U16(1);
            U16(256);
            for (int i = 0; i < 256; i++)
            {
                Color16(i < colors.Length ? colors[i] : Color.Black);
            }
            U16(2);
            U16(256);
            for (int i = 0; i < 256; i++)
            {
                Color16(i < colors.Length ? colors[i] : Color.Black);
                string name = $"{i:x2} " + (names != null && i < names.Count ? names[i] : string.Empty);
                ms.WriteByte(0);
                ms.WriteByte(0);
                U16(name.Length + 1);
                foreach (char ch in name)
                {
                    U16(ch);
                }
                U16(0);
            }
            return ms.ToArray();
        }

        // ------------------------------------------------------------------ GPL

        public static Color[] ReadGpl(string[] lines)
        {
            var list = new List<Color>();
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal) || line.StartsWith("GIMP", StringComparison.Ordinal) ||
                    line.StartsWith("Name:", StringComparison.Ordinal) || line.StartsWith("Columns:", StringComparison.Ordinal))
                {
                    continue;
                }
                string[] t = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (t.Length >= 3 && int.TryParse(t[0], out _))
                {
                    list.Add(Color.FromArgb(Clamp(t[0]), Clamp(t[1]), Clamp(t[2])));
                }
            }
            return list.ToArray();
        }

        public static string WriteGpl(Color[] colors, IList<string> names)
        {
            var sb = new StringBuilder();
            sb.Append("GIMP Palette\nName: Dragon Map Studio\nColumns: 16\n#\n");
            for (int i = 0; i < 256; i++)
            {
                Color c = i < colors.Length ? colors[i] : Color.Black;
                string name = $"{i:x2} " + (names != null && i < names.Count ? names[i] : string.Empty);
                sb.Append(CultureInfo.InvariantCulture, $"{c.R,3} {c.G,3} {c.B,3}\t{name}\n");
            }
            return sb.ToString();
        }

        private static int Clamp(string s)
        {
            int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v);
            return Math.Max(0, Math.Min(255, v));
        }
    }
}
