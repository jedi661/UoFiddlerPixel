using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Ultima;
using UoFiddler.Controls.Models.Uop;
using UoFiddler.Controls.Uop;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit
{
    /// <summary>One decoded animation frame with its UO center point (if known).</summary>
    internal sealed class SourceFrame
    {
        public Bitmap Bitmap;
        public Point? Center;
    }

    internal sealed class ListEntry
    {
        public int Id;
        public string Label;
        public override string ToString() => Label;
    }

    /// <summary>
    /// Base class for everything the animation browser can read from.
    /// LoadBodies may run on a background thread; every call is made while holding <see cref="SdkSync.Lock"/>.
    /// </summary>
    internal abstract class AnimSource : IDisposable
    {
        public abstract string Name { get; }
        public virtual bool SupportsHue => false;
        public virtual bool SupportsDirections => true;

        /// <summary>
        /// Sources that use the Ultima SDK caches must share the global SDK lock. Sources that only read
        /// their own files use their own lock, so the art browser is not blocked while they load.
        /// </summary>
        protected virtual bool UsesSdkCaches => true;

        public object SyncRoot => UsesSdkCaches ? SdkSync.Lock : this;

        public abstract List<ListEntry> LoadBodies();
        public abstract List<ListEntry> GetActions(int body);
        public abstract List<SourceFrame> GetFrames(int body, int action, int direction, int hue);

        public virtual void Dispose() { }

        /// <summary>UO stores directions 0..4, 5..7 are 3..1 mirrored.</summary>
        protected static int MapDirection(int dir, out bool mirror)
        {
            mirror = dir > 4;
            return dir > 4 ? 8 - dir : dir;
        }

        protected static SourceFrame Make(Bitmap src, Point? center, bool mirror, bool copy)
        {
            if (src == null) return new SourceFrame();
            Bitmap b = copy ? ImageUtil.To32(src) : src;
            if (mirror)
            {
                if (ReferenceEquals(b, src)) b = ImageUtil.To32(src);
                b.RotateFlip(RotateFlipType.RotateNoneFlipX);
                if (center.HasValue) center = new Point(b.Width - center.Value.X, center.Value.Y);
            }
            return new SourceFrame { Bitmap = b, Center = center };
        }

        public static string[] ActionNamesFor(int animLength)
        {
            if (animLength == 13) return UopConstants.ActionNames.AnimalActions;
            if (animLength == 22) return UopConstants.ActionNames.MonsterActions;
            return UopConstants.ActionNames.CharActions;
        }

        protected static string ActionLabel(int action, string[] names)
        {
            string n = action < names.Length && !string.IsNullOrEmpty(names[action]) ? names[action] : "Action " + action;
            return $"{action:D2}: {n}";
        }

        protected static void ApplyHue(Bitmap bmp, int hue)
        {
            if (bmp == null || hue <= 0) return;
            int h = (hue & 0x3FFF) - 1;
            if (h >= 0 && h < Hues.List.Length)
                Hues.List[h].ApplyTo(bmp, (hue & 0x8000) != 0);
        }
    }

    /// <summary>Like the client: body.def / bodyconv.def decide which anim file is used.</summary>
    internal sealed class AutoSource : AnimSource
    {
        public override string Name => "Auto (body.def / bodyconv.def)";
        public override bool SupportsHue => true;

        public override List<ListEntry> LoadBodies()
        {
            var list = new List<ListEntry>();
            for (int body = 0; body < Animations._maxAnimationValue; body++)
            {
                try
                {
                    if (Animations.IsActionDefined(body, 0, 0))
                        list.Add(new ListEntry { Id = body, Label = $"{body}  [{Animations.GetFileName(body)}]" });
                }
                catch { /* invalid entries are skipped */ }
            }
            return list;
        }

        public override List<ListEntry> GetActions(int body)
        {
            string[] names = ActionNamesFor(body < 200 ? 22 : body < 400 ? 13 : 35);
            var list = new List<ListEntry>();
            for (int a = 0; a < 35; a++)
            {
                try
                {
                    if (Animations.IsActionDefined(body, a, 0))
                        list.Add(new ListEntry { Id = a, Label = ActionLabel(a, names) });
                }
                catch { }
            }
            return list;
        }

        public override List<SourceFrame> GetFrames(int body, int action, int direction, int hue)
        {
            int h = hue;
            AnimationFrame[] frames = Animations.GetAnimation(body, action, direction, ref h, false, false);
            var list = new List<SourceFrame>();
            if (frames == null) return list;
            foreach (AnimationFrame f in frames)
                list.Add(f?.Bitmap != null ? Make(f.Bitmap, f.Center, false, true) : new SourceFrame());
            return list;
        }
    }

    /// <summary>Reads one specific anim file of the current client (anim.mul ... anim5.mul).</summary>
    internal sealed class MulSource : AnimSource
    {
        private readonly int fileType;

        public MulSource(int fileType) { this.fileType = fileType; }

        public override string Name => fileType == 1 ? "anim.mul" : $"anim{fileType}.mul";
        public override bool SupportsHue => true;

        public override List<ListEntry> LoadBodies()
        {
            var list = new List<ListEntry>();
            int count;
            try { count = Animations.GetAnimCount(fileType); }
            catch { return list; }

            for (int body = 0; body < count; body++)
            {
                try
                {
                    int len = Animations.GetAnimLength(body, fileType);
                    bool valid = false;
                    for (int a = 0; a < len && !valid; a++)
                        valid = AnimationEdit.IsActionDefined(fileType, body, a);
                    if (!valid) continue;

                    string type = len == 22 ? "H" : len == 13 ? "L" : "P";
                    list.Add(new ListEntry { Id = body, Label = $"{type}: {body}  ({BodyConverter.GetTrueBody(fileType, body)})" });
                }
                catch { }
            }
            return list;
        }

        public override List<ListEntry> GetActions(int body)
        {
            int len = Animations.GetAnimLength(body, fileType);
            string[] names = ActionNamesFor(len);
            var list = new List<ListEntry>();
            for (int a = 0; a < len; a++)
            {
                try
                {
                    if (AnimationEdit.IsActionDefined(fileType, body, a))
                        list.Add(new ListEntry { Id = a, Label = ActionLabel(a, names) });
                }
                catch { }
            }
            return list;
        }

        public override List<SourceFrame> GetFrames(int body, int action, int direction, int hue)
        {
            var list = new List<SourceFrame>();
            int dir = MapDirection(direction, out bool mirror);
            AnimIdx idx = AnimationEdit.GetAnimation(fileType, body, action, dir);
            Bitmap[] bits = idx?.GetFrames();
            if (bits == null) return list;

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] == null) { list.Add(new SourceFrame()); continue; }
                ApplyHue(bits[i], hue);
                Point center = i < idx.Frames.Count ? idx.Frames[i].Center : Point.Empty;
                SourceFrame sf = Make(bits[i], center, mirror, true);
                bits[i].Dispose();
                list.Add(sf);
            }
            return list;
        }
    }

    /// <summary>AnimationFrame*.uop via the UoFiddler UOP manager.</summary>
    internal sealed class UopSource : AnimSource
    {
        protected override bool UsesSdkCaches => false;

        private UopAnimationDataManager manager;

        public override string Name => "AnimationFrame*.uop (UOP)";

        public string LoadError { get; private set; }

        public override List<ListEntry> LoadBodies()
        {
            var list = new List<ListEntry>();
            if (manager == null)
            {
                var m = new UopAnimationDataManager();
                if (!m.LoadUopFiles())
                {
                    LoadError = "No AnimationFrame*.uop files found in the client folder.";
                    return list;
                }
                m.ProcessUopData();
                manager = m;
            }

            for (int id = 0; id < UopConstants.MAX_ANIMATIONS_DATA_INDEX_COUNT; id++)
            {
                try
                {
                    List<int> actions = manager.GetAvailableUopGroupIndexes(id);
                    if (actions.Count > 0)
                        list.Add(new ListEntry { Id = id, Label = $"{id}  (0x{id:X})  {actions.Count} actions" });
                }
                catch { }
            }
            return list;
        }

        public override List<ListEntry> GetActions(int body)
        {
            var list = new List<ListEntry>();
            if (manager == null) return list;
            string[] names = ActionNamesFor(body < 200 ? 22 : body < 400 ? 13 : 35);
            foreach (int a in manager.GetAvailableActions(body))
                list.Add(new ListEntry { Id = a, Label = ActionLabel(a, names) });
            return list;
        }

        public override List<SourceFrame> GetFrames(int body, int action, int direction, int hue)
        {
            var list = new List<SourceFrame>();
            if (manager == null) return list;
            int dir = MapDirection(direction, out bool mirror);
            UopAnimIdx anim = manager.GetUopAnimation(body, action, dir);
            if (anim?.Frames == null) return list;

            foreach (DecodedUopFrame f in anim.Frames)
            {
                if (f?.Image == null) { list.Add(new SourceFrame()); continue; }
                // The manager caches its frames -> always copy
                list.Add(Make(f.Image, new Point(f.Header.CenterX, f.Header.CenterY), mirror, true));
            }
            return list;
        }
    }

    /// <summary>
    /// A .vd file (UoFiddler "Export to VD"): short 6, short type, index table (lookup, length, extra)
    /// for every action * 5 directions, followed by the frame data in anim.mul format.
    /// </summary>
    internal sealed class VdSource : AnimSource
    {
        protected override bool UsesSdkCaches => false;

        private readonly string path;
        private byte[] data;
        private int actionCount;
        private short type;
        private (int lookup, int length, int extra)[] table;

        public VdSource(string path) { this.path = path; }

        public override string Name => "VD: " + Path.GetFileName(path);

        private void EnsureLoaded()
        {
            if (data != null) return;
            data = File.ReadAllBytes(path);
            using (var br = new BinaryReader(new MemoryStream(data)))
            {
                br.ReadInt16(); // 6
                type = br.ReadInt16();
                actionCount = type switch { 0 => 22, 1 => 13, 2 => 35, 4 => 32, _ => 22 };
                int entries = actionCount * 5;
                if (4 + entries * 12 > data.Length)
                    throw new InvalidDataException("Not a valid VD file (index table is incomplete).");
                table = new (int, int, int)[entries];
                for (int i = 0; i < entries; i++)
                    table[i] = (br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
            }
        }

        private bool Valid(int i) => i >= 0 && i < table.Length && table[i].lookup > 0 && table[i].lookup < data.Length;

        public override List<ListEntry> LoadBodies()
        {
            EnsureLoaded();
            string kind = type switch { 0 => "monster / high (22)", 1 => "animal / low (13)", 2 => "human / people (35)", 4 => "creature UOP (32)", _ => "type " + type };
            return new List<ListEntry> { new ListEntry { Id = 0, Label = $"{Path.GetFileNameWithoutExtension(path)}  [{kind}]" } };
        }

        public override List<ListEntry> GetActions(int body)
        {
            EnsureLoaded();
            string[] names = ActionNamesFor(type == 1 ? 13 : type == 2 ? 35 : 22);
            var list = new List<ListEntry>();
            for (int a = 0; a < actionCount; a++)
            {
                bool any = false;
                for (int d = 0; d < 5 && !any; d++) any = Valid(a * 5 + d);
                if (any) list.Add(new ListEntry { Id = a, Label = ActionLabel(a, names) });
            }
            return list;
        }

        // VdImportHelper knows the UOP creature variant with per frame palettes. It is private,
        // so it is called through reflection (only used for type 4 files).
        private static readonly MethodInfo ReadUopVdFrames = typeof(VdImportHelper).GetMethod(
            "ReadVdFramesForDirection", BindingFlags.NonPublic | BindingFlags.Static);

        public override List<SourceFrame> GetFrames(int body, int action, int direction, int hue)
        {
            EnsureLoaded();
            var list = new List<SourceFrame>();
            int dir = MapDirection(direction, out bool mirror);
            int i = action * 5 + dir;
            if (!Valid(i)) return list;
            (int lookup, int length, int extra) = table[i];

            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                ms.Seek(lookup, SeekOrigin.Begin);

                if (type == 4 && ReadUopVdFrames != null)
                {
                    long end = length > 0 ? lookup + length : data.Length;
                    if (ReadUopVdFrames.Invoke(null, new object[] { br, end, extra == 1 }) is List<DecodedUopFrame> uop)
                    {
                        foreach (DecodedUopFrame f in uop)
                            list.Add(f?.Image != null ? Make(f.Image, new Point(f.Header.CenterX, f.Header.CenterY), mirror, true) : new SourceFrame());
                        return list;
                    }
                }

                var idx = new AnimIdx(br, extra);
                Bitmap[] bits = idx.GetFrames();
                if (bits == null) return list;
                for (int k = 0; k < bits.Length; k++)
                {
                    if (bits[k] == null) { list.Add(new SourceFrame()); continue; }
                    ApplyHue(bits[k], hue);
                    SourceFrame sf = Make(bits[k], idx.Frames[k].Center, mirror, true);
                    bits[k].Dispose();
                    list.Add(sf);
                }
            }
            return list;
        }

        public override bool SupportsHue => true;
    }

    /// <summary>
    /// anim*.idx / anim*.mul from ANOTHER folder (e.g. a second client or a backup) to compare
    /// or to take frames from. All anim files found in the folder are listed together.
    /// </summary>
    internal sealed class ExternalMulSource : AnimSource
    {
        protected override bool UsesSdkCaches => false;

        private readonly string folder;
        private readonly Dictionary<int, byte[]> idx = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, string> mul = new Dictionary<int, string>();
        private const int Key = 100000;

        public ExternalMulSource(string folder) { this.folder = folder; }

        public override string Name => "Folder: " + folder;
        public override bool SupportsHue => true;

        private static string FileBase(int ft) => ft == 1 ? "anim" : "anim" + ft;

        private static int AnimCount(int ft, long idxLength)
        {
            switch (ft)
            {
                case 2: return 200 + (int)((idxLength - 22000 * 12) / (12 * 65));
                default: return 400 + (int)((idxLength - 35000 * 12) / (12 * 175));
            }
        }

        /// <summary>Same index layout as Ultima.Animations (anim.mul family).</summary>
        private static int IndexOf(int ft, int body, int action, int dir)
        {
            int index;
            switch (ft)
            {
                case 2:
                    index = body < 200 ? body * 110 : 22000 + (body - 200) * 65;
                    break;
                case 3:
                    index = body < 300 ? body * 65 : body < 400 ? 33000 + (body - 300) * 110 : 35000 + (body - 400) * 175;
                    break;
                case 5:
                    index = body < 200 && body != 34 ? body * 110 : body < 400 ? 22000 + (body - 200) * 65 : 35000 + (body - 400) * 175;
                    break;
                default:
                    index = body < 200 ? body * 110 : body < 400 ? 22000 + (body - 200) * 65 : 35000 + (body - 400) * 175;
                    break;
            }
            return index + action * 5 + dir;
        }

        private bool Entry(int ft, int index, out int lookup, out int length, out int extra)
        {
            lookup = length = extra = 0;
            if (!idx.TryGetValue(ft, out byte[] d)) return false;
            int pos = index * 12;
            if (index < 0 || pos + 12 > d.Length) return false;
            lookup = BitConverter.ToInt32(d, pos);
            length = BitConverter.ToInt32(d, pos + 4) & 0x7FFFFFFF;
            extra = BitConverter.ToInt32(d, pos + 8);
            return lookup >= 0 && length > 0;
        }

        public override List<ListEntry> LoadBodies()
        {
            var list = new List<ListEntry>();
            for (int ft = 1; ft <= 5; ft++)
            {
                string i = Path.Combine(folder, FileBase(ft) + ".idx");
                string m = Path.Combine(folder, FileBase(ft) + ".mul");
                if (!File.Exists(i) || !File.Exists(m)) continue;
                idx[ft] = File.ReadAllBytes(i);
                mul[ft] = m;

                int count = AnimCount(ft, idx[ft].Length);
                for (int body = 0; body < count; body++)
                {
                    int len = Animations.GetAnimLength(body, ft);
                    bool valid = false;
                    for (int a = 0; a < len && !valid; a++)
                        valid = Entry(ft, IndexOf(ft, body, a, 0), out _, out _, out _);
                    if (!valid) continue;
                    string type = len == 22 ? "H" : len == 13 ? "L" : "P";
                    list.Add(new ListEntry { Id = ft * Key + body, Label = $"{FileBase(ft)}  {type}: {body}" });
                }
            }
            return list;
        }

        public override List<ListEntry> GetActions(int bodyKey)
        {
            int ft = bodyKey / Key, body = bodyKey % Key;
            int len = Animations.GetAnimLength(body, ft);
            string[] names = ActionNamesFor(len);
            var list = new List<ListEntry>();
            for (int a = 0; a < len; a++)
                if (Entry(ft, IndexOf(ft, body, a, 0), out _, out _, out _))
                    list.Add(new ListEntry { Id = a, Label = ActionLabel(a, names) });
            return list;
        }

        public override List<SourceFrame> GetFrames(int bodyKey, int action, int direction, int hue)
        {
            var list = new List<SourceFrame>();
            int ft = bodyKey / Key, body = bodyKey % Key;
            int dir = MapDirection(direction, out bool mirror);
            if (!Entry(ft, IndexOf(ft, body, action, dir), out int lookup, out int length, out int extra)) return list;

            byte[] buffer = new byte[length];
            using (var fs = new FileStream(mul[ft], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(lookup, SeekOrigin.Begin);
                int read = 0;
                while (read < length)
                {
                    int n = fs.Read(buffer, read, length - read);
                    if (n <= 0) break;
                    read += n;
                }
            }

            using (var br = new BinaryReader(new MemoryStream(buffer)))
            {
                var anim = new AnimIdx(br, extra);
                Bitmap[] bits = anim.GetFrames();
                if (bits == null) return list;
                for (int k = 0; k < bits.Length; k++)
                {
                    if (bits[k] == null) { list.Add(new SourceFrame()); continue; }
                    ApplyHue(bits[k], hue);
                    SourceFrame sf = Make(bits[k], anim.Frames[k].Center, mirror, true);
                    bits[k].Dispose();
                    list.Add(sf);
                }
            }
            return list;
        }
    }

    /// <summary>Own images (PNG/BMP/...) or animated GIFs as a frame sequence.</summary>
    internal sealed class ImageFilesSource : AnimSource
    {
        protected override bool UsesSdkCaches => false;

        private readonly string[] files;
        private List<Bitmap> frames;

        public ImageFilesSource(string[] files) { this.files = files; }

        public override string Name => files.Length == 1 ? "Images: " + Path.GetFileName(files[0]) : $"Images: {files.Length} files";
        public override bool SupportsDirections => false;

        public override List<ListEntry> LoadBodies()
        {
            if (frames == null)
            {
                frames = new List<Bitmap>();
                foreach (string f in files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    try { frames.AddRange(ImageUtil.LoadAllFrames(f)); }
                    catch { /* not an image */ }
                }
            }
            return new List<ListEntry> { new ListEntry { Id = 0, Label = $"{Name}  ({frames.Count} frames)" } };
        }

        public override List<ListEntry> GetActions(int body) => new List<ListEntry> { new ListEntry { Id = 0, Label = "00: Sequence" } };

        public override List<SourceFrame> GetFrames(int body, int action, int direction, int hue)
        {
            return (frames ?? new List<Bitmap>()).Select(b => new SourceFrame { Bitmap = new Bitmap(b) }).ToList();
        }

        public override void Dispose()
        {
            if (frames != null) foreach (Bitmap b in frames) b.Dispose();
            frames = null;
        }
    }
}
