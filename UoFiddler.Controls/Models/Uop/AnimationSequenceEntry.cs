using System;

namespace UoFiddler.Controls.Models.Uop
{
    public class AnimationSequenceEntry
    {
        public uint UopGroupIndex { get; set; }
        public int FrameCount { get; set; }
        public uint MulGroupIndex { get; set; }
        public float Speed { get; set; }
        // 56 bytes remaining in original 72-byte record.
        // We might want to preserve them if we want "lossless" editing, 
        // but for now the user only mentioned these columns.
        // To be safe, we could store the extra bytes.
        public byte[] ExtraData { get; set; }

        public AnimationSequenceEntry()
        {
            ExtraData = new byte[56]; 
        }
    }
}
