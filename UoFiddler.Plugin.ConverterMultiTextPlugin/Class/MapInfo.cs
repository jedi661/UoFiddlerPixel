using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    internal class MapInfo
    {
        private string m_Name; // Holds the name of the map
        private byte m_Num; // Holds the number of the map
        private int m_XSize; // Holds the size of the map in the X direction
        private int m_YSize; // Holds the size of the map in the Y direction

        // Public properties to access the private member variables
        public string MapName // Property to get the name of the map
        {
            get
            {
                return this.m_Name;
            }
        }

        public byte MapNumber // Property to get the number of the map
        {
            get
            {
                return this.m_Num;
            }
        }

        public int XSize // Property to get the size of the map in the X direction
        {
            get
            {
                return this.m_XSize;
            }
        }

        public int YSize // Property to get the size of the map in the Y direction
        {
            get
            {
                return this.m_YSize;
            }
        }

        // Constructor that takes an XmlElement and initializes the member variables
        public MapInfo(XmlElement iXml)
        {
            this.m_Name = iXml.GetAttribute("Name"); // Get the name attribute
            this.m_Num = ByteType.FromString(iXml.GetAttribute("Num")); // Get the number attribute
            this.m_XSize = IntegerType.FromString(iXml.GetAttribute("XSize")); // Get the X size attribute
            this.m_YSize = IntegerType.FromString(iXml.GetAttribute("YSize")); // Get the Y size attribute
        }

        // Override of the ToString method to return the name of the map
        public override string ToString()
        {
            return string.Format("{0}", this.m_Name);
        }

    }
}
