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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public enum AddressPreference
    {
        Auto,
        IPv4,
        IPv6
    }

    public sealed class TcpCheckResult
    {
        public bool Open { get; init; }
        public long Milliseconds { get; init; }
        public string Error { get; init; } = "";
    }

    public sealed class LocalSubnet
    {
        public string AdapterName { get; init; } = "";
        public IPAddress LocalAddress { get; init; }
        public int PrefixLength { get; init; }
        public List<IPAddress> Hosts { get; init; } = new();

        public override string ToString() => $"{AdapterName}: {LocalAddress}/{PrefixLength} ({Hosts.Count} hosts)";
    }

    public sealed class ProviderInfo
    {
        public string Ip { get; init; } = "";
        public string Hostname { get; init; } = "";
        public string Org { get; init; } = "";
        public string City { get; init; } = "";
        public string Country { get; init; } = "";

        public string ToDisplayText()
        {
            var lines = new List<string>
            {
                "Provider : " + (Org.Length > 0 ? Org : "unknown"),
                "Public IP: " + (Ip.Length > 0 ? Ip : "-") + (Hostname.Length > 0 ? "   (" + Hostname + ")" : "")
            };
            string place = string.Join(", ", new[] { City, Country }.Where(x => x.Length > 0));
            if (place.Length > 0)
            {
                lines.Add("Location : " + place);
            }

            return string.Join(Environment.NewLine, lines);
        }
    }

    /// <summary>
    /// Async network helpers used by the AdminTool. Nothing here touches the UI.
    /// </summary>
    public static class NetworkTools
    {
        private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(6) };

        #region Target parsing

        /// <summary>
        /// Splits "host", "host:port", "1.2.3.4:port", "[v6]:port" or a plain IPv6 address.
        /// </summary>
        public static bool TryParseTarget(string input, out string host, out int? port)
        {
            host = "";
            port = null;
            input = (input ?? "").Trim();

            // Allow pasted URLs like "http://shard.example.com:2593/"
            if (input.Contains("://") && Uri.TryCreate(input, UriKind.Absolute, out Uri uri))
            {
                host = uri.Host.Trim('[', ']');
                if (!uri.IsDefaultPort)
                {
                    port = uri.Port;
                }

                return IsValidHost(host);
            }

            if (input.StartsWith("["))
            {
                int end = input.IndexOf(']');
                if (end < 0)
                {
                    return false;
                }

                host = input.Substring(1, end - 1);
                string rest = input.Substring(end + 1);
                if (rest.StartsWith(":") && int.TryParse(rest.Substring(1), out int p6))
                {
                    port = p6;
                }

                return IsValidHost(host) && (port == null || port is > 0 and <= 65535);
            }

            int colons = input.Count(c => c == ':');
            if (colons == 1)
            {
                int idx = input.IndexOf(':');
                host = input.Substring(0, idx);
                if (!int.TryParse(input.Substring(idx + 1), out int p) || p is <= 0 or > 65535)
                {
                    return false;
                }

                port = p;
            }
            else
            {
                host = input;
            }

            return IsValidHost(host);
        }

        public static bool IsValidHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            if (IPAddress.TryParse(host, out _))
            {
                return true;
            }

            return Uri.CheckHostName(host) == UriHostNameType.Dns;
        }

        public static bool IsIPv4(string text) =>
            IPAddress.TryParse((text ?? "").Trim(), out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetwork
            && (text ?? "").Trim().Count(c => c == '.') == 3;

        public static bool IsIPv6(string text) =>
            IPAddress.TryParse((text ?? "").Trim(), out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetworkV6;

        /// <summary>
        /// Parses "2593", "2593-2600", "80,443,2590-2599" into a sorted, distinct list.
        /// </summary>
        public static List<int> ParsePorts(string text, int max = 20000)
        {
            var ports = new SortedSet<int>();
            foreach (string raw in (text ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string part = raw.Trim();
                int dash = part.IndexOf('-');
                if (dash > 0)
                {
                    if (int.TryParse(part.Substring(0, dash), out int a) && int.TryParse(part.Substring(dash + 1), out int b))
                    {
                        if (a > b)
                        {
                            (a, b) = (b, a);
                        }

                        for (int p = Math.Max(1, a); p <= Math.Min(65535, b) && ports.Count < max; p++)
                        {
                            ports.Add(p);
                        }
                    }
                }
                else if (int.TryParse(part, out int single) && single is > 0 and <= 65535)
                {
                    ports.Add(single);
                }

                if (ports.Count >= max)
                {
                    break;
                }
            }

            return ports.ToList();
        }

        #endregion

        #region DNS

        public static async Task<IPAddress[]> ResolveAllAsync(string host, CancellationToken ct)
        {
            if (IPAddress.TryParse(host, out IPAddress direct))
            {
                return new[] { direct };
            }

            return await Dns.GetHostAddressesAsync(host, ct).ConfigureAwait(false);
        }

        public static async Task<IPAddress> ResolveAsync(string host, AddressPreference pref, CancellationToken ct)
        {
            IPAddress[] all = await ResolveAllAsync(host, ct).ConfigureAwait(false);
            return PickAddress(all, pref)
                   ?? throw new SocketException((int)SocketError.HostNotFound);
        }

        public static IPAddress PickAddress(IEnumerable<IPAddress> addresses, AddressPreference pref)
        {
            var list = addresses?.ToList() ?? new List<IPAddress>();
            return pref switch
            {
                AddressPreference.IPv4 => list.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork),
                AddressPreference.IPv6 => list.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6),
                // Auto: IPv4 first (most game servers listen there), otherwise anything.
                _ => list.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) ?? list.FirstOrDefault()
            };
        }

        public static async Task<string> ReverseLookupAsync(IPAddress address, int timeoutMs, CancellationToken ct)
        {
            try
            {
                Task<IPHostEntry> lookup = Dns.GetHostEntryAsync(address.ToString(), ct);
                Task done = await Task.WhenAny(lookup, Task.Delay(timeoutMs, ct)).ConfigureAwait(false);
                if (done == lookup && lookup.Status == TaskStatus.RanToCompletion)
                {
                    string name = lookup.Result.HostName;
                    return name == address.ToString() ? "" : name;
                }
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                // no PTR record
            }

            return "";
        }

        #endregion

        #region Ping / TCP

        public static async Task<PingReply> PingAsync(IPAddress address, int timeoutMs, int ttl = 128)
        {
            using var ping = new Ping();
            var options = new PingOptions(ttl, true);
            return await ping.SendPingAsync(address, timeoutMs, new byte[32], options).ConfigureAwait(false);
        }

        public static async Task<TcpCheckResult> TcpConnectAsync(IPAddress address, int port, int timeoutMs, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            using var client = new TcpClient(address.AddressFamily);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeoutMs);
            try
            {
                await client.ConnectAsync(address, port, cts.Token).ConfigureAwait(false);
                return new TcpCheckResult { Open = true, Milliseconds = sw.ElapsedMilliseconds };
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                return new TcpCheckResult { Open = false, Milliseconds = sw.ElapsedMilliseconds, Error = "Timeout" };
            }
            catch (SocketException ex)
            {
                return new TcpCheckResult { Open = false, Milliseconds = sw.ElapsedMilliseconds, Error = ex.SocketErrorCode.ToString() };
            }
        }

        /// <summary>
        /// Experimental: asks a RunUO / ServUO based shard for its status line
        /// (seed + 0xF1 0x00 0x04 0xFF). Returns an empty string if the shard does not answer.
        /// </summary>
        public static async Task<string> QueryUoStatusAsync(IPAddress address, int port, int timeoutMs, CancellationToken ct)
        {
            using var client = new TcpClient(address.AddressFamily);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(Math.Max(timeoutMs, 1500));

            await client.ConnectAsync(address, port, cts.Token).ConfigureAwait(false);
            NetworkStream stream = client.GetStream();

            byte[] request = { 0x7F, 0x00, 0x00, 0x01, 0xF1, 0x00, 0x04, 0xFF };
            await stream.WriteAsync(request, cts.Token).ConfigureAwait(false);

            var buffer = new byte[2048];
            var sb = new StringBuilder();
            try
            {
                while (sb.Length < 4096)
                {
                    int read = await stream.ReadAsync(buffer, cts.Token).ConfigureAwait(false);
                    if (read <= 0)
                    {
                        break;
                    }

                    for (int i = 0; i < read; i++)
                    {
                        char c = (char)buffer[i];
                        if (c >= 32 && c < 127)
                        {
                            sb.Append(c);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                // timeout: return what we have
            }
            catch (System.IO.IOException)
            {
                // server closed the connection
            }

            return sb.ToString().Trim();
        }

        #endregion

        #region Local network / internet

        public static List<LocalSubnet> GetLocalSubnets(int maxPrefixHosts = 254)
        {
            var result = new List<LocalSubnet>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    continue;
                }

                foreach (UnicastIPAddressInformation ua in nic.GetIPProperties().UnicastAddresses)
                {
                    if (ua.Address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(ua.Address))
                    {
                        continue;
                    }

                    byte[] ip = ua.Address.GetAddressBytes();
                    // 169.254.x.x (no DHCP) is not worth scanning
                    if (ip[0] == 169 && ip[1] == 254)
                    {
                        continue;
                    }

                    int prefix = ua.PrefixLength is > 0 and <= 32 ? ua.PrefixLength : 24;
                    // Never scan more than a /24 – larger networks would take ages.
                    int scanPrefix = Math.Max(prefix, 24);
                    uint self = ToUInt(ip);
                    uint mask = scanPrefix == 32 ? 0xFFFFFFFF : ~(0xFFFFFFFFu >> scanPrefix);
                    uint network = self & mask;
                    uint broadcast = network | ~mask;

                    var hosts = new List<IPAddress>();
                    for (uint a = network + 1; a < broadcast && hosts.Count < maxPrefixHosts; a++)
                    {
                        if (a != self)
                        {
                            hosts.Add(FromUInt(a));
                        }
                    }

                    result.Add(new LocalSubnet
                    {
                        AdapterName = nic.Name,
                        LocalAddress = ua.Address,
                        PrefixLength = scanPrefix,
                        Hosts = hosts
                    });
                }
            }

            return result;
        }

        private static uint ToUInt(byte[] b) => ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];

        private static IPAddress FromUInt(uint v) =>
            new(new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v });

        public static string DescribeAdapters()
        {
            var sb = new StringBuilder();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()
                         .OrderBy(n => n.OperationalStatus != OperationalStatus.Up)
                         .ThenBy(n => n.Name))
            {
                IPInterfaceProperties props = nic.GetIPProperties();
                sb.AppendLine($"■ {nic.Name}  ({nic.Description})");
                sb.AppendLine($"    Status   : {nic.OperationalStatus}   Type: {nic.NetworkInterfaceType}" +
                              (nic.Speed > 0 ? $"   Speed: {FormatSpeed(nic.Speed)}" : ""));

                string mac = nic.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(mac))
                {
                    sb.AppendLine("    MAC      : " + string.Join("-", Enumerable.Range(0, mac.Length / 2).Select(i => mac.Substring(i * 2, 2))));
                }

                foreach (UnicastIPAddressInformation ua in props.UnicastAddresses)
                {
                    string kind = ua.Address.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6";
                    sb.AppendLine($"    {kind,-9}: {ua.Address}/{ua.PrefixLength}");
                }

                foreach (GatewayIPAddressInformation gw in props.GatewayAddresses)
                {
                    sb.AppendLine($"    Gateway  : {gw.Address}");
                }

                foreach (IPAddress dns in props.DnsAddresses)
                {
                    sb.AppendLine($"    DNS      : {dns}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string FormatSpeed(long bitsPerSecond)
        {
            if (bitsPerSecond >= 1_000_000_000)
            {
                return (bitsPerSecond / 1_000_000_000d).ToString("0.#", CultureInfo.InvariantCulture) + " Gbit/s";
            }

            return (bitsPerSecond / 1_000_000d).ToString("0.#", CultureInfo.InvariantCulture) + " Mbit/s";
        }

        /// <summary>
        /// Default gateways (your router) of all active adapters.
        /// </summary>
        public static List<(string Adapter, IPAddress Address)> GetGateways()
        {
            var result = new List<(string, IPAddress)>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                foreach (GatewayIPAddressInformation gw in nic.GetIPProperties().GatewayAddresses)
                {
                    IPAddress a = gw.Address;
                    if (a == null || a.Equals(IPAddress.Any) || a.Equals(IPAddress.IPv6Any) || a.ToString() == "0.0.0.0")
                    {
                        continue;
                    }

                    if (!result.Any(r => r.Item2.Equals(a)))
                    {
                        result.Add((nic.Name, a));
                    }
                }
            }

            // IPv4 router first, it answers ping more reliably
            return result.OrderBy(r => r.Item2.AddressFamily == AddressFamily.InterNetwork ? 0 : 1).ToList();
        }

        /// <summary>
        /// DNS servers of all active adapters (without the Windows placeholder fec0::).
        /// </summary>
        public static List<IPAddress> GetDnsServers()
        {
            var result = new List<IPAddress>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                foreach (IPAddress dns in nic.GetIPProperties().DnsAddresses)
                {
                    if (!dns.IsIPv6SiteLocal && !IPAddress.IsLoopback(dns) && !result.Contains(dns))
                    {
                        result.Add(dns);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// True for addresses of the own home network (RFC 1918, link-local, loopback, IPv6 ULA).
        /// Carrier-grade NAT (100.64.0.0/10) counts as provider, not as private.
        /// </summary>
        public static bool IsPrivate(IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip))
            {
                return true;
            }

            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                byte[] b = ip.GetAddressBytes();
                return b[0] == 10 || (b[0] == 172 && b[1] >= 16 && b[1] <= 31) || (b[0] == 192 && b[1] == 168) ||
                       (b[0] == 169 && b[1] == 254);
            }

            byte[] v6 = ip.GetAddressBytes();
            return ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || (v6[0] & 0xFE) == 0xFC;
        }

        /// <summary>
        /// Finds the first address on the way to the internet that is outside the home network:
        /// that is the access node of your provider.
        /// </summary>
        public static async Task<IPAddress> FindProviderNodeAsync(int timeoutMs, CancellationToken ct)
        {
            IPAddress probe = IPAddress.Parse("1.1.1.1");
            for (int ttl = 1; ttl <= 10; ttl++)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    PingReply reply = await PingAsync(probe, timeoutMs, ttl).ConfigureAwait(false);
                    if ((reply.Status is IPStatus.TtlExpired or IPStatus.TimeExceeded) && reply.Address != null &&
                        !IsPrivate(reply.Address))
                    {
                        return reply.Address;
                    }

                    if (reply.Status == IPStatus.Success)
                    {
                        break;
                    }
                }
                catch (PingException)
                {
                    // try the next hop
                }
            }

            return null;
        }

        /// <summary>
        /// Provider name, public IP and location (ipinfo.io). Falls back to the public IP and its host name.
        /// </summary>
        public static async Task<ProviderInfo> GetProviderInfoAsync(CancellationToken ct)
        {
            try
            {
                string json = await _http.GetStringAsync("https://ipinfo.io/json", ct).ConfigureAwait(false);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                System.Text.Json.JsonElement root = doc.RootElement;

                string Get(string name) =>
                    root.TryGetProperty(name, out System.Text.Json.JsonElement v) && v.ValueKind == System.Text.Json.JsonValueKind.String
                        ? v.GetString() ?? ""
                        : "";

                string org = Get("org");
                // "AS3320 Deutsche Telekom AG" -> "Deutsche Telekom AG (AS3320)"
                if (org.StartsWith("AS", StringComparison.Ordinal) && org.Contains(' '))
                {
                    int sp = org.IndexOf(' ');
                    org = org.Substring(sp + 1) + " (" + org.Substring(0, sp) + ")";
                }

                return new ProviderInfo
                {
                    Ip = Get("ip"),
                    Hostname = Get("hostname"),
                    Org = org,
                    City = Get("city"),
                    Country = Get("country")
                };
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                string ip = await GetPublicIpAsync(false).ConfigureAwait(false);
                if (ip.Length == 0)
                {
                    return null;
                }

                string host = await ReverseLookupAsync(IPAddress.Parse(ip), 2000, ct).ConfigureAwait(false);
                return new ProviderInfo { Ip = ip, Hostname = host, Org = ProviderFromHostname(host) };
            }
        }

        /// <summary>
        /// "p5dd1234.dip0.t-ipconnect.de" -> "t-ipconnect.de"
        /// </summary>
        private static string ProviderFromHostname(string host)
        {
            string[] parts = (host ?? "").Split('.', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? parts[^2] + "." + parts[^1] : "";
        }

        public static async Task<bool> CheckInternetAsync()
        {
            try
            {
                using HttpResponseMessage response = await _http.GetAsync("http://www.google.com/generate_204").ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the public IPv4 or IPv6 address, or an empty string if not available.
        /// </summary>
        public static async Task<string> GetPublicIpAsync(bool ipv6)
        {
            try
            {
                string url = ipv6 ? "https://api6.ipify.org" : "https://api.ipify.org";
                string text = (await _http.GetStringAsync(url).ConfigureAwait(false)).Trim();
                return IPAddress.TryParse(text, out _) ? text : "";
            }
            catch
            {
                return "";
            }
        }

        #endregion
    }
}
