/***************************************************************************
 *
 * MultiEditor – walkability check
 *  - step too high (more than 2 Z up without stairs)
 *  - missing floor (hole between walkable cells)
 *  - ceiling too low (less than 16 Z room above a floor)
 *  - not reachable from a door / ban location
 *
 * The rules follow the UO movement roughly (person height 16, step 2,
 * stairs allow bigger steps). It is a help, not a server simulation.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    public enum WalkIssueKind
    {
        StepTooHigh,
        MissingFloor,
        LowCeiling,
        Unreachable
    }

    public sealed class WalkIssue
    {
        public WalkIssueKind Kind { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public string Text { get; set; }

        public string KindName => Kind switch
        {
            WalkIssueKind.StepTooHigh => L.T("Stufe zu hoch", "Step too high"),
            WalkIssueKind.MissingFloor => L.T("Fehlender Boden", "Missing floor"),
            WalkIssueKind.LowCeiling => L.T("Decke zu niedrig", "Ceiling too low"),
            _ => L.T("Nicht erreichbar", "Not reachable")
        };
    }

    public sealed class WalkabilityChecker
    {
        private const int _personHeight = 16;
        private const int _stepHeight = 2;
        private const int _stairStep = 10;

        private sealed class CheckTile
        {
            public int Z;
            public int Top;
            public TileFlag Flags;
            public bool Stair => (Flags & (TileFlag.StairBack | TileFlag.StairRight | TileFlag.Bridge)) != 0;
        }

        private readonly Dictionary<(int, int), List<CheckTile>> _cells = new Dictionary<(int, int), List<CheckTile>>();
        private readonly Dictionary<(int, int), List<CheckTile>> _walkable = new Dictionary<(int, int), List<CheckTile>>();

        public List<WalkIssue> Issues { get; } = new List<WalkIssue>();

        public int WalkableSurfaces { get; private set; }

        public int ReachableSurfaces { get; private set; }

        /// <summary>
        /// tiles: cell coordinates. starts: cells (with z) to begin the reachability test (doors, ban location)
        /// </summary>
        public void Run(IEnumerable<ImportTile> tiles, IEnumerable<(int x, int y, int z)> starts)
        {
            _cells.Clear();
            _walkable.Clear();
            Issues.Clear();

            foreach (ImportTile t in tiles)
            {
                if (t.Id == 0x1 || t.Id >= TileData.ItemTable.Length)
                {
                    continue;
                }

                ItemData d = TileData.ItemTable[t.Id];
                if ((d.Flags & (TileFlag.Surface | TileFlag.Impassable | TileFlag.Wall | TileFlag.Bridge)) == 0)
                {
                    continue;
                }

                if (!_cells.TryGetValue((t.X, t.Y), out List<CheckTile> list))
                {
                    list = new List<CheckTile>();
                    _cells[(t.X, t.Y)] = list;
                }

                list.Add(new CheckTile { Z = t.Z, Top = t.Z + d.CalcHeight, Flags = d.Flags });
            }

            FindWalkable();
            CheckSteps();
            CheckHoles();
            CheckReachable(starts?.ToList() ?? new List<(int, int, int)>());
        }

        private static bool Blocks(CheckTile t)
        {
            return (t.Flags & (TileFlag.Impassable | TileFlag.Surface | TileFlag.Wall)) != 0 && (t.Flags & TileFlag.Door) == 0;
        }

        private void FindWalkable()
        {
            WalkableSurfaces = 0;
            foreach (KeyValuePair<(int, int), List<CheckTile>> cell in _cells)
            {
                var walk = new List<CheckTile>();
                foreach (CheckTile s in cell.Value.Where(t => (t.Flags & (TileFlag.Surface | TileFlag.Bridge)) != 0 && (t.Flags & TileFlag.Impassable) == 0))
                {
                    int top = s.Top;
                    CheckTile blocker = null;
                    foreach (CheckTile o in cell.Value)
                    {
                        if (o == s || !Blocks(o))
                        {
                            continue;
                        }

                        // something inside the person space top .. top + 16
                        if (o.Z < top + _personHeight && o.Top > top)
                        {
                            blocker = o;
                            break;
                        }

                        // a flat floor above, closer than the person height
                        if (o.Z >= top && o.Z < top + _personHeight && (o.Flags & TileFlag.Surface) != 0 && o.Top == o.Z && o.Z > top)
                        {
                            blocker = o;
                            break;
                        }
                    }

                    if (blocker == null)
                    {
                        walk.Add(s);
                        ++WalkableSurfaces;
                    }
                    else if ((blocker.Flags & (TileFlag.Wall | TileFlag.Impassable)) == 0 && !blocker.Stair && blocker.Top == blocker.Z)
                    {
                        Issues.Add(new WalkIssue
                        {
                            Kind = WalkIssueKind.LowCeiling,
                            X = cell.Key.Item1,
                            Y = cell.Key.Item2,
                            Z = top,
                            Text = L.T($"Nur {blocker.Z - top} Z Platz bis zur Decke (16 nötig)", $"Only {blocker.Z - top} Z room to the ceiling (16 needed)")
                        });
                    }
                }

                if (walk.Count > 0)
                {
                    _walkable[cell.Key] = walk;
                }
            }
        }

        private static readonly (int dx, int dy)[] _dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        private bool CanStep(CheckTile from, CheckTile to)
        {
            int dz = to.Top - from.Top;
            if (dz <= _stepHeight)
            {
                return dz >= -_personHeight * 4; // falling down is always fine
            }

            return (from.Stair || to.Stair) && dz <= _stairStep;
        }

        private void CheckSteps()
        {
            var reported = new HashSet<(int, int, int)>();
            foreach (KeyValuePair<(int, int), List<CheckTile>> cell in _walkable)
            {
                foreach (CheckTile a in cell.Value)
                {
                    foreach ((int dx, int dy) in _dirs)
                    {
                        if (!_walkable.TryGetValue((cell.Key.Item1 + dx, cell.Key.Item2 + dy), out List<CheckTile> n))
                        {
                            continue;
                        }

                        // the nearest surface above a, within one floor
                        CheckTile b = n.Where(t => t.Top > a.Top && t.Top - a.Top < _personHeight).OrderBy(t => t.Top).FirstOrDefault();
                        if (b == null || n.Any(t => Math.Abs(t.Top - a.Top) <= _stepHeight))
                        {
                            continue;
                        }

                        // big differences are other floors or the side of stairs, only small steps are mistakes
                        if (CanStep(a, b) || a.Stair || b.Stair || b.Top - a.Top >= _stairStep)
                        {
                            continue;
                        }

                        int nx = cell.Key.Item1 + dx;
                        int ny = cell.Key.Item2 + dy;
                        if (reported.Add((nx, ny, b.Top)))
                        {
                            Issues.Add(new WalkIssue
                            {
                                Kind = WalkIssueKind.StepTooHigh,
                                X = nx,
                                Y = ny,
                                Z = b.Top,
                                Text = L.T($"Stufe {b.Top - a.Top} Z hoch (max. 2, Treppe max. {_stairStep}) von {cell.Key.Item1},{cell.Key.Item2}",
                                    $"Step {b.Top - a.Top} Z high (max 2, stairs max {_stairStep}) from {cell.Key.Item1},{cell.Key.Item2}")
                            });
                        }
                    }
                }
            }
        }

        private void CheckHoles()
        {
            if (_walkable.Count == 0)
            {
                return;
            }

            int minX = _walkable.Keys.Min(k => k.Item1);
            int maxX = _walkable.Keys.Max(k => k.Item1);
            int minY = _walkable.Keys.Min(k => k.Item2);
            int maxY = _walkable.Keys.Max(k => k.Item2);

            for (int x = minX; x <= maxX; ++x)
            {
                for (int y = minY; y <= maxY; ++y)
                {
                    _walkable.TryGetValue((x, y), out List<CheckTile> ownWalk);
                    _cells.TryGetValue((x, y), out List<CheckTile> own);

                    // every level the neighbours stand on
                    var levels = new List<int>();
                    foreach ((int dx, int dy) in _dirs)
                    {
                        if (_walkable.TryGetValue((x + dx, y + dy), out List<CheckTile> n))
                        {
                            levels.AddRange(n.Select(t => t.Top));
                        }
                    }

                    foreach (int z in levels.Distinct())
                    {
                        if (levels.Count(t => Math.Abs(t - z) <= _stepHeight) < 3)
                        {
                            continue;
                        }

                        // walkable here or blocked by a wall / object at this level = no hole
                        if (ownWalk != null && ownWalk.Any(t => Math.Abs(t.Top - z) <= _stepHeight))
                        {
                            continue;
                        }

                        if (own != null && own.Any(t => t.Z <= z + _personHeight && t.Top >= z - _stepHeight))
                        {
                            continue;
                        }

                        Issues.Add(new WalkIssue
                        {
                            Kind = WalkIssueKind.MissingFloor,
                            X = x,
                            Y = y,
                            Z = z,
                            Text = L.T($"Kein Boden, Nachbarn auf Z {z}", $"No floor, neighbours at Z {z}")
                        });
                        break;
                    }
                }
            }
        }

        private void CheckReachable(List<(int x, int y, int z)> starts)
        {
            ReachableSurfaces = 0;
            if (starts.Count == 0 || _walkable.Count == 0)
            {
                return;
            }

            var visited = new HashSet<CheckTile>();
            var queue = new Queue<((int, int) cell, CheckTile t)>();

            foreach ((int x, int y, int z) in starts)
            {
                // the start cell itself or a neighbour (doors stand in the wall line)
                foreach ((int dx, int dy) in new[] { (0, 0) }.Concat(_dirs))
                {
                    if (!_walkable.TryGetValue((x + dx, y + dy), out List<CheckTile> list))
                    {
                        continue;
                    }

                    CheckTile best = list.OrderBy(t => Math.Abs(t.Top - z)).First();
                    if (Math.Abs(best.Top - z) <= 20 && visited.Add(best))
                    {
                        queue.Enqueue(((x + dx, y + dy), best));
                    }
                }
            }

            while (queue.Count > 0)
            {
                ((int cx, int cy), CheckTile a) = queue.Dequeue();
                foreach ((int dx, int dy) in _dirs)
                {
                    if (!_walkable.TryGetValue((cx + dx, cy + dy), out List<CheckTile> n))
                    {
                        continue;
                    }

                    foreach (CheckTile b in n)
                    {
                        if (!visited.Contains(b) && CanStep(a, b) && b.Top - a.Top >= -20)
                        {
                            visited.Add(b);
                            queue.Enqueue(((cx + dx, cy + dy), b));
                        }
                    }
                }
            }

            ReachableSurfaces = visited.Count;

            // the highest level is the roof, nobody has to reach it
            int roofTop = _walkable.Values.SelectMany(v => v).Max(t => t.Top);
            bool hasLower = _walkable.Values.SelectMany(v => v).Any(t => t.Top < roofTop - _stepHeight);
            foreach (KeyValuePair<(int, int), List<CheckTile>> cell in _walkable)
            {
                foreach (CheckTile t in cell.Value.Where(t => !visited.Contains(t) && !(hasLower && t.Top >= roofTop - _stepHeight)))
                {
                    Issues.Add(new WalkIssue
                    {
                        Kind = WalkIssueKind.Unreachable,
                        X = cell.Key.Item1,
                        Y = cell.Key.Item2,
                        Z = t.Top,
                        Text = L.T("Von Tür / Ban-Location aus nicht erreichbar", "Not reachable from door / ban location")
                    });
                }
            }
        }
    }
}
