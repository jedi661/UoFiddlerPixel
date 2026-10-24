// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    /// <summary>
    /// Altitude adjustments for "rough edge" tiles (Data/System/RoughEdge/Corner|Left|Top.xml).
    /// </summary>
    internal class RoughEdge
    {
        private readonly HashSet<short> _cornerEdge = new HashSet<short>();
        private readonly HashSet<short> _leftEdge = new HashSet<short>();
        private readonly HashSet<short> _topEdge = new HashSet<short>();
        private readonly Random _random;

        /// <summary>Problems while loading (missing files ...). Empty when everything was fine.</summary>
        public List<string> Errors { get; } = new List<string>();

        public RoughEdge()
            : this(new Random())
        {
        }

        public RoughEdge(Random random)
        {
            _random = random ?? new Random();
            Load("Corner.xml", "//Terrains/Corner", _cornerEdge);
            Load("Left.xml", "//Terrains/Left", _leftEdge);
            Load("Top.xml", "//Terrains/Top", _topEdge);
        }

        private void Load(string file, string xpath, HashSet<short> target)
        {
            string path = Path.Combine(MapDataPaths.RoughEdgeDir, file);
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(path);
                foreach (XmlElement element in xml.SelectNodes(xpath))
                {
                    // HashSet: duplicate entries no longer throw
                    target.Add(XmlConvert.ToInt16(element.GetAttribute("TileID")));
                }
            }
            catch (Exception ex)
            {
                Errors.Add(string.Format("RoughEdge {0}: {1}", path, ex.Message));
            }
        }

        public short CheckCorner(short tileId)
        {
            return _cornerEdge.Contains(tileId) ? (short)-5 : (short)0;
        }

        public short CheckLeft(short tileId)
        {
            return _leftEdge.Contains(tileId) ? RandomEdge() : (short)0;
        }

        public short CheckTop(short tileId)
        {
            return _topEdge.Contains(tileId) ? RandomEdge() : (short)0;
        }

        /// <summary>
        /// Same distribution the original tool intended (0 => -4, 1-3 => -3, 4-8 => -2, 9 => -1, 10 => 0,
        /// 11-13 => +1, 14 => +2, 15 => +3). The old port compared a float with whole numbers,
        /// so most random values matched no branch at all.
        /// </summary>
        private short RandomEdge()
        {
            int value;
            lock (_random)
            {
                value = _random.Next(16);
            }

            if (value == 0)
            {
                return -4;
            }

            if (value <= 3)
            {
                return -3;
            }

            if (value <= 8)
            {
                return -2;
            }

            if (value == 9)
            {
                return -1;
            }

            if (value == 10)
            {
                return 0;
            }

            if (value <= 13)
            {
                return 1;
            }

            return value == 14 ? (short)2 : (short)3;
        }
    }
}
