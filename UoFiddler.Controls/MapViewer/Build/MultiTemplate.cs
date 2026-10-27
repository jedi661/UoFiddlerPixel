/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace UoFiddler.Controls.MapViewer.Build
{
    /// <summary>
    /// One part of a building, coordinates relative to the origin of the template
    /// </summary>
    public struct MultiComponent : IEquatable<MultiComponent>
    {
        public MultiComponent(ushort itemId, int x, int y, int z, ushort hue = 0, int flags = 1, int level = 0)
        {
            ItemId = itemId;
            X = x;
            Y = y;
            Z = z;
            Hue = hue;
            Flags = flags;
            Level = level;
        }

        public ushort ItemId;
        public int X;
        public int Y;
        public int Z;
        public ushort Hue;

        /// <summary>multi.mul flag: 0 = invisible part (e.g. dynamic items placed by the server), otherwise visible</summary>
        public int Flags;

        /// <summary>floor/level (UO Architect)</summary>
        public int Level;

        public bool IsVisible => Flags != 0;

        public bool Equals(MultiComponent other)
        {
            return ItemId == other.ItemId && X == other.X && Y == other.Y && Z == other.Z && Hue == other.Hue;
        }

        public override bool Equals(object obj) => obj is MultiComponent other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(ItemId, X, Y, Z, Hue);
    }

    /// <summary>
    /// Building template independent of the file format (TXT, UOA, multi.mul, library, area of the map ...)
    /// </summary>
    public sealed class MultiTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        /// <summary>Where it came from (file format, multi.mul id, library ...)</summary>
        public string Source { get; set; } = string.Empty;

        public List<MultiComponent> Components { get; } = new List<MultiComponent>();

        public int MinX => Components.Count == 0 ? 0 : Components.Min(c => c.X);
        public int MaxX => Components.Count == 0 ? 0 : Components.Max(c => c.X);
        public int MinY => Components.Count == 0 ? 0 : Components.Min(c => c.Y);
        public int MaxY => Components.Count == 0 ? 0 : Components.Max(c => c.Y);
        public int MinZ => Components.Count == 0 ? 0 : Components.Min(c => c.Z);
        public int MaxZ => Components.Count == 0 ? 0 : Components.Max(c => c.Z);
        public int Width => Components.Count == 0 ? 0 : MaxX - MinX + 1;
        public int Height => Components.Count == 0 ? 0 : MaxY - MinY + 1;

        public bool HasHues => Components.Any(c => c.Hue != 0);

        public bool HasInvisibleParts => Components.Any(c => !c.IsVisible);

        /// <summary>Moves all parts so that the lowest one is at z 0</summary>
        public MultiTemplate NormalizeZ()
        {
            int min = MinZ;
            return Transform((x, y, z) => (x, y, z - min));
        }

        /// <summary>Moves all parts so that the upper left corner is at 0,0</summary>
        public MultiTemplate NormalizeXY()
        {
            int minX = MinX, minY = MinY;
            return Transform((x, y, z) => (x - minX, y - minY, z));
        }

        /// <summary>Removes exact duplicates (same id, position and hue)</summary>
        public MultiTemplate RemoveDuplicates()
        {
            MultiTemplate copy = CloneEmpty();
            copy.Components.AddRange(Components.Distinct());
            return copy;
        }

        public MultiTemplate Transform(Func<int, int, int, (int x, int y, int z)> f)
        {
            MultiTemplate copy = CloneEmpty();
            foreach (MultiComponent c in Components)
            {
                (int x, int y, int z) = f(c.X, c.Y, c.Z);
                MultiComponent n = c;
                n.X = x;
                n.Y = y;
                n.Z = z;
                copy.Components.Add(n);
            }

            return copy;
        }

        public MultiTemplate Clone()
        {
            MultiTemplate copy = CloneEmpty();
            copy.Components.AddRange(Components);
            return copy;
        }

        private MultiTemplate CloneEmpty()
        {
            return new MultiTemplate { Name = Name, Author = Author, Category = Category, Source = Source };
        }

        public override string ToString()
        {
            return $"{Name} [{Source}] {Components.Count} parts, {Width}x{Height}, z {MinZ}..{MaxZ}";
        }
    }
}
