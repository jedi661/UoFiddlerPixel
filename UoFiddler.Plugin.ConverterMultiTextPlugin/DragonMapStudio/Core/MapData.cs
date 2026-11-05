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
            if (Tiles[i] == id && Z[i] == nz)
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
            var step = new EditStep { Land = pending?.Values.ToList() ?? new List<LandChange>(), Statics = statics };
            _undo.Push(step);
            _redo.Clear();
            LandModified |= step.Land.Count > 0;
            StaticsModified |= statics.Count > 0;
            return step.Land.Count + statics.Count;
        }

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
