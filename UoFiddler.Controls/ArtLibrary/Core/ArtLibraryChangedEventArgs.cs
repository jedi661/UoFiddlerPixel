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

namespace UoFiddler.Controls.ArtLibrary.Core
{
    /// <summary>
    /// Raised by <see cref="ArtLibraryFile.EntryChanged"/>. <see cref="Id"/> is the changed graphic id,
    /// or <see cref="AllIds"/> when many entries changed at once (grow, compact).
    /// </summary>
    public sealed class ArtLibraryChangedEventArgs : EventArgs
    {
        public const int AllIds = -1;

        public ArtLibraryChangedEventArgs(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public bool IsAll => Id == AllIds;
    }
}
