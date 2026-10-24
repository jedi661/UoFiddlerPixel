using System.Xml;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    /// <summary>One entry of Data/System/MapInfo.xml.</summary>
    internal class MapInfo
    {
        public string MapName { get; }

        /// <summary>Map file number (map#.mul).</summary>
        public byte MapNumber { get; }

        public int XSize { get; }
        public int YSize { get; }

        /// <summary>X where the dungeon area starts, 0 = none.</summary>
        public int DungeonX { get; }

        /// <summary>Separator line in the list (XSize = 0).</summary>
        public bool IsSeparator => XSize == 0 || YSize == 0;

        /// <summary>"Custom size ..." entry (XSize = -1).</summary>
        public bool IsCustom => XSize < 0 || YSize < 0;

        public MapInfo(string name, byte number, int xSize, int ySize, int dungeonX)
        {
            MapName = name;
            MapNumber = number;
            XSize = xSize;
            YSize = ySize;
            DungeonX = dungeonX;
        }

        public MapInfo(XmlElement xml)
        {
            MapName = xml.GetAttribute("Name");
            byte.TryParse(xml.GetAttribute("Num"), out byte number);
            int.TryParse(xml.GetAttribute("XSize"), out int x);
            int.TryParse(xml.GetAttribute("YSize"), out int y);
            MapNumber = number;
            XSize = x;
            YSize = y;
            if (!int.TryParse(xml.GetAttribute("DungeonX"), out int dungeonX))
            {
                // old MapInfo.xml without DungeonX: Felucca/Trammel sized maps have the dungeon area at 5120
                dungeonX = x >= 6144 ? 5120 : 0;
            }

            DungeonX = dungeonX;
        }

        public override string ToString() => MapName;
    }
}
