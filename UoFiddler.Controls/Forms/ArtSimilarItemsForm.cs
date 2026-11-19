/***************************************************************************
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
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Finds items in art.mul that look like the source (in any facing) and shows their direction groups
    /// (neighbouring ids with the same name). From a group the generator learns the object type:
    /// only copies / mirrors -> wall-like, a real new drawing -> pointing object.
    /// </summary>
    public sealed class ArtSimilarItemsForm : Form
    {
        private readonly ArtFeatures _query;
        private readonly int _excludeId;
        private readonly Action<ArtObjectKind, double, string> _applyKind; // kind, learned light swap (-1 = unknown), text
        private readonly Action<int> _loadAsSource;

        private readonly ListView _list;
        private readonly ImageList _thumbs;
        private readonly FlowLayoutPanel _groupPanel;
        private readonly Label _status;
        private readonly Label _groupInfo;
        private ArtObjectKind? _learnedKind;
        private double _learnedRelight = -1;
        private string _learnedText;

        public ArtSimilarItemsForm(ArtPixelImage source, int sourceId, Action<ArtObjectKind, double, string> applyKind, Action<int> loadAsSource)
        {
            _query = ArtFeatures.Compute(source, sourceId);
            _excludeId = sourceId;
            _applyKind = applyKind;
            _loadAsSource = loadAsSource;

            Text = "Similar existing items";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 640);

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 560 };
            Controls.Add(split);

            _thumbs = new ImageList { ImageSize = new Size(64, 64), ColorDepth = ColorDepth.Depth32Bit };
            _list = new ListView { Dock = DockStyle.Fill, View = View.LargeIcon, LargeImageList = _thumbs, MultiSelect = false, HideSelection = false };
            _list.SelectedIndexChanged += (s, e) => ShowGroup();
            _list.DoubleClick += (s, e) => LoadSelected();
            split.Panel1.Controls.Add(_list);

            var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _groupInfo = new Label { AutoSize = true, MaximumSize = new Size(400, 0), Margin = new Padding(3, 6, 3, 6) };
            right.Controls.Add(_groupInfo, 0, 0);
            _groupPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
            right.Controls.Add(_groupPanel, 0, 1);

            var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            var apply = new Button { Text = "Use the learned settings", AutoSize = true };
            apply.Click += (s, e) =>
            {
                if (_learnedKind.HasValue)
                {
                    _applyKind?.Invoke(_learnedKind.Value, _learnedRelight, _learnedText);
                }
            };
            var load = new Button { Text = "Load selected as source", AutoSize = true };
            load.Click += (s, e) => LoadSelected();
            buttons.Controls.Add(apply);
            buttons.Controls.Add(load);
            right.Controls.Add(buttons, 0, 2);
            split.Panel2.Controls.Add(right);

            _status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft };
            Controls.Add(_status);

            ArtSimilarityIndex.Instance.Changed += OnIndexChanged;
            FormClosed += (s, e) => ArtSimilarityIndex.Instance.Changed -= OnIndexChanged;
            Shown += (s, e) =>
            {
                ArtSimilarityIndex.Instance.EnsureBuilt();
                OnIndexChanged();
            };
        }

        private bool _filled;

        private void OnIndexChanged()
        {
            ArtSimilarityIndex index = ArtSimilarityIndex.Instance;
            if (!index.IsReady)
            {
                _status.Text = index.Total > 0
                    ? $"Building the art index (only once, then cached): {index.Done:N0} / {index.Total:N0} ..."
                    : "Building the art index ...";
                return;
            }

            if (_filled)
            {
                return;
            }

            _filled = true;
            Fill();
        }

        private void Fill()
        {
            _list.BeginUpdate();
            _list.Items.Clear();
            _thumbs.Images.Clear();
            List<(ArtFeatures Item, double Distance)> found = ArtSimilarityIndex.Instance.FindSimilar(_query, 48, _excludeId);
            foreach ((ArtFeatures item, double distance) in found)
            {
                ArtPixelImage img = ArtDirectionLearning.DecodeStaticRaw(Art.GetRawStatic(item.Id));
                _thumbs.Images.Add(Thumbnail(img));
                string name = ArtSimilarityIndex.GetName(item.Id);
                _list.Items.Add(new ListViewItem($"0x{item.Id:X4} {ArtFeatures.Similarity(distance)}%\n{name}", _thumbs.Images.Count - 1) { Tag = item.Id });
            }

            _list.EndUpdate();
            _status.Text = $"{found.Count} similar items (facing ignored). Click one to see its direction group, double-click to load it as source.";
            if (_list.Items.Count > 0)
            {
                _list.Items[0].Selected = true;
            }
        }

        private void ShowGroup()
        {
            // detach the images before disposing them (a PictureBox showing a disposed image crashes on WM_SHOWWINDOW)
            var old = _groupPanel.Controls.OfType<PictureBox>().ToList();
            _groupPanel.Controls.Clear();
            foreach (PictureBox pb in old)
            {
                Image img = pb.Image;
                pb.Image = null;
                img?.Dispose();
                pb.Dispose();
            }

            _learnedKind = null;
            _learnedRelight = -1;
            if (_list.SelectedItems.Count == 0 || _list.SelectedItems[0].Tag is not int id)
            {
                return;
            }

            List<int> group = ArtSimilarityIndex.Instance.GetGroup(id);
            var images = group.ToDictionary(g => g, g => ArtDirectionLearning.DecodeStaticRaw(Art.GetRawStatic(g)));
            ArtPixelImage first = images[group[0]];

            var lines = new List<string>();
            var relations = new List<ArtVariantRelation>();
            foreach (int g in group)
            {
                ArtPixelImage img = images[g];
                if (img == null)
                {
                    continue;
                }

                string text = $"0x{g:X4}";
                if (g != group[0] && first != null)
                {
                    ArtVariantRelation rel = ArtDirectionLearning.Relation(first, img, out double score);
                    relations.Add(rel);
                    text += rel switch
                    {
                        ArtVariantRelation.Copy => $"  = copy of 0x{group[0]:X4}",
                        ArtVariantRelation.Mirror => $"  = mirror of 0x{group[0]:X4}",
                        _ => "  = drawn new"
                    };
                }

                ArtSuggestion dir = Suggest(g, img);
                text += $"  ({dir.Direction})";
                lines.Add(text);

                var pb = new PictureBox { Image = Thumbnail(img, 96), SizeMode = PictureBoxSizeMode.AutoSize, Margin = new Padding(4) };
                new ToolTip().SetToolTip(pb, text);
                _groupPanel.Controls.Add(pb);
            }

            if (group.Count > 1)
            {
                // wall or box? the TileData of the group (wall / door / window flag) or a slim upright shape decides
                ArtSuggestion firstKind = Suggest(group[0], first);
                bool wallLike = firstKind.Kind == ArtObjectKind.Symmetric;
                _learnedKind = ArtDirectionLearning.KindFromRelations(relations, wallLike);

                // light: how did the artist light the mirrored variant? (learned from the first mirror pair)
                string lightText = string.Empty;
                int mirrorIndex = relations.FindIndex(r => r == ArtVariantRelation.Mirror);
                if (mirrorIndex >= 0 && first != null)
                {
                    int partner = group.Where(g => g != group[0]).ElementAt(mirrorIndex);
                    _learnedRelight = ArtDirectionLearning.LearnRelightStrength(first, images[partner], _learnedKind.Value, out double diff);
                    if (_learnedRelight >= 0)
                    {
                        lightText = $"{Environment.NewLine}Learned light swap: {_learnedRelight:P0} (closest to 0x{partner:X4}, brightness difference {diff:0.0})";
                    }
                }
                _learnedText = $"learned from the group 0x{group[0]:X4}-0x{group[^1]:X4}";
                _groupInfo.Text = $"Direction group ({group.Count} items, same name):{Environment.NewLine}{string.Join(Environment.NewLine, lines)}" +
                                  $"{Environment.NewLine}{Environment.NewLine}Learned object type: {KindText(_learnedKind.Value)}{lightText}";
            }
            else
            {
                ArtSuggestion s = Suggest(id, images[id]);
                _learnedKind = s.Kind;
                _learnedText = $"from 0x{id:X4} ({s.Reason})";
                _groupInfo.Text = $"No direction group found next to 0x{id:X4} (no neighbours with the same name).{Environment.NewLine}" +
                                  $"Object type from its TileData/shape: {KindText(s.Kind)}";
            }
        }

        private static ArtSuggestion Suggest(int id, ArtPixelImage img)
        {
            bool has = TileData.ItemTable != null && id >= 0 && id < TileData.ItemTable.Length;
            ItemData d = has ? TileData.ItemTable[id] : default;
            return ArtDirectionLearning.Suggest(img ?? new ArtPixelImage(1, 1), has,
                has && (d.Flags & TileFlag.Wall) != 0, has && (d.Flags & TileFlag.Door) != 0, has && (d.Flags & TileFlag.Window) != 0,
                has && (d.Flags & TileFlag.Surface) != 0, has && (d.Flags & TileFlag.Foliage) != 0, has ? d.Height : 1,
                has && (d.Flags & TileFlag.Container) != 0);
        }

        private static string KindText(ArtObjectKind k) => k switch
        {
            ArtObjectKind.Symmetric => "Wall / fence (front = back)",
            ArtObjectKind.Flat => "Flat on the ground",
            ArtObjectKind.Directional => "Directional (points somewhere)",
            ArtObjectKind.Box => "Box / furniture (same footprint)",
            _ => "Solid object"
        };

        private void LoadSelected()
        {
            if (_list.SelectedItems.Count > 0 && _list.SelectedItems[0].Tag is int id)
            {
                _loadAsSource?.Invoke(id);
            }
        }

        private static Bitmap Thumbnail(ArtPixelImage img, int size = 64)
        {
            var bmp = new Bitmap(size, size);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            if (img == null)
            {
                return bmp;
            }

            using Bitmap art = ArtDirectionForm.ToBitmap(img);
            float scale = Math.Min(1f, Math.Min(size / (float)img.Width, size / (float)img.Height));
            int w = Math.Max(1, (int)(img.Width * scale)), h = Math.Max(1, (int)(img.Height * scale));
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.DrawImage(art, new Rectangle((size - w) / 2, size - h, w, h));
            return bmp;
        }
    }
}
