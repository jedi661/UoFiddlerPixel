using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Controls.Models.Uop.Imaging
{
    internal class OctreeNode
    {
        private static readonly Byte[] Mask = new Byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        private int red;
        private int green;
        private int blue;

        private int pixelCount;
        private int paletteIndex;

        private readonly OctreeNode[] nodes;

        public OctreeNode(int level, OctreeQuantizer parent)
        {
            nodes = new OctreeNode[8];

            if (level < 7)
            {
                parent.AddLevelNode(level, this);
            }
        }

        public Boolean IsLeaf
        {
            get { return pixelCount > 0; }
        }

        public Color Color
        {
            get
            {
                Color result;
                if (IsLeaf)
                {
                    result = pixelCount == 1 ?
                        Color.FromArgb(255, red, green, blue) :
                        Color.FromArgb(255, red / pixelCount, green / pixelCount, blue / pixelCount);
                }
                else
                {
                    throw new InvalidOperationException("Cannot retrieve a color for other node than leaf.");
                }
                return result;
            }
        }

        public int ActiveNodesPixelCount
        {
            get
            {
                int result = pixelCount;
                for (int index = 0; index < 8; index++)
                {
                    OctreeNode node = nodes[index];
                    if (node != null)
                    {
                        result += node.pixelCount;
                    }
                }
                return result;
            }
        }

        public IEnumerable<OctreeNode> ActiveNodes
        {
            get
            {
                List<OctreeNode> result = new List<OctreeNode>();
                for (int index = 0; index < 8; index++)
                {
                    OctreeNode node = nodes[index];
                    if (node != null)
                    {
                        if (node.IsLeaf)
                        {
                            result.Add(node);
                        }
                        else
                        {
                            result.AddRange(node.ActiveNodes);
                        }
                    }
                }
                return result;
            }
        }

        public void AddColor(Color color, int level, OctreeQuantizer parent)
        {
            if (level == 8)
            {
                red += color.R;
                green += color.G;
                blue += color.B;
                pixelCount++;
            }
            else if (level < 8)
            {
                int index = GetColorIndexAtLevel(color, level);
                if (nodes[index] == null)
                {
                    nodes[index] = new OctreeNode(level, parent);
                }
                nodes[index].AddColor(color, level + 1, parent);
            }
        }

        public int GetPaletteIndex(Color color, int level)
        {
            int result;
            if (IsLeaf)
            {
                result = paletteIndex;
            }
            else
            {
                int index = GetColorIndexAtLevel(color, level);
                result = nodes[index] != null ? nodes[index].GetPaletteIndex(color, level + 1) : nodes.Where(node => node != null).First().GetPaletteIndex(color, level + 1);
            }
            return result;
        }

        public int RemoveLeaves()
        {
            int result = 0;
            for (int index = 0; index < 8; index++)
            {
                OctreeNode node = nodes[index];
                if (node != null)
                {
                    red += node.red;
                    green += node.green;
                    blue += node.blue;
                    pixelCount += node.pixelCount;
                    result++;
                }
            }
            return result - 1;
        }

        private static int GetColorIndexAtLevel(Color color, int level)
        {
            return ((color.R & Mask[level]) == Mask[level] ? 4 : 0) |
                   ((color.G & Mask[level]) == Mask[level] ? 2 : 0) |
                   ((color.B & Mask[level]) == Mask[level] ? 1 : 0);
        }

        internal void SetPaletteIndex(int index)
        {
            paletteIndex = index;
        }
    }
}
