using Microsoft.VisualBasic.CompilerServices;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;

namespace Elevation
{
	public class ClsElevation
	{
		private int m_Key;

		private Color m_AltColor;

		private string m_Type;

		private short m_Alt;

		[Category("ID")]
		public Color AltitudeColor
		{
			get
			{
				return this.m_AltColor;
			}
			set
			{
				this.m_AltColor = value;
			}
		}

		[Category("Method")]
		public short GetAltitude
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

		[Category("Key")]
		public int Key
		{
			get
			{
				return this.m_Key;
			}
			set
			{
				this.m_Key = value;
			}
		}

		[Category("ID")]
		public string Type
		{
			get
			{
				return this.m_Type;
			}
			set
			{
				this.m_Type = value;
			}
		}

		public ClsElevation(int iKey, string iType, short iAlt, Color iAltColor)
		{
			this.m_Key = iKey;
			this.m_Type = iType;
			this.m_Alt = iAlt;
			this.m_AltColor = iAltColor;
		}

		public ClsElevation(XmlElement xmlInfo)
		{
			this.m_Key = XmlConvert.ToInt32(xmlInfo.GetAttribute("Key"));
			this.m_Type = xmlInfo.GetAttribute("Type");
			this.m_Alt = XmlConvert.ToInt16(xmlInfo.GetAttribute("Altitude"));
			this.m_AltColor = Color.FromArgb((int)XmlConvert.ToByte(xmlInfo.GetAttribute("R")), (int)XmlConvert.ToByte(xmlInfo.GetAttribute("G")), (int)XmlConvert.ToByte(xmlInfo.GetAttribute("B")));
		}

		public void Save(XmlTextWriter xmlInfo)
		{
			xmlInfo.WriteStartElement("Altitude");
			xmlInfo.WriteAttributeString("Key", StringType.FromInteger(this.m_Key));
			xmlInfo.WriteAttributeString("Type", this.m_Type);
			xmlInfo.WriteAttributeString("Altitude", StringType.FromInteger(this.m_Alt));
			xmlInfo.WriteAttributeString("R", StringType.FromByte(this.m_AltColor.R));
			xmlInfo.WriteAttributeString("G", StringType.FromByte(this.m_AltColor.G));
			xmlInfo.WriteAttributeString("B", StringType.FromByte(this.m_AltColor.B));
			xmlInfo.WriteEndElement();
		}

		public void SaveACO(BinaryWriter iACTFile)
		{
			byte num = 0;
			iACTFile.Write(this.m_AltColor.R);
			iACTFile.Write(this.m_AltColor.R);
			iACTFile.Write(this.m_AltColor.G);
			iACTFile.Write(this.m_AltColor.G);
			iACTFile.Write(this.m_AltColor.B);
			iACTFile.Write(this.m_AltColor.B);
			iACTFile.Write(num);
			iACTFile.Write(num);
		}

		public void SaveACOText(BinaryWriter iACTFile)
		{
			byte num = 0;
			iACTFile.Write(this.m_AltColor.R);
			iACTFile.Write(this.m_AltColor.R);
			iACTFile.Write(this.m_AltColor.G);
			iACTFile.Write(this.m_AltColor.G);
			iACTFile.Write(this.m_AltColor.B);
			iACTFile.Write(this.m_AltColor.B);
			iACTFile.Write(num);
			iACTFile.Write(num);
			iACTFile.Write(num);
			iACTFile.Write(num);
			UnicodeEncoding unicodeEncoding = new UnicodeEncoding(true, true);
			string str = string.Format("{0} {1}", this.m_Type, this.m_Alt);
			byte[] bytes = unicodeEncoding.GetBytes(str);
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
			iACTFile.Write(this.m_AltColor.R);
			iACTFile.Write(this.m_AltColor.G);
			iACTFile.Write(this.m_AltColor.B);
		}

		public override string ToString()
		{
			string str = string.Format("[{0:X3}] {1} {2}", this.m_Key, this.m_Type, this.m_Alt);
			return str;
		}
	}
}