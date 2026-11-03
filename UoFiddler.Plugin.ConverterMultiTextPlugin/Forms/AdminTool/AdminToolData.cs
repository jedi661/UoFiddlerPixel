/***************************************************************************
 *
 * $Author: Nikodemus
 * Coder: Nikodemus
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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// One saved server in the AdminTool address book.
    /// </summary>
    public class ServerEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "";
        public string Host { get; set; } = "";
        public string IPv4 { get; set; } = "";
        public string IPv6 { get; set; } = "";
        public int Port { get; set; } = 2593;
        public string Group { get; set; } = "";
        public string Notes { get; set; } = "";
        public bool Favorite { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? LastCheck { get; set; }
        public string LastStatus { get; set; } = "";
        public long? LastLatency { get; set; }

        /// <summary>
        /// The address used for network operations: host name first, then IPv4, then IPv6.
        /// </summary>
        [JsonIgnore]
        public string PreferredAddress =>
            !string.IsNullOrWhiteSpace(Host) ? Host.Trim()
            : !string.IsNullOrWhiteSpace(IPv4) ? IPv4.Trim()
            : (IPv6 ?? "").Trim();

        /// <summary>
        /// All search terms must be found in at least one field (case-insensitive).
        /// </summary>
        public bool Matches(IReadOnlyList<string> terms)
        {
            if (terms == null || terms.Count == 0)
            {
                return true;
            }

            string haystack = string.Join("\n", Name, Host, IPv4, IPv6,
                Port.ToString(CultureInfo.InvariantCulture), Group, Notes, LastStatus);

            foreach (string term in terms)
            {
                if (haystack.IndexOf(term, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public ServerEntry Clone()
        {
            return (ServerEntry)MemberwiseClone();
        }
    }

    /// <summary>
    /// A reference address of the own connection (router, provider node, DNS, internet).
    /// Pinging these shows whether a problem is on the own side or at the server.
    /// </summary>
    public class ReferenceHost
    {
        public static readonly string[] Kinds = { "Router", "Provider", "DNS", "Internet", "Other" };

        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string Kind { get; set; } = "Provider";
        public string Notes { get; set; } = "";
        public DateTime? LastCheck { get; set; }
        public string LastStatus { get; set; } = "";
        public long? LastLatency { get; set; }

        public ReferenceHost Clone()
        {
            return (ReferenceHost)MemberwiseClone();
        }
    }

    public class AdminToolSettings
    {
        public int TimeoutMs { get; set; } = 1000;
        public int PingCount { get; set; } = 4;
        public int DefaultPort { get; set; } = 2593;
        public string PortList { get; set; } = "2593-2600,80,443";
        public string AddressFamily { get; set; } = "Auto";
        public bool TopMost { get; set; }
        public bool CheckAllOnOpen { get; set; }
        public int SplitterDistance { get; set; } = 470;
        public int Parallelism { get; set; } = 32;
    }

    public class AdminToolData
    {
        public List<ServerEntry> Servers { get; set; } = new();
        public List<string> History { get; set; } = new();
        public List<ReferenceHost> Providers { get; set; } = new();
        public string ProviderInfo { get; set; } = "";
        public AdminToolSettings Settings { get; set; } = new();
    }

    /// <summary>
    /// Loads and saves the AdminTool data as JSON in the UoFiddler app data folder.
    /// </summary>
    public static class AdminToolStore
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static string FilePath
        {
            get
            {
                string root = string.IsNullOrEmpty(Options.AppDataPath)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UoFiddler")
                    : Options.AppDataPath;
                return Path.Combine(root, "AdminTool", "AdminToolServers.json");
            }
        }

        public static AdminToolData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var data = JsonSerializer.Deserialize<AdminToolData>(File.ReadAllText(FilePath), _jsonOptions);
                    if (data != null)
                    {
                        data.Servers ??= new List<ServerEntry>();
                        data.History ??= new List<string>();
                        data.Providers ??= new List<ReferenceHost>();
                        data.ProviderInfo ??= "";
                        data.Settings ??= new AdminToolSettings();
                        return data;
                    }
                }
            }
            catch
            {
                // A broken file must not prevent the tool from opening; keep a copy for the user.
                try
                {
                    File.Copy(FilePath, FilePath + ".broken", true);
                }
                catch
                {
                    // ignored
                }
            }

            return new AdminToolData();
        }

        public static void Save(AdminToolData data)
        {
            string dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Write to a temp file first so a crash never leaves a half-written file.
            string tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8);
            File.Move(tmp, FilePath, true);
        }

        #region Import / Export

        public static void ExportJson(string path, IEnumerable<ServerEntry> servers)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(servers.ToList(), _jsonOptions), Encoding.UTF8);
        }

        public static List<ServerEntry> ImportJson(string path)
        {
            string json = File.ReadAllText(path);

            // Accept both a plain list and a full AdminToolData file.
            if (json.TrimStart().StartsWith("{"))
            {
                var data = JsonSerializer.Deserialize<AdminToolData>(json, _jsonOptions);
                return data?.Servers ?? new List<ServerEntry>();
            }

            return JsonSerializer.Deserialize<List<ServerEntry>>(json, _jsonOptions) ?? new List<ServerEntry>();
        }

        private static readonly string[] _csvHeader = { "Name", "Host", "IPv4", "IPv6", "Port", "Group", "Notes", "Favorite" };

        public static void ExportCsv(string path, IEnumerable<ServerEntry> servers)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(";", _csvHeader));
            foreach (var s in servers)
            {
                sb.AppendLine(string.Join(";",
                    Csv(s.Name), Csv(s.Host), Csv(s.IPv4), Csv(s.IPv6),
                    s.Port.ToString(CultureInfo.InvariantCulture), Csv(s.Group), Csv(s.Notes),
                    s.Favorite ? "1" : "0"));
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        public static List<ServerEntry> ImportCsv(string path)
        {
            var result = new List<ServerEntry>();
            List<string> lines = SplitCsvRecords(File.ReadAllText(path));
            if (lines.Count == 0)
            {
                return result;
            }

            char sep = lines[0].Contains(';') ? ';' : ',';
            List<string> header = SplitCsvLine(lines[0], sep).Select(h => h.Trim().ToLowerInvariant()).ToList();
            int Col(string name) => header.IndexOf(name.ToLowerInvariant());

            for (int i = 1; i < lines.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                List<string> f = SplitCsvLine(lines[i], sep);
                string Get(string name)
                {
                    int c = Col(name);
                    return c >= 0 && c < f.Count ? f[c].Trim() : "";
                }

                var entry = new ServerEntry
                {
                    Name = Get("Name"),
                    Host = Get("Host"),
                    IPv4 = Get("IPv4"),
                    IPv6 = Get("IPv6"),
                    Group = Get("Group"),
                    Notes = Get("Notes"),
                    Favorite = Get("Favorite") is "1" or "true" or "True"
                };

                if (int.TryParse(Get("Port"), out int port) && port is > 0 and <= 65535)
                {
                    entry.Port = port;
                }

                if (string.IsNullOrEmpty(entry.Name))
                {
                    entry.Name = entry.PreferredAddress;
                }

                if (!string.IsNullOrEmpty(entry.PreferredAddress))
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        private static string Csv(string value)
        {
            value ??= "";
            if (value.IndexOfAny(new[] { ';', ',', '"', '\n', '\r' }) >= 0)
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        /// <summary>
        /// Splits CSV text into records; line breaks inside quoted fields stay part of the field.
        /// </summary>
        private static List<string> SplitCsvRecords(string text)
        {
            var records = new List<string>();
            var sb = new StringBuilder();
            bool quoted = false;

            foreach (char c in text)
            {
                if (c == '"')
                {
                    quoted = !quoted;
                }

                if (!quoted && (c == '\n' || c == '\r'))
                {
                    if (sb.Length > 0)
                    {
                        records.Add(sb.ToString());
                        sb.Clear();
                    }

                    continue;
                }

                sb.Append(c);
            }

            if (sb.Length > 0)
            {
                records.Add(sb.ToString());
            }

            return records;
        }

        private static List<string> SplitCsvLine(string line, char sep)
        {
            var fields = new List<string>();
            var sb = new StringBuilder();
            bool quoted = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (quoted)
                {
                    if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else if (c == '"')
                    {
                        quoted = false;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else if (c == '"')
                {
                    quoted = true;
                }
                else if (c == sep)
                {
                    fields.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            fields.Add(sb.ToString());
            return fields;
        }

        #endregion
    }
}
