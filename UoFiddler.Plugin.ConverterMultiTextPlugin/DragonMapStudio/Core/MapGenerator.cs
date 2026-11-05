// /***************************************************************************
//  *
//  * Dragon Map Studio – random world generator (works with Dragon colours)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>Seeded 2D Perlin noise with fractal (fbm) and ridged variants.</summary>
    public sealed class PerlinNoise
    {
        private readonly int[] _perm = new int[512];

        public PerlinNoise(int seed)
        {
            var rnd = new Random(seed);
            var p = Enumerable.Range(0, 256).ToArray();
            for (int i = 255; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (p[i], p[j]) = (p[j], p[i]);
            }
            for (int i = 0; i < 512; i++)
            {
                _perm[i] = p[i & 255];
            }
        }

        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);

        private static double Grad(int hash, double x, double y)
        {
            switch (hash & 7)
            {
                case 0: return x + y;
                case 1: return -x + y;
                case 2: return x - y;
                case 3: return -x - y;
                case 4: return x;
                case 5: return -x;
                case 6: return y;
                default: return -y;
            }
        }

        /// <summary>Noise in about -1..1.</summary>
        public double Noise(double x, double y)
        {
            int xi = (int)Math.Floor(x), yi = (int)Math.Floor(y);
            double xf = x - xi, yf = y - yi;
            xi &= 255;
            yi &= 255;
            double u = Fade(xf), v = Fade(yf);
            int aa = _perm[_perm[xi] + yi], ab = _perm[_perm[xi] + yi + 1];
            int ba = _perm[_perm[xi + 1] + yi], bb = _perm[_perm[xi + 1] + yi + 1];
            double x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
            double x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);
            return Lerp(x1, x2, v);
        }

        /// <summary>Fractal noise normalised to 0..1.</summary>
        public double Fbm(double x, double y, int octaves, double persistence, double lacunarity)
        {
            double sum = 0, amp = 1, freq = 1, max = 0;
            for (int i = 0; i < octaves; i++)
            {
                sum += Noise(x * freq + i * 17.13, y * freq + i * 31.7) * amp;
                max += amp;
                amp *= persistence;
                freq *= lacunarity;
            }
            return Math.Clamp(sum / max * 0.75 + 0.5, 0, 1);
        }

        /// <summary>Ridged noise 0..1 (mountain chains).</summary>
        public double Ridged(double x, double y, int octaves, double persistence, double lacunarity)
        {
            double sum = 0, amp = 1, freq = 1, max = 0;
            for (int i = 0; i < octaves; i++)
            {
                double n = 1 - Math.Abs(Noise(x * freq + i * 7.7, y * freq + i * 3.3));
                sum += n * n * amp;
                max += amp;
                amp *= persistence;
                freq *= lacunarity;
            }
            return sum / max;
        }

        public static double Lerp(double a, double b, double t) => a + (b - a) * t;
    }

    /// <summary>Which terrain group is used for which climate (temperature / moisture 0..1).</summary>
    public sealed class GeneratorBiome
    {
        [DisplayName("Aktiv")]
        public bool Enabled { get; set; } = true;
        [DisplayName("Name")]
        public string Name { get; set; } = string.Empty;

        [Description("Gruppe (hex in Dragon) – die Höhe wählt die passende Farbe der Gruppe.")]
        [DisplayName("Gruppe")]
        public int Group { get; set; }

        [DisplayName("Temp. min")]

        public double MinTemperature { get; set; }
        [DisplayName("Temp. max")]
        public double MaxTemperature { get; set; } = 1;
        [DisplayName("Feuchte min")]
        public double MinMoisture { get; set; }
        [DisplayName("Feuchte max")]
        public double MaxMoisture { get; set; } = 1;

        public GeneratorBiome Clone() => (GeneratorBiome)MemberwiseClone();
    }

    /// <summary>All generator options (shown in a PropertyGrid; new properties appear automatically).</summary>
    public sealed class GeneratorSettings
    {
        [Category("1 Allgemein"), DisplayName("Seed"), Description("Gleiche Zahl = gleiche Karte.")]
        public int Seed { get; set; } = 1234;

        [Category("1 Allgemein"), DisplayName("Neuer Seed bei jedem Lauf")]
        public bool RandomSeed { get; set; }

        [Category("1 Allgemein"), DisplayName("Größe der aktuellen Karte nutzen"), Description("Aus = Breite/Höhe unten verwenden (neue Karte).")]
        public bool UseCurrentMapSize { get; set; } = true;

        [Category("1 Allgemein"), DisplayName("Breite"), Description("Vielfaches von 8 (UO-Blöcke). Felucca: 6144 / 7168.")]
        public int Width { get; set; } = 6144;

        [Category("1 Allgemein"), DisplayName("Höhe"), Description("Vielfaches von 8. Felucca: 4096.")]
        public int Height { get; set; } = 4096;

        [Category("2 Land und Wasser"), DisplayName("Land %"), Description("Anteil Land an der Karte (ohne Umriss-Maske).")]
        public double LandPercent { get; set; } = 42;

        [Category("2 Land und Wasser"), DisplayName("Kontinent-Größe (0..1)"), Description("Typische Größe der Landmassen als Anteil der Kartenbreite (0.3 = ein Drittel).")]
        public double FeatureSize { get; set; } = 0.3;

        [Category("2 Land und Wasser"), DisplayName("Oktaven"), Description("Detailstufen des Rauschens (mehr = rauere Küsten).")]
        public int Octaves { get; set; } = 7;

        [Category("2 Land und Wasser"), DisplayName("Persistenz")]
        public double Persistence { get; set; } = 0.5;

        [Category("2 Land und Wasser"), DisplayName("Lakunarität")]
        public double Lacunarity { get; set; } = 2.0;

        [Category("2 Land und Wasser"), DisplayName("Wasser am Rand (0..1)"), Description("0 = Land bis zum Rand, 1 = immer Wasser am Kartenrand.")]
        public double EdgeFalloff { get; set; } = 0.8;

        [Category("2 Land und Wasser"), DisplayName("Rand-Breite (0..1)"), Description("Wie weit der Wasserrand ins Innere reicht.")]
        public double EdgeWidth { get; set; } = 0.18;

        [Category("2 Land und Wasser"), DisplayName("Strand-Breite (Tiles)"), Description("Breite des Strandes an der Küste (0 = kein Strand).")]
        public int BeachWidth { get; set; } = 3;

        [Category("2 Land und Wasser"), DisplayName("Strand-Anteil %"), Description("Wie viel der Küste Strand bekommt (100 = überall).")]
        public double BeachPercent { get; set; } = 70;

        [Category("2 Land und Wasser"), DisplayName("Rand-Verwirbelung (0..1)"), Description("Macht den Wasserrand unregelmäßig.")]
        public double EdgeWarp { get; set; } = 0.5;

        [Category("3 Gebirge"), DisplayName("Gebirge %"), Description("Anteil des Landes, der Gebirge wird.")]
        public double MountainPercent { get; set; } = 10;

        [Category("3 Gebirge"), DisplayName("Gebirgs-Größe (0..1)"), Description("Anteil der Kartenbreite.")]
        public double MountainSize { get; set; } = 0.12;

        [Category("3 Gebirge"), DisplayName("Schnee auf Gipfeln %"), Description("Anteil der Gebirge, der oben Schnee bekommt (0 = aus).")]
        public double SnowPercent { get; set; } = 15;

        [Category("4 Höhen"), DisplayName("Max. Landhöhe"), Description("Höchste Z des normalen Landes (IMod13: Gras bis 40).")]
        public int MaxLandAltitude { get; set; } = 30;

        [Category("4 Höhen"), DisplayName("Min. Gebirgshöhe über Boden"), Description("Gebirge beginnen so hoch über dem Land daneben (Berge gehen immer nach oben).")]
        public int MinMountainAltitude { get; set; } = 15;

        [Category("4 Höhen"), DisplayName("Max. Gebirgshöhe über Boden"), Description("IMod13: Fels bis 90 (absolut).")]
        public int MaxMountainAltitude { get; set; } = 70;

        [Category("3 Gebirge"), DisplayName("Höhleneingänge"), Description("Anzahl unsichtbarer Höhleneingänge (Gruppe 1d) am Bergrand. Aktiviert die Regel 1d→Berg (map/cave-enti2mountain.txt). 0 = keine.")]
        public int CaveEntrances { get; set; } = 8;

        [Category("3 Gebirge"), DisplayName("Höhleneingang-Gruppe"), Description("IMod13: 29 (= 1d hex, Invisible Cave entrance).")]
        public int CaveGroup { get; set; } = 0x1d;

        [Category("4 Höhen"), DisplayName("Wasserhöhe")]
        public int WaterAltitude { get; set; } = -5;

        [Category("5 Klima"), DisplayName("Norden kalt / Süden warm")]
        public bool NorthCold { get; set; } = true;

        [Category("5 Klima"), DisplayName("Klima-Größe (0..1)"), Description("Größe der Klimazonen als Anteil der Kartenbreite.")]
        public double ClimateSize { get; set; } = 0.25;

        [Category("5 Klima"), DisplayName("Höhe kühlt (0..1)"), Description("Wie stark die Höhe die Temperatur senkt.")]
        public double HeightCooling { get; set; } = 0.35;

        [Category("6 Gruppen"), DisplayName("Wasser-Gruppe"), Description("Dragon-Gruppe (dezimal; IMod13: 2 = Water).")]
        public int WaterGroup { get; set; } = 0x02;

        [Category("6 Gruppen"), DisplayName("Strand-Gruppe"), Description("IMod13: 5 = Desert/Sand.")]
        public int BeachGroup { get; set; } = 0x05;

        [Category("6 Gruppen"), DisplayName("Gebirgs-Gruppe"), Description("IMod13: 6 = Mountain/Rock.")]
        public int MountainGroup { get; set; } = 0x06;

        [Category("6 Gruppen"), DisplayName("Gipfel-Gruppe"), Description("IMod13: 7 = Snow.")]
        public int SnowGroup { get; set; } = 0x07;

        [Category("6 Gruppen"), DisplayName("Ersatz-Gruppe"), Description("Wenn kein Biom passt (IMod13: 0 = Grass).")]
        public int FallbackGroup { get; set; } = 0x00;

        [Category("6 Gruppen"), DisplayName("Nach dem Erzeugen reparieren"), Description("Tutorial-Regeln anwenden: keine 1-Tile-Streifen, Brücken-Gruppen zwischen Gruppen ohne Übergang, Sumpf mit Gras umgeben, Berge in Ringen ansteigen lassen, keine Stacheln.")]
        public bool SeparateBiomes { get; set; } = true;

        [Category("7 Umriss-Maske"), DisplayName("Umriss-Maske benutzen"), Description("Land nur dort, wo die geladene Umriss-Maske Land ist.")]
        public bool UseOutlineMask { get; set; } = true;

        [Category("7 Umriss-Maske"), DisplayName("Küsten-Unschärfe (Tiles)"), Description("Weicht die Maske auf, damit die Küste natürlich wirkt.")]
        public int MaskBlur { get; set; } = 12;

        [Category("7 Umriss-Maske"), DisplayName("Küsten-Rauheit (0..1)"), Description("0 = Küste exakt wie die Maske, 1 = stark verwirbelt.")]
        public double MaskRoughness { get; set; } = 0.35;

        [Browsable(false)]
        public List<GeneratorBiome> Biomes { get; set; } = CreateDefaultBiomes();

        /// <summary>Biomes for the IMod13 groups (temperature: 0 cold … 1 hot, moisture: 0 dry … 1 wet).</summary>
        public static List<GeneratorBiome> CreateDefaultBiomes()
        {
            return new List<GeneratorBiome>
            {
                new GeneratorBiome { Name = "Schnee", Group = 0x07, MaxTemperature = 0.16, MinMoisture = 0, MaxMoisture = 0.45 },
                new GeneratorBiome { Name = "Schneewald", Group = 0x17, MaxTemperature = 0.2, MinMoisture = 0.45, MaxMoisture = 1 },
                new GeneratorBiome { Name = "Schneewiese", Group = 0x16, MinTemperature = 0.16, MaxTemperature = 0.26, MaxMoisture = 0.45 },
                new GeneratorBiome { Name = "Sumpf", Group = 0x18, MinTemperature = 0.4, MaxTemperature = 0.75, MinMoisture = 0.82, MaxMoisture = 1 },
                new GeneratorBiome { Name = "Dschungel", Group = 0x08, MinTemperature = 0.72, MaxTemperature = 1, MinMoisture = 0.58, MaxMoisture = 1 },
                new GeneratorBiome { Name = "Wüste", Group = 0x05, MinTemperature = 0.8, MaxTemperature = 1, MinMoisture = 0, MaxMoisture = 0.3 },
                new GeneratorBiome { Name = "Savanne", Group = 0x1a, MinTemperature = 0.7, MaxTemperature = 1, MinMoisture = 0.3, MaxMoisture = 0.58 },
                new GeneratorBiome { Name = "Wald", Group = 0x01, MinTemperature = 0.2, MaxTemperature = 0.75, MinMoisture = 0.55, MaxMoisture = 0.82 },
                new GeneratorBiome { Name = "Prärie", Group = 0x19, MinTemperature = 0.45, MaxTemperature = 0.8, MinMoisture = 0.15, MaxMoisture = 0.35 },
                new GeneratorBiome { Name = "Ödland", Group = 0x0b, MinTemperature = 0.55, MaxTemperature = 0.85, MinMoisture = 0, MaxMoisture = 0.15 },
                new GeneratorBiome { Name = "Gras", Group = 0x00, MinTemperature = 0, MaxTemperature = 1, MinMoisture = 0, MaxMoisture = 1 }
            };
        }

        public GeneratorSettings Clone()
        {
            var c = (GeneratorSettings)MemberwiseClone();
            c.Biomes = Biomes.Select(b => b.Clone()).ToList();
            return c;
        }
    }

    /// <summary>
    /// Generates a Dragon map: noise height field (optionally shaped by an outline mask),
    /// sea level by land percentage, ridged mountains, climate biomes. The altitude of every tile
    /// is encoded by choosing the colour of the group whose altitude fits (like the IMod colour table).
    /// </summary>
    public sealed class MapGenerator
    {
        private readonly DragonRuleSet _rules;
        private readonly GeneratorSettings _s;
        private readonly int[][] _altLookup = new int[256][];

        public MapGenerator(DragonRuleSet rules, GeneratorSettings settings)
        {
            _rules = rules;
            _s = settings;
        }

        /// <summary>
        /// Generates <paramref name="outW"/>×<paramref name="outH"/> tiles of a world with the logical size
        /// <paramref name="worldW"/>×<paramref name="worldH"/> (smaller output = preview of the same world).
        /// <paramref name="mask"/> (optional, world size) is 0 = water … 1 = land.
        /// </summary>
        public IndexedMap Generate(int worldW, int worldH, int outW, int outH, float[] mask, int maskW, int maskH,
            IProgress<int> progress, CancellationToken cancel)
        {
            int seed = _s.Seed;
            var heightNoise = new PerlinNoise(seed);
            var ridgeNoise = new PerlinNoise(seed + 101);
            var tempNoise = new PerlinNoise(seed + 202);
            var moistNoise = new PerlinNoise(seed + 303);
            var roughNoise = new PerlinNoise(seed + 404);

            double sx = (double)worldW / outW, sy = (double)worldH / outH;
            int n = outW * outH;
            var height = new float[n];
            var ridge = new float[n];
            bool useMask = mask != null && _s.UseOutlineMask;
            double span = Math.Max(worldW, worldH);
            double feature = Math.Max(16, Rel(_s.FeatureSize) * span);
            double mountainSize = Math.Max(16, Rel(_s.MountainSize) * span);
            double climate = Math.Max(16, Rel(_s.ClimateSize) * span);
            var warpNoise = new PerlinNoise(seed + 505);
            int octaves = Math.Clamp(_s.Octaves, 1, 12);

            int done = 0;
            Parallel.For(0, outH, new ParallelOptions { CancellationToken = cancel }, y =>
            {
                double wy = y * sy;
                for (int x = 0; x < outW; x++)
                {
                    double wx = x * sx;
                    double h = heightNoise.Fbm(wx / feature, wy / feature, octaves, _s.Persistence, _s.Lacunarity);

                    if (useMask)
                    {
                        int mx = Math.Min(maskW - 1, (int)(wx * maskW / worldW));
                        int my = Math.Min(maskH - 1, (int)(wy * maskH / worldH));
                        double m = mask[my * maskW + mx];
                        double rough = (roughNoise.Fbm(wx / 180.0, wy / 180.0, 5, 0.55, 2.0) - 0.5) * 2 * _s.MaskRoughness;
                        m = Math.Clamp(m + rough * 0.5, 0, 1);
                        // land inside the mask lies above 0.5, water below; the noise gives the relief
                        // inside: low near the coast, relief from the noise inland (same height range as without mask)
                        h = m >= 0.5 ? 0.5 + 0.005 + (m - 0.5) * (0.15 + 0.5 * h) : m * 0.9 * (0.6 + 0.4 * h);
                    }
                    else if (_s.EdgeFalloff > 0)
                    {
                        // rounded (super-ellipse) border, warped by noise so the coast does not look like a box
                        double warp = _s.EdgeWarp * 0.25;
                        double qx = wx / worldW + (warpNoise.Fbm(wx / (span * 0.15), wy / (span * 0.15), 4, 0.5, 2) - 0.5) * warp;
                        double qy = wy / worldH + (warpNoise.Fbm(wx / (span * 0.15) + 40, wy / (span * 0.15) + 40, 4, 0.5, 2) - 0.5) * warp;
                        double dx = Math.Abs(2 * qx - 1), dy = Math.Abs(2 * qy - 1);
                        double sd = Math.Pow(Math.Pow(dx, 4) + Math.Pow(dy, 4), 0.25);
                        double e = (1 - sd) / Math.Max(0.01, _s.EdgeWidth * 2);
                        double f = SmoothStep(Math.Clamp(e, 0, 1));
                        h *= PerlinNoise.Lerp(1, f, _s.EdgeFalloff);
                    }

                    int i = y * outW + x;
                    height[i] = (float)h;
                    ridge[i] = (float)ridgeNoise.Ridged(wx / mountainSize, wy / mountainSize, 5, 0.5, 2.1);
                }
                int d = Interlocked.Increment(ref done);
                if (progress != null && d % 64 == 0)
                {
                    progress.Report(d * 50 / outH);
                }
            });

            double seaLevel = useMask ? 0.5 : Percentile(height, 1 - _s.LandPercent / 100.0);

            // mountain score of land cells
            var landScores = new List<float>(n / 4);
            for (int i = 0; i < n; i += 3)
            {
                if (height[i] > seaLevel)
                {
                    landScores.Add(MountainScore(height[i], ridge[i], seaLevel));
                }
            }
            landScores.Sort();
            double mountainLevel = landScores.Count == 0 ? 2 : landScores[(int)Math.Clamp((1 - _s.MountainPercent / 100.0) * (landScores.Count - 1), 0, landScores.Count - 1)];
            double snowLevel = mountainLevel + (1 - mountainLevel) * (1 - Math.Clamp(_s.SnowPercent / 100.0, 0, 1));
            if (_s.MountainPercent <= 0)
            {
                mountainLevel = 2;
                snowLevel = 3;
            }

            byte water = IndexFor(_s.WaterGroup, _s.WaterAltitude);
            int snowMax = MaxAltitudeOf(_s.SnowGroup);
            var map = new IndexedMap(outW, outH, water);
            List<GeneratorBiome> biomes = _s.Biomes.Where(b => b.Enabled).ToList();
            done = 0;

            Parallel.For(0, outH, new ParallelOptions { CancellationToken = cancel }, y =>
            {
                double wy = y * sy;
                double lat = worldH > 0 ? wy / worldH : 0.5;
                for (int x = 0; x < outW; x++)
                {
                    int i = y * outW + x;
                    double h = height[i];
                    if (h <= seaLevel)
                    {
                        map.Pixels[i] = water;
                        continue;
                    }
                    double wx = x * sx;
                    double r = (h - seaLevel) / Math.Max(1e-6, 1 - seaLevel);
                    double ms = MountainScore(height[i], ridge[i], seaLevel);

                    if (ms >= mountainLevel)
                    {
                        double t = Math.Clamp((ms - mountainLevel) / Math.Max(1e-6, 1 - mountainLevel), 0, 1);
                        int ground = (int)Math.Round(Math.Clamp(r * 2.2, 0, 1) * _s.MaxLandAltitude);
                        int alt = Math.Min(127, ground + (int)Math.Round(PerlinNoise.Lerp(_s.MinMountainAltitude, _s.MaxMountainAltitude, t)));
                        int group = ms >= snowLevel && _s.SnowPercent > 0 ? _s.SnowGroup : _s.MountainGroup;
                        if (group == _s.SnowGroup)
                        {
                            alt = Math.Min(alt, snowMax);
                        }
                        map.Pixels[i] = IndexFor(group, alt);
                        continue;
                    }

                    int landAlt = (int)Math.Round(Math.Clamp(r * 2.2, 0, 1) * _s.MaxLandAltitude);

                    double temp = (_s.NorthCold ? lat : 0.5) * 0.65 + tempNoise.Fbm(wx / climate, wy / climate, 4, 0.5, 2) * 0.45 - r * _s.HeightCooling;
                    temp = Math.Clamp(temp, 0, 1);
                    double moist = Math.Clamp((moistNoise.Fbm(wx / (climate * 0.7), wy / (climate * 0.7), 5, 0.5, 2) - 0.5) * 1.6 + 0.5, 0, 1);

                    int g = _s.FallbackGroup;
                    foreach (GeneratorBiome b in biomes)
                    {
                        if (temp >= b.MinTemperature && temp <= b.MaxTemperature && moist >= b.MinMoisture && moist <= b.MaxMoisture)
                        {
                            g = b.Group;
                            break;
                        }
                    }
                    map.Pixels[i] = IndexFor(g, landAlt);
                }
                int d = Interlocked.Increment(ref done);
                if (progress != null && d % 64 == 0)
                {
                    progress.Report(50 + d * 50 / outH);
                }
            });

            AddBeaches(map, height, seaLevel, ridge, mountainLevel, sx, seed, cancel);
            if (_s.SeparateBiomes)
            {
                // tutorial rules: no 1 tile strips, bridge groups, swamp in grass, mountains in rings, no spikes
                var fix = new MapFixOptions
                {
                    WaterGroup = _s.WaterGroup,
                    MountainGroup = _s.MountainGroup,
                    FallbackGroup = _s.FallbackGroup
                };
                MapFixer.Fix(map, _rules, fix, null, cancel);
            }
            if (_s.CaveEntrances > 0)
            {
                AddCaveEntrances(map, seed, cancel);
            }
            progress?.Report(100);
            return map;
        }

        /// <summary>Places 2×2 invisible cave entrances in the mountain border (one side touches the land).</summary>
        private void AddCaveEntrances(IndexedMap map, int seed, CancellationToken cancel)
        {
            int cave = _s.CaveGroup & 0xFF, mountain = _s.MountainGroup, water = _s.WaterGroup;
            List<DragonPaletteEntry> caveColours = _rules.EntriesOfGroup(cave);
            if (caveColours.Count == 0)
            {
                return;
            }
            var groupOf = new int[256];
            var altOf = new int[256];
            foreach (DragonPaletteEntry e in _rules.Palette)
            {
                groupOf[e.Index] = e.Group;
                altOf[e.Index] = e.Altitude;
            }
            int w = map.Width, h = map.Height;
            byte[] px = map.Pixels;
            int G(int x, int y) => x < 0 || y < 0 || x >= w || y >= h ? -1 : groupOf[px[y * w + x]];
            bool IsLand(int x, int y)
            {
                int g = G(x, y);
                return g >= 0 && g != mountain && g != water && g != cave;
            }
            int[] dxs = { -1, 1, 0, 0 }, dys = { 0, 0, -1, 1 };
            var candidates = new List<(int x, int y, int dx, int dy)>();
            for (int y = 2; y < h - 2; y += 2)
            {
                for (int x = 2; x < w - 2; x += 2)
                {
                    if (G(x, y) != mountain)
                    {
                        continue;
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        int px1 = dys[k], py1 = dxs[k]; // along the border
                        // (x,y) and (x+p) touch land in direction d, the cells behind are mountain
                        if (IsLand(x + dxs[k], y + dys[k]) && IsLand(x + px1 + dxs[k], y + py1 + dys[k]) &&
                            G(x + px1, y + py1) == mountain && G(x - dxs[k], y - dys[k]) == mountain &&
                            G(x + px1 - dxs[k], y + py1 - dys[k]) == mountain)
                        {
                            candidates.Add((x, y, dxs[k], dys[k]));
                            break;
                        }
                    }
                }
                if ((y & 63) == 0)
                {
                    cancel.ThrowIfCancellationRequested();
                }
            }
            var rnd = new Random(seed + 707);
            var placed = new List<(int x, int y)>();
            int minDist = Math.Max(16, Math.Max(w, h) / 12);
            foreach (var c in candidates.OrderBy(_ => rnd.Next()))
            {
                if (placed.Count >= _s.CaveEntrances)
                {
                    break;
                }
                if (placed.Any(p => Math.Abs(p.x - c.x) + Math.Abs(p.y - c.y) < minDist))
                {
                    continue;
                }
                int groundZ = altOf[px[(c.y + c.dy) * w + c.x + c.dx]];
                int alt = groundZ + 10;
                DragonPaletteEntry colour = caveColours.OrderBy(e => Math.Abs(e.Altitude - alt)).First();
                int pX = c.dy, pY = c.dx;
                foreach ((int x, int y) in new[] { (c.x, c.y), (c.x + pX, c.y + pY), (c.x - c.dx, c.y - c.dy), (c.x + pX - c.dx, c.y + pY - c.dy) })
                {
                    px[y * w + x] = (byte)colour.Index;
                }
                placed.Add((c.x, c.y));
            }
        }

        private static double Rel(double v) => v > 1.5 ? v / 6144.0 : Math.Clamp(v, 0.005, 2);

        /// <summary>Land cells within BeachWidth tiles of the sea become beach (with noise gaps).</summary>
        private void AddBeaches(IndexedMap map, float[] height, double seaLevel, float[] ridge, double mountainLevel, double scale, int seed, CancellationToken cancel)
        {
            int width = (int)Math.Round(_s.BeachWidth / Math.Max(1, scale));
            if (_s.BeachWidth <= 0 || _s.BeachPercent <= 0)
            {
                return;
            }
            width = Math.Max(1, width);
            int w = map.Width, h = map.Height, n = w * h;
            var dist = new int[n];
            var queue = new Queue<int>();
            for (int i = 0; i < n; i++)
            {
                if (height[i] <= seaLevel)
                {
                    dist[i] = 0;
                    queue.Enqueue(i);
                }
                else
                {
                    dist[i] = int.MaxValue;
                }
            }
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                int d = dist[i] + 1;
                if (d > width)
                {
                    continue;
                }
                int x = i % w, y = i / w;
                if (x > 0 && dist[i - 1] > d) { dist[i - 1] = d; queue.Enqueue(i - 1); }
                if (x < w - 1 && dist[i + 1] > d) { dist[i + 1] = d; queue.Enqueue(i + 1); }
                if (y > 0 && dist[i - w] > d) { dist[i - w] = d; queue.Enqueue(i - w); }
                if (y < h - 1 && dist[i + w] > d) { dist[i + w] = d; queue.Enqueue(i + w); }
            }
            cancel.ThrowIfCancellationRequested();
            var gapNoise = new PerlinNoise(seed + 606);
            double gapLevel = 1 - Math.Clamp(_s.BeachPercent / 100.0, 0, 1);
            byte beach = IndexFor(_s.BeachGroup, 0);
            double gapScale = 60.0 / Math.Max(1, scale);
            for (int i = 0; i < n; i++)
            {
                if (dist[i] == 0 || dist[i] > width)
                {
                    continue;
                }
                if (MountainScore(height[i], ridge[i], seaLevel) >= mountainLevel)
                {
                    continue;
                }
                int x = i % w, y = i / w;
                if (gapLevel > 0 && gapNoise.Fbm(x / gapScale, y / gapScale, 3, 0.5, 2) < gapLevel)
                {
                    continue;
                }
                map.Pixels[i] = beach;
            }
        }

        private static float MountainScore(float h, float ridge, double sea)
        {
            double r = (h - sea) / Math.Max(1e-6, 1 - sea);
            return (float)(ridge * 0.7 + Math.Clamp(r * 2, 0, 1) * 0.3);
        }

        private static double SmoothStep(double t) => t * t * (3 - 2 * t);

        private static double Percentile(float[] values, double p)
        {
            var hist = new int[2048];
            foreach (float v in values)
            {
                hist[Math.Clamp((int)(v * 2047), 0, 2047)]++;
            }
            long target = (long)(Math.Clamp(p, 0, 1) * values.Length);
            long sum = 0;
            for (int i = 0; i < hist.Length; i++)
            {
                sum += hist[i];
                if (sum >= target)
                {
                    return i / 2047.0;
                }
            }
            return 1;
        }

        private int MaxAltitudeOf(int group)
        {
            List<DragonPaletteEntry> entries = _rules.EntriesOfGroup(group);
            return entries.Count == 0 ? 127 : entries.Max(e => e.Altitude);
        }

        /// <summary>Colour of <paramref name="group"/> nearest to <paramref name="alt"/> (cached; falls back to the fallback group).</summary>
        private byte IndexFor(int group, int alt)
        {
            group &= 0xFF;
            int[] lookup = _altLookup[group];
            if (lookup == null)
            {
                lookup = new int[256];
                for (int z = -128; z < 128; z++)
                {
                    lookup[z + 128] = _rules.NearestIndex(group, z);
                }
                _altLookup[group] = lookup;
            }
            int idx = lookup[Math.Clamp(alt, -128, 127) + 128];
            if (idx < 0 && group != (_s.FallbackGroup & 0xFF))
            {
                return IndexFor(_s.FallbackGroup, alt);
            }
            return (byte)Math.Max(0, idx);
        }
    }
}
