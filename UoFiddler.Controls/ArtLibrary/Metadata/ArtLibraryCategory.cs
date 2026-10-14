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

namespace UoFiddler.Controls.ArtLibrary.Metadata
{
    /// <summary>
    /// A user defined category (folder). Categories can be nested through <see cref="ParentId"/>.
    /// </summary>
    public sealed class ArtLibraryCategory
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Parent category id, null for a top level category.</summary>
        public int? ParentId { get; set; }

        public override string ToString() => Name;
    }
}
