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
        private readonly Stack<List<StaticChange>> _undo = new Stack<List<StaticChange>>();
        private readonly Stack<List<StaticChange>> _redo = new Stack<List<StaticChange>>();

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

        /// <summary>Applies a group of changes as one undo step.</summary>
        public void Apply(List<StaticChange> changes)
        {
            if (changes == null || changes.Count == 0)
            {
                return;
            }
            foreach (StaticChange c in changes)
            {
                SetState(c.Item, c.After, c.After != null);
            }
            _undo.Push(changes);
            _redo.Clear();
            StaticsModified = true;
        }

        public bool Undo()
        {
            if (_undo.Count == 0)
            {
                return false;
            }
            List<StaticChange> step = _undo.Pop();
            for (int i = step.Count - 1; i >= 0; i--)
            {
                SetState(step[i].Item, step[i].Before, step[i].Before != null);
            }
            _redo.Push(step);
            StaticsModified = true;
            return true;
        }

        public bool Redo()
        {
            if (_redo.Count == 0)
            {
                return false;
            }
            List<StaticChange> step = _redo.Pop();
            foreach (StaticChange c in step)
            {
                SetState(c.Item, c.After, c.After != null);
            }
            _undo.Push(step);
            StaticsModified = true;
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
