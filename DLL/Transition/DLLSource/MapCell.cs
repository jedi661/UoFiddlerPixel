using System;
using System.IO;
namespace Transition
{
	public class MapCell
	{
		private byte m_GroupID;
		private short m_TileID;
		private short m_Alt;
		public byte GroupID
		{
			get
			{
				return this.m_GroupID;
			}
		}
		public short TileID
		{
			get
			{
				return this.m_TileID;
			}
			set
			{
				this.m_TileID = value;
			}
		}
		public short AltID
		{
			get
			{
				return this.m_Alt;
			}
			set
			{
				this.m_Alt = value;
			}
		}
		public void ChangeAltID(short AltMOD)
		{
			this.m_Alt += AltMOD;
		}
		public MapCell(byte i_Terrian, short i_Alt)
		{
			this.m_GroupID = i_Terrian;
			this.m_TileID = 0;
			this.m_Alt = i_Alt;
		}
		public void WriteMapMul(BinaryWriter i_MapFile)
		{
			i_MapFile.Write(this.m_TileID);
			if (this.m_Alt <= -127)
			{
				this.m_Alt = -127;
			}
			if (this.m_Alt >= 127)
			{
				this.m_Alt = 127;
			}
			sbyte b = Convert.ToSByte(this.m_Alt);
			i_MapFile.Write(b);
		}
	}
}
