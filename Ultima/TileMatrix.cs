using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Ultima.Helpers;

namespace Ultima
{
    public sealed class TileMatrix
    {
        private readonly HuedTile[][][][][] _staticTiles;
        private readonly Tile[][][] _landTiles;
        private bool[][] _removedStaticBlock;
        private List<StaticTile>[][] _staticTilesToAdd;

        public static Tile[] InvalidLandBlock { get; private set; }
        public static HuedTile[][][] EmptyStaticBlock { get; private set; }

        private FileStream _map;
        private BinaryReader _uopReader;
        private FileStream _statics;
        private Entry3D[] _staticIndex;

        public bool StaticIndexInit { get; set; }

        public TileMatrixPatch Patch { get; }

        public int BlockWidth { get; }

        public int BlockHeight { get; }

        public int Width { get; }

        public int Height { get; }

        private readonly string _mapPath;
        private readonly string _indexPath;
        private readonly string _staticsPath;

        /// <summary>Full path of the map file (mul or uop) or null</summary>
        public string MapPath => _mapPath;

        /// <summary>Full path of staidx or null</summary>
        public string IndexPath => _indexPath;

        /// <summary>Full path of statics or null</summary>
        public string StaticsPath => _staticsPath;

        // blocks replaced in memory (key = x * BlockHeight + y)
        private readonly Dictionary<long, StaticTile[]> _staticOverrides = new Dictionary<long, StaticTile[]>();
        private readonly HashSet<long> _modifiedLandBlocks = new HashSet<long>();

        private long BlockKey(int x, int y) => ((long)x * BlockHeight) + y;

        public void CloseStreams()
        {
            _map?.Close();
            _uopReader?.Close();
            _statics?.Close();
        }

        public bool AllFilesExist()
        {
            return _mapPath != null && _indexPath != null && _staticsPath != null;
        }

        public TileMatrix(int fileIndex, int mapId, int width, int height, string path)
        {
            Width = width;
            Height = height;
            BlockWidth = width >> 3;
            BlockHeight = height >> 3;

            if (path == null)
            {
                _mapPath = Files.GetFilePath($"map{fileIndex}.mul");
                if (string.IsNullOrEmpty(_mapPath) || !File.Exists(_mapPath))
                {
                    _mapPath = Files.GetFilePath($"map{fileIndex}LegacyMUL.uop");
                }

                if (_mapPath?.EndsWith(".uop") == true)
                {
                    IsUOPFormat = true;
                }
            }
            else
            {
                _mapPath = Path.Combine(path, $"map{fileIndex}.mul");
                if (!File.Exists(_mapPath))
                {
                    _mapPath = Path.Combine(path, $"map{fileIndex}LegacyMUL.uop");
                }

                if (!File.Exists(_mapPath))
                {
                    _mapPath = null;
                }
                else if (_mapPath?.EndsWith(".uop") == true)
                {
                    IsUOPFormat = true;
                }
            }

            if (path == null)
            {
                _indexPath = Files.GetFilePath($"staidx{fileIndex}.mul");
            }
            else
            {
                _indexPath = Path.Combine(path, $"staidx{fileIndex}.mul");
                if (!File.Exists(_indexPath))
                {
                    _indexPath = null;
                }
            }

            if (path == null)
            {
                _staticsPath = Files.GetFilePath($"statics{fileIndex}.mul");
            }
            else
            {
                _staticsPath = Path.Combine(path, $"statics{fileIndex}.mul");
                if (!File.Exists(_staticsPath))
                {
                    _staticsPath = null;
                }
            }

            EmptyStaticBlock = new HuedTile[8][][];

            for (int i = 0; i < 8; ++i)
            {
                EmptyStaticBlock[i] = new HuedTile[8][];

                for (int j = 0; j < 8; ++j)
                {
                    EmptyStaticBlock[i][j] = Array.Empty<HuedTile>();
                }
            }

            InvalidLandBlock = new Tile[196];

            _landTiles = new Tile[BlockWidth][][];
            _staticTiles = new HuedTile[BlockWidth][][][][];

            Patch = new TileMatrixPatch(this, mapId, path);
        }

        // TODO: unused?
        //public void SetStaticBlock(int x, int y, HuedTile[][][] value)
        //{
        //    if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
        //    {
        //        return;
        //    }

        //    if (_staticTiles[x] == null)
        //    {
        //        _staticTiles[x] = new HuedTile[BlockHeight][][][];
        //    }

        //    _staticTiles[x][y] = value;
        //}

        public HuedTile[][][] GetStaticBlock(int x, int y, bool patch = true)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
            {
                return EmptyStaticBlock;
            }

            if (_staticTiles[x] == null)
            {
                _staticTiles[x] = new HuedTile[BlockHeight][][][];
            }

            HuedTile[][][] tiles = _staticTiles[x][y] ?? (_staticTiles[x][y] = ReadStaticBlock(x, y));

            if (_staticOverrides.Count > 0 && _staticOverrides.ContainsKey(BlockKey(x, y)))
            {
                return tiles; // replaced in memory, ignore diff patches
            }

            if (Map.UseDiff && patch && Patch.StaticBlocksCount > 0 && Patch.StaticBlocks[x]?[y] != null)
            {
                tiles = Patch.StaticBlocks[x][y];
            }

            return tiles;
        }

        public HuedTile[] GetStaticTiles(int x, int y, bool patch)
        {
            return GetStaticBlock(x >> 3, y >> 3, patch)[x & 0x7][y & 0x7];
        }

        public HuedTile[] GetStaticTiles(int x, int y)
        {
            return GetStaticBlock(x >> 3, y >> 3)[x & 0x7][y & 0x7];
        }

        public void SetLandBlock(int x, int y, Tile[] value)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
            {
                return;
            }

            if (_landTiles[x] == null)
            {
                _landTiles[x] = new Tile[BlockHeight][];
            }

            _landTiles[x][y] = value;
        }

        public Tile[] GetLandBlock(int x, int y, bool patch = true)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
            {
                return InvalidLandBlock;
            }

            if (_landTiles[x] == null)
            {
                _landTiles[x] = new Tile[BlockHeight][];
            }

            Tile[] tiles = _landTiles[x][y] ?? (_landTiles[x][y] = ReadLandBlock(x, y));

            if (_modifiedLandBlocks.Count > 0 && _modifiedLandBlocks.Contains(BlockKey(x, y)))
            {
                return tiles; // changed in memory, ignore diff patches
            }

            if (Map.UseDiff && patch && Patch.LandBlocksCount > 0 && Patch.LandBlocks[x]?[y] != null)
            {
                tiles = Patch.LandBlocks[x][y];
            }

            return tiles;
        }
        public Tile GetLandTile(int x, int y, bool patch)
        {
            return GetLandBlock(x >> 3, y >> 3, patch)[((y & 0x7) << 3) + (x & 0x7)];
        }

        public Tile GetLandTile(int x, int y)
        {
            return GetLandBlock(x >> 3, y >> 3)[((y & 0x7) << 3) + (x & 0x7)];
        }

        private void InitStatics()
        {
            _staticIndex = new Entry3D[BlockHeight * BlockWidth];
            if (_indexPath == null || _staticsPath == null)
            {
                // staidx or statics missing: map without statics
                StaticIndexInit = true;
                return;
            }

            using (var index = new FileStream(_indexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _statics = new FileStream(_staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                GCHandle gc = GCHandle.Alloc(_staticIndex, GCHandleType.Pinned);
                var buffer = new byte[index.Length];
                index.Read(buffer, 0, (int)index.Length);
                Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)Math.Min(index.Length, BlockHeight * BlockWidth * 12));
                gc.Free();
                for (var i = (int)Math.Min(index.Length, BlockHeight * BlockWidth); i < BlockHeight * BlockWidth; ++i)
                {
                    _staticIndex[i].Lookup = -1;
                    _staticIndex[i].Length = -1;
                    _staticIndex[i].Extra = -1;
                }

                StaticIndexInit = true;
            }
        }

        private static HuedTileList[][] _lists;
        private static byte[] _buffer;

        private unsafe HuedTile[][][] ReadStaticBlock(int x, int y)
        {
            if (!StaticIndexInit)
            {
                InitStatics();
            }

            if (_statics?.CanRead != true || !_statics.CanSeek)
            {
                _statics = _staticsPath == null
                    ? null
                    : new FileStream(_staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (_statics == null)
            {
                return EmptyStaticBlock;
            }

            int lookup = _staticIndex[(x * BlockHeight) + y].Lookup;
            int length = _staticIndex[(x * BlockHeight) + y].Length;

            if (lookup < 0 || length <= 0)
            {
                return EmptyStaticBlock;
            }

            int count = length / 7;

            _statics.Seek(lookup, SeekOrigin.Begin);

            if (_buffer == null || _buffer.Length < length)
            {
                _buffer = new byte[length];
            }

            GCHandle gc = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            try
            {
                _statics.Read(_buffer, 0, length);

                if (_lists == null)
                {
                    _lists = new HuedTileList[8][];

                    for (int i = 0; i < 8; ++i)
                    {
                        _lists[i] = new HuedTileList[8];

                        for (int j = 0; j < 8; ++j)
                        {
                            _lists[i][j] = new HuedTileList();
                        }
                    }
                }

                HuedTileList[][] lists = _lists;

                for (int i = 0; i < count; ++i)
                {
                    var ptr = new IntPtr((long)gc.AddrOfPinnedObject() + (i * sizeof(StaticTile)));
                    var cur = (StaticTile)Marshal.PtrToStructure(ptr, typeof(StaticTile));
                    lists[cur.X & 0x7][cur.Y & 0x7].Add(Art.GetLegalItemId(cur.Id), cur.Hue, cur.Z);
                }

                var tiles = new HuedTile[8][][];

                for (int i = 0; i < 8; ++i)
                {
                    tiles[i] = new HuedTile[8][];

                    for (int j = 0; j < 8; ++j)
                    {
                        tiles[i][j] = lists[i][j].ToArray();
                    }
                }

                return tiles;
            }
            finally
            {
                gc.Free();
            }
        }

        /*
         * UOP map files support code, written by Wyatt (c) www.ruosi.org
         * It's not possible if some entry has unknown hash. Thrown exception
         * means that EA changed maps UOPs again.
         */
        public bool IsUOPFormat { get; set; }
        public bool IsUOPAlreadyRead { get; set; }

        private readonly struct UopFile
        {
            public readonly long Offset;
            public readonly int Length;

            public UopFile(long offset, int length)
            {
                Offset = offset;
                Length = length;
            }
        }

        private UopFile[] UOPFiles { get; set; }
        private long UOPLength { get { return _map.Length; } }

        private void ReadUOPFiles(string pattern)
        {
            _uopReader = new BinaryReader(_map);

            _uopReader.BaseStream.Seek(0, SeekOrigin.Begin);

            if (_uopReader.ReadInt32() != 0x50594D)
            {
                throw new ArgumentException("Bad UOP file.");
            }

            _uopReader.ReadInt64(); // version + signature
            long nextBlock = _uopReader.ReadInt64();
            _uopReader.ReadInt32(); // block capacity
            int count = _uopReader.ReadInt32();

            UOPFiles = new UopFile[count];

            var hashes = new Dictionary<ulong, int>();

            for (int i = 0; i < count; i++)
            {
                string file = $"build/{pattern}/{i:D8}.dat";
                ulong hash = UopUtils.HashFileName(file);

                hashes.TryAdd(hash, i);
            }

            _uopReader.BaseStream.Seek(nextBlock, SeekOrigin.Begin);

            do
            {
                int filesCount = _uopReader.ReadInt32();
                nextBlock = _uopReader.ReadInt64();

                for (int i = 0; i < filesCount; i++)
                {
                    long offset = _uopReader.ReadInt64();
                    int headerLength = _uopReader.ReadInt32();
                    int compressedLength = _uopReader.ReadInt32();
                    int decompressedLength = _uopReader.ReadInt32();
                    ulong hash = _uopReader.ReadUInt64();
                    _uopReader.ReadUInt32(); // Adler32
                    short flag = _uopReader.ReadInt16();

                    int length = flag == 1 ? compressedLength : decompressedLength;

                    if (offset == 0)
                    {
                        continue;
                    }

                    if (hashes.TryGetValue(hash, out int idx))
                    {
                        if (idx < 0 || idx > UOPFiles.Length)
                        {
                            throw new IndexOutOfRangeException("hashes dictionary and files collection have different count of entries!");
                        }

                        UOPFiles[idx] = new UopFile(offset + headerLength, length);
                    }
                    else
                    {
                        throw new ArgumentException($"File with hash 0x{hash:X8} was not found in hashes dictionary! EA Mythic changed UOP format!");
                    }
                }
            }
            while (_uopReader.BaseStream.Seek(nextBlock, SeekOrigin.Begin) != 0);
        }

        private long CalculateOffsetFromUOP(long offset)
        {
            long pos = 0;

            foreach (UopFile t in UOPFiles)
            {
                long currentPosition = pos + t.Length;

                if (offset < currentPosition)
                {
                    return t.Offset + (offset - pos);
                }

                pos = currentPosition;
            }

            return UOPLength;
        }

        private Tile[] ReadLandBlock(int x, int y)
        {
            if (_map?.CanRead != true || !_map.CanSeek)
            {
                _map = _mapPath == null
                    ? null
                    : new FileStream(_mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                if (IsUOPFormat && _mapPath != null && !IsUOPAlreadyRead)
                {
                    var fi = new FileInfo(_mapPath);
                    string uopPattern = fi.Name.Replace(fi.Extension, "").ToLowerInvariant();

                    ReadUOPFiles(uopPattern);
                    IsUOPAlreadyRead = true;
                }
            }

            var tiles = new Tile[64];
            if (_map == null)
            {
                return tiles;
            }

            long offset = (((x * BlockHeight) + y) * 196) + 4;

            if (IsUOPFormat)
            {
                offset = CalculateOffsetFromUOP(offset);
            }

            _map.Seek(offset, SeekOrigin.Begin);

            GCHandle gc = GCHandle.Alloc(tiles, GCHandleType.Pinned);
            try
            {
                if (_buffer == null || _buffer.Length < 192)
                {
                    _buffer = new byte[192];
                }

                _map.Read(_buffer, 0, 192);

                Marshal.Copy(_buffer, 0, gc.AddrOfPinnedObject(), 192);
            }
            finally
            {
                gc.Free();
            }

            return tiles;
        }

        #region In-memory editing (used by the region editor)

        /// <summary>
        /// Reads a land block from the file without putting it into the cache (cached/changed blocks are returned as they are).
        /// </summary>
        public Tile[] ReadLandBlockUncached(int x, int y)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
            {
                return InvalidLandBlock;
            }

            return _landTiles[x]?[y] ?? ReadLandBlock(x, y);
        }

        /// <summary>
        /// Replaces a land block in memory and marks it as modified (written by <see cref="Map.SaveLand"/>).
        /// </summary>
        public void SetModifiedLandBlock(int x, int y, Tile[] tiles)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight || tiles == null || tiles.Length < 64)
            {
                return;
            }

            SetLandBlock(x, y, tiles);
            _modifiedLandBlocks.Add(BlockKey(x, y));
        }

        /// <summary>
        /// Drops an in-memory land change, the block is read from the file again.
        /// </summary>
        public void RevertLandBlock(int x, int y)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
            {
                return;
            }

            _modifiedLandBlocks.Remove(BlockKey(x, y));
            if (_landTiles[x] != null)
            {
                _landTiles[x][y] = null;
            }
        }

        public bool IsLandBlockModified(int x, int y)
        {
            return _modifiedLandBlocks.Count > 0 && _modifiedLandBlocks.Contains(BlockKey(x, y));
        }

        public bool HasModifiedLand => _modifiedLandBlocks.Count > 0;

        public bool HasStaticOverrides => _staticOverrides.Count > 0;

        /// <summary>
        /// Returns all statics of a block (file/patch + pending) as <see cref="StaticTile"/> list with block-relative X/Y.
        /// </summary>
        public List<StaticTile> GetStaticBlockTiles(int x, int y, bool patch = true)
        {
            var list = new List<StaticTile>();
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
            {
                return list;
            }

            if (_staticOverrides.TryGetValue(BlockKey(x, y), out StaticTile[] replaced))
            {
                list.AddRange(replaced);
                return list;
            }

            HuedTile[][][] block = GetStaticBlock(x, y, patch);
            for (int bx = 0; bx < 8; ++bx)
            {
                for (int by = 0; by < 8; ++by)
                {
                    foreach (HuedTile t in block[bx][by])
                    {
                        list.Add(new StaticTile { Id = t.Id, X = (byte)bx, Y = (byte)by, Z = t.Z, Hue = (short)t.Hue });
                    }
                }
            }

            StaticTile[] pending = GetPendingStatics(x, y);
            if (pending != null)
            {
                list.AddRange(pending);
            }

            return list;
        }

        /// <summary>
        /// Replaces all statics of a block in memory. Pending statics of the block are merged into the replacement
        /// by the caller (see <see cref="GetStaticBlockTiles"/>) and dropped here. <see cref="Map.DefragStatics"/> writes the replacement.
        /// </summary>
        public void SetStaticBlockOverride(int x, int y, StaticTile[] tiles)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
            {
                return;
            }

            tiles ??= Array.Empty<StaticTile>();
            _staticOverrides[BlockKey(x, y)] = tiles;

            var lists = new List<HuedTile>[8][];
            for (int i = 0; i < 8; ++i)
            {
                lists[i] = new List<HuedTile>[8];
            }

            foreach (StaticTile t in tiles)
            {
                int bx = t.X & 0x7;
                int by = t.Y & 0x7;
                (lists[bx][by] ??= new List<HuedTile>()).Add(new HuedTile(t.Id, t.Hue, t.Z));
            }

            var block = new HuedTile[8][][];
            for (int bx = 0; bx < 8; ++bx)
            {
                block[bx] = new HuedTile[8][];
                for (int by = 0; by < 8; ++by)
                {
                    block[bx][by] = lists[bx][by]?.ToArray() ?? Array.Empty<HuedTile>();
                }
            }

            if (_staticTiles[x] == null)
            {
                _staticTiles[x] = new HuedTile[BlockHeight][][][];
            }

            _staticTiles[x][y] = block;

            if (_staticTilesToAdd?[y] != null)
            {
                _staticTilesToAdd[y][x] = null;
            }

            if (_removedStaticBlock?[x] != null)
            {
                _removedStaticBlock[x][y] = false;
            }
        }

        public bool HasStaticBlockOverride(int x, int y)
        {
            return _staticOverrides.Count > 0 && _staticOverrides.ContainsKey(BlockKey(x, y));
        }

        public StaticTile[] GetStaticBlockOverride(int x, int y)
        {
            return _staticOverrides.TryGetValue(BlockKey(x, y), out StaticTile[] tiles) ? tiles : null;
        }

        /// <summary>
        /// Drops an in-memory static replacement, the block is read from the file again.
        /// </summary>
        public void RevertStaticBlock(int x, int y)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
            {
                return;
            }

            _staticOverrides.Remove(BlockKey(x, y));
            if (_staticTiles[x] != null)
            {
                _staticTiles[x][y] = null;
            }
        }

        #endregion

        public void RemoveStaticBlock(int blockX, int blockY)
        {
            if (_removedStaticBlock == null)
            {
                _removedStaticBlock = new bool[BlockWidth][];
            }

            if (_removedStaticBlock[blockX] == null)
            {
                _removedStaticBlock[blockX] = new bool[BlockHeight];
            }

            _removedStaticBlock[blockX][blockY] = true;

            if (_staticTiles[blockX] == null)
            {
                _staticTiles[blockX] = new HuedTile[BlockHeight][][][];
            }

            _staticTiles[blockX][blockY] = EmptyStaticBlock;
        }

        public bool IsStaticBlockRemoved(int blockX, int blockY)
        {
            if (_removedStaticBlock?[blockX] == null)
            {
                return false;
            }

            return _removedStaticBlock[blockX][blockY];
        }

        public bool PendingStatic(int blockX, int blockY)
        {
            if (_staticTilesToAdd?[blockY] == null)
            {
                return false;
            }

            if (_staticTilesToAdd[blockY][blockX] == null)
            {
                return false;
            }

            return true;
        }

        public void AddPendingStatic(int blockX, int blockY, StaticTile toAdd)
        {
            if (_staticTilesToAdd == null)
            {
                _staticTilesToAdd = new List<StaticTile>[BlockHeight][];
            }

            if (_staticTilesToAdd[blockY] == null)
            {
                _staticTilesToAdd[blockY] = new List<StaticTile>[BlockWidth];
            }

            if (_staticTilesToAdd[blockY][blockX] == null)
            {
                _staticTilesToAdd[blockY][blockX] = new List<StaticTile>();
            }

            _staticTilesToAdd[blockY][blockX].Add(toAdd);
        }

        public StaticTile[] GetPendingStatics(int blockX, int blockY)
        {
            if (_staticTilesToAdd?[blockY] == null)
            {
                return null;
            }

            if (_staticTilesToAdd[blockY][blockX] == null)
            {
                return null;
            }

            return _staticTilesToAdd[blockY][blockX].ToArray();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticTile
    {
        public ushort Id;
        public byte X;
        public byte Y;
        public sbyte Z;
        public short Hue;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HuedTile
    {
        public ushort Id { get; set; }

        public int Hue { get; set; }

        public sbyte Z { get; set; }

        public HuedTile(ushort id, short hue, sbyte z)
        {
            Id = id;
            Hue = hue;
            Z = z;
        }
    }

    public struct MTile : IComparable
    {
        public ushort Id { get; internal set; }
        public sbyte Z { get; set; }

        public sbyte Flag { get; set; }

        public int Unk1 { get; set; }

        public int Solver { get; set; }

        public MTile(ushort id, sbyte z)
        {
            Id = Art.GetLegalItemId(id);
            Z = z;
            Flag = 1;
            Solver = 0;
            Unk1 = 0;
        }

        public MTile(ushort id, sbyte z, sbyte flag)
        {
            Id = Art.GetLegalItemId(id);
            Z = z;
            Flag = flag;
            Solver = 0;
            Unk1 = 0;
        }

        public MTile(ushort id, sbyte z, sbyte flag, int unk1)
        {
            Id = Art.GetLegalItemId(id);
            Z = z;
            Flag = flag;
            Solver = 0;
            Unk1 = unk1;
        }

        public void Set(ushort id, sbyte z)
        {
            Id = Art.GetLegalItemId(id);
            Z = z;
        }

        public void Set(ushort id, sbyte z, sbyte flag)
        {
            Id = Art.GetLegalItemId(id);
            Z = z;
            Flag = flag;
        }

        public void Set(ushort id, sbyte z, sbyte flag, int unk1)
        {
            Id = Art.GetLegalItemId(id);
            Z = z;
            Flag = flag;
            Unk1 = unk1;
        }

        public int CompareTo(object x)
        {
            if (x == null)
            {
                return 1;
            }

            if (!(x is MTile))
            {
                throw new ArgumentNullException();
            }

            var a = (MTile)x;

            ItemData ourData = TileData.ItemTable[Id];
            ItemData theirData = TileData.ItemTable[a.Id];

            int ourThreshold = 0;
            if (ourData.Height > 0)
            {
                ++ourThreshold;
            }

            if (!ourData.Background)
            {
                ++ourThreshold;
            }

            int ourZ = Z;
            int theirThreshold = 0;
            if (theirData.Height > 0)
            {
                ++theirThreshold;
            }

            if (!theirData.Background)
            {
                ++theirThreshold;
            }

            int theirZ = a.Z;

            ourZ += ourThreshold;
            theirZ += theirThreshold;
            int res = ourZ - theirZ;
            if (res == 0)
            {
                res = ourThreshold - theirThreshold;
            }

            if (res == 0)
            {
                res = Solver - a.Solver;
            }

            return res;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tile : IComparable
    {
        public ushort Id { get; internal set; }

        public sbyte Z { get; set; }

        public Tile(ushort id, sbyte z)
        {
            Id = id;
            Z = z;
        }

        public void Set(ushort id, sbyte z)
        {
            Id = id;
            Z = z;
        }

        public int CompareTo(object x)
        {
            if (x == null)
            {
                return 1;
            }

            if (!(x is Tile))
            {
                throw new ArgumentNullException();
            }

            var a = (Tile)x;

            if (Z > a.Z)
            {
                return 1;
            }

            if (a.Z > Z)
            {
                return -1;
            }

            ItemData ourData = TileData.ItemTable[Id];
            ItemData theirData = TileData.ItemTable[a.Id];

            if (ourData.Height > theirData.Height)
            {
                return 1;
            }

            if (theirData.Height > ourData.Height)
            {
                return -1;
            }

            if (ourData.Background && !theirData.Background)
            {
                return -1;
            }

            if (theirData.Background && !ourData.Background)
            {
                return 1;
            }

            return 0;
        }
    }
}
