/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * The client check: compares the client files with each other.
 *
 * The Ultima SDK reads through shared file streams and is not thread safe, so
 * all SDK calls run on the UI thread in small portions (the UI stays usable).
 * Only the map statics are read in the background with their own file streams.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Plugin.ClientCheck.Classes
{
    public sealed class CheckProgress
    {
        public string Step { get; set; }
        public int Percent { get; set; }
    }

    public sealed class CheckEngine
    {
        private const int Portion = 400;
        private const int MaxBody = 0x1000;

        private readonly List<CheckIssue> _issues = new List<CheckIssue>();
        private IProgress<CheckProgress> _progress;
        private CancellationToken _token;
        private Stopwatch _slice;

        private bool[] _validArt = Array.Empty<bool>();
        private bool[] _validLand = Array.Empty<bool>();
        private int _maxItem;

        public async Task<List<CheckIssue>> RunAsync(CheckCategory categories, IProgress<CheckProgress> progress, CancellationToken token)
        {
            _progress = progress;
            _token = token;
            _slice = Stopwatch.StartNew();
            _issues.Clear();

            var steps = new List<(CheckCategory Category, Func<Task> Run)>
            {
                (CheckCategory.ArtTileData, CheckArtTileData),
                (CheckCategory.LandTiles, CheckLandTiles),
                (CheckCategory.Equipment, CheckEquipment),
                (CheckCategory.Bodies, CheckBodies),
                (CheckCategory.RadarCol, CheckRadarCol),
                (CheckCategory.Multis, CheckMultis),
                (CheckCategory.Statics, CheckStatics),
                (CheckCategory.Cliloc, CheckCliloc)
            };
            List<(CheckCategory Category, Func<Task> Run)> selected = steps.Where(s => (categories & s.Category) != 0).ToList();

            Report(Loc.T("step.prepare"), 0);
            await PrepareValidity();

            for (int i = 0; i < selected.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                _stepBase = 5 + (i * 95 / Math.Max(1, selected.Count));
                _stepSpan = 95 / Math.Max(1, selected.Count);
                _stepName = Loc.T("cat." + selected[i].Category);
                Report(_stepName, _stepBase);
                await selected[i].Run();
            }

            Report(string.Empty, 100);
            return _issues.ToList();
        }

        #region Helpers

        private int _stepBase;
        private int _stepSpan;
        private string _stepName;

        private void Report(string step, int percent)
        {
            _progress?.Report(new CheckProgress { Step = step, Percent = Math.Max(0, Math.Min(100, percent)) });
        }

        /// <summary>Gives the UI time to react every ~40 ms (all SDK calls stay on the UI thread)</summary>
        private async Task Breathe(int done, int total)
        {
            _token.ThrowIfCancellationRequested();
            if (_slice.ElapsedMilliseconds < 40)
            {
                return;
            }

            if (_stepName != null && total > 0)
            {
                Report(_stepName, _stepBase + (int)((long)_stepSpan * done / total));
            }

            await Task.Delay(1);
            _slice.Restart();
        }

        private void Add(CheckSeverity severity, CheckCategory category, int id, string idText, string key,
            PreviewKind preview = PreviewKind.None, int previewId = 0, FixKind fix = FixKind.None, string fixData = null, params object[] args)
        {
            _issues.Add(new CheckIssue
            {
                Severity = severity,
                Category = category,
                Id = id,
                IdText = idText,
                Key = key,
                Args = args,
                Preview = preview,
                PreviewId = previewId,
                Fix = fix,
                FixData = fixData
            });
        }

        private static string Hex(int id) => $"0x{id:X4}";

        private static string ItemName(int id)
        {
            ItemData[] table = TileData.ItemTable;
            return table != null && id >= 0 && id < table.Length ? (table[id].Name ?? string.Empty).Trim() : string.Empty;
        }

        private static string LandName(int id)
        {
            LandData[] table = TileData.LandTable;
            return table != null && id >= 0 && id < table.Length ? (table[id].Name ?? string.Empty).Trim() : string.Empty;
        }

        private static bool IsNoDraw(string name) => name.IndexOf("nodraw", StringComparison.OrdinalIgnoreCase) >= 0;

        private static bool SafeAnim(int body)
        {
            try
            {
                return Animations.IsActionDefined(body, 0, 0);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool SafeGump(int id)
        {
            try
            {
                return id >= 0 && id <= 0xFFFE && Gumps.IsValidIndex(id);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ValidArt(int id) => id >= 0 && id < _validArt.Length && _validArt[id];

        #endregion

        #region Prepare

        private async Task PrepareValidity()
        {
            _maxItem = Art.GetMaxItemId();
            _validArt = new bool[_maxItem + 1];
            for (int id = 0; id <= _maxItem; id++)
            {
                try
                {
                    _validArt[id] = Art.IsValidStatic(id);
                }
                catch (Exception)
                {
                    _validArt[id] = false;
                }

                if (id % Portion == 0)
                {
                    await Breathe(id, _maxItem);
                    Report(Loc.T("step.prepare"), id * 4 / Math.Max(1, _maxItem));
                }
            }

            int landCount = TileData.LandTable?.Length ?? 0x4000;
            _validLand = new bool[landCount];
            for (int id = 0; id < landCount; id++)
            {
                try
                {
                    _validLand[id] = Art.IsValidLand(id);
                }
                catch (Exception)
                {
                    _validLand[id] = false;
                }

                if (id % Portion == 0)
                {
                    await Breathe(id, landCount);
                }
            }
        }

        #endregion

        #region Art / land / equipment

        private async Task CheckArtTileData()
        {
            ItemData[] table = TileData.ItemTable ?? Array.Empty<ItemData>();
            int max = Math.Max(_maxItem + 1, table.Length);
            for (int id = 1; id < max; id++)
            {
                bool valid = ValidArt(id);
                string name = id < table.Length ? (table[id].Name ?? string.Empty).Trim() : string.Empty;
                if (valid && name.Length == 0)
                {
                    Add(CheckSeverity.Info, CheckCategory.ArtTileData, id, Hex(id), "art.noname", PreviewKind.Item, id, args: new object[] { id });
                }
                else if (!valid && id < table.Length && name.Length > 0 && !IsNoDraw(name))
                {
                    Add(CheckSeverity.Info, CheckCategory.ArtTileData, id, Hex(id), "art.tiledatanoart", args: new object[] { id, name });
                }

                if (id % Portion == 0)
                {
                    await Breathe(id, max);
                }
            }
        }

        private async Task CheckLandTiles()
        {
            LandData[] table = TileData.LandTable ?? Array.Empty<LandData>();
            for (int id = 1; id < table.Length; id++)
            {
                bool valid = id < _validLand.Length && _validLand[id];
                string name = (table[id].Name ?? string.Empty).Trim();
                if (valid && name.Length == 0)
                {
                    Add(CheckSeverity.Info, CheckCategory.LandTiles, id, Hex(id), "land.noname", PreviewKind.Land, id, args: new object[] { id });
                }

                int texture = table[id].TextureId;
                if ((valid || name.Length > 0) && texture > 0 && !IsNoDraw(name))
                {
                    bool exists;
                    try
                    {
                        exists = Textures.TestTexture(texture);
                    }
                    catch (Exception)
                    {
                        exists = false;
                    }

                    if (!exists)
                    {
                        Add(CheckSeverity.Warning, CheckCategory.LandTiles, id, Hex(id), "land.notexture", PreviewKind.Land, id, args: new object[] { id, name, texture });
                    }
                }

                if (id % Portion == 0)
                {
                    await Breathe(id, table.Length);
                }
            }
        }

        private static HashSet<int> ReadGumpDef()
        {
            var set = new HashSet<int>();
            string path = Files.GetFilePath("gump.def");
            if (path == null || !File.Exists(path))
            {
                return set;
            }

            foreach (string raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                int brace = line.IndexOf('{');
                if (line.Length == 0 || line.StartsWith("#") || brace <= 0)
                {
                    continue;
                }

                if (int.TryParse(line.Substring(0, brace).Trim(), out int gump))
                {
                    set.Add(gump);
                }
            }

            return set;
        }

        private async Task CheckEquipment()
        {
            ItemData[] table = TileData.ItemTable ?? Array.Empty<ItemData>();
            HashSet<int> gumpDef = ReadGumpDef();
            var animCache = new Dictionary<int, bool>();

            for (int id = 1; id < table.Length; id++)
            {
                if (id % Portion == 0)
                {
                    await Breathe(id, table.Length);
                }

                ItemData data = table[id];
                if (!data.Wearable || !ValidArt(id))
                {
                    continue;
                }

                string name = (data.Name ?? string.Empty).Trim();
                int layer = data.Quality;
                int anim = data.Animation;

                if (layer == 0)
                {
                    Add(CheckSeverity.Warning, CheckCategory.Equipment, id, Hex(id), "equip.nolayer", PreviewKind.Item, id, args: new object[] { id, name });
                    continue;
                }

                // backpack and mount items have no paperdoll gump of their own
                if (anim <= 0 || layer == 21 || layer == 25)
                {
                    continue;
                }

                if (!animCache.TryGetValue(anim, out bool animOk))
                {
                    animOk = SafeAnim(anim);
                    animCache[anim] = animOk;
                }

                if (!animOk)
                {
                    Add(CheckSeverity.Error, CheckCategory.Equipment, id, Hex(id), "equip.noanim", PreviewKind.Item, id, args: new object[] { id, name, anim });
                }

                int male = 50000 + anim, female = 60000 + anim;
                bool maleOk = male > 0xFFFE || SafeGump(male) || gumpDef.Contains(male);
                if (!maleOk)
                {
                    Add(CheckSeverity.Warning, CheckCategory.Equipment, id, Hex(id), "equip.nogump", PreviewKind.Item, id, args: new object[] { id, name, male });
                }
                else if (female <= 0xFFFE && !SafeGump(female) && !gumpDef.Contains(female) && SafeGump(male))
                {
                    Add(CheckSeverity.Info, CheckCategory.Equipment, id, Hex(id), "equip.nogumpfemale", PreviewKind.Gump, male, args: new object[] { id, name, female });
                }
            }
        }

        #endregion

        #region Bodies

        private async Task CheckBodies()
        {
            string mobtypesPath = Files.GetFilePath("mobtypes.txt");
            Dictionary<int, string> mobtypes = ServUOAnimationSetup.ReadIdLines(mobtypesPath);
            Dictionary<int, BodyTableEntry> bodyDef = BodyTable.Entries ?? new Dictionary<int, BodyTableEntry>();

            var hasAnim = new bool[MaxBody];
            for (int body = 1; body < MaxBody; body++)
            {
                hasAnim[body] = SafeAnim(body);
                if (body % 100 == 0)
                {
                    await Breathe(body, MaxBody * 2);
                }
            }

            for (int body = 1; body < MaxBody; body++)
            {
                if (body % 50 == 0)
                {
                    await Breathe(MaxBody + body, MaxBody * 2);
                }

                // bodies that are only shown through a body.def redirect are not checked here
                if (bodyDef.ContainsKey(body))
                {
                    continue;
                }

                if (hasAnim[body] && mobtypesPath != null && !mobtypes.ContainsKey(body))
                {
                    int type = AnimationListStore.SuggestType(body, AnimationListControl.ActionNames);
                    string typeName = ServUOAnimationSetup.MobTypeName(type);
                    Add(CheckSeverity.Warning, CheckCategory.Bodies, body, $"Body {body}", "body.nomobtypes", PreviewKind.Animation, body,
                        FixKind.MobTypes, ServUOAnimationSetup.MobtypesLine(body, type), body, typeName);
                }
                else if (!hasAnim[body] && mobtypes.ContainsKey(body))
                {
                    Add(CheckSeverity.Info, CheckCategory.Bodies, body, $"Body {body}", "body.mobtypesnoanim", args: new object[] { body });
                }
            }

            // Bodyconv.def: body -> anim2..anim5 index without data
            int[][] tables = { BodyConverter.Table1, BodyConverter.Table2, BodyConverter.Table3, BodyConverter.Table4 };
            for (int t = 0; t < tables.Length; t++)
            {
                int[] table = tables[t];
                if (table == null)
                {
                    continue;
                }

                for (int body = 0; body < table.Length; body++)
                {
                    if (table[body] < 0 || bodyDef.ContainsKey(body))
                    {
                        continue;
                    }

                    if (!(body < MaxBody ? hasAnim[body] : SafeAnim(body)))
                    {
                        Add(CheckSeverity.Error, CheckCategory.Bodies, body, $"Body {body}", "body.bodyconvmissing", args: new object[] { body, t + 2, table[body] });
                    }

                    if (body % 100 == 0)
                    {
                        await Breathe(body, table.Length);
                    }
                }
            }

            // body.def: redirect to a body without animation
            foreach (KeyValuePair<int, BodyTableEntry> pair in bodyDef)
            {
                int target = pair.Value.OldId;
                if (!(target > 0 && target < MaxBody ? hasAnim[target] : SafeAnim(target)))
                {
                    Add(CheckSeverity.Warning, CheckCategory.Bodies, pair.Key, $"Body {pair.Key}", "body.bodydefmissing", args: new object[] { pair.Key, target });
                }
            }
        }

        #endregion

        #region radarcol

        private async Task CheckRadarCol()
        {
            ushort[] colors = RadarCol.Colors ?? Array.Empty<ushort>();
            int firstTooSmall = -1, tooSmall = 0;

            for (int id = 1; id <= _maxItem; id++)
            {
                if (id % Portion == 0)
                {
                    await Breathe(id, _maxItem + _validLand.Length);
                }

                if (!ValidArt(id))
                {
                    continue;
                }

                string name = ItemName(id);
                if (IsNoDraw(name))
                {
                    continue;
                }

                int index = id + 0x4000;
                if (index >= colors.Length)
                {
                    tooSmall++;
                    if (firstTooSmall < 0)
                    {
                        firstTooSmall = id;
                    }

                    continue;
                }

                if (colors[index] == 0)
                {
                    Add(CheckSeverity.Warning, CheckCategory.RadarCol, id, Hex(id), "radar.item", PreviewKind.Item, id, FixKind.RadarItem, args: new object[] { id, name });
                }
            }

            if (tooSmall > 0)
            {
                Add(CheckSeverity.Warning, CheckCategory.RadarCol, firstTooSmall, Hex(firstTooSmall), "radar.toosmall", PreviewKind.Item, firstTooSmall,
                    args: new object[] { firstTooSmall, Math.Max(0, colors.Length - 0x4000 - 1) });
            }

            for (int id = 1; id < _validLand.Length && id < 0x4000; id++)
            {
                if (id % Portion == 0)
                {
                    await Breathe(_maxItem + id, _maxItem + _validLand.Length);
                }

                if (!_validLand[id] || id >= colors.Length)
                {
                    continue;
                }

                string name = LandName(id);
                if (colors[id] == 0 && !IsNoDraw(name))
                {
                    Add(CheckSeverity.Warning, CheckCategory.RadarCol, id, Hex(id), "radar.land", PreviewKind.Land, id, FixKind.RadarLand, args: new object[] { id, name });
                }
            }
        }

        #endregion

        #region Multis

        private async Task CheckMultis()
        {
            for (int index = 0; index < Multis.MaximumMultiIndex; index++)
            {
                if (index % 20 == 0)
                {
                    await Breathe(index, Multis.MaximumMultiIndex);
                }

                MultiComponentList multi;
                try
                {
                    multi = Multis.GetComponents(index);
                }
                catch (Exception)
                {
                    continue;
                }

                if (multi == null || multi == MultiComponentList.Empty || multi.SortedTiles == null || multi.SortedTiles.Length == 0)
                {
                    continue;
                }

                int invalid = 0, first = -1;
                foreach (MultiComponentList.MultiTileEntry tile in multi.SortedTiles)
                {
                    if (!ValidArt(tile.ItemId))
                    {
                        invalid++;
                        if (first < 0)
                        {
                            first = tile.ItemId;
                        }
                    }
                }

                if (invalid > 0)
                {
                    Add(CheckSeverity.Error, CheckCategory.Multis, index, $"Multi {index}", "multi.invalid", PreviewKind.None, 0, args: new object[] { index, invalid, first });
                }
            }
        }

        #endregion

        #region Statics

        private sealed class MapFiles
        {
            public string Name;
            public string Index;
            public string Statics;
            public int BlockHeight;
        }

        private sealed class Aggregate
        {
            public int Count;
            public int Item;
            public string Sample;
        }

        private async Task CheckStatics()
        {
            // the paths are read on the UI thread, the files are read in the background with their own streams
            var maps = new List<MapFiles>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Map map in Map.AllMaps())
            {
                try
                {
                    TileMatrix tiles = map.Tiles;
                    if (tiles.IndexPath == null || tiles.StaticsPath == null || !File.Exists(tiles.StaticsPath) ||
                        !seen.Add(Path.GetFullPath(tiles.StaticsPath)))
                    {
                        continue;
                    }

                    string name;
                    try
                    {
                        name = MapRegistry.GetName(map.MapId);
                    }
                    catch (Exception)
                    {
                        name = $"Map {map.MapId}";
                    }

                    maps.Add(new MapFiles { Name = name, Index = tiles.IndexPath, Statics = tiles.StaticsPath, BlockHeight = Math.Max(1, map.Height >> 3) });
                }
                catch (Exception)
                {
                    // map without files
                }
            }

            bool[] valid = _validArt;
            int hueCount = Hues.List?.Length ?? 3000;
            var invalidIds = new Dictionary<int, Aggregate>();
            var invalidHues = new Dictionary<int, Aggregate>();
            var errors = new List<(string Map, string Error)>();

            for (int m = 0; m < maps.Count; m++)
            {
                MapFiles map = maps[m];
                Report($"{_stepName}: {map.Name}", _stepBase + (_stepSpan * m / Math.Max(1, maps.Count)));
                try
                {
                    await Task.Run(() => ScanMap(map, valid, hueCount, invalidIds, invalidHues, _token), _token);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    errors.Add((map.Name, ex.Message));
                }
            }

            foreach (KeyValuePair<int, Aggregate> pair in invalidIds.OrderByDescending(p => p.Value.Count))
            {
                Add(CheckSeverity.Error, CheckCategory.Statics, pair.Key, Hex(pair.Key), "statics.invalidid", args: new object[] { pair.Key, pair.Value.Count, pair.Value.Sample });
            }

            foreach (KeyValuePair<int, Aggregate> pair in invalidHues.OrderByDescending(p => p.Value.Count))
            {
                Add(CheckSeverity.Warning, CheckCategory.Statics, pair.Value.Item, $"Hue {pair.Key}", "statics.invalidhue", PreviewKind.Item, pair.Value.Item,
                    args: new object[] { pair.Value.Count, pair.Key, hueCount, pair.Value.Item, pair.Value.Sample });
            }

            foreach ((string mapName, string error) in errors)
            {
                Add(CheckSeverity.Error, CheckCategory.Statics, 0, mapName, "statics.readerror", args: new object[] { mapName, error });
            }
        }

        private static void ScanMap(MapFiles map, bool[] valid, int hueCount, Dictionary<int, Aggregate> invalidIds,
            Dictionary<int, Aggregate> invalidHues, CancellationToken token)
        {
            byte[] index = File.ReadAllBytes(map.Index);
            using (var fs = new FileStream(map.Statics, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16))
            {
                long length = fs.Length;
                int blocks = index.Length / 12;
                var buffer = new byte[4096];
                for (int block = 0; block < blocks; block++)
                {
                    if ((block & 0x3FFF) == 0)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    int lookup = BitConverter.ToInt32(index, block * 12);
                    int size = BitConverter.ToInt32(index, (block * 12) + 4);
                    if (lookup < 0 || size <= 0 || lookup + (long)size > length)
                    {
                        continue;
                    }

                    if (buffer.Length < size)
                    {
                        buffer = new byte[size];
                    }

                    fs.Seek(lookup, SeekOrigin.Begin);
                    int read = fs.Read(buffer, 0, size);
                    int bx = block / map.BlockHeight, by = block % map.BlockHeight;

                    for (int p = 0; p + 7 <= read; p += 7)
                    {
                        ushort id = BitConverter.ToUInt16(buffer, p);
                        int hue = BitConverter.ToUInt16(buffer, p + 5) & 0x3FFF;
                        bool badId = id >= valid.Length || !valid[id];
                        bool badHue = hue > hueCount;
                        if (!badId && !badHue)
                        {
                            continue;
                        }

                        string sample = $"{map.Name} {(bx << 3) + buffer[p + 2]},{(by << 3) + buffer[p + 3]},{(sbyte)buffer[p + 4]}";
                        if (badId)
                        {
                            Count(invalidIds, id, id, sample);
                        }

                        if (badHue)
                        {
                            Count(invalidHues, hue, id, sample);
                        }
                    }
                }
            }
        }

        private static void Count(Dictionary<int, Aggregate> dict, int key, int item, string sample)
        {
            if (!dict.TryGetValue(key, out Aggregate a))
            {
                a = new Aggregate { Item = item, Sample = sample };
                dict[key] = a;
            }

            a.Count++;
        }

        #endregion

        #region Cliloc

        /// <summary>Cliloc number of an item name (as ServUO: Item.LabelNumber)</summary>
        public static int ItemClilocNumber(int id) => id < 0x4000 ? 1020000 + id : 1078872 + id;

        private async Task CheckCliloc()
        {
            HashSet<int> numbers;
            try
            {
                var cliloc = new StringList("enu", Options.NewClilocFormat);
                numbers = new HashSet<int>(cliloc.Entries.Select(e => e.Number));
            }
            catch (Exception ex)
            {
                Add(CheckSeverity.Warning, CheckCategory.Cliloc, 0, "Cliloc", "cliloc.notloaded", args: new object[] { ex.Message });
                return;
            }

            if (numbers.Count == 0)
            {
                Add(CheckSeverity.Warning, CheckCategory.Cliloc, 0, "Cliloc", "cliloc.notloaded", args: new object[] { "0 entries" });
                return;
            }

            for (int id = 1; id <= _maxItem; id++)
            {
                if (id % Portion == 0)
                {
                    await Breathe(id, _maxItem);
                }

                if (!ValidArt(id))
                {
                    continue;
                }

                string name = ItemName(id);
                if (IsNoDraw(name))
                {
                    continue;
                }

                int number = ItemClilocNumber(id);
                if (!numbers.Contains(number))
                {
                    Add(CheckSeverity.Info, CheckCategory.Cliloc, id, Hex(id), "cliloc.item", PreviewKind.Item, id, args: new object[] { id, name, number });
                }
            }
        }

        #endregion
    }
}
