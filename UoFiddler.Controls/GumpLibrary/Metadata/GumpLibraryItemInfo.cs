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
using System.Text.Json.Serialization;

namespace UoFiddler.Controls.GumpLibrary.Metadata
{
    /// <summary>
    /// Everything the library knows about one gump slot besides the graphic.
    /// </summary>
    public sealed class GumpLibraryItemInfo
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>All categories the gump belongs to.</summary>
        public List<int> CategoryIds { get; set; } = new List<int>();

        /// <summary>Search terms.</summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>Gump id in gumpart.mul the graphic was taken from (or written to last).</summary>
        public int? SourceId { get; set; }

        /// <summary>Position for the sort mode "Eigene Reihenfolge" (smaller first, equal = by id).</summary>
        public int SortOrder { get; set; }

        public bool Favorite { get; set; }

        public System.DateTime Added { get; set; } = System.DateTime.Now;

        public System.DateTime Modified { get; set; } = System.DateTime.Now;

        [JsonIgnore]
        public bool IsEmpty =>
            string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Description) && CategoryIds.Count == 0 &&
            Tags.Count == 0 && SourceId == null && SortOrder == 0 && !Favorite;

        public GumpLibraryItemInfo Clone()
        {
            var clone = (GumpLibraryItemInfo)MemberwiseClone();
            clone.CategoryIds = new List<int>(CategoryIds);
            clone.Tags = new List<string>(Tags);
            return clone;
        }
    }
}
