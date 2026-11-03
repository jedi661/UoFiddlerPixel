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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UoFiddler.Forms.Notepad
{
    /// <summary>
    /// Native RichEdit formatting. Unlike SelectionFont these calls change only one attribute,
    /// so bold / size / color work on mixed selections without losing the other styles.
    /// </summary>
    internal static class RichEditNative
    {
        private const int WM_USER = 0x0400;
        private const int WM_SETREDRAW = 0x000B;
        private const int EM_GETCHARFORMAT = WM_USER + 58;
        private const int EM_SETCHARFORMAT = WM_USER + 68;
        private const int EM_GETPARAFORMAT = WM_USER + 61;
        private const int EM_SETPARAFORMAT = WM_USER + 71;
        private const int EM_FORMATRANGE = WM_USER + 57;
        private const int EM_GETEVENTMASK = WM_USER + 59;
        private const int EM_SETEVENTMASK = WM_USER + 69;
        private const int EM_GETSCROLLPOS = WM_USER + 221;
        private const int EM_SETSCROLLPOS = WM_USER + 222;
        private const int SCF_SELECTION = 1;

        public const uint CFM_BOLD = 0x1;
        public const uint CFM_ITALIC = 0x2;
        public const uint CFM_UNDERLINE = 0x4;
        public const uint CFM_STRIKEOUT = 0x8;
        public const uint CFM_BACKCOLOR = 0x04000000;
        public const uint CFM_FACE = 0x20000000;
        public const uint CFM_COLOR = 0x40000000;
        public const uint CFM_SIZE = 0x80000000;
        public const uint CFE_AUTOCOLOR = 0x40000000;
        public const uint CFE_AUTOBACKCOLOR = 0x04000000;

        private const uint PFM_STARTINDENT = 0x1;
        private const uint PFM_OFFSET = 0x4;
        private const uint PFM_NUMBERING = 0x20;
        private const uint PFM_SPACEAFTER = 0x80;
        private const uint PFM_LINESPACING = 0x100;
        private const uint PFM_NUMBERINGSTYLE = 0x2000;
        private const uint PFM_NUMBERINGTAB = 0x4000;
        private const uint PFM_NUMBERINGSTART = 0x8000;

        public const short PFN_BULLET = 1;
        public const short PFN_ARABIC = 2;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CHARFORMAT2
        {
            public int cbSize;
            public uint dwMask;
            public uint dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szFaceName;
            public short wWeight;
            public short sSpacing;
            public int crBackColor;
            public int lcid;
            public int dwReserved;
            public short sStyle;
            public short wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bReserved1;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PARAFORMAT2
        {
            public int cbSize;
            public uint dwMask;
            public short wNumbering;
            public short wEffects;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public short wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;
            public int dySpaceBefore;
            public int dySpaceAfter;
            public int dyLineSpacing;
            public short sStyle;
            public byte bLineSpacingRule;
            public byte bOutlineLevel;
            public short wShadingWeight;
            public short wShadingStyle;
            public short wNumberingStart;
            public short wNumberingStyle;
            public short wNumberingTab;
            public short wBorderSpace;
            public short wBorderWidth;
            public short wBorders;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CHARRANGE
        {
            public int cpMin, cpMax;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FORMATRANGE
        {
            public IntPtr hdc;
            public IntPtr hdcTarget;
            public RECT rc;
            public RECT rcPage;
            public CHARRANGE chrg;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X, Y;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref CHARFORMAT2 lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref PARAFORMAT2 lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref FORMATRANGE lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref POINT lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static CHARFORMAT2 NewCharFormat() => new() { cbSize = Marshal.SizeOf<CHARFORMAT2>(), szFaceName = "" };

        private static PARAFORMAT2 NewParaFormat() => new() { cbSize = Marshal.SizeOf<PARAFORMAT2>(), rgxTabs = new int[32] };

        #region Character format

        public static CHARFORMAT2 GetCharFormat(RichTextBox box)
        {
            CHARFORMAT2 cf = NewCharFormat();
            SendMessage(box.Handle, EM_GETCHARFORMAT, (IntPtr)SCF_SELECTION, ref cf);
            return cf;
        }

        /// <summary>
        /// True when the whole selection has the effect (bold, italic, …).
        /// </summary>
        public static bool HasEffect(RichTextBox box, uint flag)
        {
            CHARFORMAT2 cf = GetCharFormat(box);
            return (cf.dwMask & flag) != 0 && (cf.dwEffects & flag) != 0;
        }

        public static void SetEffect(RichTextBox box, uint flag, bool on)
        {
            CHARFORMAT2 cf = NewCharFormat();
            cf.dwMask = flag;
            cf.dwEffects = on ? flag : 0;
            SendMessage(box.Handle, EM_SETCHARFORMAT, (IntPtr)SCF_SELECTION, ref cf);
        }

        public static void ToggleEffect(RichTextBox box, uint flag) => SetEffect(box, flag, !HasEffect(box, flag));

        public static void SetFontFace(RichTextBox box, string face)
        {
            CHARFORMAT2 cf = NewCharFormat();
            cf.dwMask = CFM_FACE;
            cf.szFaceName = face;
            SendMessage(box.Handle, EM_SETCHARFORMAT, (IntPtr)SCF_SELECTION, ref cf);
        }

        public static void SetFontSize(RichTextBox box, float points)
        {
            CHARFORMAT2 cf = NewCharFormat();
            cf.dwMask = CFM_SIZE;
            cf.yHeight = (int)Math.Round(points * 20);
            SendMessage(box.Handle, EM_SETCHARFORMAT, (IntPtr)SCF_SELECTION, ref cf);
        }

        public static void SetTextColor(RichTextBox box, Color? color)
        {
            CHARFORMAT2 cf = NewCharFormat();
            cf.dwMask = CFM_COLOR;
            if (color.HasValue)
            {
                cf.crTextColor = ColorTranslator.ToWin32(color.Value);
            }
            else
            {
                cf.dwEffects = CFE_AUTOCOLOR;
            }

            SendMessage(box.Handle, EM_SETCHARFORMAT, (IntPtr)SCF_SELECTION, ref cf);
        }

        public static void SetBackColor(RichTextBox box, Color? color)
        {
            CHARFORMAT2 cf = NewCharFormat();
            cf.dwMask = CFM_BACKCOLOR;
            if (color.HasValue)
            {
                cf.crBackColor = ColorTranslator.ToWin32(color.Value);
            }
            else
            {
                cf.dwEffects = CFE_AUTOBACKCOLOR;
            }

            SendMessage(box.Handle, EM_SETCHARFORMAT, (IntPtr)SCF_SELECTION, ref cf);
        }

        /// <summary>
        /// Font face and size of the selection, or null / 0 when mixed.
        /// </summary>
        public static (string Face, float Size) GetFont(RichTextBox box)
        {
            CHARFORMAT2 cf = GetCharFormat(box);
            string face = (cf.dwMask & CFM_FACE) != 0 ? cf.szFaceName : null;
            float size = (cf.dwMask & CFM_SIZE) != 0 ? cf.yHeight / 20f : 0f;
            return (face, size);
        }

        #endregion

        #region Paragraph format

        public static short GetNumbering(RichTextBox box)
        {
            PARAFORMAT2 pf = NewParaFormat();
            SendMessage(box.Handle, EM_GETPARAFORMAT, IntPtr.Zero, ref pf);
            return pf.wNumbering;
        }

        /// <summary>
        /// 0 = none, PFN_BULLET, PFN_ARABIC (1. 2. 3.)
        /// </summary>
        public static void SetNumbering(RichTextBox box, short numbering)
        {
            PARAFORMAT2 pf = NewParaFormat();
            pf.dwMask = PFM_NUMBERING | PFM_NUMBERINGSTYLE | PFM_NUMBERINGTAB | PFM_NUMBERINGSTART | PFM_OFFSET | PFM_STARTINDENT;
            pf.wNumbering = numbering;
            pf.wNumberingStyle = 0x200; // PFNS_PERIOD: "1."
            pf.wNumberingStart = 1;
            pf.wNumberingTab = numbering == 0 ? (short)0 : (short)360;
            pf.dxOffset = numbering == 0 ? 0 : 360;
            pf.dxStartIndent = numbering == 0 ? 0 : 200;
            SendMessage(box.Handle, EM_SETPARAFORMAT, IntPtr.Zero, ref pf);
        }

        /// <summary>
        /// rule: 0 = single, 1 = 1.5 lines, 2 = double
        /// </summary>
        public static void SetLineSpacing(RichTextBox box, byte rule)
        {
            PARAFORMAT2 pf = NewParaFormat();
            pf.dwMask = PFM_LINESPACING;
            pf.bLineSpacingRule = rule;
            SendMessage(box.Handle, EM_SETPARAFORMAT, IntPtr.Zero, ref pf);
        }

        public static void SetSpaceAfter(RichTextBox box, int twips)
        {
            PARAFORMAT2 pf = NewParaFormat();
            pf.dwMask = PFM_SPACEAFTER;
            pf.dySpaceAfter = twips;
            SendMessage(box.Handle, EM_SETPARAFORMAT, IntPtr.Zero, ref pf);
        }

        #endregion

        #region Redraw / events / scroll

        public static void BeginUpdate(RichTextBox box, out IntPtr eventMask, out Point scroll)
        {
            var p = new POINT();
            SendMessage(box.Handle, EM_GETSCROLLPOS, IntPtr.Zero, ref p);
            scroll = new Point(p.X, p.Y);
            SendMessage(box.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            eventMask = SendMessage(box.Handle, EM_SETEVENTMASK, IntPtr.Zero, IntPtr.Zero);
        }

        public static void EndUpdate(RichTextBox box, IntPtr eventMask, Point scroll)
        {
            var p = new POINT { X = scroll.X, Y = scroll.Y };
            SendMessage(box.Handle, EM_SETSCROLLPOS, IntPtr.Zero, ref p);
            SendMessage(box.Handle, EM_SETEVENTMASK, IntPtr.Zero, eventMask);
            SendMessage(box.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            box.Invalidate();
        }

        #endregion

        #region Printing

        /// <summary>
        /// Renders the text from charFrom into the rectangle (in 1/100 inch) of the graphics.
        /// Returns the index of the first character that did not fit.
        /// </summary>
        public static int FormatRange(RichTextBox box, Graphics g, Rectangle bounds, Rectangle page, int charFrom, int charTo, bool measureOnly)
        {
            // 1/100 inch -> twips
            static int Twips(int v) => (int)(v * 14.4);

            IntPtr hdc = g.GetHdc();
            try
            {
                var fr = new FORMATRANGE
                {
                    hdc = hdc,
                    hdcTarget = hdc,
                    rc = new RECT { Left = Twips(bounds.Left), Top = Twips(bounds.Top), Right = Twips(bounds.Right), Bottom = Twips(bounds.Bottom) },
                    rcPage = new RECT { Left = Twips(page.Left), Top = Twips(page.Top), Right = Twips(page.Right), Bottom = Twips(page.Bottom) },
                    chrg = new CHARRANGE { cpMin = charFrom, cpMax = charTo }
                };

                IntPtr result = SendMessage(box.Handle, EM_FORMATRANGE, (IntPtr)(measureOnly ? 0 : 1), ref fr);
                return result.ToInt32();
            }
            finally
            {
                g.ReleaseHdc(hdc);
            }
        }

        public static void FormatRangeDone(RichTextBox box) => SendMessage(box.Handle, EM_FORMATRANGE, IntPtr.Zero, IntPtr.Zero);

        #endregion
    }
}
