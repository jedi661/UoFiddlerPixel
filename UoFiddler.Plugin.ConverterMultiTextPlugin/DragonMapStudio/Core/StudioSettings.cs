// /***************************************************************************
//  *
//  * Dragon Map Studio – settings (folders, last files, options)
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
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms.Design;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    /// <summary>How a clicked colour is copied to the clipboard.</summary>
    public enum CopyFormat
    {
        /// <summary>"0a" – like in the Dragon scripts.</summary>
        HexIndex,
        /// <summary>"0x0A"</summary>
        HexIndex0x,
        /// <summary>"10"</summary>
        DecimalIndex,
        /// <summary>"#RRGGBB"</summary>
        HtmlColor,
        /// <summary>"R,G,B"</summary>
        Rgb,
        /// <summary>The complete maptrans line "0a 020 0003 0004 …"</summary>
        MapTransLine,
        /// <summary>Everything: index, colour, group, altitude, tiles.</summary>
        Full
    }

    /// <summary>All settings of the studio (JSON, %AppData%\UoFiddler\DragonMapStudio\settings.json).</summary>
    public sealed class StudioSettings
    {
        [Category("1 Ordner"), DisplayName("Projekt-Ordner (Karten)"), Description("Hier speichert der Editor die Karten-BMPs; die Kartenliste zeigt diesen Ordner.")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string ProjectFolder { get; set; } = string.Empty;

        [Category("1 Ordner"), DisplayName("Ausgabe-Ordner (MUL / UOP)"), Description("Ziel für map#.mul, staidx#.mul, statics#.mul, map#LegacyMUL.uop.")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string OutputFolder { get; set; } = string.Empty;

        [Category("1 Ordner"), DisplayName("Dragon Skript-Ordner"), Description("Ordner mit groups.txt, maptrans.txt … (Standard: mitgelieferte IMod13-Skripte).")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string ScriptFolder { get; set; } = string.Empty;

        [Category("1 Ordner"), DisplayName("Farbtabelle (Bild)"), Description("Bild der Farbtabelle (IMod13_Colortable.bmp). Klick auf eine Farbe = Adresse kopieren.")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string ColorTableImage { get; set; } = string.Empty;

        [Category("1 Ordner"), DisplayName("Palette"), Description("Palette für die 256 Farben (BMP, ACT, PAL, ACO, GPL). Leer = Farbtabelle.")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string PaletteFile { get; set; } = string.Empty;

        [Category("1 Ordner"), DisplayName("Regelsatz (JSON)"), Description("Eigenes Format mit allen Infos. Wenn gesetzt und vorhanden, wird es statt der Skripte geladen.")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string RuleSetFile { get; set; } = string.Empty;

        [Category("1 Ordner"), DisplayName("Vorlagen-Ordner (Gebäude / Böden)"), Description("Gebäude (UoFiddler-txt, UO Architect .uoa, .wsc, Sphere .scp) und eigene Vorlagen (.dmsv.json). Jeder Unterordner ist eine Kategorie. Standard: mitgelieferter Ordner Multis.")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string TemplateFolder { get; set; } = string.Empty;

        [Category("1 Ordner"), DisplayName("maptrans-Datei"), Description("maptrans.txt oder maptrans105.txt …")]
        public string MapTransFile { get; set; } = "maptrans.txt";

        [Category("2 Farbtabelle"), DisplayName("Kopier-Format"), Description("Was beim Klick auf eine Farbe in die Zwischenablage kommt.")]
        public CopyFormat CopyFormat { get; set; } = CopyFormat.HexIndex;

        [Category("2 Farbtabelle"), DisplayName("Zoom Farbtabelle")]
        public int ColorTableZoom { get; set; } = 2;

        [Category("3 Editor"), DisplayName("Raster ab Zoom"), Description("8×8-Blockraster ab diesem Zoom anzeigen.")]
        public int GridFromZoom { get; set; } = 6;

        [Category("3 Editor"), DisplayName("Undo-Speicher (MB)")]
        public int UndoMemoryMb { get; set; } = 768;

        [Category("3 Editor"), DisplayName("Neue Karte: Breite")]
        public int NewMapWidth { get; set; } = 6144;

        [Category("3 Editor"), DisplayName("Neue Karte: Höhe")]
        public int NewMapHeight { get; set; } = 4096;

        [Category("3 Editor"), DisplayName("Fremde 8-Bit-Bilder umrechnen"), Description("Aus = Pixelwerte direkt übernehmen (wie Dragon). An = Farben vergleichen.")]
        public bool RemapForeignPalettes { get; set; }

        [Browsable(false)]
        public List<string> RecentMaps { get; set; } = new List<string>();

        [Browsable(false)]
        public string LastMap { get; set; } = string.Empty;

        [Browsable(false)]
        public GeneratorSettings Generator { get; set; } = new GeneratorSettings();

        [Browsable(false)]
        public OutlineSettings Outline { get; set; } = new OutlineSettings();

        [Browsable(false)]
        public BuildOptions Build { get; set; } = new BuildOptions();

        [Browsable(false)]
        public int PrimaryIndex { get; set; }

        [Browsable(false)]
        public int SecondaryIndex { get; set; } = 0x20;

        [Browsable(false)]
        public int BrushSize { get; set; } = 8;

        /// <summary>Version of the settings file (for changed defaults).</summary>
        [Browsable(false)]
        public int SettingsVersion { get; set; }

        public const int CurrentSettingsVersion = 6;

        // ------------------------------------------------------------------ storage

        [JsonIgnore, Browsable(false)]
        public static string SettingsFolder
        {
            get
            {
                string root = null;
                try
                {
                    root = UoFiddler.Controls.Classes.Options.AppDataPath;
                }
                catch (Exception)
                {
                    // UoFiddler not initialised (designer) – use AppData
                }
                if (string.IsNullOrEmpty(root))
                {
                    root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UoFiddler");
                }
                return Path.Combine(root, "DragonMapStudio");
            }
        }

        [JsonIgnore, Browsable(false)]
        public static string SettingsFile => Path.Combine(SettingsFolder, "settings.json");

        private static JsonSerializerOptions JsonOptions => new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };

        public static StudioSettings Load()
        {
            StudioSettings s = null;
            try
            {
                if (File.Exists(SettingsFile))
                {
                    s = JsonSerializer.Deserialize<StudioSettings>(File.ReadAllText(SettingsFile), JsonOptions);
                }
            }
            catch (Exception)
            {
                // broken file -> defaults
            }
            s ??= new StudioSettings();
            s.ApplyDefaults();
            return s;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsFolder);
                File.WriteAllText(SettingsFile, JsonSerializer.Serialize(this, JsonOptions));
            }
            catch (Exception)
            {
                // not important enough to bother the user
            }
        }

        /// <summary>Fills empty folders with the shipped data / sensible defaults.</summary>
        public void ApplyDefaults()
        {
            Generator ??= new GeneratorSettings();
            Generator.Biomes ??= GeneratorSettings.CreateDefaultBiomes();
            Outline ??= new OutlineSettings();
            Build ??= new BuildOptions();
            Build.Fix ??= new MapFixOptions();
            if (SettingsVersion < CurrentSettingsVersion)
            {
                // version 3: defaults from the Dragon tutorial (one random static id per line, duplicates removed)
                Build.StaticMode = StaticTileMode.RandomOne;
                Build.RemoveDuplicateStatics = true;
                Generator.SeparateBiomes = true;
                Build.StaticZ = StaticZMode.SourceLowest;
                Build.SmoothCoast = true;
                // mountain heights are now relative to the ground
                Generator.MinMountainAltitude = 15;
                Generator.MaxMountainAltitude = 70;
                Build.Fix.SkipGroups = string.IsNullOrWhiteSpace(Build.Fix.SkipGroups) ? "1d" : Build.Fix.SkipGroups;
                Build.CoastProfile = true;
                Build.CoastStaticFixed = true;
                // version 6: values from the original map0 (map analysis)
                Build.CoastStaticZ = -5;
                Build.CoastLandHeights = "-3 -1 0";
                Build.CoastWaterWidth = 6;
                Build.CoastWaterZ = -15;
                SettingsVersion = CurrentSettingsVersion;
            }
            RecentMaps ??= new List<string>();

            string data = StudioPaths.DataFolder;
            if (string.IsNullOrWhiteSpace(ScriptFolder) || !Directory.Exists(ScriptFolder))
            {
                ScriptFolder = data == null ? string.Empty : Path.Combine(data, "Scripts");
            }
            if (string.IsNullOrWhiteSpace(ColorTableImage) || !File.Exists(ColorTableImage))
            {
                ColorTableImage = data == null ? string.Empty : Path.Combine(data, "Addon", "IMod13_Colortable.bmp");
            }
            if (string.IsNullOrWhiteSpace(TemplateFolder) || !Directory.Exists(TemplateFolder))
            {
                TemplateFolder = data == null ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DragonMapStudio", "Multis") : Path.Combine(data, "Multis");
            }
            string docs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DragonMapStudio");
            if (string.IsNullOrWhiteSpace(ProjectFolder))
            {
                ProjectFolder = Path.Combine(docs, "Maps");
            }
            if (string.IsNullOrWhiteSpace(OutputFolder))
            {
                OutputFolder = Path.Combine(docs, "Output");
            }
            if (string.IsNullOrWhiteSpace(Build.OutputFolder))
            {
                Build.OutputFolder = OutputFolder;
            }
        }

        public void AddRecent(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            RecentMaps.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            RecentMaps.Insert(0, path);
            while (RecentMaps.Count > 12)
            {
                RecentMaps.RemoveAt(RecentMaps.Count - 1);
            }
            LastMap = path;
        }
    }

    /// <summary>Finds the shipped Dragon data (Scripts + Addon).</summary>
    public static class StudioPaths
    {
        private static string _data;

        /// <summary>Folder that contains "Scripts" and "Addon" (null if not found).</summary>
        public static string DataFolder
        {
            get
            {
                if (_data != null)
                {
                    return _data;
                }
                var candidates = new List<string>();
                try
                {
                    string asm = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    if (!string.IsNullOrEmpty(asm))
                    {
                        candidates.Add(Path.Combine(asm, "DragonMapStudio", "DragonData"));
                        candidates.Add(Path.Combine(asm, "DragonData"));
                    }
                }
                catch (Exception)
                {
                    // ignore
                }
                candidates.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "DragonMapStudio", "DragonData"));
                candidates.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "DragonMapStudio"));
                candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "Data", "DragonMapStudio"));
                _data = candidates.FirstOrDefault(c => Directory.Exists(Path.Combine(c, "Scripts")));
                return _data;
            }
        }
    }
}
