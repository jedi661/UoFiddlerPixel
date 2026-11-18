using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Features (<see cref="ArtFeatures"/>) of every static in art.mul (+ extension) for "find similar items".
    /// Built once in small portions on the UI thread (Art is not thread safe) and cached in
    /// AppData\ArtFeatures.bin - the cache is rebuilt automatically when art.mul / artext.mul change.
    /// </summary>
    public sealed class ArtSimilarityIndex
    {
        private const int ItemsPerTick = 300;
        private static readonly byte[] _magic = Encoding.ASCII.GetBytes("UOAF");
        private const int CacheVersion = 1;

        public static ArtSimilarityIndex Instance { get; } = new ArtSimilarityIndex();

        private readonly Dictionary<int, ArtFeatures> _items = new Dictionary<int, ArtFeatures>();
        private List<int> _todo;
        private int _todoPos;
        private Timer _timer;
        private string _key;

        public bool IsReady { get; private set; }
        public bool IsBuilding => _timer != null;
        public int Done => _todoPos;
        public int Total => _todo?.Count ?? 0;

        /// <summary>Raised on the UI thread while building (progress) and when ready.</summary>
        public event Action Changed;

        private static string CachePath => Path.Combine(Options.AppDataPath ?? Path.GetTempPath(), "ArtFeatures.bin");

        /// <summary>Identifies the art files the index belongs to (path, size, time of art + extension).</summary>
        private static string CurrentKey()
        {
            var sb = new StringBuilder();
            foreach (string name in new[] { "art.mul", "artidx.mul", "artlegacymul.uop", ExtendedFormat.ArtIndexFileName, ExtendedFormat.ArtDataFileName })
            {
                string p = Files.GetFilePath(name);
                if (p == null)
                {
                    continue;
                }

                var fi = new FileInfo(p);
                sb.Append(fi.FullName).Append('|').Append(fi.Length).Append('|').Append(fi.LastWriteTimeUtc.Ticks).Append(';');
            }

            return sb.ToString();
        }

        /// <summary>Loads the cache or starts building. Safe to call repeatedly.</summary>
        public void EnsureBuilt()
        {
            string key = CurrentKey();
            if ((IsReady || IsBuilding) && key == _key)
            {
                return;
            }

            Stop();
            _items.Clear();
            IsReady = false;
            _key = key;

            if (TryLoadCache(key))
            {
                IsReady = true;
                Changed?.Invoke();
                return;
            }

            _todo = new List<int>();
            for (int id = 0; id <= Art.GetMaxItemId(); ++id)
            {
                _todo.Add(id);
            }

            if (Art.HasExtension)
            {
                _todo.AddRange(Art.GetUsedExtendedIds());
            }

            _todoPos = 0;
            _timer = new Timer { Interval = 1 };
            _timer.Tick += (s, e) => BuildStep();
            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        private void BuildStep()
        {
            int end = Math.Min(_todo.Count, _todoPos + ItemsPerTick);
            for (; _todoPos < end; ++_todoPos)
            {
                int id = _todo[_todoPos];
                try
                {
                    ArtPixelImage img = ArtDirectionLearning.DecodeStaticRaw(Art.GetRawStatic(id));
                    if (img != null)
                    {
                        _items[id] = ArtFeatures.Compute(img, id);
                    }
                }
                catch (Exception)
                {
                    // a broken entry must not stop the index
                }
            }

            if (_todoPos >= _todo.Count)
            {
                Stop();
                IsReady = true;
                SaveCache();
            }

            Changed?.Invoke();
        }

        /// <summary>The <paramref name="max"/> most similar items (any facing), without <paramref name="excludeId"/>.</summary>
        public List<(ArtFeatures Item, double Distance)> FindSimilar(ArtFeatures query, int max, int excludeId = -1)
        {
            return _items.Values
                .Where(f => f.Id != excludeId)
                .Select(f => (Item: f, Distance: ArtFeatures.DistanceAnyFacing(query, f)))
                .OrderBy(t => t.Distance)
                .Take(max)
                .ToList();
        }

        /// <summary>
        /// Direction variants of an item that exist in art.mul: neighbouring ids (+-4) with the same TileData name
        /// and a similar look (in any facing), as one contiguous group.
        /// </summary>
        public List<int> GetGroup(int id)
        {
            var group = new List<int> { id };
            string name = GetName(id);
            if (!_items.TryGetValue(id, out ArtFeatures f))
            {
                return group;
            }

            foreach (int step in new[] { -1, 1 })
            {
                for (int n = id + step, gap = 0; Math.Abs(n - id) <= 4 && gap < 1; n += step)
                {
                    bool ok = _items.TryGetValue(n, out ArtFeatures g) &&
                              string.Equals(GetName(n), name, StringComparison.OrdinalIgnoreCase) &&
                              ArtFeatures.DistanceAnyFacing(f, g) < 0.9;
                    if (ok)
                    {
                        group.Add(n);
                    }
                    else
                    {
                        ++gap;
                    }
                }
            }

            group.Sort();
            return group;
        }

        public static string GetName(int id)
        {
            return TileData.ItemTable != null && id >= 0 && id < TileData.ItemTable.Length
                ? (TileData.ItemTable[id].Name ?? string.Empty).Trim()
                : string.Empty;
        }

        #region Cache
        private bool TryLoadCache(string key)
        {
            try
            {
                if (!File.Exists(CachePath))
                {
                    return false;
                }

                using var br = new BinaryReader(File.OpenRead(CachePath), Encoding.UTF8);
                if (!br.ReadBytes(4).SequenceEqual(_magic) || br.ReadInt32() != CacheVersion || br.ReadString() != key)
                {
                    return false;
                }

                int count = br.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    ArtFeatures f = ArtFeatures.Read(br);
                    _items[f.Id] = f;
                }

                return true;
            }
            catch (Exception)
            {
                _items.Clear();
                return false;
            }
        }

        private void SaveCache()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CachePath));
                using var bw = new BinaryWriter(File.Create(CachePath), Encoding.UTF8);
                bw.Write(_magic);
                bw.Write(CacheVersion);
                bw.Write(_key ?? string.Empty);
                bw.Write(_items.Count);
                foreach (ArtFeatures f in _items.Values)
                {
                    f.Write(bw);
                }
            }
            catch (Exception)
            {
                // no cache - it is built again next time
            }
        }
        #endregion
    }
}
