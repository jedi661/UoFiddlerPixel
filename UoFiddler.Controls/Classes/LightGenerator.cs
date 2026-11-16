/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Light designer logic: creates light images for light.mul from a few
 * parameters (shape, radius, direction, opening angle, length …) and offers
 * the operations for existing lights (mirror, rotate, resize, brightness).
 *
 * A light is an array of bytes, one per pixel, 0 = no light, 1..31 = brightness.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UoFiddler.Controls.Classes
{
    public enum LightShape
    {
        /// <summary>round light (candle, lamp)</summary>
        Radial,

        /// <summary>oval light (torch, standing lamp)</summary>
        Ellipse,

        /// <summary>light cone from a point (spot)</summary>
        Cone,

        /// <summary>light that falls through an opening and gets wider (window)</summary>
        Window,

        /// <summary>straight light strip of constant width</summary>
        Beam,

        /// <summary>light ring (e.g. magic circle)</summary>
        Ring
    }

    /// <summary>raw light image</summary>
    public sealed class LightImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Data { get; set; }

        public LightImage()
        {
        }

        public LightImage(int width, int height, byte[] data = null)
        {
            Width = width;
            Height = height;
            Data = data ?? new byte[width * height];
        }

        public byte this[int x, int y]
        {
            get => Data[(y * Width) + x];
            set => Data[(y * Width) + x] = value;
        }

        public LightImage Clone() => new LightImage(Width, Height, (byte[])Data.Clone());

        /// <summary>number of pixels with light</summary>
        public int LitPixels => Data.Count(b => b != 0);

        public int MaxValue => Data.Length == 0 ? 0 : Data.Max(b => b > 31 ? ~b & 0x1F : b);
    }

    /// <summary>all parameters of a designed light (saved as JSON)</summary>
    public sealed class LightDesign
    {
        public string Name { get; set; } = "New light";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LightShape Shape { get; set; } = LightShape.Radial;

        /// <summary>size of the light image</summary>
        public int Width { get; set; } = 200;
        public int Height { get; set; } = 200;

        /// <summary>position of the light source relative to the image center</summary>
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        /// <summary>radius (radial, ring) or horizontal radius (ellipse)</summary>
        public int Radius { get; set; } = 80;

        /// <summary>vertical radius (ellipse), ring thickness (ring)</summary>
        public int Radius2 { get; set; } = 60;

        /// <summary>direction in degrees: 0 = right, 90 = down, 180 = left, 270 = up (screen)</summary>
        public int Angle { get; set; } = 90;

        /// <summary>opening angle in degrees (cone, window)</summary>
        public int Spread { get; set; } = 40;

        /// <summary>reach of the light (cone, window, beam)</summary>
        public int Length { get; set; } = 150;

        /// <summary>width of the opening (window) or of the strip (beam)</summary>
        public int SourceWidth { get; set; } = 30;

        /// <summary>brightness at the source, 1..31</summary>
        public int Intensity { get; set; } = 24;

        /// <summary>how fast the light gets darker: 1 = linear, &gt;1 = faster, &lt;1 = slower (in %)</summary>
        public int Falloff { get; set; } = 150;

        /// <summary>soft edges, 0..100 %</summary>
        public int Softness { get; set; } = 40;

        /// <summary>grain like the original lights, 0..100 %</summary>
        public int Noise { get; set; } = 25;

        public int Seed { get; set; } = 1;

        public bool FlipH { get; set; }
        public bool FlipV { get; set; }

        /// <summary>additional rotation of the finished image: 0, 90, 180, 270</summary>
        public int Rotate { get; set; }

        public LightDesign Clone() => (LightDesign)MemberwiseClone();
    }

    public static class LightGenerator
    {
        public const int MaxValue = 31;

        #region Presets

        /// <summary>ready-made lights to start with</summary>
        public static IReadOnlyList<LightDesign> Presets { get; } = new List<LightDesign>
        {
            new LightDesign { Name = "Candle", Shape = LightShape.Radial, Width = 100, Height = 100, Radius = 45, Intensity = 20, Falloff = 160, Noise = 25 },
            new LightDesign { Name = "Lamp", Shape = LightShape.Radial, Width = 250, Height = 250, Radius = 120, Intensity = 28, Falloff = 140, Noise = 25 },
            new LightDesign { Name = "Big light", Shape = LightShape.Radial, Width = 300, Height = 300, Radius = 150, Intensity = 31, Falloff = 120, Noise = 30 },
            new LightDesign { Name = "Torch", Shape = LightShape.Ellipse, Width = 200, Height = 220, Radius = 80, Radius2 = 100, OffsetY = -10, Intensity = 26, Falloff = 150, Noise = 30 },
            new LightDesign { Name = "Window (falls left)", Shape = LightShape.Window, Width = 250, Height = 250, OffsetX = 60, OffsetY = -60, Angle = 135, Spread = 25, Length = 220, SourceWidth = 40, Intensity = 24, Falloff = 120, Softness = 50, Noise = 30 },
            new LightDesign { Name = "Window (falls right)", Shape = LightShape.Window, Width = 250, Height = 250, OffsetX = -60, OffsetY = -60, Angle = 45, Spread = 25, Length = 220, SourceWidth = 40, Intensity = 24, Falloff = 120, Softness = 50, Noise = 30 },
            new LightDesign { Name = "Spot (down)", Shape = LightShape.Cone, Width = 250, Height = 250, OffsetY = -80, Angle = 90, Spread = 50, Length = 200, Intensity = 28, Falloff = 130, Softness = 40, Noise = 25 },
            new LightDesign { Name = "Beam", Shape = LightShape.Beam, Width = 220, Height = 220, OffsetX = -70, OffsetY = -70, Angle = 45, Length = 200, SourceWidth = 18, Intensity = 26, Falloff = 110, Softness = 60, Noise = 30 },
            new LightDesign { Name = "Ring", Shape = LightShape.Ring, Width = 200, Height = 200, Radius = 70, Radius2 = 18, Intensity = 24, Falloff = 100, Softness = 70, Noise = 20 }
        };

        #endregion

        #region Generating

        private static double Edge(double r, double softness)
        {
            // r: 0 = inside, 1 = edge; soft edges fade over the last part
            if (r >= 1)
            {
                return 0;
            }

            if (softness <= 0.001)
            {
                return 1;
            }

            double start = 1 - softness;
            return r <= start ? 1 : (1 - r) / softness;
        }

        /// <summary>brightness 0..1 of a point relative to the light source</summary>
        private static double Shade(LightDesign d, double dx, double dy)
        {
            double soft = Math.Max(0, Math.Min(1, d.Softness / 100.0));
            double falloff = Math.Max(0.1, d.Falloff / 100.0);
            double rad = d.Angle * Math.PI / 180;
            double ux = Math.Cos(rad), uy = Math.Sin(rad);
            double along = (dx * ux) + (dy * uy);
            double across = Math.Abs((-dx * uy) + (dy * ux));

            switch (d.Shape)
            {
                case LightShape.Radial:
                    {
                        double r = Math.Sqrt((dx * dx) + (dy * dy)) / Math.Max(1, d.Radius);
                        return r >= 1 ? 0 : Math.Pow(1 - r, falloff) * Edge(r, soft * 0.5);
                    }

                case LightShape.Ellipse:
                    {
                        double ex = dx / Math.Max(1, d.Radius), ey = dy / Math.Max(1, d.Radius2);
                        double r = Math.Sqrt((ex * ex) + (ey * ey));
                        return r >= 1 ? 0 : Math.Pow(1 - r, falloff) * Edge(r, soft * 0.5);
                    }

                case LightShape.Ring:
                    {
                        double dist = Math.Sqrt((dx * dx) + (dy * dy));
                        double r = Math.Abs(dist - d.Radius) / Math.Max(1, d.Radius2);
                        return r >= 1 ? 0 : Math.Pow(1 - r, falloff) * Edge(r, soft);
                    }

                case LightShape.Cone:
                    {
                        double dist = Math.Sqrt((dx * dx) + (dy * dy));
                        if (along <= 0 && dist > 2)
                        {
                            return 0;
                        }

                        double half = Math.Max(1, d.Spread) / 2.0 * Math.PI / 180;
                        double ang = dist < 0.5 ? 0 : Math.Acos(Math.Max(-1, Math.Min(1, along / dist)));
                        double r = dist / Math.Max(1, d.Length);
                        if (r >= 1 || ang >= half)
                        {
                            return 0;
                        }

                        return Math.Pow(1 - r, falloff) * Edge(ang / half, soft);
                    }

                case LightShape.Window:
                case LightShape.Beam:
                    {
                        if (along < 0)
                        {
                            return 0;
                        }

                        double r = along / Math.Max(1, d.Length);
                        if (r >= 1)
                        {
                            return 0;
                        }

                        double spread = d.Shape == LightShape.Beam ? 0 : Math.Max(0, d.Spread) / 2.0 * Math.PI / 180;
                        double halfWidth = (Math.Max(1, d.SourceWidth) / 2.0) + (along * Math.Tan(spread));
                        double w = across / halfWidth;
                        if (w >= 1)
                        {
                            return 0;
                        }

                        // the light is brightest close to the opening, the edges are soft
                        return Math.Pow(1 - r, falloff) * Edge(w, soft) * Edge(1 - Math.Min(1, along / 3.0), 0.99);
                    }

                default:
                    return 0;
            }
        }

        /// <summary>creates the light image of a design</summary>
        public static LightImage Generate(LightDesign d)
        {
            int width = Math.Max(1, Math.Min(1024, d.Width)), height = Math.Max(1, Math.Min(1024, d.Height));
            var img = new LightImage(width, height);
            double cx = (width / 2.0) + d.OffsetX, cy = (height / 2.0) + d.OffsetY;
            double intensity = Math.Max(0, Math.Min(MaxValue, d.Intensity));
            double noise = Math.Max(0, Math.Min(1, d.Noise / 100.0));
            var random = new Random(d.Seed);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double s = Shade(d, x + 0.5 - cx, y + 0.5 - cy);
                    double grain = noise > 0 ? (random.NextDouble() - 0.5) * noise * 10 : 0;
                    if (s <= 0)
                    {
                        continue;
                    }

                    double v = (s * intensity) + (grain * Math.Min(1, s * 4));
                    int value = (int)Math.Round(v);
                    img[x, y] = (byte)Math.Max(0, Math.Min(MaxValue, value));
                }
            }

            if (d.FlipH)
            {
                img = FlipHorizontal(img);
            }

            if (d.FlipV)
            {
                img = FlipVertical(img);
            }

            for (int r = ((d.Rotate % 360) + 360) % 360; r >= 90; r -= 90)
            {
                img = Rotate90(img);
            }

            return img;
        }

        #endregion

        #region Operations on light images

        public static LightImage FlipHorizontal(LightImage img)
        {
            var result = new LightImage(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    result[img.Width - 1 - x, y] = img[x, y];
                }
            }

            return result;
        }

        public static LightImage FlipVertical(LightImage img)
        {
            var result = new LightImage(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            {
                Array.Copy(img.Data, y * img.Width, result.Data, (img.Height - 1 - y) * img.Width, img.Width);
            }

            return result;
        }

        /// <summary>90° clockwise</summary>
        public static LightImage Rotate90(LightImage img)
        {
            var result = new LightImage(img.Height, img.Width);
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    result[img.Height - 1 - y, x] = img[x, y];
                }
            }

            return result;
        }

        /// <summary>free rotation around the center (degrees, clockwise), the size stays</summary>
        public static LightImage RotateFree(LightImage img, double degrees)
        {
            var result = new LightImage(img.Width, img.Height);
            double rad = -degrees * Math.PI / 180, cos = Math.Cos(rad), sin = Math.Sin(rad);
            double cx = img.Width / 2.0, cy = img.Height / 2.0;
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    double dx = x + 0.5 - cx, dy = y + 0.5 - cy;
                    int sx = (int)Math.Floor((dx * cos) - (dy * sin) + cx), sy = (int)Math.Floor((dx * sin) + (dy * cos) + cy);
                    if (sx >= 0 && sy >= 0 && sx < img.Width && sy < img.Height)
                    {
                        result[x, y] = img[sx, sy];
                    }
                }
            }

            return result;
        }

        /// <summary>new size (bilinear)</summary>
        public static LightImage Resize(LightImage img, int width, int height)
        {
            width = Math.Max(1, Math.Min(1024, width));
            height = Math.Max(1, Math.Min(1024, height));
            var result = new LightImage(width, height);
            double sx = (double)img.Width / width, sy = (double)img.Height / height;
            for (int y = 0; y < height; y++)
            {
                double fy = Math.Max(0, ((y + 0.5) * sy) - 0.5);
                int y0 = Math.Min(img.Height - 1, (int)fy), y1 = Math.Min(img.Height - 1, y0 + 1);
                double ty = fy - y0;
                for (int x = 0; x < width; x++)
                {
                    double fx = Math.Max(0, ((x + 0.5) * sx) - 0.5);
                    int x0 = Math.Min(img.Width - 1, (int)fx), x1 = Math.Min(img.Width - 1, x0 + 1);
                    double tx = fx - x0;
                    double v = (V(img, x0, y0) * (1 - tx) * (1 - ty)) + (V(img, x1, y0) * tx * (1 - ty)) +
                               (V(img, x0, y1) * (1 - tx) * ty) + (V(img, x1, y1) * tx * ty);
                    result[x, y] = (byte)Math.Max(0, Math.Min(MaxValue, Math.Round(v)));
                }
            }

            return result;
        }

        private static int V(LightImage img, int x, int y) => Ultima.Light.DecodeValue(img[x, y]);

        /// <summary>brightness in percent (100 = unchanged)</summary>
        public static LightImage Brightness(LightImage img, int percent)
        {
            var result = new LightImage(img.Width, img.Height);
            for (int i = 0; i < img.Data.Length; i++)
            {
                int v = Ultima.Light.DecodeValue(img.Data[i]);
                result.Data[i] = v == 0 ? (byte)0 : (byte)Math.Max(1, Math.Min(MaxValue, (int)Math.Round(v * percent / 100.0)));
            }

            return result;
        }

        /// <summary>moves the light inside the image (pixels outside are lost)</summary>
        public static LightImage Shift(LightImage img, int dx, int dy)
        {
            var result = new LightImage(img.Width, img.Height);
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && ny >= 0 && nx < img.Width && ny < img.Height)
                    {
                        result[nx, ny] = img[x, y];
                    }
                }
            }

            return result;
        }

        /// <summary>sum of two lights (both centered), the bigger size is used</summary>
        public static LightImage Combine(LightImage a, LightImage b)
        {
            int width = Math.Max(a.Width, b.Width), height = Math.Max(a.Height, b.Height);
            var result = new LightImage(width, height);
            void Add(LightImage src)
            {
                int ox = (width - src.Width) / 2, oy = (height - src.Height) / 2;
                for (int y = 0; y < src.Height; y++)
                {
                    for (int x = 0; x < src.Width; x++)
                    {
                        int v = Ultima.Light.DecodeValue(result[x + ox, y + oy]) + Ultima.Light.DecodeValue(src[x, y]);
                        result[x + ox, y + oy] = (byte)Math.Min(MaxValue, v);
                    }
                }
            }

            Add(a);
            Add(b);
            return result;
        }

        #endregion
    }

    /// <summary>
    /// Names of the lights (light.mul has no names) – stored in &lt;AppData&gt;\LightNames.json.
    /// </summary>
    public static class LightNames
    {
        private static Dictionary<int, string> _names;

        private static string FileName => Path.Combine(Options.AppDataPath ?? string.Empty, "LightNames.json");

        private static Dictionary<int, string> Names => _names ?? (_names = Load());

        private static Dictionary<int, string> Load()
        {
            try
            {
                if (File.Exists(FileName))
                {
                    return JsonSerializer.Deserialize<Dictionary<int, string>>(File.ReadAllText(FileName)) ?? new Dictionary<int, string>();
                }
            }
            catch (Exception)
            {
                // broken file
            }

            return new Dictionary<int, string>();
        }

        public static string Get(int id) => Names.TryGetValue(id, out string name) ? name : null;

        public static void Set(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Names.Remove(id);
            }
            else
            {
                Names[id] = name.Trim();
            }

            Save();
        }

        public static IReadOnlyDictionary<int, string> All => Names;

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FileName));
                File.WriteAllText(FileName, JsonSerializer.Serialize(Names, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>text of a light in the list: "12 (0x0C) – name"</summary>
        public static string Display(int id)
        {
            string name = Get(id);
            return string.IsNullOrEmpty(name) ? $"{id} (0x{id:X2})" : $"{id} (0x{id:X2}) – {name}";
        }
    }

    /// <summary>
    /// Light library: designs (JSON) and light images, stored in a folder
    /// (default &lt;AppData&gt;\LightLibrary). The file "name.uolight" contains the design
    /// and the raw light as base64, so it can be loaded even without the design.
    /// </summary>
    public sealed class LightLibraryEntry
    {
        public string Name { get; set; }
        public LightDesign Design { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Data { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        public LightImage ToImage()
        {
            if (!string.IsNullOrEmpty(Data) && Width > 0 && Height > 0)
            {
                return new LightImage(Width, Height, Convert.FromBase64String(Data));
            }

            return Design != null ? LightGenerator.Generate(Design) : null;
        }

        public static LightLibraryEntry From(string name, LightDesign design, LightImage image) => new LightLibraryEntry
        {
            Name = name,
            Design = design?.Clone(),
            Width = image.Width,
            Height = image.Height,
            Data = Convert.ToBase64String(image.Data)
        };

        public override string ToString() => Name;
    }

    public static class LightLibrary
    {
        public const string Extension = ".uolight";

        public static string DefaultFolder => Path.Combine(Options.AppDataPath ?? string.Empty, "LightLibrary");

        private static readonly JsonSerializerOptions _json = new JsonSerializerOptions { WriteIndented = true };

        public static void Save(LightLibraryEntry entry, string fileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            File.WriteAllText(fileName, JsonSerializer.Serialize(entry, _json));
            entry.FileName = fileName;
        }

        public static LightLibraryEntry Load(string fileName)
        {
            LightLibraryEntry entry = JsonSerializer.Deserialize<LightLibraryEntry>(File.ReadAllText(fileName), _json);
            if (entry != null)
            {
                entry.FileName = fileName;
                entry.Name = string.IsNullOrWhiteSpace(entry.Name) ? Path.GetFileNameWithoutExtension(fileName) : entry.Name;
            }

            return entry;
        }

        public static List<LightLibraryEntry> LoadFolder(string folder)
        {
            var list = new List<LightLibraryEntry>();
            if (!Directory.Exists(folder))
            {
                return list;
            }

            foreach (string file in Directory.GetFiles(folder, "*" + Extension).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    LightLibraryEntry e = Load(file);
                    if (e != null)
                    {
                        list.Add(e);
                    }
                }
                catch (Exception)
                {
                    // not a light file
                }
            }

            return list;
        }

        public static string SafeFileName(string name)
        {
            // the characters Windows does not allow, on every system
            const string invalid = "<>:\"/\\|?*";
            string s = new string((name ?? "light").Select(c => c < 32 || invalid.IndexOf(c) >= 0 || Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray()).Trim();
            return s.Length == 0 ? "light" : s;
        }
    }
}
