// /***************************************************************************
//  * MulAnimDiagnostics — Diagnostic tool for MUL animations
//  * Shows EXACTLY what happens to a body: BodyTable, BodyConverter,
//  * IDX entries (all 3 fields) to check if data is readable.
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Controls.Forms
{
    public static class MulAnimDiagnostics
    {
        // ── Result types ──────────────────────────────────────────────────────

        public sealed class BodyDiagResult
        {
            public int OriginalBody;
            public int BodyTableRemapped;  // -1 = no entry
            public int BodyConverterBody;  // -1 = no remapping
            public int BodyConverterFile;  // -1 = no remapping
            public int FinalBody;
            public int FinalFileType;
            public int AnimLength;
            public string AnimType;
            public List<ActionDiagResult> Actions = new();
            public string Summary;
        }

        public sealed class ActionDiagResult
        {
            public int ActionIndex;
            public string ActionName;
            public List<DirDiagResult> Dirs = new();
            public bool AnyValid;
        }

        public sealed class DirDiagResult
        {
            public int Dir;
            public int EntryIndex;
            public int RawOffset;
            public int RawLength;
            public int RawExtra;
            public int ActualLength;
            public bool IsEmpty;
            public bool IsValid;
            public bool DataReadable;
            public int FrameCount;
            public string FileUsed;
            public string Error;
        }

        // ── Action name tables ────────────────────────────────────────────────

        private static readonly string[] NamesAnimal = { "Walk", "Run", "Stand", "Eat", "Alert", "Attack1", "Attack2", "GetHit", "Die1", "Fidget1", "Fidget2", "LieDown", "Die2" };
        private static readonly string[] NamesMonster = { "Walk", "Stand", "Die1", "Die2", "Attack1", "Attack2", "Attack3", "AttackBow", "AttackCrossBow", "AttackThrow", "GetHit", "Pillage", "Stomp", "Cast2", "Cast3", "BlockRight", "BlockLeft", "Idle", "Fidget", "Fly", "TakeOff", "GetHitInAir" };
        private static readonly string[] NamesHuman = { "Walk_01", "WalkStaff_01", "Run_01", "RunStaff_01", "Idle_01", "Idle_01v", "Fidget", "CombatIdle1H", "CombatIdle1Hv", "AttackSlash1H", "AttackPierce1H", "AttackBash1H", "AttackBash2H", "AttackSlash2H", "AttackPierce2H", "CombatAdv1H", "Spell1", "Spell2", "AttackBow", "AttackCrossbow", "GetHit", "DieFwd", "DieBack", "HorseWalk", "HorseRun", "HorseIdle", "HorseAtk1H", "HorseAtkBow", "HorseAtkXBow", "HorseAtk2H", "BlockShield", "PunchJab", "BowLesser", "Salute", "Ingest" };

        private static string GetActionName(int action, int animLength)
        {
            string[] tbl = animLength == 13 ? NamesAnimal : animLength == 22 ? NamesMonster : NamesHuman;
            return (action >= 0 && action < tbl.Length) ? tbl[action] : $"Action{action}";
        }

        // ── IDX cache — avoids repeatedly opening the same file ─────

        private static readonly Dictionary<string, byte[]> _idxCache = new();

        private static byte[] GetIdxData(string idxPath)
        {
            if (_idxCache.TryGetValue(idxPath, out var cached)) return cached;
            try
            {
                var data = File.ReadAllBytes(idxPath);
                _idxCache[idxPath] = data;
                return data;
            }
            catch { return null; }
        }

        public static void ClearCache() => _idxCache.Clear();

        // ── Main diagnosis ────────────────────────────────────────────────────

        public static BodyDiagResult Diagnose(int originalBody, int loadedFileType)
        {
            var result = new BodyDiagResult { OriginalBody = originalBody };

            // Step 1: BodyTable
            int btBody = originalBody;
            result.BodyTableRemapped = -1;
            if (BodyTable.Entries != null
                && BodyTable.Entries.TryGetValue(originalBody, out BodyTableEntry btEntry)
                && !BodyConverter.Contains(originalBody))
            {
                btBody = btEntry.OldId;
                result.BodyTableRemapped = btBody;
            }

            // Step 2: BodyConverter
            int bcBody = btBody;
            int bcFt = BodyConverter.Convert(ref bcBody);

            // Only set BodyConverterBody/-File if remapping is actually required.
            bool wasConverted = (bcBody != btBody || (bcFt >= 1 && bcFt <= 5 && bcFt != loadedFileType));
            result.BodyConverterBody = wasConverted ? bcBody : -1;
            result.BodyConverterFile = (bcFt >= 1 && bcFt <= 5) ? bcFt : -1;

            result.FinalBody = bcBody;
            result.FinalFileType = (bcFt >= 1 && bcFt <= 5) ? bcFt : loadedFileType;

            // Animation Length — multiple fallbacks
            result.AnimLength = Animations.GetAnimLength(result.FinalBody, result.FinalFileType);
            if (result.AnimLength <= 0)
                result.AnimLength = Animations.GetAnimLength(originalBody, loadedFileType);
            if (result.AnimLength <= 0)
            {
                // Last resort: estimating from IDX data
                result.AnimLength = 22;
            }

            result.AnimType = result.AnimLength == 13 ? "L (Animal)"
                            : result.AnimLength == 22 ? "H (Monster)"
                            : "P (Human/Equipment)";

            // Step 3: Check all actions + dirs
            // Load IDX once (cached) instead of per entry
            string idxName = result.FinalFileType == 1 ? "anim.idx" : $"anim{result.FinalFileType}.idx";
            string idxPath = Files.GetFilePath(idxName);
            byte[] idxData = (!string.IsNullOrEmpty(idxPath) && File.Exists(idxPath))
                             ? GetIdxData(idxPath) : null;

            string mulName = result.FinalFileType == 1 ? "anim.mul" : $"anim{result.FinalFileType}.mul";
            string mulPath = Files.GetFilePath(mulName);

            for (int action = 0; action < result.AnimLength; action++)
            {
                var adr = new ActionDiagResult
                {
                    ActionIndex = action,
                    ActionName = GetActionName(action, result.AnimLength)
                };

                for (int dir = 0; dir < 5; dir++)
                {
                    var ddr = DiagnoseIdxEntry(
                        result.FinalBody, action, dir,
                        result.FinalFileType, idxData, mulPath, mulName);
                    adr.Dirs.Add(ddr);
                    if (ddr.IsValid) adr.AnyValid = true;
                }

                result.Actions.Add(adr);
            }

            int validActions = 0;
            foreach (var a in result.Actions) if (a.AnyValid) validActions++;
            result.Summary =
                $"Body {originalBody} → Final Body {result.FinalBody} in anim{result.FinalFileType}.mul" +
                $" | {validActions}/{result.AnimLength} Actions have data";

            return result;
        }

        // ── Single IDX entry — uses cached IDX data ──────────────────────

        private static DirDiagResult DiagnoseIdxEntry(
            int body, int action, int dir, int fileType,
            byte[] idxData, string mulPath, string mulName)
        {
            var r = new DirDiagResult { Dir = dir, FileUsed = mulName };
            r.EntryIndex = body * 110 * 5 + action * 5 + dir;

            if (idxData == null)
            { r.Error = "IDX not loaded"; return r; }

            if (string.IsNullOrEmpty(mulPath) || !File.Exists(mulPath))
            { r.Error = "MUL file not found"; return r; }

            long idxPos = (long)r.EntryIndex * 12;

            if (idxPos + 12 > idxData.Length)
            { r.Error = $"entry {r.EntryIndex} outside ({idxData.Length / 12} entries)"; return r; }

            //Read directly from the byte[] — no FileStream needed
            r.RawOffset = BitConverter.ToInt32(idxData, (int)idxPos);
            r.RawLength = BitConverter.ToInt32(idxData, (int)idxPos + 4);
            r.RawExtra = BitConverter.ToInt32(idxData, (int)idxPos + 8);

            if (r.RawOffset == -1 || r.RawOffset < 0)
            { r.IsEmpty = true; return r; }

            // extra field as fallback length (UO quirk: length=0 → extra contains true length)
            r.ActualLength = r.RawLength > 0 ? r.RawLength
                           : r.RawExtra > 0 ? r.RawExtra
                           : 0;

            if (r.ActualLength <= 0)
            { r.IsEmpty = true; return r; }

            r.IsValid = true;

            // Read only the first 520 bytes to check the frame count.
            try
            {
                using var mulFs = new FileStream(mulPath,
                    FileMode.Open, FileAccess.Read, FileShare.Read);

                if (r.RawOffset + r.ActualLength > mulFs.Length)
                {
                    r.Error = $"Offset+len ({r.RawOffset}+{r.ActualLength}) > MUL ({mulFs.Length})";
                    return r;
                }

                mulFs.Seek(r.RawOffset, SeekOrigin.Begin);
                int readLen = Math.Min(r.ActualLength, 520);
                var buf = new byte[readLen];
                int read = mulFs.Read(buf, 0, readLen);

                // FIX: DataReadable = wirklich genug Bytes gelesen
                r.DataReadable = (read >= 514);
                if (r.DataReadable)
                {
                    r.FrameCount = buf[512] | (buf[513] << 8);
                    // Plausibilitätsprüfung: FrameCount > 0 und sinnvoll
                    if (r.FrameCount == 0)
                        r.Error = "FrameCount=0 (leer oder falsches Format)";
                    else if (r.FrameCount > 2000)
                        r.Error = $"FrameCount={r.FrameCount} (implausibly high)";
                }
            }
            catch (Exception ex)
            { r.Error = ex.Message; }

            return r;
        }

        // ── Single body diagnosis dialog ──────────────────────────────────────

        public static void ShowSingleDiagDialog(int body, int fileType, IWin32Window owner = null)
        {
            var r = Diagnose(body, fileType);
            var sb = new StringBuilder();

            sb.AppendLine($"═══════════════════════════════════════════════════");
            sb.AppendLine($"  DIAGNOSE  Body {body}  (anim{fileType}.mul)");
            sb.AppendLine($"═══════════════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"  BodyTable Remap : {(r.BodyTableRemapped >= 0 ? r.BodyTableRemapped.ToString() : "–")}");
            sb.AppendLine($"  BodyConv Remap  : Body {r.OriginalBody} → {r.FinalBody}  in anim{r.FinalFileType}.mul");
            sb.AppendLine($"  AnimLength      : {r.AnimLength}  ({r.AnimType})");
            sb.AppendLine($"  {r.Summary}");
            sb.AppendLine();
            sb.AppendLine($"  {"#",-4} {"Name",-22} {"D0",6} {"D1",6} {"D2",6} {"D3",6} {"D4",6}  Note");
            sb.AppendLine($"  {new string('─', 72)}");

            foreach (var a in r.Actions)
            {
                string mark = a.AnyValid ? " " : "✗";
                sb.Append($"  {mark} {a.ActionIndex:D2}  {a.ActionName,-22}");
                foreach (var d in a.Dirs)
                {
                    if (d.IsEmpty) sb.Append($"  {"–",4}");
                    else if (!d.IsValid) sb.Append($"  {"len0",4}");
                    else if (d.Error != null && d.FrameCount == 0)
                        sb.Append($"  {"!fc0",4}");
                    else if (d.Error != null) sb.Append($"  {"ERR",4}");
                    else sb.Append($"  {d.FrameCount + "f",4}");
                }

                // Quick note for empty actions
                if (!a.AnyValid)
                {
                    var d0 = a.Dirs[0];
                    if (d0.Error != null)
                        sb.Append($"  ← {d0.Error}");
                    else
                        sb.Append($"  ← off={d0.RawOffset} len={d0.RawLength} ext={d0.RawExtra}");
                }
                sb.AppendLine();
            }

            // IDX detail block only if problems exist
            bool hasProblems = false;
            foreach (var a in r.Actions)
            {
                if (a.AnyValid) continue;
                if (!hasProblems)
                {
                    sb.AppendLine();
                    sb.AppendLine("  ── IDX-Details (empty actions) ───────────────────────");
                    hasProblems = true;
                }
                sb.AppendLine($"  Action {a.ActionIndex:D2} {a.ActionName}:");
                foreach (var d in a.Dirs)
                {
                    string detail = d.Error != null
                        ? $"ERROR: {d.Error}"
                        : $"off={d.RawOffset}  len={d.RawLength}  extra={d.RawExtra}  entry={d.EntryIndex}";
                    sb.AppendLine($"    Dir{d.Dir}: {detail}");
                }
            }

            ShowMonoDialog($"Diagnose Body {body} / anim{fileType}.mul", sb.ToString(), owner);
        }

        // ── Comparison dialog ─────────────────────────────────────────────────

        public static void ShowComparisonDialog(
            int workingBody, int brokenBody, int fileType, IWin32Window owner = null)
        {
            var working = Diagnose(workingBody, fileType);
            var broken = Diagnose(brokenBody, fileType);

            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════════");
            sb.AppendLine($"  COMPARISON:  Body {workingBody} (OK)  vs  Body {brokenBody} (PROBLEM)");
            sb.AppendLine("═══════════════════════════════════════════════════════════════");
            sb.AppendLine();

            void AppendBody(BodyDiagResult r, string label)
            {
                sb.AppendLine($"── {label} ──────────────────────────────────────────────");
                sb.AppendLine($"  BodyTable → {(r.BodyTableRemapped >= 0 ? r.BodyTableRemapped.ToString() : "–")}");
                sb.AppendLine($"  BodyConv  → Body {r.FinalBody}  File anim{r.FinalFileType}.mul");
                sb.AppendLine($"  AnimType  : {r.AnimLength} ({r.AnimType})");
                sb.AppendLine($"  {r.Summary}");
                sb.AppendLine();
                foreach (var a in r.Actions)
                {
                    string status = a.AnyValid ? "✓" : "✗";
                    sb.Append($"  [{status}] {a.ActionIndex:D2} {a.ActionName,-20}");
                    if (!a.AnyValid)
                    {
                        var d0 = a.Dirs[0];
                        if (d0.Error != null) sb.Append($"  ERR: {d0.Error}");
                        else sb.Append($"  off={d0.RawOffset} len={d0.RawLength} ext={d0.RawExtra}");
                    }
                    else
                    {
                        foreach (var d in a.Dirs)
                            if (d.IsValid) sb.Append($" D{d.Dir}({d.FrameCount}f)");
                    }
                    sb.AppendLine();
                }
            }

            AppendBody(working, $"OK     Body {workingBody}");
            sb.AppendLine();
            AppendBody(broken, $"ERROR Body {brokenBody}");
            sb.AppendLine();
            sb.AppendLine("── DIFFERENCES ──────────────────────────────────────────────");

            bool anyDiff = false;
            if (working.FinalFileType != broken.FinalFileType)
            { sb.AppendLine($"  ⚠ FileType: {working.FinalFileType} vs {broken.FinalFileType}"); anyDiff = true; }
            if (working.FinalBody != working.OriginalBody || broken.FinalBody != broken.OriginalBody)
            { sb.AppendLine($"  ⚠ Remapping: {working.OriginalBody}→{working.FinalBody}  vs  {broken.OriginalBody}→{broken.FinalBody}"); anyDiff = true; }
            if (working.AnimLength != broken.AnimLength)
            { sb.AppendLine($"  ⚠ AnimLength: {working.AnimLength} vs {broken.AnimLength}"); anyDiff = true; }
            if (!anyDiff)
                sb.AppendLine("  No structural difference — missing actions are simply not in the IDX..");

            ShowMonoDialog($"Diagnose: {workingBody} vs {brokenBody}", sb.ToString(), owner);
        }

        // ── Mass scan with progress ───────────────────────────────────────────

        public static void ShowMassScanDialog(int fileType, IWin32Window owner = null)
        {
            // Realistic body count instead of arbitrary fallback
            int count = Animations.GetAnimCount(fileType);
            if (count <= 0)
            {
                // Calculating from IDX file size: each entry = 12 bytes, each body = 110*5 = 550 entries
                string ixp = Files.GetFilePath(fileType == 1 ? "anim.idx" : $"anim{fileType}.idx");
                if (!string.IsNullOrEmpty(ixp) && File.Exists(ixp))
                {
                    long idxSize = new FileInfo(ixp).Length;
                    count = (int)(idxSize / 12 / 550);
                }
                if (count <= 0) count = 2048; // Safe fallback
            }

            // IDX vorab cachen
            string idxName = fileType == 1 ? "anim.idx" : $"anim{fileType}.idx";
            string idxPath = Files.GetFilePath(idxName);
            if (!string.IsNullOrEmpty(idxPath) && File.Exists(idxPath))
                GetIdxData(idxPath); // in Cache laden

            var sb = new StringBuilder();
            sb.AppendLine($"MASSEN-SCAN  anim{fileType}.mul  —  {count} Bodies");
            sb.AppendLine($"{"Body",6}  {"→Body",6}  {"File",-10}  {"Valid/Total",-12}  Status");
            sb.AppendLine(new string('─', 56));

            int ok = 0, partial = 0, broken = 0, empty = 0;

            // Progress form
            using var progress = new Form
            {
                Text = $"Scanning anim{fileType}.mul...",
                Size = new Size(400, 100),
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                StartPosition = FormStartPosition.CenterScreen,
                ControlBox = false
            };
            var pb = new ProgressBar { Dock = DockStyle.Fill, Maximum = count, Value = 0 };
            var lbl = new Label { Dock = DockStyle.Bottom, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Height = 24 };
            progress.Controls.Add(pb);
            progress.Controls.Add(lbl);
            progress.Show(owner as Form);

            for (int body = 0; body < count; body++)
            {
                if (body % 50 == 0)
                {
                    pb.Value = Math.Min(body, count);
                    lbl.Text = $"Body {body} / {count}...";
                    System.Windows.Forms.Application.DoEvents();
                }

                var r = Diagnose(body, fileType);
                int total = r.Actions.Count;
                int valid = 0;
                foreach (var a in r.Actions) if (a.AnyValid) valid++;

                if (total == 0) { empty++; continue; }

                string status;
                if (valid == total) { status = "OK"; ok++; }
                else if (valid > 0) { status = "PARTIAL"; partial++; }
                else { status = "BROKEN"; broken++; }

                if (status != "OK")
                {
                    string remap = r.FinalBody != body ? $"→{r.FinalBody}" : "–";
                    string file = r.FinalFileType != fileType ? $"anim{r.FinalFileType}" : "–";
                    sb.AppendLine($"{body,6}  {remap,6}  {file,-10}  {valid}/{total,-10}  {status}");
                }
            }

            progress.Close();

            sb.AppendLine(new string('─', 56));
            sb.AppendLine($"OK: {ok}  Partial: {partial}  Broken: {broken}  Empty: {empty}  Total: {ok + partial + broken + empty}");

            var diagForm = ShowMonoDialog($"Massen-Scan anim{fileType}.mul", sb.ToString(), owner);

            // Export-Button
            diagForm.Controls.Add(new Button
            {
                Text = "💾 Als TXT exportieren",
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(0, 100, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            });
            ((Button)diagForm.Controls[diagForm.Controls.Count - 1]).Click += (s, e) =>
            {
                using var sfd = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt",
                    FileName = $"MassScan_anim{fileType}.txt"
                };
                if (sfd.ShowDialog() == DialogResult.OK)
                    File.WriteAllText(sfd.FileName, sb.ToString());
            };
        }

        // ── UI Helper — returns form for post-processing─────────────────

        private static Form ShowMonoDialog(string title, string content, IWin32Window owner)
        {
            var frm = new Form
            {
                Text = title,
                Size = new Size(860, 640),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,                
                StartPosition = owner != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(28, 28, 32),
                ForeColor = Color.White
            };

            var tb = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(28, 28, 38),
                ForeColor = Color.FromArgb(200, 220, 255),
                Font = new Font("Consolas", 9f),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Text = content
            };

            var btnCopy = new Button
            {
                Text = "📋 On clipboard",
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(0, 80, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCopy.Click += (s, e) => Clipboard.SetText(content);

            frm.Controls.Add(tb);
            frm.Controls.Add(btnCopy);
            
            if (owner != null) frm.Show(owner);
            else frm.Show();

            return frm;
        }
    }
}