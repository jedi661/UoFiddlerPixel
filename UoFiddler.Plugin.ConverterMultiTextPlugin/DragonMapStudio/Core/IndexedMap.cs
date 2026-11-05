// /***************************************************************************
//  *
//  * Dragon Map Studio – 8-bit map in memory + undo / redo
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>The map as Dragon sees it: one byte (palette index) per tile.</summary>
    public sealed class IndexedMap
    {
        public IndexedMap(int width, int height, byte fill = 0)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Die Kartengröße muss größer als 0 sein.");
            }
            Width = width;
            Height = height;
            Pixels = new byte[width * height];
            if (fill != 0)
            {
                Array.Fill(Pixels, fill);
            }
        }

        public IndexedMap(int width, int height, byte[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public int Width { get; }
        public int Height { get; }
        public byte[] Pixels { get; }

        /// <summary>Full path of the file the map was loaded from / saved to (null = new map).</summary>
        public string FilePath { get; set; }

        public bool Modified { get; set; }

        public byte this[int x, int y]
        {
            get => Pixels[y * Width + x];
            set => Pixels[y * Width + x] = value;
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public Rectangle Bounds => new Rectangle(0, 0, Width, Height);

        public IndexedMap Clone()
        {
            return new IndexedMap(Width, Height, (byte[])Pixels.Clone()) { FilePath = FilePath };
        }

        /// <summary>Copies a rectangle into a new map (for copy / paste).</summary>
        public IndexedMap Crop(Rectangle r)
        {
            r.Intersect(Bounds);
            if (r.Width <= 0 || r.Height <= 0)
            {
                return null;
            }
            var result = new IndexedMap(r.Width, r.Height);
            for (int y = 0; y < r.Height; y++)
            {
                Buffer.BlockCopy(Pixels, (r.Y + y) * Width + r.X, result.Pixels, y * r.Width, r.Width);
            }
            return result;
        }

        /// <summary>Histogram of the palette indices.</summary>
        public int[] CountIndices()
        {
            var counts = new int[256];
            foreach (byte b in Pixels)
            {
                counts[b]++;
            }
            return counts;
        }
    }

    /// <summary>
    /// Undo / redo with copy-on-write tiles (64×64). A brush stroke only stores the touched tiles,
    /// a generator run stores the whole map once. Old steps are dropped when the memory limit is reached.
    /// </summary>
    public sealed class MapHistory
    {
        private const int TileSize = 64;

        private sealed class Step
        {
            public string Name;
            public readonly Dictionary<int, byte[]> Before = new Dictionary<int, byte[]>();
            public readonly Dictionary<int, byte[]> After = new Dictionary<int, byte[]>();
            public long Bytes;
        }

        private readonly LinkedList<Step> _undo = new LinkedList<Step>();
        private readonly Stack<Step> _redo = new Stack<Step>();
        private Step _current;
        private IndexedMap _map;
        private int _tilesX;
        private int _tilesY;

        public long MemoryLimit { get; set; } = 768L * 1024 * 1024;

        public event EventHandler Changed;

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;
        public string UndoName => _undo.Count > 0 ? _undo.Last.Value.Name : string.Empty;
        public string RedoName => _redo.Count > 0 ? _redo.Peek().Name : string.Empty;
        public bool IsRecording => _current != null;

        public void Attach(IndexedMap map)
        {
            _map = map;
            _undo.Clear();
            _redo.Clear();
            _current = null;
            if (map != null)
            {
                _tilesX = (map.Width + TileSize - 1) / TileSize;
                _tilesY = (map.Height + TileSize - 1) / TileSize;
            }
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Begin(string name)
        {
            if (_map == null)
            {
                return;
            }
            if (_current != null)
            {
                End();
            }
            _current = new Step { Name = name };
        }

        /// <summary>Call before pixels inside the rectangle are changed.</summary>
        public void Touch(Rectangle r)
        {
            if (_current == null || _map == null)
            {
                return;
            }
            r.Intersect(_map.Bounds);
            if (r.Width <= 0 || r.Height <= 0)
            {
                return;
            }
            int tx0 = r.Left / TileSize, ty0 = r.Top / TileSize;
            int tx1 = (r.Right - 1) / TileSize, ty1 = (r.Bottom - 1) / TileSize;
            for (int ty = ty0; ty <= ty1; ty++)
            {
                for (int tx = tx0; tx <= tx1; tx++)
                {
                    int key = ty * _tilesX + tx;
                    if (!_current.Before.ContainsKey(key))
                    {
                        _current.Before[key] = CopyTile(tx, ty);
                    }
                }
            }
        }

        public void Touch(int x, int y) => Touch(new Rectangle(x, y, 1, 1));

        public void TouchAll() => Touch(_map?.Bounds ?? Rectangle.Empty);

        public void End()
        {
            Step step = _current;
            _current = null;
            if (step == null || step.Before.Count == 0)
            {
                return;
            }
            bool changed = false;
            foreach (KeyValuePair<int, byte[]> kv in step.Before)
            {
                byte[] after = CopyTile(kv.Key % _tilesX, kv.Key / _tilesX);
                step.After[kv.Key] = after;
                step.Bytes += kv.Value.Length * 2L;
                if (!changed && !after.AsSpan().SequenceEqual(kv.Value))
                {
                    changed = true;
                }
            }
            if (!changed)
            {
                return;
            }
            _undo.AddLast(step);
            _redo.Clear();
            Trim();
            _map.Modified = true;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Cancel()
        {
            if (_current == null)
            {
                return;
            }
            foreach (KeyValuePair<int, byte[]> kv in _current.Before)
            {
                WriteTile(kv.Key % _tilesX, kv.Key / _tilesX, kv.Value);
            }
            _current = null;
        }

        /// <summary>Undo the last step. Returns the changed area (empty if nothing happened).</summary>
        public Rectangle Undo()
        {
            if (_current != null)
            {
                End();
            }
            if (_undo.Count == 0)
            {
                return Rectangle.Empty;
            }
            Step step = _undo.Last.Value;
            _undo.RemoveLast();
            Rectangle area = Apply(step.Before);
            _redo.Push(step);
            _map.Modified = true;
            Changed?.Invoke(this, EventArgs.Empty);
            return area;
        }

        public Rectangle Redo()
        {
            if (_redo.Count == 0)
            {
                return Rectangle.Empty;
            }
            Step step = _redo.Pop();
            Rectangle area = Apply(step.After);
            _undo.AddLast(step);
            _map.Modified = true;
            Changed?.Invoke(this, EventArgs.Empty);
            return area;
        }

        private Rectangle Apply(Dictionary<int, byte[]> tiles)
        {
            Rectangle area = Rectangle.Empty;
            foreach (KeyValuePair<int, byte[]> kv in tiles)
            {
                int tx = kv.Key % _tilesX, ty = kv.Key / _tilesX;
                WriteTile(tx, ty, kv.Value);
                var r = new Rectangle(tx * TileSize, ty * TileSize, TileSize, TileSize);
                area = area.IsEmpty ? r : Rectangle.Union(area, r);
            }
            area.Intersect(_map.Bounds);
            return area;
        }

        private void Trim()
        {
            long total = 0;
            foreach (Step s in _undo)
            {
                total += s.Bytes;
            }
            while (_undo.Count > 1 && total > MemoryLimit)
            {
                total -= _undo.First.Value.Bytes;
                _undo.RemoveFirst();
            }
        }

        private byte[] CopyTile(int tx, int ty)
        {
            int x0 = tx * TileSize, y0 = ty * TileSize;
            int w = Math.Min(TileSize, _map.Width - x0), h = Math.Min(TileSize, _map.Height - y0);
            var data = new byte[w * h];
            for (int y = 0; y < h; y++)
            {
                Buffer.BlockCopy(_map.Pixels, (y0 + y) * _map.Width + x0, data, y * w, w);
            }
            return data;
        }

        private void WriteTile(int tx, int ty, byte[] data)
        {
            int x0 = tx * TileSize, y0 = ty * TileSize;
            int w = Math.Min(TileSize, _map.Width - x0), h = Math.Min(TileSize, _map.Height - y0);
            for (int y = 0; y < h; y++)
            {
                Buffer.BlockCopy(data, y * w, _map.Pixels, (y0 + y) * _map.Width + x0, w);
            }
        }
    }
}
