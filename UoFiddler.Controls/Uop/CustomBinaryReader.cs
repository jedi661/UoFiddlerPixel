using System.IO;
using System.Text;

namespace UoFiddler.Controls.Uop
{
    public class CustomBinaryReader : BinaryReader
    {
        public CustomBinaryReader(Stream input) : base(input, Encoding.ASCII, false) { }

        public uint ReadUInt32LE()
        {
            return ReadUInt32();
        }

        public int ReadInt32LE()
        {
            return ReadInt32();
        }

        public void Move(long count)
        {
            BaseStream.Seek(count, SeekOrigin.Current);
        }
    }
}
