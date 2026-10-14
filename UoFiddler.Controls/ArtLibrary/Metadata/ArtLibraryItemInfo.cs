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
using System.Collections.Generic;

namespace UoFiddler.Controls.ArtLibrary.Metadata
{
    /// <summary>
    /// Metadata of one graphic in the library (stored in Name.artlib.json).
    /// </summary>
    public sealed class ArtLibraryItemInfo
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>Category id, null = no category.</summary>
        public int? CategoryId { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>Id in the original art.mul the graphic was taken from, if known.</summary>
        public int? SourceId { get; set; }

        public DateTime Added { get; set; } = DateTime.Now;

        /// <summary>True when nothing but the date is set (such entries are not saved).</summary>
        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(Name)
            && string.IsNullOrWhiteSpace(Description)
            && CategoryId == null
            && (Tags == null || Tags.Count == 0)
            && SourceId == null;

        public ArtLibraryItemInfo Clone()
        {
            return new ArtLibraryItemInfo
            {
                Name = Name,
                Description = Description,
                CategoryId = CategoryId,
                Tags = Tags == null ? new List<string>() : new List<string>(Tags),
                SourceId = SourceId,
                Added = Added
            };
        }
    }
}
