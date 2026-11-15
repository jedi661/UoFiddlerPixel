/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Automatic fixes of the client check and the report export.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.ClientCheck.Classes
{
    public static class CheckFixes
    {
        /// <summary>true while radar colours were changed in memory and not saved</summary>
        public static bool RadarChanged { get; private set; }

        public static bool CanFix(CheckIssue issue) => issue != null && !issue.Fixed && issue.Fix != FixKind.None;

        public static string FixDescription(FixKind kind) => kind switch
        {
            FixKind.RadarItem or FixKind.RadarLand => Loc.T("fix.radar"),
            FixKind.MobTypes => Loc.T("fix.mobtypes"),
            _ => string.Empty
        };

        /// <summary>Applies the fix of an issue. Returns false if it could not be fixed.</summary>
        public static bool Apply(CheckIssue issue, out string error)
        {
            error = null;
            try
            {
                switch (issue.Fix)
                {
                    case FixKind.RadarItem:
                        return FixRadar(issue.Id, false, out error);
                    case FixKind.RadarLand:
                        return FixRadar(issue.Id, true, out error);
                    case FixKind.MobTypes:
                        return FixMobTypes(issue, out error);
                    default:
                        error = Loc.T("msg.nofix");
                        return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool FixRadar(int id, bool land, out string error)
        {
            error = null;
            Bitmap bmp = land ? Art.GetLand(id) : Art.GetStatic(id);
            if (bmp == null)
            {
                error = Loc.T("detail.nopreview");
                return false;
            }

            Color avg = AverageColor(bmp);
            ushort value = (ushort)((avg.R * 31 / 255 << 10) | (avg.G * 31 / 255 << 5) | (avg.B * 31 / 255));
            if (value == 0)
            {
                value = 0x0421; // almost black, but not "no colour"
            }

            int index = land ? id : id + 0x4000;
            if (RadarCol.Colors == null || index >= RadarCol.Colors.Length)
            {
                error = Loc.F("radar.toosmall", id, Math.Max(0, (RadarCol.Colors?.Length ?? 0x4000) - 0x4000 - 1));
                return false;
            }

            if (land)
            {
                RadarCol.SetLandColor(id, value);
            }
            else
            {
                RadarCol.SetItemColor(id, value);
            }

            RadarChanged = true;
            Options.ChangedUltimaClass["RadarColor"] = true;
            return true;
        }

        /// <summary>Average colour of the non transparent pixels</summary>
        public static Color AverageColor(Bitmap source)
        {
            using (var bmp = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(source, 0, 0, source.Width, source.Height);
                }

                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    var pixels = new int[(data.Stride / 4) * bmp.Height];
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                    long r = 0, gr = 0, b = 0, n = 0;
                    foreach (int p in pixels)
                    {
                        Color c = Color.FromArgb(p);
                        if (c.A < 128 || (c.R == 0 && c.G == 0 && c.B == 0))
                        {
                            continue;
                        }

                        r += c.R;
                        gr += c.G;
                        b += c.B;
                        n++;
                    }

                    return n == 0 ? Color.Black : Color.FromArgb((int)(r / n), (int)(gr / n), (int)(b / n));
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
            }
        }

        public static string SaveRadarCol()
        {
            string path = Options.OutputPath;
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                throw new InvalidOperationException(Loc.T("msg.nooutput"));
            }

            string file = Path.Combine(path, "radarcol.mul");
            RadarCol.Save(file);
            RadarChanged = false;
            Options.ChangedUltimaClass["RadarColor"] = false;
            return file;
        }

        private static bool FixMobTypes(CheckIssue issue, out string error)
        {
            error = null;
            string output = Options.OutputPath;
            if (string.IsNullOrEmpty(output) || !Directory.Exists(output))
            {
                error = Loc.T("msg.nooutput");
                return false;
            }

            // the client file is copied into the output folder once, the lines are added there (backup .bak)
            string target = Path.Combine(output, "mobtypes.txt");
            string client = Files.GetFilePath("mobtypes.txt");
            if (!File.Exists(target) && client != null && File.Exists(client))
            {
                File.Copy(client, target);
            }

            ServUOAnimationSetup.WriteIdLine(target, issue.Id, issue.FixData);
            return true;
        }
    }

    public static class CheckReport
    {
        private static IEnumerable<CheckIssue> Ordered(IEnumerable<CheckIssue> issues) =>
            issues.OrderBy(i => i.Severity).ThenBy(i => i.Category).ThenBy(i => i.Id);

        public static void SaveCsv(IEnumerable<CheckIssue> issues, string fileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(";", Loc.T("col.severity"), Loc.T("col.category"), Loc.T("col.id"), Loc.T("col.message"), Loc.T("detail.hint")));
            foreach (CheckIssue i in Ordered(issues))
            {
                sb.AppendLine(string.Join(";", Csv(i.SeverityText), Csv(i.CategoryText), Csv(i.IdText), Csv(i.Message), Csv(i.Hint)));
            }

            File.WriteAllText(fileName, sb.ToString(), new UTF8Encoding(true));
        }

        private static string Csv(string text) => "\"" + (text ?? string.Empty).Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ") + "\"";

        public static void SaveText(IEnumerable<CheckIssue> issues, string fileName)
        {
            List<CheckIssue> list = Ordered(issues).ToList();
            var sb = new StringBuilder();
            sb.AppendLine(Loc.T("report.title"));
            sb.AppendLine($"{Loc.T("report.date")}: {DateTime.Now.ToString("g", CultureInfo.CurrentCulture)}");
            sb.AppendLine($"{Loc.T("report.client")}: {Files.RootDir}");
            sb.AppendLine(Summary(list));
            sb.AppendLine(new string('=', 80));
            foreach (IGrouping<CheckCategory, CheckIssue> group in list.GroupBy(i => i.Category))
            {
                sb.AppendLine();
                sb.AppendLine($"## {Loc.T("cat." + group.Key)} ({group.Count()})");
                foreach (CheckIssue i in group)
                {
                    sb.AppendLine($"[{i.SeverityText}] {i.IdText}: {i.Message}");
                    sb.AppendLine($"    → {i.Hint}");
                }
            }

            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
        }

        public static void SaveHtml(IEnumerable<CheckIssue> issues, string fileName)
        {
            List<CheckIssue> list = Ordered(issues).ToList();
            string E(string s) => WebUtility.HtmlEncode(s ?? string.Empty);
            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\">");
            sb.AppendLine($"<title>{E(Loc.T("report.title"))}</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#222}h1{font-size:22px}" +
                          "h2{font-size:17px;margin-top:28px}table{border-collapse:collapse;width:100%}" +
                          "td,th{border:1px solid #ddd;padding:5px 8px;vertical-align:top;font-size:13px}th{background:#f2f2f2;text-align:left}" +
                          ".Error{color:#b00020;font-weight:bold}.Warning{color:#c66a00;font-weight:bold}.Info{color:#1565c0}" +
                          ".hint{color:#555}.fixed{text-decoration:line-through;color:#888}.sum{margin:8px 0 18px 0}</style></head><body>");
            sb.AppendLine($"<h1>{E(Loc.T("report.title"))}</h1>");
            sb.AppendLine($"<div>{E(Loc.T("report.date"))}: {E(DateTime.Now.ToString("g", CultureInfo.CurrentCulture))}<br>" +
                          $"{E(Loc.T("report.client"))}: {E(Files.RootDir)}</div>");
            sb.AppendLine($"<div class=\"sum\"><b>{E(Summary(list))}</b></div>");
            foreach (IGrouping<CheckCategory, CheckIssue> group in list.GroupBy(i => i.Category))
            {
                sb.AppendLine($"<h2>{E(Loc.T("cat." + group.Key))} ({group.Count()})</h2>");
                sb.AppendLine($"<table><tr><th>{E(Loc.T("col.severity"))}</th><th>{E(Loc.T("col.id"))}</th>" +
                              $"<th>{E(Loc.T("col.message"))}</th><th>{E(Loc.T("detail.hint"))}</th></tr>");
                foreach (CheckIssue i in group)
                {
                    string cls = i.Fixed ? " class=\"fixed\"" : string.Empty;
                    sb.AppendLine($"<tr{cls}><td class=\"{i.Severity}\">{E(i.SeverityText)}</td><td>{E(i.IdText)}</td>" +
                                  $"<td>{E(i.Message)}</td><td class=\"hint\">{E(i.Hint)}</td></tr>");
                }

                sb.AppendLine("</table>");
            }

            sb.AppendLine("</body></html>");
            File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
        }

        public static string Summary(IEnumerable<CheckIssue> issues)
        {
            List<CheckIssue> list = issues.ToList();
            return Loc.F("summary",
                list.Count(i => i.Severity == CheckSeverity.Error),
                list.Count(i => i.Severity == CheckSeverity.Warning),
                list.Count(i => i.Severity == CheckSeverity.Info));
        }
    }
}
