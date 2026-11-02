/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Art;
using UoFiddler.Plugin.Enhanced.EcUltima.Gumps;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Classic art and gumps → Enhanced Client: lists with the state of every id, converting marked ids,
    /// own images on any id, report of what is missing in the EC, help.
    /// </summary>
    public sealed partial class EcArtConverterControl : UserControl, IEcEditor
    {
        private sealed class Row
        {
            public int Id;
            public bool Classic;
            public string State;
            public int StateCode;
            public string Name;
        }

        // item filter: 0 all, 1 missing, 2 no image, 3 legacy only, 4 EC image, 5 not in classic, 6 missing + no image
        // gump filter: 0 all, 1 missing, 2 empty, 3 present, 4 not in classic

        private EcClient _client;
        private EcArtConverter _converter;
        private bool _busy;
        private List<Row> _items = new List<Row>();
        private List<Row> _itemView = new List<Row>();
        private List<Row> _gumps = new List<Row>();
        private List<Row> _gumpView = new List<Row>();

        public EcArtConverterControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            Disposed += OnControlDisposed;
            comboBoxScale.SelectedIndex = 0;
            toolStripComboBoxFilterItems.SelectedIndex = 6;
            toolStripComboBoxFilterGumps.SelectedIndex = 1;
            ShowHelp();
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _converter = client == null ? null : new EcArtConverter(client);
            _items = new List<Row>();
            _gumps = new List<Row>();
            ApplyItemFilter();
            ApplyGumpFilter();
            UpdateSaveButton();
        }

        public void OnActivated()
        {
            UpdateClassicLabel();
            UpdateSaveButton();
        }

        private static bool ClassicLoaded
        {
            get
            {
                try
                {
                    return Ultima.TileData.ItemTable != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private void UpdateClassicLabel()
        {
            string folder;
            try
            {
                folder = Ultima.Files.RootDir;
            }
            catch (Exception)
            {
                folder = null;
            }
            toolStripLabelClassic.Text = ClassicLoaded ? $"Classic: {folder}" : "Classic: no client loaded in UOFiddler (only own images can be imported)";
        }

        private void UpdateSaveButton()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(UpdateSaveButton));
                return;
            }
            toolStripButtonSave.Enabled = _client != null && _client.HasUnsavedChanges;
        }

        // ==================================================================== classic data

        private static int ClassicItemCount()
        {
            try
            {
                return Math.Max(Ultima.Art.GetMaxItemId() + 1, Ultima.TileData.ItemTable?.Length ?? 0);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static bool ClassicItemValid(int id)
        {
            try
            {
                return Ultima.Art.IsValidStatic(id);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string ClassicItemName(int id)
        {
            try
            {
                Ultima.ItemData[] table = Ultima.TileData.ItemTable;
                return table != null && id < table.Length ? table[id].Name : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static ClassicItemInfo ClassicInfo(int id)
        {
            try
            {
                Ultima.ItemData[] table = Ultima.TileData.ItemTable;
                if (table == null || id >= table.Length)
                {
                    return null;
                }
                Ultima.ItemData d = table[id];
                ushort radar = 0;
                try
                {
                    radar = Ultima.RadarCol.GetItemColor(id);
                }
                catch (Exception)
                {
                    // no radarcol.mul
                }
                return new ClassicItemInfo
                {
                    Name = d.Name,
                    Flags = (ulong)d.Flags,
                    Weight = d.Weight,
                    Quality = d.Quality,
                    Quantity = d.Quantity,
                    Value = d.Value,
                    Height = d.Height,
                    Animation = d.Animation,
                    RadarColor = radar
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// A COPY of the classic image: UOFiddler keeps the images in its cache, they must never be disposed here.
        /// </summary>
        private static Bitmap ClassicItemImage(int id)
        {
            try
            {
                Bitmap cached = Ultima.Art.IsValidStatic(id) ? Ultima.Art.GetStatic(id) : null;
                return cached == null ? null : new Bitmap(cached);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static int ClassicGumpCount()
        {
            try
            {
                return Ultima.Gumps.GetCount();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static bool ClassicGumpValid(int id)
        {
            try
            {
                return Ultima.Gumps.IsValidIndex(id);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>A COPY of the classic gump (the original stays in the cache of UOFiddler).</summary>
        private static Bitmap ClassicGumpImage(int id)
        {
            try
            {
                Bitmap cached = Ultima.Gumps.IsValidIndex(id) ? Ultima.Gumps.GetGump(id) : null;
                return cached == null ? null : new Bitmap(cached);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ==================================================================== scanning

        private bool NeedClient()
        {
            if (_client != null)
            {
                return true;
            }
            EcDialogs.ShowError(this, "Choose the Enhanced Client folder on top first.");
            return false;
        }

        private List<Row> ScanItems(IProgress<int> progress)
        {
            TileArtFile tileArt = _client.GetTileArt();
            int classicCount = ClassicItemCount();
            int max = Math.Max(classicCount, tileArt == null || tileArt.Count == 0 ? 0 : (int)Math.Min(0x20000, tileArt.Records.Where(r => r.Id < 0x20000).Select(r => r.Id).DefaultIfEmpty(0u).Max() + 1));
            var rows = new List<Row>();
            for (int id = 0; id < max; id++)
            {
                if ((id & 1023) == 0)
                {
                    progress.Report(id * 100 / Math.Max(1, max));
                }
                bool classic = id < classicCount && ClassicItemValid(id);
                EcItemState state = _converter.GetState((uint)id);
                if (!classic && state == EcItemState.Missing)
                {
                    continue;
                }
                string name = classic ? ClassicItemName(id) : null;
                if (string.IsNullOrEmpty(name) && state != EcItemState.Missing && _client.HasStringDictionary)
                {
                    TileArtEntry e = tileArt?.Get((uint)id)?.Entry;
                    name = e == null ? null : _client.StringDictionary.Get(e.NameIndex);
                }
                rows.Add(new Row { Id = id, Classic = classic, StateCode = (int)state, State = EcArtConverter.StateText(state), Name = name ?? string.Empty });
            }
            progress.Report(100);
            return rows;
        }

        private List<Row> ScanGumps(IProgress<int> progress)
        {
            int classicCount = ClassicGumpCount();
            EcUopPackage package = _client.GetPackage(EcArtConverter.GumpFile);
            package?.ResolveNames(EcArtConverter.GumpPattern, 0, 0x20000);
            var ecIds = new HashSet<int>();
            if (package != null)
            {
                foreach (EcUopEntry entry in package.Entries)
                {
                    if (entry.Name != null && EcClient.TryGetIdFromName(entry.Name, out int id))
                    {
                        ecIds.Add(id);
                    }
                }
            }
            int max = Math.Max(classicCount, ecIds.Count == 0 ? 0 : ecIds.Max() + 1);
            var rows = new List<Row>();
            for (int id = 0; id < max; id++)
            {
                if ((id & 1023) == 0)
                {
                    progress.Report(id * 100 / Math.Max(1, max));
                }
                bool classic = id < classicCount && ClassicGumpValid(id);
                GumpState state = ecIds.Contains(id) ? _converter.GetGumpState(id) : GumpState.Missing;
                if (!classic && state == GumpState.Missing)
                {
                    continue;
                }
                rows.Add(new Row { Id = id, Classic = classic, StateCode = (int)state, State = GumpStateText(state), Name = string.Empty });
            }
            progress.Report(100);
            return rows;
        }

        private static string GumpStateText(GumpState state) => state switch
        {
            GumpState.Missing => "missing in EC",
            GumpState.Empty => "empty in EC",
            _ => "present"
        };

        private async void ScanItemsAsync()
        {
            if (!NeedClient())
            {
                return;
            }
            List<Row> rows = null;
            await Run("Check items", p =>
            {
                rows = ScanItems(p);
                return $"{rows.Count} items: " + Summary(rows, true);
            });
            if (rows != null)
            {
                _items = rows;
                ApplyItemFilter();
            }
        }

        private async void ScanGumpsAsync()
        {
            if (!NeedClient())
            {
                return;
            }
            List<Row> rows = null;
            await Run("Check gumps", p =>
            {
                rows = ScanGumps(p);
                return $"{rows.Count} gumps: " + Summary(rows, false);
            });
            if (rows != null)
            {
                _gumps = rows;
                ApplyGumpFilter();
            }
        }

        private static string Summary(List<Row> rows, bool items)
        {
            int classic = rows.Count(r => r.Classic);
            int missing = rows.Count(r => r.Classic && r.StateCode == 0);
            if (items)
            {
                int noImage = rows.Count(r => r.Classic && r.StateCode == (int)EcItemState.NoImage);
                int legacy = rows.Count(r => r.StateCode == (int)EcItemState.LegacyOnly);
                int ec = rows.Count(r => r.StateCode == (int)EcItemState.EcImage);
                return $"{classic} classic, {missing} missing in EC, {noImage} without image, {legacy} legacy only, {ec} with EC image";
            }
            int empty = rows.Count(r => r.Classic && r.StateCode == (int)GumpState.Empty);
            return $"{classic} classic, {missing} missing in EC, {empty} empty in EC";
        }

        // ==================================================================== lists

        private static bool SearchMatch(Row r, string search)
        {
            if (search.Length == 0)
            {
                return true;
            }
            if (EcDialogs.TryParseNumber(search, out long number))
            {
                return r.Id == number;
            }
            return r.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ApplyItemFilter()
        {
            int f = toolStripComboBoxFilterItems.SelectedIndex;
            string search = toolStripTextBoxSearchItems.Text.Trim();
            bool jump = search.Length > 0 && EcDialogs.TryParseNumber(search, out _);
            _itemView = _items.Where(r => (jump || f switch
            {
                1 => r.Classic && r.StateCode == (int)EcItemState.Missing,
                2 => r.Classic && r.StateCode == (int)EcItemState.NoImage,
                3 => r.StateCode == (int)EcItemState.LegacyOnly,
                4 => r.StateCode == (int)EcItemState.EcImage,
                5 => !r.Classic,
                6 => r.Classic && (r.StateCode == (int)EcItemState.Missing || r.StateCode == (int)EcItemState.NoImage),
                _ => true
            }) && SearchMatch(r, search)).ToList();
            listViewItems.VirtualListSize = _itemView.Count;
            listViewItems.Invalidate();
            toolStripStatusLabel.Text = _items.Count == 0 ? "Press \"Check\" to compare the classic items with the Enhanced Client." : $"{_itemView.Count} of {_items.Count} items shown. " + Summary(_items, true);
            if (jump && _itemView.Count == 0 && EcDialogs.TryParseNumber(search, out long id))
            {
                toolStripStatusLabel.Text = $"Id {id} is free (neither classic nor EC) – \"Import image...\" puts an image on it.";
            }
        }

        private void ApplyGumpFilter()
        {
            int f = toolStripComboBoxFilterGumps.SelectedIndex;
            string search = toolStripTextBoxSearchGumps.Text.Trim();
            bool jump = search.Length > 0 && EcDialogs.TryParseNumber(search, out _);
            _gumpView = _gumps.Where(r => (jump || f switch
            {
                1 => r.Classic && r.StateCode == (int)GumpState.Missing,
                2 => r.Classic && r.StateCode == (int)GumpState.Empty,
                3 => r.StateCode == (int)GumpState.Present,
                4 => !r.Classic,
                _ => true
            }) && SearchMatch(r, search)).ToList();
            listViewGumps.VirtualListSize = _gumpView.Count;
            listViewGumps.Invalidate();
            if (tabControl.SelectedTab == tabPageGumps)
            {
                toolStripStatusLabel.Text = _gumps.Count == 0 ? "Press \"Check\" to compare the classic gumps with the Enhanced Client." : $"{_gumpView.Count} of {_gumps.Count} gumps shown. " + Summary(_gumps, false);
            }
        }

        private static ListViewItem MakeItem(List<Row> view, int index)
        {
            if (index < 0 || index >= view.Count)
            {
                return new ListViewItem(string.Empty);
            }
            Row r = view[index];
            var item = new ListViewItem(r.Id.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add($"0x{r.Id:X4}");
            item.SubItems.Add(r.Classic ? "yes" : "-");
            item.SubItems.Add(r.State);
            item.SubItems.Add(r.Name);
            return item;
        }

        private static void DrawRow(List<Row> view, DrawListViewSubItemEventArgs e, bool items)
        {
            if (e.ColumnIndex != 3 || e.ItemIndex >= view.Count)
            {
                e.DrawDefault = true;
                return;
            }
            Row r = view[e.ItemIndex];
            Color color = items
                ? r.StateCode switch { (int)EcItemState.Missing => Color.Firebrick, (int)EcItemState.NoImage => Color.DarkOrange, (int)EcItemState.LegacyOnly => Color.SteelBlue, _ => Color.ForestGreen }
                : r.StateCode switch { (int)GumpState.Missing => Color.Firebrick, (int)GumpState.Empty => Color.DarkOrange, _ => Color.ForestGreen };
            e.DrawBackground();
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            }
            using (var brush = new SolidBrush(color))
            {
                e.Graphics.FillEllipse(brush, e.Bounds.X + 3, e.Bounds.Y + ((e.Bounds.Height - 9) / 2), 9, 9);
            }
            TextRenderer.DrawText(e.Graphics, r.State, e.Item.ListView.Font, new Rectangle(e.Bounds.X + 15, e.Bounds.Y, e.Bounds.Width - 15, e.Bounds.Height),
                e.Item.Selected ? SystemColors.HighlightText : SystemColors.WindowText, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static List<Row> Marked(ListView list, List<Row> view)
        {
            var rows = new List<Row>();
            foreach (int index in list.SelectedIndices)
            {
                if (index < view.Count)
                {
                    rows.Add(view[index]);
                }
            }
            return rows;
        }

        private static void MarkAll(ListView list, int count)
        {
            list.Focus();
            for (int i = 0; i < count; i++)
            {
                list.SelectedIndices.Add(i);
            }
        }

        // ==================================================================== previews

        private ArtConvertOptions Options() => new ArtConvertOptions
        {
            Target = radioButtonLegacyOnly.Checked ? ArtTarget.LegacyOnly : ArtTarget.LegacyAndEc,
            Scale = (ArtScaleMode)Math.Max(0, comboBoxScale.SelectedIndex),
            ItemData = checkBoxItemData.Checked,
            Uncompressed = checkBoxUncompressed.Checked
        };

        private static void SetImage(PictureBox box, Image image)
        {
            Image old = box.Image;
            box.Image = image;
            old?.Dispose();
        }

        /// <summary>Shows small images 2 times larger (pixel exact) so they can be judged.</summary>
        private static Bitmap Zoom(Bitmap image, Size box)
        {
            if (image == null)
            {
                return null;
            }
            int factor = image.Width * 2 <= box.Width && image.Height * 2 <= box.Height ? 2 : 1;
            var result = new Bitmap(image.Width * factor, image.Height * factor);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(image, 0, 0, result.Width, result.Height);
            }
            return result;
        }

        private Bitmap EcItemImageNow(uint id, out string text)
        {
            text = null;
            TileArtEntry e = _client.GetTileArt()?.Get(id)?.Entry;
            if (e == null)
            {
                text = "no tileart entry";
                return null;
            }
            try
            {
                bool ec = e.Textures.Any(t => t.Slot == 0) && e.EcImageOffset.XEnd > e.EcImageOffset.XStart;
                byte[] data = ec ? _client.GetArtData(EcClient.ArtSources[0], (int)id) : null;
                TileArtImageOffset area = e.EcImageOffset;
                text = "EC image (world art)";
                if (data == null)
                {
                    data = _client.GetArtData(EcClient.ArtSources[1], (int)id);
                    area = e.CcImageOffset;
                    text = "legacy texture (classic size)";
                }
                if (data == null)
                {
                    text = "no image";
                    return null;
                }
                DdsImage dds = DdsImage.Decode(data);
                using (Bitmap full = EcImageHelper.ToBitmap(dds))
                {
                    var rect = Rectangle.Intersect(new Rectangle(area.XStart, area.YStart, area.XEnd - area.XStart, area.YEnd - area.YStart), new Rectangle(0, 0, full.Width, full.Height));
                    if (rect.Width <= 0 || rect.Height <= 0)
                    {
                        rect = new Rectangle(0, 0, full.Width, full.Height);
                    }
                    return full.Clone(rect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                }
            }
            catch (Exception ex)
            {
                text = "can't be read: " + ex.Message;
                return null;
            }
        }

        private void ShowItem()
        {
            List<Row> marked = Marked(listViewItems, _itemView);
            Row row = marked.FirstOrDefault();
            if (row == null || _client == null || _busy)
            {
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine($"Item {row.Id} (0x{row.Id:X4})" + (marked.Count > 1 ? $"   – {marked.Count} marked" : string.Empty));
            using (Bitmap classic = row.Classic ? ClassicItemImage(row.Id) : null)
            {
                SetImage(pictureBoxClassicItems, Zoom(classic, pictureBoxClassicItems.Size));
                if (classic != null)
                {
                    sb.AppendLine($"Classic: {classic.Width} x {classic.Height}, \"{ClassicItemName(row.Id)}\"");
                    ShowPreview(classic);
                }
                else
                {
                    SetImage(pictureBoxPreviewItems, null);
                    sb.AppendLine("Classic: no image");
                }
            }
            using (Bitmap now = EcItemImageNow((uint)row.Id, out string text))
            {
                SetImage(pictureBoxEcItems, now == null ? null : new Bitmap(now));
                sb.AppendLine($"EC now: {text}" + (now != null ? $" {now.Width} x {now.Height}" : string.Empty));
            }
            sb.AppendLine($"State: {row.State}");
            labelInfoItems.Text = sb.ToString();
        }

        private void ShowPreview(Bitmap classic)
        {
            if (classic == null)
            {
                SetImage(pictureBoxPreviewItems, null);
                return;
            }
            byte[] px = EcImageHelper.ToBgra(classic, out int w, out int h);
            if (radioButtonLegacyOnly.Checked)
            {
                // the client enlarges the legacy texture itself
                byte[] canvas = EcArtConverter.LegacyCanvas(px, w, h, out int cw);
                SetImage(pictureBoxPreviewItems, EcImageHelper.ToBitmap(canvas, cw, h));
                return;
            }
            byte[] scaled = EcArtScaler.Scale(px, w, h, (ArtScaleMode)Math.Max(0, comboBoxScale.SelectedIndex), out int sw, out int sh);
            SetImage(pictureBoxPreviewItems, EcImageHelper.ToBitmap(scaled, sw, sh));
        }

        private void ShowGump()
        {
            List<Row> marked = Marked(listViewGumps, _gumpView);
            Row row = marked.FirstOrDefault();
            if (row == null || _client == null || _busy)
            {
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine($"Gump {row.Id} (0x{row.Id:X4})" + (marked.Count > 1 ? $"   – {marked.Count} marked" : string.Empty));
            using (Bitmap classic = row.Classic ? ClassicGumpImage(row.Id) : null)
            {
                SetImage(pictureBoxClassicGumps, classic == null ? null : new Bitmap(classic));
                sb.AppendLine(classic != null ? $"Classic: {classic.Width} x {classic.Height}" : "Classic: no image");
            }
            try
            {
                EcUopPackage package = _client.GetPackage(EcArtConverter.GumpFile);
                byte[] data = package?.ReadData(string.Format(CultureInfo.InvariantCulture, EcArtConverter.GumpPattern, row.Id));
                if (data != null && EcLegacyGump.IsLegacyGump(data))
                {
                    byte[] px = EcLegacyGump.Decode(data, out int w, out int h);
                    SetImage(pictureBoxEcGumps, w > 0 && h > 0 ? EcImageHelper.ToBitmap(px, w, h) : null);
                    sb.AppendLine(w > 0 ? $"EC now: {w} x {h}" : "EC now: empty");
                }
                else
                {
                    SetImage(pictureBoxEcGumps, null);
                    sb.AppendLine("EC now: missing");
                }
            }
            catch (Exception ex)
            {
                SetImage(pictureBoxEcGumps, null);
                sb.AppendLine("EC now: can't be read: " + ex.Message);
            }
            labelInfoGumps.Text = sb.ToString();
        }

        // ==================================================================== converting

        private async void ConvertItems()
        {
            if (!NeedClient())
            {
                return;
            }
            List<Row> rows = Marked(listViewItems, _itemView).Where(r => r.Classic).ToList();
            if (rows.Count == 0)
            {
                EcDialogs.ShowError(this, "Mark classic items in the list first (\"Check\", then a filter such as \"Missing + no image\").");
                return;
            }
            ArtConvertOptions options = Options();
            int existing = rows.Count(r => r.StateCode != (int)EcItemState.Missing);
            if (existing > 0 && !EcDialogs.Confirm(this, $"{existing} of the {rows.Count} marked items exist in the EC already. Replace their images?"))
            {
                return;
            }
            var log = new StringBuilder();
            await Run($"Convert {rows.Count} items", progress =>
            {
                int done = 0, failed = 0;
                foreach (Row row in rows)
                {
                    progress.Report(done++ * 100 / rows.Count);
                    try
                    {
                        using (Bitmap image = ClassicItemImage(row.Id))
                        {
                            if (image == null)
                            {
                                throw new InvalidOperationException("no classic image");
                            }
                            byte[] px = EcImageHelper.ToBgra(image, out int w, out int h);
                            log.AppendLine(_converter.ConvertItem((uint)row.Id, px, w, h, ClassicInfo(row.Id), options));
                        }
                        EcItemState state = _converter.GetState((uint)row.Id);
                        row.StateCode = (int)state;
                        row.State = EcArtConverter.StateText(state);
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        log.AppendLine($"Item {row.Id}: ERROR {ex.Message}");
                    }
                }
                return $"{rows.Count - failed} items converted" + (failed > 0 ? $", {failed} failed" : string.Empty) + " – \"Save\" writes the files.";
            });
            AppendReport("=== Convert items\r\n" + log);
            ApplyItemFilter();
            UpdateSaveButton();
        }

        private async void ConvertGumps()
        {
            if (!NeedClient())
            {
                return;
            }
            List<Row> rows = Marked(listViewGumps, _gumpView).Where(r => r.Classic).ToList();
            if (rows.Count == 0)
            {
                EcDialogs.ShowError(this, "Mark classic gumps in the list first (\"Check\", then the filter \"Missing in EC\").");
                return;
            }
            int existing = rows.Count(r => r.StateCode == (int)GumpState.Present);
            if (existing > 0 && !EcDialogs.Confirm(this, $"{existing} of the {rows.Count} marked gumps exist in the EC already. Replace them?"))
            {
                return;
            }
            var log = new StringBuilder();
            await Run($"Convert {rows.Count} gumps", progress =>
            {
                int done = 0, failed = 0;
                foreach (Row row in rows)
                {
                    progress.Report(done++ * 100 / rows.Count);
                    try
                    {
                        using (Bitmap image = ClassicGumpImage(row.Id))
                        {
                            if (image == null)
                            {
                                throw new InvalidOperationException("no classic image");
                            }
                            byte[] px = EcImageHelper.ToBgra(image, out int w, out int h);
                            log.AppendLine(_converter.ConvertGump(row.Id, px, w, h));
                        }
                        row.StateCode = (int)GumpState.Present;
                        row.State = GumpStateText(GumpState.Present);
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        log.AppendLine($"Gump {row.Id}: ERROR {ex.Message}");
                    }
                }
                return $"{rows.Count - failed} gumps converted" + (failed > 0 ? $", {failed} failed" : string.Empty) + " – \"Save\" writes the files.";
            });
            AppendReport("=== Convert gumps\r\n" + log);
            ApplyGumpFilter();
            UpdateSaveButton();
        }

        /// <summary>The id for an imported image: the selected row, or one typed in (existing or free).</summary>
        private bool AskId(Row selected, string what, out int id)
        {
            id = -1;
            string text = EcDialogs.Prompt(this, $"Import {what}", $"Id for the image (decimal or 0x hex; existing or free):", selected != null ? $"0x{selected.Id:X4}" : string.Empty);
            if (text == null || !EcDialogs.TryParseNumber(text, out long value) || value < 0 || value >= 0x20000)
            {
                return false;
            }
            id = (int)value;
            return true;
        }

        private static byte[] LoadImage(IWin32Window owner, out int w, out int h, out string file)
        {
            w = h = 0;
            file = null;
            using (var dialog = new OpenFileDialog { Filter = "Images (*.png;*.bmp;*.gif;*.tga;*.dds;*.jpg)|*.png;*.bmp;*.gif;*.tga;*.dds;*.jpg|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return null;
                }
                file = dialog.FileName;
                using (Bitmap image = EcImageHelper.LoadImageFile(dialog.FileName))
                {
                    return EcImageHelper.ToBgra(image, out w, out h);
                }
            }
        }

        private void ImportItem()
        {
            if (!NeedClient())
            {
                return;
            }
            Row selected = Marked(listViewItems, _itemView).FirstOrDefault();
            if (!AskId(selected, "item", out int id))
            {
                return;
            }
            try
            {
                byte[] px = LoadImage(this, out int w, out int h, out string file);
                if (px == null)
                {
                    return;
                }
                bool exists = _client.GetTileArt()?.Get((uint)id) != null;
                if (exists && !EcDialogs.Confirm(this, $"Item {id} (0x{id:X4}) exists in the EC. Replace its image?"))
                {
                    return;
                }
                ArtConvertOptions options = Options();
                ClassicItemInfo info = ClassicInfo(id) ?? new ClassicItemInfo { Name = Path.GetFileNameWithoutExtension(file) };
                if (!exists && string.IsNullOrWhiteSpace(info.Name))
                {
                    info.Name = Path.GetFileNameWithoutExtension(file);
                }
                // own images: the tiledata only for new entries (unless chosen)
                string result;
                if (checkBoxImportEcSize.Checked)
                {
                    byte[] classicPx = EcArtConverter.ToClassicSize(px, w, h, out int cw, out int ch);
                    result = _converter.ConvertItem((uint)id, classicPx, cw, ch, info, options, px, w, h);
                }
                else
                {
                    result = _converter.ConvertItem((uint)id, px, w, h, info, options);
                }
                AppendReport("=== Import item image\r\n" + result + $"  ({file})");
                toolStripStatusLabel.Text = result + " – \"Save\" writes the files.";
                UpdateRow(_items, id, true);
                ApplyItemFilter();
                UpdateSaveButton();
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Import failed.", ex);
            }
        }

        private void ImportGump()
        {
            if (!NeedClient())
            {
                return;
            }
            Row selected = Marked(listViewGumps, _gumpView).FirstOrDefault();
            if (!AskId(selected, "gump", out int id))
            {
                return;
            }
            try
            {
                byte[] px = LoadImage(this, out int w, out int h, out string file);
                if (px == null)
                {
                    return;
                }
                if (_converter.GetGumpState(id) == GumpState.Present && !EcDialogs.Confirm(this, $"Gump {id} (0x{id:X4}) exists in the EC. Replace it?"))
                {
                    return;
                }
                string result = _converter.ConvertGump(id, px, w, h);
                AppendReport("=== Import gump image\r\n" + result + $"  ({file})");
                toolStripStatusLabel.Text = result + " – \"Save\" writes the files.";
                UpdateRow(_gumps, id, false);
                ApplyGumpFilter();
                UpdateSaveButton();
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Import failed.", ex);
            }
        }

        private void UpdateRow(List<Row> rows, int id, bool item)
        {
            Row row = rows.FirstOrDefault(r => r.Id == id);
            if (row == null)
            {
                row = new Row { Id = id, Classic = item ? ClassicItemValid(id) : ClassicGumpValid(id), Name = string.Empty };
                rows.Add(row);
                rows.Sort((a, b) => a.Id.CompareTo(b.Id));
            }
            if (item)
            {
                EcItemState state = _converter.GetState((uint)id);
                row.StateCode = (int)state;
                row.State = EcArtConverter.StateText(state);
            }
            else
            {
                GumpState state = _converter.GetGumpState(id);
                row.StateCode = (int)state;
                row.State = GumpStateText(state);
            }
        }

        // ==================================================================== report

        private async void CreateReport()
        {
            if (!NeedClient())
            {
                return;
            }
            List<Row> items = null, gumps = null;
            await Run("Report", progress =>
            {
                items = ScanItems(new Progress<int>(p => progress.Report(p / 2)));
                gumps = ScanGumps(new Progress<int>(p => progress.Report(50 + (p / 2))));
                return "Report created.";
            });
            if (items == null)
            {
                return;
            }
            _items = items;
            _gumps = gumps;
            ApplyItemFilter();
            ApplyGumpFilter();

            var sb = new StringBuilder();
            sb.AppendLine("WHAT IS MISSING IN THE ENHANCED CLIENT");
            sb.AppendLine($"Created {DateTime.Now:yyyy-MM-dd HH:mm}   EC: {_client.Folder}");
            sb.AppendLine();
            sb.AppendLine("ITEMS: " + Summary(items, true));
            sb.AppendLine("  Missing in EC (classic item, no tileart entry):");
            sb.AppendLine(Ranges(items.Where(r => r.Classic && r.StateCode == (int)EcItemState.Missing).Select(r => r.Id)));
            sb.AppendLine("  No image in EC (tileart entry without texture – invisible):");
            sb.AppendLine(Ranges(items.Where(r => r.Classic && r.StateCode == (int)EcItemState.NoImage).Select(r => r.Id)));
            sb.AppendLine("  Only in EC (not in the classic files):");
            sb.AppendLine(Ranges(items.Where(r => !r.Classic).Select(r => r.Id)));
            sb.AppendLine();
            sb.AppendLine("GUMPS: " + Summary(gumps, false));
            sb.AppendLine("  Missing in EC (gumpartLegacyMUL.uop):");
            sb.AppendLine(Ranges(gumps.Where(r => r.Classic && r.StateCode == (int)GumpState.Missing).Select(r => r.Id)));
            sb.AppendLine("  Empty in EC:");
            sb.AppendLine(Ranges(gumps.Where(r => r.Classic && r.StateCode == (int)GumpState.Empty).Select(r => r.Id)));
            textBoxReport.Text = sb.ToString().Replace("\n", "\r\n").Replace("\r\r", "\r");
            tabControl.SelectedTab = tabPageReport;
        }

        /// <summary>Ids as ranges in hex and decimal ("0x4000-0x40FF (16384-16639)"), 4 per line.</summary>
        private static string Ranges(IEnumerable<int> ids)
        {
            List<int> list = ids.OrderBy(i => i).ToList();
            if (list.Count == 0)
            {
                return "    (none)";
            }
            var parts = new List<string>();
            int start = list[0], prev = list[0];
            foreach (int id in list.Skip(1).Concat(new[] { int.MinValue }))
            {
                if (id == prev + 1)
                {
                    prev = id;
                    continue;
                }
                parts.Add(start == prev ? $"0x{start:X4} ({start})" : $"0x{start:X4}-0x{prev:X4} ({start}-{prev}, {prev - start + 1})");
                start = prev = id;
            }
            var sb = new StringBuilder();
            sb.AppendLine($"    {list.Count} ids:");
            for (int i = 0; i < parts.Count; i += 4)
            {
                sb.AppendLine("    " + string.Join("   ", parts.Skip(i).Take(4)));
            }
            return sb.ToString().TrimEnd();
        }

        private void AppendReport(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendReport), text);
                return;
            }
            textBoxReport.AppendText(text.Replace("\r\n", "\n").Replace("\n", Environment.NewLine).TrimEnd() + Environment.NewLine);
        }

        private void SaveReport()
        {
            using (var dialog = new SaveFileDialog { Filter = "Text|*.txt", FileName = "MissingInEC.txt" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, textBoxReport.Text);
                }
            }
        }

        // ==================================================================== saving / jobs

        private void Save(bool saveAs)
        {
            if (_client == null)
            {
                return;
            }
            _client.ApplyPendingChanges();
            EcPackageSaver.Save(this, _client.OpenPackages, saveAs);
            UpdateSaveButton();
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            tabControl.Enabled = !busy;
            toolStrip.Enabled = !busy;
            if (!busy)
            {
                toolStripProgressBar.Value = 0;
            }
        }

        private async Task Run(string title, Func<IProgress<int>, string> job)
        {
            var progress = new Progress<int>(p => toolStripProgressBar.Value = Math.Max(0, Math.Min(100, p)));
            SetBusy(true);
            toolStripStatusLabel.Text = title + "...";
            try
            {
                string result = await Task.Run(() => job(progress));
                toolStripStatusLabel.Text = result;
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = $"{title}: error";
                EcDialogs.ShowError(this, $"{title} failed.", ex);
            }
            finally
            {
                SetBusy(false);
                UpdateSaveButton();
            }
        }

        private void ShowHelp()
        {
            richTextBoxHelp.Text = radioButtonHelpGerman.Checked ? ArtConverterHelp.German : ArtConverterHelp.English;
        }

        private void OnControlDisposed(object sender, EventArgs e)
        {
            foreach (PictureBox box in new[] { pictureBoxClassicItems, pictureBoxEcItems, pictureBoxPreviewItems, pictureBoxClassicGumps, pictureBoxEcGumps })
            {
                SetImage(box, null);
            }
        }

        // ==================================================================== event handlers (designer)

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) => e.DrawDefault = true;

        private void OnClickScanItems(object sender, EventArgs e) => ScanItemsAsync();

        private void OnFilterChangedItems(object sender, EventArgs e) => ApplyItemFilter();

        private void OnSearchKeyDownItems(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyItemFilter();
            }
        }

        private void OnClickConvertItems(object sender, EventArgs e) => ConvertItems();

        private void OnClickImportItems(object sender, EventArgs e) => ImportItem();

        private void RetrieveItemItems(object sender, RetrieveVirtualItemEventArgs e) => e.Item = MakeItem(_itemView, e.ItemIndex);

        private void DrawSubItemItems(object sender, DrawListViewSubItemEventArgs e) => DrawRow(_itemView, e, true);

        private void OnSelectionChangedItems(object sender, EventArgs e) => ShowItem();

        private void OnSelectionRangeChangedItems(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e) => ShowItem();

        private void OnClickMarkAllItems(object sender, EventArgs e) => MarkAll(listViewItems, _itemView.Count);

        private void OnClickCopyIdsItems(object sender, EventArgs e) => CopyIds(Marked(listViewItems, _itemView));

        private void OnClickScanGumps(object sender, EventArgs e) => ScanGumpsAsync();

        private void OnFilterChangedGumps(object sender, EventArgs e) => ApplyGumpFilter();

        private void OnSearchKeyDownGumps(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyGumpFilter();
            }
        }

        private void OnClickConvertGumps(object sender, EventArgs e) => ConvertGumps();

        private void OnClickImportGumps(object sender, EventArgs e) => ImportGump();

        private void RetrieveItemGumps(object sender, RetrieveVirtualItemEventArgs e) => e.Item = MakeItem(_gumpView, e.ItemIndex);

        private void DrawSubItemGumps(object sender, DrawListViewSubItemEventArgs e) => DrawRow(_gumpView, e, false);

        private void OnSelectionChangedGumps(object sender, EventArgs e) => ShowGump();

        private void OnSelectionRangeChangedGumps(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e) => ShowGump();

        private void OnClickMarkAllGumps(object sender, EventArgs e) => MarkAll(listViewGumps, _gumpView.Count);

        private void OnClickCopyIdsGumps(object sender, EventArgs e) => CopyIds(Marked(listViewGumps, _gumpView));

        private void CopyIds(List<Row> rows)
        {
            if (rows.Count > 0)
            {
                Clipboard.SetText(string.Join(Environment.NewLine, rows.Select(r => $"0x{r.Id:X4}")));
            }
        }

        private void OnOptionsChanged(object sender, EventArgs e)
        {
            comboBoxScale.Enabled = radioButtonLegacyAndEc.Checked;
            ShowItem();
        }

        private void OnClickReport(object sender, EventArgs e) => CreateReport();

        private void OnClickSaveReport(object sender, EventArgs e) => SaveReport();

        private void OnHelpLanguageChanged(object sender, EventArgs e) => ShowHelp();
    }
}
