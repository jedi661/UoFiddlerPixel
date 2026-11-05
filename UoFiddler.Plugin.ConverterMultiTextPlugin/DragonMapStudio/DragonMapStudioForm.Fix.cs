// /***************************************************************************
//  *
//  * Dragon Map Studio – check / repair the map, log files, help language
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private string _lastLog;

        private void InitHelp()
        {
            cmbHelpLanguage.Items.Clear();
            cmbHelpLanguage.Items.AddRange(new object[] { "Deutsch", "English" });
            cmbHelpLanguage.SelectedIndex = 0;
        }

        private void CmbHelpLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = cmbHelpLanguage.SelectedIndex == 1 ? StudioHelp.TextEn : StudioHelp.TextDe;
            txtHelp.Text = text.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }

        private string LogFolder
        {
            get
            {
                string folder = CurrentBuildOptions().OutputFolder;
                if (string.IsNullOrWhiteSpace(folder))
                {
                    folder = _settings.OutputFolder;
                }
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        private string WriteLog(string fileName, string text)
        {
            string path = Path.Combine(LogFolder, fileName);
            File.WriteAllText(path, text);
            _lastLog = path;
            return path;
        }

        private MapFixOptions FixOptions => _settings.Build.Fix ??= new MapFixOptions();

        /// <summary>Checks the editor map (nothing is changed) and writes map_check_log.txt.</summary>
        private void BtnCheckMap_Click(object sender, EventArgs e)
        {
            if (_map == null)
            {
                MessageBox.Show(this, "Keine Karte geöffnet.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            IndexedMap copy = _map.Clone();
            DragonRuleSet rules = _rules;
            MapFixOptions o = FixOptions.Clone();
            string name = _map.FilePath ?? "Unbenannt";
            RunBusy("Karte prüfen", (progress, token) => MapFixer.Check(copy, rules, o, token), report =>
            {
                string path = WriteLog("map_check_log.txt", BuildLog.CreateCheck(rules, report, name));
                tabMain.SelectedTab = tabBuild;
                Log("=== Karte prüfen / map check ===");
                foreach (MapFixEntry entry in report.Entries.Where(x => x.Count > 0).OrderByDescending(x => x.Count))
                {
                    Log($"{entry.TitleDe}: {entry.Count:N0}  z. B. {string.Join(" ", entry.Samples.Take(5).Select(p => $"({p.X},{p.Y})"))}");
                }
                Log("Log: " + path);
                SetStatus(report.Summary() + "  →  " + path);
            });
        }

        /// <summary>Repairs the editor map (one undo step) and writes map_fix_log.txt.</summary>
        private void BtnFixMap_Click(object sender, EventArgs e)
        {
            if (_map == null)
            {
                MessageBox.Show(this, "Keine Karte geöffnet.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            IndexedMap work = _map.Clone();
            DragonRuleSet rules = _rules;
            MapFixOptions o = FixOptions.Clone();
            string name = _map.FilePath ?? "Unbenannt";
            RunBusy("Karte reparieren", (progress, token) => (work, MapFixer.Fix(work, rules, o, null, token)), r =>
            {
                if (_map == null || _map.Width != r.work.Width || _map.Height != r.work.Height)
                {
                    return;
                }
                _history.Begin("Karte reparieren");
                _history.TouchAll();
                Buffer.BlockCopy(r.work.Pixels, 0, _map.Pixels, 0, _map.Pixels.Length);
                _history.End();
                AfterExternalEdit();
                string path = WriteLog("map_fix_log.txt", BuildLog.CreateCheck(rules, r.Item2, name));
                Log("=== Karte reparieren / map repair ===");
                Log(r.Item2.Summary());
                Log("Log: " + path);
                SetStatus(r.Item2.Summary() + "  (Rückgängig mit Strg+Z)");
            });
        }

        private void BtnOpenLog_Click(object sender, EventArgs e)
        {
            string folder = LogFolder;
            string log = _lastLog != null && File.Exists(_lastLog) ? _lastLog : Path.Combine(folder, "dragon_log.txt");
            try
            {
                if (File.Exists(log))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = log, UseShellExecute = true });
                }
                else
                {
                    OpenFolder(folder);
                }
            }
            catch (Exception)
            {
                OpenFolder(folder);
            }
        }

        // ------------------------------------------------------------------ CGI

        private void BtnCgiRemoveDup_Click(object sender, EventArgs e)
        {
            if (_cgiMap == null)
            {
                return;
            }
            var seen = new HashSet<(int, int, int, ushort, ushort)>();
            var changes = new List<StaticChange>();
            foreach (StaticItem s in _cgiMap.AllStatics().ToList())
            {
                if (!seen.Add((s.X, s.Y, s.Z, s.Id, s.Hue)))
                {
                    changes.Add(MapData.Remove(s));
                }
            }
            if (changes.Count == 0)
            {
                SetStatus("Keine doppelten Statics gefunden.");
                return;
            }
            _cgiMap.Apply(changes);
            isoView.Refresh(true);
            UpdateCgiState();
            SetStatus($"{changes.Count:N0} doppelte Statics entfernt (Rückgängig möglich).");
        }

        /// <summary>Reads heights, coast, statics of the loaded map (e.g. the original map) into map_analysis_log.txt.</summary>
        private void BtnCgiAnalyze_Click(object sender, EventArgs e)
        {
            if (_cgiMap == null)
            {
                MessageBox.Show(this, "Erst eine Karte laden (z. B. die Original-map0.mul mit 'MUL / UOP laden').", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            MapData data = _cgiMap;
            System.Drawing.Rectangle area = isoView.Area;
            RunBusy("Karte analysieren", (progress, token) => MapAnalyzer.Analyze(data, area, data.Source, progress, token), text =>
            {
                string path = WriteLog("map_analysis_log.txt", text);
                SetStatus("Analyse gespeichert: " + path + (area.IsEmpty ? "  (Tipp: mit 'Bereich' ein Stück markieren – dann kommt ein Höhen-Raster dazu)" : string.Empty));
                Log("Analyse: " + path);
            });
        }

        private void BtnCgiSaveLog_Click(object sender, EventArgs e)
        {
            if (_cgiMap == null)
            {
                return;
            }
            string path = WriteLog("cgi_check_log.txt", BuildLog.CreateProblems(_rules, _problems, _cgiMap.Source));
            SetStatus("Log gespeichert: " + path);
        }
    }
}
