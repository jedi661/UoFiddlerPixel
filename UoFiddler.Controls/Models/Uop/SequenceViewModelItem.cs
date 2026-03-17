using System;

namespace UoFiddler.Controls.Models.Uop
{
    public class SequenceViewModelItem
    {
        private AnimationSequenceEntry _entry;

        public SequenceViewModelItem(AnimationSequenceEntry entry)
        {
            _entry = entry;
        }

        public AnimationSequenceEntry Entry => _entry;

        public uint UopGroupIndex 
        { 
            get => _entry.UopGroupIndex; 
            set => _entry.UopGroupIndex = value; 
        }

        public int FrameCount 
        { 
            get => _entry.FrameCount; 
            set => _entry.FrameCount = value; 
        }

        public uint MulGroupIndex 
        { 
            get => _entry.MulGroupIndex; 
            set => _entry.MulGroupIndex = value; 
        }

        public float Speed 
        { 
            get => _entry.Speed; 
            set => _entry.Speed = value; 
        }

        // Display-only columns
        public int BaseGroup => (FrameCount == 0) ? (int)MulGroupIndex : (int)UopGroupIndex;

        public int? PeaceGroupId { get; set; }
        public ushort? PeaceModifier { get; set; }
        public int? WarGroupId { get; set; }
        public ushort? WarModifier { get; set; }
        public int? MountPeaceGroupId { get; set; }
        public ushort? MountPeaceModifier { get; set; }
        public int? MountWarGroupId { get; set; }
        public ushort? MountWarModifier { get; set; }
    }
}
