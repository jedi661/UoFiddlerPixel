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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Server admin tool: ping, traceroute, DNS, port check / scan, shard status,
    /// LAN discovery and an address book for servers (host, IPv4, IPv6, port, group, notes).
    /// </summary>
    public partial class AdminToolForm : Form
    {
        private static AdminToolForm _instance;

        #region Colors

        // Log colors (dark background)
        private static readonly Color _logOk = Color.FromArgb(98, 210, 130);
        private static readonly Color _logWarn = Color.FromArgb(240, 185, 70);
        private static readonly Color _logError = Color.FromArgb(245, 100, 100);
        private static readonly Color _logInfo = Color.FromArgb(220, 223, 228);
        private static readonly Color _logHeader = Color.FromArgb(100, 175, 255);
        private static readonly Color _logMuted = Color.FromArgb(125, 132, 145);

        // List colors (light background)
        private static readonly Color _listOk = Color.FromArgb(20, 135, 60);
        private static readonly Color _listWarn = Color.FromArgb(190, 120, 0);
        private static readonly Color _listError = Color.FromArgb(195, 40, 40);

        #endregion

        private const string CheckingText = "Checking…";

        private static readonly Dictionary<int, string> _wellKnownPorts = new()
        {
            [21] = "FTP", [22] = "SSH", [23] = "Telnet", [25] = "SMTP", [53] = "DNS", [80] = "HTTP",
            [110] = "POP3", [143] = "IMAP", [443] = "HTTPS", [445] = "SMB", [1433] = "MS SQL",
            [2593] = "UO shard (RunUO / ServUO)", [2594] = "UO shard", [2595] = "UO shard", [2596] = "UO shard",
            [2597] = "UO shard", [2598] = "UO shard", [2599] = "UO shard", [3306] = "MySQL", [3389] = "Remote Desktop",
            [5432] = "PostgreSQL", [5900] = "VNC", [7775] = "UO login (classic)", [7776] = "UO game (classic)",
            [8080] = "HTTP (alt)", [8443] = "HTTPS (alt)"
        };

        private readonly AdminToolData _data;
        private readonly Dictionary<string, ListViewItem> _itemsById = new();
        private readonly System.Windows.Forms.Timer _statusTimer = new() { Interval = 60000 };

        private CancellationTokenSource _cts;
        private bool _busy;
        private string _operationTitle = "";
        private int _sortColumn;
        private bool _sortAscending = true;
        private bool _updatingGroups;
        private string _publicIPv4 = "";
        private string _publicIPv6 = "";
        private Font _boldListFont;

        // Ping statistics
        private int _statSent;
        private readonly List<long> _statRtts = new();

        #region Construction / singleton

        public AdminToolForm()
        {
            InitializeComponent();

            _data = AdminToolStore.Load();

            ApplySettingsToUi();
            RefreshHistory();
            RefreshGroups();
            RefreshServerList();
            ResetStats();
            UpdateSelectionButtons();

            labelIP.Text = "-";
            _statusTimer.Tick += async (_, _) => await UpdateInternetStatusAsync();

            Load += AdminToolForm_Load;
            KeyDown += AdminToolForm_KeyDown;
            FormClosing += AdminToolForm_FormClosing;
            FormClosed += (_, _) =>
            {
                _statusTimer.Dispose();
                _boldListFont?.Dispose();
                if (_instance == this)
                {
                    _instance = null;
                }
            };
        }

        /// <summary>
        /// Returns the open instance or creates a new one (only one AdminTool window at a time).
        /// </summary>
        public static AdminToolForm GetInstance()
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new AdminToolForm();
            }
            else
            {
                if (_instance.WindowState == FormWindowState.Minimized)
                {
                    _instance.WindowState = FormWindowState.Normal;
                }

                _instance.Activate();
            }

            return _instance;
        }

        public void ÖffneAdminToolForm()
        {
            AdminToolForm form = GetInstance();
            form.Show();
            form.Activate();
        }

        private async void AdminToolForm_Load(object sender, EventArgs e)
        {
            try
            {
                splitMain.SplitterDistance = Math.Clamp(_data.Settings.SplitterDistance,
                    splitMain.Panel1MinSize, Math.Max(splitMain.Panel1MinSize, splitMain.Width - splitMain.Panel2MinSize));
            }
            catch (InvalidOperationException)
            {
                // window too small for the saved splitter position
            }

            LogHeader("Server Admin Tool ready");
            LogMuted($"{_data.Servers.Count} saved servers · data file: {AdminToolStore.FilePath}");
            LogMuted("Tip: Enter = Ping · Esc = Stop · Ctrl+F = search · F5 = check all · double click = use server");

            _statusTimer.Start();

            try
            {
                Task internet = UpdateInternetStatusAsync();
                Task network = RefreshNetworkInfoAsync();
                await Task.WhenAll(internet, network);
            }
            catch (Exception ex)
            {
                LogError("Network info failed: " + ex.Message);
            }

            if (_data.Settings.CheckAllOnOpen && _data.Servers.Count > 0 && !IsDisposed)
            {
                await CheckServersAsync(_data.Servers.ToList());
            }
        }

        private void AdminToolForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
            _statusTimer.Stop();
            SaveData();
        }

        private void AdminToolForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && _busy)
            {
                _cts?.Cancel();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                textBoxSearch.Focus();
                textBoxSearch.SelectAll();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                BtnCheckAll_Click(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        #endregion

        #region Settings / persistence

        private AddressPreference Family =>
            Enum.TryParse(_data.Settings.AddressFamily, true, out AddressPreference p) ? p : AddressPreference.Auto;

        private void ApplySettingsToUi()
        {
            AdminToolSettings s = _data.Settings;
            txtPort.Text = s.DefaultPort.ToString(CultureInfo.InvariantCulture);
            txtTimeout.Text = s.TimeoutMs.ToString(CultureInfo.InvariantCulture);
            txtCount.Text = s.PingCount.ToString(CultureInfo.InvariantCulture);
            txtPortList.Text = s.PortList;
            comboFamily.SelectedIndex = Math.Max(0, comboFamily.Items.IndexOf(s.AddressFamily ?? "Auto"));
            BtnTopMost.Checked = s.TopMost;
            TopMost = s.TopMost;
            BtnCheckOnOpen.Checked = s.CheckAllOnOpen;
        }

        private void ReadSettingsFromUi()
        {
            AdminToolSettings s = _data.Settings;
            s.DefaultPort = ParseInt(txtPort, 1, 65535, s.DefaultPort);
            s.TimeoutMs = ParseInt(txtTimeout, 100, 60000, s.TimeoutMs);
            s.PingCount = ParseInt(txtCount, 0, 100000, s.PingCount);
            s.PortList = txtPortList.Text.Trim();
            s.AddressFamily = comboFamily.SelectedItem?.ToString() ?? "Auto";
            s.Parallelism = Math.Clamp(s.Parallelism, 1, 256);
        }

        private static int ParseInt(ToolStripTextBox box, int min, int max, int fallback)
        {
            if (int.TryParse(box.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= min && v <= max)
            {
                return v;
            }

            // invalid input: show the value that is actually used
            box.Text = fallback.ToString(CultureInfo.InvariantCulture);
            return fallback;
        }

        private void SaveData()
        {
            try
            {
                ReadSettingsFromUi();
                if (splitMain.SplitterDistance > 0)
                {
                    _data.Settings.SplitterDistance = splitMain.SplitterDistance;
                }

                AdminToolStore.Save(_data);
            }
            catch (Exception ex)
            {
                LogError("Could not save the address book: " + ex.Message);
            }
        }

        private void BtnTopMost_Click(object sender, EventArgs e)
        {
            TopMost = BtnTopMost.Checked;
            _data.Settings.TopMost = BtnTopMost.Checked;
            SaveData();
        }

        private void BtnCheckOnOpen_Click(object sender, EventArgs e)
        {
            _data.Settings.CheckAllOnOpen = BtnCheckOnOpen.Checked;
            SaveData();
        }

        #endregion

        #region Logging

        private void Log(string text, Color color)
        {
            if (IsDisposed || richLog.IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Log(text, color)));
                return;
            }

            // Keep the log fast: drop the oldest lines when it gets very long.
            if (richLog.GetLineFromCharIndex(richLog.TextLength) > 6000)
            {
                richLog.ReadOnly = false;
                richLog.Select(0, richLog.GetFirstCharIndexFromLine(2000));
                richLog.SelectedText = "";
                richLog.ReadOnly = true;
            }

            richLog.SelectionStart = richLog.TextLength;
            richLog.SelectionLength = 0;
            richLog.SelectionColor = _logMuted;
            richLog.AppendText(DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "  ");
            richLog.SelectionColor = color;
            richLog.AppendText(text + Environment.NewLine);
            richLog.SelectionColor = richLog.ForeColor;
            richLog.ScrollToCaret();
        }

        private void LogInfo(string text) => Log(text, _logInfo);
        private void LogOk(string text) => Log(text, _logOk);
        private void LogWarn(string text) => Log(text, _logWarn);
        private void LogError(string text) => Log(text, _logError);
        private void LogMuted(string text) => Log(text, _logMuted);
        private void LogHeader(string text) => Log("── " + text + " ──", _logHeader);

        private void BtnClearLog_Click(object sender, EventArgs e) => richLog.Clear();

        private void BtnCopyLog_Click(object sender, EventArgs e)
        {
            string text = richLog.SelectionLength > 0 ? richLog.SelectedText : richLog.Text;
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
                SetState("Output copied to the clipboard");
            }
        }

        private void BtnSaveLog_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"AdminTool_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(dialog.FileName, richLog.Text, Encoding.UTF8);
                SetState("Output saved: " + dialog.FileName);
            }
        }

        private void SetLastAddress(string address)
        {
            labelIP.Text = string.IsNullOrEmpty(address) ? "-" : address;
        }

        private void BtnCopyIP_Click(object sender, EventArgs e)
        {
            if (labelIP.Text != "-" && !string.IsNullOrEmpty(labelIP.Text))
            {
                Clipboard.SetText(labelIP.Text);
                SetState("Copied: " + labelIP.Text);
            }
        }

        #endregion

        #region Operation runner

        private void SetState(string text) => statusState.Text = text;

        private void SetProgress(int done, int total)
        {
            if (total <= 0)
            {
                return;
            }

            statusProgress.Style = ProgressBarStyle.Blocks;
            statusProgress.Maximum = total;
            statusProgress.Value = Math.Clamp(done, 0, total);
            statusState.Text = $"{_operationTitle}: {done} / {total}";
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            foreach (ToolStripItem item in new ToolStripItem[]
                     {
                         BtnPing, BtnPingContinuous, BtnTracert, BtnDns, BtnPortCheck, BtnPortScan, BtnUoStatus, BtnLanScan
                     })
            {
                item.Enabled = !busy;
            }

            BtnCheckAll.Enabled = !busy;
            BtnStop.Enabled = busy;
            statusProgress.Visible = busy;
            statusProgress.Style = ProgressBarStyle.Marquee;
            statusProgress.MarqueeAnimationSpeed = 30;
            UseWaitCursor = false;
        }

        /// <summary>
        /// Runs one network operation at a time with cancel support, timing and error handling.
        /// </summary>
        private async Task RunAsync(string title, Func<CancellationToken, Task> work, bool showOutput = true)
        {
            if (_busy)
            {
                SystemSounds.Beep.Play();
                SetState("Another operation is running – press Stop (Esc) first.");
                return;
            }

            ReadSettingsFromUi();
            _operationTitle = title;
            _cts = new CancellationTokenSource();
            SetBusy(true);
            SetState(title + " …");
            if (showOutput)
            {
                tabMain.SelectedTab = tabOutput;
            }

            var sw = Stopwatch.StartNew();
            try
            {
                await work(_cts.Token);
                SetState($"{title} finished in {sw.Elapsed.TotalSeconds:0.0} s");
            }
            catch (OperationCanceledException)
            {
                LogWarn($"{title} stopped.");
                SetState(title + " stopped");
            }
            catch (Exception ex)
            {
                LogError($"{title} failed: {ex.GetBaseException().Message}");
                SetState(title + " failed");
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                if (!IsDisposed)
                {
                    SetBusy(false);
                }
            }
        }

        private void BtnStop_Click(object sender, EventArgs e) => _cts?.Cancel();

        #endregion

        #region Target handling

        private void ComboTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnPing_Click(sender, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Reads the target box. Accepts host, IP, host:port, [IPv6]:port, URL or the name of a saved server.
        /// </summary>
        private bool TryGetTarget(out string host, out int port)
        {
            ReadSettingsFromUi();
            string text = comboTarget.Text.Trim();
            host = "";
            port = _data.Settings.DefaultPort;

            if (text.Length == 0)
            {
                ShowInputError("Enter a host name, IPv4 or IPv6 address – or pick a saved server.");
                return false;
            }

            // The name of a saved server works as target too.
            ServerEntry byName = _data.Servers.FirstOrDefault(s => string.Equals(s.Name, text, StringComparison.OrdinalIgnoreCase));
            if (byName != null && !NetworkTools.TryParseTarget(text, out _, out _))
            {
                host = EntryAddress(byName);
                port = byName.Port;
                comboTarget.Text = FormatTarget(host, port);
                AddToHistory(comboTarget.Text);
                return true;
            }

            if (!NetworkTools.TryParseTarget(text, out host, out int? parsedPort))
            {
                ShowInputError($"\"{text}\" is not a valid host name, IPv4 or IPv6 address.");
                return false;
            }

            if (parsedPort.HasValue)
            {
                port = parsedPort.Value;
            }

            AddToHistory(text);
            return true;
        }

        private void ShowInputError(string message)
        {
            LogError(message);
            SetState(message);
            MessageBox.Show(this, message, "AdminTool", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            comboTarget.Focus();
        }

        private static string FormatTarget(string host, int port)
        {
            if (IPAddress.TryParse(host, out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return $"[{host}]:{port}";
            }

            return $"{host}:{port}";
        }

        private static string FormatEndpoint(IPAddress ip, int port) =>
            ip.AddressFamily == AddressFamily.InterNetworkV6 ? $"[{ip}]:{port}" : $"{ip}:{port}";

        private string EntryAddress(ServerEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.Host))
            {
                if (Family == AddressPreference.IPv6 && NetworkTools.IsIPv6(entry.IPv6))
                {
                    return entry.IPv6.Trim();
                }

                if (Family == AddressPreference.IPv4 && NetworkTools.IsIPv4(entry.IPv4))
                {
                    return entry.IPv4.Trim();
                }
            }

            return entry.PreferredAddress;
        }

        private void AddToHistory(string text)
        {
            text = text.Trim();
            if (text.Length == 0)
            {
                return;
            }

            _data.History.RemoveAll(h => string.Equals(h, text, StringComparison.OrdinalIgnoreCase));
            _data.History.Insert(0, text);
            if (_data.History.Count > 30)
            {
                _data.History.RemoveRange(30, _data.History.Count - 30);
            }

            RefreshHistory();
        }

        private void RefreshHistory()
        {
            string current = comboTarget.Text;
            comboTarget.BeginUpdate();
            comboTarget.Items.Clear();
            comboTarget.Items.AddRange(_data.History.Cast<object>().ToArray());
            comboTarget.EndUpdate();
            comboTarget.Text = current;

            // Autocomplete from history and from every saved address / name.
            var source = new AutoCompleteStringCollection();
            source.AddRange(_data.History
                .Concat(_data.Servers.SelectMany(s => new[] { s.Name, s.Host, s.IPv4, s.IPv6 }))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());
            comboTarget.AutoCompleteCustomSource = source;
            comboTarget.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private async Task<IPAddress> ResolveLoggedAsync(string host, CancellationToken ct)
        {
            IPAddress[] all = await NetworkTools.ResolveAllAsync(host, ct);
            IPAddress ip = NetworkTools.PickAddress(all, Family);
            if (ip == null)
            {
                throw new InvalidOperationException($"{host} has no {Family} address ({string.Join(", ", all.Select(a => a.ToString()))}).");
            }

            if (!IPAddress.TryParse(host, out _))
            {
                LogMuted($"{host} → {ip}" + (all.Length > 1 ? $"   ({all.Length} addresses, use DNS lookup to see all)" : ""));
            }

            SetLastAddress(ip.ToString());
            return ip;
        }

        #endregion

        #region Ping

        private async void BtnPing_Click(object sender, EventArgs e)
        {
            if (!_busy && TryGetTarget(out string host, out _))
            {
                await RunAsync("Ping", ct => PingLoopAsync(host, _data.Settings.PingCount, ct));
            }
            else if (_busy)
            {
                SetState("Another operation is running – press Stop (Esc) first.");
            }
        }

        private async void BtnPingContinuous_Click(object sender, EventArgs e)
        {
            if (!_busy && TryGetTarget(out string host, out _))
            {
                await RunAsync("Continuous ping", ct => PingLoopAsync(host, 0, ct));
            }
        }

        private async Task PingLoopAsync(string host, int count, CancellationToken ct)
        {
            LogHeader(count == 0 ? $"Ping {host} (continuous – Esc to stop)" : $"Ping {host} × {count}");
            IPAddress ip = await ResolveLoggedAsync(host, ct);
            ResetStats();

            try
            {
                for (int i = 0; count == 0 || i < count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    long? rtt = null;
                    try
                    {
                        PingReply reply = await NetworkTools.PingAsync(ip, _data.Settings.TimeoutMs);
                        if (reply.Status == IPStatus.Success)
                        {
                            rtt = reply.RoundtripTime;
                            string ttl = reply.Options != null ? $"  TTL={reply.Options.Ttl}" : "";
                            LogOk($"Reply from {reply.Address}: time={reply.RoundtripTime} ms{ttl}");
                        }
                        else
                        {
                            LogWarn($"#{i + 1}: {reply.Status}");
                        }
                    }
                    catch (PingException ex)
                    {
                        LogError($"#{i + 1}: {ex.GetBaseException().Message}");
                    }

                    AddStat(rtt);
                    if (count > 0)
                    {
                        SetProgress(i + 1, count);
                    }

                    if (count == 0 || i < count - 1)
                    {
                        await Task.Delay(1000, ct);
                    }
                }
            }
            finally
            {
                LogPingSummary(ip);
            }
        }

        private void ResetStats()
        {
            _statSent = 0;
            _statRtts.Clear();
            latencyGraph.Clear();
            UpdateStatLabels();
        }

        private void AddStat(long? rtt)
        {
            _statSent++;
            if (rtt.HasValue)
            {
                _statRtts.Add(rtt.Value);
            }

            latencyGraph.AddSample(rtt);
            UpdateStatLabels();
        }

        private void UpdateStatLabels()
        {
            int received = _statRtts.Count;
            int lost = _statSent - received;
            static string Card(string title, string value) => title + Environment.NewLine + value;

            lblStatSent.Text = Card("Sent", _statSent.ToString(CultureInfo.InvariantCulture));
            lblStatReceived.Text = Card("Received", received.ToString(CultureInfo.InvariantCulture));
            lblStatLoss.Text = Card("Loss", _statSent == 0 ? "-" : $"{lost * 100.0 / _statSent:0.#} %");
            lblStatLoss.ForeColor = lost == 0 ? Color.White : _logError;

            if (received == 0)
            {
                lblStatMin.Text = Card("Min", "-");
                lblStatAvg.Text = Card("Avg", "-");
                lblStatMax.Text = Card("Max", "-");
                lblStatJitter.Text = Card("Jitter", "-");
                return;
            }

            lblStatMin.Text = Card("Min", _statRtts.Min() + " ms");
            lblStatAvg.Text = Card("Avg", _statRtts.Average().ToString("0", CultureInfo.InvariantCulture) + " ms");
            lblStatMax.Text = Card("Max", _statRtts.Max() + " ms");
            double jitter = received > 1
                ? _statRtts.Zip(_statRtts.Skip(1), (a, b) => Math.Abs(a - b)).Average()
                : 0;
            lblStatJitter.Text = Card("Jitter", jitter.ToString("0.#", CultureInfo.InvariantCulture) + " ms");
        }

        private void LogPingSummary(IPAddress ip)
        {
            if (_statSent == 0)
            {
                return;
            }

            int received = _statRtts.Count;
            string loss = $"{(_statSent - received) * 100.0 / _statSent:0.#}%";
            string text = $"{ip}: sent {_statSent}, received {received}, loss {loss}";
            if (received > 0)
            {
                text += $" · min {_statRtts.Min()} / avg {_statRtts.Average():0} / max {_statRtts.Max()} ms";
            }

            if (received == _statSent)
            {
                LogOk(text);
            }
            else if (received > 0)
            {
                LogWarn(text);
            }
            else
            {
                LogError(text);
            }
        }

        private static async Task<long?> SafePingAsync(IPAddress ip, int timeoutMs)
        {
            try
            {
                PingReply reply = await NetworkTools.PingAsync(ip, timeoutMs);
                return reply.Status == IPStatus.Success ? reply.RoundtripTime : (long?)null;
            }
            catch (PingException)
            {
                return null;
            }
        }

        #endregion

        #region Traceroute

        private async void BtnTracert_Click(object sender, EventArgs e)
        {
            if (!_busy && TryGetTarget(out string host, out _))
            {
                await RunAsync("Traceroute", ct => TracerouteAsync(host, ct));
            }
        }

        private async Task TracerouteAsync(string host, CancellationToken ct)
        {
            const int maxHops = 30;
            LogHeader($"Traceroute to {host} (max {maxHops} hops)");
            IPAddress ip = await ResolveLoggedAsync(host, ct);
            int timeout = Math.Max(_data.Settings.TimeoutMs, 1000);

            for (int ttl = 1; ttl <= maxHops; ttl++)
            {
                ct.ThrowIfCancellationRequested();
                SetProgress(ttl, maxHops);

                var sw = Stopwatch.StartNew();
                PingReply reply;
                try
                {
                    reply = await NetworkTools.PingAsync(ip, timeout, ttl);
                }
                catch (PingException ex)
                {
                    LogWarn($"{ttl,3}   error: {ex.GetBaseException().Message}");
                    continue;
                }

                if (reply.Status is IPStatus.Success or IPStatus.TtlExpired or IPStatus.TimeExceeded)
                {
                    // RoundtripTime is 0 for "TTL expired" replies, so measure ourselves.
                    long ms = reply.Status == IPStatus.Success ? reply.RoundtripTime : sw.ElapsedMilliseconds;
                    string name = await NetworkTools.ReverseLookupAsync(reply.Address, 800, ct);
                    string line = $"{ttl,3}  {ms,5} ms   {reply.Address}" + (name.Length > 0 ? $"   [{name}]" : "");

                    if (reply.Status == IPStatus.Success)
                    {
                        LogOk(line);
                        LogOk($"Target reached in {ttl} hops.");
                        return;
                    }

                    LogInfo(line);
                }
                else
                {
                    LogMuted($"{ttl,3}      *      {reply.Status}");
                }
            }

            LogWarn($"Target not reached within {maxHops} hops.");
        }

        #endregion

        #region DNS lookup

        private async void BtnDns_Click(object sender, EventArgs e)
        {
            if (!_busy && TryGetTarget(out string host, out _))
            {
                await RunAsync("DNS lookup", ct => DnsLookupAsync(host, ct));
            }
        }

        private async Task DnsLookupAsync(string host, CancellationToken ct)
        {
            if (IPAddress.TryParse(host, out IPAddress ip))
            {
                LogHeader($"Reverse lookup {ip}");
                SetLastAddress(ip.ToString());
                try
                {
                    IPHostEntry entry = await Dns.GetHostEntryAsync(ip.ToString(), ct);
                    LogOk($"PTR    {entry.HostName}");
                    foreach (string alias in entry.Aliases)
                    {
                        LogInfo($"Alias  {alias}");
                    }
                }
                catch (SocketException)
                {
                    LogWarn("No host name (PTR record) found for this address.");
                }

                return;
            }

            LogHeader($"DNS lookup {host}");
            var sw = Stopwatch.StartNew();
            IPHostEntry result = await Dns.GetHostEntryAsync(host, ct);
            LogMuted($"Answer in {sw.ElapsedMilliseconds} ms");

            if (!string.Equals(result.HostName, host, StringComparison.OrdinalIgnoreCase))
            {
                LogInfo($"Name   {result.HostName}");
            }

            foreach (string alias in result.Aliases)
            {
                LogInfo($"Alias  {alias}");
            }

            var v4 = result.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToList();
            var v6 = result.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetworkV6).ToList();

            foreach (IPAddress a in v4)
            {
                LogOk($"A      {a}");
            }

            foreach (IPAddress a in v6)
            {
                LogOk($"AAAA   {a}");
            }

            if (v4.Count == 0)
            {
                LogMuted("No IPv4 (A) address.");
            }

            if (v6.Count == 0)
            {
                LogMuted("No IPv6 (AAAA) address.");
            }

            IPAddress picked = NetworkTools.PickAddress(result.AddressList, Family);
            if (picked != null)
            {
                SetLastAddress(picked.ToString());
            }
        }

        #endregion

        #region Port check / scan / shard status

        private async void BtnPortCheck_Click(object sender, EventArgs e)
        {
            if (!_busy && TryGetTarget(out string host, out int port))
            {
                await RunAsync("Port check", ct => PortCheckAsync(host, port, ct));
            }
        }

        private async Task PortCheckAsync(string host, int port, CancellationToken ct)
        {
            LogHeader($"TCP port check {host}:{port}");
            IPAddress ip = await ResolveLoggedAsync(host, ct);
            TcpCheckResult r = await NetworkTools.TcpConnectAsync(ip, port, _data.Settings.TimeoutMs, ct);
            string service = _wellKnownPorts.TryGetValue(port, out string s) ? $" ({s})" : "";

            if (r.Open)
            {
                LogOk($"{FormatEndpoint(ip, port)}{service} is OPEN – connected in {r.Milliseconds} ms");
            }
            else
            {
                LogError($"{FormatEndpoint(ip, port)}{service} is CLOSED or filtered ({r.Error}, {r.Milliseconds} ms)");
            }
        }

        private async void BtnPortScan_Click(object sender, EventArgs e)
        {
            if (!_busy && TryGetTarget(out string host, out _))
            {
                await RunAsync("Port scan", ct => PortScanAsync(host, ct));
            }
        }

        private async Task PortScanAsync(string host, CancellationToken ct)
        {
            List<int> ports = NetworkTools.ParsePorts(_data.Settings.PortList);
            if (ports.Count == 0)
            {
                throw new InvalidOperationException("No ports to scan. Enter e.g. 2593-2600,80,443 in \"Scan ports\".");
            }

            LogHeader($"Port scan {host}: {ports.Count} port(s)");
            IPAddress ip = await ResolveLoggedAsync(host, ct);

            var open = new List<(int Port, long Ms)>();
            int done = 0;
            using var gate = new SemaphoreSlim(Math.Clamp(_data.Settings.Parallelism * 4, 16, 200));

            // The lambdas resume on the UI thread, so the counters and UI updates below are safe.
            IEnumerable<Task> tasks = ports.Select(async port =>
            {
                await gate.WaitAsync(ct);
                try
                {
                    TcpCheckResult r = await NetworkTools.TcpConnectAsync(ip, port, _data.Settings.TimeoutMs, ct);
                    if (r.Open)
                    {
                        open.Add((port, r.Milliseconds));
                        LogOk($"Port {port,5} open ({r.Milliseconds} ms){(_wellKnownPorts.TryGetValue(port, out string s) ? "  " + s : "")}");
                    }
                }
                finally
                {
                    gate.Release();
                    done++;
                    if (done % 5 == 0 || done == ports.Count)
                    {
                        SetProgress(done, ports.Count);
                    }
                }
            });

            await Task.WhenAll(tasks);

            foreach ((int port, long ms) in open.OrderBy(o => o.Port))
            {
                AddResultRow(new ScanResult { Address = ip.ToString(), HostName = IPAddress.TryParse(host, out _) ? "" : host, Port = port, PortOpen = true },
                    ms, $"{port} open", _wellKnownPorts.TryGetValue(port, out string s) ? s : "");
            }

            if (open.Count == 0)
            {
                LogWarn($"No open port found on {ip} ({ports.Count} scanned).");
            }
            else
            {
                LogOk($"{open.Count} open port(s) on {ip}: {string.Join(", ", open.Select(o => o.Port).OrderBy(p => p))}");
            }
        }

        private async void BtnUoStatus_Click(object sender, EventArgs e)
        {
            if (!_busy && TryGetTarget(out string host, out int port))
            {
                await RunAsync("Shard status", ct => ShardStatusAsync(host, port, ct));
            }
        }

        private async Task ShardStatusAsync(string host, int port, CancellationToken ct)
        {
            LogHeader($"Shard status {host}:{port} (experimental)");
            IPAddress ip = await ResolveLoggedAsync(host, ct);
            var sw = Stopwatch.StartNew();
            string answer = await NetworkTools.QueryUoStatusAsync(ip, port, _data.Settings.TimeoutMs, ct);

            if (string.IsNullOrEmpty(answer))
            {
                LogWarn($"Connected to {FormatEndpoint(ip, port)}, but the server sent no status answer. " +
                        "The shard may not support the status query.");
                return;
            }

            LogOk($"Answer in {sw.ElapsedMilliseconds} ms:");
            // Typical answer: "ServUO, Name=My Shard, Age=12, Clients=5, Items=123456, Chars=789, Mem=512000K"
            foreach (string part in answer.Split(','))
            {
                string p = part.Trim();
                int eq = p.IndexOf('=');
                LogInfo(eq > 0 ? $"   {p.Substring(0, eq),-8}: {p.Substring(eq + 1)}" : "   " + p);
            }
        }

        #endregion

        #region LAN discovery

        private sealed class ScanResult
        {
            public string Address { get; init; } = "";
            public string HostName { get; init; } = "";
            public int Port { get; init; }
            public bool PortOpen { get; init; }
        }

        private async void BtnLanScan_Click(object sender, EventArgs e)
        {
            if (!_busy)
            {
                await RunAsync("LAN search", LanScanAsync, showOutput: false);
            }
        }

        private async Task LanScanAsync(CancellationToken ct)
        {
            int port = _data.Settings.DefaultPort;
            List<LocalSubnet> subnets = await Task.Run(() => NetworkTools.GetLocalSubnets(), ct);
            if (subnets.Count == 0)
            {
                LogWarn("No active IPv4 network adapter found.");
                return;
            }

            LogHeader($"Searching the local network (ping + TCP port {port})");
            foreach (LocalSubnet subnet in subnets)
            {
                LogMuted(subnet.ToString());
            }

            List<IPAddress> hosts = subnets.SelectMany(s => s.Hosts).Distinct().ToList();
            listResults.Items.Clear();
            tabMain.SelectedTab = tabResults;

            int timeout = Math.Min(_data.Settings.TimeoutMs, 800);
            int done = 0, online = 0, withPort = 0;
            using var gate = new SemaphoreSlim(Math.Clamp(_data.Settings.Parallelism * 2, 16, 128));

            IEnumerable<Task> tasks = hosts.Select(async ip =>
            {
                await gate.WaitAsync(ct);
                try
                {
                    Task<long?> ping = SafePingAsync(ip, timeout);
                    Task<TcpCheckResult> tcp = NetworkTools.TcpConnectAsync(ip, port, timeout, ct);
                    await Task.WhenAll(ping, tcp);

                    long? rtt = ping.Result;
                    bool open = tcp.Result.Open;
                    if (rtt == null && !open)
                    {
                        return;
                    }

                    online++;
                    string name = await NetworkTools.ReverseLookupAsync(ip, 1000, ct);
                    if (open)
                    {
                        withPort++;
                        LogOk($"Server found: {ip}{(name.Length > 0 ? $" [{name}]" : "")} – port {port} open");
                    }

                    AddResultRow(new ScanResult { Address = ip.ToString(), HostName = name, Port = port, PortOpen = open },
                        rtt ?? (open ? tcp.Result.Milliseconds : (long?)null),
                        open ? $"{port} open" : $"{port} closed",
                        open ? (_wellKnownPorts.TryGetValue(port, out string s) ? s : "Port open") : "Host online");
                }
                finally
                {
                    gate.Release();
                    done++;
                    if (done % 4 == 0 || done == hosts.Count)
                    {
                        SetProgress(done, hosts.Count);
                    }
                }
            });

            await Task.WhenAll(tasks);
            SortResults();
            LogHeader($"LAN search done: {online} host(s) online, {withPort} with port {port} open");
            SetState($"LAN search: {online} host(s) online, {withPort} with port {port} open");
        }

        private void AddResultRow(ScanResult result, long? ms, string portText, string info)
        {
            var item = new ListViewItem(new[]
            {
                result.Address,
                result.HostName,
                ms.HasValue ? ms.Value + " ms" : "-",
                portText,
                info
            })
            {
                Tag = result,
                UseItemStyleForSubItems = false
            };
            item.SubItems[3].ForeColor = result.PortOpen ? _listOk : SystemColors.GrayText;
            listResults.Items.Add(item);
        }

        private void SortResults()
        {
            ListViewItem[] items = listResults.Items.Cast<ListViewItem>()
                .OrderByDescending(i => ((ScanResult)i.Tag).PortOpen)
                .ThenBy(i => IpSortKey(((ScanResult)i.Tag).Address), StringComparer.Ordinal)
                .ToArray();
            listResults.BeginUpdate();
            listResults.Items.Clear();
            listResults.Items.AddRange(items);
            listResults.EndUpdate();
        }

        private void BtnResultsClear_Click(object sender, EventArgs e) => listResults.Items.Clear();

        private void BtnResultsUse_Click(object sender, EventArgs e)
        {
            if (listResults.SelectedItems.Count > 0 && listResults.SelectedItems[0].Tag is ScanResult r)
            {
                comboTarget.Text = FormatTarget(r.Address, r.Port);
                comboTarget.Focus();
            }
        }

        private void BtnResultsSave_Click(object sender, EventArgs e)
        {
            int added = 0, skipped = 0;
            foreach (ListViewItem item in listResults.SelectedItems)
            {
                if (item.Tag is not ScanResult r)
                {
                    continue;
                }

                var entry = new ServerEntry
                {
                    Name = r.HostName.Length > 0 ? r.HostName : r.Address,
                    Host = r.HostName,
                    IPv4 = NetworkTools.IsIPv4(r.Address) ? r.Address : "",
                    IPv6 = NetworkTools.IsIPv6(r.Address) ? r.Address : "",
                    Port = r.Port > 0 ? r.Port : _data.Settings.DefaultPort,
                    Group = "LAN"
                };

                if (FindDuplicate(entry) != null)
                {
                    skipped++;
                    continue;
                }

                _data.Servers.Add(entry);
                added++;
            }

            if (added + skipped == 0)
            {
                SetState("Select one or more results first.");
                return;
            }

            AfterServersChanged();
            LogOk($"{added} server(s) saved to the address book" + (skipped > 0 ? $", {skipped} already existed." : "."));
        }

        #endregion

        #region Network info / internet status

        private async Task UpdateInternetStatusAsync()
        {
            bool online = await NetworkTools.CheckInternetAsync();
            if (IsDisposed)
            {
                return;
            }

            LabelInternetStatus.Text = online ? "● Internet connected" : "● No internet connection";
            LabelInternetStatus.ForeColor = online ? _logOk : _logError;
        }

        private async void BtnNetworkRefresh_Click(object sender, EventArgs e)
        {
            tabMain.SelectedTab = tabNetwork;
            await RefreshNetworkInfoAsync();
            await UpdateInternetStatusAsync();
        }

        private async Task RefreshNetworkInfoAsync()
        {
            BtnNetworkRefresh.Enabled = false;
            try
            {
                string adapters = await Task.Run(() => NetworkTools.DescribeAdapters());
                richNetwork.Text = "Loading public addresses …" + Environment.NewLine + Environment.NewLine + adapters;

                Task<string> v4 = NetworkTools.GetPublicIpAsync(false);
                Task<string> v6 = NetworkTools.GetPublicIpAsync(true);
                await Task.WhenAll(v4, v6);
                if (IsDisposed)
                {
                    return;
                }

                _publicIPv4 = v4.Result;
                _publicIPv6 = v6.Result;

                var sb = new StringBuilder();
                sb.AppendLine($"Computer     : {Dns.GetHostName()}");
                sb.AppendLine($"Public IPv4  : {(_publicIPv4.Length > 0 ? _publicIPv4 : "not available")}");
                sb.AppendLine($"Public IPv6  : {(_publicIPv6.Length > 0 ? _publicIPv6 : "not available")}");
                sb.AppendLine();
                sb.Append(adapters);
                richNetwork.Text = sb.ToString();

                string shown = string.Join("  |  ", new[] { _publicIPv4, _publicIPv6 }.Where(x => x.Length > 0));
                statusPublicIp.Text = "Public IP: " + (shown.Length > 0 ? shown : "-");
            }
            catch (Exception ex)
            {
                richNetwork.Text = "Could not read the network information: " + ex.Message;
            }
            finally
            {
                if (!IsDisposed)
                {
                    BtnNetworkRefresh.Enabled = true;
                }
            }
        }

        private void BtnCopyPublicIp_Click(object sender, EventArgs e)
        {
            string text = string.Join(Environment.NewLine, new[] { _publicIPv4, _publicIPv6 }.Where(x => x.Length > 0));
            if (text.Length > 0)
            {
                Clipboard.SetText(text);
                SetState("Public IP copied");
            }
        }

        #endregion

        #region Address book – list, search, sort

        private List<ServerEntry> SelectedEntries =>
            listServers.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag).OfType<ServerEntry>().ToList();

        private ServerEntry SelectedEntry => SelectedEntries.FirstOrDefault();

        private void RefreshGroups()
        {
            _updatingGroups = true;
            string current = comboGroup.SelectedIndex > 0 ? comboGroup.SelectedItem?.ToString() : null;
            comboGroup.BeginUpdate();
            comboGroup.Items.Clear();
            comboGroup.Items.Add("All groups");
            foreach (string g in _data.Servers.Select(s => s.Group).Where(g => !string.IsNullOrWhiteSpace(g))
                         .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(g => g, StringComparer.CurrentCultureIgnoreCase))
            {
                comboGroup.Items.Add(g);
            }

            comboGroup.EndUpdate();
            int idx = current == null ? 0 : comboGroup.Items.IndexOf(current);
            comboGroup.SelectedIndex = idx < 0 ? 0 : idx;
            _updatingGroups = false;
        }

        private void RefreshServerList()
        {
            string[] terms = textBoxSearch.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string group = comboGroup.SelectedIndex > 0 ? comboGroup.SelectedItem?.ToString() : null;
            HashSet<string> selectedIds = SelectedEntries.Select(s => s.Id).ToHashSet();

            IEnumerable<ServerEntry> query = _data.Servers
                .Where(s => group == null || string.Equals(s.Group, group, StringComparison.OrdinalIgnoreCase))
                .Where(s => s.Matches(terms));

            List<ServerEntry> visible = SortEntries(query).ToList();

            listServers.BeginUpdate();
            listServers.Items.Clear();
            _itemsById.Clear();
            var items = new ListViewItem[visible.Count];
            for (int i = 0; i < visible.Count; i++)
            {
                items[i] = CreateItem(visible[i]);
                _itemsById[visible[i].Id] = items[i];
            }

            listServers.Items.AddRange(items);
            foreach (ListViewItem item in items)
            {
                if (selectedIds.Contains(((ServerEntry)item.Tag).Id))
                {
                    item.Selected = true;
                }
            }

            listServers.EndUpdate();

            statusCount.Text = visible.Count == _data.Servers.Count
                ? $"{_data.Servers.Count} server(s)"
                : $"{visible.Count} of {_data.Servers.Count} server(s)";
            UpdateSelectionButtons();
        }

        private IEnumerable<ServerEntry> SortEntries(IEnumerable<ServerEntry> entries)
        {
            IOrderedEnumerable<ServerEntry> fav = entries.OrderByDescending(s => s.Favorite);
            StringComparer cmp = StringComparer.CurrentCultureIgnoreCase;

            IOrderedEnumerable<ServerEntry> sorted = _sortColumn switch
            {
                1 => Order(fav, s => s.Host ?? "", cmp),
                2 => Order(fav, s => IpSortKey(s.IPv4), StringComparer.Ordinal),
                3 => Order(fav, s => IpSortKey(s.IPv6), StringComparer.Ordinal),
                4 => _sortAscending ? fav.ThenBy(s => s.Port) : fav.ThenByDescending(s => s.Port),
                5 => Order(fav, s => s.Group ?? "", cmp),
                6 => Order(fav, s => s.LastStatus ?? "", cmp),
                7 => _sortAscending ? fav.ThenBy(s => s.LastLatency ?? long.MaxValue) : fav.ThenByDescending(s => s.LastLatency ?? -1),
                8 => _sortAscending ? fav.ThenBy(s => s.LastCheck ?? DateTime.MaxValue) : fav.ThenByDescending(s => s.LastCheck ?? DateTime.MinValue),
                _ => Order(fav, s => s.Name ?? "", cmp)
            };

            return sorted.ThenBy(s => s.Name, cmp);
        }

        private IOrderedEnumerable<ServerEntry> Order(IOrderedEnumerable<ServerEntry> source, Func<ServerEntry, string> key, IComparer<string> cmp) =>
            _sortAscending ? source.ThenBy(key, cmp) : source.ThenByDescending(key, cmp);

        /// <summary>
        /// Sort key so that 10.0.0.2 comes before 10.0.0.10.
        /// </summary>
        private static string IpSortKey(string text)
        {
            if (IPAddress.TryParse((text ?? "").Trim(), out IPAddress ip))
            {
                byte[] bytes = ip.GetAddressBytes();
                return (bytes.Length == 4 ? "4" : "6") + Convert.ToHexString(bytes);
            }

            return "z" + text;
        }

        private ListViewItem CreateItem(ServerEntry s)
        {
            var item = new ListViewItem(new[]
            {
                (s.Favorite ? "★ " : "") + s.Name,
                s.Host,
                s.IPv4,
                s.IPv6,
                s.Port.ToString(CultureInfo.InvariantCulture),
                s.Group,
                s.LastStatus,
                s.LastLatency.HasValue ? s.LastLatency.Value + " ms" : "",
                s.LastCheck?.ToString("g", CultureInfo.CurrentCulture) ?? ""
            })
            {
                Tag = s,
                UseItemStyleForSubItems = false,
                ToolTipText = string.IsNullOrWhiteSpace(s.Notes) ? s.PreferredAddress : s.Notes
            };

            if (s.Favorite)
            {
                _boldListFont ??= new Font(listServers.Font, FontStyle.Bold);
                item.Font = _boldListFont;
            }

            ApplyStatusColor(item, s.LastStatus);
            return item;
        }

        private static void ApplyStatusColor(ListViewItem item, string status)
        {
            Color color = status switch
            {
                null or "" => SystemColors.WindowText,
                CheckingText => SystemColors.GrayText,
                _ when status.StartsWith("Online", StringComparison.Ordinal) => _listOk,
                "Ping only" => _listWarn,
                _ => _listError
            };
            item.SubItems[6].ForeColor = color;
            item.SubItems[7].ForeColor = color;
        }

        private void UpdateItem(ServerEntry s)
        {
            if (!_itemsById.TryGetValue(s.Id, out ListViewItem item) || item.ListView == null)
            {
                return;
            }

            item.SubItems[6].Text = s.LastStatus;
            item.SubItems[7].Text = s.LastLatency.HasValue ? s.LastLatency.Value + " ms" : "";
            item.SubItems[8].Text = s.LastCheck?.ToString("g", CultureInfo.CurrentCulture) ?? "";
            ApplyStatusColor(item, s.LastStatus);
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            // Debounce: filter after the user stopped typing.
            timerSearch.Stop();
            timerSearch.Start();
        }

        private void TimerSearch_Tick(object sender, EventArgs e)
        {
            timerSearch.Stop();
            RefreshServerList();
        }

        private void TextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && textBoxSearch.TextLength > 0)
            {
                textBoxSearch.Clear();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down && listServers.Items.Count > 0)
            {
                listServers.Focus();
                listServers.Items[0].Selected = true;
                listServers.Items[0].Focused = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                timerSearch.Stop();
                RefreshServerList();
                if (listServers.Items.Count == 1)
                {
                    listServers.Items[0].Selected = true;
                    UseAsTarget(SelectedEntry);
                }

                e.SuppressKeyPress = true;
            }
        }

        private void ComboGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updatingGroups)
            {
                RefreshServerList();
            }
        }

        private void ListServers_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_sortColumn == e.Column)
            {
                _sortAscending = !_sortAscending;
            }
            else
            {
                _sortColumn = e.Column;
                _sortAscending = true;
            }

            for (int i = 0; i < listServers.Columns.Count; i++)
            {
                string text = listServers.Columns[i].Text.TrimEnd(' ', '▲', '▼');
                listServers.Columns[i].Text = i == _sortColumn ? text + (_sortAscending ? " ▲" : " ▼") : text;
            }

            RefreshServerList();
        }

        private void ListServers_SelectedIndexChanged(object sender, EventArgs e) => UpdateSelectionButtons();

        private void UpdateSelectionButtons()
        {
            int count = listServers.SelectedItems.Count;
            BtnEdit.Enabled = count == 1;
            BtnDelete.Enabled = count > 0;
        }

        private void ListServers_DoubleClick(object sender, EventArgs e)
        {
            ServerEntry entry = SelectedEntry;
            if (entry != null)
            {
                UseAsTarget(entry);
                BtnPing_Click(sender, e);
            }
        }

        private void ListServers_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    BtnDelete_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.F2:
                    BtnEdit_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    ListServers_DoubleClick(sender, e);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.C when e.Control:
                    MenuCopyAll_Click(sender, e);
                    e.Handled = true;
                    break;
                case Keys.A when e.Control:
                    listServers.BeginUpdate();
                    foreach (ListViewItem item in listServers.Items)
                    {
                        item.Selected = true;
                    }

                    listServers.EndUpdate();
                    e.Handled = true;
                    break;
            }
        }

        private void UseAsTarget(ServerEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            ReadSettingsFromUi();
            comboTarget.Text = FormatTarget(EntryAddress(entry), entry.Port);
            SetState($"Target: {entry.Name}");
        }

        #endregion

        #region Address book – add, edit, delete, import, export

        private ServerEntry FindDuplicate(ServerEntry candidate, ServerEntry ignore = null)
        {
            string Key(string x) => (x ?? "").Trim().ToLowerInvariant();

            return _data.Servers.FirstOrDefault(s => s != ignore && s.Port == candidate.Port &&
                ((Key(s.Host).Length > 0 && Key(s.Host) == Key(candidate.Host)) ||
                 (Key(s.IPv4).Length > 0 && Key(s.IPv4) == Key(candidate.IPv4)) ||
                 (Key(s.IPv6).Length > 0 && Key(s.IPv6) == Key(candidate.IPv6))));
        }

        private void AfterServersChanged(ServerEntry select = null)
        {
            SaveData();
            RefreshGroups();
            RefreshHistory();
            if (select != null)
            {
                listServers.SelectedItems.Clear();
            }

            RefreshServerList();

            if (select != null && _itemsById.TryGetValue(select.Id, out ListViewItem item))
            {
                item.Selected = true;
                item.Focused = true;
                item.EnsureVisible();
            }
        }

        private bool EditEntry(ServerEntry entry, bool isNew)
        {
            using var dialog = new ServerEntryDialog(entry, _data.Servers.Select(s => s.Group), isNew);
            return dialog.ShowDialog(this) == DialogResult.OK;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            ReadSettingsFromUi();
            var entry = new ServerEntry { Port = _data.Settings.DefaultPort };
            if (comboGroup.SelectedIndex > 0)
            {
                entry.Group = comboGroup.SelectedItem?.ToString() ?? "";
            }

            if (EditEntry(entry, true))
            {
                AddEntry(entry);
            }
        }

        private void AddEntry(ServerEntry entry)
        {
            ServerEntry duplicate = FindDuplicate(entry);
            if (duplicate != null &&
                MessageBox.Show(this, $"\"{duplicate.Name}\" has the same address and port.\n\nSave anyway?", "AdminTool",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _data.Servers.Add(entry);
            AfterServersChanged(entry);
            LogOk($"Saved \"{entry.Name}\" ({entry.PreferredAddress}:{entry.Port}).");
        }

        private async void BtnAddFromTarget_Click(object sender, EventArgs e)
        {
            if (!TryGetTarget(out string host, out int port))
            {
                return;
            }

            var entry = new ServerEntry { Name = host, Port = port };
            if (NetworkTools.IsIPv4(host))
            {
                entry.IPv4 = host;
            }
            else if (NetworkTools.IsIPv6(host))
            {
                entry.IPv6 = host;
            }
            else
            {
                entry.Host = host;
                // Fill both addresses so the entry also works when DNS is not available.
                try
                {
                    BtnAddFromTarget.Enabled = false;
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    IPAddress[] all = await NetworkTools.ResolveAllAsync(host, cts.Token);
                    entry.IPv4 = all.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "";
                    entry.IPv6 = all.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6)?.ToString() ?? "";
                }
                catch (Exception ex)
                {
                    LogWarn($"Could not resolve {host}: {ex.GetBaseException().Message}");
                }
                finally
                {
                    BtnAddFromTarget.Enabled = true;
                }
            }

            if (comboGroup.SelectedIndex > 0)
            {
                entry.Group = comboGroup.SelectedItem?.ToString() ?? "";
            }

            if (EditEntry(entry, true))
            {
                AddEntry(entry);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            ServerEntry original = SelectedEntry;
            if (original == null)
            {
                return;
            }

            ServerEntry copy = original.Clone();
            if (!EditEntry(copy, false))
            {
                return;
            }

            int index = _data.Servers.IndexOf(original);
            if (index >= 0)
            {
                _data.Servers[index] = copy;
            }

            AfterServersChanged(copy);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            List<ServerEntry> selected = SelectedEntries;
            if (selected.Count == 0)
            {
                return;
            }

            string question = selected.Count == 1
                ? $"Delete \"{selected[0].Name}\"?"
                : $"Delete {selected.Count} servers?";
            if (MessageBox.Show(this, question, "AdminTool", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            foreach (ServerEntry s in selected)
            {
                _data.Servers.Remove(s);
            }

            AfterServersChanged();
            LogMuted($"{selected.Count} server(s) deleted.");
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Server lists (*.json;*.csv)|*.json;*.csv|JSON (*.json)|*.json|CSV (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Import servers"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                List<ServerEntry> imported = dialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                    ? AdminToolStore.ImportCsv(dialog.FileName)
                    : AdminToolStore.ImportJson(dialog.FileName);

                int added = 0, skipped = 0;
                foreach (ServerEntry entry in imported)
                {
                    if (string.IsNullOrWhiteSpace(entry.PreferredAddress) || FindDuplicate(entry) != null)
                    {
                        skipped++;
                        continue;
                    }

                    entry.Id = Guid.NewGuid().ToString("N");
                    if (entry.Port is <= 0 or > 65535)
                    {
                        entry.Port = _data.Settings.DefaultPort;
                    }

                    if (string.IsNullOrWhiteSpace(entry.Name))
                    {
                        entry.Name = entry.PreferredAddress;
                    }

                    _data.Servers.Add(entry);
                    added++;
                }

                AfterServersChanged();
                LogOk($"Import: {added} server(s) added" + (skipped > 0 ? $", {skipped} skipped (duplicate or no address)." : "."));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Import failed: " + ex.Message, "AdminTool", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            List<ServerEntry> servers = listServers.SelectedItems.Count > 1 ? SelectedEntries : _data.Servers;
            if (servers.Count == 0)
            {
                SetState("Nothing to export.");
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "JSON (*.json)|*.json|CSV for Excel (*.csv)|*.csv",
                FileName = $"servers_{DateTime.Now:yyyyMMdd}",
                Title = servers == _data.Servers ? "Export all servers" : $"Export {servers.Count} selected servers"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                if (dialog.FilterIndex == 2 || dialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    AdminToolStore.ExportCsv(dialog.FileName, servers);
                }
                else
                {
                    AdminToolStore.ExportJson(dialog.FileName, servers);
                }

                LogOk($"{servers.Count} server(s) exported to {dialog.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Export failed: " + ex.Message, "AdminTool", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Address book – status check

        private async void BtnCheckAll_Click(object sender, EventArgs e)
        {
            List<ServerEntry> visible = listServers.Items.Cast<ListViewItem>().Select(i => i.Tag).OfType<ServerEntry>().ToList();
            if (visible.Count == 0)
            {
                SetState("No servers to check.");
                return;
            }

            await CheckServersAsync(visible);
        }

        private async Task CheckServersAsync(List<ServerEntry> servers)
        {
            await RunAsync(servers.Count == 1 ? $"Check {servers[0].Name}" : "Check servers", async ct =>
            {
                LogHeader($"Checking {servers.Count} server(s) – ping + TCP port");
                int done = 0, online = 0;
                AddressPreference family = Family;
                int timeout = _data.Settings.TimeoutMs;
                using var gate = new SemaphoreSlim(Math.Clamp(_data.Settings.Parallelism, 1, 128));

                foreach (ServerEntry s in servers)
                {
                    s.LastStatus = CheckingText;
                    UpdateItem(s);
                }

                try
                {
                    IEnumerable<Task> tasks = servers.Select(async s =>
                    {
                        await gate.WaitAsync(ct);
                        try
                        {
                            await CheckServerAsync(s, family, timeout, ct);
                            if (s.LastStatus.StartsWith("Online", StringComparison.Ordinal))
                            {
                                online++;
                            }
                        }
                        finally
                        {
                            gate.Release();
                            done++;
                            SetProgress(done, servers.Count);
                            UpdateItem(s);
                        }
                    });

                    await Task.WhenAll(tasks);
                }
                finally
                {
                    foreach (ServerEntry s in servers.Where(x => x.LastStatus == CheckingText))
                    {
                        s.LastStatus = "";
                        UpdateItem(s);
                    }

                    SaveData();
                }

                string summary = $"{online} of {servers.Count} server(s) online";
                if (online == servers.Count)
                {
                    LogOk(summary);
                }
                else
                {
                    LogWarn(summary);
                }
            });
        }

        private async Task CheckServerAsync(ServerEntry s, AddressPreference family, int timeout, CancellationToken ct)
        {
            var candidates = new List<IPAddress>();
            if (!string.IsNullOrWhiteSpace(s.Host))
            {
                try
                {
                    IPAddress[] resolved = await NetworkTools.ResolveAllAsync(s.Host.Trim(), ct);
                    candidates.AddRange(resolved);

                    // Keep the saved addresses up to date when they are still empty.
                    if (string.IsNullOrWhiteSpace(s.IPv4))
                    {
                        s.IPv4 = resolved.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "";
                    }

                    if (string.IsNullOrWhiteSpace(s.IPv6))
                    {
                        s.IPv6 = resolved.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6)?.ToString() ?? "";
                    }
                }
                catch (SocketException)
                {
                    // fall back to the saved IPs below
                }
            }

            if (IPAddress.TryParse(s.IPv4?.Trim(), out IPAddress v4))
            {
                candidates.Add(v4);
            }

            if (IPAddress.TryParse(s.IPv6?.Trim(), out IPAddress v6))
            {
                candidates.Add(v6);
            }

            IPAddress ip = NetworkTools.PickAddress(candidates, family);
            if (ip == null)
            {
                s.LastStatus = candidates.Count == 0 ? "DNS error" : $"No {family}";
                s.LastLatency = null;
                s.LastCheck = DateTime.Now;
                LogError($"{s.Name}: {s.LastStatus}");
                return;
            }

            Task<long?> ping = SafePingAsync(ip, timeout);
            Task<TcpCheckResult> tcp = NetworkTools.TcpConnectAsync(ip, s.Port, timeout, ct);
            await Task.WhenAll(ping, tcp);

            long? rtt = ping.Result;
            bool open = tcp.Result.Open;

            s.LastStatus = open ? (rtt.HasValue ? "Online" : "Online (TCP)") : rtt.HasValue ? "Ping only" : "Offline";
            s.LastLatency = rtt ?? (open ? tcp.Result.Milliseconds : (long?)null);
            s.LastCheck = DateTime.Now;

            string line = $"{s.Name,-24} {FormatEndpoint(ip, s.Port),-28} {s.LastStatus,-13}" +
                          (s.LastLatency.HasValue ? $" {s.LastLatency} ms" : "");
            if (open)
            {
                LogOk(line);
            }
            else if (rtt.HasValue)
            {
                LogWarn(line);
            }
            else
            {
                LogError(line);
            }
        }

        #endregion

        #region Context menu

        private void ContextServers_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int count = listServers.SelectedItems.Count;
            if (count == 0)
            {
                e.Cancel = true;
                return;
            }

            ServerEntry s = SelectedEntry;
            bool single = count == 1;
            menuUseTarget.Enabled = single;
            menuPing.Enabled = single && !_busy;
            menuPortCheck.Enabled = single && !_busy;
            menuTracert.Enabled = single && !_busy;
            menuUoStatus.Enabled = single && !_busy;
            menuCheck.Enabled = !_busy;
            menuCheck.Text = single ? "Check status" : $"Check {count} servers";
            menuEdit.Enabled = single;
            menuCopyHost.Enabled = single && !string.IsNullOrWhiteSpace(s.Host);
            menuCopyIPv4.Enabled = single && !string.IsNullOrWhiteSpace(s.IPv4);
            menuCopyIPv6.Enabled = single && !string.IsNullOrWhiteSpace(s.IPv6);
            menuFavorite.Text = single && s.Favorite ? "Remove from favorites" : "Mark as favorite";
            menuDelete.Text = single ? "Delete" : $"Delete {count} servers";
        }

        private void MenuUseTarget_Click(object sender, EventArgs e)
        {
            UseAsTarget(SelectedEntry);
            comboTarget.Focus();
        }

        private void MenuPing_Click(object sender, EventArgs e)
        {
            UseAsTarget(SelectedEntry);
            BtnPing_Click(sender, e);
        }

        private void MenuPortCheck_Click(object sender, EventArgs e)
        {
            UseAsTarget(SelectedEntry);
            BtnPortCheck_Click(sender, e);
        }

        private void MenuTracert_Click(object sender, EventArgs e)
        {
            UseAsTarget(SelectedEntry);
            BtnTracert_Click(sender, e);
        }

        private void MenuUoStatus_Click(object sender, EventArgs e)
        {
            UseAsTarget(SelectedEntry);
            BtnUoStatus_Click(sender, e);
        }

        private async void MenuCheck_Click(object sender, EventArgs e)
        {
            List<ServerEntry> selected = SelectedEntries;
            if (selected.Count > 0)
            {
                await CheckServersAsync(selected);
            }
        }

        private void CopyToClipboard(string text, string what)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                Clipboard.SetText(text.Trim());
                SetState($"{what} copied: {text.Trim()}");
            }
        }

        private void MenuCopyHost_Click(object sender, EventArgs e) => CopyToClipboard(SelectedEntry?.Host, "Host");

        private void MenuCopyIPv4_Click(object sender, EventArgs e) => CopyToClipboard(SelectedEntry?.IPv4, "IPv4");

        private void MenuCopyIPv6_Click(object sender, EventArgs e) => CopyToClipboard(SelectedEntry?.IPv6, "IPv6");

        private void MenuCopyAll_Click(object sender, EventArgs e)
        {
            List<ServerEntry> selected = SelectedEntries;
            if (selected.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Name\tHost\tIPv4\tIPv6\tPort\tGroup\tNotes");
            foreach (ServerEntry s in selected)
            {
                sb.AppendLine(string.Join("\t", s.Name, s.Host, s.IPv4, s.IPv6, s.Port, s.Group, (s.Notes ?? "").Replace("\r", " ").Replace("\n", " ")));
            }

            Clipboard.SetText(sb.ToString());
            SetState($"{selected.Count} server(s) copied as table (paste into Excel)");
        }

        private void MenuFavorite_Click(object sender, EventArgs e)
        {
            List<ServerEntry> selected = SelectedEntries;
            if (selected.Count == 0)
            {
                return;
            }

            bool value = !selected[0].Favorite;
            foreach (ServerEntry s in selected)
            {
                s.Favorite = value;
            }

            AfterServersChanged(selected[0]);
        }

        private void MenuEdit_Click(object sender, EventArgs e) => BtnEdit_Click(sender, e);

        private void MenuDelete_Click(object sender, EventArgs e) => BtnDelete_Click(sender, e);

        #endregion
    }
}
