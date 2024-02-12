/***************************************************************************
 *
 * $Author: Turley
 * Advanced: Nikodemus
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class MultisControl : UserControl
    {
        //private readonly string _multiXmlFileName = Path.Combine(Options.AppDataPath, "Multilist.xml");
        private readonly string _multiXmlFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Multilist.xml");
        private readonly XmlDocument _xmlDocument;
        //private readonly XmlElement _xmlElementMultis;
        private XmlElement _xmlElementMultis; // Readonly entfernt

        private int selectedId;
        private string selectedMultiName;
        private int selectedMultiType;

        public MultisControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;

            if (!File.Exists(_multiXmlFileName))
            {
                return;
            }

            _xmlDocument = new XmlDocument();
            _xmlDocument.Load(_multiXmlFileName);
            _xmlElementMultis = _xmlDocument["Multis"];

            // Initialize the selectedId variable.
            if (TreeViewMulti.SelectedNode != null)
            {
                selectedId = int.Parse(TreeViewMulti.SelectedNode.Name);
            }

            // Initialize the selectedMultiName variable.
            if (TreeViewMulti.SelectedNode != null)
            {
                selectedMultiName = TreeViewMulti.SelectedNode.Text;
            }

            // Event Handler für das KeyUp-Ereignis der ToolStripTextBoxSearch
            ToolStripTextBoxSearch.KeyUp += ToolStripTextBoxSearch_KeyUp;


        }

        private bool _loaded;
        private bool _showFreeSlots;
        private readonly MultisControl _refMarker;
        private Color _backgroundImageColor = Color.White;
        private bool _useTransparencyForPng = true;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["Multis"] = true;
            Options.LoadedUltimaClass["Hues"] = true;

            TreeViewMulti.BeginUpdate();
            try
            {
                TreeViewMulti.Nodes.Clear();
                var cache = new List<TreeNode>();
                for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
                {
                    MultiComponentList multi = Multis.GetComponents(i);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    TreeNode node;
                    if (_xmlDocument == null)
                    {
                        node = new TreeNode(string.Format("{0,5} (0x{0:X})", i));
                    }
                    else
                    {
                        XmlNodeList xMultiNodeList = _xmlElementMultis.SelectNodes("/Multis/Multi[@id='" + i + "']");
                        string j = "";

                        foreach (XmlNode xMultiNode in xMultiNodeList)
                        {
                            j = xMultiNode.Attributes["name"].Value;
                        }

                        node = new TreeNode($"{i,5} (0x{i:X}) {j}");
                        xMultiNodeList = _xmlElementMultis.SelectNodes("/Multis/ToolTip[@id='" + i + "']");
                        foreach (XmlNode xMultiNode in xMultiNodeList)
                        {
                            node.ToolTipText = j + "\r\n" + xMultiNode.Attributes["text"].Value;
                        }

                        if (xMultiNodeList.Count == 0)
                        {
                            node.ToolTipText = j;
                        }
                    }

                    node.Tag = multi;
                    node.Name = i.ToString();
                    cache.Add(node);
                }

                TreeViewMulti.Nodes.AddRange(cache.ToArray());
            }
            finally
            {
                TreeViewMulti.EndUpdate();
            }

            if (TreeViewMulti.Nodes.Count > 0)
            {
                TreeViewMulti.SelectedNode = TreeViewMulti.Nodes[0];
            }

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.MultiChangeEvent += OnMultiChangeEvent;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnMultiChangeEvent(object sender, int id)
        {
            if (!_loaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            MultiComponentList multi = Multis.GetComponents(id);
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            bool done = false;
            for (int i = 0; i < TreeViewMulti.Nodes.Count; ++i)
            {
                if (id == int.Parse(TreeViewMulti.Nodes[i].Name))
                {
                    TreeViewMulti.Nodes[i].Tag = multi;
                    TreeViewMulti.Nodes[i].ForeColor = Color.Black;
                    if (i == TreeViewMulti.SelectedNode.Index)
                    {
                        AfterSelect_Multi(this, null);
                    }

                    done = true;
                    break;
                }

                if (id >= int.Parse(TreeViewMulti.Nodes[i].Name))
                {
                    continue;
                }

                TreeNode node = new TreeNode(string.Format("{0,5} (0x{0:X})", id))
                {
                    Tag = multi,
                    Name = id.ToString()
                };
                TreeViewMulti.Nodes.Insert(i, node);
                done = true;
                break;
            }

            if (!done)
            {
                TreeNode node = new TreeNode(string.Format("{0,5} (0x{0:X})", id))
                {
                    Tag = multi,
                    Name = id.ToString()
                };
                TreeViewMulti.Nodes.Add(node);
            }
        }

        public void ChangeMulti(int id, MultiComponentList multi)
        {
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int index = _refMarker.TreeViewMulti.SelectedNode.Index;
            if (int.Parse(_refMarker.TreeViewMulti.SelectedNode.Name) != id)
            {
                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    if (int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name) != id)
                    {
                        continue;
                    }

                    index = i;
                    break;
                }
            }
            _refMarker.TreeViewMulti.Nodes[index].Tag = multi;
            _refMarker.TreeViewMulti.Nodes[index].ForeColor = Color.Black;
            if (index != _refMarker.TreeViewMulti.SelectedNode.Index)
            {
                _refMarker.TreeViewMulti.SelectedNode = _refMarker.TreeViewMulti.Nodes[index];
            }

            AfterSelect_Multi(this, null);
            ControlEvents.FireMultiChangeEvent(this, index);
        }

        private void AfterSelect_Multi(object sender, TreeViewEventArgs e)
        {
            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                HeightChangeMulti.Maximum = 0;
                toolTip.SetToolTip(HeightChangeMulti, "MaxHeight: 0");
                StatusMultiText.Text = "Size: 0,0 MaxHeight: 0 MultiRegion: 0,0,0,0";
            }
            else
            {
                HeightChangeMulti.Maximum = multi.MaxHeight;
                toolTip.SetToolTip(HeightChangeMulti,
                    $"MaxHeight: {HeightChangeMulti.Maximum - HeightChangeMulti.Value}");
                StatusMultiText.Text =
                    $"Size: {multi.Width},{multi.Height} MaxHeight: {multi.MaxHeight} MultiRegion: {multi.Min.X},{multi.Min.Y},{multi.Max.X},{multi.Max.Y} Surface: {multi.Surface}";
            }
            ChangeComponentList(multi);
            MultiPictureBox.Invalidate();
        }

        private void OnPaint_MultiPic(object sender, PaintEventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            if ((MultiComponentList)TreeViewMulti.SelectedNode.Tag == MultiComponentList.Empty)
            {
                e.Graphics.Clear(Color.White);
                return;
            }
            int h = HeightChangeMulti.Maximum - HeightChangeMulti.Value;
            Bitmap mMainPictureMulti = ((MultiComponentList)TreeViewMulti.SelectedNode.Tag).GetImage(h);
            if (mMainPictureMulti == null)
            {
                e.Graphics.Clear(Color.White);
                return;
            }
            Point location = Point.Empty;
            Size size = MultiPictureBox.Size;
            Rectangle destRect;
            if (mMainPictureMulti.Height < size.Height && mMainPictureMulti.Width < size.Width)
            {
                location.X = (MultiPictureBox.Width - mMainPictureMulti.Width) / 2;
                location.Y = (MultiPictureBox.Height - mMainPictureMulti.Height) / 2;
                destRect = new Rectangle(location, mMainPictureMulti.Size);
            }
            else if (mMainPictureMulti.Height < size.Height)
            {
                location.X = 0;
                location.Y = (MultiPictureBox.Height - mMainPictureMulti.Height) / 2;
                destRect = mMainPictureMulti.Width > size.Width
                    ? new Rectangle(location, new Size(size.Width, mMainPictureMulti.Height))
                    : new Rectangle(location, mMainPictureMulti.Size);
            }
            else if (mMainPictureMulti.Width < size.Width)
            {
                location.X = (MultiPictureBox.Width - mMainPictureMulti.Width) / 2;
                location.Y = 0;
                destRect = mMainPictureMulti.Height > size.Height
                    ? new Rectangle(location, new Size(mMainPictureMulti.Width, size.Height))
                    : new Rectangle(location, mMainPictureMulti.Size);
            }
            else
            {
                destRect = new Rectangle(new Point(0, 0), size);
            }

            e.Graphics.DrawImage(mMainPictureMulti, destRect, 0, 0, mMainPictureMulti.Width, mMainPictureMulti.Height, GraphicsUnit.Pixel);
        }

        private void OnValue_HeightChangeMulti(object sender, EventArgs e)
        {
            toolTip.SetToolTip(HeightChangeMulti, $"MaxHeight: {HeightChangeMulti.Maximum - HeightChangeMulti.Value}");
            MultiPictureBox.Invalidate();
        }

        private void ChangeComponentList(MultiComponentList multi)
        {
            MultiComponentBox.Clear();
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            bool isUohsa = Art.IsUOAHS();
            for (int x = 0; x < multi.Width; ++x)
            {
                for (int y = 0; y < multi.Height; ++y)
                {
                    foreach (var mTile in multi.Tiles[x][y])
                    {
                        MultiComponentBox.AppendText(
                            isUohsa
                                ? $"0x{mTile.Id:X4} {x,3} {y,3} {mTile.Z,2} {mTile.Flag,2} {mTile.Unk1,2}\n"
                                : $"0x{mTile.Id:X4} {x,3} {y,3} {mTile.Z,2} {mTile.Flag,2}\n");
                    }
                }
            }
        }
        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            ExtractMultiImage(ImageFormat.Bmp, _backgroundImageColor);
        }

        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            ExtractMultiImage(ImageFormat.Tiff, _backgroundImageColor);
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            ExtractMultiImage(ImageFormat.Jpeg, _backgroundImageColor);
        }

        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            ExtractMultiImage(ImageFormat.Png, _useTransparencyForPng ? Color.Transparent : _backgroundImageColor);
        }

        private void ExtractMultiImage(ImageFormat imageFormat, Color backgroundColor)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string floorSuffix = HeightChangeMulti.Value > 0
                ? $"_Z{HeightChangeMulti.Value:000}"
                : string.Empty;

            string fileName = Path.Combine(Options.OutputPath, $"Multi 0x{int.Parse(TreeViewMulti.SelectedNode.Name):X4}{floorSuffix}.{fileExtension}");

            int selectedMaxHeight = HeightChangeMulti.Maximum - HeightChangeMulti.Value;

            using (Bitmap multiBitmap = ((MultiComponentList)TreeViewMulti.SelectedNode.Tag)?.GetImage(selectedMaxHeight))
            {
                if (multiBitmap == null)
                {
                    return;
                }

                SaveImage(multiBitmap, fileName, imageFormat, backgroundColor);

                MessageBox.Show($"Multi saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private static void SaveImage(Image sourceImage, string fileName, ImageFormat imageFormat, Color backgroundColor)
        {
            using (Bitmap newBitmap = new Bitmap(sourceImage.Width, sourceImage.Height))
            using (Graphics newGraph = Graphics.FromImage(newBitmap))
            {
                newGraph.Clear(backgroundColor);
                newGraph.DrawImage(sourceImage, new Point(0, 0));
                newGraph.Save();

                newBitmap.Save(fileName, imageFormat);
            }
        }

        private void OnClickFreeSlots(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;
            TreeViewMulti.BeginUpdate();
            TreeViewMulti.Nodes.Clear();

            if (_showFreeSlots)
            {
                for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
                {
                    MultiComponentList multi = Multis.GetComponents(i);
                    TreeNode node;
                    if (_xmlDocument == null)
                    {
                        node = new TreeNode($"{i,5} (0x{i:X})");
                    }
                    else
                    {
                        XmlNodeList xMultiNodeList = _xmlElementMultis.SelectNodes("/Multis/Multi[@id='" + i + "']");
                        string j = "";
                        foreach (XmlNode xMultiNode in xMultiNodeList)
                        {
                            j = xMultiNode.Attributes["name"].Value;
                        }
                        node = new TreeNode(string.Format("{0,5} (0x{0:X}) {1}", i, j));
                    }
                    node.Name = i.ToString();
                    node.Tag = multi;
                    if (multi == MultiComponentList.Empty)
                    {
                        node.ForeColor = Color.Red;
                    }

                    TreeViewMulti.Nodes.Add(node);
                }
            }
            else
            {
                for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
                {
                    MultiComponentList multi = Multis.GetComponents(i);
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    TreeNode node;
                    if (_xmlDocument == null)
                    {
                        node = new TreeNode($"{i,5} (0x{i:X})");
                    }
                    else
                    {
                        XmlNodeList xMultiNodeList = _xmlElementMultis.SelectNodes("/Multis/Multi[@id='" + i + "']");
                        string j = "";
                        foreach (XmlNode xMultiNode in xMultiNodeList)
                        {
                            j = xMultiNode.Attributes["name"].Value;
                        }
                        node = new TreeNode(string.Format("{0,5} (0x{0:X}) {1}", i, j));
                    }
                    node.Tag = multi;
                    node.Name = i.ToString();
                    TreeViewMulti.Nodes.Add(node);
                }
            }
            TreeViewMulti.EndUpdate();
        }

        private void OnExportTextFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{id:X}.txt");
            multi.ExportToTextFile(fileName);
            MessageBox.Show($"Multi saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnExportWscFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{id:X}.wsc");
            multi.ExportToWscFile(fileName);
            MessageBox.Show($"Multi saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnExportUOAFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{id:X}.uoa");
            multi.ExportToUOAFile(fileName);
            MessageBox.Show($"Multi saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Multis.Save(Options.OutputPath);
            MessageBox.Show($"Saved to {Options.OutputPath}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Multis"] = false;
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);
            DialogResult result = MessageBox.Show(string.Format("Are you sure to remove {0} (0x{0:X})", id), "Remove",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Multis.Remove(id);
            TreeViewMulti.SelectedNode.Remove();
            Options.ChangedUltimaClass["Multis"] = true;
            ControlEvents.FireMultiChangeEvent(this, id);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            int id = int.Parse(TreeViewMulti.SelectedNode.Name);
            if (multi != MultiComponentList.Empty)
            {
                DialogResult result = MessageBox.Show(string.Format("Are you sure to replace {0} (0x{0:X})", id),
                    "Import", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            using (var dialog = new MultiImportForm(id, ChangeMulti) { TopMost = true })
            {
                dialog.ShowDialog();
            }
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            ExportAllMultis(ImageFormat.Bmp, _backgroundImageColor);
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            ExportAllMultis(ImageFormat.Tiff, _backgroundImageColor);
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            ExportAllMultis(ImageFormat.Jpeg, _backgroundImageColor);
        }

        private void OnClick_SaveAllPng(object sender, EventArgs e)
        {
            ExportAllMultis(ImageFormat.Png, _useTransparencyForPng ? Color.Transparent : _backgroundImageColor);
        }

        #region ExportAllMulti
        private void ExportAllMultis(ImageFormat imageFormat, Color backgroundColor)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; i++)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    const int maximumMultiHeight = 127;
                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X4}.{fileExtension}");

                    using (Bitmap multiBitmap = ((MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag)?.GetImage(maximumMultiHeight))
                    {
                        if (multiBitmap != null)
                        {
                            SaveImage(multiBitmap, fileName, imageFormat, backgroundColor);
                        }
                    }
                }

                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region  OnClick_SaveAllText
        private void OnClick_SaveAllText(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X4}.txt");
                    multi.ExportToTextFile(fileName);
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region OnClick_SaveAllUOA
        private void OnClick_SaveAllUOA(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X4}.uoa");
                    multi.ExportToUOAFile(fileName);
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region OnClick_SaveAllWSC
        private void OnClick_SaveAllWSC(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X4}.wsc");
                    multi.ExportToWscFile(fileName);
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region OnClick_SaveAllCSV
        private void OnClick_SaveAllCSV(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"{index:D4}.csv");
                    multi.ExportToCsvFile(fileName);
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region OnClick_SaveAllUox3
        private void OnClick_SaveAllUox3(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < _refMarker.TreeViewMulti.Nodes.Count; ++i)
                {
                    int index = int.Parse(_refMarker.TreeViewMulti.Nodes[i].Name);
                    if (index < 0)
                    {
                        continue;
                    }

                    MultiComponentList multi = (MultiComponentList)_refMarker.TreeViewMulti.Nodes[i].Tag;
                    if (multi == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Multi 0x{index:X4}.uox3");
                    multi.ExportToUox3File(fileName);
                }
                MessageBox.Show($"All Multis saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region OnExportCsvFile
        private void OnExportCsvFile(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"{id:D4}.csv");
            multi.ExportToCsvFile(fileName);
            MessageBox.Show($"Multi saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region OnExportUox3File
        private void OnExportUox3File(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            MultiComponentList multi = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
            if (multi == MultiComponentList.Empty)
            {
                return;
            }

            int id = int.Parse(TreeViewMulti.SelectedNode.Name);

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Multi 0x{id:X4}.uox3");
            multi.ExportToUox3File(fileName);
            MessageBox.Show($"Multi saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region ChangeBackgroundColorToolStripMenuItem
        private void ChangeBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _backgroundImageColor = colorDialog.Color;
            MultiPictureBox.BackColor = _backgroundImageColor;
        }
        #endregion

        #region UseTransparencyForPNGToolStripMenuItem
        private void UseTransparencyForPNGToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _useTransparencyForPng = UseTransparencyForPNGToolStripMenuItem.Checked;
        }
        #endregion  

        #region Edit

        /*private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Überprüfe, ob ein Knoten ausgewählt ist
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            // Hole die ausgewählte ID
            int selectedID = int.Parse(TreeViewMulti.SelectedNode.Name);

            // Öffne das Bearbeitungsfenster für das ausgewählte Multi
            EditMultiForm editForm = new EditMultiForm(selectedMultiName, selectedID, selectedMultiType);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                // Speichere die Änderungen in der Multilist.xml Datei
                string newName = editForm.MultiName;
                int newId = editForm.MultiID;
                int newType = editForm.SelectedMultiType;
                int idToFind = selectedID; // Verwende die ausgewählte ID als ID zum Suchen
                SaveChangesToXml(newName, newId, newType, idToFind);
            }
        }*/

        #region  editToolStripMenuItem
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check whether a node is selected
            if (TreeViewMulti.SelectedNode == null)
            {
                return;
            }

            // Get the selected ID
            int selectedID = int.Parse(TreeViewMulti.SelectedNode.Name);

            // Assigning a value to selectedMultiType
            selectedMultiType = 0; // Set the value to 0

            // Open the editing window for the selected Multi
            EditMultiForm editForm = new EditMultiForm(selectedMultiName, selectedID, selectedMultiType);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                // Save the changes in the Multilist.xml file
                string newName = editForm.MultiName;
                int newId = editForm.MultiID;
                int newType = editForm.SelectedMultiType;
                int idToFind = selectedID; // Use the selected ID as the ID to search
                SaveChangesToXml(newName, newId, newType, idToFind);
            }
        }
        #endregion

        #region SaveChangesToXml
        private void SaveChangesToXml(string newName, int newId, int newType, int idToFind)
        {
            // Load the XML file
            XmlDocument doc = new XmlDocument();
            doc.Load("Multilist.xml");

            // Find the multi-element with the selected ID
            XmlNode multiNode = null;
            XmlNodeList multiNodes = doc.SelectNodes("/Multis/Multi");
            foreach (XmlNode node in multiNodes)
            {
                if (node.Attributes["id"].Value == idToFind.ToString())
                {
                    multiNode = node;
                    break;
                }
            }

            if (multiNode != null)
            {
                // If the multi-element exists, update the name, id and type attribute
                multiNode.Attributes["name"].Value = newName;
                multiNode.Attributes["id"].Value = newId.ToString();
                multiNode.Attributes["type"].Value = newType.ToString();
            }
            else
            {
                // If the multi element does not exist, create it
                XmlNode multisNode = doc.SelectSingleNode("/Multis");
                XmlNode newNode = doc.CreateElement("Multi");

                XmlAttribute nameAttr = doc.CreateAttribute("name");
                nameAttr.Value = newName;
                newNode.Attributes.Append(nameAttr);

                XmlAttribute idAttr = doc.CreateAttribute("id");
                idAttr.Value = newId.ToString();
                newNode.Attributes.Append(idAttr);

                XmlAttribute typeAttr = doc.CreateAttribute("type");
                typeAttr.Value = newType.ToString();
                newNode.Attributes.Append(typeAttr);

                // Add the new multi-element in the correct place
                XmlNode previousNode = null;
                foreach (XmlNode node in multiNodes)
                {
                    if (int.Parse(node.Attributes["id"].Value) > newId)
                    {
                        break;
                    }

                    previousNode = node;
                }

                if (previousNode != null)
                {
                    multisNode.InsertAfter(newNode, previousNode);
                }
                else
                {
                    multisNode.PrependChild(newNode);
                }
            }

            // Save the changes to the XML file
            doc.Save("Multilist.xml");
        }
        #endregion

        #region AddNewEntryToXm
        private void AddNewEntryToXml(int id, string name, int type)
        {
            // Load the XML file
            XmlDocument doc = new XmlDocument();
            doc.Load("Multilist.xml");

            // Create a new multi-element
            XmlElement multiElement = doc.CreateElement("Multi");
            multiElement.SetAttribute("id", id.ToString());
            multiElement.SetAttribute("name", name);
            multiElement.SetAttribute("type", type.ToString());

            // Find the multis element
            XmlNode multisElement = doc.SelectSingleNode("/Multis");
            if (multisElement == null)
            {
                // If the Multis element doesn't exist, create it
                multisElement = doc.CreateElement("Multis");
                doc.AppendChild(multisElement);
            }

            // Insert the new multi-element in the correct place
            XmlNodeList multiNodes = multisElement.SelectNodes("/Multis/Multi");
            bool inserted = false;
            foreach (XmlNode multiNode in multiNodes)
            {
                int currentId = int.Parse(multiNode.Attributes["id"].Value);
                if (currentId > id)
                {
                    multisElement.InsertBefore(multiElement, multiNode);
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                // If the new multi element has not been inserted, add it at the end
                multisElement.AppendChild(multiElement);
            }

            // Save the changes to the XML file
            doc.Save("Multilist.xml");
        }
        #endregion

        #endregion

        #region copy Multilist.xml
        // Reload the Multilist.xml file.
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the Multilist.xml file exists.
            if (!File.Exists(_multiXmlFileName))
            {
                return;
            }

            // Reload the Multilist.xml file.
            _xmlDocument.Load(_multiXmlFileName);
            _xmlElementMultis = _xmlDocument["Multis"];

            // Update the display.
            Reload();
        }

        // Copy the Multilist.xml file to the OLDScript directory.
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the Multilist.xml file exists.
            if (!File.Exists(_multiXmlFileName))
            {
                return;
            }

            // Check if the OLDScript directory exists.
            string oldScriptDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OLDScript");
            if (!Directory.Exists(oldScriptDirectory))
            {
                // Create the OLDScript directory.
                Directory.CreateDirectory(oldScriptDirectory);
            }

            // Copy the Multilist.xml file to the OLDScript directory.
            string destinationFileName = Path.Combine(oldScriptDirectory, "Multilist.xml");
            File.Copy(_multiXmlFileName, destinationFileName, true);
        }
        #endregion

        #region Copy tabPage6 contextMenuStrip3
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(MultiComponentBox.Text))
            {
                Clipboard.SetText(MultiComponentBox.Text);
            }
        }

        private void TabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl3.SelectedTab == tabPage5)
            {
                MultiComponentBox.ContextMenuStrip = contextMenuStrip1;
            }
            else if (tabControl3.SelectedTab == tabPage6)
            {
                MultiComponentBox.ContextMenuStrip = contextMenuStrip3;
            }
        }
        #endregion

        #region Copy Image Clipboard

        private void copyclipboardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (TreeViewMulti.SelectedNode != null)
            {
                MultiComponentList selectedComponent = (MultiComponentList)TreeViewMulti.SelectedNode.Tag;
                if (selectedComponent != MultiComponentList.Empty)
                {
                    Bitmap image = selectedComponent.GetImage(HeightChangeMulti.Maximum - HeightChangeMulti.Value);
                    if (image != null)
                    {
                        try
                        {
                            Clipboard.SetImage(image);
                            MessageBox.Show("The image was successfully copied to the clipboard.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An error occurred while copying the image to the clipboard.: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            image.Dispose(); // Release the image to free up resources.
                        }
                        return;
                    }
                }
            }

            MessageBox.Show("There is no image available to copy to the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
        #endregion

        #region New Search KeyUp event of ToolStripTextBoxSearch.
        // Event handler for the KeyUp event of ToolStripTextBoxSearch.
        private void ToolStripTextBoxSearch_KeyUp(object sender, KeyEventArgs e)
        {
            // Retrieve text from the search and attempt to convert it into an ID.
            if (int.TryParse(ToolStripTextBoxSearch.Text, out int searchId))
            {
                // Search for the ID in the TreeViewMulti.
                TreeNode node = FindNodeById(TreeViewMulti.Nodes, searchId);
                if (node != null)
                {
                    // Select and make the found node visible in the TreeViewMulti.
                    TreeViewMulti.SelectedNode = node;
                    node.EnsureVisible();
                }
            }
        }
        private TreeNode FindNodeById(TreeNodeCollection nodes, int searchId)
        {
            foreach (TreeNode node in nodes)
            {
                if (int.TryParse(node.Name, out int nodeId) && nodeId == searchId)
                    return node;

                if (node.Nodes.Count > 0)
                {
                    TreeNode foundNode = FindNodeById(node.Nodes, searchId);
                    if (foundNode != null)
                        return foundNode;
                }
            }

            return null;
        }
        #endregion

        #region toolStripButtonMultiScript
        private UoFiddler.Controls.Forms.MultiScript multiScriptForm;
        private void toolStripButtonMultiScript_Click(object sender, EventArgs e)
        {
            // Überprüfen, ob die Form bereits geöffnet ist
            if (multiScriptForm == null || multiScriptForm.IsDisposed)
            {
                // Wenn nicht, erstellen Sie eine neue Instanz der Form und öffnen Sie sie
                multiScriptForm = new UoFiddler.Controls.Forms.MultiScript();
                multiScriptForm.Show();
            }
        }
        #endregion

        #region fillMultiScripterToolStripMenuItem
        private static MultiScript currentInstance = null;
        private void fillMultiScripterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Überprüfen Sie, ob bereits eine Instanz von MultiScript existiert
            if (currentInstance == null || currentInstance.IsDisposed)
            {
                // Erstellen Sie eine neue Instanz von MultiScript, wenn keine existiert
                currentInstance = new MultiScript();

                // Übertragen Sie die Komponenteninformationen von MultiComponentBox in tbIndexImage
                currentInstance.IndexImageText = GetComponentsFromMultiComponentBox();
            }

            // Bringen Sie die MultiScript-Form in den Vordergrund
            currentInstance.BringToFront();

            // Öffnen Sie die MultiScript-Form
            currentInstance.Show();
        }

        private string GetComponentsFromMultiComponentBox()
        {
            // Hier holen Sie die Komponenteninformationen aus MultiComponentBox
            // Dies ist nur ein Platzhalter und muss durch Ihren tatsächlichen Code ersetzt werden
            return MultiComponentBox.Text;
        }
        #endregion
    }
}
