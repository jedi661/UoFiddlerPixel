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
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Controls.TerrainLibrary.Core;
using UoFiddler.Controls.TerrainLibrary.Helpers;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.TerrainLibrary.Forms
{
    /// <summary>
    /// Opens the two terrain library windows (one instance each) and keeps them docked side by side:
    /// land tiles left, textures right. While docked, moving or resizing one window moves the other.
    /// </summary>
    public static class TerrainWindows
    {
        private static LandTileLibraryForm _land;
        private static TextureLibraryForm _texture;
        private static bool _arranging;

        /// <summary>Land Tiles tab that opened the window (for "Auswahl aus dem Tab holen").</summary>
        public static LandTilesControl LandSource { get; private set; }

        /// <summary>Textures tab that opened the window.</summary>
        public static TexturesControl TextureSource { get; private set; }

        public static bool IsLandOpen => _land != null && !_land.IsDisposed;

        public static bool IsTextureOpen => _texture != null && !_texture.IsDisposed;

        public static bool Docked => TerrainSession.Settings.Docked;

        /// <summary>Opens (or activates) the land tile library window. Call from the Land Tiles tab.</summary>
        public static LandTileLibraryForm ShowLandTiles(LandTilesControl source = null, int selectId = -1)
        {
            if (source != null)
            {
                LandSource = source;
            }

            if (!IsLandOpen)
            {
                _land = new LandTileLibraryForm();
                _land.Move += (s, e) => OnWindowMoved(TerrainLayer.LandTile);
                _land.Resize += (s, e) => OnWindowMoved(TerrainLayer.LandTile);
                _land.FormClosed += (s, e) => _land = null;
                if (IsTextureOpen && Docked)
                {
                    _land.StartPosition = FormStartPosition.Manual;
                }

                _land.Show();
                if (IsTextureOpen && Docked)
                {
                    Arrange(TerrainLayer.Texture);
                }
            }
            else
            {
                Activate(_land);
            }

            if (selectId >= 0)
            {
                _land.Panel.SelectSlotIfVisible(selectId);
            }

            return _land;
        }

        /// <summary>Opens (or activates) the texture library window. Call from the Textures tab.</summary>
        public static TextureLibraryForm ShowTextures(TexturesControl source = null, int selectId = -1)
        {
            if (source != null)
            {
                TextureSource = source;
            }

            if (!IsTextureOpen)
            {
                _texture = new TextureLibraryForm();
                _texture.Move += (s, e) => OnWindowMoved(TerrainLayer.Texture);
                _texture.Resize += (s, e) => OnWindowMoved(TerrainLayer.Texture);
                _texture.FormClosed += (s, e) => _texture = null;
                if (IsLandOpen && Docked)
                {
                    _texture.StartPosition = FormStartPosition.Manual;
                }

                _texture.Show();
                if (IsLandOpen && Docked)
                {
                    Arrange(TerrainLayer.LandTile);
                }
            }
            else
            {
                Activate(_texture);
            }

            if (selectId >= 0)
            {
                _texture.Panel.SelectSlotIfVisible(selectId);
            }

            return _texture;
        }

        /// <summary>Opens the window of the other layer next to this one.</summary>
        public static void ShowPartner(TerrainLayer layer, int selectId)
        {
            if (layer == TerrainLayer.LandTile)
            {
                ShowTextures(null, selectId);
            }
            else
            {
                ShowLandTiles(null, selectId);
            }
        }

        public static void SetDocked(bool docked)
        {
            TerrainSession.Settings.Docked = docked;
            TerrainSession.Settings.Save();
            if (docked && IsLandOpen && IsTextureOpen)
            {
                Arrange(TerrainLayer.LandTile);
            }

            if (IsLandOpen)
            {
                _land.Panel.UpdateDockButton();
            }

            if (IsTextureOpen)
            {
                _texture.Panel.UpdateDockButton();
            }
        }

        /// <summary>The other window of the given layer, or null.</summary>
        public static Form GetWindow(TerrainLayer layer)
        {
            return layer == TerrainLayer.LandTile ? (IsLandOpen ? _land : null) : (IsTextureOpen ? _texture : null);
        }

        /// <summary>True when the other window is still open (then the library stays open when one window closes).</summary>
        public static bool IsOtherOpen(TerrainLayer layer) => layer == TerrainLayer.LandTile ? IsTextureOpen : IsLandOpen;

        private static void Activate(Form form)
        {
            if (form.WindowState == FormWindowState.Minimized)
            {
                form.WindowState = FormWindowState.Normal;
            }

            form.Activate();
        }

        private static void OnWindowMoved(TerrainLayer moved)
        {
            if (!_arranging && Docked && IsLandOpen && IsTextureOpen)
            {
                Arrange(moved);
            }
        }

        /// <summary>Places the other window next to <paramref name="anchor"/> (land left, textures right).</summary>
        private static void Arrange(TerrainLayer anchor)
        {
            if (!IsLandOpen || !IsTextureOpen
                || _land.WindowState != FormWindowState.Normal || _texture.WindowState != FormWindowState.Normal)
            {
                return;
            }

            _arranging = true;
            try
            {
                if (anchor == TerrainLayer.LandTile)
                {
                    _texture.Bounds = new Rectangle(_land.Right, _land.Top, _texture.Width, _land.Height);
                }
                else
                {
                    _land.Bounds = new Rectangle(_texture.Left - _land.Width, _texture.Top, _land.Width, _texture.Height);
                }
            }
            finally
            {
                _arranging = false;
            }
        }
    }
}
