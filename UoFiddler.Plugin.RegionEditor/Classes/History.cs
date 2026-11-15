/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Undo / redo of the region editor. Every change stores a snapshot (JSON)
 * of the whole project before the change – simple and safe for all kinds
 * of operations (import, auto fix, drag & drop …).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace UoFiddler.Plugin.RegionEditor.Classes
{
    public sealed class HistoryEntry
    {
        public string Description { get; set; }
        public DateTime Time { get; set; }
        internal string Snapshot { get; set; }

        public override string ToString() => $"{Time:HH:mm:ss}  {Description}";
    }

    public sealed class History
    {
        private readonly List<HistoryEntry> _undo = new List<HistoryEntry>();
        private readonly List<HistoryEntry> _redo = new List<HistoryEntry>();

        public int Limit { get; set; } = 200;

        /// <summary>raised after every change of the lists</summary>
        public event EventHandler Changed;

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        public IReadOnlyList<HistoryEntry> UndoEntries => _undo;
        public IReadOnlyList<HistoryEntry> RedoEntries => _redo;

        public string UndoText => _undo.LastOrDefault()?.Description;
        public string RedoText => _redo.LastOrDefault()?.Description;

        private string _savedSnapshot;

        public static string Serialize(RegionProject project) => JsonSerializer.Serialize(project, ProjectJson.Options);

        public static RegionProject Deserialize(string json) => JsonSerializer.Deserialize<RegionProject>(json, ProjectJson.Options);

        /// <summary>Call BEFORE a change: stores the current state</summary>
        public void Record(RegionProject project, string description)
        {
            _undo.Add(new HistoryEntry { Description = description, Time = DateTime.Now, Snapshot = Serialize(project) });
            if (_undo.Count > Limit)
            {
                _undo.RemoveAt(0);
            }

            _redo.Clear();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Removes the last record (the change was cancelled)</summary>
        public void DropLast()
        {
            if (_undo.Count > 0)
            {
                _undo.RemoveAt(_undo.Count - 1);
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public RegionProject Undo(RegionProject current)
        {
            if (!CanUndo)
            {
                return current;
            }

            HistoryEntry entry = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            string now = Serialize(current);
            _redo.Add(new HistoryEntry { Description = entry.Description, Time = entry.Time, Snapshot = now });
            Changed?.Invoke(this, EventArgs.Empty);
            return Deserialize(entry.Snapshot);
        }

        public RegionProject Redo(RegionProject current)
        {
            if (!CanRedo)
            {
                return current;
            }

            HistoryEntry entry = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            _undo.Add(new HistoryEntry { Description = entry.Description, Time = entry.Time, Snapshot = Serialize(current) });
            Changed?.Invoke(this, EventArgs.Empty);
            return Deserialize(entry.Snapshot);
        }

        /// <summary>Undo several steps at once (history list: jump back to an entry)</summary>
        public RegionProject UndoTo(RegionProject current, int undoIndex)
        {
            while (_undo.Count > undoIndex && CanUndo)
            {
                current = Undo(current);
            }

            return current;
        }

        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>marks the given project state as saved</summary>
        public void MarkSaved(RegionProject project)
        {
            _savedSnapshot = Serialize(project);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>true when the project differs from the saved state</summary>
        public bool HasChanges(RegionProject project) => _savedSnapshot == null ? _undo.Count > 0 : Serialize(project) != _savedSnapshot;
    }

    public static class ProjectJson
    {
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new PointJsonConverter() }
        };
    }

    /// <summary>System.Drawing.Point as [x, y]</summary>
    public sealed class PointJsonConverter : System.Text.Json.Serialization.JsonConverter<System.Drawing.Point>
    {
        public override System.Drawing.Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int[] values = JsonSerializer.Deserialize<int[]>(ref reader);
            return values != null && values.Length >= 2 ? new System.Drawing.Point(values[0], values[1]) : System.Drawing.Point.Empty;
        }

        public override void Write(Utf8JsonWriter writer, System.Drawing.Point value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}
