// /***************************************************************************
//  *
//  * Dragon Map Studio – compiled map (land + editable statics) for the CGI view
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>One static on the map (mutable – the CGI view edits it).</summary>
    public sealed class StaticItem
    {
        public ushort Id;
        public int X;
        public int Y;
        public int Z;
        public ushort Hue;

        public StaticItem Copy() => (StaticItem)MemberwiseClone();

        public override string ToString() => $"{Id:x4} ({X},{Y},{Z}) hue {Hue}";
    }

    /// <summary>A land tile before / after an edit.</summary>
    public struct LandChange
    {
        public int Index;
        public ushort OldId;
        public sbyte OldZ;
        public ushort NewId;
        public sbyte NewZ;
    }

    /// <summary>One undo step: statics and / or land.</summary>
    public sealed class EditStep
    {
        /// <summary>Steps with the same group (> 0) belong to one stroke.</summary>
        public int Group;

        /// <summary>Text of the step for the history list (null = not named yet).</summary>
        public string Name;
        public List<StaticChange> Statics = new List<StaticChange>();
        public List<LandChange> Land = new List<LandChange>();
    }

    /// <summary>State of a static before / after an edit (null = does not exist).</summary>
    public sealed class StaticChange
    {
        public StaticItem Item;
        public StaticItem Before;
        public StaticItem After;
    }

    /// <summary>
    /// Land (tiles + z) and statics of a map. Statics are kept per 8×8 block so a tile lookup is fast,
    /// every change goes through <see cref="Apply"/> and can be undone.
    /// </summary>
    public sealed class MapData
    {
        private readonly List<StaticItem>[] _blocks;
        private readonly Stack<EditStep> _undo = new Stack<EditStep>();
        private readonly Stack<EditStep> _redo = new Stack<EditStep>();
        private Dictionary<int, LandChange> _pendingLand;

        public MapData(int width, int height, ushort[] tiles, sbyte[] z)
        {
            Width = width;
            Height = height;
            Tiles = tiles;
            Z = z;
            BlocksX = width / 8;
            BlocksY = height / 8;
            _blocks = new List<StaticItem>[BlocksX * BlocksY];
            Statics = new StaticLoader(this);
        }

        public int Width { get; }
        public int Height { get; }
        public int BlocksX { get; }
        public int BlocksY { get; }
        public ushort[] Tiles { get; }
        public sbyte[] Z { get; }

        public string Source { get; set; } = string.Empty;
        public int MapNumber { get; set; }

        /// <summary>Folder of the loaded files (null = built in memory).</summary>
        public string Folder { get; set; }

        public bool LandModified { get; set; }
        public bool StaticsModified { get; set; }
        public BuildResult Report { get; set; }
        public int StaticCount { get; private set; }

        public StaticLoader Statics { get; }

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public int LandZ(int x, int y) => InBounds(x, y) ? Z[y * Width + x] : 0;

        public ushort LandId(int x, int y) => InBounds(x, y) ? Tiles[y * Width + x] : (ushort)0;

        private List<StaticItem> Block(int x, int y, bool create)
        {
            int b = (x >> 3) * BlocksY + (y >> 3);
            List<StaticItem> list = _blocks[b];
            if (list == null && create)
            {
                list = _blocks[b] = new List<StaticItem>();
            }
            return list;
        }

        /// <summary>Statics on one tile (in file order).</summary>
        public IEnumerable<StaticItem> StaticsAt(int x, int y)
        {
            if (!InBounds(x, y))
            {
                yield break;
            }
            List<StaticItem> list = Block(x, y, false);
            if (list == null)
            {
                yield break;
            }
            foreach (StaticItem s in list)
            {
                if (s.X == x && s.Y == y)
                {
                    yield return s;
                }
            }
        }

        public IEnumerable<StaticItem> AllStatics() => _blocks.Where(b => b != null).SelectMany(b => b);

        /// <summary>Highest top (z + height) of the statics on a tile, or the land z.</summary>
        public int TopZ(int x, int y, Func<ushort, int> heightOf)
        {
            int top = LandZ(x, y);
            foreach (StaticItem s in StaticsAt(x, y))
            {
                top = Math.Max(top, s.Z + heightOf(s.Id));
            }
            return top;
        }

        internal void AddRaw(StaticItem s)
        {
            if (!InBounds(s.X, s.Y))
            {
                return;
            }
            Block(s.X, s.Y, true).Add(s);
            StaticCount++;
        }

        // ------------------------------------------------------------------ working copies (dungeon workshop)

        /// <summary>
        /// Copy of a part of the map (land + statics, no history). The rectangle is widened to whole 8×8 blocks;
        /// <paramref name="origin"/> is its top left corner in the big map.
        /// </summary>
        public MapData CopyRegion(Rectangle r, out Point origin)
        {
            int x0 = Math.Max(0, r.Left / 8 * 8), y0 = Math.Max(0, r.Top / 8 * 8);
            int x1 = Math.Min(Width, (r.Right + 7) / 8 * 8), y1 = Math.Min(Height, (r.Bottom + 7) / 8 * 8);
            int w = Math.Max(8, x1 - x0), h = Math.Max(8, y1 - y0);
            var tiles = new ushort[w * h];
            var z = new sbyte[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int sx = x0 + x, sy = y0 + y;
                    if (InBounds(sx, sy))
                    {
                        tiles[y * w + x] = Tiles[sy * Width + sx];
                        z[y * w + x] = Z[sy * Width + sx];
                    }
                }
            }
            var copy = new MapData(w, h, tiles, z) { Source = Source, MapNumber = MapNumber };
            for (int y = y0; y < y0 + h; y += 8)
            {
                for (int x = x0; x < x0 + w; x += 8)
                {
                    List<StaticItem> list = InBounds(x, y) ? Block(x, y, false) : null;
                    if (list == null)
                    {
                        continue;
                    }
                    foreach (StaticItem s in list)
                    {
                        StaticItem c = s.Copy();
                        c.X -= x0;
                        c.Y -= y0;
                        copy.AddRaw(c);
                    }
                }
            }
            origin = new Point(x0, y0);
            return copy;
        }

        /// <summary>
        /// Writes a working copy back (land + statics of its whole area) as ONE undo step of this map:
        /// changed land tiles, the old statics of the area removed and the statics of the copy added.
        /// </summary>
        public int WriteBack(MapData copy, Point origin)
        {
            var land = new List<LandChange>();
            for (int y = 0; y < copy.Height; y++)
            {
                for (int x = 0; x < copy.Width; x++)
                {
                    int mx = origin.X + x, my = origin.Y + y;
                    if (!InBounds(mx, my))
                    {
                        continue;
                    }
                    int i = my * Width + mx, j = y * copy.Width + x;
                    if (Tiles[i] != copy.Tiles[j] || Z[i] != copy.Z[j])
                    {
                        land.Add(new LandChange { Index = i, OldId = Tiles[i], OldZ = Z[i], NewId = copy.Tiles[j], NewZ = copy.Z[j] });
                    }
                }
            }
            var statics = new List<StaticChange>();
            var area = new Rectangle(origin.X, origin.Y, copy.Width, copy.Height);
            var before = new List<StaticItem>();
            for (int y = area.Top; y < area.Bottom; y += 8)
            {
                for (int x = area.Left; x < area.Right; x += 8)
                {
                    List<StaticItem> list = InBounds(x, y) ? Block(x, y, false) : null;
                    if (list != null)
                    {
                        before.AddRange(list.Where(s => area.Contains(s.X, s.Y)));
                    }
                }
            }
            var after = copy.AllStatics().Select(s =>
            {
                StaticItem c = s.Copy();
                c.X += origin.X;
                c.Y += origin.Y;
                return c;
            }).Where(s => InBounds(s.X, s.Y)).ToList();
            // unchanged statics stay (no churn in the undo step)
            var key = new Dictionary<(int, int, int, ushort, ushort), int>();
            foreach (StaticItem s in after)
            {
                var k = (s.X, s.Y, s.Z, s.Id, s.Hue);
                key[k] = key.TryGetValue(k, out int n) ? n + 1 : 1;
            }
            foreach (StaticItem s in before)
            {
                var k = (s.X, s.Y, s.Z, s.Id, s.Hue);
                if (key.TryGetValue(k, out int n) && n > 0)
                {
                    key[k] = n - 1;
                    continue;
                }
                statics.Add(Remove(s));
            }
            foreach (StaticItem s in after)
            {
                var k = (s.X, s.Y, s.Z, s.Id, s.Hue);
                if (key.TryGetValue(k, out int n) && n > 0)
                {
                    key[k] = n - 1;
                    statics.Add(Add(s));
                }
            }
            HashSet<int> mask = EditMask;
            EditMask = null;
            Apply(statics, land);
            EditMask = mask;
            return statics.Count + land.Count;
        }

        /// <summary>Forgets undo / redo (a fresh working copy).</summary>
        public void ClearHistory()
        {
            _undo.Clear();
            _redo.Clear();
        }

        private void RemoveRaw(StaticItem s)
        {
            if (Block(s.X, s.Y, false)?.Remove(s) == true)
            {
                StaticCount--;
            }
        }

        private void SetState(StaticItem item, StaticItem state, bool exists)
        {
            RemoveRaw(item);
            if (state != null)
            {
                item.Id = state.Id;
                item.X = state.X;
                item.Y = state.Y;
                item.Z = Math.Clamp(state.Z, -128, 127);
                item.Hue = state.Hue;
            }
            if (exists)
            {
                AddRaw(item);
            }
        }

        // ------------------------------------------------------------------ edits (undoable)

        /// <summary>Applies a group of static changes as one undo step.</summary>
        public void Apply(List<StaticChange> changes) => Apply(changes, null);

        /// <summary>Applies static and land changes as one undo step.</summary>
        public void Apply(List<StaticChange> changes, List<LandChange> land)
        {
            if (EditMask != null)
            {
                changes = changes?.Where(c => InMask(c.Before ?? c.After)).ToList();
                land = land?.Where(l => EditMask.Contains(l.Index)).ToList();
            }
            var step = new EditStep { Statics = changes ?? new List<StaticChange>(), Land = land ?? new List<LandChange>() };
            if (step.Statics.Count == 0 && step.Land.Count == 0)
            {
                return;
            }
            foreach (StaticChange c in step.Statics)
            {
                SetState(c.Item, c.After, c.After != null);
            }
            foreach (LandChange l in step.Land)
            {
                Tiles[l.Index] = l.NewId;
                Z[l.Index] = l.NewZ;
            }
            _undo.Push(step);
            _redo.Clear();
            StaticsModified |= step.Statics.Count > 0;
            LandModified |= step.Land.Count > 0;
        }

        // ------------------------------------------------------------------ land strokes (mouse drag = one undo step)

        /// <summary>Group of the next stroke steps (BeginLand … EndLand); 0 = none.</summary>
        public int StrokeGroup { get; set; }

        private int _nextGroup;

        public int NewGroup() => ++_nextGroup;

        public int UndoGroup => _undo.Count > 0 ? _undo.Peek().Group : 0;

        /// <summary>Undoes all steps of the stroke of the last step.</summary>
        public bool UndoWholeGroup()
        {
            int g = UndoGroup;
            bool any = Undo();
            while (g > 0 && UndoGroup == g)
            {
                Undo();
            }
            return any;
        }

        public void BeginLand()
        {
            _pendingLand = new Dictionary<int, LandChange>();
            _pendingStatics = new List<StaticChange>();
        }

        private List<StaticChange> _pendingStatics;

        /// <summary>
        /// Static changes inside a stroke (BeginLand / EndLand): applied at once and stored in the same undo step as the land.
        /// Outside a stroke they become an undo step of their own.
        /// </summary>
        public void ApplyStatics(List<StaticChange> changes)
        {
            if (EditMask != null && changes != null)
            {
                changes = changes.Where(c => InMask(c.Before ?? c.After)).ToList();
            }
            if (changes == null || changes.Count == 0)
            {
                return;
            }
            if (_pendingStatics == null)
            {
                Apply(changes);
                return;
            }
            foreach (StaticChange c in changes)
            {
                SetState(c.Item, c.After, c.After != null);
                _pendingStatics.Add(c);
            }
        }

        public bool LandStrokeActive => _pendingLand != null;

        /// <summary>Changes a land tile; inside BeginLand / EndLand the change is collected into one step.</summary>
        public void SetLand(int x, int y, ushort id, int z)
        {
            if (!InBounds(x, y))
            {
                return;
            }
            int i = y * Width + x;
            sbyte nz = (sbyte)Math.Clamp(z, -128, 127);
            if ((Tiles[i] == id && Z[i] == nz) || (EditMask != null && !EditMask.Contains(i)))
            {
                return;
            }
            if (_pendingLand == null)
            {
                Apply(null, new List<LandChange> { new LandChange { Index = i, OldId = Tiles[i], OldZ = Z[i], NewId = id, NewZ = nz } });
                return;
            }
            if (!_pendingLand.TryGetValue(i, out LandChange c))
            {
                c = new LandChange { Index = i, OldId = Tiles[i], OldZ = Z[i] };
            }
            c.NewId = id;
            c.NewZ = nz;
            _pendingLand[i] = c;
            Tiles[i] = id;
            Z[i] = nz;
        }

        public int EndLand()
        {
            Dictionary<int, LandChange> pending = _pendingLand;
            List<StaticChange> statics = _pendingStatics ?? new List<StaticChange>();
            _pendingLand = null;
            _pendingStatics = null;
            if ((pending == null || pending.Count == 0) && statics.Count == 0)
            {
                return 0;
            }
            var step = new EditStep { Land = pending?.Values.ToList() ?? new List<LandChange>(), Statics = statics, Group = StrokeGroup };
            _undo.Push(step);
            _redo.Clear();
            LandModified |= step.Land.Count > 0;
            StaticsModified |= statics.Count > 0;
            return step.Land.Count + statics.Count;
        }

        // ------------------------------------------------------------------ mask + history names

        /// <summary>
        /// Lasso / magic wand: only cells with these indices (y * Width + x) can be changed (land and statics);
        /// null = everywhere. Undo / redo are not limited.
        /// </summary>
        public HashSet<int> EditMask { get; set; }

        private bool InMask(StaticItem s) => s == null || EditMask == null || (InBounds(s.X, s.Y) && EditMask.Contains(s.Y * Width + s.X));

        public int UndoCount => _undo.Count;

        public int RedoCount => _redo.Count;

        /// <summary>Gives the newest undo step a name (history list) when it has none yet.</summary>
        public void NameLastStep(string name)
        {
            if (_undo.Count > 0 && _undo.Peek().Name == null)
            {
                _undo.Peek().Name = name;
            }
        }

        /// <summary>Names of the undo steps, newest first.</summary>
        public IEnumerable<string> UndoNames => _undo.Select(Describe);

        /// <summary>Names of the redo steps, next first.</summary>
        public IEnumerable<string> RedoNames => _redo.Select(Describe);

        private static string Describe(EditStep s)
        {
            string what = s.Land.Count > 0 && s.Statics.Count > 0 ? $"{s.Land.Count} Land, {s.Statics.Count} Statics"
                : s.Land.Count > 0 ? $"{s.Land.Count} Land" : $"{s.Statics.Count} Statics";
            return string.IsNullOrEmpty(s.Name) ? what : s.Name + "  (" + what + ")";
        }

        /// <summary>The newest <paramref name="count"/> undo steps (newest first) – for the test log: what a tool really changed.</summary>
        public List<EditStep> RecentSteps(int count) => _undo.Take(Math.Max(0, count)).ToList();

        public bool Undo()
        {
            if (_undo.Count == 0)
            {
                return false;
            }
            EditStep step = _undo.Pop();
            for (int i = step.Statics.Count - 1; i >= 0; i--)
            {
                SetState(step.Statics[i].Item, step.Statics[i].Before, step.Statics[i].Before != null);
            }
            foreach (LandChange l in step.Land)
            {
                Tiles[l.Index] = l.OldId;
                Z[l.Index] = l.OldZ;
            }
            _redo.Push(step);
            StaticsModified |= step.Statics.Count > 0;
            LandModified |= step.Land.Count > 0;
            return true;
        }

        public bool Redo()
        {
            if (_redo.Count == 0)
            {
                return false;
            }
            EditStep step = _redo.Pop();
            foreach (StaticChange c in step.Statics)
            {
                SetState(c.Item, c.After, c.After != null);
            }
            foreach (LandChange l in step.Land)
            {
                Tiles[l.Index] = l.NewId;
                Z[l.Index] = l.NewZ;
            }
            _undo.Push(step);
            StaticsModified |= step.Statics.Count > 0;
            LandModified |= step.Land.Count > 0;
            return true;
        }

        public static StaticChange Add(StaticItem s) => new StaticChange { Item = s, Before = null, After = s.Copy() };

        public static StaticChange Remove(StaticItem s) => new StaticChange { Item = s, Before = s.Copy(), After = null };

        public static StaticChange Modify(StaticItem s, Action<StaticItem> change)
        {
            StaticItem after = s.Copy();
            change(after);
            return new StaticChange { Item = s, Before = s.Copy(), After = after };
        }

        // ------------------------------------------------------------------ files

        public List<StaticEntry> ToEntries()
        {
            var list = new List<StaticEntry>(StaticCount);
            foreach (StaticItem s in AllStatics())
            {
                list.Add(new StaticEntry { Id = s.Id, X = s.X, Y = s.Y, Z = (sbyte)Math.Clamp(s.Z, -128, 127), Hue = s.Hue });
            }
            return list;
        }

        /// <summary>Loads map#.mul (or .uop) and the statics next to it.</summary>
        public static MapData LoadFiles(string mapFile, int width, int height, int mapNumber, out string staticsInfo)
        {
            MulMapIO.ReadMap(mapFile, width, height, out ushort[] tiles, out sbyte[] z, mapNumber);
            var data = new MapData(width, height, tiles, z)
            {
                Source = mapFile,
                MapNumber = mapNumber,
                Folder = Path.GetDirectoryName(mapFile)
            };
            string idx = Path.Combine(data.Folder, $"staidx{mapNumber}.mul");
            string sta = Path.Combine(data.Folder, $"statics{mapNumber}.mul");
            if (File.Exists(idx) && File.Exists(sta))
            {
                data.Statics.AddRange(MulMapIO.ReadStatics(idx, sta, width, height));
                staticsInfo = $"{data.StaticCount:N0} Statics aus {Path.GetFileName(sta)}";
            }
            else
            {
                staticsInfo = "keine staidx/statics-Dateien gefunden – leer gestartet";
            }
            data.StaticsModified = false;
            return data;
        }

        /// <summary>Writes the statics (and the land when <paramref name="writeMap"/>) into a folder. Existing files get a .bak copy.</summary>
        public List<string> Save(string folder, bool writeMap, bool writeUop)
        {
            Directory.CreateDirectory(folder);
            var files = new List<string>();
            string idx = Path.Combine(folder, $"staidx{MapNumber}.mul");
            string sta = Path.Combine(folder, $"statics{MapNumber}.mul");
            Backup(idx);
            Backup(sta);
            MulMapIO.WriteStatics(idx, sta, BlocksX, BlocksY, ToEntries());
            files.Add(idx);
            files.Add(sta);
            if (writeMap)
            {
                string map = Path.Combine(folder, $"map{MapNumber}.mul");
                Backup(map);
                MulMapIO.WriteMap(map, Width, Height, Tiles, Z);
                files.Add(map);
                if (writeUop)
                {
                    string uop = Path.Combine(folder, $"map{MapNumber}LegacyMUL.uop");
                    Backup(uop);
                    MulMapIO.WriteUop(map, uop, MapNumber);
                    files.Add(uop);
                }
                LandModified = false;
            }
            Folder = folder;
            StaticsModified = false;
            return files;
        }

        private static void Backup(string path)
        {
            if (File.Exists(path))
            {
                File.Copy(path, path + ".bak", true);
            }
        }

        /// <summary>Bulk loader (no undo) for the statics of a file / build.</summary>
        public sealed class StaticLoader
        {
            private readonly MapData _owner;

            public StaticLoader(MapData owner)
            {
                _owner = owner;
            }

            public void AddRange(IEnumerable<StaticEntry> entries)
            {
                foreach (StaticEntry e in entries)
                {
                    _owner.AddRaw(new StaticItem { Id = e.Id, X = e.X, Y = e.Y, Z = e.Z, Hue = e.Hue });
                }
            }
        }
    }
}
