// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public class HexRegion
    {
        public long Offset { get; set; }
        public long Length { get; set; }
        public Color HighlightColor { get; set; } = Color.FromArgb(60, 100, 200, 255);
        public string Label { get; set; }
        public string Tooltip { get; set; }
        public bool IsSequenceStart { get; set; }
        public int SequenceIndex { get; set; }
        public int DirectionIndex { get; set; }
        public int FrameIndex { get; set; }
        public Bitmap PreviewImage { get; set; }
    }

    public class UopRawAnimationBlock
    {
        public byte[] RawData { get; set; }
        public long FileOffset { get; set; }
        public string FilePath { get; set; }
        public List<UopSequenceOffset> SequenceOffsets { get; set; } = new();
    }

    public class UopSequenceOffset
    {
        public int SequenceIndex { get; set; }
        public int DirectionIndex { get; set; }
        public long Offset { get; set; }
        public long Length { get; set; }
        public int FrameCount { get; set; }
    }

    public class HexPanel : Panel
    {
        public HexPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);

            UpdateStyles();
            TabStop = true;
        }
    }
}

