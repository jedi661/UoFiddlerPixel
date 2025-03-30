/***************************************************************************
 *
 * $Author: Turley
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
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareLandControl : UserControl
    {
        // Create a list to store the selected IDs
        private List<int> selectedIds = new List<int>();

        public CompareLandControl()
        {
            InitializeComponent();
        }

        private readonly Dictionary<int, bool> _compare = new Dictionary<int, bool>();
        private readonly SHA256 _sha256 = SHA256.Create();
        private readonly ImageConverter _ic = new ImageConverter();

        private void OnLoad(object sender, EventArgs e)
        {
            listBoxOrg.BeginUpdate();
            listBoxOrg.Items.Clear();
            List<object> cache = new List<object>();
            for (int i = 0; i < 0x4000; i++)
            {
                cache.Add(i);
            }
            listBoxOrg.Items.AddRange(cache.ToArray());
            listBoxOrg.EndUpdate();
        }

        private void OnIndexChangedOrg(object sender, EventArgs e)
        {
            if (listBoxOrg.SelectedIndex == -1 || listBoxOrg.Items.Count < 1)
            {
                return;
            }

            int i = int.Parse(listBoxOrg.Items[listBoxOrg.SelectedIndex].ToString());
            if (listBoxSec.Items.Count > 0)
            {
                listBoxSec.SelectedIndex = listBoxSec.Items.IndexOf(i);
            }

            pictureBoxOrg.BackgroundImage = Art.IsValidLand(i)
                ? Art.GetLand(i)
                : null;

            listBoxOrg.Invalidate();
        }

        private void DrawitemOrg(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            Brush fontBrush = Brushes.Gray;

            int i = int.Parse(listBoxOrg.Items[e.Index].ToString());
            if (listBoxOrg.SelectedIndex == e.Index)
            {
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }

            if (!Art.IsValidLand(i))
            {
                fontBrush = Brushes.Red;
            }
            else if (listBoxSec.Items.Count > 0)
            {
                if (!Compare(i))
                {
                    fontBrush = Brushes.Blue;
                }
            }

            e.Graphics.DrawString($"0x{i:X} ({i})", Font, fontBrush,
                new PointF(5, e.Bounds.Y + ((e.Bounds.Height / 2) -
                (e.Graphics.MeasureString($"0x{i:X} ({i})", Font).Height / 2))));
        }

        private void MeasureOrg(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 13;
        }

        private void OnClickLoadSecond(object sender, EventArgs e)
        {
            if (textBoxSecondDir.Text == null)
            {
                return;
            }

            string path = textBoxSecondDir.Text;
            string file = Path.Combine(path, "art.mul");
            string file2 = Path.Combine(path, "artidx.mul");
            if (File.Exists(file) && File.Exists(file2))
            {
                SecondArt.SetFileIndex(file2, file);
                LoadSecond();
            }
        }

        private void LoadSecond()
        {
            _compare.Clear();
            listBoxSec.BeginUpdate();
            listBoxSec.Items.Clear();
            List<object> cache = new List<object>();
            for (int i = 0; i < 0x4000; i++)
            {
                cache.Add(i);
            }
            listBoxSec.Items.AddRange(cache.ToArray());
            listBoxSec.EndUpdate();
        }

        #region DrawItemSec
        private void DrawItemSec(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            Brush fontBrush = Brushes.Gray;
            Brush selectionBrush = Brushes.LightSteelBlue; // Color for the first selection
            Brush secondSelectionBrush = Brushes.Yellow; // Color for the second selection (CTRL)

            int i = int.Parse(listBoxOrg.Items[e.Index].ToString());

            if (listBoxSec.SelectedIndices.Contains(e.Index))
            {
                if (ModifierKeys.HasFlag(Keys.Control)) // When the CTRL key is pressed
                {
                    e.Graphics.FillRectangle(secondSelectionBrush, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }
                else
                {
                    e.Graphics.FillRectangle(selectionBrush, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }
            }

            // Check whether the entry is in the list of selected IDs
            if (selectedIds.Contains(i))
            {
                e.Graphics.FillRectangle(Brushes.Yellow, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }

            if (!SecondArt.IsValidLand(i))
            {
                fontBrush = Brushes.Red;
            }
            else if (!Compare(i))
            {
                fontBrush = Brushes.Blue;
            }

            e.Graphics.DrawString($"0x{i:X} ({i})", Font, fontBrush,
                new PointF(5,
                e.Bounds.Y + ((e.Bounds.Height / 2) -
                (e.Graphics.MeasureString($"0x{i:X} ({i})", Font).Height / 2))));

        }
        #endregion

        private void MeasureSec(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 13;
        }

        #region OnIndexChangeSec
        private void OnIndexChangedSec(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1 || listBoxSec.Items.Count < 1)
            {
                return;
            }

            // Add the selected ID to the list when the CTRL key is pressed
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                int selectedId = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
                if (!selectedIds.Contains(selectedId))
                {
                    selectedIds.Add(selectedId);
                }
            }

            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            listBoxOrg.SelectedIndex = listBoxOrg.Items.IndexOf(i);
            pictureBoxSec.BackgroundImage = SecondArt.IsValidLand(i) ? SecondArt.GetLand(i) : null;

            listBoxSec.Invalidate();
        }
        #endregion
        private bool Compare(int index)
        {
            if (_compare.ContainsKey(index))
            {
                return _compare[index];
            }

            Bitmap bitorg = Art.GetLand(index);
            Bitmap bitsec = SecondArt.GetLand(index);
            if (bitorg == null && bitsec == null)
            {
                _compare[index] = true;
                return true;
            }
            if (bitorg == null || bitsec == null
                               || bitorg.Size != bitsec.Size)
            {
                _compare[index] = false;
                return false;
            }

            byte[] btImage1 = new byte[1];
            btImage1 = (byte[])_ic.ConvertTo(bitorg, btImage1.GetType());
            byte[] btImage2 = new byte[1];
            btImage2 = (byte[])_ic.ConvertTo(bitsec, btImage2.GetType());

            string hash1String = BitConverter.ToString(_sha256.ComputeHash(btImage1));
            string hash2String = BitConverter.ToString(_sha256.ComputeHash(btImage2));

            bool res = hash1String == hash2String;
            _compare[index] = res;

            return res;
        }

        private void OnChangeShowDiff(object sender, EventArgs e)
        {
            if (_compare.Count < 1)
            {
                if (!checkBox1.Checked)
                {
                    return;
                }

                MessageBox.Show("Second Land file is not loaded!");
                checkBox1.Checked = false;
                return;
            }

            listBoxOrg.BeginUpdate();
            listBoxSec.BeginUpdate();
            listBoxOrg.Items.Clear();
            listBoxSec.Items.Clear();
            List<object> cache = new List<object>();
            if (checkBox1.Checked)
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    if (!Compare(i))
                    {
                        cache.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    cache.Add(i);
                }
            }
            listBoxOrg.Items.AddRange(cache.ToArray());
            listBoxSec.Items.AddRange(cache.ToArray());
            listBoxOrg.EndUpdate();
            listBoxSec.EndUpdate();
        }

        private void ExportAsBmp(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
            {
                return;
            }

            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondArt.IsValidLand(i))
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Landtile(Sec) 0x{i:X}.bmp");
            SecondArt.GetLand(i).Save(fileName, ImageFormat.Bmp);
            MessageBox.Show(
                $"Landtile saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void ExportAsTiff(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
            {
                return;
            }

            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondArt.IsValidLand(i))
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Landtile(Sec) 0x{i:X}.tiff");
            SecondArt.GetLand(i).Save(fileName, ImageFormat.Tiff);
            MessageBox.Show(
                $"Landtile saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void BrowseOnClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the art files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxSecondDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1)
            {
                return;
            }

            int i = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            if (!SecondArt.IsValidLand(i))
            {
                return;
            }

            Bitmap copy = new Bitmap(SecondArt.GetLand(i));
            Art.ReplaceLand(i, copy);
            Options.ChangedUltimaClass["Art"] = true;
            ControlEvents.FireLandTileChangeEvent(this, i);
            _compare[i] = true;
            listBoxOrg.BeginUpdate();
            bool done = false;
            for (int id = 0; id < 0x4000; id++)
            {
                if (id > i)
                {
                    listBoxOrg.Items.Insert(id, i);
                    done = true;
                    break;
                }
                if (id == i)
                {
                    done = true;
                    break;
                }
            }
            if (!done)
            {
                listBoxOrg.Items.Add(i);
            }

            listBoxOrg.EndUpdate();
            listBoxOrg.Invalidate();
            listBoxSec.Invalidate();
            OnIndexChangedOrg(this, null);
        }
        #region Buttons and Left and Right
        private void btmoveItemtoId_Click(object sender, EventArgs e)
        {
            if (listBoxSec.SelectedIndex == -1 || listBoxOrg.SelectedIndex == -1)
            {
                return;
            }

            int sourceId = int.Parse(listBoxSec.Items[listBoxSec.SelectedIndex].ToString());
            int targetId = int.Parse(listBoxOrg.Items[listBoxOrg.SelectedIndex].ToString());

            if (!SecondArt.IsValidLand(sourceId))
            {
                return;
            }

            Bitmap sourceImage = SecondArt.GetLand(sourceId);
            Art.ReplaceLand(targetId, new Bitmap(sourceImage));
            Options.ChangedUltimaClass["Art"] = true;
            ControlEvents.FireLandTileChangeEvent(this, targetId);

            pictureBoxOrg.BackgroundImage = Art.GetLand(targetId);
        }

        private void btRemoveImageId_Click(object sender, EventArgs e)
        {
            if (listBoxOrg.SelectedIndex == -1)
            {
                return;
            }

            int targetId = int.Parse(listBoxOrg.Items[listBoxOrg.SelectedIndex].ToString());

            Art.RemoveLand(targetId);
            Options.ChangedUltimaClass["Art"] = true;
            ControlEvents.FireLandTileChangeEvent(this, targetId);

            pictureBoxOrg.BackgroundImage = null;
        }

        private void btmultipleImageID_Click(object sender, EventArgs e)
        {
            if (selectedIds.Count == 0)
            {
                return;
            }

            // Move the selected entries from listBoxSec to listBoxOrg
            foreach (int sourceId in selectedIds)
            {
                if (!SecondArt.IsValidLand(sourceId))
                {
                    continue;
                }

                Bitmap sourceImage = SecondArt.GetLand(sourceId);
                Art.ReplaceLand(sourceId, new Bitmap(sourceImage));
                Options.ChangedUltimaClass["Art"] = true;
                ControlEvents.FireLandTileChangeEvent(this, sourceId);
            }

            // Update listBoxOrg and pictureBoxOrg
            listBoxOrg.Invalidate();
            if (listBoxOrg.SelectedIndex != -1)
            {
                int selectedId = int.Parse(listBoxOrg.Items[listBoxOrg.SelectedIndex].ToString());
                pictureBoxOrg.BackgroundImage = Art.GetLand(selectedId);
            }

            selectedIds.Clear();
        }

        private void ListBoxSec_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                btmoveItemtoId_Click(sender, e);
                e.Handled = true; // Prevents the default behavior of the left arrow key
            }
            if (e.KeyCode == Keys.Right)
            {
                btRemoveImageId_Click(sender, e);
                e.Handled = true; // Prevents the default behavior of the right arrow key
            }
        }
        #endregion

        #region Search Hex
        private void tbSearchHex_TextChanged(object sender, EventArgs e)
        {
            string hexInput = tbSearchHex.Text;

            // Check that the input is a valid hexadecimal number
            if (!int.TryParse(hexInput, System.Globalization.NumberStyles.HexNumber, null, out int id))
            {
                return;
            }

            // Find the entry in listBoxOrg
            int index = listBoxOrg.Items.IndexOf(id);
            if (index != -1)
            {
                // Select the entry when it is found
                listBoxOrg.SelectedIndex = index;
            }
        }
        #endregion

        #region MouseDown
        private void listBoxSec_MouseDown(object sender, MouseEventArgs e)
        {
            int index = this.listBoxSec.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    // Add the selected ID to the list when the CTRL key is pressed
                    int selectedId = int.Parse(listBoxSec.Items[index].ToString());
                    if (!selectedIds.Contains(selectedId))
                    {
                        selectedIds.Add(selectedId);
                    }
                }
            }
        }
        #endregion
    }
}
