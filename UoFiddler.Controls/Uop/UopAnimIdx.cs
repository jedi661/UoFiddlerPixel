using System.Collections.Generic;
using UoFiddler.Controls.Models.Uop;

namespace UoFiddler.Controls.Uop
{
    /// <summary>
    /// Cache en mémoire pour les animations UOP, similaire à AnimIdx pour les MUL
    /// </summary>
    public class UopAnimIdx
    {
        public int AnimId { get; private set; }
        public int Action { get; private set; }
        public int Direction { get; private set; }

        // ✅ Cache des frames décodées, comme AnimIdx.Frames pour les MUL
        public List<DecodedUopFrame> Frames { get; private set; }

        // ✅ Indique si les données ont été modifiées
        public bool IsModified { get; set; }

        public UopBinHeader Header { get; internal set; }

        public UopAnimIdx(int animId, int action, int direction)
        {
            AnimId = animId;
            Action = action;
            Direction = direction;
            Frames = new List<DecodedUopFrame>();
            IsModified = false;
        }

        /// <summary>
        /// Charge les frames depuis les données UOP binaires
        /// </summary>
        public void LoadFrames(byte[] binData, int direction)
        {
            Frames.Clear();

            using (var stream = new System.IO.MemoryStream(binData))
            using (var reader = new System.IO.BinaryReader(stream))
            {
                var header = UopAnimationDataManager.ReadUopBinHeader(reader);
                Header = header; // Store header with bounds

                uint framesPerDirection = header.FrameCount / 5;
                if (framesPerDirection == 0 && header.FrameCount > 0) framesPerDirection = 1;

                for (int i = 0; i < framesPerDirection; i++)
                {
                    var frame = UopAnimationDataManager.LoadFromUopBin(binData, direction, i);
                    if (frame != null)
                    {
                        Frames.Add(frame);
                    }
                }
            }
        }

        /// <summary>
        /// Modifie le centre d'une frame (comme AnimIdx.Frames[i].ChangeCenter)
        /// </summary>
        public void ChangeCenter(int frameIndex, int centerX, int centerY)
        {
            if (frameIndex >= 0 && frameIndex < Frames.Count)
            {
                Frames[frameIndex].Header.CenterX = (short)centerX;
                Frames[frameIndex].Header.CenterY = (short)centerY;
                IsModified = true;
            }
        }
    }
}