/***************************************************************************
 *
 *  * Dragon Map Studio – test assistant
 *  *
 *  *  1. automatic self tests (Core/SelfTests) with the real client files – every value is logged
 *  *  2. a guided check list: what to do, what you should see; ✔ / ✖ / ? with a comment; "📷 Aufnehmen" saves a
 *  *     screenshot of every studio window, the tool with its settings and every static / land tile the tool changed
 *  *  3. errors of the studio (exceptions) are recorded while the window is open
 *  *  4. "Bericht speichern" packs everything into one ZIP for the developer
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class TestHost
    {
        public Func<MapData> Map;
        public CgiBrushSettings Brushes;
        public Action<IsoTool> SelectTool;

        /// <summary>Opens a part of the studio: "bridge", "workshop", "cellar", "tooloptions", "side", "cgi", "editor", "course".</summary>
        public Action<string> Open;

        /// <summary>Current tool with its settings (text).</summary>
        public Func<string> ToolInfo;

        public string BuildMark;
        public Action<string> Status;

        /// <summary>Gives a CGI view the settings of the studio (art, transitions …) – for the pictures of the automatic tests.</summary>
        public Action<IsoMapView> Configure;
    }

    public sealed class TestAssistantForm : Form
    {
        private sealed class Step
        {
            public string Area;
            public string Title;
            public string Todo;
            public string Expect;
            public Action Prepare;
            public string Answer = string.Empty;
            public string Comment = string.Empty;
            public int UndoAtStart = -1;
            public readonly List<string> Captures = new List<string>();
        }

        private readonly TestHost _host;
        private readonly List<Step> _steps = new List<Step>();
        private readonly List<TestResult> _auto = new List<TestResult>();
        private readonly StringBuilder _log = new StringBuilder();
        private readonly Dictionary<string, int> _errors = new Dictionary<string, int>();
        private readonly List<string> _errorText = new List<string>();
        private readonly string _folder;
        private readonly ListBox _list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false, Font = new Font("Segoe UI", 9.5f) };
        private readonly Label _title = new Label { Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 12f, FontStyle.Bold), Padding = new Padding(6, 4, 0, 0) };
        private readonly Label _todo = new Label { Dock = DockStyle.Top, Height = 110, Font = new Font("Segoe UI", 10f), Padding = new Padding(6) };
        private readonly Label _expect = new Label { Dock = DockStyle.Top, Height = 80, Font = new Font("Segoe UI", 10f, FontStyle.Italic), ForeColor = Color.FromArgb(22, 101, 52), Padding = new Padding(6) };
        private readonly TextBox _comment = new TextBox { Dock = DockStyle.Top, Height = 60, Multiline = true, ScrollBars = ScrollBars.Vertical };
        private readonly TextBox _out = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 8.5f) };
        private Step _current;
        private int _shot;

        public TestAssistantForm(TestHost host)
        {
            _host = host;
            Text = L.T("Test-Assistent") + " – Dragon Map Studio";
            Size = new Size(1100, 760);
            StartPosition = FormStartPosition.Manual;
            Location = new Point(Math.Max(0, Screen.PrimaryScreen.WorkingArea.Right - 1120), 40);
            Font = new Font("Segoe UI", 9f);
            _folder = Path.Combine(StudioSettings.SettingsFolder, "Tests", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(_folder);
            BuildSteps();
            BuildUi();
            Log($"Test-Assistent gestartet – {host.BuildMark} – {DateTime.Now:yyyy-MM-dd HH:mm} – Windows {Environment.OSVersion.Version}, .NET {Environment.Version}");
            MapData m = host.Map?.Invoke();
            Log(m == null ? "Keine CGI-Karte geladen." : $"CGI-Karte: {m.Source} ({m.Width}×{m.Height}), {m.StaticCount:N0} Statics");
            AppDomain.CurrentDomain.FirstChanceException += OnException;
            FormClosed += (s, e) => AppDomain.CurrentDomain.FirstChanceException -= OnException;
        }

        // ================================================================== steps of the check list

        private void BuildSteps()
        {
            void S(string area, string title, string todo, string expect, Action prepare = null) =>
                _steps.Add(new Step { Area = area, Title = title, Todo = todo, Expect = expect, Prepare = prepare });
            Action Tool(IsoTool t) => () =>
            {
                _host.Open?.Invoke("cgi");
                _host.SelectTool?.Invoke(t);
            };
            Action Open(string what) => () => _host.Open?.Invoke(what);

            S("Brücke", "Brücken-Designer: Teile",
                "Brücken-Designer öffnen (▶ Vorbereiten). Links nacheinander 'Bretter', 'Geländer W-O', 'Geländer N-S', 'Pfosten', 'Pfeiler' klicken und unten im Katalog schauen.",
                "Im Katalog stehen passende Teile mit Bild. Die automatisch gewählten Teile im Testfeld sehen wie eine Brücke aus. Danach 📷 Aufnehmen.", Open("bridge"));
            S("Brücke", "Brücken-Designer: Regler und Bogen aus Stufen",
                "Im Designer Länge, Breite und Bogen mit den Reglern ändern. 'Bogen aus Stufen' einschalten (vorher Treppe West / Ost zuordnen).",
                "Die lange Vorschau folgt sofort; mit Stufen steigt die Brücke an, ist oben flach und geht wieder hinunter, das Geländer läuft mit. 📷 Aufnehmen.", Open("bridge"));
            S("Brücke", "Brücke in der CGI-Ansicht",
                "In der CGI-Ansicht über Wasser von Ufer zu Ufer ziehen (einmal West-Ost, einmal Nord-Süd). Nach jeder Brücke 📷 Aufnehmen.",
                "Beim Ziehen sieht man die echten Teile, nach dem Loslassen steht die Brücke mit Geländer auf beiden Seiten und Pfeilern.", Tool(IsoTool.Bridge));
            S("Treppe", "Treppe aus Auswahl lernen",
                "Eine Treppe von Hand bauen (Stufen in mehreren Richtungen, Blöcke, Absatz), alle Stücke mit Auswahl markieren, dann im Werkzeug Treppe 'Treppe aus Auswahl lernen'. 📷 Aufnehmen.",
                "Ein Fenster zeigt die erkannten Stufen je Richtung, den Füll-Block und den Absatz.", Tool(IsoTool.Select));
            S("Treppe", "Treppenhaus setzen",
                "Werkzeug Treppe: einmal klicken (1 Etage), dann Etagen 2 mit Wende 'rechts herum', dann 'zurück'. R dreht. Nach jeder Treppe 📷 Aufnehmen.",
                "4 Stufen je Lauf (5 Z), ein flaches Feld, nächster Lauf; alles bis unten gefüllt, Stufen zeigen in die richtige Richtung.", Tool(IsoTool.Stairs));
            S("Drehen", "Drehen 90° tauscht Wände",
                "Mit Wand / Zaun eine Wand West-Ost ziehen, mit 'Bereich' markieren und R (drehen) drücken. 📷 Aufnehmen.",
                "Die Wand liegt jetzt Nord-Süd und benutzt die Nord-Süd-Teile (nicht mehr quer stehende Teile).", Tool(IsoTool.Wall));
            S("Wand", "Wand Punkt zu Punkt mit Dicke und Ecken",
                "Wand-Werkzeug: oben 'Punkt zu Punkt' an, Dicke 2. Vier Punkte im L / U klicken, Doppelklick. 📷 Aufnehmen.",
                "Die Wand läuft von Punkt zu Punkt, 2 Reihen dick, an den Ecken Pfosten oder Eckstück, nichts doppelt.", Tool(IsoTool.Wall));
            S("Fluss", "Fluss-Lauf (CGI)",
                "Fluss-Lauf: Breite am Anfang 2, am Ende 6; einmal mit 'Grund tiefer' 8 und Wasser-Statics. Von der Quelle zur Mündung ziehen. 📷 Aufnehmen.",
                "Wasser durchgehend, wird breiter, weiche Ufer; mit Grund tiefer liegen Wasser-Statics auf dem Spiegel.", Tool(IsoTool.Course));
            S("Küste", "Küste glätten",
                "An einer zackigen Küste mit 'Küste glätten' darüber ziehen (oder Bereich + J). 📷 Aufnehmen vorher und nachher.",
                "Keine Sand-Zacken mehr, die Tiefe bleibt, Statics gehen mit.", Tool(IsoTool.CoastSmooth));
            S("Unebenheit", "Unebenheit mit Mustern",
                "Unebenheit: Muster 'Hügel', dann 'Rillen' (Richtung 30), dann 'Stufen' über eine flache Wiese ziehen. 📷 Aufnehmen.",
                "Das Vorschau-Bild oben passt zum Ergebnis im Gelände.", Tool(IsoTool.Noise));
            S("Varianten", "Varianten-Muster aus Bereich",
                "Ein Stück gemischten Boden mit 'Bereich' markieren, Varianten-Pinsel → 'Neu aus Bereich', dann woanders malen. 📷 Aufnehmen.",
                "Der gemalte Boden mischt die Tiles wie im markierten Stück.", Tool(IsoTool.Variants));
            S("Weg", "Weg mit Stempel",
                "Weg-Werkzeug: oben 'aus Tiles' → einen Stempel wählen. Punkte klicken, Doppelklick. Danach 'Jetzige Einstellungen als Vorlage speichern'. 📷 Aufnehmen.",
                "Der Stempel läuft entlang der Kurve; die Vorlage steht unter 'Weg-Vorlage'.", Tool(IsoTool.Road));
            S("Hue", "Hue wählen und merken",
                "Färben: '🎨 Wählen …' öffnen, eine Hue suchen und wählen, '☆ merken', ein Static färben. 📷 Aufnehmen.",
                "Farbfeld und Name stimmen; die Hue steht unter ★ Favoriten.", Tool(IsoTool.Hue));
            S("Radierer", "Radierer mit IDs",
                "Zwei Statics derselben Art auswählen, Radierer → 'Auswahl merken', über eine gemischte Fläche radieren. 📷 Aufnehmen.",
                "Nur Statics mit den gemerkten IDs werden gelöscht.", Tool(IsoTool.Eraser));
            S("Multi", "Multi-Katalog",
                "Reiter Multis öffnen: das Bild erscheint sofort; im Katalog unten eine Multi wählen, setzen, dann '✖ Auswahl löschen'. 📷 Aufnehmen.",
                "Bild sofort da, Multi am Mauszeiger, nach dem Löschen nichts mehr am Zeiger.", Tool(IsoTool.Multi));
            S("Streuen", "Streuen mischen und Rand ausdünnen",
                "Streu-Pinsel: '+ Set mischen' ein zweites Set dazu, 'Rand ausdünnen' 70, groß über Gras ziehen. 📷 Aufnehmen.",
                "Beide Sets gemischt, zum Rand des Pinsels weniger dicht.", Tool(IsoTool.Scatter));
            S("Fenster", "Optionen-Fenster und Seitenleiste",
                "In der Optionsleiste ⧉ klicken, Fenster auf einen anderen Monitor schieben. '⧉ Seite lösen', dort ↶ / ↷ probieren. Studio schließen und neu öffnen. 📷 Aufnehmen.",
                "Beide Fenster gehen wieder an derselben Stelle auf; Rückgängig / Wiederholen wirken.", Open("tooloptions"));
            S("Werkstatt", "Dungeon mit 2 Etagen",
                "Platz mit 'Bereich' markieren, Dungeon-Werkstatt öffnen, Treppen-Set gewählt, Grundriss → Etagen 2, ▶ Generieren. Unten die Etage −20 ansehen. 📷 Aufnehmen.",
                "Zwei Ebenen, eine Treppe verbindet sie, oben ein Loch über der Treppe. Im Client begehbar?", Open("workshop"));
            S("Werkstatt", "Fremde Karte und Vorlagen mit Bild",
                "In der Werkstatt 'Fremde Karte öffnen …', einen Dungeon markieren, '✔ Bereich in die Werkstatt'. 'Platz als Vorlage speichern', dann '🖼 Vorlagen mit Bild …'. 📷 Aufnehmen.",
                "Die fremde Karte lädt, der Bereich erscheint in der Werkstatt, die Vorlage hat ein Bild.", Open("workshop"));
            S("Karten-Editor", "Natürlicher Lauf: Arten",
                "Karten-Editor, Werkzeug 'Natürlicher Lauf': Art 'Fluss', dann 'Berg-Kamm', dann eine eigene Art (Lauf-Arten …). Jeweils ziehen. 📷 Aufnehmen.",
                "Fluss = Wasser mit Ufer, Berg-Kamm = Fels mit Hang, die eigene Art in ihren Farben.", Open("course"));
            S("Übersicht", "Lesezeichen und Fehler in der Kartenübersicht",
                "Ein Lesezeichen setzen (Reiter Pinsel → ★ Hier merken), dann Küsten-Fehler zeigen. Kartenübersicht ansehen. 📷 Aufnehmen.",
                "Gold = Lesezeichen mit Namen, rot = Fehlerstellen.", Open("cgi"));
        }

        // ================================================================== user interface

        private void BuildUi()
        {
            var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Font = new Font("Segoe UI", 9.5f) };
            var auto = new ToolStripButton("⚙ " + L.T("Automatisch prüfen (alle)")) { Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            auto.Click += (s, e) => RunAuto();
            var autoAll = new ToolStripButton("⚙💾 " + L.T("Automatisch + Bericht")) { ToolTipText = L.T("Alle automatischen Tests mit Bildern, danach gleich den Bericht (ZIP) speichern") };
            autoAll.Click += (s, e) =>
            {
                RunAuto();
                SaveReport();
            };
            var report = new ToolStripButton("💾 " + L.T("Bericht speichern …"));
            report.Click += (s, e) => SaveReport();
            var folder = new ToolStripButton("📂 " + L.T("Ordner"));
            folder.Click += (s, e) => System.Diagnostics.Process.Start("explorer.exe", _folder);
            bar.Items.AddRange(new ToolStripItem[] { auto, autoAll, new ToolStripSeparator(), report, folder });

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 300, FixedPanel = FixedPanel.Panel1 };
            _list.DrawMode = DrawMode.OwnerDrawFixed;
            _list.ItemHeight = 22;
            _list.DrawItem += DrawStep;
            foreach (Step s in _steps)
            {
                _list.Items.Add(s);
            }
            _list.SelectedIndexChanged += (s, e) => ShowStep(_list.SelectedItem as Step);
            split.Panel1.Controls.Add(_list);
            split.Panel1.Controls.Add(new Label { Dock = DockStyle.Top, Height = 22, Text = "  " + L.T("PRÜFLISTE – Schritt für Schritt"), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft });

            var right = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 400 };
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(4) };
            Button B(string text, Color c, Action a)
            {
                var b = new Button { Text = text, AutoSize = true, Height = 32, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Margin = new Padding(3) };
                b.Click += (s, e) => a();
                buttons.Controls.Add(b);
                return b;
            }
            B("▶ " + L.T("Vorbereiten"), Color.FromArgb(37, 99, 235), Prepare);
            B("📷 " + L.T("Aufnehmen"), Color.FromArgb(126, 34, 206), () => Capture(_current, null));
            B("✔ " + L.T("Passt"), Color.FromArgb(22, 128, 61), () => Answer("OK"));
            B("✖ " + L.T("Falsch"), Color.FromArgb(185, 28, 28), () => Answer("FALSCH"));
            B("? " + L.T("Unklar"), Color.FromArgb(180, 83, 9), () => Answer("UNKLAR"));
            B(L.T("Weiter") + " →", Color.FromArgb(71, 85, 105), Next);
            right.Panel1.Controls.Add(_comment);
            right.Panel1.Controls.Add(new Label { Dock = DockStyle.Top, Height = 20, Text = L.T("Dein Kommentar (was ist falsch, was hast du erwartet):"), Padding = new Padding(6, 3, 0, 0) });
            right.Panel1.Controls.Add(buttons);
            right.Panel1.Controls.Add(_expect);
            right.Panel1.Controls.Add(_todo);
            right.Panel1.Controls.Add(_title);
            right.Panel2.Controls.Add(_out);
            right.Panel2.Controls.Add(new Label { Dock = DockStyle.Top, Height = 20, Text = "  " + L.T("PROTOKOLL – das kommt in den Bericht"), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft });
            split.Panel2.Controls.Add(right);
            Controls.Add(split);
            Controls.Add(bar);
            _list.SelectedIndex = 0;
        }

        private void DrawStep(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0)
            {
                return;
            }
            var s = (Step)_list.Items[e.Index];
            string mark = s.Answer == "OK" ? "✔" : s.Answer == "FALSCH" ? "✖" : s.Answer == "UNKLAR" ? "?" : "·";
            Color c = s.Answer == "OK" ? Color.FromArgb(22, 128, 61) : s.Answer == "FALSCH" ? Color.FromArgb(185, 28, 28) : s.Answer == "UNKLAR" ? Color.FromArgb(180, 83, 9) : e.ForeColor;
            using var b = new SolidBrush((e.State & DrawItemState.Selected) != 0 ? e.ForeColor : c);
            e.Graphics.DrawString($"{mark}  {s.Area}: {s.Title}" + (s.Captures.Count > 0 ? $"  📷{s.Captures.Count}" : string.Empty), e.Font, b, e.Bounds.X + 2, e.Bounds.Y + 3);
        }

        private void ShowStep(Step s)
        {
            if (_current != null)
            {
                _current.Comment = _comment.Text;
            }
            _current = s;
            if (s == null)
            {
                return;
            }
            _title.Text = $"{_steps.IndexOf(s) + 1} / {_steps.Count}  ·  {s.Area}: {s.Title}";
            _todo.Text = L.T("So geht's:") + " " + s.Todo;
            _expect.Text = L.T("Das solltest du sehen:") + " " + s.Expect;
            _comment.Text = s.Comment;
            MapData m = _host.Map?.Invoke();
            if (s.UndoAtStart < 0 && m != null)
            {
                s.UndoAtStart = m.UndoCount;
            }
        }

        private void Prepare()
        {
            if (_current == null)
            {
                return;
            }
            MapData m = _host.Map?.Invoke();
            _current.UndoAtStart = m?.UndoCount ?? -1;
            try
            {
                _current.Prepare?.Invoke();
            }
            catch (Exception ex)
            {
                Log("Vorbereiten fehlgeschlagen: " + ex.Message);
            }
            Log($"▶ Schritt '{_current.Title}' vorbereitet. Werkzeug: {_host.ToolInfo?.Invoke()?.Split('\n').FirstOrDefault()}");
        }

        private void Answer(string a)
        {
            if (_current == null)
            {
                return;
            }
            _current.Comment = _comment.Text;
            _current.Answer = a;
            if (_current.Captures.Count == 0)
            {
                Capture(_current, a); // the data of the step always goes into the report
            }
            Log($"{(a == "OK" ? "✔" : a == "FALSCH" ? "✖" : "?")} {_current.Area}: {_current.Title} – {a}" + (_current.Comment.Length > 0 ? $" – Kommentar: {_current.Comment.Replace(Environment.NewLine, " / ")}" : string.Empty));
            _list.Invalidate();
            if (a == "OK")
            {
                Next();
            }
        }

        private void Next()
        {
            if (_list.SelectedIndex < _list.Items.Count - 1)
            {
                _list.SelectedIndex++;
            }
        }

        // ================================================================== capture: screenshot + data

        /// <summary>Screenshot of every studio window, the tool with its settings and what the tool changed since the step began.</summary>
        private void Capture(Step s, string why)
        {
            if (s == null)
            {
                return;
            }
            _shot++;
            string prefix = $"schritt{_steps.IndexOf(s) + 1:00}_{_shot:000}";
            var files = new List<string>();
            double opacity = Opacity;
            try
            {
                Opacity = 0;
                Application.DoEvents();
                System.Threading.Thread.Sleep(120);
                int n = 0;
                foreach (Form f in Application.OpenForms.Cast<Form>().ToList())
                {
                    if (f == this || !f.Visible || f.WindowState == FormWindowState.Minimized || f.Width < 50 || f.Height < 50)
                    {
                        continue;
                    }
                    Rectangle r = f.Bounds;
                    using var bmp = new Bitmap(r.Width, r.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(r.Location, Point.Empty, r.Size);
                    }
                    string file = $"{prefix}_fenster{++n}.png";
                    bmp.Save(Path.Combine(_folder, file), ImageFormat.Png);
                    files.Add(file + $" ({f.Text})");
                }
            }
            catch (Exception ex)
            {
                Log("Bildschirmfoto fehlgeschlagen: " + ex.Message);
            }
            finally
            {
                Opacity = opacity;
            }
            // data: tool, settings and the changes since the start of the step
            var data = new StringBuilder();
            data.AppendLine($"Schritt: {s.Area}: {s.Title}");
            data.AppendLine($"Antwort: {why ?? s.Answer}  Kommentar: {_comment.Text}");
            data.AppendLine("Werkzeug und Einstellungen:");
            data.AppendLine(_host.ToolInfo?.Invoke() ?? "-");
            MapData m = _host.Map?.Invoke();
            if (m != null)
            {
                int count = s.UndoAtStart >= 0 ? m.UndoCount - s.UndoAtStart : 3;
                List<EditStep> steps = m.RecentSteps(Math.Clamp(count, 0, 20));
                data.AppendLine($"Änderungen seit Beginn des Schritts: {steps.Count} Rückgängig-Schritte (neueste zuerst)");
                foreach (EditStep es in steps)
                {
                    data.AppendLine($"== {es.Name ?? "Schritt"}: {es.Statics.Count} Statics, {es.Land.Count} Land");
                    foreach (StaticChange c in es.Statics.Take(1500))
                    {
                        StaticItem it = c.After ?? c.Before;
                        string kind = c.Before == null ? "+" : c.After == null ? "−" : "~";
                        data.AppendLine($"  {kind} {it.Id:x4} {UoArtCache.ItemName(it.Id),-22} ({it.X},{it.Y},{it.Z}) hue {it.Hue} h{UoArtCache.ItemHeight(it.Id)} dir {DirOf(it.Id)}" +
                                        (c.Before != null && c.After != null ? $"  vorher ({c.Before.X},{c.Before.Y},{c.Before.Z}) {c.Before.Id:x4}" : string.Empty));
                    }
                    foreach (LandChange l in es.Land.Take(800))
                    {
                        data.AppendLine($"  L ({l.Index % m.Width},{l.Index / m.Width}) {l.OldId:x4}@{l.OldZ} → {l.NewId:x4}@{l.NewZ}");
                    }
                }
            }
            string dataFile = $"{prefix}_daten.txt";
            File.WriteAllText(Path.Combine(_folder, dataFile), data.ToString());
            files.Add(dataFile);
            s.Captures.AddRange(files);
            _list.Invalidate();
            Log($"📷 {s.Title}: " + string.Join(", ", files));
        }

        private static string DirOf(ushort id)
        {
            try
            {
                int o = BridgeKit.Orientation(id);
                return o > 0 ? "W-O" : o < 0 ? "N-S" : "-";
            }
            catch (Exception)
            {
                return "?";
            }
        }

        // ================================================================== automatic tests

        private void RunAuto()
        {
            UseWaitCursor = true;
            _auto.Clear();
            Log("⚙ Automatische Prüfung startet …");
            var all = new StringBuilder();
            try
            {
                foreach ((string area, string name, Action<object> run) in SelfTests.List(_host.Brushes))
                {
                    TestResult r = SelfTests.Run(area, name, run);
                    _auto.Add(r);
                    int idx = _auto.Count, nr = 0;
                    foreach (TestPicture pic in r.Pictures)
                    {
                        string file = SavePicture(pic, $"auto{idx:00}_{++nr}_{Safe(pic.Title)}.png");
                        if (file != null)
                        {
                            all.AppendLine("   Bild: " + file);
                        }
                    }
                    Log($"{(r.Status == TestStatus.Ok ? "✔" : r.Status == TestStatus.Warn ? "⚠" : r.Status == TestStatus.Fail ? "✖" : "·")} {r.Area}: {r.Name} – {r.Summary} ({r.Milliseconds:0} ms)");
                    all.AppendLine($"######## [{r.Status}] {r.Area}: {r.Name} ({r.Milliseconds:0} ms)");
                    all.AppendLine(r.Details);
                    Application.DoEvents();
                }
            }
            finally
            {
                UseWaitCursor = false;
            }
            File.WriteAllText(Path.Combine(_folder, "automatisch_details.txt"), all.ToString());
            File.WriteAllText(Path.Combine(_folder, "automatisch.json"), JsonSerializer.Serialize(_auto, new JsonSerializerOptions { WriteIndented = true }));
            int ok = _auto.Count(r => r.Status == TestStatus.Ok), warn = _auto.Count(r => r.Status == TestStatus.Warn), fail = _auto.Count(r => r.Status == TestStatus.Fail);
            Log($"⚙ Fertig: {ok} in Ordnung, {warn} Hinweise, {fail} Fehler, {Directory.GetFiles(_folder, "auto*.png").Length} Bilder – Einzelheiten in automatisch_details.txt");
            _host.Status?.Invoke(L.T("Automatische Prüfung") + $": {ok} ✔, {warn} ⚠, {fail} ✖");
        }

        // ================================================================== pictures of the automatic tests

        private static string Safe(string s) => string.Concat(s.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')).Trim('_');

        /// <summary>Draws a test map like the client (with the real art) into a PNG of the report folder; null on failure.</summary>
        private string SavePicture(TestPicture pic, string file)
        {
            Form f = null;
            try
            {
                f = new Form
                {
                    FormBorderStyle = FormBorderStyle.None, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual,
                    Location = new Point(-4000, -4000), Size = new Size(1100, 760), Opacity = 0
                };
                var view = new IsoMapView { Dock = DockStyle.Fill };
                f.Controls.Add(view);
                _host.Configure?.Invoke(view);
                view.BrushSettings = new CgiBrushSettings();
                view.UnderView = false;
                view.MaxZ = 127;
                view.ShowGrid = true;
                view.ShowStatics = true;
                view.Tool = IsoTool.Select;
                f.Show();
                view.SetMap(pic.Map);
                // zoom so the area fits: a tile is 44 pixels wide, the area spans (w + h) / 2 tiles across the screen
                double need = Math.Min(view.Width / ((pic.Area.Width + pic.Area.Height) * 22.0 + 60), view.Height / ((pic.Area.Width + pic.Area.Height) * 11.0 + 200));
                for (int i = 0; i < 20 && view.Zoom > need && view.Zoom > 0.25; i++)
                {
                    view.ZoomStep(-1);
                }
                // small areas: bigger (up to 2×) so the single pieces can be seen
                for (int i = 0; i < 6 && view.Zoom < 2; i++)
                {
                    double before = view.Zoom;
                    view.ZoomStep(1);
                    if (view.Zoom > need)
                    {
                        if (before != view.Zoom)
                        {
                            view.ZoomStep(-1);
                        }
                        break;
                    }
                }
                view.CenterOn(pic.Area.X + pic.Area.Width / 2.0, pic.Area.Y + pic.Area.Height / 2.0);
                view.Refresh(true);
                using var bmp = new Bitmap(view.Width, view.Height);
                view.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using var font = new Font("Segoe UI", 11f, FontStyle.Bold);
                    using var back = new SolidBrush(Color.FromArgb(190, 15, 23, 42));
                    SizeF sz = g.MeasureString(pic.Title, font);
                    g.FillRectangle(back, 6, 6, sz.Width + 12, sz.Height + 6);
                    g.DrawString(pic.Title, font, Brushes.White, 12, 9);
                }
                bmp.Save(Path.Combine(_folder, file), ImageFormat.Png);
                return file;
            }
            catch (Exception ex)
            {
                Log("Bild für '" + pic.Title + "' fehlgeschlagen: " + ex.Message);
                return null;
            }
            finally
            {
                f?.Close();
                f?.Dispose();
            }
        }

        // ================================================================== errors of the studio

        private void OnException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            try
            {
                string trace = Environment.StackTrace;
                if (!trace.Contains("DragonMapStudio") || trace.Contains("TestAssistantForm.OnException") && trace.Split('\n').Count(l => l.Contains("OnException")) > 1)
                {
                    return;
                }
                string where = trace.Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.Contains("DragonMapStudio") && !l.Contains("TestAssistantForm") && !l.Contains("SelfTests")) ?? "?";
                string key = e.Exception.GetType().Name + ": " + e.Exception.Message + " @ " + where;
                lock (_errors)
                {
                    _errors[key] = _errors.TryGetValue(key, out int n) ? n + 1 : 1;
                    if (_errors[key] == 1 && _errorText.Count < 200)
                    {
                        _errorText.Add($"{DateTime.Now:HH:mm:ss} {key}");
                    }
                }
            }
            catch (Exception)
            {
                // never fail here
            }
        }

        // ================================================================== log and report

        private void Log(string text)
        {
            string line = $"{DateTime.Now:HH:mm:ss}  {text}";
            _log.AppendLine(line);
            if (_out.IsHandleCreated || true)
            {
                _out.AppendText(line + Environment.NewLine);
            }
        }

        private void SaveReport()
        {
            if (_current != null)
            {
                _current.Comment = _comment.Text;
            }
            var sb = new StringBuilder();
            sb.AppendLine($"DRAGON MAP STUDIO – TESTBERICHT  {_host.BuildMark}  {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();
            sb.AppendLine("=== PRÜFLISTE ===");
            foreach (Step s in _steps)
            {
                sb.AppendLine($"[{(s.Answer.Length == 0 ? "nicht geprüft" : s.Answer)}] {s.Area}: {s.Title}");
                if (s.Comment.Length > 0)
                {
                    sb.AppendLine("    Kommentar: " + s.Comment.Replace(Environment.NewLine, Environment.NewLine + "               "));
                }
                if (s.Captures.Count > 0)
                {
                    sb.AppendLine("    Aufnahmen: " + string.Join(", ", s.Captures));
                }
            }
            sb.AppendLine();
            sb.AppendLine("=== AUTOMATISCH ===");
            sb.AppendLine(_auto.Count == 0 ? "nicht ausgeführt" : string.Join(Environment.NewLine, _auto.Select(r => r.ToString())));
            sb.AppendLine();
            sb.AppendLine("=== FEHLER DES STUDIOS (Ausnahmen, auch abgefangene) ===");
            lock (_errors)
            {
                sb.AppendLine(_errorText.Count == 0 ? "keine" : string.Join(Environment.NewLine, _errorText.Select(t => t + (_errors.TryGetValue(t.Substring(9), out int n) && n > 1 ? $"  (×{n})" : string.Empty))));
            }
            sb.AppendLine();
            sb.AppendLine("=== PROTOKOLL ===");
            sb.Append(_log);
            File.WriteAllText(Path.Combine(_folder, "bericht.txt"), sb.ToString());
            File.WriteAllText(Path.Combine(_folder, "einstellungen_pinsel.json"), JsonSerializer.Serialize(_host.Brushes, new JsonSerializerOptions { WriteIndented = true }));
            string zip = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"DragonMapStudio_Testbericht_{DateTime.Now:yyyyMMdd_HHmm}.zip");
            try
            {
                if (File.Exists(zip))
                {
                    File.Delete(zip);
                }
                ZipFile.CreateFromDirectory(_folder, zip);
                Log("💾 " + L.T("Bericht gespeichert") + ": " + zip);
                MessageBox.Show(this, L.T("Bericht gespeichert – bitte diese Datei schicken:") + Environment.NewLine + zip, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + zip + "\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text);
            }
        }
    }
}
