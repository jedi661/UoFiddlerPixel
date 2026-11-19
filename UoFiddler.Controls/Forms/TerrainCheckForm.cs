/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Land tiles / textures check:
 *  - empty / one colour / few colour images (land tiles and textures, incl. the extensions): find, delete
 *  - land tile <-> texture: texture id missing / not linked / texture does not fit the art / texture turned wrong
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.ArtLibrary.Core;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class TerrainCheckForm : Form
    {
        private static TerrainCheckForm _instance;

        private sealed class EmptyHit
        {
            public bool Land;
            public int Id;
            public TerrainImageKind Kind;
            public int Colours;
        }

        private sealed class LinkHit
        {
            public int LandId;
            public int TextureId;
            public int TextureSize;
            public double Match = -1;
            public int BestTurn = -1;
            public double BestMatch = -1;
            public string Problem;
            public bool CanLinkSameId;
        }

        private IEnumerator<bool> _job;
        private string _jobName;
        private int _jobTotal, _jobDone;
        private bool _cancel;
        private Button _jobCancelButton;
        private Label _jobStatus;
        private Action _jobFinished;

        private readonly List<EmptyHit> _emptyHits = new List<EmptyHit>();
        private readonly List<LinkHit> _linkHits = new List<LinkHit>();
        private Bitmap _pvLandImage, _pvFromTexImage, _pvTexImage;

        private static readonly string[] _turnNames =
        {
            "as it is", "turned right", "turned 180", "turned left", "mirrored", "mirrored + turned right", "mirrored + turned 180", "mirrored + turned left"
        };

        public TerrainCheckForm()
        {
            InitializeComponent();
        }

        public static void ShowCheck(bool links = false)
        {
            if (_instance == null || _instance.IsDisposed)
            {
                _instance = new TerrainCheckForm();
                _instance.Show();
            }
            else
            {
                _instance.Activate();
            }

            _instance._tabs.SelectedIndex = links ? 1 : 0;
        }

        private void DisposeImages()
        {
            foreach (PictureBox b in new[] { _pvLand, _pvFromTex, _pvTex })
            {
                if (b != null)
                {
                    b.Image = null;
                }
            }

            _pvLandImage?.Dispose();
            _pvFromTexImage?.Dispose();
            _pvTexImage?.Dispose();
        }

        #region Job runner (UI thread, small time slices - Art is not thread safe)

        private void StartJob(string name, int total, IEnumerable<bool> steps, Button cancel, Label status, Action finished)
        {
            if (_job != null)
            {
                return;
            }

            _jobName = name;
            _jobTotal = Math.Max(1, total);
            _jobDone = 0;
            _cancel = false;
            _job = steps.GetEnumerator();
            _jobCancelButton = cancel;
            _jobStatus = status;
            _jobFinished = finished;
            cancel.Enabled = true;
            _scanButton.Enabled = false;
            _linkScanButton.Enabled = false;
            _jobTimer.Start();
        }

        private void OnJobTick(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();
            bool finished = false;
            while (sw.ElapsedMilliseconds < 60)
            {
                if (_cancel || !_job.MoveNext())
                {
                    finished = true;
                    break;
                }

                ++_jobDone;
            }

            _jobStatus.Text = $"{_jobName}: {_jobDone * 100 / _jobTotal} %";
            if (!finished)
            {
                return;
            }

            _jobTimer.Stop();
            _job.Dispose();
            _job = null;
            _jobCancelButton.Enabled = false;
            _scanButton.Enabled = true;
            _linkScanButton.Enabled = true;
            _jobFinished?.Invoke();
            if (_cancel)
            {
                _jobStatus.Text += " (cancelled)";
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            _cancel = true;
        }

        #endregion

        #region Empty / placeholder

        private static IEnumerable<int> LandIds()
        {
            int max = Art.GetMaxLandId();
            for (int id = 0; id <= max; ++id)
            {
                if (Art.IsLegalLandId(id))
                {
                    yield return id;
                }
            }
        }

        private static IEnumerable<int> TextureIds()
        {
            int legacy = Math.Min(Textures.GetIdxLength(), Textures.LegacyCount);
            for (int id = 0; id < legacy; ++id)
            {
                yield return id;
            }

            if (Textures.HasExtension)
            {
                for (int id = Textures.Extension.BaseId; id <= Textures.Extension.MaxId; ++id)
                {
                    yield return id;
                }
            }
        }

        private void OnScanEmpty(object sender, EventArgs e)
        {
            _emptyHits.Clear();
            _emptyList.Items.Clear();
            _thumbs.Images.Clear();
            int total = (_scanLand.Checked ? Art.GetMaxLandId() + 1 : 0) + (_scanTextures.Checked ? TextureIds().Count() : 0);
            StartJob("Search", total, ScanEmpty(), _cancelEmptyButton, _emptyStatus, FillEmptyList);
        }

        private bool Wanted(TerrainImageKind kind) =>
            (kind == TerrainImageKind.Empty && _kindEmpty.Checked) ||
            (kind == TerrainImageKind.SingleColour && _kindSingle.Checked) ||
            (kind == TerrainImageKind.FewColours && _kindFew.Checked);

        private IEnumerable<bool> ScanEmpty()
        {
            if (_scanLand.Checked)
            {
                foreach (int id in LandIds())
                {
                    if (Art.IsValidLand(id))
                    {
                        Bitmap bmp = Art.GetLand(id);
                        if (bmp != null)
                        {
                            TerrainImageKind kind = LandTextureMath.Classify(ArtLibraryCodec.ToPixels(bmp), bmp.Width, bmp.Height, true, out int colours);
                            if (Wanted(kind))
                            {
                                _emptyHits.Add(new EmptyHit { Land = true, Id = id, Kind = kind, Colours = colours });
                            }
                        }
                    }

                    yield return true;
                }
            }

            if (_scanTextures.Checked)
            {
                foreach (int id in TextureIds())
                {
                    if (Textures.TestTexture(id))
                    {
                        Bitmap bmp = Textures.GetTexture(id);
                        if (bmp != null)
                        {
                            TerrainImageKind kind = LandTextureMath.Classify(ArtLibraryCodec.ToPixels(bmp), bmp.Width, bmp.Height, false, out int colours);
                            if (Wanted(kind))
                            {
                                _emptyHits.Add(new EmptyHit { Land = false, Id = id, Kind = kind, Colours = colours });
                            }
                        }
                    }

                    yield return true;
                }
            }
        }

        /// <summary>texture id -> land tiles that use it (from the land tiledata).</summary>
        private static Dictionary<int, List<int>> TextureUsers()
        {
            var map = new Dictionary<int, List<int>>();
            if (TileData.LandTable == null)
            {
                return map;
            }

            for (int i = 0; i < TileData.LandTable.Length; ++i)
            {
                int tex = TileData.LandTable[i].TextureId;
                if (tex == 0)
                {
                    continue;
                }

                if (!map.TryGetValue(tex, out List<int> list))
                {
                    map[tex] = list = new List<int>();
                }

                list.Add(i);
            }

            return map;
        }

        private void FillEmptyList()
        {
            Dictionary<int, List<int>> users = TextureUsers();
            _emptyList.BeginUpdate();
            try
            {
                _emptyList.Items.Clear();
                _thumbs.Images.Clear();
                foreach (EmptyHit h in _emptyHits)
                {
                    Bitmap bmp = h.Land ? Art.GetLand(h.Id) : Textures.GetTexture(h.Id);
                    _thumbs.Images.Add(Thumb(bmp));
                    string link;
                    if (h.Land)
                    {
                        int tex = TileData.LandTable != null && h.Id < TileData.LandTable.Length ? TileData.LandTable[h.Id].TextureId : 0;
                        link = $"'{(TileData.LandTable != null && h.Id < TileData.LandTable.Length ? TileData.LandTable[h.Id].Name : string.Empty)}', texture 0x{tex:X}";
                    }
                    else
                    {
                        link = users.TryGetValue(h.Id, out List<int> list)
                            ? $"used by {list.Count} land tile(s): {string.Join(", ", list.Take(8).Select(i => $"0x{i:X}"))}{(list.Count > 8 ? " ..." : string.Empty)}"
                            : "not used by a land tile";
                    }

                    var item = new ListViewItem(h.Land ? "Land tile" : "Texture", _thumbs.Images.Count - 1) { Tag = h };
                    item.SubItems.Add($"0x{h.Id:X}");
                    item.SubItems.Add(h.Kind == TerrainImageKind.Empty ? "empty (black / transparent)" : h.Kind == TerrainImageKind.SingleColour ? "one colour (placeholder)" : "2 - 4 colours");
                    item.SubItems.Add(h.Colours.ToString());
                    item.SubItems.Add(link);
                    _emptyList.Items.Add(item);
                }
            }
            finally
            {
                _emptyList.EndUpdate();
            }

            _emptyStatus.Text = $"{_emptyHits.Count(h => h.Land):N0} land tile(s), {_emptyHits.Count(h => !h.Land):N0} texture(s) found.";
        }

        private static Bitmap Thumb(Bitmap src)
        {
            var t = new Bitmap(32, 32);
            using Graphics g = Graphics.FromImage(t);
            g.Clear(Color.FromArgb(60, 60, 60));
            if (src != null)
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(src, new Rectangle(0, 0, 32, 32));
            }

            return t;
        }

        private void OnCheckAll(object sender, EventArgs e)
        {
            foreach (ListViewItem i in _emptyList.Items)
            {
                i.Checked = true;
            }
        }

        private void OnCheckNone(object sender, EventArgs e)
        {
            foreach (ListViewItem i in _emptyList.Items)
            {
                i.Checked = false;
            }
        }

        private void OnDeleteChecked(object sender, EventArgs e)
        {
            List<EmptyHit> todo = _emptyList.CheckedItems.Cast<ListViewItem>().Select(i => (EmptyHit)i.Tag).ToList();
            if (todo.Count == 0)
            {
                MessageBox.Show(this, "Check the entries to delete first.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this,
                    $"Delete {todo.Count(h => h.Land):N0} land tile(s) and {todo.Count(h => !h.Land):N0} texture(s)?" + Environment.NewLine + Environment.NewLine +
                    "Careful: some black / one colour graphics are used on purpose (e.g. the black 'void' land tiles or NODRAW). " +
                    "Only the loaded data is changed - save afterwards; a backup (Extended tab) is recommended.",
                    "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            int cleared = 0;
            var deletedTextures = new HashSet<int>();
            foreach (EmptyHit h in todo)
            {
                if (h.Land)
                {
                    Art.RemoveLand(h.Id);
                    ControlEvents.FireLandTileChangeEvent(this, h.Id);
                    Options.ChangedUltimaClass["Art"] = true;
                }
                else
                {
                    Textures.Remove(h.Id);
                    ControlEvents.FireTextureChangeEvent(this, h.Id);
                    Options.ChangedUltimaClass["Texture"] = true;
                    deletedTextures.Add(h.Id);
                }
            }

            if (_clearLinksCheck.Checked && deletedTextures.Count > 0 && TileData.LandTable != null)
            {
                for (int i = 0; i < TileData.LandTable.Length; ++i)
                {
                    if (deletedTextures.Contains(TileData.LandTable[i].TextureId))
                    {
                        LandData d = TileData.LandTable[i];
                        d.TextureId = 0;
                        TileData.LandTable[i] = d;
                        ++cleared;
                    }
                }

                if (cleared > 0)
                {
                    Options.ChangedUltimaClass["TileData"] = true;
                    ControlEvents.FireTileDataChangeEvent(this, 0);
                }
            }

            foreach (ListViewItem i in _emptyList.CheckedItems.Cast<ListViewItem>().ToList())
            {
                _emptyList.Items.Remove(i);
            }

            _emptyStatus.Text = $"Deleted {todo.Count:N0} entr(y/ies)" + (cleared > 0 ? $", {cleared:N0} land tile link(s) set to 0" : string.Empty) + ". Save the tabs to write it.";
        }

        private void OnEmptyShow(object sender, EventArgs e)
        {
            if (_emptyList.SelectedItems.Count == 0)
            {
                return;
            }

            var h = (EmptyHit)_emptyList.SelectedItems[0].Tag;
            if (h.Land)
            {
                LandTilesControl.ActivateTab();
                LandTilesControl.SearchGraphic(h.Id);
            }
            else
            {
                TexturesControl.SelectInTab(h.Id);
            }
        }

        #endregion

        #region Land tile <-> texture

        private void OnScanLinks(object sender, EventArgs e)
        {
            _linkHits.Clear();
            _linkList.Items.Clear();
            StartJob("Check", Art.GetMaxLandId() + 1, ScanLinks(), _cancelLinkButton, _linkStatus, FillLinkList);
        }

        private IEnumerable<bool> ScanLinks()
        {
            double threshold = (double)_threshold.Value;
            bool onlyProblems = _onlyProblemsCheck.Checked;
            var textureCache = new Dictionary<int, (ushort[] Px, int Size, ushort[] Land)>();
            foreach (int id in LandIds())
            {
                if (TileData.LandTable == null || id >= TileData.LandTable.Length || !Art.IsValidLand(id))
                {
                    yield return true;
                    continue;
                }

                var hit = new LinkHit { LandId = id, TextureId = TileData.LandTable[id].TextureId };
                if (hit.TextureId == 0)
                {
                    if (Textures.TestTexture(id))
                    {
                        hit.Problem = $"no texture linked, but texture 0x{id:X} exists";
                        hit.CanLinkSameId = true;
                    }
                    else if (!onlyProblems)
                    {
                        hit.Problem = "no texture (flat only)";
                    }
                }
                else if (!Textures.TestTexture(hit.TextureId))
                {
                    hit.Problem = $"texture 0x{hit.TextureId:X} does NOT exist";
                    hit.CanLinkSameId = hit.TextureId != id && Textures.TestTexture(id);
                }
                else
                {
                    if (!textureCache.TryGetValue(hit.TextureId, out var tex))
                    {
                        Bitmap t = Textures.GetTexture(hit.TextureId);
                        ushort[] px = ArtLibraryCodec.ToPixels(t);
                        tex = (px, t.Width, LandTextureMath.LandFromTexture(px, t.Width));
                        textureCache[hit.TextureId] = tex;
                    }

                    hit.TextureSize = tex.Size;
                    ushort[] land = ArtLibraryCodec.ToPixels(Art.GetLand(id));
                    hit.Match = LandTextureMath.MatchPercent(tex.Land, land);
                    if (hit.Match < threshold)
                    {
                        hit.BestTurn = LandTextureMath.BestOrientation(tex.Px, tex.Size, land, out hit.BestMatch);
                        hit.Problem = hit.BestTurn != 0 && hit.BestMatch >= hit.Match + 5
                            ? $"texture seems turned: '{_turnNames[hit.BestTurn]}' fits {hit.BestMatch:0.0} %"
                            : "texture looks different from the land tile art";
                    }
                    else if (!onlyProblems)
                    {
                        hit.Problem = "ok";
                    }
                }

                if (hit.Problem != null)
                {
                    _linkHits.Add(hit);
                }

                yield return true;
            }
        }

        private void FillLinkList()
        {
            _linkList.BeginUpdate();
            try
            {
                _linkList.Items.Clear();
                foreach (LinkHit h in _linkHits)
                {
                    var item = new ListViewItem($"0x{h.LandId:X}") { Tag = h };
                    item.SubItems.Add(TileData.LandTable[h.LandId].Name);
                    item.SubItems.Add(h.TextureId == 0 ? "-" : $"0x{h.TextureId:X}");
                    item.SubItems.Add(h.TextureSize > 0 ? $"{h.TextureSize}x{h.TextureSize}" : string.Empty);
                    item.SubItems.Add(h.Match >= 0 ? $"{h.Match:0.0} %" : string.Empty);
                    item.SubItems.Add(h.BestTurn >= 0 ? $"{_turnNames[h.BestTurn]} ({h.BestMatch:0.0} %)" : string.Empty);
                    item.SubItems.Add(h.Problem);
                    item.ForeColor = h.Problem == "ok" || h.Problem.StartsWith("no texture (") ? SystemColors.WindowText
                        : h.Problem.Contains("NOT exist") ? Color.DarkRed : Color.DarkOrange;
                    _linkList.Items.Add(item);
                }
            }
            finally
            {
                _linkList.EndUpdate();
            }

            int problems = _linkHits.Count(h => h.Problem != "ok" && !h.Problem.StartsWith("no texture ("));
            _linkStatus.Text = $"{problems:N0} problem(s) in {Art.GetMaxLandId() + 1:N0} land ids.";
        }

        private LinkHit SelectedLink() => _linkList.SelectedItems.Count > 0 ? (LinkHit)_linkList.SelectedItems[0].Tag : null;

        private void OnLinkSelected(object sender, EventArgs e)
        {
            LinkHit h = SelectedLink();
            if (h == null)
            {
                return;
            }

            Bitmap land = Art.GetLand(h.LandId);
            ShowImage(_pvLand, ref _pvLandImage, land == null ? null : Enlarge(ArtLibraryCodec.ToPixels(land), 44, 4));
            if (h.TextureId > 0 && Textures.TestTexture(h.TextureId))
            {
                Bitmap t = Textures.GetTexture(h.TextureId);
                ushort[] px = ArtLibraryCodec.ToPixels(t);
                ShowImage(_pvFromTex, ref _pvFromTexImage, Enlarge(LandTextureMath.LandFromTexture(px, t.Width), 44, 4));
                ShowImage(_pvTex, ref _pvTexImage, Enlarge(px, t.Width, 256 / t.Width));
            }
            else
            {
                ShowImage(_pvFromTex, ref _pvFromTexImage, null);
                ShowImage(_pvTex, ref _pvTexImage, null);
            }

            _setTexIdButton.Enabled = h.CanLinkSameId;
            _fixTurnButton.Enabled = h.BestTurn > 0;
        }

        private static void ShowImage(PictureBox box, ref Bitmap current, Bitmap next)
        {
            Bitmap old = current;
            box.Image = next;
            current = next;
            old?.Dispose();
        }

        private static Bitmap Enlarge(ushort[] px, int size, int factor)
        {
            using Bitmap small = ArtLibraryCodec.ToBitmap(px, size, size);
            var big = new Bitmap(size * factor, size * factor);
            using Graphics g = Graphics.FromImage(big);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(small, 0, 0, size * factor, size * factor);
            return big;
        }

        private void OnSetTexIdToLand(object sender, EventArgs e)
        {
            LinkHit h = SelectedLink();
            if (h == null || !Textures.TestTexture(h.LandId))
            {
                return;
            }

            LandData d = TileData.LandTable[h.LandId];
            d.TextureId = h.LandId;
            TileData.LandTable[h.LandId] = d;
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, ControlEvents.TileDataLandEventId(h.LandId));
            h.TextureId = h.LandId;
            h.Problem = $"linked to texture 0x{h.LandId:X} (not saved yet)";
            h.CanLinkSameId = false;
            FillLinkList();
        }

        private void OnFixTurn(object sender, EventArgs e)
        {
            LinkHit h = SelectedLink();
            if (h == null || h.BestTurn <= 0 || !Textures.TestTexture(h.TextureId))
            {
                return;
            }

            int users = TextureUsers().TryGetValue(h.TextureId, out List<int> list) ? list.Count : 1;
            if (MessageBox.Show(this,
                    $"Turn texture 0x{h.TextureId:X} '{_turnNames[h.BestTurn]}'?" + (users > 1 ? $"{Environment.NewLine}{users} land tiles use this texture - all of them are affected." : string.Empty),
                    "Turn texture", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            Bitmap t = Textures.GetTexture(h.TextureId);
            ushort[] turned = LandTextureMath.Orient(ArtLibraryCodec.ToPixels(t), t.Width, h.BestTurn);
            Textures.Replace(h.TextureId, ArtLibraryCodec.ToBitmap(turned, t.Width, t.Width));
            Options.ChangedUltimaClass["Texture"] = true;
            ControlEvents.FireTextureChangeEvent(this, h.TextureId);
            h.Match = h.BestMatch;
            h.BestTurn = -1;
            h.Problem = "texture turned (not saved yet)";
            FillLinkList();
        }

        private void OnOpenStudio(object sender, EventArgs e)
        {
            LinkHit h = SelectedLink();
            if (h != null)
            {
                LandTextureStudioForm.ShowStudio(h.LandId);
            }
        }

        private void OnLinkShowLand(object sender, EventArgs e)
        {
            LinkHit h = SelectedLink();
            if (h != null)
            {
                LandTilesControl.ActivateTab();
                LandTilesControl.SearchGraphic(h.LandId);
            }
        }

        private void OnLinkShowTileData(object sender, EventArgs e)
        {
            LinkHit h = SelectedLink();
            if (h != null)
            {
                TileDataControl.ActivateTab();
                TileDataControl.Select(h.LandId, true);
            }
        }

        #endregion
    }
}
