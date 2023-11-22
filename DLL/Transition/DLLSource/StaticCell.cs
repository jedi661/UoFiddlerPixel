using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.IO;

namespace Transition
{
    // The StaticCell class represents a cell in a static map.
    public class StaticCell
    {
        // Private member variables for storing cell information
        private int m_TileID; // The ID of the cell's tile type
        private byte m_X; // The x coordinate of the cell
        private byte m_Y; // The Y coordinate of the cell
        private sbyte m_Z; // The Z coordinate (height) of the cell
        private int m_Hue; // The hue of the cell

        // Constructor that initializes the cell information
        public StaticCell(int iTileID, byte iX, byte iY, int iZ)
        {
            this.m_Hue = 0;
            this.m_TileID = iTileID;
            this.m_X = iX;
            this.m_Y = iY;
            this.m_Z = Convert.ToSByte(iZ);
        }

        // Overloaded constructor that additionally initializes the cell's hue
        public StaticCell(int iTileID, byte iX, byte iY, int iZ, int iHue)
        {
            this.m_Hue = 0;
            this.m_TileID = iTileID;
            this.m_X = iX;
            this.m_Y = iY;
            this.m_Z = Convert.ToSByte(iZ);
            this.m_Hue = iHue;
        }

        // Method of writing the cell information to a file
        public void Write(BinaryWriter i_StaticFile)
        {
            try
            {
                // Writing the cell information to the file
                i_StaticFile.Write(this.m_TileID);
                i_StaticFile.Write(this.m_X);
                i_StaticFile.Write(this.m_Y);
                i_StaticFile.Write(this.m_Z);
                i_StaticFile.Write(this.m_Hue);
            }
            catch (Exception expr_45)
            {
                ProjectData.SetProjectError(expr_45);
                // Display an error message if an error occurs while writing the cell information to the file
                Interaction.MsgBox(string.Format("Error [{0}] X:{1} Y:{2} Z:{3} Hue:{4}", new object[]
                {
                    this.m_TileID,
                    this.m_X,
                    this.m_Y,
                    this.m_Z,
                    this.m_Hue
                }), MsgBoxStyle.OkOnly, null);
                ProjectData.ClearProjectError();
            }
        }
    }
}
