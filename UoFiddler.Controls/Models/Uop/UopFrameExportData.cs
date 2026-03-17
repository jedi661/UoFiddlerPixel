using System.Collections.Generic;
using UoFiddler.Controls.Models.Uop.Imaging;

namespace UoFiddler.Controls.Models.Uop
{
    public class UopFrameExportData
    {
        public DirectBitmap Image { get; set; }
        public List<ColourEntry> Palette { get; set; }
        public short CenterX { get; set; }
        public short CenterY { get; set; }
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public short InitCoordsX { get; set; }
        public short InitCoordsY { get; set; }
        public short EndCoordsX { get; set; }
        public short EndCoordsY { get; set; }
        public ushort ID { get; set; }
        public ushort Frame { get; set; }
        public uint DataOffset { get; set; }
    }
}
