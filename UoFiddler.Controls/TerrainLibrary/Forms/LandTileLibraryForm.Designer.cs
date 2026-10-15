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

namespace UoFiddler.Controls.TerrainLibrary.Forms
{
    partial class LandTileLibraryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            terrainLibraryPanel = new UoFiddler.Controls.TerrainLibrary.Forms.TerrainLibraryPanel();
            SuspendLayout();
            // 
            // terrainLibraryPanel
            // 
            terrainLibraryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            terrainLibraryPanel.Layer = UoFiddler.Controls.TerrainLibrary.Core.TerrainLayer.LandTile;
            terrainLibraryPanel.Name = "terrainLibraryPanel";
            terrainLibraryPanel.Size = new System.Drawing.Size(1000, 640);
            terrainLibraryPanel.TabIndex = 0;
            // 
            // LandTileLibraryForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1000, 640);
            Controls.Add(terrainLibraryPanel);
            MinimumSize = new System.Drawing.Size(700, 420);
            Name = "LandTileLibraryForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "LandTile-Bibliothek";
            FormClosing += LandTileLibraryForm_FormClosing;
            FormClosed += LandTileLibraryForm_FormClosed;
            Load += LandTileLibraryForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private UoFiddler.Controls.TerrainLibrary.Forms.TerrainLibraryPanel terrainLibraryPanel;
    }
}
