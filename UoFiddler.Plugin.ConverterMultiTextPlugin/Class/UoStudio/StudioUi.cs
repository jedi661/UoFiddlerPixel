/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio
{
    /// <summary>Small helpers used by all UO Studio forms.</summary>
    public static class StudioUi
    {
        public const string ImageFilter = "Bilder|*.png;*.bmp;*.gif;*.jpg;*.jpeg;*.tif;*.tiff|Alle Dateien|*.*";

        /// <summary>Output directory of UO Fiddler (falls back to the desktop).</summary>
        public static string OutputPath
        {
            get
            {
                string path = Options.OutputPath;
                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                }
                return path;
            }
        }

        /// <summary>Marks a Ultima class as changed so UO Fiddler asks to save on exit.</summary>
        public static void MarkChanged(string ultimaClass)
        {
            if (Options.ChangedUltimaClass.ContainsKey(ultimaClass))
            {
                Options.ChangedUltimaClass[ultimaClass] = true;
            }
        }

        /// <summary>Resets the changed flag after the files were written.</summary>
        public static void ResetChanged(string ultimaClass)
        {
            if (Options.ChangedUltimaClass.ContainsKey(ultimaClass))
            {
                Options.ChangedUltimaClass[ultimaClass] = false;
            }
        }

        public static void Info(IWin32Window owner, string text)
        {
            MessageBox.Show(owner, text, "UO Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Error(IWin32Window owner, string text)
        {
            MessageBox.Show(owner, text, "UO Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Ask(IWin32Window owner, string text)
        {
            return MessageBox.Show(owner, text, "UO Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>Parses decimal or hex ("0x1F") numbers.</summary>
        public static bool TryParseId(string text, out int value)
        {
            text = (text ?? string.Empty).Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out value);
            }
            return int.TryParse(text, out value);
        }

        /// <summary>Returns the hue (0 = no hue, 1.. = hues.mul index + 1) or null.</summary>
        public static Ultima.Hue GetHue(int hueId)
        {
            if (hueId <= 0)
            {
                return null;
            }
            try
            {
                var list = Ultima.Hues.List;
                if (list == null || hueId - 1 >= list.Length)
                {
                    return null;
                }
                return list[hueId - 1];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Renders the 32 colours of a hue as a small strip.</summary>
        public static Bitmap HueStrip(Ultima.Hue hue, int width, int height)
        {
            var bmp = new Bitmap(Math.Max(1, width), Math.Max(1, height));
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(SystemColors.Control);
            if (hue == null)
            {
                g.DrawString("kein Hue", SystemFonts.DefaultFont, Brushes.Gray, 2, 1);
                return bmp;
            }
            float w = width / 32f;
            for (int i = 0; i < 32; i++)
            {
                using var b = new SolidBrush(hue.GetColor(i));
                g.FillRectangle(b, i * w, 0, w + 1, height);
            }
            return bmp;
        }
    }
}
