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

namespace UoFiddler.Controls.ArtLibrary.Core
{
    /// <summary>
    /// Type of graphics stored in a library. Only <see cref="ItemArt"/> is used by the art library;
    /// the other values are reserved for future libraries (land tiles, gumps, textures) that reuse
    /// the same file layout.
    /// </summary>
    public enum ArtLibraryKind
    {
        ItemArt = 0,
        LandArt = 1,
        Gump = 2,
        Texture = 3
    }
}
