/***************************************************************************
 *
 * $Author: Turley
 * $Advanced: Nikodemus
 * 
 * "THE BEER-WINE-WARE LICENSE"
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
using UoFiddler.Controls.Classes;
using Ultima;

namespace UoFiddler.Controls.Forms
{
    public partial class ItemSearchForm : Form
    {
        private readonly Func<int, bool> _searchByIdCallback;
        private readonly Func<string, bool, bool> _searchByNameCallback;
        private Dictionary<string, List<int>> _nameToIdsMap = new Dictionary<string, List<int>>();
        private Dictionary<string, int> _currentIndexMap = new Dictionary<string, int>();
        private string _lastSearchedName;

        public ItemSearchForm(Func<int, bool> searchByIdCallback, Func<string, bool, bool> searchByNameCallback)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _searchByIdCallback = searchByIdCallback;
            _searchByNameCallback = searchByNameCallback;
        }

        #region Search_Graphic
        private void Search_Graphic(object sender, EventArgs e)
        {
            if (!Utils.ConvertStringToInt(textBoxGraphic.Text, out int graphic, 0, Ultima.Art.GetMaxItemId()))
            {
                return;
            }

            bool exist = _searchByIdCallback(graphic);
            if (exist)
            {
                AddToListBox(textBoxGraphic.Text, graphic);
                ShowGraphic(graphic);
                return;
            }

            ShowNoItemFoundDialog();
        }
        #endregion

        #region Search_ItemName
        private void Search_ItemName(object sender, EventArgs e)
        {
            _lastSearchedName = textBoxItemName.Text;

            bool exist = _searchByNameCallback(textBoxItemName.Text, false);
            if (exist)
            {
                try
                {
                    int id = GetNextIdByNamePartial(textBoxItemName.Text);
                    AddToListBox(textBoxItemName.Text, id);
                    ShowGraphic(id);
                }
                catch (ArgumentException)
                {
                    ShowNoItemFoundDialog();
                }
                return;
            }

            ShowNoItemFoundDialog();
        }
        #endregion

        #region GetNextIdByNamePartial
        private int GetNextIdByNamePartial(string partialName)
        {
            if (!_nameToIdsMap.ContainsKey(partialName))
            {
                _nameToIdsMap[partialName] = new List<int>();
                _currentIndexMap[partialName] = 0;
            }

            // Start search from the next index after the current last found index
            int startIndex = _currentIndexMap[partialName] + 1;

            // Find the next ID for the given partial name
            for (int id = startIndex; id <= Ultima.Art.GetMaxItemId(); id++)
            {
                if (TileData.ItemTable[id].Name.Contains(partialName, StringComparison.OrdinalIgnoreCase))
                {
                    _currentIndexMap[partialName] = id;
                    return id;
                }
            }

            throw new ArgumentException($"No more items found with name containing '{partialName}'");
        }
        #endregion

        #region SearchNextName
        private void SearchNextName(object sender, EventArgs e)
        {
            bool exist = _searchByNameCallback(textBoxItemName.Text, true);
            if (exist)
            {
                try
                {
                    AddToListBox(textBoxItemName.Text, GetNextIdByName(textBoxItemName.Text));
                }
                catch (ArgumentException)
                {
                    ShowNoItemFoundDialog();
                }
                return;
            }

            ShowNoItemFoundDialog();
        }
        #endregion

        #region OnKeyDownSearch
        private void OnKeyDownSearch(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if ((TextBox)sender == textBoxGraphic)
                {
                    Search_Graphic(sender, e);
                }
                else
                {
                    if (textBoxItemName.Text != _lastSearchedName)
                    {
                        Search_ItemName(sender, e);
                    }
                    else
                    {
                        SearchNextName(sender, e);
                    }
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
        #endregion

        #region ListBoxSearch_SelectedIndexChanged
        private void ListBoxSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = ListBoxSearch.SelectedItem.ToString();
            if (selectedItem.Contains("(") && selectedItem.Contains(")"))
            {
                string idStr = selectedItem.Split('(', ')')[1];
                if (int.TryParse(idStr, out int id))
                {
                    ShowGraphic(id);
                    //Search_Graphic(id);
                }
            }
        }
        #endregion

        #region Search_Graphic
        private void Search_Graphic(int graphic)
        {
            bool exist = _searchByIdCallback(graphic);
            if (exist)
            {
                AddToListBox(textBoxGraphic.Text, graphic);
                ShowGraphic(graphic);
                return;
            }

            ShowNoItemFoundDialog();
        }
        #endregion

        #region ShowGraphic
        private void ShowGraphic(int id)
        {
            Image graphic = Ultima.Art.GetStatic(id);
            pictureBoxGraphic.Image = graphic;
        }
        #endregion

        #region AddToListBox
        private void AddToListBox(string name, int id)
        {
            if (!_nameToIdsMap.ContainsKey(name))
            {
                _nameToIdsMap[name] = new List<int>();
                _currentIndexMap[name] = 0;
            }

            if (!_nameToIdsMap[name].Contains(id))
            {
                _nameToIdsMap[name].Add(id);
                ListBoxSearch.Items.Add($"{name} ({id})");
            }
        }
        #endregion

        #region GetNextIdByName
        private int GetNextIdByName(string name)
        {
            if (!_nameToIdsMap.ContainsKey(name))
            {
                _nameToIdsMap[name] = new List<int>();
                _currentIndexMap[name] = 0;
            }

            // Start search from the next index after the current last found index
            int startIndex = _currentIndexMap[name] + 1;

            // Find the next ID for the given name
            for (int id = startIndex; id <= Ultima.Art.GetMaxItemId(); id++)
            {
                if (TileData.ItemTable[id].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    _currentIndexMap[name] = id;
                    return id;
                }
            }

            throw new ArgumentException($"No more items found with name '{name}'");
        }
        #endregion

        #region ShowNoItemFoundDialog
        private void ShowNoItemFoundDialog()
        {
            DialogResult result = MessageBox.Show("No item found", "Result",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Cancel)
            {
                Close();
            }
        }
        #endregion

        #region clearToolStripMenuItem_Click
        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListBoxSearch.Items.Clear();
        }
        #endregion
    }
}
