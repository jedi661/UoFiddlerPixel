// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Common contract for all tile-slicing image handlers (IsoTiloSlicer).
    /// Allows the form to work with both handlers through one interface,
    /// eliminating the copy-paste duplication in the original code.
    /// </summary>
    internal interface ITiloSlicerHandler
    {
        /// <summary>Full path to the source image file.</summary>
        string ImagePath { get; set; }
        /// <summary>Width of each output tile in pixels.</summary>
        int TileWidth { get; set; }
        /// <summary>Height of each output tile in pixels.</summary>
        int TileHeight { get; set; }
        /// <summary>Pixel gap / border between tiles.</summary>
        int Offset { get; set; }
        /// <summary>Directory where sliced tiles are written.</summary>
        string OutputDirectory { get; set; }
        /// <summary>
        /// Composite-format string used for output file names.
        /// Example: "{0}" → "0.png", "1.png", …
        /// </summary>
        string FileNameFormat { get; set; }
        /// <summary>First numeric suffix for the output file name sequence.</summary>
        int StartingFileNumber { get; set; }
        /// <summary>
        /// Human-readable description of the last error, or <c>null</c> when
        /// the last <see cref="Process"/> call succeeded.
        /// </summary>
        string LastErrorMessage { get; }
        /// <summary>
        /// Slices the source image according to the current settings.
        /// </summary>
        /// <returns><c>true</c> on success; <c>false</c> if an error occurred
        /// (see <see cref="LastErrorMessage"/>).</returns>
        bool Process();
    }
}