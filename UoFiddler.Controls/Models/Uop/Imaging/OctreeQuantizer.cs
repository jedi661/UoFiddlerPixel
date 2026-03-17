using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Controls.Models.Uop.Imaging
{
    public class OctreeQuantizer
    {
        private OctreeNode root;
        private List<OctreeNode>[] levels;

        public OctreeQuantizer()
        {
            prepare();
        }

        public void Init()
        {
            levels = new List<OctreeNode>[7];
            for (Int32 level = 0; level < 7; level++)
            {
                levels[level] = new List<OctreeNode>();
            }
            root = new OctreeNode(0, this);
        }

        internal IEnumerable<OctreeNode> Leaves
        {
            get { return root.ActiveNodes.Where(node => node.IsLeaf); }
        }

        internal void AddLevelNode(int level, OctreeNode octreeNode)
        {
            levels[level].Add(octreeNode);
        }

        public void AddColor(Color color)
        {
            root.AddColor(color, 0, this);
        }
        private void prepare()
        {
            levels = new List<OctreeNode>[7];
            for (Int32 level = 0; level < 7; level++)
            {
                levels[level] = new List<OctreeNode>();
            }
            root = new OctreeNode(0, this);
        }

        public void Clear()
        {
            prepare();
        }

        public List<Color> GetPalette(int colorCount)
        {
            List<Color> result = new List<Color>();
            int leafCount = Leaves.Count();
            int paletteIndex = 0;

            for (int level = 6; level >= 0; level--)
            {
                if (levels[level].Count > 0)
                {
                    IEnumerable<OctreeNode> sortedNodeList = levels[level].OrderBy(node => node.ActiveNodesPixelCount);

                    foreach (OctreeNode node in sortedNodeList)
                    {
                        leafCount -= node.RemoveLeaves();
                        if (leafCount <= colorCount) break;
                    }

                    if (leafCount <= colorCount) break;
                    levels[level].Clear();
                }
            }

            foreach (OctreeNode node in Leaves.OrderByDescending(node => node.ActiveNodesPixelCount))
            {
                if (paletteIndex >= colorCount) break;

                if (node.IsLeaf)
                {
                    result.Add(node.Color);
                }
                
                node.SetPaletteIndex(paletteIndex++);
            }
            
            if (result.Count == 0)
            {
                throw new NotSupportedException("The Octree contains after the reduction 0 colors.");
            }

            return result;
        }

        public int GetPaletteIndex(Color color)
        {
           return root.GetPaletteIndex(color, 0);
        }
    }
}
