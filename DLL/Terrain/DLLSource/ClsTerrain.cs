using Microsoft.VisualBasic.CompilerServices;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;

namespace Terrain
{
	public class ClsTerrain
	{
		private int m_GroupID;

		private string m_Name;

		private Color m_Color;

		private short m_TileID;

		private bool m_RandAlt;

		private byte m_BaseAlt;

		[Category("Tile ID")]
		public byte AltID
		{
			get
			{
				return this.m_BaseAlt;
			}
			set
			{
				this.m_BaseAlt = value;
			}
		}

		[Category("Colour")]
		public Color Colour
		{
			get
			{
				return this.m_Color;
			}
			set
			{
				this.m_Color = value;
			}
		}

		[Category("Key")]
		public int GroupID
		{
			get
			{
				return this.m_GroupID;
			}
			set
			{
				this.m_GroupID = value;
			}
		}

		[Category("Group ID")]
		public string GroupIDHex
		{
			get
			{
				return string.Format("{0:X}", this.m_GroupID);
			}
		}

		[Category("Description")]
		public string Name
		{
			get
			{
				return this.m_Name;
			}
			set
			{
				this.m_Name = value;
			}
		}

		[Category("Tile ID")]
		public bool RandAlt
		{
			get
			{
				return this.m_RandAlt;
			}
			set
			{
				this.m_RandAlt = value;
			}
		}

		[Category("Tile ID")]
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

		public ClsTerrain(string iName, int iGroupID, short iTileID, Color iColor, byte iBase, bool iRandAlt)
		{
			this.m_Name = iName;
			this.m_GroupID = iGroupID;
			this.m_TileID = iTileID;
			this.m_Color = iColor;
			this.m_BaseAlt = iBase;
			this.m_RandAlt = iRandAlt;
		}

		public ClsTerrain(XmlElement xmlInfo)
		{
			this.m_Name = xmlInfo.GetAttribute("Name");
			this.m_GroupID = XmlConvert.ToInt32(xmlInfo.GetAttribute("ID"));
			this.m_TileID = XmlConvert.ToInt16(xmlInfo.GetAttribute("TileID"));
			this.m_Color = Color.FromArgb((int)XmlConvert.ToByte(xmlInfo.GetAttribute("R")), (int)XmlConvert.ToByte(xmlInfo.GetAttribute("G")), (int)XmlConvert.ToByte(xmlInfo.GetAttribute("B")));
			this.m_BaseAlt = XmlConvert.ToByte(xmlInfo.GetAttribute("Base"));
			string attribute = xmlInfo.GetAttribute("Random");
			if (StringType.StrCmp(attribute, "False", false) == 0)
			{
				this.m_RandAlt = false;
			}
			else if (StringType.StrCmp(attribute, "True", false) == 0)
			{
				this.m_RandAlt = true;
			}
		}

		public void Save(XmlTextWriter xmlInfo)
		{
			xmlInfo.WriteStartElement("Terrain");
			xmlInfo.WriteAttributeString("Name", this.m_Name);
			xmlInfo.WriteAttributeString("ID", StringType.FromInteger(this.m_GroupID));
			xmlInfo.WriteAttributeString("TileID", StringType.FromInteger(this.m_TileID));
			xmlInfo.WriteAttributeString("R", StringType.FromByte(this.m_Color.R));
			xmlInfo.WriteAttributeString("G", StringType.FromByte(this.m_Color.G));
			xmlInfo.WriteAttributeString("B", StringType.FromByte(this.m_Color.B));
			xmlInfo.WriteAttributeString("Base", StringType.FromByte(this.m_BaseAlt));
			xmlInfo.WriteAttributeString("Random", StringType.FromBoolean(this.m_RandAlt));
			xmlInfo.WriteEndElement();
		}

		public void SaveACO(BinaryWriter iACTFile)
		{
			byte num = 0;
			iACTFile.Write(this.m_Color.R);
			iACTFile.Write(this.m_Color.R);
			iACTFile.Write(this.m_Color.G);
			iACTFile.Write(this.m_Color.G);
			iACTFile.Write(this.m_Color.B);
			iACTFile.Write(this.m_Color.B);
			iACTFile.Write(num);
			iACTFile.Write(num);
		}

		public void SaveACOText(BinaryWriter iACTFile)
		{
			byte num = 0;
			iACTFile.Write(this.m_Color.R);
			iACTFile.Write(this.m_Color.R);
			iACTFile.Write(this.m_Color.G);
			iACTFile.Write(this.m_Color.G);
			iACTFile.Write(this.m_Color.B);
			iACTFile.Write(this.m_Color.B);
			iACTFile.Write(num);
			iACTFile.Write(num);
			iACTFile.Write(num);
			iACTFile.Write(num);
			byte[] bytes = (new UnicodeEncoding(true, true)).GetBytes(this.m_Name);
			byte num1 = Convert.ToByte(bytes.Length);
			byte num2 = checked((byte)Math.Round((double)num1 / 2 + 1));
			iACTFile.Write(num);
			iACTFile.Write(num2);
			byte[] numArray = bytes;
			for (int i = 0; i < (int)numArray.Length; i++)
			{
				iACTFile.Write(numArray[i]);
			}
			iACTFile.Write(num);
			iACTFile.Write(num);
		}

		public void SaveACT(BinaryWriter iACTFile)
		{
			iACTFile.Write(this.m_Color.R);
			iACTFile.Write(this.m_Color.G);
			iACTFile.Write(this.m_Color.B);
		}

		public override string ToString()
		{
			string str;
			str = (!this.m_RandAlt ? string.Format("[{0:X2}] {1}", this.m_GroupID, this.m_Name) : string.Format("[{0:X2}] *{1}", this.m_GroupID, this.m_Name));
			return str;
		}
	}
}