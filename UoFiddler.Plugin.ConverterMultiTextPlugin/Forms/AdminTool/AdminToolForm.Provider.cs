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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Provider / connection part of the AdminTool: saved addresses of the own router, provider,
    /// DNS and internet hosts, and a diagnosis that shows whether a problem is local, at the
    /// provider or at the server.
    /// </summary>
    public partial class AdminToolForm
    {
        private readonly Dictionary<string, ListViewItem> _providerItems = new();

        #region List

        private void InitProviderTab()
        {
            RefreshProviderList();
            lblProviderInfo.Text = string.IsNullOrEmpty(_data.ProviderInfo)
                ? "Provider: not detected yet – click \"Auto-detect\"."
                : _data.ProviderInfo;
            SetDiagnosis("Click \"Diagnose\": checks router → provider → internet → DNS → server and shows where a problem is.", Color.White);
        }

        private ReferenceHost SelectedProvider =>
            listProviders.SelectedItems.Count > 0 ? listProviders.SelectedItems[0].Tag as ReferenceHost : null;

        private void RefreshProviderList()
        {
            string selectedId = SelectedProvider?.Id;
            listProviders.BeginUpdate();
            listProviders.Items.Clear();
            _providerItems.Clear();

            foreach (ReferenceHost h in _data.Providers
                         .OrderBy(h => Array.IndexOf(ReferenceHost.Kinds, h.Kind) is int i && i >= 0 ? i : 99)
                         .ThenBy(h => h.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                var item = new ListViewItem(new[] { h.Name, h.Address, h.Kind, "", "", "" })
                {
                    Tag = h,
                    UseItemStyleForSubItems = false,
                    ToolTipText = string.IsNullOrWhiteSpace(h.Notes) ? h.Address : h.Notes
                };
                listProviders.Items.Add(item);
                _providerItems[h.Id] = item;
                UpdateProviderItem(h);
                item.Selected = h.Id == selectedId;
            }

            listProviders.EndUpdate();
        }

        private void UpdateProviderItem(ReferenceHost h)
        {
            if (!_providerItems.TryGetValue(h.Id, out ListViewItem item) || item.ListView == null)
            {
                return;
            }

            item.SubItems[3].Text = h.LastStatus;
            item.SubItems[4].Text = h.LastLatency.HasValue ? h.LastLatency.Value + " ms" : "";
            item.SubItems[5].Text = h.LastCheck?.ToString("g", CultureInfo.CurrentCulture) ?? "";
            Color color = h.LastStatus switch
            {
                null or "" => SystemColors.WindowText,
                CheckingText => SystemColors.GrayText,
                "Online" => _listOk,
                _ => _listError
            };
            item.SubItems[3].ForeColor = color;
            item.SubItems[4].ForeColor = color;
        }

        private void SetDiagnosis(string text, Color color)
        {
            lblDiagnosis.Text = text;
            lblDiagnosis.ForeColor = color;
        }

        #endregion

        #region Add / edit / delete

        private void BtnProvAdd_Click(object sender, EventArgs e)
        {
            var host = new ReferenceHost { Kind = "Provider" };
            using var dialog = new ReferenceHostDialog(host, true);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _data.Providers.Add(host);
                SaveData();
                RefreshProviderList();
            }
        }

        private void BtnProvEdit_Click(object sender, EventArgs e)
        {
            ReferenceHost original = SelectedProvider;
            if (original == null)
            {
                return;
            }

            ReferenceHost copy = original.Clone();
            using var dialog = new ReferenceHostDialog(copy, false);
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            int index = _data.Providers.IndexOf(original);
            if (index >= 0)
            {
                _data.Providers[index] = copy;
            }

            SaveData();
            RefreshProviderList();
        }

        private void BtnProvDelete_Click(object sender, EventArgs e)
        {
            List<ReferenceHost> selected = listProviders.SelectedItems.Cast<ListViewItem>()
                .Select(i => i.Tag).OfType<ReferenceHost>().ToList();
            if (selected.Count == 0)
            {
                return;
            }

            string question = selected.Count == 1 ? $"Delete \"{selected[0].Name}\"?" : $"Delete {selected.Count} addresses?";
            if (MessageBox.Show(this, question, "AdminTool", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            foreach (ReferenceHost h in selected)
            {
                _data.Providers.Remove(h);
            }

            SaveData();
            RefreshProviderList();
        }

        private void ListProviders_DoubleClick(object sender, EventArgs e)
        {
            ReferenceHost h = SelectedProvider;
            if (h != null)
            {
                comboTarget.Text = h.Address;
                BtnPing_Click(sender, e);
            }
        }

        private void ListProviders_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    BtnProvDelete_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.F2:
                    BtnProvEdit_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    ListProviders_DoubleClick(sender, e);
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        #endregion

        #region Provider info / auto-detect

        private async Task UpdateProviderInfoAsync(CancellationToken ct)
        {
            ProviderInfo info = await NetworkTools.GetProviderInfoAsync(ct);
            if (info == null || IsDisposed)
            {
                return;
            }

            _data.ProviderInfo = info.ToDisplayText();
            lblProviderInfo.Text = _data.ProviderInfo;
            if (info.Org.Length > 0)
            {
                statusPublicIp.ToolTipText = "Provider: " + info.Org;
            }
        }

        private async void BtnProvDetect_Click(object sender, EventArgs e)
        {
            tabMain.SelectedTab = tabProvider;
            await RunAsync("Auto-detect", AutoDetectProviderAsync, showOutput: false);
        }

        private async Task AutoDetectProviderAsync(CancellationToken ct)
        {
            LogHeader("Auto-detect router, provider and DNS");
            int added = 0;

            void Add(string name, string address, string kind)
            {
                if (_data.Providers.Any(p => string.Equals(p.Address, address, StringComparison.OrdinalIgnoreCase)))
                {
                    LogMuted($"{kind,-9} {address} is already saved");
                    return;
                }

                _data.Providers.Add(new ReferenceHost { Name = name, Address = address, Kind = kind });
                added++;
                LogOk($"{kind,-9} {address}  ({name})");
            }

            // 1. Router = default gateway
            List<(string Adapter, IPAddress Address)> gateways = NetworkTools.GetGateways();
            foreach ((string adapter, IPAddress address) in gateways)
            {
                Add($"Router ({adapter})", address.ToString(), "Router");
            }

            if (gateways.Count == 0)
            {
                LogWarn("No router (default gateway) found.");
            }

            // 2. DNS servers
            foreach (IPAddress dns in NetworkTools.GetDnsServers())
            {
                Add(NetworkTools.IsPrivate(dns) ? "DNS (local)" : "DNS server", dns.ToString(), "DNS");
            }

            // 3. Provider name and the first node of the provider
            SetState("Auto-detect: asking for provider info …");
            await UpdateProviderInfoAsync(ct);
            string providerName = _data.ProviderInfo.Split('\n').FirstOrDefault()?.Replace("Provider :", "").Trim() ?? "";

            SetState("Auto-detect: searching the provider node …");
            IPAddress node = await NetworkTools.FindProviderNodeAsync(Math.Max(_data.Settings.TimeoutMs, 1000), ct);
            if (node != null)
            {
                string nodeName = await NetworkTools.ReverseLookupAsync(node, 1500, ct);
                string label = providerName.Length > 0 && providerName != "unknown" ? providerName : "Provider";
                Add(nodeName.Length > 0 ? $"{label} node ({nodeName})" : $"{label} node", node.ToString(), "Provider");
            }
            else
            {
                LogWarn("Provider node not found (the hops after your router do not answer). You can add it by hand.");
            }

            // 4. Internet reference hosts
            if (!_data.Providers.Any(p => p.Kind == "Internet"))
            {
                Add("Cloudflare", "1.1.1.1", "Internet");
                Add("Google DNS", "8.8.8.8", "Internet");
                if (_publicIPv6.Length > 0)
                {
                    Add("Cloudflare IPv6", "2606:4700:4700::1111", "Internet");
                }
            }

            SaveData();
            RefreshProviderList();
            LogOk($"Auto-detect done: {added} address(es) added.");
            if (_data.ProviderInfo.Length > 0)
            {
                foreach (string line in _data.ProviderInfo.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    LogInfo(line);
                }
            }
        }

        #endregion

        #region Ping all / diagnosis

        private async void BtnProvCheck_Click(object sender, EventArgs e)
        {
            if (_data.Providers.Count == 0)
            {
                SetState("No connection addresses saved – click \"Auto-detect\" first.");
                return;
            }

            await RunAsync("Ping connection addresses", async ct =>
            {
                LogHeader("Ping router / provider / DNS / internet");
                await PingReferenceHostsAsync(_data.Providers.ToList(), ct);
                SaveData();
            });
        }

        /// <summary>
        /// Pings the hosts in parallel. Returns null when the list is empty, otherwise whether at least one answered.
        /// </summary>
        private async Task<bool?> PingReferenceHostsAsync(List<ReferenceHost> hosts, CancellationToken ct)
        {
            if (hosts.Count == 0)
            {
                return null;
            }

            int timeout = Math.Max(_data.Settings.TimeoutMs, 1000);
            foreach (ReferenceHost h in hosts)
            {
                h.LastStatus = CheckingText;
                UpdateProviderItem(h);
            }

            bool any = false;
            try
            {
                await Task.WhenAll(hosts.Select(async h =>
                {
                    long? rtt = null;
                    try
                    {
                        IPAddress[] all = await NetworkTools.ResolveAllAsync(h.Address, ct);
                        IPAddress ip = NetworkTools.PickAddress(all, AddressPreference.Auto);
                        if (ip != null)
                        {
                            rtt = await SafePingAsync(ip, timeout);
                            // one retry, a single lost packet should not count as down
                            rtt ??= await SafePingAsync(ip, timeout);
                        }
                    }
                    catch (SocketException)
                    {
                        // name could not be resolved
                    }

                    h.LastStatus = rtt.HasValue ? "Online" : "No reply";
                    h.LastLatency = rtt;
                    h.LastCheck = DateTime.Now;
                    any |= rtt.HasValue;
                    UpdateProviderItem(h);

                    string line = $"{h.Kind,-9} {h.Name,-28} {h.Address,-26} " + (rtt.HasValue ? $"{rtt} ms" : "no reply");
                    if (rtt.HasValue)
                    {
                        LogOk(line);
                    }
                    else
                    {
                        LogError(line);
                    }
                }));
            }
            finally
            {
                foreach (ReferenceHost h in hosts.Where(x => x.LastStatus == CheckingText))
                {
                    h.LastStatus = "";
                    UpdateProviderItem(h);
                }
            }

            return any;
        }

        private async void BtnDiagnose_Click(object sender, EventArgs e)
        {
            await RunAsync("Diagnosis", DiagnoseAsync);
        }

        private void BtnProvDiagnose_Click(object sender, EventArgs e) => BtnDiagnose_Click(sender, e);

        private async Task DiagnoseAsync(CancellationToken ct)
        {
            LogHeader("Connection diagnosis: router → provider → internet → DNS → server");
            SetDiagnosis("Diagnosis running …", Color.Silver);

            List<ReferenceHost> Saved(string kind) => _data.Providers.Where(p => p.Kind == kind).ToList();

            // Without saved addresses the detected gateway and well known hosts are used (not saved).
            List<ReferenceHost> routers = Saved("Router");
            if (routers.Count == 0)
            {
                routers = NetworkTools.GetGateways()
                    .Select(g => new ReferenceHost { Name = "Router (detected)", Address = g.Address.ToString(), Kind = "Router" })
                    .ToList();
            }

            List<ReferenceHost> providers = Saved("Provider").Concat(Saved("DNS").Where(d =>
                IPAddress.TryParse(d.Address, out IPAddress ip) && !NetworkTools.IsPrivate(ip))).ToList();

            List<ReferenceHost> internet = Saved("Internet");
            if (internet.Count == 0)
            {
                internet = new List<ReferenceHost>
                {
                    new() { Name = "Cloudflare", Address = "1.1.1.1", Kind = "Internet" },
                    new() { Name = "Google DNS", Address = "8.8.8.8", Kind = "Internet" }
                };
            }

            LogInfo("Step 1/5 – Router");
            bool? routerOk = await PingReferenceHostsAsync(routers, ct);
            SetProgress(1, 5);

            LogInfo("Step 2/5 – Provider");
            bool? providerOk = await PingReferenceHostsAsync(providers, ct);
            if (providerOk == null)
            {
                LogMuted("No provider address saved – \"Auto-detect\" in the Provider tab finds it.");
            }

            SetProgress(2, 5);

            LogInfo("Step 3/5 – Internet");
            bool internetOk = await PingReferenceHostsAsync(internet, ct) == true;
            if (!internetOk)
            {
                // Some networks block ping: a web request is the better test then.
                internetOk = await NetworkTools.CheckInternetAsync();
                if (internetOk)
                {
                    LogWarn("Ping is blocked, but web access works – the internet is reachable.");
                }
            }

            SetProgress(3, 5);

            LogInfo("Step 4/5 – Name resolution (DNS)");
            bool dnsOk;
            try
            {
                using var dnsCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                dnsCts.CancelAfter(4000);
                IPAddress[] r = await NetworkTools.ResolveAllAsync("www.google.com", dnsCts.Token);
                dnsOk = r.Length > 0;
                LogOk($"www.google.com → {r.FirstOrDefault()}");
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                dnsOk = false;
                LogError("Name resolution failed: " + ex.GetBaseException().Message);
            }

            SetProgress(4, 5);

            LogInfo("Step 5/5 – Server");
            bool? serverOk = null;
            string serverText = comboTarget.Text.Trim();
            ServerEntry byName = _data.Servers.FirstOrDefault(s => string.Equals(s.Name, serverText, StringComparison.OrdinalIgnoreCase));
            string host = null;
            int port = _data.Settings.DefaultPort;
            if (byName != null)
            {
                host = EntryAddress(byName);
                port = byName.Port;
            }
            else if (serverText.Length > 0 && NetworkTools.TryParseTarget(serverText, out string h, out int? p))
            {
                host = h;
                port = p ?? port;
            }

            if (host == null)
            {
                LogMuted("No server in the target box – skipped. Select a saved server to include it.");
            }
            else
            {
                try
                {
                    IPAddress ip = await ResolveLoggedAsync(host, ct);
                    Task<long?> ping = SafePingAsync(ip, _data.Settings.TimeoutMs);
                    Task<TcpCheckResult> tcp = NetworkTools.TcpConnectAsync(ip, port, _data.Settings.TimeoutMs, ct);
                    await Task.WhenAll(ping, tcp);
                    serverOk = tcp.Result.Open || ping.Result.HasValue;

                    string line = $"{FormatEndpoint(ip, port)}: ping {(ping.Result.HasValue ? ping.Result + " ms" : "no reply")}, " +
                                  $"port {(tcp.Result.Open ? "open" : "closed")}";
                    if (tcp.Result.Open)
                    {
                        LogOk(line);
                    }
                    else if (ping.Result.HasValue)
                    {
                        LogWarn(line + " – the machine is up, but the game server does not accept connections");
                    }
                    else
                    {
                        LogError(line);
                    }

                    if (!tcp.Result.Open && ping.Result.HasValue)
                    {
                        serverOk = false;
                    }
                }
                catch (Exception ex) when (ex is SocketException or InvalidOperationException)
                {
                    serverOk = false;
                    LogError($"{host}: {ex.GetBaseException().Message}");
                }
            }

            SetProgress(5, 5);

            // Conclusion
            string verdict;
            Color color;
            if (internetOk)
            {
                if (!dnsOk)
                {
                    verdict = "✖ Internet works, but name resolution (DNS) fails. Use the IP address or change the DNS server.";
                    color = _logWarn;
                }
                else if (serverOk == null)
                {
                    verdict = "✔ Your connection is fine (router, provider and internet reachable).";
                    color = _logOk;
                }
                else if (serverOk == true)
                {
                    verdict = $"✔ Everything OK – your connection works and the server {host} is reachable.";
                    color = _logOk;
                }
                else
                {
                    verdict = $"✖ Your connection is fine – the SERVER {host}:{port} is down or blocks the connection.";
                    color = _logError;
                }
            }
            else if (routerOk == false)
            {
                verdict = "✖ Your router does not answer – problem in your own network (cable, Wi-Fi, router).";
                color = _logError;
            }
            else if (providerOk == false)
            {
                verdict = "✖ Router OK, but your PROVIDER does not answer – provider outage or line problem.";
                color = _logError;
            }
            else
            {
                verdict = "✖ Router OK, but no internet – most likely a problem at your provider.";
                color = _logError;
            }

            if (routerOk == false && internetOk)
            {
                LogMuted("Note: your router does not answer ping, that is normal for some routers.");
            }

            Log(verdict, color);
            SetDiagnosis(verdict, color);
            SaveData();
        }

        #endregion
    }
}
