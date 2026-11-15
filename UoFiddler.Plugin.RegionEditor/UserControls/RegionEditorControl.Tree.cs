/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Region editor – region tree: build, search, select, add, delete,
 * duplicate, move (buttons and drag & drop), rename.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.RegionEditor.Classes;
using UoFiddler.Plugin.RegionEditor.Formats;

namespace UoFiddler.Plugin.RegionEditor.UserControls
{
    public partial class RegionEditorControl
    {
        private readonly Dictionary<int, int> _colorImages = new Dictionary<int, int>();

        #region Build

        /// <summary>regions of the shown map; create = add an empty facet to the project</summary>
        private FacetRegions CurrentFacet(bool create)
        {
            int mapId = _mapView.MapId;
            if (mapId < 0)
            {
                return null;
            }

            return _project.GetFacet(mapId, RegionCatalog.FacetName(mapId, _mapView.MapDefinition?.Name), create);
        }

        private void BuildTree()
        {
            FacetRegions facet = CurrentFacet(false);
            string filter = _txtSearch.Text.Trim();
            HashSet<string> expanded = new HashSet<string>(AllNodes(_tree.Nodes).Where(n => n.IsExpanded).Select(n => ((RegionNode)n.Tag).Id));
            bool first = _tree.Nodes.Count == 0;

            _updating = true;
            _tree.BeginUpdate();
            try
            {
                if (_tree.ImageList == null)
                {
                    _tree.ImageList = new ImageList { ImageSize = new Size(12, 12), ColorDepth = ColorDepth.Depth32Bit };
                }

                _tree.Nodes.Clear();
                if (facet != null)
                {
                    AddNodes(_tree.Nodes, facet.Regions, filter);
                }

                foreach (TreeNode n in AllNodes(_tree.Nodes))
                {
                    if (filter.Length > 0 || expanded.Contains(((RegionNode)n.Tag).Id) || (first && n.Level == 0 && n.Nodes.Count < 12))
                    {
                        n.Expand();
                    }
                }

                TreeNode selected = _selected == null ? null : AllNodes(_tree.Nodes).FirstOrDefault(n => n.Tag == _selected);
                _tree.SelectedNode = selected;
                selected?.EnsureVisible();
            }
            finally
            {
                _tree.EndUpdate();
                _updating = false;
            }
        }

        /// <returns>true when at least one node was added (search)</returns>
        private bool AddNodes(TreeNodeCollection target, IEnumerable<RegionNode> regions, string filter)
        {
            bool any = false;
            foreach (RegionNode region in regions)
            {
                TreeNode node = CreateNode(region);
                bool childMatch = AddNodes(node.Nodes, region.Children, filter);
                bool match = filter.Length == 0 || Matches(region, filter);
                if (match || childMatch)
                {
                    target.Add(node);
                    any = true;
                }
            }

            return any;
        }

        private static bool Matches(RegionNode r, string filter)
        {
            return (r.Name ?? string.Empty).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   r.DisplayType.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   (r.Music ?? string.Empty).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private TreeNode CreateNode(RegionNode region)
        {
            int image = ColorImage(RegionMapView.RegionColor(region));
            var node = new TreeNode(region.DisplayName)
            {
                Tag = region,
                ImageIndex = image,
                SelectedImageIndex = image,
                ToolTipText = $"{region.DisplayType} · {Loc.T("prop.priority")} {region.Priority}" +
                              (string.IsNullOrEmpty(region.Music) ? string.Empty : $" · {region.Music}") +
                              $"\n{Loc.F("info.area", region.Rects.Count + region.Polygons.Count, region.Area)}" +
                              (string.IsNullOrWhiteSpace(region.Notes) ? string.Empty : "\n" + region.Notes)
            };

            if (region.Hidden)
            {
                node.ForeColor = SystemColors.GrayText;
                node.Text += "  " + Loc.T("tree.hidden");
            }

            if (region.Locked)
            {
                node.Text += "  🔒";
            }

            if (region.Rects.Count == 0 && region.Polygons.Count == 0)
            {
                node.NodeFont = new Font(_tree.Font, FontStyle.Italic);
            }

            return node;
        }

        private int ColorImage(Color color)
        {
            int key = color.ToArgb();
            if (_colorImages.TryGetValue(key, out int index))
            {
                return index;
            }

            var bmp = new Bitmap(12, 12);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, 1, 1, 10, 10);
                g.DrawRectangle(Pens.Black, 1, 1, 9, 9);
            }

            _tree.ImageList.Images.Add(bmp);
            index = _tree.ImageList.Images.Count - 1;
            _colorImages[key] = index;
            return index;
        }

        private static IEnumerable<TreeNode> AllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode n in nodes)
            {
                yield return n;
                foreach (TreeNode c in AllNodes(n.Nodes))
                {
                    yield return c;
                }
            }
        }

        private void OnSearchChanged(object sender, EventArgs e) => BuildTree();

        private void OnClickExpandAll(object sender, EventArgs e) => _tree.ExpandAll();

        private void OnClickCollapseAll(object sender, EventArgs e) => _tree.CollapseAll();

        #endregion

        #region Selection

        private void OnTreeAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_updating)
            {
                return;
            }

            SelectRegion(e.Node?.Tag as RegionNode, -1, false);
        }

        private void OnTreeNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is RegionNode region)
            {
                SelectRegion(region, -1, true);
            }
        }

        /// <summary>selects a region in all views; zoom = show it on the map</summary>
        private void SelectRegion(RegionNode region, int rectIndex, bool zoom)
        {
            _selected = region;
            _selectedRect = region == null ? -1 : Math.Min(rectIndex, region.Rects.Count - 1);
            _mapView.SelectedRegion = region;
            _mapView.SelectedRectIndex = _selectedRect;

            if (region != null && (_tree.SelectedNode == null || _tree.SelectedNode.Tag != region))
            {
                _updating = true;
                TreeNode node = AllNodes(_tree.Nodes).FirstOrDefault(n => n.Tag == region);
                if (node == null && _txtSearch.Text.Length > 0)
                {
                    _txtSearch.Text = string.Empty;
                    node = AllNodes(_tree.Nodes).FirstOrDefault(n => n.Tag == region);
                }

                _tree.SelectedNode = node;
                node?.EnsureVisible();
                _updating = false;
            }
            else if (region == null)
            {
                _updating = true;
                _tree.SelectedNode = null;
                _updating = false;
            }

            ShowProperties();
            FillRects();
            if (zoom && region != null && !region.Bounds.IsEmpty)
            {
                _mapView.ZoomTo(region.Bounds);
            }

            _mapView.Invalidate();
        }

        #endregion

        #region Add / delete / duplicate

        private string UniqueName(string baseName)
        {
            var names = new HashSet<string>(_project.AllRegions().Select(r => r.Name ?? string.Empty), StringComparer.OrdinalIgnoreCase);
            if (!names.Contains(baseName))
            {
                return baseName;
            }

            int n = 2;
            while (names.Contains($"{baseName} {n}"))
            {
                n++;
            }

            return $"{baseName} {n}";
        }

        private RegionNode NewRegion(RegionNode parent)
        {
            return new RegionNode
            {
                Name = UniqueName(parent == null ? Loc.T("new.region") : $"{parent.Name} – {Loc.T("new.part")}"),
                Type = parent == null ? "GuardedRegion" : string.Empty,
                Priority = parent?.Priority ?? RegionEditorSettings.Current.DefaultPriority,
                Music = parent?.Music
            };
        }

        private void OnClickAddRegion(object sender, EventArgs e) => AddRegion(null, null);

        private void OnClickAddChild(object sender, EventArgs e)
        {
            if (_selected == null)
            {
                SetMessage(Loc.T("msg.selectfirst"), true);
                return;
            }

            AddRegion(_selected, null);
        }

        /// <summary>adds a region; without area the rectangle tool is started</summary>
        private RegionNode AddRegion(RegionNode parent, Action<RegionNode> fill)
        {
            if (_mapView.MapId < 0)
            {
                return null;
            }

            RegionNode region = NewRegion(parent);
            Change(Loc.F("hist.add", region.Name), () =>
            {
                fill?.Invoke(region);
                if (parent == null)
                {
                    CurrentFacet(true).Regions.Add(region);
                }
                else
                {
                    parent.Children.Add(region);
                }

                _selected = region;
                _selectedRect = region.Rects.Count > 0 ? 0 : -1;
            });

            if (region.Rects.Count == 0 && region.Polygons.Count == 0)
            {
                SetTool(MapTool.Rect);
                SetMessage(Loc.T("msg.drawarea"));
            }

            return region;
        }

        private void OnClickDeleteRegion(object sender, EventArgs e) => DeleteRegion(_selected);

        private void DeleteRegion(RegionNode region)
        {
            if (region == null)
            {
                return;
            }

            if (region.Children.Count > 0 &&
                MessageBox.Show(this, Loc.F("ask.deletechildren", region.DisplayName, region.SelfAndDescendants().Count() - 1), Loc.T("tab.title"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            List<RegionNode> owner = _project.OwnerList(region);
            if (owner == null)
            {
                return;
            }

            int index = owner.IndexOf(region);
            _project.FindById(region.Id, out FacetRegions _, out RegionNode parent);
            Change(Loc.F("hist.delete", region.DisplayName), () =>
            {
                owner.Remove(region);
                _selected = owner.Count > 0 ? owner[Math.Min(index, owner.Count - 1)] : parent;
                _selectedRect = -1;
            });
        }

        private void OnClickDuplicate(object sender, EventArgs e)
        {
            if (_selected == null)
            {
                return;
            }

            List<RegionNode> owner = _project.OwnerList(_selected);
            RegionNode source = _selected;
            RegionNode copy = History.Deserialize(History.Serialize(new RegionProject
            {
                Facets = { new FacetRegions { Regions = { source } } }
            })).Facets[0].Regions[0];

            foreach (RegionNode r in copy.SelfAndDescendants())
            {
                r.Id = Guid.NewGuid().ToString("N");
            }

            copy.Name = UniqueName(source.Name);
            // a little bit to the side so the copy can be seen
            int offset = Math.Max(8, RegionEditorSettings.Current.Snap);
            foreach (RegionNode r in copy.SelfAndDescendants())
            {
                foreach (RegionRect rect in r.Rects)
                {
                    rect.X += offset;
                    rect.Y += offset;
                }

                foreach (RegionPolygon p in r.Polygons)
                {
                    p.Points = p.Points.Select(pt => new Point(pt.X + offset, pt.Y + offset)).ToList();
                }

                if (r.Go != null)
                {
                    r.Go.X += offset;
                    r.Go.Y += offset;
                }
            }

            Change(Loc.F("hist.duplicate", source.DisplayName), () =>
            {
                owner.Insert(owner.IndexOf(source) + 1, copy);
                _selected = copy;
            });
        }

        private void OnClickRename(object sender, EventArgs e) => _tree.SelectedNode?.BeginEdit();

        private void OnTreeAfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null || !(e.Node.Tag is RegionNode region))
            {
                return;
            }

            string name = e.Label.Trim();
            if (name.Length == 0 || name == region.Name)
            {
                e.CancelEdit = true;
                return;
            }

            e.CancelEdit = true; // the tree is built again
            BeginInvoke(new Action(() => Change(Loc.F("hist.rename", region.DisplayName, name), () => region.Name = name)));
        }

        private void OnTreeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteRegion(_selected);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Insert)
            {
                AddRegion(e.Shift ? _selected : null, null);
                e.Handled = true;
            }
        }

        #endregion

        #region Move (buttons, drag & drop)

        private void OnClickMoveUp(object sender, EventArgs e) => MoveSelected(-1);

        private void OnClickMoveDown(object sender, EventArgs e) => MoveSelected(1);

        private void MoveSelected(int dir)
        {
            if (_selected == null)
            {
                return;
            }

            List<RegionNode> owner = _project.OwnerList(_selected);
            int index = owner.IndexOf(_selected);
            int target = index + dir;
            if (target < 0 || target >= owner.Count)
            {
                return;
            }

            RegionNode region = _selected;
            Change(Loc.F("hist.move", region.DisplayName), () =>
            {
                owner.RemoveAt(index);
                owner.Insert(target, region);
            });
        }

        private void OnClickToTop(object sender, EventArgs e)
        {
            if (_selected == null)
            {
                return;
            }

            _project.FindById(_selected.Id, out FacetRegions facet, out RegionNode parent);
            if (parent == null)
            {
                return;
            }

            RegionNode region = _selected;
            Change(Loc.F("hist.move", region.DisplayName), () =>
            {
                parent.Children.Remove(region);
                facet.Regions.Insert(facet.Regions.IndexOf(TopOf(facet, parent)) + 1, region);
            });
        }

        private static RegionNode TopOf(FacetRegions facet, RegionNode region) =>
            facet.Regions.FirstOrDefault(t => t.SelfAndDescendants().Contains(region)) ?? region;

        private void OnTreeItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode node && e.Button == MouseButtons.Left)
            {
                DoDragDrop(node, DragDropEffects.Move);
            }
        }

        private void OnTreeDragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (!(e.Data.GetData(typeof(TreeNode)) is TreeNode dragged))
            {
                return;
            }

            TreeNode target = _tree.GetNodeAt(_tree.PointToClient(new Point(e.X, e.Y)));
            // not into itself or into one of its children
            if (target != null && (target == dragged || ((RegionNode)dragged.Tag).SelfAndDescendants().Contains((RegionNode)target.Tag)))
            {
                return;
            }

            e.Effect = DragDropEffects.Move;
        }

        /// <summary>drop on a region = becomes its child; Shift = insert before it on the same level; empty place = top level</summary>
        private void OnTreeDragDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(typeof(TreeNode)) is TreeNode dragged) || !(dragged.Tag is RegionNode region))
            {
                return;
            }

            TreeNode targetNode = _tree.GetNodeAt(_tree.PointToClient(new Point(e.X, e.Y)));
            RegionNode target = targetNode?.Tag as RegionNode;
            if (target == region || (target != null && region.SelfAndDescendants().Contains(target)))
            {
                return;
            }

            FacetRegions facet = CurrentFacet(true);
            bool before = (ModifierKeys & Keys.Shift) != 0;
            Change(Loc.F("hist.move", region.DisplayName), () =>
            {
                _project.OwnerList(region)?.Remove(region);
                if (target == null)
                {
                    facet.Regions.Add(region);
                }
                else if (before)
                {
                    List<RegionNode> owner = _project.OwnerList(target);
                    owner.Insert(owner.IndexOf(target), region);
                }
                else
                {
                    target.Children.Add(region);
                }

                _selected = region;
            });
        }

        #endregion

        #region Context menu of the tree

        private void OnTreeMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool sel = _selected != null;
            foreach (ToolStripItem item in new ToolStripItem[]
                     {
                         _ctxAddChild, _ctxDuplicate, _ctxRename, _ctxDelete, _ctxZoomTo, _ctxSendGo, _ctxCopyXml, _ctxOptimize, _ctxPolyToRects,
                         _ctxToTop, _ctxHide, _ctxLock
                     })
            {
                item.Enabled = sel;
            }

            if (sel)
            {
                _ctxHide.Checked = _selected.Hidden;
                _ctxLock.Checked = _selected.Locked;
                _ctxPolyToRects.Enabled = _selected.Polygons.Count > 0;
                _project.FindById(_selected.Id, out FacetRegions _, out RegionNode parent);
                _ctxToTop.Enabled = parent != null;
                _ctxSendGo.Enabled = _selected.Go != null || !_selected.Bounds.IsEmpty;
            }
        }

        private void OnClickToggleHidden(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            if (r != null)
            {
                Change(Loc.F("hist.props", r.DisplayName), () => r.Hidden = !r.Hidden);
            }
        }

        private void OnClickToggleLocked(object sender, EventArgs e)
        {
            RegionNode r = _selected;
            if (r != null)
            {
                Change(Loc.F("hist.props", r.DisplayName), () => r.Locked = !r.Locked);
            }
        }

        private void OnClickCopyXml(object sender, EventArgs e)
        {
            if (_selected == null)
            {
                return;
            }

            _project.FindById(_selected.Id, out FacetRegions _, out RegionNode parent);
            var options = new ServUoFormat.Options { WidthHeight = RegionEditorSettings.Current.WidthHeight };
            string xml = ServUoFormat.RegionElement(_selected, parent, options).ToString();
            Clipboard.SetText(xml);
            SetMessage(Loc.T("msg.copied"));
        }

        #endregion
    }
}
