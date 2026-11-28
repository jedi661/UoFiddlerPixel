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
        [Category("0 Sprache / Language"), DisplayName("Sprache / Language / Langue"), Description("Sprache der Oberfläche: Deutsch, English, Français. Auch im Menü 'Sprache / Language / Langue'.")]
        public UiLanguage Language { get; set; } = UiLanguage.Deutsch;

        /// <summary>Mouse button of "Sprung ins Live-CGI" (e.g. "XButton1"), empty = none.</summary>
        [Browsable(false)]
        public string LiveJumpButton { get; set; } = string.Empty;

        /// <summary>Modifier keys that belong to <see cref="LiveJumpButton"/> (Control / Alt).</summary>
        [Browsable(false)]
        public int LiveJumpModifiers { get; set; }

        /// <summary>A jump also switches "folgt Maus" off (default: no, drawing and following stay as they are).</summary>
        [Browsable(false)]
        public bool LiveJumpStopsFollow { get; set; }

        /// <summary>Map editor colour tools: keep the height of the ground (same group at that height, else the height layer).</summary>
        [Browsable(false)]
        public bool BrushKeepHeight { get; set; }

        /// <summary>Map editor: show the cells of the height layer.</summary>
        [Browsable(false)]
        public bool ShowHeightLayer { get; set; }

        /// <summary>Tool palette of the map editor in its own window: "x,y,w,h", empty = docked at the left.</summary>
        [Browsable(false)]
        public string EditorPaletteWindow { get; set; } = string.Empty;

        /// <summary>Tool palette of the CGI view in its own window: "x,y,w,h", empty = docked at the left.</summary>
        [Browsable(false)]
        public string CgiPaletteWindow { get; set; } = string.Empty;

        /// <summary>Own window of the tool options ("x,y,w,h", empty = in the options bar).</summary>
        public string ToolOptionsWindow { get; set; } = string.Empty;

        /// <summary>Own window of the CGI side panel ("x,y,w,h", empty = docked).</summary>
        public string SideWindow { get; set; } = string.Empty;

        public bool ToolOptionsOpen { get; set; }

        /// <summary>Version of the settings (one time changes of defaults).</summary>
        [Browsable(false)]
        public int SettingsLevel { get; set; }

        public bool SideWindowOpen { get; set; }

        /// <summary>Palettes / side panel switched off (◧ / ◨, F7 / F8) – they stay off at the next start.</summary>
        [Browsable(false)]
        public bool CgiPaletteHidden { get; set; }

        [Browsable(false)]
        public bool EditorPaletteHidden { get; set; }

        [Browsable(false)]
        public bool CgiSideCollapsed { get; set; }

        /// <summary>Width of the docked CGI side panel (splitter of the CGI view), 0 = default.</summary>
        [Browsable(false)]
        public int CgiSideWidth { get; set; }

        /// <summary>Region window: "x,y,w,h", splitters, detached panes ("x,y,w,h" or empty = docked).</summary>
        [Browsable(false)]
        public string RegionWindow { get; set; } = string.Empty;

        [Browsable(false)]
        public int RegionSplit { get; set; }

        [Browsable(false)]
        public int RegionSideSplit { get; set; }

        [Browsable(false)]
        public string RegionLevelPane { get; set; } = string.Empty;

        [Browsable(false)]
        public string RegionBuildPane { get; set; } = string.Empty;

        /// <summary>View helpers of the CGI view: coordinate lines (0 = off), tile info, height scale.</summary>
        [Browsable(false)]
        public int ViewCoordLines { get; set; }

        [Browsable(false)]
        public bool ViewTileInfo { get; set; }

        [Browsable(false)]
        public bool ViewHeightScale { get; set; }

        /// <summary>Floor tile sets of the tool "Höhle" (own textures can be added).</summary>
        [Browsable(false)]
        public List<CaveTileSet> CaveTileSets { get; set; } = CaveTileSet.Defaults();

        /// <summary>Cave tool window: "x,y,w,h", and whether it opens with the tools Höhle / Eingang.</summary>
        [Browsable(false)]
        public string CaveWindow { get; set; } = string.Empty;

        [Browsable(false)]
        public bool CaveWindowAuto { get; set; } = true;

        /// <summary>Height of the cellar level (absolute Z).</summary>
        [Browsable(false)]
        public int ViewLayerZ { get; set; } = -40;

        /// <summary>Map editor ↔ CGI view when the tab changes: 0 = automatically, 1 = ask, 2 = off (only warn).</summary>
        [Browsable(false)]
        public int SyncMode { get; set; }

        /// <summary>New brushes of the CGI view (scatter, wall, river, road …), view filter, bookmarks.</summary>
        [Browsable(false)]
        public CgiBrushSettings CgiBrushes { get; set; } = new CgiBrushSettings();

        /// <summary>"Küste glätten" of the CGI view.</summary>
        [Browsable(false)]
        public CoastSmoothSettings CoastSmooth { get; set; } = new CoastSmoothSettings();

        /// <summary>Language of the help window: "de", "en", empty = like the user interface.</summary>
        [Browsable(false)]
        public string HelpLanguage { get; set; } = string.Empty;

        [Category("1 Ordner"), DisplayName("Projekt-Ordner (Karten)"), Description("Hier speichert der Editor die Karten-BMPs; die Kartenliste zeigt diesen Ordner.")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string ProjectFolder { get; set; } = string.Empty;

        [Category("1 Ordner"), DisplayName("Farbstempel-Ordner"), Description("Hier liegen die Farb-Stempel des Karten-Editors (*.dmstamp.json). Leer = Unterordner 'Farbstempel' im Projekt-Ordner.")]
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string ColorStampFolder { get; set; } = string.Empty;

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

        /// <summary>Own cave land tiles (hex) for the gallery in the tab "Höhle / Weg" – in addition to the tiledata names with "cave".</summary>
        public string CaveTileIds { get; set; } = string.Empty;

        /// <summary>Mountain brush / area of the CGI view (tab "Berg").</summary>
        public MountainSettings Mountain { get; set; } = new MountainSettings();

        /// <summary>Cave stamp of the CGI view (tab "Höhlen-Stempel").</summary>
        public CaveStampSettings CaveStamp { get; set; } = new CaveStampSettings();

        /// <summary>Cave entrance stamp of the 2D map editor (colours).</summary>
        public CaveColorOptions CaveColor { get; set; } = new CaveColorOptions();

        /// <summary>Pattern brush / stamps: which heights are drawn.</summary>
        public PatternHeightMode PatternHeight { get; set; } = PatternHeightMode.Terrain;

        /// <summary>Pattern brush: fixed height (mode Fixed).</summary>
        public int PatternHeightValue { get; set; }

        /// <summary>Pattern brush: added to the height in every mode (e.g. -3 for a river bed).</summary>
        public int PatternHeightOffset { get; set; }

        /// <summary>Pattern brush: after a stroke, height spikes at the stroke are smoothed and transitions set again.</summary>
        public bool PatternAutoFix { get; set; } = true;

        /// <summary>Folded sections of the CGI side panel.</summary>
        public List<string> CgiCollapsed { get; set; } = new List<string>();

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
            if (s.SettingsLevel < 28)
            {
                // Werkstatt 28: the soft coast is switched on once for existing settings (can be switched off again)
                s.Build ??= new BuildOptions();
                s.Build.CoastSoft = true;
                s.SettingsLevel = 28;
            }
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
