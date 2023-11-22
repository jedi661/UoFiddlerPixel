using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Xml;
namespace Transition
{
	public class RandomStatic
	{
        //private short m_TileID;
        private int m_TileID;
        private short m_XMod;
		private short m_YMod;
		private short m_ZMod;
		private int m_HueMod;
		public int TileID
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
		public short X
		{
			get
			{
				return this.m_XMod;
			}
			set
			{
				this.m_XMod = value;
			}
		}
		public short Y
		{
			get
			{
				return this.m_YMod;
			}
			set
			{
				this.m_YMod = value;
			}
		}
		public short Z
		{
			get
			{
				return this.m_ZMod;
			}
			set
			{
				this.m_ZMod = value;
			}
		}
		public int Hue
		{
			get
			{
				return this.m_HueMod;
			}
			set
			{
				this.m_HueMod = value;
			}
		}
		public RandomStatic()
		{
		}
        public RandomStatic(int iTileID, short iXMod, short iYMod, short iZMod, int iHueMod)  // Ändern Sie den Datentyp von short zu int für iTileID und iHueMod
        {
            this.m_TileID = iTileID;
            this.m_XMod = iXMod;
            this.m_YMod = iYMod;
            this.m_ZMod = iZMod;
            this.m_HueMod = iHueMod;
        }

        //Gestern
        /*public RandomStatic(XmlElement xmlInfo)
        {
            try
            {
                this.m_TileID = XmlConvert.ToInt32(xmlInfo.GetAttribute("TileID"));  // Ändern Sie ToInt16 zu ToInt32
                this.m_XMod = XmlConvert.ToInt16(xmlInfo.GetAttribute("X"));
                this.m_YMod = XmlConvert.ToInt16(xmlInfo.GetAttribute("Y"));
                this.m_ZMod = XmlConvert.ToInt16(xmlInfo.GetAttribute("Z"));
                this.m_HueMod = XmlConvert.ToInt32(xmlInfo.GetAttribute("Hue"));  // Ändern Sie ToInt16 zu ToInt32
            }
            catch (Exception expr_AC)
            {
                ProjectData.SetProjectError(expr_AC);
                Interaction.MsgBox(string.Format("Error\r\n{0}", xmlInfo.OuterXml), MsgBoxStyle.OkOnly, null);
                ProjectData.ClearProjectError();
            }
        }*/

        /*public RandomStatic(XmlElement xmlInfo)
        {
            try
            {
                this.m_TileID = Convert.ToInt32(xmlInfo.GetAttribute("TileID"), 16); // Änderung hier
                this.m_XMod = XmlConvert.ToInt16(xmlInfo.GetAttribute("X"));
                this.m_YMod = XmlConvert.ToInt16(xmlInfo.GetAttribute("Y"));
                this.m_ZMod = XmlConvert.ToInt16(xmlInfo.GetAttribute("Z"));
                this.m_HueMod = XmlConvert.ToInt32(xmlInfo.GetAttribute("Hue")); // Ändern Sie ToInt16 zu ToInt32
            }
            catch (Exception expr_AC)
            {
                ProjectData.SetProjectError(expr_AC);
                Interaction.MsgBox(string.Format("Error\r\n{0}", xmlInfo.OuterXml), MsgBoxStyle.OkOnly, null);
                ProjectData.ClearProjectError();
            }
        }*/

        public RandomStatic(XmlElement xmlInfo)
        {
            try
            {
                string tileID = xmlInfo.GetAttribute("TileID");
                if (tileID.StartsWith("0x") || tileID.StartsWith("&H"))
                {
                    this.m_TileID = Convert.ToInt32(tileID.Substring(2), 16); // Überspringen Sie das Präfix und konvertieren Sie von Hex
                }
                else
                {
                    this.m_TileID = Convert.ToInt32(tileID); // Konvertieren Sie von Dezimal
                }
                this.m_XMod = XmlConvert.ToInt16(xmlInfo.GetAttribute("X"));
                this.m_YMod = XmlConvert.ToInt16(xmlInfo.GetAttribute("Y"));
                this.m_ZMod = XmlConvert.ToInt16(xmlInfo.GetAttribute("Z"));
                this.m_HueMod = XmlConvert.ToInt32(xmlInfo.GetAttribute("Hue")); // Ändern Sie ToInt16 zu ToInt32
            }
            catch (Exception expr_AC)
            {
                ProjectData.SetProjectError(expr_AC);
                Interaction.MsgBox(string.Format("Error\r\n{0}", xmlInfo.OuterXml), MsgBoxStyle.OkOnly, null);
                ProjectData.ClearProjectError();
            }
        }


        public override string ToString()
		{
			return string.Format("Tile:{0:X4} X:{1} Y:{2} Z:{3} Hue:{4}", new object[]
			{
				this.m_TileID,
				this.m_XMod,
				this.m_YMod,
				this.m_ZMod,
				this.m_HueMod
			});
		}
		public void Save(XmlTextWriter xmlInfo)
		{
			xmlInfo.WriteStartElement("Static");
			xmlInfo.WriteAttributeString("TileID", StringType.FromInteger((int)this.m_TileID));
			xmlInfo.WriteAttributeString("X", StringType.FromInteger((int)this.m_XMod));
			xmlInfo.WriteAttributeString("Y", StringType.FromInteger((int)this.m_YMod));
			xmlInfo.WriteAttributeString("Z", StringType.FromInteger((int)this.m_ZMod));
			xmlInfo.WriteAttributeString("Hue", StringType.FromInteger((int)this.m_HueMod));
			xmlInfo.WriteEndElement();
		}
	}
}
