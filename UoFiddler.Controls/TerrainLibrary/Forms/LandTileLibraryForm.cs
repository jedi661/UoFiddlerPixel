// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.TerrainLibrary.Forms
{
    /// <summary>
    /// Window of the land tile library. All logic is in <see cref="TerrainLibraryPanel"/> (shared with the
    /// other terrain window). Open it with TerrainWindows.ShowLandTiles.
    /// </summary>
    public partial class LandTileLibraryForm : Form
    {
        public LandTileLibraryForm()
        {
            InitializeComponent();

            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Icon = Options.GetFiddlerIcon();
        }

        /// <summary>The library view inside the window.</summary>
        public TerrainLibraryPanel Panel => terrainLibraryPanel;

        private void LandTileLibraryForm_Load(object sender, EventArgs e)
        {
            terrainLibraryPanel.Initialize();
        }

        private void LandTileLibraryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // the library is closed only with the last terrain window; ask then
            if (!TerrainWindows.IsOtherOpen(terrainLibraryPanel.Layer) && !terrainLibraryPanel.ConfirmUnsavedChanges())
            {
                e.Cancel = true;
            }
        }

        private void LandTileLibraryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            terrainLibraryPanel.Shutdown();
        }
    }
}
