using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>One gump found in a scripts folder.</summary>
    public sealed class GalleryEntry
    {
        public string File { get; set; }

        public string ClassName { get; set; }

        public GumpDesign Design { get; set; }

        public List<string> Hints { get; set; } = new List<string>();

        public bool Resolve { get; set; }

        public string Code { get; set; }
    }

    /// <summary>
    /// Gump gallery: searches a ServUO / RunUO / ModernUO scripts folder for gump classes, imports each one and shows a picture.
    /// Open = import into the studio (can be written back later), Add to templates = into the library.
    /// </summary>
    public partial class GumpGalleryForm : Form
    {
        private static string _lastFolder;
        private readonly List<GalleryEntry> _entries = new List<GalleryEntry>();
        private CancellationTokenSource _cancel;

        public GumpGalleryForm()
        {
            InitializeComponent();
            txtFolder.Text = _lastFolder ?? string.Empty;
            foreach (string c in GumpTemplateLibrary.Categories())
            {
                comboCategory.Items.Add(c);
            }
        }

        /// <summary>The gump chosen with "Open".</summary>
        public GalleryEntry Chosen { get; private set; }

        private GalleryEntry Selected => listGumps.SelectedItems.Count > 0 ? listGumps.SelectedItems[0].Tag as GalleryEntry : null;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cancel?.Cancel();
            _lastFolder = txtFolder.Text;
            base.OnFormClosing(e);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var d = new FolderBrowserDialog { Description = "ServUO / RunUO / ModernUO scripts folder", SelectedPath = txtFolder.Text })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    txtFolder.Text = d.SelectedPath;
                }
            }
        }

        private void BtnStop_Click(object sender, EventArgs e) => _cancel?.Cancel();

        private async void BtnScan_Click(object sender, EventArgs e)
        {
            string folder = txtFolder.Text.Trim();
            if (!Directory.Exists(folder))
            {
                MessageBox.Show(this, "Please choose an existing folder. / Bitte einen vorhandenen Ordner wählen.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _cancel?.Cancel();
            _cancel = new CancellationTokenSource();
            CancellationToken token = _cancel.Token;
            _entries.Clear();
            listGumps.Items.Clear();
            imageThumbs.Images.Clear();
            btnScan.Enabled = false;
            btnStop.Enabled = true;
            bool resolve = chkResolve.Checked;
            var progress = new Progress<(int Done, int Total, GalleryEntry Entry)>(p =>
            {
                progressScan.Maximum = Math.Max(1, p.Total);
                progressScan.Value = Math.Min(p.Done, progressScan.Maximum);
                lblStatus.Text = $"{p.Done} / {p.Total} files · {_entries.Count} gumps";
                if (p.Entry != null)
                {
                    _entries.Add(p.Entry);
                    AddItem(p.Entry);
                }
            });
            try
            {
                await Task.Run(() => Scan(folder, resolve, progress, token), token);
                lblStatus.Text = $"{_entries.Count} gumps found in {folder}";
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = $"Stopped – {_entries.Count} gumps.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
            }
            finally
            {
                btnScan.Enabled = true;
                btnStop.Enabled = false;
                progressScan.Value = 0;
            }
        }

        private static void Scan(string folder, bool resolve, IProgress<(int, int, GalleryEntry)> progress, CancellationToken token)
        {
            List<string> files = Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories).Take(30000).ToList();
            Func<string, string> resolver = resolve ? GumpImporter.RootResolver(folder) : null;
            var quick = new Regex(@"\bclass\s+\w+[^{]*:\s*[\w.<>]*Gump\b", RegexOptions.Compiled);
            int done = 0;
            foreach (string file in files)
            {
                token.ThrowIfCancellationRequested();
                done++;
                string code;
                try
                {
                    code = File.ReadAllText(file);
                }
                catch (Exception)
                {
                    continue;
                }
                if (!quick.IsMatch(code))
                {
                    if (done % 50 == 0)
                    {
                        progress.Report((done, files.Count, null));
                    }
                    continue;
                }
                foreach (string cls in GumpImporter.FindGumpClasses(code))
                {
                    token.ThrowIfCancellationRequested();
                    var hints = new List<string>();
                    GumpDesign d;
                    try
                    {
                        d = GumpImporter.Import(code, hints, cls, resolver);
                    }
                    catch (Exception ex)
                    {
                        hints.Add("Import failed: " + ex.Message);
                        d = new GumpDesign { ClassName = cls };
                    }
                    if (d.Elements.Count == 0)
                    {
                        continue;
                    }
                    progress.Report((done, files.Count, new GalleryEntry { File = file, ClassName = cls, Design = d, Hints = hints, Resolve = resolve, Code = code }));
                }
            }
            progress.Report((done, files.Count, null));
        }

        private void AddItem(GalleryEntry entry)
        {
            string key = entry.File + "|" + entry.ClassName;
            if (!imageThumbs.Images.ContainsKey(key))
            {
                imageThumbs.Images.Add(key, Thumb(entry.Design, imageThumbs.ImageSize));
            }
            string search = txtSearch.Text.Trim();
            if (search.Length > 0 && !Matches(entry, search))
            {
                return;
            }
            listGumps.Items.Add(new ListViewItem(entry.ClassName) { Tag = entry, ImageKey = key, ToolTipText = entry.File + (string.IsNullOrEmpty(entry.Design.Command) ? string.Empty : "\n[" + entry.Design.Command) });
        }

        private static bool Matches(GalleryEntry e, string search) =>
            e.ClassName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || e.File.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
            || (e.Design.Command ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        private static Bitmap Thumb(GumpDesign d, Size size)
        {
            var bmp = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(40, 40, 40));
                Bitmap pic = null;
                try
                {
                    pic = GumpRenderer.RenderImage(d, 1);
                }
                catch (Exception)
                {
                    // no client files
                }
                if (pic != null)
                {
                    float scale = Math.Min(1f, Math.Min((size.Width - 4f) / pic.Width, (size.Height - 4f) / pic.Height));
                    int w = Math.Max(1, (int)(pic.Width * scale)), h = Math.Max(1, (int)(pic.Height * scale));
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(pic, (size.Width - w) / 2, (size.Height - h) / 2, w, h);
                    pic.Dispose();
                }
                g.DrawRectangle(Pens.Gray, 0, 0, size.Width - 1, size.Height - 1);
            }
            return bmp;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            listGumps.BeginUpdate();
            listGumps.Items.Clear();
            foreach (GalleryEntry entry in _entries)
            {
                AddItem(entry);
            }
            listGumps.EndUpdate();
        }

        private void ListGumps_SelectedIndexChanged(object sender, EventArgs e)
        {
            GalleryEntry entry = Selected;
            picturePreview.Image?.Dispose();
            picturePreview.Image = null;
            if (entry == null)
            {
                txtInfo.Text = string.Empty;
                return;
            }
            try
            {
                picturePreview.Image = GumpRenderer.RenderImage(entry.Design, 1);
            }
            catch (Exception)
            {
                // no client files
            }
            txtInfo.Text = $"{entry.ClassName}\r\n{entry.File}\r\n{entry.Design.Elements.Count} elements · {entry.Design.Pages} page(s)"
                           + (string.IsNullOrEmpty(entry.Design.Command) ? string.Empty : $" · [{entry.Design.Command} ({entry.Design.CommandAccess})")
                           + "\r\n\r\n" + string.Join("\r\n", entry.Hints.Take(30));
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            if (Selected == null)
            {
                return;
            }
            Chosen = Selected;
            DialogResult = DialogResult.OK;
        }

        private void BtnAddTemplates_Click(object sender, EventArgs e)
        {
            string cat = GumpTemplateLibrary.Safe(comboCategory.Text, true);
            if (cat.Length == 0)
            {
                cat = "ServUO";
            }
            int n = 0;
            foreach (GalleryEntry entry in listGumps.SelectedItems.Cast<ListViewItem>().Select(i => (GalleryEntry)i.Tag))
            {
                try
                {
                    GumpDesign d = entry.Design.Clone();
                    d.Description = "From " + entry.File;
                    d.SourceFile = string.Empty;
                    d.Elements.ForEach(x => x.Source = string.Empty);
                    GumpTemplateLibrary.Save(d, GumpTemplateLibrary.Unique(entry.ClassName, cat), cat, d.Description);
                    n++;
                }
                catch (Exception ex)
                {
                    lblStatus.Text = ex.Message;
                }
            }
            lblStatus.Text = $"{n} gump(s) added to the templates ({cat}).";
        }

        private void BtnShowFile_Click(object sender, EventArgs e)
        {
            if (Selected != null)
            {
                try
                {
                    Process.Start("explorer.exe", "/select,\"" + Selected.File + "\"");
                }
                catch (Exception ex)
                {
                    lblStatus.Text = ex.Message;
                }
            }
        }
    }
}
