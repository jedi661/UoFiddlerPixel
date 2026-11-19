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
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Decides where several graphics (an imported sprite sheet, or the generated directions) go:
    /// from an address (exactly / next free / block of free slots), browse the free blocks with the arrows,
    /// take the slots selected in the Items tab, or type every id by hand. Then insert all.
    /// </summary>
    public sealed class SpriteSheetPlacementForm : Form
    {
        private readonly List<(string Label, ArtPixelImage Image)> _items;
        private readonly Action<int, Bitmap> _insert;
        private readonly SlotPlanner _planner;

        private readonly ListView _list;
        private readonly ImageList _thumbs;
        private readonly TextBox _startBox;
        private readonly ComboBox _modeBox;
        private readonly Label _status;
        private readonly List<int> _ids = new List<int>();

        public SpriteSheetPlacementForm(List<(string Label, ArtPixelImage Image)> items, int suggestedStart, Action<int, Bitmap> insert)
        {
            _items = items;
            _insert = insert;
            _planner = new SlotPlanner(id => !Art.IsValidStatic(id), Art.IsLegalStaticId, Art.GetMaxExtendedItemId());
            _ids.AddRange(Enumerable.Repeat(-1, items.Count));

            Text = $"Place {items.Count} graphic(s)";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(760, 600);
            MinimumSize = new Size(600, 400);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(layout);

            // --- placement controls
            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            top.Controls.Add(Lbl("Address:"));
            _startBox = new TextBox { Width = 90, Text = $"0x{Math.Max(1, suggestedStart):X4}" };
            new ToolTip().SetToolTip(_startBox, "Start id (hex 0x... or decimal)");
            top.Controls.Add(_startBox);
            _modeBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230 };
            _modeBox.Items.AddRange(new object[]
            {
                "Exactly from the address (in a row)",
                "Next free slots from the address",
                $"Block of {items.Count} free slots in a row"
            });
            _modeBox.SelectedIndex = 2;
            top.Controls.Add(_modeBox);
            top.Controls.Add(Btn("Apply", (s, e) => ApplyPlan()));
            var prev = Btn("◀", (s, e) => BrowseBlock(-1));
            var next = Btn("▶", (s, e) => BrowseBlock(1));
            new ToolTip().SetToolTip(prev, "Previous block of free slots in a row");
            new ToolTip().SetToolTip(next, "Next block of free slots in a row");
            top.Controls.Add(prev);
            top.Controls.Add(next);
            top.Controls.Add(Btn("Use selection from Items tab", (s, e) => UseItemsSelection()));
            layout.Controls.Add(top, 0, 0);

            // --- list
            _thumbs = new ImageList { ImageSize = new Size(48, 48), ColorDepth = ColorDepth.Depth32Bit };
            _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, SmallImageList = _thumbs, LabelEdit = false, HideSelection = false };
            _list.Columns.Add("Graphic", 170);
            _list.Columns.Add("Target id", 110);
            _list.Columns.Add("Size", 80);
            _list.Columns.Add("Slot", 260);
            _list.SelectedIndexChanged += (s, e) => ShowSelectedInItems();
            _list.DoubleClick += (s, e) => EditSelectedId();
            for (int i = 0; i < items.Count; ++i)
            {
                _thumbs.Images.Add(Thumbnail(items[i].Image));
                var row = new ListViewItem(items[i].Label, i);
                row.SubItems.Add(string.Empty);
                row.SubItems.Add(items[i].Image == null ? "-" : $"{items[i].Image.Width}x{items[i].Image.Height}");
                row.SubItems.Add(string.Empty);
                _list.Items.Add(row);
            }

            layout.Controls.Add(_list, 0, 1);

            var hint = Lbl("Click a row = show the slot in the Items tab.  Double click = type the id of this graphic.  " +
                           "In the Items tab you can also mark free slots (Ctrl+click) and use 'Use selection from Items tab'.");
            hint.MaximumSize = new Size(720, 0);
            hint.ForeColor = Color.DimGray;
            layout.Controls.Add(hint, 0, 2);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            bottom.Controls.Add(Btn("Insert all", (s, e) => InsertAll()));
            bottom.Controls.Add(Btn("Close", (s, e) => Close()));
            _status = Lbl(string.Empty);
            bottom.Controls.Add(_status);
            layout.Controls.Add(bottom, 0, 3);

            Shown += (s, e) => ApplyPlan();
        }

        private static Button Btn(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            return b;
        }

        private static Label Lbl(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 3) };

        private int CountWithImage => _items.Count(i => i.Image != null);

        private bool TryStart(out int start)
        {
            return Utils.ConvertStringToInt(_startBox.Text, out start, 0, Art.GetMaxExtendedItemId());
        }

        /// <summary>Assigns ids to the graphics (empty cells get none).</summary>
        private void SetIds(IReadOnlyList<int> ids)
        {
            int k = 0;
            for (int i = 0; i < _items.Count; ++i)
            {
                _ids[i] = _items[i].Image != null && k < ids.Count ? ids[k++] : -1;
            }

            RefreshRows();
        }

        private void ApplyPlan()
        {
            if (!TryStart(out int start))
            {
                _status.Text = $"Invalid address '{_startBox.Text}'.";
                return;
            }

            List<int> ids = _planner.Plan((SlotPlacementMode)_modeBox.SelectedIndex, start, CountWithImage);
            if (ids == null)
            {
                _status.Text = "No room for all graphics from this address.";
                return;
            }

            SetIds(ids);
            _status.Text = $"{ids.Count} slot(s): 0x{ids[0]:X4} - 0x{ids[^1]:X4}.";
        }

        /// <summary>Jumps to the previous / next block of free slots in a row (as many as graphics).</summary>
        private void BrowseBlock(int direction)
        {
            int n = CountWithImage;
            int current = _ids.Where(id => id >= 0).DefaultIfEmpty(TryStart(out int s) ? s : 0).Min();
            int block = direction > 0 ? _planner.NextBlock(current, n) : _planner.PreviousBlock(current, n);
            if (block < 0)
            {
                _status.Text = direction > 0 ? "No further free block." : "No free block before.";
                return;
            }

            _modeBox.SelectedIndex = (int)SlotPlacementMode.FreeBlock;
            _startBox.Text = $"0x{block:X4}";
            SetIds(SlotPlanner.Range(block, n));
            ItemsControl.ShowGraphic(block);
            _status.Text = $"Free block 0x{block:X4} - 0x{block + n - 1:X4} (shown in the Items tab).";
        }

        private void UseItemsSelection()
        {
            IReadOnlyList<int> sel = ItemsControl.GetSelectedGraphicIds();
            if (sel.Count == 0)
            {
                _status.Text = "Nothing selected in the Items tab (mark slots there, Ctrl+click for several).";
                return;
            }

            SetIds(sel);
            _status.Text = sel.Count < CountWithImage
                ? $"Only {sel.Count} slot(s) selected for {CountWithImage} graphics - the rest has no id yet."
                : $"{Math.Min(sel.Count, CountWithImage)} slot(s) from the Items tab.";
        }

        private void EditSelectedId()
        {
            if (_list.SelectedIndices.Count == 0)
            {
                return;
            }

            int i = _list.SelectedIndices[0];
            string input = Microsoft.VisualBasic.Interaction.InputBox($"Target id for '{_items[i].Label}' (hex 0x... or decimal):",
                "Target id", _ids[i] >= 0 ? $"0x{_ids[i]:X4}" : "0x");
            if (Utils.ConvertStringToInt(input, out int id, 0, Art.GetMaxExtendedItemId()))
            {
                _ids[i] = id;
                RefreshRows();
                ItemsControl.ShowGraphic(id);
            }
        }

        private void ShowSelectedInItems()
        {
            if (_list.SelectedIndices.Count > 0 && _ids[_list.SelectedIndices[0]] >= 0)
            {
                ItemsControl.ShowGraphic(_ids[_list.SelectedIndices[0]]);
            }
        }

        private void RefreshRows()
        {
            var seen = new HashSet<int>();
            for (int i = 0; i < _items.Count; ++i)
            {
                int id = _ids[i];
                ListViewItem row = _list.Items[i];
                row.SubItems[1].Text = id >= 0 ? $"0x{id:X4}" : "-";
                string state;
                Color color;
                if (_items[i].Image == null)
                {
                    state = "empty cell - skipped";
                    color = Color.Gray;
                }
                else if (id < 0)
                {
                    state = "no id";
                    color = Color.DarkOrange;
                }
                else if (!Art.IsLegalStaticId(id))
                {
                    state = "invalid id";
                    color = Color.Firebrick;
                }
                else if (!seen.Add(id))
                {
                    state = "id used twice!";
                    color = Color.Firebrick;
                }
                else if (Art.IsValidStatic(id))
                {
                    string name = TileData.ItemTable != null && id < TileData.ItemTable.Length ? (TileData.ItemTable[id].Name ?? string.Empty).Trim() : string.Empty;
                    state = $"used ({name}) - will be replaced";
                    color = Color.DarkOrange;
                }
                else
                {
                    state = "free";
                    color = Color.DarkGreen;
                }

                row.SubItems[3].Text = state;
                row.ForeColor = color;
            }
        }

        private void InsertAll()
        {
            var todo = Enumerable.Range(0, _items.Count).Where(i => _items[i].Image != null && _ids[i] >= 0 && Art.IsLegalStaticId(_ids[i])).ToList();
            if (todo.Count == 0)
            {
                _status.Text = "Nothing to insert.";
                return;
            }

            if (todo.Select(i => _ids[i]).Distinct().Count() != todo.Count)
            {
                _status.Text = "An id is used twice - please fix it first.";
                return;
            }

            int used = todo.Count(i => Art.IsValidStatic(_ids[i]));
            if (used > 0 && MessageBox.Show(this, $"{used} target slot(s) are already used and will be replaced. Continue?",
                    "Insert", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            foreach (int i in todo)
            {
                _insert?.Invoke(_ids[i], ArtDirectionForm.ToBitmap(_items[i].Image));
            }

            RefreshRows();
            _status.Text = $"{todo.Count} graphic(s) inserted. Save the Art to write the files.";
        }

        private static Bitmap Thumbnail(ArtPixelImage img)
        {
            var bmp = new Bitmap(48, 48);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            if (img == null)
            {
                return bmp;
            }

            using Bitmap art = ArtDirectionForm.ToBitmap(img);
            float scale = Math.Min(1f, Math.Min(48f / img.Width, 48f / img.Height));
            int w = Math.Max(1, (int)(img.Width * scale)), h = Math.Max(1, (int)(img.Height * scale));
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.DrawImage(art, new Rectangle((48 - w) / 2, 48 - h, w, h));
            return bmp;
        }
    }
}
