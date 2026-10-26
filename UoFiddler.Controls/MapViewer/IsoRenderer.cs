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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Ultima;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Settings that change the rendered chunks (a change needs <see cref="IsoRenderer.Invalidate"/>).
    /// </summary>
    public sealed class IsoRenderSettings
    {
        public bool ShowLand { get; set; } = true;
        public bool ShowStatics { get; set; } = true;

        /// <summary>Draw hills with stretched texmaps (like the client), otherwise flat land art</summary>
        public bool StretchLand { get; set; } = true;

        /// <summary>Simple light on stretched land</summary>
        public bool Lighting { get; set; } = true;

        /// <summary>Colors the land by its height</summary>
        public bool HeightColors { get; set; }

        /// <summary>Show tiles that the client does not draw (nodraw)</summary>
        public bool ShowNoDraw { get; set; }

        /// <summary>Statics above this z are hidden (look into buildings)</summary>
        public int MaxZ { get; set; } = 127;

        public Color Background { get; set; } = Color.FromArgb(18, 18, 22);

        public IsoRenderSettings Clone()
        {
            return (IsoRenderSettings)MemberwiseClone();
        }
    }

    /// <summary>
    /// Interface of a region renderer. The isometric GDI+/software renderer implements it,
    /// a future 3D renderer (e.g. OpenGL via OpenTK) can implement the same contract on top of <see cref="RegionData"/>.
    /// </summary>
    public interface IRegionRenderer : IDisposable
    {
        RegionData Data { get; }
        IsoRenderSettings Settings { get; }

        /// <summary>Drops all cached images (after data or settings changed)</summary>
        void Invalidate();
    }

    /// <summary>
    /// Software renderer for the isometric client view. The canvas is split into chunks of
    /// <see cref="RegionData.ChunkSize"/> pixels which are rendered on demand and cached (memory LRU,
    /// optional disk cache). Zoomed out views use downscaled chunks (level of detail).
    /// </summary>
    public sealed class IsoRenderer : IRegionRenderer
    {
        public const int MaxLod = 3;
        private const int Size = RegionData.ChunkSize;
        private const int ChunkBytes = Size * Size * 4;

        private readonly Dictionary<long, LinkedListNode<CacheEntry>> _cache = new Dictionary<long, LinkedListNode<CacheEntry>>();
        private readonly LinkedList<CacheEntry> _lru = new LinkedList<CacheEntry>();
        private readonly HashSet<long> _onDisk = new HashSet<long>();
        private readonly int[] _work = new int[Size * Size];
        private string _diskDirectory;
        private bool _useDiskCache;

        private sealed class CacheEntry
        {
            public long Key;
            public Bitmap Bitmap;
        }

        public IsoRenderer(RegionData data, ArtPixelCache art, IsoRenderSettings settings)
        {
            Data = data;
            Art = art;
            Settings = settings;
        }

        public RegionData Data { get; }
        public ArtPixelCache Art { get; }
        public IsoRenderSettings Settings { get; }

        /// <summary>Memory budget of the chunk cache in bytes</summary>
        public long MemoryBudget { get; set; } = 256L * 1024 * 1024;

        /// <summary>Chunks removed from memory are written to a temp directory and loaded again from there</summary>
        public bool UseDiskCache
        {
            get => _useDiskCache;
            set
            {
                _useDiskCache = value;
                if (!value)
                {
                    ClearDiskCache();
                }
            }
        }

        public int CachedChunks => _cache.Count;
        public long CachedBytes => (long)_cache.Count * ChunkBytes;
        public int DiskChunks => _onDisk.Count;

        /// <summary>Number of chunks rendered since the last reset of the counter</summary>
        public int RenderedChunks { get; set; }

        #region LOD helpers

        public static int LodForZoom(double zoom)
        {
            if (zoom >= 0.75)
            {
                return 0;
            }

            if (zoom >= 0.375)
            {
                return 1;
            }

            return zoom >= 0.1875 ? 2 : 3;
        }

        public int Columns(int lod)
        {
            int span = Size << lod;
            return Math.Max(1, (Data.CanvasSize.Width + span - 1) / span);
        }

        public int Rows(int lod)
        {
            int span = Size << lod;
            return Math.Max(1, (Data.CanvasSize.Height + span - 1) / span);
        }

        private static long Key(int lod, int cx, int cy)
        {
            return ((long)lod << 48) | ((long)(cx & 0xFFFFFF) << 24) | (uint)(cy & 0xFFFFFF);
        }

        #endregion

        #region Cache

        /// <summary>
        /// Returns a cached chunk (memory or disk) or null
        /// </summary>
        public Bitmap TryGetChunk(int lod, int cx, int cy)
        {
            long key = Key(lod, cx, cy);
            if (_cache.TryGetValue(key, out LinkedListNode<CacheEntry> node))
            {
                _lru.Remove(node);
                _lru.AddFirst(node);
                return node.Value.Bitmap;
            }

            if (_useDiskCache && _onDisk.Contains(key))
            {
                Bitmap bmp = LoadFromDisk(key);
                if (bmp != null)
                {
                    AddToCache(key, bmp);
                    return bmp;
                }
            }

            return null;
        }

        /// <summary>
        /// Makes sure a chunk exists. Returns false if the time budget ran out before (child chunks are kept).
        /// </summary>
        public bool EnsureChunk(int lod, int cx, int cy, Stopwatch watch, long budgetMs)
        {
            if (TryGetChunk(lod, cx, cy) != null)
            {
                return true;
            }

            if (lod == 0)
            {
                RenderBaseChunk(cx, cy);
                return true;
            }

            // children of the next better level
            for (int i = 0; i < 4; i++)
            {
                int ccx = (cx * 2) + (i & 1);
                int ccy = (cy * 2) + (i >> 1);
                if (ccx >= Columns(lod - 1) || ccy >= Rows(lod - 1))
                {
                    continue;
                }

                if (!EnsureChunk(lod - 1, ccx, ccy, watch, budgetMs))
                {
                    return false;
                }

                if (watch.ElapsedMilliseconds > budgetMs && i < 3)
                {
                    return false;
                }
            }

            ComposeChunk(lod, cx, cy);
            return true;
        }

        private void AddToCache(long key, Bitmap bmp)
        {
            var node = new LinkedListNode<CacheEntry>(new CacheEntry { Key = key, Bitmap = bmp });
            _lru.AddFirst(node);
            _cache[key] = node;

            while (CachedBytes > MemoryBudget && _lru.Count > 4)
            {
                LinkedListNode<CacheEntry> last = _lru.Last;
                _lru.RemoveLast();
                _cache.Remove(last.Value.Key);

                if (_useDiskCache && !_onDisk.Contains(last.Value.Key))
                {
                    SaveToDisk(last.Value.Key, last.Value.Bitmap);
                }

                last.Value.Bitmap.Dispose();
            }
        }

        /// <summary>
        /// Drops all cached chunks (memory and disk)
        /// </summary>
        public void Invalidate()
        {
            foreach (CacheEntry entry in _lru)
            {
                entry.Bitmap.Dispose();
            }

            _lru.Clear();
            _cache.Clear();
            ClearDiskCache();
        }

        private void ClearDiskCache()
        {
            _onDisk.Clear();
            if (_diskDirectory == null)
            {
                return;
            }

            try
            {
                if (Directory.Exists(_diskDirectory))
                {
                    Directory.Delete(_diskDirectory, true);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            _diskDirectory = null;
        }

        private string DiskDirectory
        {
            get
            {
                if (_diskDirectory == null)
                {
                    _diskDirectory = Path.Combine(Path.GetTempPath(), "UoFiddlerPixel", "RegionViewer", Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(_diskDirectory);
                }

                return _diskDirectory;
            }
        }

        private void SaveToDisk(long key, Bitmap bmp)
        {
            try
            {
                int[] pixels = ReadPixels(bmp);
                var bytes = new byte[pixels.Length * 4];
                Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);

                string file = Path.Combine(DiskDirectory, $"{key:X16}.chunk");
                using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
                using (var deflate = new DeflateStream(fs, CompressionLevel.Fastest))
                {
                    deflate.Write(bytes, 0, bytes.Length);
                }

                _onDisk.Add(key);
            }
            catch (Exception)
            {
                // disk cache is optional
            }
        }

        private Bitmap LoadFromDisk(long key)
        {
            try
            {
                string file = Path.Combine(DiskDirectory, $"{key:X16}.chunk");
                var bytes = new byte[ChunkBytes];
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (var deflate = new DeflateStream(fs, CompressionMode.Decompress))
                {
                    int read = 0;
                    while (read < bytes.Length)
                    {
                        int n = deflate.Read(bytes, read, bytes.Length - read);
                        if (n <= 0)
                        {
                            break;
                        }

                        read += n;
                    }
                }

                var pixels = new int[Size * Size];
                Buffer.BlockCopy(bytes, 0, pixels, 0, bytes.Length);
                return CreateBitmap(pixels);
            }
            catch (Exception)
            {
                _onDisk.Remove(key);
                return null;
            }
        }

        public void Dispose()
        {
            Invalidate();
        }

        #endregion

        #region Bitmap helpers

        private static Bitmap CreateBitmap(int[] pixels)
        {
            var bmp = new Bitmap(Size, Size, PixelFormat.Format32bppPArgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, Size, Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            try
            {
                for (int y = 0; y < Size; y++)
                {
                    Marshal.Copy(pixels, y * Size, bd.Scan0 + (y * bd.Stride), Size);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            return bmp;
        }

        private static int[] ReadPixels(Bitmap bmp)
        {
            var pixels = new int[Size * Size];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, Size, Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
            try
            {
                for (int y = 0; y < Size; y++)
                {
                    Marshal.Copy(bd.Scan0 + (y * bd.Stride), pixels, y * Size, Size);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            return pixels;
        }

        #endregion

        #region Rendering

        private void ComposeChunk(int lod, int cx, int cy)
        {
            int background = Settings.Background.ToArgb();
            int[] target = new int[Size * Size];
            Array.Fill(target, background);

            for (int i = 0; i < 4; i++)
            {
                int ccx = (cx * 2) + (i & 1);
                int ccy = (cy * 2) + (i >> 1);
                if (ccx >= Columns(lod - 1) || ccy >= Rows(lod - 1))
                {
                    continue;
                }

                Bitmap child = TryGetChunk(lod - 1, ccx, ccy);
                if (child == null)
                {
                    continue;
                }

                int[] src = ReadPixels(child);
                int ox = (i & 1) * (Size / 2);
                int oy = (i >> 1) * (Size / 2);

                for (int y = 0; y < Size / 2; y++)
                {
                    int s0 = (y * 2) * Size;
                    int s1 = s0 + Size;
                    int t = ((oy + y) * Size) + ox;
                    for (int x = 0; x < Size / 2; x++)
                    {
                        int a = src[s0 + (x * 2)];
                        int b = src[s0 + (x * 2) + 1];
                        int c = src[s1 + (x * 2)];
                        int d = src[s1 + (x * 2) + 1];

                        int r = (((a >> 16) & 0xFF) + ((b >> 16) & 0xFF) + ((c >> 16) & 0xFF) + ((d >> 16) & 0xFF)) >> 2;
                        int g = (((a >> 8) & 0xFF) + ((b >> 8) & 0xFF) + ((c >> 8) & 0xFF) + ((d >> 8) & 0xFF)) >> 2;
                        int bl = ((a & 0xFF) + (b & 0xFF) + (c & 0xFF) + (d & 0xFF)) >> 2;
                        target[t + x] = unchecked((int)0xFF000000) | (r << 16) | (g << 8) | bl;
                    }
                }
            }

            AddToCache(Key(lod, cx, cy), CreateBitmap(target));
            RenderedChunks++;
        }

        private void RenderBaseChunk(int cx, int cy)
        {
            int[] buffer = _work;
            Array.Fill(buffer, Settings.Background.ToArgb());

            int chunkX = cx * Size;
            int chunkY = cy * Size;

            List<int> list = Data.GetChunkDrawables(cx, cy);
            if (list != null)
            {
                IReadOnlyList<Drawable> drawables = Data.Drawables;
                foreach (int index in list)
                {
                    Drawable d = drawables[index];
                    if (!IsVisible(d))
                    {
                        continue;
                    }

                    RegionCell cell = Data.GetCell(d.X, d.Y);
                    if (d.Kind == DrawableKind.Land)
                    {
                        DrawLand(buffer, chunkX, chunkY, cell);
                    }
                    else
                    {
                        DrawStatic(buffer, chunkX, chunkY, cell, cell.Statics[d.StaticIndex]);
                    }
                }
            }

            AddToCache(Key(0, cx, cy), CreateBitmap(buffer));
            RenderedChunks++;
        }

        /// <summary>
        /// True if the drawable is shown with the current settings
        /// </summary>
        public bool IsVisible(Drawable d)
        {
            RegionCell cell = Data.GetCell(d.X, d.Y);
            if (d.Kind == DrawableKind.Land)
            {
                return Settings.ShowLand && (Settings.ShowNoDraw || !IsNoDrawLand(cell.LandId));
            }

            if (!Settings.ShowStatics)
            {
                return false;
            }

            RegionStatic s = cell.Statics[d.StaticIndex];
            if (s.Z > Settings.MaxZ)
            {
                return false;
            }

            return Settings.ShowNoDraw || !IsNoDrawStatic(s.Id);
        }

        public static bool IsNoDrawLand(int id)
        {
            return id == 0x0002;
        }

        public static bool IsNoDrawStatic(int id)
        {
            return id == 0x0001 || id == 0x21BC || id == 0x5690;
        }

        /// <summary>
        /// True if the land of the cell is drawn as stretched texture
        /// </summary>
        public bool IsStretched(RegionCell cell)
        {
            return Settings.StretchLand && !cell.IsFlat && Art.GetTexture(cell.TextureId) != null;
        }

        private void DrawLand(int[] buffer, int chunkX, int chunkY, RegionCell cell)
        {
            int tint = Settings.HeightColors ? HeightColor(cell.AverageZ) : 0;

            if (IsStretched(cell))
            {
                ArtImage texture = Art.GetTexture(cell.TextureId);
                Point[] c = Data.GetLandCorners(cell, true);
                int t = texture.Width;

                // corners: 0 top, 1 right, 2 bottom, 3 left -> texture (0,0) (t,0) (t,t) (0,t)
                const float k = 0.125f;
                float zT = cell.LandZ * k, zR = cell.ZRight * k, zB = cell.ZBottom * k, zL = cell.ZLeft * k;

                float light1 = Settings.Lighting ? Light(-(zR - zT), -(zL - zT)) : 1f;
                float light2 = Settings.Lighting ? Light(zL - zB, zR - zB) : 1f;

                DrawTexturedTriangle(buffer, chunkX, chunkY, texture,
                    c[0], c[1], c[3], new PointF(0, 0), new PointF(t, 0), new PointF(0, t), light1, tint);
                DrawTexturedTriangle(buffer, chunkX, chunkY, texture,
                    c[1], c[2], c[3], new PointF(t, 0), new PointF(t, t), new PointF(0, t), light2, tint);
                return;
            }

            ArtImage art = Art.GetLand(cell.LandId);
            int x = Data.CanvasX(cell.X, cell.Y) - RegionData.TileHalf;
            int y = Data.CanvasY(cell.X, cell.Y, cell.LandZ);
            if (art != null)
            {
                Blit(buffer, chunkX, chunkY, art, x, y, false, tint);
            }
        }

        private void DrawStatic(int[] buffer, int chunkX, int chunkY, RegionCell cell, RegionStatic s)
        {
            ArtImage art = Art.GetStatic(s.Id, s.Hue);
            if (art == null)
            {
                return;
            }

            Point p = Data.GetStaticPosition(cell.X, cell.Y, s.Z, art.Width, art.Height);
            bool translucent = (s.Flags & TileFlag.Translucent) != 0;
            Blit(buffer, chunkX, chunkY, art, p.X, p.Y, translucent, 0);
        }

        private static float Light(float nx, float ny)
        {
            // normal (nx, ny, 1), light from north west and above
            const float lx = -0.4558f, ly = -0.4558f, lz = 0.7646f; // normalized (-1, -1, 1.6)
            float len = (float)Math.Sqrt((nx * nx) + (ny * ny) + 1);
            float dot = ((nx * lx) + (ny * ly) + lz) / len;
            float b = 1f + ((dot - 0.7646f) * 1.4f);
            return Math.Max(0.45f, Math.Min(1.35f, b));
        }

        private static int HeightColor(int z)
        {
            // -128 .. 127: blue -> green -> yellow -> red -> white
            float t = Math.Max(0, Math.Min(1, (z + 40) / 120f));
            Color[] stops = { Color.FromArgb(40, 60, 200), Color.FromArgb(40, 180, 60), Color.FromArgb(230, 220, 40), Color.FromArgb(220, 60, 30), Color.White };
            float pos = t * (stops.Length - 1);
            int i = Math.Min(stops.Length - 2, (int)pos);
            float f = pos - i;
            int r = (int)(stops[i].R + ((stops[i + 1].R - stops[i].R) * f));
            int g = (int)(stops[i].G + ((stops[i + 1].G - stops[i].G) * f));
            int b = (int)(stops[i].B + ((stops[i + 1].B - stops[i].B) * f));
            return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
        }

        private static int Mix(int a, int b)
        {
            int r = ((((a >> 16) & 0xFF) + ((b >> 16) & 0xFF)) >> 1);
            int g = ((((a >> 8) & 0xFF) + ((b >> 8) & 0xFF)) >> 1);
            int bl = (((a & 0xFF) + (b & 0xFF)) >> 1);
            return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | bl;
        }

        private static void Blit(int[] buffer, int chunkX, int chunkY, ArtImage art, int x, int y, bool translucent, int tint)
        {
            int dx0 = x - chunkX;
            int dy0 = y - chunkY;

            int sx0 = Math.Max(0, -dx0);
            int sy0 = Math.Max(0, -dy0);
            int sx1 = Math.Min(art.Width, Size - dx0);
            int sy1 = Math.Min(art.Height, Size - dy0);

            int[] src = art.Pixels;
            for (int sy = sy0; sy < sy1; sy++)
            {
                int srcRow = sy * art.Width;
                int dstRow = (dy0 + sy) * Size;
                for (int sx = sx0; sx < sx1; sx++)
                {
                    int c = src[srcRow + sx];
                    if ((c >> 24) == 0)
                    {
                        continue;
                    }

                    if (tint != 0)
                    {
                        c = Mix(c, tint);
                    }

                    int di = dstRow + dx0 + sx;
                    buffer[di] = translucent ? Mix(c, buffer[di]) : (c | unchecked((int)0xFF000000));
                }
            }
        }

        private static void DrawTexturedTriangle(int[] buffer, int chunkX, int chunkY, ArtImage tex,
            Point p0, Point p1, Point p2, PointF t0, PointF t1, PointF t2, float light, int tint)
        {
            float x0 = p0.X - chunkX, y0 = p0.Y - chunkY;
            float x1 = p1.X - chunkX, y1 = p1.Y - chunkY;
            float x2 = p2.X - chunkX, y2 = p2.Y - chunkY;

            float area = ((x1 - x0) * (y2 - y0)) - ((x2 - x0) * (y1 - y0));
            if (Math.Abs(area) < 0.001f)
            {
                return;
            }

            int minX = Math.Max(0, (int)Math.Floor(Math.Min(x0, Math.Min(x1, x2))));
            int maxX = Math.Min(Size - 1, (int)Math.Ceiling(Math.Max(x0, Math.Max(x1, x2))));
            int minY = Math.Max(0, (int)Math.Floor(Math.Min(y0, Math.Min(y1, y2))));
            int maxY = Math.Min(Size - 1, (int)Math.Ceiling(Math.Max(y0, Math.Max(y1, y2))));
            if (minX > maxX || minY > maxY)
            {
                return;
            }

            float invArea = 1f / area;
            const float eps = -0.01f; // closes small gaps between the two triangles
            int tw = tex.Width, th = tex.Height;
            int[] src = tex.Pixels;

            for (int py = minY; py <= maxY; py++)
            {
                float cyp = py + 0.5f;
                int row = py * Size;
                for (int px = minX; px <= maxX; px++)
                {
                    float cxp = px + 0.5f;
                    float w0 = (((x1 - cxp) * (y2 - cyp)) - ((x2 - cxp) * (y1 - cyp))) * invArea;
                    float w1 = (((x2 - cxp) * (y0 - cyp)) - ((x0 - cxp) * (y2 - cyp))) * invArea;
                    float w2 = 1f - w0 - w1;
                    if (w0 < eps || w1 < eps || w2 < eps)
                    {
                        continue;
                    }

                    int u = (int)((w0 * t0.X) + (w1 * t1.X) + (w2 * t2.X));
                    int v = (int)((w0 * t0.Y) + (w1 * t1.Y) + (w2 * t2.Y));
                    u = u < 0 ? 0 : (u >= tw ? tw - 1 : u);
                    v = v < 0 ? 0 : (v >= th ? th - 1 : v);

                    int c = src[(v * tw) + u];
                    int r = Math.Min(255, (int)(((c >> 16) & 0xFF) * light));
                    int g = Math.Min(255, (int)(((c >> 8) & 0xFF) * light));
                    int b = Math.Min(255, (int)((c & 0xFF) * light));
                    c = unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
                    if (tint != 0)
                    {
                        c = Mix(c, tint);
                    }

                    buffer[row + px] = c;
                }
            }
        }

        #endregion

        #region Hit test

        /// <summary>
        /// Returns the index of the topmost visible drawable at a canvas position or -1
        /// </summary>
        public int HitTest(Point canvas)
        {
            List<int> list = Data.GetChunkDrawables(canvas.X / Size, canvas.Y / Size);
            if (list == null || canvas.X < 0 || canvas.Y < 0)
            {
                return -1;
            }

            IReadOnlyList<Drawable> drawables = Data.Drawables;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Drawable d = drawables[list[i]];
                if (!d.Bounds.Contains(canvas) || !IsVisible(d))
                {
                    continue;
                }

                RegionCell cell = Data.GetCell(d.X, d.Y);
                if (d.Kind == DrawableKind.Static)
                {
                    RegionStatic s = cell.Statics[d.StaticIndex];
                    ArtImage art = Art.GetStatic(s.Id, 0);
                    if (art == null)
                    {
                        continue;
                    }

                    Point p = Data.GetStaticPosition(cell.X, cell.Y, s.Z, art.Width, art.Height);
                    if (!art.IsTransparentAt(canvas.X - p.X, canvas.Y - p.Y))
                    {
                        return list[i];
                    }
                }
                else if (PointInPolygon(canvas, Data.GetLandCorners(cell, IsStretched(cell))))
                {
                    return list[i];
                }
            }

            return -1;
        }

        /// <summary>
        /// Map cell (land) at a canvas position, statics are ignored. Returns null if there is none.
        /// </summary>
        public RegionCell LandAt(Point canvas)
        {
            List<int> list = Data.GetChunkDrawables(canvas.X / Size, canvas.Y / Size);
            if (list == null || canvas.X < 0 || canvas.Y < 0)
            {
                return null;
            }

            IReadOnlyList<Drawable> drawables = Data.Drawables;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Drawable d = drawables[list[i]];
                if (d.Kind != DrawableKind.Land || !d.Bounds.Contains(canvas))
                {
                    continue;
                }

                RegionCell cell = Data.GetCell(d.X, d.Y);
                if (PointInPolygon(canvas, Data.GetLandCorners(cell, IsStretched(cell))))
                {
                    return cell;
                }
            }

            return null;
        }

        private static bool PointInPolygon(Point p, Point[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (((poly[i].Y > p.Y) != (poly[j].Y > p.Y)) &&
                    (p.X < ((poly[j].X - poly[i].X) * (p.Y - poly[i].Y) / (float)(poly[j].Y - poly[i].Y)) + poly[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        #endregion
    }
}
