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

using System.Collections.Generic;

namespace UoFiddler.Controls.GumpLibrary.Metadata
{
    /// <summary>
    /// A user defined category of the gump library (e.g. "Kleidung", "Rüstung/Helme").
    /// Categories can be nested through <see cref="ParentId"/>; a gump can be in any number of categories.
    /// </summary>
    public sealed class GumpLibraryCategory
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Parent category id, null for a top level category.</summary>
        public int? ParentId { get; set; }

        public string Description { get; set; }

        /// <summary>Position among the categories with the same parent (smaller first).</summary>
        public int SortOrder { get; set; }

        /// <summary>Optional marker color (ARGB) shown in the list; null = none.</summary>
        public int? Color { get; set; }

        public GumpLibraryCategory Clone() => (GumpLibraryCategory)MemberwiseClone();

        public override string ToString() => Name;
    }
}
