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
    /// A static with absolute map coordinates (used for batch changes)
    /// </summary>
    public readonly struct MapStatic
    {
        public MapStatic(int x, int y, ushort id, sbyte z, short hue)
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
        public sbyte Z { get; }
        public short Hue { get; }
    }

    /// <summary>
    /// Changes land and statics of a map in memory (like Freeze/Melt Statics). Every change is an
    /// operation that can be undone (last one or all). <see cref="MapSaveService"/> writes the files.
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

        private sealed class Operation
        {
            public string Name;
            public readonly Dictionary<long, LandBackup> Land = new Dictionary<long, LandBackup>();
            public readonly Dictionary<long, StaticBackup> Statics = new Dictionary<long, StaticBackup>();
            public bool IsEmpty => Land.Count == 0 && Statics.Count == 0;

            /// <summary>the operation was counted in <see cref="ChangeCount"/> (once, also for batches with several parts)</summary>
            public bool Counted;
        }

        private readonly Map _map;
        private readonly List<Operation> _operations = new List<Operation>();

        /// <summary>undone steps: state after the step (redo) and the step itself (to undo it again)</summary>
        private readonly List<(Operation Redo, Operation Undo)> _redo = new List<(Operation, Operation)>();
        private Operation _current;
        private int _batchDepth;

        public RegionEditSession(Map map)
        {
            _map = map;
        }

        public Map Map => _map;

        /// <summary>Changes since the start or the last save</summary>
        public int ChangeCount { get; private set; }

        public bool HasUnsavedChanges => ChangeCount > 0;

        public bool CanUndo => _operations.Count > 0;

        public int OperationCount => _operations.Count;

        public string LastOperationName => _operations.Count > 0 ? _operations[_operations.Count - 1].Name : null;

        private static long Key(int bx, int by) => ((long)bx << 32) | (uint)by;

        private int _changedX1 = int.MaxValue, _changedY1 = int.MaxValue, _changedX2 = int.MinValue, _changedY2 = int.MinValue;

        private void MarkChanged(int bx, int by)
        {
            _changedX1 = Math.Min(_changedX1, bx);
            _changedY1 = Math.Min(_changedY1, by);
            _changedX2 = Math.Max(_changedX2, bx);
            _changedY2 = Math.Max(_changedY2, by);
        }

        /// <summary>
        /// Area (map tiles, whole 8x8 blocks) changed since the last call, or null if nothing changed.
        /// For redrawing only the changed part of overview pictures.
        /// </summary>
        public System.Drawing.Rectangle? TakeChangedArea()
        {
            if (_changedX1 > _changedX2)
            {
                return null;
            }

            var area = System.Drawing.Rectangle.FromLTRB(_changedX1 << 3, _changedY1 << 3, (_changedX2 + 1) << 3, (_changedY2 + 1) << 3);
            _changedX1 = _changedY1 = int.MaxValue;
            _changedX2 = _changedY2 = int.MinValue;
            return area;
        }

        #region Operations

        /// <summary>
        /// Groups all following changes into one undo step: <c>using (session.Batch("Place castle")) { ... }</c>
        /// </summary>
        public IDisposable Batch(string name)
        {
            if (_batchDepth++ == 0)
            {
                _current = new Operation { Name = name };
            }

            return new BatchScope(this);
        }

        private sealed class BatchScope : IDisposable
        {
            private RegionEditSession _session;

            public BatchScope(RegionEditSession session)
            {
                _session = session;
            }

            public void Dispose()
            {
                _session?.EndBatch();
                _session = null;
            }
        }

        private void EndBatch()
        {
            if (--_batchDepth > 0)
            {
                return;
            }

            if (_current != null && !_current.IsEmpty)
            {
                _operations.Add(_current);
                _redo.Clear();
            }

            _current = null;
        }

        /// <summary>Counts the running operation as one unsaved change</summary>
        private void CountChange()
        {
            Operation op = _current ?? (_operations.Count > 0 ? _operations[_operations.Count - 1] : null);
            if (op == null)
            {
                ChangeCount++;
                return;
            }

            if (!op.Counted)
            {
                op.Counted = true;
                ChangeCount++;
            }
        }

        private Operation Current(string name)
        {
            if (_current != null)
            {
                return _current;
            }

            // single change outside of a batch = own operation
            var op = new Operation { Name = name };
            _operations.Add(op);
            _redo.Clear();
            return op;
        }

        #endregion

        #region Land

        public void SetLandTile(int x, int y, ushort id, sbyte z)
        {
            SetLandTiles(new[] { (x, y, id, z) }, "Change land");
        }

        /// <summary>
        /// Changes many land tiles (grouped per block)
        /// </summary>
        public int SetLandTiles(IEnumerable<(int X, int Y, ushort Id, sbyte Z)> tiles, string name = "Change land")
        {
            using (Batch(name))
            {
                TileMatrix matrix = _map.Tiles;
                Operation op = null;
                int count = 0;

                foreach (IGrouping<long, (int X, int Y, ushort Id, sbyte Z)> group in tiles
                             .Where(t => t.X >= 0 && t.Y >= 0 && t.X < _map.Width && t.Y < _map.Height)
                             .GroupBy(t => Key(t.X >> 3, t.Y >> 3)))
                {
                    (int X, int Y, ushort Id, sbyte Z) first = group.First();
                    int bx = first.X >> 3, by = first.Y >> 3;

                    op ??= Current(name);
                    BackupLand(op, bx, by);

                    Tile[] block = (Tile[])matrix.GetLandBlock(bx, by).Clone();
                    foreach ((int X, int Y, ushort Id, sbyte Z) t in group)
                    {
                        block[((t.Y & 0x7) << 3) + (t.X & 0x7)] = new Tile(t.Id, t.Z);
                        count++;
                    }

                    matrix.SetModifiedLandBlock(bx, by, block);
                }

                if (count > 0)
                {
                    CountChange();
                }

                return count;
            }
        }

        /// <summary>
        /// Sets the land of all given positions to one height (tile ids are kept)
        /// </summary>
        public int FlattenLand(IEnumerable<(int X, int Y)> positions, int z)
        {
            sbyte target = (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, z));
            TileMatrix matrix = _map.Tiles;
            var tiles = positions.Distinct()
                .Where(p => p.X >= 0 && p.Y >= 0 && p.X < _map.Width && p.Y < _map.Height)
                .Select(p => (p.X, p.Y, matrix.GetLandTile(p.X, p.Y).Id, target))
                .ToList();
            return SetLandTiles(tiles, "Flatten land");
        }

        private void BackupLand(Operation op, int bx, int by)
        {
            MarkChanged(bx, by);
            long key = Key(bx, by);
            if (op.Land.ContainsKey(key))
            {
                return;
            }

            TileMatrix tiles = _map.Tiles;
            op.Land[key] = new LandBackup
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
            return RemoveStatics(new[] { new MapStatic(x, y, (ushort)id, (sbyte)z, (short)hue) }, "Remove static") > 0;
        }

        /// <summary>
        /// Removes many statics (each entry removes one matching static). Returns the number of removed statics.
        /// </summary>
        public int RemoveStatics(IEnumerable<MapStatic> statics, string name = "Remove statics")
        {
            using (Batch(name))
            {
                int removed = 0;
                foreach (IGrouping<long, MapStatic> group in statics.GroupBy(s => Key(s.X >> 3, s.Y >> 3)))
                {
                    MapStatic first = group.First();
                    removed += ChangeBlock(first.X >> 3, first.Y >> 3, name, list =>
                    {
                        int n = 0;
                        foreach (MapStatic s in group)
                        {
                            int index = list.FindIndex(t => Matches(t, s.X, s.Y, s.Id, s.Z, s.Hue));
                            if (index >= 0)
                            {
                                list.RemoveAt(index);
                                n++;
                            }
                        }

                        return n;
                    });
                }

                if (removed > 0)
                {
                    CountChange();
                }

                return removed;
            }
        }

        /// <summary>
        /// Removes all statics in a rectangle (map coordinates, inclusive) that match the filter.
        /// </summary>
        public int RemoveStaticsInArea(int x1, int y1, int x2, int y2, Func<StaticTile, bool> filter, string name = "Clear area")
        {
            var list = new List<MapStatic>();
            foreach (MapStatic s in GetStaticsInArea(x1, y1, x2, y2))
            {
                if (filter == null || filter(new StaticTile { Id = s.Id, Z = s.Z, Hue = s.Hue }))
                {
                    list.Add(s);
                }
            }

            return RemoveStatics(list, name);
        }

        /// <summary>
        /// All statics (file, patches, pending, changes) in a rectangle (map coordinates, inclusive)
        /// </summary>
        public List<MapStatic> GetStaticsInArea(int x1, int y1, int x2, int y2)
        {
            var result = new List<MapStatic>();
            x1 = Math.Max(0, x1);
            y1 = Math.Max(0, y1);
            x2 = Math.Min(_map.Width - 1, x2);
            y2 = Math.Min(_map.Height - 1, y2);

            TileMatrix tiles = _map.Tiles;
            for (int bx = x1 >> 3; bx <= x2 >> 3; bx++)
            {
                for (int by = y1 >> 3; by <= y2 >> 3; by++)
                {
                    foreach (StaticTile t in tiles.GetStaticBlockTiles(bx, by))
                    {
                        int x = (bx << 3) + t.X, y = (by << 3) + t.Y;
                        if (x >= x1 && x <= x2 && y >= y1 && y <= y2)
                        {
                            result.Add(new MapStatic(x, y, t.Id, t.Z, t.Hue));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Changes z of one static (first match). Returns false if not found.
        /// </summary>
        public bool SetStaticZ(int x, int y, int id, int z, int hue, int newZ)
        {
            newZ = Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, newZ));
            int changed = ChangeBlock(x >> 3, y >> 3, "Change static z", list =>
            {
                int index = list.FindIndex(t => Matches(t, x, y, id, z, hue));
                if (index < 0)
                {
                    return 0;
                }

                StaticTile t = list[index];
                t.Z = (sbyte)newZ;
                list[index] = t;
                return 1;
            });

            if (changed > 0)
            {
                CountChange();
            }

            return changed > 0;
        }

        public void AddStatic(int x, int y, ushort id, sbyte z, short hue)
        {
            AddStatics(new[] { new MapStatic(x, y, id, z, hue) }, "Add static");
        }

        /// <summary>
        /// Adds many statics (grouped per block). Positions outside of the map are skipped.
        /// </summary>
        public int AddStatics(IEnumerable<MapStatic> statics, string name = "Add statics")
        {
            using (Batch(name))
            {
                int added = 0;
                foreach (IGrouping<long, MapStatic> group in statics
                             .Where(s => s.X >= 0 && s.Y >= 0 && s.X < _map.Width && s.Y < _map.Height)
                             .GroupBy(s => Key(s.X >> 3, s.Y >> 3)))
                {
                    MapStatic first = group.First();
                    added += ChangeBlock(first.X >> 3, first.Y >> 3, name, list =>
                    {
                        int n = 0;
                        foreach (MapStatic s in group)
                        {
                            list.Add(new StaticTile { Id = s.Id, X = (byte)(s.X & 0x7), Y = (byte)(s.Y & 0x7), Z = s.Z, Hue = s.Hue });
                            n++;
                        }

                        return n;
                    });
                }

                if (added > 0)
                {
                    CountChange();
                }

                return added;
            }
        }

        private static bool Matches(StaticTile t, int x, int y, int id, int z, int hue)
        {
            return t.X == (x & 0x7) && t.Y == (y & 0x7) && t.Id == id && t.Z == z && t.Hue == hue;
        }

        /// <summary>
        /// Applies a change to the statics list of one block. The change returns the number of changed statics.
        /// </summary>
        private int ChangeBlock(int bx, int by, string name, Func<List<StaticTile>, int> change)
        {
            TileMatrix tiles = _map.Tiles;
            List<StaticTile> list = tiles.GetStaticBlockTiles(bx, by);
            int changed = change(list);
            if (changed == 0)
            {
                return 0;
            }

            BackupStatics(Current(name), bx, by);
            tiles.SetStaticBlockOverride(bx, by, list.ToArray());
            return changed;
        }

        private void BackupStatics(Operation op, int bx, int by)
        {
            MarkChanged(bx, by);
            long key = Key(bx, by);
            if (op.Statics.ContainsKey(key))
            {
                return;
            }

            TileMatrix tiles = _map.Tiles;
            bool hadOverride = tiles.HasStaticBlockOverride(bx, by);
            op.Statics[key] = new StaticBackup
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

        #region Undo

        /// <summary>
        /// Undoes the last operation. Returns its name or null.
        /// </summary>
        public string UndoLast() => UndoLast(true);

        /// <param name="allowRedo">false: the step is dropped (e.g. a preview), it can not be done again</param>
        public string UndoLast(bool allowRedo)
        {
            if (_operations.Count == 0)
            {
                return null;
            }

            Operation op = _operations[_operations.Count - 1];
            _operations.RemoveAt(_operations.Count - 1);
            if (allowRedo)
            {
                _redo.Add((Capture(op), op));
            }

            Restore(op);
            if (op.Counted)
            {
                ChangeCount = Math.Max(0, ChangeCount - 1);
            }
            _map.ResetCache();
            return op.Name;
        }

        public bool CanRedo => _redo.Count > 0;

        public string NextRedoName => _redo.Count > 0 ? _redo[_redo.Count - 1].Undo.Name : null;

        /// <summary>Does the last undone step again. Returns its name or null.</summary>
        public string Redo()
        {
            if (_redo.Count == 0)
            {
                return null;
            }

            (Operation redo, Operation undo) = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            Restore(redo);
            _operations.Add(undo);
            if (undo.Counted)
            {
                ChangeCount++;
            }

            _map.ResetCache();
            return undo.Name;
        }

        /// <summary>Current state of all blocks of an operation (to go back to it with <see cref="Restore"/>)</summary>
        private Operation Capture(Operation op)
        {
            var state = new Operation { Name = op.Name };
            foreach (long key in op.Land.Keys)
            {
                BackupLand(state, (int)(key >> 32), (int)(key & 0xFFFFFFFF));
            }

            foreach (long key in op.Statics.Keys)
            {
                BackupStatics(state, (int)(key >> 32), (int)(key & 0xFFFFFFFF));
            }

            return state;
        }

        /// <summary>
        /// Restores all blocks changed by this session
        /// </summary>
        public void UndoAll()
        {
            _redo.Clear();
            for (int i = _operations.Count - 1; i >= 0; i--)
            {
                Restore(_operations[i]);
            }

            _operations.Clear();
            ChangeCount = 0;
            _map.ResetCache();
        }

        private void Restore(Operation op)
        {
            TileMatrix tiles = _map.Tiles;
            foreach (long key in op.Land.Keys.Concat(op.Statics.Keys))
            {
                MarkChanged((int)(key >> 32), (int)(key & 0xFFFFFFFF));
            }

            foreach (KeyValuePair<long, LandBackup> pair in op.Land)
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

            foreach (KeyValuePair<long, StaticBackup> pair in op.Statics)
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
        }

        /// <summary>
        /// Called after the files were saved. <paramref name="keepUndo"/> = false drops the undo history
        /// (needed when the original files were replaced and the map was reloaded).
        /// </summary>
        public void MarkSaved(bool keepUndo)
        {
            ChangeCount = 0;
            if (!keepUndo)
            {
                _operations.Clear();
                _redo.Clear();
            }
        }

        #endregion
    }
}
