/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Settings of the region editor (<AppData>\RegionEditor.json):
 * server folders, export folder, language, theme, view options.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.RegionEditor.Formats;

namespace UoFiddler.Plugin.RegionEditor.Classes
{
    public enum EditorTheme
    {
        Light,
        Dark
    }

    public sealed class RegionEditorSettings
    {
        private static RegionEditorSettings _current;

        /// <summary>raised after <see cref="Save"/> (theme, folders … changed)</summary>
        public static event EventHandler Changed;

        public static RegionEditorSettings Current => _current ?? (_current = Load());

        // ── server folders ──
        /// <summary>ServUO/RunUO main folder (contains Data\Regions.xml)</summary>
        public string ServUoDir { get; set; } = string.Empty;

        /// <summary>Sphere areas file (e.g. scripts\maps\map0\map0_areas.scp)</summary>
        public string SphereFile { get; set; } = string.Empty;

        /// <summary>POL regions folder (regions.cfg, justice.cfg, music.cfg)</summary>
        public string PolDir { get; set; } = string.Empty;

        /// <summary>folder for exports that are copied to a server by hand</summary>
        public string ExportDir { get; set; } = string.Empty;

        // ── export options ──
        public MergeMode Merge { get; set; } = MergeMode.ReplaceFile;
        public bool WidthHeight { get; set; } = true;
        public bool SkipHidden { get; set; }
        public bool PolGuardTexts { get; set; } = true;
        public string SphereEvents { get; set; } = "r_default";

        // ── editor ──
        public string Language { get; set; }
        public EditorTheme Theme { get; set; } = EditorTheme.Light;
        public int Snap { get; set; } = 1;
        public int DefaultPriority { get; set; } = 50;
        public bool ShowStatics { get; set; } = true;
        public bool ShowGrid { get; set; }
        public bool ShowLabels { get; set; } = true;
        public bool ShowMinimap { get; set; } = true;
        public bool ShowClient { get; set; } = true;
        public int FillAlpha { get; set; } = 70;
        public string LastProject { get; set; }
        public List<string> RecentFiles { get; set; } = new List<string>();

        /// <summary>Regions.xml of the configured ServUO folder</summary>
        public string ServUoRegionsFile => string.IsNullOrWhiteSpace(ServUoDir) ? null : ServUoRegionsPath(ServUoDir);

        /// <summary>accepts the server folder, its Data folder or the file itself</summary>
        public static string ServUoRegionsPath(string dir)
        {
            if (dir.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                return dir;
            }

            return string.Equals(Path.GetFileName(dir.TrimEnd('\\', '/')), "Data", StringComparison.OrdinalIgnoreCase)
                ? Path.Combine(dir, "Regions.xml")
                : Path.Combine(dir, "Data", "Regions.xml");
        }

        public string EffectiveExportDir => string.IsNullOrWhiteSpace(ExportDir)
            ? Path.Combine(Options.AppDataPath ?? Path.GetTempPath(), "RegionExport")
            : ExportDir;

        private static string FileName => Path.Combine(Options.AppDataPath ?? string.Empty, "RegionEditor.json");

        public static RegionEditorSettings Load()
        {
            try
            {
                if (File.Exists(FileName))
                {
                    return JsonSerializer.Deserialize<RegionEditorSettings>(File.ReadAllText(FileName)) ?? new RegionEditorSettings();
                }
            }
            catch (Exception)
            {
                // broken file: defaults
            }

            return new RegionEditorSettings();
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(FileName, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                // ignored
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void AddRecent(string file)
        {
            RecentFiles.RemoveAll(f => string.Equals(f, file, StringComparison.OrdinalIgnoreCase));
            RecentFiles.Insert(0, file);
            if (RecentFiles.Count > 10)
            {
                RecentFiles.RemoveRange(10, RecentFiles.Count - 10);
            }
        }
    }

    /// <summary><see cref="IMapInfo"/> with the maps of the <see cref="MapRegistry"/></summary>
    public sealed class UltimaMapInfo : IMapInfo
    {
        public Size GetSize(int mapId)
        {
            MapDefinition def = MapRegistry.GetById(mapId);
            return def == null ? Size.Empty : new Size(def.Width, def.Height);
        }

        public bool IsWater(int mapId, int x, int y)
        {
            MapDefinition def = MapRegistry.GetById(mapId);
            if (def == null || x < 0 || y < 0 || x >= def.Width || y >= def.Height)
            {
                return false;
            }

            Map map = def.GetMap();
            Tile land = map.Tiles.GetLandTile(x, y);
            bool water = land.Id < TileData.LandTable.Length && (TileData.LandTable[land.Id].Flags & TileFlag.Wet) != 0;
            if (!water)
            {
                return false;
            }

            // a bridge, a dock … above the water
            foreach (HuedTile s in map.Tiles.GetStaticTiles(x, y))
            {
                if (s.Id < TileData.ItemTable.Length && (TileData.ItemTable[s.Id].Flags & (TileFlag.Surface | TileFlag.Bridge)) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>land tile id and z, -1 when outside</summary>
        public static (int Id, int Z) Land(int mapId, int x, int y)
        {
            MapDefinition def = MapRegistry.GetById(mapId);
            if (def == null || x < 0 || y < 0 || x >= def.Width || y >= def.Height)
            {
                return (-1, 0);
            }

            Tile t = def.GetMap().Tiles.GetLandTile(x, y);
            return (t.Id, t.Z);
        }
    }
}
