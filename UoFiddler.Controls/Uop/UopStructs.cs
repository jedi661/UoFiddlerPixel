using System.Collections.Generic;
using System.Drawing;

namespace UoFiddler.Controls.Uop
{
    public class UopAnimationGlobalHeader
    {
        public uint AnimationId { get; set; }
        public uint FrameCount { get; set; }
        public short InitCoordsX { get; set; }
        public short InitCoordsY { get; set; }
        public short EndCoordsX { get; set; }
        public short EndCoordsY { get; set; }
        public int CellWidth { get; set; }
        public int CellHeight { get; set; }
    }

    public class UopBinHeader
    {
        public uint Magic;
        public uint Version;
        public uint TotalSize;
        public uint AnimationId;
        public short BoundLeft;
        public short BoundTop;
        public short BoundRight;
        public short BoundBottom;
        public uint FrameCount;
        public uint FrameIndexOffset;
    }

    public class UopFrameIndex
    {
        public ushort Direction;
        public ushort FrameNumber;
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
        public uint FrameDataOffset;
        public long StreamPosition;
    }

    public class UopFrameHeader
    {
        public short CenterX;
        public short CenterY;
        public ushort Width;
        public ushort Height;
    }

    public class DecodedUopFrame
    {
        public UopFrameHeader Header { get; set; } = new UopFrameHeader();
        public UopFrameIndex IndexInfo { get; set; }
        public List<Color> Palette { get; set; } = new List<Color>();
        public Bitmap Image { get; set; }
    }
}
