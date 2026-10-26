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
using Ultima;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Changes land and statics of a map in memory (like Freeze/Melt Statics) and remembers the original
    /// blocks, so all changes can be undone. <see cref="Save"/> writes map/statics files into a directory.
    /// </summary>
    public sealed class RegionEditSession
    {
        private sealed class LandBackup
        {
            public bool WasModified;
            public Tile[] Tiles;
        }

        private sealed class StaticBackup
        {
            public bool HadOverride;
            public StaticTile[] Override;
            public StaticTile[] Pending;
        }

        private readonly Map _map;
        private readonly Dictionary<long, LandBackup> _land = new Dictionary<long, LandBackup>();
        private readonly Dictionary<long, StaticBackup> _statics = new Dictionary<long, StaticBackup>();

        public RegionEditSession(Map map)
        {
            _map = map;
        }

        /// <summary>Changes since the start or the last save</summary>
        public int ChangeCount { get; private set; }

        public bool HasUnsavedChanges => ChangeCount > 0;

        /// <summary>Blocks with changes that can be undone</summary>
        public bool CanUndo => _land.Count > 0 || _statics.Count > 0;

        private long Key(int bx, int by) => ((long)bx << 32) | (uint)by;

        #region Land

        public void SetLandTile(int x, int y, ushort id, sbyte z)
        {
            TileMatrix tiles = _map.Tiles;
            int bx = x >> 3, by = y >> 3;
            BackupLand(bx, by);

            Tile[] block = (Tile[])tiles.GetLandBlock(bx, by).Clone();
            int index = ((y & 0x7) << 3) + (x & 0x7);
            block[index] = new Tile(id, z);
            tiles.SetModifiedLandBlock(bx, by, block);
            ChangeCount++;
        }

        private void BackupLand(int bx, int by)
        {
            long key = Key(bx, by);
            if (_land.ContainsKey(key))
            {
                return;
            }

            TileMatrix tiles = _map.Tiles;
            _land[key] = new LandBackup
            {
                WasModified = tiles.IsLandBlockModified(bx, by),
                Tiles = (Tile[])tiles.GetLandBlock(bx, by).Clone()
            };
        }

        #endregion

        #region Statics

        /// <summary>
        /// Removes one static (first match of id, z and hue) at a map position. Returns false if not found.
        /// </summary>
        public bool RemoveStatic(int x, int y, int id, int z, int hue)
        {
            return ChangeStatics(x, y, list =>
            {
                int index = list.FindIndex(t => Matches(t, x, y, id, z, hue));
                if (index < 0)
                {
                    return false;
                }

                list.RemoveAt(index);
                return true;
            });
        }

        /// <summary>
        /// Changes z of one static (first match). Returns false if not found.
        /// </summary>
        public bool SetStaticZ(int x, int y, int id, int z, int hue, int newZ)
        {
            newZ = Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, newZ));
            return ChangeStatics(x, y, list =>
            {
                int index = list.FindIndex(t => Matches(t, x, y, id, z, hue));
                if (index < 0)
                {
                    return false;
                }

                StaticTile t = list[index];
                t.Z = (sbyte)newZ;
                list[index] = t;
                return true;
            });
        }

        /// <summary>
        /// Adds a static at a map position
        /// </summary>
        public void AddStatic(int x, int y, ushort id, sbyte z, short hue)
        {
            ChangeStatics(x, y, list =>
            {
                list.Add(new StaticTile { Id = id, X = (byte)(x & 0x7), Y = (byte)(y & 0x7), Z = z, Hue = hue });
                return true;
            });
        }

        private static bool Matches(StaticTile t, int x, int y, int id, int z, int hue)
        {
            return t.X == (x & 0x7) && t.Y == (y & 0x7) && t.Id == id && t.Z == z && t.Hue == hue;
        }

        private bool ChangeStatics(int x, int y, Func<List<StaticTile>, bool> change)
        {
            TileMatrix tiles = _map.Tiles;
            int bx = x >> 3, by = y >> 3;
            List<StaticTile> list = tiles.GetStaticBlockTiles(bx, by);
            if (!change(list))
            {
                return false;
            }

            BackupStatics(bx, by);
            tiles.SetStaticBlockOverride(bx, by, list.ToArray());
            ChangeCount++;
            return true;
        }

        private void BackupStatics(int bx, int by)
        {
            long key = Key(bx, by);
            if (_statics.ContainsKey(key))
            {
                return;
            }

            TileMatrix tiles = _map.Tiles;
            bool hadOverride = tiles.HasStaticBlockOverride(bx, by);
            _statics[key] = new StaticBackup
            {
                HadOverride = hadOverride,
                Override = hadOverride ? (StaticTile[])tiles.GetStaticBlockOverride(bx, by).Clone() : null,
                Pending = tiles.GetPendingStatics(bx, by)
            };
        }

        #endregion

        #region Fix

        /// <summary>
        /// Repairs an issue. Returns false if it can not be repaired (anymore).
        /// </summary>
        public bool Fix(RegionIssue issue, RegionData data)
        {
            if (!issue.CanAutoFix || issue.Status != RegionIssueStatus.Open)
            {
                return false;
            }

            RegionCell cell = data.GetCell(issue.X, issue.Y);
            bool done;

            switch (issue.Type)
            {
                case RegionIssueType.InvalidStaticId:
                case RegionIssueType.DuplicateStatic:
                case RegionIssueType.StaticBelowLand:
                    done = RemoveStatic(issue.X, issue.Y, issue.TileId, issue.Z, issue.Hue);
                    break;

                case RegionIssueType.InvalidLandId:
                    {
                        ushort replacement = FindNeighbourLand(issue.X, issue.Y);
                        Tile current = _map.Tiles.GetLandTile(issue.X, issue.Y);
                        SetLandTile(issue.X, issue.Y, replacement, current.Z);
                        done = true;
                        break;
                    }

                case RegionIssueType.SteepLand:
                    {
                        int avg = AverageNeighbourZ(issue.X, issue.Y);
                        Tile current = _map.Tiles.GetLandTile(issue.X, issue.Y);
                        SetLandTile(issue.X, issue.Y, current.Id, (sbyte)avg);
                        done = true;
                        break;
                    }

                case RegionIssueType.FloatingStatic:
                    {
                        if (cell == null)
                        {
                            return false;
                        }

                        RegionStatic s = cell.Statics.FirstOrDefault(t => t.Id == issue.TileId && t.Z == issue.Z && t.Hue == issue.Hue);
                        int landTop = Math.Max(Math.Max(cell.LandZ, cell.ZRight), Math.Max(cell.ZBottom, cell.ZLeft));
                        int support = RegionAnalyzer.GetSupport(cell, s, landTop);
                        done = SetStaticZ(issue.X, issue.Y, issue.TileId, issue.Z, issue.Hue, support);
                        break;
                    }

                default:
                    return false;
            }

            if (done)
            {
                issue.Status = RegionIssueStatus.Fixed;
            }

            return done;
        }

        private ushort FindNeighbourLand(int x, int y)
        {
            var counts = new Dictionary<ushort, int>();
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if ((dx == 0 && dy == 0) || nx < 0 || ny < 0 || nx >= _map.Width || ny >= _map.Height)
                    {
                        continue;
                    }

                    ushort id = _map.Tiles.GetLandTile(nx, ny).Id;
                    if (Art.IsValidLand(id))
                    {
                        counts[id] = counts.TryGetValue(id, out int c) ? c + 1 : 1;
                    }
                }
            }

            return counts.Count == 0 ? (ushort)0x0003 : counts.OrderByDescending(p => p.Value).First().Key;
        }

        private int AverageNeighbourZ(int x, int y)
        {
            int sum = 0, count = 0;
            int[,] offsets = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
            for (int i = 0; i < 4; i++)
            {
                int nx = x + offsets[i, 0], ny = y + offsets[i, 1];
                if (nx < 0 || ny < 0 || nx >= _map.Width || ny >= _map.Height)
                {
                    continue;
                }

                sum += _map.Tiles.GetLandTile(nx, ny).Z;
                count++;
            }

            return count == 0 ? _map.Tiles.GetLandTile(x, y).Z : (int)Math.Round(sum / (double)count);
        }

        #endregion

        #region Undo / Save

        /// <summary>
        /// Restores all blocks changed by this session
        /// </summary>
        public void UndoAll()
        {
            TileMatrix tiles = _map.Tiles;

            foreach (KeyValuePair<long, LandBackup> pair in _land)
            {
                int bx = (int)(pair.Key >> 32), by = (int)(pair.Key & 0xFFFFFFFF);
                if (pair.Value.WasModified)
                {
                    tiles.SetModifiedLandBlock(bx, by, pair.Value.Tiles);
                }
                else
                {
                    tiles.RevertLandBlock(bx, by);
                }
            }

            foreach (KeyValuePair<long, StaticBackup> pair in _statics)
            {
                int bx = (int)(pair.Key >> 32), by = (int)(pair.Key & 0xFFFFFFFF);
                if (pair.Value.HadOverride)
                {
                    tiles.SetStaticBlockOverride(bx, by, pair.Value.Override);
                }
                else
                {
                    tiles.RevertStaticBlock(bx, by);
                    if (pair.Value.Pending != null)
                    {
                        foreach (StaticTile t in pair.Value.Pending)
                        {
                            tiles.AddPendingStatic(bx, by, t);
                        }
                    }
                }
            }

            _land.Clear();
            _statics.Clear();
            ChangeCount = 0;
            _map.ResetCache();
        }

        /// <summary>
        /// Writes map{n}.mul (if land was changed) and staidx{n}/statics{n}.mul (if statics were changed) into a directory.
        /// Returns the written file names.
        /// </summary>
        public List<string> Save(string directory)
        {
            var files = new List<string>();
            TileMatrix tiles = _map.Tiles;

            if (tiles.HasModifiedLand)
            {
                files.Add(_map.SaveLand(directory));
            }

            if (tiles.HasStaticOverrides || _statics.Count > 0)
            {
                Map.DefragStatics(directory, _map, _map.Width, _map.Height, false);
                files.Add(System.IO.Path.Combine(directory, $"staidx{_map.FileIndex}.mul"));
                files.Add(System.IO.Path.Combine(directory, $"statics{_map.FileIndex}.mul"));
            }

            ChangeCount = 0;
            return files;
        }

        #endregion
    }
}
