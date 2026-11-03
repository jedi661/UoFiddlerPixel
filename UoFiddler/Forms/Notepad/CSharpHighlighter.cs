/***************************************************************************
 *
 * $Author: Nikodemus
 * Coder: Nikodemus
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace UoFiddler.Forms.Notepad
{
    /// <summary>
    /// Simple C# coloring for ServUO / RunUO scripts in code notes.
    /// </summary>
    internal static class CSharpHighlighter
    {
        private static readonly Color _keyword = Color.FromArgb(0, 0, 255);
        private static readonly Color _type = Color.FromArgb(43, 145, 175);
        private static readonly Color _string = Color.FromArgb(163, 21, 21);
        private static readonly Color _comment = Color.FromArgb(0, 128, 0);
        private static readonly Color _number = Color.FromArgb(9, 134, 88);
        private static readonly Color _preprocessor = Color.FromArgb(128, 128, 128);

        private static readonly Regex _keywords = new(
            @"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|var|virtual|void|volatile|while|async|await|get|set|value|partial)\b",
            RegexOptions.Compiled);

        private static readonly Regex _types = new(
            @"\b(Item|Mobile|BaseCreature|PlayerMobile|Container|Static|Serial|GenericReader|GenericWriter|Point3D|Map|TimeSpan|DateTime|Timer|CommandSystem|CommandEventArgs|AccessLevel|Constructable|CommandProperty|Gump|TargetFlags|Target|List|Dictionary|Utility|Console|Math)\b",
            RegexOptions.Compiled);

        private static readonly Regex _numbers = new(@"\b(0x[0-9A-Fa-f]+|\d+(\.\d+)?[fFdDmMuUlL]?)\b", RegexOptions.Compiled);
        private static readonly Regex _preproc = new(@"^\s*#.*$", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex _strings = new(@"@""(?:[^""]|"""")*""|""(?:\\.|[^""\\\n])*""|'(?:\\.|[^'\\\n])'", RegexOptions.Compiled);
        private static readonly Regex _comments = new(@"//[^\n]*|/\*[\s\S]*?\*/", RegexOptions.Compiled);

        public static void Highlight(RichTextBox box)
        {
            string text = box.Text;
            if (text.Length > 300_000)
            {
                return; // too large for live coloring
            }

            int selStart = box.SelectionStart, selLength = box.SelectionLength;
            RichEditNative.BeginUpdate(box, out IntPtr mask, out Point scroll);
            try
            {
                box.SelectAll();
                RichEditNative.SetTextColor(box, null);

                void Paint(Regex regex, Color color)
                {
                    foreach (Match m in regex.Matches(text))
                    {
                        box.Select(m.Index, m.Length);
                        RichEditNative.SetTextColor(box, color);
                    }
                }

                Paint(_keywords, _keyword);
                Paint(_types, _type);
                Paint(_numbers, _number);
                Paint(_preproc, _preprocessor);
                // strings and comments last: they win over keywords inside them
                Paint(_strings, _string);
                Paint(_comments, _comment);
            }
            finally
            {
                box.Select(selStart, selLength);
                RichEditNative.EndUpdate(box, mask, scroll);
            }
        }
    }
}
