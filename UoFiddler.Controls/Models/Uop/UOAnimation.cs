using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UoFiddler.Controls.Models.Uop.Imaging;
using UoFiddler.Controls.Uop;

namespace UoFiddler.Controls.Models.Uop
{
    public class UOAnimation : IDisposable
    {
        private uint m_ID; 
        private int m_ActionID;
        private short m_InitCoordsX;
        private short m_InitCoordsY;
        private short m_EndCoordsX;
        private short m_EndCoordsY;
        private uint m_FrameCount;
        private List<ColourEntry> m_Colours = new List<ColourEntry>();
        private List<FrameEntry> m_Frames = new List<FrameEntry>();
        private int m_width;
        private int m_height;
        
        public bool disposed = false;
        public uint BodyID => m_ID;
        public int FramesPerDirection => (int)m_FrameCount / 5;
        public List<FrameEntry> Frames => m_Frames;

        public UOAnimation(uint bodyId, int actionId, int width, int height, short initCoordsX, short initCoordsY, short endCoordsX, short endCoordsY, List<FrameEntry> frames, List<ColourEntry> colours, uint frameCount)
        {
            m_ID = bodyId;
            m_ActionID = actionId;
            m_width = width;
            m_height = height;
            m_InitCoordsX = initCoordsX;
            m_InitCoordsY = initCoordsY;
            m_EndCoordsX = endCoordsX;
            m_EndCoordsY = endCoordsY;
            m_Frames = frames;
            m_Colours = colours;
            m_FrameCount = frameCount;
        }

        public void Resize(double scale)
        {
            if (scale == 1.0) return;
            foreach (var frame in m_Frames)
            {
                frame.Resize(scale);
            }
            m_InitCoordsX = (short)(m_InitCoordsX * scale);
            m_InitCoordsY = (short)(m_InitCoordsY * scale);
            m_EndCoordsX = (short)(m_EndCoordsX * scale);
            m_EndCoordsY = (short)(m_EndCoordsY * scale);
        }
        
        public bool ExportAnimationToVD(BinaryWriter writer, int dir, ref long headerPos, ref long animPos)
        {
            writer.BaseStream.Seek(headerPos, SeekOrigin.Begin);
            writer.Write((int)animPos);
            headerPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(animPos, SeekOrigin.Begin);

            // CORRECTED: Generate palette from the first frame of the direction
            List<ColourEntry> paletteToUse;
            if (Frames.Any() && FramesPerDirection > 0 && Frames[dir * FramesPerDirection]?.Image != null)
            {
                paletteToUse = VdExportHelper.GenerateProperPaletteFromImage(
                    Frames[dir * FramesPerDirection].Image,
                    Frames[dir * FramesPerDirection].VDFrameColors
                );
            }
            else
            {
                paletteToUse = Enumerable.Range(0, 256).Select(i => new ColourEntry(0, 0, 0, 0)).ToList();
            }
            
            // Write the 256-color palette with the required XOR mask
            for (int i = 0; i < 256; i++)
            {
                writer.Write((ushort)(paletteToUse[i].ColorRGB555 ^ 0x8000));
            }

            long startPos = writer.BaseStream.Position;
            writer.Write(FramesPerDirection);
            long seek = writer.BaseStream.Position;
            long curr = writer.BaseStream.Position + (4 * FramesPerDirection);

            for (int f = dir * FramesPerDirection; f < (dir * FramesPerDirection) + FramesPerDirection; f++)
            {
                if (f >= Frames.Count || Frames[f] == null)
                {
                    writer.BaseStream.Seek(seek, SeekOrigin.Begin);
                    writer.Write((int)0);
                    seek = writer.BaseStream.Position;
                    writer.BaseStream.Seek(curr, SeekOrigin.Begin);
                    continue; 
                }

                FrameEntry currentFrame = Frames[f];
                writer.BaseStream.Seek(seek, SeekOrigin.Begin);
                writer.Write((int)(curr - startPos));
                seek = writer.BaseStream.Position;
                writer.BaseStream.Seek(curr, SeekOrigin.Begin);

                // CORRECTED: Use frame's InitCoords for calculation
                short frameImageCenterX = currentFrame.CenterX;
                short frameImageCenterY = currentFrame.CenterY;
                int frameTopX = Math.Abs((int)m_InitCoordsX - (int)currentFrame.InitCoordsX);
                int frameTopY = Math.Abs((int)m_InitCoordsY - (int)currentFrame.InitCoordsY);

                if (currentFrame.Image != null)
                {
                    currentFrame.ExportVDImageData(writer, currentFrame.Image, frameImageCenterX, frameImageCenterY, frameTopX, frameTopY, paletteToUse);
                }
                else
                {
                    writer.Write(0x7FFF7FFF);
                }
                curr = writer.BaseStream.Position;
            }

            long length = writer.BaseStream.Position - animPos;
            animPos = writer.BaseStream.Position;
            writer.BaseStream.Seek(headerPos, SeekOrigin.Begin);
            writer.Write((int)length);
            writer.Write((int)0);
            headerPos = writer.BaseStream.Position;

            return true;
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                foreach (var frame in m_Frames)
                {
                    frame.Dispose();
                }
                m_Frames.Clear();
                m_Colours.Clear();
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
