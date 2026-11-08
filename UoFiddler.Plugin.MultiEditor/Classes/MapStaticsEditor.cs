/***************************************************************************
 *
 * MultiEditor – add / remove map statics in memory (block overrides) with
 * undo. Saving is done with Map.DefragStatics (statics*.mul / staidx*.mul).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// A static on the map (world coordinates)
    /// </summary>
    public readonly struct MapStatic
    {
        public MapStatic(int x, int y, HuedTile tile)
        {
            X = x;
            Y = y;
            Id = tile.Id;
            Z = tile.Z;
            Hue = tile.Hue;
        }

        public MapStatic(int x, int y, ushort id, int z, int hue)
        {
            X = x;
            Y = y;
            Id = id;
            Z = z;
            Hue = hue;
        }

        public int X { get; }
        public int Y { get; }
        public ushort Id { get; }
        public int Z { get; }
        public int Hue { get; }
    }

    public sealed class MapStaticsEditor
    {
        private sealed class BlockState
        {
            public Map Map;
            public int BlockX;
            public int BlockY;
            public StaticTile[] PreviousOverride; // null = there was no override
        }

        private readonly List<(string action, List<BlockState> blocks)> _undo = new List<(string, List<BlockState>)>();

        public bool CanUndo => _undo.Count > 0;

        /// <summary>
        /// Raised after statics were added, removed or restored
        /// </summary>
        public event Action Changed;

        public string UndoAction => _undo.Count > 0 ? _undo[_undo.Count - 1].action : string.Empty;

        /// <summary>
        /// Maps with unsaved changes
        /// </summary>
        public HashSet<Map> ChangedMaps { get; } = new HashSet<Map>();

        private static long Key(int bx, int by) => ((long)bx << 32) | (uint)by;

        private List<BlockState> Prepare(Map map, IEnumerable<(int bx, int by)> blocks)
        {
            var states = new List<BlockState>();
            foreach ((int bx, int by) in blocks.Distinct())
            {
                states.Add(new BlockState
                {
                    Map = map,
                    BlockX = bx,
                    BlockY = by,
                    PreviousOverride = map.Tiles.GetStaticBlockOverride(bx, by)?.ToArray()
                });
            }

            return states;
        }

        /// <summary>
        /// Adds statics (world coordinates). Returns number of added statics.
        /// </summary>
        public int Add(Map map, IList<MapStatic> statics, string action)
        {
            var valid = statics.Where(s => s.X >= 0 && s.Y >= 0 && s.X < map.Width && s.Y < map.Height).ToList();
            if (valid.Count == 0)
            {
                return 0;
            }

            var groups = valid.GroupBy(s => Key(s.X >> 3, s.Y >> 3)).ToList();
            _undo.Add((action, Prepare(map, valid.Select(s => (s.X >> 3, s.Y >> 3)))));

            foreach (var g in groups)
            {
                int bx = g.First().X >> 3;
                int by = g.First().Y >> 3;
                List<StaticTile> list = map.Tiles.GetStaticBlockTiles(bx, by);
                foreach (MapStatic s in g)
                {
                    list.Add(new StaticTile
                    {
                        Id = s.Id,
                        X = (byte)(s.X & 0x7),
                        Y = (byte)(s.Y & 0x7),
                        Z = (sbyte)Math.Max(-128, Math.Min(127, s.Z)),
                        Hue = (short)s.Hue
                    });
                }

                map.Tiles.SetStaticBlockOverride(bx, by, list.ToArray());
            }

            Finish(map);
            return valid.Count;
        }

        /// <summary>
        /// Removes statics (each entry once). Returns number of removed statics.
        /// </summary>
        public int Remove(Map map, IList<MapStatic> statics, string action)
        {
            if (statics.Count == 0)
            {
                return 0;
            }

            _undo.Add((action, Prepare(map, statics.Select(s => (s.X >> 3, s.Y >> 3)))));
            int removed = 0;
            foreach (var g in statics.GroupBy(s => Key(s.X >> 3, s.Y >> 3)))
            {
                int bx = g.First().X >> 3;
                int by = g.First().Y >> 3;
                List<StaticTile> list = map.Tiles.GetStaticBlockTiles(bx, by);
                foreach (MapStatic s in g)
                {
                    int idx = list.FindIndex(t => t.Id == s.Id && t.X == (s.X & 0x7) && t.Y == (s.Y & 0x7) && t.Z == s.Z && t.Hue == s.Hue);
                    if (idx < 0)
                    {
                        idx = list.FindIndex(t => t.Id == s.Id && t.X == (s.X & 0x7) && t.Y == (s.Y & 0x7) && t.Z == s.Z);
                    }

                    if (idx >= 0)
                    {
                        list.RemoveAt(idx);
                        ++removed;
                    }
                }

                map.Tiles.SetStaticBlockOverride(bx, by, list.ToArray());
            }

            Finish(map);
            return removed;
        }

        private void Finish(Map map)
        {
            ChangedMaps.Add(map);
            map.ResetCache();
            Changed?.Invoke();
            while (_undo.Count > 50)
            {
                _undo.RemoveAt(0);
            }
        }

        public bool Undo()
        {
            if (_undo.Count == 0)
            {
                return false;
            }

            List<BlockState> states = _undo[_undo.Count - 1].blocks;
            _undo.RemoveAt(_undo.Count - 1);
            foreach (BlockState b in states)
            {
                if (b.PreviousOverride == null)
                {
                    b.Map.Tiles.RevertStaticBlock(b.BlockX, b.BlockY);
                }
                else
                {
                    b.Map.Tiles.SetStaticBlockOverride(b.BlockX, b.BlockY, b.PreviousOverride);
                }

                b.Map.ResetCache();
            }

            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// Writes statics*.mul / staidx*.mul of all changed maps into the folder
        /// </summary>
        public int SaveAll(string path)
        {
            int n = 0;
            foreach (Map map in ChangedMaps)
            {
                Map.DefragStatics(path, map, map.Width, map.Height, false);
                ++n;
            }

            ChangedMaps.Clear();
            _undo.Clear();
            return n;
        }
    }
}
