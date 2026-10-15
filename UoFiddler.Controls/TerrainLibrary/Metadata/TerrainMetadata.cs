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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using UoFiddler.Controls.ArtLibrary.Metadata;

namespace UoFiddler.Controls.TerrainLibrary.Metadata
{
    /// <summary>
    /// Everything of a terrain library that is not a graphic, stored as Name.terrain.json:
    /// <list type="bullet">
    /// <item><see cref="Catalog"/>: names, descriptions, categories, tags (same classes as the art library)</item>
    /// <item><see cref="Slots"/>: tiledata values, radar color, source ids, favorite per slot</item>
    /// <item><see cref="Presets"/>: own templates</item>
    /// </list>
    /// </summary>
    public sealed class TerrainMetadata
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private string _path;
        private bool _dirty;

        public int Version { get; set; } = 1;

        public ArtLibraryMetadata Catalog { get; set; } = new ArtLibraryMetadata();

        public Dictionary<int, TerrainSlotData> Slots { get; set; } = new Dictionary<int, TerrainSlotData>();

        public List<TerrainPreset> Presets { get; set; } = new List<TerrainPreset>();

        [JsonIgnore]
        public bool IsDirty => _dirty || Catalog.IsDirty;

        #region [ Load / Save ]

        public static TerrainMetadata Load(string path)
        {
            TerrainMetadata metadata = null;
            if (File.Exists(path))
            {
                metadata = JsonSerializer.Deserialize<TerrainMetadata>(File.ReadAllText(path), _jsonOptions);
            }

            metadata ??= new TerrainMetadata();
            metadata._path = path;
            metadata.Catalog ??= new ArtLibraryMetadata();
            metadata.Catalog.Categories ??= new List<ArtLibraryCategory>();
            metadata.Catalog.Tags ??= new List<string>();
            metadata.Catalog.Items ??= new Dictionary<int, ArtLibraryItemInfo>();
            foreach (ArtLibraryItemInfo info in metadata.Catalog.Items.Values)
            {
                info.Tags ??= new List<string>();
            }

            metadata.Slots ??= new Dictionary<int, TerrainSlotData>();
            metadata.Presets ??= new List<TerrainPreset>();
            metadata.Catalog.AcceptChanges();
            metadata._dirty = false;
            return metadata;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(_path))
            {
                throw new InvalidOperationException("Metadata has no file path.");
            }

            foreach (int id in Slots.Where(p => p.Value == null || p.Value.IsEmpty).Select(p => p.Key).ToList())
            {
                Slots.Remove(id);
            }

            foreach (int id in Catalog.Items.Where(p => p.Value == null || p.Value.IsEmpty).Select(p => p.Key).ToList())
            {
                Catalog.Items.Remove(id);
            }

            string temp = _path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(this, _jsonOptions));
            File.Move(temp, _path, true);
            _dirty = false;
            Catalog.AcceptChanges();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        #endregion

        #region [ Slots ]

        public TerrainSlotData GetSlot(int id) => Slots.TryGetValue(id, out TerrainSlotData data) ? data : null;

        public TerrainSlotData GetOrCreateSlot(int id)
        {
            if (!Slots.TryGetValue(id, out TerrainSlotData data) || data == null)
            {
                data = new TerrainSlotData();
                Slots[id] = data;
                _dirty = true;
            }

            return data;
        }

        /// <summary>Removes settings, names, categories and tags of a slot.</summary>
        public void RemoveSlot(int id)
        {
            if (Slots.Remove(id))
            {
                _dirty = true;
            }

            Catalog.Remove(id);
        }

        public void CopySlot(int sourceId, int targetId)
        {
            TerrainSlotData data = GetSlot(sourceId);
            if (data == null)
            {
                Slots.Remove(targetId);
            }
            else
            {
                Slots[targetId] = data.Clone();
            }

            Catalog.Copy(sourceId, targetId);
            _dirty = true;
        }

        public void MoveSlot(int sourceId, int targetId)
        {
            if (sourceId == targetId)
            {
                return;
            }

            CopySlot(sourceId, targetId);
            RemoveSlot(sourceId);
        }

        public void SwapSlots(int firstId, int secondId)
        {
            TerrainSlotData first = GetSlot(firstId);
            TerrainSlotData second = GetSlot(secondId);
            Slots.Remove(firstId);
            Slots.Remove(secondId);
            if (second != null)
            {
                Slots[firstId] = second;
            }

            if (first != null)
            {
                Slots[secondId] = first;
            }

            Catalog.Swap(firstId, secondId);
            _dirty = true;
        }

        #endregion

        #region [ Presets ]

        /// <summary>Own templates plus (optionally) the built-in ones, sorted by name.</summary>
        public List<TerrainPreset> GetAllPresets(bool includeBuiltIn)
        {
            var result = new List<TerrainPreset>();
            if (includeBuiltIn)
            {
                result.AddRange(TerrainDefaults.Create());
            }

            result.AddRange(Presets.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase));
            return result;
        }

        /// <summary>
        /// Applies a template to a slot. Returns false (and changes nothing) when the slot already has settings
        /// and <paramref name="overwrite"/> is false.
        /// </summary>
        public bool ApplyPreset(int id, TerrainPreset preset, bool overwrite)
        {
            TerrainSlotData data = GetOrCreateSlot(id);
            if (data.HasSettings && !overwrite)
            {
                return false;
            }

            data.TileName = preset.TileName ?? data.TileName;
            data.TileFlags = preset.TileFlags ?? data.TileFlags;
            data.TextureId = preset.TextureId ?? data.TextureId;
            data.RadarColor = preset.RadarColor ?? data.RadarColor;
            data.PresetName = preset.Name;

            if (preset.CategoryId != null && Catalog.GetCategory(preset.CategoryId) != null)
            {
                Catalog.GetOrCreate(id).CategoryId = preset.CategoryId;
                Catalog.MarkDirty();
            }

            if (!string.IsNullOrWhiteSpace(preset.Description))
            {
                ArtLibraryItemInfo info = Catalog.GetOrCreate(id);
                if (string.IsNullOrWhiteSpace(info.Description))
                {
                    info.Description = preset.Description;
                    Catalog.MarkDirty();
                }
            }

            _dirty = true;
            return true;
        }

        /// <summary>Creates an own template from the settings of a slot.</summary>
        public TerrainPreset CreatePresetFromSlot(int id, string name)
        {
            TerrainSlotData data = GetSlot(id) ?? new TerrainSlotData();
            var preset = new TerrainPreset
            {
                Name = name,
                TileName = data.TileName,
                TileFlags = data.TileFlags,
                TextureId = data.TextureId,
                RadarColor = data.RadarColor,
                CategoryId = Catalog.Get(id)?.CategoryId,
                Description = Catalog.Get(id)?.Description
            };

            Presets.RemoveAll(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase));
            Presets.Add(preset);
            _dirty = true;
            return preset;
        }

        #endregion

        #region [ Search ]

        /// <summary>Catalog search plus tiledata name and color (hex) of the slot.</summary>
        public bool Matches(int id, string text, int? parsedId)
        {
            if (string.IsNullOrWhiteSpace(text) || Catalog.Matches(id, text, parsedId))
            {
                return true;
            }

            TerrainSlotData data = GetSlot(id);
            if (data == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(data.TileName) && data.TileName.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(data.PresetName) && data.PresetName.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return true;
            }

            return data.RadarColor != null && $"0x{data.RadarColor.Value:X4}".IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion
    }
}
