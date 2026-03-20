// /***************************************************************************
//  * MulAnimDiagnostics — Diagnose-Tool für MUL-Animationen
//  * Zeigt für einen Body GENAU was passiert: BodyTable, BodyConverter,
//  * IDX-Einträge (alle 3 Felder), ob Daten lesbar sind.
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
        // ── Ergebnis für einen einzelnen Body ────────────────────────────────

        public sealed class BodyDiagResult
        {
            public int OriginalBody;
            public int BodyTableRemapped;   // -1 = kein Eintrag
            public int BodyConverterBody;   // -1 = kein Remapping
            public int BodyConverterFile;   // -1 = kein Remapping
            public int FinalBody;           // der tatsächlich verwendete Body
            public int FinalFileType;       // der tatsächlich verwendete FileType
            public int AnimLength;          // 13/22/35
            public string AnimType;          // L/H/P

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
            public int EntryIndex;       // body * 110 * 5 + action * 5 + dir
            public int RawOffset;
            public int RawLength;
            public int RawExtra;
            public int ActualLength;     // resolved
            public bool IsEmpty;          // offset == -1
            public bool IsValid;          // hat Daten
            public bool DataReadable;     // konnte gelesen werden
            public int FrameCount;       // aus den Rohdaten
            public string FileUsed;         // welche anim*.mul
            public string Error;
        }

        // ── Action-Namen ──────────────────────────────────────────────────────

        private static readonly string[] NamesAnimal = { "Walk", "Run", "Stand", "Eat", "Alert", "Attack1", "Attack2", "GetHit", "Die1", "Fidget1", "Fidget2", "LieDown", "Die2" };
        private static readonly string[] NamesMonster = { "Walk", "Stand", "Die1", "Die2", "Attack1", "Attack2", "Attack3", "AttackBow", "AttackCrossBow", "AttackThrow", "GetHit", "Pillage", "Stomp", "Cast2", "Cast3", "BlockRight", "BlockLeft", "Idle", "Fidget", "Fly", "TakeOff", "GetHitInAir" };
        private static readonly string[] NamesHuman = { "Walk_01", "WalkStaff_01", "Run_01", "RunStaff_01", "Idle_01", "Idle_01v", "Fidget", "CombatIdle1H", "CombatIdle1Hv", "AttackSlash1H", "AttackPierce1H", "AttackBash1H", "AttackBash2H", "AttackSlash2H", "AttackPierce2H", "CombatAdv1H", "Spell1", "Spell2", "AttackBow", "AttackCrossbow", "GetHit", "DieFwd", "DieBack", "HorseWalk", "HorseRun", "HorseIdle", "HorseAtk1H", "HorseAtkBow", "HorseAtkXBow", "HorseAtk2H", "BlockShield", "PunchJab", "BowLesser", "Salute", "Ingest" };

        private static string GetActionName(int action, int animLength)
        {
            string[] tbl = animLength == 13 ? NamesAnimal : animLength == 22 ? NamesMonster : NamesHuman;
            return (action >= 0 && action < tbl.Length) ? tbl[action] : $"Action{action}";
        }

        // ── Haupt-Diagnose ────────────────────────────────────────────────────

        public static BodyDiagResult Diagnose(int originalBody, int loadedFileType)
        {
            var result = new BodyDiagResult { OriginalBody = originalBody };

            // Schritt 1: BodyTable
            int btBody = originalBody;
            result.BodyTableRemapped = -1;
            if (BodyTable.Entries != null
                && BodyTable.Entries.TryGetValue(originalBody, out BodyTableEntry btEntry)
                && !BodyConverter.Contains(originalBody))
            {
                btBody = btEntry.OldId;
                result.BodyTableRemapped = btBody;
            }

            // Schritt 2: BodyConverter
            int bcBody = btBody;
            int bcFt = BodyConverter.Convert(ref bcBody);
            result.BodyConverterBody = (bcFt != loadedFileType || bcBody != btBody) ? bcBody : -1;
            result.BodyConverterFile = (bcFt >= 1 && bcFt <= 5) ? bcFt : -1;

            // Finaler Body/FileType
            result.FinalBody = bcBody;
            result.FinalFileType = (bcFt >= 1 && bcFt <= 5) ? bcFt : loadedFileType;

            // AnimLength
            result.AnimLength = Animations.GetAnimLength(result.FinalBody, result.FinalFileType);
            if (result.AnimLength <= 0)
                result.AnimLength = Animations.GetAnimLength(originalBody, loadedFileType);
            if (result.AnimLength <= 0) result.AnimLength = 22; // Fallback Monster

            result.AnimType = result.AnimLength == 13 ? "L (Animal)"
                            : result.AnimLength == 22 ? "H (Monster)"
                            : "P (Human/Equipment)";

            // Schritt 3: Alle Actions + alle Dirs prüfen
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
                        result.FinalFileType);
                    adr.Dirs.Add(ddr);
                    if (ddr.IsValid) adr.AnyValid = true;
                }

                result.Actions.Add(adr);
            }

            // Zusammenfassung
            int validActions = 0;
            foreach (var a in result.Actions) if (a.AnyValid) validActions++;
            result.Summary = $"Body {originalBody} → Final Body {result.FinalBody} in anim{result.FinalFileType}.mul | "
                           + $"{validActions}/{result.AnimLength} Actions haben Daten";

            return result;
        }

        // ── Einzelner IDX-Eintrag ─────────────────────────────────────────────

        private static DirDiagResult DiagnoseIdxEntry(int body, int action, int dir, int fileType)
        {
            var r = new DirDiagResult { Dir = dir };
            r.EntryIndex = body * 110 * 5 + action * 5 + dir;

            string mulName = fileType == 1 ? "anim.mul" : $"anim{fileType}.mul";
            string idxName = fileType == 1 ? "anim.idx" : $"anim{fileType}.idx";
            r.FileUsed = mulName;

            string mulPath = Files.GetFilePath(mulName);
            string idxPath = Files.GetFilePath(idxName);

            if (string.IsNullOrEmpty(idxPath) || !File.Exists(idxPath))
            { r.Error = "IDX-Datei nicht gefunden"; return r; }

            if (string.IsNullOrEmpty(mulPath) || !File.Exists(mulPath))
            { r.Error = "MUL-Datei nicht gefunden"; return r; }

            try
            {
                long idxPos = (long)r.EntryIndex * 12;
                using var fs = new FileStream(idxPath,
                    FileMode.Open, FileAccess.Read, FileShare.Read);

                if (idxPos + 12 > fs.Length)
                { r.Error = $"Eintrag {r.EntryIndex} außerhalb ({fs.Length / 12} Einträge)"; return r; }

                fs.Seek(idxPos, SeekOrigin.Begin);
                using var br = new BinaryReader(fs);
                r.RawOffset = br.ReadInt32();
                r.RawLength = br.ReadInt32();
                r.RawExtra = br.ReadInt32();

                if (r.RawOffset == -1 || r.RawOffset < 0)
                { r.IsEmpty = true; return r; }

                r.ActualLength = r.RawLength > 0 ? r.RawLength
                               : r.RawExtra > 0 ? r.RawExtra
                               : 0;

                if (r.ActualLength <= 0)
                { r.IsEmpty = true; return r; }

                r.IsValid = true;

                // Rohdaten testweise lesen + FrameCount prüfen
                using var mulFs = new FileStream(mulPath,
                    FileMode.Open, FileAccess.Read, FileShare.Read);

                if (r.RawOffset + r.ActualLength > mulFs.Length)
                { r.Error = $"Offset+Länge ({r.RawOffset}+{r.ActualLength}) > MUL-Größe ({mulFs.Length})"; return r; }

                mulFs.Seek(r.RawOffset, SeekOrigin.Begin);
                int readLen = Math.Min(r.ActualLength, 520); // nur Anfang für FrameCount
                var buf = new byte[readLen];
                int read = mulFs.Read(buf, 0, readLen);

                r.DataReadable = (read >= 514);
                if (r.DataReadable)
                    r.FrameCount = buf[512] | (buf[513] << 8);
            }
            catch (Exception ex)
            { r.Error = ex.Message; }

            return r;
        }

        // ── Vergleichs-Diagnose: Zwei Bodies nebeneinander ───────────────────

        /// <summary>
        /// Vergleicht zwei Bodies und zeigt wo sie sich unterscheiden.
        /// Ideal um "Body X funktioniert, Body Y nicht" zu analysieren.
        /// </summary>
        public static void ShowComparisonDialog(
            int workingBody, int brokenBody, int fileType, IWin32Window owner = null)
        {
            var working = Diagnose(workingBody, fileType);
            var broken = Diagnose(brokenBody, fileType);

            var sb = new StringBuilder();
            sb.AppendLine("════════════════════════════════════════════════════════════");
            sb.AppendLine($"  VERGLEICH: Body {workingBody} (OK) vs Body {brokenBody} (PROBLEM)");
            sb.AppendLine("════════════════════════════════════════════════════════════");
            sb.AppendLine();

            void AppendBody(BodyDiagResult r, string label)
            {
                sb.AppendLine($"── {label}: Body {r.OriginalBody} ──────────────────────────────");
                sb.AppendLine($"  BodyTable  → {(r.BodyTableRemapped >= 0 ? r.BodyTableRemapped.ToString() : "kein Remapping")}");
                sb.AppendLine($"  BodyConv   → Body {(r.BodyConverterBody < 0 ? r.OriginalBody.ToString() : r.BodyConverterBody.ToString())}  File anim{r.FinalFileType}.mul");
                sb.AppendLine($"  FinalBody  = {r.FinalBody}  FileType = {r.FinalFileType}");
                sb.AppendLine($"  AnimLength = {r.AnimLength}  ({r.AnimType})");
                sb.AppendLine($"  {r.Summary}");
                sb.AppendLine();

                // Actions mit Problemen hervorheben
                foreach (var a in r.Actions)
                {
                    string status = a.AnyValid ? "✓" : "✗ LEER";
                    sb.Append($"  [{status}] Action {a.ActionIndex:D2} {a.ActionName,-20}");

                    if (!a.AnyValid)
                    {
                        // Zeige ersten Dir-Eintrag zur Diagnose
                        var d0 = a.Dirs[0];
                        if (d0.IsEmpty)
                            sb.Append($"  offset={d0.RawOffset}  len={d0.RawLength}  extra={d0.RawExtra}");
                        else if (d0.Error != null)
                            sb.Append($"  FEHLER: {d0.Error}");
                    }
                    else
                    {
                        // Zeige welche Dirs Daten haben
                        var validDirs = new List<string>();
                        foreach (var d in a.Dirs)
                            if (d.IsValid) validDirs.Add($"Dir{d.Dir}({d.FrameCount}fr)");
                        sb.Append($"  {string.Join(" ", validDirs)}");
                    }
                    sb.AppendLine();
                }
            }

            AppendBody(working, $"OK     Body {workingBody}");
            sb.AppendLine();
            AppendBody(broken, $"FEHLER Body {brokenBody}");

            // Unterschiede hervorheben
            sb.AppendLine();
            sb.AppendLine("── UNTERSCHIEDE ──────────────────────────────────────────");
            if (working.FinalFileType != broken.FinalFileType)
                sb.AppendLine($"  ⚠ FileType unterschiedlich: {working.FinalFileType} vs {broken.FinalFileType}");
            if (working.FinalBody == working.OriginalBody && broken.FinalBody != broken.OriginalBody)
                sb.AppendLine($"  ⚠ Body {brokenBody} wird remappt auf {broken.FinalBody} — prüfe BodyConverter/BodyTable");
            if (working.AnimLength != broken.AnimLength)
                sb.AppendLine($"  ⚠ AnimLength unterschiedlich: {working.AnimLength} vs {broken.AnimLength}");

            ShowMonoDialog($"Diagnose: Body {workingBody} vs {brokenBody}", sb.ToString(), owner);
        }

        // ── Einzel-Diagnose Dialog ────────────────────────────────────────────

        public static void ShowSingleDiagDialog(int body, int fileType, IWin32Window owner = null)
        {
            var r = Diagnose(body, fileType);
            var sb = new StringBuilder();

            sb.AppendLine($"═══════════════════════════════════════════");
            sb.AppendLine($"  DIAGNOSE Body {body}  (geladen: anim{fileType}.mul)");
            sb.AppendLine($"═══════════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"  BodyTable Remap  : {(r.BodyTableRemapped >= 0 ? r.BodyTableRemapped.ToString() : "–")}");
            sb.AppendLine($"  BodyConv Remap   : Body → {r.FinalBody}  in anim{r.FinalFileType}.mul");
            sb.AppendLine($"  AnimLength       : {r.AnimLength}  ({r.AnimType})");
            sb.AppendLine($"  {r.Summary}");
            sb.AppendLine();
            sb.AppendLine($"  {"Action",-5} {"Name",-22} {"Dir0",8} {"Dir1",8} {"Dir2",8} {"Dir3",8} {"Dir4",8}");
            sb.AppendLine($"  {new string('─', 70)}");

            foreach (var a in r.Actions)
            {
                string mark = a.AnyValid ? " " : "✗";
                sb.Append($"  {mark} {a.ActionIndex:D2}   {a.ActionName,-22}");
                foreach (var d in a.Dirs)
                {
                    if (d.IsEmpty) sb.Append($"  {"leer",6}");
                    else if (!d.IsValid) sb.Append($"  {"len=0",6}");
                    else if (d.Error != null) sb.Append($"  {"ERR",6}");
                    else sb.Append($"  {d.FrameCount + "fr",6}");
                }
                sb.AppendLine();
            }

            // IDX-Details für Actions ohne Daten
            bool hasProblems = false;
            foreach (var a in r.Actions)
            {
                if (a.AnyValid) continue;
                if (!hasProblems) { sb.AppendLine(); sb.AppendLine("  ── IDX-Details für leere Actions ─────────"); hasProblems = true; }
                sb.AppendLine($"  Action {a.ActionIndex:D2} ({a.ActionName}):");
                foreach (var d in a.Dirs)
                {
                    string detail;
                    if (d.Error != null) detail = $"FEHLER: {d.Error}";
                    else detail = $"offset={d.RawOffset}  len={d.RawLength}  extra={d.RawExtra}  entry={d.EntryIndex}";
                    sb.AppendLine($"    Dir{d.Dir}: {detail}");
                }
            }

            ShowMonoDialog($"Diagnose Body {body} in anim{fileType}.mul", sb.ToString(), owner);
        }

        // ── Massen-Scan: alle Bodies eines FileType ───────────────────────────

        public static void ShowMassScanDialog(int fileType, IWin32Window owner = null)
        {
            int count = Animations.GetAnimCount(fileType);
            if (count <= 0) count = 1000;

            var sb = new StringBuilder();
            sb.AppendLine($"MASSEN-SCAN anim{fileType}.mul — {count} Bodies");
            sb.AppendLine($"Format: Body | FinalBody | FinalFile | ValidActions/Total | Problem");
            sb.AppendLine(new string('─', 80));

            int ok = 0, partial = 0, broken = 0;

            for (int body = 0; body < count; body++)
            {
                var r = Diagnose(body, fileType);
                int totalActions = r.Actions.Count;
                int validActions = 0;
                foreach (var a in r.Actions) if (a.AnyValid) validActions++;

                if (validActions == 0 && totalActions == 0) continue; // skip unknown

                string status;
                if (validActions == totalActions) { status = "OK"; ok++; }
                else if (validActions > 0) { status = "PARTIAL"; partial++; }
                else { status = "BROKEN"; broken++; }

                if (status != "OK") // nur Probleme ausgeben
                {
                    string remap = r.FinalBody != body ? $"→{r.FinalBody}" : "";
                    string file = r.FinalFileType != fileType ? $"(anim{r.FinalFileType}!)" : "";
                    sb.AppendLine(
                        $"Body {body,5} {remap,6} {file,-10} {validActions}/{totalActions,-3} {status}");
                }
            }

            sb.AppendLine(new string('─', 80));
            sb.AppendLine($"OK: {ok}  Partial: {partial}  Broken: {broken}  Total: {ok + partial + broken}");

            ShowMonoDialog($"Massen-Scan anim{fileType}.mul", sb.ToString(), owner);
        }

        // ── UI Helper ────────────────────────────────────────────────────────

        private static void ShowMonoDialog(string title, string content, IWin32Window owner)
        {
            var frm = new Form
            {
                Text = title,
                Size = new Size(860, 640),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterScreen,
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
                Text = "📋 Kopieren",
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(0, 80, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCopy.Click += (s, e) => Clipboard.SetText(content);
            frm.Controls.Add(tb);
            frm.Controls.Add(btnCopy);
            frm.Show(); // nicht-modal damit man gleichzeitig in der App navigieren kann
        }
    }
}