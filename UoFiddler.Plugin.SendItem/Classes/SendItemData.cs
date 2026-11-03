/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.SendItem.Classes
{
    public enum ButtonKind
    {
        Command,
        Static
    }

    /// <summary>
    /// One button on a category tab: either a static to place or one or more saved commands.
    /// </summary>
    public class CommandButton
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Text { get; set; } = "";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ButtonKind Kind { get; set; } = ButtonKind.Command;

        /// <summary>
        /// Command lines, one per line. "#wait 500" pauses 500 ms.
        /// </summary>
        public string Commands { get; set; } = "";

        public int StaticId { get; set; }
        public int Hue { get; set; }

        /// <summary>
        /// Template for static buttons. Empty = use the global place template.
        /// </summary>
        public string Template { get; set; } = "";

        /// <summary>
        /// Button color as ARGB, 0 = default.
        /// </summary>
        public int Color { get; set; }

        public string Tooltip { get; set; } = "";

        public CommandButton Clone()
        {
            var copy = (CommandButton)MemberwiseClone();
            copy.Id = Guid.NewGuid().ToString("N");
            return copy;
        }
    }

    public class CommandTab
    {
        public string Name { get; set; } = "New tab";
        public List<CommandButton> Buttons { get; set; } = new();
    }

    public class SendItemData
    {
        public int Version { get; set; } = 1;
        public string PlaceTemplate { get; set; } = "[add static {hex}{sethue}";
        public int SendDelayMs { get; set; } = 250;
        public bool FocusClientAfterSend { get; set; } = true;
        public int TileSize { get; set; } = 64;
        public int SplitterDistance { get; set; } = 560;
        public List<CommandTab> Tabs { get; set; } = new();
        public List<string> History { get; set; } = new();

        public static SendItemData CreateDefault()
        {
            var data = new SendItemData();
            data.Tabs.Add(new CommandTab { Name = "Statics" });
            data.Tabs.Add(new CommandTab
            {
                Name = "Commands",
                Buttons =
                {
                    Cmd("Props", "[props"),
                    Cmd("Remove", "[remove"),
                    Cmd("Area remove", "[m remove"),
                    Cmd("Z +1", "[inc z 1"),
                    Cmd("Z -1", "[inc z -1"),
                    Cmd("Z +5", "[inc z 5"),
                    Cmd("Z -5", "[inc z -5"),
                    Cmd("Hue 0", "[set hue 0"),
                    Cmd("Movable off", "[set movable false"),
                    Cmd("Teleport", "[tele"),
                    Cmd("Where", "[where"),
                    Cmd("Save world", "[save")
                }
            });
            return data;
        }

        private static CommandButton Cmd(string text, string command) =>
            new() { Text = text, Kind = ButtonKind.Command, Commands = command };
    }

    /// <summary>
    /// Placeholders for templates and commands.
    /// </summary>
    public static class CommandText
    {
        public static readonly string[] PlaceTemplatePresets =
        {
            "[add static {hex}{sethue}",
            "[tile static {hex}{sethue}",
            "[add static {id}{sethue}",
            ".create {hex}",
            "[add {name}"
        };

        public const string PlaceholderHelp =
            "{hex} = 0x1234   {id} = 4660   {name} = tiledata name   {hue} = hue number   {sethue} = \" set hue N\" when a hue is set";

        public static string Expand(string template, int staticId, string name, int hue)
        {
            if (string.IsNullOrEmpty(template))
            {
                return "";
            }

            return template
                .Replace("{hex}", "0x" + staticId.ToString("X4", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
                .Replace("{id}", staticId.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
                .Replace("{name}", (name ?? "").Replace(" ", ""), StringComparison.OrdinalIgnoreCase)
                .Replace("{sethue}", hue > 0 ? " set hue " + hue.ToString(CultureInfo.InvariantCulture) : "", StringComparison.OrdinalIgnoreCase)
                .Replace("{hue}", hue.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parses "0x1234", "1234h", "4660" into an id.
        /// </summary>
        public static bool TryParseId(string text, out int id)
        {
            text = (text ?? "").Trim();
            id = -1;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id) && id >= 0;
            }

            if (text.EndsWith("h", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(0, text.Length - 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id) && id >= 0;
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out id) && id >= 0;
        }
    }

    public static class SendItemStore
    {
        private static readonly JsonSerializerOptions _json = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static string FilePath =>
            Path.Combine(string.IsNullOrEmpty(Options.AppDataPath)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UoFiddler")
                    : Options.AppDataPath,
                "plugins", "SendItemTabs.json");

        public static SendItemData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    SendItemData data = JsonSerializer.Deserialize<SendItemData>(File.ReadAllText(FilePath), _json);
                    if (data != null)
                    {
                        Normalize(data);
                        return data;
                    }
                }
            }
            catch
            {
                try
                {
                    File.Copy(FilePath, FilePath + ".broken", true);
                }
                catch
                {
                    // ignored
                }
            }

            return SendItemData.CreateDefault();
        }

        private static void Normalize(SendItemData data)
        {
            data.Tabs ??= new List<CommandTab>();
            data.History ??= new List<string>();
            foreach (CommandTab tab in data.Tabs)
            {
                tab.Buttons ??= new List<CommandButton>();
            }

            if (data.Tabs.Count == 0)
            {
                data.Tabs.Add(new CommandTab { Name = "Statics" });
            }

            if (string.IsNullOrWhiteSpace(data.PlaceTemplate))
            {
                data.PlaceTemplate = CommandText.PlaceTemplatePresets[0];
            }
        }

        public static void Save(SendItemData data)
        {
            string dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(data, _json), Encoding.UTF8);
            File.Move(tmp, FilePath, true);
        }

        /// <summary>
        /// Exports tabs so they can be shared with other builders.
        /// </summary>
        public static void ExportTabs(string path, IEnumerable<CommandTab> tabs)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(tabs.ToList(), _json), Encoding.UTF8);
        }

        public static List<CommandTab> ImportTabs(string path)
        {
            string json = File.ReadAllText(path);
            List<CommandTab> tabs = json.TrimStart().StartsWith("{")
                ? JsonSerializer.Deserialize<SendItemData>(json, _json)?.Tabs
                : JsonSerializer.Deserialize<List<CommandTab>>(json, _json);

            tabs ??= new List<CommandTab>();
            foreach (CommandTab tab in tabs)
            {
                tab.Buttons ??= new List<CommandButton>();
                foreach (CommandButton b in tab.Buttons)
                {
                    b.Id = Guid.NewGuid().ToString("N");
                }
            }

            return tabs;
        }
    }
}
