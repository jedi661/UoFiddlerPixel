// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System.Collections.Generic;
using System.Drawing;

namespace UoFiddler.Controls.TerrainLibrary.Core
{
    /// <summary>
    /// Keeps the most recently used decoded graphics of one layer in memory (least recently used
    /// entries are dropped first). Bitmaps handed out are shared: do not dispose or modify them.
    /// </summary>
    public sealed class TerrainCache
    {
        private readonly Dictionary<int, LinkedListNode<KeyValuePair<int, Bitmap>>> _map =
            new Dictionary<int, LinkedListNode<KeyValuePair<int, Bitmap>>>();

        private readonly LinkedList<KeyValuePair<int, Bitmap>> _order = new LinkedList<KeyValuePair<int, Bitmap>>();

        public TerrainCache(int capacity)
        {
            Capacity = capacity < 16 ? 16 : capacity;
        }

        public int Capacity { get; }

        public int Count => _map.Count;

        public bool TryGet(int id, out Bitmap bitmap)
        {
            if (_map.TryGetValue(id, out LinkedListNode<KeyValuePair<int, Bitmap>> node))
            {
                _order.Remove(node);
                _order.AddFirst(node);
                bitmap = node.Value.Value;
                return true;
            }

            bitmap = null;
            return false;
        }

        public void Add(int id, Bitmap bitmap)
        {
            Remove(id);
            var node = new LinkedListNode<KeyValuePair<int, Bitmap>>(new KeyValuePair<int, Bitmap>(id, bitmap));
            _order.AddFirst(node);
            _map[id] = node;

            while (_map.Count > Capacity)
            {
                LinkedListNode<KeyValuePair<int, Bitmap>> last = _order.Last;
                _order.RemoveLast();
                _map.Remove(last.Value.Key);
            }
        }

        public void Remove(int id)
        {
            if (_map.TryGetValue(id, out LinkedListNode<KeyValuePair<int, Bitmap>> node))
            {
                _order.Remove(node);
                _map.Remove(id);
            }
        }

        public void Clear()
        {
            _map.Clear();
            _order.Clear();
        }
    }
}
